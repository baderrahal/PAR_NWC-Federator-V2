namespace Federator.Core.Clash
{
    /// <summary>Why two tests are a mirrored pair, F132.</summary>
    public enum MirrorKind
    {
        /// <summary>The same two sets swapped, Bader's Q114 point 4.</summary>
        Swapped,

        /// <summary>
        /// Two sets that differ by name carry the same rule list, so the two tests ask the
        /// same question, Bader's answer B to Q121.
        /// </summary>
        SameRules
    }
}
