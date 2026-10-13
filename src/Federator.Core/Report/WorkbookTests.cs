using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using ClosedXML.Excel;

namespace Federator.Core.Report
{
    /// <summary>
    /// The client's sheet read back test by test, by the name in column A, F127, so the rows
    /// under each test and its Clashes cell can be set beside what Clash Detective holds.
    ///
    /// ONE SHAPE. A full block is the name over two rows with Tolerance and Clashes over its
    /// values, then the item labels, the column headings with Clash Name in C, and one row
    /// per clash until C is empty, the rule WorkbookCheck counts rows by. Since F153, Bader's
    /// decision of 2026-10-13, a test that found nothing writes nothing in the workbook, so
    /// the one row Q73 gave it is never read as a test, and the workbook check names it.
    ///
    /// IT TAKES A SHEET OF A WORKBOOK ALREADY OPEN, so the file is loaded once for every
    /// check that reads it.
    ///
    /// WHAT IT DOUBTS, and says so rather than guessing. A name on two tests of the sheet,
    /// because a count can then belong to either. A test row with no name, because nothing
    /// can be paired on it. A Clashes cell that is no whole number, read as minus one. A full
    /// block with no clash table under it, its rows read as minus one. Names are read
    /// exactly and never trimmed, because two set names in the reference file end in a space
    /// and a test name is built out of two set names.
    /// </summary>
    public sealed class WorkbookTests
    {
        /// <summary>Their second column heading, Clash Name, read off the one list of their columns.</summary>
        private static readonly string ClashNameHeading = ClientReportColumns.General[1];

        private readonly List<WorkbookTest> tests = new List<WorkbookTest>();
        private readonly List<string> doubts = new List<string>();

        private WorkbookTests()
        {
            Tests = new ReadOnlyCollection<WorkbookTest>(tests);
            Doubts = new ReadOnlyCollection<string>(doubts);
        }

        /// <summary>Every test found, in sheet order.</summary>
        public ReadOnlyCollection<WorkbookTest> Tests { get; private set; }

        /// <summary>Every doubt, each a plain sentence naming the row or the name.</summary>
        public ReadOnlyCollection<string> Doubts { get; private set; }

        /// <summary>The client's sheet of a workbook already open.</summary>
        public static WorkbookTests Read(IXLWorksheet sheet)
        {
            if (sheet == null)
            {
                throw new ArgumentNullException("sheet");
            }

            WorkbookTests read = new WorkbookTests();
            read.Walk(sheet);
            read.DoubtNamesOnTwoTests();
            return read;
        }

