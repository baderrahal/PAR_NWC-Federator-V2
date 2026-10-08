using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;

namespace Federator.Core.Coverage
{
    /// <summary>
    /// Every test of the picked file against what happened to it, one reason each, F127,
    /// Bader's request 2 under Q112, section 1.2 of the F127 design, which the loop keeps outside the repo as turn5\f127-design.md.
    ///
    /// THE REASON IS THE RUNNER'S OWN RECORD, ClashRunOutcome, read in the order the code
    /// applies it. The plan drops a test it cannot build before any model is looked at. A
    /// test naming a set not in the document is dropped next. F77's creation plan keeps out
    /// a test whose side finds nothing. Only a test that is in the document then meets the
    /// coordinates rule or the one discipline rule, and only one that passes those meets the
    /// run time side check and runs. So a test F77 kept out of a group whose clash the
    /// coordinates rule skipped keeps its side reason, which is what keeps the gaps of the
    /// matrix visible on both wave buildings. Read the other way round every set gap there
    /// would read as a coordinates skip.
    ///
    /// A SIDE THAT FOUND NOTHING IS JUDGED BY ITS SET'S CODE, read through the one reader,
    /// DisciplinePairRule.CodeIn, off the set's own name, until F131 re-points it to the team
    /// map. The code against the codes of the group's files and of every file of the run says
    /// whether the discipline is not in the group, whether the group carries it and the set
    /// still found nothing, or whether no file of the run carries it, FF, PL and DR on this
    /// project, so it is UNKNOWN until the team map. Where both sides found nothing the
    /// stronger reason is given and both sets are named, in the order of SideDiscipline.
    ///
    /// A COUNT OF MINUS ONE IS NEVER A SET THAT FOUND NOTHING. It is a side nobody counted
    /// and reads UNKNOWN, and a side that did find nothing beside it still gives its reason,
    /// because a definite reason is given wherever one exists.
    ///
    /// THE RUNNER KEYS ITS RECORD ON THE NAME, so a test sharing its name with another test
    /// of the file, one the plan dropped included, reads UNKNOWN wherever its reason would
    /// come off that record, rather than taking the other's result. A test the plan dropped
    /// keeps the plan's own reason, because the plan keys it on its place in the file. A run
    /// with no record at all reads UNKNOWN, except for the tests the plan itself dropped.
    ///
    /// THE SAVED TESTS WITH NO XML. The plan is then read off the tests saved in the
    /// document, and a test it drops, a type number this tool does not run or no name, was
    /// read out of the document. It is already there and not run, with the plan's words, and
    /// never the test was not created.
    /// </summary>
    public static class CoverageRule
    {
        /// <summary>
        /// One row per test of the plan, in the order of the file, or of the document where
        /// the plan was read off the tests saved there. The outcome is the clash step's
        /// record, null where it never ran. The codes are the discipline codes of the
        /// group's files and of every file of the run, null where they were not read, and
        /// an empty set of the group's is read as not read too, because a group whose files
        /// gave no code says nothing about which disciplines it holds. The settings say
        /// which codes a set name can carry.
        /// </summary>
        public static IList<TestCoverage> For(
            ClashTestPlan plan,
            ClashRunOutcome outcome,
            ICollection<string> groupCodes,
            ICollection<string> runCodes,
            ViewpointSettings codes)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            if (codes == null)
            {
                throw new ArgumentNullException("codes");
            }

            SortedDictionary<int, PlannedClashTest> buildable = new SortedDictionary<int, PlannedClashTest>();
            SortedDictionary<int, SkippedClashTest> dropped = new SortedDictionary<int, SkippedClashTest>();
            Dictionary<string, int> testsNamed = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (PlannedClashTest test in plan.Buildable)
            {
                if (!buildable.ContainsKey(test.FileIndex))
                {
                    buildable.Add(test.FileIndex, test);

                    int already;
                    testsNamed.TryGetValue(test.Name, out already);
                    testsNamed[test.Name] = already + 1;
                }
            }

