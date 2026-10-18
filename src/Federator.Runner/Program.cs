using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Federator.Core.Communities;

namespace Federator.Runner
{
    /// <summary>
    /// F214, Bader's answer to Q204 of 18 Oct 2026: a fresh Navisworks per package, Run pressed once, the next starting
    /// only after the last closed. Started by the window of a run of packages with the Navisworks install folder and the
    /// order. For each package of the order in turn it starts one Navisworks through the Automation API, read by
    /// reflection off the install folder as scan.md 5z-zj measured, sets it visible, runs the installed add-in there with
    /// the order and the package's index, quits it by Dispose and waits for its process to be gone, ending it past the
    /// order's wait, before the next package starts. A package that throws, crashes or passes the order's ceiling is said
    /// and the next one runs. Every step is one line of the status file, which the window copies into its log. It closes
    /// only the Navisworks it started.
    ///
    ///     Federator.Runner.exe navisworksFolder orderPath
    /// </summary>
    internal static class Program
    {
        private const string AutomationDll = "Autodesk.Navisworks.Automation.dll";
        private const string AutomationType = "Autodesk.Navisworks.Api.Automation.NavisworksApplication";

        private static string status;

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
                return 2;
            }

            status = order.StatusPath;
            Stopwatch clock = Stopwatch.StartNew();
            Say(PackageRunWords.RunnerStarted(order.Packages.Count, order.OrderPath));

            Type type;

            try
            {
                type = Assembly.LoadFrom(Path.Combine(args[0], AutomationDll)).GetType(AutomationType, true);
            }
            catch (Exception error)
            {
                Say(PackageRunWords.Threw("every package", "loading " + AutomationDll + " from " + args[0], error.GetType().Name, error.Message));
                return 1;
            }

            for (int i = 0; i < order.Packages.Count; i++)
            {
                RunOne(type, order, i);
            }

            Say(PackageRunWords.RunnerEnded(order.Packages.Count, clock.Elapsed.TotalSeconds));
            return 0;
        }

        /// <summary>One package in one fresh Navisworks, from its start to its process gone. Never throws.</summary>
        private static void RunOne(Type type, PackageRunOrder order, int index)
        {
            string package = order.Packages[index];
            Say(PackageRunWords.Starting(index, order.Packages.Count, package));
            HashSet<int> before = Roamers();
            object app = null;
            int pid = 0;
            Timer ceiling = null;

            try
            {
                Stopwatch step = Stopwatch.StartNew();
                app = Activator.CreateInstance(type);
                pid = TheOneNew(before);
                Say(PackageRunWords.Started(package, pid, step.Elapsed.TotalSeconds));

                if (pid > 0)
                {
                    int ceilingPid = pid;
                    ceiling = new Timer(
                        delegate
                        {
                            Say(PackageRunWords.Ceiling(package, order.Settings.CeilingMinutes));
                            End(ceilingPid);
                        },
                        null,
                        TimeSpan.FromMinutes(order.Settings.CeilingMinutes),
                        TimeSpan.FromMilliseconds(-1));
                }

                type.GetProperty("Visible").SetValue(app, true, null);
                step = Stopwatch.StartNew();
                object returned = type.GetMethod("ExecuteAddInPlugin").Invoke(
                    app,
                    new object[] { order.PluginId, new[] { PackageRunOrder.ChildMode, order.OrderPath, index.ToString(System.Globalization.CultureInfo.InvariantCulture) } });
                Say(PackageRunWords.Returned(package, returned, step.Elapsed.TotalSeconds));
            }
            catch (Exception error)
            {
                Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                Say(PackageRunWords.Threw(package, "its run", inner.GetType().Name, inner.Message));
            }
            finally
            {
                if (ceiling != null)
                {
                    ceiling.Dispose();
                }

                Quit(app, package);
                WaitGone(pid, package, order.Settings.GoneWithinSeconds);
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

        private static void WaitGone(int pid, string package, int seconds)
        {
            if (pid <= 0)
            {
                Say(PackageRunWords.NotTold(package));
                return;
            }

            Stopwatch wait = Stopwatch.StartNew();

            while (wait.Elapsed.TotalSeconds < seconds)
            {
                if (IsGone(pid))
                {
                    Say(PackageRunWords.Gone(package, wait.Elapsed.TotalSeconds));
                    return;
                }

                Thread.Sleep(250);
            }

            Say(PackageRunWords.Ended(package, seconds));
            End(pid);
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

        private static readonly object Gate = new object();

        private static int linesLost;

        /// <summary>
        /// One line of the status file, written whole and never stopping the run. The window reads the file while it
        /// grows, so a write it holds up is tried again five times 50 ms apart, and a line still not written is counted
        /// and the count carried on the next line that is.
        /// </summary>
        private static void Say(string line)
        {
            lock (Gate)
            {
                string text = DateTime.Now.ToString("HH:mm:ss.fff") + "  " + line
                    + (linesLost > 0 ? PackageRunWords.LinesLost(linesLost) : string.Empty) + Environment.NewLine;

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        File.AppendAllText(status, text, new UTF8Encoding(false));
                        linesLost = 0;
                        return;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Thread.Sleep(50);
                    }
                }

                linesLost++;
            }
        }
    }
}
