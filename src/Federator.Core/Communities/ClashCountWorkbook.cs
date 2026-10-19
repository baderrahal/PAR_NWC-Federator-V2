using System;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;

namespace Federator.Core.Communities
{
    /// <summary>
    /// The workbook of a community's clash count check, F236: one sheet, its title and the run date, the table of Bader's
    /// columns with one row per group per test with clashes, a total per group and one for the community, a row red where its
    /// four counts differ, then the groups not built and the tests the two rules left out, a count a group. Saved in the
    /// community's report folder as "community clash count check.xlsx", a name already there taking a dated one, and read
    /// back off the disk.
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
                IXLWorksheet sheet = workbook.Worksheets.Add(SheetName(check.Community));
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
                    sheet.Cell(row, 2).SetValue(group.LeftOutByOneDiscipline);
                    sheet.Cell(row, 3).SetValue(group.LeftOutBySetCodes);
                    row++;
                }

                foreach (string group in check.NotBuilt)
                {
                    sheet.Cell(row, 1).SetValue(group);
                    sheet.Cell(row, 2).SetValue(EveryTestLeftOut);
                    row++;
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

            if (line.Differs)
            {
                range.Style.Fill.BackgroundColor = Red;
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

        /// <summary>The sheet's name, the community's, cut to the 31 characters Excel allows.</summary>
        public static string SheetName(string community)
        {
            string name = string.IsNullOrEmpty(community) ? "Clash count check" : community;
            return name.Length > 31 ? name.Substring(0, 31) : name;
        }

        /// <summary>
        /// The CHECKS line of the workbook read back off the disk: its rows of tests, those red, and the groups not built,
        /// or why it would not read. A row of a test is any row of the table whose Test cell does not begin with the total's word.
        /// </summary>
        public static string ReadBackLine(string path, string community)
        {
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
                        string test = sheet.Cell(row, 2).GetString();

                        if (!test.StartsWith(ClashCountCheck.TotalWords + " ", StringComparison.Ordinal))
                        {
                            tests++;
                            red += sheet.Cell(row, 1).Style.Fill.BackgroundColor == Red ? 1 : 0;
                        }

                        row++;
                    }

                    return "COUNTS   read back " + tests + (tests == 1 ? " test row, " : " test rows, ") + red + " red, of " + community;
                }
            }
            catch (Exception error)
            {
                return "COUNTS   read back FAILED  " + path + ", " + error.GetType().Name + ": " + error.Message;
            }
        }
    }
}
