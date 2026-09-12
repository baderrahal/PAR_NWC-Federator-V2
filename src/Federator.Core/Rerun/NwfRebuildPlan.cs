using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// What rebuilding a CHANGED NWF from the scan means, read off the comparison.
    ///
    /// WHY THIS EXISTS. On 2026-09-07 six groups in one run had an NWF on disk built from
    /// an older folder that held fewer files than the scan. 1B06BC held 4 of 5, 1B06K1
    /// held 1 of 4. The tool read CHANGED and left every one alone, so nothing rebuilt
    /// them and the NWDs went out missing models. Bader decided, Q22: rebuild the NWF
    /// from the scan folder by itself, keep the tests saved inside it, and say what was
    /// added, what moved and what was removed.
    ///
    /// A file on both sides under a different folder is a MOVE, not an addition and a
    /// removal, because that is what happened to it. A file only in the scan is an
    /// addition. A file only in the NWF is a removal. The files to append are the scan,
    /// in scan order, because the scan is what the person asked for.
    /// </summary>
    public sealed class NwfRebuildPlan
    {
        private NwfRebuildPlan(
            bool rebuild,
            IList<string> files,
            IList<string> added,
            IList<NwfMove> moved,
            IList<string> removed,
            IList<string> unchanged)
        {
            Rebuild = rebuild;
            Files = new ReadOnlyCollection<string>(files);
            Added = new ReadOnlyCollection<string>(added);
            Moved = new ReadOnlyCollection<NwfMove>(moved);
            Removed = new ReadOnlyCollection<string>(removed);
            Unchanged = new ReadOnlyCollection<string>(unchanged);
        }

        /// <summary>True only for a CHANGED comparison. Build and Open need no rebuild.</summary>
        public bool Rebuild { get; private set; }

        /// <summary>Every file to append, in scan order. Empty where nothing is rebuilt.</summary>
        public ReadOnlyCollection<string> Files { get; private set; }

        /// <summary>In the scan and not in the NWF under any folder.</summary>
        public ReadOnlyCollection<string> Added { get; private set; }

        /// <summary>Same file name on both sides under a different folder.</summary>
        public ReadOnlyCollection<NwfMove> Moved { get; private set; }

        /// <summary>In the NWF and not in the scan under any folder.</summary>
        public ReadOnlyCollection<string> Removed { get; private set; }

        public ReadOnlyCollection<string> Unchanged { get; private set; }

        public static NwfRebuildPlan From(NwfComparison comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }

            if (comparison.Decision != RerunDecision.Changed)
            {
                return new NwfRebuildPlan(
                    false,
                    new List<string>(),
                    new List<string>(),
                    new List<NwfMove>(),
                    new List<string>(),
                    new List<string>(comparison.Unchanged));
            }

            // A removed file whose name reappears in the added list is the same model
            // under a new folder. Each removed path is matched at most once, in order,
            // so two files of one name on one side still come out right.
            List<string> removedLeft = new List<string>(comparison.Removed);
            List<string> added = new List<string>();
            List<NwfMove> moved = new List<NwfMove>();

            foreach (string file in comparison.Added)
            {
                int match = IndexOfLeaf(removedLeft, LeafOf(file));

                if (match < 0)
                {
                    added.Add(file);
                }
                else
                {
                    moved.Add(new NwfMove(removedLeft[match], file));
                    removedLeft.RemoveAt(match);
                }
            }

            return new NwfRebuildPlan(
                true,
                new List<string>(comparison.Scanned),
                added,
                moved,
                removedLeft,
                new List<string>(comparison.Unchanged));
        }

        /// <summary>
        /// The lines the log carries, one shape. The heading names the NWF and the three
        /// counts, then one line per file saying added, moved or removed. Empty where
        /// nothing is rebuilt, so a Build or an Open writes nothing here.
        /// </summary>
        public IList<string> Lines(string nwfPath)
        {
            List<string> lines = new List<string>();

            if (!Rebuild)
            {
                return lines;
            }

            lines.Add("REBUILT  " + nwfPath + "  " + Summary());

            foreach (string file in Added)
            {
                lines.Add("         added   " + file);
            }

            foreach (NwfMove move in Moved)
            {
                lines.Add("         moved   " + move.Name + "  from " + move.FromFolder + "  to " + move.ToFolder);
            }

            foreach (string file in Removed)
            {
                lines.Add("         removed " + file);
            }

            return lines;
        }

        /// <summary>The three counts in one phrase, in the order the log always uses.</summary>
        public string Summary()
        {
            return Added.Count + " added, " + Moved.Count + " moved, " + Removed.Count + " removed";
        }

        /// <summary>
        /// The one line that says what became of the tests saved in the NWF. Three
        /// numbers, read off the document and never assumed: how many were read before
        /// the clear, how many the document held straight after the clear and the
        /// appends, and how many it held once the copy was put back. Whether the clear
        /// keeps them is UNKNOWN until a run, so the line says which of the two happened.
        /// </summary>
        public static string SavedTestsLine(int readBefore, int afterClear, int afterRestore)
        {
            if (readBefore <= 0)
            {
                return "         saved tests: none, the NWF held no clash test";
            }

            if (afterClear >= readBefore)
            {
                return "         saved tests kept: " + readBefore + " read before the clear, "
                    + afterClear + " still in the document after it, nothing to put back";
            }

            if (afterRestore >= readBefore)
            {
                return "         saved tests kept: " + readBefore + " read before the clear, "
                    + afterClear + " left after it, " + afterRestore + " put back from the copy";
            }

            return "         saved tests LOST: " + readBefore + " read before the clear, "
                + afterClear + " left after it, " + afterRestore
                + " after putting the copy back. The NWF on disk was NOT saved over, so it keeps them";
        }

        /// <summary>True when every test read before the clear is in the document at the end.</summary>
        public static bool SavedTestsKept(int readBefore, int afterRestore)
        {
            return readBefore <= 0 || afterRestore >= readBefore;
        }

        /// <summary>
        /// The one line that says what became of the selection sets across the rebuild.
        /// F29. Three numbers read off the document by walking the sets tree: before the
        /// clear, after the appends, and after the copy was put back. The sets are put
        /// back on their own count, whether or not the tests dropped, because a test side
        /// points at a set and a document that lost its sets runs every test against
        /// nothing.
        /// </summary>
        public static string SetsLine(int before, int afterAppends, int afterRestore)
        {
            return "SETS     before clear " + before + ", after appends " + afterAppends
                + ", after restore " + afterRestore
                + (SetsKept(before, afterRestore) ? string.Empty : ". LOST, the NWF on disk was NOT saved over, so it keeps them");
        }

        /// <summary>True when every set counted before the clear is in the document at the end.</summary>
        public static bool SetsKept(int before, int afterRestore)
        {
            return before <= 0 || afterRestore >= before;
        }

        /// <summary>True when the sets have to be put back, which is any drop in the count.</summary>
        public static bool SetsNeedRestoring(int before, int afterAppends)
        {
            return before > 0 && afterAppends < before;
        }

        private static int IndexOfLeaf(IList<string> paths, string leaf)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (string.Equals(LeafOf(paths[i]), leaf, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        internal static string LeafOf(string path)
        {
            try
            {
                string leaf = Path.GetFileName(path);
                return string.IsNullOrEmpty(leaf) ? path : leaf;
            }
            catch (ArgumentException)
            {
                return path;
            }
        }

        internal static string FolderOf(string path)
        {
            try
            {
                string folder = Path.GetDirectoryName(path);
                return string.IsNullOrEmpty(folder) ? path : folder;
            }
            catch (ArgumentException)
            {
                return path;
            }
        }

        public override string ToString()
        {
            return (Rebuild ? "rebuild, " : "no rebuild, ") + Summary();
        }
    }

    /// <summary>One file that kept its name and changed folder between the NWF and the scan.</summary>
    public sealed class NwfMove
    {
        internal NwfMove(string from, string to)
        {
            From = from ?? string.Empty;
            To = to ?? string.Empty;
        }

        /// <summary>The path the NWF held.</summary>
        public string From { get; private set; }

        /// <summary>The path the scan found.</summary>
        public string To { get; private set; }

        public string Name
        {
            get { return NwfRebuildPlan.LeafOf(To); }
        }

        public string FromFolder
        {
            get { return NwfRebuildPlan.FolderOf(From); }
        }

        public string ToFolder
        {
            get { return NwfRebuildPlan.FolderOf(To); }
        }

        public override string ToString()
        {
            return Name + " from " + FromFolder + " to " + ToFolder;
        }
    }
}
