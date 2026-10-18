using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Federator.Addin.Engine;
using Federator.Core.Clash;
using Federator.Core.Communities;
using Federator.Core.Report;
using Federator.Core.Rerun;
using Federator.Core.Views;

namespace Federator.Addin.Ui
{
    /// <summary>One row of the table the NWF page shows after a run: a test that moved and its five counts, before > after.</summary>
    public sealed class RefreshRow
    {
        public string Test { get; set; }

        public string New { get; set; }

        public string Active { get; set; }

        public string Reviewed { get; set; }

        public string Approved { get; set; }

        public string Resolved { get; set; }
    }

    /// <summary>
    /// The NWF page of the window, F188, Bader's answer 3 of Q174 and his order of 14 Oct 2026, task 9. The words,
    /// the rules for the steps, the list of the NWFs last used and the words of the confirmation are Core's,
    /// Federator.Core.Rerun, and are tested there. This part reads the disk and the controls and nothing more.
    /// </summary>
    public partial class FederatorWindow
    {
        private readonly ObservableCollection<RefreshRow> nwfTable = new ObservableCollection<RefreshRow>();
        private RecentNwfs recentNwfs = new RecentNwfs();
        private bool fillingRecent;

        /// <summary>Called once from the constructor, after the tolerance drop down of the NWC page is filled.</summary>
        private void StartTheNwfPage()
        {
            NwfPageHeading.Text = NwfPageWords.Heading;
            RecentNwfLabel.Content = NwfPageWords.RecentLabel;
            NwfChangedHeading.Text = NwfPageWords.ChangedHeading;
            NwfStepsHeading.Text = NwfPageWords.StepsHeading;
            NwfStepsHelp.Text = NwfPageWords.StepsHelp;
            NwfRulesLine.Text = NwfPageWords.RulesLine;
            NwfTableHeading.Text = NwfPageWords.TableHeading;
            RunPickedNwfsButton.Content = NwfPageWords.Run;

            NwfStepRefresh.Content = NwfPageSteps.RefreshName;
            NwfStepTests.Content = NwfPageSteps.TestsName;
            NwfStepPictures.Content = NwfPageSteps.PicturesName;
            NwfStepViewpoints.Content = NwfPageSteps.ViewpointsName;
            NwfStepReports.Content = NwfPageSteps.ReportsName;

            NwfToleranceLabel.Content = ToleranceChoice.PickerLabel;
            NwfToleranceHelp.Text = ToleranceChoice.HelpLine;
            NwfToleranceBox.Items.Clear();

            foreach (string choice in ToleranceChoice.Choices())
            {
                NwfToleranceBox.Items.Add(choice);
            }

            NwfToleranceBox.SelectedIndex = Array.IndexOf(ToleranceChoice.OfferedMillimetres, ToleranceChoice.DefaultMillimetres);

            NwfTable.ItemsSource = nwfTable;
            NwfChangedBox.Text = NwfPageWords.NoNwfPicked;

            RebuildExisting.Content = NwcPageRule.RebuildLabel;
            RebuildHelp.Text = NwcPageRule.RebuildHelp;

            LoadTheRecentNwfs();
            ApplyTheNwfSteps();
        }

        // ---------- the steps ----------

        private NwfPageSteps TheNwfSteps()
        {
            NwfPageSteps steps = new NwfPageSteps();
            steps.Refresh = NwfStepRefresh.IsChecked == true;
            steps.RunTests = NwfStepTests.IsChecked == true;
            steps.Pictures = NwfStepPictures.IsChecked == true;
            steps.Viewpoints = NwfStepViewpoints.IsChecked == true;
            steps.Reports = NwfStepReports.IsChecked == true;
            return steps;
        }

        private void OnNwfStepChanged(object sender, RoutedEventArgs e)
        {
            ApplyTheNwfSteps();
        }

