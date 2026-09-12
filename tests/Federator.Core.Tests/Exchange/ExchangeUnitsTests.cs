using System;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    [TestFixture]
    public class ExchangeUnitsTests
    {
        [Test]
        public void TheClashFileToleranceIsSeventyFiveMillimetres()
        {
            Assert.That(
                ExchangeUnits.Convert(0.2460629921, "ft", "mm"), Is.EqualTo(75.0).Within(1e-6));
        }

        [Test]
        public void TheBuildingFileToleranceIsFiftyMillimetres()
        {
            Assert.That(
                ExchangeUnits.Convert(0.1640419948, "ft", "mm"), Is.EqualTo(50.0).Within(1e-6));
        }

        [TestCase("mm", 1.0)]
        [TestCase("cm", 10.0)]
        [TestCase("m", 1000.0)]
        [TestCase("in", 25.4)]
        [TestCase("ft", 304.8)]
        [TestCase("yd", 914.4)]
        public void KnownUnitsConvertToMillimetres(string units, double expected)
        {
            Assert.That(ExchangeUnits.FactorToMillimetres(units), Is.EqualTo(expected).Within(1e-9));
        }

        [Test]
        public void ConvertingThereAndBackReturnsTheSameValue()
        {
            double metres = ExchangeUnits.Convert(0.2460629921, "ft", "m");

            Assert.That(metres, Is.EqualTo(0.075).Within(1e-9));
            Assert.That(ExchangeUnits.Convert(metres, "m", "ft"), Is.EqualTo(0.2460629921).Within(1e-9));
        }

        [Test]
        public void AnUnknownUnitIsRefusedRatherThanGuessedAt()
        {
            Assert.Throws<NotSupportedException>(() => ExchangeUnits.FactorToMillimetres("furlong"));
            Assert.That(ExchangeUnits.IsKnown("furlong"), Is.False);
            Assert.That(ExchangeUnits.IsKnown("ft"), Is.True);
        }
    }
}
