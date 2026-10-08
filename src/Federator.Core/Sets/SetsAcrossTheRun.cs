using System;
using System.Collections.Generic;
using Federator.Core.Coverage;

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

        private readonly List<string> order = new List<string>();
        private readonly Dictionary<string, SetAcrossTheRun> byPath =
            new Dictionary<string, SetAcrossTheRun>(StringComparer.Ordinal);

        // Paths whose count was not taken in some group. One that is never counted in any group is in
        // byPath nowhere, and the block says how many there are, so a set that failed in every group
        // is not left out of a sentence that says every set found something.
        private readonly HashSet<string> notCountedSomewhere = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// How many are named before the count takes over, nought for every one. It was a
        /// constant TEN that F82 chose for a reason its log entry does not say, and set 03's
        /// C06 run named 10 of its 14, log lines 8346 to 8357. Bader's request 2 under Q112
        /// asks for every set that found no items in any group of the run, so a set spelled
        /// wrong or pointing at nothing shows at once, and a number that shapes the run is a
        /// setting, CoverageSettings.SetsAtZeroNamedInTheRun, F127, whose default names every one.
        /// </summary>
        private readonly int named;

        /// <summary>The run's tally with the default settings, which name every set that found nothing.</summary>
        public SetsAcrossTheRun()
            : this(new CoverageSettings())
        {
        }

        public SetsAcrossTheRun(CoverageSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            named = settings.SetsAtZeroNamedInTheRun;
        }

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

            // A path is one set in a group however many sets of that name the group holds, so the group
            // is counted once for it. It found something where any set of that path did. It is at zero
            // only where every set of that path was counted and found nothing, and where the rest found
            // nothing and one was not counted the group is left out for that path, since the one not
            // counted may have found something. Two sets of one name read as two groups before, 2 of 2
            // in a run of one.
            List<string> inThisGroup = new List<string>();
            Dictionary<string, PathInAGroup> here = new Dictionary<string, PathInAGroup>(StringComparer.Ordinal);

            foreach (SetResult result in outcome.Results)
            {
                if (result == null || result.Path == null)
                {
                    continue;
                }

                PathInAGroup state;

                if (!here.TryGetValue(result.Path, out state))
                {
                    state = new PathInAGroup(result.Name);
                    here.Add(result.Path, state);
                    inThisGroup.Add(result.Path);
                }

                if (result.ItemCount < 0)
                {
                    // Minus one is UNKNOWN and never zero. A set that failed to resolve
                    // says nothing about what it would have found.
                    state.NotCounted = true;
                    continue;
                }

                if (result.ItemCount > 0)
                {
                    state.FoundItems = true;
                }

                if (!string.IsNullOrEmpty(result.Asked))
                {
                    state.Asked = result.Asked;
                }
            }

            foreach (string path in inThisGroup)
            {
                PathInAGroup state = here[path];

                if (state.NotCounted && !state.FoundItems)
                {
                    notCountedSomewhere.Add(path);
                    continue;
                }

                SetAcrossTheRun set;

                if (!byPath.TryGetValue(path, out set))
                {
                    set = new SetAcrossTheRun(path, state.Name);
                    byPath.Add(path, set);
                    order.Add(path);
                }

                set.GroupsSeen = set.GroupsSeen + 1;

                if (!state.FoundItems)
                {
                    set.GroupsAtZero = set.GroupsAtZero + 1;
                }

                if (!string.IsNullOrEmpty(state.Asked))
                {
                    set.Asked = state.Asked;
                }
            }
        }

        /// <summary>One set path as one group read it, before the group is counted.</summary>
        private sealed class PathInAGroup
        {
            internal PathInAGroup(string name)
            {
                Name = name;
            }

            internal string Name { get; private set; }

            internal bool FoundItems { get; set; }

            internal bool NotCounted { get; set; }

            internal string Asked { get; set; }
        }

        /// <summary>
        /// The block. The sets that found nothing anywhere first, because that is the
        /// finding, then the counts. Every one is named by default, and where the setting
        /// caps them the rest are counted and the block SAYS it truncated, because a
        /// truncated list that does not say so is the fault this log has already been caught
        /// by once.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            IList<SetAcrossTheRun> nowhere = FoundNothingAnywhere();

            // A set whose count was not taken in some groups found nothing in every group it was
            // looked at in, which is fewer than the groups counted here, and its line says so.
            int partial = 0;

            foreach (SetAcrossTheRun set in nowhere)
            {
                if (set.GroupsSeen < Groups)
                {
                    partial++;
                }
            }

            int neverCounted = 0;

            foreach (string path in notCountedSomewhere)
            {
                if (!byPath.ContainsKey(path))
                {
                    neverCounted++;
                }
            }

            lines.Add("groups in this run : " + Groups);
            lines.Add("sets looked at     : " + byPath.Count);

            if (neverCounted > 0)
            {
                lines.Add("sets never counted in any group : " + neverCounted + ", in none of the numbers here");
            }

            lines.Add("found nothing in every group : " + nowhere.Count
                + (partial > 0
                    ? ", " + partial + " of them looked at in fewer than the " + Groups + " groups above"
                    : string.Empty));

            int shown = 0;

            foreach (SetAcrossTheRun set in nowhere)
            {
                if (named > 0 && shown == named)
                {
                    break;
                }

                lines.Add("    " + set.Path + "  "
                    // A BARE UNKNOWN IS NOT A FINDING, it is a check that reported nothing.
                    // This block exists to say WHICH sets are wrong, and it used to say
                    // UNKNOWN on every line, because on a weekly run every set is already
                    // in the NWF and nothing read its question. 5w reads it now, off
                    // SelectionSet.Search, so what is printed is what the set in the
                    // DOCUMENT asks and never what the picked file asks. Where the two
                    // differ the SET DRIFT lines beside the group say both, Q72.
                    + (string.IsNullOrEmpty(set.Asked)
                        ? "asked UNKNOWN, and its search would not read, which is a fault in the set and not in this block"
                        : "asks " + set.Asked)
                    + (set.GroupsSeen < Groups
                        ? ", looked at in " + set.GroupsSeen + " of " + Groups + " groups only"
                        : string.Empty));
                shown++;
            }

            if (nowhere.Count > shown)
            {
                lines.Add("    and " + (nowhere.Count - shown)
                    + " more that found nothing in every group, counted and not listed");
            }

            if (byPath.Count == 0)
            {
                // Said over nothing it would read as a check that ran and found every set well.
                lines.Add("No set was counted, so nothing is said of what the sets found.");
            }
            else if (nowhere.Count == 0)
            {
                lines.Add(neverCounted > 0
                    ? "Every set that was counted found something somewhere."
                    : "Every set found something somewhere.");
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
                rows.Add(new SetRunRow(set.Path, set.GroupsAtZero, set.GroupsSeen, Groups));
            }

            return rows;
        }
    }

    /// <summary>One row of the run tally, for the machine readable log.</summary>
    public sealed class SetRunRow
    {
        // The groups the block counted, so a set looked at in fewer says so in its row as in the block.
        private readonly int groupsInTheRun;

        internal SetRunRow(string path, int groupsAtZero, int groupsSeen, int groupsInTheRun)
        {
            Path = path;
            GroupsAtZero = groupsAtZero;
            GroupsSeen = groupsSeen;
            this.groupsInTheRun = groupsInTheRun;
        }

        public string Path { get; private set; }

        public int GroupsAtZero { get; private set; }

        public int GroupsSeen { get; private set; }

        /// <summary>The sentence the row carries beside its number.</summary>
        public string Phrase()
        {
            return "found nothing in " + GroupsAtZero + " of " + GroupsSeen
                + (GroupsSeen == 1 ? " group" : " groups")
                + (GroupsSeen < groupsInTheRun
                    ? ", looked at in " + GroupsSeen + " of " + groupsInTheRun + " groups only"
                    : string.Empty);
        }
    }
}
