using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;

namespace Federator.Core.Health
{
    /// <summary>
    /// What the shared coordinates rule did across ONE run, for the RESULT block and for the
    /// one list the run writes for the modellers, Bader's answer to Q99 and Q100: "the RESULT
    /// block lists these groups, and the run writes one short list Bader can forward to the
    /// modellers, one file for the run".
    ///
    /// ONE PER RUN, made when the run starts with that run's rule state, its start and its
    /// count of groups, and never kept on the window's log. It lived on the log until
    /// c5d8aa8, one per window, its rule state overwritten each run and its groups never
    /// cleared, so the second run of a window listed the first run's groups again under the
    /// second run's rule state, the breaker's first finding.
    ///
    /// EVERY GROUP THE RULE REACHED IS ADDED, judged or not, so the count of groups judged,
    /// not reached and not read, and of models not judged, is said beside any zero, and a
    /// run that judged three groups of 46 never reads as 46 clean ones, the breaker's fourth.
    /// </summary>
    public sealed class OffCoordinatesAcrossTheRun
    {
        private const string ListNameStart = "Models not on the same shared coordinates, run ";

        private readonly bool skipsTheClash;
        private readonly DateTime runStarted;
        private readonly int groupsInTheRun;
        private readonly List<Entry> entries = new List<Entry>();

        public OffCoordinatesAcrossTheRun(bool skipsTheClash, DateTime runStarted, int groupsInTheRun)
        {
            this.skipsTheClash = skipsTheClash;
            this.runStarted = runStarted;
            this.groupsInTheRun = groupsInTheRun;
        }

        /// <summary>
        /// The name of this run's list, its own file named for the second the run started.
        /// One fixed name let a smaller run, or the open file run of one group, write over
        /// the list of a fuller run, so every run's list is kept and none stands for another.
        /// </summary>
        public string ListName
        {
            get { return ListNameStart + runStarted.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture) + ".txt"; }
        }

        /// <summary>
        /// One group, added as its ALIGNMENT block is written: what the rule judged of its
        /// models, or null where the read threw, and whether this run would have run a clash
        /// test in it, which decides whether its clash is skipped.
        /// </summary>
        public void Add(string building, OffCoordinates off, bool runsATest)
        {
            entries.Add(new Entry(string.IsNullOrEmpty(building) ? "UNKNOWN" : building, off, runsATest, skipsTheClash));
        }

        /// <summary>The RESULT lines: what the rule did, how many groups it judged, and one line per group holding such a model.</summary>
        public IList<string> ResultLines()
        {
            List<string> lines = new List<string>();
            int skipped = Count(e => e.Skipped);

            if (skipsTheClash)
            {
                lines.Add("clash skipped  : " + Words.Counted(skipped, "group", "groups") + ", models not on the same shared coordinates"
                    + (skipped == 0
                        ? string.Empty
                        : ". In each " + OffCoordinates.TestsCreatedNoneRun.Substring(0, 1).ToLowerInvariant()
                            + OffCoordinates.TestsCreatedNoneRun.Substring(1)));
            }
            else
            {
                int clashed = Count(e => e.Off != null && e.Off.Any && e.RunsATest);

                lines.Add("clash skipped  : none, the rule that skips it was off for this run"
                    + (clashed == 0
                        ? string.Empty
                        : ", so " + Words.Counted(clashed, "group", "groups") + (clashed == 1 ? " was" : " were")
                            + " clashed with a model not on the same shared coordinates"));
            }

            lines.Add("coordinates    : " + Judged() + " of " + Words.Counted(groupsInTheRun, "group", "groups") + " judged" + NotJudgedResult());

            foreach (Entry entry in entries)
            {
                if (entry.Off != null && entry.Off.Any)
                {
                    lines.Add("      " + entry.Building.PadRight(10) + " " + entry.Off.Models.Count
                        + " model(s) not on the same shared coordinates, " + entry.Status());
                }
            }

            return lines;
        }

