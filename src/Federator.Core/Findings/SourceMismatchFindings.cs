using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Naming;

namespace Federator.Core.Findings
{
    /// <summary>
    /// One model inside an opened document: the NWC the federation points at, and the
    /// name of the thing the NWC was published from, which for these projects is a Revit
    /// container in Autodesk Docs.
    /// </summary>
    public sealed class SourcePair
    {
        public SourcePair(string groupBuilding, string nwcPath, string sourceName)
        {
            GroupBuilding = groupBuilding;
            NwcPath = nwcPath;
            SourceName = sourceName;
        }

        /// <summary>The building code of the group this model was federated into.</summary>
        public string GroupBuilding { get; private set; }

        /// <summary>Model.FileName, the NWC this run appended or the NWF points at.</summary>
        public string NwcPath { get; private set; }

        /// <summary>
        /// Model.SourceFileName, for example
        /// Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt
        /// </summary>
        public string SourceName { get; private set; }

        public override string ToString()
        {
            return NwcPath + " from " + SourceName;
        }
    }

    /// <summary>
    /// Where the building code inside the Revit source name is not the building code on
    /// the NWC that was published from it.
    ///
    /// This is information. It does not block a group, does not unpick a grouping and
    /// does not merge anything. Bader decides.
    ///
    /// Both codes are read with the same parser used on the NWC names, never a separate
    /// rule, so the split character and the part positions stay settings rather than
    /// becoming constants in a second place.
    /// </summary>
    public sealed class SourceMismatchFindings
    {
        public const string SourceMismatchLabel = "SOURCE MISMATCH";
        public const string SharedSourceLabel = "SHARED SOURCE";

        public const string NothingOdd =
            "Every NWC carries a Revit source whose building code matches its own, and no "
            + "two groups are fed by the same Revit building code.";

        private SourceMismatchFindings(
            IList<ScanFinding> all,
            int pairsRead,
            int unreadableSourceNames,
            int unreadableNwcNames)
        {
            All = new ReadOnlyCollection<ScanFinding>(all);
            PairsRead = pairsRead;
            UnreadableSourceNames = unreadableSourceNames;
            UnreadableNwcNames = unreadableNwcNames;
        }

        public ReadOnlyCollection<ScanFinding> All { get; private set; }

        /// <summary>Distinct NWC and source pairs that were compared.</summary>
        public int PairsRead { get; private set; }

        /// <summary>
        /// Source names the parser could not read, so there was no code to compare. Counted
        /// rather than reported one by one, and never guessed at.
        /// </summary>
        public int UnreadableSourceNames { get; private set; }

        public int UnreadableNwcNames { get; private set; }

        public bool Any
        {
            get { return All.Count > 0; }
        }

        public int Count
        {
            get { return All.Count; }
        }

        public IList<ScanFinding> OfKind(FindingKind kind)
        {
            List<ScanFinding> found = new List<ScanFinding>();

            foreach (ScanFinding finding in All)
            {
                if (finding.Kind == kind)
                {
                    found.Add(finding);
                }
            }

            return found;
        }

        public static SourceMismatchFindings From(IEnumerable<SourcePair> pairs)
        {
            return From(pairs, new ContainerNameSettings());
        }

        public static SourceMismatchFindings From(
            IEnumerable<SourcePair> pairs, ContainerNameSettings settings)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException("pairs");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            List<ScanFinding> findings = new List<ScanFinding>();

            // First seen order everywhere, so two runs over the same log read the same.
            List<Comparison> compared = new List<Comparison>();
            HashSet<string> seenPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int unreadableSource = 0;
            int unreadableNwc = 0;

            foreach (SourcePair pair in pairs)
            {
                if (pair == null)
                {
                    continue;
                }

                // One Revit container can be published to several NWC, and a group can hold
                // the same pair several times. Each distinct pair is reported once.
                if (!seenPairs.Add(Key(pair)))
                {
                    continue;
                }

                ParsedContainerName nwc = ContainerName.Parse(pair.NwcPath, settings);

                if (!nwc.IsReadable)
                {
                    // Already reported by the scan, which is where an unreadable NWC name
                    // belongs. Counted here so the block accounts for every pair it saw.
                    unreadableNwc++;
                    continue;
                }

                ParsedContainerName source = ContainerName.Parse(pair.SourceName, settings);

                if (!source.IsReadable)
                {
                    unreadableSource++;
                    continue;
                }

                compared.Add(new Comparison(pair, nwc, source));
            }

            AddMismatches(compared, findings);
            AddSharedSources(compared, findings);

            return new SourceMismatchFindings(
                findings, compared.Count, unreadableSource, unreadableNwc);
        }

        /// <summary>
        /// A separator that cannot turn up inside a path or a building code, so two
        /// different pairs can never build the same key.
        /// </summary>
        private const string KeySeparator = "\u001F";

