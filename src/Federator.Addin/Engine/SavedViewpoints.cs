using System;
using System.Collections.Generic;
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
        /// Whether this build knows how to put a viewpoint into the NWF. FALSE until
        /// tools\probes\probe-viewpoints.ps1 has been run and ShowOnly and Add below have
        /// been written against what it found.
        ///
        /// WHY A FLAG AND NOT JUST THE THROW. A step this tool cannot do is not a step that
        /// failed. Wiring the builder into the run while the two methods below throw would
        /// report every group FAILED over a feature that was never attempted, which is
        /// exactly the fault that once reported a clean 22 group run as failed because an
        /// NWD nobody asked for was missing. So the engine asks for viewpoints only when
        /// this reads true, the judgement is told they were not requested, and the log says
        /// plainly that the measurement is outstanding.
        ///
        /// This is the ONE line to change when the probe comes back, and the Core half,
        /// the plan, the VIEWS block and the judgement rule are all written and tested
        /// behind it.
        /// </summary>
        public static bool CanBuild
        {
            get { return false; }
        }

        /// <summary>What the log says while CanBuild is false, so the run is never silent about it.</summary>
        public static string WhyNotYet()
        {
            return "VIEWS    not attempted. How a saved viewpoint folder is made and a viewpoint "
                + "put in it was never read off the installed DLL, so nothing was written into the "
                + "NWF. See docs\\history\\scan.md section 5b and run tools\\probes\\probe-viewpoints.ps1";
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
        /// Whether a viewpoint of that name is already in that folder. UNMEASURED, see the
        /// note at the top of this file. False where the folder is not there at all, which
        /// is the ordinary first run case and not a failure.
        /// </summary>
        public static bool Exists(Document document, string folder, string name)
        {
            if (document == null || string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(name))
            {
                return false;
            }

            using (GroupItem root = document.SavedViewpoints.RootItem)
            using (GroupItem parent = FindFolder(root, folder))
            {
                return parent != null && FindLeaf(parent, name);
            }
        }

        /// <summary>
        /// Shows one discipline and hides every other named one, so the viewpoint saved
        /// straight after carries that state.
        ///
        /// UNMEASURED. How a model item is hidden was never read off the DLL, and neither
        /// was whether a viewpoint records hidden state at all. If it does not, this whole
        /// feature is a different shape and the probe is what says so. It throws rather
        /// than doing nothing, because a viewpoint that shows every discipline is not the
        /// thing that was asked for and would read as a working feature.
        /// </summary>
        public static void ShowOnly(Document document, string shows, IList<string> hides)
        {
            if (document == null || string.IsNullOrEmpty(shows))
            {
                throw new ArgumentException("A viewpoint needs a discipline to show.", "shows");
            }

            throw new NotSupportedException(
                "How a discipline is shown and the others hidden was never read off the installed "
                    + "DLL. docs\\history\\scan.md section 5b records the question and "
                    + "tools\\probes\\probe-viewpoints.ps1 answers it. Run that probe and this "
                    + "method is the one place that changes. Nothing was written into the NWF.");
        }

        /// <summary>
        /// Puts a viewpoint of the current view into that folder, making the folder where it
        /// is not there. UNMEASURED, see the note at the top of this file.
        /// </summary>
        public static void Add(Document document, string folder, string name)
        {
            if (document == null || string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A viewpoint needs a folder and a name.", "name");
            }

            throw new NotSupportedException(
                "How a saved viewpoint folder is made and a viewpoint added to it was never read "
                    + "off the installed DLL. docs\\history\\scan.md section 5b records the "
                    + "question and tools\\probes\\probe-viewpoints.ps1 answers it. Run that "
                    + "probe and this method is the one place that changes. Nothing was written "
                    + "into the NWF.");
        }

        /// <summary>The folder of that name directly under this group, or null.</summary>
        private static GroupItem FindFolder(GroupItem parent, string folder)
        {
            if (parent == null)
            {
                return null;
            }

            SavedItemCollection children = parent.Children;

            if (children == null)
            {
                return null;
            }

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                GroupItem asFolder = child as GroupItem;

                if (asFolder != null && string.Equals(child.DisplayName, folder, StringComparison.Ordinal))
                {
                    return asFolder;
                }

                child.Dispose();
            }

            return null;
        }

        /// <summary>Whether a leaf of that name sits directly under this group.</summary>
        private static bool FindLeaf(GroupItem parent, string name)
        {
            SavedItemCollection children = parent.Children;

            if (children == null)
            {
                return false;
            }

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    if (!(child is GroupItem)
                        && string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
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
