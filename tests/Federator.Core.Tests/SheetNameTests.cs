using System;
using System.Collections.Generic;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Excel stops a sheet name at 31 characters, and 1703 of the 1830 test names in the
    /// reference file are longer than that, which is why a sheet is never named after its
    /// test.
    /// </summary>
    [TestFixture]
    public class SheetNameTests
    {
        [Test]
        public void TheFirstTestIsTZeroZeroZeroOne()
        {
            Assert.That(SheetNames.ForTest(1), Is.EqualTo("T0001"));
        }

        [Test]
        public void ThePaddingKeepsThemInOrder()
        {
            Assert.That(SheetNames.ForTest(9), Is.EqualTo("T0009"));
            Assert.That(SheetNames.ForTest(10), Is.EqualTo("T0010"));
            Assert.That(SheetNames.ForTest(99), Is.EqualTo("T0099"));
        }

        // The one the brief asks for by name.
        [Test]
        public void NamingCarriesOnPastAThousandTests()
        {
            Assert.That(SheetNames.ForTest(999), Is.EqualTo("T0999"));
            Assert.That(SheetNames.ForTest(1000), Is.EqualTo("T1000"));
            Assert.That(SheetNames.ForTest(1001), Is.EqualTo("T1001"));
            Assert.That(SheetNames.ForTest(1830), Is.EqualTo("T1830"));
        }

        [Test]
        public void PastTenThousandItGrowsRatherThanWrapping()
        {
            Assert.That(SheetNames.ForTest(10000), Is.EqualTo("T10000"));
            Assert.That(SheetNames.ForTest(123456), Is.EqualTo("T123456"));
        }

        [Test]
        public void EveryNameIsUniqueAcrossTheWholeReferenceFile()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i <= 1830; i++)
            {
                Assert.That(seen.Add(SheetNames.ForTest(i)), Is.True,
                    "two tests were given the same sheet name at " + i);
            }
        }

        // The limit, checked over a range far past anything real.
        [Test]
        public void NoGeneratedNameEverExceedsThirtyOneCharacters()
        {
            Assert.That(SheetNames.MaxLength, Is.EqualTo(31));

            foreach (int number in new[] { 1, 9, 10, 999, 1000, 1830, 9999, 10000, 1000000 })
            {
                string name = SheetNames.ForTest(number);

                Assert.That(name.Length, Is.LessThanOrEqualTo(SheetNames.MaxLength), name);
                Assert.That(SheetNames.IsAcceptable(name), Is.True, name);
            }
        }

        [Test]
        public void TheSummaryAndMatrixNamesAreAcceptableToo()
        {
            Assert.That(SheetNames.IsAcceptable(SheetNames.SummarySheet), Is.True);
            Assert.That(SheetNames.IsAcceptable(SheetNames.MatrixSheet), Is.True);
        }

        [Test]
        public void ATestNumberBelowOneIsRefusedRatherThanNamed()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate { SheetNames.ForTest(0); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { SheetNames.ForTest(-1); });
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

        // A real name off the reference file, which is exactly why sheets are numbered.
        [Test]
        public void ARealTestNameIsTooLongForASheetWhichIsWhyTheyAreNumbered()
        {
            string real = "BLD-ME-Air Terminals v BLD-AR-Floors and Ceilings";

            Assert.That(real.Length, Is.GreaterThan(SheetNames.MaxLength));
            Assert.That(SheetNames.IsAcceptable(real), Is.False);
            Assert.That(SheetNames.IsAcceptable(SheetNames.ForTest(1)), Is.True);
        }
    }
}
