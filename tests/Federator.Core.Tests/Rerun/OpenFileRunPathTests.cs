using System.Collections.Generic;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F129 attempt 2, the reviewer's blocking finding. A picked NWF that opened and read empty
    /// was FAILED while its GROUP finished line and RESULT's path tally called it a Weekly run,
    /// so the NWF read empty line of RESULT was never written. The label of a group the open
    /// file run's route ran is read off how its open went where this run opened it.
    /// </summary>
    [TestFixture]
    public class OpenFileRunPathTests
    {
        [Test]
        public void APickedNwfThatReadEmptyIsStoppedAsTheScannedRunLabelsTheSameRefusal()
        {
            Assert.That(RunPath.ForAnOpenFile(true, RerunDecision.Refused, false), Is.EqualTo(RunPath.Stopped));
            Assert.That(RunPath.ForAnOpenFile(true, RerunDecision.Refused, true), Is.EqualTo(RunPath.Stopped));
            Assert.That(RunPath.ForAnOpenFile(true, RerunDecision.Refused, false),
                Is.EqualTo(RunPath.Label(RerunDecision.Refused, false)));
        }

        [Test]
        public void APickedNwfThatRanIsAWeeklyRunWithOrWithoutTheXml()
        {
            Assert.That(RunPath.ForAnOpenFile(true, RerunDecision.Open, false), Is.EqualTo(RunPath.WeeklyRun));
            Assert.That(RunPath.ForAnOpenFile(true, RerunDecision.Open, true), Is.EqualTo(RunPath.WeeklyRunPlusXml));
        }

        /// <summary>
        /// One that would not open, or was refused before the open, never reached the decision a
        /// run sets, so it carries the one a group starts with, Build, which is what the scanned
        /// run labels a group whose Decide threw.
        /// </summary>
        [Test]
        public void APickedNwfThatWouldNotOpenCarriesWhatTheScannedRunGivesAGroupWhoseDecideThrew()
        {
            RerunDecision start = default(RerunDecision);

            Assert.That(start, Is.EqualTo(RerunDecision.Build));
            Assert.That(RunPath.ForAnOpenFile(true, start, false), Is.EqualTo(RunPath.Label(start, false)));
            Assert.That(RunPath.ForAnOpenFile(true, start, false), Is.Not.EqualTo(RunPath.WeeklyRun));
        }

        [Test]
        public void TheDocumentAlreadyOpenIsAlwaysAWeeklyRunAsBefore()
        {
            foreach (RerunDecision decision in new[] { RerunDecision.Build, RerunDecision.Open, RerunDecision.Refused })
            {
                Assert.That(RunPath.ForAnOpenFile(false, decision, false), Is.EqualTo(RunPath.WeeklyRun));
                Assert.That(RunPath.ForAnOpenFile(false, decision, true), Is.EqualTo(RunPath.WeeklyRunPlusXml));
            }
        }

        [Test]
        public void ResultCountsAPickedNwfThatReadEmptyUnderNwfReadEmpty()
        {
            List<string> labels = new List<string>
            {
                RunPath.ForAnOpenFile(true, RerunDecision.Open, false),
                RunPath.ForAnOpenFile(true, RerunDecision.Refused, false)
            };

            IList<string> lines = RunPath.ResultLines(labels);

            Assert.That(lines, Has.Some.EqualTo("weekly run     : 1"));
            Assert.That(lines, Has.Some.EqualTo("NWF read empty : 1"));
        }
    }
}
