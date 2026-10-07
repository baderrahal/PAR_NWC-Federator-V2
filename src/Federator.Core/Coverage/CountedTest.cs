namespace Federator.Core.Coverage
{
    /// <summary>
    /// One test of the picked file with the four numbers the count check set side by side,
    /// F127, and its verdict. A side with no test of that name carries minus one and says so
    /// through InDocument and InWorkbook, because a missing test is not a test holding none.
    /// </summary>
    public sealed class CountedTest
    {
        internal CountedTest(
            string name,
            CountVerdict verdict,
            bool inDocument,
            int documentTopLevel,
            int documentLeaves,
            bool inWorkbook,
            int workbookRows,
            int workbookClashes,
            string why)
        {
            Name = name ?? string.Empty;
            Verdict = verdict;
            InDocument = inDocument;
            DocumentTopLevel = documentTopLevel;
            DocumentLeaves = documentLeaves;
            InWorkbook = inWorkbook;
            WorkbookRows = workbookRows;
            WorkbookClashes = workbookClashes;
            Why = why ?? string.Empty;
        }

        public string Name { get; private set; }

        public CountVerdict Verdict { get; private set; }

        /// <summary>Whether Clash Detective holds one test of this name.</summary>
        public bool InDocument { get; private set; }

        /// <summary>Its results at the top level, or minus one.</summary>
        public int DocumentTopLevel { get; private set; }

        /// <summary>Every clash in it, or minus one.</summary>
        public int DocumentLeaves { get; private set; }

        /// <summary>Whether the client's sheet carries one test of this name.</summary>
        public bool InWorkbook { get; private set; }

        /// <summary>The clash rows under its block, or minus one.</summary>
        public int WorkbookRows { get; private set; }

        /// <summary>Its Clashes cell, or minus one.</summary>
        public int WorkbookClashes { get; private set; }

        /// <summary>Why it failed where that is known, or why it was not compared.</summary>
        public string Why { get; private set; }
    }
}
