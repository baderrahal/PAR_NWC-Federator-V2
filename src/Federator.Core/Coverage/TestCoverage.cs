namespace Federator.Core.Coverage
{
    /// <summary>
    /// One test of the picked file, or of the tests saved in the document where no file was
    /// picked, against what the run did with it, F127. Built by CoverageRule and by nothing
    /// else, so every row is read off the same record the same way.
    /// </summary>
    public sealed class TestCoverage
    {
        internal TestCoverage(
            int position,
            string name,
            string leftSet,
            string rightSet,
            int leftItems,
            int rightItems,
            TestPresence presence,
            bool ran,
            int clashesFound,
            CoverageReason reason,
            string detail)
        {
            Position = position;
            Name = name ?? string.Empty;
            LeftSet = leftSet ?? string.Empty;
            RightSet = rightSet ?? string.Empty;
            LeftItems = leftItems;
            RightItems = rightItems;
            Presence = presence;
            Ran = ran;
            ClashesFound = clashesFound;
            Reason = reason;
            Detail = detail ?? string.Empty;
        }

        /// <summary>Where the test sits in the picked file, or in the document, from one.</summary>
        public int Position { get; private set; }

        /// <summary>The test name exactly as the file carries it, never trimmed.</summary>
        public string Name { get; private set; }

        /// <summary>The left locator, or empty where the plan dropped the test before reading its sides.</summary>
        public string LeftSet { get; private set; }

        /// <summary>The right locator, or empty where the plan dropped the test before reading its sides.</summary>
        public string RightSet { get; private set; }

        /// <summary>Items the left set found in this group, or minus one where nobody counted them.</summary>
        public int LeftItems { get; private set; }

        /// <summary>Items the right set found in this group, or minus one where nobody counted them.</summary>
        public int RightItems { get; private set; }

        public TestPresence Presence { get; private set; }

        /// <summary>Whether the test ran in this run.</summary>
        public bool Ran { get; private set; }

        /// <summary>
        /// The clashes the runner counted when the test ran, every clash under a group
        /// counted, or minus one where it did not run. The count Clash Detective holds after
        /// the clash step is DocumentTestCount, read apart, because Compact can change it.
        /// </summary>
        public int ClashesFound { get; private set; }

        public CoverageReason Reason { get; private set; }

        /// <summary>The sets and codes behind the reason, or the runner's own words for it.</summary>
        public string Detail { get; private set; }

        public override string ToString()
        {
            return Position + "  " + Name + "  " + CoverageWords.For(Reason);
        }
    }
}
