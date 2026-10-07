namespace Federator.Core.Views
{
    /// <summary>What ToolViewMark.Judge found on one saved view or folder, and why.</summary>
    public sealed class MarkJudgement
    {
        internal MarkJudgement(ViewOwner owner, string why, ToolViewMark mark)
        {
            Owner = owner;
            Why = why;
            Mark = mark;
        }

        public ViewOwner Owner { get; private set; }

        /// <summary>The reason in plain words, for the log.</summary>
        public string Why { get; private set; }

        /// <summary>The mark read off it, or null where none reads.</summary>
        public ToolViewMark Mark { get; private set; }
    }
}
