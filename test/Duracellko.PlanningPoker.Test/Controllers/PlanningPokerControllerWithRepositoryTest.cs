using System;
using System.Collections.Generic;
using System.Linq;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Test.Controllers
{
    [TestClass]
    public class PlanningPokerControllerWithRepositoryTest
    {
        [TestMethod]
        public void ScrumTeamNames_2TeamsInRepository_Returns2Teams()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.SetupGet(r => r.ScrumTeamNames).Returns(new string[] { "team1", "team2" });
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            IEnumerable<string> result = target.ScrumTeamNames;

            // Verify
            repository.VerifyGet(r => r.ScrumTeamNames);
            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(new string[] { "team1", "team2" }, result.ToList());
        }

        [TestMethod]
        public void ScrumTeamNames_2TeamsInRepositoryAnd2TeamCreated_Returns2Teams()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.SetupGet(r => r.ScrumTeamNames).Returns(new string[] { "team1", "team2" });
            repository.Setup(r => r.LoadScrumTeam("team1")).Returns((ScrumTeam)null);
            repository.Setup(r => r.LoadScrumTeam("team3")).Returns((ScrumTeam)null);
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);
            using (target.CreateScrumTeam("team1", "master"))
            {
            }

            using (target.CreateScrumTeam("team3", "master"))
            {
            }

            // Act
            IEnumerable<string> result = target.ScrumTeamNames;

            // Verify
            repository.VerifyGet(r => r.ScrumTeamNames);
            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(new string[] { "team1", "team2", "team3" }, result.ToList());
        }

        [TestMethod]
        public void ScrumTeamNames_AllEmpty_ReturnsZeroTeams()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.SetupGet(r => r.ScrumTeamNames).Returns(Enumerable.Empty<string>());
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            IEnumerable<string> result = target.ScrumTeamNames;

            // Verify
            repository.VerifyGet(r => r.ScrumTeamNames);
            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(Array.Empty<string>(), result.ToList());
        }

        [TestMethod]
        public void CreateScrumTeam_TeamNotInRepository_TriedToLoadFromRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.IsNotNull(teamLock);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateScrumTeam_TeamInRepository_DoesNotCreateNewTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            target.CreateScrumTeam("team", "master");
        }

        [TestMethod]
        public void CreateScrumTeam_TeamInRepository_DoesNotDeleteOldTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            try
            {
                target.CreateScrumTeam("team", "master");
            }
            catch (ArgumentException)
            {
                // expected exception when adding same team
            }

            // Verify
            repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
            repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
        }

        [TestMethod]
        public void CreateScrumTeam_EmptyTeamInRepository_CreatesNewTeamAndDeletesOldOne()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("team");
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
            }
        }

        [TestMethod]
        public void CreateScrumTeam_ExpiredTeamInRepository_CreatesNewTeamAndDeletesOldOne()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team", timeProvider);
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
            }
        }

        [TestMethod]
        public void CreateScrumTeam_TeamAlreadyLoaded_NotLoadingAgain()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            using (target.CreateScrumTeam("team", "master"))
            {
            }

            // Act
            try
            {
                target.CreateScrumTeam("team", "master");
            }
            catch (ArgumentException)
            {
                // expected exception when adding same team
            }

            // Verify
            repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateScrumTeam_TeamCreatedWhileLoading_DoesNotCreateNewTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            bool firstLoad = true;
            bool firstReturn = true;
            PlanningPokerController target = null;

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team"))
                .Callback<string>(n =>
                {
                    if (firstLoad)
                    {
                        firstLoad = false;
                        try
                        {
                            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
                            {
                                Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
                            }
                        }
                        catch (ArgumentException)
                        {
                            // if ArgumentException is here, test should fail
                        }
                    }
                }).Returns<string>(n =>
                {
                    if (firstReturn)
                    {
                        firstReturn = false;
                        return null;
                    }
                    else
                    {
                        return team;
                    }
                });

            target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            target.CreateScrumTeam("team", "master");
        }

        [TestMethod]
        public void AttachScrumTeam_TeamNotInRepository_TriedToLoadFromRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);

            ScrumTeam team = new ScrumTeam("team");
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.AttachScrumTeam(team))
            {
                // Verify
                Assert.IsNotNull(teamLock);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AttachScrumTeam_TeamInRepository_DoesNotCreateNewTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            ScrumTeam inputTeam = new ScrumTeam("team");

            // Act
            target.AttachScrumTeam(inputTeam);
        }

        [TestMethod]
        public void AttachScrumTeam_TeamInRepository_DoesNotDeleteOldTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            ScrumTeam inputTeam = new ScrumTeam("team");

            // Act
            try
            {
                target.AttachScrumTeam(inputTeam);
            }
            catch (ArgumentException)
            {
                // expected exception when adding same team
            }

            // Verify
            repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
            repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
        }

        [TestMethod]
        public void GetScrumTeam_TeamInRepository_ReturnsTeamFromRepository()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                // Verify
                Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_TeamNotInRepository_ReturnsTeamFromRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);

            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_EmptyTeamInRepository_ThrowsException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("team");
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        public void GetScrumTeam_EmptyTeamInRepository_DeletesOldTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("team");
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            try
            {
                target.GetScrumTeam("team");
            }
            catch (ArgumentException)
            {
                // expected exception
            }

            // Verify
            repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
            repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_ExpiredTeamInRepository_ThrowsException()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team", timeProvider);
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        public void GetScrumTeam_ExpiredTeamInRepository_DeletesOldTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team", timeProvider);
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            try
            {
                target.GetScrumTeam("team");
            }
            catch (ArgumentException)
            {
                // expected exception
            }

            // Verify
            repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
            repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
        }

        [TestMethod]
        public void GetScrumTeam_TeamAlreadyLoaded_NotLoadingAgain()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (target.GetScrumTeam("team"))
            {
            }

            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                // Verify
                Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
            }
        }

        [TestMethod]
        public void GetScrumTeam_TeamCreatedWhileLoading_DoesNotCreateNewTeam()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            bool firstLoad = true;
            bool firstReturn = true;
            PlanningPokerController target = null;
            ScrumTeam createdTeam = null;

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team"))
                .Callback<string>(n =>
                {
                    if (firstLoad)
                    {
                        firstLoad = false;
                        using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
                        {
                            createdTeam = teamLock.Team;
                        }
                    }
                }).Returns<string>(n =>
                {
                    if (firstReturn)
                    {
                        firstReturn = false;
                        return null;
                    }
                    else
                    {
                        return team;
                    }
                });

            target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                // Verify
                Assert.AreNotEqual<ScrumTeam>(team, createdTeam);
                Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
                Assert.AreEqual<ScrumTeam>(createdTeam, teamLock.Team);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Exactly(2));
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
            }
        }

        [TestMethod]
        public void GetScrumTeam_DisconnectAfterwards_TeamIsRemovedFromRepository()
        {
            // Arrange
            DateTimeProviderMock timeProvider = new DateTimeProviderMock();
            Mock<IPlanningPokerConfiguration> configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            ScrumTeam team = new ScrumTeam("team");
            ScrumMaster master = team.SetScrumMaster("master");
            master.UpdateActivity();

            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            repository.SetupGet(r => r.ScrumTeamNames).Returns(Enumerable.Empty<string>());

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            PlanningPokerController target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.GetScrumTeam("team"))
            {
                teamLock.Team.Disconnect(master.Name);
                IEnumerable<string> result = target.ScrumTeamNames;

                // Verify
                Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
                Assert.IsFalse(result.Any());
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
            }
        }

        [TestMethod]
        public void PlanningPokerController_ObserverUpdateActivity_ScrumTeamSavedToRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                teamLock.Team.ScrumMaster.UpdateActivity();

                // Verify
                repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
            }
        }

        [TestMethod]
        public void PlanningPokerController_JoinMember_ScrumTeamSavedToRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                teamLock.Team.Join("member", false);

                // Verify
                repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
            }
        }

        [TestMethod]
        public void PlanningPokerController_StartEstimate_ScrumTeamSavedToRepository()
        {
            // Arrange
            Mock<IScrumTeamRepository> repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
            PlanningPokerController target = CreatePlanningPokerController(repository: repository.Object);

            // Act
            using (IScrumTeamLock teamLock = target.CreateScrumTeam("team", "master"))
            {
                teamLock.Team.ScrumMaster.StartEstimate();

                // Verify
                repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
            }
        }

        private static PlanningPokerController CreatePlanningPokerController(
            DateTimeProvider dateTimeProvider = null,
            IPlanningPokerConfiguration configuration = null,
            IScrumTeamRepository repository = null,
            ILogger<PlanningPokerController> logger = null)
        {
            if (logger == null)
            {
                logger = Mock.Of<ILogger<PlanningPokerController>>();
            }

            return new PlanningPokerController(dateTimeProvider, configuration, repository, logger);
        }
    }
}
