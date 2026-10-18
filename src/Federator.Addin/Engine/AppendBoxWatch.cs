using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Watches the windows of this process while one file is appended, F213, F210-R6, and answers the message box a bad
    /// NWC opens inside TryAppendFile, so an unattended run goes on. Measured first, scan.md 5z-zi: a DispatcherTimer on
    /// Navisworks' main thread fires inside the box, and WM_COMMAND with the id of the box's one button closes it the way
    /// a click on OK does, after which the append returns false. Which window is a box, which box is answered and with
    /// what are Core's, AppendBox, handed the class and the children read here, and no other window is touched. A box of
    /// the file is pressed again on every tick while it stands, and said once where it still stands after ten presses.
    /// Nothing here starts a thread and nothing here calls Navisworks: the timer runs on the thread the run is on, and
    /// the only calls are Windows' own, reading windows and posting one message. No setting is changed, Bader's rule of
    /// 18 Oct 2026 that the tool changes no setting to append.
    /// </summary>
    internal sealed class AppendBoxWatch : IDisposable
    {
        private const int WmCommand = 0x0111;
        private const int PressesBeforeSaying = 10;

        /// <summary>
        /// The boxes left alone this session, each said once, since a box that is not the file's, the window's own
        /// warning among them, would otherwise be said again by the watch of every later file.
        /// </summary>
        private static readonly HashSet<IntPtr> LeftAlone = new HashSet<IntPtr>();

        private readonly string file;
        private readonly RunLog log;
        private readonly uint processId;
        private readonly DispatcherTimer timer;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly Dictionary<IntPtr, int> presses = new Dictionary<IntPtr, int>();
        private bool readFailureSaid;

        public AppendBoxWatch(string file, int everyMilliseconds, RunLog log)
        {
            this.file = file;
            this.log = log;

            using (Process self = Process.GetCurrentProcess())
            {
                processId = (uint)self.Id;
            }

            timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher.CurrentDispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(everyMilliseconds);
            timer.Tick += OnTick;
            timer.Start();
        }

        /// <summary>What Navisworks said is wrong with the file, read off the box this watch pressed, or null where none was.</summary>
        public string AnsweredReason { get; private set; }

        public void Dispose()
        {
            timer.Stop();
            timer.Tick -= OnTick;
            clock.Stop();
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                foreach (IntPtr box in BoxesOfThisProcess())
                {
                    List<BoxPart> parts = PartsOf(box);

                    if (!AppendBox.IsTheBoxOf(file, ClassOf(box), parts))
                    {
                        if (LeftAlone.Add(box))
                        {
                            Say(AppendBox.LeftAloneLine(file, TextsOf(parts)));
                        }

                        continue;
                    }

                    Press(box, parts);
                }
            }
            catch (Exception error)
            {
                // The watch never stops a run: a read of the windows that threw is said once and the next tick reads again.
                if (!readFailureSaid)
                {
                    readFailureSaid = true;
                    Say("APPEND   the windows could not be read while " + file + " was appended, " + error.GetType().Name + ": "
                        + error.Message + ". The watch reads them again on every tick and says no more of it");
                }
            }
        }

        /// <summary>
        /// Presses the one button of the file's box, and marks it pressed only once the press was posted, so a box whose
        /// read or post failed is tried again on the next tick.
        /// </summary>
        private void Press(IntPtr box, List<BoxPart> parts)
        {
            int? button = AppendBox.ButtonToAnswer(parts);

            if (button == null || !PostMessage(box, WmCommand, new IntPtr(button.Value), IntPtr.Zero))
            {
                if (!presses.ContainsKey(box))
                {
                    presses[box] = 0;
                    Say("APPEND   the message box on " + file + " could not be answered, its button id "
                        + (button == null ? "UNKNOWN" : button.Value.ToString()) + ", the watch tries again on every tick");
                }

                return;
            }

            int pressed;
            presses.TryGetValue(box, out pressed);
            pressed++;
            presses[box] = pressed;

            if (AnsweredReason == null)
            {
                AnsweredReason = AppendBox.Reason(file, parts);
                Say(AppendBox.AnsweredLine(file, button.Value, AnsweredReason, clock.ElapsedMilliseconds));
            }

            if (pressed == PressesBeforeSaying)
            {
                Say(AppendBox.StillStandsLine(file, pressed));
            }
        }

        private void Say(string line)
        {
            if (log != null)
            {
                log.Line(line);
            }
        }

        /// <summary>The visible top level windows of this process that Core calls a message box, by the class read off each.</summary>
        private List<IntPtr> BoxesOfThisProcess()
        {
            List<IntPtr> found = new List<IntPtr>();

            EnumWindows((hwnd, l) =>
            {
                uint owner;
                GetWindowThreadProcessId(hwnd, out owner);

                if (owner == processId && IsWindowVisible(hwnd) && AppendBox.IsAMessageBox(ClassOf(hwnd)))
                {
                    found.Add(hwnd);
                }

                return true;
            }, IntPtr.Zero);

            return found;
        }

        private static List<BoxPart> PartsOf(IntPtr box)
        {
            List<BoxPart> parts = new List<BoxPart>();

            EnumChildWindows(box, (child, l) =>
            {
                parts.Add(new BoxPart(ClassOf(child), GetDlgCtrlID(child), TextOf(child)));
                return true;
            }, IntPtr.Zero);

            return parts;
        }

        private static string TextsOf(IList<BoxPart> parts)
        {
            List<string> texts = new List<string>();

            foreach (BoxPart part in parts)
            {
                if (part.Text.Length > 0)
                {
                    texts.Add(part.Text);
                }
            }

            return string.Join(" | ", texts.ToArray());
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
