using System;
using System.Collections.Generic;
using System.IO;

namespace Federator.Core.Report
{
    /// <summary>
    /// The one order every output uses, and the one place a picture gets its number.
    ///
    /// THE RULE. Tests in report order, which is most clashes first with ties in the order
    /// the tests were created, and inside each test the clashes in the order Clash
    /// Detective lists them, which is the order they were harvested. That is the order
    /// the Navisworks HTML tabular export lists the clashes, measured off both client
    /// exports in samples\client-report, and it is the order the rows are written and
    /// the pictures are numbered, so the two cannot drift.
    ///
    /// WHY THIS EXISTS. The pictures used to take their test number from the order the
    /// tests RAN, while the rows were written in report order. A test that ran fifth and
    /// had the most clashes was written first with pictures named cd04xxxx, and the
    /// client's export names that block cd00xxxx. Every link still opened its picture,
    /// but the numbers were not the export's numbers, and the report must match what
    /// Bader gets by hand.
    /// </summary>
    public static class ReportOrder
    {
        /// <summary>
        /// The tests in report order. THE ONE LIST EVERY WRITER WALKS, and the one place
        /// the order is decided, so the workbook, the clash XML and the picture numbering
        /// cannot disagree about it.
        ///
        /// THE MEASURED ORDER AND NOTHING ELSE, with a priority file picked or not: most
        /// clashes first, ties in the order the tests were created. That was read off both
        /// of the client's exports over all 1830 blocks and the tie rule is explicitly not
        /// alphabetical, including in one tie group of 1807.
        ///
        /// WITH A PRIORITY FILE PICKED the letters go in the Priority column, F83, and the
        /// order does not move, FR-199, Bader's answer to Q49 on 2026-10-06: the priority is a
        /// column to sort on in Excel, and the blocks stay in the order measured off the
        /// client's exports. F83 sorted them A, then B, then C, then the rest, by name inside
        /// each, an order the client never accepted, and every output followed it.
        ///
        /// THE PICTURES FOLLOW FOR FREE, because PictureNumbers walks this list and
        /// ImageRenumbering renames off that. Sorting the blocks anywhere else would leave
        /// every picture carrying a number from an order nothing else uses, which is the
        /// exact fault ImageRenumbering exists to stop.
        /// </summary>
        public static IList<TestReport> Tests(ClashReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            // THE MEASURED ORDER WHETHER OR NOT A PRIORITY FILE WAS PICKED, FR-199, Bader's
            // answer to Q49 on 2026-10-06: the priority is a column to sort on in Excel, and
            // the blocks stay in the order measured off the client's exports. Picking a file
            // sorted them A, then B, then C, then the rest, by name inside each, F83, and the
            // workbook, the clash XML and the picture numbers all followed into an order the
            // client never accepted.
            return report.InReportOrder();
        }

