using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Federator.Core.Clash;

namespace Federator.Core.Report
{
    /// <summary>
    /// One side of one clash row.
    ///
    /// Family, type name, material, source file and discipline are read off every item and
    /// reach no output. They were put here so whoever has to fix a clash would not have to
    /// open the model, and then the workbook became the client's one sheet with none of
    /// ours on it and the page stayed theirs. Whether they go or wait for a column that
    /// does not exist yet is Q25.
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
            ItemType = string.Empty;
            IdLabel = ClientFormat.DefaultIdLabel;
            IdFrom = string.Empty;
            Layer = string.Empty;
        }

        public string Name { get; set; }

        /// <summary>
        /// The client's Item Type column, which is the Navisworks item type and reads
        /// Solid on every one of the 120 item cells in the accepted report. Not the same
        /// as <see cref="Type"/>, which is the Revit type name and is ours.
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        /// What Navisworks puts in front of the id, which it reads off whichever property
        /// carried it. Element ID on a Revit sourced NWC. A default, never a constant.
        /// </summary>
        public string IdLabel { get; set; }

        /// <summary>
        /// The display name of the property the id actually came from, which is not what
        /// the cell says. The cell says Element ID to match the client's own report, so
        /// this is where the real answer lives and it goes in the log.
        /// </summary>
        public string IdFrom { get; set; }

        /// <summary>
        /// The client's Layer column, which holds the level on both their exports.
        /// </summary>
        public string Layer { get; set; }

        /// <summary>
        /// The Layer cell, falling back to the clash's own level.
        ///
        /// The page writes the level into its layer element and the workbook was writing
        /// this property, which nothing ever filled, so the same row read LGF in the page
        /// and nothing in the workbook. Both read the same thing now.
        ///
        /// Theirs is a real per item Layer and is not the level: one of their rows carries
        /// GRF on item 1 with the level at LGF, and nothing at all on item 2. This tool
        /// does not read that property, so it carries the level in both files rather than
        /// leaving one of them empty.
        /// </summary>
        public string LayerOr(string level)
        {
            return string.IsNullOrEmpty(Layer) ? (level ?? string.Empty) : Layer;
        }

        /// <summary>The client's Item ID column, label and value in one field.</summary>
        public string ClientId()
        {
            return ClientFormat.ItemId(IdLabel, ElementId);
        }

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
            Description = string.Empty;
            ImageFile = string.Empty;
            ImageLink = string.Empty;
            ImagePath = string.Empty;
        }

        public string Name { get; set; }

        /// <summary>
        /// The client's Description column, which is the clash's own description and
        /// reads Hard (Conservative) on every row of the accepted report. Read off the
        /// result, never filled in from the test type here.
        /// </summary>
        public string Description { get; set; }

        /// <summary>The picture's file name, empty where none was written.</summary>
        public string ImageFile { get; set; }

        /// <summary>What the workbook cell links to, relative so the pair can be moved.</summary>
        public string ImageLink { get; set; }

        /// <summary>Where the picture actually is, for embedding a thumbnail.</summary>
        public string ImagePath { get; set; }

        /// <summary>True when this row has a picture beside the workbook.</summary>
        public bool HasImage
        {
            get { return !string.IsNullOrEmpty(ImageFile); }
        }

        /// <summary>The client's Grid Location column, grid and level in one field.</summary>
        public string ClientGridLocation()
        {
            return ClientFormat.GridLocation(GridLocation, Level);
        }

        /// <summary>The client's Clash Point column, the three coordinates in one field.</summary>
        public string ClientClashPoint()
        {
            return ClientFormat.ClashPoint(X, Y, Z);
        }

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
    /// One test, whether or not it ran. The report holds one of these per test in the
    /// file, so a test that never ran is here too, carrying why. Only the ones that found
    /// something get a block on the sheet.
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
            StatusWord = string.Empty;
            ImageIndex = -1;
        }

        /// <summary>Where this test sits in the file, from one.</summary>
        public int Number { get; private set; }

        /// <summary>The full test name, as the file carries it.</summary>
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

        /// <summary>
        /// The client's Status column on the test header. Navisworks writes its own word
        /// there and the accepted report says OK on all 1830 of its tests, which is the
        /// only value anyone here has seen. So this carries whatever the run read off the
        /// test rather than a translation, and it is left empty rather than invented.
        /// </summary>
        public string StatusWord { get; set; }

        /// <summary>
        /// This test's number in the picture names, zero based, or minus one until it
        /// writes one. It counts tests that write pictures rather than tests in the file,
        /// which is what makes cd000001 the first picture of the first test that has one.
        /// </summary>
        public int ImageIndex { get; set; }

        /// <summary>The client's Tolerance column, value and unit in one field.</summary>
        public string ClientTolerance()
        {
            return ClientFormat.Tolerance(Tolerance, ToleranceUnits);
        }

        /// <summary>How many rows on this sheet carry a picture.</summary>
        public int ImageCount
        {
            get
            {
                int count = 0;

                foreach (ClashRow row in rows)
                {
                    if (row.HasImage)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

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
        /// How many are Resolved. Resolved clashes stay in the file and keep counting, so
        /// this is how the growth becomes visible before anyone decides to compact.
        /// </summary>
        public int Resolved
        {
            get { return tally.Of(ClashStatus.Resolved); }
        }

        /// <summary>
        /// A test with rows gets a block on the sheet. One that found nothing does not.
        /// It was called HasSheet while every test had a sheet of its own.
        /// </summary>
        public bool HasRows
        {
            get { return rows.Count > 0; }
        }

        public override string ToString()
        {
            return Number + "  " + Name;
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
            CompactedAway = -1;
            Images = new ImageTally();
        }

        /// <summary>What the pictures cost for this group. Measured, never estimated.</summary>
        public ImageTally Images { get; private set; }

        /// <summary>
        /// The next number for a test that is about to write its first picture, and the
        /// same number again for a test that already has one.
        ///
        /// Handed out here rather than by the caller so the numbering cannot repeat or
        /// skip, the same reason the sheet numbers are.
        /// </summary>
        public int ImageIndexFor(TestReport test)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            if (test.ImageIndex >= 0)
            {
                return test.ImageIndex;
            }

            int next = 0;

            foreach (TestReport other in tests)
            {
                if (other.ImageIndex >= next)
                {
                    next = other.ImageIndex + 1;
                }
            }

            test.ImageIndex = next;
            return next;
        }

        /// <summary>
        /// Hands an index back where the test claimed one and then wrote nothing, which
        /// happens when its first render fails. Without this the numbering would carry a
        /// gap, and the numbering is the one thing about the pictures the client's own
        /// tooling might rely on.
        /// </summary>
        public void ReleaseImageIndex(TestReport test)
        {
            if (test != null && test.ImageCount == 0)
            {
                test.ImageIndex = -1;
            }
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

        public double ClashStepSeconds { get; set; }

        /// <summary>How many results compacting removed, or minus one when it was not run.</summary>
        public int CompactedAway { get; set; }

        public ReadOnlyCollection<TestReport> Tests
        {
            get { return new ReadOnlyCollection<TestReport>(tests); }
        }

        /// <summary>
        /// The tests in the order a report puts them: most clashes first, and where two
        /// hold the same number the order they were created in.
        ///
        /// MEASURED off both exports in samples\client-report on 2026-09-01. Both are
        /// strictly descending by clash count over all 1830 blocks. The tie rule was read
        /// off the tie groups against the order the tests sit in the exchange file, and it
        /// is that original order in every group, including one of 1807 tests, and it is
        /// NOT alphabetical in any of them.
        ///
        /// So it is a STABLE sort by count descending, which is why this carries the
        /// original position rather than handing the list to a comparer. List.Sort is not
        /// stable and would scramble the 1807.
        ///
        /// WHY THE OUTPUT IS SORTED AND NOT THE DOCUMENT. Whether their report is sorted
        /// by the report writer, or simply walks a tests collection that Clash Detective
        /// had already sorted, is UNKNOWN and cannot be read off the files. Sorting the
        /// document would mean calling DocumentClashTests.TestsSortTests, which is a
        /// mutator that reorders the tests inside the NWF, and the NWF is the record of
        /// what has been fixed. So the output is sorted and the document is left alone.
        /// </summary>
        public IList<TestReport> InReportOrder()
        {
            List<TestReport> sorted = new List<TestReport>(tests);

            // A stable sort by count descending. The index is the original position, so
            // two tests holding the same count come out in the order they were created.
            List<int> at = new List<int>();

            for (int i = 0; i < sorted.Count; i++)
            {
                at.Add(i);
            }

            at.Sort(delegate(int left, int right)
            {
                int byCount = sorted[right].RawClashes.CompareTo(sorted[left].RawClashes);

                return byCount != 0 ? byCount : left.CompareTo(right);
            });

            List<TestReport> ordered = new List<TestReport>();

            foreach (int i in at)
            {
                ordered.Add(sorted[i]);
            }

            return ordered;
        }

        /// <summary>
        /// Which property actually supplied each item's id, counted.
        ///
        /// The label on the report is always Element ID and this tool CHOOSES it rather
        /// than reading it off whichever property matched, so what was renamed has to stay
        /// visible somewhere, and that somewhere is the log. One line per property and
        /// never one per item: a group holds hundreds of items and the log has already been
        /// drowned once by a line each.
        ///
        /// Empty when the report holds no item at all, so the block is not written.
        /// </summary>
        public IList<string> IdSourceLines()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            int items = 0;

            foreach (TestReport test in tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    items += CountIdSource(row.Left, counts);
                    items += CountIdSource(row.Right, counts);
                }
            }

            List<string> lines = new List<string>();

            if (items == 0)
            {
                return lines;
            }

            List<string> names = new List<string>(counts.Keys);
            names.Sort(StringComparer.Ordinal);

            foreach (string name in names)
            {
                lines.Add(name + " supplied " + counts[name]
                    + (counts[name] == 1 ? " item id" : " item ids")
                    + " of " + items + ", written as \"" + ClientFormat.DefaultIdLabel + "\"");
            }

            return lines;
        }

        private static int CountIdSource(ClashItem item, IDictionary<string, int> counts)
        {
            if (item == null)
            {
                return 0;
            }

            string from = string.IsNullOrEmpty(item.IdFrom) ? "no id property" : item.IdFrom;
            int already;

            counts[from] = counts.TryGetValue(from, out already) ? already + 1 : 1;
            return 1;
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
    }
}
