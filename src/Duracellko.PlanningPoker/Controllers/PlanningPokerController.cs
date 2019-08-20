﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;

using Observer = Duracellko.PlanningPoker.Domain.Observer;

namespace Duracellko.PlanningPoker.Controllers
{
    /// <summary>
    /// Manager of all Scrum teams playing planning poker.
    /// </summary>
    public class PlanningPokerController : IPlanningPoker
    {
        private readonly ConcurrentDictionary<string, Tuple<ScrumTeam, object>> _scrumTeams = new ConcurrentDictionary<string, Tuple<ScrumTeam, object>>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<PlanningPokerController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerController"/> class.
        /// </summary>
        public PlanningPokerController()
            : this(null, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerController" /> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider to provide current date-time.</param>
        /// <param name="configuration">The configuration of the planning poker.</param>
        /// <param name="repository">The Scrum teams repository.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public PlanningPokerController(DateTimeProvider dateTimeProvider, IPlanningPokerConfiguration configuration, IScrumTeamRepository repository, ILogger<PlanningPokerController> logger)
        {
            DateTimeProvider = dateTimeProvider ?? Duracellko.PlanningPoker.Domain.DateTimeProvider.Default;
            Configuration = configuration ?? new PlanningPokerConfiguration();
            Repository = repository ?? new EmptyScrumTeamRepository();
            _logger = logger;
        }

        /// <summary>
        /// Gets the date time provider to provide current date-time.
        /// </summary>
        /// <value>The <see cref="DateTimeProvider"/> object.</value>
        public DateTimeProvider DateTimeProvider { get; private set; }

        /// <summary>
        /// Gets the configuration of planning poker.
        /// </summary>
        /// <value>The configuration.</value>
        public IPlanningPokerConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets a collection of Scrum team names.
        /// </summary>
        public IEnumerable<string> ScrumTeamNames
        {
            get
            {
                return _scrumTeams.ToArray().Select(p => p.Key)
                    .Union(Repository.ScrumTeamNames.ToArray(), StringComparer.OrdinalIgnoreCase).ToArray();
            }
        }

        /// <summary>
        /// Gets the team repository.
        /// </summary>
        /// <value>
        /// The team repository.
        /// </value>
        protected IScrumTeamRepository Repository { get; private set; }

        /// <summary>
        /// Creates new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>
        /// The new Scrum team.
        /// </returns>
        public IScrumTeamLock CreateScrumTeam(string teamName, string scrumMasterName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException(nameof(teamName));
            }

            if (string.IsNullOrEmpty(scrumMasterName))
            {
                throw new ArgumentNullException(nameof(scrumMasterName));
            }

            OnBeforeCreateScrumTeam(teamName, scrumMasterName);

            ScrumTeam team = new ScrumTeam(teamName, DateTimeProvider);
            team.SetScrumMaster(scrumMasterName);
            object teamLock = new object();
            Tuple<ScrumTeam, object> teamTuple = new Tuple<ScrumTeam, object>(team, teamLock);

            // loads team from repository and adds it to in-memory collection
            LoadScrumTeam(teamName);

            if (!_scrumTeams.TryAdd(teamName, teamTuple))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Error_ScrumTeamAlreadyExists, teamName), nameof(teamName));
            }

            OnTeamAdded(team);
            _logger?.LogInformation(Resources.Info_ScrumTeamCreated, team.Name, team.ScrumMaster.Name);

            return new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2);
        }

        /// <summary>
        /// Adds existing Scrum team to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team to add.</param>
        /// <returns>The joined Scrum team.</returns>
        public IScrumTeamLock AttachScrumTeam(ScrumTeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            string teamName = team.Name;
            object teamLock = new object();
            Tuple<ScrumTeam, object> teamTuple = new Tuple<ScrumTeam, object>(team, teamLock);

            // loads team from repository and adds it to in-memory collection
            LoadScrumTeam(teamName);

            if (!_scrumTeams.TryAdd(teamName, teamTuple))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Error_ScrumTeamAlreadyExists, teamName), nameof(team));
            }

            OnTeamAdded(team);
            _logger?.LogInformation(Resources.Info_ScrumTeamAttached, team.Name);

            return new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2);
        }

        /// <summary>
        /// Gets existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>
        /// The Scrum team.
        /// </returns>
        public IScrumTeamLock GetScrumTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException(nameof(teamName));
            }

            OnBeforeGetScrumTeam(teamName);

            Tuple<ScrumTeam, object> teamTuple = LoadScrumTeam(teamName);
            if (teamTuple == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Error_ScrumTeamNotExist, teamName), nameof(teamName));
            }

            _logger?.LogDebug(Resources.Debug_ReadScrumTeam, teamTuple.Item1.Name);
            return new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2);
        }

        /// <summary>
        /// Calls specified callback when an observer receives new message or after configured timeout.
        /// </summary>
        /// <param name="observer">The observer to wait for message to receive.</param>
        /// <param name="callback">The callback delegate to call when a message is received or after timeout. First parameter specifies if message was received or not
        /// (the timeout occurs). Second parameter specifies observer, who received a message.</param>
        public void GetMessagesAsync(Observer observer, Action<bool, Observer> callback)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (observer.HasMessage)
            {
                _logger?.LogDebug(Resources.Debug_ObserverMessageReceived, observer.Name, observer.Team.Name, true);
                callback(true, observer);
            }
            else
            {
                // not nead to load from repository, because team was already obtained
                Tuple<ScrumTeam, object> teamTuple;
                if (!_scrumTeams.TryGetValue(observer.Team.Name, out teamTuple) || teamTuple.Item1 != observer.Team)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_ScrumTeamNotExist, observer.Team.Name));
                }

                IObservable<EventPattern<object>> messageReceivedObservable = Observable.FromEventPattern(h => observer.MessageReceived += h, h => observer.MessageReceived -= h);
                messageReceivedObservable = messageReceivedObservable.Timeout(Configuration.WaitForMessageTimeout, Observable.Return<System.Reactive.EventPattern<object>>(null));
                messageReceivedObservable = messageReceivedObservable.Take(1);
                messageReceivedObservable.Subscribe(p => ExecuteGetMessagesAsyncCallback(callback, observer, teamTuple));
            }
        }

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for configured period of time.
        /// </summary>
        public void DisconnectInactiveObservers()
        {
            DisconnectInactiveObservers(Configuration.ClientInactivityTimeout);
        }

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        public void DisconnectInactiveObservers(TimeSpan inactivityTime)
        {
            KeyValuePair<string, Tuple<ScrumTeam, object>>[] teamTuples = _scrumTeams.ToArray();
            foreach (KeyValuePair<string, Tuple<ScrumTeam, object>> teamTuple in teamTuples)
            {
                using (ScrumTeamLock teamLock = new ScrumTeamLock(teamTuple.Value.Item1, teamTuple.Value.Item2))
                {
                    teamLock.Lock();
                    _logger?.LogInformation(Resources.Info_DisconnectingInactiveObservers, teamLock.Team.Name);
                    teamLock.Team.DisconnectInactiveObservers(inactivityTime);
                }
            }
        }

        /// <summary>
        /// Executed when a Scrum team is added to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was added.</param>
        protected virtual void OnTeamAdded(ScrumTeam team)
        {
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>(ScrumTeamOnMessageReceived);
            _logger?.LogDebug(Resources.Debug_ScrumTeamAdded, team.Name);
        }

        /// <summary>
        /// Executed when a Scrum team is removed from collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team that was removed.</param>
        protected virtual void OnTeamRemoved(ScrumTeam team)
        {
            team.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(ScrumTeamOnMessageReceived);
            _logger?.LogDebug(Resources.Debug_ScrumTeamRemoved, team.Name);
        }

        /// <summary>
        /// Executed before creating new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        protected virtual void OnBeforeCreateScrumTeam(string teamName, string scrumMasterName)
        {
            // empty implementation by default
        }

        /// <summary>
        /// Executed before getting existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        protected virtual void OnBeforeGetScrumTeam(string teamName)
        {
            // empty implementation by default
        }

        private void ExecuteGetMessagesAsyncCallback(Action<bool, Observer> callback, Observer observer, Tuple<ScrumTeam, object> teamTuple)
        {
            using (ScrumTeamLock teamLock = new ScrumTeamLock(teamTuple.Item1, teamTuple.Item2))
            {
                teamLock.Lock();
                _logger?.LogDebug(Resources.Debug_ObserverMessageReceived, observer.Name, teamLock.Team.Name, observer.HasMessage);
                callback(observer.HasMessage, observer.HasMessage ? observer : null);
            }
        }

        private void ScrumTeamOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ScrumTeam team = (ScrumTeam)sender;
            bool saveTeam = true;

            LogScrumTeamMessage(team, e.Message);

            if (e.Message.MessageType == MessageType.MemberDisconnected)
            {
                if (!team.Members.Any() && !team.Observers.Any())
                {
                    saveTeam = false;
                    OnTeamRemoved(team);

                    Tuple<ScrumTeam, object> teamTuple;
                    _scrumTeams.TryRemove(team.Name, out teamTuple);
                    Repository.DeleteScrumTeam(team.Name);
                    _logger?.LogInformation(Resources.Info_ScrumTeamRemoved, team.Name);
                }
            }

            if (saveTeam)
            {
                SaveScrumTeam(team);
            }
        }

        private Tuple<ScrumTeam, object> LoadScrumTeam(string teamName)
        {
            Tuple<ScrumTeam, object> result = null;
            bool retry = true;

            while (retry)
            {
                retry = false;

                if (!_scrumTeams.TryGetValue(teamName, out result))
                {
                    result = null;

                    ScrumTeam team = Repository.LoadScrumTeam(teamName);
                    if (team != null)
                    {
                        if (VerifyTeamActive(team))
                        {
                            object teamLock = new object();
                            result = new Tuple<ScrumTeam, object>(team, teamLock);
                            if (_scrumTeams.TryAdd(team.Name, result))
                            {
                                OnTeamAdded(team);
                            }
                            else
                            {
                                result = null;
                                retry = true;
                            }
                        }
                        else
                        {
                            Repository.DeleteScrumTeam(team.Name);
                        }
                    }
                }
            }

            return result;
        }

        private void SaveScrumTeam(ScrumTeam team)
        {
            Repository.SaveScrumTeam(team);
        }

        private bool VerifyTeamActive(ScrumTeam team)
        {
            team.DisconnectInactiveObservers(Configuration.ClientInactivityTimeout);
            return team.Members.Any() || team.Observers.Any();
        }

        private void LogScrumTeamMessage(ScrumTeam team, Message message)
        {
            if (message is MemberMessage memberMessage)
            {
                _logger?.LogInformation(Resources.Info_MemberMessage, team.Name, memberMessage.Id, memberMessage.MessageType, memberMessage.Member?.Name);
            }
            else
            {
                _logger?.LogInformation(Resources.Info_ScrumTeamMessage, team.Name, message.Id, message.MessageType);
            }
        }

        /// <summary>
        /// Object used to lock Scrum team, so that only one thread can access the Scrum team at time.
        /// </summary>
        private sealed class ScrumTeamLock : IScrumTeamLock
        {
            private readonly object _lockObject;
            private bool _locked;

            /// <summary>
            /// Initializes a new instance of the <see cref="ScrumTeamLock"/> class.
            /// </summary>
            /// <param name="team">The Scrum team.</param>
            /// <param name="lockObj">The object used to lock the Scrum team.</param>
            public ScrumTeamLock(ScrumTeam team, object lockObj)
            {
                Team = team;
                _lockObject = lockObj;
            }

            /// <summary>
            /// Gets the Scrum team associated to the lock.
            /// </summary>
            /// <value>
            /// The Scrum team.
            /// </value>
            public ScrumTeam Team { get; private set; }

            /// <summary>
            /// Locks the Scrum team, so that other threads are not able to access the team.
            /// </summary>
            public void Lock()
            {
                if (!_locked)
                {
                    Monitor.TryEnter(_lockObject, 10000, ref _locked);
                    if (!_locked)
                    {
                        throw new TimeoutException(Resources.Error_ScrumTeamTimeout);
                    }
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (_locked)
                {
                    Monitor.Exit(_lockObject);
                }
            }
        }
    }
}
