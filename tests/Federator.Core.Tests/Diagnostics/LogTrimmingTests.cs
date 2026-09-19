using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Clash;
using Federator.Core.Diagnostics;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F80, F81 and F82. The run of 2026-09-19 wrote a log of 2.1 MB whose headline time
    /// was eight minutes longer than the run, and buried in it were 38 of 61 sets that
    /// found nothing in every group, which nothing anywhere added up.
    /// </summary>
    [TestFixture]
    public class LogTrimmingTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorLogTrimming");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 9, 19, 9, 0, 0));
        }

        private static string ReadWhileOpen(string path)
        {
            using (FileStream stream = new FileStream(
                       path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static int Occurrences(string text, string what)
        {
            int count = 0;
            int at = text.IndexOf(what, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(what, at + 1, StringComparison.Ordinal);
            }

            return count;
        }

        // ---------- F80, the run against the session ----------

        /// <summary>
        /// The headline read 1424 seconds and the run took 951. The other 473 were a
        /// person reading the group table and pressing Run, which is not work this tool
        /// did and is not the time criterion 2 is about.
        /// </summary>
        [Test]
        public void TheRunIsRunStartedToRunFinishedAndNotTheSession()
        {
            RunClock clock = RunClock.From(1424.0, 473.0, 1424.0 - 0.0);

            Assert.That(clock.Marked, Is.True);
            Assert.That(clock.WaitingSeconds, Is.EqualTo(473.0));
            Assert.That(clock.RunSeconds, Is.EqualTo(951.0));
            Assert.That(clock.SessionSeconds, Is.EqualTo(1424.0));
        }

        /// <summary>
        /// The seconds between the last group finishing and the block being written are
        /// neither run nor waiting, so they land in a named row rather than vanishing.
        /// That is the same rule the outside every step row exists for.
        /// </summary>
        [Test]
        public void TheTailAfterTheRunIsNamedAndNeverFoldedIntoEitherOfTheOthers()
        {
            RunClock clock = RunClock.From(1000.0, 400.0, 900.0);

            Assert.That(clock.WaitingSeconds, Is.EqualTo(400.0));
            Assert.That(clock.RunSeconds, Is.EqualTo(500.0));
            Assert.That(clock.AfterSeconds, Is.EqualTo(100.0));
            Assert.That(clock.WaitingSeconds + clock.RunSeconds + clock.AfterSeconds,
                Is.EqualTo(clock.SessionSeconds));
        }

        /// <summary>
        /// The open file run and the two hand buttons never mark a run. The block says it
        /// fell back rather than dividing by zero or reporting a negative.
        /// </summary>
        [Test]
        public void WithNoRunMarkTheRunIsTheSessionAndTheBlockSaysSo()
        {
            RunClock clock = RunClock.NotMarked(300.0);

            Assert.That(clock.Marked, Is.False);
            Assert.That(clock.RunSeconds, Is.EqualTo(300.0));
            Assert.That(clock.WaitingSeconds, Is.EqualTo(0.0));
            Assert.That(clock.AfterSeconds, Is.EqualTo(0.0));

            string all = string.Join("\n", new List<string>(clock.Lines()).ToArray());

            Assert.That(all, Does.Contain("no run was marked"));
            Assert.That(all, Does.Contain("IS the session time"));
        }

        /// <summary>
        /// A clock that went back, which is what a machine syncing its time does, gives no
        /// time rather than time owed.
        /// </summary>
        [Test]
        public void AClockThatWentBackwardsNeverReportsANegative()
        {
            RunClock clock = RunClock.From(100.0, 90.0, 50.0);

            Assert.That(clock.RunSeconds, Is.EqualTo(0.0));
            Assert.That(clock.WaitingSeconds, Is.EqualTo(90.0));
            Assert.That(clock.AfterSeconds, Is.EqualTo(50.0));
        }

        [Test]
        public void TheBlockNamesAllThreeStretchesAndSaysWhichOneTheSharesUse()
        {
            string all = string.Join("\n",
                new List<string>(RunClock.From(1424.0, 473.0, 1424.0).Lines()).ToArray());

            Assert.That(all, Does.Contain(RunClock.WaitingForThePerson));
            Assert.That(all, Does.Contain(RunClock.AfterTheRun));
            Assert.That(all, Does.Contain("the run"));
            Assert.That(all, Does.Contain("every share below is worked off it"));
        }

        [Test]
        public void ALogWithBothMarksReportsTheRunAndTheWaitApart()
        {
            using (RunLog log = Start())
            {
                log.RunStarted(7);
                log.RunFinished();
                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("RUN      started, 7 groups"));
                Assert.That(text, Does.Contain("RUN      finished"));
                Assert.That(text, Does.Contain("run time       : "));
                Assert.That(text, Does.Contain("waiting for the person : "));
                Assert.That(text, Does.Contain("total elapsed  : "));
                Assert.That(log.WhereTheTimeWent.Marked, Is.True);
            }
        }

        [Test]
        public void ALogWithNoRunMarkStillWritesAResultBlock()
        {
            using (RunLog log = Start())
            {
                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(log.WhereTheTimeWent.Marked, Is.False);
                Assert.That(text, Does.Contain("no run was marked"));
                Assert.That(text, Does.Contain("which is the session, no run was marked"));
            }
        }

        [Test]
        public void TheResultBlockSaysWhatBothFilesCameTo()
        {
            using (RunLog log = Start())
            {
                log.Line("something");
                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("the .log so far: "));
                Assert.That(text, Does.Contain("read before this block finished writing"));
                Assert.That(text, Does.Contain("the .tsv       : "));
                Assert.That(text, Does.Contain("keeps every line the .log collapsed"));
            }
        }

        // ---------- F81, the .log is trimmed and the .tsv is not ----------

        /// <summary>
        /// The one that would silently break the brief. Collapsing a repeated line by not
        /// calling Numbered would cost the machine readable log 936 rows, which is exactly
        /// what F81 says not to do.
        /// </summary>
        [Test]
        public void ARepeatedLineCollapsesInTheLogAndEveryRowStillReachesTheTsv()
        {
            using (RunLog log = Start())
            {
                for (int i = 1; i <= 20; i++)
                {
                    log.NumberedRepeat(
                        "ROWS the two numbers agree",
                        "ROWS     test " + i + "  they agree",
                        "rows for the workbook",
                        "test " + i,
                        EventRow.Count(i),
                        "they agree");
                }

                string text = ReadWhileOpen(log.Path);
                string rows = ReadWhileOpen(log.RowLogPath);

                Assert.That(Occurrences(text, "they agree"),
                    Is.EqualTo(RunLog.KeptOfARepeat),
                    "five of the twenty, and then one line saying the rest are counted");
                Assert.That(text, Does.Contain("counted and not written out"));
                Assert.That(text, Does.Contain("test 5"));
                Assert.That(text, Does.Not.Contain("test 6  they agree"));

                Assert.That(Occurrences(rows, "rows for the workbook"), Is.EqualTo(20),
                    "the machine readable log keeps every one of them");
                Assert.That(rows, Does.Contain("test 20"));
            }
        }

        [Test]
        public void TwoDifferentKeysCollapseApart()
        {
            using (RunLog log = Start())
            {
                for (int i = 1; i <= 10; i++)
                {
                    log.NumberedRepeat("agree", "A" + i, "h", "n", "1", "t");
                    log.NumberedRepeat("differ", "D" + i, "h", "n", "1", "t");
                }

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("A5"));
                Assert.That(text, Does.Contain("D5"));
                Assert.That(text, Does.Not.Contain("A6"));
                Assert.That(text, Does.Not.Contain("D6"));
            }
        }

        /// <summary>
        /// A truncated list that does not say it truncated is the fault this log has
        /// already been caught by, so the count goes in the RESULT block as well.
        /// </summary>
        [Test]
        public void TheResultBlockSaysWhatWasCollapsed()
        {
            using (RunLog log = Start())
            {
                for (int i = 0; i < 936; i++)
                {
                    log.NumberedRepeat("ROWS the two numbers agree", "line " + i, "h", "n", "1", "t");
                }

                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("lines collapsed in this file"));
                Assert.That(text, Does.Contain("936 written as 5 and 931 counted"));
                Assert.That(text, Does.Contain("Every one is in the machine readable log"));
            }
        }

        [Test]
        public void ARunThatCollapsedNothingSaysNothingAboutIt()
        {
            using (RunLog log = Start())
            {
                log.NumberedRepeat("k", "one", "h", "n", "1", "t");
                log.WriteResultBlock();

                Assert.That(ReadWhileOpen(log.Path), Does.Not.Contain("lines collapsed"));
            }
        }

        // ---------- F81, the drift ----------

        /// <summary>
        /// One run wrote 1830 drift lines and every one said the same thing: the file says
        /// tolerance 0.025 and the test in the document has 0.075. What varies is the test
        /// name and nothing else.
        /// </summary>
        [Test]
        public void IdenticalDriftDifferencesAreGroupedAndCounted()
        {
            List<TestDifference> all = new List<TestDifference>();

            for (int i = 1; i <= 1830; i++)
            {
                all.Add(new TestDifference("test " + i, "tolerance", "0.025m", "0.075m"));
            }

            IList<string> lines = TestDrift.Grouped(all);

            Assert.That(lines[0], Does.StartWith("1830 tests: the file says tolerance 0.025m"));
            Assert.That(lines.Count, Is.EqualTo(1 + TestDrift.ExamplesShown + 1));
            Assert.That(lines[lines.Count - 1],
                Does.Contain("and 1825 more with the same difference, counted and not listed"));
        }

        [Test]
        public void TwoDifferentDifferencesAreTwoGroups()
        {
            IList<string> lines = TestDrift.Grouped(new[]
            {
                new TestDifference("a", "tolerance", "0.025m", "0.075m"),
                new TestDifference("b", "merge composites", "on", "off")
            });

            Assert.That(lines.Count, Is.EqualTo(4), "two headings and one name each");
            Assert.That(lines[0], Does.Contain("1 test: the file says tolerance"));
            Assert.That(lines[2], Does.Contain("1 test: the file says merge composites"));
        }

        [Test]
        public void NothingToGroupIsNotAThrow()
        {
            Assert.That(TestDrift.Grouped(null), Is.Empty);
            Assert.That(TestDrift.Grouped(new TestDifference[] { null }), Is.Empty);
        }

        // ---------- F82, the sets across the run ----------

        /// <summary>
        /// 38 of the client's 61 sets found nothing in every group of the first real run.
        /// That number was in the log seven times as seven separate group facts and
        /// nowhere as the one thing it means.
        /// </summary>
        [Test]
        public void ASetThatFoundNothingInEveryGroupIsTheFinding()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();

            for (int group = 0; group < 7; group++)
            {
                SetBuildOutcome outcome = new SetBuildOutcome();
                outcome.AddAlreadyPresent("tree/BLD-EL-Telecom Equipment", "Telecom Equipment", 1, 0);
                outcome.AddAlreadyPresent("tree/BLD-AR-Walls", "Walls", 1, 400);
                run.Add(outcome);
            }

            IList<SetAcrossTheRun> nowhere = run.FoundNothingAnywhere();

            Assert.That(run.Groups, Is.EqualTo(7));
            Assert.That(nowhere.Count, Is.EqualTo(1));
            Assert.That(nowhere[0].Path, Is.EqualTo("tree/BLD-EL-Telecom Equipment"));
            Assert.That(nowhere[0].GroupsSeen, Is.EqualTo(7));
            Assert.That(nowhere[0].GroupsAtZero, Is.EqualTo(7));
        }

        /// <summary>
        /// The break. A set at zero in six of seven groups is a different finding from one
        /// at zero in all seven, and only the second says the set itself is wrong.
        /// </summary>
        [Test]
        public void ASetThatFoundSomethingSomewhereIsNotTheFinding()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();

            for (int group = 0; group < 7; group++)
            {
                SetBuildOutcome outcome = new SetBuildOutcome();
                outcome.AddAlreadyPresent("tree/a", "a", 1, group == 3 ? 12 : 0);
                run.Add(outcome);
            }

            Assert.That(run.FoundNothingAnywhere(), Is.Empty);
            Assert.That(run.All()[0].GroupsAtZero, Is.EqualTo(6));
            Assert.That(run.All()[0].GroupsWithItems, Is.EqualTo(1));
        }

        /// <summary>
        /// SetResult.IsZero is true only of a CREATED set. On a weekly run every set is
        /// already there and carries Present, so reading IsZero here would make the block
        /// empty on exactly the runs it was written for.
        /// </summary>
        [Test]
        public void ASetThatWasAlreadyThereAndFoundNothingStillCounts()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult present = outcome.AddAlreadyPresent("tree/a", "a", 1, 0);

            Assert.That(present.IsZero, Is.False, "IsZero is about a created set");
            Assert.That(present.ItemCount, Is.EqualTo(0));

            SetsAcrossTheRun run = new SetsAcrossTheRun();
            run.Add(outcome);

            Assert.That(run.FoundNothingAnywhere().Count, Is.EqualTo(1));
        }

        [Test]
        public void ASetThatNeverResolvedIsUnknownAndNotZero()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("tree/a", "a", 1, -1);
            run.Add(outcome);

            Assert.That(run.All(), Is.Empty, "minus one is UNKNOWN and never zero");
            Assert.That(run.Groups, Is.EqualTo(1));
        }

        [Test]
        public void TheBlockNamesTenAndCountsTheRest()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < 38; i++)
            {
                outcome.AddAlreadyPresent("tree/set " + i.ToString("00"), "set", 1, 0);
            }

            run.Add(outcome);

            string all = string.Join("\n", new List<string>(run.Lines()).ToArray());

            Assert.That(all, Does.Contain("groups in this run : 1"));
            Assert.That(all, Does.Contain("sets looked at     : 38"));
            Assert.That(all, Does.Contain("found nothing in every group : 38"));
            Assert.That(all, Does.Contain("and 28 more that found nothing in every group, "
                + "counted and not listed"));
        }

        [Test]
        public void ARunWhereEverySetFoundSomethingSaysSo()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("tree/a", "a", 1, 5);
            run.Add(outcome);

            Assert.That(string.Join("\n", new List<string>(run.Lines()).ToArray()),
                Does.Contain("Every set found something somewhere."));
        }

        /// <summary>
        /// The block is written with Block, which writes no row at all, so the rows are
        /// added on their own or the machine readable log does not carry what the block
        /// carries.
        /// </summary>
        [Test]
        public void EverySetGetsARowForTheMachineReadableLog()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("tree/a", "a", 1, 0);
            outcome.AddAlreadyPresent("tree/b", "b", 1, 9);
            run.Add(outcome);

            IList<SetRunRow> rows = run.Rows();

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0].Path, Is.EqualTo("tree/a"));
            Assert.That(rows[0].Phrase(), Is.EqualTo("found nothing in 1 of 1 group"));
            Assert.That(rows[1].Phrase(), Is.EqualTo("found nothing in 0 of 1 group"));
        }

        [Test]
        public void NothingToAddIsNotAThrow()
        {
            SetsAcrossTheRun run = new SetsAcrossTheRun();
            run.Add(null);

            Assert.That(run.Groups, Is.EqualTo(0));
            Assert.That(run.All(), Is.Empty);
        }

        [Test]
        public void TheBlockHasOneTitleAndNothingElseSpellsIt()
        {
            Assert.That(SetsAcrossTheRun.BlockTitle, Is.EqualTo("SETS ACROSS THE RUN"));
        }

        /// <summary>
        /// A19. The repeat threshold AT the number. Exactly five go out in full and nothing
        /// says the rest were counted, because there is no rest. A less than written as at
        /// most would pass every test above this one and fail here.
        /// </summary>
        [Test]
        public void ExactlyTheKeptCountWritesEveryLineAndNoCountedLine()
        {
            using (RunLog log = Start())
            {
                for (int i = 1; i <= RunLog.KeptOfARepeat; i++)
                {
                    log.NumberedRepeat("k", "line " + i + "  they agree", "h", "n" + i, "1", "t");
                }

                string text = ReadWhileOpen(log.Path);

                Assert.That(Occurrences(text, "they agree"), Is.EqualTo(RunLog.KeptOfARepeat));
                Assert.That(text, Does.Not.Contain("counted and not written out"));
            }
        }

        /// <summary>A19. One past the threshold is the first that must collapse.</summary>
        [Test]
        public void OnePastTheKeptCountWritesFiveAndTheCountedLine()
        {
            using (RunLog log = Start())
            {
                for (int i = 1; i <= RunLog.KeptOfARepeat + 1; i++)
                {
                    log.NumberedRepeat("k", "line " + i + "  they agree", "h", "n" + i, "1", "t");
                }

                string text = ReadWhileOpen(log.Path);

                Assert.That(Occurrences(text, "they agree"), Is.EqualTo(RunLog.KeptOfARepeat));
                Assert.That(text, Does.Contain("counted and not written out"));
                Assert.That(text, Does.Contain("line " + RunLog.KeptOfARepeat + "  they agree"));
                Assert.That(text, Does.Not.Contain("line " + (RunLog.KeptOfARepeat + 1) + "  they agree"));
            }
        }

        /// <summary>A19. The sets block AT ten names all ten and says nothing about more.</summary>
        [Test]
        public void ExactlyTenSetsAtZeroAreAllNamedAndOnePastItIsCounted()
        {
            SetsAcrossTheRun at = new SetsAcrossTheRun();
            SetBuildOutcome ten = new SetBuildOutcome();

            for (int i = 0; i < SetsAcrossTheRun.ExamplesShown; i++)
            {
                ten.AddAlreadyPresent("tree/set " + i.ToString("00"), "set", 1, 0);
            }

            at.Add(ten);
            string all = string.Join("\n", new List<string>(at.Lines()).ToArray());

            Assert.That(all, Does.Contain("tree/set " + (SetsAcrossTheRun.ExamplesShown - 1).ToString("00")));
            Assert.That(all, Does.Not.Contain("more that found nothing in every group"));

            SetsAcrossTheRun past = new SetsAcrossTheRun();
            SetBuildOutcome eleven = new SetBuildOutcome();

            for (int i = 0; i <= SetsAcrossTheRun.ExamplesShown; i++)
            {
                eleven.AddAlreadyPresent("tree/set " + i.ToString("00"), "set", 1, 0);
            }

            past.Add(eleven);
            all = string.Join("\n", new List<string>(past.Lines()).ToArray());

            Assert.That(all, Does.Not.Contain("tree/set " + SetsAcrossTheRun.ExamplesShown.ToString("00")));
            Assert.That(all, Does.Contain("and 1 more that found nothing in every group, counted and not listed"));
        }

        /// <summary>A19. Exactly five in one bucket are all named and six is the first that counts.</summary>
        [Test]
        public void ExactlyFiveDriftingTestsAreAllNamedAndOnePastItIsCounted()
        {
            List<TestDifference> all = new List<TestDifference>();

            for (int i = 1; i <= TestDrift.ExamplesShown; i++)
            {
                all.Add(new TestDifference("test " + i, "tolerance", "0.025m", "0.075m"));
            }

            IList<string> lines = TestDrift.Grouped(all);
            string text = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(lines.Count, Is.EqualTo(1 + TestDrift.ExamplesShown), "the bucket line and five names");
            Assert.That(text, Does.Contain("test " + TestDrift.ExamplesShown));
            Assert.That(text, Does.Not.Contain("more with the same difference"));

            all.Add(new TestDifference("test " + (TestDrift.ExamplesShown + 1), "tolerance", "0.025m", "0.075m"));
            lines = TestDrift.Grouped(all);
            text = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(lines.Count, Is.EqualTo(1 + TestDrift.ExamplesShown + 1));
            Assert.That(text, Does.Not.Contain("test " + (TestDrift.ExamplesShown + 1)));
            Assert.That(lines[lines.Count - 1],
                Does.Contain("and 1 more with the same difference, counted and not listed"));
        }
    }
}
