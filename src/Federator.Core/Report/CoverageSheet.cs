using System;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Health;
using Federator.Core.Sets;

namespace Federator.Core.Report
{
    /// <summary>
    /// The Coverage sheet, F127, Bader's request 2 under Q112 and FR-200: the second and last
    /// sheet of the group's workbook, named by CoverageSettings.SheetName, Coverage by default,
    /// written in the same save as the client's sheet so that sheet never goes through a second
    /// round trip. Nothing of ours goes on sheet 1, which stays exactly theirs.
    ///
    /// ONE ROW PER TEST OF THE PICKED FILE, in the file's order, A TEST NOT CREATED INCLUDED,
    /// FR-200, Bader's decision under Q46: F77 keeps a test whose side finds nothing out of the
    /// document on every run, and the sheet names every such test and why, so nothing is missed
    /// without a line saying so. Plain cells, section titles in column A, and AN UNKNOWN VALUE IS
    /// THE WORD UNKNOWN, never an empty cell and never nought, because a count nobody took reads
    /// as a count of nothing otherwise.
    ///
    /// THE ROWS ARE ONE SHAPE, TestRows, read by the writer here and by any check of the sheet,
    /// so the two cannot disagree about a cell. The sets, the items no set catches and the
    /// mirrored pairs the design names are sections of their own: the sets as the sets step
    /// counted them, and the other two said as not in this build, since the count of items no
    /// set catches and F132's mirror rule are not on main.
    /// </summary>
    public static class CoverageSheet
    {
        /// <summary>The word for a number nobody took.</summary>
        public const string Unknown = "UNKNOWN";

        /// <summary>The first words of the sheet, so a reader sees at once it is not part of the client's report.</summary>
        public const string Title = "Coverage, written by the NWC Federator and not part of the client's report";

        /// <summary>The headings of the TESTS table, the design's columns A to P.</summary>
        public static readonly string[] TestHeadings =
        {
            "Position",
            "Test",
            "Priority",
            "Left set",
            "Left items",
            "Right set",
            "Right items",
            "In the document",
            "Run",
            "Results at the top level in Clash Detective",
            "Clashes in Clash Detective",
            "Rows in the workbook",
            "Clashes cell in the workbook",
            "Check",
            "Reason",
            "Detail"
        };

        /// <summary>The headings of the SETS table.</summary>
        public static readonly string[] SetHeadings = { "Set", "Conditions", "Items in this group", "At zero" };

        /// <summary>The section titles, in the order they are written.</summary>
        public const string TestsSection = "TESTS";

        public const string SetsSection = "SETS";

        public const string ItemsSection = "ITEMS NO SET CATCHES";

        public const string MirrorsSection = "MIRRORED PAIRS";

        public const string NotNamedSection = "TESTS IN THE DOCUMENT NOT IN THE XML";

        /// <summary>
        /// Why no Coverage sheet can be written beside that report, or null where it can: a report
        /// whose own sheet is named like the coverage sheet, since Excel refuses two sheets of one
        /// name and the client's sheet keeps its name.
        /// </summary>
        public static string WhyRefused(ClashReport report, string sheetName)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            string theirs = SheetNames.ForReport(report.OutputName);

            if (string.Equals(theirs, sheetName, StringComparison.OrdinalIgnoreCase))
            {
                return "no Coverage sheet was written, because the report's own sheet is named " + theirs
                    + " and the coverage sheet would be named " + sheetName + ", which Excel reads as one name";
            }

            return null;
        }

