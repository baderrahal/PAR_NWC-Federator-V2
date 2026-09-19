using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;

namespace Federator.Core.Rerun
{
    /// <summary>
    /// Everything known about how one group went, with no Navisworks types in it, so the
    /// judgement can be tested without Navisworks.
    /// </summary>
    public sealed class GroupFacts
    {
        private readonly List<string> errors = new List<string>();

        public GroupFacts()
        {
            Decision = RerunDecision.Build;
            NwfPath = "the NWF path";
            NwdPath = "the NWD path";
        }

        /// <summary>Which of the four rerun cases this group turned out to be.</summary>
        public RerunDecision Decision { get; set; }

        /// <summary>
        /// This and NwdOnDisk below are set only after File.Exists has been checked, never
        /// off a call that reported success.
        /// </summary>
        public bool NwfOnDisk { get; set; }

        /// <summary>
        /// Whether this run was asked to republish the NWD. With the tick box off the NWD
        /// is not a requested step, so its absence is not a failure.
        /// </summary>
        public bool NwdRequested { get; set; }

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

        /// <summary>
        /// Whether this run was asked to put viewpoints in. F52. A run that was not asked
        /// cannot fail at it, which is the same rule the NWD follows: a step deliberately
        /// switched off is not a failure.
        /// </summary>
        public bool ViewpointsRequested { get; set; }

        /// <summary>
        /// How many planned viewpoints threw or produced nothing. Only meaningful when
        /// ViewpointsRequested is true.
        /// </summary>
        public int FailedViewpointCount { get; set; }

        /// <summary>
        /// Everything that threw for this group, in the order it threw. A list rather
        /// than one slot, because a group now has several steps that can each throw
        /// independently of the others. The model side can append cleanly and the clash
        /// step still fail, and one slot would have kept whichever wrote to it last and
        /// silently lost the other.
        /// </summary>
        internal IList<string> Errors
        {
            get { return errors; }
        }

        /// <summary>Records one thrown error. An empty message is ignored, never stored blank.</summary>
        public void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                errors.Add(error);
            }
        }

        public bool HasErrors
        {
            get { return errors.Count > 0; }
        }

        /// <summary>
        /// Every error on one line. All of them, because the RESULT block is what Bader
        /// sends back and a truncated list is a second run to find the rest.
        /// </summary>
        public string DescribeErrors()
        {
            return errors.Count == 0
                ? null
                : string.Join(", and ", new List<string>(errors).ToArray());
        }

        public string NwfPath { get; set; }

        /// <summary>
        /// Why the NWF read empty, F74, carried across so the FAILED reason says what to
        /// do rather than only what happened. Null on every group that did not stop this
        /// way. It comes off NwfComparison.Reason and is never typed anywhere else.
        /// </summary>
        public string NwfReadEmptyReason { get; set; }

        public string NwdPath { get; set; }
    }

    /// <summary>
    /// Decides how one group ended, judged against what was ASKED FOR rather than against
    /// what happens to be on disk.
    ///
    ///   DONE     everything requested for this group succeeded, a rebuilt group included
    ///   PARTIAL  something requested did not complete, or the group was CHANGED and left alone
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
            if (facts.HasErrors)
            {
                reason = facts.DescribeErrors();
                return GroupOutcome.Failed;
            }

            // F74. The NWF was there, it opened, and it reported no models. Nothing after
            // this point can be judged, because every later test reads a document that was
            // never loaded. It is FAILED and not PARTIAL: PARTIAL is a group that was left
            // alone on purpose, and this one was stopped because the tool could not tell
            // what it was looking at.
            if (facts.Decision == RerunDecision.Refused)
            {
                reason = Words.Or(facts.NwfReadEmptyReason,
                    "the NWF opened and reported no models at all, so the group was stopped");
                return GroupOutcome.Failed;
            }

            if (!facts.NwfOnDisk)
            {
                // Required whichever case this was. In Build it was just written, and in
                // Open or Changed it was opened, so either way it has to be there.
                reason = "the NWF is not on disk at " + Words.Or(facts.NwfPath, "an unknown path");
                return GroupOutcome.Failed;
            }

            // PARTIAL, judged before the NWD checks. A CHANGED group is left alone
            // entirely, and since F9 that includes its NWD, so a missing or stale NWD is
            // what was asked for and not a failure. Only a missing NWF, above, or an error
            // can make it FAILED.
            if (facts.Decision == RerunDecision.Changed)
            {
                reason = "the NWF points at a different set of files, so it was left alone";
                return GroupOutcome.Partial;
            }

            if (facts.Decision == RerunDecision.Build && facts.AppendedCount == 0)
            {
                reason = "nothing appended, so the group produced nothing";
                return GroupOutcome.Failed;
            }

            // A rebuilt group appended the scan into a cleared document, exactly as a
            // Build does, so nothing appended is the same failure. Everything after the
            // rebuild is judged the way an opened group is, so a rebuilt group whose
            // units, clash step, reports and NWD all went right is DONE.
            if (facts.Decision == RerunDecision.Rebuilt && facts.AppendedCount == 0)
            {
                reason = "nothing appended, so the rebuild produced nothing";
                return GroupOutcome.Failed;
            }

            // F52. A group whose viewpoints failed is not DONE. The viewpoints live in
            // the NWF and the NWF is the record, so a group that wrote every output and
            // silently lost a viewpoint has not done what it was asked. Judged on what was
            // ASKED FOR, so a run that wanted none cannot fail here.
            if (facts.ViewpointsRequested && facts.FailedViewpointCount > 0)
            {
                reason = facts.FailedViewpointCount
                    + (facts.FailedViewpointCount == 1 ? " viewpoint" : " viewpoints")
                    + " could not be put into the NWF";
                return GroupOutcome.Failed;
            }

            if (facts.NwdRequested && !facts.NwdOnDisk)
            {
                reason = "the NWD was requested and is not on disk at " + Words.Or(facts.NwdPath, "an unknown path");
                return GroupOutcome.Failed;
            }

            if (facts.NwdRequested && !facts.NwdPublishReportedSuccess)
            {
                // Something is at the path, but this run did not put it there. On a rerun
                // that is last week's NWD, and calling the group DONE off it would report
                // a stale file as a fresh one.
                reason = "the NWD publish did not report success, so the file at "
                    + Words.Or(facts.NwdPath, "an unknown path") + " is not from this run";
                return GroupOutcome.Failed;
            }

            // PARTIAL. Something requested did not complete.
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

    }
}
