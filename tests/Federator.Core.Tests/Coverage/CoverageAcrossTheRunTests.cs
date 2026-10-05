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
    /// The count check across the run and what RESULT says of it, F127. Bader's answer to
    /// the lead's notes under Q112: a count that differs is a FAILED line in COVERAGE and
    /// RESULT, and the group keeps its own result. So RESULT carries every FAILED line in
    /// full, RESULT never says Nothing failed beside one, and a group judged DONE stays DONE.
    /// The lines here are written by RunLog off the tally and never by the test.
    /// </summary>
    [TestFixture]
    public class CoverageAcrossTheRunTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorCoverageAcrossTheRun");
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

        /// <summary>A real workbook holding each named test with that many plain rows.</summary>
        private WorkbookTests Workbook(string group, params object[] nameThenRows)
        {
            ClashReport report = new ClashReport(group, "1104-PAR-" + group + "-ZZZ-BM-RPT-000001");
            report.DocumentUnits = "m";

            for (int i = 0; i < nameThenRows.Length; i += 2)
            {
                WorkbookTestsTests.AddTest(report, (string)nameThenRows[i], (int)nameThenRows[i + 1], 0);
            }

            string groupFolder = Path.Combine(folder, group);
            Directory.CreateDirectory(groupFolder);
            return WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(report, groupFolder));
        }

        /// <summary>One test T whose document holds that many and whose workbook block that many rows.</summary>
        private CountCheck OneTest(string group, int inTheDocument, int rowsInTheWorkbook)
        {
            return CountCheck.Judge(
                new List<TestCoverage> { Ran("T") },
                new List<DocumentTestCount> { new DocumentTestCount("T", "Tests", inTheDocument, inTheDocument) },
                Workbook(group, "T", rowsInTheWorkbook),
                null,
                -1);
        }

        private static GroupFacts DoneFacts()
        {
            return new GroupFacts
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
        }

        /// <summary>The RESULT block alone, read off the file while the log is open.</summary>
        private static string ResultOf(RunLog log)
        {
            string text;

            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                text = reader.ReadToEnd();
            }

            string title = "RESULT" + Environment.NewLine + "================";
            int at = text.LastIndexOf(title, StringComparison.Ordinal);

            Assert.That(at, Is.GreaterThanOrEqualTo(0), "the log holds no RESULT block");
            return text.Substring(at);
        }

        // ---------- RESULT ----------

        /// <summary>
        /// HIS WORDS. One group judged DONE, one test whose workbook reads two rows where
        /// Clash Detective holds one. RESULT carries the FAILED line in full, counts it, still
        /// counts no failed group, and does not say Nothing failed beside it. The group's
        /// facts and its judgement are unchanged, because a report check never fails a group.
        /// </summary>
        [Test]
        public void ACountThatDiffersIsAFailedLineInResultAndTheGroupKeepsItsOwnResult()
        {
            GroupFacts facts = DoneFacts();
            CountCheck check = OneTest("100000", 1, 2);
            string failedLine = check.FailedLines("100000")[0];
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", check);

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                log.GroupFinished("100000", GroupJudgement.Judge(facts), 1.0, null, null);
                log.WriteResultBlock(null, coverage);

                string result = ResultOf(log);

                Assert.That(result, Does.Contain(failedLine), "the FAILED line is in RESULT in full");
                Assert.That(result, Does.Contain(
                    "COVERAGE checked : 1 compared, 0 agree, 1 FAILED in 1 of 1 group, 0 not compared, "
                        + "every group keeps its own result"));
                Assert.That(result, Does.Contain("groups done    : 1"));
                Assert.That(result, Does.Contain("groups failed  : 0"));
                Assert.That(result, Does.Not.Contain("Nothing failed."),
                    "a FAILED line and Nothing failed cannot both be true");
                Assert.That(result, Does.Contain("No group failed and no error was logged, and 1 coverage count "
                    + "differs, on its COVERAGE FAILED line above"));
            }

            Assert.That(facts.HasErrors, Is.False);
            Assert.That(GroupJudgement.Judge(facts), Is.EqualTo(GroupOutcome.Done));
        }

        /// <summary>Where every count agrees, RESULT still says it checked them, and Nothing failed stands.</summary>
        [Test]
        public void EveryCountAgreeingIsSaidAndNothingFailedStands()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", OneTest("100000", 2, 2));

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                log.WriteResultBlock(null, coverage);
                string result = ResultOf(log);

                Assert.That(result, Does.Contain(
                    "COVERAGE checked : 1 compared, 1 agree, 0 FAILED in 0 of 1 group, 0 not compared"));
                Assert.That(result, Does.Not.Contain(CountCheck.FailedPrefix + "  "));
                Assert.That(result, Does.Contain("Nothing failed."));
            }
        }

        /// <summary>
        /// Nothing compared is never nought FAILED, the breaker's reading of attempt 1: both
        /// wave buildings skip their clash under F112, so every test there is NOT COMPARED.
        /// </summary>
        [Test]
        public void NothingComparedReadsUnknownAndNeverNoughtFailed()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("1A02MM", CountCheck.Judge(new List<TestCoverage> { Ran("T"), Ran("U") },
                new List<DocumentTestCount>(), null, "the clash was skipped by the coordinates rule", -1));

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                log.WriteResultBlock(null, coverage);
                string result = ResultOf(log);

                Assert.That(result, Does.Contain(
                    "COVERAGE checked : UNKNOWN, 0 compared in 1 group, 2 not compared, so no count was checked"));
                Assert.That(result, Does.Not.Contain("0 FAILED"));
            }
        }

        /// <summary>A run that handed RESULT no coverage says so, because a missing line reads as a check that did not run.</summary>
        [Test]
        public void NoCoverageHandedInIsSaidInSoManyWords()
        {
            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                log.WriteResultBlock();
                string result = ResultOf(log);

                Assert.That(result, Does.Contain("COVERAGE checked : UNKNOWN, no coverage was taken in this run"));
                Assert.That(result, Does.Contain("Nothing failed."));
            }
        }

        /// <summary>A group whose coverage could not be taken is counted as such, never as a group that agreed.</summary>
        [Test]
        public void AGroupWithNoCheckIsCountedAsNotChecked()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", OneTest("100000", 1, 2));
            coverage.Add("200000", null);

            IList<string> lines = coverage.ResultLines();

            Assert.That(lines[0], Does.Contain("1 FAILED in 1 of 2 groups"));
            Assert.That(lines[0], Does.Contain("1 group not checked"));
        }

        /// <summary>
        /// Every FAILED line by default, his words. The setting caps them, and where it does
        /// the rest are counted and the block says it truncated.
        /// </summary>
        [Test]
        public void EveryFailedLineIsInResultUnlessTheSettingCapsThem()
        {
            CountCheck first = OneTest("100000", 1, 2);
            CountCheck second = OneTest("200000", 3, 1);

            CoverageAcrossTheRun every = new CoverageAcrossTheRun(new CoverageSettings());
            every.Add("100000", first);
            every.Add("200000", second);

            Assert.That(every.Failed, Is.EqualTo(2));
            Assert.That(every.ResultLines(), Has.Some.StartWith(CountCheck.FailedPrefix + "  100000  T  "));
            Assert.That(every.ResultLines(), Has.Some.StartWith(CountCheck.FailedPrefix + "  200000  T  "));

            CoverageSettings capped = new CoverageSettings();
            capped.FailedLinesInResult = 1;
            CoverageAcrossTheRun one = new CoverageAcrossTheRun(capped);
            one.Add("100000", first);
            one.Add("200000", second);
            IList<string> lines = one.ResultLines();

            Assert.That(lines, Has.Some.StartWith(CountCheck.FailedPrefix + "  100000  T  "));
            Assert.That(lines, Has.None.StartWith(CountCheck.FailedPrefix + "  200000  T  "));
            Assert.That(lines[lines.Count - 1], Is.EqualTo(
                "and 1 more COVERAGE FAILED line, counted and not listed here, because RESULT is set to list 1"));
        }

        [Test]
        public void TheTotalsAddUpOverTheGroups()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", OneTest("100000", 1, 2));
            coverage.Add("200000", OneTest("200000", 2, 2));
            coverage.Add("300000", OneTest("300000", -1, 0));

            Assert.That(coverage.Groups, Is.EqualTo(3));
            Assert.That(coverage.Agree, Is.EqualTo(1));
            Assert.That(coverage.Failed, Is.EqualTo(1));
            Assert.That(coverage.NotCompared, Is.EqualTo(1));
            Assert.That(coverage.GroupsWithAFailedLine, Is.EqualTo(1));
        }

        [Test]
        public void NoSettingsAreRefused()
        {
            Assert.Throws<ArgumentNullException>(() => new CoverageAcrossTheRun(null));
        }
    }
}
