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
    /// F158. The logs and the reports moved out of the NWF folder into one Log folder beside the NWF
    /// and the NWD folders. The page's picture links are relative to the page and the workbook's linked
    /// pictures (F152) are absolute file:/// paths, so both are read back here, off the files the writers
    /// wrote after the move, and each is followed to a jpg that is on the disk inside the Log folder and
    /// in neither the NWF nor the NWD folder.
    /// </summary>
    [TestFixture]
    public class LogFolderLinksTests
    {
        private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
        private static readonly XNamespace PackageRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        private const string Install = @"C:\Program Files\Autodesk\Navisworks Manage 2025";

        private string root;

        [SetUp]
        public void MakeFolder()
        {
            root = TempFolder.Make("FederatorLogFolderLinks");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(root);
        }

        [Test]
        public void TheExcelsLinkedPicturesAndThePagesLinksResolveInsideTheLogFolderAfterTheMove()
        {
            TestPaths.OnWindowsOnly("the file:/// form of a drive letter path");

            string nwf = Path.Combine(root, "NM fed", "NWF", "C04");
            string nwd = Path.Combine(root, "NM fed", "NWD", "C04");
            string log = ReportPaths.LogFolder(nwf, nwd, Path.Combine(root, "NM fed", "NWC", "C04"));
            Directory.CreateDirectory(nwf);
            Directory.CreateDirectory(nwd);
            Directory.CreateDirectory(log);

            string workbook = ReportPaths.Workbook(log, "1104-PAR-1A04PK-ZZZ-BM-MOD-000001");
            ClashReport report = Report(workbook);

            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(row.ImagePath));
                    File.WriteAllBytes(row.ImagePath, new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 });
                }
            }

            new WorkbookWriter().Write(report, workbook);

            // The Excel, read as the zip it is. Each linked picture is an absolute path, decoded as
            // the target writes it, and it has to be a jpg that is there, in the Log folder.
            List<string> targets = new List<string>();

            using (ZipArchive zip = ZipFile.OpenRead(workbook))
            using (Stream stream = zip.GetEntry("xl/drawings/_rels/drawing1.xml.rels").Open())
            {
                foreach (XElement relationship in XDocument.Load(stream).Root.Elements(PackageRel + "Relationship"))
                {
                    targets.Add((string)relationship.Attribute("Target"));
                }
            }

            Assert.That(targets.Count, Is.EqualTo(3), "one linked picture for each clash row");

            string pictures = ImageNaming.FolderFor(workbook);

            foreach (string target in targets)
            {
                Assert.That(target, Does.StartWith("file:///"));

                string path = Uri.UnescapeDataString(target.Substring("file:///".Length));

                Assert.That(File.Exists(path), Is.True, "the Excel links to a picture that is not there: " + path);
                Assert.That(ReportPaths.IsInside(path, pictures), Is.True, "the picture is not in the _files folder: " + path);
                Assert.That(ReportPaths.IsInside(path, log), Is.True, "the picture is not in the Log folder: " + path);
                Assert.That(ReportPaths.IsInside(path, nwf), Is.False, "a picture is in the NWF folder: " + path);
                Assert.That(ReportPaths.IsInside(path, nwd), Is.False, "a picture is in the NWD folder: " + path);
            }

            // The page, whose picture links are relative to the page, which sits in the same folder as
            // the _files folder. The page is written here through the installed stylesheet, as the
            // run writes it, and the links it carries are read back off the page itself.
            string stylesheet = InstallFiles.FindStylesheet(Install, "en-US");

            if (stylesheet.Length == 0)
            {
                Assert.Ignore("Navisworks is not on this machine, so its stylesheet cannot be read.");
            }

            string page = HtmlTabularWriter.PathFor(workbook);
            string pageFolder = Path.GetDirectoryName(page);

            Assert.That(pageFolder, Is.EqualTo(log), "the page is not in the Log folder");
            Assert.That(Path.GetDirectoryName(pictures), Is.EqualTo(log), "the _files folder is not beside the page");

            new HtmlTabularWriter().Write(new ClashReportXml().Build(report), stylesheet, page);

            string html = File.ReadAllText(page);
            string[] links = System.Text.RegularExpressions.Regex.Matches(html, @"[^""'<>=\s]+_files\\cd\d+\.jpg")
                .Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value).Distinct().ToArray();

            Assert.That(links.Length, Is.EqualTo(3), "the page links each of the three pictures");

            foreach (string link in links)
            {
                Assert.That(link, Does.StartWith(Path.GetFileNameWithoutExtension(workbook) + "_files"),
                    "the link does not name the _files folder beside the page: " + link);
                Assert.That(File.Exists(Path.Combine(pageFolder, link)), Is.True,
                    "the page's link does not resolve from the page's folder: " + link);
            }

            // And the NWF and the NWD folders hold no log, report or picture of this run.
            Assert.That(Directory.GetFileSystemEntries(nwf), Is.Empty, "the NWF folder holds more than the NWF");
            Assert.That(Directory.GetFileSystemEntries(nwd), Is.Empty, "the NWD folder holds more than the NWD");
        }

        private static ClashReport Report(string workbookPath)
        {
            const string treeRoot = "lcop_selection_set_tree";

            ClashReport report = new ClashReport("1A04PK", Path.GetFileNameWithoutExtension(workbookPath));
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

            for (int c = 0; c < 3; c++)
            {
                ClashRow row = new ClashRow();
                row.Name = "Clash" + (c + 1);
                row.Status = ClashStatus.New;
                row.Distance = -0.05;
                row.ImageFile = ImageNaming.FileNameFor(0, c + 1);
                row.ImageLink = ImageNaming.LinkFor(workbookPath, 0, c + 1);
                row.ImagePath = ImageNaming.PathFor(workbookPath, 0, c + 1);
                test.Add(row);
            }

            return report;
        }
    }
}
