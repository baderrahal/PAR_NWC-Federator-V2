using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The saved viewpoints in the open document, F85: how one is counted, found, and
    /// put in with the hidden state it was saved with.
    ///
    /// EVERY MEMBER THIS FILE CALLS IS MEASURED. tools\probes\probe-viewpoints.ps1 read
    /// the shape off the installed Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-19,
    /// docs\history\scan.md 5d, and tools\probes\ViewpointProbe measured what a DLL
    /// cannot say on runs the same day and the next, 5j to 5m:
    ///
    ///     Document.SavedViewpoints                       is a DocumentSavedViewpoints
    ///     DocumentSavedViewpoints.RootItem               is a FolderItem, a GroupItem
    ///     DocumentSavedViewpoints.AddCopy(GroupItem, SavedItem)   puts a copy in a folder,
    ///         keeping the camera and the overrides of what it copies, 5m
    ///     DocumentSavedViewpoints.Remove(SavedItem)      takes one out, 5k and 5m
    ///     new FolderItem()                               makes a folder
    ///     new SavedViewpoint(Viewpoint)                  records the camera ALONE, 5j
    ///     DocumentSavedViewpoints.CaptureRuntimeOverrides()       records what is hidden
    ///         and NO camera at all, its Viewpoint throwing Camera not set, 5l. It is used
    ///         here for one thing, reading the hidden state before the writer hides
    ///         anything, and never to write a viewpoint
    ///     InwOpView with ApplyHideAttribs true, the COM API, added to InwOpState.SavedViews
    ///         records BOTH the camera it is given and what is hidden, reads back through
    ///         the .NET API with ContainsVisibilityOverrides true, and presses with both
    ///         after a save and a reopen, 5m. It is the one way found to write a viewpoint
    ///         that opens on its clash with the other disciplines hidden
    ///     DocumentModels.SetHidden, ResetAllHidden, IsHidden
    ///     SavedViewpoint.GetVisibilityOverrides().Hidden read off a capture NOT in the
    ///         tree, 5k, which is how the hidden state is read before the writer hides
    ///         anything and put back after. ResetAllHiddenToModelState is measured too,
    ///         5k, and measured to LOSE a hide the document held, so it is not called
    ///
    /// EVERY WALK STARTS FROM A FRESH RootItem. AddCopy takes a copy, so a handle read
    /// before it does not see what it put in, which is what SetBuilder learned with the
    /// sets and 5b said not to assume from the pattern. It was read, and the pattern held.
    /// Everything read is disposed on the way, because a wrapper over a document object is
    /// borrowed and never kept.
    /// </summary>
    public static class SavedViewpoints
    {
        /// <summary>
        /// Whether this build knows how to put a viewpoint into the NWF. It went TRUE in
        /// the viewpoints round on 2026-09-19, once 5j had measured that a captured
        /// viewpoint records the hidden state and the run had shown the tree.
        ///
        /// WHY A FLAG AT ALL. A step this tool cannot do is not a step that failed. While
        /// this was false the engine asked for no viewpoints, the judgement was told they
        /// were not requested, and the log said the measurement was outstanding, which is
        /// the opposite of reporting every group FAILED over a feature never attempted.
        /// </summary>
        public static bool CanBuild
        {
            get { return true; }
        }


        /// <summary>
        /// How many saved viewpoints the document holds, walked from the root with every
        /// wrapper disposed on the way, or MINUS ONE where the count could not be taken.
        ///
        /// Minus one is not zero. A rebuild that could not count them does not know whether
        /// they came back, and F50 treats not counted as a reason to leave the NWF on disk
        /// alone, exactly as it treats something lost. Saying zero here would let a rebuild
        /// throw viewpoints away and report that it kept them all.
        /// </summary>
        public static int Count(Document document)
        {
            if (document == null)
            {
                return -1;
            }

            try
            {
                using (GroupItem root = document.SavedViewpoints.RootItem)
                {
                    return CountUnder(root);
                }
            }
            catch (Exception)
            {
                // Swallowed on purpose and reported as not counted. Counting viewpoints is a
                // diagnostic and a diagnostic never stops a run, which is the rule. What it
                // must not do is come back as zero, because zero reads as a real count.
                return -1;
            }
        }

        /// <summary>
        /// Whether a viewpoint of that name is already in that folder path. False where
        /// any folder on the path is not there, which is the ordinary first run case and
        /// not a failure.
        /// </summary>
        public static bool Exists(Document document, IList<string> folders, string name)
        {
            if (document == null || folders == null || string.IsNullOrEmpty(name))
            {
                return false;
            }

            using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
            {
                return parent != null && FindLeaf(parent, name);
            }
        }

        /// <summary>
        /// Makes every folder on the path that is not there yet, outermost first, each one
        /// re-resolved from a fresh RootItem after the AddCopy that made it, which is the
        /// shape SetBuilder.EnsureFolders measured for the sets.
        /// </summary>
        public static void EnsureFolders(Document document, IList<string> folders)
        {
            if (document == null || folders == null)
            {
                return;
            }

            for (int depth = 0; depth < folders.Count; depth++)
            {
                using (GroupItem already = ResolveFolders(document, folders, depth + 1))
                {
                    if (already != null)
                    {
                        continue;
                    }
                }

                using (GroupItem parent = ResolveFolders(document, folders, depth))
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException(
                            "The folder above \"" + folders[depth] + "\" is not there, so it cannot be made.");
                    }

                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = folders[depth];
                        document.SavedViewpoints.AddCopy(parent, folder);
                    }
                }

                using (GroupItem made = ResolveFolders(document, folders, depth + 1))
                {
                    if (made == null)
                    {
                        throw new InvalidOperationException(
                            "The folder \"" + folders[depth] + "\" was added and a fresh read does not show it.");
                    }
                }
            }
        }

        /// <summary>
        /// Puts a saved viewpoint with BOTH that camera and what is hidden right now into
        /// that folder path, under that name. MEASURED on 2026-09-20, docs\history\scan.md
        /// 5m, after two .NET routes each recorded half: new SavedViewpoint(Viewpoint)
        /// records the camera and no overrides, 5j, and CaptureRuntimeOverrides records
        /// the overrides and NO camera, its Viewpoint throwing Camera not set, 5l, which
        /// is why the first viewpoints run opened every viewpoint on sky. The COM view is
        /// the one object that carries a flag for it, InwOpView.ApplyHideAttribs, and a
        /// view added with that flag on reads back through the .NET API with
        /// ContainsVisibilityOverrides true and the camera it was given, presses with
        /// both, and keeps both across a save and a reopen.
        ///
        /// The COM collection adds at the ROOT, so the view is copied into the folder
        /// with AddCopy, which keeps both, and the root one is removed, which is the
        /// shape 5m measured. The root one is found as the LAST root child of that name,
        /// because the add appends and a file may already hold a root item so named.
        /// The caller reads the folder copy back rather than trusting any of it.
        /// </summary>
        public static void Record(Document document, IList<string> folders, string name, Viewpoint camera)
        {
            if (document == null || folders == null || string.IsNullOrEmpty(name) || camera == null)
            {
                throw new ArgumentException("A viewpoint needs a document, a folder path, a name and a camera.", "name");
            }

            InwOpState10 state = ComApiBridge.State;
            InwOpView view = (InwOpView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpView, null, null);
            view.name = name;
            view.ApplyHideAttribs = true;

            // Both flags on since the dimming round. ApplyMaterialAttribs is what records
            // that everything but the two clashing items is dimmed, MEASURED on 2026-09-20,
            // docs\history\scan.md 5o: with it on the viewpoint carries one material
            // override per item that has a material, and pressing it after a save and a
            // reopen dims them again. With it off the viewpoint carries none, which is
            // what F85 shipped and what Bader could not read.
            view.ApplyMaterialAttribs = true;
            view.anonview = ComApiBridge.ToInwOpAnonView(camera);
            state.SavedViews().Add(view);

            using (SavedViewpoint atRoot = FindLastAtRoot(document, name))
            {
                if (atRoot == null)
                {
                    throw new InvalidOperationException("The view was added and a fresh read of the root does not show it.");
                }

                using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException(
                            "The folder path " + string.Join("/", ToArray(folders)) + " is not there.");
                    }

                    document.SavedViewpoints.AddCopy(parent, atRoot);
                }

                if (!document.SavedViewpoints.Remove(atRoot))
                {
                    throw new InvalidOperationException("The view was copied into its folder and the root copy would not remove.");
                }
            }
        }

        /// <summary>
        /// What the viewpoint at that path actually recorded, read off the tree: whether
        /// it is there, how far its camera sits from the one asked for in document units,
        /// and whether it carries visibility overrides. Read back rather than trusted,
        /// because the first run's tree looked complete and every viewpoint opened on sky.
        /// </summary>
        public static ViewpointReadBack ReadBack(Document document, IList<string> folders, string name, Viewpoint camera)
        {
            ViewpointReadBack read = new ViewpointReadBack();

            if (document == null || folders == null || string.IsNullOrEmpty(name) || camera == null)
            {
                return read;
            }

            using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
            {
                if (parent == null)
                {
                    return read;
                }

                using (SavedViewpoint found = FindLeafItem(parent, name))
                {
                    if (found == null)
                    {
                        return read;
                    }

                    read.Found = true;

                    // THE COUNTS AND NOT THE FLAGS. ContainsVisibilityOverrides and
                    // ContainsAppearanceOverrides both read TRUE in the same session on a
                    // viewpoint that overrides nothing at all, MEASURED on 2026-09-20,
                    // docs\history\scan.md 5o, so F85's check that a viewpoint carries
                    // visibility overrides was a check that could not fail. The two
                    // collections underneath carry real numbers before the save and are
                    // what this reads.
                    read.HiddenCount = CountOf(found.GetVisibilityOverrides());
                    read.MaterialOverrideCount = CountOf(found.GetAppearanceOverrides());

                    using (Viewpoint recorded = found.Viewpoint)
                    {
                        Point3D a = recorded.Position;
                        Point3D b = camera.Position;
                        double dx = a.X - b.X;
                        double dy = a.Y - b.Y;
                        double dz = a.Z - b.Z;
                        read.CameraDistance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    }
                }
            }

            return read;
        }

        /// <summary>
        /// Hides every model whose index is not in the list and shows the rest, so the
        /// viewpoint captured next carries exactly that. The hidden state the document
        /// held before the first call is put back through RestoreHiddenState with the
        /// snapshot the builder took, when the group's writing ends. Each model and each
        /// root read here is a fresh wrapper and is released once its path is copied in.
        /// </summary>
        public static int ShowOnlyModels(Document document, ICollection<int> keep)
        {
            if (document == null || keep == null)
            {
                return 0;
            }

            document.Models.ResetAllHidden();

            using (ModelItemCollection hide = new ModelItemCollection())
            {
                for (int i = 0; i < document.Models.Count; i++)
                {
                    if (keep.Contains(i))
                    {
                        continue;
                    }

                    using (Model model = document.Models[i])
                    using (ModelItem root = model.RootItem)
                    {
                        hide.Add(root);
                    }
                }

                if (hide.Count > 0)
                {
                    document.Models.SetHidden(hide, true);
                }

                return hide.Count;
            }
        }

        /// <summary>How many items the overrides name, or zero where there are none. Both collections are borrowed.</summary>
        private static int CountOf(VisibilityOverrides overrides)
        {
            if (overrides == null)
            {
                return 0;
            }

            using (ModelItemCollection hidden = overrides.Hidden)
            {
                return hidden == null ? 0 : hidden.Count;
            }
        }

        private static int CountOf(AppearanceOverrides overrides)
        {
            return overrides == null || overrides.MaterialOverrides == null ? 0 : overrides.MaterialOverrides.Count;
        }

        /// <summary>
        /// Makes everything transparent except the two items the clash is between, so the
        /// viewpoint recorded next opens the way Clash Detective looks at a clash, which
        /// is the whole of the dimming round. Returns how many of the two were brought
        /// back to solid, which the caller reads back.
        ///
        /// TWO CALLS AND NOT ONE PER ITEM. The override goes on the model ROOTS and
        /// reaches every leaf under them, and the reset goes on the two items and brings
        /// exactly those back, MEASURED on 2026-09-20, docs\history\scan.md 5o. Dimming
        /// 2,606 items one at a time, 430 times, is 1.1 million calls in one group.
        ///
        /// TEMPORARY AND NOT PERMANENT. Both record and both survive a save and a reopen,
        /// 5o. The permanent one writes the transparency onto the item, so it lands in the
        /// NWF if a save arrives before the reset, and its only undo clears every
        /// appearance override the file already held, which nothing can read back first.
        /// That is 5k's trap again and the answer is the same one.
        /// </summary>
        public static int DimAllBut(Document document, double transparency, ModelItem first, ModelItem second)
        {
            if (document == null)
            {
                return 0;
            }

            using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
            {
                document.Models.OverrideTemporaryTransparency(roots, transparency);
            }

            using (ModelItemCollection solid = new ModelItemCollection())
            {
                if (first != null)
                {
                    solid.Add(first);
                }

                if (second != null)
                {
                    solid.Add(second);
                }

                if (solid.Count > 0)
                {
                    document.Models.ResetTemporaryMaterials(solid);
                }

                return solid.Count;
            }
        }

        /// <summary>
        /// Takes the dimming off again, scoped to the roots this tool overrode rather than
        /// ResetAllTemporaryMaterials, which would also clear a temporary override this
        /// tool did not set. Measured at under a millisecond, 5o.
        /// </summary>
        public static void Undim(Document document)
        {
            if (document == null)
            {
                return;
            }

            using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
            {
                document.Models.ResetTemporaryMaterials(roots);
            }
        }

        /// <summary>
        /// The item at that index path, or null. The path is plain ints, so walk one can
        /// name an item and walk two resolve it without keeping a native handle alive
        /// across the group, which is what 1,950 handles would be. The caller disposes.
        /// </summary>
        public static ModelItem ItemAt(Document document, int[] path)
        {
            if (document == null || path == null || path.Length == 0)
            {
                return null;
            }

            return document.Models.ResolveIndexPath(path);
        }

        /// <summary>The index path of that item, as plain ints, or null where it has none.</summary>
        public static int[] PathOf(Document document, ModelItem item)
        {
            if (document == null || item == null)
            {
                return null;
            }

            System.Collections.ObjectModel.Collection<int> path = document.Models.CreateIndexPath(item);

            if (path == null || path.Count == 0)
            {
                return null;
            }

            int[] copy = new int[path.Count];
            path.CopyTo(copy, 0);
            return copy;
        }

        /// <summary>
        /// The hidden state the document holds right now, read off a runtime capture that
        /// never goes into the tree, so it can be put back after the writer has hidden and
        /// shown models for every viewpoint. MEASURED on 2026-09-19, docs\history\scan.md
        /// 5k: GetVisibilityOverrides().Hidden reads off the un-added capture, and
        /// ResetAllHidden followed by SetHidden on that collection hides the same items
        /// again. ResetAllHiddenToModelState, which this once called instead, ends at the
        /// state the NWC files define and LOSES a hide the document held, measured on the
        /// same run, so it is not called anywhere now. The caller disposes what comes back.
        /// </summary>
        public static HiddenSnapshot SnapshotHidden(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            SavedViewpoint captured = document.SavedViewpoints.CaptureRuntimeOverrides();

            if (captured == null)
            {
                throw new InvalidOperationException("CaptureRuntimeOverrides returned nothing, so the hidden state could not be read.");
            }

            return new HiddenSnapshot(captured);
        }

        /// <summary>
        /// Back to the hidden state the snapshot read: everything shown, then exactly the
        /// items that were hidden hidden again. Returns whether the document reads those
        /// items as hidden afterwards, which is a check and not a trust, and true where
        /// nothing was hidden to begin with.
        /// </summary>
        public static bool RestoreHiddenState(Document document, HiddenSnapshot snapshot)
        {
            if (document == null || snapshot == null)
            {
                return false;
            }

            document.Models.ResetAllHidden();

            if (snapshot.HiddenCount == 0)
            {
                return true;
            }

            document.Models.SetHidden(snapshot.Hidden, true);
            return document.Models.IsHidden(snapshot.Hidden);
        }

        /// <summary>
        /// Walks the folder path from a freshly read RootItem and returns the folder at
        /// that depth, or null when any level is missing. The caller disposes what comes
        /// back. Depth zero is the root itself.
        /// </summary>
        private static GroupItem ResolveFolders(Document document, IList<string> folders, int depth)
        {
            GroupItem current = document.SavedViewpoints.RootItem;

            for (int i = 0; i < depth; i++)
            {
                GroupItem next = FindFolder(current, folders[i]);
                current.Dispose();

                if (next == null)
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        private static GroupItem FindFolder(GroupItem parent, string folder)
        {
            if (parent == null)
            {
                return null;
            }

            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                GroupItem group = child as GroupItem;

                if (group != null && string.Equals(child.DisplayName, folder, StringComparison.Ordinal))
                {
                    return group;
                }

                child.Dispose();
            }

            return null;
        }

        private static bool FindLeaf(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (!(child is GroupItem) && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>The LAST saved viewpoint of that name at the root, where the COM add appends, or null. The caller disposes it.</summary>
        private static SavedViewpoint FindLastAtRoot(Document document, string name)
        {
            SavedItemCollection items = document.SavedViewpoints.Value;

            for (int i = items.Count - 1; i >= 0; i--)
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

        /// <summary>The saved viewpoint of that name directly under the folder, or null. The caller disposes it.</summary>
        private static SavedViewpoint FindLeafItem(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                SavedViewpoint viewpoint = child as SavedViewpoint;

                if (viewpoint != null && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                {
                    return viewpoint;
                }

                child.Dispose();
            }

            return null;
        }

        private static int CountUnder(GroupItem parent)
        {
            if (parent == null)
            {
                return 0;
            }

            int count = 0;
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        count += CountUnder(group);
                    }
                    else
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static string[] ToArray(IList<string> list)
        {
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return array;
        }
    }

    /// <summary>
    /// What a written viewpoint recorded, read off the tree. Nothing here is trusted from
    /// the write, and nothing here is a FLAG, because both flags this API offers read true
    /// on a viewpoint that recorded nothing, 5o.
    /// </summary>
    public sealed class ViewpointReadBack
    {
        /// <summary>Whether a viewpoint of that name sits at that path at all.</summary>
        public bool Found { get; set; }

        /// <summary>How far its camera sits from the one asked for, in document units, meaningful only where Found.</summary>
        public double CameraDistance { get; set; }

        /// <summary>How many items it hides, which is what makes it show one pair of disciplines when pressed.</summary>
        public int HiddenCount { get; set; }

        /// <summary>How many items it dims, which is what makes the clash readable when pressed.</summary>
        public int MaterialOverrideCount { get; set; }
    }

    /// <summary>
    /// What the document had hidden at one moment, held on a runtime capture that is
    /// never put into the tree. The Hidden collection is read once, because the getter
    /// hands out a wrapper, and both are released together.
    /// </summary>
    public sealed class HiddenSnapshot : IDisposable
    {
        private readonly SavedViewpoint captured;
        private readonly ModelItemCollection hidden;

        internal HiddenSnapshot(SavedViewpoint captured)
        {
            this.captured = captured;
            VisibilityOverrides overrides = captured.GetVisibilityOverrides();
            hidden = overrides == null ? null : overrides.Hidden;
        }

        /// <summary>The items that were hidden, or null where the capture carried no overrides.</summary>
        public ModelItemCollection Hidden
        {
            get { return hidden; }
        }

        public int HiddenCount
        {
            get { return hidden == null ? 0 : hidden.Count; }
        }

        public void Dispose()
        {
            if (hidden != null)
            {
                hidden.Dispose();
            }

            captured.Dispose();
        }
    }
}
