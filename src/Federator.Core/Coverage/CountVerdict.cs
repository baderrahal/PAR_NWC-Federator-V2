namespace Federator.Core.Coverage
{
    /// <summary>What the count check says about one test of the picked file, F127.</summary>
    public enum CountVerdict
    {
        /// <summary>The workbook's rows and Clashes cell equal what Clash Detective holds, exactly.</summary>
        Agree,

        /// <summary>A count differs. A FAILED line in COVERAGE and RESULT, and the group keeps its own result.</summary>
        Failed,

        /// <summary>A number on one side is UNKNOWN, a name is doubted, or a side was not read. Never Agree.</summary>
        NotCompared
    }
}
