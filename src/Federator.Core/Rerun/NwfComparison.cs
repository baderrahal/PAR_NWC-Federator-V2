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

        /// <summary>
        /// The NWF points at a different set of files. This is what the comparison reads.
        /// Since F24 the engine rebuilds such a group from the scan and ends it as Rebuilt,
        /// so a group only ENDS as Changed when the rebuild never started.
        /// </summary>
        Changed,

        /// <summary>
        /// The NWF pointed at a different set of files and was cleared and rebuilt from the
        /// scan folder, keeping the tests saved inside it. Bader decided this on 2026-09-07,
        /// Q22, after six groups in one run were left alone with NWFs missing files.
        /// </summary>
        Rebuilt
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
            IList<string> unchanged)
        {
            Decision = decision;
            InNwf = new ReadOnlyCollection<string>(inNwf);
            Scanned = new ReadOnlyCollection<string>(scanned);
            Added = new ReadOnlyCollection<string>(added);
            Removed = new ReadOnlyCollection<string>(removed);
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

        public ReadOnlyCollection<string> Unchanged { get; private set; }

        internal bool Matches
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

            RerunDecision decision = added.Count == 0 && removed.Count == 0
                ? RerunDecision.Open
                : RerunDecision.Changed;

            return new NwfComparison(decision, existing, wanted, added, removed, unchanged);
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
                    // The heading and the counts only. Which file was added, which moved
                    // and which was removed is said once, by NwfRebuildPlan, in the
                    // REBUILT block that follows, so a move never reads as an addition
                    // and a removal here and a move there.
                    lines.Add("CHANGED  " + nwfPath + " points at a different set of files");
                    lines.Add("         " + Unchanged.Count + " unchanged, "
                        + Added.Count + " added, " + Removed.Count + " removed, "
                        + "so it is rebuilt from the scan folder");
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
