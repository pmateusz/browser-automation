using BrowserAuto.Core;
using CommandLine;
using CommandLine.Text;

namespace BrowserAuto.Install
{
    internal sealed class ConsoleOptions : OptionsBase
    {
        private const string ToolName = "InstallBrowser";

        private static readonly HeadingInfo HeadingInfo = new HeadingInfo(ToolName);

        [Option('i', "installer", Required = true, HelpText = "Browser installer. Chrome Canary and Firefox Nightly are supported.")]
        public string Installer { get; set; }

        protected override HeadingInfo Header
        {
            get { return HeadingInfo; }
        }

        protected override void Customize(HelpText help)
        {
            base.Customize(help);

            help.AddPostOptionsLine("Example: BrowserExec --installer \"firefox-38.0a1.en-US.win64.installer.exe\"");
            help.AddPostOptionsLine("\t BrowserExec --installer \"ChromeSetup.exe\"");
        }
    }
}
