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

namespace GenericProbe
{
    /// <summary>
    /// F128, generic models, 2026-10-08. Which property and which value name a Generic Models
    /// item in 1A02MM and 1A04PK, what its Source File property holds, and how many such items
    /// each model of the building holds. Read only: the document is cleared, the NWC copies of
    /// one building are appended, every item of every model is walked, and nothing is saved,
    /// published or changed in any model.
    ///
    /// Three readings, so each fact is read two ways where it can be:
    /// 1. THE WALK. Model.RootItem.DescendantsAndSelf of every model, every tab and every
    ///    property of every item, read by kind the way ClashHarvest.Text reads one. An item is
    ///    a hit when any property's text equals the value, exactly, after a trim, without case,
    ///    or holds it, and each hit says which tab, which property and which of the four.
    /// 2. THE ELEMENT TAB. The one property the client's file asks, LcRevitData_Element with
    ///    LcRevitPropertyElementCategory, read on its own on every item, so whether the value
    ///    sits there is a fact of its own and not an inference from the walk.
    /// 3. THE SEARCHES. The exact two conditions GenericModelsPlan writes, built the way
    ///    SetBuilder.BuildCondition builds them, the category equal and the Source File
    ///    contains a text, run through Search.FindAll over the whole document, once with the
    ///    category alone and once per model with each candidate text, so the count the add-in's
    ///    set would show is Navisworks's own number.
    ///
    /// The model an item lives in is read off its topmost ancestor's Model.FileName, scan.md 5n,
    /// and the count by that is written beside the count by the model whose root was walked, so
    /// the two can be seen to agree or not.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Generic models probe", ToolTip = "F128, which property and value name Generic Models")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class GenericProbePlugin : AddInPlugin
    {
        public const string PluginName = "GenericProbe";
        public const string DeveloperCode = "PARS";

        private const string ElementTab = "LcRevitData_Element";
        private const string ElementTabDisplay = "Element";
        private const string CategoryProperty = "LcRevitPropertyElementCategory";
        private const string CategoryPropertyDisplay = "Category";
        private const string SourceFileProperty = "LcOaNodeSourceFile";
        private const string SourceFilePropertyDisplay = "Source File";
        private const int DumpItemsPerModel = 2;
        private const int DistinctCap = 25;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private StreamWriter results;
        private string categoryValue;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 5 || parameters[0] != "generic")
            {
                return 2;
            }

