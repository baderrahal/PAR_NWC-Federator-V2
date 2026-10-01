using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Federator.Core.Diagnostics;

namespace NwcFederatorLoop.StandIn
{
    /// <summary>
    /// A stand-in named Roamer.exe for the proof harness, F103. It takes its role from its
    /// arguments, or from NWCLOOP_STANDIN, its words split by a bar, when its only argument is
    /// -Embedding, so it can be started with the command line a COM start gives. Every role
    /// ends by itself, so a harness that dies leaves nothing running for longer than the role
    /// said.
    /// </summary>
    internal static class Program
    {
        private const int WmGetText = 0x000D;
        private const int WmClose = 0x0010;

        [STAThread]
        private static int Main(string[] args)
        {
            // -Embedding is the option a COM start gives, and a word starting --note only shapes
            // the command line a guard reads, so neither is part of the role.
            List<string> words = new List<string>();
            foreach (string a in args)
            {
                if (!string.Equals(a, "-Embedding", StringComparison.OrdinalIgnoreCase)
                    && !a.StartsWith("--note", StringComparison.OrdinalIgnoreCase))
                {
                    words.Add(a);
                }
            }

            if (words.Count == 0)
            {
                string fromEnvironment = Environment.GetEnvironmentVariable("NWCLOOP_STANDIN");
                if (!string.IsNullOrEmpty(fromEnvironment))
                {
                    words.AddRange(fromEnvironment.Split(new[] { '|' }, StringSplitOptions.None));
                }
            }

            if (words.Count == 0)
            {
                words.Add("sleep");
                words.Add("300");
            }

            string role = words[0];

            switch (role)
            {
                case "sleep":
                    Thread.Sleep(Seconds(words, 1) * 1000);
                    return 0;
                case "spin":
                    Spin(Seconds(words, 1));
                    return 0;
                case "runlog":
                    return WriteLog(words[1], int.Parse(words[2], CultureInfo.InvariantCulture), Seconds(words, 3), Seconds(words, 4));
                case "prune":
                    return Prune(words[1], words[2]);
                case "dialogs":
                    return Dialogs(words[1], words[2], Seconds(words, 3));
                case "decoy":
                    return Decoy(words[1], words[2], Seconds(words, 3));
                case "hang":
                    return Hang(words[1], Seconds(words, 2), words[3], Seconds(words, 4));
                case "owned":
                    return Owned(words[1], words[2], Seconds(words, 3));
                default:
                    return 64;
            }
        }

        private static int Seconds(List<string> words, int index)
        {
            return int.Parse(words[index], CultureInfo.InvariantCulture);
        }

        private static void Spin(int seconds)
        {
            Stopwatch watch = Stopwatch.StartNew();
            long sum = 0;
            while (watch.Elapsed.TotalSeconds < seconds)
            {
                sum += watch.ElapsedTicks % 7;
            }

            GC.KeepAlive(sum);
        }

        /// <summary>
        /// Opens a log in the folder with the tool's own RunLog, which prunes the folder back to
        /// thirty, writes a line every few milliseconds for a while, then goes quiet and sleeps.
        /// </summary>
        private static int WriteLog(string folder, int everyMilliseconds, int forSeconds, int thenSleepSeconds)
        {
            using (RunLog log = RunLog.Start(folder, DateTime.Now, 30))
            {
                Stopwatch watch = Stopwatch.StartNew();
                int n = 0;
                while (watch.Elapsed.TotalSeconds < forSeconds)
                {
                    n++;
                    log.Line("STANDIN tick " + n.ToString(CultureInfo.InvariantCulture));
                    Thread.Sleep(everyMilliseconds);
                }

                log.Line("RESULT   the stand-in wrote " + n.ToString(CultureInfo.InvariantCulture) + " ticks");
                log.Line("COPY     nothing, the stand-in copies nothing");
                Thread.Sleep(thenSleepSeconds * 1000);
            }

            return 0;
        }

        /// <summary>
        /// M1. One RunLog.Start against the folder, then the log is closed and its path written
        /// to the result file, so the harness reads the RETAIN line the real prune wrote.
        /// </summary>
        private static int Prune(string folder, string resultFile)
        {
            string path;
            using (RunLog log = RunLog.Start(folder, DateTime.Now, 30))
            {
                path = log.Path;
            }

            File.WriteAllText(resultFile, path + Environment.NewLine, new UTF8Encoding(false));
            return 0;
        }

        /// <summary>
        /// A message box and a WinForms dialog with a label, both up at once, for the monitor
        /// to read as dialogs of the adopted process.
        /// </summary>
        private static int Dialogs(string messageText, string labelText, int seconds)
        {
            ExitAfter(seconds);
            Thread box = new Thread(() => MessageBox.Show(messageText, "NwcFederatorLoop test message"));
            box.SetApartmentState(ApartmentState.STA);
            box.IsBackground = true;
            box.Start();

            Form form = new Form();
            form.Text = "NwcFederatorLoop test form";
            form.Width = 420;
            form.Height = 140;
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Left = 12;
            label.Top = 12;
            form.Controls.Add(label);
            Application.Run(form);
            return 0;
        }

