using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;

namespace Federator.Core.Report
{
    /// <summary>
    /// Reads a written workbook back off the disk and compares it against the report the
    /// client receives.
    ///
    /// WHY THIS HAD TO CHANGE. The check before this one passed while the test order, the
    /// id label and both number formats all differed from the samples, because it counted
    /// PRESENCE and nothing else. A column being there says nothing about whether it is in
    /// the right place, holds the right shape of value, or sits in the right block. So this
    /// compares three things it did not: which columns, in what shape, in what order.
    ///
    /// It reports the FIRST divergence with what ours holds and what theirs holds, because
    /// a list of forty consequences of one fault is harder to act on than the fault.
    ///
    /// AND WHY IT CHANGED AGAIN. That version still passed with eight visible differences
    /// sitting in the file: no fill anywhere, no border anywhere, the wrong row heights,
    /// the wrong column widths, a Layer column nothing filled, a filename in the Image
    /// cell, the raw double behind a display format and Complete where theirs says OK. It
    /// looked at the words in the header row and the shape of three values, so none of
    /// those was in front of it. This one walks every cell of every block and compares the
    /// VALUE, the data TYPE, the NUMBER FORMAT, the FILL, the BORDERS, the ROW HEIGHT and
    /// the COLUMN WIDTH, and prints both sides of whatever differs.
    ///
    /// WHAT IT STILL CANNOT CATCH, which is the third time a check here has reported clean
    /// over a real difference, so it is written down rather than discovered again:
    ///
    ///   THE TABLE BEING WRONG. This compares the file against ClientLayout, and the
    ///   writer paints from ClientLayout too. A number that is wrong in both is invisible
    ///   here. Only ClientLayoutTests can catch it, by opening the sample and asserting
    ///   every number against their file
    ///
    ///   ANYTHING NOT IN THE LIST ABOVE. Fonts, font colours, the sheet name, freeze panes,
    ///   print setup and merged ranges are not compared. They are listed in scan.md
    ///   section 4q with what each of ours holds and what each of theirs holds
    ///
    ///   WHETHER A VALUE IS TRUE. It can see that the Distance cell holds a number to three
    ///   decimals. It cannot see that the number came off the wrong clash
    ///
    ///   A BLOCK THAT WAS NEVER WRITTEN. It checks the blocks it finds. A test dropped
    ///   before the workbook was written leaves nothing here to find, which is what the
    ///   Summary in the log is for
    ///
    /// Nothing here throws. Checking a report must never be the reason a run fails.
    /// </summary>
    public sealed class WorkbookCheck
    {
        private readonly List<string> problems = new List<string>();

        private WorkbookCheck()
        {
            Path = string.Empty;
            CouldNotRead = string.Empty;
            SheetName = string.Empty;
        }

        public string Path { get; private set; }

        /// <summary>Empty unless the workbook could not be read at all.</summary>
        public string CouldNotRead { get; private set; }

        public bool Ran
        {
            get { return CouldNotRead.Length == 0; }
        }

        /// <summary>How many sheets it holds. Theirs holds one.</summary>
        public int Sheets { get; private set; }

        public string SheetName { get; private set; }

        /// <summary>Test blocks found on the sheet.</summary>
        public int Blocks { get; private set; }

        /// <summary>Clash rows across every block.</summary>
        public int Rows { get; private set; }

        /// <summary>The clash count of each block, in the order they appear.</summary>
        public IList<int> BlockCounts { get; private set; }

        /// <summary>Everything wrong, each a plain sentence, worst first.</summary>
        internal IList<string> Problems
        {
            get { return problems.AsReadOnly(); }
        }

        public bool Passed
        {
            get { return Ran && problems.Count == 0; }
        }

        /// <summary>The first thing that differs, or an empty string.</summary>
        public string FirstDivergence
        {
            get { return problems.Count == 0 ? string.Empty : problems[0]; }
        }

