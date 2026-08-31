using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Grouping;
using Federator.Core.Naming;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A group holding one NWC cannot clash with anything, whatever the test list says. In
    /// the last real folder that was 1B06BS and 1C06PK, and both ran the full 1830 tests
    /// for nothing.
    ///
    /// Every test is still created, so the NWF is complete and matches the other groups and
    /// a later run against a fuller model finds them already there. None of them is run,
    /// and the reason is recorded as the group holding one model rather than as a side
    /// finding nothing. Those are different facts about different problems and they count
    /// separately.
    /// </summary>
    [TestFixture]
    public class SingleModelGroupTests
    {
        // ---------- the group knows ----------

        [Test]
        public void AGroupWithOneFileKnowsItCannotClash()
        {
            BuildingGroupingResult result = BuildingGrouping.GroupNames(
                new[]
                {
                    "1104-PAR-1B06BS-ZZZ-EL-MOD-000001.nwc",
                    "1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc",
                    "1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc"
                },
                new ContainerNameSettings());

            Assert.That(result.Find("1B06BS").IsSingleModel, Is.True);
            Assert.That(result.Find("1C07BC").IsSingleModel, Is.False);
        }

        // ---------- the reason is its own ----------

        // The one the brief asks for by name.
        [Test]
        public void OneModelAndASideFindingNothingAreDifferentReasons()
        {
            Assert.That(ClashSkipReason.SingleModel, Is.Not.EqualTo(ClashSkipReason.EmptySide));

            Assert.That(ClashTestPlan.Describe(ClashSkipReason.SingleModel),
                Is.Not.EqualTo(ClashTestPlan.Describe(ClashSkipReason.EmptySide)));
            Assert.That(ClashTestPlan.Describe(ClashSkipReason.SingleModel),
                Does.Contain("one model"));
            Assert.That(ClashTestPlan.Describe(ClashSkipReason.EmptySide),
                Does.Contain("finds nothing"));
        }

        [Test]
        public void TheTwoReasonsAreCountedSeparately()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();

            for (int i = 0; i < 1830; i++)
            {
                outcome.AddSkipped(
                    "T" + i,
                    ClashSkipReason.SingleModel,
                    "the group holds one model, so there is nothing for this test to clash against");
            }

            outcome.AddSkipped("other", ClashSkipReason.EmptySide, "the left side finds nothing");

            IDictionary<ClashSkipReason, int> counts = outcome.SkipReasonCounts();

            Assert.That(counts[ClashSkipReason.SingleModel], Is.EqualTo(1830));
            Assert.That(counts[ClashSkipReason.EmptySide], Is.EqualTo(1));
            Assert.That(outcome.SkippedCount, Is.EqualTo(1831));
        }

        [Test]
        public void TheBlockNamesTheOneModelReasonOnItsOwnLine()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();
            outcome.TestsInFile = 1830;

            for (int i = 0; i < 1830; i++)
            {
                outcome.AddSkipped("T" + i, ClashSkipReason.SingleModel, "one model");
            }

            string block = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(block, Does.Contain("SKIPPED 1830 tests, "
                + ClashTestPlan.Describe(ClashSkipReason.SingleModel)));
            Assert.That(block, Does.Contain("tests skipped     : 1830, not run and not passed"));
            Assert.That(block, Does.Not.Contain(ClashTestPlan.Describe(ClashSkipReason.EmptySide)),
                "a one model group was reported as a side finding nothing");
        }

        // A single model group is not a passed group. Nothing ran.
        [Test]
        public void NoneOfThemCountsAsPassed()
        {
            ClashRunOutcome outcome = new ClashRunOutcome();

            for (int i = 0; i < 100; i++)
            {
                outcome.AddCreated("T" + i);
                outcome.AddSkipped("T" + i, ClashSkipReason.SingleModel, "one model");
            }

            Assert.That(outcome.CreatedCount, Is.EqualTo(100), "the tests are still created");
            Assert.That(outcome.RanCount, Is.EqualTo(0));
            Assert.That(outcome.PassedCount, Is.EqualTo(0),
                "a test that never ran was counted as having passed");
            Assert.That(outcome.SkippedCount, Is.EqualTo(100));
            Assert.That(outcome.TotalClashes, Is.EqualTo(0));
        }

        // ---------- the workbook still gets written ----------

        [Test]
        public void TheWorkbookShowsEveryTestAsNotRunForThatReason()
        {
            ClashReport report = new ClashReport("1B06BS", "1104-PAR-1B06BS-ZZZ-BM-MOD-000001");

            for (int i = 0; i < 1830; i++)
            {
                TestReport test = report.AddTest("test " + i);
                test.State = TestState.Skipped;
                test.SkippedReason =
                    "the group holds one model, so there is nothing for this test to clash against";
            }

            Assert.That(report.Tests.Count, Is.EqualTo(1830),
                "the Summary still carries one row per test in the file");
            Assert.That(report.CountOf(TestState.Skipped), Is.EqualTo(1830));
            Assert.That(report.CountOf(TestState.Passed), Is.EqualTo(0),
                "not run is not the same as passed");
            Assert.That(report.RanCount, Is.EqualTo(0));

            foreach (TestReport test in report.Tests)
            {
                Assert.That(test.HasSheet, Is.False, "nothing ran, so nothing has a sheet");
                Assert.That(test.DescribeState(), Does.Contain("one model"));
                Assert.That(test.DescribeState(), Does.Contain("skipped, not run"));
            }
        }

        [Test]
        public void TheMatrixShowsSkippedRatherThanZeroForASingleModelGroup()
        {
            ClashReport report = new ClashReport("1B06BS", "1104-PAR-1B06BS-ZZZ-BM-MOD-000001");

            TestReport test = report.AddTest("AR v ME");
            test.LeftLocator = "lcop_selection_set_tree/Architecture/BLD-AR-Floors";
            test.RightLocator = "lcop_selection_set_tree/Mechanical/BLD-ME-Ducts";
            test.State = TestState.Skipped;
            test.SkippedReason = "the group holds one model";

            MatrixCell cell = ClashMatrix.From(report).At(test.LeftLocator, test.RightLocator);

            Assert.That(cell.Kind, Is.EqualTo(MatrixCellKind.Skipped));
            Assert.That(cell.Text(), Is.EqualTo("skipped"),
                "a group that could never clash read as coordinated");
        }

        // ---------- the reason reads as words a person would say ----------

        [Test]
        public void TheReasonExplainsItselfWithoutTheReaderKnowingTheTool()
        {
            string reason = ClashTestPlan.Describe(ClashSkipReason.SingleModel);

            Assert.That(reason, Does.Contain("one model"));
            Assert.That(reason, Does.Contain("clash"));
            Assert.That(reason, Is.Not.EqualTo("UNKNOWN"));
        }

        [Test]
        public void EverySkipReasonStillHasWordsForIt()
        {
            foreach (ClashSkipReason reason in Enum.GetValues(typeof(ClashSkipReason)))
            {
                Assert.That(ClashTestPlan.Describe(reason), Is.Not.EqualTo("UNKNOWN"),
                    reason + " has no words, so its count in the totals reads as nothing");
            }
        }
    }
}
