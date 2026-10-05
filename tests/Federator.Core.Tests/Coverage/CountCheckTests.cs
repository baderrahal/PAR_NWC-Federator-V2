using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Coverage;
using Federator.Core.Diagnostics;
using Federator.Core.Report;
using Federator.Core.Rerun;
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

        /// <summary>F77's tests not created: not in the document, and one row reading nought.</summary>
        [Test]
        public void ATestNotInTheDocumentThatTheWorkbookReadsAsNoneAgrees()
        {
            CountCheck check = Judge(Tests(NotRun("T")), Document(), Workbook("T", 0, 0));

            Assert.That(Judged(check, "T").Verdict, Is.EqualTo(CountVerdict.Agree));
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
            Assert.That(check.NotInTheXml, Is.EqualTo(new[] { "Old" }));
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
        /// THE GROUP KEEPS ITS OWN RESULT, by construction. GroupFacts has no coverage member
        /// and nothing here adds an error, so a group judged DONE stays DONE beside a COVERAGE
        /// FAILED line, RESULT counts no failed group, and the line is in the log.
        /// </summary>
        [Test]
        public void ADoneGroupBesideACoverageFailedLineStaysDoneAndResultCountsNoFailedGroup()
        {
            GroupFacts facts = new GroupFacts
            {
                Decision = RerunDecision.Build,
                NwfOnDisk = true,
                NwdRequested = true,
                NwdOnDisk = true,
                NwdPublishReportedSuccess = true,
                AppendedCount = 3,
                FileCount = 3,
                FailedFileCount = 0
            };

            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));

            CountCheck check = Judge(Tests(Ran("T")), Document(InDocument("T", 1, 1)), Workbook("T", 2, 0));
            string logFolder = Path.Combine(folder, "logs");

            using (RunLog log = RunLog.Start(logFolder, new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                foreach (string line in check.FailedLines("100000"))
                {
                    log.Line(line);
                }

                log.GroupFinished("100000", GroupJudgement.Judge(facts), 1.0, null, null);
                log.WriteResultBlock();

                string text;

                using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    text = reader.ReadToEnd();
                }

                Assert.That(text, Does.Contain("COVERAGE FAILED  100000  T  "));
                Assert.That(text, Does.Contain("groups done    : 1"));
                Assert.That(text, Does.Contain("groups failed  : 0"));
                Assert.That(text, Does.Contain("Nothing failed."));
                Assert.That(facts.HasErrors, Is.False);
                Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
            }
        }

        [Test]
        public void NoTestsToJudgeIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => CountCheck.Judge(null, Document(), null, null, -1));
        }
    }
}
