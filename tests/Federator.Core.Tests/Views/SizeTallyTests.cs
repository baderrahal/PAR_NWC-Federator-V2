using System;
using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The SIZE block.
    ///
    /// The number this block exists for is how many items are in the viewpoint because
    /// nobody could measure them. It is the one most likely to say the rule is wrong on
    /// the first run, so it is never folded into another total and never left to be
    /// worked out by subtraction.
    /// </summary>
    [TestFixture]
    public class SizeTallyTests
    {
        private static SizeDecision Decide(IDictionary<string, double> read, SizeSettings settings)
        {
            return SizeRule.Decide(read, "Millimeters", settings);
        }

        private static IDictionary<string, double> Diameter(double mm)
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Diameter", mm);
            return read;
        }

        private static IDictionary<string, double> Nothing()
        {
            return new Dictionary<string, double>();
        }

        [Test]
        public void TheThreeCountsAreKeptApart()
        {
            SizeSettings settings = new SizeSettings();
            SizeTally tally = new SizeTally();

            tally.Add("big pipe", Decide(Diameter(200), settings));
            tally.Add("small pipe", Decide(Diameter(50), settings));
            tally.Add("a bend", Decide(Nothing(), settings));
            tally.Add("a tee", Decide(Nothing(), settings));

            Assert.That(tally.LargeCount, Is.EqualTo(1));
            Assert.That(tally.SmallCount, Is.EqualTo(1));
            Assert.That(tally.SizeUnknownCount, Is.EqualTo(2));
            Assert.That(tally.IncludedCount, Is.EqualTo(3), "large plus unmeasurable");
        }

        /// <summary>
        /// The loud line. A reader who stops at it must still know these items were kept
        /// rather than dropped.
        /// </summary>
        [Test]
        public void TheBlockSaysTheUnmeasurableOnesWereKeptAndNotDropped()
        {
            SizeSettings settings = new SizeSettings();
            SizeTally tally = new SizeTally();

            tally.Add("a bend", Decide(Nothing(), settings));

            IList<string> lines = tally.Lines(settings);
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(all, Does.Contain("because no size could be read"));
            Assert.That(all, Does.Contain("Nothing was dropped"));
            Assert.That(all, Does.Contain("expected to be large"));
        }

        [Test]
        public void EveryUnmeasurableItemIsNamedByDefault()
        {
            SizeSettings settings = new SizeSettings();
            SizeTally tally = new SizeTally();

            for (int i = 0; i < 12; i++)
            {
                tally.Add("fitting " + i, Decide(Nothing(), settings));
            }

            IList<string> lines = tally.Lines(settings);
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(settings.NameEveryUnknown, Is.True);
            Assert.That(all, Does.Contain("fitting 0"));
            Assert.That(all, Does.Contain("fitting 11"));
            Assert.That(all, Does.Not.Contain("more, not named"));
        }

        /// <summary>
        /// Turned off, the block must SAY it truncated. A truncated list that does not say
        /// so is the fault both this rule and the five examples rule exist to prevent.
        /// </summary>
        [Test]
        public void TurningOffEveryNameTruncatesAndSaysSo()
        {
            SizeSettings settings = new SizeSettings();
            settings.NameEveryUnknown = false;
            settings.ExamplesWhenNotNamingEvery = 3;

            SizeTally tally = new SizeTally();

            for (int i = 0; i < 12; i++)
            {
                tally.Add("fitting " + i, Decide(Nothing(), settings));
            }

            string all = string.Join("\n", new List<string>(tally.Lines(settings)).ToArray());

            Assert.That(all, Does.Contain("fitting 2"));
            Assert.That(all, Does.Not.Contain("fitting 3"));
            Assert.That(all, Does.Contain("and 9 more, not named because naming every one is switched off"));
        }

        [Test]
        public void EverythingMeasurableSaysSoRatherThanLeavingTheBlockLookingUnfinished()
        {
            SizeSettings settings = new SizeSettings();
            SizeTally tally = new SizeTally();

            tally.Add("big pipe", Decide(Diameter(200), settings));

            string all = string.Join("\n", new List<string>(tally.Lines(settings)).ToArray());

            Assert.That(all, Does.Contain("every item carried a size this tool could read"));
            Assert.That(all, Does.Not.Contain("Nothing was dropped"));
        }

        [Test]
        public void OneUnmeasurableItemReadsAsIsAndNotAsAre()
        {
            SizeSettings settings = new SizeSettings();
            SizeTally tally = new SizeTally();

            tally.Add("a bend", Decide(Nothing(), settings));

            string all = string.Join("\n", new List<string>(tally.Lines(settings)).ToArray());

            Assert.That(all, Does.Contain("1 item is in the viewpoint"));
            Assert.That(all, Does.Contain("read off it"));
        }

        [Test]
        public void AnItemWithNoNameIsStillNamedSomething()
        {
            SizeSettings settings = new SizeSettings();
            SizeTally tally = new SizeTally();

            tally.Add(null, Decide(Nothing(), settings));

            Assert.That(tally.SizeUnknownNames[0], Is.EqualTo("an item with no name"));
        }

        [Test]
        public void TheTotalsLineCarriesTheThresholdItUsed()
        {
            SizeSettings settings = new SizeSettings();
            settings.ThresholdMillimetres = 250;

            SizeTally tally = new SizeTally();
            tally.Add("big pipe", Decide(Diameter(300), settings));

            Assert.That(tally.Lines(settings)[0], Does.Contain("over 250mm"));
        }

        [Test]
        public void ANullDecisionOrNullSettingsAreRefused()
        {
            SizeTally tally = new SizeTally();

            Assert.That(() => tally.Add("x", null), Throws.ArgumentNullException);
            Assert.That(() => tally.Lines(null), Throws.ArgumentNullException);
        }
    }
}
