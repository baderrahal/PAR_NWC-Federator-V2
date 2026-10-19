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
    /// F158, F170 and F243. The Log folder beside the NWF and the NWD folders holds the text type files. The
    /// Clash report folder beside the Log folder holds the Excel and the HTML and their _files folder of pictures,
    /// Bader's order of 19 Oct 2026. The page's picture links are relative to the page, the workbook's linked
    /// pictures (F152) are absolute file:/// paths, and its click links (F169) and cell links are relative to the
    /// Excel, the folder and the picture with nothing going up. All of them are read back here, off the files the writers wrote, and each is followed to a
    /// jpg that is on the disk in the Clash report folder and in none of the NWF, NWD or Log folders.
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
        public void TheExcelsLinkedPicturesClickLinksAndThePagesLinksAllResolveToJpgsInTheClashReportFolder()
        {
            TestPaths.OnWindowsOnly("the file:/// form of a drive letter path");

            string nwf = Path.Combine(root, "NM fed", "NWF", "C04");
            string nwd = Path.Combine(root, "NM fed", "NWD", "C04");
            string log = ReportPaths.LogFolder(nwf, nwd, Path.Combine(root, "NM fed", "NWC", "C04"));
            Directory.CreateDirectory(nwf);
            Directory.CreateDirectory(nwd);
            Directory.CreateDirectory(log);

            string clashReport = ReportPaths.ClashReportFolder(log);

            Assert.That(clashReport, Is.EqualTo(Path.Combine(root, "NM fed", "Clash report")));

            string workbook = ReportPaths.Workbook(clashReport, "1104-PAR-1A04PK-ZZZ-BM-MOD-000001");
            string pageAnchor = workbook;
            ClashReport report = Report(pageAnchor);

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
            // the target writes it, and it has to be a jpg that is there, beside the Excel in the Clash report folder.
            List<string> targets = new List<string>();
            List<string> clickLinks = new List<string>();
            List<string> cellLinks = new List<string>();

            using (ZipArchive zip = ZipFile.OpenRead(workbook))
            using (Stream stream = zip.GetEntry("xl/drawings/_rels/drawing1.xml.rels").Open())
            {
                foreach (XElement relationship in XDocument.Load(stream).Root.Elements(PackageRel + "Relationship"))
                {
                    if ((string)relationship.Attribute("Type") == LinkedPictures.ImageRelationship)
                    {
                        targets.Add((string)relationship.Attribute("Target"));
                    }
                    else
                    {
                        clickLinks.Add((string)relationship.Attribute("Target"));
                    }
                }
            }

            using (ZipArchive zip = ZipFile.OpenRead(workbook))
            using (Stream stream = zip.GetEntry("xl/worksheets/_rels/sheet1.xml.rels").Open())
            {
                foreach (XElement relationship in XDocument.Load(stream).Root.Elements(PackageRel + "Relationship"))
                {
                    if ((string)relationship.Attribute("TargetMode") == "External")
                    {
                        cellLinks.Add((string)relationship.Attribute("Target"));
                    }
                }
            }

            Assert.That(targets.Count, Is.EqualTo(3), "one linked picture for each clash row");

            string pictures = ImageNaming.FolderFor(pageAnchor);

            foreach (string target in targets)
            {
                Assert.That(target, Does.StartWith("file:///"));

                string path = Uri.UnescapeDataString(target.Substring("file:///".Length));

                Assert.That(File.Exists(path), Is.True, "the Excel links to a picture that is not there: " + path);
                Assert.That(ReportPaths.IsInside(path, pictures), Is.True, "the picture is not in the _files folder: " + path);
                Assert.That(ReportPaths.IsInside(path, clashReport), Is.True, "the picture is not in the Clash report folder: " + path);
                Assert.That(ReportPaths.IsInside(path, log), Is.False, "a picture is in the Log folder: " + path);
                Assert.That(ReportPaths.IsInside(path, nwf), Is.False, "a picture is in the NWF folder: " + path);
                Assert.That(ReportPaths.IsInside(path, nwd), Is.False, "a picture is in the NWD folder: " + path);
            }

            // F169. The click link of each picture is relative to the Excel and has to reach a jpg that is
            // there, read the way a person's click reads it, from the folder the Excel sits in. F243: the Excel sits
            // beside its pictures, so the link is the folder and the picture with nothing going up.
            Assert.That(clickLinks.Count, Is.EqualTo(3), "a click link for each picture");

            foreach (string click in clickLinks)
            {
                string resolved = Path.GetFullPath(Path.Combine(clashReport, Uri.UnescapeDataString(click).Replace('/', Path.DirectorySeparatorChar)));

                Assert.That(File.Exists(resolved), Is.True, "the click link reaches no file: " + click + " from " + clashReport);
                Assert.That(click, Does.StartWith("1104-PAR-1A04PK-ZZZ-BM-MOD-000001_files/cd"), "the click link is not the folder and the picture");
                Assert.That(click, Does.Not.Contain(".."), "the click link goes up a folder");
                Assert.That(ReportPaths.IsInside(resolved, clashReport), Is.True);
            }

            // The link of the cell itself, the one a person clicks in the Image column.
            Assert.That(cellLinks.Count, Is.EqualTo(3), "a cell link for each picture");

            foreach (string cell in cellLinks)
            {
                string resolved = Path.GetFullPath(Path.Combine(clashReport, Uri.UnescapeDataString(cell).Replace('/', Path.DirectorySeparatorChar)));

                Assert.That(File.Exists(resolved), Is.True, "the cell link reaches no file: " + cell + " from " + clashReport);
                Assert.That(cell, Does.Not.Contain(".."), "the cell link goes up a folder");
                Assert.That(ReportPaths.IsInside(resolved, clashReport), Is.True);
            }

            // The XML sits in the Log folder too, and its hrefs are relative to it.
            string xml = ReportPaths.Xml(log, "1104-PAR-1A04PK-ZZZ-BM-MOD-000001");
            new ClashReportXml().Write(report, xml);

            foreach (XAttribute href in XDocument.Load(xml).Descendants().Attributes("href"))
            {
                string resolved = Path.GetFullPath(Path.Combine(log, href.Value));

                Assert.That(File.Exists(resolved), Is.True, "the XML href reaches no file: " + href.Value);
            }

            // The page, whose picture links are relative to the page, which sits in the same folder as
            // the _files folder. The page is written here through the installed stylesheet, as the
            // run writes it, and the links it carries are read back off the page itself.
            string stylesheet = InstallFiles.FindStylesheet(Install, "en-US");

            if (stylesheet.Length == 0)
            {
                Assert.Ignore("Navisworks is not on this machine, so its stylesheet cannot be read.");
            }

            string page = HtmlTabularWriter.PathFor(pageAnchor);
            string pageFolder = Path.GetDirectoryName(page);

            Assert.That(pageFolder, Is.EqualTo(clashReport), "the page is not in the Clash report folder");
            Assert.That(Path.GetDirectoryName(pictures), Is.EqualTo(clashReport), "the _files folder is not beside the page");

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

            // The Log folder holds no page, no Excel and no pictures folder, and nothing is in the NWF or the NWD folder.
            Assert.That(Directory.GetFiles(log, "*.html"), Is.Empty);
            Assert.That(Directory.GetFiles(log, "*.xlsx"), Is.Empty, "the clash Excel is in the Log folder");
            Assert.That(Directory.GetDirectories(log), Is.Empty, "the Log folder holds a folder of pictures");
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
