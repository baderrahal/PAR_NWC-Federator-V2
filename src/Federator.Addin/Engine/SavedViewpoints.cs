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
    ///     DocumentModels.SetHidden, ResetAllHidden, ResetAllHiddenToModelState
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
        /// viewpoint captured next carries exactly that. Hidden state is put back to what
        /// the file held through RestoreHiddenState when the group's writing ends.
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
                    if (!keep.Contains(i))
                    {
                        hide.Add(document.Models[i].RootItem);
                    }
                }

                if (hide.Count > 0)
                {
                    document.Models.SetHidden(hide, true);
                }

                return hide.Count;
            }
        }

        /// <summary>Back to the hidden state the file held, so the NWF saved next carries what it always did.</summary>
        public static void RestoreHiddenState(Document document)
        {
            if (document != null)
            {
                document.Models.ResetAllHiddenToModelState();
            }
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
}
