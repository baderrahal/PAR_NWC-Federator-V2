using System;
using System.Collections.Generic;
using System.Globalization;

namespace Federator.Core.Health
{
    /// <summary>
    /// Every group of the run holding a model not on the same shared coordinates, for the
    /// RESULT block and for the one list the run writes for the modellers, Bader's answer to
    /// Q99 and Q100: "the RESULT block lists these groups, and the run writes one short list
    /// Bader can forward to the modellers, one file for the run".
    ///
    /// A group is added whether the rule that skips the clash was on or off, because the
    /// models are off their coordinates either way and the modellers want to know, and both
    /// the RESULT block and the list say which way the rule was.
    /// </summary>
    public sealed class OffCoordinatesAcrossTheRun
    {
        private readonly List<string> groups = new List<string>();
        private readonly List<OffCoordinates> found = new List<OffCoordinates>();

        /// <summary>
        /// Whether the rule that skips the clash was on for this run, or null until the engine
        /// says, and then the RESULT block says nothing rather than guess which way it was.
        /// </summary>
        public bool? SkipsTheClash { get; set; }

        /// <summary>How many groups held a model not on the same shared coordinates.</summary>
        public int Groups
        {
            get { return groups.Count; }
        }

        /// <summary>One group, added as its ALIGNMENT block is written. A group with no such model is not added.</summary>
        public void Add(string building, OffCoordinates off)
        {
            if (off == null || !off.Any)
            {
                return;
            }

            groups.Add(string.IsNullOrEmpty(building) ? "UNKNOWN" : building);
            found.Add(off);
        }

        /// <summary>The RESULT lines: whether the rule was on, how many groups, and one line per group.</summary>
        public IList<string> ResultLines()
        {
            List<string> lines = new List<string>();

            if (!SkipsTheClash.HasValue)
            {
                return lines;
            }

            if (SkipsTheClash.Value)
            {
                lines.Add("clash skipped  : " + Counted(groups.Count) + ", models not on the same shared coordinates"
                    + (groups.Count == 0
                        ? string.Empty
                        : ". Each wrote its NWF and its NWD with its clash tests in them and ran none"));
            }
            else
            {
                lines.Add("clash skipped  : none, the rule that skips it was off for this run"
                    + (groups.Count == 0
                        ? string.Empty
                        : ", so " + Counted(groups.Count) + (groups.Count == 1 ? " was" : " were")
                            + " clashed with a model not on the same shared coordinates"));
            }

            for (int i = 0; i < groups.Count; i++)
            {
                lines.Add("      " + groups[i].PadRight(10) + " " + found[i].Models.Count
                    + " model(s) not on the same shared coordinates");
            }

            return lines;
        }

        /// <summary>
        /// The one list for the run, which Bader forwards to the modellers: each group, the
        /// reference model it was measured from, and the same line per model the log carries.
        /// Written every run, so a list from an earlier run never stands as this run's.
        /// </summary>
        public IList<string> ForModellers(DateTime runStarted)
        {
            List<string> lines = new List<string>();
            lines.Add("Models not on the same shared coordinates, from the run started "
                + runStarted.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));

            if (groups.Count == 0)
            {
                lines.Add("No model in this run was found off its group's shared coordinates, so no group is listed.");
                return lines;
            }

            lines.Add(SkipsTheClash == false
                ? "The rule that skips the clash was off for this run, so each group below was clashed, and its clashes"
                    + " with the other disciplines cannot be trusted until these models are exported again on the"
                    + " project's shared coordinates."
                : "The clash of each group below was skipped. Its NWF and its NWD were written with every model, set and"
                    + " clash test in them, and the next run clashes it once these models are exported again on the"
                    + " project's shared coordinates.");

            for (int i = 0; i < groups.Count; i++)
            {
                lines.Add(string.Empty);
                lines.Add(groups[i] + (found[i].Reference == null
                    ? ", where no model could be placed to measure from"
                    : ", measured from the reference model " + found[i].Reference));

                foreach (string model in found[i].Models)
                {
                    lines.Add("   " + model);
                }
            }

            return lines;
        }

        private static string Counted(int count)
        {
            return count + (count == 1 ? " group" : " groups");
        }
    }
}
