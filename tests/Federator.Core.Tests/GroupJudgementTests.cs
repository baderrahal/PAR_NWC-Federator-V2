using System;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// A real run built 22 NWF and reported all 22 as FAILED. The rule required an NWD on
    /// disk, and the republish tick box was off, so a step that was deliberately switched
    /// off was being counted as a failure.
    ///
    ///   DONE     everything requested for this group succeeded
    ///   PARTIAL  something requested did not complete, or the group was CHANGED
    ///   FAILED   something requested threw or produced nothing
    /// </summary>
    [TestFixture]
    public class GroupJudgementTests
    {
        /// <summary>A group that federated four files cleanly with everything requested.</summary>
        private static GroupFacts Clean()
        {
            return new GroupFacts
            {
                Decision = RerunDecision.Build,
                NwfOnDisk = true,
                NwdRequested = true,
                NwdOnDisk = true,
                NwdPublishReportedSuccess = true,
                AppendedCount = 4,
                FileCount = 4,
                FailedFileCount = 0,
                NwfPath = @"C:\out\nwf\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf",
                NwdPath = @"C:\out\nwd\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd"
            };
        }

        // ---------- DONE ----------

        [Test]
        public void EverythingRequestedSucceededIsDone()
        {
            string reason;

            Assert.That(GroupJudgement.Judge(Clean(), out reason), Is.EqualTo(GroupOutcome.Done));
            Assert.That(reason, Is.Null, "a group that is DONE has nothing to explain");
        }

        // This is the run that reported 22 FAILED. Republishing was off, so no NWD was
        // written and none existed. That is a step switched off, not a failure.
        [Test]
        public void AGroupWithRepublishingOffAndNoNwdIsDone()
        {
            GroupFacts facts = Clean();
            facts.NwdRequested = false;
            facts.NwdOnDisk = false;

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Done),
                "a step deliberately switched off is not a failure");
            Assert.That(reason, Is.Null);
        }

        [Test]
        public void AReusedNwfWithNothingElseWrongIsDone()
        {
            GroupFacts facts = Clean();
            facts.Decision = RerunDecision.Open;

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
        }

        // In the reuse cases AppendedCount is how many the NWF holds, and nothing was
        // appended by this run. That must not read as producing nothing.
        [Test]
        public void AReusedNwfWithNoAppendsThisRunIsStillDone()
        {
            GroupFacts facts = Clean();
            facts.Decision = RerunDecision.Open;
            facts.AppendedCount = 0;

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done),
                "nothing is appended on a reuse, so zero appends is not producing nothing");
        }

        [Test]
        public void AnNwdAlreadyOnDiskWhileRepublishingIsOffIsStillDone()
        {
            GroupFacts facts = Clean();
            facts.NwdRequested = false;
            facts.NwdOnDisk = true;

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
        }

        // ---------- PARTIAL ----------

        [Test]
        public void AGroupLeftAloneBecauseItsFileListChangedIsPartial()
        {
            GroupFacts facts = Clean();
            facts.Decision = RerunDecision.Changed;

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Partial));
            Assert.That(reason, Does.Contain("different set of files"));
            Assert.That(reason, Does.Contain("left alone"));
        }

        [Test]
        public void AFileThatWouldNotAppendIsPartialAndTheReasonCounts()
        {
            GroupFacts facts = Clean();
            facts.AppendedCount = 3;
            facts.FailedFileCount = 1;

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Partial));
            Assert.That(reason, Is.EqualTo("1 of 4 files did not append"));
        }

        [Test]
        public void APartialGroupWithRepublishingOffIsStillPartialAndNotFailed()
        {
            GroupFacts facts = Clean();
            facts.NwdRequested = false;
            facts.NwdOnDisk = false;
            facts.AppendedCount = 3;
            facts.FailedFileCount = 1;

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Partial));
        }

        // ---------- FAILED ----------

        [Test]
        public void SomethingThatThrewIsFailedAndKeepsItsMessage()
        {
            GroupFacts facts = Clean();
            facts.AddError("IOException: the file is locked by another process");

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Is.EqualTo("IOException: the file is locked by another process"));
        }

        [Test]
        public void AnNwfThatIsNotOnDiskIsFailedWhicheverCaseItWas()
        {
            foreach (RerunDecision decision in
                new[] { RerunDecision.Build, RerunDecision.Open, RerunDecision.Changed })
            {
                GroupFacts facts = Clean();
                facts.Decision = decision;
                facts.NwfOnDisk = false;

                string reason;

                Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed),
                    decision.ToString());
                Assert.That(reason, Does.Contain("NWF is not on disk"));
                Assert.That(reason, Does.Contain(@"C:\out\nwf\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf"));
            }
        }

        [Test]
        public void ABuildThatAppendedNothingProducedNothingAndIsFailed()
        {
            GroupFacts facts = Clean();
            facts.AppendedCount = 0;
            facts.FailedFileCount = 4;

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Does.Contain("nothing appended"));
        }

        // The other half of the fix. Off is not a failure, but ON and missing still is.
        [Test]
        public void AnNwdThatWasRequestedAndIsMissingIsFailed()
        {
            GroupFacts facts = Clean();
            facts.NwdRequested = true;
            facts.NwdOnDisk = false;

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Does.Contain("NWD was requested"));
            Assert.That(reason, Does.Contain(@"C:\out\nwd\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd"));
        }

        // Outputs overwrite with no date suffix, so on a rerun last week's NWD is sitting
        // at exactly this path. File.Exists on its own cannot tell it from a fresh one.
        [Test]
        public void ARequestedNwdThatDidNotPublishIsFailedEvenWithLastWeeksFileAtThePath()
        {
            GroupFacts facts = Clean();
            facts.NwdRequested = true;
            facts.NwdOnDisk = true;                    // last week's file is there
            facts.NwdPublishReportedSuccess = false;   // but this run did not write it

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed),
                "a stale NWD was reported as a successful publish");
            Assert.That(reason, Does.Contain("did not report success"));
            Assert.That(reason, Does.Contain("not from this run"));
        }

        // With the tick box off nothing is published, so the publish result says nothing
        // and must not be consulted.
        [Test]
        public void ThePublishResultIsIgnoredWhenTheNwdWasNotRequested()
        {
            GroupFacts facts = Clean();
            facts.NwdRequested = false;
            facts.NwdPublishReportedSuccess = false;
            facts.NwdOnDisk = true;

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));

            facts.NwdOnDisk = false;
            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
        }

        [Test]
        public void ANwdThatPublishedButIsNotOnDiskIsStillFailed()
        {
            GroupFacts facts = Clean();
            facts.NwdPublishReportedSuccess = true;
            facts.NwdOnDisk = false;

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Does.Contain("is not on disk"));
        }

        // ---------- the outcome and the reason cannot disagree ----------

        [Test]
        public void EveryOutcomeThatIsNotDoneCarriesAReason()
        {
            GroupFacts[] all =
            {
                Failing(f => f.AddError("boom")),
                Failing(f => f.NwfOnDisk = false),
                Failing(f => { f.AppendedCount = 0; f.FailedFileCount = 4; }),
                Failing(f => f.NwdOnDisk = false),
                Failing(f => f.NwdPublishReportedSuccess = false),
                Failing(f => f.Decision = RerunDecision.Changed),
                Failing(f => { f.AppendedCount = 3; f.FailedFileCount = 1; })
            };

            foreach (GroupFacts facts in all)
            {
                string reason;
                GroupOutcome outcome = GroupJudgement.Judge(facts, out reason);

                Assert.That(outcome, Is.Not.EqualTo(GroupOutcome.Done));
                Assert.That(reason, Is.Not.Null.And.Not.Empty,
                    "an outcome that is not DONE reached the log with no reason");
            }
        }

        [Test]
        public void TheReasonAlwaysMatchesTheOutcomeItCameFrom()
        {
            GroupFacts facts = Clean();
            facts.Decision = RerunDecision.Changed;

            string fromJudge;
            GroupOutcome outcome = GroupJudgement.Judge(facts, out fromJudge);

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(outcome));
            Assert.That(GroupJudgement.ReasonFor(facts), Is.EqualTo(fromJudge));
        }

        [Test]
        public void NoFactsAtAllIsRefusedRatherThanJudged()
        {
            string reason;
            Assert.Throws<ArgumentNullException>(delegate { GroupJudgement.Judge(null, out reason); });
        }

        // ---------- the errors are a list, not one slot ----------

        // The model side can append cleanly and the clash step still throw. With one slot
        // whichever wrote last was kept and the other was silently lost.
        [Test]
        public void TwoStepsThatBothThrewBothReachTheReason()
        {
            GroupFacts facts = Clean();
            facts.AddError("building the sets threw InvalidOperationException: the folder was not found");
            facts.AddError("creating the clash tests threw NullReferenceException");

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(facts.Errors.Count, Is.EqualTo(2));
            Assert.That(reason, Does.Contain("building the sets threw"));
            Assert.That(reason, Does.Contain("creating the clash tests threw"),
                "the second error was lost, which is what one slot did");
        }

        [Test]
        public void OneErrorReadsExactlyAsItDidBefore()
        {
            GroupFacts facts = Clean();
            facts.AddError("IOException: the file is locked");

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Is.EqualTo("IOException: the file is locked"));
        }

        [Test]
        public void NoErrorsIsNotAFailure()
        {
            GroupFacts facts = Clean();

            Assert.That(facts.HasErrors, Is.False);
            Assert.That(facts.Errors.Count, Is.EqualTo(0));
            Assert.That(facts.DescribeErrors(), Is.Null);
            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
        }

        // A caller passing null or empty must not turn a clean group into a failure with a
        // blank reason, which is the shape that produced a bare count in the first place.
        [Test]
        public void ABlankErrorIsNotRecorded()
        {
            GroupFacts facts = Clean();
            facts.AddError(null);
            facts.AddError(string.Empty);

            Assert.That(facts.Errors.Count, Is.EqualTo(0));
            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
        }

        [Test]
        public void TheErrorsAreKeptInTheOrderTheyThrew()
        {
            GroupFacts facts = Clean();
            facts.AddError("first");
            facts.AddError("second");
            facts.AddError("third");

            Assert.That(facts.Errors[0], Is.EqualTo("first"));
            Assert.That(facts.Errors[2], Is.EqualTo("third"));
            Assert.That(facts.DescribeErrors(), Does.StartWith("first"));
            Assert.That(facts.DescribeErrors(), Does.EndWith("third"));
        }

        private static GroupFacts Failing(Action<GroupFacts> change)
        {
            GroupFacts facts = Clean();
            change(facts);
            return facts;
        }

        /// <summary>
        /// B11. The open file run is one group with Decision Open and nothing appended by
        /// this run, and it is judged by the same rule as an NWF the scan reused.
        /// </summary>
        [Test]
        public void AnOpenFileRunWithEverythingWrittenIsDone()
        {
            GroupFacts facts = new GroupFacts
            {
                Decision = RerunDecision.Open,
                NwfOnDisk = true,
                NwdRequested = true,
                NwdOnDisk = true,
                NwdPublishReportedSuccess = true,
                AppendedCount = 4,
                FileCount = 0,
                FailedFileCount = 0
            };

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Done));
            Assert.That(reason, Is.Null);
        }

        [Test]
        public void AnOpenFileRunWhoseNwdWasNotPublishedIsFailedAndSaysSo()
        {
            GroupFacts facts = new GroupFacts
            {
                Decision = RerunDecision.Open,
                NwfOnDisk = true,
                NwdRequested = true,
                NwdOnDisk = false,
                NwdPublishReportedSuccess = false,
                AppendedCount = 4,
                NwdPath = "D:\\Federations\\one.nwd"
            };

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Does.Contain("NWD was requested and is not on disk"));
        }
    }
}
