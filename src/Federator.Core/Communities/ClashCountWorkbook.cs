using System;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;
using Federator.Core.Report;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The workbook of a community's clash count check, F236: one sheet, its title and the run date, the table of Bader's
    /// columns with one row per group per test with clashes, a total per group and one for the community, a row red where its
    /// four counts differ, then the groups not built, the tests the two rules left out, a count a group, and the groups whose
    /// files could not be read. Saved in the community's report folder as "community clash count check.xlsx", a file of that
    /// name overwritten since F243, Bader's order of 19 Oct 2026, and read back off the disk.
    /// </summary>
    public static class ClashCountWorkbook
    {
        public const string Suffix = "clash count check";

        public const int HeaderRow = 4;

        public static readonly XLColor Red = XLColor.FromHtml("#FFC7CE");
        public static readonly XLColor HeaderFill = XLColor.FromHtml("#D9D9D9");

        /// <summary>The widths of the columns, chosen so the words and the numbers show whole.</summary>
        public static readonly double[] Widths = { 34, 62, 9, 14, 11, 11, 14, 11, 11, 54, 9, 9, 10, 10, 10 };

        public const string NotBuiltHeading = "Groups not built for one discipline";
        public const string LeftOutHeading = "Tests left out by a rule";
        public static readonly string[] LeftOutColumns = { "Building", "Left out by one discipline", "Left out by two set codes" };
        public const string EveryTestLeftOut = "every test, the group was not built";
        public const string NotReadHeading = "Groups whose counts could not be read";
        public static readonly string[] NotReadColumns = { "Building", "Why" };

        public static string NameFor(string community)
        {
            return (community ?? string.Empty) + " " + Suffix + ".xlsx";
        }

        public static void Write(string path, ClashCountCheck check, DateTime run)
        {
            if (check == null)
            {
                throw new ArgumentNullException("check");
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet sheet = workbook.Worksheets.Add(SheetNames.ForReport(check.Community));
                sheet.Cell(1, 1).SetValue(check.Community + " " + Suffix);
                sheet.Cell(1, 1).Style.Font.Bold = true;
                sheet.Cell(1, 1).Style.Font.FontSize = 14;
                sheet.Cell(2, 1).SetValue("Run date " + run.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                Header(sheet, HeaderRow, ClashCountRow.Columns);
                int row = HeaderRow + 1;

                foreach (ClashCountGroup group in check.Groups)
                {
                    foreach (ClashCountRow line in group.Rows)
                    {
                        Put(sheet, row, line, false);
                        row++;
                    }

                    Put(sheet, row, group.Total(), true);
                    row++;
                }

                Put(sheet, row, check.Total(), true);
                int tableEnd = row;
                row += 2;
                Header(sheet, row, new[] { NotBuiltHeading });
                row++;

                foreach (string group in check.NotBuilt)
                {
                    sheet.Cell(row, 1).SetValue(group);
                    row++;
                }

                row++;
                Header(sheet, row, new[] { LeftOutHeading });
                row++;
                Header(sheet, row, LeftOutColumns);
                row++;

                foreach (ClashCountGroup group in check.Groups)
                {
                    sheet.Cell(row, 1).SetValue(group.Building);
                    PutCount(sheet.Cell(row, 2), group.LeftOutByOneDiscipline);
                    PutCount(sheet.Cell(row, 3), group.LeftOutBySetCodes);
                    row++;
                }

                foreach (string group in check.NotBuilt)
                {
                    sheet.Cell(row, 1).SetValue(group);
                    sheet.Cell(row, 2).SetValue(EveryTestLeftOut);
                    row++;
                }

                if (check.NotRead.Count > 0)
                {
                    row++;
                    Header(sheet, row, new[] { NotReadHeading });
                    row++;
                    Header(sheet, row, NotReadColumns);
                    row++;

                    foreach (KeyValuePair<string, string> group in check.NotRead)
                    {
                        sheet.Cell(row, 1).SetValue(group.Key);
                        sheet.Cell(row, 2).SetValue(group.Value);
                        row++;
                    }
                }

                sheet.Range(HeaderRow, 1, Math.Max(HeaderRow + 1, tableEnd), ClashCountRow.Columns.Length).SetAutoFilter();
                sheet.SheetView.FreezeRows(HeaderRow);

                for (int c = 0; c < Widths.Length; c++)
                {
                    sheet.Column(c + 1).Width = Widths[c];
                }

                workbook.SaveAs(path);
            }
        }

        /// <summary>A row of the table, a total bold. Only a row of a test is red, a total adding up rows already red.</summary>
        private static void Put(IXLWorksheet sheet, int row, ClashCountRow line, bool total)
        {
            IList<object> cells = line.Cells();

            for (int c = 0; c < cells.Count; c++)
            {
                IXLCell cell = sheet.Cell(row, c + 1);

                if (cells[c] is int)
                {
                    cell.SetValue((int)cells[c]);
                }
                else
                {
                    cell.SetValue(cells[c] as string ?? string.Empty);
                }
            }

            IXLRange range = sheet.Range(row, 1, row, ClashCountRow.Columns.Length);
            range.Style.Alignment.WrapText = true;
            range.Style.Font.Bold = total;

            if (!total && line.Differs)
            {
                range.Style.Fill.BackgroundColor = Red;
            }
        }

        /// <summary>A count as a number, or UNKNOWN where it was not read, never a nought.</summary>
        private static void PutCount(IXLCell cell, int count)
        {
            if (count < 0)
            {
                cell.SetValue(ClashCountRow.UnknownWord);
            }
            else
            {
                cell.SetValue(count);
            }
        }

        private static void Header(IXLWorksheet sheet, int row, string[] columns)
        {
            for (int c = 0; c < columns.Length; c++)
            {
                IXLCell cell = sheet.Cell(row, c + 1);
                cell.SetValue(columns[c]);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = HeaderFill;
                cell.Style.Alignment.WrapText = true;
            }
        }

        /// <summary>
        /// The CHECKS line of the workbook read back off the disk against the check it was written from: its rows of tests, a
        /// row of the table that is not bold, and those red, the same as written, or THE WORKBOOK DIFFERS with both sides, or
        /// why it would not read.
        /// </summary>
        public static string ReadBackLine(string path, ClashCountCheck check)
        {
            if (check == null)
            {
                throw new ArgumentNullException("check");
            }

            int rowsWritten = 0;

            foreach (ClashCountGroup group in check.Groups)
            {
                rowsWritten += group.Rows.Count;
            }

            int redWritten = check.Differing().Count;

            try
            {
                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    IXLWorksheet sheet = workbook.Worksheet(1);
                    int tests = 0;
                    int red = 0;
                    int row = HeaderRow + 1;

                    while (sheet.Cell(row, 2).GetString().Length > 0)
                    {
                        if (!sheet.Cell(row, 2).Style.Font.Bold)
                        {
                            tests++;
                            red += sheet.Cell(row, 1).Style.Fill.BackgroundColor == Red ? 1 : 0;
                        }

                        row++;
                    }

                    string read = tests + (tests == 1 ? " test row, " : " test rows, ") + red + " red";

                    return tests == rowsWritten && red == redWritten
                        ? "COUNTS   read back " + read + ", as written, of " + check.Community
                        : "COUNTS   read back THE WORKBOOK DIFFERS  " + check.Community + "  read " + read + ", written " + rowsWritten
                            + (rowsWritten == 1 ? " test row, " : " test rows, ") + redWritten + " red";
                }
            }
            catch (Exception error)
            {
                return "COUNTS   read back FAILED  " + path + ", " + error.GetType().Name + ": " + error.Message;
            }
        }
    }
}
