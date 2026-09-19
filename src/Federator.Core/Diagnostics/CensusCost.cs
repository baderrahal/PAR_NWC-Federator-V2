using System;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// What the census costs, measured rather than assumed, and the rule that turns it
    /// down when it is not free.
    ///
    /// THE LOG MUST NEVER CHANGE WHAT THE RUN DOES. Counting five things around fourteen
    /// steps of every group is real work: the sets and the viewpoints are walked from
    /// their roots and the clash results are walked per test. If that costs a second a
    /// group it is costing a 22 group run most of half a minute, which is a log slowing
    /// the thing it is meant to be watching.
    ///
    /// So the cost is MEASURED, once per group, and said in the log. Over the threshold
    /// the census drops to the steps that may write and says in the log that it did.
    /// Nothing here estimates: the seconds come off the same monotonic clock the steps
    /// use, added up across every census the group took.
    /// </summary>
    public sealed class CensusCost
    {
        /// <summary>
        /// How long a group may spend counting before the census narrows, in seconds. A
        /// setting and not a constant, because the number shapes a run.
        /// </summary>
        public const double DefaultTooLongSeconds = 1.0;

        private static double tooLongSeconds = DefaultTooLongSeconds;

        public static double TooLongSeconds
        {
            get { return tooLongSeconds; }

            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", "A census cannot be allowed no time at all to run in.");
                }

                tooLongSeconds = value;
            }
        }

        private double seconds;
        private int taken;

        /// <summary>Records one census and what it cost. Never negative, for the same
        /// reason a step is never negative: a clock that went back is a fault in the
        /// machine and must not take time off the total.</summary>
        public void Took(double howLong)
        {
            if (howLong > 0)
            {
                seconds += howLong;
            }

            taken++;
        }

        /// <summary>How many censuses this group took.</summary>
        public int Taken
        {
            get { return taken; }
        }

        /// <summary>What they cost together.</summary>
        public double Seconds
        {
            get { return seconds; }
        }

        /// <summary>Whether the last group cost more than a group is allowed to.</summary>
        public bool TooExpensive
        {
            get { return seconds > TooLongSeconds; }
        }

        /// <summary>
        /// The one line a group writes about what its census cost. Written once per
        /// group and never per census, because the point is the total.
        /// </summary>
        public string Line()
        {
            return "CENSUS   cost " + Show(seconds) + " over " + taken
                + (taken == 1 ? " count" : " counts")
                + " in this group"
                + (TooExpensive
                    ? ". That is over the " + Show(TooLongSeconds)
                        + " a group is allowed, so from the next group the census is taken "
                        + "only around the steps that may write, and this line says so"
                    : ". That is inside the " + Show(TooLongSeconds) + " a group is allowed");
        }

        /// <summary>
        /// The line written at the top of a group whose census has already been narrowed,
        /// so no reader ever wonders why a step has no census around it.
        /// </summary>
        public static string NarrowedLine()
        {
            return "CENSUS   narrowed. An earlier group spent over " + Show(TooLongSeconds)
                + " counting, so the census is taken only around the steps that may change "
                + "the document: " + string.Join(", ", ToArray(CensusRule.StepsThatMayWrite()));
        }

        /// <summary>Starts the next group's count from nothing.</summary>
        public void NextGroup()
        {
            seconds = 0.0;
            taken = 0;
        }

        private static string Show(double howLong)
        {
            return howLong.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }

        private static string[] ToArray(System.Collections.Generic.IList<string> steps)
        {
            string[] all = new string[steps.Count];

            for (int i = 0; i < steps.Count; i++)
            {
                all[i] = steps[i];
            }

            return all;
        }
    }
}
