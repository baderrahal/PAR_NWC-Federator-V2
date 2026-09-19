using System;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the census costs, and the rule that narrows it when it is not free.
    ///
    /// The log must never change what the run does. Counting five things around fourteen
    /// steps of twenty two groups is real work, and a log that slows the thing it is
    /// watching is a log that has made the run worse. So the cost is measured and said,
    /// and over the threshold the census narrows and says that too.
    /// </summary>
    [TestFixture]
    public class CensusCostTests
    {
        [TearDown]
        public void PutTheThresholdBack()
        {
            CensusCost.TooLongSeconds = CensusCost.DefaultTooLongSeconds;
        }

        [Test]
        public void ACheapGroupSaysWhatItCostAndStaysWide()
        {
            CensusCost cost = new CensusCost();

            for (int i = 0; i < 28; i++)
            {
                cost.Took(0.004);
            }

            Assert.That(cost.Taken, Is.EqualTo(28));
            Assert.That(cost.Seconds, Is.EqualTo(0.112).Within(0.0005));
            Assert.That(cost.TooExpensive, Is.False);
            Assert.That(cost.Line(), Does.Contain("cost 0.112s over 28 counts"));
            Assert.That(cost.Line(), Does.Contain("inside the 1.000s a group is allowed"));
        }

        /// <summary>
        /// The break. A group that spent over its second counting says so and says what
        /// happens next, rather than quietly carrying on costing the run that time.
        /// </summary>
        [Test]
        public void AnExpensiveGroupSaysSoAndSaysWhatItWillDoNext()
        {
            CensusCost cost = new CensusCost();
            cost.Took(1.5);

            Assert.That(cost.TooExpensive, Is.True);
            Assert.That(cost.Line(), Does.Contain("over the 1.000s a group is allowed"));
            Assert.That(cost.Line(), Does.Contain("only around the steps that may write"));
        }

        [Test]
        public void ExactlyTheThresholdIsNotTooExpensive()
        {
            CensusCost cost = new CensusCost();
            cost.Took(1.0);

            Assert.That(cost.TooExpensive, Is.False);
        }

        [Test]
        public void ACountThatReadsAsNegativeTakesNoTimeOffTheTotal()
        {
            CensusCost cost = new CensusCost();
            cost.Took(0.5);
            cost.Took(-90.0);

            Assert.That(cost.Seconds, Is.EqualTo(0.5).Within(0.0001));
            Assert.That(cost.Taken, Is.EqualTo(2), "it still happened, it just took no time");
        }

        [Test]
        public void EachGroupCountsFromNothing()
        {
            CensusCost cost = new CensusCost();
            cost.Took(2.0);
            cost.NextGroup();

            Assert.That(cost.Seconds, Is.EqualTo(0.0));
            Assert.That(cost.Taken, Is.EqualTo(0));
            Assert.That(cost.TooExpensive, Is.False);
        }

        [Test]
        public void TheNarrowedLineNamesEveryStepTheCensusStillCovers()
        {
            string line = CensusCost.NarrowedLine();

            Assert.That(line, Does.StartWith("CENSUS   narrowed"));
            Assert.That(line, Does.Contain(RunSteps.Decide));
            Assert.That(line, Does.Contain(RunSteps.Append));
            Assert.That(line, Does.Contain(RunSteps.Sets));
            Assert.That(line, Does.Contain(RunSteps.TestsCreate));
            Assert.That(line, Does.Contain(RunSteps.TestsRun));
            Assert.That(line, Does.Not.Contain(RunSteps.Nwd));
        }

        [Test]
        public void OneCountReadsInTheSingular()
        {
            CensusCost cost = new CensusCost();
            cost.Took(0.01);

            Assert.That(cost.Line(), Does.Contain("over 1 count in this group"));
        }

        [Test]
        public void TheThresholdIsASettingAndANonsenseValueIsRefused()
        {
            CensusCost.TooLongSeconds = 5.0;

            CensusCost cost = new CensusCost();
            cost.Took(3.0);

            Assert.That(cost.TooExpensive, Is.False);
            Assert.That(cost.Line(), Does.Contain("inside the 5.000s"));

            Assert.That(() => CensusCost.TooLongSeconds = 0.0, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => CensusCost.TooLongSeconds = -1.0, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(CensusCost.TooLongSeconds, Is.EqualTo(5.0));
        }
    }
}
