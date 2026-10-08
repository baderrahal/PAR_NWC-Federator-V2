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
        /// A write that fails is made by closing the stream the log holds. The failure comes out of the same
        /// line, an ObjectDisposedException where a full disk gives an IOException, and the catch takes both.
        /// It reaches a private field because no running code needs a member to do this, and it returns the
        /// message the closed stream throws, so a test can hand in the thing a label must not carry.
        /// </summary>
        private static string TheDiskFails(RunLog log)
        {
            System.Reflection.FieldInfo field = typeof(RunLog).GetField(
                "stream", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, "RunLog no longer holds a field named stream, so this fault cannot be made");

            FileStream stream = (FileStream)field.GetValue(log);
            stream.Dispose();

            try
            {
                stream.WriteByte(0);
            }
            catch (Exception error)
            {
                return error.Message;
            }

            Assert.Fail("a closed stream took a write");
            return null;
        }

        /// <summary>
        /// FR-057's disk half. A write to the file that threw came out of Line, up through the run, and
        /// the failure lines that would have reported it hit the same write. The line is kept in memory
        /// and told to the window, the file is said to have stopped once, the log says it is no longer
        /// writing to disk, and the run goes on. The break: lines before the fault are still in the file
        /// and none after it are, and the memory holds both. The label carries what happened and never
        /// what the stream threw, which is handed in and looked for.
        /// </summary>
        [Test]
        public void AWriteThatFailsStopsTheFileAndNeverTheRun()
        {
            using (RunLog log = Start())
            {
                List<string> heard = new List<string>();
                log.LineWritten += heard.Add;

                log.Line("before the fault");
                string whatTheStreamThrows = TheDiskFails(log);

                Assert.DoesNotThrow(() => log.Line("after the fault"));
                Assert.DoesNotThrow(() => log.Line("and again after it"));

                Assert.That(log.IsWritingToDisk, Is.False);
                Assert.That(log.WhereTheLogIs(), Does.StartWith("WARNING the log is not being written to disk."));
                Assert.That(log.WhereTheLogIs(), Does.Not.Contain(whatTheStreamThrows), "a label never carries a framework message");
                Assert.That(log.WhereTheLogIs(), Does.Not.Contain("ObjectDisposedException"));
                Assert.That(log.DisabledReason, Is.Not.Null.And.Not.Empty);

                string told = string.Join("\n", heard.ToArray());

                Assert.That(told, Does.Contain("after the fault"));
                Assert.That(told, Does.Contain("and again after it"));
                Assert.That(CountOf(told, "the log file stopped taking lines"), Is.EqualTo(1), "said once, not for every line");
                Assert.That(told, Does.Contain("ObjectDisposedException: " + whatTheStreamThrows.TrimEnd('.')),
                    "the log line carries what threw, where a person looks for it");

                string whole = log.ReadAll();

                Assert.That(whole, Does.Contain("before the fault"));
                Assert.That(whole, Does.Contain("after the fault"));
                Assert.That(whole, Does.Contain("and again after it"));

                string onDisk = ReadWhileOpen(log.Path);

                Assert.That(onDisk, Does.Contain("before the fault"));
                Assert.That(onDisk, Does.Not.Contain("and again after it"));
            }
        }

        /// <summary>
        /// A copy of a log whose file stopped is the lines held in memory, which are all of them, and says
        /// so. Reading the short file would hand out a log that ends part way and calls itself the log. The
        /// RESULT size of the file says it is short and does not print it as the size of a whole log.
        /// </summary>
        [Test]
        public void ACopyAndASizeAfterAFailedWriteAreNeverTheShortFileCalledWhole()
        {
            string copies = Path.Combine(folder, "copies");

            using (RunLog log = Start())
            {
                log.Line("before the fault");
                TheDiskFails(log);
                log.Line("after the fault");

                string copied;

                Assert.That(log.TryCopyTo(copies, out copied), Is.True);

                string copy = File.ReadAllText(copied);

                Assert.That(copy, Does.Contain("before the fault"));
                Assert.That(copy, Does.Contain("after the fault"));

                string said = log.ReadAll();

                Assert.That(said, Does.Contain("from the lines held in memory, because the log file stopped taking lines"));

                log.WriteResultBlock();

                Assert.That(log.ReadAll(), Does.Contain("the file stopped taking lines part way, so it is short"));
            }
        }

        /// <summary>
        /// The flush a read or a copy makes is a write too, and the first the disk refuses may be that one.
        /// ReadAll and TryCopyTo called with no line between the fault and them find it themselves, say
        /// so once, and give the whole log from memory. The break: nothing came out of either call.
        /// </summary>
        [Test]
        public void AFlushThatFailsInAReadOrACopyIsTheFaultAndNeverAThrow()
        {
            string copies = Path.Combine(folder, "copies");

            using (RunLog log = Start())
            {
                List<string> heard = new List<string>();
                log.LineWritten += heard.Add;

                log.Line("before the fault");
                TheDiskFails(log);

                string whole = null;

                Assert.DoesNotThrow(() => whole = log.ReadAll());
                Assert.That(whole, Does.Contain("before the fault"));
                Assert.That(whole, Does.Contain("the log file stopped taking lines"));
                Assert.That(log.IsWritingToDisk, Is.False);
                Assert.That(CountOf(string.Join("\n", heard.ToArray()), "the log file stopped taking lines"), Is.EqualTo(1));
            }

            using (RunLog log = RunLog.Start(Path.Combine(folder, "second"), new DateTime(2026, 10, 7, 9, 0, 0)))
            {
                log.Line("before the fault");
                TheDiskFails(log);

                string copied = null;
                bool done = false;

                Assert.DoesNotThrow(() => done = log.TryCopyTo(copies, out copied));
                Assert.That(done, Is.True);
                Assert.That(File.ReadAllText(copied), Does.Contain("before the fault"));
                Assert.That(log.IsWritingToDisk, Is.False);
            }
        }

        /// <summary>
        /// A copy of a log that was only closed is not a fault. TryCopyTo flushed a writer that Dispose had
        /// closed, which threw, so a closed log could not be copied, and with a file that stops it would have
        /// been taken for the file stopping.
        /// </summary>
        [Test]
        public void ACopyOfAClosedLogIsNotAFileThatStopped()
        {
            RunLog log = Start();
            log.Line("a line");
            log.Dispose();

            string copied;
            bool done = log.TryCopyTo(Path.Combine(folder, "copies"), out copied);

            Assert.That(done, Is.True);
            Assert.That(File.ReadAllText(copied), Does.Contain("a line"));
            Assert.That(log.ReadAll(), Does.Not.Contain("stopped taking lines"));
        }

        /// <summary>
        /// A stream whose flush fails and which records that it was closed, for the one thing a closed
        /// stream cannot show: Dispose reaching a handle that is still open.
        /// </summary>
        private sealed class StreamThatCannotFlush : FileStream
        {
            internal StreamThatCannotFlush(string path)
                : base(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, 1024, false)
            {
            }

            internal bool WasClosed { get; private set; }

            public override void Flush()
            {
                throw new IOException("the disk is full");
            }

            public override void Flush(bool flushToDisk)
            {
                throw new IOException("the disk is full");
            }

            protected override void Dispose(bool disposing)
            {
                WasClosed = true;
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Closing a log whose file stopped must still close the handle. Dispose flushed first and left
        /// the writer and the stream open when the flush threw. Built through the log's private
        /// constructor with a stream that refuses to flush, because a stream already closed cannot show
        /// a handle left open.
        /// </summary>
        [Test]
        public void ADisposeAfterAFailedFlushStillClosesTheHandle()
        {
            string path = Path.Combine(folder, "run-flush.log");
            StreamThatCannotFlush refusing = new StreamThatCannotFlush(path);

            System.Reflection.ConstructorInfo constructor = typeof(RunLog).GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(DateTime), typeof(FileStream), typeof(string) },
                null);

            Assert.That(constructor, Is.Not.Null, "RunLog's constructor no longer takes a path, a time, a stream and a reason");

            RunLog log = (RunLog)constructor.Invoke(new object[] { path, new DateTime(2026, 10, 7, 9, 0, 0), refusing, null });

            Assert.DoesNotThrow(() => log.Line("a line the disk refuses"));
            Assert.That(log.IsWritingToDisk, Is.False);
            Assert.That(refusing.WasClosed, Is.False, "the stream is still open until the log is closed");

            Assert.DoesNotThrow(() => log.Dispose());
            Assert.That(refusing.WasClosed, Is.True, "Dispose left the handle open after a flush that failed");
        }

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
                Assert.That(text, Does.Contain("2,048 bytes"));
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

        /// <summary>
        /// A run counted to now has no stretch after it that anyone measured, because it ends where the block
        /// is written. The block printed an after the run finished row of 0.0 seconds, a number that reads as
        /// a measurement. It names the stretch as not measured and prints no row. The break: a run that
        /// finished keeps its row.
        /// </summary>
        [Test]
        public void ARunCountedToNowPrintsNoAfterTheRunRowAndSaysTheStretchWasNotMeasured()
        {
            string unfinished = string.Join("\n", new List<string>(RunClock.Unfinished(100.0, 40.0).Lines()).ToArray());

            Assert.That(unfinished, Does.Not.Contain(RunClock.AfterTheRun + " "));
            Assert.That(unfinished, Does.Contain("the run started and RUN finished was never marked"));
            Assert.That(unfinished, Does.Contain("the time after the run is not measured"));
            Assert.That(unfinished, Does.Contain(RunClock.WaitingForThePerson));

            string finished = string.Join("\n", new List<string>(RunClock.From(100.0, 40.0, 90.0).Lines()).ToArray());

            Assert.That(finished, Does.Contain(RunClock.AfterTheRun));
            Assert.That(finished, Does.Not.Contain("not measured"));

            using (RunLog log = Start())
            {
                log.RunStarted(2);
                log.WriteResultBlock();

                Assert.That(ReadWhileOpen(log.Path), Does.Not.Contain(RunClock.AfterTheRun + " "));
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
        /// Retention feeds a refused .tsv to its own count. The summary counted it among the logs that
        /// would not go, so one held .tsv beside a log that did go read as a log that stayed. A .tsv held
        /// open refuses its delete on Windows, which is a rule of the file system, so this one skips
        /// elsewhere and RetainLine's own test holds the sentence everywhere. The break: the log whose
        /// .tsv was held is gone, and counts as deleted.
        /// </summary>
        [Test]
        public void AHeldTsvIsCountedAsATsvAndNeverAsALogThatStayed()
        {
            TestPaths.OnWindowsOnly("a file held open refusing to be deleted");

            MakeOldLogs(folder, 35, true);
            string oldestLog = Path.Combine(folder, "run-20260101-000000.log");
            string oldestRows = Path.Combine(folder, "run-20260101-000000.tsv");

            using (FileStream hold = new FileStream(oldestRows, FileMode.Open, FileAccess.Read, FileShare.None))
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 7, 9, 0, 0), 30))
            {
                string text = ReadWhileOpen(log.Path);

                Assert.That(File.Exists(oldestLog), Is.False, "the log whose .tsv was held still goes");
                Assert.That(File.Exists(oldestRows), Is.True, "a held .tsv was somehow deleted");
                Assert.That(text, Does.Contain(
                    "RETAIN   keeping 30 logs, deleted 6, could not delete 0, and 5 .tsv beside them, 1 .tsv could not be deleted"));
                Assert.That(text, Does.Contain("RETAIN   could not delete run-20260101-000000.tsv"));
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
