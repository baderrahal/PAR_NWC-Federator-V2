using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// FR-061. The text log said every collapsed line is in the .tsv beside it, whether or not the
    /// .tsv opened, so a log with no machine readable file contradicted itself.
    /// </summary>
    [TestFixture]
    public class RunLogRowsWordsTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorRunLogRowsWords");
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

        private static readonly DateTime Started = new DateTime(2026, 10, 7, 9, 0, 0);

        private static void SixRepeatsAndTheResult(RunLog log)
        {
            for (int i = 0; i < 6; i++)
            {
                log.NumberedRepeat("one key", "line " + i, "h", "n", "1", "t");
            }

            log.WriteResultBlock();
        }

        [Test]
        public void WithTheTsvOpenTheLogSaysEveryCollapsedLineIsKeptThere()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                SixRepeatsAndTheResult(log);

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Contain("The machine readable log carries every one of them"));
                Assert.That(text, Does.Contain("Every one is in the machine readable log"));
                Assert.That(text, Does.Contain("all of them kept in the .tsv beside it"));
            }
        }

        /// <summary>
        /// A write to the .tsv that fails is made by closing the stream the row file holds, which is how FR-057's tests
        /// fail the text log. It reaches two private fields because no running code needs a member to do this, and it
        /// returns the message the closed stream throws, so a test can look for it.
        /// </summary>
        private static string TheRowFileFails(object rowLogOrRunLog)
        {
            object rowLog = rowLogOrRunLog;

            if (rowLogOrRunLog is RunLog)
            {
                System.Reflection.FieldInfo held = typeof(RunLog).GetField(
                    "rows", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                Assert.That(held, Is.Not.Null, "RunLog no longer holds a field named rows, so this fault cannot be made");
                rowLog = held.GetValue(rowLogOrRunLog);
            }

            System.Reflection.FieldInfo field = typeof(RowLog).GetField(
                "stream", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, "RowLog no longer holds a field named stream, so this fault cannot be made");

            FileStream stream = (FileStream)field.GetValue(rowLog);
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

        private static int CountOf(string text, string what)
        {
            int count = 0;

            for (int at = text.IndexOf(what, StringComparison.Ordinal); at >= 0; at = text.IndexOf(what, at + what.Length, StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// FR-061's side. A write to the .tsv that threw was swallowed with no line and no flag, so the text log went
        /// on saying the .tsv carries every collapsed line, on a full disk. It is said once, with what threw, the
        /// sentences about the .tsv stop claiming the rows after that, the file keeps the rows before it and none
        /// after, and the run goes on. The break: before the fault the same sentences still say every one is kept.
        /// </summary>
        [Test]
        public void AFailedRowWriteIsSaidOnceAndNoSentenceClaimsTheRowsAfterIt()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                for (int i = 0; i < 3; i++)
                {
                    log.NumberedRepeat("one key", "line " + i, "h", "n", "1", "row" + i);
                }

                string whatItThrows = TheRowFileFails(log);

                for (int i = 3; i < 9; i++)
                {
                    int at = i;
                    Assert.DoesNotThrow(() => log.NumberedRepeat("one key", "line " + at, "h", "n", "1", "row" + at));
                }

                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);

                Assert.That(log.IsWritingToDisk, Is.True, "the text log is unaffected");
                Assert.That(CountOf(text, "ROWS     the machine readable log stopped taking rows"), Is.EqualTo(1), "said once, not for every row");
                Assert.That(text, Does.Contain("ObjectDisposedException: " + whatItThrows.TrimEnd('.') + ". It holds"));
                Assert.That(text, Does.Contain("It holds the rows written before this one and none after it, the one that failed may be cut short, and the text log is unaffected"));
                Assert.That(text, Does.Contain("The machine readable log stopped taking rows, so the ones after that are kept nowhere"));
                Assert.That(text, Does.Contain("Those counted before the machine readable log stopped taking rows are in it and any after that are kept nowhere"));
                Assert.That(text, Does.Contain("lines collapsed in this file, and the .tsv stopped taking rows part way, so those after that are kept nowhere:"));
                Assert.That(text, Does.Contain("and it stopped taking rows part way, so it is short"));
                Assert.That(text, Does.Not.Contain("The machine readable log carries every one of them"));
                Assert.That(text, Does.Not.Contain("Every one is in the machine readable log"));
                Assert.That(text, Does.Not.Contain("all of them kept in the .tsv beside it"));
                Assert.That(text, Does.Not.Contain("which keeps every line the .log collapsed"));

                foreach (string line in text.Split('\n'))
                {
                    if (line.Contains("stopped taking rows, "))
                    {
                        Assert.That(line, Does.Not.Contain(".."), "the message ends in a full stop and the sentence adds its own");
                    }
                }

                string rows = ReadWhileOpen(log.RowLogPath);

                Assert.That(rows, Does.Contain("row0"));
                Assert.That(rows, Does.Contain("row2"));
                Assert.That(rows, Does.Not.Contain("row3"), "no row after the fault is in the file");
                Assert.That(rows, Does.Not.Contain("row8"));
            }
        }

        /// <summary>
        /// A fault after the sixth repeat, which has already said the .tsv carries every one of them. That sentence was
        /// true when it was written and stays, the fault is told once after it, and the RESULT block and the size line
        /// are the ones that correct it.
        /// </summary>
        [Test]
        public void AFaultAfterTheSixthRepeatLeavesTheEarlierSentenceAndTheResultCorrectsIt()
        {
            using (RunLog log = RunLog.Start(folder, Started))
            {
                for (int i = 0; i < 6; i++)
                {
                    log.NumberedRepeat("one key", "line " + i, "h", "n", "1", "row" + i);
                }

                TheRowFileFails(log);

                for (int i = 6; i < 9; i++)
                {
                    log.NumberedRepeat("one key", "line " + i, "h", "n", "1", "row" + i);
                }

                log.WriteResultBlock();

                string text = ReadWhileOpen(log.Path);
                int carried = text.IndexOf("The machine readable log carries every one of them", StringComparison.Ordinal);
                int told = text.IndexOf("ROWS     the machine readable log stopped taking rows", StringComparison.Ordinal);

                Assert.That(carried, Is.GreaterThan(0), "it was true when the sixth repeat was written");
                Assert.That(told, Is.GreaterThan(carried), "the fault is told after it, once");
                Assert.That(CountOf(text, "ROWS     the machine readable log stopped taking rows"), Is.EqualTo(1));
                Assert.That(CountOf(text, "The machine readable log carries every one of them"), Is.EqualTo(1));
                Assert.That(text, Does.Not.Contain("Every one is in the machine readable log"));
                Assert.That(text, Does.Not.Contain("all of them kept in the .tsv beside it"));
                Assert.That(text, Does.Contain("Those counted before the machine readable log stopped taking rows are in it and any after that are kept nowhere"));
                Assert.That(text, Does.Contain("and it stopped taking rows part way, so it is short"));
            }
        }

        /// <summary>
        /// A stream whose flush fails with a message that ends in a full stop, as a full disk does, and which records
        /// that it was closed, for the one thing a closed stream cannot show: the handle of a file given up on.
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
                throw new IOException("There is not enough space on the disk.");
            }

            public override void Flush(bool flushToDisk)
            {
                throw new IOException("There is not enough space on the disk.");
            }

            protected override void Dispose(bool disposing)
            {
                WasClosed = true;
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// A header that could not be written is a file that could not be used, so the line that names the file says
        /// there is none and why, where it said UNKNOWN, and the handle of the file given up on is closed even though
        /// its flush threw.
        /// </summary>
        [Test]
        public void ARowFileWhoseHeaderCannotBeWrittenIsGivenUpWithTheCause()
        {
            StreamThatCannotFlush refusing = new StreamThatCannotFlush(Path.Combine(folder, "header.tsv"));

            using (RowLog rows = RowLog.Opened(refusing.Name, refusing))
            {
                Assert.That(rows.Path, Is.Null);
                Assert.That(rows.IsWritingToDisk, Is.False);
                Assert.That(rows.WhyNot, Is.EqualTo("IOException: There is not enough space on the disk"));
                Assert.That(rows.WhereItIs(),
                    Is.EqualTo("ROWS     no machine readable log. IOException: There is not enough space on the disk. The text log is unaffected"));
                Assert.That(refusing.WasClosed, Is.True, "a flush that threw must not leave the handle open");
            }
        }

        /// <summary>The row file on its own: a write that threw is kept and handed over once, it stops writing, and it never throws.</summary>
        [Test]
        public void ARowFileThatFailedHandsOverItsFaultOnceAndStopsWriting()
        {
            using (RowLog rows = RowLog.StartBeside(Path.Combine(folder, "x.log")))
            {
                EventRow row = new EventRow("09:00:00.000", 1.0, "g", "s", "h", "n", "1", "t");

                Assert.That(rows.IsWritingToDisk, Is.True);
                Assert.That(rows.Stopped, Is.False);
                Assert.That(rows.TakeTheFaultToTell(), Is.Null, "nothing has failed");

                TheRowFileFails(rows);

                Assert.DoesNotThrow(() => rows.Write(row));
                Assert.DoesNotThrow(() => rows.Write(row));
                Assert.That(rows.Stopped, Is.True);
                Assert.That(rows.IsWritingToDisk, Is.False);
                Assert.That(rows.TakeTheFaultToTell(), Does.StartWith("ObjectDisposedException: "));
                Assert.That(rows.TakeTheFaultToTell(), Is.Null, "handed over once");
            }
        }

        /// <summary>
        /// Stops writing, held apart from a closed stream that fails again whatever is asked of it: a working stream put
        /// where the closed one was takes no row after the fault, and the first cause stays the cause.
        /// </summary>
        [Test]
        public void ARowFileThatFailedNeverTriesAgain()
        {
            using (RowLog rows = RowLog.StartBeside(Path.Combine(folder, "x.log")))
            {
                string first = TheRowFileFails(rows);
                rows.Write(new EventRow("09:00:00.000", 1.0, "g", "s", "h", "n", "1", "before"));

                string other = Path.Combine(folder, "other.tsv");
                FileStream working = new FileStream(other, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                using (StreamWriter replacement = new StreamWriter(working, new UTF8Encoding(true)))
                {
                    SetPrivate(rows, "stream", working);
                    SetPrivate(rows, "writer", replacement);

                    rows.Write(new EventRow("09:00:01.000", 2.0, "g", "s", "h", "n", "1", "after"));
                }

                Assert.That(ReadWhileOpen(other), Is.Empty, "the row after the fault was never attempted");
                Assert.That(rows.TakeTheFaultToTell(), Does.StartWith("ObjectDisposedException: " + first.TrimEnd('.')), "the first cause is kept");
            }
        }

        private static void SetPrivate(object target, string name, object value)
        {
            System.Reflection.FieldInfo field = typeof(RowLog).GetField(
                name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, "RowLog no longer holds a field named " + name + ", so this fault cannot be made");
            field.SetValue(target, value);
        }

        [Test]
        public void WithNoTsvOpenTheLogNeverSaysTheCollapsedLinesAreKeptThere()
        {
            Directory.CreateDirectory(Path.Combine(folder, "run-20261007-090000.tsv"));

            using (RunLog log = RunLog.Start(folder, Started))
            {
                Assert.That(log.RowLogPath, Is.Null, "the .tsv did not open, a folder sits at its path");

                SixRepeatsAndTheResult(log);

                string text = ReadWhileOpen(log.Path);

                Assert.That(text, Does.Not.Contain("The machine readable log carries every one of them"));
                Assert.That(text, Does.Not.Contain("Every one is in the machine readable log"));
                Assert.That(text, Does.Not.Contain("all of them kept in the .tsv beside it"));
                Assert.That(text, Does.Contain("No machine readable log is open, so none of them is kept"));
                Assert.That(text, Does.Contain("None is kept, no machine readable log is open"));
                Assert.That(text, Does.Contain("and no .tsv is open, so none of them is kept"));
            }
        }
    }
}
