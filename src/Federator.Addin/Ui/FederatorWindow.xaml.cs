using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Federator.Addin.Engine;
using Federator.Core.Diagnostics;
using Federator.Core.Findings;
using Federator.Core.Grouping;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using Federator.Core.Sets;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// Three steps: source, grouping, outputs. The window does the deciding. The engine
    /// does the work, on this same thread.
    /// </summary>
    public partial class FederatorWindow : Window
    {
        private readonly ContainerNameSettings settings = new ContainerNameSettings();
        private readonly ObservableCollection<FileRow> files = new ObservableCollection<FileRow>();
        private readonly ObservableCollection<GroupRow> groups = new ObservableCollection<GroupRow>();
        private readonly RunLog log;
        private ScanFindings findings = ScanFindings.From(new List<BuildingGroup>());
        private bool running;
        private bool suspendRegroup;

        public FederatorWindow(RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;

            InitializeComponent();

            FilesGrid.ItemsSource = files;
            GroupsGrid.ItemsSource = groups;
            OutputsGrid.ItemsSource = groups;

            files.CollectionChanged += delegate { Regroup(); };

            // Every line the log writes appears in the window as it is written, so the run
            // is watched rather than read afterwards.
            log.LineWritten += OnLogLine;
            Closed += delegate { log.LineWritten -= OnLogLine; };

            // Which binary this is, in the title bar, because a stale install is otherwise
            // invisible and has caught Bader out twice.
            Title = "Parsons NWC Federator   [" + BuildStamp.Of(typeof(FederatorWindow).Assembly) + "]";

            LogBox.AppendText(log.ReadAll());
            LogBox.ScrollToEnd();

            ProgressLine.Text = log.IsWritingToDisk
                ? "Log: " + log.Path
                : "WARNING the log is not being written to disk. " + log.DisabledReason;
        }

        private void OnLogLine(string line)
        {
            LogBox.AppendText(line + Environment.NewLine);
            LogBox.ScrollToEnd();
        }

        // ---------- Step 1, source ----------

        private void OnBrowseSource(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder("Pick the folder holding the NWC files", SourceFolderBox.Text);

            if (picked != null)
            {
                SourceFolderBox.Text = picked;
            }
        }

        private void OnScan(object sender, RoutedEventArgs e)
        {
            string folder = SourceFolderBox.Text == null ? string.Empty : SourceFolderBox.Text.Trim();

            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                Warn("Pick a folder that exists first.");
                return;
            }

            foreach (FileRow row in files)
            {
                row.PropertyChanged -= OnFileRowChanged;
            }

            files.Clear();

            bool subfolders = IncludeSubfolders.IsChecked == true;
            SearchOption depth = subfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            log.ScanStarted(folder, subfolders);

            string[] found;

            try
            {
                found = Directory.GetFiles(folder, "*.nwc", depth);
            }
            catch (Exception error)
            {
                log.Failure("scanning " + folder, error, "stopped the scan, nothing was changed");
                Warn("The folder could not be read." + Environment.NewLine + error.Message);
                return;
            }

            Array.Sort(found, StringComparer.OrdinalIgnoreCase);

            int unreadable = 0;

            // Grouping every time a row lands would regroup once per file. Do it once at
            // the end instead.
            suspendRegroup = true;

            try
            {
                foreach (string path in found)
                {
                    FileRow row = new FileRow(path, ContainerName.Parse(path, settings));
                    row.PropertyChanged += OnFileRowChanged;
                    files.Add(row);

                    if (!row.IsReadable)
                    {
                        unreadable++;
                        log.UnreadableFile(row.FileName, row.Reason);
                    }
                }
            }
            finally
            {
                suspendRegroup = false;
            }

            SourceSummary.Text = found.Length + " NWC found, " + (found.Length - unreadable)
                + " readable, " + unreadable + " that cannot be read.";

            log.ScanFinished(found.Length, found.Length - unreadable, unreadable);

            Regroup();
            Steps.SelectedIndex = 1;
        }

        private void OnFileRowChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Include")
            {
                Regroup();
            }
        }

        // ---------- Step 2, grouping ----------

        private void Regroup()
        {
            if (suspendRegroup)
            {
                return;
            }

            groups.Clear();

            List<ParsedContainerName> ticked = new List<ParsedContainerName>();
            Dictionary<string, List<string>> pathsByStem = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (FileRow row in files)
            {
                if (!row.Include || !row.IsReadable)
                {
                    continue;
                }

                ticked.Add(row.Parsed);

                List<string> paths;

                if (!pathsByStem.TryGetValue(row.Parsed.Stem, out paths))
                {
                    paths = new List<string>();
                    pathsByStem.Add(row.Parsed.Stem, paths);
                }

                paths.Add(row.FullPath);
            }

            BuildingGroupingResult result = BuildingGrouping.Group(ticked);

            foreach (BuildingGroup group in result.Groups)
            {
                groups.Add(GroupRow.Usable(
                    group.Building,
                    PathsFor(group.Files, pathsByStem),
                    group.Disciplines,
                    ContainerName.BuildOutputName(group.Project, group.Originator, group.Building, settings)));
            }

            foreach (SkippedBuildingGroup skipped in result.Skipped)
            {
                groups.Add(GroupRow.Blocked(
                    skipped.Building,
                    PathsFor(skipped.Files, pathsByStem),
                    skipped.Reason));
            }

            int blocked = result.Skipped.Count;
            GroupingSummary.Text = result.Groups.Count + " groups ready, " + blocked + " blocked.";

            // Worked out here so it is on screen before Run is pressed, not after.
            findings = ScanFindings.From(result);
            ShowFindings();

            RefreshOutputsSummary();
        }

        /// <summary>
        /// The findings panel. Nothing here blocks a run, it is information and the
        /// decision stays with the person reading it.
        /// </summary>
        private void ShowFindings()
        {
            FindingsHeading.Text = findings.Any
                ? "Findings: " + findings.Count + (findings.Count == 1 ? " thing" : " things")
                    + " worth a look. None of this stops a run."
                : "Findings";

            FindingsBox.Text = string.Join(
                Environment.NewLine, new List<string>(findings.Lines()).ToArray());
        }

        private static IList<string> PathsFor(
            IEnumerable<ParsedContainerName> parsedFiles, IDictionary<string, List<string>> pathsByStem)
        {
            List<string> paths = new List<string>();

            foreach (ParsedContainerName parsed in parsedFiles)
            {
                List<string> found;

                if (pathsByStem.TryGetValue(parsed.Stem, out found))
                {
                    foreach (string path in found)
                    {
                        if (!paths.Contains(path))
                        {
                            paths.Add(path);
                        }
                    }
                }
            }

            return paths;
        }

        // ---------- Step 3, outputs ----------

        private void OnBrowseNwf(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder("Pick the folder for the NWF files", NwfFolderBox.Text);

            if (picked != null)
            {
                NwfFolderBox.Text = picked;
                RefreshOutputsSummary();
            }
        }

        private void OnBrowseNwd(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder("Pick the folder for the NWD files", NwdFolderBox.Text);

            if (picked != null)
            {
                NwdFolderBox.Text = picked;
                RefreshOutputsSummary();
            }
        }

        private int TickedFileCount()
        {
            int ticked = 0;

            foreach (FileRow row in files)
            {
                if (row.Include && row.IsReadable)
                {
                    ticked++;
                }
            }

            return ticked;
        }

        private void OnRepublishChanged(object sender, RoutedEventArgs e)
        {
            RefreshOutputsSummary();
        }

        private void RefreshOutputsSummary()
        {
            // IsChecked="True" in the XAML raises Checked while the tree is still being
            // built, so this can be reached before the controls it reads exist.
            if (OutputsSummary == null || RepublishNwd == null)
            {
                return;
            }

            int ready = 0;

            foreach (GroupRow group in groups)
            {
                if (group.Include)
                {
                    ready++;
                }
            }

            string nwd = RepublishNwd.IsChecked == true
                ? "The NWD is republished every run."
                : "The NWD is NOT being republished.";

            OutputsSummary.Text = ready + " groups ticked to run. " + nwd + " The log is written to "
                + (log.IsWritingToDisk ? log.Path : "the window only")
                + " and copied next to the NWF folder at the end.";
        }

        // ---------- Run ----------

        private void OnRun(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            string nwfFolder = NwfFolderBox.Text == null ? string.Empty : NwfFolderBox.Text.Trim();
            string nwdFolder = NwdFolderBox.Text == null ? string.Empty : NwdFolderBox.Text.Trim();

            if (nwfFolder.Length == 0 || nwdFolder.Length == 0)
            {
                Warn("Pick an NWF folder and an NWD folder in step 3 first.");
                Steps.SelectedIndex = 2;
                return;
            }

            List<FederationJob> jobs = new List<FederationJob>();

            foreach (GroupRow group in groups)
            {
                if (!group.Include || group.IsBlocked || group.Files.Count == 0)
                {
                    continue;
                }

                jobs.Add(new FederationJob(
                    group.Building,
                    group.OutputName,
                    Path.Combine(nwfFolder, group.OutputName + ".nwf"),
                    Path.Combine(nwdFolder, group.OutputName + ".nwd"),
                    group.Files));
            }

            if (jobs.Count == 0)
            {
                Warn("No group is ticked to run.");
                return;
            }

            if (!ConfirmClear(jobs.Count))
            {
                Log("Run cancelled before anything was cleared.");
                return;
            }

            log.RunSettings(
                SourceFolderBox.Text,
                IncludeSubfolders.IsChecked == true,
                nwfFolder,
                nwdFolder,
                files.Count,
                TickedFileCount(),
                groups.Count);

            log.Line("republish NWD    : " + (RepublishNwd.IsChecked == true ? "yes" : "no"));
            log.Block(RunLog.GroupsSectionTitle, GroupListLines());
            log.Block(RunLog.FindingsSectionTitle, findings.Lines());

            RunJobs(jobs, nwfFolder);
        }

        /// <summary>
        /// One line per group, so the findings block that follows has something to refer
        /// to and the log reads on its own.
        /// </summary>
        private IList<string> GroupListLines()
        {
            List<string> lines = new List<string>();

            foreach (GroupRow group in groups)
            {
                string state = group.IsBlocked ? "BLOCKED" : (group.Include ? "run    " : "skipped");

                lines.Add(state + "  " + group.Building.PadRight(10)
                    + group.FileCount.ToString().PadLeft(3)
                    + (group.FileCount == 1 ? " file   " : " files  ")
                    + (group.OutputName.Length == 0 ? "no output name" : group.OutputName)
                    + "  [" + group.Disciplines + "]");

                if (group.IsBlocked)
                {
                    lines.Add("          " + group.BlockedReason);
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("No groups.");
            }

            return lines;
        }

        /// <summary>
        /// Clearing throws away whatever is open, so the warning names it and the user
        /// can cancel. Asked once, before the first group.
        /// </summary>
        private bool ConfirmClear(int jobCount)
        {
            string discarded;

            try
            {
                discarded = DocumentGuard.WhatClearWouldDiscard();
            }
            catch (Exception error)
            {
                discarded = "Navisworks would not say what is open (" + error.Message + ").";
            }

            string message =
                "This run federates " + jobCount + (jobCount == 1 ? " group." : " groups.")
                + Environment.NewLine + Environment.NewLine
                + "Before each group the document is cleared.";

            if (discarded != null)
            {
                message += Environment.NewLine + Environment.NewLine
                    + "This will be discarded without saving:" + Environment.NewLine
                    + "    " + discarded;
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine
                    + "Nothing is open at the moment, so nothing is lost.";
            }

            message += Environment.NewLine + Environment.NewLine + "Carry on?";

            MessageBoxResult answer = MessageBox.Show(
                this, message, "Parsons NWC Federator", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            return answer == MessageBoxResult.OK;
        }

        /// <summary>
        /// Runs on the plugin thread. The dispatcher is pumped between groups so the log
        /// and the progress line repaint, which is not the same as moving the work onto a
        /// background thread. No Navisworks call may leave this thread.
        /// </summary>
        private void RunJobs(IList<FederationJob> jobs, string nwfFolder)
        {
            running = true;
            RunButton.IsEnabled = false;

            try
            {
                log.Line("RUN      started, " + jobs.Count + " groups");

                FederationEngine engine = new FederationEngine(
                    SetProgress, log, RepublishNwd.IsChecked == true);
                engine.Run(jobs);

                log.Line("RUN      finished");
                SetProgress("Run finished. " + log.CountOf(GroupOutcome.Done) + " done, "
                    + log.CountOf(GroupOutcome.Partial) + " partial, "
                    + log.CountOf(GroupOutcome.Failed) + " failed.");
            }
            catch (Exception error)
            {
                log.Failure("the run", error, "stopped, everything already written is kept");
                SetProgress("Run stopped on an error.");
                Warn("The run stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                // The result block and the second copy are written whatever happened, so a
                // run that stopped still leaves a readable log with its summary at the end.
                WriteTheResultAndCopyTheLog(nwfFolder);
                running = false;
                RunButton.IsEnabled = true;
            }
        }

        private void WriteTheResultAndCopyTheLog(string nwfFolder)
        {
            try
            {
                log.WriteResultBlock();
            }
            catch (Exception error)
            {
                log.Failure("writing the result block", error, "kept going, the lines above are still on disk");
            }

            string copied;

            // A failure here is written into the first log and then ignored. Logging is
            // never the thing that stops a run.
            log.TryCopyTo(nwfFolder, out copied);
        }

        // ---------- Step 4, clash. Sets only in this session ----------

        private void OnBrowseSetsFile(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "Pick the sets or combined XML";
                dialog.Filter = "Navisworks exchange XML (*.xml)|*.xml|All files (*.*)|*.*";
                dialog.CheckFileExists = true;

                string current = SetsFileBox.Text == null ? string.Empty : SetsFileBox.Text.Trim();

                if (current.Length > 0)
                {
                    try
                    {
                        string folder = System.IO.Path.GetDirectoryName(current);

                        if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                        {
                            dialog.InitialDirectory = folder;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // A path that cannot be read is not worth failing the browse over.
                    }
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SetsFileBox.Text = dialog.FileName;
                    SetsSummary.Text = "Picked " + dialog.FileName + ". Press Build sets.";
                }
            }
        }

        /// <summary>
        /// Reads whichever file was picked and rebuilds its sets into whatever document is
        /// open. Runs on the plugin thread, like everything else that touches the API.
        /// </summary>
        private void OnBuildSets(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            string path = SetsFileBox.Text == null ? string.Empty : SetsFileBox.Text.Trim();

            if (path.Length == 0 || !File.Exists(path))
            {
                Warn("Pick a sets or combined XML that exists first.");
                return;
            }

            running = true;
            BuildSetsButton.IsEnabled = false;

            try
            {
                log.Line("SETS     started, reading " + path);
                ExchangeDocument exchange = new ExchangeReader().ReadFile(path);

                log.Line("SETS     the file holds " + exchange.Sets.Count
                    + (exchange.Sets.Count == 1 ? " set and " : " sets and ")
                    + exchange.Tests.Count
                    + (exchange.Tests.Count == 1 ? " test" : " tests"));

                SetBuildPlan plan = SetBuildPlan.From(exchange);

                foreach (string unknown in plan.UnknownTestValues)
                {
                    log.Line("SETS     condition test \"" + unknown
                        + "\" is not one this tool rebuilds, every set using it is skipped");
                }

                if (!plan.HasWork)
                {
                    // A project keeping its sets in the model and supplying only tests is a
                    // normal case, not an error.
                    string nothing = plan.Skipped.Count > 0
                        ? "No set in this file can be rebuilt. " + plan.Skipped.Count + " skipped."
                        : "This file holds no sets. Nothing to build.";

                    log.Line("SETS     " + nothing);
                    SetsSummary.Text = nothing;
                    ShowSetLines(PlanOnlyLines(plan));
                    return;
                }

                log.Line("SETS     " + plan.Buildable.Count + " to build, "
                    + plan.Skipped.Count + " skipped, deepest folder depth "
                    + plan.DeepestFolderDepth());

                SetBuilder builder = new SetBuilder(SetProgress, log);
                SetBuildOutcome outcome = builder.Build(plan);

                log.Block(SetsSectionTitle, outcome.Lines());
                ShowSetLines(outcome.Lines());

                string summary = outcome.CreatedCount + " created, "
                    + outcome.FindingItemsCount + " finding items, "
                    + outcome.ZeroCount + " at zero"
                    + (outcome.FailedCount > 0 ? ", " + outcome.FailedCount + " failed" : string.Empty)
                    + (outcome.SkippedCount > 0 ? ", " + outcome.SkippedCount + " skipped" : string.Empty)
                    + ".";

                SetsSummary.Text = summary;
                SetProgress("Sets finished. " + summary);
                log.Line("SETS     finished. " + summary);
            }
            catch (Exception error)
            {
                log.Failure("building the sets from " + path, error, "stopped, nothing further was built");
                SetProgress("Building the sets stopped on an error.");
                Warn("Building the sets stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                running = false;
                BuildSetsButton.IsEnabled = true;
            }
        }

        private const string SetsSectionTitle = "SETS";

        private static IList<string> PlanOnlyLines(SetBuildPlan plan)
        {
            List<string> lines = new List<string>();

            foreach (SkippedSet skipped in plan.Skipped)
            {
                lines.Add("SKIPPED " + skipped.Path + "  " + skipped.Reason);
            }

            if (lines.Count == 0)
            {
                lines.Add("This file holds no sets.");
            }

            return lines;
        }

        private void ShowSetLines(IEnumerable<string> lines)
        {
            SetsBox.Text = string.Join(Environment.NewLine, new List<string>(lines).ToArray());
        }

        // ---------- Small helpers ----------

        private void SetProgress(string text)
        {
            ProgressLine.Text = text;
            Pump();
        }

        private void Log(string text)
        {
            log.Line(text);
            Pump();
        }

        /// <summary>
        /// Lets the window repaint without handing the work to another thread. Everything
        /// still runs on the plugin thread.
        /// </summary>
        private static void Pump()
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        private void Warn(string text)
        {
            MessageBox.Show(this, text, "Parsons NWC Federator", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private string PickFolder(string title, string startAt)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = title;
                dialog.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(startAt) && Directory.Exists(startAt))
                {
                    dialog.SelectedPath = startAt;
                }

                return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                    ? dialog.SelectedPath
                    : null;
            }
        }

        /// <summary>Wraps a path in quotes so a folder with spaces reaches Explorer whole.</summary>
        private static string Quoted(string path)
        {
            return "\"" + path + "\"";
        }

        private void OnOpenLogFolder(object sender, RoutedEventArgs e)
        {
            string folder = log.IsWritingToDisk
                ? System.IO.Path.GetDirectoryName(log.Path)
                : RunLog.DefaultLogFolder();

            try
            {
                Directory.CreateDirectory(folder);

                if (log.IsWritingToDisk)
                {
                    // Opens the folder with this run's log already picked out.
                    Process.Start("explorer.exe", "/select," + Quoted(log.Path));
                }
                else
                {
                    Process.Start("explorer.exe", Quoted(folder));
                }
            }
            catch (Exception error)
            {
                log.Failure("opening the log folder " + folder, error, "kept going, nothing else changed");
                Warn("The log folder could not be opened." + Environment.NewLine
                    + folder + Environment.NewLine + Environment.NewLine + error.Message);
            }
        }

        private void OnCopyLog(object sender, RoutedEventArgs e)
        {
            try
            {
                string all = log.ReadAll();
                Clipboard.SetText(all);
                SetProgress("Whole log copied to the clipboard, " + all.Length + " characters.");
            }
            catch (Exception error)
            {
                log.Failure("copying the log to the clipboard", error, "kept going, nothing else changed");
                Warn("The log could not be copied." + Environment.NewLine + Environment.NewLine + error.Message);
            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                Warn("The run is still going. Let it finish.");
                return;
            }

            Close();
        }
    }
}
