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
    /// The client's sheet read back test by test, by the name in column A, F127, so the rows
    /// under each test and its Clashes cell can be set beside what Clash Detective holds.
    /// The workbook check counts a block only where it finds the Clash Name heading, so it
    /// sees full blocks alone and never the one row a test that found nothing has been
    /// written as since Q73. This reader takes both shapes. Every workbook here is a real
    /// xlsx the writer made, read off the disk.
    /// </summary>
    [TestFixture]
    public class WorkbookTestsTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string OutputName = "1104-PAR-100000-ZZZ-BM-RPT-000001";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorWorkbookTests");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        // ---------- a workbook to read ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("100000", OutputName);
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 5, 3, 0, 0);
            return report;
        }

        /// <summary>A test with that many plain clash rows, and a group row holding that many clashes where above zero.</summary>
        internal static TestReport AddTest(ClashReport report, string name, int plainRows, int clashesInAGroup)
        {
            TestReport test = report.AddTest(name);
            test.LeftLocator = Root + "/Architecture/BLD-AR-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Floors";
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = plainRows + clashesInAGroup > 0 ? TestState.FoundClashes : TestState.Passed;

            for (int i = 1; i <= plainRows; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + i;
                row.Status = ClashStatus.New;
                row.RawClashes = 1;
                test.Add(row);
            }

            if (clashesInAGroup > 0)
            {
                ClashRow group = ClashRow.ForGroup(clashesInAGroup);
                group.Name = "Group1";
                group.Status = ClashStatus.New;
                test.Add(group);
            }

            return test;
        }

        internal static string Write(ClashReport report, string folder)
        {
            return new WorkbookWriter().Write(report, Path.Combine(folder, OutputName + ".xlsx"));
        }

        internal static WorkbookTests ReadBack(string path)
        {
            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                return WorkbookTests.Read(workbook.Worksheet(1));
            }
        }

        private static WorkbookTest Only(WorkbookTests read, string name)
        {
            IList<WorkbookTest> named = read.Named(name);
            Assert.That(named.Count, Is.EqualTo(1), name + " should be on exactly one test of the sheet");
            return named[0];
        }

        /// <summary>The row of the first clash under the block of that test.</summary>
        private static int FirstClashRow(IXLWorksheet sheet, string name)
        {
            int last = sheet.LastRowUsed().RowNumber();

            for (int row = 1; row <= last; row++)
            {
                if (sheet.Cell(row, 1).GetString() == name)
                {
                    return row + 5;
                }
            }

            throw new InvalidOperationException(name + " is not on the sheet");
        }

        private static int RowOf(IXLWorksheet sheet, string name)
        {
            return FirstClashRow(sheet, name) - 5;
        }

        // ---------- both shapes ----------

        [Test]
        public void EveryTestIsReadInBothShapesWithItsRowsAndItsClashesCell()
        {
            ClashReport report = Report();
            AddTest(report, "Walls-vs-Floors", 3, 0);
            AddTest(report, "Doors-vs-Walls", 0, 0);
            AddTest(report, "Site-vs-Doors", 1, 5);

            WorkbookTests read = ReadBack(Write(report, folder));

            Assert.That(read.Tests.Count, Is.EqualTo(3));
            Assert.That(read.Doubts, Is.Empty);

            WorkbookTest full = Only(read, "Walls-vs-Floors");
            Assert.That(full.FullBlock, Is.True);
            Assert.That(full.Rows, Is.EqualTo(3));
            Assert.That(full.Clashes, Is.EqualTo(3));

            WorkbookTest oneRow = Only(read, "Doors-vs-Walls");
            Assert.That(oneRow.FullBlock, Is.False, "a test that found nothing is one row since Q73");
            Assert.That(oneRow.Rows, Is.EqualTo(0));
            Assert.That(oneRow.Clashes, Is.EqualTo(0));

            // A group is one row standing for the clashes under it.
            WorkbookTest grouped = Only(read, "Site-vs-Doors");
            Assert.That(grouped.Rows, Is.EqualTo(2));
            Assert.That(grouped.Clashes, Is.EqualTo(6));
        }

        // The break the design names: one clash row taken out of a real file reads one fewer.
        [Test]
        public void AClashRowDeletedFromTheFileReadsOneFewer()
        {
            ClashReport report = Report();
            AddTest(report, "Walls-vs-Floors", 3, 0);
            AddTest(report, "Doors-vs-Walls", 0, 0);
            string path = Write(report, folder);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Row(FirstClashRow(sheet, "Walls-vs-Floors")).Delete();
                workbook.Save();
            }

            WorkbookTest test = Only(ReadBack(path), "Walls-vs-Floors");

            Assert.That(test.Rows, Is.EqualTo(2));
            Assert.That(test.Clashes, Is.EqualTo(3), "the Clashes cell still says what was written");
        }

        // ---------- doubts ----------

        [Test]
        public void ANameOnTwoTestsOfTheSheetIsADoubt()
        {
            ClashReport report = Report();
            AddTest(report, "T", 2, 0);
            AddTest(report, "T", 0, 0);

            WorkbookTests read = ReadBack(Write(report, folder));

            Assert.That(read.Named("T").Count, Is.EqualTo(2));
            Assert.That(read.Doubts.Count, Is.EqualTo(1));
            Assert.That(read.Doubts[0], Does.Contain("\"T\""));
            Assert.That(read.Doubts[0], Does.Contain("2 tests"));
        }

        [Test]
        public void ATestRowWithNoNameIsADoubtAndNoTest()
        {
            ClashReport report = Report();
            AddTest(report, "A", 0, 0);
            AddTest(report, "B", 0, 0);
            string path = Write(report, folder);
            int row;

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                row = RowOf(sheet, "B");
                sheet.Cell(row, 1).Clear(XLClearOptions.Contents);
                workbook.Save();
            }

            WorkbookTests read = ReadBack(path);

            Assert.That(read.Tests.Count, Is.EqualTo(1));
            Assert.That(read.Doubts.Count, Is.EqualTo(1));
            Assert.That(read.Doubts[0], Does.Contain("row " + row));
            Assert.That(read.Doubts[0], Does.Contain("no name"));
        }

        [Test]
        public void AFullBlockWithNoNameIsADoubtToo()
        {
            ClashReport report = Report();
            AddTest(report, "A", 2, 0);
            string path = Write(report, folder);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Cell(RowOf(sheet, "A"), 1).Clear(XLClearOptions.Contents);
                workbook.Save();
            }

            WorkbookTests read = ReadBack(path);

            Assert.That(read.Tests, Is.Empty);
            Assert.That(read.Doubts.Count, Is.EqualTo(1));
            Assert.That(read.Doubts[0], Does.Contain("no name"));
        }

        [Test]
        public void AClashesCellThatIsNoWholeNumberReadsMinusOneAndIsADoubt()
        {
            ClashReport report = Report();
            AddTest(report, "A", 0, 0);
            AddTest(report, "B", 2, 0);
            string path = Write(report, folder);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Cell(RowOf(sheet, "A"), WorkbookWriter.ColumnTestHeader + 1).Value = "none";
                sheet.Cell(RowOf(sheet, "B") + 1, WorkbookWriter.ColumnTestHeader + 1).Value = 2.5;
                workbook.Save();
            }

            WorkbookTests read = ReadBack(path);

            Assert.That(Only(read, "A").Clashes, Is.EqualTo(-1), "a word read as a count");
            Assert.That(Only(read, "B").Clashes, Is.EqualTo(-1), "a fraction read as a count");
            Assert.That(read.Doubts.Count, Is.EqualTo(2));
        }

        [Test]
        public void ABlockWithNoClashTableUnderItReadsMinusOneRowsAndIsADoubt()
        {
            ClashReport report = Report();
            AddTest(report, "A", 2, 0);
            string path = Write(report, folder);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Cell(FirstClashRow(sheet, "A") - 1, WorkbookWriter.ColumnClashName).Clear(XLClearOptions.Contents);
                workbook.Save();
            }

            WorkbookTests read = ReadBack(path);

            Assert.That(Only(read, "A").Rows, Is.EqualTo(-1));
            Assert.That(read.Doubts.Count, Is.EqualTo(1));
            Assert.That(read.Doubts[0], Does.Contain("\"A\""));
        }

        [Test]
        public void AReportWithNoTestsReadsNoTestAndNoDoubt()
        {
            WorkbookTests read = ReadBack(Write(Report(), folder));

            Assert.That(read.Tests, Is.Empty);
            Assert.That(read.Doubts, Is.Empty, "the title row is no test");
        }

        [Test]
        public void ANameIsReadExactlyAndNeverTrimmed()
        {
            ClashReport report = Report();
            AddTest(report, "BLD-EL-Devices -vs-BLD-AR-Walls", 1, 0);

            WorkbookTests read = ReadBack(Write(report, folder));

            Assert.That(read.Named("BLD-EL-Devices -vs-BLD-AR-Walls").Count, Is.EqualTo(1));
            Assert.That(read.Named("BLD-EL-Devices-vs-BLD-AR-Walls").Count, Is.EqualTo(0));
        }

        [Test]
        public void NoSheetIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => WorkbookTests.Read(null));
        }

        // ---------- the clash workbook read by the client's sheet's name ----------

        /// <summary>
        /// THE CLIENT'S SHEET IS READ BY ITS NAME, the readers' point on F127 that Read handed the first
        /// sheet would read a sheet put before it. A sheet named Coverage put first is not read, and the
        /// client's sheet behind it is. The break beside it: the first sheet read by place holds none of the
        /// client's tests.
        /// </summary>
        [Test]
        public void ReadFileReadsTheClientsSheetByItsNameWhereAnotherSheetStandsFirst()
        {
            ClashReport report = Report();
            AddTest(report, "T1", 2, 0);
            string path = Write(report, folder);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                workbook.Worksheets.Add("Coverage", 1);
                workbook.Save();
            }

            string why;
            WorkbookTests read = WorkbookTests.ReadFile(path, SheetNames.ForReport(OutputName), out why);

            Assert.That(why, Is.Null);
            Assert.That(read, Is.Not.Null);
            Assert.That(Only(read, "T1").Rows, Is.EqualTo(2));
            Assert.That(ReadBack(path).Named("T1"), Is.Empty, "the first sheet by place is the one put before it");
        }

        [Test]
        public void ReadFileSaysWhereTheWorkbookHoldsNoSheetOfThatName()
        {
            ClashReport report = Report();
            AddTest(report, "T1", 2, 0);
            string path = Write(report, folder);
            string why;

            Assert.That(WorkbookTests.ReadFile(path, "Coverage", out why), Is.Null);
            Assert.That(why, Is.EqualTo("the workbook at " + path + " holds no sheet named Coverage"));

            // Exactly, so a name differing by letter case alone is not the client's sheet.
            Assert.That(WorkbookTests.ReadFile(path, SheetNames.ForReport(OutputName).ToLowerInvariant(), out why), Is.Null);
        }

        [Test]
        public void ReadFileSaysWhereThereIsNoWorkbook()
        {
            string path = Path.Combine(folder, "not-there.xlsx");
            string why;

            Assert.That(WorkbookTests.ReadFile(path, "x", out why), Is.Null);
            Assert.That(why, Is.EqualTo("there is no workbook at " + path));
        }
    }
}
