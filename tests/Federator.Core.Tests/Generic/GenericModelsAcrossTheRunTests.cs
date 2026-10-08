using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Federator.Core.Diagnostics;
using Federator.Core.Generic;
using Federator.Core.Sets;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The Generic Models counts added up over the run and the one line RESULT writes of them, F128 and
    /// FR-177. A count nobody took is UNKNOWN and never nought, here as in the block, and the line is
    /// written by RunLog off the tally and never by the test.
    /// </summary>
    [TestFixture]
    public class GenericModelsAcrossTheRunTests
    {
        private string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = TempFolder.Make("FederatorGenericAcrossTheRun");
        }

        [TearDown]
        public void RemoveFolder()
        {
            TempFolder.Remove(folder);
        }

        /// <summary>One group's report, each model counted at the number given, or not counted where it is negative.</summary>
        private static GenericModelsReport Report(params int[] counts)
        {
            List<GenericModelInput> models = new List<GenericModelInput>();

            for (int i = 0; i < counts.Length; i++)
            {
                models.Add(new GenericModelInput("model" + i + ".nwc"));
            }

            GenericModelsPlan plan = GenericModelsPlan.For(models, new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();

            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] < 0)
                {
                    outcome.AddFailed(plan.Sets[i].Set.Path, "model" + i, 2, "it threw");
                }
                else
                {
                    outcome.AddCreated(plan.Sets[i].Set, counts[i], null);
                }
            }

            return GenericModelsReport.From(plan, outcome.Results);
        }

        /// <summary>A group whose two models are both found by one text, so its sets overlap.</summary>
        private static GenericModelsReport Overlapping()
        {
            GenericModelsPlan plan = GenericModelsPlan.For(
                new[] { new GenericModelInput("a.nwc", "shared"), new GenericModelInput("b.nwc", "shared") },
                new GenericModelsSettings());
            SetBuildOutcome outcome = new SetBuildOutcome();
            outcome.AddCreated(plan.Sets[0].Set, 4, null);
            outcome.AddCreated(plan.Sets[1].Set, 4, null);
            return GenericModelsReport.From(plan, outcome.Results);
        }

        /// <summary>The RESULT block alone, read off the file while the log is open.</summary>
        private static string ResultOf(RunLog log)
        {
            string text;

            using (FileStream stream = new FileStream(log.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                text = reader.ReadToEnd();
            }

            string title = "RESULT" + Environment.NewLine + "================";
            int at = text.LastIndexOf(title, StringComparison.Ordinal);

            Assert.That(at, Is.GreaterThanOrEqualTo(0), "the log holds no RESULT block");
            return text.Substring(at);
        }

        // ---------- the tally ----------

        [Test]
        public void TheCountsAreAddedOverTheGroups()
        {
            GenericModelsAcrossTheRun run = new GenericModelsAcrossTheRun();
            run.Add(Report(528, 0, 3));
            run.Add(Report(0, 0));

            Assert.That(run.Groups, Is.EqualTo(2));
            Assert.That(run.Models, Is.EqualTo(5));
            Assert.That(run.WithItems, Is.EqualTo(2));
            Assert.That(run.WithNone, Is.EqualTo(3));
            Assert.That(run.NotCounted, Is.EqualTo(0));
            Assert.That(run.Items, Is.EqualTo(531));
            Assert.That(run.ResultLine(), Is.EqualTo(
                "generic models : 531 items in 2 models of 5 over 2 groups, 3 at nought, 0 not counted"));
        }

        /// <summary>A model not counted is UNKNOWN and never nought, so the items are at least and the model is counted apart.</summary>
        [Test]
        public void AModelNotCountedMakesTheItemsAtLeastAndIsNeverNought()
        {
            GenericModelsAcrossTheRun run = new GenericModelsAcrossTheRun();
            run.Add(Report(6, -1));

            Assert.That(run.NotCounted, Is.EqualTo(1));
            Assert.That(run.Items, Is.EqualTo(6));
            Assert.That(run.ResultLine(), Is.EqualTo(
                "generic models : at least 6 items in 1 model of 2 over 1 group, 0 at nought, 1 not counted"));
        }

        [Test]
        public void EveryModelNotCountedReadsUnknownAndNeverNought()
        {
            GenericModelsAcrossTheRun run = new GenericModelsAcrossTheRun();
            run.Add(Report(-1, -1));

            Assert.That(run.ResultLine(), Is.EqualTo("generic models : UNKNOWN, 2 models in 1 group and none was counted"));
            Assert.That(run.ResultLine(), Does.Not.Contain("0 items"));
        }

        /// <summary>Where the texts of some sets meet the total is not a count of items, because an item two sets find is in both counts.</summary>
        [Test]
        public void WhereTheTextsMeetTheTotalIsNotACountOfItems()
        {
            GenericModelsAcrossTheRun run = new GenericModelsAcrossTheRun();
            run.Add(Overlapping());
            run.Add(Report(3));

            Assert.That(run.GroupsWhoseTextsMeet, Is.EqualTo(1));
            Assert.That(run.ResultLine(), Is.EqualTo(
                "generic models : 11 added over the sets, which is not a count of items because the texts of some sets meet in 1 group"
                    + " in 3 models of 3 over 2 groups, 0 at nought, 0 not counted"));
        }

        /// <summary>A group that reached no count is named and is in no number, and a run with no group says so.</summary>
        [Test]
        public void AGroupThatReachedNoCountIsNamedAndARunWithNoGroupSaysSo()
        {
            GenericModelsAcrossTheRun run = new GenericModelsAcrossTheRun();

            Assert.That(run.ResultLine(), Is.EqualTo("generic models : UNKNOWN, no group reached the sets step, so nothing was counted"));

            run.AddNotCounted("1A02MM");
            Assert.That(run.ResultLine(), Is.EqualTo("generic models : UNKNOWN, no group was counted, and no count was taken in 1 group: 1A02MM"));

            run.Add(Report(3));
            run.AddNotCounted(null);
            Assert.That(run.GroupsNotCounted, Is.EqualTo(new[] { "1A02MM", "UNKNOWN" }));
            Assert.That(run.ResultLine(), Is.EqualTo(
                "generic models : 3 items in 1 model of 1 over 1 group, 0 at nought, 0 not counted, and no count was taken in 2 groups: 1A02MM, UNKNOWN"));
            Assert.That(run.Groups, Is.EqualTo(1), "a group not counted is not a group counted");
        }

        [Test]
        public void ANullReportIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericModelsAcrossTheRun().Add(null));
        }

        // ---------- RESULT ----------

        /// <summary>RESULT carries the line off the tally, and where none was handed in it says so rather than leaving the line out.</summary>
        [Test]
        public void ResultCarriesTheLineAndSaysWhereNoCountWasHandedIn()
        {
            GenericModelsAcrossTheRun run = new GenericModelsAcrossTheRun();
            run.Add(Report(528, 0, 3));

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs"), new DateTime(2026, 10, 8, 3, 0, 0)))
            {
                log.WriteResultBlock(null, generic: run);

                Assert.That(ResultOf(log), Does.Contain(
                    "generic models : 531 items in 2 models of 3 over 1 group, 1 at nought, 0 not counted"));
            }

            using (RunLog log = RunLog.Start(Path.Combine(folder, "logs2"), new DateTime(2026, 10, 8, 3, 0, 0)))
            {
                log.WriteResultBlock(null);

                Assert.That(ResultOf(log), Does.Contain("generic models : UNKNOWN, no Generic Models count was handed in"));
            }
        }
    }
}
