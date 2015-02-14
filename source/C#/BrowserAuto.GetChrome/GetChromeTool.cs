using BrowserAuto.Core;
using System;
using System.IO;

namespace BrowserAuto.GetChrome
{
    internal sealed class GetChromeTool : ToolBase<ConsoleOptions>
    {
        protected override void Run(ConsoleOptions options)
        {
            var platform = Platform.Parse(options.Platform);
            var outputDirectory = GetOutputDirectory(options);

            using (var firefox = new FirefoxAutomation(outputDirectory))
            {
                firefox.DownloadCanary(platform);
            }
        }

        private static DirectoryInfo GetOutputDirectory(ConsoleOptions options)
        {
            var outputDirPath = Environment.ExpandEnvironmentVariables(options.OutputDirectory);
            var outputDir = new DirectoryInfo(outputDirPath);

            if (!outputDir.Exists)
            {
                var msg = string.Format("Output directory ({0}) does not exist", outputDir.FullName);
                throw new InvalidOperationException(msg);
            }

            return outputDir;
        }
    }
}
