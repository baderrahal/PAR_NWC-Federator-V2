using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The result statuses, the numbers of Autodesk.Navisworks.Api.Clash.ClashResultStatus
    /// read off the installed DLL and recorded in docs\scan.md. Repeated here because Core
    /// never references the Navisworks API.
    /// </summary>
    public enum ClashStatus
    {
        New = 0,
        Active = 1,
        Reviewed = 2,
        Approved = 3,
        Resolved = 4
    }

    /// <summary>Clash counts by status. The total is counted off the same numbers.</summary>
    public sealed class ClashTally
    {
        private readonly Dictionary<ClashStatus, int> counts = new Dictionary<ClashStatus, int>();

        /// <summary>The statuses in the order they are reported, so every block reads the same.</summary>
        public static readonly ClashStatus[] AllStatuses =
        {
            ClashStatus.New,
            ClashStatus.Active,
            ClashStatus.Reviewed,
            ClashStatus.Approved,
            ClashStatus.Resolved
        };

        public void Add(ClashStatus status)
        {
            Add(status, 1);
        }

        public void Add(ClashStatus status, int howMany)
        {
            int already;
            counts.TryGetValue(status, out already);
            counts[status] = already + howMany;
        }

        public void Add(ClashTally other)
        {
            if (other == null)
            {
                return;
            }

            foreach (ClashStatus status in AllStatuses)
            {
                Add(status, other.Of(status));
            }
        }

        public int Of(ClashStatus status)
        {
            int count;
            return counts.TryGetValue(status, out count) ? count : 0;
        }

        public int Total
        {
            get
            {
                int total = 0;

                foreach (ClashStatus status in AllStatuses)
                {
                    total += Of(status);
                }

                return total;
            }
        }

        /// <summary>New 12, Active 3. Only the statuses that carry a number.</summary>
        public string Describe()
        {
            StringBuilder text = new StringBuilder();

            foreach (ClashStatus status in AllStatuses)
            {
                int count = Of(status);

                if (count == 0)
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text.Append(", ");
                }

                text.Append(status).Append(' ').Append(count);
            }

            return text.Length == 0 ? "none" : text.ToString();
        }

        public override string ToString()
        {
            return Describe();
        }
    }

    /// <summary>The empty side rule, on its own so it can be tested without Navisworks.</summary>
    public static class ClashSideCheck
    {
        /// <summary>
        /// Most groups hold two or three disciplines, so most pairs have a side finding
        /// nothing in that model. A test with a side at zero cannot clash. It is SKIPPED,
        /// naming the empty side, and it is neither run nor passed, because a zero from a
        /// test that never ran reads exactly like a zero from a test that found nothing
        /// wrong.
        /// </summary>
        public static bool CanRun(
            PlannedClashTest test, int leftItems, int rightItems, out string reason)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            bool leftEmpty = leftItems <= 0;
            bool rightEmpty = rightItems <= 0;

            if (!leftEmpty && !rightEmpty)
            {
                reason = null;
                return true;
            }

            if (leftEmpty && rightEmpty)
            {
                reason = "both sides find nothing in this model, left \"" + test.Left.Locator
                    + "\" and right \"" + test.Right.Locator + "\"";
                return false;
            }

            reason = leftEmpty
                ? "the left side \"" + test.Left.Locator + "\" finds nothing in this model, the right finds "
                    + rightItems
                : "the right side \"" + test.Right.Locator + "\" finds nothing in this model, the left finds "
                    + leftItems;

            return false;
        }
    }

    /// <summary>One test that actually ran, and what it found.</summary>
    public sealed class ClashTestResult
    {
        internal ClashTestResult(
            string name, int leftItems, int rightItems, ClashTally tally, double seconds)
        {
            Name = name;
            LeftItems = leftItems;
            RightItems = rightItems;
            Tally = tally ?? new ClashTally();
            Seconds = seconds;
        }

        public string Name { get; private set; }

        public int LeftItems { get; private set; }

        public int RightItems { get; private set; }

        public ClashTally Tally { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>Ran and found nothing. This is what passed means, and it is not skipped.</summary>
        public bool Passed
        {
            get { return Tally.Total == 0; }
        }

        public string Line()
        {
            return (Passed ? "passed  " : "clashes ") + Name
                + "  items " + LeftItems + " v " + RightItems
                + "  " + Tally.Describe()
                + "  " + Seconds.ToString("0.0", CultureInfo.InvariantCulture) + "s";
        }

        public override string ToString()
        {
            return Line();
        }
    }

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

        /// <summary>Seconds the whole clash step took, creating and running included.</summary>
        public double Seconds { get; set; }

        public ReadOnlyCollection<ClashTestResult> Ran
        {
            get { return new ReadOnlyCollection<ClashTestResult>(ran); }
        }

        public ReadOnlyCollection<SkippedClashTest> Skipped
        {
            get { return new ReadOnlyCollection<SkippedClashTest>(skipped); }
        }

        public ReadOnlyCollection<string> Created
        {
            get { return new ReadOnlyCollection<string>(created); }
        }

        public ReadOnlyCollection<string> AlreadyPresent
        {
            get { return new ReadOnlyCollection<string>(alreadyPresent); }
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

        public void AddSkipped(string name, ClashSkipReason kind, string reason)
        {
            skipped.Add(new SkippedClashTest(name, kind, reason));
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
        /// One line per test, then the totals. The totals are counted off the same lists
        /// the lines came from, so they cannot disagree with them.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            foreach (ClashTestResult result in ran)
            {
                lines.Add(result.Line());
            }

            foreach (SkippedClashTest test in skipped)
            {
                lines.Add("SKIPPED " + test.Name + "  " + test.Reason);
            }

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

            foreach (ClashSkipReason reason in SkipReasonsInOrder())
            {
                int count = SkipReasonCounts()[reason];
                lines.Add("    " + count.ToString().PadLeft(5) + "  " + ClashTestPlan.Describe(reason));
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

        private IList<ClashSkipReason> SkipReasonsInOrder()
        {
            IDictionary<ClashSkipReason, int> counts = SkipReasonCounts();
            List<ClashSkipReason> order = new List<ClashSkipReason>();

            foreach (ClashSkipReason reason in
                new[]
                {
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
