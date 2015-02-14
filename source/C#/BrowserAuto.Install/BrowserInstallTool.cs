using BrowserAuto.Core;
using BrowserAuto.Install.Automation;
using System;
using System.IO;

namespace BrowserAuto.Install
{
    internal sealed class BrowserInstallTool : ToolBase<ConsoleOptions>
    {
        private static readonly TimeSpan InstallationTimeout = TimeSpan.FromMinutes(5);

        protected override void Run(ConsoleOptions options)
        {
            var installer = GetInstaller(options);
            var automation = GetAutomation(installer);
            var runner = new InstallRunner(installer, automation);
            runner.Install(InstallationTimeout);
        }

        private static FileInfo GetInstaller(ConsoleOptions options)
        {
            var installerPath = Environment.ExpandEnvironmentVariables(options.Installer);
            var installerFile = new FileInfo(installerPath);

            if (installerFile.Exists)
            {
                return installerFile;
            }

            var msg = string.Format("Installer file ({0}) does not exist", installerPath);
            throw new InvalidOperationException(msg);
        }

        private static IAutomation GetAutomation(FileInfo installer)
        {
            var filename = installer.Name.ToLowerInvariant();

            if (filename.Contains("chrome"))
            {
                return new ChromeAutomation();
            }

            if (filename.Contains("firefox"))
            {
                return new FirefoxAutomation();
            }

            var msg = string.Format(
                "Failed to launch automation. Installer file ({0}) should contain the name of the supported browser (Firefox) "
                + "or (Chrome) in the file name to indicate which automation should be lanunched.",
                installer.Name);
            throw new InvalidOperationException(msg);
        }
    }
}
