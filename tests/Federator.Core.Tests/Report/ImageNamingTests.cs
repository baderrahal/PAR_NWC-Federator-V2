using System;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Where the pictures go and what they are called, which has to match the report the
    /// client already receives so the two travel together.
    ///
    /// Every expected name in here was read off two real exports on 2026-08-31, one with
    /// 60 pictures and one with 2672. The big one is why the numbering is not a running
    /// sequence: it reaches test 100 and the name grows to seven digits.
    /// </summary>
    [TestFixture]
    public class ImageNamingTests
    {
        private const string Workbook = @"C:\out\reports\1104-PAR-1A02WO-XXX-BM-RPT-000001.xlsx";
        private const string Folder = "1104-PAR-1A02WO-XXX-BM-RPT-000001_files";

        // ---------- the folder is named after the report ----------

        // The one the brief asks for by name.
        [Test]
        public void TheFolderIsTheReportNameWithFilesOnTheEnd()
        {
            Assert.That(ImageNaming.FolderNameFor(Workbook), Is.EqualTo(Folder));
            Assert.That(ImageNaming.FolderFor(Workbook),
                Is.EqualTo(Path.Combine(@"C:\out\reports", Folder)));
        }

        [Test]
        public void ItIsASiblingOfTheWorkbookAndNotInsideIt()
        {
            Assert.That(Path.GetDirectoryName(ImageNaming.FolderFor(Workbook)),
                Is.EqualTo(Path.GetDirectoryName(Workbook)));
        }

        [Test]
        public void TheExtensionOnTheWorkbookMakesNoDifference()
        {
            Assert.That(ImageNaming.FolderNameFor(@"C:\out\name.xlsx"), Is.EqualTo("name_files"));
            Assert.That(ImageNaming.FolderNameFor(@"C:\out\name"), Is.EqualTo("name_files"));
        }

        [Test]
        public void NothingAtAllIsNothingRatherThanAThrow()
        {
            Assert.That(ImageNaming.FolderFor(null), Is.EqualTo(string.Empty));
            Assert.That(ImageNaming.FolderFor(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(ImageNaming.FolderNameFor(null), Is.EqualTo(string.Empty));
        }

        // ---------- the numbering is theirs ----------

        // The one the brief asks for by name. These eight names are read off the supplied
        // report's own folder, which holds 64 pictures and the logo beside them. Counted on
        // 2026-09-12 on 1A02WN, and 1A04WN holds 65 and the same logo.
        [Test]
        public void TheNumberingMatchesTheAcceptedReportExactly()
        {
            Assert.That(ImageNaming.FileNameFor(0, 1), Is.EqualTo("cd000001.jpg"));
            Assert.That(ImageNaming.FileNameFor(0, 13), Is.EqualTo("cd000013.jpg"));
            Assert.That(ImageNaming.FileNameFor(1, 1), Is.EqualTo("cd010001.jpg"));
            Assert.That(ImageNaming.FileNameFor(1, 6), Is.EqualTo("cd010006.jpg"));
            Assert.That(ImageNaming.FileNameFor(2, 6), Is.EqualTo("cd020006.jpg"));
            Assert.That(ImageNaming.FileNameFor(9, 2), Is.EqualTo("cd090002.jpg"));
            Assert.That(ImageNaming.FileNameFor(19, 1), Is.EqualTo("cd190001.jpg"));
            Assert.That(ImageNaming.FileNameFor(20, 1), Is.EqualTo("cd200001.jpg"));
        }

        // The case a fixed width would have got wrong. The 2672 picture export holds
        // cd1000001.jpg and cd1010001.jpg, which are seven digits rather than six.
        [Test]
        public void PastTestNinetyNineTheNameGrowsRatherThanWrapping()
        {
            Assert.That(ImageNaming.FileNameFor(99, 1), Is.EqualTo("cd990001.jpg"));
            Assert.That(ImageNaming.FileNameFor(100, 1), Is.EqualTo("cd1000001.jpg"));
            Assert.That(ImageNaming.FileNameFor(101, 1), Is.EqualTo("cd1010001.jpg"));
        }

        // The same export has a test with 1244 clashes in it.
        [Test]
        public void FourDigitsIsTheFloorForTheClashAndNotTheCeiling()
        {
            Assert.That(ImageNaming.FileNameFor(0, 1244), Is.EqualTo("cd001244.jpg"));
            Assert.That(ImageNaming.FileNameFor(0, 9999), Is.EqualTo("cd009999.jpg"));
            Assert.That(ImageNaming.FileNameFor(0, 10000), Is.EqualTo("cd0010000.jpg"));
        }

        [Test]
        public void ATestIndexStartsAtZeroAndAClashIndexAtOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { ImageNaming.FileNameFor(-1, 1); });
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { ImageNaming.FileNameFor(0, 0); });
        }

        [Test]
        public void EveryNameReadsBackIntoTheTwoNumbersItWasBuiltFrom()
        {
            int[][] cases =
            {
                new[] { 0, 1 }, new[] { 0, 13 }, new[] { 20, 1 },
                new[] { 99, 1 }, new[] { 100, 1 }, new[] { 101, 1 }, new[] { 0, 1244 }
            };

            foreach (int[] one in cases)
            {
                int test;
                int clash;

                Assert.That(ImageNaming.TryRead(ImageNaming.FileNameFor(one[0], one[1]),
                    out test, out clash), Is.True);
                Assert.That(test, Is.EqualTo(one[0]));
                Assert.That(clash, Is.EqualTo(one[1]));
            }
        }

        [Test]
        public void TheLogoIsTheirsAndIsNotReadAsAPicture()
        {
            int test;
            int clash;

            Assert.That(ImageNaming.TryRead(ImageNaming.LogoName, out test, out clash), Is.False);
            Assert.That(ImageNaming.TryRead("something else.jpg", out test, out clash), Is.False);
            Assert.That(ImageNaming.TryRead(null, out test, out clash), Is.False);
        }

        // ---------- the path and the link ----------

        [Test]
        public void ThePathIsTheFolderAndTheName()
        {
            Assert.That(ImageNaming.PathFor(Workbook, 0, 1),
                Is.EqualTo(Path.Combine(@"C:\out\reports", Folder, "cd000001.jpg")));
        }

        // Relative and forward slashed, so moving the workbook and its folder together
        // keeps every link working, which is the whole point of writing them side by side.
        [Test]
        public void TheLinkIsRelativeSoThePairCanBeMoved()
        {
            string link = ImageNaming.LinkFor(Workbook, 0, 1);

            Assert.That(link, Is.EqualTo(Folder + "/cd000001.jpg"));
            Assert.That(link, Does.Not.Contain(":"), "an absolute link only works on one machine");
            Assert.That(link, Does.Not.Contain("\\"));
        }
    }
}
