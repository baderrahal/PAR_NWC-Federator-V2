using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// Writes one workbook for one group.
    ///
    /// This lives in Core rather than in the add-in so the tests write a real xlsx and
    /// read it back without Navisworks. Nothing in here touches the Navisworks API.
    /// </summary>
    public sealed class WorkbookWriter
    {
        /// <summary>The columns of a test sheet, in the order the brief sets out.</summary>
        public static readonly string[] TestColumns =
        {
            "Group or clash",
            "Status",
            "Distance",
            "Grid",
            "Level",
            "A item", "A family", "A type", "A material", "A source file", "A discipline",
            "B item", "B family", "B type", "B material", "B source file", "B discipline",
            "Found", "Position",
            "A element id", "B element id",
            "Raw clashes"
        };

        public static readonly string[] SummaryColumns =
        {
            "Test", "Sheet", "Test name", "Type",
            "A set", "B set", "A items", "B items",
            "Tolerance", "Units",
            "Outcome", "Why skipped",
            "Groups", "Raw clashes",
            "New", "Active", "Reviewed", "Approved", "Resolved",
            "Open", "Resolved so far", "Seconds"
        };

        /// <summary>
        /// Writes the workbook and returns the path. Overwrites, the same as the NWF and
        /// the NWD. The caller reads the size back off the disk, this never reports one.
        /// </summary>
        public string Write(ClashReport report, string path)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A workbook needs a path.", "path");
            }

            string folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                WriteSummary(workbook, report);
                WriteMatrix(workbook, report);

                foreach (TestReport test in report.Tests)
                {
                    if (test.HasSheet)
                    {
                        WriteTest(workbook, test);
                    }
                }

                workbook.SaveAs(path);
            }

            return path;
        }

        // ---------- the Summary sheet ----------

        /// <summary>
        /// One row per test in the file, not just the ones that ran, so skipped, passed
        /// and found are three separate numbers that can be read off one sheet.
        /// </summary>
        private void WriteSummary(XLWorkbook workbook, ClashReport report)
        {
            IXLWorksheet sheet = workbook.Worksheets.Add(SheetNames.SummarySheet);
            int row = 1;

            sheet.Cell(row, 1).Value = "Clash report, building " + report.Building;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            row = Fact(sheet, row, "Ran against", report.OpenDocument);
            row = Fact(sheet, row, "Clash file", report.SourceFile);
            row = Fact(sheet, row, "Document units", report.DocumentUnits);
            row = Fact(sheet, row, "Run at", report.RunAt.ToString(
                "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            row = Fact(sheet, row, "Build", report.BuildStamp);
            row = Fact(sheet, row, "Clash step seconds",
                report.ClashStepSeconds.ToString("0.0", CultureInfo.InvariantCulture));

            row++;
            row = Fact(sheet, row, "Tests in the file", report.Tests.Count.ToString());
            row = Fact(sheet, row, "Ran", report.RanCount.ToString());
            row = Fact(sheet, row, "Passed, ran and found nothing",
                report.CountOf(TestState.Passed).ToString());
            row = Fact(sheet, row, "Found clashes",
                report.CountOf(TestState.FoundClashes).ToString());
            row = Fact(sheet, row, "Skipped, not run and not passed",
                report.CountOf(TestState.Skipped).ToString());
            row = Fact(sheet, row, "Raw clashes", report.TotalRawClashes.ToString());

            ClashTally totals = report.Totals;

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                row = Fact(sheet, row, status.ToString(), totals.Of(status).ToString());
            }

            row = Fact(sheet, row, "Open, counted as " + OpenClashes.Heading(report.OpenCount),
                OpenClashes.Of(totals, report.OpenCount).ToString());
            row = Fact(sheet, row, "Resolved, which stay in the file and keep counting",
                totals.Of(ClashStatus.Resolved).ToString());

            if (report.CompactedAway >= 0)
            {
                row = Fact(sheet, row, "Removed by compacting this run",
                    report.CompactedAway.ToString());
            }

            row += 2;
            int headerRow = row;

            for (int i = 0; i < SummaryColumns.Length; i++)
            {
                sheet.Cell(headerRow, i + 1).Value = SummaryColumns[i];
                sheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
            }

            row++;

            foreach (TestReport test in report.Tests)
            {
                int column = 1;

                sheet.Cell(row, column++).Value = test.Number;

                // A link only where there is a sheet to link to. A link to a sheet that
                // does not exist opens an error box rather than a sheet.
                IXLCell sheetCell = sheet.Cell(row, column++);

                if (test.HasSheet)
                {
                    sheetCell.Value = test.SheetName();
                    sheetCell.SetHyperlink(new XLHyperlink("'" + test.SheetName() + "'!A1"));
                }

                // The full name, which is why this column exists. 1703 of the 1830 names
                // in the reference file are longer than a sheet name can be.
                sheet.Cell(row, column++).Value = test.Name;
                sheet.Cell(row, column++).Value = test.TestTypeName;
                sheet.Cell(row, column++).Value = ClashReport.SetNameOf(test.LeftLocator);
                sheet.Cell(row, column++).Value = ClashReport.SetNameOf(test.RightLocator);
                sheet.Cell(row, column++).Value = test.LeftItems;
                sheet.Cell(row, column++).Value = test.RightItems;
                sheet.Cell(row, column++).Value = test.Tolerance;
                sheet.Cell(row, column++).Value = test.ToleranceUnits;
                sheet.Cell(row, column++).Value = test.DescribeState();
                sheet.Cell(row, column++).Value = test.SkippedReason;
                sheet.Cell(row, column++).Value = test.GroupCount;
                sheet.Cell(row, column++).Value = test.RawClashes;

                foreach (ClashStatus status in ClashTally.AllStatuses)
                {
                    sheet.Cell(row, column++).Value = test.Tally.Of(status);
                }

                sheet.Cell(row, column++).Value = test.OpenUnder(report.OpenCount);
                sheet.Cell(row, column++).Value = test.Resolved;
                sheet.Cell(row, column).Value = test.Seconds;

                row++;
            }

            if (report.Tests.Count > 0)
            {
                sheet.Range(headerRow, 1, headerRow + report.Tests.Count, SummaryColumns.Length)
                    .SetAutoFilter();
                sheet.SheetView.FreezeRows(headerRow);
            }

            sheet.Columns().AdjustToContents(1, 60);
        }

        private static int Fact(IXLWorksheet sheet, int row, string name, string value)
        {
            sheet.Cell(row, 1).Value = name;
            sheet.Cell(row, 2).Value = value ?? string.Empty;
            return row + 1;
        }

        // ---------- the Matrix sheet ----------

        /// <summary>
        /// Counts per pair, sides down and across. The cell holds New plus Active. A pair
        /// whose test was skipped says so rather than showing a zero, because a zero reads
        /// as coordinated and a skip reads as nobody looked.
        /// </summary>
        private void WriteMatrix(XLWorkbook workbook, ClashReport report)
        {
            IXLWorksheet sheet = workbook.Worksheets.Add(SheetNames.MatrixSheet);
            ClashMatrix matrix = ClashMatrix.From(report);

            sheet.Cell(1, 1).Value = OpenClashes.Heading(report.OpenCount)
                + " per pair, building " + report.Building;
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(2, 1).Value = OpenClashes.SheetLabel(report.OpenCount);
            sheet.Cell(3, 1).Value =
                "A cell reads \"" + MatrixCell.Skipped + "\" where the test did not run. That is not a "
                + "zero. A zero means the pair was tested and nothing clashed. The disciplines come "
                + "from the folder names in the picked file, never from a list in this tool.";

            int top = 5;
            int size = matrix.Size;

            // Two header rows, the discipline then the set, both read off the file.
            for (int i = 0; i < size; i++)
                {
                string locator = matrix.Locators[i];
                sheet.Cell(top, 3 + i).Value = matrix.DisciplineOf(locator);
                sheet.Cell(top, 3 + i).Style.Font.Bold = true;
                sheet.Cell(top + 1, 3 + i).Value = ClashMatrix.SetNameOf(locator);
                sheet.Cell(top + 1, 3 + i).Style.Alignment.TextRotation = 90;
            }

            for (int down = 0; down < size; down++)
            {
                string locator = matrix.Locators[down];
                int row = top + 2 + down;

                sheet.Cell(row, 1).Value = matrix.DisciplineOf(locator);
                sheet.Cell(row, 1).Style.Font.Bold = true;
                sheet.Cell(row, 2).Value = ClashMatrix.SetNameOf(locator);

                for (int across = 0; across < size; across++)
                {
                    if (down == across)
                    {
                        // A set never clashes against itself here, because the file pairs
                        // 61 sets with no self pairs.
                        sheet.Cell(row, 3 + across).Style.Fill.BackgroundColor = XLColor.LightGray;
                        continue;
                    }

                    MatrixCell cell = matrix.At(locator, matrix.Locators[across]);
                    IXLCell target = sheet.Cell(row, 3 + across);

                    switch (cell.Kind)
                    {
                        case MatrixCellKind.Ran:
                            target.Value = cell.NewPlusActive;
                            break;
                        case MatrixCellKind.Skipped:
                            target.Value = MatrixCell.Skipped;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (size > 0)
            {
                sheet.SheetView.Freeze(top + 1, 2);
            }

            sheet.Columns(1, 2).AdjustToContents(1, 40);
        }

        // ---------- one sheet per test that found something ----------

        private void WriteTest(XLWorkbook workbook, TestReport test)
        {
            // Numbered, never named after the test, because Excel stops at 31 characters.
            // Sanitised as the floor rather than as the plan, so a name that somehow
            // reached here cannot throw and lose the whole workbook.
            IXLWorksheet sheet = workbook.Worksheets.Add(
                SheetNames.Sanitise(test.SheetName(), "T" + test.Number));

            sheet.Cell(1, 1).Value = test.Name;
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(2, 1).Value = ClashReport.SetNameOf(test.LeftLocator)
                + "  against  " + ClashReport.SetNameOf(test.RightLocator)
                + "   items " + test.LeftItems + " v " + test.RightItems
                + "   tolerance " + test.Tolerance.ToString("0.####", CultureInfo.InvariantCulture)
                + " " + test.ToleranceUnits;
            sheet.Cell(3, 1).Value = test.GroupCount + " rows covering " + test.RawClashes
                + " raw clashes. A group is one row and carries the count behind it.";

            IXLCell back = sheet.Cell(4, 1);
            back.Value = "Back to " + SheetNames.SummarySheet;
            back.SetHyperlink(new XLHyperlink("'" + SheetNames.SummarySheet + "'!A1"));

            const int headerRow = 6;

            for (int i = 0; i < TestColumns.Length; i++)
            {
                sheet.Cell(headerRow, i + 1).Value = TestColumns[i];
                sheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
            }

            int row = headerRow + 1;

            foreach (ClashRow clash in test.Rows)
            {
                int column = 1;

                sheet.Cell(row, column++).Value = clash.Name;
                sheet.Cell(row, column++).Value = clash.Status.ToString();
                sheet.Cell(row, column++).Value = clash.Distance;
                sheet.Cell(row, column++).Value = clash.GridLocation;
                sheet.Cell(row, column++).Value = clash.Level;

                column = WriteItem(sheet, row, column, clash.Left);
                column = WriteItem(sheet, row, column, clash.Right);

                if (clash.Found.HasValue)
                {
                    sheet.Cell(row, column).Value = clash.Found.Value;
                    sheet.Cell(row, column).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                }

                column++;

                sheet.Cell(row, column++).Value = clash.Position();
                sheet.Cell(row, column++).Value = clash.Left.ElementId;
                sheet.Cell(row, column++).Value = clash.Right.ElementId;

                // Never hidden by the grouping. One for a plain clash, the count inside
                // the group otherwise.
                sheet.Cell(row, column).Value = clash.RawClashes;

                row++;
            }

            if (test.Rows.Count > 0)
            {
                sheet.Range(headerRow, 1, headerRow + test.Rows.Count, TestColumns.Length)
                    .SetAutoFilter();
                sheet.SheetView.FreezeRows(headerRow);
            }

            sheet.Columns().AdjustToContents(1, 45);
        }

        private static int WriteItem(IXLWorksheet sheet, int row, int column, ClashItem item)
        {
            sheet.Cell(row, column++).Value = item.Name;
            sheet.Cell(row, column++).Value = item.Family;
            sheet.Cell(row, column++).Value = item.Type;
            sheet.Cell(row, column++).Value = item.Material;
            sheet.Cell(row, column++).Value = item.SourceFile;
            sheet.Cell(row, column++).Value = item.Discipline;
            return column;
        }

        /// <summary>
        /// Every sheet name the workbook would use, so a test can check them without
        /// writing a file.
        /// </summary>
        public static IList<string> SheetNamesFor(ClashReport report)
        {
            List<string> names = new List<string> { SheetNames.SummarySheet, SheetNames.MatrixSheet };

            foreach (TestReport test in report.Tests)
            {
                if (test.HasSheet)
                {
                    names.Add(SheetNames.Sanitise(test.SheetName(), "T" + test.Number));
                }
            }

            return names;
        }
    }
}