            foreach (SkippedClashTest test in plan.Skipped)
            {
                if (!buildable.ContainsKey(test.FileIndex) && !dropped.ContainsKey(test.FileIndex))
                {
                    dropped.Add(test.FileIndex, test);

                    // The runner adds the plan's own skips to its record first, under the
                    // same name, so a buildable test of this name cannot be told from it.
                    int already;
                    testsNamed.TryGetValue(test.Name, out already);
                    testsNamed[test.Name] = already + 1;
                }
            }

            List<int> positions = new List<int>(buildable.Keys);
            positions.AddRange(dropped.Keys);
            positions.Sort();

            Codes judged = new Codes(groupCodes, runCodes, codes);
            List<TestCoverage> rows = new List<TestCoverage>();

            foreach (int index in positions)
            {
                PlannedClashTest test;

                if (buildable.TryGetValue(index, out test))
                {
                    rows.Add(Judge(index + 1, test, testsNamed[test.Name], outcome, judged));
                    continue;
                }

                SkippedClashTest skipped = dropped[index];
                rows.Add(plan.Source == ClashPlanSource.Document
                    ? new TestCoverage(
                        index + 1, skipped.Name, string.Empty, string.Empty, -1, -1,
                        TestPresence.AlreadyThere, false, -1, CoverageReason.SavedTestNotRun, skipped.Reason)
                    : new TestCoverage(
                        index + 1, skipped.Name, string.Empty, string.Empty, -1, -1,
                        TestPresence.Unknown, false, -1, CoverageReason.NotCreated, NotCreatedDetail(skipped)));
            }

            return rows;
        }

        /// <summary>
        /// The coverage reason for each kind the runner records. Every kind maps to one, and
        /// a test enumerates the kinds, so one added to ClashSkipReason fails there until it
        /// is mapped here rather than reading UNKNOWN on every test it skips. EmptySide maps
        /// to the side reason the side rule then sharpens.
        /// </summary>
        internal static CoverageReason ReasonFor(ClashSkipReason kind)
        {
            switch (kind)
            {
                case ClashSkipReason.UnknownTestType:
                case ClashSkipReason.NoLocator:
                case ClashSkipReason.UnknownUnits:
                case ClashSkipReason.NoTolerance:
                case ClashSkipReason.NoName:
                case ClashSkipReason.LocatorNotResolved:
                    return CoverageReason.NotCreated;
                case ClashSkipReason.EmptySide:
                    return CoverageReason.SideFoundNothing;
                case ClashSkipReason.SingleDiscipline:
                    return CoverageReason.OneDiscipline;
                case ClashSkipReason.NotOnTheSameCoordinates:
                    return CoverageReason.CoordinatesRule;
                case ClashSkipReason.Failed:
                    return CoverageReason.Failed;
                default:
                    return CoverageReason.Unknown;
            }
        }

        /// <summary>
        /// What one side says about the group. Minus one is nobody counted it. A code is
        /// read off the set's own name, the end of the locator, never trimmed.
        /// </summary>
        internal static SideDiscipline JudgeSide(string locator, int items, Codes judged)
        {
            if (items < 0)
            {
                return SideDiscipline.NotCounted;
            }

            string code = judged.CodeOf(locator);

            if (code.Length == 0)
            {
                return SideDiscipline.NoCode;
            }

            if (judged.Group != null && judged.Group.Contains(code))
            {
                return SideDiscipline.InGroup;
            }

            if (judged.Group == null || judged.Group.Count == 0 || judged.Run == null)
            {
                return SideDiscipline.CodesNotRead;
            }

            return judged.Run.Contains(code) ? SideDiscipline.NotInGroup : SideDiscipline.NoModelOfTheRun;
        }

