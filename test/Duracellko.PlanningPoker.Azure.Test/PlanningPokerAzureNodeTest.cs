using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    public class PlanningPokerAzureNodeTest
    {
        private const string TeamName = "test team";
        private const string ScrumMasterName = "master";
        private const string MemberName = "member";
        private const string ObserverName = "observer";

        [TestMethod]
        public void Constructor_PlanningPoker_PlanningPokerIsSet()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);

            // Act
            PlanningPokerAzureNode result = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, null);

            // Verify
            Assert.AreEqual<IAzurePlanningPoker>(planningPoker.Object, result.PlanningPoker);
        }

        [TestMethod]
        public void Constructor_Configuration_ConfigurationIsSet()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            AzurePlanningPokerConfiguration configuration = CreateConfigutartion();

            // Act
            PlanningPokerAzureNode result = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, configuration);

            // Verify
            Assert.AreEqual<IAzurePlanningPokerConfiguration>(configuration, result.Configuration);
        }

        [TestMethod]
        public void Constructor_NoConfiguration_DefaultConfigurationIsSet()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);

            // Act
            PlanningPokerAzureNode result = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, null);

            // Verify
            Assert.IsNotNull(result.Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_PlanningPokerIsNull_ArgumentNullException()
        {
            // Arrange
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);

            // Act
            PlanningPokerAzureNode result = CreatePlanningPokerAzureNode(null, serviceBus.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ServiceBusIsNull_ArgumentNullException()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);

            // Act
            PlanningPokerAzureNode result = CreatePlanningPokerAzureNode(planningPoker.Object, null, null);
        }

        [TestMethod]
        public async Task Start_TeamCreatedMessage_MessageIsSentToServiceBus()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());
            ScrumTeam team = CreateBasicTeam();
            Mock<IScrumTeamLock> teamLock = CreateTeamLock(team);
            ScrumTeamMessage message = new ScrumTeamMessage(TeamName, MessageType.TeamCreated);
            Action startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();

            Action sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId, null);
            NodeMessage nodeMessage = null;
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.TeamCreated)))
                .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

            // Act
            await target.Start();
            sendServiceBusMsg();
            startPlanningPokerMsg();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Assert.IsNotNull(nodeMessage);
            Assert.AreEqual<NodeMessageType>(NodeMessageType.TeamCreated, nodeMessage.MessageType);
            Assert.AreEqual<string>(target.NodeId, nodeMessage.SenderNodeId);
            Assert.IsNotNull(nodeMessage.Data);
            Assert.IsInstanceOfType(nodeMessage.Data, typeof(byte[]));
        }

        [TestMethod]
        public async Task Start_MemberJoined_MessageIsSentToServiceBus()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());
            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined) { MemberName = MemberName };
            Action startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);

            Action sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId, null);
            NodeMessage nodeMessage = null;
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.ScrumTeamMessage)))
                .Callback<NodeMessage>(m => nodeMessage = m).Returns(Task.CompletedTask).Verifiable();

            // Act
            await target.Start();
            sendServiceBusMsg();
            startPlanningPokerMsg();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(nodeMessage);
            Assert.AreEqual<NodeMessageType>(NodeMessageType.ScrumTeamMessage, nodeMessage.MessageType);
            Assert.AreEqual<string>(target.NodeId, nodeMessage.SenderNodeId);
            Assert.AreEqual(message, nodeMessage.Data);
        }

        [TestMethod]
        public async Task Start_EstimateEnded_NoMessageIsSentToServiceBus()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());
            ScrumTeamMessage message = new ScrumTeamMessage(TeamName, MessageType.EstimateEnded);
            Action startPlanningPokerMsg = SetupPlanningPokerMsg(planningPoker, message);

            Action sendServiceBusMsg = SetupServiceBus(serviceBus, target.NodeId, null);

            // Act
            await target.Start();
            sendServiceBusMsg();
            startPlanningPokerMsg();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            serviceBus.Verify(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType != NodeMessageType.RequestTeamList)), Times.Never());
        }

        [TestMethod]
        public async Task Start_MemberJoinedFromServiceBus_MemberJoinedTeam()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
            {
                MemberName = MemberName,
                MemberType = "Member"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            ScrumTeam team = CreateBasicTeam();
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Observer observer = team.FindMemberOrObserver(MemberName);
            Assert.IsNotNull(observer);
            Assert.IsInstanceOfType(observer, typeof(Member));
            Assert.AreEqual<string>(MemberName, observer.Name);
        }

        [TestMethod]
        public async Task Start_NotInitAndMemberJoinedFromServiceBus_MessageIgnored()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
            {
                MemberName = MemberName,
                MemberType = "Member"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_ObserverJoinedFromServiceBus_ObserverJoinedTeam()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
            {
                MemberName = ObserverName,
                MemberType = "Observer"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            ScrumTeam team = CreateBasicTeam();
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Observer observer = team.FindMemberOrObserver(ObserverName);
            Assert.IsNotNull(observer);
            Assert.IsInstanceOfType(observer, typeof(Observer));
            Assert.AreEqual<string>(ObserverName, observer.Name);
        }

        [TestMethod]
        public async Task Start_MasterDisconnectedFromServiceBus_MasterDisconnectedFromTeam()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberDisconnected)
            {
                MemberName = ScrumMasterName,
                MemberType = "ScrumMaster"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            ScrumTeam team = CreateBasicTeam();
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Assert.IsNull(team.ScrumMaster);
        }

        [TestMethod]
        public async Task Start_NotInitAndMasterDisconnectedFromServiceBus_MessageIgnored()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberDisconnected)
            {
                MemberName = ScrumMasterName,
                MemberType = "ScrumMaster"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_EstimateStartedFromServiceBus_TeamEstimateStarted()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMessage message = new ScrumTeamMessage(TeamName, MessageType.EstimateStarted);
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            ScrumTeam team = CreateBasicTeam();
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Assert.AreEqual<TeamState>(TeamState.EstimateInProgress, team.State);
        }

        [TestMethod]
        public async Task Start_NotInitAndEstimateStartedFromServiceBus_MessageIgnored()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMessage message = new ScrumTeamMessage(TeamName, MessageType.EstimateStarted);
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_EstimateCanceledFromServiceBus_TeamEstimateCanceled()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMessage message = new ScrumTeamMessage(TeamName, MessageType.EstimateCanceled);
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            ScrumTeam team = CreateBasicTeam();
            team.ScrumMaster.StartEstimate();
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Assert.AreEqual<TeamState>(TeamState.EstimateCanceled, team.State);
        }

        [TestMethod]
        public async Task Start_NotInitAndEstimateCanceledFromServiceBus_MessageIgnored()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMessage message = new ScrumTeamMessage(TeamName, MessageType.EstimateCanceled);
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_MasterEstimatedFromServiceBus_MasterEstimateIsSet()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberEstimateMessage message = new ScrumTeamMemberEstimateMessage(TeamName, MessageType.MemberEstimated)
            {
                MemberName = ScrumMasterName,
                Estimate = 5.0
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            ScrumTeam team = CreateBasicTeam();
            team.ScrumMaster.StartEstimate();
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Assert.IsNotNull(team.ScrumMaster.Estimate);
            Assert.AreEqual<double?>(5.0, team.ScrumMaster.Estimate.Value);
        }

        [TestMethod]
        public async Task Start_NotInitAndMasterEstimatedFromServiceBus_MessageIgnored()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberEstimateMessage message = new ScrumTeamMemberEstimateMessage(TeamName, MessageType.MemberEstimated)
            {
                MemberName = ScrumMasterName,
                Estimate = 5.0
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_MasterActivityFromServiceBus_MasterUpdatedActivityInTeam()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberActivity)
            {
                MemberName = ScrumMasterName,
                MemberType = "ScrumMaster"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 9, 9, 23, 27, 33, DateTimeKind.Utc));

            ScrumTeam team = new ScrumTeam(TeamName, dateTimeProvider);
            team.SetScrumMaster(ScrumMasterName);
            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, team);

            dateTimeProvider.SetUtcNow(new DateTime(2012, 9, 9, 23, 28, 27, DateTimeKind.Utc));

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
            Assert.AreEqual<DateTime>(dateTimeProvider.UtcNow, team.ScrumMaster.LastActivity);
        }

        [TestMethod]
        public async Task Start_NotInitAndMasterActivityFromServiceBus_MessageIgnored()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            ScrumTeamMemberMessage message = new ScrumTeamMemberMessage(TeamName, MessageType.MemberActivity)
            {
                MemberName = ScrumMasterName,
                MemberType = "ScrumMaster"
            };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage) { Data = message };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.GetScrumTeam(It.IsAny<string>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_TeamCreatedFromServiceBus_TeamAttachedToPlanningPoker()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.TeamCreated) { Data = CreateSerializedBasicTeam() };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            ScrumTeam team = null;
            planningPoker.Setup(p => p.AttachScrumTeam(It.IsAny<ScrumTeam>()))
                .Callback<ScrumTeam>(t => team = t).Returns(default(IScrumTeamLock)).Verifiable();
            planningPoker.Setup(p => p.DateTimeProvider).Returns(dateTimeProvider).Verifiable();
            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(team);
            Assert.AreEqual<string>(TeamName, team.Name);
            Assert.AreEqual<DateTimeProvider>(dateTimeProvider, team.DateTimeProvider);
        }

        [TestMethod]
        public async Task Start_NotInitAndTeamCreatedFromServiceBus_IgnoreMessage()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.TeamCreated) { Data = CreateSerializedBasicTeam() };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, new string[] { TeamName }, nodeMessage);

            SetupPlanningPoker(planningPoker, null, true);
            planningPoker.Setup(p => p.DateTimeProvider).Returns(new DateTimeProviderMock()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify(p => p.AttachScrumTeam(It.IsAny<ScrumTeam>()), Times.Never());
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_TeamListMessageReceived_SetScrumTeamListOnPlanningPoker()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, null);

            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            IEnumerable<string> initializationTeamList = null;
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()))
                .Callback<IEnumerable<string>>(t => initializationTeamList = t).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(initializationTeamList);
            CollectionAssert.AreEquivalent(teamList, initializationTeamList.ToList());
        }

        [TestMethod]
        public async Task Start_TeamListMessageReceived_RequestForTeams()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, null);
            NodeMessage requestTeamsMessage = null;
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.RequestTeams)))
                .Callback<NodeMessage>(m => requestTeamsMessage = m).Returns(Task.CompletedTask).Verifiable();

            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>())).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(requestTeamsMessage);
            Assert.IsNotNull(requestTeamsMessage.Data);
            string[] requestedTeams = (string[])requestTeamsMessage.Data;
            CollectionAssert.AreEquivalent(teamList, requestedTeams);
        }

        [TestMethod]
        public async Task Start_InitializeTeamMessageReceived_InitializeTeamOnPlanningPoker()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam) { Data = CreateSerializedBasicTeam() };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, nodeMessage);

            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            ScrumTeam initializingTeam = null;
            planningPoker.Setup(p => p.InitializeScrumTeam(It.IsAny<ScrumTeam>())).Callback<ScrumTeam>(t => initializingTeam = t).Verifiable();
            planningPoker.Setup(p => p.EndInitialization()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(initializingTeam);
            Assert.AreEqual<string>(TeamName, initializingTeam.Name);
        }

        [TestMethod]
        public async Task Start_InitializeTeamMessageReceivedButAnotherTeamIsNotInitializedYet_EndInitializationIsNotExecuted()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName, "team 2" };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam) { Data = CreateSerializedBasicTeam() };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, nodeMessage);

            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());
            planningPoker.Setup(p => p.InitializeScrumTeam(It.IsAny<ScrumTeam>())).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            planningPoker.Verify(p => p.EndInitialization(), Times.Never());
        }

        [TestMethod]
        public async Task Start_InitializeTeamMessageReceivedWithTeamNameOnly_SkipsInitializationOfThisTeam()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam) { Data = TeamName };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, teamList, nodeMessage);

            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization()).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_RequestTeamListMessageReceived_TeamListIsObtainedFromPlanningPoker()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList);
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            string[] teamList = new string[] { TeamName };
            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());
            planningPoker.Setup(p => p.ScrumTeamNames).Returns(teamList).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
        }

        [TestMethod]
        public async Task Start_RequestTeamListMessageReceived_TeamListIsSentToServiceBus()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList) { SenderNodeId = "sender" };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);
            NodeMessage teamListMessage = null;
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.TeamList)))
                .Callback<NodeMessage>(m => teamListMessage = m).Returns(Task.CompletedTask).Verifiable();

            string[] teamList = new string[] { TeamName };
            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>());
            planningPoker.Setup(p => p.DateTimeProvider).Returns(DateTimeProvider.Default);
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization());
            planningPoker.Setup(p => p.ScrumTeamNames).Returns(teamList);

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(teamListMessage);
            Assert.IsNotNull(teamListMessage.Data);
            CollectionAssert.AreEquivalent(teamList, (string[])teamListMessage.Data);
            Assert.AreEqual<string>(nodeMessage.SenderNodeId, teamListMessage.RecipientNodeId);
        }

        [TestMethod]
        public async Task Start_RequestTeamsMessageReceived_TeamIsObtainedFromPlanningPoker()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.RequestTeams) { Data = teamList };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);

            Mock<IScrumTeamLock> teamLock = SetupPlanningPoker(planningPoker, CreateBasicTeam());

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            teamLock.Verify();
        }

        [TestMethod]
        public async Task Start_RequestTeamsMessageReceived_TeamIsSentToServiceBus()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.RequestTeams) { Data = teamList };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);
            NodeMessage initializeTeamMessage = null;
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.InitializeTeam)))
                .Callback<NodeMessage>(m => initializeTeamMessage = m).Returns(Task.CompletedTask).Verifiable();

            SetupPlanningPoker(planningPoker, CreateBasicTeam());

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(initializeTeamMessage);
            Assert.IsNotNull(initializeTeamMessage.Data);
            Assert.IsInstanceOfType(initializeTeamMessage.Data, typeof(byte[]));
            Assert.AreEqual<string>(nodeMessage.SenderNodeId, initializeTeamMessage.RecipientNodeId);
        }

        [TestMethod]
        public async Task Start_RequestTeamsMessageReceivedButTeamDoesNotExistAnymore_TeamNameIsSentToServiceBus()
        {
            // Arrange
            Mock<IAzurePlanningPoker> planningPoker = new Mock<IAzurePlanningPoker>(MockBehavior.Strict);
            Mock<IServiceBus> serviceBus = new Mock<IServiceBus>(MockBehavior.Strict);
            PlanningPokerAzureNode target = CreatePlanningPokerAzureNode(planningPoker.Object, serviceBus.Object, CreateConfigutartion());

            string[] teamList = new string[] { TeamName };
            NodeMessage nodeMessage = new NodeMessage(NodeMessageType.RequestTeams) { Data = teamList };
            serviceBus.Setup(b => b.SendMessage(It.IsAny<NodeMessage>())).Returns(Task.CompletedTask);
            Action sendMessages = SetupServiceBus(serviceBus, target.NodeId, nodeMessage);
            NodeMessage initializeTeamMessage = null;
            serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.InitializeTeam)))
                .Callback<NodeMessage>(m => initializeTeamMessage = m).Returns(Task.CompletedTask).Verifiable();

            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            planningPoker.Setup(p => p.EndInitialization()).Verifiable();
            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Throws(new ArgumentException("teamName")).Verifiable();

            // Act
            await target.Start();
            sendMessages();
            await target.Stop();

            // Verify
            planningPoker.Verify();
            serviceBus.Verify();
            Assert.IsNotNull(initializeTeamMessage);
            Assert.IsNotNull(initializeTeamMessage.Data);
            Assert.IsInstanceOfType(initializeTeamMessage.Data, typeof(string));
            Assert.AreEqual<string>(TeamName, (string)initializeTeamMessage.Data);
            Assert.AreEqual<string>(nodeMessage.SenderNodeId, initializeTeamMessage.RecipientNodeId);
        }

        private static PlanningPokerAzureNode CreatePlanningPokerAzureNode(
            IAzurePlanningPoker planningPoker = null,
            IServiceBus serviceBus = null,
            IAzurePlanningPokerConfiguration configuration = null,
            ILogger<PlanningPokerAzureNode> logger = null)
        {
            if (logger == null)
            {
                logger = Mock.Of<ILogger<PlanningPokerAzureNode>>();
            }

            return new PlanningPokerAzureNode(planningPoker, serviceBus, configuration, logger);
        }

        private static ScrumTeam CreateBasicTeam()
        {
            ScrumTeam result = new ScrumTeam(TeamName);
            result.SetScrumMaster(ScrumMasterName);
            return result;
        }

        private static Mock<IScrumTeamLock> CreateTeamLock(ScrumTeam scrumTeam)
        {
            Mock<IScrumTeamLock> result = new Mock<IScrumTeamLock>(MockBehavior.Strict);
            result.Setup(l => l.Team).Returns(scrumTeam);
            result.Setup(l => l.Lock()).Verifiable();
            result.Setup(l => l.Dispose()).Verifiable();
            return result;
        }

        private static Action SetupServiceBus(Mock<IServiceBus> serviceBus, string nodeId, NodeMessage nodeMessage)
        {
            return SetupServiceBus(serviceBus, nodeId, null, nodeMessage);
        }

        private static Action SetupServiceBus(Mock<IServiceBus> serviceBus, string nodeId, string[] initializationTeamList, NodeMessage nodeMessage)
        {
            serviceBus.Setup(b => b.Register(nodeId)).Returns(Task.CompletedTask).Verifiable();
            serviceBus.Setup(b => b.Unregister()).Returns(Task.CompletedTask).Verifiable();

            NodeMessage emptyTeamListMessage = new NodeMessage(NodeMessageType.TeamList)
            {
                Data = initializationTeamList ?? Array.Empty<string>(),
                RecipientNodeId = nodeId
            };

            Subject<NodeMessage> observableMessages = new Subject<NodeMessage>();
            serviceBus.Setup(b => b.ObservableMessages).Returns(observableMessages).Verifiable();
            if (initializationTeamList != null && initializationTeamList.Length != 0)
            {
                serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m =>
                    m.MessageType == NodeMessageType.RequestTeamList || m.MessageType == NodeMessageType.RequestTeams)))
                    .Returns(Task.CompletedTask).Verifiable();
            }
            else
            {
                serviceBus.Setup(b => b.SendMessage(It.Is<NodeMessage>(m => m.MessageType == NodeMessageType.RequestTeamList)))
                    .Returns(Task.CompletedTask).Verifiable();
            }

            return new Action(
                () =>
                {
                    observableMessages.OnNext(emptyTeamListMessage);
                    if (nodeMessage != null)
                    {
                        observableMessages.OnNext(nodeMessage);
                    }

                    observableMessages.OnCompleted();
                });
        }

        private static Mock<IScrumTeamLock> SetupPlanningPoker(Mock<IAzurePlanningPoker> planningPoker, ScrumTeam team, bool noEnd = false)
        {
            planningPoker.Setup(p => p.SetTeamsInitializingList(It.IsAny<IEnumerable<string>>()));
            if (!noEnd)
            {
                planningPoker.Setup(p => p.EndInitialization()).Verifiable();
            }

            planningPoker.Setup(p => p.ObservableMessages).Returns(Observable.Empty<ScrumTeamMessage>()).Verifiable();
            if (team != null)
            {
                Mock<IScrumTeamLock> teamLock = CreateTeamLock(team);
                planningPoker.Setup(p => p.GetScrumTeam(team.Name)).Returns(teamLock.Object).Verifiable();
                return teamLock;
            }
            else
            {
                return null;
            }
        }

        private static Action SetupPlanningPokerMsg(Mock<IAzurePlanningPoker> planningPoker, ScrumTeamMessage message)
        {
            Subject<ScrumTeamMessage> observableMessages = new Subject<ScrumTeamMessage>();
            planningPoker.Setup(p => p.ObservableMessages).Returns(observableMessages).Verifiable();
            return new Action(
                () =>
                {
                    observableMessages.OnNext(message);
                    observableMessages.OnCompleted();
                });
        }

        private static byte[] CreateSerializedBasicTeam()
        {
            ScrumTeam team = CreateBasicTeam();
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, team);
                return stream.ToArray();
            }
        }

        private static AzurePlanningPokerConfiguration CreateConfigutartion()
        {
            return new AzurePlanningPokerConfiguration();
        }
    }
}
