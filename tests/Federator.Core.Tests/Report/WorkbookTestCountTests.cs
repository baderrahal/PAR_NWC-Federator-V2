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
    /// FR-035. The WORKBOOK CHECK counted a test only where its block held clashes, so every
    /// group said its workbook was short of the matrix while it held all the tests, and the
    /// check could not see a missing one row test. A test with no clash is one row since Q73,
    /// and it is a block of its own for the count, as the oracle read-out of a workbook counts
    /// it, 5 full blocks and 1825 of one row.
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
        public void TwoTestsWithClashesAndThreeWithNoneAreFiveTests()
        {
            WorkbookCheck check = WorkbookCheck.Of(TwoWithClashesAndThreeWithNone());

            Assert.That(check.Blocks, Is.EqualTo(5));
            Assert.That(check.Rows, Is.EqualTo(3));
            Assert.That(check.Passed, Is.True, string.Join(" | ", check.Lines()));
            Assert.That(check.Summary(), Does.Contain("one sheet, 5 tests, 3 rows"));
            Assert.That(string.Join("\n", check.Lines()), Does.Contain("5 test blocks (2 with clashes and 3 of one row), 3 clash rows."));
            Assert.That(CreationPlan.BlockCountLine(check.Blocks, 5), Is.EqualTo(
                "BLOCKS   5 in the workbook, one for every test in the file"));
        }

        [Test]
        public void TheLineSaysHowManyAreFullAndHowManyAreOneRow()
        {
            WorkbookCheck check = WorkbookCheck.Of(TwoWithClashesAndThreeWithNone());

            Assert.That(check.FullBlocks, Is.EqualTo(2));
            Assert.That(check.OneRowTests, Is.EqualTo(3));
        }

        [Test]
        public void AWorkbookOfTestsWithNoClashAtAllIsNotCalledEmpty()
        {
            WorkbookCheck check = WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 0), T("BLD-A-vs-BLD-C", 0), T("BLD-A-vs-BLD-D", 0)));

            Assert.That(check.Blocks, Is.EqualTo(3));
            Assert.That(check.Rows, Is.EqualTo(0));
            Assert.That(check.Passed, Is.True, string.Join(" | ", check.Lines()));
            Assert.That(string.Join("\n", check.Lines()), Does.Not.Contain("no clash table at all"));
        }

        /// <summary>
        /// A workbook of one row tests has no heading row, so no block layout can be compared, and it
        /// must not say it matched. It still reads row 1 and the widths, which hold without a block.
        /// </summary>
        [Test]
        public void AWorkbookOfOneRowTestsSaysItComparedNoBlockLayout()
        {
            WorkbookCheck check = WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 0), T("BLD-A-vs-BLD-C", 0)));
            string lines = string.Join("\n", check.Lines());

            Assert.That(check.Passed, Is.True, lines);
            Assert.That(lines, Does.Not.Contain("Every column, value shape, fill, border, row height"));
            Assert.That(lines, Does.Contain("No test holds a clash, so no heading, cell, fill or border of a block was compared"));
            Assert.That(check.Summary(), Does.Contain("no test holds a clash, so the layout of a block was not compared"));
            Assert.That(check.Summary(), Does.Not.Contain("matching the client's layout"));
        }

        /// <summary>The sheet wide things a one row workbook can still be wrong in are still read.</summary>
        [Test]
        public void AWorkbookOfOneRowTestsStillNamesAWrongTitleAndAWrongWidth()
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
        /// With a priority file picked the order is A, B, C, so a test of one row can stand before and
        /// between full blocks. Each is counted once and no clash row is counted as a test.
        /// </summary>
        [Test]
        public void OneRowTestsBeforeAndBetweenFullBlocksAreCountedOnce()
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

            Assert.That(check.Blocks, Is.EqualTo(5));
            Assert.That(check.FullBlocks, Is.EqualTo(2));
            Assert.That(check.OneRowTests, Is.EqualTo(3));
            Assert.That(check.Rows, Is.EqualTo(5));
        }

        [Test]
        public void AWorkbookMissingAOneRowTestNamesTheShortfall()
        {
            string path = TwoWithClashesAndThreeWithNone();
            int lastUsed;

            using (XLWorkbook book = new XLWorkbook(path))
            {
                IXLWorksheet sheet = book.Worksheet(1);
                lastUsed = sheet.LastRowUsed().RowNumber();
                sheet.Row(lastUsed).Delete();
                book.Save();
            }

            WorkbookCheck check = WorkbookCheck.Of(path);

            Assert.That(check.Blocks, Is.EqualTo(4), "the last one row test was deleted");
            Assert.That(CreationPlan.BlockCountLine(check.Blocks, 5), Does.StartWith("BLOCKS   4 in the workbook against 5 tests in the file."));
        }

        [Test]
        public void ATestWithNoNameIsStillCounted()
        {
            WorkbookCheck check = WorkbookCheck.Of(Written(T("BLD-A-vs-BLD-B", 1), T(string.Empty, 0)));

            Assert.That(check.Blocks, Is.EqualTo(2));
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
