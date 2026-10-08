namespace Federator.Core.Views
{
    /// <summary>
    /// What one removal the inventory asked for came to, F114 attempt 2 of the add-in pass,
    /// the breaker's B5: the item, the decision that asked for it, whether RemoveAt was made
    /// and returned, and why not where it was refused or threw. The VIEWS TREE block's
    /// removed counts are read off these and never off the inventory's decisions, one number
    /// from one list of results, so a removal refused or thrown is counted and named as not
    /// removed.
    /// </summary>
    public sealed class RemovalOutcome
    {
        public RemovalOutcome(ViewNode node, InventoryDecision decision, bool removed, string whyNot)
        {
            Node = node;
            Decision = decision;
            Removed = removed;
            WhyNot = whyNot;
        }

        /// <summary>
        /// The one rule for whether a RemoveAt that returned removed its item, the second
        /// reading's F4: its folder's children fell by exactly one. Anything else, the same
        /// count, more than one fewer, or the folder not found again, is not removed, with what
        /// happened, and is never counted as removed.
        /// </summary>
        public static RemovalOutcome Counted(ViewNode node, InventoryDecision decision, int countBefore, int countAfter)
        {
            string why = CountWords(countBefore, countAfter);
            return new RemovalOutcome(node, decision, why == null, why);
        }

        /// <summary>Null where the folder fell by exactly one child, else what happened, in the words the FAILED lines carry.</summary>
        public static string CountWords(int countBefore, int countAfter)
        {
            if (countAfter < 0)
            {
                return "RemoveAt returned and its folder was not found again, so whether it fell by one is UNKNOWN";
            }

            return countAfter == countBefore - 1
                ? null
                : "RemoveAt returned and its folder went from " + countBefore + " to " + countAfter + " children, not one fewer";
        }

        /// <summary>The item the inventory asked to remove.</summary>
        public ViewNode Node { get; private set; }

        /// <summary>The inventory's decision that asked for it.</summary>
        public InventoryDecision Decision { get; private set; }

        /// <summary>Whether the removal was made.</summary>
        public bool Removed { get; private set; }

        /// <summary>Why it was not made, or null where it was.</summary>
        public string WhyNot { get; private set; }
    }
}
