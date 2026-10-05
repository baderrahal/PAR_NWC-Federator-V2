namespace Federator.Core.Coverage
{
    /// <summary>
    /// What one side that found no items says about the group, F127. The values are in the
    /// order of strength the coverage reads them in when both sides are empty: a code no
    /// file of the group carries is the strongest, because it is the gap in the matrix the
    /// coverage exists to show, and a side nobody counted is the weakest, because it says
    /// nothing.
    /// </summary>
    internal enum SideDiscipline
    {
        /// <summary>A file of another group of the run carries the code and no file of this group does.</summary>
        NotInGroup,

        /// <summary>A file of this group carries the code. The set found no items in this group.</summary>
        InGroup,

        /// <summary>No file of the run carries the code.</summary>
        NoModelOfTheRun,

        /// <summary>The set's name carries no discipline code.</summary>
        NoCode,

        /// <summary>The codes of the group's files or of the run's were not handed in.</summary>
        CodesNotRead,

        /// <summary>The side's count is minus one, UNKNOWN and never zero.</summary>
        NotCounted
    }
}
