using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F206. A picture shows nothing of a hidden item, so the picture pass shows everything and hides again what was hidden.
    /// These are the lines it says. The numbers are the C04 copy's of 17 Oct 2026: 8 hidden items, the 8 root items of 8 of
    /// its 10 models.
    /// </summary>
    [TestFixture]
    public class HiddenForThePicturesTests
    {
        [Test]
        public void SomethingHiddenIsNamedWithItsCountsAndWhatIsDone()
        {
            string line = HiddenForThePictures.Found(8, 8, 10);

            Assert.That(line, Does.StartWith("IMAGES   HIDDEN   8 hidden item(s), 8 of 10 model(s) hidden whole."));
            Assert.That(line, Does.Contain("A picture shows nothing of a hidden item"));
            Assert.That(line, Does.EndWith("everything is shown for the pictures and hidden again after them"));
        }

        // The break. A model hidden whole with no item the snapshot lists still counts as hidden, so it is not said to be
        // pictured as it stands.
        [Test]
        public void AModelHiddenWholeIsHiddenEvenWithNoHiddenItemRead()
        {
            string line = HiddenForThePictures.Found(0, 1, 10);

            Assert.That(line, Does.Contain("0 hidden item(s), 1 of 10 model(s) hidden whole"));
            Assert.That(line, Does.Not.Contain("nothing is hidden"));
        }

        [Test]
        public void NothingHiddenSaysSoAndShowsNothing()
        {
            string line = HiddenForThePictures.Found(0, 0, 10);

            Assert.That(line, Is.EqualTo("IMAGES   HIDDEN   nothing is hidden in the document, so every picture shows its clash as it stands"));
            Assert.That(line, Does.Not.Contain("shown for the pictures"));
        }

        [Test]
        public void PutBackReadBackIsSaidPlainly()
        {
            Assert.That(HiddenForThePictures.PutBack(true, 8),
                Is.EqualTo("IMAGES   HIDDEN   the 8 item(s) hidden before the pictures are hidden again, read back"));
        }

        // The break. A put back that does not read back is a NOTICE that says the pictures and the saved files are right and
        // only the open document may differ, never a line that reads as if all were hidden again.
        [Test]
        public void PutBackNotReadBackIsANotice()
        {
            string line = HiddenForThePictures.PutBack(false, 8);

            Assert.That(line, Does.StartWith("IMAGES   HIDDEN   NOTICE the 8 item(s)"));
            Assert.That(line, Does.Contain("do not all read hidden again"));
            Assert.That(line, Does.Contain("the NWF and the NWD were saved before them"));
            Assert.That(line, Does.Not.Contain("are hidden again, read back"));
        }

        [Test]
        public void AThrowIsANoticeThatNamesWhatThrewAndTheRisk()
        {
            string line = HiddenForThePictures.Threw("showing every item for the pictures", "boom");

            Assert.That(line, Is.EqualTo("IMAGES   HIDDEN   NOTICE showing every item for the pictures threw: boom. "
                + "The run goes on, and a picture of an item still hidden may show nothing of it"));
            Assert.That(HiddenForThePictures.Threw("x", null), Does.Contain("x threw: ."));
        }

        // The two steps say their own words, so a reader of the log knows which step showed and which put back.
        [Test]
        public void ThePictureLinesAreNotTheClashLines()
        {
            Assert.That(HiddenForThePictures.Found(8, 8, 10), Is.Not.EqualTo(HiddenForTheClash.Found(8, 8, 10)));
            Assert.That(HiddenForThePictures.PutBack(true, 8), Is.Not.EqualTo(HiddenForTheClash.PutBack(true, 8)));
        }
    }
}
