using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Federator.Core.Diagnostics;
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

    /// <summary>One NWF the pick gave, with what it writes and whether it can run.</summary>
    public sealed class PickedNwf
    {
        internal PickedNwf(string path, string whyNot, string pickedExcelFolder)
        {
            Path = path;
            WhyNot = whyNot ?? string.Empty;
            Name = string.Empty;
            NwdPath = string.Empty;
            ReportFolder = string.Empty;
            WorkbookPath = string.Empty;

            // Only where the checks pass, because a path they refuse can hold a character the
            // path methods of .NET Framework throw on, FR-173, and a refused NWF writes nothing.
            if (CanRun)
            {
                Name = OpenDocumentJob.NameFrom(path);
                NwdPath = OpenDocumentJob.NwdBeside(path);
                ReportFolder = OpenDocumentJob.ReportFolder(path, pickedExcelFolder).Folder;
                WorkbookPath = ReportFolder.Length == 0 ? string.Empty : ReportPaths.Workbook(ReportFolder, Name);
            }
        }

        public string Path { get; private set; }

        /// <summary>The name the outputs take, read off the file as the open file run reads it. Empty where it cannot run.</summary>
        public string Name { get; private set; }

        /// <summary>Beside the NWF with the extension swapped, OpenDocumentJob.NwdBeside. Empty where it cannot run.</summary>
        public string NwdPath { get; private set; }

        /// <summary>Clash Reports beside the NWF, or the Excel folder picked, OpenDocumentJob.ReportFolder. Empty where it cannot run.</summary>
        public string ReportFolder { get; private set; }

        /// <summary>The workbook it writes. Empty where it cannot run.</summary>
        public string WorkbookPath { get; private set; }

        /// <summary>Why the open file run's checks refuse it, in their words. Empty where it can run.</summary>
        public string WhyNot { get; private set; }

        public bool CanRun
        {
            get { return WhyNot.Length == 0; }
        }
    }

    /// <summary>
    /// What one pick decides, F129: the NWFs it gives in the order they run, which of them the
    /// open file run's checks refuse, the folders that would not read, and whether the run can
    /// start at all.
    /// </summary>
    public sealed class NwfPickPlan
    {
        private readonly List<PickedNwf> nwfs = new List<PickedNwf>();
        private readonly List<string> problems = new List<string>();

        internal NwfPickPlan(string picked, bool pickedAFolder, bool subfolders)
        {
            Picked = picked ?? string.Empty;
            PickedAFolder = pickedAFolder;
            Subfolders = subfolders;
            WhyNoRun = string.Empty;
            WhyNoRunForALabel = string.Empty;
        }

        public string Picked { get; private set; }

        public bool PickedAFolder { get; private set; }

        public bool Subfolders { get; private set; }

        public IList<PickedNwf> Nwfs
        {
            get { return nwfs.AsReadOnly(); }
        }

        /// <summary>Each folder that would not read, with what the disk said, for the log.</summary>
        public IList<string> Problems
        {
            get { return problems.AsReadOnly(); }
        }

        /// <summary>
        /// Why the run cannot start, for the log and the failure dialog, which may carry what the
        /// disk said. Empty where it can.
        /// </summary>
        public string WhyNoRun { get; private set; }

        /// <summary>The same reason with nothing the disk said in it, since a label never carries a framework message.</summary>
        private string WhyNoRunForALabel { get; set; }

        public bool CanStart
        {
            get { return WhyNoRun.Length == 0; }
        }

        public int Runnable
        {
            get { return nwfs.FindAll(n => n.CanRun).Count; }
        }

        public int Refused
        {
            get { return nwfs.Count - Runnable; }
        }

        /// <summary>
        /// Every NWF the pick gave, in the order they run. A refused one is handed on as well,
        /// because the engine judges each by the same checks before any open and counts a refused
        /// one a failed group, so RESULT carries it.
        /// </summary>
        public IList<string> Paths()
        {
            return nwfs.ConvertAll(n => n.Path);
        }

        /// <summary>Where the second copy of the log goes: the folder picked, or the picked file's own folder.</summary>
        public string LogFolder
        {
            get { return PickedAFolder ? Picked : OpenDocumentJob.FolderOf(Picked); }
        }

        internal void Add(PickedNwf nwf)
        {
            nwfs.Add(nwf);
        }

        internal void Problem(string problem)
        {
            problems.Add(problem);
        }

        internal void Refuse(string forTheLog, string forALabel)
        {
            if (WhyNoRun.Length > 0)
            {
                return;
            }

            WhyNoRun = forTheLog;
            WhyNoRunForALabel = forALabel;
        }

        /// <summary>
        /// The RUN SETTINGS lines of a run of picked NWFs. The fields of the scanned run's block
        /// that do not apply say so rather than being left out, the way the OPEN FILE block does.
        /// </summary>
        public IList<string> SettingsLines()
        {
            List<string> lines = new List<string>();

            lines.Add("NWFs picked       : " + Words.Or(Picked, "nothing") + (PickedAFolder ? ", a folder" : ", one file"));
            lines.Add("include subfolders: " + (PickedAFolder
                ? (Subfolders ? "yes" : "no")
                : "not applicable, one file was picked"));
            lines.Add("NWFs found        : " + nwfs.Count.ToString(CultureInfo.InvariantCulture));
            lines.Add("NWFs refused      : " + Refused.ToString(CultureInfo.InvariantCulture)
                + (Refused == 0 ? string.Empty : ", each named below and never opened"));

            foreach (PickedNwf nwf in nwfs)
            {
                if (!nwf.CanRun)
                {
                    lines.Add("    " + nwf.Path + "  " + nwf.WhyNot);
                }
            }

            if (problems.Count > 0)
            {
                lines.Add("folders not read  : " + problems.Count.ToString(CultureInfo.InvariantCulture)
                    + ", an NWF in one of them is not in this run");

                foreach (string problem in problems)
                {
                    lines.Add("    " + problem);
                }
            }

            lines.Add("each NWF          : opened where it sits and run the way the open file run runs it, "
                + "nothing scanned, appended or cleared, the tests of the XML where one is picked "
                + "and otherwise the tests saved inside");
            lines.Add("NWD               : beside each NWF with the extension swapped");
            lines.Add("reports           : " + WhereTheReportsGo());
            lines.Add("source folder     : not applicable, nothing is scanned");
            lines.Add("grouping          : not applicable, each NWF is one group");

            return lines;
        }

        private string WhereTheReportsGo()
        {
            foreach (PickedNwf nwf in nwfs)
            {
                if (nwf.CanRun && nwf.ReportFolder.Length > 0)
                {
                    string beside = Path.Combine(OpenDocumentJob.FolderOf(nwf.Path), ReportPaths.Subfolder);

                    return string.Equals(nwf.ReportFolder, beside, StringComparison.OrdinalIgnoreCase)
                        ? "in " + ReportPaths.Subfolder + " beside each NWF"
                        : "in the Excel folder picked, " + nwf.ReportFolder;
                }
            }

            return "not worked out, no NWF can run";
        }

        /// <summary>One line under the group counts of RESULT.</summary>
        public string ResultLine()
        {
            return "NWFs picked    : " + nwfs.Count.ToString(CultureInfo.InvariantCulture) + " from "
                + Words.Or(Picked, "nothing") + ", each run as the open file run runs it"
                + (Refused == 0
                    ? string.Empty
                    : ", " + Refused.ToString(CultureInfo.InvariantCulture) + " refused before "
                        + (Refused == 1 ? "it was" : "they were") + " opened");
        }

        /// <summary>The window line after a pick. A label, so nothing the disk said is in it.</summary>
        public string Describe()
        {
            if (!CanStart)
            {
                return WhyNoRunForALabel;
            }

            string found = Words.Counted(nwfs.Count, "NWF", "NWFs") + " found in " + Picked + ", "
                + Runnable.ToString(CultureInfo.InvariantCulture) + " can run.";

            if (problems.Count > 0)
            {
                found += " " + Words.Counted(problems.Count, "folder", "folders") + " would not read. "
                    + RunLog.TheLogSaysWhy();
            }

            return found + " Each writes its NWD and report beside it.";
        }
    }

    /// <summary>
    /// The third choice on the Source step, F129, Bader's request 4 under Q112: one NWF or a
    /// folder of NWFs, each run the way the open file run runs it.
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
        /// <summary>The label beside the box on the Source step, by the tick box rule.</summary>
        public const string Label = "Existing NWF or folder of NWFs";

        /// <summary>The grey line under it, by the tick box rule.</summary>
        public const string HelpLine = "Runs each where it sits, its NWD and report beside it";

        /// <summary>The block written for each picked NWF once the run ends, in the shape of OPEN FILE.</summary>
        public const string SummaryTitle = "PICKED NWF";

        /// <summary>The first OPEN line of a picked NWF's group, where the open file run says it runs what is open.</summary>
        public const string RunningLine = "OPEN     running a picked NWF, opened by this run where it sits, no scan";

        /// <summary>The line said where the scanned run writes its SOURCE FINDINGS block.</summary>
        public const string SourceFindingsSkipped =
            "SOURCE   findings skipped, the picked NWFs have no scanned source folder to compare against";

        private const string PickFirst = "Pick one NWF or a folder of NWFs first.";

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

        /// <summary>What the pick gives, read off the disk itself.</summary>
        public static NwfPickPlan From(string picked, bool subfolders, string pickedExcelFolder)
        {
            return From(picked, subfolders, pickedExcelFolder, NwfPickDisk.Real);
        }

        /// <summary>
        /// The same with the disk handed in. In order: nothing picked, a pick that is neither a
        /// file nor a folder, a picked folder that will not read or holds no NWF, every NWF
        /// refused by the open file run's checks, and two NWFs writing one file. The first that
        /// applies is the reason the run cannot start.
        /// </summary>
        public static NwfPickPlan From(string picked, bool subfolders, string pickedExcelFolder, NwfPickDisk disk)
        {
            if (disk == null)
            {
                throw new ArgumentNullException("disk");
            }

            string path = picked == null ? string.Empty : picked.Trim();
            bool isFolder = path.Length > 0 && disk.IsFolder(path);
            NwfPickPlan plan = new NwfPickPlan(path, isFolder, subfolders);

            if (path.Length == 0)
            {
                plan.Refuse(PickFirst, PickFirst);
                return plan;
            }

            List<string> found = new List<string>();

            if (isFolder)
            {
                string whyNot = Walk(path, subfolders, disk, found, plan);

                if (whyNot != null)
                {
                    plan.Refuse(
                        "The folder " + path + " would not read, so no NWF in it can run. " + whyNot,
                        "The folder " + path + " would not read, so no NWF in it can run. " + RunLog.TheLogSaysWhy());
                    return plan;
                }

                if (found.Count == 0)
                {
                    string none = "The folder " + path + " holds no NWF"
                        + (subfolders ? ", in it or in any folder under it." : ", and its subfolders were not read, because the box to include them is unticked.");
                    plan.Refuse(none, none);
                    return plan;
                }
            }
            else if (disk.IsFile(path))
            {
                found.Add(path);
            }
            else
            {
                string nothing = "There is no file and no folder at " + path + ". " + PickFirst;
                plan.Refuse(nothing, nothing);
                return plan;
            }

            found.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string nwf in found)
            {
                plan.Add(new PickedNwf(nwf, OpenDocumentJob.WhyNot(nwf, f => disk.IsFolder(f)), pickedExcelFolder));
            }

            if (plan.Runnable == 0)
            {
                string why = !isFolder
                    ? plan.Nwfs[0].WhyNot
                    : "The folder " + path + " holds " + Words.Counted(found.Count, "NWF", "NWFs")
                        + (found.Count == 1 ? " and it cannot run. " : " and none of them can run. ")
                        + "The log names each with its reason.";
                plan.Refuse(why, why);
                return plan;
            }

            string collisions = Collisions(plan.Nwfs);

            if (collisions != null)
            {
                plan.Refuse(collisions, collisions);
            }

            return plan;
        }

        /// <summary>
        /// The NWFs of one folder, and of the folders under it where the box asks. A subfolder
        /// that will not read is a problem named in the log and the rest still run. The folder
        /// that was picked not reading is the reason none can, handed back.
        /// </summary>
        private static string Walk(string top, bool subfolders, NwfPickDisk disk, List<string> found, NwfPickPlan plan)
        {
            Queue<string> waiting = new Queue<string>();
            waiting.Enqueue(top);

            while (waiting.Count > 0)
            {
                string folder = waiting.Dequeue();

                try
                {
                    foreach (string file in disk.FilesIn(folder))
                    {
                        if (IsNwf(file))
                        {
                            found.Add(file);
                        }
                    }

                    if (subfolders)
                    {
                        foreach (string under in disk.FoldersIn(folder))
                        {
                            waiting.Enqueue(under);
                        }
                    }
                }
                catch (Exception error)
                {
                    if (ReferenceEquals(folder, top))
                    {
                        return error.Message;
                    }

                    plan.Problem(folder + "  " + error.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// Two NWFs whose NWDs or workbooks land on one path, read case blind as Windows reads a
        /// name, one sentence each. The second would write over the first and nothing would say
        /// so, which is why the scanned run refuses a shared name before anything is written, and
        /// this is the same refusal for picked NWFs. Null where none collide.
        /// </summary>
        private static string Collisions(IList<PickedNwf> nwfs)
        {
            List<string> sentences = new List<string>();
            Dictionary<string, PickedNwf> nwds = new Dictionary<string, PickedNwf>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, PickedNwf> workbooks = new Dictionary<string, PickedNwf>(StringComparer.OrdinalIgnoreCase);

            foreach (PickedNwf nwf in nwfs)
            {
                if (!nwf.CanRun)
                {
                    continue;
                }

                Collide(nwds, nwf.NwdPath, nwf, sentences);
                Collide(workbooks, nwf.WorkbookPath, nwf, sentences);
            }

            return sentences.Count == 0
                ? null
                : string.Join(Environment.NewLine + Environment.NewLine, sentences.ToArray());
        }

        private static void Collide(
            Dictionary<string, PickedNwf> seen, string output, PickedNwf nwf, List<string> sentences)
        {
            if (string.IsNullOrEmpty(output))
            {
                return;
            }

            PickedNwf first;

            if (!seen.TryGetValue(output, out first))
            {
                seen.Add(output, nwf);
                return;
            }

            sentences.Add(first.Path + " and " + nwf.Path + " would both write " + output
                + ", so the second would write over the first. Pick them apart, or clear the "
                + "Excel folder on the Outputs step so each report goes beside its own NWF.");
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
