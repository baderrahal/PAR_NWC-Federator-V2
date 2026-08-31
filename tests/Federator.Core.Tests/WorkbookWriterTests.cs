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
    /// The writer lives in Core rather than in the add-in exactly so these can write a
    /// real xlsx and read it back without Navisworks.
    /// </summary>
    [TestFixture]
    public class WorkbookWriterTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string Floors = Root + "/Architecture/BLD-AR-Floors";
        private const string Ducts = Root + "/Mechanical/BLD-ME-Ducts";
        private const string Cables = Root + "/Electrical/BLD-EL-Cables";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "FederatorWorkbook", Guid.NewGuid().ToString("N"));
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

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", "1104-PAR-1C07BC-ZZZ-BM-MOD-000001");
            report.SetTreeRoot = Root;
            report.OpenDocument = "1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf";
            report.SourceFile = @"C:\in\1104-PAR_CLASH_AllInOne.xml";
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 9, 1, 9, 0, 0);
            report.BuildStamp = "1.0.0.0 abcdef12 built 2026-08-31 10:33:38";
            report.ClashStepSeconds = 105.3;
            return report;
        }

        private static ClashRow Row(string name, ClashStatus status, double distance, int raw)
        {
            ClashRow row = new ClashRow();
            row.Name = name;
            row.Status = status;
            row.Distance = distance;
            row.RawClashes = raw;
            row.IsGroup = raw > 1;
            row.GridLocation = "C-4";
            row.Level = "Level 03";
            row.Found = new DateTime(2026, 8, 31, 10, 15, 0);
            row.X = 12.5;
            row.Y = -3.25;
            row.Z = 9.0;
            row.Left.Name = "Floor 200mm";
            row.Left.Family = "Floor";
            row.Left.Type = "Generic 200mm";
            row.Left.Material = "Concrete, Cast In Situ";
            row.Left.SourceFile = @"C:\in\1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc";
            row.Left.Discipline = "AR";
            row.Left.ElementId = "884213";
            row.Right.Name = "Rectangular Duct";
            row.Right.Family = "Rectangular Duct";
            row.Right.Type = "Mitered Elbows";
            row.Right.Material = "Galvanised Steel";
            row.Right.SourceFile = @"C:\in\1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc";
            row.Right.Discipline = "ME";
            row.Right.ElementId = "991002";
            return row;
        }

        private string Write(ClashReport report)
        {
            string path = ReportPaths.Workbook(folder, report.OutputName);
            return new WorkbookWriter().Write(report, path);
        }

        // ---------- it actually writes a workbook Excel can open ----------

        [Test]
        public void ItWritesAFileThatOpensAsAWorkbook()
        {
            ClashReport report = Report();
            TestReport test = report.AddTest("BLD-AR-Floors v BLD-ME-Ducts");
            test.LeftLocator = Floors;
            test.RightLocator = Ducts;
            test.State = TestState.FoundClashes;
            test.Add(Row("Clash1", ClashStatus.New, -0.145, 1));

            string path = Write(report);

            Assert.That(File.Exists(path), Is.True);
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));
            Assert.That(path, Does.EndWith(".xlsx"));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheet(SheetNames.SummarySheet), Is.Not.Null);
                Assert.That(workbook.Worksheet(SheetNames.MatrixSheet), Is.Not.Null);
                Assert.That(workbook.Worksheet("T0001"), Is.Not.Null);
            }
        }

        [Test]
        public void ItOverwritesTheSameWayTheNwfAndNwdDo()
        {
            ClashReport first = Report();
            first.AddTest("one").State = TestState.Passed;
            string path = Write(first);
            long firstSize = new FileInfo(path).Length;

            ClashReport second = Report();

            for (int i = 0; i < 40; i++)
            {
                TestReport test = second.AddTest("test " + i);
                test.LeftLocator = Floors;
                test.RightLocator = Ducts;
                test.State = TestState.FoundClashes;
                test.Add(Row("Clash", ClashStatus.New, -0.1, 1));
            }

            string again = Write(second);

            Assert.That(again, Is.EqualTo(path), "a second copy appeared rather than an overwrite");
            Assert.That(Directory.GetFiles(folder, "*.xlsx").Length, Is.EqualTo(1));
            Assert.That(new FileInfo(path).Length, Is.Not.EqualTo(firstSize));
        }

        // ---------- one sheet per test that found something ----------

        [Test]
        public void ATestThatFoundNothingGetsNoSheetAndTheOthersKeepTheirNumbers()
        {
            ClashReport report = Report();

            report.AddTest("skipped one").State = TestState.Skipped;
            report.AddTest("passed one").State = TestState.Passed;

            TestReport third = report.AddTest("found one");
            third.LeftLocator = Floors;
            third.RightLocator = Ducts;
            third.State = TestState.FoundClashes;
            third.Add(Row("Clash1", ClashStatus.New, -0.1, 1));

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(3),
                    "Summary, Matrix and the one test that found something");
                Assert.That(workbook.Worksheets.Contains("T0003"), Is.True,
                    "the sheet is numbered by the test, not by which ones have sheets");
                Assert.That(workbook.Worksheets.Contains("T0001"), Is.False);
                Assert.That(workbook.Worksheets.Contains("T0002"), Is.False);
            }
        }

        // Past a thousand tests the numbering has to keep working and stay inside Excel's
        // limit on every sheet.
        [Test]
        public void ThousandsOfSheetsAllStayInsideExcelsLimit()
        {
            ClashReport report = Report();

            for (int i = 0; i < 1200; i++)
            {
                TestReport test = report.AddTest("test number " + i);
                test.LeftLocator = Floors;
                test.RightLocator = Ducts;

                if (i % 100 == 0)
                {
                    test.State = TestState.FoundClashes;
                    test.Add(Row("Clash", ClashStatus.New, -0.1, 1));
                }
                else
                {
                    test.State = TestState.Passed;
                }
            }

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                Assert.That(workbook.Worksheets.Contains("T0001"), Is.True);
                Assert.That(workbook.Worksheets.Contains("T1001"), Is.True,
                    "numbering stopped working past a thousand");

                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    Assert.That(sheet.Name.Length, Is.LessThanOrEqualTo(SheetNames.MaxLength),
                        sheet.Name);
                    Assert.That(SheetNames.IsAcceptable(sheet.Name), Is.True, sheet.Name);
                }
            }
        }

        // A test name Excel would refuse must never reach a sheet name.
        [Test]
        public void ATestNameFullOfCharactersExcelRefusesStillWrites()
        {
            ClashReport report = Report();

            TestReport test = report.AddTest(@"Floors [1] : Ducts / Pipes \ Cables * ? and more");
            test.LeftLocator = Floors;
            test.RightLocator = Ducts;
            test.State = TestState.FoundClashes;
            test.Add(Row("Clash1", ClashStatus.New, -0.1, 1));

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                Assert.That(workbook.Worksheets.Contains("T0001"), Is.True);

                // The full name is still in the Summary, where a cell can hold anything.
                IXLWorksheet summary = workbook.Worksheet(SheetNames.SummarySheet);
                Assert.That(Contains(summary, test.Name), Is.True,
                    "the full test name was lost as well as being kept out of the sheet name");
            }
        }

        // ---------- the columns ----------

        [Test]
        public void TheTestSheetCarriesEveryColumnTheBriefAsksFor()
        {
            ClashReport report = Report();
            TestReport test = report.AddTest("BLD-AR-Floors v BLD-ME-Ducts");
            test.LeftLocator = Floors;
            test.RightLocator = Ducts;
            test.State = TestState.FoundClashes;
            test.Add(Row("Clash1", ClashStatus.New, -0.145, 1));

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");

                foreach (string column in WorkbookWriter.TestColumns)
                {
                    Assert.That(Contains(sheet, column), Is.True, "no column called " + column);
                }

                // The values that matter most, because without them whoever fixes it has
                // to open the model.
                Assert.That(Contains(sheet, "Floor"), Is.True, "the family is missing");
                Assert.That(Contains(sheet, "Generic 200mm"), Is.True, "the type is missing");
                Assert.That(Contains(sheet, "Concrete, Cast In Situ"), Is.True,
                    "the material is missing");
                Assert.That(Contains(sheet, "Mitered Elbows"), Is.True);
                Assert.That(Contains(sheet, "Galvanised Steel"), Is.True);
                Assert.That(Contains(sheet, "884213"), Is.True, "the element id is missing");
                Assert.That(Contains(sheet, "C-4"), Is.True, "the grid is missing");
                Assert.That(Contains(sheet, "Level 03"), Is.True, "the level is missing");
            }
        }

        [Test]
        public void AGroupRowCarriesItsRawCountSoTheGroupingHidesNothing()
        {
            ClashReport report = Report();
            TestReport test = report.AddTest("grouped");
            test.LeftLocator = Floors;
            test.RightLocator = Ducts;
            test.State = TestState.FoundClashes;
            test.Add(Row("Group1", ClashStatus.New, -0.145, 14));

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheet("T0001");

                Assert.That(Contains(sheet, "Raw clashes"), Is.True);
                Assert.That(Contains(sheet, "14"), Is.True,
                    "the count behind the group was not written");
            }
        }

        // ---------- the Summary ----------

        [Test]
        public void TheSummaryHasOneRowPerTestInTheFile()
        {
            ClashReport report = Report();

            for (int i = 0; i < 120; i++)
            {
                TestReport test = report.AddTest("test " + i);
                test.LeftLocator = Floors;
                test.RightLocator = i % 2 == 0 ? Ducts : Cables;
                test.State = i < 90 ? TestState.Skipped : TestState.Passed;

                if (i < 90)
                {
                    test.SkippedReason = "a side finds nothing in this model";
                }
            }

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheet(SheetNames.SummarySheet);
                int headerRow = RowOf(sheet, SummaryHeader);

                Assert.That(headerRow, Is.GreaterThan(0), "no Summary header row");

                int lastRow = sheet.LastRowUsed().RowNumber();

                Assert.That(lastRow - headerRow, Is.EqualTo(120),
                    "the Summary lost tests, it must carry every test in the file");
            }
        }

        [Test]
        public void TheSummaryKeepsSkippedPassedAndFoundApart()
        {
            ClashReport report = Report();

            TestReport skipped = report.AddTest("skipped");
            skipped.State = TestState.Skipped;
            skipped.SkippedReason = "the right side finds nothing in this model";

            report.AddTest("passed").State = TestState.Passed;

            TestReport found = report.AddTest("found");
            found.LeftLocator = Floors;
            found.RightLocator = Ducts;
            found.State = TestState.FoundClashes;
            found.Add(Row("Clash1", ClashStatus.New, -0.1, 3));

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheet(SheetNames.SummarySheet);

                Assert.That(Contains(sheet, "Passed, ran and found nothing"), Is.True);
                Assert.That(Contains(sheet, "Skipped, not run and not passed"), Is.True);
                Assert.That(Contains(sheet, "the right side finds nothing in this model"), Is.True,
                    "a skipped test has to carry why");
            }
        }

        [Test]
        public void TheSummaryLinksToTheSheetsThatExistAndNotToOnesThatDoNot()
        {
            ClashReport report = Report();

            report.AddTest("passed").State = TestState.Passed;

            TestReport found = report.AddTest("found");
            found.LeftLocator = Floors;
            found.RightLocator = Ducts;
            found.State = TestState.FoundClashes;
            found.Add(Row("Clash1", ClashStatus.New, -0.1, 1));

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheet(SheetNames.SummarySheet);
                int links = 0;

                foreach (IXLCell cell in sheet.CellsUsed())
                {
                    if (cell.HasHyperlink)
                    {
                        links++;
                    }
                }

                Assert.That(links, Is.EqualTo(1),
                    "a link was written to a sheet that does not exist");
            }
        }

        // ---------- the Matrix ----------

        [Test]
        public void TheMatrixSaysSkippedRatherThanZero()
        {
            ClashReport report = Report();

            TestReport skipped = report.AddTest("skipped");
            skipped.LeftLocator = Floors;
            skipped.RightLocator = Ducts;
            skipped.State = TestState.Skipped;

            TestReport passed = report.AddTest("passed");
            passed.LeftLocator = Floors;
            passed.RightLocator = Cables;
            passed.State = TestState.Passed;

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheet(SheetNames.MatrixSheet);

                Assert.That(Contains(sheet, "skipped"), Is.True,
                    "a skipped pair did not say so on the matrix");
                Assert.That(Contains(sheet, "Architecture"), Is.True,
                    "the discipline read off the folder is missing");
                Assert.That(Contains(sheet, "Mechanical"), Is.True);
                Assert.That(Contains(sheet, "BLD-AR-Floors"), Is.True);
            }
        }

        // ---------- refusals ----------

        [Test]
        public void NoReportOrNoPathIsRefusedRatherThanWritten()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { new WorkbookWriter().Write(null, Path.Combine(folder, "x.xlsx")); });
            Assert.Throws<ArgumentException>(
                delegate { new WorkbookWriter().Write(Report(), null); });
        }

        [Test]
        public void AFolderThatIsNotThereYetIsMadeRatherThanRefused()
        {
            string deeper = Path.Combine(folder, "Clash Reports", "again");
            ClashReport report = Report();
            report.AddTest("one").State = TestState.Passed;

            string path = new WorkbookWriter().Write(
                report, ReportPaths.Workbook(deeper, report.OutputName));

            Assert.That(File.Exists(path), Is.True);
        }

        private const string SummaryHeader = "Test name";

        private static bool Contains(IXLWorksheet sheet, string text)
        {
            foreach (IXLCell cell in sheet.CellsUsed())
            {
                if (cell.GetFormattedString().IndexOf(text, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int RowOf(IXLWorksheet sheet, string text)
        {
            foreach (IXLCell cell in sheet.CellsUsed())
            {
                if (string.Equals(cell.GetFormattedString(), text, StringComparison.Ordinal))
                {
                    return cell.Address.RowNumber;
                }
            }

            return 0;
        }
    }
}
