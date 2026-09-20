using System.Collections.Generic;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests.Views
{
    /// <summary>
    /// 5s. The strings in these tests are the real ones, read off the client's own
    /// 1A02MM models on 2026-09-20 by `ViewpointProbe` in its `census` mode, off the
    /// 29 services the penetration rule reported as having no readable size.
    /// </summary>
    [TestFixture]
    public class SizeTextTests
    {
        [Test]
        public void AConduitDiameterWrittenAsWordsIsRead()
        {
            Assert.That(SizeText.LargestMillimetres("53 mmø"), Is.EqualTo(53.0));
        }

        /// <summary>
        /// A cable tray fitting writes one measurement per connector, so a four way
        /// fitting writes four. The largest is what has to fit through the wall, which
        /// is the reading SizeRule already takes for a duct.
        /// </summary>
        [Test]
        public void ACableTrayFittingWritesOnePairPerConnectorAndTheLargestWins()
        {
            IList<double> all = SizeText.Millimetres("600 mmx100 mm-600 mmx100 mm");

            Assert.That(all.Count, Is.EqualTo(4));
            Assert.That(all[0], Is.EqualTo(600.0));
            Assert.That(all[1], Is.EqualTo(100.0));
            Assert.That(SizeText.LargestMillimetres("600 mmx100 mm-600 mmx100 mm"), Is.EqualTo(600.0));
        }

        [Test]
        public void AFourWayFittingIsReadTheSameWay()
        {
            Assert.That(
                SizeText.LargestMillimetres("600 mmx100 mm-600 mmx100 mm-600 mmx100 mm-600 mmx100 mm"),
                Is.EqualTo(600.0));
        }

        /// <summary>
        /// The whole reason this reader is allowed to exist. A number with no unit could
        /// be millimetres or inches and this tool says UNKNOWN rather than filling the
        /// gap, because the penetration rule LEAVES ALONE a service it cannot measure,
        /// so refusing costs a clash somebody looks at and guessing costs a hole nobody
        /// checked.
        /// </summary>
        [Test]
        public void ANumberWithNoUnitIsRefusedAndNeverGuessedAt()
        {
            Assert.That(SizeText.Millimetres("300x300"), Is.Empty);
            Assert.That(SizeText.LargestMillimetres("300x300"), Is.Null);
            Assert.That(SizeText.LargestMillimetres("150"), Is.Null);
        }

        [Test]
        public void EveryUnitTheOneTableKnowsIsReadThroughIt()
        {
            Assert.That(SizeText.LargestMillimetres("2 m"), Is.EqualTo(2000.0));
            Assert.That(SizeText.LargestMillimetres("3 cm"), Is.EqualTo(30.0));
            Assert.That(SizeText.LargestMillimetres("1 ft"), Is.EqualTo(304.8));
            Assert.That(SizeText.LargestMillimetres("2 in"), Is.EqualTo(50.8));
        }

        [Test]
        public void TheUnitMayBeWrittenAgainstTheNumberOrWithOneSpace()
        {
            Assert.That(SizeText.LargestMillimetres("53mm"), Is.EqualTo(53.0));
            Assert.That(SizeText.LargestMillimetres("53 mm"), Is.EqualTo(53.0));
        }

        /// <summary>
        /// The longest label wins, or "600 mm" would read as 600 metres. And a label with
        /// a letter after it is a longer word, so "5 minutes" is not five miles.
        /// </summary>
        [Test]
        public void TheLongestUnitWinsAndAUnitInsideAWordIsNotAUnit()
        {
            Assert.That(SizeText.LargestMillimetres("600 mm"), Is.EqualTo(600.0), "mm and not m");
            Assert.That(SizeText.LargestMillimetres("5 minutes"), Is.Null, "mi is not a unit inside minutes");
            Assert.That(SizeText.LargestMillimetres("2 mil"), Is.EqualTo(0.0508), "mil and not mi");
        }

        [Test]
        public void ADecimalIsRead()
        {
            Assert.That(SizeText.LargestMillimetres("12.7 mm"), Is.EqualTo(12.7));
        }

        [Test]
        public void NothingAndNonsenseComeBackEmptyRatherThanThrowing()
        {
            Assert.That(SizeText.Millimetres(null), Is.Empty);
            Assert.That(SizeText.Millimetres(string.Empty), Is.Empty);
            Assert.That(SizeText.Millimetres("Standard"), Is.Empty);
            Assert.That(SizeText.LargestMillimetres("ELE-CNF-70mm BARE COPPER CABLE"), Is.EqualTo(70.0),
                "a number with a unit inside a longer name is still a number with a unit");
        }
    }
}
