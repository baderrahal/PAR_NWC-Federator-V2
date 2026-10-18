using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Federator.Core.Diagnostics;
using Federator.Core.Generic;
using Federator.Core.Report;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// How the pick reads the disk, handed in so every reason can be proved without a folder
    /// on the machine running the tests. The add-in hands <see cref="Real"/>.
    /// </summary>
    public sealed class NwfPickDisk
    {
        public NwfPickDisk(
            Func<string, bool> isFile,
            Func<string, bool> isFolder,
            Func<string, IList<string>> filesIn,
            Func<string, IList<string>> foldersIn)
        {
            if (isFile == null)
            {
                throw new ArgumentNullException("isFile");
            }

            if (isFolder == null)
            {
                throw new ArgumentNullException("isFolder");
            }

            if (filesIn == null)
            {
                throw new ArgumentNullException("filesIn");
            }

            if (foldersIn == null)
            {
                throw new ArgumentNullException("foldersIn");
            }

            IsFile = isFile;
            IsFolder = isFolder;
            FilesIn = filesIn;
            FoldersIn = foldersIn;
        }

        public Func<string, bool> IsFile { get; private set; }

        public Func<string, bool> IsFolder { get; private set; }

        /// <summary>The files directly in one folder, never its subfolders.</summary>
        public Func<string, IList<string>> FilesIn { get; private set; }

        /// <summary>The folders directly in one folder.</summary>
        public Func<string, IList<string>> FoldersIn { get; private set; }

        /// <summary>
        /// The disk itself. A folder is listed with no pattern, so nothing is matched by a
        /// wildcard and the extension is judged in one place, <see cref="NwfPick.IsNwf"/>.
        /// </summary>
        public static NwfPickDisk Real
        {
            get
            {
                return new NwfPickDisk(
                    File.Exists,
                    Directory.Exists,
                    folder => Directory.GetFiles(folder),
                    folder => Directory.GetDirectories(folder));
            }
        }
    }

    /// <summary>
    /// The words of an NWF of the NWF page, F129 and F188, Bader's request 4 under Q112, each run the way the open
    /// file run runs it, since F232 every NWF ticked in the tree the page reads off the Report folder.
    ///
    /// WHY THE OPEN FILE RUN'S ROUTE. Read off the code on 2026-10-08: the scanned run reaches an
    /// NWF only through Decide, which compares its file list against a scan, and there is no
    /// scan here. The open file run already runs an NWF where it sits with no scan and no
    /// Decide, through FinishTheGroup, the tail both routes share. So each picked NWF is opened
    /// and then handed to that route, and its checks, its names and its guard are the ones used
    /// here, read through <see cref="OpenDocumentJob"/> and never copied.
    ///
    /// There are no Navisworks types in here, so all of it is tested.
    /// </summary>
    public static class NwfPick
    {
        /// <summary>The block written for each picked NWF once the run ends, in the shape of OPEN FILE.</summary>
        public const string SummaryTitle = "PICKED NWF";

        /// <summary>The first OPEN line of a picked NWF's group, where the open file run says it runs what is open.</summary>
        public const string RunningLine = "OPEN     running a picked NWF, opened by this run where it sits, no scan";

        /// <summary>
        /// The phrase of the DECIDE step around a picked NWF's open, F129 attempt 2: opened and
        /// read empty, not opened, or the label of the run it goes on to, so the step says what
        /// the open did and never reads Weekly run over an open that stopped the group.
        /// </summary>
        public static string DecidePhrase(bool readEmpty, bool failed, bool xmlPicked)
        {
            if (readEmpty)
            {
                return "opened and read empty";
            }

            return failed ? "not opened" : RunPath.Label(RerunDecision.Open, xmlPicked);
        }

        /// <summary>True for a file whose extension is .nwf, read case blind, and for no other.</summary>
        public static bool IsNwf(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                return string.Equals(Path.GetExtension(path), ".nwf", StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                // A name with a character no path may hold has no extension to read, and the
                // open file run's checks name that character if it is ever picked as a file.
                return path.EndsWith(".nwf", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>The reason a picked NWF that would not open stops its group.</summary>
        public static string WouldNotOpen(string nwfPath)
        {
            return "The NWF at " + nwfPath + " would not open, so nothing was run on it.";
        }

        /// <summary>
        /// The reason a picked NWF that opened and reported no models stops its group, F74's
        /// refusal on this route. Never run, because every test would pass against an empty
        /// document, which is a wrong answer that reads as a good one.
        /// </summary>
        public static string ReadEmpty(string nwfPath, double secondsWaited)
        {
            return "the NWF at " + nwfPath + " opened with no error and then reported no models at all, after waiting "
                + secondsWaited.ToString("0.0", CultureInfo.InvariantCulture) + "s. Nothing was run on it, because "
                + "every test would pass against an empty document. Open it in Navisworks and look: one that opens "
                + "with its models in it is a loading fault to report";
        }
    }
}
