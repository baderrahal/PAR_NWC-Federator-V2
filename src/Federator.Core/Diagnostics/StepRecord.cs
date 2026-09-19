using System;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// One closed step, kept so a group can say afterwards where its time went.
    ///
    /// A RECORD AND NOT THE STEP ITSELF. The step holds a clock and closes itself, and
    /// keeping thousands of them alive would keep thousands of closures alive with them.
    /// TESTS RUN is entered once per test and a real group has 1830 of those, so what is
    /// kept is four numbers and two strings.
    /// </summary>
    public sealed class StepRecord
    {
        public StepRecord(string group, string name, int depth, double startedAt, double seconds, bool threw)
        {
            if (!RunSteps.IsAStep(name))
            {
                throw new ArgumentException(RunSteps.NotAStep(name), "name");
            }

            Group = group;
            Name = name;
            Depth = depth < 0 ? 0 : depth;
            StartedAt = startedAt;
            Seconds = seconds;
            Threw = threw;
        }

        /// <summary>The building this step belonged to, or null outside a group.</summary>
        public string Group { get; private set; }

        public string Name { get; private set; }

        /// <summary>How many steps were open when this one opened. Zero at the top.</summary>
        public int Depth { get; private set; }

        /// <summary>Seconds since the run started, when the step opened.</summary>
        public double StartedAt { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>Whether the work inside it threw.</summary>
        public bool Threw { get; private set; }

        /// <summary>
        /// The one line a group writes for a step it entered more than once, because the
        /// start and finish pair is written for the first visit only and the rest are
        /// counted. Without this line a group that spent twelve minutes in TESTS RUN
        /// would carry one pair of lines about the first test and nothing about the other
        /// 1829.
        /// </summary>
        public static string RepeatedLine(string name, int visits, double seconds, int threw)
        {
            return "STEP     " + RunSteps.Padded(name) + "  " + visits
                + (visits == 1 ? " visit, " : " visits, ")
                + seconds.ToString("0.000", CultureInfo.InvariantCulture) + "s in total"
                + (threw > 0
                    ? ", " + threw + (threw == 1 ? " of them threw" : " of them threw")
                    : string.Empty);
        }

        /// <summary>
        /// The line written the second time a step opens inside one group, saying the
        /// rest are counted rather than written out. The same shape RunLog.Failure uses
        /// for a repeat, because a log that writes one line per test is a log nobody
        /// reads and one run left 17.8 MB of exactly that.
        /// </summary>
        public static string CountingFromHere(string name)
        {
            return "STEP     " + RunSteps.Padded(name)
                + "  entered again. Every further visit in this group is counted, not written out";
        }
    }
}
