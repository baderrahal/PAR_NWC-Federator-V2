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
    /// F83, the one column this tool puts on the client's sheet, and the check that covers
    /// it. It sits one past their table, so none of the cell, width or column order checks
    /// reach it, and without a check of its own it would ship with nothing proving it is
    /// in the right place, filled, or absent on a run that picked no file.
    /// </summary>
    [TestFixture]
    public class PriorityColumnTests
    {
        private const string Root = "lcop_selection_set_tree";
        private string folder;

        [SetUp]
        public void Setup()
        {
            folder = TempFolder.Make("priority-column");
        }

        [TearDown]
        public void Cleanup()
        {
            TempFolder.Remove(folder);
        }

        private static ClashReport Report(bool picked)
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-RPT-000001");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "ft";
            report.RunAt = new DateTime(2026, 9, 19, 20, 0, 0);

            Add(report, "BLD-AR-Walls-vs-BLD-AR-Columns", 6);
            Add(report, "BLD-ME-Ducts-vs-BLD-AR-Walls", 2);

            if (picked)
            {
                report.Priorities = PriorityMap.Read(
                    "test_name,left_set,right_set,priority\r\n"
                        + "BLD-ME-Ducts-vs-BLD-AR-Walls,L,R,A\r\n",
                    "a.csv");

                foreach (TestReport test in report.Tests)
                {
                    test.Priority = report.Priorities.Of(test.Name);
                }
            }

            return report;
        }

        private static void Add(ClashReport report, string name, int clashes)
        {
            TestReport test = report.AddTest(name);
            test.LeftLocator = Root + "/a";
            test.RightLocator = Root + "/b";
            test.Tolerance = 0.2460629921;
            test.ToleranceUnits = "ft";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = TestState.FoundClashes;

            for (int i = 1; i <= clashes; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = name + " clash " + i;
                row.Status = ClashStatus.New;
                row.Distance = -0.328084;
                row.GridLocation = "D-8 : LGF";
                row.Level = "LGF";
                test.Add(row);
            }
        }

        private string Write(bool picked)
        {
            string path = Path.Combine(folder, "book.xlsx");
            return new WorkbookWriter(new ReportOptions()).Write(Report(picked), path);
        }

        [Test]
        public void WithNoFilePickedThereIsNoPriorityColumnAtAll()
        {
            string path = Write(false);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);

                for (int row = 1; row <= sheet.LastRowUsed().RowNumber(); row++)
                {
                    Assert.That(sheet.Cell(row, WorkbookWriter.ColumnPriority).GetString(),
                        Is.EqualTo(string.Empty), "row " + row);
                }
            }

            WorkbookCheck check = WorkbookCheck.Of(path, false);

            Assert.That(check.HasPriorityColumn, Is.False);
            Assert.That(check.Passed, Is.True,
                string.Join(" ", new List<string>(check.Problems).ToArray()));
        }

        [Test]
        public void WithAFilePickedEveryClashRowCarriesItsTestsLetter()
        {
            string path = Write(true);
            int headings = 0;
            int letters = 0;
            int empties = 0;

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);

                for (int row = 1; row <= sheet.LastRowUsed().RowNumber(); row++)
                {
                    string cell = sheet.Cell(row, WorkbookWriter.ColumnPriority).GetString();
                    bool isHeadingRow = sheet.Cell(row, WorkbookWriter.ColumnClashName)
                        .GetString() == "Clash Name";

                    if (isHeadingRow)
                    {
                        Assert.That(cell, Is.EqualTo(WorkbookWriter.PriorityHeading), "row " + row);
                        headings++;
                        continue;
                    }

                    if (cell == "A")
                    {
                        letters++;
                    }
                    else if (cell.Length == 0)
                    {
                        empties++;
                    }
                }
            }

            Assert.That(headings, Is.EqualTo(2), "one heading per block");
            Assert.That(letters, Is.EqualTo(2), "the two clashes of the A test");
        }

        /// <summary>
        /// THE BLOCKS STAY IN THE MEASURED ORDER WITH A PRIORITY FILE PICKED, FR-199, Bader's
        /// answer to Q49: the priority is a column to sort on in Excel, and the blocks keep
        /// the order measured off the client's exports, most clashes first. Picking a file
        /// moved the A test to the top though it holds fewer clashes, and the workbook, the
        /// clash XML and the picture numbers all followed, so a report with a file picked
        /// listed its tests in an order the client never accepted.
        /// </summary>
        [Test]
        public void PickingAFileLeavesTheBlocksInTheMeasuredOrder()
        {
            Assert.That(ReportOrder.Tests(Report(false))[0].Name,
                Is.EqualTo("BLD-AR-Walls-vs-BLD-AR-Columns"), "six clashes beats two");
            Assert.That(ReportOrder.Tests(Report(true))[0].Name,
                Is.EqualTo("BLD-AR-Walls-vs-BLD-AR-Columns"), "six clashes still beats two, A is a column and not an order");
            Assert.That(ReportOrder.Tests(Report(true))[1].Priority, Is.EqualTo(ClashPriority.A), "the letter stays on the test");
        }

        /// <summary>
        /// Since FR-199 a workbook with a priority file picked is in the measured order too, so
        /// the order check runs whether or not a file was picked, and a block out of order is
        /// named either way. It used to be switched off for a priority sorted workbook, which
        /// left every run that picked a file with no order check at all.
        /// </summary>
        [Test]
        public void AWorkbookWithAPriorityFileIsCheckedForTheMeasuredOrderToo()
        {
            string path = Write(true);

            WorkbookCheck told = WorkbookCheck.Of(path, true);

            Assert.That(told.Passed, Is.True,
                string.Join(" ", new List<string>(told.Problems).ToArray()));

            PutTheLastBlockOutOfOrder(path);

            WorkbookCheck broken = WorkbookCheck.Of(path, true);
            string all = string.Join(" ", new List<string>(broken.Problems).ToArray());

            Assert.That(broken.Passed, Is.False);
            Assert.That(all, Does.Contain("The tests are in the wrong order"));
        }

        /// <summary>
        /// Copies the last clash row of the last block eight times and raises its Clashes cell by eight, so that block
        /// holds more clashes than the one before it. The order is read off the Clashes cell, which is the count the
        /// writer sorted by, T1-N82.
        /// </summary>
        private static void PutTheLastBlockOutOfOrder(string path)
        {
            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                int last = sheet.LastRowUsed().RowNumber();

                while (last > 1 && sheet.Cell(last, WorkbookWriter.ColumnClashName).GetString().Length == 0)
                {
                    last--;
                }

                int heading = last;

                while (heading > 1 && sheet.Cell(heading, WorkbookWriter.ColumnClashName).GetString() != "Clash Name")
                {
                    heading--;
                }

                IXLCell held = sheet.Cell(heading - 3, WorkbookWriter.ColumnTestHeader + 1);
                held.Value = held.GetDouble() + 8;

                sheet.Row(last).InsertRowsBelow(8);

                for (int i = 1; i <= 8; i++)
                {
                    sheet.Row(last).CopyTo(sheet.Row(last + i));
                }

                workbook.Save();
            }
        }

        [Test]
        public void ACheckToldAFileWasPickedComplainsWhenTheColumnIsMissing()
        {
            WorkbookCheck check = WorkbookCheck.Of(Write(false), true);

            Assert.That(check.Passed, Is.False);
            Assert.That(check.FirstDivergence, Does.Contain("carries no Priority column"));
        }

        [Test]
        public void ACheckToldNothingWasPickedComplainsWhenTheColumnIsThere()
        {
            WorkbookCheck check = WorkbookCheck.Of(Write(true), false);
            string all = string.Join(" ", new List<string>(check.Problems).ToArray());

            Assert.That(check.Passed, Is.False);
            Assert.That(all, Does.Contain("no clash priority file was picked"));
        }

        /// <summary>
        /// The client's table stops at S and our column is past it. Widening LastColumn
        /// would make the check demand client banding and a measured client width on a
        /// column the client has never had.
        /// </summary>
        [Test]
        public void OurColumnIsPastTheirTableAndTheirTableDidNotMove()
        {
            Assert.That(WorkbookWriter.LastColumn, Is.EqualTo(19));
            Assert.That(WorkbookWriter.ColumnPriority, Is.EqualTo(20));
            Assert.That(WorkbookWriter.TheirWidths.Length, Is.EqualTo(19));
            Assert.That(new List<string>(ClientReportColumns.All()),
                Does.Not.Contain(WorkbookWriter.PriorityHeading),
                "that list is theirs, re-read off their own exports on every run");
        }

        /// <summary>
        /// The stylesheet makes a column out of every smarttag, so anything of ours in the
        /// clash XML becomes a column on the page the client receives. Priority is a
        /// workbook column and must not reach the page.
        /// </summary>
        [Test]
        public void PriorityNeverReachesTheClashXml()
        {
            string path = Path.Combine(folder, "clash.xml");
            new ClashReportXml().Write(Report(true), path);
            string xml = File.ReadAllText(path);

            Assert.That(xml, Does.Not.Contain("Priority"));
            Assert.That(xml, Does.Not.Contain("priority"));
        }
    }
}
