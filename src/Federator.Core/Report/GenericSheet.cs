using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Federator.Core.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// The Generic Models sheet, F128 and FR-177, Bader's request 3 under Q112: each model of the group
    /// that holds Generic Models items, with its count, and a model with none left out. It is written into
    /// an open workbook and the caller saves, the way the Coverage sheet is, so this class decides what is
    /// on the sheet and nothing about which workbook it goes into or where it stands among the sheets.
    ///
    /// WHERE IT GOES IS NOT DECIDED HERE. Bader's decision under Q46 makes the Coverage sheet the second and
    /// last sheet of the group's workbook, FR-200, and the check of the workbook allows nothing else after
    /// the client's sheet. A third sheet breaks one of those two, and a workbook of its own for the group,
    /// as Q126 B gives the Coverage sheet when the clash is skipped, breaks neither. The laptop lane puts
    /// the question to him with the add-in half.
    ///
    /// THE ROWS ARE ONE SHAPE, Rows, read by the writer and by any check of the sheet. A count is a number,
    /// and a count nobody took is the word UNKNOWN with why, never an empty cell and never nought.
    /// </summary>
    public static class GenericSheet
    {
        /// <summary>The first words of the sheet, so a reader sees at once it is not part of the client's report.</summary>
        public const string Title = "Generic Models, written by the NWC Federator and not part of the client's report";

        /// <summary>The headings of the table, in the order of its columns.</summary>
        public static readonly string[] Headings = { "Model", "Items", "Why not counted" };

        /// <summary>The width of each column, in Excel's units, chosen and not measured, as the Coverage sheet's are.</summary>
        public static readonly double[] Widths = { 60, 12, 70 };

        /// <summary>
        /// Why no Generic Models sheet can be written beside the sheets named, or null where it can: Excel
        /// refuses two sheets of one name, whatever the case, and the sheets already there keep theirs.
        /// </summary>
        public static string WhyRefused(string reportSheetName, string coverageSheetName, string sheetName)
        {
            if (string.Equals(reportSheetName, sheetName, StringComparison.OrdinalIgnoreCase))
            {
                return "no Generic Models sheet was written, because the report's own sheet is named " + reportSheetName
                    + " and this sheet would be named " + sheetName + ", which Excel reads as one name";
            }

            if (string.Equals(coverageSheetName, sheetName, StringComparison.OrdinalIgnoreCase))
            {
                return "no Generic Models sheet was written, because the Coverage sheet is named " + coverageSheetName
                    + " and this sheet would be named " + sheetName + ", which Excel reads as one name";
            }

            return null;
        }

        /// <summary>
        /// The table's rows, a model with items first in the plan's order and a model not counted after
        /// them, each an array of its three cells: the model, the count as a number or the word UNKNOWN,
        /// and why not. A model counted at nought is not a row.
        /// </summary>
        public static IList<object[]> Rows(GenericModelsReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            List<object[]> rows = new List<object[]>();

            foreach (GenericModelCount count in report.Counts)
            {
                if (count.Counted && count.Items > 0)
                {
                    rows.Add(new object[] { count.ModelName, count.Items, string.Empty });
                }
            }

            foreach (GenericModelCount count in report.Counts)
            {
                if (!count.Counted)
                {
                    rows.Add(new object[] { count.ModelName, GenericModelsReport.Unknown, count.WhyNotCounted });
                }
            }

            return rows;
        }

        /// <summary>
        /// Writes the sheet into that open workbook, after the sheets it holds. The caller saves. The name
        /// goes through the same floor as every sheet name here, so one Excel refuses never loses the workbook.
        /// </summary>
        internal static void Write(XLWorkbook workbook, GenericModelsReport report, string group, string sheetName)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException("workbook");
            }

            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            IXLWorksheet sheet = workbook.Worksheets.Add(
                SheetNames.Sanitise(sheetName, GenericModelsSettings.DefaultSheetName));
            int row = 1;

            sheet.Cell(row, 1).Value = Title;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
            sheet.Cell(row, 1).SetValue("Group: " + (string.IsNullOrEmpty(group) ? GenericModelsReport.Unknown : group));
            sheet.Cell(row, 2).SetValue(report.Asked + ", found in each model by its Source File");
            row++;
            sheet.Cell(row, 1).SetValue(Summary(report));
            row += 2;

            for (int column = 0; column < Headings.Length; column++)
            {
                sheet.Cell(row, column + 1).Value = Headings[column];
                sheet.Cell(row, column + 1).Style.Font.Bold = true;
            }

            row++;

            foreach (object[] cells in Rows(report))
            {
                // The model is text on purpose, so a name that looks like a number or a date stays as the
                // file carries it. The count is a number so it sorts, and UNKNOWN stays the word.
                sheet.Cell(row, 1).SetValue((string)cells[0]);

                if (cells[1] is int)
                {
                    sheet.Cell(row, 2).SetValue((int)cells[1]);
                }
                else
                {
                    sheet.Cell(row, 2).SetValue((string)cells[1]);
                }

                sheet.Cell(row, 3).SetValue((string)cells[2]);
                row++;
            }

            row++;

            foreach (string note in report.Notes)
            {
                sheet.Cell(row, 1).SetValue("note: " + note);
                row++;
            }

            sheet.Cell(row, 1).SetValue("No clash test is made for these sets");

            for (int column = 1; column <= Widths.Length; column++)
            {
                sheet.Column(column).Width = Widths[column - 1];
            }
        }

        /// <summary>The line under the title: how many models of each kind, and the items counted.</summary>
        public static string Summary(GenericModelsReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            if (report.Models == 0)
            {
                return "No model of this group was planned, so nothing was counted";
            }

            return report.Models + (report.Models == 1 ? " model" : " models") + ": " + report.WithItems
                + " hold some, " + report.WithNone + " hold none and are left out, " + report.NotCounted
                + " not counted. " + (report.NotCounted > 0 ? "At least " : string.Empty) + report.Items + " items in all";
        }
    }
}
