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
using Federator.Core.Rerun;

namespace Federator.Addin.Ui
{
    /// <summary>The package a Navisworks started for one package runs, F214: the order and the package's place in it.</summary>
    public sealed class PackageRunChild
    {
        public PackageRunChild(PackageRunOrder order, int index)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (index < 0 || index >= order.Packages.Count)
            {
                throw new ArgumentOutOfRangeException("index", index, "the order holds " + order.Packages.Count + " packages");
            }

            Order = order;
            Index = index;
        }

        public PackageRunOrder Order { get; private set; }

        public int Index { get; private set; }

        public string Package
        {
            get { return Order.Packages[Index]; }
        }
    }

    /// <summary>
    /// A fresh Navisworks per package, F214, Bader's answer to Q204 of 18 Oct 2026, measured in docs\history\scan.md 5z-zj.
    /// Run pressed once: the window that a person runs plans and confirms the packages as before, then writes the order,
    /// starts Federator.Runner.exe from the bundle and answers while it waits, copying the runner's status lines into its
    /// log. The runner starts one Navisworks per package in turn and runs this add-in there with the order and the
    /// package's index, and that window, a child, sets every control as the person left them, reads the tree, ticks its one
    /// package, runs the route of its page with no box shown, writes its result and closes. One package failing, crashing
    /// or passing the ceiling never stops the next, and the parent joins every result into one RESULT. The rules and the
    /// words are Core's, Federator.Core.Communities.
    /// </summary>
    public partial class FederatorWindow
    {
        /// <summary>The runner's file in the bundle, beside this add-in.</summary>
        private const string RunnerFile = "Federator.Runner.exe";

        /// <summary>The package this window runs in its own Navisworks, or null in the window a person runs.</summary>
        private readonly PackageRunChild child;

        /// <summary>The groups the last run of this window ended with, read by a child to write its result.</summary>
        private IList<JobOutcome> lastOutcomes;

        /// <summary>Why a child ran no group, where it knows, or null.</summary>
        private string childRanNoGroupBecause;

        /// <summary>The window of one package's own Navisworks, which runs that package as soon as it is shown and closes.</summary>
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
                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.ChildStarted(child.Index, order.Packages.Count, package, order.ParentLog));
                SetTheControls(order.Controls);
                packagePanel.Parent = order.Parent;
                packagePanel.Report = order.Report;
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

                if (order.Page == PackageRunOrder.NwfPage)
                {
                    OnRunNwfPackages(this, null);
                }
                else
                {
                    OnRunPackages(this, null);
                }
            }
            catch (Exception error)
            {
                childRanNoGroupBecause = "its run threw " + error.GetType().Name + ": " + error.Message;
                log.Failure("the run of package " + package + " in its own Navisworks", error, "its result says so and the window closes");
            }
            finally
            {
                WriteThePackagesResult();
                Close();
            }
        }

        /// <summary>The child's result: every group the run ended with, or why none ran, and its own RESULT block.</summary>
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
                        result.Groups.Add(outcome == null || outcome.Job == null
                            ? new PackageRunGroup("UNKNOWN", GroupOutcome.Failed, -1, "the group's outcome was not kept")
                            : new PackageRunGroup(outcome.Job.Building, outcome.Result, GroupSize.ClashesIn(outcome.Clash), outcome.Reason));
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
                log.Failure("writing the result of package " + child.Package + " to " + path, error, "the window that started the run says it has no result");
            }
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

        /// <summary>Each control of the order set by its name, in the order's order, one not in this window named and left.</summary>
        private void SetTheControls(IList<ControlValue> controls)
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
        }

        /// <summary>
        /// The parent's half: the order written, the runner started, the window answering while it runs, the runner's
        /// lines copied into the log, then every package's result read and joined into one RESULT. A package with no
        /// result is named with why and counted as not run, and nothing here stops on one.
        /// </summary>
        private void RunPackagesInFreshStarts(string page, IList<Package> packages, string report, PackageTally tally)
        {
            PackageRunOrder order = new PackageRunOrder
            {
                Page = page,
                Parent = packageTree.Parent,
                Report = report,
                PluginId = FederatorPlugin.PluginName + "." + FederatorPlugin.DeveloperCode,
                ParentLog = log.IsWritingToDisk ? log.Path : null,
                Folder = PackageRunOrder.FolderFor(
                    Path.Combine(Path.GetDirectoryName(RunLog.DefaultLogFolder()), PackageRunOrder.RunsFolderName), DateTime.Now, Directory.Exists)
            };

            foreach (Package package in packages)
            {
                order.Packages.Add(package.RelativePath);
            }

            foreach (ControlValue control in TheControlsAsTheyStand())
            {
                order.Controls.Add(control);
            }

            running = true;
            SetTheRunButtons(false);
            log.RunStarted(packages.Count);
            SetProgress(PackageRunWords.Waiting(packages.Count));

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

                log.Line(PackageRunWords.Prefix + " " + PackageRunWords.RunnerStarting(runner, packages.Count, order.OrderPath));

                if (!File.Exists(runner))
                {
                    throw new FileNotFoundException("the runner is not in the bundle", runner);
                }

                ProcessStartInfo start = new ProcessStartInfo(runner, Quoted(navisworks) + " " + Quoted(order.OrderPath))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                long read = 0;

                using (Process process = Process.Start(start))
                {
                    while (!process.WaitForExit(200))
                    {
                        Pump();
                        read = TheRunnersNewLines(order.StatusPath, read);
                    }

                    read = TheRunnersNewLines(order.StatusPath, read);
                    log.Line(PackageRunWords.Prefix + " " + PackageRunWords.RunnerEndedWith(process.ExitCode));
                }
            }
            catch (Exception error)
            {
                log.Failure("the run of packages, each in a fresh Navisworks", error, "every package with no result is named in RESULT");
            }
            finally
            {
                JoinTheResults(order, packages, tally);
                running = false;
                SetTheRunButtons(true);
            }
        }

        /// <summary>Every package's result read and the one RESULT written, a package with none named and never dropped.</summary>
        private void JoinTheResults(PackageRunOrder order, IList<Package> packages, PackageTally tally)
        {
            List<PackageRunResult> results = new List<PackageRunResult>();
            List<string> whys = new List<string>();
            List<string> names = new List<string>();

            try
            {
                for (int i = 0; i < packages.Count; i++)
                {
                    string why;
                    PackageRunResult result = PackageRunResult.Read(order.ResultPathFor(i), out why);
                    names.Add(packages[i].RelativePath);
                    results.Add(result);
                    whys.Add(why);

                    if (result == null)
                    {
                        tally.NotRun(packages[i], PackageRunWords.NoResult(packages[i].RelativePath, why));
                        continue;
                    }

                    foreach (PackageRunGroup group in result.Groups)
                    {
                        tally.Add(packages[i], group.Group, group.Outcome, group.Clashes);
                    }

                    if (result.Groups.Count == 0)
                    {
                        tally.NotRun(packages[i], result.NoGroupBecause);
                    }
                }

                log.RunFinished();
                log.Section(PackageRunResult.ResultTitle);

                foreach (string line in PackageRunWords.JoinedResult(names, results, whys, tally.Lines()))
                {
                    log.Line(line);
                }

                SetProgress(PackageRunWords.Finished(packages.Count));
            }
            catch (Exception error)
            {
                log.Failure("joining the results of the packages", error, "each package's own log still holds its RESULT");
            }
        }

        /// <summary>The runner's lines written since the last read, each into the log and the last onto the live line.</summary>
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
                    int end = Array.LastIndexOf(bytes, (byte)'\n', got - 1);

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
            catch (IOException error)
            {
                if (!statusReadFailureSaid)
                {
                    statusReadFailureSaid = true;
                    log.Line(PackageRunWords.Prefix + " " + PackageRunWords.StatusNotRead(error.Message));
                }

                return from;
            }
        }

        private bool statusReadFailureSaid;
    }
}
