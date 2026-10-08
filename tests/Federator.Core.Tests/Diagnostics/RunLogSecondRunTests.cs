using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-049. The log lives as long as the window, so a second RESULT of it found every list the first one had
    /// read, and counted both runs: the groups, the files, the errors, the clashes, the penetration and priority
    /// lines, the collapsed lines and the timing block. A RESULT now closes the account, so the next one counts
    /// what was recorded since. The break in each test is the first run's own names, which must not appear in the
    /// second run's blocks.
    /// </summary>
    [TestFixture]
    public class RunLogSecondRunTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorSecondRun");
            CensusCost.TooLongSeconds = CensusCost.DefaultTooLongSeconds;
        }

        [TearDown]
        public void RemoveFolder()
        {
            CensusCost.TooLongSeconds = CensusCost.DefaultTooLongSeconds;
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

        /// <summary>The text from the timing block of the last RESULT to the end, which is that RESULT's blocks.</summary>
        private static string TheLastResultsBlocks(string text)
        {
            int at = text.LastIndexOf(TimingBlock.RunTitle, StringComparison.Ordinal);

            Assert.That(at, Is.GreaterThanOrEqualTo(0), "the log holds no timing block of a RESULT");
            return text.Substring(at);
        }

        private static int CountOf(string text, string what)
        {
            int count = 0;

            for (int at = text.IndexOf(what, StringComparison.Ordinal); at >= 0; at = text.IndexOf(what, at + what.Length, StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }

        [Test]
        public void TheSecondResultCountsTheSecondRunAlone()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                ARunThatRecordsEverything(log, "FIRST");
                log.WriteResultBlock();

                string first = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(first, Does.Contain("by priority    : "), "the first RESULT carries the line the second must not");
                Assert.That(first, Does.Contain("penetrations   : 4 clashes moved to Reviewed"));

                log.RunStarted(1);
                log.GroupStarted("SECONDA", new string[0]);
                log.GroupFinished("SECONDA", GroupOutcome.Done, 2.0, null, null);
                log.ClashesFound.Add("SECONDA", 2, 1);
                log.WriteFinished("NWD", AFile("SECOND.nwd"));
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastResultsBlocks(ReadWhileOpen(log));

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
                Assert.That(second, Does.Not.Contain("by priority    :"));
                Assert.That(log.PenetrationsWanted, Is.False);
                Assert.That(log.PriorityAcrossTheRun, Is.Null);
                Assert.That(log.StepRecords.Count, Is.EqualTo(0), "the first run's timed step is not in the second run's timing");
            }
        }

        /// <summary>
        /// Both boxes ticked and a priority file picked in both runs, which is the common case. The moved counts the
        /// engine adds to, and the tallies it adds groups to, must start the second run at nothing.
        /// </summary>
        [Test]
        public void WithTheBoxesTickedInBothRunsTheSecondResultHoldsTheSecondRunsNumbers()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                ARunThatRecordsEverything(log, "FIRST");
                log.WriteResultBlock();

                log.RunStarted(1);
                log.GroupStarted("SECONDA", new string[0]);
                log.GroupFinished("SECONDA", GroupOutcome.Done, 2.0, null, null);
                log.PenetrationsWanted = true;
                log.PenetrationsMoved += 1;
                log.PenetrationsAcrossTheRun.Add("SECONDA", 1, 3);
                log.ByDesignWanted = true;
                log.ByDesignMoved += 1;
                log.ByDesignAcrossTheRun.Add("SECONDA", 1, 2);

                PriorityTally priority = new PriorityTally();
                priority.Add(ClashPriority.B, 2);
                log.PriorityAcrossTheRun = priority;

                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Contain("penetrations   : 1 clash moved to Reviewed"), "1 and not 4 plus 1");
                Assert.That(second, Does.Contain("by design      : 1 clash moved to Reviewed"), "1 and not 2 plus 1");
                Assert.That(second, Does.Contain("SECONDA"));
                Assert.That(second, Does.Not.Contain("FIRST"), "the per group lines under the totals are the second run's alone");
                Assert.That(second, Does.Contain("by priority    : "));
                Assert.That(second, Does.Contain("B 2"));
                Assert.That(second, Does.Not.Contain("A 3"));
            }
        }

        [Test]
        public void TheFirstResultIsWhatItWasAndTheSecondRunIsSaidToBeTheSecond()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                ARunThatRecordsEverything(log, "FIRST");
                log.WriteResultBlock();

                string first = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(first, Does.Contain("groups done    : 1"));
                Assert.That(first, Does.Contain("groups failed  : 1"));
                Assert.That(first, Does.Contain("clashes found  : 10 across 1 group"));
                Assert.That(first, Does.Contain("FIRST failing step"));
                Assert.That(first, Does.Contain("lines collapsed"));
                Assert.That(ReadWhileOpen(log), Does.Not.Contain("this is run 2 in this window"));

                log.RunStarted(1);

                Assert.That(ReadWhileOpen(log), Does.Contain(
                    "RUN      this is run 2 in this window. Its RESULT counts what was recorded since the RESULT above"));
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
                log.WriteResultBlock();

                log.RunStarted(1);
                log.Failure("the same step", new InvalidOperationException("boom"), "went on");
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Contain("the same step"));
                Assert.That(second, Does.Contain("errors         : 1"));
                Assert.That(second, Does.Not.Contain("THIS HAPPENED"), "the first run's happening is not counted into the second's");
                Assert.That(CountOf(ReadWhileOpen(log), "FAILURE  the same step"), Is.EqualTo(2), "written in full in each run");
            }
        }

        /// <summary>
        /// The window writes a RESULT in the finally of every run, so a run that died still closes its account, and
        /// the next one does not carry its groups. Its end was never marked, so its time stays waiting for the person.
        /// </summary>
        [Test]
        public void ARunThatNeverFinishedIsClosedByItsResultAndItsTimeIsNotCalledAnEarlierRun()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.RunStarted(1);
                log.GroupFinished("FIRSTA", GroupOutcome.Failed, 1.0, "died", null);
                Thread.Sleep(20);
                log.WriteResultBlock();

                log.RunStarted(1);

                Assert.That(log.WhereTheTimeWent.EarlierRunsSeconds, Is.EqualTo(0.0), "a run with no marked end is not work that was measured");
                Assert.That(ReadWhileOpen(log), Does.Contain("the run before it never marked RUN finished, so its time is counted as waiting for the person"));

                log.GroupFinished("SECONDA", GroupOutcome.Done, 1.0, null, null);
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Not.Contain("FIRSTA"));
                Assert.That(second, Does.Contain("groups failed  : 0"));
                Assert.That(second, Does.Contain("Nothing failed."));
                Assert.That(second, Does.Not.Contain(RunClock.EarlierRuns));
            }
        }

        /// <summary>
        /// What was recorded before the first Run is the window's, as it always was, so the first run's RESULT still
        /// holds it. The break: clearing at every RunStarted drops it.
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

                string first = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(first, Does.Contain("a hand button before the run"));
                Assert.That(first, Does.Contain("early.nwf"));
            }
        }

        /// <summary>
        /// A failure of the preview, which runs before RunStarted in the same press, and one of a hand button between
        /// two presses, belong to the RESULT that follows them. Clearing at RunStarted lost both from every press but
        /// the first, so the second RESULT said Nothing failed beside a FAILURE block in the log.
        /// </summary>
        [Test]
        public void AFailureBeforeTheNextRunStartsIsInTheNextResult()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.RunStarted(1);
                log.RunFinished();
                log.WriteResultBlock();

                log.Failure("a hand button between the runs", new InvalidOperationException("between"), "went on");
                log.Failure("the preview of the second press", new InvalidOperationException("preview"), "went on");

                log.RunStarted(1);
                log.RunFinished();
                log.WriteResultBlock();

                string second = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Contain("errors         : 2"));
                Assert.That(second, Does.Contain("a hand button between the runs"));
                Assert.That(second, Does.Contain("the preview of the second press"));
                Assert.That(second, Does.Not.Contain("Nothing failed."));
            }
        }

        /// <summary>
        /// The open file run marks no run and still writes a RESULT. Run twice in one window, or run before a Run, its
        /// RESULT held the other's groups, clashes and files, and the Run's own RESULT said more groups than its
        /// RUN started line.
        /// </summary>
        [Test]
        public void ARunThatMarkedNoRunStillGetsItsOwnAccount()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.GroupFinished("OPENFILEA", GroupOutcome.Done, 1.0, null, null);
                log.ClashesFound.Add("OPENFILEA", 7, 2);
                log.WriteFinished("NWD", AFile("A.nwd"));
                log.WriteResultBlock();

                log.GroupFinished("OPENFILEB", GroupOutcome.Done, 1.0, null, null);
                log.ClashesFound.Add("OPENFILEB", 3, 1);
                log.WriteFinished("NWD", AFile("B.nwd"));
                log.WriteResultBlock();

                string second = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(second, Does.Not.Contain("OPENFILEA"));
                Assert.That(second, Does.Not.Contain("A.nwd"));
                Assert.That(second, Does.Contain("clashes found  : 3 across 1 group"));
                Assert.That(second, Does.Contain("groups done    : 1"));

                log.RunStarted(2);
                log.GroupFinished("RUNA", GroupOutcome.Done, 1.0, null, null);
                log.GroupFinished("RUNB", GroupOutcome.Done, 1.0, null, null);
                log.RunFinished();
                log.WriteResultBlock();

                string run = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(run, Does.Contain("groups done    : 2"), "the Run counts its own two groups and not the open file run's");
                Assert.That(run, Does.Not.Contain("OPENFILEB"));
            }
        }

        /// <summary>What a RESULT was written from is emptied after it, whatever the log was told to do next.</summary>
        [Test]
        public void AResultEmptiesWhatItWasWrittenFrom()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.GroupFinished("FIRSTA", GroupOutcome.Done, 1.0, null, null);
                log.WriteFinished("NWF", AFile("x.nwf"));
                log.Failure("a step", new InvalidOperationException("boom"), "went on");
                log.WriteResultBlock();

                Assert.That(log.CountOf(GroupOutcome.Done), Is.EqualTo(0));
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(0));
                Assert.That(log.Failures.Count, Is.EqualTo(0));
            }
        }

        // ---------- the census ----------

        /// <summary>
        /// A run starts with a whole census. Narrowing is decided by what counting cost in a group, and one run's
        /// cost is not another's, so a second run in the window must not inherit it and end a group DONE under a
        /// narrower watch than a window opened for it would have had.
        /// </summary>
        [Test]
        public void ASecondRunDoesNotInheritTheNarrowedCensusOfTheFirst()
        {
            CensusCost.TooLongSeconds = 0.000000001;

            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.CensusReader = delegate { return new DocumentCensus(5, 61, 1830, 0, 0); };

                log.RunStarted(2);
                log.GroupStarted("FIRSTA", new string[0]);

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                log.GroupFinished("FIRSTA", GroupOutcome.Done, 1.0, null, null);
                log.GroupStarted("FIRSTB", new string[0]);
                log.GroupFinished("FIRSTB", GroupOutcome.Done, 1.0, null, null);
                log.RunFinished();
                log.WriteResultBlock();

                Assert.That(CountOf(ReadWhileOpen(log), "CENSUS   narrowed."), Is.EqualTo(1), "the second group of the first run was narrowed");

                log.RunStarted(1);
                log.GroupStarted("SECONDA", new string[0]);

                Assert.That(CountOf(ReadWhileOpen(log), "CENSUS   narrowed."), Is.EqualTo(1), "the second run's first group starts whole");
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

            string lines = string.Join("\n", new List<string>(second.Lines()).ToArray());

            Assert.That(lines, Does.Contain(RunClock.EarlierRuns));
            Assert.That(lines, Does.Contain("the earlier runs were work but are not this run"));

            string alone = string.Join("\n", new List<string>(RunClock.From(100.0, 60.0, 80.0).Lines()).ToArray());

            Assert.That(alone, Does.Not.Contain(RunClock.EarlierRuns), "a window that ran once has no such row");
            Assert.That(alone, Does.Not.Contain("earlier runs"));
        }

        [Test]
        public void EveryRowOfTheClockHasItsColonInTheSameColumn()
        {
            int rows = 0;

            foreach (string line in RunClock.From(100.0, 60.0, 80.0, 30.0).Lines())
            {
                if (line.Contains(" seconds"))
                {
                    rows++;
                    Assert.That(line.IndexOf(": ", StringComparison.Ordinal), Is.EqualTo(24), line);
                }
            }

            Assert.That(rows, Is.EqualTo(5), "session, waiting, earlier runs, the run and the tail");
        }

        [Test]
        public void EarlierRunsCannotBeMoreThanCameBeforeTheRun()
        {
            RunClock odd = RunClock.From(100.0, 10.0, 50.0, 30.0);

            Assert.That(odd.EarlierRunsSeconds, Is.EqualTo(10.0), "no more than the time before this run started");
            Assert.That(odd.WaitingSeconds, Is.EqualTo(0.0));
        }

        [Test]
        public void ARunCountedToNowAlsoLeavesTheEarlierRunOutOfTheWaiting()
        {
            RunClock unfinished = RunClock.Unfinished(100.0, 60.0, 30.0);

            Assert.That(unfinished.WaitingSeconds, Is.EqualTo(30.0));
            Assert.That(
                string.Join("\n", new List<string>(unfinished.Lines()).ToArray()),
                Does.Contain(RunClock.EarlierRuns));
        }

        /// <summary>
        /// Three runs. The first took longer than the second, so a log that kept only the last earlier run, and not
        /// the sum, would read less at the third start than at the second.
        /// </summary>
        [Test]
        public void TheLogAddsEveryEarlierRunAndHandsTheSumToTheClock()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                log.RunStarted(1);
                Thread.Sleep(120);
                log.RunFinished();
                log.WriteResultBlock();

                Assert.That(log.WhereTheTimeWent.EarlierRunsSeconds, Is.EqualTo(0.0), "the first run has no earlier run");

                log.RunStarted(1);
                Thread.Sleep(5);
                log.RunFinished();

                double atTheSecond = log.WhereTheTimeWent.EarlierRunsSeconds;

                log.WriteResultBlock();
                log.RunStarted(1);
                log.RunFinished();

                RunClock third = log.WhereTheTimeWent;

                Assert.That(atTheSecond, Is.GreaterThan(0.0));
                Assert.That(third.EarlierRunsSeconds, Is.GreaterThan(atTheSecond), "the second run is added to the first, not put in its place");
                Assert.That(third.WaitingSeconds, Is.LessThan(third.StartedAt));

                log.WriteResultBlock();

                string text = TheLastResultsBlocks(ReadWhileOpen(log));

                Assert.That(text, Does.Contain(RunClock.EarlierRuns));
                Assert.That(text, Does.Contain("earlier runs   : "), "the footer names the fourth stretch beside the waiting");
            }
        }
    }
}
