using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The workbook check counts the blocks, one per test with a clash, F153, Bader's decision of
    /// 2026-10-13, which retires the one row Q73 gave a test with no clash and the count of FR-035
    /// that read one block per test of the file. A row of a test with no clash is a problem, and a
    /// workbook where no test found a clash holds its title alone and passes.
    /// </summary>
    [TestFixture]
    public class WorkbookTestCountTests
    {
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001";

        private string folder;

        [SetUp]
        public void Setup()
        {
            folder = TempFolder.Make("f118-count");
        }

        [TearDown]
        public void Teardown()
        {
            TempFolder.Remove(folder);
        }

        private static void AddTest(ClashReport report, string name, int clashes)
        {
            TestReport test = report.AddTest(name);
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "Complete";
            test.State = clashes > 0 ? TestState.FoundClashes : TestState.Passed;

            for (int i = 1; i <= clashes; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + i;
                row.Status = ClashStatus.New;
                row.Distance = -0.116;
                row.GridLocation = "A-1";
                row.Level = "LGF";
                row.Description = "Hard (Conservative)";
                row.X = 33.399;
                row.Y = 8.310;
                row.Z = -0.441;
                row.Left.ElementId = "707077";
                row.Left.Name = "ELE-CND-Standard";
                row.Left.ItemType = "Solid";
                row.Right.ElementId = "1369872";
                row.Right.Name = "Concrete";
                row.Right.ItemType = "Solid";
                test.Add(row);
            }
        }

        private string Written(params KeyValuePair<string, int>[] tests)
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.DocumentUnits = "m";

            foreach (KeyValuePair<string, int> test in tests)
            {
                AddTest(report, test.Key, test.Value);
            }

            string path = Path.Combine(folder, OutputName + ".xlsx");
            new WorkbookWriter().Write(report, path);
            return path;
        }

        private static KeyValuePair<string, int> T(string name, int clashes)
        {
            return new KeyValuePair<string, int>(name, clashes);
        }

        private string TwoWithClashesAndThreeWithNone()
        {
            return Written(T("BLD-A-vs-BLD-B", 2), T("BLD-A-vs-BLD-C", 1), T("BLD-A-vs-BLD-D", 0), T("BLD-A-vs-BLD-E", 0), T("BLD-A-vs-BLD-F", 0));
        }

        [Test]
        public void TwoTestsWithClashesAndThreeWithNoneAreTwoBlocksAndNothingElse()
        {
            WorkbookCheck check = WorkbookCheck.Of(TwoWithClashesAndThreeWithNone());

            Assert.That(check.Blocks, Is.EqualTo(2));
            Assert.That(check.NoClashRows, Is.EqualTo(0));
            Assert.That(check.Rows, Is.EqualTo(3));
            Assert.That(check.Passed, Is.True, string.Join(" | ", check.Lines()));
            Assert.That(check.Summary(), Does.Contain("one sheet, 2 tests with clashes, 3 rows"));
            Assert.That(string.Join("\n", check.Lines()), Does.Contain("2 test blocks with clashes, 3 clash rows. A test with no clash writes no row here since F153."));
            Assert.That(CreationPlan.BlockCountLine(check.Blocks, 2, 5, 0), Does.StartWith(
                "BLOCKS   2 in the workbook, one for every test with a clash. The other 3 of the 5 tests"));
        }

        /// <summary>
        /// THE BREAK BADER ASKED FOR, F153: a workbook that still holds the one row Q73 gave a test with no clash fails
        /// the check, which names the first such row and counts them. The rows are written in the old shape, the name in
        /// A and the tolerance where the test header starts, below the last block, and a third at row 4 before the first.
        /// The break of the break: the workbook the writer made holds none and passes.
        /// </summary>
        [Test]
        public void AWorkbookThatStillHoldsANoClashRowFailsTheCheck()
        {
            string path = TwoWithClashesAndThreeWithNone();

            Assert.That(WorkbookCheck.Of(path).Passed, Is.True);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                int last = sheet.LastRowUsed().RowNumber();
                sheet.Cell(last + 4, 1).Value = "BLD-A-vs-BLD-D";
                sheet.Cell(last + 4, WorkbookWriter.ColumnTestHeader).Value = "0.025m";
                sheet.Cell(last + 5, 1).Value = "BLD-A-vs-BLD-E";
                sheet.Cell(last + 5, WorkbookWriter.ColumnTestHeader).Value = "0.025m";
                workbook.SaveAs(path);
            }

            WorkbookCheck two = WorkbookCheck.Of(path);
            string lines = string.Join("\n", two.Lines());

            Assert.That(two.Passed, Is.False);
            Assert.That(two.NoClashRows, Is.EqualTo(2));
            Assert.That(two.Blocks, Is.EqualTo(2), "the rows are no blocks");
            Assert.That(lines, Does.Contain("2 rows outside every block hold a test with no clash, the first at row "));
            Assert.That(lines, Does.Contain("\"BLD-A-vs-BLD-D\". A test with no clash writes nothing in the workbook since F153"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Row(4).InsertRowsAbove(1);
                sheet.Cell(4, WorkbookWriter.ColumnTestHeader).Value = "0.025m";
                workbook.SaveAs(path);
            }

            WorkbookCheck three = WorkbookCheck.Of(path);

            Assert.That(three.NoClashRows, Is.EqualTo(3));
            Assert.That(string.Join("\n", three.Lines()), Does.Contain("the first at row 4, with no name."),
                "a row with no name still shows by its tolerance");
        }

        /// <summary>
        /// Where no test found a clash the workbook holds its title alone, which is right since F153 and never called
        /// an empty sheet. Row 1 and the widths are still read, and the line says no block was compared.
        /// </summary>
        [Test]
        public void AWorkbookWhereNoTestFoundAClashHoldsTheTitleAloneAndPasses()
        {
            WorkbookCheck check = WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 0), T("BLD-A-vs-BLD-C", 0), T("BLD-A-vs-BLD-D", 0)));
            string lines = string.Join("\n", check.Lines());

            Assert.That(check.Blocks, Is.EqualTo(0));
            Assert.That(check.NoClashRows, Is.EqualTo(0));
            Assert.That(check.Passed, Is.True, lines);
            Assert.That(lines, Does.Contain("The sheet holds no block, so no heading, cell, fill or border of a block was compared"));
            Assert.That(lines, Does.Not.Contain("Every column, value shape, fill, border, row height"));
            Assert.That(check.Summary(), Is.EqualTo("Workbook: one sheet, no test with a clash, so the layout of a block was not compared."));
            Assert.That(CreationPlan.BlockCountLine(check.Blocks, 0, 3, 0), Does.Not.Contain("MUST"));
        }

        /// <summary>A workbook of a group row standing for ten clashes before three single rows, as the writer sorts it.</summary>
        private string GroupRowBeforeThreeSingleRows()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.DocumentUnits = "m";
            AddTest(report, "BLD-A-vs-BLD-B", 1);
            AddTest(report, "BLD-A-vs-BLD-C", 3);
            report.Tests[0].Rows[0].IsGroup = true;
            report.Tests[0].Rows[0].RawClashes = 10;

            string path = Path.Combine(folder, OutputName + ".xlsx");
            new WorkbookWriter().Write(report, path);
            Assert.That(report.Tests[0].RawClashes, Is.EqualTo(10));
            return path;
        }

        /// <summary>Sets the Clashes cell of the first full block to a value, as a hand edit or a damaged file would.</summary>
        private static void SetTheFirstClashesCell(string path, object value)
        {
            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);

                for (int row = 1; row <= sheet.LastRowUsed().RowNumber(); row++)
                {
                    if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() == "Clash Name")
                    {
                        IXLCell cell = sheet.Cell(row - 3, WorkbookWriter.ColumnTestHeader + 1);

                        if (value is string)
                        {
                            cell.Value = (string)value;
                        }
                        else
                        {
                            cell.Value = (int)value;
                        }

                        break;
                    }
                }

                workbook.SaveAs(path);
            }
        }

        /// <summary>
        /// T1-N82. The blocks are sorted by the clashes a test holds and a group row stands for many, so a block of one
        /// group row holding ten clashes comes before a block of three single rows and is in the right order. The check
        /// counted rows and said the tests were in the wrong order. The break: the Clashes cell of the first block
        /// overwritten with 2 reads 2 before 3, and the order is named.
        /// </summary>
        [Test]
        public void AGroupRowStandingForManyClashesIsNotOutOfOrderBesideThreeSingleRows()
        {
            string path = GroupRowBeforeThreeSingleRows();
            WorkbookCheck check = WorkbookCheck.Of(path);

            Assert.That(check.BlockCounts, Is.EqualTo(new[] { 1, 3 }), "one row, then three rows");
            Assert.That(check.Passed, Is.True, string.Join(" | ", check.Lines()));
            Assert.That(string.Join("\n", check.Lines()), Does.Not.Contain("wrong order"));
            Assert.That(string.Join("\n", check.Lines()), Does.Contain("the first blocks hold 1, 3 clash rows."));

            SetTheFirstClashesCell(path, 2);

            Assert.That(string.Join("\n", WorkbookCheck.Of(path).Lines()), Does.Contain("The tests are in the wrong order. Block 1 holds 2 clashes and block 2 holds 3."));
        }

        /// <summary>
        /// A block whose Clashes cell is nought is a real count and is ordered by it, and a cell that is no whole number
        /// sends the check back to the rows under each block and says so, so a damaged cell is never read as a count.
        /// </summary>
        [Test]
        public void ANoughtCellIsACountAndACellThatIsNoNumberFallsBackToRowsAndSaysSo()
        {
            string zero = GroupRowBeforeThreeSingleRows();
            SetTheFirstClashesCell(zero, 0);

            string told = string.Join("\n", WorkbookCheck.Of(zero).Lines());

            Assert.That(told, Does.Contain("The tests are in the wrong order. Block 1 holds 0 clashes and block 2 holds 3."));
            Assert.That(told, Does.Not.Contain("because a Clashes cell is no whole number"));

            string damaged = GroupRowBeforeThreeSingleRows();
            SetTheFirstClashesCell(damaged, "ten");

            string rows = string.Join("\n", WorkbookCheck.Of(damaged).Lines());

            Assert.That(rows, Does.Contain("The tests are in the wrong order. Block 1 holds 1 clashes and block 2 holds 3."));
            Assert.That(rows, Does.Contain("The counts are the rows under each block, because a Clashes cell is no whole number."));
        }

        /// <summary>
        /// T1-N80. Only the first block is read cell by cell and only five values of its first clash row for their shape,
        /// so the pass line says that and not that every column of every row matched.
        /// </summary>
        [Test]
        public void ThePassLineSaysWhatWasReadAndNotThatEverythingMatched()
        {
            WorkbookCheck check = WorkbookCheck.Of(TwoWithClashesAndThreeWithNone());
            string lines = string.Join("\n", check.Lines());

            Assert.That(check.Passed, Is.True, lines);
            Assert.That(lines, Does.Contain("The column headings of every block with clashes match the client's report, and so do the fill, border and row height of every cell of the first block, five values of its first clash row"));
            Assert.That(lines, Does.Contain("The blocks are in their order, and no row of a test with no clash is on the sheet. The values of the other blocks and clash rows were not read."));
            Assert.That(lines, Does.Not.Contain("Every column, value shape, fill, border, row height"));
        }

        /// <summary>
        /// A result group with nothing under it is a clash row, so its test is a block, whatever its Clashes cell says,
        /// while a test with no row at all writes nothing. The check counts the one block and passes.
        /// </summary>
        [Test]
        public void ABlockOfAnEmptyResultGroupIsABlockAndATestWithNoRowWritesNothing()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.DocumentUnits = "m";
            AddTest(report, "BLD-A-vs-BLD-B", 0);
            AddTest(report, "BLD-A-vs-BLD-C", 1);
            report.Tests[1].Rows[0].IsGroup = true;
            report.Tests[1].Rows[0].RawClashes = 0;

            string path = Path.Combine(folder, OutputName + ".xlsx");
            new WorkbookWriter().Write(report, path);
            WorkbookCheck check = WorkbookCheck.Of(path);

            Assert.That(check.Blocks, Is.EqualTo(1));
            Assert.That(check.NoClashRows, Is.EqualTo(0));
            Assert.That(report.TestsWithAClash, Is.EqualTo(1));
            Assert.That(check.Passed, Is.True, string.Join(" | ", check.Lines()));
        }

        /// <summary>The sheet wide things a workbook with no block can still be wrong in are still read.</summary>
        [Test]
        public void AWorkbookWithNoBlockStillNamesAWrongTitleAndAWrongWidth()
        {
            string title = Written(T("BLD-A-vs-BLD-B", 0));

            using (XLWorkbook book = new XLWorkbook(title))
            {
                book.Worksheet(1).Cell(1, 4).Value = "Report";
                book.Save();
            }

            WorkbookCheck wrongTitle = WorkbookCheck.Of(title);

            Assert.That(wrongTitle.Passed, Is.False);
            Assert.That(string.Join(" ", wrongTitle.Lines()), Does.Contain("Row 1 reads \"Report\""));

            string widths = Written(T("BLD-A-vs-BLD-B", 0));

            using (XLWorkbook book = new XLWorkbook(widths))
            {
                book.Worksheet(1).Column(6).Width = 3.0;
                book.Save();
            }

            WorkbookCheck wrongWidth = WorkbookCheck.Of(widths);

            Assert.That(wrongWidth.Passed, Is.False);
            Assert.That(string.Join(" ", wrongWidth.Lines()), Does.Contain("Column F is"));
        }

        /// <summary>
        /// Tests with no clash and tests with clashes together, with a priority file picked. The blocks keep the measured
        /// order, FR-199, the tests with no clash write nothing, and no clash row is counted as a test.
        /// </summary>
        [Test]
        public void WithAPriorityFileTheTestsWithNoClashWriteNothingEither()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.DocumentUnits = "m";
            AddTest(report, "T1-none-A", 0);
            AddTest(report, "T2-two-B", 2);
            AddTest(report, "T3-none-B", 0);
            AddTest(report, "T4-three-C", 3);
            AddTest(report, "T5-none-C", 0);
            report.Priorities = PriorityMap.Read(
                "test_name,left_set,right_set,priority\nT1-none-A,L,R,A\nT2-two-B,L,R,B\nT3-none-B,L,R,B\nT4-three-C,L,R,C\nT5-none-C,L,R,C\n",
                "p.csv");

            foreach (TestReport test in report.Tests)
            {
                test.Priority = report.Priorities.Of(test.Name);
            }

            string path = Path.Combine(folder, OutputName + ".xlsx");
            new WorkbookWriter().Write(report, path);
            WorkbookCheck check = WorkbookCheck.Of(path);

            Assert.That(check.Blocks, Is.EqualTo(2));
            Assert.That(check.NoClashRows, Is.EqualTo(0));
            Assert.That(check.Rows, Is.EqualTo(5));
            Assert.That(string.Join("\n", WorkbookCheck.Of(path, true).Lines()), Does.Not.Contain("wrong order"));
            Assert.That(WorkbookCheck.Of(path, true).Passed, Is.True, string.Join(" | ", WorkbookCheck.Of(path, true).Lines()));
        }

        /// <summary>A workbook short of a block of a test with a clash is said in capitals by the BLOCKS line.</summary>
        [Test]
        public void AWorkbookShortOfABlockOfATestWithAClashNamesTheShortfall()
        {
            WorkbookCheck check = WorkbookCheck.Of(TwoWithClashesAndThreeWithNone());

            Assert.That(CreationPlan.BlockCountLine(check.Blocks, 3, 5, 0), Does.StartWith(
                "BLOCKS   2 in the workbook against 3 tests with a clash of the 5 tests of the report. THE WORKBOOK MUST"));
        }

        [Test]
        public void ATestWithNoNameIsStillCountedAndOneWithNoClashAndNoNameWritesNothing()
        {
            Assert.That(WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 1), T(string.Empty, 1))).Blocks, Is.EqualTo(2));

            WorkbookCheck none = WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 1), T(string.Empty, 0)));

            Assert.That(none.Blocks, Is.EqualTo(1));
            Assert.That(none.NoClashRows, Is.EqualTo(0));
        }


        /// <summary>
        /// The page keeps every test while the workbook writes the tests with a clash alone, F153: the clash XML the
        /// page is drawn from carries a clashtest for each of the five tests, the three that found none among them, and
        /// the workbook beside it two blocks. The page is then rendered by Autodesk's stylesheet, which only a run shows.
        /// </summary>
        [Test]
        public void ThePageKeepsEveryTestWhileTheWorkbookWritesTheTestsWithAClash()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.DocumentUnits = "m";
            AddTest(report, "BLD-A-vs-BLD-B", 2);
            AddTest(report, "BLD-A-vs-BLD-C", 1);
            AddTest(report, "BLD-A-vs-BLD-D", 0);
            AddTest(report, "BLD-A-vs-BLD-E", 0);
            AddTest(report, "BLD-A-vs-BLD-F", 0);

            System.Xml.Linq.XDocument page = new ClashReportXml().Build(report);
            string path = Path.Combine(folder, OutputName + ".xlsx");
            new WorkbookWriter().Write(report, path);

            Assert.That(new List<System.Xml.Linq.XElement>(page.Descendants("clashtest")).Count, Is.EqualTo(5));
            Assert.That(WorkbookCheck.Of(path).Blocks, Is.EqualTo(2));
            Assert.That(report.TestsWithAClash, Is.EqualTo(2));
        }
        [Test]
        public void AClashRowIsNeverCountedAsATest()
        {
            WorkbookCheck check = WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 6)));

            Assert.That(check.Blocks, Is.EqualTo(1));
            Assert.That(check.Rows, Is.EqualTo(6));
        }
    }
}
