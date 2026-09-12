using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Findings;
using Federator.Core.Grouping;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Naming;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Sets;
using Federator.Core.Units;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// Four tabs: source, grouping, outputs, clash. The window does the deciding. The
    /// engine does the work, on this same thread.
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

        /// <summary>Where each picker was last pointed, kept across sessions.</summary>
        private readonly FolderMemory folders = FolderMemory.Load();

        /// <summary>One row per group, filled from the patterns and editable in place.</summary>
        private OutputNameTable nameTable = new OutputNameTable();
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

            // The boxes are filled from the patterns BEFORE anything can read them back.
            //
            // FillGroupingModes sets SelectedIndex, which fires OnGroupingModeChanged,
            // which calls Regroup, which calls ReadNaming. With the boxes still empty that
            // read every default out of the patterns and replaced it with nothing, and
            // ShowNaming then put the emptied patterns back into the boxes. The window
            // opened with all fifteen naming boxes blank, so every field had to be typed
            // by hand, which is where MOD-00001 came from instead of MOD-000001.
            ShowNaming();
            ShowTheInstallsLogo();
            FillUnits();
            ShowOpenDocument();
            FillGroupingModes();

            log.Block("FOLDERS REMEMBERED", folders.Lines());

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
            string picked = PickFolder(
                "Pick the folder holding the NWC files", StartFor(PickerKind.Source, SourceFolderBox.Text));

            if (picked != null)
            {
                folders.Remember(PickerKind.Source, picked);
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

            // The scan feeds the table. Regrouping is a different set of groups, so the
            // table is built again and any hand edit belonged to groups that no longer
            // exist. A pattern change goes through Refill instead, which keeps them.
            nameTable = OutputNameTable.From(result.Groups, naming, settings);

            foreach (BuildingGroup group in result.Groups)
            {
                groups.Add(GroupRow.Usable(
                    group.Building,
                    PathsFor(group.Files, pathsByStem),
                    group.Disciplines,
                    nameTable.Find(group.Building)));
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
            string why = pattern.WhyUnusable();

            if (why != null)
            {
                // Our own sentence, asked for directly, so nothing a framework wrote can
                // reach a label. No name yet is not a shouting matter either.
                return "No name yet. " + why;
            }

            try
            {
                return pattern.NameFor(group, settings);
            }
            catch (InvalidOperationException)
            {
                return "No name yet. One of the fields on this step is empty.";
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

            naming.DateTheNwd = DateTheNwd != null && DateTheNwd.IsChecked == true;
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
            RefillNames();
        }

        private void OnNamingToggled(object sender, RoutedEventArgs e)
        {
            RefillNames();
        }

        /// <summary>
        /// A pattern changed, so every name that has not been typed over is rebuilt and
        /// every one that has is left exactly as it is. It says how many it kept, because
        /// otherwise nobody can tell whether their edit survived.
        /// </summary>
        private void RefillNames()
        {
            if (suspendNaming || nameTable == null || NamePreview == null)
            {
                return;
            }

            ReadNaming();
            int kept = nameTable.Refill(naming, settings);

            foreach (GroupRow row in groups)
            {
                row.NamesRefilled();
            }

            lastRefill = OutputNameTable.DescribeRefill(nameTable.Count, kept);
            RefreshNamePreview();
            RefreshOutputsSummary();
        }

        private string lastRefill = string.Empty;

        /// <summary>
        /// A name typed straight into the table. Only that one name changes, and only that
        /// row is left alone by the next pattern change.
        /// </summary>
        private void OnNameEdited(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            // The binding writes the value on commit, so nothing is read here. This runs
            // afterwards to refresh what the edit affects.
            Dispatcher.BeginInvoke(new Action(delegate
            {
                lastRefill = string.Empty;
                RefreshNamePreview();
                RefreshOutputsSummary();
            }), System.Windows.Threading.DispatcherPriority.Background);
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

            string collisions = nameTable.WhyTheRunCannotStart();

            NamePreview.Text = "The first group would be written as:" + Environment.NewLine
                + string.Join(Environment.NewLine, lines.ToArray())
                + (lastRefill.Length == 0
                    ? string.Empty
                    : Environment.NewLine + lastRefill)
                + (collisions == null
                    ? string.Empty
                    : Environment.NewLine + Environment.NewLine + "THE RUN CANNOT START. " + collisions);

            // A name change moves the NWF path, and the path is what decides First run
            // against Weekly run.
            RefreshRunPaths();
        }

        /// <summary>
        /// Which of the two workflows each group is expected to take, shown in the group
        /// list before Run. Read off whether the NWF is already at its output path and
        /// whether an XML is picked, through Federator.Core.Rerun.RunPath. Nothing here
        /// opens a file, so Rebuilt cannot be known yet and never shows here. It shows
        /// once PreviewRunPaths has opened the NWFs at Run, and after the run.
        /// </summary>
        private void RefreshRunPaths()
        {
            if (NwfFolderBox == null)
            {
                return;
            }

            string nwfFolder = Trimmed(NwfFolderBox.Text);
            bool xmlPicked = XmlIsPicked();

            foreach (GroupRow group in groups)
            {
                if (group.IsBlocked)
                {
                    continue;
                }

                if (nwfFolder.Length == 0 || group.NwfName.Length == 0)
                {
                    group.RunAs = RunPath.Unknown;
                    continue;
                }

                bool nwfOnDisk;

                try
                {
                    nwfOnDisk = File.Exists(OutputPaths.Nwf(nwfFolder, group.NwfName));
                }
                catch (Exception)
                {
                    group.RunAs = RunPath.Unknown;
                    continue;
                }

                group.RunAs = RunPath.Expected(nwfOnDisk, xmlPicked);
            }
        }

        /// <summary>The same test the run uses: a path in the box that is really on disk.</summary>
        private bool XmlIsPicked()
        {
            if (ExchangeFileBox == null)
            {
                return false;
            }

            string path = Trimmed(ExchangeFileBox.Text);
            return path.Length > 0 && File.Exists(path);
        }

        private void OnExchangeFileChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RefreshRunPaths();
            ShowOpenDocument();
        }

        /// <summary>The expected label of every group that will run, in list order.</summary>
        private IList<string> TickedRunPaths()
        {
            List<string> labels = new List<string>();

            foreach (GroupRow group in groups)
            {
                if (group.Include && !group.IsBlocked && group.Files.Count > 0)
                {
                    labels.Add(group.RunAs);
                }
            }

            return labels;
        }

        /// <summary>
        /// Opens each NWF that is on disk and compares it with the scan, so the confirm
        /// dialog counts the Rebuilt groups and the list shows them before Run. F24.
        ///
        /// Only when nothing open would be lost. Opening an NWF replaces the open document
        /// and the person has not yet said yes, and "Run cancelled before anything was
        /// cleared" has to stay true. Where something is open the dialog says Rebuilt is
        /// only known once each NWF is opened, and the run settles the label.
        /// </summary>
        private void PreviewRunPaths(IList<FederationJob> jobs)
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

            if (discarded != null)
            {
                log.Line("PREVIEW  not done, opening each NWF would discard what is open before the confirm dialog: "
                    + discarded);
                return;
            }

            try
            {
                IList<string> labels = FederationEngine.PreviewRunPaths(jobs, XmlIsPicked(), log, SetProgress);

                for (int i = 0; i < jobs.Count && i < labels.Count; i++)
                {
                    GroupRow group = GroupFor(jobs[i]);

                    if (group != null)
                    {
                        group.RunAs = labels[i];
                    }
                }
            }
            catch (Exception error)
            {
                log.Failure("checking the NWFs before the run", error, "the labels stay as expected and the run decides");
            }
        }

        /// <summary>The label each group ended with, read off what the engine did.</summary>
        private void ShowRunPathsAfterTheRun(IList<JobOutcome> outcomes, bool xmlPicked)
        {
            if (outcomes == null)
            {
                return;
            }

            foreach (JobOutcome outcome in outcomes)
            {
                GroupRow group = GroupFor(outcome.Job);

                if (group != null)
                {
                    group.RunAs = RunPath.Label(outcome.Decision, xmlPicked);
                }
            }
        }

        /// <summary>The row a job was built from, matched on the NWF name, which the collision check keeps unique.</summary>
        private GroupRow GroupFor(FederationJob job)
        {
            if (job == null)
            {
                return null;
            }

            foreach (GroupRow group in groups)
            {
                if (!group.IsBlocked && string.Equals(group.NwfName, job.OutputName, StringComparison.OrdinalIgnoreCase))
                {
                    return group;
                }
            }

            return null;
        }

        // ---------- Step 3, outputs ----------

        private void OnBrowseNwf(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder(
                "Pick the folder for the NWF files", StartFor(PickerKind.Nwf, NwfFolderBox.Text));

            if (picked != null)
            {
                folders.Remember(PickerKind.Nwf, picked);
                NwfFolderBox.Text = picked;
                RefreshOutputsSummary();
            }
        }

        private void OnBrowseNwd(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder(
                "Pick the folder for the NWD files", StartFor(PickerKind.Nwd, NwdFolderBox.Text));

            if (picked != null)
            {
                folders.Remember(PickerKind.Nwd, picked);
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

        /// <summary>
        /// Fills the logo box with the one Navisworks puts on its own reports, because
        /// that is the report the client accepts and there should be nothing to set up
        /// before a first run.
        ///
        /// Read off the install every time the window opens rather than remembered, so a
        /// machine whose Navisworks moved still finds it. Clearing the box means no logo.
        /// </summary>
        private void ShowTheInstallsLogo()
        {
            if (LogoBox == null || Trimmed(LogoBox.Text).Length > 0)
            {
                return;
            }

            string install = NavisworksFacts.InstallFolder();
            string language = NavisworksFacts.Language();
            string found = InstallFiles.FindLogo(install, language);

            if (found.Length > 0)
            {
                LogoBox.Text = found;
                log.Line("LOGO     the report will carry " + found);
                return;
            }

            foreach (string line in InstallFiles.WhyNoLogo(install, language))
            {
                log.Line(line);
            }
        }

        /// <summary>
        /// The units every number in the report is in. Metres by default, because this is
        /// a Saudi project and every report the team sends is metric. Since F26 this sets
        /// what the MODELS are put into, which is what a person sees in Navisworks. The
        /// report itself is always metres, converted in Core before anything is written.
        /// </summary>
        private static readonly IList<UnitRow> UnitChoices = UnitTable.Offered();

        private void FillUnits()
        {
            UnitsBox.Items.Clear();

            foreach (UnitRow row in UnitChoices)
            {
                UnitsBox.Items.Add(UnitWording(row));
            }

            UnitsBox.SelectedIndex = 0;
        }

        /// <summary>The table's display name, and on the default a word on what it is for.</summary>
        private static string UnitWording(UnitRow row)
        {
            return row.EnumName == ReportOptions.DefaultUnits
                ? row.DisplayName + ", which is what the models are set to"
                : row.DisplayName;
        }

        private string ChosenUnits()
        {
            int at = UnitsBox == null ? 0 : UnitsBox.SelectedIndex;

            return at >= 0 && at < UnitChoices.Count
                ? UnitChoices[at].EnumName
                : ReportOptions.DefaultUnits;
        }

        private void OnUnitsChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshOutputsSummary();
        }

        private void OnBrowseLogo(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "Pick the logo for the client report page";
                dialog.Filter = "Pictures (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif"
                    + "|All files (*.*)|*.*";
                dialog.CheckFileExists = true;

                string folder = PickerStart.For(folders, PickerKind.Logo, LogoBox.Text);

                if (folder.Length > 0 && Directory.Exists(folder))
                {
                    dialog.InitialDirectory = folder;
                }

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                folders.Remember(PickerKind.Logo, dialog.FileName);
                LogoBox.Text = dialog.FileName;
                RefreshOutputsSummary();
            }
        }

        private void OnBrowseExcel(object sender, RoutedEventArgs e)
        {
            string picked = PickFolder(
                "Pick the folder for the Excel reports", StartFor(PickerKind.Excel, ExcelFolderBox.Text));

            if (picked != null)
            {
                folders.Remember(PickerKind.Excel, picked);
                ExcelFolderBox.Text = picked;
                RefreshOutputsSummary();
            }
        }

        private void OnOutputFolderChanged(
            object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RefreshOutputsSummary();
            RefreshRunPaths();
        }

        private void OnXmlChanged(object sender, RoutedEventArgs e)
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

            // The page, the units and the photos are fixed on in the options themselves,
            // so nothing here sets them. A weekly run wants all three every time.
            options.ApplyFileSettings = ApplyFileSettings.IsChecked == true;
            options.CompactResolved = CompactResolved.IsChecked == true;
            options.LogoPath = Trimmed(LogoBox.Text);
            options.UnitsName = ChosenUnits();
            options.Images = ImagesWanted();
            options.Names = settings;
            return options;
        }

        /// <summary>
        /// What the run will do about pictures. A box nobody can read falls back to the
        /// default rather than stopping the run, and the summary line shows what was
        /// actually understood, so a typo is visible before Run is pressed.
        /// </summary>
        private ImageOptions ImagesWanted()
        {
            ImageOptions images = new ImageOptions();
            images.EmbedThumbnail = EmbedThumbnails.IsChecked == true;

            int pixels = Number(ImagePixelsBox.Text, ImageOptions.DefaultPixels);
            images.Width = pixels;
            images.Height = pixels;
            images.CapPerTest = Number(ImageCapBox.Text, 0);

            List<ClashStatus> wanted = new List<ClashStatus>();

            if (ImageNew.IsChecked == true) { wanted.Add(ClashStatus.New); }
            if (ImageActive.IsChecked == true) { wanted.Add(ClashStatus.Active); }
            if (ImageReviewed.IsChecked == true) { wanted.Add(ClashStatus.Reviewed); }
            if (ImageApproved.IsChecked == true) { wanted.Add(ClashStatus.Approved); }
            if (ImageResolved.IsChecked == true) { wanted.Add(ClashStatus.Resolved); }

            if (wanted.Count == 0)
            {
                // Every status unticked means no picture would ever be written. That is
                // the images box unticked, said a longer way, so it is treated as that
                // rather than refused.
                images.Write = false;
            }
            else
            {
                images.OnlyFor(wanted);
            }

            return images;
        }

        /// <summary>
        /// A whole number out of a box, or the fallback. Never throws and never stops a
        /// run, because this is read on every keystroke.
        /// </summary>
        private static int Number(string text, int fallback)
        {
            int value;

            if (!int.TryParse(Trimmed(text), System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return fallback;
            }

            return value < 0 ? fallback : value;
        }

        private void OnImageSettingChanged(object sender, RoutedEventArgs e)
        {
            RefreshImageSummary();
        }

        private void OnImageSettingChanged(object sender, TextChangedEventArgs e)
        {
            RefreshImageSummary();
        }

        /// <summary>
        /// Says what the settings will actually do, in the same words the log will use, so
        /// a cap or a size that did not read the way it was typed is visible here first.
        /// </summary>
        private void RefreshImageSummary()
        {
            if (ImageSummary == null)
            {
                return;
            }

            try
            {
                ImageSummary.Text = ImagesWanted().Describe()
                    + " The workbook is the client's layout.";
            }
            catch (Exception)
            {
                // A label, not a failure dialog, so it carries no framework message.
                ImageSummary.Text = "The photo settings on this step cannot be read.";
            }
        }

        /// <summary>
        /// Where the workbooks land, or a plain reason why that cannot be worked out yet.
        /// Never throws, because it runs on every keystroke in a folder box.
        /// </summary>
        private string WorkbookFolderOrWhyNot()
        {
            // One rule, in Core, so it can be proved rather than clicked.
            return ReportPaths.WhereTheyGo(
                Trimmed(ExcelFolderBox.Text), Trimmed(NwfFolderBox.Text), Trimmed(SourceFolderBox.Text));
        }

        private void RefreshOutputsSummary()
        {
            // IsChecked="True" in the XAML raises Checked while the tree is still being
            // built, so this can be reached before the controls it reads exist.
            if (OutputsSummary == null || ExcelFolderBox == null
                || NwfFolderBox == null || WriteClashXml == null || SourceFolderBox == null
                || DateTheNwd == null)
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

            string nwd = "The NWD is republished every run.";

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
            string collisions = nameTable.Only(TickedGroupKeys()).WhyTheRunCannotStart();

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
                    group.WorkbookName,
                    group.DisciplineCount));
            }

            if (jobs.Count == 0)
            {
                Warn("No group is ticked to run.");
                return;
            }

            // The NWF folder may have changed since the list was last refreshed, so the
            // labels are read again right before they are counted.
            RefreshRunPaths();

            // Then each NWF on disk is opened and compared, where that loses nothing, so
            // the dialog can count the Rebuilt groups and the list can show them.
            PreviewRunPaths(jobs);

            if (!ConfirmClear(TickedRunPaths()))
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
            log.Line("apply file to old: "
                + (ApplyFileSettings.IsChecked == true
                    ? "YES, which RESETS the results of every test it changes"
                    : "no, differences are reported and nothing is changed"));
            log.Line("compact resolved : "
                + (CompactResolved.IsChecked == true
                    ? "YES, which permanently removes every Resolved clash"
                    : "no"));
            log.Line("NWD naming       : "
                + (DateTheNwd.IsChecked == true
                    ? "dated, so every week is kept"
                    : "overwrites, so only the latest week exists"));
            log.Line("republish NWD    : yes, fixed");
            log.Line("model units      : " + ChosenUnits() + ", which is what the models are set to");
            log.Line("report units     : " + Federator.Core.Report.ReportUnits.Name
                + ", always, converted before anything is written");
            log.Block(RunLog.GroupsSectionTitle, GroupListLines());
            log.Block(RunLog.FindingsSectionTitle, findings.Lines());

            RunJobs(jobs, nwfFolder);
        }

        /// <summary>
        /// The groups that will actually run. A name shared with a group nobody ticked is
        /// not a collision, because only one of them is going to be written.
        /// </summary>
        private IList<string> TickedGroupKeys()
        {
            List<string> ticked = new List<string>();

            foreach (GroupRow row in groups)
            {
                if (row.Include && !row.IsBlocked && row.Files.Count > 0)
                {
                    ticked.Add(row.Building);
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
            int unticked = 0;

            foreach (GroupRow group in groups)
            {
                // Unticked, never skipped. A group is only left out of a run by the Run
                // column or by being blocked, and skipped once read as a rule that does
                // not exist.
                string state = group.IsBlocked ? "BLOCKED " : (group.Include ? "run     " : "unticked");

                if (!group.IsBlocked && !group.Include)
                {
                    unticked++;
                }

                lines.Add(state + "  " + group.Building.PadRight(10)
                    + group.FileCount.ToString().PadLeft(3)
                    + (group.FileCount == 1 ? " file   " : " files  ")
                    + (group.OutputName.Length == 0 ? "no output name" : group.OutputName)
                    + "  [" + group.Disciplines + "]"
                    + (group.IsBlocked ? string.Empty : "  " + group.RunAs));

                if (group.IsBlocked)
                {
                    lines.Add("          " + group.BlockedReason);
                }
            }

            if (lines.Count == 0)
            {
                lines.Add("No groups.");
            }
            else
            {
                lines.Add(RunLog.UntickedGroupsLine(unticked));
            }

            return lines;
        }

        /// <summary>
        /// Says which of the two workflows the run will take, per group count, before
        /// anything happens. Clearing is only said where it is true, which is the First
        /// run groups. Whatever is open is replaced either way, so it is named and the
        /// user can cancel. Asked once, before the first group.
        /// </summary>
        private bool ConfirmClear(IList<string> runPaths)
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

            string message = string.Join(
                Environment.NewLine, new List<string>(RunPath.ConfirmLines(runPaths)).ToArray());

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
                    SetProgress, log, true, exchange, options, nwfFolder);
                IList<JobOutcome> outcomes = engine.Run(jobs);

                // What each group actually did, in the list. A group that was rebuilt
                // reads Rebuilt here once the run has settled it.
                ShowRunPathsAfterTheRun(outcomes, exchange != null);

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

        // ---------- Step 4, clash ----------

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

                // StartFor already gives a FOLDER, for this picker as much as the other
                // four. Taking the directory name of it opened the parent, so a remembered
                // folder came back one level too high every time.
                string folder = StartFor(PickerKind.ClashXml, ExchangeFileBox.Text);

                if (folder.Length > 0 && Directory.Exists(folder))
                {
                    dialog.InitialDirectory = folder;
                }

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                folders.Remember(PickerKind.ClashXml, dialog.FileName);
                ExchangeFileBox.Text = dialog.FileName;
                SetsSummary.Text = Describe(dialog.FileName);
            }
        }

        /// <summary>
        /// Says what the picked file actually holds, counted out of the file itself, so a
        /// file with no tests in it is obvious before a run rather than after one, and
        /// what HealthCheck makes of it, so a damaged export is obvious then too. D1. The
        /// check's whole summary goes in the log and one line of it under the file.
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

            HealthCheckResult health = HealthCheck.Run(exchange);
            log.Block("HEALTH " + Path.GetFileName(path), health.Summary());

            return "Picked " + path + ". It holds " + held + " " + health.Line(exchange.HasSets);
        }

        private static string Trimmed(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>
        /// Where one picker should open. What is already in its box wins, because that is
        /// what the person is looking at, then what that picker was last pointed at, and
        /// a folder that has gone falls back to its nearest existing parent rather than
        /// failing to open.
        /// </summary>
        private string StartFor(PickerKind kind, string inTheBox)
        {
            return PickerStart.For(folders, kind, inTheBox);
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
        /// The whole job on the file already open. No scan, no source folder, no grouping.
        ///
        /// HOW A PERSON TELLS THE TWO APART. The blue box on this step says what it will
        /// run and where it will write, naming the open file. The Run button on the Source
        /// step is the other path, and it says how many groups are ticked. One names a
        /// file, the other names a count, so neither reads as the other.
        /// </summary>
        private void OnRunOpenDocument(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            string open = OpenDocumentPath();

            if (!OpenDocumentJob.CanRun(open))
            {
                Warn(OpenDocumentJob.WhyNot(open));
                return;
            }

            // The clash file is OPTIONAL here. Without one, the tests saved in the document
            // are run where they sit, which is the ordinary weekly case, and a document
            // holding none means nothing runs and the log says so.
            string path = Trimmed(ExchangeFileBox.Text);
            ExchangeDocument exchange = null;

            running = true;
            RunOpenButton.IsEnabled = false;

            try
            {
                if (path.Length > 0 && File.Exists(path))
                {
                    exchange = new ExchangeReader().ReadFile(path);
                    log.Line("OPEN     clash file " + path);
                }
                else
                {
                    log.Line("OPEN     no XML picked, so the tests saved in the document run, "
                        + "or nothing runs when it holds none");
                }

                ReportOptions options = ReportsWanted();

                // No folder is handed in. The engine reads the report folder off the open
                // file through OpenDocumentJob.ReportFolder, the same rule ShowOpenDocument
                // uses for the line above the button. Handing the report folder in as the
                // NWF folder is what once wrote to Clash Reports\Clash Reports.
                FederationEngine engine = new FederationEngine(
                    SetProgress, log, true, exchange, options);

                // The engine writes the GROUP lines and the OPEN FILE block itself, so
                // they are there whatever happens inside it.
                JobOutcome outcome = engine.RunOpenDocument();

                SetsSummary.Text = FederationEngine.Describe(outcome);
                SetProgress(SetsSummary.Text);
            }
            catch (Exception error)
            {
                log.Failure(
                    "running the open document",
                    error,
                    "stopped, everything already written is kept");
                SetProgress("The run on the open document stopped on an error.");
                Warn(error.Message);
            }
            finally
            {
                // The source findings compare the Revit source inside each NWC against the
                // scanned NWC name, and nothing was scanned here, so there is nothing to
                // compare. Said in the log rather than left as a missing block.
                log.Line("SOURCE   findings skipped, the open file run has no scanned source folder to compare against");

                // The RESULT block and the second copy of the log are written whatever
                // happened, the same as the scanned run. The copy goes beside the open
                // file, where the scanned run puts it beside the NWF folder. This used to
                // be missing, so an open file run ended with no RESULT block and no copy.
                WriteTheResultAndCopyTheLog(OpenDocumentJob.FolderOf(open));
                running = false;
                RunOpenButton.IsEnabled = true;
                ShowOpenDocument();
            }
        }

        /// <summary>
        /// A step was shown. The blue box names the file open RIGHT NOW, and a person can
        /// open a different one between one look and the next, so it is read again here
        /// rather than once at startup.
        /// </summary>
        private void OnStepShown(object sender, SelectionChangedEventArgs e)
        {
            // TabControl bubbles SelectionChanged from every combo box inside it.
            if (!ReferenceEquals(e.OriginalSource, Steps))
            {
                return;
            }

            ShowOpenDocument();
        }

        /// <summary>
        /// What the blue box says. Read every time the step is shown, because the person
        /// can open a different file between one look and the next.
        /// </summary>
        private void ShowOpenDocument()
        {
            if (OpenDocumentLine == null)
            {
                return;
            }

            string open = OpenDocumentPath();

            OpenDocumentLine.Text = OpenDocumentJob.Describe(
                open,
                ExcelFolderBox == null ? string.Empty : Trimmed(ExcelFolderBox.Text),
                XmlIsPicked());

            if (RunOpenButton != null)
            {
                RunOpenButton.IsEnabled = !running && OpenDocumentJob.CanRun(open);
            }
        }

        /// <summary>
        /// Where the open document was loaded from, or empty. Never throws, because the
        /// window reads this to fill a label and a label must not carry a framework
        /// message.
        /// </summary>
        private static string OpenDocumentPath()
        {
            try
            {
                Autodesk.Navisworks.Api.Document document =
                    Autodesk.Navisworks.Api.Application.ActiveDocument;

                return document == null || document.FileName == null
                    ? string.Empty
                    : document.FileName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The Build sets button. The picked file's sets into whatever document is open,
        /// nothing else, through the same engine method the run uses per group, so the
        /// SETS lines in the log read the same either way. D4. Runs on the plugin thread,
        /// like everything else that touches the API.
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
                log.Line("SETS     started by hand, reading " + path);
                ExchangeDocument exchange = new ExchangeReader().ReadFile(path);

                FederationEngine engine = new FederationEngine(SetProgress, log, exchange, ReportsWanted());
                SetBuildOutcome outcome = engine.BuildSetsByHand();

                ShowSetLines(outcome.Lines());
                SetsSummary.Text = outcome.Summary();
                SetProgress("Sets finished. " + outcome.Summary());
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
        /// The Run tests button. The picked file's tests created into and run against
        /// whatever document is open, without federating anything and with no NWF saved,
        /// through the same engine method the run uses per group, so the CLASH lines in
        /// the log read the same either way. D4. Runs on the plugin thread, like
        /// everything else that touches the API.
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
                log.Line("CLASH    started by hand, reading " + path);
                ExchangeDocument exchange = new ExchangeReader().ReadFile(path);

                FederationEngine engine = new FederationEngine(SetProgress, log, exchange, ReportsWanted());
                ClashRunOutcome outcome = engine.RunTestsByHand();

                if (outcome == null)
                {
                    // The engine said why in the log: nothing open, or a file holding no
                    // clash test, or a throw it caught.
                    string nothing = "Nothing was created or run. The log says why.";
                    SetsSummary.Text = nothing;
                    ShowSetLines(new List<string> { nothing });
                    return;
                }

                ShowSetLines(outcome.Lines());
                SetsSummary.Text = outcome.Summary();
                SetProgress("Clash tests finished. " + outcome.Summary());
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
