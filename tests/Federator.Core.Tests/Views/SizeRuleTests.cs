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

        /// <summary>
        /// The verdict a view's size split reads, F85 and F114: the LARGEST size property in
        /// millimetres, then over the threshold or not. SizeRule.Decide, the first property
        /// reading, had no caller in src and went with its tests in F114.
        /// </summary>
        private static SizeVerdict Verdict(IDictionary<string, double> read, string unit, SizeSettings settings)
        {
            return SizeRule.VerdictFor(SizeRule.LargestMillimetres(read, unit, settings), settings);
        }

        [Test]
        public void OverTheThresholdIsIn()
        {
            Assert.That(SizeRule.LargestMillimetres(Read("Diameter", 200.0), "Millimeters", Settings()), Is.EqualTo(200.0));
            Assert.That(Verdict(Read("Diameter", 200.0), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.Large));
        }

        [Test]
        public void UnderTheThresholdIsOut()
        {
            Assert.That(Verdict(Read("Diameter", 100.0), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.Small));
        }

        /// <summary>
        /// Over 150 is what was asked for, and 150 itself is not over it. This is the kind
        /// of boundary that gets read both ways, so it is pinned.
        /// </summary>
        [Test]
        public void ExactlyTheThresholdIsOutBecauseOverIsWhatWasAskedFor()
        {
            Assert.That(Verdict(Read("Diameter", 150.0), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.Small));
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
            Assert.That(SizeRule.LargestMillimetres(Read("Diameter", 0.5), "Feet", Settings()), Is.EqualTo(152.4).Within(0.001));
            Assert.That(Verdict(Read("Diameter", 0.5), "Feet", Settings()), Is.EqualTo(SizeVerdict.Large),
                "0.5ft is 152.4mm, which is over 150");
        }

        [Test]
        public void TheSameSizeInThreeUnitsGivesTheSameAnswer()
        {
            Assert.That(Verdict(Read("Diameter", 200.0), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.Large));
            Assert.That(Verdict(Read("Diameter", 20.0), "Centimeters", Settings()), Is.EqualTo(SizeVerdict.Large));
            Assert.That(Verdict(Read("Diameter", 0.2), "Meters", Settings()), Is.EqualTo(SizeVerdict.Large));
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
                () => SizeRule.LargestMillimetres(Read("Diameter", 200.0), "Furlongs", Settings()),
                Throws.Exception);
        }

        [Test]
        public void APropertyFurtherDownTheListIsUsedWhenTheEarlierOnesAreNotThere()
        {
            Assert.That(SizeRule.LargestMillimetres(Read("Overall Size", 300.0), "Millimeters", Settings()), Is.EqualTo(300.0));
            Assert.That(Verdict(Read("Overall Size", 300.0), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.Large));
        }

        // ---------- a size not read, the loud one ----------

        /// <summary>
        /// The break. A fitting with no size property is SizeUnknown, never Small, so it stays in
        /// its view and is named. Dropping it means a run quietly leaves real geometry out of
        /// the viewpoints and nothing in the output says so.
        /// </summary>
        [Test]
        public void AnItemWithNoSizePropertyIsUnknownAndNotSmall()
        {
            Assert.That(SizeRule.LargestMillimetres(Read("Material", 1.0), "Millimeters", Settings()), Is.Null);
            Assert.That(Verdict(Read("Material", 1.0), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.SizeUnknown),
                "nothing disappears because nobody could measure it");
        }

        [Test]
        public void NoPropertiesAtAllIsUnknownTheSameWay()
        {
            Assert.That(Verdict(new Dictionary<string, double>(), "Millimeters", Settings()), Is.EqualTo(SizeVerdict.SizeUnknown));
        }

        [Test]
        public void ANullLookupIsUnknownRatherThanThrowing()
        {
            Assert.That(Verdict(null, "Millimeters", Settings()), Is.EqualTo(SizeVerdict.SizeUnknown));
        }

        [Test]
        public void TheThresholdIsASettingAndChangingItChangesTheAnswer()
        {
            SizeSettings settings = Settings();
            settings.ThresholdMillimetres = 250.0;

            Assert.That(Verdict(Read("Diameter", 200.0), "Millimeters", settings), Is.EqualTo(SizeVerdict.Small));
        }

        [Test]
        public void ThePropertyNamesAreASettingAndOneOfOurOwnCanBeAdded()
        {
            SizeSettings settings = Settings();
            settings.PropertyNames = new List<string> { "Bore" };

            Assert.That(SizeRule.LargestMillimetres(Read("Bore", 200.0), "Millimeters", settings), Is.EqualTo(200.0));
            Assert.That(Verdict(Read("Bore", 200.0), "Millimeters", settings), Is.EqualTo(SizeVerdict.Large));
        }

        [Test]
        public void NullSettingsAreRefused()
        {
            Assert.That(
                () => SizeRule.LargestMillimetres(Read("Diameter", 1.0), "Millimeters", null),
                Throws.ArgumentNullException);
        }

        /// <summary>
        /// F85. The largest size of a clash side, already in millimetres, judged at the
        /// number, one past it and with no size at all. Exactly the threshold is Small,
        /// because over means over.
        /// </summary>
        [Test]
        public void TheVerdictForMillimetresIsSmallAtTheThresholdAndLargeOnePastIt()
        {
            SizeSettings settings = new SizeSettings();
            double at = settings.ThresholdMillimetres;

            Assert.That(SizeRule.VerdictFor(at, settings), Is.EqualTo(SizeVerdict.Small));
            Assert.That(SizeRule.VerdictFor(at + 0.001, settings), Is.EqualTo(SizeVerdict.Large));
            Assert.That(SizeRule.VerdictFor(at - 0.001, settings), Is.EqualTo(SizeVerdict.Small));
            Assert.That(SizeRule.VerdictFor(0, settings), Is.EqualTo(SizeVerdict.Small));
            Assert.That(SizeRule.VerdictFor(null, settings), Is.EqualTo(SizeVerdict.SizeUnknown));
            Assert.Throws<ArgumentNullException>(delegate { SizeRule.VerdictFor(1, null); });
        }
    }
}
