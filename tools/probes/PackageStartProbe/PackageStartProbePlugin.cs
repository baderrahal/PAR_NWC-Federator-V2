using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Threading;
using Autodesk.Navisworks.Api.Plugins;

namespace PackageStartProbe
{
    /// <summary>
    /// F214, F210-R1, 2026-10-18. Measured before anything is built, Bader's answer to Q204 of 18 Oct 2026: a fresh
    /// Navisworks start per package, Run pressed once. How a plugin inside one Navisworks can start another Navisworks
    /// for one package, hand it which package to run, and know it closed before the next.
    ///
    /// Mode parent, inside the Navisworks the runner started: starts tools\probes\PackageStartProbe\Helper's
    /// PackageStartHelper.exe with the install folder, this probe's dll and the child's arguments, and waits for it to
    /// end while it keeps this Navisworks answering, the dispatcher pumped every 200 ms, then writes how long and with
    /// what code. Mode child, inside the Navisworks the helper started: writes its arguments, its process id and its
    /// parent, and returns 0. Nothing is opened, saved or changed in either.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Package start probe", ToolTip = "F214, a fresh Navisworks per package")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class PackageStartProbePlugin : AddInPlugin
    {
        public const string PluginName = "PackageStartProbe";
        public const string DeveloperCode = "PARS";

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                return 2;
            }

            string output = parameters[1];

            try
            {
                if (parameters[0] == "child")
                {
                    Say(output, "child", "started in pid " + Process.GetCurrentProcess().Id + ", handed " + parameters.Length + " arguments: "
                        + string.Join(" | ", parameters));
                    Say(output, "child", "the package it was handed, its third argument: " + (parameters.Length > 2 ? parameters[2] : "none"));
                    return 0;
                }

                if (parameters[0] == "parent" && parameters.Length >= 5)
                {
                    return Parent(output, parameters[2], parameters[3], parameters[4]);
                }

                return 2;
            }
            catch (Exception error)
            {
                Say(output, parameters[0], "THREW " + error.GetType().FullName + ": " + error.Message);
                return 1;
            }
        }

        private static int Parent(string output, string helper, string navisworks, string probeDll)
        {
            Say(output, "parent", "started in pid " + Process.GetCurrentProcess().Id + ", starting " + helper);
            ProcessStartInfo start = new ProcessStartInfo(helper)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = Quoted(navisworks) + " " + Quoted(probeDll) + " PackageStartProbe.PARS " + Quoted(output)
                    + " child " + Quoted(output) + " " + Quoted("the package C06 itself, order 1 of 2")
            };

            Stopwatch clock = Stopwatch.StartNew();
            int pumps = 0;

            using (Process process = Process.Start(start))
            {
                Say(output, "parent", "the helper runs as pid " + process.Id);

                while (!process.WaitForExit(200))
                {
                    // Keeps this Navisworks answering while the other one runs, the way the window's run pumps.
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                    pumps++;
                }

                Say(output, "parent", "the helper ended with code " + process.ExitCode + " after "
                    + clock.Elapsed.TotalSeconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s, the dispatcher pumped "
                    + pumps + " times meanwhile");
                return process.ExitCode;
            }
        }

        private static string Quoted(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static void Say(string output, string who, string line)
        {
            File.AppendAllText(output, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + who + "  " + line + Environment.NewLine, new UTF8Encoding(false));
        }
    }
}
