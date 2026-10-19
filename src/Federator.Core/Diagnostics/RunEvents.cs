namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The event column of the rows the machine readable log carries that a reader of a group's .tsv reads back, F236, the
    /// clash count check, so the writer and the reader name each one in one place.
    /// </summary>
    public static class RunEvents
    {
        /// <summary>A test that ran and found clashes, its name and its count in Clash Detective.</summary>
        public const string TestFoundClashes = "test found clashes";

        /// <summary>A test that ran and found none.</summary>
        public const string TestPassed = "test passed";

        /// <summary>One row a test of the coverage, its name and the words of its reason.</summary>
        public const string CoverageTest = "coverage test";

        /// <summary>One line of the VIEWS TREE block, a view's line naming its test and its open clashes.</summary>
        public const string ViewsTree = "views tree";

        /// <summary>A file this run wrote, its kind and its full path.</summary>
        public const string Written = "written";

        /// <summary>The kind of the clash workbook among the files written.</summary>
        public const string WorkbookKind = "XLSX";

        /// <summary>The kind of the HTML page among the files written.</summary>
        public const string PageKind = "HTML";

        /// <summary>The name of the clash workbook's one sheet, the name the writer gave it, so the reader finds it by its name.</summary>
        public const string WorkbookSheet = "workbook sheet";

        /// <summary>A mirror the mirror rule paired, its run name and the test it mirrors.</summary>
        public const string Mirror = "mirror";

        /// <summary>A mirror whose clashes the merge took into its kept test, its run name and the kept test.</summary>
        public const string MirrorMerged = "mirror merged";

        /// <summary>A kept test of a merge, its name and the clashes only its mirrors found, which the merge added to it.</summary>
        public const string AddedByMirrors = "added by mirrors";
    }
}
