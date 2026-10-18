using System.Collections.Generic;
using Federator.Core.Coverage;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F225, row F219-R1 with Bader's answer to Q209. An NWF saved before a rule of what runs still holds the tests the rule
    /// now leaves out, one discipline, Q203, or one set code on both sides, Q209, with an earlier run's results. The tool never
    /// deletes a test from an NWF, so the count check reads each of them as not run by the rule, NOT COMPARED with the rule's
    /// reason and never FAILED, and the block counts them per group. Broken one thing at a time: a test the rule did not leave
    /// out, a test the document does not hold, a holding the check did not read, a block of this run's workbook and a test
    /// with no record of where it is. A test created this run is CountCheckTests' own.
    /// </summary>
    [TestFixture]
    public class LeftOutByARuleTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorLeftOutByARule");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        private static TestCoverage LeftOut(string name, CoverageReason reason, TestPresence presence)
        {
            return new TestCoverage(1, name, string.Empty, string.Empty, -1, -1, presence, false, -1, reason, string.Empty);
        }

        private static TestCoverage Ran(string name)
        {
            return new TestCoverage(2, name, string.Empty, string.Empty, 2, 2, TestPresence.CreatedThisRun,
                true, 0, CoverageReason.RanAndFoundNone, string.Empty);
        }

        /// <summary>A workbook of this run holding one test that ran and found none, so a left out test has no block.</summary>
        private WorkbookTests Workbook()
        {
            ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            report.DocumentUnits = "m";
            WorkbookTestsTests.AddTest(report, "Ran", 0, 0);
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

            Assert.Fail(name + " was not judged");
            return null;
        }

        [Test]
        public void ATestOfOneSetCodeAnNwfStillHoldsIsNotComparedAndNeverFailed()
        {
            IList<TestCoverage> tests = new List<TestCoverage>
            {
                LeftOut("ST vs ST", CoverageReason.SameSetCode, TestPresence.AlreadyThere),
                LeftOut("PL alone", CoverageReason.OneDiscipline, TestPresence.AlreadyThere),
                Ran("Ran"),
            };
            List<DocumentTestCount> document = new List<DocumentTestCount>
            {
                new DocumentTestCount("ST vs ST", "Tests", 3, 5),
                new DocumentTestCount("PL alone", "Tests", 2, 2),
                new DocumentTestCount("Ran", "Tests", 0, 0),
            };

            CountCheck check = CountCheck.Judge(tests, document, Workbook(), null, -1);
            CountedTest sameCode = Judged(check, "ST vs ST");

            Assert.That(sameCode.Verdict, Is.EqualTo(CountVerdict.NotCompared));
            Assert.That(sameCode.KeptFromAnEarlierRun, Is.True);
            Assert.That(sameCode.Why, Does.StartWith("not run by the rule, ").And.Contain("one set code")
                .And.Contain("3 results and 5 clashes for it from an earlier run").And.Contain("never deleted").And.Not.Contain("Q212"), "the words of the Coverage sheet carry no question number");
            Assert.That(Judged(check, "PL alone").Verdict, Is.EqualTo(CountVerdict.NotCompared), "the F219-R1 case of Q203");
            Assert.That(check.FailedLines("100000"), Is.Empty);
            Assert.That(check.CountOf(CountVerdict.Agree), Is.EqualTo(1));
            Assert.That(check.LeftOutByARuleLine(), Is.EqualTo(
                "not run by the rule : 2 tests left out by the rule of one discipline or of two set codes, 2 still held in"
                + " Clash Detective from an earlier run with 7 clashes, kept in the NWF and never deleted, not compared and never FAILED,"
                + " 0 held by neither side, 0 whose holding in Clash Detective is UNKNOWN, a side or a count not read or a name on two"
                + " tests, whether those held are removed is Bader's to decide, Q212"));

            string block = string.Join("\n", CoverageBlock.Lines("100000",
                new CoverageSheetData(TestPaths.At("picked", "clash.xml"), "ab12", null, null, tests, check, null, null), new CoverageSettings()));
            Assert.That(block, Does.Contain(check.LeftOutByARuleLine()));
        }

        [Test]
        public void ATestNoRuleLeftOutIsStillFailedWhereItsCountsDiffer()
        {
            IList<TestCoverage> tests = new List<TestCoverage>
            {
                LeftOut("Side empty", CoverageReason.SideFoundNothing, TestPresence.AlreadyThere),
            };

            CountCheck check = CountCheck.Judge(tests,
                new List<DocumentTestCount> { new DocumentTestCount("Side empty", "Tests", 3, 5) }, Workbook(), null, -1);

            Assert.That(Judged(check, "Side empty").Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(check.FailedLines("100000"), Has.Count.EqualTo(1));
            Assert.That(check.LeftOutByARuleLine(), Is.Null, "no rule left a test out, so no line");
        }

        [Test]
        public void ATestTheRuleLeftOutOfAPlanAndNoDocumentHoldsIsHeldByNeither()
        {
            IList<TestCoverage> tests = new List<TestCoverage>
            {
                LeftOut("AR vs AR", CoverageReason.SameSetCode, TestPresence.Unknown),
                LeftOut("Walked for", CoverageReason.SameSetCode, TestPresence.AlreadyThere),
            };

            CountCheck check = CountCheck.Judge(tests, new List<DocumentTestCount>(), Workbook(), null, -1);

            Assert.That(Judged(check, "AR vs AR").Verdict, Is.EqualTo(CountVerdict.HeldByNeither));
            Assert.That(Judged(check, "Walked for").Verdict, Is.EqualTo(CountVerdict.NotCompared),
                "one the run holds as there and the read did not return is judged as before");
            Assert.That(Judged(check, "Walked for").Why, Does.Contain("did not return it"));
            Assert.That(check.LeftOutByARuleLine(), Does.Contain("2 tests left out").And.Contain(", 0 still held in")
                .And.Contain("1 held by neither side").And.Contain("1 whose holding in Clash Detective is UNKNOWN"));
        }

        /// <summary>The reviewer's and the breaker's finding: a holding the check never read is UNKNOWN on the line, never none.</summary>
        [Test]
        public void AHoldingTheCheckDidNotReadIsUnknownAndNeverNone()
        {
            IList<TestCoverage> tests = new List<TestCoverage>
            {
                LeftOut("ST vs ST", CoverageReason.SameSetCode, TestPresence.AlreadyThere),
                LeftOut("AR vs AR", CoverageReason.SameSetCode, TestPresence.Unknown),
            };
            List<DocumentTestCount> document = new List<DocumentTestCount> { new DocumentTestCount("ST vs ST", "Tests", 3, 5) };

            string noWorkbook = CountCheck.Judge(tests, document, null, "the write threw", -1).LeftOutByARuleLine();
            Assert.That(noWorkbook, Does.Contain(", 0 still held in").And.Contain("2 whose holding in Clash Detective is UNKNOWN"));

            string noDocument = CountCheck.Judge(tests, null, Workbook(), null, -1).LeftOutByARuleLine();
            Assert.That(noDocument, Does.Contain("2 whose holding in Clash Detective is UNKNOWN"));

            List<DocumentTestCount> notCounted = new List<DocumentTestCount> { new DocumentTestCount("ST vs ST", "Tests", -1, -1) };
            string countNotTaken = CountCheck.Judge(tests, notCounted, Workbook(), null, -1).LeftOutByARuleLine();
            Assert.That(countNotTaken, Does.Contain(", 0 still held in").And.Contain("1 held by neither side").And.Contain("1 whose holding"));
        }

        /// <summary>
        /// The reviewer's second finding: a test of one discipline with no record of whether it is in the document is never
        /// said to hold an earlier run's results, and a block of this run's workbook for a test the rule left out is a writer's
        /// fault, compared as before.
        /// </summary>
        [Test]
        public void OnlyWhatTheRecordProvesIsKept()
        {
            IList<TestCoverage> tests = new List<TestCoverage>
            {
                LeftOut("No record", CoverageReason.OneDiscipline, TestPresence.Unknown),
                LeftOut("Written", CoverageReason.SameSetCode, TestPresence.AlreadyThere),
            };
            List<DocumentTestCount> document = new List<DocumentTestCount>
            {
                new DocumentTestCount("No record", "Tests", 2, 2),
                new DocumentTestCount("Written", "Tests", 3, 3),
            };

            ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
            report.DocumentUnits = "m";
            WorkbookTestsTests.AddTest(report, "Written", 2, 0);
            WorkbookTests workbook = WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(report, folder));

            CountCheck check = CountCheck.Judge(tests, document, workbook, null, -1);

            Assert.That(Judged(check, "No record").KeptFromAnEarlierRun, Is.False);
            Assert.That(Judged(check, "Written").KeptFromAnEarlierRun, Is.False);
            Assert.That(Judged(check, "Written").Verdict, Is.EqualTo(CountVerdict.Failed), "a block for a test the rule left out is the writer's fault");
            Assert.That(check.LeftOutByARuleLine(), Does.Contain(", 0 still held in"));
        }
    }
}
