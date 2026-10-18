using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;

namespace AppendBoxProbe
{
    /// <summary>
    /// F213, F210-R6, 2026-10-18. Measured before anything is built, Bader's message of 18 Oct 2026: TryAppendFile on a
    /// corrupt NWC opened a Navisworks message box in run-20261018-044816 and waited 100.7 s for a person, scan.md 5z-zh.
    /// Which ways exist to keep that box from stopping an unattended run: a check of the file before the append, or the box
    /// answered from inside the process. A Navisworks option is not tried, since Bader's rule of 18 Oct says the tool
    /// changes no setting to append.
    ///
    /// For each NWC copy of one folder, in name order: its length and its first 16 bytes, then the document cleared and
    /// the copy appended by TryAppendFile while a DispatcherTimer on this, Navisworks' main thread, reads the windows of the
    /// process every 200 ms. Each window of the class #32770 it sees is written once with its caption, every child's class
    /// and text, and when it was first seen, then answered by WM_COMMAND IDOK after it has stood 1 s, and by IDCANCEL if it
    /// still stands 1.5 s later. A thread pool timer reads the same every 500 ms and answers only a box that stood 20 s, so
    /// a timer that never fires inside the box cannot hold the probe, and it says so. A copy named with 4-locked at its
    /// start is held open by this probe with no sharing while it is appended. A copy named with 2-random is also appended
    /// once more by AppendFile, the form that may throw. Nothing is saved, published or changed but the cleared document.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Append box probe", ToolTip = "F213, the message box of a corrupt NWC")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class AppendBoxProbePlugin : AddInPlugin
    {
        public const string PluginName = "AppendBoxProbe";
        public const string DeveloperCode = "PARS";

        private const int WmCommand = 0x0111;
        private const int IdOk = 1;
        private const int IdCancel = 2;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;
        private readonly object gate = new object();

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 3 || parameters[0] != "append-box")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode append-box, folder " + parameters[2] + ", pid " + Process.GetCurrentProcess().Id
                    + ", main thread " + Thread.CurrentThread.ManagedThreadId + ", apartment " + Thread.CurrentThread.GetApartmentState());

                try
                {
                    int code = Measure(parameters[2]);
                    Say("probe finished, code " + code);
                    return code;
                }
                catch (Exception error)
                {
                    Exception inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                    Say("THREW " + inner.GetType().Name + ": " + inner.Message);
                    Say(inner.StackTrace ?? string.Empty);
                    return 1;
                }
            }
        }

        private void Say(string line)
        {
            lock (gate)
            {
                results.WriteLine(line);
            }
        }

        private int Measure(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Say("UNKNOWN: the folder of copies is not there");
                return 3;
            }

            string[] files = Directory.GetFiles(folder, "*.nwc");
            Array.Sort(files, StringComparer.Ordinal);
            Say("NWC copies in it: " + files.Length);

            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return 4;
            }

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                Say("");
                Say("==== " + name + " ====");
                Say("length " + new FileInfo(file).Length + " bytes, first 16 bytes " + FirstBytes(file));
                document.Clear();
                Say("cleared, models " + document.Models.Count);

                FileStream held = null;

                if (name.StartsWith("4-locked", StringComparison.Ordinal))
                {
                    held = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                    Say("held open by the probe with FileShare.None for the append");
                }

                try
                {
                    Watched(name, "TryAppendFile", () => document.TryAppendFile(file).ToString(Inv));
                }
                finally
                {
                    if (held != null)
                    {
                        held.Dispose();
                    }
                }

                Say("models after " + document.Models.Count);

                if (name.StartsWith("2-random", StringComparison.Ordinal))
                {
                    document.Clear();
                    Watched(name, "AppendFile", () =>
                    {
                        document.AppendFile(file);
                        return "returned";
                    });
                    Say("models after " + document.Models.Count);
                }
            }

            document.Clear();
            return 0;
        }

        /// <summary>
        /// One call made while the two watchers read the windows of the process, and every box they saw and answered.
        /// </summary>
        private void Watched(string name, string what, Func<string> call)
        {
            Watch watch = new Watch(this, Process.GetCurrentProcess().Id);
            DispatcherTimer dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher.CurrentDispatcher);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);
            dispatcherTimer.Tick += (s, e) => watch.Pass("dispatcher timer on the main thread, thread " + Thread.CurrentThread.ManagedThreadId, 1000, 2500);
            Timer poolTimer = new Timer(_ => watch.Pass("thread pool timer, thread " + Thread.CurrentThread.ManagedThreadId, 20000, 21500), null, 500, 500);
            Stopwatch clock = Stopwatch.StartNew();
            watch.Clock = clock;
            string returned;
            dispatcherTimer.Start();

            try
            {
                returned = call();
            }
            catch (Exception error)
            {
                returned = "THREW " + error.GetType().FullName + ": " + error.Message;
            }
            finally
            {
                dispatcherTimer.Stop();
                poolTimer.Dispose();
                clock.Stop();
            }

            Say(what + " " + name + ": " + returned + " after " + clock.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s, dispatcher timer passes " + watch.DispatcherPasses
                + ", thread pool passes " + watch.PoolPasses + ", boxes seen " + watch.Seen.Count);
        }

        private static string FirstBytes(string file)
        {
            byte[] head = new byte[16];
            int read;

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                read = fs.Read(head, 0, head.Length);
            }

            StringBuilder hex = new StringBuilder();
            StringBuilder text = new StringBuilder();

            for (int i = 0; i < read; i++)
            {
                hex.Append(head[i].ToString("X2", Inv)).Append(' ');
                text.Append(head[i] >= 32 && head[i] < 127 ? (char)head[i] : '.');
            }

            return hex.ToString().TrimEnd() + "  \"" + text + "\"";
        }

        /// <summary>The boxes of one call: each seen once, answered once by OK and once more by Cancel if it stands.</summary>
        private sealed class Watch
        {
            private readonly AppendBoxProbePlugin owner;
            private readonly int pid;
            private readonly Dictionary<IntPtr, long> firstSeen = new Dictionary<IntPtr, long>();
            private readonly HashSet<IntPtr> okSent = new HashSet<IntPtr>();
            private readonly HashSet<IntPtr> cancelSent = new HashSet<IntPtr>();

            public Watch(AppendBoxProbePlugin owner, int pid)
            {
                this.owner = owner;
                this.pid = pid;
                Seen = new List<IntPtr>();
            }

            public Stopwatch Clock { get; set; }

            public List<IntPtr> Seen { get; private set; }

            public int DispatcherPasses;

            public int PoolPasses;

            public void Pass(string who, long okAfterMs, long cancelAfterMs)
            {
                if (who.StartsWith("dispatcher", StringComparison.Ordinal))
                {
                    Interlocked.Increment(ref DispatcherPasses);
                }
                else
                {
                    Interlocked.Increment(ref PoolPasses);
                }

                long now = Clock == null ? 0 : Clock.ElapsedMilliseconds;

                foreach (IntPtr box in BoxesOf(pid))
                {
                    lock (firstSeen)
                    {
                        if (!firstSeen.ContainsKey(box))
                        {
                            firstSeen[box] = now;
                            Seen.Add(box);
                            owner.Say("  BOX seen by the " + who + " at " + now + " ms: " + Describe(box));
                        }

                        long stood = now - firstSeen[box];

                        if (stood >= okAfterMs && !okSent.Contains(box))
                        {
                            okSent.Add(box);
                            bool posted = PostMessage(box, WmCommand, new IntPtr(IdOk), IntPtr.Zero);
                            owner.Say("  BOX answered WM_COMMAND IDOK by the " + who + " at " + now + " ms, after it stood " + stood + " ms, posted " + posted);
                        }
                        else if (stood >= cancelAfterMs && !cancelSent.Contains(box))
                        {
                            cancelSent.Add(box);
                            bool posted = PostMessage(box, WmCommand, new IntPtr(IdCancel), IntPtr.Zero);
                            owner.Say("  BOX STILL THERE, answered WM_COMMAND IDCANCEL by the " + who + " at " + now + " ms, after it stood " + stood + " ms, posted " + posted);
                        }
                    }
                }
            }
        }

        private static List<IntPtr> BoxesOf(int pid)
        {
            List<IntPtr> found = new List<IntPtr>();

            EnumWindows((hwnd, l) =>
            {
                uint owner;
                GetWindowThreadProcessId(hwnd, out owner);

                if (owner == (uint)pid && IsWindowVisible(hwnd) && ClassOf(hwnd) == "#32770")
                {
                    found.Add(hwnd);
                }

                return true;
            }, IntPtr.Zero);

            return found;
        }

        private static string Describe(IntPtr box)
        {
            StringBuilder line = new StringBuilder();
            line.Append("caption \"").Append(TextOf(box)).Append("\"");
            List<string> children = new List<string>();

            EnumChildWindows(box, (child, l) =>
            {
                children.Add(ClassOf(child) + " id " + GetDlgCtrlID(child) + " \"" + TextOf(child).Replace("\r", "\\r").Replace("\n", "\\n") + "\"");
                return true;
            }, IntPtr.Zero);

            line.Append(", children ").Append(children.Count).Append(": ").Append(string.Join(" | ", children.ToArray()));
            return line.ToString();
        }

        private static string ClassOf(IntPtr hwnd)
        {
            StringBuilder name = new StringBuilder(256);
            GetClassName(hwnd, name, name.Capacity);
            return name.ToString();
        }

        private static string TextOf(IntPtr hwnd)
        {
            int length = GetWindowTextLength(hwnd);
            StringBuilder text = new StringBuilder(Math.Max(length + 1, 1));
            GetWindowText(hwnd, text, text.Capacity);
            return text.ToString();
        }

        private delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int capacity);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int capacity);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int GetDlgCtrlID(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool PostMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);
    }
}
