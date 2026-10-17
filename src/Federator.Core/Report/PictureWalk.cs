using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Report
{
    /// <summary>
    /// THE ORDER EVERY PICTURE IS TAKEN IN, F149. Once every test has run and every mirror merge
    /// is made: the tests in ReportOrder.Tests and each test's rows in its own order, the order
    /// ReportOrder.PictureNumbers numbers them in, so a picture numbered by
    /// ClashReport.NextPictureOf in this order is written under its final name and the renumber
    /// renames none. Set 09's 1A02MM spent 39 s of 253 s renaming 2931 pictures taken in the order
    /// the tests ran.
    ///
    /// THE STRETCHES. Consecutive rows of one test of the report read from one test of the
    /// document make one stretch, so the add-in resolves that test once and hides for it once,
    /// F145, for the whole stretch. A kept test holding the clashes only its mirror found after
    /// its own makes a stretch for each run of rows read from one test. A row that waits for no
    /// picture is passed and ends no stretch. The rule was the add-in's in attempt 1, untested,
    /// the breaker's seventh point, and lives here with no Navisworks type in it: the add-in
    /// hands in where each row was read from as a key.
    ///
    /// A ROW IS WALKED ONCE, the breaker's sixth point. The same row met under a second test of
    /// the report, which only a merge that threw between two mirrors of one kept test could
    /// leave, is passed the second time and named, so its picture is taken once and no second
    /// picture of it is written over its cell.
    ///
    /// THE TESTS AT OR UNDER THEIR CAP ARE WALKED FIRST, F203. The rule of F190 gives a test with
    /// fewer clashes than its cap a picture for each. The memory stop of F182 fired on every C04
    /// run since 14 Oct at about 3000 to 3300 pictures, and the walk went in report order, most
    /// clashes first, so every test after the stop had no picture at all. On the run of 15 Oct
    /// 14:11 that was 163 tests, all 162 under 50 clashes among them, while the 174 tests at or
    /// under their cap wanted 2518 pictures and the run made 3275. Walking those tests first, each
    /// part in report order, then the tests over their cap, keeps the rule wherever the room allows
    /// it, and the stop takes the last pictures of the largest tests instead. Where any test over
    /// its cap comes before one under it in report order the pictures are taken out of report
    /// order, so the renumber renames them to their final names.
    /// </summary>
    public static class PictureWalk
    {
        /// <param name="report">The report as it stands once every merge is made.</param>
        /// <param name="readFrom">
        /// The key of the test of the document a row was read from, or null where the row waits
        /// for no picture.
        /// </param>
        /// <param name="limitOf">The most pictures a test may have, its cap, F190 and F203.</param>
        public static PictureWalkOutcome Of(ClashReport report, Func<ClashRow, string> readFrom, Func<TestReport, int> limitOf)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            if (readFrom == null)
            {
                throw new ArgumentNullException("readFrom");
            }

            if (limitOf == null)
            {
                throw new ArgumentNullException("limitOf");
            }

            PictureWalkOutcome outcome = new PictureWalkOutcome();
            Dictionary<ClashRow, TestReport> metUnder = new Dictionary<ClashRow, TestReport>();
            PictureStretch last = null;

            foreach (TestReport test in UnderTheirCapFirst(report, readFrom, limitOf))
            {
                foreach (ClashRow row in test.Rows)
                {
                    string from = readFrom(row);

                    if (from == null)
                    {
                        continue;
                    }

                    TestReport first;

                    if (metUnder.TryGetValue(row, out first))
                    {
                        outcome.MetAgain(row, first, test);
                        continue;
                    }

                    metUnder.Add(row, test);

                    if (last == null || !ReferenceEquals(last.Into, test) || !string.Equals(last.ReadFrom, from, StringComparison.Ordinal))
                    {
                        last = new PictureStretch(test, from);
                        outcome.Add(last);
                    }

                    last.Add(row);
                }
            }

            return outcome;
        }

        /// <summary>
        /// The tests to walk, F203: first every test whose rows waiting for a picture are no more than its cap,
        /// then every other test, each part in report order.
        /// </summary>
        internal static IList<TestReport> UnderTheirCapFirst(ClashReport report, Func<ClashRow, string> readFrom, Func<TestReport, int> limitOf)
        {
            List<TestReport> under = new List<TestReport>();
            List<TestReport> over = new List<TestReport>();

            foreach (TestReport test in ReportOrder.Tests(report))
            {
                int waiting = 0;

                foreach (ClashRow row in test.Rows)
                {
                    if (readFrom(row) != null)
                    {
                        waiting++;
                    }
                }

                if (waiting <= limitOf(test))
                {
                    under.Add(test);
                }
                else
                {
                    over.Add(test);
                }
            }

            under.AddRange(over);
            return under;
        }
    }

    /// <summary>Consecutive rows of one test of the report read from one test of the document, F149.</summary>
    public sealed class PictureStretch
    {
        private readonly List<ClashRow> rows = new List<ClashRow>();

        internal PictureStretch(TestReport into, string readFrom)
        {
            Into = into;
            ReadFrom = readFrom;
        }

        /// <summary>The test of the report the rows are pictured under, and numbered in.</summary>
        public TestReport Into { get; private set; }

        /// <summary>The key of the test of the document the rows were read from, resolved once for all of them.</summary>
        public string ReadFrom { get; private set; }

        /// <summary>The rows in the order they are pictured, never empty.</summary>
        public IList<ClashRow> Rows
        {
            get { return new ReadOnlyCollection<ClashRow>(rows); }
        }

        internal void Add(ClashRow row)
        {
            rows.Add(row);
        }
    }

    /// <summary>What PictureWalk.Of found: the stretches in order, how many rows they hold, and every row met twice.</summary>
    public sealed class PictureWalkOutcome
    {
        private readonly List<PictureStretch> stretches = new List<PictureStretch>();
        private readonly List<string> metTwice = new List<string>();

        internal PictureWalkOutcome()
        {
        }

        public IList<PictureStretch> Stretches
        {
            get { return new ReadOnlyCollection<PictureStretch>(stretches); }
        }

        /// <summary>Every row of every stretch, each counted once, the whole of the pass's count.</summary>
        public int Rows
        {
            get
            {
                int all = 0;

                foreach (PictureStretch stretch in stretches)
                {
                    all += stretch.Rows.Count;
                }

                return all;
            }
        }

        /// <summary>One sentence for each row met under a second test of the report, said and taken once.</summary>
        public IList<string> MetTwice
        {
            get { return new ReadOnlyCollection<string>(metTwice); }
        }

        internal void Add(PictureStretch stretch)
        {
            stretches.Add(stretch);
        }

        internal void MetAgain(ClashRow row, TestReport first, TestReport again)
        {
            metTwice.Add(row.Name + " is a row of " + first.Name + " and again of " + again.Name
                + ", so its picture is taken once, under " + first.Name);
        }
    }
}
