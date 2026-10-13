using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the warning before a clear names, FR-169. A file name that would not read was
    /// called the current unsaved document, and the throw went nowhere. A name, no name and a
    /// name that could not be read are three different things and each is said as itself.
    /// </summary>
    [TestFixture]
    public class DiscardWarningTests
    {
        [Test]
        public void AnOpenFileIsNamedWithItsModels()
        {
            Assert.That(DiscardWarning.For(@"C:\work\1C07BC.nwf", null, 6, null),
                Is.EqualTo(@"The open file C:\work\1C07BC.nwf, holding 6 models."));
        }

        [Test]
        public void ADocumentWithNoFileNameIsTheUnsavedDocument()
        {
            Assert.That(DiscardWarning.For(null, null, 1, null),
                Is.EqualTo("The current unsaved document, holding 1 model."));
        }

        [Test]
        public void ANameThatCouldNotBeReadIsNeverCalledUnsaved()
        {
            string text = DiscardWarning.For(null, "the file name would not read", 3, null);

            Assert.That(text, Does.Not.Contain("unsaved"),
                "a document whose name would not read may well be a saved file");
            Assert.That(text, Does.Contain("file name could not be read"));
            Assert.That(text, Does.Contain("the file name would not read"),
                "the reason the read gave is carried, so the throw is not swallowed");
            Assert.That(text, Does.Contain("holding 3 models"));
        }

        [Test]
        public void AModelCountThatCouldNotBeReadIsSaidAndNotLeftOut()
        {
            string text = DiscardWarning.For(@"C:\work\1C07BC.nwf", null, 0, "the models would not read");

            Assert.That(text, Does.Contain("how many models it holds could not be read"));
            Assert.That(text, Does.Contain("the models would not read"));
        }

        [Test]
        public void AReadThatThrewWithNoMessageSaysNoReasonGiven()
        {
            Assert.That(DiscardWarning.For(null, " ", 0, null),
                Is.EqualTo("The open document, whose file name could not be read (no reason given)."));
        }

        [Test]
        public void ADocumentHoldingNoModelSaysNoCount()
        {
            Assert.That(DiscardWarning.For(@"C:\work\a.nwf", null, 0, null),
                Is.EqualTo(@"The open file C:\work\a.nwf."));
        }
    }
}
