using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F76. The client's matrix is written at 25 mm and the NWFs on disk held tests at
    /// 75 mm. A test already in the document is left exactly as it is, so the run clashed
    /// at 75 mm while everybody believed it was clashing at 25, and there was no way to
    /// say which one was wanted because this tool has never had a tolerance setting.
    /// </summary>
    [TestFixture]
    public class ToleranceChoiceTests
    {
        [Test]
        public void TheDefaultChoosesNothingAndChangesNothing()
        {
            ToleranceChoice choice = ToleranceChoice.FromTheFile();

            Assert.That(choice.ChosenInTheTool, Is.False);
            Assert.That(choice.Label(), Is.EqualTo(ToleranceChoice.UseTheFile));
            Assert.That(choice.Origin, Is.EqualTo(ToleranceOrigin.File));
            Assert.That(choice.For(0.2460629921, "ft"), Is.EqualTo(0.2460629921),
                "the default leaves whatever the caller worked out exactly as it was");
        }

        [Test]
        public void TheDropDownOffersTheFileThenThreeValuesThenOther()
        {
            IList<string> choices = ToleranceChoice.Choices();

            Assert.That(choices.Count, Is.EqualTo(5));
            Assert.That(choices[0], Is.EqualTo("Use the value in the XML"));
            Assert.That(choices[1], Is.EqualTo("25 mm"));
            Assert.That(choices[2], Is.EqualTo("50 mm"));
            Assert.That(choices[3], Is.EqualTo("75 mm"));
            Assert.That(choices[4], Is.EqualTo("Other"));
        }

        /// <summary>The break. A chosen value wins over whatever the file or the document said.</summary>
        [Test]
        public void AChosenValueBeatsTheFileAndTheDocument()
        {
            ToleranceChoice choice = ToleranceChoice.Of(25.0);

            Assert.That(choice.ChosenInTheTool, Is.True);
            Assert.That(choice.Origin, Is.EqualTo(ToleranceOrigin.Tool));
            Assert.That(choice.For(0.2460629921, "mm"), Is.EqualTo(25.0),
                "the 75 mm the file asked for loses");
            Assert.That(choice.For(999.0, "mm"), Is.EqualTo(25.0),
                "and so does whatever the document had");
        }

        [Test]
        public void TheChosenValueIsConvertedIntoTheDocumentUnits()
        {
            ToleranceChoice choice = ToleranceChoice.Of(25.0);

            Assert.That(choice.InDocumentUnits("mm"), Is.EqualTo(25.0).Within(1e-9));
            Assert.That(choice.InDocumentUnits("m"), Is.EqualTo(0.025).Within(1e-9));
            Assert.That(choice.InDocumentUnits("ft"), Is.EqualTo(25.0 / 304.8).Within(1e-9));
        }

        /// <summary>
        /// F33's rule. A unit the table has not been taught throws rather than falling
        /// back, because a report in the wrong unit reads as real and is not.
        /// </summary>
        [Test]
        public void AUnitTheTableDoesNotKnowThrowsRatherThanGuessing()
        {
            ToleranceChoice choice = ToleranceChoice.Of(25.0);

            Assert.Throws<NotSupportedException>(delegate { choice.InDocumentUnits("furlongs"); });
        }

        [Test]
        public void AskingTheDefaultForAValueIsARefusalAndNotAZero()
        {
            ToleranceChoice choice = ToleranceChoice.FromTheFile();

            Assert.Throws<InvalidOperationException>(delegate { choice.InDocumentUnits("mm"); });
        }

        [Test]
        public void ZeroIsARealToleranceAndANegativeOneIsNot()
        {
            Assert.That(ToleranceChoice.Of(0.0).Millimetres, Is.EqualTo(0.0));
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ToleranceChoice.Of(-1.0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ToleranceChoice.Of(double.NaN); });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { ToleranceChoice.Of(double.PositiveInfinity); });
        }

        [Test]
        public void ATypedValueReadsInTheWordsAPersonSays()
        {
            Assert.That(ToleranceChoice.Of(37.5).Label(), Is.EqualTo("37.5 mm"));
            Assert.That(ToleranceChoice.Of(25.0).Label(), Is.EqualTo("25 mm"));
        }

        // ---------- what the log says ----------

        [Test]
        public void TheChosenLineSaysWhatItBeatAndBothCounts()
        {
            string line = ToleranceChoice.Of(25.0).LogLine(211, 1619, "ft");

            Assert.That(line, Does.StartWith("TOLERANCE 25 mm chosen in the tool"));
            Assert.That(line, Does.Contain("beats both the XML and the document"));
            Assert.That(line, Does.Contain("Set on 1830 tests"));
            Assert.That(line, Does.Contain("211 created fresh"));
            Assert.That(line, Does.Contain("1619 already in the document"));
            Assert.That(line, Does.Contain("0.082021 ft"));
        }

        [Test]
        public void TheDefaultLineSaysTheDocumentWasLeftAlone()
        {
            string line = ToleranceChoice.FromTheFile().LogLine(211, 1619, "ft");

            Assert.That(line, Does.Contain("read per test out of the XML"));
            Assert.That(line, Does.Contain("1619 left as the document has them"));
            Assert.That(line, Does.Not.Contain("beats"));
        }

        /// <summary>
        /// Logging never stops a run. A unit the table does not know fails the group where
        /// the tolerance is set, and it must not also throw out of a log line.
        /// </summary>
        [Test]
        public void AUnitTheTableDoesNotKnowStillWritesALine()
        {
            string line = ToleranceChoice.Of(25.0).LogLine(1, 0, "furlongs");

            Assert.That(line, Does.Contain("UNKNOWN"));
            Assert.That(line, Does.Contain("furlongs"));
        }

        // ---------- what the confirm screen says ----------

        [Test]
        public void ChoosingAValueWarnsWithTheCountsAndTheCost()
        {
            IList<string> lines = ToleranceChoice.Of(25.0).WarningLines(7, 1830);
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(all, Does.Contain("Clash tolerance: 25 mm, chosen in the tool."));
            Assert.That(all, Does.Contain("7 groups"));
            Assert.That(all, Does.Contain("1830 tests"));
            Assert.That(all, Does.Contain("already saved in each NWF"));
            Assert.That(all, Does.Contain("beats the tolerance in the XML and the tolerance in the document"));
            Assert.That(all, Does.Contain("resets that test's results"));
        }

        [Test]
        public void TheDefaultWarnsAboutNothing()
        {
            Assert.That(ToleranceChoice.FromTheFile().WarningLines(7, 1830), Is.Empty);
        }

        // ---------- where the report read it ----------

        /// <summary>
        /// The report is produced by the clash tests in the open document, so the number in
        /// the Tolerance cell has to be the one those tests carry. A count under any other
        /// origin is the report saying a number the run did not clash at.
        /// </summary>
        [Test]
        public void TheReportSaysWhereEveryToleranceWasRead()
        {
            string line = ToleranceChoice.ReadFromLine(1830, 0, 0, 0);

            Assert.That(line, Does.Contain("the clash test in the document for 1830"));
            Assert.That(line, Does.Contain("the clash XML for 0"));
            Assert.That(line, Does.Contain("chosen in the tool for 0"));
            Assert.That(line, Does.Contain("UNKNOWN for 0"));
            Assert.That(line, Does.Contain("1830 tests in all"));
        }

        [Test]
        public void AReportReadingTheFileRatherThanTheDocumentShowsAsANumber()
        {
            string line = ToleranceChoice.ReadFromLine(0, 1830, 0, 0);

            Assert.That(line, Does.Contain("the clash XML for 1830"));
            Assert.That(line, Does.Contain("the clash test in the document for 0"));
        }

        [Test]
        public void ATestReportStartsSayingItDoesNotKnowWhereItsToleranceCameFrom()
        {
            TestReport test = new TestReport(1, "a");

            Assert.That(test.ToleranceFrom, Is.EqualTo(ToleranceOrigin.Unknown),
                "UNKNOWN rather than a default that reads as a real answer");
            Assert.That(ToleranceChoice.Words(ToleranceOrigin.Unknown), Is.EqualTo("UNKNOWN"));
        }

        [Test]
        public void EveryOriginHasItsOwnWords()
        {
            Assert.That(ToleranceChoice.Words(ToleranceOrigin.Document),
                Is.EqualTo("the clash test in the document"));
            Assert.That(ToleranceChoice.Words(ToleranceOrigin.File), Is.EqualTo("the clash XML"));
            Assert.That(ToleranceChoice.Words(ToleranceOrigin.Tool), Is.EqualTo("chosen in the tool"));
            Assert.That(ToleranceChoice.Words((ToleranceOrigin)99), Is.EqualTo("UNKNOWN"));
        }

        [Test]
        public void TheHelpLineIsTwelveWordsAndCarriesNoCodeIdentifier()
        {
            string[] words = ToleranceChoice.HelpLine.Split(' ');

            Assert.That(words.Length, Is.LessThanOrEqualTo(12));
            Assert.That(ToleranceChoice.HelpLine, Does.Not.Contain("ApplyFileSettings"));
            Assert.That(ToleranceChoice.PickerLabel, Is.EqualTo("Clash tolerance"));
        }
    }
}
