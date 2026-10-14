using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F152, Bader's decision of 2026-10-13, Q162-2: the workbook's pictures are linked pictures in the form
    /// of his by-hand client workbook. Every assertion reads the written xlsx as the zip it is, since the
    /// object model has called a broken file fine more than once, and the form is read off the client's own
    /// file rather than typed.
    /// </summary>
    [TestFixture]
    public class LinkedPicturesTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string ClientSample = "1104-PAR-1A02WN-XXX-BM-RPT-000001.xlsx";
        private static readonly XNamespace Xdr = "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
        private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
        private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRel = "http://schemas.openxmlformats.org/package/2006/relationships";
        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Types = "http://schemas.openxmlformats.org/package/2006/content-types";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorLinked");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        /// <summary>
        /// The count the brief asks for: one linked picture anchor for every clash row with an image, and
        /// every relationship target ending with the name of a jpg the run wrote. Three tests, five clash
        /// rows, three with a picture, and a test that found nothing, so a row with no picture and a block
        /// with none are both in it.
        /// </summary>
        [Test]
        public void OneLinkedAnchorPerClashRowWithAnImageAndEveryTargetAJpgTheRunWrote()
        {
            string path = Path.Combine(folder, "1104-PAR-1A02MM-ZZZ-BM-MOD-000001.xlsx");
            ClashReport report = Report(path, new[] { 3, 2, 0 }, new[] { true, false, true, true, false });

            new WorkbookWriter().Write(report, path);

            List<string> written = new List<string>();
            foreach (TestReport test in report.Tests)
            {
                foreach (ClashRow row in test.Rows)
                {
                    if (row.HasImage)
                    {
                        written.Add(row.ImageFile);
                    }
                }
            }

            Assert.That(written.Count, Is.EqualTo(3));

            XDocument drawing = Part(path, "xl/drawings/drawing1.xml");
            List<XElement> anchors = drawing.Root.Elements(Xdr + "twoCellAnchor").ToList();

            Assert.That(anchors.Count, Is.EqualTo(written.Count), "one anchor per clash row with an image");

            foreach (XElement anchor in anchors)
            {
                Assert.That((string)anchor.Attribute("editAs"), Is.EqualTo("oneCell"));
                XElement blip = anchor.Descendants(A + "blip").Single();
                Assert.That(blip.Attribute(R + "link"), Is.Not.Null, "a linked picture");
                Assert.That(blip.Attribute(R + "embed"), Is.Null, "never an embedded one");
            }

            List<XElement> relationships = Part(path, "xl/drawings/_rels/drawing1.xml.rels").Root
                .Elements(PackageRel + "Relationship")
                .Where(r => (string)r.Attribute("Type") == LinkedPictures.ImageRelationship).ToList();

            Assert.That(relationships.Count, Is.EqualTo(written.Count));

            List<string> named = new List<string>();

            foreach (XElement relationship in relationships)
            {
                string target = (string)relationship.Attribute("Target");

                Assert.That((string)relationship.Attribute("Type"), Is.EqualTo(LinkedPictures.ImageRelationship));
                Assert.That((string)relationship.Attribute("TargetMode"), Is.EqualTo("External"));
                Assert.That(target, Does.StartWith("file://"));
                named.Add(target.Substring(target.LastIndexOfAny(new[] { '\\', '/' }) + 1));
            }

            // The reviewer's point: each target its own jpg, the names as a set, so three targets of one jpg fail.
            named.Sort(StringComparer.Ordinal);
            written.Sort(StringComparer.Ordinal);
            Assert.That(named.Count, Is.EqualTo(written.Count));

            for (int i = 0; i < named.Count; i++)
            {
                Assert.That(named[i], Is.EqualTo(written[i]), "every target ends with a jpg the run wrote, each once");
            }

            Assert.That(Entries(path).Any(name => name.StartsWith("xl/media/", StringComparison.Ordinal)), Is.False,
                "no picture is embedded");
        }

        /// <summary>
        /// F169, Bader's answer A to Q163. Each picture carries a click link, a hlinkClick to the relative path of
        /// its jpg, as the client's own workbook does, forward slashed and with nothing after the file name. Every
        /// picture has one, each link is its own hyperlink relationship, and the linked picture itself is as it was.
        /// </summary>
        [Test]
        public void EveryPictureHasAClickLinkToItsJpgRelativeAndForwardSlashed()
        {
            string path = Path.Combine(folder, "1104-PAR-1A02MM-ZZZ-BM-MOD-000001.xlsx");
            ClashReport report = Report(path, new[] { 3, 2, 0 }, new[] { true, false, true, true, false });

            new WorkbookWriter().Write(report, path);

            XDocument drawing = Part(path, "xl/drawings/drawing1.xml");
            List<XElement> anchors = drawing.Root.Elements(Xdr + "twoCellAnchor").ToList();
            Dictionary<string, XElement> relationships = Part(path, "xl/drawings/_rels/drawing1.xml.rels").Root
                .Elements(PackageRel + "Relationship").ToDictionary(r => (string)r.Attribute("Id"));

            Assert.That(anchors.Count, Is.EqualTo(3));

            foreach (XElement anchor in anchors)
            {
                XElement click = anchor.Descendants(A + "hlinkClick").SingleOrDefault();

                Assert.That(click, Is.Not.Null, "a picture with no click link");

                XElement link = relationships[(string)click.Attribute(R + "id")];
                string target = (string)link.Attribute("Target");

                Assert.That((string)link.Attribute("Type"), Is.EqualTo(LinkedPictures.HyperlinkRelationship));
                Assert.That((string)link.Attribute("TargetMode"), Is.EqualTo("External"));
                Assert.That(target, Does.StartWith("1104-PAR-1A02MM-ZZZ-BM-MOD-000001_files/cd"));
                Assert.That(target, Does.EndWith(".jpg"), "nothing stray after the file name");
                Assert.That(target, Does.Not.Contain("\\"));

                XElement blip = anchor.Descendants(A + "blip").Single();
                Assert.That(blip.Attribute(R + "link"), Is.Not.Null, "the linked picture of F152 is kept");
                Assert.That(blip.Attribute(R + "embed"), Is.Null);
            }
        }

        [Test]
        public void TheClickLinkIsRelativeToTheFolderOfTheWorkbookAndGoesUpWhereTheJpgIsNotBelowIt()
        {
            string log = Path.Combine(Path.GetTempPath(), "NM fed", "Log");
            string workbook = Path.Combine(log, "X.xlsx");

            Assert.That(
                LinkedPictures.RelativeLinkFor(workbook, Path.Combine(log, "X_files", "cd000001.jpg")),
                Is.EqualTo("X_files/cd000001.jpg"));
            Assert.That(
                LinkedPictures.RelativeLinkFor(workbook, Path.Combine(Path.GetDirectoryName(log), "Clash report", "X_files", "cd000001.jpg")),
                Is.EqualTo("../Clash%20report/X_files/cd000001.jpg"));
            Assert.That(
                LinkedPictures.RelativeLinkFor(workbook, Path.Combine(log, "X 2026-10-14 153012_files", "cd000002.jpg")),
                Is.EqualTo("X%202026-10-14%20153012_files/cd000002.jpg"));
        }

        /// <summary>
        /// The parts the package needs besides the drawing: its content type, the sheet's relationship to it,
        /// and the drawing element in the sheet, after the page margins as the client's file has it. The
        /// break: a workbook with no picture holds none of them.
        /// </summary>
        [Test]
        public void TheDrawingIsTiedInByItsContentTypeTheSheetsRelationshipAndTheDrawingElement()
        {
            string path = Path.Combine(folder, "pictures.xlsx");
            new WorkbookWriter().Write(Report(path, new[] { 2 }, new[] { true, true }), path);

            Assert.That(Part(path, "[Content_Types].xml").Root.Elements(Types + "Override")
                .Count(o => (string)o.Attribute("PartName") == "/xl/drawings/drawing1.xml"
                    && (string)o.Attribute("ContentType") == "application/vnd.openxmlformats-officedocument.drawing+xml"),
                Is.EqualTo(1));

            XElement drawingRel = Part(path, "xl/worksheets/_rels/sheet1.xml.rels").Root
                .Elements(PackageRel + "Relationship")
                .Single(r => ((string)r.Attribute("Type")).EndsWith("/drawing", StringComparison.Ordinal));
            Assert.That((string)drawingRel.Attribute("Target"), Is.EqualTo("../drawings/drawing1.xml"));

            List<XElement> children = Part(path, "xl/worksheets/sheet1.xml").Root.Elements().ToList();
            int drawingAt = children.FindIndex(e => e.Name == Main + "drawing");
            int marginsAt = children.FindIndex(e => e.Name == Main + "pageMargins");

            Assert.That(drawingAt, Is.GreaterThan(marginsAt), "the drawing comes after the page margins");
            Assert.That((string)children[drawingAt].Attribute(R + "id"), Is.EqualTo((string)drawingRel.Attribute("Id")));

            string none = Path.Combine(folder, "none.xlsx");
            new WorkbookWriter().Write(Report(none, new[] { 2 }, new[] { false, false }), none);

            Assert.That(Entries(none).Any(name => name.StartsWith("xl/drawings/", StringComparison.Ordinal)), Is.False);
            Assert.That(Part(none, "xl/worksheets/sheet1.xml").Root.Element(Main + "drawing"), Is.Null);
        }

        /// <summary>
        /// The size and the anchor are the client's. Every picture of their drawing is 906780 EMU square in
        /// a twoCellAnchor of editAs oneCell starting at column A with no offset, read off their file, and a
        /// picture in a clash row of their 60 point height ends 144780 EMU into the row below, which is the
        /// end most of theirs carry. Ours is held to the same numbers.
        /// </summary>
        [Test]
        public void TheSizeAndTheAnchorAreReadOffTheClientsDrawing()
        {
            string sample = Path.Combine(Samples.Folder(), "client-report", ClientSample);

            if (!File.Exists(sample))
            {
                Assert.Ignore("The client's export is not in this checkout.");
            }

            List<XElement> theirs = Part(sample, "xl/drawings/drawing1.xml").Root.Elements(Xdr + "twoCellAnchor")
                .Where(a => (string)a.Descendants(Xdr + "cNvPr").Single().Attribute("descr") != "logo")
                .ToList();

            Assert.That(theirs.Count, Is.EqualTo(64), "the client's drawing as measured on 2026-10-13");

            foreach (XElement anchor in theirs)
            {
                Assert.That((string)anchor.Attribute("editAs"), Is.EqualTo("oneCell"));
                XElement ext = anchor.Descendants(A + "ext").First(e => e.Attribute("cx") != null);
                Assert.That((long)ext.Attribute("cx"), Is.EqualTo(LinkedPictures.PictureEmu));
                Assert.That((long)ext.Attribute("cy"), Is.EqualTo(LinkedPictures.PictureEmu));
                Assert.That((int)anchor.Element(Xdr + "from").Element(Xdr + "col"), Is.EqualTo(0));
                Assert.That(anchor.Descendants(A + "blip").Single().Attribute(R + "link"), Is.Not.Null);
            }

            Assert.That(theirs.Count(a => (long)a.Element(Xdr + "to").Element(Xdr + "rowOff") == 144780),
                Is.GreaterThan(theirs.Count / 3));

            foreach (XElement relationship in Part(sample, "xl/drawings/_rels/drawing1.xml.rels").Root
                .Elements(PackageRel + "Relationship")
                .Where(r => (string)r.Attribute("Type") == LinkedPictures.ImageRelationship))
            {
                Assert.That((string)relationship.Attribute("TargetMode"), Is.EqualTo("External"));
                Assert.That((string)relationship.Attribute("Target"), Does.StartWith("file:///"));
            }

            string path = Path.Combine(folder, "ours.xlsx");
            new WorkbookWriter().Write(Report(path, new[] { 2 }, new[] { true, true }), path);

            foreach (XElement anchor in Part(path, "xl/drawings/drawing1.xml").Root.Elements(Xdr + "twoCellAnchor"))
            {
                XElement from = anchor.Element(Xdr + "from");
                XElement to = anchor.Element(Xdr + "to");
                XElement ext = anchor.Descendants(A + "ext").Single();

                Assert.That((long)ext.Attribute("cx"), Is.EqualTo(906780));
                Assert.That((long)ext.Attribute("cy"), Is.EqualTo(906780));
                Assert.That((int)from.Element(Xdr + "col"), Is.EqualTo(0));
                Assert.That((long)from.Element(Xdr + "colOff"), Is.EqualTo(0));
                Assert.That((long)from.Element(Xdr + "rowOff"), Is.EqualTo(0));
                Assert.That((int)to.Element(Xdr + "col"), Is.EqualTo(0));
                Assert.That((long)to.Element(Xdr + "colOff"), Is.EqualTo(906780));
                Assert.That((int)to.Element(Xdr + "row"), Is.EqualTo((int)from.Element(Xdr + "row") + 1));
                Assert.That((long)to.Element(Xdr + "rowOff"), Is.EqualTo(144780));
            }
        }

        /// <summary>
        /// The anchor sits on the row of its clash. The pictured row's Image cell carries the link to the same
        /// jpg the anchor shows, so the cell hyperlink stays beside the picture, and ClosedXML still opens the
        /// file the coverage and the checks read.
        /// </summary>
        [Test]
        public void EachAnchorSitsOnItsClashRowAndTheCellKeepsItsLink()
        {
            string path = Path.Combine(folder, "rows.xlsx");
            ClashReport report = Report(path, new[] { 3 }, new[] { true, false, true });
            new WorkbookWriter().Write(report, path);

            XDocument drawing = Part(path, "xl/drawings/drawing1.xml");
            Dictionary<string, string> targets = Part(path, "xl/drawings/_rels/drawing1.xml.rels").Root
                .Elements(PackageRel + "Relationship")
                .ToDictionary(r => (string)r.Attribute("Id"), r => (string)r.Attribute("Target"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(1);

                foreach (XElement anchor in drawing.Root.Elements(Xdr + "twoCellAnchor"))
                {
                    int row = (int)anchor.Element(Xdr + "from").Element(Xdr + "row") + 1;
                    IXLCell cell = sheet.Cell(row, WorkbookWriter.ColumnImage);
                    string target = targets[(string)anchor.Descendants(A + "blip").Single().Attribute(R + "link")];

                    Assert.That(cell.HasHyperlink, Is.True, "row " + row + " keeps its cell link");
                    string link = cell.GetHyperlink().ExternalAddress.OriginalString;
                    Assert.That(target, Does.EndWith(Path.GetFileName(link)));
                }
            }
        }

        /// <summary>
        /// The target is the client's shape: file:/// and the absolute path with its backslashes kept and a
        /// space written %20. A per cent sign and a hash are escaped too, or they would be read as an escape
        /// and as the end of the path.
        /// </summary>
        [Test]
        public void TheTargetIsTheAbsolutePathInTheClientsShape()
        {
            TestPaths.OnWindowsOnly("a drive letter path");

            Assert.That(LinkedPictures.TargetFor(@"C:\00_NM\Clash report\x_files\cd160001.jpg"),
                Is.EqualTo(@"file:///C:\00_NM\Clash%20report\x_files\cd160001.jpg"));
            Assert.That(LinkedPictures.TargetFor(@"C:\a #1\50%\cd000001.jpg"),
                Is.EqualTo(@"file:///C:\a%20%231\50%25\cd000001.jpg"));
        }

        /// <summary>
        /// The drawing element goes before the first element the schema puts after it, the reviewer's point that
        /// nothing tested that branch. A sheet holding a table carries tableParts, which follows the drawing, so the
        /// drawing must land between the page margins and the table parts and never at the end.
        /// </summary>
        [Test]
        public void TheDrawingGoesBeforeATablePartAndAfterThePageMargins()
        {
            string path = Path.Combine(folder, "table.xlsx");

            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet sheet = workbook.Worksheets.Add("Sheet");
                sheet.Cell(1, 1).Value = "Name";
                sheet.Cell(2, 1).Value = "a";
                sheet.Range(1, 1, 2, 1).CreateTable();
                workbook.SaveAs(path);
            }

            LinkedPictures.AddTo(path, new List<LinkedPicture> { new LinkedPicture(2, Path.Combine(folder, "x_files", "cd000001.jpg")) });

            List<XElement> children = Part(path, "xl/worksheets/sheet1.xml").Root.Elements().ToList();
            int drawingAt = children.FindIndex(e => e.Name == Main + "drawing");
            int tableAt = children.FindIndex(e => e.Name == Main + "tableParts");

            Assert.That(tableAt, Is.GreaterThan(-1), "ClosedXML wrote the table parts this test needs");
            Assert.That(drawingAt, Is.GreaterThan(children.FindIndex(e => e.Name == Main + "pageMargins")));
            Assert.That(drawingAt, Is.LessThan(tableAt), "the drawing goes before the table parts");
        }

        /// <summary>
        /// The sheet's relationships keep distinct ids once the drawing's is added beside the cell links ClosedXML
        /// wrote, and a sheet that already holds a drawing is refused with the file left exactly as it was, since
        /// every part is made before the copy is written.
        /// </summary>
        [Test]
        public void TheIdsStayDistinctAndASecondDrawingIsRefusedWithTheFileUnchanged()
        {
            string path = Path.Combine(folder, "twice.xlsx");
            new WorkbookWriter().Write(Report(path, new[] { 3 }, new[] { true, true, true }), path);

            List<string> ids = Part(path, "xl/worksheets/_rels/sheet1.xml.rels").Root
                .Elements(PackageRel + "Relationship").Select(r => (string)r.Attribute("Id")).ToList();

            Assert.That(ids.Count, Is.GreaterThan(1), "the cell links and the drawing");
            Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(ids.Count));

            byte[] before = File.ReadAllBytes(path);

            Assert.Throws<InvalidDataException>(() =>
                LinkedPictures.AddTo(path, new List<LinkedPicture> { new LinkedPicture(6, Path.Combine(folder, "y.jpg")) }));
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(before), "the refused edit wrote nothing");
            Assert.That(File.Exists(path + ".linking"), Is.False, "the copy is taken out");
        }

        /// <summary>A writer used twice links the second workbook's pictures alone and says their count, not the sum.</summary>
        [Test]
        public void AWriterUsedTwiceCountsEachWorkbookOnItsOwn()
        {
            WorkbookWriter writer = new WorkbookWriter();
            string first = Path.Combine(folder, "first.xlsx");
            string second = Path.Combine(folder, "second.xlsx");

            writer.Write(Report(first, new[] { 3 }, new[] { true, true, true }), first);
            Assert.That(writer.PicturesLinked, Is.EqualTo(3));

            writer.Write(Report(second, new[] { 2 }, new[] { true, false }), second);
            Assert.That(writer.PicturesLinked, Is.EqualTo(1));
            Assert.That(Part(second, "xl/drawings/drawing1.xml").Root.Elements(Xdr + "twoCellAnchor").Count(), Is.EqualTo(1));
        }

        // ---------- helpers ----------

        /// <summary>
        /// A report with one test per entry of rowsPerTest, the rows of all of them numbered in turn, and each
        /// row's picture written or not by hasPicture in that order, its names made the way the run makes them.
        /// </summary>
        private static ClashReport Report(string workbookPath, int[] rowsPerTest, bool[] hasPicture)
        {
            ClashReport report = new ClashReport("1A02MM", Path.GetFileNameWithoutExtension(workbookPath));
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 13, 20, 33, 0);

            int n = 0;

            for (int t = 0; t < rowsPerTest.Length; t++)
            {
                TestReport test = report.AddTest("BLD-AR-Walls-vs-BLD-ST-" + t);
                test.LeftLocator = Root + "/Architecture/BLD-AR-Walls";
                test.RightLocator = Root + "/Structure/BLD-ST-" + t;
                test.Tolerance = 0.025;
                test.ToleranceUnits = "m";
                test.TestTypeName = "hard_conservative";
                test.StatusWord = "New";
                test.State = rowsPerTest[t] > 0 ? TestState.FoundClashes : TestState.Passed;

                for (int c = 0; c < rowsPerTest[t]; c++)
                {
                    ClashRow row = new ClashRow();
                    row.Name = "Clash" + (c + 1);
                    row.Status = ClashStatus.New;
                    row.Distance = -0.05;

                    if (hasPicture[n])
                    {
                        row.ImageFile = ImageNaming.FileNameFor(t, c + 1);
                        row.ImageLink = ImageNaming.LinkFor(workbookPath, t, c + 1);
                        row.ImagePath = ImageNaming.PathFor(workbookPath, t, c + 1);
                    }

                    test.Add(row);
                    n++;
                }
            }

            return report;
        }

        private static XDocument Part(string path, string part)
        {
            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                ZipArchiveEntry entry = zip.GetEntry(part);
                Assert.That(entry, Is.Not.Null, path + " holds no " + part);

                using (Stream stream = entry.Open())
                {
                    return XDocument.Load(stream);
                }
            }
        }

        private static List<string> Entries(string path)
        {
            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                return zip.Entries.Select(e => e.FullName).ToList();
            }
        }
    }
}
