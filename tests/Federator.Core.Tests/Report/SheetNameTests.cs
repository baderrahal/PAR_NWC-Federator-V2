using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Excel stops a sheet name at 31 characters, and 1703 of the 1830 test names in the
    /// reference file are longer than that, which is why a sheet is never named after a
    /// test. The one sheet is named after the report.
    /// </summary>
    [TestFixture]
    public class SheetNameTests
    {
        // The limit. The one sheet is named after the report and cut at 31, the way
        // the client's own export is.
        [Test]
        public void TheOneSheetNameNeverExceedsThirtyOneCharacters()
        {
            Assert.That(SheetNames.MaxLength, Is.EqualTo(31));

            string name = SheetNames.ForReport("1104-PAR-1A04WN-XXX-BM-RPT-000001");

            Assert.That(name, Is.EqualTo("1104-PAR-1A04WN-XXX-BM-RPT-0000"));
            Assert.That(name.Length, Is.EqualTo(SheetNames.MaxLength));
            Assert.That(SheetNames.IsAcceptable(name), Is.True);
        }

        // ---------- what Excel refuses ----------

        [Test]
        public void EveryCharacterExcelRefusesIsCaughtRatherThanWritten()
        {
            foreach (char refused in SheetNames.Refused)
            {
                Assert.That(SheetNames.IsAcceptable("Floors " + refused + " Ducts"), Is.False,
                    "Excel refuses " + refused + " and this said it was fine");
            }
        }

        // The one the brief asks for by name. A real test name full of things Excel
        // refuses still has to produce a name Excel accepts.
        [Test]
        public void ATestNameHoldingCharactersExcelRefusesStillGivesAGoodSheetName()
        {
            string awful = @"BLD-AR-Floors [1] : BLD-ME-Ducts / Pipes \ Cables * ? Everything";

            Assert.That(SheetNames.IsAcceptable(awful), Is.False, "this name is the problem");

            string tidied = SheetNames.Sanitise(awful);

            Assert.That(SheetNames.IsAcceptable(tidied), Is.True,
                "the tidied name is still one Excel refuses: " + tidied);
            Assert.That(tidied.Length, Is.LessThanOrEqualTo(SheetNames.MaxLength));
        }

        [Test]
        public void SanitisingLeavesAnAcceptableNameAlone()
        {
            Assert.That(SheetNames.Sanitise("T0001"), Is.EqualTo("T0001"));
            Assert.That(SheetNames.Sanitise("Summary"), Is.EqualTo("Summary"));
        }

        [Test]
        public void AnEmptyOrAllRefusedNameFallsBackRatherThanBeingBlank()
        {
            Assert.That(SheetNames.Sanitise(null, "T7"), Is.EqualTo("T7"));
            Assert.That(SheetNames.Sanitise(string.Empty, "T7"), Is.EqualTo("T7"));
            Assert.That(SheetNames.Sanitise("   ", "T7"), Is.EqualTo("T7"));
            Assert.That(SheetNames.Sanitise("[]:*?", "T7"), Is.EqualTo("T7"));
        }

        [Test]
        public void ALeadingOrTrailingApostropheIsRefusedAndRemoved()
        {
            Assert.That(SheetNames.IsAcceptable("'Floors"), Is.False);
            Assert.That(SheetNames.IsAcceptable("Floors'"), Is.False);
            Assert.That(SheetNames.IsAcceptable("Flo'ors"), Is.True, "one in the middle is fine");

            Assert.That(SheetNames.Sanitise("'Floors'"), Is.EqualTo("Floors"));
        }

        [Test]
        public void AVeryLongNameIsCutToTheLimitAndStillAcceptable()
        {
            string tidied = SheetNames.Sanitise(new string('a', 200));

            Assert.That(tidied.Length, Is.EqualTo(SheetNames.MaxLength));
            Assert.That(SheetNames.IsAcceptable(tidied), Is.True);
        }

        // A real name off the reference file, which is exactly why no sheet is named
        // after a test.
        [Test]
        public void ARealTestNameIsTooLongForASheetWhichIsWhyNoSheetIsNamedAfterOne()
        {
            string real = "BLD-ME-Air Terminals v BLD-AR-Floors and Ceilings";

            Assert.That(real.Length, Is.GreaterThan(SheetNames.MaxLength));
            Assert.That(SheetNames.IsAcceptable(real), Is.False);
        }
    }
}
