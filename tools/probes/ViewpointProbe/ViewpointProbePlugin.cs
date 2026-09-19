using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Plugins;

namespace ViewpointProbe
{
    /// <summary>
    /// Measures, inside Navisworks, whether a saved viewpoint records the hidden state it
    /// was saved with and brings it back after a save and a reopen. scan.md 5j, F85.
    ///
    /// Two ways of making a saved viewpoint are tried side by side, because the DLL
    /// offers both and says nothing about which records hiding:
    ///
    ///     A   new SavedViewpoint(Viewpoint)                     the camera alone, on its face
    ///     B   DocumentSavedViewpoints.CaptureRuntimeOverrides() the current view with its overrides
    ///
    /// Every step is written to the result file as it happens and flushed, so a plugin
    /// the host stops early still leaves behind what it measured. Nothing here touches
    /// anything but the copy it is handed.
    ///
    /// The second parameter, walk, turns it into the category walk for scan.md 5i: it
    /// opens each NWF it is handed and writes every distinct value of the category
    /// property, read the way the add-in reads one, into the result file.
    /// </summary>
    [Plugin(PluginName, DeveloperCode, DisplayName = "Viewpoint probe", ToolTip = "Measures saved viewpoints, scan.md 5j")]
    [AddInPlugin(AddInLocation.AddIn)]
    public sealed class ViewpointProbePlugin : AddInPlugin
    {
        public const string PluginName = "ViewpointProbe";
        public const string DeveloperCode = "PARS";

        private static readonly string[] CategoryNames = { "Category", "Revit Category", "Element Category" };

        private StreamWriter results;

        public override int Execute(params string[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                return 2;
            }

            string mode = parameters[0];
            string resultPath = parameters[1];

            using (results = new StreamWriter(resultPath, true, new UTF8Encoding(false)))
            {
                results.AutoFlush = true;
                Say("probe started, mode " + mode + ", " + (parameters.Length - 2) + " file(s)");

                try
                {
                    if (mode == "hidden")
                    {
                        MeasureHiddenState(parameters[2]);
                    }
                    else if (mode == "walk")
                    {
                        WalkCategories(parameters, 2);
                    }
                    else if (mode == "restore")
                    {
                        MeasureRestore(parameters[2]);
                    }
                    else
                    {
                        Say("UNKNOWN mode " + mode);
                        return 3;
                    }
                }
                catch (Exception error)
                {
                    Say("THREW " + error.GetType().Name + ": " + error.Message);
                    Say(error.StackTrace ?? string.Empty);
                    return 1;
                }

                Say("probe finished");
            }

            return 0;
        }

        private void Say(string line)
        {
            results.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + line);
        }

        // ---------- 5j, the hidden state ----------

        private void MeasureHiddenState(string nwfCopy)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Say("opening " + nwfCopy);

            if (!document.TryOpenFile(nwfCopy))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            Say("models " + document.Models.Count + ", saved viewpoints at the root " + document.SavedViewpoints.Value.Count);

            if (document.Models.Count < 2)
            {
                Say("UNKNOWN: the copy holds fewer than two models, so two cannot be hidden");
                return;
            }

            using (ModelItemCollection two = new ModelItemCollection())
            {
                two.Add(document.Models[0].RootItem);
                two.Add(document.Models[1].RootItem);
                Say("hiding two model roots: " + document.Models[0].FileName + " and " + document.Models[1].FileName);

                document.Models.SetHidden(two, true);
                Say("after SetHidden: IsHidden(two) = " + document.Models.IsHidden(two)
                    + ", root0.IsHidden = " + document.Models[0].RootItem.IsHidden
                    + ", root1.IsHidden = " + document.Models[1].RootItem.IsHidden);

                // A, the camera alone. The flag is read off the copy in the tree, because
                // reading it off a saved viewpoint that is not in a document yet threw
                // NullReferenceException inside the getter on the first attempt.
                using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
                using (SavedViewpoint a = new SavedViewpoint(camera))
                {
                    Say("A  new SavedViewpoint(Viewpoint) made, adding it");
                    document.SavedViewpoints.AddCopy(a);
                }

                NameAt(document, document.SavedViewpoints.Value.Count - 1, "probe A camera alone");
                ReadFlags(document, "probe A camera alone");

                // B, the current view with its overrides captured
                using (SavedViewpoint b = document.SavedViewpoints.CaptureRuntimeOverrides())
                {
                    Say("B  CaptureRuntimeOverrides() returned " + (b == null ? "null" : "a SavedViewpoint") + ", adding it");

                    if (b != null)
                    {
                        document.SavedViewpoints.AddCopy(b);
                    }
                }

                NameAt(document, document.SavedViewpoints.Value.Count - 1, "probe B runtime overrides");
                ReadFlags(document, "probe B runtime overrides");
                Say("saved viewpoints at the root now " + document.SavedViewpoints.Value.Count);

                // Pressed from a clean view, before any save
                document.Models.ResetAllHidden();
                Say("ResetAllHidden: IsHidden(two) = " + document.Models.IsHidden(two));
                PressAndRead(document, "probe A camera alone", two);
                document.Models.ResetAllHidden();
                PressAndRead(document, "probe B runtime overrides", two);
                document.Models.ResetAllHidden();

                Say("saving the copy");
                bool saved = document.TrySaveFile(nwfCopy);
                Say("TrySaveFile = " + saved + ", size on disk " + new FileInfo(nwfCopy).Length + " bytes");
            }

