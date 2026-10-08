using System;

namespace Federator.Core.Views
{
    /// <summary>Which sibling a recorded row resolves to, ResultSiblings.Pick, or none and how many carried the name.</summary>
    public sealed class SiblingPick
    {
        internal SiblingPick(int index, bool atRecorded, int carrying)
        {
            Index = index;
            AtRecorded = atRecorded;
            Carrying = carrying;
        }

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
    }
}
