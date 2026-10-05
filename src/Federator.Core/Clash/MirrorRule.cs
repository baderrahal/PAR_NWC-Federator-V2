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
    /// WHAT IS NOT A MIRROR. A test with one set on both sides is its own swap. A second
    /// test with the same two sets in the kept test's own order is a duplicate, not a mirror
    /// by his words, so it is named and created and run as before. The picked XML holds no
    /// pair and no duplicate, so on it this rule leaves every test where it was, which
    /// MirrorRuleTests.TheClientsMatrixHoldsNoPair and TheCorrectedMatrixHoldsNoPair prove.
    ///
    /// A SIDE NOT READ IS UNKNOWN. A side whose locator is empty, UNKNOWN, or one of the
    /// placeholders the add-in hands for a saved test's sides, SavedClashTest.LeftAsSaved
    /// and RightAsSaved, names no set. Read as sets, the placeholders would make every
    /// saved test a duplicate of the first. So a test with such a side is never paired,
    /// never a mirror and never a duplicate, and the lines say once how many there are.
    /// </summary>
    public sealed class MirrorRule
    {
        /// <summary>The word that begins every line this rule writes.</summary>
        public const string Prefix = "MIRROR";

        private readonly List<PlannedClashTest> tests;
        private readonly int notRead;
        private readonly List<MirrorPair> pairs;
        private readonly List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates;

        private MirrorRule(
            List<PlannedClashTest> tests,
            int notRead,
            List<MirrorPair> pairs,
            List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates)
        {
            this.tests = tests;
            this.notRead = notRead;
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
            int notRead = 0;
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

                    if (!BothSidesRead(test))
                    {
                        notRead++;
                        continue;
                    }

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

            return new MirrorRule(all, notRead, pairs, duplicates);
        }

        /// <summary>
        /// The MIRROR lines for the log. One line counting the pairs among the tests whose
        /// two sets were read, always, because a missing line reads as a check that did not
        /// run and a count of pairs alone reads as a check of every test. Then once, where
        /// any test has a side not read, how many, said UNKNOWN. Then every pair whose two
        /// tests differ in priority, type or tolerance, each with both values, because Bader
        /// asked for both in the log. Then the pairs alike in those three, the only three
        /// compared and so never said to be alike in everything, five named and the rest
        /// counted, the rule every repeated line here follows, since the skip block already
        /// names each mirror with the test it mirrors. Then the duplicates the same way.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            int read = tests.Count - notRead;
            string among = " among the " + read + (read == 1 ? " test" : " tests") + " whose two sets were read";

            if (pairs.Count == 0)
            {
                lines.Add(Prefix + "   0 pairs" + among + ", so no test is left out");
            }
            else
            {
                lines.Add(Prefix + "   " + pairs.Count + (pairs.Count == 1 ? " pair" : " pairs")
                    + " of tests with the same two sets swapped" + among
                    + ". Of each pair the higher priority is kept, "
                    + "A before B before C before no priority, and where equal the one first in the XML, "
                    + "and the other is left out");
            }

            if (notRead > 0)
            {
                lines.Add(Prefix + "   " + notRead + " of the " + tests.Count
                    + (notRead == 1
                        ? " tests has a side whose set was not read, so whether it is a mirror or a duplicate "
                            + "is UNKNOWN and it is not left out"
                        : " tests have a side whose set was not read, so whether each is a mirror or a duplicate "
                            + "is UNKNOWN and none of them is left out"));
            }

            if (pairs.Count > 0)
            {
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

                AddFive(lines, alike, " more pairs alike in priority, test type and tolerance, "
                    + "counted and not listed");
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
        /// The first test this rule was handed that was read off the document, or null. Only
        /// ClashTestPlan builds a test, and FromDocument alone gives one an address, so this
        /// is a fact the add-in cannot forge. A rule holding such a test is no XML's rule.
        /// </summary>
        internal PlannedClashTest FirstReadOffTheDocument()
        {
            foreach (PlannedClashTest test in tests)
            {
                if (test.IsFromDocument)
                {
                    return test;
                }
            }

            return null;
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
        /// The key is the by design pairs' own, ByDesignPairs.KeyFor, one rule in one place.
        /// </summary>
        internal static string SetsKey(PlannedClashTest test)
        {
            if (!BothSidesRead(test) || string.Equals(test.Left.Locator, test.Right.Locator, StringComparison.Ordinal))
            {
                return null;
            }

            return ByDesignPairs.KeyFor(test.Left.Locator, test.Right.Locator);
        }

        /// <summary>
        /// Whether both sides name a set that was read: neither empty, nor UNKNOWN, nor one
        /// of the placeholders the add-in hands for a saved test's sides.
        /// </summary>
        internal static bool BothSidesRead(PlannedClashTest test)
        {
            return test.Left != null && test.Right != null
                && WasRead(test.Left.Locator) && WasRead(test.Right.Locator);
        }

        private static bool WasRead(string locator)
        {
            return !string.IsNullOrEmpty(locator)
                && TestDrift.WasRead(locator)
                && !SavedClashTest.IsPlaceholder(locator);
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
