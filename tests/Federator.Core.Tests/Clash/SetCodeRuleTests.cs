using System.Collections.Generic;
using System.Linq;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Exchange;
using Federator.Core.Report;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F224, Bader's answer to Q209 of 18 Oct 2026: only tests across two set codes run. Proved on the matrix built into the
    /// tool, whose 284 tests setting a code against itself Q209 counted, and broken one thing at a time: a side with no code,
    /// two codes that differ, no code known. SINCE F247, Bader's answer Q230 of 20 Oct 2026, the codes left out are the pack's
    /// rules, AR and ST, so the first tests hand every code to prove the mechanism and the pack's tests read its own rules.
    /// </summary>
    [TestFixture]
    public class SetCodeRuleTests
    {
        private static MatrixPack ThePack()
        {
            string why;
            MatrixPack pack = MatrixPack.TheOne(MatrixPack.All(), out why);
            Assert.That(pack, Is.Not.Null, why);
            return pack;
        }

        private static ExchangeDocument Pack()
        {
            return MatrixCorrections.ReadPack(ThePack());
        }

        /// <summary>Every code the matrix knows, so the rule leaves out every test of one set code, as F224 did.</summary>
        private static IList<string> AllCodes(ExchangeDocument matrix)
        {
            return matrix.Teams.KnownCodes(null);
        }

        /// <summary>
        /// F247, Bader's answer Q230. The pack's own rules leave out AR and ST alone, 120 and 15 tests, and the 149 tests of one
        /// set code of ME, EL, FF, PL and DR run, which F224 had left out.
        /// </summary>
        [Test]
        public void ThePacksRulesLeaveOutArAndStAloneAndTheServicesOfOneCodeRun()
        {
            ExchangeDocument matrix = Pack();
            PackRules rules = ThePack().Rules();
            ClashTestPlan plan = ClashTestPlan.From(matrix, "ft");
            IList<string> known = matrix.Teams.KnownCodes(null);
            IDictionary<string, int> byCode;

            Assert.That(rules.IsRead, Is.True, rules.Unread);
            Assert.That(rules.SameCodeLeftOut, Is.EqualTo(new[] { "AR", "ST" }));

            ClashTestPlan across = plan.AcrossTwoSetCodes(known, rules.SameCodeLeftOut, '-', out byCode);
            IDictionary<string, int> run = SetCodeRule.OfOneCode(across.Buildable, known, '-');

            Assert.That(plan.Buildable.Count - across.Buildable.Count, Is.EqualTo(135));
            Assert.That(byCode.Count, Is.EqualTo(2));
            Assert.That(byCode["AR"], Is.EqualTo(120));
            Assert.That(byCode["ST"], Is.EqualTo(15));
            Assert.That(run.Count, Is.EqualTo(5));
            Assert.That(run["EL"], Is.EqualTo(78));
            Assert.That(run["ME"], Is.EqualTo(36));
            Assert.That(run["FF"], Is.EqualTo(15));
            Assert.That(run["PL"], Is.EqualTo(10));
            Assert.That(run["DR"], Is.EqualTo(10));
            Assert.That(across.Skipped.Count(s => s.Kind == ClashSkipReason.SameSetCode), Is.EqualTo(135));

            Assert.That(plan.AcrossTwoSetCodes(known, null, '-', out byCode).Buildable.Count, Is.EqualTo(plan.Buildable.Count),
                "no code left out, a pack whose rules were not read, runs every test of one set code");
            Assert.That(plan.AcrossTwoSetCodes(known, new[] { "EL" }, '-', out byCode).Buildable.Count, Is.EqualTo(plan.Buildable.Count - 78),
                "the codes are the pack's, so another pack's EL alone leaves out EL alone");
        }

        [Test]
        public void TheBuiltInMatrixLosesItsTestsOfOneSetCodeAndKeepsEveryOtherTest()
        {
            ExchangeDocument matrix = Pack();
            ClashTestPlan plan = ClashTestPlan.From(matrix, "ft");
            IDictionary<string, int> byCode;

            ClashTestPlan across = plan.AcrossTwoSetCodes(matrix.Teams.KnownCodes(null), AllCodes(matrix), '-', out byCode);

            Assert.That(plan.Buildable.Count - across.Buildable.Count, Is.EqualTo(284), "the 284 tests Q209 counted setting a code against itself");
            Assert.That(byCode["AR"], Is.EqualTo(120));
            Assert.That(byCode["EL"], Is.EqualTo(78));
            Assert.That(byCode["ME"], Is.EqualTo(36));
            Assert.That(byCode["ST"], Is.EqualTo(15));
            Assert.That(byCode["FF"], Is.EqualTo(15));
            Assert.That(byCode["PL"], Is.EqualTo(10));
            Assert.That(byCode["DR"], Is.EqualTo(10));
            Assert.That(across.Skipped.Count(s => s.Kind == ClashSkipReason.SameSetCode), Is.EqualTo(284));
            Assert.That(across.Skipped.First(s => s.Kind == ClashSkipReason.SameSetCode).Reason, Does.Contain("the project's rules leave out").And.Not.Contain("Q209").And.Not.Contain("Q230"), "the words of the workbook carry no question number");
            Assert.That(across.Buildable.Any(t => t.Left.Locator.Contains("BLD-PL-") && t.Right.Locator.Contains("BLD-DR-")
                || t.Left.Locator.Contains("BLD-DR-") && t.Right.Locator.Contains("BLD-PL-")), Is.True, "PL against DR still runs");
            Assert.That(across.Buildable.Any(t => SetCodeRule.SameCodeOf(t, matrix.Teams.KnownCodes(null), '-') != null), Is.False);
            Assert.That(across.Source, Is.EqualTo(plan.Source));
        }

        [Test]
        public void ATestIsLeftOutOnlyWhereBothSidesReadOneCode()
        {
            ExchangeDocument matrix = Pack();
            ClashTestPlan plan = ClashTestPlan.From(matrix, "ft");
            List<string> known = new List<string> { "AR", "ST", "EL" };

            PlannedClashTest arAgainstAr = plan.Buildable.First(t => SetCodeRule.SameCodeOf(t, known, '-') == "AR");
            Assert.That(SetCodeRule.SameCodeOf(arAgainstAr, new List<string>(), '-'), Is.Null, "no code known, nothing left out");
            Assert.That(SetCodeRule.SameCodeOf(arAgainstAr, null, '-'), Is.Null);

            PlannedClashTest noCode = plan.Buildable.FirstOrDefault(t => t.Left.Locator.EndsWith("BLD-Security Devices") || t.Right.Locator.EndsWith("BLD-Security Devices"));
            Assert.That(noCode, Is.Not.Null, "the client's set with no code in its name");
            Assert.That(SetCodeRule.SameCodeOf(noCode, matrix.Teams.KnownCodes(null), '-'), Is.Null, "a side with no code is never guessed");

            PlannedClashTest arAgainstSt = plan.Buildable.First(t => t.Left.Locator.Contains("BLD-ST-") && t.Right.Locator.Contains("BLD-AR-"));
            Assert.That(SetCodeRule.SameCodeOf(arAgainstSt, known, '-'), Is.Null, "two codes run");

            IDictionary<string, int> byCode;
            Assert.That(plan.AcrossTwoSetCodes(new string[0], AllCodes(matrix), '-', out byCode).Buildable.Count, Is.EqualTo(plan.Buildable.Count));
            Assert.That(byCode.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheLineSaysTheCountByCodeAndANoughtAndNoCodeKnownApart()
        {
            string[] arSt = { "AR", "ST" };
            Dictionary<string, int> none = new Dictionary<string, int>();
            Dictionary<string, int> services = new Dictionary<string, int> { { "PL", 2 }, { "EL", 3 } };

            Assert.That(SetCodeRule.Line(3, 10, new Dictionary<string, int> { { "ST", 1 }, { "AR", 2 } }, true, 0, arSt, services),
                Is.EqualTo("CLASH    the rule of two set codes left out 3 of the 10 tests planned, each setting a set code against itself, AR 2, ST 1,"
                    + " so none of them is created or run, the pack's rules leaving out AR and ST against themselves alone, and 5 tests of"
                    + " one set code run, EL 3, PL 2, Bader's answers to Q209 and Q230"));
            Assert.That(SetCodeRule.Line(0, 10, none, true, 0, arSt, none), Is.EqualTo("CLASH    the rule of two set codes left out no test of the 10"
                + " planned, the pack's rules leaving out AR and ST against themselves alone, and no test of one set code runs, Bader's"
                + " answers to Q209 and Q230"));
            Assert.That(SetCodeRule.Line(0, 10, none, true, 0, null, new Dictionary<string, int> { { "AR", 1 } }), Does.Contain(
                "the pack's rules leaving out no set code against itself, and 1 test of one set code runs, AR 1"));
            Assert.That(SetCodeRule.Line(0, 10, none, true, 0, new[] { "AR" }, none), Does.Contain("leaving out AR against itself alone"));
            Assert.That(SetCodeRule.Line(0, 10, none, false, 0, arSt, none), Does.Contain("read no set code"));
            Assert.That(SetCodeRule.Line(0, 10, none, true, 10, arSt, none), Does.EndWith(
                "and no test of one set code runs, and 10 of the tests that run have a side whose"
                + " set name reads no set code, so the rule could not judge them and they run, Bader's answers to Q209 and Q230"),
                "a nought where no side read a code is never read as every test judged");
            Assert.That(SetCodeRule.Line(1, 3, new Dictionary<string, int> { { "ST", 1 } }, true, 1, arSt, none), Does.EndWith(
                "and 1 of the tests that run have a side whose set name reads no set code, so the rule"
                + " could not judge it and it runs, Bader's answers to Q209 and Q230"));
            Assert.That(SetCodeRule.NotReadInAGroupOfOne(1830), Does.Contain("not read").And.Contain("one discipline").And.Contain("1830"));
        }

        [Test]
        public void ATestTheRuleLeftOutReadsItsOwnCoverageReason()
        {
            Assert.That(CoverageRule.ReasonFor(ClashSkipReason.SameSetCode), Is.EqualTo(CoverageReason.SameSetCode));
            Assert.That(CoverageWords.For(CoverageReason.SameSetCode), Does.Contain("one set code").And.Not.Contain("Q209"));
        }

        [Test]
        public void ALeftOutTestIsARowOfTheCoverageAndTheRunKnowsItDidNotCreateIt()
        {
            ExchangeDocument matrix = Pack();
            IDictionary<string, int> byCode;
            ClashTestPlan across = ClashTestPlan.From(matrix, "ft").AcrossTwoSetCodes(matrix.Teams.KnownCodes(null), AllCodes(matrix), '-', out byCode);
            ClashRunOutcome outcome = new ClashRunOutcome();

            foreach (SkippedClashTest skipped in across.Skipped)
            {
                outcome.AddSkipped(skipped);
            }

            IList<TestCoverage> rows = CoverageRule.For(across, outcome, matrix.Teams.KnownCodes(null), matrix.Teams.KnownCodes(null),
                new Federator.Core.Views.ViewpointSettings());
            List<TestCoverage> leftOut = rows.Where(r => r.Reason == CoverageReason.SameSetCode).ToList();

            Assert.That(leftOut, Has.Count.EqualTo(284));
            Assert.That(leftOut.All(r => !r.Ran && r.Presence == TestPresence.Unknown), Is.True);

            string folder = TempFolder.Make("FederatorSetCodeRule");

            try
            {
                ClashReport report = new ClashReport("100000", "1104-PAR-100000-ZZZ-BM-RPT-000001");
                report.DocumentUnits = "m";
                WorkbookTestsTests.AddTest(report, "another test", 0, 0);
                WorkbookTests workbook = WorkbookTestsTests.ReadBack(WorkbookTestsTests.Write(report, folder));
                TestCoverage one = leftOut[0];

                CountCheck fresh = CountCheck.Judge(new List<TestCoverage> { one }, new List<DocumentTestCount>(), workbook, null, -1);
                Assert.That(fresh.Tests[0].Verdict, Is.EqualTo(CountVerdict.HeldByNeither), "a fresh NWF never held it");

                CountCheck saved = CountCheck.Judge(new List<TestCoverage> { one },
                    new List<DocumentTestCount> { new DocumentTestCount(one.Name, "Tests", 3, 5) }, workbook, null, -1);
                Assert.That(saved.Tests[0].Verdict, Is.EqualTo(CountVerdict.NotCompared), "F225, an NWF saved before the rule holds it, never FAILED");
                Assert.That(saved.Tests[0].Why, Does.Contain("from an earlier run"));
            }
            finally
            {
                TempFolder.Remove(folder);
            }
        }

        [Test]
        public void ATestWithASideOfNoCodeRunsAndIsCountedAsNotJudged()
        {
            ExchangeDocument matrix = Pack();
            IList<string> known = matrix.Teams.KnownCodes(null);
            ClashTestPlan plan = ClashTestPlan.From(matrix, "ft");
            PlannedClashTest noCode = plan.Buildable.First(t => t.Left.Locator.EndsWith("BLD-Security Devices") || t.Right.Locator.EndsWith("BLD-Security Devices"));
            PlannedClashTest bothRead = plan.Buildable.First(t => t.Left.Locator.Contains("BLD-ST-") && t.Right.Locator.Contains("BLD-AR-"));

            Assert.That(SetCodeRule.WithASideOfNoCode(new[] { noCode, bothRead }, known, '-'), Is.EqualTo(1));
            Assert.That(SetCodeRule.WithASideOfNoCode(new[] { bothRead }, known, '-'), Is.EqualTo(0));
            Assert.That(SetCodeRule.WithASideOfNoCode(new[] { bothRead }, known, '_'), Is.EqualTo(1), "another separator reads no code");
            Assert.That(SetCodeRule.SameCodeOf(null, known, '-'), Is.Null, "no test is never left out");
        }
    }
}
