using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The census as the log takes it, read back off the disk as a file.
    ///
    /// The log cannot count anything in a Navisworks document and must not try. The
    /// add-in hands a reader in, the log decides when to call it, and Core decides what
    /// the answer means. These tests hand in a reader of their own, which is how every
    /// rule about the census is provable without Navisworks on the machine.
    /// </summary>
    [TestFixture]
    public class RunLogCensusTests
    {
        private string folder;
        private List<DocumentCensus> answers;
        private int asked;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorRunLogCensus");
            answers = new List<DocumentCensus>();
            asked = 0;
            CensusCost.TooLongSeconds = CensusCost.DefaultTooLongSeconds;
        }

        [TearDown]
        public void RemoveFolder()
        {
            CensusCost.TooLongSeconds = CensusCost.DefaultTooLongSeconds;
            TempFolder.Remove(folder);
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 9, 19, 9, 0, 0));
        }

        /// <summary>
        /// Hands back the answers in order and repeats the last one for ever, so a test
        /// sets out only the counts it cares about.
        /// </summary>
        private DocumentCensus Next()
        {
            asked++;

            if (answers.Count == 0)
            {
                return new DocumentCensus(5, 61, 1830, 0, 0);
            }

            int at = asked - 1 < answers.Count ? asked - 1 : answers.Count - 1;
            return answers[at];
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
        public void WithNoReaderNothingIsCountedAndNoCensusLineIsWritten()
        {
            using (RunLog log = Start())
            {
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 1.0, null, "First run");

                Assert.That(ReadWhileOpen(log), Does.Not.Contain("CENSUS"));
                Assert.That(log.CensusFaults, Is.Empty);
            }
        }

        [Test]
        public void ACensusIsWrittenBeforeAndAfterAStep()
        {
            using (RunLog log = Start())
            {
                log.CensusReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("CENSUS   before NWD"));
                Assert.That(text, Does.Contain("CENSUS   after  NWD"));
                Assert.That(text, Does.Contain("models 5"));
                Assert.That(text, Does.Contain("tests 1830"));
            }
        }

        /// <summary>
        /// The break, and the whole reason the census exists. A count that moved during a
        /// step that may not move it is named with the step, the count, the before and
        /// the after, and the reason reaches the group so it is not DONE.
        /// </summary>
        [Test]
        public void ACountMovingWhereItMayNotIsNamedAndPutsTheGroupOutOfDone()
        {
            answers.Add(new DocumentCensus(5, 61, 1830, 412, 0));
            answers.Add(new DocumentCensus(5, 61, 0, 412, 0));

            using (RunLog log = Start())
            {
                log.CensusReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("CENSUS CHANGED"));
                Assert.That(text, Does.Contain("clash tests went from 1830 to 0"));
                Assert.That(text, Does.Contain("This group is not DONE"));

                Assert.That(log.CensusFaults.Count, Is.EqualTo(1));
                Assert.That(log.CensusFaults[0], Does.Contain("clash tests went from 1830 to 0"));
                Assert.That(log.CensusFaults[0], Does.Contain(RunSteps.Nwd));
            }
        }

        [Test]
        public void ACountMovingWhereItMayIsNotAFaultAndWritesNoChangedLine()
        {
            answers.Add(new DocumentCensus(5, 61, 1830, 0, 0));
            answers.Add(new DocumentCensus(5, 61, 1830, 412, 0));

            using (RunLog log = Start())
            {
                log.CensusReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.TestsRun))
                {
                    step.Changed("ran");
                }

                Assert.That(ReadWhileOpen(log), Does.Not.Contain("CENSUS CHANGED"));
                Assert.That(log.CensusFaults, Is.Empty);
            }
        }

        /// <summary>
        /// A step entered 1830 times is counted around ONCE. Counting the whole document
        /// around every visit would be the log making the run slower, which is the one
        /// thing it must never do.
        /// </summary>
        [Test]
        public void AStepEnteredAgainAndAgainIsCountedAroundOnlyOnce()
        {
            using (RunLog log = Start())
            {
                log.CensusReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                for (int i = 0; i < 50; i++)
                {
                    using (RunStep step = log.Step(RunSteps.TestsRun))
                    {
                        step.Changed("one test");
                    }
                }

                string text = ReadWhileOpen(log);

                Assert.That(Occurrences(text, "CENSUS   before TESTS RUN"), Is.EqualTo(1));
                Assert.That(Occurrences(text, "CENSUS   after  TESTS RUN"), Is.EqualTo(1));
                Assert.That(asked, Is.EqualTo(2), "two counts for fifty visits");
            }
        }

        [Test]
        public void EachGroupIsCountedAgain()
        {
            using (RunLog log = Start())
            {
                log.CensusReader = Next;

                foreach (string building in new[] { "1B06PH", "1B06BC" })
                {
                    log.GroupStarted(building, new List<string>());

                    using (RunStep step = log.Step(RunSteps.Nwd))
                    {
                        step.Changed("published");
                    }

                    log.GroupFinished(building, GroupOutcome.Done, 1.0, null, "First run");
                }

                Assert.That(Occurrences(ReadWhileOpen(log), "CENSUS   before NWD"), Is.EqualTo(2));
            }
        }

        [Test]
        public void TheFaultsStartAgainWithEachGroup()
        {
            answers.Add(new DocumentCensus(5, 61, 1830, 0, 0));
            answers.Add(new DocumentCensus(5, 0, 1830, 0, 0));

            using (RunLog log = Start())
            {
                log.CensusReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                Assert.That(log.CensusFaults.Count, Is.EqualTo(1));

                log.GroupFinished("1B06PH", GroupOutcome.Failed, 1.0, "the census", "First run");
                log.GroupStarted("1B06BC", new List<string>());

                Assert.That(log.CensusFaults, Is.Empty, "the next group starts clean");
            }
        }

        [Test]
        public void WhatTheCensusCostIsSaidOncePerGroup()
        {
            using (RunLog log = Start())
            {
                log.CensusReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 1.0, null, "First run");

                string text = ReadWhileOpen(log);

                Assert.That(Occurrences(text, "CENSUS   cost "), Is.EqualTo(1));
                Assert.That(text, Does.Contain("2 counts in this group"));
                Assert.That(text, Does.Contain("inside the 1.000s a group is allowed"));
            }
        }

        /// <summary>
        /// A reader that throws never stops a run and never comes back as zeros. It comes
        /// back as UNKNOWN, which no rule reads as a change, and the log says what threw.
        /// </summary>
        [Test]
        public void AReaderThatThrowsSaysSoAndTheRunCarriesOn()
        {
            using (RunLog log = Start())
            {
                log.CensusReader = delegate
                {
                    throw new InvalidOperationException("the document went away");
                };

                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("CENSUS   could not be taken"));
                Assert.That(text, Does.Contain("the document went away"));
                Assert.That(text, Does.Contain("models UNKNOWN"));
                Assert.That(log.CensusFaults, Is.Empty, "UNKNOWN is never a change");
            }
        }

        /// <summary>
        /// A group that spent over its second counting narrows the census from the next
        /// group on, and both the group that spent it and the group that inherits the
        /// narrower census say so.
        /// </summary>
        [Test]
        public void AGroupThatSpentTooLongCountingNarrowsTheNextOne()
        {
            CensusCost.TooLongSeconds = 0.000000001;

            using (RunLog log = Start())
            {
                log.CensusReader = Next;

                log.GroupStarted("1B06PH", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 1.0, null, "First run");

                log.GroupStarted("1B06BC", new List<string>());

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                using (RunStep step = log.Step(RunSteps.TestsRun))
                {
                    step.Changed("ran");
                }

                log.GroupFinished("1B06BC", GroupOutcome.Done, 1.0, null, "First run");

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("CENSUS   narrowed"));
                Assert.That(text, Does.Contain("only around the steps that may write"));

                // NWD may move nothing, so the narrowed group does not count around it,
                // and TESTS RUN may move the results, so it still does.
                Assert.That(Occurrences(text, "CENSUS   before NWD"), Is.EqualTo(1));
                Assert.That(Occurrences(text, "CENSUS   before TESTS RUN"), Is.EqualTo(1));
            }
        }
    }
}
