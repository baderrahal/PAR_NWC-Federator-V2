using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// The by design rule, F72b. Whether one clash is a connection somebody has already
    /// agreed is how the building goes together.
    ///
    /// IT KNOWS NOTHING ABOUT ITEMS. The penetration rule reads both items, their
    /// categories and the service's size. This one reads two set names off the test and
    /// looks them up in a list. That is deliberate: reading every item once per clash is
    /// the walk shape that once built 1.7 million native handles in one group, and this
    /// rule needs none of it.
    ///
    /// THE PENETRATION RULE OWNS A CLASH THEY BOTH WANT. A 100 mm pipe through a wall
    /// whose two sets are also a pair in the file is moved once, there, and counted here
    /// under ThePenetrationRuleHasIt, so the two blocks add up to the number of clashes
    /// that moved rather than to twice it.
    ///
    /// THE SAME STATUS GUARD AS EVERY OTHER WRITE. Only New and Active move, through
    /// StatusesThisToolMayMoveFrom, and Reviewed is the only status this tool ever sets.
    /// </summary>
    public static class ByDesignRule
    {
        /// <summary>
        /// What to do with one clash. The pair that matched comes back too, so the log can
        /// say WHY in the words the file gave rather than in words this tool made up.
        ///
        /// THE ORDER OF THE ANSWERS IS THE ORDER OF THE QUESTIONS. Whether the two sides
        /// name sets at all, then whether those two are a pair, then whether somebody has
        /// already decided, then whether the other rule has it. Asking about the status
        /// first would report "somebody decided" against thousands of clashes this rule
        /// was never going to touch, and the block would read as though the guard was
        /// doing all the work.
        /// </summary>
        public static ByDesignVerdict Judge(
            ByDesignPairs pairs,
            string leftSet,
            string rightSet,
            ClashStatus status,
            bool thePenetrationRuleWantsIt,
            out ByDesignPair pair)
        {
            pair = null;

            if (string.IsNullOrEmpty(leftSet) || string.IsNullOrEmpty(rightSet))
            {
                return ByDesignVerdict.NoSetName;
            }

            if (pairs == null || !pairs.Picked)
            {
                return ByDesignVerdict.NotAPair;
            }

            pair = pairs.For(leftSet, rightSet);

            if (pair == null)
            {
                return ByDesignVerdict.NotAPair;
            }

            if (!StatusesThisToolMayMoveFrom.Allows(status))
            {
                return ByDesignVerdict.SomebodyDecided;
            }

            if (thePenetrationRuleWantsIt)
            {
                return ByDesignVerdict.ThePenetrationRuleHasIt;
            }

            return ByDesignVerdict.Reviewed;
        }

        /// <summary>
        /// The set name at the end of a locator, which is what the pairs file names.
        /// A locator is a path such as lcop_selection_set_tree/Mechanical/BLD-ME-Ducts and
        /// the pair file carries the leaf alone.
        ///
        /// NOTHING IS TRIMMED. Two set names in the reference file end in a space and the
        /// pairs file carries both "BLD-ME-Ducts&amp;Duct Fittings" and "BLD-DR-Pipes
        /// &amp; Pipe Fittings", which are two spellings and both are real. Trimming
        /// either would match a set nobody named.
        /// </summary>
        public static string SetNameIn(string locator)
        {
            if (string.IsNullOrEmpty(locator))
            {
                return string.Empty;
            }

            int at = locator.LastIndexOf('/');

            return at < 0 ? locator : locator.Substring(at + 1);
        }
    }
}
