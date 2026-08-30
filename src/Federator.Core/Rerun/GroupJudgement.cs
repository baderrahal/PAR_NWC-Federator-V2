using System;
using Federator.Core.Diagnostics;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// Everything known about how one group went, with no Navisworks types in it, so the
    /// judgement can be tested without Navisworks.
    /// </summary>
    public sealed class GroupFacts
    {
        public GroupFacts()
        {
            Decision = RerunDecision.Build;
            NwfPath = "the NWF path";
            NwdPath = "the NWD path";
        }

        /// <summary>Which of the three rerun cases this group turned out to be.</summary>
        public RerunDecision Decision { get; set; }

        /// <summary>Set only after File.Exists has been checked.</summary>
        public bool NwfOnDisk { get; set; }

        /// <summary>
        /// Whether this run was asked to republish the NWD. With the tick box off the NWD
        /// is not a requested step, so its absence is not a failure.
        /// </summary>
        public bool NwdRequested { get; set; }

        /// <summary>Set only after File.Exists has been checked.</summary>
        public bool NwdOnDisk { get; set; }

        /// <summary>
        /// Whether the publish call itself reported success. Outputs overwrite with no
        /// date suffix, so on a rerun last week's NWD is sitting at exactly this path.
        /// File.Exists alone would call that a success and report a group DONE off a file
        /// this run never produced. Only meaningful when NwdRequested is true.
        /// </summary>
        public bool NwdPublishReportedSuccess { get; set; }

        /// <summary>How many files went in. In the reuse cases, how many the NWF holds.</summary>
        public int AppendedCount { get; set; }

        /// <summary>How many files the group was asked to federate.</summary>
        public int FileCount { get; set; }

        /// <summary>How many of them would not append.</summary>
        public int FailedFileCount { get; set; }

        /// <summary>Non empty when something threw.</summary>
        public string Error { get; set; }

        public string NwfPath { get; set; }

        public string NwdPath { get; set; }
    }

    /// <summary>
    /// Decides how one group ended, judged against what was ASKED FOR rather than against
    /// what happens to be on disk.
    ///
    ///   DONE     everything requested for this group succeeded
    ///   PARTIAL  something requested did not complete, or the group was CHANGED
    ///   FAILED   something requested threw or produced nothing
    ///
    /// A step deliberately switched off is not a failure. Judging a group by whether an
    /// NWD existed, when republishing was switched off, reported all 22 groups of a clean
    /// run as FAILED.
    ///
    /// The outcome and the reason come out of one pass, so they can never disagree and a
    /// FAILED group can never reach the log without a reason.
    /// </summary>
    public static class GroupJudgement
    {
        public static GroupOutcome Judge(GroupFacts facts, out string reason)
        {
            if (facts == null)
            {
                throw new ArgumentNullException("facts");
            }

            // FAILED. Something requested threw, or produced nothing.
            if (!string.IsNullOrEmpty(facts.Error))
            {
                reason = facts.Error;
                return GroupOutcome.Failed;
            }

            if (!facts.NwfOnDisk)
            {
                // Required whichever case this was. In Build it was just written, and in
                // Open or Changed it was opened, so either way it has to be there.
                reason = "the NWF is not on disk at " + Or(facts.NwfPath, "an unknown path");
                return GroupOutcome.Failed;
            }

            if (facts.Decision == RerunDecision.Build && facts.AppendedCount == 0)
            {
                reason = "nothing appended, so the group produced nothing";
                return GroupOutcome.Failed;
            }

            if (facts.NwdRequested && !facts.NwdOnDisk)
            {
                reason = "the NWD was requested and is not on disk at " + Or(facts.NwdPath, "an unknown path");
                return GroupOutcome.Failed;
            }

            if (facts.NwdRequested && !facts.NwdPublishReportedSuccess)
            {
                // Something is at the path, but this run did not put it there. On a rerun
                // that is last week's NWD, and calling the group DONE off it would report
                // a stale file as a fresh one.
                reason = "the NWD publish did not report success, so the file at "
                    + Or(facts.NwdPath, "an unknown path") + " is not from this run";
                return GroupOutcome.Failed;
            }

            // PARTIAL. Something requested did not complete, or the group was left alone.
            if (facts.Decision == RerunDecision.Changed)
            {
                reason = "the NWF points at a different set of files, so it was left alone";
                return GroupOutcome.Partial;
            }

            if (facts.FailedFileCount > 0)
            {
                reason = facts.FailedFileCount + " of " + facts.FileCount + " files did not append";
                return GroupOutcome.Partial;
            }

            reason = null;
            return GroupOutcome.Done;
        }

        public static GroupOutcome Judge(GroupFacts facts)
        {
            string ignored;
            return Judge(facts, out ignored);
        }

        /// <summary>Why the group is not DONE, or null when it is.</summary>
        public static string ReasonFor(GroupFacts facts)
        {
            string reason;
            Judge(facts, out reason);
            return reason;
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
