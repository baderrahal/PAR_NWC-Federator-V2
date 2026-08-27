using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Retention runs on start, once the new file is open. It is housekeeping, so a
    /// failure inside it writes a line and the run carries on.
    /// </summary>
    [TestFixture]
    public class RunLogRetentionTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(
                Path.GetTempPath(), "FederatorRetentionTests", Guid.NewGuid().ToString("N"));
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
        /// Writes count log files with names and timestamps that run oldest to newest,
        /// so the order retention should use is unambiguous. Returns them oldest first.
        /// </summary>
        private List<string> MakeOldLogs(int count)
        {
            List<string> made = new List<string>();
            DateTime stamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            for (int i = 0; i < count; i++)
            {
                DateTime when = stamp.AddMinutes(i);
                string path = Path.Combine(
                    folder, "run-" + when.ToString("yyyyMMdd-HHmmss") + ".log");

                File.WriteAllText(path, "old log " + i);
                File.SetLastWriteTimeUtc(path, when);
                made.Add(path);
            }

            return made;
        }

        private static string[] LogsIn(string folder)
        {
            string[] found = Directory.GetFiles(folder, RunLog.LogFileSearchPattern);
            Array.Sort(found, StringComparer.OrdinalIgnoreCase);
            return found;
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

        [Test]
        public void TheDefaultIsThirty()
        {
            Assert.That(RunLog.DefaultKeepLogs, Is.EqualTo(30));
        }

        // Thirty five already there, plus the one this run opens, pruned back to thirty.
        [Test]
        public void ThirtyFiveFilesLeavesThirty()
        {
            MakeOldLogs(35);

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                Assert.That(LogsIn(folder).Length, Is.EqualTo(30));
                Assert.That(File.Exists(log.Path), Is.True);
            }
        }

        [Test]
        public void TheNewestSurviveAndTheOldestGo()
        {
            List<string> oldest_first = MakeOldLogs(35);

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                // 29 of the old ones are kept alongside the live file, so the 6 oldest go.
                for (int i = 0; i < 6; i++)
                {
                    Assert.That(File.Exists(oldest_first[i]), Is.False,
                        "an old log survived that should have gone: " + Path.GetFileName(oldest_first[i]));
                }

                for (int i = 6; i < oldest_first.Count; i++)
                {
                    Assert.That(File.Exists(oldest_first[i]), Is.True,
                        "a newer log was deleted: " + Path.GetFileName(oldest_first[i]));
                }
            }
        }

        [Test]
        public void TheLiveFileIsNeverDeleted()
        {
            MakeOldLogs(35);

            // Keep one, which would delete every other file in the folder. The live file
            // still has to survive, because it is the one being written.
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5), 1))
            {
                Assert.That(File.Exists(log.Path), Is.True, "the live file was deleted");
                Assert.That(LogsIn(folder).Length, Is.EqualTo(1));

                // Still writable afterwards, so the handle was not pulled from under it.
                log.Line("still writing after the prune");
                Assert.That(ReadWhileOpen(log), Does.Contain("still writing after the prune"));
            }
        }

        [Test]
        public void KeepingZeroStillLeavesTheLiveFile()
        {
            MakeOldLogs(5);

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5), 0))
            {
                Assert.That(File.Exists(log.Path), Is.True);
                Assert.That(LogsIn(folder).Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void AFolderUnderThirtyIsUntouched()
        {
            List<string> made = MakeOldLogs(10);

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                foreach (string path in made)
                {
                    Assert.That(File.Exists(path), Is.True,
                        "a log was deleted from a folder that was under the limit: " + Path.GetFileName(path));
                }

                Assert.That(LogsIn(folder).Length, Is.EqualTo(11));
                Assert.That(ReadWhileOpen(log), Does.Not.Contain("RETAIN"),
                    "retention wrote a line when it had nothing to do");
            }
        }

        [Test]
        public void AnEmptyFolderIsFine()
        {
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                Assert.That(LogsIn(folder).Length, Is.EqualTo(1));
                Assert.That(File.Exists(log.Path), Is.True);
            }
        }

        // A log someone still has open cannot be deleted. That must produce one line
        // naming the file and the reason, and must not throw.
        [Test]
        public void ALockedFileLogsTheReasonAndDoesNotThrow()
        {
            List<string> made = MakeOldLogs(35);
            string locked = made[0];

            using (FileStream hold = new FileStream(
                       locked, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                RunLog log = null;

                Assert.DoesNotThrow(delegate
                {
                    log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5));
                });

                using (log)
                {
                    Assert.That(File.Exists(locked), Is.True, "a locked file was somehow deleted");

                    string text = ReadWhileOpen(log);
                    Assert.That(text, Does.Contain("RETAIN   could not delete"));
                    Assert.That(text, Does.Contain(Path.GetFileName(locked)),
                        "the line does not name the file it could not delete");
                    Assert.That(text, Does.Contain("could not delete 1"),
                        "the summary line does not count the refusal");

                    // The five that were not locked still went.
                    for (int i = 1; i < 6; i++)
                    {
                        Assert.That(File.Exists(made[i]), Is.False,
                            "one bad delete stopped the rest: " + Path.GetFileName(made[i]));
                    }

                    // And the run carries on writing.
                    log.Line("carried on after the refused delete");
                    Assert.That(ReadWhileOpen(log), Does.Contain("carried on after the refused delete"));
                }
            }
        }

        [Test]
        public void RetentionWritesOneSummaryLineWhenItDeletesSomething()
        {
            MakeOldLogs(35);

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("RETAIN   keeping 30 logs, deleted 6, could not delete 0"));
            }
        }

        [Test]
        public void FilesThatAreNotLogsAreLeftAlone()
        {
            MakeOldLogs(35);

            string stranger = Path.Combine(folder, "notes.txt");
            string alsoStranger = Path.Combine(folder, "run-something.txt");
            File.WriteAllText(stranger, "keep me");
            File.WriteAllText(alsoStranger, "keep me too");
            File.SetLastWriteTimeUtc(stranger, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            File.SetLastWriteTimeUtc(alsoStranger, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                Assert.That(File.Exists(stranger), Is.True, "a file that is not a log was deleted");
                Assert.That(File.Exists(alsoStranger), Is.True, "a file that is not a log was deleted");
            }
        }

        [Test]
        public void TheNumberKeptIsASetting()
        {
            MakeOldLogs(20);

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 8, 30, 14, 23, 5), 5))
            {
                Assert.That(LogsIn(folder).Length, Is.EqualTo(5));
                Assert.That(File.Exists(log.Path), Is.True);
            }
        }

        [Test]
        public void RetentionRunsOnTheDisabledFallbackPathToo()
        {
            MakeOldLogs(35);

            using (RunLog log = RunLog.StartOrDisabled(folder, new DateTime(2026, 8, 30, 14, 23, 5)))
            {
                Assert.That(log.IsWritingToDisk, Is.True);
                Assert.That(LogsIn(folder).Length, Is.EqualTo(30));
            }
        }
    }
}
