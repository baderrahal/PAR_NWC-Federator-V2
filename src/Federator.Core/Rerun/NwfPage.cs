using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
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
    /// The NWFs last used, F188: a small list the page remembers in a settings file under
    /// %LOCALAPPDATA%\ParsonsNwcFederator, most recent first, one path each, never more than Max, a path once.
    /// The reading and the writing of the file are the add-in's. The rule is here so it is tested.
    /// </summary>
    public sealed class RecentNwfs
    {
        /// <summary>The most it remembers.</summary>
        public const int Max = 10;

        /// <summary>The file in the ParsonsNwcFederator folder under the local application data.</summary>
        public const string FileName = "recent-nwfs.txt";

        private readonly List<string> paths = new List<string>();

        public ReadOnlyCollection<string> Paths
        {
            get { return new ReadOnlyCollection<string>(paths); }
        }

        /// <summary>Puts the path first. One already there moves to the top, compared without case as Windows does.</summary>
        public void Remember(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            string clean = path.Trim();

            for (int i = paths.Count - 1; i >= 0; i--)
            {
                if (string.Equals(paths[i], clean, StringComparison.OrdinalIgnoreCase))
                {
                    paths.RemoveAt(i);
                }
            }

            paths.Insert(0, clean);

            while (paths.Count > Max)
            {
                paths.RemoveAt(paths.Count - 1);
            }
        }

        /// <summary>The list the lines give, in their order. A line that is empty is left out and a file that is not one reads empty.</summary>
        public static RecentNwfs Read(IList<string> lines)
        {
            RecentNwfs list = new RecentNwfs();

            if (lines == null)
            {
                return list;
            }

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                list.Remember(lines[i]);
            }

            return list;
        }

        public IList<string> ToLines()
        {
            return new List<string>(paths);
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
            int oldPictures,
            string viewpointFolderName)
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
                + "Statuses, assigned to and comments of clashes still there are kept.");
            text.AppendLine("Viewpoints: the folder " + viewpointFolderName + " with "
                + (toolViews >= 0 ? toolViews.ToString(CultureInfo.InvariantCulture) : "UNKNOWN") + " viewpoints is deleted and made again."
                + " A viewpoint moved by hand into that folder is deleted with it.");
            text.AppendLine("Pictures: "
                + (oldPictures >= 0 ? oldPictures.ToString(CultureInfo.InvariantCulture) : "UNKNOWN")
                + " old jpg files in the Clash report folder are written again or removed.");
            text.AppendLine();
            text.Append("Nothing is changed before OK. Cancel is the default.");
            return text.ToString();
        }

        /// <summary>The window's title of the confirmation and the words of the rule it states once more.</summary>
        public const string Title = "Parsons NWC Federator";
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

        /// <summary>Whether the group can be ticked: not blocked, and its NWF not already there unless Rebuild is ticked.</summary>
        public static bool CanRun(bool blocked, bool nwfExists, bool rebuildTicked)
        {
            return !blocked && (!nwfExists || rebuildTicked);
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
            text.AppendLine();
            text.Append("Carry on? Cancel is the default.");
            return text.ToString();
        }
    }

    /// <summary>
    /// The words of the NWF page and what the NWC page says beside it, F188, Bader's order of 14 Oct 2026, task 9.
    /// Short and plain. Written here once and read by the window in its constructor, so a test can read them
    /// and a copy typed into the XAML cannot drift.
    /// </summary>
    public static class NwfPageWords
    {
        public const string NwcPageName = "NWC";

        public const string NwfPageName = "NWF";

        public const string Heading = "The weekly work on an NWF that is built. Pick the NWF, see which NWC files changed, tick the steps and press Run. The NWC page builds an NWF once.";

        public const string RecentLabel = "Last used";

        public const string ChangedHeading = "NWC files changed since the last run";

        public const string NoNwfPicked = "Pick an NWF to see which NWC files changed.";

        public const string NoManifest = "No earlier run of this NWF left a list of its NWC files, so what changed is UNKNOWN. The run writes one.";

        public const string StepsHeading = "Steps";

        public const string StepsHelp = "A step that needs another is greyed when that one is off.";

        public const string RulesLine = "The tool viewpoint folder NWC Federator (auto) is deleted and made again on each run. A viewpoint moved by hand into it is deleted. "
            + "Set Options, Model, Performance, Close NWC/NWD files on load in Navisworks, so a new NWC export can overwrite a linked file.";

        public const string TableHeading = "After the run: clashes of each test that moved, before > after";

        public const string TableEmpty = "No test moved.";

        public const string Run = "Run";
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
