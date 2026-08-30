namespace Federator.Core.Rerun
{
    /// <summary>
    /// Which of the two names a loaded model carries identifies the file the federation
    /// points at.
    ///
    /// A Navisworks Model has both. FileName is the NWC that was appended, which is what
    /// the scan holds and therefore the only one a comparison can use. SourceFileName is
    /// the container the NWC was published from, which on these projects is a Revit file
    /// in Autodesk Docs, for example
    ///
    ///     Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt
    ///
    /// That can never equal a scanned NWC path. Comparing on it reported CHANGED for 22
    /// of 22 groups on a run where nothing had changed, so no NWF was reused, no set was
    /// built and no test ran.
    ///
    /// The rule is here rather than in the add-in so it can be tested without Navisworks,
    /// which is the whole reason it went unnoticed.
    /// </summary>
    public static class ModelFileNames
    {
        /// <summary>
        /// The path to compare against the scan. FileName, and SourceFileName only when
        /// FileName is empty, so a model reporting one and not the other is still counted
        /// rather than silently dropped out of the comparison.
        /// </summary>
        public static string PathOf(string fileName, string sourceName)
        {
            return string.IsNullOrEmpty(fileName) ? sourceName : fileName;
        }

        /// <summary>
        /// True when the two names are different, which is when both belong in the log.
        /// Logging both on a disagreement is what made the wrong field findable.
        /// </summary>
        public static bool Disagree(string fileName, string sourceName)
        {
            return !string.Equals(
                fileName ?? string.Empty,
                sourceName ?? string.Empty,
                System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
