using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The tool writes the client report, so the tool can check it and say. Bader has been
    /// opening every one in Excel and searching it by hand.
    ///
    /// Both checks read the FILE, never the object that produced it. That distinction has
    /// already caught a broken image link the object model called fine, and a test that was
    /// silently skipping instead of running.
    /// </summary>
    [TestFixture]
    public class ReportCheckTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Install = @"C:\Program Files\Autodesk\Navisworks Manage 2025";
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-RPT-000001";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorCheck", Guid.NewGuid().ToString("N"));
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

        // ---------- a real page, written by the real stylesheet ----------

        private static ClashReport Report(int clashes)
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.SetTreeRoot = Root;
            report.DocumentUnits = "ft";
            report.RunAt = new DateTime(2026, 9, 1, 20, 0, 0);

            TestReport test = report.AddTest("BLD-AR-Walls-vs-BLD-AR-Columns");
            test.LeftLocator = Root + "/Architecture/BLD-AR-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Columns";
            test.Tolerance = 0.2460629921;
            test.ToleranceUnits = "ft";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = TestState.FoundClashes;

            for (int i = 1; i <= clashes; i++)
            {
                test.Add(Row(i));
            }

            return report;
        }

        private static ClashRow Row(int number)
        {
            ClashRow row = new ClashRow();
            row.Name = "Clash" + number;
            row.Status = ClashStatus.New;
            row.Distance = -0.328084;
            row.GridLocation = "D-8 : LGF";
            row.Level = "LGF";
            row.Description = "Hard (Conservative)";
            row.Found = new DateTime(2026, 8, 31, 10, 15, 0);
            row.X = 33.17;
            row.Y = -10.41;
            row.Z = 0.328;
            row.ImageFile = ImageNaming.FileNameFor(0, number);
            row.ImageLink = OutputName + "_files/" + ImageNaming.FileNameFor(0, number);
            row.Left.ElementId = (702888 + number).ToString();
            row.Left.IdLabel = "Element ID";
            row.Left.Name = "PAR-CONC-FOUNDATION";
            row.Left.ItemType = "Solid";
            row.Left.Family = "Basic Wall";
            row.Left.Type = "HBLK 200";
            row.Left.Material = "Concrete";
            row.Left.SourceFile = @"C:\in\1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc";
            row.Left.Discipline = "AR";
            row.Right.ElementId = (807077 + number).ToString();
            row.Right.IdLabel = "Element ID";
            row.Right.Name = "Mx_Exterior";
            row.Right.ItemType = "Solid";
            row.Right.Family = "Structural Column";
            row.Right.Type = "UC 305";
            row.Right.Material = "Steel";
            row.Right.SourceFile = @"C:\in\1104-PAR-1C07BC-ZZZ-ST-MOD-000001.nwc";
            row.Right.Discipline = "ST";
            return row;
        }

        private static string Stylesheet()
        {
            string path = StylesheetLocator.Find(Install, "en-US");

            if (path.Length == 0)
            {
                Assert.Ignore("Navisworks is not on this machine, so its stylesheet cannot be read.");
            }

            return path;
        }

        /// <summary>Writes the page and the pictures it points at, and returns its path.</summary>
        private string WritePage(int clashes, bool withPictures, string logoHref)
        {
            string path = Path.Combine(folder, OutputName + ".html");

            ClashReportXml writer = new ClashReportXml();
            writer.LogoHref = logoHref;

            new HtmlTabularWriter().Write(writer.Build(Report(clashes)), Stylesheet(), path);

            if (withPictures)
            {
                string files = ImageNaming.FolderFor(path);
                Directory.CreateDirectory(files);

                for (int i = 1; i <= clashes; i++)
                {
                    File.WriteAllBytes(
                        Path.Combine(files, ImageNaming.FileNameFor(0, i)), new byte[8]);
                }
            }

            return path;
        }

        // ---------- it reads a written page and counts ----------

        // The one the brief asks for by name.
        [Test]
        public void ItReadsTheWrittenPageAndCountsCorrectly()
        {
            PageCheck check = PageCheck.Of(WritePage(5, true, string.Empty));

            Assert.That(check.Ran, Is.True, check.CouldNotRead);
            Assert.That(check.Rows, Is.EqualTo(5));
            Assert.That(check.ItemIdOnItem1, Is.EqualTo(5));
            Assert.That(check.ItemIdOnItem2, Is.EqualTo(5));
            Assert.That(check.FirstItemId, Is.EqualTo("Element ID: 702889"),
                "in full, so its shape is visible rather than described");
            Assert.That(check.ExtraColumns, Is.Empty);
            Assert.That(check.Tolerance, Is.EqualTo("0.246ft"));
            Assert.That(check.Pictures, Is.EqualTo(5));
            Assert.That(check.PicturesWithBackslash, Is.EqualTo(5));
            Assert.That(check.PicturesOnDisk, Is.EqualTo(5));
            Assert.That(check.LogoReferenced, Is.False);
            Assert.That(check.Passed, Is.True,
                string.Join(" ", new List<string>(check.Problems).ToArray()));
        }

        [Test]
        public void TheBlockSaysEveryNumberTheBriefAsksFor()
        {
            string block = string.Join("\n",
                new List<string>(PageCheck.Of(WritePage(5, true, string.Empty)).Lines()).ToArray());

            Assert.That(block, Does.Contain("5 clash rows"));
            Assert.That(block, Does.Contain("Item ID filled on 5 of 5"));
            Assert.That(block, Does.Contain("Element ID: 702889"));
            Assert.That(block, Does.Contain("no column the client's report does not have"));
            Assert.That(block, Does.Contain("the tolerance cell reads 0.246ft"));
            Assert.That(block, Does.Contain("5 picture references, 5 with a backslash, 5 on disk"));
            Assert.That(block, Does.Contain("no logo on the page"));
            Assert.That(block, Does.Contain("clash_report_html_tabular.xsl"),
                "it says where the client column set came from");
        }

        // ---------- an extra column is named ----------

        // The one the brief asks for by name.
        [Test]
        public void AColumnThatIsNotTheirsIsNamed()
        {
            string html = File.ReadAllText(WritePage(2, true, string.Empty));

            // One more heading in the item block, which is exactly what an extra smarttag
            // would produce.
            html = html.Replace(
                "<td class=\"item1Header\">Item Type</td>",
                "<td class=\"item1Header\">Item Type</td><td class=\"item1Header\">Source File</td>");

            PageCheck check = PageCheck.Of(html, folder, "in memory");

            Assert.That(check.ExtraColumns, Is.EqualTo(new[] { "Source File" }));
            Assert.That(check.Passed, Is.False);

            string said = string.Join(" ", new List<string>(check.Problems).ToArray());

            Assert.That(said, Does.Contain("Source File"));
            Assert.That(said, Does.Contain("the client's report does not have"));
            Assert.That(said, Does.Contain("Ours belong in the workbook"));
            Assert.That(check.Summary(), Does.Contain("Source File"));
        }

        // ---------- a missing Item ID is counted ----------

        // The one the brief asks for by name. This is what a run wrote into all 426 item
        // cells, and nothing said so until Bader opened the file.
        [Test]
        public void AMissingItemIdIsCounted()
        {
            string html = File.ReadAllText(WritePage(3, true, string.Empty));

            // One row's item 1 id emptied, the way an unread element id leaves it.
            int at = html.IndexOf("<td class=\"item1Content\">", StringComparison.Ordinal);
            int end = html.IndexOf("</td>", at, StringComparison.Ordinal);
            html = html.Substring(0, at) + "<td class=\"item1Content\">" + html.Substring(end);

            PageCheck check = PageCheck.Of(html, folder, "in memory");

            Assert.That(check.Rows, Is.EqualTo(3));
            Assert.That(check.ItemIdOnItem1, Is.EqualTo(2), "one of the three is empty");
            Assert.That(check.ItemIdOnItem2, Is.EqualTo(3));
            Assert.That(check.Passed, Is.False);
            Assert.That(string.Join(" ", new List<string>(check.Problems).ToArray()),
                Does.Contain("Item ID column is empty on some rows"));
            Assert.That(check.Summary(), Does.Contain("Item ID"));
        }

        [Test]
        public void EveryItemIdMissingIsCountedAsZero()
        {
            string html = File.ReadAllText(WritePage(2, true, string.Empty))
                .Replace("<i>Element ID</i>:", "<i></i>");

            PageCheck check = PageCheck.Of(html, folder, "in memory");

            Assert.That(check.ItemIdOnItem1, Is.EqualTo(0));
            Assert.That(check.FirstItemId, Is.Empty);
            Assert.That(string.Join(" ", new List<string>(check.Lines()).ToArray()),
                Does.Contain("no Item ID was found at all"));
        }

        // ---------- a picture that is not on disk is reported ----------

        // The one the brief asks for by name.
        [Test]
        public void APictureReferencePointingAtNothingIsReported()
        {
            string path = WritePage(4, true, string.Empty);

            // Two of the four taken away, the way a failed render leaves them.
            string files = ImageNaming.FolderFor(path);
            File.Delete(Path.Combine(files, ImageNaming.FileNameFor(0, 2)));
            File.Delete(Path.Combine(files, ImageNaming.FileNameFor(0, 3)));

            PageCheck check = PageCheck.Of(path);

            Assert.That(check.Pictures, Is.EqualTo(4));
            Assert.That(check.PicturesOnDisk, Is.EqualTo(2));
            Assert.That(check.MissingPictures.Count, Is.EqualTo(2));
            Assert.That(check.MissingPictures[0], Does.Contain("cd000002.jpg"));
            Assert.That(check.Passed, Is.False);

            string said = string.Join(" ", new List<string>(check.Problems).ToArray());

            Assert.That(said, Does.Contain("not on disk"));
            Assert.That(said, Does.Contain("2 of 4 are there"));
            Assert.That(said, Does.Contain("cd000002.jpg"));
        }

        [Test]
        public void AForwardSlashInAPictureReferenceIsReported()
        {
            string html = File.ReadAllText(WritePage(2, true, string.Empty))
                .Replace(OutputName + "_files\\cd000001.jpg", OutputName + "_files/cd000001.jpg");

            PageCheck check = PageCheck.Of(html, folder, "in memory");

            Assert.That(check.PicturesWithBackslash, Is.EqualTo(1));
            Assert.That(check.Pictures, Is.EqualTo(2));
            Assert.That(string.Join(" ", new List<string>(check.Problems).ToArray()),
                Does.Contain("forward slash"));
        }

        // ---------- the logo ----------

        [Test]
        public void ALogoOnTheePageIsCheckedAgainstTheDisk()
        {
            string path = WritePage(2, true, OutputName + "_files/logo.jpg");
            File.WriteAllBytes(
                Path.Combine(ImageNaming.FolderFor(path), "logo.jpg"), new byte[8]);

            PageCheck check = PageCheck.Of(path);

            Assert.That(check.LogoReferenced, Is.True);
            Assert.That(check.LogoOnDisk, Is.True);
            Assert.That(check.LogoReference, Is.EqualTo(OutputName + "_files\\logo.jpg"));
            Assert.That(check.Passed, Is.True);
        }

        [Test]
        public void ALogoThatIsNotOnDiskIsReported()
        {
            PageCheck check = PageCheck.Of(WritePage(2, true, OutputName + "_files/logo.jpg"));

            Assert.That(check.LogoReferenced, Is.True);
            Assert.That(check.LogoOnDisk, Is.False);
            Assert.That(string.Join(" ", new List<string>(check.Problems).ToArray()),
                Does.Contain("broken picture"));
        }

        // ---------- it never throws ----------

        [Test]
        public void APageThatIsNotThereSaysSoRatherThanThrowing()
        {
            PageCheck check = PageCheck.Of(Path.Combine(folder, "nothing.html"));

            Assert.That(check.Ran, Is.False);
            Assert.That(check.Passed, Is.False);
            Assert.That(check.CouldNotRead, Does.Contain("there is no file at"));
            Assert.That(check.Summary(), Does.Contain("not checked"));
            Assert.That(string.Join(" ", new List<string>(check.Lines()).ToArray()),
                Does.Contain("could not be checked"));
        }

        [Test]
        public void NonsenseInsteadOfAPageSaysSoRatherThanThrowing()
        {
            PageCheck check = PageCheck.Of("this is not a report at all", folder, "x");

            Assert.That(check.Rows, Is.EqualTo(0));
            Assert.That(string.Join(" ", new List<string>(check.Problems).ToArray()),
                Does.Contain("no clash table"));
        }

        // ---------- the workbook ----------

        private string WriteWorkbook(int clashes, bool unusedNowThatOursAreGone)
        {
            ClashReport report = Report(clashes);

            string path = Path.Combine(folder, OutputName + ".xlsx");
            return new WorkbookWriter(new ReportOptions()).Write(report, path);
        }

        // The workbook check used to count how many rows filled each of OUR columns. There
        // are none now, so it compares the workbook against the client's layout instead:
        // which columns, in what shape, in what order. See MatchOriginalTests for those.
        [Test]
        public void ItReadsTheWrittenWorkbookAndFindsTheClientLayout()
        {
            WorkbookCheck check = WorkbookCheck.Of(WriteWorkbook(6, true));

            Assert.That(check.Ran, Is.True, check.CouldNotRead);
            Assert.That(check.Sheets, Is.EqualTo(1));
            Assert.That(check.Blocks, Is.EqualTo(1));
            Assert.That(check.Rows, Is.EqualTo(6));
            Assert.That(check.Passed, Is.True,
                string.Join(" ", new List<string>(check.Problems).ToArray()));
        }

        [Test]
        public void AWorkbookThatIsNotThereSaysSoRatherThanThrowing()
        {
            WorkbookCheck check = WorkbookCheck.Of(Path.Combine(folder, "nothing.xlsx"));

            Assert.That(check.Ran, Is.False);
            Assert.That(check.CouldNotRead, Does.Contain("there is no file at"));
            Assert.That(check.Summary(), Does.Contain("not checked"));
        }

        [Test]
        public void SomethingThatIsNotAWorkbookSaysSoRatherThanThrowing()
        {
            string path = Path.Combine(folder, "notreally.xlsx");
            File.WriteAllText(path, "this is not a workbook");

            WorkbookCheck check = WorkbookCheck.Of(path);

            Assert.That(check.Ran, Is.False);
            Assert.That(check.CouldNotRead, Does.Contain("could not be opened"));
        }

        // ---------- what the window says ----------

        // The one the brief asks for by name, in the words it asks for.
        [Test]
        public void TheWindowLineReadsTheWayTheBriefAsks()
        {
            PageCheck check = PageCheck.Of(WritePage(213, true, string.Empty));

            Assert.That(check.Summary(), Is.EqualTo(
                "Client report: Item ID filled on 213 of 213 rows, no extra columns, "
                + "all 213 pictures on disk."));
        }

        [Test]
        public void AGroupWithNoClashesSaysThatRatherThanNothing()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.SetTreeRoot = Root;
            report.DocumentUnits = "ft";
            report.AddTest("nothing found").State = TestState.Passed;

            string path = Path.Combine(folder, OutputName + ".html");
            new HtmlTabularWriter().Write(
                new ClashReportXml().Build(report), Stylesheet(), path);

            PageCheck check = PageCheck.Of(path);

            Assert.That(check.Rows, Is.EqualTo(0));
            Assert.That(check.Summary(), Is.EqualTo("Client report written, no clashes on it."));
            Assert.That(check.Passed, Is.True, "no clashes is not a fault, but it said: "
                + string.Join(" ", new List<string>(check.Problems).ToArray()));
        }
    }
}
