using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Findings;
using Federator.Core.Grouping;
using Federator.Core.Exchange;
using Federator.Core.Naming;
using Federator.Core.Report;
using Federator.Core.Rerun;
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

        /// <summary>
        /// The Revit source findings from the last run. They cannot exist before a run,
        /// because nothing knows what an NWC was published from until a document has been
        /// opened, so the panel says so rather than pretending there are none.
        /// </summary>
        private SourceMismatchFindings sourceFindings;

        private ScanCounts counts = new ScanCounts();
        private IList<BuildingGroup> lastGroups = new List<BuildingGroup>();
        private readonly OutputNaming naming = new OutputNaming();
        private bool running;
        private bool suspendRegroup;
        private bool suspendNaming;

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

            FillGroupingModes();
            ShowNaming();

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

        /// <summary>
        /// The four ways of gathering files, read off GroupingModes so the window and the
        /// grouping cannot drift apart. Per building is the default and is selected here.
        /// </summary>
        private void FillGroupingModes()
        {
            GroupingModeBox.Items.Clear();

            foreach (GroupingMode mode in GroupingModes.All())
            {
                GroupingModeBox.Items.Add(GroupingModes.Describe(mode));
            }

            GroupingModeBox.SelectedIndex = Array.IndexOf(GroupingModes.All(), GroupingModes.Default);
        }

        private GroupingMode ChosenGrouping()
        {
            GroupingMode[] all = GroupingModes.All();
            int at = GroupingModeBox == null ? -1 : GroupingModeBox.SelectedIndex;

            return at >= 0 && at < all.Length ? all[at] : GroupingModes.Default;
        }

        private void OnGroupingModeChanged(
            object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Regroup();
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

            ReadNaming();

            GroupingMode mode = ChosenGrouping();
            BuildingGroupingResult result = BuildingGrouping.Group(ticked, mode, settings);
            lastGroups = new List<BuildingGroup>(result.Groups);

            foreach (BuildingGroup group in result.Groups)
            {
                groups.Add(GroupRow.Usable(
                    group.Building,
                    PathsFor(group.Files, pathsByStem),
                    group.Disciplines,
                    SafeName(naming.Nwf, group),
                    SafeName(naming.Nwd, group),
                    SafeName(naming.Workbook, group)));
            }

            foreach (SkippedBuildingGroup skipped in result.Skipped)
            {
                groups.Add(GroupRow.Blocked(
                    skipped.Building,
                    PathsFor(skipped.Files, pathsByStem),
                    skipped.Reason));
            }

            int blocked = result.Skipped.Count;
            GroupingSummary.Text = GroupingModes.Describe(mode) + ". "
                + result.Groups.Count + (result.Groups.Count == 1 ? " group ready, " : " groups ready, ")
                + blocked + " blocked.";

            // Worked out here so it is on screen before Run is pressed, not after.
            findings = ScanFindings.From(result);
            ShowFindings();
            RefreshNamePreview();

            RefreshOutputsSummary();
        }

        /// <summary>
        /// The name for one group, or the reason it cannot be built. A half typed pattern
        /// must show what is wrong rather than throwing while somebody is still typing.
        /// </summary>
        private string SafeName(NamePattern pattern, BuildingGroup group)
        {
            try
            {
                return pattern.NameFor(group, settings);
            }
            catch (InvalidOperationException error)
            {
                return "CANNOT BE NAMED: " + error.Message;
            }
        }

        /// <summary>
        /// The findings panel, on the Source step so pressing Scan reports what was found
        /// and what is wrong with it in one place. Nothing here blocks a run or unticks
        /// anything. It is information and the decision stays with the person reading it.
        /// </summary>
        private void ShowFindings()
        {
            counts = new ScanCounts();
            counts.FilesFound = files.Count;
            counts.FilesReadable = ReadableFileCount();
            counts.GroupingDescription = GroupingModes.Describe(ChosenGrouping());

            int ready = 0;
            int blocked = 0;

            foreach (GroupRow group in groups)
            {
                if (group.IsBlocked)
                {
                    blocked++;
                }
                else
                {
                    ready++;
                }
            }

            counts.Groups = ready;
            counts.BlockedGroups = blocked;
            counts.Count(AllFindings());

            FindingsHeading.Text = counts.TotalFindings == 0
                ? "What the scan found"
                : "What the scan found, including " + counts.TotalFindings
                    + (counts.TotalFindings == 1 ? " thing" : " things")
                    + " worth a look. None of it stops a run.";

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

            if (sourceFindings == null)
            {
                lines.Add("The NWC files have not been compared against the Revit models they were "
                    + "published from yet. That can only be done once a run has opened them, so it "
                    + "appears here after the first run.");
            }

            FindingsBox.Text = string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>
        /// Everything found, the scan and, once a run has read the models, the Revit source
        /// findings too. One list, so the panel and the copied table cannot disagree.
        /// </summary>
        private IList<ScanFinding> AllFindings()
        {
            List<ScanFinding> all = new List<ScanFinding>(findings.All);

            if (sourceFindings != null)
            {
                all.AddRange(sourceFindings.All);
            }

            return all;
        }

        private int ReadableFileCount()
        {
            int readable = 0;

            foreach (FileRow row in files)
            {
                if (row.IsReadable)
                {
                    readable++;
                }
            }

            return readable;
        }

        /// <summary>
        /// The findings on the clipboard as tab separated rows with a header, so pasting
        /// into Excel gives a table rather than one blob of text.
        /// </summary>
        private void OnCopyFindings(object sender, RoutedEventArgs e)
        {
            string table = FindingsTable.Tsv(AllFindings());

            try
            {
                Clipboard.SetText(table);
                SetProgress("Findings copied. Paste into Excel and it lands as a table.");
            }
            catch (Exception error)
            {
                // The clipboard can be held by another process. Never worth stopping over.
                log.Failure(
                    "copying the findings to the clipboard",
                    error,
                    "nothing was changed, the findings are still in the panel");
                Warn("The clipboard would not take it." + Environment.NewLine + error.Message);
            }
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

        // ---------- Step 3, the naming patterns ----------

        /// <summary>
        /// Puts the current patterns into the boxes. Called once when the window opens, so
        /// the defaults are visible rather than hidden in the code.
        /// </summary>
        private void ShowNaming()
        {
            suspendNaming = true;

            try
            {
                Show(naming.Nwf, NwfLevel, NwfDiscipline, NwfType, NwfNumber, NwfAllBuildings);
                Show(naming.Nwd, NwdLevel, NwdDiscipline, NwdType, NwdNumber, NwdAllBuildings);
                Show(naming.Workbook, WorkbookLevel, WorkbookDiscipline, WorkbookType,
                    WorkbookNumber, WorkbookAllBuildings);
            }
            finally
            {
                suspendNaming = false;
            }
        }

        private static void Show(
            NamePattern pattern,
            System.Windows.Controls.TextBox level,
            System.Windows.Controls.TextBox discipline,
            System.Windows.Controls.TextBox type,
            System.Windows.Controls.TextBox number,
            System.Windows.Controls.TextBox allBuildings)
        {
            level.Text = pattern.Level;
            discipline.Text = pattern.Discipline;
            type.Text = pattern.TypeCode;
            number.Text = pattern.Number;
            allBuildings.Text = pattern.AllBuildings;
        }

        /// <summary>Reads the boxes back into the patterns, exactly as typed.</summary>
        private void ReadNaming()
        {
            Read(naming.Nwf, NwfLevel, NwfDiscipline, NwfType, NwfNumber, NwfAllBuildings);
            Read(naming.Nwd, NwdLevel, NwdDiscipline, NwdType, NwdNumber, NwdAllBuildings);
            Read(naming.Workbook, WorkbookLevel, WorkbookDiscipline, WorkbookType,
                WorkbookNumber, WorkbookAllBuildings);
        }

        private static void Read(
            NamePattern pattern,
            System.Windows.Controls.TextBox level,
            System.Windows.Controls.TextBox discipline,
            System.Windows.Controls.TextBox type,
            System.Windows.Controls.TextBox number,
            System.Windows.Controls.TextBox allBuildings)
        {
            pattern.Level = Trimmed(level.Text);
            pattern.Discipline = Trimmed(discipline.Text);
            pattern.TypeCode = Trimmed(type.Text);
            pattern.Number = Trimmed(number.Text);
            pattern.AllBuildings = Trimmed(allBuildings.Text);
        }

        private void OnNamingChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (suspendNaming)
            {
                return;
            }

            Regroup();
        }

        /// <summary>
        /// What the names come out as, shown before anything runs. A pattern that would put
        /// two groups on one name is named here as well as refused at Run, because seeing
        /// it while typing is better than being stopped afterwards.
        /// </summary>
        private void RefreshNamePreview()
        {
            if (NamePreview == null)
            {
                return;
            }

            List<string> lines = new List<string>();

            foreach (GroupRow group in groups)
            {
                if (!group.IsBlocked)
                {
                    lines.Add("NWF       " + group.NwfName);
                    lines.Add("NWD       " + group.NwdName);
                    lines.Add("Workbook  " + group.WorkbookName);
                    break;
                }
            }

            if (lines.Count == 0)
            {
                NamePreview.Text = "No group to name yet. Scan a folder first.";
                return;
            }

            string collisions = OutputNameCheck.WhyTheRunCannotStart(lastGroups, naming, settings);

            NamePreview.Text = "The first group would be written as:" + Environment.NewLine
                + string.Join(Environment.NewLine, lines.ToArray())
                + (collisions == null
                    ? string.Empty
                    : Environment.NewLine + Environment.NewLine + "THE RUN CANNOT START. " + collisions);
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

        private void OnBrowseExcel(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder("Pick the folder for the Excel reports", ExcelFolderBox.Text);

            if (picked != null)
            {
                ExcelFolderBox.Text = picked;
                RefreshOutputsSummary();
            }
        }

        private void OnOutputFolderChanged(
            object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RefreshOutputsSummary();
        }

        private void OnRepublishChanged(object sender, RoutedEventArgs e)
        {
            RefreshOutputsSummary();
        }

        /// <summary>
        /// What the run will do about reports. The folder is worked out here so the window
        /// can say where the workbooks are going before Run is pressed.
        /// </summary>
        private ReportOptions ReportsWanted()
        {
            ReportOptions options = new ReportOptions();
            options.ExcelFolder = Trimmed(ExcelFolderBox.Text);

            // Reports never go inside the folder being scanned. A real run put them
            // in C:\00_NM\NWC Fed\NWC\test001, which is where its own input lives.
            options.SourceFolder = Trimmed(SourceFolderBox.Text);
            options.WriteXml = WriteClashXml.IsChecked == true;
            options.Names = settings;
            return options;
        }

        /// <summary>
        /// Where the workbooks land, or a plain reason why that cannot be worked out yet.
        /// Never throws, because it runs on every keystroke in a folder box.
        /// </summary>
        private string WorkbookFolderOrWhyNot()
        {
            try
            {
                ReportFolderChoice where = ReportsWanted().ChooseFor(Trimmed(NwfFolderBox.Text));

                return where.WasRefused
                    ? where.Folder + ". " + where.RefusedReason
                    : where.Folder;
            }
            catch (ArgumentException error)
            {
                return "UNKNOWN. " + error.Message;
            }
        }

        private void RefreshOutputsSummary()
        {
            // IsChecked="True" in the XAML raises Checked while the tree is still being
            // built, so this can be reached before the controls it reads exist.
            if (OutputsSummary == null || RepublishNwd == null || ExcelFolderBox == null
                || NwfFolderBox == null || WriteClashXml == null || SourceFolderBox == null)
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

            string xml = WriteClashXml != null && WriteClashXml.IsChecked == true
                ? " A clash XML is written beside each workbook."
                : string.Empty;

            OutputsSummary.Text = ready + " groups ticked to run. " + nwd
                + " Workbooks go in " + WorkbookFolderOrWhyNot() + "." + xml
                + " The log is written to "
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

            // Outputs overwrite with no date suffix, so two groups sharing a name is not a
            // warning. The second silently destroys the first and only shows up later as a
            // federation nobody can find. Caught before anything is cleared or written.
            string collisions = OutputNameCheck.WhyTheRunCannotStart(
                TickedGroups(), naming, settings);

            if (collisions != null)
            {
                log.Line("RUN      refused before starting. " + collisions);
                Warn("The run cannot start." + Environment.NewLine + Environment.NewLine + collisions);
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

                // Built through OutputPaths so the path a rerun looks for the NWF at is
                // the same string it was written to.
                jobs.Add(new FederationJob(
                    group.Building,
                    group.NwfName,
                    OutputPaths.Nwf(nwfFolder, group.NwfName),
                    OutputPaths.Nwd(nwdFolder, group.NwdName),
                    group.Files,
                    group.WorkbookName));
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

            log.Line("grouping         : " + GroupingModes.Describe(ChosenGrouping()));
            log.Line("republish NWD    : " + (RepublishNwd.IsChecked == true ? "yes" : "no"));
            log.Block(RunLog.GroupsSectionTitle, GroupListLines());
            log.Block(RunLog.FindingsSectionTitle, findings.Lines());

            RunJobs(jobs, nwfFolder);
        }

        /// <summary>
        /// The groups that will actually run. A name shared with a group nobody ticked is
        /// not a collision, because only one of them is going to be written.
        /// </summary>
        private IList<BuildingGroup> TickedGroups()
        {
            List<BuildingGroup> ticked = new List<BuildingGroup>();

            foreach (BuildingGroup group in lastGroups)
            {
                foreach (GroupRow row in groups)
                {
                    if (string.Equals(row.Building, group.Building, StringComparison.Ordinal)
                        && row.Include && !row.IsBlocked && row.Files.Count > 0)
                    {
                        ticked.Add(group);
                        break;
                    }
                }
            }

            return ticked;
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

                // Read once, before the first group, so a file that will not read stops
                // the run here rather than part way through the second building.
                ExchangeDocument exchange = PickedExchange();

                if (exchange == null)
                {
                    log.Line("RUN      nothing picked in the Clash step, so no set will be built "
                        + "and no test created");
                }
                else
                {
                    log.Line("RUN      the Clash step picked " + exchange.SourcePath);
                    log.Line("RUN      it holds " + exchange.Sets.Count
                        + (exchange.Sets.Count == 1 ? " set and " : " sets and ")
                        + exchange.Tests.Count + (exchange.Tests.Count == 1 ? " test" : " tests"));
                }

                ReportOptions options = ReportsWanted();
                ReportFolderChoice where = options.ChooseFor(nwfFolder);

                if (where.WasRefused)
                {
                    log.Line("RUN      " + where.RefusedReason);
                }

                log.Line("RUN      workbooks go in " + where.Folder);
                log.Line("RUN      the clash XML is "
                    + (options.WriteXml ? "written beside each workbook" : "off"));

                FederationEngine engine = new FederationEngine(
                    SetProgress, log, RepublishNwd.IsChecked == true, exchange, options, nwfFolder);
                engine.Run(jobs);

                // What the Revit container inside each NWC says its building is. Only
                // knowable once a document has been open, so it goes in after the run.
                // Information, exactly like the scan findings. Nothing acts on it.
                sourceFindings = SourceMismatchFindings.From(engine.SourcePairs, settings);
                log.Block(RunLog.SourceFindingsSectionTitle, sourceFindings.Lines());
                ShowFindings();

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

        /// <summary>
        /// One picker, one file. Nothing about any one file is in here, the file is
        /// picked every run and can be from any project.
        ///
        /// The file can hold sets, tests, or both, and the tool reads what is in it.
        /// There used to be a second box for a sets file, which meant picking the same
        /// combined file twice. Reading it once here is also what lets the message say
        /// which of the three shapes it turned out to be.
        /// </summary>
        private void OnBrowseExchangeFile(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "Pick the clash XML. It can hold sets, tests, or both.";
                dialog.Filter = "Navisworks exchange XML (*.xml)|*.xml|All files (*.*)|*.*";
                dialog.CheckFileExists = true;

                string current = Trimmed(ExchangeFileBox.Text);

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

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                ExchangeFileBox.Text = dialog.FileName;
                SetsSummary.Text = Describe(dialog.FileName);
            }
        }

        /// <summary>
        /// Says what the picked file actually holds, counted out of the file itself, so a
        /// file with no tests in it is obvious before a run rather than after one.
        /// </summary>
        private string Describe(string path)
        {
            ExchangeDocument exchange;

            try
            {
                exchange = new ExchangeReader().ReadFile(path);
            }
            catch (Exception error)
            {
                log.Failure("reading " + path, error, "the file stays picked, nothing was read from it");
                return "Picked " + path + ", but it would not read. " + error.Message;
            }

            string held = exchange.Sets.Count + (exchange.Sets.Count == 1 ? " set and " : " sets and ")
                + exchange.Tests.Count + (exchange.Tests.Count == 1 ? " test." : " tests.");

            log.Line("PICK     " + path + " holds " + held);
            return "Picked " + path + ". It holds " + held;
        }

        private static string Trimmed(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>
        /// The one file picked in the Clash step, read once. Null when nothing was
        /// picked, and then the run builds no set and creates no test, which is a step
        /// switched off rather than a failure.
        /// </summary>
        private ExchangeDocument PickedExchange()
        {
            string path = Trimmed(ExchangeFileBox.Text);

            return path.Length == 0 || !File.Exists(path)
                ? null
                : new ExchangeReader().ReadFile(path);
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

            string path = Trimmed(ExchangeFileBox.Text);

            if (path.Length == 0 || !File.Exists(path))
            {
                Warn("Pick a clash XML that exists first.");
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

        /// <summary>
        /// Creates the tests the picked file holds and runs them against whatever document
        /// is open right now, without federating anything. This is the one off. The Run
        /// button does the same work per group, in the right order, and saves the NWF
        /// after it. Runs on the plugin thread, like everything else that touches the API.
        /// </summary>
        private void OnRunTests(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            string path = Trimmed(ExchangeFileBox.Text);

            if (path.Length == 0 || !File.Exists(path))
            {
                Warn("Pick a clash XML that exists first.");
                return;
            }

            running = true;
            RunTestsButton.IsEnabled = false;

            try
            {
                log.Line("CLASH    started, reading " + path);
                ExchangeDocument exchange = new ExchangeReader().ReadFile(path);

                if (!exchange.HasTests)
                {
                    string nothing = "This file holds no clash test. Nothing to create.";
                    log.Line("CLASH    " + nothing);
                    SetsSummary.Text = nothing;
                    ShowSetLines(new List<string> { nothing });
                    return;
                }

                string units = ClashRunner.DocumentUnits();
                ClashTestPlan plan = ClashTestPlan.From(exchange, units);

                foreach (string unknown in plan.UnknownTestTypes)
                {
                    log.Line("CLASH    test type \"" + unknown
                        + "\" is not one this tool creates, every test using it is skipped by name");
                }

                log.Line("CLASH    " + plan.TestsInFile + " in the file, " + plan.Buildable.Count
                    + " to create, " + plan.Skipped.Count + " skipped before the model");

                ClashRunOutcome outcome = new ClashRunner(SetProgress, log).Run(plan);

                log.Block(ClashSectionTitle, outcome.Lines());
                ShowSetLines(outcome.Lines());

                SetsSummary.Text = outcome.Summary();
                SetProgress("Clash tests finished. " + outcome.Summary());
                log.Line("CLASH    finished. " + outcome.Summary());
            }
            catch (Exception error)
            {
                log.Failure(
                    "creating and running the clash tests from " + path,
                    error,
                    "stopped, whatever was already created and run is still in the document");
                SetProgress("The clash tests stopped on an error.");
                Warn("The clash tests stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                running = false;
                RunTestsButton.IsEnabled = true;
            }
        }

        private const string ClashSectionTitle = "CLASH";

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
