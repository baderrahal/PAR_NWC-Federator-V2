using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Report;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The run log and its .tsv copied into the Log folder when a run ends, F158. The log has to
    /// open before any folder is known, on the first line of the button handler, so it is written
    /// in the logs folder under the user's local data and is copied here once the run is over.
    /// The original stays where it was, so a run that never reaches this still leaves its log in
    /// the one place it always did.
    ///
    /// Nothing in the Log folder is replaced or deleted. A name already there takes the moment of
    /// the copy after it. Never throws, and a file that cannot be copied is said in a line and costs
    /// nothing else.
    /// </summary>
    public static class LogCopy
    {
        /// <summary>
        /// Copies the log and the .tsv beside it. Returns one line per file, saying where it went or
        /// why it did not, for the log to carry.
        /// </summary>
        public static IList<string> Into(string logFolder, string logPath, string tsvPath, DateTime now)
        {
            List<string> lines = new List<string>();

            if (string.IsNullOrEmpty(logFolder))
            {
                lines.Add("LOGS     the run log was not copied, there is no Log folder for this run");
                return lines;
            }

            One(lines, logFolder, logPath, now);
            One(lines, logFolder, tsvPath, now);
            return lines;
        }

        private static void One(List<string> lines, string folder, string source, DateTime now)
        {
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            try
            {
                if (!File.Exists(source))
                {
                    lines.Add("LOGS     not copied, " + source + " is not on the disk");
                    return;
                }

                Directory.CreateDirectory(folder);
                string target = ReportPaths.FreeFilePath(folder, Path.GetFileName(source), now, null);

                // The log is still open for writing in the run that is ending, so it is read with
                // the sharing it was opened with. Written with CreateNew, so a file that appeared
                // between the check and the write is never replaced.
                using (FileStream from = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (FileStream to = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                {
                    from.CopyTo(to);
                }

                lines.Add("LOGS     copied " + source + " to " + target + ", " + new FileInfo(target).Length + " bytes");
            }
            catch (Exception error)
            {
                if (error is OutOfMemoryException)
                {
                    throw;
                }

                lines.Add("LOGS     not copied, " + source + " to " + folder + ": " + error.GetType().Name + ": " + error.Message);
            }
        }
    }
}
