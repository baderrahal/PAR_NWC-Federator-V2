using System;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// One step of one group, opened when the work starts and closed when it ends.
    ///
    /// IT CLOSES ITSELF WHEN THE WORK THROWS. Every caller opens it in a using block, so
    /// a step is closed on the way out whether the work finished, returned early or
    /// threw. A step left open is the one thing that would make the timing block lie,
    /// because the seconds it never recorded come off no total and the run looks faster
    /// than it was. A step closed by a throw says so on its line and carries its seconds.
    ///
    /// THE CLOCK IS MONOTONIC AND IS NEVER TWO WALL CLOCK READINGS SUBTRACTED. The reader
    /// handed in comes off the run's Stopwatch. A clock that goes back, which it does
    /// when the machine syncs its time or the hour changes, would otherwise give a step a
    /// negative duration and a group a total smaller than one of its own steps.
    ///
    /// The clock is a function rather than a Stopwatch of its own so a test can drive it,
    /// which is how the line shapes are proved without a test that waits for a second to
    /// pass and fails on a slow machine.
    /// </summary>
    public sealed class RunStep : IDisposable
    {
        private readonly Func<double> clock;
        private readonly Action<RunStep> onClose;

        internal RunStep(string name, int depth, Func<double> clock, Action<RunStep> onClose)
        {
            if (!RunSteps.IsAStep(name))
            {
                throw new ArgumentException(RunSteps.NotAStep(name), "name");
            }

            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            Name = name;
            Depth = depth < 0 ? 0 : depth;
            this.clock = clock;
            this.onClose = onClose;
            StartedAt = clock();
            Seconds = 0.0;
        }

        /// <summary>The step's name, always one of <see cref="RunSteps"/>.</summary>
        public string Name { get; private set; }

        /// <summary>
        /// How many steps were already open when this one opened. Zero at the top. The
        /// timing block works its shares out over the steps at zero alone, because a step
        /// inside another one is counted in both and shares over all of them would add up
        /// to more than the group took.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>Seconds since the run started, off the monotonic clock.</summary>
        public double StartedAt { get; private set; }

        /// <summary>How long the step took. Zero until it closes.</summary>
        public double Seconds { get; private set; }

        /// <summary>Whether it has closed. A step never closed is reported as such.</summary>
        public bool Closed { get; private set; }

        /// <summary>Whether the work inside it threw. Set by Failed, never guessed.</summary>
        public bool Threw { get; private set; }

        /// <summary>
        /// What the step changed, in a few words, for the finish line. UNKNOWN where the
        /// caller said nothing, because a finish line with a blank on it reads as a step
        /// that did nothing rather than one nobody described.
        /// </summary>
        public string Phrase { get; private set; }

        /// <summary>
        /// What the step changed. Called by the work itself, just before it leaves, so the
        /// phrase describes what actually happened rather than what was going to.
        /// </summary>
        public void Changed(string phrase)
        {
            Phrase = phrase;
        }

        /// <summary>
        /// Records that the work inside threw. The step still closes and still carries its
        /// seconds, because time spent failing is time the run spent.
        /// </summary>
        public void Failed()
        {
            Threw = true;
        }

        /// <summary>
        /// Closes the step. Safe to call twice, because a using block calls it after the
        /// work already did, and a second close must not move the seconds.
        /// </summary>
        public void Dispose()
        {
            if (Closed)
            {
                return;
            }

            Closed = true;
            double now = clock();

            // Never negative. A clock that went backwards is a fault in the machine and
            // not in the run, and a negative step would take time off the group total.
            Seconds = now > StartedAt ? now - StartedAt : 0.0;

            if (onClose != null)
            {
                onClose(this);
            }
        }

        /// <summary>
        /// Closes the step and reports how long it has been open WITHOUT closing it,
        /// which is what the group end needs for a step nobody closed.
        /// </summary>
        public double SecondsSoFar()
        {
            if (Closed)
            {
                return Seconds;
            }

            double now = clock();
            return now > StartedAt ? now - StartedAt : 0.0;
        }

        // ---------- the two line shapes ----------

        /// <summary>The line written when the step opens.</summary>
        public string StartLine()
        {
            return Head() + "started";
        }

        /// <summary>
        /// The line written when it closes: the seconds and what it changed, or the word
        /// THREW where the work inside it did.
        /// </summary>
        public string FinishLine()
        {
            return Head() + (Threw ? "THREW    " : "finished ")
                + Show(Seconds) + "  " + Words.Or(Phrase, "UNKNOWN, the step said nothing about what it changed");
        }

        /// <summary>
        /// The line written for a step that was still open when its group ended. It is
        /// never silent, because a step nobody closed is the one thing that would let the
        /// timing block understate a run.
        /// </summary>
        public string NeverClosedLine()
        {
            return Head() + "NEVER CLOSED after " + Show(SecondsSoFar())
                + ". Its seconds are in the group total and the step never said it had finished";
        }

        private string Head()
        {
            return "STEP     " + new string(' ', Depth * 2) + RunSteps.Padded(Name) + "  ";
        }

        private static string Show(double seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }
    }
}
