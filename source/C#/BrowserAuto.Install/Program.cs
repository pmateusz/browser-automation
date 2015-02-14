namespace BrowserAuto.Install
{
    internal sealed class Program
    {
        static int Main(string[] args)
        {
            var tool = new BrowserInstallTool();
            return tool.Run(args);
        }
    }
}
