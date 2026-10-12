using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-041. Set 03 left the Grid Location cell empty on 345 of 5679 clash rows, all 13 of
    /// 100000 among them, and the log said nothing, because the WORKBOOK and REPORT CHECKs
    /// read the first clash row only. The line counts every empty cell and gives the heights
    /// of the empty rows and the filled ones, the first thing to measure against the grid
    /// levels, since why the grid gives nothing is UNKNOWN.
    /// </summary>
    [TestFixture]
    public class GridLocationLineTests
    {
        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1B06K1", "1104-PAR-1B06K1-ZZZ-BM-MOD-000001");
            report.DocumentUnits = "m";
            return report;
        }

        private static ClashRow Row(string grid, string level, double z)
        {
            ClashRow row = new ClashRow();
            row.GridLocation = grid;
            row.Level = level;
            row.Z = z;
            return row;
        }

        [Test]
        public void EveryEmptyCellIsCountedWithTheHeightsOfTheEmptyAndTheFilledRows()
        {
            ClashReport report = Report();
            TestReport first = report.AddTest("ARC vs MEP");
            first.Add(Row("D-1 : RO1", string.Empty, 694.491));
            first.Add(Row(string.Empty, string.Empty, 12.5));
            TestReport second = report.AddTest("STR vs MEP");
            second.Add(Row(string.Empty, string.Empty, 3.25));
            second.Add(Row(string.Empty, "RO1", 694.491));

            Assert.That(report.GridLocationLine(), Is.EqualTo(
                "GRID     Grid Location empty on 2 of 4 clash rows, the empty ones at z 3.250 to 12.500 m, "
                + "the filled ones at z 694.491 to 694.491 m"),
                "an empty cell past the first row of a test went uncounted");
        }

        [Test]
        public void AReportWhereNoRowHasAGridSaysSo()
        {
            ClashReport report = Report();
            TestReport test = report.AddTest("ARC vs MEP");
            test.Add(Row(string.Empty, string.Empty, 1.0));
            test.Add(Row(string.Empty, string.Empty, 2.0));

            Assert.That(report.GridLocationLine(), Is.EqualTo(
                "GRID     Grid Location empty on 2 of 2 clash rows, the empty ones at z 1.000 to 2.000 m, and no row has one"));
        }

        [Test]
        public void AReportWithEveryCellFilledSaysEveryOne()
        {
            ClashReport report = Report();
            report.AddTest("ARC vs MEP").Add(Row("B-1 : ROF", string.Empty, 5.0));

            Assert.That(report.GridLocationLine(), Is.EqualTo("GRID     Grid Location filled on every one of 1 clash row"));
        }

        [Test]
        public void UnitsNotKnownAreSaidUnknown()
        {
            ClashReport report = new ClashReport("1B06K1", "out");
            report.AddTest("ARC vs MEP").Add(Row(string.Empty, string.Empty, 1.0));

            Assert.That(report.GridLocationLine(), Does.Contain("1.000 in UNKNOWN units"));
        }

        [Test]
        public void AReportWithNoClashRowWritesNoLine()
        {
            ClashReport report = Report();
            report.AddTest("ARC vs MEP");

            Assert.That(report.GridLocationLine(), Is.Null);
        }
    }
}
