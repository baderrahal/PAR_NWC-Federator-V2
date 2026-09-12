using System;
using System.Collections.Generic;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The settings the pictures run under, and what they cost.
    ///
    /// The defaults are what the brief asks for: images on, no cap, New Active and
    /// Reviewed only, and a size that came off the accepted report rather than out of the
    /// air.
    /// </summary>
    [TestFixture]
    public class ImageOptionsTests
    {
        // ---------- the defaults ----------

        // The one the brief asks for by name.
        [Test]
        public void ImagesAreOnByDefault()
        {
            Assert.That(new ImageOptions().Write, Is.True);
            Assert.That(new ReportOptions().Images.Write, Is.True);
        }

        [Test]
        public void TheCapIsOffByDefault()
        {
            ImageOptions options = new ImageOptions();

            Assert.That(options.CapPerTest, Is.EqualTo(0));
            Assert.That(options.RoomFor(0), Is.True);
            Assert.That(options.RoomFor(1244), Is.True, "no cap should mean no cap");
        }

        // 1024 by 1024 is what all 60 pictures of the accepted report are, measured.
        [Test]
        public void TheSizeIsTheOneTheClientAlreadyHas()
        {
            Assert.That(ImageOptions.DefaultPixels, Is.EqualTo(1024));
            Assert.That(new ImageOptions().Width, Is.EqualTo(1024));
            Assert.That(new ImageOptions().Height, Is.EqualTo(1024));
        }

        [Test]
        public void ThumbnailsAreOffBecauseTheAcceptedReportLinksRatherThanPasting()
        {
            Assert.That(new ImageOptions().EmbedThumbnail, Is.False);
        }

        [Test]
        public void FiftyFailuresStopTheRunTheSameWayTheClashStepDoes()
        {
            Assert.That(ImageOptions.DefaultStopAfterFailures, Is.EqualTo(50));
            Assert.That(ImageOptions.DefaultStopAfterFailures,
                Is.EqualTo(RepeatedFailureGuard.DefaultThreshold));
        }

        // ---------- the status filter ----------

        // The one the brief asks for by name.
        [Test]
        public void TheFilterDefaultsToNewActiveAndReviewed()
        {
            ImageOptions options = new ImageOptions();

            Assert.That(options.Wants(ClashStatus.New), Is.True);
            Assert.That(options.Wants(ClashStatus.Active), Is.True);
            Assert.That(options.Wants(ClashStatus.Reviewed), Is.True);
            Assert.That(options.Wants(ClashStatus.Approved), Is.False);
            Assert.That(options.Wants(ClashStatus.Resolved), Is.False);

            Assert.That(options.ChosenStatuses(), Is.EqualTo(new[]
            {
                ClashStatus.New, ClashStatus.Active, ClashStatus.Reviewed
            }));
        }

        [Test]
        public void TheFilterCanBeNarrowedAndWidened()
        {
            ImageOptions options = new ImageOptions();
            options.OnlyFor(new[] { ClashStatus.New });

            Assert.That(options.Wants(ClashStatus.New), Is.True);
            Assert.That(options.Wants(ClashStatus.Active), Is.False);
            Assert.That(options.Wants(ClashStatus.Reviewed), Is.False);

            options.OnlyFor(ClashTally.AllStatuses);

            foreach (ClashStatus status in ClashTally.AllStatuses)
            {
                Assert.That(options.Wants(status), Is.True, status.ToString());
            }
        }

        [Test]
        public void ImagesOffBeatsTheFilter()
        {
            ImageOptions options = new ImageOptions();
            options.Write = false;

            Assert.That(options.Wants(ClashStatus.New), Is.False);
        }

        // An empty filter and images off are the same thing, and one of them says so in
        // the log. So the empty one is refused rather than quietly writing nothing.
        [Test]
        public void AnEmptyFilterIsRefusedRatherThanSilentlyWritingNothing()
        {
            Assert.Throws<ArgumentException>(
                delegate { new ImageOptions().OnlyFor(new List<ClashStatus>()); });
            Assert.Throws<ArgumentNullException>(
                delegate { new ImageOptions().OnlyFor(null); });
        }

        // ---------- the cap ----------

        // The one the brief asks for by name.
        [Test]
        public void ACapSelectsTheFirstOnesAndStops()
        {
            ImageOptions options = new ImageOptions();
            options.CapPerTest = 3;

            Assert.That(options.RoomFor(0), Is.True);
            Assert.That(options.RoomFor(2), Is.True);
            Assert.That(options.RoomFor(3), Is.False, "the fourth is over a cap of three");
            Assert.That(options.RoomFor(1243), Is.False);
        }

        [Test]
        public void ACapOfOneMeansOne()
        {
            ImageOptions options = new ImageOptions();
            options.CapPerTest = 1;

            Assert.That(options.RoomFor(0), Is.True);
            Assert.That(options.RoomFor(1), Is.False);
        }

        // ---------- it says what it will do ----------

        [Test]
        public void TheSettingsSayWhatTheyWillDoInWordsAPersonWouldUse()
        {
            string said = new ImageOptions().Describe();

            Assert.That(said, Does.Contain("1024 by 1024"));
            Assert.That(said, Does.Contain("New, Active, Reviewed"));
            Assert.That(said, Does.Contain("no cap per test"));
            Assert.That(said, Does.Contain("linked rather than pasted in"));
        }

        [Test]
        public void OffSaysOffRatherThanShowingSettingsThatWillNotRun()
        {
            ImageOptions options = new ImageOptions();
            options.Write = false;

            Assert.That(options.Describe(), Does.Contain("Images are off"));
            Assert.That(options.Describe(), Does.Not.Contain("1024"));
        }

        [Test]
        public void ACapSaysTheNumber()
        {
            ImageOptions options = new ImageOptions();
            options.CapPerTest = 5;

            Assert.That(options.Describe(), Does.Contain("at most 5 per test"));
        }

        // ---------- what they cost, which is the whole of job four ----------

        [Test]
        public void NothingWrittenSaysSoRatherThanPrintingARateOverZero()
        {
            ImageTally tally = new ImageTally();

            Assert.That(tally.Written, Is.EqualTo(0));
            Assert.That(tally.SecondsEach, Is.EqualTo(0.0));
            Assert.That(tally.Quickest, Is.EqualTo(0.0));
            Assert.That(string.Join("\n", Lines(tally)), Does.Contain("none written"));
        }

        // The one the brief asks for by name. Seconds per image, total seconds, the count
        // and the megabytes, all measured.
        [Test]
        public void EveryNumberTheBriefAsksForIsThere()
        {
            ImageTally tally = new ImageTally();
            tally.Wrote(0.4, 130648);
            tally.Wrote(0.6, 63760);
            tally.Wrote(0.5, 182790);

            Assert.That(tally.Written, Is.EqualTo(3));
            Assert.That(tally.TotalSeconds, Is.EqualTo(1.5).Within(0.0001));
            Assert.That(tally.SecondsEach, Is.EqualTo(0.5).Within(0.0001));
            Assert.That(tally.TotalBytes, Is.EqualTo(377198));
            Assert.That(tally.TotalMegabytes, Is.EqualTo(377198 / 1048576.0).Within(0.0001));
            Assert.That(tally.Quickest, Is.EqualTo(0.4).Within(0.0001));
            Assert.That(tally.Slowest, Is.EqualTo(0.6).Within(0.0001));

            string block = string.Join("\n", Lines(tally));

            Assert.That(block, Does.Contain("3 written"));
            Assert.That(block, Does.Contain("0.36 MB"));
            Assert.That(block, Does.Contain("1.5 seconds in total"));
            Assert.That(block, Does.Contain("0.500 seconds each"));
        }

        [Test]
        public void WhatWasPassedOverIsCountedApartFromWhatFailed()
        {
            ImageTally tally = new ImageTally();
            tally.Wrote(0.5, 1000);
            tally.WrongStatus();
            tally.WrongStatus();
            tally.CappedOne();
            tally.RenderFailed("Clash7", 0.2);

            Assert.That(tally.SkippedByStatus, Is.EqualTo(2));
            Assert.That(tally.SkippedByCap, Is.EqualTo(1));
            Assert.That(tally.Failed, Is.EqualTo(1));
            Assert.That(tally.Written, Is.EqualTo(1));

            // The time a failure wasted still counts, because a run where every image
            // fails slowly is exactly the one worth measuring.
            Assert.That(tally.TotalSeconds, Is.EqualTo(0.7).Within(0.0001));

            string block = string.Join("\n", Lines(tally));

            Assert.That(block, Does.Contain("2 passed over on status, 1 on the cap"));
            Assert.That(block, Does.Contain("1 failed"));
            Assert.That(block, Does.Contain("Clash7"));
        }

        [Test]
        public void ManyFailuresShowFiveAndThenTheTotal()
        {
            ImageTally tally = new ImageTally();

            for (int i = 1; i <= 12; i++)
            {
                tally.RenderFailed("Clash" + i, 0.1);
            }

            string block = string.Join("\n", Lines(tally));

            Assert.That(block, Does.Contain("12 failed"));
            Assert.That(block, Does.Contain("Clash5"));
            Assert.That(block, Does.Not.Contain("Clash6"));
            Assert.That(block, Does.Contain("and 7 more"));
        }

        [Test]
        public void GroupTalliesAddUp()
        {
            ImageTally one = new ImageTally();
            one.Wrote(0.4, 100);

            ImageTally two = new ImageTally();
            two.Wrote(0.8, 200);
            two.RenderFailed("Clash1", 0.1);

            one.Add(two);

            Assert.That(one.Written, Is.EqualTo(2));
            Assert.That(one.TotalBytes, Is.EqualTo(300));
            Assert.That(one.Failed, Is.EqualTo(1));
            Assert.That(one.Slowest, Is.EqualTo(0.8).Within(0.0001));
            Assert.That(one.Quickest, Is.EqualTo(0.4).Within(0.0001));
        }

        [Test]
        public void AddingNothingChangesNothing()
        {
            ImageTally tally = new ImageTally();
            tally.Wrote(0.5, 100);
            tally.Add(null);

            Assert.That(tally.Written, Is.EqualTo(1));
        }

        private static IList<string> Lines(ImageTally tally)
        {
            return tally.Lines();
        }
    }
}
