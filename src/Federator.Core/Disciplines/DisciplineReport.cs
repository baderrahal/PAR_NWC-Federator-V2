using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Naming;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// The discipline reports of one package, F209 and since F234 Bader's round of 19 Oct 2026, fix 4: the seven checks over
    /// every group of the package, one workbook per discipline, AR, ST, ME and EL by default. Each workbook holds the rows of
    /// its discipline that need action, sorted by severity, then building, then file, and a summary of each check with the
    /// files it read and its issues, which are the rows of that check in the same workbook. A row of a discipline that gets no
    /// workbook, a code outside the setting's list or a name that does not read, is counted in the log and never dropped.
    /// </summary>
    public sealed class DisciplineReport
    {
        private readonly Dictionary<string, List<DisciplineIssue>> byDiscipline = new Dictionary<string, List<DisciplineIssue>>(StringComparer.Ordinal);
        private readonly List<DisciplineIssue> leftOut = new List<DisciplineIssue>();
        private readonly List<CheckFindings> checks = new List<CheckFindings>();
        private readonly DisciplineReportSettings settings;

        private DisciplineReport(string community, string package, DateTime runAt, DisciplineReportSettings settings)
        {
            Community = community ?? string.Empty;
            Package = package ?? string.Empty;
            RunAt = runAt;
            this.settings = settings;

            foreach (string discipline in settings.Disciplines)
            {
                byDiscipline[discipline] = new List<DisciplineIssue>();
            }
        }

        public string Community { get; private set; }

        /// <summary>The package's name as row 1 says it, the path below the community or the words for its own files.</summary>
        public string Package { get; private set; }

        public DateTime RunAt { get; private set; }

        public static DisciplineReport For(
            string community,
            string package,
            IList<DisciplineGroup> groups,
            IList<FileDate> dates,
            DateTime now,
            DisciplineReportSettings settings,
            ContainerNameSettings names)
        {
            if (groups == null)
            {
                throw new ArgumentNullException("groups");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            DisciplineReport report = new DisciplineReport(community, package, now, settings);
            report.checks.Add(CoordinatesCheck.Of(groups, settings, names));
            report.checks.Add(GroupDisciplinesCheck.Missing(groups, settings, names));
            report.checks.Add(GroupDisciplinesCheck.OneDiscipline(groups, names));
            report.checks.Add(ReferenceNaming.Of(groups, settings, names));
            report.checks.Add(FileAgeCheck.Of(groups, dates, now, settings, names));
            report.checks.Add(ElementChecks.Cad(groups, settings, names));
            report.checks.Add(ElementChecks.Generic(groups, settings, names));

            foreach (CheckFindings check in report.checks)
            {
                foreach (DisciplineIssue issue in check.Issues)
                {
                    List<DisciplineIssue> into;

                    if (report.byDiscipline.TryGetValue(issue.Discipline, out into))
                    {
                        into.Add(issue);
                    }
                    else
                    {
                        report.leftOut.Add(issue);
                    }
                }
            }

            foreach (List<DisciplineIssue> issues in report.byDiscipline.Values)
            {
                Sort(issues);
            }

            return report;
        }

        /// <summary>
        /// Bader's order: severity, High first, then building, then file, Ordinal, and where all three are one the order the
        /// checks made them, so the sort is stable as List.Sort is not.
        /// </summary>
        private static void Sort(List<DisciplineIssue> issues)
        {
            List<KeyValuePair<int, DisciplineIssue>> keyed = new List<KeyValuePair<int, DisciplineIssue>>();

            for (int i = 0; i < issues.Count; i++)
            {
                keyed.Add(new KeyValuePair<int, DisciplineIssue>(i, issues[i]));
            }

            keyed.Sort((a, b) =>
            {
                int by = CheckWords.SeverityRank(a.Value.Severity).CompareTo(CheckWords.SeverityRank(b.Value.Severity));
                by = by != 0 ? by : string.CompareOrdinal(a.Value.Building, b.Value.Building);
                by = by != 0 ? by : string.CompareOrdinal(a.Value.File, b.Value.File);
                return by != 0 ? by : a.Key.CompareTo(b.Key);
            });

            issues.Clear();

            foreach (KeyValuePair<int, DisciplineIssue> pair in keyed)
            {
                issues.Add(pair.Value);
            }
        }

        /// <summary>The disciplines that get a workbook, the setting's order.</summary>
        public IList<string> Disciplines
        {
            get { return settings.Disciplines; }
        }

        public string Reference
        {
            get { return settings.Reference; }
        }

        /// <summary>The rows of one discipline's issues table, sorted, numbered from 1 in this order.</summary>
        public IList<DisciplineIssue> IssuesFor(string discipline)
        {
            List<DisciplineIssue> issues;
            return byDiscipline.TryGetValue(discipline ?? string.Empty, out issues) ? issues.AsReadOnly() : new List<DisciplineIssue>().AsReadOnly();
        }

        /// <summary>The summary table of one discipline's workbook, the seven checks in Bader's order, its issues the rows of that check in the same workbook.</summary>
        public IList<CheckSummary> SummaryFor(string discipline)
        {
            List<CheckSummary> summary = new List<CheckSummary>();
            IList<DisciplineIssue> issues = IssuesFor(discipline);

            foreach (CheckFindings check in checks)
            {
                int count = 0;

                foreach (DisciplineIssue issue in issues)
                {
                    count += string.Equals(issue.Check, check.Check, StringComparison.Ordinal) ? 1 : 0;
                }

                summary.Add(new CheckSummary(check.Check, check.FilesCheckedFor(discipline), count));
            }

            return summary;
        }

        /// <summary>Row 1 of a discipline's workbook.</summary>
        public string TitleFor(string discipline)
        {
            return CheckWords.Title(Community, Package, discipline);
        }

        /// <summary>The rows no workbook takes, a file of a discipline outside the list or whose name does not read.</summary>
        public IList<DisciplineIssue> LeftOut
        {
            get { return leftOut.AsReadOnly(); }
        }

        /// <summary>
        /// The DISCIPLINE REPORT block: per discipline each check's files read and issues, then what each check could not read
        /// and the naming pattern learned, then the rows no workbook takes, counted by their discipline.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (string discipline in settings.Disciplines)
            {
                List<string> words = new List<string>();

                foreach (CheckSummary row in SummaryFor(discipline))
                {
                    words.Add(row.Check + " " + row.FilesChecked.ToString(CultureInfo.InvariantCulture) + " files "
                        + row.Issues.ToString(CultureInfo.InvariantCulture) + " issues");
                }

                lines.Add(discipline + "  " + IssuesFor(discipline).Count.ToString(CultureInfo.InvariantCulture) + " issue rows, "
                    + string.Join(", ", words.ToArray()));
            }

            foreach (CheckFindings check in checks)
            {
                foreach (string note in check.Notes)
                {
                    lines.Add(note);
                }
            }

            if (leftOut.Count > 0)
            {
                Dictionary<string, int> byCode = new Dictionary<string, int>(StringComparer.Ordinal);
                List<string> order = new List<string>();

                foreach (DisciplineIssue issue in leftOut)
                {
                    string code = issue.Discipline.Length == 0 ? "a name that does not read" : issue.Discipline;
                    int was;

                    if (!byCode.TryGetValue(code, out was))
                    {
                        order.Add(code);
                    }

                    byCode[code] = was + 1;
                }

                foreach (string code in order)
                {
                    lines.Add("left out  " + byCode[code] + " rows of " + code + ", which gets no workbook, the disciplines being "
                        + string.Join(", ", new List<string>(settings.Disciplines).ToArray()));
                }
            }
            else
            {
                lines.Add("left out  none, every row is in a workbook");
            }

            return lines;
        }
    }

    /// <summary>
    /// One discipline's workbook of one package, F209 and since F234 Bader's layout: one sheet named after the discipline,
    /// rows 1 to 3 the title, the run date and the reference discipline, the summary table, one blank row, then the issues
    /// table, its header row frozen and carrying the filter, a High row light red and a Medium row light amber. A file of that
    /// name in the folder is never written over, ReportPaths.FreeFilePath. Read back as the file it is, never through the
    /// object that wrote it.
    /// </summary>
    public static class DisciplineWorkbook
    {
        /// <summary>The first row of the summary table, its header.</summary>
        public const int SummaryHeaderRow = 4;

        /// <summary>The light red of a High row and the light amber of a Medium row, and the grey of a header.</summary>
        public static readonly XLColor HighFill = XLColor.FromHtml("#FFC7CE");
        public static readonly XLColor MediumFill = XLColor.FromHtml("#FFEB9C");
        public static readonly XLColor HeaderFill = XLColor.FromHtml("#D9D9D9");

        /// <summary>The widths of columns A to I, chosen so the issues table's words and the summary's checks show whole.</summary>
        public static readonly double[] Widths = { 22, 12, 12, 46, 22, 44, 40, 42, 40 };

        /// <summary>The row of the issues table's header: one blank row after the summary of the seven checks.</summary>
        public static int IssuesHeaderRow
        {
            get { return SummaryHeaderRow + CheckWords.All.Length + 2; }
        }

        /// <summary>The name of a package's workbook for a discipline: the package's path from the parent in words, the discipline and the suffix.</summary>
        public static string NameFor(string packagePath, string discipline, DisciplineReportSettings settings)
        {
            string words = (packagePath ?? string.Empty).Replace(Path.DirectorySeparatorChar, ' ').Replace(Path.AltDirectorySeparatorChar, ' ').Trim();
            return (words.Length == 0 ? string.Empty : words + " ") + discipline + " " + settings.WorkbookSuffix + ".xlsx";
        }

        public static void Write(string path, DisciplineReport report, string discipline)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            IList<CheckSummary> summary = report.SummaryFor(discipline);
            IList<DisciplineIssue> issues = report.IssuesFor(discipline);

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet sheet = workbook.Worksheets.Add(SheetName(discipline));
                sheet.Cell(1, 1).SetValue(report.TitleFor(discipline));
                sheet.Cell(1, 1).Style.Font.Bold = true;
                sheet.Cell(1, 1).Style.Font.FontSize = 14;
                sheet.Cell(2, 1).SetValue(CheckWords.RunDate(report.RunAt));
                sheet.Cell(3, 1).SetValue(CheckWords.ReferenceLine(report.Reference));

                Header(sheet, SummaryHeaderRow, CheckWords.SummaryColumns);

                for (int r = 0; r < summary.Count; r++)
                {
                    int row = SummaryHeaderRow + 1 + r;
                    sheet.Cell(row, 1).SetValue(summary[r].Check);
                    sheet.Cell(row, 2).SetValue(summary[r].FilesChecked);
                    sheet.Cell(row, 3).SetValue(summary[r].Issues);
                    sheet.Cell(row, 4).SetValue(summary[r].Result);
                }

                int header = IssuesHeaderRow;
                Header(sheet, header, CheckWords.IssueColumns);

                for (int r = 0; r < issues.Count; r++)
                {
                    int row = header + 1 + r;
                    IList<string> cells = issues[r].Cells(r + 1);
                    sheet.Cell(row, 1).SetValue(r + 1);

                    for (int c = 1; c < cells.Count; c++)
                    {
                        sheet.Cell(row, c + 1).SetValue(cells[c]);
                    }

                    IXLRange line = sheet.Range(row, 1, row, CheckWords.IssueColumns.Length);
                    line.Style.Alignment.WrapText = true;
                    line.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                    if (issues[r].Severity == CheckWords.High)
                    {
                        line.Style.Fill.BackgroundColor = HighFill;
                    }
                    else if (issues[r].Severity == CheckWords.Medium)
                    {
                        line.Style.Fill.BackgroundColor = MediumFill;
                    }
                }

                sheet.Range(header, 1, header + Math.Max(1, issues.Count), CheckWords.IssueColumns.Length).SetAutoFilter();
                sheet.SheetView.FreezeRows(header);

                for (int c = 0; c < Widths.Length; c++)
                {
                    sheet.Column(c + 1).Width = Widths[c];
                }

                workbook.SaveAs(path);
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

        /// <summary>A workbook read back off the disk, or null with why where it would not read.</summary>
        public static DisciplineWorkbookRead Read(string path, out string why)
        {
            why = null;

            try
            {
                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    IXLWorksheet sheet = workbook.Worksheet(1);
                    DisciplineWorkbookRead read = new DisciplineWorkbookRead(sheet.Cell(1, 1).GetString());

                    for (int r = SummaryHeaderRow + 1; r <= SummaryHeaderRow + CheckWords.All.Length; r++)
                    {
                        int issues;
                        string text = sheet.Cell(r, 3).GetString();

                        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out issues))
                        {
                            why = "the summary's Issues cell of row " + r + " holds " + text + ", not a count";
                            return null;
                        }

                        read.AddSummary(sheet.Cell(r, 1).GetString(), issues);
                    }

                    int last = sheet.LastRowUsed() == null ? 0 : sheet.LastRowUsed().RowNumber();

                    for (int r = IssuesHeaderRow + 1; r <= last; r++)
                    {
                        List<string> cells = new List<string>();

                        for (int c = 1; c <= CheckWords.IssueColumns.Length; c++)
                        {
                            cells.Add(sheet.Cell(r, c).GetString());
                        }

                        read.AddIssue(cells);
                    }

                    return read;
                }
            }
            catch (Exception error)
            {
                why = error.GetType().Name + ": " + error.Message;
                return null;
            }
        }

        /// <summary>The sheet's name, the discipline, cut to the 31 characters Excel allows.</summary>
        public static string SheetName(string discipline)
        {
            string name = string.IsNullOrEmpty(discipline) ? "Checks" : discipline;
            return name.Length > 31 ? name.Substring(0, 31) : name;
        }
    }

    /// <summary>The words of the log about a discipline workbook read back.</summary>
    public static class DisciplineWorkbookWords
    {
        /// <summary>
        /// The CHECKS line of a workbook read back: the issue rows read against those written, the issues the summary counts,
        /// and whether each check's count in the summary is the rows of that check, Bader's proof, said in capitals where not.
        /// </summary>
        public static string ReadBackLine(DisciplineWorkbookRead back, int written, string discipline, string package)
        {
            return "CHECKS   read back " + back.Issues.Count.ToString(CultureInfo.InvariantCulture) + " of "
                + written.ToString(CultureInfo.InvariantCulture) + " issue rows, the summary counts "
                + back.SummaryIssues.ToString(CultureInfo.InvariantCulture) + " issues, "
                + (back.SummaryMatchesTheRows ? "each check's issues the rows of that check" : "THE SUMMARY AND THE ROWS DIFFER")
                + ", " + discipline + " of " + package;
        }
    }

    /// <summary>What a discipline workbook holds as read off the disk: its title, each check's issues in the summary, and the issue rows.</summary>
    public sealed class DisciplineWorkbookRead
    {
        private readonly List<KeyValuePair<string, int>> summary = new List<KeyValuePair<string, int>>();
        private readonly List<IList<string>> issues = new List<IList<string>>();

        public DisciplineWorkbookRead(string title)
        {
            Title = title ?? string.Empty;
        }

        public string Title { get; private set; }

        public IList<KeyValuePair<string, int>> Summary
        {
            get { return summary.AsReadOnly(); }
        }

        public IList<IList<string>> Issues
        {
            get { return issues.AsReadOnly(); }
        }

        /// <summary>The issues the summary counts, added over the seven checks.</summary>
        public int SummaryIssues
        {
            get
            {
                int total = 0;

                foreach (KeyValuePair<string, int> row in summary)
                {
                    total += row.Value;
                }

                return total;
            }
        }

        /// <summary>Whether each check's issues in the summary are the rows of that check in the issues table, Bader's proof.</summary>
        public bool SummaryMatchesTheRows
        {
            get
            {
                foreach (KeyValuePair<string, int> row in summary)
                {
                    int rows = 0;

                    foreach (IList<string> issue in issues)
                    {
                        rows += string.Equals(issue[4], row.Key, StringComparison.Ordinal) ? 1 : 0;
                    }

                    if (rows != row.Value)
                    {
                        return false;
                    }
                }

                return SummaryIssues == issues.Count;
            }
        }

        internal void AddSummary(string check, int count)
        {
            summary.Add(new KeyValuePair<string, int>(check, count));
        }

        internal void AddIssue(IList<string> cells)
        {
            issues.Add(cells);
        }
    }
}
