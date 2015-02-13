using System;

namespace BrowserAuto.GetChrome
{
    internal sealed class Platform
    {
        private const string X86 = "x86";
        private const string X64 = "x64";

        public static readonly Platform WindowsX86 = new Platform { Name = "Windows x86", ShortName = X86, UrlFragment = "win" };
        public static readonly Platform WindowsX64 = new Platform { Name = "Windows x64", ShortName = X64, UrlFragment = "win64" };

        private Platform() { }

        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string UrlFragment { get; private set; }

        internal static Platform Parse(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var normalizedName = name.Trim().ToLowerInvariant();
            switch (normalizedName)
            {
                case X86:
                    return WindowsX86;
                case X64:
                    return WindowsX64;
                default:
                    var msg = string.Format("Unknown platform ({0}). Use one of supported platforms instead: ({1}) or ({2})", name, X86, X64);
                    throw new ArgumentException(msg);
            }
        }
    }
}
