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
        public void NothingTouchedSaysTheFileOnDiskIsUnchangedByThisRun()
        {
            string line = DamagedDocument.NothingWasTouched("a model this group no longer holds could not be found");

            Assert.That(line, Does.Contain("nothing in the open file had been changed yet"));
            Assert.That(line, Does.Contain("unchanged by this run"));

            // IT NEVER CLAIMS THE FILE ON DISK IS GOOD. That is a claim about history and
            // this tool knows only that it did not write.
            Assert.That(line, Does.Not.Contain("last good copy"));
            Assert.That(line, Does.Not.Contain("good"));
        }

        /// <summary>
        /// THE SENTENCE THAT WAS WRONG TWICE OVER. A failure after a change must never say
        /// the file was left as it was, because it was about to be written over, and must not
        /// say the file is GOOD either, because that is a claim about history.
        /// </summary>
        [Test]
        public void ADamagedDocumentSaysUnchangedByThisRunAndNeverThatItIsGood()
        {
            string line = DamagedDocument.TheDocumentIsDamaged("a model would not come out");

            Assert.That(line, Does.Contain("had already been changed"));
            Assert.That(line, Does.Contain("NOT saved"));
            Assert.That(line, Does.Contain("unchanged by this run"));

            // THE OVERCLAIM THE FIRST DRAFT CARRIED. "The last good copy" says the file on
            // disk is GOOD, which this tool cannot know: whatever wrote it last may itself
            // have failed some other way. It knows it did not write, and nothing else.
            Assert.That(line, Does.Not.Contain("last good copy"));
            Assert.That(line, Does.Not.Contain("good"));
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
            Assert.That(DamagedDocument.Line(null, true), Does.Contain("unchanged by this run"));
            Assert.That(DamagedDocument.Line(string.Empty, false), Does.Contain("unchanged by this run"));
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
        /// so nothing is written. That is what makes "unchanged by this run" true rather than
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
