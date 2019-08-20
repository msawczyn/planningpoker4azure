using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ScrumMasterTest
    {
        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            string name = "test";

            // Act
            ScrumMaster result = new ScrumMaster(team, name);

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
            ScrumMaster result = new ScrumMaster(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");

            // Act
            ScrumMaster result = new ScrumMaster(team, string.Empty);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_StateChangedToEstimateInProgress()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");

            // Act
            master.StartEstimate();

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateInProgress, team.State);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_ScrumTeamGotMessageEstimateStarted()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message.MessageType);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_ScrumMasterGotMessageEstimateStarted()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_ScrumMasterReceivedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_MemberGotMessageEstimateStarted()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void StartEstimate_MemberHasEstimate_MembersEstimateIsReset()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            member.Estimate = new Estimate();

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNull(member.Estimate);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_MemberReceivedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_ObserverGotMessageEstimateStarted()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer observer = team.Join("observer", true);

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_ObserverReceivedMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer observer = team.Join("observer", false);
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartEstimate_EstimateInProgress_InvalidOperationException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();

            // Act
            master.StartEstimate();
        }

        [TestMethod]
        public void StartEstimate_EstimateNotStarted_EstimateResultSetToNull()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNull(team.EstimateResult);
        }

        [TestMethod]
        public void StartEstimate_EstimateFinished_EstimateResultSetToNull()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();
            master.Estimate = new Estimate();

            // Act
            master.StartEstimate();

            // Verify
            Assert.IsNull(team.EstimateResult);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_StateChangedToEstimateCanceled()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();

            // Act
            master.CancelEstimate();

            // Verify
            Assert.AreEqual<TeamState>(TeamState.EstimateCanceled, team.State);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ScrumTeamGetMessageEstimateCanceled()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
            Message message = eventArgs.Message;
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message.MessageType);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ScrumTeamGet2Messages()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            List<MessageReceivedEventArgs> eventArgsList = new List<MessageReceivedEventArgs>();
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgsList.Add(e));
            master.StartEstimate();

            // Act
            master.CancelEstimate();

            // Verify
            Assert.AreEqual<int>(2, eventArgsList.Count);
            Message message1 = eventArgsList[0].Message;
            Message message2 = eventArgsList[1].Message;
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message1.MessageType);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message2.MessageType);
        }

        [TestMethod]
        public void CancelEstimate_EstimateNotStarted_ScrumTeamGetNoMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            MessageReceivedEventArgs eventArgs = null;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventArgs = e);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ScrumMasterGetMessageEstimateCanceled()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();
            TestHelper.ClearMessages(master);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsTrue(master.HasMessage);
            Message message = master.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message.MessageType);
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ScrumMasterGet2Messages()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();

            // Act
            master.CancelEstimate();

            // Verify
            Assert.AreEqual<int>(2, master.Messages.Count());
            Message message1 = master.Messages.First();
            Message message2 = master.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ScrumMasterMessageReceived()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.StartEstimate();
            EventArgs eventArgs = null;
            master.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimate_EstimateNotStarted_ScrumMasterGetNoMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsFalse(master.HasMessage);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_MemberGetMessageEstimateCanceled()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);
            master.StartEstimate();
            TestHelper.ClearMessages(member);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsTrue(member.HasMessage);
            Message message = member.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message.MessageType);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_MemberGet2Messages()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);
            master.StartEstimate();

            // Act
            master.CancelEstimate();

            // Verify
            Assert.AreEqual<int>(2, member.Messages.Count());
            Message message1 = member.Messages.First();
            Message message2 = member.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_MemberMessageReceived()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);
            master.StartEstimate();
            EventArgs eventArgs = null;
            member.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimate_EstimateNotStarted_MemberGetNoMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ObserverGetMessageEstimateCanceled()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer observer = team.Join("observer", true);
            master.StartEstimate();
            TestHelper.ClearMessages(observer);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsTrue(observer.HasMessage);
            Message message = observer.PopMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message.MessageType);
            Assert.IsFalse(observer.HasMessage);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ObserverGet2Message()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer observer = team.Join("observer", true);
            master.StartEstimate();

            // Act
            master.CancelEstimate();

            // Verify
            Assert.AreEqual<int>(2, observer.Messages.Count());
            Message message1 = observer.Messages.First();
            Message message2 = observer.Messages.Skip(1).First();
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, message1.MessageType);
            Assert.AreEqual<long>(1, message1.Id);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, message2.MessageType);
            Assert.AreEqual<long>(2, message2.Id);
        }

        [TestMethod]
        public void CancelEstimate_EstimateInProgress_ObserverMessageReceived()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer observer = team.Join("observer", true);
            master.StartEstimate();
            EventArgs eventArgs = null;
            observer.MessageReceived += new EventHandler((s, e) => eventArgs = e);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void CancelEstimate_EstimateNotStarted_ObserverGetNoMessage()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer observer = team.Join("observer", true);

            // Act
            master.CancelEstimate();

            // Verify
            Assert.IsFalse(observer.HasMessage);
        }
    }
}
