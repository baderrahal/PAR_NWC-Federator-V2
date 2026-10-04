using System;
using System.Collections.Generic;
using System.Text;
using Federator.Core.Sets;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// The conditions of ONE set block as the exchange file writes them, each kept as the
    /// text it was, so MatrixCorrections can change a set's conditions and leave every other
    /// byte of the file as it was, F116.
    ///
    /// WHY TEXT AND NOT A DOCUMENT. The corrected matrix in the exchange folder is proved
    /// byte for byte against what the rule makes from the sample, and a document written
    /// back by an XML library changes the quotes, the whitespace and the declaration of every
    /// line of a 29,000 line file to correct a few dozen of them.
    /// </summary>
    internal sealed class SetConditionsText
    {
        private readonly string opening;
        private readonly string closing;

        private SetConditionsText(string opening, IList<WrittenCondition> conditions, string closing)
        {
            this.opening = opening;
            this.closing = closing;
            Conditions = new List<WrittenCondition>(conditions);
        }

        /// <summary>The conditions in the order the file writes them.</summary>
        internal IList<WrittenCondition> Conditions { get; private set; }

        /// <summary>
        /// The conditions of that set block, or null where it holds no conditions element or
        /// one whose conditions do not close, which is a block a correction cannot read.
        /// </summary>
        internal static SetConditionsText Read(string block)
        {
            int open = block.IndexOf(MatrixCorrections.ConditionsOpen, StringComparison.Ordinal);

            if (open < 0)
            {
                return null;
            }

            open += MatrixCorrections.ConditionsOpen.Length;
            int close = block.IndexOf(MatrixCorrections.ConditionsClose, open, StringComparison.Ordinal);

            if (close < 0)
            {
                return null;
            }

            List<WrittenCondition> conditions = new List<WrittenCondition>();
            int at = open;

            while (true)
            {
                int starts = block.IndexOf(MatrixCorrections.ConditionOpens, at, StringComparison.Ordinal);

                if (starts < 0 || starts > close)
                {
                    break;
                }

                int ends = block.IndexOf(MatrixCorrections.ConditionCloses, starts, StringComparison.Ordinal);

                if (ends < 0 || ends > close)
                {
                    return null;
                }

                ends += MatrixCorrections.ConditionCloses.Length;
                conditions.Add(WrittenCondition.Read(block.Substring(at, starts - at), block.Substring(starts, ends - starts)));
                at = ends;
            }

            return new SetConditionsText(block.Substring(0, open), conditions, block.Substring(at));
        }

        /// <summary>The same block holding other conditions, everything around them as it was.</summary>
        internal SetConditionsText With(IList<WrittenCondition> conditions)
        {
            return new SetConditionsText(opening, conditions, closing);
        }

        /// <summary>The block as text again.</summary>
        internal string Write()
        {
            StringBuilder written = new StringBuilder(opening);

            foreach (WrittenCondition condition in Conditions)
            {
                written.Append(condition.Lead).Append(condition.Element);
            }

            return written.Append(closing).ToString();
        }

        /// <summary>The conditions split into their groups by the plan's own rule, F78.</summary>
        internal IList<IList<WrittenCondition>> Groups()
        {
            return PlannedSet.GroupsOf(Conditions, condition => PlannedCondition.StartsAGroupWith(condition.Flags));
        }
    }
}
