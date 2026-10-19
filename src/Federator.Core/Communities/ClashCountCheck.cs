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
    /// Detective counted them, the paths of the clash workbook and the HTML page it wrote and the workbook's sheet, each test's
    /// open clashes in its views read back, the mirrors the rule paired and those it merged with the clashes only they found,
    /// and how many tests the rule of one discipline and the rule of two set codes left out. Read off the .tsv the run wrote,
    /// each row by EventRow.Parse and each event by its name in RunEvents.
    /// </summary>
    public sealed class GroupRunFiles
    {
        private readonly Dictionary<string, int> found = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> views = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> viewsNotReadBack = new List<string>();
        private readonly List<string> mirrors = new List<string>();
        private readonly List<string> mergedMirrors = new List<string>();
        private readonly Dictionary<string, int> addedByMirrors = new Dictionary<string, int>(StringComparer.Ordinal);

        private GroupRunFiles()
        {
        }

        /// <summary>Each test that ran and how many clashes Clash Detective found in it, nought for a test that passed.</summary>
        public IDictionary<string, int> Found
        {
            get { return found; }
        }

        /// <summary>The clash workbook this run wrote, or null where none was written.</summary>
        public string Workbook { get; private set; }

        /// <summary>The name the writer gave the clash workbook's sheet, or null where no row names it.</summary>
        public string WorkbookSheet { get; private set; }

        /// <summary>The HTML page this run wrote, or null where none was written.</summary>
        public string Page { get; private set; }

        /// <summary>Whether the group wrote a VIEWS TREE block at all, false where its views were not asked for or not reached.</summary>
        public bool ViewsTreeWritten { get; private set; }

        /// <summary>The tests the rule of two set codes left out, Q209.</summary>
        public int LeftOutBySetCodes { get; private set; }

        /// <summary>The tests the rule of one discipline left out, Q203.</summary>
        public int LeftOutByOneDiscipline { get; private set; }

        /// <summary>
        /// A test's open clashes in its views, the views of one test added up, nought for a test with no view where the block
        /// was written, and minus one, UNKNOWN, where no block was written or a view of the test was not read back.
        /// </summary>
        public int ViewsOf(string test)
        {
            if (!ViewsTreeWritten || viewsNotReadBack.Contains(test))
            {
                return ClashCountRow.Unknown;
            }

            int open;
            return views.TryGetValue(test, out open) ? open : 0;
        }

        /// <summary>Whether the mirror rule paired the test as a mirror.</summary>
        public bool IsAMirror(string test)
        {
            return mirrors.Contains(test);
        }

        /// <summary>Whether the merge took the mirror's clashes into its kept test, so the mirror has no block of its own.</summary>
        public bool IsMerged(string test)
        {
            return mergedMirrors.Contains(test);
        }

        /// <summary>The clashes only the mirrors of a kept test found, which the merge added to it, nought for any other test.</summary>
        public int AddedByMirrorsTo(string test)
        {
            int added;
            return addedByMirrors.TryGetValue(test, out added) ? added : 0;
        }

        public static GroupRunFiles Read(IEnumerable<string> tsvLines)
        {
            GroupRunFiles read = new GroupRunFiles();

            foreach (string line in tsvLines ?? new string[0])
            {
                EventRow row = EventRow.Parse(line);

                if (row != null)
                {
                    read.Take(row);
                }
            }

            return read;
        }

        private void Take(EventRow row)
        {
            int count;
            bool number = int.TryParse(row.Number, NumberStyles.None, CultureInfo.InvariantCulture, out count);

            if (row.Happening == RunEvents.TestFoundClashes && number)
            {
                found[row.Name] = count;
            }
            else if (row.Happening == RunEvents.TestPassed && !found.ContainsKey(row.Name))
            {
                found[row.Name] = 0;
            }
            else if (row.Happening == RunEvents.Written && row.Name == RunEvents.WorkbookKind)
            {
                Workbook = row.Text;
            }
            else if (row.Happening == RunEvents.WorkbookSheet)
            {
                WorkbookSheet = row.Text;
            }
            else if (row.Happening == RunEvents.Written && row.Name == RunEvents.PageKind)
            {
                Page = row.Text;
            }
            else if (row.Happening == RunEvents.ViewsTree)
            {
                ViewsTreeWritten = true;
                string test;
                int open;
                bool readBack;

                if (ViewsTree.ReadViewLine(row.Text, out test, out open, out readBack))
                {
                    int was;
                    views[test] = (views.TryGetValue(test, out was) ? was : 0) + open;

                    if (!readBack && !viewsNotReadBack.Contains(test))
                    {
                        viewsNotReadBack.Add(test);
                    }
                }
            }
            else if (row.Happening == RunEvents.Mirror && !mirrors.Contains(row.Name))
            {
                mirrors.Add(row.Name);
            }
            else if (row.Happening == RunEvents.MirrorMerged && !mergedMirrors.Contains(row.Name))
            {
                mergedMirrors.Add(row.Name);
            }
            else if (row.Happening == RunEvents.AddedByMirrors && number)
            {
                addedByMirrors[row.Name] = count;
            }
            else if (row.Happening == RunEvents.CoverageTest)
            {
                LeftOutBySetCodes += CoverageBlock.RowGives(row.Text, CoverageReason.SameSetCode) ? 1 : 0;
                LeftOutByOneDiscipline += CoverageBlock.RowGives(row.Text, CoverageReason.OneDiscipline) ? 1 : 0;
            }
        }
    }

    /// <summary>
    /// One row of the clash count check, F236, Bader's columns in his order: building, test, priority, clashes in Clash
    /// Detective, Excel rows, HTML rows, clashes in viewpoints, pictures made, pictures skipped, why skipped, New, Active,
    /// Reviewed, Approved, Resolved, every number read off the files the group wrote. A count that could not be read is minus
    /// one, written UNKNOWN, never a nought, never added into a total and never judged.
    /// </summary>
    public sealed class ClashCountRow
    {
        public const int Unknown = -1;

        public const string UnknownWord = "UNKNOWN";

        public static readonly string[] Columns =
        {
            "Building", "Test", "Priority", "Clashes in Clash Detective", "Excel rows", "HTML rows", "Clashes in viewpoints",
            "Pictures made", "Pictures skipped", "Why skipped", "New", "Active", "Reviewed", "Approved", "Resolved"
        };

        /// <summary>The statuses a row counts in the order of its columns, each as the workbook writes it.</summary>
        public static readonly ClashStatus[] Statuses = { ClashStatus.New, ClashStatus.Active, ClashStatus.Reviewed, ClashStatus.Approved, ClashStatus.Resolved };

        private readonly int[] byStatus = new int[Statuses.Length];
        private readonly IList<ClashStatus> viewStatuses;

        public ClashCountRow(string building, string test, string priority, int found, int excelRows, int htmlRows, int views,
            int made, string whySkipped, IList<int> statuses)
            : this(building, test, priority, found, excelRows, htmlRows, views, made, whySkipped, statuses, 0, false, ViewpointSettings.DefaultViewStatuses)
        {
        }

        public ClashCountRow(string building, string test, string priority, int found, int excelRows, int htmlRows, int views,
            int made, string whySkipped, IList<int> statuses, int addedByMirrors, bool unmergedMirror, OpenClashCount viewStatuses)
        {
            Building = building ?? string.Empty;
            Test = test ?? string.Empty;
            Priority = priority ?? string.Empty;
            Found = Read(found);
            ExcelRows = Read(excelRows);
            HtmlRows = Read(htmlRows);
            Views = Read(views);
            Made = Read(made);
            WhySkipped = whySkipped ?? string.Empty;
            AddedByMirrors = Math.Max(0, addedByMirrors);
            UnmergedMirror = unmergedMirror;
            this.viewStatuses = OpenClashes.StatusesFor(viewStatuses);

            for (int i = 0; i < byStatus.Length; i++)
            {
                byStatus[i] = statuses != null && i < statuses.Count ? Read(statuses[i]) : Unknown;
            }
        }

        private static int Read(int count)
        {
            return count < 0 ? Unknown : count;
        }

        public string Building { get; private set; }

        public string Test { get; private set; }

        public string Priority { get; private set; }

        /// <summary>Clash Detective's count, off the group's .tsv.</summary>
        public int Found { get; private set; }

        public int ExcelRows { get; private set; }

        public int HtmlRows { get; private set; }

        /// <summary>The open clashes of the test's views read back, off the group's .tsv.</summary>
        public int Views { get; private set; }

        public int Made { get; private set; }

        /// <summary>The clashes only the test's mirrors found, which the merge added to its rows, F132.</summary>
        public int AddedByMirrors { get; private set; }

        /// <summary>A mirror the merge did not take, which keeps its rows and gets no view, F114.</summary>
        public bool UnmergedMirror { get; private set; }

        /// <summary>The Excel rows with no picture, UNKNOWN where either count is.</summary>
        public int Skipped
        {
            get { return ExcelRows < 0 || Made < 0 ? Unknown : Math.Max(0, ExcelRows - Made); }
        }

        public string WhySkipped { get; private set; }

        /// <summary>The Excel rows at a status, the order of Statuses, UNKNOWN where the workbook was not read.</summary>
        public int At(ClashStatus status)
        {
            int i = Array.IndexOf(Statuses, status);
            return i < 0 ? Unknown : byStatus[i];
        }

        /// <summary>The rows the workbook and the page hold for Clash Detective's count, with the clashes only its mirrors found.</summary>
        public int RowsExpected
        {
            get { return Found < 0 ? Unknown : Found + AddedByMirrors; }
        }

        /// <summary>
        /// The clashes the views of the test hold for its rows, those at the statuses a view shows, the run's view setting, and
        /// none for a mirror the merge did not take, since no mirror gets a view. UNKNOWN where a status was not read.
        /// </summary>
        public int ViewsExpected
        {
            get
            {
                if (UnmergedMirror)
                {
                    return 0;
                }

                int open = 0;

                foreach (ClashStatus status in viewStatuses)
                {
                    int at = At(status);

                    if (at < 0)
                    {
                        return Unknown;
                    }

                    open += at;
                }

                return open;
            }
        }

        /// <summary>
        /// Whether the four counts differ, Bader's red row, judged over the counts read alone: Clash Detective's count with the
        /// clashes only its mirrors found against the Excel rows and the HTML rows, the two against each other, and the clashes
        /// in the views against the rows at the statuses a view shows. A count UNKNOWN is judged against nothing, HasUnknown.
        /// </summary>
        public bool Differs
        {
            get
            {
                return Apart(RowsExpected, ExcelRows) || Apart(RowsExpected, HtmlRows) || Apart(ExcelRows, HtmlRows)
                    || Apart(Views, ViewsExpected);
            }
        }

        private static bool Apart(int one, int other)
        {
            return one >= 0 && other >= 0 && one != other;
        }

        /// <summary>Whether any of the four counts could not be read, so the row is not judged whole.</summary>
        public bool HasUnknown
        {
            get { return Found < 0 || ExcelRows < 0 || HtmlRows < 0 || Views < 0 || ViewsExpected < 0; }
        }

        /// <summary>The names of the counts that could not be read, for the log.</summary>
        public string UnknownNames()
        {
            List<string> names = new List<string>();
            AddIfUnknown(names, Found, Columns[3]);
            AddIfUnknown(names, ExcelRows, Columns[4]);
            AddIfUnknown(names, HtmlRows, Columns[5]);
            AddIfUnknown(names, Views, Columns[6]);

            if (ExcelRows >= 0 && ViewsExpected < 0)
            {
                names.Add("the statuses of the Excel rows");
            }

            return string.Join(", ", names.ToArray());
        }

        private static void AddIfUnknown(List<string> names, int count, string name)
        {
            if (count < 0)
            {
                names.Add(name);
            }
        }

        /// <summary>The cells in the order of the columns, numbers as numbers and a count not read as UNKNOWN.</summary>
        public IList<object> Cells()
        {
            List<object> cells = new List<object> { Building, Test, Priority, Cell(Found), Cell(ExcelRows), Cell(HtmlRows), Cell(Views), Cell(Made), Cell(Skipped), WhySkipped };

            foreach (int count in byStatus)
            {
                cells.Add(Cell(count));
            }

            return cells;
        }

        private static object Cell(int count)
        {
            return count < 0 ? (object)UnknownWord : count;
        }

        /// <summary>
        /// A row adding up the rows given, under the words given. A count UNKNOWN in a row is left out of its column, and the
        /// Why skipped cell then says the total is at least the sum, a count not taken never being a nought.
        /// </summary>
        public static ClashCountRow Total(string building, string words, IList<ClashCountRow> rows)
        {
            int[] sums = new int[5];
            int[] statuses = new int[Statuses.Length];
            int unknown = 0;

            foreach (ClashCountRow row in rows)
            {
                int[] counts = { row.Found, row.ExcelRows, row.HtmlRows, row.Views, row.Made };

                for (int i = 0; i < counts.Length; i++)
                {
                    sums[i] += Math.Max(0, counts[i]);
                    unknown += counts[i] < 0 ? 1 : 0;
                }

                for (int i = 0; i < Statuses.Length; i++)
                {
                    statuses[i] += Math.Max(0, row.byStatus[i]);
                    unknown += row.byStatus[i] < 0 ? 1 : 0;
                }
            }

            string why = unknown == 0 ? string.Empty : "at least these, " + unknown + (unknown == 1 ? " count UNKNOWN is" : " counts UNKNOWN are") + " left out";
            return new ClashCountRow(building, words, string.Empty, sums[0], sums[1], sums[2], sums[3], sums[4], why, statuses);
        }
    }

    /// <summary>
    /// The clash count check of one community, F236, Bader's round of 19 Oct 2026, fix 6: one row per group per test with
    /// clashes, a total per group and one for the community, a red row and a log line where the four counts differ, and a
    /// section of the groups not built and of the tests the rule of one discipline or of two set codes left out, a count a
    /// group. Every number is read off the files each group wrote: its .tsv, its clash workbook and its HTML page. A group
    /// whose files could not be read is named with why and is in no row.
    /// </summary>
    public sealed class ClashCountCheck
    {
        public const string TotalWords = "Total";

        private readonly List<ClashCountGroup> groups = new List<ClashCountGroup>();
        private readonly List<string> notBuilt = new List<string>();
        private readonly List<KeyValuePair<string, string>> notRead = new List<KeyValuePair<string, string>>();

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

        /// <summary>The groups of the run whose files could not be read, each with why.</summary>
        public IList<KeyValuePair<string, string>> NotRead
        {
            get { return notRead.AsReadOnly(); }
        }

        public void AddNotBuilt(string group)
        {
            notBuilt.Add(group ?? string.Empty);
        }

        public void AddNotRead(string group, string why)
        {
            notRead.Add(new KeyValuePair<string, string>(group ?? string.Empty, string.IsNullOrEmpty(why) ? ClashCountRow.UnknownWord : why));
        }

        /// <summary>
        /// A group read off its files: its .tsv, its clash workbook read test by test and its page, each null where it was not
        /// read. A test with clashes is one Clash Detective found clashes in or one the workbook holds a block for, and a mirror
        /// the merge took into its kept test is not a row, its clashes being the kept test's. Why a row lost pictures is said off
        /// the pictures' settings, the statuses they take, the cap of a test of more than the setting's clashes, Q187, and what
        /// is left UNKNOWN. The views are held to the run's view statuses.
        /// </summary>
        public ClashCountGroup AddGroup(string building, GroupRunFiles run, WorkbookTests workbook, PageTests page, ImageOptions images,
            OpenClashCount viewStatuses)
        {
            if (run == null)
            {
                throw new ArgumentNullException("run");
            }

            ImageOptions options = images ?? new ImageOptions();
            ClashCountGroup group = new ClashCountGroup(building, run.LeftOutBySetCodes, run.LeftOutByOneDiscipline);
            List<string> names = new List<string>();

            foreach (KeyValuePair<string, int> test in run.Found)
            {
                if (test.Value > 0 && !run.IsMerged(test.Key))
                {
                    names.Add(test.Key);
                }
            }

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
                IList<WorkbookTest> inExcel = workbook == null ? new List<WorkbookTest>() : workbook.Named(name);
                IList<PageTest> inPage = page == null ? new List<PageTest>() : page.Named(name);
                WorkbookTest excel = inExcel.Count == 1 ? inExcel[0] : null;
                int excelRows = workbook == null || inExcel.Count > 1 ? ClashCountRow.Unknown : excel == null ? 0 : excel.Rows;
                int html = page == null || inPage.Count > 1 ? ClashCountRow.Unknown : inPage.Count == 0 ? 0 : inPage[0].Rows;
                List<int> statuses = new List<int>();

                foreach (ClashStatus status in ClashCountRow.Statuses)
                {
                    statuses.Add(workbook == null || inExcel.Count > 1 ? ClashCountRow.Unknown : excel == null ? 0 : excel.RowsAt(status.ToString()));
                }

                group.Add(new ClashCountRow(
                    building,
                    name,
                    excel == null ? string.Empty : excel.Priority,
                    run.Found.TryGetValue(name, out found) ? found : ClashCountRow.Unknown,
                    excelRows,
                    html,
                    run.ViewsOf(name),
                    workbook == null || inExcel.Count > 1 ? ClashCountRow.Unknown : excel == null ? 0 : excel.Pictures,
                    workbook == null ? "the workbook was not read" : excel == null ? WhyNotInTheWorkbook(inExcel.Count) : WhySkipped(excel, options),
                    statuses,
                    run.AddedByMirrorsTo(name),
                    run.IsAMirror(name) && !run.IsMerged(name),
                    viewStatuses));
            }

            groups.Add(group);
            return group;
        }

        private static string WhyNotInTheWorkbook(int blocks)
        {
            return blocks == 0 ? "the workbook holds no block of this test" : "the workbook holds " + blocks + " blocks of this test";
        }

        /// <summary>
        /// Why the rows of a test with no picture have none: every one where the run took no pictures, then those at a status
        /// the pictures do not take, those past the cap of a test of more than the setting's clashes, Q187, read off the test's
        /// Clashes cell as the pictures read it, and the rest UNKNOWN. Empty where every row has its picture.
        /// </summary>
        public static string WhySkipped(WorkbookTest test, ImageOptions images)
        {
            if (test == null || images == null)
            {
                throw new ArgumentNullException(test == null ? "test" : "images");
            }

            if (test.Rows < 0)
            {
                return "the block's clash table was not read";
            }

            int skipped = Math.Max(0, test.Rows - test.Pictures);

            if (skipped == 0)
            {
                return string.Empty;
            }

            if (!images.Write)
            {
                return skipped + " with no picture, the run took no pictures";
            }

            int notAsked = 0;

            foreach (ClashStatus status in ClashCountRow.Statuses)
            {
                notAsked += images.Wants(status) ? 0 : test.RowsAt(status.ToString());
            }

            int asked = test.Rows - notAsked;
            int limit = test.Clashes < 0 ? int.MaxValue : PictureLimits.LimitFor(Priorities.From(test.Priority), test.Clashes, images);
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
                why.Add(unknown + " UNKNOWN, a stop, a failure or a retake showing only the sky, which the group's IMAGES and RETAKE lines name");
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
            return RowsWhere(r => r.Differs);
        }

        private IList<ClashCountRow> RowsWhere(Predicate<ClashCountRow> holds)
        {
            List<ClashCountRow> rows = new List<ClashCountRow>();

            foreach (ClashCountGroup group in groups)
            {
                rows.AddRange(new List<ClashCountRow>(group.Rows).FindAll(holds));
            }

            return rows;
        }

        /// <summary>
        /// The log's lines: one a differing row, one a row with a count UNKNOWN, one a kept test holding clashes only its mirrors
        /// found, one a group not read, then the community's count of rows, of red rows and of the rest.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (ClashCountRow row in Differing())
            {
                lines.Add("COUNTS   DIFFER  " + Community + "  " + row.Building + "  " + row.Test + "  Clash Detective " + Words(row.Found)
                    + (row.AddedByMirrors > 0 ? " and " + row.AddedByMirrors + " only its mirrors found" : string.Empty)
                    + ", Excel rows " + Words(row.ExcelRows) + ", HTML rows " + Words(row.HtmlRows) + ", viewpoints " + Words(row.Views)
                    + " of " + Words(row.ViewsExpected) + (row.UnmergedMirror ? ", a mirror, which gets no view" : " open"));
            }

            foreach (ClashCountRow row in RowsWhere(r => r.HasUnknown))
            {
                lines.Add("COUNTS   UNKNOWN  " + Community + "  " + row.Building + "  " + row.Test + "  not judged on " + row.UnknownNames()
                    + ", which could not be read");
            }

            foreach (ClashCountRow row in RowsWhere(r => r.AddedByMirrors > 0))
            {
                lines.Add("COUNTS   MIRRORS  " + Community + "  " + row.Building + "  " + row.Test + "  holds " + row.AddedByMirrors
                    + " clashes only its mirrors found, merged into it, so its rows are held to Clash Detective's " + Words(row.Found) + " and those");
            }

            foreach (KeyValuePair<string, string> group in notRead)
            {
                lines.Add("COUNTS   NOT READ  " + Community + "  " + group.Key + "  " + group.Value);
            }

            int rows = 0;

            foreach (ClashCountGroup group in groups)
            {
                rows += group.Rows.Count;
            }

            lines.Add("COUNTS   " + Community + "  " + groups.Count + (groups.Count == 1 ? " group, " : " groups, ") + rows
                + (rows == 1 ? " test with clashes, " : " tests with clashes, ") + Differing().Count + " red, the four counts differing, "
                + RowsWhere(r => r.HasUnknown).Count + " with a count UNKNOWN, " + notBuilt.Count + " not built for one discipline, "
                + notRead.Count + " not read");
            return lines;
        }

        private static string Words(int count)
        {
            return count < 0 ? ClashCountRow.UnknownWord : count.ToString(CultureInfo.InvariantCulture);
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
