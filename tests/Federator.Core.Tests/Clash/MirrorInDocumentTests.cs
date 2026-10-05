using System;
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
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.True);
            Assert.That(mirror.Line(), Does.StartWith(MirrorRule.Prefix + " "));
            Assert.That(mirror.Line(), Does.Contain(ColumnsVsDucts));
            Assert.That(mirror.Line(), Does.Contain("removed"));
        }

        [Test]
        public void NoResultsIsRemoved()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.True);
        }

        [Test]
        public void ReviewedWithOurRecordIsRemoved()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.AddResult(ClashStatus.Reviewed, OurRecord(ClashStatus.New));
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.True);
        }

        // ---------- the walk of the results, fail closed ----------

        // Core cannot tell a test with no results from a walk of its results that never ran,
        // so nothing is removed until the add-in says the walk reached its end.
        [Test]
        public void AWalkNeverSaidCompleteIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("the walk of its results was not said to be complete"));
            Assert.That(mirror.Line(), Does.Contain("UNKNOWN"));
        }

        // A walk that threw after three New results, the rest never read and nothing said.
        [Test]
        public void AWalkThatStoppedPartWayIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.AddResult(ClashStatus.New, null);
            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("New 3"));
        }

        // A result handed after the walk was said complete means the walk went on, so it is
        // complete again only once that is said again.
        [Test]
        public void AResultAfterTheWalkWasSaidCompleteLeavesItUntilSaidAgain()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.AllResultsAdded();
            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Removes, Is.False);

            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.True);
        }

        // ---------- left, not run, and named ----------

        [Test]
        public void AllNewWithNoXmlIsLeft()
        {
            MirrorInDocument mirror = TheOneFound(BothSaved(), null);
            mirror.AddResult(ClashStatus.New, null);
            mirror.AllResultsAdded();

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
            mirror.AllResultsAdded();

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
            mirror.AllResultsAdded();

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
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.False);
        }

        [Test]
        public void ApprovedIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Approved, null);
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("Approved 1"));
        }

        [Test]
        public void ResolvedIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Resolved, null);
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("Resolved 1"));
        }

        [Test]
        public void AnUnreadResultIsLeft()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);
            mirror.ResultNotRead("the status threw");
            mirror.AllResultsAdded();

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
            mirror.AllResultsAdded();

            Assert.That(mirror.Saved.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("side"));
        }

        // A person opened the mirror, raised its tolerance from 25 mm to 100 mm and ran it,
        // setting no status yet. Its name and sides are still the XML's mirror, but it is not
        // the test the XML would create, so nothing proves this tool created it.
        [Test]
        public void ASavedMirrorWhoseToleranceWasRaisedIsLeft()
        {
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    new SavedClashTest(ColumnsVsDucts, 1, 0.1, true, false, 1, Columns, false, 1, Ducts, new[] { 1 })),
                TheXmlHoldingBoth());

            mirror.AddResult(ClashStatus.New, null);
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("not the test the picked XML would create"));
            Assert.That(mirror.Line(), Does.Contain("the tolerance 0.025 in the XML and 0.1 saved"));
        }

        // The same for a side's own setting, read on the same side the XML's mirror has it.
        [Test]
        public void ASavedMirrorWithSelfIntersectTurnedOnIsLeft()
        {
            MirrorInDocument mirror = TheOneFound(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    new SavedClashTest(ColumnsVsDucts, 1, 0.025, true, true, 1, Columns, false, 1, Ducts, new[] { 1 })),
                TheXmlHoldingBoth());

            mirror.AddResult(ClashStatus.New, null);
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("the left side self intersect off in the XML and on saved"));
        }

        // Two saved tests carry the mirror's name, one in another folder. Each matches the
        // XML on its own, and a removal that finds its test by name could take the other.
        [Test]
        public void TwoSavedTestsWithTheMirrorsNameAreBothLeft()
        {
            IList<MirrorInDocument> found = MirrorInDocument.Find(
                SavedInTheDocument(
                    MirrorRuleTests.Saved(DuctsVsColumns, Ducts, Columns, 0),
                    MirrorRuleTests.Saved(ColumnsVsDucts, Columns, Ducts, 1),
                    MirrorRuleTests.Saved(ColumnsVsDucts, Columns, Ducts, 2)),
                TheXmlHoldingBoth(),
                PriorityMap.NothingPicked());

            Assert.That(found.Count, Is.EqualTo(2));

            foreach (MirrorInDocument mirror in found)
            {
                mirror.AddResult(ClashStatus.New, null);
                mirror.AllResultsAdded();

                Assert.That(mirror.Removes, Is.False);
                Assert.That(mirror.Line(), Does.Contain("another saved test carries the same name"));
            }
        }

        // The same reason a result could not be read, again and again, is said once with its
        // count, never repeated once per result.
        [Test]
        public void OneReasonAResultCouldNotBeReadIsSaidOnceWithItsCount()
        {
            MirrorInDocument mirror = TheXmlsMirror();

            for (int i = 0; i < 7; i++)
            {
                mirror.ResultNotRead("the status threw");
            }

            mirror.AllResultsAdded();
            string line = mirror.Line();

            Assert.That(mirror.Removes, Is.False);
            Assert.That(line, Does.Contain("7 results could not be read: the status threw 7 times"));
            Assert.That(line.Split(new[] { "the status threw" }, StringSplitOptions.None).Length - 1, Is.EqualTo(1));
        }

        // More than five reasons: five named and the rest counted.
        [Test]
        public void ManyReasonsAResultCouldNotBeReadAreFiveNamedAndTheRestCounted()
        {
            MirrorInDocument mirror = TheXmlsMirror();

            for (int i = 1; i <= 7; i++)
            {
                mirror.ResultNotRead("reason " + i);
            }

            mirror.AllResultsAdded();
            string line = mirror.Line();

            Assert.That(line, Does.Contain("7 results could not be read: reason 1, reason 2"));
            Assert.That(line, Does.Contain("reason 5, and 2 more reasons"));
            Assert.That(line, Does.Not.Contain("reason 6"));
        }

        // Q122's default A: a Reviewed carrying this tool's record counts as this tool's,
        // also where the record says it was Active before. The line says how many were.
        [Test]
        public void ReviewedByThisToolOffActiveIsCountedInTheLine()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.Reviewed, OurRecord(ClashStatus.Active));
            mirror.AddResult(ClashStatus.Reviewed, OurRecord(ClashStatus.Active));
            mirror.AddResult(ClashStatus.Reviewed, OurRecord(ClashStatus.New));
            mirror.AllResultsAdded();

            Assert.That(mirror.Removes, Is.True);
            Assert.That(mirror.Line(), Does.Contain("3 of the Reviewed set by this tool, 2 of them Active before it moved them"));
        }

        // A walk never said complete leaves the results UNKNOWN, never none.
        [Test]
        public void AWalkNeverSaidCompleteGivesItsResultsAsUnknown()
        {
            MirrorInDocument mirror = TheXmlsMirror();

            Assert.That(mirror.Line(), Does.EndWith("Its results: UNKNOWN"));
        }

        // A walk that stopped part way names what it read and says the rest is UNKNOWN.
        [Test]
        public void AWalkThatStoppedPartWaySaysWhetherThatIsAllIsUnknown()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AddResult(ClashStatus.New, null);

            Assert.That(mirror.Line(), Does.EndWith("Its results: New 1, and whether that is all of them is UNKNOWN"));
        }

        // A walk said complete over no result has none, and says so.
        [Test]
        public void AWalkSaidCompleteOverNoResultSaysNone()
        {
            MirrorInDocument mirror = TheXmlsMirror();
            mirror.AllResultsAdded();

            Assert.That(mirror.Line(), Does.EndWith("Its results: none"));
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
            mirror.AllResultsAdded();

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
            mirror.AllResultsAdded();

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

            mirror.AllResultsAdded();

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
            mirror.AllResultsAdded();

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

        // ---------- what is refused, so a saved test is never taken for one this tool created ----------

        // No XML was picked and the rule handed as the picked XML's was built over the saved
        // tests themselves. Taken as the XML's, each saved swap would be found by its own
        // name and match itself, and a person's test would be removed with a line claiming
        // it was created from an XML nobody picked. With no XML the rule is null.
        [Test]
        public void ARuleBuiltOverTheSavedTestsIsRefusedAsThePickedXmls()
        {
            IList<PlannedClashTest> saved = BothSaved();
            MirrorRule overTheSaved = MirrorRule.Of(saved, PriorityMap.NothingPicked());

            ArgumentException refused = Assert.Throws<ArgumentException>(
                () => MirrorInDocument.Find(saved, overTheSaved, PriorityMap.NothingPicked()));

            Assert.That(refused.ParamName, Is.EqualTo("ofThePickedXml"));
            Assert.That(refused.Message, Does.Contain(DuctsVsColumns));
            Assert.That(refused.Message, Does.Contain("read off the document"));
        }

        // One saved test among the XML's is enough, because that one would match itself.
        [Test]
        public void ARuleHoldingOneSavedTestAmongTheXmlsIsRefused()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns));
            IList<PlannedClashTest> saved = SavedInTheDocument(
                MirrorRuleTests.Saved("Columns against Ducts by hand", Columns, Ducts, 0));
            List<PlannedClashTest> mixed = new List<PlannedClashTest>(xml.Buildable);
            mixed.AddRange(saved);

            ArgumentException refused = Assert.Throws<ArgumentException>(
                () => MirrorInDocument.Find(saved, MirrorRule.Of(mixed, PriorityMap.NothingPicked()),
                    PriorityMap.NothingPicked()));

            Assert.That(refused.ParamName, Is.EqualTo("ofThePickedXml"));
            Assert.That(refused.Message, Does.Contain("Columns against Ducts by hand"));
        }

        // The other way round: the XML's own tests handed as the saved ones. Each XML mirror
        // would match itself and be judged removable whatever the document holds under its
        // name, so a test that was not read off the document is refused as a saved one.
        [Test]
        public void TheXmlsTestsHandedAsTheSavedOnesAreRefused()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns),
                MirrorRuleTests.Test(ColumnsVsDucts, Columns, Ducts));

            ArgumentException refused = Assert.Throws<ArgumentException>(
                () => MirrorInDocument.Find(xml.Buildable, TheXmlHoldingBoth(), PriorityMap.NothingPicked()));

            Assert.That(refused.ParamName, Is.EqualTo("saved"));
            Assert.That(refused.Message, Does.Contain(DuctsVsColumns));
            Assert.That(refused.Message, Does.Contain("not read off the document"));
        }

        // With no XML as well, since the rule Find builds over them would hold them.
        [Test]
        public void TheXmlsTestsHandedAsTheSavedOnesAreRefusedWithNoXml()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns),
                MirrorRuleTests.Test(ColumnsVsDucts, Columns, Ducts));

            ArgumentException refused = Assert.Throws<ArgumentException>(
                () => MirrorInDocument.Find(xml.Buildable, null, PriorityMap.NothingPicked()));

            Assert.That(refused.ParamName, Is.EqualTo("saved"));
        }

        // The edge the refusal must not reach: a picked XML whose every test was skipped
        // gives a rule over no test at all. It is the XML's, it holds no pair, so it proves
        // no saved test is this tool's and none is removed.
        [Test]
        public void TheRuleOfAnXmlWithNoBuildableTestIsTakenAndRemovesNothing()
        {
            ClashTestPlan xml = MirrorRuleTests.Plan(
                MirrorRuleTests.Test(DuctsVsColumns, Ducts, Columns, type: "not_a_test_type"));

            Assert.That(xml.Buildable.Count, Is.EqualTo(0));

            MirrorInDocument mirror = TheOneFound(BothSaved(), MirrorRule.Of(xml.Buildable, PriorityMap.NothingPicked()));
            mirror.AllResultsAdded();

            Assert.That(mirror.Saved.Name, Is.EqualTo(ColumnsVsDucts));
            Assert.That(mirror.Removes, Is.False);
            Assert.That(mirror.Line(), Does.Contain("does not hold"));
        }
    }
}
