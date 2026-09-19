using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The one line the window shows while the run works: which group, which building,
    /// which step, how long that step has been going and how long the run has.
    ///
    /// ONE ROUTE AND NOT TWO. The engine already hands progress out through a single
    /// callback, and everything here widens WHAT that callback carries rather than adding
    /// a second way out. Every sentence the run used to send still goes, with the group,
    /// the building, the step and the two clocks in front of it. Nothing about the
    /// threading changes: the caller is on the plugin thread and so is this.
    ///
    /// THE CLOCK IS THE RUN'S OWN, handed in as a function, monotonic, and never two wall
    /// clock readings subtracted. The seconds on the step are worked out from when the
    /// step started, so a step that has been open for ten minutes says ten minutes rather
    /// than nothing.
    ///
    /// WHAT THIS CANNOT DO, AND IT IS WORTH SAYING PLAINLY. It renders when something
    /// calls it. The run ticks it once per test, once per set and at every step, which
    /// covers everything with a loop in it. A single Navisworks call with no loop inside,
    /// which is what publishing an NWD is, cannot be ticked from the thread it is running
    /// on, so the line sits at the seconds it last showed. It does not pretend otherwise
    /// and nothing here starts a thread to make it look livelier than the run is.
    /// </summary>
    public sealed class LiveLine
    {
        /// <summary>
        /// How much longer than the group before a step has to run before the line says
        /// so. Twice, which is the number Bader asked for. A setting, because it shapes
        /// what the person watching is told.
        /// </summary>
        public const double DefaultSlowerThan = 2.0;

        private static double slowerThan = DefaultSlowerThan;

        public static double SlowerThan
        {
            get { return slowerThan; }

            set
            {
                if (value <= 1.0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", "A step is not slower than the one before until it is longer than it.");
                }

                slowerThan = value;
            }
        }

        /// <summary>How often the line is worth rendering again, in seconds.</summary>
        public const double EverySeconds = 1.0;

        private readonly Func<double> runSeconds;

        private int groupNumber;
        private int groupCount;
        private string building;
        private string step;
        private double stepStartedAt;
        private double lastGroupSeconds = -1.0;
        private double lastSaidAt = -1.0;

        public LiveLine(Func<double> runSeconds)
        {
            if (runSeconds == null)
            {
                throw new ArgumentNullException("runSeconds");
            }

            this.runSeconds = runSeconds;
        }

        /// <summary>Which group of how many, and which building.</summary>
        public void Groups(int number, int count, string building)
        {
            groupNumber = number;
            groupCount = count;
            this.building = building;
            step = null;
            lastGroupSeconds = -1.0;
        }

        /// <summary>
        /// How the line finds what the SAME step took on the group before. Set once by
        /// whoever built the line, so the pieces that open a step can say so without
        /// each of them having to carry the records around. Null where nothing supplied
        /// one, and then the line says nothing about the pace, which is the honest answer.
        /// </summary>
        public Func<string, double> PaceReader { get; set; }

        /// <summary>
        /// A step has started. The pace it is compared against comes from PaceReader, so
        /// every piece of the run that opens a step says so the same way.
        /// </summary>
        public void StepStarted(string name)
        {
            StepStarted(name, PaceReader == null ? -1.0 : PaceOf(name));
        }

        private double PaceOf(string name)
        {
            try
            {
                return PaceReader(name);
            }
            catch (Exception)
            {
                // Swallowed on purpose. A line about the pace is a diagnostic and a
                // diagnostic never stops a run. Minus one says nothing rather than
                // guessing.
                return -1.0;
            }
        }

        /// <summary>
        /// A step has started. <paramref name="onTheGroupBefore"/> is what the SAME step
        /// took on the group before, or minus one where there was no group before or the
        /// step was not in it. Minus one means the line says nothing about the pace, which
        /// is the honest answer for the first group of a run.
        /// </summary>
        public void StepStarted(string name, double onTheGroupBefore)
        {
            step = name;
            stepStartedAt = Now();
            lastGroupSeconds = onTheGroupBefore;
            lastSaidAt = -1.0;
        }

        /// <summary>The step has finished, so the line goes back to the group.</summary>
        public void StepEnded()
        {
            step = null;
            lastGroupSeconds = -1.0;
            lastSaidAt = -1.0;
        }

        /// <summary>
        /// Whether the line is worth rendering again. True when the step changed, and
        /// otherwise at most once a second, so a loop over 1830 tests repaints the window
        /// about as often as a person can read it rather than 1830 times.
        /// </summary>
        public bool ShouldSay()
        {
            return lastSaidAt < 0 || Now() - lastSaidAt >= EverySeconds;
        }

        /// <summary>How long the step has been going, or zero where none is open.</summary>
        public double SecondsOnStep()
        {
            if (string.IsNullOrEmpty(step))
            {
                return 0.0;
            }

            double now = Now();
            return now > stepStartedAt ? now - stepStartedAt : 0.0;
        }

        /// <summary>
        /// Whether this step has already run more than twice as long as the same step
        /// took on the group before. False where there is nothing to compare against,
        /// because a first group has no pace to be slower than.
        /// </summary>
        public bool SlowerThanLastGroup()
        {
            return lastGroupSeconds > 0 && SecondsOnStep() > lastGroupSeconds * SlowerThan;
        }

        /// <summary>
        /// The line, with the sentence the caller wanted to say on the end of it. Records
        /// that it was said, so ShouldSay holds the repaints down.
        /// </summary>
        public string Line(string sentence)
        {
            lastSaidAt = Now();

            List<string> parts = new List<string>();

            if (groupCount > 0)
            {
                parts.Add("Group " + groupNumber + " of " + groupCount);
            }

            if (!string.IsNullOrEmpty(building))
            {
                parts.Add(building);
            }

            if (!string.IsNullOrEmpty(step))
            {
                parts.Add(step);
                parts.Add(Clock(SecondsOnStep()) + " on this step");
            }

            parts.Add(Clock(Now()) + " on the run");

            if (SlowerThanLastGroup())
            {
                parts.Add("SLOWER, the group before took " + Clock(lastGroupSeconds) + " on this step");
            }

            if (!string.IsNullOrEmpty(sentence))
            {
                parts.Add(sentence);
            }

            return string.Join("  ", parts.ToArray());
        }

        /// <summary>
        /// What the same step took on a named group, off the records the log already
        /// keeps, so the pace comparison reads the same numbers the timing block does.
        /// Minus one where that group never ran that step.
        /// </summary>
        public static double OnTheGroupBefore(IList<StepRecord> records, string group, string step)
        {
            if (records == null || string.IsNullOrEmpty(group) || string.IsNullOrEmpty(step))
            {
                return -1.0;
            }

            double seconds = 0.0;
            bool found = false;

            foreach (StepRecord record in records)
            {
                if (string.Equals(record.Group, group, StringComparison.Ordinal)
                    && string.Equals(record.Name, step, StringComparison.Ordinal))
                {
                    seconds += record.Seconds;
                    found = true;
                }
            }

            return found ? seconds : -1.0;
        }

        /// <summary>
        /// The group before this one in the records, or null where this is the first.
        /// Read off the records rather than kept on the side, so there is one list
        /// everything about time is worked out from.
        /// </summary>
        public static string TheGroupBefore(IList<StepRecord> records, string thisGroup)
        {
            if (records == null)
            {
                return null;
            }

            string before = null;

            foreach (StepRecord record in records)
            {
                if (string.IsNullOrEmpty(record.Group))
                {
                    continue;
                }

                if (string.Equals(record.Group, thisGroup, StringComparison.Ordinal))
                {
                    continue;
                }

                before = record.Group;
            }

            return before;
        }

        private double Now()
        {
            double now = runSeconds();
            return now < 0 ? 0 : now;
        }

        /// <summary>
        /// Seconds under a minute, then minutes and seconds, then hours. A person
        /// watching a run reads 4m 02s and not 242.113s.
        /// </summary>
        public static string Clock(double seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }

            int whole = (int)seconds;

            if (whole < 60)
            {
                return whole + "s";
            }

            int hours = whole / 3600;
            int minutes = (whole % 3600) / 60;
            int rest = whole % 60;

            if (hours > 0)
            {
                return hours + "h " + minutes.ToString("00", CultureInfo.InvariantCulture)
                    + "m " + rest.ToString("00", CultureInfo.InvariantCulture) + "s";
            }

            return minutes + "m " + rest.ToString("00", CultureInfo.InvariantCulture) + "s";
        }
    }
}
