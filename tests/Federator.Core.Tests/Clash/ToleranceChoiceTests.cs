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
        /// <summary>
        /// F155, Bader's decision of 2026-10-13: the drop down is the only source of a run's tolerance and offers 25,
        /// 50 and 75 mm and nothing else. The break: an entry for the XML or for a typed value back on the list fails.
        /// </summary>
        [Test]
        public void TheDropDownOffersTwentyFiveFiftyAndSeventyFiveAndNothingElse()
        {
            IList<string> choices = ToleranceChoice.Choices();

            Assert.That(choices.Count, Is.EqualTo(3));
            Assert.That(choices[0], Is.EqualTo("25 mm"));
            Assert.That(choices[1], Is.EqualTo("50 mm"));
            Assert.That(choices[2], Is.EqualTo("75 mm"));
            Assert.That(choices, Does.Not.Contain("Use the value in the XML"));
            Assert.That(choices, Does.Not.Contain("Other"));
        }

        /// <summary>25 mm is selected when the window opens, and every button that runs no test hands it on.</summary>
        [Test]
        public void TwentyFiveIsTheDefaultAndTheFirstEntry()
        {
            Assert.That(ToleranceChoice.Default().Millimetres, Is.EqualTo(25.0));
            Assert.That(Array.IndexOf(ToleranceChoice.OfferedMillimetres, ToleranceChoice.DefaultMillimetres), Is.EqualTo(0));
            Assert.That(new ReportOptions().Tolerance.Millimetres, Is.EqualTo(25.0));
        }

        /// <summary>
        /// The entry for the XML's value, the entry Other and what only served them are gone, so the XML's tolerance can
        /// never be chosen again and no typed text is parsed.
        /// </summary>
        [Test]
        public void TheXmlEntryOtherAndTheTypedTextAreGone()
        {
            Type type = typeof(ToleranceChoice);

            Assert.That(type.GetField("UseTheFile"), Is.Null);
            Assert.That(type.GetField("Other"), Is.Null);
            Assert.That(type.GetField("OtherIsBlank"), Is.Null);
            Assert.That(type.GetMethod("FromTheFile"), Is.Null);
            Assert.That(type.GetProperty("ChosenInTheTool"), Is.Null);
            Assert.That(type.GetMethod("FromThePicker").GetParameters().Length, Is.EqualTo(2), "the index and the refusal, no text");
        }

        /// <summary>The grey line says what the box does in twelve words at most and names no XML.</summary>
        [Test]
        public void TheGreyLineIsTwelveWordsAtMostAndNamesNoXml()
        {
            Assert.That(ToleranceChoice.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
            Assert.That(ToleranceChoice.HelpLine, Does.Not.Contain("XML"));
            Assert.That(ToleranceChoice.PickerLabel.Split(' ').Length, Is.LessThanOrEqualTo(8));
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
        public void ZeroIsARealToleranceAndANegativeOneIsNot()
        {
            Assert.That(ToleranceChoice.Of(0.0).Millimetres, Is.EqualTo(0.0));
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ToleranceChoice.Of(-1.0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { ToleranceChoice.Of(double.NaN); });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { ToleranceChoice.Of(double.PositiveInfinity); });
        }

        // ---------- the drop down read without a throw, FR-166 ----------

        [Test]
        public void EachEntryOfTheDropDownIsItsValue()
        {
            for (int at = 0; at < ToleranceChoice.Choices().Count; at++)
            {
                string refused;
                ToleranceChoice chosen = ToleranceChoice.FromThePicker(at, out refused);

                Assert.That(refused, Is.Null, ToleranceChoice.Choices()[at]);
                Assert.That(chosen.Millimetres, Is.EqualTo(ToleranceChoice.OfferedMillimetres[at]));
                Assert.That(chosen.Label(), Is.EqualTo(ToleranceChoice.Choices()[at]));
            }
        }

        /// <summary>FR-166. Nothing selected is a refusal handed back with no throw, so each caller decides whether it matters.</summary>
        [Test]
        public void NothingSelectedIsRefusedWithoutAThrow()
        {
            foreach (int at in new[] { -1, ToleranceChoice.Choices().Count })
            {
                string refused;
                ToleranceChoice chosen = ToleranceChoice.FromThePicker(at, out refused);

                Assert.That(chosen, Is.Null, at.ToString());
                Assert.That(refused, Is.EqualTo(ToleranceChoice.NothingSelected), at.ToString());
            }
        }

        [Test]
        public void AValueReadsInTheWordsAPersonSays()
        {
            Assert.That(ToleranceChoice.Of(37.5).Label(), Is.EqualTo("37.5 mm"));
            Assert.That(ToleranceChoice.Of(25.0).Label(), Is.EqualTo("25 mm"));
        }

        // ---------- what the log says ----------

        /// <summary>The saved tests of one group, each counted by what happened to its tolerance.</summary>
        private static SavedTestTolerances Saved(SavedTestTolerance what, int howMany)
        {
            SavedTestTolerances saved = new SavedTestTolerances();

            for (int i = 0; i < howMany; i++)
            {
                saved.Record(what);
            }

            return saved;
        }

        [Test]
        public void TheChosenLineSaysWhatItBeatAndBothCounts()
        {
            string line = ToleranceChoice.Of(25.0).LogLine(ClashPlanSource.ExchangeFile, 211, Saved(SavedTestTolerance.Set, 1619), "ft");

            Assert.That(line, Does.StartWith("TOLERANCE 25 mm chosen in the tool"));
            Assert.That(line, Does.Contain("beats both the built in matrix and the document"));
            Assert.That(line, Does.Contain("Set on 1830 tests"));
            Assert.That(line, Does.Contain("211 created fresh"));
            Assert.That(line, Does.Contain("1619 already in the document"));
            Assert.That(line, Does.Contain("0.082021 ft"));
            Assert.That(line, Does.Not.Contain("Not set on"),
                "a line where every saved test was set has nothing to say apart");
        }

        /// <summary>FR-063. A value chosen on a run with no XML beats the document alone.</summary>
        [Test]
        public void TheChosenLineOfARunWithNoXmlDoesNotSayItBeatTheXml()
        {
            string line = ToleranceChoice.Of(25.0)
                .LogLine(ClashPlanSource.Document, 0, Saved(SavedTestTolerance.Set, 1830), "ft");

            Assert.That(line, Does.Not.Contain("XML"));
            Assert.That(line, Does.Contain("chosen in the tool, so it beats the document"));
            Assert.That(line, Does.Contain("Set on 1830 tests"));
        }

        /// <summary>
        /// FR-053. The count went up before the test was resolved, so a saved test that was
        /// not found, would not copy as a clash test, threw, or was already at the value was
        /// counted under Set on. Only a test whose tolerance this run changed is set, and
        /// every other one is named by what happened to it.
        /// </summary>
        [Test]
        public void OnlyASavedTestWhoseToleranceWasChangedIsCountedAsSet()
        {
            SavedTestTolerances saved = Saved(SavedTestTolerance.Set, 2);
            saved.Record(SavedTestTolerance.AlreadyAtTheValue);
            saved.Record(SavedTestTolerance.AlreadyAtTheValue);
            saved.Record(SavedTestTolerance.AlreadyAtTheValue);
            saved.Record(SavedTestTolerance.NotFound);
            saved.Record(SavedTestTolerance.WouldNotCopy);
            saved.Record(SavedTestTolerance.Threw);

            string line = ToleranceChoice.Of(25.0).LogLine(ClashPlanSource.ExchangeFile, 10, saved, "ft");

            Assert.That(line, Does.Contain("Set on 12 tests, 10 created fresh and 2 already in the document"));
            Assert.That(line, Does.Contain("Not set on 6 saved tests"));
            Assert.That(line, Does.Contain("3 already at the value"));
            Assert.That(line, Does.Contain("1 not found"));
            Assert.That(line, Does.Contain("1 would not copy as a clash test"));
            Assert.That(line, Does.Contain("1 threw"));
            Assert.That(line, Does.Not.Contain("Set on 18"));
        }

        /// <summary>The break: one kind only, a test never found, and it is never counted as set.</summary>
        [Test]
        public void ASavedTestNeverFoundIsNotCountedAsSet()
        {
            string line = ToleranceChoice.Of(25.0).LogLine(ClashPlanSource.ExchangeFile, 10, Saved(SavedTestTolerance.NotFound, 4), "ft");

            Assert.That(line, Does.Contain("Set on 10 tests, 10 created fresh and 0 already in the document"));
            Assert.That(line, Does.Contain("Not set on 4 saved tests: 4 not found"));
            Assert.That(line, Does.Not.Contain("already at the value"));
        }

        [Test]
        public void TheTallyCountsEachKindApartAndAddsThemUp()
        {
            SavedTestTolerances saved = Saved(SavedTestTolerance.Set, 2);
            saved.Record(SavedTestTolerance.Threw);

            Assert.That(saved.Of(SavedTestTolerance.Set), Is.EqualTo(2));
            Assert.That(saved.Of(SavedTestTolerance.Threw), Is.EqualTo(1));
            Assert.That(saved.Of(SavedTestTolerance.NotFound), Is.EqualTo(0));
            Assert.That(saved.Total, Is.EqualTo(3));
        }

        /// <summary>
        /// Logging never stops a run. A unit the table does not know fails the group where
        /// the tolerance is set, and it must not also throw out of a log line.
        /// </summary>
        [Test]
        public void AUnitTheTableDoesNotKnowStillWritesALine()
        {
            string line = ToleranceChoice.Of(25.0).LogLine(ClashPlanSource.ExchangeFile, 1, new SavedTestTolerances(), "furlongs");

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
            Assert.That(all, Does.Contain("beats the tolerance in the built in matrix and the tolerance in the document"));
            // 5y measured on 2026-09-20 that it resets NOTHING, and this line asserted
            // it did. The real cost is which clashes the test finds next time.
            Assert.That(all, Does.Contain("Every recorded result and every status a person set is KEPT"));
            Assert.That(all, Does.Contain("may not come back at the new tolerance"));
        }

        [Test]
        public void EveryRunWarnsWithItsValueSinceAValueIsAlwaysChosen()
        {
            Assert.That(ToleranceChoice.Default().WarningLines(7, 1830)[0], Is.EqualTo("Clash tolerance: 25 mm, chosen in the tool."));
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
            string line = ToleranceChoice.ReadFromLine(1830, 0, 0);

            Assert.That(line, Does.Contain("the clash test in the document for 1830"));
            Assert.That(line, Does.Not.Contain("XML"), "F155, no row reads its tolerance off the XML");
            Assert.That(line, Does.Contain("chosen in the tool for 0"));
            Assert.That(line, Does.Contain("UNKNOWN for 0"));
            Assert.That(line, Does.Contain("1830 tests in all"));
        }

        [Test]
        public void AReportReadingTheToolRatherThanTheDocumentShowsAsANumber()
        {
            string line = ToleranceChoice.ReadFromLine(0, 1830, 0);

            Assert.That(line, Does.Contain("chosen in the tool for 1830"));
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
            Assert.That(ToleranceChoice.Words(ToleranceOrigin.Tool), Is.EqualTo("chosen in the tool"));
            Assert.That(ToleranceChoice.Words((ToleranceOrigin)99), Is.EqualTo("UNKNOWN"));
        }

        /// <summary>
        /// FR-127. The grey line said changing a saved test resets results, which scan.md 5y measured
        /// false and the confirm screen contradicts. It says what is true and keeps to twelve words.
        /// </summary>
        [Test]
        public void TheHelpLineNeverSaysResultsAreResetAndSaysTheyAreKept()
        {
            Assert.That(ToleranceChoice.HelpLine, Does.Not.Contain("reset"));
            Assert.That(ToleranceChoice.HelpLine, Does.Contain("Results and statuses are kept"));
            Assert.That(ToleranceChoice.HelpLine.Split(' ').Length, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public void TheHelpLineIsTwelveWordsAndCarriesNoCodeIdentifier()
        {
            string[] words = ToleranceChoice.HelpLine.Split(' ');

            Assert.That(words.Length, Is.LessThanOrEqualTo(12));
            Assert.That(ToleranceChoice.HelpLine, Does.Not.Contain("ApplyFileSettings"));
            Assert.That(ToleranceChoice.PickerLabel, Is.EqualTo("Clash tolerance"));
        }

        /// <summary>
        /// Q54. The line that says what the report tolerance line's numbers are, with the
        /// wiring round's own numbers, thousands separated the way Bader wrote them, and
        /// the third number the first two subtracted.
        /// </summary>
        [Test]
        public void TheBlocksLineSaysHowManyRanAndHowManyNeverRan()
        {
            string line = ToleranceChoice.BlocksLine(18300, 1041);

            Assert.That(line, Does.StartWith("WORKBOOK 18,300 test blocks, 1,041 ran and 17,259 are tests that never ran"));
            Assert.That(line, Does.EndWith("Where each block's tolerance was read is on the line below"));
            Assert.That(ToleranceChoice.BlocksLine(0, 0), Does.StartWith("WORKBOOK 0 test blocks, 0 ran and 0 are tests that never ran"));
        }

        /// <summary>
        /// FR-036. The blocks of tests that never ran are not all the clash matrix's since a
        /// chosen value reaches their rows, so the line no longer says they read off it.
        /// </summary>
        [Test]
        public void TheBlocksLineNoLongerSaysEveryTestThatNeverRanReadsOffThePickedFile()
        {
            Assert.That(ToleranceChoice.BlocksLine(18300, 1041), Does.Not.Contain("reads off the clash matrix"));
        }

        /// <summary>
        /// FR-036. A saved test set to 25 mm chosen in the tool and then skipped kept the row
        /// the plan gave it, 75 mm off the document, in its workbook block and in the counts.
        /// </summary>
        [Test]
        public void ARowWhoseTestCarriesTheChosenValueSaysItAndThatTheToolChoseIt()
        {
            TestReport row = new TestReport(1, "ARC vs MEP");
            row.Tolerance = 0.2460629921;
            row.ToleranceUnits = "ft";
            row.ToleranceFrom = ToleranceOrigin.Document;

            ToleranceChoice.Of(25.0).PutOn(row, "ft");

            Assert.That(row.Tolerance, Is.EqualTo(25.0 / 304.8).Within(1e-12),
                "the row kept the old value the plan read off the document");
            Assert.That(row.ToleranceUnits, Is.EqualTo("ft"));
            Assert.That(row.ToleranceFrom, Is.EqualTo(ToleranceOrigin.Tool),
                "the row still says the document gave it its value");
            Assert.That(row.ClientTolerance(), Is.EqualTo("0.082ft"));
        }

        [Test]
        public void ARowCreatedFromTheXmlAtTheChosenValueSaysTheToolChoseIt()
        {
            TestReport row = new TestReport(2, "STR vs MEP");
            row.Tolerance = 0.075;
            row.ToleranceUnits = "m";
            row.ToleranceFrom = ToleranceOrigin.Document;

            ToleranceChoice.Of(50.0).PutOn(row, "m");

            Assert.That(row.Tolerance, Is.EqualTo(0.05).Within(1e-12));
            Assert.That(row.ToleranceFrom, Is.EqualTo(ToleranceOrigin.Tool));
        }

        /// <summary>A row of null is no row and nothing throws, since F155 the one case where PutOn changes nothing.</summary>
        [Test]
        public void ARowOfNullIsNoRow()
        {
            Assert.DoesNotThrow(() => ToleranceChoice.Of(25.0).PutOn(null, "ft"));
        }
    }
}
