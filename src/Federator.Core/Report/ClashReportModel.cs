using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// One side of one clash row. Family, type and material are here because without them
    /// whoever has to fix the clash must open the model to find out what they are looking
    /// at.
    /// </summary>
    public sealed class ClashItem
    {
        public ClashItem()
        {
            Name = string.Empty;
            Family = string.Empty;
            Type = string.Empty;
            Material = string.Empty;
            SourceFile = string.Empty;
            Discipline = string.Empty;
            ElementId = string.Empty;
        }

        public string Name { get; set; }

        public string Family { get; set; }

        public string Type { get; set; }

        public string Material { get; set; }

        /// <summary>The NWC the item came from, which is how a discipline is traced back.</summary>
        public string SourceFile { get; set; }

        public string Discipline { get; set; }

        /// <summary>The Revit element id where there is one, otherwise the instance GUID.</summary>
        public string ElementId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// One row of a test sheet. Either one clash, or one group of clashes, which is one
    /// row carrying the count of what is inside it so the grouping hides nothing.
    /// </summary>
    public sealed class ClashRow
    {
        public ClashRow()
        {
            Name = string.Empty;
            GridLocation = string.Empty;
            Level = string.Empty;
            Left = new ClashItem();
            Right = new ClashItem();
            RawClashes = 1;
        }

        public string Name { get; set; }

        public ClashStatus Status { get; set; }

        /// <summary>
        /// For a group, the most severe distance of the clashes inside it. See
        /// <see cref="MostSevere"/> for what severe means and why.
        /// </summary>
        public double Distance { get; set; }

        public string GridLocation { get; set; }

        public string Level { get; set; }

        public ClashItem Left { get; set; }

        public ClashItem Right { get; set; }

        public DateTime? Found { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        /// <summary>
        /// True when this row stands for a group rather than for one clash.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// How many raw clashes this row stands for. One for an ungrouped clash, and the
        /// number inside the group otherwise, so a reader can always see what the grouping
        /// covered up.
        /// </summary>
        public int RawClashes { get; set; }

        /// <summary>
        /// The most severe distance among a group's clashes.
        ///
        /// A hard clash reports a negative distance, which is how far the two things
        /// overlap, so the worst one is the most negative. A clearance test reports a
        /// positive distance, which is the gap, so the worst one is the smallest. Both
        /// come out of the same rule: the most severe is the minimum.
        ///
        /// An empty group falls back rather than inventing a number, because a group with
        /// no distances is a group the API told us nothing about.
        /// </summary>
        public static double MostSevere(IEnumerable<double> distances, double fallback)
        {
            bool any = false;
            double worst = 0.0;

            if (distances != null)
            {
                foreach (double distance in distances)
                {
                    if (!any || distance < worst)
                    {
                        worst = distance;
                    }

                    any = true;
                }
            }

            return any ? worst : fallback;
        }

        public string Position()
        {
            return X.ToString("0.###", CultureInfo.InvariantCulture) + ", "
                + Y.ToString("0.###", CultureInfo.InvariantCulture) + ", "
                + Z.ToString("0.###", CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>Why a test has no rows. Ran and found nothing is not the same as not run.</summary>
    public enum TestState
    {
        /// <summary>Ran and found nothing. Not the same as skipped.</summary>
        Passed,

        /// <summary>Ran and found something.</summary>
        FoundClashes,

        /// <summary>Never run. Not the same as passed.</summary>
        Skipped
    }

    /// <summary>
    /// One test, whether or not it ran. The Summary sheet carries one row per test in the
    /// file, so a test that never ran is here too, carrying why.
    /// </summary>
    public sealed class TestReport
    {
        private readonly List<ClashRow> rows = new List<ClashRow>();
        private readonly ClashTally tally = new ClashTally();

        public TestReport(int number, string name)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException("number", "Test numbers start at one.");
            }

            Number = number;
            Name = name ?? string.Empty;
            LeftLocator = string.Empty;
            RightLocator = string.Empty;
            State = TestState.Skipped;
            SkippedReason = string.Empty;
            TestTypeName = string.Empty;
            ToleranceUnits = string.Empty;
        }

        /// <summary>One based, and the number the sheet is named after.</summary>
        public int Number { get; private set; }

        /// <summary>The full test name, which the sheet name cannot hold.</summary>
        public string Name { get; private set; }

        public string LeftLocator { get; set; }

        public string RightLocator { get; set; }

        public int LeftItems { get; set; }

        public int RightItems { get; set; }

        public double Tolerance { get; set; }

        public string ToleranceUnits { get; set; }

        public string TestTypeName { get; set; }

        public TestState State { get; set; }

        /// <summary>Empty unless the state is Skipped.</summary>
        public string SkippedReason { get; set; }

        public double Seconds { get; set; }

        public ReadOnlyCollection<ClashRow> Rows
        {
            get { return new ReadOnlyCollection<ClashRow>(rows); }
        }

        public void Add(ClashRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            rows.Add(row);
            tally.Add(row.Status, row.RawClashes);
        }

        /// <summary>Counts by status, over the raw clashes rather than over the rows.</summary>
        public ClashTally Tally
        {
            get { return tally; }
        }

        /// <summary>Rows on the sheet. A group is one row however many clashes it holds.</summary>
        public int GroupCount
        {
            get { return rows.Count; }
        }

        /// <summary>Every clash behind those rows, so the grouping hides nothing.</summary>
        public int RawClashes
        {
            get
            {
                int raw = 0;

                foreach (ClashRow row in rows)
                {
                    raw += row.RawClashes;
                }

                return raw;
            }
        }

        /// <summary>
        /// New plus Active. Kept because it is what this tool counted before there was a
        /// choice, and never labelled "open", because the clash API has no open against
        /// closed notion. See docs/scan.md section 4h.
        /// </summary>
        public int NewPlusActive
        {
            get { return OpenClashes.Of(tally, OpenClashCount.NewAndActive); }
        }

        /// <summary>How many are still outstanding under whichever rule was chosen.</summary>
        public int OpenUnder(OpenClashCount which)
        {
            return OpenClashes.Of(tally, which);
        }

        /// <summary>
        /// How many are Resolved. Resolved clashes stay in the file and keep counting, so
        /// this is how the growth becomes visible before anyone decides to compact.
        /// </summary>
        public int Resolved
        {
            get { return tally.Of(ClashStatus.Resolved); }
        }

        /// <summary>A test with rows gets a sheet. One that found nothing does not.</summary>
        public bool HasSheet
        {
            get { return rows.Count > 0; }
        }

        public string SheetName()
        {
            return SheetNames.ForTest(Number);
        }

        /// <summary>What the Summary says in its outcome column.</summary>
        public string DescribeState()
        {
            switch (State)
            {
                case TestState.Passed:
                    return "passed, ran and found nothing";
                case TestState.FoundClashes:
                    return "ran";
                default:
                    return "skipped, not run"
                        + (SkippedReason.Length == 0 ? string.Empty : ", " + SkippedReason);
            }
        }

        public override string ToString()
        {
            return SheetName() + "  " + Name;
        }
    }

    /// <summary>
    /// Everything one workbook needs, built with no Navisworks types in it, so the whole
    /// of it can be built and written without Navisworks.
    /// </summary>
    public sealed class ClashReport
    {
        private readonly List<TestReport> tests = new List<TestReport>();

        public ClashReport(string building, string outputName)
        {
            Building = building ?? string.Empty;
            OutputName = outputName ?? string.Empty;
            OpenDocument = string.Empty;
            SourceFile = string.Empty;
            DocumentUnits = string.Empty;
            SetTreeRoot = "lcop_selection_set_tree";
            OpenCount = OpenClashes.Default;
            CompactedAway = -1;
        }

        public string Building { get; private set; }

        public string OutputName { get; private set; }

        /// <summary>The document the tests ran against. A count means nothing without it.</summary>
        public string OpenDocument { get; set; }

        /// <summary>The clash XML this run was given.</summary>
        public string SourceFile { get; set; }

        public string DocumentUnits { get; set; }

        public DateTime RunAt { get; set; }

        public string BuildStamp { get; set; }

        /// <summary>The prefix every set path is built with. A setting, not a constant.</summary>
        public string SetTreeRoot { get; set; }

        /// <summary>
        /// What the matrix counts as still outstanding. A setting with two choices, and
        /// the sheet says which one it used.
        /// </summary>
        public OpenClashCount OpenCount { get; set; }

        public double ClashStepSeconds { get; set; }

        /// <summary>Resolved clashes across every test, which is what compacting removes.</summary>
        public int TotalResolved
        {
            get { return Totals.Of(ClashStatus.Resolved); }
        }

        /// <summary>How many results compacting removed, or minus one when it was not run.</summary>
        public int CompactedAway { get; set; }

        public ReadOnlyCollection<TestReport> Tests
        {
            get { return new ReadOnlyCollection<TestReport>(tests); }
        }

        /// <summary>
        /// Adds the next test. The number is handed out here rather than by the caller, so
        /// the sheet names cannot repeat or skip.
        /// </summary>
        public TestReport AddTest(string name)
        {
            TestReport test = new TestReport(tests.Count + 1, name);
            tests.Add(test);
            return test;
        }

        public int CountOf(TestState state)
        {
            int count = 0;

            foreach (TestReport test in tests)
            {
                if (test.State == state)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Tests that ran, whether or not they found anything.</summary>
        public int RanCount
        {
            get { return CountOf(TestState.Passed) + CountOf(TestState.FoundClashes); }
        }

        public int TotalRawClashes
        {
            get
            {
                int raw = 0;

                foreach (TestReport test in tests)
                {
                    raw += test.RawClashes;
                }

                return raw;
            }
        }

        public ClashTally Totals
        {
            get
            {
                ClashTally total = new ClashTally();

                foreach (TestReport test in tests)
                {
                    total.Add(test.Tally);
                }

                return total;
            }
        }

        /// <summary>
        /// The discipline a set path belongs to, which is the first folder under the tree
        /// root. Read out of the path the file itself carries, never off a list in the
        /// code, because every project names its folders differently.
        ///
        /// lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals reads
        /// as Mechanical. A set sitting at the root has no folder, so it is its own group.
        /// </summary>
        public string DisciplineOf(string locator)
        {
            if (string.IsNullOrEmpty(locator))
            {
                return string.Empty;
            }

            string[] parts = locator.Split('/');
            int start = 0;

            if (parts.Length > 0
                && string.Equals(parts[0], SetTreeRoot, StringComparison.Ordinal))
            {
                start = 1;
            }

            // The last part is the set itself, so a folder only exists when there is
            // something between the root and the name.
            return parts.Length - start >= 2 ? parts[start] : string.Empty;
        }

        /// <summary>The set name on its own, which is the last part of the path.</summary>
        public static string SetNameOf(string locator)
        {
            if (string.IsNullOrEmpty(locator))
            {
                return string.Empty;
            }

            string[] parts = locator.Split('/');
            return parts[parts.Length - 1];
        }
    }
}
