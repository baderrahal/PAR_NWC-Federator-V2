using System;

namespace Federator.Core.Views
{
    /// <summary>Which sibling a recorded row resolves to, ResultSiblings.Pick, or none and how many carried the name.</summary>
    public sealed class SiblingPick
    {
        internal SiblingPick(int index, bool atRecorded, int carrying, bool noProof = false)
        {
            Index = index;
            AtRecorded = atRecorded;
            Carrying = carrying;
            NoProof = noProof;
        }

        /// <summary>
        /// True where a row with no name of its own was refused because its siblings could have
        /// moved since it was read, so the word it carries proves nothing, ResultSiblings.PickUnnamed.
        /// </summary>
        public bool NoProof { get; private set; }

        /// <summary>The index to take among the siblings, or minus one where none is taken.</summary>
        public int Index { get; private set; }

        public bool Found
        {
            get { return Index >= 0; }
        }

        /// <summary>Whether it sits where the harvest recorded it. False where the compact moved it.</summary>
        public bool AtRecorded { get; private set; }

        /// <summary>How many siblings carry the row's name, one where found, nought or more than one where refused.</summary>
        public int Carrying { get; private set; }
    }

    /// <summary>
    /// WHICH SIBLING A RECORDED ROW RESOLVES TO, F132 attempt 2 of the add-in half, the
    /// reviewer's blocking finding on it. The compact after the mirror merge removes every
    /// Resolved result before the views run, so a live result after a Resolved sibling sits
    /// one index left of where the harvest recorded it. The rule: the result at the recorded
    /// index where it carries the row's name, read once and nothing else read, else the one
    /// sibling carrying that name, compared whole and Ordinal, and none where no sibling or
    /// more than one does, said with the count, because a guess would view the wrong clash.
    /// The names are read through the reader handed in, so no Navisworks type reaches here.
    /// </summary>
    public static class ResultSiblings
    {
        public static SiblingPick Pick(string wanted, int recordedIndex, int count, Func<int, string> nameAt)
        {
            if (nameAt == null)
            {
                throw new ArgumentNullException("nameAt");
            }

            if (string.IsNullOrEmpty(wanted) || count <= 0)
            {
                return new SiblingPick(-1, false, 0);
            }

            if (recordedIndex >= 0 && recordedIndex < count
                && string.Equals(nameAt(recordedIndex) ?? string.Empty, wanted, StringComparison.Ordinal))
            {
                return new SiblingPick(recordedIndex, true, 1);
            }

            int found = -1;
            int carrying = 0;

            for (int i = 0; i < count; i++)
            {
                if (string.Equals(nameAt(i) ?? string.Empty, wanted, StringComparison.Ordinal))
                {
                    carrying++;
                    found = i;
                }
            }

            return carrying == 1 ? new SiblingPick(found, false, 1) : new SiblingPick(-1, false, carrying);
        }

        /// <summary>
        /// WHICH SIBLING A ROW WITH NO NAME OF ITS OWN RESOLVES TO, F149, the breaker's first point on
        /// the second reading. A result or a group Clash Detective gave no display name carries the
        /// word its kind is given, ClashRow.NameOf, as every other unnamed sibling does, so a name
        /// match proves nothing about it. It is taken at its recorded index where that sibling carries
        /// the word, and only where nothing can have moved it since it was read, the pictures, taken
        /// before the compact. Anywhere else, the views among them, it is refused with NoProof and no
        /// sibling is read, since the compact moves every live result after a Resolved one an index
        /// left and the sibling at the index would be the next unnamed one, framed and painted as
        /// this row's with nothing said. It is never looked for by name among the siblings.
        /// </summary>
        public static SiblingPick PickUnnamed(string wanted, int recordedIndex, int count, Func<int, string> nameAt, bool indexIsExact)
        {
            if (nameAt == null)
            {
                throw new ArgumentNullException("nameAt");
            }

            if (!indexIsExact)
            {
                return new SiblingPick(-1, false, 0, true);
            }

            if (!string.IsNullOrEmpty(wanted) && recordedIndex >= 0 && recordedIndex < count
                && string.Equals(nameAt(recordedIndex) ?? string.Empty, wanted, StringComparison.Ordinal))
            {
                return new SiblingPick(recordedIndex, true, 1);
            }

            return new SiblingPick(-1, false, 0);
        }
    }
}