        /// <summary>
        /// Every clash in report order: tests as above, and inside each test the rows in
        /// the order Clash Detective listed them.
        /// </summary>
        internal static IList<ClashRow> Rows(ClashReport report)
        {
            List<ClashRow> rows = new List<ClashRow>();

            foreach (TestReport test in Tests(report))
            {
                foreach (ClashRow row in test.Rows)
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        /// <summary>
        /// The number every picture should carry, read off the report order. The test
        /// index counts the tests that carry at least one picture, from zero, in report
        /// order, so a test with no picture leaves no gap. The clash index counts the
        /// rows of that test that carry a picture, from one, in Clash Detective's order,
        /// so with every row pictured it is the row number.
        /// </summary>
        public static IList<PictureNumber> PictureNumbers(ClashReport report)
        {
            List<PictureNumber> numbers = new List<PictureNumber>();
            int testIndex = 0;

            foreach (TestReport test in Tests(report))
            {
                if (test.ImageCount == 0)
                {
                    continue;
                }

                int clashIndex = 0;

                foreach (ClashRow row in test.Rows)
                {
                    if (!row.HasImage)
                    {
                        continue;
                    }

                    clashIndex++;
                    numbers.Add(new PictureNumber(test, row, testIndex, clashIndex));
                }

                testIndex++;
            }

            return numbers;
        }

        /// <summary>The number one pictured row should carry, or null for a row with no picture.</summary>
        internal static PictureNumber PictureNumberFor(ClashReport report, ClashRow row)
        {
            foreach (PictureNumber number in PictureNumbers(report))
            {
                if (ReferenceEquals(number.Row, row))
                {
                    return number;
                }
            }

            return null;
        }
    }

    /// <summary>Where one picture sits in the report order, and so what it is called.</summary>
    public sealed class PictureNumber
    {
        internal PictureNumber(TestReport test, ClashRow row, int testIndex, int clashIndex)
        {
            Test = test;
            Row = row;
            TestIndex = testIndex;
            ClashIndex = clashIndex;
        }

        public TestReport Test { get; private set; }

        public ClashRow Row { get; private set; }

        /// <summary>Zero based, counting the tests with a picture in report order.</summary>
        public int TestIndex { get; private set; }

        /// <summary>One based, counting the pictured rows of the test in Clash Detective's order.</summary>
        public int ClashIndex { get; private set; }

        public string FileName
        {
            get { return ImageNaming.FileNameFor(TestIndex, ClashIndex); }
        }

        public override string ToString()
        {
            return FileName + "  " + Row.Name;
        }
    }

    /// <summary>
    /// Renames the pictures once, after the run, so every picture carries the number of
    /// its row in the report order.
    ///
    /// WHY A RENAME. A picture is rendered while its test runs, and the report order
    /// depends on the clash count of every test, which is only known when the last test
    /// has run. So a picture is rendered under a temporary number, the run order, and
    /// renamed here in one pass once the order is known. Two passes on disk, first to a
    /// holding name and then to the final one, because a swap between two tests would
    /// otherwise write one picture over another.
    ///
    /// What a picture shows and its size are untouched. Only the name changes, and the
    /// row's file, link and path are moved with it so the workbook, the XML and the page
    /// all point at the renamed file.
    /// </summary>
    public static class ImageRenumbering
    {
        private const string Holding = ".moving";

        public static ImageRenumberingOutcome Apply(ClashReport report, string workbookPath)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            ImageRenumberingOutcome outcome = new ImageRenumberingOutcome();

            if (string.IsNullOrEmpty(workbookPath))
            {
                return outcome;
            }

            IList<PictureNumber> numbers = ReportOrder.PictureNumbers(report);
            List<Move> moves = new List<Move>();

            foreach (PictureNumber number in numbers)
            {
                string from = number.Row.ImagePath;
                string to = ImageNaming.PathFor(workbookPath, number.TestIndex, number.ClashIndex);

                if (string.IsNullOrEmpty(from))
                {
                    outcome.Missing++;
                    continue;
                }

                if (string.Equals(Path.GetFullPath(from), Path.GetFullPath(to), StringComparison.OrdinalIgnoreCase))
                {
                    outcome.Unchanged++;
                    Point(number, workbookPath);
                    continue;
                }

                if (!File.Exists(from))
                {
                    outcome.Missing++;
                    outcome.Add(number.Row.Name + " should be " + number.FileName
                        + " and its picture is not on disk at " + from);
                    continue;
                }

                moves.Add(new Move(number, from, to));
            }

            List<Move> held = new List<Move>();
            List<Move> done = new List<Move>();

            // Pass one, every changing picture to a holding name, so no final name is
            // written while another picture still holds it. A move that throws is caught
            // at the move, FR-075, and everything the passes did so far is undone.
            foreach (Move move in moves)
            {
                try
                {
                    File.Move(move.From, move.From + Holding);
                }
                catch (Exception error)
                {
                    PutBack(move, error, done, held, outcome);
                    return outcome;
                }

                held.Add(move);
            }

            // Pass two, holding name to final name. A file already at a final name is not
            // one of this report's, since every changing picture is under a holding name
            // and no two rows share a number, so it is a picture an earlier run left.
            foreach (Move move in moves)
            {
                try
                {
                    if (File.Exists(move.To))
                    {
                        File.Delete(move.To);
                    }

                    File.Move(move.From + Holding, move.To);
                }
                catch (Exception error)
                {
                    PutBack(move, error, done, held, outcome);
                    return outcome;
                }

                held.Remove(move);
                done.Add(move);
            }

            // The rows follow only once every picture is at its final name, so a fault
            // above leaves every row pointing at the name its picture is still under.
            foreach (Move move in done)
            {
                Point(move.Number, workbookPath);
                outcome.Renamed++;
            }

            // The test's own index follows the report order too, so anything reading it
            // after this agrees with the file names.
            foreach (PictureNumber number in numbers)
            {
                number.Test.ImageIndex = number.TestIndex;
            }

            return outcome;
        }

        private static void Point(PictureNumber number, string workbookPath)
        {
            number.Row.ImageFile = number.FileName;
            number.Row.ImageLink = ImageNaming.LinkFor(workbookPath, number.TestIndex, number.ClashIndex);
            number.Row.ImagePath = ImageNaming.PathFor(workbookPath, number.TestIndex, number.ClashIndex);
        }

        /// <summary>
        /// Undoes what the two passes did before a move threw, so the folder is as it was
        /// before them, every picture under its run order name, which is the name its row
        /// still points at. FR-075: a throw part way used to leave the pictures not yet
        /// moved under their holding names with their rows pointing at the old names, and
        /// the log said the pictures not yet renamed keep their run order numbers, which
        /// was not the state on disk.
        ///
        /// The pictures at a final name go back to their holding name first, which is free
        /// because the pass vacated it, and then every holding name goes back to the name
        /// it came from, which is free because pass one vacated it and every final name is
        /// vacated again. One move straight from a final name to a run order name could
        /// write one picture over another, the swap the holding name exists for. Each put
        /// back is read off the disk, and one that fails is named with where its picture
        /// is, so a row pointing at a file that is not there is said and never silent.
        /// </summary>
        private static void PutBack(
            Move stoppedAt, Exception error, List<Move> done, List<Move> held, ImageRenumberingOutcome outcome)
        {
            int underHolding = held.Count;

            foreach (Move move in done)
            {
                try
                {
                    File.Move(move.To, move.From + Holding);
                    held.Add(move);
                }
                catch (Exception back)
                {
                    outcome.NotPutBack++;
                    outcome.Add(move.Number.Row.Name + " is at " + move.To + " and not at " + move.From
                        + ", where its row points, because moving it back threw " + Words(back));
                }
            }

            foreach (Move move in held)
            {
                string holding = move.From + Holding;

                try
                {
                    File.Move(holding, move.From);
                }
                catch (Exception back)
                {
                    outcome.NotPutBack++;
                    outcome.Add(move.Number.Row.Name + " is at " + holding + " and not at " + move.From
                        + ", where its row points, because moving it back threw " + Words(back));
                    continue;
                }

                // Read back off the disk, never taken from the move returning.
                if (File.Exists(move.From) && !File.Exists(holding))
                {
                    continue;
                }

                outcome.NotPutBack++;
                outcome.Add(move.Number.Row.Name + " was moved back to " + move.From
                    + " and reading the disk after it finds "
                    + (File.Exists(move.From) ? "the holding name still there" : "no file there"));
            }

            int touched = done.Count + underHolding;
            string count = done.Count == 0
                ? "no picture had been moved before it"
                : done.Count + (done.Count == 1 ? " picture" : " pictures") + " had been moved before it";
            string result = outcome.NotPutBack == 0
                ? "all " + touched + " were put back under their run order names, each read back off the disk"
                : outcome.NotPutBack + " of the " + touched
                    + " could not be put back and are named below, so their rows point at a file that is not where they say";

            outcome.AddFirst("the renaming stopped at " + stoppedAt.Number.Row.Name + ", "
                + Path.GetFileName(stoppedAt.From) + " to " + Path.GetFileName(stoppedAt.To)
                + ", which threw " + Words(error) + ". " + count + " and " + underHolding
                + (underHolding == 1 ? " was" : " were") + " under a holding name, and " + result);
        }

        private static string Words(Exception error)
        {
            return error.GetType().Name + ": " + error.Message;
        }

        private sealed class Move
        {
            internal Move(PictureNumber number, string from, string to)
            {
                Number = number;
                From = from;
                To = to;
            }

            internal PictureNumber Number { get; private set; }

            internal string From { get; private set; }

            internal string To { get; private set; }
        }
    }

    /// <summary>What the rename pass did, for the log.</summary>
    public sealed class ImageRenumberingOutcome
    {
        private readonly List<string> problems = new List<string>();

        /// <summary>Pictures whose run order number was already their report order number.</summary>
        public int Unchanged { get; internal set; }

        public int Renamed { get; internal set; }

        /// <summary>Rows that carry a picture name and whose file was not on disk.</summary>
        public int Missing { get; internal set; }

        /// <summary>
        /// Pictures that could not be put back after a move threw, FR-075. Each is named in
        /// the problems, and the first problem line carries how many had been moved and how
        /// many were under a holding name.
        /// </summary>
        public int NotPutBack { get; internal set; }

        internal IList<string> Problems
        {
            get { return problems; }
        }

        internal void Add(string problem)
        {
            problems.Add(problem);
        }

        /// <summary>The fault that stopped the renaming goes before the put backs it caused.</summary>
        internal void AddFirst(string problem)
        {
            problems.Insert(0, problem);
        }

        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            lines.Add("IMAGES   numbered in report order: " + Renamed + " renamed, "
                + Unchanged + " already right, " + Missing + " missing");

            foreach (string problem in problems)
            {
                lines.Add("IMAGES   " + problem);
            }

            return lines;
        }
    }
}
