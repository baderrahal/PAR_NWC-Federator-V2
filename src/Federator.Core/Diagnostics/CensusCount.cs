namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// The five things a census counts in the open document.
    ///
    /// WHY THESE FIVE. Each one is something the NWF carries and something a run can
    /// destroy without saying so. The models are what the federation IS. The sets are
    /// what every clash test's sides point at. The tests and the results are the only
    /// record of what has been fixed, and there is no second copy of them anywhere. The
    /// viewpoints are F52's, and they are counted even while nothing creates one, because
    /// a count that only starts once the feature works would never see the run that broke
    /// it on the way there.
    /// </summary>
    public enum CensusCount
    {
        Models = 0,
        Sets = 1,
        Tests = 2,
        Results = 3,
        Viewpoints = 4
    }
}
