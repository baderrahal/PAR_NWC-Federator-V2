using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Federator.Core.Communities
{
    /// <summary>
    /// A group of one discipline is not built, F233, Bader's round of 19 Oct 2026, fix 3, on both pages: no append, no NWF, NWD,
    /// Excel, Internal Excel, HTML, picture folder or viewpoints. The log names each one in one line, its community, its
    /// building, its discipline, its file names and the words one discipline, not built. RESULT carries one line per community
    /// with the count. An NWF of such a group left by an earlier run is not deleted and not run, and the log names it. The
    /// rule of how many disciplines a group holds is BuildingGroup.CannotClashWith, read by the window and the engine.
    /// </summary>
    public static class OneDisciplineGroups
    {
        public const string Prefix = "NOT BUILT";

        public const string Words = "one discipline, not built";

        /// <summary>The one line a group of one discipline gets in the log.</summary>
        public static string Line(string community, string building, string discipline, IEnumerable<string> files)
        {
            List<string> names = new List<string>();

            foreach (string file in files ?? new string[0])
            {
                names.Add(NameOf(file));
            }

            return Prefix + "  " + Or(community) + "  " + Or(building) + "  " + Or(discipline) + "  "
                + (names.Count == 0 ? "UNKNOWN files" : string.Join(", ", names.ToArray())) + "  " + Words;
        }

        /// <summary>The line an NWF of such a group gets where an earlier run left it, never deleted and never run.</summary>
        public static string EarlierNwfLine(string nwf)
        {
            return Prefix + "  the NWF " + Or(nwf) + " of an earlier run is there, and it is not deleted and not run";
        }

        /// <summary>Whether a reason a group's own Navisworks gave is this line, so the window that started the run counts it.</summary>
        public static bool IsTheLine(string said)
        {
            return said != null && said.StartsWith(Prefix + "  ", StringComparison.Ordinal) && said.EndsWith(Words, StringComparison.Ordinal);
        }

        /// <summary>The community of a package's path from the parent, its first folder.</summary>
        public static string CommunityOf(string relativePath)
        {
            string path = relativePath ?? string.Empty;
            int at = path.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            return at < 0 ? path : path.Substring(0, at);
        }

        /// <summary>RESULT's line for one community.</summary>
        public static string ResultLine(string community, int count)
        {
            return "not built      " + Or(community) + " : " + count.ToString(CultureInfo.InvariantCulture)
                + (count == 1 ? " group" : " groups") + " not built for one discipline";
        }

        private static string NameOf(string file)
        {
            try
            {
                return Path.GetFileName(file);
            }
            catch (ArgumentException)
            {
                return file;
            }
        }

        private static string Or(string value)
        {
            return string.IsNullOrEmpty(value) ? "UNKNOWN" : value;
        }
    }
}
