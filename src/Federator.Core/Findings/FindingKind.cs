using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Federator.Core.Findings
{
    /// <summary>
    /// What the scan noticed. None of these block a run. They are information, and the
    /// decision stays with the person reading them.
    /// </summary>
    public enum FindingKind
    {
        /// <summary>A building code whose letter and digit pattern no other code shares.</summary>
        OddShape,

        /// <summary>
        /// Two building codes of the same length differing at exactly one position, where
        /// those two characters are easy to mistake for one another when read. An inserted
        /// or a missing character is deliberately not one, because that was most of the
        /// noise and is usually a different building.
        /// </summary>
        NearMatch,

        /// <summary>A group holding one discipline, so it has nothing to clash against.</summary>
        SingleDiscipline,

        /// <summary>A group missing disciplines that other groups in this run have.</summary>
        MissingDisciplines,

        /// <summary>
        /// The building code inside the Revit source name is not the code on the NWC that
        /// was published from it. Only known once a document is open, so this one comes
        /// out of the run rather than out of the scan.
        /// </summary>
        SourceMismatch,

        /// <summary>
        /// One Revit building code feeding more than one group, which reads as one
        /// building split in two by a naming error.
        /// </summary>
        SharedSourceBuilding
    }
}
