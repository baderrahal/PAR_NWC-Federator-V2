using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The one line that says a clash was moved to Reviewed, F72b.
    ///
    /// TWO RULES NOW MOVE A CLASH: the penetration rule and the by design list. Both write
    /// the same kind of line and one rule lives in one place, so the shape of it is here
    /// and each rule supplies only its own WHY. A second copy of this string in a second
    /// tally is how the two would start reading differently, and a person scanning a log
    /// for REVIEWED would then find one rule's moves and not the other's.
    ///
    /// The spacing is measured off the line the penetration rule has always written: one
    /// space after REVIEWED, two before "in", two after the test name.
    /// </summary>
    public static class ReviewedLine
    {
        /// <summary>The words that begin every one of them.</summary>
        public const string Prefix = "REVIEWED";

        /// <summary>
        /// One clash moved. It names the clash, the test and why, because a person
        /// auditing this a month later has to be able to tell whether the rule picked the
        /// right thing without opening the model.
        /// </summary>
        public static string For(string clashName, string testName, string why)
        {
            return Prefix + " " + Words(clashName) + "  in " + Words(testName)
                + "  " + Words(why);
        }

        private static string Words(string value)
        {
            return string.IsNullOrEmpty(value) ? "UNKNOWN" : value;
        }
    }
}
