using System.Collections.Generic;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// A Source File condition the sets of one discipline ask where another discipline also
    /// uses their category, Q103 answered by Bader on 2026-10-04: every set of that discipline
    /// whose category another discipline also uses gets the Source File condition its siblings
    /// already ask.
    ///
    /// WHY. A set asking a category and nothing else finds that category in every model of
    /// the group, so in a group holding no model of its discipline it finds another
    /// discipline's items and its clash tests count them, measured on the C06 run of set 03.
    ///
    /// WHAT THE RULE READS OFF THE MATRIX. The condition to copy, the sets it belongs beside,
    /// which are the sets in the folder of the ones already asking it, and the categories
    /// another folder's sets ask. WHAT IT CANNOT READ THERE is a category no other folder's
    /// set asks that another discipline's MODELS carry, so those are measured and handed in
    /// by the project's list of corrections beside the picked XML, never in the code, Q113
    /// answered B on 2026-10-04, MatrixCorrectionList.
    /// </summary>
    public sealed class SourceFileRule
    {
        public SourceFileRule(string asks, IList<string> measuredElsewhere)
        {
            Asks = asks ?? string.Empty;
            MeasuredElsewhere = measuredElsewhere ?? new List<string>();
        }

        /// <summary>What the Source File condition asks, contains being its test.</summary>
        public string Asks { get; private set; }

        /// <summary>
        /// The categories another discipline's models were measured carrying, where no other
        /// folder's set asks them, so the matrix alone cannot say another discipline uses them.
        /// </summary>
        public IList<string> MeasuredElsewhere { get; private set; }
    }
}
