using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class PlanningPokerControllerTestMessages
    {
        private CultureInfo _originalCultureInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalCultureInfo = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_originalCultureInfo != null)
            {
                CultureInfo.CurrentCulture = _originalCultureInfo;
                _originalCultureInfo = null;
            }
        }

        [TestMethod]
        public async Task ProcessMessages_EmptyCollection_LastMessageIdIsNotUpdated()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            target.ProcessMessages(Enumerable.Empty<Message>());

            Assert.AreEqual(0, propertyChangedCounter.Count);
            Assert.AreEqual(-1, target.LastMessageId);
        }

        [TestMethod]
        public async Task ProcessMessages_EmptyMessage_LastMessageIdIsUpdated()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Message message = new Message
            {
                Id = 4,
                Type = MessageType.Empty
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(4, target.LastMessageId);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberJoinedWithMember_2Members()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            MemberMessage message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "New member"
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            string[] expectedMembers = new string[] { "New member", PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_MemberJoinedWithObserver_1Observer()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Observers = null;
            scrumTeam.State = TeamState.EstimateFinished;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            MemberMessage message = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ObserverType,
                    Name = "New observer"
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(1, target.LastMessageId);
            string[] expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = new string[] { "New observer" };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_2xMemberJoinedWithMember_3Members()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            MemberMessage message1 = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "XYZ"
                }
            };
            MemberMessage message2 = new MemberMessage
            {
                Id = 2,
                Type = MessageType.MemberJoined,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "New member"
                }
            };
            target.ProcessMessages(new Message[] { message1, message2 });

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(2, target.LastMessageId);
            string[] expectedMembers = new string[] { "New member", PlanningPokerData.MemberName, "XYZ" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithScrumMaster_ScrumMasterIsNull()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ObserverName);

            MemberMessage message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            Assert.IsNull(target.ScrumMaster);
            string[] expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithMember_1Member()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            TeamMember member = new TeamMember
            {
                Type = PlanningPokerData.MemberType,
                Name = "New member",
            };
            scrumTeam.Members.Add(member);
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            MemberMessage message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster);
            string[] expectedMembers = new string[] { "New member" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithObserver_0Observers()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            MemberMessage message = new MemberMessage
            {
                Id = 1,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ObserverType,
                    Name = PlanningPokerData.ObserverName
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(1, target.LastMessageId);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster);
            string[] expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = Array.Empty<string>();
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_MemberDisconnectedWithNotExistingName_NoChanges()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateCanceled;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            MemberMessage message = new MemberMessage
            {
                Id = 0,
                Type = MessageType.MemberDisconnected,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Disconnect"
                }
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(0, target.LastMessageId);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster);
            string[] expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            expectedMembers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedMembers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateStartedAndStateIsInitialize_StateIsEstimateInProgress()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Message message = new Message
            {
                Id = 1,
                Type = MessageType.EstimateStarted
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(1, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateInProgress, target.ScrumTeam.State);
            Assert.IsTrue(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateStartedAndStateIsEstimateFinished_StateIsEstimateInProgress()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateFinished;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Message message = new Message
            {
                Id = 2,
                Type = MessageType.EstimateStarted
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(2, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateInProgress, target.ScrumTeam.State);
            Assert.IsTrue(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateStartedAndStateIsEstimateCanceled_StateIsEstimateInProgress()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateCanceled;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Message message = new Message
            {
                Id = 3,
                Type = MessageType.EstimateStarted
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(3, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateInProgress, target.ScrumTeam.State);
            Assert.IsTrue(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateCanceled_StateIsEstimateCanceled()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Message message = new Message
            {
                Id = 4,
                Type = MessageType.EstimateCanceled
            };
            ProcessMessage(target, message);

            Assert.AreEqual(1, propertyChangedCounter.Count);
            Assert.AreEqual(4, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateCanceled, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberEstimatedWithMember_1Estimate()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            MemberMessage message = new MemberMessage
            {
                Id = 3,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            StartEstimate(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(3, target.LastMessageId);
            Assert.IsTrue(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);

            Assert.AreEqual(1, target.Estimates.Count());
            MemberEstimate estimate = target.Estimates.First();
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_MemberEstimatedWithScrumMaster_1Estimate()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ObserverName);

            MemberMessage message = new MemberMessage
            {
                Id = 4,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            StartEstimate(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(4, target.LastMessageId);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);

            Assert.AreEqual(1, target.Estimates.Count());
            MemberEstimate estimate = target.Estimates.First();
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_2xMemberEstimated_2Estimates()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            MemberMessage message1 = new MemberMessage
            {
                Id = 5,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            MemberMessage message2 = new MemberMessage
            {
                Id = 6,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            StartEstimate(target);
            target.ProcessMessages(new Message[] { message2, message1 });

            Assert.AreEqual(3, propertyChangedCounter.Count);
            Assert.AreEqual(6, target.LastMessageId);
            Assert.IsTrue(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);

            List<MemberEstimate> estimates = target.Estimates.ToList();
            Assert.AreEqual(2, estimates.Count);
            MemberEstimate estimate = estimates[0];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
            estimate = estimates[1];
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateEnded_5Estimates()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            MemberMessage message1 = new MemberMessage
            {
                Id = 5,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Developer 1"
                }
            };
            MemberMessage message2 = new MemberMessage
            {
                Id = 6,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = PlanningPokerData.MemberName
                }
            };
            MemberMessage message3 = new MemberMessage
            {
                Id = 7,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.ScrumMasterType,
                    Name = PlanningPokerData.ScrumMasterName
                }
            };
            MemberMessage message4 = new MemberMessage
            {
                Id = 8,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Tester"
                }
            };
            MemberMessage message5 = new MemberMessage
            {
                Id = 9,
                Type = MessageType.MemberEstimated,
                Member = new TeamMember
                {
                    Type = PlanningPokerData.MemberType,
                    Name = "Developer 2"
                }
            };

            EstimateResultMessage message = new EstimateResultMessage
            {
                Id = 10,
                Type = MessageType.EstimateEnded,
                EstimateResult = new List<EstimateResultItem>
                {
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                        Estimate = new Estimate { Value = 8 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" },
                        Estimate = new Estimate { Value = 8 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                        Estimate = new Estimate { Value = 3 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                        Estimate = new Estimate { Value = 8 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                        Estimate = new Estimate { Value = 2 }
                    }
                }
            };

            StartEstimate(target);
            ProcessMessage(target, message1);
            target.ProcessMessages(new Message[] { message2, message3, message4 });
            target.ProcessMessages(new Message[] { message5, message });

            Assert.AreEqual(7, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);

            List<MemberEstimate> estimates = target.Estimates.ToList();
            Assert.AreEqual(5, estimates.Count);

            MemberEstimate estimate = estimates[0];
            Assert.AreEqual("Developer 1", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(8.0, estimate.Estimate);

            estimate = estimates[1];
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(8.0, estimate.Estimate);

            estimate = estimates[2];
            Assert.AreEqual("Tester", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(8.0, estimate.Estimate);

            estimate = estimates[3];
            Assert.AreEqual("Developer 2", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(2.0, estimate.Estimate);

            estimate = estimates[4];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(3.0, estimate.Estimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateEndedAndMemberWithoutEstimate_4Estimates()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            EstimateResultMessage message = new EstimateResultMessage
            {
                Id = 10,
                Type = MessageType.EstimateEnded,
                EstimateResult = new List<EstimateResultItem>
                {
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                        Estimate = new Estimate { Value = 0 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" },
                        Estimate = new Estimate { Value = null }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                        Estimate = new Estimate { Value = double.PositiveInfinity }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                        Estimate = new Estimate { Value = double.PositiveInfinity }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    }
                }
            };

            StartEstimate(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);

            List<MemberEstimate> estimates = target.Estimates.ToList();
            Assert.AreEqual(5, estimates.Count);

            MemberEstimate estimate = estimates[0];
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(double.PositiveInfinity, estimate.Estimate);

            estimate = estimates[1];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(double.PositiveInfinity, estimate.Estimate);

            estimate = estimates[2];
            Assert.AreEqual("Developer 1", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(0.0, estimate.Estimate);

            estimate = estimates[3];
            Assert.AreEqual("Tester", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);

            estimate = estimates[4];
            Assert.AreEqual("Developer 2", estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateEndedAndSameEstimateCount_6Estimates()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            EstimateResultMessage message = new EstimateResultMessage
            {
                Id = 10,
                Type = MessageType.EstimateEnded,
                EstimateResult = new List<EstimateResultItem>
                {
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                        Estimate = new Estimate { Value = 20 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                        Estimate = new Estimate { Value = 0 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" },
                        Estimate = new Estimate { Value = 13 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                        Estimate = new Estimate { Value = 13 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                        Estimate = new Estimate { Value = 0 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" },
                        Estimate = new Estimate { Value = 20 }
                    }
                }
            };

            StartEstimate(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);

            List<MemberEstimate> estimates = target.Estimates.ToList();
            Assert.AreEqual(6, estimates.Count);

            MemberEstimate estimate = estimates[0];
            Assert.AreEqual("Developer 1", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(0.0, estimate.Estimate);

            estimate = estimates[1];
            Assert.AreEqual("Developer 2", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(0.0, estimate.Estimate);

            estimate = estimates[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(13.0, estimate.Estimate);

            estimate = estimates[3];
            Assert.AreEqual("Tester 1", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(13.0, estimate.Estimate);

            estimate = estimates[4];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(20.0, estimate.Estimate);

            estimate = estimates[5];
            Assert.AreEqual("Tester 2", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(20.0, estimate.Estimate);
        }

        [TestMethod]
        public async Task ProcessMessages_EstimateEndedAndSameEstimateCountWithNull_6Estimates()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            EstimateResultMessage message = new EstimateResultMessage
            {
                Id = 10,
                Type = MessageType.EstimateEnded,
                EstimateResult = new List<EstimateResultItem>
                {
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                        Estimate = new Estimate { Value = double.PositiveInfinity }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                        Estimate = new Estimate { Value = null }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" },
                        Estimate = new Estimate { Value = null }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                        Estimate = new Estimate { Value = double.PositiveInfinity }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                        Estimate = new Estimate { Value = 5 }
                    },
                    new EstimateResultItem
                    {
                        Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" },
                        Estimate = new Estimate { Value = 5 }
                    }
                }
            };

            StartEstimate(target);
            ProcessMessage(target, message);

            Assert.AreEqual(2, propertyChangedCounter.Count);
            Assert.AreEqual(10, target.LastMessageId);
            Assert.AreEqual(TeamState.EstimateFinished, target.ScrumTeam.State);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);

            List<MemberEstimate> estimates = target.Estimates.ToList();
            Assert.AreEqual(6, estimates.Count);

            MemberEstimate estimate = estimates[0];
            Assert.AreEqual("Developer 1", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(5.0, estimate.Estimate);

            estimate = estimates[1];
            Assert.AreEqual("Tester 2", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(5.0, estimate.Estimate);

            estimate = estimates[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(double.PositiveInfinity, estimate.Estimate);

            estimate = estimates[3];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.AreEqual(double.PositiveInfinity, estimate.Estimate);

            estimate = estimates[4];
            Assert.AreEqual("Developer 2", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);

            estimate = estimates[5];
            Assert.AreEqual("Tester 1", estimate.MemberName);
            Assert.IsTrue(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);
        }

        [TestMethod]
        public async Task ProcessMessages_Null_ArgumentNullException()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(propertyChangedCounter);
            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.ThrowsException<ArgumentNullException>(() => target.ProcessMessages(null));
        }

        private static PlanningPokerController CreateController(PropertyChangedCounter propertyChangedCounter = null)
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            Mock<IBusyIndicatorService> busyIndicator = new Mock<IBusyIndicatorService>();
            Mock<IMemberCredentialsStore> memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            PlanningPokerController result = new PlanningPokerController(planningPokerClient.Object, busyIndicator.Object, memberCredentialsStore.Object);
            if (propertyChangedCounter != null)
            {
                // Subtract 1 PropertyChanged event raised by InitializeTeam
                propertyChangedCounter.Count = -1;
                propertyChangedCounter.Target = result;
            }

            return result;
        }

        private static void ProcessMessage(PlanningPokerController controller, Message message)
        {
            controller.ProcessMessages(new Message[] { message });
        }

        private static void StartEstimate(PlanningPokerController controller)
        {
            Message message = new Message
            {
                Id = 1,
                Type = MessageType.EstimateStarted
            };
            ProcessMessage(controller, message);
        }
    }
}