        /// <summary>A step whose need is off is greyed, and the one grey line says why. The rule is Core's, NwfPageSteps.</summary>
        private void ApplyTheNwfSteps()
        {
            NwfPageSteps steps = TheNwfSteps();
            string[] whys =
            {
                steps.WhyOff(NwfPageSteps.ViewpointsName),
                steps.WhyOff(NwfPageSteps.ReportsName),
                steps.WhyOff(NwfPageSteps.PicturesName)
            };

            NwfStepViewpoints.IsEnabled = whys[0] == null;
            NwfStepReports.IsEnabled = whys[1] == null;
            NwfStepPictures.IsEnabled = whys[2] == null;

            List<string> said = new List<string>();

            foreach (string why in whys)
            {
                if (why != null)
                {
                    said.Add(why);
                }
            }

            NwfStepsHelp.Text = said.Count == 0 ? NwfPageWords.StepsHelp : string.Join(" ", said.ToArray());
        }

        // ---------- the pages ----------

        /// <summary>
        /// A page was shown. The NWC page keeps its own Run button and the NWF page has its own, so the one that
        /// does not belong is hidden and a press can never start the other page's route.
        /// </summary>
        private void OnPageShown(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, Pages))
            {
                return;
            }

            bool nwf = Pages.SelectedIndex == 1;
            RunButton.Visibility = nwf ? Visibility.Collapsed : Visibility.Visible;

