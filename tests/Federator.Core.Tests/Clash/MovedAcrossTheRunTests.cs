using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests.Clash
{
    /// <summary>
    /// How many clashes a run decided for him, per group and added up. It exists because
    /// the RESULT block carried one number per rule and no breakdown, and a number on its
    /// own cannot be judged: 7 of 14 and 7 of 7000 are the same 7 and mean the opposite.
    /// The fixture of 2026-09-21 moved 7 of 14 under the penetration rule and 21 of 27
    /// under the by design rule.
    /// </summary>
    [TestFixture]
    public class MovedAcrossTheRunTests
    {
        private static MovedAcrossTheRun ARun()
        {
            return new MovedAcrossTheRun("moved to Reviewed", "penetrations   ");
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        [Test]
        public void TheTotalIsEveryGroupAddedUpAndSoIsWhatWasLookedAt()
        {
            MovedAcrossTheRun run = ARun();
            run.Add("1A04WM", 7, 14);
            run.Add("1A04WE", 0, 27);

            Assert.That(run.Groups, Is.EqualTo(2));
            Assert.That(run.Total, Is.EqualTo(7));
            Assert.That(run.TotalLookedAt, Is.EqualTo(41));
        }

        /// <summary>
        /// THE RATE IS THE POINT. The line carries what was looked at beside what moved,
        /// because a person signing a report off needs to know whether the tool decided
        /// half of them or a thousandth.
        /// </summary>
        [Test]
        public void TheLineCarriesWhatWasLookedAtBesideWhatMoved()
        {
            MovedAcrossTheRun run = ARun();
            run.Add("1A04WM", 7, 14);

            string block = Joined(run.ResultLines(true));

            Assert.That(block, Does.Contain("7 clash(es) moved to Reviewed"));
            Assert.That(block, Does.Contain("of 14 looked at"));
            Assert.That(block, Does.Contain("1A04WM"));
        }

        /// <summary>
        /// A RULE THAT WAS NEVER ASKED FOR SAYS NOTHING AT ALL, which is the rule the
        /// priority line already keeps: a line reading zero on every run teaches people to
        /// skip the block.
        /// </summary>
        [Test]
        public void ARuleThatWasNotAskedForWritesNothing()
        {
            MovedAcrossTheRun run = ARun();
            run.Add("1A04WM", 7, 14);

            Assert.That(run.ResultLines(false), Is.Empty);
        }

        /// <summary>
        /// A run that moved nothing says so in ONE line rather than twelve rows of zero,
        /// because a block nobody can read is a block nobody reads.
        /// </summary>
        [Test]
        public void ARunThatMovedNothingWritesNoPerGroupRows()
        {
            MovedAcrossTheRun run = ARun();
            run.Add("a", 0, 27);
            run.Add("b", 0, 13);

            IList<string> lines = run.ResultLines(true);

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.Contain("0 clash(es) moved to Reviewed, of 40 looked at"));
        }

        /// <summary>A group that moved nothing is left out of the rows while a group that moved is in.</summary>
        [Test]
        public void OnlyTheGroupsThatMovedSomethingGetARow()
        {
            MovedAcrossTheRun run = ARun();
            run.Add("quiet", 0, 100);
            run.Add("busy", 5, 10);

            string block = Joined(run.ResultLines(true));

            Assert.That(block, Does.Contain("busy"));
            Assert.That(block, Does.Not.Contain("quiet"));
        }

        /// <summary>
        /// ASKED FOR AND NOBODY COUNTED IS UNKNOWN AND NEVER ZERO, the rule a census
        /// count already keeps. A rule that ran and reported nothing is not a rule that
        /// moved nothing.
        /// </summary>
        [Test]
        public void AskedForWithNoGroupReportingSaysUnknownAndNeverZero()
        {
            string block = Joined(ARun().ResultLines(true));

            Assert.That(block, Does.Contain("UNKNOWN"));
            Assert.That(block, Does.Not.Contain("0 clash(es)"));
        }

        [Test]
        public void ANegativeCountIsReadAsZeroAndNeverSubtracted()
        {
            MovedAcrossTheRun run = ARun();
            run.Add("a", -3, -9);
            run.Add("b", 4, 8);

            Assert.That(run.Total, Is.EqualTo(4));
            Assert.That(run.TotalLookedAt, Is.EqualTo(8));
        }

        [Test]
        public void AGroupWithNoNameIsSaidAsUnknownRatherThanBlank()
        {
            MovedAcrossTheRun run = ARun();
            run.Add(null, 2, 4);

            Assert.That(Joined(run.ResultLines(true)), Does.Contain("UNKNOWN"));
        }

        /// <summary>The same class serves both rules, so the by design label reads as its own.</summary>
        [Test]
        public void TheSameCountingServesTheByDesignRuleWithItsOwnLabel()
        {
            MovedAcrossTheRun run = new MovedAcrossTheRun("moved to Reviewed", "by design      ");
            run.Add("1A04WE", 21, 27);

            Assert.That(Joined(run.ResultLines(true)), Does.StartWith("by design      : 21 clash(es)"));
        }
    }
}
