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
    /// out, a test created this run, a test the document does not hold.
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
                "not run by the rule : 2 tests left out by the rule of one discipline or of two set codes, 2 of them still held in"
                + " Clash Detective from an earlier run with 7 clashes, kept in the NWF and never deleted, not compared and never FAILED, whether they are removed is Bader's to decide, Q212"));

            string block = string.Join("\n", CoverageBlock.Lines("100000",
                new CoverageSheetData(@"C:pickedash.xml", "ab12", null, null, tests, check, null, null), new CoverageSettings()));
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
        public void ATestCreatedThisRunIsNeverSaidToHoldAnEarlierRunsResults()
        {
            IList<TestCoverage> tests = new List<TestCoverage>
            {
                LeftOut("Made now", CoverageReason.OneDiscipline, TestPresence.CreatedThisRun),
            };

            CountCheck check = CountCheck.Judge(tests,
                new List<DocumentTestCount> { new DocumentTestCount("Made now", "Tests", 2, 2) }, Workbook(), null, -1);

            Assert.That(Judged(check, "Made now").Verdict, Is.EqualTo(CountVerdict.Failed));
            Assert.That(Judged(check, "Made now").KeptFromAnEarlierRun, Is.False);
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
            Assert.That(check.LeftOutByARuleLine(), Does.Contain("2 tests left out").And.Contain("none of them held in Clash Detective"));
        }
    }
}
