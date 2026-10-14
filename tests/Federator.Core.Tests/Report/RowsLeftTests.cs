using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F149, the second reading. One fault, a test not found again once every test has run or a
    /// throw that ends a stretch of pictures, leaves the rest of the stretch with no picture asked
    /// for. Each row left is counted on the IMAGES tally as the writer would have counted it had it
    /// been asked for, but the pictures' guard is told ONCE, for the one fault. Told once per row,
    /// one ObjectDisposedException at row 11 of a 1556 row test fired the fifty in a row guard and
    /// stopped the run saying fifty images failed when not one render was tried.
    /// </summary>
    [TestFixture]
    public class RowsLeftTests
    {
        private ClashReport report;
        private TestReport test;

        [SetUp]
        public void MakeTheTest()
        {
            report = new ClashReport("1A04PK", "out");
            test = report.AddTest("BLD-AR-Walls-vs-BLD-AR-Floors");
            test.PicturePriority = ClashPriority.A;
        }

        private List<ClashRow> Rows(int count, ClashStatus status)
        {
            List<ClashRow> rows = new List<ClashRow>();

            for (int i = 1; i <= count; i++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + i;
                row.Status = status;
                test.Add(row);
                rows.Add(row);
            }

            return rows;
        }

        // The break. Sixty rows left by one fault are sixty failures on the tally and one on the guard,
        // so the guard of fifty does not fire on a fault that tried no render.
        [Test]
        public void SixtyRowsLeftByOneFaultTellTheGuardOnce()
        {
            List<ClashRow> rows = Rows(60, ClashStatus.New);
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, guard);

            Assert.That(counted.Failed.Count, Is.EqualTo(60));
            Assert.That(report.Images.Failed, Is.EqualTo(60), "every row left is on the IMAGES tally");
            Assert.That(counted.GuardTold, Is.True);
            Assert.That(guard.Consecutive, Is.EqualTo(1), "one fault, one failure on the guard");
            Assert.That(guard.ShouldStopTheRun, Is.False);
        }

        // Fifty faults of one reason in a row are still fifty failures, so the guard still stops a run
        // whose stretches all fail the same way.
        [Test]
        public void FiftyFaultsInARowStillStopTheRun()
        {
            RepeatedFailureGuard guard = new RepeatedFailureGuard();
            ImageOptions options = new ImageOptions();

            for (int i = 0; i < 50; i++)
            {
                options.CountTheRowsLeft(report, test, Rows(3, ClashStatus.New), 0, ImageFailure.TestNotFoundAgain, guard);
            }

            Assert.That(guard.ShouldStopTheRun, Is.True);
            Assert.That(guard.FirstReason, Is.EqualTo(ImageFailure.TestNotFoundAgain));
        }

        // Only the rows from the one the fault met on are counted, the ones before it having had their turn.
        [Test]
        public void OnlyTheRowsFromTheFaultOnAreCounted()
        {
            List<ClashRow> rows = Rows(5, ClashStatus.New);

            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, rows, 3, ImageFailure.TestNotFoundAgain, null);

            Assert.That(counted.Failed.Count, Is.EqualTo(2));
            Assert.That(counted.Failed[0].Name, Is.EqualTo("Clash4"));
            Assert.That(counted.Failed[1].Name, Is.EqualTo("Clash5"));
            Assert.That(counted.GuardTold, Is.False, "a run with no guard tells none");
        }

        [Test]
        public void ARowOfAStatusNotAskedForIsPassedOverOnItsStatus()
        {
            List<ClashRow> rows = Rows(2, ClashStatus.New);
            rows.AddRange(Rows(3, ClashStatus.Resolved));

            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, null);

            Assert.That(counted.Failed.Count, Is.EqualTo(2));
            Assert.That(counted.PassedOverOnStatus, Is.EqualTo(3));
            Assert.That(report.Images.SkippedByStatus, Is.EqualTo(3));
            Assert.That(report.Images.Failed, Is.EqualTo(2));
        }

        // The second reading's point B. Under a cap the rows left take the room as the writer's pictures
        // would have: as many as the room as failed, the rest passed over on the cap. A test with one
        // picture under a cap of three has room for two.
        [Test]
        public void UnderACapTheRoomIsCountedAsFailedAndTheRestAsCapped()
        {
            ClashRow pictured = Rows(1, ClashStatus.New)[0];
            pictured.ImageFile = "cd000001.jpg";
            List<ClashRow> rows = Rows(5, ClashStatus.New);
            ImageOptions options = new ImageOptions();
            options.CapPerTest = 3;

            RowsLeftCount counted = options.CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, null);

            Assert.That(counted.Failed.Count, Is.EqualTo(2));
            Assert.That(counted.PassedOverOnTheCap, Is.EqualTo(3));
            Assert.That(report.Images.Failed, Is.EqualTo(2));
            Assert.That(report.Images.SkippedByCap, Is.EqualTo(3));
        }

        [Test]
        public void AFiredGuardCountsNothingAndIsToldNothing()
        {
            List<ClashRow> rows = Rows(4, ClashStatus.New);
            RepeatedFailureGuard guard = new RepeatedFailureGuard(1);
            guard.RecordFailure("before");

            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, guard);

            Assert.That(counted.Failed.Count, Is.EqualTo(0));
            Assert.That(counted.GuardTold, Is.False);
            Assert.That(report.Images.Failed, Is.EqualTo(0));
            Assert.That(guard.FirstReason, Is.EqualTo("before"));
        }

        [Test]
        public void PicturesOffCountNothing()
        {
            List<ClashRow> rows = Rows(4, ClashStatus.New);
            ImageOptions options = new ImageOptions();
            options.Write = false;

            RowsLeftCount counted = options.CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, null);

            Assert.That(counted.Failed.Count, Is.EqualTo(0));
            Assert.That(report.Images.Failed, Is.EqualTo(0));
            Assert.That(report.Images.SkippedByStatus, Is.EqualTo(0));
        }

        // A row that already has its picture was taken and is none of the rows left.
        [Test]
        public void ARowWithItsPictureIsNotCounted()
        {
            List<ClashRow> rows = Rows(3, ClashStatus.New);
            rows[0].ImageFile = "cd000001.jpg";

            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, null);

            Assert.That(counted.Failed.Count, Is.EqualTo(2));
            Assert.That(counted.Failed[0].Name, Is.EqualTo("Clash2"));
        }

        // A fault that leaves no row asked for, every one passed over, tells the guard nothing.
        [Test]
        public void AFaultLeavingNoRowAskedForTellsTheGuardNothing()
        {
            List<ClashRow> rows = Rows(3, ClashStatus.Resolved);
            RepeatedFailureGuard guard = new RepeatedFailureGuard();

            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, rows, 0, ImageFailure.TestNotFoundAgain, guard);

            Assert.That(counted.GuardTold, Is.False);
            Assert.That(guard.Consecutive, Is.EqualTo(0));
        }

        [Test]
        public void TheCountHandedOutCannotBeAddedTo()
        {
            RowsLeftCount counted = new ImageOptions().CountTheRowsLeft(report, test, Rows(1, ClashStatus.New), 0, ImageFailure.TestNotFoundAgain, null);

            Assert.That(counted.Failed.IsReadOnly, Is.True);
        }
    }
}
