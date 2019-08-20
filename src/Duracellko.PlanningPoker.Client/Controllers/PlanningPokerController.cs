using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Manages state of planning poker game and provides data for view.
    /// </summary>
    public class PlanningPokerController : IPlanningPokerInitializer, INotifyPropertyChanged
    {
        private const string ScrumMasterType = "ScrumMaster";
        private const string ObserverType = "Observer";

        private readonly IPlanningPokerClient _planningPokerService;
        private readonly IBusyIndicatorService _busyIndicator;
        private readonly IMemberCredentialsStore _memberCredentialsStore;
        private List<MemberEstimate> _memberEstimates;
        private bool _isConnected;
        private bool _hasJoinedEstimate;
        private Estimate _selectedEstimate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerController" /> class.
        /// </summary>
        /// <param name="planningPokerService">Planning poker client to send messages to server.</param>
        /// <param name="busyIndicator">Service to show busy indicator, when operation is in progress.</param>
        /// <param name="memberCredentialsStore">Service to save and load member credentials.</param>
        public PlanningPokerController(
            IPlanningPokerClient planningPokerService,
            IBusyIndicatorService busyIndicator,
            IMemberCredentialsStore memberCredentialsStore)
        {
            _planningPokerService = planningPokerService ?? throw new ArgumentNullException(nameof(planningPokerService));
            _busyIndicator = busyIndicator ?? throw new ArgumentNullException(nameof(busyIndicator));
            _memberCredentialsStore = memberCredentialsStore ?? throw new ArgumentNullException(nameof(memberCredentialsStore));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets current user joining planning poker.
        /// </summary>
        public TeamMember User { get; private set; }

        /// <summary>
        /// Gets Scrum Team data received from server.
        /// </summary>
        public ScrumTeam ScrumTeam { get; private set; }

        /// <summary>
        /// Gets Scrum Team name.
        /// </summary>
        public string TeamName => ScrumTeam?.Name;

        /// <summary>
        /// Gets a value indicating whether current user is connected to Planning Poker game.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsConnected)));
                }
            }
        }

        /// <summary>
        /// Gets ID of last received message.
        /// </summary>
        public long LastMessageId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether current user is Scrum Master and can start or stop estimate.
        /// </summary>
        public bool IsScrumMaster { get; private set; }

        /// <summary>
        /// Gets name of Scrum Master.
        /// </summary>
        public string ScrumMaster => ScrumTeam?.ScrumMaster?.Name;

        /// <summary>
        /// Gets collection of member names, who can estimate.
        /// </summary>
        public IEnumerable<string> Members => ScrumTeam.Members
            .Where(m => m.Type != ScrumMasterType).Select(m => m.Name)
            .OrderBy(m => m, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets collection of observer names, who just observe estimate.
        /// </summary>
        public IEnumerable<string> Observers => ScrumTeam.Observers.Select(m => m.Name)
            .OrderBy(m => m, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether user can estimate and estimate is in progress.
        /// </summary>
        public bool CanSelectEstimate => ScrumTeam.State == TeamState.EstimateInProgress &&
            User.Type != ObserverType && _hasJoinedEstimate && _selectedEstimate == null;

        /// <summary>
        /// Gets a value indicating whether user can start estimate.
        /// </summary>
        public bool CanStartEstimate => IsScrumMaster && ScrumTeam.State != TeamState.EstimateInProgress;

        /// <summary>
        /// Gets a value indicating whether user can cancel estimate.
        /// </summary>
        public bool CanCancelEstimate => IsScrumMaster && ScrumTeam.State == TeamState.EstimateInProgress;

        /// <summary>
        /// Gets a collection of available estimate values, user can select from.
        /// </summary>
        public IEnumerable<double?> AvailableEstimates => ScrumTeam.AvailableEstimates.Select(e => e.Value)
            .OrderBy(v => v, EstimateComparer.Default);

        /// <summary>
        /// Gets a collection of selected estimates by all users.
        /// </summary>
        public IEnumerable<MemberEstimate> Estimates => _memberEstimates;

        /// <summary>
        /// Initialize <see cref="PlanningPokerController"/> object with Scrum Team data received from server.
        /// </summary>
        /// <param name="scrumTeam">Scrum Team data received from server.</param>
        /// <param name="username">Name of user joining the Scrum Team.</param>
        /// <returns>Asynchronous operation.</returns>
        public Task InitializeTeam(ScrumTeam scrumTeam, string username)
        {
            if (scrumTeam == null)
            {
                throw new ArgumentNullException(nameof(scrumTeam));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (scrumTeam.Members == null)
            {
                scrumTeam.Members = new List<TeamMember>();
            }

            if (scrumTeam.Observers == null)
            {
                scrumTeam.Observers = new List<TeamMember>();
            }

            ScrumTeam = scrumTeam;
            User = FindTeamMember(username);
            IsScrumMaster = User != null && User == ScrumTeam.ScrumMaster;
            LastMessageId = -1;

            if (scrumTeam.EstimateResult != null)
            {
                _memberEstimates = GetMemberEstimateList(scrumTeam.EstimateResult);
            }
            else if (scrumTeam.EstimateParticipants != null)
            {
                _memberEstimates = scrumTeam.EstimateParticipants
                    .Where(p => p.Estimated).Select(p => new MemberEstimate(p.MemberName)).ToList();
            }
            else
            {
                _memberEstimates = null;
            }

            IsConnected = true;
            _hasJoinedEstimate = scrumTeam.EstimateParticipants != null &&
                scrumTeam.EstimateParticipants.Any(p => string.Equals(p.MemberName, User?.Name, StringComparison.OrdinalIgnoreCase));
            _selectedEstimate = null;

            MemberCredentials memberCredentials = new MemberCredentials
            {
                TeamName = TeamName,
                MemberName = User.Name
            };
            return _memberCredentialsStore.SetCredentialsAsync(memberCredentials);
        }

        /// <summary>
        /// Initialize <see cref="PlanningPokerController"/> object with Scrum Team data received from server.
        /// </summary>
        /// <param name="teamInfo">Scrum Team data received from server.</param>
        /// <param name="username">Name of user joining the Scrum Team.</param>
        /// <returns>Asynchronous operation.</returns>
        /// <remarks>This method overloads setup additional information after reconnecting to existing team.</remarks>
        public async Task InitializeTeam(ReconnectTeamResult teamInfo, string username)
        {
            if (teamInfo == null)
            {
                throw new ArgumentNullException(nameof(teamInfo));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            await InitializeTeam(teamInfo.ScrumTeam, username);

            LastMessageId = teamInfo.LastMessageId;
            _selectedEstimate = teamInfo.SelectedEstimate;
        }

        /// <summary>
        /// Disconnect current user from current Scrum Team.
        /// </summary>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task Disconnect()
        {
            using (_busyIndicator.Show())
            {
                await _planningPokerService.DisconnectTeam(TeamName, User.Name, CancellationToken.None);
                IsConnected = false;
                await _memberCredentialsStore.SetCredentialsAsync(null);
            }
        }

        /// <summary>
        /// Disconnects member from existing Planning Poker game. This functionality can be used by ScrumMaster only.
        /// </summary>
        /// <param name="member">Name of member to disconnect.</param>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task DisconnectMember(string member)
        {
            if (string.IsNullOrEmpty(member))
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (string.Equals(member, User.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("ScrumMaster cannot disconnect himself.", nameof(member));
            }

            using (_busyIndicator.Show())
            {
                await _planningPokerService.DisconnectTeam(TeamName, member, CancellationToken.None);
            }
        }

        /// <summary>
        /// Selects estimate by user, when estimate is in progress.
        /// </summary>
        /// <param name="estimate">Selected estimate value.</param>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task SelectEstimate(double? estimate)
        {
            if (CanSelectEstimate)
            {
                using (_busyIndicator.Show())
                {
                    Estimate selectedEstimate = ScrumTeam.AvailableEstimates.First(e => e.Value == estimate);
                    await _planningPokerService.SubmitEstimate(TeamName, User.Name, estimate, CancellationToken.None);
                    _selectedEstimate = selectedEstimate;
                }
            }
        }

        /// <summary>
        /// Starts estimate by Scrum Master.
        /// </summary>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task StartEstimate()
        {
            if (CanStartEstimate)
            {
                using (_busyIndicator.Show())
                {
                    await _planningPokerService.StartEstimate(TeamName, CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Stops estimate by Scrum Master.
        /// </summary>
        /// <returns><see cref="Task"/> representing asynchronous operation.</returns>
        public async Task CancelEstimate()
        {
            if (CanCancelEstimate)
            {
                using (_busyIndicator.Show())
                {
                    await _planningPokerService.CancelEstimate(TeamName, CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Processes messages received from server and updates status of planning poker game.
        /// </summary>
        /// <param name="messages">Collection of messages received from server.</param>
        public void ProcessMessages(IEnumerable<Message> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            foreach (Message message in messages.OrderBy(m => m.Id))
            {
                ProcessMessage(message);
            }
        }

        /// <summary>
        /// Notifies that a property of this instance has been changed.
        /// </summary>
        /// <param name="e">Arguments of PropertyChanged event.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private static List<MemberEstimate> GetMemberEstimateList(IList<EstimateResultItem> estimationResult)
        {
            Dictionary<double, int> estimationValueCounts = new Dictionary<double, int>();
            foreach (EstimateResultItem estimate in estimationResult)
            {
                if (estimate.Estimate != null)
                {
                    double key = GetEstimateValueKey(estimate.Estimate.Value);
                    if (estimationValueCounts.TryGetValue(key, out int count))
                    {
                        estimationValueCounts[key] = count + 1;
                    }
                    else
                    {
                        estimationValueCounts.Add(key, 1);
                    }
                }
            }

            return estimationResult
                .OrderByDescending(i => i.Estimate != null ? estimationValueCounts[GetEstimateValueKey(i.Estimate.Value)] : 0)
                .ThenBy(i => i.Estimate?.Value, EstimateComparer.Default)
                .ThenBy(i => i.Member.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(i => i.Estimate != null ? new MemberEstimate(i.Member.Name, i.Estimate.Value) : new MemberEstimate(i.Member.Name))
                .ToList();
        }

        private static double GetEstimateValueKey(double? value)
        {
            if (!value.HasValue)
            {
                return -1111111.0;
            }
            else if (double.IsPositiveInfinity(value.Value))
            {
                return -1111100.0;
            }
            else
            {
                return value.Value;
            }
        }

        private void ProcessMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.MemberJoined:
                    OnMemberJoined((MemberMessage)message);
                    break;
                case MessageType.MemberDisconnected:
                    OnMemberDisconnected((MemberMessage)message);
                    break;
                case MessageType.EstimateStarted:
                    OnEstimateStarted();
                    break;
                case MessageType.EstimateEnded:
                    OnEstimateEnded((EstimateResultMessage)message);
                    break;
                case MessageType.EstimateCanceled:
                    OnEstimateCanceled();
                    break;
                case MessageType.MemberEstimated:
                    OnMemberEstimated((MemberMessage)message);
                    break;
            }

            LastMessageId = message.Id;
            OnPropertyChanged(new PropertyChangedEventArgs(null));
        }

        private void OnMemberJoined(MemberMessage message)
        {
            TeamMember member = message.Member;
            if (member.Type == ObserverType)
            {
                ScrumTeam.Observers.Add(member);
            }
            else
            {
                ScrumTeam.Members.Add(member);
            }
        }

        private void OnMemberDisconnected(MemberMessage message)
        {
            string name = message.Member.Name;
            if (ScrumTeam.ScrumMaster != null &&
                string.Equals(ScrumTeam.ScrumMaster.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                ScrumTeam.ScrumMaster = null;
            }
            else
            {
                TeamMember member = ScrumTeam.Members.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                {
                    ScrumTeam.Members.Remove(member);
                }
                else
                {
                    member = ScrumTeam.Observers.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (member != null)
                    {
                        ScrumTeam.Observers.Remove(member);
                    }
                }
            }
        }

        private void OnEstimateStarted()
        {
            _memberEstimates = new List<MemberEstimate>();
            ScrumTeam.State = TeamState.EstimateInProgress;
            _hasJoinedEstimate = true;
            _selectedEstimate = null;
        }

        private void OnEstimateEnded(EstimateResultMessage message)
        {
            _memberEstimates = GetMemberEstimateList(message.EstimateResult);
            ScrumTeam.State = TeamState.EstimateFinished;
        }

        private void OnEstimateCanceled()
        {
            ScrumTeam.State = TeamState.EstimateCanceled;
        }

        private void OnMemberEstimated(MemberMessage message)
        {
            if (_memberEstimates != null && !string.IsNullOrEmpty(message.Member?.Name))
            {
                _memberEstimates.Add(new MemberEstimate(message.Member.Name));
            }
        }

        private TeamMember FindTeamMember(string name)
        {
            if (ScrumTeam.ScrumMaster != null &&
                string.Equals(ScrumTeam.ScrumMaster.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return ScrumTeam.ScrumMaster;
            }

            TeamMember result = ScrumTeam.Members.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (result != null)
            {
                return result;
            }

            result = ScrumTeam.Observers.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private class EstimateComparer : IComparer<double?>
        {
            public static EstimateComparer Default { get; } = new EstimateComparer();

            public int Compare(double? x, double? y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                else if (x == null)
                {
                    return 1;
                }
                else if (y == null)
                {
                    return -1;
                }
                else
                {
                    return Comparer<double>.Default.Compare(x.Value, y.Value);
                }
            }
        }
    }
}
