using System;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// One step, opened and closed. The clock is driven by the test rather than by real
    /// seconds passing, so the line shapes are proved exactly and nothing here can fail
    /// because the machine was busy.
    /// </summary>
    [TestFixture]
    public class RunStepTests
    {
        private double now;

        [SetUp]
        public void StartTheClock()
        {
            now = 0.0;
        }

        private double Clock()
        {
            return now;
        }

        private RunStep Open(string name)
        {
            return new RunStep(name, 0, Clock, null);
        }

        [Test]
        public void TheStartLineNamesTheStepAndSaysStarted()
        {
            RunStep step = Open(RunSteps.Nwd);

            Assert.That(step.StartLine(), Does.StartWith("STEP     NWD"));
            Assert.That(step.StartLine(), Does.EndWith("started"));
            Assert.That(step.Closed, Is.False);
            Assert.That(step.Seconds, Is.EqualTo(0.0));
        }

        [Test]
        public void TheFinishLineCarriesTheSecondsAndWhatItChanged()
        {
            RunStep step = Open(RunSteps.Workbook);
            now = 12.5;
            step.Changed("1830 rows in 53 blocks");
            step.Dispose();

            Assert.That(step.Closed, Is.True);
            Assert.That(step.Seconds, Is.EqualTo(12.5).Within(0.0001));
            Assert.That(step.FinishLine(), Does.StartWith("STEP     WORKBOOK"));
            Assert.That(step.FinishLine(), Does.Contain("finished"));
            Assert.That(step.FinishLine(), Does.Contain("12.500s"));
            Assert.That(step.FinishLine(), Does.Contain("1830 rows in 53 blocks"));
        }

        /// <summary>
        /// A step that said nothing says UNKNOWN rather than leaving a blank, because a
        /// blank reads as a step that changed nothing rather than one nobody described.
        /// </summary>
        [Test]
        public void AStepThatSaidNothingSaysUnknown()
        {
            RunStep step = Open(RunSteps.Units);
            now = 1.0;
            step.Dispose();

            Assert.That(step.FinishLine(), Does.Contain("UNKNOWN"));
        }

        /// <summary>
        /// The break. A step whose work threw still closes, still carries its seconds,
        /// and says THREW rather than finished. Time spent failing is time the run spent.
        /// </summary>
        [Test]
        public void AStepWhoseWorkThrewSaysSoAndKeepsItsSeconds()
        {
            RunStep step = Open(RunSteps.TestsRun);

            try
            {
                using (step)
                {
                    now = 8.25;
                    step.Failed();
                    throw new InvalidOperationException("the handle died");
                }
            }
            catch (InvalidOperationException)
            {
            }

            Assert.That(step.Closed, Is.True, "the using block has to close it");
            Assert.That(step.Threw, Is.True);
            Assert.That(step.Seconds, Is.EqualTo(8.25).Within(0.0001));
            Assert.That(step.FinishLine(), Does.Contain("THREW"));
            Assert.That(step.FinishLine(), Does.Not.Contain("finished"));
            Assert.That(step.FinishLine(), Does.Contain("8.250s"));
        }

        /// <summary>
        /// A step that threw and was never told so still closes on the way out, which is
        /// what the using block is for. It reads as finished, because nothing said it
        /// threw, and it carries the seconds either way.
        /// </summary>
        [Test]
        public void AThrowClosesTheStepEvenWhenNothingCalledFailed()
        {
            RunStep step = Open(RunSteps.Append);

            try
            {
                using (step)
                {
                    now = 3.0;
                    throw new InvalidOperationException("nothing caught this in time");
                }
            }
            catch (InvalidOperationException)
            {
            }

            Assert.That(step.Closed, Is.True);
            Assert.That(step.Seconds, Is.EqualTo(3.0).Within(0.0001));
        }

        [Test]
        public void ClosingTwiceDoesNotMoveTheSeconds()
        {
            RunStep step = Open(RunSteps.Html);
            now = 2.0;
            step.Dispose();
            now = 90.0;
            step.Dispose();

            Assert.That(step.Seconds, Is.EqualTo(2.0).Within(0.0001));
        }

        /// <summary>
        /// The one the monotonic rule exists for. A clock that goes back, which is what a
        /// machine syncing its time does, must never give a step a negative duration,
        /// because that would take seconds off the group total and make a slow run read
        /// as a fast one.
        /// </summary>
        [Test]
        public void AClockThatGoesBackwardsGivesNoNegativeStep()
        {
            RunStep step = Open(RunSteps.Confirm);
            now = -30.0;
            step.Dispose();

            Assert.That(step.Seconds, Is.EqualTo(0.0));
            Assert.That(step.FinishLine(), Does.Contain("0.000s"));
        }

        /// <summary>
        /// A step inside a step. The inner one is indented so the log reads the way the
        /// work happened, and it carries its depth so the timing block can work its
        /// shares out over the top level alone.
        /// </summary>
        [Test]
        public void AStepInsideAStepIsIndentedAndKnowsHowDeepItIs()
        {
            RunStep outer = new RunStep(RunSteps.Sets, 0, Clock, null);
            RunStep inner = new RunStep(RunSteps.TestsCreate, 1, Clock, null);

            Assert.That(outer.Depth, Is.EqualTo(0));
            Assert.That(inner.Depth, Is.EqualTo(1));
            Assert.That(outer.StartLine(), Does.StartWith("STEP     SETS"));
            Assert.That(inner.StartLine(), Does.StartWith("STEP       TESTS CREATE"));
            Assert.That(
                inner.StartLine().IndexOf(RunSteps.TestsCreate, StringComparison.Ordinal),
                Is.GreaterThan(outer.StartLine().IndexOf(RunSteps.Sets, StringComparison.Ordinal)));
        }

        /// <summary>
        /// A step never closed says how long it had been open and says plainly that it
        /// never finished. Silence here is what would let the timing block understate a
        /// run, which is the one thing it must not do.
        /// </summary>
        [Test]
        public void AStepNeverClosedSaysSoAndSaysHowLongItHadBeenOpen()
        {
            RunStep step = Open(RunSteps.Harvest);
            now = 40.0;

            Assert.That(step.Closed, Is.False);
            Assert.That(step.SecondsSoFar(), Is.EqualTo(40.0).Within(0.0001));
            Assert.That(step.NeverClosedLine(), Does.Contain("NEVER CLOSED"));
            Assert.That(step.NeverClosedLine(), Does.Contain("40.000s"));
            Assert.That(step.NeverClosedLine(), Does.Contain(RunSteps.Harvest));
        }

        [Test]
        public void ANameThatIsNotAStepIsRefused()
        {
            Assert.That(
                () => new RunStep("CLASH", 0, Clock, null),
                Throws.ArgumentException.With.Message.Contains("CLASH"));

            Assert.That(() => new RunStep(null, 0, Clock, null), Throws.ArgumentException);
            Assert.That(() => new RunStep(RunSteps.Nwd, 0, null, null), Throws.ArgumentNullException);
        }

        [Test]
        public void TheRepeatedLineCarriesTheVisitsAndTheTotal()
        {
            string line = StepRecord.RepeatedLine(RunSteps.TestsRun, 1830, 742.125, 0);

            Assert.That(line, Does.StartWith("STEP     TESTS RUN"));
            Assert.That(line, Does.Contain("1830 visits"));
            Assert.That(line, Does.Contain("742.125s in total"));
            Assert.That(line, Does.Not.Contain("threw"));

            Assert.That(
                StepRecord.RepeatedLine(RunSteps.TestsRun, 1830, 742.125, 3),
                Does.Contain("3 of them threw"));
        }

        [Test]
        public void ARecordRefusesAStepNameItDoesNotKnow()
        {
            Assert.That(
                () => new StepRecord("1B06PH", "CLASH", 0, 0.0, 1.0, false),
                Throws.ArgumentException);
        }
    }
}
