using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F243, Bader's order of 19 Oct 2026 point 3. The clash Excel sits beside its pictures in the Clash report folder, so every
    /// picture link in it is NAME_files/picture with nothing going up a folder. After it is written every hyperlink is read
    /// back off the file and looked for on the disk, the count checked and the count missing said, and the first missing named.
    /// </summary>
    [TestFixture]
    public class WorkbookLinksTests
    {
        private const string Name = "1104-PAR-1B06BC-ZZZ-BM-MOD-000001";

        private static readonly XNamespace PackageRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        private string root;
        private string clashReport;
        private string workbook;

        [SetUp]
        public void MakeFolder()
        {
            root = TempFolder.Make("FederatorWorkbookLinks");
            clashReport = ReportPaths.ClashReportFolder(Path.Combine(root, "C06", ReportPaths.DefaultSubfolder));
            Directory.CreateDirectory(clashReport);
            workbook = ReportPaths.Workbook(clashReport, Name);
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(root);
        }

        [Test]
        public void EveryLinkOfAnExcelBesideItsPicturesIsItsFolderAndThePictureWithNothingGoingUp()
        {
            ClashReport report = Report(3);
            WriteThePictures(report);
            new WorkbookWriter().Write(report, workbook);

            List<string> targets = HyperlinkTargets(workbook);

            Assert.That(targets.Count, Is.EqualTo(6), "a cell link and a click link for each of the three pictures");

            foreach (string target in targets)
            {
                Assert.That(target, Does.StartWith(Name + "_files/cd"), target);
                Assert.That(target, Does.Not.Contain(".."), target);
                Assert.That(target, Does.Not.Contain("\\"), target);
            }
        }

        [Test]
        public void EveryPictureOnTheDiskIsCheckedAndNoneIsMissing()
        {
            ClashReport report = Report(3);
            WriteThePictures(report);
            WorkbookWriter writer = new WorkbookWriter();
            writer.Write(report, workbook);

            WorkbookLinks links = WorkbookLinks.Check(workbook, writer.PicturesLinked, writer.LinksWritten);

            Assert.That(links.NotRead, Is.Null);
            Assert.That(links.Read, Is.EqualTo(6));
            Assert.That(links.MissingCount, Is.EqualTo(0));
            Assert.That(links.Line(), Is.EqualTo("XLSX     links    3 pictures linked, 6 links expected, 6 read, 0 missing"));
            Assert.That(links.Error(), Is.Null);
        }

        /// <summary>
        /// F243 attempt 3, the breaker's 4. The writer says when its save is on the disk, before it links the pictures, so a
        /// file held during the linking is this run's save and never called an earlier run's.
        /// </summary>
        [Test]
        public void TheWriterSaysWhenItsSaveIsOnTheDisk()
        {
            WorkbookWriter writer = new WorkbookWriter();

            Assert.That(writer.Saved, Is.False);

            ClashReport report = Report(1);
            WriteThePictures(report);
            writer.Write(report, workbook);

            Assert.That(writer.Saved, Is.True);
        }

        /// <summary>F243 attempt 2, the breaker's S5. The writer counts each link it made, the cell link and the click link of a picture.</summary>
        [Test]
        public void TheWriterCountsEveryLinkItMade()
        {
            ClashReport report = Report(3);
            report.Tests[0].Rows[2].ImageLink = string.Empty;
            WriteThePictures(report);
            WorkbookWriter writer = new WorkbookWriter();
            writer.Write(report, workbook);

            Assert.That(writer.PicturesLinked, Is.EqualTo(3));
            Assert.That(writer.LinksWritten, Is.EqualTo(5), "three click links and two cell links, the third row carrying no cell link");
            Assert.That(WorkbookLinks.Check(workbook, writer.PicturesLinked, writer.LinksWritten).Line(),
                Is.EqualTo("XLSX     links    3 pictures linked, 5 links expected, 5 read, 0 missing"));
        }

        /// <summary>The break: one picture taken away after the write is counted missing, named, and is the group's error.</summary>
        [Test]
        public void APictureNotOnTheDiskIsCountedNamedAndIsAnError()
        {
            ClashReport report = Report(3);
            WriteThePictures(report);
            WorkbookWriter writer = new WorkbookWriter();
            writer.Write(report, workbook);

            string gone = report.Tests[0].Rows[1].ImagePath;
            File.Delete(gone);

            WorkbookLinks links = WorkbookLinks.Check(workbook, writer.PicturesLinked, writer.LinksWritten);

            Assert.That(links.Read, Is.EqualTo(6));
            Assert.That(links.MissingCount, Is.EqualTo(2), "its cell link and its click link");
            Assert.That(links.Missing[0], Is.EqualTo(Path.GetFullPath(gone)));
            Assert.That(links.Line(), Is.EqualTo("XLSX     links    3 pictures linked, 6 links expected, 6 read, 2 missing, the first "
                + Path.GetFullPath(gone)));
            Assert.That(links.Error(), Does.Contain("2 of the 6 links"));
        }

        /// <summary>
        /// F243 attempt 2, the breaker's S5. A link the writer made that the workbook read back does not hold is missing too, so
        /// a workbook whose links went is never read as clean.
        /// </summary>
        [Test]
        public void ALinkTheWriterMadeThatTheWorkbookDoesNotHoldIsMissing()
        {
            ClashReport report = Report(3);
            WriteThePictures(report);
            new WorkbookWriter().Write(report, workbook);

            WorkbookLinks links = WorkbookLinks.Check(workbook, 4, 8);

            Assert.That(links.MissingCount, Is.EqualTo(2));
            Assert.That(links.Missing.Count, Is.EqualTo(0), "no link read reaches no file");
            Assert.That(links.Line(), Is.EqualTo("XLSX     links    4 pictures linked, 8 links expected, 6 read, 2 missing, 2 links expected and not in the workbook"));
            Assert.That(links.Error(), Does.Contain("2 of the 8 links"));
        }

        [Test]
        public void AWorkbookWithNoPictureChecksNothingAndMissesNothing()
        {
            ClashReport report = Report(2);

            foreach (ClashRow row in report.Tests[0].Rows)
            {
                row.ImageFile = string.Empty;
                row.ImageLink = string.Empty;
                row.ImagePath = string.Empty;
            }

            WorkbookWriter writer = new WorkbookWriter();
            writer.Write(report, workbook);

            WorkbookLinks links = WorkbookLinks.Check(workbook, writer.PicturesLinked, writer.LinksWritten);

            Assert.That(writer.LinksWritten, Is.EqualTo(0));
            Assert.That(links.Read, Is.EqualTo(0));
            Assert.That(links.MissingCount, Is.EqualTo(0));
            Assert.That(links.NotRead, Is.Null);
        }

        [Test]
        public void AWorkbookThatWillNotReadIsSaidAndNeverCountedClean()
        {
            File.WriteAllText(workbook, "not a zip");

            WorkbookLinks links = WorkbookLinks.Check(workbook, 1, 2);

            Assert.That(links.NotRead, Is.Not.Null);
            Assert.That(links.Line(), Does.Contain("UNKNOWN"));
            Assert.That(links.Line(), Does.Not.Contain("0 missing"));
            Assert.That(links.Error(), Is.Null);
        }

        private ClashReport Report(int clashes)
        {
            ClashReport report = new ClashReport("1B06BC", Name);
            report.SetTreeRoot = "lcop_selection_set_tree";
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 19, 20, 58, 0);

            TestReport test = report.AddTest("BLD-AR-Walls-vs-BLD-ST-Framing");
            test.LeftLocator = "lcop_selection_set_tree/Architecture/BLD-AR-Walls";
            test.RightLocator = "lcop_selection_set_tree/Structure/BLD-ST-Framing";
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
                row.ImageLink = ImageNaming.LinkFor(workbook, 0, c + 1);
                row.ImagePath = ImageNaming.PathFor(workbook, 0, c + 1);
                test.Add(row);
            }

            return report;
        }

        private static void WriteThePictures(ClashReport report)
        {
            foreach (ClashRow row in report.Tests.SelectMany(t => t.Rows))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(row.ImagePath));
                File.WriteAllBytes(row.ImagePath, new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 });
            }
        }

        private static List<string> HyperlinkTargets(string path)
        {
            List<string> targets = new List<string>();

            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in zip.Entries.Where(e => e.FullName.EndsWith(".rels", StringComparison.Ordinal)))
                {
                    using (Stream stream = entry.Open())
                    {
                        foreach (XElement relationship in XDocument.Load(stream).Root.Elements(PackageRel + "Relationship"))
                        {
                            if ((string)relationship.Attribute("Type") == LinkedPictures.HyperlinkRelationship)
                            {
                                targets.Add((string)relationship.Attribute("Target"));
                            }
                        }
                    }
                }
            }

            return targets;
        }
    }
}
