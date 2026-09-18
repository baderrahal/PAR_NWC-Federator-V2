using System;
using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The 150 mm rule.
    ///
    /// Two things here are worth more than the rest. The unit conversion, because the same
    /// building measured in feet and in millimetres must give the same answer. And include
    /// on unknown, because the alternative is a run quietly leaving real geometry out of
    /// the viewpoints with nothing in the output to say it happened.
    /// </summary>
    [TestFixture]
    public class SizeRuleTests
    {
        private static IDictionary<string, double> Read(string name, double value)
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add(name, value);
            return read;
        }

        private static SizeSettings Settings()
        {
            return new SizeSettings();
        }

        [Test]
        public void TheDefaultThresholdIs150Millimetres()
        {
            Assert.That(new SizeSettings().ThresholdMillimetres, Is.EqualTo(150.0));
            Assert.That(SizeSettings.DefaultThresholdMillimetres, Is.EqualTo(150.0));
        }

        [Test]
        public void TheDefaultPropertyNamesAreTheSixInTheOrderGiven()
        {
            Assert.That(
                new SizeSettings().PropertyNames,
                Is.EqualTo(new[] { "Diameter", "Width", "Height", "Size", "Nominal Diameter", "Overall Size" })
                    .AsCollection);
        }

        [Test]
        public void OverTheThresholdIsIn()
        {
            SizeDecision decided = SizeRule.Decide(Read("Diameter", 200.0), "Millimeters", Settings());

            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Large));
            Assert.That(decided.Included, Is.True);
            Assert.That(decided.MatchedProperty, Is.EqualTo("Diameter"));
            Assert.That(decided.Millimetres, Is.EqualTo(200.0));
            Assert.That(decided.Reason, Does.Contain("over 150mm"));
        }

        [Test]
        public void UnderTheThresholdIsOut()
        {
            SizeDecision decided = SizeRule.Decide(Read("Diameter", 100.0), "Millimeters", Settings());

            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Small));
            Assert.That(decided.Included, Is.False);
            Assert.That(decided.Reason, Does.Contain("not over 150mm"));
        }

        /// <summary>
        /// Over 150 is what was asked for, and 150 itself is not over it. This is the kind
        /// of boundary that gets read both ways, so it is pinned.
        /// </summary>
        [Test]
        public void ExactlyTheThresholdIsOutBecauseOverIsWhatWasAskedFor()
        {
            SizeDecision decided = SizeRule.Decide(Read("Diameter", 150.0), "Millimeters", Settings());

            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Small));
            Assert.That(decided.Included, Is.False);
        }

        /// <summary>
        /// The one that matters most. A document in feet reporting 0.5 is 152.4 mm and is
        /// IN. Comparing the raw 0.5 against 150 would put it out, and the same model in
        /// millimetres would put it in, so one building would give two different sets of
        /// viewpoints depending on a setting nobody changed.
        /// </summary>
        [Test]
        public void AFeetDocumentIsConvertedBeforeAnythingIsCompared()
        {
            SizeDecision decided = SizeRule.Decide(Read("Diameter", 0.5), "Feet", Settings());

            Assert.That(decided.Millimetres, Is.EqualTo(152.4).Within(0.001));
            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Large), "0.5ft is 152.4mm, which is over 150");
            Assert.That(decided.Reason, Does.Contain("152.4mm"));
        }

        [Test]
        public void TheSameSizeInThreeUnitsGivesTheSameAnswer()
        {
            Assert.That(
                SizeRule.Decide(Read("Diameter", 200.0), "Millimeters", Settings()).Verdict,
                Is.EqualTo(SizeVerdict.Large));

            Assert.That(
                SizeRule.Decide(Read("Diameter", 20.0), "Centimeters", Settings()).Verdict,
                Is.EqualTo(SizeVerdict.Large));

            Assert.That(
                SizeRule.Decide(Read("Diameter", 0.2), "Meters", Settings()).Verdict,
                Is.EqualTo(SizeVerdict.Large));
        }

        /// <summary>
        /// A unit the table does not know FAILS rather than falling back, which is F33's
        /// rule. Guessing the factor would build a viewpoint holding the wrong items that
        /// looks exactly like one holding the right ones.
        /// </summary>
        [Test]
        public void AUnitTheTableDoesNotKnowThrowsRatherThanGuessing()
        {
            Assert.That(
                () => SizeRule.Decide(Read("Diameter", 200.0), "Furlongs", Settings()),
                Throws.Exception);
        }

        [Test]
        public void ThePropertiesAreTriedInOrderAndTheFirstFoundWins()
        {
            Dictionary<string, double> read = new Dictionary<string, double>();
            read.Add("Width", 100.0);
            read.Add("Diameter", 200.0);

            SizeDecision decided = SizeRule.Decide(read, "Millimeters", Settings());

            Assert.That(decided.MatchedProperty, Is.EqualTo("Diameter"), "Diameter is first in the list");
            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Large));
        }

        [Test]
        public void APropertyFurtherDownTheListIsUsedWhenTheEarlierOnesAreNotThere()
        {
            SizeDecision decided = SizeRule.Decide(Read("Overall Size", 300.0), "Millimeters", Settings());

            Assert.That(decided.MatchedProperty, Is.EqualTo("Overall Size"));
            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Large));
        }

        // ---------- include on unknown, the loud one ----------

        /// <summary>
        /// The break. A fitting with no size property must be IN. Dropping it means a run
        /// quietly leaves real geometry out of the viewpoints and nothing in the output
        /// says so.
        /// </summary>
        [Test]
        public void AnItemWithNoSizePropertyIsIncludedAndNotDropped()
        {
            SizeDecision decided = SizeRule.Decide(Read("Material", 1.0), "Millimeters", Settings());

            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.SizeUnknown));
            Assert.That(decided.Included, Is.True, "nothing disappears because nobody could measure it");
            Assert.That(decided.MatchedProperty, Is.Null);
            Assert.That(decided.Millimetres, Is.Null);
            Assert.That(decided.Reason, Does.Contain("is IN"));
        }

        [Test]
        public void NoPropertiesAtAllIsIncludedTheSameWay()
        {
            SizeDecision decided = SizeRule.Decide(
                new Dictionary<string, double>(), "Millimeters", Settings());

            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.SizeUnknown));
            Assert.That(decided.Included, Is.True);
        }

        [Test]
        public void ANullLookupIsIncludedRatherThanThrowing()
        {
            SizeDecision decided = SizeRule.Decide(null, "Millimeters", Settings());

            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.SizeUnknown));
            Assert.That(decided.Included, Is.True);
        }

        [Test]
        public void TheReasonNamesThePropertiesItLookedForSoAMissingNameIsVisible()
        {
            SizeDecision decided = SizeRule.Decide(Read("Material", 1.0), "Millimeters", Settings());

            Assert.That(decided.Reason, Does.Contain("Diameter"));
            Assert.That(decided.Reason, Does.Contain("Overall Size"));
        }

        [Test]
        public void TheThresholdIsASettingAndChangingItChangesTheAnswer()
        {
            SizeSettings settings = Settings();
            settings.ThresholdMillimetres = 250.0;

            Assert.That(
                SizeRule.Decide(Read("Diameter", 200.0), "Millimeters", settings).Verdict,
                Is.EqualTo(SizeVerdict.Small));
        }

        [Test]
        public void ThePropertyNamesAreASettingAndOneOfOurOwnCanBeAdded()
        {
            SizeSettings settings = Settings();
            settings.PropertyNames = new List<string> { "Bore" };

            SizeDecision decided = SizeRule.Decide(Read("Bore", 200.0), "Millimeters", settings);

            Assert.That(decided.MatchedProperty, Is.EqualTo("Bore"));
            Assert.That(decided.Verdict, Is.EqualTo(SizeVerdict.Large));
        }

        [Test]
        public void NullSettingsAreRefused()
        {
            Assert.That(
                () => SizeRule.Decide(Read("Diameter", 1.0), "Millimeters", null),
                Throws.ArgumentNullException);
        }
    }
}
