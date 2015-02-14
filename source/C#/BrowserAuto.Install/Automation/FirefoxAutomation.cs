using BrowserAuto.Core;
using BrowserAuto.Core.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace BrowserAuto.Install.Automation
{
    internal sealed class FirefoxAutomation : IAutomation
    {
        private static readonly Regex InstallerWindowPattern = new Regex("^Nightly Setup", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Query NextButtonQuery = Query.OfControlType(ControlType.Button).Name("Next >");
        private static readonly Query StandardRadioButtonQuery = Query.OfControlType(ControlType.RadioButton).Name("Standard");
        private static readonly Query InstallButtonQuery = Query.OfControlType(ControlType.Button).Name("Upgrade", "Install");
        private static readonly Query RunFirefoxCheckbox = Query.OfControlType(ControlType.CheckBox).Name("Launch Nightly now");
        private static readonly Query FinishButtonQuery = Query.OfControlType(ControlType.Button).Name("Finish");

        public string ProductName
        {
            get { return "Firefox Nightly"; }
        }

        public async Task Start(CancellationToken token)
        {
            Log.Verbose("Waiting for installer window to open");

            var installerWindow = await GetInstallerWindow(token);
            var installation = new Installation(installerWindow, token);
            await installation.Start();
        }

        private Task<AutomationElement> GetInstallerWindow(CancellationToken token)
        {
            try
            {
                return AutomationExtensions.FindApplicationWindow(InstallerWindowPattern, token);
            }
            catch (TaskCanceledException)
            {
                Log.Error("Installer window has not opened");
                throw;
            }
        }

        private sealed class Installation
        {
            private readonly AutomationElement installerWindow;
            private readonly CancellationToken token;

            public Installation(AutomationElement installerWindow, CancellationToken token)
            {
                this.installerWindow = installerWindow;
                this.token = token;
            }

            public async Task Start()
            {
                await this.Click("Next", NextButtonQuery);
                await this.SelectStandardEdition();
                await this.Click("Next", NextButtonQuery);
                await this.Click("Install", InstallButtonQuery);
                await this.DoNotLaunchFirefoxOnFinish();
                await this.Click("Finish", FinishButtonQuery);
            }

            private Task<AutomationElement> Get(Query query)
            {
                return this.installerWindow.FirstChild(query, this.token);
            }

            private async Task SelectStandardEdition()
            {
                Log.Verbose("Select radio button (Standard edition)");

                var standardRadioButton = await this.Get(StandardRadioButtonQuery);
                standardRadioButton.Select();
            }

            private async Task DoNotLaunchFirefoxOnFinish()
            {
                Log.Verbose("Do not select checkbox (Launch Nightly now)");

                var runNowCheckbox = await this.Get(RunFirefoxCheckbox);
                var checkBoxState = runNowCheckbox.GetState();
                if (checkBoxState != ToggleState.Off)
                {
                    runNowCheckbox.Toggle();
                }
            }

            private async Task Click(string buttonFriendlyName, Query query)
            {
                Log.Verbose("Click button ({0})", buttonFriendlyName);

                var button = await this.Get(query);
                button.Click();
            }
        }
    }
}