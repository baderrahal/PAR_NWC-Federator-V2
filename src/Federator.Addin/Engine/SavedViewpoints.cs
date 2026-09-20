using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The saved viewpoints in the open document, F85: how one is counted, found, and
    /// put in with the hidden state it was saved with.
    ///
    /// EVERY MEMBER THIS FILE CALLS IS MEASURED. tools\probes\probe-viewpoints.ps1 read
    /// the shape off the installed Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-19,
    /// docs\history\scan.md 5d, and tools\probes\ViewpointProbe measured the one thing a
    /// DLL cannot say on a run the same day, 5j:
    ///
    ///     Document.SavedViewpoints                       is a DocumentSavedViewpoints
    ///     DocumentSavedViewpoints.RootItem               is a FolderItem, a GroupItem
    ///     DocumentSavedViewpoints.AddCopy(GroupItem, SavedItem)   puts one in a folder
    ///     new FolderItem()                               makes a folder
    ///     DocumentSavedViewpoints.CaptureRuntimeOverrides()       makes a SavedViewpoint of the
    ///         current view WITH what is hidden, ContainsVisibilityOverrides true, and it
    ///         hides the same items again when pressed, after a save and a reopen too
    ///     new SavedViewpoint(Viewpoint)                  makes one of the camera ALONE,
    ///         which opens on the whole federation, and is never used here
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
        /// Captures the current view WITH what is hidden as a saved viewpoint of that name
        /// in that folder path, 5j route B. The name goes on the object before it is
        /// copied in, so nothing has to guess which child the copy became, and the caller
        /// reads it back by name rather than trusting the add.
        /// </summary>
        public static void Capture(Document document, IList<string> folders, string name)
        {
            if (document == null || folders == null || string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A viewpoint needs a document, a folder path and a name.", "name");
            }

            using (SavedViewpoint captured = document.SavedViewpoints.CaptureRuntimeOverrides())
            {
                if (captured == null)
                {
                    throw new InvalidOperationException("CaptureRuntimeOverrides returned nothing.");
                }

                captured.DisplayName = name;

                using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException(
                            "The folder path " + string.Join("/", ToArray(folders)) + " is not there.");
                    }

                    document.SavedViewpoints.AddCopy(parent, captured);
                }
            }
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

        /// <summary>
        /// Puts a camera on the view the person sees AND on the document's current
        /// viewpoint. MEASURED on 2026-09-19, docs\history\scan.md 5l: CaptureRuntimeOverrides
        /// captures the ACTIVE VIEW, the 3D window, and not Document.CurrentViewpoint. On
        /// the first viewpoints run every viewpoint was captured on the same view, the one
        /// the window happened to show, because CurrentViewpoint.CopyFrom alone never
        /// reached the window while the run held the thread, while in the automation host,
        /// which has no window, the capture's own Viewpoint cannot even be read. So the
        /// camera goes through View.CopyViewpointFrom with JumpCut, which the API doc says
        /// jumps straight there with no collision or gravity, and the document's current
        /// viewpoint is set as well so the two agree. Where there is no active view the
        /// document's is all there is.
        /// </summary>
        public static void ApplyCamera(Document document, Viewpoint camera)
        {
            if (document == null || camera == null)
            {
                return;
            }

            View view = document.ActiveView;

            if (view != null)
            {
                view.CopyViewpointFrom(camera, ViewChange.JumpCut);
            }

            document.CurrentViewpoint.CopyFrom(camera);
        }

        /// <summary>A copy of the camera the person sees, or the document's where there is no view. The caller disposes it.</summary>
        public static Viewpoint ReadCamera(Document document)
        {
            View view = document.ActiveView;
            return view != null ? view.CreateViewpointCopy() : document.CurrentViewpoint.CreateCopy();
        }

        /// <summary>
        /// How far the camera recorded on the viewpoint at that path sits from the camera
        /// the writer asked for, in document units, or MINUS ONE where the viewpoint or its
        /// camera could not be read. This is the read back that proves a viewpoint opens
        /// where its clash is, and a distance is a number a person can check rather than a
        /// trust.
        /// </summary>
        public static double CameraDistance(Document document, IList<string> folders, string name, Viewpoint camera)
        {
            if (document == null || folders == null || string.IsNullOrEmpty(name) || camera == null)
            {
                return -1;
            }

            using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
            {
                if (parent == null)
                {
                    return -1;
                }

                using (SavedViewpoint found = FindLeafItem(parent, name))
                {
                    if (found == null)
                    {
                        return -1;
                    }

                    using (Viewpoint recorded = found.Viewpoint)
                    {
                        if (recorded == null)
                        {
                            return -1;
                        }

                        Point3D a = recorded.Position;
                        Point3D b = camera.Position;
                        double dx = a.X - b.X;
                        double dy = a.Y - b.Y;
                        double dz = a.Z - b.Z;
                        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    }
                }
            }
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
