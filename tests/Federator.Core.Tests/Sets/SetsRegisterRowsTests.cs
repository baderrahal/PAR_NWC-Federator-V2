using System;
using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Health;
using Federator.Core.Naming;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests.Sets
{
    /// <summary>
    /// The register rows of the sets area that change what the team sees, F115 R2 to R23, wave 2a
    /// of the fix round. Each test breaks one thing and asserts the words that name it.
    /// </summary>
    [TestFixture]
    public class SetsRegisterRowsTests
    {
        private const string Ducts = "lcop_selection_set_tree/A/BLD-ME-Ducts";

        private static string Joined(IList<string> lines)
        {
            return string.Join("\n", new List<string>(lines).ToArray());
        }

        private static SetBuildOutcome OnePresentDrifted()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(Ducts, "BLD-ME-Ducts", 1, 4);
            return outcome;
        }

        // ---------- R19, a rebuild read back against the file ----------

        /// <summary>
        /// A rebuilt set read back still asking the old question, or not read back at all, is said as a
        /// rebuild that did not take. It was counted rebuilt every run. The guard: one read back asking
        /// what the file asks is not named.
        /// </summary>
        [Test]
        public void ARebuildThatDidNotTakeIsSaidAndOneThatTookIsNot()
        {
            SetBuildOutcome stillOld = OnePresentDrifted();
            stillOld.AddDrift(SetDriftTests.TheOldSpelling(), true, SetDriftTests.TheOldSpelling());

            SetBuildOutcome lost = OnePresentDrifted();
            lost.AddDrift(SetDriftTests.TheOldSpelling(), true, null);

            SetBuildOutcome took = OnePresentDrifted();
            took.AddDrift(SetDriftTests.TheOldSpelling(), true, SetDriftTests.AsksWhatTheFileAsks());

            Assert.That(stillOld.RebuiltNotTaken.Count, Is.EqualTo(1));
            Assert.That(lost.RebuiltNotTaken.Count, Is.EqualTo(1));
            Assert.That(took.RebuiltNotTaken.Count, Is.EqualTo(0));
            Assert.That(Joined(stillOld.Lines()), Does.Contain(
                "1 of the rebuilt were not read back asking what the file asks, so the rebuild did not take or whether it took is UNKNOWN"));
            Assert.That(stillOld.PutInLine(), Does.Contain("1 of the rebuilt not read back asking what the file asks"));
            Assert.That(Joined(took.Lines()), Does.Not.Contain("not read back"));
            Assert.That(stillOld.PutAnythingIn, Is.True, "a set replaced changed the document whether or not the rebuild took");
        }

        // ---------- R18, the box blamed only where it is off ----------

        /// <summary>
        /// With the box ticked and no rebuild done, the lines said none was rebuilt because the box is off,
        /// and named the old and new questions above, where the window shows no such line. The guard: with
        /// the box off the box is still named.
        /// </summary>
        [Test]
        public void ADriftedSetNotRebuiltBlamesTheBoxOnlyWhereItIsOff()
        {
            SetBuildOutcome ticked = OnePresentDrifted();
            ticked.RebuildsAsked = true;
            ticked.AddDrift(SetDriftTests.TheOldSpelling(), false, null);

            SetBuildOutcome off = OnePresentDrifted();
            off.AddDrift(SetDriftTests.TheOldSpelling(), false, null);

            Assert.That(Joined(ticked.Lines()), Does.Contain("none was rebuilt, though the box is ticked, so each rebuild failed or its set was not found"));
            Assert.That(Joined(ticked.Lines()), Does.Not.Contain("because the box is off"));
            Assert.That(Joined(off.Lines()), Does.Contain("none was rebuilt, because the box is off"));
            Assert.That(Joined(off.Lines()), Does.Contain("The run log names each with the old question and the new one"));
            Assert.That(Joined(off.Lines()), Does.Not.Contain("named above"));
            Assert.That(ticked.NotRebuiltCount, Is.EqualTo(1));
            Assert.That(off.NotRebuiltCount, Is.EqualTo(0));
        }

        // ---------- R2 and R22, a set put in and then lost track of ----------

        /// <summary>
        /// A set created or replaced and then lost track of was reported FAILED and asked no NWF save.
        /// The guard: a set that failed before it reached the document still asks none.
        /// </summary>
        [Test]
        public void ASetPutInAndThenLostAsksForTheSaveAndSaysItIsInTheDocument()
        {
            SetBuildOutcome putIn = new SetBuildOutcome();
            SetResult lost = putIn.AddFailedAfterPutIn(Ducts, "BLD-ME-Ducts", 1, "InvalidOperationException: gone");

            SetBuildOutcome before = new SetBuildOutcome();
            before.AddFailed(Ducts, "BLD-ME-Ducts", 1, "InvalidOperationException: gone");

            Assert.That(putIn.PutAnythingIn, Is.True);
            Assert.That(putIn.FailedCount, Is.EqualTo(1));
            Assert.That(putIn.FailedAfterPutInCount, Is.EqualTo(1));
            Assert.That(lost.Line(), Does.Contain("It was put into the document before it failed, so the document holds it"));
            Assert.That(Joined(putIn.Lines()), Does.Contain("1 of them after they were put into the document, so the NWF save is asked for"));
            Assert.That(putIn.PutInLine(), Does.Contain("1 put in and then FAILED"));
            Assert.That(before.PutAnythingIn, Is.False);
            Assert.That(before.FailedAfterPutInCount, Is.EqualTo(0));
        }

        // ---------- R7, the summary carries the drift counts ----------

        /// <summary>The window label and the SETS step phrase read clean over sets that drifted or could not be read.</summary>
        [Test]
        public void TheSummaryCarriesTheDriftedRebuiltAndNotReadCounts()
        {
            SetBuildOutcome outcome = OnePresentDrifted();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/BLD-AR-Floors", "BLD-AR-Floors", 1, 4);
            outcome.AddDrift(SetDriftTests.TheOldSpelling(), true, SetDriftTests.AsksWhatTheFileAsks());
            outcome.AddDrift(SetDrift.Compare(null, PlannedFloors(), "it holds no search"), false, null);

            SetBuildOutcome clean = OnePresentDrifted();

            Assert.That(outcome.Summary(), Does.Contain(", 1 drifted (1 rebuilt), 1 could not be read."));
            Assert.That(clean.Summary(), Does.Not.Contain("drifted"));
            Assert.That(clean.Summary(), Does.Not.Contain("could not be read"));
        }

        private static PlannedSet PlannedFloors()
        {
            return new PlannedSet(
                "BLD-AR-Floors",
                "lcop_selection_set_tree/A/BLD-AR-Floors",
                new List<string> { "A" },
                new List<PlannedCondition>
                {
                    new PlannedCondition(ConditionTest.Equals, 0, "LcRevitData_Element", "Element", EmptySets.CategoryProperty, null, "wstring", "Floors")
                });
        }

        // ---------- R15, the damaged document said in the window ----------

        /// <summary>The Build sets button showed nothing of a pair that failed between its two halves.</summary>
        [Test]
        public void ADamagedDocumentIsSaidFirstInTheLinesAndInTheSummary()
        {
            SetBuildOutcome damaged = OnePresentDrifted();
            damaged.TheDocumentIsDamaged = "the unused \"A\" was removed and \"B\" would not take its name";

            SetBuildOutcome whole = OnePresentDrifted();

            Assert.That(damaged.Lines()[0], Does.StartWith("THE DOCUMENT IS DAMAGED: the unused \"A\" was removed"));
            Assert.That(damaged.Lines()[0], Does.EndWith("Do not save it."));
            Assert.That(damaged.Summary(), Does.EndWith("The document is DAMAGED and nothing may be saved from it."));
            Assert.That(Joined(whole.Lines()), Does.Not.Contain("DAMAGED"));
            Assert.That(whole.Summary(), Does.Not.Contain("DAMAGED"));
        }

        // ---------- R6 and R4, the EMPTY SETS block ----------

        /// <summary>
        /// A present set at zero whose question could not be read was in the summary's at zero and not
        /// in the block, which said fewer sets found nothing and nothing of the rest. The guard: a set
        /// with a question is judged and not counted as unjudged.
        /// </summary>
        [Test]
        public void ASetAtZeroNotReadIsCountedInTheEmptySetsBlock()
        {
            EmptySetJudge judge = EmptySetJudge.For(SetBuildPlan.From(new List<SelectionSetDefinition>()), null, null, -1);
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult unread = outcome.AddAlreadyPresent(Ducts, "BLD-ME-Ducts", 1, 0);
            SetResult read = outcome.AddAlreadyPresent("lcop_selection_set_tree/A/BLD-AR-Floors", "BLD-AR-Floors", 1, 0);

            outcome.JudgeIfEmpty(unread, null, judge);
            outcome.JudgeIfEmpty(read, ReadCondition.Of(PlannedFloors()), judge);

            string block = Joined(EmptySets.Lines(outcome.Empty, 3, 10, 0, outcome.AtZeroNotJudged));

            Assert.That(outcome.AtZeroNotJudged, Is.EqualTo(1));
            Assert.That(outcome.Empty.Count, Is.EqualTo(1));
            Assert.That(block, Does.StartWith("2 set(s) found nothing in this group, and this is why:"));
            Assert.That(block, Does.Contain("1 were not judged, because what they ask could not be read off the document"));
            Assert.That(
                EmptySets.Lines(new List<EmptySet>(), 0, 10, 0, 1),
                Is.Not.Empty,
                "a block of unjudged sets alone is still written");
            Assert.That(Joined(EmptySets.Lines(outcome.Empty, 3, 10, 0, 0)), Does.Not.Contain("not judged"));
        }

        /// <summary>
        /// A picked XML of sets alone and the Build sets button judged the sets and wrote no block. The block
        /// with no test says so in place of a cost. The guard: no set at zero writes no block.
        /// </summary>
        [Test]
        public void TheEmptySetsBlockWithNoTestSaysNoCostWasCounted()
        {
            List<EmptySet> empty = new List<EmptySet> { EmptySets.Why(Ducts, null, null) };

            IList<string> lines = EmptySets.LinesWithNoTest(empty, 0);

            Assert.That(lines[0], Is.EqualTo("1 set(s) found nothing in this group, and this is why:"));
            Assert.That(lines[lines.Count - 1], Is.EqualTo(
                "no clash test ran with these sets here, so what they cost in clash tests is not counted"));
            Assert.That(Joined(lines), Does.Not.Contain("IT COSTS"));
            Assert.That(EmptySets.LinesWithNoTest(new List<EmptySet>(), 0), Is.Empty);
            Assert.That(EmptySets.LinesWithNoTest(null, 0), Is.Empty);
        }

        // ---------- R9, naming settings that will not read a name ----------

        /// <summary>
        /// The judge parsed the model names with settings that throw, outside any guard, so the group's
        /// catch took it and no set of the group was built. It says why it cannot tell instead. The guard:
        /// good settings read the project and the judge is complete for the lists' own project.
        /// </summary>
        [Test]
        public void NamingSettingsThatWillNotReadANameMakeTheJudgeSayWhyAndNeverThrow()
        {
            ContainerNameSettings bad = new ContainerNameSettings();
            bad.DisciplinePart = bad.BuildingPart;
            List<ModelExport> models = new List<ModelExport> { Walked("ME", "ME-DUCTWORK") };

            EmptySetJudge judge = null;
            Assert.DoesNotThrow(() => judge = EmptySetJudge.For(SetBuildPlan.From(new List<SelectionSetDefinition>()), models, bad, 1));
            EmptySet why = EmptySets.Why(Ducts, AsksWorkset("ST-NO-SUCH-WORKSET"), judge);

            Assert.That(judge.RunProject, Is.Null);
            Assert.That(why.Reason, Is.EqualTo(EmptyReason.CannotTell));
            Assert.That(why.WhyNotTold, Does.Contain("the naming settings will not read a model's name, BuildingPart and DisciplinePart cannot be the same position"));

            EmptySetJudge good = EmptySetJudge.For(SetBuildPlan.From(new List<SelectionSetDefinition>()), models, new ContainerNameSettings(), 1);
            Assert.That(good.RunProject, Is.EqualTo("1104"));
        }

        // ---------- R14, a model the export check dropped ----------

        /// <summary>
        /// The judge was handed the models the export check read and never how many the document holds,
        /// so a model the export check dropped was one it knew nothing of, and a workset only that model
        /// carries was called carried by no model. The guard: every model read, the same condition is wrong.
        /// </summary>
        [Test]
        public void AModelTheExportCheckDroppedMakesTheJudgeUnableToCallAWorksetWrong()
        {
            List<ModelExport> models = new List<ModelExport> { Walked("ME", "ME-DUCTWORK"), Walked("AR", "AR-EXTERIOR") };
            SetBuildPlan plan = SetBuildPlan.From(new List<SelectionSetDefinition>());

            EmptySet every = EmptySets.Why(Ducts, AsksWorkset("ST-NO-SUCH-WORKSET"), EmptySetJudge.For(plan, models, new ContainerNameSettings(), 2));
            EmptySet dropped = EmptySets.Why(Ducts, AsksWorkset("ST-NO-SUCH-WORKSET"), EmptySetJudge.For(plan, models, new ContainerNameSettings(), 3));

            Assert.That(every.Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
            Assert.That(dropped.Reason, Is.EqualTo(EmptyReason.CannotTell));
            Assert.That(dropped.WhyNotTold, Is.EqualTo(
                "1 model of this group was not read by the export check at all, so which worksets it carries is UNKNOWN"));
        }

        private static ModelExport Walked(string discipline, params string[] worksets)
        {
            return new ModelExport(
                "1104-PAR-1B06BC-ZZZ-" + discipline + "-MOD-000001.nwc", discipline, 10, 10, 10, new List<string>(worksets));
        }

        private static List<ReadCondition> AsksWorkset(string value)
        {
            return new List<ReadCondition> { new ReadCondition("LcRevitData_Element", EmptySets.WorksetProperty, "equals", value, 0) };
        }

        // ---------- R23 and R16, the names of the clash matrix ----------

        /// <summary>
        /// Two sets at one path in the clash matrix were built as one and counted as one created and one
        /// already there. The second is skipped by name. The guard: two names in one folder are both built.
        /// </summary>
        [Test]
        public void ASecondSetAtOnePathInThePickedFileIsSkippedByName()
        {
            SetBuildPlan plan = SetBuildPlan.From(new List<SelectionSetDefinition>
            {
                Set("BLD-ME-Ducts", "A"),
                Set("BLD-ME-Ducts", "A"),
                Set("BLD-ME-Pipes", "A"),
            });

            Assert.That(plan.Buildable.Count, Is.EqualTo(2));
            Assert.That(plan.Skipped.Count, Is.EqualTo(1));
            Assert.That(plan.Skipped[0].Path, Is.EqualTo(Ducts));
            Assert.That(plan.Skipped[0].Reason, Does.StartWith("the clash matrix holds another set at this path before this one"));
        }

        /// <summary>
        /// A set the plan skipped was read by the leftover walk as one the file does not name, and with the
        /// box ticked one nothing points at was removed. The guard: a name the file does not hold is still
        /// a leftover.
        /// </summary>
        [Test]
        public void ASetThePlanSkippedIsNamedByTheFileForTheLeftoverWalk()
        {
            SelectionSetDefinition unknownTest = new SelectionSetDefinition(
                "BLD-EL-Odd", new[] { "A" }, "lcop_selection_set_tree/A/BLD-EL-Odd", "all", false, null,
                new List<SearchConditionDefinition>
                {
                    new SearchConditionDefinition("starts with", 0, null, new NameReference(EmptySets.CategoryProperty, "Category"), new ValueReference("wstring", "Cable"))
                });
            SetBuildPlan plan = SetBuildPlan.From(new List<SelectionSetDefinition> { Set("BLD-ME-Ducts", "A"), unknownTest });

            List<DocumentSet> inDocument = new List<DocumentSet>
            {
                new DocumentSet("lcop_selection_set_tree/A/BLD-EL-Odd", "BLD-EL-Odd", new List<string>(), 0),
                new DocumentSet("lcop_selection_set_tree/A/BLD-OLD", "BLD-OLD", new List<string>(), 0),
            };

            IList<LeftoverSet> leftovers = SetLeftovers.For(inDocument, plan.NamesInTheFile());

            Assert.That(plan.Skipped.Count, Is.EqualTo(1));
            Assert.That(plan.NamesInTheFile(), Is.EqualTo(new[] { "BLD-ME-Ducts", "BLD-EL-Odd" }));
            Assert.That(leftovers.Count, Is.EqualTo(1));
            Assert.That(leftovers[0].Name, Is.EqualTo("BLD-OLD"));
        }

        private static SelectionSetDefinition Set(string name, params string[] folders)
        {
            return new SelectionSetDefinition(
                name, folders, "lcop_selection_set_tree/" + string.Join("/", folders) + "/" + name, "all", false, null, null);
        }

        // ---------- R20, the leftovers reported with the box off ----------

        /// <summary>
        /// With the box off the leftovers were neither decided nor reported. They are, and the block says
        /// nothing in it is done. The guard: with the box ticked the block says no such thing.
        /// </summary>
        [Test]
        public void TheLeftoversBlockWithTheBoxOffSaysNothingInItIsDone()
        {
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { new DocumentSet("lcop_selection_set_tree/A/BLD-OLD", "BLD-OLD", new List<string>(), 0) },
                new List<string> { "BLD-ME-Ducts" });

            string off = Joined(SetLeftovers.Lines(leftovers, false));
            string ticked = Joined(SetLeftovers.Lines(leftovers, true));

            Assert.That(off, Does.Contain(
                "NOTHING BELOW IS DONE, because \"" + SetRebuildSettings.TickLabel + "\" is off. Each line says what a run with it ticked would do"));
            Assert.That(ticked, Does.Not.Contain("NOTHING BELOW IS DONE"));
        }

        // ---------- attempt 2, the readings of attempt 1 ----------

        private static IList<LeftoverSet> APairAndAnUnusedSet()
        {
            List<string> keys = new List<string> { "ducts" };

            return SetLeftovers.For(
                new List<DocumentSet>
                {
                    new DocumentSet("lcop_selection_set_tree/A/BLD-OLD", "BLD-OLD", new List<string>(), 0),
                    new DocumentSet("lcop_selection_set_tree/A/BLD-ME-Ductwork", "BLD-ME-Ductwork", keys, 12),
                    new DocumentSet("lcop_selection_set_tree/B/BLD-ME-Ducts", "BLD-ME-Ducts", keys, 0),
                },
                new List<string> { "BLD-ME-Ducts" });
        }

        /// <summary>
        /// The breaker's finding 14. With the box off each leftover line still said it is removed and
        /// renamed into it. The guard: with the box ticked the lines read as they did.
        /// </summary>
        [Test]
        public void WithTheBoxOffEachLeftoverLineSaysWhatATickedRunWouldDo()
        {
            IList<LeftoverSet> leftovers = APairAndAnUnusedSet();
            string off = Joined(SetLeftovers.Lines(leftovers, false));
            string ticked = Joined(SetLeftovers.Lines(leftovers, true));

            Assert.That(off, Does.Contain("NOTHING points at it, so a run with the box ticked would remove it"));
            Assert.That(off, Does.Contain("a run with the box ticked would remove that one and rename this one into it"));
            Assert.That(off, Does.Contain("nothing points at, so they would be removed"));
            Assert.That(off, Does.Not.Contain("so it is removed"));
            Assert.That(off, Does.Not.Contain("so that one is removed"));
            Assert.That(ticked, Does.Contain("NOTHING points at it, so it is removed"));
            Assert.That(ticked, Does.Contain("so that one is removed and this one is renamed into it"));
        }

        /// <summary>
        /// The breaker's finding 1. The add-in removed and renamed the first set of a NAME where Core had
        /// decided by PATH. The leftover carries the twin's path and where the rename lands, so the add-in
        /// acts on the set at the path decided. The twin in another folder lands beside the leftover.
        /// </summary>
        [Test]
        public void ALeftoverCarriesItsTwinsPathAndWhereTheRenameLands()
        {
            LeftoverSet pair = APairAndAnUnusedSet()[1];

            Assert.That(pair.Action, Is.EqualTo(LeftoverAction.RemoveTheTwinThenRename));
            Assert.That(pair.TwinPath, Is.EqualTo("lcop_selection_set_tree/B/BLD-ME-Ducts"));
            Assert.That(pair.RenamedPath, Is.EqualTo("lcop_selection_set_tree/A/BLD-ME-Ducts"));
            Assert.That(APairAndAnUnusedSet()[0].TwinPath, Is.Empty);
        }

        /// <summary>
        /// The breaker's finding 4. A build that stopped for no active document read as a file holding no
        /// set. The guard: a file with no set still reads as one.
        /// </summary>
        [Test]
        public void ABuildThatStoppedSaysItStoppedAndWhy()
        {
            SetBuildOutcome stopped = new SetBuildOutcome();
            stopped.StoppedBecause = "there is no active document";

            Assert.That(stopped.Summary(), Is.EqualTo("The sets build STOPPED before any set, because there is no active document."));
            Assert.That(stopped.Lines()[0], Is.EqualTo("The sets build STOPPED before any set, because there is no active document."));
            Assert.That(new SetBuildOutcome().Summary(), Is.EqualTo("This file holds no sets. Nothing to build."));
        }

        /// <summary>
        /// The breaker's finding 5. After the document was damaged the leftovers after it were still
        /// acted on. Each is recorded as not acted on and counted, and the lines say how many.
        /// </summary>
        [Test]
        public void TheLeftoversAfterTheDamageAreCountedAsUntouched()
        {
            SetBuildOutcome outcome = OnePresentDrifted();
            outcome.TheDocumentIsDamaged = "the pair failed";

            foreach (LeftoverSet leftover in APairAndAnUnusedSet())
            {
                outcome.AddLeftoverUntouched(leftover);
            }

            Assert.That(outcome.LeftoversUntouched, Is.EqualTo(2));
            Assert.That(outcome.ActedOnLeftovers, Is.EqualTo(0));
            Assert.That(outcome.Lines()[1], Is.EqualTo(
                "2 more set(s) the file no longer names were left untouched, since nothing more is changed in a damaged document."));
            Assert.That(Joined(OnePresentDrifted().Lines()), Does.Not.Contain("left untouched"));
        }

        /// <summary>The breaker's finding 9. A reason of the judge reaches a row of the .tsv, so it is one line.</summary>
        [Test]
        public void TheJudgesReasonFromTheNamingSettingsIsOneLine()
        {
            ContainerNameSettings bad = new ContainerNameSettings();
            bad.ProjectPart = 0;
            List<ModelExport> models = new List<ModelExport> { Walked("ME", "ME-DUCTWORK") };

            EmptySetJudge judge = EmptySetJudge.For(SetBuildPlan.From(new List<SelectionSetDefinition>()), models, bad, 1);
            string why = EmptySets.Why(Ducts, AsksWorkset("ST-NO-SUCH-WORKSET"), judge).WhyNotTold;

            Assert.That(why, Does.Contain("the naming settings will not read a model's name, Part positions are one based. Parameter name: ProjectPart"));
            Assert.That(why, Does.Not.Contain("\n"));
            Assert.That(why, Does.Not.Contain("\r"));
        }

        /// <summary>
        /// The reviewer's finding 5. With the box off a side not read was a FAILURE on every weekly run. It
        /// is one finding line, its error on one line.
        /// </summary>
        [Test]
        public void AReadOfTheLeftoverWalkThatFailedWithTheBoxOffIsOneFindingLine()
        {
            string line = SetLeftovers.NotReadWithTheBoxOff(
                "reading what 2 clash test side(s) point at", "InvalidOperationException: first\r\nsecond");

            Assert.That(line, Does.StartWith("FINDING  reading what 2 clash test side(s) point at failed, InvalidOperationException: first second."));
            Assert.That(line, Does.EndWith("is off, so nothing would have been removed or renamed"));
            Assert.That(line, Does.Not.Contain("\n"));
            Assert.That(SetLeftovers.NotReadWithTheBoxOff(null, null), Does.Contain("why is UNKNOWN"));
        }
    }
}
