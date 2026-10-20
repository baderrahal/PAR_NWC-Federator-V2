using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Plugins;

namespace SameSetProbe
{
    /// <summary>
    /// F248, 2026-10-20. What Clash Detective does with a clash test whose left and right sides are the SAME selection set.
    /// The models it is handed are appended into the document the automation host holds, nothing is saved. Each set it is asked
    /// for is read out of the pack's clash XML, the selectionset element whose name matches, and built as a Search of the same
    /// conditions the way the add-in builds one (SetBuilder.BuildCondition), then put in the document with AddCopy. For each
    /// set two tests are made with the add-in's calls (ClashRunner.Create, FillSide): hard conservative, 25 mm in the document's
    /// units, merge composites on, primitive types 1, each side pointing at the saved set through CreateSelectionSource,
    /// self intersect on both sides as the pack has it (off) in one test and the other way (on) in the second. Each test is
    /// run by TestsRunTest and its results walked: the item pairs, whether Item1 is Item2, whether a pair shows twice and the
    /// categories of the two items.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Same set probe", ToolTip = "F248, a clash test with the same set on both sides")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class SameSetProbePlugin : AddInPlugin
    {
        public const string PluginName = "SameSetProbe";
        public const string DeveloperCode = "PARS";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private StreamWriter results;
        private string rowsFolder;

        /// <summary>parameters: run, the output file, the NWCs joined by |, the pack XML, the sets joined by |, the folder for the row files.</summary>
        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 6 || parameters[0] != "run")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode run");

                try
                {
                    rowsFolder = parameters[5];
                    Directory.CreateDirectory(rowsFolder);
                    return Work(parameters[2].Split('|'), parameters[3], parameters[4].Split('|'));
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

        private int Work(string[] files, string pack, string[] setNames)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document");
                return 1;
            }

            Say("document at the start: models " + document.Models.Count + ", file name '" + document.FileName + "', units " + document.Units);

            foreach (string file in files)
            {
                bool appended = document.TryAppendFile(file);
                Say("TryAppendFile " + Path.GetFileName(file) + " returned " + appended + ", models " + document.Models.Count);
            }

            WaitForModels(document);
            Say("models in the document: " + document.Models.Count + ", units " + document.Units);

            foreach (Model model in document.Models)
            {
                Say("  model " + Path.GetFileName(model.SourceFileName) + ", units " + model.Units);
            }

            double scale = UnitConversion.ScaleFactor(Units.Millimeters, document.Units);
            double tolerance = 25.0 * scale;
            Say("ScaleFactor(Millimeters, " + document.Units + ") = " + scale.ToString("R", Inv) + ", 25 mm = " + tolerance.ToString("R", Inv) + " " + document.Units);

            XmlDocument xml = new XmlDocument();
            xml.Load(pack);
            DocumentSelectionSets sets = document.SelectionSets;
            Dictionary<string, SelectionSet> made = new Dictionary<string, SelectionSet>();

            foreach (string name in setNames)
            {
                XmlElement element = FindSet(xml, name);

                if (element == null)
                {
                    Say("UNKNOWN: the pack holds no selectionset named " + name);
                    continue;
                }

                List<Cond> conditions = ConditionsOf(element);
                Say("");
                Say("==== SET " + name + ", " + conditions.Count + " conditions read from the pack ====");

                using (Search search = new Search())
                {
                    search.Selection.SelectAll();
                    search.Locations = SearchLocations.DescendantsAndSelf;

                    foreach (Cond c in conditions)
                    {
                        search.SearchConditions.Add(Build(c));
                    }

                    using (SelectionSet set = new SelectionSet(search))
                    {
                        set.DisplayName = name;
                        sets.AddCopy(set);
                    }
                }

                SelectionSet inDoc = null;

                foreach (SavedItem child in sets.RootItem.Children)
                {
                    if (child.DisplayName == name)
                    {
                        inDoc = child as SelectionSet;
                    }
                }

                if (inDoc == null)
                {
                    Say("UNKNOWN: the set was added but is not found again at the root");
                    continue;
                }

                made[name] = inDoc;
                SetCounts(document, inDoc);
            }

