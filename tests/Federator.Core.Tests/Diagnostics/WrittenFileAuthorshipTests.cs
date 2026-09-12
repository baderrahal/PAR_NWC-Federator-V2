using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The result block says "files written, every size read back off the disk". That has
    /// to mean written by THIS run. Outputs overwrite with no date suffix, so last week's
    /// NWF and NWD sit at exactly the paths this run would use, and a group that threw
    /// before writing anything would otherwise list them as its own.
    /// </summary>
    [TestFixture]
    public class WrittenFileAuthorshipTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorAuthorship");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 9, 1, 9, 0, 0));
        }

        private static string ReadWhileOpen(RunLog log)
        {
            using (FileStream stream = new FileStream(
                       log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private string LastWeeksFile(string name)
        {
            string path = Path.Combine(folder, name);
            File.WriteAllText(path, new string('x', 500));
            return path;
        }

        [Test]
        public void CheckingAFileDoesNotRecordItAsWrittenByThisRun()
        {
            using (RunLog log = Start())
            {
                string stale = LastWeeksFile("1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd");

                long size = log.CheckOnDisk("NWD", stale);

                Assert.That(size, Is.EqualTo(500), "the size should still be read and reported");
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(0),
                    "a checked file was recorded as one this run wrote");

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("checked"));
                Assert.That(text, Does.Contain("not written by this run"));
                Assert.That(text, Does.Not.Contain("NWD      written"));
            }
        }

        [Test]
        public void CheckingAFileThatIsNotThereSaysSoAndRecordsNothing()
        {
            using (RunLog log = Start())
            {
                Assert.That(log.CheckOnDisk("NWF", Path.Combine(folder, "never-made.nwf")), Is.EqualTo(-1));
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(0));
                Assert.That(ReadWhileOpen(log), Does.Contain("NOT ON DISK"));
            }
        }

        // The exact case. A group throws before writing anything, and last week's outputs
        // are still at the paths.
        [Test]
        public void AGroupThatThrewDoesNotListLastWeeksOutputsAsItsOwn()
        {
            using (RunLog log = Start())
            {
                string nwf = LastWeeksFile("1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf");
                string nwd = LastWeeksFile("1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd");

                // What the engine's catch block does now.
                log.Failure("federating 1C07BC", new InvalidOperationException("would not open"), "carried on");
                log.CheckOnDisk("NWF", nwf);
                log.CheckOnDisk("NWD", nwd);
                log.GroupFinished("1C07BC", GroupOutcome.Failed, 1.0, "would not open", null);

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups failed  : 1"));
                Assert.That(text, Does.Contain("files written  : none"),
                    "a group that wrote nothing listed files as written");
                Assert.That(text, Does.Not.Contain("Nothing failed."));
            }
        }

        [Test]
        public void AFileReallyWrittenIsStillRecorded()
        {
            using (RunLog log = Start())
            {
                string real = Path.Combine(folder, "written.nwf");
                File.WriteAllText(real, new string('a', 120));

                Assert.That(log.WriteFinished("NWF", real), Is.EqualTo(120));
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(1));
                Assert.That(log.WrittenFiles[0].Path, Is.EqualTo(real));
            }
        }

        // A throw after a successful write reaches the catch, which checks the same file
        // again. Even if that ever went through WriteFinished, it must count once.
        [Test]
        public void TheSameFileWrittenTwiceIsCountedOnce()
        {
            using (RunLog log = Start())
            {
                string real = Path.Combine(folder, "written.nwf");
                File.WriteAllText(real, new string('a', 120));

                log.WriteFinished("NWF", real);
                log.WriteFinished("NWF", real);

                Assert.That(log.WrittenFiles.Count, Is.EqualTo(1),
                    "files written became a count of checks rather than of files");
            }
        }

        [Test]
        public void TheSamePathUnderTwoKindsIsCountedTwice()
        {
            using (RunLog log = Start())
            {
                string real = Path.Combine(folder, "odd.dat");
                File.WriteAllText(real, "x");

                log.WriteFinished("NWF", real);
                log.WriteFinished("NWD", real);

                Assert.That(log.WrittenFiles.Count, Is.EqualTo(2),
                    "two different outputs at one path are two records");
            }
        }

        [Test]
        public void ThePathIsComparedWithoutCaseLikeWindowsDoes()
        {
            using (RunLog log = Start())
            {
                string real = Path.Combine(folder, "written.nwf");
                File.WriteAllText(real, "x");

                log.WriteFinished("NWF", real);
                log.WriteFinished("NWF", real.ToUpperInvariant());

                Assert.That(log.WrittenFiles.Count, Is.EqualTo(1));
            }
        }
    }
}
