using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The process's memory at the start and the end of each step, F147, Bader's answer to Q155
    /// on 2026-10-12, read back off the disk as the log wrote it. The tests hand in a reader of
    /// their own, as the census tests do, so the numbers are known and the rule is proved without
    /// a process whose memory moves under it.
    /// </summary>
    [TestFixture]
    public class RunLogMemoryTests
    {
        private const long Megabyte = 1024L * 1024;

        private string folder;
        private List<ProcessMemory> answers;
        private int asked;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorRunLogMemory");
            answers = new List<ProcessMemory>();
            asked = 0;
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        [Test]
        public void AStepSaysTheMemoryAtItsStartAndHowFarItMovedAtItsEnd()
        {
            answers.Add(new ProcessMemory(100 * Megabyte, 200 * Megabyte, 10 * Megabyte, null));
            answers.Add(new ProcessMemory(150 * Megabyte, 180 * Megabyte, 10 * Megabyte, null));

            using (RunLog log = Start())
            {
                log.MemoryReader = Next;

                using (RunStep step = log.Step(RunSteps.Harvest))
                {
                    step.Changed("read");
                }

                string[] lines = Lines(log);
                int started = IndexOf(lines, "STEP     HARVEST");
                int finished = IndexOf(lines, "finished");

                Assert.That(lines[started + 1], Does.EndWith(
                    "MEMORY   at the start of HARVEST, private bytes 100.0 MB, working set 200.0 MB, managed heap 10.0 MB"));
                Assert.That(lines[finished - 1], Does.EndWith(
                    "MEMORY   at the end of HARVEST, private bytes 150.0 MB up 50.0 MB, working set 180.0 MB down 20.0 MB, managed heap 10.0 MB unchanged"));
                Assert.That(asked, Is.EqualTo(2));
            }
        }

        [Test]
        public void WithNoReaderNoMemoryLineIsWritten()
        {
            using (RunLog log = Start())
            {
                using (log.Step(RunSteps.Nwd))
                {
                }

                Assert.That(Text(log), Does.Not.Contain("MEMORY"));
            }
        }

        // The cost guard: TESTS RUN is entered once a test, and its memory is read where its own
        // start and finish lines are written, the first visit of the group, and not at every test.
        [Test]
        public void AStepEnteredAgainInAGroupReadsTheMemoryOnlyAroundItsFirstVisit()
        {
            using (RunLog log = Start())
            {
                log.MemoryReader = Next;
                log.GroupStarted("1B06PH", new List<string>());

                for (int test = 0; test < 5; test++)
                {
                    using (log.Step(RunSteps.TestsRun))
                    {
                    }
                }

                log.GroupFinished("1B06PH", GroupOutcome.Done, 1.0, null, "Weekly run");
                string text = Text(log);

                Assert.That(asked, Is.EqualTo(2));
                Assert.That(Occurrences(text, "MEMORY   at the start of TESTS RUN"), Is.EqualTo(1));
                Assert.That(Occurrences(text, "MEMORY   at the end of TESTS RUN"), Is.EqualTo(1));
            }
        }

        // Each of two nested steps carries its own pair, the inner one inside the outer one's.
        [Test]
        public void ANestedStepHasItsOwnPairInsideItsParents()
        {
            using (RunLog log = Start())
            {
                log.MemoryReader = Next;

                using (log.Step(RunSteps.Sets))
                {
                    using (log.Step(RunSteps.TestsCreate))
                    {
                    }
                }

                string text = Text(log);
                int outerStart = text.IndexOf("MEMORY   at the start of SETS", StringComparison.Ordinal);
                int innerStart = text.IndexOf("MEMORY   at the start of TESTS CREATE", StringComparison.Ordinal);
                int innerEnd = text.IndexOf("MEMORY   at the end of TESTS CREATE", StringComparison.Ordinal);
                int outerEnd = text.IndexOf("MEMORY   at the end of SETS", StringComparison.Ordinal);

                Assert.That(outerStart, Is.GreaterThanOrEqualTo(0));
                Assert.That(innerStart, Is.GreaterThan(outerStart));
                Assert.That(innerEnd, Is.GreaterThan(innerStart));
                Assert.That(outerEnd, Is.GreaterThan(innerEnd));
            }
        }

        // Logging never stops a run: a reader that throws is said as the reason the numbers are
        // UNKNOWN, never nought, and the step still finishes.
        [Test]
        public void AReaderThatThrowsIsSaidAndTheNumbersAreUnknownNeverNought()
        {
            using (RunLog log = Start())
            {
                log.MemoryReader = () => { throw new InvalidOperationException("the process has exited"); };

                using (RunStep step = log.Step(RunSteps.Nwd))
                {
                    step.Changed("published");
                }

                string text = Text(log);

                Assert.That(text, Does.Contain(
                    "MEMORY   at the start of NWD, private bytes UNKNOWN, working set UNKNOWN, managed heap UNKNOWN, UNKNOWN because InvalidOperationException: the process has exited"));
                Assert.That(text, Does.Contain(
                    "MEMORY   at the end of NWD, private bytes UNKNOWN moved UNKNOWN, working set UNKNOWN moved UNKNOWN, managed heap UNKNOWN moved UNKNOWN, UNKNOWN because InvalidOperationException"));
                Assert.That(text, Does.Contain("finished"));
                Assert.That(text, Does.Not.Contain(" 0.0 MB"));
            }
        }

        [Test]
        public void ANumberReadAtOneEndOnlyMovedUnknown()
        {
            ProcessMemory start = new ProcessMemory(-1, 200 * Megabyte, 10 * Megabyte, "Win32Exception: Access is denied");
            ProcessMemory end = new ProcessMemory(120 * Megabyte, 200 * Megabyte, 12 * Megabyte, null);

            Assert.That(
                ProcessMemory.EndLine(RunSteps.Views, start, end),
                Is.EqualTo("MEMORY   at the end of VIEWS, private bytes 120.0 MB moved UNKNOWN, working set 200.0 MB unchanged, managed heap 12.0 MB up 2.0 MB"));
        }

        // The reader the engine hands the log reads this process's own numbers, in bytes.
        [Test]
        public void TheProcessReadsItsOwnMemory()
        {
            ProcessMemory now = ProcessMemory.Read();

            Assert.That(now.ManagedBytes, Is.GreaterThan(0));

            if (now.NotReadBecause == null)
            {
                Assert.That(now.PrivateBytes, Is.GreaterThan(0));
                Assert.That(now.WorkingSet, Is.GreaterThan(0));
            }
            else
            {
                Assert.That(now.PrivateBytes, Is.EqualTo(-1), "a number not read is UNKNOWN, " + now.NotReadBecause);
            }
        }

        private ProcessMemory Next()
        {
            asked++;

            if (answers.Count == 0)
            {
                return new ProcessMemory(100 * Megabyte, 100 * Megabyte, 10 * Megabyte, null);
            }

            return answers[Math.Min(asked - 1, answers.Count - 1)];
        }

        private RunLog Start()
        {
            return RunLog.Start(folder, new DateTime(2026, 10, 12, 9, 0, 0));
        }

        private static string Text(RunLog log)
        {
            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static string[] Lines(RunLog log)
        {
            return Text(log).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        private static int IndexOf(string[] lines, string what)
        {
            for (int at = 0; at < lines.Length; at++)
            {
                if (lines[at].Contains(what))
                {
                    return at;
                }
            }

            Assert.Fail("no line holds " + what);
            return -1;
        }

        private static int Occurrences(string text, string what)
        {
            int count = 0;
            int at = text.IndexOf(what, StringComparison.Ordinal);

            while (at >= 0)
            {
                count++;
                at = text.IndexOf(what, at + 1, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
