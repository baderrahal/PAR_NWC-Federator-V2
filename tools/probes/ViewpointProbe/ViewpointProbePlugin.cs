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
                                        solidNames.Add(item.DisplayName + " in " + Path.GetFileName(model.FileName));
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
    }
}
