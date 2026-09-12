using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The running total for one group's clash step. Every count is worked out from the
    /// same three lists the lines come from, so a count and a line can never disagree.
    ///
    /// Skipped and passed are separate numbers and are never added together. A skipped
    /// test did not run. A passed test ran and found nothing.
    /// </summary>
    public sealed class ClashRunOutcome
    {
        private readonly List<ClashTestResult> ran = new List<ClashTestResult>();
        private readonly List<SkippedClashTest> skipped = new List<SkippedClashTest>();
        private readonly List<string> created = new List<string>();
        private readonly List<string> alreadyPresent = new List<string>();

        /// <summary>How many clashtest elements the picked file held.</summary>
        public int TestsInFile { get; set; }

        /// <summary>The document the tests ran against, because a count means nothing without it.</summary>
        public string OpenDocument { get; set; }

        public ClashRunOutcome()
        {
            Compacted = -1;
        }

        /// <summary>Seconds the whole clash step took, creating and running included.</summary>
        public double Seconds { get; set; }

        /// <summary>
        /// How many Resolved clashes compacting removed, or minus one when compacting was
        /// not asked for. Resolved clashes stay in the file and keep counting until
        /// something takes them out.
        /// </summary>
        public int Compacted { get; set; }

        /// <summary>
        /// Set when the run was stopped before anything was created, because no test could
        /// resolve a set. A run that cannot clash anything says so and stops, rather than
        /// walking every test to report the same thing about each of them.
        /// </summary>
        public string StoppedReason { get; private set; }

        /// <summary>
        /// Set when this group failed so uniformly that the whole run was abandoned, not
        /// just this group. A run once spent 8 hours 52 minutes over 24 groups with every
        /// test failing the same way, and stopping the group would have saved none of it.
        /// </summary>
        public string StopTheRunReason { get; private set; }

        public bool StopTheRun
        {
            get { return !string.IsNullOrEmpty(StopTheRunReason); }
        }

        public void StopTheWholeRun(string reason)
        {
            StopTheRunReason = string.IsNullOrEmpty(reason)
                ? "UNKNOWN, the run was stopped and no reason was given"
                : reason;
        }

        public bool Stopped
        {
            get { return !string.IsNullOrEmpty(StoppedReason); }
        }

        /// <summary>
        /// Records the guard. One line, naming how many sets the document holds and how
        /// many the tests name, because those two numbers are the whole diagnosis.
        /// </summary>
        public string StopBecauseNoSetResolves(int setsInDocument, int setsExpected)
        {
            StoppedReason = "the document holds " + setsInDocument
                + (setsInDocument == 1 ? " set" : " sets")
                + " and the tests name " + setsExpected
                + (setsExpected == 1 ? " set" : " sets")
                + ", so no test can resolve a set. Nothing was created and nothing was run.";

            return StoppedReason;
        }

        public void AddCreated(string name)
        {
            created.Add(name);
        }

        /// <summary>
        /// A test already in the document under this name. An OPENED group keeps its
        /// results, so the test is left exactly as it is and never recreated.
        /// </summary>
        public void AddAlreadyPresent(string name)
        {
            alreadyPresent.Add(name);
        }

        public void AddSkipped(SkippedClashTest test)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            skipped.Add(test);
        }

        /// <summary>Records a skip and hands it back, so the caller can log it once.</summary>
        public SkippedClashTest AddSkipped(string name, ClashSkipReason kind, string reason)
        {
            SkippedClashTest test = new SkippedClashTest(name, kind, reason);
            skipped.Add(test);
            return test;
        }

        public ClashTestResult AddRan(
            string name, int leftItems, int rightItems, ClashTally tally, double seconds)
        {
            ClashTestResult result = new ClashTestResult(name, leftItems, rightItems, tally, seconds);
            ran.Add(result);
            return result;
        }

        public int CreatedCount
        {
            get { return created.Count; }
        }

        public int AlreadyPresentCount
        {
            get { return alreadyPresent.Count; }
        }

        /// <summary>Tests that did not run. Never added to the passed count.</summary>
        public int SkippedCount
        {
            get { return skipped.Count; }
        }

        public int RanCount
        {
            get { return ran.Count; }
        }

        /// <summary>Tests that ran and found nothing. Never added to the skipped count.</summary>
        public int PassedCount
        {
            get
            {
                int passed = 0;

                foreach (ClashTestResult result in ran)
                {
                    if (result.Passed)
                    {
                        passed++;
                    }
                }

                return passed;
            }
        }

        /// <summary>Tests that ran and found at least one clash.</summary>
        public int WithClashesCount
        {
            get { return RanCount - PassedCount; }
        }

        public ClashTally Totals
        {
            get
            {
                ClashTally total = new ClashTally();

                foreach (ClashTestResult result in ran)
                {
                    total.Add(result.Tally);
                }

                return total;
            }
        }

        public int TotalClashes
        {
            get { return Totals.Total; }
        }

        public IDictionary<ClashSkipReason, int> SkipReasonCounts()
        {
            return ClashTestPlan.CountReasons(skipped);
        }

        /// <summary>
        /// How many skipped tests are written out in full for each reason. One real run
        /// skipped 1830 tests for a single reason and wrote 1830 near identical lines into
        /// a 1 MB log, which buries everything worth reading. A count and a few examples
        /// say the same thing.
        /// </summary>
        public const int MaxSkipExamples = 5;

        /// <summary>
        /// One line per test that was created or run, then the skips summarised by reason,
        /// then the totals. Per test detail is kept for the tests that actually did
        /// something. The totals are counted off the same lists the lines came from, so
        /// they cannot disagree with them.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (StopTheRun)
            {
                lines.Add("RUN STOPPED " + StopTheRunReason);
                lines.Add(string.Empty);
            }

            if (Stopped)
            {
                lines.Add("STOPPED " + StoppedReason);
                lines.Add(string.Empty);
                lines.Add("ran against       : "
                    + (string.IsNullOrEmpty(OpenDocument) ? "UNKNOWN" : OpenDocument));
                lines.Add("tests in the file : " + TestsInFile);
                lines.Add("tests created     : 0");
                lines.Add("tests run         : 0");
                lines.Add("clash step took   : "
                    + Seconds.ToString("0.0", CultureInfo.InvariantCulture) + " seconds");
                return lines;
            }

            foreach (ClashTestResult result in ran)
            {
                lines.Add(result.Line());
            }

            lines.AddRange(SkipLines());

            lines.Add(string.Empty);
            lines.Add("ran against       : "
                + (string.IsNullOrEmpty(OpenDocument) ? "UNKNOWN" : OpenDocument));
            lines.Add("tests in the file : " + TestsInFile);
            lines.Add("tests created     : " + CreatedCount);

            if (AlreadyPresentCount > 0)
            {
                lines.Add("already there     : " + AlreadyPresentCount + ", left alone with their results");
            }

            lines.Add("tests skipped     : " + SkippedCount + ", not run and not passed");

            IDictionary<ClashSkipReason, int> reasonCounts = SkipReasonCounts();

            foreach (ClashSkipReason reason in SkipReasonsInOrder())
            {
                lines.Add("    " + reasonCounts[reason].ToString().PadLeft(5) + "  "
                    + ClashTestPlan.Describe(reason));
            }

            lines.Add("tests run         : " + RanCount);
            lines.Add("    passed        : " + PassedCount + ", ran and found nothing");
            lines.Add("    with clashes  : " + WithClashesCount);
            lines.Add("clashes found     : " + TotalClashes);

            ClashTally totals = Totals;

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                lines.Add("    " + status.ToString().PadRight(10) + ": " + totals.Of(status));
            }

            // The number nobody has. Kept on its own line and named plainly so it is easy
            // to find in a log that is thousands of lines long.
            lines.Add("clash step took   : " + Seconds.ToString("0.0", CultureInfo.InvariantCulture)
                + " seconds");

            return lines;
        }

        /// <summary>
        /// The skipped tests, grouped by reason. Each reason gets a count, at most
        /// MaxSkipExamples named examples, and then the number not shown, so nothing is
        /// hidden and nothing is repeated a thousand times.
        /// </summary>
        public IList<string> SkipLines()
        {
            List<string> lines = new List<string>();

            if (skipped.Count == 0)
            {
                return lines;
            }

            IDictionary<ClashSkipReason, int> counts = SkipReasonCounts();

            foreach (ClashSkipReason reason in SkipReasonsInOrder())
            {
                int count = counts[reason];

                lines.Add("SKIPPED " + count + (count == 1 ? " test, " : " tests, ")
                    + ClashTestPlan.Describe(reason));

                int shown = 0;

                foreach (SkippedClashTest test in skipped)
                {
                    if (test.Kind != reason)
                    {
                        continue;
                    }

                    if (shown == MaxSkipExamples)
                    {
                        break;
                    }

                    lines.Add("        " + test.Name + "  " + test.Reason);
                    shown++;
                }

                if (count > shown)
                {
                    lines.Add("        and " + (count - shown)
                        + " more skipped for the same reason, counted and not listed");
                }
            }

            return lines;
        }

        private IList<ClashSkipReason> SkipReasonsInOrder()
        {
            IDictionary<ClashSkipReason, int> counts = SkipReasonCounts();
            List<ClashSkipReason> order = new List<ClashSkipReason>();

            foreach (ClashSkipReason reason in
                new[]
                {
                    ClashSkipReason.SingleDiscipline,
                    ClashSkipReason.EmptySide,
                    ClashSkipReason.LocatorNotResolved,
                    ClashSkipReason.UnknownTestType,
                    ClashSkipReason.NoLocator,
                    ClashSkipReason.UnknownUnits,
                    ClashSkipReason.NoName,
                    ClashSkipReason.Failed
                })
            {
                if (counts.ContainsKey(reason))
                {
                    order.Add(reason);
                }
            }

            return order;
        }

        /// <summary>The one line summary, for the window and the group line in the log.</summary>
        public string Summary()
        {
            if (StopTheRun)
            {
                return "The run was stopped. " + StopTheRunReason;
            }

            if (Stopped)
            {
                return "Stopped before creating anything. " + StoppedReason;
            }

            return CreatedCount + " created, "
                + (AlreadyPresentCount > 0 ? AlreadyPresentCount + " already there, " : string.Empty)
                + RanCount + " run, "
                + PassedCount + " passed, "
                + SkippedCount + " skipped, "
                + TotalClashes + " clashes in "
                + Seconds.ToString("0.0", CultureInfo.InvariantCulture) + "s.";
        }
    }
}
