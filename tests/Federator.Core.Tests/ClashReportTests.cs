using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The model the workbook is built from. The numbers here are the ones off the real
    /// run of 1C07BC, which read 1830 tests, ran 666, skipped 1164 and found 213 clashes.
    /// They are sample data, not settings.
    /// </summary>
    [TestFixture]
    public class ClashReportTests
    {
        private const string Root = "lcop_selection_set_tree";

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = Root;
            return report;
        }

        private static TestReport Ran(ClashReport report, string name, string left, string right)
        {
            TestReport test = report.AddTest(name);
            test.LeftLocator = left;
            test.RightLocator = right;
            test.State = TestState.Passed;
            return test;
        }

        private static ClashRow Row(ClashStatus status, double distance, int raw)
        {
            ClashRow row = new ClashRow();
            row.Status = status;
            row.Distance = distance;
            row.RawClashes = raw;
            row.IsGroup = raw > 1;
            row.Name = "row";
            return row;
        }

        // ---------- grouped rows take the most severe distance ----------

        // A hard clash reports a negative distance, which is how far the two things
        // overlap, so the worst one is the most negative.
        [Test]
        public void AGroupTakesTheMostSevereDistanceOfWhatIsInIt()
        {
            double worst = ClashRow.MostSevere(new[] { -0.012, -0.145, -0.003, -0.08 }, 0.0);

            Assert.That(worst, Is.EqualTo(-0.145).Within(0.000001),
                "a group took something other than its worst clash");
        }

        // A clearance test reports the gap, which is positive, and the worst one is the
        // smallest gap. The same rule covers both.
        [Test]
        public void ThatSameRuleTakesTheSmallestGapOnAClearanceTest()
        {
            Assert.That(ClashRow.MostSevere(new[] { 0.09, 0.02, 0.15 }, 0.0),
                Is.EqualTo(0.02).Within(0.000001));
        }

        [Test]
        public void AGroupWithNothingInItFallsBackRatherThanInventingANumber()
        {
            Assert.That(ClashRow.MostSevere(new double[0], -0.5), Is.EqualTo(-0.5));
            Assert.That(ClashRow.MostSevere(null, -0.5), Is.EqualTo(-0.5));
        }

        [Test]
        public void OneClashInAGroupIsItsOwnWorst()
        {
            Assert.That(ClashRow.MostSevere(new[] { -0.07 }, 0.0), Is.EqualTo(-0.07).Within(0.000001));
        }

        // ---------- the raw count behind a group is kept ----------

        [Test]
        public void AGroupRowCarriesHowManyClashesAreBehindIt()
        {
            ClashReport report = Report();
            TestReport test = Ran(report, "AR v ME", Root + "/A/One", Root + "/B/Two");

            test.Add(Row(ClashStatus.New, -0.1, 14));
            test.Add(Row(ClashStatus.New, -0.2, 9));
            test.Add(Row(ClashStatus.Active, -0.3, 1));

            Assert.That(test.GroupCount, Is.EqualTo(3), "three rows on the sheet");
            Assert.That(test.RawClashes, Is.EqualTo(24), "the grouping hid the real count");
        }

        [Test]
        public void TheStatusCountsAreOverTheRawClashesRatherThanTheRows()
        {
            ClashReport report = Report();
            TestReport test = Ran(report, "AR v ME", Root + "/A/One", Root + "/B/Two");

            test.Add(Row(ClashStatus.New, -0.1, 14));
            test.Add(Row(ClashStatus.Active, -0.2, 9));

            Assert.That(test.Tally.Of(ClashStatus.New), Is.EqualTo(14));
            Assert.That(test.Tally.Of(ClashStatus.Active), Is.EqualTo(9));
            Assert.That(test.Tally.Total, Is.EqualTo(23));
            Assert.That(test.NewPlusActive, Is.EqualTo(23));
        }

        [Test]
        public void AnUngroupedClashIsItsOwnRowAndCountsOnce()
        {
            ClashReport report = Report();
            TestReport test = Ran(report, "AR v ME", Root + "/A/One", Root + "/B/Two");

            ClashRow row = Row(ClashStatus.New, -0.05, 1);
            row.IsGroup = false;
            test.Add(row);

            Assert.That(test.GroupCount, Is.EqualTo(1));
            Assert.That(test.RawClashes, Is.EqualTo(1));
        }

        // ---------- skipped, passed and found are three numbers ----------

        // The real run: 1164 skipped, 618 passed, 48 with clashes, and 1164+618+48 = 1830.
        [Test]
        public void SkippedPassedAndFoundAreThreeSeparateNumbersThatAddUp()
        {
            ClashReport report = Report();

            for (int i = 0; i < 1164; i++)
            {
                TestReport test = report.AddTest("skipped " + i);
                test.State = TestState.Skipped;
                test.SkippedReason = "a side finds nothing in this model";
            }

            for (int i = 0; i < 618; i++)
            {
                report.AddTest("passed " + i).State = TestState.Passed;
            }

            for (int i = 0; i < 48; i++)
            {
                TestReport test = report.AddTest("found " + i);
                test.State = TestState.FoundClashes;
                test.Add(Row(ClashStatus.New, -0.1, 1));
            }

            Assert.That(report.CountOf(TestState.Skipped), Is.EqualTo(1164));
            Assert.That(report.CountOf(TestState.Passed), Is.EqualTo(618));
            Assert.That(report.CountOf(TestState.FoundClashes), Is.EqualTo(48));
            Assert.That(report.RanCount, Is.EqualTo(666), "passed and found are the ones that ran");
            Assert.That(report.Tests.Count, Is.EqualTo(1830), "one row per test in the file");
        }

        // The one the brief asks for by name.
        [Test]
        public void TheSummaryHasOneRowPerTestInTheFileAndNotJustTheOnesThatRan()
        {
            ClashReport report = Report();

            for (int i = 0; i < 1830; i++)
            {
                TestReport test = report.AddTest("test " + i);
                test.State = i < 1164 ? TestState.Skipped : TestState.Passed;
            }

            Assert.That(report.Tests.Count, Is.EqualTo(1830));
            Assert.That(WorkbookWriter.SheetNamesFor(report).Count, Is.EqualTo(2),
                "no test found anything, so only Summary and Matrix exist");
        }

        [Test]
        public void OnlyATestThatFoundSomethingGetsASheet()
        {
            ClashReport report = Report();

            report.AddTest("skipped").State = TestState.Skipped;
            report.AddTest("passed").State = TestState.Passed;

            TestReport found = report.AddTest("found");
            found.State = TestState.FoundClashes;
            found.Add(Row(ClashStatus.New, -0.1, 3));

            Assert.That(report.Tests[0].HasSheet, Is.False, "a skipped test has no sheet");
            Assert.That(report.Tests[1].HasSheet, Is.False, "a passed test has no sheet");
            Assert.That(report.Tests[2].HasSheet, Is.True);
            Assert.That(report.Tests[2].SheetName(), Is.EqualTo("T0003"),
                "the sheet is numbered by the test, not by which ones have sheets");
        }

        [Test]
        public void SkippedAndPassedNeverReadTheSameInTheOutcomeColumn()
        {
            ClashReport report = Report();

            TestReport skipped = report.AddTest("skipped");
            skipped.State = TestState.Skipped;
            skipped.SkippedReason = "the left side finds nothing in this model";

            TestReport passed = report.AddTest("passed");
            passed.State = TestState.Passed;

            Assert.That(skipped.DescribeState(), Does.Contain("skipped"));
            Assert.That(skipped.DescribeState(), Does.Contain("not run"));
            Assert.That(skipped.DescribeState(), Does.Contain("finds nothing"));
            Assert.That(passed.DescribeState(), Does.Contain("passed"));
            Assert.That(passed.DescribeState(), Does.Contain("found nothing"));
            Assert.That(skipped.DescribeState(), Is.Not.EqualTo(passed.DescribeState()));
        }

        // ---------- the sheet numbers ----------

        [Test]
        public void TestsAreNumberedInOrderAndNeverRepeat()
        {
            ClashReport report = Report();
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < 1830; i++)
            {
                TestReport test = report.AddTest("test " + i);

                Assert.That(test.Number, Is.EqualTo(i + 1));
                Assert.That(names.Add(test.SheetName()), Is.True, "sheet name repeated at " + i);
                Assert.That(SheetNames.IsAcceptable(test.SheetName()), Is.True);
            }
        }

        // ---------- the discipline comes off the folder names in the file ----------

        [Test]
        public void TheDisciplineIsTheFirstFolderUnderTheTreeRoot()
        {
            ClashReport report = Report();

            Assert.That(
                report.DisciplineOf(Root + "/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals"),
                Is.EqualTo("Mechanical"));
            Assert.That(report.DisciplineOf(Root + "/Architecture/BLD-AR-Floors"),
                Is.EqualTo("Architecture"));
        }

        [Test]
        public void ASetAtTheRootHasNoDisciplineRatherThanAGuessedOne()
        {
            Assert.That(Report().DisciplineOf(Root + "/BLD-AR-Floors"), Is.EqualTo(string.Empty));
            Assert.That(Report().DisciplineOf(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(Report().DisciplineOf(null), Is.EqualTo(string.Empty));
        }

        // Whatever the file calls its folders is what the report calls its disciplines.
        // Nothing in the code holds a list of them.
        [Test]
        public void AProjectUsingItsOwnFolderNamesReadsThoseAndNotAList()
        {
            ClashReport report = Report();

            Assert.That(report.DisciplineOf(Root + "/Tunnelling/Segments/INF-TU-Rings"),
                Is.EqualTo("Tunnelling"));
            Assert.That(report.DisciplineOf(Root + "/Landscape/LAN-Planting"),
                Is.EqualTo("Landscape"));
        }

        [Test]
        public void ADifferentTreeRootIsHonouredBecauseItIsASetting()
        {
            ClashReport report = Report();
            report.SetTreeRoot = "some_other_root";

            Assert.That(report.DisciplineOf("some_other_root/Mechanical/BLD-ME-Ducts"),
                Is.EqualTo("Mechanical"));
        }

        [Test]
        public void TheSetNameIsTheLastPartOfThePath()
        {
            Assert.That(
                ClashReport.SetNameOf(Root + "/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals"),
                Is.EqualTo("BLD-ME-Air Terminals"));
            Assert.That(ClashReport.SetNameOf(string.Empty), Is.EqualTo(string.Empty));
        }

        // Two set names in the reference file end in a space, so nothing may trim one.
        [Test]
        public void ASetNameEndingInASpaceKeepsIt()
        {
            Assert.That(ClashReport.SetNameOf(Root + "/Electrical/BLD-EL-Devices "),
                Is.EqualTo("BLD-EL-Devices "));
        }

        // ---------- the totals ----------

        [Test]
        public void TheTotalsAreTheSumOfTheTests()
        {
            ClashReport report = Report();

            TestReport one = Ran(report, "one", Root + "/A/x", Root + "/B/y");
            one.Add(Row(ClashStatus.New, -0.1, 200));

            TestReport two = Ran(report, "two", Root + "/A/x", Root + "/C/z");
            two.Add(Row(ClashStatus.Active, -0.2, 13));

            Assert.That(report.TotalRawClashes, Is.EqualTo(213),
                "the real run found 213 clashes on 1C07BC");
            Assert.That(report.Totals.Of(ClashStatus.New), Is.EqualTo(200));
            Assert.That(report.Totals.Of(ClashStatus.Active), Is.EqualTo(13));
        }

        [Test]
        public void ANullRowIsRefusedRatherThanCounted()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { Report().AddTest("t").Add(null); });
        }

        [Test]
        public void ATestNumberBelowOneIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new TestReport(0, "t"); });
        }
    }
}
