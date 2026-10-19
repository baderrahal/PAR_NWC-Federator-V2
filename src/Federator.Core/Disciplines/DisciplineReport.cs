using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Naming;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// The discipline reports of one package, F209, F234 and since F244 Bader's order of 19 Oct 2026: the six checks over every
    /// group of the package, one workbook per discipline, AR, ST, ME and EL by default, each with its Summary, one row per model,
    /// one row per thing to fix and one row per element id. A row of a discipline that gets no workbook, a code outside the
    /// setting's list or a name that does not read, is counted in the log and never dropped.
    /// </summary>
    public sealed class DisciplineReport
    {
        private readonly List<CheckFindings> checks = new List<CheckFindings>();
        private readonly List<DisciplineIssue> leftOut = new List<DisciplineIssue>();
        private readonly DisciplineReportSettings settings;
        private ModelRows rows;
        private ReferenceNaming naming;

        private DisciplineReport(string community, string package, DateTime runAt, DisciplineReportSettings settings)
        {
            Community = community ?? string.Empty;
            Package = package ?? string.Empty;
            RunAt = runAt;
            this.settings = settings;
        }

        public string Community { get; private set; }

        /// <summary>The package's name as the titles say it, the path below the community or the words for its own files.</summary>
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
            report.rows = ModelRows.Of(groups, names);
            report.checks.Add(CoordinatesCheck.Of(groups, report.rows, settings, names));
            report.checks.Add(GroupDisciplinesCheck.Missing(groups, report.rows, settings, names));
            report.checks.Add(ReferenceNaming.Of(groups, report.rows, settings, names, out report.naming));
            report.checks.Add(FileAgeCheck.Of(groups, report.rows, dates, now, settings, names));
            report.checks.Add(ElementChecks.Cad(groups, report.rows, settings));
            report.checks.Add(ElementChecks.Generic(groups, report.rows, settings));

            foreach (CheckFindings check in report.checks)
            {
                foreach (DisciplineIssue issue in check.Issues)
                {
                    if (!settings.Disciplines.Contains(issue.Discipline))
                    {
                        report.leftOut.Add(issue);
                    }
                }
            }

            return report;
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

        /// <summary>The setting's words in a cell whose value was not read, an element id among them.</summary>
        public string NotReadable
        {
            get { return settings.NotReadable; }
        }

        /// <summary>The words before the discipline in each sheet's title.</summary>
        public string Heading
        {
            get { return CheckWords.Heading(Community, Package); }
        }

        /// <summary>
        /// The rows of To fix of one discipline, Bader's order: High, Medium, Low, then building, model and check, Ordinal, and
        /// where all are one the order the checks made them, so the sort is stable as List.Sort is not.
        /// </summary>
        public IList<DisciplineIssue> IssuesFor(string discipline)
        {
            List<DisciplineIssue> issues = Unsorted(discipline);
            StableSort(issues, (a, b) =>
            {
                int by = CheckWords.SeverityRank(a.Priority).CompareTo(CheckWords.SeverityRank(b.Priority));
                by = by != 0 ? by : string.CompareOrdinal(a.Building, b.Building);
                by = by != 0 ? by : string.CompareOrdinal(a.Row.Model ?? string.Empty, b.Row.Model ?? string.Empty);
                return by != 0 ? by : string.CompareOrdinal(CheckWords.RowWord(a.Check), CheckWords.RowWord(b.Check));
            });

            return issues.AsReadOnly();
        }

        /// <summary>The rows of Models of one discipline, each NWC of it and each building with none, by building, then file name.</summary>
        public IList<ModelRow> ModelsFor(string discipline)
        {
            List<ModelRow> models = new List<ModelRow>();

            foreach (ModelRow row in rows.All)
            {
                if (row.Discipline == discipline)
                {
                    models.Add(row);
                }
            }

            StableSort(models, (a, b) =>
            {
                int by = string.CompareOrdinal(a.Building, b.Building);
                return by != 0 ? by : string.CompareOrdinal(a.File, b.File);
            });

            return models.AsReadOnly();
        }

        /// <summary>
        /// The six rows of the Summary of one discipline. Checked is the models, or for Missing model the buildings, the check
        /// judged, With issue those of them with a row of that check on To fix. A check that judged none, and Naming in the
        /// reference discipline's own workbook, is NOT CHECKED with both cells blank, never a pass.
        /// </summary>
        public IList<SummaryRow> SummaryFor(string discipline)
        {
            List<SummaryRow> summary = new List<SummaryRow>();
            List<DisciplineIssue> issues = Unsorted(discipline);

            foreach (CheckFindings check in checks)
            {
                bool ownPattern = check.Check == CheckWords.Naming && discipline == settings.Reference;
                int judged = check.CheckedFor(discipline);
                HashSet<ModelRow> withIssue = new HashSet<ModelRow>();

                foreach (DisciplineIssue issue in issues)
                {
                    if (issue.Check == check.Check)
                    {
                        withIssue.Add(issue.Row);
                    }
                }

                string what = CheckWords.WhatIsChecked(check.Check, discipline, settings.FarMetres, settings.OlderThanDays,
                    naming.WasLearned ? CheckWords.NameIs(naming.Pattern(discipline)) : naming.Describe());

                if (ownPattern || judged == 0)
                {
                    summary.Add(new SummaryRow(check.Check, CheckWords.UnitOf(check.Check), null, null, CheckWords.NotChecked, what,
                        ownPattern ? CheckWords.SetThePattern(settings.Reference) : null));
                    continue;
                }

                string status = withIssue.Count == 0 ? CheckWords.Ok
                    : CheckWords.PriorityOf(check.Check) == CheckWords.High ? CheckWords.FixStatus : CheckWords.Review;
                summary.Add(new SummaryRow(check.Check, CheckWords.UnitOf(check.Check), judged, withIssue.Count, status, what,
                    withIssue.Count == 0 ? null : CheckWords.SummaryFix(check.Check, discipline)));
            }

            return summary;
        }

        /// <summary>
        /// The rows of Element IDs of one discipline: by file name, each model's CAD imports then its Generic Models in the order
        /// found, one element a row and each id once a model, an id not read on a row of its own.
        /// </summary>
        public IList<ElementIdRow> ElementIdsFor(string discipline)
        {
            List<DisciplineIssue> issues = Unsorted(discipline).FindAll(i => i.Check == CheckWords.Cad || i.Check == CheckWords.Generic);
            StableSort(issues, (a, b) => string.CompareOrdinal(a.Row.File, b.Row.File));
            List<ElementIdRow> ids = new List<ElementIdRow>();
            Dictionary<ModelRow, HashSet<string>> seen = new Dictionary<ModelRow, HashSet<string>>();

            foreach (DisciplineIssue issue in issues)
            {
                HashSet<string> once;

                if (!seen.TryGetValue(issue.Row, out once))
                {
                    once = new HashSet<string>(StringComparer.Ordinal);
                    seen.Add(issue.Row, once);
                }

                foreach (string id in issue.ElementIds)
                {
                    if (id == null || once.Add(id))
                    {
                        ids.Add(new ElementIdRow(issue.Building, issue.Row.Model, CheckWords.RowWord(issue.Check), issue.Source, issue.Type, id));
                    }
                }
            }

            return ids.AsReadOnly();
        }

        /// <summary>The grey line under Tabs: the buildings of this discipline's models whose reference is not the setting's, by the discipline that is.</summary>
        public string ReferencesLineFor(string discipline)
        {
            SortedDictionary<string, IList<string>> byReference = new SortedDictionary<string, IList<string>>(StringComparer.Ordinal);

            foreach (ModelRow row in ModelsFor(discipline))
            {
                if (row.ComparedWith == null || row.ComparedWith == settings.Reference)
                {
                    continue;
                }

                IList<string> buildings;

                if (!byReference.TryGetValue(row.ComparedWith, out buildings))
                {
                    buildings = new List<string>();
                    byReference.Add(row.ComparedWith, buildings);
                }

                if (!buildings.Contains(row.Building))
                {
                    buildings.Add(row.Building);
                }
            }

            return CheckWords.ReferencesLine(settings.Reference, new List<KeyValuePair<string, IList<string>>>(byReference));
        }

        /// <summary>
        /// Whether every model of a discipline was judged by every check that judges it, so the Summary may say they all pass:
        /// none of a group the run did not build and none a check could not read.
        /// </summary>
        public bool EveryModelChecked(string discipline)
        {
            int models = 0;

            foreach (ModelRow row in ModelsFor(discipline))
            {
                if (!row.IsMissing && !row.Checked)
                {
                    return false;
                }

                models += row.IsMissing ? 0 : 1;
            }

            foreach (SummaryRow row in SummaryFor(discipline))
            {
                bool ownPattern = row.Check == CheckWords.Naming && discipline == settings.Reference;

                if (row.Check != CheckWords.Missing && !ownPattern && row.Checked != models)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>The rows no workbook takes, a file of a discipline outside the list or whose name does not read.</summary>
        internal IList<DisciplineIssue> LeftOut
        {
            get { return leftOut.AsReadOnly(); }
        }

        /// <summary>
        /// The DISCIPLINE REPORT block: per discipline its rows of To fix and Element IDs and each check's Checked and With issue,
        /// then what each check could not read, the naming pattern learned and each part a name breaks, then the rows no
        /// workbook takes, counted by their discipline.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (string discipline in settings.Disciplines)
            {
                List<string> words = new List<string>();

                foreach (SummaryRow row in SummaryFor(discipline))
                {
                    words.Add(row.Check + " " + (row.Checked.HasValue
                        ? CheckWords.Count(row.Checked.Value, row.Unit, row.Unit + "s") + " " + row.WithIssue.Value.ToString(CultureInfo.InvariantCulture) + " with issue"
                        : "not checked"));
                }

                lines.Add(discipline + "  " + IssuesFor(discipline).Count.ToString(CultureInfo.InvariantCulture) + " rows to fix, "
                    + ElementIdsFor(discipline).Count.ToString(CultureInfo.InvariantCulture) + " element IDs, " + string.Join(", ", words.ToArray()));
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

        private List<DisciplineIssue> Unsorted(string discipline)
        {
            List<DisciplineIssue> issues = new List<DisciplineIssue>();

            foreach (CheckFindings check in checks)
            {
                foreach (DisciplineIssue issue in check.Issues)
                {
                    if (issue.Discipline == (discipline ?? string.Empty))
                    {
                        issues.Add(issue);
                    }
                }
            }

            return issues;
        }

        private static void StableSort<T>(List<T> items, Comparison<T> by)
        {
            List<KeyValuePair<int, T>> keyed = new List<KeyValuePair<int, T>>();

            for (int i = 0; i < items.Count; i++)
            {
                keyed.Add(new KeyValuePair<int, T>(i, items[i]));
            }

            keyed.Sort((a, b) =>
            {
                int found = by(a.Value, b.Value);
                return found != 0 ? found : a.Key.CompareTo(b.Key);
            });

            items.Clear();

            foreach (KeyValuePair<int, T> pair in keyed)
            {
                items.Add(pair.Value);
            }
        }
    }

    /// <summary>
    /// One discipline's workbook of one package, F244, Bader's order of 19 Oct 2026, copied from the mock in docs\design, whose
    /// script is the spec of every colour, size and word: Summary, Models, To fix and Element IDs, each with the navy band, its
    /// table from column B, numbers stored as numbers, no formula and no merged cell. A file of that name in the folder is
    /// overwritten since F243. Read back as the file it is, never through the object that wrote it.
    /// </summary>
    public static class DisciplineWorkbook
    {
        public const string SummarySheet = "Summary";
        public const string ModelsSheet = "Models";
        public const string ToFixSheet = "To fix";
        public const string ElementIdsSheet = "Element IDs";

        /// <summary>The header row of Models, To fix and Element IDs. The Summary's is SummaryHeaderRow.</summary>
        public const int HeaderRow = 5;

        public const int SummaryHeaderRow = 7;

        /// <summary>A distance under this many metres is written under 1, Bader's order.</summary>
        public const double UnderMetres = 1.0;

        private const string Body = "Franklin Gothic Book";
        private const string Demi = "Franklin Gothic Demi";

        private static readonly XLColor Navy = XLColor.FromHtml("#081931");
        private static readonly XLColor Cyan = XLColor.FromHtml("#00AEE6");
        private static readonly XLColor Green = XLColor.FromHtml("#50BF34");
        private static readonly XLColor Red = XLColor.FromHtml("#C62828");
        private static readonly XLColor Amber = XLColor.FromHtml("#F2A900");
        private static readonly XLColor Ink = XLColor.FromHtml("#1F2A37");
        private static readonly XLColor Muted = XLColor.FromHtml("#6B7785");
        private static readonly XLColor HeaderFill = XLColor.FromHtml("#E8EDF4");
        private static readonly XLColor BandFill = XLColor.FromHtml("#F6F8FB");
        private static readonly XLColor Line = XLColor.FromHtml("#D5DBE3");
        private static readonly XLColor HighFill = XLColor.FromHtml("#FDE2E1");
        private static readonly XLColor HighInk = XLColor.FromHtml("#9B1C1C");
        private static readonly XLColor LowFill = XLColor.FromHtml("#FFF4D6");
        private static readonly XLColor LowInk = XLColor.FromHtml("#7A5300");
        private static readonly XLColor NotCheckedFill = XLColor.FromHtml("#BFC5CC");
        private static readonly XLColor IdsTab = XLColor.FromHtml("#8A94A3");

        public static readonly string[] SummaryColumns = { "Check", "What is checked", "Checked", "With issue", "Status", "What the modeller does" };
        public static readonly double[] SummaryWidths = { 18, 46, 12, 12, 12, 52 };

        public static readonly string[] ModelsColumns =
        {
            "Building", "Model", "Status", "Compared with", "Off the reference (m)", "Naming", "File age (days)", "CAD imports (files)",
            "Generic Models (elements)", "File name"
        };

        public static readonly double[] ModelsWidths = { 11, 12, 11, 12, 13, 10, 11, 13, 15, 46 };
        public static readonly string[] ToFixColumns = { "No.", "Priority", "Building", "Model", "Check", "Finding", "Value", "Unit", "Fix" };
        public static readonly double[] ToFixWidths = { 6, 10, 11, 12, 15, 52, 10, 10, 50 };
        public static readonly string[] ElementIdsColumns = { "Building", "Model", "Check", "CAD file or family", "Type", "Element ID" };
        public static readonly double[] ElementIdsWidths = { 11, 12, 16, 44, 32, 13 };

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

            using (XLWorkbook workbook = new XLWorkbook())
            {
                WriteSummary(workbook.Worksheets.Add(SummarySheet), report, discipline);
                WriteModels(workbook.Worksheets.Add(ModelsSheet), report, discipline);
                WriteToFix(workbook.Worksheets.Add(ToFixSheet), report, discipline);
                WriteElementIds(workbook.Worksheets.Add(ElementIdsSheet), report, discipline);
                workbook.SaveAs(path);
            }
        }

        private static void WriteSummary(IXLWorksheet sheet, DisciplineReport report, string discipline)
        {
            IList<SummaryRow> summary = report.SummaryFor(discipline);
            IList<ModelRow> models = report.ModelsFor(discipline);
            int count = 0;
            int fix = 0;
            int missing = 0;
            int review = 0;
            List<string> buildings = new List<string>();

            foreach (ModelRow row in models)
            {
                if (row.IsMissing)
                {
                    missing++;
                    continue;
                }

                count++;
                fix += row.Status == CheckWords.FixStatus ? 1 : 0;
                review += row.PriorityOf(CheckWords.Cad) != null || row.PriorityOf(CheckWords.Generic) != null ? 1 : 0;

                if (!buildings.Contains(row.Building))
                {
                    buildings.Add(row.Building);
                }
            }

            int last = SummaryColumns.Length + 1;
            Banner(sheet, Navy, last + 1,
                CheckWords.Title(report.Heading, discipline, "model checks"),
                CheckWords.RunLine(report.RunAt, report.Reference, discipline, count, buildings.Count));
            IXLCell lead = sheet.Cell(5, 2);
            lead.SetValue(CheckWords.Lead(discipline, fix, count, missing, review, report.EveryModelChecked(discipline)));
            Font(lead, Demi, 12, Navy);
            sheet.Row(5).Height = 22;
            Header(sheet, SummaryHeaderRow, SummaryColumns, SummaryWidths, 30);

            for (int i = 0; i < summary.Count; i++)
            {
                SummaryRow row = summary[i];
                int r = SummaryHeaderRow + 1 + i;
                bool band = i % 2 == 1;
                string many = row.Unit + "s";
                Cell(sheet.Cell(r, 2), row.Check, band, XLAlignmentHorizontalValues.Left, Navy, false).Style.Font.FontName = Demi;
                Cell(sheet.Cell(r, 3), row.WhatIsChecked, band, XLAlignmentHorizontalValues.Left, Ink, true);
                Number(sheet.Cell(r, 4), row.Checked, "[=1]0\" " + row.Unit + "\";0\" " + many + "\"", band, XLAlignmentHorizontalValues.Right, Ink);
                // Bader's decision of 20 Oct 2026: the mock's second section read as negative numbers, so above 1 printed none.
                Number(sheet.Cell(r, 5), row.WithIssue, "[=1]0\" " + row.Unit + "\";[>1]0\" " + many + "\";\"none\"", band, XLAlignmentHorizontalValues.Right, Ink);

                if (row.WithIssue > 0)
                {
                    Issue(sheet.Cell(r, 5), row.Status == CheckWords.Review);
                }

                Chip(sheet.Cell(r, 6), row.Status);
                bool nothing = row.Fix.Length == 0;
                Cell(sheet.Cell(r, 7), nothing ? CheckWords.NothingToDo : row.Fix, band, XLAlignmentHorizontalValues.Left,
                    nothing || row.Status == CheckWords.NotChecked ? Muted : Ink, true);
                sheet.Row(r).Height = 30;
            }

            int tabs = SummaryHeaderRow + summary.Count + 2;
            Font(sheet.Cell(tabs, 2).SetValue(CheckWords.Tabs), Demi, 10, Navy);
            IList<KeyValuePair<string, string>> lines = CheckWords.TabLines(discipline);

            for (int i = 0; i < lines.Count; i++)
            {
                Font(sheet.Cell(tabs + 1 + i, 2).SetValue(lines[i].Key), Demi, 10, Cyan);
                Font(sheet.Cell(tabs + 1 + i, 3).SetValue(lines[i].Value), Body, 10, Muted);
            }

            string references = report.ReferencesLineFor(discipline);

            if (references != null)
            {
                IXLCell cell = sheet.Cell(tabs + lines.Count + 2, 2).SetValue(references);
                Font(cell, Body, 10, Muted);
                cell.Style.Font.Italic = true;
            }

            Print(sheet, SummaryHeaderRow);
        }

        private static void WriteModels(IXLWorksheet sheet, DisciplineReport report, string discipline)
        {
            IList<ModelRow> models = report.ModelsFor(discipline);
            int last = ModelsColumns.Length + 1;
            Banner(sheet, Cyan, last + 1, CheckWords.Title(report.Heading, discipline, "models"), CheckWords.ModelsLine);
            Header(sheet, HeaderRow, ModelsColumns, ModelsWidths, 34);
            XLAlignmentHorizontalValues left = XLAlignmentHorizontalValues.Left;
            XLAlignmentHorizontalValues centre = XLAlignmentHorizontalValues.Center;

            for (int i = 0; i < models.Count; i++)
            {
                ModelRow row = models[i];
                int r = HeaderRow + 1 + i;
                bool band = i % 2 == 1;
                Cell(sheet.Cell(r, 2), row.Building, band, left, Ink, false);

                if (row.IsMissing)
                {
                    Cell(sheet.Cell(r, 3), CheckWords.None, band, left, Muted, false);
                    Chip(sheet.Cell(r, 4), row.Status);

                    for (int c = 5; c <= 10; c++)
                    {
                        Cell(sheet.Cell(r, c), null, band, left, Ink, false);
                    }

                    Cell(sheet.Cell(r, 11), CheckWords.ThisBuildingHas(discipline, row.Present), band, left, HighInk, false);
                    continue;
                }

                Cell(sheet.Cell(r, 3), row.Model, band, left, Navy, false).Style.Font.FontName = Demi;
                Chip(sheet.Cell(r, 4), row.Status);
                WriteMeasure(sheet.Cell(r, 5), sheet.Cell(r, 6), row, band);

                IXLCell name = sheet.Cell(r, 7);

                if (!row.NameFollows.HasValue)
                {
                    Cell(name, CheckWords.NameNotChecked, band, centre, Muted, false);
                }
                else
                {
                    Cell(name, row.NameFollows.Value ? CheckWords.NameOk : CheckWords.NameWrong, band, centre, row.NameFollows.Value ? Green : Ink, false);
                    Flag(name, row.PriorityOf(CheckWords.Naming));
                }

                Counted(sheet.Cell(r, 8), row.AgeDays, "0", band, false);
                Flag(sheet.Cell(r, 8), row.PriorityOf(CheckWords.FileAge));
                Counted(sheet.Cell(r, 9), row.CadFiles, "0;;\"-\"", band, true);
                Flag(sheet.Cell(r, 9), row.PriorityOf(CheckWords.Cad));
                Counted(sheet.Cell(r, 10), row.GenericElements, "0;;\"-\"", band, true);
                Flag(sheet.Cell(r, 10), row.PriorityOf(CheckWords.Generic));
                Cell(sheet.Cell(r, 11), row.File, band, left, Muted, false);
            }

            int end = HeaderRow + models.Count;
            sheet.SheetView.Freeze(HeaderRow, 3);
            sheet.Range(HeaderRow, 2, end, last).SetAutoFilter();
            Print(sheet, HeaderRow);
        }

        /// <summary>Compared with and Off the reference of a row of Models, out of what Coordinates read of it, filled as its row of To fix says.</summary>
        private static void WriteMeasure(IXLCell compared, IXLCell off, ModelRow row, bool band)
        {
            XLAlignmentHorizontalValues centre = XLAlignmentHorizontalValues.Center;
            XLAlignmentHorizontalValues right = XLAlignmentHorizontalValues.Right;

            switch (row.Measure)
            {
                case ModelMeasure.Alone:
                    Cell(compared, CheckWords.None, band, centre, Muted, false);
                    Cell(off, CheckWords.Alone, band, right, Muted, false);
                    return;
                case ModelMeasure.Unknown:
                    Cell(compared, CheckWords.Unknown, band, centre, Muted, false);
                    Cell(off, CheckWords.Unknown, band, right, Muted, false);
                    return;
                case ModelMeasure.Reference:
                    Cell(compared, CheckWords.Itself, band, centre, Muted, false);
                    Cell(off, CheckWords.ReferenceWord, band, right, Muted, false);
                    return;
                case ModelMeasure.InternalOrigin:
                    Cell(compared, CheckWords.Itself, band, centre, Muted, false);
                    Cell(off, CheckWords.InternalOrigin, band, right, Ink, false);
                    Flag(off, row.PriorityOf(CheckWords.Coordinates));
                    return;
                default:
                    Cell(compared, row.ComparedWith, band, centre, Ink, false);

                    if (row.Metres < UnderMetres)
                    {
                        Cell(off, CheckWords.Under, band, right, Muted, false);
                    }
                    else
                    {
                        Number(off, row.Metres, "#,##0.0", band, right, Ink);
                    }

                    Flag(off, row.PriorityOf(CheckWords.Coordinates));
                    return;
            }
        }

        private static void WriteToFix(IXLWorksheet sheet, DisciplineReport report, string discipline)
        {
            IList<DisciplineIssue> issues = report.IssuesFor(discipline);
            int last = ToFixColumns.Length + 1;
            Banner(sheet, Red, last + 1, CheckWords.Title(report.Heading, discipline, "to fix"), CheckWords.ToFixLine);
            Header(sheet, HeaderRow, ToFixColumns, ToFixWidths, 30);
            XLAlignmentHorizontalValues left = XLAlignmentHorizontalValues.Left;

            for (int i = 0; i < issues.Count; i++)
            {
                DisciplineIssue issue = issues[i];
                int r = HeaderRow + 1 + i;
                bool band = i % 2 == 1;
                Number(sheet.Cell(r, 2), i + 1, null, band, XLAlignmentHorizontalValues.Right, Muted);
                Chip(sheet.Cell(r, 3), issue.Priority);
                Cell(sheet.Cell(r, 4), issue.Building, band, left, Ink, false);

                if (issue.Row.IsMissing)
                {
                    Cell(sheet.Cell(r, 5), CheckWords.None, band, left, Muted, false);
                }
                else
                {
                    Cell(sheet.Cell(r, 5), issue.Row.Model, band, left, Navy, false).Style.Font.FontName = Demi;
                }

                Cell(sheet.Cell(r, 6), CheckWords.RowWord(issue.Check), band, left, Ink, false);
                Cell(sheet.Cell(r, 7), issue.Finding, band, left, Ink, true);
                Number(sheet.Cell(r, 8), issue.Value, issue.Unit == CheckWords.Metres ? "#,##0.0" : "0", band, XLAlignmentHorizontalValues.Right, Ink);
                Cell(sheet.Cell(r, 9), issue.Unit, band, left, Muted, false);
                Cell(sheet.Cell(r, 10), issue.Fix, band, left, Ink, true);
            }

            sheet.SheetView.FreezeRows(HeaderRow);
            sheet.Range(HeaderRow, 2, HeaderRow + issues.Count, last).SetAutoFilter();
            Print(sheet, HeaderRow);
        }

        private static void WriteElementIds(IXLWorksheet sheet, DisciplineReport report, string discipline)
        {
            IList<ElementIdRow> ids = report.ElementIdsFor(discipline);
            int last = ElementIdsColumns.Length + 1;
            Banner(sheet, IdsTab, last + 1, CheckWords.Title(report.Heading, discipline, "element IDs"), CheckWords.ElementIdsLine);
            Header(sheet, HeaderRow, ElementIdsColumns, ElementIdsWidths, 30);
            XLAlignmentHorizontalValues left = XLAlignmentHorizontalValues.Left;

            for (int i = 0; i < ids.Count; i++)
            {
                ElementIdRow row = ids[i];
                int r = HeaderRow + 1 + i;
                bool band = i % 2 == 1;
                long id;
                Cell(sheet.Cell(r, 2), row.Building, band, left, Ink, false);
                Cell(sheet.Cell(r, 3), row.Model, band, left, Navy, false).Style.Font.FontName = Demi;
                Cell(sheet.Cell(r, 4), row.Check, band, left, Ink, false);
                Cell(sheet.Cell(r, 5), row.Source, band, left, Ink, false);
                Cell(sheet.Cell(r, 6), row.Type, band, left, Muted, false);

                if (row.Id != null && long.TryParse(row.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                {
                    Number(sheet.Cell(r, 7), id, "0", band, XLAlignmentHorizontalValues.Right, Ink);
                }
                else
                {
                    Cell(sheet.Cell(r, 7), row.Id ?? report.NotReadable, band, XLAlignmentHorizontalValues.Right, Muted, false);
                }
            }

            sheet.SheetView.FreezeRows(HeaderRow);
            sheet.Range(HeaderRow, 2, HeaderRow + ids.Count, last).SetAutoFilter();
            Print(sheet, HeaderRow);
        }

        /// <summary>Rows 1 to 3 from column A to one past the table: navy, navy, a thin cyan line. The title and the line under it in B. Gridlines off and column A a margin.</summary>
        private static void Banner(IXLWorksheet sheet, XLColor tab, int lastColumn, string title, string subtitle)
        {
            sheet.TabColor = tab;
            sheet.ShowGridLines = false;
            sheet.Column(1).Width = 2;

            for (int c = 1; c <= lastColumn; c++)
            {
                sheet.Cell(1, c).Style.Fill.BackgroundColor = Navy;
                sheet.Cell(2, c).Style.Fill.BackgroundColor = Navy;
                sheet.Cell(3, c).Style.Fill.BackgroundColor = Cyan;
            }

            sheet.Row(1).Height = 34;
            sheet.Row(2).Height = 20;
            sheet.Row(3).Height = 4;
            IXLCell top = sheet.Cell(1, 2).SetValue(title);
            Font(top, Demi, 18, XLColor.White);
            top.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            IXLCell under = sheet.Cell(2, 2).SetValue(subtitle);
            Font(under, Body, 10, Cyan);
            under.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void Header(IXLWorksheet sheet, int row, string[] columns, double[] widths, double height)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                IXLCell cell = sheet.Cell(row, i + 2).SetValue(columns[i]);
                Font(cell, Demi, 10, Navy);
                cell.Style.Fill.BackgroundColor = HeaderFill;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                cell.Style.Border.BottomBorderColor = Cyan;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
                sheet.Column(i + 2).Width = widths[i];
            }

            sheet.Row(row).Height = height;
        }

        /// <summary>A table cell: its words or empty, the body font, the thin line under it, centred in height, and every second row's band.</summary>
        private static IXLCell Cell(IXLCell cell, string text, bool band, XLAlignmentHorizontalValues horizontal, XLColor ink, bool wrap)
        {
            if (!string.IsNullOrEmpty(text))
            {
                cell.SetValue(text);
            }

            Font(cell, Body, 10, ink);
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = Line;
            cell.Style.Alignment.Horizontal = horizontal;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = wrap;

            if (band)
            {
                cell.Style.Fill.BackgroundColor = BandFill;
            }

            return cell;
        }

        /// <summary>A number stored as a number in its format, or an empty cell in that format where there is none.</summary>
        private static void Number(IXLCell cell, double? value, string format, bool band, XLAlignmentHorizontalValues horizontal, XLColor ink)
        {
            Cell(cell, null, band, horizontal, ink, false);

            if (value.HasValue)
            {
                cell.SetValue(value.Value);
            }

            if (format != null)
            {
                cell.Style.NumberFormat.Format = format;
            }
        }

        /// <summary>A count of Models, centred, grey where nought and dash is its format, or UNKNOWN in grey where it was not read.</summary>
        private static void Counted(IXLCell cell, int? value, string format, bool band, bool greyNought)
        {
            if (!value.HasValue)
            {
                Cell(cell, CheckWords.Unknown, band, XLAlignmentHorizontalValues.Center, Muted, false);
                return;
            }

            Number(cell, value.Value, format, band, XLAlignmentHorizontalValues.Center, greyNought && value.Value == 0 ? Muted : Ink);
        }

        /// <summary>The issue fill of a cell of Models whose model has a row of that check: High red, any other amber.</summary>
        private static void Flag(IXLCell cell, string priority)
        {
            if (priority != null)
            {
                Issue(cell, priority != CheckWords.High);
            }
        }

        private static void Issue(IXLCell cell, bool low)
        {
            cell.Style.Fill.BackgroundColor = low ? LowFill : HighFill;
            Font(cell, Demi, 10, low ? LowInk : HighInk);
        }

        /// <summary>A chip: one cell, Demi 9, centred, the fill and the ink of its word.</summary>
        private static void Chip(IXLCell cell, string word)
        {
            XLColor fill;
            XLColor ink = Navy;

            switch (word)
            {
                case CheckWords.FixStatus:
                case CheckWords.MissingStatus:
                case CheckWords.High:
                    fill = Red;
                    ink = XLColor.White;
                    break;
                case CheckWords.Review:
                case CheckWords.Medium:
                    fill = Amber;
                    break;
                case CheckWords.Ok:
                    fill = Green;
                    ink = XLColor.White;
                    break;
                case CheckWords.Low:
                    fill = Line;
                    break;
                default:
                    fill = NotCheckedFill;
                    break;
            }

            cell.SetValue(word);
            cell.Style.Fill.BackgroundColor = fill;
            Font(cell, Demi, 9, ink);
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static IXLCell Font(IXLCell cell, string name, double size, XLColor color)
        {
            cell.Style.Font.FontName = name;
            cell.Style.Font.FontSize = size;
            cell.Style.Font.FontColor = color;
            return cell;
        }

        /// <summary>Landscape A3, one page wide, the header row on every page, the sheet's name left and the page right in the footer.</summary>
        private static void Print(IXLWorksheet sheet, int header)
        {
            sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            sheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
            sheet.PageSetup.FitToPages(1, 0);
            sheet.PageSetup.SetRowsToRepeatAtTop(header, header);
            sheet.PageSetup.CenterHorizontally = true;
            sheet.PageSetup.Footer.Left.AddText(XLHFPredefinedText.SheetName);
            sheet.PageSetup.Footer.Right.AddText("Page ");
            sheet.PageSetup.Footer.Right.AddText(XLHFPredefinedText.PageNumber);
            sheet.PageSetup.Footer.Right.AddText(" of ");
            sheet.PageSetup.Footer.Right.AddText(XLHFPredefinedText.NumberOfPages);
        }

        /// <summary>The rows of To fix and of Element IDs read back off the disk, or null with why where the workbook would not read.</summary>
        public static DisciplineWorkbookRead Read(string path, out string why)
        {
            why = null;

            try
            {
                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    IXLWorksheet toFix;
                    IXLWorksheet ids;

                    if (!workbook.TryGetWorksheet(ToFixSheet, out toFix) || !workbook.TryGetWorksheet(ElementIdsSheet, out ids))
                    {
                        why = "it holds no sheet named " + (toFix == null ? ToFixSheet : ElementIdsSheet);
                        return null;
                    }

                    return new DisciplineWorkbookRead(RowsUnder(toFix), RowsUnder(ids));
                }
            }
            catch (Exception error)
            {
                why = error.GetType().Name + ": " + error.Message;
                return null;
            }
        }

        /// <summary>The rows under the header whose column B holds something, the No. of To fix and the Building of Element IDs.</summary>
        private static int RowsUnder(IXLWorksheet sheet)
        {
            IXLRow last = sheet.LastRowUsed();
            int rows = 0;

            for (int r = HeaderRow + 1; last != null && r <= last.RowNumber(); r++)
            {
                rows += sheet.Cell(r, 2).IsEmpty() ? 0 : 1;
            }

            return rows;
        }
    }

    /// <summary>The words of the log about a discipline workbook read back.</summary>
    public static class DisciplineWorkbookWords
    {
        /// <summary>
        /// The CHECKS line of a workbook read back, F244: the rows of To fix and of Element IDs read off the file against the
        /// rows written, and in capitals where they differ.
        /// </summary>
        public static string ReadBackLine(DisciplineWorkbookRead back, int toFixWritten, int idsWritten, string discipline, string package)
        {
            return "CHECKS   read back " + back.ToFixRows.ToString(CultureInfo.InvariantCulture) + " of "
                + toFixWritten.ToString(CultureInfo.InvariantCulture) + " rows to fix"
                + (back.ToFixRows == toFixWritten ? string.Empty : ", THE ROWS TO FIX DIFFER")
                + " and " + back.ElementIdRows.ToString(CultureInfo.InvariantCulture) + " of " + idsWritten.ToString(CultureInfo.InvariantCulture)
                + " element ID rows" + (back.ElementIdRows == idsWritten ? string.Empty : ", THE ELEMENT ID ROWS DIFFER")
                + ", " + discipline + " of " + package;
        }
    }

    /// <summary>What a discipline workbook holds as read off the disk: the rows of To fix and of Element IDs.</summary>
    public sealed class DisciplineWorkbookRead
    {
        internal DisciplineWorkbookRead(int toFixRows, int elementIdRows)
        {
            ToFixRows = toFixRows;
            ElementIdRows = elementIdRows;
        }

        public int ToFixRows { get; private set; }

        public int ElementIdRows { get; private set; }
    }
}
