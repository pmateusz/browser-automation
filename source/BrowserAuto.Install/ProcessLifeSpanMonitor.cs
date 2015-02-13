using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BrowserAuto.Install
{
    internal sealed class ProcessLifeSpanMonitor : IDisposable
    {
        private readonly ManualResetEventSlim processNotRunningEvent;

        public ProcessLifeSpanMonitor(Process process)
        {
            this.processNotRunningEvent = new ManualResetEventSlim(false);

            if (!process.HasExited)
            {
                process.EnableRaisingEvents = true;
                process.Exited += OnExited;
            }

            if (process.HasExited)
            {
                this.processNotRunningEvent.Set();
            }

        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() => this.processNotRunningEvent.Wait(token));
        }

        private void OnExited(object sender, EventArgs e)
        {
            this.processNotRunningEvent.Set();
        }

        #region IDisposable
        public void Dispose()
        {
            this.processNotRunningEvent.Dispose();
        } 
        #endregion
    }
}
