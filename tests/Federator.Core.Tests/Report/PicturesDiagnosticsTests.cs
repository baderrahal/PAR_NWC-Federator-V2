using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// What the picture pass leaves behind for a run that dies, F157. A run died at picture 8775 of
    /// 10206 with no memory line since the start of the step and no time on its last two lines.
    /// </summary>
    [TestFixture]
    public class PicturesDiagnosticsTests
    {
        private const long Megabyte = 1024L * 1024;

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorPicturesDiagnostics");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        [Test]
        public void TheMemoryLineCarriesTheThreeSizesAndTheGdiAndUserCounts()
        {
            ProcessMemory at = new ProcessMemory(2000 * Megabyte, 1200 * Megabyte, 130 * Megabyte, 9876, 321, null);

            Assert.That(
                ProcessMemory.PicturesLine("250 of 10206 pictures", at),
                Is.EqualTo("MEMORY   PICTURES 250 of 10206 pictures, private bytes 2000.0 MB, working set 1200.0 MB, "
                    + "managed heap 130.0 MB, GDI objects 9876, USER objects 321"));
        }

        [Test]
        public void ACountNotReadIsUnknownAndNeverNought()
        {
            ProcessMemory at = new ProcessMemory(10 * Megabyte, 10 * Megabyte, 10 * Megabyte, "denied");

            string line = ProcessMemory.PicturesLine("at the start", at);

            Assert.That(line, Does.Contain("GDI objects UNKNOWN, USER objects UNKNOWN"));
            Assert.That(line, Does.EndWith(", UNKNOWN because denied"));
        }

        [Test]
        public void AReadOfThisProcessGivesGdiAndUserCountsOnWindows()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Assert.Ignore("GetGuiResources is a Windows call.");
            }

            ProcessMemory now = ProcessMemory.Read();

            Assert.That(now.GdiObjects, Is.GreaterThanOrEqualTo(0));
            Assert.That(now.UserObjects, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void TheWatchIsDueOnceAtEveryMultipleAndNotBefore()
        {
            PictureWatch watch = new PictureWatch(250, 0);

            Assert.That(watch.Due(1), Is.False);
            Assert.That(watch.Due(249), Is.False);
            Assert.That(watch.Due(250), Is.True);

            watch.RateLine(250, 10206, 60);

            Assert.That(watch.Due(250), Is.False, "the same count is not due twice");
            Assert.That(watch.Due(499), Is.False);
            Assert.That(watch.Due(500), Is.True);
        }

        [Test]
        public void ACountThatJumpsPastAMultipleIsDueOnceNotOncePerMultiple()
        {
            PictureWatch watch = new PictureWatch(250, 0);

            Assert.That(watch.Due(620), Is.True);
            watch.RateLine(620, 10206, 100);
            Assert.That(watch.Due(620), Is.False);
            Assert.That(watch.Due(749), Is.False);
            Assert.That(watch.Due(750), Is.True);
        }

        [Test]
        public void TheRateLineGivesTheWindowAndTheWholePass()
        {
            PictureWatch watch = new PictureWatch(250, 100);

            string first = watch.RateLine(250, 10206, 150);
            string second = watch.RateLine(500, 10206, 250);

            Assert.That(first, Is.EqualTo(
                "RATE     PICTURES 250 of 10206 done, 250 in the last 50.0 s, 5.00 per second, and 5.00 per second since the pass began"));
            Assert.That(second, Does.Contain("500 of 10206 done, 250 in the last 100.0 s, 2.50 per second, and 3.33 per second since the pass began"));
        }

        [Test]
        public void ARateWithNoTimePassedIsUnknownAndNeverADivisionByZero()
        {
            PictureWatch watch = new PictureWatch(250, 5);

            Assert.That(watch.RateLine(250, 0, 5), Does.Contain("UNKNOWN per second"));
        }

        [Test]
        public void AWatchOfLessThanOneIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PictureWatch(0, 0));
        }

        [Test]
        public void TwoClashesForOneFileAreWarnedWithBothNamesAndTheFile()
        {
            PictureNameWatch names = new PictureNameWatch();
            string path = @"C:\out\Report_files\cd300035.jpg";

            Assert.That(names.Take(path, "Test A", "Clash35"), Is.Null);
            Assert.That(names.Take(@"C:\out\Report_files\cd300036.jpg", "Test A", "Clash36"), Is.Null);

            string warning = names.Take(path.ToUpperInvariant(), "Test A", "Clash36");

            Assert.That(warning, Does.StartWith("WARNING  "));
            Assert.That(warning, Does.Contain("Clash35"));
            Assert.That(warning, Does.Contain("Clash36"));
            Assert.That(warning, Does.Contain("Test A"));
            Assert.That(warning, Does.Contain("cd300035.jpg").IgnoreCase);
        }

        [Test]
        public void AFailureKeepsTheWholeExceptionWithItsStackTraceAndTheGuardStillSeesOneReason()
        {
            ImageFailure one = Thrown(@"C:\out\Report_files\cd300035.jpg");
            ImageFailure two = Thrown(@"C:\out\Report_files\cd300036.jpg");

            Assert.That(one.Full, Does.Contain("ArgumentException"));
            Assert.That(one.Full, Does.Contain("Parameter is not valid"));
            Assert.That(one.Full, Does.Contain("   at "), "a stack trace is in it");
            Assert.That(one.Reason, Is.EqualTo(two.Reason), "the path is not in the reason, so fifty are one reason");
        }

        [Test]
        public void ALineIsOnTheDiskTheMomentItIsWritten()
        {
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 14, 2, 3, 23)))
            {
                log.Line("IMAGE    failed for Clash35 in Test A. ArgumentException");

                using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    Assert.That(reader.ReadToEnd(), Does.Contain("IMAGE    failed for Clash35 in Test A"));
                }
            }
        }

        private static ImageFailure Thrown(string path)
        {
            try
            {
                throw new ArgumentException("Parameter is not valid. " + path);
            }
            catch (ArgumentException error)
            {
                return ImageFailure.BecauseItThrewWriting(error, path);
            }
        }
    }
}
