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
    /// The NWF page of the window, F188, Bader's answer 3 of Q174 and his order of 14 Oct 2026, task 9, and since F232 his
    /// round of 19 Oct 2026, fix 2, with the packages first. The words, the rules for the steps and the words of the
    /// confirmation are Core's, Federator.Core.Rerun and Federator.Core.Communities, and are tested there. This part reads
    /// the disk and the controls and nothing more.
    /// </summary>
    public partial class FederatorWindow
    {
        private readonly ObservableCollection<RefreshRow> nwfTable = new ObservableCollection<RefreshRow>();

        /// <summary>Called once from the constructor, after the tolerance drop down of the NWC page is filled.</summary>
        private void StartTheNwfPage()
        {
            NwfChangedHeading.Text = NwfPageWords.ChangedHeading;
            NwfStepsHeading.Text = NwfPageWords.StepsHeading;
            NwfStepsHelp.Text = NwfPageWords.StepsHelp;
            NwfRulesLine.Text = NwfPageWords.RulesLine;
            NwfTableHeading.Text = NwfPageWords.TableHeading;

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
        /// A page was shown. The NWC page keeps its Run button at the top and the NWF page has its own, so the one that
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

        // ---------- what changed ----------

        /// <summary>
        /// The one NWF ticked on the NWF page with the Log folder its runs write, or null. None, or more than one, has no single
        /// list of changed files, and the page says so rather than showing the first, F232, Q214.
        /// </summary>
        private PackageNwf TheOneNwf(out string why)
        {
            why = null;
            IList<string> ticked = nwfTicks == null ? new List<string>() : nwfTicks.Ticked();

            if (ticked.Count == 0)
            {
                why = NwfPageWords.NoNwfPicked;
                return null;
            }

            if (ticked.Count > 1)
            {
                why = ticked.Count + " NWFs are ticked. The list of changed NWC files is shown for one NWF at a time.";
                return null;
            }

            try
            {
                foreach (Package package in nwfTicks.PackagesTicked())
                {
                    foreach (PackageNwf nwf in TickedNwfsOf(nwfTree.Parent, package))
                    {
                        return nwf;
                    }
                }
            }
            catch (Exception)
            {
                why = "The NWF folder of the ticked NWF could not be read.";
                return null;
            }

            why = "The ticked NWF is no longer in its NWF folder. Read the Report folder again.";
            return null;
        }

        /// <summary>
        /// The changed NWC files, read off the last manifest in the NWF's Log folder and the disk. Nothing is opened. A file
        /// whose size or time moved is hashed, which reads the file, so this runs when an NWF is ticked and the page is shown
        /// and never on a key.
        /// </summary>
        private ManifestComparison ReadWhatChanged(PackageNwf nwf, out NwcManifest last)
        {
            last = null;

            try
            {
                string manifestPath = Path.Combine(nwf.LogFolder, NwcManifest.FileName);

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
                log.Failure("reading what changed in the NWC files of " + nwf.NwfPath, error, "kept going, the page says UNKNOWN");
                return null;
            }
        }

        private void ShowTheNwfPreview()
        {
            if (NwfChangedBox == null)
            {
                return;
            }

            string why;
            PackageNwf nwf = TheOneNwf(out why);

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
        /// The old pictures in these folders, F217, by Core's rule, `RefreshConfirm.OldPictures`, read off the disk, minus one
        /// where a folder would not read, which the confirm says UNKNOWN.
        /// </summary>
        private int OldPicturesIn(IEnumerable<string> pictureFolders)
        {
            // A folder not there holds none. Any other failure to read it, denied or a drive gone, is thrown on, so the count is
            // UNKNOWN and never a part: Directory.Exists says false for those too.
            string why;
            int count = RefreshConfirm.OldPictures(pictureFolders, folder =>
            {
                try
                {
                    return Directory.GetFiles(folder);
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
            }, out why);

            if (count < 0)
            {
                log.Line("CONFIRM  the old jpg files are UNKNOWN, " + why);
            }

            return count;
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
                    names.Add(group.Label);
                }
            }

            return names;
        }

        private void OnRebuildToggled(object sender, RoutedEventArgs e)
        {
            bool ticked = RebuildExisting.IsChecked == true;

            // F214. In a group's own Navisworks the box is set from the order, and the person confirmed it once in the
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
                group.AllowedMoved();
            }
        }
    }
}
