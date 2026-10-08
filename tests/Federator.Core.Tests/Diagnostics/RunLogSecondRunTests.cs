using System;
using System.IO;
using System.Text;
using System.Threading;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-049. The log lives as long as the window, so a second press of Run found every list the first run
    /// filled and the second RESULT counted both runs: the groups, the files, the errors, the clashes, the
    /// penetration and priority lines, the collapsed lines and the timing block. The break in each test is the
    /// first run's own names, which must not appear anywhere in the second run's blocks.
    /// </summary>
    [TestFixture]
    public class RunLogSecondRunTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorSecondRun");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static readonly DateTime Started = new DateTime(2026, 10, 8, 9, 0, 0);

        private static string ReadWhileOpen(RunLog log)
        {
            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private string AFile(string name)
        {
            string path = Path.Combine(folder, name);
            File.WriteAllText(path, "x");
            return path;
        }

        /// <summary>Everything a run records that a RESULT or the timing block reads, with names that say whose it is.</summary>
        private void ARunThatRecordsEverything(RunLog log, string tag)
        {
            log.RunStarted(2);
            log.GroupStarted(tag + "A", new string[0]);

            using (log.Step("SETS"))
            {
            }

            log.GroupFinished(tag + "A", GroupOutcome.Done, 1.5, null, null);
            log.GroupStarted(tag + "B", new string[0]);
            log.GroupFinished(tag + "B", GroupOutcome.Failed, 0.5, tag + " group reason", null);
            log.Failure(tag + " failing step", new InvalidOperationException(tag + " boom"), "went on");
            log.WriteFinished("NWF", AFile(tag + ".nwf"));
            log.ClashesFound.Add(tag + "A", 10, 3);
            log.PenetrationsWanted = true;
            log.PenetrationsMoved = 4;
            log.PenetrationsAcrossTheRun.Add(tag + "A", 4, 9);
            log.ByDesignWanted = true;
            log.ByDesignMoved = 2;
            log.ByDesignAcrossTheRun.Add(tag + "A", 2, 5);

            PriorityTally priority = new PriorityTally();
            priority.Add(ClashPriority.A, 3);
            log.PriorityAcrossTheRun = priority;

            for (int i = 0; i < 7; i++)
            {
                log.NumberedRepeat(tag + " key", tag + " repeat " + i, "h", "n", "1", "t");
            }

            log.RunFinished();
        }

        /// <summary>The text from the timing block of the last run to the end, which is that run's blocks.</summary>
        private static string TheLastRunsBlocks(string text)
        {
            int at = text.LastIndexOf(TimingBlock.RunTitle, StringComparison.Ordinal);

            Assert.That(at, Is.GreaterThanOrEqualTo(0), "the log holds no timing block of a run");
            return text.Substring(at);
        }

        [Test]
        public void TheSecondResultCountsTheSecondRunAlone()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                ARunThatRecordsEverything(log, "FIRST");
                log.WriteResultBlock();

                log.RunStarted(1);
                log.GroupStarted("SECONDA", new string[0]);
                log.GroupFinished("SECONDA", GroupOutcome.Done, 2.0, null, null);
                log.ClashesFound.Add("SECONDA", 2, 1);
                log.WriteFinished("NWD", AFile("SECOND.nwd"));
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastRunsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Not.Contain("FIRST"), "nothing the first run recorded reaches the second run's blocks");
                Assert.That(second, Does.Contain("groups done    : 1"));
                Assert.That(second, Does.Contain("groups partial : 0"));
                Assert.That(second, Does.Contain("groups failed  : 0"));
                Assert.That(second, Does.Contain("clashes found  : 2 across 1 group"));
                Assert.That(second, Does.Contain("files written  : 1, every size read back off the disk"));
                Assert.That(second, Does.Contain("SECOND.nwd"));
                Assert.That(second, Does.Contain("Nothing failed."));
                Assert.That(second, Does.Not.Contain("lines collapsed"));
                Assert.That(second, Does.Not.Contain("penetrations   :"), "the box was on in the first run and not in this one");
                Assert.That(second, Does.Not.Contain("by design      :"));
                Assert.That(second, Does.Not.Contain("PRIORITY"));
                Assert.That(log.CountOf(GroupOutcome.Done), Is.EqualTo(1));
                Assert.That(log.CountOf(GroupOutcome.Failed), Is.EqualTo(0));
                Assert.That(log.StepRecords.Count, Is.EqualTo(0), "the first run's timed step is not in the second run's timing");
                Assert.That(log.PenetrationsWanted, Is.False);
                Assert.That(log.PriorityAcrossTheRun, Is.Null);
            }
        }

        [Test]
        public void TheFirstResultIsWhatItWasAndTheSecondRunIsSaidToBeTheSecond()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                ARunThatRecordsEverything(log, "FIRST");
                log.WriteResultBlock();

                string first = TheLastRunsBlocks(ReadWhileOpen(log));

                Assert.That(first, Does.Contain("groups done    : 1"));
                Assert.That(first, Does.Contain("groups failed  : 1"));
                Assert.That(first, Does.Contain("clashes found  : 10 across 1 group"));
                Assert.That(first, Does.Contain("penetrations   : 4 clashes moved to Reviewed"));
                Assert.That(first, Does.Contain("FIRST failing step"));
                Assert.That(first, Does.Contain("lines collapsed"));
                Assert.That(ReadWhileOpen(log), Does.Not.Contain("in this window. Its RESULT counts this run alone"));

                log.RunStarted(1);

                Assert.That(ReadWhileOpen(log), Does.Contain("RUN      this is run 2 in this window. Its RESULT counts this run alone"));
            }
        }

        [Test]
        public void AFailureOfTheFirstRunThatHappensAgainIsWrittenInFullInTheSecond()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.RunStarted(1);
                log.Failure("the same step", new InvalidOperationException("boom"), "went on");
                log.RunFinished();

                log.RunStarted(1);
                log.Failure("the same step", new InvalidOperationException("boom"), "went on");
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastRunsBlocks(ReadWhileOpen(log));

                Assert.That(log.Failures.Count, Is.EqualTo(1));
                Assert.That(log.Failures[0].Times, Is.EqualTo(1), "the first run's happening is not counted into the second's");
                Assert.That(second, Does.Contain("the same step"));
                Assert.That(second, Does.Not.Contain("THIS HAPPENED"));
            }
        }

        [Test]
        public void ARunThatNeverFinishedIsForgottenByTheNextToo()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.RunStarted(1);
                log.GroupFinished("FIRSTA", GroupOutcome.Failed, 1.0, "died", null);

                log.RunStarted(1);
                log.GroupFinished("SECONDA", GroupOutcome.Done, 1.0, null, null);
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastRunsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Not.Contain("FIRSTA"));
                Assert.That(second, Does.Contain("groups failed  : 0"));
                Assert.That(second, Does.Contain("Nothing failed."));
            }
        }

        /// <summary>
        /// What was recorded before the first Run is the window's, as it always was, so the first run's RESULT still
        /// holds it. Only a second run starts from nothing. The break: clearing at every RunStarted drops it.
        /// </summary>
        [Test]
        public void WhatWasRecordedBeforeTheFirstRunIsStillInTheFirstResult()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.Failure("a hand button before the run", new InvalidOperationException("early"), "went on");
                log.WriteFinished("NWF", AFile("early.nwf"));

                log.RunStarted(1);
                log.GroupFinished("FIRSTA", GroupOutcome.Done, 1.0, null, null);
                log.RunFinished();
                log.WriteResultBlock();

                string first = TheLastRunsBlocks(ReadWhileOpen(log));

                Assert.That(first, Does.Contain("a hand button before the run"));
                Assert.That(first, Does.Contain("early.nwf"));
            }
        }

        // ---------- the clock ----------

        [Test]
        public void TheEarlierRunIsNotCalledWaitingForThePerson()
        {
            RunClock second = RunClock.From(100.0, 60.0, 80.0, 30.0);

            Assert.That(second.EarlierRunsSeconds, Is.EqualTo(30.0));
            Assert.That(second.WaitingSeconds, Is.EqualTo(30.0), "60 seconds before this run, 30 of them the first run");
            Assert.That(second.RunSeconds, Is.EqualTo(20.0));
            Assert.That(
                second.WaitingSeconds + second.EarlierRunsSeconds + second.RunSeconds + second.AfterSeconds,
                Is.EqualTo(second.SessionSeconds).Within(1e-9), "none of the four vanishes");

            string lines = string.Join("\n", new System.Collections.Generic.List<string>(second.Lines()).ToArray());

            Assert.That(lines, Does.Contain(RunClock.EarlierRuns));
            Assert.That(lines, Does.Contain("the earlier runs were work but are not this run"));

            string alone = string.Join("\n", new System.Collections.Generic.List<string>(RunClock.From(100.0, 60.0, 80.0).Lines()).ToArray());

            Assert.That(alone, Does.Not.Contain(RunClock.EarlierRuns), "a window that ran once has no such row");
            Assert.That(alone, Does.Not.Contain("earlier runs"));
        }

        [Test]
        public void ARunCountedToNowAlsoLeavesTheEarlierRunOutOfTheWaiting()
        {
            RunClock unfinished = RunClock.Unfinished(100.0, 60.0, 30.0);

            Assert.That(unfinished.WaitingSeconds, Is.EqualTo(30.0));
            Assert.That(
                string.Join("\n", new System.Collections.Generic.List<string>(unfinished.Lines()).ToArray()),
                Does.Contain(RunClock.EarlierRuns));
        }

        [Test]
        public void TheLogHandsTheEarlierRunsSecondsToTheClockOfTheNextRun()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.RunStarted(1);
                Thread.Sleep(30);
                log.RunFinished();

                Assert.That(log.WhereTheTimeWent.EarlierRunsSeconds, Is.EqualTo(0.0), "the first run has no earlier run");

                log.RunStarted(1);
                log.RunFinished();

                RunClock second = log.WhereTheTimeWent;

                Assert.That(second.EarlierRunsSeconds, Is.GreaterThan(0.0));
                Assert.That(second.WaitingSeconds, Is.LessThan(second.StartedAt), "the first run is not in the waiting");

                log.WriteResultBlock();

                Assert.That(TheLastRunsBlocks(ReadWhileOpen(log)), Does.Contain(RunClock.EarlierRuns));
            }
        }
    }
}
