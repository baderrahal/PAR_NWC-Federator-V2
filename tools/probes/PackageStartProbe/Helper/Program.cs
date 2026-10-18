using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace PackageStartHelper
{
    /// <summary>
    /// F214, F210-R1, 2026-10-18. Started by the probe plugin's parent mode inside one Navisworks. Starts a second
    /// Navisworks through the Automation API, read by reflection off the install folder it is handed as the loop's
    /// PowerShell runners read it, sets it visible, adds the probe plugin, hands the child mode its arguments by
    /// ExecuteAddInPlugin, quits it by Dispose and waits up to 60 s for the process it started to be gone. Every step is
    /// one line in the output file with its seconds. It closes nothing it did not start and kills nothing.
    ///
    ///     PackageStartHelper.exe navisworksFolder probePluginDll pluginId outFile childArg...
    /// </summary>
    internal static class Program
    {
        private static string output;

        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length < 4)
            {
                return 2;
            }

            output = args[3];
            Stopwatch clock = Stopwatch.StartNew();
            Say("helper started, pid " + Process.GetCurrentProcess().Id + ", parent "
                + ParentOf(Process.GetCurrentProcess().Id) + ", 64 bit " + Environment.Is64BitProcess);

            HashSet<int> before = Roamers();
            Say("Roamers before the start: " + string.Join(", ", Sorted(before)));
            object app = null;
            int child = 0;

            try
            {
                Type type = Assembly.LoadFrom(Path.Combine(args[0], "Autodesk.Navisworks.Automation.dll"))
                    .GetType("Autodesk.Navisworks.Api.Automation.NavisworksApplication", true);
                Stopwatch step = Stopwatch.StartNew();
                app = Activator.CreateInstance(type);
                Say("the constructor returned after " + Seconds(step));

                HashSet<int> after = Roamers();
                after.ExceptWith(before);
                Say("Roamers new since the start: " + string.Join(", ", Sorted(after)));

                if (after.Count == 1)
                {
                    foreach (int id in after)
                    {
                        child = id;
                    }
                }

                type.GetProperty("Visible").SetValue(app, true, null);
                Say("Visible set True, reads " + type.GetProperty("Visible").GetValue(app, null));

                if (args[1].Length > 0)
                {
                    step = Stopwatch.StartNew();
                    type.GetMethod("AddPluginAssembly").Invoke(app, new object[] { args[1] });
                    Say("AddPluginAssembly returned after " + Seconds(step));
                }

                string[] pluginArgs = new string[args.Length - 4];
                Array.Copy(args, 4, pluginArgs, 0, pluginArgs.Length);
                step = Stopwatch.StartNew();
                object returned = type.GetMethod("ExecuteAddInPlugin").Invoke(app, new object[] { args[2], pluginArgs });
                Say("ExecuteAddInPlugin(" + args[2] + ", " + pluginArgs.Length + " arguments) returned " + returned + " after " + Seconds(step));

                step = Stopwatch.StartNew();
                ((IDisposable)app).Dispose();
                app = null;
                Say("Dispose returned after " + Seconds(step));
            }
            catch (Exception error)
            {
                Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                Say("THREW " + inner.GetType().FullName + ": " + inner.Message);
            }

            bool gone = child == 0;

            if (child != 0)
            {
                Stopwatch wait = Stopwatch.StartNew();

                while (wait.Elapsed.TotalSeconds < 60 && !(gone = IsGone(child)))
                {
                    System.Threading.Thread.Sleep(250);
                }

                Say("the Navisworks this helper started, pid " + child + ", " + (gone ? "is gone " + Seconds(wait) + " after Dispose" : "STILL RUNS 60 s after Dispose, left as it is"));
            }
            else
            {
                Say("which Roamer this helper started could not be told, so none is waited on");
            }

            Say("helper finished after " + Seconds(clock));
            return gone && child != 0 ? 0 : 1;
        }

        private static void Say(string line)
        {
            File.AppendAllText(output, DateTime.Now.ToString("HH:mm:ss.fff") + "  helper  " + line + Environment.NewLine, new UTF8Encoding(false));
        }

        private static string Seconds(Stopwatch watch)
        {
            return watch.Elapsed.TotalSeconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s";
        }

        private static HashSet<int> Roamers()
        {
            HashSet<int> ids = new HashSet<int>();

            foreach (Process p in Process.GetProcessesByName("Roamer"))
            {
                ids.Add(p.Id);
                p.Dispose();
            }

            return ids;
        }

        private static List<int> Sorted(HashSet<int> ids)
        {
            List<int> list = new List<int>(ids);
            list.Sort();
            return list;
        }

        private static bool IsGone(int id)
        {
            try
            {
                using (Process p = Process.GetProcessById(id))
                {
                    return p.HasExited;
                }
            }
            catch (ArgumentException)
            {
                return true;
            }
        }

        private static string ParentOf(int id)
        {
            try
            {
                using (System.Management.ManagementObjectSearcher search = new System.Management.ManagementObjectSearcher(
                    "SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + id))
                {
                    foreach (System.Management.ManagementObject row in search.Get())
                    {
                        int parent = Convert.ToInt32(row["ParentProcessId"]);

                        try
                        {
                            using (Process p = Process.GetProcessById(parent))
                            {
                                return parent + " " + p.ProcessName;
                            }
                        }
                        catch (ArgumentException)
                        {
                            return parent + " gone";
                        }
                    }
                }
            }
            catch (Exception error)
            {
                return "UNKNOWN, " + error.GetType().Name;
            }

            return "UNKNOWN";
        }
    }
}
