namespace Federator.Core.Clash
{
    /// <summary>Why two tests are a mirrored pair, F132.</summary>
    public enum MirrorKind
    {
        /// <summary>The same two sets swapped, Bader's Q114 point 4.</summary>
        Swapped,

        /// <summary>
        /// Two sets that differ by name ask the same whole question, so the two tests ask the
        /// same question, Bader's answer B to Q121.
        /// </summary>
        SameRules,

        /// <summary>
        /// Two tests saved in the document, read with no XML, one named as the other's mirror:
        /// its name is the other's with the ending, MirrorSettings. The add-in reads no set off
        /// a saved test, so the name is the one thing a run with no XML can pair them by.
        /// </summary>
        Named
    }
}
