using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Scrum team is a group of members, who play planning poker, and observers, who watch the game.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Events are placed together with protected methods.")]
    public class ScrumTeam
    {
        private readonly List<Member> _members = new List<Member>();
        private readonly List<Observer> _observers = new List<Observer>();

        private readonly Estimate[] _availableEstimates = new Estimate[]
        {
            new Estimate(0.0),
            new Estimate(0.5),
            new Estimate(1.0),
            new Estimate(2.0),
            new Estimate(3.0),
            new Estimate(5.0),
            new Estimate(8.0),
            new Estimate(13.0),
            new Estimate(20.0),
            new Estimate(40.0),
            new Estimate(100.0),
            new Estimate(double.PositiveInfinity),
            new Estimate()
        };

        private EstimateResult _estimateResult;

        [NonSerialized]
        private DateTimeProvider _dateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="name">The team name.</param>
        public ScrumTeam(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeam"/> class.
        /// </summary>
        /// <param name="name">The team name.</param>
        /// <param name="dateTimeProvider">The date time provider to provide current time. If null is specified, then default date time provider is used.</param>
        public ScrumTeam(string name, DateTimeProvider dateTimeProvider)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _dateTimeProvider = dateTimeProvider ?? Duracellko.PlanningPoker.Domain.DateTimeProvider.Default;
            Name = name;
        }

        /// <summary>
        /// Gets the Scrum team name.
        /// </summary>
        /// <value>The Scrum team name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the observers watching planning poker game of the Scrum team.
        /// </summary>
        /// <value>The observers collection.</value>
        public IEnumerable<Observer> Observers
        {
            get
            {
                return _observers;
            }
        }

        /// <summary>
        /// Gets the collection members joined to the Scrum team.
        /// </summary>
        /// <value>The members collection.</value>
        public IEnumerable<Member> Members
        {
            get
            {
                return _members;
            }
        }

        /// <summary>
        /// Gets the scrum master of the team.
        /// </summary>
        /// <value>The Scrum master.</value>
        public ScrumMaster ScrumMaster
        {
            get
            {
                return Members.OfType<ScrumMaster>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the available estimates the members can pick from.
        /// </summary>
        /// <value>The collection of available estimates.</value>
        public IEnumerable<Estimate> Estimates
        {
            get
            {
                return _availableEstimates;
            }
        }

        /// <summary>
        /// Gets the current Scrum team state.
        /// </summary>
        /// <value>The team state.</value>
        public TeamState State { get; private set; }

        /// <summary>
        /// Gets the estimate result, when <see cref="P:State"/> is EstimateFinished.
        /// </summary>
        /// <value>The estimate result.</value>
        public EstimateResult EstimateResult
        {
            get
            {
                return State == TeamState.EstimateFinished ? _estimateResult : null;
            }
        }

        /// <summary>
        /// Gets the collection of participants in current estimate.
        /// </summary>
        /// <value>
        /// The estimate participants.
        /// </value>
        public IEnumerable<EstimateParticipantStatus> EstimateParticipants
        {
            get
            {
                if (State == TeamState.EstimateInProgress)
                {
                    return _estimateResult.Select(p => new EstimateParticipantStatus(p.Key.Name, p.Value != null)).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the date time provider used by the Scrum team.
        /// </summary>
        /// <value>The date-time provider.</value>
        public DateTimeProvider DateTimeProvider
        {
            get
            {
                return _dateTimeProvider;
            }
        }

        /// <summary>
        /// Sets new scrum master of the team.
        /// </summary>
        /// <param name="name">The Scrum master name.</param>
        /// <returns>The new Scrum master.</returns>
        public ScrumMaster SetScrumMaster(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (FindMemberOrObserver(name) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Error_MemberAlreadyExists, name), nameof(name));
            }

            if (ScrumMaster != null)
            {
                throw new InvalidOperationException(Resources.Error_ScrumMasterAlreadyExists);
            }

            ScrumMaster scrumMaster = new ScrumMaster(this, name);
            _members.Add(scrumMaster);

            IEnumerable<Observer> recipients = UnionMembersAndObservers().Where(m => m != scrumMaster);
            SendMessage(recipients, () => new MemberMessage(MessageType.MemberJoined) { Member = scrumMaster });

            return scrumMaster;
        }

        /// <summary>
        /// Connects new member or observer with specified name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <param name="asObserver">If set to <c>true</c> then connect new observer, otherwise member.</param>
        /// <returns>The observer or member, who joined the Scrum team.</returns>
        public Observer Join(string name, bool asObserver)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (FindMemberOrObserver(name) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.Error_MemberAlreadyExists, name), nameof(name));
            }

            Observer result;
            if (asObserver)
            {
                Observer observer = new Observer(this, name);
                _observers.Add(observer);
                result = observer;
            }
            else
            {
                Member member = new Member(this, name);
                _members.Add(member);
                result = member;
            }

            IEnumerable<Observer> recipients = UnionMembersAndObservers().Where(m => m != result);
            SendMessage(recipients, () => new MemberMessage(MessageType.MemberJoined) { Member = result });

            return result;
        }

        /// <summary>
        /// Disconnects member with specified name from the Scrum team.
        /// </summary>
        /// <param name="name">The member name.</param>
        public void Disconnect(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Observer observer = _observers.FirstOrDefault(o => MatchObserverName(o, name));
            if (observer != null)
            {
                _observers.Remove(observer);

                IEnumerable<Observer> recipients = UnionMembersAndObservers();
                SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = observer });

                // Send message to disconnecting observer, so that he/she stops waiting for messages.
                observer.SendMessage(new Message(MessageType.Empty));
            }
            else
            {
                Member member = _members.FirstOrDefault(o => MatchObserverName(o, name));
                if (member != null)
                {
                    _members.Remove(member);

                    if (State == TeamState.EstimateInProgress)
                    {
                        // Check if all members picked estimates. If member disconnects then his/her estimate is null.
                        UpdateEstimateResult(null);
                    }

                    IEnumerable<Observer> recipients = UnionMembersAndObservers();
                    SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = member });

                    // Send message to disconnecting member, so that he/she stops waiting for messages.
                    member.SendMessage(new Message(MessageType.Empty));
                }
            }
        }

        /// <summary>
        /// Finds existing member or observer with specified name.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <returns>The member or observer.</returns>
        public Observer FindMemberOrObserver(string name)
        {
            IEnumerable<Observer> allObservers = Observers.Union(Members);
            return allObservers.FirstOrDefault(o => MatchObserverName(o, name));
        }

        /// <summary>
        /// Disconnects inactive observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        public void DisconnectInactiveObservers(TimeSpan inactivityTime)
        {
            DateTime lastInactivityTime = DateTimeProvider.UtcNow - inactivityTime;
            Func<Observer, bool> isObserverActive = new Func<Observer, bool>(o => o.LastActivity < lastInactivityTime);
            Observer[] inactiveObservers = Observers.Where(isObserverActive).ToArray();
            Member[] inactiveMembers = Members.Where<Member>(isObserverActive).ToArray();

            if (inactiveObservers.Length > 0 || inactiveMembers.Length > 0)
            {
                foreach (Observer observer in inactiveObservers)
                {
                    _observers.Remove(observer);
                }

                foreach (Member member in inactiveMembers)
                {
                    _members.Remove(member);
                }

                IEnumerable<Observer> recipients = UnionMembersAndObservers();
                foreach (Observer member in inactiveObservers.Union(inactiveMembers))
                {
                    SendMessage(recipients, () => new MemberMessage(MessageType.MemberDisconnected) { Member = member });
                }

                if (inactiveMembers.Length > 0)
                {
                    if (State == TeamState.EstimateInProgress)
                    {
                        // Check if all members picked estimates. If member disconnects then his/her estimate is null.
                        UpdateEstimateResult(null);
                    }
                }
            }
        }

        /// <summary>
        /// Starts new estimate.
        /// </summary>
        internal void StartEstimate()
        {
            State = TeamState.EstimateInProgress;

            foreach (Member member in Members)
            {
                member.ResetEstimate();
            }

            _estimateResult = new EstimateResult(Members);

            IEnumerable<Observer> recipients = UnionMembersAndObservers();
            SendMessage(recipients, () => new Message(MessageType.EstimateStarted));
        }

        /// <summary>
        /// Cancels current estimate.
        /// </summary>
        internal void CancelEstimate()
        {
            State = TeamState.EstimateCanceled;
            _estimateResult = null;

            IEnumerable<Observer> recipients = UnionMembersAndObservers();
            SendMessage(recipients, () => new Message(MessageType.EstimateCanceled));
        }

        /// <summary>
        /// Notifies that a member has placed estimate.
        /// </summary>
        /// <param name="member">The member, who estimated.</param>
        internal void OnMemberEstimated(Member member)
        {
            IEnumerable<Observer> recipients = UnionMembersAndObservers();
            SendMessage(recipients, () => new MemberMessage(MessageType.MemberEstimated) { Member = member });
            UpdateEstimateResult(member);
        }

        /// <summary>
        /// Notifies that a member is still active.
        /// </summary>
        /// <param name="observer">The observer.</param>
        internal void OnObserverActivity(Observer observer)
        {
            SendMessage(new MemberMessage(MessageType.MemberActivity) { Member = observer });
        }

        /// <summary>
        /// Occurs when a new message is received.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raises the <see cref="E:MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, e);
            }
        }

        private static bool MatchObserverName(Observer observer, string name)
        {
            return string.Equals(observer.Name, name, StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<Observer> UnionMembersAndObservers()
        {
            foreach (Member member in Members)
            {
                yield return member;
            }

            foreach (Observer observer in Observers)
            {
                yield return observer;
            }
        }

        private void SendMessage(Message message)
        {
            if (message != null)
            {
                OnMessageReceived(new MessageReceivedEventArgs(message));
            }
        }

        private void SendMessage(IEnumerable<Observer> recipients, Func<Message> messageFactory)
        {
            SendMessage(messageFactory());
            foreach (Observer recipient in recipients)
            {
                recipient.SendMessage(messageFactory());
            }
        }

        /// <summary>
        /// Checks if all members picked an estimate. If yes, then finishes the estimate.
        /// </summary>
        /// <param name="member">The who initiated member updating of estimate results.</param>
        private void UpdateEstimateResult(Member member)
        {
            if (member != null)
            {
                if (_estimateResult.ContainsMember(member))
                {
                    _estimateResult[member] = member.Estimate;
                }
            }

            if (_estimateResult.All(p => p.Value != null || !Members.Contains(p.Key)))
            {
                _estimateResult.SetReadOnly();
                State = TeamState.EstimateFinished;

                IEnumerable<Observer> recipients = UnionMembersAndObservers();
                SendMessage(recipients, () => new EstimateResultMessage(MessageType.EstimateEnded) { EstimateResult = _estimateResult });
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DateTimeProvider dateTimeProvider = context.Context as DateTimeProvider;
            _dateTimeProvider = dateTimeProvider ?? Duracellko.PlanningPoker.Domain.DateTimeProvider.Default;
        }
    }
}
