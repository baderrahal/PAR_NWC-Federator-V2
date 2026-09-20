using System;
using System.Collections.Generic;

namespace Federator.Core.Sets
{
    /// <summary>What one set did across every group of a run, F82.</summary>
    public sealed class SetAcrossTheRun
    {
        internal SetAcrossTheRun(string path, string name)
        {
            Path = path ?? string.Empty;
            Name = name ?? string.Empty;
        }

        /// <summary>The full path, which is what the run is keyed on.</summary>
        public string Path { get; private set; }

        /// <summary>The set's own name, for reading.</summary>
        public string Name { get; private set; }

        /// <summary>How many groups this set was looked at in at all.</summary>
        public int GroupsSeen { get; internal set; }

        /// <summary>How many of those it found nothing in.</summary>
        public int GroupsAtZero { get; internal set; }

        /// <summary>How many groups it found something in.</summary>
        public int GroupsWithItems
        {
            get { return GroupsSeen - GroupsAtZero; }
        }

        /// <summary>
        /// Found nothing in EVERY group it was looked at in, which is the finding worth
        /// reading. A set at zero in 13 of 14 groups is a different thing entirely and is
        /// counted and not named.
        /// </summary>
        public bool FoundNothingAnywhere
        {
            get { return GroupsSeen > 0 && GroupsAtZero == GroupsSeen; }
        }

        /// <summary>What the model asked for, from the last group that said so.</summary>
        public string Asked { get; internal set; }

        public override string ToString()
        {
            return Path + "  " + GroupsAtZero + " of " + GroupsSeen;
        }
    }

    /// <summary>
    /// Which sets found nothing, across the whole run, F82.
    ///
    /// WHY THE RUN AND NOT THE GROUP. A set at zero in one group says a discipline was not
    /// exported for that building. A set at zero in EVERY group of a run says the set
    /// itself is wrong, and 38 of the client's 61 found nothing in all seven groups of the
    /// first real run. That second number was in the log seven times as seven separate
    /// group facts and nowhere as the one thing it means.
    ///
    /// IT COUNTS A SET THAT WAS ALREADY THERE AS WELL AS ONE THIS RUN CREATED. SetResult
    /// .IsZero is true only of a CREATED set that found nothing, because that is what the
    /// SETS totals and the NWF save decision read. On a weekly run every set is already
    /// there and carries Present, so IsZero is false for all 61 while 38 of them found
    /// nothing. Reading IsZero here would make the block empty on exactly the runs it was
    /// written for.
    ///
    /// A FAILED SET IS NOT A SET THAT FOUND NOTHING. A set that never resolved carries an
    /// item count of minus one, which is UNKNOWN and not zero, and it is left out of both
    /// counts here for the same reason a census minus one is never called a move. A
    /// SKIPPED set is not one either: it was never built, so there is nothing to say about
    /// what it found.
    /// </summary>
    public sealed class SetsAcrossTheRun
    {
        /// <summary>The title of the block, so nothing else spells it.</summary>
        public const string BlockTitle = "SETS ACROSS THE RUN";

        /// <summary>
        /// How many are named before the count takes over. TEN and not the five every other
        /// list reads off RunLog.KeptOfARepeat, A14. Why ten is UNKNOWN: F82 chose it and its
        /// log entry does not say, so it is left as F82 wrote it rather than changed on a guess.
        /// </summary>
        public const int ExamplesShown = 10;

        private readonly List<string> order = new List<string>();
        private readonly Dictionary<string, SetAcrossTheRun> byPath =
            new Dictionary<string, SetAcrossTheRun>(StringComparer.Ordinal);

        /// <summary>How many groups have been added.</summary>
        public int Groups { get; private set; }

        /// <summary>Every set, in the order it was first seen.</summary>
        public IList<SetAcrossTheRun> All()
        {
            List<SetAcrossTheRun> all = new List<SetAcrossTheRun>();

            foreach (string path in order)
            {
                all.Add(byPath[path]);
            }

            return all;
        }

