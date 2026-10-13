using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-076. Why one picture did not happen is said in two parts: the reason, which is what
    /// the fifty failures guard counts and so carries no path, and the detail beside it on
    /// the log line, which does. A render that finished with no file used to carry the path
    /// in its reason, so that failure never repeated the same reason and the guard never
    /// fired on it.
    /// </summary>
    [TestFixture]
    public class ImageFailureTests
    {
        [Test]
        public void ARenderThatLeftNoFileHasOneReasonWhateverThePath()
        {
            ImageFailure first = ImageFailure.BecauseNoFileArrived(@"C:\Clash Reports\1A02MM\cd000001.jpg");
            ImageFailure second = ImageFailure.BecauseNoFileArrived(@"C:\Clash Reports\1A02MM\cd000002.jpg");

            Assert.That(first.Reason, Is.EqualTo(second.Reason));
            Assert.That(first.Reason, Is.EqualTo(ImageFailure.NoFileArrived));
            Assert.That(first.Reason, Does.Not.Contain("cd000001"));
            Assert.That(first.Line(), Does.Contain(@"C:\Clash Reports\1A02MM\cd000001.jpg"),
                "the path is said on the line beside the reason");
            Assert.That(first.Line(), Does.StartWith(ImageFailure.NoFileArrived));
        }

        [Test]
        public void ARenderThatThrewCarriesWhatItThrew()
        {
            ImageFailure failure = ImageFailure.BecauseItThrew(
                new System.InvalidOperationException("TestsImageForResult returned nothing for this clash."));

            Assert.That(failure.Reason, Is.EqualTo(
                "InvalidOperationException: TestsImageForResult returned nothing for this clash."));
            Assert.That(failure.Line(), Is.EqualTo(failure.Reason));
        }

        /// <summary>
        /// F120-R1, the developer's finding on FR-076. A render that threw with the picture's
        /// path inside the exception's message, such as access denied naming the file, was one
        /// reason per path, so fifty such throws never stopped the run. The reason now says the
        /// picture's path where the message named it, and the line still says the path.
        /// </summary>
        [Test]
        public void ARenderThatThrewNamingItsPathHasOneReasonWhateverThePath()
        {
            string one = @"C:\Clash Reports\1A02MM_files\cd000001.jpg";
            string two = @"C:\Clash Reports\1A02MM_files\cd000002.jpg";
            ImageFailure first = ImageFailure.BecauseItThrewWriting(
                new System.UnauthorizedAccessException("Access to the path '" + one + "' is denied."), one);
            ImageFailure second = ImageFailure.BecauseItThrewWriting(
                new System.UnauthorizedAccessException("Access to the path '" + two + "' is denied."), two);

            Assert.That(first.Reason, Is.EqualTo(second.Reason));
            Assert.That(first.Reason, Is.EqualTo(
                "UnauthorizedAccessException: Access to the path 'the picture's path' is denied."));
            Assert.That(first.Line(), Does.Contain(one), "the path is said on the line beside the reason");
            Assert.That(second.Line(), Does.Contain(two));
        }

        /// <summary>
        /// The folder made for the pictures can be what the message names, when making it threw,
        /// and a path written in other capitals is the same path on Windows.
        /// </summary>
        [Test]
        public void AThrowNamingThePicturesFolderInOtherCapitalsHasOneReasonToo()
        {
            string one = @"C:\Clash Reports\1A02MM_files\cd000001.jpg";
            string two = @"C:\Clash Reports\1A04PK_files\cd000001.jpg";
            ImageFailure first = ImageFailure.BecauseItThrewWriting(
                new System.UnauthorizedAccessException(@"Access to the path 'c:\clash reports\1a02mm_files' is denied."), one);
            ImageFailure second = ImageFailure.BecauseItThrewWriting(
                new System.UnauthorizedAccessException(@"Access to the path 'C:\Clash Reports\1A04PK_files' is denied."), two);

            Assert.That(first.Reason, Is.EqualTo(second.Reason));
            Assert.That(first.Reason, Does.Contain("the picture's folder"));
        }

        /// <summary>The break: two different throws stay two reasons, so the guard counts a streak of one fault only.</summary>
        [Test]
        public void TwoDifferentThrowsWritingStayTwoReasons()
        {
            string path = @"C:\Clash Reports\1A02MM_files\cd000001.jpg";
            ImageFailure denied = ImageFailure.BecauseItThrewWriting(
                new System.UnauthorizedAccessException("Access to the path '" + path + "' is denied."), path);
            ImageFailure full = ImageFailure.BecauseItThrewWriting(
                new System.IO.IOException("There is not enough space on the disk."), path);

            Assert.That(denied.Reason, Is.Not.EqualTo(full.Reason));
            Assert.That(full.Reason, Is.EqualTo("IOException: There is not enough space on the disk."));
        }

        [Test]
        public void TheRunStoppedWordsSayImagesAndCarryTheReason()
        {
            string reason = ImageFailure.RunStopped(50, ImageFailure.NoFileArrived);

            Assert.That(reason, Does.StartWith("the last 50 clash images all failed for the same reason"));
            Assert.That(reason, Does.Contain("the rest of the run was not attempted"));
            Assert.That(reason, Does.Contain("Switch images off to run without them"));
            Assert.That(reason, Does.EndWith(ImageFailure.NoFileArrived));
            Assert.That(reason, Does.Not.Contain("tests"), "what failed was pictures, not tests");
        }

        /// <summary>FR-129. Fifty failures in a row can follow any number of good pictures, so the words never say the first fifty.</summary>
        [Test]
        public void TheRunStoppedWordsNeverSayTheFirst()
        {
            Assert.That(ImageFailure.RunStopped(50, "anything"), Does.Not.Contain("first"));
            Assert.That(ImageFailure.RunStoppedInPlainWords(50), Does.Not.Contain("first"));
        }

        /// <summary>A label carries no framework message and no type name, so the plain words carry the count alone.</summary>
        [Test]
        public void ThePlainWordsForTheLabelCarryNoReason()
        {
            string words = ImageFailure.RunStoppedInPlainWords(50);

            Assert.That(words, Is.EqualTo(
                "The run was stopped. 50 clash images in a row all failed the same way, "
                + "so the rest was not attempted. The log says what the failure was."));
        }

        /// <summary>The guard reads the reason alone, so fifty renders that left no file at fifty paths stop the run.</summary>
        [Test]
        public void FiftyRendersThatLeftNoFileAtFiftyPathsStopTheRun()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            for (int i = 1; i <= 50; i++)
            {
                guard.RecordFailure(ImageFailure.BecauseNoFileArrived(@"C:\Clash Reports\cd" + i + ".jpg").Reason);
            }

            Assert.That(guard.ShouldStopTheRun, Is.True);
            Assert.That(guard.FirstReason, Is.EqualTo(ImageFailure.NoFileArrived));
        }

        // F149, the breaker's first point on attempt 1. A picture the pass could not ask for is a
        // failure like a render that failed, with one reason for one fault whichever row, so the
        // IMAGES line counts it and fifty in a row stop the run.
        [Test]
        public void AClashNotFoundAgainHasOneReasonWhateverTheRow()
        {
            ImageFailure first = ImageFailure.BecauseItWasNotFoundAgain(
                "no result named \"Clash1\" is among the 4 at the level of results[0] under Walls any more");
            ImageFailure second = ImageFailure.BecauseItWasNotFoundAgain(
                "no result named \"Clash2\" is among the 4 at the level of results[1] under Walls any more");

            Assert.That(first.Reason, Is.EqualTo(second.Reason));
            Assert.That(first.Reason, Is.EqualTo(ImageFailure.ClashNotFoundAgain));
            Assert.That(first.Reason, Does.Not.Contain("Clash1"));
            Assert.That(first.Line(), Does.StartWith(ImageFailure.ClashNotFoundAgain));
            Assert.That(first.Line(), Does.Contain("Clash1"), "what was found instead is said beside the reason");
        }

        [Test]
        public void ATestNotFoundAgainHasOneReasonWhateverTheTest()
        {
            ImageFailure first = ImageFailure.BecauseItsTestWasNotFoundAgain("Walls is not at [3] any more");
            ImageFailure second = ImageFailure.BecauseItsTestWasNotFoundAgain("Floors is not at [7] any more");

            Assert.That(first.Reason, Is.EqualTo(second.Reason));
            Assert.That(first.Reason, Is.EqualTo(ImageFailure.TestNotFoundAgain));
            Assert.That(first.Reason, Does.Not.Contain("Walls"));
            Assert.That(first.Line(), Does.Contain("Walls is not at [3] any more"));
        }

        // A row left when a throw ended its stretch failed for that throw, so it is counted under the
        // throw's own reason, the one a render that threw the same way is counted under, and the line
        // says the throw ended its stretch.
        [Test]
        public void ARowLeftByAThrowIsCountedUnderTheThrowsOwnReason()
        {
            System.ObjectDisposedException thrown = new System.ObjectDisposedException("Object has been Disposed (WeakRef)");
            ImageFailure left = ImageFailure.BecauseItsStretchEnded(thrown);

            Assert.That(left.Reason, Is.EqualTo(ImageFailure.BecauseItThrew(thrown).Reason));
            Assert.That(left.Line(), Does.StartWith(left.Reason));
            Assert.That(left.Line(), Does.Contain("ended the pictures of its stretch"));
            Assert.That(() => ImageFailure.BecauseItsStretchEnded(null), Throws.TypeOf<System.ArgumentNullException>());
        }

        [Test]
        public void EachFaultIsAReasonOfItsOwn()
        {
            string[] reasons = { ImageFailure.NoFileArrived, ImageFailure.TestNotFoundAgain, ImageFailure.ClashNotFoundAgain };

            for (int i = 0; i < reasons.Length; i++)
            {
                for (int j = i + 1; j < reasons.Length; j++)
                {
                    Assert.That(reasons[i], Is.Not.EqualTo(reasons[j]));
                }
            }
        }

        // The guard reads the new reasons the same way, so fifty clashes in a row not found again stop the run.
        [Test]
        public void FiftyClashesInARowNotFoundAgainStopTheRun()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            for (int i = 1; i <= 50; i++)
            {
                guard.RecordFailure(ImageFailure.BecauseItWasNotFoundAgain("Clash" + i + " was not among its siblings").Reason);
            }

            Assert.That(guard.ShouldStopTheRun, Is.True);
            Assert.That(guard.FirstReason, Is.EqualTo(ImageFailure.ClashNotFoundAgain));
        }
    }
}
