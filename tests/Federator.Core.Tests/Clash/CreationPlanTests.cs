using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F77. TESTS CREATE took 631 seconds of a 1424 second run, 44 per cent of it, and
    /// most of what it created was thrown away moments later by the empty side check. Of
    /// 1830 tests created, 1619 could not clash with anything because one of their sides
    /// finds nothing in that model, and the side counts were already known before a single
    /// test was created.
    /// </summary>
    [TestFixture]
    public class CreationPlanTests
    {
        private const string Ducts = "lcop_selection_set_tree/Mechanical/BLD-ME-Ducts";
        private const string Walls = "lcop_selection_set_tree/Architecture/BLD-AR-Walls";
        private const string Trays = "lcop_selection_set_tree/Electrical/BLD-EL-Cable Trays";

        /// <summary>One test between two named sets, read the way the engine reads one.</summary>
        private static PlannedClashTest ATest(string name, string left, string right)
        {
            string xml = "<exchange units=\"ft\"><batchtest name=\"b\">"
                + "<clashtest name=\"" + name + "\" test_type=\"hard_conservative\""
                + " tolerance=\"0.2460629921\" merge_composites=\"1\">"
                + "<left><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + left + "</locator></clashselection></left>"
                + "<right><clashselection selfintersect=\"0\" primtypes=\"1\"><locator>"
                + right + "</locator></clashselection></right>"
                + "</clashtest></batchtest></exchange>";

            return ClashTestPlan.From(new ExchangeReader().ReadText(xml), "m").Buildable[0];
        }

        private static IDictionary<string, int> Counts(params object[] pairs)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                counts[(string)pairs[i]] = (int)pairs[i + 1];
            }

            return counts;
        }

        [Test]
        public void ATestWithItemsOnBothSidesIsCreated()
        {
            CreationPlan plan = CreationPlan.For(
                new[] { ATest("Ducts vs Walls", Ducts, Walls) },
                Counts(Ducts, 120, Walls, 44));

            Assert.That(plan.CreateCount, Is.EqualTo(1));
            Assert.That(plan.NotCreatedCount, Is.EqualTo(0));
        }

        /// <summary>The break. This is the 1619 that cost 631 seconds.</summary>
        [Test]
        public void ATestWhoseSideFindsNothingIsNotCreatedAtAll()
        {
            CreationPlan plan = CreationPlan.For(
                new[] { ATest("Ducts vs Trays", Ducts, Trays) },
                Counts(Ducts, 120, Trays, 0));

            Assert.That(plan.CreateCount, Is.EqualTo(0));
            Assert.That(plan.NotCreatedCount, Is.EqualTo(1));
            Assert.That(plan.NotCreated[0].Kind, Is.EqualTo(ClashSkipReason.EmptySide));
            Assert.That(plan.NotCreated[0].Name, Is.EqualTo("Ducts vs Trays"));
            Assert.That(plan.NotCreated[0].Reason, Does.Contain(Trays));
            Assert.That(plan.NotCreated[0].Reason, Does.Contain("120"),
                "the side that did find something is worth knowing");
        }

        [Test]
        public void EitherSideAtZeroIsEnoughAndBothAtZeroSaysBoth()
        {
            CreationPlan left = CreationPlan.For(
                new[] { ATest("T", Ducts, Walls) }, Counts(Ducts, 0, Walls, 44));
            CreationPlan both = CreationPlan.For(
                new[] { ATest("T", Ducts, Walls) }, Counts(Ducts, 0, Walls, 0));

            Assert.That(left.NotCreatedCount, Is.EqualTo(1));
            Assert.That(both.NotCreatedCount, Is.EqualTo(1));
            Assert.That(both.NotCreated[0].Reason, Does.Contain("both sides"));
        }

        /// <summary>
        /// A count nobody took is not a count of zero. The safe mistake is to create the
        /// test and let the run-time check answer. The unsafe one is to leave a real test
        /// out of the NWF because a number was missing.
        /// </summary>
        [Test]
        public void ALocatorNobodyCountedIsCreatedAndLeftToTheSecondCheck()
        {
            CreationPlan plan = CreationPlan.For(
                new[] { ATest("T", Ducts, Walls) }, Counts(Ducts, 120));

            Assert.That(plan.CreateCount, Is.EqualTo(1));
            Assert.That(plan.NotCreatedCount, Is.EqualTo(0));
        }

        [Test]
        public void NoCountsAtAllCreatesEverything()
        {
            PlannedClashTest[] tests =
            {
                ATest("A", Ducts, Walls),
                ATest("B", Ducts, Trays)
            };

            Assert.That(CreationPlan.For(tests, null).CreateCount, Is.EqualTo(2));
            Assert.That(CreationPlan.For(tests, Counts()).CreateCount, Is.EqualTo(2));
        }

        /// <summary>
        /// Locators are compared Ordinal and never trimmed, because two set names in the
        /// reference file end in a space. A trimmed comparison would count a different set.
        /// </summary>
        [Test]
        public void ALocatorIsMatchedExactlyAndNeverTrimmed()
        {
            string spaced = Ducts + " ";

            CreationPlan plan = CreationPlan.For(
                new[] { ATest("T", spaced, Walls) },
                Counts(Ducts, 0, Walls, 44));

            Assert.That(plan.CreateCount, Is.EqualTo(1),
                "the zero belongs to a different set, so nothing is known about this one");
        }

        [Test]
        public void ThePlanSurvivesNothingAndANullInTheList()
        {
            Assert.That(CreationPlan.For(null, Counts()).CreateCount, Is.EqualTo(0));
            Assert.That(CreationPlan.For(new PlannedClashTest[] { null }, Counts()).CreateCount,
                Is.EqualTo(0));
        }

        // ---------- the counted line ----------

        [Test]
        public void TheCountedLineIsOneLineAndNotSixteenHundred()
        {
            List<PlannedClashTest> tests = new List<PlannedClashTest>();
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            counts[Ducts] = 120;
            counts[Walls] = 44;
            counts[Trays] = 0;

            for (int i = 0; i < 211; i++)
            {
                tests.Add(ATest("good " + i, Ducts, Walls));
            }

            for (int i = 0; i < 1619; i++)
            {
                tests.Add(ATest("empty " + i, Ducts, Trays));
            }

            CreationPlan plan = CreationPlan.For(tests, counts);

            Assert.That(plan.CreateCount, Is.EqualTo(211));
            Assert.That(plan.NotCreatedCount, Is.EqualTo(1619));
            Assert.That(plan.CountedLine(1830),
                Is.EqualTo("CLASH 1830 in the file, 211 created, 1619 not created, a side finds nothing"));
        }

        // ---------- the blocks of the workbook, F153 ----------

        // Bader's decision of 2026-10-13, which retires F77's rule that the workbook carries a block
        // for every test in the file: a test with no clash writes nothing there. His run of that day
        // read 130 tests with clashes and 1700 one row tests, which the line now reads as 130 blocks.
        [Test]
        public void ABlockForEveryTestWithAClashIsTheWholeWorkbook()
        {
            string line = CreationPlan.BlockCountLine(130, 130, 1830, 0);

            Assert.That(line, Is.EqualTo(
                "BLOCKS   130 in the workbook, one for every test with a clash. The other 1700 of the 1830 tests of "
                    + "the report found none and are on the page and in the .tsv alone"));
            Assert.That(line, Does.Not.Contain("MUST"));
        }

        // The break: the workbook of before F153, a block or a row for every test of the file, is a fault
        // said in capitals, and so is a block missing.
        [Test]
        public void ABlockForATestThatFoundNoneOrABlockMissingIsSaidInCapitals()
        {
            Assert.That(CreationPlan.BlockCountLine(1830, 130, 1830, 0), Is.EqualTo(
                "BLOCKS   1830 in the workbook against 130 tests with a clash of the 1830 tests of the report. "
                    + "THE WORKBOOK MUST CARRY A BLOCK FOR EVERY TEST WITH A CLASH AND NONE FOR A TEST THAT FOUND NONE, F153"));
            Assert.That(CreationPlan.BlockCountLine(129, 130, 1830, 0), Does.StartWith(
                "BLOCKS   129 in the workbook against 130 tests with a clash"));
            Assert.That(CreationPlan.BlockCountLine(129, 130, 1830, 0), Does.Contain("MUST"));
        }

        // F132. A mirror merged into its kept test is in that test's block, and the line says how many.
        [Test]
        public void AMirrorMergedIntoItsKeptTestIsSaidAndIsNotABlockMissing()
        {
            string line = CreationPlan.BlockCountLine(130, 130, 1771, 59);

            Assert.That(line, Does.EndWith(", and 59 mirrors are merged into their kept tests' blocks"));
            Assert.That(line, Does.Not.Contain("MUST"));
            Assert.That(CreationPlan.BlockCountLine(1, 1, 1, 1), Does.EndWith(", and 1 mirror is merged into its kept test's block"));
        }

        [Test]
        public void CountsThatCannotBeAreRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreationPlan.BlockCountLine(1, -1, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreationPlan.BlockCountLine(1, 2, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreationPlan.BlockCountLine(1, 1, 1, -1));
        }

        /// <summary>
        /// The old check is the second line of defence and is still the same rule, not a
        /// second copy of it. A test created because its counts were UNKNOWN here is still
        /// caught there and still counts as skipped rather than passed.
        /// </summary>
        [Test]
        public void TheRunTimeCheckStillRefusesTheSameTest()
        {
            PlannedClashTest test = ATest("T", Ducts, Trays);
            string why;

            Assert.That(ClashSideCheck.CanRun(test, 120, 0, out why), Is.False);
            Assert.That(CreationPlan.For(new[] { test }, Counts(Ducts, 120, Trays, 0)).NotCreated[0].Reason,
                Is.EqualTo(why), "one rule, read twice, never written twice");
        }
    }
}
