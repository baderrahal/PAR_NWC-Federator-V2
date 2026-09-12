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

            string all = string.Join(Environment.NewLine, new List<string>(outcome.Lines()).ToArray());
            Assert.That(all, Does.Contain("sets created      : 4"));
            Assert.That(all, Does.Contain("sets finding items: 2"));
            Assert.That(all, Does.Contain("sets at zero      : 2"));
            Assert.That(all, Does.Contain("items found       : 147"));
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
            Assert.That(all, Does.Contain("sets finding items: 0"));
            Assert.That(all, Does.Contain("sets at zero      : 0"));
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
        // ---------- the one summary line, F34 ----------

        [Test]
        public void TheSummaryCountsCreatedPresentFindingZeroFailedAndSkipped()
        {
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated("lcop_selection_set_tree/A/One", "One", 1, 12);
            outcome.AddCreated("lcop_selection_set_tree/A/Two", "Two", 1, 0);
            outcome.AddAlreadyPresent("lcop_selection_set_tree/A/Three", "Three", 2, 5);
            outcome.AddFailed("lcop_selection_set_tree/A/Four", "Four", 1, "it threw");

            Assert.That(outcome.Summary(),
                Is.EqualTo("2 created, 1 already there, 1 finding items, 1 at zero, 1 failed."));
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
