using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Diagnostics;
using Federator.Core.Report;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// What to call the outputs when the work is a document somebody already has open.
    ///
    /// WHY THIS EXISTS. Running a federation that already exists meant picking a source
    /// folder, waiting for a scan, choosing a grouping and ticking the one group back out
    /// of twenty two, all so the tool could work out a name it could have read off the
    /// file that was open in front of it. In Navisworks a person opens a file, opens Clash
    /// Detective, presses Run and reads the results. Nothing asks them where their models
    /// came from. This is what lets the tool do the same.
    ///
    /// The rule is one line: the outputs are named after the OPEN FILE, in its own folder,
    /// with the extension swapped. An NWF at
    ///   D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf
    /// gives
    ///   D:\Federations\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd
    ///   D:\Federations\Clash Reports\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.xlsx
    ///
    /// which is exactly where the scanned path puts them, so a building run this way and a
    /// building run the other way write the same files. No pattern is applied and no field
    /// is supplied, because the name is already decided: it is on the file.
    ///
    /// There are no Navisworks types in here, so all of it is tested.
    /// </summary>
    public static class OpenDocumentJob
    {
        /// <summary>What a document with no file name behind it is called.</summary>
        public const string Unsaved = "Untitled";

        /// <summary>
        /// The name the outputs take, read off the open document's path. An unsaved
        /// document has no path, and its outputs cannot be named after it, so it is named
        /// and refused rather than written somewhere surprising.
        /// </summary>
        public static string NameFrom(string openPath)
        {
            if (string.IsNullOrEmpty(openPath))
            {
                return Unsaved;
            }

            string name = Path.GetFileNameWithoutExtension(openPath);

            return string.IsNullOrEmpty(name) ? Unsaved : name;
        }

        /// <summary>
        /// True where this document can be run at all. Five things are checked in order
        /// and WhyNot names the first that fails, in the words a person would say:
        ///   it has a name, because an unsaved document has nowhere to put an NWD or a
        ///     workbook, and saving it somewhere of our choosing would be this tool
        ///     deciding where a person's federation lives
        ///   it was opened from a folder and not from an address, because acc:// and
        ///     https:// name nothing on a disk
        ///   it is an NWF, because the NWF is where the clash tests and their results
        ///     live, and an NWD or an NWC opened directly holds neither
        ///   it has a folder in front of its name, because that is where the outputs go
        ///   that folder can be read from here
        /// D2, decided on 2026-09-12. An NWD opened directly used to be allowed, and the
        /// NWD this tool publishes would have been written over the file that was open.
        /// </summary>
        public static bool CanRun(string openPath)
        {
            return CanRun(openPath, Directory.Exists);
        }

        /// <summary>
        /// The same rule with the disk read handed in, so every reason can be proved
        /// without a folder that exists on the machine running the tests.
        /// </summary>
        public static bool CanRun(string openPath, Func<string, bool> folderReadable)
        {
            return WhyNot(openPath, folderReadable).Length == 0;
        }

        /// <summary>Why it cannot, in the words a person would say. Empty where it can.</summary>
        public static string WhyNot(string openPath)
        {
            return WhyNot(openPath, Directory.Exists);
        }

        /// <summary>The same, with the disk read handed in.</summary>
        public static string WhyNot(string openPath, Func<string, bool> folderReadable)
        {
            if (folderReadable == null)
            {
                throw new ArgumentNullException("folderReadable");
            }

            if (string.IsNullOrEmpty(openPath)
                || string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(openPath)))
            {
                return "This document has not been saved anywhere, so there is nowhere to put "
                    + "the NWD and the report beside it. Save it first.";
            }

            if (openPath.IndexOf("://", StringComparison.Ordinal) >= 0)
            {
                return "This document was opened from " + openPath + ", which is an address "
                    + "and not a folder on a disk, so there is nowhere to put the NWD and the "
                    + "report beside it. Open it from a folder this machine can read.";
            }

            string extension = ExtensionOf(openPath);

            if (!string.Equals(extension, ".nwf", StringComparison.OrdinalIgnoreCase))
            {
                return "This document is " + DescribeExtension(extension) + " and this tool "
                    + "runs an NWF, which is where the clash tests and their results live. "
                    + "Open the NWF instead.";
            }

            string folder = FolderOf(openPath);

            if (folder.Length == 0)
            {
                return "This document, " + openPath + ", has no folder in front of its name, "
                    + "so there is nowhere to put the NWD and the report beside it. Open it "
                    + "from a folder this machine can read.";
            }

            if (!folderReadable(folder))
            {
                return "The folder this document was opened from, " + folder + ", cannot be "
                    + "read from here, so there is nowhere to put the NWD and the report "
                    + "beside it. Open it from a folder this machine can read.";
            }

            return string.Empty;
        }

        private static string ExtensionOf(string openPath)
        {
            try
            {
                return Path.GetExtension(openPath) ?? string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        private static string DescribeExtension(string extension)
        {
            return extension.Length == 0
                ? "a file with no extension"
                : "a " + extension.ToLowerInvariant() + " file";
        }

        /// <summary>
        /// The NWD, beside the open file with the extension swapped. Never the open file
        /// itself. Where the swap lands on the same path, read case blind because Windows
        /// reads file names that way, there is no NWD to name and empty comes back, so
        /// nothing can be published over the file that is open. CanRun refuses such a
        /// file before anything is written, and this is the second lock on the same door.
        /// </summary>
        public static string NwdBeside(string openPath)
        {
            string nwd = Beside(openPath, ".nwd");

            return string.Equals(nwd, openPath, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : nwd;
        }

        /// <summary>
        /// Where the reports go for the open file. ONE rule for both ways of running:
        /// the same <see cref="ReportPaths.Choose"/> the scanned run uses, handed the
        /// open file's own folder where the scanned run hands the NWF folder. A picked
        /// Excel folder wins, and with none picked the reports go in the Clash Reports
        /// subfolder beside the file.
        ///
        /// WHY THIS REPLACED ReportFolderBeside. That method built the Clash Reports
        /// folder here, and the window then handed that folder to the engine as if it
        /// were the NWF folder, so the engine built Clash Reports beside it again and
        /// wrote to Clash Reports\Clash Reports. The folder is now decided once, here,
        /// and nothing downstream adds to it.
        ///
        /// No source folder goes in, because nothing was scanned. The refusal to write
        /// inside the folder being scanned guards a scan, and an open file has none. The
        /// scan's folder box may still hold last time's folder, and applying it here once
        /// refused a run for a scan that never happened.
        /// </summary>
        public static ReportFolderChoice ReportFolder(string openPath, string pickedExcelFolder)
        {
            string folder = FolderOf(openPath);

            if (folder.Length == 0)
            {
                return new ReportFolderChoice(string.Empty, null);
            }

            return ReportPaths.Choose(pickedExcelFolder, folder, null);
        }

        private static string Beside(string openPath, string extension)
        {
            string folder = FolderOf(openPath);

            if (folder.Length == 0)
            {
                return string.Empty;
            }

            return Path.Combine(folder, NameFrom(openPath) + extension);
        }

        /// <summary>
        /// The open file's own folder, or empty where the path has none. This is where the
        /// NWD goes, where the Clash Reports folder goes, and where the second copy of the
        /// log goes, the same as the NWF folder on a scanned run.
        /// </summary>
        public static string FolderOf(string openPath)
        {
            if (string.IsNullOrEmpty(openPath))
            {
                return string.Empty;
            }

            try
            {
                string folder = Path.GetDirectoryName(openPath);
                return folder ?? string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        /// <summary>The title of the block written when an open file run finishes.</summary>
        public const string SummaryTitle = "OPEN FILE";

        /// <summary>
        /// The block written when an open file run finishes, just before the RESULT block.
        /// A scanned run has RUN SETTINGS and GROUPS blocks that name its folders and its
        /// groups, and the open file run had nothing, so the RESULT block that followed
        /// counted a group nobody could see. These are the same fields where they apply,
        /// and the ones that do not apply say so rather than being left blank.
        /// </summary>
        public static IList<string> SummaryLines(
            string openPath,
            string clashFile,
            string nwdPath,
            string reportFolder,
            string clashSummary,
            GroupOutcome outcome,
            string reason)
        {
            List<string> lines = new List<string>();

            lines.Add("file          : " + Words.Or(openPath, Unsaved + ", nothing to run"));
            lines.Add("decision      : opened file, no Decide, nothing appended and nothing cleared");
            lines.Add("clash file    : " + Words.Or(clashFile, "none picked"));
            lines.Add("clash         : " + Words.Or(clashSummary, "no clash step ran"));
            lines.Add("NWD           : " + Words.Or(nwdPath, "not written, the file has no folder"));
            lines.Add("report folder : " + Words.Or(reportFolder, "not worked out, the file has no folder"));
            lines.Add("source folder : not applicable, nothing was scanned");
            lines.Add("grouping      : not applicable, the open file is the one group");
            lines.Add("files ticked  : not applicable, the models are the ones inside the file");
            lines.Add("outcome       : " + outcome.ToString().ToUpperInvariant()
                + (string.IsNullOrEmpty(reason) ? string.Empty : ", " + reason));

            return lines;
        }


        /// <summary>
        /// What the window says the run will do, in one line, so a person can read it
        /// before pressing anything.
        /// </summary>
        internal static string Describe(string openPath, string pickedExcelFolder)
        {
            return Describe(openPath, pickedExcelFolder, false);
        }

        /// <summary>
        /// The same line, led by which workflow this is. The open file is always a Weekly
        /// run, because the NWF already exists and is the document, and there is no First
        /// run on this path. An XML picked makes it a Weekly run plus XML.
        /// </summary>
        public static string Describe(string openPath, string pickedExcelFolder, bool xmlPicked)
        {
            if (!CanRun(openPath))
            {
                return WhyNot(openPath);
            }

            return RunPath.Label(RerunDecision.Open, xmlPicked) + ". Runs on " + NameFrom(openPath)
                + ", writes " + NwdBeside(openPath)
                + " and the report in "
                + ReportFolder(openPath, pickedExcelFolder).Folder + ".";
        }
    }
}
