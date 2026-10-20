using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Exchange;
using Federator.Core.Teams;

namespace Federator.Core.Clash
{
    /// <summary>
    /// F248, Bader's answer Q232 of 20 Oct 2026: a test where a set meets itself, for every set of the teams the pack's rules
    /// name, Mechanical and Electrical for 1104-PAR, each at the priority the rules give, A, on both pages. The tests are made
    /// from the pack's own sets and team map, so the next project's pack gets them with no hand edit, and are added to the
    /// matrix the run builds from, after its own tests, so the plan, the rule of two set codes, the mirror rule, the coverage,
    /// the workbooks and the views read them as any other test of the matrix.
    ///
    /// A SET'S TEAM is read by the team map's one reader, TeamMap.TeamOfSet over the code CodeOf.Set reads off its name and the
    /// folders above it in the set tree, the reader the TEAMS lines and the views use, so BLD-Security Devices, whose name
    /// carries no code, is Electrical by its folder.
    ///
    /// A TEST IS NAMED as the pack names its tests, its left set, -vs-, its right set, so BLD-ME-Ducts&amp;Duct Fittings against
    /// itself is BLD-ME-Ducts&amp;Duct Fittings-vs-BLD-ME-Ducts&amp;Duct Fittings. A name the matrix already holds is not added
    /// again and is said.
    ///
    /// ITS SETTINGS are those of a test of the matrix that names the set: its type, status, merge, linkage and rules, and on
    /// both sides the self intersect and primitive types of the side that names the set, the first such test in the matrix's
    /// order, a left side before a right side. A set no test of the matrix names gets no self test, since its settings would
    /// be a guess, and is said. The tolerance comes from the drop down as for every test, F167.
    /// </summary>
    public static class SelfTests
    {
        /// <summary>What the log's lines start with.</summary>
        public const string Prefix = "SELF    ";

        /// <summary>The name of the test setting that set against itself, in the pack's shape.</summary>
        public static string NameFor(string setName)
        {
            return setName + "-vs-" + setName;
        }

        /// <summary>
        /// The matrix with a test added for every set of the teams the rules name, and the lines saying what was added and what
        /// was not. The matrix as it was, and a line saying why, where the rules name no team, were not read, or no map was read.
        /// </summary>
        public static SelfTestOutcome For(ExchangeDocument matrix, PackRules rules, char separator)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            List<string> lines = new List<string>();

            if (rules == null || !rules.IsRead || rules.SelfTestTeams.Count == 0)
            {
                lines.Add(Prefix + "no test of a set against itself is made, "
                    + (rules == null || !rules.IsRead ? "the pack's rules not read" : "the pack's rules naming no team for them")
                    + ", Bader's answer to Q232");
                return new SelfTestOutcome(matrix, lines);
            }

            TeamMap map = matrix.Teams;

            if (map == null || !map.IsRead)
            {
                lines.Add(Prefix + "no test of a set against itself is made, no team map was read to tell which sets are of "
                    + PackRules.Joined(rules.SelfTestTeams) + ", Bader's answer to Q232");
                return new SelfTestOutcome(matrix, lines);
            }

            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

            foreach (ClashTestDefinition test in matrix.Tests)
            {
                names.Add(test.Name);
            }

            List<ClashTestDefinition> added = new List<ClashTestDefinition>();
            Dictionary<string, int> byTeam = new Dictionary<string, int>(StringComparer.Ordinal);
            List<string> notMade = new List<string>();

            foreach (SelectionSetDefinition set in matrix.Sets)
            {
                if (string.IsNullOrEmpty(set.Name) || string.IsNullOrEmpty(set.Path))
                {
                    continue;
                }

                string team = map.TeamOfSet(CodeOf.Set(set.Name, map.Codes, separator), set.Folders);

                if (!rules.SelfTestTeams.Contains(team))
                {
                    continue;
                }

                string name = NameFor(set.Name);

                if (names.Contains(name))
                {
                    notMade.Add(name + ", a test of that name is in the matrix already");
                    continue;
                }

                ClashTestDefinition template;
                ClashSideDefinition side = SideOf(matrix, set.Path, out template);

                if (side == null)
                {
                    notMade.Add(set.Name + ", no test of the matrix names the set, so the settings of its sides are UNKNOWN");
                    continue;
                }

                added.Add(new ClashTestDefinition(
                    name,
                    template.TestType,
                    template.Status,
                    template.MergeComposites,
                    template.LinkageMode,
                    new List<string>(template.Rules),
                    new ClashSideDefinition(side.SelfIntersect, side.PrimitiveTypes, set.Path),
                    new ClashSideDefinition(side.SelfIntersect, side.PrimitiveTypes, set.Path),
                    new List<string>(template.MissingSettings)));
                names.Add(name);

                int seen;
                byTeam[team] = byTeam.TryGetValue(team, out seen) ? seen + 1 : 1;
            }

            List<string> counts = new List<string>();

            foreach (string team in rules.SelfTestTeams)
            {
                int count;
                counts.Add(team + " " + (byTeam.TryGetValue(team, out count) ? count : 0));
            }

            lines.Add(Prefix + added.Count + (added.Count == 1 ? " test" : " tests") + " of a set against itself added to the "
                + matrix.Tests.Count + " of the matrix, one for every set of " + PackRules.Joined(rules.SelfTestTeams) + ", "
                + string.Join(", ", counts.ToArray()) + ", each at priority " + Priorities.Words(rules.SelfTestPriority)
                + ", " + notMade.Count + " not made, Bader's answer to Q232");

            foreach (string why in notMade)
            {
                lines.Add(Prefix + "not made: " + why);
            }

            return new SelfTestOutcome(matrix.WithSelfTests(added, rules.SelfTestPriority), lines);
        }

        /// <summary>The side of the first test of the matrix that names the set, a left side before a right side, and its test.</summary>
        private static ClashSideDefinition SideOf(ExchangeDocument matrix, string path, out ClashTestDefinition template)
        {
            foreach (ClashTestDefinition test in matrix.Tests)
            {
                if (test.Left != null && string.Equals(test.Left.Locator, path, StringComparison.Ordinal))
                {
                    template = test;
                    return test.Left;
                }
            }

            foreach (ClashTestDefinition test in matrix.Tests)
            {
                if (test.Right != null && string.Equals(test.Right.Locator, path, StringComparison.Ordinal))
                {
                    template = test;
                    return test.Right;
                }
            }

            template = null;
            return null;
        }
    }

    /// <summary>The matrix a run builds from with its self tests added, and the lines saying so, F248.</summary>
    public sealed class SelfTestOutcome
    {
        internal SelfTestOutcome(ExchangeDocument matrix, IList<string> lines)
        {
            Matrix = matrix;
            Lines = new ReadOnlyCollection<string>(new List<string>(lines));
        }

        /// <summary>The matrix with the self tests after its own, or the matrix as it was where none was made.</summary>
        public ExchangeDocument Matrix { get; private set; }

        /// <summary>The SELF lines for the log.</summary>
        public ReadOnlyCollection<string> Lines { get; private set; }
    }
}
