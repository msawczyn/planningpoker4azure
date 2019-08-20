using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class CreateTeamControllerTest
    {
        [TestMethod]
        public async Task CreateTeam_TeamName_CreateTeamOnService()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            Mock<IPlanningPokerClient> planningPokerService = new Mock<IPlanningPokerClient>();
            planningPokerService.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(scrumTeam);
            CreateTeamController target = CreateController(planningPokerService: planningPokerService.Object);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            planningPokerService.Verify(o => o.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CreateTeam_TeamNameAndScrumMasterName_ReturnTrue()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            CreateTeamController target = CreateController(scrumTeam: scrumTeam);

            bool result = await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(result);
        }

        [DataTestMethod]
        [DataRow(PlanningPokerData.TeamName, "", DisplayName = "ScrumMasterName Is Empty")]
        [DataRow(PlanningPokerData.TeamName, null, DisplayName = "ScrumMasterName Is Null")]
        [DataRow("", PlanningPokerData.ScrumMasterName, DisplayName = "TeamName Is Empty")]
        [DataRow(null, PlanningPokerData.ScrumMasterName, DisplayName = "TeamName Is Null")]
        public async Task CreateTeam_TeamNameOrScrumMasterNameIsEmpty_ReturnFalse(string teamName, string scrumMasterName)
        {
            Mock<IPlanningPokerClient> planningPokerService = new Mock<IPlanningPokerClient>();
            CreateTeamController target = CreateController(planningPokerService: planningPokerService.Object);

            bool result = await target.CreateTeam(teamName, scrumMasterName);

            Assert.IsFalse(result);
            planningPokerService.Verify(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceReturnsTeam_InitializePlanningPokerController()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            Mock<IPlanningPokerInitializer> planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
            CreateTeamController target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, scrumTeam: scrumTeam);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            planningPokerInitializer.Verify(o => o.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
        {
            ScrumTeam scrumTeam = PlanningPokerData.GetInitialScrumTeam();
            Mock<IUriHelper> uriHelper = new Mock<IUriHelper>();
            CreateTeamController target = CreateController(uriHelper: uriHelper.Object, scrumTeam: scrumTeam);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            uriHelper.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20Scrum%20Master"));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_ReturnsFalse()
        {
            CreateTeamController target = CreateController(errorMessage: string.Empty);

            bool result = await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
        {
            Mock<IPlanningPokerInitializer> planningPokerInitializer = new Mock<IPlanningPokerInitializer>();

            CreateTeamController target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, errorMessage: string.Empty);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ScrumTeam>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
        {
            Mock<IUriHelper> uriHelper = new Mock<IUriHelper>();

            CreateTeamController target = CreateController(uriHelper: uriHelper.Object, errorMessage: string.Empty);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            uriHelper.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_ShowsMessage()
        {
            string errorMessage = "Planning Poker Error";
            Mock<IMessageBoxService> messageBoxService = new Mock<IMessageBoxService>();

            CreateTeamController target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error", "Error"));
        }

        [TestMethod]
        public async Task CreateTeam_ServiceThrowsException_Shows1LineMessage()
        {
            string errorMessage = "Planning Poker Error\r\nArgumentException";
            Mock<IMessageBoxService> messageBoxService = new Mock<IMessageBoxService>();

            CreateTeamController target = CreateController(messageBoxService: messageBoxService.Object, errorMessage: errorMessage);

            await target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            messageBoxService.Verify(o => o.ShowMessage("Planning Poker Error\r", "Error"));
        }

        [TestMethod]
        public async Task CreateTeam_TeamName_ShowsBusyIndicator()
        {
            Mock<IPlanningPokerClient> planningPokerService = new Mock<IPlanningPokerClient>();
            Mock<IBusyIndicatorService> busyIndicatorService = new Mock<IBusyIndicatorService>();
            TaskCompletionSource<ScrumTeam> createTeamTask = new TaskCompletionSource<ScrumTeam>();
            planningPokerService.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(createTeamTask.Task);
            Mock<IDisposable> busyIndicatorInstance = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance.Object);
            CreateTeamController target = CreateController(planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

            Task<bool> result = target.CreateTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorInstance.Verify(o => o.Dispose(), Times.Never());

            createTeamTask.SetResult(PlanningPokerData.GetInitialScrumTeam());
            await result;

            busyIndicatorInstance.Verify(o => o.Dispose());
        }

        private static CreateTeamController CreateController(
            IPlanningPokerInitializer planningPokerInitializer = null,
            IPlanningPokerClient planningPokerService = null,
            IMessageBoxService messageBoxService = null,
            IBusyIndicatorService busyIndicatorService = null,
            IUriHelper uriHelper = null,
            ScrumTeam scrumTeam = null,
            string errorMessage = null)
        {
            if (planningPokerInitializer == null)
            {
                Mock<IPlanningPokerInitializer> planningPokerInitializerMock = new Mock<IPlanningPokerInitializer>();
                planningPokerInitializer = planningPokerInitializerMock.Object;
            }

            if (planningPokerService == null)
            {
                Mock<IPlanningPokerClient> planningPokerServiceMock = new Mock<IPlanningPokerClient>();
                ISetup<IPlanningPokerClient, Task<ScrumTeam>> createSetup = planningPokerServiceMock.Setup(o => o.CreateTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
                if (errorMessage == null)
                {
                    createSetup.ReturnsAsync(scrumTeam);
                }
                else
                {
                    createSetup.ThrowsAsync(new PlanningPokerException(errorMessage));
                }

                planningPokerService = planningPokerServiceMock.Object;
            }

            if (messageBoxService == null)
            {
                Mock<IMessageBoxService> messageBoxServiceMock = new Mock<IMessageBoxService>();
                messageBoxService = messageBoxServiceMock.Object;
            }

            if (busyIndicatorService == null)
            {
                Mock<IBusyIndicatorService> busyIndicatorServiceMock = new Mock<IBusyIndicatorService>();
                busyIndicatorService = busyIndicatorServiceMock.Object;
            }

            if (uriHelper == null)
            {
                Mock<IUriHelper> uriHelperMock = new Mock<IUriHelper>();
                uriHelper = uriHelperMock.Object;
            }

            return new CreateTeamController(planningPokerService, planningPokerInitializer, messageBoxService, busyIndicatorService, uriHelper);
        }
    }
}
