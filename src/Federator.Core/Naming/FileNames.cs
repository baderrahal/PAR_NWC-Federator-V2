using System.Collections.Generic;
using System.Globalization;

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

        /// <summary>
        /// The four printable ones Path.GetInvalidPathChars lists, which the documentation of the path
        /// methods of .NET Framework says they throw ArgumentException on, with every control character.
        /// A test on the Windows runner measured it for a bar, a quote and a tab. The colon is a drive and
        /// an address, and the star and the question mark are not among the characters that list holds.
        /// </summary>
        public const string RefusedPrintableInAPath = "\"<>|";

        private static readonly char[] refused = Build();
        private static readonly char[] refusedInAPath = BuildForAPath();

        /// <summary>
        /// Every character Windows refuses: the nine above and every control character
        /// below a space.
        /// </summary>
        public static char[] Refused
        {
            get { return (char[])refused.Clone(); }
        }

        /// <summary>True when this name is not empty and carries no character Windows will not take.</summary>
        public static bool CanBeAName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.IndexOfAny(refused) < 0;
        }

        /// <summary>
        /// Where the first character is that Path.GetFileName and its kind are documented to throw on,
        /// or minus one. Asked before a path is split, so such a path is named and not handed to them.
        /// </summary>
        public static int IndexOfRefusedInAPath(string path)
        {
            return path == null ? -1 : path.IndexOfAny(refusedInAPath);
        }

        /// <summary>
        /// A refused character in words a person can find in the name: the character in quotes, or its
        /// code where it cannot be seen.
        /// </summary>
        public static string Name(char refusedCharacter)
        {
            return refusedCharacter < ' '
                ? "a control character (U+" + ((int)refusedCharacter).ToString("X4", CultureInfo.InvariantCulture) + ")"
                : "\"" + refusedCharacter + "\"";
        }

        private static char[] BuildForAPath()
        {
            List<char> all = new List<char>(RefusedPrintableInAPath.ToCharArray());

            for (char c = (char)0; c < ' '; c++)
            {
                all.Add(c);
            }

            return all.ToArray();
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
