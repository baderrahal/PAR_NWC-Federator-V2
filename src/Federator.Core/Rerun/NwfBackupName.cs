using System;
using System.Globalization;
using System.IO;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// Where the NWF is copied before a refresh, F187, Bader's order of 14 Oct 2026, task 7.4. A Backup folder beside
    /// the Log folder, one timestamped copy a run, and an older backup is NEVER overwritten: a name already taken
    /// gets a counter. The NWF folder keeps only the NWF and the NWD, so the copy does not go there.
    /// </summary>
    public static class NwfBackupName
    {
        /// <summary>The folder name, beside the Log folder.</summary>
        public const string FolderName = "Backup";

        /// <summary>The Backup folder beside the Log folder, or null where there is no Log folder to be beside.</summary>
        public static string FolderBeside(string logFolder)
        {
            if (string.IsNullOrEmpty(logFolder))
            {
                return null;
            }

            string parent = Path.GetDirectoryName(logFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            return string.IsNullOrEmpty(parent) ? null : Path.Combine(parent, FolderName);
        }

        /// <summary>
        /// The path of the copy: the NWF's name, the moment of the run to the second, and a counter where that name
        /// is taken. Never a name that exists, so nothing is overwritten.
        /// </summary>
        public static string PathFor(string nwfPath, string backupFolder, DateTime now, Func<string, bool> exists)
        {
            if (string.IsNullOrEmpty(nwfPath) || string.IsNullOrEmpty(backupFolder))
            {
                return null;
            }

            string name = Path.GetFileNameWithoutExtension(nwfPath);
            string extension = Path.GetExtension(nwfPath);
            string stamp = now.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture);
            string first = Path.Combine(backupFolder, name + " " + stamp + extension);

            if (exists == null || !exists(first))
            {
                return first;
            }

            for (int counter = 2; counter < 10000; counter++)
            {
                string next = Path.Combine(backupFolder, name + " " + stamp + " (" + counter.ToString(CultureInfo.InvariantCulture) + ")" + extension);

                if (!exists(next))
                {
                    return next;
                }
            }

            return null;
        }
    }
}
