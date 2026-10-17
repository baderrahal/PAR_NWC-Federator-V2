using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Grouping;
using Federator.Core.Communities;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Sets;
using Federator.Core.Teams;
using Federator.Core.Views;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// Many communities at once, F208, Bader's message of 17 Oct 2026. One parent folder, the first level under it the
    /// communities, every folder below holding NWC files directly one package, a tree of them with a tick box each on the
    /// NWC page's step 5 and on the NWF page, nothing ticked at the start. The NWC page builds each ticked package in turn
    /// and the NWF page runs the NWFs each ticked package holds, every output under the package's own report folder. The
    /// rules and the words are Core's, Federator.Core.Communities, and are tested there. This part reads the disk and the
    /// controls, and calls the engine.
    /// </summary>
    public partial class FederatorWindow
    {
        private readonly PackagePanel packagePanel = new PackagePanel();
        private PackageTree packageTree;
        private PackageTicks packageTicks;

        /// <summary>Called once from the constructor: both panels on the one data, the folders as they were last picked.</summary>
        private void StartThePackages()
        {
            PackagesPanel.DataContext = packagePanel;
            NwfPackagesPanel.DataContext = packagePanel;
            PackagesStep.Header = PackageWords.StepHeader;
            packagePanel.Parent = folders.LastFor(PickerKind.PackageParent);
            packagePanel.Report = folders.LastFor(PickerKind.PackageReport);
        }

        private void OnBrowsePackageParent(object sender, RoutedEventArgs e)
        {
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
        /// and package with its path and what was left out.
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
        }

        /// <summary>The lines of both trees, made again from the tree read, each package with its files and its groups.</summary>
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
                    package, Trimmed(packagePanel.Report), ChosenGrouping(), settings, naming, internalWorkbook, DateTime.Today);
                return PackageWords.Detail(package.Files.Count, packageTree.Extension, plan.Groups.Count, plan.Blocked);
            }
            catch (Exception error)
            {
                log.Failure("counting the groups of " + package.RelativePath, error, "kept going, its line shows the files alone");
                return PackageWords.FilesWord(package.Files.Count, packageTree.Extension);
            }
        }

        /// <summary>The group counts again, when the packages step is shown, since the grouping may have changed since the Read.</summary>
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
            if (packageTicks != null && !running)
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

        private void TheTicksMoved()
        {
            foreach (PackageNode node in packagePanel.Nodes)
            {
                node.TickMoved();
            }

            packagePanel.Summary = PackageWords.Summary(packageTree, packageTicks);
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
        /// Why no run of packages can start, or null: no tree read, nothing ticked, or a Report folder the rule refuses.
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

            return PackageOutput.WhyNot(packageTree.Parent, packagePanel.Report);
        }

        private bool RefuseThePackages(string what)
        {
            string why = WhyThePackagesCannotRun();

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

        // ---------- the NWC page's route ----------

        /// <summary>
        /// Builds every ticked package in turn, F208: each package's files grouped and named by steps 2 and 3, each group's
        /// NWF and NWD in the package's own folders and its logs and reports in the package's Log folder, a group whose NWF
        /// is already there left to the NWF page unless Rebuild is ticked. A package that cannot run is named and the others
        /// run. One engine for the whole run, so RESULT is one block with each group's community and package and the totals.
        /// </summary>
        private void OnRunPackages(object sender, RoutedEventArgs e)
        {
            if (running || RefuseThePackages("the run of packages"))
            {
                return;
            }

            ToleranceChoice tolerance = ToleranceOrSayWhyNot("the run of packages");

            if (tolerance == null)
            {
                return;
            }

            ReadNaming();
            GroupingMode mode = ChosenGrouping();
            string report = Trimmed(packagePanel.Report);
            ReportOptions options = ReportsWanted(tolerance);
            PackageTally tally = new PackageTally();
            List<FederationJob> jobs = new List<FederationJob>();
            List<Package> runnable = new List<Package>();
            List<string> planLines = new List<string>();
            int tickedFiles = 0;

            foreach (Package package in packageTicks.Ticked())
            {
                tally.Expect(package);
                tickedFiles += package.Files.Count;
                PackagePlan plan = PackagePlan.For(package, report, mode, settings, naming, internalWorkbook, DateTime.Today);

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
                    if (!NwcPageRule.CanRun(false, File.Exists(group.NwfPath), RebuildExisting.IsChecked == true))
                    {
                        planLines.Add(group.Label + "  left out, " + NwcPageRule.ExistsLine + " " + group.NwfPath);
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
                    tally.NotRun(package, "every group of it has an NWF already. " + NwcPageRule.ExistsLine);
                }
                else
                {
                    runnable.Add(package);
                }
            }

            log.Block(PackageWords.TreeTitle, packageTree.Lines());
            log.Block(PackageWords.ChosenTitle, packageTicks.Lines());
            log.Block("PACKAGE GROUPS", planLines);

            if (jobs.Count == 0)
            {
                log.Line(PackageWords.Prefix + " the run of packages refused before starting. No group of the ticked packages can run, each is named above");
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
                false,
                "the NWF folder of each package under " + report,
                "the NWD folder of each package under " + report,
                packageTree.FileCount,
                tickedFiles,
                jobs.Count);
            log.Line("grouping         : " + GroupingModes.Describe(mode));
            SayTheRunsChoices(tolerance);

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

            RunJobs(jobs, null, null, tolerance, tally);
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

        // ---------- the NWF page's route ----------

        /// <summary>
        /// Runs the NWFs of every ticked package, F208: each NWF in the NWF folder under the package's report folder, opened
        /// and run as a picked NWF is, its NWD and its logs written into the package's own folders. A ticked package holding
        /// no NWF there is named and the others run. One engine for the whole run, one RESULT with the totals.
        /// </summary>
        private void OnRunNwfPackages(object sender, RoutedEventArgs e)
        {
            if (running || RefuseThePackages("the run of packages on the NWF page"))
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
            string report = Trimmed(packagePanel.Report);
            PackageTally tally = new PackageTally();
            List<string> nwfs = new List<string>();
            List<PackageNwf> inPackages = new List<PackageNwf>();
            List<string> lines = new List<string>();
            int packagesWithNwfs = 0;

            foreach (Package package in packageTicks.Ticked())
            {
                tally.Expect(package);
                IList<PackageNwf> found;

                try
                {
                    found = PackageNwfs.For(report, package, NwfPickDisk.Real);
                }
                catch (Exception error)
                {
                    string why = "its NWF folder could not be read, " + error.Message;
                    tally.NotRun(package, why);
                    lines.Add(package.RelativePath + "  NOT RUN  " + why);
                    continue;
                }

                if (found.Count == 0)
                {
                    string why = PackageWords.NoNwfYet(package.RelativePath, PackageOutput.NwfFolderFor(report, package));
                    tally.NotRun(package, why);
                    lines.Add(why);
                    continue;
                }

                packagesWithNwfs++;

                foreach (PackageNwf nwf in found)
                {
                    nwfs.Add(nwf.NwfPath);
                    inPackages.Add(nwf);
                    lines.Add(package.RelativePath + "  runs  " + nwf.NwfPath + ", NWD " + nwf.NwdPath + ", Log " + nwf.LogFolder);
                }
            }

            log.Block(PackageWords.TreeTitle, packageTree.Lines());
            log.Block(PackageWords.ChosenTitle, packageTicks.Lines());
            log.Block("PACKAGE NWFS", lines);

            if (nwfs.Count == 0)
            {
                log.Line(PackageWords.Prefix + " the run of packages on the NWF page refused before starting. No ticked package holds an NWF, each is named above");
                Warn("No ticked package holds an NWF under the Report folder. Run the NWC page on them first.");
                return;
            }

            if (!ConfirmTheOpenDocumentGoes())
            {
                Log("Run cancelled before anything was opened.");
                return;
            }

            string text = RefreshConfirm.Text(
                nwfs.Count + " NWFs of " + packagesWithNwfs + " packages",
                null,
                null,
                nwfSteps.Words(),
                TestsInTheMatrix(),
                -1,
                -1,
                -1,
                ToolViewFolder.Name);

            log.Block("CONFIRM packages", new List<string>(text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)));

            if (MessageBox.Show(this, text, RefreshConfirm.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel)
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

            nwfTable.Clear();
            running = true;
            SetTheRunButtons(false);
            FederationEngine engine = null;

            try
            {
                log.RunStarted(nwfs.Count);
                log.Block("RUN SETTINGS", new List<string>
                {
                    "parent folder    : " + packageTree.Parent,
                    "report folder    : " + report,
                    "NWFs             : " + nwfs.Count + " in " + packagesWithNwfs + " of " + packageTicks.Ticked().Count + " ticked packages",
                    "steps            : " + nwfSteps.Words()
                });
                SayTheRunsChoices(options.Tolerance);

                TeamMap teams;
                ExchangeDocument exchange = ReadTheMatrixForAnOpenFileRun(out teams);

                engine = new FederationEngine(SetProgress, log, exchange, teams, options);
                engine.UsePack(chosenPack);
                IList<JobOutcome> outcomes = engine.RunPickedNwfs(nwfs, inPackages);
                ShowTheNwfTable(outcomes);

                for (int i = 0; i < outcomes.Count && i < inPackages.Count; i++)
                {
                    JobOutcome outcome = outcomes[i];
                    tally.Add(
                        inPackages[i].Package,
                        outcome == null || outcome.Job == null ? inPackages[i].NwfPath : outcome.Job.Building,
                        outcome == null ? GroupOutcome.Failed : outcome.Result,
                        outcome == null ? -1 : GroupSize.ClashesIn(outcome.Clash));
                }

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
                WriteTheResult(engine, null, tally.Lines());
                running = false;
                SetTheRunButtons(true);
                ShowOpenDocument();
            }
        }

        /// <summary>Every button that starts a run, off while one goes and on after, the packages' among them.</summary>
        private void SetTheRunButtons(bool on)
        {
            RunButton.IsEnabled = on;
            RunPickedNwfsButton.IsEnabled = on;
            RunPackagesButton.IsEnabled = on;
            RunNwfPackagesButton.IsEnabled = on;
        }
    }
}
