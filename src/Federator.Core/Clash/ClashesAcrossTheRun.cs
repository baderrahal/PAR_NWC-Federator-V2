using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Clash
{
    /// <summary>
    /// How many clashes the whole run found, per group and added up, for the RESULT block.
    ///
    /// WHY THIS EXISTS. The RESULT block counted groups, run paths, penetrations moved,
    /// clashes by priority and files written, and it never once said how many clashes the
    /// run found. The number was in the log ten times, once per group, on a line called
    /// `clashes found` inside each CLASH block, and adding it up meant reading a file
    /// thousands of lines long. Two round reports in a row carried no run total for
    /// clashes because there was none to carry.
    ///
    /// UNLIKE THE PRIORITY AND PENETRATION LINES THIS ONE IS ALWAYS THERE. Those two are
    /// written only when their box was ticked, because a line reading zero on every run
    /// teaches people to skip the block. This one is the point of the run. A run that
    /// found nothing is the most interesting run there is, and it is exactly the run
    /// whose number must not be missing.
    /// </summary>
    public sealed class ClashesAcrossTheRun
    {
        private readonly List<string> names = new List<string>();
        private readonly List<int> found = new List<int>();
        private readonly List<int> testsWithClashes = new List<int>();

        /// <summary>One group's numbers, added as its CLASH block is written.</summary>
        public void Add(string group, int clashes, int testsThatFoundSomething)
        {
            names.Add(string.IsNullOrEmpty(group) ? "UNKNOWN" : group);
            found.Add(clashes < 0 ? 0 : clashes);
            testsWithClashes.Add(testsThatFoundSomething < 0 ? 0 : testsThatFoundSomething);
        }

        /// <summary>How many groups reported a clash count at all.</summary>
        public int Groups
        {
            get { return names.Count; }
        }

        /// <summary>Every clash the run found.</summary>
        public int Total
        {
            get
            {
                int total = 0;

                foreach (int one in found)
                {
                    total += one;
                }

                return total;
            }
        }

        /// <summary>Groups whose clash count was zero. Said apart, because none is not the same as few.</summary>
        public int GroupsThatFoundNothing
        {
            get
            {
                int none = 0;

                foreach (int one in found)
                {
                    if (one == 0)
                    {
                        none++;
                    }
                }

                return none;
            }
        }

        /// <summary>
        /// The RESULT lines. One total, then one line per group in the order they ran, so
        /// the run total and the per group numbers are read together and a total that does
        /// not add up is visible rather than trusted.
        /// </summary>
        public IList<string> ResultLines()
        {
            List<string> lines = new List<string>();

            if (names.Count == 0)
            {
                // Nothing reported a count, which is not the same as a run that found
                // nothing, so it says UNKNOWN rather than zero.
                lines.Add("clashes found  : UNKNOWN, no group reported a clash count");
                return lines;
            }

            lines.Add("clashes found  : " + Total.ToString(CultureInfo.InvariantCulture)
                + " across " + names.Count + (names.Count == 1 ? " group" : " groups")
                + (GroupsThatFoundNothing > 0
                    ? ", " + GroupsThatFoundNothing + " of which found none"
                    : string.Empty));

            for (int i = 0; i < names.Count; i++)
            {
                lines.Add("      " + names[i].PadRight(10) + " "
                    + found[i].ToString(CultureInfo.InvariantCulture).PadLeft(6)
                    + "  from " + testsWithClashes[i] + " test(s) that found something");
            }

            return lines;
        }
    }
}
