using System;
using System.Collections.Generic;
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
        public IList<string> Problems
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

                int rows = 0;

                for (int at = row + 1; at <= lastRow; at++)
                {
                    if (sheet.Cell(at, WorkbookWriter.ColumnClashName).GetString().Length == 0)
                    {
                        break;
                    }

                    rows++;
                    Rows++;
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

            if (distance.DataType == XLDataType.Number
                && distance.Style.NumberFormat.Format != ClientFormat.FixedFormat)
            {
                Say("The Distance cell carries the number format "
                    + Quote(distance.Style.NumberFormat.Format)
                    + " and the client's report writes three decimals, "
                    + Quote(ClientFormat.FixedFormat) + ", so ours prints the whole double.");
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
                lines.Add("         Every column, shape and block order matches the client's "
                    + "report.");
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
