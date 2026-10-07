namespace Federator.Core.Views
{
    /// <summary>Whose one saved view or folder is, as its mark proves it, F114, Q114 point 16.</summary>
    public enum ViewOwner
    {
        /// <summary>No mark, or a mark that does not read. Never touched.</summary>
        NotOurs = 0,

        /// <summary>
        /// Marked, but renamed, moved, turned, commented on or drawn on, or a part of the
        /// fingerprint could not be read to prove it unchanged. A person's from then on, kept and
        /// named, Q120 by its default A.
        /// </summary>
        ChangedByAPerson = 1,

        /// <summary>Marked, one comment, and every part of the fingerprint as the tool wrote it.</summary>
        Ours = 2
    }
}
