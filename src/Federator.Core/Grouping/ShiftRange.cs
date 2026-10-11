using System;
using System.Collections.Generic;

namespace Federator.Core.Grouping
{
    /// <summary>
    /// Which rows of the group list one click on a Run box sets, and to what, F130, Bader's
    /// request 5 under Q112. A Shift click gives every row from the anchor, the row clicked
    /// last, to the row clicked now the state the anchor was left in, both ends included, in
    /// the order the grid shows them, so a sorted or filtered grid ranges over what the
    /// person sees. Anything else is a plain click and sets only the row clicked, to the state
    /// the click left it in: no Shift, no anchor, an anchor the grid no longer shows, the
    /// anchor itself, or a row the grid does not show.
    ///
    /// The rows are handed in and back as they are, so the rule knows nothing of the window.
    /// Whether a row can take a tick at all is the row's own rule and not this one.
    /// </summary>
    public static class ShiftRange
    {
        /// <summary>The grey line under the group list, by the tick box rule.</summary>
        public const string HelpLine = "Shift click a Run box to tick or untick the rows between";

        public static RowsToSet<T> Of<T>(
            IList<T> shown, T anchor, bool anchorState, T clicked, bool clickedState, bool shift) where T : class
        {
            if (shown == null)
            {
                throw new ArgumentNullException("shown");
            }

            if (clicked == null)
            {
                throw new ArgumentNullException("clicked");
            }

            int to = At(shown, clicked);
            int from = anchor == null ? -1 : At(shown, anchor);

            if (!shift || from < 0 || to < 0 || from == to)
            {
                return new RowsToSet<T>(new List<T> { clicked }, clickedState);
            }

            List<T> rows = new List<T>();

            for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
            {
                rows.Add(shown[i]);
            }

            return new RowsToSet<T>(rows, anchorState);
        }

        /// <summary>
        /// Where a row sits by reference, because two rows equal by value are still two rows and
        /// the groups made again are new rows even under the same building.
        /// </summary>
        private static int At<T>(IList<T> shown, T row) where T : class
        {
            for (int i = 0; i < shown.Count; i++)
            {
                if (ReferenceEquals(shown[i], row))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
