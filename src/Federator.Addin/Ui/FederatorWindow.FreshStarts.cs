using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Federator.Addin.Engine;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;
using Federator.Core.Disciplines;
using Federator.Core.Rerun;

namespace Federator.Addin.Ui
{
    /// <summary>
    /// The group a Navisworks started for one group runs, F214 and since F222 one group and not a package: the order and
    /// the group's place in it.
    /// </summary>
    public sealed class PackageRunChild
    {
        public PackageRunChild(PackageRunOrder order, int index)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (index < 0 || index >= order.Units.Count)
            {
                throw new ArgumentOutOfRangeException("index", index, "the order holds " + order.Units.Count + " groups");
            }

            Order = order;
            Index = index;
        }

        public PackageRunOrder Order { get; private set; }

        public int Index { get; private set; }

        public PackageRunUnit Unit
        {
            get { return Order.Units[Index]; }
        }

        public string Package
        {
            get { return Unit.Package; }
        }
    }

    /// <summary>
    /// A fresh Navisworks per package, F214, Bader's answer to Q204 of 18 Oct 2026, measured in docs\history\scan.md 5z-zj,
    /// and since F222, his answer to Q211, a fresh Navisworks per group. Run pressed once: the window that a person runs
    /// plans and confirms the packages as before, then writes the order, one unit a group, starts Federator.Runner.exe from
    /// the bundle and answers while it waits, copying the runner's status lines into its log. The runner starts one
    /// Navisworks per group in turn and runs this add-in there with the order and the group's index, and that window, a
    /// child, sets every control as the person left them, reads the tree, ticks its one package, runs the route of its page
    /// on its one group with no box shown, writes its result and the facts its discipline reports need, and closes. One
    /// group failing, crashing or passing the ceiling never stops the next, and the parent writes each package's discipline
    /// workbooks off the facts and joins every result into one RESULT. The rules and the words are Core's,
    /// Federator.Core.Communities.
    /// </summary>
    public partial class FederatorWindow
    {
        /// <summary>The runner's file in the bundle, beside this add-in.</summary>
        private const string RunnerFile = "Federator.Runner.exe";

        /// <summary>The group this window runs in its own Navisworks, or null in the window a person runs.</summary>
        private readonly PackageRunChild child;

        /// <summary>The groups the last run of this window ended with, read by a child to write its result.</summary>
        private IList<JobOutcome> lastOutcomes;

        /// <summary>Why a child ran no group, where it knows, or null.</summary>
        private string childRanNoGroupBecause;

        /// <summary>The window of one group's own Navisworks, which runs that group as soon as it is shown and closes.</summary>
        public FederatorWindow(RunLog log, PackageRunChild child)
            : this(log)
        {
            this.child = child;
            Loaded += OnLoadedAsAPackage;
        }

        private void OnLoadedAsAPackage(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedAsAPackage;
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(RunAsAPackage));
        }

        /// <summary>The child's whole life: the controls, the tree, the one tick, the route, the result, the close.</summary>
        private void RunAsAPackage()
        {
            PackageRunOrder order = child.Order;
            string package = child.Package;

            try
            {
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ChildStarted(child.Index, order.Units.Count, child.Unit.Label, order.ParentLog));

                if (!SetTheControls(order.Controls))
                {
                    return;
                }

                packagePanel.Report = order.Report;

                // F232. The NWF page reads its tree off the Report folder and ticks its one NWF, the NWC page reads its tree off
                // the parent and ticks its one package.
                if (order.Page == PackageRunOrder.NwfPage)
                {
                    ReadTheNwfs();

                    if (nwfTicks == null || !nwfTree.WasRead || nwfTree.Find(package) == null)
                    {
                        childRanNoGroupBecause = "the package is not in the tree read off " + order.Report + " in its own Navisworks, "
                            + Words.Or(nwfTree == null ? null : nwfTree.WhyNoTree, "it is not among the packages read");
                        log.Line(PackageRunWords.Prefix + " " + childRanNoGroupBecause);
                        return;
                    }

                    nwfTicks.Tick(child.Unit.Group, true);
                    TheNwfTicksMoved();
                    OnRunNwfPackages(this, null);
                    return;
                }

                packagePanel.Parent = order.Parent;
                ReadThePackages();

                Package found = packageTree == null ? null : packageTree.Find(package);

                if (found == null)
                {
                    childRanNoGroupBecause = "the package is not in the tree read off " + order.Parent + " in its own Navisworks, "
                        + Words.Or(packageTree == null ? null : packageTree.WhyNoTree, "it is not among the packages read");
                    log.Line(PackageRunWords.Prefix + " " + childRanNoGroupBecause);
                    return;
                }

                packageTicks.Tick(found, true);
                TheTicksMoved();
                OnRun(this, null);
            }
            catch (Exception error)
            {
                childRanNoGroupBecause = "its run threw " + error.GetType().Name + ": " + error.Message;
                log.Failure("the run of group " + child.Unit.Label + " in its own Navisworks", error, "its result says so and the window closes");
            }
            finally
            {
                WriteThePackagesResult();
                Close();
            }
        }

        /// <summary>
        /// The child's result: the group the run ended with and what it read for the discipline reports, its files and the
        /// facts of its models, F222, or why none ran, and its own RESULT block.
        /// </summary>
        private void WriteThePackagesResult()
        {
            string path = child.Order.ResultPathFor(child.Index);

            try
            {
                PackageRunResult result = new PackageRunResult(child.Package, log.IsWritingToDisk ? log.Path : null);

                if (lastOutcomes == null || lastOutcomes.Count == 0)
                {
                    result.NoGroupBecause = childRanNoGroupBecause ?? "no group of it ran, its log says why";
                }
                else
                {
                    foreach (JobOutcome outcome in lastOutcomes)
                    {
                        result.Groups.Add(TheGroupsEnd(outcome));
                    }
                }

                if (log.IsWritingToDisk)
                {
                    string all = log.ReadAll();
                    IList<string> lines = PackageRunResult.ResultLinesOf(all.Replace("\r\n", "\n").Split('\n'));

                    foreach (string line in lines)
                    {
                        result.ResultLines.Add(line);
                    }
                }

                result.Write(path);
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ResultWritten(path));
            }
            catch (Exception error)
            {
                log.Failure("writing the result of group " + child.Unit.Label + " to " + path, error, "the window that started the run says it has no result");
            }
        }

        /// <summary>One group's end for its result, with its files and the facts its run read, or why they were not read.</summary>
        private PackageRunGroup TheGroupsEnd(JobOutcome outcome)
        {
            if (outcome == null || outcome.Job == null)
            {
                return new PackageRunGroup(child.Unit.Name, GroupOutcome.Failed, -1, "the group's outcome was not kept")
                    .FactsNot("the group's outcome was not kept");
            }

            PackageRunGroup end = new PackageRunGroup(outcome.Job.Building, outcome.Result, GroupSize.ClashesIn(outcome.Clash), outcome.Reason);

            foreach (string file in outcome.Job.Files)
            {
                end.Files.Add(file);
            }

            return outcome.DisciplineFacts != null
                ? end.WithFacts(outcome.DisciplineFacts)
                : end.FactsNot(outcome.DisciplineFactsNotRead ?? "the group ended before its models were read");
        }

        /// <summary>
        /// Every control of the window a run reads, by its name: each text box a person can type in, each tick box and each
        /// drop down, in that order, so a child sets the boxes the grouping reads before the drop down that regroups.
        /// </summary>
        private IList<ControlValue> TheControlsAsTheyStand()
        {
            List<ControlValue> texts = new List<ControlValue>();
            List<ControlValue> ticks = new List<ControlValue>();
            List<ControlValue> choices = new List<ControlValue>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            Walk(this, texts, ticks, choices, seen);
            texts.AddRange(ticks);
            texts.AddRange(choices);
            return texts;
        }

        private static void Walk(DependencyObject node, List<ControlValue> texts, List<ControlValue> ticks, List<ControlValue> choices, HashSet<string> seen)
        {
            FrameworkElement element = node as FrameworkElement;

            if (element != null && !string.IsNullOrEmpty(element.Name) && seen.Add(element.Name))
            {
                TextBox text = element as TextBox;
                CheckBox tick = element as CheckBox;
                ComboBox choice = element as ComboBox;

                if (text != null && !text.IsReadOnly)
                {
                    texts.Add(new ControlValue(ControlValue.TextKind, text.Name, text.Text));
                }
                else if (tick != null)
                {
                    ticks.Add(new ControlValue(ControlValue.TickKind, tick.Name, tick.IsChecked == null ? string.Empty : tick.IsChecked.Value.ToString()));
                }
                else if (choice != null)
                {
                    choices.Add(new ControlValue(ControlValue.ChoiceKind, choice.Name, choice.SelectedIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                }
            }

            foreach (object next in LogicalTreeHelper.GetChildren(node))
            {
                DependencyObject inner = next as DependencyObject;

                if (inner != null)
                {
                    Walk(inner, texts, ticks, choices, seen);
                }
            }
        }

        /// <summary>
        /// Each control of the order set by its name, in the order's order, one that cannot be set named. False where any
        /// could not be set, and the group is then not run, so it never runs on settings the person did not choose.
        /// </summary>
        private bool SetTheControls(IList<ControlValue> controls)
        {
            int set = 0;

            foreach (ControlValue control in controls)
            {
                object found = FindName(control.Name);
                TextBox text = found as TextBox;
                CheckBox tick = found as CheckBox;
                ComboBox choice = found as ComboBox;
                int index;

                if (control.Kind == ControlValue.TextKind && text != null)
                {
                    text.Text = control.Value;
                    set++;
                }
                else if (control.Kind == ControlValue.TickKind && tick != null)
                {
                    tick.IsChecked = control.Value.Length == 0 ? (bool?)null : string.Equals(control.Value, bool.TrueString, StringComparison.Ordinal);
                    set++;
                }
                else if (control.Kind == ControlValue.ChoiceKind && choice != null
                    && int.TryParse(control.Value, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out index)
                    && index >= -1 && index < choice.Items.Count)
                {
                    choice.SelectedIndex = index;
                    set++;
                }
                else
                {
                    log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ControlNotHere(control.Kind, control.Name));
                }
            }

            log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ControlsCopied(set));

            if (set == controls.Count)
            {
                return true;
            }

            childRanNoGroupBecause = PackageRunWords.ControlsNotAllSet(controls.Count - set, controls.Count);
            log.Line(PackageRunWords.Prefix + " " + childRanNoGroupBecause);
            return false;
        }

        /// <summary>
        /// The parent's half: the order written, one unit a group, the runner started, the window answering while it runs,
        /// the runner's lines copied into the log, then every group's result read, each package's discipline workbooks
        /// written off the facts the results carried, F222, and the results joined into one RESULT. A group with no result
        /// is named with why and never said as not run, and nothing here stops on one. A wait that throws is said once and
        /// goes on, and a runner past every group's ceiling and wait added up is ended, so this window never waits for ever
        /// and never reads the results while its runner still runs.
        /// </summary>
        private void RunPackagesInFreshStarts(string page, string parent, PackageRun run, IList<PackageRunUnit> units)
        {
            PackageRunOrder order = new PackageRunOrder
            {
                Page = page,
                Parent = parent,
                Report = run.Report,
                PluginId = FederatorPlugin.PluginName + "." + FederatorPlugin.DeveloperCode,
                ParentLog = log.IsWritingToDisk ? log.Path : null,
                Folder = PackageRunOrder.FolderFor(
                    Path.Combine(Path.GetDirectoryName(RunLog.DefaultLogFolder()), PackageRunOrder.RunsFolderName), DateTime.Now, Directory.Exists)
            };

            foreach (PackageRunUnit unit in units)
            {
                order.Units.Add(unit);
            }

            foreach (ControlValue control in TheControlsAsTheyStand())
            {
                order.Controls.Add(control);
            }

            running = true;
            SetTheRunButtons(false);
            log.RunStarted(units.Count, "group", "groups");
            SetProgress(PackageRunWords.Waiting(units.Count));
            bool runnerEnded = false;

            try
            {
                Directory.CreateDirectory(order.Folder);
                order.Write();

                string runner = Path.Combine(Path.GetDirectoryName(typeof(FederatorWindow).Assembly.Location), RunnerFile);
                string navisworks;

                using (Process self = Process.GetCurrentProcess())
                {
                    navisworks = Path.GetDirectoryName(self.MainModule.FileName);
                }

                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.RunnerStarting(runner, order.Units.Count, order.PackageCount, order.OrderPath));

                if (!File.Exists(runner))
                {
                    throw new FileNotFoundException("the runner is not in the bundle", runner);
                }

                ProcessStartInfo start = new ProcessStartInfo(runner, Quoted(navisworks) + " " + Quoted(order.OrderPath))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(start))
                {
                    runnerEnded = WaitForTheRunner(process, order, order.Units.Count);
                }
            }
            catch (Exception error)
            {
                log.Failure("the run of packages, each group in a fresh Navisworks", error, "every group with no result is named in RESULT");
            }
            finally
            {
                // F223. A runner that ended before its put back left its record, which is put back here, and kept where this
                // window ended the runner, since a Navisworks of the run may still run.
                AutoSaveOff.PutBackWhatARunLeft(log.Line, runnerEnded);
                JoinTheResults(order, run);
                running = false;
                SetTheRunButtons(true);
            }
        }

        /// <summary>
        /// Answers and copies the runner's lines until it ends. A pass that throws is said once and the wait goes on, so
        /// the results are never read while the runner still starts Navisworks, and past the order's limit the runner is
        /// ended, the one process this window started.
        /// </summary>
        private bool WaitForTheRunner(Process process, PackageRunOrder order, int groups)
        {
            bool ended = false;
            long read = 0;
            bool failureSaid = false;
            int limit = order.Settings.RunnerLimitMinutes(groups);
            Stopwatch clock = Stopwatch.StartNew();

            while (!process.WaitForExit(order.Settings.PollMilliseconds))
            {
                if (clock.Elapsed.TotalMinutes > limit)
                {
                    log.Line(PackageRunWords.Prefix + " " + PackageRunWords.RunnerPastItsLimit(limit));
                    process.Kill();
                    process.WaitForExit();
                    ended = true;
                    break;
                }

                try
                {
                    Pump();
                    read = TheRunnersNewLines(order.StatusPath, read);
                }
                catch (Exception error)
                {
                    if (!failureSaid)
                    {
                        failureSaid = true;
                        log.Line(PackageRunWords.Prefix + " " + PackageRunWords.WaitFailed(error.GetType().Name, error.Message));
                    }
                }
            }

            TheRunnersNewLines(order.StatusPath, read);
            log.Line(PackageRunWords.Prefix + " " + PackageRunWords.RunnerEndedWith(process.ExitCode));
            return ended;
        }

        /// <summary>
        /// Every group's result read, each package's discipline workbooks written off the facts they carried, F222, and the
        /// one RESULT written last, a group with none named and never dropped. The live line counts the groups that wrote a
        /// result, and where one did not the person is warned, since it may have written files nobody is told of otherwise.
        /// </summary>
        private void JoinTheResults(PackageRunOrder order, PackageRun run)
        {
            List<PackageRunResult> results = new List<PackageRunResult>();
            List<string> whys = new List<string>();
            int ran = 0;
            PackageTally tally = run.Tally;

            // F222. The packages the run planned, never the tree read again, which a person may have read off another folder.
            Dictionary<string, Package> packages = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);

            foreach (Package package in run.Runnable)
            {
                packages[package.RelativePath] = package;
            }

            for (int i = 0; i < order.Units.Count; i++)
            {
                PackageRunUnit unit = order.Units[i];
                string why;
                PackageRunResult result = PackageRunResult.Read(order.ResultPathFor(i), out why);
                results.Add(result);
                whys.Add(why);

                // F222. A result naming no group is a group that ran nothing, and the live line never says it ran.
                if (result != null && result.Groups.Count > 0)
                {
                    ran++;
                }

                try
                {
                    Package package = packages[unit.Package];

                    if (result == null)
                    {
                        tally.NoResult(package, unit.Name, why);
                        continue;
                    }

                    foreach (PackageRunGroup group in result.Groups)
                    {
                        tally.Add(package, group.Group, group.Outcome, group.Clashes);
                    }

                    if (result.Groups.Count == 0)
                    {
                        tally.NotRun(package, unit.Name, result.NoGroupBecause);
                    }
                }
                catch (Exception error)
                {
                    log.Failure("counting the group " + unit.Label, error, "the other groups are still counted and the RESULT still written");
                }
            }

            try
            {
                // F209 and F222. One workbook per discipline of every package that ran, once all its groups have.
                WriteTheDisciplineReports(run, order.Units, results, whys);
                log.RunFinished();
                log.WriteJoinedResult(PackageRunWords.JoinedResult(order.Units, results, whys, tally));
            }
            catch (Exception error)
            {
                log.Failure("joining the results of the groups", error, "each group's own log still holds its RESULT");
            }

            SetProgress(PackageRunWords.Finished(ran, order.Units.Count));
            string warning = PackageRunWords.Warning(ran, order.Units.Count);

            if (warning != null)
            {
                Warn(warning);
            }
        }

        /// <summary>The runner's lines written since the last read, each into the log, never onto the live line.</summary>
        private long TheRunnersNewLines(string path, long from)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return from;
                }

                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    if (stream.Length <= from)
                    {
                        return from;
                    }

                    stream.Seek(from, SeekOrigin.Begin);
                    byte[] bytes = new byte[stream.Length - from];
                    int got = stream.Read(bytes, 0, bytes.Length);
                    int end = got <= 0 ? -1 : Array.LastIndexOf(bytes, (byte)'\n', got - 1);

                    if (end < 0)
                    {
                        return from;
                    }

                    string text = new UTF8Encoding(false).GetString(bytes, 0, end + 1);

                    foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
                    {
                        if (line.Length > 0)
                        {
                            log.Line(PackageRunWords.Prefix + " " + line);
                        }
                    }

                    return from + end + 1;
                }
            }
            catch (Exception error)
            {
                if (!statusReadFailureSaid)
                {
                    statusReadFailureSaid = true;
                    log.Line(PackageRunWords.Prefix + " " + PackageRunWords.StatusNotRead(error.GetType().Name + ": " + error.Message));
                }

                return from;
            }
        }

        private bool statusReadFailureSaid;
    }
}