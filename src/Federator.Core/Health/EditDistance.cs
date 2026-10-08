using System;

namespace Federator.Core.Health
{
    /// <summary>
    /// How many single letter edits turn one word into the other. One routine for the EMPTY SETS judge
    /// and the workset disagreements, which each wrote it out in full, T1-N89, and each hands in its own
    /// cap.
    /// </summary>
    internal static class EditDistance
    {
        /// <summary>
        /// The number of insertions, deletions and changes between two words as written, with no case
        /// folding here. A pair whose lengths differ by more than the cap is given up on without work
        /// and reads int.MaxValue, so it is never counted as near.
        /// </summary>
        internal static int Between(string a, string b, int cap)
        {
            if (Math.Abs(a.Length - b.Length) > cap)
            {
                return int.MaxValue;
            }

            int[] previous = new int[b.Length + 1];
            int[] current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++)
            {
                previous[j] = j;
            }

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int best = Math.Min(current[j - 1] + 1, previous[j] + 1);
                    current[j] = Math.Min(best, previous[j - 1] + cost);
                }

                int[] swap = previous;
                previous = current;
                current = swap;
            }

            return previous[b.Length];
        }
    }
}
