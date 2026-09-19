using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The PENETRATION block, F72.
    ///
    /// The block is the only record of what a run changed inside the NWF, so every line of
    /// it is pinned here. The one that matters most is that a reason at ZERO is still
    /// printed: a reason missing from the block reads as a reason nobody thought of rather
    /// than one that did not come up, and the whole point of the block is that a run which
    /// moved nothing can be told apart from a run that was never asked to.
    /// </summary>
    [TestFixture]
    public class PenetrationTallyTests
    {
        private static PenetrationSettings Settings()
        {
            return new PenetrationSettings();
        }

        private static SizeSettings Sizes()
        {
            return new SizeSettings();
        }

        private static PenetrationDecision Decide(
            string serviceCategory, double? millimetres, string solidCategory, ClashStatus status)
        {
            return PenetrationRule.Decide(
                new PenetrationSide("a service", serviceCategory, millimetres),
                new PenetrationSide("a solid", solidCategory, null),
                status,
                Settings(),
                Sizes());
        }

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        [Test]
        public void ANewTallyHasCountedNothing()
        {
            PenetrationTally tally = new PenetrationTally();

            Assert.That(tally.Considered, Is.EqualTo(0));
            Assert.That(tally.MovedCount, Is.EqualTo(0));
            Assert.That(tally.MovedLines.Count, Is.EqualTo(0));
        }

        [Test]
        public void EveryClashLookedAtIsCountedWhetherItMovedOrNot()
        {
            PenetrationTally tally = new PenetrationTally();

            tally.Add("T1", "Clash1", Decide("Pipes", 100.0, "Walls", ClashStatus.New));
            tally.Add("T1", "Clash2", Decide("Pipes", 900.0, "Walls", ClashStatus.New));
            tally.Add("T1", "Clash3", Decide("Pipes", 100.0, "Pipes", ClashStatus.New));

            Assert.That(tally.Considered, Is.EqualTo(3));
            Assert.That(tally.MovedCount, Is.EqualTo(1));
        }

        [Test]
        public void EachReasonIsCountedOnItsOwn()
        {
            PenetrationTally tally = new PenetrationTally();

            tally.Add("T1", "C1", Decide("Pipes", 100.0, "Walls", ClashStatus.New));
            tally.Add("T1", "C2", Decide("Pipes", 900.0, "Walls", ClashStatus.New));
            tally.Add("T1", "C3", Decide("Pipes", null, "Walls", ClashStatus.New));
            tally.Add("T1", "C4", Decide("Pipes", 100.0, "Walls", ClashStatus.Approved));
            tally.Add("T1", "C5", Decide("Pipes", 100.0, "Ducts", ClashStatus.New));

            Assert.That(tally.Of(PenetrationVerdict.Reviewed), Is.EqualTo(1));
            Assert.That(tally.Of(PenetrationVerdict.ServiceTooLarge), Is.EqualTo(1));
            Assert.That(tally.Of(PenetrationVerdict.SizeUnknown), Is.EqualTo(1));
            Assert.That(tally.Of(PenetrationVerdict.AlreadyDecided), Is.EqualTo(1));
            Assert.That(tally.Of(PenetrationVerdict.BothService), Is.EqualTo(1));
            Assert.That(tally.Of(PenetrationVerdict.BothSolid), Is.EqualTo(0));
        }

        [Test]
        public void ADecisionThatIsNullIsRefusedRatherThanCounted()
        {
            PenetrationTally tally = new PenetrationTally();

            Assert.Throws<ArgumentNullException>(delegate { tally.Add("T1", "C1", null); });
        }

        // ---------- the lines ----------

        [Test]
        public void OneLinePerClashMovedNamingTheTestTheClashBothCategoriesAndTheSize()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("BLD-ME vs BLD-AR", "Clash7", Decide("Pipes", 100.0, "Walls", ClashStatus.New));

            string first = tally.MovedLines[0];

            Assert.That(first, Does.StartWith("REVIEWED "));
            Assert.That(first, Does.Contain("Clash7"));
            Assert.That(first, Does.Contain("BLD-ME vs BLD-AR"));
            Assert.That(first, Does.Contain("Pipes"));
            Assert.That(first, Does.Contain("Walls"));
            Assert.That(first, Does.Contain("100mm"));
        }

        [Test]
        public void NothingLeftAloneGetsALineOfItsOwn()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("T1", "C1", Decide("Pipes", 900.0, "Walls", ClashStatus.New));

            Assert.That(tally.MovedLines.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheBlockCountsWhatItLookedAtAndWhatItMoved()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("T1", "C1", Decide("Pipes", 100.0, "Walls", ClashStatus.New));
            tally.Add("T1", "C2", Decide("Pipes", 900.0, "Walls", ClashStatus.New));

            string block = Joined(tally.Lines(Settings(), Sizes()));

            Assert.That(block, Does.Contain("clashes looked at : 2"));
            Assert.That(block, Does.Contain("moved to Reviewed : 1"));
        }

        /// <summary>
        /// The one that matters. A reason at zero is still printed, because a reason
        /// missing from the block reads as one nobody thought of.
        /// </summary>
        [Test]
        public void EveryReasonIsPrintedIncludingTheOnesAtZero()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("T1", "C1", Decide("Pipes", 100.0, "Walls", ClashStatus.New));

            string block = Joined(tally.Lines(Settings(), Sizes()));

            foreach (PenetrationVerdict verdict in PenetrationRule.InOrder())
            {
                if (verdict == PenetrationVerdict.Reviewed)
                {
                    continue;
                }

                Assert.That(block, Does.Contain(PenetrationRule.Describe(verdict)));
            }
        }

        [Test]
        public void TheBlockSaysWhichSizeWasInUse()
        {
            string block = Joined(new PenetrationTally().Lines(Settings(), Sizes()));

            Assert.That(block, Does.Contain("the size in use   : 150mm or under"));
        }

        [Test]
        public void TheBlockReadsTheThresholdOffTheSettingsAndNotACopy()
        {
            SizeSettings sizes = Sizes();
            sizes.ThresholdMillimetres = 250.0;

            string block = Joined(new PenetrationTally().Lines(Settings(), sizes));

            Assert.That(block, Does.Contain("250mm"));
            Assert.That(block, Does.Not.Contain("150mm"));
        }

        [Test]
        public void ABlockThatMovedNothingSaysSoInWords()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("T1", "C1", Decide("Pipes", 900.0, "Walls", ClashStatus.New));

            string block = Joined(tally.Lines(Settings(), Sizes()));

            Assert.That(block, Does.Contain("Nothing moved. Every clash is exactly as it was."));
        }

        [Test]
        public void ABlockThatMovedSomethingDoesNotSayNothingMoved()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("T1", "C1", Decide("Pipes", 100.0, "Walls", ClashStatus.New));

            Assert.That(
                Joined(tally.Lines(Settings(), Sizes())),
                Does.Not.Contain("Nothing moved."));
        }

        [Test]
        public void AnEmptyBlockIsStillWritten()
        {
            IList<string> lines = new PenetrationTally().Lines(Settings(), Sizes());

            Assert.That(lines.Count, Is.GreaterThan(0));
            Assert.That(Joined(lines), Does.Contain("clashes looked at : 0"));
        }

        [Test]
        public void TheMovedLinesComeBeforeTheTotals()
        {
            PenetrationTally tally = new PenetrationTally();
            tally.Add("T1", "C1", Decide("Pipes", 100.0, "Walls", ClashStatus.New));

            IList<string> lines = tally.Lines(Settings(), Sizes());

            Assert.That(lines[0], Does.StartWith("REVIEWED "));
            Assert.That(lines[1], Is.EqualTo(string.Empty));
        }

        [Test]
        public void SettingsAreRequiredRatherThanGuessedAt()
        {
            PenetrationTally tally = new PenetrationTally();

            Assert.Throws<ArgumentNullException>(delegate { tally.Lines(null, Sizes()); });
            Assert.Throws<ArgumentNullException>(delegate { tally.Lines(Settings(), null); });
        }

        // ---------- the RESULT line ----------

        [Test]
        public void TheResultLineIsNullWhereNothingWasAskedFor()
        {
            Assert.That(PenetrationTally.ResultLine(false, 0), Is.Null);
            Assert.That(PenetrationTally.ResultLine(false, 9), Is.Null);
        }

        [Test]
        public void TheResultLineSaysZeroWhereTheBoxWasOnAndNothingMoved()
        {
            Assert.That(PenetrationTally.ResultLine(true, 0), Does.Contain("0 clashes moved to Reviewed"));
        }

        [Test]
        public void TheResultLineCountsOneClashInTheSingular()
        {
            Assert.That(PenetrationTally.ResultLine(true, 1), Does.Contain("1 clash moved"));
            Assert.That(PenetrationTally.ResultLine(true, 1), Does.Not.Contain("clashes"));
        }

        [Test]
        public void TheResultLineReadsLikeTheOtherResultLines()
        {
            Assert.That(PenetrationTally.ResultLine(true, 90), Does.StartWith("penetrations   : "));
        }
    }
}
