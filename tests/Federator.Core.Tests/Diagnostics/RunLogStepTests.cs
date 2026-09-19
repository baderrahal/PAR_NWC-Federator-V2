using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The steps as the log writes them, read back off the disk as a file.
    ///
    /// Read back off the disk and never off the object that wrote it, because the object
    /// model has reported a broken file as fine more than once, and because the whole
    /// point of this log is that what is on the disk is what happened.
    /// </summary>
    [TestFixture]
    public class RunLogStepTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorRunLogSteps");
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

        private static string ReadWhileOpen(RunLog log)
        {
            using (FileStream stream = new FileStream(
                       log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

        [Test]
        public void AStepWritesOneLineWhenItStartsAndOneWhenItFinishes()
        {
            using (RunLog log = Start())
            {
                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published 82,114,000 bytes");
                }

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("STEP     NWD"));
                Assert.That(text, Does.Contain("started"));
                Assert.That(text, Does.Contain("finished"));
                Assert.That(text, Does.Contain("published 82,114,000 bytes"));
            }
        }

        /// <summary>
        /// The one that keeps the log readable. A step entered 1830 times writes one pair
        /// of lines and one line saying the rest are counted, not 3660 lines. This is the
        /// same rule the repeated failure already follows, and it is here because one run
        /// left a 17.8 MB log that was almost entirely one thing said over and over.
        /// </summary>
        [Test]
        public void AStepEnteredAgainAndAgainIsCountedAndNotWrittenOut()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                for (int i = 0; i < 50; i++)
                {
                    using (RunStep step = log.Step(RunSteps.TestsRun))
                    {
                        step.Changed("test " + i);
                    }
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 1.0, null, "Weekly run");

                string text = ReadWhileOpen(log);

                Assert.That(Occurrences(text, "STEP     TESTS RUN     started"), Is.EqualTo(1));
                Assert.That(Occurrences(text, "STEP     TESTS RUN     finished"), Is.EqualTo(1));
                Assert.That(text, Does.Contain("Every further visit in this group is counted"));
                Assert.That(text, Does.Contain("50 visits"));
                Assert.That(text, Does.Not.Contain("test 49"));
            }
        }

        /// <summary>
        /// The break. A step nobody closed is named at the end of its group with the
        /// seconds it had been open. Silence here would let the timing block understate
        /// the run, which is the one thing it must not do.
        /// </summary>
        [Test]
        public void AStepNeverClosedIsNamedWhenTheGroupEnds()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06BC", new List<string>());

                // Deliberately not in a using block. This is what the rest of the code
                // must never do, and the log has to survive it saying so.
                log.Step(RunSteps.Images);

                log.GroupFinished("1B06BC", GroupOutcome.Done, 2.0, null, "First run");

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("STEP     IMAGES"));
                Assert.That(text, Does.Contain("NEVER CLOSED"));

                // Named before the GROUP finished line, so it reads against the group it
                // belonged to and not against the next one.
                Assert.That(
                    text.IndexOf("NEVER CLOSED", StringComparison.Ordinal),
                    Is.LessThan(text.IndexOf("GROUP    finished", StringComparison.Ordinal)));
            }
        }

        [Test]
        public void EverythingStillOpenIsClosedDeepestFirst()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06G1", new List<string>());

                log.Step(RunSteps.Sets);
                log.Step(RunSteps.TestsCreate);

                log.GroupFinished("1B06G1", GroupOutcome.Done, 3.0, null, "First run");

                string text = ReadWhileOpen(log);

                // Both are open when the group ends, so both are named. The names are
                // matched rather than the padding, which the step list works out.
                int inner = text.IndexOf(
                    RunSteps.Padded(RunSteps.TestsCreate) + "  NEVER CLOSED", StringComparison.Ordinal);
                int outer = text.IndexOf(
                    RunSteps.Padded(RunSteps.Sets) + "  NEVER CLOSED", StringComparison.Ordinal);

                Assert.That(inner, Is.GreaterThan(0), "TESTS CREATE was not named");
                Assert.That(outer, Is.GreaterThan(0), "SETS was not named");
                Assert.That(inner, Is.LessThan(outer), "the inner step closes first");
            }
        }

        [Test]
        public void AStepInsideAStepIsIndentedInTheFile()
        {
            using (RunLog log = Start())
            {
                using (RunStep outer = log.Step(RunSteps.Sets))
                {
                    using (RunStep inner = log.Step(RunSteps.TestsCreate))
                    {
                        Assert.That(outer.Depth, Is.EqualTo(0));
                        Assert.That(inner.Depth, Is.EqualTo(1));
                        inner.Changed("1830 created");
                    }

                    outer.Changed("61 sets");
                }

                Assert.That(ReadWhileOpen(log), Does.Contain("STEP       TESTS CREATE"));
            }
        }

        /// <summary>
        /// Every closed step is recorded, with the group it belonged to, because the
        /// timing blocks are built off this list and off nothing else, so what the lines
        /// say and what the block says cannot disagree.
        /// </summary>
        [Test]
        public void EveryClosedStepIsRecordedAgainstItsGroup()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Units))
                {
                    step.Changed("5 models set");
                }

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 4.0, null, "First run");

                IList<StepRecord> records = log.StepRecords;

                Assert.That(records.Count, Is.EqualTo(2));
                Assert.That(records[0].Name, Is.EqualTo(RunSteps.Units));
                Assert.That(records[0].Group, Is.EqualTo("1B06PH"));
                Assert.That(records[1].Name, Is.EqualTo(RunSteps.Nwd));
                Assert.That(records[1].Group, Is.EqualTo("1B06PH"));
            }
        }

        [Test]
        public void TheVisitCountStartsAgainWithEachGroup()
        {
            using (RunLog log = Start())
            {
                foreach (string building in new[] { "1B06PH", "1B06BC" })
                {
                    log.GroupStarted(building, new List<string>());

                    for (int i = 0; i < 3; i++)
                    {
                        using (RunStep step = log.Step(RunSteps.TestsRun))
                        {
                            step.Changed("one test");
                        }
                    }

                    log.GroupFinished(building, GroupOutcome.Done, 1.0, null, "First run");
                }

                string text = ReadWhileOpen(log);

                // Two groups, so two start lines and two repeat totals, not one of each.
                // The repeat total is matched with its own prefix, because the TIMING
                // block of each group carries a visit count on its rows as well.
                Assert.That(Occurrences(text, "STEP     TESTS RUN     started"), Is.EqualTo(2));
                Assert.That(
                    Occurrences(text, "STEP     " + RunSteps.Padded(RunSteps.TestsRun) + "  3 visits"),
                    Is.EqualTo(2));
            }
        }

        [Test]
        public void ALogThatCannotWriteToDiskStillTakesSteps()
        {
            using (RunLog log = RunLog.StartOrDisabled(null, new DateTime(2026, 9, 19), 5))
            {
                List<string> said = new List<string>();
                log.LineWritten += said.Add;

                using (RunStep step = log.Step(RunSteps.Confirm))
                {
                    step.Changed("the NWF is intact");
                }

                Assert.That(said, Has.Some.Contains("STEP     CONFIRM"));
                Assert.That(log.StepRecords.Count, Is.EqualTo(1));
            }
        }

        /// <summary>
        /// The two buttons on the Clash step run outside any group. Each press is its own
        /// occasion, so the second one writes its pair of lines rather than reading as a
        /// repeat of the first and going silent.
        /// </summary>
        [Test]
        public void AStepOutsideAnyGroupIsNeverCountedAsARepeat()
        {
            using (RunLog log = Start())
            {
                for (int i = 0; i < 3; i++)
                {
                    using (RunStep step = log.Step(RunSteps.Sets))
                    {
                        step.Changed("press " + i);
                    }
                }

                string text = ReadWhileOpen(log);

                Assert.That(Occurrences(text, "STEP     SETS          started"), Is.EqualTo(3));
                Assert.That(Occurrences(text, "STEP     SETS          finished"), Is.EqualTo(3));
                Assert.That(text, Does.Not.Contain("counted, not written out"));
                Assert.That(text, Does.Contain("press 2"));
            }
        }

        [Test]
        public void ANameThatIsNotAStepIsRefusedByTheLogToo()
        {
            using (RunLog log = Start())
            {
                Assert.That(() => log.Step("CLASH"), Throws.ArgumentException);
                Assert.That(log.StepRecords, Is.Empty);
            }
        }
    }
}
