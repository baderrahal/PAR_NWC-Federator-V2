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
    /// The pictures are numbered in the export order, which is the order the rows are
    /// written, and not in the order the tests ran. Measured off both client exports in
    /// samples\client-report, where the first block is cd00 and the second cd01 whatever
    /// order the tests ran in.
    ///
    /// The samples here are built in a MIXED order on purpose: the test with the fewest
    /// clashes runs first, so the run order and the report order disagree on every test.
    /// A sample where they agree would pass against the old numbering too.
    /// </summary>
    [TestFixture]
    public class ReportOrderTests
    {
        private const string Root = "lcop_selection_set_tree";
        private string folder;
        private string workbook;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorReportOrder", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            workbook = Path.Combine(folder, "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.xlsx");
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

        // ---------- the sample ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            return report;
        }

        private static TestReport Found(ClashReport report, string name, params string[] clashes)
        {
            TestReport test = report.AddTest(name);
            test.LeftLocator = Root + "/Architecture/BLD-AR-" + name;
            test.RightLocator = Root + "/Mechanical/BLD-ME-Ducts";
            test.TestTypeName = "hard_conservative";
            test.Tolerance = 0.075;
            test.ToleranceUnits = "m";
            test.State = TestState.FoundClashes;

            foreach (string clash in clashes)
            {
                ClashRow row = new ClashRow();
                row.Name = clash;
                row.Status = ClashStatus.New;
                row.Distance = -0.05;
                row.GridLocation = "C-4";
                row.Level = "Level 03";
                row.Left.Name = "left";
                row.Right.Name = "right";
                test.Add(row);
            }

            return test;
        }

        /// <summary>
        /// Three tests in RUN order: Floors ran first with one clash, Walls second with
        /// three, Columns third with two. The report order is Walls, Columns, Floors.
        /// </summary>
        private static ClashReport MixedOrder()
        {
            ClashReport report = Report();
            Found(report, "Floors", "Floors 1");
            Found(report, "Walls", "Walls 1", "Walls 2", "Walls 3");
            Found(report, "Columns", "Columns 1", "Columns 2");
            return report;
        }

        /// <summary>
        /// Renders the pictures the way the add-in does during the run: the test number
        /// is handed out in the order the tests write their first picture, which is the
        /// order they RAN, and the clash number counts up inside the test. Each picture
        /// holds its own row's name, so a picture that ends up under another row's number
        /// is visible as a wrong name, not just a wrong count.
        /// </summary>
        private void RenderInRunOrder(ClashReport report, IList<string> skip)
        {
            Directory.CreateDirectory(ImageNaming.FolderFor(workbook));

            foreach (TestReport test in ReportOrderTestsHelper.InRunOrder(report))
            {
                foreach (ClashRow row in test.Rows)
                {
                    if (skip != null && skip.Contains(row.Name))
                    {
                        continue;
                    }

                    int testIndex = report.ImageIndexFor(test);
                    int clashIndex = test.ImageCount + 1;
                    string path = ImageNaming.PathFor(workbook, testIndex, clashIndex);
                    File.WriteAllText(path, row.Name);

                    row.ImageFile = ImageNaming.FileNameFor(testIndex, clashIndex);
                    row.ImageLink = ImageNaming.LinkFor(workbook, testIndex, clashIndex);
                    row.ImagePath = path;
                }

                report.ReleaseImageIndex(test);
            }
        }

        private static IList<string> Names(IEnumerable<TestReport> tests)
        {
            List<string> names = new List<string>();

            foreach (TestReport test in tests)
            {
                names.Add(test.Name);
            }

            return names;
        }

        private static IList<string> Names(IEnumerable<ClashRow> rows)
        {
            List<string> names = new List<string>();

            foreach (ClashRow row in rows)
            {
                names.Add(row.Name);
            }

            return names;
        }

        private static IList<string> FileNames(IEnumerable<PictureNumber> numbers)
        {
            List<string> names = new List<string>();

            foreach (PictureNumber number in numbers)
            {
                names.Add(number.FileName);
            }

            return names;
        }

        // ---------- the order ----------

        // Run order Floors 1, Walls 3, Columns 2. Report order most clashes first.
        [Test]
        public void TestsComeMostClashesFirstWhateverOrderTheyRanIn()
        {
            ClashReport report = MixedOrder();

            Assert.That(Names(ReportOrder.Tests(report)),
                Is.EqualTo(new[] { "Walls", "Columns", "Floors" }),
                "the report order is most clashes first, not the order the tests ran");
        }

        // Two tests on the same count keep the order they were created in, which is what
        // both client exports do, measured over a tie group of 1807.
        [Test]
        public void TiedTestsKeepTheOrderTheyWereCreatedIn()
        {
            ClashReport report = Report();
            Found(report, "Floors", "Floors 1", "Floors 2");
            Found(report, "Walls", "Walls 1", "Walls 2", "Walls 3");
            Found(report, "Columns", "Columns 1", "Columns 2");
            Found(report, "Doors", "Doors 1", "Doors 2");

            Assert.That(Names(ReportOrder.Tests(report)),
                Is.EqualTo(new[] { "Walls", "Floors", "Columns", "Doors" }),
                "a tie has to keep the creation order, the sort is stable");
        }

        // Inside a test the rows stay as Clash Detective listed them. Nothing sorts them.
        [Test]
        public void RowsInsideATestKeepClashDetectivesOrder()
        {
            ClashReport report = MixedOrder();

            Assert.That(Names(ReportOrder.Rows(report)),
                Is.EqualTo(new[] { "Walls 1", "Walls 2", "Walls 3", "Columns 1", "Columns 2", "Floors 1" }),
                "every clash in report order, test by test, row by row");
        }

        // ---------- the numbers ----------

        // Walls is block 0, Columns block 1, Floors block 2, and inside a block the clash
        // number is the row number. Under the old rule Floors was cd00 because it ran first.
        [Test]
        public void EveryPictureCarriesTheNumberOfItsRow()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);

            Assert.That(FileNames(ReportOrder.PictureNumbers(report)),
                Is.EqualTo(new[]
                {
                    "cd000001.jpg", "cd000002.jpg", "cd000003.jpg",
                    "cd010001.jpg", "cd010002.jpg",
                    "cd020001.jpg",
                }),
                "the picture number is the row number in report order");
        }

        // The old numbering, kept here so the difference is visible: Floors ran first and
        // took cd00, Walls took cd01, Columns took cd02.
        [Test]
        public void TheRunOrderNumberingIsNotTheReportOrderNumbering()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);

            Assert.That(Names(ReportOrder.Rows(report)),
                Is.EqualTo(new[] { "Walls 1", "Walls 2", "Walls 3", "Columns 1", "Columns 2", "Floors 1" }));

            List<string> before = new List<string>();

            foreach (ClashRow row in ReportOrder.Rows(report))
            {
                before.Add(row.ImageFile);
            }

            Assert.That(before,
                Is.EqualTo(new[]
                {
                    "cd010001.jpg", "cd010002.jpg", "cd010003.jpg",
                    "cd020001.jpg", "cd020002.jpg",
                    "cd000001.jpg",
                }),
                "this is what the run order wrote, and it is the wrong numbers for the rows");
        }

        // A test with no picture at all leaves no gap, the same way the run order did.
        [Test]
        public void ATestWithoutAPictureLeavesNoGapInTheTestNumbers()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, new[] { "Columns 1", "Columns 2" });

            Assert.That(FileNames(ReportOrder.PictureNumbers(report)),
                Is.EqualTo(new[] { "cd000001.jpg", "cd000002.jpg", "cd000003.jpg", "cd010001.jpg" }),
                "Columns has no picture, so Floors is block 1 and not block 2");
        }

        // A row without a picture inside a pictured test is skipped, so the clash number
        // counts pictured rows, which is what the run order did too.
        [Test]
        public void ARowWithoutAPictureIsNotNumbered()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, new[] { "Walls 2" });

            IList<PictureNumber> numbers = ReportOrder.PictureNumbers(report);

            Assert.That(FileNames(numbers),
                Is.EqualTo(new[] { "cd000001.jpg", "cd000002.jpg", "cd010001.jpg", "cd010002.jpg", "cd020001.jpg" }));
            Assert.That(numbers[1].Row.Name, Is.EqualTo("Walls 3"),
                "the second picture of Walls is the third row, because the second has none");
        }

        [Test]
        public void ARowWithNoPictureHasNoNumber()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, new[] { "Walls 2" });

            Assert.That(ReportOrder.PictureNumberFor(report, report.InReportOrder()[0].Rows[1]), Is.Null);
            Assert.That(ReportOrder.PictureNumberFor(report, report.InReportOrder()[0].Rows[2]).FileName,
                Is.EqualTo("cd000002.jpg"));
        }

        // ---------- the rename ----------

        // The one that matters. After the rename, walk the rows in report order and the
        // picture on row N is cd<block><N>, the file is on disk, and it is the SAME
        // picture the row had before, checked by its content.
        [Test]
        public void AfterTheRenameTheRowNumberAndThePictureNumberAgree()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);

            ImageRenumberingOutcome outcome = ImageRenumbering.Apply(report, workbook);

            Assert.That(outcome.Renamed, Is.EqualTo(6), "every picture was under a run order number");
            Assert.That(outcome.Missing, Is.EqualTo(0));
            Assert.That(outcome.Problems, Is.Empty);

            int block = 0;

            foreach (TestReport test in ReportOrder.Tests(report))
            {
                int rowNumber = 0;

                foreach (ClashRow row in test.Rows)
                {
                    rowNumber++;
                    int testIndex;
                    int clashIndex;

                    Assert.That(ImageNaming.TryRead(row.ImageFile, out testIndex, out clashIndex), Is.True);
                    Assert.That(testIndex, Is.EqualTo(block), row.Name + " is in block " + block);
                    Assert.That(clashIndex, Is.EqualTo(rowNumber), row.Name + " is row " + rowNumber);
                    Assert.That(File.Exists(row.ImagePath), Is.True, row.Name + " has no file at " + row.ImagePath);
                    Assert.That(File.ReadAllText(row.ImagePath), Is.EqualTo(row.Name),
                        "the picture under " + row.ImageFile + " is another row's picture");
                    Assert.That(Path.GetFileName(row.ImagePath), Is.EqualTo(row.ImageFile));
                    Assert.That(row.ImageLink, Is.EqualTo(ImageNaming.LinkFor(workbook, block, rowNumber)));
                }

                Assert.That(test.ImageIndex, Is.EqualTo(block), test.Name + " carries its report order block");
                block++;
            }
        }

        // Floors ran first and held cd000001. Walls is block 0 in the report, so Walls 1
        // has to become cd000001 while Floors 1 still holds it. A one pass rename would
        // write one over the other.
        [Test]
        public void ASwapBetweenTwoTestsOverwritesNothing()
        {
            ClashReport report = Report();
            Found(report, "Floors", "Floors 1");
            Found(report, "Walls", "Walls 1", "Walls 2");
            RenderInRunOrder(report, null);

            Assert.That(File.ReadAllText(ImageNaming.PathFor(workbook, 0, 1)), Is.EqualTo("Floors 1"));

            ImageRenumbering.Apply(report, workbook);

            Assert.That(File.ReadAllText(ImageNaming.PathFor(workbook, 0, 1)), Is.EqualTo("Walls 1"));
            Assert.That(File.ReadAllText(ImageNaming.PathFor(workbook, 0, 2)), Is.EqualTo("Walls 2"));
            Assert.That(File.ReadAllText(ImageNaming.PathFor(workbook, 1, 1)), Is.EqualTo("Floors 1"));
            Assert.That(Directory.GetFiles(ImageNaming.FolderFor(workbook)).Length, Is.EqualTo(3),
                "three pictures in, three pictures out");
        }

        [Test]
        public void TheRenameLeavesNoHoldingFileBehind()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);

            ImageRenumbering.Apply(report, workbook);

            foreach (string file in Directory.GetFiles(ImageNaming.FolderFor(workbook)))
            {
                Assert.That(file, Does.EndWith(".jpg"), "a holding name was left on disk");
            }

            Assert.That(Directory.GetFiles(ImageNaming.FolderFor(workbook)).Length, Is.EqualTo(6));
        }

        // Run order and report order agree when the tests ran most clashes first, which
        // is the case the sample where every test has the same count reduces to.
        [Test]
        public void APictureAlreadyUnderItsRowNumberIsNotMoved()
        {
            ClashReport report = Report();
            Found(report, "Walls", "Walls 1", "Walls 2", "Walls 3");
            Found(report, "Columns", "Columns 1", "Columns 2");
            RenderInRunOrder(report, null);

            ImageRenumberingOutcome outcome = ImageRenumbering.Apply(report, workbook);

            Assert.That(outcome.Unchanged, Is.EqualTo(5));
            Assert.That(outcome.Renamed, Is.EqualTo(0));
            Assert.That(outcome.Lines()[0],
                Is.EqualTo("IMAGES   numbered in report order: 0 renamed, 5 already right, 0 missing"));
        }

        // A picture the row names and the disk does not hold is reported and skipped.
        // The others are still renamed. A missing picture never stops anything.
        [Test]
        public void AMissingPictureIsNamedAndTheRestAreStillRenamed()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);
            File.Delete(ImageNaming.PathFor(workbook, 0, 1));

            ImageRenumberingOutcome outcome = ImageRenumbering.Apply(report, workbook);

            Assert.That(outcome.Missing, Is.EqualTo(1));
            Assert.That(outcome.Renamed, Is.EqualTo(5));
            Assert.That(outcome.Problems.Count, Is.EqualTo(1));
            Assert.That(outcome.Problems[0], Does.StartWith("Floors 1 should be cd020001.jpg"));
            Assert.That(File.ReadAllText(ImageNaming.PathFor(workbook, 0, 1)), Is.EqualTo("Walls 1"));
        }

        [Test]
        public void AReportWithNoPicturesRenamesNothing()
        {
            ClashReport report = MixedOrder();

            ImageRenumberingOutcome outcome = ImageRenumbering.Apply(report, workbook);

            Assert.That(outcome.Renamed, Is.EqualTo(0));
            Assert.That(outcome.Unchanged, Is.EqualTo(0));
            Assert.That(outcome.Missing, Is.EqualTo(0));
        }

        // ---------- what points at the renamed file ----------

        // The workbook link is read off the written file, because the object model has
        // twice reported a written file fine when it was not.
        [Test]
        public void TheWorkbookLinkPointsAtTheRenamedFile()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);
            ImageRenumbering.Apply(report, workbook);

            string path = new WorkbookWriter(new ReportOptions()).Write(report, workbook);

            using (XLWorkbook written = new XLWorkbook(path))
            {
                IXLWorksheet sheet = written.Worksheets.Worksheet(1);
                int row = HeaderRow(sheet) + 1;
                IXLCell cell = sheet.Cell(row, WorkbookWriter.ColumnImage);

                Assert.That(sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString(), Is.EqualTo("Walls 1"),
                    "the first row of the workbook is the first row of the report order");
                Assert.That(cell.HasHyperlink, Is.True);
                Assert.That(cell.GetHyperlink().ExternalAddress.OriginalString,
                    Does.EndWith("/cd000001.jpg"),
                    "the link has to open the renamed picture");
            }
        }

        // The XML href feeds the HTML page, so the page follows the same attribute.
        [Test]
        public void TheXmlHrefPointsAtTheRenamedFile()
        {
            ClashReport report = MixedOrder();
            RenderInRunOrder(report, null);
            ImageRenumbering.Apply(report, workbook);

            string path = new ClashReportXml().Write(report, ReportPaths.Xml(folder, report.OutputName));
            XDocument x = XDocument.Load(path);
            List<string> hrefs = new List<string>();

            foreach (XElement result in x.Root.Descendants("clashresult"))
            {
                hrefs.Add(Path.GetFileName(result.Attribute("href").Value.Replace('\\', '/')));
            }

            Assert.That(hrefs,
                Is.EqualTo(new[]
                {
                    "cd000001.jpg", "cd000002.jpg", "cd000003.jpg",
                    "cd010001.jpg", "cd010002.jpg",
                    "cd020001.jpg",
                }),
                "the page reads its pictures off these, so they have to be the renamed names in report order");
        }

        private static int HeaderRow(IXLWorksheet sheet)
        {
            for (int row = 1; row <= 40; row++)
            {
                if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() == "Clash Name")
                {
                    return row;
                }
            }

            throw new InvalidOperationException("no header row in the first 40");
        }
    }

    /// <summary>The order the tests were created in, which is the order they ran.</summary>
    internal static class ReportOrderTestsHelper
    {
        internal static IList<TestReport> InRunOrder(ClashReport report)
        {
            List<TestReport> tests = new List<TestReport>(report.InReportOrder());
            tests.Sort(delegate(TestReport left, TestReport right) { return left.Number.CompareTo(right.Number); });
            return tests;
        }
    }
}
