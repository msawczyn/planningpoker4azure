using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
    public class PlanningPokerControllerTest
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
        public void Constructor_IsConnected_False()
        {
            PlanningPokerController target = CreateController();

            Assert.IsFalse(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_TeamNameIsSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberName_IsNotScrumMaster()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberNameIsLowerCase_UserIsSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, "test member");

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterNameIsUpperCase_UserIsSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, "TEST SCRUM MASTER");

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_ObserverName_UserIsSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ObserverName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ObserverName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ObserverType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_LastMessageId_IsMinus1()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(-1, target.LastMessageId);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_ScrumMasterIsSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_MembersAndObserversAreSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            string[] expectedMembers = new string[] { PlanningPokerData.MemberName };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            string[] expectedObservers = new string[] { PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedObservers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeamWith4MembersAnd3Observers_MembersAndObserversAreSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Name = "me", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "1st Member", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "XYZ", Type = PlanningPokerData.MemberType });
            scrumTeam.Observers.Add(new TeamMember { Name = "ABC", Type = PlanningPokerData.ObserverType });
            scrumTeam.Observers.Add(new TeamMember { Name = "Hello, World!", Type = PlanningPokerData.ObserverType });

            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            string[] expectedMembers = new string[] { "1st Member", "me", PlanningPokerData.MemberName, "XYZ" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.ToList());
            string[] expectedObservers = new string[] { "ABC", "Hello, World!", PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedObservers, target.Observers.ToList());
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeamWithMembersSetToNull_MembersAndObserversAreSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members = null;
            scrumTeam.Observers = null;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            CollectionAssert.AreEqual(Array.Empty<string>(), target.Members.ToList());
            CollectionAssert.AreEqual(Array.Empty<string>(), target.Observers.ToList());
            Assert.IsNotNull(target.ScrumTeam.Members);
            Assert.AreEqual(0, target.ScrumTeam.Members.Count);
            Assert.IsNotNull(target.ScrumTeam.Observers);
            Assert.AreEqual(0, target.ScrumTeam.Observers.Count);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_AvailableEstimatesAreSet()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            double?[] expectedEstimates = new double?[] { 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100, double.PositiveInfinity, null };
            CollectionAssert.AreEqual(expectedEstimates, target.AvailableEstimates.ToList());
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndInitialState_CanStartEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimateInProgress_CanCancelEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimateFinished_CanStartEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateFinished;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimateCanceled_CanStartEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateCanceled;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndInitialState_CannotStartEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimateInProgress_CannotCancelEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimateFinished_CannotStartEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateFinished;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimateCanceled_CannotStartEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateCanceled;
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateFinishedAnd5Estimates_5Estimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.State = TeamState.EstimateFinished;
            scrumTeam.EstimateResult = new List<EstimateResultItem>
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
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

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
        public async Task InitializeTeam_EstimateFinishedAndMemberWithoutEstimate_4Estimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.State = TeamState.EstimateFinished;
            scrumTeam.EstimateResult = new List<EstimateResultItem>
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
            };

            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = new Estimate { Value = double.PositiveInfinity }
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

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
        public async Task InitializeTeam_EstimateFinishedAndSameEstimateCount_6Estimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            scrumTeam.State = TeamState.EstimateFinished;
            scrumTeam.EstimateResult = new List<EstimateResultItem>
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
            };

            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = null
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

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
        public async Task InitializeTeam_EstimateFinishedAndSameEstimateCountWithNull_6Estimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            scrumTeam.State = TeamState.EstimateFinished;
            scrumTeam.EstimateResult = new List<EstimateResultItem>
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
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

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
        public async Task InitializeTeam_EstimateFinishedAndEmptyEstimatesList_NoEstimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateFinished;
            scrumTeam.EstimateResult = new List<EstimateResultItem>();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

            Assert.AreEqual(0, target.Estimates.Count());
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateInProgressAnd4Participants_3Estimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.State = TeamState.EstimateInProgress;
            scrumTeam.EstimateParticipants = new List<EstimateParticipantStatus>
            {
                new EstimateParticipantStatus { MemberName = PlanningPokerData.ScrumMasterName, Estimated = true },
                new EstimateParticipantStatus { MemberName = "Tester", Estimated = false },
                new EstimateParticipantStatus { MemberName = "Developer 1", Estimated = true },
                new EstimateParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = true }
            };
            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = new Estimate { Value = 8 }
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

            List<MemberEstimate> estimates = target.Estimates.ToList();
            Assert.AreEqual(3, estimates.Count);

            MemberEstimate estimate = estimates[0];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);

            estimate = estimates[1];
            Assert.AreEqual("Developer 1", estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);

            estimate = estimates[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimate.MemberName);
            Assert.IsFalse(estimate.HasEstimate);
            Assert.IsNull(estimate.Estimate);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateInProgressAnd0Participants_0Estimates()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            scrumTeam.EstimateParticipants = new List<EstimateParticipantStatus>();
            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = null
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

            Assert.AreEqual(0, target.Estimates.Count());
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateInProgressAndParticipantsListIsNull_EstimatesIsNull()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = null
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsTrue(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);

            Assert.IsNull(target.Estimates);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateInProgressAndMemberInParticipantsAndNotEstimated_CanSelectEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            scrumTeam.EstimateParticipants = new List<EstimateParticipantStatus>
            {
                new EstimateParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = null
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsTrue(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateInProgressAndMemberInParticipantsAndEstimated_CannotSelectEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            scrumTeam.EstimateParticipants = new List<EstimateParticipantStatus>
            {
                new EstimateParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = true }
            };
            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = new Estimate { Value = 1 }
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimateInProgressAndMemberNotInParticipants_CannotSelectEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            scrumTeam.EstimateParticipants = new List<EstimateParticipantStatus>
            {
                new EstimateParticipantStatus { MemberName = PlanningPokerData.ScrumMasterName, Estimated = false }
            };
            ReconnectTeamResult reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimate = null
            };
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimate);
            Assert.IsFalse(target.CanCancelEstimate);
            Assert.IsFalse(target.CanSelectEstimate);
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_NotifyPropertyChanged()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();
            propertyChangedCounter.Target = target;

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(1, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_SetCredentialsAsync()
        {
            Mock<IMemberCredentialsStore> memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            MemberCredentials memberCredentials = null;
            memberCredentialsStore.Setup(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()))
                .Callback<MemberCredentials>(c => memberCredentials = c)
                .Returns(Task.CompletedTask);
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()));
            Assert.IsNotNull(memberCredentials);
            Assert.AreEqual(PlanningPokerData.TeamName, memberCredentials.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, memberCredentials.MemberName);
        }

        [TestMethod]
        public async Task InitializeTeam_ReconnectTeamResult_SetCredentialsAsync()
        {
            Mock<IMemberCredentialsStore> memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            MemberCredentials memberCredentials = null;
            memberCredentialsStore.Setup(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()))
                .Callback<MemberCredentials>(c => memberCredentials = c)
                .Returns(Task.CompletedTask);
            ReconnectTeamResult reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            PlanningPokerController target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(reconnectTeamResult, "test member");

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()));
            Assert.IsNotNull(memberCredentials);
            Assert.AreEqual(PlanningPokerData.TeamName, memberCredentials.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, memberCredentials.MemberName);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_DisconnectTeam()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.Disconnect();

            planningPokerClient.Verify(o => o.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task Disconnect_Initialized_IsConnectedIsFalse()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            Assert.IsFalse(target.IsConnected);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.IsConnected);

            await target.Disconnect();

            Assert.IsFalse(target.IsConnected);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_SetCredentialsToNull()
        {
            Mock<IMemberCredentialsStore> memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.Disconnect();

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(null));
        }

        [TestMethod]
        public async Task Disconnect_Initialized_NotifyPropertyChanged()
        {
            PropertyChangedCounter propertyChangedCounter = new PropertyChangedCounter();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();
            propertyChangedCounter.Target = target;

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.Disconnect();

            Assert.AreEqual(2, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_ShowsBusyIndicator()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.DisconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            Mock<IBusyIndicatorService> busyIndicatorService = new Mock<IBusyIndicatorService>();
            Mock<IDisposable> busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Task result = target.Disconnect();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task DisconnectMember_MemberName_DisconnectTeam()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.DisconnectMember(PlanningPokerData.MemberName);

            planningPokerClient.Verify(o => o.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()));
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task DisconnectMember_ScrumMasterName_ArgumentException()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.DisconnectMember(PlanningPokerData.ScrumMasterName));
        }

        [TestMethod]
        public async Task DisconnectMember_Null_ArgumentNullException()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.DisconnectMember(null));
        }

        [TestMethod]
        public async Task DisconnectMember_Empty_ArgumentNullException()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.DisconnectMember(string.Empty));
        }

        [TestMethod]
        public async Task DisconnectMember_MemberName_ShowsBusyIndicator()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.DisconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            Mock<IBusyIndicatorService> busyIndicatorService = new Mock<IBusyIndicatorService>();
            Mock<IDisposable> busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Task result = target.DisconnectMember(PlanningPokerData.MemberName);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task StartEstimate_CanStartEstimate_StartEstimateOnService()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.StartEstimate();

            planningPokerClient.Verify(o => o.StartEstimate(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task StartEstimate_CannotStartEstimate_DoNothing()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.StartEstimate();

            planningPokerClient.Verify(o => o.StartEstimate(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task StartEstimate_CanStartEstimate_ShowsBusyIndicator()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.StartEstimate(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            Mock<IBusyIndicatorService> busyIndicatorService = new Mock<IBusyIndicatorService>();
            Mock<IDisposable> busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Task result = target.StartEstimate();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task CancelEstimate_CanCancelEstimate_CancelEstimateOnService()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.CancelEstimate();

            planningPokerClient.Verify(o => o.CancelEstimate(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CancelEstimate_CannotCancelEstimate_DoNothing()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.CancelEstimate();

            planningPokerClient.Verify(o => o.CancelEstimate(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CancelEstimate_CanCancelEstimate_ShowsBusyIndicator()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.CancelEstimate(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            Mock<IBusyIndicatorService> busyIndicatorService = new Mock<IBusyIndicatorService>();
            Mock<IDisposable> busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimateInProgress;
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Task result = target.CancelEstimate();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task SelectEstimate_5AndCanSelectEstimate_SelectEstimateOnService()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimate(5);

            planningPokerClient.Verify(o => o.SubmitEstimate(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, 5, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimate_PositiveInfinityAndCanSelectEstimate_SelectEstimateOnService()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimate(double.PositiveInfinity);

            planningPokerClient.Verify(o => o.SubmitEstimate(PlanningPokerData.TeamName, PlanningPokerData.MemberName, double.PositiveInfinity, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimate_NullAndCanSelectEstimate_SelectEstimateOnService()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimate(null);

            planningPokerClient.Verify(o => o.SubmitEstimate(PlanningPokerData.TeamName, PlanningPokerData.MemberName, null, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimate_CannotSelectEstimate_DoNothing()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.SelectEstimate(5);

            planningPokerClient.Verify(o => o.SubmitEstimate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task SelectEstimate_Selects5_CannotSelectEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimate);

            await target.SelectEstimate(5);

            Assert.IsFalse(target.CanSelectEstimate);
        }

        [TestMethod]
        public async Task SelectEstimate_SelectsPositiveInfinity_CannotSelectEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimate);

            await target.SelectEstimate(double.PositiveInfinity);

            Assert.IsFalse(target.CanSelectEstimate);
        }

        [TestMethod]
        public async Task SelectEstimate_SelectsNull_CannotSelectEstimate()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimate);

            await target.SelectEstimate(null);

            Assert.IsFalse(target.CanSelectEstimate);
        }

        [TestMethod]
        public async Task SelectEstimate_CanSelectEstimate_ShowsBusyIndicator()
        {
            Mock<IPlanningPokerClient> planningPokerClient = new Mock<IPlanningPokerClient>();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.SubmitEstimate(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<double?>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            Mock<IBusyIndicatorService> busyIndicatorService = new Mock<IBusyIndicatorService>();
            Mock<IDisposable> busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            ScrumTeam scrumTeam = PlanningPokerData.GetScrumTeam();
            PlanningPokerController target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            Message message = new Message { Id = 1, Type = MessageType.EstimateStarted };
            target.ProcessMessages(new Message[] { message });

            Task result = target.SelectEstimate(5);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        private static PlanningPokerController CreateController(
            IPlanningPokerClient planningPokerClient = null,
            IBusyIndicatorService busyIndicator = null,
            IMemberCredentialsStore memberCredentialsStore = null)
        {
            if (planningPokerClient == null)
            {
                Mock<IPlanningPokerClient> planningPokerClientMock = new Mock<IPlanningPokerClient>();
                planningPokerClient = planningPokerClientMock.Object;
            }

            if (busyIndicator == null)
            {
                Mock<IBusyIndicatorService> busyIndicatorMock = new Mock<IBusyIndicatorService>();
                busyIndicator = busyIndicatorMock.Object;
            }

            if (memberCredentialsStore == null)
            {
                Mock<IMemberCredentialsStore> memberCredentialsStoreMock = new Mock<IMemberCredentialsStore>();
                memberCredentialsStore = memberCredentialsStoreMock.Object;
            }

            return new PlanningPokerController(planningPokerClient, busyIndicator, memberCredentialsStore);
        }
    }
}
