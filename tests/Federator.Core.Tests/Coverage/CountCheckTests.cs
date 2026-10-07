using System;
using System.Collections.Generic;
using Federator.Core.Coverage;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The clash count in Clash Detective against the workbook rows, F127, Bader's request 2
    /// and his answer to the lead's notes under Q112: a count that differs is a FAILED line
    /// in COVERAGE and RESULT, and the group keeps its own result. These are F104's checks 1
    /// and 2 restated in Core: the rows under a block against the document's results at the
    /// top level, and the Clashes cell against every clash in the test. Exact, and a count
    /// of minus one is never zero. compare-document.ps1 stays the independent witness,
    /// because a check that shares its reading of the document with the harvest cannot
    /// catch a fault common to both.
    /// </summary>
    [TestFixture]
    public class CountCheckTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorCountCheck");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        // ---------- what is handed in ----------

        private static TestCoverage Ran(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 2, 2, TestPresence.CreatedThisRun,
                true, 0, CoverageReason.RanAndFoundNone, string.Empty);
        }

        private static TestCoverage NotRun(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 0, 2, TestPresence.AlreadyThere,
                false, -1, CoverageReason.SideFoundNothing, string.Empty);
        }

        /// <summary>A test F77 kept out: not in the document and never run.</summary>
        private static TestCoverage KeptOut(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 0, 2, TestPresence.NotInDocument,
                false, -1, CoverageReason.SideFoundNothing, string.Empty);
        }

        /// <summary>A test the clash step recorded as Failed, with what it threw.</summary>
        private static TestCoverage Threw(string name, TestPresence presence, string what)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 2, 2, presence,
                false, -1, CoverageReason.Failed, what);
        }

        /// <summary>A test created this run and not run, its clash skipped by the coordinates rule.</summary>
        private static TestCoverage CreatedNotRun(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 2, 2, TestPresence.CreatedThisRun,
                false, -1, CoverageReason.CoordinatesRule, string.Empty);
        }

        private static IList<TestCoverage> Tests(params TestCoverage[] tests)
        {
            return new List<TestCoverage>(tests);
        }

        private static IList<DocumentTestCount> Document(params DocumentTestCount[] tests)
        {
            return new List<DocumentTestCount>(tests);
        }

        private static DocumentTestCount InDocument(string name, int topLevel, int leaves)
        {
            return new DocumentTestCount(name, "Tests", topLevel, leaves);
        }

        /// <summary>A real workbook with these tests, each name, plain rows, clashes in one group.</summary>
        private WorkbookTests Workbook(params object[] nameRowsGrouped)
        {
            ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            report.DocumentUnits = "m";

            for (int i = 0; i < nameRowsGrouped.Length; i += 3)
            {
                WorkbookTestsTests.AddTest(report, (string)nameRowsGrouped[i], (int)nameRowsGrouped[i + 1],
                    (int)nameRowsGrouped[i + 2]);
            }

            return WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(report, folder));
        }

        private static CountedTest Judged(CountCheck check, string name)
        {
            foreach (CountedTest test in check.Tests)
            {
                if (test.Name == name)
                {
                    return test;
                }
            }

            throw new InvalidOperationException(name + " was not judged");
        }

        private static CountCheck Judge(IList<TestCoverage> tests, IList<DocumentTestCount> document, WorkbookTests workbook)
        {
            return CountCheck.Judge(tests, document, workbook, null, -1);
        }

        // ---------- agree ----------

        [Test]
        public void RowsAndClashesEqualToTheDocumentAgree()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), Workbook("T", 2, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.Agree));
            Assert.That(check.FailedLines("100000"), Is.Empty);
        }

        /// <summary>A result group is one row and one result at the top level, holding the clashes under it.</summary>
        [Test]
        public void AResultGroupAgreesOnThreeRowsThreeAtTheTopAndSevenClashes()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 3, 7)), Workbook("T", 2, 5));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.Agree));
        }

        /// <summary>
        /// F77's tests not created: not in the document, and one row reading nought. Neither side
        /// holds the test, so nothing was set beside Clash Detective and it is not counted as agreeing,
        /// the breaker's reading of lane B's first attempt, since 1794 of 1830 such tests under a
        /// headline of all agree read as a verification that never happened.
        /// </summary>
        [Test]
        public void ATestNeitherSideHoldsIsHeldByNeitherAndNeverAgrees()
        {
            CountCheck check = Judge(Tests(KeptOut("T")), Document(), Workbook("T", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.HeldByNeither));
            Assert.That(check.CountOf(CountVerdict.Agree), Is.EqualTo(0));
            Assert.That(check.CountOf(CountVerdict.HeldByNeither), Is.EqualTo(1));
            Assert.That(check.FailedLines("100000"), Is.Empty);
        }

        /// <summary>
        /// A name on two tests of the picked file was judged twice against the one test of the
        /// document and the one block of the workbook, so the totals counted a test that does not exist.
        /// Both rows are not compared, as a name on two tests of either other side already is.
        /// </summary>
        [Test]
        public void ANameOnTwoTestsOfThePickedFileIsNotComparedTwice()
        {
            CountCheck check = Judge(Tests(Ran("T"), Ran("T")), Document(InDocument("T", 2, 2)), Workbook("T", 2, 0));

            Assert.That(check.Tests.Count, Is.EqualTo(2));
            Assert.That(check.CountOf(CountVerdict.Agree), Is.EqualTo(0));
            Assert.That(check.CountOf(CountVerdict.NotCompared), Is.EqualTo(2));
            Assert.That(check.Tests[0].Why, Does.Contain("the name is on 2 tests of the picked file"));
            Assert.That(check.FailedLines("100000"), Is.Empty);
        }

        /// <summary>The line of a test that threw is one line, whatever the message held.</summary>
        [Test]
        public void AFailedLineIsOneLineWhateverTheExceptionMessageHeld()
        {
            CountCheck check = Judge(
                Tests(Threw("T", TestPresence.CreatedThisRun, "ArgumentException: bad value\r\nParameter name: x")),
                Document(InDocument("T", 3, 3)),
                Workbook("T", 1, 0));

            IList<string> lines = check.FailedLines("100000");

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.Not.Contain("\n"));
            Assert.That(lines[0], Does.Not.Contain("\r"));
            Assert.That(lines[0], Does.Contain("bad value Parameter name: x"));
        }

        /// <summary>
        /// The breaker's reading of attempt 1. A test the runner holds as created this run
        /// or already there that the read of Clash Detective does not return has gone from
        /// the document, a walk that stopped early or a name Navisworks changed. Its block
        /// reading nought matches nothing, so it is never Agree and says why.
        /// </summary>
        [Test]
        public void ATestTheRunHoldsAsPresentThatTheDocumentDidNotReturnIsNotComparedNeverAgree()
        {
            CountCheck check = Judge(Tests(Ran("T"), NotRun("U")), Document(), Workbook("T", 0, 0, "U", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(Judged(check, "T").Why, Does.Contain("created this run"));
            Assert.That(Judged(check, "U").Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(Judged(check, "U").Why, Does.Contain("already there"));
        }

        // ---------- failed, each by one number ----------

        [Test]
        public void ARowShortOfTheDocumentIsFailedAndTheLineNamesAllFourNumbers()
        {
            ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            WorkbookTestsTests.AddTest(report, "T", 3, 0);
            string path = WorkbookTestsTests.Write(report, folder);

            using (ClosedXML.Excel.XLWorkbook workbook = new ClosedXML.Excel.XLWorkbook(path))
            {
                ClosedXML.Excel.IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Row(9).Delete();
                workbook.Save();
            }

            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 3, 3)), WorkbookTestsTests.ReadBack(path));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.WorkbookRows, Is.EqualTo(2));

            string line = CountCheck.FailedLine("100000", test);

            Assert.That(line, Does.Contain("Clash Detective holds 3 results at the top level and 3 clashes"));
            Assert.That(line, Does.Contain("the workbook 2 rows and Clashes 3"));
        }

        [Test]
        public void AClashesCellOneMoreThanTheDocumentIsFailed()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), Workbook("T", 1, 2));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(Judged(check, "T").WorkbookClashes, Is.EqualTo(3));
        }

        [Test]
        public void NoBlockForATestTheDocumentHoldsIsFailed()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), Workbook("U", 0, 0));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(CountCheck.FailedLine("100000", test), Does.Contain("the workbook holds no block for it"));
        }

        [Test]
        public void ABlockWithNumbersForATestTheDocumentDoesNotHoldIsFailed()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(), Workbook("T", 2, 0));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(CountCheck.FailedLine("100000", test), Does.Contain("Clash Detective holds no test of that name"));
        }

        /// <summary>
        /// Decision 4, default A. A test not run this run whose NWF still holds an earlier
        /// run's results, while its block reads less, is FAILED and the line says why.
        /// </summary>
        [Test]
        public void ATestNotRunWhoseNwfHoldsEarlierResultsIsFailedAndSaysWhy()
        {
            CountCheck check = Judge(Tests(NotRun("T")), Document(InDocument("T", 4, 4)), Workbook("T", 0, 0));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.Why, Does.Contain("not run this run"));
            Assert.That(CountCheck.FailedLine("100000", test), Does.Contain(test.Why));
        }

        [Test]
        public void ResultsCompactRemovedAfterTheHarvestAreFailedAndTheLineNamesCompact()
        {
            CountCheck check = CountCheck.Judge(Tests(Ran("T")), Document(InDocument("T", 1, 1)),
                Workbook("T", 3, 0), null, 2);
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.Why, Does.Contain("Compact"));
            Assert.That(test.Why, Does.Contain("which could account for this test's gap of 2"));
            Assert.That(test.Why, Does.Contain("which test they were in is not recorded"));
            Assert.That(test.Why, Does.Not.Contain("as many as or more than"));
        }

        /// <summary>
        /// The breaker's reading of attempt 1. Compact's count is the group's, so it explains
        /// a test's gap only where it is at least that gap. A test whose workbook reads two
        /// clashes more than the document beside a Compact of one is never read as explained.
        /// </summary>
        [Test]
        public void CompactIsNeverGivenAsTheCauseOfAGapBiggerThanWhatItRemoved()
        {
            CountCheck check = CountCheck.Judge(Tests(Ran("T")), Document(InDocument("T", 1, 1)),
                Workbook("T", 3, 0), null, 1);
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.Why, Does.Contain("fewer than this test's gap of 2"));
            Assert.That(test.Why, Does.Not.Contain("after the workbook's rows were read, as many"));
        }

        /// <summary>
        /// The breaker's blocking finding on attempt 1. A test this run created and ran,
        /// whose harvest or count then threw, is recorded Failed and never as ran. Clash
        /// Detective holds its results and the workbook none. The line must never say it was
        /// not run, or that the NWF holds an earlier run's results, because a test created
        /// this run holds none, and it carries what was thrown.
        /// </summary>
        [Test]
        public void ATestCreatedAndRunWhoseRowsOrCountThenThrewIsNeverSaidToBeNotRun()
        {
            CountCheck check = Judge(
                Tests(Threw("T", TestPresence.CreatedThisRun, "InvalidOperationException: the harvest broke")),
                Document(InDocument("T", 40, 40)), Workbook("T", 0, 0));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.Why, Does.Not.Contain("not run this run"));
            Assert.That(test.Why, Does.Not.Contain("earlier run"));
            Assert.That(test.Why, Does.Contain("ran this run"));
            Assert.That(test.Why, Does.Contain("the harvest broke"));
            Assert.That(CountCheck.FailedLine("100000", test), Does.Contain(test.Why));
        }

        /// <summary>
        /// A test already in the NWF that threw: whether its results are this run's or an
        /// earlier run's cannot be told off the record, so it says UNKNOWN and never names
        /// the weekly run as the cause.
        /// </summary>
        [Test]
        public void AnAlreadyThereTestThatThrewSaysWhetherItRanIsUnknown()
        {
            CountCheck check = Judge(
                Tests(Threw("T", TestPresence.AlreadyThere, "COMException: the test is busy")),
                Document(InDocument("T", 4, 4)), Workbook("T", 0, 0));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.Why, Does.Contain("UNKNOWN"));
            Assert.That(test.Why, Does.Not.Contain("not run this run"));
            Assert.That(test.Why, Does.Contain("the test is busy"));
        }

        /// <summary>
        /// The weekly cause belongs to a test already in the NWF alone. A test created this
        /// run and skipped by the coordinates rule holds no earlier run's results, so a
        /// difference there is said with no cause rather than the wrong one.
        /// </summary>
        [Test]
        public void ATestCreatedThisRunAndNotRunIsNeverSaidToHoldAnEarlierRunsResults()
        {
            CountCheck check = Judge(Tests(CreatedNotRun("T")), Document(InDocument("T", 2, 2)), Workbook("T", 0, 0));
            CountedTest test = Judged(check, "T");

            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(test.Why, Does.Not.Contain("earlier run"));
        }

        // ---------- not compared, and never agree ----------

        [Test]
        public void ACountOfMinusOneInTheDocumentIsNotComparedAndNeverAgrees()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", -1, -1)), Workbook("T", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
        }

        [Test]
        public void ALeafCountOfMinusOneAloneIsNotComparedToo()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 0, -1)), Workbook("T", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
        }

        /// <summary>
        /// The reviewer's blocking finding on attempt 1: minus one on the workbook's side. A
        /// real xlsx with one Clashes cell broken reads minus one there, and the count check
        /// says its block could not be read whole, NOT COMPARED, and never a FAILED line
        /// against the document's two.
        /// </summary>
        [Test]
        public void ABlockWhoseClashesCellIsNoWholeNumberIsNotComparedAndNeverFailed()
        {
            ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            WorkbookTestsTests.AddTest(report, "T", 2, 0);
            string path = WorkbookTestsTests.Write(report, folder);

            using (ClosedXML.Excel.XLWorkbook workbook = new ClosedXML.Excel.XLWorkbook(path))
            {
                ClosedXML.Excel.IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Cell(RowOf(sheet, "T") + 1, WorkbookWriter.ColumnTestHeader + 1).Value = "two";
                workbook.Save();
            }

            WorkbookTests read = WorkbookTestsTests.ReadBack(path);
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), read);
            CountedTest test = Judged(check, "T");

            Assert.That(read.Named("T")[0].Clashes, Is.EqualTo(-1), "the broken cell reads minus one");
            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(test.Why, Does.Contain("could not be read whole"));
            Assert.That(check.FailedLines("100000"), Is.Empty);
        }

        /// <summary>The same for a full block whose clash table is gone, its rows read as minus one.</summary>
        [Test]
        public void ABlockWithNoClashTableUnderItIsNotComparedAndNeverFailed()
        {
            ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            WorkbookTestsTests.AddTest(report, "T", 2, 0);
            string path = WorkbookTestsTests.Write(report, folder);

            using (ClosedXML.Excel.XLWorkbook workbook = new ClosedXML.Excel.XLWorkbook(path))
            {
                ClosedXML.Excel.IXLWorksheet sheet = workbook.Worksheet(1);
                sheet.Cell(RowOf(sheet, "T") + 4, WorkbookWriter.ColumnClashName).Clear(ClosedXML.Excel.XLClearOptions.Contents);
                workbook.Save();
            }

            WorkbookTests read = WorkbookTestsTests.ReadBack(path);
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), read);
            CountedTest test = Judged(check, "T");

            Assert.That(read.Named("T")[0].Rows, Is.EqualTo(-1), "no clash table reads minus one rows");
            Assert.That(test.Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(test.Why, Does.Contain("could not be read whole"));
        }

        private static int RowOf(ClosedXML.Excel.IXLWorksheet sheet, string name)
        {
            int last = sheet.LastRowUsed().RowNumber();

            for (int row = 1; row <= last; row++)
            {
                if (sheet.Cell(row, 1).GetString() == name)
                {
                    return row;
                }
            }

            throw new InvalidOperationException(name + " is not on the sheet");
        }

        [Test]
        public void ANameOnTwoTestsOfTheDocumentIsNotCompared()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2), InDocument("T", 0, 0)),
                Workbook("T", 2, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(Judged(check, "T").Why, Does.Contain("2 tests of the document"));
        }

        [Test]
        public void ANameOnTwoTestsOfTheWorkbookIsNotCompared()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), Workbook("T", 2, 0, "T", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(Judged(check, "T").Why, Does.Contain("2 tests of the workbook"));
        }

        [Test]
        public void ATestInNeitherIsNotCompared()
        {
            CountCheck check = Judge(Tests(NotRun("T")), Document(), Workbook("U", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
        }

        /// <summary>
        /// A group with no workbook, a coordinates skip, outputs off or a refused unit, is
        /// not compared and says why, Q126's default A.
        /// </summary>
        [Test]
        public void NoWorkbookIsNotComparedAndTheReasonIsKept()
        {
            CountCheck check = CountCheck.Judge(Tests(Ran("T")), Document(InDocument("T", 2, 2)), null,
                "the clash was skipped by the coordinates rule", -1);

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(Judged(check, "T").Why, Does.Contain("the clash was skipped by the coordinates rule"));
        }

        [Test]
        public void TheDocumentNotReadIsNotCompared()
        {
            CountCheck check = Judge(Tests(Ran("T")), null, Workbook("T", 2, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.NotCompared));
        }

        [Test]
        public void TestsInTheDocumentTheXmlDoesNotHoldAreNamedAndNotJudged()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 0, 0), InDocument("Old", 9, 9)),
                Workbook("T", 0, 0));

            Assert.That(check.Tests.Count, Is.EqualTo(1));
            Assert.That(check.NotInTheXml.Count, Is.EqualTo(1));
            Assert.That(check.NotInTheXml[0], Is.EqualTo("Old"));
        }

        // ---------- the FAILED line ----------

        /// <summary>
        /// It starts with COVERAGE so it is never read as a group's FAILED, and it says the
        /// group keeps its own result, his answer to the notes under Q112.
        /// </summary>
        [Test]
        public void TheFailedLineIsTheCoverageLineAndSaysTheGroupKeepsItsResult()
        {
            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 1, 1)), Workbook("T", 2, 0));
            IList<string> lines = check.FailedLines("100000");

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.StartWith("COVERAGE FAILED  100000  T  "));
            Assert.That(lines[0], Does.Contain("Clash Detective holds 1 result at the top level and 1 clash, "
                + "the workbook 2 rows and Clashes 2"));
            Assert.That(lines[0], Does.EndWith(". The group keeps its own result"));
        }

        [Test]
        public void OnlyTheFailedTestsGetALineInFileOrder()
        {
            CountCheck check = Judge(Tests(Ran("A"), Ran("B"), Ran("C")),
                Document(InDocument("A", 1, 1), InDocument("B", 2, 2), InDocument("C", 3, 3)),
                Workbook("A", 0, 0, "B", 2, 0, "C", 1, 0));
            IList<string> lines = check.FailedLines("100000");

            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[0], Does.Contain("  A  "));
            Assert.That(lines[1], Does.Contain("  C  "));
            Assert.That(CountCheck.FailedLine("100000", Judged(check, "B")), Is.Null, "B agrees");
        }

        /// <summary>
        /// The totals RESULT reads, one per verdict, so a run where nothing was compared
        /// never reads as nought FAILED and nothing else, the breaker's reading of attempt 1.
        /// </summary>
        [Test]
        public void TheCheckCountsEachVerdictAndTheyAddUpToTheTests()
        {
            CountCheck check = Judge(Tests(Ran("A"), Ran("B"), Ran("C"), Ran("D")),
                Document(InDocument("A", 1, 1), InDocument("B", 2, 2), InDocument("C", -1, -1)),
                Workbook("A", 1, 0, "B", 1, 0, "C", 0, 0));

            Assert.That(check.CountOf(CountVerdict.Agree), Is.EqualTo(1), "A");
            Assert.That(check.CountOf(CountVerdict.Failed), Is.EqualTo(1), "B");
            Assert.That(check.CountOf(CountVerdict.NotCompared), Is.EqualTo(2), "C counted minus one, D in neither");
        }

        [Test]
        public void NoTestsToJudgeIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => CountCheck.Judge(null, Document(), null, null, -1));
        }
    }
}
