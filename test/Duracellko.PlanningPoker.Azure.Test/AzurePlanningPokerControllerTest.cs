using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    public class AzurePlanningPokerControllerTest
    {
        [TestMethod]
        public void ObservableMessages_TeamCreated_ScrumTeamCreatedMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            target.CreateScrumTeam("test", "master");
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.TeamCreated, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
        }

        [TestMethod]
        public void ObservableMessages_MemberJoined_ScrumTeamMemberMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();
            IScrumTeamLock teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.Join("member", false);
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberMessage));
            ScrumTeamMemberMessage memberMessage = (ScrumTeamMemberMessage)messages[0];
            Assert.AreEqual<string>("member", memberMessage.MemberName);
            Assert.AreEqual<string>("Member", memberMessage.MemberType);
        }

        [TestMethod]
        public void ObservableMessages_MemberDisconnected_ScrumTeamMemberMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();
            IScrumTeamLock teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.Disconnect("master");
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberDisconnected, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberMessage));
            ScrumTeamMemberMessage memberMessage = (ScrumTeamMemberMessage)messages[0];
            Assert.AreEqual<string>("master", memberMessage.MemberName);
            Assert.AreEqual<string>("ScrumMaster", memberMessage.MemberType);
        }

        [TestMethod]
        public void ObservableMessages_MemberUpdateActivity_ScrumTeamMemberMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();
            IScrumTeamLock teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.UpdateActivity();
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberActivity, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberMessage));
            ScrumTeamMemberMessage memberMessage = (ScrumTeamMemberMessage)messages[0];
            Assert.AreEqual<string>("master", memberMessage.MemberName);
            Assert.AreEqual<string>("ScrumMaster", memberMessage.MemberType);
        }

        [TestMethod]
        public void ObservableMessages_EstimateStarted_ScrumTeamMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();
            IScrumTeamLock teamLock = target.CreateScrumTeam("test", "master");

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.StartEstimate();
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
        }

        [TestMethod]
        public void ObservableMessages_EstimateCanceled_ScrumTeamMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();
            IScrumTeamLock teamLock = target.CreateScrumTeam("test", "master");
            teamLock.Team.ScrumMaster.StartEstimate();

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.CancelEstimate();
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.EstimateCanceled, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
        }

        [TestMethod]
        public void ObservableMessages_MemberEstimated_ScrumTeamMemberMessage()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            List<ScrumTeamMessage> messages = new List<ScrumTeamMessage>();
            IScrumTeamLock teamLock = target.CreateScrumTeam("test", "master");
            teamLock.Team.ScrumMaster.StartEstimate();

            // Act
            target.ObservableMessages.Subscribe(m => messages.Add(m));
            teamLock.Team.ScrumMaster.Estimate = new Estimate(3.0);
            target.Dispose();

            // Verify
            Assert.AreEqual<int>(2, messages.Count);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, messages[0].MessageType);
            Assert.AreEqual<string>("test", messages[0].TeamName);
            Assert.IsInstanceOfType(messages[0], typeof(ScrumTeamMemberEstimateMessage));
            ScrumTeamMemberEstimateMessage memberMessage = (ScrumTeamMemberEstimateMessage)messages[0];
            Assert.AreEqual<string>("master", memberMessage.MemberName);
            Assert.AreEqual<double?>(3.0, memberMessage.Estimate);
        }

        [TestMethod]
        public void CreateScrumteam_AfterInitialization_CreatesNewTeam()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();

            // Act
            IScrumTeamLock result = target.CreateScrumTeam("test", "master");

            // Verify
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Team);
            Assert.AreEqual<string>("test", result.Team.Name);
            Assert.AreEqual<string>("master", result.Team.ScrumMaster.Name);
        }

        [TestMethod]
        public void CreateScrumteam_InitializationTeamListIsNotSet_WaitForInitializationTeamList()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();

            // Act
            Task<IScrumTeamLock> task = Task.Factory.StartNew<IScrumTeamLock>(() => target.CreateScrumTeam("test", "master"), default(CancellationToken), TaskCreationOptions.None, TaskScheduler.Default);
            Assert.IsFalse(task.IsCompleted);
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            target.SetTeamsInitializingList(Enumerable.Empty<string>());
            Assert.IsTrue(task.Wait(1000));

            // Verify
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateScrumteam_TeamNameIsInInitializationTeamList_ArgumentException()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.SetTeamsInitializingList(new string[] { "test" });

            // Act
            target.CreateScrumTeam("test", "master");
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void CreateScrumteam_InitializationTimeout_Exception()
        {
            // Arrange
            AzurePlanningPokerConfiguration configuration = new AzurePlanningPokerConfiguration() { InitializationTimeout = 1 };
            AzurePlanningPokerController target = CreateAzurePlanningPokerController(configuration: configuration);

            // Act
            target.CreateScrumTeam("test", "master");
        }

        [TestMethod]
        public void GetScrumTeam_AfterInitialization_GetsExistingTeam()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.EndInitialization();
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("test team", "master"))
            {
                team = teamLock.Team;
            }

            // Act
            IScrumTeamLock result = target.GetScrumTeam("test team");

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
        }

        [TestMethod]
        public void GetScrumTeam_TeamIsNotInitialized_WaitForTeamInitialization()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.SetTeamsInitializingList(new string[] { "test team", "team2" });

            // Act
            Task<IScrumTeamLock> task = Task.Factory.StartNew<IScrumTeamLock>(() => target.GetScrumTeam("test team"), default(CancellationToken), TaskCreationOptions.None, TaskScheduler.Default);
            Assert.IsFalse(task.IsCompleted);
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            target.InitializeScrumTeam(new ScrumTeam("test team"));
            Assert.IsTrue(task.Wait(1000));

            // Verify
            Assert.IsNotNull(task.Result);
        }

        [TestMethod]
        public void GetScrumTeam_TeamIsNotWaitingForInitialization_ReturnsTeam()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            target.SetTeamsInitializingList(new string[] { "test team", "team2" });
            target.InitializeScrumTeam(new ScrumTeam("test team"));

            // Act
            IScrumTeamLock result = target.GetScrumTeam("test team");

            // Verify
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_InitializationTimeout_ArgumentException()
        {
            // Arrange
            AzurePlanningPokerConfiguration configuration = new AzurePlanningPokerConfiguration() { InitializationTimeout = 1 };
            AzurePlanningPokerController target = CreateAzurePlanningPokerController(configuration: configuration);

            // Act
            target.GetScrumTeam("test team");
        }

        [TestMethod]
        public void SetTeamsInitializingList_TeamSpeacified_DeleteAllFromRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.DeleteAll());
            AzurePlanningPokerController target = CreateAzurePlanningPokerController(repository: repository.Object);

            // Act
            target.SetTeamsInitializingList(new string[] { "team" });

            // Verify
            repository.Verify(r => r.DeleteAll());
        }

        [TestMethod]
        public void SetTeamsInitializingList_AfterEndInitialization_NotDeleteAnythingFromRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            AzurePlanningPokerController target = CreateAzurePlanningPokerController(repository: repository.Object);
            target.EndInitialization();

            // Act
            target.SetTeamsInitializingList(new string[] { "team" });

            // Verify
            repository.Verify(r => r.DeleteAll(), Times.Never());
        }

        [TestMethod]
        public void InitializeScrumTeam_TeamSpeacified_TeamAddedToController()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            ScrumTeam team = new ScrumTeam("team");
            target.SetTeamsInitializingList(new string[] { "team" });

            // Act
            target.InitializeScrumTeam(team);

            // Verify
            IScrumTeamLock result = target.GetScrumTeam("team");
            Assert.AreEqual<ScrumTeam>(team, result.Team);
        }

        [TestMethod]
        public void InitializeScrumTeam_TeamSpecified_TeamCreatedMessageIsNotSent()
        {
            // Arrange
            AzurePlanningPokerController target = CreateAzurePlanningPokerController();
            ScrumTeam team = new ScrumTeam("team");
            target.SetTeamsInitializingList(new string[] { "team" });
            ScrumTeamMessage message = null;
            target.ObservableMessages.Subscribe(m => message = m);

            // Act
            target.InitializeScrumTeam(team);

            // Verify
            Assert.IsNull(message);
        }

        private static AzurePlanningPokerController CreateAzurePlanningPokerController(
            DateTimeProvider dateTimeProvider = null,
            IAzurePlanningPokerConfiguration configuration = null,
            IScrumTeamRepository repository = null,
            ILogger<Controllers.PlanningPokerController> logger = null)
        {
            if (logger == null)
            {
                logger = Mock.Of<ILogger<Controllers.PlanningPokerController>>();
            }

            return new AzurePlanningPokerController(dateTimeProvider, configuration, repository, logger);
        }
    }
}
