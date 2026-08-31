using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Report
{
    /// <summary>What one cell of the matrix says.</summary>
    public enum MatrixCellKind
    {
        /// <summary>No test in the file pairs these two sets at all.</summary>
        NoTest,

        /// <summary>
        /// A test exists and did not run. This is NOT zero. Zero reads as coordinated and
        /// skipped does not, so the two can never share a cell value.
        /// </summary>
        Skipped,

        /// <summary>The test ran. The number is New plus Active.</summary>
        Ran
    }

    /// <summary>One cell. A count only means anything alongside what kind of cell it is.</summary>
    public sealed class MatrixCell
    {
        internal MatrixCell(MatrixCellKind kind, int newPlusActive, int rawClashes, int testNumber)
        {
            Kind = kind;
            NewPlusActive = newPlusActive;
            RawClashes = rawClashes;
            TestNumber = testNumber;
        }

        public MatrixCellKind Kind { get; private set; }

        /// <summary>Only meaningful when the kind is Ran.</summary>
        public int NewPlusActive { get; private set; }

        public int RawClashes { get; private set; }

        /// <summary>Zero when there is no test for this pair.</summary>
        public int TestNumber { get; private set; }

        /// <summary>
        /// What goes in the cell. A skipped pair reads "skipped" and never "0", because a
        /// zero says the two disciplines are coordinated and a skip says nobody looked.
        /// </summary>
        public string Text()
        {
            switch (Kind)
            {
                case MatrixCellKind.Ran:
                    return NewPlusActive.ToString();
                case MatrixCellKind.Skipped:
                    return Skipped;
                default:
                    return string.Empty;
            }
        }

        /// <summary>The exact word a skipped cell carries. One place, so it cannot drift.</summary>
        public const string Skipped = "skipped";

        public override string ToString()
        {
            return Text();
        }
    }

    /// <summary>
    /// Counts per pair of sets, sides down and across.
    ///
    /// The axes are the sets the tests actually name, and their discipline grouping comes
    /// from the folder names in the picked file, never from a list in the code, because
    /// every project names its folders differently.
    /// </summary>
    public sealed class ClashMatrix
    {
        private readonly List<string> locators = new List<string>();
        private readonly Dictionary<string, MatrixCell> cells =
            new Dictionary<string, MatrixCell>(StringComparer.Ordinal);
        private readonly ClashReport report;

        private ClashMatrix(ClashReport report)
        {
            this.report = report;
        }

        /// <summary>Every set named by a test, in first seen order.</summary>
        public ReadOnlyCollection<string> Locators
        {
            get { return new ReadOnlyCollection<string>(locators); }
        }

        public int Size
        {
            get { return locators.Count; }
        }

        /// <summary>The discipline for one axis entry, read off its folder.</summary>
        public string DisciplineOf(string locator)
        {
            return report.DisciplineOf(locator);
        }

        /// <summary>The set name for one axis entry, without its folders.</summary>
        public static string SetNameOf(string locator)
        {
            return ClashReport.SetNameOf(locator);
        }

        public static ClashMatrix From(ClashReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            ClashMatrix matrix = new ClashMatrix(report);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (TestReport test in report.Tests)
            {
                matrix.Remember(test.LeftLocator, seen);
                matrix.Remember(test.RightLocator, seen);
            }

            // The axes are sorted by discipline first and then by set name, so the block
            // of one discipline against another reads as a block.
            matrix.locators.Sort(matrix.CompareAxis);

            foreach (TestReport test in report.Tests)
            {
                MatrixCell cell = Cell(test);

                // Symmetric. A clash between A and B is the same clash as B against A, so
                // both halves of the grid carry it and neither half is left blank.
                matrix.Put(test.LeftLocator, test.RightLocator, cell);
                matrix.Put(test.RightLocator, test.LeftLocator, cell);
            }

            return matrix;
        }

        private static MatrixCell Cell(TestReport test)
        {
            if (test.State == TestState.Skipped)
            {
                return new MatrixCell(MatrixCellKind.Skipped, 0, 0, test.Number);
            }

            return new MatrixCell(
                MatrixCellKind.Ran, test.NewPlusActive, test.RawClashes, test.Number);
        }

        private void Remember(string locator, HashSet<string> seen)
        {
            if (!string.IsNullOrEmpty(locator) && seen.Add(locator))
            {
                locators.Add(locator);
            }
        }

        private int CompareAxis(string left, string right)
        {
            int byDiscipline = string.Compare(
                DisciplineOf(left), DisciplineOf(right), StringComparison.Ordinal);

            return byDiscipline != 0
                ? byDiscipline
                : string.Compare(SetNameOf(left), SetNameOf(right), StringComparison.Ordinal);
        }

        private void Put(string down, string across, MatrixCell cell)
        {
            string key = Key(down, across);

            // Two tests over one pair is a file that was built oddly, not a reason to
            // stop. The first is kept, exactly as two sets at one path are.
            if (!cells.ContainsKey(key))
            {
                cells.Add(key, cell);
            }
        }

        /// <summary>
        /// The cell for one pair. A pair no test covers comes back as NoTest rather than
        /// as null, so a caller can never read a missing pair as a zero.
        /// </summary>
        public MatrixCell At(string down, string across)
        {
            MatrixCell cell;

            return cells.TryGetValue(Key(down, across), out cell)
                ? cell
                : new MatrixCell(MatrixCellKind.NoTest, 0, 0, 0);
        }

        /// <summary>
        /// A separator that cannot turn up inside a set path, so two different pairs
        /// can never build the same key. An empty separator would make "a/b" against
        /// "c" and "a" against "/b/c" the same cell.
        /// </summary>
        private const string KeySeparator = "\u001F";

        private static string Key(string down, string across)
        {
            return (down ?? string.Empty) + KeySeparator + (across ?? string.Empty);
        }

        /// <summary>
        /// How many cells there are of each kind, counting the upper half only so a pair
        /// is counted once rather than twice.
        /// </summary>
        public int CountOf(MatrixCellKind kind)
        {
            int count = 0;

            for (int down = 0; down < locators.Count; down++)
            {
                for (int across = down + 1; across < locators.Count; across++)
                {
                    if (At(locators[down], locators[across]).Kind == kind)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
