using System;
using System.IO;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The run log and its .tsv copied into the Log folder, F158. Nothing already there is replaced
    /// and nothing is deleted, and the log is copied while the run still holds it open.
    /// </summary>
    [TestFixture]
    public class LogCopyTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorLogCopy");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        [Test]
        public void TheLogAndItsTsvAreCopiedWhileTheRunStillHoldsThemOpen()
        {
            string logs = Path.Combine(folder, "logs");
            string target = Path.Combine(folder, "NM fed", "Log");

            using (RunLog log = RunLog.Start(logs, new DateTime(2026, 10, 14, 2, 3, 23)))
            {
                log.Line("RUN      a line before the copy");

                string tsv = RowLog.PathFor(log.Path);
                var said = LogCopy.Into(target, log.Path, tsv, new DateTime(2026, 10, 14, 3, 0, 0));

                Assert.That(said.Count, Is.EqualTo(2));
                Assert.That(said[0], Does.StartWith("LOGS     copied "));
                Assert.That(File.ReadAllText(Path.Combine(target, Path.GetFileName(log.Path))),
                    Does.Contain("a line before the copy"));
                Assert.That(File.Exists(Path.Combine(target, Path.GetFileName(tsv))), Is.True);
            }
        }

        [Test]
        public void ANameAlreadyInTheLogFolderIsLeftAloneAndTheCopyTakesADatedName()
        {
            string source = Path.Combine(folder, "run-20261014-020323.log");
            string target = Path.Combine(folder, "Log");
            Directory.CreateDirectory(target);
            File.WriteAllText(source, "this run");
            File.WriteAllText(Path.Combine(target, "run-20261014-020323.log"), "an earlier copy");

            var said = LogCopy.Into(target, source, null, new DateTime(2026, 10, 14, 15, 30, 12));

            Assert.That(File.ReadAllText(Path.Combine(target, "run-20261014-020323.log")), Is.EqualTo("an earlier copy"),
                "the earlier file was replaced");
            Assert.That(File.ReadAllText(Path.Combine(target, "run-20261014-020323 2026-10-14 153012.log")),
                Is.EqualTo("this run"));
            Assert.That(said.Count, Is.EqualTo(1));
        }

        [Test]
        public void AMissingSourceOrNoFolderIsSaidAndNeverThrown()
        {
            Assert.That(LogCopy.Into(null, "a.log", null, DateTime.Now)[0], Does.Contain("no Log folder"));

            var said = LogCopy.Into(Path.Combine(folder, "Log"), Path.Combine(folder, "gone.log"), null, DateTime.Now);

            Assert.That(said[0], Does.Contain("is not on the disk"));
            Assert.That(Directory.Exists(Path.Combine(folder, "Log")), Is.False, "a folder was made for nothing");
        }
    }
}
