using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Views;

namespace Federator.Core.Communities
{
    /// <summary>
    /// What one group's machine readable log says that the clash count check reads, F236: each test's clashes as Clash
    /// Detective counted them, the paths of the clash workbook and the HTML page it wrote, each test's open clashes in its
    /// views, and how many tests the rule of one discipline and the rule of two set codes left out. Read off the .tsv the
    /// run wrote, by the event names of RunEvents, every value unescaped by EventRow's own rule.
    /// </summary>
    public sealed class GroupRunFiles
    {
        private readonly Dictionary<string, int> found = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> views = new Dictionary<string, int>(StringComparer.Ordinal);

        private GroupRunFiles()
        {
        }

        /// <summary>Each test that found clashes and how many, as Clash Detective counted them.</summary>
        public IDictionary<string, int> Found
        {
            get { return found; }
        }

        /// <summary>Each test's open clashes in its views, the views of one test added up.</summary>
        public IDictionary<string, int> Views
        {
            get { return views; }
        }

        /// <summary>The clash workbook this run wrote, or null where none was written.</summary>
        public string Workbook { get; private set; }

        /// <summary>The HTML page this run wrote, or null where none was written.</summary>
        public string Page { get; private set; }

        /// <summary>The tests the rule of two set codes left out, Q209.</summary>
        public int LeftOutBySetCodes { get; private set; }

        /// <summary>The tests the rule of one discipline left out, Q203.</summary>
        public int LeftOutByOneDiscipline { get; private set; }

        public static GroupRunFiles Read(IEnumerable<string> tsvLines)
        {
            GroupRunFiles read = new GroupRunFiles();
            string setCodes = ", " + CoverageWords.For(CoverageReason.SameSetCode);
            string oneDiscipline = ", " + CoverageWords.For(CoverageReason.OneDiscipline);
            bool header = true;

            foreach (string line in tsvLines ?? new string[0])
            {
                if (header)
                {
                    header = false;
                    continue;
                }

                string[] cells = line.Split('\t');

                if (cells.Length < 8)
                {
                    continue;
                }

                string kind = EventRow.Unescape(cells[4]);
                string name = EventRow.Unescape(cells[5]);
                string number = EventRow.Unescape(cells[6]);
                string text = EventRow.Unescape(cells[7]);
                int count;

                if (kind == RunEvents.TestFoundClashes && int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out count))
                {
                    read.found[name] = count;
                }
                else if (kind == RunEvents.Written && name == RunEvents.WorkbookKind)
                {
                    read.Workbook = text;
                }
                else if (kind == RunEvents.Written && name == RunEvents.PageKind)
                {
                    read.Page = text;
                }
                else if (kind == RunEvents.ViewsTree)
                {
                    string test;
                    int open;

                    if (ViewsTree.ReadViewLine(text, out test, out open))
                    {
                        int was;
                        read.views[test] = (read.views.TryGetValue(test, out was) ? was : 0) + open;
                    }
                }
                else if (kind == RunEvents.CoverageTest)
                {
                    read.LeftOutBySetCodes += text.Contains(setCodes) ? 1 : 0;
                    read.LeftOutByOneDiscipline += text.Contains(oneDiscipline) ? 1 : 0;
                }
            }

