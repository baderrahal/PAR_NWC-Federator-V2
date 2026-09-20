using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests.Rerun
{
    /// <summary>
    /// The one rule for a document that was modified and then failed. It exists because
    /// `ReshapeFromScan` said "the file on disk is left exactly as it was" on every way
    /// out, which was true for the failure before anything is touched and false for the
    /// two after it, and because returning false there let the clear and rebuild run over
    /// the damage and save it.
    ///
    /// Every test here breaks one thing and asserts the rule names it, because a test that
    /// only asserts the good path is what let this survive.
    /// </summary>
    [TestFixture]
    public class DamagedDocumentTests
    {
        [Test]
        public void NothingTouchedSaysTheFileOnDiskIsExactlyAsItWas()
        {
            string line = DamagedDocument.NothingWasTouched("a model this group no longer holds could not be found");

            Assert.That(line, Does.Contain("nothing in the open file had been changed yet"));
            Assert.That(line, Does.Contain("exactly as it was"));
            Assert.That(line, Does.Not.Contain("last good copy"));
        }

        /// <summary>
        /// THE SENTENCE THAT WAS WRONG. A failure after a change must never say the file
        /// was left as it was, because the file was about to be written over.
        /// </summary>
        [Test]
        public void ADamagedDocumentNeverSaysTheFileWasLeftAsItWas()
        {
            string line = DamagedDocument.TheDocumentIsDamaged("a model would not come out");

            Assert.That(line, Does.Contain("had already been changed"));
            Assert.That(line, Does.Contain("NOT saved"));
            Assert.That(line, Does.Contain("last good copy"));
            Assert.That(line, Does.Not.Contain("exactly as it was"));
        }

        [Test]
        public void TheLineIsChosenByTheOneFactThatDecidesIt()
        {
            Assert.That(
                DamagedDocument.Line("something failed", false),
                Is.EqualTo(DamagedDocument.NothingWasTouched("something failed")));

            Assert.That(
                DamagedDocument.Line("something failed", true),
                Is.EqualTo(DamagedDocument.TheDocumentIsDamaged("something failed")));
        }

        [Test]
        public void TheReasonIsCarriedIntoBothSentences()
        {
            Assert.That(DamagedDocument.Line("the second set would not rename", true),
                Does.StartWith("the second set would not rename, and "));

            Assert.That(DamagedDocument.Line("the second set would not rename", false),
                Does.StartWith("the second set would not rename, and "));
        }

        [Test]
        public void NoReasonStillGivesAWholeSentence()
        {
            Assert.That(DamagedDocument.Line(null, true), Does.Contain("last good copy"));
            Assert.That(DamagedDocument.Line(string.Empty, false), Does.Contain("exactly as it was"));
        }

        /// <summary>
        /// A FALLBACK MAY ONLY RUN OVER A DOCUMENT NOTHING HAS TOUCHED. Over a damaged one
        /// it reads the damage as the starting state, finds everything present, and
        /// reports everything kept. That is the defect this whole rule exists for.
        /// </summary>
        [Test]
        public void AFallbackMayRunOnlyWhenNothingWasChanged()
        {
            Assert.That(DamagedDocument.AFallbackMayRun(false), Is.True);
            Assert.That(DamagedDocument.AFallbackMayRun(true), Is.False);
        }

        /// <summary>
        /// THE SAVE GATE, and the row that matters is the third: changed, and it failed,
        /// so nothing is written. That is what makes "the last good copy" true rather than
        /// a hope.
        /// </summary>
        [Test]
        public void TheFileIsSavedOnlyWhenItIsSafeTo()
        {
            Assert.That(DamagedDocument.TheFileMayBeSaved(false, true), Is.True, "untouched and it worked");
            Assert.That(DamagedDocument.TheFileMayBeSaved(true, true), Is.True, "changed and it worked, which is the ordinary path");
            Assert.That(DamagedDocument.TheFileMayBeSaved(false, false), Is.True, "nothing was changed, so writing it changes nothing");
            Assert.That(DamagedDocument.TheFileMayBeSaved(true, false), Is.False, "CHANGED AND IT FAILED, so the disk copy is left alone");
        }
    }
}
