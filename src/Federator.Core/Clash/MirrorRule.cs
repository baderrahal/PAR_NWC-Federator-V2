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
    /// its sets ask the same whole questions as the other test's, in either order, Q121 B.
    /// Two sets ask one whole question where their findspec mode, disjoint and start, and
    /// every condition's rule and flags, are the same, SelectionSetDefinition.WholeQuestion,
    /// grouped by SetWarnings.FindIdentical, the HEALTH block's own grouping. The flags are
    /// whole, the negation, the Or group, the ignore case and the ignore name bits, because
    /// the merge acts on this comparison. The sets are the picked XML's. A side is matched by
    /// its locator, Ordinal and never trimmed, because two set names in the reference file
    /// end in a space.
    ///
    /// WITH NO XML, BY THE SIDES AND NEVER BY A NAME, Bader's answers D to Q133 and A to
    /// Q136. A mirror carries its own name with the ending, so a saved name says that a test
    /// is a mirror and never which test it mirrors, and no name is read back to a test. A
    /// saved test whose name ends with the ending pairs with the one saved test without it
    /// whose question its sides ask as a mirror, AsAMirror, the one rule the XML's tests pair
    /// by, and the one without the ending is kept, never a priority or an order read again.
    /// It is not paired, and its line says so, where its sides were not read, where two or
    /// more saved tests without the ending ask its question, where none does and whether
    /// one does is UNKNOWN for a side or a set's rule list not read, and where only tests
    /// with the ending ask it. Each of those is UNKNOWN and its line says that a clash both
    /// find may then be counted twice. Where no other saved test asks its question it keeps
    /// its own clashes and its line says so. A saved test with the ending is never a test kept. A
    /// saved side the add-in hands as a placeholder, SavedClashTest.LeftAsSaved and
    /// RightAsSaved, is a side not read. The sets of a saved test's sides are the ones
    /// handed, the document's as the add-in reads them, or none, and with none only the same
    /// two sets swapped ask one question.
    ///
    /// A TEST SAVED BEFORE THE MIRROR RULE, Bader's answer A to Q136. RenamesIn plans each
    /// saved test under the XML's name of a mirror, Y, whose sides ask the kept test's
    /// question by the same rule, to be renamed to its own name with the ending, Y (mirror),
    /// never the kept test's, its statuses kept, and run as the mirror, MirrorRenames.
    ///
    /// WHAT IS DONE WITH ONE, Q133 D. Both tests of a pair are created and run. The mirror is
    /// created under its own name with the ending of MirrorSettings, MirrorSettings.NameFor,
    /// and stays in Clash Detective. Their clashes are merged by the pair of items into the
    /// one kept, Report.MirrorMerge, so the report, the views and every count hold each clash once,
    /// and a clash only the mirror finds is added to the kept test and named as found by the
    /// mirror only. Probe P1 measured that a swap can find more, docs\history\scan.md 5z-k on
    /// the branch fix-F114-probes.
    ///
    /// WHICH ONE IS KEPT, Q114 point 6. The higher priority off the priority file, A before
    /// B before C before none, and where equal the one first in the XML. With no XML the
    /// one whose name does not end with the ending.
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

        /// <summary>The words every line of a saved test with the ending not paired for an UNKNOWN ends with.</summary>
        private const string MayBeCountedTwice = ". It keeps its own clashes, and a clash both find may be counted twice";

        private readonly int fromTheXml;
        private readonly int saved;
        private readonly int notRead;
        private readonly bool rulesRead;
        private readonly string ending;
        private readonly SetIdentity identity;
        private readonly List<MirrorPair> pairs;
        private readonly List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates;
        private readonly List<NotPaired> noTestKept;

        private MirrorRule(
            int fromTheXml,
            int saved,
            int notRead,
            bool rulesRead,
            string ending,
            SetIdentity identity,
            List<MirrorPair> pairs,
            List<KeyValuePair<PlannedClashTest, PlannedClashTest>> duplicates,
            List<NotPaired> noTestKept)
        {
            this.fromTheXml = fromTheXml;
            this.saved = saved;
            this.notRead = notRead;
            this.rulesRead = rulesRead;
            this.ending = ending;
            this.identity = identity;
            this.pairs = pairs;
            this.duplicates = duplicates;
            this.noTestKept = noTestKept;
            Pairs = new ReadOnlyCollection<MirrorPair>(pairs);
        }

        /// <summary>Every pair, one per mirror, in the order the first test of its question came.</summary>
        public ReadOnlyCollection<MirrorPair> Pairs { get; private set; }

        /// <summary>
        /// The rule over tests in the XML's order, or in the document's where no XML was
        /// picked. The priority file is the one the run picked, NothingPicked where none was,
        /// and then every pair keeps the one first in the XML. The sets are the ones the
        /// tests' sides name, the picked XML's, ExchangeDocument.Sets, whose whole questions
        /// pair two tests of two sets alike, or null where none was read. A test read off the
        /// document with the ending pairs by its sides, never by its name.
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
            List<PlannedClashTest> fromTheXml = new List<PlannedClashTest>();
            List<PlannedClashTest> saved = new List<PlannedClashTest>();
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

                    names.Add(test.Name ?? string.Empty);

                    if (test.IsFromDocument)
                    {
                        saved.Add(test);
                        continue;
                    }

                    fromTheXml.Add(test);

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
                    string name = settings.NameFor(other.Name, names);

                    names.Add(name);
                    pairs.Add(new MirrorPair(
                        kept,
                        priorities.Of(kept.Name),
                        other,
                        priorities.Of(other.Name),
                        swapped ? MirrorKind.Swapped : MirrorKind.SameRules,
                        swapped ? null : identity.Alike(other, kept),
                        name,
                        swapped || identity.SidesSwapped(other, kept)));
                }
            }

            List<NotPaired> noTestKept = new List<NotPaired>();
            PairBySides(saved, priorities, settings, identity, pairs, noTestKept);

            return new MirrorRule(
                fromTheXml.Count, saved.Count, notRead, identity.RulesRead, settings.Ending, identity, pairs, duplicates,
                noTestKept);
        }

        /// <summary>
        /// The pairs among the tests saved in the document, read with no XML, by their sides
        /// and never by a name. Each saved test whose name ends with the ending, in the order
        /// they are saved, is a mirror of the one saved test without the ending whose question
        /// its sides ask as a mirror, AsAMirror, and that one is kept. A saved test with the
        /// ending is never a test kept, so a chain of endings is not followed. Every saved test
        /// with the ending that is not paired keeps its own clashes and is said with why.
        /// </summary>
        private static void PairBySides(
            List<PlannedClashTest> saved,
            PriorityMap priorities,
            MirrorSettings settings,
            SetIdentity identity,
            List<MirrorPair> pairs,
            List<NotPaired> notPaired)
        {
            foreach (PlannedClashTest test in saved)
            {
                if (!settings.CarriesTheEnding(test.Name))
                {
                    continue;
                }

                string said = Prefix + "   " + test.Name + " ends with " + settings.Ending + ", and ";

                if (!BothSidesRead(test))
                {
                    notPaired.Add(new NotPaired(
                        said + "a side of it was not read, so which saved test it mirrors is UNKNOWN" + MayBeCountedTwice,
                        true));
                    continue;
                }

                List<PlannedClashTest> asked = new List<PlannedClashTest>();
                List<string> askedWithTheEnding = new List<string>();
                int unknown = 0;

                foreach (PlannedClashTest other in saved)
                {
                    if (ReferenceEquals(other, test))
                    {
                        continue;
                    }

                    bool? mirror = identity.AsAMirror(test, other);
                    bool otherIsAMirror = settings.CarriesTheEnding(other.Name);

                    if (mirror == true && otherIsAMirror)
                    {
                        askedWithTheEnding.Add(other.Name);
                    }
                    else if (mirror == true)
                    {
                        asked.Add(other);
                    }
                    else if (mirror == null && !otherIsAMirror)
                    {
                        unknown++;
                    }
                }

                if (asked.Count != 1)
                {
                    bool mayBeCountedTwice;
                    string why = WhyNotPaired(asked, askedWithTheEnding, unknown, out mayBeCountedTwice);

                    notPaired.Add(new NotPaired(said + why, mayBeCountedTwice));
                    continue;
                }

                PlannedClashTest kept = asked[0];
                bool swapped = Same(test.Left, kept.Right) && Same(test.Right, kept.Left);

                pairs.Add(new MirrorPair(
                    kept,
                    priorities.Of(kept.Name),
                    test,
                    ClashPriority.None,
                    MirrorKind.Named,
                    swapped ? null : identity.Alike(test, kept),
                    test.Name,
                    swapped || identity.SidesSwapped(test, kept)));
            }
        }

        /// <summary>
        /// Why a saved test with the ending whose sides were read is not paired: two or more
        /// saved tests without the ending ask its question, or none does and whether one does
        /// is UNKNOWN for some, or only tests with the ending ask it, each UNKNOWN and a clash
        /// both find may be counted twice, or no other saved test asks it at all.
        /// </summary>
        private static string WhyNotPaired(
            List<PlannedClashTest> asked, List<string> askedWithTheEnding, int unknown, out bool mayBeCountedTwice)
        {
            mayBeCountedTwice = true;

            if (asked.Count > 1)
            {
                List<string> names = new List<string>();

                foreach (PlannedClashTest test in asked)
                {
                    names.Add(test.Name);
                }

                return "its sides ask the question of " + Joined(names) + " as a mirror, so which one it mirrors is "
                    + "UNKNOWN" + MayBeCountedTwice;
            }

            if (unknown > 0)
            {
                return "whether its sides ask the question of " + unknown + (unknown == 1 ? " saved test" : " saved tests")
                    + " is UNKNOWN, a side or a set's rule list not read, so which saved test it mirrors is UNKNOWN"
                    + MayBeCountedTwice;
            }

            if (askedWithTheEnding.Count > 0)
            {
                return "its sides ask the question only of " + Joined(askedWithTheEnding)
                    + (askedWithTheEnding.Count == 1 ? ", whose name ends with it too" : ", whose names end with it too")
                    + ", so which of them is kept is UNKNOWN" + MayBeCountedTwice;
            }

            mayBeCountedTwice = false;
            return "its sides ask the question of no other saved test as a mirror, so it keeps its own clashes";
        }

        /// <summary>
        /// Bader's answer A to Q136. Each test saved in the document under the XML's name of a
        /// mirror, which an NWF made before the mirror rule holds, is planned to be renamed to
        /// its own name with the ending, the name this tool gives that mirror, MirrorPair
        /// .MirrorName, never the kept test's, and run as it, its statuses kept, where its
        /// sides ask the kept test's question as a mirror by the rule the pair was read by,
        /// AsAMirror. Refused and said where the new name is taken in the document, where two
        /// saved tests carry the old name, and where its sides do not ask that question or
        /// were not read. The saved tests are the document's, ClashTestPlan.FromDocument's
        /// buildable tests, their sides as the add-in reads them.
        /// </summary>
        public MirrorRenames RenamesIn(IEnumerable<PlannedClashTest> savedInTheDocument)
        {
            Dictionary<string, List<PlannedClashTest>> byName =
                new Dictionary<string, List<PlannedClashTest>>(StringComparer.Ordinal);
            int handed = 0;

            if (savedInTheDocument != null)
            {
                foreach (PlannedClashTest test in savedInTheDocument)
                {
                    if (test == null)
                    {
                        continue;
                    }

                    handed++;

                    List<PlannedClashTest> named;
                    string name = test.Name ?? string.Empty;

                    if (!byName.TryGetValue(name, out named))
                    {
                        named = new List<PlannedClashTest>();
                        byName.Add(name, named);
                    }

                    named.Add(test);
                }
            }

            List<MirrorRename> planned = new List<MirrorRename>();
            List<string> each = new List<string>();

            foreach (MirrorPair pair in pairs)
            {
                List<PlannedClashTest> old;

                // A mirror whose own name already ends with the ending is saved under the
                // name it runs under, so there is nothing to rename.
                if (pair.Mirror.IsFromDocument
                    || string.Equals(pair.Mirror.Name, pair.MirrorName, StringComparison.Ordinal)
                    || !byName.TryGetValue(pair.Mirror.Name, out old))
                {
                    continue;
                }

                string said = Prefix + "   " + pair.Mirror.Name + ", saved before the mirror rule under the XML's name "
                    + "of the mirror of " + pair.Kept.Name + ", ";

                if (byName.ContainsKey(pair.MirrorName))
                {
                    each.Add(said + "is not renamed " + pair.MirrorName + ", because the document already holds a "
                        + "test of that name, so it is left as it is and not run");
                    continue;
                }

                if (old.Count != 1)
                {
                    each.Add(said + "is not renamed, because the document holds " + old.Count + " tests of that "
                        + "name, so which one is the mirror is UNKNOWN and each is left as it is and not run");
                    continue;
                }

                bool? mirror = identity.AsAMirror(old[0], pair.Kept);

                if (mirror != true)
                {
                    each.Add(said + "is not renamed, because " + (mirror == null
                        ? "whether its sides ask the question of " + pair.Kept.Name + " is UNKNOWN, a side or its set "
                            + "not read"
                        : "its sides do not ask the question of " + pair.Kept.Name + " as a mirror")
                        + ", so it is left as it is and not run");
                    continue;
                }

                planned.Add(new MirrorRename(old[0], pair.MirrorName, pair.Kept.Name));
                each.Add(said + "is renamed " + pair.MirrorName + ", its statuses kept, and run as that mirror, its "
                    + "clashes merged into " + pair.Kept.Name + "'s");
            }

            int refused = each.Count - planned.Count;
            List<string> lines = new List<string>
            {
                Prefix + "   " + planned.Count + " of the " + handed + (handed == 1 ? " test" : " tests")
                    + " saved in the document " + (planned.Count == 1 ? "is" : "are") + " renamed as the mirror "
                    + (planned.Count == 1 ? "it was" : "they were") + " saved for before the mirror rule, and "
                    + refused + " under the XML's name of a mirror " + (refused == 1 ? "is" : "are") + " not"
            };

            lines.AddRange(each);
            return new MirrorRenames(planned, lines);
        }

        /// <summary>
        /// The MIRROR lines for the log. One line counting the pairs among the XML's tests whose
        /// two sets were read, always where the XML's tests were handed, because a missing line
        /// reads as a check that did not run and a count of pairs alone reads as a check of
        /// every test. Then once, where no rule list was read, that only the same two sets
        /// swapped pair. Then, where tests saved in the document were handed, one line counting
        /// the pairs found by their sides and saying that saved tests whose names do not end
        /// with the ending are UNKNOWN. Then once, where any test has a side not read, how
        /// many, said UNKNOWN. Then each saved test with the ending not paired, with why, five
        /// named and the rest counted, the count saying how many of them may have a clash both
        /// find counted twice and how many ask a question no other saved test asks, Bader's
        /// answer A to Q137. Then every pair whose two tests
        /// differ in priority or in a setting TestDrift compares, each with both values,
        /// because Bader asked for both in the log. Then the pairs alike in those, never said
        /// to be alike in everything, five named and the rest counted, the rule every repeated
        /// line here follows. Then the duplicates the same way.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            int swapped = OfKind(MirrorKind.Swapped);
            int sameRules = OfKind(MirrorKind.SameRules);
            int named = OfKind(MirrorKind.Named);

            if (fromTheXml > 0 || saved == 0)
            {
                int read = fromTheXml - notRead;
                string among = " among the " + read + (read == 1 ? " test" : " tests") + " whose two sets were read";

                if (swapped + sameRules == 0)
                {
                    lines.Add(Prefix + "   0 pairs" + among + ", so every test keeps its own clashes");
                }
                else
                {
                    lines.Add(Prefix + "   " + (swapped + sameRules) + (swapped + sameRules == 1 ? " pair" : " pairs")
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
            }

            if (saved > 0)
            {
                lines.Add(Prefix + "   " + named + (named == 1 ? " pair" : " pairs") + " among the " + saved
                    + (saved == 1 ? " test" : " tests") + " saved in the document, which pair by their sides and never "
                    + "by a name: a test whose name ends with " + ending + " pairs with the one saved test without it "
                    + "whose question its sides ask as a mirror. Whether two saved tests whose names do not end with "
                    + "it ask one question is UNKNOWN, so each keeps its own clashes"
                    + (rulesRead
                        ? string.Empty
                        : ". No rule list of a set was read, so only a test with the same two sets swapped pairs")
                    + (named == 0
                        ? string.Empty
                        : ". Both tests of each pair are run as they are saved, and their clashes are merged by the "
                            + "pair of items into the one without the ending"));
            }

            if (notRead > 0)
            {
                lines.Add(Prefix + "   " + notRead + " of the " + fromTheXml
                    + (notRead == 1
                        ? " tests has a side whose set was not read, so whether it is a mirror or a duplicate "
                            + "is UNKNOWN and its clashes are not merged"
                        : " tests have a side whose set was not read, so whether each is a mirror or a duplicate "
                            + "is UNKNOWN and none of their clashes is merged"));
            }

            AddNotPaired(lines);

            if (pairs.Count > 0)
            {
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

                AddFive(lines, alike, "pair", "pairs", " alike in priority and in every test setting compared, "
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

        private int OfKind(MirrorKind kind)
        {
            int count = 0;

            foreach (MirrorPair pair in pairs)
            {
                if (pair.Kind == kind)
                {
                    count++;
                }
            }

            return count;
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

        /// <summary>
        /// The saved tests with the ending not paired, five named with why and then one line
        /// counting the rest, Bader's answer A to Q137: how many of them are UNKNOWN, so a clash
        /// both find may be counted twice, and how many ask a question no other saved test asks
        /// as a mirror, so nothing is counted twice, each keeping its own clashes.
        /// </summary>
        private void AddNotPaired(List<string> lines)
        {
            int shown = Math.Min(noTestKept.Count, RunLog.KeptOfARepeat);
            int twice = 0;
            int alone = 0;

            for (int i = 0; i < noTestKept.Count; i++)
            {
                if (i < shown)
                {
                    lines.Add(noTestKept[i].Line);
                }
                else if (noTestKept[i].MayBeCountedTwice)
                {
                    twice++;
                }
                else
                {
                    alone++;
                }
            }

            int more = twice + alone;

            if (more > 0)
            {
                lines.Add(Prefix + "   and " + more + (more == 1 ? " more saved test" : " more saved tests")
                    + " with the ending not paired, each keeping its own clashes, counted and not listed: " + twice
                    + " for which the saved test it mirrors is UNKNOWN, so a clash both find may be counted twice, and "
                    + alone + " whose question no other saved test asks as a mirror");
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

        /// <summary>One saved test with the ending not paired: its line, and whether a clash both find may be counted twice.</summary>
        private sealed class NotPaired
        {
            internal NotPaired(string line, bool mayBeCountedTwice)
            {
                Line = line;
                MayBeCountedTwice = mayBeCountedTwice;
            }

            internal string Line { get; private set; }

            internal bool MayBeCountedTwice { get; private set; }
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
            private readonly HashSet<string> withRules = new HashSet<string>(StringComparer.Ordinal);

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

                        if (set.Path != null)
                        {
                            withRules.Add(set.Path);
                        }
                    }

                    if (set.Path != null && !names.ContainsKey(set.Path))
                    {
                        names.Add(set.Path, set.Name);
                    }
                }

                foreach (IdenticalSets alike in SetWarnings.FindIdentical(all, set => set.WholeQuestion))
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
            /// Whether a test asks the question of the kept test as its mirror, the one rule a
            /// saved test with the ending is paired by and an old test is renamed by, Q136 A:
            /// true where both tests' sides were read, the two are not the same two sets in the
            /// same order, and their question keys are the same, false where the sides were
            /// read and the two are a duplicate, or ask other questions with the rule list of
            /// each of their sets read, and null, UNKNOWN, where a side was not read or a set's
            /// rule list was not, since two sets of one whole question are only known by their
            /// rule lists.
            /// </summary>
            internal bool? AsAMirror(PlannedClashTest test, PlannedClashTest kept)
            {
                if (!BothSidesRead(test) || !BothSidesRead(kept))
                {
                    return null;
                }

                if (Same(test.Left, kept.Left) && Same(test.Right, kept.Right))
                {
                    return false;
                }

                string key = QuestionKey(test);

                if (key != null && string.Equals(key, QuestionKey(kept), StringComparison.Ordinal))
                {
                    return true;
                }

                if (withRules.Contains(test.Left.Locator) && withRules.Contains(test.Right.Locator)
                    && withRules.Contains(kept.Left.Locator) && withRules.Contains(kept.Right.Locator))
                {
                    return false;
                }

                return null;
            }

            /// <summary>
            /// The sets of the mirror that differ by name from the kept test's set of the same
            /// rule list, each as the mirror's set then the kept test's, by their set names.
            /// </summary>
            internal string Alike(PlannedClashTest mirror, PlannedClashTest kept)
            {
                bool straight = !SidesSwapped(mirror, kept);
                PlannedClashSide keptForLeft = straight ? kept.Left : kept.Right;
                PlannedClashSide keptForRight = straight ? kept.Right : kept.Left;
                List<string> said = new List<string>();

                Say(said, mirror.Left, keptForLeft);
                Say(said, mirror.Right, keptForRight);
                return string.Join(" and ", said.ToArray());
            }

            /// <summary>
            /// Whether the mirror's left side stands for the kept test's right side, read by
            /// the sets each stands for, so its flags are compared side for side.
            /// </summary>
            internal bool SidesSwapped(PlannedClashTest mirror, PlannedClashTest kept)
            {
                return !string.Equals(
                    Standing(mirror.Left.Locator), Standing(kept.Left.Locator), StringComparison.Ordinal);
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
