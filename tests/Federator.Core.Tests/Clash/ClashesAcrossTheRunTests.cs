using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests.Clash
{
    /// <summary>
    /// The run total for clashes, which the RESULT block carried no version of until the
    /// drift round. The brief that opened the round said it plainly: the last two round
    /// reports carried no run total for clashes at all, because there was none in the log
    /// to carry.
    /// </summary>
    [TestFixture]
    public class ClashesAcrossTheRunTests
    {
        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        [Test]
        public void TheTotalIsEveryGroupAddedUp()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add("1A02BS", 109, 11);
            run.Add("1A02MM", 542, 57);
            run.Add("1A02WE", 29, 14);

            Assert.That(run.Groups, Is.EqualTo(3));
            Assert.That(run.Total, Is.EqualTo(680));
        }

        [Test]
        public void TheLineNamesTheTotalAndEveryGroupUnderIt()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add("1A02BS", 109, 11);
            run.Add("1A02MM", 542, 57);

            string block = Joined(run.ResultLines());

            Assert.That(block, Does.Contain("clashes found  : 651 across 2 groups"));
            Assert.That(block, Does.Contain("1A02MM"));
            Assert.That(block, Does.Contain("542"));
            Assert.That(block, Does.Contain("from 57 test(s) that found something"));
        }

        /// <summary>
        /// Groups at zero are counted apart, because none is not the same as few and a
        /// run where half the groups found nothing is a run with something wrong in it.
        /// </summary>
        [Test]
        public void GroupsThatFoundNothingAreCountedApart()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add("1000BS", 0, 0);
            run.Add("1A0215", 0, 0);
            run.Add("1A02BS", 109, 11);

            Assert.That(run.GroupsThatFoundNothing, Is.EqualTo(2));
            Assert.That(Joined(run.ResultLines()), Does.Contain("2 of which found none"));
        }

        [Test]
        public void ARunWhereEveryGroupFoundSomethingSaysNothingAboutGroupsAtZero()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add("1A02BS", 109, 11);

            Assert.That(Joined(run.ResultLines()), Does.Not.Contain("found none"));
        }

        /// <summary>
        /// A RUN THAT FOUND NOTHING STILL CARRIES THE LINE. This is the one place where
        /// this tally deliberately breaks the rule the priority and penetration lines
        /// keep, which is that a line reading zero on every run teaches people to skip
        /// the block. A run that found no clashes is the run whose number matters most.
        /// </summary>
        [Test]
        public void ARunThatFoundNoClashesAtAllStillSaysSo()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add("1000BS", 0, 0);

            string block = Joined(run.ResultLines());

            Assert.That(block, Does.Contain("clashes found  : 0 across 1 group"));
            Assert.That(block, Does.Contain("1 of which found none"));
        }

        /// <summary>
        /// No group reported a count at all, which is not the same as a run that found
        /// nothing, so it says UNKNOWN rather than zero. That is the rule a viewpoint
        /// count already keeps: not counted is not the same as none.
        /// </summary>
        [Test]
        public void NoGroupReportingACountSaysUnknownAndNeverZero()
        {
            string block = Joined(new ClashesAcrossTheRun().ResultLines());

            Assert.That(block, Does.Contain("UNKNOWN"));
            Assert.That(block, Does.Not.Contain(": 0 across"));
        }

        [Test]
        public void AGroupWithNoNameIsSaidAsUnknownRatherThanBlank()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add(null, 5, 1);

            Assert.That(Joined(run.ResultLines()), Does.Contain("UNKNOWN"));
        }

        /// <summary>A negative count is nonsense from a caller and is never added into the total.</summary>
        [Test]
        public void ANegativeCountIsReadAsZeroAndNeverSubtracted()
        {
            ClashesAcrossTheRun run = new ClashesAcrossTheRun();
            run.Add("a", -4, -1);
            run.Add("b", 10, 2);

            Assert.That(run.Total, Is.EqualTo(10));
        }
    }
}
