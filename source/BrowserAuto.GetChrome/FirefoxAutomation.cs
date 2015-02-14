using BrowserAuto.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;

namespace BrowserAuto.GetChrome
{
    internal sealed class FirefoxAutomation : IDisposable
    {
        private const string ChromeCanaryDownloadPage = @"https://www.google.com/chrome/browser/canary.html";
        private const string ChromeThankYouPage = @"https://www.google.com/chrome/browser/thankyou.html";

        private static readonly TimeSpan AjaxTimeout = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan RedirectionTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DownloadTimeout = TimeSpan.FromSeconds(60);

        private readonly DirectoryInfo downloadDir;
        private readonly IWebDriver driver;
        private readonly WebDriverWait ajaxDriver;

        public FirefoxAutomation(DirectoryInfo downloadDir)
        {
            this.downloadDir = downloadDir;

            var browserProfile = CreateBrowserProfile(this.downloadDir);
            this.driver = new FirefoxDriver(browserProfile);
            this.ajaxDriver = new WebDriverWait(this.driver, AjaxTimeout);
        }

        /// <exception cref="InvalidOperationException" />
        public void DownloadCanary(Platform platform)
        {
            this.OpenDownloadPage(platform);
            this.SelectCanary();
            this.UploadStatistics(false);
            this.AcceptEula();
            this.WaitForRedirection(RedirectionTimeout);
            this.WaitForDownloadToComplete(DownloadTimeout);
        }

        internal void OpenDownloadPage(Platform platform)
        {
            var url = string.Format("{0}?platform={1}", ChromeCanaryDownloadPage, platform.UrlFragment);

            Log.Verbose("Open page ({0})", url);

            this.driver.Url = url;
        }

        /// <exception cref="InvalidOperationException" />
        internal void SelectCanary()
        {
            Log.Verbose("Click button (Download Chrome Canary)");

            var canaryBtn = this.FindAjaxElement(By.ClassName("canary-button"));
            canaryBtn.Click();
        }

        /// <exception cref="InvalidOperationException" />
        internal void UploadStatistics(bool value)
        {
            Log.Verbose("{0} checkbox (sending usage statistics and crash reports)", value ? "Select" : "Do not select");

            var uploadStatsCb = this.FindAjaxElement(By.Id("stats-cb"));
            if (uploadStatsCb.Selected ^ value)
            {
                uploadStatsCb.Click();
            }
        }

        /// <exception cref="InvalidOperationException" />
        internal void AcceptEula()
        {
            Log.Verbose("Click button (Accept and Install)");

            var acceptEulaBtn = this.FindAjaxElement(By.Id("eula-accept"));
            acceptEulaBtn.Click();
        }

        /// <exception cref="InvalidOperationException" />
        private IWebElement FindAjaxElement(By condition)
        {
            try
            {
                return this.ajaxDriver.Until(d => d.FindElement(condition));
            }
            catch (WebDriverTimeoutException ex)
            {
                var errMsg = string.Format("Element ({0}) not found", condition);
                throw new InvalidOperationException(errMsg, ex);
            }
        }

        private void WaitForDownloadToComplete(TimeSpan timeout)
        {
            using (var cancelSource = new CancellationTokenSource())
            {
                var downloadFinishMonitor = new DownloadFinishMonitor(this.downloadDir);
                var downloadFinishTask = downloadFinishMonitor.Start(cancelSource.Token);
                cancelSource.CancelAfter(timeout);

                try
                {
                    downloadFinishTask.Wait(cancelSource.Token);
                    var outputFile = downloadFinishTask.Result;
                    Log.Information("Output file: {0}", outputFile.Name);
                }
                catch (OperationCanceledException ex)
                {
                    var msg = string.Format("Download has not been completed within ({0}) seconds", timeout.Seconds);
                    throw new InvalidOperationException(msg, ex);
                }
            }
        }

        /// <exception cref="InvalidOperationException" />
        private void WaitForRedirection(TimeSpan timeout)
        {
            Log.Verbose("Waiting for redirection to page ({0}) for ({1}) seconds", ChromeThankYouPage, timeout.TotalSeconds);

            var waitDriver = new WebDriverWait(this.driver, timeout);
            try
            {
                waitDriver.Until(d => d.Url.StartsWith(ChromeThankYouPage));
            }
            catch (WebDriverTimeoutException ex)
            {
                var errMsg = string.Format("Redirection to page ({0}) has not happened within ({1}) seconds", ChromeThankYouPage, timeout.TotalSeconds);
                throw new InvalidOperationException(errMsg, ex);
            }
        }

        private static FirefoxProfile CreateBrowserProfile(DirectoryInfo downloadDir)
        {
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("browser.download.dir", downloadDir.FullName);
            profile.SetPreference("browser.download.folderList", 2); // it must be 2 in order to download successfully in container environment
            profile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/x-msdos-program");
            return profile;
        }

        #region IDisposable
        public void Dispose()
        {
            this.driver.Close();
        }
        #endregion
    }
}
