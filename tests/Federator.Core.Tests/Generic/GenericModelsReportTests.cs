using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// A set already in the document, as the builder records it: counted, and carrying the question the
        /// document's own set asks, read off it the way the add-in reads it, the conditions and the prose
        /// SetDrift.AskedNow writes of them, which carries no display names. What the plan asks unless the
        /// test hands other conditions.
        /// </summary>
        private static SetResult Present(SetBuildOutcome outcome, GenericModelsPlan plan, int model, int items)
        {
            return Present(outcome, plan, model, items, ReadCondition.Of(plan.Sets[model].Set));
        }

        private static SetResult Present(SetBuildOutcome outcome, GenericModelsPlan plan, int model, int items, IList<ReadCondition> asks)
        {
            GenericModelSet set = plan.Sets[model];
            SetResult result = outcome.AddAlreadyPresent(set.Set.Path, set.ModelName, set.Set.ConditionCount, items);
            SetDrift drift = SetDrift.Compare(asks, set.Set);

            result.Asked = drift.AskedNow();
            result.AskedConditions = asks;
            return result;
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
            Present(outcome, plan, 0, 12);
            outcome.AddCreated(Path(plan, 1), "b", 2, 0);
            Present(outcome, plan, 2, 3);

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
            Present(outcome, plan, 0, 5);

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
            Present(outcome, plan, 2, SetResult.NotCounted);
            Present(outcome, plan, 3, 4);
            Present(outcome, plan, 3, 9);
            Present(outcome, plan, 4, 7);

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

            string text = Text(report);

            Assert.That(text, Does.Contain("items   : UNKNOWN, no model was counted"), "a total over no count is not at least nought");
            Assert.That(text, Does.Not.Contain("at least"));
            Assert.That(text, Does.Not.Contain("nought  :"), "nothing was counted at nought");
        }

        /// <summary>A null result and a result with no path are ignored and count for no model.</summary>
        [Test]
        public void ANullResultAndAResultWithNoPathCountForNoModel()
        {
            GenericModelsPlan plan = Plan("a.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            Present(outcome, plan, 0, 6);

            List<SetResult> results = new List<SetResult>(outcome.Results);
            results.Insert(0, null);
            results.Insert(0, new SetResult(null, "pathless", 2, 99, null, null));

            GenericModelsReport report = GenericModelsReport.From(plan, results);

            Assert.That(report.Items, Is.EqualTo(6));
            Assert.That(report.NotCounted, Is.EqualTo(0));
        }

        // ---------- a set already there that asks something else ----------

        /// <summary>
        /// A set already in the NWF keeps the question it was built with unless the rebuild box is ticked, so a plan
        /// changed after the measurement meets sets that ask the old one. Their count answers another question and
        /// is not the plan's, so it is not counted, with the question it asks. The break: the same set asking what
        /// the plan asks is counted.
        /// </summary>
        [Test]
        public void APresentSetThatAsksAnotherQuestionIsNotCounted()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            Present(outcome, plan, 0, 8);
            Present(outcome, plan, 1, 0, new List<ReadCondition>
            {
                new ReadCondition("LcRevitData_Element", "LcRevitPropertyElementCategory", "equals", "Furniture", 0),
                new ReadCondition(string.Empty, "LcOaNodeSourceFile", "contains", "b", 0)
            });

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(Of(report, "a").Counted, Is.True);
            Assert.That(Of(report, "b").Counted, Is.False, "a zero for another question is not a zero for this one");
            Assert.That(report.WithNone, Is.EqualTo(0));
            Assert.That(report.NotCounted, Is.EqualTo(1));
            Assert.That(Of(report, "b").WhyNotCounted, Does.Contain("asks something other than the plan asks"));
            Assert.That(Of(report, "b").WhyNotCounted, Does.Contain("equals \"Furniture\""));
            Assert.That(Of(report, "b").WhyNotCounted, Does.Contain("equals \"Generic Models\""));
        }

        /// <summary>
        /// THE WEEKLY RERUN, the readers' finding on attempt 1. The question read off a present set is said
        /// without display names, SetDrift.AskedNow, and the plan's with them, PlannedSet.Describe, so a
        /// compare of the two words called every present set a set asking another question and every
        /// model was UNKNOWN after the first run. The set is compared by the keys SetDrift compares on.
        /// </summary>
        [Test]
        public void APresentSetAskingWhatThePlanAsksIsCountedThoughItsWordsCarryNoDisplayNames()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            SetResult present = Present(outcome, plan, 0, 528);
            Present(outcome, plan, 1, 0);

            Assert.That(present.Asked, Is.Not.EqualTo(plan.Sets[0].Set.Describe()), "the two prose forms differ, which is the trap");
            Assert.That(present.Asked, Does.Not.Contain("(Category)"));
            Assert.That(plan.Sets[0].Set.Describe(), Does.Contain("(Category)"));

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(Of(report, "a").Counted, Is.True, "a present set asking the plan's question is counted");
            Assert.That(Of(report, "a").Items, Is.EqualTo(528));
            Assert.That(Of(report, "b").Counted, Is.True);
            Assert.That(report.WithNone, Is.EqualTo(1));
            Assert.That(report.NotCounted, Is.EqualTo(0));
        }

        /// <summary>A present set whose value would not read is a question not read, by the same rule, FR-017.</summary>
        [Test]
        public void APresentSetWhoseValueWouldNotReadIsNotCounted()
        {
            GenericModelsPlan plan = Plan("a.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            Present(outcome, plan, 0, 4, new List<ReadCondition>
            {
                new ReadCondition("LcRevitData_Element", "LcRevitPropertyElementCategory", "equals", "Generic Models", 0),
                ReadCondition.Unread(string.Empty, "LcOaNodeSourceFile", "contains", 0, "the value would not read")
            });

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(Of(report, "a").Counted, Is.False);
            Assert.That(Of(report, "a").WhyNotCounted, Does.Contain("the question it asks was not read"));
        }

        [Test]
        public void TheFailedBlockSaysWhatThrewThatTheCountIsUnknownAndThatTheGroupKeepsItsResult()
        {
            IList<string> lines = GenericModelsReport.FailedLines("planning the Generic Models sets", "InvalidOperationException: the model would not read");

            Assert.That(lines.Count, Is.EqualTo(2));
            Assert.That(lines[0], Is.EqualTo("FAILED  planning the Generic Models sets threw InvalidOperationException: the model would not read, "
                + "so no model of this group is counted, its count is UNKNOWN and the group keeps its own result"));
            Assert.That(lines[1], Is.EqualTo(GenericModelsReport.NoClashTest));
            Assert.That(GenericModelsReport.FailedLines(null, null)[0], Does.Contain("the Generic Models count threw UNKNOWN"));
        }

        [Test]
        public void APresentSetWhoseQuestionWasNotReadIsNotCountedAndACreatedSetIsCounted()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc", "c.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddAlreadyPresent(Path(plan, 0), "a", 2, 4);
            SetResult unread = outcome.AddAlreadyPresent(Path(plan, 1), "b", 2, 4);
            unread.Asked = string.Empty;
            outcome.AddCreated(Path(plan, 2), "c", 2, 5);

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);

            Assert.That(Of(report, "a").Counted, Is.False);
            Assert.That(Of(report, "a").WhyNotCounted, Does.Contain("the question it asks was not read"));
            Assert.That(Of(report, "b").Counted, Is.False);
            Assert.That(Of(report, "c").Items, Is.EqualTo(5), "a set this run created asks what the plan asked");
        }

        // ---------- what a nought means ----------

        /// <summary>
        /// A model left out as nought is also one whose items carry another text, and the output cannot tell the
        /// two apart, so the block says what a nought means, and says more where every model is at nought. The
        /// break: with no model at nought neither line is there.
        /// </summary>
        [Test]
        public void ANoughtSaysWhatItCanMeanAndEveryModelAtNoughtSaysMore()
        {
            GenericModelsPlan plan = Plan("a.nwc", "b.nwc");
            SetBuildOutcome some = new SetBuildOutcome();
            Present(some, plan, 0, 3);
            Present(some, plan, 1, 0);
            string text = Text(GenericModelsReport.From(plan, some.Results));

            Assert.That(text, Does.Contain("nought  : " + GenericModelsReport.NoughtMeans));
            Assert.That(text, Does.Not.Contain(GenericModelsReport.EveryModelAtNought));

            SetBuildOutcome none = new SetBuildOutcome();
            Present(none, plan, 0, 0);
            Present(none, plan, 1, 0);
            string all = Text(GenericModelsReport.From(plan, none.Results));

            Assert.That(all, Does.Contain("nought  : " + GenericModelsReport.EveryModelAtNought));

            SetBuildOutcome full = new SetBuildOutcome();
            Present(full, plan, 0, 3);
            Present(full, plan, 1, 2);

            Assert.That(Text(GenericModelsReport.From(plan, full.Results)), Does.Not.Contain("nought  :"));
        }

        // ---------- the total over sets whose texts meet ----------

        /// <summary>
        /// Where one set's text also finds another set's items the total adds an item twice, so it is not a count of
        /// items and not a lower bound. The break: with texts that do not meet it is the plain total.
        /// </summary>
        [Test]
        public void ATotalOverSetsWhoseTextsMeetIsNotACountOfItems()
        {
            GenericModelsPlan plan = Plan("A-ME.nwc", "A-ME2.nwc");
            SetBuildOutcome outcome = new SetBuildOutcome();
            Present(outcome, plan, 0, 14);
            Present(outcome, plan, 1, 4);

            GenericModelsReport report = GenericModelsReport.From(plan, outcome.Results);
            string text = Text(report);

            Assert.That(report.SetsOverlap, Is.True);
            Assert.That(text, Does.Contain("items   : 18 added over the sets, which is not a count of items, because the texts of some sets meet"));
            Assert.That(text, Does.Not.Contain("in all"));

            SetBuildOutcome partial = new SetBuildOutcome();
            Present(partial, plan, 0, 14);

            Assert.That(Text(GenericModelsReport.From(plan, partial.Results)), Does.Contain("and the models not counted are not in it"));
            Assert.That(Text(GenericModelsReport.From(plan, partial.Results)), Does.Not.Contain("at least"), "an overlapped sum is not a lower bound either");

            GenericModelsPlan apart = Plan("a.nwc", "b.nwc");
            SetBuildOutcome plain = new SetBuildOutcome();
            Present(plain, apart, 0, 2);
            Present(plain, apart, 1, 3);

            Assert.That(GenericModelsReport.From(apart, plain.Results).SetsOverlap, Is.False);
            Assert.That(Text(GenericModelsReport.From(apart, plain.Results)), Does.Contain("items   : 5 in all"));
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
            Present(outcome, plan, 0, 1234);
            Present(outcome, plan, 1, 0);
            Present(outcome, plan, 2, 3);

            string text = Text(GenericModelsReport.From(plan, outcome.Results));

            Assert.That(text, Does.Contain("asked   : Category equals \"Generic Models\", found in each model by its Source File"));
            Assert.That(text, Does.Contain("models  : 3 models in this group, 2 found some, 1 found none and are left out, 0 were not counted"));
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
            Present(outcome, plan, 0, 10);

            string text = Text(GenericModelsReport.From(plan, outcome.Results));

            Assert.That(text, Does.Contain("1 was not counted"));
            Assert.That(text, Does.Contain("items   : at least 10 in all, which is the models counted and not the ones that were not"));
            Assert.That(text, Does.Contain("    b  UNKNOWN, its set was not built, or no result of it was handed in"));

            SetBuildOutcome whole = new SetBuildOutcome();
            Present(whole, plan, 0, 10);
            Present(whole, plan, 1, 0);

            Assert.That(Text(GenericModelsReport.From(plan, whole.Results)), Does.Not.Contain("at least"));
        }

        [Test]
        public void ThePlansNotesAreCarriedIntoTheBlock()
        {
            GenericModelsPlan plan = Plan("A-ME.nwc", "A-ME2.nwc");
            string text = Text(GenericModelsReport.From(plan, new List<SetResult>()));

            Assert.That(text, Does.Contain("note    : the text A-ME that finds the model A-ME is also in the text of the model A-ME2"));
        }

        [Test]
        public void AGroupWithNoModelSaysSoAndCountsNothing()
        {
            string text = Text(GenericModelsReport.From(Plan(), new List<SetResult>()));

            Assert.That(text, Does.Contain("models  : none, the plan holds no model of this group, so nothing was counted"));
            Assert.That(text, Does.Not.Contain("items   :"));
        }

        /// <summary>The block's title is typed once under src, so nothing else can spell it another way.</summary>
        [Test]
        public void TheBlockTitleIsTypedOnceUnderSrc()
        {
            int found = 0;

            foreach (string file in Directory.GetFiles(System.IO.Path.Combine(Samples.Repo(), "src"), "*.cs", SearchOption.AllDirectories))
            {
                if (file.IndexOf(System.IO.Path.DirectorySeparatorChar + "obj" + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal) >= 0
                    || file.IndexOf(System.IO.Path.DirectorySeparatorChar + "bin" + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                string text = File.ReadAllText(file);

                for (int at = text.IndexOf("\"" + GenericModelsReport.BlockTitle + "\"", StringComparison.Ordinal);
                    at >= 0;
                    at = text.IndexOf("\"" + GenericModelsReport.BlockTitle + "\"", at + 1, StringComparison.Ordinal))
                {
                    found++;
                }
            }

            Assert.That(GenericModelsReport.BlockTitle, Is.Not.Empty);
            Assert.That(found, Is.EqualTo(1));
        }
    }
}
