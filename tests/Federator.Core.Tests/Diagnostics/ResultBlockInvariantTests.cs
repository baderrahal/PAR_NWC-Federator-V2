using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The RESULT block once printed "groups failed: 22" and "Nothing failed." in the same
    /// block with an empty error list, because the count came from one place and the errors
    /// from another. Everything about group outcomes now derives from one list.
    /// </summary>
    [TestFixture]
    public class ResultBlockInvariantTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(
                Path.GetTempPath(), "FederatorResultBlock", Guid.NewGuid().ToString("N"));
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

        // The one the brief asked for by name.
        [Test]
        public void ANonZeroFailedCountAlwaysCarriesAtLeastOneError()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07BC", GroupOutcome.Failed, 1.0, "the NWF is not on disk", null);

                Assert.That(log.CountOf(GroupOutcome.Failed), Is.EqualTo(1));

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups failed  : 1"));
                Assert.That(text, Does.Not.Contain("Nothing failed."),
                    "a failed count and Nothing failed cannot both be true");
                Assert.That(text, Does.Contain("errors         : 1"));
                Assert.That(text, Does.Contain("the NWF is not on disk"));
            }
        }

        // The exact contradiction from the real run: 22 failed groups, no exception anywhere.
        [Test]
        public void TwentyTwoFailedGroupsAndNoExceptionStillProducesTwentyTwoErrors()
        {
            using (RunLog log = Start())
            {
                for (int i = 0; i < 22; i++)
                {
                    log.GroupFinished(
                        "1B06P" + i, GroupOutcome.Failed, 1.0,
                        "the NWD was requested and is not on disk", null);
                }

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(log.CountOf(GroupOutcome.Failed), Is.EqualTo(22));
                Assert.That(log.FailedGroups.Count, Is.EqualTo(22));
                Assert.That(text, Does.Contain("groups failed  : 22"));
                Assert.That(text, Does.Contain("errors         : 22"));
                Assert.That(text, Does.Not.Contain("Nothing failed."));
            }
        }

        // Even a caller that forgets the reason cannot produce a bare count, because one is
        // substituted rather than thrown over. Logging never stops a run.
        [Test]
        public void AFailedGroupWithNoReasonGivenStillCarriesOne()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07BC", GroupOutcome.Failed, 1.0, null, null);

                Assert.That(log.FailedGroups.Count, Is.EqualTo(1));
                Assert.That(log.FailedGroups[0].Reason, Is.Not.Null.And.Not.Empty);
                Assert.That(log.FailedGroups[0].Reason, Does.Contain("UNKNOWN"));

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Not.Contain("Nothing failed."));
                Assert.That(text, Does.Contain("errors         : 1"));
            }
        }

        [Test]
        public void TheFailedCountAndTheFailedListAlwaysAgree()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("a", GroupOutcome.Done, 1.0, null, null);
                log.GroupFinished("b", GroupOutcome.Partial, 1.0, "one file did not append", null);
                log.GroupFinished("c", GroupOutcome.Failed, 1.0, "nothing appended", null);
                log.GroupFinished("d", GroupOutcome.Failed, 1.0, "the NWF is not on disk", null);

                Assert.That(log.CountOf(GroupOutcome.Failed), Is.EqualTo(log.FailedGroups.Count),
                    "the count and the list came from different places");
                Assert.That(log.CountOf(GroupOutcome.Done), Is.EqualTo(1));
                Assert.That(log.CountOf(GroupOutcome.Partial), Is.EqualTo(1));
                Assert.That(log.CountOf(GroupOutcome.Failed), Is.EqualTo(2));

                // All four groups reached the one list, read off the block on the disk
                // rather than off the list the block is built from.
                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups done    : 1"));
                Assert.That(text, Does.Contain("groups partial : 1"));
                Assert.That(text, Does.Contain("groups failed  : 2"));
            }
        }

        [Test]
        public void ACleanRunStillSaysNothingFailed()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("a", GroupOutcome.Done, 1.0, null, null);
                log.GroupFinished("b", GroupOutcome.Done, 1.0, null, null);

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups failed  : 0"));
                Assert.That(text, Does.Contain("Nothing failed."));
                Assert.That(text, Does.Not.Contain("errors         :"));
            }
        }

        // A PARTIAL group is not a failure, so it must not push the errors count up, but
        // its reason is still on its group line.
        [Test]
        public void APartialGroupIsNotCountedAsAnError()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07BC", GroupOutcome.Partial, 1.0, "1 of 4 files did not append", null);

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups partial : 1"));
                Assert.That(text, Does.Contain("groups failed  : 0"));
                Assert.That(text, Does.Contain("Nothing failed."));
                Assert.That(text, Does.Contain("1 of 4 files did not append"),
                    "the partial reason should still be on the group line");
            }
        }

        // An exception and a failed group are both errors and both get numbered.
        [Test]
        public void ExceptionsAndFailedGroupsAreCountedTogether()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07BC", GroupOutcome.Failed, 1.0, "nothing appended", null);
                log.Failure("saving the NWF", new InvalidOperationException("locked"), "carried on");

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("errors         : 2"));
                Assert.That(text, Does.Contain("[1] group 1C07BC ended FAILED"));
                Assert.That(text, Does.Contain("[2] saving the NWF"));
                Assert.That(text, Does.Contain("nothing appended"));
                Assert.That(text, Does.Contain("locked"));
            }
        }

        [Test]
        public void AnExceptionWithNoFailedGroupStillReportsAnError()
        {
            using (RunLog log = Start())
            {
                log.GroupFinished("1C07BC", GroupOutcome.Done, 1.0, null, null);
                log.Failure("copying the log", new IOException("no such drive"), "carried on");

                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups failed  : 0"));
                Assert.That(text, Does.Contain("errors         : 1"));
                Assert.That(text, Does.Not.Contain("Nothing failed."));
            }
        }

        [Test]
        public void ARunWithNoGroupsAtAllReportsZerosAndNothingFailed()
        {
            using (RunLog log = Start())
            {
                log.WriteResultBlock();
                string text = ReadWhileOpen(log);

                Assert.That(text, Does.Contain("groups done    : 0"));
                Assert.That(text, Does.Contain("groups partial : 0"));
                Assert.That(text, Does.Contain("groups failed  : 0"));
                Assert.That(text, Does.Contain("Nothing failed."));
            }
        }
    }
}