        private static string Key(SourcePair pair)
        {
            return (pair.GroupBuilding ?? string.Empty) + KeySeparator
                + (pair.NwcPath ?? string.Empty) + KeySeparator
                + (pair.SourceName ?? string.Empty);
        }

        /// <summary>
        /// Where the code on the NWC and the code inside its Revit source are different.
        /// Both are named, and neither is treated as the right one.
        /// </summary>
        private static void AddMismatches(IList<Comparison> compared, IList<ScanFinding> findings)
        {
            foreach (Comparison one in compared)
            {
                if (string.Equals(one.NwcBuilding, one.SourceBuilding, StringComparison.Ordinal))
                {
                    continue;
                }

                findings.Add(new ScanFinding(
                    FindingKind.SourceMismatch,
                    SourceMismatchLabel,
                    "NWC " + one.NwcBuilding + " was published from Revit " + one.SourceBuilding,
                    "The NWC name says building " + one.NwcBuilding
                        + " and the Revit container it came from says building " + one.SourceBuilding
                        + ". The group is built from the NWC name, which is unchanged. "
                        + "Neither code is assumed right.",
                    new List<string> { one.NwcBuilding, one.SourceBuilding },
                    new List<string> { one.Nwc.Stem, "from " + one.Source.Stem }));
            }
        }

        /// <summary>
        /// One Revit building code feeding more than one group. That means one building
        /// has been split in two by a naming error, so it is worth saying on its own
        /// rather than leaving it to be spotted across two mismatch lines.
        /// </summary>
        private static void AddSharedSources(IList<Comparison> compared, IList<ScanFinding> findings)
        {
            Dictionary<string, List<string>> groupsBySource =
                new Dictionary<string, List<string>>(StringComparer.Ordinal);
            List<string> sourceOrder = new List<string>();

            foreach (Comparison one in compared)
            {
                List<string> groups;

                if (!groupsBySource.TryGetValue(one.SourceBuilding, out groups))
                {
                    groups = new List<string>();
                    groupsBySource.Add(one.SourceBuilding, groups);
                    sourceOrder.Add(one.SourceBuilding);
                }

                if (!groups.Contains(one.GroupBuilding))
                {
                    groups.Add(one.GroupBuilding);
                }
            }

            foreach (string source in sourceOrder)
            {
                List<string> groups = groupsBySource[source];

                if (groups.Count < 2)
                {
                    continue;
                }

                List<string> named = new List<string>(groups);
                named.Sort(StringComparer.Ordinal);

                List<string> buildings = new List<string> { source };
                buildings.AddRange(named);

                findings.Add(new ScanFinding(
                    FindingKind.SharedSourceBuilding,
                    SharedSourceLabel,
                    "Revit " + source + " feeds " + named.Count + " groups: "
                        + string.Join(", ", named.ToArray()),
                    "One Revit building is being federated into more than one output, which "
                        + "reads as one building split in two by a naming error. Nothing is "
                        + "merged and no group is unpicked.",
                    buildings,
                    null));
            }
        }

        /// <summary>
        /// The block this contributes to the log. Nothing odd is one line, not an empty
        /// panel, the same as the scan findings.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add("NWC and Revit source pairs compared: " + PairsRead);

            if (UnreadableSourceNames > 0)
            {
                lines.Add(UnreadableSourceNames
                    + (UnreadableSourceNames == 1 ? " source name" : " source names")
                    + " could not be read with the same parser, so no code could be compared");
            }

            if (UnreadableNwcNames > 0)
            {
                lines.Add(UnreadableNwcNames
                    + (UnreadableNwcNames == 1 ? " NWC name" : " NWC names")
                    + " could not be read, which the scan already reports");
            }

            if (!Any)
            {
                lines.Add(NothingOdd);
                return lines;
            }

            foreach (ScanFinding finding in All)
            {
                lines.Add(finding.Label.PadRight(18) + finding.Headline);
                lines.Add(new string(' ', 18) + finding.Detail);

                foreach (string file in finding.Files)
                {
                    lines.Add(new string(' ', 18) + file);
                }
            }

            return lines;
        }

        /// <summary>One NWC name and its Revit source, both already read.</summary>
        private sealed class Comparison
        {
            internal Comparison(SourcePair pair, ParsedContainerName nwc, ParsedContainerName source)
            {
                Pair = pair;
                Nwc = nwc;
                Source = source;
            }

            internal SourcePair Pair { get; private set; }

            internal ParsedContainerName Nwc { get; private set; }

            internal ParsedContainerName Source { get; private set; }

            internal string NwcBuilding
            {
                get { return Nwc.Building; }
            }

            internal string SourceBuilding
            {
                get { return Source.Building; }
            }

            /// <summary>
            /// The group this model was federated into. Falls back to the code on the NWC
            /// itself, because that is what the grouping used.
            /// </summary>
            internal string GroupBuilding
            {
                get
                {
                    return string.IsNullOrEmpty(Pair.GroupBuilding) ? NwcBuilding : Pair.GroupBuilding;
                }
            }
        }
    }
}
