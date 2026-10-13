using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Generic;
using Federator.Core.Health;
using Federator.Core.Report;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The Coverage sheet, F127, Bader's request 2 under Q112 and his decision under Q46,
    /// FR-200: one row per test of the picked file in the file's order, a test F77 did not
    /// create included, with its reason. Since Bader's answer B to Q146 a sheet of the internal
    /// workbook beside the clash workbook, after the Generic Models sheet, and the clash workbook
    /// holds the client's sheet alone. Every workbook here is a real xlsx the writer made, read
    /// back off the disk as a file.
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

        /// <summary>A Generic Models count of two models, one with items, for the sheet beside the Coverage sheet.</summary>
        private static GenericModelsReport Generic()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new List<GenericModelInput> { new GenericModelInput("model0.nwc"), new GenericModelInput("model1.nwc") },
                new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated(plan.Sets[0].Set, 5, null);
            outcome.AddCreated(plan.Sets[1].Set, 0, null);
            return GenericModelsReport.From(plan, outcome.Results);
        }

        /// <summary>The internal workbook of the group, with the Generic Models sheet where a count is handed in.</summary>
        private string Write(GenericModelsReport generic, CoverageSheetData coverage)
        {
            string path = Path.Combine(folder, new GenericModelsSettings().WorkbookNameFor(OutputName) + ".xlsx");
            InternalWorkbook.Write(path, "100000", generic, GenericModelsSettings.DefaultSheetName, coverage, CoverageSettings.DefaultSheetName);
            return path;
        }

        private static List<string> SheetsOf(string path)
        {
            List<string> names = new List<string>();

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    names.Add(sheet.Name);
                }
            }

            return names;
        }

        /// <summary>The TESTS rows read back off the file: the row of each test by its name in column B, as cell texts.</summary>
        private static Dictionary<string, string[]> TestRowsOnDisk(string path)
        {
            Dictionary<string, string[]> rows = new Dictionary<string, string[]>(StringComparer.Ordinal);

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(CoverageSettings.DefaultSheetName);
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
            string path = Write((GenericModelsReport)null, Data(tests, null, null));

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
            string path = Write((GenericModelsReport)null, data);

            Dictionary<string, string[]> onDisk = TestRowsOnDisk(path);
            IList<string[]> given = CoverageSheet.TestRows(data);

            Assert.That(given.Count, Is.EqualTo(2));

            foreach (string[] row in given)
            {
                Assert.That(onDisk[row[1]], Is.EqualTo(row), row[1]);
            }
        }

        // ---------- where it sits, Q146 answered B ----------

        /// <summary>
        /// THE COVERAGE SHEET IS IN THE INTERNAL WORKBOOK BESIDE THE GENERIC MODELS SHEET, Bader's answer B to
        /// Q146, and the clash workbook of the same group holds the client's sheet alone, the Navisworks
        /// export's layout. Both sheets in one save, named as the settings name them, Generic Models first.
        /// </summary>
        [Test]
        public void TheCoverageSheetIsInTheInternalWorkbookAndTheClashWorkbookHoldsTheClientsSheetAlone()
        {
            string clash = new WorkbookWriter().Write(Report("T1"), Path.Combine(folder, OutputName + ".xlsx"));
            string path = Path.Combine(folder, new GenericModelsSettings().WorkbookNameFor(OutputName) + ".xlsx");
            IList<string> written = InternalWorkbook.Write(
                path, "100000", Generic(), GenericModelsSettings.DefaultSheetName,
                Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null), CoverageSettings.DefaultSheetName);

            Assert.That(path, Does.EndWith(OutputName + " Internal.xlsx"));
            Assert.That(written, Is.EqualTo(new[] { GenericModelsSettings.DefaultSheetName, CoverageSettings.DefaultSheetName }));
            Assert.That(SheetsOf(path), Is.EqualTo(written), "both sheets in the one file, in the order written");
            Assert.That(SheetsOf(clash), Is.EqualTo(new[] { OutputName.Substring(0, 31) }), "the clash workbook holds the client's sheet alone");

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                Assert.That(workbook.Worksheet(CoverageSettings.DefaultSheetName).Cell(1, 1).GetString(), Is.EqualTo(CoverageSheet.Title));
                Assert.That(workbook.Worksheet(GenericModelsSettings.DefaultSheetName).Cell(1, 1).GetString(), Is.EqualTo(GenericSheet.Title));
            }

            Assert.That(WorkbookCheck.Of(clash).Sheets, Is.EqualTo(1));
        }

        /// <summary>A group with a coverage and no Generic Models count writes the workbook with the Coverage sheet alone.</summary>
        [Test]
        public void ACoverageAndNoGenericModelsCountWritesTheCoverageSheetAlone()
        {
            string path = Write((GenericModelsReport)null, Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null));

            Assert.That(SheetsOf(path), Is.EqualTo(new[] { CoverageSettings.DefaultSheetName }));
        }

        /// <summary>A group with a Generic Models count and no coverage writes the workbook with the Generic Models sheet alone.</summary>
        [Test]
        public void AGenericModelsCountAndNoCoverageWritesTheGenericModelsSheetAlone()
        {
            string path = Write(Generic(), null);

            Assert.That(SheetsOf(path), Is.EqualTo(new[] { GenericModelsSettings.DefaultSheetName }));
        }

        /// <summary>A group with neither is refused, so the caller writes no workbook and says why.</summary>
        [Test]
        public void NeitherACountNorACoverageIsRefusedAndNoFileIsWritten()
        {
            string path = Path.Combine(folder, "neither.xlsx");

            Assert.Throws<ArgumentException>(() => InternalWorkbook.Write(path, "100000", null, "Generic Models", null, "Coverage"));
            Assert.That(File.Exists(path), Is.False);
            Assert.Throws<ArgumentException>(() => InternalWorkbook.Write(string.Empty, "100000", Generic(), "Generic Models", null, "Coverage"));
        }

        /// <summary>
        /// Two sheets Excel reads as one name: the Generic Models sheet is left out by GenericSheet's one rule
        /// and the Coverage sheet keeps its name, and the rule says why for the log.
        /// </summary>
        [Test]
        public void AGenericModelsSheetNamedLikeTheCoverageSheetIsLeftOutAndTheRuleSaysWhy()
        {
            CoverageSheetData data = Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null);

            Assert.That(InternalWorkbook.WhyNoGenericSheet(data, "coverage", "Coverage"), Does.Contain("no Generic Models sheet was written"));
            Assert.That(InternalWorkbook.WhyNoGenericSheet(data, "Generic Models", "Coverage"), Is.Null);
            Assert.That(InternalWorkbook.WhyNoGenericSheet(null, "Coverage", "Coverage"), Is.Null, "no Coverage sheet to meet");

            string path = Path.Combine(folder, "one-name.xlsx");
            IList<string> written = InternalWorkbook.Write(path, "100000", Generic(), "coverage", data, "Coverage");

            Assert.That(written, Is.EqualTo(new[] { "Coverage" }));
            Assert.That(SheetsOf(path), Is.EqualTo(new[] { "Coverage" }));
        }

        /// <summary>The check reads both sheets back by name and says so, the good file.</summary>
        [Test]
        public void TheCheckReadsBothSheetsBackByName()
        {
            string path = Write(Generic(), Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null));
            IList<string> lines = InternalWorkbook.Check(path, SheetsOf(path));

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Is.EqualTo("read back, it holds 2 sheets, Generic Models, Coverage, each found by the name it was written under"));
        }

        /// <summary>The break: a sheet written and not in the file, and one in the file not written, are each named.</summary>
        [Test]
        public void TheCheckNamesASheetMissingAndASheetNotWritten()
        {
            string path = Write(Generic(), Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null));

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                workbook.Worksheet(CoverageSettings.DefaultSheetName).Delete();
                workbook.Worksheets.Add("Summary");
                workbook.Save();
            }

            string lines = string.Join(" | ", new List<string>(InternalWorkbook.Check(
                path, new List<string> { GenericModelsSettings.DefaultSheetName, CoverageSettings.DefaultSheetName })).ToArray());

            Assert.That(lines, Does.Contain("the sheet Coverage was written and the workbook read back holds no sheet of that name"));
            Assert.That(lines, Does.Contain("holds a sheet Summary this run did not write"));
            Assert.That(lines, Does.Not.Contain("read back, it holds"));
        }

        /// <summary>A file that will not open is said with what it threw, and nothing throws out of the check.</summary>
        [Test]
        public void TheCheckSaysAFileThatWillNotOpen()
        {
            IList<string> lines = InternalWorkbook.Check(Path.Combine(folder, "not-there.xlsx"), new List<string> { "Coverage" });

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.StartWith("the internal workbook could not be read back, "));
        }

        /// <summary>The MIRRORED PAIRS section never says F132's rule is missing, since it is on main.</summary>
        [Test]
        public void TheMirroredPairsSectionSaysWhereThePairsAreNamed()
        {
            string path = Write((GenericModelsReport)null, Data(new List<TestCoverage> { Ran(1, "T1", 2) }, null, null));
            bool found = false;

            using (XLWorkbook workbook = new XLWorkbook(path))
            {
                IXLWorksheet sheet = workbook.Worksheet(CoverageSettings.DefaultSheetName);

                for (int row = 1; row <= sheet.LastRowUsed().RowNumber(); row++)
                {
                    if (sheet.Cell(row, 1).GetString() == CoverageSheet.MirrorsSection)
                    {
                        Assert.That(sheet.Cell(row + 1, 1).GetString(), Is.EqualTo(CoverageSheet.MirrorsUnknown));
                        Assert.That(sheet.Cell(row + 1, 1).GetString(), Does.Not.Contain("not on main"));
                        found = true;
                    }
                }
            }

            Assert.That(found, Is.True);
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
        /// client and keeps the Navisworks export's layout, and the Coverage sheet is in the internal workbook
        /// beside it. So a Coverage sheet second in the clash workbook is a problem, as any second sheet was
        /// before FR-200.
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
