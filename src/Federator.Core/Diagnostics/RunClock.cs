using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// How long the RUN took, as against how long the session was open, F80.
    ///
    /// WHAT THE RUN SHOWED. The headline read 1424 seconds and the run itself took 951.
    /// The other 473 were a person reading the group table and pressing Run, which is not
    /// work this tool did and is not time criterion 2 is about. Every percentage in the
    /// timing block was worked off the bigger number too, so every step read as a smaller
    /// share of the run than it really was.
    ///
    /// THE LOG STILL OPENS ON THE FIRST LINE OF THE BUTTON HANDLER and the session clock
    /// still starts there, because a run that dies at startup has to leave a file. So the
    /// run is TWO MARKS off that same monotonic clock and never a second stopwatch and
    /// never two wall clock readings subtracted. A clock that goes back, which is what a
    /// machine syncing its time does, would otherwise give a run a negative length.
    ///
    /// THREE STRETCHES AND NONE OF THEM VANISHES. Waiting for the person, the run itself,
    /// and whatever happened after the run finished, which is the result block being
    /// written. The third is small and is named rather than folded into either of the
    /// others, for the same reason the timing block has a row for everything outside every
    /// step.
    ///
    /// WITH NO RUN MARK AT ALL the run IS the session and the block SAYS it fell back. The
    /// open file run and the two hand buttons on the Clash step never write a run mark,
    /// because they are not a run over groups, and a block that divided by zero or
    /// reported a negative there would be worse than one that says what it did.
    /// </summary>
    public sealed class RunClock
    {
        /// <summary>What the waiting row is called, so nothing else spells it.</summary>
        public const string WaitingForThePerson = "waiting for the person";

        /// <summary>What the tail row is called.</summary>
        public const string AfterTheRun = "after the run finished";

        private RunClock(double session, double started, double finished, bool marked)
        {
            SessionSeconds = Never(session);
            Marked = marked;
            StartedAt = marked ? Never(started) : 0.0;
            FinishedAt = marked ? Never(finished) : SessionSeconds;
        }

        /// <summary>
        /// A run that was marked: the seconds on the session clock when Run was pressed
        /// and when the last group finished.
        /// </summary>
        public static RunClock From(double sessionSeconds, double startedAt, double finishedAt)
        {
            return new RunClock(sessionSeconds, startedAt, finishedAt, true);
        }

        /// <summary>
        /// No run mark at all. The run is the session and the block says so. This is what
        /// the open file run and the two hand buttons give.
        /// </summary>
        public static RunClock NotMarked(double sessionSeconds)
        {
            return new RunClock(sessionSeconds, 0.0, sessionSeconds, false);
        }

        /// <summary>Whether a run was marked at all.</summary>
        public bool Marked { get; private set; }

        /// <summary>The whole time the log has been open.</summary>
        public double SessionSeconds { get; private set; }

        /// <summary>Where the run started on the session clock.</summary>
        public double StartedAt { get; private set; }

        /// <summary>Where it finished.</summary>
        public double FinishedAt { get; private set; }

        /// <summary>
        /// RUN started to RUN finished, which is the number criterion 2 is about and the
        /// number every share in the timing block is worked off. Never negative: a clock
        /// that went back gives no time rather than time owed.
        /// </summary>
        public double RunSeconds
        {
            get { return Never(FinishedAt - StartedAt); }
        }

        /// <summary>Between the window opening and Run being pressed.</summary>
        public double WaitingSeconds
        {
            get { return Never(StartedAt); }
        }

        /// <summary>
        /// Between the last group finishing and now, which is the result block being
        /// written. Small, named, and never folded into either of the other two.
        /// </summary>
        public double AfterSeconds
        {
            get { return Never(SessionSeconds - FinishedAt); }
        }

        /// <summary>
        /// The lines that say where the session went, F80. Every number is read off the
        /// one clock and none is worked out from another except the tail, which is what is
        /// left, and the line says so.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();

            if (!Marked)
            {
                lines.Add("no run was marked, so the run time below IS the session time. "
                    + "That is what the open file run and the two hand buttons give");
                lines.Add(Row("session", SessionSeconds));
                return lines;
            }

            lines.Add(Row("session", SessionSeconds));
            lines.Add(Row(WaitingForThePerson, WaitingSeconds));
            lines.Add(Row("the run", RunSeconds));
            lines.Add(Row(AfterTheRun, AfterSeconds));
            lines.Add("the run is RUN started to RUN finished, and every share below is "
                + "worked off it. The other two are not work this tool did");

            return lines;
        }

        private static string Row(string what, double seconds)
        {
            return what.PadRight(24) + ": " + TimingBlock.Clock(seconds)
                + ", " + seconds.ToString("0.0", CultureInfo.InvariantCulture) + " seconds";
        }

        private static double Never(double seconds)
        {
            // A clock that went back gives no time rather than time owed, which is the
            // same rule a step's own seconds follow.
            return seconds < 0 || double.IsNaN(seconds) ? 0.0 : seconds;
        }
    }
}
