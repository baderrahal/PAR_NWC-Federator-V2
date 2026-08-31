using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The workbook as the client's own report has it, written for real and read back.
    ///
    /// The values here are the first row of the accepted report,
    /// 1104-PAR-1A02WO-XXX-BM-RPT-000001, so a failure can be checked against a file that
    /// exists rather than against an opinion.
    /// </summary>
    [TestFixture]
    public class ClientWorkbookTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Pipes = Root + "/Drainage/BLD-DR-Pipes & Pipe Fittings";
        private const string Walls = Root + "/Architecture/BLD-AR-Walls";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorClient", Guid.NewGuid().ToString("N"));
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

        // ---------- one test block, off the real report ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1A02WO", "1104-PAR-1A02WO-XXX-BM-RPT-000001");
            report.SetTreeRoot = Root;
            report.OpenDocument = "1104-PAR-1A02WO-XXX-BM-MOD-000001.nwf";
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 8, 5, 15, 18, 0);
            return report;
        }

        private static TestReport FirstTest(ClashReport report)
        {
            TestReport test = report.AddTest("BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Walls");
            test.LeftLocator = Pipes;
            test.RightLocator = Walls;
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "Hard (Conservative)";
            test.StatusWord = "OK";
            test.State = TestState.FoundClashes;
            test.Add(Clash1());
            return test;
        }

        /// <summary>Clash1 of the accepted report, field for field.</summary>
        private static ClashRow Clash1()
        {
            ClashRow row = new ClashRow();
            row.Name = "Clash1";
            row.Status = ClashStatus.New;
            row.Distance = -0.122;
            row.GridLocation = "B-1";
            row.Level = "ROF";
            row.Description = "Hard (Conservative)";
            row.X = 31.643;
            row.Y = -2.913;
            row.Z = 3.325;
            row.Left.ElementId = "1554240";
            row.Left.Name = "PVC";
            row.Left.ItemType = "Solid";
            row.Left.Family = "Pipe Types";
            row.Left.Type = "PVC-U";
            row.Right.ElementId = "784457";
            row.Right.Name = "ARC-WLL-HBLK-PAR";
            row.Right.ItemType = "Solid";
            row.Right.Family = "Basic Wall";
            row.Right.Type = "HBLK 200";
            return row;
        }

        private string Write(ClashReport report, ReportOptions options)
        {
            string path = Path.Combine(folder, report.OutputName + ".xlsx");
            return new WorkbookWriter(options).Write(report, path);
        }

        private static IList<string> RowText(IXLWorksheet sheet, int row, int width)
        {
            List<string> values = new List<string>();

            for (int column = 1; column <= width; column++)
            {
                values.Add(sheet.Cell(row, column).GetString());
            }

            return values;
        }

        /// <summary>The row the client's column headers are on, found by its first cell.</summary>
        private static int HeaderRow(IXLWorksheet sheet)
        {
            for (int row = 1; row <= 40; row++)
            {
                if (sheet.Cell(row, 2).GetString() == "Clash Name")
                {
                    return row;
                }
            }

            Assert.Fail("no row carries the client's Clash Name header");
            return 0;
        }

        // ---------- their columns, in their order, in a real file ----------

        // The one the brief asks for by name.
        [Test]
        public void TheSheetCarriesTheirColumnsInTheirOrder()
        {
            ClashReport report = Report();
            FirstTest(report);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                int header = HeaderRow(sheet);

                Assert.That(RowText(sheet, header, ClientFormat.ClashColumns.Length),
                    Is.EqualTo(ClientFormat.ClashColumns));
            }
        }

        [Test]
        public void TheirTestHeaderIsAboveItInTheirWords()
        {
            ClashReport report = Report();
            FirstTest(report);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");

                // The name, then their nine headers beside it, the way their own summary
                // table has them.
                Assert.That(sheet.Cell(1, 1).GetString(),
                    Is.EqualTo("BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Walls"));

                for (int i = 0; i < ClientFormat.TestHeader.Length; i++)
                {
                    Assert.That(sheet.Cell(1, i + 2).GetString(),
                        Is.EqualTo(ClientFormat.TestHeader[i]));
                }

                Assert.That(sheet.Cell(2, 2).GetString(), Is.EqualTo("0.025m"));
                Assert.That(sheet.Cell(2, 3).GetString(), Is.EqualTo("1"));
                Assert.That(sheet.Cell(2, 4).GetString(), Is.EqualTo("1"), "one New");
                Assert.That(sheet.Cell(2, 9).GetString(), Is.EqualTo("Hard (Conservative)"));
                Assert.That(sheet.Cell(2, 10).GetString(), Is.EqualTo("OK"));
            }
        }

        [Test]
        public void TheItemGroupLabelsSitOverTheirBlocks()
        {
            ClashReport report = Report();
            FirstTest(report);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                int labels = HeaderRow(sheet) - 1;

                Assert.That(sheet.Cell(labels, ClientFormat.FirstItemColumn + 1).GetString(),
                    Is.EqualTo("Item 1"));
                Assert.That(sheet.Cell(labels,
                        ClientFormat.FirstItemColumn + 1 + ClientFormat.ItemColumns).GetString(),
                    Is.EqualTo("Item 2"));
            }
        }

        // The whole row, checked against the accepted report's Clash1.
        [Test]
        public void TheFirstRowReadsExactlyAsTheirsDoes()
        {
            ClashReport report = Report();
            FirstTest(report);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                int row = HeaderRow(sheet) + 1;

                Assert.That(sheet.Cell(row, 2).GetString(), Is.EqualTo("Clash1"));
                Assert.That(sheet.Cell(row, 3).GetString(), Is.EqualTo("New"));
                Assert.That(sheet.Cell(row, 4).GetDouble(), Is.EqualTo(-0.122).Within(0.0000001));
                Assert.That(sheet.Cell(row, 5).GetString(), Is.EqualTo("B-1 : ROF"));
                Assert.That(sheet.Cell(row, 6).GetString(), Is.EqualTo("Hard (Conservative)"));
                Assert.That(sheet.Cell(row, 7).GetString(),
                    Is.EqualTo("x:31.643, y:-2.913, z:3.325"));
                Assert.That(sheet.Cell(row, 8).GetString(), Is.EqualTo("Element ID: 1554240"));
                Assert.That(sheet.Cell(row, 9).GetString(), Is.EqualTo("PVC"));
                Assert.That(sheet.Cell(row, 10).GetString(), Is.EqualTo("Solid"));
                Assert.That(sheet.Cell(row, 11).GetString(), Is.EqualTo("Element ID: 784457"));
                Assert.That(sheet.Cell(row, 12).GetString(), Is.EqualTo("ARC-WLL-HBLK-PAR"));
                Assert.That(sheet.Cell(row, 13).GetString(), Is.EqualTo("Solid"));
            }
        }

        // Their distance is the raw signed number, negative on a hard clash, and a number
        // rather than text so it sorts and filters.
        [Test]
        public void TheDistanceIsANumberAndKeepsItsSign()
        {
            ClashReport report = Report();
            FirstTest(report);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLCell cell = workbook.Worksheet("T0001")
                    .Cell(HeaderRow(workbook.Worksheet("T0001")) + 1, 4);

                Assert.That(cell.DataType, Is.EqualTo(XLDataType.Number));
                Assert.That(cell.GetDouble(), Is.LessThan(0.0));
            }
        }

        // ---------- ours after theirs, and the switch that drops ours ----------

        // The one the brief asks for by name.
        [Test]
        public void OursComeAfterTheirsAndCanBeLeftOutForASubmission()
        {
            ClashReport report = Report();
            FirstTest(report);

            ReportOptions everything = new ReportOptions();
            ReportOptions clientOnly = new ReportOptions();
            clientOnly.ClientColumnsOnly = true;

            using (XLWorkbook workbook = new XLWorkbook(Write(report, everything)))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                int header = HeaderRow(sheet);

                Assert.That(sheet.Cell(header, 14).GetString(), Is.EqualTo("Item 1 Family"));
                Assert.That(sheet.Cell(header + 1, 14).GetString(), Is.EqualTo("Pipe Types"));
            }

            // A second file, because the two must not share one.
            string path = Path.Combine(folder, "client only.xlsx");
            new WorkbookWriter(clientOnly).Write(report, path);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                int header = HeaderRow(sheet);

                Assert.That(sheet.Cell(header, 14).GetString(), Is.EqualTo(string.Empty),
                    "the client only sheet still has our columns on it");

                // And theirs are untouched, which is the point. Ours came off the right
                // hand end and moved none of theirs.
                Assert.That(RowText(sheet, header, ClientFormat.ClashColumns.Length),
                    Is.EqualTo(ClientFormat.ClashColumns));
            }
        }

        // ---------- images ----------

        [Test]
        public void ARowWithAPictureLinksToItRelatively()
        {
            ClashReport report = Report();
            TestReport test = FirstTest(report);
            ClashRow row = test.Rows[0];

            row.ImageFile = ImageNaming.FileNameFor(0, 1);
            row.ImageLink = ImageNaming.LinkFor(
                Path.Combine(folder, report.OutputName + ".xlsx"), 0, 1);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                IXLCell cell = sheet.Cell(HeaderRow(sheet) + 1, 1);

                Assert.That(cell.GetString(), Is.EqualTo("cd000001.jpg"));
                Assert.That(cell.HasHyperlink, Is.True, "the picture is not reachable from the row");

                // External, not internal. Handed a plain string ClosedXML reads a relative
                // path as an internal address, and the cell then tries to jump to a sheet
                // by that name rather than opening the picture.
                Assert.That(cell.GetHyperlink().IsExternal, Is.True,
                    "the link is internal, so it opens nothing");
                Assert.That(cell.GetHyperlink().ExternalAddress, Is.Not.Null);
                Assert.That(cell.GetHyperlink().ExternalAddress.ToString(),
                    Does.Contain("cd000001.jpg"));
            }
        }

        /// <summary>
        /// The same again, read out of the written file rather than out of the library, so
        /// this proves what a person opening the workbook actually gets.
        /// </summary>
        [Test]
        public void TheWrittenFileItselfHoldsARelativeLinkToThePicture()
        {
            ClashReport report = Report();
            TestReport test = FirstTest(report);
            string path = Path.Combine(folder, report.OutputName + ".xlsx");

            test.Rows[0].ImageFile = ImageNaming.FileNameFor(0, 1);
            test.Rows[0].ImageLink = ImageNaming.LinkFor(path, 0, 1);

            new WorkbookWriter(new ReportOptions()).Write(report, path);

            string rels = SheetRelationships(path);

            Assert.That(rels, Does.Contain("cd000001.jpg"), "the file holds no link to the picture");
            Assert.That(rels, Does.Contain("External"),
                "the link is not external, so it will not open a file");
            Assert.That(rels, Does.Contain("1104-PAR-1A02WO-XXX-BM-RPT-000001_files"));
            Assert.That(rels, Does.Not.Contain(folder),
                "an absolute path only works on the machine that wrote it");
        }

        /// <summary>Every sheet relationship part of a written workbook, as one string.</summary>
        private static string SheetRelationships(string path)
        {
            System.Text.StringBuilder all = new System.Text.StringBuilder();

            using (System.IO.Compression.ZipArchive archive =
                System.IO.Compression.ZipFile.OpenRead(path))
            {
                foreach (System.IO.Compression.ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.IndexOf("worksheets/_rels", StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    using (StreamReader reader = new StreamReader(entry.Open()))
                    {
                        all.AppendLine(reader.ReadToEnd());
                    }
                }
            }

            return all.ToString();
        }

        // The one the brief asks for by name. A picture that fails leaves its cell empty,
        // and the workbook is still written, because the workbook is the point of the run
        // and a missing picture is not worth losing it over.
        [Test]
        public void TheWorkbookIsStillWrittenWhenEveryImageFails()
        {
            ClashReport report = Report();
            TestReport test = FirstTest(report);

            for (int i = 2; i <= 13; i++)
            {
                ClashRow row = Clash1();
                row.Name = "Clash" + i;
                test.Add(row);
            }

            // Every one of them failed, so not a single row carries a picture and the
            // tally is all failures.
            foreach (ClashRow row in test.Rows)
            {
                report.Images.RenderFailed(row.Name, 0.3);
            }

            Assert.That(report.Images.Written, Is.EqualTo(0));
            Assert.That(report.Images.Failed, Is.EqualTo(13));

            string path = Write(report, new ReportOptions());

            Assert.That(File.Exists(path), Is.True, "the workbook was lost with the pictures");
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                int header = HeaderRow(sheet);

                // Thirteen rows, every one of them complete except the Image cell.
                for (int i = 0; i < 13; i++)
                {
                    int row = header + 1 + i;

                    Assert.That(sheet.Cell(row, 1).GetString(), Is.EqualTo(string.Empty),
                        "row " + (i + 1) + " claims a picture it does not have");
                    Assert.That(sheet.Cell(row, 2).GetString(), Is.EqualTo("Clash" + (i + 1)));
                    Assert.That(sheet.Cell(row, 7).GetString(),
                        Is.EqualTo("x:31.643, y:-2.913, z:3.325"),
                        "row " + (i + 1) + " lost a column because its picture failed");
                }
            }
        }

        [Test]
        public void ARowWithNoPictureLeavesTheCellEmptyRatherThanSayingSo()
        {
            ClashReport report = Report();
            FirstTest(report);

            using (XLWorkbook workbook = new XLWorkbook(Write(report, new ReportOptions())))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");
                IXLCell cell = sheet.Cell(HeaderRow(sheet) + 1, 1);

                Assert.That(cell.GetString(), Is.EqualTo(string.Empty));
                Assert.That(cell.HasHyperlink, Is.False);
            }
        }

        // ---------- the numbering is handed out here, so it cannot repeat or skip ----------

        [Test]
        public void TheImageTestNumberCountsTestsThatHavePictures()
        {
            ClashReport report = Report();

            TestReport first = report.AddTest("first");
            TestReport second = report.AddTest("second");
            TestReport third = report.AddTest("third");

            Assert.That(report.ImageIndexFor(first), Is.EqualTo(0));
            Assert.That(report.ImageIndexFor(first), Is.EqualTo(0), "asking twice moved it");
            Assert.That(report.ImageIndexFor(second), Is.EqualTo(1));
            Assert.That(report.ImageIndexFor(third), Is.EqualTo(2));
        }

        // A test whose first render fails must hand the number back, or the next test
        // starts at a number with nothing before it and the folder carries a gap.
        [Test]
        public void ATestThatClaimedANumberAndWroteNothingGivesItBack()
        {
            ClashReport report = Report();
            TestReport first = report.AddTest("first");
            TestReport second = report.AddTest("second");

            Assert.That(report.ImageIndexFor(first), Is.EqualTo(0));
            report.ReleaseImageIndex(first);

            Assert.That(first.ImageIndex, Is.EqualTo(-1));
            Assert.That(report.ImageIndexFor(second), Is.EqualTo(0),
                "the second test should take the number the first gave back");
        }

        [Test]
        public void ATestThatActuallyWroteOneKeepsItsNumber()
        {
            ClashReport report = Report();
            TestReport test = FirstTest(report);

            report.ImageIndexFor(test);
            test.Rows[0].ImageFile = "cd000001.jpg";
            report.ReleaseImageIndex(test);

            Assert.That(test.ImageIndex, Is.EqualTo(0), "a test with a picture lost its number");
            Assert.That(test.ImageCount, Is.EqualTo(1));
        }
    }
}
