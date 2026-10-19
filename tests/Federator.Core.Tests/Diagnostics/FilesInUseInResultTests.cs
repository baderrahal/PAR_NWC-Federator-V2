using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Communities;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243, Bader's order of 19 Oct 2026 point 6. A file left open is said on its line in the log when it is met, kept for the
    /// run window's end message, and written in RESULT, which is what a group's own Navisworks hands back to the window that
    /// started the run, PackageRunResult.ResultLinesOf. A RESULT closes the account, so the next run starts with none.
    /// </summary>
    [TestFixture]
    public class FilesInUseInResultTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorFilesInUse");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static string ReadWhileOpen(RunLog log)
        {
            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Test]
        public void AFileLeftOpenIsSaidKeptOnceAndCarriedInResult()
        {
            string xlsx = Path.Combine(folder, "Clash report", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.xlsx");
            string html = Path.Combine(folder, "Clash report", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.html");

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 19, 21, 0, 0)))
            {
                log.NotWrittenWhileOpen(xlsx);
                log.NotWrittenWhileOpen(html);
                log.NotWrittenWhileOpen(xlsx);

                Assert.That(log.FilesInUse.Count, Is.EqualTo(2));
                Assert.That(log.FilesInUse[0], Is.EqualTo(FileInUse.Line(xlsx)));
                Assert.That(log.FilesInUse[1], Is.EqualTo(FileInUse.Line(html)));
                Assert.That(ReadWhileOpen(log), Does.Contain(FileInUse.Line(xlsx)));

                log.WriteResultBlock();

                IList<string> result = PackageRunResult.ResultLinesOf(ReadWhileOpen(log).Replace("\r\n", "\n").Split('\n'));
                List<string> carried = new List<string>();

                foreach (string line in result)
                {
                    string found = FileInUse.LineIn(line);

                    if (found != null)
                    {
                        carried.Add(found);
                    }
                }

                Assert.That(carried.Count, Is.EqualTo(2), "each file once in RESULT");
                Assert.That(carried[0], Is.EqualTo(FileInUse.Line(xlsx)));
                Assert.That(carried[1], Is.EqualTo(FileInUse.Line(html)));
                Assert.That(log.FilesInUse.Count, Is.EqualTo(0), "a RESULT closes the account");
            }
        }

        /// <summary>The break: a run with no file left open writes no such line in RESULT.</summary>
        [Test]
        public void ARunWithNoFileLeftOpenSaysNoneOfItInResult()
        {
            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 19, 21, 0, 0)))
            {
                log.WriteResultBlock();

                foreach (string line in PackageRunResult.ResultLinesOf(ReadWhileOpen(log).Replace("\r\n", "\n").Split('\n')))
                {
                    Assert.That(FileInUse.LineIn(line), Is.Null, line);
                }

                Assert.That(log.FilesInUse.Count, Is.EqualTo(0));
            }
        }
    }
}
