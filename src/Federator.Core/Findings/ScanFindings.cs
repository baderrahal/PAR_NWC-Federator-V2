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
        /// Characters that are easy to mistake for one another when a code is read off a
        /// drawing or retyped. Each line is one group, and any two different characters
        /// from the same group are confusable in either direction.
        ///
        /// Anything wider than this is noise. A plain one character difference reported
        /// about 45 pairs on a 22 group run and buried everything else, because codes
        /// that share a prefix differ by one character constantly and 1B06PE against
        /// 1B06PG is two real buildings.
        /// </summary>
        private static readonly string[] ConfusableGroups =
        {
            "1Il",
            "0O",
            "5S",
            "8B",
            "2Z",
            "6G"
        };

        /// <summary>
        /// True when the two characters are different but easy to mistake for one another.
        /// Compared as written, so only the exact characters listed above count.
        /// </summary>
        public static bool AreConfusableCharacters(char left, char right)
        {
            if (left == right)
            {
                return false;
            }

            foreach (string group in ConfusableGroups)
            {
                if (group.IndexOf(left) >= 0 && group.IndexOf(right) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when two codes are the same length and differ in exactly one position, and
        /// the two characters in that position are confusable when read.
        ///
        /// An inserted or a missing character is deliberately not a near match. That was
        /// most of the noise, and a code one character longer than another is usually a
        /// different building rather than a typing slip.
        /// </summary>
        public static bool IsConfusablePair(string left, string right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            int at = -1;

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] == right[i])
                {
                    continue;
                }

                if (at >= 0)
                {
                    return false;
                }

                at = i;
            }

            return at >= 0 && AreConfusableCharacters(left[at], right[at]);
        }

        /// <summary>
        /// Where the two codes differ and what the pair is, for the line that reports it.
        /// Returns null when they are not a confusable pair.
        /// </summary>
        public static string DescribeConfusion(string left, string right)
        {
            if (!IsConfusablePair(left, right))
            {
                return null;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return "character " + (i + 1) + ", a " + left[i] + " against an " + right[i];
                }
            }

            return null;
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

                // An example of what the others look like, rather than the shape string.
                // Nobody reads 9A99AA. Everybody reads 1B06PK.
                string example = MostSharedExample(byShape, shapeOrder, shape);
                int others = 0;

                foreach (string other in shapeOrder)
                {
                    if (!string.Equals(other, shape, StringComparison.Ordinal))
                    {
                        others += byShape[other].Count;
                    }
                }

                findings.Add(new ScanFinding(
                    FindingKind.OddShape,
                    OddShapeLabel,
                    "The building code " + odd.Building
                        + " is written differently from every other code in this run.",
                    "The other " + others + (others == 1 ? " code looks" : " codes look")
                        + " like " + example + ", and " + odd.Building + " does not follow that. "
                        + "That is often a typing error in the file name, and it is sometimes a "
                        + "real building named another way. Nothing is changed and the group still "
                        + "runs.",
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

                    string confusion = DescribeConfusion(left.Building, right.Building);

                    if (confusion == null)
                    {
                        continue;
                    }

                    findings.Add(new ScanFinding(
                        FindingKind.NearMatch,
                        NearMatchLabel,
                        left.Building + " and " + right.Building
                            + " look almost the same and could be one building typed two ways.",
                        "They differ only at " + confusion
                            + ", which are easy to mistake for one another when a code is read off "
                            + "a drawing or retyped. " + left.Building + " holds " + left.FileCount
                            + FilesWord(left.FileCount) + " and " + right.Building + " holds "
                            + right.FileCount + FilesWord(right.FileCount)
                            + ". They are being kept as two separate federations. If they are meant "
                            + "to be one building, one of the NWC file names needs correcting.",
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
                        group.Building + " holds only " + group.Disciplines[0]
                            + " files, so there is nothing for them to clash against.",
                        "This run also has "
                            + string.Join(", ", Without(disciplinesInRun, group.Disciplines[0]).ToArray())
                            + " files in other buildings. The federation is still built and every "
                            + "clash test is still created, and none of them is run, because one "
                            + "discipline cannot clash with itself. "
                            + "Either the other disciplines have not been exported yet, or this "
                            + "building really is " + group.Disciplines[0] + " only.",
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
                    group.Building + " has no " + string.Join(" or ", missing.ToArray())
                        + " files, which other buildings in this run do have.",
                    "It holds " + string.Join(", ", new List<string>(group.Disciplines).ToArray())
                        + ", and this run has "
                        + string.Join(", ", new List<string>(disciplinesInRun).ToArray())
                        + " between all its buildings. That may be deliberate, or those models may "
                        + "not have been exported yet. Nothing is blocked either way.",
                    new List<string> { group.Building },
                    null));
            }
        }

        /// <summary>
        /// A real code that has the most widely shared shape, so the finding can say what
        /// the others look like instead of printing a pattern nobody reads.
        /// </summary>
        private static string MostSharedExample(
            Dictionary<string, List<BuildingGroup>> byShape, IList<string> shapeOrder, string exclude)
        {
            string best = null;
            int most = 0;

            foreach (string shape in shapeOrder)
            {
                if (string.Equals(shape, exclude, StringComparison.Ordinal))
                {
                    continue;
                }

                if (byShape[shape].Count > most)
                {
                    most = byShape[shape].Count;
                    best = byShape[shape][0].Building;
                }
            }

            return best == null ? "the others" : best;
        }

        private static List<string> Without(IEnumerable<string> all, string one)
        {
            List<string> rest = new List<string>();

            foreach (string value in all)
            {
                if (!string.Equals(value, one, StringComparison.Ordinal))
                {
                    rest.Add(value);
                }
            }

            return rest;
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
            + "differ by a single character that is easy to misread, and every group holds "
            + "every discipline in the run.";

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
