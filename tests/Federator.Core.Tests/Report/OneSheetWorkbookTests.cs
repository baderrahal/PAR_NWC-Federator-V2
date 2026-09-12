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
    /// The workbook as the client's own report has it: ONE sheet, every test one after
    /// another, most clashes first.
    ///
    /// This replaces WorkbookWriterTests and ClientWorkbookTests, whose whole subject was
    /// the Summary sheet, the Matrix sheet and the sheet per test. All three were asked
    /// for before anyone had put a real Navisworks report beside ours, and all three were
    /// removed on purpose once both had been seen together.
    ///
    /// Every expected value here was measured off
    /// samples\client-report\1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx on 2026-09-01.
    /// </summary>
    [TestFixture]
    public class OneSheetWorkbookTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string OutputName = "1104-PAR-1C07BC-ZZZ-BM-RPT-000001";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorOneSheet");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        // ---------- a report to write ----------

        private static ClashReport Report()
        {
            ClashReport report = new ClashReport("1C07BC", OutputName);
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 9, 1, 20, 0, 0);
            return report;
        }

        private static TestReport AddTest(ClashReport report, string name, int clashes)
        {
            TestReport test = report.AddTest(name);
            test.LeftLocator = Root + "/Architecture/BLD-AR-Walls";
            test.RightLocator = Root + "/Architecture/BLD-AR-Columns";
            test.Tolerance = 0.025;
            test.ToleranceUnits = "m";
            test.TestTypeName = "hard_conservative";
            test.StatusWord = "OK";
            test.State = clashes > 0 ? TestState.FoundClashes : TestState.Passed;

            for (int i = 1; i <= clashes; i++)
            {
                test.Add(Row(i));
            }

            return test;
        }

        /// <summary>Clash1 of 1A04WN, field for field.</summary>
        private static ClashRow Row(int number)
        {
            ClashRow row = new ClashRow();
            row.Name = "Clash" + number;
            row.Status = ClashStatus.New;
            row.Distance = -0.116;
            row.GridLocation = "A-1";
            row.Level = "LGF";
            row.Description = "Hard (Conservative)";
            row.X = 33.399;
            row.Y = 8.310;
            row.Z = -0.441;
            row.Left.ElementId = "707077";
            row.Left.Layer = "GRF";
            row.Left.Name = "ELE-CND-Standard";
            row.Left.ItemType = "Solid";
            row.Right.ElementId = "1369872";
            row.Right.Layer = "GRF";
            row.Right.Name = "Concrete - Cast-in-Place Concrete - 35 MPa";
            row.Right.ItemType = "Solid";
            return row;
        }

        private string Write(ClashReport report)
        {
            return Write(report, new ReportOptions());
        }

        private string Write(ClashReport report, ReportOptions options)
        {
            return new WorkbookWriter(options)
                .Write(report, Path.Combine(folder, OutputName + ".xlsx"));
        }

        /// <summary>The row each test block's column headings sit on, in order.</summary>
        private static IList<int> HeaderRows(IXLWorksheet sheet)
        {
            List<int> rows = new List<int>();
            int last = sheet.LastRowUsed() == null ? 0 : sheet.LastRowUsed().RowNumber();

            for (int row = 1; row <= last; row++)
            {
                if (sheet.Cell(row, WorkbookWriter.ColumnClashName).GetString() == "Clash Name")
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        // ---------- one sheet and no others ----------

        // The one the brief asks for by name.
        [Test]
        public void ThereIsOneSheetAndNoOthers()
        {
            ClashReport report = Report();
            AddTest(report, "one", 3);
            AddTest(report, "two", 1);
            AddTest(report, "three", 0);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                List<string> names = new List<string>();

                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    names.Add(sheet.Name);
                }

                Assert.That(names.Count, Is.EqualTo(1),
                    "the client's report has one sheet and ours had fifty");
                Assert.That(names, Does.Not.Contain("Summary"));
                Assert.That(names, Does.Not.Contain("Matrix"));
                Assert.That(names[0], Is.EqualTo(OutputName.Substring(0, 31)),
                    "named after the report, cut at 31 the way Excel cuts theirs");
            }

            Assert.That(WorkbookWriter.SheetNamesFor(report).Count, Is.EqualTo(1));
        }

        [Test]
        public void EveryTestIsOnThatOneSheetOneAfterAnother()
        {
            ClashReport report = Report();
            AddTest(report, "first", 3);
            AddTest(report, "second", 2);
            AddTest(report, "third", 0);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);

                Assert.That(HeaderRows(sheet).Count, Is.EqualTo(3),
                    "one block per test, including the one that found nothing");
            }
        }

        [Test]
        public void TheTitleRowReadsAsTheirsDoes()
        {
            ClashReport report = Report();
            AddTest(report, "one", 1);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);

                Assert.That(sheet.Cell(1, 4).GetString(), Is.EqualTo("Clash Report"));
                Assert.That(HeaderRows(sheet)[0], Is.EqualTo(8),
                    "their first block starts at row 4 and its headings are on row 8");
            }
        }

        // ---------- their columns, in their positions ----------

        // The one the brief asks for by name.
        [Test]
        public void TheColumnsAreTheirsInTheirOrderAndTheirPositions()
        {
            ClashReport report = Report();
            AddTest(report, "one", 1);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                int row = HeaderRows(sheet)[0];

                // Measured off their own sheet, column letter for column letter.
                Assert.That(sheet.Cell(row, 1).GetString(), Is.EqualTo("Image"));
                Assert.That(sheet.Cell(row, 3).GetString(), Is.EqualTo("Clash Name"));
                Assert.That(sheet.Cell(row, 5).GetString(), Is.EqualTo("Status"));
                Assert.That(sheet.Cell(row, 6).GetString(), Is.EqualTo("Distance"));
                Assert.That(sheet.Cell(row, 7).GetString(), Is.EqualTo("Grid Location"));
                Assert.That(sheet.Cell(row, 8).GetString(), Is.EqualTo("Description"));
                Assert.That(sheet.Cell(row, 9).GetString(), Is.EqualTo("Clash Point"));
                Assert.That(sheet.Cell(row, 12).GetString(), Is.EqualTo("Item ID"));
                Assert.That(sheet.Cell(row, 13).GetString(), Is.EqualTo("Layer"));
                Assert.That(sheet.Cell(row, 14).GetString(), Is.EqualTo("Item Name"));
                Assert.That(sheet.Cell(row, 15).GetString(), Is.EqualTo("Item Type"));
                Assert.That(sheet.Cell(row, 16).GetString(), Is.EqualTo("Item ID"));
                Assert.That(sheet.Cell(row, 17).GetString(), Is.EqualTo("Layer"));
                Assert.That(sheet.Cell(row, 18).GetString(), Is.EqualTo("Item Name"));
                Assert.That(sheet.Cell(row, 19).GetString(), Is.EqualTo("Item Type"));

                // Item 1 and Item 2 over the two blocks, one row above.
                Assert.That(sheet.Cell(row - 1, 12).GetString(), Is.EqualTo("Item 1"));
                Assert.That(sheet.Cell(row - 1, 16).GetString(), Is.EqualTo("Item 2"));
            }
        }

        [Test]
        public void TheTestHeaderIsTheirNineBesideTheName()
        {
            ClashReport report = Report();
            AddTest(report, "BLD-AR-Walls-vs-BLD-AR-Columns", 2);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                int name = HeaderRows(sheet)[0] - 4;

                Assert.That(sheet.Cell(name, 1).GetString(),
                    Is.EqualTo("BLD-AR-Walls-vs-BLD-AR-Columns"));

                for (int i = 0; i < ClientFormat.TestHeader.Length; i++)
                {
                    Assert.That(sheet.Cell(name, 3 + i).GetString(),
                        Is.EqualTo(ClientFormat.TestHeader[i]));
                }

                Assert.That(sheet.Cell(name + 1, 3).GetString(), Is.EqualTo("0.025m"));
                Assert.That(sheet.Cell(name + 1, 4).GetString(), Is.EqualTo("2"));
                // Their nine run C to K, so Type is J and Status is K.
                Assert.That(sheet.Cell(name + 1, 10).GetString(), Is.EqualTo("Hard (Conservative)"));
                Assert.That(sheet.Cell(name + 1, 11).GetString(), Is.EqualTo("OK"));
            }
        }

        [Test]
        public void AClashRowReadsExactlyAsTheirsDoes()
        {
            ClashReport report = Report();
            AddTest(report, "one", 1);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                int row = HeaderRows(sheet)[0] + 1;

                Assert.That(sheet.Cell(row, 3).GetString(), Is.EqualTo("Clash1"));
                Assert.That(sheet.Cell(row, 5).GetString(), Is.EqualTo("New"));
                Assert.That(sheet.Cell(row, 6).GetDouble(), Is.EqualTo(-0.116).Within(0.0000001));
                Assert.That(sheet.Cell(row, 7).GetString(), Is.EqualTo("A-1 : LGF"));
                Assert.That(sheet.Cell(row, 8).GetString(), Is.EqualTo("Hard (Conservative)"));
                Assert.That(sheet.Cell(row, 9).GetString(),
                    Is.EqualTo("x:33.399, y:8.310, z:-0.441"));
                Assert.That(sheet.Cell(row, 12).GetString(), Is.EqualTo("Element ID: 707077"));
                Assert.That(sheet.Cell(row, 13).GetString(), Is.EqualTo("GRF"));
                Assert.That(sheet.Cell(row, 14).GetString(), Is.EqualTo("ELE-CND-Standard"));
                Assert.That(sheet.Cell(row, 15).GetString(), Is.EqualTo("Solid"));
                Assert.That(sheet.Cell(row, 16).GetString(), Is.EqualTo("Element ID: 1369872"));
            }
        }

        // The one the brief asks for by name.
        [Test]
        public void TheDistanceIsANumberWithThreeDecimals()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "one", 1);
            test.Rows[0].Distance = -0.328083992004395;

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                IXLCell cell = sheet.Cell(HeaderRows(sheet)[0] + 1, 6);

                Assert.That(cell.DataType, Is.EqualTo(XLDataType.Number),
                    "it has to stay a number so the column still sorts");
                Assert.That(cell.GetDouble(), Is.EqualTo(-0.328).Within(0.0000000001),
                    "the value is rounded, not the display. Theirs holds the short number");
                Assert.That(cell.Style.NumberFormat.Format, Is.Empty,
                    "theirs carries no number format at all");
                Assert.That(cell.GetFormattedString(), Is.EqualTo("-0.328"));
            }
        }

        // ---------- most clashes first ----------

        // The one the brief asks for by name.
        [Test]
        public void TheMostClashesComeFirstAndTheEmptyTestsLast()
        {
            ClashReport report = Report();
            AddTest(report, "empty one", 0);
            AddTest(report, "empty two", 0);
            AddTest(report, "two clashes", 2);
            AddTest(report, "twelve clashes", 12);
            AddTest(report, "six clashes", 6);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                List<string> names = new List<string>();

                foreach (int header in HeaderRows(sheet))
                {
                    names.Add(sheet.Cell(header - 4, 1).GetString());
                }

                Assert.That(names, Is.EqualTo(new[]
                {
                    "twelve clashes", "six clashes", "two clashes", "empty one", "empty two"
                }));
            }
        }

        // The tie rule, measured off both client exports against the order the tests sit in
        // the exchange file. Every tie group is in that original order and none is
        // alphabetical, including one group of 1807 tests.
        [Test]
        public void TwoTestsWithTheSameCountKeepTheOrderTheyWereCreatedIn()
        {
            ClashReport report = Report();
            AddTest(report, "zebra", 4);
            AddTest(report, "apple", 4);
            AddTest(report, "mango", 4);

            List<string> order = new List<string>();

            foreach (TestReport test in report.InReportOrder())
            {
                order.Add(test.Name);
            }

            Assert.That(order, Is.EqualTo(new[] { "zebra", "apple", "mango" }),
                "the tie rule is the order they were created, not the name");
        }

        [Test]
        public void ALargeTieGroupIsNotScrambled()
        {
            ClashReport report = Report();
            AddTest(report, "has some", 5);

            for (int i = 0; i < 400; i++)
            {
                AddTest(report, "empty " + i.ToString("000"), 0);
            }

            IList<TestReport> order = report.InReportOrder();

            Assert.That(order[0].Name, Is.EqualTo("has some"));

            for (int i = 0; i < 400; i++)
            {
                Assert.That(order[i + 1].Name, Is.EqualTo("empty " + i.ToString("000")),
                    "an unstable sort would scramble the tie group");
            }
        }

        // ---------- the pictures ----------

        [Test]
        public void ARowWithAPictureLinksToItRelatively()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "one", 1);
            test.Rows[0].ImageFile = "cd000001.jpg";
            test.Rows[0].ImageLink = OutputName + "_files/cd000001.jpg";

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                IXLCell cell = sheet.Cell(HeaderRows(sheet)[0] + 1, 1);

                // The cell is EMPTY and carries the link. Theirs holds nothing there at
                // all, with the picture sitting behind it, so writing the file name put a
                // string where their report shows a photo.
                Assert.That(cell.GetString(), Is.Empty,
                    "theirs shows a photo here, not a file name");
                Assert.That(cell.HasHyperlink, Is.True);
                Assert.That(cell.GetHyperlink().IsExternal, Is.True,
                    "an internal link opens nothing");
            }
        }

        [Test]
        public void ARowWithNoPictureLeavesTheCellEmpty()
        {
            ClashReport report = Report();
            AddTest(report, "one", 1);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                IXLCell cell = sheet.Cell(HeaderRows(sheet)[0] + 1, 1);

                Assert.That(cell.GetString(), Is.EqualTo(string.Empty));
                Assert.That(cell.HasHyperlink, Is.False);
            }
        }

        [Test]
        public void TheWorkbookIsStillWrittenWhenEveryImageFails()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "one", 13);

            foreach (ClashRow row in test.Rows)
            {
                report.Images.RenderFailed(row.Name, 0.3);
            }

            string path = Write(report);

            Assert.That(File.Exists(path), Is.True, "the workbook was lost with the pictures");

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                int header = HeaderRows(sheet)[0];

                for (int i = 0; i < 13; i++)
                {
                    Assert.That(sheet.Cell(header + 1 + i, 3).GetString(),
                        Is.EqualTo("Clash" + (i + 1)),
                        "row " + (i + 1) + " lost a column because its picture failed");
                }
            }
        }

        // ---------- the things that still hold whatever the layout ----------

        [Test]
        public void ItWritesAFileThatOpensAsAWorkbook()
        {
            ClashReport report = Report();
            AddTest(report, "one", 2);

            string path = Write(report);

            Assert.That(File.Exists(path), Is.True);
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));
        }

        [Test]
        public void ItOverwritesTheSameWayTheNwfAndNwdDo()
        {
            ClashReport report = Report();
            AddTest(report, "one", 2);

            string first = Write(report);
            string second = Write(report);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(Directory.GetFiles(folder, "*.xlsx").Length, Is.EqualTo(1),
                "no second copy and no date suffix");
        }

        [Test]
        public void ATestNameFullOfCharactersExcelRefusesStillWrites()
        {
            ClashReport report = Report();
            AddTest(report, @"a/b\c[d]e*f?g:h", 1);

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);

                // The name is a CELL now, not a sheet name, so nothing has to be sanitised
                // out of it and it survives whole.
                Assert.That(sheet.Cell(HeaderRows(sheet)[0] - 4, 1).GetString(),
                    Is.EqualTo(@"a/b\c[d]e*f?g:h"));
            }
        }

        [Test]
        public void AGroupRowCarriesItsRawCountSoTheGroupingHidesNothing()
        {
            ClashReport report = Report();
            TestReport test = AddTest(report, "one", 1);
            test.Rows[0].IsGroup = true;
            test.Rows[0].RawClashes = 14;

            using (XLWorkbook workbook = new XLWorkbook(Write(report)))
            {
                IXLWorksheet sheet = workbook.Worksheets.Worksheet(1);
                int name = HeaderRows(sheet)[0] - 4;

                // The Clashes cell counts the raw clashes, not the rows, so the grouping
                // hides nothing even though the client's layout has no column for it.
                Assert.That(sheet.Cell(name + 1, 4).GetString(), Is.EqualTo("14"));
            }
        }

        [Test]
        public void NoReportOrNoPathIsRefusedRatherThanWritten()
        {
            Assert.Throws<ArgumentNullException>(
                delegate { new WorkbookWriter().Write(null, "x.xlsx"); });
            Assert.Throws<ArgumentException>(
                delegate { new WorkbookWriter().Write(Report(), string.Empty); });
        }

        [Test]
        public void AFolderThatIsNotThereYetIsMadeRatherThanRefused()
        {
            ClashReport report = Report();
            AddTest(report, "one", 1);

            string path = Path.Combine(folder, "not there yet", OutputName + ".xlsx");

            Assert.That(new WorkbookWriter().Write(report, path), Is.EqualTo(path));
            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void AReportWithNoTestsStillWritesASheet()
        {
            using (XLWorkbook workbook = new XLWorkbook(Write(Report())))
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
                Assert.That(workbook.Worksheets.Worksheet(1).Cell(1, 4).GetString(),
                    Is.EqualTo("Clash Report"));
            }
        }
    }
}
