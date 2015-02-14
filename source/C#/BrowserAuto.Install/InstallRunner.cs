using BrowserAuto.Core;
using BrowserAuto.Install.Automation;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BrowserAuto.Install
{
    internal class InstallRunner
    {
        private readonly FileInfo installer;
        private readonly IAutomation automation;

        public InstallRunner(FileInfo installer, IAutomation automation)
        {
            this.installer = installer;
            this.automation = automation;
        }

        public void Install(TimeSpan timeout)
        {
            using (var installProcess = StartInstallation(installer))
            using (var processMonitor = new ProcessLifeSpanMonitor(installProcess))
            using (var cancellationSource = new CancellationTokenSource())
            {
                cancellationSource.CancelAfter(timeout);

                try
                {
                    var processLifeSpanTask = processMonitor.Start(cancellationSource.Token);
                    var automationTask = this.automation.Start(cancellationSource.Token);
                    Task.WaitAny(new[] { automationTask }, cancellationSource.Token);

                    cancellationSource.Cancel();

                    if (processLifeSpanTask.IsCompleted && installProcess.ExitCode != 0)
                    {
                        throw OnExitCode(installProcess);
                    }

                    if (automationTask.IsCompleted || processLifeSpanTask.IsCompleted)
                    {
                        Log.Information("Installation of {0} completed", this.automation.ProductName);
                        return;
                    }

                    throw OnUknownFailure(installProcess, automationTask.Exception);
                }
                catch (OperationCanceledException)
                {
                    throw OnTimeout(installProcess, timeout);
                }
                finally
                {
                    QuietKill(installProcess);
                }
            }
        }

        private InvalidOperationException OnExitCode(Process process)
        {
            var msg = string.Format("Installation process PID ({0}) exited unexpectedly with error code ({1})", process.Id, process.ExitCode);
            return new InvalidOperationException(msg);
        }

        private Exception OnUknownFailure(Process process, Exception ex)
        {
            var msg = string.Format("Automation failed to complete installation of ({0}) launched in process PID ({1})", this.automation.ProductName, process.Id);
            return new Exception(msg, ex);
        }

        private InvalidOperationException OnTimeout(Process process, TimeSpan timeout)
        {
            var msg = string.Format("Installation process PID ({0}) was aborted after ({1}) minutes", process.Id, timeout.TotalMinutes);
            return new InvalidOperationException(msg);
        }

        private static Process StartInstallation(FileInfo installer)
        {
            try
            {
                return Process.Start(installer.FullName);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Cannot start process ({0})", installer.FullName, ex);
                throw new Exception(msg, ex);
            }
        }

        private static void QuietKill(Process process)
        {
            try
            {
                process.Kill();
            }
            catch (Exception)
            {
                // no-op
            }
        }
    }
}
