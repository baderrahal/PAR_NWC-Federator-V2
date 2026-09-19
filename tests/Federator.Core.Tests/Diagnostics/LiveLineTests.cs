using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The line the window shows while the run works.
    ///
    /// The clock is driven by the test rather than by real seconds passing, so the shapes
    /// are proved exactly and nothing here fails because the machine was busy.
    /// </summary>
    [TestFixture]
    public class LiveLineTests
    {
        private double now;
        private LiveLine live;

        [SetUp]
        public void Start()
        {
            now = 0.0;
            live = new LiveLine(Clock);
            LiveLine.SlowerThan = LiveLine.DefaultSlowerThan;
        }

        [TearDown]
        public void PutTheSettingBack()
        {
            LiveLine.SlowerThan = LiveLine.DefaultSlowerThan;
        }

        private double Clock()
        {
            return now;
        }

        [Test]
        public void TheLineCarriesTheGroupTheBuildingTheStepAndBothClocks()
        {
            live.Groups(3, 14, "1B06PH");
            now = 100.0;
            live.StepStarted(RunSteps.TestsRun, -1.0);
            now = 112.0;

            string line = live.Line(null);

            Assert.That(line, Does.Contain("Group 3 of 14"));
            Assert.That(line, Does.Contain("1B06PH"));
            Assert.That(line, Does.Contain(RunSteps.TestsRun));
            Assert.That(line, Does.Contain("12s on this step"));
            Assert.That(line, Does.Contain("1m 52s on the run"));
        }

        [Test]
        public void TheSentenceTheRunWantedToSayIsStillOnTheEnd()
        {
            live.Groups(1, 1, "1B06PH");
            Assert.That(live.Line("Test 25 of 1830: BLD-ME v BLD-EL"),
                Does.EndWith("Test 25 of 1830: BLD-ME v BLD-EL"));
        }

        [Test]
        public void WithNoStepOpenTheLineIsTheGroupAndTheRun()
        {
            live.Groups(2, 14, "1B06BC");
            now = 65.0;

            string line = live.Line(null);

            Assert.That(line, Does.Contain("Group 2 of 14"));
            Assert.That(line, Does.Contain("1m 05s on the run"));
            Assert.That(line, Does.Not.Contain("on this step"));
        }

        [Test]
        public void BeforeAnyGroupTheLineIsJustTheRunClockAndWhateverWasSaid()
        {
            now = 4.0;

            string line = live.Line("Scanning");

            Assert.That(line, Is.EqualTo("4s on the run  Scanning"));
        }

        /// <summary>
        /// The one Bader asked for. A step running longer than twice what the same step
        /// took on the group before says so, so a run going wrong is visible while it is
        /// going wrong rather than in the log afterwards.
        /// </summary>
        [Test]
        public void AStepOverTwiceTheGroupBeforeSaysSo()
        {
            live.Groups(2, 14, "1B06BC");
            live.StepStarted(RunSteps.TestsRun, 40.0);

            now = 80.0;
            Assert.That(live.SlowerThanLastGroup(), Is.False, "exactly twice is not longer than twice");
            Assert.That(live.Line(null), Does.Not.Contain("SLOWER"));

            now = 81.0;
            Assert.That(live.SlowerThanLastGroup(), Is.True);
            Assert.That(live.Line(null), Does.Contain("SLOWER, the group before took 40s on this step"));
        }

        [Test]
        public void WithNothingToCompareAgainstNothingIsSaidAboutThePace()
        {
            live.Groups(1, 14, "1B06PH");
            live.StepStarted(RunSteps.TestsRun, -1.0);
            now = 9000.0;

            Assert.That(live.SlowerThanLastGroup(), Is.False);
            Assert.That(live.Line(null), Does.Not.Contain("SLOWER"));
        }

        [Test]
        public void AStepThatEndsTakesThePaceWithIt()
        {
            live.Groups(2, 14, "1B06BC");
            live.StepStarted(RunSteps.TestsRun, 1.0);
            now = 500.0;

            Assert.That(live.Line(null), Does.Contain("SLOWER"));

            live.StepEnded();

            Assert.That(live.SecondsOnStep(), Is.EqualTo(0.0));
            Assert.That(live.Line(null), Does.Not.Contain("SLOWER"));
            Assert.That(live.Line(null), Does.Not.Contain("on this step"));
        }

        /// <summary>
        /// A loop over 1830 tests must not repaint the window 1830 times, and it must not
        /// go quiet either. At most once a second is the rule, and the step changing
        /// always says.
        /// </summary>
        [Test]
        public void TheLineIsRenderedAtMostOnceASecondAndAlwaysWhenTheStepChanges()
        {
            live.Groups(1, 1, "1B06PH");
            live.StepStarted(RunSteps.TestsRun, -1.0);

            Assert.That(live.ShouldSay(), Is.True, "a step that just started has not said anything yet");
            live.Line(null);

            Assert.That(live.ShouldSay(), Is.False);

            now += 0.9;
            Assert.That(live.ShouldSay(), Is.False);

            now += 0.1;
            Assert.That(live.ShouldSay(), Is.True);
            live.Line(null);
            Assert.That(live.ShouldSay(), Is.False);

            live.StepStarted(RunSteps.Nwd, -1.0);
            Assert.That(live.ShouldSay(), Is.True, "the step changed");
        }

        /// <summary>
        /// The fault this pins cost a render that mattered. Line RENDERS and records that
        /// it did, so anything that renders the line itself and then hands the result to
        /// a throttle gets skipped by that throttle. A caller that wants a throttle to do
        /// the rendering hands it a sentence, never a line.
        /// </summary>
        [Test]
        public void RenderingTheLineIsWhatMarksItAsSaid()
        {
            live.Groups(1, 1, "1B06PH");
            live.StepStarted(RunSteps.TestsRun, -1.0);

            Assert.That(live.ShouldSay(), Is.True);

            live.Line(null);

            Assert.That(live.ShouldSay(), Is.False, "rendering it is what says it");
        }

        /// <summary>
        /// The pace comes from a reader set once, so every piece of the run that opens a
        /// step says so the same way without carrying the records around.
        /// </summary>
        [Test]
        public void ThePaceComesFromTheReaderWhereOneWasSupplied()
        {
            live.PaceReader = name => name == RunSteps.TestsRun ? 40.0 : -1.0;
            live.Groups(2, 14, "1B06BC");

            live.StepStarted(RunSteps.TestsRun);
            now = 81.0;
            Assert.That(live.SlowerThanLastGroup(), Is.True);

            live.StepStarted(RunSteps.Nwd);
            now = 9000.0;
            Assert.That(live.SlowerThanLastGroup(), Is.False, "the reader said it does not know");
        }

        /// <summary>
        /// A reader that throws says nothing about the pace and never stops a run. A line
        /// about how fast a step is going is a diagnostic like any other.
        /// </summary>
        [Test]
        public void APaceReaderThatThrowsSaysNothingRatherThanStoppingTheRun()
        {
            live.PaceReader = name => { throw new InvalidOperationException("no records"); };
            live.Groups(2, 14, "1B06BC");

            Assert.That(() => live.StepStarted(RunSteps.TestsRun), Throws.Nothing);

            now = 9000.0;
            Assert.That(live.SlowerThanLastGroup(), Is.False);
            Assert.That(live.Line(null), Does.Not.Contain("SLOWER"));
        }

        [Test]
        public void WithNoPaceReaderAStepStillStarts()
        {
            live.Groups(1, 1, "1B06PH");
            live.StepStarted(RunSteps.Nwd);
            now = 9000.0;

            Assert.That(live.Line(null), Does.Contain(RunSteps.Nwd));
            Assert.That(live.Line(null), Does.Not.Contain("SLOWER"));
        }

        [Test]
        public void AClockThatGoesBackwardsGivesNoNegativeSeconds()
        {
            live.Groups(1, 1, "1B06PH");
            now = 100.0;
            live.StepStarted(RunSteps.Nwd, -1.0);
            now = 10.0;

            Assert.That(live.SecondsOnStep(), Is.EqualTo(0.0));
            Assert.That(live.Line(null), Does.Contain("0s on this step"));
        }

        [Test]
        public void TheClockReadsInSecondsThenMinutesThenHours()
        {
            Assert.That(LiveLine.Clock(0.0), Is.EqualTo("0s"));
            Assert.That(LiveLine.Clock(59.9), Is.EqualTo("59s"));
            Assert.That(LiveLine.Clock(60.0), Is.EqualTo("1m 00s"));
            Assert.That(LiveLine.Clock(242.0), Is.EqualTo("4m 02s"));
            Assert.That(LiveLine.Clock(3661.0), Is.EqualTo("1h 01m 01s"));
            Assert.That(LiveLine.Clock(-5.0), Is.EqualTo("0s"));
        }

        [Test]
        public void TheTwiceIsASettingAndANonsenseValueIsRefused()
        {
            LiveLine.SlowerThan = 3.0;

            live.Groups(2, 14, "1B06BC");
            live.StepStarted(RunSteps.TestsRun, 10.0);
            now = 25.0;

            Assert.That(live.SlowerThanLastGroup(), Is.False, "over twice but not over three times");

            now = 31.0;
            Assert.That(live.SlowerThanLastGroup(), Is.True);

            Assert.That(() => LiveLine.SlowerThan = 1.0, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => LiveLine.SlowerThan = 0.5, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ALineWithNoClockAtAllIsRefusedWhereItIsBuilt()
        {
            Assert.That(() => new LiveLine(null), Throws.TypeOf<ArgumentNullException>());
        }

        // ---------- reading the pace off the records ----------

        private static StepRecord Step(string group, string name, double seconds)
        {
            return new StepRecord(group, name, 0, 0.0, seconds, false);
        }

        [Test]
        public void WhatTheSameStepTookOnAGroupIsAddedAcrossEveryVisit()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.TestsRun, 10.0),
                Step("1B06PH", RunSteps.TestsRun, 15.0),
                Step("1B06PH", RunSteps.Nwd, 60.0)
            };

            Assert.That(
                LiveLine.OnTheGroupBefore(records, "1B06PH", RunSteps.TestsRun),
                Is.EqualTo(25.0).Within(0.0001));
        }

        [Test]
        public void AStepThatGroupNeverRanIsMinusOneAndNotZero()
        {
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.Nwd, 60.0) };

            Assert.That(LiveLine.OnTheGroupBefore(records, "1B06PH", RunSteps.TestsRun), Is.EqualTo(-1.0));
            Assert.That(LiveLine.OnTheGroupBefore(records, "1B06BC", RunSteps.Nwd), Is.EqualTo(-1.0));
            Assert.That(LiveLine.OnTheGroupBefore(null, "1B06PH", RunSteps.Nwd), Is.EqualTo(-1.0));
        }

        [Test]
        public void TheGroupBeforeIsTheLastOneInTheRecordsThatIsNotThisOne()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.Nwd, 1.0),
                Step("1B06BC", RunSteps.Nwd, 1.0),
                Step("1B06G1", RunSteps.Nwd, 1.0)
            };

            Assert.That(LiveLine.TheGroupBefore(records, "1B06G1"), Is.EqualTo("1B06BC"));
            Assert.That(LiveLine.TheGroupBefore(new List<StepRecord>(), "1B06PH"), Is.Null);
        }
    }
}
