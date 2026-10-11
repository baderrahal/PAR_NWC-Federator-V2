using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F143. A long Navisworks call is named in the .log and carries its .tsv row, the seconds as
    /// its number, the rule that a line with a number reaches the log through RunLog.Numbered. Read
    /// back off the disk as files, never through the log object.
    /// </summary>
    [TestFixture]
    public class LongCallLogTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorLongCall");
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

        [Test]
        public void ALongCallIsInTheLogAndItsRowCarriesTheSeconds()
        {
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 11, 18, 0, 0)))
            {
                LongCall.Say(log, "TestsImageForResult", "BLD-A-vs-BLD-B Clash12", 41.26, 30.0);

                string text = ReadWhileOpen(log.Path);
                string rows = ReadWhileOpen(log.RowLogPath);

                Assert.That(text, Does.Contain("CALL     TestsImageForResult on BLD-A-vs-BLD-B Clash12 took 41.3s, one Navisworks call"));
                Assert.That(rows, Does.Contain("long call\tTestsImageForResult\t41.3\tBLD-A-vs-BLD-B Clash12"));
            }
        }

        [Test]
        public void ACallUnderTheLineWritesNeitherLineNorRow()
        {
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 11, 18, 0, 0)))
            {
                LongCall.Say(log, "TestsRunTest", "BLD-A-vs-BLD-B", 29.9, 30.0);

                Assert.That(ReadWhileOpen(log.Path), Does.Not.Contain("CALL     "));
                Assert.That(ReadWhileOpen(log.RowLogPath), Does.Not.Contain("long call"));
            }
        }

        [Test]
        public void ACallOfExactlyTheLineIsNamed()
        {
            Assert.That(LongCall.Line("TestsRunTest", "x", 30.0, 30.0), Does.StartWith("CALL     TestsRunTest on x took 30.0s"));
        }

        [Test]
        public void NoLogWritesNothingAndDoesNotThrow()
        {
            Assert.DoesNotThrow(() => LongCall.Say(null, "TestsRunTest", "x", 99.0, 30.0));
        }
    }
}
