using System;
using System.Collections.Generic;
using Federator.Core.Generic;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// How many Generic Models items each model of a group holds, F128 and FR-177: each model with its
    /// count, a model with none left out, and a count nobody took UNKNOWN and never nought.
    /// </summary>
    [TestFixture]
    public class GenericModelsReportTests
    {
        private static GenericModelsPlan Plan(params string[] files)
        {
            List<GenericModelInput> models = new List<GenericModelInput>();

            foreach (string file in files)
            {
                models.Add(new GenericModelInput(file));
            }

            return GenericModelsPlan.For(models, new GenericModelsSettings());
        }

        private static string Path(GenericModelsPlan plan, int model)
        {
            return plan.Sets[model].Set.Path;
        }

        private static string Text(GenericModelsReport report)
        {
            return string.Join("\n", new List<string>(report.Lines()).ToArray());
        }

        private static GenericModelCount Of(GenericModelsReport report, string model)
        {
            foreach (GenericModelCount count in report.Counts)
            {
                if (count.ModelName == model)
                {
                    return count;
                }
            }

            throw new InvalidOperationException(model + " was not counted");
        }

        // ---------- the counts ----------

        [Test]
        public void EachModelIsCountedOffTheResultOfItsOwnSet()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc", "c.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(Path(plan, 0), "a", 2, 12);
            outcome.AddCreated(Path(plan, 1), "b", 2, 0);
            outcome.AddAlreadyPresent(Path(plan, 2), "c", 2, 3);

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(report.Models, Is.EqualTo(3));
            Assert.That(report.WithItems, Is.EqualTo(2));
            Assert.That(report.WithNone, Is.EqualTo(1));
            Assert.That(report.NotCounted, Is.EqualTo(0));
            Assert.That(report.Items, Is.EqualTo(15));
            Assert.That(Of(report, "a").Items, Is.EqualTo(12));
            Assert.That(Of(report, "b").Items, Is.EqualTo(0));
            Assert.That(Of(report, "b").Counted, Is.True);
        }

        /// <summary>A set of another path, one the plan does not hold, counts for no model.</summary>
        [Test]
        public void AResultOfAnotherSetCountsForNoModel()
        {
            GenericModelsPlan plan = Plan("a.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent("lcop_selection_set_tree/Architecture/BLD-AR-Walls", "BLD-AR-Walls", 2, 400);
            outcome.AddAlreadyPresent(Path(plan, 0), "a", 2, 5);

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(report.Items, Is.EqualTo(5));
        }

        // ---------- a count nobody took ----------

        /// <summary>
        /// Each way a count is not taken is a model not counted with the reason, and none is nought: a set
        /// that was not built, a result not handed in, a count of minus one, and two results on one path.
        /// </summary>
        [Test]
        public void AModelWhoseCountWasNotTakenIsNeverNought()
        {
            GenericModelsPlan plan = Plan("failed.nwc", "missing.nwc", "minus.nwc", "twice.nwc", "good.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddFailed(Path(plan, 0), "failed", 2, "ArgumentException: bad value");
            outcome.AddAlreadyPresent(Path(plan, 2), "minus", 2, SetResult.NotCounted);
            outcome.AddAlreadyPresent(Path(plan, 3), "twice", 2, 4);
            outcome.AddAlreadyPresent(Path(plan, 3), "twice", 2, 9);
            outcome.AddAlreadyPresent(Path(plan, 4), "good", 2, 7);

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(report.NotCounted, Is.EqualTo(4));
            Assert.That(report.WithNone, Is.EqualTo(0), "not counted is not none");
            Assert.That(report.WithItems, Is.EqualTo(1));
            Assert.That(Of(report, "failed").Counted, Is.False);
            Assert.That(Of(report, "failed").Items, Is.EqualTo(GenericModelCount.NotCountedItems));
            Assert.That(Of(report, "failed").WhyNotCounted, Is.EqualTo("its set could not be built"));
            Assert.That(Of(report, "failed").WhyNotCounted, Does.Not.Contain("ArgumentException"), "no framework message in what a sheet carries");
            Assert.That(Of(report, "missing").WhyNotCounted, Does.Contain("not built, or no result of it was handed in"));
            Assert.That(Of(report, "minus").WhyNotCounted, Is.EqualTo("the count of its set was not taken"));
            Assert.That(Of(report, "twice").WhyNotCounted, Does.Contain("holds 2 results"));
        }

        [Test]
        public void NoResultsAtAllCountsEveryModelAsNotCounted()
        {
            GenericModelsReport report = GenericModelsReport.From(Plan("a.nwc", "b.nwc"), null);

            Assert.That(report.NotCounted, Is.EqualTo(2));
            Assert.That(report.WithNone, Is.EqualTo(0));
            Assert.That(report.Items, Is.EqualTo(0));
        }

        [Test]
        public void NoPlanIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => GenericModelsReport.From(null, new List<SetResult>()));
        }

        // ---------- the block ----------

        [Test]
        public void TheBlockNamesEachModelWithItemsAndLeavesOutTheOnesWithNone()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc", "c.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(Path(plan, 0), "a", 2, 1234);
            outcome.AddAlreadyPresent(Path(plan, 1), "b", 2, 0);
            outcome.AddAlreadyPresent(Path(plan, 2), "c", 2, 3);

            string text = Text(GenericModelsReport.From(plan, outcome.Results));

            Assert.That(text, Does.Contain("asked   : Category equals \"Generic Models\", found in each model by its Source File"));
            Assert.That(text, Does.Contain("models  : 3 models in this group, 2 hold some, 1 hold none and are left out, 0 were not counted"));
            Assert.That(text, Does.Contain("items   : 1,237 in all"));
            Assert.That(text, Does.Contain("    a  1,234"));
            Assert.That(text, Does.Contain("    c  3"));
            Assert.That(text, Does.Not.Contain("    b  "), "a model with none is counted above and not listed");
            Assert.That(text, Does.Contain("no clash test is made for these sets"));
        }

        /// <summary>
        /// A model not counted is listed with UNKNOWN and why, the total says at least, and neither is a
        /// nought. The break: with every model counted the total does not say at least.
        /// </summary>
        [Test]
        public void AModelNotCountedIsUnknownInTheBlockAndTheTotalIsALowerBound()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(Path(plan, 0), "a", 2, 10);

            string text = Text(GenericModelsReport.From(plan, outcome.Results));

            Assert.That(text, Does.Contain("1 was not counted"));
            Assert.That(text, Does.Contain("items   : at least 10 in all, which is the models counted and not the ones that were not"));
            Assert.That(text, Does.Contain("    b  UNKNOWN, its set was not built, or no result of it was handed in"));

            SetBuildOutcome whole = new SetBuildOutcome();
            whole.AddAlreadyPresent(Path(plan, 0), "a", 2, 10);
            whole.AddAlreadyPresent(Path(plan, 1), "b", 2, 0);

            Assert.That(Text(GenericModelsReport.From(plan, whole.Results)), Does.Not.Contain("at least"));
        }

        [Test]
        public void ThePlansNotesAreCarriedIntoTheBlock()
        {
            GenericModelsPlan plan = Plan("A-ME.nwc", "A-ME2.nwc");
            string text = Text(GenericModelsReport.From(plan, new List<SetResult>()));

            Assert.That(text, Does.Contain("note    : the text A-ME that finds the model A-ME is also in the text A-ME2"));
        }

        [Test]
        public void AGroupWithNoModelSaysSoAndCountsNothing()
        {
            string text = Text(GenericModelsReport.From(Plan(), new List<SetResult>()));

            Assert.That(text, Does.Contain("models  : none, the plan holds no model of this group, so nothing was counted"));
            Assert.That(text, Does.Not.Contain("items   :"));
        }

        [Test]
        public void TheBlockHasOneTitleAndNothingElseSpellsIt()
        {
            Assert.That(GenericModelsReport.BlockTitle, Is.EqualTo("GENERIC MODELS"));
        }
    }
}
