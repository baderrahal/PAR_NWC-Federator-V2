using System;
using System.Collections.Generic;

namespace Federator.Core.Report
{
    /// <summary>
    /// How many gaps a whole run found, counted by NAME and not by line.
    ///
    /// WHY DISTINCT AND NOT ADDED UP. The same six are held back on every group, so a run
    /// of twenty two groups would report 132 gaps, which says nothing except that there
    /// were twenty two groups. The number that means something is how many distinct
    /// things this tool knows and does not show, and that is six however many buildings
    /// it ran over.
    /// </summary>
    public sealed class GapTally
    {
        private readonly List<string> names = new List<string>();
        private int groups;

        /// <summary>Records one group's gaps and the group itself.</summary>
        public void Add(IList<ReportGap> gaps)
        {
            groups++;

            if (gaps == null)
            {
                return;
            }

            foreach (ReportGap gap in gaps)
            {
                if (gap == null || string.IsNullOrEmpty(gap.Name))
                {
                    continue;
                }

                if (!names.Contains(gap.Name))
                {
                    names.Add(gap.Name);
                }
            }
        }

        /// <summary>How many distinct things this run measured and showed nowhere.</summary>
        public int Distinct
        {
            get { return names.Count; }
        }

        /// <summary>How many groups were counted.</summary>
        public int Groups
        {
            get { return groups; }
        }

        /// <summary>Every name, in the order they were first seen.</summary>
        public IList<string> Names
        {
            get { return new List<string>(names); }
        }

        /// <summary>
        /// The one line at the end of the run. Written even when there is nothing to
        /// report, because a line saying none is a check that ran and silence is not.
        /// </summary>
        public string Line()
        {
            if (groups < 1)
            {
                return "GAPS     no group ran, so nothing was looked at";
            }

            if (names.Count < 1)
            {
                return "GAPS     none over " + groups + (groups == 1 ? " group" : " groups")
                    + ". Every number this run measured reached an output";
            }

            return "GAPS     " + names.Count
                + (names.Count == 1 ? " thing is" : " things are")
                + " measured on every item and reach no output, over "
                + groups + (groups == 1 ? " group" : " groups") + ": "
                + string.Join(", ", names.ToArray())
                + ". Each is a question rather than a fault and nothing acts on them";
        }
    }
}
