using System.Collections.Generic;
using Federator.Core.Clash;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F132, FR-183, Bader's Q114 point 7, with Q122's default A. In an NWF that already
    /// holds a test and its mirror, the mirror is not run. One this tool created whose
    /// results carry no status a person set is removed from the NWF. One that carries a
    /// person's status is left, not run, and named for Bader. Only a test that matches a
    /// mirror of the picked XML exactly, name and both sides, is proved to be one this tool
    /// created, so with no XML nothing is ever removed. Each test breaks one thing.
    /// </summary>
    [TestFixture]
    public class MirrorInDocumentTests
    {
        private const string Ducts = MirrorRuleTests.Ducts;
        private const string Columns = MirrorRuleTests.Columns;
        private const string Walls = MirrorRuleTests.Walls;
        private const string DuctsVsColumns = MirrorRuleTests.DuctsVsColumns;
        private const string ColumnsVsDucts = MirrorRuleTests.ColumnsVsDucts;

        /// <summary>The picked XML holding a test and its swap, the swap second, so it is the mirror.</summary>
        private static MirrorRule TheXmlHoldingBoth()
        {
            ClashTestPlan plan = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns),
                MirrorRuleTests.Test(ColumnsVsDucts, Columns, Ducts));

            return MirrorRule.Of(plan.Buildable, PriorityMap.NothingPicked());
        }

        private static IList<PlannedClashTest> SavedInTheDocument(params SavedClashTest[] saved)
        {
            return ClashTestPlan.FromDocument(new List<SavedClashTest>(saved), "m").Buildable;
        }

        /// <summary>The NWF an earlier run made off that XML, holding both as the XML wrote them.</summary>
        private static IList<PlannedClashTest> BothSaved()
        {
            return SavedInTheDocument(
                MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                MirrorRuleTests.Saved(ColumnsVsDucts, Columns, Ducts, 1));
        }

        private static MirrorInDocument TheOneFound(IList<PlannedClashTest> saved, MirrorRule xml)
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(saved, xml, PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(1), "one saved test is a mirror");
            return found[0];
        }

        private static MirrorInDocument TheXmlsMirror()
        {
            MirrorInDocument mirror = TheOneFound(BothSaved(), TheXmlHoldingBoth());

            Assert.That(mirror.Saved.Name, Is.EqualTo(ColumnsVsDucts));
            return mirror;
        }

        private static string OurRecord(ClashStatus wasAt)
        {
            return new AutoReviewRecord(AutoReviewRule.Penetration, wasAt, "a small duct through a wall").Text();
        }

        // ---------- removed ----------

        [Test]
        public void AllNewWithTheXmlIsRemoved()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, string.Empty);
            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Removes, Is.True);
            Assert.That(mirror.Line(), Does.StartWith(MirrorRule.Prefix + " "));
            Assert.That(mirror.Line(), Does.Contain(ColumnsVsDucts));
            Assert.That(mirror.Line(), Does.Contain("removed"));
        }

        [Test]
        public void NoResultsIsRemoved()
        {
            Assert.That(TheXmlsMirror().Removes, Is.True);
        }

        [Test]
        public void ReviewedWithOurRecordIsRemoved()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.AddResult(ClashStatus.Reviewed, OurRecord(ClashStatus.New));

            Assert.That(mirror.Removes, Is.True);
        }

        // ---------- left, not run, and named ----------

        [Test]
        public void AllNewWithNoXmlIsLeft()
        {
            MirrorInDocument mirror = TheOneFound(BothSaved(), null);
            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Saved.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("not run"));
            Assert.That(mirror.Line(), Does.Contain("no XML was picked"));
        }

        [Test]
        public void OneActiveIsLeftNamingActive()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.AddResult(ClashStatus.Active, null);

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("not run"));
            Assert.That(mirror.Line(), Does.Contain("Active 1"));
            Assert.That(mirror.Line(), Does.Contain(ColumnsVsDucts));
            Assert.That(mirror.Line(), Does.Contain(DuctsVsColumns), "the one kept is named beside it");
        }

        [Test]
        public void ReviewedWithNoRecordIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Reviewed, "checked on site");

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("Reviewed 1"));
        }

        // This tool moved it to Reviewed and a person moved it on. The record is still on
        // it and the decision is theirs.
        [Test]
        public void OurRecordMovedOnByAPersonIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Approved, OurRecord(ClashStatus.New));

            Assert.That(mirror.Removes, Is.False);
        }

        [Test]
        public void ApprovedIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Approved, null);

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("Approved 1"));
        }

        [Test]
        public void ResolvedIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Resolved, null);

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("Resolved 1"));
        }

        [Test]
        public void AnUnreadResultIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.ResultNotRead("the status threw");

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("could not be read"));
            Assert.That(mirror.Line(), Does.Contain("the status threw"));
        }

        [Test]
        public void ANameOfTheXmlWithOneSideDifferingIsLeft()
        {
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    MirrorRuleTests.Saved(ColumnsVsDucts, Columns, Ducts + " ", 1)),
                TheXmlHoldingBoth());

            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Saved.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("side"));
        }

        // The add-in hands every saved test the same two placeholders for its sides. A name
        // the picked XML calls a mirror is still that mirror, since the XML decides what
        // runs, but its sides are UNKNOWN, so nothing proves this tool created it and its
        // line says UNKNOWN and never quotes a placeholder as a set.
        [Test]
        public void ANameOfTheXmlWhoseSidesWereNotReadIsLeftAsUnknown()
        {
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved, 0),
                    MirrorRuleTests.Saved(ColumnsVsDucts, SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved, 1)),
                TheXmlHoldingBoth());

            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Saved.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("its sides were not read, UNKNOWN"));
            Assert.That(mirror.Line(), Does.Not.Contain(SavedClashTest.LeftAsSaved));
            Assert.That(mirror.Line(), Does.Not.Contain(SavedClashTest.RightAsSaved));
        }

        // With no XML the placeholders are no sets, so no saved test is a mirror.
        [Test]
        public void SavedTestsWhoseSidesWereNotReadHaveNothingToJudge()
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, SavedClashTest.LeftAsSaved, SavedClashTest.RightAsSaved, 0),
                    MirrorRuleTests.Saved(ColumnsVsDucts, SavedClashTest.RightAsSaved, SavedClashTest.LeftAsSaved, 1)),
                null,
                PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(0));
        }

        // The picked XML holds the test and not its swap. A swap in the NWF was made by a
        // person or by an older XML, so nothing proves this tool created it.
        [Test]
        public void TheSwapOfAnXmlTestTheXmlDoesNotHoldIsLeft()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns));
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    MirrorRuleTests.Saved("Columns against Ducts by hand", Columns, Ducts, 1)),
                MirrorRule.Of(xml.Buildable, PriorityMap.NothingPicked()));

            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Saved.Name, Is.EqualTo("Columns against Ducts by hand"));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("does not hold"));
            Assert.That(mirror.Line(), Does.Contain(DuctsVsColumns));
        }

        // The swap is in the NWF and the test it mirrors is not, because this run creates
        // it. It is still that test's mirror.
        [Test]
        public void ASwapWhoseTestThisRunCreatesIsStillFound()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns));
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(MirrorRuleTests.Saved("Columns against Ducts by hand", Columns, Ducts, 0)),
                MirrorRule.Of(xml.Buildable, PriorityMap.NothingPicked()));

            Assert.That(mirror.Saved.Name, Is.EqualTo("Columns against Ducts by hand"));
            Assert.That(mirror.Removes, Is.False);
        }

        // Two saved tests the XML does not hold, each the other's swap and neither the swap
        // of a test the XML runs. The rule over the two keeps the first the NWF holds, and
        // nothing proves this tool created the other.
        [Test]
        public void TwoSavedTestsTheXmlDoesNotHoldAreAPair()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns));
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    MirrorRuleTests.Saved("Walls against Ducts by hand", Walls, Ducts, 1),
                    MirrorRuleTests.Saved("Ducts against Walls by hand", Ducts, Walls, 2)),
                MirrorRule.Of(xml.Buildable, PriorityMap.NothingPicked()));

            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Saved.Name, Is.EqualTo("Ducts against Walls by hand"));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("a mirror of Walls against Ducts by hand"));
            Assert.That(mirror.Line(), Does.Contain("does not hold"));
        }

        // ---------- what is never judged ----------

        // A saved test the XML does not hold, in the order of the test the XML keeps. Its
        // swap is the XML's mirror, which this run never runs, so it is the kept test's
        // duplicate and no test's mirror.
        [Test]
        public void ASavedTestInTheKeptTestsOwnOrderIsNoMirror()
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    MirrorRuleTests.Saved("Ducts against Columns by hand", Ducts, Columns, 1)),
                TheXmlHoldingBoth(),
                PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(0));
        }

        // The XML keeps the test it holds first and the NWF saved them the other way round.
        // The picked XML decides what this run creates and runs, so the test it keeps is
        // never judged a mirror, whatever order the NWF holds them in.
        [Test]
        public void TheTestTheXmlKeepsIsNeverJudged()
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(ColumnsVsDucts, Columns, Ducts, 0),
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 1)),
                TheXmlHoldingBoth(),
                PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(found[0].Saved.Name, Is.EqualTo(ColumnsVsDucts));
        }

        [Test]
        public void ATestOfTheXmlThatIsNoMirrorIsNeverJudged()
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(
                SavedInTheDocument(MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0)),
                TheXmlHoldingBoth(),
                PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(0));
        }

        [Test]
        public void ADocumentWithNoPairHasNothingToJudge()
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    MirrorRuleTests.Saved("BLD-ME-Ducts-vs-BLD-AR-Walls", Ducts, "walls", 1)),
                null,
                PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(0));
        }
    }
}
