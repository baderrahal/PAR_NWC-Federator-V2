using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;

namespace Federator.Runner
{
    /// <summary>
    /// F214, Bader's answer to Q204 of 18 Oct 2026, and F222, his answer to Q211: a fresh Navisworks per group, Run pressed
    /// once, the next starting only after the last closed. Started by the window of a run of packages with the Navisworks
    /// install folder and the order. For each group of the order in turn it starts one Navisworks through the Automation
    /// API, read by reflection off the install folder as scan.md 5z-zj measured, takes the one Roamer new since before as
    /// its own, sets it visible, runs the installed add-in there with the order and the group's index, quits it by Dispose
    /// and waits for its process to be gone, ending it past the order's wait, before the next group starts. The order's
    /// ceiling runs from before the start to after the wait, so a start, a run or a quit that hangs is ended too. A group
    /// whose Navisworks cannot be told from another is not run, since the runner never ends what it cannot tell is its
    /// own. A group that throws, crashes or passes the ceiling is said and the next one runs. Every step is one line of the
    /// status file, which the window copies into its log, each start and each quit with its seconds and the last line with
    /// both added up. F223, Bader's answer to Q207: Auto-Save is switched off in the registry before every group's start and put back
    /// once as the run ends to the value it held before the run, kept on Core's record, AutoSaveKeeper, until it reads as it was.
    ///
    ///     Federator.Runner.exe navisworksFolder orderPath
    /// </summary>
    internal static class Program
    {
        private const string AutomationDll = "Autodesk.Navisworks.Automation.dll";
        private const string AutomationType = "Autodesk.Navisworks.Api.Automation.NavisworksApplication";

        /// <summary>How many times a status line the window holds up is tried, PackageRunSettings' poll apart. Not a run's number.</summary>
        private const int StatusAttempts = 5;

        private static readonly object Gate = new object();
        private static string status;
        private static int linesLost;
        private static int poll = PackageRunSettings.DefaultPollMilliseconds;

        /// <summary>The starts told and their seconds added up, and the quits seen through to a process gone, F222.</summary>
        private static int starts;
        private static double startSeconds;
        private static int quits;
        private static double quitSeconds;

        /// <summary>The Navisworks the runner had to end, still there past the wait after they were told to quit.</summary>
        private static int ended;

        /// <summary>The registry's Auto-Save switch across the run, F223, made once the order and the Automation DLL read.</summary>
        private static AutoSaveKeeper autoSave;

        /// <summary>The Navisworks one group runs in, read by the ceiling's timer on its own thread.</summary>
        private sealed class Started
        {
            private int pid;

            public int Pid
            {
                get { return Volatile.Read(ref pid); }
                set { Volatile.Write(ref pid, value); }
            }
        }

        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                return 2;
            }

            string why;
            PackageRunOrder order = PackageRunOrder.Read(args[1], out why);

