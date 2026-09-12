using System;
using System.Collections.Generic;
using System.IO;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The NWF is saved twice, once after the models are appended and again after the
    /// clash step, and the NWD is published after both. The RESULT block has to say what
    /// the file is at the END of that, because the clash tests and every clash result live
    /// inside the NWF and it is the only record of what has been fixed.
    ///
    /// A real run on 2026-09-01 reported it at 4,141 bytes, which is an empty federation.
    /// Nothing had shrunk. The second save had read 165,844 bytes off the disk and printed
    /// it four minutes earlier, and the recorded entry was simply never updated. That
    /// reads exactly like an NWF that lost a week of review, which is why these exist.
    /// </summary>
    [TestFixture]
    public class WrittenSizeTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorWritten");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private RunLog Log()
        {
            return RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 9, 1, 9, 37, 8));
        }

        private string Write(string name, int bytes)
        {
            string path = Path.Combine(folder, name);
            File.WriteAllBytes(path, new byte[bytes]);
            return path;
        }

        private static WrittenFile Only(RunLog log, string kind)
        {
            foreach (WrittenFile file in log.WrittenFiles)
            {
                if (file.Kind == kind)
                {
                    return file;
                }
            }

            Assert.Fail("nothing was recorded as written for " + kind);
            return null;
        }

        // ---------- the one the whole job is about ----------

        // The one the brief asks for by name.
        [Test]
        public void AnNwfSavedTwiceReportsTheSizeOfTheSecondSave()
        {
            using (RunLog log = Log())
            {
                string nwf = Write("federation.nwf", 4141);
                Assert.That(log.WriteFinished("NWF", nwf), Is.EqualTo(4141));

                // The clash step ran, so the NWF is saved again and is much bigger.
                File.WriteAllBytes(nwf, new byte[165844]);
                Assert.That(log.WriteFinished("NWF", nwf), Is.EqualTo(165844));

                Assert.That(Only(log, "NWF").SizeInBytes, Is.EqualTo(165844),
                    "the RESULT block would report the empty federation");
            }
        }

        [Test]
        public void WritingTheSameFileTwiceStillCountsAsOneFile()
        {
            using (RunLog log = Log())
            {
                string nwf = Write("federation.nwf", 4141);
                log.WriteFinished("NWF", nwf);
                File.WriteAllBytes(nwf, new byte[165844]);
                log.WriteFinished("NWF", nwf);

                Assert.That(log.WrittenFiles.Count, Is.EqualTo(1),
                    "files written should count files, not writes");
            }
        }

        [Test]
        public void TwoDifferentFilesAreTwoEntries()
        {
            using (RunLog log = Log())
            {
                log.WriteFinished("NWF", Write("a.nwf", 10));
                log.WriteFinished("NWD", Write("a.nwd", 20));

                Assert.That(log.WrittenFiles.Count, Is.EqualTo(2));
            }
        }

        // ---------- the NWF is looked at again after the NWD ----------

        // The one the brief asks for by name.
        [Test]
        public void TheNwfIsConfirmedWholeAfterTheNwdIsPublished()
        {
            using (RunLog log = Log())
            {
                string nwf = Write("federation.nwf", 165844);
                log.WriteFinished("NWF", nwf);

                // The NWD is published, and then the NWF is looked at once more.
                long after = log.ConfirmStillWhole("NWF", nwf, "publishing the NWD");

                Assert.That(after, Is.EqualTo(165844));
                Assert.That(Only(log, "NWF").SizeInBytes, Is.EqualTo(165844));
                Assert.That(Read(log), Does.Contain("intact"));
                Assert.That(Read(log), Does.Contain("unchanged by publishing the NWD"));
            }
        }

        // The case this whole check exists to catch. If the NWF ever really does shrink
        // while the NWD is published, the run has destroyed the clash history and must
        // say so in the loudest words the log has.
        [Test]
        public void AnNwfThatReallyShrankIsReportedAsShrinking()
        {
            using (RunLog log = Log())
            {
                string nwf = Write("federation.nwf", 165844);
                log.WriteFinished("NWF", nwf);

                File.WriteAllBytes(nwf, new byte[4141]);
                long after = log.ConfirmStillWhole("NWF", nwf, "publishing the NWD");

                Assert.That(after, Is.EqualTo(4141));
                Assert.That(Only(log, "NWF").SizeInBytes, Is.EqualTo(4141),
                    "the result block must carry the truth, whichever way it goes");

                string text = Read(log);

                Assert.That(text, Does.Contain("CHANGED"));
                Assert.That(text, Does.Contain("It got SMALLER"));
                Assert.That(text, Does.Contain("clash results live in this file"));
            }
        }

        [Test]
        public void AnNwfThatHasGoneIsReportedAsGone()
        {
            using (RunLog log = Log())
            {
                string nwf = Write("federation.nwf", 165844);
                log.WriteFinished("NWF", nwf);
                File.Delete(nwf);

                Assert.That(log.ConfirmStillWhole("NWF", nwf, "publishing the NWD"),
                    Is.EqualTo(-1));
                Assert.That(Read(log), Does.Contain("GONE"));
            }
        }

        [Test]
        public void ConfirmingAFileNobodyRecordedSaysSoWithoutInventingAnEntry()
        {
            using (RunLog log = Log())
            {
                string other = Write("something.nwf", 99);

                Assert.That(log.ConfirmStillWhole("NWF", other, "publishing the NWD"),
                    Is.EqualTo(99));
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(0),
                    "confirming must not turn a check into a claim that this run wrote it");
            }
        }

        /// <summary>Reads the live log the way the Copy log button does, share aware.</summary>
        private static string Read(RunLog log)
        {
            using (FileStream stream = new FileStream(
                log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
