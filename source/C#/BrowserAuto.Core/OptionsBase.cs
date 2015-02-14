using CommandLine;
using CommandLine.Text;
using System;
using System.Linq;

namespace BrowserAuto.Core
{
    public abstract class OptionsBase
    {
        private const int IndentSize = 2;

        [Option('d', "diagnostic", DefaultValue = false, HelpText = "Enable diagnostic logging")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetHelp()
        {
            var help = new HelpText
            {
                Heading = this.Header,
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            this.Customize(help);

            if (this.LastParserState != null && this.LastParserState.Errors.Any())
            {
                var errors = help.RenderParsingErrorsText(this, IndentSize);
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(Environment.NewLine + "Errors:");
                    help.AddPreOptionsLine(errors);
                    help.AddPostOptionsLine(" ");
                }
            }

            help.AddPreOptionsLine(Environment.NewLine + "Options:");
            help.AddOptions(this);

            return help;
        }

        protected abstract HeadingInfo Header { get; }

        protected virtual void Customize(HelpText help) { }
    }
}
