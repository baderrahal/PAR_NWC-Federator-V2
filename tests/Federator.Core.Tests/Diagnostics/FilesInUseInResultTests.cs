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
                Assert.That(ReadWhileOpen(log), Does.Contain("OPEN     " + xlsx + " not written, it is open in another program"));

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

        /// <summary>
        /// F243 attempt 2, the reviewer's N4 and the breaker's C2. The window that started a run of packages writes its own files
        /// left open, the cleanup's and its workbooks', into the joined RESULT beside the groups' own RESULT lines, in the words
        /// RESULT gives them everywhere.
        /// </summary>
        [Test]
        public void TheWindowsOwnFilesLeftOpenAreInItsJoinedResult()
        {
            string own = FileInUse.Line(Path.Combine(folder, "C06 clash count check.xlsx"));
            List<PackageRunUnit> units = new List<PackageRunUnit> { new PackageRunUnit("C06", Path.Combine(folder, "a.nwf"), "1B06BC") };
            List<PackageRunResult> results = new List<PackageRunResult> { new PackageRunResult("C06", null) };

            IList<string> lines = PackageRunWords.JoinedResult(units, results, new List<string> { null }, new PackageTally(), new List<string> { own });

            Assert.That(lines, Does.Contain("files open     : 1 left as it was, open in another program"));
            Assert.That(lines, Does.Contain(own));
            Assert.That(FileInUse.ResultLines(new List<string> { own }), Is.EqualTo(new List<string>
            {
                "files open     : 1 left as it was, open in another program",
                own
            }));
            Assert.That(FileInUse.ResultLines(new List<string>()).Count, Is.EqualTo(0));
            Assert.That(PackageRunWords.JoinedResult(units, results, new List<string> { null }, new PackageTally()),
                Has.None.StartsWith("files open"), "a run with none of its own says nothing of it");
        }

        /// <summary>
        /// F243 attempt 3, the reviewer's N3. Each file left open says what was left undone, not written only where a write was
        /// skipped, not deleted where a delete was, not read where a copy could not read it, and the Close line is Bader's.
        /// </summary>
        [Test]
        public void EachFileLeftOpenSaysWhatWasLeftUndone()
        {
            string old = Path.Combine(folder, "Log", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001 Internal.xlsx");
            string nwf = Path.Combine(folder, "NWF", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf");

            using (RunLog log = RunLog.Start(folder, new DateTime(2026, 10, 19, 21, 0, 0)))
            {
                log.NotDeletedWhileOpen(old);
                log.NotReadWhileOpen(nwf);

                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("OPEN     " + old + " not deleted, it is open in another program"));
                Assert.That(text, Does.Contain("OPEN     " + nwf + " not read, so nothing was copied from it, it is open in another program"));
                Assert.That(text, Does.Contain(FileInUse.Line(old)));
                Assert.That(text, Does.Not.Contain(old + " not written"));
                Assert.That(log.FilesInUse.Count, Is.EqualTo(2));
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
