using System.Collections.Generic;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The saved tests of one group, each counted by what happened to its tolerance, FR-053.
    /// The clash step records one value per saved test at the point it knows the answer,
    /// and the TOLERANCE line reads the counts apart, so a test that was not found, would
    /// not copy, threw or already carried the value is never counted as set.
    /// </summary>
    public sealed class SavedTestTolerances
    {
        private readonly Dictionary<SavedTestTolerance, int> counts = new Dictionary<SavedTestTolerance, int>();

        /// <summary>Counts one saved test under what happened to it.</summary>
        public void Record(SavedTestTolerance what)
        {
            int already;
            counts.TryGetValue(what, out already);
            counts[what] = already + 1;
        }

        /// <summary>How many saved tests ended this way.</summary>
        public int Of(SavedTestTolerance what)
        {
            int count;
            return counts.TryGetValue(what, out count) ? count : 0;
        }

        /// <summary>Every saved test recorded, whichever way it ended.</summary>
        public int Total
        {
            get
            {
                int total = 0;

                foreach (int count in counts.Values)
                {
                    total += count;
                }

                return total;
            }
        }
    }
}
