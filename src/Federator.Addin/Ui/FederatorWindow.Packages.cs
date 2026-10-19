using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Findings;
using Federator.Core.Grouping;
using Federator.Core.Communities;
using Federator.Core.Disciplines;
using Federator.Core.Naming;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Sets;
using Federator.Core.Teams;
using Federator.Core.Views;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// The packages, F208, and since F232, Bader's round of 19 Oct 2026, fix 2, the one route of each page. The NWC page's
    /// step 1. Packages reads the tree off the parent folder, step 2. Grouping shows one row per group of the ticked packages,
    /// and the one Run button, RunButton in the bar under the steps, runs the ticked groups, each in its own Navisworks. The NWF page reads a tree of
    /// community, package and NWF off the Report folder and its Run runs the ticked NWFs, each in its own Navisworks. The rules
    /// and the words are Core's, Federator.Core.Communities, and are tested there. This part reads the disk and the controls,
    /// and calls the engine.
    /// </summary>
    public partial class FederatorWindow
    {
        private readonly PackagePanel packagePanel = new PackagePanel();
        private PackageTree packageTree;
        private PackageTicks packageTicks;
        private PackageTree nwfTree;
        private NwfTicks nwfTicks;

        /// <summary>Called once from the constructor: both pages on the one data, the folders as they were last picked.</summary>
        private void StartThePackages()
        {
            PackagesPanel.DataContext = packagePanel;
            NwfPackagesPanel.DataContext = packagePanel;
            PackagesStep.Header = PackageWords.StepHeader;
            OutputsHeading.Text = PackageWords.OutputsHeading;
            RunNwfPackagesButton.Content = NwfPageWords.Run;
            packagePanel.Parent = folders.LastFor(PickerKind.PackageParent);
            packagePanel.Report = folders.LastFor(PickerKind.PackageReport);
        }

        private void OnBrowsePackageParent(object sender, RoutedEventArgs e)
        {
            // F222. Read again during a run of packages the tree would no longer be the one the run joins its results against.
            if (running)
            {
                return;
            }

            string picked = PickFolder(PackageWords.PickTheParent, StartFor(PickerKind.PackageParent, packagePanel.Parent));

            if (picked != null)
            {
                packagePanel.Parent = picked;
                ReadThePackages();
            }
        }

        private void OnBrowsePackageReport(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder(PackageWords.PickTheReport, StartFor(PickerKind.PackageReport, packagePanel.Report));

            if (picked != null)
            {
                RememberFolder(PickerKind.PackageReport, picked);
                packagePanel.Report = picked;
            }
        }

        private void OnReadPackages(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            ReadThePackages();
        }

        /// <summary>
        /// Reads the tree off the parent folder and shows it with nothing ticked, and names it in the log, every community
        /// and package with its path and what was left out. The groups of step 2 start empty, since nothing is ticked.
        /// </summary>
        private void ReadThePackages()
        {
            string parent = Trimmed(packagePanel.Parent);
            packageTree = PackageTree.Read(parent, PackageTree.NwcExtension);
            packageTicks = new PackageTicks(packageTree);

            log.Block(PackageWords.TreeTitle, packageTree.Lines());

            if (packageTree.WasRead)
            {
                RememberFolder(PickerKind.PackageParent, parent);
            }

            FillTheTree();
            Regroup(false);
        }

        /// <summary>The lines of the tree, made again from the tree read, each package with its files and its groups.</summary>
        private void FillTheTree()
        {
            packagePanel.Nodes.Clear();

            if (packageTree == null)
            {
                packagePanel.Summary = PackageWords.NotReadYet;
                return;
            }

            foreach (Community community in packageTree.Communities)
            {
                PackageNode node = PackageNode.ForCommunity(
                    community,
                    PackageWords.CommunityDetail(community.Packages.Count, community.FileCount, packageTree.Extension),
                    TickOf,
                    TickIt);

                foreach (Package package in community.Packages)
                {
                    node.Children.Add(PackageNode.ForPackage(package, DetailOf(package), TickOf, TickIt));
                }

                packagePanel.Nodes.Add(node);
            }

            packagePanel.Summary = PackageWords.Summary(packageTree, packageTicks);
        }

        /// <summary>A package's files and the groups they make with the grouping and the names of steps 2 and 3 as they stand.</summary>
        private string DetailOf(Package package)
        {
            try
            {
                ReadNaming();
                PackagePlan plan = PackagePlan.For(
                    package, Trimmed(packagePanel.Report), ChosenGrouping(), settings, naming, DateTime.Today);
                return PackageWords.Detail(package.Files.Count, packageTree.Extension, plan.Groups.Count, plan.Blocked);
            }
            catch (Exception error)
            {
                log.Failure("counting the groups of " + package.RelativePath, error, "kept going, its line shows the files alone");
                return PackageWords.FilesWord(package.Files.Count, packageTree.Extension);
            }
        }

        /// <summary>The group counts again, when step 1 is shown, since the grouping may have changed since the Read.</summary>
        private void RefreshThePackageDetails()
        {
            if (packageTree == null)
            {
                return;
            }

            foreach (PackageNode community in packagePanel.Nodes)
            {
                foreach (PackageNode child in community.Children)
                {
                    child.Detail = DetailOf(child.Package);
                }
            }
        }

        private bool? TickOf(PackageNode node)
        {
            if (packageTicks == null)
            {
                return false;
            }

            return node.Package != null ? packageTicks.IsTicked(node.Package) : packageTicks.StateOf(node.Community);
        }

        private void TickIt(PackageNode node, bool on)
        {
            // During a run a tick is not taken and nothing is planned again, the line only shows its tick as it stands.
            if (running)
            {
                node.TickMoved();
                return;
            }

            if (packageTicks != null)
            {
                if (node.Package != null)
                {
                    packageTicks.Tick(node.Package, on);
                }
                else
                {
                    packageTicks.TickCommunity(node.Community, on);
                }
            }

            TheTicksMoved();
        }

        /// <summary>The tree's ticks may have moved: the tree, the line under it and the groups of step 2 follow.</summary>
        private void TheTicksMoved()
        {
            foreach (PackageNode node in packagePanel.Nodes)
            {
                node.TickMoved();
            }

            packagePanel.Summary = PackageWords.Summary(packageTree, packageTicks);
            Regroup(true);
        }

        private void OnTickAllPackages(object sender, RoutedEventArgs e)
        {
            if (packageTicks != null && !running)
            {
                packageTicks.TickAll();
                TheTicksMoved();
            }
        }

        private void OnClearAllPackages(object sender, RoutedEventArgs e)
        {
            if (packageTicks != null && !running)
            {
                packageTicks.ClearAll();
                TheTicksMoved();
            }
        }

        /// <summary>
        /// Why no run of the NWC page can start, or null: no tree read, nothing ticked, or a Report folder the rule refuses.
        /// Said in the log and the warning before anything is opened.
        /// </summary>
        private string WhyThePackagesCannotRun()
        {
            if (packageTicks == null)
            {
                return PackageWords.NotReadYet;
            }

            string why = packageTicks.WhyNoRun();

            if (why != null)
            {
                return why;
            }

            return PackageOutput.WhyNot(packageTree.Parent, packagePanel.Report, packageTicks.Ticked());
        }

        private bool RefuseThe(string what, string why)
        {
            if (why == null)
            {
                return false;
            }

            log.Line(PackageWords.Prefix + " " + what + " refused before starting. " + why);
            Warn("The run cannot start." + Environment.NewLine + Environment.NewLine + why);
            return true;
        }

        /// <summary>
        /// Makes each ticked package's report folder and its NWF and NWD folders before its first group, one line each saying
        /// made or there. Only the packages that will run, so nothing is made for a package not ticked or one that runs nothing.
        /// </summary>
        private void MakeThePackageFolders(string report, IList<Package> runnable)
        {
            foreach (Package package in runnable)
            {
                foreach (string folder in PackageOutput.FoldersToMake(report, package))
                {
                    bool there = Directory.Exists(folder);

                    if (!there)
                    {
                        Directory.CreateDirectory(folder);
                    }

                    log.Line(there ? PackageOutput.ThereLine(folder) : PackageOutput.MadeLine(folder));
                }
            }
        }

        // ---------- step 2, the groups of the ticked packages ----------

        /// <summary>
        /// The rows of step 2, F232: one per group of each ticked package, made by its plan, PackagePlan, with the grouping
        /// and the names as they stand, a group the grouping blocked and every group of a package the name guard refuses
        /// blocked with why, and the findings of step 1 over the files of the ticked packages. keepChoices keeps each row's
        /// tick by its group's label, FR-153, and a Read or another grouping starts every group as its rule allows.
        /// </summary>
        private void Regroup(bool keepChoices)
        {
            if (GroupsGrid == null)
            {
                return;
            }

            KeptRunTicks runTicks = new KeptRunTicks();

            if (keepChoices)
            {
                foreach (GroupRow row in groups)
                {
                    // A row its rule left unticked, a group of one discipline or one whose NWF is there, was never a person's untick.
                    runTicks.Read(row.Label, row.IsBlocked || !row.CanInclude, row.Include);
                }
            }

            groups.Clear();
            runAnchor = null;

            List<BuildingGroup> grouped = new List<BuildingGroup>();
            int filesFound = 0;
            int unreadable = 0;
            GroupingMode mode = ChosenGrouping();

            if (packageTicks != null)
            {
                ReadNaming();

                foreach (Package package in packageTicks.Ticked())
                {
                    filesFound += package.Files.Count;
                    PackagePlan plan;

                    try
                    {
                        plan = PackagePlan.For(package, Trimmed(packagePanel.Report), mode, settings, naming, DateTime.Today);
                    }
                    catch (Exception error)
                    {
                        log.Failure("grouping the files of " + package.RelativePath, error, "kept going, its groups are not shown");
                        continue;
                    }

                    unreadable += plan.Unreadable;
                    grouped.AddRange(plan.Grouped);

                    foreach (PackageGroup group in plan.Groups)
                    {
                        groups.Add(plan.WhyNot == null
                            ? GroupRow.Usable(group)
                            : GroupRow.Blocked(package, group.Building, group.Files.Count, group.Disciplines, plan.WhyNot));
                    }

                    foreach (SkippedBuildingGroup skipped in plan.Skipped)
                    {
                        groups.Add(GroupRow.Blocked(package, skipped.Building, skipped.Files.Count, new List<string>(), skipped.Reason));
                    }
                }
            }

            RefreshRunPaths();

            foreach (GroupRow row in groups)
            {
                row.Include = runTicks.StartsTicked(row.Label);
            }

            int blocked = groups.Count(row => row.IsBlocked);
            ShowGroupingSummary();
            findings = ScanFindings.From(grouped);
            ShowFindings(filesFound, filesFound - unreadable, groups.Count - blocked, blocked, mode);
        }

        /// <summary>
        /// Which of the workflows each group of step 2 is expected to take, read off whether its NWF is already at the path its
        /// plan gives it, through Federator.Core.Rerun.RunPath, and whether a matrix is built in. A group whose NWF is there is
        /// run from the NWF page, unless Rebuild is ticked, F188. A group about to build an NWF beside a file whose name is
        /// nearly the same says so, F71, each NWF folder listed once.
        /// </summary>
        private void RefreshRunPaths()
        {
            bool xmlPicked = MatrixIsBuiltIn();
            Dictionary<string, IList<string>> namesIn = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
            int nearby = 0;

            foreach (GroupRow group in groups)
            {
                if (group.IsBlocked)
                {
                    continue;
                }

                bool nwfOnDisk;
                string folder;

                try
                {
                    nwfOnDisk = File.Exists(group.NwfPath);
                    folder = Path.GetDirectoryName(group.NwfPath);
                }
                catch (Exception error)
                {
                    log.Failure("reading whether the NWF of " + group.Label + " is there", error, "its Run as reads UNKNOWN");
                    group.RunAs = RunPath.Unknown;
                    group.NearbyNwf = string.Empty;
                    continue;
                }

                group.RunAs = RunPath.Expected(nwfOnDisk, xmlPicked);
                group.NwfExists = nwfOnDisk;
                group.RebuildAllowed = RebuildExisting.IsChecked == true;
                group.AllowedMoved();

                IList<string> names;

                if (!namesIn.TryGetValue(folder ?? string.Empty, out names))
                {
                    names = NwfNamesIn(folder);
                    namesIn[folder ?? string.Empty] = names;
                }

                string note = nwfOnDisk
                    ? string.Empty
                    : SimilarNames.Note(SimilarNames.ClosestIn(group.NwfName, names, settings));

                group.NearbyNwf = note;

                if (note.Length > 0)
                {
                    nearby++;
                }
            }

            ShowNearbyNwfLine(nearby);
        }

        /// <summary>The line under the groups of step 2: the grouping, the groups, the blocked and how many are ticked.</summary>
        private void ShowGroupingSummary()
        {
            int blocked = groups.Count(row => row.IsBlocked);
            int ready = groups.Count - blocked;
            int ticked = groups.Count(row => row.Include);

            GroupingSummary.Text = GroupingModes.Describe(ChosenGrouping()) + ". "
                + ready + (ready == 1 ? " group, " : " groups, ") + blocked + " blocked, " + ticked + " ticked to run.";
        }

        /// <summary>
        /// The findings panel, under the tree of step 1 since F232, over the NWC files of the ticked packages. Nothing here
        /// blocks a run or unticks anything. It is information and the decision stays with the person reading it.
        /// </summary>
        private void ShowFindings(int filesFound, int filesReadable, int ready, int blocked, GroupingMode mode)
        {
            counts = new ScanCounts();
            counts.FilesFound = filesFound;
            counts.FilesReadable = filesReadable;
            counts.GroupingDescription = GroupingModes.Describe(mode);
            counts.Groups = ready;
            counts.BlockedGroups = blocked;
            counts.Count(AllFindings());

            FindingsHeading.Text = PackageWords.FindingsHeading(counts.TotalFindings);

            List<string> lines = new List<string>(counts.Lines());
            lines.Add(string.Empty);

            foreach (ScanFinding finding in AllFindings())
            {
                lines.Add(finding.Label + "   " + finding.Sentence);

                foreach (string file in finding.Files)
                {
                    lines.Add("      " + file);
                }

                lines.Add(string.Empty);
            }

            lines.Add(PackageWords.SourceFindingsInEachGroupLog);

            FindingsBox.Text = string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>The row of step 2 a group of a plan was shown on, by its label, or null.</summary>
        private GroupRow RowFor(PackageGroup group)
        {
            foreach (GroupRow row in groups)
            {
                if (string.Equals(row.Label, group.Label, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        // ---------- the NWC page's route ----------

        /// <summary>
        /// The one Run button of the NWC page, RunButton in the bar under the steps, which Bader's words call the one at the top, Q215, F232: every group of the ticked packages that is ticked on step 2, each group's NWF and
        /// NWD in the package's own folders and its logs and reports in the package's Log folder, a group whose NWF is already
        /// there left to the NWF page unless Rebuild is ticked, a group of one discipline not built. A package that cannot run
        /// is named and the others run. Each group runs in its own Navisworks with its own engine, F214 and F222, and one
        /// RESULT joins them.
        /// </summary>
        private void OnRun(object sender, RoutedEventArgs e)
        {
            if (running || RefuseThe("the run of packages", WhyThePackagesCannotRun()))
            {
                return;
            }

            ToleranceChoice tolerance = ToleranceOrSayWhyNot("the run of packages");

            if (tolerance == null)
            {
                return;
            }

            // The groups again with the names, the grouping and the Report folder as they stand, each tick kept.
            Regroup(true);

            ReadNaming();
            GroupingMode mode = ChosenGrouping();
            string report = Trimmed(packagePanel.Report);
            ReportOptions options = ReportsWanted(tolerance);
            PackageRun run = new PackageRun(report, options.Disciplines) { Images = options.Images };
            PackageTally tally = run.Tally;
            List<FederationJob> jobs = new List<FederationJob>();
            List<Package> runnable = run.Runnable;
            List<string> planLines = new List<string>();
            int tickedFiles = 0;
            bool rebuild = RebuildExisting.IsChecked == true;

            foreach (Package package in packageTicks.Ticked())
            {
                tally.Expect(package);
                tickedFiles += package.Files.Count;
                PackagePlan plan;

                try
                {
                    plan = PackagePlan.For(package, report, mode, settings, naming, DateTime.Today);
                }
                catch (Exception error)
                {
                    string why = "its files could not be grouped, " + error.GetType().Name + ": " + error.Message;
                    log.Failure("grouping the files of " + package.RelativePath, error, "its groups are not run, the other packages run");
                    tally.NotRun(package, why);
                    planLines.Add(package.RelativePath + "  NOT RUN  " + why);
                    continue;
                }

                run.Plans[package.RelativePath] = plan;

                foreach (string line in plan.NotRun)
                {
                    planLines.Add(package.RelativePath + "  " + line);
                }

                if (plan.WhyNot != null)
                {
                    tally.NotRun(package, plan.WhyNot);
                    planLines.Add(package.RelativePath + "  NOT RUN  " + plan.WhyNot);
                    continue;
                }

                int before = jobs.Count;
                string logFolder = options.LogFolderFor(plan.NwfFolder, plan.NwdFolder);

                foreach (PackageGroup group in plan.Groups)
                {
                    bool exists = File.Exists(group.NwfPath);

                    // F233, Bader's round of 19 Oct 2026, fix 3. A group of one discipline is not built: its one line, the line
                    // of an NWF an earlier run left, never deleted and never run, and its count in RESULT.
                    if (BuildingGroup.CannotClashWith(group.Disciplines.Count))
                    {
                        planLines.Add(group.Label + "  left out, " + NwcPageRule.OneDiscipline);

                        // Said and counted once, by the window that started the run, and not again in each group's own Navisworks,
                        // whose RESULT the joined RESULT nests under its group.
                        if (child == null)
                        {
                            log.Line(OneDisciplineGroups.Line(package.Community, group.Building, group.Disciplines.Count == 0 ? null : group.Disciplines[0], group.Files));

                            if (exists)
                            {
                                log.Line(OneDisciplineGroups.EarlierNwfLine(group.NwfPath));
                            }

                            tally.NotBuilt(package, group.Building);

                            if (!run.Runnable.Contains(package) && !run.NotBuiltOnly.Contains(package))
                            {
                                run.NotBuiltOnly.Add(package);
                            }
                        }

                        continue;
                    }

                    if (!NwcPageRule.CanRun(false, exists, rebuild))
                    {
                        planLines.Add(group.Label + "  left out, " + NwcPageRule.ExistsLine + " " + group.NwfPath);
                        continue;
                    }

                    GroupRow row = RowFor(group);

                    if (row != null && !row.Include)
                    {
                        planLines.Add(group.Label + "  left out, " + PackageWords.UntickedOnStep2);
                        continue;
                    }

                    jobs.Add(new FederationJob(
                        group.Building,
                        group.NwfName,
                        group.NwfPath,
                        group.NwdPath,
                        group.Files,
                        group.WorkbookName,
                        group.Disciplines.Count,
                        group.Disciplines).InPackage(package.RelativePath, logFolder));
                    planLines.Add(group.Label + "  runs, " + group.Files.Count + " files, NWF " + group.NwfPath);
                }

                if (jobs.Count == before)
                {
                    tally.NotRun(package, PackageWords.NoGroupOfItRuns);
                }
                else
                {
                    runnable.Add(package);
                    run.NotBuiltOnly.Remove(package);
                }
            }

            log.Block(PackageWords.TreeTitle, packageTree.Lines());
            log.Block(PackageWords.ChosenTitle, packageTicks.Lines());
            log.Block("PACKAGE GROUPS", planLines);
            log.Block(RunLog.FindingsSectionTitle, findings.Lines());

            // F222, Q211. A group's own Navisworks runs its one group, the one whose NWF the order names, and no other. Where its
            // package plans no group at all the refusal below gives the real reason.
            if (child != null && jobs.Count > 0)
            {
                jobs = jobs.FindAll(job => child.Unit.IsTheGroupOf(job.NwfPath));

                if (jobs.Count == 0)
                {
                    childRanNoGroupBecause = PackageRunWords.NotTheGroup(child.Unit.Label, child.Unit.Group);
                    log.Line(PackageRunWords.Prefix + " " + childRanNoGroupBecause);
                    return;
                }
            }

            if (jobs.Count == 0)
            {
                log.Line(PackageWords.Prefix + " the run of packages refused before starting. No group of the ticked packages can run, each is named above");

                // F233. A run whose every group is of one discipline still writes RESULT, its line per community with the count,
                // and F234 its discipline workbooks, where each such group is a row.
                if (child == null && tally.NotBuiltCount > 0)
                {
                    WriteTheDisciplineReports(run, new List<PackageRunUnit>(), new List<PackageRunResult>(), new List<string>());
                    WriteTheClashCountChecks(run, new List<PackageRunUnit>(), new List<PackageRunResult>());
                    WriteTheResult(null, tally.Lines());
                }

                Warn("No group of the ticked packages can run. The log names each and why.");
                return;
            }

            List<KeyValuePair<string, IList<string>>> packGroups = new List<KeyValuePair<string, IList<string>>>();
            List<string> labels = new List<string>();

            foreach (FederationJob job in jobs)
            {
                packGroups.Add(PackChoice.Group(job.Label, job.Files));
                labels.Add(RunPath.Label(RerunDecision.Build, MatrixIsBuiltIn()));
            }

            if (!ChoosePackOrStop(packGroups, "the run of packages"))
            {
                return;
            }

            if (!ConfirmClear(labels, tolerance))
            {
                Log("Run cancelled before anything was cleared.");
                return;
            }

            log.RunSettings(
                packageTree.Parent,
                true,
                "the NWF folder of each package under " + report,
                "the NWD folder of each package under " + report,
                packageTree.FileCount,
                tickedFiles,
                jobs.Count);
            log.Line("grouping         : " + GroupingModes.Describe(mode));
            SayTheRunsChoices(tolerance, MarkPenetrations.IsChecked == true);

            try
            {
                MakeThePackageFolders(report, runnable);
            }
            catch (Exception error)
            {
                log.Failure("making the report folders of the ticked packages", error, "nothing was run");
                Warn("A report folder could not be made, so nothing was run." + Environment.NewLine + Environment.NewLine + error.Message);
                return;
            }

            // F214 and F222. The window a person runs hands each group to a fresh Navisworks, and a group's own window runs it.
            if (child == null)
            {
                List<PackageRunUnit> units = new List<PackageRunUnit>();

                foreach (FederationJob job in jobs)
                {
                    units.Add(new PackageRunUnit(job.Package, job.NwfPath, job.Building));
                }

                // F243 point 7. The old names, once, before any group starts: each group of the plan of every package of the
                // run by its NWF name and its workbook name, and each package's own report folder, under its community.
                Dictionary<string, List<string>> groupsOfEachCommunity = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, Dictionary<string, string>> foldersOfEachCommunity =
                    new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                List<Package> inTheRun = new List<Package>(runnable);
                inTheRun.AddRange(run.NotBuiltOnly);

                foreach (Package package in inTheRun)
                {
                    PackagePlan plan;
                    FoldersOf(foldersOfEachCommunity, package.Community)[package.RelativePath] = PackageOutput.FolderFor(report, package);

                    if (run.Plans.TryGetValue(package.RelativePath, out plan))
                    {
                        foreach (PackageGroup group in plan.Groups)
                        {
                            GroupsOf(groupsOfEachCommunity, package.Community).Add(group.NwfName);
                            GroupsOf(groupsOfEachCommunity, package.Community).Add(group.WorkbookName);
                        }
                    }
                }

                CleanTheOldReportFiles(report, foldersOfEachCommunity, groupsOfEachCommunity, run.Disciplines);
                RunPackagesInFreshStarts(PackageRunOrder.NwcPage, packageTree.Parent, run, units);
                return;
            }

            RunJobs(jobs, tolerance, run);
        }

        /// <summary>
        /// One run of packages, F208 and F209: the Report folder, the tally RESULT reads, the packages that run with the
        /// plan of each on the NWC page, by its path from the parent, and the discipline settings, for the discipline
        /// reports the window that started the run writes once every group has run, F222.
        /// </summary>
        private sealed class PackageRun
        {
            public PackageRun(string report, DisciplineReportSettings disciplines)
            {
                Report = report;
                Disciplines = disciplines;
                Tally = new PackageTally();
                Runnable = new List<Package>();
                NotBuiltOnly = new List<Package>();
                Plans = new Dictionary<string, PackagePlan>(StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>F234 on row F233-R3. The ticked packages whose every group is of one discipline, which run nothing and still get their workbooks.</summary>
            public List<Package> NotBuiltOnly { get; private set; }

            /// <summary>F236. The run's picture settings, which the clash count check reads to say why a row has no picture.</summary>
            public ImageOptions Images { get; set; }

            public string Report { get; private set; }

            public DisciplineReportSettings Disciplines { get; private set; }

            public PackageTally Tally { get; private set; }

            public List<Package> Runnable { get; private set; }

            public Dictionary<string, PackagePlan> Plans { get; private set; }
        }

        /// <summary>
        /// F236, Bader's round of 19 Oct 2026, fix 6. Every run writes one clash count check a community, written by the window
        /// that started the run once every group has run, off the files each group wrote: its .tsv beside its log, its clash
        /// workbook by the sheet the .tsv names and its HTML page, Core's GroupRunFiles, WorkbookTests and PageTests. The groups
        /// not built are read off the plans on the NWC page and off the results on the NWF page, and a group of the run whose
        /// files could not be read is named with why, never dropped. Each group and each community in a try of its own, and the
        /// whole in one, since a report check never stops a run.
        /// </summary>
        private void WriteTheClashCountChecks(PackageRun run, IList<PackageRunUnit> units, IList<PackageRunResult> results)
        {
            try
            {
                WriteTheClashCountChecksOf(run, units, results);
            }
            catch (Exception error)
            {
                log.Failure("writing the clash count checks", error, "the run's RESULT is still written");
            }
        }

        private void WriteTheClashCountChecksOf(PackageRun run, IList<PackageRunUnit> units, IList<PackageRunResult> results)
        {
            List<Package> reported = new List<Package>(run.Runnable);
            reported.AddRange(run.NotBuiltOnly);
            Dictionary<string, ClashCountCheck> checks = new Dictionary<string, ClashCountCheck>(StringComparer.OrdinalIgnoreCase);
            List<string> order = new List<string>();
            Dictionary<string, Package> packages = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);
            OpenClashCount viewStatuses = new ViewpointSettings().ViewStatuses;

            foreach (Package package in reported)
            {
                packages[package.RelativePath] = package;

                if (!checks.ContainsKey(package.Community))
                {
                    checks[package.Community] = new ClashCountCheck(package.Community);
                    order.Add(package.Community);
                }

                PackagePlan plan;

                if (run.Plans.TryGetValue(package.RelativePath, out plan))
                {
                    foreach (PackageGroup group in plan.Groups)
                    {
                        if (BuildingGroup.CannotClashWith(group.Disciplines.Count))
                        {
                            checks[package.Community].AddNotBuilt(group.Building);
                        }
                    }
                }
            }

            for (int i = 0; i < units.Count; i++)
            {
                Package package;

                if (!packages.TryGetValue(units[i].Package, out package))
                {
                    log.Line("COUNTS   " + units[i].Label + " is in no clash count check, its package " + units[i].Package + " is not among the packages the run planned");
                    continue;
                }

                ClashCountCheck check = checks[package.Community];

                try
                {
                    PackageRunResult result = i < results.Count ? results[i] : null;

                    if (result == null)
                    {
                        check.AddNotRead(units[i].Name, "its Navisworks wrote no result, so whether it wrote any file is UNKNOWN");
                        continue;
                    }

                    if (result.Groups.Count == 0)
                    {
                        if (OneDisciplineGroups.IsTheLine(result.NoGroupBecause))
                        {
                            check.AddNotBuilt(units[i].Name);
                        }
                        else
                        {
                            check.AddNotRead(units[i].Name, "its Navisworks ran no group, " + (result.NoGroupBecause ?? "why is UNKNOWN"));
                        }

                        continue;
                    }

                    string tsv = result.Log == null ? null : RowLog.PathFor(result.Log);

                    if (tsv == null || !File.Exists(tsv))
                    {
                        check.AddNotRead(result.Groups[0].Group, "its .tsv " + (tsv ?? "UNKNOWN") + " is not there");
                        continue;
                    }

                    GroupRunFiles files = GroupRunFiles.Read(File.ReadAllLines(tsv));

                    // F236 on the breaker's S1. A group whose .tsv names no test that ran has no count to read, and a total of
                    // noughts would read as checked and clean.
                    if (!files.AnyTestRead)
                    {
                        check.AddNotRead(result.Groups[0].Group, "its .tsv names no test that ran, its tests not asked for or the group ending before its clash step");
                        continue;
                    }

                    string whyNoWorkbook;
                    WorkbookTests workbook = TheClashWorkbook(files, out whyNoWorkbook);
                    string whyNoPage = files.Page == null ? "no page was written" : !File.Exists(files.Page) ? "the page written, " + files.Page + ", is not on the disk" : null;
                    PageTests page = whyNoPage == null ? PageTests.Read(File.ReadAllText(files.Page)) : null;

                    if (workbook == null)
                    {
                        log.Line("COUNTS   " + units[i].Label + " read with no workbook, " + whyNoWorkbook + ", so its Excel rows are UNKNOWN");
                    }

                    if (page == null)
                    {
                        log.Line("COUNTS   " + units[i].Label + " read with no page, " + whyNoPage + ", so its HTML rows are UNKNOWN");
                    }

                    check.AddGroup(result.Groups[0].Group, files, workbook, page, run.Images, viewStatuses);
                }
                catch (Exception error)
                {
                    check.AddNotRead(units[i].Name, "reading its files threw " + error.GetType().Name);
                    log.Failure("reading the files of " + units[i].Label + " for the clash count check", error, "the other groups are still counted");
                }
            }

            foreach (string community in order)
            {
                string path = null;
                bool saving = false;

                try
                {
                    ClashCountCheck check = checks[community];
                    string folder = PackageOutput.CommunityFolderFor(run.Report, community);
                    Directory.CreateDirectory(folder);
                    // F243, Bader's order of 19 Oct 2026: one name, overwritten.
                    path = Path.Combine(folder, ClashCountWorkbook.NameFor(community));
                    log.Block("CLASH COUNT CHECK " + community, check.Lines());
                    log.WriteAttempted("COUNTS", path);
                    saving = true;
                    ClashCountWorkbook.Write(path, check, DateTime.Now);
                    saving = false;
                    log.WriteFinished("COUNTS", path);
                    log.Line(ClashCountWorkbook.ReadBackLine(path, check));
                }
                catch (Exception error)
                {
                    // F243 point 6. Open in Excel: left as it is, its one line said and kept for the end message.
                    if (saving && FileInUse.IsInUse(error))
                    {
                        log.NotWrittenWhileOpen(path);
                        continue;
                    }

                    log.Failure("writing the clash count check of " + community, error, "the other communities are still written");

                    // F236 on the breaker's C1. A save that threw lists nothing as written, as every workbook's does.
                    if (saving)
                    {
                        log.WriteThrew("COUNTS", path);
                    }
                }
            }
        }

        /// <summary>
        /// F236. The clash workbook a group's .tsv names, read by the sheet the writer gave it as the .tsv names it, or null with
        /// why where none was written, it is not on the disk, no row names its sheet or the read gave a reason.
        /// </summary>
        private static WorkbookTests TheClashWorkbook(GroupRunFiles files, out string why)
        {
            why = files.Workbook == null ? "no clash workbook was written"
                : !File.Exists(files.Workbook) ? "the clash workbook written, " + files.Workbook + ", is not on the disk"
                : files.WorkbookSheet == null ? "no row of the .tsv names its sheet"
                : null;

            if (why != null)
            {
                return null;
            }

            WorkbookTests workbook = WorkbookTests.ReadFile(files.Workbook, files.WorkbookSheet, out why);
            why = workbook == null ? why ?? "it would not read" : null;
            return workbook;
        }

        /// <summary>
        /// F209 and F222. The discipline workbooks of every package that ran, written by the window that started the run off
        /// the facts each group's result carried, Core's PackageRunFacts, each package in a try of its own, so one package's
        /// fault never stops the others' reports, and every package's DISCIPLINE REPORT block in the log.
        /// </summary>
        private void WriteTheDisciplineReports(PackageRun run, IList<PackageRunUnit> units, IList<PackageRunResult> results, IList<string> whys)
        {
            List<Package> reported = new List<Package>(run.Runnable);
            reported.AddRange(run.NotBuiltOnly);

            foreach (Package package in reported)
            {
                try
                {
                    // F234. A package of one discipline groups alone ran nothing, so its folders are made here for its workbooks.
                    if (run.NotBuiltOnly.Contains(package))
                    {
                        MakeThePackageFolders(run.Report, new[] { package });
                    }

                    PackagePlan plan;
                    IList<DisciplineGroup> groupsOfIt = PackageRunFacts.GroupsOf(
                        package, run.Plans.TryGetValue(package.RelativePath, out plan) ? plan : null, units, results, whys);
                    WriteOnePackagesReports(package, groupsOfIt, run.Report, run.Disciplines);
                }
                catch (Exception error)
                {
                    log.Failure("the discipline reports of " + package.RelativePath, error, "the other packages' reports are still written");
                }
            }
        }

        /// <summary>
        /// One package's four workbooks off its groups, each with its files and the facts of its models where they were read.
        /// Each NWC's date off the disk. Each workbook under its one name, overwritten, F243, Bader's order of 19 Oct 2026, one
        /// open in another program left as it is and named, its size read back by the log and its lines read back off the file.
        /// </summary>
        private void WriteOnePackagesReports(Package package, IList<DisciplineGroup> groupsOfIt, string report, DisciplineReportSettings disciplines)
        {
            List<FileDate> dates = new List<FileDate>();

            foreach (DisciplineGroup group in groupsOfIt)
            {
                foreach (string file in group.Files)
                {
                    try
                    {
                        dates.Add(File.Exists(file) ? new FileDate(file, File.GetLastWriteTime(file), null) : new FileDate(file, null, "the file is not there"));
                    }
                    catch (Exception error)
                    {
                        dates.Add(new FileDate(file, null, error.GetType().Name + ": " + error.Message));
                    }
                }
            }

            DisciplineReport made = DisciplineReport.For(package.Community, package.Name, groupsOfIt, dates, DateTime.Now, disciplines, settings);
            log.Block("DISCIPLINE REPORT " + package.RelativePath, made.Lines());
            string folder = PackageOutput.FolderFor(report, package);

            foreach (string discipline in made.Disciplines)
            {
                // F243, Bader's order of 19 Oct 2026: one name, overwritten.
                string path = Path.Combine(folder, DisciplineWorkbook.NameFor(package.RelativePath, discipline, disciplines));
                IList<DisciplineIssue> rows = made.IssuesFor(discipline);
                log.WriteAttempted("CHECKS", path);

                try
                {
                    DisciplineWorkbook.Write(path, made, discipline);
                }
                catch (Exception error)
                {
                    // F243 point 6. Open in Excel: left as it is, its one line said and kept for the end message.
                    if (FileInUse.IsInUse(error))
                    {
                        log.NotWrittenWhileOpen(path);
                        continue;
                    }

                    log.WriteThrew("CHECKS", path);
                    log.Failure("writing the " + discipline + " workbook of " + package.RelativePath, error, "the other workbooks are still written");
                    continue;
                }

                log.WriteFinished("CHECKS", path);
                string why;
                DisciplineWorkbookRead back = DisciplineWorkbook.Read(path, out why);
                log.Line(back == null
                    ? "CHECKS   read back FAILED  " + path + ", " + why
                    : DisciplineWorkbookWords.ReadBackLine(back, rows.Count, discipline, package.RelativePath));
            }
        }

        /// <summary>The list of group names of one community, made the first time it is asked for.</summary>
        private static List<string> GroupsOf(Dictionary<string, List<string>> groupsOfEachCommunity, string community)
        {
            List<string> names;

            if (!groupsOfEachCommunity.TryGetValue(community, out names))
            {
                names = new List<string>();
                groupsOfEachCommunity.Add(community, names);
            }

            return names;
        }

        /// <summary>The report folder of each package of one community by its path from the parent, made the first time it is asked for.</summary>
        private static Dictionary<string, string> FoldersOf(Dictionary<string, Dictionary<string, string>> foldersOfEachCommunity, string community)
        {
            Dictionary<string, string> folders;

            if (!foldersOfEachCommunity.TryGetValue(community, out folders))
            {
                folders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foldersOfEachCommunity.Add(community, folders);
            }

            return folders;
        }

        /// <summary>
        /// F243, Bader's order of 19 Oct 2026 point 7. The files earlier runs wrote under names this tool no longer writes,
        /// deleted once by the window that starts the run, before any group starts, inside Report\COMMUNITY\ of each community
        /// of the run and nowhere else, each package's own report folder among it, Core's OldReportFiles. Each file on its own
        /// line with its kind, one held open said in FileInUse's words and kept for the end message, any other failure logged,
        /// and one line a community with the count. Nothing here stops the run.
        /// </summary>
        private void CleanTheOldReportFiles(
            string report, Dictionary<string, Dictionary<string, string>> foldersOfEachCommunity,
            Dictionary<string, List<string>> groupsOfEachCommunity, DisciplineReportSettings disciplines)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> community in foldersOfEachCommunity)
            {
                try
                {
                    List<string> groups;
                    groupsOfEachCommunity.TryGetValue(community.Key, out groups);
                    OldReportFilesOutcome cleaned = OldReportFiles.Clean(
                        PackageOutput.CommunityFolderFor(report, community.Key), community.Key, community.Value, groups, disciplines);

                    foreach (string line in cleaned.Lines)
                    {
                        log.Line(line);
                    }

                    foreach (string path in cleaned.InUse)
                    {
                        log.NotWrittenWhileOpen(path);
                    }

                    foreach (KeyValuePair<string, Exception> failed in cleaned.Failed)
                    {
                        log.Failure("deleting or renaming " + failed.Key + ", a name this tool no longer writes", failed.Value,
                            "kept going, the file is left where it is");
                    }

                    log.Line(cleaned.SummaryLine());
                }
                catch (Exception error)
                {
                    log.Failure("cleaning the old names in the report folder of " + community.Key, error,
                        "kept going, the run starts with those files as they are");
                }
            }
        }

        /// <summary>Each group of a run of packages added to the tally, by the package its job names.</summary>
        private void CountThePackageGroups(PackageTally tally, IList<JobOutcome> outcomes)
        {
            foreach (JobOutcome outcome in outcomes)
            {
                Package package = outcome == null || outcome.Job == null ? null : packageTree.Find(outcome.Job.Package);

                if (package != null)
                {
                    tally.Add(package, outcome.Job.Building, outcome.Result, GroupSize.ClashesIn(outcome.Clash));
                }
            }
        }

        // ---------- the NWF page's tree ----------

        private void OnBrowseNwfReport(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            string picked = PickFolder(PackageWords.PickTheReport, StartFor(PickerKind.PackageReport, packagePanel.Report));

            if (picked != null)
            {
                packagePanel.Report = picked;
                ReadTheNwfs();
            }
        }

        private void OnReadNwfPackages(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            ReadTheNwfs();
        }

        /// <summary>
        /// Reads the NWF page's tree off the Report folder, F232, shows it with nothing ticked, and names it in the log,
        /// every community, package and NWF and what was left out.
        /// </summary>
        private void ReadTheNwfs()
        {
            string report = Trimmed(packagePanel.Report);
            string refused = PackageOutput.WhyNot(null, report);

            // F232, the breaker's reading: a Report folder that is not a full path is refused before any folder is walked.
            if (refused != null)
            {
                log.Line(PackageWords.Prefix + " the NWF tree was not read. " + refused);
                Warn(refused);
                return;
            }

            nwfTree = PackageTree.ReadNwfs(report);
            nwfTicks = new NwfTicks(nwfTree);

            log.Block(PackageWords.NwfTreeTitle, nwfTree.Lines());

            if (nwfTree.WasRead)
            {
                RememberFolder(PickerKind.PackageReport, report);
            }

            FillTheNwfTree();
            ShowTheNwfPreview();
        }

        private void FillTheNwfTree()
        {
            packagePanel.NwfNodes.Clear();

            if (nwfTree == null)
            {
                packagePanel.NwfSummary = PackageWords.NwfNotReadYet;
                return;
            }

            foreach (Community community in nwfTree.Communities)
            {
                PackageNode node = PackageNode.ForCommunity(
                    community,
                    PackageWords.CommunityDetail(community.Packages.Count, community.FileCount, nwfTree.Extension),
                    NwfTickOf,
                    NwfTickIt);

                foreach (Package package in community.Packages)
                {
                    PackageNode line = PackageNode.ForPackage(package, PackageWords.FilesWord(package.Files.Count, nwfTree.Extension), NwfTickOf, NwfTickIt);

                    foreach (string nwf in package.Files)
                    {
                        line.Children.Add(PackageNode.ForNwf(nwf, string.Empty, NwfTickOf, NwfTickIt));
                    }

                    node.Children.Add(line);
                }

                packagePanel.NwfNodes.Add(node);
            }

            packagePanel.NwfSummary = PackageWords.NwfSummary(nwfTree, nwfTicks);
        }

        private bool? NwfTickOf(PackageNode node)
        {
            if (nwfTicks == null)
            {
                return false;
            }

            if (node.Nwf != null)
            {
                return nwfTicks.IsTicked(node.Nwf);
            }

            return node.Package != null ? nwfTicks.StateOf(node.Package) : nwfTicks.StateOf(node.Community);
        }

        private void NwfTickIt(PackageNode node, bool on)
        {
            if (running)
            {
                node.TickMoved();
                return;
            }

            if (nwfTicks != null)
            {
                if (node.Nwf != null)
                {
                    nwfTicks.Tick(node.Nwf, on);
                }
                else if (node.Package != null)
                {
                    nwfTicks.TickPackage(node.Package, on);
                }
                else
                {
                    nwfTicks.TickCommunity(node.Community, on);
                }
            }

            TheNwfTicksMoved();
        }

        private void TheNwfTicksMoved()
        {
            foreach (PackageNode node in packagePanel.NwfNodes)
            {
                node.TickMoved();
            }

            packagePanel.NwfSummary = PackageWords.NwfSummary(nwfTree, nwfTicks);
            ShowTheNwfPreview();
        }

        private void OnTickAllNwfs(object sender, RoutedEventArgs e)
        {
            if (nwfTicks != null && !running)
            {
                nwfTicks.TickAll();
                TheNwfTicksMoved();
            }
        }

        private void OnClearAllNwfs(object sender, RoutedEventArgs e)
        {
            if (nwfTicks != null && !running)
            {
                nwfTicks.ClearAll();
                TheNwfTicksMoved();
            }
        }

        /// <summary>Why no run of the NWF page can start, or null: no tree read, or no NWF ticked.</summary>
        private string WhyTheNwfsCannotRun()
        {
            return nwfTicks == null ? PackageWords.NwfNotReadYet : nwfTicks.WhyNoRun();
        }

        /// <summary>The ticked NWFs of one package of the NWF tree, each with its NWD and Log folder, PackageNwfs.</summary>
        private List<PackageNwf> TickedNwfsOf(string report, Package package)
        {
            return new List<PackageNwf>(PackageNwfs.For(report, package, NwfPickDisk.Real)).FindAll(nwf => nwfTicks.IsTicked(nwf.NwfPath));
        }

        // ---------- the NWF page's route ----------

        /// <summary>
        /// The NWF page's Run, F208 and F232: every ticked NWF of the tree read off the Report folder, opened and run as a picked
        /// NWF is, its NWD and its logs written into its package's own folders. Each NWF runs in its own Navisworks, F214 and
        /// F222, and one RESULT joins them.
        /// </summary>
        private void OnRunNwfPackages(object sender, RoutedEventArgs e)
        {
            if (running || RefuseThe("the run of packages on the NWF page", WhyTheNwfsCannotRun()))
            {
                return;
            }

            ToleranceChoice tolerance = NwfToleranceOrSayWhyNot("the run of packages on the NWF page");

            if (tolerance == null)
            {
                return;
            }

            ReportOptions options = ReportsWanted(tolerance);
            NwfPageSteps nwfSteps = TheNwfSteps();
            nwfSteps.Apply(options);

            // F235, Bader's answer to Q195. The NWF page's run reads the NWF page's own penetration box.
            options.MarkPenetrations = NwfMarkPenetrations.IsChecked == true;
            string report = nwfTree.Parent;
            PackageRun run = new PackageRun(report, options.Disciplines) { Images = options.Images };
            PackageTally tally = run.Tally;
            List<string> nwfs = new List<string>();
            List<PackageNwf> inPackages = new List<PackageNwf>();
            List<string> lines = new List<string>();
            int packagesWithNwfs = 0;

            foreach (Package package in nwfTicks.PackagesTicked())
            {
                tally.Expect(package);
                List<PackageNwf> found;

                try
                {
                    found = TickedNwfsOf(report, package);
                }
                catch (Exception error)
                {
                    string why = "its NWF folder could not be read, " + error.Message;
                    tally.NotRun(package, why);
                    lines.Add(package.RelativePath + "  NOT RUN  " + why);
                    continue;
                }

                // F232, the breaker's reading: a ticked NWF that has gone since the Read is named and counted, never dropped in silence.
                foreach (string ticked in package.Files)
                {
                    if (nwfTicks.IsTicked(ticked) && !found.Exists(nwf => string.Equals(nwf.NwfPath, ticked, StringComparison.OrdinalIgnoreCase)))
                    {
                        string gone = PackageWords.TickedNwfGone(ticked);
                        tally.NotRun(package, Path.GetFileNameWithoutExtension(ticked), gone);
                        lines.Add(package.RelativePath + "  NOT RUN  " + gone);
                    }
                }

                if (found.Count == 0)
                {
                    string why = PackageWords.NoNwfYet(package.RelativePath, PackageOutput.NwfFolderFor(report, package));
                    tally.NotRun(package, why);
                    lines.Add(why);
                    continue;
                }

                packagesWithNwfs++;
                run.Runnable.Add(package);

                foreach (PackageNwf nwf in found)
                {
                    nwfs.Add(nwf.NwfPath);
                    inPackages.Add(nwf);
                    lines.Add(package.RelativePath + "  runs  " + nwf.NwfPath + ", NWD " + nwf.NwdPath + ", Log " + nwf.LogFolder);
                }
            }

            log.Block(PackageWords.NwfTreeTitle, nwfTree.Lines());
            log.Block(PackageWords.NwfChosenTitle, nwfTicks.Lines());
            log.Block("PACKAGE NWFS", lines);

            // F222, Q211. A group's own Navisworks runs its one NWF, the one the order names, and no other. Where its package holds
            // no NWF at all the refusal below gives the real reason.
            if (child != null && nwfs.Count > 0)
            {
                inPackages = inPackages.FindAll(nwf => child.Unit.IsTheGroupOf(nwf.NwfPath));
                nwfs = inPackages.ConvertAll(nwf => nwf.NwfPath);

                if (nwfs.Count == 0)
                {
                    childRanNoGroupBecause = PackageRunWords.NotTheGroup(child.Unit.Label, child.Unit.Group);
                    log.Line(PackageRunWords.Prefix + " " + childRanNoGroupBecause);
                    return;
                }
            }

            if (nwfs.Count == 0)
            {
                log.Line(PackageWords.Prefix + " the run of packages on the NWF page refused before starting. No ticked NWF is there, each package is named above");
                Warn("No ticked NWF is there under the Report folder. Read the Report folder again.");
                return;
            }

            // F214. Nothing is asked about the document open here: since F214 every NWF opens in its own fresh Navisworks, so
            // what is open in this one is never touched. F217, Bader's F-R5. The old pictures counted across every NWF of the
            // run, each in its own package's Clash report folder.
            List<string> pictureFolders = new List<string>();

            foreach (PackageNwf nwf in inPackages)
            {
                pictureFolders.Add(RefreshConfirm.PicturesFolderOf(nwf.LogFolder, nwf.NwfPath));
            }

            string text = RefreshConfirm.Text(
                nwfs.Count + " NWFs of " + packagesWithNwfs + " packages",
                PackageWords.BackupOfEach,
                null,
                nwfSteps.Words(),
                TestsInTheMatrix(),
                -1,
                -1,
                OldPicturesIn(pictureFolders),
                ToolViewFolder.Name,
                options.MarkPenetrations && options.RunTests);

            log.Block("CONFIRM packages", new List<string>(text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)));

            if (child != null)
            {
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ConfirmedByTheParent("The refresh"));
            }
            else if (MessageBox.Show(this, text, RefreshConfirm.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel)
                != MessageBoxResult.OK)
            {
                Log("Run cancelled before anything was opened.");
                return;
            }

            List<KeyValuePair<string, IList<string>>> pickedGroups = new List<KeyValuePair<string, IList<string>>>();

            foreach (string nwf in nwfs)
            {
                pickedGroups.Add(PackChoice.Group(nwf, new List<string> { nwf }));
            }

            if (!ChoosePackOrStop(pickedGroups, "the run of packages on the NWF page"))
            {
                return;
            }

            // F214 and F222. The window a person runs hands each NWF to a fresh Navisworks, and a group's own window runs it.
            if (child == null)
            {
                List<PackageRunUnit> units = new List<PackageRunUnit>();
                Dictionary<string, List<string>> groupsOfEachCommunity = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, Dictionary<string, string>> foldersOfEachCommunity =
                    new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

                foreach (PackageNwf nwf in inPackages)
                {
                    units.Add(new PackageRunUnit(nwf.Package.RelativePath, nwf.NwfPath, Path.GetFileNameWithoutExtension(nwf.NwfPath)));
                    GroupsOf(groupsOfEachCommunity, nwf.Package.Community).Add(Path.GetFileNameWithoutExtension(nwf.NwfPath));
                    FoldersOf(foldersOfEachCommunity, nwf.Package.Community)[nwf.Package.RelativePath] = PackageOutput.FolderFor(report, nwf.Package);
                }

                // F243 point 7. The old names, once, before any NWF opens: each NWF of the run by its name, the name its group's
                // workbook and backup carried on this page, and each package's own report folder, under its community.
                CleanTheOldReportFiles(report, foldersOfEachCommunity, groupsOfEachCommunity, run.Disciplines);

                RunPackagesInFreshStarts(PackageRunOrder.NwfPage, report, run, units);
                return;
            }

            nwfTable.Clear();
            running = true;
            SetTheRunButtons(false);
            FederationEngine engine = null;
            AutoSaveOff autoSave = null;

            try
            {
                log.RunStarted(nwfs.Count);

                // F223, Q207. Auto-Save off while the run works, put back in the finally before RESULT.
                autoSave = new AutoSaveOff(log.Line);
                log.Block("RUN SETTINGS", new List<string>
                {
                    "report folder    : " + report,
                    "NWFs             : " + nwfs.Count + " in " + packagesWithNwfs + " of " + nwfTicks.PackagesTicked().Count + " packages with an NWF ticked",
                    "steps            : " + nwfSteps.Words()
                });
                SayTheRunsChoices(options.Tolerance, options.MarkPenetrations && options.RunTests);

                TeamMap teams;
                ExchangeDocument exchange = ReadTheMatrixForAnOpenFileRun(out teams);

                engine = new FederationEngine(SetProgress, log, exchange, teams, options);
                engine.UsePack(chosenPack);
                IList<JobOutcome> outcomes = engine.RunPickedNwfs(nwfs, inPackages);
                ShowTheNwfTable(outcomes);

                for (int i = 0; i < outcomes.Count && i < inPackages.Count; i++)
                {
                    JobOutcome outcome = outcomes[i];

                    // F233. An NWF of a group of one discipline was opened and not built, so it is counted as that.
                    if (outcome != null && outcome.NotBuiltBecause != null)
                    {
                        tally.NotBuilt(inPackages[i].Package, outcome.Job == null ? inPackages[i].NwfPath : outcome.Job.Building);
                        continue;
                    }

                    tally.Add(
                        inPackages[i].Package,
                        outcome == null || outcome.Job == null ? inPackages[i].NwfPath : outcome.Job.Building,
                        outcome == null ? GroupOutcome.Failed : outcome.Result,
                        outcome == null ? -1 : GroupSize.ClashesIn(outcome.Clash));
                }

                // F222. No discipline workbook is written here: the window that started the run writes them off this NWF's result.
                SayWhatAnOpenFileRunAddedUp(engine);
                log.Block(SetsAcrossTheRun.BlockTitle, engine.SetsAcrossTheRun.Lines());
                log.RunFinished();
                SetProgress(RunFinishedLine());
            }
            catch (Exception error)
            {
                log.Failure("the run of packages on the NWF page", error, "stopped, everything already written is kept");
                SetProgress("The run of packages stopped on an error.");
                Warn("The run stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                if (autoSave != null)
                {
                    autoSave.Dispose();
                }

                WriteTheResult(engine, tally.Lines());

                // F214. The NWFs this run ended with, those before a throw among them, for a package's own result.
                lastOutcomes = engine == null ? null : engine.OutcomesSoFar;
                running = false;
                SetTheRunButtons(true);
                ShowOpenDocument();
            }
        }

        /// <summary>The two buttons that start a run of a page, off while one goes and on after.</summary>
        private void SetTheRunButtons(bool on)
        {
            RunButton.IsEnabled = on;
            RunNwfPackagesButton.IsEnabled = on;
        }
    }
}
