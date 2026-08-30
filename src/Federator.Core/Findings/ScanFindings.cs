using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Federator.Core.Grouping;
using Federator.Core.Naming;

namespace Federator.Core.Findings
{
    /// <summary>
    /// What the scan noticed about a run, worked out from the run itself. Nothing here
    /// is compared against a list of known building codes or known disciplines, because
    /// every project differs. The run is its own reference.
    /// </summary>
    public sealed class ScanFindings
    {
        public const string OddShapeLabel = "ODD SHAPE";
        public const string NearMatchLabel = "NEAR MATCH";
        public const string SingleDisciplineLabel = "SINGLE DISCIPLINE";
        public const string MissingDisciplinesLabel = "MISSING";

        /// <summary>What a shape uses for a letter.</summary>
        public const char LetterMark = 'A';

        /// <summary>What a shape uses for a digit.</summary>
        public const char DigitMark = '9';

        private ScanFindings(IList<ScanFinding> all, IList<string> disciplinesInRun)
        {
            All = new ReadOnlyCollection<ScanFinding>(all);
            DisciplinesInRun = new ReadOnlyCollection<string>(disciplinesInRun);
        }

        public ReadOnlyCollection<ScanFinding> All { get; private set; }

        /// <summary>Every discipline seen anywhere in this run, sorted. Read, never assumed.</summary>
        public ReadOnlyCollection<string> DisciplinesInRun { get; private set; }

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

        /// <summary>
        /// The letter and digit pattern of a code. 1B06PK becomes 9A99AA. Anything that
        /// is neither a letter nor a digit is kept as itself, so a stray separator shows.
        /// </summary>
        public static string Shape(string code)
        {
            if (code == null)
            {
                return string.Empty;
            }

            StringBuilder shape = new StringBuilder(code.Length);

            foreach (char c in code)
            {
                if (char.IsDigit(c))
                {
                    shape.Append(DigitMark);
                }
                else if (char.IsLetter(c))
                {
                    shape.Append(LetterMark);
                }
                else
                {
                    shape.Append(c);
                }
            }

            return shape.ToString();
        }

        /// <summary>
        /// True when two codes are one character apart, counting a swap, an extra
        /// character or a missing one. Compared ordinally, so a difference in case counts.
        /// </summary>
        public static bool IsOneCharacterApart(string left, string right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (string.Equals(left, right, StringComparison.Ordinal))
            {
                return false;
            }

            int difference = left.Length - right.Length;

            if (difference < -1 || difference > 1)
            {
                return false;
            }

            if (difference == 0)
            {
                int changed = 0;

                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        changed++;

                        if (changed > 1)
                        {
                            return false;
                        }
                    }
                }

                return changed == 1;
            }

            string longer = difference == 1 ? left : right;
            string shorter = difference == 1 ? right : left;

            // Walk both, allowing exactly one skip in the longer one.
            int shortIndex = 0;
            bool skipped = false;

            for (int longIndex = 0; longIndex < longer.Length; longIndex++)
            {
                if (shortIndex < shorter.Length && longer[longIndex] == shorter[shortIndex])
                {
                    shortIndex++;
                    continue;
                }

                if (skipped)
                {
                    return false;
                }

                skipped = true;
            }

