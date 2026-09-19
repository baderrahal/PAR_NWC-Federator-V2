using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// One step's share of one group, or of the whole run. Built by TimingBlock and read
    /// by nothing else, so the sorting and the arithmetic sit in one place.
    /// </summary>
    public sealed class TimedThing
    {
        public TimedThing(string name, double seconds, int visits, bool nested)
        {
            Name = name;
            Seconds = seconds;
            Visits = visits;
            Nested = nested;
        }

        public string Name { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>How many times it was entered. One for a group.</summary>
        public int Visits { get; private set; }

        /// <summary>
        /// Whether this ran INSIDE another step, so its seconds are already counted in
        /// that one. A nested thing is left out of the shares, because shares over
        /// everything would add up to more than the group took.
        /// </summary>
        public bool Nested { get; private set; }
    }

    /// <summary>
    /// Where the time went, per group and then for the whole run.
    ///
    /// EVERY NUMBER HERE IS MEASURED. The seconds come off the steps the run actually
    /// opened and closed and off the group clock the engine actually read. Nothing is
    /// estimated, nothing is rounded up into a claim, and where the block cannot account
    /// for a stretch of time it says so and names the difference rather than quietly
    /// spreading it over the steps it does know about.
    ///
    /// SHARES ARE WORKED OUT OVER THE TOP LEVEL STEPS ALONE. IMAGES runs inside HARVEST,
    /// so its seconds are already inside HARVEST's. Counting both would give a group a
    /// set of shares adding to more than a hundred, which is the kind of number that
    /// makes a reader stop trusting the rest of the block.
    /// </summary>
    public static class TimingBlock
    {
        /// <summary>The words the group block is titled with.</summary>
        public const string GroupTitle = "TIMING";

        /// <summary>The words the run block is titled with.</summary>
        public const string RunTitle = "TIMING, THE WHOLE RUN";

        /// <summary>
        /// The row that carries whatever the steps do not account for. It is named the
        /// same in both blocks so one reading answers the same question at either level.
        /// </summary>
        public const string OutsideEveryStep = "outside every step";

        /// <summary>The forty five minutes, in seconds, as the default.</summary>
        public const double DefaultUnattendedSeconds = 45.0 * 60.0;

        private static double unattendedSeconds = DefaultUnattendedSeconds;

        /// <summary>
        /// What done means for a whole run. Criterion 2: every ticked building runs
        /// unattended in under forty five minutes. A setting and not a constant, because
        /// the number shapes what the run says about itself, and static for the same
        /// reason RunLog.KeepLogs is: the block is written from the log, which opens
        /// before any options object exists. A number at or below zero is refused where
        /// it is set rather than at the moment the run needs it.
        /// </summary>
        public static double UnattendedSeconds
        {
            get { return unattendedSeconds; }

            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value", "A run cannot be asked to finish in no time at all.");
                }

                unattendedSeconds = value;
            }
        }

        /// <summary>
        /// The same steps the group block lists, as values rather than as lines, so the
        /// machine readable log carries the numbers the block carries and the two cannot
        /// say different things. Slowest first, the top level ones and then the nested.
        /// </summary>
        public static IList<TimedThing> ThingsForGroup(string building, IList<StepRecord> records)
        {
            IList<TimedThing> things = Gather(records, building);
            List<TimedThing> inOrder = new List<TimedThing>();

            foreach (TimedThing thing in Slowest(things, false))
            {
                inOrder.Add(thing);
            }

            foreach (TimedThing thing in Slowest(things, true))
            {
                inOrder.Add(thing);
            }

            return inOrder;
        }

        /// <summary>
        /// Every step of one group, slowest first, with its share of the group, then the
        /// group total. The nested steps come after, under a line saying their seconds
        /// are already inside the step above them.
        /// </summary>
        public static IList<string> ForGroup(string building, IList<StepRecord> records, double groupSeconds)
        {
            List<string> lines = new List<string>();
            IList<TimedThing> things = Gather(records, building);

            if (things.Count == 0)
            {
                lines.Add("no step was timed for this group");
                lines.Add(Total("group total", groupSeconds));
                return lines;
            }

            double counted = 0.0;

            foreach (TimedThing thing in things)
            {
                if (!thing.Nested)
                {
                    counted += thing.Seconds;
                }
            }

            foreach (TimedThing thing in Slowest(things, false))
            {
                lines.Add(Row(thing, groupSeconds));
            }

            // A ROW and not a footnote, and straight after the steps rather than at the
            // end, so the share column reads down to a hundred without a heading in the
            // middle of it. A reader sees at once whether the steps account for the group
            // or whether most of it is happening somewhere nothing times yet.
            lines.Add(Row(new TimedThing(OutsideEveryStep, groupSeconds - counted, 1, false), groupSeconds));

            lines.Add(string.Empty);
            lines.Add(Total("group total", groupSeconds));
            lines.Add(Unaccounted("this group", groupSeconds, counted));

            // Last, because these seconds are already inside a row above and a reader who
            // stops at the total has lost nothing.
            AddTheNestedOnes(lines, things, groupSeconds);

            return lines;
        }

        /// <summary>
        /// The run: every group with its total, slowest first, then the run total, then
        /// the same table by step name added across every group, then whether the run
        /// fitted in the forty five minutes done means.
        /// </summary>
        public static IList<string> ForRun(
            IList<GroupRecord> groups, IList<StepRecord> records, double runSeconds)
        {
            List<string> lines = new List<string>();

            lines.Add("by group, slowest first");

            List<TimedThing> byGroup = new List<TimedThing>();
            double inGroups = 0.0;

            if (groups != null)
            {
                foreach (GroupRecord group in groups)
                {
                    byGroup.Add(new TimedThing(group.Building, group.Seconds, 1, false));
                    inGroups += group.Seconds;
                }
            }

            if (byGroup.Count == 0)
            {
                lines.Add("no group ran");
            }
            else
            {
                foreach (TimedThing thing in Slowest(byGroup, false))
                {
                    lines.Add(Row(thing, runSeconds));
                }
            }

            lines.Add(Row(new TimedThing(OutsideEveryStep, runSeconds - inGroups, 1, false), runSeconds));

            lines.Add(string.Empty);
            lines.Add(Total("run total", runSeconds));
            lines.Add(Unaccounted("the run", runSeconds, inGroups));

            // The same numbers read the other way. Which BUILDING cost the run is one
            // question and which STEP cost it is a different one, and a run of twenty two
            // groups answers the second only when the steps are added across all of them.
            lines.Add(string.Empty);
            lines.Add("by step across every group, slowest first");

            IList<TimedThing> byStep = Gather(records, null);

            if (byStep.Count == 0)
            {
                lines.Add("no step was timed in this run");
            }
            else
            {
                double countedAcrossTheRun = 0.0;

                foreach (TimedThing thing in byStep)
                {
                    if (!thing.Nested)
                    {
                        countedAcrossTheRun += thing.Seconds;
                    }
                }

                foreach (TimedThing thing in Slowest(byStep, false))
                {
                    lines.Add(Row(thing, runSeconds));
                }

                lines.Add(Row(
                    new TimedThing(OutsideEveryStep, runSeconds - countedAcrossTheRun, 1, false),
                    runSeconds));

                AddTheNestedOnes(lines, byStep, runSeconds);
            }

            lines.Add(string.Empty);
            lines.Add(FittedInTheTime(runSeconds));

            return lines;
        }

        /// <summary>
        /// Whether the run fitted in the forty five minutes criterion 2 asks for, said
        /// plainly and with the measured number either way. Never a rounded claim: a run
        /// of 44 minutes 59 seconds says so in seconds as well.
        /// </summary>
        public static string FittedInTheTime(double runSeconds)
        {
            string took = "The run took " + Clock(runSeconds)
                + ", " + Show(runSeconds) + ".";

            if (runSeconds <= UnattendedSeconds)
            {
                return took + " That is inside the " + Clock(UnattendedSeconds)
                    + " a run has to finish in, by " + Clock(UnattendedSeconds - runSeconds) + ".";
            }

            return took + " That is OVER the " + Clock(UnattendedSeconds)
                + " a run has to finish in, by " + Clock(runSeconds - UnattendedSeconds) + ".";
        }

        /// <summary>
        /// The steps that ran inside another one, under a line saying their seconds are
        /// already counted. They are kept out of the share column so it reads down to a
        /// hundred, and listed rather than dropped because IMAGES taking most of HARVEST
        /// is exactly the sort of thing this block exists to show.
        /// </summary>
        private static void AddTheNestedOnes(
            List<string> lines, IList<TimedThing> things, double total)
        {
            IList<TimedThing> inside = Slowest(things, true);

            if (inside.Count == 0)
            {
                return;
            }

            lines.Add(string.Empty);
            lines.Add("inside another step, so these seconds are already counted above");

            foreach (TimedThing thing in inside)
            {
                lines.Add(Row(thing, total));
            }
        }

        /// <summary>
        /// Every step in the records, added up by name, for one group or for the whole
        /// run when the building is null.
        /// </summary>
        private static IList<TimedThing> Gather(IList<StepRecord> records, string building)
        {
            List<TimedThing> things = new List<TimedThing>();

            if (records == null)
            {
                return things;
            }

            foreach (string name in RunSteps.All)
            {
                double seconds = 0.0;
                int visits = 0;
                bool nested = false;

                foreach (StepRecord record in records)
                {
                    if (!string.Equals(record.Name, name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (building != null
                        && !string.Equals(record.Group, building, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    seconds += record.Seconds;
                    visits++;

                    // A step that ran inside another one even once is reported as nested,
                    // because its seconds are inside that one's and counting them twice
                    // is what would break the shares.
                    if (record.Depth > 0)
                    {
                        nested = true;
                    }
                }

                if (visits > 0)
                {
                    things.Add(new TimedThing(name, seconds, visits, nested));
                }
            }

            return things;
        }

        /// <summary>
        /// Slowest first. A tie is broken by the step order, so two steps of the same
        /// length read in the order a group meets them rather than in whatever order the
        /// sort happened to leave them.
        /// </summary>
        private static IList<TimedThing> Slowest(IList<TimedThing> things, bool nested)
        {
            List<TimedThing> wanted = new List<TimedThing>();

            foreach (TimedThing thing in things)
            {
                if (thing.Nested == nested)
                {
                    wanted.Add(thing);
                }
            }

            wanted.Sort(SlowestFirst);
            return wanted;
        }

        private static int SlowestFirst(TimedThing left, TimedThing right)
        {
            int bySeconds = right.Seconds.CompareTo(left.Seconds);

            if (bySeconds != 0)
            {
                return bySeconds;
            }

            int leftOrder = RunSteps.OrderOf(left.Name);
            int rightOrder = RunSteps.OrderOf(right.Name);

            if (leftOrder >= 0 && rightOrder >= 0 && leftOrder != rightOrder)
            {
                return leftOrder.CompareTo(rightOrder);
            }

            return string.CompareOrdinal(left.Name, right.Name);
        }

        private static string Row(TimedThing thing, double total)
        {
            return Name(thing.Name) + Show(thing.Seconds).PadLeft(12) + "  "
                + Share(thing.Seconds, total).PadLeft(6)
                + (thing.Visits > 1
                    ? "  " + thing.Visits + " visits"
                    : string.Empty);
        }

        private static string Total(string what, double seconds)
        {
            return Name(what) + Show(seconds).PadLeft(12);
        }

        /// <summary>
        /// The stretch of time the steps do not account for, named rather than hidden. A
        /// block that spread it over the steps it does know about would be inventing
        /// numbers, which is the one thing a timing block must not do.
        /// </summary>
        private static string Unaccounted(string what, double total, double counted)
        {
            double left = total - counted;

            if (left < 0.0005 && left > -0.0005)
            {
                return "every second of " + what + " is inside a step";
            }

            if (left < 0)
            {
                return "the steps add up to MORE than " + what + " took, by " + Show(-left)
                    + ". A step ran outside the stretch that was being timed";
            }

            return OutsideEveryStep + " is the work between them and the work nothing times yet";
        }

        /// <summary>
        /// How wide the first column is. Read off the longest thing that can go in it
        /// rather than typed, because a label wider than the column pushes every number
        /// on its row out of line and a block that does not line up does not get read.
        /// The building names are shorter than any of these.
        /// </summary>
        private static int ColumnWidth()
        {
            int widest = RunSteps.NameWidth;

            foreach (string label in new[] { OutsideEveryStep, "group total", "run total" })
            {
                if (label.Length > widest)
                {
                    widest = label.Length;
                }
            }

            return widest + 2;
        }

        private static string Name(string name)
        {
            return (name ?? string.Empty).PadRight(ColumnWidth());
        }

        private static string Share(double seconds, double total)
        {
            if (total <= 0.0)
            {
                return "UNKNOWN";
            }

            return (100.0 * seconds / total).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        }

        private static string Show(double seconds)
        {
            return seconds.ToString("0.000", CultureInfo.InvariantCulture) + "s";
        }

        /// <summary>Hours, minutes and seconds, for the sentence about the forty five.</summary>
        /// <summary>
        /// Seconds as a clock, h:mm:ss. Public since F80, because RunClock says where the
        /// session went in the same shape and two spellings of one duration in one block
        /// is how a reader stops trusting either.
        /// </summary>
        public static string Clock(double seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }

            int whole = (int)Math.Round(seconds);
            int hours = whole / 3600;
            int minutes = (whole % 3600) / 60;
            int rest = whole % 60;

            if (hours > 0)
            {
                return hours + (hours == 1 ? " hour " : " hours ")
                    + minutes + (minutes == 1 ? " minute " : " minutes ")
                    + rest + (rest == 1 ? " second" : " seconds");
            }

            if (minutes > 0)
            {
                return minutes + (minutes == 1 ? " minute " : " minutes ")
                    + rest + (rest == 1 ? " second" : " seconds");
            }

            return rest + (rest == 1 ? " second" : " seconds");
        }
    }
}
