using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Federator.Core.Rerun
{
    /// <summary>What to do with a group on this run.</summary>
    public enum RerunDecision
    {
        /// <summary>No NWF at the output path. Build it from the scanned files.</summary>
        Build,

        /// <summary>The NWF is already pointing at exactly this group. Open it and leave it alone.</summary>
        Open,

        /// <summary>The NWF points at a different set of files. Report it and touch nothing.</summary>
        Changed
    }

    /// <summary>
    /// The file list inside an existing NWF against the group the scan found. The NWF is
    /// the record, so this reads the list out of it rather than out of any side file this
    /// tool wrote.
    ///
    /// Paths are compared whole and without case, which is how Windows treats them. A file
    /// that kept its name and moved folder shows as added and removed, and is also called
    /// out on its own, because that reads as a relocation rather than as two changes.
    /// </summary>
    public sealed class NwfComparison
    {
        private NwfComparison(
            RerunDecision decision,
            IList<string> inNwf,
            IList<string> scanned,
            IList<string> added,
            IList<string> removed,
            IList<string> moved,
            IList<string> unchanged)
        {
            Decision = decision;
            InNwf = new ReadOnlyCollection<string>(inNwf);
            Scanned = new ReadOnlyCollection<string>(scanned);
            Added = new ReadOnlyCollection<string>(added);
            Removed = new ReadOnlyCollection<string>(removed);
            Moved = new ReadOnlyCollection<string>(moved);
            Unchanged = new ReadOnlyCollection<string>(unchanged);
        }

        public RerunDecision Decision { get; private set; }

        /// <summary>The files the NWF already points at. Empty when there is no NWF.</summary>
        public ReadOnlyCollection<string> InNwf { get; private set; }

        public ReadOnlyCollection<string> Scanned { get; private set; }

        /// <summary>In the scan and not in the NWF.</summary>
        public ReadOnlyCollection<string> Added { get; private set; }

        /// <summary>In the NWF and not in the scan.</summary>
        public ReadOnlyCollection<string> Removed { get; private set; }

        /// <summary>Same file name on both sides, different folder. Also in Added and Removed.</summary>
        public ReadOnlyCollection<string> Moved { get; private set; }

        public ReadOnlyCollection<string> Unchanged { get; private set; }

        public bool Matches
        {
            get { return Decision == RerunDecision.Open; }
        }

        /// <summary>No NWF at the output path, so there is nothing to preserve.</summary>
        public static NwfComparison NoNwfYet(IEnumerable<string> scanned)
        {
            List<string> files = Normalise(scanned);

            return new NwfComparison(
                RerunDecision.Build,
                new List<string>(),
                files,
                new List<string>(files),
                new List<string>(),
                new List<string>(),
                new List<string>());
        }

        /// <summary>
        /// Compares what the NWF holds against what the scan found. An empty NWF still
        /// counts as existing, so a group whose scan is also empty matches it.
        /// </summary>
        public static NwfComparison Compare(IEnumerable<string> inNwf, IEnumerable<string> scanned)
        {
            if (inNwf == null)
            {
                throw new ArgumentNullException("inNwf");
            }

            if (scanned == null)
            {
                throw new ArgumentNullException("scanned");
            }

            List<string> existing = Normalise(inNwf);
            List<string> wanted = Normalise(scanned);

            HashSet<string> existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
            HashSet<string> wantedSet = new HashSet<string>(wanted, StringComparer.OrdinalIgnoreCase);

            List<string> added = new List<string>();
            List<string> removed = new List<string>();
            List<string> unchanged = new List<string>();

            foreach (string file in wanted)
            {
                if (existingSet.Contains(file))
                {
                    unchanged.Add(file);
                }
                else
                {
                    added.Add(file);
                }
            }

            foreach (string file in existing)
            {
                if (!wantedSet.Contains(file))
                {
                    removed.Add(file);
                }
            }

            List<string> moved = Moves(added, removed);

            RerunDecision decision = added.Count == 0 && removed.Count == 0
                ? RerunDecision.Open
                : RerunDecision.Changed;

            return new NwfComparison(decision, existing, wanted, added, removed, moved, unchanged);
        }

        /// <summary>
        /// File names that appear on both sides under a different folder. These are still
        /// counted as added and removed, this only names them so the report reads honestly.
        /// </summary>
        private static List<string> Moves(IList<string> added, IList<string> removed)
        {
            List<string> moved = new List<string>();

            if (added.Count == 0 || removed.Count == 0)
            {
                return moved;
            }

            HashSet<string> removedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in removed)
            {
                removedNames.Add(LeafOf(file));
            }

            foreach (string file in added)
            {
                string leaf = LeafOf(file);

                if (removedNames.Contains(leaf) && !moved.Contains(leaf))
                {
                    moved.Add(leaf);
                }
            }

            return moved;
        }

        private static string LeafOf(string path)
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

        /// <summary>
        /// Full paths where they can be made, in the order given, with repeats and blanks
        /// dropped. A path that cannot be expanded is kept exactly as it came.
        /// </summary>
        private static List<string> Normalise(IEnumerable<string> paths)
        {
            List<string> normalised = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (paths == null)
            {
                return normalised;
            }

            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string full = path;

                try
                {
                    full = Path.GetFullPath(path);
                }
                catch (Exception)
                {
                    // A path Windows will not expand is kept as it came, so it can still
                    // be compared and still be named in the report.
                    full = path;
                }

                if (seen.Add(full))
                {
                    normalised.Add(full);
                }
            }

            return normalised;
        }

        /// <summary>The lines this comparison contributes to the log.</summary>
        public IList<string> Lines(string nwfPath)
        {
            List<string> lines = new List<string>();

            switch (Decision)
            {
                case RerunDecision.Build:
                    lines.Add("BUILD    no NWF at " + nwfPath + ", building it from "
                        + Scanned.Count + Word(Scanned.Count, " file", " files"));
                    break;

                case RerunDecision.Open:
                    lines.Add("OPENED   " + nwfPath);
                    lines.Add("         the file list matches the scan, "
                        + Unchanged.Count + Word(Unchanged.Count, " file", " files")
                        + ", so it was not cleared and nothing was re-appended");
                    break;

                default:
                    lines.Add("CHANGED  " + nwfPath + " points at a different set of files, "
                        + "so it was left exactly as it is");
                    lines.Add("         " + Unchanged.Count + " unchanged, "
                        + Added.Count + " added, " + Removed.Count + " removed");

                    foreach (string file in Added)
                    {
                        lines.Add("         added   " + file);
                    }

                    foreach (string file in Removed)
                    {
                        lines.Add("         removed " + file);
                    }

                    foreach (string leaf in Moved)
                    {
                        lines.Add("         note    " + leaf
                            + " is on both sides under a different folder, which reads as a move");
                    }

                    break;
            }

            return lines;
        }

        private static string Word(int count, string one, string many)
        {
            return count == 1 ? one : many;
        }

        public override string ToString()
        {
            return Decision + ", " + Added.Count + " added, " + Removed.Count + " removed";
        }
    }
}