            return shortIndex == shorter.Length;
        }

        public static ScanFindings From(BuildingGroupingResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            return From(result.Groups);
        }

        /// <summary>
        /// Works only from the groups that would actually run. A blocked group is already
        /// reported by the grouping step and is not going to federate, so counting it here
        /// would put disciplines into the run that no output ever sees.
        /// </summary>
        public static ScanFindings From(IEnumerable<BuildingGroup> groups)
        {
            if (groups == null)
            {
                throw new ArgumentNullException("groups");
            }

            List<BuildingGroup> all = new List<BuildingGroup>(groups);
            List<ScanFinding> findings = new List<ScanFinding>();
            List<string> disciplinesInRun = DisciplinesAcross(all);

            AddOddShapes(all, findings);
            AddNearMatches(all, findings);
            AddDisciplineFindings(all, disciplinesInRun, findings);

            return new ScanFindings(findings, disciplinesInRun);
        }

        // ---------- job 1, odd shapes ----------

        private static void AddOddShapes(IList<BuildingGroup> groups, IList<ScanFinding> findings)
        {
            Dictionary<string, List<BuildingGroup>> byShape =
                new Dictionary<string, List<BuildingGroup>>(StringComparer.Ordinal);
            List<string> shapeOrder = new List<string>();

            foreach (BuildingGroup group in groups)
            {
                string shape = Shape(group.Building);
                List<BuildingGroup> bucket;

                if (!byShape.TryGetValue(shape, out bucket))
                {
                    bucket = new List<BuildingGroup>();
                    byShape.Add(shape, bucket);
                    shapeOrder.Add(shape);
                }

                bucket.Add(group);
            }

            // Only call a code odd when some other shape is actually shared. With every
            // shape held once there is no majority to differ from, and flagging all of
            // them would say nothing.
            bool anyShapeShared = false;

            foreach (string shape in shapeOrder)
            {
                if (byShape[shape].Count > 1)
                {
                    anyShapeShared = true;
                    break;
                }
            }

            if (!anyShapeShared)
            {
                return;
            }

            foreach (string shape in shapeOrder)
            {
                List<BuildingGroup> holders = byShape[shape];

                if (holders.Count != 1)
                {
                    continue;
                }

                BuildingGroup odd = holders[0];
                List<string> otherShapes = new List<string>();

                foreach (string other in shapeOrder)
                {
                    if (!string.Equals(other, shape, StringComparison.Ordinal))
                    {
                        otherShapes.Add(other + " x" + byShape[other].Count);
                    }
                }

                otherShapes.Sort(StringComparer.Ordinal);

                findings.Add(new ScanFinding(
                    FindingKind.OddShape,
                    OddShapeLabel,
                    odd.Building + " is the only code shaped " + shape,
                    "Every other code in this run has a different shape: "
                        + string.Join(", ", otherShapes.ToArray())
                        + ". The group still runs, this is only worth a look.",
                    new List<string> { odd.Building },
                    FileNames(odd)));
            }
        }

        // ---------- job 2, near matches ----------

        private static void AddNearMatches(IList<BuildingGroup> groups, IList<ScanFinding> findings)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = i + 1; j < groups.Count; j++)
                {
                    BuildingGroup left = groups[i];
                    BuildingGroup right = groups[j];

                    if (!IsOneCharacterApart(left.Building, right.Building))
                    {
                        continue;
                    }

                    findings.Add(new ScanFinding(
                        FindingKind.NearMatch,
                        NearMatchLabel,
                        left.Building + " and " + right.Building + " differ by one character",
                        left.Building + " holds " + left.FileCount + FilesWord(left.FileCount)
                            + " and " + right.Building + " holds " + right.FileCount
                            + FilesWord(right.FileCount)
                            + ". They are kept apart. Nothing is merged and neither is assumed right.",
                        new List<string> { left.Building, right.Building },
                        null));
                }
            }
        }

        // ---------- job 3, what each group is missing ----------

        private static void AddDisciplineFindings(
            IList<BuildingGroup> groups, IList<string> disciplinesInRun, IList<ScanFinding> findings)
        {
            foreach (BuildingGroup group in groups)
            {
                if (group.Disciplines.Count == 1)
                {
                    findings.Add(new ScanFinding(
                        FindingKind.SingleDiscipline,
                        SingleDisciplineLabel,
                        group.Building + " holds only " + group.Disciplines[0],
                        "A group with one discipline has nothing to clash against. "
                            + "This run has " + string.Join(", ", new List<string>(disciplinesInRun).ToArray())
                            + ".",
                        new List<string> { group.Building },
                        FileNames(group)));

                    continue;
                }

                List<string> missing = Missing(group, disciplinesInRun);

                if (missing.Count == 0)
                {
                    continue;
                }

                findings.Add(new ScanFinding(
                    FindingKind.MissingDisciplines,
                    MissingDisciplinesLabel,
                    group.Building + " has no " + string.Join(", ", missing.ToArray()),
                    "It holds " + string.Join(", ", new List<string>(group.Disciplines).ToArray())
                        + ". This run has " + string.Join(", ", new List<string>(disciplinesInRun).ToArray())
                        + ".",
                    new List<string> { group.Building },
                    null));
            }
        }

        private static List<string> Missing(BuildingGroup group, IList<string> disciplinesInRun)
        {
            HashSet<string> held = new HashSet<string>(group.Disciplines, StringComparer.Ordinal);
            List<string> missing = new List<string>();

            foreach (string discipline in disciplinesInRun)
            {
                if (!held.Contains(discipline))
                {
                    missing.Add(discipline);
                }
            }

            return missing;
        }

        private static List<string> DisciplinesAcross(IEnumerable<BuildingGroup> groups)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (BuildingGroup group in groups)
            {
                foreach (string discipline in group.Disciplines)
                {
                    seen.Add(discipline);
                }
            }

            List<string> all = new List<string>(seen);
            all.Sort(StringComparer.Ordinal);
            return all;
        }

        // ---------- reporting ----------

        /// <summary>
        /// The findings as text, one finding per pair of lines. When nothing is odd this
        /// is a single line saying so rather than an empty block.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            lines.Add("Disciplines in this run: "
                + (DisciplinesInRun.Count == 0
                    ? "none"
                    : string.Join(", ", new List<string>(DisciplinesInRun).ToArray())));

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
                    lines.Add(new string(' ', 18) + "file: " + file);
                }
            }

            return lines;
        }

        public const string NothingOdd =
            "Nothing odd. Every building code shares its shape with another, no two codes "
            + "are one character apart, and every group holds every discipline in the run.";

        private static List<string> FileNames(BuildingGroup group)
        {
            List<string> names = new List<string>();

            foreach (ParsedContainerName file in group.Files)
            {
                names.Add(file.Stem);
            }

            return names;
        }

        private static string FilesWord(int count)
        {
            return count == 1 ? " file" : " files";
        }
    }
}
