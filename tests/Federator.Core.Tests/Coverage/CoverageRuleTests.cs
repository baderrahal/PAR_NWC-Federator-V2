using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Exchange;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Every test of the picked file against what happened, one reason each, F127, Bader's
    /// request 2 under Q112: created or not, run or not, and for a test with no results why.
    /// The reason is the runner's own record, read in the order the code applies it, so F77
    /// keeping a test out comes before the coordinates rule skipping the clash.
    ///
    /// The codes in these tests are those of set 03's C06 run, group 100000: its files carry
    /// AR and ME, and the run's files AR, EL, ME and ST, C06 log line 101. Sample data only.
    /// </summary>
    [TestFixture]
    public class CoverageRuleTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string ArFloors = Root + "/Architecture/BLD-AR-Floors";
        private const string ArStairs = Root + "/Architecture/BLD-AR-Stairs";
        private const string ArWalls = Root + "/Architecture/BLD-AR-Walls";
        private const string StFoundation = Root + "/Structure/BLD-ST-Foundation";
        private const string FfSprinklers = Root + "/Mechanical/Mechanical-Fire Fighting/BLD-FF-Sprinklers";
        private const string Security = Root + "/Electrical/BLD-Security Devices";

        private static readonly string[] GroupCodes = { "AR", "ME" };
        private static readonly string[] RunCodes = { "AR", "EL", "ME", "ST" };

        // ---------- a plan and a run to read ----------

        private static string Test(string name, string left, string right)
        {
            return Test(name, left, right, "hard_conservative", true);
        }

        private static string Test(string name, string left, string right, string type, bool tolerance)
        {
            return "<clashtest name=\"" + SecurityElement.Escape(name) + "\" test_type=\"" + type + "\""
                + (tolerance ? " tolerance=\"0.2460629921\"" : string.Empty) + " merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + SecurityElement.Escape(left) + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + SecurityElement.Escape(right) + "</locator></clashselection></right>"
                + "</clashtest>";
        }

        private static ClashTestPlan Plan(params string[] tests)
        {
            StringBuilder xml = new StringBuilder("<exchange units=\"ft\"><batchtest name=\"b\">");

            foreach (string test in tests)
            {
                xml.Append(test);
            }

            xml.Append("</batchtest></exchange>");
            return ClashTestPlan.From(new ExchangeReader().ReadText(xml.ToString()), "ft");
        }

        private static Dictionary<string, int> Counts(params object[] locatorThenItems)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < locatorThenItems.Length; i += 2)
            {
                counts[(string)locatorThenItems[i]] = (int)locatorThenItems[i + 1];
            }

            return counts;
        }

        private static ClashTally Clashes(int howMany)
        {
            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, howMany);
            return tally;
        }

        /// <summary>
        /// What the runner records for a plan, in its own order: the plan's skips, F77's
        /// creation plan over the counts, and every created test run with no clash.
        /// </summary>
        private static ClashRunOutcome RunAsTheRunnerDoes(ClashTestPlan plan, IDictionary<string, int> counts)
        {
            ClashRunOutcome outcome = new ClashRunOutcome();

            foreach (SkippedClashTest skipped in plan.Skipped)
            {
                outcome.AddSkipped(skipped);
            }

            CreationPlan creation = CreationPlan.For(plan.Buildable, counts);
            outcome.KeepItemsByLocator(counts);

            foreach (SkippedClashTest skipped in creation.NotCreated)
            {
                outcome.AddSkipped(skipped);
            }

            foreach (PlannedClashTest test in creation.Create)
            {
                outcome.AddCreated(test.Name);
                outcome.AddRan(test.Name, counts[test.Left.Locator], counts[test.Right.Locator], Clashes(0), 0.1);
            }

            return outcome;
        }

        private static IList<TestCoverage> Coverage(ClashTestPlan plan, ClashRunOutcome outcome)
        {
            return CoverageRule.For(plan, outcome, GroupCodes, RunCodes, new ViewpointSettings());
        }

        private static TestCoverage Only(ClashTestPlan plan, ClashRunOutcome outcome)
        {
            IList<TestCoverage> rows = Coverage(plan, outcome);
            Assert.That(rows.Count, Is.EqualTo(1));
            return rows[0];
        }

        // ---------- it ran ----------

        [Test]
        public void ATestThatRanAndFoundClashesHasClashes()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.AddRan("T", 2, 2, Clashes(3), 0.1);

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.HasClashes));
            Assert.That(row.Presence, Is.EqualTo(TestPresence.CreatedThisRun));
            Assert.That(row.Ran, Is.True);
            Assert.That(row.ClashesFound, Is.EqualTo(3));
            Assert.That(row.LeftItems, Is.EqualTo(2));
        }

        [Test]
        public void ATestThatRanAndFoundNoneSaysSo()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.AddRan("T", 2, 2, Clashes(0), 0.1);

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.RanAndFoundNone));
            Assert.That(row.ClashesFound, Is.EqualTo(0));
        }

        // Presence is kept apart from the reason: a test already in the NWF ran as well.
        [Test]
        public void ATestAlreadyThereThatRanIsAlreadyThere()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddAlreadyPresent("T");
            outcome.AddRan("T", 2, 2, Clashes(1), 0.1);

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Presence, Is.EqualTo(TestPresence.AlreadyThere));
            Assert.That(row.Reason, Is.EqualTo(CoverageReason.HasClashes));
        }

        // ---------- it was not created ----------

        [Test]
        public void ATestThePlanDroppedBeforeTheModelWasNotCreatedAndSaysWhy()
        {
            ClashTestPlan plan = Plan(
                Test("unknown type", ArWalls, ArFloors, "bogus", true),
                Test("no tolerance", ArWalls, ArFloors, "hard_conservative", false));

            IList<TestCoverage> rows = Coverage(plan, RunAsTheRunnerDoes(plan, Counts()));

            Assert.That(rows[0].Reason, Is.EqualTo(CoverageReason.NotCreated));
            Assert.That(rows[0].Detail, Does.Contain(ClashTestPlan.Describe(ClashSkipReason.UnknownTestType)));
            Assert.That(rows[0].Detail, Does.Contain("bogus"));
            Assert.That(rows[1].Reason, Is.EqualTo(CoverageReason.NotCreated));
            Assert.That(rows[1].Detail, Does.Contain(ClashTestPlan.Describe(ClashSkipReason.NoTolerance)));

            // The runner never looked for these in the document.
            Assert.That(rows[0].Presence, Is.EqualTo(TestPresence.Unknown));
            Assert.That(rows[0].Ran, Is.False);
        }

        [Test]
        public void ATestNamingASetNotInTheDocumentWasNotCreated()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashTestPlan resolved = plan.ResolveAgainst(new[] { ArWalls });
            ClashRunOutcome outcome = new ClashRunOutcome();

            foreach (SkippedClashTest skipped in resolved.Skipped)
            {
                outcome.AddSkipped(skipped);
            }

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.NotCreated));
            Assert.That(row.Detail, Does.Contain(ClashTestPlan.Describe(ClashSkipReason.LocatorNotResolved)));
            Assert.That(row.Detail, Does.Contain(ArFloors));
        }

        [Test]
        public void ARunStoppedBecauseNoTestResolvesASetSaysSoForEveryTest()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors), Test("U", ArWalls, ArStairs));
            ClashRunOutcome outcome = new ClashRunOutcome();
            string why = outcome.StopBecauseNoSetResolves(0, 3);
            IList<TestCoverage> rows = Coverage(plan, outcome);

            Assert.That(rows.Count, Is.EqualTo(2));

            foreach (TestCoverage row in rows)
            {
                Assert.That(row.Reason, Is.EqualTo(CoverageReason.NoTestResolvesASet));
                Assert.That(row.Detail, Is.EqualTo(why));
            }
        }

        // ---------- a side found nothing, F77 kept it out ----------

        [Test]
        public void TheLeftSideEmptyInTheGroupNamesTheLeftSet()
        {
            ClashTestPlan plan = Plan(Test("T", ArStairs, ArFloors));
            TestCoverage row = Only(plan, RunAsTheRunnerDoes(plan, Counts(ArStairs, 0, ArFloors, 2)));

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(row.Presence, Is.EqualTo(TestPresence.NotInDocument));
            Assert.That(row.Detail, Does.Contain(ArStairs));
            Assert.That(row.Detail, Does.Not.Contain(ArFloors), "the side that found items is not the reason");
            Assert.That(row.LeftItems, Is.EqualTo(0));
            Assert.That(row.RightItems, Is.EqualTo(2));
        }

        [Test]
        public void TheRightSideEmptyNamesTheRightSet()
        {
            ClashTestPlan plan = Plan(Test("T", ArFloors, ArStairs));
            TestCoverage row = Only(plan, RunAsTheRunnerDoes(plan, Counts(ArStairs, 0, ArFloors, 2)));

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(row.Detail, Does.Contain(ArStairs));
            Assert.That(row.Detail, Does.Not.Contain(ArFloors));
        }

        [Test]
        public void ASetOfACodeNoFileOfTheGroupCarriesIsTheDisciplineNotInTheGroup()
        {
            ClashTestPlan plan = Plan(Test("T", StFoundation, ArFloors));
            TestCoverage row = Only(plan, RunAsTheRunnerDoes(plan, Counts(StFoundation, 0, ArFloors, 2)));

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.DisciplineNotInGroup));
            Assert.That(row.Detail, Does.Contain("ST"));
            Assert.That(row.Detail, Does.Contain(StFoundation));
        }

        /// <summary>
        /// FF, PL and DR on this project: no file of the run carries the code, so whether the
        /// discipline is in the group is UNKNOWN until the team map, and the set found no
        /// items is all that is known.
        /// </summary>
        [Test]
        public void ACodeNoFileOfTheRunCarriesIsUnknownWhetherInTheGroup()
        {
            ClashTestPlan plan = Plan(Test("T", FfSprinklers, ArFloors));
            TestCoverage row = Only(plan, RunAsTheRunnerDoes(plan, Counts(FfSprinklers, 0, ArFloors, 2)));

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.NoModelOfTheRunCarriesTheCode));
            Assert.That(CoverageWords.For(row.Reason), Does.Contain("UNKNOWN"));
            Assert.That(row.Detail, Does.Contain("FF"));
        }

        [Test]
        public void ASetNameWithNoCodeSaysItCarriesNone()
        {
            ClashTestPlan plan = Plan(Test("T", Security, ArFloors));
            TestCoverage row = Only(plan, RunAsTheRunnerDoes(plan, Counts(Security, 0, ArFloors, 2)));

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SetNameCarriesNoCode));
            Assert.That(CoverageWords.For(row.Reason), Does.Contain("UNKNOWN"));
            Assert.That(row.Detail, Does.Contain(Security));
        }

        /// <summary>
        /// Both sides empty: the stronger reason, in the order not in the group, in the
        /// group, no file of the run, no code, and both sets named.
        /// </summary>
        [Test]
        public void BothSidesEmptyGiveTheStrongerReasonAndNameBothSets()
        {
            ClashTestPlan plan = Plan(Test("T", ArStairs, StFoundation), Test("U", FfSprinklers, ArStairs),
                Test("V", Security, FfSprinklers));
            IList<TestCoverage> rows = Coverage(plan,
                RunAsTheRunnerDoes(plan, Counts(ArStairs, 0, StFoundation, 0, FfSprinklers, 0, Security, 0)));

            Assert.That(rows[0].Reason, Is.EqualTo(CoverageReason.DisciplineNotInGroup));
            Assert.That(rows[0].Detail, Does.Contain(ArStairs));
            Assert.That(rows[0].Detail, Does.Contain(StFoundation));
            Assert.That(rows[1].Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(rows[2].Reason, Is.EqualTo(CoverageReason.NoModelOfTheRunCarriesTheCode));
        }

        // ---------- a side found nothing at the run time check ----------

        /// <summary>A count of minus one is a side nobody counted and never a side that found nothing.</summary>
        [Test]
        public void ACountOfMinusOneNeverReadsFoundNoItems()
        {
            ClashTestPlan plan = Plan(Test("T", ArStairs, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.RecordSides("T", -1, 2);
            outcome.AddSkipped("T", ClashSkipReason.EmptySide, "the left side finds nothing");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SideNotCounted));
            Assert.That(CoverageWords.For(row.Reason), Does.Contain("UNKNOWN"));
            Assert.That(CoverageWords.For(row.Reason), Does.Not.Contain("found no items"));
            Assert.That(row.LeftItems, Is.EqualTo(-1));
        }

        [Test]
        public void ASideThatFoundNothingBesideOneNobodyCountedGivesTheDefiniteReason()
        {
            ClashTestPlan plan = Plan(Test("T", StFoundation, ArStairs));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.RecordSides("T", -1, 0);
            outcome.AddSkipped("T", ClashSkipReason.EmptySide, "the right side finds nothing");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(row.Detail, Does.Contain(ArStairs));
        }

        /// <summary>
        /// A test already in the NWF that the run time check did not run reads already there
        /// with its side reason, off the counts the check read and not the creation plan's.
        /// </summary>
        [Test]
        public void AnAlreadyThereTestTheCheckSkippedKeepsPresenceAndItsSideReason()
        {
            ClashTestPlan plan = Plan(Test("T", ArStairs, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.KeepItemsByLocator(Counts(ArStairs, 5, ArFloors, 2));
            outcome.AddAlreadyPresent("T");
            outcome.RecordSides("T", 0, 2);
            outcome.AddSkipped("T", ClashSkipReason.EmptySide, "the left side finds nothing");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Presence, Is.EqualTo(TestPresence.AlreadyThere));
            Assert.That(row.Ran, Is.False);
            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(row.LeftItems, Is.EqualTo(0), "the run time read decided the skip, not the plan's count");
        }

        [Test]
        public void TheDisciplinesNotReadSayUnknownAndNeverGuess()
        {
            ClashTestPlan plan = Plan(Test("T", StFoundation, ArFloors));
            IList<TestCoverage> rows = CoverageRule.For(plan,
                RunAsTheRunnerDoes(plan, Counts(StFoundation, 0, ArFloors, 2)), null, null, new ViewpointSettings());

            Assert.That(rows[0].Reason, Is.EqualTo(CoverageReason.CodesNotRead));
            Assert.That(CoverageWords.For(rows[0].Reason), Does.Contain("UNKNOWN"));
        }

        // ---------- created and not run ----------

        [Test]
        public void ACreatedTestInAGroupSkippedByTheCoordinatesRuleSaysSo()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.AddSkipped("T", ClashSkipReason.NotOnTheSameCoordinates, "not run, coordinates");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.CoordinatesRule));
            Assert.That(row.Presence, Is.EqualTo(TestPresence.CreatedThisRun));
            Assert.That(row.Ran, Is.False);
        }

        /// <summary>
        /// F77 runs before the coordinates rule, so a test it kept out of a skipped group
        /// keeps its side reason. Read the other way round, every set gap of both wave
        /// buildings would read as a coordinates skip, which hides the gaps in the XML.
        /// </summary>
        [Test]
        public void ATestF77KeptOutOfACoordinatesSkippedGroupKeepsItsSideReason()
        {
            ClashTestPlan plan = Plan(Test("T", ArStairs, ArFloors), Test("U", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            Dictionary<string, int> counts = Counts(ArStairs, 0, ArFloors, 2, ArWalls, 2);
            CreationPlan creation = CreationPlan.For(plan.Buildable, counts);
            outcome.KeepItemsByLocator(counts);
            outcome.ClashSkipped = "clash skipped, models not on the same shared coordinates";

            foreach (SkippedClashTest skipped in creation.NotCreated)
            {
                outcome.AddSkipped(skipped);
            }

            outcome.AddCreated("U");
            outcome.AddSkipped("U", ClashSkipReason.NotOnTheSameCoordinates, "not run, coordinates");

            IList<TestCoverage> rows = Coverage(plan, outcome);

            Assert.That(rows[0].Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(rows[1].Reason, Is.EqualTo(CoverageReason.CoordinatesRule));
        }

        [Test]
        public void AOneDisciplineGroupNamesItsCode()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.AddSkipped("T", ClashSkipReason.SingleDiscipline, "one discipline");

            IList<TestCoverage> rows = CoverageRule.For(plan, outcome, new[] { "AR" }, RunCodes, new ViewpointSettings());

            Assert.That(rows[0].Reason, Is.EqualTo(CoverageReason.OneDiscipline));
            Assert.That(rows[0].Detail, Does.Contain("AR"));
        }

        [Test]
        public void AFailedTestCarriesWhatItSaid()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.AddSkipped("T", ClashSkipReason.Failed, "the test is no longer where it was put");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.Failed));
            Assert.That(row.Detail, Does.Contain("the test is no longer where it was put"));
            Assert.That(row.Presence, Is.EqualTo(TestPresence.CreatedThisRun));
        }

        // ---------- no record ----------

        [Test]
        public void ATestTheRunNeverReachedBecauseItStoppedSaysSo()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.StopTheWholeRun("50 tests failed the same way");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.NotReached));
            Assert.That(row.Detail, Does.Contain("50 tests failed the same way"));
        }

        [Test]
        public void ATestWithNoRecordAndNoStopIsUnknown()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors));
            TestCoverage row = Only(plan, new ClashRunOutcome());

            Assert.That(row.Reason, Is.EqualTo(CoverageReason.Unknown));
            Assert.That(row.Presence, Is.EqualTo(TestPresence.Unknown));
        }

        [Test]
        public void NoClashStepAtAllLeavesThePlansOwnReasonsAndCallsTheRestUnknown()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors), Test("U", ArWalls, ArFloors, "bogus", true));
            IList<TestCoverage> rows = Coverage(plan, null);

            Assert.That(rows[0].Reason, Is.EqualTo(CoverageReason.Unknown));
            Assert.That(rows[1].Reason, Is.EqualTo(CoverageReason.NotCreated));
        }

        /// <summary>
        /// The runner keys its record on the name, so two tests of one name cannot be told
        /// apart in it, and a guess would hand one test the other's result.
        /// </summary>
        [Test]
        public void TwoTestsOfOneNameAreUnknownRatherThanGuessed()
        {
            ClashTestPlan plan = Plan(Test("T", ArWalls, ArFloors), Test("T", ArStairs, ArFloors));
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("T");
            outcome.AddRan("T", 2, 2, Clashes(4), 0.1);
            IList<TestCoverage> rows = Coverage(plan, outcome);

            Assert.That(rows.Count, Is.EqualTo(2));

            foreach (TestCoverage row in rows)
            {
                Assert.That(row.Reason, Is.EqualTo(CoverageReason.Unknown));
                Assert.That(row.Detail, Does.Contain("2 tests"));
            }
        }

        // ---------- the whole file ----------

        [Test]
        public void OneRowPerTestInTheFileInFileOrder()
        {
            ClashTestPlan plan = Plan(Test("A", ArWalls, ArFloors), Test("B", ArWalls, ArFloors, "bogus", true),
                Test("C", ArStairs, ArFloors));
            IList<TestCoverage> rows = Coverage(plan, RunAsTheRunnerDoes(plan,
                Counts(ArWalls, 2, ArFloors, 2, ArStairs, 0)));

            Assert.That(rows.Count, Is.EqualTo(plan.TestsInFile));
            Assert.That(rows[0].Name, Is.EqualTo("A"));
            Assert.That(rows[1].Name, Is.EqualTo("B"));
            Assert.That(rows[2].Name, Is.EqualTo("C"));
            Assert.That(rows[0].Position, Is.EqualTo(1));
            Assert.That(rows[2].Position, Is.EqualTo(3));
            Assert.That(rows[2].LeftSet, Is.EqualTo(ArStairs));
        }

        /// <summary>
        /// The saved tests of the document with no XML make no creation plan, so the side
        /// counts are the ones the run time check read and nothing else.
        /// </summary>
        [Test]
        public void TheSavedTestsWithNoXmlReadTheirSidesOffTheRunTimeCheck()
        {
            SavedClashTest saved = new SavedClashTest("T", (int)ClashTestKind.HardConservative, 0.08, true,
                false, 1, ArStairs, false, 1, ArFloors, new[] { 0 });
            ClashTestPlan plan = ClashTestPlan.FromDocument(new[] { saved }, "ft");
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddAlreadyPresent("T");
            outcome.RecordSides("T", 0, 2);
            outcome.AddSkipped("T", ClashSkipReason.EmptySide, "the left side finds nothing");

            TestCoverage row = Only(plan, outcome);

            Assert.That(row.Presence, Is.EqualTo(TestPresence.AlreadyThere));
            Assert.That(row.Reason, Is.EqualTo(CoverageReason.SideFoundNothing));
            Assert.That(row.Detail, Does.Contain(ArStairs));
        }

        /// <summary>
        /// THE ENUMERATION. Every reason the runner can record maps to a coverage reason,
        /// so a reason added to ClashSkipReason, F132's Mirror among them, fails here until
        /// it is mapped, rather than reading UNKNOWN on every test it skips.
        /// </summary>
        [Test]
        public void EverySkipReasonTheRunnerRecordsMapsToACoverageReason()
        {
            foreach (ClashSkipReason kind in Enum.GetValues(typeof(ClashSkipReason)))
            {
                Assert.That(CoverageRule.ReasonFor(kind), Is.Not.EqualTo(CoverageReason.Unknown),
                    kind + " has no coverage reason, so every test it skips reads UNKNOWN");
            }
        }

        [Test]
        public void EveryCoverageReasonHasWordsOfItsOwn()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (CoverageReason reason in Enum.GetValues(typeof(CoverageReason)))
            {
                string words = CoverageWords.For(reason);

                Assert.That(words, Is.Not.Null.And.Not.Empty, reason + " has no words");
                Assert.That(seen.Add(words), Is.True, reason + " reads the same as another reason");
                Assert.That(words, Does.Not.Contain(reason.ToString()), reason + " shows its code name");
            }
        }

        [Test]
        public void EveryPresenceHasWordsOfItsOwn()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (TestPresence presence in Enum.GetValues(typeof(TestPresence)))
            {
                Assert.That(seen.Add(CoverageWords.For(presence)), Is.True, presence + " reads the same as another");
            }

            Assert.That(CoverageWords.For(TestPresence.Unknown), Does.Contain("UNKNOWN"));
        }

        /// <summary>
        /// The design's split of group 100000, turn5\f127-design.md section 4, on the exchange
        /// file's 1830 tests with the counts of set 03's C06 run, log lines 215 to 286: the
        /// nine sets that found items there, every other set at zero. 36 tests both find, and
        /// of the 1794 with no results 969 have a side whose code no file of the group
        /// carries, 561 a side of the group's own codes, 255 a code no file of the run
        /// carries, and 9 the one set name with no code. Sample data only.
        /// </summary>
        [Test]
        public void TheExchangeFileOnC06Group100000SplitsAsTheDesignCounted()
        {
            ClashTestPlan plan = ClashTestPlan.From(new ExchangeReader().ReadFile(Samples.CorrectedMatrix()), "ft");
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (string locator in plan.DistinctLocators())
            {
                counts[locator] = 0;
            }

            Dictionary<string, int> found = Counts(
                Root + "/Architecture/BLD-AR-Floors", 2,
                Root + "/Architecture/BLD-AR-Ceilings", 1,
                Root + "/Architecture/BLD-AR-Walls", 2,
                Root + "/Architecture/BLD-AR-Curtain Panels", 14,
                Root + "/Architecture/BLD-AR-Curtain Mullions", 29,
                Root + "/Architecture/BLD-AR-Doors", 1,
                Root + "/Architecture/BLD-AR-Furniture", 3,
                Root + "/Architecture/BLD-AR-Site", 8,
                Root + "/Mechanical/Mechanical-Drainage/BLD-DR-Pipes & Pipe Fittings", 4);

            foreach (KeyValuePair<string, int> set in found)
            {
                Assert.That(counts.ContainsKey(set.Key), Is.True, set.Key + " is not a set the file's tests name");
                counts[set.Key] = set.Value;
            }

            IList<TestCoverage> rows = Coverage(plan, RunAsTheRunnerDoes(plan, counts));
            Dictionary<CoverageReason, int> byReason = new Dictionary<CoverageReason, int>();

            foreach (TestCoverage row in rows)
            {
                int already;
                byReason.TryGetValue(row.Reason, out already);
                byReason[row.Reason] = already + 1;
            }

            Assert.That(rows.Count, Is.EqualTo(1830));
            Assert.That(byReason[CoverageReason.RanAndFoundNone], Is.EqualTo(36));
            Assert.That(byReason[CoverageReason.DisciplineNotInGroup], Is.EqualTo(969));
            Assert.That(byReason[CoverageReason.SideFoundNothing], Is.EqualTo(561));
            Assert.That(byReason[CoverageReason.NoModelOfTheRunCarriesTheCode], Is.EqualTo(255));
            Assert.That(byReason[CoverageReason.SetNameCarriesNoCode], Is.EqualTo(9));
            Assert.That(byReason.Count, Is.EqualTo(5), "a reason the split does not hold");
        }
    }
}
