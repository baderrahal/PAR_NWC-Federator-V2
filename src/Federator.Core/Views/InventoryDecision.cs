namespace Federator.Core.Views
{
    /// <summary>
    /// What the inventory decided for one item of the saved viewpoints tree, F114. Every keep
    /// comes before every remove, InventoryItem.Removes reads it so.
    /// </summary>
    public enum InventoryDecision
    {
        /// <summary>No mark and not a per clash viewpoint of an earlier run: a person's, or the NWCs'.</summary>
        KeepNotOurs = 0,

        /// <summary>Marked, then changed by a person or not provable, kept and named.</summary>
        KeepChangedByAPerson = 1,

        /// <summary>The tool's, but its test was not read this run, so nothing replaces it, S4.</summary>
        KeepTestNotRead = 2,

        /// <summary>The tool's, but what replaces it was not written or did not read back, S2.</summary>
        KeepReplacementFailed = 3,

        /// <summary>Written, marked and read back by this run.</summary>
        KeepWrittenThisRun = 4,

        /// <summary>A folder that is not the tool's, was empty before the run, or holds something kept.</summary>
        KeepFolder = 5,

        /// <summary>The clash step threw or the report is missing, so nothing is removed, S4.</summary>
        KeepClashStepNotSound = 6,

        /// <summary>A folder on its path shares its name with a folder beside it, so a parent path cannot find it again for certain.</summary>
        KeepPlaceNotUnique = 7,

        /// <summary>The tool's earlier view, replaced at its place by one written, marked and read back this run.</summary>
        RemoveReplaced = 8,

        /// <summary>The tool's earlier view, whose test was read and needs no view at its place this run.</summary>
        RemoveNoLongerNeeded = 9,

        /// <summary>A per clash viewpoint of an earlier run, once what replaces it is written and read back.</summary>
        RemoveLegacy = 10,

        /// <summary>Written this run and not marked or not read back, removed at once, S2.</summary>
        RemoveAtOnce = 11,

        /// <summary>A folder of the tool's that this run empties.</summary>
        RemoveFolder = 12
    }
}
