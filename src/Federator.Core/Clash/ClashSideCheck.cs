using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Federator.Core.Clash
{
    /// <summary>The empty side rule, on its own so it can be tested without Navisworks.</summary>
    public static class ClashSideCheck
    {
        /// <summary>
        /// Most groups hold two or three disciplines, so most pairs have a side finding
        /// nothing in that model. A test with a side at zero cannot clash. It is SKIPPED,
        /// naming the empty side, and it is neither run nor passed, because a zero from a
        /// test that never ran reads exactly like a zero from a test that found nothing
        /// wrong.
        /// </summary>
        public static bool CanRun(
            PlannedClashTest test, int leftItems, int rightItems, out string reason)
        {
            if (test == null)
            {
                throw new ArgumentNullException("test");
            }

            bool leftEmpty = leftItems <= 0;
            bool rightEmpty = rightItems <= 0;

            if (!leftEmpty && !rightEmpty)
            {
                reason = null;
                return true;
            }

            if (leftEmpty && rightEmpty)
            {
                reason = "both sides find nothing in this model, left \"" + test.Left.Locator
                    + "\" and right \"" + test.Right.Locator + "\"";
                return false;
            }

            reason = leftEmpty
                ? "the left side \"" + test.Left.Locator + "\" finds nothing in this model, the right finds "
                    + rightItems
                : "the right side \"" + test.Right.Locator + "\" finds nothing in this model, the left finds "
                    + leftItems;

            return false;
        }
    }
}
