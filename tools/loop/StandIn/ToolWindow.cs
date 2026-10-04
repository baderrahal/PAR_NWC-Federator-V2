using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Threading;
using Federator.Core.Diagnostics;

namespace NwcFederatorLoop.StandIn
{
    /// <summary>
    /// The window role, F106. A small WPF window carrying the x:Name values of the tool's window,
    /// src\Federator.Addin\Ui\FederatorWindow.xaml, its four tab headers and a caption that names a
    /// build stamp, so tools\probes\drive-window-run.ps1 and run.ps1's monitor can be proved on it
    /// with no Navisworks. It is not the tool. What it does with a press is the least the proof
    /// needs, and every press, every answer to its confirm and every WM_CLOSE it receives is
    /// written to its events file. With a logs folder it writes its log through the tool's own
    /// RunLog, in the shapes the window's run writes, so the monitor reads the real format.
    /// The mode names the one thing that differs from the plain window:
    ///   plain         Scan, then Run shows the confirm, and OK writes a run into the log
    ///   badbox        NwfFolderBox adds a ~ to whatever is typed into it
    ///   outside       the confirm names a path outside the loop folder
    ///   tolerance     ToleranceBox opens on its second entry
    ///   dialog        Run shows a warning that is not the confirm
    ///   open-nwd      the open file line is the tool's refusal of an NWD, its button disabled
    ///   open-nwf      the open file line names a file beside the events file, its button enabled
    ///   open-outside  the open file line names a path outside the loop folder, button enabled
    ///   selfclose     the window closes itself six seconds after it is shown
    ///   autorun       a run starts by itself when the window is shown, and its RESULT block is
    ///                 written 25 seconds later
    /// F125 added three modes, each the shape the baseline run of 2026-10-04 read, UnderPane below:
    ///   pane          a main window, a floating pane owned by it, then this window over the main
    ///                 window with ShowDialog, and after that as plain
    ///   pane-dialog   as pane, and Run shows a warning that is not the confirm
    ///   pane-new      as pane, and Run shows a new WinForms window owned by this one, not modal,
    ///                 and no confirm
    /// </summary>
    internal static class ToolWindow
    {
        private const int WmClose = 0x0010;

        public sealed class FileLine
        {
            public string FileName { get; set; }
        }

        public sealed class GroupLine
        {
            public bool Include { get; set; }

            public string Building { get; set; }
        }

        /// <summary>F125. A WPF window's handle handed to WinForms as the owner of a form.</summary>
        private sealed class OwnerHandle : System.Windows.Forms.IWin32Window
        {
            public OwnerHandle(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; private set; }
        }

