using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Components;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.Test.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [TestClass]
    public class PlanningPokerDeskTest
    {
        [TestMethod]
        public async Task InitializedTeamWithScrumMaster_ShowStartEstimateButton()
        {
            IServiceProvider serviceProvider = CreateServiceProvider();
            TestRenderer renderer = serviceProvider.GetRequiredService<TestRenderer>();
            PlanningPokerController controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            await controller.InitializeTeam(PlanningPokerData.GetScrumTeam(), PlanningPokerData.ScrumMasterName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            int componentId = renderer.AssignRootComponentId(target);
            renderer.RenderRootComponent(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            List<RenderTreeFrame> frames = renderer.Batches[0].ReferenceFrames;
            Assert.AreEqual(39, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 39);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[3], "div", 17);
            AssertFrame.Attribute(frames[4], "class", "team-title");
            AssertFrame.Element(frames[6], "h2", 6);
            AssertFrame.Markup(frames[8], "<span class=\"badge\"><span class=\"glyphicon glyphicon-tasks\"></span></span>\n            ");
            AssertFrame.Element(frames[9], "span", 2);
            AssertFrame.Text(frames[10], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[13], "h3", 6);
            AssertFrame.Markup(frames[15], "<span class=\"badge\"><span class=\"glyphicon glyphicon-user\"></span></span>\n            ");
            AssertFrame.Element(frames[16], "span", 2);
            AssertFrame.Text(frames[17], PlanningPokerData.ScrumMasterName);

            // Button to start estimate
            AssertFrame.Element(frames[23], "div", 14);
            AssertFrame.Attribute(frames[24], "class", "actionsBar");
            AssertFrame.Element(frames[26], "p", 10);
            AssertFrame.Element(frames[29], "a", 4);
            AssertFrame.Attribute(frames[30], "onclick");
            AssertFrame.Attribute(frames[31], "class", "btn btn-default");
            AssertFrame.Markup(frames[32], "\n                        <span class=\"glyphicon glyphicon-play\"></span> Start estimate\n                    ");
        }

        [TestMethod]
        public async Task PlanningPokerStartedWithMember_ShowsAvailableEstimates()
        {
            IServiceProvider serviceProvider = CreateServiceProvider();
            TestRenderer renderer = serviceProvider.GetRequiredService<TestRenderer>();
            PlanningPokerController controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            ReconnectTeamResult reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam.State = TeamState.EstimateInProgress;
            reconnectResult.ScrumTeam.EstimateParticipants = new List<EstimateParticipantStatus>
            {
                new EstimateParticipantStatus() { MemberName = PlanningPokerData.ScrumMasterName, Estimated = true },
                new EstimateParticipantStatus() { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.MemberName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            int componentId = renderer.AssignRootComponentId(target);
            renderer.RenderRootComponent(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            List<RenderTreeFrame> frames = renderer.Batches[0].ReferenceFrames;
            Assert.AreEqual(133, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 133);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[3], "div", 17);
            AssertFrame.Attribute(frames[4], "class", "team-title");
            AssertFrame.Element(frames[6], "h2", 6);
            AssertFrame.Markup(frames[8], "<span class=\"badge\"><span class=\"glyphicon glyphicon-tasks\"></span></span>\n            ");
            AssertFrame.Element(frames[9], "span", 2);
            AssertFrame.Text(frames[10], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[13], "h3", 6);
            AssertFrame.Markup(frames[15], "<span class=\"badge\"><span class=\"glyphicon glyphicon-user\"></span></span>\n            ");
            AssertFrame.Element(frames[16], "span", 2);
            AssertFrame.Text(frames[17], PlanningPokerData.MemberName);

            // Available estimates
            AssertFrame.Element(frames[22], "div", 86);
            AssertFrame.Attribute(frames[23], "class", "availableEstimates");
            AssertFrame.Markup(frames[25], "<h3>Pick estimate</h3>\n            ");
            AssertFrame.Element(frames[26], "ul", 81);
            AssertAvailableEstimate(frames, 29, "0");
            AssertAvailableEstimate(frames, 35, "½");
            AssertAvailableEstimate(frames, 41, "1");
            AssertAvailableEstimate(frames, 47, "2");
            AssertAvailableEstimate(frames, 53, "3");
            AssertAvailableEstimate(frames, 59, "5");
            AssertAvailableEstimate(frames, 65, "8");
            AssertAvailableEstimate(frames, 71, "13");
            AssertAvailableEstimate(frames, 77, "20");
            AssertAvailableEstimate(frames, 83, "40");
            AssertAvailableEstimate(frames, 89, "100");
            AssertAvailableEstimate(frames, 95, "∞");
            AssertAvailableEstimate(frames, 101, "?");

            // Members, who estimated already
            AssertFrame.Element(frames[112], "div", 20);
            AssertFrame.Attribute(frames[113], "class", "estimationResult");
            AssertFrame.Markup(frames[115], "<h3>Selected estimates</h3>\n            ");
            AssertFrame.Element(frames[116], "ul", 15);
            AssertSelectedEstimate(frames, 119, PlanningPokerData.ScrumMasterName, string.Empty);
        }

        [TestMethod]
        public async Task PlanningPokerEstimatedWithObserver_ShowsEstimates()
        {
            IServiceProvider serviceProvider = CreateServiceProvider();
            TestRenderer renderer = serviceProvider.GetRequiredService<TestRenderer>();
            PlanningPokerController controller = serviceProvider.GetRequiredService<PlanningPokerController>();

            ReconnectTeamResult reconnectResult = PlanningPokerData.GetReconnectTeamResult();
            reconnectResult.ScrumTeam.State = TeamState.EstimateFinished;
            reconnectResult.ScrumTeam.EstimateResult = new List<EstimateResultItem>
            {
                new EstimateResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimate = new Estimate { Value = 8 }
                },
                new EstimateResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimate = new Estimate { Value = 3 }
                }
            };
            await controller.InitializeTeam(reconnectResult, PlanningPokerData.ObserverName);
            var target = renderer.InstantiateComponent<PlanningPokerDesk>();

            int componentId = renderer.AssignRootComponentId(target);
            renderer.RenderRootComponent(componentId);

            Assert.AreEqual(1, renderer.Batches.Count);
            List<RenderTreeFrame> frames = renderer.Batches[0].ReferenceFrames;
            Assert.AreEqual(57, frames.Count);

            // Team name and user name
            AssertFrame.Element(frames[0], "div", 57);
            AssertFrame.Attribute(frames[1], "class", "pokerDeskPanel");
            AssertFrame.Element(frames[3], "div", 17);
            AssertFrame.Attribute(frames[4], "class", "team-title");
            AssertFrame.Element(frames[6], "h2", 6);
            AssertFrame.Markup(frames[8], "<span class=\"badge\"><span class=\"glyphicon glyphicon-tasks\"></span></span>\n            ");
            AssertFrame.Element(frames[9], "span", 2);
            AssertFrame.Text(frames[10], PlanningPokerData.TeamName);
            AssertFrame.Element(frames[13], "h3", 6);
            AssertFrame.Markup(frames[15], "<span class=\"badge\"><span class=\"glyphicon glyphicon-user\"></span></span>\n            ");
            AssertFrame.Element(frames[16], "span", 2);
            AssertFrame.Text(frames[17], PlanningPokerData.ObserverName);

            // Estimates
            AssertFrame.Element(frames[24], "div", 32);
            AssertFrame.Attribute(frames[25], "class", "estimationResult");
            AssertFrame.Markup(frames[27], "<h3>Selected estimates</h3>\n            ");
            AssertFrame.Element(frames[28], "ul", 27);
            AssertSelectedEstimate(frames, 31, PlanningPokerData.MemberName, "3");
            AssertSelectedEstimate(frames, 43, PlanningPokerData.ScrumMasterName, "8");
        }

        private static IServiceProvider CreateServiceProvider()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<TestRenderer>();
            serviceCollection.AddSingleton<PlanningPokerController>();
            serviceCollection.AddSingleton(new Mock<IMessageBoxService>().Object);
            serviceCollection.AddSingleton(new Mock<IPlanningPokerClient>().Object);
            serviceCollection.AddSingleton(new Mock<IBusyIndicatorService>().Object);
            serviceCollection.AddSingleton(new Mock<IMemberCredentialsStore>().Object);
            return serviceCollection.BuildServiceProvider();
        }

        private static void AssertAvailableEstimate(List<RenderTreeFrame> frames, int index, string estimationText)
        {
            AssertFrame.Element(frames[index], "li", 4);
            AssertFrame.Element(frames[index + 1], "a", 3);
            AssertFrame.Attribute(frames[index + 2], "onclick");
            AssertFrame.Text(frames[index + 3], estimationText);
        }

        private static void AssertSelectedEstimate(List<RenderTreeFrame> frames, int index, string memberName, string estimationText)
        {
            AssertFrame.Element(frames[index], "li", 10);
            AssertFrame.Element(frames[index + 2], "span", 3);
            AssertFrame.Attribute(frames[index + 3], "class", "estimationItemValue");
            AssertFrame.Text(frames[index + 4], estimationText);
            AssertFrame.Element(frames[index + 6], "span", 3);
            AssertFrame.Attribute(frames[index + 7], "class", "estimationItemName");
            AssertFrame.Text(frames[index + 8], memberName);
        }
    }
}
