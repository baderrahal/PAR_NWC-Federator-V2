namespace Federator.Core.Coverage
{
    /// <summary>What the count check says about one test of the picked file, F127.</summary>
    public enum CountVerdict
    {
        /// <summary>The workbook's rows and Clashes cell equal what Clash Detective holds, exactly.</summary>
        Agree,

        /// <summary>A count differs. A FAILED line in COVERAGE and RESULT, and the group keeps its own result.</summary>
        Failed,

        /// <summary>
        /// Neither side holds the test: Clash Detective has none of that name and the workbook's block
        /// reads no row and Clashes nought, a test F77 did not create. Counted apart from Agree,
        /// because nothing was set beside Clash Detective, and a headline that said all agree over
        /// 1794 of 1830 such tests would read as a verification that never happened.
        /// </summary>
        HeldByNeither,

        /// <summary>A number on one side is UNKNOWN, a name is doubted, or a side was not read. Never Agree.</summary>
        NotCompared
    }
}
