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
        /// The order the blocks are written in. With no file picked it is most clashes
        /// first, which is measured. With one picked the A test goes first even though it
        /// holds fewer clashes, which is the point.
        /// </summary>
        [Test]
        public void PickingAFileMovesTheABlockToTheTop()
        {
            Assert.That(ReportOrder.Tests(Report(false))[0].Name,
                Is.EqualTo("BLD-AR-Walls-vs-BLD-AR-Columns"), "six clashes beats two");
            Assert.That(ReportOrder.Tests(Report(true))[0].Name,
                Is.EqualTo("BLD-ME-Ducts-vs-BLD-AR-Walls"), "A beats no priority");
        }

        /// <summary>
        /// The break. A priority sorted workbook is in priority order on purpose, and the
        /// order check would call every one of them wrongly ordered, which would be the
        /// headline of the block on every run that picked a file.
        /// </summary>
        [Test]
        public void ThePriorityOrderIsNotReportedAsTheWrongOrder()
        {
            string path = Write(true);

            WorkbookCheck told = WorkbookCheck.Of(path, true);
            WorkbookCheck notTold = WorkbookCheck.Of(path, false);

            Assert.That(told.Passed, Is.True,
                string.Join(" ", new List<string>(told.Problems).ToArray()));
            Assert.That(notTold.Passed, Is.False,
                "a check that was not told still catches a block order it did not expect");
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
