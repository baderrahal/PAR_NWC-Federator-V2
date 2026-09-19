using System;
using System.Globalization;

namespace Federator.Core.Rerun
{
    /// <summary>What one reading of the model count means, F74.</summary>
    public enum LoadWaitVerdict
    {
        /// <summary>Not settled and not out of time. Pause and read again.</summary>
        KeepWaiting = 0,

        /// <summary>The count stopped changing and is a real count. Read the file list.</summary>
        Settled = 1,

        /// <summary>The ceiling was reached. Take the count as it stands and say so.</summary>
        Ceiling = 2
    }

    /// <summary>
    /// How long to wait for an opened NWF to have its models in it, F74.
    ///
    /// WHAT THE RUN SHOWED. All five existing NWFs reported "0 unchanged, 4 added, 0
    /// removed" and were thrown away and rebuilt, and the step that read them finished in
    /// 0.248 seconds. Every open in the one earlier log on record took between 2.3 and 4.8
    /// seconds and produced a file list. So TryOpenFile returning true does not mean the
    /// models are in the document, and reading Document.Models the instant it returns is
    /// reading a document that is still filling.
    ///
    /// NO NAVISWORKS TYPE IS IN HERE. The add-in reads the count and the clock and hands
    /// both over, and this decides what to do about them, so the rule can be proved
    /// without Navisworks and cannot drift between the run and the preview, which are two
    /// separate readers of the same thing.
    ///
    /// ZERO NEVER SETTLES, AND THAT IS THE POINT. Zero is exactly the reading that cannot
    /// be told apart from a document that has not started filling, so three zeros in a row
    /// prove nothing. A zero count is only accepted at the ceiling, and what happens then
    /// is not a rebuild: it is NwfComparison.ReadEmpty, which stops the group. A real
    /// empty NWF therefore costs one full ceiling and is then reported rather than
    /// silently rebuilt, which is the right way round, because rebuilding threw five
    /// federations and their clash history away.
    ///
    /// WHETHER THE API OFFERS SOMETHING BETTER IS UNKNOWN. If a member reports that a
    /// document has finished loading, this polling is the fallback and not the answer.
    /// That is the measurement in docs\history\scan.md 5e, which needs Navisworks on the
    /// machine, and this rule is written so the poll can be replaced without anything else
    /// moving.
    /// </summary>
    public sealed class ModelLoadWait
    {
        /// <summary>How many identical readings in a row mean the count has stopped moving.</summary>
        public const int DefaultSteadyReadings = 3;

        /// <summary>How long to pause between two readings.</summary>
        public const int DefaultPauseMilliseconds = 250;

        /// <summary>
        /// The longest this will ever wait for one NWF. Thirty seconds against an open
        /// that took 4.8 seconds on the slowest group of the one recorded run, so it is
        /// six times the worst reading anyone has and it is still bounded.
        /// </summary>
        public const double DefaultCeilingSeconds = 30.0;

        private readonly int steadyReadings;
        private readonly int pauseMilliseconds;
        private readonly double ceilingSeconds;

        private int readings;
        private int lastCount;
        private int sameInARow;
        private double seconds;
        private LoadWaitVerdict verdict;

        public ModelLoadWait()
            : this(DefaultSteadyReadings, DefaultPauseMilliseconds, DefaultCeilingSeconds)
        {
        }

        public ModelLoadWait(int steadyReadings, int pauseMilliseconds, double ceilingSeconds)
        {
            if (steadyReadings < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "steadyReadings",
                    "One reading is the fewest that can settle anything. "
                        + "Fewer than one would settle before it had read.");
            }

            if (pauseMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "pauseMilliseconds", "A pause cannot be shorter than no pause.");
            }

