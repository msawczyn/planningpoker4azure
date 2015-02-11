﻿// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Test.Controllers
{
    [TestClass]
    public class PlanningPokerControllerWithRepositoryTest
    {
        #region CreateScrumTeam

        [TestMethod]
        public void CreateScrumTeam_TeamNotInRepository_TriedToLoadFromRepository()
        {
            // Arrange
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            target.CreateScrumTeam("team", "master");
        }

        [TestMethod]
        public void CreateScrumTeam_TeamInRepository_DoesNotDeleteOldTeam()
        {
            // Arrange
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

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
            var team = new ScrumTeam("team");
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team", timeProvider);
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
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
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            var target = new PlanningPokerController(null, null, repository.Object);

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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            bool firstLoad = true;
            bool firstReturn = true;
            PlanningPokerController target = null;

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team"))
                .Callback<string>(n =>
                {
                    if (firstLoad)
                    {
                        firstLoad = false;
                        try
                        {
                            using (var teamLock = target.CreateScrumTeam("team", "master"))
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

            target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            target.CreateScrumTeam("team", "master");
        }

        #endregion

        #region AttachScrumTeam

        [TestMethod]
        public void AttachScrumTeam_TeamNotInRepository_TriedToLoadFromRepository()
        {
            // Arrange
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);

            var team = new ScrumTeam("team");
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            using (var teamLock = target.AttachScrumTeam(team))
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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            var inputTeam = new ScrumTeam("team");

            // Act
            target.AttachScrumTeam(inputTeam);
        }

        [TestMethod]
        public void AttachScrumTeam_TeamInRepository_DoesNotDeleteOldTeam()
        {
            // Arrange
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            var inputTeam = new ScrumTeam("team");

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

        #endregion

        #region GetScrumTeam

        [TestMethod]
        public void GetScrumTeam_TeamInRepository_ReturnsTeamFromRepository()
        {
            // Arrange
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (var teamLock = target.GetScrumTeam("team"))
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
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);

            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_EmptyTeamInRepository_ThrowsException()
        {
            // Arrange
            var team = new ScrumTeam("team");
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        public void GetScrumTeam_EmptyTeamInRepository_DeletesOldTeam()
        {
            // Arrange
            var team = new ScrumTeam("team");
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));
            var target = new PlanningPokerController(null, null, repository.Object);

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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team", timeProvider);
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        public void GetScrumTeam_ExpiredTeamInRepository_DeletesOldTeam()
        {
            // Arrange
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team", timeProvider);
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
            repository.Setup(r => r.DeleteScrumTeam("team"));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
            var target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (target.GetScrumTeam("team"))
            {
            }

            using (var teamLock = target.GetScrumTeam("team"))
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
            var timeProvider = new DateTimeProviderMock();
            var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
            configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

            timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
            var team = new ScrumTeam("team");
            var master = team.SetScrumMaster("master");
            master.UpdateActivity();

            bool firstLoad = true;
            bool firstReturn = true;
            PlanningPokerController target = null;
            ScrumTeam createdTeam = null;

            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team"))
                .Callback<string>(n =>
                {
                    if (firstLoad)
                    {
                        firstLoad = false;
                        using (var teamLock = target.CreateScrumTeam("team", "master"))
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

            target = new PlanningPokerController(timeProvider, configuration.Object, repository.Object);

            // Act
            using (var teamLock = target.GetScrumTeam("team"))
            {
                // Verify
                Assert.AreNotEqual<ScrumTeam>(team, createdTeam);
                Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
                Assert.AreEqual<ScrumTeam>(createdTeam, teamLock.Team);
                repository.Verify(r => r.LoadScrumTeam("team"), Times.Exactly(2));
                repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
            }
        }

        #endregion

        #region ScrumTeam activity

        [TestMethod]
        public void PlanningPokerController_ObserverUpdateActivity_ScrumTeamSavedToRepository()
        {
            // Arrange
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
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
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                teamLock.Team.Join("member", false);

                // Verify
                repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
            }
        }

        [TestMethod]
        public void PlanningPokerController_StartEstimation_ScrumTeamSavedToRepository()
        {
            // Arrange
            var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
            repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam)null);
            repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
            var target = new PlanningPokerController(null, null, repository.Object);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                teamLock.Team.ScrumMaster.StartEstimation();

                // Verify
                repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
            }
        }

        #endregion
    }
}