            using (results = new StreamWriter(parameters[1], true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode " + parameters[0] + ", building " + parameters[4]);

                try
                {
                    int code = Measure(parameters[2], parameters[3], parameters[4]);
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

        // ---------- one building ----------

        private int Measure(string folder, string value, string building)
        {
            categoryValue = value;
            Say("the value asked: [" + value + "], " + value.Length + " characters, compared Ordinal");
            Say("the folder of copies " + folder);

            if (!Directory.Exists(folder))
            {
                Say("UNKNOWN: the folder of copies is not there");
                return 3;
            }

            string[] files = Directory.GetFiles(folder, "*.nwc");
            Array.Sort(files, StringComparer.Ordinal);
            Say("NWC copies in it: " + files.Length);

            if (files.Length == 0)
            {
                Say("UNKNOWN: no NWC to append");
                return 3;
            }

            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return 4;
            }

            Say("models before the clear " + document.Models.Count + ", IsClear " + document.IsClear);
            document.Clear();
            Say("models after the clear " + document.Models.Count + ", IsClear " + document.IsClear);

            int appended = 0;

            foreach (string file in files)
            {
                Stopwatch one = Stopwatch.StartNew();
                bool ok = document.TryAppendFile(file);
                one.Stop();
                Say("TryAppendFile " + Path.GetFileName(file) + " returned " + ok + " after " + one.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s, models now " + document.Models.Count);

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

            Say("appended " + appended + " of " + files.Length + ", models after the appends " + document.Models.Count + ", waited " + wait.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s");

            if (document.Models.Count == 0)
            {
                Say("UNKNOWN: no model in the document, nothing was read");
                return 5;
            }

            List<ModelFacts> models = new List<ModelFacts>();

            for (int i = 0; i < document.Models.Count; i++)
            {
                Model model = document.Models[i];
                ModelFacts facts = new ModelFacts(i, model.FileName ?? string.Empty, model.SourceFileName ?? string.Empty);
                models.Add(facts);
                Say("model " + i + " FileName " + facts.FileName + ", stem " + facts.Stem + ", SourceFileName " + facts.SourceFileName + ", its file name " + facts.SourceName + ", its stem " + facts.SourceStem);
            }

            Say("");
            Say("==== THE SEARCHES, the plan's own conditions, built as SetBuilder.BuildCondition builds them ====");
            MeasureSearches(document, models);

            Say("");
            Say("==== THE WALK, every tab and every property of every item of every model ====");
            Dictionary<string, Tally> byTop = new Dictionary<string, Tally>(StringComparer.OrdinalIgnoreCase);

            foreach (ModelFacts facts in models)
            {
                WalkModel(document.Models[facts.Index], facts, byTop);
            }

            Say("");
            Say("==== THE COUNTS, by the model whose root was walked, then by the topmost ancestor's Model.FileName ====");

            foreach (ModelFacts facts in models)
            {
                ReportModel(facts);
            }

            Say("");
            Say("---- by the topmost ancestor's Model.FileName, every item whose Element tab Category reads the value exactly ----");

            foreach (KeyValuePair<string, Tally> pair in byTop)
            {
                Say("  " + Path.GetFileName(pair.Key) + "  items " + pair.Value.ExactElement + "  of which elements, whose parent does not read it, " + pair.Value.ExactElementTop);
            }

            if (byTop.Count == 0)
            {
                Say("  none");
            }

            Say("");
            Say("nothing in any model was changed, nothing was saved and nothing was published");
            return 0;
        }

        // ---------- the searches ----------

        private void MeasureSearches(Document document, List<ModelFacts> models)
        {
            int whole = CountSearch(document, null, "category alone, whole document");
            Say("  SEARCH category alone finds " + whole + " items over the whole document");

            using (Search search = new Search())
            {
                search.Selection.SelectAll();
                search.Locations = SearchLocations.DescendantsAndSelf;
                search.SearchConditions.Add(CategoryCondition());

                using (ModelItemCollection found = search.FindAll(document, false))
                {
                    Dictionary<string, int> perTop = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    int noTop = 0;

                    foreach (ModelItem item in found)
                    {
                        string top = TopModelFile(item);

                        if (top.Length == 0)
                        {
                            noTop++;
                            continue;
                        }

                        int n;
                        perTop.TryGetValue(top, out n);
                        perTop[top] = n + 1;
                    }

                    foreach (KeyValuePair<string, int> pair in perTop)
                    {
                        Say("    found by the search, by the topmost ancestor's Model.FileName: " + Path.GetFileName(pair.Key) + "  " + pair.Value);
                    }

                    if (noTop > 0)
                    {
                        Say("    found by the search with no topmost model: " + noTop);
                    }
                }
            }

            foreach (ModelFacts facts in models)
            {
                string name = Path.GetFileName(facts.FileName);
                facts.SearchStem = CountSearch(document, facts.Stem, name + " with Source File contains the NWC stem");
                Say("  SEARCH " + name + ": category AND Source File contains [" + facts.Stem + "], the NWC's stem: " + facts.SearchStem);
                facts.SearchNwcName = CountSearch(document, Path.GetFileName(facts.FileName), name + " with Source File contains the NWC name");
                Say("  SEARCH " + name + ": category AND Source File contains [" + Path.GetFileName(facts.FileName) + "], the NWC's name: " + facts.SearchNwcName);

                if (facts.SourceName.Length > 0)
                {
                    facts.SearchSourceName = CountSearch(document, facts.SourceName, name + " with Source File contains the model's SourceFileName's file name");
                    Say("  SEARCH " + name + ": category AND Source File contains [" + facts.SourceName + "], the file name of Model.SourceFileName: " + facts.SearchSourceName);
                }
                else
                {
                    Say("  SEARCH " + name + ": Model.SourceFileName is empty, so no search with its file name");
                }

                if (facts.SourceStem.Length > 0 && !string.Equals(facts.SourceStem, facts.Stem, StringComparison.Ordinal))
                {
                    facts.SearchSourceStem = CountSearch(document, facts.SourceStem, name + " with Source File contains the model's SourceFileName's stem");
                    Say("  SEARCH " + name + ": category AND Source File contains [" + facts.SourceStem + "], the stem of Model.SourceFileName: " + facts.SearchSourceStem);
                }
            }
        }

        private int CountSearch(Document document, string containsText, string what)
        {
            try
            {
                using (Search search = new Search())
                {
                    search.Selection.SelectAll();
                    search.Locations = SearchLocations.DescendantsAndSelf;
                    search.SearchConditions.Add(CategoryCondition());

                    if (containsText != null)
                    {
                        search.SearchConditions.Add(SourceFileCondition(containsText));
                    }

                    Stopwatch clock = Stopwatch.StartNew();

                    using (ModelItemCollection found = search.FindAll(document, false))
                    {
                        clock.Stop();
                        int n = found == null ? -1 : found.Count;
                        Say("    search " + what + " took " + clock.Elapsed.TotalSeconds.ToString("0.000", Inv) + " s");
                        return n;
                    }
                }
            }
            catch (Exception error)
            {
                Say("    search " + what + " THREW " + error.GetType().Name + ": " + error.Message);
                return -1;
            }
        }

        private SearchCondition CategoryCondition()
        {
            SearchConditionOptions options = SearchConditionOptions.IgnoreCategoryDisplayName | SearchConditionOptions.IgnorePropertyDisplayName;
            return new SearchCondition(
                new NamedConstant(ElementTab, ElementTabDisplay),
                new NamedConstant(CategoryProperty, CategoryPropertyDisplay),
                options,
                SearchConditionComparison.Equal,
                VariantData.FromDisplayString(categoryValue));
        }

        private static SearchCondition SourceFileCondition(string text)
        {
            SearchConditionOptions options = SearchConditionOptions.IgnoreCategoryDisplayName | SearchConditionOptions.IgnorePropertyDisplayName;
            return new SearchCondition(
                null,
                new NamedConstant(SourceFileProperty, SourceFilePropertyDisplay),
                options,
                SearchConditionComparison.DisplayStringContains,
                VariantData.FromDisplayString(text));
        }

        // ---------- the walk ----------

        private void WalkModel(Model model, ModelFacts facts, Dictionary<string, Tally> byTop)
        {
            string name = Path.GetFileName(facts.FileName);
            Say("---- walking " + name + " ----");
            Stopwatch clock = Stopwatch.StartNew();
            ModelItem root = model.RootItem;

            if (root == null)
            {
                Say("  UNKNOWN: RootItem is null, nothing walked");
                return;
            }

            foreach (ModelItem item in root.DescendantsAndSelf)
            {
                facts.Items++;

                try
                {
                    ReadItem(item, facts, byTop);
                }
                catch (Exception error)
                {
                    facts.ItemsThatThrew++;

                    if (facts.ItemsThatThrew <= 3)
                    {
                        Say("  item " + facts.Items + " threw " + error.GetType().Name + ": " + error.Message);
                    }
                }

                if (facts.Items % 10000 == 0)
                {
                    Say("  " + facts.Items + " items so far, " + clock.Elapsed.TotalSeconds.ToString("0.0", Inv) + " s");
                }
            }

            clock.Stop();
            Say("  walked " + facts.Items + " items in " + clock.Elapsed.TotalSeconds.ToString("0.0", Inv) + " s, items that threw " + facts.ItemsThatThrew);
        }

        private void ReadItem(ModelItem item, ModelFacts facts, Dictionary<string, Tally> byTop)
        {
            ItemRead read = new ItemRead();
            List<string> dump = facts.Dumped < DumpItemsPerModel ? new List<string>() : null;

            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    return;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    using (tab)
                    {
                        string tabName = tab.Name ?? string.Empty;
                        string tabDisplay = tab.DisplayName ?? string.Empty;

                        if (string.Equals(tabName, ElementTab, StringComparison.Ordinal))
                        {
                            read.HasElementTab = true;
                        }

                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            if (properties == null)
                            {
                                continue;
                            }

                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    string propName = property.Name ?? string.Empty;
                                    string propDisplay = property.DisplayName ?? string.Empty;
                                    string kind;
                                    string text = Text(property, out kind);

                                    if (dump != null)
                                    {
                                        dump.Add("      [" + tabName + " | " + tabDisplay + "] [" + propName + " | " + propDisplay + "] " + kind + " = " + Shown(text));
                                    }

                                    string where = tabName + " | " + tabDisplay + " > " + propName + " | " + propDisplay + " (" + kind + ")";

                                    if (string.Equals(tabName, ElementTab, StringComparison.Ordinal) && string.Equals(propName, CategoryProperty, StringComparison.Ordinal))
                                    {
                                        read.ElementCategory = text;
                                        read.ElementCategoryKind = kind;
                                    }

                                    string how = Match(text);

                                    if (how != null)
                                    {
                                        read.Hits.Add(where + " " + how + " [" + text + "]");
                                    }

                                    if (string.Equals(propName, SourceFileProperty, StringComparison.Ordinal) || string.Equals(propDisplay, SourceFilePropertyDisplay, StringComparison.OrdinalIgnoreCase))
                                    {
                                        read.SourceFiles.Add(new KeyValuePair<string, string>(where, text));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (read.HasElementTab)
            {
                facts.ItemsWithElementTab++;
            }

            if (read.SourceFiles.Count > 0)
            {
                facts.ItemsWithSourceFile++;

                foreach (KeyValuePair<string, string> sf in read.SourceFiles)
                {
                    facts.AllSourceFiles.Add(sf.Key + " = " + sf.Value);
                }
            }

            bool exact = read.ElementCategory != null && string.Equals(read.ElementCategory, categoryValue, StringComparison.Ordinal);
            bool loose = !exact && read.ElementCategory != null && string.Equals(read.ElementCategory.Trim(), categoryValue.Trim(), StringComparison.OrdinalIgnoreCase);

            foreach (string hit in read.Hits)
            {
                facts.HitsAnywhere.Add(hit.Substring(0, hit.IndexOf(" [", StringComparison.Ordinal)));
            }

            if (read.Hits.Count > 0 && !exact && !loose)
            {
                facts.ItemsHitElsewhereOnly++;

                if (facts.ItemsHitElsewhereOnly <= 3)
                {
                    Say("  item [" + (item.DisplayName ?? string.Empty) + "] class " + (item.ClassDisplayName ?? string.Empty) + " reads the value on another property and not on the Element tab's Category, which reads " + (read.ElementCategory == null ? "absent" : "[" + read.ElementCategory + "]"));

                    foreach (string hit in read.Hits)
                    {
                        Say("      " + hit);
                    }
                }
            }

            if (loose)
            {
                facts.LooseElement++;

                if (facts.LooseElement <= 3)
                {
                    Say("  item [" + (item.DisplayName ?? string.Empty) + "] Element tab Category reads [" + read.ElementCategory + "], " + read.ElementCategory.Length + " characters, equal to the value only after a trim or without case");
                }
            }

            if (!exact)
            {
                return;
            }

            facts.ExactElement++;
            facts.ExactElementKinds.Add(read.ElementCategoryKind);
            bool parentExact = ParentReadsExact(item);

            if (!parentExact)
            {
                facts.ExactElementTop++;
            }

            if (item.HasGeometry)
            {
                facts.ExactWithGeometry++;
            }

            if (item.IsComposite)
            {
                facts.ExactComposite++;
            }

            string top = TopModelFile(item);
            Tally tally;

            if (!byTop.TryGetValue(top, out tally))
            {
                tally = new Tally();
                byTop[top] = tally;
            }

            tally.ExactElement++;

            if (!parentExact)
            {
                tally.ExactElementTop++;
            }

            if (!string.Equals(top, facts.FileName, StringComparison.OrdinalIgnoreCase))
            {
                facts.ExactInAnotherTop++;
            }

            if (read.SourceFiles.Count == 0)
            {
                facts.ExactNoSourceFile++;
            }
            else
            {
                foreach (KeyValuePair<string, string> sf in read.SourceFiles)
                {
                    facts.ExactSourceFiles.Add(sf.Key + " = " + sf.Value);
                    facts.ExactSourceFileHolds.Add(Holds(sf.Value, facts));
                }

                if (read.SourceFiles.Count > 1)
                {
                    facts.ExactManySourceFiles++;
                }
            }

            if (dump != null)
            {
                facts.Dumped++;
                Say("  ITEM " + facts.Dumped + " of " + Path.GetFileName(facts.FileName) + " whose Element tab Category reads the value exactly: [" + (item.DisplayName ?? string.Empty) + "] class " + (item.ClassDisplayName ?? string.Empty) + ", IsComposite " + item.IsComposite + ", HasGeometry " + item.HasGeometry + ", parent reads it too " + parentExact + ", topmost ancestor's model " + Path.GetFileName(top));

                foreach (string line in dump)
                {
                    Say(line);
                }
            }
        }

        private bool ParentReadsExact(ModelItem item)
        {
            ModelItem parent = item.Parent;

            if (parent == null)
            {
                return false;
            }

            try
            {
                using (PropertyCategoryCollection tabs = parent.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return false;
                    }

                    using (PropertyCategory tab = tabs.FindCategoryByName(ElementTab))
                    {
                        if (tab == null)
                        {
                            return false;
                        }

                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            if (properties == null)
                            {
                                return false;
                            }

                            using (DataProperty property = properties.FindPropertyByName(CategoryProperty))
                            {
                                if (property == null)
                                {
                                    return false;
                                }

                                string kind;
                                return string.Equals(Text(property, out kind), categoryValue, StringComparison.Ordinal);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string TopModelFile(ModelItem item)
        {
            ModelItem top = item;

            while (top.Parent != null)
            {
                top = top.Parent;
            }

            if (!top.HasModel || top.Model == null)
            {
                return string.Empty;
            }

            return top.Model.FileName ?? string.Empty;
        }

        private string Match(string text)
        {
            if (text.Length == 0)
            {
                return null;
            }

            if (string.Equals(text, categoryValue, StringComparison.Ordinal))
            {
                return "EQUALS exactly";
            }

            if (string.Equals(text.Trim(), categoryValue.Trim(), StringComparison.Ordinal))
            {
                return "equals after a trim, " + text.Length + " characters";
            }

            if (string.Equals(text.Trim(), categoryValue.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return "equals without case";
            }

            if (text.IndexOf(categoryValue, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "holds the value, " + text.Length + " characters";
            }

            return null;
        }

        private static string Holds(string sourceFile, ModelFacts facts)
        {
            List<string> holds = new List<string>();

            if (facts.Stem.Length > 0 && sourceFile.IndexOf(facts.Stem, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                holds.Add("the NWC stem");
            }

            if (sourceFile.IndexOf(Path.GetFileName(facts.FileName), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                holds.Add("the NWC name with .nwc");
            }

            if (facts.SourceName.Length > 0 && sourceFile.IndexOf(facts.SourceName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                holds.Add("the file name of Model.SourceFileName");
            }

            if (string.Equals(sourceFile, facts.SourceFileName, StringComparison.Ordinal))
            {
                holds.Add("is Model.SourceFileName exactly");
            }

            if (string.Equals(sourceFile, facts.FileName, StringComparison.Ordinal))
            {
                holds.Add("is Model.FileName exactly");
            }

            if (holds.Count == 0)
            {
                return "holds neither the NWC stem nor the file name of Model.SourceFileName";
            }

            return string.Join(", ", holds.ToArray());
        }

        // ---------- the report per model ----------

        private void ReportModel(ModelFacts facts)
        {
            string name = Path.GetFileName(facts.FileName);
            Say("MODEL " + facts.Index + "  " + name);
            Say("  items walked " + facts.Items + ", with an Element tab " + facts.ItemsWithElementTab + ", with a Source File property " + facts.ItemsWithSourceFile + ", that threw " + facts.ItemsThatThrew);
            Say("  Element tab Category reads the value exactly: " + facts.ExactElement + " items, of which elements, whose parent does not read it, " + facts.ExactElementTop + ", composite " + facts.ExactComposite + ", with geometry " + facts.ExactWithGeometry + ", whose topmost model is another file " + facts.ExactInAnotherTop);
            Say("  the kinds that value was read as: " + Join(facts.ExactElementKinds));
            Say("  Element tab Category equal only after a trim or without case: " + facts.LooseElement);
            Say("  items reading the value on some other property and not on the Element tab's Category: " + facts.ItemsHitElsewhereOnly);
            Say("  every tab and property the value was read on, over all items, with how:");

            foreach (string where in facts.HitsAnywhere.Lines())
            {
                Say("      " + where);
            }

            if (facts.HitsAnywhere.Count == 0)
            {
                Say("      none");
            }

            Say("  searches: category AND Source File contains the NWC stem " + facts.SearchStem + ", the NWC name " + facts.SearchNwcName + ", the file name of Model.SourceFileName " + Shown(facts.SearchSourceName) + ", its stem where it differs " + Shown(facts.SearchSourceStem));
            Say("  of the " + facts.ExactElement + " exact items: with no Source File property at all " + facts.ExactNoSourceFile + ", with more than one " + facts.ExactManySourceFiles);
            Say("  the Source File texts of the exact items, tab | display > property | display (kind) = value, with counts:");

            foreach (string line in facts.ExactSourceFiles.Lines())
            {
                Say("      " + line);
                Say("        shape: " + Shape(ValueOf(line)));
            }

            if (facts.ExactSourceFiles.Count == 0)
            {
                Say("      none");
            }

            Say("  what each of those texts holds: " + Join(facts.ExactSourceFileHolds));
            Say("  the Source File texts over ALL items of the model, with counts, capped at " + DistinctCap + " distinct:");

            foreach (string line in facts.AllSourceFiles.Lines())
            {
                Say("      " + line);
            }

            if (facts.AllSourceFiles.Count == 0)
            {
                Say("      none");
            }

            Say("  distinct Source File texts over all items: " + facts.AllSourceFiles.Count + (facts.AllSourceFiles.Capped ? " or more, the cap was reached" : ""));
        }

        // ---------- helpers ----------

        /// <summary>A value as text by its kind, the way ClashHarvest.Text reads one, never throwing.</summary>
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
                        case VariantDataType.Point3D:
                            Point3D p3 = value.ToPoint3D();
                            return p3.X.ToString("R", Inv) + " " + p3.Y.ToString("R", Inv) + " " + p3.Z.ToString("R", Inv);
                        case VariantDataType.Point2D:
                            Point2D p2 = value.ToPoint2D();
                            return p2.X.ToString("R", Inv) + " " + p2.Y.ToString("R", Inv);
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

        /// <summary>The shape of a text without its words, so a path's form is known even where the line is masked.</summary>
        private static string Shape(string text)
        {
            int backslashes = 0;
            int slashes = 0;

            foreach (char c in text)
            {
                if (c == '\\')
                {
                    backslashes++;
                }
                else if (c == '/')
                {
                    slashes++;
                }
            }

            bool drive = text.Length >= 3 && char.IsLetter(text[0]) && text[1] == ':' && (text[2] == '\\' || text[2] == '/');
            string last = text;
            int cut = Math.Max(text.LastIndexOf('\\'), text.LastIndexOf('/'));

            if (cut >= 0 && cut < text.Length - 1)
            {
                last = text.Substring(cut + 1);
            }

            return text.Length + " characters, backslashes " + backslashes + ", slashes " + slashes + ", starts with a drive letter " + drive + ", holds :// " + (text.IndexOf("://", StringComparison.Ordinal) >= 0) + ", extension [" + Path.GetExtension(last) + "], last segment [" + last + "]";
        }

        private static string ValueOf(string tallyLine)
        {
            int at = tallyLine.IndexOf(" = ", StringComparison.Ordinal);
            int end = tallyLine.LastIndexOf("  x", StringComparison.Ordinal);

            if (at < 0)
            {
                return tallyLine;
            }

            if (end > at)
            {
                return tallyLine.Substring(at + 3, end - at - 3);
            }

            return tallyLine.Substring(at + 3);
        }

        private static string Shown(string text)
        {
            return text.Length == 0 ? "(empty)" : text;
        }

        private static string Shown(int n)
        {
            return n == int.MinValue ? "not run" : n.ToString(Inv);
        }

        private static string Join(Distinct d)
        {
            if (d.Count == 0)
            {
                return "none";
            }

            return string.Join(", ", d.Lines().ToArray());
        }

        private void Say(string line)
        {
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", Inv) + "  " + line);
        }

        // ---------- the tallies ----------

        private sealed class ItemRead
        {
            public bool HasElementTab;
            public string ElementCategory;
            public string ElementCategoryKind = string.Empty;
            public readonly List<string> Hits = new List<string>();
            public readonly List<KeyValuePair<string, string>> SourceFiles = new List<KeyValuePair<string, string>>();
        }

        private sealed class Tally
        {
            public int ExactElement;
            public int ExactElementTop;
        }

        private sealed class ModelFacts
        {
            public ModelFacts(int index, string fileName, string sourceFileName)
            {
                Index = index;
                FileName = fileName;
                SourceFileName = sourceFileName;
                Stem = SafeStem(fileName);
                SourceName = SafeName(sourceFileName);
                SourceStem = SafeStem(sourceFileName);
            }

            public int Index;
            public string FileName;
            public string SourceFileName;
            public string Stem;
            public string SourceName;
            public string SourceStem;
            public int Items;
            public int ItemsThatThrew;
            public int ItemsWithElementTab;
            public int ItemsWithSourceFile;
            public int ExactElement;
            public int ExactElementTop;
            public int ExactComposite;
            public int ExactWithGeometry;
            public int ExactInAnotherTop;
            public int ExactNoSourceFile;
            public int ExactManySourceFiles;
            public int LooseElement;
            public int ItemsHitElsewhereOnly;
            public int Dumped;
            public int SearchStem = int.MinValue;
            public int SearchNwcName = int.MinValue;
            public int SearchSourceName = int.MinValue;
            public int SearchSourceStem = int.MinValue;
            public readonly Distinct ExactElementKinds = new Distinct(DistinctCap);
            public readonly Distinct HitsAnywhere = new Distinct(DistinctCap);
            public readonly Distinct ExactSourceFiles = new Distinct(DistinctCap);
            public readonly Distinct ExactSourceFileHolds = new Distinct(DistinctCap);
            public readonly Distinct AllSourceFiles = new Distinct(DistinctCap);

            private static string SafeStem(string path)
            {
                try
                {
                    return Path.GetFileNameWithoutExtension(path) ?? string.Empty;
                }
                catch (Exception)
                {
                    int cut = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
                    string last = cut >= 0 ? path.Substring(cut + 1) : path;
                    int dot = last.LastIndexOf('.');
                    return dot > 0 ? last.Substring(0, dot) : last;
                }
            }

            private static string SafeName(string path)
            {
                try
                {
                    return Path.GetFileName(path) ?? string.Empty;
                }
                catch (Exception)
                {
                    int cut = Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
                    return cut >= 0 ? path.Substring(cut + 1) : path;
                }
            }
        }

        /// <summary>Distinct texts with counts, in first seen order, capped so a model of many values cannot flood the record.</summary>
        private sealed class Distinct
        {
            private readonly int cap;
            private readonly List<string> order = new List<string>();
            private readonly Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            public Distinct(int cap)
            {
                this.cap = cap;
            }

            public bool Capped;

            public int Count
            {
                get { return order.Count; }
            }

            public void Add(string text)
            {
                int n;

                if (counts.TryGetValue(text, out n))
                {
                    counts[text] = n + 1;
                    return;
                }

                if (order.Count >= cap)
                {
                    Capped = true;
                    return;
                }

                order.Add(text);
                counts[text] = 1;
            }

            public List<string> Lines()
            {
                List<string> lines = new List<string>();

                foreach (string text in order)
                {
                    lines.Add(text + "  x" + counts[text].ToString(Inv));
                }

                return lines;
            }
        }
    }
}
