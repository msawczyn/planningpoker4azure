using System;
using System.Collections.Generic;
using System.Linq;

using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Test;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Controllers.Test
{
    [TestClass]
    public class PlanningPokerControllerTest
    {
        [TestMethod]
        public void PlanningPokerController_Create_DefaultDateTimeProvider()
        {
            // Act
            PlanningPokerController result = CreatePlanningPokerController();

            // Verify
            Assert.AreEqual<DateTimeProvider>(DateTimeProvider.Default, result.DateTimeProvider);
        }

        [TestMethod]
        public void PlanningPokerController_SpecificDateTimeProvider_DateTimeProviderIsSet()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();

            // Act
            PlanningPokerController result = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);

            // Verify
            Assert.AreEqual<DateTimeProvider>(dateTimeProvider, result.DateTimeProvider);
        }

        [TestMethod]
        public void PlanningPokerController_Configuration_ConfigurationIsSet()
        {
            // Arrange
            PlanningPokerConfiguration configuration = new Duracellko.PlanningPoker.Configuration.PlanningPokerConfiguration();

            // Act
            PlanningPokerController result = CreatePlanningPokerController(configuration: configuration);

            // Verify
            Assert.AreEqual(configuration, result.Configuration);
        }

        [TestMethod]
        public void ScrumTeamNames_Get_ReturnsListOfTeamNames()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();
            using (IScrumTeamLock teamLock1 = target.CreateScrumTeam("team1", "master1"))
            {
            }

            using (IScrumTeamLock teamLock2 = target.CreateScrumTeam("team2", "master1"))
            {
            }

            // Act
            IEnumerable<string> result = target.ScrumTeamNames;

            // Verify
            string[] expectedCollection = new string[] { "team1", "team2" };
            CollectionAssert.AreEquivalent(expectedCollection, result.ToList());
        }

        [TestMethod]
        public void CreateScrumTeam_TeamName_CreatedTeamWithSpecifiedName()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.IsNotNull(teamLock);
                Assert.IsNotNull(teamLock.Team);
                Assert.AreEqual<string>("team", teamLock.Team.Name);
            }
        }

        [TestMethod]
        public void CreateScrumTeam_ScrumMasterName_CreatedTeamWithSpecifiedScrumMaster()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.IsNotNull(teamLock.Team.ScrumMaster);
                Assert.AreEqual<string>("master", teamLock.Team.ScrumMaster.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateScrumTeam_TeamNameAlreadyExists_ArgumentException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();
            IScrumTeamLock team = target.CreateScrumTeam("team", "master");
            team.Dispose();

            // Act
            target.CreateScrumTeam("team", "master2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScrumTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.CreateScrumTeam(string.Empty, "master");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScrumTeam_ScrumMasterNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.CreateScrumTeam("test team", string.Empty);
        }

        [TestMethod]
        public void CreateScrumTeam_SpecificDateTimeProvider_CreatedTeamWithDateTimeProvider()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            PlanningPokerController target = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.AreEqual<DateTimeProvider>(dateTimeProvider, teamLock.Team.DateTimeProvider);
            }
        }

        [TestMethod]
        public void AttachScrumTeam_ScrumTeam_TeamIsInCollection()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.AttachScrumTeam(team);

            // Verify
            Assert.IsNotNull(target.GetScrumTeam(team.Name));
        }

        [TestMethod]
        public void AttachScrumTeam_ScrumTeam_ReturnsSameTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            IScrumTeamLock result = target.AttachScrumTeam(team);

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AttachScrumTeam_TeamNameAlreadyExists_ArgumentException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();
            IScrumTeamLock existingTeam = target.CreateScrumTeam("team", "master");
            existingTeam.Dispose();
            ScrumTeam team = new ScrumTeam("team");

            // Act
            target.AttachScrumTeam(team);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AttachScrumTeam_Null_ArgumentNullException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.AttachScrumTeam(null);
        }

        [TestMethod]
        public void GetScrumTeam_TeamNameExists_ReturnsExistingTeam()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
            }

            // Act
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                // Verify
                Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_TeamNameNotExists_ArgumentException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetScrumTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.GetScrumTeam(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_AfterDisconnectingAllMembers_ArgumentException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
            }

            team.Disconnect("master");

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMessagesAsync_ObserverIsNull_ArgumentNullException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();

            // Act
            target.GetMessagesAsync(null, (f, o) => { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMessagesAsync_CallbackIsNull_ArgumentNullException()
        {
            // Arrange
            PlanningPokerController target = CreatePlanningPokerController();
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Act
                target.GetMessagesAsync(teamLock.Team.ScrumMaster, null);
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveMembers_TeamIsUnchanged()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            PlanningPokerController target = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("member", false);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(2, teamLock.Team.Members.Count());
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveMember_MemberIsDisconnected()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            PlanningPokerController target = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("member", false);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(1, teamLock.Team.Members.Count());
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveObservers_TeamIsUnchanged()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            PlanningPokerController target = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("observer", true);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(1, teamLock.Team.Observers.Count());
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveObserver_ObserverIsDisconnected()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            PlanningPokerController target = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("observer", true);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(0, teamLock.Team.Observers.Count());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisconnectInactiveObservers_InactiveScrumMaster_TeamIsClosed()
        {
            // Arrange
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            PlanningPokerController target = CreatePlanningPokerController(dateTimeProvider: dateTimeProvider);
            ScrumTeam team;
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            target.GetScrumTeam("team");
        }

        private static PlanningPokerController CreatePlanningPokerController(
            DateTimeProvider dateTimeProvider = null,
            Configuration.IPlanningPokerConfiguration configuration = null,
            Data.IScrumTeamRepository repository = null,
            ILogger<PlanningPokerController> logger = null)
        {
            return new PlanningPokerController(dateTimeProvider, configuration, repository, logger);
        }
    }
}
