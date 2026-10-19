using System;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243, Bader's order of 19 Oct 2026 point 6. A file another program holds open is skipped, never written under another
    /// name, and named on one line, Close FULL PATH and run the report again, in the log and in the run window's end message.
    /// Open is read off the exception the write gave, the two Windows codes for a file held by another process, and nothing
    /// else is taken for it.
    /// </summary>
    [TestFixture]
    public class FileInUseTests
    {
        private static IOException WithCode(int code)
        {
            return new IOException("held", unchecked((int)0x80070000) | code);
        }

        [Test]
        public void ASharingOrALockViolationIsAFileInUse()
        {
            Assert.That(FileInUse.IsInUse(WithCode(FileInUse.SharingViolation)), Is.True);
            Assert.That(FileInUse.IsInUse(WithCode(FileInUse.LockViolation)), Is.True);
        }

        /// <summary>The break: every other failure of a write stays a failure of its own and is never called open.</summary>
        [Test]
        public void AnyOtherFailureIsNotAFileInUse()
        {
            Assert.That(FileInUse.IsInUse(WithCode(5)), Is.False, "access denied");
            Assert.That(FileInUse.IsInUse(WithCode(112)), Is.False, "a full disk");
            Assert.That(FileInUse.IsInUse(new FileNotFoundException("gone")), Is.False);
            Assert.That(FileInUse.IsInUse(new UnauthorizedAccessException("read only")), Is.False);
            Assert.That(FileInUse.IsInUse(new InvalidOperationException("other", new ArgumentException("inner"))), Is.False);
            Assert.That(FileInUse.IsInUse(null), Is.False);
        }

        [Test]
        public void ASharingViolationWrappedByALibraryIsStillFound()
        {
            Assert.That(FileInUse.IsInUse(new InvalidOperationException("saving", WithCode(FileInUse.SharingViolation))), Is.True);
        }

        /// <summary>
        /// What Windows gives a write over a file another handle holds with no sharing, which is what Excel does to its xlsx.
        /// A rule of the file system, so it runs where Windows is.
        /// </summary>
        [Test]
        public void AWriteOverAFileHeldOpenIsReadAsInUse()
        {
            TestPaths.OnWindowsOnly("a file held open refusing to be written");

            string folder = TempFolder.Make("FederatorFileInUse");

            try
            {
                string path = Path.Combine(folder, "C06 clash count check.xlsx");
                File.WriteAllText(path, "last run");
                Exception caught = null;

                using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    try
                    {
                        File.WriteAllText(path, "this run");
                    }
                    catch (Exception error)
                    {
                        caught = error;
                    }
                }

                Assert.That(caught, Is.Not.Null, "the write over a file held open did not fail");
                Assert.That(FileInUse.IsInUse(caught), Is.True, caught.GetType().Name + " " + caught.HResult);
                Assert.That(File.ReadAllText(path), Is.EqualTo("last run"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        [Test]
        public void TheLineIsBadersWordsWithTheFullPath()
        {
            string path = TestPaths.At("Report", "C06", "Clash report", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.xlsx");

            Assert.That(FileInUse.Line(path), Is.EqualTo("Close " + path + " and run the report again"));
        }

        /// <summary>
        /// The line a group's own Navisworks wrote into its RESULT is found again behind the log's time stamp, and a line
        /// that only looks like it is not.
        /// </summary>
        [Test]
        public void TheLineIsFoundAgainBehindTheLogsStampAndNothingElseIs()
        {
            string path = TestPaths.At("Report", "C06", "Log", "1104-PAR-1B06BC-ZZZ-BM-MOD-000001.xml");
            string line = FileInUse.Line(path);

            Assert.That(FileInUse.LineIn("06:21:33  +0012.345s  " + line), Is.EqualTo(line));
            Assert.That(FileInUse.LineIn(line), Is.EqualTo(line));
            Assert.That(FileInUse.LineIn("06:21:33  +0012.345s  groups done    : 1"), Is.Null);
            Assert.That(FileInUse.LineIn("Close  and run the report again"), Is.Null, "a line naming no path");
            Assert.That(FileInUse.LineIn("Close the window and run the report again later"), Is.Null);
            Assert.That(FileInUse.LineIn(null), Is.Null);
        }

        [Test]
        public void TheEndMessageAddsEachFileOnceUnderWhatItAlreadySays()
        {
            string one = FileInUse.Line(TestPaths.At("a.xlsx"));
            string two = FileInUse.Line(TestPaths.At("b.html"));
            string message = FileInUse.EndMessage("1 of 2 groups wrote no result.", new[] { one, two, one });
            string[] lines = message.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.That(lines.Length, Is.EqualTo(5));
            Assert.That(lines[0], Is.EqualTo("1 of 2 groups wrote no result."));
            Assert.That(lines[1], Is.Empty);
            Assert.That(lines[2], Is.EqualTo("2 files were open in another program and left as they were:"));
            Assert.That(lines[3], Is.EqualTo(one));
            Assert.That(lines[4], Is.EqualTo(two));
        }

        [Test]
        public void WithNoFileOpenTheEndMessageIsWhatItAlreadySaidOrNothing()
        {
            Assert.That(FileInUse.EndMessage("it ran", new string[0]), Is.EqualTo("it ran"));
            Assert.That(FileInUse.EndMessage(null, null), Is.Null);
            Assert.That(FileInUse.EndMessage(null, new[] { FileInUse.Line(TestPaths.At("a.xlsx")) }),
                Is.EqualTo("A file was open in another program and left as it was:" + Environment.NewLine + FileInUse.Line(TestPaths.At("a.xlsx"))));
        }
    }
}