        /// <summary>
        /// The TESTS rows, one string array per test in the order of the file, each cell the text
        /// the sheet carries. The count check's four numbers and its verdict ride beside the
        /// test where the check holds one test of that name, and read UNKNOWN where the check
        /// was not taken or holds none or two of the name.
        /// </summary>
        public static IList<string[]> TestRows(CoverageSheetData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            Dictionary<string, CountedTest> counted = CountedByName(data.Check);
            List<string[]> rows = new List<string[]>();

            foreach (TestCoverage test in data.Tests)
            {
                CountedTest count;
                counted.TryGetValue(test.Name, out count);

                rows.Add(new[]
                {
                    Number(test.Position),
                    test.Name,
                    data.Priorities.Picked ? Priorities.Words(data.Priorities.Of(test.Name)) : "no priority file",
                    test.LeftSet.Length == 0 ? Unknown : test.LeftSet,
                    Number(test.LeftItems),
                    test.RightSet.Length == 0 ? Unknown : test.RightSet,
                    Number(test.RightItems),
                    CoverageWords.For(test.Presence),
                    test.Ran ? "yes" : "no",
                    count == null || !count.InDocument ? Unknown : Number(count.DocumentTopLevel),
                    count == null || !count.InDocument ? Unknown : Number(count.DocumentLeaves),
                    count == null || !count.InWorkbook ? Unknown : Number(count.WorkbookRows),
                    count == null || !count.InWorkbook ? Unknown : Number(count.WorkbookClashes),
                    count == null ? (data.Check == null ? "NOT COMPARED, the count check was not taken" : "NOT COMPARED, the check holds no one test of this name") : Verdict(count),
                    CoverageWords.For(test.Reason),
                    test.Detail
                });
            }

            return rows;
        }

        /// <summary>
        /// The counts line of the sheet's head, off the rows alone: how many tests, how many in
        /// the document created this run or already there, not created, run, with clashes and
        /// without, each read off TestCoverage and never off another count.
        /// </summary>
        public static string Counts(CoverageSheetData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            int created = 0;
            int alreadyThere = 0;
            int notCreated = 0;
            int unknown = 0;
            int ran = 0;
            int withClashes = 0;

            foreach (TestCoverage test in data.Tests)
            {
                switch (test.Presence)
                {
                    case TestPresence.CreatedThisRun:
                        created++;
                        break;
                    case TestPresence.AlreadyThere:
                        alreadyThere++;
                        break;
                    case TestPresence.NotInDocument:
                        notCreated++;
                        break;
                    default:
                        unknown++;
                        break;
                }

                if (test.Ran)
                {
                    ran++;

                    if (test.Reason == CoverageReason.HasClashes)
                    {
                        withClashes++;
                    }
                }
            }

            return data.Tests.Count + (data.FromAPickedFile ? " tests in the picked file: " : " tests saved in the document: ")
                + created + " created this run, " + alreadyThere + " already there, " + notCreated + " not created"
                + (unknown > 0 ? ", " + unknown + " whose presence is UNKNOWN" : string.Empty)
                + ", " + ran + " run, " + withClashes + " with clashes, " + (ran - withClashes) + " without";
        }

        /// <summary>
        /// Writes the sheet into that open workbook, after the sheets it holds. The caller saves,
        /// so the client's sheet and this one go to the disk in one save.
        /// </summary>
        internal static void Write(XLWorkbook workbook, CoverageSheetData data, string sheetName)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException("workbook");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            IXLWorksheet sheet = workbook.Worksheets.Add(SheetNames.Sanitise(sheetName, CoverageSettings.DefaultSheetName));
            int row = 1;

            sheet.Cell(row, 1).Value = Title;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
            sheet.Cell(row, 1).Value = (data.FromAPickedFile ? "Picked file: " : "Source: ") + data.Source;
            sheet.Cell(row, 2).Value = "sha256: " + data.Sha256;
            sheet.Cell(row, 3).Value = "Corrections: " + data.Corrections;
            row++;
            sheet.Cell(row, 1).Value = ModelsLine(data.Models);
            row++;
            sheet.Cell(row, 1).Value = Counts(data);
            row += 2;

            row = Section(sheet, row, TestsSection, TestHeadings, TestRows(data));
            row = Section(sheet, row, SetsSection, SetHeadings, SetRows(data));

            sheet.Cell(row, 1).Value = ItemsSection;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
            sheet.Cell(row, 1).Value = "UNKNOWN, the count of items no set catches is not in this build";
            row += 2;

            sheet.Cell(row, 1).Value = MirrorsSection;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;
            sheet.Cell(row, 1).Value = "UNKNOWN, the mirror rule of F132 is not on main, so no pair was judged";
            row += 2;

            sheet.Cell(row, 1).Value = NotNamedSection;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            foreach (string line in NotNamedLines(data))
            {
                sheet.Cell(row, 1).Value = line;
                row++;
            }

