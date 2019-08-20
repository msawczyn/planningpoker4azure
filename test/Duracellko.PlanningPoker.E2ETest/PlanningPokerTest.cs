using System.Collections.Generic;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest
{
    [TestClass]
    public class PlanningPokerTest : E2ETestBase
    {
        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Estimate_2_Rounds(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Estimate_2_Rounds),
                browserType,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Estimate_2_Rounds),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member = "Bob";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);
            ClientTests[1].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member);

            // Alice starts estimate
            ClientTest.StartEstimate();
            TakeScreenshot("09-A-EstimateStarted");

            // Bob estimates
            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimateStarted");
            ClientTests[1].AssertAvailableEstimates();
            ClientTests[1].SelectEstimate("\u221E");

            await Task.Delay(500);
            KeyValuePair<string, string>[] expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
            TakeScreenshot(1, "11-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            ClientTests[1].AssertNotAvailableEstimates();
            TakeScreenshot("12-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimates();
            ClientTest.SelectEstimate("\u00BD");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "\u00BD"),
                new KeyValuePair<string, string>(member, "\u221E")
            };
            TakeScreenshot("13-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(1, "14-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);

            // Alice starts 2nd round of estimate
            ClientTest.StartEstimate();
            TakeScreenshot("15-A-EstimateStarted");

            // Alice estimates
            await Task.Delay(200);
            TakeScreenshot(1, "16-B-EstimateStarted");
            ClientTest.AssertAvailableEstimates();
            ClientTest.SelectEstimate("5");

            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            ClientTest.AssertNotAvailableEstimates();
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);

            // Bob estimates
            ClientTests[1].AssertAvailableEstimates();
            ClientTests[1].SelectEstimate("5");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "5"),
                new KeyValuePair<string, string>(member, "5")
            };
            TakeScreenshot(1, "19-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot("20-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);

            // Bob disconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "21-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("22-A-Disconnected");
            ClientTest.AssertMembersInTeam();

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("23-A-Disconnected");
        }

        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Cancel_Estimate(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cancel_Estimate),
                browserType,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cancel_Estimate),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member = "Bob";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);
            ClientTests[1].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member);

            // Alice starts estimate
            ClientTest.StartEstimate();
            TakeScreenshot("09-A-EstimateStarted");
            ClientTest.AssertAvailableEstimates();

            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimateStarted");
            ClientTests[1].AssertAvailableEstimates();

            // Alice estimates
            ClientTest.SelectEstimate("100");
            await Task.Delay(500);
            KeyValuePair<string, string>[] expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot(1, "11-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot("12-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            ClientTest.AssertNotAvailableEstimates();

            // Alice cancels estimate
            ClientTest.CancelEstimate();
            await Task.Delay(200);

            TakeScreenshot("13-A-EstimateCancelled");
            ClientTest.AssertNotAvailableEstimates();
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(1, "14-B-EstimateCancelled");
            ClientTests[1].AssertNotAvailableEstimates();
            ClientTests[1].AssertSelectedEstimate(expectedResult);

            // Alice starts estimate again
            ClientTest.StartEstimate();
            TakeScreenshot("15-A-EstimateStarted");

            // Alice estimates
            await Task.Delay(200);
            TakeScreenshot(1, "16-B-EstimateStarted");
            ClientTest.AssertAvailableEstimates();
            ClientTest.SelectEstimate("100");

            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(scrumMaster, string.Empty) };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);

            // Bob estimates
            ClientTests[1].AssertAvailableEstimates();
            ClientTests[1].SelectEstimate("20");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member, "20"),
                new KeyValuePair<string, string>(scrumMaster, "100")
            };
            TakeScreenshot(1, "19-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            ClientTests[1].AssertNotAvailableEstimates();
            TakeScreenshot("20-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("21-A-Disconnected");

            await Task.Delay(200);
            TakeScreenshot(1, "22-B-Disconnected");
            ClientTests[1].AssertScrumMasterInTeam(string.Empty);
            ClientTests[1].AssertMembersInTeam(member);

            // Bob disconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "23-A-Disconnected");
        }

        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Observer_Cannot_Estimate(bool serverSide, BrowserType browserType)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Observer_Cannot_Estimate),
                browserType,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Observer_Cannot_Estimate),
                browserType,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Observer_Cannot_Estimate),
                browserType,
                serverSide));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member = "Bob";
            string observer = "Charlie";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member);
            ClientTests[1].AssertObserversInTeam();

            // Charlie joins team as observer
            await ClientTests[2].OpenApplication();
            TakeScreenshot(2, "09-C-Loading");
            ClientTests[2].AssertIndexPage();
            TakeScreenshot(2, "10-C-Index");
            ClientTests[2].FillJoinTeamForm(team, observer, true);
            TakeScreenshot(2, "11-C-JoinTeamForm");
            ClientTests[2].SubmitJoinTeamForm();
            ClientTests[2].AssertPlanningPokerPage(team, observer);
            TakeScreenshot(2, "12-C-PlanningPoker");
            ClientTests[2].AssertTeamName(team, observer);
            ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[2].AssertMembersInTeam(member);
            ClientTests[2].AssertObserversInTeam(observer);

            await Task.Delay(200);
            ClientTest.AssertObserversInTeam(observer);
            ClientTests[1].AssertObserversInTeam(observer);

            // Alice starts estimate
            ClientTest.StartEstimate();
            TakeScreenshot("13-A-EstimateStarted");

            await Task.Delay(200);
            TakeScreenshot(1, "14-B-EstimateStarted");
            ClientTests[1].AssertAvailableEstimates();
            TakeScreenshot(2, "15-C-EstimateStarted");
            ClientTests[2].AssertNotAvailableEstimates();

            // Bob estimates
            ClientTests[1].SelectEstimate("3");
            await Task.Delay(500);
            KeyValuePair<string, string>[] expectedResult = new[] { new KeyValuePair<string, string>(member, string.Empty) };
            TakeScreenshot(1, "16-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot("17-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(2, "16-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimates();
            ClientTest.SelectEstimate("2");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "2"),
                new KeyValuePair<string, string>(member, "3")
            };
            TakeScreenshot("17-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(1, "18-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot(2, "19-C-ScrumMasterEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);

            // Bob disconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "20-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("21-A-Disconnected");
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam(observer);
            TakeScreenshot(2, "22-C-Disconnected");
            ClientTests[2].AssertMembersInTeam();
            ClientTests[2].AssertObserversInTeam(observer);

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("23-A-Disconnected");

            await Task.Delay(200);
            TakeScreenshot(2, "24-C-Disconnected");
            ClientTests[2].AssertScrumMasterInTeam(string.Empty);
            ClientTests[2].AssertMembersInTeam();
            ClientTests[2].AssertObserversInTeam(observer);

            // Charlie disconnects
            ClientTests[2].Disconnect();
            TakeScreenshot("25-C-Disconnected");
        }

        [DataTestMethod]
        [DataRow(false, BrowserType.Chrome, BrowserType.Chrome, DisplayName = "Client-side Chrome")]
        [DataRow(true, BrowserType.Chrome, BrowserType.Chrome, DisplayName = "Server-side Chrome")]
        public async Task Cannot_Estimate_When_Joining_After_Start(bool serverSide, BrowserType browserType1, BrowserType browserType2)
        {
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cannot_Estimate_When_Joining_After_Start),
                browserType1,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cannot_Estimate_When_Joining_After_Start),
                browserType2,
                serverSide));
            Contexts.Add(new BrowserTestContext(
                nameof(PlanningPokerTest),
                nameof(Cannot_Estimate_When_Joining_After_Start),
                browserType1,
                serverSide));

            await StartServer();
            StartClients();

            string team = "Duracellko.NET";
            string scrumMaster = "Alice";
            string member1 = "Bob";
            string member2 = "Charlie";

            // Alice creates team
            await ClientTest.OpenApplication();
            TakeScreenshot("01-A-Loading");
            ClientTest.AssertIndexPage();
            TakeScreenshot("02-A-Index");
            ClientTest.FillCreateTeamForm(team, scrumMaster);
            TakeScreenshot("03-A-CreateTeamForm");
            ClientTest.SubmitCreateTeamForm();
            ClientTest.AssertPlanningPokerPage(team, scrumMaster);
            TakeScreenshot("04-A-PlanningPoker");
            ClientTest.AssertTeamName(team, scrumMaster);
            ClientTest.AssertScrumMasterInTeam(scrumMaster);
            ClientTest.AssertMembersInTeam();
            ClientTest.AssertObserversInTeam();

            // Bob joins team
            await ClientTests[1].OpenApplication();
            TakeScreenshot(1, "05-B-Loading");
            ClientTests[1].AssertIndexPage();
            TakeScreenshot(1, "06-B-Index");
            ClientTests[1].FillJoinTeamForm(team, member1);
            TakeScreenshot(1, "07-B-JoinTeamForm");
            ClientTests[1].SubmitJoinTeamForm();
            ClientTests[1].AssertPlanningPokerPage(team, member1);
            TakeScreenshot(1, "08-B-PlanningPoker");
            ClientTests[1].AssertTeamName(team, member1);
            ClientTests[1].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[1].AssertMembersInTeam(member1);
            ClientTests[1].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member1);

            // Alice starts estimate
            ClientTest.StartEstimate();
            TakeScreenshot("09-A-EstimateStarted");
            ClientTest.AssertAvailableEstimates();

            await Task.Delay(200);
            TakeScreenshot(1, "10-B-EstimateStarted");
            ClientTests[1].AssertAvailableEstimates();

            // Charlie joins team
            await ClientTests[2].OpenApplication();
            TakeScreenshot(2, "11-C-Loading");
            ClientTests[2].AssertIndexPage();
            TakeScreenshot(2, "12-C-Index");
            ClientTests[2].FillJoinTeamForm(team, member2);
            TakeScreenshot(2, "13-C-JoinTeamForm");
            ClientTests[2].SubmitJoinTeamForm();
            ClientTests[2].AssertPlanningPokerPage(team, member2);
            TakeScreenshot(2, "14-C-PlanningPoker");
            ClientTests[2].AssertTeamName(team, member2);
            ClientTests[2].AssertScrumMasterInTeam(scrumMaster);
            ClientTests[2].AssertMembersInTeam(member1, member2);
            ClientTests[2].AssertObserversInTeam();

            await Task.Delay(200);
            ClientTest.AssertMembersInTeam(member1, member2);
            TakeScreenshot("15-A-MemberJoiner");
            ClientTests[1].AssertMembersInTeam(member1, member2);
            TakeScreenshot(1, "16-B-MemberJoiner");
            ClientTests[2].AssertNotAvailableEstimates();

            // Bob estimates
            ClientTests[1].SelectEstimate("13");
            await Task.Delay(500);
            KeyValuePair<string, string>[] expectedResult = new[] { new KeyValuePair<string, string>(member1, string.Empty) };
            TakeScreenshot(1, "17-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot("18-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(2, "19-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimates();
            ClientTest.SelectEstimate("20");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member1, "13"),
                new KeyValuePair<string, string>(scrumMaster, "20")
            };
            TakeScreenshot("20-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(1, "21-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot(2, "22-C-ScrumMasterEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);

            // Alice starts 2nd round of estimate
            ClientTest.StartEstimate();
            TakeScreenshot("23-A-EstimateStarted");
            ClientTest.AssertAvailableEstimates();

            await Task.Delay(200);
            TakeScreenshot(1, "24-B-EstimateStarted");
            ClientTests[1].AssertAvailableEstimates();
            TakeScreenshot(2, "25-C-EstimateStarted");
            ClientTests[2].AssertAvailableEstimates();

            // Charlie estimates
            ClientTests[2].SelectEstimate("20");
            await Task.Delay(500);
            expectedResult = new[] { new KeyValuePair<string, string>(member2, string.Empty) };
            TakeScreenshot(2, "26-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);
            ClientTests[2].AssertNotAvailableEstimates();
            TakeScreenshot("27-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(1, "28-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);

            // Alice estimates
            ClientTest.AssertAvailableEstimates();
            ClientTest.SelectEstimate("20");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(member2, string.Empty),
                new KeyValuePair<string, string>(scrumMaster, string.Empty)
            };
            TakeScreenshot("29-A-ScrumMasterEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            ClientTest.AssertNotAvailableEstimates();
            TakeScreenshot(1, "30-B-ScrumMasterEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            TakeScreenshot(2, "31-C-ScrumMasterEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);

            // Bob estimates
            ClientTests[1].AssertAvailableEstimates();
            ClientTests[1].SelectEstimate("2");
            await Task.Delay(500);
            expectedResult = new[]
            {
                new KeyValuePair<string, string>(scrumMaster, "20"),
                new KeyValuePair<string, string>(member2, "20"),
                new KeyValuePair<string, string>(member1, "2")
            };
            TakeScreenshot(1, "32-B-MemberEstimated");
            ClientTests[1].AssertSelectedEstimate(expectedResult);
            ClientTests[1].AssertNotAvailableEstimates();
            TakeScreenshot("33-A-MemberEstimated");
            ClientTest.AssertSelectedEstimate(expectedResult);
            TakeScreenshot(2, "34-C-MemberEstimated");
            ClientTests[2].AssertSelectedEstimate(expectedResult);

            // Bob diconnects
            ClientTests[1].Disconnect();
            TakeScreenshot(1, "35-B-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("36-A-Disconnected");
            ClientTest.AssertMembersInTeam(member2);
            TakeScreenshot(2, "37-C-Disconnected");
            ClientTests[2].AssertMembersInTeam(member2);

            // Charlie disconnects
            ClientTests[2].Disconnect();
            TakeScreenshot(2, "38-C-Disconnected");

            await Task.Delay(200);
            TakeScreenshot("39-A-Disconnected");
            ClientTest.AssertMembersInTeam();

            // Alice disconnects
            ClientTest.Disconnect();
            TakeScreenshot("40-A-Disconnected");
        }
    }
}
