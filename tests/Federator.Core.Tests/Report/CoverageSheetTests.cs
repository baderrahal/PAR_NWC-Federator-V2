using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Coverage;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The counts line of the COVERAGE block, CoverageSheet.Counts, F127, Bader's request 2 under Q112, and the sheets the
    /// workbook check allows the clash workbook, which holds the client's sheet alone. The Coverage sheet itself and the
    /// internal workbook it sat in are gone since F243, Bader's order of 19 Oct 2026 point 4, "no more internal ... files",
    /// and their tests with them. Every workbook here is a real xlsx the writer made, read back off the disk as a file.
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

        private static CoverageSheetData Data(IList<TestCoverage> tests)
        {
            return new CoverageSheetData(@"C:\picked\1104-PAR_CLASH_AllInOne.xml", "ab12", null, tests, null);
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

        [Test]
        public void TheCountsLineIsReadOffTheRowsAlone()
        {
            IList<TestCoverage> tests = new List<TestCoverage> { Ran(1, "T1", 2), Ran(2, "T2", 0), KeptOut(3, "T3") };

            Assert.That(CoverageSheet.Counts(Data(tests)), Is.EqualTo(
                "3 tests in the clash matrix: 2 created this run, 0 already there, 1 not created, 2 run, 1 with clashes, 1 without"));
        }

        // ---------- the workbook check and the sheets it allows ----------

        private string WorkbookWithSheets(params string[] names)
        {
            string path = Path.Combine(folder, "sheets-" + names.Length + ".xlsx");
            new WorkbookWriter().Write(Report("T1"), path);

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

        /// <summary>
        /// THE CLASH WORKBOOK HOLDS THE CLIENT'S SHEET ALONE AGAIN, Bader's answer B to Q146: it goes to the
        /// client and keeps the Navisworks export's layout, and since F243 no Coverage sheet is written anywhere.
        /// So a Coverage sheet second in the clash workbook is a problem, as any second sheet was before FR-200.
        /// </summary>
        [Test]
        public void ACoverageSheetSecondInTheClashWorkbookIsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "Coverage"));

            Assert.That(check.Sheets, Is.EqualTo(2));
            Assert.That(Problems(check), Does.Contain("The workbook has 2 sheets and the client's report has one"));
            Assert.That(Problems(check), Does.Contain("Coverage"));
        }

        [Test]
        public void ASecondSheetOfAnotherNameIsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "Summary"));

            Assert.That(Problems(check), Does.Contain("The workbook has 2 sheets and the client's report has one"));
            Assert.That(Problems(check), Does.Contain("Summary"));
        }

        [Test]
        public void ACoverageSheetInAnyLetterCaseIsASecondSheetAndAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "COVERAGE"));

            Assert.That(Problems(check), Does.Contain("The workbook has 2 sheets and the client's report has one"));
            Assert.That(Problems(check), Does.Contain("COVERAGE"));
        }

        [Test]
        public void ACoverageSheetFirstIsAProblemOfTheSheetCount()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("Coverage", "1104-PAR-100000-ZZZ-BM-RPT-0000"));

            Assert.That(check.SheetName, Is.EqualTo("Coverage"));
            Assert.That(Problems(check), Does.Contain("The workbook has 2 sheets and the client's report has one"));
        }

        [Test]
        public void AThirdSheetIsNamedAsAProblem()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000", "Coverage", "Generic Models"));

            Assert.That(Problems(check), Does.Contain("The workbook has 3 sheets and the client's report has one"));
            Assert.That(Problems(check), Does.Contain("Coverage, Generic Models"));
        }

        /// <summary>The good file, so the problem above is the second sheet and nothing else of the small report.</summary>
        [Test]
        public void TheClientsSheetAloneIsNoProblemOfTheSheets()
        {
            WorkbookCheck check = WorkbookCheck.Of(WorkbookWithSheets("1104-PAR-100000-ZZZ-BM-RPT-0000"));

            Assert.That(check.Ran, Is.True, check.CouldNotRead);
            Assert.That(check.Sheets, Is.EqualTo(1));

            foreach (string problem in check.Problems)
            {
                Assert.That(problem, Does.Not.Contain("sheet").IgnoreCase, problem);
            }
        }
    }
}
