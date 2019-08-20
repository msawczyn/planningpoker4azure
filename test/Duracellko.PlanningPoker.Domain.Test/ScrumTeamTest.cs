using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ScrumTeamTest
    {
        [TestMethod]
        public void Constructor_TeamNameSpecified_TeamNameIsSet()
        {
            // Arrange
            string name = "test team";

            // Act
            ScrumTeam result = new ScrumTeam(name);

            // Verify
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            string name = string.Empty;

            // Act
            ScrumTeam result = new ScrumTeam(name);
        }

        [TestMethod]
        public void Observers_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            IEnumerable<Observer> result = target.Observers;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void Members_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            IEnumerable<Member> result = target.Members;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void ScrumMaster_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            ScrumMaster result = target.ScrumMaster;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ScrumMaster_SetScrumMaster_ReturnsNewScrumMasterOfTheTeam()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster(name);

            // Act
            ScrumMaster result = target.ScrumMaster;

            // Verify
            Assert.AreEqual<ScrumMaster>(master, result);
        }

        [TestMethod]
        public void AvailableEstimates_Get_ReturnsPlanningPokerCardValues()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            var result = target.AvailableEstimates;

            // Verify
            double?[] expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, double.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
        }

        [TestMethod]
        public void State_GetAfterConstruction_ReturnsInitial()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            TeamState result = target.State;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.Initial, result);
        }

        [TestMethod]
        public void EstimateResult_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            EstimateResult result = target.EstimateResult;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EstimateParticipants_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            IEnumerable<EstimateParticipantStatus> result = target.EstimateParticipants;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EstimateParticipants_EstimateStarted_ReturnsScrumMaster()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            master.StartEstimate();

            // Act
            IEnumerable<EstimateParticipantStatus> result = target.EstimateParticipants;

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<int>(1, result.Count());
            Assert.AreEqual<string>(master.Name, result.First().MemberName);
            Assert.IsFalse(result.First().Estimated);
        }

        [TestMethod]
        public void EstimateParticipants_MemberEstimated_MemberEstimatedButNotScrumMaster()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            master.StartEstimate();

            // Act
            IEnumerable<EstimateParticipantStatus> result = target.EstimateParticipants;

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<int>(2, result.Count());

            EstimateParticipantStatus masterParticipant = result.First(p => p.MemberName == master.Name);
            Assert.IsNotNull(masterParticipant);
            Assert.IsFalse(masterParticipant.Estimated);

            EstimateParticipantStatus memberParticipant = result.First(p => p.MemberName == member.Name);
            Assert.IsNotNull(memberParticipant);
            Assert.IsFalse(memberParticipant.Estimated);
        }

        [TestMethod]
        public void SetScrumMaster_SetName_ReturnsNewScrumMasterOfTheTeam()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            ScrumMaster result = target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<ScrumTeam>(target, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void SetScrumMaster_NoMembers_ScrumTeamGetsMemberJoinedMessage()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            ScrumMaster result = target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void SetScrumMaster_ObserverAlreadyJoined_ObserverGetsMemberJoinedMessage()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            Observer observer = target.Join("observer", true);

            // Act
            ScrumMaster result = target.SetScrumMaster(name);

            // Verify
            Assert.AreEqual<int>(1, observer.Messages.Count());
            Message message = observer.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void SetScrumMaster_ObserverAlreadyJoined_ObserverMessageReceived()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            Observer observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void SetScrumMaster_MemberAlreadyJoined_MemberGetsMemberJoinedMessage()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            Observer member = target.Join("member", false);

            // Act
            ScrumMaster result = target.SetScrumMaster(name);

            // Verify
            Assert.AreEqual<int>(1, member.Messages.Count());
            Message message = member.Messages.First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void SetScrumMaster_MemberAlreadyJoined_MemberMessageReceived()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            Observer member = target.Join("member", false);
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.SetScrumMaster(name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetScrumMaster_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            string name = string.Empty;
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            ScrumMaster result = target.SetScrumMaster(name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetScrumMaster_ScrumMasterIsAlreadySet_InvalidOperationException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.SetScrumMaster("master");

            // Act
            ScrumMaster result = target.SetScrumMaster(name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetScrumMaster_MemberWithSpecifiedNameExists_ArgumentException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join("test", false);

            // Act
            ScrumMaster result = target.SetScrumMaster(name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetScrumMaster_ObserverWithSpecifiedNameExists_ArgumentException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join("test", true);

            // Act
            ScrumMaster result = target.SetScrumMaster(name);
        }

        [TestMethod]
        public void Join_SetNameAndNotIsObserver_ReturnsNewMemberOfTheTeam()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result = target.Join(name, false);

            // Verify
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Member));
            Assert.AreEqual<ScrumTeam>(target, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void Join_SetNameAndIsObserver_ReturnsNewObserverOfTheTeam()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result = target.Join(name, true);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<Type>(typeof(Observer), result.GetType());
            Assert.AreEqual<ScrumTeam>(target, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Join_NameIsEmpty_ArgumentNullEsception()
        {
            // Arrange
            string name = string.Empty;
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result = target.Join(name, false);
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserver_MemberIsInMembersCollection()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result = target.Join(name, false);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result }, target.Members.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserverTwice_MembersAreInMembersCollection()
        {
            // Arrange
            string name1 = "test1";
            string name2 = "test2";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result1 = target.Join(name1, false);
            Observer result2 = target.Join(name2, false);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result1, result2 }, target.Members.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserverTwice_ScrumTeamGets2Messages()
        {
            // Arrange
            string name1 = "test1";
            string name2 = "test2";
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            List<MessageReceivedEventArgs> eventArgsList = new List<MessageReceivedEventArgs>();
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

            // Act
            target.Join(name1, false);
            target.Join(name2, false);

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            Message message1 = eventArgsList[0].Message;
            Message message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
        }

        [TestMethod]
        public void Join_SetNameAndIsNotObserverTwice_ScrumMasterGets2Messages()
        {
            // Arrange
            string name1 = "test1";
            string name2 = "test2";
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");

            // Act
            target.Join(name1, false);
            target.Join(name2, false);

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            Message message1 = master.Messages.First();
            Message message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void Join_SetNameAndIsObserver_ObserverIsInObserversCollection()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result = target.Join(name, true);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result }, target.Observers.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsObserverTwice_ObserversAreInObserversCollection()
        {
            // Arrange
            string name1 = "test1";
            string name2 = "test2";
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            Observer result1 = target.Join(name1, true);
            Observer result2 = target.Join(name2, true);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { result1, result2 }, target.Observers.ToList());
        }

        [TestMethod]
        public void Join_SetNameAndIsObserverTwice_ScrumTeamGets2Messages()
        {
            // Arrange
            string name1 = "test1";
            string name2 = "test2";
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            List<MessageReceivedEventArgs> eventArgsList = new List<MessageReceivedEventArgs>();
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));

            // Act
            target.Join(name1, true);
            target.Join(name2, true);

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            Message message1 = eventArgsList[0].Message;
            Message message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
        }

        [TestMethod]
        public void Join_SetNameAndIsObserverTwice_ScrumMasterGets2Messages()
        {
            // Arrange
            string name1 = "test1";
            string name2 = "test2";
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");

            // Act
            target.Join(name1, true);
            target.Join(name2, true);

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            Message message1 = master.Messages.First();
            Message message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsMemberAndMemberWithNameExists_ArgumentException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join(name, false);

            // Act
            Observer result = target.Join(name, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsMemberAndObserverWithNameExists_ArgumentException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join(name, true);

            // Act
            Observer result = target.Join(name, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsObserverAndMemberWithNameExists_ArgumentException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join(name, false);

            // Act
            Observer result = target.Join(name, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Join_AsObserverAndObserverWithNameExists_ArgumentException()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join(name, true);

            // Act
            Observer result = target.Join(name, true);
        }

        [TestMethod]
        public void Join_EstimateStarted_OnlyScrumMasterIsInEstimateResult()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();

            // Act
            Member member = (Member)target.Join("member", false);
            master.Estimate = masterEstimate;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateFinished, target.State);
            Assert.IsNotNull(target.EstimateResult);
            KeyValuePair<Member, Estimate>[] expectedResult = new KeyValuePair<Member, Estimate>[]
            {
                new KeyValuePair<Member, Estimate>(master, masterEstimate)
            };
            CollectionAssert.AreEquivalent(expectedResult, target.EstimateResult.ToList());
        }

        [TestMethod]
        public void Join_EstimateStarted_OnlyScrumMasterIsInEstimateParticipants()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            master.StartEstimate();

            // Act
            Member member = (Member)target.Join("member", false);
            IEnumerable<EstimateParticipantStatus> result = target.EstimateParticipants;

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<int>(1, result.Count());
            Assert.AreEqual<string>(master.Name, result.First().MemberName);
        }

        [TestMethod]
        public void Join_AsMember_ScrumMasterGetMemberJoinedMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void Join_AsMember_ScrumTeamGetMessageWithMember()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void Join_AsMember_ScrumMasterGetMessageWithMember()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Message message = master.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void Join_AsMember_ScrumMasterMessageReceived()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Join_AsMember_MemberDoesNotGetMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsFalse(result.HasMessage);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_ObserverGetMemberJoinedMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer observer = target.Join("observer", true);

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_ObserverGetMessageWithMember()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer observer = target.Join("observer", true);

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Message message = observer.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(result, memberMessage.Member);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_ObserverMessageReceived()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Join_AsMemberWhenObserverExists_MemberDoesNotGetMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer observer = target.Join("observer", true);

            // Act
            Observer result = target.Join("member", false);

            // Verify
            Assert.IsFalse(result.HasMessage);
        }

        [TestMethod]
        public void Disconnect_NameOfTheMember_MemberIsRemovedFromTheTeam()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join(name, false);

            // Act
            target.Disconnect(name);

            // Verify
            Assert.IsFalse(target.Members.Any());
        }

        [TestMethod]
        public void Disconnect_NameOfTheObserver_ObserverIsRemovedFromTheTeam()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join(name, true);

            // Act
            target.Disconnect(name);

            // Verify
            Assert.IsFalse(target.Observers.Any());
        }

        [TestMethod]
        public void Disconnect_ObserverNorMemberWithTheNameDoNotExist_ObserversAndMembersAreUnchanged()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            Observer observer = target.Join("observer", true);
            Observer member = target.Join("member", false);

            // Act
            target.Disconnect(name);

            // Verify
            CollectionAssert.AreEquivalent(new Observer[] { observer }, target.Observers.ToList());
            CollectionAssert.AreEquivalent(new Observer[] { member }, target.Members.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Disconnect_EmptyName_ArgumentNullException()
        {
            // Arrange
            string name = string.Empty;
            ScrumTeam target = new ScrumTeam("test team");

            // Act
            target.Disconnect(name);
        }

        [TestMethod]
        public void Disconnect_EstimateStarted_OnlyScrumMasterIsInEstimateResult()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Member member = (Member)target.Join("member", false);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();

            // Act
            target.Disconnect(member.Name);
            master.Estimate = masterEstimate;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateFinished, target.State);
            Assert.IsNotNull(target.EstimateResult);
            KeyValuePair<Member, Estimate>[] expectedResult = new KeyValuePair<Member, Estimate>[]
            {
                new KeyValuePair<Member, Estimate>(master, masterEstimate),
                new KeyValuePair<Member, Estimate>(member, null)
            };
            CollectionAssert.AreEquivalent(expectedResult, target.EstimateResult.ToList());
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumTeamGetMemberDisconnectedMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumTeamGetMessageWithMember()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            MessageReceivedEventArgs eventArgs = null;
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumTeamGet2Messages()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            List<MessageReceivedEventArgs> eventArgsList = new List<MessageReceivedEventArgs>();
            target.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));
            Observer member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            Message message1 = eventArgsList[0].Message;
            Message message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterGetMemberDisconnectedMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            TestHelper.ClearMessages(master);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterGetMessageWithMember()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            TestHelper.ClearMessages(master);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Message message = master.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterGet2Messages()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            Message message1 = master.Messages.First();
            Message message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void Disconnect_AsMember_ScrumMasterMessageReceived()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Disconnect_AsMember_MemberGetsEmptyMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.Empty, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverGetMemberDisconnectedMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            Observer observer = target.Join("observer", true);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverGetMessageWithMember()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            Observer observer = target.Join("observer", true);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Message message = observer.PopMessage();
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverMessageReceived()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            Observer observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_MemberGetsEmptyMessage()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);
            Observer observer = target.Join("observer", true);
            TestHelper.ClearMessages(member);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.Empty, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void Disconnect_AsMemberWhenObserverExists_ObserverGet2Messages()
        {
            // Arrange
            ScrumTeam target = new ScrumTeam("test team");
            ScrumMaster master = target.SetScrumMaster("master");
            Observer observer = target.Join("observer", true);
            Observer member = target.Join("member", false);

            // Act
            target.Disconnect(member.Name);

            // Verify
            Assert.AreEqual<int>(2, observer.Messages.Count());
            Message message1 = observer.Messages.First();
            Message message2 = observer.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void FindMemberOrObserver_ObserverExists_ReturnsObserver()
        {
            // Arrange
            string name = "observer2";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join("observer1", true);
            target.Join("observer2", true);
            target.Join("member1", false);
            target.Join("member2", false);

            // Act
            Observer result = target.FindMemberOrObserver(name);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void FindMemberOrObserver_MemberExists_ReturnsMember()
        {
            // Arrange
            string name = "member2";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join("observer1", true);
            target.Join("observer2", true);
            target.Join("member1", false);
            target.Join("member2", false);

            // Act
            Observer result = target.FindMemberOrObserver(name);

            // Verify
            Assert.IsNotNull(result);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        public void FindMemberOrObserver_MemberNorObserverExists_ReturnsNull()
        {
            // Arrange
            string name = "test";
            ScrumTeam target = new ScrumTeam("test team");
            target.Join("observer1", true);
            target.Join("observer2", true);
            target.Join("member1", false);
            target.Join("member2", false);

            // Act
            Observer result = target.FindMemberOrObserver(name);

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveMembers_TeamIsUnchanged()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            string name = "test";
            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, false);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(1, target.Members.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveMember_MemberIsDisconnected()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            string name = "test";
            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, false);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(0, target.Members.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveObservers_TeamIsUnchanged()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            string name = "test";
            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, true);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(1, target.Observers.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveObserver_ObserverIsDisconnected()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            string name = "test";
            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            target.Join(name, true);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<int>(0, target.Observers.Count());
        }

        [TestMethod]
        public void DisconnectInactiveObservers_ActiveMemberAndInactiveObserver_MemberMessageReceived()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            ScrumMaster master = target.SetScrumMaster("master");
            Observer observer = target.Join("observer", true);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30));
            Observer member = target.Join("member", false);
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
        }

        [TestMethod]
        public void DisconnectInactiveObservers_ActiveObserverAndInactiveMember_ObserverMessageReceived()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            ScrumMaster master = target.SetScrumMaster("master");
            Observer member = target.Join("member", false);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30));
            Observer observer = target.Join("observer", true);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.IsNotNull(eventArgs);
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, message.MessageType);
        }

        [TestMethod]
        public void DisconnectInactiveObservers_EstimateStartedActiveScrumMasterInactiveMember_ScrumMasterGetsEstimateResult()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            ScrumTeam target = new ScrumTeam("test team", dateTimeProvider);
            ScrumMaster master = target.SetScrumMaster("master");
            Member member = (Member)target.Join("member", false);
            master.StartEstimate();

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 30));
            master.Estimate = new Estimate();
            master.UpdateActivity();

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));
            TestHelper.ClearMessages(master);

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateFinished, target.State);
            Assert.IsNotNull(target.EstimateResult);

            Assert.IsTrue(master.HasMessage);
            master.PopMessage();
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.EstimateEnded, message.MessageType);
        }
    }
}