        public static int Run(string eventsFile, string stamp, string mode, string logsFolder)
        {
            File.WriteAllText(eventsFile, string.Empty, new UTF8Encoding(false));
            string version = "1.0.0.0 " + stamp + " built 2026-10-01 00:00:00";
            RunLog log = null;

            if (logsFolder != "-")
            {
                log = RunLog.Start(logsFolder, DateTime.Now, 30);
                log.Session(version, "the stand-in, not Navisworks", "none");
                log.Block("FOLDERS REMEMBERED", new[]
                {
                    "Source    C:\\NwcFederatorLoop stand-in remembered\\source",
                    "ClashXml  D:\\NwcFederatorLoop stand-in remembered\\xml   [gone, opening at nowhere]",
                });
            }

            Window window = new Window();
            window.Title = "Parsons NWC Federator   [" + version + "]";
            window.Width = 900;
            window.Height = 560;

            TextBox source = new TextBox { Name = "SourceFolderBox" };
            CheckBox subfolders = new CheckBox { Name = "IncludeSubfolders", Content = "Include subfolders", IsChecked = true };
            Button scan = new Button { Content = "Scan" };
            TextBlock summary = new TextBlock { Name = "SourceSummary" };
            ObservableCollection<FileLine> files = new ObservableCollection<FileLine>();
            DataGrid filesGrid = new DataGrid { Name = "FilesGrid", AutoGenerateColumns = false, CanUserAddRows = false, ItemsSource = files };
            filesGrid.Columns.Add(new DataGridTextColumn { Header = "File name", Binding = new Binding("FileName") });

            ObservableCollection<GroupLine> groups = new ObservableCollection<GroupLine>();
            DataGrid groupsGrid = new DataGrid { Name = "GroupsGrid", AutoGenerateColumns = false, CanUserAddRows = false, ItemsSource = groups };
            groupsGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Run", Binding = new Binding("Include") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged } });
            groupsGrid.Columns.Add(new DataGridTextColumn { Header = "Building", Binding = new Binding("Building") });

            TextBox nwf = new TextBox { Name = "NwfFolderBox" };
            TextBox nwd = new TextBox { Name = "NwdFolderBox" };
            TextBox excel = new TextBox { Name = "ExcelFolderBox" };

            TextBox exchange = new TextBox { Name = "ExchangeFileBox" };
            ComboBox tolerance = new ComboBox { Name = "ToleranceBox" };
            tolerance.Items.Add("Use the value in the XML");
            tolerance.Items.Add("25 mm");
            tolerance.Items.Add("Other");
            tolerance.SelectedIndex = mode == "tolerance" ? 1 : 0;
            TextBox priority = new TextBox { Name = "PriorityBox" };
            CheckBox penetrations = new CheckBox { Name = "MarkPenetrations", Content = "Mark penetrations", IsChecked = false };
            CheckBox byDesign = new CheckBox { Name = "MarkByDesign", Content = "Mark by design", IsChecked = false };
            Button runOpen = new Button { Name = "RunOpenButton", Content = "Run the open file" };
            TextBlock openLine = new TextBlock { Name = "OpenDocumentLine", Text = "Nothing is open." };
            Button run = new Button { Name = "RunButton", Content = "Run" };

            string here = Path.GetDirectoryName(Path.GetFullPath(eventsFile));
            runOpen.IsEnabled = false;

            if (mode == "open-nwd")
            {
                openLine.Text = "This document is a .nwd file and this tool runs an NWF, which is where the clash tests and their results live. Open the NWF instead.";
            }
            else if (mode == "open-nwf")
            {
                openLine.Text = "Weekly run. Runs on standin, writes " + Path.Combine(here, "standin.nwd") + " and the report in " + Path.Combine(here, "Clash Reports") + ".";
                runOpen.IsEnabled = true;
            }
            else if (mode == "open-outside")
            {
                openLine.Text = "Weekly run. Runs on standin, writes C:\\Windows\\Temp\\standin.nwd and the report in C:\\Windows\\Temp\\Clash Reports.";
                runOpen.IsEnabled = true;
            }

            if (mode == "badbox")
            {
                nwf.TextChanged += delegate
                {
                    if (nwf.Text.Length > 0 && !nwf.Text.EndsWith("~", StringComparison.Ordinal))
                    {
                        nwf.Text = nwf.Text + "~";
                    }
                };
            }

            TabControl steps = new TabControl { Name = "Steps" };
            steps.Items.Add(new TabItem { Header = "1. Source", Content = Stack(source, subfolders, scan, summary, filesGrid) });
            steps.Items.Add(new TabItem { Header = "2. Grouping", Content = Stack(groupsGrid) });
            steps.Items.Add(new TabItem { Header = "3. Outputs", Content = Stack(nwf, nwd, excel) });
            steps.Items.Add(new TabItem { Header = "4. Clash", Content = Stack(exchange, tolerance, priority, penetrations, byDesign, runOpen, openLine) });

            DockPanel dock = new DockPanel();
            DockPanel.SetDock(run, Dock.Bottom);
            dock.Children.Add(run);
            dock.Children.Add(steps);
            window.Content = dock;

            scan.Click += delegate
            {
                Event(eventsFile, "Scan pressed, the source box reads " + source.Text);
                string folder = source.Text.Trim();

                if (folder.Length == 0 || !Directory.Exists(folder))
                {
                    MessageBox.Show(window, "Pick a folder that exists first.", "Parsons NWC Federator", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string[] found = Directory.GetFiles(folder, "*.nwc", SearchOption.AllDirectories);
                files.Clear();
                groups.Clear();

                foreach (string path in found)
                {
                    files.Add(new FileLine { FileName = Path.GetFileName(path) });
                }

                groups.Add(new GroupLine { Include = true, Building = "STANDIN1" });
                groups.Add(new GroupLine { Include = true, Building = "STANDIN2" });
                summary.Text = found.Length.ToString(CultureInfo.InvariantCulture) + " NWC found, " + found.Length.ToString(CultureInfo.InvariantCulture) + " readable, 0 that cannot be read.";

                if (log != null)
                {
                    log.ScanStarted(folder, subfolders.IsChecked == true);
                    log.ScanFinished(found.Length, found.Length, 0);
                }

                steps.SelectedIndex = 1;
            };

            run.Click += delegate
            {
                Event(eventsFile, "Run pressed");

                if (mode == "dialog" || mode == "pane-dialog")
                {
                    MessageBox.Show(window, "No group is ticked to run.", "Parsons NWC Federator", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // A WinForms window owned by this one and not modal, which the window rule alone
                // reads as a pane, so only its coming up after Run can stop the driver.
                if (mode == "pane-new")
                {
                    System.Windows.Forms.Form after = new System.Windows.Forms.Form();
                    after.Text = "NwcFederatorLoop stand-in, a window after Run";
                    after.Width = 380;
                    after.Height = 160;
                    after.Controls.Add(new System.Windows.Forms.Label { Text = "a window that came up after Run, not modal", AutoSize = true, Left = 12, Top = 12 });
                    after.Show(new OwnerHandle(new WindowInteropHelper(window).Handle));
                    Event(eventsFile, "a window after Run shown");
                    return;
                }

                string open = mode == "outside"
                    ? "This will be discarded without saving:" + Environment.NewLine + "    C:\\Windows\\Temp\\NwcFederatorLoop stand-in outside.nwf"
                    : "Nothing is open at the moment, so nothing is lost.";
                string text = "This run federates 2 groups." + Environment.NewLine + Environment.NewLine
                    + "First run: 2. The NWF is built new, and the document is cleared before each one." + Environment.NewLine
                    + "Weekly run: 0." + Environment.NewLine
                    + "Weekly run plus XML: 0." + Environment.NewLine
                    + "Rebuilt: 0." + Environment.NewLine + Environment.NewLine
                    + open + Environment.NewLine + Environment.NewLine + "Carry on?";
                MessageBoxResult answer = MessageBox.Show(window, text, "Parsons NWC Federator", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                Event(eventsFile, "confirm answered " + answer.ToString());

                if (answer == MessageBoxResult.OK && log != null)
                {
                    WriteRun(log, source.Text, nwf.Text, nwd.Text, groups);
                    log.WriteResultBlock();
                    string copied;
                    log.TryCopyTo(nwf.Text, out copied);
                }
            };

            runOpen.Click += delegate
            {
                Event(eventsFile, "RunOpenButton pressed");

                if (log != null)
                {
                    log.Line("OPEN     no XML picked, so the tests saved in the document run, or nothing runs when it holds none");
                    log.WriteResultBlock();
                    string copied;
                    log.TryCopyTo(here, out copied);
                }
            };

            window.SourceInitialized += delegate
            {
                HwndSource hwnd = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
                hwnd.AddHook(delegate(IntPtr h, int message, IntPtr w, IntPtr l, ref bool handled)
                {
                    if (message == WmClose)
                    {
                        Event(eventsFile, "WM_CLOSE received");
                    }

                    return IntPtr.Zero;
                });
            };

            window.ContentRendered += delegate
            {
                Event(eventsFile, "shown");

                if (mode == "selfclose")
                {
                    After(6, delegate { Event(eventsFile, "closing by itself"); window.Close(); });
                }

                if (mode == "autorun" && log != null)
                {
                    WriteRun(log, "-", "-", "-", groups);
                    After(25, delegate
                    {
                        log.WriteResultBlock();
                        string copied;
                        log.TryCopyTo(here, out copied);
                    });
                }
            };

            window.Closed += delegate
            {
                Event(eventsFile, "closed");

                if (log != null)
                {
                    log.Line("Window closed.");
                    log.Dispose();
                }
            };

            if (mode.StartsWith("pane", StringComparison.Ordinal))
            {
                return UnderPane(window, eventsFile);
            }

            // Navisworks stays up when the tool's window closes, so the stand-in does too, until the
            // role's seconds end it, and the monitor reads the window closed and not the process gone.
            Application application = new Application();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            application.Run(window);
            return 0;
        }

        /// <summary>
        /// F125. What the baseline run of 2026-10-04 read in Navisworks, steps\runs\04\item1-C02
        /// record.txt lines 36, 37 and 46: a WinForms main window whose caption ends as the
        /// Navisworks one, a WinForms pane captioned Clash Detective owned by it and not modal, and
        /// the tool's window owned by the main window and shown with ShowDialog, the way
        /// src\Federator.Addin\FederatorPlugin.cs shows it. The tool's window opens three seconds
        /// after the pane, so a reader sees the pane both before and after it. All three are on
        /// this one thread, and the role's seconds end the process, as Navisworks stays up when
        /// the tool's window closes.
        /// </summary>
        private static int UnderPane(Window window, string eventsFile)
        {
            System.Windows.Forms.Form main = new System.Windows.Forms.Form();
            main.Text = "NwcFederatorLoop stand-in - Autodesk Navisworks Manage 2025";
            main.Width = 700;
            main.Height = 480;
            System.Windows.Forms.Form pane = new System.Windows.Forms.Form();
            pane.Text = "Clash Detective";
            pane.Width = 360;
            pane.Height = 200;
            pane.Controls.Add(new System.Windows.Forms.Label { Text = "the stand-in's floating pane", AutoSize = true, Left = 12, Top = 12 });
            System.Windows.Forms.Timer later = new System.Windows.Forms.Timer { Interval = 3000 };
            later.Tick += delegate
            {
                later.Stop();
                Event(eventsFile, "the tool's window opens with ShowDialog over the main window");
                new WindowInteropHelper(window).Owner = main.Handle;
                window.ShowDialog();
            };
            main.Shown += delegate
            {
                pane.Show(main);
                Event(eventsFile, "pane shown");
                later.Start();
            };
            System.Windows.Forms.Application.Run(main);
            return 0;
        }

        private static StackPanel Stack(params UIElement[] children)
        {
            StackPanel panel = new StackPanel();

            foreach (UIElement child in children)
            {
                panel.Children.Add(child);
            }

            return panel;
        }

        /// <summary>The blocks the window's run writes before its RESULT block, in its shapes.</summary>
        private static void WriteRun(RunLog log, string source, string nwf, string nwd, IList<GroupLine> groups)
        {
            log.RunSettings(source, true, nwf, nwd, 2, 2, 2);
            List<string> lines = new List<string>();
            int unticked = 0;

            foreach (GroupLine group in groups)
            {
                lines.Add((group.Include ? "run     " : "unticked") + "  " + group.Building.PadRight(10) + "  1 file   STANDIN");

                if (!group.Include)
                {
                    unticked++;
                }
            }

            lines.Add(RunLog.UntickedGroupsLine(unticked));
            log.Block(RunLog.GroupsSectionTitle, lines);
            log.RunStarted(groups.Count);
            log.RunFinished();
        }

        private static void After(int seconds, Action action)
        {
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            timer.Tick += delegate
            {
                timer.Stop();
                action();
            };
            timer.Start();
        }

        private static void Event(string eventsFile, string text)
        {
            File.AppendAllText(eventsFile, DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + " " + text + Environment.NewLine, new UTF8Encoding(false));
        }
    }
}
