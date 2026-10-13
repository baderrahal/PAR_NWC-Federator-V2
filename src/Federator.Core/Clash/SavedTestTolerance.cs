namespace Federator.Core.Clash
{
    /// <summary>
    /// What happened to the tolerance of one clash test already saved in the document,
    /// FR-053. The TOLERANCE line counted every saved test under Set on before the test
    /// was even found, so each way a test can end is its own value and only Set is set.
    /// </summary>
    public enum SavedTestTolerance
    {
        /// <summary>No tolerance was chosen in the tool, so the test keeps the one it has.</summary>
        LeftAsTheDocumentHasIt = 0,

        /// <summary>The chosen tolerance was put on the test, through an edit from its own copy.</summary>
        Set = 1,

        /// <summary>The test already carried the chosen value, so nothing was edited.</summary>
        AlreadyAtTheValue = 2,

        /// <summary>The test could not be read back at its address, so nothing was edited.</summary>
        NotFound = 3,

        /// <summary>The test's copy was not a clash test, so nothing was edited.</summary>
        WouldNotCopy = 4,

        /// <summary>Setting it threw, and the test keeps the tolerance it had.</summary>
        Threw = 5
    }
}
