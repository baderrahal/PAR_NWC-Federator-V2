using System;
using System.Collections.Generic;
using Federator.Core.Exchange;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The totals have to agree with the lines, because a total that disagrees with what
    /// was logged is worse than no total at all.
    /// </summary>
    [TestFixture]
    public class SetBuildOutcomeTests
    {
        private static SetBuildOutcome WithSampleResults()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("lcop_selection_set_tree/A/One", "One", 2, 140);
            outcome.AddCreated("lcop_selection_set_tree/A/Two", "Two", 1, 0);
            outcome.AddCreated("lcop_selection_set_tree/B/Three", "Three", 4, 7);
            outcome.AddCreated("lcop_selection_set_tree/B/Four", "Four", 1, 0);
            return outcome;
        }

        [Test]
        public void EverySetContributesOneLine()
        {
            SetBuildOutcome outcome = WithSampleResults();
            IList<string> lines = outcome.Lines();

            int setLines = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("ok      ", StringComparison.Ordinal)
                    || line.StartsWith("ZERO    ", StringComparison.Ordinal))
                {
                    setLines++;
                }
            }

            Assert.That(setLines, Is.EqualTo(4));
        }

        [Test]
        public void ALineCarriesThePathTheConditionCountAndTheItemCount()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult result = outcome.AddCreated("lcop_selection_set_tree/Mechanical/HVAC/Ducts", "Ducts", 2, 140);

            Assert.That(result.Line(), Does.Contain("lcop_selection_set_tree/Mechanical/HVAC/Ducts"));
            Assert.That(result.Line(), Does.Contain("2 conditions"));
            Assert.That(result.Line(), Does.Contain("140 items"));
        }

        [Test]
        public void ASingleConditionAndASingleItemReadAsSingular()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            Assert.That(outcome.AddCreated("p", "n", 1, 1).Line(), Does.Contain("1 condition  1 item"));
        }

        // A set that finds nothing is worth knowing about. It is not an error and it is
        // not hidden.
        [Test]
        public void ASetAtZeroIsMarkedZeroAndNamedRatherThanHidden()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult result = outcome.AddCreated("lcop_selection_set_tree/A/Empty", "Empty", 1, 0);

            Assert.That(result.IsZero, Is.True);
            Assert.That(result.Created, Is.True, "zero items is not a failure");
            Assert.That(result.Line(), Does.StartWith("ZERO"));
            Assert.That(result.Line(), Does.Contain("Empty"));
            Assert.That(outcome.Lines(), Has.Some.Contains("ZERO"));
        }

        [Test]
        public void TheTotalsMatchTheLinesThatWereLogged()
        {
            SetBuildOutcome outcome = WithSampleResults();

            Assert.That(outcome.CreatedCount, Is.EqualTo(4));
            Assert.That(outcome.FindingItemsCount, Is.EqualTo(2));
            Assert.That(outcome.ZeroCount, Is.EqualTo(2));
            Assert.That(outcome.CreatedCount, Is.EqualTo(outcome.FindingItemsCount + outcome.ZeroCount),
                "created should be exactly those finding items plus those at zero");
            Assert.That(outcome.TotalItems, Is.EqualTo(147));

            // The labels sit under the count of the sets created since FR-022, which said
            // nowhere that they counted only those. The numbers are the same.
            string all = string.Join(Environment.NewLine, new List<string>(outcome.Lines()).ToArray());
            Assert.That(all, Does.Contain(
                "sets created      : 4" + Environment.NewLine
                + "   finding items  : 2" + Environment.NewLine
                + "   at zero        : 2" + Environment.NewLine
                + "   items found    : 147"));
        }

        [Test]
        public void TheCountedTotalsAreDerivedFromTheSameListAsTheLines()
        {
            SetBuildOutcome outcome = WithSampleResults();

            int zeroLines = 0;
            int okLines = 0;

            foreach (string line in outcome.Lines())
            {
                if (line.StartsWith("ZERO", StringComparison.Ordinal)) { zeroLines++; }
                if (line.StartsWith("ok", StringComparison.Ordinal)) { okLines++; }
            }

            Assert.That(zeroLines, Is.EqualTo(outcome.ZeroCount));
            Assert.That(okLines, Is.EqualTo(outcome.FindingItemsCount));
        }

        [Test]
        public void AFailedSetIsCountedApartFromZeroAndCarriesItsReason()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("p/ok", "ok", 1, 5);
            SetResult failed = outcome.AddFailed("p/bad", "bad", 3, "the category could not be built");

            Assert.That(failed.Created, Is.False);
            Assert.That(failed.IsZero, Is.False, "a failure must never be counted as a zero");
            Assert.That(failed.ItemCount, Is.EqualTo(-1));
            Assert.That(failed.Line(), Does.StartWith("FAILED"));
            Assert.That(failed.Line(), Does.Contain("the category could not be built"));

            Assert.That(outcome.FailedCount, Is.EqualTo(1));
            Assert.That(outcome.CreatedCount, Is.EqualTo(1));
            Assert.That(outcome.ZeroCount, Is.EqualTo(0));

            string all = string.Join(Environment.NewLine, new List<string>(outcome.Lines()).ToArray());
            Assert.That(all, Does.Contain("sets that failed  : 1"));
        }

        [Test]
        public void ASkippedSetIsReportedWithItsReasonAndCountedApart()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets>"
                + "<selectionset name=\"Bad\"><findspec mode=\"all\" disjoint=\"0\"><conditions>"
                + "<condition test=\"wildcard\" flags=\"0\">"
                + "<property><name internal=\"P\">P</name></property>"
                + "<value><data type=\"wstring\">v</data></value></condition>"
                + "</conditions><locator>/</locator></findspec></selectionset>"
                + "</selectionsets></exchange>");

            SetBuildPlan plan = SetBuildPlan.From(document);
            SetBuildOutcome outcome = new SetBuildOutcome();

            foreach (SkippedSet skipped in plan.Skipped)
            {
                outcome.AddSkipped(skipped);
            }

            Assert.That(outcome.SkippedCount, Is.EqualTo(1));
            Assert.That(outcome.CreatedCount, Is.EqualTo(0));

            string all = string.Join(Environment.NewLine, new List<string>(outcome.Lines()).ToArray());
            Assert.That(all, Does.Contain("SKIPPED"));
            Assert.That(all, Does.Contain("wildcard"));
            Assert.That(all, Does.Contain("sets skipped      : 1"));
        }

        // A zero is not an error, but it is useless without knowing what was asked. Bader
        // ran the reference file against a model and 53 of 60 sets came back at zero, and
        // the log did not say what any of them had looked for.
        [Test]
        public void AZeroLineSaysWhatTheSetAskedFor()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult result = outcome.AddCreated(
                "lcop_selection_set_tree/Architecture/BLD-AR-Roofs", "BLD-AR-Roofs", 1, 0,
                "LcRevitData_Element/LcRevitPropertyElementCategory equals \"Roofs\"");

            Assert.That(result.IsZero, Is.True);
            Assert.That(result.Line(), Does.StartWith("ZERO"));
            Assert.That(result.Line(), Does.Contain("asked for"));
            Assert.That(result.Line(), Does.Contain("LcRevitPropertyElementCategory equals \"Roofs\""));
        }

        [Test]
        public void ASetThatFoundItemsDoesNotRepeatWhatItAskedFor()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult result = outcome.AddCreated("p", "n", 1, 2564, "something equals \"Walls\"");

            Assert.That(result.Line(), Does.StartWith("ok"));
            Assert.That(result.Line(), Does.Not.Contain("asked for"),
                "a set that worked does not need explaining");
        }

        [Test]
        public void AZeroWithNothingRecordedStillReadsCleanly()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            Assert.That(outcome.AddCreated("p", "n", 1, 0).Line(), Does.StartWith("ZERO"));
            Assert.That(outcome.AddCreated("p", "n", 1, 0).Line(), Does.Not.Contain("asked for"));
        }

        [Test]
        public void AnOutcomeWithNothingInItStillReportsZeroTotals()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            string all = string.Join(Environment.NewLine, new List<string>(outcome.Lines()).ToArray());

            Assert.That(all, Does.Contain("sets created      : 0"));
            Assert.That(all, Does.Contain("   finding items  : 0"));
            Assert.That(all, Does.Contain("   at zero        : 0"));
            Assert.That(all, Does.Not.Contain("sets that failed"));
            Assert.That(all, Does.Not.Contain("sets skipped"));
        }

        /// <summary>
        /// B1. The second NWF save is decided by whether the build put anything into the
        /// document. Sets already there were left alone, so they do not count against
        /// the ones created. A rerun that finds sixty present and creates one still put
        /// one in, and the engine once said it had built nothing in exactly that case.
        /// </summary>
        [Test]
        public void OneCreatedAmongManyAlreadyPresentStillPutSomethingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("lcop_selection_set_tree/A/New", "New", 1, 3);

            for (int i = 0; i < 60; i++)
            {
                outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Old" + i, "Old" + i, 1, 40);
            }

            Assert.That(outcome.CreatedCount, Is.EqualTo(1));
            Assert.That(outcome.AlreadyPresentCount, Is.EqualTo(60));
            Assert.That(outcome.PutAnythingIn, Is.True);
        }

        [Test]
        public void EverySetAlreadyPresentPutNothingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/One", "One", 1, 12);
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Two", "Two", 2, 0);

            Assert.That(outcome.PutAnythingIn, Is.False);
        }

        [Test]
        public void AFailedSetPutNothingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddFailed("lcop_selection_set_tree/A/One", "One", 1, "threw");

            Assert.That(outcome.PutAnythingIn, Is.False);
        }

        [Test]
        public void ASetCreatedAtZeroItemsStillPutSomethingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("lcop_selection_set_tree/A/One", "One", 1, 0);

            Assert.That(outcome.PutAnythingIn, Is.True);
        }

        // F28. A present set is one call, printed as present with its item count, counted
        // as present and never as created. BuildOne used to add it to both lists, so a
        // weekly run reported sixty one created and saved the NWF a second time for nothing.
        [Test]
        public void SixtyOnePresentAndNoneCreatedPutNothingInAndPrintsNoOkLine()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < 61; i++)
            {
                outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Set" + i, "Set" + i, 2, 10 + i);
            }

            Assert.That(outcome.PutAnythingIn, Is.False);
            Assert.That(outcome.CreatedCount, Is.EqualTo(0));
            Assert.That(outcome.AlreadyPresentCount, Is.EqualTo(61));
            Assert.That(outcome.FindingItemsCount, Is.EqualTo(0));
            Assert.That(outcome.ZeroCount, Is.EqualTo(0));
            Assert.That(outcome.FailedCount, Is.EqualTo(0));
            Assert.That(outcome.TotalItems, Is.EqualTo(0));

            foreach (string line in outcome.Lines())
            {
                Assert.That(line, Does.Not.StartWith("ok"), "a present set is not an ok line");
            }

            string all = string.Join("\n", new List<string>(outcome.Lines()).ToArray());
            Assert.That(all, Does.Contain("present lcop_selection_set_tree/A/Set0  2 conditions  10 items  already there, left alone"));
            Assert.That(all, Does.Contain("sets created      : 0"));
            Assert.That(all, Does.Contain("already there     : 61"));
        }

        [Test]
        public void SixtyPresentAndOneCreatedPutSomethingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < 60; i++)
            {
                outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Set" + i, "Set" + i, 1, 5);
            }

            outcome.AddCreated("lcop_selection_set_tree/A/New", "New", 1, 3);

            Assert.That(outcome.PutAnythingIn, Is.True);
            Assert.That(outcome.CreatedCount, Is.EqualTo(1));
            Assert.That(outcome.AlreadyPresentCount, Is.EqualTo(60));
            Assert.That(outcome.FindingItemsCount, Is.EqualTo(1), "only the created set counts as finding items");
            Assert.That(outcome.TotalItems, Is.EqualTo(3), "the present sets do not add their items");
        }

        /// <summary>
        /// A SET REBUILT FROM THE PICKED FILE CHANGED THE DOCUMENT, FR-020. ReplaceWithCopy puts
        /// a new set in the slot, and with nothing created the sets step did not ask for the
        /// NWF to be saved, so where no test was created or run the NWD was published from the
        /// rebuilt document and the NWF on disk kept the old sets.
        /// </summary>
        [Test]
        public void ADriftRebuiltWithNothingCreatedPutSomethingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Ducts", "Ducts", 1, 12);
            outcome.AddDrift(OneDrift("lcop_selection_set_tree/A/Ducts"), true);

            Assert.That(outcome.CreatedCount, Is.EqualTo(0));
            Assert.That(outcome.RebuiltCount, Is.EqualTo(1));
            Assert.That(outcome.PutAnythingIn, Is.True);
        }

        /// <summary>
        /// The window's totals say a rebuilt set was rebuilt, as the run log's line does, and never
        /// that every set already there was left alone, FR-020.
        /// </summary>
        [Test]
        public void TheTotalsSayARebuiltSetWasRebuiltAndNotLeftAlone()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Ducts", "Ducts", 1, 12);
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Pipes", "Pipes", 1, 7);
            outcome.AddDrift(OneDrift("lcop_selection_set_tree/A/Ducts"), true);

            string lines = string.Join("\n", new List<string>(outcome.Lines()).ToArray());

            Assert.That(lines, Does.Contain("already there     : 2, 1 of them rebuilt from the picked file and 1 left alone, not copied again"));
            Assert.That(lines, Does.Not.Contain("2, left alone"));
        }

        /// <summary>With nothing rebuilt the totals keep their words.</summary>
        [Test]
        public void TheTotalsKeepTheirWordsWhereNothingWasRebuilt()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Ducts", "Ducts", 1, 12);

            Assert.That(
                string.Join("\n", new List<string>(outcome.Lines()).ToArray()),
                Does.Contain("already there     : 1, left alone, not copied again"));
        }

        /// <summary>A drift left alone, the box off, changed nothing.</summary>
        [Test]
        public void ADriftLeftAlonePutNothingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Ducts", "Ducts", 1, 12);
            outcome.AddDrift(OneDrift("lcop_selection_set_tree/A/Ducts"), false);

            Assert.That(outcome.PutAnythingIn, Is.False);
        }

        /// <summary>
        /// A leftover removed or renamed changed the document too, which the engine added beside
        /// this answer. Now the one answer holds every change the sets step makes, FR-020.
        /// </summary>
        [Test]
        public void ALeftoverActedOnPutSomethingIn()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            IList<LeftoverSet> leftovers = SetLeftovers.For(
                new List<DocumentSet> { new DocumentSet("a/Old", "Old", new List<string> { "k" }, 0) },
                new List<string> { "New" });

            outcome.AddLeftover(leftovers[0], false);
            Assert.That(outcome.PutAnythingIn, Is.False, "a leftover not acted on changed nothing");

            outcome.AddLeftover(leftovers[0], true);
            Assert.That(outcome.PutAnythingIn, Is.True);
        }

        /// <summary>
        /// A SET THIS RUN REBUILT IS NEVER CALLED LEFT ALONE, the reviewer's finding on attempt 1.
        /// The engine's line said every set already there was left alone, and since FR-020 a set
        /// rebuilt from the picked file is one that changed the document, so with the box ticked
        /// the line called a set it had just replaced left alone. The line is Core's and carries
        /// the rebuilt count apart from the ones left alone.
        /// </summary>
        [Test]
        public void TheLineOfWhatWasPutInSaysTheRebuiltSetsApartFromTheOnesLeftAlone()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Ducts", "Ducts", 1, 12);
            outcome.AddDrift(OneDrift("lcop_selection_set_tree/A/Ducts"), true);
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Pipes", "Pipes", 1, 3);

            Assert.That(outcome.PutInLine(), Is.EqualTo(
                "put into the document: 0 created, 2 already there, 1 of them rebuilt from the picked file and 1 left alone"));
        }

        /// <summary>With nothing rebuilt every set already there was left alone, and a leftover acted on is said after.</summary>
        [Test]
        public void WithNothingRebuiltTheSetsAlreadyThereAreLeftAlone()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("lcop_selection_set_tree/A/One", "One", 1, 12);
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Ducts", "Ducts", 1, 12);
            outcome.AddDrift(OneDrift("lcop_selection_set_tree/A/Ducts"), false);
            outcome.AddLeftover(
                SetLeftovers.For(
                    new List<DocumentSet> { new DocumentSet("a/Old", "Old", new List<string> { "k" }, 0) },
                    new List<string> { "New" })[0],
                true);

            Assert.That(outcome.PutInLine(), Is.EqualTo(
                "put into the document: 1 created, 1 already there and left alone, 1 of 1 set(s) the file no longer names brought up to date"));
        }

        private static SetDrift OneDrift(string path)
        {
            return SetDrift.Compare(
                new List<ReadCondition> { new ReadCondition("LcRevitData_Element", EmptySets.WorksetProperty, "equals", "ME-DUCTWORK") },
                new PlannedSet(
                    path.Substring(path.LastIndexOf('/') + 1),
                    path,
                    new List<string>(),
                    new List<PlannedCondition>
                    {
                        new PlannedCondition(
                            ConditionTest.Equals, 0, "LcRevitData_Element", "Element", EmptySets.WorksetProperty, "Workset", "wstring", "ME-Ductwork")
                    }));
        }

        [Test]
        public void APresentLineCarriesItsItemCountAndIsNotCreated()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult result = outcome.AddAlreadyPresent("lcop_selection_set_tree/B/Ducts", "Ducts", 1, 140);

            Assert.That(result.Present, Is.True);
            Assert.That(result.Created, Is.False);
            Assert.That(result.IsZero, Is.False);
            Assert.That(result.Line(), Is.EqualTo("present lcop_selection_set_tree/B/Ducts  1 condition  140 items  already there, left alone"));
        }
        // ---------- a count that could not be taken, FR-018 ----------

        /// <summary>A judge for a group of the project the lists inside Core were measured on, FR-011.</summary>
        private static EmptySetJudge OnThisProject()
        {
            return new EmptySetJudge(RevitWorksets.With(null), RevitWorksets.Project, null);
        }

        private static readonly List<ReadCondition> AsksTheOldSpelling = new List<ReadCondition>
        {
            new ReadCondition("LcRevitData_Element", EmptySets.WorksetProperty, "equals", "ME-DUCTWORK")
        };

        /// <summary>
        /// A PRESENT SET WHOSE COUNT IS UNKNOWN FOUND NOTHING NOBODY KNOWS OF, FR-018. A set
        /// rebuilt and not found again was recorded at 0 items, judged empty on the question it
        /// asked before the rebuild and counted at zero across the run. Minus one is UNKNOWN: it
        /// is not judged, not counted at zero, and its line says UNKNOWN.
        /// </summary>
        [Test]
        public void APresentSetWhoseCountIsUnknownIsNotJudgedEmptyNorCountedAtZero()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult present = outcome.AddAlreadyPresent("a/Mechanical/BLD-ME-Ducts", "BLD-ME-Ducts", 1, -1);

            outcome.JudgeIfEmpty(present, AsksTheOldSpelling, OnThisProject());

            Assert.That(outcome.Empty, Is.Empty);
            Assert.That(outcome.ZeroCount, Is.EqualTo(0));
            Assert.That(present.Line(), Is.EqualTo("present a/Mechanical/BLD-ME-Ducts  1 condition  UNKNOWN items  already there, left alone"));

            SetsAcrossTheRun run = new SetsAcrossTheRun();
            run.Add(outcome);
            Assert.That(run.All(), Is.Empty, "a set whose count is UNKNOWN is in neither count across the run");
        }

        /// <summary>A present set at zero is judged, and one whose question could not be read is not.</summary>
        [Test]
        public void APresentSetAtZeroIsJudgedAndOneNotReadIsNot()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            outcome.JudgeIfEmpty(outcome.AddAlreadyPresent("a/One", "One", 1, 0), AsksTheOldSpelling, OnThisProject());
            outcome.JudgeIfEmpty(outcome.AddAlreadyPresent("a/Two", "Two", 1, 0), null, OnThisProject());
            outcome.JudgeIfEmpty(outcome.AddAlreadyPresent("a/Three", "Three", 1, 5), AsksTheOldSpelling, OnThisProject());

            Assert.That(outcome.Empty.Count, Is.EqualTo(1));
            Assert.That(outcome.Empty[0].Path, Is.EqualTo("a/One"));
        }

        /// <summary>
        /// NO JUDGE, NO JUDGEMENT, F128. The Generic Models sets are built with no judge, so a set of
        /// theirs at nought is in no EMPTY list, present or created, and its count still reads nought.
        /// </summary>
        [Test]
        public void ASetAtZeroBuiltWithNoJudgeIsNotJudged()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            outcome.JudgeIfEmpty(outcome.AddAlreadyPresent("a/One", "One", 1, 0), AsksTheOldSpelling, null);
            SetResult created = outcome.AddCreated(PlannedAsking("Two", "Generic Models"), 0, null);

            Assert.That(outcome.Empty, Is.Empty, "a set built with no judge is judged by nobody");
            Assert.That(created.IsZero, Is.True, "its count is still nought");
            Assert.That(outcome.ZeroCount, Is.EqualTo(1));
        }

        // ---------- a created set judged, FR-027 ----------

        private static PlannedSet PlannedAsking(string name, string category)
        {
            return new PlannedSet(
                name,
                "lcop_selection_set_tree/Electrical/" + name,
                new List<string> { "Electrical" },
                new List<PlannedCondition>
                {
                    new PlannedCondition(
                        ConditionTest.Equals, 0, "LcRevitData_Element", "Element", EmptySets.CategoryProperty, "Category", "wstring", category)
                });
        }

        /// <summary>
        /// A CREATED SET THAT FOUND NOTHING IS JUDGED, FR-027. Only a set already in the NWF was
        /// judged, so no EMPTY SETS block was ever written on a first run, the run that creates
        /// every set, and set 03's 22 groups had sets at zero in every one. It is judged on what
        /// the file asks, read into Core's condition shape, and a created set that found items is
        /// not judged.
        /// </summary>
        [Test]
        public void ACreatedSetAtZeroLandsInEmptyWithAReason()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            SetResult zero = outcome.AddCreated(PlannedAsking("BLD-EL-Nurse Call", "Nurse Call Devices"), 0, OnThisProject());
            outcome.AddCreated(PlannedAsking("BLD-EL-Lighting Fixtures", "Lighting Fixtures"), 49, OnThisProject());

            Assert.That(zero.IsZero, Is.True);
            Assert.That(zero.Line(), Does.Contain("asked for LcRevitData_Element/LcRevitPropertyElementCategory (Category) equals \"Nurse Call Devices\""));
            Assert.That(outcome.Empty.Count, Is.EqualTo(1));
            Assert.That(outcome.Empty[0].Path, Is.EqualTo("lcop_selection_set_tree/Electrical/BLD-EL-Nurse Call"));
            Assert.That(outcome.Empty[0].Reason, Is.EqualTo(EmptyReason.NoModelCarriesTheValue));
            Assert.That(outcome.Empty[0].Asked, Is.EqualTo("Nurse Call Devices"));
        }

        /// <summary>
        /// A CREATED SET WHOSE COUNT COULD NOT BE TAKEN IS UNKNOWN AND NEVER ZERO, the breaker's
        /// finding on attempt 1. The add-in turned a null read of what a set finds into 0 items, the
        /// shape FR-018 was written against, so such a set would be judged empty and counted at
        /// zero. NotCounted is said UNKNOWN, is in no zero count and no item total, and is never
        /// judged. It was summed into the items found as minus one.
        /// </summary>
        [Test]
        public void ACreatedSetWhoseCountCouldNotBeTakenIsUnknownAndNeverZero()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated(PlannedAsking("BLD-EL-Phones", "Telephone Equipment"), SetResult.NotCounted, OnThisProject());

            Assert.That(outcome.ZeroCount, Is.EqualTo(0));
            Assert.That(outcome.FindingItemsCount, Is.EqualTo(0));
            Assert.That(outcome.TotalItems, Is.EqualTo(0));
            Assert.That(outcome.Empty, Is.Empty);
            Assert.That(outcome.Results[0].Line(), Is.EqualTo(
                "ok      lcop_selection_set_tree/Electrical/BLD-EL-Phones  1 condition  UNKNOWN items"));
            Assert.That(outcome.Summary(), Is.EqualTo("1 created (0 finding items, 0 at zero, 1 not counted), 0 already there."));
            Assert.That(outcome.Lines(), Does.Contain("   not counted    : 1, could not be counted, so what they find is UNKNOWN"));
        }

        /// <summary>The planned conditions read into Core's condition shape, flags and test kept, FR-027.</summary>
        [Test]
        public void APlannedSetReadsIntoTheConditionsTheJudgeReads()
        {
            PlannedSet planned = new PlannedSet(
                "BLD-EL-Devices",
                "a/BLD-EL-Devices",
                new List<string>(),
                new List<PlannedCondition>
                {
                    new PlannedCondition(ConditionTest.Contains, 0, "LcRevitData_Element", "Element", EmptySets.CategoryProperty, "Category", "wstring", "Devices"),
                    new PlannedCondition(ConditionTest.Equals, PlannedCondition.NegateFlag, null, null, "LcOaNodeSourceFile", "Source File", "wstring", "-AR-")
                });

            IList<ReadCondition> read = ReadCondition.Of(planned);

            Assert.That(read.Count, Is.EqualTo(2));
            Assert.That(read[0].Key(), Is.EqualTo(planned.Conditions[0].Key()));
            Assert.That(read[1].Key(), Is.EqualTo(planned.Conditions[1].Key()));
            Assert.That(read[1].CategoryInternalName, Is.Empty);
            Assert.That(read[1].Flags, Is.EqualTo(PlannedCondition.NegateFlag));
        }

        // ---------- the one summary line, F34 ----------

        [Test]
        public void TheSummaryCountsCreatedPresentFindingZeroFailedAndSkipped()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("lcop_selection_set_tree/A/One", "One", 1, 12);
            outcome.AddCreated("lcop_selection_set_tree/A/Two", "Two", 1, 0);
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Three", "Three", 2, 5);
            outcome.AddFailed("lcop_selection_set_tree/A/Four", "Four", 1, "it threw");

            // The finding and zero counts sit beside the kind of set they count since FR-022.
            Assert.That(outcome.Summary(),
                Is.EqualTo("2 created (1 finding items, 1 at zero), 1 already there (1 finding items, 0 at zero), 1 failed."));
        }

        /// <summary>
        /// THE SETS ALREADY THERE ARE COUNTED TOO, FR-022. A weekly run where every set is
        /// already there read 0 finding items and 0 at zero while the present sets found
        /// thousands of items and many found none, because both counts were of the sets this
        /// build created, and nothing in the words said so. The created counts stay as they are.
        /// </summary>
        [Test]
        public void SixtyOnePresentWithThirtyEightAtZeroDoNotReadAsNoneFindingAndNoneAtZero()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < 61; i++)
            {
                outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Set" + i, "Set" + i, 1, i < 38 ? 0 : 100);
            }

            Assert.That(outcome.FindingItemsCount, Is.EqualTo(0), "the created count stays");
            Assert.That(outcome.ZeroCount, Is.EqualTo(0), "the created count stays");
            Assert.That(outcome.PresentFindingItemsCount, Is.EqualTo(23));
            Assert.That(outcome.PresentZeroCount, Is.EqualTo(38));
            Assert.That(outcome.Summary(), Is.EqualTo("0 created, 61 already there (23 finding items, 38 at zero)."));

            string all = string.Join(Environment.NewLine, new List<string>(outcome.Lines()).ToArray());
            Assert.That(all, Does.Contain(
                "   finding items  : 23" + Environment.NewLine
                + "   at zero        : 38" + Environment.NewLine
                + "   items found    : 2300"));
        }

        /// <summary>A present set whose count is UNKNOWN, FR-018, is in neither count and is said apart.</summary>
        [Test]
        public void APresentSetNotCountedIsInNeitherPresentCount()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("a/One", "One", 1, 0);
            outcome.AddAlreadyPresent("a/Two", "Two", 1, SetResult.NotCounted);

            Assert.That(outcome.PresentZeroCount, Is.EqualTo(1));
            Assert.That(outcome.PresentFindingItemsCount, Is.EqualTo(0));
            Assert.That(outcome.Summary(), Is.EqualTo("0 created, 2 already there (0 finding items, 1 at zero, 1 not counted)."));
        }

        [Test]
        public void NothingBuiltAndNothingSkippedIsAFileHoldingNoSets()
        {
            Assert.That(new SetBuildOutcome().Summary(),
                Is.EqualTo("This file holds no sets. Nothing to build."));
        }

        [Test]
        public void NothingBuiltWithSetsSkippedSaysHowMany()
        {
            ExchangeDocument document = new ExchangeReader().ReadText(
                "<exchange units=\"ft\"><selectionsets>"
                + "<selectionset name=\"Odd\"><findspec mode=\"all\"><conditions>"
                + "<condition test=\"wildcard\" flags=\"10\">"
                + "<property><name internal=\"n\">Name</name></property>"
                + "<value><data type=\"wstring\">v</data></value></condition>"
                + "</conditions><locator>/</locator></findspec></selectionset>"
                + "</selectionsets></exchange>");

            SetBuildOutcome outcome = new SetBuildOutcome();

            foreach (SkippedSet skipped in SetBuildPlan.From(document).Skipped)
            {
                outcome.AddSkipped(skipped);
            }

            Assert.That(outcome.Summary(), Is.EqualTo("No set in this file can be rebuilt. 1 skipped."));
        }
    }
}