            Say("clearing and reopening");
            document.Clear();
            Say("after Clear: models " + document.Models.Count);

            if (!document.TryOpenFile(nwfCopy))
            {
                Say("UNKNOWN: the copy would not reopen");
                return;
            }

            Say("reopened: models " + document.Models.Count + ", saved viewpoints at the root " + document.SavedViewpoints.Value.Count);

            using (ModelItemCollection two = new ModelItemCollection())
            {
                two.Add(document.Models[0].RootItem);
                two.Add(document.Models[1].RootItem);
                Say("after reopen, before pressing anything: IsHidden(two) = " + document.Models.IsHidden(two));

                foreach (string name in new[] { "probe A camera alone", "probe B runtime overrides" })
                {
                    ReadFlags(document, name + " after reopen", name);
                    document.Models.ResetAllHidden();
                    PressAndRead(document, name, two);
                }

                document.Models.ResetAllHidden();
            }
        }

        private void ReadFlags(Document document, string name)
        {
            ReadFlags(document, name, name);
        }

        private void ReadFlags(Document document, string said, string name)
        {
            using (SavedViewpoint found = FindAtRoot(document, name))
            {
                if (found == null)
                {
                    Say(said + ": NOT FOUND in the tree");
                    return;
                }

                try
                {
                    Say(said + ": ContainsVisibilityOverrides = " + found.ContainsVisibilityOverrides
                        + ", ContainsAppearanceOverrides = " + found.ContainsAppearanceOverrides);
                }
                catch (Exception error)
                {
                    Say(said + ": reading the flags threw " + error.GetType().Name + ": " + error.Message);
                }

                DescribeOverrides(found);
            }
        }

        private void DescribeOverrides(SavedViewpoint viewpoint)
        {
            try
            {
                VisibilityOverrides overrides = viewpoint.GetVisibilityOverrides();

                if (overrides == null)
                {
                    Say("   GetVisibilityOverrides() returned null");
                    return;
                }

                Say("   GetVisibilityOverrides() returned " + overrides.GetType().FullName);

                foreach (System.Reflection.PropertyInfo property in overrides.GetType().GetProperties())
                {
                    try
                    {
                        object value = property.GetValue(overrides, null);
                        System.Collections.ICollection collection = value as System.Collections.ICollection;
                        Say("   " + property.Name + " = " + (collection != null ? collection.Count + " item(s)" : Convert.ToString(value)));
                    }
                    catch (Exception error)
                    {
                        Say("   " + property.Name + " threw " + error.GetType().Name);
                    }
                }
            }
            catch (Exception error)
            {
                Say("   GetVisibilityOverrides threw " + error.GetType().Name + ": " + error.Message);
            }
        }

        private void NameAt(Document document, int index, string name)
        {
            using (SavedItem item = document.SavedViewpoints.Value[index])
            {
                document.SavedViewpoints.EditDisplayName(item, name);
            }
        }

        private SavedViewpoint FindAtRoot(Document document, string name)
        {
            SavedItemCollection items = document.SavedViewpoints.Value;

            for (int i = 0; i < items.Count; i++)
            {
                SavedItem item = items[i];
                SavedViewpoint viewpoint = item as SavedViewpoint;

                if (viewpoint != null && string.Equals(viewpoint.DisplayName, name, StringComparison.Ordinal))
                {
                    return viewpoint;
                }

                item.Dispose();
            }

            return null;
        }

        private void PressAndRead(Document document, string name, ModelItemCollection two)
        {
            using (SavedViewpoint found = FindAtRoot(document, name))
            {
                if (found == null)
                {
                    Say("press " + name + ": NOT FOUND");
                    return;
                }

                document.SavedViewpoints.CurrentSavedViewpoint = found;
            }

            Say("press " + name + ": IsHidden(two) = " + document.Models.IsHidden(two)
                + ", root0.IsHidden = " + document.Models[0].RootItem.IsHidden
                + ", root1.IsHidden = " + document.Models[1].RootItem.IsHidden);
        }

        // ---------- 5k, putting the hidden state back ----------

        /// <summary>
        /// Measures how a hidden state the DOCUMENT holds, not the model files, can be
        /// read before the viewpoint writer hides things and put back after. The review
        /// of the viewpoints round found that ResetAllHiddenToModelState ends at the
        /// state the NWC files define, which is not what the NWF held. Four routes are
        /// tried on the copy, each one timed and read back:
        ///
        ///     1   CaptureRuntimeOverrides() before anything, then read its Hidden
        ///         collection WITHOUT adding it to the tree
        ///     2   press that capture, CurrentSavedViewpoint = it, without it being in the tree
        ///     3   walk RootItemDescendantsAndSelf and keep every item whose IsHidden is
        ///         true, then ResetAllHidden and SetHidden(kept, true)
        ///     4   what ResetAllHiddenToModelState does to a document level hide, and what
        ///         GetAllHiddenAtModelState returns
        ///
        /// Nothing is saved. The copy is opened, measured and left.
        /// </summary>
        private void MeasureRestore(string nwfCopy)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Say("opening " + nwfCopy);

            if (!document.TryOpenFile(nwfCopy))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            Say("models " + document.Models.Count);

            if (document.Models.Count < 2)
            {
                Say("UNKNOWN: the copy holds fewer than two models");
                return;
            }

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            using (ModelItemCollection one = new ModelItemCollection())
            {
                one.Add(document.Models[0].RootItem);
                document.Models.ResetAllHidden();
                Say("start: IsHidden(root0) = " + document.Models.IsHidden(one));

                // A document level hide, the thing a reviewer does and saves
                document.Models.SetHidden(one, true);
                Say("SetHidden(root0): IsHidden(root0) = " + document.Models.IsHidden(one));

                // Route 4a, what the writer's restore does to it today
                document.Models.ResetAllHiddenToModelState();
                Say("ResetAllHiddenToModelState: IsHidden(root0) = " + document.Models.IsHidden(one)
                    + "   (false means the document level hide is LOST by that call)");

                watch.Restart();
                using (ModelItemCollection atModel = document.Models.GetAllHiddenAtModelState())
                {
                    Say("GetAllHiddenAtModelState: " + atModel.Count + " item(s) in " + watch.ElapsedMilliseconds + " ms");
                }

                document.Models.SetHidden(one, true);

                // Route 1, capture before and read it without adding it
                SavedViewpoint before = null;

                try
                {
                    watch.Restart();
                    before = document.SavedViewpoints.CaptureRuntimeOverrides();
                    Say("route 1: CaptureRuntimeOverrides in " + watch.ElapsedMilliseconds + " ms, returned " + (before == null ? "null" : "a SavedViewpoint"));

                    try
                    {
                        VisibilityOverrides overrides = before.GetVisibilityOverrides();
                        Say("route 1: GetVisibilityOverrides off the un-added capture returned " + (overrides == null ? "null" : "an object")
                            + ", Hidden.Count = " + (overrides == null ? "n/a" : overrides.Hidden.Count.ToString()));
                    }
                    catch (Exception error)
                    {
                        Say("route 1: reading the un-added capture threw " + error.GetType().Name + ": " + error.Message);
                    }

                    try
                    {
                        Say("route 1: ContainsVisibilityOverrides off the un-added capture = " + before.ContainsVisibilityOverrides);
                    }
                    catch (Exception error)
                    {
                        Say("route 1: ContainsVisibilityOverrides threw " + error.GetType().Name);
                    }
                }
                catch (Exception error)
                {
                    Say("route 1: CaptureRuntimeOverrides threw " + error.GetType().Name + ": " + error.Message);
                }

                // Route 2, press the un-added capture
                document.Models.ResetAllHidden();
                Say("ResetAllHidden: IsHidden(root0) = " + document.Models.IsHidden(one));

                if (before != null)
                {
                    try
                    {
                        watch.Restart();
                        document.SavedViewpoints.CurrentSavedViewpoint = before;
                        Say("route 2: CurrentSavedViewpoint = the un-added capture in " + watch.ElapsedMilliseconds
                            + " ms, IsHidden(root0) = " + document.Models.IsHidden(one)
                            + "   (true means pressing an un-added capture puts the hide back)");
                    }
                    catch (Exception error)
                    {
                        Say("route 2: the setter threw " + error.GetType().Name + ": " + error.Message);
                    }

                    // Route 2b, add it, press the tree copy, remove it
                    try
                    {
                        int rootBefore = document.SavedViewpoints.Value.Count;
                        before.DisplayName = "probe restore";
                        document.SavedViewpoints.AddCopy(before);
                        Say("route 2b: added, root count " + rootBefore + " -> " + document.SavedViewpoints.Value.Count);
                        document.Models.ResetAllHidden();

                        using (SavedViewpoint found = FindAtRoot(document, "probe restore"))
                        {
                            if (found != null)
                            {
                                document.SavedViewpoints.CurrentSavedViewpoint = found;
                                Say("route 2b: pressed the tree copy, IsHidden(root0) = " + document.Models.IsHidden(one));
                                bool removed = document.SavedViewpoints.Remove(found);
                                Say("route 2b: Remove returned " + removed + ", root count now " + document.SavedViewpoints.Value.Count);
                            }
                            else
                            {
                                Say("route 2b: the added copy was not found by name");
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        Say("route 2b threw " + error.GetType().Name + ": " + error.Message);
                    }

                    before.Dispose();
                }

                // Route 3, the walk
                document.Models.ResetAllHidden();
                document.Models.SetHidden(one, true);
                watch.Restart();
                int walked = 0;

                using (ModelItemCollection kept = new ModelItemCollection())
                {
                    foreach (ModelItem item in document.Models.RootItemDescendantsAndSelf)
                    {
                        walked++;

                        if (item.IsHidden)
                        {
                            kept.Add(item);
                        }
                    }

                    Say("route 3: walked " + walked + " items in " + watch.ElapsedMilliseconds + " ms, " + kept.Count + " hidden");
                    document.Models.ResetAllHidden();
                    Say("route 3: ResetAllHidden, IsHidden(root0) = " + document.Models.IsHidden(one));
                    watch.Restart();
                    document.Models.SetHidden(kept, true);
                    Say("route 3: SetHidden(kept, true) in " + watch.ElapsedMilliseconds + " ms, IsHidden(root0) = " + document.Models.IsHidden(one)
                        + "   (true means the walk puts the hide back)");
                }

                // Route 3b, a walk that keeps only the topmost hidden item of each branch
                watch.Restart();
                int topmost = 0;

                using (ModelItemCollection tops = new ModelItemCollection())
                {
                    foreach (Model model in document.Models)
                    {
                        using (ModelItem root = model.RootItem)
                        {
                            topmost += CollectTopmostHidden(root, tops);
                        }
                    }

                    Say("route 3b: topmost hidden items " + tops.Count + " in " + watch.ElapsedMilliseconds + " ms");
                }

                document.Models.ResetAllHidden();
                Say("end: ResetAllHidden, IsHidden(root0) = " + document.Models.IsHidden(one));
            }
        }

        /// <summary>Adds the topmost hidden item of every branch and does not descend below it.</summary>
        private static int CollectTopmostHidden(ModelItem item, ModelItemCollection into)
        {
            if (item.IsHidden)
            {
                into.Add(item);
                return 1;
            }

            int count = 0;

            foreach (ModelItem child in item.Children)
            {
                count += CollectTopmostHidden(child, into);
            }

            return count;
        }

        // ---------- 5i, the category walk ----------

        private void WalkCategories(string[] parameters, int from)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Dictionary<string, int> categories = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = from; i < parameters.Length; i++)
            {
                string path = parameters[i];
                Say("opening " + path);

                if (!document.TryOpenFile(path))
                {
                    Say("UNKNOWN: TryOpenFile returned false for " + path);
                    continue;
                }

                int items = 0;
                int withCategory = 0;

                foreach (Model model in document.Models)
                {
                    ModelItem root = model.RootItem;

                    if (root == null)
                    {
                        continue;
                    }

                    foreach (ModelItem item in root.DescendantsAndSelf)
                    {
                        items++;
                        string category = FirstProperty(item, CategoryNames);

                        if (category.Length == 0)
                        {
                            continue;
                        }

                        withCategory++;
                        categories[category] = categories.ContainsKey(category) ? categories[category] + 1 : 1;
                    }
                }

                Say("walked " + items + " items, " + withCategory + " carrying a category, " + document.Models.Count + " models");
            }

            List<string> names = new List<string>(categories.Keys);
            names.Sort(StringComparer.Ordinal);
            Say("DISTINCT " + names.Count);

            foreach (string name in names)
            {
                Say("CATEGORY\t" + name + "\t" + categories[name]);
            }
        }

        /// <summary>The first property with one of those display names, read by kind, the way ClashHarvest reads one.</summary>
        private static string FirstProperty(ModelItem item, string[] wanted)
        {
            PropertyCategoryCollection categories = item.PropertyCategories;

            if (categories == null)
            {
                return string.Empty;
            }

            foreach (string name in wanted)
            {
                foreach (PropertyCategory category in categories)
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

            return string.Empty;
        }

        private static string Text(VariantData value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            switch (value.DataType)
            {
                case VariantDataType.DisplayString:
                    return value.ToDisplayString();
                case VariantDataType.IdentifierString:
                    return value.ToIdentifierString();
                case VariantDataType.NamedConstant:
                    using (NamedConstant named = value.ToNamedConstant())
                    {
                        return named == null ? string.Empty : (named.DisplayName ?? string.Empty);
                    }
                default:
                    return string.Empty;
            }
        }
    }
}
