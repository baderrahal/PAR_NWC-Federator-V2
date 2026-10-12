using System.Collections.Generic;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Navisworks' texture cache kept from growing across a long run, F147, Bader's order of
    /// 2026-10-11: cleared between tests once it passes five gigabytes, its size said before and
    /// after. The watch is tested on a folder of its own with a limit of a few bytes, since the
    /// rule is the same at any size.
    /// </summary>
    [TestFixture]
    public class TextureCacheTests
    {
        private string folder;
        private double now;
        private List<string> said;

        [SetUp]
        public void MakeTheFolder()
        {
            folder = TempFolder.Make("texture-cache");
            now = 0;
            said = new List<string>();
        }

        [TearDown]
        public void RemoveTheFolder()
        {
            TempFolder.Remove(folder);
        }

        // ---------- the rule ----------

        [Test]
        public void TheDefaultsAreFiveGigabytesAndOneMinute()
        {
            Assert.That(new ImageOptions().TextureCacheLimitBytes, Is.EqualTo(5L * 1024 * 1024 * 1024));
            Assert.That(new ImageOptions().TextureCacheCheckEverySeconds, Is.EqualTo(60));
            Assert.That(TextureCache.Size(TextureCache.DefaultLimitBytes), Is.EqualTo("5.00 GB"));
        }

        [Test]
        public void OnlyASizeReadAndPastTheLimitClears()
        {
            Assert.That(TextureCache.ShouldClear(101, 100), Is.True);
            Assert.That(TextureCache.ShouldClear(100, 100), Is.False, "at the limit is not past it");
            Assert.That(TextureCache.ShouldClear(-1, 100), Is.False, "a size not read never clears anything");
            Assert.That(TextureCache.ShouldClear(long.MaxValue, 0), Is.False, "a limit of zero switches the clearing off");
            Assert.That(TextureCache.ShouldClear(long.MaxValue, -5), Is.False);
        }

        [Test]
        public void TheFirstCheckIsAlwaysDueAndTheNextOnlyOnceTheIntervalHasPassed()
        {
            Assert.That(TextureCache.DueToMeasure(false, 0, 60), Is.True);
            Assert.That(TextureCache.DueToMeasure(true, 59.9, 60), Is.False);
            Assert.That(TextureCache.DueToMeasure(true, 60, 60), Is.True);
            Assert.That(TextureCache.DueToMeasure(true, 0, 0), Is.True, "an interval of zero checks between every two tests");
        }

        [Test]
        public void ASizeIsInMegabytesBelowAGigabyteAndUnknownWhenNotRead()
        {
            Assert.That(TextureCache.Size(-1), Is.EqualTo("UNKNOWN"));
            Assert.That(TextureCache.Size(0), Is.EqualTo("0.0 MB"));
            Assert.That(TextureCache.Size(512L * 1024 * 1024), Is.EqualTo("512.0 MB"));
            Assert.That(TextureCache.Size(3L * 1024 * 1024 * 1024 / 2), Is.EqualTo("1.50 GB"));
        }

        // ---------- the watch ----------

        [Test]
        public void ACachePastTheLimitIsClearedBetweenTestsAndItsSizeSaidBeforeAndAfter()
        {
            string inner = Directory.CreateDirectory(Path.Combine(folder, "shaders")).FullName;
            Write(Path.Combine(folder, "a.bin"), 100);
            Write(Path.Combine(inner, "b.bin"), 100);
            Write(Path.Combine(inner, "c.bin"), 100);

            TextureCacheWatch watch = Watch(250, 60);
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder, "*", SearchOption.AllDirectories), Is.Empty);
            Assert.That(Directory.Exists(inner), Is.True, "a folder is never removed, only the files under it");
            Assert.That(said.Count, Is.EqualTo(1));
            Assert.That(said[0], Does.Contain("held 0.0 MB in 3 files, past the limit of 0.0 MB"));
            Assert.That(said[0], Does.Contain("3 files were removed"));
            Assert.That(said[0], Does.EndWith("and it holds 0.0 MB now"));
            Assert.That(watch.Cleared, Is.EqualTo(1));
        }

        [Test]
        public void ACacheUnderTheLimitIsLeftWhole()
        {
            Write(Path.Combine(folder, "a.bin"), 100);
            Write(Path.Combine(folder, "b.bin"), 100);

            TextureCacheWatch watch = Watch(200, 60);
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(2), "at the limit is not past it");
            Assert.That(said, Is.Empty);
            Assert.That(watch.AtTheEnd(), Does.Contain("cleared 0 times"));
        }

        // The check that guards the long run's own speed: within the interval nothing is walked,
        // so a cache past the limit waits for the next check rather than being walked per test.
        [Test]
        public void WithinTheIntervalTheCacheIsNotWalkedAgain()
        {
            TextureCacheWatch watch = Watch(150, 60);
            watch.AtTheStart();

            Write(Path.Combine(folder, "a.bin"), 100);
            Write(Path.Combine(folder, "b.bin"), 100);
            now = 30;
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(2), "30 s into a 60 s interval, so not checked");
            Assert.That(said, Is.Empty);

            now = 61;
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder), Is.Empty, "the interval has passed, so checked and cleared");
            Assert.That(said.Count, Is.EqualTo(1));
        }

        [Test]
        public void ALimitOfZeroWalksNothingAndSaysTheClearingIsOff()
        {
            Write(Path.Combine(folder, "a.bin"), 100);

            TextureCacheWatch watch = Watch(0, 0);
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(1));
            Assert.That(watch.AtTheEnd(), Does.EndWith("is not checked, its limit is off"));
        }

        [Test]
        public void NoFolderIsSaidAndNothingThrows()
        {
            TempFolder.Remove(folder);

            TextureCacheWatch watch = Watch(1, 0);
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(said, Is.Empty);
            Assert.That(watch.AtTheEnd(), Does.EndWith("is not there, so nothing was cleared"));
        }

        [Test]
        public void TheEndLineGivesTheSizesAndTheCountsOfThePass()
        {
            Write(Path.Combine(folder, "a.bin"), 300);

            TextureCacheWatch watch = Watch(200, 0);
            watch.AtTheStart();
            Write(Path.Combine(folder, "b.bin"), 50);
            string line = watch.AtTheEnd();

            Assert.That(line, Does.StartWith("the texture cache under %TEMP%\\ogs held 0.0 MB as the pictures began and 0.0 MB as they ended"));
            Assert.That(line, Does.Contain("cleared 1 time"));
            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(1), "the file written after the clearing is still there");
        }

        [Test]
        public void AFileInUseIsLeftAndCounted()
        {
            TestPaths.OnWindowsOnly("share locking");

            Write(Path.Combine(folder, "a.bin"), 100);
            string held = Path.Combine(folder, "held.bin");
            Write(held, 100);

            using (new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Watch(150, 0).BetweenTests();
            }

            Assert.That(File.Exists(held), Is.True);
            Assert.That(said.Count, Is.EqualTo(1));
            Assert.That(said[0], Does.Contain("1 file was removed and 1 file was in use and left"));
        }

        [Test]
        public void TheFolderIsOgsUnderTheTempFolder()
        {
            Assert.That(TextureCacheWatch.FolderIn(folder), Is.EqualTo(Path.Combine(folder, "ogs")));
        }

        private TextureCacheWatch Watch(long limit, int every)
        {
            return new TextureCacheWatch(folder, limit, every, () => now, line => said.Add(line));
        }

        private static void Write(string path, int bytes)
        {
            File.WriteAllBytes(path, new byte[bytes]);
        }
    }
}
