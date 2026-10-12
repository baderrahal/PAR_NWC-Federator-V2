using System;
using System.Globalization;
using System.IO;
using Federator.Core.Diagnostics;

namespace Federator.Core.Report
{
    /// <summary>
    /// Navisworks' texture cache, the folder ogs under the user's temp folder, F147, Bader's
    /// order of 2026-10-11, Q152 item 2: between tests, cleared once it passes the limit, its
    /// size logged before and after. On this laptop it held no file after any run up to
    /// 2026-10-12, docs\history\scan.md 5z-zd, so this guards the long runs of other machines
    /// rather than a cost measured here.
    ///
    /// It is measured between two tests' pictures at most once a check interval, so walking a
    /// large cache is never itself what slows a run of 741 tests. The limit and the interval are
    /// settings on ImageOptions. A size that was not read whole is UNKNOWN and never clears
    /// anything. Only a rooted folder named ogs is ever walked, so a wrong path can never empty
    /// another folder, and every line names the folder actually walked.
    /// </summary>
    public static class TextureCache
    {
        /// <summary>The folder under the user's temp folder, Bader's words of 2026-10-11.</summary>
        public const string FolderUnderTemp = "ogs";

        /// <summary>Five gigabytes, counted as Windows counts them, 1024 cubed bytes each.</summary>
        public const long DefaultLimitBytes = 5L * 1024 * 1024 * 1024;

        /// <summary>The least seconds between two measurements of the cache.</summary>
        public const int DefaultCheckEverySeconds = 60;

        private const double Kilobyte = 1024d;
        private const double Megabyte = 1024d * 1024;
        private const double Gigabyte = 1024d * 1024 * 1024;

        /// <summary>Whether a check is due: the first one always, then once the interval has passed. Zero or less checks every time.</summary>
        public static bool DueToMeasure(bool measuredBefore, double secondsSinceLast, int everySeconds)
        {
            return !measuredBefore || everySeconds <= 0 || secondsSinceLast >= everySeconds;
        }

        /// <summary>
        /// Whether to clear it: only a size read whole and past the limit. A limit of zero or less
        /// switches the clearing off, and a size not read, below zero, never clears anything.
        /// </summary>
        public static bool ShouldClear(long bytes, long limitBytes)
        {
            return limitBytes > 0 && bytes >= 0 && bytes > limitBytes;
        }

        /// <summary>
        /// Whether a path may be walked and cleared as the cache: rooted, and its last part ogs. A
        /// null, empty or relative path, the temp folder itself or any other folder is refused, so a
        /// caller handing the wrong path can never empty it.
        /// </summary>
        public static bool IsACacheFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return false;
            }

            try
            {
                if (!Path.IsPathRooted(folder))
                {
                    return false;
                }

                string trimmed = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return string.Equals(Path.GetFileName(trimmed), FolderUnderTemp, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// A size in words: bytes below a kilobyte, then kilobytes, megabytes and gigabytes, each
        /// unit taken once the rounded figure reaches it, UNKNOWN when it was not read.
        /// </summary>
        public static string Size(long bytes)
        {
            if (bytes < 0)
            {
                return "UNKNOWN";
            }

            if (bytes < Kilobyte)
            {
                return Words.Counted((int)bytes, "byte", "bytes");
            }

            if (Math.Round(bytes / Megabyte, 1) < 1)
            {
                return (bytes / Kilobyte).ToString("0.0", CultureInfo.InvariantCulture) + " KB";
            }

            if (Math.Round(bytes / Megabyte, 1) < 1024)
            {
                return (bytes / Megabyte).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
            }

            return (bytes / Gigabyte).ToString("0.00", CultureInfo.InvariantCulture) + " GB";
        }

        /// <summary>A size in words with its exact bytes beside it, for a line that compares it with the limit.</summary>
        public static string Exactly(long bytes)
        {
            return bytes < 0 || bytes < Kilobyte
                ? Size(bytes)
                : Size(bytes) + " (" + bytes.ToString(CultureInfo.InvariantCulture) + " bytes)";
        }

        /// <summary>
        /// The line of one clearing: the size before with its files, what was removed, what could not
        /// be, and the size after, read again. A clearing that removed nothing says the cache is not
        /// cleared again in this pass.
        /// </summary>
        public static string ClearedLine(
            string folder, long before, int filesBefore, long limitBytes, int removed, int notRemoved, long after, int filesAfter, double seconds)
        {
            return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " held " + Exactly(before) + " in "
                + Words.Counted(filesBefore, "file", "files") + ", past the limit of " + Exactly(limitBytes) + ", so "
                + Words.Counted(removed, "file was", "files were") + " removed"
                + (notRemoved > 0 ? " and " + Words.Counted(notRemoved, "file", "files") + " could not be" : string.Empty)
                + ", and it holds " + Exactly(after) + " in " + Words.Counted(filesAfter, "file", "files") + " now, "
                + Seconds(seconds) + " for the clearing"
                + (removed == 0 ? ", so it is not cleared again in this pass" : string.Empty);
        }

        /// <summary>The line of a check whose walk did not finish, so nothing was cleared at it.</summary>
        public static string NotReadWholeLine(string folder, string why)
        {
            return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " could not be read whole, " + Words.Or(why, "UNKNOWN")
                + ", so its size is UNKNOWN and nothing was cleared at this check";
        }

        /// <summary>The line of a folder refused before anything is walked.</summary>
        public static string RefusedLine(string folder)
        {
            return "the texture cache is not checked, " + Words.Or(folder, "no folder") + " is not a rooted folder named "
                + FolderUnderTemp + ", so nothing is walked or removed";
        }

        /// <summary>The line of a cache folder that is itself a link to another folder, never walked.</summary>
        public static string LinkedLine(string folder)
        {
            return "the texture cache at " + Words.Or(folder, "UNKNOWN")
                + " is a link to another folder, so it is not walked and nothing is removed";
        }

        /// <summary>
        /// The one line of a picture pass: the size and the files at its start and at its end, how
        /// often it was read and how often not whole, how often cleared and how many files that
        /// removed, the linked folders inside it left unwalked, and the seconds all of it took. Or
        /// that the clearing is off, or the folder is not there.
        /// </summary>
        public static string PassLine(
            string folder, long limitBytes, bool folderThere, long atStart, int filesAtStart, long atEnd, int filesAtEnd,
            int reads, int notWhole, int clearings, int removed, int linksLeft, double seconds)
        {
            if (limitBytes <= 0)
            {
                return "the texture cache is not checked, its limit is off";
            }

            if (!folderThere)
            {
                return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " is not there, so nothing was cleared";
            }

            return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " held " + Size(atStart) + " in "
                + Words.Counted(filesAtStart, "file", "files") + " as the pictures began and " + Size(atEnd) + " in "
                + Words.Counted(filesAtEnd, "file", "files") + " as they ended, read "
                + Words.Counted(reads, "time", "times")
                + (notWhole > 0 ? ", " + notWhole.ToString(CultureInfo.InvariantCulture) + " of them not whole" : string.Empty)
                + " against the limit of " + Size(limitBytes) + ", cleared " + Words.Counted(clearings, "time", "times")
                + (clearings > 0 ? " removing " + Words.Counted(removed, "file", "files") : string.Empty)
                + (linksLeft > 0 ? ", " + Words.Counted(linksLeft, "linked folder", "linked folders") + " inside it left unwalked" : string.Empty)
                + ", " + Seconds(seconds) + " in all";
        }

        private static string Seconds(double seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture) + " s";
        }
    }
}
