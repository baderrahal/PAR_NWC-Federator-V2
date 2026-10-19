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
using Federator.Core.Generic;
using Federator.Core.Grouping;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Naming;
using Federator.Core.Communities;
using Federator.Core.Probe;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Sets;
using Federator.Core.Teams;
using Federator.Core.Units;
using Federator.Core.Views;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// Two pages. The NWC page has four steps, packages, grouping, outputs and clash, and the NWF page its packages
    /// and its steps, F232. The window does the deciding. The engine does the work, on this same thread.
    /// </summary>
    public partial class FederatorWindow : Window
    {
        private readonly ContainerNameSettings settings = new ContainerNameSettings();
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
        private readonly OutputNaming naming = new OutputNaming();

        /// <summary>Where each picker was last pointed, kept across sessions.</summary>
        private readonly FolderMemory folders = FolderMemory.Load();

        /// <summary>
        /// The team map a run with no matrix pack reads, the one the last run with a clash matrix used
        /// before F155, kept across sessions the way the folders are, Q123 answered B by Bader on 2026-10-05.
        /// </summary>
        private readonly TeamMapMemory keptTeams = TeamMapMemory.Load();

        private readonly TeamMapSettings teamSettings = new TeamMapSettings();

        /// <summary>
        /// How the internal workbook is named, the one copy the run's options and the checks before a run
        /// both read, so a check never reads the tolerance box to reach it, F127-R5 attempt 2.
        /// </summary>
        private readonly GenericModelsSettings internalWorkbook = new GenericModelsSettings();

        private bool running;

        /// <summary>
        /// The Run box clicked last and the state it was left in, the start of the next Shift
        /// click's range, F130. Per window, and dropped when the groups are made again.
        /// </summary>
        private GroupRow runAnchor;
        private bool runAnchorState;

        public FederatorWindow(RunLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;

            InitializeComponent();

            GroupsGrid.ItemsSource = groups;

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
            ShowPenetrationWording();
            FillTolerance();
            ShowByDesignWording();
            ShowUndoWording();
            ShowProbeWording();
            RunRangeHelp.Text = ShiftRange.HelpLine;
            ShowOpenDocument();
            StartTheNwfPage();
            StartThePackages();
            FillGroupingModes();

            log.Block("FOLDERS REMEMBERED", folders.Lines());

            // F131, Q123 answered B. The map a run with no clash matrix reads, named when the window
            // opens as the remembered folders are. Since F155 a run with a matrix reads the pack's map,
            // so the block is written only where no matrix is built in.
            if (!MatrixIsBuiltIn())
            {
                log.Block("TEAMS KEPT", keptTeams.ForNoXml(teamSettings).Lines());
            }

            // F155. The built in matrix is read and judged when the window opens, where a picked
            // file was read and judged at its pick, so a matrix that will not read shows before Run.
            ShowTheMatrix();

            // Every line the log writes appears in the window as it is written, so the run
            // is watched rather than read afterwards.
            log.LineWritten += OnLogLine;
            Closing += OnClosing;
            Closed += delegate { log.LineWritten -= OnLogLine; };

            // Which binary this is, in the title bar, because a stale install is otherwise
            // invisible and has caught Bader out twice.
            Title = "Parsons NWC Federator   [" + BuildStamp.Of(typeof(FederatorWindow).Assembly) + "]";

            LogBox.AppendText(log.ReadAll());
            LogBox.ScrollToEnd();

            // The words are in Core where a test reads them, RunLog.WhereTheLogIs.
            ProgressLine.Text = log.WhereTheLogIs();
        }

        private void OnLogLine(string line)
        {
            LogBox.AppendText(line + Environment.NewLine);
            LogBox.ScrollToEnd();
        }

        // ---------- Step 2, the grouping choice ----------

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
            // Another grouping makes other groups, and a key means another thing under it, so
            // the groups start clean as after a Scan.
            Regroup(false);
        }

        // ---------- Step 2, grouping ----------

        /// <summary>
        /// A click on a Run box, F130, Bader's request 5. The box has already flipped its own
        /// row through the binding. Shift held at the click gives every row from the anchor to
        /// this one the anchor's state, in the order the grid shows them, by ShiftRange. Each
        /// row is set through Include, so the blocked row's refusal and every reader of the
        /// ticks see it, and the summary of ticked groups is refreshed once.
        /// </summary>
        private void OnRunBoxClicked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            RunBoxFlipped(box == null ? null : box.DataContext as GroupRow);
        }

        /// <summary>
        /// The space bar on a Run cell, F130-R1, the reviewer's point on F130. The stock column
        /// ticked a row when Space was pressed on its cell, and the read only template column
        /// took that away, so a person who arrows onto the Run cell and presses Space ticked
        /// nothing. Space on the cell now flips its row and goes the way a click on the box
        /// goes, Shift and all. Space on the box itself is left to the box, which raises Click.
        /// </summary>
        private void OnGroupsGridKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DataGridCell cell = e.OriginalSource as DataGridCell;

            if (e.Key != System.Windows.Input.Key.Space || cell == null || !ReferenceEquals(cell.Column, RunColumn))
            {
                return;
            }

            GroupRow row = cell.DataContext as GroupRow;

            if (row == null || !row.CanInclude)
            {
                return;
            }

            e.Handled = true;
            row.Include = !row.Include;
            RunBoxFlipped(row);
        }

        /// <summary>
        /// One Run box flipped, by a click on it or the space bar on its cell. The row has already
        /// taken its new state.
        /// </summary>
        private void RunBoxFlipped(GroupRow clicked)
        {
            if (clicked == null)
            {
                return;
            }

            bool shift = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift)
                == System.Windows.Input.ModifierKeys.Shift;

            List<GroupRow> shown = new List<GroupRow>();

            foreach (object item in GroupsGrid.Items)
            {
                GroupRow row = item as GroupRow;

                if (row != null)
                {
                    shown.Add(row);
                }
            }

            RowsToSet<GroupRow> set = ShiftRange.Of(shown, runAnchor, runAnchorState, clicked, clicked.Include, shift);

            foreach (GroupRow taken in set.Rows)
            {
                taken.Include = set.State;
            }

            runAnchor = clicked;
            runAnchorState = set.State;
            ShowGroupingSummary();
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

        // ---------- Step 3, the naming patterns ----------

        /// <summary>
        /// Puts the current patterns into the boxes. Called once when the window opens, so
        /// the defaults are visible rather than hidden in the code.
        /// </summary>
        private void ShowNaming()
        {
            Show(naming.Nwf, NwfLevel, NwfDiscipline, NwfType, NwfNumber, NwfAllBuildings);
            Show(naming.Nwd, NwdLevel, NwdDiscipline, NwdType, NwdNumber, NwdAllBuildings);
            Show(naming.Workbook, WorkbookLevel, WorkbookDiscipline, WorkbookType,
                WorkbookNumber, WorkbookAllBuildings);
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

        /// <summary>
        /// Every NWF name in that folder, or an empty list. F71 hands these to Core, which
        /// compares names and knows nothing about a disk.
        ///
        /// A folder that is not there, cannot be read or was typed half way through is the
        /// ordinary case while somebody is filling the box in, so it is an empty list and
        /// never an error. Nothing here stops a run or reaches a label.
        /// </summary>
        private static IList<string> NwfNamesIn(string folder)
        {
            List<string> names = new List<string>();

            if (string.IsNullOrEmpty(folder))
            {
                return names;
            }

            try
            {
                if (!Directory.Exists(folder))
                {
                    return names;
                }

                foreach (string path in Directory.GetFiles(folder, "*" + OutputPaths.NwfExtension))
                {
                    names.Add(Path.GetFileName(path));
                }
            }
            catch (Exception)
            {
                // A drive that went away, a permission, a path too long. An empty list is
                // the honest answer and the column simply says nothing extra.
                return names;
            }

            return names;
        }

        /// <summary>
        /// The one line under the group table, shown only while a group is in that state.
        /// The words are Core's so a test can read them.
        /// </summary>
        private void ShowNearbyNwfLine(int nearby)
        {
            if (NearbyNwfLine == null)
            {
                return;
            }

            if (nearby == 0)
            {
                NearbyNwfLine.Text = string.Empty;
                NearbyNwfLine.Visibility = Visibility.Collapsed;
                return;
            }

            NearbyNwfLine.Text = SimilarNames.WhatToDo(nearby);
            NearbyNwfLine.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Whether a run has a clash matrix to read, F155, Bader's decision of 2026-10-13: one matrix pack built
        /// into the tool, MatrixPack.TheOne. It stands where a path in the Clash XML box stood, so the labels, the
        /// confirm and the open file run read a built in matrix the way they read a picked one.
        /// </summary>
        private static bool MatrixIsBuiltIn()
        {
            foreach (MatrixPack built in MatrixPack.All())
            {
                string whyNone;

                if (built.XmlFile(out whyNone) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The pack the run being started reads, chosen by the project code of its NWC file names,
        /// PackChoice, F166. Null until a run chooses it, and then the matrix is read off the only
        /// pack built in where there is one.
        /// </summary>
        private MatrixPack chosenPack;

        /// <summary>
        /// Chooses the pack of a run from the file names alone, before anything is opened. A run
        /// that matches no pack, more than one or mixed codes stops here, said in the log and in a
        /// window, and false is returned.
        /// </summary>
        private bool ChoosePackOrStop(IList<KeyValuePair<string, IList<string>>> groups, string what)
        {
            chosenPack = null;
            PackChoice choice = PackChoice.For(groups, MatrixPack.All(), new ContainerNameSettings());

            if (choice.Stopped)
            {
                log.Line("PACK     " + what + " is stopped, " + choice.Why);
                SetProgress("Stopped. " + choice.Why);
                Warn(choice.Why);
                return false;
            }

            chosenPack = choice.Pack;
            return true;
        }

        /// <summary>
        /// The built in clash matrix, read through MatrixCorrections.ReadPack with the corrections and the team map
        /// the pack holds beside it, or null with no pack, where the tests saved in each NWF run as they did with
        /// no clash matrix read. A pack that will not read throws, so the caller says why as it did for a clash matrix.
        /// </summary>
        private ExchangeDocument ReadTheMatrix()
        {
            string whyNone;
            MatrixPack pack = chosenPack ?? MatrixPack.TheOneWithAMatrix(MatrixPack.All(), out whyNone);
            return pack == null ? null : MatrixCorrections.ReadPack(pack);
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
        /// The penetration box's label and its grey line, both read off Core. F72.
        ///
        /// The help line names the threshold, and the threshold is a SETTING, so typing
        /// the number into the XAML would put a second copy of it in the one place nothing
        /// can test. A project that changes the setting gets a window that says the new
        /// number, because the window asks rather than remembers.
        /// </summary>
        private void ShowPenetrationWording()
        {
            if (MarkPenetrations == null)
            {
                return;
            }

            // The defaults come from the Core object that holds them, the same way
            // ShowImageDefaults reads a fresh ImageOptions.
            SizeSettings defaults = new SizeSettings();

            MarkPenetrations.Content = PenetrationSettings.TickLabel;

            if (MarkPenetrationsHelp != null)
            {
                MarkPenetrationsHelp.Text = PenetrationSettings.HelpLine(defaults);
            }

            // F235, Bader's answer to Q195. The box opens as the Core default says, ticked, on both pages, and the NWF page's run
            // reads its own box.
            MarkPenetrations.IsChecked = new ReportOptions().MarkPenetrations;

            if (NwfMarkPenetrations != null)
            {
                NwfMarkPenetrations.Content = PenetrationSettings.TickLabel;
                NwfMarkPenetrations.IsChecked = new ReportOptions().MarkPenetrations;
            }

            if (NwfMarkPenetrationsHelp != null)
            {
                NwfMarkPenetrationsHelp.Text = PenetrationSettings.HelpLine(defaults);
            }
        }


        /// <summary>
        /// The tolerance drop down, F76 and F155. Every entry, the label and the grey line come off Core, because
        /// the offered values are a setting and a copy typed into the XAML is the one nothing can test. 25 mm is
        /// selected when the window opens, Bader's decision of 2026-10-13, and the drop down is the only control.
        /// </summary>
        private void FillTolerance()
        {
            if (ToleranceBox == null)
            {
                return;
            }

            ToleranceLabel.Content = ToleranceChoice.PickerLabel;
            ToleranceHelp.Text = ToleranceChoice.HelpLine;
            ToleranceBox.Items.Clear();

            foreach (string choice in ToleranceChoice.Choices())
            {
                ToleranceBox.Items.Add(choice);
            }

            ToleranceBox.SelectedIndex = Array.IndexOf(ToleranceChoice.OfferedMillimetres, ToleranceChoice.DefaultMillimetres);
        }

        /// <summary>
        /// What the drop down says, F76, for a run that creates or runs clash tests, read by
        /// ToleranceChoice.FromThePicker with no throw, FR-166. Null where nothing is selected,
        /// and then the person is told why in Core's words before anything starts.
        /// </summary>
        private ToleranceChoice ToleranceOrSayWhyNot(string what)
        {
            string refused;
            ToleranceChoice tolerance = ToleranceChoice.FromThePicker(
                ToleranceBox == null ? -1 : ToleranceBox.SelectedIndex,
                out refused);

            if (tolerance == null)
            {
                log.Line(ToleranceChoice.Prefix + " " + what + " did not start. " + refused);
                Warn(refused);
            }

            return tolerance;
        }

        /// <summary>
        /// How many tests the built in matrix holds, for the confirm dialog, or zero where
        /// no pack is built in or it will not read. The run reads and reports the matrix
        /// itself, so a failure here is not said twice.
        /// </summary>
        private int TestsInTheMatrix()
        {
            try
            {
                ExchangeDocument exchange = ReadTheMatrix();
                return exchange == null ? 0 : exchange.Tests.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// The by design box's label and grey line, read
        /// off Core, F72b. Nothing here is typed into the XAML.
        /// </summary>
        private void ShowByDesignWording()
        {
            if (MarkByDesign == null)
            {
                return;
            }

            MarkByDesign.Content = ByDesignPairs.TickLabel;

            if (MarkByDesignHelp != null)
            {
                MarkByDesignHelp.Text = ByDesignPairs.HelpLine;
            }

            // Q72. Read off Core in the constructor the way every other label is, because
            // a default typed into the XAML as well is a second copy that drifts.
            if (RebuildDriftedSets != null)
            {
                RebuildDriftedSets.Content = SetRebuildSettings.TickLabel;
            }

            if (RebuildDriftedSetsHelp != null)
            {
                RebuildDriftedSetsHelp.Text = SetRebuildSettings.HelpLine;
            }



            // F136. The same shape, ticked or not off the setting the run reads, and set
            // here because the window is made new at every open. The setting is ticked
            // since F114's add-in pass, Q131, so a box unticked for one run is ticked again
            // the next time the window opens, and the loop unticks it through -Untick.
            if (MakeViewpoints != null)
            {
                MakeViewpoints.Content = ViewpointRequest.TickLabel;
                MakeViewpoints.IsChecked = new ReportOptions().MakeViewpoints;
            }

            if (MakeViewpointsHelp != null)
            {
                MakeViewpointsHelp.Text = ViewpointRequest.HelpLine + ". " + ViewpointRequest.DeleteLine + ".";
            }
        }

        /// <summary>The undo button's label and grey line, both read off Core, F72c.</summary>
        private void ShowUndoWording()
        {
            if (UndoAutoReviewedButton == null)
            {
                return;
            }

            UndoAutoReviewedButton.Content = UndoAutoReview.ButtonLabel;

            if (UndoAutoReviewedHelp != null)
            {
                UndoAutoReviewedHelp.Text = UndoAutoReview.HelpLine;
            }
        }

        /// <summary>
        /// The Undo auto Reviewed button, F72c. Every test in the open document, and only
        /// the clashes carrying this tool's record that are still at Reviewed, each put
        /// back to the status its record names. Nothing is saved, the same as the two hand
        /// buttons above it. Runs on the plugin thread like everything else.
        /// </summary>
        private void OnUndoAutoReviewed(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            running = true;
            UndoAutoReviewedButton.IsEnabled = false;

            try
            {
                log.Line(UndoAutoReview.Prefix + " started by hand on the open document, nothing is saved");

                // FR-166. The undo creates no clash test and never reads the tolerance, so the
                // drop down is not read and nothing selected in it stops nothing.
                FederationEngine engine = new FederationEngine(
                    SetProgress, log, null, null, ReportsWanted(ToleranceChoice.Default()));
                UndoAutoReviewed undo = engine.UndoAutoReviewedByHand();

                string said = undo.Tally == null
                    ? "Nothing was read."
                    : undo.Tally.PutBackCount + " put back of " + undo.Tally.Considered + " looked at."
                        + (undo.ChangedTheDocument
                            ? " The open file is changed and not saved."
                            : string.Empty);

                UndoAutoReviewedLine.Text = said;
                SetProgress(said);
            }
            catch (Exception error)
            {
                log.Failure("the undo", error, "stopped, whatever was already put back stays put back");
                SetProgress("The undo stopped on an error.");
                Warn("The undo stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                running = false;
                UndoAutoReviewedButton.IsEnabled = true;
            }
        }

        /// <summary>The probe button's label and grey line, both read off Core, F86.</summary>
        private void ShowProbeWording()
        {
            if (ProbeButton == null)
            {
                return;
            }

            ProbeButton.Content = ProbeSettings.ButtonLabel;

            if (ProbeHelp != null)
            {
                ProbeHelp.Text = ProbeSettings.HelpLine;
            }
        }

        /// <summary>
        /// The Probe model properties button, F86. A folder of NWC files, each opened in
        /// turn, or the document that is open. Opening a folder replaces whatever is open,
        /// so that is confirmed the way the run confirms it. Runs on the plugin thread.
        /// </summary>
        private void OnProbeModelProperties(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                return;
            }

            MessageBoxResult which = MessageBox.Show(
                this,
                "Probe a folder of NWC files?" + Environment.NewLine + Environment.NewLine
                    + "Yes picks the folder and opens each NWC in turn, which replaces whatever is open."
                    + Environment.NewLine
                    + "No reads the document that is open now, without opening anything."
                    + Environment.NewLine
                    + "Cancel does nothing.",
                "Parsons NWC Federator",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (which == MessageBoxResult.Cancel)
            {
                return;
            }

            string folder = null;

            if (which == MessageBoxResult.Yes)
            {
                // The parent folder of the packages is where the NWC files are, so the picker
                // starts there, and nothing is remembered because this is not that picker.
                folder = PickFolder(
                    "Pick the folder of NWC files to probe", StartFor(PickerKind.PackageParent, packagePanel.Parent));

                if (folder == null)
                {
                    return;
                }

                if (!ConfirmTheOpenDocumentGoes())
                {
                    Log("Probe cancelled before anything was opened.");
                    return;
                }
            }

            running = true;
            ProbeButton.IsEnabled = false;

            try
            {
                log.Line("PROBE    started by hand, "
                    + (folder == null ? "on the open document" : "over " + folder));

                // FR-166. The probe creates no clash test and never reads the tolerance.
                FederationEngine engine = new FederationEngine(
                    SetProgress, log, null, null, ReportsWanted(ToleranceChoice.Default()));
                IList<string> lines = folder == null
                    ? engine.ProbeTheOpenDocumentByHand()
                    : engine.ProbeTheFolderByHand(folder);

                ShowSetLines(lines);
                ProbeLine.Text = lines.Count == 0 ? "Nothing was read." : lines[lines.Count - 1].Trim();
                SetProgress("Probe finished. " + RunLog.TheLogSaysWhy());
            }
            catch (Exception error)
            {
                log.Failure("the probe", error, "stopped, every CSV already written is kept");
                SetProgress("The probe stopped on an error.");
                Warn("The probe stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                running = false;
                ProbeButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Says what opening a file would discard, the way the run's confirm dialog does,
        /// and asks. Nothing open means nothing is lost and nothing is asked.
        /// </summary>
        private bool ConfirmTheOpenDocumentGoes()
        {
            if (child != null)
            {
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ConfirmedByTheParent("What opening a file discards"));
                return true;
            }

            string discarded;

            try
            {
                discarded = DocumentGuard.WhatClearWouldDiscard();
            }
            catch (Exception error)
            {
                discarded = "Navisworks would not say what is open (" + error.Message + ").";
            }

            if (discarded == null)
            {
                return true;
            }

            MessageBoxResult answer = MessageBox.Show(
                this,
                "This will be discarded without saving:" + Environment.NewLine + "    " + discarded
                    + Environment.NewLine + Environment.NewLine + "Carry on?",
                "Parsons NWC Federator",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            return answer == MessageBoxResult.OK;
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

                RememberFolder(PickerKind.Logo, dialog.FileName);
                LogoBox.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// What the run will do about reports. The folder is worked out here so the window
        /// can say where the workbooks are going before Run is pressed. The tolerance is
        /// handed in, read once by the caller, FR-166.
        /// </summary>
        private ReportOptions ReportsWanted(ToleranceChoice tolerance)
        {
            ReportOptions options = new ReportOptions();

            // Logs and reports never go inside the folder being scanned. A real run put them
            // in C:\00_NM\NWC Fed\NWC\test001, which is where its own input lives.
            // Since F232 the folder read is the parent folder of the packages, and the clash XML is off, its default.
            options.SourceFolder = Trimmed(packagePanel.Parent);
            options.RebuildExisting = RebuildExisting.IsChecked == true;

            // The page and the photos are fixed on in the options themselves, so nothing here
            // sets them. A weekly run wants both every time. The units of a model are never set, F201.
            options.ApplyFileSettings = ApplyFileSettings.IsChecked == true;
            options.MarkPenetrations = MarkPenetrations.IsChecked == true;
            options.Tolerance = tolerance;
            options.MarkByDesign = MarkByDesign.IsChecked == true;
            options.RebuildDriftedSets = RebuildDriftedSets.IsChecked == true;

            options.MakeViewpoints = MakeViewpoints.IsChecked == true;
            options.LogoPath = Trimmed(LogoBox.Text);
            options.Names = settings;
            options.GenericModels = internalWorkbook;
            return options;
        }

        // ---------- Run ----------

        /// <summary>
        /// The choices on the Outputs and Clash steps that every run of groups reads, under
        /// RUN SETTINGS, for both pages, one method so the two say them alike.
        /// </summary>
        private void SayTheRunsChoices(ToleranceChoice tolerance, bool penetrations)
        {
            log.Line("apply file to old: "
                + (ApplyFileSettings.IsChecked == true
                    ? "YES, which changes WHICH CLASHES every test it changes will find next time it runs. It resets nothing by itself, measured 5y"
                    : "no, differences are reported and nothing is changed"));
            log.Line("penetrations     : "
                + (penetrations
                    ? "YES, a small service through a wall, floor, roof or structural foundation becomes Reviewed"
                    : "no, every clash keeps the status it has"));
            log.Line("clash tolerance  : " + tolerance.Label() + ", chosen in the tool, the drop down being its only source, "
                + "set on every test and beats the built in matrix and the document");
            log.Line("priority         : built in, read off the matrix pack built into the tool, so "
                + "a Priority column where the pack reads, which its PACK and PRIORITY lines say");
            log.Line("by design        : " + (MarkByDesign.IsChecked == true
                ? "YES, a clash between two sets the built in pairs name becomes Reviewed"
                : "no, the built in pairs are not read"));
            log.Line(ViewpointRequest.SettingsLine(MakeViewpoints.IsChecked == true));
            log.Line("NWD naming       : "
                + (naming.DateTheNwd
                    ? "dated, so every week is kept"
                    : "overwrites, so only the latest week exists"));
            log.Line("republish NWD    : yes, fixed");
            log.Line("model units      : as each NWC carries them, never set, F201");
            log.Line("report units     : " + Federator.Core.Report.ReportUnits.Name
                + ", always, converted before anything is written");
        }

        /// <summary>
        /// The run's one confirm. Since F214 every group runs in a Navisworks of its own, and since F232 the NWC page has
        /// that route alone, so what is open here is never discarded and the screen says so.
        /// </summary>
        private bool ConfirmClear(IList<string> runPaths, ToleranceChoice tolerance)
        {
            if (child != null)
            {
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ConfirmedByTheParent("The run"));
                return true;
            }

            string message = string.Join(
                Environment.NewLine, new List<string>(RunPath.ConfirmLines(runPaths)).ToArray());

            // F76. Since F155 a tolerance is always chosen, so the lines are said on every
            // run. The test count is read off the clash matrix, the one number they need.
            IList<string> toleranceLines = tolerance.WarningLines(
                runPaths.Count, TestsInTheMatrix());

            if (toleranceLines.Count > 0)
            {
                message += Environment.NewLine + Environment.NewLine
                    + string.Join(Environment.NewLine, new List<string>(toleranceLines).ToArray());
            }

            // Q72, and the same rule as the tolerance line above it: said only when the
            // box is ON, because a sentence that appears on every run saying nothing will
            // happen teaches people to skip the screen. The box changes the NWF and the
            // NWF is the only record of what has been fixed, so the screen says so.
            string rebuildLine = SetRebuildSettings.ConfirmLine(
                RebuildDriftedSets.IsChecked == true);

            if (rebuildLine != null)
            {
                message += Environment.NewLine + Environment.NewLine + rebuildLine;
            }

            // F132 attempt 2, the breaker's finding R5, and the same rule again: said only
            // when the box is ON. The pass reaches the tests saved in the NWF as well as
            // those from the clash matrix, and the NWF is saved with the statuses it moves.
            string byDesignLine = ByDesignPairs.ConfirmLine(MarkByDesign.IsChecked == true);

            if (byDesignLine != null)
            {
                message += Environment.NewLine + Environment.NewLine + byDesignLine;
            }

            message += Environment.NewLine + Environment.NewLine + PackageRunWords.NothingHereIsTouched;

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
        private void RunJobs(IList<FederationJob> jobs, ToleranceChoice tolerance, PackageRun packages)
        {
            running = true;
            SetTheRunButtons(false);

            // Outside the try, so the RESULT block written in the finally carries what the
            // shared coordinates rule did in THIS run, or nothing where no engine was made.
            FederationEngine engine = null;
            AutoSaveOff autoSave = null;

            try
            {
                // F80. The mark and the line together, off the log's own monotonic clock,
                // so the run time in the timing block is RUN started to RUN finished and
                // not the session, which carried 473 seconds of a person reading a table.
                log.RunStarted(jobs.Count);

                // F223, Q207. Auto-Save off while the run works, put back in the finally before RESULT.
                autoSave = new AutoSaveOff(log.Line);

                // Read once, before the first group, so a matrix that will not read stops
                // the run here rather than part way through the second building. F155: the
                // matrix is the one built into the tool, and the log names its pack.
                ExchangeDocument exchange = ReadTheMatrix();
                TeamMap teams = keptTeams.ForRun(exchange, teamSettings);

                if (exchange == null)
                {
                    string whyNone;
                    MatrixPack.TheOneWithAMatrix(MatrixPack.All(), out whyNone);
                    log.Line("RUN      no clash matrix is read, " + whyNone + ", so the tests saved in each NWF "
                        + "run, no set will be built and no test created");

                    // F131, Q123 answered B. The kept map, named before any group reads it.
                    SayLines(teams.Lines());
                }
                else
                {
                    log.Line("RUN      the clash matrix is " + exchange.SourcePath);
                    log.Line("RUN      it holds " + exchange.Sets.Count
                        + (exchange.Sets.Count == 1 ? " set and " : " sets and ")
                        + exchange.Tests.Count + (exchange.Tests.Count == 1 ? " test" : " tests"));
                    SayTheTeamsAndCorrections(exchange);
                }

                ReportOptions options = ReportsWanted(tolerance);

                // F208. A run of packages has no one NWF folder: each group writes into its package's Log folder, under
                // the package's report folder beside its NWF and NWD folders, which its job carries.
                log.Line("RUN      logs and reports go in the Log folder of each package, under its report folder beside its NWF "
                    + "and NWD folders. The NWF and NWD folders hold the NWF and NWD files alone");
                log.Line("RUN      the clash XML is "
                    + (options.WriteXml ? "written beside each workbook" : "off"));

                engine = new FederationEngine(SetProgress, log, exchange, teams, options);
                engine.UsePack(chosenPack);
                IList<JobOutcome> outcomes = engine.Run(jobs);

                // F222. No discipline workbook is written here: the window that started the run writes them off this group's result.
                CountThePackageGroups(packages.Tally, outcomes);

                // What each group actually did, in the list. A group that was rebuilt
                // reads Rebuilt here once the run has settled it.
                ShowRunPathsAfterTheRun(outcomes, exchange != null);

                // What the Revit container inside each NWC says its building is. Only
                // knowable once a document has been open, so it goes in after the run.
                // Information, exactly like the scan findings. Nothing acts on it.
                sourceFindings = SourceMismatchFindings.From(engine.SourcePairs, settings);
                log.Block(RunLog.SourceFindingsSectionTitle, sourceFindings.Lines());

                // F82. Straight after the source findings, because it is the same kind of
                // thing: something the run noticed that no output carries. A set at zero
                // in ONE group says a discipline was not exported for that building. A set
                // at zero in EVERY group says the set itself is wrong, and 38 of 61 were
                // in that state with nothing anywhere adding it up.
                log.Block(SetsAcrossTheRun.BlockTitle, engine.SetsAcrossTheRun.Lines());

                // The rows on their own, because Block writes none, so the machine
                // readable log carries what the block carries.
                foreach (SetRunRow row in engine.SetsAcrossTheRun.Rows())
                {
                    log.Row("set across the run", row.Path,
                        EventRow.Count(row.GroupsAtZero), row.Phrase());
                }

                // PART 4 and PART 5 across the run, one line each. The blocks themselves
                // are per group, because a model sits in a group and a workset belongs
                // to one. Nothing stops the run for either, Q65, and since Bader's answer to
                // Q99 and Q100 the ALIGNMENT line says which groups had their clash skipped.
                foreach (string line in engine.ModelCheckRunLines())
                {
                    log.Line(line);
                }

                // F72b. Rule B across the run, and the pairs that matched nothing, named once.
                foreach (string line in engine.ByDesignRunLines())
                {
                    log.Line(line);
                }

                // F76. Where every report row's tolerance was read, counted across the
                // run. Every row should read off the document, and the line says so when
                // one did not.
                log.Line(ToleranceChoice.BlocksLine(engine.ReportBlocks, engine.ReportRan));
                log.Line(ToleranceChoice.ReadFromLine(
                    engine.ToleranceFromDocument,
                    engine.ToleranceFromTool, engine.ToleranceUnknown));

                log.RunFinished();
                SetProgress(RunFinishedLine());
            }
            catch (Exception error)
            {
                log.Failure("the run", error, "stopped, everything already written is kept");
                SetProgress("Run stopped on an error.");
                Warn("The run stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                if (autoSave != null)
                {
                    autoSave.Dispose();
                }

                // The result block is written whatever happened, so a run that stopped still
                // leaves a readable log with its summary at the end.
                WriteTheResult(engine, packages.Tally.Lines());

                // F214. The groups this run ended with, those before a throw among them, for a package's own result.
                lastOutcomes = engine == null ? null : engine.OutcomesSoFar;
                running = false;
                SetTheRunButtons(true);
            }
        }

        /// <summary>The progress line once a run of groups ends, for the scanned run and the picked NWFs.</summary>
        private string RunFinishedLine()
        {
            return "Run finished. " + log.CountOf(GroupOutcome.Done) + " done, "
                + log.CountOf(GroupOutcome.Partial) + " partial, "
                + log.CountOf(GroupOutcome.Failed) + " failed.";
        }

        /// <summary>
        /// The RESULT block of one run. engine is that run's engine, or null where the run
        /// stopped before one was made, and then the block says nothing of the shared
        /// coordinates rule or the viewpoints box rather than guess. packages is the tally of a run of
        /// packages, and null for the open file run.
        /// The log is written in the logs folder and, once the block is on the disk, copied with
        /// its .tsv into the Log folder of the run, F158, which replaces F151's rule that no copy
        /// is written anywhere else. The Log folder is not the NWF or the NWD folder.
        /// </summary>
        private void WriteTheResult(FederationEngine engine, IList<string> packages = null)
        {
            try
            {
                // F127. The coverage of this run, the engine's own tally, so RESULT carries every
                // group's COVERAGE FAILED line and the counts of the tests once across the run. F208, each
                // group of a run of packages with its community and package and the totals.
                log.WriteResultBlock(
                    engine == null ? null : engine.CoordinatesAcrossTheRun,
                    engine == null || engine.MakesViewpoints,
                    engine == null ? null : engine.CoverageAcrossTheRun,
                    engine == null ? null : engine.GenericModelsAcrossTheRun,
                    packages: packages);
            }
            catch (Exception error)
            {
                log.Failure("writing the result block", error, "kept going, the lines above are still on disk");
            }

            CopyTheLogIntoTheLogFolder(engine);
        }

        /// <summary>
        /// F158. The run log and its .tsv copied into every Log folder the run wrote into, once the
        /// RESULT block is on the disk. Nothing there is replaced, LogCopy. What it did is one line
        /// each in the window and in the original log, which the copy cannot carry since it was
        /// made first. A run that made no engine has no Log folder and copies nothing.
        /// </summary>
        private void CopyTheLogIntoTheLogFolder(FederationEngine engine)
        {
            try
            {
                if (engine == null || engine.LogFolders.Count == 0)
                {
                    return;
                }

                foreach (string folder in engine.LogFolders)
                {
                    foreach (string line in LogCopy.Into(folder, log.Path, RowLog.PathFor(log.Path), DateTime.Now))
                    {
                        log.Line(line);
                    }
                }
            }
            catch (Exception error)
            {
                log.Failure("copying the run log into the Log folder", error, "kept going, the log is still in the logs folder");
            }
        }

        // ---------- Step 4, clash ----------

        /// <summary>
        /// Says what the built in clash matrix holds when the window opens, F155, where the Clash XML picker
        /// said it of a clash matrix: its pack, its sets and tests counted out of it, and what HealthCheck makes
        /// of the corrected sets, so a damaged matrix is obvious before a run rather than after one. D1. The
        /// check's whole summary goes in the log and one line of it under the Clash step.
        /// </summary>
        private void ShowTheMatrix()
        {
            if (SetsSummary == null)
            {
                return;
            }

            // F155. This runs while the window opens, so nothing in it may throw out of it: a fault
            // anywhere in the read, the TEAMS and MATRIX lines or the health check is said and the
            // window still opens.
            try
            {
                ExchangeDocument exchange = ReadTheMatrix();

                if (exchange == null)
                {
                    string whyNone;
                    MatrixPack.TheOneWithAMatrix(MatrixPack.All(), out whyNone);
                    log.Line("MATRIX   no clash matrix is read, " + whyNone);
                    SetsSummary.Text = "No clash matrix is built in, so each NWF runs the tests saved in it.";
                    return;
                }

                string held = exchange.Sets.Count + (exchange.Sets.Count == 1 ? " set and " : " sets and ")
                    + exchange.Tests.Count + (exchange.Tests.Count == 1 ? " test." : " tests.");

                log.Line("MATRIX   " + exchange.SourcePath + " holds " + held);

                // Q104. The health check judges the sets as the run will build them, corrected.
                SayTheTeamsAndCorrections(exchange);

                HealthCheckResult health = HealthCheck.Run(exchange);
                log.Block("HEALTH " + exchange.Pack.Name, health.Summary());

                SetsSummary.Text = "Built in matrix " + exchange.Pack.Name + ". It holds " + held + " " + health.Line(exchange.HasSets);
            }
            catch (Exception error)
            {
                log.Failure("reading the built in clash matrix", error, "a run reads it again and says why it will not read");

                // A label, so it says where to look and never what was thrown.
                SetsSummary.Text = "The built in clash matrix would not read. " + RunLog.TheLogSaysWhy();
            }
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
        /// Remembers what one picker was pointed at, and logs the one line Core hands back
        /// where the folders file could not be written, FR-162.
        /// </summary>
        private void RememberFolder(PickerKind kind, string picked)
        {
            string couldNotKeep = folders.Remember(kind, picked);

            if (couldNotKeep != null)
            {
                log.Line(couldNotKeep);
            }
        }

        /// <summary>
        /// What was read beside the clash matrix, wherever a run or the window's open says which matrix it read:
        /// the TEAMS lines of its team map with each set whose name carries no code named, F131, then every
        /// correction MatrixCorrections made to it, Q104, so the TEAMS lines come before the MATRIX lines. Since
        /// F155 the map is the pack's and is never kept for a later run, so no line says it is.
        /// </summary>
        private void SayTheTeamsAndCorrections(ExchangeDocument exchange)
        {
            SayLines(exchange.Teams.Lines());
            SayLines(exchange.Teams.SetLines(exchange.Sets, new ViewpointSettings().SetNameSeparator));
            SayLines(exchange.Corrections);
        }

        private void SayLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                log.Line(line);
            }
        }

        /// <summary>
        /// The whole job on the file already open. No scan, no source folder, no grouping.
        ///
        /// HOW A PERSON TELLS THE TWO APART. The blue box on this step says what it will
        /// run and where it will write, naming the open file. The Run button in the bar under
        /// the steps is the other path, and it says how many groups are ticked. One names a
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

            // FR-166. Refused before the run starts, as the scanned run refuses it, where the
            // throw inside the run stopped it part way and wrote a RESULT for nothing.
            ToleranceChoice tolerance = ToleranceOrSayWhyNot("the run on the open document");

            if (tolerance == null)
            {
                return;
            }

            string openPath = OpenDocumentPath();

            if (!ChoosePackOrStop(
                new List<KeyValuePair<string, IList<string>>> { PackChoice.Group(openPath, new List<string> { openPath }) },
                "the run on the open file"))
            {
                return;
            }

            running = true;
            RunOpenButton.IsEnabled = false;

            // Outside the try, for the same reason as the scanned run.
            FederationEngine engine = null;

            AutoSaveOff autoSave = null;

            try
            {
                // F223, Q207. Auto-Save off while the run works, put back in the finally before RESULT.
                autoSave = new AutoSaveOff(log.Line);

                TeamMap teams;
                ExchangeDocument exchange = ReadTheMatrixForAnOpenFileRun(out teams);

                ReportOptions options = ReportsWanted(tolerance);

                // F136. The open file run has no RUN SETTINGS block, and the box holds for
                // it too, so its state is said here before the group starts.
                log.Line(ViewpointRequest.SettingsLine(options.MakeViewpoints));

                // No folder is handed in. The engine reads the report folder off the open
                // file through OpenDocumentJob.ReportFolder, the same rule ShowOpenDocument
                // uses for the line above the button. Handing the report folder in as the
                // NWF folder is what once wrote to Clash Reports\Clash Reports, which is gone.
                engine = new FederationEngine(
                    SetProgress, log, exchange, teams, options);
                engine.UsePack(chosenPack);

                // The engine writes the GROUP lines and the OPEN FILE block itself, so
                // they are there whatever happens inside it.
                JobOutcome outcome = engine.RunOpenDocument();
                SayWhatAnOpenFileRunAddedUp(engine);

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

                // A21. The open file run feeds the sets tally and nothing wrote the block, so
                // a reader could not tell whether that was by design. It is: one file is one
                // group and there is nothing to add up across groups.
                log.Line("SETS     " + SetsAcrossTheRun.BlockTitle.ToLowerInvariant()
                    + " is not written for the open file run, one file is one group and there is nothing to add up across groups");

                if (autoSave != null)
                {
                    autoSave.Dispose();
                }

                // The RESULT block is written whatever happened, the same as the scanned run.
                // This used to be missing, so an open file run ended with no RESULT block.
                WriteTheResult(engine);
                running = false;
                RunOpenButton.IsEnabled = true;
                ShowOpenDocument();
            }
        }

        /// <summary>
        /// The clash matrix of an open file run and of a run of picked NWFs, F129, read once before the first
        /// group. Since F155 it is the matrix built into the tool, whose saved tests each document already holds
        /// run where they sit. With no pack built in, the tests saved in each document run, the ordinary weekly
        /// case before F155, and a document holding none means nothing runs and the log says so. The team map is
        /// the run's, named before any group reads it.
        /// </summary>
        private ExchangeDocument ReadTheMatrixForAnOpenFileRun(out TeamMap teams)
        {
            ExchangeDocument exchange = ReadTheMatrix();

            if (exchange != null)
            {
                teams = keptTeams.ForRun(exchange, teamSettings);
                log.Line("OPEN     clash matrix " + exchange.SourcePath);
                SayTheTeamsAndCorrections(exchange);
                return exchange;
            }

            string whyNone;
            MatrixPack.TheOneWithAMatrix(MatrixPack.All(), out whyNone);
            log.Line("OPEN     no clash matrix is read, " + whyNone + ", so the tests saved in the document run, "
                + "or nothing runs when it holds none");

            // F131, Q123 answered B. The kept map, named before the group reads it.
            teams = keptTeams.ForRun(null, teamSettings);
            SayLines(teams.Lines());
            return null;
        }

        /// <summary>
        /// The lines an open file run and a run of picked NWFs write across the run once the
        /// engine is done, F129, one method so the two routes say the same.
        /// </summary>
        private void SayWhatAnOpenFileRunAddedUp(FederationEngine engine)
        {
            // PART 4 and PART 5 across the run, one line each. The blocks themselves
            // are per group, because a model sits in a group and a workset belongs
            // to one. Nothing stops the run for either, Q65, and since Bader's answer to
            // Q99 and Q100 the ALIGNMENT line says whether the clash was skipped.
            foreach (string line in engine.ModelCheckRunLines())
            {
                log.Line(line);
            }

            // F72b. Rule B across the run, and the pairs that matched nothing, named once.
            foreach (string line in engine.ByDesignRunLines())
            {
                log.Line(line);
            }

            // F76. Where every report row's tolerance was read, counted across the
            // run. Every row should read off the document, and the line says so when
            // one did not.
            log.Line(ToleranceChoice.BlocksLine(engine.ReportBlocks, engine.ReportRan));
            log.Line(ToleranceChoice.ReadFromLine(
                engine.ToleranceFromDocument,
                engine.ToleranceFromTool, engine.ToleranceUnknown));
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

            // F208. The groups each package makes follow the grouping and the names of steps 2 and 3, which may have
            // changed since the tree was read.
            if (ReferenceEquals(Steps.SelectedItem, PackagesStep))
            {
                RefreshThePackageDetails();
            }

            // F232. The rows of step 2 follow the names of step 3 and the Report folder, which may have changed, each tick kept.
            if (ReferenceEquals(Steps.SelectedItem, GroupingStep) && !running)
            {
                Regroup(true);
            }
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
                MatrixIsBuiltIn());

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
        /// The Build sets button. The clash matrix's sets into whatever document is open,
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

            if (!MatrixIsBuiltIn())
            {
                Warn("No clash matrix is built into the tool, so there are no sets to build.");
                return;
            }

            string setsPath = OpenDocumentPath();

            if (!ChoosePackOrStop(
                new List<KeyValuePair<string, IList<string>>> { PackChoice.Group(setsPath, new List<string> { setsPath }) },
                "building the sets by hand"))
            {
                return;
            }

            running = true;
            BuildSetsButton.IsEnabled = false;

            try
            {
                log.Line("SETS     started by hand, reading the built in clash matrix");
                ExchangeDocument exchange = ReadTheMatrix();
                SayTheTeamsAndCorrections(exchange);

                // FR-166. Building the sets creates no clash test and never reads the tolerance.
                FederationEngine engine = new FederationEngine(
                    SetProgress, log, exchange, keptTeams.ForRun(exchange, teamSettings),
                    ReportsWanted(ToleranceChoice.Default()));
                engine.UsePack(chosenPack);
                SetBuildOutcome outcome = engine.BuildSetsByHand();

                ShowSetLines(outcome.Lines());
                SetsSummary.Text = outcome.Summary();
                SetProgress("Sets finished. " + outcome.Summary());
            }
            catch (Exception error)
            {
                log.Failure("building the sets from the built in clash matrix", error, "stopped, nothing further was built");
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
        /// The Run tests button. The clash matrix's tests created into and run against
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

            if (!MatrixIsBuiltIn())
            {
                Warn("No clash matrix is built into the tool, so there are no tests to run.");
                return;
            }

            // FR-166. The tests are created at the chosen tolerance, so it is refused before
            // anything is read, where it threw as the engine was made.
            ToleranceChoice tolerance = ToleranceOrSayWhyNot("Tests into open model");

            if (tolerance == null)
            {
                return;
            }

            string testsPath = OpenDocumentPath();

            if (!ChoosePackOrStop(
                new List<KeyValuePair<string, IList<string>>> { PackChoice.Group(testsPath, new List<string> { testsPath }) },
                "running the tests by hand"))
            {
                return;
            }

            running = true;
            RunTestsButton.IsEnabled = false;

            try
            {
                log.Line("CLASH    started by hand, reading the built in clash matrix");
                ExchangeDocument exchange = ReadTheMatrix();
                SayTheTeamsAndCorrections(exchange);

                FederationEngine engine = new FederationEngine(
                    SetProgress, log, exchange, keptTeams.ForRun(exchange, teamSettings), ReportsWanted(tolerance));
                engine.UsePack(chosenPack);
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
                    "creating and running the clash tests from the built in clash matrix",
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
            // F214. Nobody is at a package's own Navisworks, so what a box would say goes in the log and nothing waits.
            if (child != null)
            {
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.NoBox(text));

                // A refusal before any group says why through this box, so the first one is the package's reason.
                childRanNoGroupBecause = childRanNoGroupBecause ?? PackageRunWords.OneLine(text);
                return;
            }

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

        /// <summary>The Close button. Whether the window may close is OnClosing's one rule.</summary>
        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Every way the window closes, the Close button, the title bar close and Alt+F4, FR-163.
        /// Only the Close button refused while a run was going, and the run's repaint pump let
        /// the other two close the window under it. Now each is cancelled while a run, an undo,
        /// a probe or a hand button is going, in the words the Close button used.
        /// </summary>
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!running)
            {
                return;
            }

            e.Cancel = true;
            Warn("The run is still going. Let it finish.");
        }
    }
}
