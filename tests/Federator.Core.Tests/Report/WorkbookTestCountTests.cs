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
