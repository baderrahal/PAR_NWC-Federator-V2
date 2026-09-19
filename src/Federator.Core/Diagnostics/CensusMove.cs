namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// What the census rule says about one count moving during one step, F73.
    ///
    /// TWO ANSWERS WERE NOT ENOUGH. The rule used to answer yes or no, and the log wrote a
    /// line for every no. On the first real run APPEND raised the saved viewpoints from 0
    /// to 20 in all seven groups, because an NWC exported from Revit carries the saved
    /// viewpoints that model was exported with and appending it brings them in. That is
    /// the ordinary way a federation is built, so calling it a fault put every group out
    /// of DONE and the run reported 0 done and 7 failed while 28 files had been written
    /// correctly.
    ///
    /// Saying nothing at all would be the other mistake. A count moving is still worth a
    /// line, because the next time it moves by a number nobody expects, the reader wants
    /// the ordinary case in front of them to compare it against. So there is a third
    /// answer: the move is expected, the line is written as an observation, and no reason
    /// goes on the group.
    /// </summary>
    public enum CensusMove
    {
        /// <summary>The step exists to change that count. Nothing is written.</summary>
        Allowed = 0,

        /// <summary>
        /// The step changes that count as a side effect of doing its own job. One line
        /// saying so, and the group can still be DONE.
        /// </summary>
        Noted = 1,

        /// <summary>
        /// Nothing about that step should have touched that count. One line saying so,
        /// and a reason on the group, which puts it out of DONE.
        /// </summary>
        Refused = 2
    }
}
