namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// How one group ended, with the reason when it did not end cleanly. The result block
    /// counts these and reads the reasons off the same list, so a failed count without a
    /// matching reason cannot happen.
    /// </summary>
    public sealed class GroupRecord
    {
        internal GroupRecord(string building, GroupOutcome outcome, double seconds, string reason)
            : this(building, outcome, seconds, reason, null)
        {
        }

        internal GroupRecord(
            string building, GroupOutcome outcome, double seconds, string reason, string runPath)
        {
            Building = building;
            Outcome = outcome;
            Seconds = seconds;
            Reason = reason;
            RunPath = runPath;
        }

        public string Building { get; private set; }

        /// <summary>
        /// Which of the two workflows the group took, in the words of
        /// Federator.Core.Rerun.RunPath, or null where the caller did not say.
        /// </summary>
        public string RunPath { get; private set; }

        public GroupOutcome Outcome { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>Never null or empty when Outcome is Failed.</summary>
        public string Reason { get; private set; }

        public override string ToString()
        {
            return Building + " " + Outcome + (string.IsNullOrEmpty(Reason) ? string.Empty : ": " + Reason);
        }
    }
}
