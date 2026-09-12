namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// How one group ended. The three counts in the result block are these.
    ///
    /// The test is always what was ASKED FOR, not what happens to be on disk. A step
    /// deliberately switched off is not a failure. Republishing the NWD used to be a tick
    /// box, and with it off a group that federated cleanly was Done, not Failed. That
    /// mistake once reported all 22 groups of a clean run as FAILED. The box is fixed on
    /// now, and the rule stays for the steps that can still be switched off.
    /// </summary>
    public enum GroupOutcome
    {
        /// <summary>Everything requested for this group succeeded.</summary>
        Done,

        /// <summary>
        /// Something requested did not complete, or the group was CHANGED and left alone
        /// on purpose.
        /// </summary>
        Partial,

        /// <summary>Something requested threw, or produced nothing.</summary>
        Failed
    }
}
