using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F74. Five existing NWFs reported "0 unchanged, 4 added, 0 removed" and were thrown
    /// away, and the step that read them finished in 0.248 seconds against 2.3 to 4.8
    /// seconds for every open in the one earlier log on record. Opening returns before the
    /// models are in the document.
    ///
    /// No Navisworks type is in this rule. The add-in reads the count and the clock, this
    /// decides what to do about them, so both the run and the preview can read it and the
    /// two cannot disagree.
    /// </summary>
    [TestFixture]
    public class ModelLoadWaitTests
    {
        /// <summary>Hands the wait a list of readings a quarter second apart.</summary>
        private static ModelLoadWait After(ModelLoadWait wait, params int[] counts)
        {
            double seconds = 0;

            foreach (int count in counts)
            {
                seconds = seconds + wait.PauseMilliseconds / 1000.0;

                if (wait.Read(count, seconds) != LoadWaitVerdict.KeepWaiting)
                {
                    break;
                }
            }

            return wait;
        }

        [Test]
        public void ACountThatStopsMovingIsSettled()
        {
            ModelLoadWait wait = After(new ModelLoadWait(), 0, 1, 3, 4, 4, 4);

            Assert.That(wait.Verdict, Is.EqualTo(LoadWaitVerdict.Settled));
            Assert.That(wait.LastCount, Is.EqualTo(4));
            Assert.That(wait.GaveUp, Is.False);
        }

        [Test]
        public void ACountStillMovingIsNotSettledYet()
        {
            ModelLoadWait wait = After(new ModelLoadWait(), 0, 1, 2, 3);

            Assert.That(wait.Verdict, Is.EqualTo(LoadWaitVerdict.KeepWaiting));
            Assert.That(wait.Readings, Is.EqualTo(4));
        }

        /// <summary>
        /// The whole reason this exists. Three zeros in a row are what the run read, and
        /// they prove nothing at all, because a document that has not started filling and
        /// an empty federation give the same reading.
        /// </summary>
        [Test]
        public void ZeroNeverSettlesHoweverManyTimesItIsRead()
        {
            ModelLoadWait wait = new ModelLoadWait(3, 250, 30.0);

            for (int i = 1; i <= 20; i++)
            {
                Assert.That(wait.Read(0, i * 0.25), Is.EqualTo(LoadWaitVerdict.KeepWaiting),
                    "reading " + i);
            }
        }

        [Test]
        public void ZeroAtTheCeilingGivesUpAndSaysSo()
        {
            ModelLoadWait wait = new ModelLoadWait(3, 250, 1.0);

            Assert.That(wait.Read(0, 0.25), Is.EqualTo(LoadWaitVerdict.KeepWaiting));
            Assert.That(wait.Read(0, 0.50), Is.EqualTo(LoadWaitVerdict.KeepWaiting));
            Assert.That(wait.Read(0, 0.75), Is.EqualTo(LoadWaitVerdict.KeepWaiting));
            Assert.That(wait.Read(0, 1.00), Is.EqualTo(LoadWaitVerdict.Ceiling));
            Assert.That(wait.GaveUp, Is.True);
        }

        /// <summary>
        /// A count rising all the way to the ceiling is still a ceiling, not a settle. It
        /// means the document was filling slower than the wait allowed for, and the line
        /// has to say that rather than quietly reporting whatever number it stopped at.
        /// </summary>
        [Test]
        public void ACountStillRisingAtTheCeilingGivesUp()
        {
            ModelLoadWait wait = new ModelLoadWait(3, 250, 1.0);

            wait.Read(1, 0.25);
            wait.Read(2, 0.50);
            wait.Read(3, 0.75);

            Assert.That(wait.Read(4, 1.00), Is.EqualTo(LoadWaitVerdict.Ceiling));
            Assert.That(wait.LastCount, Is.EqualTo(4));
        }

        [Test]
        public void SettlingOnTheReadingThatCrossesTheCeilingIsStillASettle()
        {
            ModelLoadWait wait = new ModelLoadWait(3, 250, 1.0);

            wait.Read(4, 0.50);
            wait.Read(4, 0.75);

            Assert.That(wait.Read(4, 1.00), Is.EqualTo(LoadWaitVerdict.Settled));
            Assert.That(wait.GaveUp, Is.False);
        }

        [Test]
        public void AVerdictIsNeverRevisited()
        {
            ModelLoadWait wait = After(new ModelLoadWait(), 4, 4, 4);

            Assert.That(wait.Verdict, Is.EqualTo(LoadWaitVerdict.Settled));
            Assert.That(wait.Read(0, 99.0), Is.EqualTo(LoadWaitVerdict.Settled));
            Assert.That(wait.LastCount, Is.EqualTo(4));
        }

        [Test]
        public void TheSettledLineCarriesEveryNumberItWaitedOn()
        {
            ModelLoadWait wait = After(new ModelLoadWait(), 0, 4, 4, 4);
            string line = wait.Line();

            Assert.That(line, Does.StartWith(ModelLoadWait.Prefix));
            Assert.That(line, Does.Contain("4 models"));
            Assert.That(line, Does.Contain("1.000s"));
            Assert.That(line, Does.Contain("steady over 3 reads"));
            Assert.That(line, Does.Contain("4 readings"));
        }

        [Test]
        public void TheCeilingLineSaysItGaveUpAndWhatItSaw()
        {
            ModelLoadWait wait = new ModelLoadWait(3, 250, 1.0);
            wait.Read(0, 0.25);
            wait.Read(0, 1.00);

            string line = wait.Line();

            Assert.That(line, Does.Contain("still reporting 0 models"));
            Assert.That(line, Does.Contain("ceiling of 1.000s"));
            Assert.That(line, Does.Contain("gave up"));
        }

        [Test]
        public void AWaitNothingWasReadIntoSaysThatAndNotZeroModels()
        {
            string line = new ModelLoadWait().Line();

            Assert.That(line, Does.Contain("nothing was read"));
            Assert.That(line, Does.Not.Contain("0 models"));
        }

        [Test]
        public void TheDefaultsAreTheOnesTheCommentExplains()
        {
            ModelLoadWait wait = new ModelLoadWait();

            Assert.That(wait.SteadyReadings, Is.EqualTo(3));
            Assert.That(wait.PauseMilliseconds, Is.EqualTo(250));
            Assert.That(wait.CeilingSeconds, Is.EqualTo(30.0));
        }

        [Test]
        public void ASettingThatWouldMakeTheWaitMeaninglessIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ModelLoadWait(0, 250, 30.0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ModelLoadWait(3, -1, 30.0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ModelLoadWait(3, 250, 0.0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ModelLoadWait(3, 250, -5.0); });
        }

        [Test]
        public void AClockThatWentBackwardsIsNeverANegativeWait()
        {
            ModelLoadWait wait = new ModelLoadWait();
            wait.Read(1, -3.0);

            Assert.That(wait.Seconds, Is.EqualTo(0));
        }

        // ---------- the refusal the wait feeds ----------

        /// <summary>
        /// What the run did, and it is still what Compare says, because Compare cannot
        /// tell an empty NWF from one that has not loaded. This test pins the behaviour
        /// the caller has to stop relying on.
        /// </summary>
        [Test]
        public void ComparingAnEmptyListAgainstFourFilesStillReadsAsChanged()
        {
            NwfComparison comparison = NwfComparison.Compare(
                new List<string>(),
                new[] { "a.nwc", "b.nwc", "c.nwc", "d.nwc" });

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added.Count, Is.EqualTo(4));
        }

        [Test]
        public void AnNwfThatOpenedAndReadEmptyIsRefusedAndNotRebuilt()
        {
            string path = TestPaths.At("out", "nwf", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf");

            NwfComparison comparison = NwfComparison.ReadEmpty(
                path,
                new[] { "a.nwc", "b.nwc", "c.nwc", "d.nwc" },
                "after waiting 30.000s");

            Assert.That(comparison.Decision, Is.EqualTo(RerunDecision.Refused));
            Assert.That(comparison.Decision, Is.Not.EqualTo(RerunDecision.Changed));
            Assert.That(comparison.Added, Is.Empty, "nothing is added, because nothing is rebuilt");
            Assert.That(comparison.Removed, Is.Empty);
        }

        [Test]
        public void TheRefusalSaysWhatToDoAboutIt()
        {
            string path = TestPaths.At("out", "nwf", "a.nwf");

            NwfComparison comparison = NwfComparison.ReadEmpty(path, new[] { "a.nwc" }, "after waiting 30.000s");

            Assert.That(comparison.Reason, Does.Contain(path));
            Assert.That(comparison.Reason, Does.Contain("after waiting 30.000s"));
            Assert.That(comparison.Reason, Does.Contain("NOT rebuilt"));
            Assert.That(comparison.Reason, Does.Contain("Open it in Navisworks"));
        }

        [Test]
        public void TheRefusalWritesAStoppedBlockAndNoRebuildLine()
        {
            string path = TestPaths.At("out", "nwf", "a.nwf");

            IList<string> lines = NwfComparison.ReadEmpty(path, new[] { "a.nwc" }, null).Lines(path);

            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[0], Does.StartWith("STOPPED"));
            Assert.That(lines[0], Does.Contain(path));
            Assert.That(lines[1], Does.Contain(NwfComparison.WhatToDo));

            foreach (string line in lines)
            {
                Assert.That(line, Does.Not.Contain("rebuilt from the scan folder"));
            }
        }

        [Test]
        public void EveryComparisonButTheRefusalCarriesNoReason()
        {
            Assert.That(NwfComparison.NoNwfYet(new[] { "a.nwc" }).Reason, Is.Null);
            Assert.That(NwfComparison.Compare(new[] { "a.nwc" }, new[] { "a.nwc" }).Reason, Is.Null);
            Assert.That(NwfComparison.Compare(new[] { "a.nwc" }, new[] { "b.nwc" }).Reason, Is.Null);
        }

        [Test]
        public void AStoppedGroupIsFailedAndCarriesTheReasonThatSaysWhatToDo()
        {
            string path = TestPaths.At("out", "nwf", "a.nwf");
            NwfComparison comparison = NwfComparison.ReadEmpty(path, new[] { "a.nwc" }, "after waiting 30.000s");

            GroupFacts facts = new GroupFacts
            {
                Decision = comparison.Decision,
                NwfReadEmptyReason = comparison.Reason,
                NwfOnDisk = true,
                NwfPath = path
            };

            string reason;

            Assert.That(GroupJudgement.Judge(facts, out reason), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(reason, Does.Contain("Open it in Navisworks"));
        }

        /// <summary>
        /// A stopped group is COUNTED. A run that stops five of seven groups and shows no
        /// line for them reads as a run where five groups vanished.
        /// </summary>
        [Test]
        public void AStoppedGroupIsCountedInBothBlocks()
        {
            string[] labels = { RunPath.Stopped, RunPath.Stopped, RunPath.WeeklyRun };

            string result = string.Join("\n", new List<string>(RunPath.ResultLines(labels)).ToArray());
            string confirm = string.Join("\n", new List<string>(RunPath.ConfirmLines(labels)).ToArray());

            Assert.That(RunPath.Count(labels)[RunPath.Stopped], Is.EqualTo(2));
            Assert.That(result, Does.Contain("NWF read empty : 2"));
            Assert.That(confirm, Does.Contain(RunPath.Stopped + ": 2"));
            Assert.That(confirm, Does.Contain("stopped rather than rebuilt"));
        }

        [Test]
        public void WithNoStoppedGroupNeitherBlockMentionsIt()
        {
            string[] labels = { RunPath.WeeklyRun };

            string result = string.Join("\n", new List<string>(RunPath.ResultLines(labels)).ToArray());
            string confirm = string.Join("\n", new List<string>(RunPath.ConfirmLines(labels)).ToArray());

            Assert.That(result, Does.Not.Contain("NWF read empty"));
            Assert.That(confirm, Does.Not.Contain(RunPath.Stopped));
        }

        [Test]
        public void AStoppedGroupIsNeverReadAsRebuilt()
        {
            Assert.That(RunPath.Label(RerunDecision.Refused, false), Is.EqualTo(RunPath.Stopped));
            Assert.That(RunPath.Label(RerunDecision.Refused, true), Is.EqualTo(RunPath.Stopped));
            Assert.That(RunPath.AfterOpening(RerunDecision.Refused, false), Is.EqualTo(RunPath.Stopped));
            Assert.That(RunPath.All, Does.Contain(RunPath.Stopped));
        }
    }
}
