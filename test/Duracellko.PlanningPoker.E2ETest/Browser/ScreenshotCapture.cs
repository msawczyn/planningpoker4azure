﻿using System;
using System.IO;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest.Browser
{
    public class ScreenshotCapture
    {
        private string _basePath;

        private string BasePath
        {
            get
            {
                if (_basePath == null)
                {
                    string assemblyLocation = typeof(ScreenshotCapture).Assembly.Location;
                    _basePath = Path.Combine(Path.GetDirectoryName(assemblyLocation), "Screenshots");
                }

                return _basePath;
            }
        }

        public string TakeScreenshot(ITakesScreenshot driver, BrowserTestContext context, string name)
        {
            if (driver == null)
            {
                throw new ArgumentNullException(nameof(driver));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Screenshot screenshot = driver.GetScreenshot();
            string screenshotFolder = GetScreenshotFolder(context);
            string screenshotPath = Path.Combine(screenshotFolder, name + ".png");
            screenshot.SaveAsFile(screenshotPath);
            return screenshotPath;
        }

        private string GetScreenshotFolder(BrowserTestContext context)
        {
            string browserName = context.BrowserType.ToString();
            string serverSide = context.ServerSide ? "Server" : "Client";
            string path = Path.Combine(BasePath, browserName, serverSide, context.ClassName, context.TestName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}
