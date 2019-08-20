﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    public abstract class E2ETestBase
    {
        protected ServerFixture Server { get; private set; }

        protected IList<BrowserFixture> BrowserFixtures { get; } = new List<BrowserFixture>();

        protected IList<BrowserTestContext> Contexts { get; } = new List<BrowserTestContext>();

        protected IList<ClientTest> ClientTests { get; } = new List<ClientTest>();

        protected BrowserFixture BrowserFixture => BrowserFixtures.FirstOrDefault();

        protected BrowserTestContext Context => Contexts.FirstOrDefault();

        protected ClientTest ClientTest => ClientTests.FirstOrDefault();

        protected ScreenshotCapture ScreenshotCapture { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Contexts.Clear();
            ClientTests.Clear();
            Server = new ServerFixture();
            BrowserFixtures.Clear();
            BrowserFixtures.Add(new BrowserFixture());
            ScreenshotCapture = new ScreenshotCapture();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ScreenshotCapture = null;
            ClientTests.Clear();
            Contexts.Clear();

            foreach (BrowserFixture browserFixture in BrowserFixtures)
            {
                browserFixture.Dispose();
            }

            BrowserFixtures.Clear();

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        protected IWebDriver GetBrowser() => BrowserFixture.Browser;

        protected IWebDriver GetBrowser(int index) => BrowserFixtures[index].Browser;

        protected async Task StartServer()
        {
            Server.UseServerSide = Context.ServerSide;
            await Server.Start();
            await AssertServerSide(Context.ServerSide);
        }

        protected void StartClients()
        {
            bool first = true;
            foreach (BrowserTestContext context in Contexts)
            {
                BrowserFixture browserFixture;
                if (first)
                {
                    browserFixture = BrowserFixtures[0];
                    first = false;
                }
                else
                {
                    browserFixture = new BrowserFixture();
                    BrowserFixtures.Add(browserFixture);
                }

                browserFixture.Initialize(context.BrowserType);
                ClientTests.Add(new ClientTest(browserFixture.Browser, Server));
            }
        }

        protected async Task AssertServerSide(bool serverSide)
        {
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(Server.Uri);

            string expected = serverSide ? "server" : "webassembly";
            expected = @"<script src=""_framework/blazor." + expected + @".js""></script>";
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Contains(expected, StringComparison.Ordinal));
        }

        protected string TakeScreenshot(string name)
        {
            return ScreenshotCapture.TakeScreenshot((ITakesScreenshot)GetBrowser(), Context, name);
        }

        protected string TakeScreenshot(int index, string name)
        {
            return ScreenshotCapture.TakeScreenshot((ITakesScreenshot)GetBrowser(index), Context, name);
        }
    }
}