            return read;
        }
    }

    /// <summary>
    /// One row of the clash count check, F236, Bader's columns in his order: building, test, priority, clashes in Clash
    /// Detective, Excel rows, HTML rows, clashes in viewpoints, pictures made, pictures skipped, why skipped, New, Active,
    /// Reviewed, Approved, Resolved, every number read off the files the group wrote.
    /// </summary>
    public sealed class ClashCountRow
    {
        public static readonly string[] Columns =
        {
            "Building", "Test", "Priority", "Clashes in Clash Detective", "Excel rows", "HTML rows", "Clashes in viewpoints",
            "Pictures made", "Pictures skipped", "Why skipped", "New", "Active", "Reviewed", "Approved", "Resolved"
        };

        /// <summary>The statuses a row counts in the order of its columns, each as the workbook writes it.</summary>
        public static readonly ClashStatus[] Statuses = { ClashStatus.New, ClashStatus.Active, ClashStatus.Reviewed, ClashStatus.Approved, ClashStatus.Resolved };

        private readonly int[] byStatus = new int[Statuses.Length];

        public ClashCountRow(string building, string test, string priority, int found, int excelRows, int htmlRows, int views,
            int made, string whySkipped, IList<int> statuses)
        {
            Building = building ?? string.Empty;
            Test = test ?? string.Empty;
            Priority = priority ?? string.Empty;
            Found = found;
            ExcelRows = excelRows;
            HtmlRows = htmlRows;
            Views = views;
            Made = made;
            WhySkipped = whySkipped ?? string.Empty;

            for (int i = 0; i < byStatus.Length && statuses != null && i < statuses.Count; i++)
            {
                byStatus[i] = statuses[i];
            }
        }

        public string Building { get; private set; }

        public string Test { get; private set; }

        public string Priority { get; private set; }

        /// <summary>Clash Detective's count, off the group's .tsv.</summary>
        public int Found { get; private set; }

        public int ExcelRows { get; private set; }

        public int HtmlRows { get; private set; }

        /// <summary>The open clashes of the test's views, off the group's .tsv.</summary>
        public int Views { get; private set; }

        public int Made { get; private set; }

        /// <summary>The Excel rows with no picture.</summary>
        public int Skipped
        {
            get { return Math.Max(0, ExcelRows - Made); }
        }

        public string WhySkipped { get; private set; }

        /// <summary>The Excel rows at a status, the order of Statuses.</summary>
        public int At(ClashStatus status)
        {
            int i = Array.IndexOf(Statuses, status);
            return i < 0 ? 0 : byStatus[i];
        }

        /// <summary>The rows at the statuses a view shows, New and Active by default, the count the views are held to.</summary>
        public int OpenForViews
        {
            get
            {
                int open = 0;

                foreach (ClashStatus status in OpenClashes.StatusesFor(ViewpointSettings.DefaultViewStatuses))
                {
                    open += At(status);
                }

                return open;
            }
        }

        /// <summary>
        /// Whether the four counts differ, Bader's red row: Clash Detective's count against the Excel rows and the HTML rows,
        /// and the clashes in the views against the rows at the statuses a view shows, since a view holds the open clashes alone.
        /// </summary>
        public bool Differs
        {
            get { return Found != ExcelRows || Found != HtmlRows || Views != OpenForViews; }
        }

        /// <summary>The cells in the order of the columns, numbers as numbers.</summary>
        public IList<object> Cells()
        {
            List<object> cells = new List<object> { Building, Test, Priority, Found, ExcelRows, HtmlRows, Views, Made, Skipped, WhySkipped };

            foreach (int count in byStatus)
            {
                cells.Add(count);
            }

            return cells;
        }

        /// <summary>A row adding up the rows given, under the words given, its Why skipped empty.</summary>
        public static ClashCountRow Total(string building, string words, IList<ClashCountRow> rows)
        {
            int found = 0;
            int excel = 0;
            int html = 0;
            int views = 0;
            int made = 0;
            int[] statuses = new int[Statuses.Length];

            foreach (ClashCountRow row in rows)
            {
                found += row.Found;
                excel += row.ExcelRows;
                html += row.HtmlRows;
                views += row.Views;
                made += row.Made;

                for (int i = 0; i < Statuses.Length; i++)
                {
                    statuses[i] += row.byStatus[i];
                }
            }

            return new ClashCountRow(building, words, string.Empty, found, excel, html, views, made, string.Empty, statuses);
        }
    }

    /// <summary>
    /// The clash count check of one community, F236, Bader's round of 19 Oct 2026, fix 6: one row per group per test with
    /// clashes, a total per group and one for the community, a red row and a log line where the four counts differ, and a
    /// section of the groups not built and of the tests the rule of one discipline or of two set codes left out, a count a
    /// group. Every number is read off the files each group wrote: its .tsv, its clash workbook and its HTML page.
    /// </summary>
    public sealed class ClashCountCheck
    {
        public const string TotalWords = "Total";

        private readonly List<ClashCountGroup> groups = new List<ClashCountGroup>();
        private readonly List<string> notBuilt = new List<string>();

        public ClashCountCheck(string community)
        {
            Community = community ?? string.Empty;
        }

        public string Community { get; private set; }

        public IList<ClashCountGroup> Groups
        {
            get { return groups.AsReadOnly(); }
        }

        /// <summary>The groups not built for one discipline, each by its name.</summary>
        public IList<string> NotBuilt
        {
            get { return notBuilt.AsReadOnly(); }
        }

        public void AddNotBuilt(string group)
        {
            notBuilt.Add(group ?? string.Empty);
        }

        /// <summary>
        /// A group read off its files: its .tsv, its clash workbook read test by test and its page. A test with clashes is
        /// one Clash Detective found clashes in or one the workbook holds a block for. Why a row lost pictures is said off the
        /// statuses the pictures take, the cap of a test over the setting's clashes, and what is left UNKNOWN.
        /// </summary>
        public ClashCountGroup AddGroup(string building, GroupRunFiles run, WorkbookTests workbook, PageTests page, ImageOptions images)
        {
            if (run == null)
            {
                throw new ArgumentNullException("run");
            }

            ImageOptions options = images ?? new ImageOptions();
            ClashCountGroup group = new ClashCountGroup(building, run.LeftOutBySetCodes, run.LeftOutByOneDiscipline);
            List<string> names = new List<string>(run.Found.Keys);

            foreach (WorkbookTest test in workbook == null ? new List<WorkbookTest>() : new List<WorkbookTest>(workbook.Tests))
            {
                if (!names.Contains(test.Name))
                {
                    names.Add(test.Name);
                }
            }

            foreach (string name in names)
            {
                int found;
                int views;
                run.Found.TryGetValue(name, out found);
                run.Views.TryGetValue(name, out views);
                IList<WorkbookTest> inExcel = workbook == null ? new List<WorkbookTest>() : workbook.Named(name);
                IList<PageTest> inPage = page == null ? new List<PageTest>() : page.Named(name);
                WorkbookTest excel = inExcel.Count == 1 ? inExcel[0] : null;
                int html = inPage.Count == 1 ? inPage[0].Rows : 0;
                List<int> statuses = new List<int>();

                foreach (ClashStatus status in ClashCountRow.Statuses)
                {
                    statuses.Add(excel == null ? 0 : excel.RowsAt(status.ToString()));
                }

                group.Add(new ClashCountRow(
                    building,
                    name,
                    excel == null ? string.Empty : excel.Priority,
                    found,
                    excel == null ? 0 : excel.Rows,
                    html,
                    views,
                    excel == null ? 0 : excel.Pictures,
                    excel == null ? WhyNotInTheWorkbook(inExcel.Count) : WhySkipped(excel, found, options),
                    statuses));
            }

            groups.Add(group);
            return group;
        }

        private static string WhyNotInTheWorkbook(int blocks)
        {
            return blocks == 0 ? "the workbook holds no block of this test" : "the workbook holds " + blocks + " blocks of this test";
        }

        /// <summary>
        /// Why the rows of a test with no picture have none: those at a status the pictures do not take, those past the cap
        /// of a test of more than the setting's clashes, Q187, and the rest UNKNOWN, a stop or a failure the group's IMAGES
        /// lines name. Empty where every row has its picture.
        /// </summary>
        public static string WhySkipped(WorkbookTest test, int clashes, ImageOptions images)
        {
            int skipped = Math.Max(0, test.Rows - test.Pictures);

            if (skipped == 0)
            {
                return string.Empty;
            }

            int notAsked = 0;

            foreach (ClashStatus status in ClashCountRow.Statuses)
            {
                notAsked += images.Wants(status) ? 0 : test.RowsAt(status.ToString());
            }

            int asked = test.Rows - notAsked;
            int limit = PictureLimits.LimitFor(Priorities.From(test.Priority), clashes, images);
            int capped = limit == int.MaxValue ? 0 : Math.Max(0, asked - limit);
            notAsked = Math.Min(notAsked, skipped);
            capped = Math.Min(capped, skipped - notAsked);
            int unknown = skipped - notAsked - capped;
            List<string> why = new List<string>();

            if (notAsked > 0)
            {
                why.Add(notAsked + " at a status the pictures do not take");
            }

            if (capped > 0)
            {
                why.Add(capped + " past the cap of " + limit + " of a test of more than " + images.CapOverClashes + " clashes");
            }

            if (unknown > 0)
            {
                why.Add(unknown + " UNKNOWN, a stop or a failure the group's IMAGES lines name");
            }

            return string.Join(", ", why.ToArray());
        }

        /// <summary>The community's total over every group's rows.</summary>
        public ClashCountRow Total()
        {
            List<ClashCountRow> all = new List<ClashCountRow>();

            foreach (ClashCountGroup group in groups)
            {
                all.AddRange(group.Rows);
            }

            return ClashCountRow.Total(Community, TotalWords + " " + Community, all);
        }

        /// <summary>The rows whose four counts differ, over every group.</summary>
        public IList<ClashCountRow> Differing()
        {
            List<ClashCountRow> differing = new List<ClashCountRow>();

            foreach (ClashCountGroup group in groups)
            {
                differing.AddRange(new List<ClashCountRow>(group.Rows).FindAll(r => r.Differs));
            }

            return differing;
        }

        /// <summary>The log's lines: one a differing row, then the community's count of rows and of red rows.</summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (ClashCountRow row in Differing())
            {
                lines.Add("COUNTS   DIFFER  " + Community + "  " + row.Building + "  " + row.Test + "  Clash Detective " + row.Found
                    + ", Excel rows " + row.ExcelRows + ", HTML rows " + row.HtmlRows + ", viewpoints " + row.Views + " of " + row.OpenForViews + " open");
            }

            int rows = 0;

            foreach (ClashCountGroup group in groups)
            {
                rows += group.Rows.Count;
            }

            lines.Add("COUNTS   " + Community + "  " + groups.Count + (groups.Count == 1 ? " group, " : " groups, ") + rows
                + (rows == 1 ? " test with clashes, " : " tests with clashes, ") + Differing().Count + " red, the four counts differing, "
                + notBuilt.Count + " not built for one discipline");
            return lines;
        }
    }

    /// <summary>One group of the clash count check: its rows, its total, and the tests the two rules left out.</summary>
    public sealed class ClashCountGroup
    {
        private readonly List<ClashCountRow> rows = new List<ClashCountRow>();

        internal ClashCountGroup(string building, int leftOutBySetCodes, int leftOutByOneDiscipline)
        {
            Building = building ?? string.Empty;
            LeftOutBySetCodes = leftOutBySetCodes;
            LeftOutByOneDiscipline = leftOutByOneDiscipline;
        }

        public string Building { get; private set; }

        public IList<ClashCountRow> Rows
        {
            get { return rows.AsReadOnly(); }
        }

        public int LeftOutBySetCodes { get; private set; }

        public int LeftOutByOneDiscipline { get; private set; }

        public ClashCountRow Total()
        {
            return ClashCountRow.Total(Building, ClashCountCheck.TotalWords + " " + Building, rows);
        }

        internal void Add(ClashCountRow row)
        {
            rows.Add(row);
        }
    }
}
