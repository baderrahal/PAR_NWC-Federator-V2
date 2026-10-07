using System;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
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

            Assert.That(settings.SheetName, Is.EqualTo("Coverage"), "Bader's own word for the sheet");
            Assert.That(settings.ExamplesPerReason, Is.EqualTo(RunLog.KeptOfARepeat));
            Assert.That(settings.CategoriesNamedPerModel, Is.EqualTo(10));
            Assert.That(settings.FailedLinesInResult, Is.EqualTo(0), "every FAILED line is in RESULT, his words");
            Assert.That(settings.SetsAtZeroNamedInTheRun, Is.EqualTo(0), "every set that found nothing is named");
        }

        [Test]
        public void TheDefaultSheetNameIsOneExcelAccepts()
        {
            Assert.That(SheetNames.IsAcceptable(new CoverageSettings().SheetName), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Cover:age")]
        [TestCase("Cover/age")]
        [TestCase("'Coverage")]
        [TestCase("Coverage of every test of the XML")]
        public void ASheetNameExcelRefusesIsRefusedWhereItIsSet(string name)
        {
            CoverageSettings settings = new CoverageSettings();

            Assert.Throws<ArgumentException>(() => settings.SheetName = name);
            Assert.That(settings.SheetName, Is.EqualTo("Coverage"), "a refused name changed the setting");
        }

        [Test]
        public void ASheetNameExcelAcceptsIsKept()
        {
            CoverageSettings settings = new CoverageSettings();
            settings.SheetName = "Tests covered";

            Assert.That(settings.SheetName, Is.EqualTo("Tests covered"));
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
