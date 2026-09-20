using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// How many clash tests point at a set that finds nothing, 3b, which is what a set
    /// finding nothing actually COSTS.
    ///
    /// WHY THIS IS NOT CreationPlan.NotCreatedCount, WHICH IS WHAT 3b USED FIRST AND WAS
    /// WRONG. That count is the tests this run did not CREATE because a side was empty,
    /// and it is only the tests that were ABSENT from the document to begin with. On a
    /// weekly run every test is already there, so almost nothing is absent and almost
    /// nothing is not created. The run of 2026-09-20 21:36 proved it: seven of the ten
    /// groups reported the cost as exactly 60 while holding between 37 and 55 sets that
    /// found nothing, because 60 was all that was absent. The real cost does not care
    /// whether the test was created today, last week or by a person in the panel.
    ///
    /// A SIDE NOBODY COUNTED IS NOT A SIDE FINDING NOTHING. A locator missing from the
    /// counts is one no set resolved, and it is left out of the cost rather than counted
    /// as empty, which is the rule CreationPlan already keeps for the same dictionary.
    /// The tests it could not judge are reported on their own, because a cost with a
    /// silent hole in it reads as a smaller cost.
    /// </summary>
    public static class EmptySideCost
    {
        /// <summary>
        /// How many of these tests have at least one side pointing at a locator that
        /// finds nothing, and how many could not be judged.
        /// </summary>
        public static EmptySideTally Count(
            IEnumerable<PlannedClashTest> tests, IDictionary<string, int> itemsByLocator)
        {
            EmptySideTally tally = new EmptySideTally();

            if (tests == null)
            {
                return tally;
            }

            foreach (PlannedClashTest test in tests)
            {
                if (test == null)
                {
                    continue;
                }

                int left;
                int right;

                if (!Counted(itemsByLocator, test.Left, out left)
                    || !Counted(itemsByLocator, test.Right, out right))
                {
                    tally.CouldNotTell++;
                    continue;
                }

                tally.Judged++;

                if (left == 0 || right == 0)
                {
                    tally.WithAnEmptySide++;
                }
            }

            return tally;
        }

        private static bool Counted(
            IDictionary<string, int> itemsByLocator, PlannedClashSide side, out int count)
        {
            count = 0;

            if (itemsByLocator == null || side == null || string.IsNullOrEmpty(side.Locator))
            {
                return false;
            }

            return itemsByLocator.TryGetValue(side.Locator, out count);
        }
    }

    /// <summary>What <see cref="EmptySideCost"/> found, kept apart so a hole in it is visible.</summary>
    public sealed class EmptySideTally
    {
        /// <summary>Tests with at least one side pointing at a set that finds nothing.</summary>
        public int WithAnEmptySide { get; set; }

        /// <summary>Tests both of whose sides were counted, so the answer means something.</summary>
        public int Judged { get; set; }

        /// <summary>Tests where a side's locator was not in the counts at all.</summary>
        public int CouldNotTell { get; set; }
    }
}
