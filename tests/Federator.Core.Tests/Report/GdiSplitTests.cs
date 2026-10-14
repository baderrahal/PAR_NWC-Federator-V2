using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F161. Where the GDI objects of the picture pass go, so a run that climbs one a picture can say
    /// whether the object is made in Navisworks's render or in the save of the bitmap.
    /// </summary>
    [TestFixture]
    public class GdiSplitTests
    {
        [Test]
        public void TheLineSumsTheRenderAndTheSaveApartAndPutsTheSumsBack()
        {
            GdiSplit split = new GdiSplit();

            split.Add(1092, 1093, 1093);
            split.Add(1093, 1094, 1094);
            split.Add(1094, 1095, 1095);

            Assert.That(split.TakeLine(), Is.EqualTo(
                "GDI      PICTURES in the last 3 pictures the GDI objects moved +3 in TestsImageForResult and +0 in the save"
                + " and the dispose of the bitmap"));
            Assert.That(split.TakeLine(), Is.Null, "the sums were put back, so nothing is said twice");
        }

        [Test]
        public void ASaveThatLeaksIsToldApartFromARenderThatDoesNot()
        {
            GdiSplit split = new GdiSplit();

            split.Add(100, 100, 102);
            split.Add(102, 102, 104);

            Assert.That(split.TakeLine(), Does.Contain("moved +0 in TestsImageForResult and +4 in the save"));
        }

        [Test]
        public void ACountThatFellIsShownWithItsMinusAndANotReadPictureIsCounted()
        {
            GdiSplit split = new GdiSplit();

            split.Add(100, 90, 90);
            split.Add(-1, 5, 5);

            string line = split.TakeLine();

            Assert.That(line, Does.Contain("the last 1 pictures"));
            Assert.That(line, Does.Contain("moved -10 in TestsImageForResult and +0 in the save"));
            Assert.That(line, Does.EndWith(", and 1 not read"));
        }
    }
}