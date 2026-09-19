using System;
using System.Collections.Generic;
using System.Globalization;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Where the time went, per group and for the run.
    ///
    /// Every number in these blocks is measured off steps the run really opened and
    /// closed. The tests that matter most here are the ones about what the block does
    /// with time it cannot account for, because a block that quietly spread it over the
    /// steps it does know about would be inventing numbers, and a timing block nobody can
    /// trust is worse than none.
    /// </summary>
    [TestFixture]
    public class TimingBlockTests
    {
        [TearDown]
        public void PutTheThresholdBack()
        {
            TimingBlock.UnattendedSeconds = TimingBlock.DefaultUnattendedSeconds;
        }

        private static StepRecord Step(string group, string name, double seconds)
        {
            return new StepRecord(group, name, 0, 0.0, seconds, false);
        }

        private static StepRecord Nested(string group, string name, double seconds)
        {
            return new StepRecord(group, name, 1, 0.0, seconds, false);
        }

        private static string Find(IList<string> lines, string startingWith)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith(startingWith, StringComparison.Ordinal))
                {
                    return line;
                }
            }

            return null;
        }

        private static int IndexOfLineStarting(IList<string> lines, string startingWith)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(startingWith, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Every share on a row, read back off the text rather than recomputed.</summary>
        private static IList<double> Shares(IList<string> lines)
        {
            List<double> shares = new List<double>();

            foreach (string line in lines)
            {
                int percent = line.IndexOf('%');

                if (percent < 0)
                {
                    continue;
                }

                int start = percent;

                while (start > 0 && (char.IsDigit(line[start - 1]) || line[start - 1] == '.'))
                {
                    start--;
                }

                shares.Add(double.Parse(
                    line.Substring(start, percent - start), CultureInfo.InvariantCulture));
            }

            return shares;
        }

        // ---------- the group block ----------

        [Test]
        public void TheGroupBlockPutsTheSlowestStepFirst()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.Units, 1.0),
                Step("1B06PH", RunSteps.TestsRun, 80.0),
                Step("1B06PH", RunSteps.Nwd, 15.0)
            };

            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            Assert.That(lines[0], Does.StartWith(RunSteps.TestsRun));
            Assert.That(lines[1], Does.StartWith(RunSteps.Nwd));
            Assert.That(lines[2], Does.StartWith(RunSteps.Units));
        }

        [Test]
        public void EveryRowCarriesItsSecondsAndItsShare()
        {
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.Nwd, 25.0) };
            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            string row = Find(lines, RunSteps.Nwd);

            Assert.That(row, Is.Not.Null);
            Assert.That(row, Does.Contain("25.000s"));
            Assert.That(row, Does.Contain("25.0%"));
        }

        /// <summary>
        /// The one the whole block turns on. Every share in the column adds to a hundred,
        /// because whatever the steps do not account for is a ROW of its own rather than
        /// a number left off the page. A column that added to 78 would leave a reader
        /// guessing where the other 22 went, and guessing is the thing this log exists to
        /// stop.
        /// </summary>
        [Test]
        public void TheSharesAddToAHundred()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.Append, 30.0),
                Step("1B06PH", RunSteps.TestsRun, 45.0),
                Step("1B06PH", RunSteps.Workbook, 5.0)
            };

            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);
            IList<double> shares = Shares(lines);

            double total = 0.0;

            foreach (double share in shares)
            {
                total += share;
            }

            Assert.That(shares.Count, Is.EqualTo(4), "three steps and the row for the rest");
            Assert.That(total, Is.EqualTo(100.0).Within(0.05));
        }

        [Test]
        public void WhateverIsNotInAStepIsARowOfItsOwn()
        {
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.Append, 30.0) };
            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            string row = Find(lines, TimingBlock.OutsideEveryStep);

            Assert.That(row, Is.Not.Null);
            Assert.That(row, Does.Contain("70.000s"));
            Assert.That(row, Does.Contain("70.0%"));
        }

        [Test]
        public void AGroupWhoseStepsAccountForAllOfItSaysSo()
        {
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.Append, 100.0) };
            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            Assert.That(lines, Has.Some.Contains("every second of this group is inside a step"));
        }

        /// <summary>
        /// The break. Steps adding to more than the group took means something was timed
        /// outside the stretch the group clock covered, and the block says that in words
        /// rather than printing a share over a hundred and leaving it.
        /// </summary>
        [Test]
        public void StepsAddingToMoreThanTheGroupTookAreCalledOut()
        {
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.Append, 120.0) };
            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            Assert.That(lines, Has.Some.Contains("add up to MORE than this group took"));
            Assert.That(lines, Has.Some.Contains("20.000s"));
        }

        /// <summary>
        /// A nested step's seconds are already inside its parent's, so it is kept out of
        /// the shares and listed under a line saying so. Counting both would give a group
        /// shares adding to more than a hundred.
        /// </summary>
        [Test]
        public void ANestedStepIsListedApartAndLeftOutOfTheShares()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.Harvest, 60.0),
                Nested("1B06PH", RunSteps.Images, 50.0)
            };

            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            Assert.That(lines, Has.Some.Contains("already counted above"));

            int harvest = IndexOfLineStarting(lines, RunSteps.Harvest);
            int note = IndexOfLineStarting(lines, "inside another step");
            int images = IndexOfLineStarting(lines, RunSteps.Images);

            Assert.That(harvest, Is.GreaterThanOrEqualTo(0));
            Assert.That(images, Is.GreaterThan(note), "the nested step comes after the note");

            // 60 in a step and 40 outside, so the shares are 60 and 40 and IMAGES is in
            // neither, which is what keeps them adding to a hundred.
            string outside = Find(lines, TimingBlock.OutsideEveryStep);
            Assert.That(outside, Does.Contain("40.000s"));
        }

        [Test]
        public void AStepEnteredManyTimesCarriesItsVisits()
        {
            List<StepRecord> records = new List<StepRecord>();

            for (int i = 0; i < 1830; i++)
            {
                records.Add(Step("1B06PH", RunSteps.TestsRun, 0.4));
            }

            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 800.0);
            string row = Find(lines, RunSteps.TestsRun);

            Assert.That(row, Does.Contain("1830 visits"));
            Assert.That(row, Does.Contain("732.000s"));
        }

        [Test]
        public void AStepEnteredOnceCarriesNoVisitCount()
        {
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.Nwd, 10.0) };

            Assert.That(Find(TimingBlock.ForGroup("1B06PH", records, 100.0), RunSteps.Nwd),
                Does.Not.Contain("visits"));
        }

        [Test]
        public void AGroupWithNoStepAtAllStillWritesItsTotal()
        {
            IList<string> lines = TimingBlock.ForGroup("1B06PH", new List<StepRecord>(), 12.5);

            Assert.That(lines, Has.Some.Contains("no step was timed for this group"));
            Assert.That(Find(lines, "group total"), Does.Contain("12.500s"));
        }

        [Test]
        public void AnotherGroupsStepsAreNotInThisGroupsBlock()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.Nwd, 10.0),
                Step("1B06BC", RunSteps.Nwd, 90.0)
            };

            Assert.That(Find(TimingBlock.ForGroup("1B06PH", records, 100.0), RunSteps.Nwd),
                Does.Contain("10.000s"));
        }

        [Test]
        public void TwoStepsOfTheSameLengthReadInTheOrderAGroupMeetsThem()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.Nwd, 10.0),
                Step("1B06PH", RunSteps.Append, 10.0)
            };

            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            Assert.That(
                IndexOfLineStarting(lines, RunSteps.Append),
                Is.LessThan(IndexOfLineStarting(lines, RunSteps.Nwd)));
        }

        // ---------- the run block ----------

        private static GroupRecord Group(string building, double seconds)
        {
            return new GroupRecord(building, GroupOutcome.Done, null, "First run", seconds);
        }

        [Test]
        public void TheRunBlockReadsTheGroupsSlowestFirstAndThenTheStepsAcrossThem()
        {
            IList<GroupRecord> groups = new List<GroupRecord>
            {
                Group("1B06PH", 100.0),
                Group("1B06BC", 300.0)
            };

            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.TestsRun, 80.0),
                Step("1B06BC", RunSteps.TestsRun, 250.0),
                Step("1B06BC", RunSteps.Nwd, 40.0)
            };

            IList<string> lines = TimingBlock.ForRun(groups, records, 500.0);

            Assert.That(lines[0], Is.EqualTo("by group, slowest first"));
            Assert.That(lines[1], Does.StartWith("1B06BC"));
            Assert.That(lines[2], Does.StartWith("1B06PH"));

            int byStep = IndexOfLineStarting(lines, "by step across every group");
            Assert.That(byStep, Is.GreaterThan(0));

            // The same seconds read the other way round. Which BUILDING cost the run and
            // which STEP cost it are two questions and the second needs the steps added
            // across every group.
            string testsRun = Find(lines, RunSteps.TestsRun);
            Assert.That(testsRun, Does.Contain("330.000s"));
            Assert.That(testsRun, Does.Contain("2 visits"));
        }

        [Test]
        public void TheRunTotalIsTheMeasuredElapsedAndNotTheGroupsAddedUp()
        {
            IList<GroupRecord> groups = new List<GroupRecord> { Group("1B06PH", 100.0) };

            IList<string> lines = TimingBlock.ForRun(groups, new List<StepRecord>(), 460.0);

            Assert.That(Find(lines, "run total"), Does.Contain("460.000s"));

            // The scan and the preview happen outside every group, so the difference is a
            // row of its own rather than a number nobody accounts for.
            string outside = Find(lines, TimingBlock.OutsideEveryStep);
            Assert.That(outside, Does.Contain("360.000s"));
        }

        /// <summary>
        /// A run of one group. It is the ordinary case for every proof in
        /// steps/03_bader_next.md, so the block has to read properly with one row in it.
        /// </summary>
        [Test]
        public void ARunOfOneGroupStillReadsAsABlock()
        {
            IList<GroupRecord> groups = new List<GroupRecord> { Group("1B06PH", 90.0) };
            IList<StepRecord> records = new List<StepRecord> { Step("1B06PH", RunSteps.TestsRun, 80.0) };

            IList<string> lines = TimingBlock.ForRun(groups, records, 100.0);

            Assert.That(lines[1], Does.StartWith("1B06PH"));
            Assert.That(lines[1], Does.Contain("90.0%"));
            Assert.That(Find(lines, "run total"), Does.Contain("100.000s"));
            Assert.That(Find(lines, RunSteps.TestsRun), Does.Contain("80.000s"));
            Assert.That(lines, Has.Some.Contains("inside the 45 minutes 0 seconds"));
        }

        [Test]
        public void ARunWithNoGroupAtAllSaysSo()
        {
            IList<string> lines = TimingBlock.ForRun(new List<GroupRecord>(), new List<StepRecord>(), 3.0);

            Assert.That(lines, Has.Some.EqualTo("no group ran"));
            Assert.That(lines, Has.Some.EqualTo("no step was timed in this run"));
            Assert.That(Find(lines, "run total"), Does.Contain("3.000s"));
        }

        // ---------- the forty five minutes ----------

        [Test]
        public void ARunInsideTheTimeSaysSoAndSaysByHowMuch()
        {
            string said = TimingBlock.FittedInTheTime(40.0 * 60.0);

            Assert.That(said, Does.Contain("40 minutes 0 seconds"));
            Assert.That(said, Does.Contain("2400.000s"));
            Assert.That(said, Does.Contain("inside the 45 minutes 0 seconds"));
            Assert.That(said, Does.Contain("by 5 minutes 0 seconds"));
            Assert.That(said, Does.Not.Contain("OVER"));
        }

        /// <summary>
        /// The break, and the one that has to be loud. A run of 45 minutes and one second
        /// is OVER, because criterion 2 is a number and not a feeling.
        /// </summary>
        [Test]
        public void ARunOneSecondOverIsOver()
        {
            string said = TimingBlock.FittedInTheTime(45.0 * 60.0 + 1.0);

            Assert.That(said, Does.Contain("OVER"));
            Assert.That(said, Does.Contain("by 1 second"));
        }

        [Test]
        public void ExactlyTheTimeIsInsideIt()
        {
            Assert.That(TimingBlock.FittedInTheTime(45.0 * 60.0), Does.Not.Contain("OVER"));
        }

        [Test]
        public void AnHourLongRunReadsInHours()
        {
            string said = TimingBlock.FittedInTheTime(3661.0);

            Assert.That(said, Does.Contain("1 hour 1 minute 1 second"));
            Assert.That(said, Does.Contain("OVER"));
        }

        /// <summary>
        /// The forty five is a setting, because the number shapes what the run says about
        /// itself. A run asked to finish in no time at all is refused where it is set.
        /// </summary>
        [Test]
        public void TheFortyFiveIsASettingAndANonsenseValueIsRefused()
        {
            TimingBlock.UnattendedSeconds = 60.0;

            Assert.That(TimingBlock.FittedInTheTime(90.0), Does.Contain("OVER"));
            Assert.That(TimingBlock.FittedInTheTime(30.0), Does.Not.Contain("OVER"));

            Assert.That(() => TimingBlock.UnattendedSeconds = 0.0, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => TimingBlock.UnattendedSeconds = -1.0, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(TimingBlock.UnattendedSeconds, Is.EqualTo(60.0));
        }

        [Test]
        public void TheColumnIsWideEnoughForEveryLabelThatGoesInIt()
        {
            IList<StepRecord> records = new List<StepRecord>
            {
                Step("1B06PH", RunSteps.TestsCreate, 10.0)
            };

            IList<string> lines = TimingBlock.ForGroup("1B06PH", records, 100.0);

            // Every row that carries a number lines its number up in the same column, so
            // a block with a long label in it still reads as a table.
            List<int> at = new List<int>();

            foreach (string line in lines)
            {
                int percent = line.IndexOf('%');

                if (percent >= 0)
                {
                    at.Add(percent);
                }
            }

            Assert.That(at.Count, Is.GreaterThan(1));

            foreach (int where in at)
            {
                Assert.That(where, Is.EqualTo(at[0]), "every share sits in the same column");
            }
        }
    }
}
