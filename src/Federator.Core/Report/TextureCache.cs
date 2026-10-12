using System.Globalization;
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
    /// settings on ImageOptions. A size that cannot be read is UNKNOWN and never clears anything.
    /// </summary>
    public static class TextureCache
    {
        /// <summary>The folder under the user's temp folder, Bader's words of 2026-10-11.</summary>
        public const string FolderUnderTemp = "ogs";

        /// <summary>Five gigabytes, counted as Windows counts them, 1024 cubed bytes each.</summary>
        public const long DefaultLimitBytes = 5L * 1024 * 1024 * 1024;

        /// <summary>The least seconds between two measurements of the cache.</summary>
        public const int DefaultCheckEverySeconds = 60;

        private const double BytesInAGigabyte = 1024d * 1024 * 1024;
        private const double BytesInAMegabyte = 1024d * 1024;

        /// <summary>Whether a check is due: the first one always, then once the interval has passed. Zero or less checks every time.</summary>
        public static bool DueToMeasure(bool measuredBefore, double secondsSinceLast, int everySeconds)
        {
            return !measuredBefore || everySeconds <= 0 || secondsSinceLast >= everySeconds;
        }

        /// <summary>
        /// Whether to clear it: only a size read and past the limit. A limit of zero or less
        /// switches the clearing off, and an unread size, below zero, never clears anything.
        /// </summary>
        public static bool ShouldClear(long bytes, long limitBytes)
        {
            return limitBytes > 0 && bytes >= 0 && bytes > limitBytes;
        }

        /// <summary>A size in words, gigabytes from one up and megabytes below, UNKNOWN when it was not read.</summary>
        public static string Size(long bytes)
        {
            if (bytes < 0)
            {
                return "UNKNOWN";
            }

            return bytes >= BytesInAGigabyte
                ? (bytes / BytesInAGigabyte).ToString("0.00", CultureInfo.InvariantCulture) + " GB"
                : (bytes / BytesInAMegabyte).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }

        /// <summary>The line of one clearing, the size before and after and what could not be removed.</summary>
        public static string ClearedLine(long before, int filesBefore, long limitBytes, int removed, int left, long after)
        {
            return "the texture cache under %TEMP%\\" + FolderUnderTemp + " held " + Size(before) + " in "
                + Words.Counted(filesBefore, "file", "files") + ", past the limit of " + Size(limitBytes) + ", so "
                + Words.Counted(removed, "file was", "files were") + " removed"
                + (left > 0 ? " and " + Words.Counted(left, "file was", "files were") + " in use and left" : string.Empty)
                + ", and it holds " + Size(after) + " now";
        }

        /// <summary>
        /// The one line of a picture pass: the size at its start and at its end, how often it was
        /// measured and cleared, or that the clearing is off.
        /// </summary>
        public static string PassLine(long limitBytes, long atStart, long atEnd, int measured, int cleared, bool folderThere)
        {
            if (limitBytes <= 0)
            {
                return "the texture cache under %TEMP%\\" + FolderUnderTemp + " is not checked, its limit is off";
            }

            if (!folderThere)
            {
                return "the texture cache under %TEMP%\\" + FolderUnderTemp + " is not there, so nothing was cleared";
            }

            return "the texture cache under %TEMP%\\" + FolderUnderTemp + " held " + Size(atStart) + " as the pictures began and "
                + Size(atEnd) + " as they ended, measured " + Words.Counted(measured, "time", "times")
                + " against the limit of " + Size(limitBytes) + " and cleared " + Words.Counted(cleared, "time", "times");
        }
    }
}