        /// <summary>
        /// The client's sheet of the clash workbook at that path, read BY ITS NAME and never by its place,
        /// the readers' point on F127 that Read handed the first sheet would read any sheet put before it.
        /// The name is the one the writer gives the client's sheet, SheetNames.ForReport of the report's
        /// output name. Null where there is no file at the path or no sheet of that name, exactly, with
        /// why. A file that will not open throws, so the caller logs what it threw.
        /// </summary>
        public static WorkbookTests ReadFile(string path, string sheetName, out string whyNotRead)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                whyNotRead = "there is no workbook at " + (path ?? string.Empty);
                return null;
            }

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    if (string.Equals(sheet.Name, sheetName, StringComparison.Ordinal))
                    {
                        whyNotRead = null;
                        return Read(sheet);
                    }
                }
            }

            whyNotRead = "the workbook at " + path + " holds no sheet named " + (sheetName ?? string.Empty);
            return null;
        }

        /// <summary>Every test of the sheet with that name, Ordinal and never trimmed. Two or more is a doubt.</summary>
        public IList<WorkbookTest> Named(string name)
        {
            List<WorkbookTest> named = new List<WorkbookTest>();

            foreach (WorkbookTest test in tests)
            {
                if (string.Equals(test.Name, name, StringComparison.Ordinal))
                {
                    named.Add(test);
                }
            }

            return named;
        }

        private void Walk(IXLWorksheet sheet)
        {
            IXLRow lastUsed = sheet.LastRowUsed();
            int last = lastUsed == null ? 0 : lastUsed.RowNumber();
            int clashesColumn = WorkbookWriter.ColumnTestHeader + 1;
            int row = 1;

            while (row <= last)
            {
                string name = sheet.Cell(row, 1).GetString();
                bool fullBlock =
                    sheet.Cell(row, WorkbookWriter.ColumnTestHeader).GetString() == ClientFormat.TestHeader[0]
                    && sheet.Cell(row, clashesColumn).GetString() == ClientFormat.TestHeader[1];

                if (fullBlock)
                {
                    row = ReadBlock(sheet, row, name, last, clashesColumn);
                    continue;
                }

                // F153. A test with no clash writes nothing in the workbook, so only a full block is a
                // test here. The one row Q73 gave such a test is a problem of the workbook check's and is
                // never read as a test, so a test with no block reads as one that found none, CountCheck.
                row++;
            }
        }

        /// <summary>A full block from its first row. Returns the row after its last clash row.</summary>
        private int ReadBlock(IXLWorksheet sheet, int start, string name, int last, int clashesColumn)
        {
            int clashes = Whole(sheet.Cell(start + 1, clashesColumn), start + 1);
            int headings = start + 4;

            if (sheet.Cell(headings, WorkbookWriter.ColumnClashName).GetString() != ClashNameHeading)
            {
                if (name.Length > 0)
                {
                    doubts.Add("the block of \"" + name + "\" at row " + start
                        + " has no clash table under it, so its rows are UNKNOWN");
                }

                Add(name, start, -1, clashes);
                return start + 2;
            }

            int rows = 0;
            int at = headings + 1;

            while (at <= last && sheet.Cell(at, WorkbookWriter.ColumnClashName).GetString().Length > 0)
            {
                rows++;
                at++;
            }

            Add(name, start, rows, clashes);
            return at;
        }

        private void Add(string name, int row, int rows, int clashes)
        {
            if (string.IsNullOrEmpty(name))
            {
                doubts.Add("row " + row.ToString(CultureInfo.InvariantCulture)
                    + " carries a test with no name, so it cannot be set beside any test");
                return;
            }

            tests.Add(new WorkbookTest(name, row, rows, clashes));
        }

        /// <summary>The cell as a whole number, or minus one and a doubt.</summary>
        private int Whole(IXLCell cell, int row)
        {
            int whole = WholeOrMinusOne(cell);

            if (whole < 0)
            {
                doubts.Add("the Clashes cell at row " + row.ToString(CultureInfo.InvariantCulture) + " reads \""
                    + cell.GetString() + "\", which is no whole number, so it is read as UNKNOWN");
            }

            return whole;
        }

        /// <summary>The cell as a whole number from nought up, or minus one where it is not one. The one rule for it.</summary>
        internal static int WholeOrMinusOne(IXLCell cell)
        {
            if (cell.DataType == XLDataType.Number)
            {
                double value = cell.GetDouble();

                if (value >= 0 && value <= int.MaxValue && Math.Floor(value) == value)
                {
                    return (int)value;
                }
            }

            return -1;
        }

        /// <summary>
        /// The Clashes cell of the full block that starts at that row, the count the writer sorted the blocks by,
        /// as a whole number or minus one. A block starts on the row of its name, with its values under it, so the
        /// cell is one row down. The one place that layout is read, for the test count and the order check.
        /// </summary>
        internal static int ClashesOfBlock(IXLWorksheet sheet, int start)
        {
            return start < 1 ? -1 : WholeOrMinusOne(sheet.Cell(start + 1, WorkbookWriter.ColumnTestHeader + 1));
        }

        private void DoubtNamesOnTwoTests()
        {
            List<string> order = new List<string>();
            Dictionary<string, List<int>> rows = new Dictionary<string, List<int>>(StringComparer.Ordinal);

            foreach (WorkbookTest test in tests)
            {
                List<int> at;

                if (!rows.TryGetValue(test.Name, out at))
                {
                    at = new List<int>();
                    rows.Add(test.Name, at);
                    order.Add(test.Name);
                }

                at.Add(test.Row);
            }

            foreach (string name in order)
            {
                List<int> at = rows[name];

                if (at.Count < 2)
                {
                    continue;
                }

                List<string> numbers = new List<string>();

                foreach (int row in at)
                {
                    numbers.Add(row.ToString(CultureInfo.InvariantCulture));
                }

                doubts.Add("the name \"" + name + "\" is on " + at.Count + " tests of the sheet, rows "
                    + string.Join(", ", numbers.ToArray()) + ", so neither is set beside the document");
            }
        }
    }
}
