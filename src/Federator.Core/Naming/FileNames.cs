using System.Collections.Generic;

namespace Federator.Core.Naming
{
    /// <summary>
    /// What Windows refuses in a file name, named here rather than asked of the platform
    /// that happens to be running.
    ///
    /// This tool runs on Windows and writes Windows file names. Path.GetInvalidFileNameChars
    /// answers for the RUNNING platform, and off Windows it names only the null character
    /// and the forward slash, so a colon or a pipe passed straight through in the container
    /// and two checks read as passing over a name Windows would refuse. The list is the
    /// same 41 characters Path.GetInvalidFileNameChars returns on Windows.
    /// </summary>
    public static class FileNames
    {
        /// <summary>The nine printable ones, in the order Windows documents them.</summary>
        public const string RefusedPrintable = "<>:\"/\\|?*";

        private static readonly char[] refused = Build();

        /// <summary>
        /// Every character Windows refuses: the nine above and every control character
        /// below a space.
        /// </summary>
        public static char[] Refused
        {
            get { return (char[])refused.Clone(); }
        }

        /// <summary>True when this name carries a character Windows will not take.</summary>
        public static bool CanBeAName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.IndexOfAny(refused) < 0;
        }

        private static char[] Build()
        {
            List<char> all = new List<char>(RefusedPrintable.ToCharArray());

            for (char c = (char)0; c < ' '; c++)
            {
                all.Add(c);
            }

            return all.ToArray();
        }
    }
}
