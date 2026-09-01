using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace Federator.Core.Report
{
    /// <summary>
    /// Reads a written workbook back off the disk and says what is in it.
    ///
    /// The file, never the object that produced it, for the same reason as
    /// <see cref="PageCheck"/>. This is the check that would have caught Source File and
    /// Discipline coming out empty on every row without Bader opening the workbook and
    /// searching it.
    ///
    /// Nothing here throws. Checking a report must never be the reason a run fails.
    /// </summary>
    public sealed class WorkbookCheck
    {
        private readonly List<ColumnFill> columns = new List<ColumnFill>();
        private readonly List<string> problems = new List<string>();

        private WorkbookCheck()
        {
            Path = string.Empty;
            CouldNotRead = string.Empty;
        }

        public string Path { get; private set; }

        /// <summary>Empty unless the workbook could not be read at all.</summary>
        public string CouldNotRead { get; private set; }

        public bool Ran
        {
            get { return CouldNotRead.Length == 0; }
        }

        /// <summary>Clash rows across every test sheet.</summary>
        public int Rows { get; private set; }

        public int Sheets { get; private set; }

        /// <summary>One per column of ours, with how many rows filled it.</summary>
        public IList<ColumnFill> Columns
        {
            get { return columns.AsReadOnly(); }
        }

        public IList<string> Problems
        {
            get { return problems.AsReadOnly(); }
        }

        public bool Passed
        {
            get { return Ran && problems.Count == 0; }
        }

        /// <summary>Reads the workbook at this path.</summary>
        public static WorkbookCheck Of(string path)
        {
            WorkbookCheck check = new WorkbookCheck();
            check.Path = path ?? string.Empty;

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
            Dictionary<string, int> filled = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (string ours in WorkbookWriter.OurColumns)
            {
                filled[ours] = 0;
            }

            foreach (IXLWorksheet sheet in workbook.Worksheets)
            {
                if (sheet.Name == SheetNames.SummarySheet || sheet.Name == SheetNames.MatrixSheet)
                {
                    continue;
                }

                Sheets++;
                ReadSheet(sheet, filled);
            }

            foreach (string ours in WorkbookWriter.OurColumns)
            {
                columns.Add(new ColumnFill(ours, filled[ours]));
            }

            Judge();
        }

        private void ReadSheet(IXLWorksheet sheet, IDictionary<string, int> filled)
        {
            // The header is the row whose second cell is the client's Clash Name column.
            int header = 0;

            for (int row = 1; row <= 40; row++)
            {
                if (sheet.Cell(row, 2).GetString() == "Clash Name")
                {
                    header = row;
                    break;
                }
            }

            if (header == 0)
            {
                problems.Add("The sheet " + sheet.Name
                    + " has no clash table, so nothing on it could be counted.");
                return;
            }

            // Where each of our columns sits on this sheet, read off the header rather
            // than assumed, because the client only mode leaves them out entirely.
            Dictionary<int, string> where = new Dictionary<int, string>();
            int width = 1;

            for (int column = 1; column <= 60; column++)
            {
                string name = sheet.Cell(header, column).GetString();

                if (name.Length == 0)
                {
                    continue;
                }

                width = column;

                if (filled.ContainsKey(name))
                {
                    where[column] = name;
                }
            }

            for (int row = header + 1; ; row++)
            {
                // A row is real while its Clash Name cell holds something.
                if (sheet.Cell(row, 2).GetString().Length == 0)
                {
                    break;
                }

                Rows++;

                foreach (KeyValuePair<int, string> at in where)
                {
                    if (sheet.Cell(row, at.Key).GetString().Trim().Length > 0)
                    {
                        filled[at.Value] = filled[at.Value] + 1;
                    }
                }
            }
        }

        private void Judge()
        {
            if (Rows == 0)
            {
                return;
            }

            List<string> empty = new List<string>();

            foreach (ColumnFill column in columns)
            {
                if (column.Filled == 0)
                {
                    empty.Add(column.Name);
                }
            }

            if (empty.Count > 0)
            {
                problems.Add("These columns are empty on every row, "
                    + string.Join(", ", empty.ToArray())
                    + ". Either the model does not carry them or they are not being read.");
            }
        }

        /// <summary>The block for the log.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (!Ran)
            {
                lines.Add("CHECK    the workbook could not be checked, " + CouldNotRead);
                return lines;
            }

            lines.Add("CHECK    " + Rows + " clash "
                + (Rows == 1 ? "row" : "rows") + " across " + Sheets
                + " test " + (Sheets == 1 ? "sheet" : "sheets") + ".");

            if (columns.Count == 0 || Rows == 0)
            {
                lines.Add("         nothing of ours to count on it.");
            }

            foreach (ColumnFill column in columns)
            {
                lines.Add("         " + column.Name.PadRight(20)
                    + column.Filled + " of " + Rows
                    + (column.Filled == 0 && Rows > 0 ? "   EMPTY ON EVERY ROW" : string.Empty));
            }

            foreach (string problem in problems)
            {
                lines.Add("         " + problem);
            }

            if (problems.Count == 0 && Rows > 0)
            {
                lines.Add("         Every column of ours is filled somewhere.");
            }

            return lines;
        }

        /// <summary>One line for the run view.</summary>
        public string Summary()
        {
            if (!Ran)
            {
                return "Workbook not checked, " + CouldNotRead + ".";
            }

            if (Rows == 0)
            {
                return "Workbook written, no clash rows on it.";
            }

            if (problems.Count > 0)
            {
                return "Workbook: " + problems[0];
            }

            return "Workbook: " + Rows + " rows, every column of ours filled somewhere.";
        }
    }

    /// <summary>One of our own columns, and how many rows put something in it.</summary>
    public sealed class ColumnFill
    {
        internal ColumnFill(string name, int filled)
        {
            Name = name;
            Filled = filled;
        }

        public string Name { get; private set; }

        public int Filled { get; private set; }

        public override string ToString()
        {
            return Name + " " + Filled;
        }
    }
}
