using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// One run threw the same ObjectDisposedException tens of thousands of times and left
    /// a 17.8 MB log that was almost entirely one stack trace repeated. The trace is
    /// written once and the repeats are counted.
    /// </summary>
    [TestFixture]
    public class RepeatedFailureLogTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(
                Path.GetTempPath(), "FederatorRepeatFailures", Guid.NewGuid().ToString("N"));
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

        /// <summary>
        /// An exception carrying a real stack trace, so the dedupe is exercised on the
        /// thing it actually keys on rather than on a message alone.
        /// </summary>
        private static Exception Thrown(string message)
        {
            try
            {
                throw new ObjectDisposedException("NativeHandle", message);
            }
            catch (Exception caught)
            {
                return caught;
            }
        }

        // ---------- one trace, then a count ----------

        [Test]
        public void TheSameFailureIsWrittenOutOnceAndThenCounted()
        {
            using (RunLog log = Start())
            {
                Exception error = Thrown("Object has been Disposed (WeakRef)");

                for (int i = 0; i < 1830; i++)
                {
                    log.Failure("clash test T" + 1, error, "kept going");
                }

                Assert.That(log.Failures.Count, Is.EqualTo(1),
                    "one trace repeating is one thing that went wrong");
                Assert.That(log.Failures[0].Times, Is.EqualTo(1830),
                    "every repeat still has to be counted");
            }
        }

        [Test]
        public void TheStackTraceAppearsOnceInTheLogFile()
        {
            using (RunLog log = Start())
            {
                Exception error = Thrown("Object has been Disposed (WeakRef)");

                for (int i = 0; i < 500; i++)
                {
                    log.Failure("clash test T1", error, "kept going");
                }

                string text = ReadWhileOpen(log);
                int traces = Occurrences(text, "System.ObjectDisposedException");

                Assert.That(traces, Is.LessThanOrEqualTo(2),
                    "the trace was written out for every repeat, which is the 17.8 MB log");
                Assert.That(text,
                    Does.Contain("Every further repeat of this exact trace is counted, not written out."));
            }
        }

        // The whole point is the size of the file.
        [Test]
        public void ThousandsOfRepeatsDoNotProduceAHugeLog()
        {
            using (RunLog log = Start())
            {
                Exception error = Thrown("Object has been Disposed (WeakRef)");

                for (int i = 0; i < 40000; i++)
                {
                    log.Failure("clash test T1", error, "kept going");
                }

                long size = new FileInfo(log.Path).Length;

                Assert.That(size, Is.LessThan(64 * 1024),
                    "40000 repeats wrote " + size + " bytes, so they are still being written out");
            }
        }

        [Test]
        public void TheResultBlockSaysHowManyTimesItHappened()
        {
            using (RunLog log = Start())
            {
                Exception error = Thrown("Object has been Disposed (WeakRef)");

                for (int i = 0; i < 43920; i++)
                {
                    log.Failure("clash test T1", error, "kept going");
                }

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("THIS HAPPENED 43920 TIMES"));
                Assert.That(text, Does.Contain("the trace is written once"));
                Assert.That(text, Does.Contain("errors         : 1"),
                    "one trace repeating is one error, not 43920 of them");
            }
        }

        // ---------- different failures are still kept apart ----------

        [Test]
        public void TwoDifferentFailuresAreBothWrittenOut()
        {
            using (RunLog log = Start())
            {
                log.Failure("appending a file", Thrown("the file is locked"), "kept going");
                log.Failure("saving the NWF", Thrown("no such drive"), "kept going");

                Assert.That(log.Failures.Count, Is.EqualTo(2));
                Assert.That(log.Failures[0].Times, Is.EqualTo(1));
                Assert.That(log.Failures[1].Times, Is.EqualTo(1));

                string text = ReadWhileOpen(log);
                Assert.That(text, Does.Contain("the file is locked"));
                Assert.That(text, Does.Contain("no such drive"));
            }
        }

        // The same exception under a different heading is a different failure, because
        // which test threw is part of what happened.
        [Test]
        public void TheSameExceptionUnderTwoHeadingsIsTwoFailures()
        {
            using (RunLog log = Start())
            {
                Exception error = Thrown("Object has been Disposed (WeakRef)");

                log.Failure("clash test AR v ME", error, "kept going");
                log.Failure("clash test AR v ST", error, "kept going");

                Assert.That(log.Failures.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public void AFailureThatHappenedOnceCarriesNoRepeatCount()
        {
            using (RunLog log = Start())
            {
                log.Failure("saving the NWF", Thrown("locked"), "kept going");
                log.WriteResultBlock();

                string text = ReadWhileOpen(log);

                Assert.That(log.Failures[0].Times, Is.EqualTo(1));
                Assert.That(text, Does.Not.Contain("THIS HAPPENED"));
            }
        }

        [Test]
        public void ANullExceptionStillLogsAndStillDedupes()
        {
            using (RunLog log = Start())
            {
                log.Failure("something", null, "kept going");
                log.Failure("something", null, "kept going");

                Assert.That(log.Failures.Count, Is.EqualTo(1));
                Assert.That(log.Failures[0].Times, Is.EqualTo(2));
                Assert.That(ReadWhileOpen(log), Does.Contain("no exception was supplied"));
            }
        }

        // Logging is never the thing that stops a run, dedupe included.
        [Test]
        public void TheFirstFailureStillReadsExactlyAsItAlwaysDid()
        {
            using (RunLog log = Start())
            {
                log.Failure("appending a file", Thrown("the file is locked"), "carried on");

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("FAILURE  appending a file"));
                Assert.That(text, Does.Contain("type     : System.ObjectDisposedException"));
                Assert.That(text, Does.Contain("message  :"));
                Assert.That(text, Does.Contain("stack    :"));
                Assert.That(text, Does.Contain("next     : carried on"));
            }
        }

        private static int Occurrences(string text, string needle)
        {
            int count = 0;
            int at = 0;

            while ((at = text.IndexOf(needle, at, StringComparison.Ordinal)) >= 0)
            {
                count++;
                at += needle.Length;
            }

            return count;
        }
    }
}