        /// <summary>The sets that found nothing in every group they were looked at in.</summary>
        public IList<SetAcrossTheRun> FoundNothingAnywhere()
        {
            List<SetAcrossTheRun> found = new List<SetAcrossTheRun>();

            foreach (SetAcrossTheRun set in All())
            {
                if (set.FoundNothingAnywhere)
                {
                    found.Add(set);
                }
            }

            return found;
        }

        /// <summary>One group's sets, rolled in.</summary>
        public void Add(SetBuildOutcome outcome)
        {
            if (outcome == null)
            {
                return;
            }

            Groups++;

            foreach (SetResult result in outcome.Results)
            {
                if (result == null || result.ItemCount < 0)
                {
                    // Minus one is UNKNOWN and never zero. A set that failed to resolve
                    // says nothing about what it would have found.
                    continue;
                }

                SetAcrossTheRun set;

                if (!byPath.TryGetValue(result.Path, out set))
                {
                    set = new SetAcrossTheRun(result.Path, result.Name);
                    byPath.Add(result.Path, set);
                    order.Add(result.Path);
                }

                set.GroupsSeen = set.GroupsSeen + 1;

                if (result.ItemCount == 0)
                {
                    set.GroupsAtZero = set.GroupsAtZero + 1;
                }

                if (!string.IsNullOrEmpty(result.Asked))
                {
                    set.Asked = result.Asked;
                }
            }
        }

        /// <summary>
        /// The block. The sets that found nothing anywhere first, because that is the
        /// finding, then the counts. Ten are named and the rest are counted, and the block
        /// SAYS it truncated, because a truncated list that does not say so is the fault
        /// this log has already been caught by once.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            IList<SetAcrossTheRun> nowhere = FoundNothingAnywhere();

            lines.Add("groups in this run : " + Groups);
            lines.Add("sets looked at     : " + byPath.Count);
            lines.Add("found nothing in every group : " + nowhere.Count);

            int shown = 0;

            foreach (SetAcrossTheRun set in nowhere)
            {
                if (shown == ExamplesShown)
                {
                    break;
                }

                lines.Add("    " + set.Path + "  "
                    // A BARE UNKNOWN IS NOT A FINDING, it is a check that reported nothing.
                    // This block exists to say WHICH sets are wrong, and on a weekly run
                    // every set is already in the NWF, so its question was never read and
                    // every line of the block said UNKNOWN. Saying WHY turns it back into
                    // a fact a person can act on, and the action is Q72.
                    + (string.IsNullOrEmpty(set.Asked)
                        ? "asked UNKNOWN, because it was already in the NWF and this run never read its question. Q72"
                        : "asked " + set.Asked));
                shown++;
            }

            if (nowhere.Count > shown)
            {
                lines.Add("    and " + (nowhere.Count - shown)
                    + " more that found nothing in every group, counted and not listed");
            }

            if (nowhere.Count == 0)
            {
                lines.Add("Every set found something somewhere.");
            }

            return lines;
        }

        /// <summary>
        /// One row per set for the machine readable log, F82, because the block is written
        /// with Block and Block writes no row at all. Each is the path and the two counts.
        /// </summary>
        public IList<SetRunRow> Rows()
        {
            List<SetRunRow> rows = new List<SetRunRow>();

            foreach (SetAcrossTheRun set in All())
            {
                rows.Add(new SetRunRow(set.Path, set.GroupsAtZero, set.GroupsSeen));
            }

            return rows;
        }
    }

    /// <summary>One row of the run tally, for the machine readable log.</summary>
    public sealed class SetRunRow
    {
        internal SetRunRow(string path, int groupsAtZero, int groupsSeen)
        {
            Path = path;
            GroupsAtZero = groupsAtZero;
            GroupsSeen = groupsSeen;
        }

        public string Path { get; private set; }

        public int GroupsAtZero { get; private set; }

        public int GroupsSeen { get; private set; }

        /// <summary>The sentence the row carries beside its number.</summary>
        public string Phrase()
        {
            return "found nothing in " + GroupsAtZero + " of " + GroupsSeen
                + (GroupsSeen == 1 ? " group" : " groups");
        }
    }
}
