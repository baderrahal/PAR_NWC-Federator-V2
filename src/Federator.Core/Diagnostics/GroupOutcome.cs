namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// How one group ended. The three counts in the result block are these.
    ///
    /// The test is always what was ASKED FOR, not what happens to be on disk. A step
    /// deliberately switched off is not a failure. Republishing the NWD is a tick box, and
    /// with it off a group that federated cleanly is Done, not Failed. That mistake once
    /// reported all 22 groups of a clean run as FAILED.
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

    /// <summary>
    /// How one group ended, with the reason when it did not end cleanly. The result block
    /// counts these and reads the reasons off the same list, so a failed count without a
    /// matching reason cannot happen.
    /// </summary>
    public sealed class GroupRecord
    {
        internal GroupRecord(string building, GroupOutcome outcome, double seconds, string reason)
        {
            Building = building;
            Outcome = outcome;
            Seconds = seconds;
            Reason = reason;
        }

        public string Building { get; private set; }

        public GroupOutcome Outcome { get; private set; }

        public double Seconds { get; private set; }

        /// <summary>Never null or empty when Outcome is Failed.</summary>
        public string Reason { get; private set; }

        public override string ToString()
        {
            return Building + " " + Outcome + (string.IsNullOrEmpty(Reason) ? string.Empty : ": " + Reason);
        }
    }

    /// <summary>A file the log verified on disk, with the size it read back.</summary>
    public sealed class WrittenFile
    {
        internal WrittenFile(string kind, string path, long sizeInBytes)
        {
            Kind = kind;
            Path = path;
            SizeInBytes = sizeInBytes;
        }

        /// <summary>NWF, NWD, or whatever the caller named it.</summary>
        public string Kind { get; private set; }

        public string Path { get; private set; }

        /// <summary>Read back off the disk. Never a size that was assumed.</summary>
        public long SizeInBytes { get; internal set; }
    }

    /// <summary>One failure, kept whole so the result block can repeat it in full.</summary>
    public sealed class LoggedFailure
    {
        internal LoggedFailure(string what, string detail, string whatNext)
        {
            What = what;
            Detail = detail;
            WhatNext = whatNext;
            Times = 1;
        }

        public string What { get; private set; }

        /// <summary>Type name, message, inner exception and stack trace, already laid out.</summary>
        public string Detail { get; private set; }

        /// <summary>What the tool did after this failure, kept going or stopped.</summary>
        public string WhatNext { get; private set; }

        /// <summary>
        /// How many times this exact failure happened. One run threw the same exception
        /// tens of thousands of times and wrote a 17.8 MB log that was almost entirely one
        /// stack trace, so the trace is kept once and the rest are counted.
        /// </summary>
        public int Times { get; private set; }

        internal void AgainOnce()
        {
            Times++;
        }
    }
}
