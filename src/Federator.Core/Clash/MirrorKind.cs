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
        /// Two tests saved in the document, read with no XML: one whose name ends with the
        /// ending of MirrorSettings, and the one saved test without it whose question its
        /// sides ask as a mirror. Paired by the sides and never by the name, Bader's answers D
        /// to Q133 and A to Q136.
        /// </summary>
        Named
    }
}
