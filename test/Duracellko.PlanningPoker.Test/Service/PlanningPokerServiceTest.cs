using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using D = Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Service.Test
{
    [TestClass]
    public class PlanningPokerServiceTest
    {
        private const string TeamName = "test team";
        private const string ScrumMasterName = "master";
        private const string MemberName = "member";
        private const string ObserverName = "observer";

        private const string LongTeamName = "ttttttttttttttttttttttttttttttttttttttttttttttttttt";
        private const string LongMemberName = "mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm";

        [TestMethod]
        public void Constructor_PlanningPoker_PlanningPokerPropertyIsSet()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);

            // Act
            PlanningPokerService result = new PlanningPokerService(planningPoker.Object);

            // Verify
            Assert.AreEqual<D.IPlanningPoker>(planningPoker.Object, result.PlanningPoker);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Null_ArgumentNullException()
        {
            // Act
            PlanningPokerService result = new PlanningPokerService(null);
        }

        [TestMethod]
        public void CreateTeam_TeamNameAndScrumMasterName_ReturnsCreatedTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName)).Returns(teamLock.Object).Verifiable();

            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ScrumTeam result = target.CreateTeam(TeamName, ScrumMasterName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.AreEqual<string>(TeamName, result.Name);
            Assert.IsNotNull(result.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual<string>(typeof(D.ScrumMaster).Name, result.ScrumMaster.Type);
        }

        [TestMethod]
        public void CreateTeam_TeamNameAndScrumMasterName_ReturnsTeamWithAvilableEstimates()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.CreateScrumTeam(TeamName, ScrumMasterName)).Returns(teamLock.Object).Verifiable();

            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ScrumTeam result = target.CreateTeam(TeamName, ScrumMasterName).Value;

            // Verify
            Assert.IsNotNull(result.AvailableEstimates);
            double?[] expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, Estimate.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.AvailableEstimates.Select(e => e.Value).ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(null, ScrumMasterName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateTeam_ScrumMasterNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(TeamName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(LongTeamName, ScrumMasterName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateTeam_ScrumMasterNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CreateTeam(TeamName, LongMemberName);
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndMemberNameAsMember_ReturnsTeamJoined()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ScrumTeam result = target.JoinTeam(TeamName, MemberName, false).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.AreEqual<string>(TeamName, result.Name);
            Assert.IsNotNull(result.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumMaster.Name);
            Assert.IsNotNull(result.Members);
            string[] expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, result.Members.Select(m => m.Name).ToList());
            string[] expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name, typeof(D.Member).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.Members.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndMemberNameAsMember_MemberIsAddedToTheTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, MemberName, false);

            // Verify
            string[] expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, team.Members.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndMemberNameAsMemberAndEstimateStarted_ScrumMasterIsEstimateParticipant()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            team.ScrumMaster.StartEstimate();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, MemberName, false);

            // Verify
            string[] expectedParticipants = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedParticipants, team.EstimateParticipants.Select(m => m.MemberName).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndObserverNameAsObserver_ReturnsTeamJoined()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ScrumTeam result = target.JoinTeam(TeamName, ObserverName, true).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.AreEqual<string>(TeamName, result.Name);
            Assert.IsNotNull(result.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumMaster.Name);
            Assert.IsNotNull(result.Observers);
            string[] expectedObservers = new string[] { ObserverName };
            CollectionAssert.AreEquivalent(expectedObservers, result.Observers.Select(m => m.Name).ToList());
            string[] expectedMemberTypes = new string[] { typeof(D.Observer).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.Observers.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void JoinTeam_TeamNameAndObserverNameAsObserver_ObserverIsAddedToTheTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, ObserverName, true);

            // Verify
            string[] expectedObservers = new string[] { ObserverName };
            CollectionAssert.AreEquivalent(expectedObservers, team.Observers.Select(m => m.Name).ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(null, MemberName, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JoinTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(LongTeamName, MemberName, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JoinTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.JoinTeam(TeamName, LongMemberName, false);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndScrumMasterName_ReturnsReconnectedTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, ScrumMasterName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(0, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
            Assert.IsNotNull(result.ScrumTeam.Members);
            string[] expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            string[] expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberName_ReturnsReconnectedTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Observer member = team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(0, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);
            Assert.IsNotNull(result.ScrumTeam.Members);
            string[] expectedMembers = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            string[] expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name, typeof(D.Member).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndObserverName_ReturnsReconnectedTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Observer observer = team.Join(ObserverName, true);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, ObserverName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(0, result.LastMessageId);
            Assert.AreEqual<string>(TeamName, result.ScrumTeam.Name);
            Assert.IsNotNull(result.ScrumTeam.ScrumMaster);
            Assert.AreEqual<string>(ScrumMasterName, result.ScrumTeam.ScrumMaster.Name);

            Assert.IsNotNull(result.ScrumTeam.Members);
            string[] expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, result.ScrumTeam.Members.Select(m => m.Name).ToList());
            string[] expectedMemberTypes = new string[] { typeof(D.ScrumMaster).Name };
            CollectionAssert.AreEquivalent(expectedMemberTypes, result.ScrumTeam.Members.Select(m => m.Type).ToList());

            Assert.IsNotNull(result.ScrumTeam.Observers);
            string[] expectedObservers = new string[] { ObserverName };
            CollectionAssert.AreEquivalent(expectedObservers, result.ScrumTeam.Observers.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndNonExistingMemberName_ArgumentException()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ActionResult<ReconnectTeamResult> result = target.ReconnectTeam(TeamName, MemberName);

            // Verify
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameWithMessages_ReturnsLastMessageId()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Observer member = team.Join(MemberName, false);
            team.ScrumMaster.StartEstimate();

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual<long>(1, result.LastMessageId);
            Assert.IsFalse(member.HasMessage);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimateInProgress_NoEstimateResult()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimate();
            member.Estimate = new D.Estimate(1);

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNull(result.ScrumTeam.EstimateResult);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimateFinished_EstimateResultIsSet()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimate();
            member.Estimate = new D.Estimate(1);
            team.ScrumMaster.Estimate = new D.Estimate(2);

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNotNull(result.ScrumTeam.EstimateResult);

            double?[] expectedEstimates = new double?[] { 2, 1 };
            CollectionAssert.AreEquivalent(expectedEstimates, result.ScrumTeam.EstimateResult.Select(e => e.Estimate.Value).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndMemberEstimated_ReturnsSelectedEstimate()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimate();
            member.Estimate = new D.Estimate(1);

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectedEstimate);
            Assert.AreEqual<double?>(1, result.SelectedEstimate.Value);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndMemberNotEstimated_NoSelectedEstimate()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimate();

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNull(result.SelectedEstimate);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimateNotStarted_NoSelectedEstimate()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNull(result.SelectedEstimate);
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimateStarted_AllMembersAreEstimateParticipants()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            team.ScrumMaster.StartEstimate();

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNotNull(result.ScrumTeam.EstimateParticipants);
            string[] expectedParticipants = new string[] { ScrumMasterName, MemberName };
            CollectionAssert.AreEqual(expectedParticipants, result.ScrumTeam.EstimateParticipants.Select(p => p.MemberName).ToList());
        }

        [TestMethod]
        public void ReconnectTeam_TeamNameAndMemberNameAndEstimateNotStarted_NoEstimateParticipants()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            ReconnectTeamResult result = target.ReconnectTeam(TeamName, MemberName).Value;

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ScrumTeam);
            Assert.IsNull(result.ScrumTeam.EstimateParticipants);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReconnectTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.ReconnectTeam(null, MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReconnectTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.ReconnectTeam(TeamName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReconnectTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.ReconnectTeam(LongTeamName, MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReconnectTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.ReconnectTeam(TeamName, LongMemberName);
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameAndScrumMasterName_ScrumMasterIsRemovedFromTheTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, ScrumMasterName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNull(team.ScrumMaster);
            Assert.IsFalse(team.Members.Any());
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameAndMemberName_MemberIsRemovedFromTheTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, MemberName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            string[] expectedMembers = new string[] { ScrumMasterName };
            CollectionAssert.AreEquivalent(expectedMembers, team.Members.Select(m => m.Name).ToList());
        }

        [TestMethod]
        public void DisconnectTeam_TeamNameAndObserverName_ObserverIsRemovedFromTheTeam()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            team.Join(ObserverName, true);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, ObserverName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsFalse(team.Observers.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DisconnectTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(null, MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DisconnectTeam_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisconnectTeam_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(LongTeamName, MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisconnectTeam_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.DisconnectTeam(TeamName, LongMemberName);
        }

        [TestMethod]
        public void StartEstimate_TeamName_ScrumTeamEstimateIsInProgress()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.StartEstimate(TeamName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.AreEqual<D.TeamState>(D.TeamState.EstimateInProgress, team.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartEstimate_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.StartEstimate(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StartEstimate_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.StartEstimate(LongTeamName);
        }

        [TestMethod]
        public void CancelEstimate_TeamName_ScrumTeamEstimateIsCanceled()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            team.ScrumMaster.StartEstimate();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CancelEstimate(TeamName);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.AreEqual<D.TeamState>(D.TeamState.EstimateCanceled, team.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CancelEstimate_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CancelEstimate(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CancelEstimate_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.CancelEstimate(LongTeamName);
        }

        [TestMethod]
        public void SubmitEstimate_TeamNameAndScrumMasterName_EstimateIsSetForScrumMaster()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, ScrumMasterName, 2.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(team.ScrumMaster.Estimate);
            Assert.AreEqual<double?>(2.0, team.ScrumMaster.Estimate.Value);
        }

        [TestMethod]
        public void SubmitEstimate_TeamNameAndScrumMasterNameAndMinus1111111_EstimateIsSetToNull()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, ScrumMasterName, -1111111.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(team.ScrumMaster.Estimate);
            Assert.IsNull(team.ScrumMaster.Estimate.Value);
        }

        [TestMethod]
        public void SubmitEstimate_TeamNameAndMemberNameAndMinus1111100_EstimateOfMemberIsSetToInfinity()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, MemberName, -1111100.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimate);
            Assert.IsTrue(double.IsPositiveInfinity(member.Estimate.Value.Value));
        }

        [TestMethod]
        public void SubmitEstimate_TeamNameAndMemberName_EstimateOfMemberIsSet()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, MemberName, 8.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimate);
            Assert.AreEqual<double?>(8.0, member.Estimate.Value);
        }

        [TestMethod]
        public void SubmitEstimate_TeamNameAndMemberNameAndMinus1111100_EstimateOfMemberIsSetInifinty()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, MemberName, -1111100.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimate);
            Assert.IsTrue(double.IsPositiveInfinity(member.Estimate.Value.Value));
        }

        [TestMethod]
        public void SubmitEstimate_TeamNameAndMemberNameAndMinus1111111_EstimateIsSetToNull()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, MemberName, -1111111.0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();
            teamLock.Verify(l => l.Team);

            Assert.IsNotNull(member.Estimate);
            Assert.IsNull(member.Estimate.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubmitEstimate_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(null, MemberName, 0.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubmitEstimate_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, null, 0.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SubmitEstimate_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(LongTeamName, MemberName, 1.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SubmitEstimate_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            target.SubmitEstimate(TeamName, LongMemberName, 1.0);
        }

        [TestMethod]
        public async Task GetMessages_MemberJoinedTeam_ScrumMasterGetsMessage()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Observer member = team.Join(MemberName, false);
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<Action<bool, D.Observer>>()))
                .Callback<D.Observer, Action<bool, D.Observer>>((o, c) => c(true, o)).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            IList<Message> result = await target.GetMessages(TeamName, ScrumMasterName, 0);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(1, result.Count);
            Assert.AreEqual<long>(1, result[0].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberJoined, result[0].Type);
            Assert.IsInstanceOfType(result[0], typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)result[0];
            Assert.IsNotNull(memberMessage.Member);
            Assert.AreEqual<string>(MemberName, memberMessage.Member.Name);
        }

        [TestMethod]
        public async Task GetMessages_EstimateEnded_ScrumMasterGetsMessages()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            D.Member member = (D.Member)team.Join(MemberName, false);
            D.ScrumMaster master = team.ScrumMaster;
            master.StartEstimate();
            master.Estimate = new D.Estimate(1.0);
            member.Estimate = new D.Estimate(2.0);

            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(master, It.IsAny<Action<bool, D.Observer>>()))
                .Callback<D.Observer, Action<bool, D.Observer>>((o, c) => c(true, o)).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            IList<Message> result = await target.GetMessages(TeamName, ScrumMasterName, 1);

            // Verify
            planningPoker.Verify();
            teamLock.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(4, result.Count);
            Assert.AreEqual<long>(2, result[0].Id);
            Assert.AreEqual<MessageType>(MessageType.EstimateStarted, result[0].Type);

            Assert.AreEqual<long>(3, result[1].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[1].Type);
            Assert.AreEqual<long>(4, result[2].Id);
            Assert.AreEqual<MessageType>(MessageType.MemberEstimated, result[2].Type);

            Assert.AreEqual<long>(5, result[3].Id);
            Assert.AreEqual<MessageType>(MessageType.EstimateEnded, result[3].Type);
            Assert.IsInstanceOfType(result[3], typeof(EstimateResultMessage));
            EstimateResultMessage estimationResultMessage = (EstimateResultMessage)result[3];

            Assert.IsNotNull(estimationResultMessage.EstimateResult);
            Tuple<string, double>[] expectedResult = new Tuple<string, double>[]
            {
                new Tuple<string, double>(ScrumMasterName, 1.0),
                new Tuple<string, double>(MemberName, 2.0)
            };
            CollectionAssert.AreEquivalent(expectedResult, estimationResultMessage.EstimateResult.Select(i => new Tuple<string, double>(i.Member.Name, i.Estimate.Value.Value)).ToList());
        }

        [TestMethod]
        public async Task GetMessages_NoMessagesOnTime_ReturnsEmptyCollection()
        {
            // Arrange
            D.ScrumTeam team = CreateBasicTeam();
            Mock<D.IScrumTeamLock> teamLock = CreateTeamLock(team);
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            planningPoker.Setup(p => p.GetScrumTeam(TeamName)).Returns(teamLock.Object).Verifiable();
            planningPoker.Setup(p => p.GetMessagesAsync(team.ScrumMaster, It.IsAny<Action<bool, D.Observer>>()))
                .Callback<D.Observer, Action<bool, D.Observer>>((o, c) => c(false, null)).Verifiable();
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            IList<Message> result = await target.GetMessages(TeamName, ScrumMasterName, 0);

            // Verify
            planningPoker.Verify();

            Assert.IsNotNull(result);
            Assert.AreEqual<int>(0, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetMessages_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            await target.GetMessages(null, MemberName, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetMessages_MemberNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            await target.GetMessages(TeamName, null, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetMessages_TeamNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            await target.GetMessages(LongTeamName, MemberName, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetMessages_MemberNameTooLong_ArgumentException()
        {
            // Arrange
            Mock<D.IPlanningPoker> planningPoker = new Mock<D.IPlanningPoker>(MockBehavior.Strict);
            PlanningPokerService target = new PlanningPokerService(planningPoker.Object);

            // Act
            await target.GetMessages(TeamName, LongMemberName, 0);
        }

        private static D.ScrumTeam CreateBasicTeam()
        {
            D.ScrumTeam result = new D.ScrumTeam(TeamName);
            result.SetScrumMaster(ScrumMasterName);
            return result;
        }

        private static Mock<D.IScrumTeamLock> CreateTeamLock(D.ScrumTeam scrumTeam)
        {
            Mock<D.IScrumTeamLock> result = new Mock<D.IScrumTeamLock>(MockBehavior.Strict);
            result.Setup(l => l.Team).Returns(scrumTeam);
            result.Setup(l => l.Lock()).Verifiable();
            result.Setup(l => l.Dispose()).Verifiable();
            return result;
        }
    }
}
