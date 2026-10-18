using System.Collections.Generic;
using System.Linq;
using Federator.Core.Clash;
using Federator.Core.Coverage;
using Federator.Core.Exchange;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F224, Bader's answer to Q209 of 18 Oct 2026: only tests across two set codes run. Proved on the matrix built into the
    /// tool, whose 284 tests setting a code against itself Q203 counted, and broken one thing at a time: a side with no code,
    /// two codes that differ, no code known.
    /// </summary>
    [TestFixture]
    public class SetCodeRuleTests
    {
        private static ExchangeDocument Pack()
        {
            string why;
            MatrixPack pack = MatrixPack.TheOne(MatrixPack.All(), out why);
            Assert.That(pack, Is.Not.Null, why);
            return MatrixCorrections.ReadPack(pack);
        }

        [Test]
        public void TheBuiltInMatrixLosesItsTestsOfOneSetCodeAndKeepsEveryOtherTest()
        {
            ExchangeDocument matrix = Pack();
            ClashTestPlan plan = ClashTestPlan.From(matrix, "ft");
            IDictionary<string, int> byCode;

            ClashTestPlan across = plan.AcrossTwoSetCodes(matrix.Teams.KnownCodes(null), '-', out byCode);

            Assert.That(plan.Buildable.Count - across.Buildable.Count, Is.EqualTo(284), "the 284 tests Q203 counted setting a code against itself");
            Assert.That(byCode["AR"], Is.EqualTo(120));
            Assert.That(byCode["EL"], Is.EqualTo(78));
            Assert.That(byCode["ME"], Is.EqualTo(36));
            Assert.That(byCode["ST"], Is.EqualTo(15));
            Assert.That(byCode["FF"], Is.EqualTo(15));
            Assert.That(byCode["PL"], Is.EqualTo(10));
            Assert.That(byCode["DR"], Is.EqualTo(10));
            Assert.That(across.Skipped.Count(s => s.Kind == ClashSkipReason.SameSetCode), Is.EqualTo(284));
            Assert.That(across.Skipped.First(s => s.Kind == ClashSkipReason.SameSetCode).Reason, Does.Contain("Q209"));
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
            Assert.That(plan.AcrossTwoSetCodes(new string[0], '-', out byCode).Buildable.Count, Is.EqualTo(plan.Buildable.Count));
            Assert.That(byCode.Count, Is.EqualTo(0));
        }

        [Test]
        public void TheLineSaysTheCountByCodeAndANoughtAndNoCodeKnownApart()
        {
            Assert.That(SetCodeRule.Line(3, 10, new Dictionary<string, int> { { "ST", 1 }, { "AR", 2 } }, true),
                Is.EqualTo("CLASH    the rule of two set codes left out 3 of the 10 tests planned, each setting a set code against itself, AR 2, ST 1, so none of them is created or run, Bader's answer to Q209"));
            Assert.That(SetCodeRule.Line(0, 10, new Dictionary<string, int>(), true), Does.Contain("left out no test of the 10 planned"));
            Assert.That(SetCodeRule.Line(0, 10, new Dictionary<string, int>(), false), Does.Contain("read no set code"));
        }

        [Test]
        public void ATestTheRuleLeftOutReadsItsOwnCoverageReason()
        {
            Assert.That(CoverageRule.ReasonFor(ClashSkipReason.SameSetCode), Is.EqualTo(CoverageReason.SameSetCode));
            Assert.That(CoverageWords.For(CoverageReason.SameSetCode), Does.Contain("one set code").And.Contain("Q209"));
        }
    }
}
