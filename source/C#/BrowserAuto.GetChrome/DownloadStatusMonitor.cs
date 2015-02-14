using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BrowserAuto.GetChrome
{
    internal sealed class DownloadFinishMonitor
    {
        private static readonly Regex SetupFileRegex = new Regex(@"^ChromeSetup(\s?\(\d+\))?.exe$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly TimeSpan ProbingTimeout = TimeSpan.FromMilliseconds(5000);

        private readonly DirectoryInfo downloadDirectory;

        public DownloadFinishMonitor(DirectoryInfo downloadDirectory)
        {
            this.downloadDirectory = downloadDirectory;
        }

        public async Task<FileInfo> Start(CancellationToken token)
        {
            FileInfo downloadedFile = null;

            while (!this.TryGetDownloadedFile(out downloadedFile))
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(ProbingTimeout, token);
            }

            while (IsLocked(downloadedFile))
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(ProbingTimeout, token);
            }

            return downloadedFile;
        }

        private static bool IsLocked(FileInfo file)
        {
            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        private bool TryGetDownloadedFile(out FileInfo file)
        {
            file = this.downloadDirectory.GetFiles().Where(f => SetupFileRegex.IsMatch(f.Name)).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            return file != null;
        }
    }
}
