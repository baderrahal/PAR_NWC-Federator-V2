using System;
using System.IO;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243, Bader's order of 19 Oct 2026 point 2. Before a group's clash report is written its whole NAME_files folder in
    /// the Clash report folder is deleted, and one line names the folder and its picture count. Nothing beside the folder is
    /// touched, and a folder another program holds a file of is said as open and never taken for deleted.
    /// </summary>
    [TestFixture]
    public class PicturesFolderTests
    {
        private const string Name = "1104-PAR-1B06BC-ZZZ-BM-MOD-000001";

        private string clashReport;
        private string pictures;

        [SetUp]
        public void MakeFolder()
        {
            clashReport = Path.Combine(TempFolder.Make("FederatorPicturesFolder"), ReportPaths.ClashReportSubfolder);
            pictures = ImageNaming.FolderFor(ReportPaths.Workbook(clashReport, Name));
            Directory.CreateDirectory(pictures);
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(Path.GetDirectoryName(clashReport));
        }

        private string Put(string folder, string name)
        {
            string path = Path.Combine(folder, name);
            File.WriteAllBytes(path, new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 });
            return path;
        }

        [Test]
        public void TheWholeFolderGoesAndItsClashPicturesAreCounted()
        {
            Put(pictures, "cd000001.jpg");
            Put(pictures, "cd000002.jpg");
            Put(pictures, "cd1000001.jpg");
            Put(pictures, ImageNaming.LogoName);
            File.WriteAllText(Path.Combine(pictures, "notes.txt"), "a person's note");
            string old = Path.Combine(pictures, "old");
            Directory.CreateDirectory(old);
            Put(old, "cd000009.jpg");

            PicturesFolder outcome = PicturesFolder.Delete(pictures);

            Assert.That(outcome.Deleted, Is.True);
            Assert.That(outcome.InUse, Is.False);
            Assert.That(outcome.Pictures, Is.EqualTo(3), "the logo, the note and the folder are not clash pictures of the folder");
            Assert.That(Directory.Exists(pictures), Is.False);
            Assert.That(outcome.Line(), Is.EqualTo("IMAGES   deleted " + pictures + " with its 3 pictures, logo.jpg and 2 other files and"
                + " folders, notes.txt, old, before this run's clash report of the group is written"));
        }

        /// <summary>F243 attempt 2, the breaker's W4: a folder of pictures alone says the pictures alone.</summary>
        [Test]
        public void AFolderOfPicturesAloneNamesNothingElse()
        {
            Put(pictures, "cd000001.jpg");

            Assert.That(PicturesFolder.Delete(pictures).Line(),
                Is.EqualTo("IMAGES   deleted " + pictures + " with its 1 picture, before this run's clash report of the group is written"));
        }

        /// <summary>
        /// F243 attempt 2, the breaker's S3 and W5. A page or a workbook this run did not write, left at its path after the
        /// folder went, is named, and the line says its links now reach this run's pictures.
        /// </summary>
        [Test]
        public void AnEarlierFileLeftAtItsPathIsNamedWithWhatItsLinksNowReach()
        {
            string page = Path.Combine(clashReport, Name + ".html");

            Assert.That(PicturesFolder.EarlierFileLine(page), Is.EqualTo("IMAGES   " + page + " is an earlier run's, left as it is since"
                + " this run did not write it, and its picture links now reach this run's pictures"));
        }

        /// <summary>The break: another group's folder and the page and the Excel beside the folder are left as they are.</summary>
        [Test]
        public void NothingBesideTheFolderIsTouched()
        {
            Put(pictures, "cd000001.jpg");
            string peer = Path.Combine(clashReport, "OTHER-GROUP_files");
            Directory.CreateDirectory(peer);
            string peerJpg = Put(peer, "cd000001.jpg");
            string page = Path.Combine(clashReport, Name + ".html");
            File.WriteAllText(page, "the page");
            string excel = ReportPaths.Workbook(clashReport, Name);
            File.WriteAllText(excel, "the workbook");

            PicturesFolder.Delete(pictures);

            Assert.That(File.Exists(peerJpg), Is.True);
            Assert.That(File.Exists(page), Is.True);
            Assert.That(File.Exists(excel), Is.True);
        }

        [Test]
        public void AFolderThatIsNotThereIsSaidAndNothingIsMade()
        {
            string absent = ImageNaming.FolderFor(ReportPaths.Workbook(clashReport, "OTHER"));

            PicturesFolder outcome = PicturesFolder.Delete(absent);

            Assert.That(outcome.Deleted, Is.False);
            Assert.That(outcome.Pictures, Is.EqualTo(0));
            Assert.That(Directory.Exists(absent), Is.False);
            Assert.That(outcome.Line(), Is.EqualTo("IMAGES   " + absent + " is not there, so no picture of an earlier run is deleted"));
        }

        [Test]
        public void OnlyAFolderOfPicturesIsEverHandedIn()
        {
            Assert.Throws<ArgumentException>(() => PicturesFolder.Delete(clashReport));
            Assert.Throws<ArgumentException>(() => PicturesFolder.Delete(null));
            Assert.That(Directory.Exists(clashReport), Is.True);
        }

        /// <summary>
        /// F243 attempt 2, the reviewer's N3 and the breaker's C3. The files go one by one, so the file held open is the one
        /// named, by its full path, for the line of FileInUse, and the pictures already deleted are counted.
        /// </summary>
        [Test]
        public void APictureHeldOpenIsNamedAndThePicturesAlreadyDeletedAreCounted()
        {
            TestPaths.OnWindowsOnly("a file held open refusing to be deleted");

            string free = Put(pictures, "cd000001.jpg");
            string held = Put(pictures, "cd000002.jpg");
            Put(pictures, "cd000003.jpg");
            PicturesFolder outcome;

            using (new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                outcome = PicturesFolder.Delete(pictures);
            }

            Assert.That(outcome.InUse, Is.True);
            Assert.That(outcome.Deleted, Is.False);
            Assert.That(outcome.Held.Count, Is.EqualTo(1));
            Assert.That(outcome.Held[0], Is.EqualTo(held));
            Assert.That(outcome.PicturesDeleted, Is.EqualTo(2));
            Assert.That(outcome.Line(), Is.EqualTo("IMAGES   " + pictures + " not deleted whole, 2 of its 3 pictures deleted and 1 file"
                + " held open by another program, named on the line after"));
            Assert.That(File.Exists(held), Is.True);
            Assert.That(File.Exists(free), Is.False);
        }
    }
}
