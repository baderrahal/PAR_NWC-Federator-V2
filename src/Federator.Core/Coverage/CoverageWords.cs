namespace Federator.Core.Coverage
{
    /// <summary>
    /// The words for each coverage reason and each presence, F127, in Core so a test reads
    /// them and the block, the .tsv and the sheet cannot say one reason two ways. Bader's own
    /// words are kept where his request gave them: a side's set found no items, the
    /// discipline is not in the group, the test was not created, the clash was skipped by
    /// the coordinates rule, it ran and found no clashes. A reason that cannot say why reads
    /// UNKNOWN in so many words, and none reads as another.
    ///
    /// Presence is said apart, so a reason never claims a test was created. A test already
    /// in the NWF is skipped for the same reasons as one created a minute ago.
    /// </summary>
    public static class CoverageWords
    {
        public static string For(CoverageReason reason)
        {
            switch (reason)
            {
                case CoverageReason.HasClashes:
                    return "has clashes";
                case CoverageReason.RanAndFoundNone:
                    return "ran and found no clashes";
                case CoverageReason.NotCreated:
                    return "the test was not created";
                case CoverageReason.SavedTestNotRun:
                    return "already in the document and not run, a saved test this tool does not run";
                case CoverageReason.NoTestResolvesASet:
                    return "the test was not created, no test of the file resolves a set";
                case CoverageReason.DisciplineNotInGroup:
                    return "the discipline is not in the group";
                case CoverageReason.SideFoundNothing:
                    return "a side's set found no items in this group";
                case CoverageReason.NoModelOfTheRunCarriesTheCode:
                    return "a side's set found no items and no model of this run carries its discipline code, "
                        + "so whether its discipline is in the group is UNKNOWN";
                case CoverageReason.SetNameCarriesNoCode:
                    return "a side's set found no items and its name carries no discipline code, "
                        + "so whether its discipline is in the group is UNKNOWN";
                case CoverageReason.CodesNotRead:
                    return "a side's set found no items and the discipline codes of the models are not known, "
                        + "so whether its discipline is in the group is UNKNOWN";
                case CoverageReason.SideNotCounted:
                    return "a side was not counted, so why it has no results is UNKNOWN";
                case CoverageReason.CoordinatesRule:
                    return "not run, the clash was skipped by the coordinates rule";
                case CoverageReason.OneDiscipline:
                    return "not run, the group holds one discipline";
                case CoverageReason.Failed:
                    return "creating it, running it or reading its results threw";
                case CoverageReason.NotReached:
                    return "not reached, the run was stopped";
                default:
                    return "UNKNOWN, the run left no record of this test that can be read as one";
            }
        }

        public static string For(TestPresence presence)
        {
            switch (presence)
            {
                case TestPresence.CreatedThisRun:
                    return "created this run";
                case TestPresence.AlreadyThere:
                    return "already there";
                case TestPresence.NotInDocument:
                    return "not created";
                default:
                    return "UNKNOWN";
            }
        }
    }
}