        public static WorkbookCheck Of(string path)
        {
            WorkbookCheck check = new WorkbookCheck();
            check.Path = path ?? string.Empty;
            check.BlockCounts = new List<int>();

            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    check.CouldNotRead = "there is no file at " + check.Path;
                    return check;
                }

                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    check.Read(workbook);
                }
            }
            catch (Exception error)
            {
                check.CouldNotRead = "it could not be opened, " + error.GetType().Name;
            }

            return check;
        }

        private void Read(XLWorkbook workbook)
        {
            List<IXLWorksheet> sheets = new List<IXLWorksheet>();

            foreach (IXLWorksheet sheet in workbook.Worksheets)
            {
                sheets.Add(sheet);
            }

            Sheets = sheets.Count;

            if (Sheets == 0)
            {
                problems.Add("The workbook has no sheets at all.");
                return;
            }

            SheetName = sheets[0].Name;

            // Theirs is one sheet holding every test. Ours had fifty.
            if (Sheets != 1)
            {
                List<string> names = new List<string>();

                for (int i = 0; i < sheets.Count && i < 4; i++)
                {
                    names.Add(sheets[i].Name);
                }

                problems.Add("The workbook has " + Sheets
                    + " sheets and the client's report has one. Ours starts "
                    + string.Join(", ", names.ToArray())
                    + " and theirs is a single sheet holding every test one after another.");
            }

            ReadSheet(sheets[0]);
        }

        private void ReadSheet(IXLWorksheet sheet)
        {
            List<int> counts = new List<int>();
            int lastRow = sheet.LastRowUsed() == null ? 0 : sheet.LastRowUsed().RowNumber();

            for (int row = 1; row <= lastRow; row++)
            {
                // A block header is the row whose Clash Name column holds their heading.
                if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() != "Clash Name")
                {
                    continue;
                }

                Blocks++;
                CheckColumnOrder(sheet, row);

                // Only the first block is walked cell by cell. Every block is painted by
                // the same code, so a fault in one is a fault in all of them, and 1830
                // blocks times nineteen columns is a check nobody reads.
                if (Blocks == 1)
                {
                    for (int at = row - 4; at <= row; at++)
                    {
                        if (at >= 1)
                        {
                            CheckCells(sheet, at, ClientLayout.KindOf(at, row));
                        }
                    }

                    CheckWidths(sheet);
                    CheckTitle(sheet);
                }

                int rows = 0;

                for (int at = row + 1; at <= lastRow; at++)
                {
                    if (sheet.Cell(at, WorkbookWriter.ColumnClashName).GetString().Length == 0)
                    {
                        break;
                    }

                    rows++;
                    Rows++;

                    if (Blocks == 1 && rows == 1)
                    {
                        CheckCells(sheet, at, ClientLayout.RowKind.Clash);
                    }

                    CheckShape(sheet, at, rows == 1 && Blocks == 1);
                }

                counts.Add(rows);
            }

            BlockCounts = counts;
            CheckOrder(counts);

            if (Blocks == 0)
            {
                problems.Add("The sheet has no clash table at all, so nothing on it could "
                    + "be compared with the client's report.");
            }
        }

        // ---------- which columns, in what order ----------

        private void CheckColumnOrder(IXLWorksheet sheet, int row)
        {
            IList<string> theirs = ClientReportColumns.All();
            List<string> ours = new List<string>();

            foreach (int column in Columns())
            {
                ours.Add(sheet.Cell(row, column).GetString());
            }

            for (int i = 0; i < theirs.Count; i++)
            {
                string mine = i < ours.Count ? ours[i] : string.Empty;

                if (string.Equals(mine, theirs[i], StringComparison.Ordinal))
                {
                    continue;
                }

                Say("Column " + (i + 1) + " of the clash table is wrong. Ours reads "
                    + Quote(mine) + " and the client's report reads " + Quote(theirs[i])
                    + ". The whole order should be " + string.Join(", ", Array(theirs)) + ".");
                return;
            }
        }

        /// <summary>The sheet columns a clash row uses, in order, gaps skipped.</summary>
        private static IList<int> Columns()
        {
            List<int> at = new List<int>
            {
                WorkbookWriter.ColumnImage,
                WorkbookWriter.ColumnClashName,
                WorkbookWriter.ColumnStatus,
                WorkbookWriter.ColumnDistance,
                WorkbookWriter.ColumnGridLocation,
                WorkbookWriter.ColumnDescription,
                WorkbookWriter.ColumnClashPoint
            };

            for (int i = 0; i < ClientFormat.ItemColumns; i++)
            {
                at.Add(WorkbookWriter.ColumnItem1 + i);
            }

            for (int i = 0; i < ClientFormat.ItemColumns; i++)
            {
                at.Add(WorkbookWriter.ColumnItem2 + i);
            }

            return at;
        }

        // ---------- cell against cell ----------

        /// <summary>
        /// One row, every column, against what the client's report carries there. The row
        /// height first, because a wrong height is one line rather than nineteen.
        /// </summary>
        private void CheckCells(IXLWorksheet sheet, int row, ClientLayout.RowKind kind)
        {
            double wanted = ClientLayout.Height(kind);
            double got = sheet.Row(row).Height;

            if (Math.Abs(got - wanted) > ClientLayout.Epsilon)
            {
                Say("Row " + row + ", the " + Words(kind) + " row, is "
                    + Number(got) + " high and the client's report has it at "
                    + Number(wanted) + ".");
            }

            for (int column = 1; column <= WorkbookWriter.LastColumn; column++)
            {
                CheckFill(sheet, row, column, kind);
                CheckBorder(sheet, row, column, kind);
            }
        }

        private void CheckFill(
            IXLWorksheet sheet, int row, int column, ClientLayout.RowKind kind)
        {
            string wanted = ClientLayout.Fill(kind, column);
            string got = Colour(sheet.Cell(row, column));

            if (string.Equals(got, wanted, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Say("Cell " + Where(row, column) + ", on the " + Words(kind) + " row, is "
                + Paint(got) + " and the client's report paints it " + Paint(wanted)
                + ". That banding is the first thing a reader sees, so a report without "
                + "it does not look like the one they accepted.");
        }

        /// <summary>
        /// Whether the cell is ruled at all. Which edges carry which weight is theirs and
        /// is painted from the same runs, so what is worth reporting is a row that came
        /// out bare.
        /// </summary>
        private void CheckBorder(
            IXLWorksheet sheet, int row, int column, ClientLayout.RowKind kind)
        {
            if (column > ClientLayout.LastColumnOf(kind))
            {
                return;
            }

            IXLStyle style = sheet.Cell(row, column).Style;

            bool ruled = style.Border.TopBorder != XLBorderStyleValues.None
                || style.Border.BottomBorder != XLBorderStyleValues.None
                || style.Border.LeftBorder != XLBorderStyleValues.None
                || style.Border.RightBorder != XLBorderStyleValues.None;

            if (ruled)
            {
                return;
            }

            Say("Cell " + Where(row, column) + ", on the " + Words(kind)
                + " row, carries no border and every cell of the client's table is boxed.");
        }

        /// <summary>Their nineteen column widths, against ours.</summary>
        private void CheckWidths(IXLWorksheet sheet)
        {
            for (int column = 1; column <= WorkbookWriter.LastColumn; column++)
            {
                // ClosedXML reports a width with its own padding already taken off, and
                // the file carries it with the padding on, which is what the writer adds.
                // Comparing the two directly reported all nineteen as wrong on a file that
                // matched theirs to six decimals.
                double wanted = ClientLayout.Width(column) - WorkbookWriter.ClosedXmlWidthPadding;
                double got = sheet.Column(column).Width;

                if (Math.Abs(got - wanted) <= ClientLayout.Epsilon)
                {
                    continue;
                }

                Say("Column " + Letter(column) + " is " + Number(got)
                    + " wide and the client's report has it at " + Number(wanted)
                    + ", so their columns and ours do not line up.");
                return;
            }
        }

        /// <summary>Row 1, which carries their words and their height.</summary>
        private void CheckTitle(IXLWorksheet sheet)
        {
            double got = sheet.Row(1).Height;

            if (Math.Abs(got - WorkbookWriter.TitleRowHeight) > ClientLayout.Epsilon)
            {
                Say("Row 1 is " + Number(got) + " high and the client's report has it at "
                    + Number(WorkbookWriter.TitleRowHeight) + ", which is the room the "
                    + "logo sits in.");
            }

            string title = sheet.Cell(1, 4).GetString();

            if (!string.Equals(title, WorkbookWriter.TitleText, StringComparison.Ordinal))
            {
                Say("Row 1 reads " + Quote(title) + " and the client's report reads "
                    + Quote(WorkbookWriter.TitleText) + ".");
            }
        }

        private static string Colour(IXLCell cell)
        {
            try
            {
                if (cell.Style.Fill.PatternType == XLFillPatternValues.None)
                {
                    return string.Empty;
                }

                // XLColor.Color is a System.Drawing colour, so the six digits are read
                // off it rather than through a helper that does not exist on it.
                System.Drawing.Color colour = cell.Style.Fill.BackgroundColor.Color;

                return colour.R.ToString("X2", CultureInfo.InvariantCulture)
                    + colour.G.ToString("X2", CultureInfo.InvariantCulture)
                    + colour.B.ToString("X2", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string Paint(string colour)
        {
            return colour.Length == 0 ? "not filled" : "filled " + colour;
        }

        private static string Words(ClientLayout.RowKind kind)
        {
            switch (kind)
            {
                case ClientLayout.RowKind.Title: return "title";
                case ClientLayout.RowKind.TestHeader: return "test heading";
                case ClientLayout.RowKind.TestValues: return "test value";
                case ClientLayout.RowKind.Gap: return "blank";
                case ClientLayout.RowKind.ItemLabels: return "Item 1 and Item 2";
                case ClientLayout.RowKind.Headings: return "column heading";
                default: return "clash";
            }
        }

        private static string Where(int row, int column)
        {
            return Letter(column) + row.ToString(CultureInfo.InvariantCulture);
        }

        private static string Letter(int column)
        {
            string name = string.Empty;
            int at = column;

            while (at > 0)
            {
                int part = (at - 1) % 26;
                name = (char)(65 + part) + name;
                at = (at - 1 - part) / 26;
            }

            return name;
        }

        private static string Number(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        // ---------- what shape the values are ----------

        private void CheckShape(IXLWorksheet sheet, int row, bool first)
        {
            if (!first)
            {
                return;
            }

            string id = sheet.Cell(row, WorkbookWriter.ColumnItem1).GetString();

            if (id.Length > 0 && !ClientShapes.LooksLikeAnItemId(id))
            {
                Say("The Item ID cell is the wrong shape. Ours reads " + Quote(id)
                    + " and the client's report reads "
                    + Quote(ClientShapes.ExampleItemId) + ".");
            }

            string point = sheet.Cell(row, WorkbookWriter.ColumnClashPoint).GetString();

            if (point.Length > 0 && !ClientShapes.LooksLikeAClashPoint(point))
            {
                Say("The Clash Point cell is the wrong shape. Ours reads " + Quote(point)
                    + " and the client's report reads "
                    + Quote(ClientShapes.ExampleClashPoint) + ".");
            }

            IXLCell distance = sheet.Cell(row, WorkbookWriter.ColumnDistance);

            if (distance.DataType != XLDataType.Number)
            {
                Say("The Distance cell holds " + Quote(distance.GetString())
                    + " as text and the client's report holds a number there, so ours "
                    + "cannot be sorted or filtered on.");
            }
            else
            {
                // Theirs stores the ROUNDED number under General. A format here means ours
                // is keeping the raw double and hiding it behind a display, which is what
                // put -0.328083992004395 into a cell reading -0.328.
                if (distance.Style.NumberFormat.Format != ClientLayout.DistanceNumberFormat)
                {
                    Say("The Distance cell carries the number format "
                        + Quote(distance.Style.NumberFormat.Format)
                        + " and the client's report carries none, which means ours is "
                        + "storing the raw value behind a display and theirs is not.");
                }

                double held = distance.GetDouble();

                if (Math.Abs(held - ClientFormat.Rounded(held)) > 1e-9)
                {
                    Say("The Distance cell holds "
                        + held.ToString("R", CultureInfo.InvariantCulture)
                        + " and the client's report holds it rounded, "
                        + Quote(ClientFormat.Fixed(held)) + ".");
                }
            }

            string layer = sheet.Cell(row, WorkbookWriter.ColumnItem1 + 1).GetString();

            if (layer.Length == 0)
            {
                Say("The Item 1 Layer cell is empty and the client's report carries the "
                    + "level in it. Nothing was ever written into it, so the column reads "
                    + "as a column their report fills and ours does not.");
            }

            IXLCell image = sheet.Cell(row, WorkbookWriter.ColumnImage);

            if (image.GetString().Length > 0)
            {
                Say("The Image cell reads " + Quote(image.GetString())
                    + " and the client's report leaves it empty with the picture behind "
                    + "it, so ours shows a file name where theirs shows a photo.");
            }
        }

        // ---------- in what order ----------

        private void CheckOrder(IList<int> counts)
        {
            for (int i = 1; i < counts.Count; i++)
            {
                if (counts[i] <= counts[i - 1])
                {
                    continue;
                }

                Say("The tests are in the wrong order. Block " + i + " holds " + counts[i - 1]
                    + " clashes and block " + (i + 1) + " holds " + counts[i]
                    + ". The client's report puts the most clashes first, so a reader is "
                    + "not scrolling past empty tests.");
                return;
            }
        }

        private void Say(string problem)
        {
            problems.Add(problem);
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty) + "\"";
        }

        private static string[] Array(IList<string> values)
        {
            string[] all = new string[values.Count];
            values.CopyTo(all, 0);
            return all;
        }

        // ---------- what it says ----------

        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (!Ran)
            {
                lines.Add("CHECK    the workbook could not be checked, " + CouldNotRead);
                return lines;
            }

            lines.Add("CHECK    " + Sheets + (Sheets == 1 ? " sheet, " : " sheets, ")
                + Quote(SheetName) + ", " + Blocks + " test "
                + (Blocks == 1 ? "block" : "blocks") + ", " + Rows + " clash "
                + (Rows == 1 ? "row" : "rows") + ".");

            if (BlockCounts.Count > 0)
            {
                List<string> first = new List<string>();

                for (int i = 0; i < BlockCounts.Count && i < 8; i++)
                {
                    first.Add(BlockCounts[i].ToString());
                }

                lines.Add("         the first blocks hold " + string.Join(", ", first.ToArray())
                    + (BlockCounts.Count > 8 ? " and so on." : "."));
            }

            foreach (string problem in problems)
            {
                lines.Add("         " + problem);
            }

            if (problems.Count == 0)
            {
                lines.Add("         Every column, value shape, fill, border, row height "
                    + "and column width matches the client's report, and the blocks are "
                    + "in their order.");
                lines.Add("         Not compared: the font, the sheet name, freeze panes, "
                    + "print setup, merged ranges, and whether a value is true. See "
                    + "scan.md 4q.");
            }

            lines.Add("         " + ClientReportColumns.ReadFrom());

            return lines;
        }

        public string Summary()
        {
            if (!Ran)
            {
                return "Workbook not checked, " + CouldNotRead + ".";
            }

            if (problems.Count > 0)
            {
                return "Workbook: " + FirstDivergence;
            }

            return "Workbook: one sheet, " + Blocks + " tests, " + Rows
                + " rows, matching the client's layout.";
        }
    }
}
