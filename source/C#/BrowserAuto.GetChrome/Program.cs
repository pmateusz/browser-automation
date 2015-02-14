
namespace BrowserAuto.GetChrome
{
    internal sealed class Program
    {
        internal static int Main(string[] args)
        {
            var tool = new GetChromeTool();
            return tool.Run(args);
        }
    }
}