            if (nwf)
            {
                ShowTheNwfPreview();
            }
        }

        // ---------- the NWFs last used ----------

        private static string RecentNwfsPath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ParsonsNwcFederator");

            return Path.Combine(folder, RecentNwfs.FileName);
        }

        private void LoadTheRecentNwfs()
        {
            try
            {
                string path = RecentNwfsPath();
                recentNwfs = File.Exists(path) ? RecentNwfs.Read(File.ReadAllLines(path)) : new RecentNwfs();
            }
            catch (Exception)
            {
                // A settings file that will not read is no list. The page works without it.
                recentNwfs = new RecentNwfs();
            }

            FillTheRecentBox();
        }

        private void FillTheRecentBox()
        {
            fillingRecent = true;

            try
            {
                RecentNwfBox.Items.Clear();

                foreach (string path in recentNwfs.Paths)
                {
                    RecentNwfBox.Items.Add(path);
                }
            }
            finally
            {
                fillingRecent = false;
            }
        }

        private void RememberTheNwfs(IList<string> paths)
        {
            try
            {
                foreach (string path in paths)
                {
                    recentNwfs.Remember(path);
                }

                string file = RecentNwfsPath();
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllLines(file, recentNwfs.ToLines());
            }
            catch (Exception error)
            {
                log.Failure("remembering the NWFs last used", error, "kept going, the list is not saved");
            }

            FillTheRecentBox();
        }

        private void OnRecentNwfChosen(object sender, SelectionChangedEventArgs e)
        {
            if (fillingRecent || RecentNwfBox.SelectedItem == null)
            {
                return;
            }

            PickedNwfBox.Text = RecentNwfBox.SelectedItem.ToString();
            ShowTheNwfPreview();
        }

        private void OnPickedNwfLostFocus(object sender, RoutedEventArgs e)
        {
            ShowTheNwfPreview();
        }

        // ---------- what changed ----------

        /// <summary>
        /// The one NWF the box names, or null. A folder of NWFs or more than one has no single list of changed files,
        /// and the page says so rather than showing the first.
        /// </summary>
        private string TheOneNwf(out string why)
        {
            why = null;
            string text = Trimmed(PickedNwfBox.Text);

            if (text.Length == 0)
            {
                why = NwfPageWords.NoNwfPicked;
                return null;
            }

            NwfPickPlan plan;

            try
            {
                plan = NwfPick.From(text, IncludeSubfolders.IsChecked == true);
            }
            catch (Exception)
            {
                why = "The pick could not be read.";
                return null;
            }

            IList<string> paths = plan.Paths();

            if (paths.Count == 0)
            {
                why = plan.WhyNoRun ?? NwfPageWords.NoNwfPicked;
                return null;
            }

            if (paths.Count > 1)
            {
                why = paths.Count + " NWFs are picked. The list of changed NWC files is shown for one NWF at a time.";
                return null;
            }

            return paths[0];
        }

        private string TheLogFolderOf(string nwf)
        {
            // F189. Where the run that built or refreshed this NWF put its Log folder, else beside the file.
            string known = NwfPlacesStore.Load().LogFolderOf(nwf);
            return known.Length > 0 ? known : OpenDocumentJob.ReportFolder(nwf);
        }

        /// <summary>
        /// The changed NWC files, read off the last manifest and the disk. Nothing is opened. A file whose size or time
        /// moved is hashed, which reads the file, so this runs when the NWF is picked and the page is shown and never
        /// on a key.
        /// </summary>
        private ManifestComparison ReadWhatChanged(string nwf, out NwcManifest last)
        {
            last = null;

            try
            {
                string manifestPath = Path.Combine(TheLogFolderOf(nwf), NwcManifest.FileName);

                if (!File.Exists(manifestPath))
                {
                    return null;
                }

                last = NwcManifest.Parse(File.ReadAllLines(manifestPath));

                if (last == null)
                {
                    return null;
                }

                List<ManifestEntry> now = new List<ManifestEntry>();
                List<string> gone = new List<string>();

                foreach (ManifestEntry entry in last.Entries)
                {
                    if (File.Exists(entry.Path))
                    {
                        FileInfo info = new FileInfo(entry.Path);
                        now.Add(new ManifestEntry(entry.Path, info.Length, info.LastWriteTimeUtc, null));
                    }
                }

                return NwcManifest.Compare(last, now, RefreshRun.Sha256Of);
            }
            catch (Exception error)
            {
                log.Failure("reading what changed in the NWC files of " + nwf, error, "kept going, the page says UNKNOWN");
                return null;
            }
        }

        private void ShowTheNwfPreview()
        {
            string why;
            string nwf = TheOneNwf(out why);

            if (nwf == null)
            {
                NwfChangedBox.Text = why ?? NwfPageWords.NoNwfPicked;
                return;
            }

            NwcManifest last;
            ManifestComparison changes = ReadWhatChanged(nwf, out last);

            if (changes == null)
            {
                NwfChangedBox.Text = NwfPageWords.NoManifest;
                return;
            }

            List<string> lines = new List<string>(changes.Lines());

            foreach (ManifestEntry entry in last.Entries)
            {
                if (!File.Exists(entry.Path))
                {
                    lines.Add("missing  " + entry.Path + "  (the run will name it and stop)");
                }
            }

            NwfChangedBox.Text = string.Join(Environment.NewLine, lines.ToArray());
        }

        // ---------- the run ----------

        private ToleranceChoice NwfToleranceOrSayWhyNot(string what)
        {
            string refused;
            ToleranceChoice tolerance = ToleranceChoice.FromThePicker(
                NwfToleranceBox == null ? -1 : NwfToleranceBox.SelectedIndex,
                out refused);

            if (tolerance == null)
            {
                log.Line(ToleranceChoice.Prefix + " " + what + " did not start. " + refused);
                Warn(refused);
            }

            return tolerance;
        }

        /// <summary>
        /// The confirmation before a refresh, Cancel the default. It lists the changed files, the tests, the tool
        /// viewpoint folder that will be made again and the old jpg files. What is not known says UNKNOWN. The NWF
        /// is copied into the Backup folder by the run before it opens anything, so Cancel changes nothing.
        /// </summary>
        private bool ConfirmTheRefresh(NwfPickPlan plan, NwfPageSteps steps)
        {
            string nwf = plan.Paths().Count > 0 ? plan.Paths()[0] : string.Empty;
            string name = plan.Paths().Count == 1 ? Path.GetFileName(nwf) : plan.Paths().Count + " NWFs";
            string logFolder = plan.Paths().Count == 1 ? TheLogFolderOf(nwf) : null;
            NwcManifest last = null;
            ManifestComparison changes = plan.Paths().Count == 1 ? ReadWhatChanged(nwf, out last) : null;

            int toolViews = last == null ? -1 : (int)last.Fact("toolviews");
            int oldPictures = logFolder == null ? -1 : OldPicturesIn(new[] { RefreshConfirm.PicturesFolderOf(logFolder, nwf) });

            string text = RefreshConfirm.Text(
                name,
                logFolder == null ? null : NwfBackupName.FolderBeside(logFolder),
                changes,
                steps.Words(),
                TestsInTheMatrix(),
                toolViews,
                last == null ? -1 : (int)last.Fact("outside"),
                oldPictures,
                ToolViewFolder.Name);

            log.Block("CONFIRM " + name, new List<string>(text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)));
            MessageBoxResult answer = MessageBox.Show(
                this, text, RefreshConfirm.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

            return answer == MessageBoxResult.OK;
        }

        /// <summary>
        /// The old pictures in these folders, F217, by Core's rule, `RefreshConfirm.OldPictures`, read off the disk, minus one
        /// where a folder would not read, which the confirm says UNKNOWN.
        /// </summary>
        private static int OldPicturesIn(IEnumerable<string> folders)
        {
            return RefreshConfirm.OldPictures(folders, folder => Directory.Exists(folder) ? Directory.GetFiles(folder) : null);
        }

        /// <summary>Puts the table of what each test did on the NWF page, the tests that moved, from every NWF the run refreshed.</summary>
        private void ShowTheNwfTable(IList<JobOutcome> outcomes)
        {
            nwfTable.Clear();

            foreach (JobOutcome outcome in outcomes)
            {
                if (outcome == null || outcome.RefreshResults == null)
                {
                    continue;
                }

                foreach (TestRefresh row in outcome.RefreshResults.Moved())
                {
                    nwfTable.Add(new RefreshRow
                    {
                        Test = row.Test,
                        New = row.Before[0] + " > " + row.After[0],
                        Active = row.Before[1] + " > " + row.After[1],
                        Reviewed = row.Before[2] + " > " + row.After[2],
                        Approved = row.Before[3] + " > " + row.After[3],
                        Resolved = row.Before[4] + " > " + row.After[4]
                    });
                }
            }

            NwfSummary.Text = nwfTable.Count == 0 ? NwfPageWords.TableEmpty : nwfTable.Count + " tests moved. The table is below.";
        }

        // ---------- the NWC page's Rebuild ----------

        /// <summary>The names of the ticked or unticked groups whose NWF is already there.</summary>
        private List<string> GroupsWithAnNwf()
        {
            List<string> names = new List<string>();

            foreach (GroupRow group in groups)
            {
                if (group.NwfExists)
                {
                    names.Add(group.Building);
                }
            }

            return names;
        }

        private void OnRebuildToggled(object sender, RoutedEventArgs e)
        {
            bool ticked = RebuildExisting.IsChecked == true;

            // F214. In a package's own Navisworks the box is set from the order, and the person confirmed it once in the
            // window that started the run, so nothing is asked again where nobody is there to answer.
            if (ticked && child != null)
            {
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ConfirmedByTheParent("Rebuilding the groups whose NWF is there"));
            }
            else if (ticked)
            {
                MessageBoxResult answer = MessageBox.Show(
                    this,
                    NwcPageRule.RebuildConfirm(GroupsWithAnNwf()),
                    RefreshConfirm.Title,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Cancel);

                if (answer != MessageBoxResult.OK)
                {
                    RebuildExisting.IsChecked = false;
                    ticked = false;
                }
            }

            foreach (GroupRow group in groups)
            {
                group.RebuildAllowed = ticked;

                if (!ticked && group.NwfExists)
                {
                    group.Include = false;
                }
            }
        }
    }
}