        /// <summary>
        /// The one list for the run, which Bader forwards to the modellers: how many groups
        /// the rule judged, each group holding such a model with the reference it was
        /// measured from, what this run did with it, and the same line per model the log
        /// carries. Says only what was checked, and never that no model was found off while
        /// any group or model was not judged.
        /// </summary>
        public IList<string> ForModellers()
        {
            List<string> lines = new List<string>();
            lines.Add("Models not on the same shared coordinates, from the run started "
                + runStarted.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            lines.Add("The rule judged " + Judged() + " of " + Words.Counted(groupsInTheRun, "group", "groups") + " of this run." + NotJudgedList());

            if (Count(e => e.Off != null && e.Off.Any) == 0)
            {
                lines.Add(Judged() == groupsInTheRun && ModelsNotJudged() == 0
                    ? "No model in this run was found off its group's shared coordinates, so no group is listed."
                    : "No group is listed, and that is not a clean bill for the groups and models above that were not judged.");
                return lines;
            }

            lines.Add(skipsTheClash
                ? "The rule that skips the clash was on for this run. The next run clashes each skipped group once its"
                    + " models are exported again on the project's shared coordinates."
                : "The rule that skips the clash was off for this run, so a group below that ran a clash test was"
                    + " clashed, and its clashes with the other disciplines cannot be trusted until these models are"
                    + " exported again on the project's shared coordinates.");

            foreach (Entry entry in entries)
            {
                if (entry.Off == null || !entry.Off.Any)
                {
                    continue;
                }

                lines.Add(string.Empty);
                lines.Add(entry.Building + (entry.Off.Reference == null
                    ? ", where no model could be placed to measure from"
                    : ", measured from the reference model " + entry.Off.Reference));
                lines.Add("   " + entry.Sentence());

                foreach (string model in entry.Off.Models)
                {
                    lines.Add("   " + model);
                }
            }

            return lines;
        }

        /// <summary>
        /// The ALIGNMENT run line, which ended "Nothing was changed and every group ran."
        /// while RESULT listed groups whose clash was skipped. It says what the rule did.
        /// The models counted are those the block calls different, over the 1 mm tolerance.
        /// </summary>
        public string AlignmentRunLine(int differentModels)
        {
            int skipped = Count(e => e.Skipped);
            string line = "ALIGNMENT across the run: " + differentModels
                + " model(s) sit somewhere their group's reference model does not";

            if (differentModels == 0 && skipped == 0)
            {
                return line;
            }

            return line + ". Nothing was changed in any model"
                + (skipped == 0
                    ? "."
                    : ", and the clash of " + Words.Counted(skipped, "group", "groups") + " was skipped, models not on the same shared"
                        + " coordinates, which RESULT lists.");
        }

        private int Judged()
        {
            return Count(e => e.Off != null && e.Off.ModelsRead > 0);
        }

        private int ModelsNotJudged()
        {
            int models = 0;

            foreach (Entry entry in entries)
            {
                if (entry.Off != null)
                {
                    models += entry.Off.NotJudged;
                }
            }

            return models;
        }

        /// <summary>The groups the rule never reached: the run stopped, or the group ended before its models were read.</summary>
        private int NotReached()
        {
            return Math.Max(0, groupsInTheRun - entries.Count);
        }

        /// <summary>The groups it reached and could not judge: the read threw, or no model was read.</summary>
        private int NotRead()
        {
            return entries.Count - Judged();
        }

        private string NotJudgedResult()
        {
            string said = string.Empty;

            if (NotReached() > 0)
            {
                said += ", " + NotReached() + " not reached, the run stopped or the group ended before its models were read";
            }

            if (NotRead() > 0)
            {
                said += ", " + NotRead() + " whose models could not be read";
            }

            if (ModelsNotJudged() > 0)
            {
                said += ", " + ModelsNotJudged() + " model(s) in the groups judged not judged, a placement or a site UNKNOWN";
            }

            return said;
        }

        private string NotJudgedList()
        {
            string said = string.Empty;

            if (NotReached() > 0)
            {
                said += " " + NotReached() + (NotReached() == 1 ? " was" : " were")
                    + " not reached, the run stopped or the group ended before its models were read.";
            }

            if (NotRead() > 0)
            {
                said += " " + NotRead() + " could not have their models read.";
            }

            if (ModelsNotJudged() > 0)
            {
                said += " " + ModelsNotJudged() + " model(s) in the groups judged could not be judged, a placement or a"
                    + " site UNKNOWN, and are not listed.";
            }

            return said;
        }

        private int Count(Predicate<Entry> which)
        {
            return entries.FindAll(which).Count;
        }

        /// <summary>One group as the rule left it.</summary>
        private sealed class Entry
        {
            public Entry(string building, OffCoordinates off, bool runsATest, bool ruleOn)
            {
                Building = building;
                Off = off;
                RunsATest = runsATest;
                Skipped = off != null && off.SkipsTheClash(ruleOn, runsATest);
            }

            public string Building { get; private set; }

            public OffCoordinates Off { get; private set; }

            public bool RunsATest { get; private set; }

            public bool Skipped { get; private set; }

            /// <summary>What this run did with the group, short, for its RESULT line.</summary>
            public string Status()
            {
                return Skipped
                    ? "clash skipped"
                    : !RunsATest
                        ? "no clash test to run, so nothing was skipped"
                        : "clashed, the rule was off";
            }

            /// <summary>The same, as a sentence, for the list.</summary>
            public string Sentence()
            {
                return Skipped
                    ? "its clash was skipped. " + OffCoordinates.TestsCreatedNoneRun + "."
                    : !RunsATest
                        ? "no clash test was to run in it this run, so nothing was skipped."
                        : "it was clashed, the rule being off.";
            }
        }
    }
}
