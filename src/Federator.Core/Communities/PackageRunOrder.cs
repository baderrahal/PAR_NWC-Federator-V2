using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Federator.Core.Communities
{
    /// <summary>One control of the window as the parent run left it, its kind, its name and its value in words.</summary>
    public sealed class ControlValue
    {
        public const string TextKind = "text";
        public const string TickKind = "tick";
        public const string ChoiceKind = "choice";

        public ControlValue(string kind, string name, string value)
        {
            if (kind != TextKind && kind != TickKind && kind != ChoiceKind)
            {
                throw new ArgumentException("a control is text, tick or choice, not " + kind, "kind");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("a control needs its name", "name");
            }

            Kind = kind;
            Name = name;
            Value = value ?? string.Empty;
        }

        public string Kind { get; private set; }

        public string Name { get; private set; }

        public string Value { get; private set; }
    }

    /// <summary>
    /// What the window hands each fresh Navisworks of a run of packages, F214, Bader's answer to Q204 of 18 Oct 2026: the
    /// page, the parent folder, the Report folder, the packages in the order they run, every setting of the window as the
    /// person left it, the folder the results go in, the plugin to run and the ceiling. Written once by the parent before
    /// the runner starts, read by the runner and by each package's Navisworks, never changed after. The window's settings
    /// are its controls by name, so every rule a run reads off the window reads the same in each package's Navisworks.
    /// </summary>
    public sealed class PackageRunOrder
    {
        /// <summary>The first line of an order file.</summary>
        public const string Header = "parsons nwc federator, the order of a run of packages, 1";

        /// <summary>The name of the order file in its run's folder.</summary>
        public const string FileName = "order.txt";

        /// <summary>The name of the status file the runner writes and the parent reads, one line a step.</summary>
        public const string StatusFileName = "status.txt";

        /// <summary>The folder beside the tool's logs folder holding one folder a run of packages, FolderFor.</summary>
        public const string RunsFolderName = "package-runs";

        /// <summary>The first argument the plugin is handed in a package's Navisworks, before the order's path and the index.</summary>
        public const string ChildMode = "package-run";

        public const string NwcPage = "NWC";
        public const string NwfPage = "NWF";

        public PackageRunOrder()
        {
            Packages = new List<string>();
            Controls = new List<ControlValue>();
            Settings = new PackageRunSettings();
        }

        /// <summary>NwcPage or NwfPage, the route each package runs.</summary>
        public string Page { get; set; }

        public string Parent { get; set; }

        public string Report { get; set; }

        /// <summary>The run's own folder, holding the order, the status file and each package's result.</summary>
        public string Folder { get; set; }

        /// <summary>The plugin each Navisworks runs, its name and developer code joined by a full stop.</summary>
        public string PluginId { get; set; }

        /// <summary>The parent's own log, named in each package's log.</summary>
        public string ParentLog { get; set; }

        /// <summary>Each package by its path from the parent, in the order it runs.</summary>
        public IList<string> Packages { get; private set; }

        /// <summary>The window's controls, the text boxes first, then the tick boxes, then the drop downs.</summary>
        public IList<ControlValue> Controls { get; private set; }

        public PackageRunSettings Settings { get; private set; }

        public string OrderPath
        {
            get { return Path.Combine(Folder, FileName); }
        }

        public string StatusPath
        {
            get { return Path.Combine(Folder, StatusFileName); }
        }

        /// <summary>Where the package at this index, counted from nought, writes its result.</summary>
        public string ResultPathFor(int index)
        {
            return Path.Combine(Folder, "result-" + (index + 1).ToString("000", CultureInfo.InvariantCulture) + ".txt");
        }

        /// <summary>
        /// The folder of one run under the root, named for the second it started, and a name already there taking the
        /// next free number, so no run of packages writes into another's.
        /// </summary>
        public static string FolderFor(string root, DateTime now, Func<string, bool> exists)
        {
            string name = now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string path = Path.Combine(root, name);

            for (int n = 2; exists(path); n++)
            {
                path = Path.Combine(root, name + "-" + n.ToString(CultureInfo.InvariantCulture));
            }

            return path;
        }

        /// <summary>Why this order cannot be run, or null.</summary>
        public string WhyNot()
        {
            if (Page != NwcPage && Page != NwfPage)
            {
                return "the page is " + (Page ?? "not named") + ", where an order runs the NWC page or the NWF page";
            }

            if (string.IsNullOrEmpty(Parent) || string.IsNullOrEmpty(Report) || string.IsNullOrEmpty(Folder) || string.IsNullOrEmpty(PluginId))
            {
                return "the order names no parent folder, Report folder, run folder or plugin";
            }

            if (Packages.Count == 0)
            {
                return "the order names no package";
            }

            return null;
        }

        public void Write()
        {
            string why = WhyNot();

            if (why != null)
            {
                throw new InvalidOperationException(why);
            }

            List<string> lines = new List<string>
            {
                PackageRunFile.Line("page", Page),
                PackageRunFile.Line("parent", Parent),
                PackageRunFile.Line("report", Report),
                PackageRunFile.Line("folder", Folder),
                PackageRunFile.Line("plugin", PluginId),
                PackageRunFile.Line("parentlog", ParentLog ?? string.Empty),
                PackageRunFile.Line("ceiling", Settings.CeilingMinutes.ToString(CultureInfo.InvariantCulture)),
                PackageRunFile.Line("gone", Settings.GoneWithinSeconds.ToString(CultureInfo.InvariantCulture)),
                PackageRunFile.Line("poll", Settings.PollMilliseconds.ToString(CultureInfo.InvariantCulture))
            };

            foreach (string package in Packages)
            {
                lines.Add(PackageRunFile.Line("package", package));
            }

            foreach (ControlValue control in Controls)
            {
                lines.Add(PackageRunFile.Line("control", control.Kind, control.Name, control.Value));
            }

            PackageRunFile.Write(OrderPath, Header, lines);
        }

        /// <summary>
        /// The order at this path, or null with why: the file not read, a line of a key it does not know or of the wrong
        /// number of values, a number that is not one or that the settings refuse, or an order that cannot be run.
        /// </summary>
        public static PackageRunOrder Read(string path, out string why)
        {
            IList<string[]> lines = PackageRunFile.Read(path, Header, out why);

            if (lines == null)
            {
                return null;
            }

            PackageRunOrder order = new PackageRunOrder();

            foreach (string[] line in lines)
            {
                string fault = order.Take(line);

                if (fault != null)
                {
                    why = "the order " + path + " has a line that does not read, " + fault;
                    return null;
                }
            }

            why = order.WhyNot();
            return why == null ? order : null;
        }

        private string Take(string[] line)
        {
            string key = line[0];
            int values = line.Length - 1;

            if (key == "control")
            {
                if (values != 3)
                {
                    return "a control line holds " + values + " values where it holds 3";
                }

                try
                {
                    Controls.Add(new ControlValue(line[1], line[2], line[3]));
                    return null;
                }
                catch (ArgumentException error)
                {
                    return error.Message;
                }
            }

            if (values != 1)
            {
                return "the line " + key + " holds " + values + " values where it holds 1";
            }

            string value = line[1];

            switch (key)
            {
                case "page":
                    Page = value;
                    return null;
                case "parent":
                    Parent = value;
                    return null;
                case "report":
                    Report = value;
                    return null;
                case "folder":
                    Folder = value;
                    return null;
                case "plugin":
                    PluginId = value;
                    return null;
                case "parentlog":
                    ParentLog = value.Length == 0 ? null : value;
                    return null;
                case "package":
                    Packages.Add(value);
                    return null;
                case "ceiling":
                case "gone":
                case "poll":
                    int number;

                    if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number))
                    {
                        return "the " + key + " is " + value + ", not a whole number";
                    }

                    try
                    {
                        if (key == "ceiling")
                        {
                            Settings.CeilingMinutes = number;
                        }
                        else if (key == "gone")
                        {
                            Settings.GoneWithinSeconds = number;
                        }
                        else
                        {
                            Settings.PollMilliseconds = number;
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return "the " + key + " is " + value + ", which the settings refuse";
                    }

                    return null;
                default:
                    return "the key " + key + " is not one an order holds";
            }
        }
    }
}
