using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Federator.Addin.Engine;
using Federator.Core.Grouping;
using Federator.Core.Naming;

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
        private bool running;
        private bool suspendRegroup;

        public FederatorWindow()
        {
            InitializeComponent();

            FilesGrid.ItemsSource = files;
            GroupsGrid.ItemsSource = groups;
            OutputsGrid.ItemsSource = groups;

            files.CollectionChanged += delegate { Regroup(); };
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

            SearchOption depth = IncludeSubfolders.IsChecked == true
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            string[] found;

            try
            {
                found = Directory.GetFiles(folder, "*.nwc", depth);
            }
            catch (Exception error)
            {
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
                    }
                }
            }
            finally
            {
                suspendRegroup = false;
            }

            SourceSummary.Text = found.Length + " NWC found, " + (found.Length - unreadable)
                + " readable, " + unreadable + " that cannot be read.";

            Log("Scanned " + folder + " and found " + found.Length + " NWC files.");

            if (unreadable > 0)
            {
                Log(unreadable + " file names could not be read. They are unticked in step 1 with the reason.");
            }

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
            RefreshOutputsSummary();
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

        private void RefreshOutputsSummary()
        {
            int ready = 0;

            foreach (GroupRow group in groups)
            {
                if (group.Include)
                {
                    ready++;
                }
            }

            OutputsSummary.Text = ready + " groups ticked to run. The run log goes next to the NWF folder as "
                + FederationEngine.RunLogFileName + ".";
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

            RunJobs(jobs, Path.Combine(nwfFolder, FederationEngine.RunLogFileName));
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
        private void RunJobs(IList<FederationJob> jobs, string runLogPath)
        {
            running = true;
            RunButton.IsEnabled = false;

            try
            {
                Log("Run started. " + jobs.Count + " groups.");

                FederationEngine engine = new FederationEngine(SetProgress, Log);
                IList<JobOutcome> outcomes = engine.Run(jobs, runLogPath);

                int written = 0;
                int partial = 0;
                int failed = 0;

                foreach (JobOutcome outcome in outcomes)
                {
                    switch (outcome.Result)
                    {
                        case JobResult.Written:
                            written++;
                            break;
                        case JobResult.Partial:
                            partial++;
                            break;
                        default:
                            failed++;
                            break;
                    }
                }

                string summary = "Run finished. " + written + " written, " + partial
                    + " partial, " + failed + " failed.";
                SetProgress(summary);
                Log(summary);
                Log("Run log: " + runLogPath);
            }
            catch (Exception error)
            {
                SetProgress("Run stopped on an error.");
                Log("Run stopped: " + error);
                Warn("The run stopped." + Environment.NewLine + Environment.NewLine + error.Message);
            }
            finally
            {
                running = false;
                RunButton.IsEnabled = true;
            }
        }

        // ---------- Small helpers ----------

        private void SetProgress(string text)
        {
            ProgressLine.Text = text;
            Pump();
        }

        private void Log(string text)
        {
            LogBox.AppendText(text + Environment.NewLine);
            LogBox.ScrollToEnd();
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
