using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Naming;

namespace Federator.Core.Disciplines
{
    /// <summary>
    /// The discipline reports of one package, F209, Bader's message of 17 Oct 2026, feature 2: the six checks over every
    /// group of the package, each line in the workbook of its own discipline, AR, ST, ME and EL by default, one workbook
    /// each. Every line is made by a rule of this folder. A file whose discipline gets no workbook, a code outside the
    /// setting's list, or whose name does not read is named in the log with its count and never dropped in silence.
    /// </summary>
    public sealed class DisciplineReport
    {
        private readonly Dictionary<string, List<CheckRow>> byDiscipline = new Dictionary<string, List<CheckRow>>(StringComparer.Ordinal);
        private readonly List<CheckRow> leftOut = new List<CheckRow>();
        private readonly DisciplineReportSettings settings;

        private DisciplineReport(string community, string package, DisciplineReportSettings settings)
        {
            Community = community ?? string.Empty;
            Package = package ?? string.Empty;
            this.settings = settings;

            foreach (string discipline in settings.Disciplines)
            {
                byDiscipline[discipline] = new List<CheckRow>();
            }
        }

        public string Community { get; private set; }

        public string Package { get; private set; }

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

            DisciplineReport report = new DisciplineReport(community, package, settings);
            List<CheckRow> all = new List<CheckRow>();

            foreach (DisciplineGroup group in groups)
            {
                all.AddRange(CoordinatesCheck.Rows(community, package, group, settings, names));
            }

            all.AddRange(MissingDisciplineCheck.Rows(community, package, groups, settings, names));
            all.AddRange(ReferenceNaming.Rows(community, package, groups, settings, names));
            all.AddRange(FileAgeCheck.Rows(community, package, groups, dates, now, settings, names));

            foreach (DisciplineGroup group in groups)
            {
                all.AddRange(ElementChecks.Rows(community, package, group, settings, names));
            }