            for (int column = 1; column <= TestHeadings.Length; column++)
            {
                sheet.Column(column).Width = Widths[column - 1];
            }
        }

        /// <summary>
        /// The width of each column of the TESTS table, in Excel's units, chosen so a heading and
        /// a set locator read without a resize and measured by nothing, because no report of the
        /// client's has these columns.
        /// </summary>
        public static readonly double[] Widths =
            { 9, 48, 12, 48, 11, 48, 11, 16, 6, 14, 14, 14, 14, 30, 60, 60 };

        private static int Section(IXLWorksheet sheet, int row, string title, string[] headings, IList<string[]> rows)
        {
            sheet.Cell(row, 1).Value = title;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            for (int column = 0; column < headings.Length; column++)
            {
                sheet.Cell(row, column + 1).Value = headings[column];
                sheet.Cell(row, column + 1).Style.Font.Bold = true;
            }

            row++;

            foreach (string[] cells in rows)
            {
                for (int column = 0; column < cells.Length; column++)
                {
                    // Text on purpose, so a test name or a set locator that looks like a number
                    // or a date stays exactly as the file carries it.
                    sheet.Cell(row, column + 1).SetValue(cells[column] ?? string.Empty);
                }

                row++;
            }

            return row + 1;
        }

        /// <summary>The SETS rows, or one row saying no set was handed in.</summary>
        public static IList<string[]> SetRows(CoverageSheetData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            List<string[]> rows = new List<string[]>();

            if (data.Sets == null)
            {
                rows.Add(new[] { "UNKNOWN, the sets step handed in no result for this group", Unknown, Unknown, Unknown });
                return rows;
            }

            foreach (SetResult set in data.Sets)
            {
                if (set == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    set.Path,
                    Number(set.ConditionCount),
                    Number(set.ItemCount),
                    set.ItemCount < 0 ? Unknown : (set.ItemCount == 0 ? "yes" : "no")
                });
            }

            return rows;
        }

        /// <summary>The names Clash Detective holds that the file does not, or why none is listed.</summary>
        public static IList<string> NotNamedLines(CoverageSheetData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            List<string> lines = new List<string>();

            if (data.Check == null)
            {
                lines.Add("UNKNOWN, the count check was not taken, so Clash Detective's tests were not set beside the file's");
                return lines;
            }

            if (data.Check.NotInTheXml.Count == 0)
            {
                lines.Add("none");
                return lines;
            }

            foreach (string name in data.Check.NotInTheXml)
            {
                lines.Add(name);
            }

            return lines;
        }

        private static string ModelsLine(IList<ModelExport> models)
        {
            if (models.Count == 0)
            {
                return "Models: UNKNOWN, none was handed in";
            }

            List<string> parts = new List<string>();

            foreach (ModelExport model in models)
            {
                if (model == null)
                {
                    continue;
                }

                parts.Add(model.File + " (" + (string.IsNullOrEmpty(model.Discipline) ? Unknown : model.Discipline) + ", "
                    + (model.Elements < 0 ? Unknown : Number(model.Elements) + " elements") + ")");
            }

            return "Models: " + string.Join("; ", parts.ToArray());
        }

        private static Dictionary<string, CountedTest> CountedByName(CountCheck check)
        {
            Dictionary<string, CountedTest> byName = new Dictionary<string, CountedTest>(StringComparer.Ordinal);

            if (check == null)
            {
                return byName;
            }

            HashSet<string> twice = new HashSet<string>(StringComparer.Ordinal);

            foreach (CountedTest test in check.Tests)
            {
                if (byName.ContainsKey(test.Name))
                {
                    twice.Add(test.Name);
                    continue;
                }

                byName.Add(test.Name, test);
            }

            // A name on two tests was judged on neither, and the sheet says so on each.
            foreach (string name in twice)
            {
                byName.Remove(name);
            }

            return byName;
        }

        private static string Verdict(CountedTest test)
        {
            switch (test.Verdict)
            {
                case CountVerdict.Agree:
                    return "AGREE";
                case CountVerdict.Failed:
                    return "FAILED" + (test.Why.Length == 0 ? string.Empty : ", " + test.Why);
                case CountVerdict.HeldByNeither:
                    return "held by neither side";
                default:
                    return "NOT COMPARED" + (test.Why.Length == 0 ? string.Empty : ", " + test.Why);
            }
        }

        private static string Number(int value)
        {
            return value < 0 ? Unknown : value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
