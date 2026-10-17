using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F204. Clash Detective finds no clash on a hidden item, so the clash step shows everything and hides again what was
    /// hidden. These are the lines it says. The numbers are the C04 copy's of 17 Oct 2026: 8 hidden items, the 8 root items
    /// of 8 of its 10 models.
    /// </summary>
    [TestFixture]
    public class HiddenForTheClashTests
    {
        [Test]
        public void SomethingHiddenIsNamedWithItsCountsAndWhatIsDone()
        {
            string line = HiddenForTheClash.Found(8, 8, 10);

            Assert.That(line, Does.StartWith("CLASH    HIDDEN   8 hidden item(s), 8 of 10 model(s) hidden whole."));
            Assert.That(line, Does.Contain("would mark the clashes of a hidden model Resolved"));
            Assert.That(line, Does.EndWith("everything is shown for the clash and hidden again after it"));
        }

        // The break. A model hidden whole with no item the snapshot lists still counts as hidden, so it is not said to be
        // clashed as it stands.
        [Test]
        public void AModelHiddenWholeIsHiddenEvenWithNoHiddenItemRead()
        {
            string line = HiddenForTheClash.Found(0, 1, 10);

            Assert.That(line, Does.Contain("0 hidden item(s), 1 of 10 model(s) hidden whole"));
            Assert.That(line, Does.Not.Contain("nothing is hidden"));
        }

        [Test]
        public void NothingHiddenSaysSoAndShowsNothing()
        {
            string line = HiddenForTheClash.Found(0, 0, 10);

            Assert.That(line, Is.EqualTo("CLASH    HIDDEN   nothing is hidden in the document, so every item is clashed as it stands"));
            Assert.That(line, Does.Not.Contain("shown for the clash"));
        }

        [Test]
        public void PutBackReadBackIsSaidPlainly()
        {
            Assert.That(HiddenForTheClash.PutBack(true, 8),
                Is.EqualTo("CLASH    HIDDEN   the 8 item(s) hidden before the clash are hidden again, read back"));
        }

        // The break. A put back that does not read back is a NOTICE that says the clash results are right and only the view
        // may differ, never a line that reads as if all were hidden again.
        [Test]
        public void PutBackNotReadBackIsANotice()
        {
            string line = HiddenForTheClash.PutBack(false, 8);

            Assert.That(line, Does.StartWith("CLASH    HIDDEN   NOTICE the 8 item(s)"));
            Assert.That(line, Does.Contain("do not all read hidden again"));
            Assert.That(line, Does.Not.Contain("are hidden again, read back"));
        }

        [Test]
        public void AThrowIsANoticeThatNamesWhatThrewAndTheRisk()
        {
            string line = HiddenForTheClash.Threw("showing every item for the clash", "boom");

            Assert.That(line, Is.EqualTo("CLASH    HIDDEN   NOTICE showing every item for the clash threw: boom. "
                + "The run goes on, and the clashes of any item still hidden may read Resolved"));
            Assert.That(HiddenForTheClash.Threw("x", null), Does.Contain("x threw: ."));
        }
    }
}
