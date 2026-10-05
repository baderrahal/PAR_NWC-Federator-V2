namespace Federator.Core.Coverage
{
    /// <summary>
    /// The one reason each test of the picked file gets, F127, Bader's request 2 under Q112.
    /// Read off the clash runner's own record in the order the code applies it, so a test
    /// F77 kept out of the document keeps its side reason in a group whose clash the
    /// coordinates rule skipped. The words are CoverageWords.
    /// </summary>
    public enum CoverageReason
    {
        /// <summary>It ran and Clash Detective holds clashes for it.</summary>
        HasClashes,

        /// <summary>It ran and found no clashes.</summary>
        RanAndFoundNone,

        /// <summary>
        /// The test was not created: the plan dropped it before the model, an unknown test
        /// type, no locator, no tolerance, no name or units it cannot convert, or a set it
        /// names is not in the document. The words of ClashTestPlan.Describe go beside it.
        /// </summary>
        NotCreated,

        /// <summary>The test was not created because no test of the file resolves a set.</summary>
        NoTestResolvesASet,

        /// <summary>A side's set found no items and its discipline code is carried by no file of this group.</summary>
        DisciplineNotInGroup,

        /// <summary>A side's set found no items, and a file of this group carries its code.</summary>
        SideFoundNothing,

        /// <summary>
        /// A side's set found no items and no file of the run carries its code, FF, PL and
        /// DR on this project, so whether its discipline is in the group is UNKNOWN until
        /// the team map of F131.
        /// </summary>
        NoModelOfTheRunCarriesTheCode,

        /// <summary>A side's set found no items and its name carries no discipline code, BLD-Security Devices.</summary>
        SetNameCarriesNoCode,

        /// <summary>
        /// A side's set found no items and the disciplines of the models were not handed in,
        /// so whether its discipline is in the group is UNKNOWN.
        /// </summary>
        CodesNotRead,

        /// <summary>A side was not counted, so why the test has no results is UNKNOWN.</summary>
        SideNotCounted,

        /// <summary>It was not run because the group's clash was skipped by the coordinates rule, F112.</summary>
        CoordinatesRule,

        /// <summary>It was not run because the group holds one discipline.</summary>
        OneDiscipline,

        /// <summary>Creating or running it threw.</summary>
        Failed,

        /// <summary>The run was stopped before it reached this test.</summary>
        NotReached,

        /// <summary>The run left no record of this test that can be read as one.</summary>
        Unknown
    }
}
