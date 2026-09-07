using System.Collections.Generic;
using Federator.Core.Rerun;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// F22. Which of the two workflows a group takes, in the words the window shows. The
    /// window once said nothing about it and the confirm dialog said the document was
    /// cleared before every group, which is only true of a First run.
    /// </summary>
    [TestFixture]
    public class RunPathTests
    {
        [Test]
        public void TheFiveLabelsAreExactlyTheseWords()
        {
            Assert.That(RunPath.FirstRun, Is.EqualTo("First run"));
            Assert.That(RunPath.WeeklyRun, Is.EqualTo("Weekly run"));
            Assert.That(RunPath.WeeklyRunPlusXml, Is.EqualTo("Weekly run plus XML"));
            Assert.That(RunPath.Skipped, Is.EqualTo("Skipped (changed on disk)"));
            Assert.That(RunPath.Unknown, Is.EqualTo("Unknown"));
            Assert.That(RunPath.All, Has.Length.EqualTo(5));
        }

        [Test]
        public void BuildIsAFirstRunWithOrWithoutAnXml()
        {
            Assert.That(RunPath.Label(RerunDecision.Build, false), Is.EqualTo(RunPath.FirstRun));
            Assert.That(RunPath.Label(RerunDecision.Build, true), Is.EqualTo(RunPath.FirstRun));
        }

        [Test]
        public void OpenIsAWeeklyRunAndAnXmlMakesItPlusXml()
        {
            Assert.That(RunPath.Label(RerunDecision.Open, false), Is.EqualTo(RunPath.WeeklyRun));
            Assert.That(RunPath.Label(RerunDecision.Open, true), Is.EqualTo(RunPath.WeeklyRunPlusXml));
        }

        [Test]
        public void ChangedIsSkippedWithOrWithoutAnXml()
        {
            Assert.That(RunPath.Label(RerunDecision.Changed, false), Is.EqualTo(RunPath.Skipped));
            Assert.That(RunPath.Label(RerunDecision.Changed, true), Is.EqualTo(RunPath.Skipped));
        }

        [Test]
        public void ADecisionTheRuleCannotNameIsUnknownRatherThanGuessed()
        {
            Assert.That(RunPath.Label((RerunDecision)99, false), Is.EqualTo(RunPath.Unknown));
            Assert.That(RunPath.Label((RerunDecision)99, true), Is.EqualTo(RunPath.Unknown));
        }

        [Test]
        public void BeforeTheRunTheNwfOnDiskDecidesAndChangedNeverShows()
        {
            Assert.That(RunPath.Expected(false, false), Is.EqualTo(RunPath.FirstRun));
            Assert.That(RunPath.Expected(false, true), Is.EqualTo(RunPath.FirstRun));
            Assert.That(RunPath.Expected(true, false), Is.EqualTo(RunPath.WeeklyRun));
            Assert.That(RunPath.Expected(true, true), Is.EqualTo(RunPath.WeeklyRunPlusXml));
        }

        [Test]
        public void TheCountsCarryEveryLabelAtZeroWhereNoneHasIt()
        {
            IDictionary<string, int> counts = RunPath.Count(
                new[] { RunPath.FirstRun, RunPath.WeeklyRun, RunPath.WeeklyRun, "nonsense", null });

            Assert.That(counts[RunPath.FirstRun], Is.EqualTo(1));
            Assert.That(counts[RunPath.WeeklyRun], Is.EqualTo(2));
            Assert.That(counts[RunPath.WeeklyRunPlusXml], Is.EqualTo(0));
            Assert.That(counts[RunPath.Skipped], Is.EqualTo(0));
            Assert.That(counts[RunPath.Unknown], Is.EqualTo(2), "a label nobody knows counts as Unknown");
        }

        [Test]
        public void TheConfirmLinesCountEachLabelAndSayClearedOnlyForFirstRuns()
        {
            IList<string> lines = RunPath.ConfirmLines(
                new[] { RunPath.FirstRun, RunPath.FirstRun, RunPath.WeeklyRun });
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(lines[0], Is.EqualTo("This run federates 3 groups."));
            Assert.That(all, Does.Contain("First run: 2"));
            Assert.That(all, Does.Contain("Weekly run: 1"));
            Assert.That(all, Does.Contain("Weekly run plus XML: 0"));
            Assert.That(all, Does.Contain("Skipped (changed on disk): 0"));
            Assert.That(all, Does.Contain("cleared before each one"));
            Assert.That(all, Does.Not.Contain("Unknown"), "no unknown line when no group is unknown");
        }

        [Test]
        public void WithNoFirstRunTheDialogNeverSaysCleared()
        {
            IList<string> lines = RunPath.ConfirmLines(new[] { RunPath.WeeklyRun, RunPath.WeeklyRunPlusXml });
            string all = string.Join("\n", new List<string>(lines).ToArray());

            Assert.That(all, Does.Contain("First run: 0."));
            Assert.That(all, Does.Not.Contain("cleared before"));
            Assert.That(all, Does.Contain("Nothing is cleared"));
        }

        [Test]
        public void TheResultLinesNameEachPathInAFixedOrder()
        {
            IList<string> lines = RunPath.ResultLines(
                new[] { RunPath.FirstRun, RunPath.WeeklyRunPlusXml, RunPath.Skipped, RunPath.Skipped });

            Assert.That(lines[0], Is.EqualTo("first run      : 1"));
            Assert.That(lines[1], Is.EqualTo("weekly run     : 0"));
            Assert.That(lines[2], Is.EqualTo("weekly + XML   : 1"));
            Assert.That(lines[3], Is.EqualTo("skipped        : 2"));
            Assert.That(lines.Count, Is.EqualTo(4), "no unknown line when no group is unknown");
        }

        [Test]
        public void AnUnknownPathIsListedRatherThanFoldedIntoAnotherCount()
        {
            IList<string> lines = RunPath.ResultLines(new[] { RunPath.FirstRun, "something else" });

            Assert.That(lines[4], Is.EqualTo("path unknown   : 1"));
        }
    }
}
