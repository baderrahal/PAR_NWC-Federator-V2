using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F119, the faults of the run log and RESULT that a Core test shows: a listener that throws,
    /// a size that was never read, a run that started and never finished, a waiting time nobody
    /// measured, and the retention that deleted what was not this tool's.
    /// </summary>
    [TestFixture]
    public class RunLogHardeningTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorRunLogHardening");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static string ReadWhileOpen(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 10, 7, 9, 0, 0));
        }

        // ---------- FR-057, a log line never stops the run ----------

        /// <summary>
        /// The window's listener threw and the exception came out of Line, up through the run. The line is
        /// on the disk first, the listener is named and taken off so it is not called again, and the run
        /// goes on.
        /// </summary>
        [Test]
        public void AListenerThatThrowsNeitherStopsTheLineNorStaysSubscribed()
        {
            using (RunLog log = Start())
            {
                int calls = 0;
                log.LineWritten += line =>
                {
                    calls++;
                    throw new InvalidOperationException("the window is gone");
                };

                log.Line("first line");
                log.Line("second line");

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("first line"));
                Assert.That(text, Does.Contain("second line"));
                Assert.That(text, Does.Contain(
                    "a listener of this log threw InvalidOperationException: the window is gone,"
                    + " so it was removed and the run goes on"));
                Assert.That(calls, Is.EqualTo(1), "a listener that threw is not called again");
            }
        }

        /// <summary>A second listener that does not throw keeps hearing every line.</summary>
        [Test]
        public void AListenerThatThrowsDoesNotSilenceTheOtherOne()
        {
            using (RunLog log = Start())
            {
                List<string> heard = new List<string>();
                log.LineWritten += line => { throw new InvalidOperationException("one"); };
                log.LineWritten += line => heard.Add(line);

                log.Line("a");
                log.Line("b");

                Assert.That(heard.FindAll(line => line.EndsWith("a") || line.EndsWith("b")).Count, Is.EqualTo(2));
            }
        }

        /// <summary>
        /// Two listeners that throw: each is named once and neither is called again, and the log line
        /// they were handed is not heard twice by the one that follows.
        /// </summary>
        [Test]
        public void TwoListenersThatThrowAreEachNamedOnceAndNeverCalledAgain()
        {
            using (RunLog log = Start())
            {
                int first = 0;
                int second = 0;
                List<string> heard = new List<string>();
                log.LineWritten += line => { first++; throw new InvalidOperationException("one"); };
                log.LineWritten += line => { second++; throw new InvalidOperationException("two"); };
                log.LineWritten += line => heard.Add(line);

                log.Line("a line");
                log.Line("another line");

                string text = ReadWhileOpen(log.Path);

                Assert.That(first, Is.EqualTo(1));
                Assert.That(second, Is.EqualTo(1));
                Assert.That(CountOf(text, "a listener of this log threw InvalidOperationException: one"), Is.EqualTo(1));
                Assert.That(CountOf(text, "a listener of this log threw InvalidOperationException: two"), Is.EqualTo(1));
                Assert.That(heard.FindAll(line => line.EndsWith("a line")).Count, Is.EqualTo(1));
            }
        }

        private static int CountOf(string text, string part)
        {
            int count = 0;
            int at = text.IndexOf(part, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(part, at + part.Length, StringComparison.Ordinal);
            }

            return count;
        }

        /// <summary>
        /// A file that cannot be written, a full disk or a handle gone, stopped the run from a log line.
        /// The first fault is said once to the window and the run goes on with the lines in memory. The
        /// stream is closed from outside here, which is the one way a test can make the write throw.
        /// </summary>
        [Test]
        public void ALogFileThatCannotBeWrittenNeverStopsTheRunAndSaysSoOnce()
        {
            using (RunLog log = Start())
            {
                List<string> heard = new List<string>();
                log.LineWritten += line => heard.Add(line);

                System.Reflection.FieldInfo field = typeof(RunLog).GetField(
                    "stream", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                ((IDisposable)field.GetValue(log)).Dispose();

                Assert.DoesNotThrow(() => log.Line("after the file went"));
                Assert.DoesNotThrow(() => log.Line("and again"));

                Assert.That(heard.FindAll(line => line.Contains("after the file went")).Count, Is.EqualTo(1));
                Assert.That(heard.FindAll(line => line.Contains("and again")).Count, Is.EqualTo(1));
                Assert.That(heard.FindAll(line => line.Contains("the log file could not be written")).Count, Is.EqualTo(1));
                Assert.That(log.ReadAll(), Does.Contain("after the file went"));
            }
        }

        // ---------- FR-046, a file that exists is never said to be missing ----------

        /// <summary>
        /// A file held open with no sharing cannot be opened for a read, and it is still on the disk. The
        /// size falls back to the directory's own figure and never reads as not on disk.
        /// </summary>
        [Test]
        public void AFileHeldWithNoSharingIsStillOnDiskAndNeverMissing()
        {
            string held = Path.Combine(folder, "held.nwc");
            File.WriteAllText(held, new string('n', 2048));

            using (RunLog log = Start())
            using (new FileStream(held, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                log.AppendFinished(held, true);

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Not.Contain("NOT ON DISK"));
                Assert.That(text, Does.Contain("APPEND   ok"));
            }
        }

        // ---------- FR-048, a size that was never read is not a size of zero ----------

        [Test]
        public void ARowForAFileThatIsNotOnDiskCarriesNoNumber()
        {
            using (RunLog log = Start())
            {
                log.AppendFinished(Path.Combine(folder, "gone.nwc"), false);

                string[] rows = ReadWhileOpen(log.RowLogPath).Split('\n');
                string row = null;

                foreach (string candidate in rows)
                {
                    if (candidate.Contains("append failed"))
                    {
                        row = candidate;
                    }
                }

                Assert.That(row, Is.Not.Null);
                string[] fields = row.TrimEnd('\r').Split('\t');
                Assert.That(fields.Length, Is.EqualTo(EventRow.FieldCount));
                Assert.That(fields[6], Is.Empty, "the number column of a file that was not there");
            }
        }

        // ---------- FR-050 and FR-051, the clock of a run that did not finish ----------

        /// <summary>
        /// A run that started and threw before RUN finished said no run was marked, so every share was
        /// worked off the session. It is counted to the moment the block is written and says so.
        /// </summary>
        [Test]
        public void ARunThatStartedAndNeverFinishedIsCountedAndSaysSo()
        {
            using (RunLog log = Start())
            {
                log.RunStarted(3);
                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Not.Contain("no run was marked"));
                Assert.That(text, Does.Contain("the run started and RUN finished was never marked"));
                Assert.That(text, Does.Contain("waiting for the person : "));
                Assert.That(log.WhereTheTimeWent.Marked, Is.True);
            }
        }

        /// <summary>A second run in the window that did not finish is not given the first run's finish.</summary>
        [Test]
        public void ASecondRunThatDidNotFinishIsNotGivenTheFirstRunsFinish()
        {
            using (RunLog log = Start())
            {
                log.RunStarted(1);
                log.RunFinished();
                log.RunStarted(2);
                log.WriteResultBlock();

                Assert.That(ReadWhileOpen(log.Path), Does.Contain("RUN finished was never marked"));
            }
        }

        /// <summary>
        /// No run mark at all means the waiting time was never measured, so RESULT does not state
        /// 0.000 seconds of it beside a run time that already holds the waiting.
        /// </summary>
        [Test]
        public void WithNoRunMarkResultStatesNoWaitingTime()
        {
            using (RunLog log = Start())
            {
                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("no run was marked"));
                Assert.That(text, Does.Not.Contain("waiting for the person : "));
            }
        }

        // ---------- FR-046, the sizes the block prints are the sizes the files hold ----------

        /// <summary>
        /// FileInfo.Length of a file this process holds open for writing lagged on the disk of the run
        /// and the block printed the .tsv as 0 bytes against 1,499,250. It fails before only where that
        /// lag happens, so on the Windows runner.
        /// </summary>
        [Test]
        public void TheTsvSizeTheBlockPrintsIsWhatTheFileHoldsWhileItIsOpen()
        {
            using (RunLog log = Start())
            {
                for (int i = 0; i < 200; i++)
                {
                    log.Row("test", "T" + i, "1", new string('x', 40));
                }

                log.WriteResultBlock();

                long held;

                using (FileStream stream = new FileStream(
                           log.RowLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    held = stream.Length;
                }

                Assert.That(held, Is.GreaterThan(0));

                string printed = null;

                foreach (string line in ReadWhileOpen(log.Path).Split('\n'))
                {
                    if (line.Contains("the .tsv       : "))
                    {
                        printed = line;
                    }
                }

                Assert.That(printed, Is.Not.Null);
                Assert.That(printed, Does.Contain(held.ToString("#,##0", System.Globalization.CultureInfo.InvariantCulture) + " bytes"));
            }
        }

        // ---------- FR-054 and FR-055, retention only deletes this tool's files ----------

        private void MakeOldLogs(string where, int count, bool withTsv)
        {
            DateTime stamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            for (int i = 0; i < count; i++)
            {
                DateTime when = stamp.AddMinutes(i);
                string stem = Path.Combine(where, "run-" + when.ToString("yyyyMMdd-HHmmss"));

                File.WriteAllText(stem + ".log", "old log " + i);
                File.SetLastWriteTimeUtc(stem + ".log", when);

                if (withTsv)
                {
                    File.WriteAllText(stem + ".tsv", "old rows " + i);
                    File.SetLastWriteTimeUtc(stem + ".tsv", when);
                }
            }
        }

        /// <summary>
        /// The .tsv beside each log was never pruned, so a machine kept every one of them for ever. A
        /// .tsv goes with its log, and one with no log of its own is not this tool's to delete.
        /// </summary>
        [Test]
        public void ATsvGoesWithItsLogAndAStrayOneStays()
        {
            MakeOldLogs(folder, 40, true);
            string stray = Path.Combine(folder, "run-stray.tsv");
            File.WriteAllText(stray, "not beside any log");

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 7, 9, 0, 0), 30))
            {
                Assert.That(Directory.GetFiles(folder, "run-*.log").Length, Is.EqualTo(30));
                Assert.That(Directory.GetFiles(folder, "run-*.tsv").Length, Is.EqualTo(31), "30 beside their logs and the stray one");
                Assert.That(File.Exists(stray), Is.True);
                Assert.That(File.Exists(log.RowLogPath), Is.True, "the live .tsv is kept");
            }
        }

        /// <summary>
        /// When the logs folder cannot be opened the log falls back to a folder that is not this
        /// tool's, the system temp folder, where a run-*.log may belong to another program. Nothing in
        /// it is pruned, and the log says why.
        /// </summary>
        [Test]
        public void TheFallbackFolderIsNeverPrunedBecauseItIsNotTheToolsOwn()
        {
            string fallback = Path.Combine(folder, "fallback");
            Directory.CreateDirectory(fallback);
            MakeOldLogs(fallback, 35, false);
            string notAFolder = Path.Combine(folder, "a-file-where-the-logs-folder-should-be");
            File.WriteAllText(notAFolder, "this is a file");

            using (RunLog log = RunLog.StartOrDisabled(notAFolder, fallback, new DateTime(2026, 10, 7, 9, 0, 0), 30))
            {
                Assert.That(log.IsWritingToDisk, Is.True);
                Assert.That(Directory.GetFiles(fallback, "run-*.log").Length, Is.EqualTo(36), "35 foreign logs and this run's");
                Assert.That(ReadWhileOpen(log.Path), Does.Contain(
                    "RETAIN   nothing was deleted, " + fallback + " is not the folder this tool keeps its logs in"));
            }
        }
    }
}
