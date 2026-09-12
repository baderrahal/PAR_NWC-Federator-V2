using System;

namespace Federator.Core.Clash
{
    /// <summary>
    /// Stops a run that is failing everything for one reason.
    ///
    /// A real run spent 8 hours 52 minutes across 24 groups and produced nothing, because
    /// every single test threw the same exception and the run carried on regardless. Once
    /// enough tests in a row have failed for the same reason, nothing later in the run is
    /// going to behave differently, so the whole run stops rather than the group.
    ///
    /// Stopping the group would not have helped. Every one of the 24 groups failed the
    /// same way, so a per group stop would still have burned the same nine hours.
    /// </summary>
    public sealed class RepeatedFailureGuard
    {
        /// <summary>How many failures in a row it takes to stop the run.</summary>
        public const int DefaultThreshold = 50;

        private readonly int threshold;
        private string firstReason;
        private int consecutive;

        public RepeatedFailureGuard()
            : this(DefaultThreshold)
        {
        }

        public RepeatedFailureGuard(int threshold)
        {
            if (threshold < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "threshold", "The guard needs at least one failure before it can stop a run.");
            }

            this.threshold = threshold;
        }

        /// <summary>How many failures in a row, all carrying the same reason.</summary>
        public int Consecutive
        {
            get { return consecutive; }
        }

        public bool ShouldStopTheRun
        {
            get { return consecutive >= threshold; }
        }

        /// <summary>Null until the guard has fired.</summary>
        public string Reason
        {
            get
            {
                return ShouldStopTheRun
                    ? "the first " + consecutive
                        + " tests all failed for the same reason, so the rest of the run was not attempted. "
                        + firstReason
                    : null;
            }
        }

        /// <summary>
        /// One test that did what it was meant to. A single success means the run is not
        /// uniformly broken, so the count starts again.
        /// </summary>
        public void RecordSuccess()
        {
            consecutive = 0;
            firstReason = null;
        }

        /// <summary>
        /// One test that threw. A different reason from the last one also starts the count
        /// again, because a run failing for several reasons is not the case this guards.
        /// </summary>
        public void RecordFailure(string reason)
        {
            string tidied = reason == null ? string.Empty : reason;

            if (consecutive > 0 && string.Equals(firstReason, tidied, StringComparison.Ordinal))
            {
                consecutive++;
                return;
            }

            firstReason = tidied;
            consecutive = 1;
        }

        /// <summary>
        /// A test that was skipped without being attempted, for example because a side
        /// finds nothing in this model. That is the ordinary case and says nothing about
        /// whether the run is broken, so it is not counted either way.
        /// </summary>
        public void RecordNotAttempted()
        {
        }
    }
}
