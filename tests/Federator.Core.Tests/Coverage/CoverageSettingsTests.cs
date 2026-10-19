using System;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Every number that shapes the coverage is a setting, F127, turn5\f127-design.md section
    /// 1.1, and a value that cannot work is refused where it is set rather than reaching a
    /// run. Nought on a count of lines means every one, the way the image cap and the
    /// viewpoint cap read nought.
    /// </summary>
    [TestFixture]
    public class CoverageSettingsTests
    {
        [Test]
        public void TheDefaults()
        {
            CoverageSettings settings = new CoverageSettings();

            Assert.That(settings.ExamplesPerReason, Is.EqualTo(RunLog.KeptOfARepeat));
            Assert.That(settings.CategoriesNamedPerModel, Is.EqualTo(10));
            Assert.That(settings.FailedLinesInResult, Is.EqualTo(0), "every FAILED line is in RESULT, his words");
            Assert.That(settings.SetsAtZeroNamedInTheRun, Is.EqualTo(0), "every set that found nothing is named");
        }

        [Test]
        public void ExamplesBelowOneAreRefused()
        {
            CoverageSettings settings = new CoverageSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.ExamplesPerReason = 0);
            settings.ExamplesPerReason = 1;
            Assert.That(settings.ExamplesPerReason, Is.EqualTo(1));
        }

        [Test]
        public void CategoriesBelowOneAreRefused()
        {
            CoverageSettings settings = new CoverageSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.CategoriesNamedPerModel = 0);
            settings.CategoriesNamedPerModel = 1;
            Assert.That(settings.CategoriesNamedPerModel, Is.EqualTo(1));
        }

        [Test]
        public void FailedLinesBelowNoughtAreRefused()
        {
            CoverageSettings settings = new CoverageSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.FailedLinesInResult = -1);
            settings.FailedLinesInResult = 3;
            Assert.That(settings.FailedLinesInResult, Is.EqualTo(3));
        }

        [Test]
        public void SetsNamedBelowNoughtAreRefused()
        {
            CoverageSettings settings = new CoverageSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.SetsAtZeroNamedInTheRun = -1);
            settings.SetsAtZeroNamedInTheRun = 10;
            Assert.That(settings.SetsAtZeroNamedInTheRun, Is.EqualTo(10));
        }
    }
}
