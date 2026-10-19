using System.IO;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// Where the NWF is copied before a refresh, F187, Bader's order of 14 Oct 2026, task 7.4, a Backup folder beside the Log
    /// folder. Since F243, Bader's order of 19 Oct 2026, "no more internal or dated named files": ONE copy per NWF under the
    /// NWF's own name, Backup\NAME.nwf, with no moment in it, overwritten by each refresh, the default until he answers Q217.
    /// The dated copies earlier runs left are cleaned at the start of a run, Communities.OldReportFiles. The NWF folder keeps
    /// only the NWF and the NWD, so the copy does not go there.
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

        /// <summary>The path of the copy, the NWF's own file name in the Backup folder, or null where either is missing.</summary>
        public static string PathFor(string nwfPath, string backupFolder)
        {
            if (string.IsNullOrEmpty(nwfPath) || string.IsNullOrEmpty(backupFolder))
            {
                return null;
            }

            return Path.Combine(backupFolder, Path.GetFileName(nwfPath));
        }
    }
}
