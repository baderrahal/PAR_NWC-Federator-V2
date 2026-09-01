using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// Writes one workbook for one group, laid out exactly as the report the client
    /// receives.
    ///
    /// ONE SHEET. Every test one after another, most clashes first, the way both exports
    /// in samples\client-report are. Measured off
    /// 1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx on 2026-09-01, block by block and merge by
    /// merge.
    ///
    /// WHAT WAS REMOVED AND WHY, so a later session does not helpfully put it back. The
    /// Summary sheet, the Matrix sheet and the sheet per test were all asked for in an
    /// earlier session, before anyone had put a real Navisworks report beside ours. Now
    /// that both have been seen together the ask is one thing: our output matching theirs.
    /// Theirs has one sheet and none of those three, so ours has one sheet and none of
    /// those three. Nothing of ours is carried across because it seemed useful. If it is
    /// not in theirs it is not in ours.
    ///
    /// This lives in Core rather than in the add-in so the tests write a real xlsx and
    /// read it back without Navisworks. Nothing in here touches the Navisworks API.
    /// </summary>
    public sealed class WorkbookWriter
    {
        private readonly ReportOptions options;

        /// <summary>The defaults, which is images on.</summary>
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

        // ---------- where every cell of a block sits, measured off theirs ----------

        /// <summary>
        /// The columns, one for one with theirs. The gaps are theirs too: their Image cell
        /// spans A and B, Clash Name spans C and D, and Clash Point spans I, J and K,
        /// which is what the colspans in the stylesheet become when the page is opened in
        /// Excel.
        /// </summary>
        public const int ColumnImage = 1;          // A, merged A:B

        public const int ColumnClashName = 3;      // C, merged C:D

        public const int ColumnStatus = 5;         // E

        public const int ColumnDistance = 6;       // F

        public const int ColumnGridLocation = 7;   // G

        public const int ColumnDescription = 8;    // H

        public const int ColumnClashPoint = 9;     // I, merged I:K

        public const int ColumnItem1 = 12;         // L, then M N O

        public const int ColumnItem2 = 16;         // P, then Q R S

        /// <summary>The last column any block touches, which is S.</summary>
        public const int LastColumn = 19;

        /// <summary>The nine cells of the test header, which start in C beside the name.</summary>
        public const int ColumnTestHeader = 3;

        /// <summary>Blank rows between one block and the next, measured off theirs.</summary>
        public const int RowsBetweenBlocks = 3;

        /// <summary>
        /// The columns of a clash row, in their order and their words, so a test can
        /// assert the header without spelling it out again.
        /// </summary>
        public static readonly string[] ClientColumns = ClientFormat.ClashColumns;

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
                IXLWorksheet sheet = workbook.Worksheets.Add(SheetNames.ForReport(report.OutputName));

                int row = WriteTitle(sheet, report);

                // Most clashes first, ties in the order they were created. Theirs is
                // sorted this way and ours followed the order the tests sat in the file,
                // so it opened with empty tests.
                foreach (TestReport test in report.InReportOrder())
                {
                    row = WriteBlock(sheet, row, test);
                }

                Widths(sheet);
                workbook.SaveAs(path);
            }

            return path;
        }

        // ---------- the title row ----------

        /// <summary>
        /// Their row 1 holds the words Clash Report in D, with A to C left for the logo
        /// picture. Returns the row the first block starts on, which is their row 4.
        /// </summary>
        private static int WriteTitle(IXLWorksheet sheet, ClashReport report)
        {
            sheet.Range(1, 1, 1, 3).Merge();

            IXLCell title = sheet.Cell(1, 4);
            title.Value = TitleText;
            title.Style.Font.Bold = true;
            title.Style.Font.FontSize = 18;
            title.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.Range(1, 4, 1, LastColumn).Merge();

            return 4;
        }

        /// <summary>Their own words at the top of the page.</summary>
        public const string TitleText = "Clash Report";

        // ---------- one test block ----------

        /// <summary>
        /// One test, exactly as theirs lays it out. Returns the row the next block starts
        /// on.
        ///
        ///     start      name merged over two rows, then Tolerance to Status
        ///     start + 1  their nine values
        ///     start + 2  blank
        ///     start + 3  Item 1 and Item 2 over the two item blocks
        ///     start + 4  the thirteen column headings
        ///     start + 5  one row per clash
        ///     then three blank rows
        /// </summary>
        private int WriteBlock(IXLWorksheet sheet, int start, TestReport test)
        {
            WriteTestHeader(sheet, start, test);

            int groupRow = start + 3;
            int headerRow = start + 4;

            WriteItemGroupLabels(sheet, groupRow);
            WriteColumnHeadings(sheet, headerRow);

            int row = headerRow + 1;

            foreach (ClashRow clash in test.Rows)
            {
                WriteClashRow(sheet, row, clash);
                row++;
            }

            return row + RowsBetweenBlocks;
        }

        private static void WriteTestHeader(IXLWorksheet sheet, int start, TestReport test)
        {
            IXLCell name = sheet.Cell(start, 1);
            name.Value = test.Name;
            name.Style.Font.Bold = true;
            name.Style.Font.FontSize = 16;
            name.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            name.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.Range(start, 1, start + 1, 2).Merge();

            for (int i = 0; i < ClientFormat.TestHeader.Length; i++)
            {
                IXLCell header = sheet.Cell(start, ColumnTestHeader + i);
                header.Value = ClientFormat.TestHeader[i];
                header.Style.Font.Bold = true;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int column = ColumnTestHeader;
            int values = start + 1;

            sheet.Cell(values, column++).Value = test.ClientTolerance();
            sheet.Cell(values, column++).Value = test.RawClashes;

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                sheet.Cell(values, column++).Value = test.Tally.Of(status);
            }

            sheet.Cell(values, column++).Value = ClientFormat.TestTypeWording(test.TestTypeName);
            sheet.Cell(values, column).Value = test.StatusWord;

            for (int i = 0; i < ClientFormat.TestHeader.Length; i++)
            {
                sheet.Cell(values, ColumnTestHeader + i).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
            }
        }

        private static void WriteItemGroupLabels(IXLWorksheet sheet, int row)
        {
            sheet.Range(row, 1, row, ColumnItem1 - 1).Merge();

            Label(sheet, row, ColumnItem1, ClientFormat.ItemGroup1);
            Label(sheet, row, ColumnItem2, ClientFormat.ItemGroup2);
        }

        private static void Label(IXLWorksheet sheet, int row, int column, string text)
        {
            IXLCell cell = sheet.Cell(row, column);
            cell.Value = text;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range(row, column, row, column + ClientFormat.ItemColumns - 1).Merge();
        }

        /// <summary>
        /// The thirteen headings, in their columns. Image and Clash Name are merged over
        /// two and Clash Point over three, which is where the gaps in the column numbers
        /// come from.
        /// </summary>
        private static void WriteColumnHeadings(IXLWorksheet sheet, int row)
        {
            Heading(sheet, row, ColumnImage, "Image", 2);
            Heading(sheet, row, ColumnClashName, "Clash Name", 2);
            Heading(sheet, row, ColumnStatus, "Status", 1);
            Heading(sheet, row, ColumnDistance, "Distance", 1);
            Heading(sheet, row, ColumnGridLocation, "Grid Location", 1);
            Heading(sheet, row, ColumnDescription, "Description", 1);
            Heading(sheet, row, ColumnClashPoint, "Clash Point", 3);

            for (int side = 0; side < 2; side++)
            {
                int at = side == 0 ? ColumnItem1 : ColumnItem2;

                for (int i = 0; i < ClientFormat.ItemColumns; i++)
                {
                    Heading(sheet, row, at + i, ClientFormat.PerItemColumns[i], 1);
                }
            }
        }

        private static void Heading(IXLWorksheet sheet, int row, int column, string text, int wide)
        {
            IXLCell cell = sheet.Cell(row, column);
            cell.Value = text;
            cell.Style.Font.Bold = true;

            if (wide > 1)
            {
                sheet.Range(row, column, row, column + wide - 1).Merge();
            }
        }

        private void WriteClashRow(IXLWorksheet sheet, int row, ClashRow clash)
        {
            WriteImageCell(sheet, sheet.Cell(row, ColumnImage), clash);
            sheet.Range(row, ColumnImage, row, ColumnImage + 1).Merge();

            sheet.Cell(row, ColumnClashName).Value = clash.Name;
            sheet.Range(row, ColumnClashName, row, ColumnClashName + 1).Merge();

            sheet.Cell(row, ColumnStatus).Value = clash.Status.ToString();

            // The raw signed number, written as a NUMBER so the column still sorts, with
            // their three decimal format on it.
            IXLCell distance = sheet.Cell(row, ColumnDistance);
            distance.Value = clash.Distance;
            distance.Style.NumberFormat.Format = ClientFormat.FixedFormat;

            sheet.Cell(row, ColumnGridLocation).Value = clash.ClientGridLocation();
            sheet.Cell(row, ColumnDescription).Value = clash.Description;

            sheet.Cell(row, ColumnClashPoint).Value = clash.ClientClashPoint();
            sheet.Range(row, ColumnClashPoint, row, ColumnClashPoint + 2).Merge();

            WriteItem(sheet, row, ColumnItem1, clash.Left);
            WriteItem(sheet, row, ColumnItem2, clash.Right);
        }

        /// <summary>
        /// One item block, their four columns. Layer carries the level, which is what
        /// theirs holds in it.
        /// </summary>
        private static void WriteItem(IXLWorksheet sheet, int row, int column, ClashItem item)
        {
            sheet.Cell(row, column).Value = item.ClientId();
            sheet.Cell(row, column + 1).Value = item.Layer;
            sheet.Cell(row, column + 2).Value = item.Name;
            sheet.Cell(row, column + 3).Value = item.ItemType;
        }

        /// <summary>
        /// The Image cell. A link to the jpg beside the workbook, and the picture itself
        /// only when that was asked for.
        ///
        /// Neither their workbook nor ours embeds a picture. The native export never does.
        /// The page and its _files folder together is what gets sent.
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

        /// <summary>
        /// Column widths off theirs, measured on
        /// 1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx, so a side by side comparison lines up.
        /// </summary>
        private static void Widths(IXLWorksheet sheet)
        {
            double[] widths =
            {
                26.66, 26.66, 9.44, 7.44, 6.22, 8.33, 12.33, 18.33, 9.33, 19.66,
                6.55, 18.78, 10.11, 26.66, 9.44, 18.78, 10.11, 26.66, 9.44
            };

            for (int i = 0; i < widths.Length && i < LastColumn; i++)
            {
                sheet.Column(i + 1).Width = widths[i];
            }
        }

        /// <summary>
        /// Every sheet name the workbook would use, so a test can check them without
        /// writing a file. One now, where there used to be one per test plus two.
        /// </summary>
        public static IList<string> SheetNamesFor(ClashReport report)
        {
            return new List<string> { SheetNames.ForReport(report.OutputName) };
        }
    }
}