        private static TestCoverage Judge(
            int position, PlannedClashTest test, int testsOfThisName, ClashRunOutcome outcome, Codes judged)
        {
            string left = test.Left == null ? string.Empty : test.Left.Locator;
            string right = test.Right == null ? string.Empty : test.Right.Locator;

            if (outcome == null)
            {
                return new TestCoverage(position, test.Name, left, right, -1, -1, TestPresence.Unknown, false, -1,
                    CoverageReason.Unknown, "the clash step left no record for this group");
            }

            if (outcome.Stopped)
            {
                return new TestCoverage(position, test.Name, left, right, -1, -1, TestPresence.Unknown, false, -1,
                    CoverageReason.NoTestResolvesASet, outcome.StoppedReason);
            }

            if (testsOfThisName > 1)
            {
                return new TestCoverage(position, test.Name, left, right, -1, -1, TestPresence.Unknown, false, -1,
                    CoverageReason.Unknown, "the name is on " + testsOfThisName
                        + " tests of the file, and the run keys its record on the name, so they cannot be told apart");
            }

            TestPresence presence = PresenceOf(test.Name, outcome);
            ClashTestResult ran = RanRecord(test.Name, outcome);

            if (ran != null)
            {
                return new TestCoverage(position, test.Name, left, right, ran.LeftItems, ran.RightItems, presence,
                    true, ran.Tally.Total,
                    ran.Tally.Total > 0 ? CoverageReason.HasClashes : CoverageReason.RanAndFoundNone, string.Empty);
            }

            SkippedClashTest skipped = SkipRecord(test.Name, outcome);
            int leftItems;
            int rightItems;
            SidesOf(test, outcome, out leftItems, out rightItems);

            if (skipped == null)
            {
                return outcome.StopTheRun
                    ? new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                        CoverageReason.NotReached, outcome.StopTheRunReason)
                    : new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                        CoverageReason.Unknown, "the run holds no record of this test");
            }

            if (skipped.Kind == ClashSkipReason.NotOnTheSameCoordinates || skipped.Kind == ClashSkipReason.Failed)
            {
                return new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                    ReasonFor(skipped.Kind), skipped.Reason);
            }

            if (skipped.Kind == ClashSkipReason.SingleDiscipline)
            {
                return new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                    CoverageReason.OneDiscipline, judged.GroupWords());
            }

            if (skipped.Kind == ClashSkipReason.EmptySide)
            {
                return SideReason(position, test, left, right, leftItems, rightItems, presence, judged);
            }

            CoverageReason reason = ReasonFor(skipped.Kind);

            return new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                reason, reason == CoverageReason.NotCreated ? NotCreatedDetail(skipped) : skipped.Reason);
        }

        private static TestCoverage SideReason(
            int position, PlannedClashTest test, string left, string right, int leftItems, int rightItems,
            TestPresence presence, Codes judged)
        {
            List<KeyValuePair<SideDiscipline, string>> sides = new List<KeyValuePair<SideDiscipline, string>>();
            SideDiscipline? strongest = null;

            foreach (bool isLeft in new[] { true, false })
            {
                string locator = isLeft ? left : right;
                int items = isLeft ? leftItems : rightItems;

                if (items > 0)
                {
                    continue;
                }

                // The declared order of SideDiscipline is its strength, the strongest first.
                SideDiscipline side = JudgeSide(locator, items, judged);

                if (strongest == null || side < strongest.Value)
                {
                    strongest = side;
                }

                string code = judged.CodeOf(locator);
                sides.Add(new KeyValuePair<SideDiscipline, string>(side, (isLeft ? "left \"" : "right \"") + locator
                    + "\" " + (items < 0 ? "not counted" : "0 items")
                    + (code.Length == 0 ? ", no discipline code in its name" : ", code " + code)));
            }

            if (strongest == null)
            {
                return new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                    CoverageReason.Unknown, "the run recorded a side finding nothing, and the counts in hand read left "
                        + leftItems + " and right " + rightItems);
            }

            // Where one side found nothing and the other was not counted, the side that found
            // nothing gives the reason and the one nobody counted is not named as a cause.
            List<string> said = new List<string>();

            foreach (KeyValuePair<SideDiscipline, string> side in sides)
            {
                if (side.Key != SideDiscipline.NotCounted || strongest.Value == SideDiscipline.NotCounted)
                {
                    said.Add(side.Value);
                }
            }

            said.Add(judged.GroupWords());

            return new TestCoverage(position, test.Name, left, right, leftItems, rightItems, presence, false, -1,
                ReasonOf(strongest.Value), string.Join(", ", said.ToArray()));
        }

        private static CoverageReason ReasonOf(SideDiscipline side)
        {
            switch (side)
            {
                case SideDiscipline.NotInGroup:
                    return CoverageReason.DisciplineNotInGroup;
                case SideDiscipline.InGroup:
                    return CoverageReason.SideFoundNothing;
                case SideDiscipline.NoModelOfTheRun:
                    return CoverageReason.NoModelOfTheRunCarriesTheCode;
                case SideDiscipline.NoCode:
                    return CoverageReason.SetNameCarriesNoCode;
                case SideDiscipline.CodesNotRead:
                    return CoverageReason.CodesNotRead;
                default:
                    return CoverageReason.SideNotCounted;
            }
        }

        /// <summary>
        /// The counts the side reason reads: what the run time check read where it reached
        /// the test, because that decided the skip, and otherwise what the creation plan was
        /// handed, because that decided the test was not created. Minus one where neither
        /// counted the side.
        /// </summary>
        private static void SidesOf(PlannedClashTest test, ClashRunOutcome outcome, out int left, out int right)
        {
            if (outcome.TrySidesRead(test.Name, out left, out right))
            {
                return;
            }

            IDictionary<string, int> counts = outcome.ItemsByLocator;
            int items;

            left = PlannedClashSide.Counted(counts, test.Left, out items) ? items : -1;
            right = PlannedClashSide.Counted(counts, test.Right, out items) ? items : -1;
        }

        /// <summary>
        /// Created this run, already there, or kept out by F77: a test with a side finding
        /// nothing that the runner neither created nor found already there was absent and
        /// was left out, because every run time skip follows one of the other two.
        /// </summary>
        private static TestPresence PresenceOf(string name, ClashRunOutcome outcome)
        {
            if (Holds(outcome.CreatedNames, name))
            {
                return TestPresence.CreatedThisRun;
            }

            if (Holds(outcome.AlreadyPresentNames, name))
            {
                return TestPresence.AlreadyThere;
            }

            SkippedClashTest skipped = SkipRecord(name, outcome);

            return skipped != null && skipped.Kind == ClashSkipReason.EmptySide
                ? TestPresence.NotInDocument
                : TestPresence.Unknown;
        }

        private static bool Holds(IList<string> names, string name)
        {
            foreach (string held in names)
            {
                if (string.Equals(held, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static ClashTestResult RanRecord(string name, ClashRunOutcome outcome)
        {
            foreach (ClashTestResult result in outcome.Ran)
            {
                if (string.Equals(result.Name, name, StringComparison.Ordinal))
                {
                    return result;
                }
            }

            return null;
        }

        private static SkippedClashTest SkipRecord(string name, ClashRunOutcome outcome)
        {
            foreach (SkippedClashTest skipped in outcome.Skipped)
            {
                if (string.Equals(skipped.Name, name, StringComparison.Ordinal))
                {
                    return skipped;
                }
            }

            return null;
        }

        private static string NotCreatedDetail(SkippedClashTest skipped)
        {
            return ClashTestPlan.Describe(skipped.Kind) + ", " + skipped.Reason;
        }

        /// <summary>The codes a side is judged against, and how a set name's code is read.</summary>
        internal sealed class Codes
        {
            private readonly ViewpointSettings settings;

            internal Codes(ICollection<string> group, ICollection<string> run, ViewpointSettings settings)
            {
                Group = group == null ? null : new HashSet<string>(group, StringComparer.Ordinal);
                Run = run == null ? null : new HashSet<string>(run, StringComparer.Ordinal);
                this.settings = settings;
            }

            /// <summary>The codes of the group's files, null where they were not read.</summary>
            internal HashSet<string> Group { get; private set; }

            /// <summary>The codes of every file of the run, null where they were not read.</summary>
            internal HashSet<string> Run { get; private set; }

            /// <summary>The code in the set's own name, the end of its locator, or empty where it carries none.</summary>
            internal string CodeOf(string locator)
            {
                return DisciplinePairRule.CodeIn(ByDesignRule.SetNameIn(locator), settings);
            }

            internal string GroupWords()
            {
                if (Group == null)
                {
                    return "the disciplines of the group's models were not read";
                }

                List<string> codes = new List<string>(Group);
                codes.Sort(StringComparer.Ordinal);

                return codes.Count == 0
                    ? "the group's files carry no discipline code"
                    : "the group holds " + string.Join(", ", codes.ToArray());
            }
        }
    }
}