            if (order == null)
            {
                status = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[1])), PackageRunOrder.StatusFileName);
                Say(PackageRunWords.RunnerRefused(why));
                return 2;
            }

            status = order.StatusPath;
            poll = order.Settings.PollMilliseconds;
            Stopwatch clock = Stopwatch.StartNew();
            Say(PackageRunWords.RunnerStarted(order.Units.Count, order.PackageCount, order.OrderPath));

            Type type;

            try
            {
                type = Assembly.LoadFrom(Path.Combine(args[0], AutomationDll)).GetType(AutomationType, true);
            }
            catch (Exception error)
            {
                Say(PackageRunWords.RunnerRefused("loading " + AutomationDll + " from " + args[0] + " threw "
                    + error.GetType().Name + ": " + error.Message));
                return 1;
            }

            // F223, Q207. The value the switch held before the run, kept on its record, and put back once as the run ends,
            // with a Navisworks of the run still running said, so the record stays for the window to read again.
            HashSet<int> atTheStart = RoamersOrNull();
            autoSave = AutoSaveKeeper.Begin(new WindowsAutoSaveRegistry(), new FileAutoSaveRecord(AutoSaveKeeper.RecordPath()), Say);

            try
            {
                for (int i = 0; i < order.Units.Count; i++)
                {
                    try
                    {
                        RunOne(type, order, i);
                    }
                    catch (Exception error)
                    {
                        Say(PackageRunWords.Threw(order.Units[i].Label, "the runner", error.GetType().Name, error.Message));
                    }
                }
            }
            finally
            {
                autoSave.PutBack(StillRuns(atTheStart));
            }

            Say(PackageRunWords.RunnerEnded(order.Units.Count, order.PackageCount, clock.Elapsed.TotalSeconds,
                starts, startSeconds, quits, quitSeconds, ended));
            return 0;
        }

        /// <summary>The Navisworks running now, or null where they could not be read.</summary>
        private static HashSet<int> RoamersOrNull()
        {
            try
            {
                return Roamers();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Whether a Navisworks the run started may still run, true where either reading could not be taken.</summary>
        private static bool StillRuns(HashSet<int> atTheStart)
        {
            HashSet<int> now = RoamersOrNull();

            if (atTheStart == null || now == null)
            {
                return true;
            }

            now.ExceptWith(atTheStart);
            return now.Count > 0;
        }

        /// <summary>One group in one fresh Navisworks, from before its start to its process gone. A throw before its try, the read of the Navisworks running, goes to Main, which says it and runs the next.</summary>
        private static void RunOne(Type type, PackageRunOrder order, int index)
        {
            string package = order.Units[index].Label;
            Say(PackageRunWords.Starting(index, order.Units.Count, package));
            HashSet<int> before = Roamers();
            Started started = new Started();
            object app = null;

            Timer ceiling = new Timer(
                delegate
                {
                    // On a thread of the pool, where a throw would end the runner before its put back.
                    try
                    {
                        int pid = started.Pid > 0 ? started.Pid : TheOneNew(before);

                        if (pid > 0)
                        {
                            started.Pid = pid;
                            Say(PackageRunWords.Ceiling(package, order.Settings.CeilingMinutes));
                            End(pid);
                        }
                        else
                        {
                            Say(PackageRunWords.CeilingNotTold(package, order.Settings.CeilingMinutes));
                        }
                    }
                    catch (Exception error)
                    {
                        Say(PackageRunWords.Threw(package, "its ceiling", error.GetType().Name, error.Message));
                    }
                },
                null,
                TimeSpan.FromMinutes(order.Settings.CeilingMinutes),
                TimeSpan.FromMilliseconds(-1));

            try
            {
                // F223, Q207. The registry's switch off before this group's Navisworks starts and reads it.
                autoSave.SwitchOffFor(package);
                Stopwatch step = Stopwatch.StartNew();
                app = Activator.CreateInstance(type);
                started.Pid = TheOneNew(before);

                // F222. A start is counted once its process is told, so a start that cannot be told adds no seconds.
                if (started.Pid > 0)
                {
                    starts++;
                    startSeconds += step.Elapsed.TotalSeconds;
                }

                Say(PackageRunWords.Started(package, started.Pid, step.Elapsed.TotalSeconds));

                if (started.Pid <= 0)
                {
                    Say(PackageRunWords.NotTold(package));
                    return;
                }

                type.GetProperty("Visible").SetValue(app, true, null);
                step = Stopwatch.StartNew();
                object returned = type.GetMethod("ExecuteAddInPlugin").Invoke(
                    app,
                    new object[] { order.PluginId, new[] { PackageRunOrder.ChildMode, order.OrderPath, index.ToString(CultureInfo.InvariantCulture) } });
                Say(PackageRunWords.Returned(package, returned, step.Elapsed.TotalSeconds));
            }
            catch (Exception error)
            {
                Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                Say(PackageRunWords.Threw(package, "its run", inner.GetType().Name, inner.Message));
            }
            finally
            {
                // A start that threw may still have brought a Navisworks up, which is found here and waited on as well.
                if (started.Pid <= 0 && app == null)
                {
                    started.Pid = TheOneNew(before);
                }

                // F222. The quit is timed from the call that tells it to quit to its process gone.
                Stopwatch quit = Stopwatch.StartNew();
                Quit(app, package);

                if (started.Pid > 0)
                {
                    WaitGone(started.Pid, package, order.Settings.GoneWithinSeconds, app == null ? null : quit);
                }
                else
                {
                    WaitAllNewGone(before, package, order.Settings.GoneWithinSeconds);
                }

                // F223. What the switch reads once its Navisworks is gone, which that Navisworks may have written as it quit.
                autoSave.ReadAfter(package);

                // Only now, so a quit or a wait that hangs still meets the ceiling.
                ceiling.Dispose();
            }
        }

        private static void Quit(object app, string package)
        {
            if (app == null)
            {
                return;
            }

            try
            {
                ((IDisposable)app).Dispose();
            }
            catch (Exception error)
            {
                Say(PackageRunWords.Threw(package, "telling its Navisworks to quit", error.GetType().Name, error.Message));
            }
        }

        private static void WaitGone(int pid, string package, int seconds, Stopwatch told)
        {
            Stopwatch wait = Stopwatch.StartNew();

            while (wait.Elapsed.TotalSeconds < seconds)
            {
                if (IsGone(pid))
                {
                    // A Navisworks whose start threw was never told to quit, so it is waited on and not counted as a quit.
                    if (told != null)
                    {
                        quits++;
                        quitSeconds += told.Elapsed.TotalSeconds;
                    }

                    Say(PackageRunWords.Gone(package, (told ?? wait).Elapsed.TotalSeconds));
                    return;
                }

                Thread.Sleep(poll);
            }

            ended++;
            Say(PackageRunWords.Ended(package, seconds));
            End(pid);
        }

        /// <summary>Where no Navisworks could be told as the group's own, waits for every one new since its start and ends none.</summary>
        private static void WaitAllNewGone(HashSet<int> before, string package, int seconds)
        {
            Stopwatch wait = Stopwatch.StartNew();

            while (wait.Elapsed.TotalSeconds < seconds)
            {
                HashSet<int> now = Roamers();
                now.ExceptWith(before);

                if (now.Count == 0)
                {
                    Say(PackageRunWords.NotToldGone(package, wait.Elapsed.TotalSeconds));
                    return;
                }

                Thread.Sleep(poll);
            }

            Say(PackageRunWords.NotToldStillThere(package, seconds));
        }

        /// <summary>The one Roamer new since the set before, or nought where none or more than one is, so nothing else is ever ended.</summary>
        private static int TheOneNew(HashSet<int> before)
        {
            HashSet<int> after = Roamers();
            after.ExceptWith(before);

            if (after.Count != 1)
            {
                return 0;
            }

            foreach (int id in after)
            {
                return id;
            }

            return 0;
        }

        private static void End(int pid)
        {
            try
            {
                using (Process process = Process.GetProcessById(pid))
                {
                    if (string.Equals(process.ProcessName, "Roamer", StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                    }
                }
            }
            catch (ArgumentException)
            {
                // Gone already, which is what ending it was for.
            }
            catch (Exception error)
            {
                Say(PackageRunWords.NotEnded(pid, error.GetType().Name, error.Message));
            }
        }

        private static HashSet<int> Roamers()
        {
            HashSet<int> ids = new HashSet<int>();

            foreach (Process process in Process.GetProcessesByName("Roamer"))
            {
                ids.Add(process.Id);
                process.Dispose();
            }

            return ids;
        }

        private static bool IsGone(int pid)
        {
            try
            {
                using (Process process = Process.GetProcessById(pid))
                {
                    return process.HasExited;
                }
            }
            catch (ArgumentException)
            {
                return true;
            }
        }

        /// <summary>
        /// One line of the status file, written whole and never stopping the run. The window reads the file while it
        /// grows, so a write it holds up is tried again a poll apart, and a line still not written is counted and the count
        /// carried on the next line that is.
        /// </summary>
        private static void Say(string line)
        {
            lock (Gate)
            {
                string text = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  " + line
                    + (linesLost > 0 ? PackageRunWords.LinesLost(linesLost) : string.Empty) + Environment.NewLine;

                for (int attempt = 0; attempt < StatusAttempts; attempt++)
                {
                    try
                    {
                        File.AppendAllText(status, text, new UTF8Encoding(false));
                        linesLost = 0;
                        return;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(poll);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Thread.Sleep(poll);
                    }
                }

                linesLost++;
            }
        }
    }
}
