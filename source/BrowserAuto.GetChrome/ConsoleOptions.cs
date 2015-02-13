using BrowserAuto.Core;
using CommandLine;
using CommandLine.Text;
using System;

namespace BrowserAuto.GetChrome
{
    internal sealed class ConsoleOptions : OptionsBase
    {
        private const string ToolName = "GetChrome";

        private static readonly HeadingInfo HeadingInfo = new HeadingInfo(ToolName);

        [Option('o', "output-dir", Required = true, HelpText = "Directory where Chrome Canary should be saved")]
        public string OutputDirectory { get; set; }

        [Option('p', "platform", Required = true, HelpText = "Target platform of Chrome Canary. Supported values: <x86>|<x64>")]
        public string Platform { get; set; }

        protected override HeadingInfo Header
        {
            get { return HeadingInfo; }
        }

        protected override void Customize(HelpText help)
        {
            base.Customize(help);

            var example = string.Format("Example: {0} --platform x64 --outputDir=\"C:\\Downloads\"", ToolName);
            help.AddPostOptionsLine(example);
        }
    }
}
