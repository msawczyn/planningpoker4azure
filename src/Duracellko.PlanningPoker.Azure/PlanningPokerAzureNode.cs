﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Instance of Planning Poker application in Windows Azure. Synchronizes the planning poker teams with other nodes.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Destructor is placed together with Dispose.")]
    public class PlanningPokerAzureNode : IDisposable
    {
        private readonly InitializationList _teamsToInitialize = new InitializationList();
        private readonly ILogger<PlanningPokerAzureNode> _logger;

        private IDisposable _sendNodeMessageSubscription;
        private IDisposable _serviceBusScrumTeamMessageSubscription;
        private IDisposable _serviceBusTeamCreatedMessageSubscription;
        private IDisposable _serviceBusRequestTeamListMessageSubscription;
        private IDisposable _serviceBusRequestTeamsMessageSubscription;

        private volatile string _processingScrumTeamName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerAzureNode"/> class.
        /// </summary>
        /// <param name="planningPoker">The planning poker teams controller instance.</param>
        /// <param name="serviceBus">The service bus used to send messages between nodes.</param>
        /// <param name="configuration">The configuration of planning poker for Azure platform.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public PlanningPokerAzureNode(IAzurePlanningPoker planningPoker, IServiceBus serviceBus, IAzurePlanningPokerConfiguration configuration, ILogger<PlanningPokerAzureNode> logger)
        {
            PlanningPoker = planningPoker ?? throw new ArgumentNullException(nameof(planningPoker));
            ServiceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
            Configuration = configuration ?? new AzurePlanningPokerConfiguration();
            _logger = logger;
            NodeId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets a controller of planning poker teams.
        /// </summary>
        /// <value>The planning poker controller.</value>
        public IAzurePlanningPoker PlanningPoker { get; private set; }

        /// <summary>
        /// Gets an ID of the Planning Poker node.
        /// </summary>
        public string NodeId { get; private set; }

        /// <summary>
        /// Gets a configuration of planning poker for Azure platform.
        /// </summary>
        public IAzurePlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets an Azure service bus object used to send messages between nodes.
        /// </summary>
        protected IServiceBus ServiceBus { get; private set; }

        /// <summary>
        /// Starts synchronization with other nodes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Start()
        {
            _logger?.LogInformation(Resources.Info_PlanningPokerAzureNodeStarting, NodeId);

            await ServiceBus.Register(NodeId);
            SetupPlanningPokerListeners();
            SetupServiceBusListeners();

            RequestTeamList();
        }

        /// <summary>
        /// Stops synchronization with other nodes.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Stop()
        {
            _logger?.LogInformation(Resources.Info_PlanningPokerAzureNodeStopping, NodeId);

            if (_sendNodeMessageSubscription != null)
            {
                _sendNodeMessageSubscription.Dispose();
                _sendNodeMessageSubscription = null;
            }

            if (_serviceBusScrumTeamMessageSubscription != null)
            {
                _serviceBusScrumTeamMessageSubscription.Dispose();
                _serviceBusScrumTeamMessageSubscription = null;
            }

            if (_serviceBusTeamCreatedMessageSubscription != null)
            {
                _serviceBusTeamCreatedMessageSubscription.Dispose();
                _serviceBusTeamCreatedMessageSubscription = null;
            }

            if (_serviceBusRequestTeamListMessageSubscription != null)
            {
                _serviceBusRequestTeamListMessageSubscription.Dispose();
                _serviceBusRequestTeamListMessageSubscription = null;
            }

            if (_serviceBusRequestTeamsMessageSubscription != null)
            {
                _serviceBusRequestTeamsMessageSubscription.Dispose();
                _serviceBusRequestTeamsMessageSubscription = null;
            }

            return ServiceBus.Unregister();
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> if disposing not using GC; otherwise <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop().Wait();
            }
        }

        ~PlanningPokerAzureNode()
        {
            Dispose(false);
        }

        private void SetupPlanningPokerListeners()
        {
            IObservable<ScrumTeamMessage> teamMessages = PlanningPoker.ObservableMessages.Where(m => !string.Equals(m.TeamName, _processingScrumTeamName, StringComparison.OrdinalIgnoreCase));
            IObservable<NodeMessage> nodeTeamMessages = teamMessages
                .Where(m => m.MessageType != MessageType.Empty && m.MessageType != MessageType.TeamCreated && m.MessageType != MessageType.EstimateEnded)
                .Select(m => new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = m });
            IObservable<NodeMessage> createTeamMessages = teamMessages.Where(m => m.MessageType == MessageType.TeamCreated)
                .Select(m => CreateTeamCreatedMessage(m.TeamName));
            IObservable<NodeMessage> nodeMessages = nodeTeamMessages.Merge(createTeamMessages);

            _sendNodeMessageSubscription = nodeMessages.Subscribe(SendNodeMessage);
        }

        private void SetupServiceBusListeners()
        {
            IObservable<NodeMessage> serviceBusMessages = ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, NodeId, StringComparison.OrdinalIgnoreCase));

            IObservable<NodeMessage> busTeamMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.ScrumTeamMessage);
            _serviceBusScrumTeamMessageSubscription = busTeamMessages.Subscribe(ProcessTeamMessage);

            IObservable<NodeMessage> busTeamCreatedMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.TeamCreated);
            _serviceBusTeamCreatedMessageSubscription = busTeamCreatedMessages.Subscribe(OnScrumTeamCreated);
        }

        private NodeMessage CreateTeamCreatedMessage(string teamName)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                return new NodeMessage(NodeMessageType.TeamCreated)
                {
                    Data = ScrumTeamHelper.SerializeScrumTeam(team)
                };
            }
        }

        private async void SendNodeMessage(NodeMessage message)
        {
            message.SenderNodeId = NodeId;
            await ServiceBus.SendMessage(message);

            _logger?.LogInformation(Resources.Info_NodeMessageSent, NodeId, message.SenderNodeId, message.RecipientNodeId, message.MessageType);
        }

        private void OnScrumTeamCreated(NodeMessage message)
        {
            byte[] scrumTeamData = (byte[])message.Data;
            ScrumTeam scrumTeam = ScrumTeamHelper.DeserializeScrumTeam(scrumTeamData, PlanningPoker.DateTimeProvider);
            _logger?.LogInformation(Resources.Info_ScrumTeamCreatedNodeMessageReceived, NodeId, message.SenderNodeId, message.RecipientNodeId, message.MessageType, scrumTeam.Name);

            if (!_teamsToInitialize.ContainsOrNotInit(scrumTeam.Name))
            {
                try
                {
                    _processingScrumTeamName = scrumTeam.Name;
                    using (IScrumTeamLock teamLock = PlanningPoker.AttachScrumTeam(scrumTeam))
                    {
                    }
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void ProcessTeamMessage(NodeMessage nodeMessage)
        {
            ScrumTeamMessage message = (ScrumTeamMessage)nodeMessage.Data;
            _logger?.LogInformation(Resources.Info_ScrumTeamNodeMessageReceived, NodeId, nodeMessage.SenderNodeId, nodeMessage.RecipientNodeId, nodeMessage.MessageType, message.TeamName, message.MessageType);

            if (!_teamsToInitialize.ContainsOrNotInit(message.TeamName))
            {
                switch (message.MessageType)
                {
                    case MessageType.MemberJoined:
                        OnMemberJoinedMessage(message.TeamName, (ScrumTeamMemberMessage)message);
                        break;
                    case MessageType.MemberDisconnected:
                        OnMemberDisconnectedMessage(message.TeamName, (ScrumTeamMemberMessage)message);
                        break;
                    case MessageType.EstimateStarted:
                        OnEstimateStartedMessage(message.TeamName);
                        break;
                    case MessageType.EstimateCanceled:
                        OnEstimateCanceledMessage(message.TeamName);
                        break;
                    case MessageType.MemberEstimated:
                        OnMemberEstimatedMessage(message.TeamName, (ScrumTeamMemberEstimateMessage)message);
                        break;
                    case MessageType.MemberActivity:
                        OnMemberActivityMessage(message.TeamName, (ScrumTeamMemberMessage)message);
                        break;
                }
            }
        }

        private void OnMemberJoinedMessage(string teamName, ScrumTeamMemberMessage message)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                try
                {
                    _processingScrumTeamName = team.Name;
                    team.Join(message.MemberName, string.Equals(message.MemberType, typeof(Observer).Name, StringComparison.OrdinalIgnoreCase));
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void OnMemberDisconnectedMessage(string teamName, ScrumTeamMemberMessage message)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                try
                {
                    _processingScrumTeamName = team.Name;
                    team.Disconnect(message.MemberName);
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void OnEstimateStartedMessage(string teamName)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                try
                {
                    _processingScrumTeamName = team.Name;
                    team.ScrumMaster.StartEstimate();
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void OnEstimateCanceledMessage(string teamName)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                try
                {
                    _processingScrumTeamName = team.Name;
                    team.ScrumMaster.CancelEstimate();
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void OnMemberEstimatedMessage(string teamName, ScrumTeamMemberEstimateMessage message)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                try
                {
                    _processingScrumTeamName = team.Name;
                    Member member = team.FindMemberOrObserver(message.MemberName) as Member;
                    if (member != null)
                    {
                        member.Estimate = new Estimate(message.Estimate);
                    }
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void OnMemberActivityMessage(string teamName, ScrumTeamMemberMessage message)
        {
            using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(teamName))
            {
                teamLock.Lock();
                ScrumTeam team = teamLock.Team;
                try
                {
                    _processingScrumTeamName = team.Name;
                    Observer observer = team.FindMemberOrObserver(message.MemberName);
                    if (observer != null)
                    {
                        observer.UpdateActivity();
                    }
                }
                finally
                {
                    _processingScrumTeamName = null;
                }
            }
        }

        private void RequestTeamList()
        {
            if (!_teamsToInitialize.IsEmpty)
            {
                IObservable<NodeMessage> serviceBusMessages = ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, NodeId, StringComparison.OrdinalIgnoreCase));
                IObservable<Action> teamListActions = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.TeamList).Take(1)
                    .Timeout(Configuration.InitializationMessageTimeout, Observable.Return<NodeMessage>(null))
                    .Select(m => new Action(() => ProcessTeamListMessage(m)));
                teamListActions.Subscribe(a => a());

                SendNodeMessage(new NodeMessage(NodeMessageType.RequestTeamList));
            }
            else
            {
                EndInitialization();
            }
        }

        private void ProcessTeamListMessage(NodeMessage message)
        {
            if (message != null)
            {
                _logger?.LogInformation(Resources.Info_NodeMessageReceived, NodeId, message.SenderNodeId, message.RecipientNodeId, message.MessageType);

                IEnumerable<string> teamList = (IEnumerable<string>)message.Data;
                if (_teamsToInitialize.Setup(teamList))
                {
                    PlanningPoker.SetTeamsInitializingList(teamList);
                }

                RequestTeams(message.SenderNodeId);
            }
            else
            {
                EndInitialization();
            }
        }

        private void RequestTeams(string recipientId)
        {
            if (_teamsToInitialize.IsEmpty)
            {
                EndInitialization();
            }
            else
            {
                object lockObject = new object();
                IObservable<NodeMessage> serviceBusMessages = ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, NodeId, StringComparison.OrdinalIgnoreCase));
                serviceBusMessages = serviceBusMessages.Synchronize(lockObject);

                DateTime lastMessageTime = PlanningPoker.DateTimeProvider.UtcNow;

                IObservable<Action> initTeamActions = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.InitializeTeam)
                    .TakeWhile(m => !_teamsToInitialize.IsEmpty)
                    .Select(m => new Action(() =>
                    {
                        lastMessageTime = PlanningPoker.DateTimeProvider.UtcNow;
                        ProcessInitializeTeamMessage(m);
                    }));
                IObservable<Action> messageTimeoutActions = Observable.Interval(TimeSpan.FromSeconds(1.0)).Synchronize(lockObject)
                    .SelectMany(i => lastMessageTime + Configuration.InitializationMessageTimeout < PlanningPoker.DateTimeProvider.UtcNow ? Observable.Throw<Action>(new TimeoutException()) : Observable.Empty<Action>());

                void RetryRequestTeamList(Exception ex)
                {
                    _logger?.LogWarning(Resources.Warning_RetryRequestTeamList, NodeId);
                    RequestTeamList();
                }

                initTeamActions.Merge(messageTimeoutActions)
                    .Subscribe(a => a(), RetryRequestTeamList);

                NodeMessage requestTeamsMessage = new NodeMessage(NodeMessageType.RequestTeams)
                {
                    RecipientNodeId = recipientId,
                    Data = _teamsToInitialize.Values.ToArray()
                };
                SendNodeMessage(requestTeamsMessage);
            }
        }

        private void ProcessInitializeTeamMessage(NodeMessage message)
        {
            byte[] scrumTeamData = message.Data as byte[];
            if (scrumTeamData != null)
            {
                ScrumTeam scrumTeam = ScrumTeamHelper.DeserializeScrumTeam(scrumTeamData, PlanningPoker.DateTimeProvider);
                _logger?.LogInformation(Resources.Info_ScrumTeamCreatedNodeMessageReceived, NodeId, message.SenderNodeId, message.RecipientNodeId, message.MessageType, scrumTeam.Name);

                _teamsToInitialize.Remove(scrumTeam.Name);
                PlanningPoker.InitializeScrumTeam(scrumTeam);
            }
            else
            {
                // team does not exist anymore
                _teamsToInitialize.Remove((string)message.Data);
            }

            if (_teamsToInitialize.IsEmpty)
            {
                EndInitialization();
            }
        }

        private void EndInitialization()
        {
            _teamsToInitialize.Clear();
            PlanningPoker.EndInitialization();
            _logger?.LogInformation(Resources.Info_PlanningPokerAzureNodeInitialized, NodeId);

            IObservable<NodeMessage> serviceBusMessages = ServiceBus.ObservableMessages.Where(m => !string.Equals(m.SenderNodeId, NodeId, StringComparison.OrdinalIgnoreCase));

            IObservable<NodeMessage> requestTeamListMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.RequestTeamList);
            _serviceBusRequestTeamListMessageSubscription = requestTeamListMessages.Subscribe(ProcessRequestTeamListMesage);

            IObservable<NodeMessage> requestTeamsMessages = serviceBusMessages.Where(m => m.MessageType == NodeMessageType.RequestTeams);
            _serviceBusRequestTeamsMessageSubscription = requestTeamsMessages.Subscribe(ProcessRequestTeamsMessage);
        }

        private void ProcessRequestTeamListMesage(NodeMessage message)
        {
            _logger?.LogInformation(Resources.Info_NodeMessageReceived, NodeId, message.SenderNodeId, message.RecipientNodeId, message.MessageType);

            string[] scrumTeamNames = PlanningPoker.ScrumTeamNames.ToArray();
            NodeMessage teamListMessage = new NodeMessage(NodeMessageType.TeamList)
            {
                RecipientNodeId = message.SenderNodeId,
                Data = scrumTeamNames
            };
            SendNodeMessage(teamListMessage);
        }

        private void ProcessRequestTeamsMessage(NodeMessage message)
        {
            _logger?.LogInformation(Resources.Info_NodeMessageReceived, NodeId, message.SenderNodeId, message.RecipientNodeId, message.MessageType);

            IEnumerable<string> scrumTeamNames = (IEnumerable<string>)message.Data;
            foreach (string scrumTeamName in scrumTeamNames)
            {
                try
                {
                    byte[] scrumTeamData = null;
                    try
                    {
                        using (IScrumTeamLock teamLock = PlanningPoker.GetScrumTeam(scrumTeamName))
                        {
                            teamLock.Lock();
                            scrumTeamData = ScrumTeamHelper.SerializeScrumTeam(teamLock.Team);
                        }
                    }
                    catch (Exception)
                    {
                        scrumTeamData = null;
                    }

                    NodeMessage initializeTeamMessage = new NodeMessage(NodeMessageType.InitializeTeam)
                    {
                        RecipientNodeId = message.SenderNodeId,
                        Data = scrumTeamData != null ? (object)scrumTeamData : (object)scrumTeamName
                    };
                    SendNodeMessage(initializeTeamMessage);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
