using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Navisworks' texture cache kept from growing across a long run, F147, Bader's order of
    /// 2026-10-11: cleared between tests once it passes five gigabytes, its size said before and
    /// after. The watch is tested on a folder named ogs of its own, with files of whole megabytes
    /// against a limit of a few, since the rule is the same at any size.
    /// </summary>
    [TestFixture]
    public class TextureCacheTests
    {
        private const int Megabyte = 1024 * 1024;

        private string temp;
        private string folder;
        private double now;
        private List<string> said;

        [SetUp]
        public void MakeTheFolder()
        {
            temp = TempFolder.Make("texture-cache");
            folder = Directory.CreateDirectory(Path.Combine(temp, "ogs")).FullName;
            now = 0;
            said = new List<string>();
        }

        [TearDown]
        public void RemoveTheFolder()
        {
            TempFolder.Remove(temp);
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
        public void ASizeTakesTheUnitItsRoundedFigureReaches()
        {
            Assert.That(TextureCache.Size(-1), Is.EqualTo("UNKNOWN"));
            Assert.That(TextureCache.Size(0), Is.EqualTo("0 bytes"));
            Assert.That(TextureCache.Size(1), Is.EqualTo("1 byte"));
            Assert.That(TextureCache.Size(40 * 1024), Is.EqualTo("40.0 KB"));
            Assert.That(TextureCache.Size(512L * Megabyte), Is.EqualTo("512.0 MB"));
            Assert.That(TextureCache.Size(1024L * 1024 * 1024 - 1), Is.EqualTo("1.00 GB"), "never 1024.0 MB");
            Assert.That(TextureCache.Size(3L * 1024 * 1024 * 1024 / 2), Is.EqualTo("1.50 GB"));
            Assert.That(TextureCache.Exactly(5L * 1024 * 1024 * 1024 + 1), Is.EqualTo("5.00 GB (5368709121 bytes)"),
                "a size just past the limit is told apart from it by its bytes");
        }

        [Test]
        public void OnlyARootedFolderNamedOgsIsACacheFolder()
        {
            Assert.That(TextureCache.IsACacheFolder(folder), Is.True);
            Assert.That(TextureCache.IsACacheFolder(folder + Path.DirectorySeparatorChar), Is.True);
            Assert.That(TextureCache.IsACacheFolder(temp), Is.False, "the temp folder itself is never a cache folder");
            Assert.That(TextureCache.IsACacheFolder("ogs"), Is.False, "a relative path is refused");
            Assert.That(TextureCache.IsACacheFolder(null), Is.False);
            Assert.That(TextureCache.IsACacheFolder(string.Empty), Is.False);
            Assert.That(TextureCache.IsACacheFolder(Path.Combine(temp, "ogs2")), Is.False);
            Assert.That(TextureCacheWatch.FolderIn(temp), Is.EqualTo(Path.Combine(temp, "ogs")));
            Assert.That(TextureCacheWatch.FolderIn(string.Empty), Is.Null, "no temp folder gives no cache folder, never a relative ogs");
        }

        // ---------- the watch ----------

        [Test]
        public void ACachePastTheLimitIsClearedAndItsSizeSaidBeforeAndAfter()
        {
            string inner = Directory.CreateDirectory(Path.Combine(folder, "shaders")).FullName;
            Write(Path.Combine(folder, "a.bin"), 1 * Megabyte);
            Write(Path.Combine(inner, "b.bin"), 2 * Megabyte);

            Watch(2 * Megabyte, 60).BetweenTests();

            Assert.That(Directory.GetFiles(folder, "*", SearchOption.AllDirectories), Is.Empty);
            Assert.That(Directory.Exists(inner), Is.True, "a folder is never removed, only the files under it");
            Assert.That(said.Count, Is.EqualTo(1));
            Assert.That(said[0], Does.StartWith("the texture cache at " + folder + " held 3.0 MB (3145728 bytes) in 2 files, past the limit of 2.0 MB (2097152 bytes), so 2 files were removed"));
            Assert.That(said[0], Does.Contain("and it holds 0 bytes in 0 files now"));
            Assert.That(said[0], Does.Not.Contain("not cleared again"));
        }

        [Test]
        public void ACacheUnderTheLimitIsLeftWholeAtEveryCheck()
        {
            Write(Path.Combine(folder, "a.bin"), 1 * Megabyte);
            Write(Path.Combine(folder, "b.bin"), 1 * Megabyte);

            TextureCacheWatch watch = Watch(2 * Megabyte, 60);
            watch.AtTheStart();
            now = 61;
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(2), "at the limit is not past it");
            Assert.That(said, Is.Empty);
            Assert.That(watch.AtTheEnd(), Does.Contain("held 2.0 MB in 2 files as the pictures began and 2.0 MB in 2 files as they ended, read 3 times against the limit of 2.0 MB, cleared 0 times"));
        }

        // The check that guards the long run's own speed: within the interval nothing is walked, so a
        // cache past the limit waits for the next check, and the interval runs from the last check.
        [Test]
        public void WithinTheIntervalTheCacheIsNotWalkedAgain()
        {
            TextureCacheWatch watch = Watch(1 * Megabyte, 60);
            watch.AtTheStart();

            now = 50;
            watch.BetweenTests();
            now = 70;
            watch.BetweenTests();
            Write(Path.Combine(folder, "a.bin"), 2 * Megabyte);

            Assert.That(said, Is.Empty, "70 s after the first check of 0 s, so checked, with the cache still empty");

            now = 80;
            watch.BetweenTests();
            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(1), "10 s after the check at 70 s, so not checked");

            now = 131;
            watch.BetweenTests();
            Assert.That(Directory.GetFiles(folder), Is.Empty, "61 s after the check at 70 s, so checked and cleared");
            Assert.That(said.Count, Is.EqualTo(1));
        }

        [Test]
        public void ALimitOfZeroTouchesNothingAndSaysTheClearingIsOff()
        {
            Write(Path.Combine(folder, "a.bin"), 1 * Megabyte);

            TextureCacheWatch watch = Watch(0, 0);
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(Directory.GetFiles(folder), Has.Length.EqualTo(1));
            Assert.That(said, Is.Empty);
            Assert.That(watch.AtTheEnd(), Is.EqualTo("the texture cache is not checked, its limit is off"));
        }

        [Test]
        public void NoFolderIsSaidAndNothingThrows()
        {
            Directory.Delete(folder);

            TextureCacheWatch watch = Watch(1, 0);
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(said, Is.Empty);
            Assert.That(watch.AtTheEnd(), Is.EqualTo("the texture cache at " + folder + " is not there, so nothing was cleared"));
        }

        // A caller handing the temp folder rather than the cache under it must empty nothing.
        [Test]
        public void AFolderNotNamedOgsIsRefusedAndNothingIsRemoved()
        {
            string beside = Path.Combine(temp, "beside.bin");
            Write(beside, 2 * Megabyte);

            TextureCacheWatch watch = new TextureCacheWatch(temp, 1, 0, () => now, line => said.Add(line));
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(File.Exists(beside), Is.True);
            Assert.That(said.Count, Is.EqualTo(1));
            Assert.That(said[0], Does.EndWith("is not a rooted folder named ogs, so nothing is walked or removed"));
            Assert.That(watch.AtTheEnd(), Does.EndWith("so nothing is walked or removed"));
        }

        [Test]
        public void TheEndReadsOnceMoreAndNeverClears()
        {
            Write(Path.Combine(folder, "a.bin"), 3 * Megabyte);

            TextureCacheWatch watch = Watch(2 * Megabyte, 600);
            watch.AtTheStart();
            Write(Path.Combine(folder, "b.bin"), 3 * Megabyte);
            string line = watch.AtTheEnd();

            Assert.That(File.Exists(Path.Combine(folder, "b.bin")), Is.True, "past the limit at the end, and still not cleared there");
            Assert.That(line, Does.StartWith("the texture cache at " + folder + " held 3.0 MB in 1 file as the pictures began and 3.0 MB in 1 file as they ended"));
            Assert.That(line, Does.Contain("cleared 1 time removing 1 file"));
        }

        [Test]
        public void AFileInUseIsLeftAndTheCacheIsNotClearedAgainOnceAClearingRemovedNothing()
        {
            TestPaths.OnWindowsOnly("share locking");

            string held = Path.Combine(folder, "held.bin");
            Write(held, 3 * Megabyte);

            using (new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                TextureCacheWatch watch = Watch(1 * Megabyte, 0);
                watch.BetweenTests();
                watch.BetweenTests();

                Assert.That(File.Exists(held), Is.True);
                Assert.That(said.Count, Is.EqualTo(1), "one clearing that removed nothing, then none again in the pass");
                Assert.That(said[0], Does.Contain("so 0 files were removed and 1 file could not be"));
                Assert.That(said[0], Does.EndWith("so it is not cleared again in this pass"));
                Assert.That(watch.AtTheEnd(), Does.Contain("cleared 1 time removing 0 files"));
            }
        }

        // The framework's own walk with AllDirectories follows a junction, so a clearing would empty
        // the folder it points at. The watch counts the link and never steps into it.
        [Test]
        public void AFolderLinkedInsideTheCacheIsNeverWalkedOrEmptied()
        {
            TestPaths.OnWindowsOnly("a directory junction");

            string elsewhere = Directory.CreateDirectory(Path.Combine(temp, "elsewhere")).FullName;
            string precious = Path.Combine(elsewhere, "precious.bin");
            Write(precious, 3 * Megabyte);
            Junction(Path.Combine(folder, "link"), elsewhere);
            Write(Path.Combine(folder, "a.bin"), 2 * Megabyte);

            TextureCacheWatch watch = Watch(1 * Megabyte, 0);
            watch.BetweenTests();

            Assert.That(File.Exists(precious), Is.True, "the file behind the link is not the cache's");
            Assert.That(said.Count, Is.EqualTo(1));
            Assert.That(said[0], Does.Contain("held 2.0 MB (2097152 bytes) in 1 file"), "the file behind the link is not counted either");
            Assert.That(watch.AtTheEnd(), Does.Contain("1 linked folder inside it left unwalked"));
        }

        [Test]
        public void ACacheFolderThatIsItselfALinkIsNeverWalked()
        {
            TestPaths.OnWindowsOnly("a directory junction");

            string elsewhere = Directory.CreateDirectory(Path.Combine(temp, "elsewhere")).FullName;
            string precious = Path.Combine(elsewhere, "precious.bin");
            Write(precious, 3 * Megabyte);
            Directory.Delete(folder);
            Junction(folder, elsewhere);

            TextureCacheWatch watch = Watch(1 * Megabyte, 0);
            watch.AtTheStart();
            watch.BetweenTests();

            Assert.That(File.Exists(precious), Is.True);
            Assert.That(said.Count, Is.EqualTo(1));
            Assert.That(said[0], Is.EqualTo("the texture cache at " + folder + " is a link to another folder, so it is not walked and nothing is removed"));
        }

        private TextureCacheWatch Watch(long limit, int every)
        {
            return new TextureCacheWatch(folder, limit, every, () => now, line => said.Add(line));
        }

        private static void Write(string path, int bytes)
        {
            File.WriteAllBytes(path, new byte[bytes]);
        }

        private static void Junction(string link, string target)
        {
            ProcessStartInfo start = new ProcessStartInfo("cmd.exe", "/c mklink /J \"" + link + "\" \"" + target + "\"");
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;

            using (Process made = Process.Start(start))
            {
                made.StandardOutput.ReadToEnd();
                made.WaitForExit();
                Assert.That(made.ExitCode, Is.EqualTo(0), "mklink /J made the junction");
            }
        }
    }
}
