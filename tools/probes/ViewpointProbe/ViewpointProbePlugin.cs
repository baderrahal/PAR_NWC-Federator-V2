using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Interop.ComApi;
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
                    else if (mode == "camera")
                    {
                        MeasureCamera(parameters[2]);
                    }
                    else if (mode == "record")
                    {
                        MeasureRecord(parameters[2]);
                    }
                    else if (mode == "com")
                    {
                        MeasureComRoute(parameters[2]);
                    }
                    else if (mode == "home")
                    {
                        MeasureHome(parameters[2]);
                    }
                    else if (mode == "dim")
                    {
                        MeasureDim(parameters[2]);
                    }
                    else if (mode == "negate")
                    {
                        MeasureNegate(parameters[2]);
                    }
                    else if (mode == "press")
                    {
                        MeasurePress(parameters[2], parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "route")
                    {
                        MeasureRoute(parameters[2]);
                    }
                    else if (mode == "colour")
                    {
                        MeasureColour(parameters[2]);
                    }
                    else if (mode == "pen")
                    {
                        MeasurePenetration(parameters[2]);
                    }
                    else if (mode == "tolerance")
                    {
                        MeasureTolerance(parameters[2]);
                    }
                    else if (mode == "drift")
                    {
                        MeasureDrift(parameters, 2);
                    }
                    else if (mode == "census")
                    {
                        MeasureCensus(parameters, 2);
                    }
                    else if (mode == "survey")
                    {
                        MeasureSurvey(parameters, 2);
                    }
                    else if (mode == "setremove")
                    {
                        MeasureSetRemove(parameters[2]);
                    }
                    else if (mode == "pointsat")
                    {
                        CountWhatPointsAtSets(parameters, 2);
                    }
                    else if (mode == "setrename")
                    {
                        MeasureSetRename(parameters[2]);
                    }
                    else if (mode == "mirror")
                    {
                        MeasureMirrorSwap(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null,
                            parameters.Length > 4 ? parameters[4] : null);
                    }
                    else if (mode == "testremove")
                    {
                        MeasureTestRemove(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null,
                            parameters.Length > 4 ? parameters[4] : null);
                    }
                    else if (mode == "mirrorcount")
                    {
                        MeasureMirrorCount(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null,
                            parameters.Length > 4 ? parameters[4] : null);
                    }
                    else if (mode == "worksets")
                    {
                        MeasureModelWorksets(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vptree")
                    {
                        DumpViewpointTree(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vpguid")
                    {
                        MeasureViewGuids(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vpcopy")
                    {
                        MeasureViewCopy(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vpspace")
                    {
                        MeasureViewNameSpaces(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vpremove")
                    {
                        MeasureViewRemove(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vpfolder")
                    {
                        MeasureFolderRemove(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
                    }
                    else if (mode == "vpcomment")
                    {
                        MeasureViewComments(
                            parameters[2],
                            parameters.Length > 3 ? parameters[3] : null);
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

        // ---------- 5l, the camera of a clash viewpoint ----------

        /// <summary>
        /// Measures what DocumentClashTests.TestsViewpointForResult gives back, because
        /// every viewpoint the viewpoints round wrote opened on the same empty top view
        /// while the picture of the same clash, TestsImageForResult, framed it. For the
        /// first results of the first tests that have any: the position that method
        /// returns, the centre of the bounding box of each side's first item, and then
        /// a camera BUILT from those boxes, position above and beside the centre, pointed
        /// at it, applied through CurrentViewpoint.CopyFrom and read back.
        /// </summary>
        private void MeasureCamera(string nwfCopy)
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

            using (Viewpoint opened = document.CurrentViewpoint.CreateCopy())
            {
                Say("the view the file opened on: " + Describe(opened));
            }

            Autodesk.Navisworks.Api.Clash.DocumentClashTests clashTests = document.GetClash().TestsData;
            Say("tests " + clashTests.Tests.Count);
            int shown = 0;

            for (int t = 0; t < clashTests.Tests.Count && shown < 6; t++)
            {
                Autodesk.Navisworks.Api.Clash.ClashTest test = clashTests.Tests[t] as Autodesk.Navisworks.Api.Clash.ClashTest;

                if (test == null || test.Children.Count == 0)
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count && shown < 6; r++)
                {
                    Autodesk.Navisworks.Api.Clash.ClashResult result = test.Children[r] as Autodesk.Navisworks.Api.Clash.ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    shown++;
                    Say("--- " + test.DisplayName + "  " + result.DisplayName + "  status " + result.Status);

                    try
                    {
                        using (Viewpoint framed = clashTests.TestsViewpointForResult(result))
                        {
                            Say("TestsViewpointForResult: " + (framed == null ? "null" : Describe(framed)));
                        }
                    }
                    catch (Exception error)
                    {
                        Say("TestsViewpointForResult threw " + error.GetType().Name + ": " + error.Message);
                    }

                    // Route a, what the builder did: a CreateCopy of the framed viewpoint
                    // kept after the framed one is disposed, applied later through CopyFrom.
                    try
                    {
                        Viewpoint copy;

                        using (Viewpoint framed = clashTests.TestsViewpointForResult(result))
                        {
                            copy = framed.CreateCopy();
                        }

                        Say("   route a, the copy after the framed one is disposed: " + Describe(copy));
                        document.CurrentViewpoint.CopyFrom(copy);
                        Say("   route a, read back after CopyFrom(copy): " + Describe(document.CurrentViewpoint.Value));

                        using (SavedViewpoint captured = document.SavedViewpoints.CaptureRuntimeOverrides())
                        {
                            Say("   route a, the capture's own Viewpoint: " + Describe(captured.Viewpoint));
                        }

                        copy.Dispose();
                    }
                    catch (Exception error)
                    {
                        Say("   route a threw " + error.GetType().Name + ": " + error.Message);
                    }

                    // Route b, the framed viewpoint itself kept, the result wrapper it came
                    // from disposed first, applied through CopyFrom.
                    try
                    {
                        Viewpoint framed = clashTests.TestsViewpointForResult(result);
                        result.Dispose();
                        Say("   route b, the framed one after its result is disposed: " + Describe(framed));
                        document.CurrentViewpoint.CopyFrom(framed);
                        Say("   route b, read back after CopyFrom(framed): " + Describe(document.CurrentViewpoint.Value));
                        framed.Dispose();
                        result = test.Children[r] as ClashResult;
                    }
                    catch (Exception error)
                    {
                        Say("   route b threw " + error.GetType().Name + ": " + error.Message);
                    }

                    BoundingBox3D box = null;

                    foreach (ModelItemCollection side in new[] { result.Selection1, result.Selection2 })
                    {
                        using (side)
                        {
                            if (side.Count == 0)
                            {
                                Say("   a side with no item");
                                continue;
                            }

                            using (ModelItem item = side[0])
                            {
                                BoundingBox3D b = item.BoundingBox();
                                Say("   item " + item.DisplayName + "  box " + Describe(b));
                                box = box == null ? b : box.Extend(b);
                            }
                        }
                    }

                    if (box == null || box.IsEmpty)
                    {
                        Say("   no box to build a camera from");
                        continue;
                    }

                    Point3D centre = box.Center;
                    double size = Math.Max(box.Size.X, Math.Max(box.Size.Y, box.Size.Z));
                    double back = Math.Max(size, 0.5) * 2.5;

                    using (Viewpoint built = document.CurrentViewpoint.CreateCopy())
                    {
                        built.Position = new Point3D(centre.X - back * 0.6, centre.Y - back * 0.6, centre.Z + back * 0.5);
                        built.PointAt(centre);
                        built.FocalDistance = back;
                        Say("   built camera: " + Describe(built));
                        document.CurrentViewpoint.CopyFrom(built);
                    }

                    using (Viewpoint now = document.CurrentViewpoint.CreateCopy())
                    {
                        Say("   read back after CopyFrom: " + Describe(now));
                    }
                }
            }
        }

        // ---------- 5m, a saved viewpoint with BOTH a camera and the hidden state ----------

        /// <summary>
        /// 5j measured that new SavedViewpoint(Viewpoint) records the camera and no
        /// overrides, and 5l measured that CaptureRuntimeOverrides records the overrides
        /// and NO camera, its Viewpoint throwing Camera not set. The API doc of
        /// DocumentSavedViewpoints.ReplaceFromCurrentView reads "Viewpoint, Redlines and
        /// visibility are updated to those in the current View", so this measures that
        /// route: a camera only viewpoint put into a folder, then replaced from the
        /// current view while a model is hidden, then read back and pressed.
        /// </summary>
        private void MeasureRecord(string nwfCopy)
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

            View view = document.ActiveView;
            Say("ActiveView in this host: " + (view == null ? "null" : "a View " + view.Width + "x" + view.Height));

            using (ModelItemCollection one = new ModelItemCollection())
            {
                one.Add(document.Models[0].RootItem);
                document.Models.ResetAllHidden();

                // A camera somewhere definite, applied to the view and the document
                using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
                {
                    camera.Position = new Point3D(12.5, -34.25, 56.125);
                    camera.PointAt(new Point3D(0, 0, 0));

                    if (view != null)
                    {
                        try { view.CopyViewpointFrom(camera, ViewChange.JumpCut); Say("view.CopyViewpointFrom done"); }
                        catch (Exception error) { Say("view.CopyViewpointFrom threw " + error.GetType().Name + ": " + error.Message); }
                    }

                    document.CurrentViewpoint.CopyFrom(camera);
                    Say("camera applied: " + Describe(document.CurrentViewpoint.Value));
                    document.Models.SetHidden(one, true);
                    Say("root0 hidden: " + document.Models.IsHidden(one));

                    // A folder, a camera only viewpoint in it, then the replace
                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = "probe record folder";
                        document.SavedViewpoints.AddCopy(folder);
                    }

                    GroupItem parent = FindFolderAtRoot(document, "probe record folder");
                    Say("folder added: " + (parent != null));

                    using (SavedViewpoint fresh = new SavedViewpoint(camera))
                    {
                        fresh.DisplayName = "probe record";
                        document.SavedViewpoints.AddCopy(parent, fresh);
                    }

                    parent.Dispose();
                    parent = FindFolderAtRoot(document, "probe record folder");

                    using (SavedViewpoint added = FindUnder(parent, "probe record"))
                    {
                        Say("before replace: " + (added == null ? "NOT FOUND" : Flags(added)));

                        if (added != null)
                        {
                            try
                            {
                                document.SavedViewpoints.ReplaceFromCurrentView(added);
                                Say("ReplaceFromCurrentView done");
                            }
                            catch (Exception error)
                            {
                                Say("ReplaceFromCurrentView threw " + error.GetType().Name + ": " + error.Message);
                            }
                        }
                    }

                    parent.Dispose();
                    parent = FindFolderAtRoot(document, "probe record folder");

                    using (SavedViewpoint replaced = FindUnder(parent, "probe record"))
                    {
                        Say("after replace: " + (replaced == null ? "NOT FOUND" : Flags(replaced)));
                    }

                    // Then pressed from elsewhere with nothing hidden
                    document.Models.ResetAllHidden();
                    camera.Position = new Point3D(100, 100, 100);
                    document.CurrentViewpoint.CopyFrom(camera);
                    Say("moved away: root0 hidden " + document.Models.IsHidden(one) + ", " + Describe(document.CurrentViewpoint.Value));

                    using (SavedViewpoint press = FindUnder(parent, "probe record"))
                    {
                        if (press != null)
                        {
                            document.SavedViewpoints.CurrentSavedViewpoint = press;
                        }
                    }

                    Say("pressed: root0 hidden " + document.Models.IsHidden(one) + ", " + Describe(document.CurrentViewpoint.Value)
                        + "   (hidden true and position 12.5, -34.25, 56.125 means the route records both)");
                    parent.Dispose();
                }

                document.Models.ResetAllHidden();
            }
        }

        // ---------- 5m, the COM route, a view with ApplyHideAttribs ----------

        /// <summary>
        /// The COM API's saved view carries a flag, InwOpView.ApplyHideAttribs, that the
        /// .NET SavedViewpoint does not expose, and the .NET route that writes the camera
        /// records no overrides. Measures: a COM view made with that flag on, its camera
        /// set from a .NET Viewpoint through ComApiBridge, added to the COM SavedViews
        /// while a model is hidden, then read back THROUGH THE .NET API, copied into a
        /// folder with AddCopy, removed from the root, and pressed.
        /// </summary>
        private void MeasureComRoute(string nwfCopy)
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

            using (ModelItemCollection one = new ModelItemCollection())
            {
                one.Add(document.Models[0].RootItem);
                document.Models.ResetAllHidden();

                using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
                {
                    camera.Position = new Point3D(12.5, -34.25, 56.125);
                    camera.PointAt(new Point3D(0, 0, 0));
                    document.CurrentViewpoint.CopyFrom(camera);
                    document.Models.SetHidden(one, true);
                    Say("camera applied and root0 hidden: " + document.Models.IsHidden(one));

                    int rootBefore = document.SavedViewpoints.Value.Count;

                    try
                    {
                        Autodesk.Navisworks.Api.Interop.ComApi.InwOpState10 state = Autodesk.Navisworks.Api.ComApi.ComApiBridge.State;
                        Autodesk.Navisworks.Api.Interop.ComApi.InwOpView view =
                            (Autodesk.Navisworks.Api.Interop.ComApi.InwOpView)state.ObjectFactory(
                                Autodesk.Navisworks.Api.Interop.ComApi.nwEObjectType.eObjectType_nwOpView, null, null);
                        view.name = "probe com";
                        view.ApplyHideAttribs = true;
                        view.ApplyMaterialAttribs = false;
                        view.anonview = Autodesk.Navisworks.Api.ComApi.ComApiBridge.ToInwOpAnonView(camera);
                        state.SavedViews().Add(view);
                        Say("COM view added, root count " + rootBefore + " -> " + document.SavedViewpoints.Value.Count);
                    }
                    catch (Exception error)
                    {
                        Say("COM route threw " + error.GetType().Name + ": " + error.Message);
                        return;
                    }

                    using (SavedViewpoint found = FindAtRoot(document, "probe com"))
                    {
                        Say("read back through .NET at the root: " + (found == null ? "NOT FOUND" : Flags(found)));
                    }

                    // Into a folder by AddCopy, then the root one removed
                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = "probe com folder";
                        document.SavedViewpoints.AddCopy(folder);
                    }

                    GroupItem parent = FindFolderAtRoot(document, "probe com folder");

                    using (SavedViewpoint found = FindAtRoot(document, "probe com"))
                    {
                        if (found != null && parent != null)
                        {
                            document.SavedViewpoints.AddCopy(parent, found);
                            Say("copied into the folder, removing the root one: " + document.SavedViewpoints.Remove(found));
                        }
                    }

                    parent.Dispose();
                    parent = FindFolderAtRoot(document, "probe com folder");

                    using (SavedViewpoint inFolder = FindUnder(parent, "probe com"))
                    {
                        Say("the folder copy: " + (inFolder == null ? "NOT FOUND" : Flags(inFolder)));
                    }

                    document.Models.ResetAllHidden();
                    camera.Position = new Point3D(100, 100, 100);
                    document.CurrentViewpoint.CopyFrom(camera);
                    Say("moved away: root0 hidden " + document.Models.IsHidden(one) + ", " + Describe(document.CurrentViewpoint.Value));

                    using (SavedViewpoint press = FindUnder(parent, "probe com"))
                    {
                        if (press != null)
                        {
                            document.SavedViewpoints.CurrentSavedViewpoint = press;
                        }
                    }

                    Say("pressed the folder copy: root0 hidden " + document.Models.IsHidden(one) + ", " + Describe(document.CurrentViewpoint.Value)
                        + "   (hidden true and position 12.5, -34.25, 56.125 means the route records both)");
                    parent.Dispose();

                    // Saved, cleared, reopened, pressed again
                    document.Models.ResetAllHidden();
                    string saved = Path.Combine(Path.GetDirectoryName(nwfCopy), "probe-com-saved.nwf");
                    Say("TrySaveFile to " + saved + " = " + document.TrySaveFile(saved));
                    document.Clear();

                    if (!document.TryOpenFile(saved))
                    {
                        Say("UNKNOWN: the saved copy would not reopen");
                        return;
                    }
                }
            }

            using (ModelItemCollection one = new ModelItemCollection())
            {
                one.Add(document.Models[0].RootItem);
                GroupItem parent = FindFolderAtRoot(document, "probe com folder");

                using (SavedViewpoint press = FindUnder(parent, "probe com"))
                {
                    Say("after reopen, the folder copy: " + (press == null ? "NOT FOUND" : Flags(press)));

                    if (press != null)
                    {
                        document.SavedViewpoints.CurrentSavedViewpoint = press;
                    }
                }

                Say("after reopen, pressed: root0 hidden " + document.Models.IsHidden(one) + ", " + Describe(document.CurrentViewpoint.Value));
                parent.Dispose();
                document.Models.ResetAllHidden();
            }
        }

        // ---------- 5n, which model a clashing item lives in ----------

        /// <summary>
        /// Three runs read no home model for any clash through ClashResult.Item1.Model
        /// and through Selection1[0].Model, while ClashHarvest fills the source file
        /// column through Item1.Model on every run. Measures, for the first results of
        /// the first tests that have any: what Item1.Model and its FileName read, what
        /// HasModel reads, what the ancestors' Model read, and what the document's own
        /// Models list its FileName as, so the two strings can be compared by eye.
        /// </summary>
        private void MeasureHome(string nwfCopy)
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

            for (int i = 0; i < document.Models.Count; i++)
            {
                using (Model model = document.Models[i])
                {
                    Say("document.Models[" + i + "].FileName = [" + model.FileName + "]  SourceFileName = [" + model.SourceFileName + "]");
                }
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            int shown = 0;

            for (int t = 0; t < clashTests.Tests.Count && shown < 4; t++)
            {
                ClashTest test = clashTests.Tests[t] as ClashTest;

                if (test == null || test.Children.Count == 0)
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count && shown < 4; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    shown++;
                    Say("--- " + test.DisplayName + "  " + result.DisplayName);

                    foreach (string side in new[] { "Item1", "Item2" })
                    {
                        try
                        {
                            using (ModelItem item = side == "Item1" ? result.Item1 : result.Item2)
                            {
                                if (item == null)
                                {
                                    Say("   " + side + " is null");
                                    continue;
                                }

                                string line = "   " + side + " [" + item.DisplayName + "] HasModel " + item.HasModel;

                                using (Model model = item.Model)
                                {
                                    line += ", Model " + (model == null ? "null" : "[" + model.FileName + "]");
                                }

                                int depth = 0;
                                string top = "none";

                                foreach (ModelItem ancestor in item.AncestorsAndSelf)
                                {
                                    depth++;

                                    if (ancestor.HasModel)
                                    {
                                        using (Model model = ancestor.Model)
                                        {
                                            top = model == null ? "null" : "[" + model.FileName + "] at depth " + depth;
                                        }
                                    }
                                }

                                Say(line + ", ancestors and self " + depth + ", the one with a model " + top);
                            }
                        }
                        catch (Exception error)
                        {
                            Say("   " + side + " threw " + error.GetType().Name + ": " + error.Message);
                        }
                    }
                }
            }
        }

        // ---------- 5o, does a saved viewpoint record that items are dimmed ----------

        /// <summary>
        /// F85 writes a viewpoint that opens on its clash with the other disciplines
        /// hidden, and Bader pressed two and could not see the clash, because the camera
        /// lands inside a solid beam. Clash Detective dims everything but the two clashing
        /// items. This measures whether a saved viewpoint can record that dimming, and
        /// whether the record survives a save, a close and a reopen off the disk, which is
        /// the part 5l caught the camera route failing.
        ///
        /// It measures the writer's REAL sequence, including the reset before the save, so
        /// a viewpoint that only holds a reference to live document state is caught here
        /// rather than on a run.
        /// </summary>
        private void MeasureDim(string nwfCopy)
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

            TryReadClashOptions();

            // The two items of the first clash that has two with geometry, named by index
            // path so they can be resolved again after the reopen without a live handle.
            int[] firstPath = null;
            int[] secondPath = null;
            string clashName = null;

            Autodesk.Navisworks.Api.Clash.DocumentClashTests clashTests = document.GetClash().TestsData;

            for (int t = 0; t < clashTests.Tests.Count && firstPath == null; t++)
            {
                ClashTest test = clashTests.Tests[t] as ClashTest;

                if (test == null)
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count && firstPath == null; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    using (ModelItem a = result.Item1)
                    using (ModelItem b = result.Item2)
                    {
                        if (a == null || b == null || !a.HasGeometry || !b.HasGeometry)
                        {
                            continue;
                        }

                        firstPath = PathOf(document, a);
                        secondPath = PathOf(document, b);
                        clashName = test.DisplayName + "  " + result.DisplayName;
                    }
                }
            }

            if (firstPath == null)
            {
                Say("UNKNOWN: no clash in this copy has two items with geometry");
                return;
            }

            Say("the clash measured against: " + clashName);
            Say("index path of item 1: " + string.Join(",", Strings(firstPath)) + "   item 2: " + string.Join(",", Strings(secondPath)));

            // A third item, neither of the two, to prove the dimming reached the rest
            int[] otherPath = FindOther(document, firstPath, secondPath);
            Say("index path of a third item: " + (otherPath == null ? "none found" : string.Join(",", Strings(otherPath))));

            SayThree(document, firstPath, secondPath, otherPath, "at the start");

            // The index path round trip on its own, because the writer names an item in
            // walk one and resolves it in walk two, and must not keep a handle to do it.
            using (ModelItem resolved = Resolve(document, firstPath))
            {
                Say("index path round trip: " + (resolved == null ? "RESOLVED NOTHING" : "[" + resolved.DisplayName + "] has geometry " + resolved.HasGeometry));
            }

            using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
            {
                // Control. No override at all, ApplyMaterialAttribs true. 5j noted the
                // appearance flag reading true on a capture that set none, and a flag
                // that is always true is no read back.
                AddComView("probe dim none", camera, true);
                Say("CONTROL, no override at all: " + MaterialFlags(document, "probe dim none"));

                // Route T, temporary materials
                document.Models.ResetAllTemporaryMaterials();
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

                using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
                {
                    document.Models.OverrideTemporaryTransparency(roots, 0.85);
                }

                Say("route T: OverrideTemporaryTransparency 0.85 on " + document.Models.Count + " root(s) in " + watch.ElapsedMilliseconds + " ms");
                SayThree(document, firstPath, secondPath, otherPath, "after the root override");

                watch.Restart();
                ResetTwo(document, firstPath, secondPath, false);
                Say("route T: ResetTemporaryMaterials on the two items in " + watch.ElapsedMilliseconds + " ms");
                SayThree(document, firstPath, secondPath, otherPath, "after the two were reset");

                AddComView("probe dim temporary", camera, true);
                Say("route T, the view just added: " + MaterialFlags(document, "probe dim temporary"));

                // Route T with a model HIDDEN as well, which is what the writer does, so
                // the visibility read back F85 relies on can be judged. The appearance
                // flag reads true on the control above with nothing overridden, so a flag
                // may be no read back at all and the COUNT is what has to be looked at.
                using (ModelItemCollection one = new ModelItemCollection())
                using (Model model = document.Models[document.Models.Count - 1])
                using (ModelItem root = model.RootItem)
                {
                    one.Add(root);
                    document.Models.SetHidden(one, true);
                    Say("route T with a hide: hid [" + model.FileName + "], IsHidden " + document.Models.IsHidden(one));
                }

                AddComView("probe dim temporary hidden", camera, true);
                Say("route T with a hide, the view just added: " + MaterialFlags(document, "probe dim temporary hidden"));
                document.Models.ResetAllHidden();

                // The SCOPED undo, because ResetAllTemporaryMaterials would also clear a
                // temporary override this tool did not set, which is the 5k trap in a
                // second shape. Reset only the roots this writer overrode.
                watch.Restart();

                using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
                {
                    document.Models.ResetTemporaryMaterials(roots);
                }

                Say("route T: the SCOPED undo, ResetTemporaryMaterials on the roots, in " + watch.ElapsedMilliseconds + " ms");
                SayThree(document, firstPath, secondPath, otherPath, "after the scoped undo");

                // Route P, permanent materials
                document.Models.ResetAllTemporaryMaterials();
                document.Models.ResetAllPermanentMaterials();
                watch.Restart();

                using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
                {
                    document.Models.OverridePermanentTransparency(roots, 0.85);
                }

                Say("route P: OverridePermanentTransparency 0.85 on the roots in " + watch.ElapsedMilliseconds + " ms");
                ResetTwo(document, firstPath, secondPath, true);
                SayThree(document, firstPath, secondPath, otherPath, "after the permanent override and the two resets");

                AddComView("probe dim permanent", camera, true);
                Say("route P, the view just added: " + MaterialFlags(document, "probe dim permanent"));
            }

            // THE WRITER'S REAL SEQUENCE. Everything is put back before the NWF is saved,
            // so a viewpoint holding a reference to live state rather than a snapshot
            // comes back empty and is caught here.
            document.Models.ResetAllTemporaryMaterials();
            document.Models.ResetAllPermanentMaterials();
            SayThree(document, firstPath, secondPath, otherPath, "after both resets, before the save");

            string saved = Path.Combine(Path.GetDirectoryName(nwfCopy), "probe-dim-saved.nwf");
            Say("TrySaveFile to " + saved + " = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            Say("reopened off the disk");

            foreach (string name in new[] { "probe dim none", "probe dim temporary", "probe dim temporary hidden", "probe dim permanent" })
            {
                Say("AFTER THE REOPEN, " + name + ": " + MaterialFlags(document, name));

                using (SavedViewpoint press = FindAtRoot(document, name))
                {
                    if (press == null)
                    {
                        continue;
                    }

                    document.SavedViewpoints.CurrentSavedViewpoint = press;
                }

                SayThree(document, firstPath, secondPath, otherPath, "after pressing " + name);
                document.Models.ResetAllTemporaryMaterials();
            }

            Say("done. A route works only where, after the reopen, pressing it leaves the two items solid and the third dim.");
        }

        // ---------- what a WRITTEN viewpoint actually shows when it is pressed ----------

        /// <summary>
        /// Opens an NWF this tool wrote and presses the first viewpoints it finds, three
        /// folders down, counting what a person would see: how many items are hidden, how
        /// many are dimmed and how many are left solid. A screenshot shows a grey shape
        /// and leaves which shape it is to the eye. This counts it.
        ///
        /// A dimmed viewpoint is right where exactly TWO items read solid, which are the
        /// two the clash is between, and everything else visible reads at the dim value.
        /// </summary>
        private void MeasurePress(string nwf, string under)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Say("opening " + nwf);

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            Say("models " + document.Models.Count);

            List<SavedViewpoint> found = new List<SavedViewpoint>();
            List<string> paths = new List<string>();

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                if (string.IsNullOrEmpty(under))
                {
                    FirstFew(root, string.Empty, found, paths, 3);
                }
                else
                {
                    // Only under the named top folder, because a client's NWF carries its
                    // own viewpoints at the root and those are not the ones being judged.
                    SavedItemCollection children = root.Children;

                    for (int i = 0; i < children.Count; i++)
                    {
                        SavedItem child = children[i];
                        GroupItem folder = child as GroupItem;

                        if (folder != null && string.Equals(child.DisplayName, under, StringComparison.Ordinal))
                        {
                            FirstFew(folder, "/" + child.DisplayName, found, paths, 3);
                        }

                        child.Dispose();
                    }
                }
            }

            Say("pressing " + found.Count + " viewpoint(s) out of the tree"
                + (string.IsNullOrEmpty(under) ? string.Empty : " under " + under));

            for (int i = 0; i < found.Count; i++)
            {
                using (SavedViewpoint viewpoint = found[i])
                {
                    Say("--- " + paths[i]);
                    document.Models.ResetAllTemporaryMaterials();
                    document.Models.ResetAllHidden();
                    document.SavedViewpoints.CurrentSavedViewpoint = viewpoint;
                    CountWhatIsSeen(document);
                }
            }

            document.Models.ResetAllTemporaryMaterials();
            document.Models.ResetAllHidden();
        }

        /// <summary>The first few leaf viewpoints under the tree, with the path each sits at.</summary>
        private static void FirstFew(GroupItem parent, string path, List<SavedViewpoint> into, List<string> paths, int want)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count && into.Count < want; i++)
            {
                SavedItem child = children[i];
                GroupItem folder = child as GroupItem;

                if (folder != null)
                {
                    FirstFew(folder, path + "/" + child.DisplayName, into, paths, want);
                    folder.Dispose();
                    continue;
                }

                SavedViewpoint viewpoint = child as SavedViewpoint;

                if (viewpoint != null && path.Length > 0)
                {
                    into.Add(viewpoint);
                    paths.Add(path + "/" + child.DisplayName);
                    continue;
                }

                child.Dispose();
            }
        }

        /// <summary>
        /// PER MODEL, because hiding a model ROOT does not set IsHidden on the leaves
        /// under it: the first press measured 817 solid items and they were the hidden
        /// models' items counted as visible. A model whose root is hidden contributes
        /// nothing a person sees.
        /// </summary>
        private void CountWhatIsSeen(Document document)
        {
            int hiddenModels = 0;
            int hiddenItems = 0;
            int solid = 0;
            int dim = 0;
            string dimValue = "none";
            List<string> solidNames = new List<string>();

            for (int m = 0; m < document.Models.Count; m++)
            {
                using (Model model = document.Models[m])
                using (ModelItem root = model.RootItem)
                {
                    bool modelHidden;

                    using (ModelItemCollection one = new ModelItemCollection())
                    {
                        one.Add(root);
                        modelHidden = document.Models.IsHidden(one);
                    }

                    foreach (ModelItem item in root.DescendantsAndSelf)
                    {
                        using (item)
                        {
                            if (!item.HasGeometry)
                            {
                                continue;
                            }

                            if (modelHidden)
                            {
                                hiddenItems++;
                                continue;
                            }

                            using (ModelGeometry geometry = item.Geometry)
                            {
                                if (geometry.ActiveTransparency > 0.0)
                                {
                                    dim++;
                                    dimValue = Round(geometry.ActiveTransparency);
                                }
                                else
                                {
                                    solid++;

                                    if (solidNames.Count < 4)
                                    {
                                        // THE COLOUR AS WELL AS THE NAME, Q58. A solid item
                                        // in the wrong colour is as wrong as a dim one, and
                                        // a screenshot leaves which shade it is to the eye.
                                        solidNames.Add(item.DisplayName + " " + Rgb(geometry.ActiveColor)
                                            + " in " + Path.GetFileName(model.FileName));
                                    }
                                }
                            }
                        }
                    }

                    if (modelHidden)
                    {
                        hiddenModels++;
                    }
                }
            }

            Say("   " + hiddenModels + " model(s) hidden carrying " + hiddenItems + " item(s), then of what is left: "
                + dim + " dimmed at " + dimValue + " and SOLID " + solid
                + (solidNames.Count == 0 ? string.Empty : ": " + string.Join(" | ", solidNames.ToArray())));
        }

        // ---------- 5g, does a NEGATED search condition build, find and survive ----------

        /// <summary>
        /// F87 wants BLD-EL-Devices to ask for category CONTAINS Devices AND NOT each of
        /// six named device categories, and shipped the fallback, one equals, because
        /// nothing had measured whether a negation survives the exchange file. The
        /// exchange file's flags attribute IS SearchConditionOptions, F78, and
        /// NegateCondition is 32, so the question is whether a condition built with that
        /// bit finds the right items and keeps the bit when the NWF is saved and reopened.
        ///
        /// The condition is built through the SAME constructor SetBuilder.BuildCondition
        /// calls, with the same two Ignore bits it always adds, so what is measured here
        /// is the tool's own call and not a near relative of it.
        /// </summary>
        private void MeasureNegate(string nwfCopy)
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

            int all = 0;

            foreach (ModelItem item in document.Models.RootItemDescendantsAndSelf)
            {
                using (item)
                {
                    all++;
                }
            }

            Say("the copy holds " + all + " items in " + document.Models.Count + " models");

            // flags="0" and flags="32", the two the exchange file could carry
            int plain = CountBySearch(document, 0, "Lighting Fixtures");
            int negated = CountBySearch(document, 32, "Lighting Fixtures");

            Say("flags=0,  Category equals Lighting Fixtures  found " + plain + " item(s)");
            Say("flags=32, the same condition negated,        found " + negated + " item(s)");
            Say("   negation works where the second is neither the first nor zero, out of " + all + " items");

            // The options read back off the built condition, so the bit is seen to survive
            // the constructor the tool calls
            using (Search search = BuildSearch(32, "Lighting Fixtures"))
            {
                using (SearchCondition built = search.SearchConditions[0])
                {
                    Say("the built condition's Options read " + built.Options + " = " + (int)built.Options
                        + ", NegateCondition in it: " + ((built.Options & SearchConditionOptions.NegateCondition) == SearchConditionOptions.NegateCondition));
                }
            }

            // Into the document as a set, then saved, cleared, reopened, and read back
            using (Search search = BuildSearch(32, "Lighting Fixtures"))
            using (SelectionSet set = new SelectionSet(search))
            {
                set.DisplayName = "probe negated set";
                document.SelectionSets.AddCopy(set);
            }

            Say("added the negated set, the document now holds " + document.SelectionSets.Value.Count + " at the root");

            string saved = Path.Combine(Path.GetDirectoryName(nwfCopy), "probe-negate-saved.nwf");
            Say("TrySaveFile to " + saved + " = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            Say("reopened off the disk");
            ReadSetBack(document, "probe negated set");

            // THE SHAPE F87 ACTUALLY WANTS, which is two conditions and not one: category
            // CONTAINS Devices, and NOT one named category. One negated condition on its
            // own answered 4 items above and that number needs explaining before the
            // corrected matrix is written with a negation in it.
            Say("--- the two condition shape F87 wants ---");
            Say("contains Devices                          found " + CountContains(document, 0, "Devices", 0, null) + " item(s)");

            foreach (string category in new[] { "Nurse Call Devices", "Data Devices", "Security Devices", "Lighting Devices", "Communication Devices", "Fire Alarm Devices" })
            {
                Say("   equals [" + category + "] on its own " + CountBySearch(document, 0, category)
                    + ", and contains Devices NOT equals it " + CountContains(document, 0, "Devices", 32, category));
            }
            Say("NOT equals Lighting Fixtures, what the 4 are:");
            NameWhatItFinds(document, 32, "Lighting Fixtures");
        }

        private void NameWhatItFinds(Document document, int flags, string value)
        {
            using (Search search = BuildSearch(flags, value))
            using (ModelItemCollection found = search.FindAll(document, false))
            {
                for (int i = 0; i < found.Count && i < 6; i++)
                {
                    using (ModelItem item = found[i])
                    {
                        Say("      [" + item.DisplayName + "] geometry " + item.HasGeometry + ", model " + item.HasModel);
                    }
                }
            }
        }

        private static int CountContains(Document document, int firstFlags, string firstValue, int secondFlags, string secondValue)
        {
            SearchConditionOptions common = SearchConditionOptions.IgnoreCategoryDisplayName
                | SearchConditionOptions.IgnorePropertyDisplayName;

            using (Search search = new Search())
            {
                search.Selection.SelectAll();
                search.SearchConditions.Add(new SearchCondition(
                    new NamedConstant("LcRevitData_Element", "Element"),
                    new NamedConstant("LcRevitPropertyElementCategory", "Category"),
                    (SearchConditionOptions)firstFlags | common,
                    SearchConditionComparison.DisplayStringContains,
                    VariantData.FromDisplayString(firstValue)));

                if (secondValue != null)
                {
                    search.SearchConditions.Add(new SearchCondition(
                        new NamedConstant("LcRevitData_Element", "Element"),
                        new NamedConstant("LcRevitPropertyElementCategory", "Category"),
                        (SearchConditionOptions)secondFlags | common,
                        SearchConditionComparison.Equal,
                        VariantData.FromDisplayString(secondValue)));
                }

                using (ModelItemCollection found = search.FindAll(document, false))
                {
                    return found.Count;
                }
            }
        }

        private void ReadSetBack(Document document, string name)
        {
            SavedItemCollection items = document.SelectionSets.Value;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    SelectionSet set = item as SelectionSet;

                    if (set == null || !string.Equals(item.DisplayName, name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Say("after the reopen the set [" + name + "] is there, HasSearch " + set.HasSearch);

                    if (!set.HasSearch)
                    {
                        return;
                    }

                    using (Search search = set.Search)
                    {
                        Say("   it holds " + search.SearchConditions.Count + " condition(s)");

                        for (int c = 0; c < search.SearchConditions.Count; c++)
                        {
                            using (SearchCondition condition = search.SearchConditions[c])
                            {
                                Say("   condition " + c + " Options " + condition.Options + " = " + (int)condition.Options
                                    + ", NegateCondition in it: "
                                    + ((condition.Options & SearchConditionOptions.NegateCondition) == SearchConditionOptions.NegateCondition));
                            }
                        }

                        using (ModelItemCollection found = search.FindAll(document, false))
                        {
                            Say("   it finds " + found.Count + " item(s) after the reopen");
                        }
                    }

                    return;
                }
            }

            Say("after the reopen the set [" + name + "] is NOT THERE");
        }

        private static Search BuildSearch(int flags, string value)
        {
            // The same options SetBuilder.BuildCondition assembles: the file's flags, then
            // the two Ignore bits it always adds.
            SearchConditionOptions options = (SearchConditionOptions)flags
                | SearchConditionOptions.IgnoreCategoryDisplayName
                | SearchConditionOptions.IgnorePropertyDisplayName;

            Search search = new Search();
            search.Selection.SelectAll();
            search.SearchConditions.Add(new SearchCondition(
                new NamedConstant("LcRevitData_Element", "Element"),
                new NamedConstant("LcRevitPropertyElementCategory", "Category"),
                options,
                SearchConditionComparison.Equal,
                VariantData.FromDisplayString(value)));

            return search;
        }

        private static int CountBySearch(Document document, int flags, string value)
        {
            using (Search search = BuildSearch(flags, value))
            using (ModelItemCollection found = search.FindAll(document, false))
            {
                return found.Count;
            }
        }

        /// <summary>Clash Detective's own dim value, if anything in this API will say it.</summary>
        private void TryReadClashOptions()
        {
            try
            {
                object options = Autodesk.Navisworks.Api.Application.Options;
                Say("Application.Options is " + (options == null ? "null" : options.GetType().FullName)
                    + ", public members: " + string.Join(", ", MemberNames(options)));
            }
            catch (Exception error)
            {
                Say("reading Application.Options threw " + error.GetType().Name);
            }

            try
            {
                InwOpState10 state = ComApiBridge.State;
                Say("the COM state is " + state.GetType().FullName + ", members naming an option: "
                    + string.Join(", ", OptionMemberNames(state)));
            }
            catch (Exception error)
            {
                Say("reading the COM state threw " + error.GetType().Name);
            }
        }

        private static string[] MemberNames(object value)
        {
            if (value == null)
            {
                return new string[0];
            }

            List<string> names = new List<string>();

            foreach (System.Reflection.MemberInfo member in value.GetType().GetMembers(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                if (member.Name.IndexOf("get_", StringComparison.Ordinal) != 0)
                {
                    names.Add(member.Name);
                }
            }

            return names.ToArray();
        }

        private static string[] OptionMemberNames(object value)
        {
            List<string> names = new List<string>();

            foreach (string name in MemberNames(value))
            {
                if (name.IndexOf("Option", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Setting", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    names.Add(name);
                }
            }

            return names.Count == 0 ? new[] { "none" } : names.ToArray();
        }

        private void AddComView(string name, Viewpoint camera, bool applyMaterial)
        {
            InwOpState10 state = ComApiBridge.State;
            InwOpView view = (InwOpView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpView, null, null);
            view.name = name;
            view.ApplyHideAttribs = true;
            view.ApplyMaterialAttribs = applyMaterial;
            view.anonview = ComApiBridge.ToInwOpAnonView(camera);
            state.SavedViews().Add(view);
        }

        private string MaterialFlags(Document document, string name)
        {
            using (SavedViewpoint found = FindAtRoot(document, name))
            {
                if (found == null)
                {
                    return "NOT FOUND at the root";
                }

                string line;

                try
                {
                    line = "ContainsAppearanceOverrides " + found.ContainsAppearanceOverrides;
                }
                catch (Exception error)
                {
                    line = "ContainsAppearanceOverrides threw " + error.GetType().Name;
                }

                try
                {
                    AppearanceOverrides overrides = found.GetAppearanceOverrides();
                    line += ", MaterialOverrides " + (overrides == null ? "null" : overrides.MaterialOverrides.Count.ToString());
                }
                catch (Exception error)
                {
                    line += ", GetAppearanceOverrides threw " + error.GetType().Name + ": " + error.Message;
                }

                try
                {
                    line += ", ContainsVisibilityOverrides " + found.ContainsVisibilityOverrides;
                }
                catch (Exception error)
                {
                    line += ", ContainsVisibilityOverrides threw " + error.GetType().Name;
                }

                try
                {
                    VisibilityOverrides hidden = found.GetVisibilityOverrides();

                    if (hidden == null)
                    {
                        line += ", Hidden null";
                    }
                    else
                    {
                        using (ModelItemCollection items = hidden.Hidden)
                        {
                            line += ", Hidden " + items.Count;
                        }
                    }
                }
                catch (Exception error)
                {
                    line += ", GetVisibilityOverrides threw " + error.GetType().Name + ": " + error.Message;
                }

                return line;
            }
        }

        private void SayThree(Document document, int[] first, int[] second, int[] other, string when)
        {
            Say("   " + when + ":");
            Say("      item 1 " + Transparency(document, first));
            Say("      item 2 " + Transparency(document, second));
            Say("      other  " + Transparency(document, other));
        }

        private static string Transparency(Document document, int[] path)
        {
            if (path == null)
            {
                return "no item";
            }

            using (ModelItem item = Resolve(document, path))
            {
                if (item == null)
                {
                    return "resolved nothing";
                }

                if (!item.HasGeometry)
                {
                    return "[" + item.DisplayName + "] has no geometry";
                }

                using (ModelGeometry geometry = item.Geometry)
                {
                    return "[" + item.DisplayName + "] active " + Round(geometry.ActiveTransparency)
                        + " permanent " + Round(geometry.PermanentTransparency)
                        + " original " + Round(geometry.OriginalTransparency);
                }
            }
        }

        private static void ResetTwo(Document document, int[] first, int[] second, bool permanent)
        {
            using (ModelItemCollection two = new ModelItemCollection())
            using (ModelItem a = Resolve(document, first))
            using (ModelItem b = Resolve(document, second))
            {
                if (a != null)
                {
                    two.Add(a);
                }

                if (b != null)
                {
                    two.Add(b);
                }

                if (two.Count == 0)
                {
                    return;
                }

                if (permanent)
                {
                    document.Models.ResetPermanentMaterials(two);
                }
                else
                {
                    document.Models.ResetTemporaryMaterials(two);
                }
            }
        }

        private static int[] PathOf(Document document, ModelItem item)
        {
            System.Collections.ObjectModel.Collection<int> path = document.Models.CreateIndexPath(item);
            int[] copy = new int[path.Count];
            path.CopyTo(copy, 0);
            return copy;
        }

        private static ModelItem Resolve(Document document, int[] path)
        {
            if (path == null)
            {
                return null;
            }

            return document.Models.ResolveIndexPath(path);
        }

        private static int[] FindOther(Document document, int[] first, int[] second)
        {
            foreach (ModelItem item in document.Models.RootItemDescendantsAndSelf)
            {
                using (item)
                {
                    if (!item.HasGeometry)
                    {
                        continue;
                    }

                    int[] path = PathOf(document, item);

                    if (!Same(path, first) && !Same(path, second))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        private static bool Same(int[] a, int[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static string[] Strings(int[] path)
        {
            string[] text = new string[path.Length];

            for (int i = 0; i < path.Length; i++)
            {
                text[i] = path[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return text;
        }

        private string Flags(SavedViewpoint v)
        {
            string flags;

            try
            {
                flags = "ContainsVisibilityOverrides " + v.ContainsVisibilityOverrides;
            }
            catch (Exception error)
            {
                flags = "ContainsVisibilityOverrides threw " + error.GetType().Name;
            }

            try
            {
                using (Viewpoint recorded = v.Viewpoint)
                {
                    flags += ", camera " + Describe(recorded);
                }
            }
            catch (Exception error)
            {
                flags += ", camera threw " + error.GetType().Name + ": " + error.Message;
            }

            return flags;
        }

        private static GroupItem FindFolderAtRoot(Document document, string name)
        {
            SavedItemCollection items = document.SavedViewpoints.Value;

            for (int i = 0; i < items.Count; i++)
            {
                SavedItem item = items[i];
                GroupItem group = item as GroupItem;

                if (group != null && string.Equals(item.DisplayName, name, StringComparison.Ordinal))
                {
                    return group;
                }

                item.Dispose();
            }

            return null;
        }

        private static SavedViewpoint FindUnder(GroupItem parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            SavedItemCollection items = parent.Children;

            for (int i = 0; i < items.Count; i++)
            {
                SavedItem item = items[i];
                SavedViewpoint viewpoint = item as SavedViewpoint;

                if (viewpoint != null && string.Equals(item.DisplayName, name, StringComparison.Ordinal))
                {
                    return viewpoint;
                }

                item.Dispose();
            }

            return null;
        }

        private static string Describe(Viewpoint v)
        {
            if (v == null)
            {
                return "null";
            }

            try
            {
                Point3D p = v.Position;
                return "position (" + Round(p.X) + ", " + Round(p.Y) + ", " + Round(p.Z) + ")"
                    + " projection " + v.Projection
                    + " focal " + (v.HasFocalDistance ? Round(v.FocalDistance) : "none")
                    + " heightField " + Round(v.HeightField);
            }
            catch (Exception error)
            {
                return "describe threw " + error.GetType().Name;
            }
        }

        private static string Describe(BoundingBox3D b)
        {
            if (b == null || b.IsEmpty)
            {
                return "empty";
            }

            Point3D c = b.Center;
            return "centre (" + Round(c.X) + ", " + Round(c.Y) + ", " + Round(c.Z) + ") size ("
                + Round(b.Size.X) + ", " + Round(b.Size.Y) + ", " + Round(b.Size.Z) + ")";
        }

        private static string Round(double d)
        {
            return d.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
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

        // ---------- PART 1 and PART 2, the cheap write route and the colours ----------

        /// <summary>
        /// Writes the SAME scene twice, once through the route the tool uses today and
        /// once through the COM folder collection, and reads both back off the disk on
        /// the four counts that matter: the camera, the hidden state, the dimming, and
        /// the two clashing items left solid. The colours are measured beside them,
        /// because a viewpoint that records no colour must not be counted either.
        ///
        /// ROUTE A, what the tool does today: add the view to the COM root collection,
        /// copy it into its folder with the .NET AddCopy, remove the root one. Three
        /// tree operations per viewpoint.
        /// ROUTE B, the cheap one: find the folder's own InwOpFolderView and add the
        /// view straight into its SavedViews collection. One tree operation.
        ///
        /// Nothing is decided here on speed. A route that writes fast and records
        /// nothing is how this feature failed twice, so the read back is the whole test
        /// and the milliseconds are a footnote.
        /// </summary>
        private void MeasureRoute(string nwfCopy)
        {
            string folder = Path.GetDirectoryName(nwfCopy);
            string aFile = Path.Combine(folder, "probe-route-a.nwf");
            string bFile = Path.Combine(folder, "probe-route-b.nwf");

            long aMs = WriteOneRoute(nwfCopy, aFile, "A", false);

            if (aMs < 0)
            {
                return;
            }

            ReadRouteBack(aFile, "A");

            long bMs = WriteOneRoute(nwfCopy, bFile, "B", true);

            if (bMs < 0)
            {
                return;
            }

            ReadRouteBack(bFile, "B");

            Say(string.Empty);
            Say("THE TWO SIDE BY SIDE, " + RouteViews + " viewpoints of the same scene:");
            Say("   route A, root add then AddCopy then Remove:   " + aMs + " ms, " + Bytes(aFile) + " bytes");
            Say("   route B, straight into the folder collection: " + bMs + " ms, " + Bytes(bFile) + " bytes");
        }

        /// <summary>How many viewpoints each route writes, enough that a per call figure means something.</summary>
        private const int RouteViews = 20;

        /// <summary>
        /// Opens the copy, builds the one scene both routes are measured on, writes
        /// RouteViews viewpoints through the named route and saves. Returns the
        /// milliseconds the writing took, or minus one where it could not be done.
        /// </summary>
        private long WriteOneRoute(string nwfCopy, string saveAs, string route, bool throughFolder)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return -1;
            }

            Say(string.Empty);
            Say("ROUTE " + route + ", opening " + nwfCopy);
            document.Clear();

            if (!document.TryOpenFile(nwfCopy))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return -1;
            }

            int[] firstPath;
            int[] secondPath;
            string clashName;

            if (!FindClashPair(document, out firstPath, out secondPath, out clashName))
            {
                Say("UNKNOWN: no clash in this copy has two items with geometry");
                return -1;
            }

            Say("the clash measured against: " + clashName);

            // The scene every viewpoint records: one model hidden, everything else dimmed,
            // the two clashing items solid and coloured. The same sequence the writer runs.
            using (ModelItemCollection one = new ModelItemCollection())
            using (Model last = document.Models[document.Models.Count - 1])
            using (ModelItem lastRoot = last.RootItem)
            {
                one.Add(lastRoot);
                document.Models.SetHidden(one, true);
                Say("hid [" + last.FileName + "], IsHidden " + document.Models.IsHidden(one));
            }

            using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
            {
                document.Models.OverrideTemporaryTransparency(roots, 0.85);
            }

            ResetTwo(document, firstPath, secondPath, false);
            ColourTwo(document, firstPath, secondPath);
            SayThree(document, firstPath, secondPath, null, "the scene every viewpoint records");
            SayColours(document, firstPath, secondPath, "live, before any viewpoint is written");

            EnsureFolder(document, route);

            using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

                for (int i = 1; i <= RouteViews; i++)
                {
                    string name = route + " " + i;

                    if (throughFolder)
                    {
                        AddComViewIntoFolder(route, name, camera);
                    }
                    else
                    {
                        AddComView(name, camera, true);
                        MoveRootViewIntoFolder(document, route, name);
                    }
                }

                watch.Stop();
                Say("ROUTE " + route + ": " + RouteViews + " viewpoints written in " + watch.ElapsedMilliseconds
                    + " ms, " + Per(watch.ElapsedMilliseconds, RouteViews) + " ms each");

                // Everything put back before the save, exactly as the writer does it, so
                // a viewpoint holding live state rather than a snapshot comes back empty.
                document.Models.ResetAllTemporaryMaterials();
                document.Models.ResetAllHidden();

                Say("TrySaveFile to " + saveAs + " = " + document.TrySaveFile(saveAs));
                return watch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Reopens what a route saved and reads every viewpoint it wrote on the four
        /// counts, plus the two colours. Counts rather than flags, because both flags
        /// this API offers read true on a viewpoint that recorded nothing, 5o.
        /// </summary>
        private void ReadRouteBack(string file, string route)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            document.Clear();

            if (!document.TryOpenFile(file))
            {
                Say("UNKNOWN: route " + route + " saved a file that would not reopen");
                return;
            }

            Say("ROUTE " + route + ", reopened off the disk, " + Bytes(file) + " bytes");

            int[] firstPath;
            int[] secondPath;
            string clashName;
            FindClashPair(document, out firstPath, out secondPath, out clashName);

            int found = 0;
            int withCamera = 0;
            int withHidden = 0;
            int withMaterial = 0;
            int withBothColours = 0;

            using (GroupItem folder = FindFolderItem(document, route))
            {
                if (folder == null)
                {
                    Say("ROUTE " + route + ": THE FOLDER IS NOT THERE after the reopen. Nothing was recorded.");
                    return;
                }

                for (int i = 1; i <= RouteViews; i++)
                {
                    using (SavedViewpoint view = FindUnder(folder, route + " " + i))
                    {
                        if (view == null)
                        {
                            continue;
                        }

                        found++;

                        try
                        {
                            using (Viewpoint recorded = view.Viewpoint)
                            {
                                if (recorded != null)
                                {
                                    withCamera++;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // A viewpoint with no camera throws here, which is 5l's shape
                            // and is exactly what this count is for.
                        }

                        int hidden = HiddenCount(view);
                        int material = MaterialCount(view);
                        bool colours = ColoursRecorded(document, view, firstPath, secondPath);

                        if (hidden > 0)
                        {
                            withHidden++;
                        }

                        if (material > 0)
                        {
                            withMaterial++;
                        }

                        if (colours)
                        {
                            withBothColours++;
                        }

                        if (i == 1)
                        {
                            Say("   the first one: hidden " + hidden + ", material overrides " + material
                                + ", the two colours " + (colours ? "BOTH THERE" : "NOT BOTH THERE"));
                            SayRecordedColours(document, view, firstPath, secondPath);
                        }
                    }
                }
            }

            Say("ROUTE " + route + " READ BACK of " + RouteViews + ": found " + found
                + ", with a camera " + withCamera + ", hiding something " + withHidden
                + ", dimming something " + withMaterial + ", both colours " + withBothColours);

            // And what a person actually sees when one is pressed.
            using (GroupItem folder = FindFolderItem(document, route))
            {
                if (folder == null)
                {
                    return;
                }

                using (SavedViewpoint press = FindUnder(folder, route + " 1"))
                {
                    if (press == null)
                    {
                        return;
                    }

                    document.SavedViewpoints.CurrentSavedViewpoint = press;
                }
            }

            Say("   pressed " + route + " 1, what a person sees:");
            CountWhatIsSeen(document);
            SayThree(document, firstPath, secondPath, null, "   after pressing " + route + " 1");
            SayColours(document, firstPath, secondPath, "   after pressing " + route + " 1");
            document.Models.ResetAllTemporaryMaterials();
            document.Models.ResetAllHidden();
        }

        /// <summary>Red on the first item and green on the second, the order Clash Detective paints them.</summary>
        private static void ColourTwo(Document document, int[] first, int[] second)
        {
            using (ModelItem a = Resolve(document, first))
            using (ModelItem b = Resolve(document, second))
            {
                if (a != null)
                {
                    using (ModelItemCollection one = new ModelItemCollection())
                    {
                        one.Add(a);
                        document.Models.OverrideTemporaryColor(one, new Color(1.0, 0.0, 0.0));
                    }
                }

                if (b != null)
                {
                    using (ModelItemCollection one = new ModelItemCollection())
                    {
                        one.Add(b);
                        document.Models.OverrideTemporaryColor(one, new Color(0.0, 1.0, 0.0));
                    }
                }
            }
        }

        private void SayColours(Document document, int[] first, int[] second, string when)
        {
            Say("   colours " + when + ":");
            Say("      item 1 " + LiveColour(document, first));
            Say("      item 2 " + LiveColour(document, second));
        }

        /// <summary>What the viewpoint itself recorded for the two, item by item, off its own overrides.</summary>
        private void SayRecordedColours(Document document, SavedViewpoint view, int[] first, int[] second)
        {
            try
            {
                AppearanceOverrides overrides = view.GetAppearanceOverrides();

                if (overrides == null || overrides.MaterialOverrides == null)
                {
                    Say("      the viewpoint carries no appearance overrides at all");
                    return;
                }

                foreach (MaterialOverride material in overrides.MaterialOverrides)
                {
                    using (ModelItem item = material.Item)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        int[] path = PathOf(document, item);
                        string which = Same(path, first) ? "item 1" : (Same(path, second) ? "item 2" : null);

                        if (which == null)
                        {
                            continue;
                        }

                        Say("      the viewpoint recorded for " + which + ": colour " + Rgb(material.Color)
                            + ", transparency " + (material.Transparency.HasValue ? Round(material.Transparency.Value) : "none"));
                    }
                }
            }
            catch (Exception error)
            {
                Say("      reading the recorded colours threw " + error.GetType().Name);
            }
        }

        private static string LiveColour(Document document, int[] path)
        {
            if (path == null)
            {
                return "no item";
            }

            using (ModelItem item = Resolve(document, path))
            {
                if (item == null || !item.HasGeometry)
                {
                    return "no geometry";
                }

                using (ModelGeometry geometry = item.Geometry)
                {
                    return "active " + Rgb(geometry.ActiveColor) + " original " + Rgb(geometry.OriginalColor);
                }
            }
        }

        private static string Rgb(Color colour)
        {
            return "(" + Round(colour.R) + "," + Round(colour.G) + "," + Round(colour.B) + ")";
        }

        /// <summary>
        /// Whether that viewpoint recorded a colour for BOTH clashing items, read off its
        /// own MaterialOverrides rather than off the live scene. MaterialOverride carries
        /// the item, a Color and a nullable Transparency, read off the installed
        /// Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-20.
        /// </summary>
        private static bool ColoursRecorded(Document document, SavedViewpoint view, int[] first, int[] second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            bool one = false;
            bool two = false;

            try
            {
                AppearanceOverrides overrides = view.GetAppearanceOverrides();

                if (overrides == null || overrides.MaterialOverrides == null)
                {
                    return false;
                }

                foreach (MaterialOverride material in overrides.MaterialOverrides)
                {
                    using (ModelItem item = material.Item)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        int[] path = PathOf(document, item);

                        if (Same(path, first))
                        {
                            one = true;
                        }

                        if (Same(path, second))
                        {
                            two = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return one && two;
        }

        private static int HiddenCount(SavedViewpoint view)
        {
            try
            {
                VisibilityOverrides overrides = view.GetVisibilityOverrides();

                if (overrides == null)
                {
                    return 0;
                }

                using (ModelItemCollection hidden = overrides.Hidden)
                {
                    return hidden == null ? 0 : hidden.Count;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int MaterialCount(SavedViewpoint view)
        {
            try
            {
                AppearanceOverrides overrides = view.GetAppearanceOverrides();
                return overrides == null || overrides.MaterialOverrides == null ? 0 : overrides.MaterialOverrides.Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>Makes a top level folder of that name through the .NET API, which is how the tool makes one.</summary>
        private static void EnsureFolder(Document document, string name)
        {
            using (GroupItem already = FindFolderItem(document, name))
            {
                if (already != null)
                {
                    return;
                }
            }

            using (GroupItem root = document.SavedViewpoints.RootItem)
            using (FolderItem folder = new FolderItem())
            {
                folder.DisplayName = name;
                document.SavedViewpoints.AddCopy(root, folder);
            }
        }

        private static GroupItem FindFolderItem(Document document, string name)
        {
            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                SavedItemCollection children = root.Children;

                for (int i = 0; i < children.Count; i++)
                {
                    SavedItem child = children[i];
                    GroupItem group = child as GroupItem;

                    if (group != null && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        return group;
                    }

                    child.Dispose();
                }
            }

            return null;
        }

        /// <summary>Route A's tail: the view is at the COM root, so copy it into the folder and take the root one out.</summary>
        private static void MoveRootViewIntoFolder(Document document, string folderName, string name)
        {
            using (SavedViewpoint atRoot = LastAtRoot(document, name))
            {
                if (atRoot == null)
                {
                    return;
                }

                using (GroupItem folder = FindFolderItem(document, folderName))
                {
                    if (folder != null)
                    {
                        document.SavedViewpoints.AddCopy(folder, atRoot);
                    }
                }

                document.SavedViewpoints.Remove(atRoot);
            }
        }

        private static SavedViewpoint LastAtRoot(Document document, string name)
        {
            SavedItemCollection items = document.SavedViewpoints.Value;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                SavedItem item = items[i];
                SavedViewpoint view = item as SavedViewpoint;

                if (view != null && string.Equals(item.DisplayName, name, StringComparison.Ordinal))
                {
                    return view;
                }

                item.Dispose();
            }

            return null;
        }

        /// <summary>
        /// Route B: the folder's OWN saved views collection, found by walking the COM root
        /// collection for a folder view of that name, and one Add into it. One tree
        /// operation where route A costs three.
        /// </summary>
        private void AddComViewIntoFolder(string folderName, string name, Viewpoint camera)
        {
            InwOpState10 state = ComApiBridge.State;
            InwOpFolderView folder = FindComFolder(state, folderName);

            if (folder == null)
            {
                Say("ROUTE B: no COM folder view called " + folderName + " at the root, so nothing was added");
                return;
            }

            InwOpView view = (InwOpView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpView, null, null);
            view.name = name;
            view.ApplyHideAttribs = true;
            view.ApplyMaterialAttribs = true;
            view.anonview = ComApiBridge.ToInwOpAnonView(camera);
            folder.SavedViews().Add(view);
        }

        private static InwOpFolderView FindComFolder(InwOpState10 state, string name)
        {
            InwSavedViewsColl views = state.SavedViews();

            for (int i = 1; i <= views.Count; i++)
            {
                InwOpFolderView folder = views[i] as InwOpFolderView;

                if (folder != null && string.Equals(folder.name, name, StringComparison.Ordinal))
                {
                    return folder;
                }
            }

            return null;
        }

        private static string Per(long total, int count)
        {
            return count == 0 ? "UNKNOWN" : Round((double)total / count);
        }

        private static string Bytes(string file)
        {
            try
            {
                return new FileInfo(file).Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return "UNKNOWN";
            }
        }

        /// <summary>The first clash in the document whose two items both have geometry, named by index path.</summary>
        private static bool FindClashPair(Document document, out int[] first, out int[] second, out string name)
        {
            first = null;
            second = null;
            name = null;

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null)
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    using (ModelItem a = result.Item1)
                    using (ModelItem b = result.Item2)
                    {
                        if (a == null || b == null || !a.HasGeometry || !b.HasGeometry)
                        {
                            continue;
                        }

                        first = PathOf(document, a);
                        second = PathOf(document, b);
                        name = test.DisplayName + "  " + result.DisplayName;
                        return true;
                    }
                }
            }

            return false;
        }

        // ---------- PART 4 and PART 5, the shared coordinate, the worksets and the ids ----------

        /// <summary>
        /// Reads, per model, everything this API will say about where a model sits and
        /// what its elements carry. Nothing is built on any of it until this has been
        /// read, because a check that reports the wrong thing confidently is worse than
        /// no check, and a bounding box comparison is exactly that.
        ///
        /// What is asked, scan.md 5q:
        ///     Model.Transform, whether it is the identity on a correct export
        ///     every property the model ROOT carries, by tab and name, so a shared
        ///         coordinate or a base point shows up under whatever it is called
        ///     whether anything coordinate shaped is per ITEM instead
        /// And beside it, PART 5: the Workset values and the Element ID share per model.
        /// </summary>
        private void MeasureSurvey(string[] parameters, int from)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            for (int f = from; f < parameters.Length; f++)
            {
                string file = parameters[f];
                Say(string.Empty);
                Say("================ " + Path.GetFileName(file) + " ================");
                document.Clear();

                if (!document.TryOpenFile(file))
                {
                    Say("UNKNOWN: TryOpenFile returned false for " + file);
                    continue;
                }

                Say(document.Models.Count + " model(s)");

                for (int m = 0; m < document.Models.Count; m++)
                {
                    using (Model model = document.Models[m])
                    {
                        SurveyOneModel(model, m);
                    }
                }
            }
        }

        private void SurveyOneModel(Model model, int index)
        {
            Say(string.Empty);
            Say("---- model " + index + ": " + Path.GetFileName(Words(model.FileName)) + " ----");
            Say("   SourceFileName " + Words(model.SourceFileName) + ", Creator " + Words(model.Creator));

            try
            {
                Transform3D transform = model.Transform;

                if (transform == null)
                {
                    Say("   Transform is null");
                }
                else
                {
                    Vector3D t = transform.Translation;
                    Say("   Transform IsIdentity " + transform.IsIdentity()
                        + ", IsTranslation " + transform.IsTranslation()
                        + ", Translation (" + Round(t.X) + ", " + Round(t.Y) + ", " + Round(t.Z) + ")");
                }
            }
            catch (Exception error)
            {
                Say("   reading Transform threw " + error.GetType().Name + ": " + error.Message);
            }

            try
            {
                Say("   HasNorthVector " + model.HasNorthVector
                    + ", HasUpVector " + model.HasUpVector
                    + ", IsTransformReflected " + model.IsTransformReflected);

                if (model.HasNorthVector)
                {
                    UnitVector3D north = model.NorthVector;
                    Say("   NorthVector (" + Round(north.X) + ", " + Round(north.Y) + ", " + Round(north.Z) + ")");
                }
            }
            catch (Exception error)
            {
                Say("   reading the vectors threw " + error.GetType().Name);
            }

            using (ModelItem root = model.RootItem)
            {
                Say("   THE MODEL ROOT carries these properties:");
                SayEveryProperty(root, "      ");

                try
                {
                    BoundingBox3D box = root.BoundingBox();

                    if (box != null)
                    {
                        Say("   root BoundingBox min (" + Round(box.Min.X) + ", " + Round(box.Min.Y) + ", " + Round(box.Min.Z)
                            + ") max (" + Round(box.Max.X) + ", " + Round(box.Max.Y) + ", " + Round(box.Max.Z) + ")");
                    }
                }
                catch (Exception error)
                {
                    Say("   reading the root BoundingBox threw " + error.GetType().Name);
                }

                SayFirstLeafAndItsParent(root);
                WalkForWorksetsAndIds(root);
            }
        }

        /// <summary>
        /// The first item with geometry and the item above it, in full. A Revit element
        /// reaches Navisworks as a COMPOSITE item with geometry leaves under it, and the
        /// first survey counted worksets and ids over the leaves and read zero where a
        /// real run reads 860 of 1,052. Where a property lives is measured here and not
        /// assumed, because the count is only as honest as the node it is taken on.
        /// </summary>
        private void SayFirstLeafAndItsParent(ModelItem root)
        {
            try
            {
                foreach (ModelItem item in root.DescendantsAndSelf)
                {
                    using (item)
                    {
                        if (!item.HasGeometry)
                        {
                            continue;
                        }

                        Say("   THE FIRST ITEM WITH GEOMETRY, [" + Words(item.DisplayName) + "], IsComposite " + item.IsComposite + ":");
                        SayEveryProperty(item, "      ");

                        using (ModelItem parent = item.Parent)
                        {
                            if (parent == null)
                            {
                                Say("   it has no parent");
                                return;
                            }

                            Say("   THE ITEM ABOVE IT, [" + Words(parent.DisplayName) + "], IsComposite " + parent.IsComposite + ", HasGeometry " + parent.HasGeometry + ":");
                            SayEveryProperty(parent, "      ");
                        }

                        return;
                    }
                }
            }
            catch (Exception error)
            {
                Say("   reading the first leaf threw " + error.GetType().Name);
            }
        }

        /// <summary>Every tab, every property and every value on that one item, which is how a coordinate is found without guessing its name.</summary>
        private void SayEveryProperty(ModelItem item, string indent)
        {
            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        Say(indent + "no property tabs at all");
                        return;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (tab)
                        {
                            Say(indent + "[" + Words(tab.DisplayName) + "] internal " + Words(tab.Name));

                            using (DataPropertyCollection properties = tab.Properties)
                            {
                                for (int i = 0; i < properties.Count; i++)
                                {
                                    using (DataProperty property = properties[i])
                                    {
                                        Say(indent + "   " + Words(property.DisplayName)
                                            + "  internal " + Words(property.Name)
                                            + "  =  " + AnyText(property));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Say(indent + "reading the properties threw " + error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>
        /// Walks every item under that root once and counts the two things a set and a
        /// report column need: whether anything carries a Workset at all and under what
        /// names, and what share of items carry an Element ID. Counted over items with
        /// GEOMETRY, because that is what a clash test can find and what a report row is.
        /// Anything whose name looks like a coordinate is named too, so a per item shared
        /// coordinate cannot be missed by looking only at the root.
        /// </summary>
        private void WalkForWorksetsAndIds(ModelItem root)
        {
            int items = 0;
            int geometry = 0;
            int elements = 0;
            int withWorkset = 0;
            int withId = 0;
            Dictionary<string, int> worksets = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> coordinateish = new Dictionary<string, int>(StringComparer.Ordinal);

            try
            {
                foreach (ModelItem item in root.DescendantsAndSelf)
                {
                    using (item)
                    {
                        items++;

                        if (item.HasGeometry)
                        {
                            geometry++;
                        }

                        if (items <= 400)
                        {
                            NoteCoordinateish(item, coordinateish);
                        }

                        // THE REVIT ELEMENT AND NOT THE GEOMETRY LEAF. Id and Workset sit
                        // on the [Element] tab of the COMPOSITE item, and the geometry
                        // solids under it carry neither. Counting them over the leaves
                        // read zero where a real run reads 860 of 1,052 ids, so the node
                        // is chosen by the tab it carries and not by having geometry.
                        string workset;
                        string id;

                        if (!ReadElementTab(item, out id, out workset))
                        {
                            continue;
                        }

                        elements++;

                        if (workset.Length > 0)
                        {
                            withWorkset++;
                            Bump(worksets, workset);
                        }

                        if (id.Length > 0)
                        {
                            withId++;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Say("   the walk threw " + error.GetType().Name + ": " + error.Message);
            }

            Say("   " + items + " item(s), " + geometry + " with geometry, " + elements + " Revit element(s)");
            Say("   WORKSET: " + withWorkset + " of " + elements + " carry one, "
                + worksets.Count + " distinct name(s)");

            foreach (KeyValuePair<string, int> pair in worksets)
            {
                Say("      " + pair.Key + "  " + pair.Value);
            }

            Say("   ELEMENT ID: " + withId + " of " + elements + " carry one, " + Share(withId, elements));

            Say("   property names that look like a coordinate, off the first 200 of each:");

            if (coordinateish.Count == 0)
            {
                Say("      none");
            }

            foreach (KeyValuePair<string, int> pair in coordinateish)
            {
                Say("      " + pair.Key + "  " + pair.Value);
            }
        }

        /// <summary>
        /// The Id and the Workset off that item's OWN [Element] tab, and whether it has
        /// one at all. A Revit element reaches Navisworks carrying LcRevitData_Element,
        /// and the geometry under it does not, so this is what tells an element from a
        /// solid without guessing at the tree shape.
        /// </summary>
        private static bool ReadElementTab(ModelItem item, out string id, out string workset)
        {
            id = string.Empty;
            workset = string.Empty;

            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return false;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        if (!string.Equals(Words(tab.Name), ElementTabInternalName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    string name = Words(property.DisplayName);

                                    if (string.Equals(name, "Id", StringComparison.OrdinalIgnoreCase))
                                    {
                                        id = AnyText(property);
                                    }
                                    else if (string.Equals(name, "Workset", StringComparison.OrdinalIgnoreCase))
                                    {
                                        workset = AnyText(property);
                                    }
                                }
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>What Navisworks calls the tab a Revit element carries, read off a real NWC on 2026-09-20.</summary>
        private const string ElementTabInternalName = "LcRevitData_Element";

        private static readonly string[] WorksetNames = { "Workset" };

        private static readonly string[] ElementIdNames = { "Id", "Element Id", "ElementId", "Element ID" };

        private static readonly string[] CoordinateWords =
            { "coordinate", "base point", "survey", "origin", "north", "elevation", "location", "shared", "level" };

        private static void NoteCoordinateish(ModelItem item, Dictionary<string, int> into)
        {
            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (tab)
                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    string name = Words(property.DisplayName);

                                    for (int w = 0; w < CoordinateWords.Length; w++)
                                    {
                                        if (name.IndexOf(CoordinateWords[w], StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            Bump(into, "[" + Words(tab.DisplayName) + "] " + name + " = " + AnyText(property));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // One unreadable item costs one item, never the walk.
            }
        }

        private static void Bump(Dictionary<string, int> into, string key)
        {
            int already;
            into.TryGetValue(key, out already);
            into[key] = already + 1;
        }

        private static string Share(int part, int whole)
        {
            if (whole == 0)
            {
                return "UNKNOWN";
            }

            return Round(100.0 * part / whole) + "%";
        }

        /// <summary>The first of those property display names that answers on that item, or empty.</summary>
        private static string FirstOf(ModelItem item, string[] wanted)
        {
            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return string.Empty;
                    }

                    for (int w = 0; w < wanted.Length; w++)
                    {
                        foreach (PropertyCategory tab in tabs)
                        {
                            using (DataPropertyCollection properties = tab.Properties)
                            {
                                for (int i = 0; i < properties.Count; i++)
                                {
                                    using (DataProperty property = properties[i])
                                    {
                                        if (!string.Equals(property.DisplayName, wanted[w], StringComparison.OrdinalIgnoreCase))
                                        {
                                            continue;
                                        }

                                        string text = AnyText(property);

                                        if (text.Length > 0)
                                        {
                                            return text;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// A property's value as text WHATEVER its type. The probe's older Text reader
        /// answers on three types and empty on the rest, which would read a double valued
        /// coordinate as absent. Every type the API offers is handled here.
        /// </summary>
        private static string AnyText(DataProperty property)
        {
            try
            {
                using (VariantData value = property.Value)
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
                        case VariantDataType.Int32:
                            return value.ToInt32().ToString(System.Globalization.CultureInfo.InvariantCulture);
                        case VariantDataType.Boolean:
                            return value.ToBoolean().ToString();
                        case VariantDataType.DateTime:
                            return value.ToDateTime().ToString("s", System.Globalization.CultureInfo.InvariantCulture);
                        case VariantDataType.Double:
                        case VariantDataType.DoubleAngle:
                        case VariantDataType.DoubleArea:
                        case VariantDataType.DoubleLength:
                        case VariantDataType.DoubleVolume:
                            return Round(value.ToAnyDouble());
                        case VariantDataType.NamedConstant:
                            using (NamedConstant named = value.ToNamedConstant())
                            {
                                return named == null ? string.Empty : Words(named.DisplayName);
                            }
                        case VariantDataType.Point3D:
                            Point3D point = value.ToPoint3D();
                            return "(" + Round(point.X) + ", " + Round(point.Y) + ", " + Round(point.Z) + ")";
                        default:
                            return Words(value.ToString());
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string Words(string value)
        {
            return value ?? string.Empty;
        }

        // ---------- PART 2, what a viewpoint records of a COLOUR ----------

        /// <summary>
        /// Three colour pairs on the same two clashing items, each recorded into its own
        /// viewpoint and read back before and after a save, because the first route pass
        /// recorded a colour for item 1 and NOTHING for item 2, whose original colour
        /// already was the green it was being given.
        ///
        /// The question this settles: does a viewpoint record a colour override for an
        /// item whose original colour already IS that colour. If it does not, then a read
        /// back that insists on both colours would fail a viewpoint that is perfectly
        /// right, and the read back has to be written to know the difference.
        /// </summary>
        private void MeasureColour(string nwfCopy)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            document.Clear();

            if (!document.TryOpenFile(nwfCopy))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            int[] firstPath;
            int[] secondPath;
            string clashName;

            if (!FindClashPair(document, out firstPath, out secondPath, out clashName))
            {
                Say("UNKNOWN: no clash in this copy has two items with geometry");
                return;
            }

            Say("the clash measured against: " + clashName);
            Say("ORIGINAL colours, before anything is overridden:");
            Say("   item 1 " + LiveColour(document, firstPath));
            Say("   item 2 " + LiveColour(document, secondPath));

            EnsureFolder(document, "C");

            Color[][] pairs =
            {
                new[] { new Color(1.0, 0.0, 0.0), new Color(0.0, 1.0, 0.0) },
                new[] { new Color(1.0, 0.0, 0.0), new Color(0.0, 0.0, 1.0) },
                new[] { new Color(1.0, 1.0, 0.0), new Color(1.0, 0.0, 1.0) }
            };

            for (int p = 0; p < pairs.Length; p++)
            {
                string name = "C " + (p + 1);
                Say(string.Empty);
                Say("PAIR " + (p + 1) + ": item 1 " + Rgb(pairs[p][0]) + ", item 2 " + Rgb(pairs[p][1]));

                document.Models.ResetAllTemporaryMaterials();
                document.Models.ResetAllHidden();

                using (ModelItemCollection one = new ModelItemCollection())
                using (Model last = document.Models[document.Models.Count - 1])
                using (ModelItem lastRoot = last.RootItem)
                {
                    one.Add(lastRoot);
                    document.Models.SetHidden(one, true);
                }

                using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
                {
                    document.Models.OverrideTemporaryTransparency(roots, 0.85);
                }

                ResetTwo(document, firstPath, secondPath, false);
                PaintOne(document, firstPath, pairs[p][0]);
                PaintOne(document, secondPath, pairs[p][1]);

                Say("   live after painting: item 1 " + LiveColour(document, firstPath));
                Say("   live after painting: item 2 " + LiveColour(document, secondPath));

                using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
                {
                    AddComView(name, camera, true);
                    MoveRootViewIntoFolder(document, "C", name);
                }

                SayBothRecorded(document, name, firstPath, secondPath, "   before the save");
            }

            document.Models.ResetAllTemporaryMaterials();
            document.Models.ResetAllHidden();

            string saved = Path.Combine(Path.GetDirectoryName(nwfCopy), "probe-colour-saved.nwf");
            Say(string.Empty);
            Say("TrySaveFile to " + saved + " = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            Say("reopened off the disk, " + Bytes(saved) + " bytes");
            FindClashPair(document, out firstPath, out secondPath, out clashName);

            for (int p = 0; p < pairs.Length; p++)
            {
                string name = "C " + (p + 1);
                Say(string.Empty);
                SayBothRecorded(document, name, firstPath, secondPath, "AFTER THE REOPEN");

                using (GroupItem folder = FindFolderItem(document, "C"))
                {
                    if (folder == null)
                    {
                        continue;
                    }

                    using (SavedViewpoint press = FindUnder(folder, name))
                    {
                        if (press == null)
                        {
                            continue;
                        }

                        document.SavedViewpoints.CurrentSavedViewpoint = press;
                    }
                }

                Say("   pressed " + name + ": item 1 " + LiveColour(document, firstPath));
                Say("   pressed " + name + ": item 2 " + LiveColour(document, secondPath));
                document.Models.ResetAllTemporaryMaterials();
                document.Models.ResetAllHidden();
            }

            Say(string.Empty);
            Say("A colour is recorded only where the viewpoint's own MaterialOverrides name the item.");
        }

        private static void PaintOne(Document document, int[] path, Color colour)
        {
            using (ModelItem item = Resolve(document, path))
            {
                if (item == null)
                {
                    return;
                }

                using (ModelItemCollection one = new ModelItemCollection())
                {
                    one.Add(item);
                    document.Models.OverrideTemporaryColor(one, colour);
                }
            }
        }

        /// <summary>Whether that viewpoint names each of the two items in its own overrides, said one item at a time.</summary>
        private void SayBothRecorded(Document document, string name, int[] first, int[] second, string when)
        {
            using (GroupItem folder = FindFolderItem(document, "C"))
            {
                if (folder == null)
                {
                    Say(when + ": no folder C");
                    return;
                }

                using (SavedViewpoint view = FindUnder(folder, name))
                {
                    if (view == null)
                    {
                        Say(when + ": " + name + " NOT FOUND");
                        return;
                    }

                    Say(when + ", " + name + ": " + MaterialCount(view) + " material override(s) in all");
                    Say("      item 1 " + OverrideFor(document, view, first));
                    Say("      item 2 " + OverrideFor(document, view, second));
                }
            }
        }

        /// <summary>What that viewpoint recorded for that one item, or that it named it at all.</summary>
        private static string OverrideFor(Document document, SavedViewpoint view, int[] path)
        {
            if (path == null)
            {
                return "no item";
            }

            try
            {
                AppearanceOverrides overrides = view.GetAppearanceOverrides();

                if (overrides == null || overrides.MaterialOverrides == null)
                {
                    return "the viewpoint carries no appearance overrides at all";
                }

                foreach (MaterialOverride material in overrides.MaterialOverrides)
                {
                    using (ModelItem item = material.Item)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        if (!Same(PathOf(document, item), path))
                        {
                            continue;
                        }

                        return "NAMED, colour " + Rgb(material.Color) + ", transparency "
                            + (material.Transparency.HasValue ? Round(material.Transparency.Value) : "none");
                    }
                }

                return "NOT NAMED by any override";
            }
            catch (Exception error)
            {
                return "reading threw " + error.GetType().Name;
            }
        }

        // ---------- why the penetration rule has never moved a clash ----------

        /// <summary>
        /// Opens an NWF this tool wrote and reads, for the first clashes in it, exactly
        /// what the penetration rule reads: the item each side gives back, and the
        /// Category property found on it and on each of the four ancestors above it, the
        /// way ClashHarvest.FirstPropertyOn looks for one.
        ///
        /// WHY. Two real runs moved ZERO clashes to Reviewed and put every single clash
        /// in the one bucket "not a service against a solid", with zero in all five other
        /// buckets. A rule that never fires and never says why is worse than no rule, and
        /// which of the two it is, no category at all or a category nobody expected, is
        /// not guessable off the log.
        /// </summary>
        private void MeasurePenetration(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            Say("document units " + document.Units + ", " + document.Models.Count + " model(s)");

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;
            int shown = 0;

            for (int t = 0; t < tests.Tests.Count && shown < 12; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null || test.Children.Count == 0)
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count && shown < 12; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    shown++;
                    Say(string.Empty);
                    Say("CLASH " + shown + ": " + test.DisplayName + "  " + result.DisplayName);
                    Say("   --- what Selection1 and Selection2 give, which is what the penetration rule reads ---");
                    SaySide(document, result.Selection1, "side 1");
                    SaySide(document, result.Selection2, "side 2");
                    Say("   --- what Item1 and Item2 give, which is what the harvest reads ---");
                    SayItem(document, result.Item1, "item 1");
                    SayItem(document, result.Item2, "item 2");
                }
            }

            Say(string.Empty);
            Say("The rule asks: is one side a SERVICE category and the other a SOLID category.");
            Say("services: Pipes, Pipe Fittings, Ducts, Duct Fittings, Cable Trays, Conduits and the rest.");
            Say("solids: Walls, Floors, Roofs, Structural Foundations.");
        }

        /// <summary>
        /// What one clash side gives back and what Category reads on it and above it. The
        /// SAME four levels and the same property names Penetrations uses, so what this
        /// prints is what that rule sees and not a near relative of it.
        /// </summary>
        private void SaySide(Document document, ModelItemCollection selection, string which)
        {
            using (selection)
            {
                if (selection == null || selection.Count == 0)
                {
                    Say("   " + which + ": nothing selected");
                    return;
                }

                Say("   " + which + ": " + selection.Count + " item(s) selected, the rule reads the FIRST");

                using (ModelItem item = selection[0])
                {
                    if (item == null)
                    {
                        Say("      the first item is null");
                        return;
                    }

                    ModelItem walker = item;

                    for (int level = 0; level <= 4 && walker != null; level++)
                    {
                        Say("      level " + level + " [" + Words(walker.DisplayName) + "]"
                            + " geometry " + walker.HasGeometry
                            + " composite " + walker.IsComposite
                            + "   Category = [" + FirstOf(walker, PenetrationCategoryNames) + "]"
                            + "   every Category on it: " + EveryCategoryOn(walker));

                        ModelItem parent = walker.Parent;

                        if (level > 0)
                        {
                            walker.Dispose();
                        }

                        walker = parent;
                    }

                    if (walker != null)
                    {
                        walker.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// The same walk on the item ClashResult.Item1 gives, which is the OTHER route to
        /// the same clash side and the one the harvest reads its ids off. If one throws
        /// and the other does not, that is the whole answer.
        /// </summary>
        private void SayItem(Document document, ModelItem item, string which)
        {
            if (item == null)
            {
                Say("   " + which + ": null");
                return;
            }

            ModelItem walker = item;

            for (int level = 0; level <= 4 && walker != null; level++)
            {
                Say("      " + which + " level " + level + " [" + Words(walker.DisplayName) + "]"
                    + " geometry " + walker.HasGeometry
                    + " composite " + walker.IsComposite
                    + "   Category = [" + FirstOf(walker, PenetrationCategoryNames) + "]"
                    + "   every Category on it: " + EveryCategoryOn(walker));

                ModelItem parent = walker.Parent;
                walker.Dispose();
                walker = parent;
            }

            if (walker != null)
            {
                walker.Dispose();
            }
        }

        /// <summary>The same three names PenetrationSettings.DefaultCategoryNames holds, in the same order.</summary>
        private static readonly string[] PenetrationCategoryNames = { "Category", "Revit Category", "Element Category" };

        /// <summary>
        /// EVERY property called Category on that item, with the tab it came from, because
        /// the rule takes the FIRST one and a tab order nobody looked at would decide
        /// which. Measured rather than reasoned about.
        /// </summary>
        private static string EveryCategoryOn(ModelItem item)
        {
            List<string> found = new List<string>();

            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return "no tabs";
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    if (string.Equals(Words(property.DisplayName), "Category", StringComparison.OrdinalIgnoreCase))
                                    {
                                        found.Add("[" + Words(tab.DisplayName) + "]=" + AnyText(property));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                return "reading threw " + error.GetType().Name;
            }

            return found.Count == 0 ? "none" : string.Join("  ", found.ToArray());
        }

        // ---------- 5s, 5t and 5u, the whole of the worksets round's PART 1 ----------

        /// <summary>
        /// THREE MEASUREMENTS IN ONE PASS, because Navisworks is started once for all of
        /// them and because all three walk the same ten files.
        ///
        ///   5t  every distinct workset name in the folder, and which models carry it,
        ///       grouped so a name differing only by CASE sits beside its twin
        ///   5u  the shared site of every model, the architecture reference of each
        ///       group, and how many groups would FAIL on a model naming Internal
        ///   5s  for the FIRST file handed that has clash results, every clash whose
        ///       service side reports no readable size, dumped property by property
        ///
        /// NOTHING HERE DECIDES ANYTHING. It reads and writes what it read. The rules
        /// this feeds are built afterwards, from this file and from nothing else.
        /// </summary>
        private void MeasureCensus(string[] parameters, int from)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            // 5t, every workset name against the models that carry it, across the folder.
            Dictionary<string, List<string>> worksets = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            // 5u, one row per group.
            List<string> siteRows = new List<string>();
            int groupsWithInternal = 0;
            int groupsWithNoSite = 0;
            int groupsRead = 0;
            bool sizeDone = false;

            for (int f = from; f < parameters.Length; f++)
            {
                string file = parameters[f];
                Say(string.Empty);
                Say("================ " + Path.GetFileName(file) + " ================");
                document.Clear();

                if (!document.TryOpenFile(file))
                {
                    Say("UNKNOWN: TryOpenFile returned false for " + file);
                    continue;
                }

                groupsRead++;
                string group = Path.GetFileName(file);
                bool anyInternal = false;
                bool anyWithoutSite = false;
                string reference = "none";

                Say(document.Models.Count + " model(s), document units " + document.Units);

                for (int m = 0; m < document.Models.Count; m++)
                {
                    using (Model model = document.Models[m])
                    {
                        string name = Path.GetFileName(Words(model.FileName));
                        string discipline = DisciplineOf(name);
                        string site;

                        using (ModelItem root = model.RootItem)
                        {
                            site = SiteOn(root);
                        }

                        if (site.Length == 0)
                        {
                            anyWithoutSite = true;
                        }
                        else if (string.Equals(site, "Internal", StringComparison.Ordinal))
                        {
                            anyInternal = true;
                        }

                        if (string.Equals(discipline, "AR", StringComparison.Ordinal) && string.Equals(reference, "none", StringComparison.Ordinal))
                        {
                            reference = name;
                        }

                        List<string> mine = WorksetsIn(model);
                        Say("   " + (discipline.Length == 0 ? "??" : discipline) + "  " + name
                            + "   site [" + (site.Length == 0 ? "NONE" : site) + "]"
                            + "   " + mine.Count + " workset name(s)");

                        for (int w = 0; w < mine.Count; w++)
                        {
                            Say("      " + mine[w]);

                            if (!worksets.ContainsKey(mine[w]))
                            {
                                worksets[mine[w]] = new List<string>();
                            }

                            if (!worksets[mine[w]].Contains(name))
                            {
                                worksets[mine[w]].Add(name);
                            }
                        }
                    }
                }

                if (anyInternal)
                {
                    groupsWithInternal++;
                }

                if (anyWithoutSite)
                {
                    groupsWithNoSite++;
                }

                siteRows.Add(group + "   reference " + reference
                    + "   names Internal: " + anyInternal
                    + "   a model with no site at all: " + anyWithoutSite);

                if (!sizeDone && SayUnmeasuredServices(document))
                {
                    sizeDone = true;
                }
            }

            SayWorksets(worksets);
            SaySites(siteRows, groupsRead, groupsWithInternal, groupsWithNoSite);
        }

        // ---------- 5t ----------

        /// <summary>
        /// The whole folder's workset names, grouped so a name differing from another
        /// ONLY BY CASE sits beside it and one differing by more than case sits beside it
        /// too and is MARKED as a different word. That grouping is the whole point: one
        /// is a correction the tool can make and the other is a typo a person has to fix.
        /// </summary>
        private void SayWorksets(Dictionary<string, List<string>> worksets)
        {
            Say(string.Empty);
            Say("================ 5t, EVERY WORKSET NAME IN THIS FOLDER ================");
            Say(worksets.Count + " distinct name(s) across every model read");

            List<string> names = new List<string>(worksets.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);

            // Grouped on the name lowered, so two spellings of one word land together.
            Dictionary<string, List<string>> byLowered = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            for (int i = 0; i < names.Count; i++)
            {
                string key = names[i].ToLowerInvariant();

                if (!byLowered.ContainsKey(key))
                {
                    byLowered[key] = new List<string>();
                }

                byLowered[key].Add(names[i]);
            }

            Say(string.Empty);
            Say("--- names that differ ONLY BY CASE, which a correction rule may fix ---");
            int caseOnly = 0;

            foreach (KeyValuePair<string, List<string>> pair in byLowered)
            {
                if (pair.Value.Count < 2)
                {
                    continue;
                }

                caseOnly++;
                Say("   the one word \"" + pair.Key + "\" is spelled " + pair.Value.Count + " ways:");

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    Say("      [" + pair.Value[i] + "] in " + Joined(worksets[pair.Value[i]]));
                }
            }

            if (caseOnly == 0)
            {
                Say("   none");
            }

            Say(string.Empty);
            Say("--- names that differ by MORE THAN CASE, which are a different word and may be a typo ---");
            int near = 0;

            for (int i = 0; i < names.Count; i++)
            {
                for (int j = i + 1; j < names.Count; j++)
                {
                    if (string.Equals(names[i], names[j], StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    int distance = Distance(names[i].ToLowerInvariant(), names[j].ToLowerInvariant());

                    if (distance == 0 || distance > 2)
                    {
                        continue;
                    }

                    near++;
                    Say("   [" + names[i] + "] and [" + names[j] + "] differ by " + distance
                        + " letter(s), which is MORE than case, so they are two different words");
                    Say("      [" + names[i] + "] in " + Joined(worksets[names[i]]));
                    Say("      [" + names[j] + "] in " + Joined(worksets[names[j]]));
                }
            }

            if (near == 0)
            {
                Say("   none");
            }

            Say(string.Empty);
            Say("--- every name and every model that carries it ---");

            for (int i = 0; i < names.Count; i++)
            {
                Say("   [" + names[i] + "]  " + worksets[names[i]].Count + " model(s): " + Joined(worksets[names[i]]));
            }
        }

        /// <summary>
        /// How many single letter edits turn one word into the other, capped so a long
        /// walk cannot cost the pass. Plain Levenshtein, no library, because Core has
        /// none and this is a probe.
        /// </summary>
        private static int Distance(string a, string b)
        {
            if (Math.Abs(a.Length - b.Length) > 2)
            {
                return int.MaxValue;
            }

            int[] previous = new int[b.Length + 1];
            int[] current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++)
            {
                previous[j] = j;
            }

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int best = Math.Min(current[j - 1] + 1, previous[j] + 1);
                    current[j] = Math.Min(best, previous[j - 1] + cost);
                }

                int[] swap = previous;
                previous = current;
                current = swap;
            }

            return previous[b.Length];
        }

        /// <summary>Every distinct workset name in one model, read off the Revit element tab.</summary>
        private static List<string> WorksetsIn(Model model)
        {
            List<string> found = new List<string>();

            try
            {
                using (ModelItem root = model.RootItem)
                {
                    foreach (ModelItem item in root.DescendantsAndSelf)
                    {
                        using (item)
                        {
                            string id;
                            string workset;

                            if (!ReadElementTab(item, out id, out workset))
                            {
                                continue;
                            }

                            if (workset.Length > 0 && !found.Contains(workset))
                            {
                                found.Add(workset);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // A walk that threw part way has read part of the model, and the names it
                // did read are still real names. Nothing is invented and nothing is lost.
            }

            found.Sort(StringComparer.Ordinal);
            return found;
        }

        // ---------- 5u ----------

        private void SaySites(IList<string> rows, int groups, int withInternal, int withNoSite)
        {
            Say(string.Empty);
            Say("================ 5u, THE SHARED SITE OF EVERY MODEL ================");

            for (int i = 0; i < rows.Count; i++)
            {
                Say("   " + rows[i]);
            }

            Say(string.Empty);
            Say("GROUPS READ                                  : " + groups);
            Say("groups with a model naming Internal          : " + withInternal);
            Say("groups with a model carrying no site at all  : " + withNoSite);
            Say("GROUPS THAT WOULD FAIL UNDER Q70             : " + WouldFail(rows));
            Say("Q70 fails a group where ANY model names Internal OR carries no site at all.");
        }

        /// <summary>How many rows say either fault, counted once per group and not once per model.</summary>
        private static int WouldFail(IList<string> rows)
        {
            int failed = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].IndexOf("names Internal: True", StringComparison.Ordinal) >= 0
                    || rows[i].IndexOf("no site at all: True", StringComparison.Ordinal) >= 0)
                {
                    failed++;
                }
            }

            return failed;
        }

        private static string SiteOn(ModelItem root)
        {
            try
            {
                using (PropertyCategoryCollection tabs = root.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return string.Empty;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        if (!string.Equals(Words(tab.Name), "LcRevitPropertyLocation", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    if (string.Equals(Words(property.Name), "revit_ProjectLocation", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return AnyText(property);
                                    }
                                }
                            }
                        }

                        return string.Empty;
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        /// <summary>Part 5 of the file name, split the way the tool splits one.</summary>
        private static string DisciplineOf(string file)
        {
            string[] parts = Path.GetFileNameWithoutExtension(file).Split('-');
            return parts.Length >= 5 ? parts[4] : string.Empty;
        }

        // ---------- 5s ----------

        /// <summary>
        /// Every clash in this document whose SERVICE side reports no readable size,
        /// dumped property by property. Returns whether the document held clashes at all.
        ///
        /// IT MIRRORS Penetrations EXACTLY: Item1 and Item2, never Selection1 and
        /// Selection2, five levels up, the same three category names and the same six
        /// size property names. What this prints is what that rule sees, so the answer to
        /// "is it the models or is it the reader" is read off one run and not argued about.
        /// </summary>
        private bool SayUnmeasuredServices(Document document)
        {
            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests;

            try
            {
                tests = document.GetClash().TestsData;
            }
            catch (Exception)
            {
                return false;
            }

            if (tests == null || tests.Tests.Count == 0)
            {
                return false;
            }

            int clashes = 0;
            int serviceAgainstSolid = 0;
            int noSize = 0;
            int shown = 0;
            Dictionary<string, int> byCategory = new Dictionary<string, int>(StringComparer.Ordinal);

            Say(string.Empty);
            Say("================ 5s, THE SERVICES WITH NO READABLE SIZE ================");

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null)
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    clashes++;

                    string firstCategory;
                    bool firstSized;
                    string secondCategory;
                    bool secondSized;

                    using (ModelItem a = result.Item1)
                    using (ModelItem b = result.Item2)
                    {
                        firstCategory = CategoryUp(a, out firstSized);
                        secondCategory = CategoryUp(b, out secondSized);

                        bool firstIsService = IsService(firstCategory);
                        bool secondIsService = IsService(secondCategory);
                        bool firstIsSolid = IsSolid(firstCategory);
                        bool secondIsSolid = IsSolid(secondCategory);

                        if (!((firstIsService && secondIsSolid) || (secondIsService && firstIsSolid)))
                        {
                            continue;
                        }

                        serviceAgainstSolid++;
                        bool serviceIsFirst = firstIsService && secondIsSolid;
                        bool sized = serviceIsFirst ? firstSized : secondSized;

                        if (sized)
                        {
                            continue;
                        }

                        noSize++;
                        string category = serviceIsFirst ? firstCategory : secondCategory;
                        Bump(byCategory, category);

                        if (shown < 8)
                        {
                            shown++;
                            Say(string.Empty);
                            Say("NO SIZE " + shown + ": " + test.DisplayName + "  " + result.DisplayName
                                + "   the service side is [" + category + "]");
                            SayEveryLevel(serviceIsFirst ? a : b);
                        }
                    }
                }
            }

            Say(string.Empty);
            Say("clashes in this document              : " + clashes);
            Say("a service against a solid             : " + serviceAgainstSolid);
            Say("of those, NO READABLE SIZE            : " + noSize);
            Say("by category:");

            foreach (KeyValuePair<string, int> pair in byCategory)
            {
                Say("   [" + pair.Key + "]  " + pair.Value);
            }

            Say(string.Empty);
            Say("READ THE DUMPS ABOVE. If a level carries Diameter, Width, Height, Size,");
            Say("Nominal Diameter or Overall Size and the reader still said no size, the");
            Say("reader is on the wrong node and it is a BUG. If no level carries any of");
            Say("the six, the service genuinely has no size property and PART 5 names them.");
            return true;
        }

        /// <summary>Every level of one clash side, with its tabs and every size property on it.</summary>
        private void SayEveryLevel(ModelItem item)
        {
            ModelItem walker = item;

            for (int level = 0; level <= 4 && walker != null; level++)
            {
                Say("   level " + level + " [" + Words(walker.DisplayName) + "]"
                    + " geometry " + walker.HasGeometry + " composite " + walker.IsComposite);
                Say("      tabs: " + TabNames(walker));
                Say("      size properties on it: " + SizePropertiesOn(walker));

                ModelItem parent = walker.Parent;

                if (level > 0)
                {
                    walker.Dispose();
                }

                walker = parent;
            }

            if (walker != null)
            {
                walker.Dispose();
            }
        }

        private static string TabNames(ModelItem item)
        {
            List<string> names = new List<string>();

            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return "none";
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        names.Add(Words(tab.DisplayName));
                    }
                }
            }
            catch (Exception error)
            {
                return "reading threw " + error.GetType().Name;
            }

            return names.Count == 0 ? "none" : string.Join(", ", names.ToArray());
        }

        /// <summary>The six names SizeSettings.DefaultPropertyNames holds, in the same order.</summary>
        private static readonly string[] SizePropertyNames =
            { "Diameter", "Width", "Height", "Size", "Nominal Diameter", "Overall Size" };
        /// <summary>
        /// Whether ItemSizes.Read WOULD get a number off that item, which is a narrower
        /// question than whether a size property is there at all. It takes the value only
        /// where the kind is DoubleLength or Double, so a Size written as a STRING is a
        /// property that is there and unreadable, and that difference is the whole of 5s.
        /// </summary>
        private static bool ReadableSizeOn(ModelItem item)
        {
            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return false;
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    string name = Words(property.DisplayName);

                                    for (int s = 0; s < SizePropertyNames.Length; s++)
                                    {
                                        if (!string.Equals(name, SizePropertyNames[s], StringComparison.OrdinalIgnoreCase))
                                        {
                                            continue;
                                        }

                                        using (VariantData value = property.Value)
                                        {
                                            if (value != null
                                                && (value.DataType == VariantDataType.DoubleLength
                                                    || value.DataType == VariantDataType.Double))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>The VariantData kind of that property, which is what ItemSizes judges on.</summary>
        private static string TypeOf(DataProperty property)
        {
            try
            {
                using (VariantData value = property.Value)
                {
                    return value == null ? "null" : value.DataType.ToString();
                }
            }
            catch (Exception)
            {
                return "threw";
            }
        }



        private static string SizePropertiesOn(ModelItem item)
        {
            List<string> found = new List<string>();

            try
            {
                using (PropertyCategoryCollection tabs = item.PropertyCategories)
                {
                    if (tabs == null)
                    {
                        return "none";
                    }

                    foreach (PropertyCategory tab in tabs)
                    {
                        using (DataPropertyCollection properties = tab.Properties)
                        {
                            for (int i = 0; i < properties.Count; i++)
                            {
                                using (DataProperty property = properties[i])
                                {
                                    string name = Words(property.DisplayName);

                                    for (int s = 0; s < SizePropertyNames.Length; s++)
                                    {
                                        if (string.Equals(name, SizePropertyNames[s], StringComparison.OrdinalIgnoreCase))
                                        {
                                            // THE TYPE AS WELL AS THE VALUE. ItemSizes only
                                            // takes DoubleLength or Double and leaves every
                                            // other kind out, so a size written as a STRING
                                            // is a property that is there and unreadable.
                                            found.Add("[" + Words(tab.DisplayName) + "] " + name
                                                + " = " + AnyText(property) + " <" + TypeOf(property) + ">");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                return "reading threw " + error.GetType().Name;
            }

            return found.Count == 0 ? "NONE of the six" : string.Join("   ", found.ToArray());
        }

        /// <summary>The category off this item or the four above it, and whether any level carried a size.</summary>
        private static string CategoryUp(ModelItem item, out bool sized)
        {
            sized = false;
            string category = string.Empty;

            if (item == null)
            {
                return category;
            }

            ModelItem walker = item;

            for (int level = 0; level <= 4 && walker != null; level++)
            {
                if (category.Length == 0)
                {
                    category = FirstOf(walker, PenetrationCategoryNames);
                }

                if (!sized && ReadableSizeOn(walker))
                {
                    sized = true;
                }

                ModelItem parent = walker.Parent;

                if (level > 0)
                {
                    walker.Dispose();
                }

                walker = parent;
            }

            if (walker != null)
            {
                walker.Dispose();
            }

            return category;
        }

        private static readonly string[] ServiceCategories =
        {
            "Pipes", "Pipe Fittings", "Pipe Accessories", "Pipe Insulation",
            "Ducts", "Duct Fittings", "Duct Accessories", "Flex Pipes", "Flex Ducts",
            "Cable Trays", "Cable Tray Fittings", "Conduits", "Conduit Fittings"
        };

        private static readonly string[] SolidCategories =
            { "Walls", "Floors", "Roofs", "Structural Foundations" };

        private static bool IsService(string category)
        {
            return Names(ServiceCategories, category);
        }

        private static bool IsSolid(string category)
        {
            return Names(SolidCategories, category);
        }

        private static bool Names(string[] list, string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return false;
            }

            for (int i = 0; i < list.Length; i++)
            {
                if (string.Equals(list[i], category, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Joined(IList<string> values)
        {
            string[] array = new string[values.Count];
            values.CopyTo(array, 0);
            return string.Join(", ", array);
        }

        // ---------- 5v, 5w and 5x, the whole of the drift round's PART 1 ----------

        /// <summary>
        /// THREE MEASUREMENTS IN ONE PASS, because Navisworks is started once for all of
        /// them. The fourth, 5y's count over his past logs, needs no Navisworks at all.
        ///
        ///   5v  can a set be REPLACED without losing the clash test pointing at it, its
        ///       recorded results, or a status a person set. Five read backs, after a
        ///       save and a reopen off the disk
        ///   5w  what every set in the file is ACTUALLY asking, read off the set through
        ///       SelectionSet.Search, which is a getter nothing in this tool has read
        ///   5x  what taking ONE MODEL out of an open document costs the sets, the tests,
        ///       the results, the statuses and the viewpoints that point into it
        ///
        /// NOTHING HERE DECIDES ANYTHING. It reads and writes what it read.
        /// </summary>
        private void MeasureDrift(string[] parameters, int from)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (parameters.Length <= from)
            {
                Say("UNKNOWN: no file handed to the drift probe");
                return;
            }

            // 5v goes first and on the SMALLEST file, because it saves and reopens twice.
            MeasureSetReplace(parameters[from]);

            // 5w over every file handed, which is all ten of his groups.
            for (int f = from; f < parameters.Length; f++)
            {
                SayWhatTheSetsAsk(parameters[f]);
            }

            // 5x on the LAST file handed, which is chosen as one with several models and
            // real clash results.
            MeasureModelRemove(parameters[parameters.Length - 1]);
        }

        // ---------- 5v ----------

        /// <summary>
        /// Replaces one set in place and reads back everything that pointed at it, after
        /// a save and a reopen off the disk. `ReplaceWithCopy(GroupItem, int, SavedItem)`
        /// is the route the scan named at 4d and is tried first.
        ///
        /// A STATUS IS SET BEFORE THE REPLACE so there is something to lose. A measurement
        /// that replaces a set nothing points at proves nothing at all.
        /// </summary>
        private void MeasureSetReplace(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say(string.Empty);
            Say("================ 5v, CAN A SET BE REPLACED ================");
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false for " + nwf);
                return;
            }

            Say("opened " + Path.GetFileName(nwf) + ", " + document.Models.Count + " model(s)");

            // The set the most clash tests point at, so the replace is measured on one
            // that matters rather than on whichever came first.
            string setName;
            int setIndex;
            string parentName;

            // THE SET A TEST WITH RESULTS ACTUALLY POINTS AT, and not merely the first
            // set in the tree. The first attempt replaced BLD-AR-Floors and measured a
            // test pointing at BLD-EL-Lighting Fixtures, which proves the file survived
            // and says nothing about the question being asked, which is whether the test
            // POINTING AT THE REPLACED SET keeps its results and its statuses.
            if (!FindASetATestPointsAt(document, out setName, out setIndex, out parentName))
            {
                Say("UNKNOWN: no clash test with results in this file points at a set that could be found");
                return;
            }

            Say("the set measured: [" + setName + "] at index " + setIndex + " under [" + parentName + "]");
            Say("what it asks now: " + AskedBy(document, setName));
            Say("what it finds now: " + FoundBy(document, setName) + " item(s)");

            // Something to lose: one clash moved to Reviewed, and the counts before.
            string testName;
            int resultsBefore;
            int reviewedBefore;
            SetOneReviewed(document, setName, out testName, out resultsBefore, out reviewedBefore);

            Say("the test pointing at it: [" + Words(testName) + "]");
            Say("BEFORE the replace: " + resultsBefore + " result(s), " + reviewedBefore + " at Reviewed");

            bool replaced = ReplaceThatSet(document, setName, setIndex);
            Say("ReplaceWithCopy returned without throwing: " + replaced);

            string saved = Path.Combine(Path.GetDirectoryName(nwf), "probe-replace-saved.nwf");
            Say("TrySaveFile = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            Say(string.Empty);
            Say("REOPENED OFF THE DISK. The five read backs:");
            SayFiveReadBacks(document, setName, testName, setIndex, parentName, resultsBefore, reviewedBefore);
        }

        /// <summary>The five counts 5v turns on, read off a document that has been through the disk.</summary>
        private void SayFiveReadBacks(
            Document document, string setName, string testName, int setIndex, string parentName,
            int resultsBefore, int reviewedBefore)
        {
            int resultsAfter;
            int reviewedAfter;
            bool pointsAtASet;
            string pointsAt;

            ReadTestBack(document, testName, out pointsAtASet, out pointsAt, out resultsAfter, out reviewedAfter);

            Say("   1. the clash test still points at a set : " + pointsAtASet
                + (pointsAtASet ? ", at [" + pointsAt + "]" : string.Empty)
                + (string.Equals(pointsAt, setName, StringComparison.Ordinal) ? ", WHICH IS THE RIGHT ONE" : string.Empty));
            Say("   2. the clash test still holds results   : " + resultsAfter + " against " + resultsBefore + " before"
                + (resultsAfter == resultsBefore ? ", KEPT" : ", LOST " + (resultsBefore - resultsAfter)));
            Say("   3. the Reviewed status survived         : " + reviewedAfter + " against " + reviewedBefore + " before"
                + (reviewedAfter == reviewedBefore ? ", KEPT" : ", LOST " + (reviewedBefore - reviewedAfter)));
            Say("   4. what the set finds now               : " + FoundBy(document, setName) + " item(s)");
            Say("      what it asks now                     : " + AskedBy(document, setName));

            int nowAt;
            string nowUnder;
            bool stillThere = WhereIsSet(document, setName, out nowAt, out nowUnder);

            Say("   5. the set is still in the tree         : " + stillThere
                + (stillThere
                    ? ", at index " + nowAt + " under [" + nowUnder + "], which was " + setIndex + " under [" + parentName + "]"
                    : string.Empty));

            Say(string.Empty);
            Say("A REPLACE IS ONLY USABLE WHERE 1, 2, 3 AND 5 ALL HOLD. Losing 2 or 3 means the");
            Say("tick box would destroy a week of review, and then it must refuse rather than act.");
        }

        /// <summary>
        /// A set that a clash test WITH RESULTS points at, with where it sits. That is
        /// the only shape where the five read backs mean anything: replacing a set
        /// nothing points at proves the file survived and answers no question.
        /// </summary>
        private bool FindASetATestPointsAt(Document document, out string name, out int index, out string parent)
        {
            name = null;
            index = -1;
            parent = null;

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null || test.Children.Count == 0)
                {
                    continue;
                }

                string pointed = SideOneSetOf(document, test);

                if (pointed == null)
                {
                    continue;
                }

                if (WhereIsSet(document, pointed, out index, out parent))
                {
                    name = pointed;
                    Say("chosen because the test [" + Words(test.DisplayName) + "] holds "
                        + test.Children.Count + " result(s) and its first side points at it");
                    return true;
                }
            }

            return false;
        }

        /// <summary>The name of the set the first side of that test points at, or null.</summary>
        private static string SideOneSetOf(Document document, ClashTest test)
        {
            try
            {
                SelectionSourceCollection sources = test.SelectionA.Selection.SelectionSources;

                if (sources == null || sources.Count == 0)
                {
                    return null;
                }

                using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[0]))
                {
                    return pointed == null ? null : Words(pointed.DisplayName);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>The set the most clash tests point at, with where it sits.</summary>
        private static bool FindABusySet(Document document, out string name, out int index, out string parent)
        {
            name = null;
            index = -1;
            parent = null;

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                return FirstSetUnder(root, "root", ref name, ref index, ref parent);
            }
        }

        private static bool FirstSetUnder(GroupItem folder, string folderName, ref string name, ref int index, ref string parent)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                SelectionSet set = child as SelectionSet;

                if (set != null && set.HasSearch)
                {
                    name = child.DisplayName;
                    index = i;
                    parent = folderName;
                    child.Dispose();
                    return true;
                }

                GroupItem group = child as GroupItem;

                if (group != null)
                {
                    string under = child.DisplayName;

                    if (FirstSetUnder(group, under, ref name, ref index, ref parent))
                    {
                        child.Dispose();
                        return true;
                    }
                }

                child.Dispose();
            }

            return false;
        }

        /// <summary>Moves one clash of the first test that has any to Reviewed, and counts what is there.</summary>
        private void SetOneReviewed(
            Document document, string setName, out string testName, out int results, out int reviewed)
        {
            testName = null;
            results = 0;
            reviewed = 0;

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null || test.Children.Count == 0)
                {
                    continue;
                }

                // THE TEST POINTING AT THAT SET, and not merely the first with results,
                // or the read back measures a test the replace never touched. The first
                // pass replaced BLD-AR-Floors and read back a test pointing at
                // BLD-EL-Lighting Fixtures, which proves the file survived and answers
                // no question at all.
                if (!string.Equals(SideOneSetOf(document, test), setName, StringComparison.Ordinal))
                {
                    continue;
                }

                testName = test.DisplayName;
                results = test.Children.Count;

                ClashResult first = test.Children[0] as ClashResult;

                if (first != null)
                {
                    try
                    {
                        tests.TestsEditResultStatus(first, ClashResultStatus.Reviewed);
                        Say("set one clash of [" + testName + "] to Reviewed so there is something to lose");
                    }
                    catch (Exception error)
                    {
                        Say("could not set a status, " + error.GetType().Name + ": " + error.Message);
                    }
                }

                reviewed = ReviewedIn(document, testName);
                return;
            }
        }

        private static int ReviewedIn(Document document, string testName)
        {
            int reviewed = 0;
            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null || !string.Equals(test.DisplayName, testName, StringComparison.Ordinal))
                {
                    continue;
                }

                for (int r = 0; r < test.Children.Count; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result != null && result.Status == ClashResultStatus.Reviewed)
                    {
                        reviewed++;
                    }
                }

                return reviewed;
            }

            return reviewed;
        }

        /// <summary>What a named test points at and what it holds, after the reopen.</summary>
        private static void ReadTestBack(
            Document document, string testName, out bool pointsAtASet, out string pointsAt, out int results, out int reviewed)
        {
            pointsAtASet = false;
            pointsAt = string.Empty;
            results = 0;
            reviewed = 0;

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null || !string.Equals(test.DisplayName, testName, StringComparison.Ordinal))
                {
                    continue;
                }

                results = test.Children.Count;

                for (int r = 0; r < test.Children.Count; r++)
                {
                    ClashResult result = test.Children[r] as ClashResult;

                    if (result != null && result.Status == ClashResultStatus.Reviewed)
                    {
                        reviewed++;
                    }
                }

                try
                {
                    // A CLASH SIDE POINTS AT A SET THROUGH A SELECTION SOURCE, which is
                    // what CreateSelectionSource made when the side was filled in. A side
                    // holding none was filled with a copy of the items instead.
                    SelectionSourceCollection sources = test.SelectionA.Selection.SelectionSources;

                    if (sources == null || sources.Count == 0)
                    {
                        pointsAt = "no selection source at all, so it holds items and not a set";
                    }
                    else
                    {
                        using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[0]))
                        {
                            pointsAtASet = pointed != null;
                            pointsAt = pointed == null ? "a source that resolves to nothing" : Words(pointed.DisplayName);
                        }
                    }
                }
                catch (Exception error)
                {
                    pointsAt = "resolving threw " + error.GetType().Name;
                }

                return;
            }
        }

        /// <summary>
        /// Replaces that set's slot with a fresh set carrying a DIFFERENT question, so the
        /// read back can tell a replace that happened from one that did nothing.
        /// </summary>
        private bool ReplaceThatSet(Document document, string setName, int index)
        {
            try
            {
                using (Search search = new Search())
                {
                    search.Selection.SelectAll();
                    search.Locations = SearchLocations.DescendantsAndSelf;
                    search.SearchConditions.Add(SearchCondition.HasPropertyByDisplayName("Item", "Name"));

                    using (SelectionSet made = new SelectionSet(search))
                    {
                        made.DisplayName = setName;

                        // THE PARENT IS RESOLVED FRESH FROM AN INDEX PATH, because the
                        // first attempt walked the tree and handed back a folder that its
                        // own walk had already disposed, and ReplaceWithCopy refused it by
                        // name. Plain ints out, a fresh wrapper in, which is the shape the
                        // viewpoint writer already uses for exactly this reason.
                        int[] path = PathToParentOf(document, setName);

                        if (path == null)
                        {
                            document.SelectionSets.ReplaceWithCopy(index, made);
                        }
                        else
                        {
                            using (GroupItem parent = (GroupItem)document.SelectionSets.ResolveIndexPath(path))
                            {
                                document.SelectionSets.ReplaceWithCopy(parent, index, made);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception error)
            {
                Say("ReplaceWithCopy THREW " + error.GetType().Name + ": " + error.Message);
                return false;
            }
        }

        /// <summary>
        /// The index path of the FOLDER holding that set, as plain ints, or null where it
        /// sits at the root. Ints and never a handle, so the caller resolves a fresh
        /// wrapper at the moment it needs one and nothing is used after its walk released it.
        /// </summary>
        private static int[] PathToParentOf(Document document, string setName)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                SelectionSet found = SetNamed(root, setName);

                if (found == null)
                {
                    return null;
                }

                using (found)
                {
                    System.Collections.ObjectModel.Collection<int> path =
                        document.SelectionSets.CreateIndexPath(found);

                    if (path == null || path.Count < 2)
                    {
                        return null;
                    }

                    // The set own last step dropped, which leaves the folder above it.
                    int[] parent = new int[path.Count - 1];

                    for (int i = 0; i < parent.Length; i++)
                    {
                        parent[i] = path[i];
                    }

                    return parent;
                }
            }
        }

        private static GroupItem ParentOf(Document document, string setName)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                return ParentUnder(root, setName);
            }
        }

        private static GroupItem ParentUnder(GroupItem folder, string setName)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];

                if (child is SelectionSet && string.Equals(child.DisplayName, setName, StringComparison.Ordinal))
                {
                    child.Dispose();
                    return folder;
                }

                GroupItem group = child as GroupItem;

                if (group != null)
                {
                    GroupItem found = ParentUnder(group, setName);

                    if (found != null)
                    {
                        child.Dispose();
                        return found;
                    }
                }

                child.Dispose();
            }

            return null;
        }

        private static bool WhereIsSet(Document document, string setName, out int index, out string parent)
        {
            index = -1;
            parent = null;
            string name = null;
            return FindNamedSet(document, setName, ref name, ref index, ref parent);
        }

        private static bool FindNamedSet(Document document, string setName, ref string name, ref int index, ref string parent)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                return NamedSetUnder(root, "root", setName, ref index, ref parent);
            }
        }

        private static bool NamedSetUnder(GroupItem folder, string folderName, string setName, ref int index, ref string parent)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];

                if (child is SelectionSet && string.Equals(child.DisplayName, setName, StringComparison.Ordinal))
                {
                    index = i;
                    parent = folderName;
                    child.Dispose();
                    return true;
                }

                GroupItem group = child as GroupItem;

                if (group != null && NamedSetUnder(group, child.DisplayName, setName, ref index, ref parent))
                {
                    child.Dispose();
                    return true;
                }

                child.Dispose();
            }

            return false;
        }

        // ---------- 5w ----------

        /// <summary>
        /// What EVERY set in that file is asking, read off `SelectionSet.Search`, which is
        /// a getter nothing in this tool has ever read. The whole of PART 3 rests on this
        /// working, and `BLD-DRPipe Accessories` is the check that it does, because that
        /// set is known to have drifted from the corrected matrix.
        /// </summary>
        private void SayWhatTheSetsAsk(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say(string.Empty);
            Say("================ 5w, " + Path.GetFileName(nwf) + " ================");
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            List<string> lines = new List<string>();
            int sets = 0;
            int withSearch = 0;
            int unreadable = 0;

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                WalkSets(root, string.Empty, lines, ref sets, ref withSearch, ref unreadable);
            }

            Say(sets + " set(s), " + withSearch + " carry a search, " + unreadable + " could not be read");

            for (int i = 0; i < lines.Count; i++)
            {
                Say("   " + lines[i]);
            }
        }

        private void WalkSets(
            GroupItem folder, string path, IList<string> lines, ref int sets, ref int withSearch, ref int unreadable)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                SelectionSet set = child as SelectionSet;

                if (set != null)
                {
                    sets++;
                    string where = path + "/" + Words(child.DisplayName);

                    if (!set.HasSearch)
                    {
                        lines.Add(where + "   carries NO SEARCH, explicit items only");
                    }
                    else
                    {
                        string asked = ConditionsOf(set);

                        if (asked == null)
                        {
                            unreadable++;
                            lines.Add(where + "   COULD NOT BE READ");
                        }
                        else
                        {
                            withSearch++;
                            lines.Add(where + "   asks " + asked);
                        }
                    }
                }

                GroupItem group = child as GroupItem;

                if (group != null)
                {
                    WalkSets(group, path + "/" + Words(child.DisplayName), lines, ref sets, ref withSearch, ref unreadable);
                }

                child.Dispose();
            }
        }

        /// <summary>
        /// The conditions one set carries, in the shape `SetBuildPlan.Describe` writes, so
        /// what the SET asks and what the FILE asks can be put beside each other and read
        /// as one sentence. Null where the search will not read at all.
        /// </summary>
        private static string ConditionsOf(SelectionSet set)
        {
            try
            {
                Search search = set.Search;

                if (search == null)
                {
                    return null;
                }

                List<string> parts = new List<string>();

                foreach (SearchCondition condition in search.SearchConditions)
                {
                    parts.Add(OneCondition(condition));
                }

                return parts.Count == 0 ? "nothing at all" : string.Join(" and ", parts.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string OneCondition(SearchCondition condition)
        {
            string category = NameOf(condition.CategoryCombinedName);
            string property = NameOf(condition.PropertyCombinedName);
            string value = ValueOf(condition.Value);
            string how = condition.Comparison.ToString();
            string flags = condition.Options.ToString();

            return (category.Length == 0 ? string.Empty : category + "/")
                + property + " " + how + " \"" + value + "\" [" + flags + "]";
        }

        private static string NameOf(NamedConstant named)
        {
            if (named == null)
            {
                return string.Empty;
            }

            string internalName = Words(named.Name);
            string display = Words(named.DisplayName);

            return display.Length == 0 || string.Equals(display, internalName, StringComparison.Ordinal)
                ? internalName
                : internalName + " (" + display + ")";
        }

        private static string ValueOf(VariantData value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                switch (value.DataType)
                {
                    case VariantDataType.DisplayString:
                        return value.ToDisplayString();
                    case VariantDataType.IdentifierString:
                        return value.ToIdentifierString();
                    default:
                        return Words(value.ToString());
                }
            }
            catch (Exception)
            {
                return "unreadable";
            }
        }

        private static string AskedBy(Document document, string setName)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                SelectionSet found = SetNamed(root, setName);

                if (found == null)
                {
                    return "NOT FOUND";
                }

                using (found)
                {
                    string asked = ConditionsOf(found);
                    return asked ?? "COULD NOT BE READ";
                }
            }
        }

        private static int FoundBy(Document document, string setName)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                SelectionSet found = SetNamed(root, setName);

                if (found == null)
                {
                    return -1;
                }

                using (found)
                {
                    try
                    {
                        using (ModelItemCollection items = found.GetSelectedItems(document))
                        {
                            return items == null ? -1 : items.Count;
                        }
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                }
            }
        }

        private static SelectionSet SetNamed(GroupItem folder, string setName)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                SelectionSet set = child as SelectionSet;

                if (set != null && string.Equals(child.DisplayName, setName, StringComparison.Ordinal))
                {
                    return set;
                }

                GroupItem group = child as GroupItem;

                if (group != null)
                {
                    SelectionSet found = SetNamed(group, setName);

                    if (found != null)
                    {
                        child.Dispose();
                        return found;
                    }
                }

                child.Dispose();
            }

            return null;
        }

        // ---------- 5x ----------

        /// <summary>
        /// What taking ONE MODEL out of an open document costs. 5c measured that
        /// `Document.RemoveFile(int)` exists and scan.md says in as many words that what
        /// it does to what points INTO that model is UNKNOWN. That is the only question
        /// F50's rebuild turns on and it has been open since 2026-09-18.
        /// </summary>
        private void MeasureModelRemove(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say(string.Empty);
            Say("================ 5x, WHAT REMOVING ONE MODEL COSTS ================");
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            if (document.Models.Count < 2)
            {
                Say("UNKNOWN: this file holds " + document.Models.Count + " model(s), so nothing can be removed and still leave a federation");
                return;
            }

            SayCounts(document, "BEFORE the remove");

            string going;

            using (Model model = document.Models[document.Models.Count - 1])
            {
                going = Path.GetFileName(Words(model.FileName));
            }

            Say("removing the LAST model, [" + going + "], index " + (document.Models.Count - 1));

            bool removed;

            try
            {
                removed = document.TryRemoveFile(document.Models.Count - 1);
                Say("TryRemoveFile returned " + removed);
            }
            catch (Exception error)
            {
                Say("TryRemoveFile THREW " + error.GetType().Name + ": " + error.Message);
                return;
            }

            SayCounts(document, "AFTER the remove, before any save");

            string saved = Path.Combine(Path.GetDirectoryName(nwf), "probe-remove-saved.nwf");
            Say("TrySaveFile = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            SayCounts(document, "AFTER a save and a reopen off the disk");

            Say(string.Empty);
            Say("REMOVING A MODEL IS ONLY USABLE WHERE THE SETS, THE TESTS, THE RESULTS, THE");
            Say("STATUSES AND THE VIEWPOINTS ALL COME BACK. Anything lost means F50 keeps its");
            Say("clear and rebuild, which is a good answer and closes Q34 either way.");
        }

        /// <summary>The five things that can point into a model, plus the models, counted the way the census counts them.</summary>
        private void SayCounts(Document document, string when)
        {
            int sets = 0;
            int withSearch = 0;
            int unreadable = 0;
            List<string> ignored = new List<string>();

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                WalkSets(root, string.Empty, ignored, ref sets, ref withSearch, ref unreadable);
            }

            int tests = 0;
            int results = 0;
            int statuses = 0;

            try
            {
                Autodesk.Navisworks.Api.Clash.DocumentClashTests data = document.GetClash().TestsData;
                tests = data.Tests.Count;

                for (int t = 0; t < data.Tests.Count; t++)
                {
                    ClashTest test = data.Tests[t] as ClashTest;

                    if (test == null)
                    {
                        continue;
                    }

                    results += CountResultsUnder(test.Children, ref statuses);
                }
            }
            catch (Exception error)
            {
                Say("   counting the clash side threw " + error.GetType().Name);
            }

            Say("   " + when + ":");
            Say("      models " + document.Models.Count
                + "   sets " + sets
                + "   tests " + tests
                + "   results " + results
                + "   statuses a person set " + statuses
                + "   viewpoints " + CountViewpoints(document));
        }

        /// <summary>Results as LEAVES, descending result groups, which is how the census counts them.</summary>
        private static int CountResultsUnder(SavedItemCollection items, ref int statuses)
        {
            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        count += CountResultsUnder(group.Children, ref statuses);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    count++;

                    // Everything except New is somebody's decision, which is the rule
                    // StatusesAPersonSet already states.
                    if (result.Status != ClashResultStatus.New)
                    {
                        statuses++;
                    }
                }
            }

            return count;
        }

        private static int CountViewpoints(Document document)
        {
            try
            {
                using (GroupItem root = document.SavedViewpoints.RootItem)
                {
                    return ViewpointsUnder(root);
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int ViewpointsUnder(GroupItem parent)
        {
            int count = 0;
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    GroupItem group = child as GroupItem;
                    count += group != null ? ViewpointsUnder(group) : 1;
                }
            }

            return count;
        }

        // ---------- 5y, does setting a tolerance on a saved test cost its results ----------

        /// <summary>
        /// What setting a tolerance on a SAVED clash test actually costs. ClashRunner
        /// says in four places that it RESETS the results, and the window says so in
        /// capitals, and 175,434 saved tests across his runs have had one set on them.
        /// Nobody has ever measured whether the results actually go.
        ///
        /// TWO CASES, because they are not the same question:
        ///   the SAME value the test already carries, which is what every one of his runs
        ///     has done, because he picks 25 mm and the tests are already at 25 mm
        ///   a DIFFERENT value, which is what the warning is about
        /// </summary>
        private void MeasureTolerance(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say(string.Empty);
            Say("================ 5y, WHAT SETTING A TOLERANCE COSTS ================");
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;
            int at = -1;

            for (int t = 0; t < tests.Tests.Count && at < 0; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test != null && test.Children.Count > 0)
                {
                    at = t;
                }
            }

            if (at < 0)
            {
                Say("UNKNOWN: no saved test in this file holds any result");
                return;
            }

            SayTolerance(document, at, "at the start");

            // CASE ONE, the same value it already carries, which is his every run.
            double same;

            using (ClashTest test = (ClashTest)tests.Tests[at])
            {
                same = test.Tolerance;
            }

            Say(string.Empty);
            Say("CASE ONE, setting the value it ALREADY carries, " + Round(same) + ":");
            SetToleranceOn(document, at, same);
            SayTolerance(document, at, "straight after");

            string saved = Path.Combine(Path.GetDirectoryName(nwf), "probe-tolerance-same.nwf");
            Say("TrySaveFile = " + document.TrySaveFile(saved));
            document.Clear();
            document.TryOpenFile(saved);
            SayTolerance(document, at, "after a save and a reopen");

            // CASE TWO, a different value, on a fresh copy of the original.
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: the original would not reopen for case two");
                return;
            }

            Say(string.Empty);
            Say("CASE TWO, setting a DIFFERENT value, " + Round(same * 2.0) + ":");
            SayTolerance(document, at, "before");
            SetToleranceOn(document, at, same * 2.0);
            SayTolerance(document, at, "straight after");

            string other = Path.Combine(Path.GetDirectoryName(nwf), "probe-tolerance-other.nwf");
            Say("TrySaveFile = " + document.TrySaveFile(other));
            document.Clear();
            document.TryOpenFile(other);
            SayTolerance(document, at, "after a save and a reopen");

            Say(string.Empty);
            Say("THE WARNING IS ONLY HONEST IF CASE TWO LOSES SOMETHING. If case one loses");
            Say("nothing, then his 12,531 tests a run have cost him nothing and the line");
            Say("saying it reset their results has been frightening him over nothing.");
        }

        private void SetToleranceOn(Document document, int at, double value)
        {
            try
            {
                Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

                using (ClashTest copy = (ClashTest)((ClashTest)tests.Tests[at]).CreateCopy())
                {
                    copy.Tolerance = value;
                    tests.TestsReplaceWithCopy(at, copy);
                }

                Say("   set, without throwing");
            }
            catch (Exception error)
            {
                Say("   setting it THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        private void SayTolerance(Document document, int at, string when)
        {
            try
            {
                Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

                using (ClashTest test = (ClashTest)tests.Tests[at])
                {
                    int statuses = 0;
                    int results = CountResultsUnder(test.Children, ref statuses);

                    Say("   " + when + ": tolerance " + Round(test.Tolerance)
                        + ", results " + results
                        + ", statuses a person set " + statuses
                        + ", test status " + test.Status);
                }
            }
            catch (Exception error)
            {
                Say("   " + when + ": reading threw " + error.GetType().Name);
            }
        }

        /// <summary>
        /// 5z. WHAT REMOVING A SET COSTS, which 5v did NOT measure.
        ///
        /// 5v measured REPLACING a set and found it keeps everything that points at it.
        /// REMOVING IS NOT THE SAME THING. A replace leaves an object in the slot for the
        /// clash test's SelectionSource to resolve to. A remove takes the slot away, and
        /// what a live SelectionSource pointing into nothing does is unmeasured. Nothing
        /// in src has ever called Remove, RemoveAt, Clear or Move on DocumentSelectionSets.
        ///
        /// A SET IN THE MIDDLE AND NOT ONLY THE LAST. 5x removed the LAST model of four and
        /// left the middle case UNKNOWN, and that gap is why PART 5 of the drift round has
        /// to remove by name rather than by a remembered index. This does not repeat it:
        /// the set measured is chosen to have siblings AFTER it, so whether the ones behind
        /// it shift is read rather than assumed.
        ///
        /// Against a COPY under the temp folder. Never his own files.
        /// </summary>
        private void MeasureSetRemove(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say(string.Empty);
            Say("================ 5z, WHAT REMOVING A SET COSTS ================");
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false for " + nwf);
                return;
            }

            Say("opened " + Path.GetFileName(nwf) + ", " + document.Models.Count + " model(s)");
            SayRemovalMembers();

            // The set a clash test WITH RESULTS points at, and which has siblings after it,
            // so the middle case is what gets measured.
            string setName;
            int setIndex;
            string parentName;

            if (!FindAMiddleSetATestPointsAt(document, out setName, out setIndex, out parentName))
            {
                Say("UNKNOWN: no clash test with results points at a set that has siblings after it");
                return;
            }

            Say("the set measured: [" + setName + "] at index " + setIndex + " under [" + parentName + "]");
            Say("what it finds now: " + FoundBy(document, setName) + " item(s)");

            // The siblings, in order, so a shift behind the removal is read and not guessed.
            List<string> siblingsBefore = SiblingsOf(document, setName);
            Say("its folder holds " + siblingsBefore.Count + " child(ren) in this order:");

            for (int i = 0; i < siblingsBefore.Count; i++)
            {
                Say("      " + i + "  " + siblingsBefore[i] + (i == setIndex ? "   <= the one being removed" : string.Empty));
            }

            // Something to lose.
            string testName;
            int resultsBefore;
            int reviewedBefore;
            SetOneReviewed(document, setName, out testName, out resultsBefore, out reviewedBefore);

            Say("the test pointing at it: [" + Words(testName) + "]");
            Say("BEFORE the remove: " + resultsBefore + " result(s), " + reviewedBefore + " at Reviewed");

            int viewpointsBefore = CountViewpoints(document);
            SayCounts(document, "BEFORE the remove");

            bool removed = RemoveThatSet(document, setName);
            Say("the remove returned without throwing: " + removed);

            if (!removed)
            {
                Say("NOTHING WAS REMOVED, so there is nothing to read back and 5z answers nothing.");
                return;
            }

            SayCounts(document, "AFTER the remove, before any save");

            string saved = Path.Combine(Path.GetDirectoryName(nwf), "probe-remove-saved.nwf");
            Say("TrySaveFile = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            Say(string.Empty);
            Say("REOPENED OFF THE DISK. The five read backs:");
            SayCounts(document, "AFTER a save and a reopen");

            int resultsAfter;
            int reviewedAfter;
            bool pointsAtASet;
            string pointsAt;

            ReadTestBack(document, testName, out pointsAtASet, out pointsAt, out resultsAfter, out reviewedAfter);

            Say("   1. the clash test still EXISTS         : " + (resultsAfter >= 0)
                + ". It points at a set: " + pointsAtASet
                + (pointsAtASet ? ", at [" + pointsAt + "]" : ", SO ITS SIDE NOW RESOLVES TO NOTHING"));
            Say("   2. the clash test still holds results  : " + resultsAfter + " against " + resultsBefore + " before"
                + (resultsAfter == resultsBefore ? ", KEPT" : ", LOST " + (resultsBefore - resultsAfter)));
            Say("   3. the Reviewed status survived        : " + reviewedAfter + " against " + reviewedBefore + " before"
                + (reviewedAfter == reviewedBefore ? ", KEPT" : ", LOST " + (reviewedBefore - reviewedAfter)));

            int viewpointsAfter = CountViewpoints(document);
            Say("   4. the saved viewpoints survived       : " + viewpointsAfter + " against " + viewpointsBefore + " before"
                + (viewpointsAfter == viewpointsBefore ? ", KEPT" : ", LOST " + (viewpointsBefore - viewpointsAfter)));

            // 5. THE POSITIONS. The whole point of measuring a middle set.
            int goneAt;
            string goneUnder;
            bool stillThere = WhereIsSet(document, setName, out goneAt, out goneUnder);
            Say("   5. the removed set is still in the tree: " + stillThere
                + (stillThere ? " AT INDEX " + goneAt + ", SO THE REMOVE DID NOT TAKE" : ", which is what a remove should do"));

            List<string> siblingsAfter = SiblingsOfFolder(document, parentName);
            Say("      its folder now holds " + siblingsAfter.Count + " child(ren), was " + siblingsBefore.Count + ":");

            for (int i = 0; i < siblingsAfter.Count; i++)
            {
                string wasAt = "not in the before list";

                for (int b = 0; b < siblingsBefore.Count; b++)
                {
                    if (string.Equals(siblingsBefore[b], siblingsAfter[i], StringComparison.Ordinal))
                    {
                        wasAt = "was at " + b;
                        break;
                    }
                }

                Say("      " + i + "  " + siblingsAfter[i] + "   " + wasAt);
            }

            Say(string.Empty);
            Say("AND DOES ANYTHING THAT RESOLVES A SET BY INDEX STILL RESOLVE TO THE RIGHT ONE:");
            SayEverySetAndWhatPointsAtIt(document);

            Say(string.Empty);
            Say("A REMOVE IS ONLY USABLE WHERE 2, 3 AND 4 ALL HOLD. If the test that pointed at");
            Say("the removed set loses its results or its statuses, the tick box must REFUSE that");
            Say("set, name it, and say what would be lost, rather than removing and mentioning it.");
        }

        /// <summary>
        /// What the installed DLL really offers for removal, read by reflection rather than
        /// trusted. scan.md records Remove(SavedItem) and RemoveAt(Int32) verbatim, and a
        /// PARENT SCOPED RemoveAt(GroupItem, Int32) only in prose about a different
        /// collection, so it is asserted and not quoted. This prints what is actually there.
        /// </summary>
        private void SayRemovalMembers()
        {
            try
            {
                Type type = typeof(DocumentSelectionSets);
                Say("DocumentSelectionSets removal and ordering members on THIS install:");

                foreach (System.Reflection.MethodInfo method in type.GetMethods())
                {
                    string name = method.Name;

                    if (name != "Remove" && name != "RemoveAt" && name != "Move"
                        && name != "Clear" && name != "ResolveIndexPath" && name != "CreateIndexPath")
                    {
                        continue;
                    }

                    List<string> args = new List<string>();

                    foreach (System.Reflection.ParameterInfo parameter in method.GetParameters())
                    {
                        args.Add(parameter.ParameterType.Name);
                    }

                    Say("   " + method.ReturnType.Name + " " + name + "(" + string.Join(", ", args.ToArray()) + ")");
                }
            }
            catch (Exception error)
            {
                Say("   reading the members threw " + error.GetType().Name);
            }
        }

        /// <summary>
        /// A set a clash test with results points at AND which has siblings after it in its
        /// own folder, so removing it measures the middle case. Falls back to any set a test
        /// points at, saying so, because a measurement on the last one is worth more than none.
        /// </summary>
        private bool FindAMiddleSetATestPointsAt(Document document, out string name, out int index, out string parent)
        {
            name = null;
            index = -1;
            parent = null;

            Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;
            string firstAny = null;
            int firstAnyIndex = -1;
            string firstAnyParent = null;

            for (int t = 0; t < tests.Tests.Count; t++)
            {
                ClashTest test = tests.Tests[t] as ClashTest;

                if (test == null || test.Children.Count == 0)
                {
                    continue;
                }

                string pointed = SideOneSetOf(document, test);

                if (pointed == null)
                {
                    continue;
                }

                int at;
                string under;

                if (!WhereIsSet(document, pointed, out at, out under))
                {
                    continue;
                }

                if (firstAny == null)
                {
                    firstAny = pointed;
                    firstAnyIndex = at;
                    firstAnyParent = under;
                }

                if (at < SiblingsOf(document, pointed).Count - 1)
                {
                    name = pointed;
                    index = at;
                    parent = under;
                    Say("chosen because the test [" + Words(test.DisplayName) + "] holds "
                        + test.Children.Count + " result(s), its first side points at it, AND IT HAS SIBLINGS AFTER IT");
                    return true;
                }
            }

            if (firstAny == null)
            {
                return false;
            }

            name = firstAny;
            index = firstAnyIndex;
            parent = firstAnyParent;
            Say("NO SET A TEST POINTS AT HAS SIBLINGS AFTER IT, so the LAST one is measured and the middle case stays UNKNOWN");
            return true;
        }

        /// <summary>Every child of the folder that set sits in, in order.</summary>
        private static List<string> SiblingsOf(Document document, string setName)
        {
            int at;
            string under;

            if (!WhereIsSet(document, setName, out at, out under))
            {
                return new List<string>();
            }

            return SiblingsOfFolder(document, under);
        }

        /// <summary>Every child of the named folder, in order. The root is named "root".</summary>
        private static List<string> SiblingsOfFolder(Document document, string folderName)
        {
            List<string> names = new List<string>();

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                GroupItem folder = string.Equals(folderName, "root", StringComparison.Ordinal)
                    ? (GroupItem)root
                    : FindFolderNamed(root, folderName);

                if (folder == null)
                {
                    return names;
                }

                SavedItemCollection children = folder.Children;

                for (int i = 0; i < children.Count; i++)
                {
                    using (SavedItem child = children[i])
                    {
                        names.Add(child.DisplayName + (child is GroupItem ? "  (folder)" : string.Empty));
                    }
                }
            }

            return names;
        }

        private static GroupItem FindFolderNamed(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                GroupItem group = child as GroupItem;

                if (group != null)
                {
                    if (string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        return group;
                    }

                    GroupItem deeper = FindFolderNamed(group, name);

                    if (deeper != null)
                    {
                        child.Dispose();
                        return deeper;
                    }
                }

                child.Dispose();
            }

            return null;
        }

        /// <summary>
        /// Takes that set out. The set is RESOLVED FRESH at the moment of the call, because
        /// a wrapper read earlier is a borrowed handle and every mutator on these collections
        /// is a copy form that kills what it is handed, which is rule 4g.
        /// </summary>
        private bool RemoveThatSet(Document document, string setName)
        {
            try
            {
                int[] path = PathToSet(document, setName);

                if (path == null)
                {
                    Say("   the set could not be found again to remove");
                    return false;
                }

                using (SavedItem found = document.SelectionSets.ResolveIndexPath(path))
                {
                    if (found == null)
                    {
                        Say("   ResolveIndexPath gave nothing back");
                        return false;
                    }

                    // THE ONE ARGUMENT FORM IS TRIED FIRST AND IS EXPECTED TO FAIL for a
                    // nested set, because it addresses the ROOT collection. It returns
                    // FALSE rather than throwing, which is the quiet kind of failure a
                    // caller reads as "there was nothing to remove".
                    bool atRoot = document.SelectionSets.Remove(found);
                    Say("   Remove(SavedItem), the root form, returned " + atRoot);

                    if (atRoot)
                    {
                        return true;
                    }
                }

                // The PARENT SCOPED form, on a parent resolved fresh at the moment of the
                // call. Both the parent and the index come off the same walk, so they
                // cannot disagree.
                int last = path[path.Length - 1];

                if (path.Length == 1)
                {
                    using (FolderItem root = document.SelectionSets.RootItem)
                    {
                        document.SelectionSets.RemoveAt(root, last);
                    }

                    Say("   RemoveAt(root, " + last + ") returned without throwing");
                    return true;
                }

                int[] parentPath = new int[path.Length - 1];
                Array.Copy(path, parentPath, parentPath.Length);

                using (SavedItem parentItem = document.SelectionSets.ResolveIndexPath(parentPath))
                {
                    GroupItem parent = parentItem as GroupItem;

                    if (parent == null)
                    {
                        Say("   the parent index path did not resolve to a folder");
                        return false;
                    }

                    document.SelectionSets.RemoveAt(parent, last);
                    Say("   RemoveAt(parent, " + last + ") returned without throwing");
                    return true;
                }
            }
            catch (Exception error)
            {
                Say("   the remove THREW " + error.GetType().Name + ": " + error.Message);
                return false;
            }
        }

        /// <summary>The index path to that set, as plain ints, which survive across a mutator.</summary>
        private static int[] PathToSet(Document document, string setName)
        {
            using (FolderItem root = document.SelectionSets.RootItem)
            {
                List<int> path = new List<int>();

                return WalkToSet(root, setName, path) ? path.ToArray() : null;
            }
        }

        private static bool WalkToSet(GroupItem parent, string setName, List<int> path)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];

                if (child is SelectionSet && string.Equals(child.DisplayName, setName, StringComparison.Ordinal))
                {
                    path.Add(i);
                    child.Dispose();
                    return true;
                }

                GroupItem group = child as GroupItem;

                if (group != null)
                {
                    path.Add(i);

                    if (WalkToSet(group, setName, path))
                    {
                        child.Dispose();
                        return true;
                    }

                    path.RemoveAt(path.Count - 1);
                }

                child.Dispose();
            }

            return false;
        }

        /// <summary>
        /// Every clash test, what each side resolves to now, and how many resolve to
        /// NOTHING. That last number is the whole question: a side pointing into a set
        /// that is gone is a test that can never find anything again.
        /// </summary>
        private void SayEverySetAndWhatPointsAtIt(Document document)
        {
            try
            {
                Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;
                int bothResolve = 0;
                int oneDangling = 0;
                int bothDangling = 0;
                List<string> dangling = new List<string>();

                for (int t = 0; t < tests.Tests.Count; t++)
                {
                    ClashTest test = tests.Tests[t] as ClashTest;

                    if (test == null)
                    {
                        continue;
                    }

                    bool left = SideResolves(document, test.SelectionA);
                    bool right = SideResolves(document, test.SelectionB);

                    if (left && right)
                    {
                        bothResolve++;
                    }
                    else if (left || right)
                    {
                        oneDangling++;

                        if (dangling.Count < 5)
                        {
                            dangling.Add(Words(test.DisplayName));
                        }
                    }
                    else
                    {
                        bothDangling++;

                        if (dangling.Count < 5)
                        {
                            dangling.Add(Words(test.DisplayName));
                        }
                    }
                }

                Say("   tests whose two sides both resolve to a set : " + bothResolve);
                Say("   tests with ONE side resolving to nothing    : " + oneDangling);
                Say("   tests with BOTH sides resolving to nothing  : " + bothDangling);

                for (int i = 0; i < dangling.Count; i++)
                {
                    Say("      dangling: " + dangling[i]);
                }
            }
            catch (Exception error)
            {
                Say("   walking the tests threw " + error.GetType().Name);
            }
        }

        private static bool SideResolves(Document document, ClashSelection side)
        {
            try
            {
                SelectionSourceCollection sources = side.Selection.SelectionSources;

                if (sources == null || sources.Count == 0)
                {
                    return false;
                }

                using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[0]))
                {
                    return pointed != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// HOW MANY CLASH TESTS POINT AT EACH SET, read off his own NWFs and changing
        /// nothing. That number decides what PART 2 is allowed to do: a set nothing points
        /// at can be removed freely, and one that 300 tests point at cannot.
        /// Read only. Nothing is saved.
        /// </summary>
        private void CountWhatPointsAtSets(string[] parameters, int from)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            for (int f = from; f < parameters.Length; f++)
            {
                string nwf = parameters[f];
                Say(string.Empty);
                Say("================ " + Path.GetFileName(nwf) + " ================");
                document.Clear();

                if (!document.TryOpenFile(nwf))
                {
                    Say("UNKNOWN: TryOpenFile returned false for " + nwf);
                    continue;
                }

                Dictionary<string, int> byName = new Dictionary<string, int>(StringComparer.Ordinal);
                int sidesThatResolveToNothing = 0;
                int testCount = 0;

                try
                {
                    Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;
                    testCount = tests.Tests.Count;

                    for (int t = 0; t < tests.Tests.Count; t++)
                    {
                        ClashTest test = tests.Tests[t] as ClashTest;

                        if (test == null)
                        {
                            continue;
                        }

                        CountOneSide(document, test.SelectionA, byName, ref sidesThatResolveToNothing);
                        CountOneSide(document, test.SelectionB, byName, ref sidesThatResolveToNothing);
                    }
                }
                catch (Exception error)
                {
                    Say("   walking the tests threw " + error.GetType().Name + ": " + error.Message);
                    continue;
                }

                Say(testCount + " clash test(s), " + byName.Count + " distinct set(s) pointed at, "
                    + sidesThatResolveToNothing + " side(s) resolving to nothing");

                List<string> names = new List<string>(byName.Keys);
                names.Sort(StringComparer.Ordinal);

                foreach (string name in names)
                {
                    Say("   " + byName[name].ToString().PadLeft(5) + "  test side(s) point at  [" + name + "]");
                }
            }
        }

        private static void CountOneSide(
            Document document, ClashSelection side, Dictionary<string, int> byName, ref int nothing)
        {
            try
            {
                SelectionSourceCollection sources = side.Selection.SelectionSources;

                if (sources == null || sources.Count == 0)
                {
                    nothing++;
                    return;
                }

                using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[0]))
                {
                    if (pointed == null)
                    {
                        nothing++;
                        return;
                    }

                    string name = Words(pointed.DisplayName);
                    byName[name] = byName.ContainsKey(name) ? byName[name] + 1 : 1;
                }
            }
            catch (Exception)
            {
                nothing++;
            }
        }

        /// <summary>
        /// 5z-b. WHETHER RENAMING A SET KEEPS WHAT POINTS AT IT, and PART 2 is blocked on
        /// the answer.
        ///
        /// THREE DIFFERENT FIELDS, THREE DIFFERENT MEASUREMENTS. 5v changed a set's
        /// CONDITIONS through ReplaceWithCopy and found the clash test kept its side, its
        /// results and its statuses. 5z REMOVED a set and found the results and statuses
        /// survive while 60 test sides stop resolving. A DISPLAY NAME is a third field and
        /// nothing has measured it. It cannot be inferred from either: a rename might be a
        /// cheap label change that a SelectionSource never notices, or it might be a
        /// replace underneath, in which case whether the source follows is exactly the
        /// open question.
        ///
        /// A SET IN THE MIDDLE, because 5z found the ones behind a removal shift up by one
        /// and a rename that reorders the tree would do the same to anything resolving by
        /// index.
        ///
        /// Against a COPY under the temp folder. Never his own files.
        /// </summary>
        private void MeasureSetRename(string nwf)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;
            Say(string.Empty);
            Say("================ 5z-b, DOES RENAMING A SET KEEP WHAT POINTS AT IT ================");
            document.Clear();

            if (!document.TryOpenFile(nwf))
            {
                Say("UNKNOWN: TryOpenFile returned false for " + nwf);
                return;
            }

            Say("opened " + Path.GetFileName(nwf) + ", " + document.Models.Count + " model(s)");
            SayRenameMembers();

            string setName;
            int setIndex;
            string parentName;

            if (!FindAMiddleSetATestPointsAt(document, out setName, out setIndex, out parentName))
            {
                Say("UNKNOWN: no clash test with results points at a set that has siblings after it");
                return;
            }

            string renamed = setName + " RENAMED BY 5z-b";
            Say("the set measured: [" + setName + "] at index " + setIndex + " under [" + parentName + "]");
            Say("the new name    : [" + renamed + "]");
            Say("what it finds now: " + FoundBy(document, setName) + " item(s)");

            List<string> siblingsBefore = SiblingsOf(document, setName);
            Say("its folder holds " + siblingsBefore.Count + " child(ren) in this order:");

            for (int i = 0; i < siblingsBefore.Count; i++)
            {
                Say("      " + i + "  " + siblingsBefore[i] + (i == setIndex ? "   <= the one being renamed" : string.Empty));
            }

            // HOW MANY SIDES POINT AT IT, counted before, because that is the number the
            // whole question is about.
            int sidesBefore = SidesPointingAt(document, setName);
            Say("test sides pointing at it BEFORE the rename: " + sidesBefore);

            string testName;
            int resultsBefore;
            int reviewedBefore;
            SetOneReviewed(document, setName, out testName, out resultsBefore, out reviewedBefore);

            Say("the test pointing at it: [" + Words(testName) + "]");
            Say("BEFORE the rename: " + resultsBefore + " result(s), " + reviewedBefore + " at Reviewed");

            int viewpointsBefore = CountViewpoints(document);
            SayCounts(document, "BEFORE the rename");

            bool wasRenamed = RenameThatSet(document, setName, renamed);
            Say("the rename returned without throwing: " + wasRenamed);

            if (!wasRenamed)
            {
                Say("NOTHING WAS RENAMED, so there is nothing to read back and 5z-b answers nothing.");
                return;
            }

            SayCounts(document, "AFTER the rename, before any save");

            string saved = Path.Combine(Path.GetDirectoryName(nwf), "probe-rename-saved.nwf");
            Say("TrySaveFile = " + document.TrySaveFile(saved));
            document.Clear();

            if (!document.TryOpenFile(saved))
            {
                Say("UNKNOWN: the saved copy would not reopen");
                return;
            }

            Say(string.Empty);
            Say("REOPENED OFF THE DISK. The five read backs:");
            SayCounts(document, "AFTER a save and a reopen");

            int sidesAtNew = SidesPointingAt(document, renamed);
            int sidesAtOld = SidesPointingAt(document, setName);
            int resultsAfter;
            int reviewedAfter;
            bool pointsAtASet;
            string pointsAt;

            ReadTestBack(document, testName, out pointsAtASet, out pointsAt, out resultsAfter, out reviewedAfter);

            Say("   1. the test side still resolves        : " + pointsAtASet
                + (pointsAtASet ? ", to [" + pointsAt + "]" : ", SO IT NOW RESOLVES TO NOTHING"));
            Say("      and it is the RIGHT set             : "
                + string.Equals(pointsAt, renamed, StringComparison.Ordinal));
            Say("      sides pointing at the NEW name      : " + sidesAtNew + " against " + sidesBefore + " before"
                + (sidesAtNew == sidesBefore ? ", ALL FOLLOWED THE RENAME" : ", " + (sidesBefore - sidesAtNew) + " DID NOT"));
            Say("      sides pointing at the OLD name      : " + sidesAtOld + ", which should be 0");
            Say("   2. the clash test still holds results  : " + resultsAfter + " against " + resultsBefore + " before"
                + (resultsAfter == resultsBefore ? ", KEPT" : ", LOST " + (resultsBefore - resultsAfter)));
            Say("   3. the Reviewed status survived        : " + reviewedAfter + " against " + reviewedBefore + " before"
                + (reviewedAfter == reviewedBefore ? ", KEPT" : ", LOST " + (reviewedBefore - reviewedAfter)));

            int viewpointsAfter = CountViewpoints(document);
            Say("   4. the saved viewpoints survived       : " + viewpointsAfter + " against " + viewpointsBefore + " before"
                + (viewpointsAfter == viewpointsBefore ? ", KEPT" : ", LOST " + (viewpointsBefore - viewpointsAfter)));

            Say("   5. what the set finds under its new name: " + FoundBy(document, renamed) + " item(s)");
            Say("      what it asks now                    : " + AskedBy(document, renamed));

            int nowAt;
            string nowUnder;
            bool stillThere = WhereIsSet(document, renamed, out nowAt, out nowUnder);
            Say("      it is in the tree under the new name: " + stillThere
                + (stillThere ? " at index " + nowAt + " under [" + nowUnder + "], which was " + setIndex + " under [" + parentName + "]" : string.Empty));

            List<string> siblingsAfter = SiblingsOfFolder(document, parentName);
            Say("      its folder now holds " + siblingsAfter.Count + " child(ren), was " + siblingsBefore.Count + ":");

            for (int i = 0; i < siblingsAfter.Count; i++)
            {
                Say("      " + i + "  " + siblingsAfter[i]);
            }

            Say(string.Empty);
            SayEverySetAndWhatPointsAtIt(document);

            Say(string.Empty);
            Say("A RENAME IS ONLY USABLE WHERE 1, 2, 3 AND 4 ALL HOLD AND EVERY SIDE FOLLOWED IT.");
            Say("If one side stops resolving, PART 2 builds nothing for this case and the box");
            Say("refuses, names what points at the set, and says a rename would lose them.");
        }

        /// <summary>What the installed DLL offers for renaming, read rather than trusted.</summary>
        private void SayRenameMembers()
        {
            try
            {
                Type type = typeof(DocumentSelectionSets);
                Say("DocumentSelectionSets rename members on THIS install:");

                foreach (System.Reflection.MethodInfo method in type.GetMethods())
                {
                    if (method.Name != "EditDisplayName")
                    {
                        continue;
                    }

                    List<string> args = new List<string>();

                    foreach (System.Reflection.ParameterInfo parameter in method.GetParameters())
                    {
                        args.Add(parameter.ParameterType.Name);
                    }

                    Say("   " + method.ReturnType.Name + " " + method.Name + "(" + string.Join(", ", args.ToArray()) + ")");
                }

                Say("   SavedItem.DisplayName has a setter: "
                    + (typeof(SavedItem).GetProperty("DisplayName") != null
                        && typeof(SavedItem).GetProperty("DisplayName").CanWrite));
            }
            catch (Exception error)
            {
                Say("   reading the members threw " + error.GetType().Name);
            }
        }

        /// <summary>
        /// Renames that set through EditDisplayName, on an item RESOLVED FRESH at the
        /// moment of the call, because every mutator on this collection is a copy form
        /// that kills the handle it is handed, which is rule 4g.
        /// </summary>
        private bool RenameThatSet(Document document, string setName, string newName)
        {
            try
            {
                int[] path = PathToSet(document, setName);

                if (path == null)
                {
                    Say("   the set could not be found again to rename");
                    return false;
                }

                using (SavedItem found = document.SelectionSets.ResolveIndexPath(path))
                {
                    if (found == null)
                    {
                        Say("   ResolveIndexPath gave nothing back");
                        return false;
                    }

                    document.SelectionSets.EditDisplayName(found, newName);
                    Say("   EditDisplayName returned without throwing");
                }

                // Read it back off the tree rather than off the handle, which the mutator
                // may have killed.
                int at;
                string under;
                return WhereIsSet(document, newName, out at, out under);
            }
            catch (Exception error)
            {
                Say("   the rename THREW " + error.GetType().Name + ": " + error.Message);
                return false;
            }
        }

        /// <summary>How many clash test SIDES resolve to the set of that name.</summary>
        private static int SidesPointingAt(Document document, string setName)
        {
            int count = 0;

            try
            {
                Autodesk.Navisworks.Api.Clash.DocumentClashTests tests = document.GetClash().TestsData;

                for (int t = 0; t < tests.Tests.Count; t++)
                {
                    ClashTest test = tests.Tests[t] as ClashTest;

                    if (test == null)
                    {
                        continue;
                    }

                    if (SideNames(document, test.SelectionA, setName))
                    {
                        count++;
                    }

                    if (SideNames(document, test.SelectionB, setName))
                    {
                        count++;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return count;
        }

        private static bool SideNames(Document document, ClashSelection side, string setName)
        {
            try
            {
                SelectionSourceCollection sources = side.Selection.SelectionSources;

                if (sources == null || sources.Count == 0)
                {
                    return false;
                }

                using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[0]))
                {
                    return pointed != null
                        && string.Equals(Words(pointed.DisplayName), setName, StringComparison.Ordinal);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ---------- P1 of Q114, a test with its sides swapped, run beside the original ----------

        /// <summary>
        /// P1 of Q114, scan.md 5z-k. Finds the test named testName in a copy, adds a copy of it
        /// whose side A is the original's side B and whose side B is the original's side A, clears
        /// the copy's results, runs the copy and the original, and compares the clashes each finds
        /// as UNORDERED pairs of item index paths. The copy is saved to saveAs for P2.
        /// </summary>
        private void MeasureMirrorSwap(string nwf, string testName, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(testName))
            {
                Say("UNKNOWN: no test name was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count + ", document units " + document.Units);

            for (int m = 0; m < document.Models.Count; m++)
            {
                Model model = document.Models[m];
                Say("   model " + m + "  " + Path.GetFileName(model.FileName)
                    + "  under the loop folder " + (model.FileName ?? string.Empty).StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            Say("tests at the root " + clashTests.Tests.Count);

            int matches;
            List<int> address = FindTest(clashTests.Tests, testName, new List<int>(), out matches);
            Say("tests named \"" + testName + "\": " + matches);

            string[] halves = testName.Split(new[] { "-vs-" }, StringSplitOptions.None);

            if (halves.Length == 2)
            {
                int mirrors;
                FindTest(clashTests.Tests, halves[1] + "-vs-" + halves[0], new List<int>(), out mirrors);
                Say("tests named \"" + halves[1] + "-vs-" + halves[0] + "\", its mirror by name: " + mirrors);
            }

            if (address == null || matches != 1)
            {
                Say("UNKNOWN: the test is not there exactly once, so nothing is created");
                return;
            }

            Say("its address " + string.Join(".", Strings(address.ToArray())));

            PairsFound stored;

            using (ClashTest original = ResolveTest(clashTests, address))
            {
                SayTest(document, original, "THE ORIGINAL as the NWF holds it");
                stored = ReadPairs(document, original.Children);
            }

            SayPairs(stored, "the original's results as the NWF holds them");

            string swapName = testName + " P1 swap";
            int before = clashTests.Tests.Count;
            clock = System.Diagnostics.Stopwatch.StartNew();

            // Run 1 made the swap with the original's CreateCopy, and TestsAddCopy threw
            // "Contains an item whose GUID is already present in the group", because the copy
            // keeps the original's Guid. So the swap is a new ClashTest, built the way
            // ClashRunner.Create builds one, carrying every setting SayTest prints, and the
            // ignore rules are checked to be none, since a new test carries none.
            using (ClashTest original = ResolveTest(clashTests, address))
            {
                if (original.IgnoreRules.Count != 0)
                {
                    Say("UNKNOWN: the original carries " + original.IgnoreRules.Count + " ignore rules, which a new test does not, so nothing is created");
                    return;
                }

                using (ClashTest swap = new ClashTest())
                {
                    swap.DisplayName = swapName;
                    swap.TestType = original.TestType;
                    swap.Tolerance = original.Tolerance;
                    swap.MergeComposites = original.MergeComposites;
                    swap.SimulationType = original.SimulationType;

                    using (ClashSelection originalA = original.SelectionA)
                    using (ClashSelection originalB = original.SelectionB)
                    using (ClashSelection swapA = swap.SelectionA)
                    using (ClashSelection swapB = swap.SelectionB)
                    {
                        swapA.CopyFrom(originalB);
                        swapB.CopyFrom(originalA);
                    }

                    clashTests.TestsAddCopy(swap);
                }
            }

            int after = clashTests.Tests.Count;
            Say("new ClashTest, its settings, the two CopyFrom and TestsAddCopy took " + Seconds(clock) + ", tests at the root " + before + " then " + after);

            if (after != before + 1)
            {
                Say("UNKNOWN: the root did not grow by one, so the swap cannot be found by its place");
                return;
            }

            List<int> swapAddress = new List<int> { before };

            using (ClashTest swap = ResolveTest(clashTests, swapAddress))
            {
                if (swap == null || swap.DisplayName != swapName)
                {
                    Say("UNKNOWN: the last test at the root is not the swap, it reads " + (swap == null ? "null" : "\"" + swap.DisplayName + "\""));
                    return;
                }

                SayTest(document, swap, "THE SWAP as added");
                Say("   results it carried in from the copy: " + ReadPairs(document, swap.Children).Leaves);
            }

            using (ClashTest swap = ResolveTest(clashTests, swapAddress))
            {
                clashTests.TestsClearResults(swap);
            }

            using (ClashTest swap = ResolveTest(clashTests, swapAddress))
            {
                Say("   results after TestsClearResults: " + ReadPairs(document, swap.Children).Leaves);
            }

            using (ClashTest swap = ResolveTest(clashTests, swapAddress))
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                clashTests.TestsRunTest(swap);
                Say("TestsRunTest on the swap took " + Seconds(clock));
            }

            PairsFound swapped;

            using (ClashTest swap = ResolveTest(clashTests, swapAddress))
            {
                Say("   the swap after its run: status " + swap.Status + ", last run " + (swap.LastRun.HasValue ? "set" : "never"));
                swapped = ReadPairs(document, swap.Children);
            }

            SayPairs(swapped, "the swap's results after its run");

            using (ClashTest original = ResolveTest(clashTests, address))
            {
                if (original == null || original.DisplayName != testName)
                {
                    Say("UNKNOWN: the original is no longer at its address");
                    return;
                }

                clock = System.Diagnostics.Stopwatch.StartNew();
                clashTests.TestsRunTest(original);
                Say("TestsRunTest on the original took " + Seconds(clock));
            }

            PairsFound rerun;

            using (ClashTest original = ResolveTest(clashTests, address))
            {
                rerun = ReadPairs(document, original.Children);
            }

            SayPairs(rerun, "the original's results after it ran again beside the swap");

            bool swapVsStored = ComparePairs("the swap", swapped, "the original as stored", stored);
            bool swapVsRerun = ComparePairs("the swap", swapped, "the original run again", rerun);
            bool rerunVsStored = ComparePairs("the original run again", rerun, "the original as stored", stored);

            int open = stored.Open.Count;
            bool readable = stored.NullItems == 0 && swapped.NullItems == 0 && rerun.NullItems == 0;

            if (!readable)
            {
                Say("P1 UNKNOWN   a result had an item that did not read, so a pair is not whole");
            }
            else if (swapVsStored && swapVsRerun)
            {
                Say("P1 YES   the swap finds " + swapped.Open.Count + " clashes, the original " + open
                    + " as stored and " + rerun.Open.Count + " run again, over the same unordered pairs of item index paths");
            }
            else
            {
                Say("P1 NO   the swap finds " + swapped.Open.Count + " clashes, the original " + open
                    + " as stored and " + rerun.Open.Count + " run again, and the unordered pairs differ, see the lists above");
            }

            Say("the original run again finds what it held: " + rerunVsStored);

            if (!string.IsNullOrEmpty(saveAs))
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                document.SaveFile(saveAs);
                Say("SaveFile of the copy with the swap into " + Path.GetFileName(saveAs) + " took " + Seconds(clock)
                    + ", " + Bytes(saveAs) + " bytes read back off the disk");
            }
        }

        // ---------- P2 of Q114, scan.md 5z-l, does TestsRemoveAt take one test and nothing else ----------

        /// <summary>
        /// P2 of Q114. Opens a copy of P1's NWF, which holds the swap P1 added, removes that one
        /// test by DocumentClashTests.TestsRemoveAt(GroupItem parent, int index) with the parent
        /// read fresh just before the call and the index checked by name, times the call, and
        /// compares every model, set, test, result, status and viewpoint before the call, after
        /// it, and after a save, a Document.Clear and a reopen of the saved file.
        /// </summary>
        private void MeasureTestRemove(string nwf, string testName, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(testName) || string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no test name or no save path was handed in");
                return;
            }

            Say("TestsRemoveAt and TestsRemove on this install, read by reflection:");

            foreach (System.Reflection.MethodInfo method in typeof(DocumentClashTests).GetMethods())
            {
                if (method.Name == "TestsRemoveAt" || method.Name == "TestsRemove")
                {
                    List<string> args = new List<string>();

                    foreach (System.Reflection.ParameterInfo parameter in method.GetParameters())
                    {
                        args.Add(parameter.ParameterType.Name + " " + parameter.Name);
                    }

                    Say("   " + method.ReturnType.Name + " " + method.Name + "(" + string.Join(", ", args.ToArray()) + ")");
                }
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            int matches;
            List<int> address = FindTest(clashTests.Tests, testName, new List<int>(), out matches);
            Say("tests named \"" + testName + "\": " + matches);

            if (address == null || matches != 1)
            {
                Say("UNKNOWN: the test is not there exactly once, so nothing is removed");
                return;
            }

            Say("its address " + string.Join(".", Strings(address.ToArray())));
            string removedLine;
            int removedResults;

            using (ClashTest target = ResolveTest(clashTests, address))
            {
                SayTest(document, target, "THE TEST TO BE REMOVED");
                removedLine = TestLine(target, out removedResults);
                Say("   its census line: " + removedLine);
            }

            Snapshot before = TakeSnapshot(document);
            SaySnapshot(before, "BEFORE the remove");

            int index = address[address.Count - 1];
            List<int> parentAddress = address.GetRange(0, address.Count - 1);
            double removeSeconds;

            // The parent read fresh, and the child at the index checked by name, just before the call.
            clock = System.Diagnostics.Stopwatch.StartNew();
            GroupItem parent = ParentFolderOf(clashTests, parentAddress);

            if (parent == null)
            {
                Say("UNKNOWN: the parent folder did not resolve, so nothing is removed");
                return;
            }

            using (parent)
            {
                string atIndex;

                using (SavedItem child = parent.Children[index])
                {
                    atIndex = child == null ? null : child.DisplayName;
                }

                double resolveSeconds = clock.Elapsed.TotalSeconds;
                Say("the parent resolved fresh: \"" + Words(parent.DisplayName) + "\", a " + parent.GetType().Name
                    + ", " + parent.Children.Count + " children, the child at " + index + " reads \"" + Words(atIndex) + "\""
                    + ", resolve and check took " + resolveSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s");

                if (!string.Equals(atIndex, testName, StringComparison.Ordinal))
                {
                    Say("UNKNOWN: the child at the index is not the test, so nothing is removed");
                    return;
                }

                clock = System.Diagnostics.Stopwatch.StartNew();
                clashTests.TestsRemoveAt(parent, index);
                removeSeconds = clock.Elapsed.TotalSeconds;
            }

            Say("TestsRemoveAt(parent, " + index + ") RETURNED after "
                + removeSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s");

            int left;
            FindTest(clashTests.Tests, testName, new List<int>(), out left);
            Say("tests named \"" + testName + "\" after the call: " + left);

            Snapshot afterRemove = TakeSnapshot(document);
            SaySnapshot(afterRemove, "AFTER the remove, before any save");
            bool sameAfterRemove = CompareSnapshots(before, afterRemove, removedLine, removedResults, "after the remove");

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk");

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count
                + ", tests now " + document.GetClash().TestsData.Tests.Count);

            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (!reopened)
            {
                Say("P2 UNKNOWN   the saved file would not reopen, so nothing after a reopen is read");
                return;
            }

            int afterReopen;
            FindTest(document.GetClash().TestsData.Tests, testName, new List<int>(), out afterReopen);
            Say("tests named \"" + testName + "\" after the reopen: " + afterReopen);

            Snapshot reopenedSnap = TakeSnapshot(document);
            SaySnapshot(reopenedSnap, "AFTER a save, a clear and a reopen");
            bool sameAfterReopen = CompareSnapshots(before, reopenedSnap, removedLine, removedResults, "after the reopen");
            bool removeVsReopen = CompareSnapshots(afterRemove, reopenedSnap, null, 0, "the reopen against the state after the remove");

            if (left == 0 && afterReopen == 0 && sameAfterRemove && sameAfterReopen && removeVsReopen)
            {
                Say("P2 YES   TestsRemoveAt(parent, index) took the one test with its " + removedResults
                    + " results in " + removeSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + " s, and every model, set, other test, result, status and viewpoint read the same after the call and after a save, a clear and a reopen");
            }
            else
            {
                Say("P2 NO   see the differences above: the test left " + left + " after the call and " + afterReopen
                    + " after the reopen, the rest the same after the call " + sameAfterRemove + ", after the reopen " + sameAfterReopen
                    + ", the reopen against the call " + removeVsReopen);
            }
        }

        private sealed class Snapshot
        {
            public readonly List<string> Models = new List<string>();
            public readonly List<string> Sets = new List<string>();
            public readonly List<string> Tests = new List<string>();
            public readonly List<string> Viewpoints = new List<string>();
            public int Results;
            public int NotNew;
            public int ViewpointLeaves;
            public int SetLeaves;
            public int TestLeaves;
        }

        private static GroupItem ParentFolderOf(DocumentClashTests clashTests, List<int> parentAddress)
        {
            GroupItem folder = clashTests.Value.TestsRoot;

            for (int level = 0; level < parentAddress.Count; level++)
            {
                if (folder == null || parentAddress[level] >= folder.Children.Count)
                {
                    return null;
                }

                folder = folder.Children[parentAddress[level]] as GroupItem;
            }

            return folder;
        }

        private Snapshot TakeSnapshot(Document document)
        {
            Snapshot snap = new Snapshot();

            for (int m = 0; m < document.Models.Count; m++)
            {
                Model model = document.Models[m];
                snap.Models.Add(m + "  " + Path.GetFileName(model.FileName ?? string.Empty));
            }

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                SnapTree(root, string.Empty, snap.Sets, ref snap.SetLeaves);
            }

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                SnapTree(root, string.Empty, snap.Viewpoints, ref snap.ViewpointLeaves);
            }

            SnapTests(document.GetClash().TestsData.Tests, string.Empty, snap);
            return snap;
        }

        /// <summary>Every item of a tree as its path, its name and whether it is a folder, in order.</summary>
        private static void SnapTree(GroupItem folder, string path, List<string> into, ref int leaves)
        {
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    string where = path + "/" + Words(child.DisplayName);
                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        into.Add(where + "   folder of " + group.Children.Count);
                        SnapTree(group, where, into, ref leaves);
                    }
                    else
                    {
                        leaves++;
                        into.Add(where + "   " + child.GetType().Name);
                    }
                }
            }
        }

        private void SnapTests(SavedItemCollection items, string path, Snapshot snap)
        {
            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    ClashTest test = item as ClashTest;

                    if (test != null)
                    {
                        int results;
                        string line = TestLine(test, out results);
                        int notNew = 0;
                        CountResultsUnder(test.Children, ref notNew);
                        snap.Tests.Add(path + "/" + line);
                        snap.TestLeaves++;
                        snap.Results += results;
                        snap.NotNew += notNew;
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        snap.Tests.Add(path + "/" + Words(folder.DisplayName) + "   folder of " + folder.Children.Count);
                        SnapTests(folder.Children, path + "/" + Words(folder.DisplayName), snap);
                    }
                }
            }
        }

        /// <summary>A test's name, its settings, its result count, its statuses and a hash of every result's name and status in order.</summary>
        private static string TestLine(ClashTest test, out int results)
        {
            List<string> each = new List<string>();
            Dictionary<string, int> byStatus = new Dictionary<string, int>(StringComparer.Ordinal);
            ResultsOf(test.Children, string.Empty, each, byStatus);
            results = each.Count;

            List<string> statuses = new List<string>();

            foreach (KeyValuePair<string, int> pair in byStatus)
            {
                statuses.Add(pair.Key + " " + pair.Value);
            }

            statuses.Sort(StringComparer.Ordinal);
            string hash;

            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", each.ToArray())));
                hash = BitConverter.ToString(bytes, 0, 6).Replace("-", string.Empty);
            }

            return Words(test.DisplayName) + "   type " + test.TestType
                + ", tolerance " + test.Tolerance.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                + ", status " + test.Status + ", results " + results
                + ", " + (statuses.Count == 0 ? "none" : string.Join(", ", statuses.ToArray()))
                + ", names and statuses sha256 " + hash;
        }

        private static void ResultsOf(SavedItemCollection children, string path, List<string> each, Dictionary<string, int> byStatus)
        {
            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem item = children[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        ResultsOf(group.Children, path + "/" + Words(group.DisplayName), each, byStatus);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    string status = result.Status.ToString();
                    int had;
                    byStatus.TryGetValue(status, out had);
                    byStatus[status] = had + 1;
                    each.Add(path + "/" + Words(result.DisplayName) + " " + status);
                }
            }
        }

        private void SaySnapshot(Snapshot snap, string when)
        {
            Say(when + ": models " + snap.Models.Count
                + ", set tree items " + snap.Sets.Count + " of which sets " + snap.SetLeaves
                + ", test tree items " + snap.Tests.Count + " of which tests " + snap.TestLeaves
                + ", results " + snap.Results + ", results not New " + snap.NotNew
                + ", viewpoint tree items " + snap.Viewpoints.Count + " of which viewpoints " + snap.ViewpointLeaves);
        }

        /// <summary>
        /// True when the later snapshot is the earlier one with exactly the removed test's line
        /// gone, or the same when no line was removed. Every other difference is said.
        /// </summary>
        private bool CompareSnapshots(Snapshot earlier, Snapshot later, string removedLine, int removedResults, string label)
        {
            bool same = true;
            same &= CompareList(earlier.Models, later.Models, null, "models", label);
            same &= CompareList(earlier.Sets, later.Sets, null, "sets", label);
            same &= CompareList(earlier.Viewpoints, later.Viewpoints, null, "viewpoints", label);
            same &= CompareList(earlier.Tests, later.Tests, removedLine == null ? null : "/" + removedLine, "tests", label);

            int wantResults = earlier.Results - removedResults;
            Say("   " + label + ": results " + later.Results + " against " + wantResults + " wanted"
                + (later.Results == wantResults ? ", the same" : ", DIFFERENT"));
            same &= later.Results == wantResults;
            return same;
        }

        private bool CompareList(List<string> earlier, List<string> later, string removed, string what, string label)
        {
            List<string> want = new List<string>(earlier);
            bool removedFound = true;

            if (removed != null)
            {
                removedFound = want.Remove(removed);
            }

            int differ = 0;
            int shown = 0;

            for (int i = 0; i < Math.Max(want.Count, later.Count); i++)
            {
                string a = i < want.Count ? want[i] : "(none)";
                string b = i < later.Count ? later[i] : "(none)";

                if (!string.Equals(a, b, StringComparison.Ordinal))
                {
                    differ++;

                    if (shown < 20)
                    {
                        shown++;
                        Say("      " + what + " " + i + " wanted: " + a);
                        Say("      " + what + " " + i + " read  : " + b);
                    }
                }
            }

            Say("   " + label + ": " + what + " " + later.Count + " against " + want.Count + " wanted"
                + (removed == null ? string.Empty : ", the removed test's line found in the earlier list " + removedFound)
                + ", lines that differ " + differ);
            return differ == 0 && removedFound;
        }

        private sealed class PairsFound
        {
            public int Leaves;
            public int Groups;
            public int NullItems;
            public int Duplicates;
            public readonly Dictionary<string, int> ByStatus = new Dictionary<string, int>(StringComparer.Ordinal);

            // Every result whose status is not Resolved, by its unordered pair, holding its
            // ordered pair and its distance.
            public readonly Dictionary<string, string> Open = new Dictionary<string, string>(StringComparer.Ordinal);
            public readonly Dictionary<string, double> Distance = new Dictionary<string, double>(StringComparer.Ordinal);
        }

        private static string Seconds(System.Diagnostics.Stopwatch clock)
        {
            return clock.Elapsed.TotalSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s";
        }

        /// <summary>Tests at the root and in folders, a test being a leaf even though it holds results.</summary>
        private static List<int> FindTest(SavedItemCollection items, string name, List<int> at, out int matches)
        {
            matches = 0;
            List<int> found = null;

            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    List<int> here = new List<int>(at) { i };

                    if (item is ClashTest)
                    {
                        if (string.Equals(item.DisplayName, name, StringComparison.Ordinal))
                        {
                            matches++;
                            found = here;
                        }

                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        int below;
                        List<int> under = FindTest(folder.Children, name, here, out below);
                        matches += below;

                        if (under != null)
                        {
                            found = under;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>A fresh handle by address, each level read from the collection again.</summary>
        private static ClashTest ResolveTest(DocumentClashTests clashTests, List<int> address)
        {
            SavedItemCollection children = clashTests.Tests;

            for (int level = 0; level < address.Count; level++)
            {
                int index = address[level];

                if (children == null || index < 0 || index >= children.Count)
                {
                    return null;
                }

                SavedItem item = children[index];

                if (level + 1 == address.Count)
                {
                    return item as ClashTest;
                }

                GroupItem folder = item as GroupItem;
                children = folder == null ? null : folder.Children;
            }

            return null;
        }

        private void SayTest(Document document, ClashTest test, string label)
        {
            string rules;

            try
            {
                rules = test.IgnoreRules.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception error)
            {
                rules = "UNKNOWN, " + error.GetType().Name;
            }

            Say(label + ": \"" + test.DisplayName + "\"");
            Say("   type " + test.TestType + ", tolerance " + test.Tolerance.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
                + ", merge composites " + test.MergeComposites + ", status " + test.Status
                + ", last run " + (test.LastRun.HasValue ? "set" : "never") + ", ignore rules " + rules
                + ", simulation " + test.SimulationType);

            using (ClashSelection a = test.SelectionA)
            {
                SaySide(document, a, "   side A");
            }

            using (ClashSelection b = test.SelectionB)
            {
                SaySide(document, b, "   side B");
            }
        }

        private void SaySide(Document document, ClashSelection side, string label)
        {
            List<string> names = new List<string>();
            int items = -1;
            bool explicitItems;

            using (Selection selection = side.Selection)
            {
                explicitItems = selection.HasExplicitSelection;
                SelectionSourceCollection sources = selection.SelectionSources;

                for (int i = 0; i < sources.Count; i++)
                {
                    try
                    {
                        using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[i]))
                        {
                            names.Add(pointed == null ? "a source that resolves to nothing" : "\"" + pointed.DisplayName + "\"");
                        }
                    }
                    catch (Exception error)
                    {
                        names.Add("a source that threw " + error.GetType().Name);
                    }
                }

                try
                {
                    using (ModelItemCollection got = selection.GetSelectedItems(document))
                    {
                        items = got.Count;
                    }
                }
                catch (Exception error)
                {
                    Say(label + " GetSelectedItems threw " + error.GetType().Name + ": " + error.Message);
                }
            }

            Say(label + ": sets " + (names.Count == 0 ? "none" : string.Join(", ", names.ToArray()))
                + ", explicit items " + explicitItems + ", items selected " + items
                + ", self intersect " + side.SelfIntersect + ", primitive types " + side.PrimitiveTypes);
        }

        private static PairsFound ReadPairs(Document document, SavedItemCollection children)
        {
            PairsFound found = new PairsFound();
            ReadPairsUnder(document, children, found);
            return found;
        }

        private static void ReadPairsUnder(Document document, SavedItemCollection children, PairsFound found)
        {
            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem item = children[i])
                {
                    ClashResultGroup group = item as ClashResultGroup;

                    if (group != null)
                    {
                        found.Groups++;
                        ReadPairsUnder(document, group.Children, found);
                        continue;
                    }

                    ClashResult result = item as ClashResult;

                    if (result == null)
                    {
                        continue;
                    }

                    found.Leaves++;
                    string status = result.Status.ToString();
                    int had;
                    found.ByStatus.TryGetValue(status, out had);
                    found.ByStatus[status] = had + 1;

                    string first = ItemPath(document, result.Item1);
                    string second = ItemPath(document, result.Item2);

                    if (first == null || second == null)
                    {
                        found.NullItems++;
                        continue;
                    }

                    if (result.Status == ClashResultStatus.Resolved)
                    {
                        continue;
                    }

                    string key = string.CompareOrdinal(first, second) <= 0 ? first + " | " + second : second + " | " + first;

                    if (found.Open.ContainsKey(key))
                    {
                        found.Duplicates++;
                        continue;
                    }

                    found.Open[key] = first + " | " + second;
                    found.Distance[key] = result.Distance;
                }
            }
        }

        private static string ItemPath(Document document, ModelItem item)
        {
            if (item == null)
            {
                return null;
            }

            using (item)
            {
                return string.Join(".", Strings(PathOf(document, item)));
            }
        }

        private void SayPairs(PairsFound found, string label)
        {
            List<string> statuses = new List<string>();

            foreach (KeyValuePair<string, int> pair in found.ByStatus)
            {
                statuses.Add(pair.Key + " " + pair.Value);
            }

            statuses.Sort(StringComparer.Ordinal);
            Say(label + ": results " + found.Leaves + ", groups " + found.Groups
                + ", by status " + (statuses.Count == 0 ? "none" : string.Join(", ", statuses.ToArray()))
                + ", not Resolved " + found.Open.Count + ", an item that did not read " + found.NullItems
                + ", a pair met twice " + found.Duplicates);

            List<string> keys = new List<string>(found.Open.Keys);
            keys.Sort(StringComparer.Ordinal);

            foreach (string key in keys)
            {
                Say("      " + found.Open[key] + "   distance " + found.Distance[key].ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        /// <summary>True when both hold the same unordered pairs. Says the pairs in one only, which way round each common pair is held, and the largest distance difference.</summary>
        private bool ComparePairs(string leftLabel, PairsFound left, string rightLabel, PairsFound right)
        {
            List<string> onlyLeft = new List<string>();
            List<string> onlyRight = new List<string>();
            int same = 0;
            int reversed = 0;
            int common = 0;
            double largest = 0;

            foreach (string key in left.Open.Keys)
            {
                if (!right.Open.ContainsKey(key))
                {
                    onlyLeft.Add(key);
                    continue;
                }

                common++;

                if (left.Open[key] == right.Open[key])
                {
                    same++;
                }
                else
                {
                    reversed++;
                }

                largest = Math.Max(largest, Math.Abs(left.Distance[key] - right.Distance[key]));
            }

            foreach (string key in right.Open.Keys)
            {
                if (!left.Open.ContainsKey(key))
                {
                    onlyRight.Add(key);
                }
            }

            onlyLeft.Sort(StringComparer.Ordinal);
            onlyRight.Sort(StringComparer.Ordinal);
            Say("COMPARE " + leftLabel + " " + left.Open.Count + " against " + rightLabel + " " + right.Open.Count
                + ": in both " + common + ", in " + leftLabel + " only " + onlyLeft.Count + ", in " + rightLabel + " only " + onlyRight.Count);
            Say("   of the " + common + " in both, the same item first " + same + ", the items the other way round " + reversed
                + ", the largest difference in distance " + largest.ToString("R", System.Globalization.CultureInfo.InvariantCulture));

            foreach (string key in onlyLeft)
            {
                Say("   only in " + leftLabel + ": " + key);
            }

            foreach (string key in onlyRight)
            {
                Say("   only in " + rightLabel + ": " + key);
            }

            return onlyLeft.Count == 0 && onlyRight.Count == 0;
        }

        // ---------- Q133, how often a mirror finds more, and what running both costs ----------

        private sealed class Q133Test
        {
            public List<int> Address;
            public string Name;
        }

        private sealed class Q133Diff
        {
            public int Both;
            public readonly List<string> OnlyLeft = new List<string>();
            public readonly List<string> OnlyRight = new List<string>();
        }

        private sealed class Q133Totals
        {
            public int Tests;
            public int Same;
            public int More;
            public int Fewer;
            public int Other;
            public int Unknown;
            public int Stored;
            public int Original;
            public int Swap;
            public int OnlySwap;
            public int OnlyOriginal;
            public double OriginalSeconds;
            public double SwapSeconds;
            public double CreateSeconds;
        }

        /// <summary>
        /// Q133 on one building. Part 1 reads the pairs F132's rule finds in the picked XML, written
        /// by q133-rule-pairs.py, and for each pair whose two tests are both in the NWF runs both
        /// and compares their clashes by the unordered pair of item index paths, as P1 did. Part 2
        /// does, for every test whose stored results hold at least one clash not Resolved, what P1
        /// did for one test: a new ClashTest with the sides swapped is added at the root, the
        /// original is run, then the swap, each TestsRunTest timed alone, and the two compared.
        /// Part 3 does the same for every other test of the NWF. The copy is saved to saveAs.
        /// </summary>
        private void MeasureMirrorCount(string nwf, string pairsFile, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count + ", document units " + document.Units);

            for (int m = 0; m < document.Models.Count; m++)
            {
                Model model = document.Models[m];
                Say("   model " + m + "  " + Path.GetFileName(model.FileName)
                    + "  under the loop folder " + (model.FileName ?? string.Empty).StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            DocumentClashTests clashTests = document.GetClash().TestsData;
            List<Q133Test> tests = new List<Q133Test>();
            Q133Walk(clashTests.Tests, new List<int>(), tests);
            Dictionary<string, List<Q133Test>> byName = new Dictionary<string, List<Q133Test>>(StringComparer.Ordinal);
            int inFolders = 0;

            foreach (Q133Test t in tests)
            {
                List<Q133Test> same;

                if (!byName.TryGetValue(t.Name, out same))
                {
                    same = new List<Q133Test>();
                    byName[t.Name] = same;
                }

                same.Add(t);

                if (t.Address.Count > 1)
                {
                    inFolders++;
                }
            }

            int rootCount = clashTests.Tests.Count;
            Say("tests " + tests.Count + ", at the root " + rootCount + ", in a folder " + inFolders + ", names used more than once "
                + Q133Repeated(byName, 2));

            // The stored results of every test, read before anything runs.
            Dictionary<string, PairsFound> stored = new Dictionary<string, PairsFound>(StringComparer.Ordinal);
            clock = System.Diagnostics.Stopwatch.StartNew();
            int storedTotal = 0;
            int storedTests = 0;

            foreach (Q133Test t in tests)
            {
                using (ClashTest test = ResolveTest(clashTests, t.Address))
                {
                    PairsFound found = ReadPairs(document, test.Children);
                    stored[t.Name] = found;
                    storedTotal += found.Open.Count;

                    if (found.Open.Count > 0)
                    {
                        storedTests++;
                    }
                }
            }

            Say("stored results read in " + Seconds(clock) + ": tests with at least one clash not Resolved " + storedTests
                + ", clashes not Resolved " + storedTotal);
            Say(string.Empty);

            // ---- Part 1, the rule's pairs ----
            Say("==== PART 1. The pairs F132's rule finds in the picked XML ====");
            List<string[]> pairs = new List<string[]>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            if (string.IsNullOrEmpty(pairsFile) || !File.Exists(pairsFile))
            {
                Say("UNKNOWN: no pairs file was handed in, or it is not there");
            }
            else
            {
                foreach (string line in File.ReadAllLines(pairsFile, new UTF8Encoding(false)))
                {
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        if (line.Length > 0)
                        {
                            Say("   " + line);
                        }

                        continue;
                    }

                    string[] f = line.Split('\t');

                    if (f.Length < 4)
                    {
                        Say("   a line of the pairs file with " + f.Length + " fields, skipped: " + line);
                        continue;
                    }

                    if (seen.Add(f[1] + "\n" + f[2] + "\n" + f[3]))
                    {
                        pairs.Add(new[] { f[1], f[2], f[3] });
                    }
                }
            }

            Say("distinct pairs and self tests over the XMLs read: " + pairs.Count);
            int pairsBoth = 0;
            int pairsRun = 0;

            foreach (string[] p in pairs)
            {
                int first = byName.ContainsKey(p[1]) ? byName[p[1]].Count : 0;
                int second = p[2].Length == 0 ? -1 : (byName.ContainsKey(p[2]) ? byName[p[2]].Count : 0);
                string head = "P1 PAIR  " + p[0] + "  \"" + p[1] + "\" in the NWF " + first
                    + (second < 0 ? string.Empty : ", \"" + p[2] + "\" in the NWF " + second);

                if (second < 0)
                {
                    if (first != 1)
                    {
                        Say(head + "  NOT CREATED on this building, so not run");
                        continue;
                    }

                    pairsBoth++;
                    double s;
                    PairsFound self = Q133Run(document, clashTests, byName[p[1]][0], out s);
                    pairsRun++;
                    Say(head + "  ran in " + s.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s, clashes "
                        + (self == null ? "UNKNOWN" : self.Open.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    continue;
                }

                if (first != 1 || second != 1)
                {
                    Say(head + "  NOT BOTH CREATED on this building, so not run");
                    continue;
                }

                pairsBoth++;
                double s1;
                double s2;
                PairsFound one = Q133Run(document, clashTests, byName[p[1]][0], out s1);
                PairsFound two = Q133Run(document, clashTests, byName[p[2]][0], out s2);
                pairsRun++;

                if (one == null || two == null)
                {
                    Say(head + "  UNKNOWN, a test was not at its address when run");
                    continue;
                }

                Q133Diff d = Q133Compare(one, two);
                Say(head + "  first " + one.Open.Count + " in " + s1.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + " s, second " + two.Open.Count + " in " + s2.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                    + " s, in both " + d.Both + ", first only " + d.OnlyLeft.Count + ", second only " + d.OnlyRight.Count
                    + ", items that did not read " + one.NullItems + " and " + two.NullItems);

                foreach (string k in d.OnlyLeft)
                {
                    Say("      only in the first: " + k);
                }

                foreach (string k in d.OnlyRight)
                {
                    Say("      only in the second: " + k);
                }
            }

            Say("PART 1 TOTAL  pairs and self tests " + pairs.Count + ", with every test in the NWF " + pairsBoth + ", run " + pairsRun);
            Say(string.Empty);

            // ---- Part 2 and part 3, every test against its swap ----
            List<Q133Test> withClashes = new List<Q133Test>();
            List<Q133Test> without = new List<Q133Test>();

            foreach (Q133Test t in tests)
            {
                if (byName[t.Name].Count != 1)
                {
                    Say("SKIPPED, the name is used more than once: \"" + t.Name + "\"");
                    continue;
                }

                if (stored[t.Name].Open.Count > 0)
                {
                    withClashes.Add(t);
                }
                else
                {
                    without.Add(t);
                }
            }

            Say("==== PART 2. Every test whose stored results hold a clash, " + withClashes.Count + " tests, against its swap ====");
            Q133Totals two2 = Q133Swaps(document, clashTests, withClashes, stored, "P2", true);
            Q133Say("PART 2 TOTAL", two2);
            Say(string.Empty);

            Say("==== PART 3. Every other test, " + without.Count + " tests, against its swap ====");
            Q133Totals three = Q133Swaps(document, clashTests, without, stored, "P3", false);
            Q133Say("PART 3 TOTAL", three);
            Say(string.Empty);

            Q133Totals all = new Q133Totals();
            Q133Add(all, two2);
            Q133Add(all, three);
            Q133Say("PARTS 2 AND 3 TOTAL", all);
            Say("tests at the root at the end " + clashTests.Tests.Count + ", against " + rootCount + " at the start");

            if (!string.IsNullOrEmpty(saveAs))
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                document.SaveFile(saveAs);
                Say("SaveFile of the copy with the swaps into " + Path.GetFileName(saveAs) + " took " + Seconds(clock)
                    + ", " + Bytes(saveAs) + " bytes read back off the disk");
            }
        }

        private static int Q133Repeated(Dictionary<string, List<Q133Test>> byName, int atLeast)
        {
            int n = 0;

            foreach (KeyValuePair<string, List<Q133Test>> pair in byName)
            {
                if (pair.Value.Count >= atLeast)
                {
                    n++;
                }
            }

            return n;
        }

        private static void Q133Walk(SavedItemCollection items, List<int> at, List<Q133Test> into)
        {
            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    List<int> here = new List<int>(at) { i };

                    if (item is ClashTest)
                    {
                        into.Add(new Q133Test { Address = here, Name = item.DisplayName });
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        Q133Walk(folder.Children, here, into);
                    }
                }
            }
        }

        /// <summary>Runs the test at the address, its TestsRunTest timed alone, and reads its results. Null when the name does not match.</summary>
        private static PairsFound Q133Run(Document document, DocumentClashTests clashTests, Q133Test t, out double seconds)
        {
            seconds = -1;

            using (ClashTest test = ResolveTest(clashTests, t.Address))
            {
                if (test == null || test.DisplayName != t.Name)
                {
                    return null;
                }

                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                clashTests.TestsRunTest(test);
                clock.Stop();
                seconds = clock.Elapsed.TotalSeconds;
            }

            using (ClashTest test = ResolveTest(clashTests, t.Address))
            {
                return test == null ? null : ReadPairs(document, test.Children);
            }
        }

        private static Q133Diff Q133Compare(PairsFound left, PairsFound right)
        {
            Q133Diff d = new Q133Diff();

            foreach (string key in left.Open.Keys)
            {
                if (right.Open.ContainsKey(key))
                {
                    d.Both++;
                }
                else
                {
                    d.OnlyLeft.Add(key + "   distance " + left.Distance[key].ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            foreach (string key in right.Open.Keys)
            {
                if (!left.Open.ContainsKey(key))
                {
                    d.OnlyRight.Add(key + "   distance " + right.Distance[key].ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            d.OnlyLeft.Sort(StringComparer.Ordinal);
            d.OnlyRight.Sort(StringComparer.Ordinal);
            return d;
        }

        private static string SideSets(Document document, ClashSelection side)
        {
            List<string> names = new List<string>();

            using (Selection selection = side.Selection)
            {
                SelectionSourceCollection sources = selection.SelectionSources;

                for (int i = 0; i < sources.Count; i++)
                {
                    try
                    {
                        using (SavedItem pointed = document.SelectionSets.ResolveSelectionSource(sources[i]))
                        {
                            names.Add(pointed == null ? "(a source that resolves to nothing)" : pointed.DisplayName);
                        }
                    }
                    catch (Exception error)
                    {
                        names.Add("(a source that threw " + error.GetType().Name + ")");
                    }
                }

                if (selection.HasExplicitSelection)
                {
                    names.Add("(explicit items)");
                }
            }

            return string.Join(" + ", names.ToArray());
        }

        /// <summary>
        /// Adds a new ClashTest with the original's sides swapped at the end of the root, the way P1
        /// made its swap, and clears its results. Returns null and the swap's address, or why not.
        /// </summary>
        private static string Q133AddSwap(Document document, DocumentClashTests clashTests, Q133Test t, string swapName, out Q133Test swapTest, out bool sidesSwapped)
        {
            swapTest = null;
            sidesSwapped = false;
            int before = clashTests.Tests.Count;
            string originalA;
            string originalB;

            using (ClashTest original = ResolveTest(clashTests, t.Address))
            {
                if (original == null || original.DisplayName != t.Name)
                {
                    return "the original is not at its address";
                }

                if (original.IgnoreRules.Count != 0)
                {
                    return "the original carries " + original.IgnoreRules.Count + " ignore rules, which a new test does not";
                }

                using (ClashTest swap = new ClashTest())
                {
                    swap.DisplayName = swapName;
                    swap.TestType = original.TestType;
                    swap.Tolerance = original.Tolerance;
                    swap.MergeComposites = original.MergeComposites;
                    swap.SimulationType = original.SimulationType;

                    using (ClashSelection a = original.SelectionA)
                    using (ClashSelection b = original.SelectionB)
                    using (ClashSelection swapA = swap.SelectionA)
                    using (ClashSelection swapB = swap.SelectionB)
                    {
                        originalA = SideSets(document, a);
                        originalB = SideSets(document, b);
                        swapA.CopyFrom(b);
                        swapB.CopyFrom(a);
                        swapA.SelfIntersect = b.SelfIntersect;
                        swapB.SelfIntersect = a.SelfIntersect;
                        swapA.PrimitiveTypes = b.PrimitiveTypes;
                        swapB.PrimitiveTypes = a.PrimitiveTypes;
                    }

                    clashTests.TestsAddCopy(swap);
                }
            }

            int after = clashTests.Tests.Count;

            if (after != before + 1)
            {
                return "the root went from " + before + " to " + after + " tests, not one more";
            }

            Q133Test added = new Q133Test { Address = new List<int> { before }, Name = swapName };

            using (ClashTest swap = ResolveTest(clashTests, added.Address))
            {
                if (swap == null || swap.DisplayName != swapName)
                {
                    return "the last test at the root is not the swap";
                }

                using (ClashSelection a = swap.SelectionA)
                using (ClashSelection b = swap.SelectionB)
                {
                    sidesSwapped = SideSets(document, a) == originalB && SideSets(document, b) == originalA;
                }

                clashTests.TestsClearResults(swap);
            }

            swapTest = added;
            return null;
        }

        private Q133Totals Q133Swaps(Document document, DocumentClashTests clashTests, List<Q133Test> list, Dictionary<string, PairsFound> stored, string tag, bool listPairs)
        {
            Q133Totals totals = new Q133Totals();
            System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;

            foreach (Q133Test t in list)
            {
                string swapName = t.Name + " Q133 swap";
                Q133Test swap;
                bool sidesSwapped;
                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                string why;

                try
                {
                    why = Q133AddSwap(document, clashTests, t, swapName, out swap, out sidesSwapped);
                }
                catch (Exception error)
                {
                    why = "the swap threw " + error.GetType().Name + ": " + error.Message;
                    swap = null;
                    sidesSwapped = false;
                }

                double create = clock.Elapsed.TotalSeconds;
                totals.Tests++;

                if (why != null)
                {
                    totals.Unknown++;
                    Say(tag + " LINE  \"" + t.Name + "\"  UNKNOWN, no swap: " + why);
                    continue;
                }

                double s1;
                double s2;
                PairsFound original = Q133Run(document, clashTests, t, out s1);
                PairsFound swapped = Q133Run(document, clashTests, swap, out s2);

                if (original == null || swapped == null)
                {
                    totals.Unknown++;
                    Say(tag + " LINE  \"" + t.Name + "\"  UNKNOWN, the original or the swap was not at its address when run");
                    continue;
                }

                Q133Diff d = Q133Compare(original, swapped);
                Q133Diff rerun = Q133Compare(stored[t.Name], original);
                string verdict;

                if (original.NullItems > 0 || swapped.NullItems > 0 || !sidesSwapped)
                {
                    verdict = "UNKNOWN";
                    totals.Unknown++;
                }
                else if (d.OnlyLeft.Count == 0 && d.OnlyRight.Count == 0)
                {
                    verdict = "same";
                    totals.Same++;
                }
                else if (d.OnlyLeft.Count == 0)
                {
                    verdict = "swap finds more";
                    totals.More++;
                }
                else if (d.OnlyRight.Count == 0)
                {
                    verdict = "swap finds fewer";
                    totals.Fewer++;
                }
                else
                {
                    verdict = "other clashes";
                    totals.Other++;
                }

                totals.Stored += stored[t.Name].Open.Count;
                totals.Original += original.Open.Count;
                totals.Swap += swapped.Open.Count;
                totals.OnlySwap += d.OnlyRight.Count;
                totals.OnlyOriginal += d.OnlyLeft.Count;
                totals.OriginalSeconds += s1;
                totals.SwapSeconds += s2;
                totals.CreateSeconds += create;

                Say(tag + " LINE  \"" + t.Name + "\"  stored " + stored[t.Name].Open.Count
                    + ", original run " + original.Open.Count + " in " + s1.ToString("0.000", inv) + " s"
                    + ", swap " + swapped.Open.Count + " in " + s2.ToString("0.000", inv) + " s"
                    + ", swap made in " + create.ToString("0.000", inv) + " s"
                    + ", in both " + d.Both + ", original only " + d.OnlyLeft.Count + ", swap only " + d.OnlyRight.Count
                    + ", run against stored differ " + (rerun.OnlyLeft.Count + rerun.OnlyRight.Count)
                    + ", items that did not read " + original.NullItems + " and " + swapped.NullItems
                    + ", sides read swapped " + sidesSwapped + ", " + verdict);

                if (listPairs || d.OnlyLeft.Count > 0 || d.OnlyRight.Count > 0)
                {
                    foreach (string k in d.OnlyLeft)
                    {
                        Say("      only in the original: " + k);
                    }

                    foreach (string k in d.OnlyRight)
                    {
                        Say("      only in the swap: " + k);
                    }
                }
            }

            return totals;
        }

        private static void Q133Add(Q133Totals into, Q133Totals from)
        {
            into.Tests += from.Tests;
            into.Same += from.Same;
            into.More += from.More;
            into.Fewer += from.Fewer;
            into.Other += from.Other;
            into.Unknown += from.Unknown;
            into.Stored += from.Stored;
            into.Original += from.Original;
            into.Swap += from.Swap;
            into.OnlySwap += from.OnlySwap;
            into.OnlyOriginal += from.OnlyOriginal;
            into.OriginalSeconds += from.OriginalSeconds;
            into.SwapSeconds += from.SwapSeconds;
            into.CreateSeconds += from.CreateSeconds;
        }

        private void Q133Say(string label, Q133Totals t)
        {
            System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;
            Say(label + "  tests " + t.Tests + ": swap finds the same " + t.Same + ", more " + t.More + ", fewer " + t.Fewer
                + ", other clashes " + t.Other + ", UNKNOWN " + t.Unknown);
            Say(label + "  clashes stored " + t.Stored + ", original run " + t.Original + ", swap " + t.Swap
                + ", only the swap finds " + t.OnlySwap + ", only the original finds " + t.OnlyOriginal);
            Say(label + "  seconds of TestsRunTest, the originals " + t.OriginalSeconds.ToString("0.000", inv)
                + ", the swaps " + t.SwapSeconds.ToString("0.000", inv)
                + ", both " + (t.OriginalSeconds + t.SwapSeconds).ToString("0.000", inv)
                + ", and making the swaps " + t.CreateSeconds.ToString("0.000", inv));
        }

        // ---------- P4 of Q114, scan.md 5z-m ----------

        /// <summary>
        /// The workset names of the models whose file name carries one of the codes handed
        /// in, each list whole. Whole means every item under the model's root was visited,
        /// no item's read threw, and the model was read from under the loop folder. The
        /// Workset is read off the LcRevitData_Element tab the way ModelFactsReader reads
        /// it, and every other tab is searched for a property whose name holds "workset",
        /// so a workset carried somewhere else cannot hide.
        /// </summary>
        private void MeasureModelWorksets(string nwf, string codes)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(codes))
            {
                Say("UNKNOWN: no discipline codes were handed in");
                return;
            }

            List<string> wanted = new List<string>();

            foreach (string part in codes.Split(','))
            {
                if (part.Trim().Length > 0)
                {
                    wanted.Add(part.Trim());
                }
            }

            Say("codes asked: " + Joined(wanted));
            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count + ", document units " + document.Units);

            List<int> chosen = new List<int>();

            for (int m = 0; m < document.Models.Count; m++)
            {
                Model model = document.Models[m];
                string file = model.FileName ?? string.Empty;
                string name = Path.GetFileName(file);
                bool under = file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase);
                string code = string.Empty;

                foreach (string w in wanted)
                {
                    if (name.IndexOf("-" + w + "-", StringComparison.Ordinal) >= 0)
                    {
                        code = w;
                    }
                }

                Say("   model " + m + "  " + name + "  under the loop folder " + under
                    + (code.Length > 0 ? "  READ, code " + code : string.Empty));

                if (code.Length > 0)
                {
                    chosen.Add(m);
                }
            }

            Say("models whose file name carries an asked code: " + chosen.Count);

            foreach (int m in chosen)
            {
                WalkWorksetsWhole(document.Models[m], loopRoot);
            }
        }

        private void WalkWorksetsWhole(Model model, string loopRoot)
        {
            string file = model.FileName ?? string.Empty;
            bool under = file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase);
            Say(string.Empty);
            Say("================ " + Path.GetFileName(file) + " ================");

            int items = 0;
            int geometry = 0;
            int elementTabs = 0;
            int withWorkset = 0;
            int itemErrors = 0;
            List<string> errorNotes = new List<string>();
            string walkThrew = string.Empty;
            Dictionary<string, int> worksets = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> elementProperty = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> elsewhere = new Dictionary<string, int>(StringComparer.Ordinal);
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (ModelItem root = model.RootItem)
                {
                    foreach (ModelItem item in root.DescendantsAndSelf)
                    {
                        using (item)
                        {
                            items++;

                            try
                            {
                                if (item.HasGeometry)
                                {
                                    geometry++;
                                }

                                ReadWorksetTabs(item, worksets, elementProperty, elsewhere, ref elementTabs, ref withWorkset);
                            }
                            catch (Exception error)
                            {
                                itemErrors++;

                                if (errorNotes.Count < 5)
                                {
                                    errorNotes.Add(error.GetType().Name + ": " + error.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                walkThrew = error.GetType().Name + ": " + error.Message;
            }

            Say("walk took " + Seconds(clock) + ", the walk threw: " + (walkThrew.Length == 0 ? "no" : walkThrew));
            Say("items " + items + ", with geometry " + geometry + ", with the LcRevitData_Element tab " + elementTabs
                + ", of those with a Workset value " + withWorkset + ", items whose read threw " + itemErrors);

            foreach (string note in errorNotes)
            {
                Say("   an item's read threw " + note);
            }

            Say("the Workset property on the Element tab, by display name, internal name and value type:");

            if (elementProperty.Count == 0)
            {
                Say("   none");
            }

            foreach (KeyValuePair<string, int> pair in elementProperty)
            {
                Say("   " + pair.Key + "  on " + pair.Value + " item(s)");
            }

            Say("a property naming workset on any OTHER tab, by tab, property and value:");

            if (elsewhere.Count == 0)
            {
                Say("   none");
            }

            foreach (KeyValuePair<string, int> pair in elsewhere)
            {
                Say("   " + pair.Key + "  on " + pair.Value + " item(s)");
            }

            List<string> names = new List<string>(worksets.Keys);
            names.Sort(StringComparer.Ordinal);
            Say("WORKSET NAMES, " + names.Count + ", each in brackets, with its length and the Element tabs carrying it:");

            foreach (string name in names)
            {
                Say("   [" + name + "]  length " + name.Length + "  on " + worksets[name] + "  " + Unusual(name));
            }

            bool whole = walkThrew.Length == 0 && itemErrors == 0 && under;
            Say("LIST WHOLE: " + (whole ? "YES" : "NO")
                + ", the walk finished " + (walkThrew.Length == 0)
                + ", no item's read threw " + (itemErrors == 0)
                + ", read from under the loop folder " + under);
        }

        private static void ReadWorksetTabs(
            ModelItem item,
            Dictionary<string, int> worksets,
            Dictionary<string, int> elementProperty,
            Dictionary<string, int> elsewhere,
            ref int elementTabs,
            ref int withWorkset)
        {
            using (PropertyCategoryCollection tabs = item.PropertyCategories)
            {
                if (tabs == null)
                {
                    return;
                }

                foreach (PropertyCategory tab in tabs)
                {
                    bool isElement = string.Equals(Words(tab.Name), ElementTabInternalName, StringComparison.OrdinalIgnoreCase);

                    if (isElement)
                    {
                        elementTabs++;
                    }

                    using (DataPropertyCollection properties = tab.Properties)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            using (DataProperty property = properties[i])
                            {
                                string display = Words(property.DisplayName);
                                string internalName = Words(property.Name);
                                string text;
                                string type;

                                if (isElement && string.Equals(display, "Workset", StringComparison.OrdinalIgnoreCase))
                                {
                                    StrictText(property, out text, out type);
                                    Bump(elementProperty, "[" + display + "] [" + internalName + "] " + type);

                                    if (text.Length > 0)
                                    {
                                        withWorkset++;
                                        Bump(worksets, text);
                                    }
                                }
                                else if (display.IndexOf("workset", StringComparison.OrdinalIgnoreCase) >= 0
                                    || internalName.IndexOf("workset", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    StrictText(property, out text, out type);
                                    Bump(elsewhere, "tab [" + Words(tab.DisplayName) + "] [" + Words(tab.Name) + "] property ["
                                        + display + "] [" + internalName + "] " + type + " value [" + text + "]");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>The value as text, letting a failed read throw so it is counted and not hidden.</summary>
        private static void StrictText(DataProperty property, out string text, out string type)
        {
            using (VariantData value = property.Value)
            {
                if (value == null)
                {
                    text = string.Empty;
                    type = "null";
                    return;
                }

                type = value.DataType.ToString();

                if (value.DataType == VariantDataType.DisplayString)
                {
                    text = value.ToDisplayString() ?? string.Empty;
                }
                else if (value.DataType == VariantDataType.IdentifierString)
                {
                    text = value.ToIdentifierString() ?? string.Empty;
                }
                else
                {
                    text = value.ToString() ?? string.Empty;
                }
            }
        }

        /// <summary>Says a leading or trailing space and any character outside printable ASCII, by code point.</summary>
        private static string Unusual(string name)
        {
            List<string> notes = new List<string>();

            if (name.Length > 0 && char.IsWhiteSpace(name[0]))
            {
                notes.Add("LEADING SPACE");
            }

            if (name.Length > 0 && char.IsWhiteSpace(name[name.Length - 1]))
            {
                notes.Add("TRAILING SPACE");
            }

            foreach (char c in name)
            {
                if (c < 0x20 || c > 0x7e)
                {
                    notes.Add("U+" + ((int)c).ToString("X4"));
                }
            }

            return notes.Count == 0 ? "plain ASCII" : string.Join(", ", notes.ToArray());
        }

        // ---------- P8 of Q114, the whole saved viewpoint tree, read only ----------

        /// <summary>
        /// P8 of Q114, scan.md 5z-n. Read only: opens the copy, writes nothing to it and saves
        /// nothing. Every item of the saved viewpoint tree goes in the dump with its index
        /// path, depth, folder or viewpoint, child count, comment count, redline count, Guid,
        /// folder path and name. Each item is judged by the legacy rule of the design's 1.9 as
        /// written there, with F85's own defaults: the priority words A, B, C and No priority,
        /// the codes AR ST ME FF PL DR EL and UNKNOWN sorted Ordinal with " vs " between, the
        /// size folder Over 150mm, the name separator of two spaces and the prefix Clash. The
        /// test names are the document's own. The XML's are not read.
        /// </summary>
        private void DumpViewpointTree(string nwf, string dumpPath)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(dumpPath))
            {
                Say("UNKNOWN: no dump path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count + ", document units " + document.Units);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            HashSet<string> testNames = new HashSet<string>(StringComparer.Ordinal);
            int testCount = 0;
            CollectTestNames(document.GetClash().TestsData.Tests, testNames, ref testCount);
            Say("tests in the document " + testCount + ", distinct names " + testNames.Count);

            TreeTally tally = new TreeTally();
            List<string> rows = new List<string>();
            rows.Add("index_path\tdepth\tkind\tchildren\tcomments\tredlines\tguid\tlegacy\twhy\tfolder_path\tname");
            clock = System.Diagnostics.Stopwatch.StartNew();

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                Say("the root: a " + root.GetType().Name + " with " + root.Children.Count + " children");
                WalkViewpointTree(root, string.Empty, new List<string>(), testNames, rows, tally);
            }

            Say("the walk took " + Seconds(clock) + ", items " + (rows.Count - 1));

            using (StreamWriter dump = new StreamWriter(dumpPath, false, new UTF8Encoding(false)))
            {
                foreach (string row in rows)
                {
                    dump.Write(row);
                    dump.Write("\r\n");
                }
            }

            Say("the dump written, " + rows.Count + " lines with its header, " + Bytes(dumpPath) + " bytes read back off the disk");
            Say(string.Empty);
            Say("items " + tally.Items + ", folders " + tally.Folders + ", viewpoints " + tally.Viewpoints
                + ", other kinds " + tally.Other);
            Say("items whose read threw " + tally.Threw);
            Say("comment counts that threw " + tally.CommentThrew + ", redline reads that threw " + tally.RedlineThrew);
            Say("items carrying at least one comment " + tally.WithComments + ", comments in all " + tally.Comments);
            Say("viewpoints carrying at least one redline " + tally.WithRedlines);
            Say("Guids read " + tally.Guids.Count + ", empty " + tally.EmptyGuids + ", distinct " + DistinctCount(tally.Guids)
                + ", Guid reads that threw " + tally.GuidThrew);
            Say("names with a leading or trailing space " + tally.SpaceEdged);

            foreach (KeyValuePair<int, int> pair in Sorted(tally.ViewpointsAtDepth))
            {
                Say("   viewpoints at depth " + pair.Key + ": " + pair.Value);
            }

            Say(string.Empty);
            Say("EVERY FOLDER, its index path, depth, children, viewpoints under it and comments:");

            foreach (string line in tally.FolderLines)
            {
                Say("   " + line);
            }

            Say(string.Empty);
            Say("LEGACY BY THE RULE OF 1.9: " + tally.Legacy + ", NOT LEGACY: " + tally.NotLegacy
                + " of which viewpoints " + tally.NotLegacyViewpoints + " and folders " + (tally.NotLegacy - tally.NotLegacyViewpoints));

            foreach (KeyValuePair<string, int> pair in SortedText(tally.WhyNot))
            {
                Say("   not legacy, " + pair.Key + ": " + pair.Value);
            }

            Say("legacy viewpoints by folder path:");

            foreach (KeyValuePair<string, int> pair in SortedText(tally.LegacyByFolder))
            {
                Say("   " + pair.Value.ToString().PadLeft(5) + "  " + pair.Key);
            }

            Say(string.Empty);
            Say("EVERY VIEWPOINT THAT IS NOT LEGACY, its index path, depth, comments, redlines, Guid, why, folder path and [name]:");

            foreach (string line in tally.NotLegacyViewpointLines)
            {
                Say("   " + line);
            }

            Say(string.Empty);
            Say("legacy viewpoints by test, all of them and those under Over 150mm, for "
                + tally.LegacyByTest.Count + " tests:");
            int underOver = 0;
            int testsWithOver = 0;

            foreach (KeyValuePair<string, int> pair in SortedText(tally.LegacyByTest))
            {
                int over;
                tally.OverByTest.TryGetValue(pair.Key, out over);
                underOver += over;

                if (over > 0)
                {
                    testsWithOver++;
                }

                Say("   " + pair.Value.ToString().PadLeft(5) + "  over " + over.ToString().PadLeft(5) + "  [" + Shown(pair.Key) + "]");
            }

            Say("legacy viewpoints under Over 150mm " + underOver + ", in " + testsWithOver + " tests");
            Say("distinct folders holding a legacy viewpoint " + tally.LegacyByFolder.Count
                + ", distinct pairs of test and folder path " + tally.TestFolderPairs.Count);
            Say(string.Empty);

            bool total = tally.Viewpoints == 2847;
            bool legacy = tally.Legacy == 2813;
            bool other = tally.NotLegacyViewpoints == 34;
            Say("P8 " + (total && legacy && other && tally.Threw == 0 ? "YES" : "NO")
                + "   viewpoints " + tally.Viewpoints + " against 2847, legacy " + tally.Legacy + " against 2813, viewpoints not legacy "
                + tally.NotLegacyViewpoints + " against 34, items whose read threw " + tally.Threw);
        }

        private sealed class TreeTally
        {
            public int Items;
            public int Folders;
            public int Viewpoints;
            public int Other;
            public int Threw;
            public int CommentThrew;
            public int RedlineThrew;
            public int GuidThrew;
            public int WithComments;
            public int Comments;
            public int WithRedlines;
            public int EmptyGuids;
            public int SpaceEdged;
            public int Legacy;
            public int NotLegacy;
            public int NotLegacyViewpoints;
            public readonly List<Guid> Guids = new List<Guid>();
            public readonly Dictionary<int, int> ViewpointsAtDepth = new Dictionary<int, int>();
            public readonly Dictionary<string, int> WhyNot = new Dictionary<string, int>(StringComparer.Ordinal);
            public readonly Dictionary<string, int> LegacyByFolder = new Dictionary<string, int>(StringComparer.Ordinal);
            public readonly Dictionary<string, int> LegacyByTest = new Dictionary<string, int>(StringComparer.Ordinal);
            public readonly Dictionary<string, int> OverByTest = new Dictionary<string, int>(StringComparer.Ordinal);
            public readonly HashSet<string> TestFolderPairs = new HashSet<string>(StringComparer.Ordinal);
            public readonly List<string> FolderLines = new List<string>();
            public readonly List<string> NotLegacyViewpointLines = new List<string>();
        }

        private static readonly string[] LegacyPriorityWords = { "A", "B", "C", "No priority" };
        private static readonly string[] LegacyCodes = { "AR", "ST", "ME", "FF", "PL", "DR", "EL", "UNKNOWN" };
        private const string LegacyPairSeparator = " vs ";
        private const string LegacySizeFolder = "Over 150mm";
        private const string LegacyNameSeparator = "  ";
        private const string LegacyClashPrefix = "Clash";

        /// <summary>Walks one folder. Returns the viewpoints under it, so a folder's line can carry them.</summary>
        private int WalkViewpointTree(GroupItem folder, string indexPath, List<string> folders, HashSet<string> testNames, List<string> rows, TreeTally tally)
        {
            int under = 0;
            SavedItemCollection children = folder.Children;

            for (int i = 0; i < children.Count; i++)
            {
                string at = indexPath.Length == 0 ? i.ToString() : indexPath + "." + i;

                try
                {
                    using (SavedItem child = children[i])
                    {
                        tally.Items++;
                        string name = child.DisplayName ?? string.Empty;
                        int depth = folders.Count;
                        string folderPath = string.Join("/", folders.ToArray());

                        if (name.Length > 0 && (char.IsWhiteSpace(name[0]) || char.IsWhiteSpace(name[name.Length - 1])))
                        {
                            tally.SpaceEdged++;
                        }

                        string comments = "UNKNOWN";
                        int commentCount = -1;

                        try
                        {
                            commentCount = child.Comments.Count;
                            comments = commentCount.ToString();
                            tally.Comments += commentCount;

                            if (commentCount > 0)
                            {
                                tally.WithComments++;
                            }
                        }
                        catch (Exception error)
                        {
                            tally.CommentThrew++;
                            comments = "THREW " + error.GetType().Name;
                        }

                        string guid = "UNKNOWN";

                        try
                        {
                            Guid g = child.Guid;
                            guid = g.ToString("D");
                            tally.Guids.Add(g);

                            if (g == Guid.Empty)
                            {
                                tally.EmptyGuids++;
                            }
                        }
                        catch (Exception error)
                        {
                            tally.GuidThrew++;
                            guid = "THREW " + error.GetType().Name;
                        }

                        GroupItem group = child as GroupItem;
                        SavedViewpoint viewpoint = child as SavedViewpoint;
                        string kind;
                        string childCount = string.Empty;
                        string redlines = string.Empty;
                        int redlineCount = 0;

                        if (group != null)
                        {
                            kind = "folder";
                            tally.Folders++;
                            childCount = group.Children.Count.ToString();
                        }
                        else if (viewpoint != null)
                        {
                            kind = "viewpoint";
                            tally.Viewpoints++;
                            under++;
                            int had;
                            tally.ViewpointsAtDepth.TryGetValue(depth, out had);
                            tally.ViewpointsAtDepth[depth] = had + 1;

                            try
                            {
                                redlineCount = viewpoint.Redlines.Size();
                                redlines = redlineCount.ToString();

                                if (redlineCount > 0)
                                {
                                    tally.WithRedlines++;
                                }
                            }
                            catch (Exception error)
                            {
                                tally.RedlineThrew++;
                                redlineCount = -1;
                                redlines = "THREW " + error.GetType().Name;
                            }
                        }
                        else
                        {
                            kind = child.GetType().Name;
                            tally.Other++;
                        }

                        string why = LegacyWhyNot(folders, name, viewpoint != null, commentCount, redlineCount, testNames);
                        bool isLegacy = why.Length == 0;

                        if (isLegacy)
                        {
                            tally.Legacy++;
                            Bump(tally.LegacyByFolder, folderPath);
                            string test = name.Substring(0, name.LastIndexOf(LegacyNameSeparator + LegacyClashPrefix, StringComparison.Ordinal));
                            Bump(tally.LegacyByTest, test);
                            tally.TestFolderPairs.Add(test + "\n" + folderPath);

                            if (folders[folders.Count - 1] == LegacySizeFolder)
                            {
                                Bump(tally.OverByTest, test);
                            }
                        }
                        else
                        {
                            tally.NotLegacy++;
                            Bump(tally.WhyNot, why);

                            if (viewpoint != null)
                            {
                                tally.NotLegacyViewpoints++;
                                tally.NotLegacyViewpointLines.Add(at + "  depth " + depth + "  comments " + comments + "  redlines " + redlines
                                    + "  " + guid + "  " + why + "  " + Shown(folderPath) + "  [" + Shown(name) + "]");
                            }
                        }

                        rows.Add(at + "\t" + depth + "\t" + kind + "\t" + childCount + "\t" + comments + "\t" + redlines + "\t" + guid
                            + "\t" + (isLegacy ? "legacy" : "no") + "\t" + why + "\t" + Shown(folderPath) + "\t" + Shown(name));

                        if (group != null)
                        {
                            folders.Add(name);
                            int inside = WalkViewpointTree(group, at, folders, testNames, rows, tally);
                            folders.RemoveAt(folders.Count - 1);
                            under += inside;
                            tally.FolderLines.Add(at + "  depth " + depth + "  children " + childCount + "  viewpoints under it " + inside
                                + "  comments " + comments + "  " + guid + "  [" + Shown(folderPath.Length == 0 ? name : folderPath + "/" + name) + "]");
                        }
                    }
                }
                catch (Exception error)
                {
                    tally.Threw++;
                    rows.Add(at + "\t\tTHREW\t\t\t\t\t\t" + error.GetType().Name + ": " + Shown(error.Message) + "\t\t");
                }
            }

            return under;
        }

        /// <summary>Empty when the item is legacy by the rule of 1.9, else the first condition it fails.</summary>
        private static string LegacyWhyNot(List<string> folders, string name, bool isViewpoint, int comments, int redlines, HashSet<string> testNames)
        {
            if (!isViewpoint)
            {
                return "2 not a viewpoint";
            }

            if (folders.Count < 1 || folders.Count > 3)
            {
                return "1 depth " + folders.Count;
            }

            int at = 0;

            if (folders.Count > 1 && Array.IndexOf(LegacyPriorityWords, folders[0]) >= 0)
            {
                at = 1;
            }

            if (at >= folders.Count || !IsSortedPair(folders[at]))
            {
                return "1 no sorted code pair folder where one belongs";
            }

            at++;

            if (at < folders.Count && folders[at] == LegacySizeFolder)
            {
                at++;
            }

            if (at != folders.Count)
            {
                return "1 a folder the rule does not allow";
            }

            int split = name.LastIndexOf(LegacyNameSeparator + LegacyClashPrefix, StringComparison.Ordinal);

            if (split <= 0)
            {
                return "3 no test, two spaces and Clash";
            }

            string digits = name.Substring(split + LegacyNameSeparator.Length + LegacyClashPrefix.Length);

            if (digits.Length == 0)
            {
                return "3 Clash with no digits";
            }

            foreach (char c in digits)
            {
                if (c < '0' || c > '9')
                {
                    return "3 Clash followed by more than digits";
                }
            }

            if (!testNames.Contains(name.Substring(0, split)))
            {
                return "4 the test is not in the document";
            }

            if (comments != 0)
            {
                return comments < 0 ? "5 comments UNKNOWN" : "5 carries a comment";
            }

            if (redlines != 0)
            {
                return redlines < 0 ? "5 redlines UNKNOWN" : "5 carries a redline";
            }

            return string.Empty;
        }

        private static bool IsSortedPair(string folder)
        {
            int split = folder.IndexOf(LegacyPairSeparator, StringComparison.Ordinal);

            if (split <= 0)
            {
                return false;
            }

            string first = folder.Substring(0, split);
            string second = folder.Substring(split + LegacyPairSeparator.Length);

            return Array.IndexOf(LegacyCodes, first) >= 0
                && Array.IndexOf(LegacyCodes, second) >= 0
                && string.CompareOrdinal(first, second) <= 0;
        }

        private static void CollectTestNames(SavedItemCollection items, HashSet<string> names, ref int count)
        {
            for (int i = 0; i < items.Count; i++)
            {
                using (SavedItem item = items[i])
                {
                    if (item is ClashTest)
                    {
                        count++;
                        names.Add(item.DisplayName ?? string.Empty);
                        continue;
                    }

                    GroupItem folder = item as GroupItem;

                    if (folder != null)
                    {
                        CollectTestNames(folder.Children, names, ref count);
                    }
                }
            }
        }

        private static int DistinctCount(List<Guid> guids)
        {
            return new HashSet<Guid>(guids).Count;
        }

        private static List<KeyValuePair<int, int>> Sorted(Dictionary<int, int> counts)
        {
            List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>(counts);
            list.Sort((a, b) => a.Key.CompareTo(b.Key));
            return list;
        }

        private static List<KeyValuePair<string, int>> SortedText(Dictionary<string, int> counts)
        {
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(counts);
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }

        /// <summary>The text as it is, but a tab, a line break or any other control character written as \uXXXX, so a dump row stays one row.</summary>
        private static string Shown(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder built = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (c < 0x20 || c == 0x7f || c == '\\')
                {
                    built.Append("\\u").Append(((int)c).ToString("X4"));
                }
                else
                {
                    built.Append(c);
                }
            }

            return built.ToString();
        }

        // ---------- P9 of Q114, scan.md 5z-o, does a comment on a saved view or folder survive ----------

        private const string P9Author = "Parsons NWC Federator";
        private const string P9Sentence = "Made by the NWC Federator and replaced on its next run. Rename it, move it or add a comment to keep it.";
        private const string P9Top = "P9 probe";
        private const string P9Sub = "P9 sub";
        private const string P9ComFolder = "P9 com folder";

        private sealed class P9Target
        {
            public string Label;
            public string Route;
            public bool IsFolder;
            public List<string> Path = new List<string>();
            public string Body;
            public bool Written;
            public string WriteSeconds = "UNKNOWN";
            public int HiddenBefore = -2;
            public int MaterialBefore = -2;
            public int HiddenAfterEdit = -2;
            public int MaterialAfterEdit = -2;
            public bool NowSame;
            public bool ReopenSame;
            public int HiddenNow = -2;
            public int MaterialNow = -2;
            public int HiddenReopen = -2;
            public int MaterialReopen = -2;
        }

        /// <summary>
        /// P9: a comment written by DocumentSavedViewpoints.AddComment after the add, and one put
        /// on the COM view's Comments() before InwSavedViewsColl.Add where P6 said yes, on a
        /// viewpoint two folders deep and on a folder. Each is read back off SavedItem.Comments
        /// before a save, and after a SaveFile, a Document.Clear and a TryOpenFile of the saved
        /// file. A viewpoint's Hidden count and MaterialOverrides count are read before and after
        /// the edit, and the seconds of each write are read.
        /// </summary>
        private void MeasureViewComments(string nwf, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no save path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            int folders0;
            int comments0;
            int views0 = TreeCounts(document, out folders0, out comments0);
            Say("the tree at the open: viewpoints " + views0 + ", folders " + folders0 + ", comments on any item " + comments0);

            List<string> legacyPath = FirstViewTwoDeep(document);

            if (legacyPath == null)
            {
                Say("UNKNOWN: no viewpoint two folders deep in this tree, so the existing view is not measured");
            }
            else
            {
                Say("the existing viewpoint two folders deep, the first found: [" + Shown(string.Join(" / ", legacyPath.ToArray())) + "]");
            }

            // The state the new views record: model 0's root hidden, one clash pair painted red and green.
            int[] first;
            int[] second;
            string pairName;
            document.Models.ResetAllHidden();
            document.Models.ResetAllTemporaryMaterials();

            using (ModelItemCollection one = new ModelItemCollection())
            {
                one.Add(document.Models[0].RootItem);
                document.Models.SetHidden(one, true);
            }

            if (FindClashPair(document, out first, out second, out pairName))
            {
                PaintOne(document, first, Color.Red);
                PaintOne(document, second, Color.Green);
                Say("model 0's root hidden, the pair [" + Shown(pairName) + "] painted red and green");
            }
            else
            {
                Say("model 0's root hidden, no clash pair with geometry found, so nothing is painted");
            }

            // The folders, made the way the tool makes them, a .NET FolderItem by AddCopy.
            clock = System.Diagnostics.Stopwatch.StartNew();
            EnsureFolder(document, P9Top);

            using (GroupItem top = FindFolderItem(document, P9Top))
            using (FolderItem sub = new FolderItem())
            {
                sub.DisplayName = P9Sub;
                document.SavedViewpoints.AddCopy(top, sub);
            }

            Say("folders \"" + P9Top + "\" and \"" + P9Top + " / " + P9Sub + "\" made by FolderItem and AddCopy in " + Seconds(clock));

            string stamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            List<P9Target> targets = new List<P9Target>();

            P9Target comBefore = NewTarget(targets, "V1", "the COM view's Comments() before the add", false, stamp, P9Top, P9Sub, "P9 view com before add");
            P9Target addAfter = NewTarget(targets, "V2", "AddComment after the add", false, stamp, P9Top, P9Sub, "P9 view addcomment after add");
            P9Target plain = NewTarget(targets, "V3", "no comment, the control", false, stamp, P9Top, P9Sub, "P9 view plain");
            P9Target folderAdd = NewTarget(targets, "F1", "AddComment on a folder one below the root folder", true, stamp, P9Top, P9Sub);
            P9Target folderCom = NewTarget(targets, "F2", "the COM folder view's Comments() before the add", true, stamp, P9Top, P9ComFolder);
            P9Target legacy = null;

            if (legacyPath != null)
            {
                legacy = NewTarget(targets, "L1", "AddComment on an existing viewpoint two folders deep", false, stamp, legacyPath.ToArray());
            }

            plain.Body = null;
            InwOpState10 state = ComApiBridge.State;
            InwOpFolderView comSub = FindComFolderAt(state, P9Top, P9Sub);
            InwOpFolderView comTop = FindComFolderAt(state, P9Top);
            Say("the COM folders found: \"" + P9Top + "\" " + (comTop != null) + ", \"" + P9Top + " / " + P9Sub + "\" " + (comSub != null));

            using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
            {
                // V1, the comment on the COM view before the add.
                try
                {
                    InwOpView view = NewComView(state, comBefore.Path[comBefore.Path.Count - 1], camera);
                    InwCommentsColl before = view.Comments();
                    Say("V1 the new COM view's Comments(): Count " + before.Count + ", ReadOnly " + ComReadOnly(before));
                    clock = System.Diagnostics.Stopwatch.StartNew();
                    object made = state.ObjectFactory(nwEObjectType.eObjectType_nwOpComment, null, null);
                    InwOpComment comment = (InwOpComment)made;
                    comment.Body = comBefore.Body;
                    comment.User = P9Author;
                    before.Add(comment);
                    comBefore.WriteSeconds = Seconds(clock);
                    Say("V1 the factory's comment is InwOpComment " + (made is InwOpComment) + ", InwOpComment2 " + (made is InwOpComment2)
                        + ", InwOpComment3 " + (made is InwOpComment3) + ". Made, Body and User set, and added in " + comBefore.WriteSeconds
                        + ", the view's Comments().Count now " + view.Comments().Count);
                    clock = System.Diagnostics.Stopwatch.StartNew();
                    comSub.SavedViews().Add(view);
                    Say("V1 InwSavedViewsColl.Add into \"" + P9Sub + "\" took " + Seconds(clock));
                    comBefore.Written = true;
                    SayComViewComments(FindComFolderAt(state, P9Top, P9Sub), comBefore.Path[comBefore.Path.Count - 1], "V1");
                }
                catch (Exception error)
                {
                    Say("V1 THREW " + error.GetType().Name + ": " + error.Message);
                }

                // V2 and V3, the COM view with no comment.
                foreach (P9Target bare in new[] { addAfter, plain })
                {
                    try
                    {
                        InwOpView view = NewComView(state, bare.Path[bare.Path.Count - 1], camera);
                        FindComFolderAt(state, P9Top, P9Sub).SavedViews().Add(view);
                        Say(bare.Label + " added through COM with no comment");

                        if (bare == plain)
                        {
                            plain.Written = true;
                        }
                    }
                    catch (Exception error)
                    {
                        Say(bare.Label + " THREW " + error.GetType().Name + ": " + error.Message);
                    }
                }

                // F2, a COM folder view with a comment before the add.
                try
                {
                    InwOpFolderView folder = (InwOpFolderView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpFolderView, null, null);
                    folder.name = P9ComFolder;
                    InwCommentsColl before = folder.Comments();
                    Say("F2 the new COM folder view's Comments(): Count " + before.Count + ", ReadOnly " + ComReadOnly(before));
                    clock = System.Diagnostics.Stopwatch.StartNew();
                    InwOpComment comment = (InwOpComment)state.ObjectFactory(nwEObjectType.eObjectType_nwOpComment, null, null);
                    comment.Body = folderCom.Body;
                    comment.User = P9Author;
                    before.Add(comment);
                    folderCom.WriteSeconds = Seconds(clock);
                    clock = System.Diagnostics.Stopwatch.StartNew();
                    FindComFolderAt(state, P9Top).SavedViews().Add(folder);
                    Say("F2 comment added in " + folderCom.WriteSeconds + ", InwSavedViewsColl.Add into \"" + P9Top + "\" took " + Seconds(clock));
                    folderCom.Written = true;
                }
                catch (Exception error)
                {
                    Say("F2 THREW " + error.GetType().Name + ": " + error.Message);
                }
            }

            // The .NET edits after the add: V2, F1 and L1.
            foreach (P9Target edit in new[] { addAfter, folderAdd, legacy })
            {
                if (edit == null)
                {
                    continue;
                }

                try
                {
                    using (SavedItem item = ResolveNames(document, edit.Path))
                    {
                        if (item == null)
                        {
                            Say(edit.Label + " NOT FOUND by its names, so no comment is written");
                            continue;
                        }

                        if (!edit.IsFolder)
                        {
                            edit.HiddenBefore = HiddenCount((SavedViewpoint)item);
                            edit.MaterialBefore = MaterialCount((SavedViewpoint)item);
                        }

                        Say(edit.Label + " before the edit: a " + item.GetType().Name + ", comments " + item.Comments.Count
                            + (edit.IsFolder ? string.Empty : ", Hidden " + edit.HiddenBefore + ", MaterialOverrides " + edit.MaterialBefore));

                        using (Comment comment = document.CreateCommentWithUniqueId(edit.Body, CommentStatus.New, P9Author))
                        {
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddComment(item, comment);
                            edit.WriteSeconds = Seconds(clock);
                        }

                        edit.Written = true;
                        Say(edit.Label + " AddComment(item, comment) RETURNED after " + edit.WriteSeconds);
                    }

                    using (SavedItem again = ResolveNames(document, edit.Path))
                    {
                        if (again != null && !edit.IsFolder)
                        {
                            edit.HiddenAfterEdit = HiddenCount((SavedViewpoint)again);
                            edit.MaterialAfterEdit = MaterialCount((SavedViewpoint)again);
                            Say(edit.Label + " after the edit, re-found by its names: Hidden " + edit.HiddenAfterEdit
                                + ", MaterialOverrides " + edit.MaterialAfterEdit);
                        }
                    }
                }
                catch (Exception error)
                {
                    Say(edit.Label + " THREW " + error.GetType().Name + ": " + error.Message);
                }
            }

            Say(string.Empty);
            Say("READ BACK BEFORE ANY SAVE, off SavedItem.Comments, each item re-found by its names from a fresh RootItem:");

            foreach (P9Target target in targets)
            {
                target.NowSame = ReadTarget(document, target, false);
            }

            int folders1;
            int comments1;
            int views1 = TreeCounts(document, out folders1, out comments1);
            Say("the tree before the save: viewpoints " + views1 + ", folders " + folders1 + ", comments on any item " + comments1);

            document.Models.ResetAllHidden();
            document.Models.ResetAllTemporaryMaterials();
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (!reopened)
            {
                Say("P9 UNKNOWN   the saved file would not reopen, so nothing after a reopen is read");
                return;
            }

            Say(string.Empty);
            Say("READ BACK AFTER A SAVE, A CLEAR AND A REOPEN:");

            foreach (P9Target target in targets)
            {
                target.ReopenSame = ReadTarget(document, target, true);
            }

            int folders2;
            int comments2;
            int views2 = TreeCounts(document, out folders2, out comments2);
            Say("the tree after the reopen: viewpoints " + views2 + ", folders " + folders2 + ", comments on any item " + comments2);
            Say(string.Empty);
            Say("EACH TARGET:  label | route | written | write seconds | read back the same before the save | after the reopen | Hidden before edit, after edit, before save, after reopen | MaterialOverrides the same four");

            foreach (P9Target target in targets)
            {
                Say("   " + target.Label + " | " + target.Route + " | " + target.Written + " | " + target.WriteSeconds + " | " + target.NowSame
                    + " | " + target.ReopenSame
                    + (target.IsFolder ? " | a folder" : " | " + target.HiddenBefore + ", " + target.HiddenAfterEdit + ", " + target.HiddenNow + ", " + target.HiddenReopen
                    + " | " + target.MaterialBefore + ", " + target.MaterialAfterEdit + ", " + target.MaterialNow + ", " + target.MaterialReopen));
            }

            Say("(-2 is not read, -1 is a read that threw)");
            bool viewAdd = addAfter.Written && addAfter.NowSame && addAfter.ReopenSame;
            bool viewCom = comBefore.Written && comBefore.NowSame && comBefore.ReopenSame;
            bool folderA = folderAdd.Written && folderAdd.NowSame && folderAdd.ReopenSame;
            bool folderC = folderCom.Written && folderCom.NowSame && folderCom.ReopenSame;
            bool legacyA = legacy != null && legacy.Written && legacy.NowSame && legacy.ReopenSame;
            bool countsHeld = SameCounts(addAfter) && (legacy == null || SameCounts(legacy));
            bool comCounts = comBefore.HiddenReopen == plain.HiddenReopen && comBefore.MaterialReopen == plain.MaterialReopen
                && comBefore.HiddenNow == plain.HiddenNow && comBefore.MaterialNow == plain.MaterialNow;
            Say("P9 by route: AddComment on a view two folders deep " + Yes(viewAdd) + ", on an existing view two folders deep " + Yes(legacyA)
                + ", on a folder " + Yes(folderA) + ". COM before the add on a view two folders deep " + Yes(viewCom) + ", on a folder " + Yes(folderC));
            Say("P9 counts: AddComment left Hidden and MaterialOverrides the same before and after the edit and through the reopen " + Yes(countsHeld)
                + ". The COM view with a comment reads the same counts as the plain one " + Yes(comCounts));
            Say("P9 " + ((viewAdd || viewCom) && (folderA || folderC) && countsHeld ? "YES" : "NO")
                + "   a comment on a view two folders deep and on a folder read back with the same body and author after a save, a clear and a reopen, by at least one route each, with the counts held");
        }

        private static string Yes(bool value)
        {
            return value ? "YES" : "NO";
        }

        private static bool SameCounts(P9Target t)
        {
            return t.HiddenBefore >= 0 && t.MaterialBefore >= 0
                && t.HiddenBefore == t.HiddenAfterEdit && t.HiddenBefore == t.HiddenNow && t.HiddenBefore == t.HiddenReopen
                && t.MaterialBefore == t.MaterialAfterEdit && t.MaterialBefore == t.MaterialNow && t.MaterialBefore == t.MaterialReopen;
        }

        private static P9Target NewTarget(List<P9Target> into, string label, string route, bool isFolder, string stamp, params string[] path)
        {
            P9Target target = new P9Target();
            target.Label = label;
            target.Route = route;
            target.IsFolder = isFolder;
            target.Path.AddRange(path);
            target.Body = P9Sentence + "\n" + "[nwcfed-mark 1] stamp=" + stamp + " path=" + string.Join("/", path, 0, path.Length - 1)
                + " name=" + path[path.Length - 1] + " probe=" + label;
            into.Add(target);
            return target;
        }

        private static InwOpView NewComView(InwOpState10 state, string name, Viewpoint camera)
        {
            InwOpView view = (InwOpView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpView, null, null);
            view.name = name;
            view.ApplyHideAttribs = true;
            view.ApplyMaterialAttribs = true;
            view.anonview = ComApiBridge.ToInwOpAnonView(camera);
            return view;
        }

        private static string ComReadOnly(InwCommentsColl comments)
        {
            try
            {
                return comments.ReadOnly.ToString();
            }
            catch (Exception error)
            {
                return "UNKNOWN, the read threw " + error.GetType().Name;
            }
        }

        /// <summary>The COM folder at that path of names from the COM root, each level read fresh.</summary>
        private static InwOpFolderView FindComFolderAt(InwOpState10 state, params string[] names)
        {
            InwSavedViewsColl views = state.SavedViews();
            InwOpFolderView found = null;

            foreach (string name in names)
            {
                found = null;

                for (int i = 1; i <= views.Count; i++)
                {
                    InwOpFolderView folder = views[i] as InwOpFolderView;

                    if (folder != null && string.Equals(folder.name, name, StringComparison.Ordinal))
                    {
                        found = folder;
                        break;
                    }
                }

                if (found == null)
                {
                    return null;
                }

                views = found.SavedViews();
            }

            return found;
        }

        private void SayComViewComments(InwOpFolderView folder, string name, string label)
        {
            if (folder == null)
            {
                Say(label + " read back through COM: the folder was not found");
                return;
            }

            InwSavedViewsColl views = folder.SavedViews();

            for (int i = views.Count; i >= 1; i--)
            {
                InwOpView view = views[i] as InwOpView;

                if (view != null && string.Equals(view.name, name, StringComparison.Ordinal))
                {
                    InwCommentsColl comments = view.Comments();
                    Say(label + " read back through COM, the added view's Comments().Count " + comments.Count);

                    for (int c = 1; c <= comments.Count; c++)
                    {
                        InwOpComment comment = comments[c] as InwOpComment;
                        Say("   COM comment " + c + ": " + (comment == null ? "not an InwOpComment" : "User [" + Shown(comment.User) + "] Body [" + Shown(comment.Body) + "]"));
                    }

                    return;
                }
            }

            Say(label + " read back through COM: no view of that name in the folder");
        }

        /// <summary>The item at that path of names, each level the first child of that name, Ordinal, from a fresh RootItem.</summary>
        private static SavedItem ResolveNames(Document document, List<string> names)
        {
            GroupItem parent = document.SavedViewpoints.RootItem;

            for (int level = 0; level < names.Count; level++)
            {
                SavedItem found = null;
                SavedItemCollection children = parent.Children;

                for (int i = 0; i < children.Count; i++)
                {
                    SavedItem child = children[i];

                    if (string.Equals(child.DisplayName, names[level], StringComparison.Ordinal))
                    {
                        found = child;
                        break;
                    }

                    child.Dispose();
                }

                parent.Dispose();

                if (found == null)
                {
                    return null;
                }

                if (level == names.Count - 1)
                {
                    return found;
                }

                parent = found as GroupItem;

                if (parent == null)
                {
                    found.Dispose();
                    return null;
                }
            }

            return null;
        }

        /// <summary>Reads one target's comments and counts, and says whether exactly one comment reads the body and author written.</summary>
        private bool ReadTarget(Document document, P9Target target, bool reopened)
        {
            try
            {
                using (SavedItem item = ResolveNames(document, target.Path))
                {
                    if (item == null)
                    {
                        Say("   " + target.Label + " NOT FOUND by its names [" + Shown(string.Join(" / ", target.Path.ToArray())) + "]");
                        return false;
                    }

                    int hidden = -2;
                    int material = -2;
                    SavedViewpoint view = item as SavedViewpoint;

                    if (view != null)
                    {
                        hidden = HiddenCount(view);
                        material = MaterialCount(view);
                    }

                    if (reopened)
                    {
                        target.HiddenReopen = hidden;
                        target.MaterialReopen = material;
                    }
                    else
                    {
                        target.HiddenNow = hidden;
                        target.MaterialNow = material;
                    }

                    CommentCollection comments = item.Comments;
                    int count = comments == null ? 0 : comments.Count;
                    Say("   " + target.Label + " a " + item.GetType().Name + ", comments " + count
                        + (view == null ? string.Empty : ", Hidden " + hidden + ", MaterialOverrides " + material));
                    bool same = false;
                    bool sameButNewline = false;

                    for (int c = 0; c < count; c++)
                    {
                        Comment comment = comments[c];
                        Say("      comment " + c + ": Author [" + Shown(comment.Author) + "] Status " + comment.Status + " Id " + comment.Id
                            + " CreationDate " + comment.CreationDate.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                        Say("         Body [" + Shown(comment.Body) + "]");

                        if (target.Body != null && string.Equals(comment.Author, P9Author, StringComparison.Ordinal))
                        {
                            if (string.Equals(comment.Body, target.Body, StringComparison.Ordinal))
                            {
                                same = true;
                            }
                            else if (string.Equals((comment.Body ?? string.Empty).Replace("\r\n", "\n"), target.Body, StringComparison.Ordinal))
                            {
                                sameButNewline = true;
                            }
                        }
                    }

                    if (target.Body == null)
                    {
                        return count == 0;
                    }

                    bool exact = same && count == 1;
                    Say("      written Body [" + Shown(target.Body) + "] Author [" + P9Author + "]");
                    Say("      " + target.Label + (exact ? " EXACTLY ONE COMMENT, BODY AND AUTHOR THE SAME, Ordinal"
                        : sameButNewline ? " the body differs from the one written only by its line break"
                        : same ? " the body and author read the same, but " + count + " comments are there"
                        : " NOT the same"));
                    return exact;
                }
            }
            catch (Exception error)
            {
                Say("   " + target.Label + " the read THREW " + error.GetType().Name + ": " + error.Message);
                return false;
            }
        }

        private static List<string> FirstViewTwoDeep(Document document)
        {
            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                SavedItemCollection top = root.Children;

                for (int i = 0; i < top.Count; i++)
                {
                    GroupItem first = top[i] as GroupItem;

                    if (first == null)
                    {
                        continue;
                    }

                    SavedItemCollection middle = first.Children;

                    for (int j = 0; j < middle.Count; j++)
                    {
                        GroupItem second = middle[j] as GroupItem;

                        if (second == null)
                        {
                            continue;
                        }

                        SavedItemCollection leaves = second.Children;

                        for (int k = 0; k < leaves.Count; k++)
                        {
                            if (leaves[k] is SavedViewpoint)
                            {
                                return new List<string> { first.DisplayName, second.DisplayName, leaves[k].DisplayName };
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static int TreeCounts(Document document, out int folders, out int comments)
        {
            folders = 0;
            comments = 0;

            try
            {
                using (GroupItem root = document.SavedViewpoints.RootItem)
                {
                    return TreeCountsUnder(root, ref folders, ref comments);
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int TreeCountsUnder(GroupItem parent, ref int folders, ref int comments)
        {
            int views = 0;
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    comments += child.Comments == null ? 0 : child.Comments.Count;
                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        folders++;
                        views += TreeCountsUnder(group, ref folders, ref comments);
                    }
                    else
                    {
                        views++;
                    }
                }
            }

            return views;
        }

        // ---------- P10 of Q114, scan.md 5z-p, does a saved item's Guid hold and resolve ----------

        private const string P10Top = "P10 probe";
        private const string P10Body = "P10 probe comment, the edit whose effect on the Guid is read";

        private sealed class P10Target
        {
            public string Label;
            public string Route;
            public bool IsFolder;
            public bool New;
            public List<string> Path = new List<string>();
            public Guid Set = Guid.Empty;
            public readonly List<string> Stages = new List<string>();
            public readonly Dictionary<string, string> GuidAt = new Dictionary<string, string>(StringComparer.Ordinal);
            public readonly Dictionary<string, bool> UniqueAt = new Dictionary<string, bool>(StringComparer.Ordinal);
            public readonly Dictionary<string, bool> ResolvedAt = new Dictionary<string, bool>(StringComparer.Ordinal);
        }

        private sealed class GuidTally
        {
            public int Items;
            public int Empty;
            public int Threw;
            public readonly Dictionary<Guid, int> Count = new Dictionary<Guid, int>();
        }

        /// <summary>
        /// P10: does a viewpoint's Guid read the same after the comment edit and after a save and a
        /// reopen, is it unique in the tree, and does ResolveGuid return the item? Read on the items
        /// P9 left in its saved copy and on new items made here by the routes the tool uses, a COM
        /// view by InwSavedViewsColl.Add and a .NET folder by FolderItem and AddCopy, and by the
        /// .NET routes with the Guid set before AddCopy and without.
        /// </summary>
        private void MeasureViewGuids(string nwf, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no save path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            List<P10Target> targets = new List<P10Target>();
            P10NewTarget(targets, "E1", "P9's COM view with P9's AddComment comment", false, false, P9Top, P9Sub, "P9 view addcomment after add");
            P10NewTarget(targets, "E2", "P9's COM view with no comment", false, false, P9Top, P9Sub, "P9 view plain");
            P10NewTarget(targets, "E3", "P9's .NET folder by FolderItem and AddCopy, one below a root folder", true, false, P9Top, P9Sub);
            P10NewTarget(targets, "E4", "P9's COM folder view", true, false, P9Top, P9ComFolder);
            P10NewTarget(targets, "E5", "P9's .NET folder at the root", true, false, P9Top);
            List<string> legacyPath = FirstViewTwoDeep(document);

            if (legacyPath != null)
            {
                P10NewTarget(targets, "E6", "an F85 viewpoint two folders deep, P9's L1", false, false, legacyPath.ToArray());
            }
            else
            {
                Say("UNKNOWN: no viewpoint two folders deep, so E6 is not read");
            }

            P10Target n1 = P10NewTarget(targets, "N1", "a .NET folder at the root by FolderItem and AddCopy, the Guid untouched, the tool's folder route", true, true, P10Top);
            P10Target n2 = P10NewTarget(targets, "N2", "a .NET folder by FolderItem and AddCopy, its Guid set before the AddCopy", true, true, P10Top, "P10 folder guid set");
            P10Target n3 = P10NewTarget(targets, "N3", "a COM view by InwSavedViewsColl.Add into a folder, the tool's view route", false, true, P10Top, "P10 com view");
            P10Target n4 = P10NewTarget(targets, "N4", "a .NET SavedViewpoint(Viewpoint) by AddCopy, the Guid untouched", false, true, P10Top, "P10 net view");
            P10Target n5 = P10NewTarget(targets, "N5", "a .NET SavedViewpoint(Viewpoint) by AddCopy, its Guid set before the AddCopy", false, true, P10Top, "P10 net view guid set");

            Say(string.Empty);
            Say("STAGE open, P9's items as the saved copy gives them:");
            GuidTally atOpen = P10Tree(document, "at the open");

            foreach (P10Target t in targets)
            {
                if (!t.New)
                {
                    P10Read(document, t, "open", atOpen, true);
                }
            }

            try
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                using (SavedItem empty = document.SavedViewpoints.ResolveGuid(Guid.Empty))
                {
                    Say("ResolveGuid(the empty Guid) returned after " + Seconds(clock) + ": "
                        + (empty == null ? "null" : "a " + empty.GetType().Name + " [" + Shown(empty.DisplayName) + "] at " + P10IndexPath(document, empty)));
                }
            }
            catch (Exception error)
            {
                Say("ResolveGuid(the empty Guid) THREW " + error.GetType().Name + ": " + error.Message);
            }

            Say(string.Empty);
            Say("THE NEW ITEMS, each read back right after its add, re-found by its names from a fresh RootItem:");

            try
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                using (GroupItem root = document.SavedViewpoints.RootItem)
                using (FolderItem folder = new FolderItem())
                {
                    folder.DisplayName = P10Top;
                    Say("N1 the FolderItem's Guid before the AddCopy " + folder.Guid);
                    document.SavedViewpoints.AddCopy(root, folder);
                }

                Say("N1 AddCopy at the root RETURNED after " + Seconds(clock));
            }
            catch (Exception error)
            {
                Say("N1 THREW " + error.GetType().Name + ": " + error.Message);
            }

            P10Read(document, n1, "add", null, false);

            try
            {
                using (GroupItem top = (GroupItem)ResolveNames(document, new List<string> { P10Top }))
                using (FolderItem folder = new FolderItem())
                {
                    folder.DisplayName = n2.Path[1];
                    n2.Set = Guid.NewGuid();
                    folder.Guid = n2.Set;
                    Say("N2 the FolderItem's Guid set to " + n2.Set + ", read back off it before the AddCopy " + folder.Guid);
                    clock = System.Diagnostics.Stopwatch.StartNew();
                    document.SavedViewpoints.AddCopy(top, folder);
                    Say("N2 AddCopy RETURNED after " + Seconds(clock));
                }
            }
            catch (Exception error)
            {
                Say("N2 THREW " + error.GetType().Name + ": " + error.Message);
            }

            P10Read(document, n2, "add", null, false);

            using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
            {
                try
                {
                    InwOpState10 state = ComApiBridge.State;
                    InwOpFolderView comTop = FindComFolderAt(state, P10Top);
                    Say("N3 the COM folder \"" + P10Top + "\" found " + (comTop != null));
                    InwOpView view = NewComView(state, n3.Path[1], camera);
                    clock = System.Diagnostics.Stopwatch.StartNew();
                    comTop.SavedViews().Add(view);
                    Say("N3 InwSavedViewsColl.Add RETURNED after " + Seconds(clock));
                }
                catch (Exception error)
                {
                    Say("N3 THREW " + error.GetType().Name + ": " + error.Message);
                }

                P10Read(document, n3, "add", null, false);

                foreach (P10Target net in new[] { n4, n5 })
                {
                    try
                    {
                        using (GroupItem top = (GroupItem)ResolveNames(document, new List<string> { P10Top }))
                        using (SavedViewpoint view = new SavedViewpoint(camera))
                        {
                            view.DisplayName = net.Path[1];
                            Say(net.Label + " the SavedViewpoint's Guid before anything is set " + view.Guid);

                            if (net == n5)
                            {
                                n5.Set = Guid.NewGuid();
                                view.Guid = n5.Set;
                                Say("N5 its Guid set to " + n5.Set + ", read back off it before the AddCopy " + view.Guid);
                            }

                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddCopy(top, view);
                            Say(net.Label + " AddCopy RETURNED after " + Seconds(clock));
                        }
                    }
                    catch (Exception error)
                    {
                        Say(net.Label + " THREW " + error.GetType().Name + ": " + error.Message);
                    }

                    P10Read(document, net, "add", null, false);
                }
            }

            Say(string.Empty);
            Say("THE COMMENT EDIT, AddComment with a comment from CreateCommentWithUniqueId on every item, the Guid read just before and just after:");

            foreach (P10Target t in targets)
            {
                try
                {
                    using (SavedItem item = ResolveNames(document, t.Path))
                    {
                        if (item == null)
                        {
                            Say("   " + t.Label + " NOT FOUND by its names, so no comment is written");
                            continue;
                        }

                        Guid before = item.Guid;

                        using (Comment comment = document.CreateCommentWithUniqueId(P10Body, CommentStatus.New, P9Author))
                        {
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddComment(item, comment);
                            Say("   " + t.Label + " AddComment RETURNED after " + Seconds(clock) + ", the Guid just before " + before);
                        }
                    }
                }
                catch (Exception error)
                {
                    Say("   " + t.Label + " AddComment THREW " + error.GetType().Name + ": " + error.Message);
                }

                P10Read(document, t, "edit", null, false);
            }

            Say(string.Empty);
            Say("STAGE before the save, the whole tree and every item, with ResolveGuid:");
            GuidTally beforeSave = P10Tree(document, "before the save");

            foreach (P10Target t in targets)
            {
                P10Read(document, t, "save", beforeSave, true);
            }

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (!reopened)
            {
                Say("P10 UNKNOWN   the saved file would not reopen, so nothing after a reopen is read");
                return;
            }

            Say(string.Empty);
            Say("STAGE after a save, a clear and a reopen, the whole tree and every item, with ResolveGuid:");
            GuidTally reopenTally = P10Tree(document, "after the reopen");

            foreach (P10Target t in targets)
            {
                P10Read(document, t, "reopen", reopenTally, true);
            }

            Say(string.Empty);
            Say("EACH TARGET:  label | route | the Guid at each stage | the same at every stage | not empty | unique in the tree before the save, after the reopen | ResolveGuid gave the item before the save, after the reopen | the set Guid kept | all of these");
            bool toolView = false;
            bool toolFolder = false;
            bool anyKept = false;

            foreach (P10Target t in targets)
            {
                List<string> shown = new List<string>();
                HashSet<string> distinct = new HashSet<string>(StringComparer.Ordinal);

                foreach (string stage in t.Stages)
                {
                    string g;
                    t.GuidAt.TryGetValue(stage, out g);
                    shown.Add(stage + " " + (g ?? "UNKNOWN"));
                    distinct.Add(g ?? "UNKNOWN");
                }

                bool same = t.Stages.Count > 0 && distinct.Count == 1 && !distinct.Contains("UNKNOWN") && !distinct.Contains("NOT FOUND") && !distinct.Contains("THREW");
                bool notEmpty = same && !distinct.Contains(Guid.Empty.ToString());
                bool uniqueSave = P10Flag(t.UniqueAt, "save");
                bool uniqueReopen = P10Flag(t.UniqueAt, "reopen");
                bool resolvedSave = P10Flag(t.ResolvedAt, "save");
                bool resolvedReopen = P10Flag(t.ResolvedAt, "reopen");
                string setKept = t.Set == Guid.Empty ? "not set" : Yes(same && distinct.Contains(t.Set.ToString()));
                bool all = same && notEmpty && uniqueSave && uniqueReopen && resolvedSave && resolvedReopen;

                Say("   " + t.Label + " | " + t.Route + " | " + string.Join(", ", shown.ToArray()) + " | " + Yes(same) + " | " + Yes(notEmpty)
                    + " | " + Yes(uniqueSave) + ", " + Yes(uniqueReopen) + " | " + Yes(resolvedSave) + ", " + Yes(resolvedReopen) + " | " + setKept
                    + " | " + Yes(all));

                if (t == n3)
                {
                    toolView = all;
                }

                if (t == n1)
                {
                    toolFolder = all;
                }

                if (all)
                {
                    anyKept = true;
                }
            }

            Say("P10 by route: the tool's view route, a COM view, " + Yes(toolView) + ". The tool's folder route, a .NET folder by AddCopy, " + Yes(toolFolder)
                + ". Any item of any route " + Yes(anyKept));
            Say("P10 " + (toolView && toolFolder ? "YES" : "NO")
                + "   a Guid that is not empty, the same after the comment edit and after a save, a clear and a reopen, unique in the tree, and resolved by ResolveGuid to the item, on the tool's view and folder routes");
        }

        private static bool P10Flag(Dictionary<string, bool> flags, string stage)
        {
            bool value;
            return flags.TryGetValue(stage, out value) && value;
        }

        private static P10Target P10NewTarget(List<P10Target> into, string label, string route, bool isFolder, bool isNew, params string[] path)
        {
            P10Target target = new P10Target();
            target.Label = label;
            target.Route = route;
            target.IsFolder = isFolder;
            target.New = isNew;
            target.Path.AddRange(path);
            into.Add(target);
            return target;
        }

        private static string P10IndexPath(Document document, SavedItem item)
        {
            try
            {
                System.Collections.ObjectModel.Collection<int> path = document.SavedViewpoints.CreateIndexPath(item);

                if (path == null)
                {
                    return "null";
                }

                List<string> parts = new List<string>();

                foreach (int i in path)
                {
                    parts.Add(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

                return string.Join(".", parts.ToArray());
            }
            catch (Exception error)
            {
                return "THREW " + error.GetType().Name;
            }
        }

        /// <summary>Reads one target's Guid and index path at a stage, and where a tally is given, its uniqueness, and where asked, ResolveGuid.</summary>
        private void P10Read(Document document, P10Target t, string stage, GuidTally tally, bool resolve)
        {
            t.Stages.Add(stage);

            try
            {
                using (SavedItem item = ResolveNames(document, t.Path))
                {
                    if (item == null)
                    {
                        t.GuidAt[stage] = "NOT FOUND";
                        Say("   " + t.Label + " at " + stage + ": NOT FOUND by its names [" + Shown(string.Join(" / ", t.Path.ToArray())) + "]");
                        return;
                    }

                    Guid guid = item.Guid;
                    string at = P10IndexPath(document, item);
                    string reference;

                    try
                    {
                        using (SavedItemReference r = document.SavedViewpoints.CreateReference(item))
                        {
                            reference = r == null ? "null" : "SavedItemId [" + Shown(r.SavedItemId) + "]";
                        }
                    }
                    catch (Exception error)
                    {
                        reference = "THREW " + error.GetType().Name;
                    }

                    t.GuidAt[stage] = guid.ToString();
                    string line = "   " + t.Label + " at " + stage + ": a " + item.GetType().Name + ", Guid " + guid + ", index path " + at
                        + ", comments " + (item.Comments == null ? 0 : item.Comments.Count) + ", CreateReference " + reference;

                    if (tally != null)
                    {
                        int count;
                        tally.Count.TryGetValue(guid, out count);
                        t.UniqueAt[stage] = guid != Guid.Empty && count == 1;
                        line += ", items in the tree with this Guid " + count;
                    }

                    if (resolve)
                    {
                        System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

                        try
                        {
                            using (SavedItem found = document.SavedViewpoints.ResolveGuid(guid))
                            {
                                string seconds = Seconds(clock);

                                if (found == null)
                                {
                                    t.ResolvedAt[stage] = false;
                                    line += ", ResolveGuid null after " + seconds;
                                }
                                else
                                {
                                    string foundAt = P10IndexPath(document, found);
                                    bool same = string.Equals(found.DisplayName, item.DisplayName, StringComparison.Ordinal) && string.Equals(foundAt, at, StringComparison.Ordinal);
                                    t.ResolvedAt[stage] = same && guid != Guid.Empty;
                                    line += ", ResolveGuid after " + seconds + " gave a " + found.GetType().Name + " [" + Shown(found.DisplayName) + "] at " + foundAt
                                        + (same ? ", THE SAME ITEM" : ", ANOTHER ITEM");
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            t.ResolvedAt[stage] = false;
                            line += ", ResolveGuid THREW " + error.GetType().Name + ": " + error.Message;
                        }
                    }

                    Say(line);
                }
            }
            catch (Exception error)
            {
                t.GuidAt[stage] = "THREW";
                Say("   " + t.Label + " at " + stage + ": the read THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>Every item's Guid in the saved viewpoint tree, counted.</summary>
        private GuidTally P10Tree(Document document, string when)
        {
            GuidTally tally = new GuidTally();
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                P10TreeUnder(root, tally);
            }

            int shared = 0;
            int itemsShared = 0;

            foreach (KeyValuePair<Guid, int> pair in tally.Count)
            {
                if (pair.Key != Guid.Empty && pair.Value > 1)
                {
                    shared++;
                    itemsShared += pair.Value;
                }
            }

            int distinct = tally.Count.Count - (tally.Count.ContainsKey(Guid.Empty) ? 1 : 0);
            Say("the tree " + when + ": items " + tally.Items + ", Guid reads that threw " + tally.Threw + ", empty Guids " + tally.Empty
                + ", distinct Guids that are not empty " + distinct + ", Guids that are not empty carried by more than one item " + shared
                + " over " + itemsShared + " items, the walk " + Seconds(clock));
            return tally;
        }

        private static void P10TreeUnder(GroupItem parent, GuidTally tally)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    tally.Items++;

                    try
                    {
                        Guid guid = child.Guid;
                        int count;
                        tally.Count.TryGetValue(guid, out count);
                        tally.Count[guid] = count + 1;

                        if (guid == Guid.Empty)
                        {
                            tally.Empty++;
                        }
                    }
                    catch (Exception)
                    {
                        tally.Threw++;
                    }

                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        P10TreeUnder(group, tally);
                    }
                }
            }
        }

        // ---------- P11 of Q114, scan.md 5z-q, does AddCopy of a marked view give a new Guid, and does its comment travel ----------

        private const string P11Top = "P11 probe";
        private const string P11Sources = "P11 sources";
        private const string P11SourceName = "P11 source guid set";
        private const string P11Body = "P11 probe mark, written on the source whose Guid the probe set";

        private sealed class P11State
        {
            public string Type = "NOT FOUND";
            public string Name = string.Empty;
            public string Guid = "UNKNOWN";
            public int Siblings = -1;
            public int InTree = -1;
            public int Hidden = -2;
            public int Material = -2;
            public readonly List<string> Comments = new List<string>();
            public readonly List<string> CommentIds = new List<string>();
        }

        private sealed class P11Item
        {
            public string Label;
            public string Route;
            public P11Item Source;
            public List<string> Path;
            public List<string> Folder;
            public readonly Dictionary<string, P11State> At = new Dictionary<string, P11State>(StringComparer.Ordinal);
        }

        /// <summary>
        /// P11: AddCopy of a view that carries the tool's mark, a comment written by AddComment. Does
        /// the copy get a new Guid, and does the comment travel with it? Read on P9's COM view with
        /// its AddComment comment, on F85's view P9 marked, and on a marked view whose Guid the probe
        /// set, each copied by AddCopy of the document's item itself, of SavedItem.CreateCopy and of
        /// SavedItem.CreateUniqueCopy, each copy into a folder of its own. Read right after the add,
        /// before the save, and after a SaveFile, a Document.Clear and a TryOpenFile of the saved file.
        /// </summary>
        private void MeasureViewCopy(string nwf, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no save path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            int folders0;
            int comments0;
            int views0 = TreeCounts(document, out folders0, out comments0);
            Say("the tree at the open: viewpoints " + views0 + ", folders " + folders0 + ", comments on any item " + comments0);
            GuidTally atOpen = P10Tree(document, "at the open");

            List<P11Item> sources = new List<P11Item>();
            P11Item s1 = P11Source(sources, "S1", "P9's COM view, the tool's view route, marked by AddComment after the add", P9Top, P9Sub, "P9 view addcomment after add");
            List<string> legacyPath = FirstViewTwoDeep(document);
            P11Item s2 = null;

            if (legacyPath != null)
            {
                s2 = P11Source(sources, "S2", "an F85 view two folders deep, marked by P9's AddComment", legacyPath.ToArray());
            }
            else
            {
                Say("UNKNOWN: no viewpoint two folders deep, so S2 is not read");
            }

            P11Item s3 = new P11Item();
            s3.Label = "S3";
            s3.Route = "a copy of S1 by CreateCopy, its Guid set to Guid.NewGuid() and its name changed before AddCopy into a folder, marked";
            s3.Folder = new List<string> { P11Top, P11Sources };
            sources.Add(s3);

            Say(string.Empty);
            Say("STAGE open, the sources as P9's saved copy gives them:");

            foreach (P11Item s in sources)
            {
                if (s != s3)
                {
                    P11Read(document, s, "open", atOpen);
                }
            }

            Say(string.Empty);
            Say("THE FOLDERS, each by FolderItem and AddCopy, one for the source S3 and one for each copy:");
            List<P11Item> copies = new List<P11Item>();
            P11Item c1 = P11Copy(copies, "C1", s1, "direct", "AddCopy(folder, S1 itself)");
            P11Copy(copies, "C2", s1, "copy", "AddCopy(folder, S1.CreateCopy())");
            P11Copy(copies, "C3", s1, "unique", "AddCopy(folder, S1.CreateUniqueCopy())");

            if (s2 != null)
            {
                P11Copy(copies, "C4", s2, "direct", "AddCopy(folder, S2 itself)");
            }

            P11Copy(copies, "C5", s3, "direct", "AddCopy(folder, S3 itself)");
            P11Copy(copies, "C6", s3, "copy", "AddCopy(folder, S3.CreateCopy())");
            P11Copy(copies, "C7", s3, "unique", "AddCopy(folder, S3.CreateUniqueCopy())");

            try
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                EnsureFolder(document, P11Top);
                List<string> names = new List<string> { P11Sources };

                foreach (P11Item c in copies)
                {
                    names.Add(c.Label);
                }

                foreach (string name in names)
                {
                    using (GroupItem top = (GroupItem)ResolveNames(document, new List<string> { P11Top }))
                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = name;
                        document.SavedViewpoints.AddCopy(top, folder);
                    }
                }

                Say("\"" + P11Top + "\" at the root and " + names.Count + " folders under it made in " + Seconds(clock) + ": " + string.Join(", ", names.ToArray()));
            }
            catch (Exception error)
            {
                Say("the folders THREW " + error.GetType().Name + ": " + error.Message);
                Say("P11 UNKNOWN   the folders could not be made, so nothing is copied");
                return;
            }

            Say(string.Empty);
            Say("THE SOURCE S3, a copy of S1 with a Guid the probe set:");
            Guid s3Guid = Guid.NewGuid();

            try
            {
                using (SavedItem from = P11Resolve(document, s1))
                using (GroupItem into = (GroupItem)ResolveNames(document, s3.Folder))
                {
                    if (from == null || into == null)
                    {
                        Say("S3 NOT MADE, S1 found " + (from != null) + ", the folder found " + (into != null));
                    }
                    else
                    {
                        using (SavedItem made = from.CreateCopy())
                        {
                            Say("S3 S1.CreateCopy() gave a " + made.GetType().Name + ", before anything is set: Guid " + made.Guid + ", name [" + Shown(made.DisplayName)
                                + "], comments " + (made.Comments == null ? 0 : made.Comments.Count));
                            made.DisplayName = P11SourceName;
                            made.Guid = s3Guid;
                            Say("S3 its name set to [" + P11SourceName + "] and its Guid set to " + s3Guid + ", read back off it before the AddCopy " + made.Guid);
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddCopy(into, made);
                            Say("S3 AddCopy RETURNED after " + Seconds(clock));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Say("S3 THREW " + error.GetType().Name + ": " + error.Message);
            }

            P11Read(document, s3, "add", null);
            P11State s3Added;

            if (s3.At.TryGetValue("add", out s3Added) && s3Added.Type != "NOT FOUND" && s3Added.Comments.Count == 0)
            {
                try
                {
                    using (SavedItem item = P11Resolve(document, s3))
                    using (Comment comment = document.CreateCommentWithUniqueId(P11Body, CommentStatus.New, P9Author))
                    {
                        clock = System.Diagnostics.Stopwatch.StartNew();
                        document.SavedViewpoints.AddComment(item, comment);
                        Say("S3 carried no comment after its AddCopy, so it is marked here: AddComment RETURNED after " + Seconds(clock));
                    }
                }
                catch (Exception error)
                {
                    Say("S3 AddComment THREW " + error.GetType().Name + ": " + error.Message);
                }

                P11Read(document, s3, "add", null);
            }

            Say(string.Empty);
            Say("THE COPIES, each read back right after its add, as the one child of its own folder:");

            foreach (P11Item c in copies)
            {
                P11MakeCopy(document, c);
                P11Read(document, c, "add", null);
            }

            Say(string.Empty);
            Say("STAGE before the save, the whole tree, every source and every copy:");
            GuidTally beforeSave = P10Tree(document, "before the save");

            foreach (P11Item s in sources)
            {
                P11Read(document, s, "save", beforeSave);
            }

            foreach (P11Item c in copies)
            {
                P11Read(document, c, "save", beforeSave);
            }

            P11ResolveSet(document, s3Guid, "before the save");

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (reopened)
            {
                Say(string.Empty);
                Say("STAGE after a save, a clear and a reopen, the whole tree, every source and every copy:");
                GuidTally reopenTally = P10Tree(document, "after the reopen");

                foreach (P11Item s in sources)
                {
                    P11Read(document, s, "reopen", reopenTally);
                }

                foreach (P11Item c in copies)
                {
                    P11Read(document, c, "reopen", reopenTally);
                }

                P11ResolveSet(document, s3Guid, "after the reopen");
                int folders1;
                int comments1;
                int views1 = TreeCounts(document, out folders1, out comments1);
                Say("the tree after the reopen: viewpoints " + views1 + ", folders " + folders1 + ", comments on any item " + comments1);
            }
            else
            {
                Say("UNKNOWN: the saved file would not reopen, so nothing after a reopen is read");
            }

            Say(string.Empty);
            Say("EACH COPY:  label | route | stage | the copy's Guid | the source's Guid | the Guid is | the comments the same as the source's, Body, Author and Status, Ordinal | the comment Ids and dates the same | Hidden and MaterialOverrides the same | its name the same");
            int travelled = 0;
            int notTravelled = 0;
            Dictionary<string, int> guidKinds = new Dictionary<string, int>(StringComparer.Ordinal);
            string c1Comment = "UNKNOWN";
            string c1Guid = "UNKNOWN";

            foreach (P11Item c in copies)
            {
                bool allTravel = true;
                HashSet<string> kinds = new HashSet<string>(StringComparer.Ordinal);

                foreach (string stage in new[] { "add", "save", "reopen" })
                {
                    P11State mine;

                    if (!c.At.TryGetValue(stage, out mine))
                    {
                        Say("   " + c.Label + " | " + stage + " | not read");
                        allTravel = false;
                        kinds.Add("UNKNOWN");
                        continue;
                    }

                    P11State theirs = P11SourceAt(c.Source, stage);

                    if (mine.Type == "NOT FOUND" || theirs == null || theirs.Type == "NOT FOUND")
                    {
                        Say("   " + c.Label + " | " + stage + " | the copy found " + Yes(mine.Type != "NOT FOUND") + ", the source found " + Yes(theirs != null && theirs.Type != "NOT FOUND"));
                        allTravel = false;
                        kinds.Add("UNKNOWN");
                        continue;
                    }

                    string kind = mine.Guid == Guid.Empty.ToString()
                        ? (theirs.Guid == Guid.Empty.ToString() ? "empty, as the source's" : "empty, the source's is not")
                        : mine.Guid == theirs.Guid ? "KEPT, the source's" : "NEW";
                    bool sameComments = mine.Comments.Count > 0 && P11SameList(mine.Comments, theirs.Comments);
                    bool sameIds = mine.CommentIds.Count > 0 && P11SameList(mine.CommentIds, theirs.CommentIds);
                    bool sameCounts = mine.Hidden == theirs.Hidden && mine.Material == theirs.Material && mine.Hidden >= 0 && mine.Material >= 0;
                    bool sameName = string.Equals(mine.Name, theirs.Name, StringComparison.Ordinal);
                    kinds.Add(kind);

                    if (!sameComments)
                    {
                        allTravel = false;
                    }

                    Say("   " + c.Label + " | " + c.Route + " | " + stage + " | " + mine.Guid + " | " + theirs.Guid + " | " + kind
                        + " | " + Yes(sameComments) + " (" + mine.Comments.Count + " and " + theirs.Comments.Count + ")"
                        + " | " + Yes(sameIds) + " | " + Yes(sameCounts) + " (" + mine.Hidden + ", " + mine.Material + " and " + theirs.Hidden + ", " + theirs.Material + ")"
                        + " | " + Yes(sameName));
                }

                string kindText = string.Join(" then ", new List<string>(kinds).ToArray());
                int seen;
                guidKinds.TryGetValue(kindText, out seen);
                guidKinds[kindText] = seen + 1;

                if (allTravel)
                {
                    travelled++;
                }
                else
                {
                    notTravelled++;
                }

                if (c == c1)
                {
                    c1Comment = Yes(allTravel);
                    c1Guid = kindText;
                }

                Say("   " + c.Label + " in all: the comment travelled at every stage " + Yes(allTravel) + ", the Guid " + kindText);
            }

            List<string> kindLines = new List<string>();

            foreach (KeyValuePair<string, int> pair in guidKinds)
            {
                kindLines.Add(pair.Value + " " + pair.Key);
            }

            Say("P11 on the tool's marked view, C1, AddCopy of S1 itself: the comment travels " + c1Comment + ", the Guid " + c1Guid);
            Say("P11 over all " + copies.Count + " copies: the comment travelled on " + travelled + " and not on " + notTravelled + ". The Guid: " + string.Join(", ", kindLines.ToArray()));
        }

        private static P11Item P11Source(List<P11Item> into, string label, string route, params string[] path)
        {
            P11Item item = new P11Item();
            item.Label = label;
            item.Route = route;
            item.Path = new List<string>(path);
            into.Add(item);
            return item;
        }

        private static P11Item P11Copy(List<P11Item> into, string label, P11Item source, string how, string route)
        {
            P11Item item = new P11Item();
            item.Label = label;
            item.Route = route;
            item.Source = source;
            item.Path = null;
            item.Folder = new List<string> { P11Top, label };
            item.At["how"] = new P11State { Type = how };
            into.Add(item);
            return item;
        }

        private static P11State P11SourceAt(P11Item source, string stage)
        {
            P11State state;

            foreach (string s in new[] { stage, "add", "open" })
            {
                if (source.At.TryGetValue(s, out state))
                {
                    return state;
                }
            }

            return null;
        }

        private static bool P11SameList(List<string> a, List<string> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The item by its path of names, or the first child of its folder, the folder's child count given out.</summary>
        private static SavedItem P11Resolve(Document document, P11Item item, out int siblings)
        {
            siblings = -1;

            if (item.Path != null)
            {
                return ResolveNames(document, item.Path);
            }

            using (SavedItem found = ResolveNames(document, item.Folder))
            {
                GroupItem folder = found as GroupItem;

                if (folder == null)
                {
                    return null;
                }

                SavedItemCollection children = folder.Children;
                siblings = children.Count;
                return siblings > 0 ? children[0] : null;
            }
        }

        private static SavedItem P11Resolve(Document document, P11Item item)
        {
            int siblings;
            return P11Resolve(document, item, out siblings);
        }

        private void P11MakeCopy(Document document, P11Item copy)
        {
            string how = copy.At["how"].Type;

            try
            {
                using (SavedItem source = P11Resolve(document, copy.Source))
                using (GroupItem folder = (GroupItem)ResolveNames(document, copy.Folder))
                {
                    if (source == null || folder == null)
                    {
                        Say(copy.Label + " NOT MADE, the source found " + (source != null) + ", its folder found " + (folder != null));
                        return;
                    }

                    System.Diagnostics.Stopwatch clock;

                    if (how == "direct")
                    {
                        clock = System.Diagnostics.Stopwatch.StartNew();
                        document.SavedViewpoints.AddCopy(folder, source);
                        Say(copy.Label + " " + copy.Route + " RETURNED after " + Seconds(clock));
                        return;
                    }

                    using (SavedItem made = how == "unique" ? source.CreateUniqueCopy() : source.CreateCopy())
                    {
                        Say(copy.Label + " " + (how == "unique" ? "CreateUniqueCopy" : "CreateCopy") + " of " + copy.Source.Label + " gave a " + made.GetType().Name
                            + ", before the AddCopy: Guid " + made.Guid + ", the source's " + source.Guid + ", name [" + Shown(made.DisplayName)
                            + "], comments " + (made.Comments == null ? 0 : made.Comments.Count));
                        clock = System.Diagnostics.Stopwatch.StartNew();
                        document.SavedViewpoints.AddCopy(folder, made);
                        Say(copy.Label + " " + copy.Route + " RETURNED after " + Seconds(clock));
                    }
                }
            }
            catch (Exception error)
            {
                Say(copy.Label + " " + copy.Route + " THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>Reads one item at a stage: its type, name, Guid, comments, and for a view its Hidden and MaterialOverrides counts.</summary>
        private void P11Read(Document document, P11Item target, string stage, GuidTally tally)
        {
            P11State state = new P11State();
            target.At[stage] = state;

            try
            {
                int siblings;

                using (SavedItem item = P11Resolve(document, target, out siblings))
                {
                    state.Siblings = siblings;

                    if (item == null)
                    {
                        Say("   " + target.Label + " at " + stage + ": NOT FOUND" + (siblings >= 0 ? ", its folder holds " + siblings : string.Empty));
                        return;
                    }

                    Guid guid = item.Guid;
                    state.Type = item.GetType().Name;
                    state.Name = item.DisplayName ?? string.Empty;
                    state.Guid = guid.ToString();
                    SavedViewpoint view = item as SavedViewpoint;

                    if (view != null)
                    {
                        state.Hidden = HiddenCount(view);
                        state.Material = MaterialCount(view);
                    }

                    CommentCollection comments = item.Comments;
                    int count = comments == null ? 0 : comments.Count;

                    for (int c = 0; c < count; c++)
                    {
                        Comment comment = comments[c];
                        state.Comments.Add("Body [" + Shown(comment.Body) + "] Author [" + Shown(comment.Author) + "] Status " + comment.Status);
                        state.CommentIds.Add("Id " + comment.Id + " CreationDate "
                            + comment.CreationDate.ToString("yyyy-MM-dd HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture));
                    }

                    string line = "   " + target.Label + " at " + stage + ": a " + state.Type + " [" + Shown(state.Name) + "], index path " + P10IndexPath(document, item)
                        + ", Guid " + guid + (siblings >= 0 ? ", its folder holds " + siblings : string.Empty)
                        + (view == null ? string.Empty : ", Hidden " + state.Hidden + ", MaterialOverrides " + state.Material)
                        + ", comments " + count;

                    if (tally != null)
                    {
                        int inTree;
                        tally.Count.TryGetValue(guid, out inTree);
                        state.InTree = inTree;
                        line += ", items in the tree with this Guid " + inTree;
                    }

                    Say(line);

                    for (int c = 0; c < count; c++)
                    {
                        Say("      comment " + c + ": " + state.Comments[c] + " " + state.CommentIds[c]);
                    }
                }
            }
            catch (Exception error)
            {
                state.Type = "NOT FOUND";
                Say("   " + target.Label + " at " + stage + ": the read THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        private void P11ResolveSet(Document document, Guid set, string when)
        {
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (SavedItem found = document.SavedViewpoints.ResolveGuid(set))
                {
                    string seconds = Seconds(clock);
                    Say("ResolveGuid of S3's set Guid " + set + " " + when + " returned after " + seconds + ": "
                        + (found == null ? "null" : "a " + found.GetType().Name + " [" + Shown(found.DisplayName) + "] at " + P10IndexPath(document, found)));
                }
            }
            catch (Exception error)
            {
                Say("ResolveGuid of S3's set Guid " + when + " THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        // ---------- P12 of Q114, does a view whose name ends in a space read back its name unchanged ----------

        private const string P12Top = "P12 probe";

        private sealed class P12Item
        {
            public string Label;
            public string Route;
            public string Written;
            public string Body;
            public bool Control;
            public readonly List<string> Plain = new List<string>();
            public readonly List<int> Steps = new List<int>();
            public readonly List<string> WrittenPath = new List<string>();
            public readonly Dictionary<string, string> Net = new Dictionary<string, string>(StringComparer.Ordinal);
            public readonly Dictionary<string, string> Com = new Dictionary<string, string>(StringComparer.Ordinal);
            public readonly Dictionary<string, bool> ByName = new Dictionary<string, bool>(StringComparer.Ordinal);
            public readonly Dictionary<string, bool> Mark = new Dictionary<string, bool>(StringComparer.Ordinal);
        }

        /// <summary>
        /// P12: a view whose name ends in a space, made by the tool's routes, reads back its
        /// DisplayName unchanged, Ordinal, right after the add, before a save and after a SaveFile,
        /// a Document.Clear and a TryOpenFile of the saved file. Each item sits at a known position
        /// in a folder of plain name, so it is found by position and never by the name being read.
        /// The COM name of the same item is read too, and whether a lookup by the written names,
        /// Ordinal, finds that same item. A mark whose body holds the name, in its middle or at its
        /// end, is read back Ordinal.
        /// </summary>
        private void MeasureViewNameSpaces(string nwf, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no save path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            int folders0;
            int comments0;
            int views0 = TreeCounts(document, out folders0, out comments0);
            Say("the tree at the open: viewpoints " + views0 + ", folders " + folders0 + ", comments on any item " + comments0);

            string stamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            List<P12Item> items = new List<P12Item>();
            P12Item v0 = P12New(items, "V0", "control, no space, COM view added into its folder's own SavedViews", "V0", 0, "P12 V0 control");
            v0.Control = true;
            P12Item v1 = P12New(items, "V1", "COM view added into its folder's own SavedViews, the tool's route, a test name's characters, marked with the name inside the body", "V1", 0, "P12-AR-XX_Alpha-vs-P12-ME-YY_Beta & Gamma ");
            P12Item v2 = P12New(items, "V2", "COM view added at the root, AddCopy into its folder, the root one removed, the tool's other route", "V2", 0, "P12 V2 root route one space ");
            P12Item v3 = P12New(items, "V3", "COM view into its folder, two spaces at the end, marked with the body ending in the name", "V3", 0, "P12 V3 two trailing spaces  ");
            P12Item v4 = P12New(items, "V4", "COM view into its folder, a space at each end", "V4", 0, " P12 V4 leading and trailing ");
            P12Item v5a = P12New(items, "V5a", "COM view into its folder, no space, the first of two names that differ by the end space", "V5", 0, "P12 V5 twin");
            v5a.Control = true;
            P12Item v5b = P12New(items, "V5b", "COM view into the same folder, the second twin, one space at the end", "V5", 1, "P12 V5 twin ");
            P12Item v6 = P12New(items, "V6", ".NET new SavedViewpoint(Viewpoint), DisplayName set, AddCopy into its folder", "V6", 0, "P12 V6 dotnet one space ");
            P12Item v7 = P12New(items, "V7", "COM view into its folder with no space, then DocumentSavedViewpoints.EditDisplayName to a name ending in a space", "V7", 0, "P12 V7 renamed ");
            P12Item f1 = P12New(items, "F1", "FolderItem, DisplayName set, AddCopy, the tool's folder route, marked with the body ending in the name", "F1", 0, "P12 F1 folder end space ");
            P12Item f1v = P12New(items, "F1v", "COM view added into the space-ended folder's own SavedViews, found by position", "F1", 0, "P12 F1 view one space ");
            f1v.Steps.Add(0);
            f1v.WrittenPath.Clear();
            f1v.WrittenPath.AddRange(new[] { P12Top, "F1", f1.Written, "P12 F1 view one space " });

            v1.Body = P9Sentence + "\n" + "[nwcfed-mark 1] stamp=" + stamp + " path=" + P12Top + "/V1 name=" + v1.Written + " probe=V1";
            v3.Body = P9Sentence + "\n" + "[nwcfed-mark 1] stamp=" + stamp + " probe=V3 path=" + P12Top + "/V3 name=" + v3.Written;
            f1.Body = P9Sentence + "\n" + "[nwcfed-mark 1] stamp=" + stamp + " probe=F1 path=" + P12Top + "/F1 name=" + f1.Written;

            Say(string.Empty);
            Say("WHAT IS WRITTEN, each name in brackets with its length and its last character:");

            foreach (P12Item it in items)
            {
                Say("   " + it.Label + " " + P12Show(it.Written) + " in " + string.Join(" / ", it.Plain.ToArray()) + " at position " + string.Join(".", P12Ints(it.Steps)) + ", " + it.Route);
            }

            try
            {
                clock = System.Diagnostics.Stopwatch.StartNew();
                EnsureFolder(document, P12Top);

                foreach (string name in new[] { "V0", "V1", "V2", "V3", "V4", "V5", "V6", "V7", "F1" })
                {
                    using (GroupItem top = (GroupItem)ResolveNames(document, new List<string> { P12Top }))
                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = name;
                        document.SavedViewpoints.AddCopy(top, folder);
                    }
                }

                Say("\"" + P12Top + "\" at the root and 9 plain folders under it made by FolderItem and AddCopy in " + Seconds(clock));
            }
            catch (Exception error)
            {
                Say("the folders THREW " + error.GetType().Name + ": " + error.Message);
                Say("P12 UNKNOWN   the folders could not be made, so nothing is written");
                return;
            }

            InwOpState10 state = ComApiBridge.State;
            Say(string.Empty);
            Say("THE WRITES, each item read right after its add:");

            using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
            {
                foreach (P12Item it in new[] { v0, v1, v3, v4, v5a, v5b })
                {
                    P12ComIntoFolder(state, it, it.Written, camera);
                    P12Read(document, state, it, "add");
                }

                P12RootRoute(document, state, v2, camera);
                P12Read(document, state, v2, "add");

                try
                {
                    using (SavedViewpoint made = new SavedViewpoint(camera))
                    {
                        made.DisplayName = v6.Written;
                        Say("V6 the new SavedViewpoint's DisplayName read back before the add " + P12Show(made.DisplayName));

                        using (GroupItem folder = (GroupItem)ResolveNames(document, v6.Plain))
                        {
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddCopy(folder, made);
                            Say("V6 AddCopy RETURNED after " + Seconds(clock));
                        }
                    }
                }
                catch (Exception error)
                {
                    Say("V6 THREW " + error.GetType().Name + ": " + error.Message);
                }

                P12Read(document, state, v6, "add");

                P12ComIntoFolder(state, v7, "P12 V7 renamed", camera);

                try
                {
                    using (SavedItem item = P12Locate(document, v7))
                    {
                        Say("V7 before the rename " + (item == null ? "NOT FOUND" : P12Show(item.DisplayName)));

                        if (item != null)
                        {
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.EditDisplayName(item, v7.Written);
                            Say("V7 EditDisplayName(item, " + P12Show(v7.Written) + ") RETURNED after " + Seconds(clock));
                        }
                    }
                }
                catch (Exception error)
                {
                    Say("V7 EditDisplayName THREW " + error.GetType().Name + ": " + error.Message);
                }

                P12Read(document, state, v7, "add");

                try
                {
                    using (FolderItem made = new FolderItem())
                    {
                        made.DisplayName = f1.Written;
                        Say("F1 the new FolderItem's DisplayName read back before the add " + P12Show(made.DisplayName));

                        using (GroupItem folder = (GroupItem)ResolveNames(document, f1.Plain))
                        {
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddCopy(folder, made);
                            Say("F1 AddCopy RETURNED after " + Seconds(clock));
                        }
                    }
                }
                catch (Exception error)
                {
                    Say("F1 THREW " + error.GetType().Name + ": " + error.Message);
                }

                P12Read(document, state, f1, "add");

                try
                {
                    InwOpFolderView holder = FindComFolderAt(state, P12Top, "F1");
                    InwOpFolderView byName = FindComFolderAt(state, P12Top, "F1", f1.Written);
                    InwOpFolderView spaced = null;

                    if (holder != null && holder.SavedViews().Count >= 1)
                    {
                        spaced = holder.SavedViews()[1] as InwOpFolderView;
                    }

                    Say("F1v the COM folder of F1 found by its written name, Ordinal " + Yes(byName != null) + ", by position " + Yes(spaced != null)
                        + (spaced == null ? string.Empty : ", its COM name " + P12Show(spaced.name)));

                    if (spaced != null)
                    {
                        InwOpView view = NewComView(state, f1v.Written, camera);
                        Say("F1v the COM view's name read back before the add " + P12Show(view.name));
                        clock = System.Diagnostics.Stopwatch.StartNew();
                        spaced.SavedViews().Add(view);
                        Say("F1v InwSavedViewsColl.Add RETURNED after " + Seconds(clock));
                    }
                }
                catch (Exception error)
                {
                    Say("F1v THREW " + error.GetType().Name + ": " + error.Message);
                }

                P12Read(document, state, f1v, "add");
            }

            Say(string.Empty);
            Say("THE MARKS, by AddComment after the add:");

            foreach (P12Item it in new[] { v1, v3, f1 })
            {
                try
                {
                    using (SavedItem item = P12Locate(document, it))
                    {
                        if (item == null)
                        {
                            Say(it.Label + " NOT FOUND by position, so no mark is written");
                            continue;
                        }

                        using (Comment comment = document.CreateCommentWithUniqueId(it.Body, CommentStatus.New, P9Author))
                        {
                            clock = System.Diagnostics.Stopwatch.StartNew();
                            document.SavedViewpoints.AddComment(item, comment);
                            Say(it.Label + " AddComment RETURNED after " + Seconds(clock) + ", the body written " + P12Show(it.Body));
                        }
                    }
                }
                catch (Exception error)
                {
                    Say(it.Label + " AddComment THREW " + error.GetType().Name + ": " + error.Message);
                }
            }

            Say(string.Empty);
            Say("STAGE before the save, every item:");

            foreach (P12Item it in items)
            {
                P12Read(document, state, it, "save");
            }

            int folders1;
            int comments1;
            int views1 = TreeCounts(document, out folders1, out comments1);
            Say("the tree before the save: viewpoints " + views1 + ", folders " + folders1 + ", comments on any item " + comments1);

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (reopened)
            {
                state = ComApiBridge.State;
                Say(string.Empty);
                Say("STAGE after a save, a clear and a reopen, every item:");

                foreach (P12Item it in items)
                {
                    P12Read(document, state, it, "reopen");
                }

                int folders2;
                int comments2;
                int views2 = TreeCounts(document, out folders2, out comments2);
                Say("the tree after the reopen: viewpoints " + views2 + ", folders " + folders2 + ", comments on any item " + comments2);
            }
            else
            {
                Say("UNKNOWN: the saved file would not reopen, so nothing after a reopen is read");
            }

            Say(string.Empty);
            Say("EACH ITEM:  label | written | stage add, save, reopen: the .NET DisplayName the same, Ordinal | the COM name the same | a lookup by the written names finds this item | the mark the same");
            int spacedSame = 0;
            int spacedNot = 0;
            int comSame = 0;
            int comNot = 0;
            int nameSame = 0;
            int nameNot = 0;
            int markSame = 0;
            int markNot = 0;
            List<string> notSame = new List<string>();

            foreach (P12Item it in items)
            {
                List<string> netCells = new List<string>();
                List<string> comCells = new List<string>();
                List<string> byCells = new List<string>();
                List<string> markCells = new List<string>();
                bool netAll = true;
                bool comAll = true;
                bool byAll = true;
                bool markAll = true;

                foreach (string stage in new[] { "add", "save", "reopen" })
                {
                    string net;
                    string com;
                    bool by;
                    bool mark;
                    bool netOk = it.Net.TryGetValue(stage, out net) && net != null && string.Equals(net, it.Written, StringComparison.Ordinal);
                    bool comOk = it.Com.TryGetValue(stage, out com) && com != null && string.Equals(com, it.Written, StringComparison.Ordinal);
                    bool byOk = it.ByName.TryGetValue(stage, out by) && by;
                    netCells.Add(Yes(netOk));
                    comCells.Add(Yes(comOk));
                    byCells.Add(Yes(byOk));
                    netAll &= netOk;
                    comAll &= comOk;
                    byAll &= byOk;

                    if (it.Body != null)
                    {
                        bool markOk = it.Mark.TryGetValue(stage, out mark) && mark;

                        if (stage != "add")
                        {
                            markCells.Add(Yes(markOk));
                            markAll &= markOk;
                        }
                    }
                }

                Say("   " + it.Label + " | " + P12Show(it.Written) + " | " + string.Join(", ", netCells.ToArray()) + " | " + string.Join(", ", comCells.ToArray())
                    + " | " + string.Join(", ", byCells.ToArray()) + " | " + (it.Body == null ? "no mark" : string.Join(", ", markCells.ToArray())));

                if (!it.Control)
                {
                    if (netAll) { spacedSame++; } else { spacedNot++; notSame.Add(it.Label); }
                    if (comAll) { comSame++; } else { comNot++; }
                    if (byAll) { nameSame++; } else { nameNot++; }
                }

                if (it.Body != null)
                {
                    if (markAll) { markSame++; } else { markNot++; }
                }
            }

            Say("(the mark is read from the save stage on, since it is written after every add)");
            Say("P12 over the " + (spacedSame + spacedNot) + " names with a space at an end: the .NET DisplayName read back unchanged at every stage on " + spacedSame
                + " and not on " + spacedNot + (notSame.Count == 0 ? string.Empty : " (" + string.Join(", ", notSame.ToArray()) + ")")
                + ". The COM name the same on " + comSame + " and not on " + comNot + ". A lookup by the written names found the item on " + nameSame + " and not on " + nameNot
                + ". The marks read back the same on " + markSame + " of " + (markSame + markNot));
            Say("P12 " + (spacedNot == 0 && spacedSame > 0 ? "YES" : "NO") + "   a view or folder whose name ends in a space reads back its DisplayName unchanged, Ordinal, after the add, before the save and after a save, a clear and a reopen, by every route tried");
        }

        private static P12Item P12New(List<P12Item> into, string label, string route, string folder, int position, string written)
        {
            P12Item item = new P12Item();
            item.Label = label;
            item.Route = route;
            item.Written = written;
            item.Plain.Add(P12Top);
            item.Plain.Add(folder);
            item.Steps.Add(position);
            item.WrittenPath.Add(P12Top);
            item.WrittenPath.Add(folder);
            item.WrittenPath.Add(written);
            into.Add(item);
            return item;
        }

        private static string[] P12Ints(List<int> values)
        {
            List<string> parts = new List<string>();

            foreach (int v in values)
            {
                parts.Add(v.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return parts.ToArray();
        }

        /// <summary>The text in brackets, its length, and the code point of its first and last characters.</summary>
        private static string P12Show(string text)
        {
            if (text == null)
            {
                return "null";
            }

            if (text.Length == 0)
            {
                return "[] length 0";
            }

            return "[" + Shown(text) + "] length " + text.Length + ", first U+" + ((int)text[0]).ToString("X4") + ", last U+" + ((int)text[text.Length - 1]).ToString("X4");
        }

        /// <summary>The item at its plain folder path, then by position, never by the name being measured.</summary>
        private static SavedItem P12Locate(Document document, P12Item it)
        {
            SavedItem current = ResolveNames(document, it.Plain);

            foreach (int position in it.Steps)
            {
                GroupItem group = current as GroupItem;

                if (group == null)
                {
                    if (current != null)
                    {
                        current.Dispose();
                    }

                    return null;
                }

                SavedItemCollection children = group.Children;
                SavedItem next = position < children.Count ? children[position] : null;
                group.Dispose();

                if (next == null)
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        /// <summary>The COM name of the item at its plain folder path, then by position, or null with why.</summary>
        private static string P12ComName(InwOpState10 state, P12Item it, out string why)
        {
            why = string.Empty;

            try
            {
                InwOpFolderView folder = FindComFolderAt(state, it.Plain.ToArray());

                if (folder == null)
                {
                    why = "the COM folder " + string.Join(" / ", it.Plain.ToArray()) + " NOT FOUND";
                    return null;
                }

                object current = folder;

                foreach (int position in it.Steps)
                {
                    InwOpFolderView f = current as InwOpFolderView;

                    if (f == null)
                    {
                        why = "a step is not a COM folder";
                        return null;
                    }

                    InwSavedViewsColl views = f.SavedViews();

                    if (position + 1 > views.Count)
                    {
                        why = "position " + position + " of " + views.Count;
                        return null;
                    }

                    current = views[position + 1];
                }

                InwOpView view = current as InwOpView;

                if (view != null)
                {
                    return view.name;
                }

                InwOpFolderView asFolder = current as InwOpFolderView;

                if (asFolder != null)
                {
                    return asFolder.name;
                }

                why = "UNKNOWN COM type";
                return null;
            }
            catch (Exception error)
            {
                why = "THREW " + error.GetType().Name + ": " + error.Message;
                return null;
            }
        }

        private void P12ComIntoFolder(InwOpState10 state, P12Item it, string name, Viewpoint camera)
        {
            try
            {
                InwOpFolderView folder = FindComFolderAt(state, it.Plain.ToArray());

                if (folder == null)
                {
                    Say(it.Label + " NOT MADE, the COM folder " + string.Join(" / ", it.Plain.ToArray()) + " not found");
                    return;
                }

                InwOpView view = NewComView(state, name, camera);
                Say(it.Label + " the COM view's name read back before the add " + P12Show(view.name));
                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                folder.SavedViews().Add(view);
                Say(it.Label + " InwSavedViewsColl.Add into " + string.Join(" / ", it.Plain.ToArray()) + " RETURNED after " + Seconds(clock));
            }
            catch (Exception error)
            {
                Say(it.Label + " THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>The tool's other route: the COM view added at the root, the last root view named exactly so copied into the folder, and the root one removed.</summary>
        private void P12RootRoute(Document document, InwOpState10 state, P12Item it, Viewpoint camera)
        {
            try
            {
                InwOpView view = NewComView(state, it.Written, camera);
                Say(it.Label + " the COM view's name read back before the add " + P12Show(view.name));
                state.SavedViews().Add(view);
                int exact = -1;
                int count;
                string lastName;

                using (GroupItem root = document.SavedViewpoints.RootItem)
                {
                    SavedItemCollection children = root.Children;
                    count = children.Count;
                    lastName = null;

                    for (int i = 0; i < count; i++)
                    {
                        using (SavedItem child = children[i])
                        {
                            if (child is SavedViewpoint && string.Equals(child.DisplayName, it.Written, StringComparison.Ordinal))
                            {
                                exact = i;
                            }

                            if (i == count - 1)
                            {
                                lastName = child.DisplayName;
                            }
                        }
                    }
                }

                Say(it.Label + " added at the root: the root holds " + count + ", its last child " + P12Show(lastName)
                    + ", the last root view named exactly as written " + (exact < 0 ? "NONE, so the tool's lookup by name would not find it, and the last child is used" : "at " + exact));
                int use = exact >= 0 ? exact : count - 1;

                using (GroupItem root = document.SavedViewpoints.RootItem)
                using (SavedItem atRoot = root.Children[use])
                using (GroupItem folder = (GroupItem)ResolveNames(document, it.Plain))
                {
                    System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
                    document.SavedViewpoints.AddCopy(folder, atRoot);
                    bool removed = document.SavedViewpoints.Remove(atRoot);
                    Say(it.Label + " AddCopy into " + string.Join(" / ", it.Plain.ToArray()) + " and Remove of the root one, Remove returned " + removed + ", both in " + Seconds(clock));
                }
            }
            catch (Exception error)
            {
                Say(it.Label + " THREW " + error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>Reads one item at a stage: its .NET DisplayName found by position, its COM name, whether the written names find it, and its mark.</summary>
        private void P12Read(Document document, InwOpState10 state, P12Item it, string stage)
        {
            string net = null;
            string at = "NOT FOUND";
            string type = string.Empty;
            int comments = -1;
            bool mark = false;
            List<string> bodies = new List<string>();

            try
            {
                using (SavedItem item = P12Locate(document, it))
                {
                    if (item != null)
                    {
                        net = item.DisplayName ?? string.Empty;
                        at = P10IndexPath(document, item);
                        type = item.GetType().Name;
                        CommentCollection cc = item.Comments;
                        comments = cc == null ? 0 : cc.Count;

                        for (int c = 0; c < comments; c++)
                        {
                            Comment comment = cc[c];
                            bodies.Add("Author [" + Shown(comment.Author) + "] Body " + P12Show(comment.Body));

                            if (it.Body != null && comments == 1 && string.Equals(comment.Body, it.Body, StringComparison.Ordinal)
                                && string.Equals(comment.Author, P9Author, StringComparison.Ordinal))
                            {
                                mark = true;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                at = "the read THREW " + error.GetType().Name + ": " + error.Message;
            }

            string why;
            string com = P12ComName(state, it, out why);
            string byWritten;
            bool byOk = false;

            try
            {
                using (SavedItem found = ResolveNames(document, it.WrittenPath))
                {
                    byWritten = found == null ? "NOT FOUND" : P10IndexPath(document, found);
                    byOk = found != null && string.Equals(byWritten, at, StringComparison.Ordinal);
                }
            }
            catch (Exception error)
            {
                byWritten = "THREW " + error.GetType().Name;
            }

            List<string> trimmed = new List<string>();

            foreach (string n in it.WrittenPath)
            {
                trimmed.Add(n.Trim());
            }

            string byTrimmed = "the same names";

            if (!P11SameList(trimmed, it.WrittenPath))
            {
                try
                {
                    using (SavedItem found = ResolveNames(document, trimmed))
                    {
                        byTrimmed = found == null ? "NOT FOUND" : P10IndexPath(document, found);
                    }
                }
                catch (Exception error)
                {
                    byTrimmed = "THREW " + error.GetType().Name;
                }
            }

            it.Net[stage] = net;
            it.Com[stage] = com;
            it.ByName[stage] = byOk;
            it.Mark[stage] = mark;
            Say("   " + it.Label + " at " + stage + ": " + (net == null ? at : "a " + type + " at " + at + ", DisplayName " + P12Show(net)
                + ", the same as written, Ordinal " + Yes(string.Equals(net, it.Written, StringComparison.Ordinal))
                + (string.Equals(net, it.Written, StringComparison.Ordinal) ? string.Empty : ", the same once both are trimmed " + Yes(string.Equals(net.Trim(), it.Written.Trim(), StringComparison.Ordinal)))));
            Say("      COM name " + (com == null ? why : P12Show(com) + ", the same as written, Ordinal " + Yes(string.Equals(com, it.Written, StringComparison.Ordinal)))
                + ". The written names, Ordinal, find " + byWritten + (byOk ? ", this item" : string.Empty) + ". The names trimmed find " + byTrimmed
                + (comments < 0 ? string.Empty : ". Comments " + comments));

            foreach (string body in bodies)
            {
                Say("      comment: " + body);
            }

            if (it.Body != null && comments >= 0)
            {
                Say("      the mark: exactly one comment, body and author the same as written, Ordinal " + Yes(mark));
            }
        }

        // ---------- P13 of Q114, does RemoveAt(parent, index) remove one viewpoint two folders deep and nothing else ----------

        private const string P13Top = "P13 probe";
        private const string P13Sub = "P13 sub";
        private const int P13Sentinels = 12;
        private const int P13SentinelRemoved = 5;
        private const int P13Series = 5;

        private sealed class P13Row
        {
            public string Index;
            public string Folder;
            public string Name;
            public string Kind;
            public string Guid;
            public int Comments;

            public string Key
            {
                get { return Folder + "\u0001" + Name + "\u0001" + Kind + "\u0001" + Guid + "\u0001" + Comments.ToString(System.Globalization.CultureInfo.InvariantCulture); }
            }

            public P13Row Copy()
            {
                P13Row row = new P13Row();
                row.Index = Index;
                row.Folder = Folder;
                row.Name = Name;
                row.Kind = Kind;
                row.Guid = Guid;
                row.Comments = Comments;
                return row;
            }
        }

        private sealed class P13Tree
        {
            public readonly List<P13Row> Rows = new List<P13Row>();
            public int Views;
            public int Folders;
            public int Other;
            public int Threw;
            public int NotEmptyGuids;
        }

        /// <summary>
        /// P13: RemoveAt(parent, index), the parent resolved fresh from RootItem by its names and the
        /// target re-found by its name in it just before, on one viewpoint two folders deep of a fresh
        /// copy of the baseline NWF. The whole tree is read before, after the call and after a
        /// SaveFile, a Document.Clear and a TryOpenFile of the saved file: every item's index path,
        /// folder names, name, kind, Guid and comment count. After the call the tree must be the tree
        /// at the open less that one row, with only the later siblings' last index one lower, and
        /// after the reopen exactly the tree after the call. The models, sets, tests, results and
        /// statuses are counted at each stage. Part B gives a folder of views whose Guids the probe
        /// set, since every Guid of the baseline reads empty, removes one from the middle and five
        /// from the end, timed each, and reads the Guids back through ResolveGuid after a second save
        /// and reopen.
        /// </summary>
        private void MeasureViewRemove(string nwf, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no save path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            Say(string.Empty);
            Say("PART A. ONE VIEWPOINT TWO FOLDERS DEEP OF THE BASELINE TREE, REMOVED BY RemoveAt(parent, index)");
            string counts0 = P13Counts(document);
            Say("the document at the open: " + counts0);
            P13Tree t0 = P13Snap(document, "at the open");

            List<string> folderNames;
            string targetName;
            int pickedIndex;
            int siblings;

            if (!P13Pick(document, out folderNames, out targetName, out pickedIndex, out siblings))
            {
                Say("P13 UNKNOWN   the tree holds no folder two deep with at least 3 children and a viewpoint in the middle, so nothing was removed");
                return;
            }

            string pickedPath = null;

            foreach (P13Row row in t0.Rows)
            {
                if (string.Equals(row.Folder, string.Join("\u0001", folderNames.ToArray()), StringComparison.Ordinal)
                    && string.Equals(row.Name, targetName, StringComparison.Ordinal))
                {
                    pickedPath = row.Index;
                    Say("the target: [" + Shown(folderNames[0]) + "] / [" + Shown(folderNames[1]) + "] / [" + Shown(targetName) + "], a " + row.Kind
                        + " at index path " + row.Index + ", child " + pickedIndex + " of " + siblings + " in its folder, Guid " + row.Guid + ", comments " + row.Comments);
                    break;
                }
            }

            string removedA;
            double secondsA;
            bool returnedA = P13Remove(document, folderNames, targetName, "A", out removedA, out secondsA);

            if (!returnedA)
            {
                Say("P13 NO   RemoveAt did not return on the target, so nothing more is read");
                return;
            }

            Say("the index path removed " + removedA + ", the same as the one read off the tree at the open " + Yes(string.Equals(removedA, pickedPath, StringComparison.Ordinal)));
            P13Tree t1 = P13Snap(document, "after the removal");
            string counts1 = P13Counts(document);
            Say("the document after the removal: " + counts1);
            int shifted1;
            List<P13Row> expected1 = P13Apply(t0.Rows, removedA, out shifted1);
            Say("expected after the removal: the tree at the open less 1 row, " + expected1.Count + " rows, " + shifted1 + " of them later siblings whose last index falls by one");
            int mismatch1 = P13Compare(expected1, t1.Rows, "after the removal against the open less the one");
            bool gone1 = P13Gone(document, folderNames, targetName, "after the removal");

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (!reopened)
            {
                Say("P13 UNKNOWN   the saved file would not reopen, so nothing after a reopen is read");
                return;
            }

            P13Tree t2 = P13Snap(document, "after the save, the clear and the reopen");
            string counts2 = P13Counts(document);
            Say("the document after the reopen: " + counts2);
            int mismatch2 = P13Compare(t1.Rows, t2.Rows, "after the reopen against after the removal");
            bool gone2 = P13Gone(document, folderNames, targetName, "after the reopen");

            bool viewsOne = t1.Views == t0.Views - 1 && t2.Views == t1.Views;
            bool rowsOne = t1.Rows.Count == t0.Rows.Count - 1 && t2.Rows.Count == t1.Rows.Count;
            bool countsSame = string.Equals(counts0, counts1, StringComparison.Ordinal) && string.Equals(counts1, counts2, StringComparison.Ordinal);
            Say(string.Empty);
            Say("PART A SUMMARY: viewpoints " + t0.Views + ", " + t1.Views + ", " + t2.Views + " at the open, after the removal, after the reopen. Items " + t0.Rows.Count + ", " + t1.Rows.Count + ", " + t2.Rows.Count
                + ". Fell by exactly one and stayed " + Yes(viewsOne && rowsOne)
                + ". Every other item the same folder names, name, kind, Guid and comment count, in the same order, after the removal " + Yes(mismatch1 == 0) + " and after the reopen " + Yes(mismatch2 == 0)
                + ". Only the " + shifted1 + " later siblings moved, by one " + Yes(mismatch1 == 0)
                + ". The target found by its names after the removal " + Yes(!gone1) + " and after the reopen " + Yes(!gone2)
                + ". Models, sets, tests, results and statuses the same at every stage " + Yes(countsSame)
                + ". Guids not empty in the tree at the open " + t0.NotEmptyGuids + ". RemoveAt took " + secondsA.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s in a tree of " + t0.Views + " viewpoints");
            bool yesA = viewsOne && rowsOne && mismatch1 == 0 && mismatch2 == 0 && gone1 && gone2 && countsSame;
            Say("P13 " + (yesA ? "YES" : "NO") + "   RemoveAt(parent, index), the parent resolved fresh, removed one viewpoint two folders deep, the count fell by exactly one and every other item kept its path, name and Guid through a save and a reopen");

            Say(string.Empty);
            Say("PART B. A FOLDER OF " + P13Sentinels + " VIEWS WHOSE GUIDS THE PROBE SET, TWO FOLDERS DEEP, IN THE REOPENED DOCUMENT, ONE REMOVED FROM THE MIDDLE AND " + P13Series + " FROM THE END");
            Dictionary<string, Guid> sentinels = new Dictionary<string, Guid>(StringComparer.Ordinal);
            List<string> subNames = new List<string> { P13Top, P13Sub };

            try
            {
                using (GroupItem root = document.SavedViewpoints.RootItem)
                using (FolderItem folder = new FolderItem())
                {
                    folder.DisplayName = P13Top;
                    folder.Guid = Guid.NewGuid();
                    sentinels[P13Top] = folder.Guid;
                    document.SavedViewpoints.AddCopy(root, folder);
                }

                using (GroupItem top = (GroupItem)ResolveNames(document, new List<string> { P13Top }))
                using (FolderItem folder = new FolderItem())
                {
                    folder.DisplayName = P13Sub;
                    folder.Guid = Guid.NewGuid();
                    sentinels[P13Sub] = folder.Guid;
                    document.SavedViewpoints.AddCopy(top, folder);
                }

                using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
                {
                    for (int k = 0; k < P13Sentinels; k++)
                    {
                        using (GroupItem sub = (GroupItem)ResolveNames(document, subNames))
                        using (SavedViewpoint view = new SavedViewpoint(camera))
                        {
                            view.DisplayName = P13SentinelName(k);
                            view.Guid = Guid.NewGuid();
                            sentinels[view.DisplayName] = view.Guid;
                            document.SavedViewpoints.AddCopy(sub, view);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Say("PART B the adds THREW " + error.GetType().Name + ": " + error.Message + ", so part B stops here");
                return;
            }

            Say("added the folder [" + P13Top + "] at the root, [" + P13Sub + "] in it and " + P13Sentinels + " .NET SavedViewpoints in that, each Guid set before its AddCopy");
            P13Tree b0 = P13Snap(document, "after the adds");
            int resolved0 = P13Resolve(document, sentinels, b0, "after the adds");

            string removedB;
            double secondsB;
            List<double> seconds = new List<double>();
            List<string> removedNames = new List<string>();
            string middle = P13SentinelName(P13SentinelRemoved);
            bool returnedB = P13Remove(document, subNames, middle, "B middle", out removedB, out secondsB);

            if (!returnedB)
            {
                Say("PART B RemoveAt did not return on the middle view, so part B stops here");
                return;
            }

            seconds.Add(secondsB);
            removedNames.Add(middle);
            int shiftedB;
            List<P13Row> expectedB = P13Apply(b0.Rows, removedB, out shiftedB);
            P13Tree b1 = P13Snap(document, "after the middle removal");
            int mismatchB1 = P13Compare(expectedB, b1.Rows, "after the middle removal against the adds less the one");
            Say("later siblings whose last index falls by one: " + shiftedB);

            for (int s = 0; s < P13Series; s++)
            {
                string last = null;

                using (GroupItem sub = (GroupItem)ResolveNames(document, subNames))
                {
                    if (sub != null && sub.Children.Count > 0)
                    {
                        using (SavedItem child = sub.Children[sub.Children.Count - 1])
                        {
                            last = child.DisplayName;
                        }
                    }
                }

                if (last == null)
                {
                    Say("PART B the folder [" + P13Sub + "] is not found or empty, so the series stops");
                    break;
                }

                string removedS;
                double secondsS;

                if (!P13Remove(document, subNames, last, "B end " + (s + 1), out removedS, out secondsS))
                {
                    Say("PART B the series stops");
                    break;
                }

                seconds.Add(secondsS);
                removedNames.Add(last);
                int ignored;
                expectedB = P13Apply(expectedB, removedS, out ignored);
            }

            P13Tree b2 = P13Snap(document, "after the series");
            int mismatchB2 = P13Compare(expectedB, b2.Rows, "after the series against the adds less every one removed");
            int resolved2 = P13Resolve(document, sentinels, b2, "after the series");
            string saveB = Path.Combine(Path.GetDirectoryName(saveAs), Path.GetFileNameWithoutExtension(saveAs) + "-sentinels.nwf");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveB);
            Say("SaveFile into " + Path.GetFileName(saveB) + " took " + Seconds(clock) + ", " + Bytes(saveB) + " bytes read back off the disk");
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopenedB = document.TryOpenFile(saveB);
            Say("TryOpenFile of the saved file returned " + reopenedB + " after " + Seconds(clock));

            if (!reopenedB)
            {
                Say("PART B UNKNOWN   the second saved file would not reopen");
                return;
            }

            P13Tree b3 = P13Snap(document, "after the second save, clear and reopen");
            int mismatchB3 = P13Compare(b2.Rows, b3.Rows, "after the second reopen against after the series");
            int resolved3 = P13Resolve(document, sentinels, b3, "after the second reopen");
            int kept = P13Sentinels + 2 - removedNames.Count;
            List<string> secondsShown = new List<string>();
            double total = 0;
            double most = 0;

            foreach (double d in seconds)
            {
                secondsShown.Add(d.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
                total += d;
                most = Math.Max(most, d);
            }

            Say(string.Empty);
            Say("PART B SUMMARY: removed " + removedNames.Count + " of " + P13Sentinels + " views, [" + Shown(string.Join("], [", removedNames.ToArray())) + "]. Every other item the same, in the same order, after the middle removal "
                + Yes(mismatchB1 == 0) + ", after the series " + Yes(mismatchB2 == 0) + ", after the reopen " + Yes(mismatchB3 == 0)
                + ". ResolveGuid gave the item at its index path for " + resolved0 + " of " + (P13Sentinels + 2) + " after the adds, " + resolved2 + " of " + kept + " kept after the series, " + resolved3 + " of " + kept + " after the reopen");
            Say("SECONDS PER RemoveAt CALL: part A " + secondsA.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s in a tree of " + t0.Views + " viewpoints. Part B "
                + string.Join(", ", secondsShown.ToArray()) + " s, the most " + most.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s, the mean "
                + (seconds.Count == 0 ? "UNKNOWN" : (total / seconds.Count).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s") + ", in a tree of " + b0.Views + " viewpoints");
            bool yesB = mismatchB1 == 0 && mismatchB2 == 0 && mismatchB3 == 0 && resolved0 == P13Sentinels + 2 && resolved2 == kept && resolved3 == kept && removedNames.Count == P13Series + 1;
            Say("P13 WITH GUIDS SET " + (yesB ? "YES" : "NO") + "   every item left kept its Guid, and ResolveGuid found it where it is, through each removal, a save and a reopen");
        }

        private static string P13SentinelName(int k)
        {
            return "P13 view " + k.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
        }

        private P13Tree P13Snap(Document document, string when)
        {
            P13Tree tree = new P13Tree();
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                P13Walk(root, string.Empty, string.Empty, tree);
            }

            foreach (P13Row row in tree.Rows)
            {
                if (row.Guid != Guid.Empty.ToString())
                {
                    tree.NotEmptyGuids++;
                }
            }

            Say("the tree " + when + ": items " + tree.Rows.Count + ", viewpoints " + tree.Views + ", folders " + tree.Folders + ", other kinds " + tree.Other
                + ", reads that threw " + tree.Threw + ", Guids not empty " + tree.NotEmptyGuids + ", the walk " + Seconds(clock));
            return tree;
        }

        private static void P13Walk(GroupItem parent, string index, string folder, P13Tree tree)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    P13Row row = new P13Row();
                    string at = i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    row.Index = index.Length == 0 ? at : index + "." + at;
                    row.Folder = folder;
                    row.Kind = child.GetType().Name;

                    try
                    {
                        row.Name = child.DisplayName ?? string.Empty;
                    }
                    catch (Exception)
                    {
                        row.Name = "THREW";
                        tree.Threw++;
                    }

                    try
                    {
                        row.Guid = child.Guid.ToString();
                    }
                    catch (Exception)
                    {
                        row.Guid = "THREW";
                        tree.Threw++;
                    }

                    try
                    {
                        row.Comments = child.Comments == null ? 0 : child.Comments.Count;
                    }
                    catch (Exception)
                    {
                        row.Comments = -1;
                        tree.Threw++;
                    }

                    tree.Rows.Add(row);
                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        tree.Folders++;
                        P13Walk(group, row.Index, folder.Length == 0 ? row.Name : folder + "\u0001" + row.Name, tree);
                    }
                    else if (child is SavedViewpoint)
                    {
                        tree.Views++;
                    }
                    else
                    {
                        tree.Other++;
                    }
                }
            }
        }

        private static int[] P13Parts(string index)
        {
            string[] parts = index.Split('.');
            int[] values = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                values[i] = int.Parse(parts[i], System.Globalization.CultureInfo.InvariantCulture);
            }

            return values;
        }

        /// <summary>The rows less the one at the index path and anything under it, with every later sibling and what is under it one lower at that level.</summary>
        private static List<P13Row> P13Apply(List<P13Row> rows, string removed, out int shifted)
        {
            int[] gone = P13Parts(removed);
            int level = gone.Length - 1;
            List<P13Row> left = new List<P13Row>();
            shifted = 0;

            foreach (P13Row row in rows)
            {
                int[] parts = P13Parts(row.Index);
                bool samePrefix = parts.Length >= gone.Length;

                for (int i = 0; samePrefix && i < level; i++)
                {
                    if (parts[i] != gone[i])
                    {
                        samePrefix = false;
                    }
                }

                if (samePrefix && parts[level] == gone[level])
                {
                    continue;
                }

                P13Row copy = row.Copy();

                if (samePrefix && parts[level] > gone[level])
                {
                    parts[level]--;
                    string[] text = new string[parts.Length];

                    for (int i = 0; i < parts.Length; i++)
                    {
                        text[i] = parts[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }

                    copy.Index = string.Join(".", text);
                    shifted++;
                }

                left.Add(copy);
            }

            return left;
        }

        /// <summary>Row by row, in tree order: folder names, name, kind, Guid and comment count, then the index path.</summary>
        private int P13Compare(List<P13Row> expected, List<P13Row> actual, string label)
        {
            int keys = 0;
            int indexes = 0;
            int shown = 0;
            int most = Math.Max(expected.Count, actual.Count);

            for (int i = 0; i < most; i++)
            {
                P13Row e = i < expected.Count ? expected[i] : null;
                P13Row a = i < actual.Count ? actual[i] : null;
                string why = null;

                if (e == null || a == null)
                {
                    keys++;
                    why = e == null ? "an extra row" : "a missing row";
                }
                else if (!string.Equals(e.Key, a.Key, StringComparison.Ordinal))
                {
                    keys++;
                    why = "folder names, name, kind, Guid or comments differ";
                }
                else if (!string.Equals(e.Index, a.Index, StringComparison.Ordinal))
                {
                    indexes++;
                    why = "the index path differs";
                }

                if (why != null && shown < 10)
                {
                    shown++;
                    Say("      row " + i + ": " + why + ". Expected " + (e == null ? "none" : e.Index + " [" + Shown(e.Folder.Replace("\u0001", " / ")) + "] [" + Shown(e.Name) + "] " + e.Kind + " " + e.Guid + " comments " + e.Comments)
                        + ", read " + (a == null ? "none" : a.Index + " [" + Shown(a.Folder.Replace("\u0001", " / ")) + "] [" + Shown(a.Name) + "] " + a.Kind + " " + a.Guid + " comments " + a.Comments));
                }
            }

            Say("   compare " + label + ": rows expected " + expected.Count + ", read " + actual.Count + ", rows whose names, kind, Guid or comments differ " + keys + ", rows whose index path alone differs " + indexes);
            return keys + indexes;
        }

        private bool P13Gone(Document document, List<string> folderNames, string name, string when)
        {
            List<string> path = new List<string>(folderNames);
            path.Add(name);

            using (SavedItem found = ResolveNames(document, path))
            {
                Say("   the target's names find " + (found == null ? "nothing" : "a " + found.GetType().Name + " at " + P10IndexPath(document, found)) + " " + when);
                return found == null;
            }
        }

        /// <summary>The first folder two deep, in tree order, with at least 3 children and a viewpoint at its middle child.</summary>
        private static bool P13Pick(Document document, out List<string> folderNames, out string name, out int index, out int count)
        {
            folderNames = null;
            name = null;
            index = -1;
            count = 0;

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                SavedItemCollection tops = root.Children;

                for (int i = 0; i < tops.Count; i++)
                {
                    using (SavedItem top = tops[i])
                    {
                        GroupItem topGroup = top as GroupItem;

                        if (topGroup == null)
                        {
                            continue;
                        }

                        SavedItemCollection subs = topGroup.Children;

                        for (int j = 0; j < subs.Count; j++)
                        {
                            using (SavedItem sub = subs[j])
                            {
                                GroupItem subGroup = sub as GroupItem;

                                if (subGroup == null || subGroup.Children.Count < 3)
                                {
                                    continue;
                                }

                                int middle = subGroup.Children.Count / 2;

                                using (SavedItem leaf = subGroup.Children[middle])
                                {
                                    if (!(leaf is SavedViewpoint))
                                    {
                                        continue;
                                    }

                                    folderNames = new List<string> { top.DisplayName, sub.DisplayName };
                                    name = leaf.DisplayName;
                                    index = middle;
                                    count = subGroup.Children.Count;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>The parent resolved fresh from RootItem by its names, the target re-found in it by its name, then RemoveAt(parent, index), timed.</summary>
        private bool P13Remove(Document document, List<string> folderNames, string name, string label, out string removed, out double seconds)
        {
            removed = null;
            seconds = -1;
            SavedItem resolved = ResolveNames(document, folderNames);
            GroupItem parent = resolved as GroupItem;

            if (parent == null)
            {
                if (resolved != null)
                {
                    resolved.Dispose();
                }

                Say("   " + label + ": the parent [" + Shown(string.Join(" / ", folderNames.ToArray())) + "] is NOT FOUND as a folder by its names");
                return false;
            }

            using (parent)
            {
                string parentAt = P10IndexPath(document, parent);
                SavedItemCollection children = parent.Children;
                int before = children.Count;
                int at = -1;
                int same = 0;

                for (int i = 0; i < children.Count; i++)
                {
                    using (SavedItem child = children[i])
                    {
                        if (child is SavedViewpoint && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                        {
                            same++;

                            if (at < 0)
                            {
                                at = i;
                            }
                        }
                    }
                }

                if (at < 0)
                {
                    Say("   " + label + ": no viewpoint named [" + Shown(name) + "] in the parent at " + parentAt);
                    return false;
                }

                removed = parentAt + "." + at.ToString(System.Globalization.CultureInfo.InvariantCulture);
                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    document.SavedViewpoints.RemoveAt(parent, at);
                }
                catch (Exception error)
                {
                    Say("   " + label + ": RemoveAt(parent, " + at + ") THREW after " + Seconds(clock) + ", " + error.GetType().Name + ": " + error.Message);
                    return false;
                }

                seconds = clock.Elapsed.TotalSeconds;
                Say("   " + label + ": RemoveAt(parent at " + parentAt + ", " + at + ") on [" + Shown(name) + "] RETURNED after " + Seconds(clock)
                    + ". The parent held " + before + " children and the name was found on " + same + " of them");
            }

            using (SavedItem again = ResolveNames(document, folderNames))
            {
                GroupItem group = again as GroupItem;
                Say("   " + label + ": the parent resolved again holds " + (group == null ? "UNKNOWN, not found" : group.Children.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)) + " children");
            }

            return true;
        }

        /// <summary>For each Guid the probe set, ResolveGuid, counted as found when it gives the item of that name at the index path the tree gives that name.</summary>
        private int P13Resolve(Document document, Dictionary<string, Guid> sentinels, P13Tree tree, string when)
        {
            Dictionary<string, string> indexByName = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (P13Row row in tree.Rows)
            {
                if (sentinels.ContainsKey(row.Name) && string.Equals(row.Guid, sentinels[row.Name].ToString(), StringComparison.Ordinal))
                {
                    indexByName[row.Name] = row.Index;
                }
            }

            int found = 0;
            List<string> missing = new List<string>();

            foreach (KeyValuePair<string, Guid> pair in sentinels)
            {
                string at;
                bool inTree = indexByName.TryGetValue(pair.Key, out at);

                try
                {
                    using (SavedItem item = document.SavedViewpoints.ResolveGuid(pair.Value))
                    {
                        if (item == null)
                        {
                            missing.Add(pair.Key + (inTree ? " NULL BUT IN THE TREE" : " null, not in the tree"));
                            continue;
                        }

                        string itemAt = P10IndexPath(document, item);

                        if (inTree && string.Equals(item.DisplayName, pair.Key, StringComparison.Ordinal) && string.Equals(itemAt, at, StringComparison.Ordinal))
                        {
                            found++;
                        }
                        else
                        {
                            missing.Add(pair.Key + " gave [" + Shown(item.DisplayName) + "] at " + itemAt + (inTree ? ", the tree has it at " + at : ", not in the tree"));
                        }
                    }
                }
                catch (Exception error)
                {
                    missing.Add(pair.Key + " THREW " + error.GetType().Name);
                }
            }

            Say("   ResolveGuid " + when + ": the item at its index path for " + found + " of " + sentinels.Count + " Guids set. The others: " + (missing.Count == 0 ? "none" : string.Join("; ", missing.ToArray())));
            return found;
        }

        private string P13Counts(Document document)
        {
            int sets = 0;
            int withSearch = 0;
            int unreadable = 0;
            List<string> ignored = new List<string>();

            using (FolderItem root = document.SelectionSets.RootItem)
            {
                WalkSets(root, string.Empty, ignored, ref sets, ref withSearch, ref unreadable);
            }

            int tests = 0;
            int results = 0;
            int statuses = 0;

            try
            {
                DocumentClashTests data = document.GetClash().TestsData;
                tests = data.Tests.Count;

                for (int t = 0; t < data.Tests.Count; t++)
                {
                    ClashTest test = data.Tests[t] as ClashTest;

                    if (test != null)
                    {
                        results += CountResultsUnder(test.Children, ref statuses);
                    }
                }
            }
            catch (Exception error)
            {
                return "the clash side THREW " + error.GetType().Name;
            }

            return "models " + document.Models.Count + ", sets " + sets + ", tests " + tests + ", results " + results + ", statuses a person set " + statuses;
        }

        // ---------- P14 of Q114, does RemoveAt(parent, index) on a folder take it and every view under it in one call ----------

        private const string P14Top = "P14 probe";
        private const string P14Keep = "P14 keep";
        private const string P14Gone = "P14 gone";
        private const string P14Inner = "P14 gone inner";
        private const string P14After = "P14 after";
        private const double P14SeriesCapSeconds = 600;

        /// <summary>
        /// P14: RemoveAt(parent, index) on a FOLDER, the parent resolved fresh and the folder re-found
        /// in it by its name just before. Part A takes the top level folder of the copy that holds the
        /// most viewpoints and removes it in one call, timed. The whole tree is read before, after the
        /// call and after a SaveFile, a Document.Clear and a TryOpenFile of the saved file. After the
        /// call the tree must be the tree at the open less the folder and every row under it, with only
        /// the later siblings one lower, and after the reopen exactly the tree after the call. Part B
        /// does the same two folders deep on a folder of views and a folder in it whose Guids the probe
        /// set, beside a folder kept before it and one after it, and reads every Guid back through
        /// ResolveGuid: the kept ones at their index path, the removed ones null. Part C reopens the
        /// untouched copy and takes the same folder's views one at a time from the end, each call timed,
        /// then the empty folder, and compares the tree with part A's.
        /// </summary>
        private void MeasureFolderRemove(string nwf, string saveAs)
        {
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document == null)
            {
                Say("UNKNOWN: no active document in this host");
                return;
            }

            if (string.IsNullOrEmpty(saveAs))
            {
                Say("UNKNOWN: no save path was handed in");
                return;
            }

            Say("opening " + Path.GetFileName(nwf));
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("UNKNOWN: TryOpenFile returned false");
                return;
            }

            string loopRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NwcFederatorLoop") + "\\";
            Say("models " + document.Models.Count);

            for (int m = 0; m < document.Models.Count; m++)
            {
                string file = document.Models[m].FileName ?? string.Empty;
                Say("   model " + m + "  " + Path.GetFileName(file) + "  under the loop folder "
                    + file.StartsWith(loopRoot, StringComparison.OrdinalIgnoreCase));
            }

            Say(string.Empty);
            Say("PART A. THE TOP LEVEL FOLDER HOLDING THE MOST VIEWPOINTS, REMOVED WITH EVERYTHING UNDER IT BY ONE RemoveAt(root, index)");
            string counts0 = P13Counts(document);
            Say("the document at the open: " + counts0);
            P13Tree t0 = P13Snap(document, "at the open");

            string folderName;
            int folderAt;

            if (!P14PickLargest(document, out folderName, out folderAt))
            {
                Say("P14 UNKNOWN   the root holds no folder with a viewpoint under it, so nothing was removed");
                return;
            }

            string folderIndex = folderAt.ToString(System.Globalization.CultureInfo.InvariantCulture);
            int underRows = 0;
            int underViews = 0;
            int underFolders = 0;
            int directChildren = 0;

            foreach (P13Row row in t0.Rows)
            {
                if (row.Index.StartsWith(folderIndex + ".", StringComparison.Ordinal))
                {
                    underRows++;

                    if (row.Kind == "SavedViewpoint")
                    {
                        underViews++;
                    }
                    else if (row.Kind == "FolderItem" || row.Kind == "GroupItem")
                    {
                        underFolders++;
                    }

                    if (row.Index.IndexOf('.', folderIndex.Length + 1) < 0)
                    {
                        directChildren++;
                    }
                }
            }

            Say("the target: [" + Shown(folderName) + "] at index path " + folderIndex + ", direct children " + directChildren + ", rows under it " + underRows
                + ", of them viewpoints " + underViews + " and folders " + underFolders + ". Viewpoints elsewhere in the tree " + (t0.Views - underViews));

            string removedA;
            double secondsA;
            bool returnedA = P14RemoveFolder(document, new List<string>(), folderName, "A", out removedA, out secondsA);

            if (!returnedA)
            {
                Say("P14 NO   RemoveAt did not return on the folder, so nothing more is read");
                return;
            }

            Say("the index path removed " + removedA + ", the same as the one read off the tree at the open " + Yes(string.Equals(removedA, folderIndex, StringComparison.Ordinal)));
            P13Tree t1 = P13Snap(document, "after the removal");
            string counts1 = P13Counts(document);
            Say("the document after the removal: " + counts1);
            int shifted1;
            List<P13Row> expected1 = P13Apply(t0.Rows, removedA, out shifted1);
            Say("expected after the removal: the tree at the open less the folder and its " + underRows + " rows, " + expected1.Count + " rows, " + shifted1 + " of them later siblings or under one, whose index at that level falls by one");
            int mismatch1 = P13Compare(expected1, t1.Rows, "after the removal against the open less the folder");
            bool gone1 = P13Gone(document, new List<string>(), folderName, "after the removal");

            clock = System.Diagnostics.Stopwatch.StartNew();
            document.SaveFile(saveAs);
            Say("SaveFile into " + Path.GetFileName(saveAs) + " took " + Seconds(clock) + ", " + Bytes(saveAs) + " bytes read back off the disk, the copy opened was " + Bytes(nwf));
            clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool reopened = document.TryOpenFile(saveAs);
            Say("TryOpenFile of the saved file returned " + reopened + " after " + Seconds(clock));

            if (!reopened)
            {
                Say("P14 UNKNOWN   the saved file would not reopen, so nothing after a reopen is read");
                return;
            }

            P13Tree t2 = P13Snap(document, "after the save, the clear and the reopen");
            string counts2 = P13Counts(document);
            Say("the document after the reopen: " + counts2);
            int mismatch2 = P13Compare(t1.Rows, t2.Rows, "after the reopen against after the removal");
            bool gone2 = P13Gone(document, new List<string>(), folderName, "after the reopen");

            bool viewsFell = t0.Views - t1.Views == underViews && t2.Views == t1.Views;
            bool rowsFell = t0.Rows.Count - t1.Rows.Count == underRows + 1 && t2.Rows.Count == t1.Rows.Count;
            bool countsSame = string.Equals(counts0, counts1, StringComparison.Ordinal) && string.Equals(counts1, counts2, StringComparison.Ordinal);
            Say(string.Empty);
            Say("PART A SUMMARY: viewpoints " + t0.Views + ", " + t1.Views + ", " + t2.Views + " at the open, after the removal, after the reopen. Items " + t0.Rows.Count + ", " + t1.Rows.Count + ", " + t2.Rows.Count
                + ". Fell by exactly the folder's " + underViews + " viewpoints and " + (underRows + 1) + " items and stayed " + Yes(viewsFell && rowsFell)
                + ". Every other item the same folder names, name, kind, Guid and comment count, in the same order, after the removal " + Yes(mismatch1 == 0) + " and after the reopen " + Yes(mismatch2 == 0)
                + ". The folder found by its name after the removal " + Yes(!gone1) + " and after the reopen " + Yes(!gone2)
                + ". Models, sets, tests, results and statuses the same at every stage " + Yes(countsSame)
                + ". RemoveAt took " + secondsA.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s for a folder of " + underViews + " viewpoints in a tree of " + t0.Views);
            bool yesA = viewsFell && rowsFell && mismatch1 == 0 && mismatch2 == 0 && gone1 && gone2 && countsSame;
            Say("P14 " + (yesA ? "YES" : "NO") + "   RemoveAt(root, index), the root read fresh and the folder found again by its name, removed the folder with every view under it in one call, and every other item kept its path, name and Guid through a save and a reopen");

            Say(string.Empty);
            Say("PART B. A FOLDER OF VIEWS AND A FOLDER IN IT, WHOSE GUIDS THE PROBE SET, TWO FOLDERS DEEP IN THE REOPENED DOCUMENT, REMOVED BY ONE RemoveAt(parent, index)");
            Dictionary<string, Guid> sentinels = new Dictionary<string, Guid>(StringComparer.Ordinal);
            HashSet<string> removedNames = new HashSet<string>(StringComparer.Ordinal);

            try
            {
                using (GroupItem root = document.SavedViewpoints.RootItem)
                using (FolderItem folder = new FolderItem())
                {
                    folder.DisplayName = P14Top;
                    folder.Guid = Guid.NewGuid();
                    sentinels[P14Top] = folder.Guid;
                    document.SavedViewpoints.AddCopy(root, folder);
                }

                P14AddFolder(document, new List<string> { P14Top }, P14Keep, sentinels);
                P14AddFolder(document, new List<string> { P14Top }, P14Gone, sentinels);
                P14AddFolder(document, new List<string> { P14Top, P14Gone }, P14Inner, sentinels);
                P14AddFolder(document, new List<string> { P14Top }, P14After, sentinels);

                using (Viewpoint camera = document.CurrentViewpoint.CreateCopy())
                {
                    P14AddViews(document, camera, new List<string> { P14Top, P14Keep }, "P14 keep view ", 3, sentinels);
                    P14AddViews(document, camera, new List<string> { P14Top, P14Gone }, "P14 gone view ", 12, sentinels);
                    P14AddViews(document, camera, new List<string> { P14Top, P14Gone, P14Inner }, "P14 inner view ", 3, sentinels);
                    P14AddViews(document, camera, new List<string> { P14Top, P14After }, "P14 after view ", 2, sentinels);
                }
            }
            catch (Exception error)
            {
                Say("PART B the adds THREW " + error.GetType().Name + ": " + error.Message + ", so part B stops here");
                P14SeriesPart(document, nwf, folderName, t0, expected1);
                return;
            }

            foreach (string name in sentinels.Keys)
            {
                if (name == P14Gone || name == P14Inner || name.StartsWith("P14 gone view ", StringComparison.Ordinal) || name.StartsWith("P14 inner view ", StringComparison.Ordinal))
                {
                    removedNames.Add(name);
                }
            }

            Say("added [" + P14Top + "] at the root, in it [" + P14Keep + "] of 3 views, [" + P14Gone + "] of 12 views and [" + P14Inner + "] of 3 views after them, and [" + P14After + "] of 2 views, "
                + sentinels.Count + " items in all, each Guid set before its AddCopy. To be removed with [" + P14Gone + "]: " + removedNames.Count);
            P13Tree b0 = P13Snap(document, "after the adds");
            int keptB0;
            int nullB0;
            P14Resolve(document, sentinels, new HashSet<string>(StringComparer.Ordinal), b0, "after the adds", out keptB0, out nullB0);

            string removedB;
            double secondsB;
            bool returnedB = P14RemoveFolder(document, new List<string> { P14Top }, P14Gone, "B", out removedB, out secondsB);
            int mismatchB1 = -1;
            int mismatchB2 = -1;
            int keptB1 = 0;
            int nullB1 = 0;
            int keptB2 = 0;
            int nullB2 = 0;
            int keep = sentinels.Count - removedNames.Count;

            if (returnedB)
            {
                int shiftedB;
                List<P13Row> expectedB = P13Apply(b0.Rows, removedB, out shiftedB);
                P13Tree b1 = P13Snap(document, "after the folder removal");
                mismatchB1 = P13Compare(expectedB, b1.Rows, "after the folder removal against the adds less the folder");
                Say("rows under a later sibling or a later sibling, whose index at that level falls by one: " + shiftedB);
                P14Resolve(document, sentinels, removedNames, b1, "after the folder removal", out keptB1, out nullB1);
                string saveB = Path.Combine(Path.GetDirectoryName(saveAs), Path.GetFileNameWithoutExtension(saveAs) + "-sentinels.nwf");
                clock = System.Diagnostics.Stopwatch.StartNew();
                document.SaveFile(saveB);
                Say("SaveFile into " + Path.GetFileName(saveB) + " took " + Seconds(clock) + ", " + Bytes(saveB) + " bytes read back off the disk");
                clock = System.Diagnostics.Stopwatch.StartNew();
                document.Clear();
                Say("Document.Clear took " + Seconds(clock) + ", models now " + document.Models.Count + ", viewpoints now " + CountViewpoints(document));
                clock = System.Diagnostics.Stopwatch.StartNew();
                bool reopenedB = document.TryOpenFile(saveB);
                Say("TryOpenFile of the saved file returned " + reopenedB + " after " + Seconds(clock));

                if (reopenedB)
                {
                    P13Tree b2 = P13Snap(document, "after the second save, clear and reopen");
                    mismatchB2 = P13Compare(b1.Rows, b2.Rows, "after the second reopen against after the folder removal");
                    P14Resolve(document, sentinels, removedNames, b2, "after the second reopen", out keptB2, out nullB2);
                }
                else
                {
                    Say("PART B UNKNOWN   the second saved file would not reopen");
                }
            }

            Say(string.Empty);
            Say("PART B SUMMARY: RemoveAt on [" + P14Gone + "] returned " + Yes(returnedB) + (returnedB ? " after " + secondsB.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s" : string.Empty)
                + ". Every other item the same, in the same order, after the removal " + Yes(mismatchB1 == 0) + " and after the reopen " + Yes(mismatchB2 == 0)
                + ". ResolveGuid gave the item at its index path for " + keptB0 + " of " + sentinels.Count + " after the adds, for " + keptB1 + " of " + keep + " kept after the removal and " + keptB2 + " of " + keep + " after the reopen"
                + ". The " + removedNames.Count + " removed Guids resolved to null " + nullB1 + " after the removal and " + nullB2 + " after the reopen");
            bool yesB = returnedB && mismatchB1 == 0 && mismatchB2 == 0 && keptB0 == sentinels.Count && keptB1 == keep && keptB2 == keep && nullB1 == removedNames.Count && nullB2 == removedNames.Count;
            Say("P14 TWO DEEP WITH GUIDS SET " + (yesB ? "YES" : "NO") + "   one call took the folder, the folder in it and every view under both, the kept items kept their Guids where they are, and every removed Guid resolved to nothing, through a save and a reopen");

            P14SeriesPart(document, nwf, folderName, t0, expected1);
        }

        /// <summary>Part C: the untouched copy reopened, the same folder's views removed one at a time from the end, each call timed, then the empty folder, the tree compared with part A's.</summary>
        private void P14SeriesPart(Document document, string nwf, string folderName, P13Tree t0, List<P13Row> expectedA)
        {
            Say(string.Empty);
            Say("PART C. THE UNTOUCHED COPY REOPENED, THE SAME FOLDER'S VIEWS REMOVED ONE AT A TIME FROM THE END, EACH TIMED, CAPPED AT " + P14SeriesCapSeconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " s");
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            document.Clear();
            Say("Document.Clear took " + Seconds(clock));
            clock = System.Diagnostics.Stopwatch.StartNew();
            bool opened = document.TryOpenFile(nwf);
            Say("TryOpenFile of the untouched copy " + Path.GetFileName(nwf) + " returned " + opened + " after " + Seconds(clock));

            if (!opened)
            {
                Say("PART C UNKNOWN   the copy would not reopen");
                return;
            }

            P13Tree c0 = P13Snap(document, "at the second open");
            int mismatchC0 = P13Compare(t0.Rows, c0.Rows, "at the second open against the first open");
            List<string> names = new List<string> { folderName };
            List<double> calls = new List<double>();
            List<double> rounds = new List<double>();
            int notViews = 0;
            int threw = 0;
            string stopped = "the folder is empty";
            System.Diagnostics.Stopwatch all = System.Diagnostics.Stopwatch.StartNew();

            while (true)
            {
                if (all.Elapsed.TotalSeconds > P14SeriesCapSeconds)
                {
                    stopped = "the cap of " + P14SeriesCapSeconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " s was reached";
                    break;
                }

                System.Diagnostics.Stopwatch round = System.Diagnostics.Stopwatch.StartNew();
                SavedItem resolved = ResolveNames(document, names);
                GroupItem parent = resolved as GroupItem;

                if (parent == null)
                {
                    if (resolved != null)
                    {
                        resolved.Dispose();
                    }

                    stopped = "the folder was NOT FOUND by its name";
                    break;
                }

                using (parent)
                {
                    int count = parent.Children.Count;

                    if (count == 0)
                    {
                        break;
                    }

                    using (SavedItem last = parent.Children[count - 1])
                    {
                        if (!(last is SavedViewpoint))
                        {
                            notViews++;
                        }
                    }

                    System.Diagnostics.Stopwatch call = System.Diagnostics.Stopwatch.StartNew();

                    try
                    {
                        document.SavedViewpoints.RemoveAt(parent, count - 1);
                    }
                    catch (Exception error)
                    {
                        threw++;
                        stopped = "RemoveAt THREW " + error.GetType().Name + ": " + error.Message;
                        break;
                    }

                    calls.Add(call.Elapsed.TotalSeconds);
                }

                rounds.Add(round.Elapsed.TotalSeconds);
            }

            double wall = all.Elapsed.TotalSeconds;
            Say("   the series stopped because " + stopped + ". Calls " + calls.Count + ", items at the end that were not a viewpoint " + notViews + ", throws " + threw);
            Say("   RemoveAt alone: " + P14Stats(calls));
            Say("   each round, the folder resolved fresh by its name, the last child's kind read and RemoveAt: " + P14Stats(rounds) + ". Wall time of the series " + wall.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s");

            if (calls.Count >= 20)
            {
                Say("   RemoveAt, the first 10 calls: " + P14Stats(calls.GetRange(0, 10)) + ". The last 10: " + P14Stats(calls.GetRange(calls.Count - 10, 10)));
            }

            P13Tree c1 = P13Snap(document, "after the series");
            string folderIndex = null;

            foreach (P13Row row in c0.Rows)
            {
                if (row.Folder.Length == 0 && string.Equals(row.Name, folderName, StringComparison.Ordinal))
                {
                    folderIndex = row.Index;
                    break;
                }
            }

            List<P13Row> expectedC = new List<P13Row>();

            foreach (P13Row row in c0.Rows)
            {
                if (folderIndex == null || !row.Index.StartsWith(folderIndex + ".", StringComparison.Ordinal))
                {
                    expectedC.Add(row.Copy());
                }
            }

            int mismatchC1 = P13Compare(expectedC, c1.Rows, "after the series against the second open less every row under the folder, the folder kept");
            string removedC;
            double secondsC;
            bool returnedC = P14RemoveFolder(document, new List<string>(), folderName, "C, the emptied folder", out removedC, out secondsC);
            int mismatchC2 = -1;

            if (returnedC)
            {
                P13Tree c2 = P13Snap(document, "after the emptied folder is removed");
                mismatchC2 = P13Compare(expectedA, c2.Rows, "after the emptied folder is removed against part A after its one call");
            }

            Say(string.Empty);
            Say("PART C SUMMARY: the second open read the same as the first " + Yes(mismatchC0 == 0) + ". " + calls.Count + " calls of RemoveAt(parent, last) in " + wall.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + " s, the folder emptied " + Yes(stopped == "the folder is empty") + ", every other item the same after the series " + Yes(mismatchC1 == 0)
                + ", and after the emptied folder's own RemoveAt, " + (returnedC ? secondsC.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " s" : "which did not return") + ", the tree the same as part A's after its one call " + Yes(mismatchC2 == 0));
        }

        private static string P14Stats(List<double> values)
        {
            if (values.Count == 0)
            {
                return "none";
            }

            List<double> sorted = new List<double>(values);
            sorted.Sort();
            double total = 0;

            foreach (double d in values)
            {
                total += d;
            }

            System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;
            return values.Count + " values, total " + total.ToString("0.000", inv) + " s, mean " + (total / values.Count).ToString("0.000000", inv) + " s, median "
                + sorted[sorted.Count / 2].ToString("0.000000", inv) + " s, least " + sorted[0].ToString("0.000000", inv) + " s, most " + sorted[sorted.Count - 1].ToString("0.000000", inv) + " s";
        }

        /// <summary>The top level folder holding the most viewpoints under it, the first of them in tree order on a tie.</summary>
        private static bool P14PickLargest(Document document, out string name, out int index)
        {
            name = null;
            index = -1;
            int most = 0;

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                SavedItemCollection tops = root.Children;

                for (int i = 0; i < tops.Count; i++)
                {
                    using (SavedItem top = tops[i])
                    {
                        GroupItem group = top as GroupItem;

                        if (group == null)
                        {
                            continue;
                        }

                        int views = ViewpointsUnder(group);

                        if (views > most)
                        {
                            most = views;
                            name = top.DisplayName;
                            index = i;
                        }
                    }
                }
            }

            return name != null;
        }

        /// <summary>The parent read fresh, the root when no names are given, the folder re-found in it by its name, then RemoveAt(parent, index), timed.</summary>
        private bool P14RemoveFolder(Document document, List<string> parentNames, string name, string label, out string removed, out double seconds)
        {
            removed = null;
            seconds = -1;
            GroupItem parent;

            if (parentNames.Count == 0)
            {
                parent = document.SavedViewpoints.RootItem;
            }
            else
            {
                SavedItem resolved = ResolveNames(document, parentNames);
                parent = resolved as GroupItem;

                if (parent == null)
                {
                    if (resolved != null)
                    {
                        resolved.Dispose();
                    }

                    Say("   " + label + ": the parent [" + Shown(string.Join(" / ", parentNames.ToArray())) + "] is NOT FOUND as a folder by its names");
                    return false;
                }
            }

            using (parent)
            {
                string parentAt = parentNames.Count == 0 ? string.Empty : P10IndexPath(document, parent);
                SavedItemCollection children = parent.Children;
                int before = children.Count;
                int at = -1;
                int same = 0;
                int anyKind = 0;
                int childrenOfTarget = -1;
                int viewsOfTarget = -1;

                for (int i = 0; i < children.Count; i++)
                {
                    using (SavedItem child = children[i])
                    {
                        if (!string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        anyKind++;
                        GroupItem group = child as GroupItem;

                        if (group != null)
                        {
                            same++;

                            if (at < 0)
                            {
                                at = i;
                                childrenOfTarget = group.Children.Count;
                                viewsOfTarget = ViewpointsUnder(group);
                            }
                        }
                    }
                }

                if (at < 0)
                {
                    Say("   " + label + ": no folder named [" + Shown(name) + "] in the parent at " + (parentAt.Length == 0 ? "the root" : parentAt));
                    return false;
                }

                string atText = at.ToString(System.Globalization.CultureInfo.InvariantCulture);
                removed = parentAt.Length == 0 ? atText : parentAt + "." + atText;
                System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    document.SavedViewpoints.RemoveAt(parent, at);
                }
                catch (Exception error)
                {
                    Say("   " + label + ": RemoveAt(parent, " + at + ") THREW after " + Seconds(clock) + ", " + error.GetType().Name + ": " + error.Message);
                    return false;
                }

                seconds = clock.Elapsed.TotalSeconds;
                Say("   " + label + ": RemoveAt(parent at " + (parentAt.Length == 0 ? "the root" : parentAt) + ", " + at + ") on the folder [" + Shown(name) + "] of " + childrenOfTarget + " direct children and "
                    + viewsOfTarget + " viewpoints under it RETURNED after " + Seconds(clock) + ". The parent held " + before + " children, the name was found on " + anyKind + " of them and on " + same + " folders");
            }

            if (parentNames.Count == 0)
            {
                using (GroupItem again = document.SavedViewpoints.RootItem)
                {
                    Say("   " + label + ": the root read again holds " + again.Children.Count + " children");
                }
            }
            else
            {
                using (SavedItem again = ResolveNames(document, parentNames))
                {
                    GroupItem group = again as GroupItem;
                    Say("   " + label + ": the parent resolved again holds " + (group == null ? "UNKNOWN, not found" : group.Children.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)) + " children");
                }
            }

            return true;
        }

        private static void P14AddFolder(Document document, List<string> parentNames, string name, Dictionary<string, Guid> sentinels)
        {
            using (GroupItem parent = (GroupItem)ResolveNames(document, parentNames))
            using (FolderItem folder = new FolderItem())
            {
                folder.DisplayName = name;
                folder.Guid = Guid.NewGuid();
                sentinels[name] = folder.Guid;
                document.SavedViewpoints.AddCopy(parent, folder);
            }
        }

        private static void P14AddViews(Document document, Viewpoint camera, List<string> parentNames, string prefix, int count, Dictionary<string, Guid> sentinels)
        {
            for (int k = 0; k < count; k++)
            {
                using (GroupItem parent = (GroupItem)ResolveNames(document, parentNames))
                using (SavedViewpoint view = new SavedViewpoint(camera))
                {
                    view.DisplayName = prefix + k.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
                    view.Guid = Guid.NewGuid();
                    sentinels[view.DisplayName] = view.Guid;
                    document.SavedViewpoints.AddCopy(parent, view);
                }
            }
        }

        /// <summary>For each Guid the probe set: a kept one counts when ResolveGuid gives the item of that name at the index path the tree gives it, a removed one when ResolveGuid gives null.</summary>
        private void P14Resolve(Document document, Dictionary<string, Guid> sentinels, HashSet<string> removed, P13Tree tree, string when, out int keptFound, out int removedNull)
        {
            keptFound = 0;
            removedNull = 0;
            Dictionary<string, string> indexByName = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (P13Row row in tree.Rows)
            {
                if (sentinels.ContainsKey(row.Name) && string.Equals(row.Guid, sentinels[row.Name].ToString(), StringComparison.Ordinal))
                {
                    indexByName[row.Name] = row.Index;
                }
            }

            List<string> other = new List<string>();

            foreach (KeyValuePair<string, Guid> pair in sentinels)
            {
                bool gone = removed.Contains(pair.Key);
                string at;
                bool inTree = indexByName.TryGetValue(pair.Key, out at);

                try
                {
                    using (SavedItem item = document.SavedViewpoints.ResolveGuid(pair.Value))
                    {
                        if (item == null)
                        {
                            if (gone && !inTree)
                            {
                                removedNull++;
                            }
                            else
                            {
                                other.Add(pair.Key + " null" + (inTree ? " BUT IN THE TREE at " + at : ", not in the tree"));
                            }

                            continue;
                        }

                        string itemAt = P10IndexPath(document, item);

                        if (!gone && inTree && string.Equals(item.DisplayName, pair.Key, StringComparison.Ordinal) && string.Equals(itemAt, at, StringComparison.Ordinal))
                        {
                            keptFound++;
                        }
                        else
                        {
                            other.Add(pair.Key + (gone ? " REMOVED BUT" : string.Empty) + " gave [" + Shown(item.DisplayName) + "] at " + itemAt + (inTree ? ", the tree has it at " + at : ", not in the tree"));
                        }
                    }
                }
                catch (Exception error)
                {
                    other.Add(pair.Key + " THREW " + error.GetType().Name);
                }
            }

            Say("   ResolveGuid " + when + ": kept items found at their index path " + keptFound + " of " + (sentinels.Count - removed.Count) + ", removed items resolving to null " + removedNull + " of " + removed.Count
                + ". The others: " + (other.Count == 0 ? "none" : string.Join("; ", other.ToArray())));
        }
    }
}
