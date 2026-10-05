namespace Federator.Core.Coverage
{
    /// <summary>
    /// Whether a test of the picked file is in the document, as the clash runner recorded
    /// it, F127. Kept apart from the reason a test has no results, because a test already
    /// in the NWF can be skipped for the same reason as one created a minute ago, and a
    /// reader has to see both facts.
    /// </summary>
    public enum TestPresence
    {
        /// <summary>This run created it.</summary>
        CreatedThisRun,

        /// <summary>It was already in the document and was left as it was.</summary>
        AlreadyThere,

        /// <summary>It was not in the document and the run did not create it, F77.</summary>
        NotInDocument,

        /// <summary>
        /// The runner never looked: the plan dropped the test before the model, a set it
        /// names is not in the document, no test resolved a set, or the run never reached it.
        /// </summary>
        Unknown
    }
}