            if (ceilingSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "ceilingSeconds",
                    "A ceiling at or below zero would give up before the first reading, "
                        + "and a wait with no ceiling is a run that never ends.");
            }

            this.steadyReadings = steadyReadings;
            this.pauseMilliseconds = pauseMilliseconds;
            this.ceilingSeconds = ceilingSeconds;

            lastCount = -1;
            verdict = LoadWaitVerdict.KeepWaiting;
        }

        public int SteadyReadings
        {
            get { return steadyReadings; }
        }

        public int PauseMilliseconds
        {
            get { return pauseMilliseconds; }
        }

        public double CeilingSeconds
        {
            get { return ceilingSeconds; }
        }

        /// <summary>How many readings have been handed in.</summary>
        public int Readings
        {
            get { return readings; }
        }

        /// <summary>The last count read. Minus one before anything was read.</summary>
        public int LastCount
        {
            get { return lastCount; }
        }

        /// <summary>The elapsed seconds at the last reading.</summary>
        public double Seconds
        {
            get { return seconds; }
        }

        /// <summary>Where the wait stands now.</summary>
        public LoadWaitVerdict Verdict
        {
            get { return verdict; }
        }

        /// <summary>Whether the wait ended at the ceiling rather than by settling.</summary>
        public bool GaveUp
        {
            get { return verdict == LoadWaitVerdict.Ceiling; }
        }

        /// <summary>
        /// One reading of the model count, with the seconds the monotonic clock has
        /// counted since the open returned. The verdict says whether to read again.
        ///
        /// The ceiling is tested AFTER the settle, so a count that settles on the same
        /// reading that crosses the ceiling is reported as settled, which is what it is.
        /// </summary>
        public LoadWaitVerdict Read(int count, double elapsedSeconds)
        {
            if (verdict != LoadWaitVerdict.KeepWaiting)
            {
                return verdict;
            }

            readings = readings + 1;
            seconds = elapsedSeconds < 0 ? 0 : elapsedSeconds;

            if (count == lastCount)
            {
                sameInARow = sameInARow + 1;
            }
            else
            {
                sameInARow = 1;
                lastCount = count;
            }

            // A count above zero that has stopped moving is a loaded document. Zero is
            // the one reading that cannot be told apart from a document that has not
            // started filling, so it is never allowed to settle.
            if (count > 0 && sameInARow >= steadyReadings)
            {
                verdict = LoadWaitVerdict.Settled;
                return verdict;
            }

            if (seconds >= ceilingSeconds)
            {
                verdict = LoadWaitVerdict.Ceiling;
                return verdict;
            }

            return LoadWaitVerdict.KeepWaiting;
        }

        /// <summary>The words that begin every line this rule writes.</summary>
        public const string Prefix = "LOADING  ";

        /// <summary>
        /// The one line the wait writes, whichever way it ended. Every number on it was
        /// read, none is worked out from another, and a wait that gave up says so in the
        /// same words every time so it can be counted in a log.
        /// </summary>
        public string Line()
        {
            if (readings == 0)
            {
                return Prefix + "nothing was read, so the model count was never waited for";
            }

            if (verdict == LoadWaitVerdict.Settled)
            {
                return Prefix + "the NWF reported " + lastCount + Word(lastCount, " model", " models")
                    + " after " + Fixed(seconds) + "s, steady over " + steadyReadings
                    + " reads " + Fixed(pauseMilliseconds / 1000.0) + "s apart, over "
                    + readings + Word(readings, " reading", " readings") + " in all";
            }

            if (verdict == LoadWaitVerdict.Ceiling)
            {
                return Prefix + "the NWF was still reporting " + lastCount
                    + Word(lastCount, " model", " models") + " after " + Fixed(seconds)
                    + "s, which is the ceiling of " + Fixed(ceilingSeconds)
                    + "s, so the wait gave up and the count was taken as it stood";
            }

            return Prefix + "the wait was still going at " + Fixed(seconds) + "s, last count "
                + lastCount + ", over " + readings + Word(readings, " reading", " readings");
        }

        private static string Word(int count, string one, string many)
        {
            return count == 1 ? one : many;
        }

        private static string Fixed(double value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}
