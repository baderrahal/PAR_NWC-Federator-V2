using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;
using Federator.Core.Exchange;
using Federator.Core.Health;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The mirrored tests, F132, Bader's Q114 points 4 to 6 and 8, his answer B to Q121 and
    /// his answer D to Q133, a rule of the code for any project.
    ///
    /// WHAT A MIRROR IS. A test that asks another test's question: its two sides are the
    /// same two sets swapped, such as Ducts against Columns and Columns against Ducts, or
    /// its sets carry the same rule lists as the other test's, in either order, Q121 B. Two
    /// sets carry the same rule list where SetWarnings.FindIdentical finds them alike, the
    /// HEALTH block's own rule, so a rule list is compared one way in one place. The sets are
    /// the picked XML's. With no XML they were not read, and only the same two sets swapped
    /// pair, which the log says. A side is matched by its locator, Ordinal and never trimmed,
    /// because two set names in the reference file end in a space.
    ///
    /// WHAT IS DONE WITH ONE, Q133 D. Both tests of a pair are created and run. The mirror is
    /// created under its XML name with the ending of MirrorSettings, and stays in Clash
    /// Detective. Their clashes are merged by the pair of items into the one kept,
    /// Report.MirrorMerge, so the report, the views and every count hold each clash once,
    /// and a clash only the mirror finds is added to the kept test and named as found by the
    /// mirror only. Probe P1 measured that a swap can find more, docs\history\scan.md 5z-k on
    /// the branch fix-F114-probes.
    ///
    /// WHICH ONE IS KEPT, Q114 point 6. The higher priority off the priority file, A before
    /// B before C before none, and where equal the one first in the XML. With no XML the
    /// tests saved in the document are read in the order they are saved, and the log says
    /// that order stands for the XML's.
    ///
    /// WHAT IS NOT A MIRROR. A test with one set on both sides, or two sets of one rule list,
    /// is its own swap. A second test with the same two sets in the kept test's own order is
    /// a duplicate, not a mirror by his words, so it is named and created and run as before.
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
        private readonly bool rulesRead;
        private readonly List<MirrorPair> pairs;
        private readonly List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates;

        private MirrorRule(
            List<PlannedClashTest> tests,
            int notRead,
            bool rulesRead,
            List<MirrorPair> pairs,
            List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates)
        {
            this.tests = tests;
            this.notRead = notRead;
            this.rulesRead = rulesRead;
            this.pairs = pairs;
            this.duplicates = duplicates;
            Pairs = new ReadOnlyCollection<MirrorPair>(pairs);
        }

        /// <summary>Every pair, one per mirror, in the order the first test of its question came.</summary>
        public ReadOnlyCollection<MirrorPair> Pairs { get; private set; }

        /// <summary>
        /// The rule over tests in the XML's order, or in the document's where no XML was
        /// picked. The priority file is the one the run picked, NothingPicked where none was,
        /// and then every pair keeps the one first in the XML. The sets are the picked XML's,
        /// ExchangeDocument.Sets, whose rule lists pair two tests of two sets alike, or null
        /// where no XML was picked.
        /// </summary>
        public static MirrorRule Of(
            IEnumerable<PlannedClashTest> tests,
            PriorityMap priorities,
            IEnumerable<SelectionSetDefinition> sets,
            MirrorSettings settings)
        {
            if (priorities == null)
            {
                throw new ArgumentNullException("priorities");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            SetIdentity identity = new SetIdentity(sets);
            List<PlannedClashTest> all = new List<PlannedClashTest>();
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            int notRead = 0;
            List<string> keyOrder = new List<string>();
            Dictionary<string, List<PlannedClashTest>> byQuestion =
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
                    names.Add(test.Name ?? string.Empty);

                    if (!BothSidesRead(test))
                    {
                        notRead++;
                        continue;
                    }

                    string key = identity.QuestionKey(test);

                    if (key == null)
                    {
                        continue;
                    }

                    List<PlannedClashTest> group;

                    if (!byQuestion.TryGetValue(key, out group))
                    {
                        group = new List<PlannedClashTest>();
                        byQuestion.Add(key, group);
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
                List<PlannedClashTest> group = byQuestion[key];
                PlannedClashTest kept = KeptOf(group, priorities);

                foreach (PlannedClashTest other in group)
                {
                    if (ReferenceEquals(other, kept))
                    {
                        continue;
                    }

                    if (Same(other.Left, kept.Left) && Same(other.Right, kept.Right))
                    {
                        duplicates.Add(new KeyValuePair<PlannedClashTest, PlannedClashTest>(kept, other));
                        continue;
                    }

                    bool swapped = Same(other.Left, kept.Right) && Same(other.Right, kept.Left);
                    string wanted = other.IsFromDocument ? other.Name : settings.NameOf(other.Name);
                    bool endingTaken = !string.Equals(wanted, other.Name, StringComparison.Ordinal)
                        && names.Contains(wanted);

                    pairs.Add(new MirrorPair(
                        kept,
                        priorities.Of(kept.Name),
                        other,
                        priorities.Of(other.Name),
                        swapped ? MirrorKind.Swapped : MirrorKind.SameRules,
                        swapped ? null : identity.Alike(other, kept),
                        endingTaken ? other.Name : wanted,
                        endingTaken));
                }
            }

            return new MirrorRule(all, notRead, identity.RulesRead, pairs, duplicates);
        }

        /// <summary>
        /// The MIRROR lines for the log. One line counting the pairs among the tests whose
        /// two sets were read, always, because a missing line reads as a check that did not
        /// run and a count of pairs alone reads as a check of every test. Then once, where
        /// no rule list was read, that only the same two sets swapped pair. Then once, where
        /// any test has a side not read, how many, said UNKNOWN. Then every pair whose two
        /// tests differ in priority, type or tolerance, each with both values, because Bader
        /// asked for both in the log. Then the pairs alike in those three, the only three
        /// compared and so never said to be alike in everything, five named and the rest
        /// counted, the rule every repeated line here follows. Then the duplicates the same way.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            int read = tests.Count - notRead;
            string among = " among the " + read + (read == 1 ? " test" : " tests") + " whose two sets were read";

            if (pairs.Count == 0)
            {
                lines.Add(Prefix + "   0 pairs" + among + ", so every test keeps its own clashes");
            }
            else
            {
                int swapped = 0;

                foreach (MirrorPair pair in pairs)
                {
                    if (pair.Kind == MirrorKind.Swapped)
                    {
                        swapped++;
                    }
                }

                int sameRules = pairs.Count - swapped;

                lines.Add(Prefix + "   " + pairs.Count + (pairs.Count == 1 ? " pair" : " pairs")
                    + " of tests that ask the same question" + among + ", " + swapped
                    + " with the same two sets swapped and " + sameRules + " whose sets carry the same rule lists. "
                    + "Both tests of each pair are created and run, and their clashes are merged by the pair of "
                    + "items into the one kept, the higher priority, A before B before C before no priority, "
                    + "and where equal the one first in the XML");
            }

            if (!rulesRead)
            {
                lines.Add(Prefix + "   no rule list of a set was read, so only tests with the same two sets "
                    + "swapped are paired");
            }

            if (notRead > 0)
            {
                lines.Add(Prefix + "   " + notRead + " of the " + tests.Count
                    + (notRead == 1
                        ? " tests has a side whose set was not read, so whether it is a mirror or a duplicate "
                            + "is UNKNOWN and its clashes are not merged"
                        : " tests have a side whose set was not read, so whether each is a mirror or a duplicate "
                            + "is UNKNOWN and none of their clashes is merged"));
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

                AddFive(lines, alike, "pair", "pairs", " alike in priority, test type and tolerance, "
                    + "counted and not listed");
            }

            List<string> repeated = new List<string>();

            foreach (KeyValuePair<PlannedClashTest, PlannedClashTest> duplicate in duplicates)
            {
                repeated.Add(Prefix + "   " + duplicate.Value.Name + " has the same two sets as "
                    + duplicate.Key.Name + " in the same order, so it is a duplicate and not a mirror, and it is "
                    + (duplicate.Value.IsFromDocument ? "run" : "created and run") + " as before");
            }

            AddFive(lines, repeated, "duplicate", "duplicates", ", counted and not listed");
            return lines;
        }

        /// <summary>
        /// The names the coverage sheet gives the tests of the pairs, F127's data, Q114 point
        /// 6 and Q121 B: each test of a pair once, as the XML or the document names it, with
        /// its words, in the order the XML holds them. A mirror is named a mirror of the test
        /// kept, how the two ask one question and the name it runs under. A test kept is
        /// named once however many mirrors it has, each by the name it runs under.
        /// </summary>
        public IList<KeyValuePair<string, string>> CoverageNames()
        {
            List<PlannedClashTest> order = new List<PlannedClashTest>();
            Dictionary<PlannedClashTest, string> words = new Dictionary<PlannedClashTest, string>();
            Dictionary<PlannedClashTest, List<string>> mirrorsOf = new Dictionary<PlannedClashTest, List<string>>();

            foreach (MirrorPair pair in pairs)
            {
                List<string> mirrors;

                if (!mirrorsOf.TryGetValue(pair.Kept, out mirrors))
                {
                    mirrors = new List<string>();
                    mirrorsOf.Add(pair.Kept, mirrors);
                    order.Add(pair.Kept);
                }

                mirrors.Add(pair.MirrorName);
                words[pair.Mirror] = pair.CoverageOfTheMirror();
                order.Add(pair.Mirror);
            }

            foreach (KeyValuePair<PlannedClashTest, List<string>> kept in mirrorsOf)
            {
                words[kept.Key] = kept.Value.Count == 1
                    ? "kept of a mirrored pair, the clashes only its mirror " + kept.Value[0] + " finds are added to it"
                    : "kept of " + kept.Value.Count + " mirrored pairs, the clashes only its mirrors "
                        + Joined(kept.Value) + " find are added to it";
            }

            List<PlannedClashTest> inFileOrder = new List<PlannedClashTest>(order);
            inFileOrder.Sort((one, other) => one.FileIndex.CompareTo(other.FileIndex));

            List<KeyValuePair<string, string>> names = new List<KeyValuePair<string, string>>();

            foreach (PlannedClashTest test in inFileOrder)
            {
                names.Add(new KeyValuePair<string, string>(test.Name, words[test]));
            }

            return names;
        }

        /// <summary>Names joined with commas and a last and.</summary>
        private static string Joined(List<string> names)
        {
            return names.Count == 1
                ? names[0]
                : string.Join(", ", names.GetRange(0, names.Count - 1).ToArray()) + " and " + names[names.Count - 1];
        }

        private static void AddFive(List<string> lines, List<string> from, string one, string many, string rest)
        {
            int shown = Math.Min(from.Count, RunLog.KeptOfARepeat);
            int more = from.Count - shown;

            lines.AddRange(from.GetRange(0, shown));

            if (more > 0)
            {
                lines.Add(Prefix + "   and " + more + " more " + (more == 1 ? one : many) + rest);
            }
        }

        private static bool Same(PlannedClashSide one, PlannedClashSide other)
        {
            return string.Equals(one.Locator, other.Locator, StringComparison.Ordinal);
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

        /// <summary>
        /// Which sets ask one question, Q121 B. Two sets whose rule lists SetWarnings
        /// .FindIdentical finds alike stand for the first of them, and every other set stands
        /// for itself. A set is found by its path, which is built to be compared with a
        /// test's locator, Ordinal and never trimmed.
        /// </summary>
        private sealed class SetIdentity
        {
            private readonly Dictionary<string, string> firstAlike = new Dictionary<string, string>(StringComparer.Ordinal);
            private readonly Dictionary<string, string> names = new Dictionary<string, string>(StringComparer.Ordinal);

            internal SetIdentity(IEnumerable<SelectionSetDefinition> sets)
            {
                if (sets == null)
                {
                    return;
                }

                List<SelectionSetDefinition> all = new List<SelectionSetDefinition>();

                foreach (SelectionSetDefinition set in sets)
                {
                    if (set == null)
                    {
                        continue;
                    }

                    all.Add(set);

                    if (set.Conditions.Count > 0)
                    {
                        RulesRead = true;
                    }

                    if (set.Path != null && !names.ContainsKey(set.Path))
                    {
                        names.Add(set.Path, set.Name);
                    }
                }

                foreach (IdenticalSets alike in SetWarnings.FindIdentical(all))
                {
                    foreach (SelectionSetDefinition set in alike.Sets)
                    {
                        if (set.Path != null && !firstAlike.ContainsKey(set.Path))
                        {
                            firstAlike.Add(set.Path, alike.Sets[0].Path);
                        }
                    }
                }
            }

            /// <summary>Whether any set handed carries a rule list, so two sets could be compared at all.</summary>
            internal bool RulesRead { get; private set; }

            /// <summary>
            /// The question a test asks, the same key whichever way round and whichever set of
            /// one rule list it names, or null where it can be no test's mirror. A test of two
            /// sets of one rule list asks what one set against itself asks, so only its swap
            /// by name is its mirror, on the key of its two sets.
            /// </summary>
            internal string QuestionKey(PlannedClashTest test)
            {
                string bySets = SetsKey(test);

                if (bySets == null)
                {
                    return null;
                }

                string left = Standing(test.Left.Locator);
                string right = Standing(test.Right.Locator);

                return string.Equals(left, right, StringComparison.Ordinal)
                    ? "sets " + bySets
                    : "rules " + ByDesignPairs.KeyFor(left, right);
            }

            /// <summary>
            /// The sets of the mirror that differ by name from the kept test's set of the same
            /// rule list, each as the mirror's set then the kept test's, by their set names.
            /// </summary>
            internal string Alike(PlannedClashTest mirror, PlannedClashTest kept)
            {
                bool straight = string.Equals(
                    Standing(mirror.Left.Locator), Standing(kept.Left.Locator), StringComparison.Ordinal);
                PlannedClashSide keptForLeft = straight ? kept.Left : kept.Right;
                PlannedClashSide keptForRight = straight ? kept.Right : kept.Left;
                List<string> said = new List<string>();

                Say(said, mirror.Left, keptForLeft);
                Say(said, mirror.Right, keptForRight);
                return string.Join(" and ", said.ToArray());
            }

            private void Say(List<string> said, PlannedClashSide mirror, PlannedClashSide kept)
            {
                if (!string.Equals(mirror.Locator, kept.Locator, StringComparison.Ordinal))
                {
                    said.Add(NameOf(mirror.Locator) + " as " + NameOf(kept.Locator));
                }
            }

            private string Standing(string locator)
            {
                string first;
                return firstAlike.TryGetValue(locator, out first) ? first : locator;
            }

            private string NameOf(string locator)
            {
                string name;
                return names.TryGetValue(locator, out name) ? name : locator;
            }
        }
    }
}