            // The six checks in Bader's order in every workbook, each check's lines in the order its rule made them.
            foreach (string check in CheckWords.All)
            {
                foreach (CheckRow row in all)
                {
                    if (!string.Equals(row.Check, check, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    List<CheckRow> into;

                    if (report.byDiscipline.TryGetValue(row.Discipline, out into))
                    {
                        into.Add(row);
                    }
                    else
                    {
                        report.leftOut.Add(row);
                    }
                }
            }

            return report;
        }

        /// <summary>The lines of one discipline's workbook, the six checks in order.</summary>
        public IList<CheckRow> RowsFor(string discipline)
        {
            List<CheckRow> rows;
            return byDiscipline.TryGetValue(discipline ?? string.Empty, out rows) ? rows.AsReadOnly() : new List<CheckRow>().AsReadOnly();
        }

        /// <summary>The disciplines that get a workbook, the setting's order.</summary>
        public IList<string> Disciplines
        {
            get { return settings.Disciplines; }
        }

        /// <summary>The lines no workbook takes, a file of a discipline outside the list or whose name does not read.</summary>
        public IList<CheckRow> LeftOut
        {
            get { return leftOut.AsReadOnly(); }
        }

        /// <summary>How many lines of one discipline's workbook name an element whose id could not be read.</summary>
        public int IdsNotReadable(string discipline)
        {
            int count = 0;

            foreach (CheckRow row in RowsFor(discipline))
            {
                if (string.Equals(row.ElementId, settings.NotReadable, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// The DISCIPLINE REPORT block: per discipline the lines each check made, by result, and the ids not readable, then
        /// the lines no workbook takes, counted by their discipline.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (string discipline in settings.Disciplines)
            {
                IList<CheckRow> rows = RowsFor(discipline);
                lines.Add(discipline + "  " + rows.Count + (rows.Count == 1 ? " line" : " lines") + ", element ids not readable " + IdsNotReadable(discipline));

                foreach (string check in CheckWords.All)
                {
                    Dictionary<string, int> byResult = new Dictionary<string, int>(StringComparer.Ordinal);
                    List<string> order = new List<string>();

                    foreach (CheckRow row in rows)
                    {
                        if (!string.Equals(row.Check, check, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        int was;

                        if (!byResult.TryGetValue(row.Result, out was))
                        {
                            order.Add(row.Result);
                            was = 0;
                        }

                        byResult[row.Result] = was + 1;
                    }

                    List<string> words = order.ConvertAll(result => byResult[result] + " " + result);
                    lines.Add("    " + check.PadRight(19) + (words.Count == 0 ? "no line" : string.Join(", ", words.ToArray())));
                }
            }

            if (leftOut.Count > 0)
            {
                Dictionary<string, int> byCode = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (CheckRow row in leftOut)
                {
                    string code = row.Discipline.Length == 0 ? "a name that does not read" : row.Discipline;
                    int was;
                    byCode[code] = byCode.TryGetValue(code, out was) ? was + 1 : 1;
                }

                foreach (KeyValuePair<string, int> pair in byCode)
                {
                    lines.Add("left out  " + pair.Value + " lines of " + pair.Key + ", which gets no workbook, the disciplines being " + string.Join(", ", new List<string>(settings.Disciplines).ToArray()));
                }
            }
            else
            {
                lines.Add("left out  none, every line is in a workbook");
            }

            return lines;
        }
    }

    /// <summary>
    /// One discipline's workbook of one package, F209: one sheet named after the discipline, Bader's columns in his order,
    /// a header row the filter sits on, every line a row. A file of that name in the folder is never written over, it takes
    /// a dated name, ReportPaths.FreeFilePath, as nothing in the Log folder is. Read back as the file it is, never through
    /// the object that wrote it.
    /// </summary>
    public static class DisciplineWorkbook
    {
        /// <summary>The name of a package's workbook for a discipline: the package's path from the parent in words, the discipline and the suffix.</summary>
        public static string NameFor(string packagePath, string discipline, DisciplineReportSettings settings)
        {
            string words = (packagePath ?? string.Empty).Replace(Path.DirectorySeparatorChar, ' ').Replace(Path.AltDirectorySeparatorChar, ' ').Trim();
            return (words.Length == 0 ? string.Empty : words + " ") + discipline + " " + settings.WorkbookSuffix + ".xlsx";
        }

        public static void Write(string path, string discipline, IList<CheckRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows");
            }

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet sheet = workbook.Worksheets.Add(SheetName(discipline));

                for (int c = 0; c < CheckWords.Columns.Length; c++)
                {
                    sheet.Cell(1, c + 1).Value = CheckWords.Columns[c];
                }

                sheet.Row(1).Style.Font.Bold = true;

                for (int r = 0; r < rows.Count; r++)
                {
                    IList<string> cells = rows[r].Cells();

                    for (int c = 0; c < cells.Count; c++)
                    {
                        sheet.Cell(r + 2, c + 1).SetValue(cells[c]);
                    }
                }

                sheet.Range(1, 1, Math.Max(1, rows.Count + 1), CheckWords.Columns.Length).SetAutoFilter();
                sheet.SheetView.FreezeRows(1);
                double[] widths = { 12, 30, 14, 42, 18, 20, 90, 14 };

                for (int c = 0; c < widths.Length; c++)
                {
                    sheet.Column(c + 1).Width = widths[c];
                }

                workbook.SaveAs(path);
            }
        }

        /// <summary>The rows of a workbook read back off the disk, the header left out, or null with why where it would not read.</summary>
        public static IList<IList<string>> Read(string path, out string why)
        {
            why = null;
            List<IList<string>> rows = new List<IList<string>>();

            try
            {
                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    IXLWorksheet sheet = workbook.Worksheet(1);
                    int last = sheet.LastRowUsed() == null ? 0 : sheet.LastRowUsed().RowNumber();

                    for (int r = 2; r <= last; r++)
                    {
                        List<string> cells = new List<string>();

                        for (int c = 1; c <= CheckWords.Columns.Length; c++)
                        {
                            cells.Add(sheet.Cell(r, c).GetString());
                        }

                        rows.Add(cells);
                    }
                }
            }
            catch (Exception error)
            {
                why = error.GetType().Name + ": " + error.Message;
                return null;
            }

            return rows;
        }

        /// <summary>The sheet's name, the discipline, cut to the 31 characters Excel allows.</summary>
        public static string SheetName(string discipline)
        {
            string name = string.IsNullOrEmpty(discipline) ? "Checks" : discipline;
            return name.Length > 31 ? name.Substring(0, 31) : name;
        }
    }
}
