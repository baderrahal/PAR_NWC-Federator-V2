using System.Collections.Generic;
using System.Collections.ObjectModel;
using Federator.Core.Diagnostics;

namespace Federator.Core.Health
{
    /// <summary>
    /// The models of one group that are NOT ON THE SAME SHARED COORDINATES as its reference
    /// model, Bader's answer to Q99 and Q100 on 2026-10-04, each said in one line naming its
    /// file, its shared site and its distance from the reference in X, Y and Z.
    ///
    /// A model is not on the same coordinates when it sits more than the far model setting
    /// from the reference model, measured as the straight line of its X, Y and Z offsets.
    ///
    /// IN A GROUP HOLDING ONE, ONLY THE CLASH IS SKIPPED. The NWF is built with every model,
    /// set and test and the NWD is published, no test is run, no viewpoint and no clash
    /// report is made, and the group ends PARTIAL with ClashSkippedReason. The rule is read on
    /// every run, so once the models are exported again the next run clashes the group with
    /// the tests already saved in its NWF. A setting switches the rule off.
    /// </summary>
    public sealed class OffCoordinates
    {
        /// <summary>The group's reason, in Bader's words.</summary>
        public const string ClashSkippedReason = "clash skipped, models not on the same shared coordinates";

        /// <summary>
        /// How a note's file name ends, after the name of the NWD it sits beside or of the
        /// report it stands in for in the Clash Report folder.
        /// </summary>
        public const string NoteEnding = " clash skipped.txt";

        /// <summary>The run's one list for the modellers, in the Clash Report folder.</summary>
        public const string ListName = "Models not on the same shared coordinates.txt";

        internal OffCoordinates(string reference, IList<string> models)
        {
            Reference = reference;
            Models = new ReadOnlyCollection<string>(models);
        }

        /// <summary>The reference model the distances are measured from, as the block names it, or null where no model could be placed.</summary>
        public string Reference { get; private set; }

        /// <summary>One line per model not on the same shared coordinates, the line the log, the note and the run's list all carry.</summary>
        public IList<string> Models { get; private set; }

        /// <summary>Whether the group holds any such model.</summary>
        public bool Any
        {
            get { return Models.Count > 0; }
        }

        /// <summary>
        /// The note that goes beside the NWD and into the Clash Report folder, so whoever
        /// looks for the report finds the reason. The same lines as the log, with a heading
        /// saying what was and was not done and a closing line saying what fixes it.
        /// </summary>
        public IList<string> Note(string building)
        {
            List<string> lines = new List<string>();
            lines.Add(Words.Or(building, "this group") + ": " + ClashSkippedReason);
            lines.Add("The NWF and the NWD of this group hold every model, set and clash test. No clash test was run and no"
                + " clash report or viewpoint was made, because these models are not on the same shared coordinates as "
                + (Reference == null ? "the rest of the group" : "the reference model " + Reference) + ":");

            foreach (string model in Models)
            {
                lines.Add("   " + model);
            }

            lines.Add("Once these models are exported again on the project's shared coordinates, the next run clashes the"
                + " group with the tests already saved in its NWF.");
            return lines;
        }
    }
}
