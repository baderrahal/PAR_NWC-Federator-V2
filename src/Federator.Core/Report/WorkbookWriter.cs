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

                // Theirs, measured. ClosedXML's own defaults are a different four numbers.
                sheet.PageSetup.Margins.Top = 1.0;
                sheet.PageSetup.Margins.Bottom = 1.0;
                sheet.PageSetup.Margins.Left = 0.75;
                sheet.PageSetup.Margins.Right = 0.75;
                sheet.PageSetup.Margins.Header = 0.5;
                sheet.PageSetup.Margins.Footer = 0.5;

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

            // 45, measured off their row 1, which is where the logo picture sits.
            sheet.Row(1).Height = TitleRowHeight;

            IXLCell title = sheet.Cell(1, 4);
            title.Value = TitleText;
            title.Style.Font.Bold = true;
            title.Style.Font.FontSize = 18;
            ClientStyle.Cell(title.Style, XLAlignmentVerticalValues.Center);
            sheet.Range(1, 4, 1, LastColumn).Merge();

            return 4;
        }

        /// <summary>Their own words at the top of the page.</summary>
        public const string TitleText = "Clash Report";

        /// <summary>Measured off their row 1, which carries the logo.</summary>
        public const double TitleRowHeight = 45.0;

        /// <summary>Measured off every clash row of theirs, so a picture fits.</summary>
        public const double ClashRowHeight = 60.0;

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

            sheet.Row(start + 2).Height = ClientStyle.GapRowHeight;

            WriteItemGroupLabels(sheet, groupRow);
            WriteColumnHeadings(sheet, headerRow);

            int row = headerRow + 1;

            foreach (ClashRow clash in test.Rows)
            {
                WriteClashRow(sheet, row, clash);

                // 60, measured off every clash row of theirs, so a picture fits rather
                // than being squashed into a default row.
                sheet.Row(row).Height = ClashRowHeight;
                row++;
            }

            return row + RowsBetweenBlocks;
        }

        private static void WriteTestHeader(IXLWorksheet sheet, int start, TestReport test)
        {
            // Grey, boxed thick outside and medium within. Theirs, measured.
            ClientStyle.TestHeader(sheet, start, ColumnTestHeader - 1, LastTestHeaderColumn);
            sheet.Row(start).Height = ClientStyle.TestHeaderRowHeight;
            sheet.Row(start + 1).Height = ClientStyle.TestValuesRowHeight;

            IXLCell name = sheet.Cell(start, 1);
            name.Value = test.Name;
            name.Style.Font.Bold = true;
            name.Style.Font.FontSize = 16;
            name.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range(start, 1, start + 1, 2).Merge();

            for (int i = 0; i < ClientFormat.TestHeader.Length; i++)
            {
                IXLCell header = sheet.Cell(start, ColumnTestHeader + i);
                header.Value = ClientFormat.TestHeader[i];
                header.Style.Font.Bold = true;
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
            sheet.Cell(values, column).Value = ClientFormat.StatusWording(test.StatusWord);

        }

        /// <summary>
        /// The last column the test header table reaches, which is Status. Their table is
        /// nine columns wide and stops there, so the thick right edge is on it and not on
        /// the far side of the sheet.
        /// </summary>
        public static readonly int LastTestHeaderColumn =
            ColumnTestHeader + ClientFormat.TestHeader.Length - 1;

        private static void WriteItemGroupLabels(IXLWorksheet sheet, int row)
        {
            sheet.Row(row).Height = ClientStyle.HeadingRowHeight;

            // Three runs, each boxed as one merged cell is: grey over the clash columns,
            // blue over Item 1 and pink over Item 2. Theirs, measured.
            Paint(sheet, row, 1, ColumnItem1 - 1, ClientStyle.HeaderGrey,
                XLAlignmentHorizontalValues.General);
            sheet.Range(row, 1, row, ColumnItem1 - 1).Merge();

            Label(sheet, row, ColumnItem1, ClientFormat.ItemGroup1, ClientStyle.Item1Heading);
            Label(sheet, row, ColumnItem2, ClientFormat.ItemGroup2, ClientStyle.Item2Heading);
        }

        private static void Label(
            IXLWorksheet sheet, int row, int column, string text, string colour)
        {
            int last = column + ClientFormat.ItemColumns - 1;

            Paint(sheet, row, column, last, colour, XLAlignmentHorizontalValues.Center);

            IXLCell cell = sheet.Cell(row, column);
            cell.Value = text;
            cell.Style.Font.Bold = true;
            sheet.Range(row, column, row, last).Merge();
        }

        /// <summary>
        /// One run of a heading row: filled, boxed medium, centred down and wrapping,
        /// which is what every heading cell of theirs carries.
        /// </summary>
        private static void Paint(
            IXLWorksheet sheet, int row, int first, int last, string colour,
            XLAlignmentHorizontalValues across)
        {
            ClientStyle.Fill(sheet, row, first, last, colour);
            ClientStyle.Box(sheet, row, first, last, XLBorderStyleValues.Medium);

            for (int column = first; column <= last; column++)
            {
                IXLStyle style = sheet.Cell(row, column).Style;
                ClientStyle.Cell(style, XLAlignmentVerticalValues.Center);

                if (across != XLAlignmentHorizontalValues.General)
                {
                    style.Alignment.Horizontal = across;
                }
            }
        }

        /// <summary>
        /// Where the merges fall along a table row, as first and last column pairs. Their
        /// borders follow these runs, so the runs are stated once and both the heading row
        /// and every clash row are ruled off the same list.
        /// </summary>
        public static readonly int[][] Runs = BuildRuns();

        private static int[][] BuildRuns()
        {
            List<int[]> runs = new List<int[]>();

            runs.Add(new[] { ColumnImage, ColumnImage + 1 });
            runs.Add(new[] { ColumnClashName, ColumnClashName + 1 });
            runs.Add(new[] { ColumnStatus, ColumnStatus });
            runs.Add(new[] { ColumnDistance, ColumnDistance });
            runs.Add(new[] { ColumnGridLocation, ColumnGridLocation });
            runs.Add(new[] { ColumnDescription, ColumnDescription });
            runs.Add(new[] { ColumnClashPoint, ColumnClashPoint + 2 });

            for (int column = ColumnItem1; column <= LastColumn; column++)
            {
                runs.Add(new[] { column, column });
            }

            return runs.ToArray();
        }

        /// <summary>Boxes every run of one table row medium, the way theirs is.</summary>
        private static void BoxTheRuns(IXLWorksheet sheet, int row)
        {
            foreach (int[] run in Runs)
            {
                ClientStyle.Box(sheet, row, run[0], run[1], XLBorderStyleValues.Medium);
            }

            for (int column = 1; column <= LastColumn; column++)
            {
                ClientStyle.Cell(sheet.Cell(row, column).Style,
                    XLAlignmentVerticalValues.Center);
            }
        }

        /// <summary>
        /// The thirteen headings, in their columns. Image and Clash Name are merged over
        /// two and Clash Point over three, which is where the gaps in the column numbers
        /// come from.
        /// </summary>
        private static void WriteColumnHeadings(IXLWorksheet sheet, int row)
        {
            sheet.Row(row).Height = ClientStyle.HeadingRowHeight;

            ClientStyle.Fill(sheet, row, 1, ColumnItem1 - 1, ClientStyle.HeaderGrey);
            ClientStyle.Fill(sheet, row, ColumnItem1, ColumnItem2 - 1, ClientStyle.Item1Heading);
            ClientStyle.Fill(sheet, row, ColumnItem2, LastColumn, ClientStyle.Item2Heading);
            BoxTheRuns(sheet, row);

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
            // The two item blocks are tinted and every run is boxed, which is what makes
            // theirs readable across nineteen columns. Painted before the values so a cell
            // that is never given a value still carries the block it belongs to.
            ClientStyle.Fill(sheet, row, ColumnItem1, ColumnItem2 - 1, ClientStyle.Item1Body);
            ClientStyle.Fill(sheet, row, ColumnItem2, LastColumn, ClientStyle.Item2Body);
            BoxTheRuns(sheet, row);

            WriteImageCell(sheet, sheet.Cell(row, ColumnImage), clash);
            sheet.Range(row, ColumnImage, row, ColumnImage + 1).Merge();

            sheet.Cell(row, ColumnClashName).Value = clash.Name;
            sheet.Range(row, ColumnClashName, row, ColumnClashName + 1).Merge();

            sheet.Cell(row, ColumnStatus).Value = clash.Status.ToString();

            // The ROUNDED number itself, not the raw one behind a display format. Ours
            // stored -1.70603561401367 with a format of 0.000, so anyone sorting,
            // filtering or copying got the long value. Theirs stores -0.116 and carries
            // no number format at all, so General shows it as written.
            sheet.Cell(row, ColumnDistance).Value = ClientFormat.Rounded(clash.Distance);

            sheet.Cell(row, ColumnGridLocation).Value = clash.ClientGridLocation();
            sheet.Cell(row, ColumnDescription).Value = clash.Description;

            sheet.Cell(row, ColumnClashPoint).Value = clash.ClientClashPoint();
            sheet.Range(row, ColumnClashPoint, row, ColumnClashPoint + 2).Merge();

            // The level is passed in because the page writes it into its own layer element
            // and the workbook was writing an item property nothing ever filled, so the
            // same row read LGF in one file and nothing in the other.
            WriteItem(sheet, row, ColumnItem1, clash.Left, clash.Level);
            WriteItem(sheet, row, ColumnItem2, clash.Right, clash.Level);
        }

        /// <summary>
        /// One item block, their four columns. Layer carries the level, which is what
        /// theirs holds in it.
        /// </summary>
        private static void WriteItem(
            IXLWorksheet sheet, int row, int column, ClashItem item, string clashLevel)
        {
            sheet.Cell(row, column).Value = item.ClientId();
            sheet.Cell(row, column + 1).Value = item.LayerOr(clashLevel);
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

            // NO filename in the cell. Theirs is empty here and the picture is attached
            // as a linked drawing, measured on their xl/drawings/drawing1.xml which holds
            // no media of its own. A filename sitting in the cell is neither their layout
            // nor a picture, so the cell carries only the link.
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
        /// Their column widths, read out of the sheet XML of
        /// 1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx on 2026-09-01, to the digit. They are
        /// not round numbers because Excel fitted them to that file's own content.
        /// </summary>
        public static readonly double[] TheirWidths =
        {
            26.6640625, 26.6640625, 9.44140625, 7.44140625, 6.21875, 8.33203125,
            12.33203125, 18.33203125, 9.33203125, 19.6640625, 6.5546875, 18.77734375,
            10.109375, 35.5546875, 9.5546875, 18.77734375, 5.5546875, 35.5546875, 9.5546875
        };

        /// <summary>
        /// What ClosedXML adds to a column width on save. Measured: every width we set
        /// came back exactly 0.710625 larger, which is the whole of the difference Bader
        /// saw between our columns and theirs.
        /// </summary>
        public const double ClosedXmlWidthPadding = 0.710625;

        /// <summary>
        /// Column widths off theirs, measured on
        /// 1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx, so a side by side comparison lines up.
        /// </summary>
        private static void Widths(IXLWorksheet sheet)
        {
            for (int i = 0; i < TheirWidths.Length && i < LastColumn; i++)
            {
                // What ClosedXML writes is what it is given PLUS a fixed padding, measured
                // at 0.710625 across every column on 2026-09-01. Ours came out about 0.7
                // wider than theirs on every column, which is that padding and nothing
                // else. Taking it off here means the saved file holds their number.
                sheet.Column(i + 1).Width = TheirWidths[i] - ClosedXmlWidthPadding;
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
