using System;
using System.Globalization;
using System.IO;
using Federator.Core.Diagnostics;

namespace Federator.Core.Report
{
    /// <summary>
    /// Navisworks' texture cache, the folder ogs under the user's temp folder, F147, Bader's
    /// order of 2026-10-11, Q152 item 2: between tests, cleared once it passes the limit, its
    /// size logged before and after. On this laptop it held no file when read on 2026-10-12,
    /// docs\history\scan.md 5z-zd, so this guards the long runs of other machines rather than a
    /// cost measured here.
    ///
    /// It is checked between two tests' pictures at most once a check interval, a setting, so a
    /// large cache is not walked between every two of 741 tests. The seconds of such a walk are
    /// UNKNOWN until a machine with a large cache runs it, and the pass line says them. The limit
    /// is a setting too. A size not read whole is UNKNOWN and never clears anything. Only a fully
    /// qualified folder named ogs is ever walked, so a wrong path can never empty another folder,
    /// and every line names the folder walked.
    /// </summary>
    public static class TextureCache
    {
        /// <summary>The folder under the user's temp folder, Bader's words of 2026-10-11.</summary>
        public const string FolderUnderTemp = "ogs";

        /// <summary>Five gigabytes, counted as Windows counts them, 1024 cubed bytes each.</summary>
        public const long DefaultLimitBytes = 5L * 1024 * 1024 * 1024;

        /// <summary>The least seconds between two checks of the cache.</summary>
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
        /// Whether a clearing left the cache not under the limit, past it or not read whole, so it is
        /// not cleared again in the pass: files held open stay, and a clearing that cannot bring the
        /// cache under the limit is not walked and refused again once a minute for hours.
        /// </summary>
        public static bool StillPast(long after, long limitBytes)
        {
            return limitBytes > 0 && (after < 0 || after > limitBytes);
        }

        /// <summary>
        /// Whether a path may be walked and cleared as the cache: fully qualified, the same path once
        /// made full, and its last part ogs. A null, empty, relative or drive relative path, the temp
        /// folder itself or any other folder is refused, so a caller handing the wrong path can never
        /// empty it.
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

                if (!string.Equals(Path.GetFullPath(trimmed), trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return string.Equals(Path.GetFileName(trimmed), FolderUnderTemp, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception error)
            {
                if (error is ArgumentException || error is NotSupportedException || error is PathTooLongException
                    || error is System.Security.SecurityException)
                {
                    return false;
                }

                throw;
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
            return bytes < Kilobyte
                ? Size(bytes)
                : Size(bytes) + " (" + bytes.ToString(CultureInfo.InvariantCulture) + " bytes)";
        }

        /// <summary>A count of files, or words saying it is UNKNOWN where the read that counts them was not whole.</summary>
        public static string Files(int count)
        {
            return count < 0 ? "an UNKNOWN number of files" : Words.Counted(count, "file", "files");
        }

        /// <summary>
        /// The line of one clearing: the size and the files before, what was removed, what could not
        /// be, what had gone before its turn, and the size after, read again. A clearing that leaves the
        /// cache still past the limit says it is not cleared again in this pass.
        /// </summary>
        public static string ClearedLine(
            string folder, long before, int filesBefore, long limitBytes, int removed, int notRemoved, int goneBefore,
            long after, int filesAfter, double seconds)
        {
            bool stillPast = StillPast(after, limitBytes);

            return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " held " + Exactly(before) + " in "
                + Files(filesBefore) + ", past the limit of " + Exactly(limitBytes) + ", so "
                + Words.Counted(removed, "file was", "files were") + " removed"
                + (notRemoved > 0 ? ", " + Words.Counted(notRemoved, "file", "files") + " could not be removed" : string.Empty)
                + (goneBefore > 0 ? ", " + Words.Counted(goneBefore, "file", "files") + " had gone before its turn" : string.Empty)
                + ", and it holds " + Exactly(after) + " in " + Files(filesAfter) + " now, "
                + Seconds(seconds) + " for the clearing"
                + (stillPast ? ", and since it is not under the limit it is not cleared again in this pass" : string.Empty);
        }

        /// <summary>The line of the first check whose walk did not finish, so nothing was cleared at it. Later ones are counted on the pass line.</summary>
        public static string NotReadWholeLine(string folder, string why)
        {
            return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " could not be read whole, " + Words.Or(why, "UNKNOWN")
                + ", so its size is UNKNOWN and nothing was cleared at that check, and any later check like it is counted on the pass line";
        }

        /// <summary>The line of a folder refused before anything is walked.</summary>
        public static string RefusedLine(string folder)
        {
            return string.IsNullOrEmpty(folder)
                ? "the texture cache is not checked, no folder was handed for it, so nothing is walked or removed"
                : "the texture cache is not checked, " + folder + " is not a fully qualified folder named " + FolderUnderTemp
                    + ", so nothing is walked or removed";
        }

        /// <summary>The line of a cache folder that is itself a link to another folder, never walked.</summary>
        public static string LinkedLine(string folder)
        {
            return "the texture cache at " + Words.Or(folder, "UNKNOWN")
                + " is a link to another folder, so it is not walked and nothing is removed";
        }

        /// <summary>
        /// The one line of a picture pass: the size and the files at its start and at its end, the
        /// checks against the limit and those not read whole, the clearings and the files they
        /// removed, the linked folders inside it left unwalked, and the seconds all of it took. Or that
        /// the clearing is off, or the folder was never there.
        /// </summary>
        public static string PassLine(
            string folder, long limitBytes, bool everThere, long atStart, int filesAtStart, long atEnd, int filesAtEnd,
            int checks, int notWhole, int clearings, int removed, int linksLeft, int foldersGone, double seconds)
        {
            if (limitBytes <= 0)
            {
                return "the texture cache is not checked, its limit is off";
            }

            if (!everThere && notWhole == 0 && clearings == 0)
            {
                return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " is not there, so nothing was cleared";
            }

            return "the texture cache at " + Words.Or(folder, "UNKNOWN") + " held " + Size(atStart) + " in "
                + Files(filesAtStart) + " as the pictures began and " + Exactly(atEnd) + " in "
                + Files(filesAtEnd) + " as they ended, checked " + Words.Counted(checks, "time", "times")
                + " against the limit of " + Exactly(limitBytes)
                + (notWhole > 0 ? ", " + Words.Counted(notWhole, "read", "reads") + " not whole" : string.Empty)
                + ", cleared " + Words.Counted(clearings, "time", "times")
                + (clearings > 0 ? " removing " + Words.Counted(removed, "file", "files") : string.Empty)
                + (linksLeft > 0 ? ", " + Words.Counted(linksLeft, "linked folder", "linked folders") + " inside it left unwalked" : string.Empty)
                + (foldersGone > 0 ? ", " + Words.Counted(foldersGone, "folder", "folders") + " inside it gone while walked" : string.Empty)
                + ", " + Seconds(seconds) + " in all";
        }

        private static string Seconds(double seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture) + " s";
        }
    }
}
