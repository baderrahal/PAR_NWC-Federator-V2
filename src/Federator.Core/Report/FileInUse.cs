using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// A file the tool cannot write over because another program holds it open, F243, Bader's order of 19 Oct 2026 point 6:
    /// "When a file cannot be overwritten because it is open or locked, do not write another name. Skip that one file, log
    /// Close FULL PATH and run the report again, put the same line in the run window's end message, and carry on with the
    /// rest of the run."
    ///
    /// OPEN IS READ OFF THE EXCEPTION THE WRITE GAVE, never guessed from a file's age or a lock file beside it: an IOException
    /// whose HRESULT carries ERROR_SHARING_VIOLATION, 32, or ERROR_LOCK_VIOLATION, 33, the two codes Windows gives a write or a
    /// delete of a file another process holds, Excel holding its xlsx among them. Any other failure is not this and stays a
    /// failure of its own. An exception wrapping one, as a library may throw, is read through its inner exceptions.
    /// </summary>
    public static class FileInUse
    {
        /// <summary>The Windows error a write or a delete gives where another process holds the file without sharing it.</summary>
        internal const int SharingViolation = 32;

        /// <summary>The Windows error where another process holds a lock on part of the file.</summary>
        internal const int LockViolation = 33;

        private const string Start = "Close ";

        private const string End = " and run the report again";

        /// <summary>True where the exception, or one it wraps, is a write or a delete refused because another program holds the file.</summary>
        public static bool IsInUse(Exception error)
        {
            for (Exception next = error; next != null; next = next.InnerException)
            {
                IOException io = next as IOException;

                if (io == null)
                {
                    continue;
                }

                int code = io.HResult & 0xFFFF;

                if (code == SharingViolation || code == LockViolation)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The one line, in the log and in the run window's end message, Bader's words.</summary>
        public static string Line(string path)
        {
            return Start + (path ?? string.Empty) + End;
        }

        /// <summary>
        /// The line inside a line of the log, from Close to its end, so the line a group's own Navisworks wrote in its RESULT
        /// is found again behind the time stamp the log puts in front of it. Null where the line holds none.
        /// </summary>
        public static string LineIn(string logLine)
        {
            if (string.IsNullOrEmpty(logLine) || !logLine.EndsWith(End, StringComparison.Ordinal))
            {
                return null;
            }

            int at = logLine.IndexOf(Start, StringComparison.Ordinal);

            return at < 0 || at + Start.Length >= logLine.Length - End.Length ? null : logLine.Substring(at);
        }

        /// <summary>
        /// The run window's end message: what it already says, then one line for each file left open, each once in the order
        /// first met. Null where it has nothing to say.
        /// </summary>
        /// <summary>
        /// The lines RESULT gives the files left open, a group's own and, attempt 2 on the reviewer's N4, the window's that
        /// started a run of packages in its joined RESULT: one count line and each file's line, or none where none was left.
        /// </summary>
        public static IList<string> ResultLines(IList<string> closeLines)
        {
            List<string> lines = new List<string>();

            if (closeLines == null || closeLines.Count == 0)
            {
                return lines;
            }

            lines.Add("files open     : " + closeLines.Count + " not written, each was open in another program");
            lines.AddRange(closeLines);
            return lines;
        }

        public static string EndMessage(string already, IEnumerable<string> closeLines)
        {
            List<string> lines = new List<string>();

            foreach (string line in closeLines ?? new List<string>())
            {
                if (!string.IsNullOrEmpty(line) && !lines.Contains(line))
                {
                    lines.Add(line);
                }
            }

            if (lines.Count == 0)
            {
                return string.IsNullOrEmpty(already) ? null : already;
            }

            string files = (lines.Count == 1 ? "A file was" : lines.Count + " files were") + " open in another program and not written:"
                + Environment.NewLine + string.Join(Environment.NewLine, lines.ToArray());

            return string.IsNullOrEmpty(already) ? files : already + Environment.NewLine + Environment.NewLine + files;
        }
    }
}
