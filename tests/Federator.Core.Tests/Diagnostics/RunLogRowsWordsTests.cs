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
