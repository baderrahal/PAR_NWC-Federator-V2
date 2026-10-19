using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Plugins;

namespace ClashPickProbe
{
    /// <summary>
    /// F237, the full run of Bader's round of 19 Oct 2026: five random clashes a community picked from the Excel, each found in
    /// Navisworks by its two element IDs. Read only: each NWF it is handed is opened by Document.TryOpenFile, and for each pick
    /// of it, a test name and two element IDs, the test of that name is found among the document's clash tests and its results
    /// walked, each group's results among them, for the one whose two items carry the two IDs in either order. It writes the
    /// test, the clash's name and its status, or that none or more than one was found. An item's element id is read as the
    /// harvest reads it, the first property named Id, Element Id, ElementId or Element ID on the item and then up its
    /// ancestors, by display name. Nothing is saved, run, edited or changed, and the host proves the NWF unchanged by its SHA-256.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Clash pick probe", ToolTip = "F237, five clashes a community found by their element ids")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class ClashPickProbePlugin : AddInPlugin
    {
        public const string PluginName = "ClashPickProbe";
        public const string DeveloperCode = "PARS";

        private static readonly string[] IdNames = { "Id", "Element Id", "ElementId", "Element ID" };

        private StreamWriter results;

        /// <summary>parameters: read, the output file, the NWFs joined by |, their labels joined by |, the picks file.</summary>
        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 5 || parameters[0] != "read")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode read");

                try
                {
                    string[] files = parameters[2].Split('|');
                    string[] labels = parameters[3].Split('|');
                    List<string[]> picks = new List<string[]>();

                    foreach (string line in File.ReadAllLines(parameters[4]))
                    {
                        string[] cells = line.Split('\t');

                        if (cells.Length >= 5 && cells[0] != "nwf")
                        {
                            picks.Add(cells);
                        }
                    }

                    Say("ROWHEAD\tlabel\tnwf\tpick\ttest\tid 1\tid 2\tfound\tclash\tstatus\tin a group");
                    int code = 0;

                    for (int i = 0; i < files.Length && i < labels.Length; i++)
                    {
                        if (!ReadOne(files[i], labels[i], picks))
                        {
                            code = 1;
                        }
                    }

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
            results.WriteLine(line);
        }

        private bool ReadOne(string file, string label, List<string[]> picks)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say("---- " + label + "  " + file);

            if (document == null || !File.Exists(file))
            {
                Say("UNKNOWN: no active document or the file is not there");
                return false;
            }

            bool opened = document.TryOpenFile(file);
            Say("TryOpenFile returned " + opened + ", models " + document.Models.Count);

            if (!opened)
            {
                return false;
            }

            int last = -1;
            int same = 0;
            DateTime until = DateTime.UtcNow.AddSeconds(30);

            while (DateTime.UtcNow < until)
            {
                int now = document.Models.Count;
                same = now == last && now > 0 ? same + 1 : 0;
                last = now;

                if (same >= 2)
                {
                    break;
                }

                Thread.Sleep(250);
            }

            DocumentClash clash = document.GetClash();
            bool all = true;

            foreach (string[] pick in picks)
            {
                if (!string.Equals(pick[0], file, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string test = pick[2];
                string id1 = pick[3];
                string id2 = pick[4];
                List<string> found = new List<string>();

                foreach (SavedItem item in clash.TestsData.Tests)
                {
                    ClashTest saved = item as ClashTest;

                    if (saved == null || !string.Equals(saved.DisplayName, test, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Walk(saved.Children, id1, id2, false, found);
                }

                Say("ROW\t" + label + "\t" + file + "\t" + pick[1] + "\t" + test + "\t" + id1 + "\t" + id2 + "\t"
                    + found.Count.ToString(CultureInfo.InvariantCulture) + "\t" + (found.Count == 1 ? found[0] : string.Join(" | ", found.ToArray())));
                all = all && found.Count == 1;
            }

            return all;
        }

        private static void Walk(SavedItemCollection children, string id1, string id2, bool inAGroup, List<string> found)
        {
            foreach (SavedItem child in children)
            {
                ClashResultGroup group = child as ClashResultGroup;

                if (group != null)
                {
                    Walk(group.Children, id1, id2, true, found);
                    continue;
                }

                ClashResult result = child as ClashResult;

                if (result == null)
                {
                    continue;
                }

                string a = IdOf(result.Item1);
                string b = IdOf(result.Item2);

                if ((a == id1 && b == id2) || (a == id2 && b == id1))
                {
                    found.Add(result.DisplayName + "\t" + result.Status + "\t" + (inAGroup ? "yes" : "no"));
                }
            }
        }

        /// <summary>The element id of an item as the harvest reads it: the item, then up its ancestors, the first id property by its display name.</summary>
        private static string IdOf(ModelItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            List<ModelItem> lookIn = new List<ModelItem> { item };

            foreach (ModelItem above in item.Ancestors)
            {
                lookIn.Add(above);
            }

            foreach (ModelItem at in lookIn)
            {
                foreach (string name in IdNames)
                {
                    foreach (PropertyCategory category in at.PropertyCategories)
                    {
                        foreach (DataProperty property in category.Properties)
                        {
                            if (!string.Equals(property.DisplayName, name, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            string text = Text(property.Value);

                            if (text.Length > 0)
                            {
                                return text;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>A value as text by its kind, a Revit element id an Int32, the kind's prefix taken off where ToString gives one.</summary>
        private static string Text(VariantData value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value.IsInt32)
            {
                return value.ToInt32().ToString(CultureInfo.InvariantCulture);
            }

            if (value.IsDisplayString)
            {
                return value.ToDisplayString();
            }

            string all = value.ToString();
            int colon = all.IndexOf(':');
            return colon >= 0 ? all.Substring(colon + 1) : all;
        }
    }
}
