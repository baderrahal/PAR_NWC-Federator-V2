using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// HOW MANY CLASHES THIS RUN DECIDED FOR HIM, per group and added up, for the RESULT
    /// block. One class, used twice, once for the penetration rule and once for the by
    /// design rule, because two copies of the same counting would drift.
    ///
    /// WHY IT EXISTS. The RESULT block carried one number per rule and no breakdown, so a
    /// run that moved a lot and a run that moved a little read the same until somebody
    /// opened ten per group blocks and added them up. The fixture of 2026-09-21 moved 7 of
    /// 14 under the penetration rule and 21 of 27 under the by design rule, which is half
    /// and three quarters. At that rate a run over twelve buildings sets a status on a
    /// large number of clashes in one pass.
    ///
    /// NEITHER RULE IS A FAULT AND THIS IS NOT A WARNING. Both are the tool doing exactly
    /// what a person ticked a box to ask for. But a person signing a report off should see
    /// HOW MANY DECISIONS IT MADE FOR HIM before he sends it, not after somebody asks.
    ///
    /// It carries LOOKED AT beside MOVED, because a number on its own cannot be judged: 7
    /// of 14 and 7 of 7000 are the same 7 and mean opposite things.
    /// </summary>
    public sealed class MovedAcrossTheRun
    {
        private readonly List<string> names = new List<string>();
        private readonly List<int> moved = new List<int>();
        private readonly List<int> lookedAt = new List<int>();

        public MovedAcrossTheRun(string what, string label)
        {
            What = string.IsNullOrEmpty(what) ? "moved" : what;
            Label = string.IsNullOrEmpty(label) ? "moved" : label;
        }

        /// <summary>What the rule does, in the words the block already uses.</summary>
        public string What { get; private set; }

        /// <summary>The RESULT line's left hand label, padded by the caller the way its neighbours are.</summary>
        public string Label { get; private set; }

        /// <summary>One group's numbers, added as its own block is written so the two cannot disagree.</summary>
        public void Add(string group, int clashesMoved, int clashesLookedAt)
        {
            names.Add(string.IsNullOrEmpty(group) ? "UNKNOWN" : group);
            moved.Add(clashesMoved < 0 ? 0 : clashesMoved);
            lookedAt.Add(clashesLookedAt < 0 ? 0 : clashesLookedAt);
        }

        public int Groups
        {
            get { return names.Count; }
        }

        public int Total
        {
            get
            {
                int total = 0;

                foreach (int one in moved)
                {
                    total += one;
                }

                return total;
            }
        }

        public int TotalLookedAt
        {
            get
            {
                int total = 0;

                foreach (int one in lookedAt)
                {
                    total += one;
                }

                return total;
            }
        }

        /// <summary>
        /// The RESULT lines, or empty where the rule was never asked for. A rule that was
        /// off has nothing to say and a line reading zero on every run teaches people to
        /// skip the block, which is the rule the priority line already keeps.
        ///
        /// THE PER GROUP ROWS ARE ONLY WRITTEN WHERE SOMETHING MOVED, so a run that moved
        /// nothing says so in one line rather than twelve rows of zero.
        /// </summary>
        public IList<string> ResultLines(bool wanted)
        {
            List<string> lines = new List<string>();

            if (!wanted)
            {
                return lines;
            }

            if (names.Count == 0)
            {
                lines.Add(Label + ": UNKNOWN, the rule was asked for and no group reported a count");
                return lines;
            }

            lines.Add(Label + ": " + Total.ToString(CultureInfo.InvariantCulture)
                + " clash(es) " + What + ", of " + TotalLookedAt + " looked at across "
                + names.Count + (names.Count == 1 ? " group" : " groups"));

            if (Total == 0)
            {
                return lines;
            }

            for (int i = 0; i < names.Count; i++)
            {
                if (moved[i] == 0)
                {
                    continue;
                }

                lines.Add("      " + names[i].PadRight(10) + " "
                    + moved[i].ToString(CultureInfo.InvariantCulture).PadLeft(6)
                    + " of " + lookedAt[i]);
            }

            return lines;
        }
    }
}
