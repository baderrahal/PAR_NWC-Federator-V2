using System.Collections.Generic;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// A Source File condition the sets of one discipline ask where another discipline also
    /// uses their category, Q103 answered by Bader on 2026-10-04: every AR set whose category
    /// another discipline also uses gets Source File contains -AR-.
    ///
    /// WHY. In set 03 on C06, 1B06PK holds only ME and ST models, and 414 of its 1629 clashes
    /// named BLD-AR-Ramps, a set asking Category equals Ramps and nothing else, so it found
    /// the ramps of another discipline's model. BLD-AR-Floors, Stairs and Walls already ask
    /// Source File contains -AR-, because the Structure sets ask those categories too.
    ///
    /// WHAT THE RULE READS OFF THE MATRIX. The condition to copy, the sets it belongs beside,
    /// which are the sets in the folder of the ones already asking it, and the categories
    /// another folder's sets ask. WHAT IT CANNOT READ THERE is a category no other folder's
    /// set asks that another discipline's MODELS carry, Ramps among them, so those are
    /// measured and handed in, never typed into the code.
    /// </summary>
    public sealed class SourceFileRule
    {
        public SourceFileRule(string asks, IList<string> measuredElsewhere)
        {
            Asks = asks ?? string.Empty;
            MeasuredElsewhere = measuredElsewhere ?? new List<string>();
        }

        /// <summary>What the Source File condition asks, -AR- on this project, contains being its test.</summary>
        public string Asks { get; private set; }

        /// <summary>
        /// The categories another discipline's models were measured carrying, where no other
        /// folder's set asks them, so the matrix alone cannot say another discipline uses them.
        /// </summary>
        public IList<string> MeasuredElsewhere { get; private set; }
    }
}
