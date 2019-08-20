using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class MemberTest
    {
        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            string name = "test";

            // Act
            Member result = new Member(team, name);

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNotSpecified_ArgumentNullException()
        {
            // Arrange
            string name = "test";

            // Act
            Member result = new Member(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");

            // Act
            Member result = new Member(team, string.Empty);
        }

        [TestMethod]
        public void Estimate_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Member target = new Member(team, "test");

            // Act
            Estimate result = target.Estimate;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Estimate_SetAndGet_ReturnsTheValue()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Estimate estimate = new Estimate();
            Member target = new Member(team, "test");

            // Act
            target.Estimate = estimate;
            Estimate result = target.Estimate;

            // Verify
            Assert.AreEqual<Estimate>(estimate, result);
        }

        [TestMethod]
        public void Estimate_SetTwiceAndGet_ReturnsTheValue()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Estimate estimate = new Estimate();
            Member target = new Member(team, "test");

            // Act
            target.Estimate = estimate;
            target.Estimate = estimate;
            Estimate result = target.Estimate;

            // Verify
            Assert.AreEqual<Estimate>(estimate, result);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_StateChangedToEstimateFinished()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateFinished, team.State);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_EstimateResultIsGenerated()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(team.EstimateResult);
            KeyValuePair<Member, Estimate>[] expectedResult = new KeyValuePair<Member, Estimate>[]
            {
                new KeyValuePair<Member, Estimate>(master, masterEstimate),
                new KeyValuePair<Member, Estimate>(member, memberEstimate),
            };
            CollectionAssert.AreEquivalent(expectedResult, team.EstimateResult.ToList());
        }

        [TestMethod]
        public void Estimate_SetOnMemberOnly_StateIsEstimateInProgress()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateInProgress, team.State);
        }

        [TestMethod]
        public void Estimate_SetOnMemberOnly_EstimateResultIsNull()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            Estimate memberEstimate = new Estimate();

            // Act
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNull(team.EstimateResult);
        }

        [TestMethod]
        public void Estimate_SetTwiceToDifferentValues_InvalidOperationException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Estimate estimation1 = new Estimate();
            Estimate estimation2 = new Estimate();
            Member target = new Member(team, "test");
            target.Estimate = estimation1;

            // Act
            target.Estimate = estimation2;
        }

        [TestMethod]
        public void Estimate_SetOnMemberOnly_ScrumTeamGetMemberEstimatedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            Estimate memberEstimate = new Estimate();

            // Act
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_ScrumTeamGetEstimateEndedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateEnded, message.MessageType);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_EstimateResultIsGeneratedForScrumTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsInstanceOfType(message, typeof(EstimateResultMessage));
            EstimateResultMessage estimationResultMessage = (EstimateResultMessage)message;
            Assert.AreEqual<EstimateResult>(team.EstimateResult, estimationResultMessage.EstimateResult);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_ScrumMasterGetEstimateEndedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(master);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsTrue(master.HasMessage);
            master.PopMessage();
            Assert.IsTrue(master.HasMessage);
            master.PopMessage();
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateEnded, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_EstimateResultIsGeneratedForScrumMaster()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(master);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            master.PopMessage();
            master.PopMessage();
            Message message = master.PopMessage();
            Assert.IsInstanceOfType(message, typeof(EstimateResultMessage));
            EstimateResultMessage estimationResultMessage = (EstimateResultMessage)message;
            Assert.AreEqual<EstimateResult>(team.EstimateResult, estimationResultMessage.EstimateResult);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_ScrumMasterMessageReceived()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(master);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Estimate_SetOnMemberOnly_ScrumMasterGetsMemberEstimatedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(master);
            Estimate memberEstimate = new Estimate();

            // Act
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_MemberGetEstimateEndedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            TestHelper.ClearMessages(member);
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsTrue(member.HasMessage);
            member.PopMessage();
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateEnded, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_EstimateResultIsGeneratedForMember()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            TestHelper.ClearMessages(member);
            member.Estimate = memberEstimate;

            // Verify
            member.PopMessage();
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.IsInstanceOfType(message, typeof(EstimateResultMessage));
            EstimateResultMessage estimationResultMessage = (EstimateResultMessage)message;
            Assert.AreEqual<EstimateResult>(team.EstimateResult, estimationResultMessage.EstimateResult);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_MemberMessageReceived()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(member);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Estimate_SetOnMemberOnly_MemberGetsMemberEstimatedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(member);
            Estimate memberEstimate = new Estimate();

            // Act
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_ObserverGetEstimateEndedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            Observer observer = team.Join("observer", true);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            TestHelper.ClearMessages(observer);
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            observer.PopMessage();
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateEnded, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_EstimateResultIsGeneratedForObserver()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            Observer observer = team.Join("observer", true);
            master.StartEstimate();
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();

            // Act
            master.Estimate = masterEstimate;
            TestHelper.ClearMessages(observer);
            member.Estimate = memberEstimate;

            // Verify
            observer.PopMessage();
            Message message = observer.PopMessage();
            Assert.IsInstanceOfType(message, typeof(EstimateResultMessage));
            EstimateResultMessage estimationResultMessage = (EstimateResultMessage)message;
            Assert.AreEqual<EstimateResult>(team.EstimateResult, estimationResultMessage.EstimateResult);
        }

        [TestMethod]
        public void Estimate_SetOnAllMembers_ObserverMessageReceived()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            Observer observer = team.Join("observer", true);
            master.StartEstimate();
            TestHelper.ClearMessages(observer);
            Estimate masterEstimate = new Estimate();
            Estimate memberEstimate = new Estimate();
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.Estimate = masterEstimate;
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void Estimate_SetOnMemberOnly_ObserverGetsMemberEstimatedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            Observer observer = team.Join("observer", true);
            master.StartEstimate();
            TestHelper.ClearMessages(observer);
            Estimate memberEstimate = new Estimate();

            // Act
            member.Estimate = memberEstimate;

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, message.MessageType);
            Assert.IsInstanceOfType(message, typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)message;
            Assert.AreEqual<Observer>(member, memberMessage.Member);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Estimate_SetToNotAvailableValue_ArgumentException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();
            Estimate masterEstimate = new Estimate(44.0);

            // Act
            master.Estimate = masterEstimate;
        }
    }
}
