using System;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F129. What a run of picked NWFs changes in the log and in the picker memory: the GROUP
    /// started line of an NWF whose models are known only once it opens, the RESULT line that
    /// counts the picked NWFs as groups, and the picker remembered on its own.
    /// </summary>
    [TestFixture]
    public class PickedNwfLogTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorPickedNwfLog");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static string Read(RunLog log)
        {
            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Test]
        public void AGroupStartedWithNoFileListSaysTheyAreReadAtTheOpenAndNotNoFiles()
        {
            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 8, 20, 0, 0)))
            {
                log.GroupStarted("1104-PAR-1A02MM-ZZZ-BM-MOD-000001", null);

                string text = Read(log);

                Assert.That(text, Does.Contain("GROUP    started  1104-PAR-1A02MM-ZZZ-BM-MOD-000001  "
                    + RunLog.GroupFilesReadAtTheOpen));
                Assert.That(text, Does.Not.Contain("0 files"));
            }
        }

        [Test]
        public void AGroupStartedWithAFileListStillCountsIt()
        {
            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 8, 20, 0, 0)))
            {
                log.GroupStarted("1A02MM", new[] { "a.nwc", "b.nwc" });

                Assert.That(Read(log), Does.Contain("GROUP    started  1A02MM  2 files"));
            }
        }

        [Test]
        public void TheResultBlockCarriesThePickedNwfsLineUnderTheGroupCounts()
        {
            string picked = "NWFs picked    : 2 from " + TestPaths.At("Feds") + ", each run as the open file run runs it";

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 8, 20, 0, 0)))
            {
                log.GroupFinished("a", GroupOutcome.Done, 1.0, null, RunPath.Label(RerunDecision.Open, false));
                log.GroupFinished("b", GroupOutcome.Failed, 1.0, "the NWF would not open", RunPath.Label(RerunDecision.Open, false));
                log.WriteResultBlock(null, pickedNwfs: picked);

                string text = Read(log);
                string result = text.Substring(text.LastIndexOf("RESULT", StringComparison.Ordinal));

                Assert.That(result, Does.Contain("groups done    : 1"));
                Assert.That(result, Does.Contain("groups failed  : 1"));
                Assert.That(result, Does.Contain(picked));
                Assert.That(result.IndexOf(picked, StringComparison.Ordinal),
                    Is.GreaterThan(result.IndexOf("groups failed  : 1", StringComparison.Ordinal)));
            }
        }

        [Test]
        public void TheResultBlockOfAnyOtherRunHasNoPickedNwfsLine()
        {
            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 8, 20, 0, 0)))
            {
                log.GroupFinished("a", GroupOutcome.Done, 1.0, null, RunPath.FirstRun);
                log.WriteResultBlock(null);

                Assert.That(Read(log), Does.Not.Contain("NWFs picked"));
            }
        }

        [Test]
        public void ThePickedNwfPickerRemembersItsOwnFolderAndMovesNoOther()
        {
            FolderMemory memory = FolderMemory.Load(Path.Combine(folder, FolderMemory.FileName));
            string nwfs = TestPaths.At("Feds");
            string output = TestPaths.At("Out", "nwf");

            memory.Remember(PickerKind.Nwf, output);
            memory.Remember(PickerKind.PickedNwf, nwfs);

            Assert.That(memory.LastFor(PickerKind.PickedNwf), Is.EqualTo(nwfs));
            Assert.That(memory.LastFor(PickerKind.Nwf), Is.EqualTo(output));

            FolderMemory again = FolderMemory.Load(Path.Combine(folder, FolderMemory.FileName));
            Assert.That(again.LastFor(PickerKind.PickedNwf), Is.EqualTo(nwfs), "it survives a restart");
        }
    }
}
