using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Health;
using Federator.Core.Report;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The Coverage sheet, F127, Bader's request 2 under Q112 and his decision under Q46,
    /// FR-200: the second and last sheet of the group's workbook, one row per test of the
    /// picked file in the file's order, a test F77 did not create included, with its reason.
    /// Every workbook here is a real xlsx the writer made, read back off the disk as a file.
    /// </summary>
    [TestFixture]
    public class CoverageSheetTests
    {
        private const string Root = "lcop_selection_set_tree";
        private const string OutputName = "1104-PAR-100000-ZZZ-BM-RPT-000001";

        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorCoverageSheet");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        // ---------- what is handed in ----------

        private static TestCoverage Ran(int position, string name, int clashes)
        {
            return new TestCoverage(position, name, Root + "/Architecture/BLD-AR-Walls", Root + "/Architecture/BLD-AR-Floors",
                2, 2, TestPresence.CreatedThisRun, true, clashes,
                clashes > 0 ? CoverageReason.HasClashes : CoverageReason.RanAndFoundNone, string.Empty);
        }

        /// <summary>A test F77 kept out: not in the document and never run, a side's set found nothing.</summary>
        private static TestCoverage KeptOut(int position, string name)
        {
            return new TestCoverage(position, name, Root + "/Architecture/BLD-AR-Stairs", Root + "/Architecture/BLD-AR-Floors",
                0, 2, TestPresence.NotInDocument, false, -1, CoverageReason.SideFoundNothing,
                "left " + Root + "/Architecture/BLD-AR-Stairs 0 items");
        }

        private static CoverageSheetData Data(IList<TestCoverage> tests, CountCheck check, IList<SetResult> sets)
        {
            return new CoverageSheetData(
                @"C:\picked\1104-PAR_CLASH_AllInOne.xml",
                "ab12",
                null,
                new List<ModelExport> { new ModelExport("1104-PAR-100000-ZZZ-AR-MOD-003000.nwc", "AR", 53, 53, 53, new List<string>()) },
                tests,
                check,
                sets,
                null);
        }

        private ClashReport Report(params string[] testsWithTwoRows)
        {
            ClashReport report = new ClashReport("100000", OutputName);
            report.SetTreeRoot = Root;
            report.DocumentUnits = "m";
            report.RunAt = new DateTime(2026, 10, 7, 15, 0, 0);

            foreach (string name in testsWithTwoRows)
            {
                WorkbookTestsTests.AddTest(report, name, 2, 0);
            }

            return report;
        }

        private string Write(ClashReport report, CoverageSheetData coverage)
        {
            return new WorkbookWriter(new ReportOptions()).Write(
                report, Path.Combine(folder, OutputName + ".xlsx"), coverage, new CoverageSettings());
        }

        /// <summary>The TESTS rows read back off the file: the row of each test by its name in column B, as cell texts.</summary>
        private static Dictionary<string, string[]> TestRowsOnDisk(string path)
        {
            Dictionary<string, string[]> rows = new Dictionary<string, string[]>(StringComparer.Ordinal);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(2);
                int last = sheet.LastRowUsed().RowNumber();
                bool inTests = false;

                for (int row = 1; row <= last; row++)
                {
                    string first = sheet.Cell(row, 1).GetString();

                    if (first == CoverageSheet.TestsSection)
                    {
                        inTests = true;
                        row++;
                        continue;
                    }

                    if (first == CoverageSheet.SetsSection)
                    {
                        break;
                    }

                    if (!inTests || first.Length == 0)
                    {
                        continue;
                    }

                    string[] cells = new string[CoverageSheet.TestHeadings.Length];

                    for (int column = 1; column <= cells.Length; column++)
                    {
                        cells[column - 1] = sheet.Cell(row, column).GetString();
                    }

                    rows[cells[1]] = cells;
                }
            }

            return rows;
        }

        // ---------- FR-200 ----------

        /// <summary>
        /// A TEST THE PLAN DID NOT CREATE HAS A ROW ON THE SHEET WITH ITS REASON, FR-200, Bader's
        /// decision under Q46: F77 keeps a test whose side finds nothing out of the document on
        /// every run, and the sheet names every such test and why, so nothing is missed without
        /// a line saying so. The row says not created, no, and the side's set that found nothing.
        /// </summary>
        [Test]
        public void ATestNotCreatedHasARowOnTheSheetWithItsReason()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2), KeptOut(2, "BLD-AR-Stairs-vs-BLD-AR-Floors") };
            string path = Write(Report("T1"), Data(tests, null, null));

            Dictionary<string, string[]> rows = TestRowsOnDisk(path);

            Assert.That(rows.Count, Is.EqualTo(2), "one row per test of the file, the one not created included");
            Assert.That(rows.ContainsKey("BLD-AR-Stairs-vs-BLD-AR-Floors"), Is.True, "the test F77 kept out is on the sheet");

            string[] keptOut = rows["BLD-AR-Stairs-vs-BLD-AR-Floors"];
            Assert.That(keptOut[0], Is.EqualTo("2"), "its position in the file");
            Assert.That(keptOut[7], Is.EqualTo(CoverageWords.For(TestPresence.NotInDocument)));
            Assert.That(keptOut[8], Is.EqualTo("no"));
            Assert.That(keptOut[14], Is.EqualTo(CoverageWords.For(CoverageReason.SideFoundNothing)));
            Assert.That(keptOut[15], Does.Contain("BLD-AR-Stairs 0 items"));
        }

        /// <summary>The rows the writer puts on the sheet are TestRows, one shape, so a check reads what was written.</summary>
        [Test]
        public void TheRowsOnDiskAreTheRowsTheRuleGives()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2), KeptOut(2, "T2") };
            CoverageSheetData data = Data(tests, null, null);
            string path = Write(Report("T1"), data);

            Dictionary<string, string[]> onDisk = TestRowsOnDisk(path);
            IList<string[]> given = CoverageSheet.TestRows(data);

            Assert.That(given.Count, Is.EqualTo(2));

            foreach (string[] row in given)
            {
                Assert.That(onDisk[row[1]], Is.EqualTo(row), row[1]);
            }
        }

        // ---------- where it sits ----------

        [Test]
        public void TheSheetIsSecondAndLastAndNamedCoverageAndTheClientsSheetIsFirst()
        {
            string path = Write(Report("T1"), Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                List<string> names = new List<string>();

                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    names.Add(sheet.Name);
                }

                Assert.That(names.Count, Is.EqualTo(2));
                Assert.That(names[0], Is.EqualTo(OutputName.Substring(0, 31)), "the client's sheet first");
                Assert.That(names[1], Is.EqualTo(CoverageSettings.DefaultSheetName));
            }

            // The check's cell rules read the rows of this small report too, so only its sheet
            // rules are asserted here: the Coverage sheet second is no problem of the sheets.
            WorkbookCheck check = WorkbookCheck.Of(path);
            Assert.That(check.Ran, Is.True, check.CouldNotRead);
            Assert.That(check.HasCoverageSheet, Is.True);
            Assert.That(check.Sheets, Is.EqualTo(2));

            foreach (string problem in check.Problems)
            {
                Assert.That(problem, Does.Not.Contain("sheet").IgnoreCase, problem);
            }
        }

        /// <summary>Nothing of ours goes on sheet 1: the client's sheet reads cell for cell as it does with no Coverage sheet.</summary>
        [Test]
        public void TheClientsSheetIsTheSameCellForCellWithAndWithoutTheCoverageSheet()
        {
            string with = Write(Report("T1", "T2"), Data(new List<TestCoverage> { Ran(1, "T1", 2), Ran(2, "T2", 2) }, null, null));
            string without = new WorkbookWriter(new ReportOptions()).Write(Report("T1", "T2"), Path.Combine(folder, "without.xlsx"));

            using (XLWorkbook a = new XLWorkbook(with))
            using (XLWorkbook b = new XLWorkbook(without))
            {
                IXLWorksheet first = a.Worksheet(1);
                IXLWorksheet second = b.Worksheet(1);
                int rows = first.LastRowUsed().RowNumber();
                int columns = first.LastColumnUsed().ColumnNumber();

                Assert.That(second.LastRowUsed().RowNumber(), Is.EqualTo(rows));
                Assert.That(second.LastColumnUsed().ColumnNumber(), Is.EqualTo(columns));

                for (int row = 1; row <= rows; row++)
                {
                    for (int column = 1; column <= columns; column++)
                    {
                        Assert.That(first.Cell(row, column).GetString(), Is.EqualTo(second.Cell(row, column).GetString()),
                            "row " + row + " column " + column);
                    }
                }
            }
        }

        [Test]
        public void NoCoverageHandedInWritesTheClientsSheetAlone()
        {
            string path = Write(Report("T1"), null);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
            }

            Assert.That(WorkbookCheck.Of(path).HasCoverageSheet, Is.False);
        }

        /// <summary>A report whose own sheet would be named like the Coverage sheet gets no Coverage sheet, and the rule says why.</summary>
        [Test]
        public void AReportNamedCoverageGetsNoCoverageSheetAndTheRuleSaysWhy()
        {
            ClashReport report = new ClashReport("100000", "Coverage");
            report.DocumentUnits = "m";
            WorkbookTestsTests.AddTest(report, "T1", 2, 0);

            string why = CoverageSheet.WhyRefused(report, CoverageSettings.DefaultSheetName);
            Assert.That(why, Does.Contain("no Coverage sheet was written"));
            Assert.That(CoverageSheet.WhyRefused(Report("T1"), CoverageSettings.DefaultSheetName), Is.Null);

            string path = new WorkbookWriter(new ReportOptions()).Write(
                report, Path.Combine(folder, "coverage-named.xlsx"), Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null), new CoverageSettings());

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
            }
        }

        // ---------- the cells ----------

        /// <summary>A number nobody took is the word UNKNOWN, never an empty cell and never nought.</summary>
        [Test]
        public void ACountNotTakenReadsUnknownAndNeverNought()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { KeptOut(1, "T2") };
            IList<string[]> rows = CoverageSheet.TestRows(Data(tests, null, null));

            Assert.That(rows[0][4], Is.EqualTo("0"), "the left side was counted at nought");
            Assert.That(rows[0][9], Is.EqualTo(CoverageSheet.Unknown), "Clash Detective's count was not taken");
            Assert.That(rows[0][11], Is.EqualTo(CoverageSheet.Unknown), "the workbook's rows were not read");
            Assert.That(rows[0][13], Does.StartWith("NOT COMPARED"));

            foreach (string cell in rows[0])
            {
                Assert.That(cell, Is.Not.Empty, "no empty cell");
            }
        }

        /// <summary>The count check's four numbers and its verdict ride beside the test where the check holds it.</summary>
        [Test]
        public void TheCountChecksNumbersAndVerdictRideBesideTheTest()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2) };
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(Report("T1"), folder));
            CountCheck agree = CountCheck.Judge(tests, new List<DocumentTestCount> { new DocumentTestCount("T1", "Tests", 2, 2) }, workbook, null, -1);
            CountCheck failed = CountCheck.Judge(tests, new List<DocumentTestCount> { new DocumentTestCount("T1", "Tests", 3, 3) }, workbook, null, -1);

            string[] agreeing = CoverageSheet.TestRows(Data(tests, agree, null))[0];
            Assert.That(agreeing[9], Is.EqualTo("2"));
            Assert.That(agreeing[10], Is.EqualTo("2"));
            Assert.That(agreeing[11], Is.EqualTo("2"));
            Assert.That(agreeing[12], Is.EqualTo("2"));
            Assert.That(agreeing[13], Is.EqualTo("AGREE"));

            string[] failing = CoverageSheet.TestRows(Data(tests, failed, null))[0];
            Assert.That(failing[9], Is.EqualTo("3"));
            Assert.That(failing[13], Does.StartWith("FAILED"));
        }

        [Test]
        public void TheCountsLineIsReadOffTheRowsAlone()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2), Ran(2, "T2", 0), KeptOut(3, "T3") };

            Assert.That(CoverageSheet.Counts(Data(tests, null, null)), Is.EqualTo(
                "3 tests in the picked file: 2 created this run, 0 already there, 1 not created, 2 run, 1 with clashes, 1 without"));
        }

        [Test]
        public void TheSetsSectionSaysUnknownWhereNoSetWasHandedInAndNamesEachSetOtherwise()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2) };

            Assert.That(CoverageSheet.SetRows(Data(tests, null, null))[0][0], Does.StartWith("UNKNOWN"));

            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("a/BLD-AR-Walls", "BLD-AR-Walls", 1, 36, "asks");
            outcome.AddCreated("a/BLD-AR-Stairs", "BLD-AR-Stairs", 2, 0, "asks");

            IList<string[]> rows = CoverageSheet.SetRows(Data(tests, null, new List<SetResult>(outcome.Results)));
            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0], Is.EqualTo(new[] { "a/BLD-AR-Walls", "1", "36", "no" }));
            Assert.That(rows[1], Is.EqualTo(new[] { "a/BLD-AR-Stairs", "2", "0", "yes" }));
        }

        // ---------- the workbook check and the sheets it allows ----------

        private string WorkbookWithSheets(params string[] names)
        {
            string path = Path.Combine(folder, "sheets-" + names.Length + ".xlsx");
            new WorkbookWriter(new ReportOptions()).Write(Report("T1"), path);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (i == 0)
                    {
                        workbook.Worksheet(1).Name = names[0];
                        continue;
                    }

                    workbook.Worksheets.Add(names[i]);
                }

                workbook.Save();
            }

            return path;
        }

        private static string Problems(WorkbookCheck check)
        {
            return string.Join("\n", new List<string>(check.Problems).ToArray());
        }

        [Test]
        public void ASecondSheetThatIsNotTheCoverageSheetIsNamedAsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "Summary"));

            Assert.That(check.HasCoverageSheet, Is.False);
            Assert.That(Problems(check), Does.Contain("second sheet named Summary"));
        }

        [Test]
        public void ACoverageSheetDifferingByLetterCaseIsNamedAsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "COVERAGE"));

            Assert.That(check.HasCoverageSheet, Is.False);
            Assert.That(Problems(check), Does.Contain("differs by letter case alone"));
        }

        [Test]
        public void ACoverageSheetFirstIsNamedAsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("Coverage", "1104-PAR-100000-ZZZ-BM-RPT-0000"));

            Assert.That(Problems(check), Does.Contain("The Coverage sheet comes first"));
        }

        [Test]
        public void AThirdSheetIsNamedAsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "Coverage", "Generic Models"));

            Assert.That(check.HasCoverageSheet, Is.True, "the Coverage sheet is still second");
            Assert.That(Problems(check), Does.Contain("at most two"));
            Assert.That(Problems(check), Does.Contain("Generic Models"));
        }
    }
}
