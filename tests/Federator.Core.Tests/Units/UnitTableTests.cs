using System;
using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Report;
using Federator.Core.Units;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The one unit table. Every other list of units in the repo read it or went, F33,
    /// because the lists had drifted: the same unit was "micrometers" in one and "um" in
    /// another.
    /// </summary>
    [TestFixture]
    public class UnitTableTests
    {
        [Test]
        public void EveryRowHasEveryColumnFilled()
        {
            Assert.That(UnitTable.All.Count, Is.GreaterThan(0));

            foreach (UnitRow row in UnitTable.All)
            {
                Assert.That(row.EnumName, Is.Not.Empty);
                Assert.That(row.DisplayName, Is.Not.Empty, row.EnumName);
                Assert.That(row.Short, Is.Not.Empty, row.EnumName);
                Assert.That(row.ExchangeCode, Is.Not.Empty, row.EnumName);
                Assert.That(row.MillimetresPerUnit, Is.GreaterThan(0.0), row.EnumName);
            }
        }

        [Test]
        public void NoTwoRowsShareAnEnumNameOrAnExchangeCode()
        {
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (UnitRow row in UnitTable.All)
            {
                Assert.That(names.Add(row.EnumName), Is.True, "enum name twice: " + row.EnumName);
                Assert.That(codes.Add(row.ExchangeCode), Is.True, "exchange code twice: " + row.ExchangeCode);
            }
        }

        /// <summary>
        /// The window offers what this returns, so every name it shows is a name the
        /// engine can act on, and the default the options carry is the first one shown.
        /// </summary>
        [Test]
        public void EveryOfferedUnitIsInTheTableAndTheDefaultIsFirst()
        {
            IList<UnitRow> offered = UnitTable.Offered();

            Assert.That(offered.Count, Is.GreaterThan(0));
            Assert.That(offered[0].EnumName, Is.EqualTo(ReportOptions.DefaultUnits));

            foreach (UnitRow row in offered)
            {
                Assert.That(UnitTable.All, Does.Contain(row), row.EnumName);
            }
        }

        [Test]
        public void LookupsAreCaseBlindAndTrimmed()
        {
            Assert.That(UnitTable.FindByEnumName(" meters ").EnumName, Is.EqualTo("Meters"));
            Assert.That(UnitTable.FindByExchangeCode(" FT ").EnumName, Is.EqualTo("Feet"));
            Assert.That(UnitTable.FindByEnumName("cubits"), Is.Null);
            Assert.That(UnitTable.FindByExchangeCode("cubit"), Is.Null);
            Assert.That(UnitTable.FindByEnumName(null), Is.Null);
            Assert.That(UnitTable.FindByExchangeCode(null), Is.Null);
        }

        [Test]
        public void AnUnknownEnumNameIsRefusedNamingItAndEveryKnownOne()
        {
            NotSupportedException error = Assert.Throws<NotSupportedException>(
                () => UnitTable.ByEnumName("cubits"));

            Assert.That(error.Message, Does.Contain("cubits"));
            Assert.That(error.Message, Does.Contain("Meters"));
            Assert.That(error.Message, Does.Contain("Feet"));
        }

        // 0.2460629921 ft is the 75 mm .claude\rules\core.md records off the reference file.
        [Test]
        public void TheFeetRowAgreesWithTheReferenceFile()
        {
            UnitRow feet = UnitTable.FindByExchangeCode("ft");

            Assert.That(feet.EnumName, Is.EqualTo("Feet"));
            Assert.That(0.2460629921 * feet.MillimetresPerUnit, Is.EqualTo(75.0).Within(1e-6));
        }

        [Test]
        public void TheReportLabelIsTheMetresRow()
        {
            Assert.That(UnitTable.ByEnumName("Meters").Short, Is.EqualTo(ReportUnits.Short));
        }

        [Test]
        public void ExchangeUnitsConvertsThroughTheTableAndKnowsNothingElse()
        {
            Assert.That(ExchangeUnits.KnownUnits().Count, Is.EqualTo(UnitTable.All.Count));

            foreach (UnitRow row in UnitTable.All)
            {
                Assert.That(ExchangeUnits.IsKnown(row.ExchangeCode), Is.True, row.ExchangeCode);
                Assert.That(ExchangeUnits.Convert(1.0, row.ExchangeCode, "mm"),
                    Is.EqualTo(row.MillimetresPerUnit).Within(1e-12), row.ExchangeCode);
            }
        }
    }
}
