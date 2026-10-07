namespace Federator.Core.Views
{
    /// <summary>One item of the tree with the inventory's decision and its reason, F114.</summary>
    public sealed class InventoryItem
    {
        internal InventoryItem(ViewNode node, InventoryDecision decision, string why)
        {
            Node = node;
            Decision = decision;
            Why = why;
        }

        public ViewNode Node { get; private set; }

        public InventoryDecision Decision { get; private set; }

        /// <summary>The reason in plain words, for the log.</summary>
        public string Why { get; private set; }

        /// <summary>Whether the decision removes it.</summary>
        public bool Removes
        {
            get { return Decision >= InventoryDecision.RemoveReplaced; }
        }
    }
}
