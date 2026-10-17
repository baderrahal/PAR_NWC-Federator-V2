using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;

namespace DisciplineProbe
{
    /// <summary>
    /// F209, the discipline reports, 2026-10-18. Measured before anything is built, Bader's message of 17 Oct 2026: how a
    /// CAD file imported or linked into a Revit model shows in the Navisworks tree and the properties of the NWC exported
    /// from it, and which properties carry the Revit category, the element id, the family, the type and the level of an
    /// element whose category is Generic Models. Read only: the document is cleared, the NWC copies of one folder are
    /// appended, every item of every model is walked, and nothing is saved, published or changed.
    ///
    /// What it writes, per model: the root item's tabs whole, its box in the document's units, the count of items by the
    /// Element tab's Category and by the item's class, every tab and property of the first Generic Models elements, and
    /// every item whose name, class or any property value but the Source File names a CAD file by its extension, with
    /// its parents up to the root and its tabs whole. The model an item lives in is the model whose root was walked.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Discipline probe", ToolTip = "F209, CAD imports and Generic Models in an NWC")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class DisciplineProbePlugin : AddInPlugin
    {
        public const string PluginName = "DisciplineProbe";
        public const string DeveloperCode = "PARS";

        private const string ElementTab = "LcRevitData_Element";
        private const string CategoryProperty = "LcRevitPropertyElementCategory";
        private const string ItemTab = "LcOaNode";
        private const string SourceFileProperty = "LcOaNodeSourceFile";
        private const string GenericModels = "Generic Models";
        private const int GenericDumped = 4;
        private const int CadDumped = 6;

        private static readonly string[] CadExtensions = { ".dwg", ".dxf", ".dgn", ".dwf", ".dwfx", ".sat", ".skp", ".3dm", ".ifc", ".fbx", ".3ds", ".nwc", ".nwd" };
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;

        public override int Execute(params string[] parameters)
        {
            if (parameters != null && parameters.Length >= 5 && parameters[0] == "find")
            {
                using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
                {
                    results.AutoFlush = true;
                    Say("probe started, mode find, folder " + parameters[3] + ", ids " + parameters[4]);

                    try
                    {
                        int code = Find(parameters[2], parameters[4]);
                        Say("probe finished, code " + code);
                        return code;
                    }
                    catch (Exception error)
                    {
                        Say("THREW " + error.GetType().Name + ": " + error.Message);
                        return 1;
                    }
                }
            }

            if (parameters == null || parameters.Length < 4 || parameters[0] != "discipline")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode " + parameters[0] + ", folder " + parameters[3]);

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
            results.WriteLine(line);
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

            document.Clear();
            int appended = 0;

            foreach (string file in files)
            {
                bool ok = document.TryAppendFile(file);
                Say("TryAppendFile " + Path.GetFileName(file) + " returned " + ok + ", models now " + document.Models.Count);

                if (ok)
                {
                    appended++;
                }
            }

            Stopwatch wait = Stopwatch.StartNew();

            while (document.Models.Count < appended && wait.Elapsed.TotalSeconds < 120)
            {
                Thread.Sleep(250);
            }

            Say("appended " + appended + " of " + files.Length + ", models " + document.Models.Count + ", document units " + document.Units);
            Say("");

            int genericTotal = 0;
            int cadTotal = 0;
            Dictionary<string, int> genericKeys = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> cadReasons = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < document.Models.Count; i++)
            {
                Model model = document.Models[i];
                string name = Path.GetFileName(model.FileName ?? string.Empty);
                Say("==== model " + i + " " + name + ", units " + model.Units + ", SourceFileName " + Path.GetFileName(model.SourceFileName ?? string.Empty) + " ====");
                ModelItem root = model.RootItem;

                if (root == null)
                {
                    Say("  UNKNOWN: RootItem is null");
                    continue;
                }

                BoundingBox3D box = root.BoundingBox();
                Say("  root box " + (box == null || box.IsEmpty ? "empty" : Point(box.Min) + " to " + Point(box.Max)) + " in the document's units");
                Say("  root display name [" + root.DisplayName + "], class [" + root.ClassName + "] [" + root.ClassDisplayName + "]");
                DumpTabs(root, "  root ");

                Dictionary<string, int> categories = new Dictionary<string, int>(StringComparer.Ordinal);
                Dictionary<string, int> classes = new Dictionary<string, int>(StringComparer.Ordinal);
                int items = 0;
                int generic = 0;
                int cad = 0;
                int threw = 0;
                int imports = 0;

                foreach (ModelItem item in root.DescendantsAndSelf)
                {
                    items++;

                    try
                    {
                        string category = ReadOne(item, ElementTab, CategoryProperty);
                        Count(categories, category ?? "(no Element tab Category)");
                        Count(classes, (item.ClassDisplayName ?? string.Empty) + " / " + (item.ClassName ?? string.Empty));

                        if (string.Equals(category, GenericModels, StringComparison.Ordinal))
                        {
                            generic++;

                            foreach (string key in TabKeys(item))
                            {
                                Count(genericKeys, key);
                            }

                            if (generic <= GenericDumped)
                            {
                                Say("  GENERIC MODELS element " + generic + ", display name [" + item.DisplayName + "], class [" + item.ClassDisplayName + "], parent [" + (item.Parent == null ? "none" : item.Parent.DisplayName) + "], children " + Children(item));
                                DumpTabs(item, "    ");
                            }
                        }

                        // The second pass of 18 Oct: every topmost item the Item tab types Imports in Families, the
                        // first pass's shape of a CAD file imported into a family, with the element it hangs under.
                        if (IsImport(item) && (item.Parent == null || !IsImport(item.Parent)))
                        {
                            imports++;
                            Say("  IMPORT " + imports + "  [" + item.DisplayName + "]  " + ImportFacts(item));
                        }

                        string why = CadReason(item, category);

                        if (why != null)
                        {
                            cad++;
                            Count(cadReasons, why);

                            if (cad <= CadDumped)
                            {
                                Say("  CAD candidate " + cad + ", because " + why + ", display name [" + item.DisplayName + "], class [" + item.ClassDisplayName + "] [" + item.ClassName + "], children " + Children(item));
                                Say("    parents: " + Parents(item));
                                DumpTabs(item, "    ");
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        threw++;

                        if (threw <= 3)
                        {
                            Say("  item " + items + " threw " + error.GetType().Name + ": " + error.Message);
                        }
                    }
                }

                genericTotal += generic;
                cadTotal += cad;
                Say("  items " + items + ", threw " + threw + ", Generic Models elements " + generic + ", CAD candidates " + cad + ", topmost imports " + imports);
                Say("  by the Element tab's Category:");

                foreach (KeyValuePair<string, int> pair in Sorted(categories))
                {
                    Say("    " + pair.Value.ToString(Inv).PadLeft(7) + "  " + pair.Key);
                }

                Say("  by the item's class, display / internal:");

                foreach (KeyValuePair<string, int> pair in Sorted(classes))
                {
                    Say("    " + pair.Value.ToString(Inv).PadLeft(7) + "  " + pair.Key);
                }

                Say("");
            }

            Say("==== ACROSS THE FOLDER ====");
            Say("Generic Models elements " + genericTotal + ", CAD candidates " + cadTotal);
            Say("the tab and property names on Generic Models elements, with how many carry each:");

            foreach (KeyValuePair<string, int> pair in Sorted(genericKeys))
            {
                Say("  " + pair.Value.ToString(Inv).PadLeft(7) + "  " + pair.Key);
            }

            Say("the reasons an item was a CAD candidate:");

            foreach (KeyValuePair<string, int> pair in Sorted(cadReasons))
            {
                Say("  " + pair.Value.ToString(Inv).PadLeft(7) + "  " + pair.Key);
            }

            Say("nothing in any model was changed, nothing was saved and nothing was published");
            return 0;
        }

        /// <summary>
        /// The F210 mode, find, 2026-10-18: the element ids a discipline report listed, found again in Navisworks by the Element
        /// tab's Id, each with its model, its name, its class, its category, family, type and level, the parents up to the file
        /// and any import under it, so the report's line can be read against what Navisworks holds.
        /// </summary>
        private int Find(string folder, string idList)
        {
            if (!Directory.Exists(folder))
            {
                Say("UNKNOWN: the folder of copies is not there");
                return 3;
            }

            HashSet<string> ids = new HashSet<string>(idList.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
            string[] files = Directory.GetFiles(folder, "*.nwc");
            Array.Sort(files, StringComparer.Ordinal);
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            document.Clear();

            foreach (string file in files)
            {
                Say("TryAppendFile " + Path.GetFileName(file) + " returned " + document.TryAppendFile(file));
            }

            Dictionary<string, int> found = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < document.Models.Count; i++)
            {
                Model model = document.Models[i];
                string name = Path.GetFileName(model.FileName ?? string.Empty);

                foreach (ModelItem item in model.RootItem.DescendantsAndSelf)
                {
                    string id = ReadOne(item, ElementTab, "LcRevitPropertyElementId");

                    if (id == null || !ids.Contains(id))
                    {
                        continue;
                    }

                    int was;
                    found[id] = found.TryGetValue(id, out was) ? was + 1 : 1;
                    List<string> imports = new List<string>();

                    foreach (ModelItem below in item.Descendants)
                    {
                        if (IsImport(below) && imports.Count < 4 && !imports.Contains(below.DisplayName))
                        {
                            imports.Add(below.DisplayName);
                        }
                    }

                    Say("FOUND id " + id + " in " + name + ": [" + item.DisplayName + "] class [" + item.ClassDisplayName + "], category "
                        + ReadOne(item, ElementTab, CategoryProperty) + ", family " + ReadOne(item, ElementTab, "LcRevitPropertyElementFamily")
                        + ", type " + ReadOne(item, ElementTab, "LcRevitPropertyElementType") + ", Level " + ReadOne(item, ElementTab, "lcldrevit_parameter_-1001352")
                        + ", Schedule Level " + ReadOne(item, ElementTab, "lcldrevit_parameter_-1001365") + ", Layer " + ReadOne(item, ItemTab, "LcOaNodeLayer")
                        + ", imports under it " + (imports.Count == 0 ? "none" : string.Join(", ", imports.ToArray())));
                    Say("     parents: " + Parents(item));
                }
            }

            foreach (string id in ids)
            {
                int count;
                Say("id " + id + ": " + (found.TryGetValue(id, out count) ? count + " item(s) carry it" : "NOT FOUND in any model"));
            }

            Say("nothing in any model was changed, nothing was saved and nothing was published");
            return 0;
        }

        /// <summary>Why an item names a CAD file: its display name, class or category ends with a CAD extension, or a property value but the Source File holds one.</summary>
        private static string CadReason(ModelItem item, string category)
        {
            foreach (string extension in CadExtensions)
            {
                if (EndsWith(item.DisplayName, extension))
                {
                    return "its display name ends with " + extension;
                }

                if (EndsWith(category, extension))
                {
                    return "its Element tab Category ends with " + extension;
                }
            }

            string cls = (item.ClassDisplayName ?? string.Empty) + " " + (item.ClassName ?? string.Empty);

            if (cls.IndexOf("import", StringComparison.OrdinalIgnoreCase) >= 0 || cls.IndexOf("link", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "its class names import or link: " + cls.Trim();
            }

            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    return null;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    using (tab)
                    {
                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            foreach (DataProperty property in properties)
                            {
                                using (property)
                                {
                                    if (string.Equals(tab.Name, ItemTab, StringComparison.Ordinal) && string.Equals(property.Name, SourceFileProperty, StringComparison.Ordinal))
                                    {
                                        continue;
                                    }

                                    string kind;
                                    string text = Text(property, out kind);

                                    foreach (string extension in CadExtensions)
                                    {
                                        if (text.IndexOf(extension, StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            return "the property " + tab.Name + "." + property.Name + " holds " + extension;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static bool IsImport(ModelItem item)
        {
            return string.Equals(ReadOne(item, ItemTab, "LcOaSceneBaseClassUserName"), "Imports in Families", StringComparison.Ordinal);
        }

        /// <summary>
        /// The facts of one topmost import on one line: the nearest element above it carrying the Element tab's Id, with its
        /// category, family, type, Level, Schedule Level and the Item tab's Layer, the Element ID tab of the import and of its
        /// first child, and the names under it that end with a CAD extension.
        /// </summary>
        private static string ImportFacts(ModelItem item)
        {
            ModelItem host = item.Parent;

            while (host != null && ReadOne(host, ElementTab, "LcRevitPropertyElementId") == null)
            {
                host = host.Parent;
            }

            string own = ReadOne(item, "LcRevitId", "LcOaNat64AttributeValue");
            string child = null;
            List<string> named = new List<string>();

            foreach (ModelItem below in item.DescendantsAndSelf)
            {
                if (child == null && !ReferenceEquals(below, item))
                {
                    child = ReadOne(below, "LcRevitId", "LcOaNat64AttributeValue");
                }

                foreach (string extension in CadExtensions)
                {
                    if (EndsWith(below.DisplayName, extension) && named.Count < 3 && !named.Contains(below.DisplayName))
                    {
                        named.Add(below.DisplayName);
                    }
                }
            }

            return "host " + (host == null ? "none" : "[" + host.DisplayName + "] Id " + ReadOne(host, ElementTab, "LcRevitPropertyElementId")
                    + ", category " + ReadOne(host, ElementTab, CategoryProperty) + ", family " + ReadOne(host, ElementTab, "LcRevitPropertyElementFamily")
                    + ", type " + ReadOne(host, ElementTab, "LcRevitPropertyElementType") + ", Level " + ReadOne(host, ElementTab, "lcldrevit_parameter_-1001352")
                    + ", Schedule Level " + ReadOne(host, ElementTab, "lcldrevit_parameter_-1001365") + ", Layer " + ReadOne(host, ItemTab, "LcOaNodeLayer"))
                + "; the import's own Element ID tab " + (own ?? "none") + ", its first child's " + (child ?? "none")
                + "; its Layer " + (ReadOne(item, ItemTab, "LcOaNodeLayer") ?? "none")
                + "; CAD names at or under it " + (named.Count == 0 ? "none" : string.Join(", ", named.ToArray()));
        }

        private static bool EndsWith(string text, string extension)
        {
            return !string.IsNullOrEmpty(text) && text.Trim().EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadOne(ModelItem item, string tabName, string propertyName)
        {
            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    return null;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    using (tab)
                    {
                        if (!string.Equals(tab.Name, tabName, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            foreach (DataProperty property in properties)
                            {
                                using (property)
                                {
                                    if (string.Equals(property.Name, propertyName, StringComparison.Ordinal))
                                    {
                                        string kind;
                                        return Text(property, out kind);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static IList<string> TabKeys(ModelItem item)
        {
            List<string> keys = new List<string>();

            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    return keys;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    using (tab)
                    {
                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            foreach (DataProperty property in properties)
                            {
                                using (property)
                                {
                                    keys.Add("[" + tab.DisplayName + "] " + tab.Name + " . " + property.Name + " [" + property.DisplayName + "]");
                                }
                            }
                        }
                    }
                }
            }

            return keys;
        }

        private void DumpTabs(ModelItem item, string indent)
        {
            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    Say(indent + "no tabs");
                    return;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    using (tab)
                    {
                        Say(indent + "[" + tab.DisplayName + "] internal " + tab.Name);

                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            foreach (DataProperty property in properties)
                            {
                                using (property)
                                {
                                    string kind;
                                    string text = Text(property, out kind);
                                    Say(indent + "   " + property.DisplayName + "  internal " + property.Name + "  " + kind + "  = " + (text.Length > 160 ? text.Substring(0, 160) + "..." : text));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string Parents(ModelItem item)
        {
            List<string> names = new List<string>();
            ModelItem at = item.Parent;

            while (at != null && names.Count < 12)
            {
                names.Add("[" + at.DisplayName + "] " + at.ClassDisplayName);
                at = at.Parent;
            }

            return string.Join(" < ", names.ToArray());
        }

        private static string Children(ModelItem item)
        {
            int count = 0;

            foreach (ModelItem child in item.Children)
            {
                count++;
            }

            return count.ToString(Inv);
        }

        private static string Point(Point3D p)
        {
            return p.X.ToString("0.###", Inv) + " " + p.Y.ToString("0.###", Inv) + " " + p.Z.ToString("0.###", Inv);
        }

        private static void Count(Dictionary<string, int> counts, string key)
        {
            int was;
            counts[key] = counts.TryGetValue(key, out was) ? was + 1 : 1;
        }

        private static List<KeyValuePair<string, int>> Sorted(Dictionary<string, int> counts)
        {
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(counts);
            list.Sort((a, b) => b.Value != a.Value ? b.Value.CompareTo(a.Value) : string.CompareOrdinal(a.Key, b.Key));
            return list;
        }

        /// <summary>A property's value read by its kind, as the add-in's ClashHarvest reads one, scan.md 4n.</summary>
        private static string Text(DataProperty property, out string kind)
        {
            kind = "unread";

            try
            {
                using (VariantData value = property.Value)
                {
                    if (value == null)
                    {
                        kind = "null";
                        return string.Empty;
                    }

                    kind = value.DataType.ToString();

                    switch (value.DataType)
                    {
                        case VariantDataType.None:
                            return string.Empty;
                        case VariantDataType.DisplayString:
                            return value.ToDisplayString();
                        case VariantDataType.IdentifierString:
                            return value.ToIdentifierString();
                        case VariantDataType.NamedConstant:
                            using (NamedConstant named = value.ToNamedConstant())
                            {
                                return named == null ? string.Empty : (named.DisplayName ?? string.Empty);
                            }

                        case VariantDataType.Int32:
                            return value.ToInt32().ToString(Inv);
                        case VariantDataType.Boolean:
                            return value.ToBoolean().ToString(Inv);
                        case VariantDataType.DateTime:
                            return value.ToDateTime().ToString("yyyy-MM-dd HH:mm:ss", Inv);
                        case VariantDataType.Double:
                        case VariantDataType.DoubleLength:
                        case VariantDataType.DoubleAngle:
                        case VariantDataType.DoubleArea:
                        case VariantDataType.DoubleVolume:
                            return value.ToAnyDouble().ToString("R", Inv);
                        default:
                            return value.ToString() ?? string.Empty;
                    }
                }
            }
            catch (Exception error)
            {
                kind = "threw " + error.GetType().Name;
                return string.Empty;
            }
        }
    }
}
