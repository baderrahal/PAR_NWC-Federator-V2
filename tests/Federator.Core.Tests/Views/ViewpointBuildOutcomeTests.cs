using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using Federator.Core.Views;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// The VIEWS BUILT block. Shaped on the SETS block on purpose, and for the same reason: the
    /// counts are read off the same list the lines come from, so the totals under a block
    /// can never disagree with the lines above them. Since F114's add-in pass a view is
    /// created or failed, never already there, because the views are made fresh every run.
    /// </summary>
    [TestFixture]
    public class ViewpointBuildOutcomeTests
    {
        [Test]
        public void NothingPlannedSaysSoRatherThanReadingAsAStepThatDidNothing()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            Assert.That(outcome.Summary(), Is.EqualTo("No view was planned for this group."));
            Assert.That(outcome.CreatedCount, Is.EqualTo(0));
            Assert.That(outcome.PutAnythingIn, Is.False);
        }

        [Test]
        public void ACreatedViewNamesWhatItShowsAndHowManyModelsItHides()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();
            ViewResult result = outcome.AddCreated("A/Structure vs Mechanical/BLD-ME-Ducts-vs-BLD-ST-Columns", "ME ST", 2);

            Assert.That(result.Line(), Does.StartWith("VIEW     A/Structure vs Mechanical/BLD-ME-Ducts-vs-BLD-ST-Columns  created"));
            Assert.That(result.Line(), Does.Contain("shows ME ST"));
            Assert.That(result.Line(), Does.Contain("hides 2 models"));
        }

        [Test]
        public void HidingOneReadsAsOneModelAndNotOneModels()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            Assert.That(outcome.AddCreated("A/Pair/Test", "ME", 1).Line(), Does.Contain("hides 1 model"));
            Assert.That(outcome.AddCreated("A/Pair/Other", "AR", 0).Line(), Does.Contain("hides 0 models"));
        }

        /// <summary>
        /// The break. A failure with no reason is what makes a RESULT block say something
        /// failed and then name nothing, which happened once already on this project.
        /// </summary>
        [Test]
        public void AFailureWithNoReasonGetsOneRatherThanReadingAsBlank()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();
            ViewResult result = outcome.AddFailed("A/Pair/Test", "ME", null);

            Assert.That(result.Failed, Is.True);
            Assert.That(result.Error, Does.Contain("UNKNOWN"));
            Assert.That(result.Line(), Does.Contain("FAILED"));
            Assert.That(result.Line(), Does.Not.EndWith("FAILED, "));
        }

        [Test]
        public void AFailedOneIsNotCreated()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            outcome.AddCreated("A/Pair/Test", "AR", 1);
            outcome.AddFailed("A/Pair/Other", "ME", "the folder could not be made");

            Assert.That(outcome.CreatedCount, Is.EqualTo(1));
            Assert.That(outcome.FailedCount, Is.EqualTo(1));
            Assert.That(outcome.PutAnythingIn, Is.True);
            Assert.That(outcome.Summary(), Is.EqualTo("1 created, 1 failed."));
        }

        [Test]
        public void AGroupWhoseEveryViewFailedAsksForNoSecondNwfSave()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            outcome.AddFailed("A/Pair/Test", "AR", "it was added and a fresh read does not show it");

            Assert.That(outcome.PutAnythingIn, Is.False, "nothing was created, so nothing new is in the document");
            Assert.That(outcome.Summary(), Is.EqualTo("0 created, 1 failed."));
        }

        /// <summary>
        /// The counts and the lines come from one list, so a block cannot report three
        /// created over two lines. This is the fault the RESULT block had once, saying
        /// groups failed 22 and Nothing failed in the same block.
        /// </summary>
        [Test]
        public void TheTotalsAreCountedOffTheSameListTheLinesCameFrom()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            outcome.AddCreated("A/Pair/One", "AR", 2);
            outcome.AddCreated("A/Pair/Two", "ME", 2);
            outcome.AddFailed("A/Pair/Three", "EL", "the API said no");

            IList<string> lines = outcome.Lines();
            int viewLines = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("VIEW     "))
                {
                    viewLines++;
                }
            }

            Assert.That(viewLines, Is.EqualTo(outcome.Results.Count));
            Assert.That(lines, Does.Contain("views created     : 2"));
            Assert.That(lines, Does.Contain("views that failed : 1"));
        }

        [Test]
        public void TheBlockEndsWithWhatWentIntoTheDocument()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();

            outcome.AddCreated("A/Pair/One", "AR", 1);
            outcome.AddCreated("A/Pair/Two", "ME", 1);

            IList<string> lines = outcome.Lines();

            Assert.That(lines[lines.Count - 1], Is.EqualTo("put into the document: 2 created"));
        }

        [Test]
        public void NothingFailedMeansNoFailedLineInTheBlock()
        {
            ViewpointBuildOutcome outcome = new ViewpointBuildOutcome();
            outcome.AddCreated("A/Pair/One", "AR", 0);

            foreach (string line in outcome.Lines())
            {
                Assert.That(line, Does.Not.Contain("views that failed"));
            }
        }

        /// <summary>
        /// A group can put a hundred views in, so the block names five and counts the rest,
        /// at the number and one past it, and a FAILED one is always named wherever it sits,
        /// because every failure says something different.
        /// </summary>
        [Test]
        public void TheBlockNamesFiveAndCountsTheRestAndEveryFailureIsNamed()
        {
            ViewpointBuildOutcome at = new ViewpointBuildOutcome();

            for (int i = 0; i < RunLog.KeptOfARepeat; i++)
            {
                at.AddCreated("A/AR vs ST/view " + i, "AR ST", 2);
            }

            string atText = string.Join("\n", new List<string>(at.Lines()).ToArray());
            Assert.That(atText, Does.Contain("view " + (RunLog.KeptOfARepeat - 1)));
            Assert.That(atText, Does.Not.Contain("counted and not listed"));

            ViewpointBuildOutcome past = new ViewpointBuildOutcome();

            for (int i = 0; i <= RunLog.KeptOfARepeat; i++)
            {
                past.AddCreated("A/AR vs ST/view " + i, "AR ST", 2);
            }

            past.AddFailed("A/AR vs ST/view broken", "AR ST", "the API said no");

            string pastText = string.Join("\n", new List<string>(past.Lines()).ToArray());
            Assert.That(pastText, Does.Not.Contain("view " + RunLog.KeptOfARepeat + "  "));
            Assert.That(pastText, Does.Contain("and 1 more created, counted and not listed"));
            Assert.That(pastText, Does.Contain("view broken  FAILED"));
            Assert.That(pastText, Does.Contain("views created     : " + (RunLog.KeptOfARepeat + 1)));
            Assert.That(pastText, Does.Contain("views that failed : 1"));
        }
    }
}
