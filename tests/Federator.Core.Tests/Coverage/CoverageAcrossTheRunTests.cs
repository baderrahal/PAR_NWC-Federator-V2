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

        /// <summary>A test F77 kept out: not in the document and never run.</summary>
        private static TestCoverage KeptOut(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 0, 2, TestPresence.NotInDocument,
                false, -1, CoverageReason.SideFoundNothing, string.Empty);
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
                log.WriteResultBlock(null, coverage: coverage);

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
                log.WriteResultBlock(null, coverage: coverage);
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
                log.WriteResultBlock(null, coverage: coverage);
                string result = ResultOf(log);

                Assert.That(result, Does.Contain(
                    "COVERAGE checked : UNKNOWN, 0 compared in 1 group, 2 not compared, so no count was checked"));
                Assert.That(result, Does.Not.Contain("0 FAILED"));
            }
        }

        /// <summary>
        /// The tests neither side holds are counted apart from the ones that agree, and the tests Clash
        /// Detective holds that the picked file does not name are said, so the headline is never a
        /// clean bill over a test nobody set beside the panel.
        /// </summary>
        [Test]
        public void TestsNeitherSideHoldsAndTestsTheFileDoesNotNameAreSaidApartFromAgree()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", CountCheck.Judge(
                new List<TestCoverage> { Ran("T"), KeptOut("U"), KeptOut("V"), KeptOut("W") },
                new List<DocumentTestCount>
                {
                    new DocumentTestCount("T", "Tests", 2, 2),
                    new DocumentTestCount("Old", "Tests", 5, 5),
                    new DocumentTestCount("Older", "Tests", 1, 1)
                },
                Workbook("100000", "T", 2, "U", 0, "V", 0, "W", 0),
                null,
                -1));

            Assert.That(coverage.Agree, Is.EqualTo(1));
            Assert.That(coverage.HeldByNeither, Is.EqualTo(3));
            Assert.That(coverage.NotInTheXml, Is.EqualTo(2));

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                log.WriteResultBlock(null, coverage: coverage);
                string result = ResultOf(log);

                Assert.That(result, Does.Contain(
                    "COVERAGE checked : 1 compared, 1 agree, 0 FAILED in 0 of 1 group, 0 not compared, "
                        + "3 held by neither side, tests no count was set beside"));
                Assert.That(result, Does.Contain(
                    "COVERAGE not named    : 2 tests in Clash Detective that the picked file does not name, not compared"));
            }
        }

        /// <summary>Only tests neither side holds is no count that was checked, and reads UNKNOWN with them said.</summary>
        [Test]
        public void OnlyTestsNeitherSideHoldsReadsUnknownAndSaysHowManyWereHeldByNeither()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", CountCheck.Judge(
                new List<TestCoverage> { KeptOut("U"), KeptOut("V") },
                new List<DocumentTestCount>(),
                Workbook("100000", "U", 0, "V", 0),
                null,
                -1));

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 5, 3, 0, 0)))
            {
                log.WriteResultBlock(null, coverage: coverage);

                Assert.That(ResultOf(log), Does.Contain(
                    "COVERAGE checked : UNKNOWN, 0 compared in 1 group, 0 not compared, 2 held by neither side, "
                        + "tests no count was set beside, so no count was checked"));
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
        /// The tests the picked file does not name are counted by name across the run, as Q127 A counts
        /// every other test. One old test sitting in the NWFs of 46 groups read as 46 tests, which says
        /// the document holds 46 tests the file does not name. The break: a second, different old test
        /// in one group only makes two names over 47 places.
        /// </summary>
        [Test]
        public void ATestTheFileDoesNotNameIsCountedOnceAcrossTheRunWhateverTheGroups()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            WorkbookTests workbook = Workbook("100000", "T", 0);

            for (int group = 1; group <= 46; group++)
            {
                coverage.Add("group" + group, CountCheck.Judge(
                    new List<TestCoverage> { Ran("T") },
                    new List<DocumentTestCount>
                    {
                        new DocumentTestCount("T", "Tests", 0, 0),
                        new DocumentTestCount("Old", "Tests", 5, 5)
                    },
                    workbook,
                    null,
                    -1));
            }

            Assert.That(coverage.NotInTheXml, Is.EqualTo(1));

            List<string> lines = new List<string>(coverage.ResultLines());
            string named = lines.Find(line => line.StartsWith("COVERAGE not named", StringComparison.Ordinal));

            Assert.That(named, Does.Contain("1 test in Clash Detective that the picked file does not name"));
            Assert.That(named, Does.Contain("each name once here, 46 places over the groups"));

            coverage.Add("last", CountCheck.Judge(
                new List<TestCoverage> { Ran("T") },
                new List<DocumentTestCount>
                {
                    new DocumentTestCount("T", "Tests", 0, 0),
                    new DocumentTestCount("Older", "Tests", 1, 1)
                },
                workbook,
                null,
                -1));

            Assert.That(coverage.NotInTheXml, Is.EqualTo(2));

            named = new List<string>(coverage.ResultLines())
                .Find(line => line.StartsWith("COVERAGE not named", StringComparison.Ordinal));

            Assert.That(named, Does.Contain("2 tests in Clash Detective that the picked file does not name"));
            Assert.That(named, Does.Contain("each name once here, 47 places over the groups"));
        }

        /// <summary>
        /// The headline printed how many tests agreed and how many failed and never how many test places its
        /// buckets held, so they could not be added up by eye, and a group whose check could not be made has
        /// its tests in none of them, which it now says. A test is counted once for each group it is in, so
        /// the total is test places and not tests, and it counts the tests nothing compared as well.
        /// </summary>
        [Test]
        public void TheHeadlineSaysHowManyTestPlacesItHoldsAndThatAGroupNotCheckedIsInNoneOfThem()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("100000", CountCheck.Judge(
                new List<TestCoverage> { Ran("T"), KeptOut("U"), KeptOut("V"), KeptOut("W") },
                new List<DocumentTestCount> { new DocumentTestCount("T", "Tests", 2, 2) },
                Workbook("100000", "T", 2, "U", 0, "V", 0, "W", 0),
                null,
                -1));
            coverage.Add("200000", null);

            string headline = new List<string>(coverage.ResultLines())
                .Find(line => line.StartsWith("COVERAGE checked", StringComparison.Ordinal));

            Assert.That(headline, Does.Contain("1 compared, 1 agree, 0 FAILED in 0 of 2 groups, 0 not compared"));
            Assert.That(headline, Does.Contain("3 held by neither side"));
            Assert.That(headline, Does.Contain("4 test places in the groups checked, a test once for each group it is in"));
            Assert.That(headline, Does.Contain("1 group not checked, its tests in none of these counts"));
        }

        /// <summary>
        /// The run that compared nothing says UNKNOWN, and a group whose check could not be made is said
        /// there too, in the plural for two, since the line reads as a verification of every test otherwise.
        /// </summary>
        [Test]
        public void ANothingComparedRunWithGroupsNotCheckedSaysTheirTestsAreInNoneOfTheCounts()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.Add("1A02MM", CountCheck.Judge(new List<TestCoverage> { Ran("T"), Ran("U") },
                new List<DocumentTestCount>(), null, "the clash was skipped by the coordinates rule", -1));
            coverage.Add("1A04PK", null);
            coverage.Add("1B06BS", null);

            string headline = new List<string>(coverage.ResultLines())
                .Find(line => line.StartsWith("COVERAGE checked", StringComparison.Ordinal));

            Assert.That(headline, Does.Contain("UNKNOWN, 0 compared in 3 groups, 2 not compared"));
            Assert.That(headline, Does.Contain("2 groups not checked, their tests in none of these counts"));
            Assert.That(headline, Does.EndWith("so no count was checked"));
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

        // ---------- the tests of the picked file, Q127 answered A ----------

        private static TestCoverage Clashed(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 2, 2, TestPresence.CreatedThisRun,
                true, 3, CoverageReason.HasClashes, string.Empty);
        }

        private static TestCoverage AlreadyThereNotRun(string name)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, 2, 2, TestPresence.AlreadyThere,
                false, -1, CoverageReason.CoordinatesRule, string.Empty);
        }

        /// <summary>
        /// EACH TEST ONCE ACROSS THE RUN, Bader's answer A to Q127, with the sums over the groups
        /// beside it. A test created in one group and run in another counts once as in the
        /// document and once as run, a test with clashes in one group and none in another counts
        /// once as with clashes, and the sums read every place.
        /// </summary>
        [Test]
        public void EachTestIsCountedOnceAcrossTheRunAndTheSumsReadEveryPlace()
        {
            CoverageAcrossTheRun coverage = new CoverageAcrossTheRun(new CoverageSettings());
            coverage.AddTests("100000", new List<TestCoverage> { AlreadyThereNotRun("T1"), Clashed("T2"), KeptOut("T3") });
            coverage.AddTests("200000", new List<TestCoverage> { Ran("T1"), Ran("T2"), KeptOut("T3") });

            IList<string> lines = coverage.TestLines();

            Assert.That(lines[0], Is.EqualTo("COVERAGE tests counted            : 3 names, each once across 2 groups"));
            Assert.That(lines[1], Is.EqualTo("COVERAGE in the document in one group at least : 2, never in the document 1"));
            Assert.That(lines[2], Is.EqualTo("COVERAGE run in one group at least : 2"));
            Assert.That(lines[3], Is.EqualTo("COVERAGE with clashes in one group at least : 1"));
            Assert.That(lines[4], Is.EqualTo("COVERAGE run, never with a clash  : 1"));
            Assert.That(lines[5], Is.EqualTo("COVERAGE over 2 groups, added up : 6 places, 3 created this run, 1 already there,"
                + " 2 not created, 3 run, 1 with clashes, 2 without, 1 in the document and not run"));
        }

        [Test]
        public void TheTestLinesComeFirstInResultAndNoneWhereNoTestsWereHandedIn()
        {
            CoverageAcrossTheRun none = new CoverageAcrossTheRun(new CoverageSettings());
            none.Add("100000", null);

            Assert.That(none.TestLines(), Is.Empty);
            Assert.That(none.ResultLines()[0], Does.StartWith("COVERAGE checked"));

            CoverageAcrossTheRun some = new CoverageAcrossTheRun(new CoverageSettings());
            some.AddTests("100000", new List<TestCoverage> { Ran("T1") });
            some.Add("100000", null);

            Assert.That(some.ResultLines()[0], Does.StartWith("COVERAGE tests counted"));
            Assert.That(some.ResultLines()[6], Does.StartWith("COVERAGE checked"));
        }
    }
}
