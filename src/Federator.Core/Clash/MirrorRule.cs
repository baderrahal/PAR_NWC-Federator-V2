using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The mirrored tests, F132, Bader's Q114 points 4 to 6 and 8, a rule of the code for
    /// any project.
    ///
    /// WHAT A MIRROR IS. A test whose two sides are the same two sets as another test's,
    /// swapped, such as Ducts against Columns and Columns against Ducts. The two find the
    /// same clashes, so the workbook rows, the viewpoints, the time and every count would
    /// hold each clash twice. The sets are compared by their locators, Ordinal and never
    /// trimmed, because two set names in the reference file end in a space. Two sets with
    /// different names are two sets even where their rules read alike, Q121's default A.
    ///
    /// WHICH ONE IS KEPT. The higher priority off the priority file, A before B before C
    /// before none, and where equal the one first in the XML. With no XML the tests saved
    /// in the document are read in the order they are saved, and the log says that order
    /// stands for the XML's. Every test in the other order is a mirror of the one kept, and
    /// it is not created and not run.
    ///
    /// WHAT IS NOT A MIRROR. A test with one set on both sides is its own swap. A test with
    /// a side nobody could read names no set. A second test with the same two sets in the
    /// kept test's own order is a duplicate, not a mirror by his words, so it is named and
    /// created and run as before. The picked XML holds no pair and no duplicate,
    /// turn5\measure-mirrors.md, so on it this rule leaves every test where it was.
    /// </summary>
    public sealed class MirrorRule
    {
        /// <summary>The word that begins every line this rule writes.</summary>
        public const string Prefix = "MIRROR";

        /// <summary>A character no locator carries, so two different pairs of sets never share a key.</summary>
        private const char Separator = '\u001F';

        private readonly List<PlannedClashTest> tests;
        private readonly List<MirrorPair> pairs;
        private readonly List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates;

        private MirrorRule(
            List<PlannedClashTest> tests,
            List<MirrorPair> pairs,
            List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates)
        {
            this.tests = tests;
            this.pairs = pairs;
            this.duplicates = duplicates;
            Pairs = new ReadOnlyCollection<MirrorPair>(pairs);
        }

        /// <summary>Every pair, one per mirror, in the order the first test of its two sets came.</summary>
        public ReadOnlyCollection<MirrorPair> Pairs { get; private set; }

        /// <summary>
        /// The rule over tests in the XML's order, or in the document's where no XML was
        /// picked. The priority file is the one the run picked, NothingPicked where none was,
        /// and then every pair keeps the one first in the XML.
        /// </summary>
        public static MirrorRule Of(IEnumerable<PlannedClashTest> tests, PriorityMap priorities)
        {
            if (priorities == null)
            {
                throw new ArgumentNullException("priorities");
            }

            List<PlannedClashTest> all = new List<PlannedClashTest>();
            List<string> keyOrder = new List<string>();
            Dictionary<string, List<PlannedClashTest>> bySets =
                new Dictionary<string, List<PlannedClashTest>>(StringComparer.Ordinal);

            if (tests != null)
            {
                foreach (PlannedClashTest test in tests)
                {
                    if (test == null)
                    {
                        continue;
                    }

                    all.Add(test);
                    string key = SetsKey(test);

                    if (key == null)
                    {
                        continue;
                    }

                    List<PlannedClashTest> group;

                    if (!bySets.TryGetValue(key, out group))
                    {
                        group = new List<PlannedClashTest>();
                        bySets.Add(key, group);
                        keyOrder.Add(key);
                    }

                    group.Add(test);
                }
            }

            List<MirrorPair> pairs = new List<MirrorPair>();
            List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates =
                new List<KeyValuePair<PlannedClashTest, PlannedClashTest>>();

            foreach (string key in keyOrder)
            {
                List<PlannedClashTest> group = bySets[key];
                PlannedClashTest kept = KeptOf(group, priorities);

                foreach (PlannedClashTest other in group)
                {
                    if (ReferenceEquals(other, kept))
                    {
                        continue;
                    }

                    if (string.Equals(other.Left.Locator, kept.Left.Locator, StringComparison.Ordinal))
                    {
                        duplicates.Add(new KeyValuePair<PlannedClashTest, PlannedClashTest>(kept, other));
                    }
                    else
                    {
                        pairs.Add(new MirrorPair(kept, priorities.Of(kept.Name), other, priorities.Of(other.Name)));
                    }
                }
            }

            return new MirrorRule(all, pairs, duplicates);
        }

        /// <summary>
        /// The MIRROR lines for the log. One line counting the pairs, always, because a
        /// missing line reads as a check that did not run. Then every pair whose two tests
        /// differ in priority, type or tolerance, each with both values, because Bader asked
        /// for both in the log. Then the pairs alike in everything, five named and the rest
        /// counted, the rule every repeated line here follows, since the skip block already
        /// names each mirror with the test it mirrors. Then the duplicates the same way.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (pairs.Count == 0)
            {
                lines.Add(Prefix + "   0 pairs, no test has another test's two sets swapped, so no test is left out");
            }
            else
            {
                lines.Add(Prefix + "   " + pairs.Count + (pairs.Count == 1 ? " pair" : " pairs")
                    + " of tests with the same two sets swapped. Of each pair the higher priority is kept, "
                    + "A before B before C before no priority, and where equal the one first in the XML, "
                    + "and the other is left out");

                if (pairs[0].Mirror.IsFromDocument)
                {
                    lines.Add(Prefix + "   no XML was picked, so the order the tests are saved in the document "
                        + "stands for the order of the XML");
                }

                List<string> alike = new List<string>();

                foreach (MirrorPair pair in pairs)
                {
                    if (pair.Differs())
                    {
                        lines.Add(pair.Line());
                    }
                    else
                    {
                        alike.Add(pair.Line());
                    }
                }

                AddFive(lines, alike, " more pairs alike in everything, counted and not listed");
            }

            List<string> repeated = new List<string>();

            foreach (KeyValuePair<PlannedClashTest, PlannedClashTest> duplicate in duplicates)
            {
                repeated.Add(Prefix + "   " + duplicate.Value.Name + " has the same two sets as "
                    + duplicate.Key.Name + " in the same order, so it is a duplicate and not a mirror, and it is "
                    + (duplicate.Value.IsFromDocument ? "run" : "created and run") + " as before");
            }

            AddFive(lines, repeated, " more duplicates, counted and not listed");
            return lines;
        }

        /// <summary>The pair whose mirror carries that name, Ordinal and never trimmed, or null.</summary>
        internal MirrorPair PairWhoseMirrorIsNamed(string name)
        {
            foreach (MirrorPair pair in pairs)
            {
                if (string.Equals(pair.Mirror.Name, name, StringComparison.Ordinal))
                {
                    return pair;
                }
            }

            return null;
        }

        /// <summary>Whether a test this rule was handed carries that name, Ordinal and never trimmed.</summary>
        internal bool Holds(string name)
        {
            foreach (PlannedClashTest test in tests)
            {
                if (string.Equals(test.Name, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The first test this rule was handed and did not call a mirror, so one that is
        /// created and run, whose two sets are that test's swapped. Null where there is none,
        /// or where that test can be no test's mirror.
        /// </summary>
        internal PlannedClashTest RunTestSwappedFrom(PlannedClashTest other)
        {
            if (other == null || SetsKey(other) == null)
            {
                return null;
            }

            foreach (PlannedClashTest test in tests)
            {
                if (string.Equals(test.Left.Locator, other.Right.Locator, StringComparison.Ordinal)
                    && string.Equals(test.Right.Locator, other.Left.Locator, StringComparison.Ordinal)
                    && !IsAMirror(test))
                {
                    return test;
                }
            }

            return null;
        }

        private bool IsAMirror(PlannedClashTest test)
        {
            foreach (MirrorPair pair in pairs)
            {
                if (ReferenceEquals(pair.Mirror, test))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddFive(List<string> lines, List<string> from, string rest)
        {
            int shown = Math.Min(from.Count, RunLog.KeptOfARepeat);

            lines.AddRange(from.GetRange(0, shown));

            if (from.Count > shown)
            {
                lines.Add(Prefix + "   and " + (from.Count - shown) + rest);
            }
        }

        /// <summary>
        /// The two sets a test names, the same key whichever way round, or null where the
        /// test can be no test's mirror: a side nobody could read, or one set on both sides.
        /// </summary>
        private static string SetsKey(PlannedClashTest test)
        {
            string left = test.Left == null ? null : test.Left.Locator;
            string right = test.Right == null ? null : test.Right.Locator;

            if (!WasRead(left) || !WasRead(right) || string.Equals(left, right, StringComparison.Ordinal))
            {
                return null;
            }

            return string.CompareOrdinal(left, right) < 0
                ? left + Separator + right
                : right + Separator + left;
        }

        private static bool WasRead(string locator)
        {
            return !string.IsNullOrEmpty(locator) && TestDrift.WasRead(locator);
        }

        /// <summary>The higher priority, A first and none last, and where equal the first in the XML.</summary>
        private static PlannedClashTest KeptOf(List<PlannedClashTest> group, PriorityMap priorities)
        {
            PlannedClashTest kept = group[0];

            foreach (PlannedClashTest test in group)
            {
                int byPriority = Priorities.Order(priorities.Of(test.Name))
                    .CompareTo(Priorities.Order(priorities.Of(kept.Name)));

                if (byPriority < 0 || (byPriority == 0 && test.FileIndex < kept.FileIndex))
                {
                    kept = test;
                }
            }

            return kept;
        }
    }
}
