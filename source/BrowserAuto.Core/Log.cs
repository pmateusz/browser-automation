using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BrowserAuto.Core
{
    public static class Log
    {
        public static TraceEventType Level;

        static Log()
        {
            Level = TraceEventType.Information;
        }

        public static void Verbose(string msg)
        {
            Console.Out.Trace(TraceEventType.Verbose, msg);
        }

        public static void Verbose(string pattern, params object[] args)
        {
            Console.Out.Trace(TraceEventType.Verbose, pattern, args);
        }

        public static void Information(string msg)
        {
            Console.Out.Trace(TraceEventType.Information, msg);
        }

        public static void Information(string pattern, params object[] args)
        {
            Console.Out.Trace(TraceEventType.Information, pattern, args);
        }

        public static void Error(string msg)
        {
            Console.Error.Trace(TraceEventType.Error, msg);
        }

        public static void Error(string pattern, params object[] args)
        {
            Console.Out.Trace(TraceEventType.Error, pattern, args);
        }

        public static void Error(string msg, Exception ex)
        {
            if (!IsVerbose)
            {
                Error(msg);
                return;
            }

            var msgBuilder = new StringBuilder();
            msgBuilder.AppendLine(msg);
            msgBuilder.Append(ex);
            Error(msgBuilder.ToString());
        }

        private static bool IsVerbose
        {
            get { return ShouldLog(TraceEventType.Verbose); }
        }

        private static bool ShouldLog(TraceEventType level)
        {
            return Level.CompareTo(level) >= 0;
        }

        private static void Trace(this TextWriter writer, TraceEventType level, string pattern, params object[] args)
        {
            var msg = string.Format(pattern, args);
            Trace(writer, level, msg);
        }

        private static void Trace(this TextWriter writer, TraceEventType level, string msg)
        {
            if (!ShouldLog(level))
            {
                return;
            }

            writer.WriteLine(msg);
        }
    }
}