        /// <summary>
        /// A decoy started by hand. Its form and its label log every WM_GETTEXT and WM_CLOSE they
        /// receive, with the time, so the harness can show nothing of the loop's reached it.
        /// </summary>
        private static int Decoy(string logFile, string caption, int seconds)
        {
            ExitAfter(seconds);
            File.WriteAllText(logFile, string.Empty, new UTF8Encoding(false));
            LoggingForm form = new LoggingForm(logFile);
            form.Text = caption;
            form.Width = 420;
            form.Height = 140;
            LoggingLabel label = new LoggingLabel(logFile);
            label.Text = "the decoy's label";
            label.AutoSize = true;
            label.Left = 12;
            label.Top = 12;
            form.Controls.Add(label);
            Application.Run(form);
            return 0;
        }

        /// <summary>
        /// A visible window with a number of labels whose thread, once the window is shown,
        /// writes the ready file and then blocks for the seconds given, answering no message.
        /// The process ends five seconds after the block ends.
        /// </summary>
        private static int Hang(string readyFile, int seconds, string caption, int children)
        {
            ExitAfter(seconds + 5);
            Form form = new Form();
            form.Text = caption;
            form.Width = 420;
            form.Height = 80 + (children * 20);
            for (int i = 0; i < children; i++)
            {
                Label label = new Label();
                label.Text = "a label of the blocked window, number " + i.ToString(CultureInfo.InvariantCulture);
                label.AutoSize = true;
                label.Left = 12;
                label.Top = 8 + (i * 20);
                form.Controls.Add(label);
            }

            form.Shown += (sender, e) =>
            {
                Application.DoEvents();
                File.WriteAllText(readyFile, "blocked" + Environment.NewLine, new UTF8Encoding(false));
                Thread.Sleep(seconds * 1000);
            };
            Application.Run(form);
            return 0;
        }

        /// <summary>
        /// Three WinForms windows with the same caption, the three shapes the main window rule
        /// reads: one with no owner, one owned by a window that is not visible, the shape the
        /// second real start measured at 13:39:20 on 2026-09-30, record steps\runs\01\item0
        /// line 33, the main window's owner of class WindowsForms10.Window.0.app.0.27a2811_r7_ad1,
        /// caption "", visible False, enabled True, in the adopted process, and one owned by the
        /// visible first window and carrying a label, the shape a message box of Navisworks
        /// owned by its main window would have, which no run has shown.
        /// </summary>
        private static int Owned(string caption, string labelText, int seconds)
        {
            ExitAfter(seconds);
            Form main = new Form();
            main.Text = caption;
            main.Width = 420;
            main.Height = 140;
            Form parked = new Form();
            parked.Text = "NwcFederatorLoop parked owner";
            IntPtr parkedHandle = parked.Handle;
            GC.KeepAlive(parkedHandle);
            Form hiddenOwned = new Form();
            hiddenOwned.Text = caption;
            hiddenOwned.Width = 380;
            hiddenOwned.Height = 130;
            Form owned = new Form();
            owned.Text = caption;
            owned.Width = 360;
            owned.Height = 120;
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Left = 12;
            label.Top = 12;
            owned.Controls.Add(label);
            main.Shown += (sender, e) =>
            {
                hiddenOwned.Owner = parked;
                hiddenOwned.Show();
                owned.Owner = main;
                owned.Show();
            };
            Application.Run(main);
            return 0;
        }

        private static void ExitAfter(int seconds)
        {
            Thread timer = new Thread(() =>
            {
                Thread.Sleep(seconds * 1000);
                Environment.Exit(0);
            });
            timer.IsBackground = true;
            timer.Start();
        }

        [DllImport("user32.dll")]
        private static extern uint InSendMessageEx(IntPtr reserved);

        /// <summary>
        /// Only a message sent from another thread or another process is written, read off
        /// InSendMessageEx, because WinForms reads its own window text through WM_GETTEXT.
        /// </summary>
        private static void Note(string logFile, string who, int message)
        {
            uint sent = InSendMessageEx(IntPtr.Zero);
            if (sent == 0)
            {
                return;
            }

            string name = message == WmGetText ? "WM_GETTEXT" : "WM_CLOSE";
            File.AppendAllText(logFile, DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " " + who + " " + name + " sent from another thread or process, flags " + sent.ToString(CultureInfo.InvariantCulture) + Environment.NewLine);
        }

        private sealed class LoggingForm : Form
        {
            private readonly string logFile;

            public LoggingForm(string logFile)
            {
                this.logFile = logFile;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmGetText || m.Msg == WmClose)
                {
                    Note(logFile, "form", m.Msg);
                }

                base.WndProc(ref m);
            }
        }

        private sealed class LoggingLabel : Label
        {
            private readonly string logFile;

            public LoggingLabel(string logFile)
            {
                this.logFile = logFile;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmGetText || m.Msg == WmClose)
                {
                    Note(logFile, "label", m.Msg);
                }

                base.WndProc(ref m);
            }
        }
    }
}
