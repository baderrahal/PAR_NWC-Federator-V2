using System.Collections.Generic;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>F182, Bader's answer B to Q175. The lines that say which step ended last.</summary>
    [TestFixture]
    public class GroupOrderTests
    {
        [Test]
        public void TheLineBeforeThePicturesSaysBothFilesAreAlreadyWritten()
        {
            string line = GroupOrder.WrittenBeforeThePictures("1A04PK", 11600000, 480000000, true);

            Assert.That(line, Does.StartWith("ORDER    1A04PK: the NWF is 11,600,000 bytes and the NWD is 480,000,000 bytes"));
            Assert.That(line, Does.Contain("both before the pictures"));
            Assert.That(line, Does.Contain("If they stop early or fail, both files are already written"));
        }

        [Test]
        public void AFileNotOnDiskIsSaidAsNotOnDiskAndNeverAsAByteCount()
        {
            string line = GroupOrder.WrittenBeforeThePictures("1A04PK", 100, -1, false);

            Assert.That(line, Does.Contain("the NWD is NOT ON DISK"));
            Assert.That(line, Does.Contain("No picture is taken in this group"));
        }

        [Test]
        public void TheEndOfThePicturesIsSaidAsAStopAnEndOrAFailure()
        {
            Assert.That(GroupOrder.PicturesEnded("G", false, false), Does.Contain("at the end of its rows"));
            Assert.That(GroupOrder.PicturesEnded("G", true, false), Does.Contain("early at a stop, which is a normal end"));
            Assert.That(GroupOrder.PicturesEnded("G", true, true), Does.Contain("with a failure"));
            Assert.That(GroupOrder.PicturesEnded("G", false, false), Does.Contain("the last step to end was PICTURES"));
        }

        [Test]
        public void ThePicturesEndedBeforeTheNwdIsNamedAsAFaultAndTheRightOrderIsNot()
        {
            List<string> right = new List<string>
            {
                RunSteps.Views, RunSteps.NwfSave, RunSteps.Nwd, RunSteps.Confirm, RunSteps.Pictures, RunSteps.Workbook
            };
            List<string> wrong = new List<string>
            {
                RunSteps.Views, RunSteps.Pictures, RunSteps.Nwd, RunSteps.Confirm
            };

            Assert.That(GroupOrder.FaultIn(right), Is.Null);
            Assert.That(GroupOrder.FaultIn(wrong), Does.Contain("picture step ended before the NWD"));
            Assert.That(GroupOrder.FaultIn(new List<string> { RunSteps.Views, RunSteps.Nwd }), Is.Null, "no picture step is no fault");
            Assert.That(GroupOrder.FaultIn(null), Is.Null);
        }

        [Test]
        public void TheEndedLineListsTheStepsInTheOrderTheyEnded()
        {
            string line = GroupOrder.EndedInOrder("1A04PK", new List<string> { "VIEWS", "NWD", "PICTURES" });

            Assert.That(line, Is.EqualTo("ORDER    1A04PK: the steps ended in this order, VIEWS, NWD, PICTURES"));
            Assert.That(GroupOrder.EndedInOrder("", null), Is.EqualTo("ORDER    this group: the steps ended in this order, none"));
        }
    }
}
