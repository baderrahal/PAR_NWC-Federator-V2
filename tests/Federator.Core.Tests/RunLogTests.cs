using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The log has to survive a hard crash inside a Navisworks call, so every one of
    /// these reads the file back off the disk while the log is still open rather than
    /// after it is closed.
    /// </summary>
    [TestFixture]
    public class RunLogTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorRunLogTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void RemoveFolder()
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp folder is not worth failing a test over.
            }
        }

        /// <summary>
        /// Reads the file while the log still holds it open. If the writer did not share
        /// the file this throws, which is itself the thing being checked.
        /// </summary>
        private static string ReadWhileOpen(RunLog log)
        {
            using (FileStream stream = new FileStream(
                       log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5));
        }

        [Test]
        public void TheFileExistsBeforeAnyWorkIsDone()
        {
            using (RunLog log = Start())
            {
                Assert.That(File.Exists(log.Path), Is.True, "The log file was not created by Start.");
                Assert.That(Path.GetDirectoryName(log.Path), Is.EqualTo(folder));
            }
        }

        [Test]
        public void TheFileIsNamedForTheMomentTheRunStarted()
        {
            using (RunLog log = Start())
            {
                Assert.That(Path.GetFileName(log.Path), Is.EqualTo("run-20260830-142305.log"));
            }
        }

        [Test]
        public void TwoRunsInTheSameSecondDoNotOverwriteEachOther()
        {
            using (RunLog first = Start())
            using (RunLog second = Start())
            {
                Assert.That(second.Path, Is.Not.EqualTo(first.Path));
                Assert.That(File.Exists(first.Path), Is.True);
                Assert.That(File.Exists(second.Path), Is.True);
            }
        }

        // This is the whole point of the log. If a line is only on disk once the log is
        // closed, a crash inside a Navisworks call loses it.
        [Test]
        public void EveryLineIsOnDiskImmediatelyAndNotHeldToTheEnd()
        {
            using (RunLog log = Start())
            {
                log.Line("first line");
                Assert.That(ReadWhileOpen(log), Does.Contain("first line"));

                log.Line("second line");
                string afterSecond = ReadWhileOpen(log);
                Assert.That(afterSecond, Does.Contain("first line"));
                Assert.That(afterSecond, Does.Contain("second line"));

                log.Line("third line");
                Assert.That(ReadWhileOpen(log), Does.Contain("third line"));
            }
        }

        [Test]
        public void TheOpeningLineIsOnDiskBeforeAnythingElseIsWritten()
        {
            using (RunLog log = Start())
            {
                Assert.That(ReadWhileOpen(log), Does.Contain("Log opened at"));
            }
        }

        [Test]
        public void EveryLineCarriesAWallClockTimeAndSecondsElapsed()
        {
            using (RunLog log = Start())
            {
                log.Line("a line");

                string[] lines = ReadWhileOpen(log).Split('\n');
                string found = null;

                foreach (string line in lines)
                {
                    if (line.Contains("a line"))
                    {
                        found = line;
                    }
                }

                Assert.That(found, Is.Not.Null);

                // 14:23:05.117  +0000.012s  a line
                Assert.That(found, Does.Match(@"^\d{2}:\d{2}:\d{2}\.\d{3}\s+\+\d+\.\d{3}s\s+a line"));
            }
        }

        [Test]
        public void TheHeaderCarriesTheVersionsAndTheOpenDocument()
        {
            using (RunLog log = Start())
            {
                log.Session("1.0.0.0", "Navisworks Manage 2025 22.5", @"C:\models\open.nwf");

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("SESSION"));
                Assert.That(text, Does.Contain("1.0.0.0"));
                Assert.That(text, Does.Contain("Navisworks Manage 2025 22.5"));
                Assert.That(text, Does.Contain(@"C:\models\open.nwf"));
                Assert.That(text, Does.Contain(log.Path));
            }
        }

        [Test]
        public void AnEmptyOpenDocumentReadsAsNoneRatherThanBlank()
        {
            using (RunLog log = Start())
            {
                log.Session("1.0.0.0", "some version", null);
                Assert.That(ReadWhileOpen(log), Does.Contain("open document  : none"));
            }
        }

        [Test]
        public void TheRunSettingsBlockCarriesTheFoldersAndTheCounts()
        {
            using (RunLog log = Start())
            {
                log.RunSettings(@"C:\in", true, @"C:\out\nwf", @"C:\out\nwd", 12, 11, 3);

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain(@"C:\in"));
                Assert.That(text, Does.Contain("include subfolders: yes"));
                Assert.That(text, Does.Contain(@"C:\out\nwf"));
                Assert.That(text, Does.Contain(@"C:\out\nwd"));
                Assert.That(text, Does.Contain("files found       : 12"));
                Assert.That(text, Does.Contain("files ticked      : 11"));
                Assert.That(text, Does.Contain("groups built      : 3"));
            }
        }

        [Test]
        public void AnExceptionIsWrittenWithItsTypeMessageInnerAndStack()
        {
            using (RunLog log = Start())
            {
                Exception caught;

                try
                {
                    try
                    {
                        throw new InvalidOperationException("the inner reason");
                    }
                    catch (Exception inner)
                    {
                        throw new IOException("the outer reason", inner);
                    }
                }
                catch (Exception error)
                {
                    caught = error;
                }

                log.Failure("while appending a file", caught, "kept going with the rest of the group");

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("FAILURE"));
                Assert.That(text, Does.Contain("while appending a file"));
                Assert.That(text, Does.Contain("System.IO.IOException"), "the full type name is missing");
                Assert.That(text, Does.Contain("the outer reason"));
                Assert.That(text, Does.Contain("System.InvalidOperationException"), "the inner type is missing");
                Assert.That(text, Does.Contain("the inner reason"));
                Assert.That(text, Does.Contain("stack    :"));
                Assert.That(text, Does.Contain("AnExceptionIsWrittenWithItsTypeMessageInnerAndStack"),
                    "the stack trace is missing");
                Assert.That(text, Does.Contain("kept going with the rest of the group"),
                    "what the tool did next is missing");
            }
        }

        [Test]
        public void AFailureIsOnDiskImmediatelyToo()
        {
            using (RunLog log = Start())
            {
                log.Failure("something", new Exception("boom"), "stopped");
                Assert.That(ReadWhileOpen(log), Does.Contain("boom"));
            }
        }

        [Test]
        public void AFileIsOnlyRecordedAsWrittenAfterItsSizeIsReadBackOffTheDisk()
        {
            using (RunLog log = Start())
            {
                string real = Path.Combine(folder, "written.nwf");
                File.WriteAllText(real, new string('x', 1234));

                long size = log.WriteFinished("NWF", real);

                Assert.That(size, Is.EqualTo(1234));
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(1));
                Assert.That(log.WrittenFiles[0].SizeInBytes, Is.EqualTo(1234));
                Assert.That(log.WrittenFiles[0].Kind, Is.EqualTo("NWF"));
                Assert.That(ReadWhileOpen(log), Does.Contain("1,234 bytes"));
            }
        }

        [Test]
        public void AFileThatIsNotOnDiskIsNeverRecordedAsWritten()
        {
            using (RunLog log = Start())
            {
                long size = log.WriteFinished("NWD", Path.Combine(folder, "never-made.nwd"));

                Assert.That(size, Is.EqualTo(-1));
                Assert.That(log.WrittenFiles.Count, Is.EqualTo(0), "a missing file was recorded as written");
                Assert.That(ReadWhileOpen(log), Does.Contain("MISSING"));
            }
        }

        [Test]
        public void TheResultBlockCountsMatchWhatWasLogged()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07AA", GroupOutcome.Done, 12.5);
                log.GroupFinished("1C07BC", GroupOutcome.Done, 30.25);
                log.GroupFinished("1C07K1", GroupOutcome.Partial, 8.0);
                log.GroupFinished("1C07ZZ", GroupOutcome.Failed, 1.5);

                string one = Path.Combine(folder, "one.nwf");
                string two = Path.Combine(folder, "two.nwd");
                File.WriteAllText(one, new string('a', 10));
                File.WriteAllText(two, new string('b', 20));
                log.WriteFinished("NWF", one);
                log.WriteFinished("NWD", two);

                log.Failure("first problem", new Exception("one broke"), "kept going");
                log.Failure("second problem", new Exception("two broke"), "stopped");

                Assert.That(log.CountOf(GroupOutcome.Done), Is.EqualTo(2));
                Assert.That(log.CountOf(GroupOutcome.Partial), Is.EqualTo(1));
                Assert.That(log.CountOf(GroupOutcome.Failed), Is.EqualTo(1));

                log.WriteResultBlock();

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("groups done    : 2"));
                Assert.That(text, Does.Contain("groups partial : 1"));
                Assert.That(text, Does.Contain("groups failed  : 1"));
                Assert.That(text, Does.Contain("files written  : 2"));
                Assert.That(text, Does.Contain(one));
                Assert.That(text, Does.Contain(two));
                Assert.That(text, Does.Contain("errors         : 2"));

                // Every error is repeated in full in the result block, so the tail of the
                // file is enough on its own.
                int firstMention = text.IndexOf("one broke", StringComparison.Ordinal);
                int lastMention = text.LastIndexOf("one broke", StringComparison.Ordinal);
                Assert.That(lastMention, Is.GreaterThan(firstMention),
                    "the error was not repeated in the result block");
                Assert.That(text, Does.Contain("total elapsed  :"));
            }
        }

        [Test]
        public void WhenNothingFailedTheResultBlockSaysSoInOneLine()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07AA", GroupOutcome.Done, 1.0);
                log.WriteResultBlock();

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("Nothing failed."));
                Assert.That(text, Does.Not.Contain("errors         :"));
            }
        }

        // A bad output folder must not cost the log. The fixed path copy is the one that
        // matters and it has to survive, carrying the reason the second copy failed.
        [Test]
        public void ABadOutputFolderStillLeavesTheLogInTheFixedPath()
        {
            using (RunLog log = Start())
            {
                string copied;
                bool ok = log.TryCopyTo("Z:\\no-such-drive\\nowhere", out copied);

                Assert.That(ok, Is.False);
                Assert.That(copied, Is.Null);

                Assert.That(File.Exists(log.Path), Is.True, "the fixed path log was lost");

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("FAILURE"));
                Assert.That(text, Does.Contain("Z:\\no-such-drive\\nowhere"));
                Assert.That(text, Does.Contain("kept going"));
            }
        }

        [Test]
        public void AGoodOutputFolderGetsASecondCopyOfTheWholeLog()
        {
            using (RunLog log = Start())
            {
                log.Line("a line that has to reach the copy");

                string beside = Path.Combine(folder, "beside-the-nwf");
                string copied;
                bool ok = log.TryCopyTo(beside, out copied);

                Assert.That(ok, Is.True);
                Assert.That(copied, Is.Not.Null);
                Assert.That(File.Exists(copied), Is.True);
                Assert.That(Path.GetFileName(copied), Is.EqualTo(Path.GetFileName(log.Path)));
                Assert.That(File.ReadAllText(copied), Does.Contain("a line that has to reach the copy"));
            }
        }

        [Test]
        public void TheFixedFolderNeverDependsOnAFolderTheUserPicked()
        {
            string fixedFolder = RunLog.DefaultLogFolder();
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            Assert.That(fixedFolder, Does.StartWith(local));
            Assert.That(fixedFolder, Does.Contain("ParsonsNwcFederator"));
            Assert.That(fixedFolder, Does.EndWith("logs"));
        }

        [Test]
        public void EveryLineIsHandedToTheWindowAsItIsWritten()
        {
            using (RunLog log = Start())
            {
                List<string> seen = new List<string>();
                log.LineWritten += seen.Add;

                log.Line("live one");
                log.Line("live two");

                Assert.That(seen.Count, Is.EqualTo(2));
                Assert.That(seen[0], Does.Contain("live one"));
                Assert.That(seen[1], Does.Contain("live two"));
            }
        }

        [Test]
        public void ReadAllGivesBackTheWholeFileForTheClipboard()
        {
            using (RunLog log = Start())
            {
                log.Line("copy me");
                string all = log.ReadAll();

                Assert.That(all, Does.Contain("Log opened at"));
                Assert.That(all, Does.Contain("copy me"));
                Assert.That(all, Is.EqualTo(ReadWhileOpen(log)));
            }
        }

        [Test]
        public void AnAppendLogsTheSizeItReadRatherThanOneItWasGiven()
        {
            using (RunLog log = Start())
            {
                string nwc = Path.Combine(folder, "model.nwc");
                File.WriteAllText(nwc, new string('n', 4096));

                log.AppendAttempted(nwc);
                log.AppendFinished(nwc, true);

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("APPEND   attempt"));
                Assert.That(text, Does.Contain("APPEND   ok"));
                Assert.That(text, Does.Contain("4,096 bytes"));
            }
        }

        [Test]
        public void AnAppendOfAFileThatIsNotThereSaysSoRatherThanGuessingASize()
        {
            using (RunLog log = Start())
            {
                log.AppendFinished(Path.Combine(folder, "gone.nwc"), false);

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("APPEND   FAILED"));
                Assert.That(text, Does.Contain("NOT ON DISK"));
            }
        }

        [Test]
        public void ScanAndGroupLinesAreEachLoggedSeparately()
        {
            using (RunLog log = Start())
            {
                log.ScanStarted(@"C:\in", true);
                log.UnreadableFile("badname.nwc", "Split on \"-\" gave 1 parts");
                log.ScanFinished(7, 6, 1);
                log.GroupStarted("1C07BC", new List<string> { @"C:\in\a.nwc", @"C:\in\b.nwc" });
                log.GroupFinished("1C07BC", GroupOutcome.Done, 42.125);

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("SCAN     started"));
                Assert.That(text, Does.Contain("including subfolders"));
                Assert.That(text, Does.Contain("UNREAD   badname.nwc"));
                Assert.That(text, Does.Contain("Split on \"-\" gave 1 parts"));
                Assert.That(text, Does.Contain("SCAN     finished 7 found, 6 readable, 1 that cannot be read"));
                Assert.That(text, Does.Contain("GROUP    started  1C07BC  2 files"));
                Assert.That(text, Does.Contain(@"C:\in\a.nwc"));
                Assert.That(text, Does.Contain(@"C:\in\b.nwc"));
                Assert.That(text, Does.Contain("GROUP    finished 1C07BC  DONE  42.125s"));
            }
        }

        [Test]
        public void TheLogSurvivesBeingAbandonedWithoutBeingClosed()
        {
            // Standing in for the process dying part way through. Nothing is disposed and
            // no result block is written, and the lines are still on disk.
            RunLog log = Start();
            log.Line("written just before the crash");

            Assert.That(ReadWhileOpen(log), Does.Contain("written just before the crash"));

            log.Dispose();
        }

        [Test]
        public void OnceTheRunIsOverThePlainFileReadWorks()
        {
            string path;

            using (RunLog log = Start())
            {
                log.Line("after the run");
                path = log.Path;
            }

            Assert.That(File.ReadAllText(path), Does.Contain("after the run"));
        }

        // While the run holds the file, a reader has to allow write sharing. File.ReadAllText
        // does not, so it throws. Notepad and the Copy log button both share properly. This
        // is pinned here because it decides what Bader can be told to do mid run.
        [Test]
        public void APlainFileReadCannotOpenTheLogWhileTheRunIsStillWriting()
        {
            using (RunLog log = Start())
            {
                log.Line("mid run");

                Assert.Throws<IOException>(delegate { File.ReadAllText(log.Path); });

                // The two ways that do work while the run is going.
                Assert.That(log.ReadAll(), Does.Contain("mid run"));
                Assert.That(ReadWhileOpen(log), Does.Contain("mid run"));
            }
        }
    }
}
