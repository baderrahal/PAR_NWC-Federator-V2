using System;
using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// How many clashes sit under each priority, F83, the way ClashTally counts by status.
    ///
    /// IT COUNTS CLASHES AND NOT TESTS. A run holds 1830 tests and a handful of them carry
    /// most of the clashes, so a count of tests per priority says nothing about how much
    /// work there is. How many A clashes there are is the number a person acts on.
    ///
    /// PRIORITY IS NOT STATUS. Nothing here names New, Active, Reviewed, Approved or
    /// Resolved, and ClashTally names no priority.
    /// </summary>
    public sealed class PriorityTally
    {
        private readonly Dictionary<ClashPriority, int> counts;

        public PriorityTally()
        {
            counts = new Dictionary<ClashPriority, int>();

            foreach (ClashPriority priority in Priorities.AllWithNone)
            {
                counts[priority] = 0;
            }
        }

        public int Of(ClashPriority priority)
        {
            int count;
            return counts.TryGetValue(priority, out count) ? count : 0;
        }

        /// <summary>How many clashes in all, added off the four and never counted twice.</summary>
        public int Total
        {
            get
            {
                int total = 0;

                foreach (ClashPriority priority in Priorities.AllWithNone)
                {
                    total += Of(priority);
                }

                return total;
            }
        }

        public void Add(ClashPriority priority, int clashes)
        {
            if (clashes <= 0)
            {
                return;
            }

            if (!counts.ContainsKey(priority))
            {
                counts[priority] = 0;
            }

            counts[priority] = counts[priority] + clashes;
        }

        /// <summary>Rolls one group's counts into a run total.</summary>
        public void Add(PriorityTally other)
        {
            if (other == null)
            {
                return;
            }

            foreach (ClashPriority priority in Priorities.AllWithNone)
            {
                Add(priority, other.Of(priority));
            }
        }

        /// <summary>
        /// The per group block. Every priority is named including the ones at zero,
        /// because a priority missing from the block reads as one nobody counted rather
        /// than as one with nothing under it.
        /// </summary>
        public IList<string> Lines()
        {
            List<string> lines = new List<string>();
            lines.Add(PriorityMap.Prefix + " clashes by priority, " + Total + " in all");

            foreach (ClashPriority priority in Priorities.AllWithNone)
            {
                lines.Add("         " + Priorities.Words(priority).PadRight(12) + ": " + Of(priority));
            }

            return lines;
        }

        /// <summary>
        /// The one RESULT line, or null where no priority file was picked. Null and not a
        /// line reading zero, for the reason the penetration line gives: a line that reads
        /// the same on every run teaches people to skip the block.
        /// </summary>
        public static string ResultLine(bool picked, PriorityTally acrossTheRun)
        {
            if (!picked || acrossTheRun == null)
            {
                return null;
            }

            List<string> said = new List<string>();

            foreach (ClashPriority priority in Priorities.AllWithNone)
            {
                said.Add(Priorities.Words(priority) + " " + acrossTheRun.Of(priority));
            }

            return "by priority    : " + string.Join(", ", said.ToArray())
                + ", " + acrossTheRun.Total + " in all";
        }
    }
}
