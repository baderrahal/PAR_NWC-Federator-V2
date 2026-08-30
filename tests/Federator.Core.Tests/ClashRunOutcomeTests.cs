using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The rules that decide what runs and what the numbers mean. Most groups hold two or
    /// three disciplines, so most pairs have a side finding nothing in that model, which
    /// makes the difference between skipped and passed the number that matters most here.
    /// </summary>
    [TestFixture]
    public class ClashRunOutcomeTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string LeftPath = Root + "/Mechanical/BLD-ME-Ducts";
        private const string RightPath = Root + "/Architecture/BLD-AR-Floors";

        private static PlannedClashTest ATest(string name)
        {
            string xml = "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"" + name + "\" test_type=\"hard_conservative\""
                + " tolerance=\"0.2460629921\" merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + LeftPath + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + RightPath + "</locator></clashselection></right>"
                + "</clashtest></batchtest></exchange>";

            return ClashTestPlan.From(new ExchangeReader().ReadText(xml), "m").Buildable[0];
        }

        // ---------- the empty side rule ----------

        [Test]
        public void BothSidesFindingItemsRuns()
        {
            string reason;

            Assert.That(ClashSideCheck.CanRun(ATest("T"), 120, 44, out reason), Is.True);
            Assert.That(reason, Is.Null);
        }

        // This is the ordinary case. An architecture only model has no ducts in it.
        [Test]
        public void ASideAtZeroDoesNotRunAndTheEmptySideIsNamed()
        {
            string reason;

            Assert.That(ClashSideCheck.CanRun(ATest("T"), 0, 44, out reason), Is.False,
                "a test with a side at zero was run, and it can only report zero");
            Assert.That(reason, Does.Contain(LeftPath), "the empty side has to be named");
            Assert.That(reason, Does.Contain("44"), "the side that did find something is worth knowing");
        }

        [Test]
        public void TheOtherSideAtZeroIsNamedToo()
        {
            string reason;

            Assert.That(ClashSideCheck.CanRun(ATest("T"), 120, 0, out reason), Is.False);
            Assert.That(reason, Does.Contain(RightPath));
        }

        [Test]
        public void BothSidesAtZeroNamesBoth()
        {
            string reason;

            Assert.That(ClashSideCheck.CanRun(ATest("T"), 0, 0, out reason), Is.False);
            Assert.That(reason, Does.Contain(LeftPath));
            Assert.That(reason, Does.Contain(RightPath));
        }

        [Test]
        public void OneItemIsEnoughToRun()
        {
            string reason;
            Assert.That(ClashSideCheck.CanRun(ATest("T"), 1, 1, out reason), Is.True);
        }

        [Test]
        public void NoTestAtAllIsRefusedRatherThanJudged()
        {
            string reason;

            Assert.Throws<ArgumentNullException>(
                delegate { ClashSideCheck.CanRun(null, 1, 1, out reason); });
        }

        // ---------- skipped and passed are never the same number ----------

        [Test]
        public void ASkippedTestIsNotCountedAsPassed()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddSkipped("T1", ClashSkipReason.EmptySide, "the left side finds nothing in this model");

            Assert.That(outcome.SkippedCount, Is.EqualTo(1));
            Assert.That(outcome.PassedCount, Is.EqualTo(0),
                "a test that never ran was counted as having passed");
            Assert.That(outcome.RanCount, Is.EqualTo(0));
        }

        [Test]
        public void APassedTestIsNotCountedAsSkipped()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddRan("T1", 120, 44, new ClashTally(), 1.5);

            Assert.That(outcome.RanCount, Is.EqualTo(1));
            Assert.That(outcome.PassedCount, Is.EqualTo(1), "ran and found nothing is passed");
            Assert.That(outcome.SkippedCount, Is.EqualTo(0),
                "a test that ran and found nothing was counted as skipped");
        }

        // A test that never ran and a test that found nothing both show zero clashes. They
        // are different facts and the block has to keep them apart.
        [Test]
        public void SkippedAndPassedStayApartInEveryPlaceTheyAppear()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = 4;
            outcome.AddRan("passed one", 10, 10, new ClashTally(), 0.4);
            outcome.AddSkipped("skipped one", ClashSkipReason.EmptySide, "the right side finds nothing");
            outcome.AddSkipped("skipped two", ClashSkipReason.EmptySide, "the left side finds nothing");

            Assert.That(outcome.PassedCount, Is.EqualTo(1));
            Assert.That(outcome.SkippedCount, Is.EqualTo(2));

            string block = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(block, Does.Contain("tests skipped     : 2"));
            Assert.That(block, Does.Contain("passed        : 1"));
            Assert.That(block, Does.Contain("tests run         : 1"));
            Assert.That(block, Does.Contain("not run and not passed"));

            Assert.That(outcome.Summary(), Does.Contain("1 passed"));
            Assert.That(outcome.Summary(), Does.Contain("2 skipped"));
        }

        [Test]
        public void ASkippedTestNeverAppearsAsARunLine()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddSkipped("T1", ClashSkipReason.EmptySide, "nothing on the left");

            string block = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            // Skips are counted by reason with a few named examples rather than one line
            // each, but a skipped test is still named and must never reach a passed line.
            Assert.That(block, Does.Contain("SKIPPED 1 test, "));
            Assert.That(block, Does.Contain("T1  nothing on the left"));
            Assert.That(block, Does.Not.Contain("passed  T1"),
                "a skipped test was written out on a passed line");
        }

        // ---------- the counts by status ----------

        [Test]
        public void ClashesAreCountedByStatusAndTheTotalAgrees()
        {
            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, 12);
            tally.Add(ClashStatus.Active, 3);
            tally.Add(ClashStatus.Resolved);

            Assert.That(tally.Of(ClashStatus.New), Is.EqualTo(12));
            Assert.That(tally.Of(ClashStatus.Active), Is.EqualTo(3));
            Assert.That(tally.Of(ClashStatus.Resolved), Is.EqualTo(1));
            Assert.That(tally.Of(ClashStatus.Approved), Is.EqualTo(0));
            Assert.That(tally.Total, Is.EqualTo(16));
            Assert.That(tally.Describe(), Does.Contain("New 12"));
            Assert.That(tally.Describe(), Does.Not.Contain("Approved"),
                "a status with nothing in it should not clutter the line");
        }

        [Test]
        public void AnEmptyTallyReadsAsNoneRatherThanBlank()
        {
            Assert.That(new ClashTally().Describe(), Is.EqualTo("none"));
            Assert.That(new ClashTally().Total, Is.EqualTo(0));
        }

        // These are the numbers of Autodesk.Navisworks.Api.Clash.ClashResultStatus, read
        // off the installed DLL. The runner casts straight through, so they have to match.
        [Test]
        public void TheStatusNumbersAreTheOnesTheApiUses()
        {
            Assert.That((int)ClashStatus.New, Is.EqualTo(0));
            Assert.That((int)ClashStatus.Active, Is.EqualTo(1));
            Assert.That((int)ClashStatus.Reviewed, Is.EqualTo(2));
            Assert.That((int)ClashStatus.Approved, Is.EqualTo(3));
            Assert.That((int)ClashStatus.Resolved, Is.EqualTo(4));
            Assert.That(ClashTally.AllStatuses.Length, Is.EqualTo(5));
        }

        [Test]
        public void TheTotalsAcrossTestsAreTheSumOfTheTests()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();

            ClashTally first = new ClashTally();
            first.Add(ClashStatus.New, 5);
            first.Add(ClashStatus.Active, 2);

            ClashTally second = new ClashTally();
            second.Add(ClashStatus.New, 3);
            second.Add(ClashStatus.Resolved, 1);

            outcome.AddRan("a", 10, 10, first, 1.0);
            outcome.AddRan("b", 10, 10, second, 2.0);

            Assert.That(outcome.TotalClashes, Is.EqualTo(11));
            Assert.That(outcome.Totals.Of(ClashStatus.New), Is.EqualTo(8));
            Assert.That(outcome.Totals.Of(ClashStatus.Active), Is.EqualTo(2));
            Assert.That(outcome.Totals.Of(ClashStatus.Resolved), Is.EqualTo(1));
            Assert.That(outcome.WithClashesCount, Is.EqualTo(2));
            Assert.That(outcome.PassedCount, Is.EqualTo(0));
        }

        // ---------- the totals block, which is JOB 6 ----------

        [Test]
        public void TheBlockCarriesEveryNumberAskedFor()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = 1830;
            outcome.OpenDocument = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf";
            outcome.Seconds = 412.5;

            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, 7);

            outcome.AddCreated("with clashes");
            outcome.AddCreated("passed");
            outcome.AddRan("with clashes", 100, 50, tally, 3.2);
            outcome.AddRan("passed", 100, 50, new ClashTally(), 1.1);
            outcome.AddSkipped("empty", ClashSkipReason.EmptySide, "the left side finds nothing");
            outcome.AddSkipped("missing set", ClashSkipReason.LocatorNotResolved, "not in the document");

            string block = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(block, Does.Contain("tests in the file : 1830"));
            Assert.That(block, Does.Contain("tests created     : 2"));
            Assert.That(block, Does.Contain("tests skipped     : 2"));
            Assert.That(block, Does.Contain("tests run         : 2"));
            Assert.That(block, Does.Contain("clashes found     : 7"));
            Assert.That(block, Does.Contain("New"));
            Assert.That(block, Does.Contain("Resolved"));
            Assert.That(block, Does.Contain("ran against       : 1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf"));

            // The one number nobody has, on its own line and named plainly.
            Assert.That(block, Does.Contain("clash step took   : 412.5 seconds"));

            // Skipped, counted by reason.
            Assert.That(block, Does.Contain(ClashTestPlan.Describe(ClashSkipReason.EmptySide)));
            Assert.That(block, Does.Contain(ClashTestPlan.Describe(ClashSkipReason.LocatorNotResolved)));
        }

        [Test]
        public void TheSkipReasonCountsAddUpToTheSkippedCount()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddSkipped("a", ClashSkipReason.EmptySide, "x");
            outcome.AddSkipped("b", ClashSkipReason.EmptySide, "x");
            outcome.AddSkipped("c", ClashSkipReason.UnknownTestType, "x");

            int total = 0;

            foreach (KeyValuePair<ClashSkipReason, int> pair in outcome.SkipReasonCounts())
            {
                total += pair.Value;
            }

            Assert.That(total, Is.EqualTo(outcome.SkippedCount));
            Assert.That(outcome.SkipReasonCounts()[ClashSkipReason.EmptySide], Is.EqualTo(2));
        }

        [Test]
        public void ATestAlreadyThereIsNotCountedAsCreated()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("new one");
            outcome.AddAlreadyPresent("last week's one");

            Assert.That(outcome.CreatedCount, Is.EqualTo(1),
                "a test that was already there was counted as created by this run");
            Assert.That(outcome.AlreadyPresentCount, Is.EqualTo(1));

            string block = string.Join("\n", new List<string>(outcome.Lines()).ToArray());
            Assert.That(block, Does.Contain("already there     : 1"));
            Assert.That(block, Does.Contain("left alone with their results"));
        }

        [Test]
        public void AlreadyThereIsLeftOutOfTheBlockWhenThereIsNone()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.AddCreated("one");

            Assert.That(string.Join("\n", new List<string>(outcome.Lines()).ToArray()),
                Does.Not.Contain("already there"));
        }

        [Test]
        public void EveryLineCarriesTheItemsEachSideAndTheSeconds()
        {
            ClashTally tally = new ClashTally();
            tally.Add(ClashStatus.New, 2);

            ClashRunOutcome outcome = new ClashRunOutcome();
            ClashTestResult result = outcome.AddRan("BLD-ME v BLD-AR", 613, 42, tally, 2.75);

            Assert.That(result.Line(), Does.Contain("BLD-ME v BLD-AR"));
            Assert.That(result.Line(), Does.Contain("613"));
            Assert.That(result.Line(), Does.Contain("42"));
            Assert.That(result.Line(), Does.Contain("New 2"));
            Assert.That(result.Line(), Does.Contain("2.8s").Or.Contain("2.7s"));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public void ARunWithNothingInItStillReadsAsARunAndNotAsAnError()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();

            Assert.That(outcome.RanCount, Is.EqualTo(0));
            Assert.That(outcome.SkippedCount, Is.EqualTo(0));
            Assert.That(outcome.PassedCount, Is.EqualTo(0));
            Assert.That(outcome.TotalClashes, Is.EqualTo(0));
            Assert.That(string.Join("\n", new List<string>(outcome.Lines()).ToArray()),
                Does.Contain("ran against       : UNKNOWN"));
        }

        [Test]
        public void NoSkippedTestAtAllIsRefusedRatherThanStored()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { new ClashRunOutcome().AddSkipped(null); });
        }
    }
}
