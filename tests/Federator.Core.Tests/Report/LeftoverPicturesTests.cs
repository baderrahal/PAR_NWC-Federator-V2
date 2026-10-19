using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F177, Bader's order of 14 Oct 2026, round 4, task 3. A second run into the same Clash report folder
    /// overwrites the same named files and never dates a name there, removes the tool's own picture files
    /// the run did not write, keeps every other file, and every link still resolves. A failed run, or one
    /// that did not reach the end of its picture step, removes nothing. Since F243 the whole folder is deleted before
    /// a group's clash report, PicturesFolder, and the clash Excel sits beside its pictures under one name.
    /// </summary>
    [TestFixture]
    public class LeftoverPicturesTests
    {
        private const string Name = "1104-PAR-1A04PK-ZZZ-BM-MOD-000001";

        private string root;
        private string log;
        private string clashReport;
        private string pictures;

        [SetUp]
        public void MakeFolder()
        {
            root = TempFolder.Make("FederatorLeftoverPictures");
            log = Path.Combine(root, "NM fed", "Log");
            clashReport = ReportPaths.ClashReportFolder(log);
            Directory.CreateDirectory(log);
            pictures = ImageNaming.FolderFor(ReportPaths.Workbook(clashReport, Name));
            Directory.CreateDirectory(pictures);
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(root);
        }

        private static byte[] Jpg(byte mark)
        {
            return new byte[] { 0xFF, 0xD8, mark, 0xFF, 0xD9 };
        }

        private string Put(string name, byte mark)
        {
            string path = Path.Combine(pictures, name);
            File.WriteAllBytes(path, Jpg(mark));
            return path;
        }

        // ---------- the name ----------

        [Test]
        public void OnlyTheToolsOwnPictureNameIsOneOfItsPictures()
        {
            Assert.That(LeftoverPictures.IsOurPictureName("cd000001.jpg"), Is.True);
            Assert.That(LeftoverPictures.IsOurPictureName("cd1000001.jpg"), Is.True, "test 100 and over has a longer name");
            Assert.That(LeftoverPictures.IsOurPictureName("cd00001.jpg"), Is.False, "five digits is not the pattern");
            Assert.That(LeftoverPictures.IsOurPictureName("cd000001.png"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName("CD000001.jpg"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName("cd000001.JPG"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName("cd00000a.jpg"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName("xcd000001.jpg"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName("cd000001.jpg.bak"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName("logo.jpg"), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName(string.Empty), Is.False);
            Assert.That(LeftoverPictures.IsOurPictureName(null), Is.False);
        }

        // ---------- what is removed and what is kept ----------

        [Test]
        public void ALeftoverPictureOfAnEarlierRunIsRemovedAndNothingElseIs()
        {
            string written1 = Put("cd000001.jpg", 1);
            string written2 = Put("cd000002.jpg", 1);
            string leftover = Put("cd000003.jpg", 1);
            string leftoverLong = Put("cd1000001.jpg", 1);
            string logo = Put("logo.jpg", 1);
            string note = Path.Combine(pictures, "notes.txt");
            File.WriteAllText(note, "a person's note");
            string tooShort = Put("cd12.jpg", 1);
            string upper = Put("CD000007.JPG", 1);
            string otherJpg = Put("holiday.jpg", 1);

            // A sub folder, whose own cd file is not at the top level and is never looked at.
            string sub = Path.Combine(pictures, "cd000009.jpg.folder");
            Directory.CreateDirectory(sub);
            string inSub = Path.Combine(sub, "cd000008.jpg");
            File.WriteAllBytes(inSub, Jpg(1));

            LeftoverPicturesOutcome outcome = LeftoverPictures.Remove(
                pictures, new[] { "cd000001.jpg", "cd000002.jpg" }, true, null);

            Assert.That(outcome.NotDoneBecause, Is.Null);
            Assert.That(outcome.Removed, Is.EquivalentTo(new[] { "cd000003.jpg", "cd1000001.jpg" }));
            Assert.That(File.Exists(leftover), Is.False);
            Assert.That(File.Exists(leftoverLong), Is.False);

            foreach (string kept in new[] { written1, written2, logo, note, tooShort, otherJpg, inSub })
            {
                Assert.That(File.Exists(kept), Is.True, kept + " was removed");
            }

            // CD000007.JPG is a name this tool does not write, and a case insensitive disk may hold it only
            // where no lower case file shares the name, so it stays too.
            Assert.That(Directory.GetFiles(pictures).Select(Path.GetFileName), Does.Contain("CD000007.JPG"));
            Assert.That(upper, Is.Not.Null);
            Assert.That(Directory.Exists(pictures), Is.True);
            Assert.That(Directory.Exists(sub), Is.True);

            Assert.That(outcome.Line(), Does.StartWith("removed 2 leftover picture(s) from " + pictures));
            Assert.That(outcome.Line(), Does.Contain("kept the 2 it wrote"));
        }

        [Test]
        public void ARunThatDidNotReachTheEndOfItsPictureStepRemovesNothing()
        {
            string leftover = Put("cd000003.jpg", 1);

            LeftoverPicturesOutcome outcome = LeftoverPictures.Remove(pictures, new[] { "cd000001.jpg" }, false, null);

            Assert.That(File.Exists(leftover), Is.True);
            Assert.That(outcome.NotDoneBecause, Does.Contain("did not reach the end of its picture step"));
            Assert.That(outcome.Removed, Is.Empty);
            Assert.That(outcome.Line(), Does.StartWith("no leftover picture was removed"));
        }

        [Test]
        public void AFailedRunRemovesNothing()
        {
            string leftover = Put("cd000003.jpg", 1);

            LeftoverPicturesOutcome outcome = LeftoverPictures.Remove(
                pictures, new[] { "cd000001.jpg" }, true, "creating the clash tests threw NullReferenceException");

            Assert.That(File.Exists(leftover), Is.True, "a failed run deleted a picture");
            Assert.That(outcome.NotDoneBecause, Does.Contain("the run failed"));
            Assert.That(outcome.Removed, Is.Empty);
        }

        [Test]
        public void ARunThatWroteNoPictureRemovesNothingForAnEmptyListMayBeAFaultAndNotAFact()
        {
            string leftover = Put("cd000003.jpg", 1);

            LeftoverPicturesOutcome none = LeftoverPictures.Remove(pictures, new string[0], true, null);
            LeftoverPicturesOutcome unread = LeftoverPictures.Remove(pictures, null, true, null);

            Assert.That(File.Exists(leftover), Is.True);
            Assert.That(none.NotDoneBecause, Does.Contain("wrote no picture"));
            Assert.That(unread.NotDoneBecause, Does.Contain("wrote no picture"));
        }

        [Test]
        public void ItNeverWorksOutsideAFolderNamedForAReport()
        {
            // The Log folder, the Clash report folder and a folder that is not named _files all hold a
            // cd file, and none is touched.
            string inLog = Path.Combine(log, "cd000003.jpg");
            string inClashReport = Path.Combine(clashReport, "cd000003.jpg");
            string other = Path.Combine(root, "Other");
            Directory.CreateDirectory(other);
            string inOther = Path.Combine(other, "cd000003.jpg");

            foreach (string path in new[] { inLog, inClashReport, inOther })
            {
                File.WriteAllBytes(path, Jpg(1));
            }

            foreach (string folder in new[] { log, clashReport, other, root, string.Empty, null, "cd", Path.Combine(root, "_files") })
            {
                LeftoverPicturesOutcome outcome = LeftoverPictures.Remove(folder, new[] { "cd000001.jpg" }, true, null);

                Assert.That(outcome.NotDoneBecause, Is.Not.Null, "worked in " + folder);
            }

            foreach (string path in new[] { inLog, inClashReport, inOther })
            {
                Assert.That(File.Exists(path), Is.True, path + " was removed");
            }
        }

        [Test]
        public void AFolderThatIsNotThereIsSaidAndNothingIsMade()
        {
            string absent = Path.Combine(clashReport, "Other_files");

            LeftoverPicturesOutcome outcome = LeftoverPictures.Remove(absent, new[] { "cd000001.jpg" }, true, null);

            Assert.That(outcome.NotDoneBecause, Does.Contain("is not there"));
            Assert.That(Directory.Exists(absent), Is.False);
        }

        // ---------- a second run into the same Clash report folder ----------

        /// <summary>
        /// F243, Bader's order of 19 Oct 2026: the clash Excel sits in the Clash report folder beside its pictures, one name
        /// overwritten, and the group's pictures folder is deleted whole before its clash report is written. So a second run
        /// leaves exactly its own pictures, the Excel and its links under the one name, and another group's files untouched.
        /// Until then the second Excel took a dated name in the Log folder and the leftover jpgs were removed one by one.
        /// </summary>
        [Test]
        public void ASecondRunDeletesThePicturesFolderWritesOverTheExcelAndEveryLinkResolves()
        {
            TestPaths.OnWindowsOnly("the file:/// form of a drive letter path");

            string excel = ReportPaths.Workbook(clashReport, Name);

            // The first run, three pictures for three clashes, its Excel beside them and its XML in the Log folder.
            ClashReport first = Report(excel, 3);
            WritePictures(first, 1);
            new WorkbookWriter().Write(first, excel);
            string xml = ReportPaths.Xml(log, Name);
            new ClashReportXml().Write(first, xml);

            // A note a person put in the pictures folder, the logo, and another group's page and pictures.
            string note = Path.Combine(pictures, "reviewer notes.txt");
            File.WriteAllText(note, "a person's note");
            Put(ImageNaming.LogoName, 9);
            string peer = Path.Combine(clashReport, "OTHER-GROUP.html");
            File.WriteAllText(peer, "another group's page");
            string peerPictures = Path.Combine(clashReport, "OTHER-GROUP_files");
            Directory.CreateDirectory(peerPictures);
            string peerJpg = Path.Combine(peerPictures, "cd000001.jpg");
            File.WriteAllBytes(peerJpg, Jpg(7));

            // The second run: the folder goes first, then two pictures, then the Excel under the same name.
            PicturesFolder deleted = PicturesFolder.Delete(pictures);

            Assert.That(deleted.Deleted, Is.True);
            Assert.That(deleted.Pictures, Is.EqualTo(3));

            ClashReport second = Report(excel, 2);
            WritePictures(second, 2);
            WorkbookWriter writer = new WorkbookWriter();
            writer.Write(second, excel);

            Assert.That(Directory.GetFiles(pictures).Select(Path.GetFileName), Is.EquivalentTo(new[] { "cd000001.jpg", "cd000002.jpg" }),
                "the earlier run's picture, the logo and the note went with the folder");
            Assert.That(File.Exists(note), Is.False);
            Assert.That(File.ReadAllBytes(Path.Combine(pictures, "cd000001.jpg")), Is.EqualTo(Jpg(2)));
            Assert.That(Directory.GetDirectories(clashReport).Select(Path.GetFileName),
                Is.EquivalentTo(new[] { Name + "_files", "OTHER-GROUP_files" }));
            Assert.That(Directory.GetFiles(clashReport).Select(Path.GetFileName),
                Is.EquivalentTo(new[] { Name + ".xlsx", "OTHER-GROUP.html" }), "one Excel under the one name, no dated name");
            Assert.That(File.ReadAllBytes(peerJpg), Is.EqualTo(Jpg(7)), "another group's picture was touched");
            Assert.That(File.Exists(peer), Is.True);
            Assert.That(Directory.GetFiles(log, "*.xlsx"), Is.Empty, "no Excel in the Log folder");

            // Every link of the Excel resolves from the folder it sits in, the folder and the picture with nothing going up.
            WorkbookLinks links = WorkbookLinks.Check(excel, writer.PicturesLinked, writer.LinksWritten);

            Assert.That(links.Read, Is.EqualTo(4), "a click link and a cell link for each of the two pictures");
            Assert.That(links.MissingCount, Is.EqualTo(0));

            foreach (string link in ClickLinks(excel))
            {
                Assert.That(link, Does.StartWith(Name + "_files/cd"));
                Assert.That(link, Does.Not.Contain(".."));
            }
        }
        // ---------- helpers ----------

        private static void WritePictures(ClashReport report, byte mark)
        {
            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(row.ImagePath));
                    File.WriteAllBytes(row.ImagePath, Jpg(mark));
                }
            }
        }

        private static IList<string> ClickLinks(string workbook)
        {
            List<string> links = new List<string>();
            XNamespace rel = "http://schemas.openxmlformats.org/package/2006/relationships";

            using (System.IO.Compression.ZipArchive zip = System.IO.Compression.ZipFile.OpenRead(workbook))
            using (Stream stream = zip.GetEntry("xl/drawings/_rels/drawing1.xml.rels").Open())
            {
                foreach (XElement relationship in XDocument.Load(stream).Root.Elements(rel + "Relationship"))
                {
                    if ((string)relationship.Attribute("Type") != LinkedPictures.ImageRelationship)
                    {
                        links.Add((string)relationship.Attribute("Target"));
                    }
                }
            }

            return links;
        }

        private static ClashReport Report(string pageAnchor, int clashes)
        {
            const string treeRoot = "lcop_selection_set_tree";

            ClashReport report = new ClashReport("1A04PK", Path.GetFileNameWithoutExtension(pageAnchor));
            report.SetTreeRoot = treeRoot;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 14, 12, 0, 0);

            TestReport test = report.AddTest("BLD-AR-Walls-vs-BLD-ST-Columns");
            test.LeftLocator = treeRoot + "/Architecture/BLD-AR-Walls";
            test.RightLocator = treeRoot + "/Structure/BLD-ST-Columns";
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "New";
            test.State = TestState.FoundClashes;

            for (int c = 0; c < clashes; c++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + (c + 1);
                row.Status = ClashStatus.New;
                row.Distance = -0.05;
                row.ImageFile = ImageNaming.FileNameFor(0, c + 1);
                row.ImageLink = ImageNaming.LinkFor(pageAnchor, 0, c + 1);
                row.ImagePath = ImageNaming.PathFor(pageAnchor, 0, c + 1);
                test.Add(row);
            }

            return report;
        }
    }
}
