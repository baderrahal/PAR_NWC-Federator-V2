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
        private readonly ReportOptions options;

        /// <summary>The defaults, which is every column and images on.</summary>
        public WorkbookWriter()
            : this(null)
        {
        }

        public WorkbookWriter(ReportOptions options)
        {
            this.options = options ?? new ReportOptions();
        }

        /// <summary>What this writer was told to do. Never null.</summary>
        public ReportOptions Options
        {
            get { return options; }
        }

        /// <summary>
        /// The columns of a test sheet, which are the client's own columns in the client's
        /// own order and words, taken from the report they have already accepted. See
        /// <see cref="ClientFormat"/> for where each name and each format was read from.
        /// </summary>
        public static readonly string[] ClientColumns = ClientFormat.ClashColumns;

        /// <summary>
        /// Ours, and they come AFTER theirs rather than in place of any of them. Without
        /// these, whoever has to fix a clash opens the model to find out what they are
        /// looking at.
        ///
        /// Type Name rather than Type, because the client already has a column called
        /// Item Type and it holds something else. Theirs is the Navisworks item type and
        /// reads Solid. Ours is the type name off the model.
        /// </summary>
        public static readonly string[] OurColumns =
        {
            "Item 1 Family", "Item 1 Type Name", "Item 1 Material",
            "Item 1 Source File", "Item 1 Discipline",
            "Item 2 Family", "Item 2 Type Name", "Item 2 Material",
            "Item 2 Source File", "Item 2 Discipline",
            "Found", "Raw clashes"
        };

        /// <summary>
        /// The whole header of a test sheet. Client columns always, ours only when they
        /// were asked for, so a submission can be exported with exactly the columns that
        /// were signed off.
        /// </summary>
        public static string[] TestColumnsFor(bool clientColumnsOnly)
        {
            if (clientColumnsOnly)
            {
                return (string[])ClientColumns.Clone();
            }

            List<string> all = new List<string>(ClientColumns);
            all.AddRange(OurColumns);
            return all.ToArray();
        }

        /// <summary>Every column, which is what a workbook written with the defaults holds.</summary>
        public static readonly string[] TestColumns = TestColumnsFor(false);

        public static readonly string[] SummaryColumns =
        {
            "Test", "Sheet", "Test name", "Type",
            "A set", "B set", "A items", "B items",
            "Tolerance", "Units",
            "Outcome", "Why skipped",
            "Groups", "Raw clashes",
            "New", "Active", "Reviewed", "Approved", "Resolved",
            "Open", "Resolved so far", "Seconds", "Images"
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

            row++;
            row = Fact(sheet, row, "Images", options.Images.Describe());
            row = Fact(sheet, row, "Images written", report.Images.Written.ToString());
            row = Fact(sheet, row, "Images, megabytes",
                report.Images.TotalMegabytes.ToString("0.00", CultureInfo.InvariantCulture));
            row = Fact(sheet, row, "Images, seconds in total",
                report.Images.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture));
            row = Fact(sheet, row, "Images, seconds each",
                report.Images.SecondsEach.ToString("0.000", CultureInfo.InvariantCulture));
            row = Fact(sheet, row, "Images that failed", report.Images.Failed.ToString());
            row = Fact(sheet, row, "Columns",
                options.ClientColumnsOnly
                    ? "the client's columns only, as the accepted report has them"
                    : "the client's columns, then ours after them");

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
                sheet.Cell(row, column++).Value = ClientFormat.TestTypeWording(test.TestTypeName);
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
                sheet.Cell(row, column++).Value = test.Seconds;
                sheet.Cell(row, column).Value = test.ImageCount;

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

            int row = WriteTestHeader(sheet, test);

            // Client columns only means nothing of ours on the sheet at all, not just no
            // extra columns. The explanatory lines, the way back to the Summary and the
            // filter arrows are all ours, and a submission should look like the report
            // that was signed off and nothing else.
            if (!options.ClientColumnsOnly)
            {
                row = WriteOurContext(sheet, row, test);
            }

            WriteClashTable(sheet, row, test);

            sheet.Columns().AdjustToContents(1, 45);
        }

        /// <summary>
        /// The client's own test header, their nine columns in their order and their
        /// words, with the test name beside them the way their report has it.
        /// </summary>
        private int WriteTestHeader(IXLWorksheet sheet, TestReport test)
        {
            const int nameRow = 1;
            const int valueRow = 2;

            sheet.Cell(nameRow, 1).Value = test.Name;
            sheet.Cell(nameRow, 1).Style.Font.Bold = true;
            sheet.Range(nameRow, 1, valueRow, 1).Merge();

            for (int i = 0; i < ClientFormat.TestHeader.Length; i++)
            {
                IXLCell header = sheet.Cell(nameRow, i + 2);
                header.Value = ClientFormat.TestHeader[i];
                header.Style.Font.Bold = true;
            }

            int column = 2;

            sheet.Cell(valueRow, column++).Value = test.ClientTolerance();
            sheet.Cell(valueRow, column++).Value = test.RawClashes;

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                sheet.Cell(valueRow, column++).Value = test.Tally.Of(status);
            }

            // Their words, "Hard (Conservative)", not the file's token
            // "hard_conservative" which is what a real run wrote into this cell.
            sheet.Cell(valueRow, column++).Value = ClientFormat.TestTypeWording(test.TestTypeName);

            // Their Status column. This carries whatever the run read off the test and is
            // never translated. Theirs says OK, which is not a value on the API's enum, so
            // no mapping is invented here. See docs\scan.md section 4j.
            sheet.Cell(valueRow, column).Value = test.StatusWord;

            return valueRow + 2;
        }

        /// <summary>
        /// Ours, kept off their block and out of their columns. The sets, the counts and
        /// the way back to the Summary.
        /// </summary>
        private int WriteOurContext(IXLWorksheet sheet, int row, TestReport test)
        {
            sheet.Cell(row, 1).Value = ClashReport.SetNameOf(test.LeftLocator)
                + "  against  " + ClashReport.SetNameOf(test.RightLocator)
                + "   items " + test.LeftItems + " v " + test.RightItems
                + "   " + test.GroupCount + " rows covering " + test.RawClashes
                + " raw clashes, a group being one row carrying the count behind it.";
            row++;

            if (test.ImageCount > 0)
            {
                sheet.Cell(row, 1).Value = test.ImageCount
                    + " rows carry a picture, in the folder beside this workbook. A cell "
                    + "with no link is a clash no picture was asked for or one whose "
                    + "picture failed.";
                row++;
            }

            IXLCell back = sheet.Cell(row, 1);
            back.Value = "Back to " + SheetNames.SummarySheet;
            back.SetHyperlink(new XLHyperlink("'" + SheetNames.SummarySheet + "'!A1"));

            return row + 2;
        }

        private void WriteClashTable(IXLWorksheet sheet, int top, TestReport test)
        {
            string[] columns = TestColumnsFor(options.ClientColumnsOnly);

            // Their merged Item 1 and Item 2 labels, sitting over the two blocks of three
            // exactly where their report has them.
            int firstItem = ClientFormat.FirstItemColumn + 1;
            int secondItem = firstItem + ClientFormat.ItemColumns;

            WriteItemGroupLabel(sheet, top, firstItem, ClientFormat.ItemGroup1);
            WriteItemGroupLabel(sheet, top, secondItem, ClientFormat.ItemGroup2);

            int headerRow = top + 1;

            for (int i = 0; i < columns.Length; i++)
            {
                IXLCell header = sheet.Cell(headerRow, i + 1);
                header.Value = columns[i];
                header.Style.Font.Bold = true;
            }

            int row = headerRow + 1;

            foreach (ClashRow clash in test.Rows)
            {
                WriteClashRow(sheet, row, clash);
                row++;
            }

            if (test.Rows.Count == 0)
            {
                return;
            }

            // A filter arrow on every header is ours. Theirs has none, so a submission
            // gets none either.
            if (!options.ClientColumnsOnly)
            {
                sheet.Range(headerRow, 1, headerRow + test.Rows.Count, columns.Length)
                    .SetAutoFilter();
            }

            sheet.SheetView.FreezeRows(headerRow);
        }

        private static void WriteItemGroupLabel(IXLWorksheet sheet, int row, int column, string label)
        {
            IXLCell cell = sheet.Cell(row, column);
            cell.Value = label;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range(row, column, row, column + ClientFormat.ItemColumns - 1).Merge();
        }

        private void WriteClashRow(IXLWorksheet sheet, int row, ClashRow clash)
        {
            int column = 1;

            WriteImageCell(sheet, sheet.Cell(row, column++), clash);

            sheet.Cell(row, column++).Value = clash.Name;
            sheet.Cell(row, column++).Value = clash.Status.ToString();

            // The raw signed number, negative on a hard clash, written as a NUMBER so it
            // sorts and filters, and given their three decimal format so it reads the way
            // theirs does. Without the format Excel shows the whole double and a real run
            // printed -0.328083992004395 where theirs says -0.050.
            IXLCell distance = sheet.Cell(row, column++);
            distance.Value = clash.Distance;
            distance.Style.NumberFormat.Format = ClientFormat.DistanceFormat;

            sheet.Cell(row, column++).Value = clash.ClientGridLocation();
            sheet.Cell(row, column++).Value = clash.Description;
            sheet.Cell(row, column++).Value = clash.ClientClashPoint();

            column = WriteClientItem(sheet, row, column, clash.Left);
            column = WriteClientItem(sheet, row, column, clash.Right);

            if (options.ClientColumnsOnly)
            {
                return;
            }

            column = WriteOurItem(sheet, row, column, clash.Left);
            column = WriteOurItem(sheet, row, column, clash.Right);

            if (clash.Found.HasValue)
            {
                sheet.Cell(row, column).Value = clash.Found.Value;
                sheet.Cell(row, column).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            }

            column++;

            // Never hidden by the grouping. One for a plain clash, the count inside the
            // group otherwise.
            sheet.Cell(row, column).Value = clash.RawClashes;
        }

        /// <summary>
        /// The Image cell. A link to the jpg beside the workbook, and the picture itself
        /// only when that was asked for, because the accepted report links rather than
        /// pasting and a pasted picture makes the file many times larger.
        ///
        /// A clash with no picture leaves the cell empty. That is the ordinary answer for
        /// a status nobody asked for a picture of, and it is also what a failed render
        /// leaves behind, which is why the failures are counted in the log instead.
        /// </summary>
        private void WriteImageCell(IXLWorksheet sheet, IXLCell cell, ClashRow clash)
        {
            if (!clash.HasImage)
            {
                return;
            }

            cell.Value = clash.ImageFile;

            if (!string.IsNullOrEmpty(clash.ImageLink))
            {
                // A relative Uri, not a plain string. Handed the string, ClosedXML reads
                // it as an internal address, which makes the cell jump to a sheet that
                // does not exist instead of opening the picture.
                cell.SetHyperlink(new XLHyperlink(new Uri(clash.ImageLink, UriKind.Relative)));
            }

            if (!options.Images.EmbedThumbnail || string.IsNullOrEmpty(clash.ImagePath))
            {
                return;
            }

            try
            {
                if (!File.Exists(clash.ImagePath))
                {
                    return;
                }

                sheet.AddPicture(clash.ImagePath)
                    .MoveTo(cell)
                    .WithSize(ThumbnailPixels, ThumbnailPixels);

                sheet.Row(cell.Address.RowNumber).Height = ThumbnailPoints;
                sheet.Column(cell.Address.ColumnNumber).Width = ThumbnailWidth;
            }
            catch (Exception)
            {
                // A picture that will not embed must not lose the workbook. The link is
                // already on the cell, so the row still reaches its image.
            }
        }

        /// <summary>The size a pasted thumbnail is drawn at, matching their 95 pixel one.</summary>
        private const int ThumbnailPixels = 95;

        private const double ThumbnailPoints = 72.0;

        private const double ThumbnailWidth = 14.0;

        private static int WriteClientItem(IXLWorksheet sheet, int row, int column, ClashItem item)
        {
            sheet.Cell(row, column++).Value = item.ClientId();
            sheet.Cell(row, column++).Value = item.Name;
            sheet.Cell(row, column++).Value = item.ItemType;
            return column;
        }

        private static int WriteOurItem(IXLWorksheet sheet, int row, int column, ClashItem item)
        {
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
