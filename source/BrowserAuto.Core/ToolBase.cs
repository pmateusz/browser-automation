using CommandLine;
using System;
using System.Diagnostics;
using System.IO;

namespace BrowserAuto.Core
{
    public abstract class ToolBase<Options> where Options : OptionsBase, new()
    {
        public const int ErrorCodeFail = -1;
        public const int ErrorCodeOk = 0;

        private readonly Options options;

        public ToolBase()
        {
            this.options = new Options();
        }

        public int Run(string[] args)
        {
            if (!this.Configure(args))
            {
                return ErrorCodeFail;
            }


            if (this.options.Verbose)
            {
                Log.Level = TraceEventType.Verbose;
            }

            try
            {
                this.Run(this.options);

                return ErrorCodeOk;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return ErrorCodeFail;
        }

        /// <exception name="ArgumentException" />
        /// <exception name="InvalidOperationException" />
        /// <exception name="Exception" />
        protected abstract void Run(Options options);

        private bool Configure(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Log.Error(this.options.GetHelp());
                return false;
            }

            using (var parser = new Parser(s => { s.CaseSensitive = false; s.HelpWriter = Console.Error; }))
            {
                if (parser.ParseArguments(args, options))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
