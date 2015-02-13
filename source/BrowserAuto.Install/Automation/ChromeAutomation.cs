using BrowserAuto.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace BrowserAuto.Install.Automation
{
    internal sealed class ChromeAutomation : IAutomation
    {
        private static readonly Query InstallerPaneQuery = Query.OfControlType(ControlType.Pane).Name("Chrome Canary Installer");
        private static readonly Query CloseButtonQuery = Query.OfControlType(ControlType.Button).Name("Close");

        public string ProductName
        {
            get { return "Chrome Canary"; }
        }

        public async Task Start(CancellationToken token)
        {
            Log.Verbose("Waiting for installer window to open");

            var installerPane = await GetInstallerPane(token);

            Log.Verbose("Installer window opened. Waiting for installation to complete");

            await GetCloseButton(installerPane, token);
        }

        private async Task<AutomationElement> GetInstallerPane(CancellationToken token)
        {
            try
            {
                return await AutomationElement.RootElement.FirstChild(InstallerPaneQuery, token);
            }
            catch (TaskCanceledException)
            {
                Log.Error("Installer window has not opened");
                throw;
            }
        }


        private async Task<AutomationElement> GetCloseButton(AutomationElement installerPane, CancellationToken token)
        {
            try
            {
                return await installerPane.FirstChild(CloseButtonQuery, token);
            }
            catch (TaskCanceledException)
            {
                Log.Error("Installation process has not completed");
                throw;
            }
        }

        private static bool IsInstallerVisible()
        {
            var installerWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, InstallerPaneQuery);
            return installerWindow != null;
        }
    }
}
