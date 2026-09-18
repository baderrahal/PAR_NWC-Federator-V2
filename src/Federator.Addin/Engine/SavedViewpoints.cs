using System;
using Autodesk.Navisworks.Api;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The saved viewpoints in the open document.
    ///
    /// EVERYTHING IN THIS FILE RESTS ON ONE ASSUMPTION AND THIS IS THE ONE PLACE IT IS MADE.
    ///
    /// The repo has never touched this collection. docs\history\scan.md names Viewpoint,
    /// DocumentCurrentViewpoint, View.CreateViewpointCopy and ClashResult.HasSavedViewpoint,
    /// and section 4k says nothing here writes a viewpoint into the NWF. Nothing anywhere in
    /// that file records DocumentSavedViewpoints at all, so as of 2026-09-18 the shape of it
    /// is UNKNOWN and was not read off a DLL.
    ///
    /// WHAT IS ASSUMED, and it is assumed because every other document part measured on this
    /// install follows it, not because anyone read it:
    ///
    ///     Document.SavedViewpoints            is a DocumentSavedViewpoints
    ///     DocumentSavedViewpoints.RootItem    is a GroupItem
    ///     GroupItem.Children                  is a SavedItemCollection, which it is, measured
    ///     a leaf under it                     is a SavedViewpoint, and a branch a FolderItem
    ///
    /// tools\probes\probe-model-remove.ps1 is F50's probe and tools\probes\probe-viewpoints.ps1
    /// is the one that answers this. Until it has been run on a machine with Navisworks, a
    /// build error anywhere in this file means the assumption above was wrong, and this file
    /// is deliberately the only place such an error can land.
    ///
    /// Nothing here creates or names a viewpoint. F52 does that, and it builds on whatever
    /// the probe comes back with.
    /// </summary>
    public static class SavedViewpoints
    {
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
        /// Every leaf under this group, descending folders. A count and never a handle,
        /// because a handle onto anything the document owns dies the moment the document
        /// replaces the object behind it, which a clear does.
        /// </summary>
        private static int CountUnder(GroupItem parent)
        {
            if (parent == null)
            {
                return 0;
            }

            SavedItemCollection children = parent.Children;

            if (children == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    GroupItem folder = child as GroupItem;

                    if (folder != null)
                    {
                        count += CountUnder(folder);
                        continue;
                    }

                    count++;
                }
            }

            return count;
        }
    }
}