            foreach (string name in setNames)
            {
                SelectionSet set;

                if (!made.TryGetValue(name, out set))
                {
                    continue;
                }

                foreach (bool self in new[] { false, true })
                {
                    OneTest(document, sets, set, name, self, tolerance);
                }
            }

            try
            {
                document.Clear();
                Say("");
                Say("document.Clear() called at the end, models " + document.Models.Count + ". Nothing was saved");
            }
            catch (Exception error)
            {
                Say("document.Clear() threw " + error.GetType().Name + ": " + error.Message);
            }

            return 0;
        }

        private void WaitForModels(Document document)
        {
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
        }

        // ---- the pack's set, as the add-in reads and builds it ----

        private sealed class Cond
        {
            public string Test;
            public int Flags;
            public string CatInternal;
            public string CatDisplay;
            public string PropInternal;
            public string PropDisplay;
            public string Value;
            public bool HasCategory;
        }

        private static XmlElement FindSet(XmlDocument xml, string name)
        {
            foreach (XmlElement e in xml.GetElementsByTagName("selectionset"))
            {
                if (e.GetAttribute("name") == name)
                {
                    return e;
                }
            }

            return null;
        }

        private static List<Cond> ConditionsOf(XmlElement set)
        {
            List<Cond> list = new List<Cond>();

            foreach (XmlElement c in set.GetElementsByTagName("condition"))
            {
                Cond one = new Cond();
                one.Test = c.GetAttribute("test");
                one.Flags = int.Parse(c.GetAttribute("flags"), Inv);
                XmlElement cat = FirstChild(c, "category");
                XmlElement prop = FirstChild(c, "property");
                XmlElement value = FirstChild(c, "value");
                one.HasCategory = cat != null;

                if (cat != null)
                {
                    XmlElement n = FirstChild(cat, "name");
                    one.CatInternal = n.GetAttribute("internal");
                    one.CatDisplay = n.InnerText;
                }

                XmlElement pn = FirstChild(prop, "name");
                one.PropInternal = pn.GetAttribute("internal");
                one.PropDisplay = pn.InnerText;
                XmlElement data = FirstChild(value, "data");
                one.Value = data == null ? string.Empty : data.InnerText;
                list.Add(one);
            }

            return list;
        }

        private static XmlElement FirstChild(XmlElement parent, string name)
        {
            foreach (XmlNode n in parent.ChildNodes)
            {
                XmlElement e = n as XmlElement;

                if (e != null && e.Name == name)
                {
                    return e;
                }
            }

            return null;
        }

        private static SearchCondition Build(Cond c)
        {
            SearchConditionOptions options = (SearchConditionOptions)c.Flags
                | SearchConditionOptions.IgnoreCategoryDisplayName
                | SearchConditionOptions.IgnorePropertyDisplayName;
            NamedConstant category = c.HasCategory ? Named(c.CatInternal, c.CatDisplay) : null;
            NamedConstant property = Named(c.PropInternal, c.PropDisplay);
            SearchConditionComparison comparison = c.Test == "contains"
                ? SearchConditionComparison.DisplayStringContains
                : SearchConditionComparison.Equal;
            return new SearchCondition(category, property, options, comparison, VariantData.FromDisplayString(c.Value));
        }

        private static NamedConstant Named(string internalName, string displayName)
        {
            return string.IsNullOrEmpty(displayName) ? new NamedConstant(internalName) : new NamedConstant(internalName, displayName);
        }

        // ---- reading items ----

        private const string ElementCategory = "LcRevitData_Element";
        private const string CategoryProperty = "LcRevitPropertyElementCategory";

        private sealed class Info
        {
            public string Category = "UNKNOWN";
            public string Family = "UNKNOWN";
            public string Type = "UNKNOWN";
            public string Id = "UNKNOWN";
            public string Name = "";
            public string Class = "";
            public string ModelFile = "";
            public int Depth;
        }

        private static string Text(VariantData value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value.IsInt32)
            {
                return value.ToInt32().ToString(Inv);
            }

            if (value.IsDisplayString)
            {
                return value.ToDisplayString();
            }

            string all = value.ToString();
            int colon = all.IndexOf(':');
            return colon >= 0 ? all.Substring(colon + 1) : all;
        }

        private static string Prop(ModelItem at, string categoryDisplay, string propertyDisplay)
        {
            DataProperty p = at.PropertyCategories.FindPropertyByDisplayName(categoryDisplay, propertyDisplay);
            return p == null ? null : Text(p.Value);
        }

        private static Info Read(ModelItem item)
        {
            Info info = new Info();

            if (item == null)
            {
                info.Category = "NO ITEM";
                return info;
            }

            info.Name = item.DisplayName ?? string.Empty;
            info.Class = item.ClassDisplayName ?? string.Empty;

            try
            {
                ModelItem top = item;

                foreach (ModelItem a in item.Ancestors)
                {
                    top = a;
                }

                info.ModelFile = top.Model == null ? string.Empty : Path.GetFileName(top.Model.SourceFileName ?? string.Empty);
            }
            catch (Exception)
            {
                info.ModelFile = "UNKNOWN";
            }

            int depth = 0;

            foreach (ModelItem at in item.AncestorsAndSelf)
            {
                DataProperty cat = at.PropertyCategories.FindPropertyByName(ElementCategory, CategoryProperty);

                if (cat != null)
                {
                    info.Category = Text(cat.Value);
                    info.Family = Prop(at, "Element", "Family") ?? "UNKNOWN";
                    info.Type = Prop(at, "Element", "Type") ?? "UNKNOWN";
                    info.Id = Prop(at, "Element", "Id") ?? "UNKNOWN";
                    info.Depth = depth;
                    return info;
                }

                depth++;
            }

            return info;
        }

        private void SetCounts(Document document, SelectionSet set)
        {
            using (Selection selection = new Selection())
            {
                selection.SelectionSources.Add(document.SelectionSets.CreateSelectionSource(set));

                using (ModelItemCollection found = selection.GetSelectedItems(document))
                {
                    int geometry = 0;
                    Dictionary<string, int> byCategory = new Dictionary<string, int>();
                    Dictionary<string, int> byModel = new Dictionary<string, int>();

                    foreach (ModelItem item in found)
                    {
                        if (item.HasGeometry)
                        {
                            geometry++;
                        }

                        Info i = Read(item);
                        Bump(byCategory, i.Category);
                        Bump(byModel, i.ModelFile);
                    }

                    Say("  the set resolves to " + found.Count + " items, " + geometry + " with geometry of their own");
                    Say("  by category: " + Join(byCategory));
                    Say("  by model file: " + Join(byModel));
                }
            }
        }

        private static void Bump(Dictionary<string, int> d, string key)
        {
            int n;
            d.TryGetValue(key, out n);
            d[key] = n + 1;
        }

        private static string Join(Dictionary<string, int> d)
        {
            List<string> parts = new List<string>();

            foreach (KeyValuePair<string, int> kv in d)
            {
                parts.Add(kv.Key + " " + kv.Value);
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join("; ", parts.ToArray());
        }

        // ---- one test ----

        private void OneTest(Document document, DocumentSelectionSets sets, SelectionSet set, string setName, bool self, double tolerance)
        {
            string testName = "SAME " + setName + " self " + (self ? "on" : "off");
            Say("");
            Say("==== TEST " + testName + " ====");
            DocumentClashTests clashTests = document.GetClash().TestsData;
            int before = clashTests.Tests.Count;

            try
            {
                using (ClashTest test = new ClashTest())
                {
                    test.DisplayName = testName;
                    test.TestType = ClashTestType.HardConservative;
                    test.Tolerance = tolerance;
                    test.MergeComposites = true;

                    using (ClashSelection left = test.SelectionA)
                    using (ClashSelection right = test.SelectionB)
                    {
                        Fill(sets, left, set, self);
                        Fill(sets, right, set, self);
                    }

                    clashTests.TestsAddCopy(test);
                }
            }
            catch (Exception error)
            {
                Say("CREATE THREW " + error.GetType().Name + ": " + error.Message);
                return;
            }

            int after = clashTests.Tests.Count;
            Say("tests went from " + before + " to " + after + ", created without an error: " + (after == before + 1));

            if (after != before + 1)
            {
                return;
            }

            ClashTest saved = clashTests.Tests[before] as ClashTest;

            if (saved == null || saved.DisplayName != testName)
            {
                Say("UNKNOWN: the test at index " + before + " is not the one just added");
                return;
            }

            Say("saved test reads: type " + saved.TestType + ", tolerance " + saved.Tolerance.ToString("R", Inv) + ", merge composites " + saved.MergeComposites
                + ", status " + saved.Status);

            using (ClashSelection a = saved.SelectionA)
            using (ClashSelection b = saved.SelectionB)
            {
                Say("  side A self intersect " + a.SelfIntersect + ", primitive types " + a.PrimitiveTypes + ", side B self intersect " + b.SelfIntersect + ", primitive types " + b.PrimitiveTypes);
                Say("  items on side A " + CountSide(document, a) + ", on side B " + CountSide(document, b));
            }

            Exception ran = null;
            DateTime t0 = DateTime.UtcNow;

            try
            {
                clashTests.TestsRunTest(saved);
            }
            catch (Exception error)
            {
                ran = error;
            }

            double seconds = (DateTime.UtcNow - t0).TotalSeconds;
            Say("TestsRunTest " + (ran == null ? "returned" : "THREW " + ran.GetType().Name + ": " + ran.Message) + " after " + seconds.ToString("0.00", Inv) + " s");

            ClashTest done = clashTests.Tests[before] as ClashTest;
            Say("test status after the run: " + (done == null ? "UNKNOWN" : done.Status.ToString()) + ", last run " + (done == null || done.LastRun == null ? "UNKNOWN" : "set"));

            if (done != null)
            {
                Read(done, testName);
            }
        }

        private static int CountSide(Document document, ClashSelection side)
        {
            using (Selection selection = side.Selection)
            using (ModelItemCollection found = selection.GetSelectedItems(document))
            {
                return found == null ? 0 : found.Count;
            }
        }

        private static void Fill(DocumentSelectionSets sets, ClashSelection side, SelectionSet set, bool self)
        {
            side.SelfIntersect = self;
            side.PrimitiveTypes = (PrimitiveTypes)1;

            using (Selection selection = side.Selection)
            {
                selection.Clear();
                selection.SelectionSources.Add(sets.CreateSelectionSource(set));
            }
        }

        // ---- the results ----

        private sealed class Row
        {
            public string Result;
            public string Status;
            public double Distance;
            public ModelItem I1;
            public ModelItem I2;
            public ModelItem C1;
            public ModelItem C2;
            public Info A;
            public Info B;
            public int Id1;
            public int Id2;
            public bool Group;
        }

        private void Read(ClashTest test, string testName)
        {
            List<Row> rows = new List<Row>();
            int groups = 0;
            Walk(test.Children, rows, ref groups, false);
            Say("results walked: " + rows.Count + " clash results, " + groups + " groups");

            if (rows.Count == 0)
            {
                return;
            }

            // Each distinct item gets a number. InstanceHashCode buckets it and Equals decides.
            Dictionary<int, List<KeyValuePair<ModelItem, int>>> buckets = new Dictionary<int, List<KeyValuePair<ModelItem, int>>>();
            int next = 0;

            foreach (Row r in rows)
            {
                r.Id1 = Number(buckets, r.I1, ref next);
                r.Id2 = Number(buckets, r.I2, ref next);
                r.A = Read(r.I1);
                r.B = Read(r.I2);
            }

            int sameItem = 0;
            int sameElementId = 0;
            int sameEquals = 0;

            foreach (Row r in rows)
            {
                if (r.Id1 == r.Id2)
                {
                    sameItem++;
                }

                if (r.I1 != null && r.I2 != null && r.I1.Equals(r.I2))
                {
                    sameEquals++;
                }

                if (r.A.Id != "UNKNOWN" && r.A.Id == r.B.Id && r.A.ModelFile == r.B.ModelFile)
                {
                    sameElementId++;
                }
            }

            Say("Q2 result with the same item on both sides: by distinct item number " + sameItem + ", by ModelItem.Equals " + sameEquals + ", by the same Revit element id in the same model " + sameElementId);

            Dictionary<string, int> orderedKey = new Dictionary<string, int>();
            Dictionary<string, int> unorderedKey = new Dictionary<string, int>();

            foreach (Row r in rows)
            {
                Bump(orderedKey, r.Id1 + ">" + r.Id2);
                Bump(unorderedKey, Math.Min(r.Id1, r.Id2) + "-" + Math.Max(r.Id1, r.Id2));
            }

            int sameOrderRepeats = 0;

            foreach (KeyValuePair<string, int> kv in orderedKey)
            {
                if (kv.Value > 1)
                {
                    sameOrderRepeats += kv.Value - 1;
                }
            }

            int swapped = 0;
            int unorderedRepeats = 0;

            foreach (KeyValuePair<string, int> kv in unorderedKey)
            {
                if (kv.Value > 1)
                {
                    unorderedRepeats += kv.Value - 1;
                }
            }

            foreach (Row r in rows)
            {
                if (r.Id1 != r.Id2 && orderedKey.ContainsKey(r.Id2 + ">" + r.Id1))
                {
                    swapped++;
                }
            }

            Say("Q3 distinct ordered pairs " + orderedKey.Count + ", distinct unordered pairs " + unorderedKey.Count + ", results repeating an ordered pair " + sameOrderRepeats
                + ", results whose swapped pair (B,A) is also a result " + swapped + ", results repeating an unordered pair " + unorderedRepeats);

            Dictionary<string, int> pairs = new Dictionary<string, int>();
            Dictionary<string, int> kinds = new Dictionary<string, int>();
            Dictionary<string, int> status = new Dictionary<string, int>();
            Dictionary<string, int> families = new Dictionary<string, int>();

            foreach (Row r in rows)
            {
                string a = r.A.Category;
                string b = r.B.Category;
                string lo = string.CompareOrdinal(a, b) <= 0 ? a : b;
                string hi = string.CompareOrdinal(a, b) <= 0 ? b : a;
                Bump(pairs, lo + " | " + hi);
                Bump(kinds, Kind(a, b));
                Bump(status, r.Status);
                Bump(families, r.A.Family + " | " + r.B.Family);
            }

            Say("Q5 by category pair, unordered: " + Join(pairs));
            Say("Q5 by kind: " + Join(kinds));
            Say("result statuses: " + Join(status));
            Say("family pairs (side 1 | side 2): " + Join(families));

            string path = Path.Combine(rowsFolder, Safe(testName) + ".tsv");

            using (StreamWriter rowsOut = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                rowsOut.WriteLine("result\tstatus\tdistance\tgroup\titem1 no\titem2 no\tcat1\tcat2\tfamily1\tfamily2\ttype1\ttype2\tid1\tid2\tmodel1\tmodel2\tname1\tname2\tclass1\tclass2\titem1 is item2");

                foreach (Row r in rows)
                {
                    rowsOut.WriteLine(string.Join("\t", new[]
                    {
                        r.Result, r.Status, r.Distance.ToString("R", Inv), r.Group ? "yes" : "no", r.Id1.ToString(Inv), r.Id2.ToString(Inv),
                        r.A.Category, r.B.Category, r.A.Family, r.B.Family, r.A.Type, r.B.Type, r.A.Id, r.B.Id, r.A.ModelFile, r.B.ModelFile,
                        r.A.Name, r.B.Name, r.A.Class, r.B.Class, r.Id1 == r.Id2 ? "yes" : "no"
                    }));
                }
            }

            Say("rows file: " + path);
            Say("first 12 results:");

            for (int i = 0; i < rows.Count && i < 12; i++)
            {
                Row r = rows[i];
                Say("  " + r.Result + " [" + r.Status + ", " + r.Distance.ToString("0.0000", Inv) + "] item " + r.Id1 + " " + r.A.Category + " / " + r.A.Family + " / " + r.A.Type + " id " + r.A.Id
                    + "  VS  item " + r.Id2 + " " + r.B.Category + " / " + r.B.Family + " / " + r.B.Type + " id " + r.B.Id);
            }

            // One element node's own properties, so a later reader can see what 'Family' and 'Type' were read from.
            Row first = rows[0];
            DumpElement(first.I1, "item 1 of the first result");
        }

        private void DumpElement(ModelItem item, string what)
        {
            if (item == null)
            {
                return;
            }

            foreach (ModelItem at in item.AncestorsAndSelf)
            {
                DataProperty cat = at.PropertyCategories.FindPropertyByName(ElementCategory, CategoryProperty);

                if (cat == null)
                {
                    continue;
                }

                PropertyCategory element = at.PropertyCategories.FindCategoryByName(ElementCategory);
                Say("properties of the element node of " + what + ", node '" + at.DisplayName + "', class " + at.ClassDisplayName + ":");

                foreach (DataProperty p in element.Properties)
                {
                    Say("    " + p.DisplayName + " (" + p.Name + ") = " + Text(p.Value));
                }

                return;
            }
        }

        private static string Kind(string a, string b)
        {
            if (a == "UNKNOWN" || b == "UNKNOWN" || a == "NO ITEM" || b == "NO ITEM")
            {
                return "category UNKNOWN";
            }

            if (a == b)
            {
                return a.EndsWith("Fittings", StringComparison.Ordinal) ? "fitting vs fitting of the same category" : "run vs run of the same category";
            }

            if (b == Singular(a) + " Fittings" || a == Singular(b) + " Fittings")
            {
                return "run vs a fitting of its own run";
            }

            if (a.EndsWith("Fittings", StringComparison.Ordinal) && b.EndsWith("Fittings", StringComparison.Ordinal))
            {
                return "fitting vs fitting of another category";
            }

            return "other pair";
        }

        private static string Singular(string c)
        {
            return c.EndsWith("s", StringComparison.Ordinal) ? c.Substring(0, c.Length - 1) : c;
        }

        private static string Safe(string name)
        {
            StringBuilder b = new StringBuilder();

            foreach (char c in name)
            {
                b.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return b.ToString();
        }

        private static int Number(Dictionary<int, List<KeyValuePair<ModelItem, int>>> buckets, ModelItem item, ref int next)
        {
            if (item == null)
            {
                return -1;
            }

            int hash = item.InstanceHashCode;
            List<KeyValuePair<ModelItem, int>> list;

            if (!buckets.TryGetValue(hash, out list))
            {
                list = new List<KeyValuePair<ModelItem, int>>();
                buckets[hash] = list;
            }

            foreach (KeyValuePair<ModelItem, int> kv in list)
            {
                if (kv.Key.Equals(item))
                {
                    return kv.Value;
                }
            }

            list.Add(new KeyValuePair<ModelItem, int>(item, next));
            next++;
            return next - 1;
        }

        private static void Walk(SavedItemCollection children, List<Row> rows, ref int groups, bool inAGroup)
        {
            foreach (SavedItem child in children)
            {
                ClashResultGroup group = child as ClashResultGroup;

                if (group != null)
                {
                    groups++;
                    Walk(group.Children, rows, ref groups, true);
                    continue;
                }

                ClashResult result = child as ClashResult;

                if (result == null)
                {
                    continue;
                }

                Row r = new Row();
                r.Result = result.DisplayName;
                r.Status = result.Status.ToString();
                r.Distance = result.Distance;
                r.I1 = result.Item1;
                r.I2 = result.Item2;
                r.C1 = result.CompositeItem1;
                r.C2 = result.CompositeItem2;
                r.Group = inAGroup;
                rows.Add(r);
            }
        }
    }
}
