using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Federator.Core.Views;
using Federator.Core.Report;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// The steps of the NWF page, F188, Bader's answer 3 of Q174 and his order of 14 Oct 2026, task 9: Refresh, Run
    /// tests, Pictures, Viewpoints and Reports, each with a tick box. A step cannot run without what it reads, so a
    /// tick the rule cannot honour is read as off and the page greys that box and says why: nothing is built from the
    /// results of tests that were not run, a picture is linked from a report, and a report is made from the tests.
    /// </summary>
    public sealed class NwfPageSteps
    {
        public const string RefreshName = "Refresh";
        public const string TestsName = "Run tests";
        public const string PicturesName = "Pictures";
        public const string ViewpointsName = "Viewpoints";
        public const string ReportsName = "Reports";

        public NwfPageSteps()
        {
            Refresh = true;
            RunTests = true;
            Pictures = true;
            Viewpoints = true;
            Reports = true;
        }

        public bool Refresh { get; set; }

        public bool RunTests { get; set; }

        public bool Pictures { get; set; }

        public bool Viewpoints { get; set; }

        public bool Reports { get; set; }

        /// <summary>The steps as they will run: a step whose need is off reads off.</summary>
        public NwfPageSteps Resolved()
        {
            NwfPageSteps steps = new NwfPageSteps();
            steps.Refresh = Refresh;
            steps.RunTests = RunTests;
            steps.Viewpoints = Viewpoints && RunTests;
            steps.Reports = Reports && RunTests;
            steps.Pictures = Pictures && steps.Reports;
            return steps;
        }

        /// <summary>Why a step cannot run with the others as they are, in a few words, or null where it can.</summary>
        public string WhyOff(string step)
        {
            if (string.Equals(step, ViewpointsName, StringComparison.Ordinal) && !RunTests)
            {
                return "Viewpoints are built from the clashes of the tests, so Run tests is needed.";
            }

            if (string.Equals(step, ReportsName, StringComparison.Ordinal) && !RunTests)
            {
                return "Reports are made from the clashes of the tests, so Run tests is needed.";
            }

            if (string.Equals(step, PicturesName, StringComparison.Ordinal) && !Resolved().Reports)
            {
                return "A picture is linked from a report, so Reports is needed.";
            }

            return null;
        }

        /// <summary>Puts the resolved steps on the run's options. The folders, the names and the tolerance are the caller's.</summary>
        public void Apply(ReportOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            NwfPageSteps steps = Resolved();
            options.RefreshOnOpen = steps.Refresh;
            options.RunTests = steps.RunTests;
            options.MakeViewpoints = steps.Viewpoints;
            options.WriteWorkbook = steps.Reports;
            options.WriteHtml = steps.Reports;

            if (options.Images != null)
            {
                options.Images.Write = steps.Pictures;
            }
        }

        /// <summary>The steps that will run, in the words of the page, for the confirm.</summary>
        public string Words()
        {
            NwfPageSteps steps = Resolved();
            List<string> on = new List<string>();

            if (steps.Refresh)
            {
                on.Add(RefreshName);
            }

            if (steps.RunTests)
            {
                on.Add(TestsName);
            }

            if (steps.Pictures)
            {
                on.Add(PicturesName);
            }

            if (steps.Viewpoints)
            {
                on.Add(ViewpointsName);
            }

            if (steps.Reports)
            {
                on.Add(ReportsName);
            }

            return on.Count == 0 ? "no step" : string.Join(", ", on.ToArray());
        }
    }

    /// <summary>
    /// The words the NWF page puts before a refresh, F188, Bader's order of 14 Oct 2026, task 9.4, and what the
    /// confirmation lists: the changed NWC files, the number of tests, the viewpoint folder that will be made again
    /// with how many viewpoints, and how many old jpg files will be removed. Cancel is the answer it asks for first.
    /// What is not known is said UNKNOWN and never made up.
    /// </summary>
    public static class RefreshConfirm
    {
        private const int FilesListed = 12;

        public static string Text(
            string nwfName,
            string backupFolder,
            ManifestComparison changes,
            string steps,
            int testsInMatrix,
            int toolViews,
            int outsideAtTheLastRun,
            int oldPictures,
            string viewpointFolderName,
            bool penetrations = false)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("This run refreshes " + nwfName + ".");
            text.AppendLine("Steps: " + steps + ".");
            text.AppendLine();
            text.AppendLine("A copy of the NWF is made first in " + (backupFolder ?? "UNKNOWN, no Log folder was found") + ".");
            text.AppendLine();
            text.AppendLine("NWC files changed since the last run:");

            if (changes == null || !changes.HadAnEarlierManifest)
            {
                text.AppendLine("    UNKNOWN. No earlier run of this NWF left a list, so every file is read as it is.");
            }
            else
            {
                IList<string> lines = changes.Lines();
                int shown = 0;

                foreach (string line in lines)
                {
                    if (shown == FilesListed)
                    {
                        text.AppendLine("    and " + (lines.Count - FilesListed).ToString(CultureInfo.InvariantCulture) + " more");
                        break;
                    }

                    text.AppendLine("    " + line);
                    shown++;
                }
            }

            text.AppendLine();
            text.AppendLine("Tests: " + (testsInMatrix > 0
                ? "the " + testsInMatrix.ToString(CultureInfo.InvariantCulture) + " of the built in matrix, and the tests saved in the NWF, run again where they sit. "
                : "the tests saved in the NWF run again where they sit. ")
                + "A test whose two sides carry one set code is not run, and one saved in the NWF keeps its earlier results there. "
                + "Statuses, assigned to and comments of clashes still there are kept"
                + (penetrations ? ", " + Federator.Core.Clash.PenetrationSettings.ConfirmWords(new Federator.Core.Views.SizeSettings()) : string.Empty) + ".");
            text.AppendLine("Viewpoints: the folder " + viewpointFolderName + " with "
                + (toolViews >= 0 ? toolViews.ToString(CultureInfo.InvariantCulture) : "UNKNOWN") + " viewpoints is deleted and made again."
                + " A viewpoint moved by hand into that folder is deleted with it.");
            text.AppendLine(OutsideTheToolFolder.ConfirmWords(outsideAtTheLastRun));
            text.AppendLine("Pictures: "
                + (oldPictures >= 0 ? oldPictures.ToString(CultureInfo.InvariantCulture) : "UNKNOWN")
                + " old jpg files in the Clash report folder are written again or removed.");
            text.AppendLine();
            text.Append("Nothing is changed before OK. Cancel is the default.");
            return text.ToString();
        }

        /// <summary>The window's title of the confirmation and the words of the rule it states once more.</summary>
        public const string Title = "Parsons NWC Federator";

        /// <summary>
        /// Where a picked NWF's pictures sit, F217, composed the way the engine composes them for the NWF's run: the page
        /// of its Clash report folder under the name OpenDocumentJob gives the NWF, then ImageNaming's folder of that page.
        /// The folder the one NWF's confirm and the confirm of a run of packages both count in.
        /// </summary>
        public static string PicturesFolderOf(string logFolder, string nwf)
        {
            return ImageNaming.FolderFor(ReportPaths.Workbook(ReportPaths.ClashReportFolder(logFolder), OpenDocumentJob.NameFrom(nwf)));
        }

        /// <summary>
        /// The old pictures a refresh writes again or removes, F217, Bader's F-R5 of 18 Oct 2026: across every folder, the
        /// files named as the tool names its pictures, `LeftoverPictures.IsOurPictureName`, the rule F177's removal reads,
        /// so the number is the files the run can write over or remove. filesIn gives a folder's file names, or null where
        /// the folder is not there, which holds none. Minus one, said UNKNOWN, where a folder could not be read, and why
        /// then names the folder and what the disk said, so the log can say it.
        /// </summary>
        public static int OldPictures(IEnumerable<string> folders, Func<string, IEnumerable<string>> filesIn, out string why)
        {
            why = null;

            if (folders == null || filesIn == null)
            {
                why = "no folder was handed in";
                return -1;
            }

            int count = 0;

            foreach (string folder in folders)
            {
                IEnumerable<string> files;

                try
                {
                    files = filesIn(folder);
                }
                catch (System.IO.IOException error)
                {
                    why = folder + " could not be read, " + error.GetType().Name + ": " + error.Message;
                    return -1;
                }
                catch (UnauthorizedAccessException error)
                {
                    why = folder + " could not be read, " + error.GetType().Name + ": " + error.Message;
                    return -1;
                }

                if (files == null)
                {
                    continue;
                }

                foreach (string file in files)
                {
                    if (LeftoverPictures.IsOurPictureName(System.IO.Path.GetFileName(file)))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }

    /// <summary>
    /// The NWC page's rule, F188, Bader's answer 3 of Q174: the page is for the first build of a group. A group whose
    /// NWF already exists is not run from it, unless the person ticks Rebuild and confirms, and the confirmation
    /// says the clash history of that NWF is not carried into the new one.
    /// </summary>
    public static class NwcPageRule
    {
        public const string RebuildLabel = "Rebuild";

        public const string RebuildHelp = "Builds a new NWF and NWD for a group that already has an NWF, into a new dated folder. The clash history of the old NWF is not carried over.";

        public const string ExistsLine = "NWF exists. Use the NWF page.";

        /// <summary>The status of a group of one discipline on the Grouping step, F232, Bader's round of 19 Oct 2026, fix 2.</summary>
        public const string OneDiscipline = "One discipline, not built";

        public const string Ready = "Ready";

        public const string BlockedWord = "BLOCKED: ";

        /// <summary>Whether the group can be ticked: not blocked, and its NWF not already there unless Rebuild is ticked.</summary>
        public static bool CanRun(bool blocked, bool nwfExists, bool rebuildTicked)
        {
            return !blocked && (!nwfExists || rebuildTicked);
        }

        /// <summary>
        /// Whether the group's box on the Grouping step can be ticked, F232: CanRun, and a group of one discipline never,
        /// Rebuild or not, since it is not built, BuildingGroup.CannotClashWith.
        /// </summary>
        public static bool CanTick(bool blocked, int disciplines, bool nwfExists, bool rebuildTicked)
        {
            return CanRun(blocked, nwfExists, rebuildTicked) && !Grouping.BuildingGroup.CannotClashWith(disciplines);
        }

        /// <summary>
        /// The Status column of the Grouping step, F232: blocked with why, one discipline not built, its NWF there and Rebuild
        /// off, ExistsLine, or ready, so a row whose box is greyed always says why.
        /// </summary>
        public static string StatusOf(string blockedReason, int disciplines, bool nwfExists, bool rebuildTicked)
        {
            if (blockedReason != null)
            {
                return BlockedWord + blockedReason;
            }

            if (Grouping.BuildingGroup.CannotClashWith(disciplines))
            {
                return OneDiscipline;
            }

            return nwfExists && !rebuildTicked ? ExistsLine : Ready;
        }

        /// <summary>The words of the confirmation of a Rebuild, naming the groups it would build again.</summary>
        public static string RebuildConfirm(IList<string> groupsWithAnNwf)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Rebuild builds the NWF and NWD of these groups again from the NWC files:");

            foreach (string group in groupsWithAnNwf ?? new string[0])
            {
                text.AppendLine("    " + group);
            }

            text.AppendLine();
            text.AppendLine("This DESTROYS THE CLASH HISTORY of that NWF in the new files: every status, assigned to and comment a coordinator wrote is not in them.");
            text.AppendLine("The old NWF is not overwritten. The new files go in a new dated folder beside it.");
            text.AppendLine(OutsideTheToolFolder.ConfirmWords(-1));
            text.AppendLine();
            text.Append("Carry on? Cancel is the default.");
            return text.ToString();
        }
    }

    /// <summary>
    /// The words of the NWF page and what the NWC page says beside it, F188, Bader's order of 14 Oct 2026, task 9.
    /// Short and plain. Written here once and read by the window in its constructor, so a test can read them
    /// and a copy typed into the XAML cannot drift. The two tab headers, NWC and NWF, are typed in the XAML.
    /// </summary>
    public static class NwfPageWords
    {
        public const string ChangedHeading = "NWC files changed since the last run";

        public const string NoNwfPicked = "Tick one NWF to see which NWC files changed.";

        /// <summary>The changed files list where several NWFs are ticked, F232, Q214.</summary>
        public static string SeveralNwfsTicked(int ticked)
        {
            return ticked.ToString(CultureInfo.InvariantCulture) + " NWFs are ticked. The list of changed NWC files is shown for one NWF at a time.";
        }

        public const string TickedNwfFolderNotRead = "The NWF folder of the ticked NWF could not be read. The log says why.";

        public const string TickedNwfNoLongerThere = "The ticked NWF is no longer in its NWF folder. Read the Report folder again.";

        public const string NoManifest = "No earlier run of this NWF left a list of its NWC files, so what changed is UNKNOWN. The run writes one.";

        public const string StepsHeading = "Steps";

        public const string StepsHelp = "A step that needs another is greyed when that one is off.";

        public const string RulesLine = "The tool viewpoint folder " + Federator.Core.Views.ToolViewFolder.Name + " is deleted and made again on each run. "
            + "Inside it A, B and C, then Architecture, Structure, Mechanical, Electrical and Other, each made only where a test is in it, "
            + "and Over 150mm inside Mechanical and Electrical of C only. "
            + "A viewpoint moved by hand into it is deleted. Every other viewpoint and folder outside it is deleted too, except the folder Comments. "
            + "Set Options, Model, Performance, Close NWC/NWD files on load in Navisworks, so a new NWC export can overwrite a linked file.";

        public const string TableHeading = "After the run: clashes of each test that moved, before > after";

        public const string TableEmpty = "No test moved.";

        public const string Run = "Run";
    }

    /// <summary>
    /// Where an NWF's own NWD and Log folder are, F189. The NWC page builds a group with its NWF and its NWD in two
    /// folders and the Log folder beside both, so the run of that NWF on the NWF page has to write its NWD and its Log,
    /// Backup and Clash report folders there and not beside the NWF, where the open file route puts them. The list is
    /// the tool's own, one NWF a line, remembered by the run that built or refreshed it. A line with no NWD or no
    /// Log folder says so with an empty field and is not used for that part.
    /// </summary>
    public sealed class NwfPlaces
    {
        public const string FileName = "nwf-places.tsv";

        private readonly List<string[]> rows = new List<string[]>();

        public ReadOnlyCollection<string[]> Rows
        {
            get { return new ReadOnlyCollection<string[]>(rows); }
        }

        /// <summary>The NWD path of that NWF, or empty where the list holds none.</summary>
        public string NwdOf(string nwf)
        {
            string[] row = Find(nwf);
            return row == null ? string.Empty : row[1];
        }

        /// <summary>The Log folder of that NWF, or empty where the list holds none.</summary>
        public string LogFolderOf(string nwf)
        {
            string[] row = Find(nwf);
            return row == null ? string.Empty : row[2];
        }

        public void Remember(string nwf, string nwd, string logFolder)
        {
            if (string.IsNullOrWhiteSpace(nwf))
            {
                return;
            }

            for (int i = rows.Count - 1; i >= 0; i--)
            {
                if (string.Equals(rows[i][0], nwf, StringComparison.OrdinalIgnoreCase))
                {
                    rows.RemoveAt(i);
                }
            }

            rows.Add(new[] { nwf.Trim(), (nwd ?? string.Empty).Trim(), (logFolder ?? string.Empty).Trim() });
        }

        public IList<string> ToLines()
        {
            List<string> lines = new List<string>();

            foreach (string[] row in rows)
            {
                lines.Add(row[0] + "\t" + row[1] + "\t" + row[2]);
            }

            return lines;
        }

        public static NwfPlaces Read(IList<string> lines)
        {
            NwfPlaces places = new NwfPlaces();

            if (lines == null)
            {
                return places;
            }

            foreach (string line in lines)
            {
                string[] parts = (line ?? string.Empty).Split('\t');

                if (parts.Length == 3 && parts[0].Trim().Length > 0)
                {
                    places.Remember(parts[0], parts[1], parts[2]);
                }
            }

            return places;
        }

        private string[] Find(string nwf)
        {
            foreach (string[] row in rows)
            {
                if (string.Equals(row[0], nwf, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// The folder a rebuild writes into so no NWF or NWD is overwritten, F188: a dated folder under the NWF's own
    /// folder, named by the moment, the way the first runs of this tool were put by hand.
    /// </summary>
    public static class RebuildFolder
    {
        public static string Name(DateTime now)
        {
            return "run " + now.ToString("yyyy-MM-dd HHmm", CultureInfo.InvariantCulture);
        }
    }
}
