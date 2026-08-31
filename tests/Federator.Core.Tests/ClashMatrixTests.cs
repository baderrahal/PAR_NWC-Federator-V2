using System;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Counts per pair, sides down and across. The one thing that matters most here is
    /// that a skipped pair never reads as a zero, because a zero says the two disciplines
    /// are coordinated and a skip says nobody looked.
    /// </summary>
    [TestFixture]
    public class ClashMatrixTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Floors = Root + "/Architecture/BLD-AR-Floors";
        private const string Walls = Root + "/Architecture/BLD-AR-Walls";
        private const string Ducts = Root + "/Mechanical/BLD-ME-Ducts";
        private const string Cables = Root + "/Electrical/BLD-EL-Cables";

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = Root;
            return report;
        }

        private static TestReport Pair(ClashReport report, string left, string right)
        {
            TestReport test = report.AddTest(
                ClashReport.SetNameOf(left) + " v " + ClashReport.SetNameOf(right));
            test.LeftLocator = left;
            test.RightLocator = right;
            return test;
        }

        private static void Found(TestReport test, ClashStatus status, int howMany)
        {
            ClashRow row = new ClashRow();
            row.Name = "row";
            row.Status = status;
            row.RawClashes = howMany;
            row.IsGroup = howMany > 1;
            test.Add(row);
            test.State = TestState.FoundClashes;
        }

        // ---------- a skip is never a zero ----------

        // The one the brief asks for by name.
        [Test]
        public void ASkippedPairSaysSkippedAndNeverZero()
        {
            ClashReport report = Report();

            TestReport skipped = Pair(report, Floors, Ducts);
            skipped.State = TestState.Skipped;
            skipped.SkippedReason = "the right side finds nothing in this model";

            ClashMatrix matrix = ClashMatrix.From(report);
            MatrixCell cell = matrix.At(Floors, Ducts);

            Assert.That(cell.Kind, Is.EqualTo(MatrixCellKind.Skipped));
            Assert.That(cell.Text(), Is.EqualTo("skipped"));
            Assert.That(cell.Text(), Is.Not.EqualTo("0"),
                "a skipped pair read as a zero, which says the two are coordinated");
        }

        // A test that ran and found nothing IS a zero, and that is the whole difference.
        [Test]
        public void APairThatRanAndFoundNothingIsAZero()
        {
            ClashReport report = Report();
            Pair(report, Floors, Ducts).State = TestState.Passed;

            MatrixCell cell = ClashMatrix.From(report).At(Floors, Ducts);

            Assert.That(cell.Kind, Is.EqualTo(MatrixCellKind.Ran));
            Assert.That(cell.Text(), Is.EqualTo("0"));
        }

        [Test]
        public void SkippedAndPassedNeverProduceTheSameCell()
        {
            ClashReport report = Report();

            Pair(report, Floors, Ducts).State = TestState.Skipped;
            Pair(report, Walls, Cables).State = TestState.Passed;

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.At(Floors, Ducts).Text(),
                Is.Not.EqualTo(matrix.At(Walls, Cables).Text()));
        }

        // A pair no test covers is not a zero either. Nobody wrote a test for it.
        [Test]
        public void APairWithNoTestAtAllIsBlankRatherThanZero()
        {
            ClashReport report = Report();
            Pair(report, Floors, Ducts).State = TestState.Passed;

            MatrixCell cell = ClashMatrix.From(report).At(Floors, Cables);

            Assert.That(cell.Kind, Is.EqualTo(MatrixCellKind.NoTest));
            Assert.That(cell.Text(), Is.EqualTo(string.Empty));
        }

        // ---------- the number in a cell ----------

        [Test]
        public void TheCellHoldsNewPlusActive()
        {
            ClashReport report = Report();
            TestReport test = Pair(report, Floors, Ducts);

            Found(test, ClashStatus.New, 12);
            Found(test, ClashStatus.Active, 3);
            Found(test, ClashStatus.Resolved, 40);
            Found(test, ClashStatus.Approved, 7);

            MatrixCell cell = ClashMatrix.From(report).At(Floors, Ducts);

            Assert.That(cell.NewPlusActive, Is.EqualTo(15));
            Assert.That(cell.Text(), Is.EqualTo("15"));
            Assert.That(cell.RawClashes, Is.EqualTo(62), "the raw count is still carried");
        }

        [Test]
        public void TheGridIsSymmetricBecauseAClashHasNoDirection()
        {
            ClashReport report = Report();
            TestReport test = Pair(report, Floors, Ducts);
            Found(test, ClashStatus.New, 5);

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.At(Floors, Ducts).Text(), Is.EqualTo("5"));
            Assert.That(matrix.At(Ducts, Floors).Text(), Is.EqualTo("5"),
                "half the grid was left blank");
        }

        // ---------- the axes ----------

        [Test]
        public void EverySetNamedByATestIsAnAxisEntryExactlyOnce()
        {
            ClashReport report = Report();

            Pair(report, Floors, Ducts).State = TestState.Passed;
            Pair(report, Floors, Cables).State = TestState.Passed;
            Pair(report, Walls, Ducts).State = TestState.Passed;

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.Size, Is.EqualTo(4));
            Assert.That(matrix.Locators, Does.Contain(Floors));
            Assert.That(matrix.Locators, Does.Contain(Walls));
            Assert.That(matrix.Locators, Does.Contain(Ducts));
            Assert.That(matrix.Locators, Does.Contain(Cables));
        }

        // Read off the folder names in the file, never off a list in the code.
        [Test]
        public void TheAxesCarryTheDisciplineTheFileGaveTheFolder()
        {
            ClashReport report = Report();
            Pair(report, Floors, Ducts).State = TestState.Passed;

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.DisciplineOf(Floors), Is.EqualTo("Architecture"));
            Assert.That(matrix.DisciplineOf(Ducts), Is.EqualTo("Mechanical"));
            Assert.That(ClashMatrix.SetNameOf(Floors), Is.EqualTo("BLD-AR-Floors"));
        }

        [Test]
        public void TheAxesAreGroupedByDisciplineSoOneBlockReadsAsOneBlock()
        {
            ClashReport report = Report();

            Pair(report, Ducts, Cables).State = TestState.Passed;
            Pair(report, Walls, Floors).State = TestState.Passed;

            ClashMatrix matrix = ClashMatrix.From(report);

            // Architecture, Architecture, Electrical, Mechanical.
            Assert.That(matrix.DisciplineOf(matrix.Locators[0]), Is.EqualTo("Architecture"));
            Assert.That(matrix.DisciplineOf(matrix.Locators[1]), Is.EqualTo("Architecture"));
            Assert.That(matrix.DisciplineOf(matrix.Locators[2]), Is.EqualTo("Electrical"));
            Assert.That(matrix.DisciplineOf(matrix.Locators[3]), Is.EqualTo("Mechanical"));
        }

        // ---------- the counts over the whole grid ----------

        [Test]
        public void EachPairIsCountedOnceRatherThanTwice()
        {
            ClashReport report = Report();

            Pair(report, Floors, Ducts).State = TestState.Skipped;
            Pair(report, Floors, Cables).State = TestState.Skipped;

            TestReport ran = Pair(report, Walls, Ducts);
            Found(ran, ClashStatus.New, 4);

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.CountOf(MatrixCellKind.Skipped), Is.EqualTo(2));
            Assert.That(matrix.CountOf(MatrixCellKind.Ran), Is.EqualTo(1));
        }

        // On a run where most groups hold two or three disciplines, most pairs are skips.
        // That has to be visible rather than looking like a coordinated model.
        [Test]
        public void AMostlySkippedRunLooksMostlySkippedAndNotMostlyClean()
        {
            ClashReport report = Report();
            string[] sets = { Floors, Walls, Ducts, Cables };

            for (int i = 0; i < sets.Length; i++)
            {
                for (int j = i + 1; j < sets.Length; j++)
                {
                    TestReport test = Pair(report, sets[i], sets[j]);

                    // Only the one Architecture against Mechanical pair actually ran.
                    if (sets[i] == Floors && sets[j] == Ducts)
                    {
                        Found(test, ClashStatus.New, 9);
                    }
                    else
                    {
                        test.State = TestState.Skipped;
                    }
                }
            }

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.CountOf(MatrixCellKind.Ran), Is.EqualTo(1));
            Assert.That(matrix.CountOf(MatrixCellKind.Skipped), Is.EqualTo(5));
            Assert.That(matrix.CountOf(MatrixCellKind.NoTest), Is.EqualTo(0));

            foreach (string down in matrix.Locators)
            {
                foreach (string across in matrix.Locators)
                {
                    if (string.Equals(down, across, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    MatrixCell cell = matrix.At(down, across);

                    if (cell.Kind == MatrixCellKind.Skipped)
                    {
                        Assert.That(cell.Text(), Is.EqualTo("skipped"),
                            "a skipped pair has to say so wherever it appears");
                    }
                }
            }
        }

        [Test]
        public void NoReportAtAllIsRefusedRatherThanBuilt()
        {
            Assert.Throws<ArgumentNullException>(delegate { ClashMatrix.From(null); });
        }

        [Test]
        public void AReportWithNoTestsGivesAnEmptyGridRatherThanThrowing()
        {
            ClashMatrix matrix = ClashMatrix.From(Report());

            Assert.That(matrix.Size, Is.EqualTo(0));
            Assert.That(matrix.At("a", "b").Kind, Is.EqualTo(MatrixCellKind.NoTest));
        }

        // Two different pairs must never land in one cell.
        [Test]
        public void TwoPairsThatLookAlikeWhenJoinedAreStillTwoCells()
        {
            ClashReport report = Report();

            TestReport first = Pair(report, Root + "/A/b", Root + "/c");
            Found(first, ClashStatus.New, 1);

            TestReport second = Pair(report, Root + "/A", Root + "/b/c");
            Found(second, ClashStatus.Active, 2);

            ClashMatrix matrix = ClashMatrix.From(report);

            Assert.That(matrix.At(Root + "/A/b", Root + "/c").NewPlusActive, Is.EqualTo(1));
            Assert.That(matrix.At(Root + "/A", Root + "/b/c").NewPlusActive, Is.EqualTo(2));
        }
    }
}
