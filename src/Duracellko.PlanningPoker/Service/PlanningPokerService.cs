﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Service providing operations for planning poker web clients.
    /// </summary>
    [Route("api/PlanningPokerService")]
    [Controller]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class PlanningPokerService : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerService"/> class.
        /// </summary>
        /// <param name="planningPoker">The planning poker controller.</param>
        public PlanningPokerService(D.IPlanningPoker planningPoker)
        {
            PlanningPoker = planningPoker ?? throw new ArgumentNullException(nameof(planningPoker));
        }

        /// <summary>
        /// Gets the planning poker controller.
        /// </summary>
        public D.IPlanningPoker PlanningPoker { get; private set; }

        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        [HttpGet("CreateTeam")]
        public ActionResult<ScrumTeam> CreateTeam(string teamName, string scrumMasterName)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(scrumMasterName, nameof(scrumMasterName));

            try
            {
                using (D.IScrumTeamLock teamLock = PlanningPoker.CreateScrumTeam(teamName, scrumMasterName))
                {
                    teamLock.Lock();
                    return ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Connects member or observer with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member or observer.</param>
        /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
        /// <returns>
        /// The Scrum team the member or observer joined to.
        /// </returns>
        [HttpGet("JoinTeam")]
        public ActionResult<ScrumTeam> JoinTeam(string teamName, string memberName, bool asObserver)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            try
            {
                using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
                {
                    teamLock.Lock();
                    D.ScrumTeam team = teamLock.Team;
                    team.Join(memberName, asObserver);
                    return ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reconnects member with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>
        /// The Scrum team the member or observer reconnected to.
        /// </returns>
        /// <remarks>
        /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
        /// </remarks>
        [HttpGet("ReconnectTeam")]
        public ActionResult<ReconnectTeamResult> ReconnectTeam(string teamName, string memberName)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            try
            {
                using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
                {
                    teamLock.Lock();
                    D.ScrumTeam team = teamLock.Team;
                    D.Observer observer = team.FindMemberOrObserver(memberName);
                    if (observer == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Error_MemberNotFound, memberName), nameof(memberName));
                    }

                    Estimate selectedEstimate = null;
                    if (team.State == D.TeamState.EstimateInProgress)
                    {
                        D.Member member = observer as D.Member;
                        if (member != null)
                        {
                            selectedEstimate = ServiceEntityMapper.Map<D.Estimate, Estimate>(member.Estimate);
                        }
                    }

                    long lastMessageId = observer.ClearMessages();
                    observer.UpdateActivity();

                    ScrumTeam teamResult = ServiceEntityMapper.Map<D.ScrumTeam, ScrumTeam>(teamLock.Team);
                    return new ReconnectTeamResult()
                    {
                        ScrumTeam = teamResult,
                        LastMessageId = lastMessageId,
                        SelectedEstimate = selectedEstimate
                    };
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        [HttpGet("DisconnectTeam")]
        public void DisconnectTeam(string teamName, string memberName)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                D.ScrumTeam team = teamLock.Team;
                team.Disconnect(memberName);
            }
        }

        /// <summary>
        /// Signal from Scrum master to starts the estimate.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        [HttpGet("StartEstimate")]
        public void StartEstimate(string teamName)
        {
            ValidateTeamName(teamName);

            using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                D.ScrumTeam team = teamLock.Team;
                team.ScrumMaster.StartEstimate();
            }
        }

        /// <summary>
        /// Signal from Scrum master to cancels the estimate.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        [HttpGet("CancelEstimate")]
        public void CancelEstimate(string teamName)
        {
            ValidateTeamName(teamName);

            using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                D.ScrumTeam team = teamLock.Team;
                team.ScrumMaster.CancelEstimate();
            }
        }

        /// <summary>
        /// Submits the estimate for specified team member.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimate">The estimate the member is submitting.</param>
        [HttpGet("SubmitEstimate")]
        public void SubmitEstimate(string teamName, string memberName, double estimate)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            double? domainEstimate;
            if (estimate == -1111111.0)
            {
                domainEstimate = null;
            }
            else if (estimate == Estimate.PositiveInfinity)
            {
                domainEstimate = double.PositiveInfinity;
            }
            else
            {
                domainEstimate = estimate;
            }

            using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                D.ScrumTeam team = teamLock.Team;
                D.Member member = team.FindMemberOrObserver(memberName) as D.Member;
                if (member != null)
                {
                    member.Estimate = new D.Estimate(domainEstimate);
                }
            }
        }

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> object representing asynchronous operation.
        /// </returns>
        [HttpGet("GetMessages")]
        public async Task<IList<Message>> GetMessages(string teamName, string memberName, long lastMessageId)
        {
            ValidateTeamName(teamName);
            ValidateMemberName(memberName, nameof(memberName));

            GetMessagesTask getMessagesTask = new GetMessagesTask(PlanningPoker)
            {
                TeamName = teamName,
                MemberName = memberName,
                LastMessageId = lastMessageId,
            };
            getMessagesTask.Start();

            return await getMessagesTask.ProcessMessagesTask;
        }

        private static void ValidateTeamName(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException(nameof(teamName));
            }

            if (teamName.Length > 50)
            {
                throw new ArgumentException(Resources.Error_TeamNameTooLong, nameof(teamName));
            }
        }

        private static void ValidateMemberName(string memberName, string paramName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(paramName);
            }

            if (memberName.Length > 50)
            {
                throw new ArgumentException(Resources.Error_TeamNameTooLong, paramName);
            }
        }

        /// <summary>
        /// Asynchronous task of receiving messages by a team member.
        /// </summary>
        private class GetMessagesTask
        {
            private TaskCompletionSource<List<Message>> _taskCompletionSource;

            /// <summary>
            /// Initializes a new instance of the <see cref="GetMessagesTask"/> class.
            /// </summary>
            /// <param name="planningPoker">The planning poker controller.</param>
            public GetMessagesTask(D.IPlanningPoker planningPoker)
            {
                PlanningPoker = planningPoker;
            }

            /// <summary>
            /// Gets the planning poker controller.
            /// </summary>
            /// <value>
            /// The planning poker controller.
            /// </value>
            public D.IPlanningPoker PlanningPoker { get; private set; }

            /// <summary>
            /// Gets or sets the name of the Scrum team.
            /// </summary>
            /// <value>
            /// The name of the Scrum team.
            /// </value>
            public string TeamName { get; set; }

            /// <summary>
            /// Gets or sets the name of the member to receive message of.
            /// </summary>
            /// <value>
            /// The name of the team member.
            /// </value>
            public string MemberName { get; set; }

            /// <summary>
            /// Gets or sets ID of last message received by the team member.
            /// </summary>
            /// <value>
            /// The last message ID.
            /// </value>
            public long LastMessageId { get; set; }

            /// <summary>
            /// Gets the asynchronous operation of receiving messages for the team member.
            /// </summary>
            public Task<List<Message>> ProcessMessagesTask
            {
                get
                {
                    return _taskCompletionSource != null ? _taskCompletionSource.Task : null;
                }
            }

            /// <summary>
            /// Starts the asynchronous operation of receiving new messages.
            /// </summary>
            public void Start()
            {
                _taskCompletionSource = new TaskCompletionSource<List<Message>>();
                SetProcessMessagesHandler();
            }

            private void ReturnResult(List<Message> result)
            {
                _taskCompletionSource.SetResult(result);
            }

            private void ThrowException(Exception ex)
            {
                _taskCompletionSource.SetException(ex);
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions must be set as asynchronous task result.")]
            private void ProcessMessages(bool hasMessages, D.Observer member)
            {
                try
                {
                    List<Message> result;
                    if (hasMessages)
                    {
                        result = member.Messages.Select(m => ServiceEntityMapper.Map<D.Message, Message>(m)).ToList();
                    }
                    else
                    {
                        result = new List<Message>();
                    }

                    ReturnResult(result);
                }
                catch (Exception ex)
                {
                    ThrowException(ex);
                }
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions must be set as asynchronous task result.")]
            private void SetProcessMessagesHandler()
            {
                try
                {
                    using (D.IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(TeamName))
                    {
                        teamLock.Lock();
                        D.ScrumTeam team = teamLock.Team;
                        D.Observer member = team.FindMemberOrObserver(MemberName);

                        // Removes old messages, which the member has already read, from the member's message queue.
                        while (member.HasMessage && member.Messages.First().Id <= LastMessageId)
                        {
                            member.PopMessage();
                        }

                        // Updates last activity on member to record time, when member checked for new messages.
                        // also notifies to save the team into repository
                        member.UpdateActivity();

                        PlanningPoker.GetMessagesAsync(member, ProcessMessages);
                    }
                }
                catch (Exception ex)
                {
                    ThrowException(ex);
                }
            }
        }
    }
}
