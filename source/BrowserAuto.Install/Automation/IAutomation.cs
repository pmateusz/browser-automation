using System.Threading;
using System.Threading.Tasks;

namespace BrowserAuto.Install.Automation
{
    internal interface IAutomation
    {
        string ProductName { get; }

        Task Start(CancellationToken cancel);
    }
}
