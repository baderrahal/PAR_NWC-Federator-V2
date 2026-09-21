using System;

namespace Federator.Core.Sets
{
    /// <summary>
    /// Whether a set already in the NWF is rebuilt from the picked file where the two
    /// have drifted apart, Q72 answered a on 2026-09-20.
    ///
    /// OFF BY DEFAULT, the shape F76 already uses for the tolerance. It changes what is
    /// in the NWF, which is the only record of what has been fixed, so a run has to be
    /// TOLD to do it rather than told not to.
    ///
    /// IT IS SAFE, AND THAT WAS MEASURED BEFORE IT WAS WRITTEN, 5v. Replacing a set
    /// through `DocumentSelectionSets.ReplaceWithCopy` keeps the clash test pointing at
    /// it, every one of that test's recorded results, every status a person set on them,
    /// and the set's own place in the tree, and all of it survives a save and a reopen
    /// off the disk. So this never has to refuse a set whose test holds results, which is
    /// what it would have had to do if the measurement had come back the other way.
    ///
    /// IT REBUILDS ONLY WHAT DRIFTED. Never all 61, and never a set the file does not
    /// name at all, which is a leftover and is reported instead, because removing one
    /// changes what every clash test pointing at it finds and that is a decision.
    /// </summary>
    public sealed class SetRebuildSettings
    {
        /// <summary>Off, because it changes the NWF and the NWF is the record.</summary>
        public const bool DefaultRebuildDriftedSets = false;

        /// <summary>The label on the tick box. Seven words, and the limit is eight.</summary>
        public const string TickLabel = "Rebuild sets that drifted from the file";

        /// <summary>
        /// The grey line under it. ELEVEN words, counted on 2026-09-21, where this comment
        /// said twelve. The limit is twelve and the test asserts at most twelve, so there is
        /// exactly one word of headroom. Any
        /// rewording has to be counted again. It says what it costs, which is the rule
        /// every other help line keeps, and what it costs is NOTHING, measured.
        /// </summary>
        public const string HelpLine =
            "Only sets whose question changed. Results and statuses are kept, measured";

        public SetRebuildSettings()
        {
            RebuildDriftedSets = DefaultRebuildDriftedSets;
        }

        /// <summary>Whether a drifted set is rebuilt from the picked file.</summary>
        public bool RebuildDriftedSets { get; set; }

        /// <summary>
        /// The one line the confirm screen carries, or null where the box is off. Null
        /// rather than a line reading none, because a sentence that appears on every run
        /// saying nothing will happen teaches people to skip the screen, which is the
        /// rule the tolerance line already keeps.
        /// </summary>
        public static string ConfirmLine(bool wanted)
        {
            if (!wanted)
            {
                return null;
            }

            return "Sets that drifted from the picked file are REBUILT in the NWF. The clash tests "
                + "pointing at them keep their results and their statuses, which was measured on "
                + "2026-09-20 before this was built. A set the file no longer names is brought up to date too: an unused one is removed, and one that clash tests point at is renamed into the corrected name rather than removed, Q74.";
        }
    }
}
