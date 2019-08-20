using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    public class ClientTest
    {
        private static readonly string[] _availableEstimates = new string[]
            { "0", "\u00BD", "1", "2", "3", "5", "8", "13", "20", "40", "100", "\u221E", "?" };

        public ClientTest(IWebDriver browser, ServerFixture server)
        {
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public IWebDriver Browser { get; }

        public ServerFixture Server { get; }

        public IWebElement AppElement { get; private set; }

        public IWebElement PageContentElement { get; private set; }

        public IWebElement PlanningPokerContainerElement { get; private set; }

        public IWebElement ContainerElement { get; private set; }

        public IWebElement CreateTeamForm { get; private set; }

        public IWebElement JoinTeamForm { get; private set; }

        public IWebElement PlanningPokerDeskElement { get; private set; }

        public IWebElement MembersPanelElement { get; private set; }

        public Task OpenApplication()
        {
            Browser.Navigate().GoToUrl(Server.Uri);
            AppElement = Browser.FindElement(By.TagName("app"));
            Assert.IsNotNull(AppElement);
            return Task.Delay(2000);
        }

        public void AssertIndexPage()
        {
            PageContentElement = AppElement.FindElement(By.CssSelector("div.pageContent"));
            PlanningPokerContainerElement = PageContentElement.FindElement(By.CssSelector("div.planningPokerContainer"));
            ContainerElement = PlanningPokerContainerElement.FindElement(By.XPath("./div[@class='container']"));
            CreateTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            JoinTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='joinTeam']"));
        }

        public void FillCreateTeamForm(string team, string scrumMaster)
        {
            IWebElement teamNameInput = CreateTeamForm.FindElement(By.Id("createTeam$teamName"));
            IWebElement scrumMasterNameInput = CreateTeamForm.FindElement(By.Id("createTeam$scrumMasterName"));

            teamNameInput.SendKeys(team);
            scrumMasterNameInput.SendKeys(scrumMaster);

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, scrumMasterNameInput.FindElements(By.XPath("../span")).Count);
        }

        public void SubmitCreateTeamForm()
        {
            IWebElement submitButton = CreateTeamForm.FindElement(By.Id("createTeam$Submit"));
            submitButton.Click();
        }

        public void FillJoinTeamForm(string team, string member)
        {
            FillJoinTeamForm(team, member, false);
        }

        public void FillJoinTeamForm(string team, string member, bool asObserver)
        {
            IWebElement teamNameInput = JoinTeamForm.FindElement(By.Id("joinTeam$teamName"));
            IWebElement memberNameInput = JoinTeamForm.FindElement(By.Id("joinTeam$memberName"));
            IWebElement observerInput = JoinTeamForm.FindElement(By.Id("joinTeam$asObserver"));

            teamNameInput.SendKeys(team);
            memberNameInput.SendKeys(member);
            if (asObserver)
            {
                observerInput.Click();
            }

            Assert.AreEqual(0, teamNameInput.FindElements(By.XPath("../span")).Count);
            Assert.AreEqual(0, memberNameInput.FindElements(By.XPath("../span")).Count);
        }

        public void SubmitJoinTeamForm()
        {
            IWebElement submitButton = JoinTeamForm.FindElement(By.Id("joinTeam$submit"));
            submitButton.Click();
        }

        public void AssertPlanningPokerPage(string team, string scrumMaster)
        {
            PlanningPokerDeskElement = ContainerElement.FindElement(By.CssSelector("div.pokerDeskPanel"));
            MembersPanelElement = ContainerElement.FindElement(By.CssSelector("div.membersPanel"));

            Assert.AreEqual($"{Server.Uri}PlanningPoker/{team}/{scrumMaster}", Browser.Url);
        }

        public void AssertTeamName(string team, string member)
        {
            IWebElement teamNameHeader = PlanningPokerDeskElement.FindElement(By.CssSelector("div.team-title h2"));
            Assert.AreEqual(team, teamNameHeader.Text);
            IWebElement userHeader = PlanningPokerDeskElement.FindElement(By.CssSelector("div.team-title h3"));
            Assert.AreEqual(member, userHeader.Text);
        }

        public void AssertScrumMasterInTeam(string scrumMaster)
        {
            ReadOnlyCollection<IWebElement> scrumMasterElements = MembersPanelElement.FindElements(By.XPath("./div[1]/ul/li"));
            Assert.AreEqual(1, scrumMasterElements.Count);
            Assert.AreEqual(scrumMaster, scrumMasterElements[0].Text);
        }

        public void AssertMembersInTeam(params string[] members)
        {
            ReadOnlyCollection<IWebElement> elements = MembersPanelElement.FindElements(By.XPath("./div[2]/ul/li"));
            if (members == null)
            {
                Assert.AreEqual(0, elements.Count);
            }
            else
            {
                Assert.AreEqual(members.Length, elements.Count);
                CollectionAssert.AreEqual(members, elements.Select(e => e.Text).ToList());
            }
        }

        public void AssertObserversInTeam(params string[] observers)
        {
            ReadOnlyCollection<IWebElement> elements = MembersPanelElement.FindElements(By.XPath("./div[3]/ul/li"));
            if (observers == null)
            {
                Assert.AreEqual(0, elements.Count);
            }
            else
            {
                Assert.AreEqual(observers.Length, elements.Count);
                CollectionAssert.AreEqual(observers, elements.Select(e => e.Text).ToList());
            }
        }

        public void StartEstimate()
        {
            IWebElement button = PlanningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar a"));
            Assert.AreEqual("Start estimate", button.Text);

            button.Click();

            PlanningPokerDeskElement.FindElement(By.CssSelector("div.availableEstimates"));
        }

        public void CancelEstimate()
        {
            IWebElement button = PlanningPokerDeskElement.FindElement(By.CssSelector("div.actionsBar a"));
            Assert.AreEqual("Cancel estimate", button.Text);
            button.Click();
        }

        public void AssertAvailableEstimates()
        {
            ReadOnlyCollection<IWebElement> availableEstimateElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimates ul li a"));
            Assert.AreEqual(13, availableEstimateElements.Count);
            CollectionAssert.AreEqual(_availableEstimates, availableEstimateElements.Select(e => e.Text).ToList());
        }

        public void AssertNotAvailableEstimates()
        {
            ReadOnlyCollection<IWebElement> availableEstimateElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimates"));
            Assert.AreEqual(0, availableEstimateElements.Count);
        }

        public void SelectEstimate(string estimate)
        {
            int index = Array.IndexOf<string>(_availableEstimates, estimate);
            ReadOnlyCollection<IWebElement> availableEstimateElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.availableEstimates ul li a"));
            availableEstimateElements[index].Click();
        }

        public void AssertSelectedEstimate(params KeyValuePair<string, string>[] estimates)
        {
            ReadOnlyCollection<IWebElement> estimationResultElements = PlanningPokerDeskElement.FindElements(By.CssSelector("div.estimationResult ul li"));
            if (estimates == null)
            {
                Assert.AreEqual(0, estimationResultElements.Count);
            }
            else
            {
                Assert.AreEqual(estimates.Length, estimationResultElements.Count);

                for (int i = 0; i < estimates.Length; i++)
                {
                    KeyValuePair<string, string> estimate = estimates[i];
                    IWebElement estimationResultElement = estimationResultElements[i];
                    IWebElement valueElement = estimationResultElement.FindElement(By.XPath("./span[1]"));
                    IWebElement nameElement = estimationResultElement.FindElement(By.XPath("./span[2]"));
                    Assert.AreEqual(estimate.Key, nameElement.Text);
                    Assert.AreEqual(estimate.Value, valueElement.Text);
                }
            }
        }

        public void Disconnect()
        {
            IWebElement navbarPlanningPokerElement = PlanningPokerContainerElement.FindElement(By.Id("navbarPlanningPoker"));
            IWebElement disconnectElement = navbarPlanningPokerElement.FindElement(By.TagName("a"));
            Assert.AreEqual("Disconnect", disconnectElement.Text);

            disconnectElement.Click();

            CreateTeamForm = ContainerElement.FindElement(By.CssSelector("form[name='createTeam']"));
            Assert.AreEqual($"{Server.Uri}Index", Browser.Url);
        }

        public void AssertMessageBox(string text)
        {
            IWebElement messageBoxElement = PageContentElement.FindElement(By.Id("messageBox"));
            Assert.AreEqual("block", messageBoxElement.GetCssValue("display"));

            IWebElement messageBodyElement = messageBoxElement.FindElement(By.CssSelector("div.modal-body"));
            Assert.AreEqual(text, messageBodyElement.Text);
        }
    }
}
