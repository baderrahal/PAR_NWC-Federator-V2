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
    /// A model is not on the same coordinates when it names Internal as its shared site,
    /// which is Revit's own origin, when it names no site at all, Bader's answer to Q111,
    /// or when it sits more than the far model setting from the reference model, measured
    /// as the straight line of its X, Y and Z offsets. A site whose read threw is none of
    /// these and is not judged.
    ///
    /// SINCE F159 NOTHING IS SKIPPED. Bader's order of 2026-10-14: the option that skipped the
    /// clash of such a group is gone, the clash tests always run whatever the coordinates are,
    /// and the check only says what it found, in a NOTICE, Notice. The tool reports what it
    /// noticed and never acts on it.
    /// </summary>
    public sealed class OffCoordinates
    {
        /// <summary>What a NOTICE says of the results, in Bader's words of 2026-10-14.</summary>
        public const string ResultsMayBeWrong = "The clash results may be wrong for these models.";

        internal OffCoordinates(
            string reference, IList<string> models, int notJudged, int modelsRead, IList<string> onInternal = null,
            IList<string> noSite = null)
        {
            Reference = reference;
            Models = new ReadOnlyCollection<string>(models);
            OnInternal = new ReadOnlyCollection<string>(onInternal ?? new List<string>());
            NoSite = new ReadOnlyCollection<string>(noSite ?? new List<string>());
            NotJudged = notJudged;
            ModelsRead = modelsRead;
        }

        /// <summary>The reference model the distances are measured from, as the block names it, or null where no model could be placed.</summary>
        public string Reference { get; private set; }

        /// <summary>One line per model not on the same shared coordinates, the line the log, the note and the run's list all carry.</summary>
        public IList<string> Models { get; private set; }

        /// <summary>
        /// F171, Bader's answer to Q170. The models that sit on Revit's internal origin and not on a shared
        /// site, by name. Each is in Models too. This is a NOTICE and never a reason for FAILED.
        /// </summary>
        public IList<string> OnInternal { get; private set; }

        /// <summary>
        /// F176, Bader's answer to Q172. The models that name no shared site at all, by name, the same
        /// as a model on Internal. Each is in Models too. This is a NOTICE and never a reason for FAILED,
        /// and the clash always runs. A site whose read threw is not in it, since it is not a fact.
        /// </summary>
        public IList<string> NoSite { get; private set; }

        /// <summary>
        /// How many models this rule could not judge: not named above, and either their
        /// placement or their site UNKNOWN. A site that was not read could be Internal and a
        /// placement that was not read could be anywhere, so neither is a pass.
        /// </summary>
        internal int NotJudged { get; private set; }

        /// <summary>How many models were read at all. Nought means the rule judged nothing of the group.</summary>
        internal int ModelsRead { get; private set; }

        /// <summary>True where at least one model is off.</summary>
        public bool Any
        {
            get { return Models.Count > 0; }
        }

        /// <summary>
        /// The NOTICE for the log and for the run's list, F159: the group, which models, and that
        /// the clash results may be wrong for them. The first line is the sentence, and the models
        /// follow it one to a line, indented. Empty where no model is off. It says the clash was run
        /// and nothing was skipped, since a reader who knew the old rule would look for that.
        /// </summary>
        /// <summary>
        /// Which models sit on Revit's internal origin, said for the group result line, the run log and the
        /// list: the names, and that their clash results may be wrong. Empty where none does.
        /// </summary>
        public string InternalSaid()
        {
            if (OnInternal.Count == 0)
            {
                return string.Empty;
            }

            return Words.Counted(OnInternal.Count, "model", "models") + " sit"
                + (OnInternal.Count == 1 ? "s" : string.Empty)
                + " on Revit's internal origin and not on a shared site: " + string.Join(", ", OnInternal)
                + ". Their clash results may be wrong.";
        }

        /// <summary>
        /// F176. Which models name no shared site at all, said for the group result line, the run log and
        /// the list: the names, and that their clash results may be wrong. Empty where none does.
        /// </summary>
        public string NoSiteSaid()
        {
            if (NoSite.Count == 0)
            {
                return string.Empty;
            }

            return Words.Counted(NoSite.Count, "model", "models") + " name"
                + (NoSite.Count == 1 ? "s" : string.Empty)
                + " no shared site at all: " + string.Join(", ", NoSite)
                + ". Their clash results may be wrong.";
        }

        public IList<string> Notice(string building)
        {
            List<string> lines = new List<string>();

            if (!Any)
            {
                return lines;
            }

            lines.Add(Words.Or(building, "this group") + ": " + Words.Counted(Models.Count, "model", "models")
                + (Models.Count == 1 ? " is" : " are") + " not on the same shared coordinates"
                + (Reference == null
                    ? ", and no model could be placed to measure from"
                    : ", measured from the reference model " + Reference)
                + ". " + ResultsMayBeWrong + " The clash was run and nothing was skipped."
                + (OnInternal.Count == 0
                    ? string.Empty
                    : " " + InternalSaid() + " This is a notice and the group does not end FAILED for it.")
                + (NoSite.Count == 0
                    ? string.Empty
                    : " " + NoSiteSaid() + " This is a notice and the group does not end FAILED for it."));

            foreach (string model in Models)
            {
                lines.Add("   " + model);
            }

            return lines;
        }
    }
}
