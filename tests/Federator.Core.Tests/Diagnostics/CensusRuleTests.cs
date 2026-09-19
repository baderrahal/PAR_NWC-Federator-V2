using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using NUnit.Framework;

namespace Federator.Core.Tests
{
    /// <summary>
    /// Which step may move which count, every allowed move and every refused one.
    ///
    /// The useful half of this rule is the refusals. A count moving during the workbook
    /// write or the NWD publish is exactly the kind of thing nobody would think to look
    /// for, and it would have gone out as a DONE group with a published NWD and nothing
    /// in the log saying a word.
    /// </summary>
    [TestFixture]
    public class CensusRuleTests
    {
        [Test]
        public void OpeningTheNwfMayMoveEverything()
        {
            foreach (CensusCount what in DocumentCensus.All)
            {
                Assert.That(CensusRule.MayMove(RunSteps.Decide, what), Is.True, what.ToString());
            }
        }

        [Test]
        public void AppendingMovesTheModelsAndNothingElse()
        {
            Assert.That(CensusRule.MayMove(RunSteps.Append, CensusCount.Models), Is.True);
            Assert.That(CensusRule.MayMove(RunSteps.Append, CensusCount.Sets), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.Append, CensusCount.Tests), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.Append, CensusCount.Results), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.Append, CensusCount.Viewpoints), Is.False);
        }

        [Test]
        public void TheThreeThatBuildMoveOneCountEach()
        {
            Assert.That(CensusRule.MayMove(RunSteps.Sets, CensusCount.Sets), Is.True);
            Assert.That(CensusRule.MayMove(RunSteps.TestsCreate, CensusCount.Tests), Is.True);
            Assert.That(CensusRule.MayMove(RunSteps.TestsRun, CensusCount.Results), Is.True);

            Assert.That(CensusRule.MayMove(RunSteps.Sets, CensusCount.Tests), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.TestsCreate, CensusCount.Results), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.TestsRun, CensusCount.Tests), Is.False);
        }

        /// <summary>
        /// The break, and the reason the rule is worth having. Every step that writes a
        /// FILE may move nothing in the document at all.
        /// </summary>
        [Test]
        public void EveryStepThatWritesAFileMayMoveNothing()
        {
            string[] writeAFile =
            {
                RunSteps.NwfSave, RunSteps.Workbook, RunSteps.Html, RunSteps.Xml,
                RunSteps.Nwd, RunSteps.Confirm, RunSteps.Harvest, RunSteps.Images,
                RunSteps.Units
            };

            foreach (string step in writeAFile)
            {
                foreach (CensusCount what in DocumentCensus.All)
                {
                    Assert.That(CensusRule.MayMove(step, what), Is.False, step + " and " + what);
                }
            }
        }

        [Test]
        public void NothingAndAnUnknownStepMoveNothing()
        {
            Assert.That(CensusRule.MayMove(null, CensusCount.Models), Is.False);
            Assert.That(CensusRule.MayMove(string.Empty, CensusCount.Models), Is.False);
            Assert.That(CensusRule.MayMove("CLASH", CensusCount.Models), Is.False);
        }

        [Test]
        public void TheStepsThatMayWriteAreTheFiveAndOnlyThose()
        {
            IList<string> writes = CensusRule.StepsThatMayWrite();

            Assert.That(writes.Count, Is.EqualTo(5));
            Assert.That(writes, Does.Contain(RunSteps.Decide));
            Assert.That(writes, Does.Contain(RunSteps.Append));
            Assert.That(writes, Does.Contain(RunSteps.Sets));
            Assert.That(writes, Does.Contain(RunSteps.TestsCreate));
            Assert.That(writes, Does.Contain(RunSteps.TestsRun));
            Assert.That(writes, Does.Not.Contain(RunSteps.Nwd));

            // In the order a group meets them, so the narrowed line reads the same way
            // the run does.
            Assert.That(writes[0], Is.EqualTo(RunSteps.Decide));
            Assert.That(writes[4], Is.EqualTo(RunSteps.TestsRun));
        }

        [Test]
        public void WhileTheCensusIsCheapItIsTakenAroundEveryStep()
        {
            foreach (string step in RunSteps.All)
            {
                Assert.That(CensusRule.TakenAround(step, true), Is.True, step);
            }

            Assert.That(CensusRule.TakenAround("CLASH", true), Is.False);
        }

        [Test]
        public void OnceItIsNotCheapItIsTakenOnlyAroundTheStepsThatMayWrite()
        {
            Assert.That(CensusRule.TakenAround(RunSteps.TestsRun, false), Is.True);
            Assert.That(CensusRule.TakenAround(RunSteps.Nwd, false), Is.False);
            Assert.That(CensusRule.TakenAround(RunSteps.Workbook, false), Is.False);
        }

        // ---------- the wording ----------

        [Test]
        public void ARefusedMoveNamesTheStepTheCountTheBeforeAndTheAfter()
        {
            string line = CensusRule.ChangedLine(RunSteps.Nwd, CensusCount.Tests, 1830, 0);

            Assert.That(line, Does.StartWith(CensusRule.ChangedPrefix));
            Assert.That(line, Does.Contain(RunSteps.Nwd));
            Assert.That(line, Does.Contain("clash tests"));
            Assert.That(line, Does.Contain("went from 1830 to 0"));
            Assert.That(line, Does.Contain("not one that may change them"));
            Assert.That(line, Does.Contain("This group is not DONE"));
            Assert.That(line, Does.Contain("Nothing was undone and the run carried on"));
        }

        [Test]
        public void TheReasonSaysTheSameThingTheLineSays()
        {
            string reason = CensusRule.Reason(RunSteps.Workbook, CensusCount.Sets, 61, 60);

            Assert.That(reason, Does.Contain("selection sets"));
            Assert.That(reason, Does.Contain("went from 61 to 60"));
            Assert.That(reason, Does.Contain(RunSteps.Workbook));
        }

        [Test]
        public void AnAllowedMoveWritesNoLineAtAll()
        {
            DocumentCensus before = new DocumentCensus(5, 61, 1830, 0, 0);
            DocumentCensus after = new DocumentCensus(5, 61, 1830, 412, 0);

            Assert.That(CensusRule.Lines(RunSteps.TestsRun, before, after), Is.Empty);
            Assert.That(CensusRule.Reasons(RunSteps.TestsRun, before, after), Is.Empty);
        }

        [Test]
        public void ARefusedMoveWritesOneLineAndOneReason()
        {
            DocumentCensus before = new DocumentCensus(5, 61, 1830, 412, 0);
            DocumentCensus after = new DocumentCensus(5, 61, 0, 412, 0);

            IList<string> lines = CensusRule.Lines(RunSteps.Nwd, before, after);
            IList<string> reasons = CensusRule.Reasons(RunSteps.Nwd, before, after);

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.Contain("clash tests went from 1830 to 0"));
            Assert.That(reasons.Count, Is.EqualTo(1));
        }

        [Test]
        public void TwoRefusedMovesInOneStepWriteTwoLines()
        {
            DocumentCensus before = new DocumentCensus(5, 61, 1830, 412, 0);
            DocumentCensus after = new DocumentCensus(4, 61, 1829, 412, 0);

            Assert.That(CensusRule.Lines(RunSteps.Nwd, before, after).Count, Is.EqualTo(2));
        }

        /// <summary>
        /// A step that is allowed to move one count is not allowed to move a different
        /// one, which is the case a rule stated as a single may-write flag would miss.
        /// </summary>
        [Test]
        public void AStepAllowedOneCountIsStillCalledOutOnAnother()
        {
            DocumentCensus before = new DocumentCensus(5, 61, 1830, 0, 0);
            DocumentCensus after = new DocumentCensus(5, 61, 1000, 412, 0);

            IList<string> lines = CensusRule.Lines(RunSteps.TestsRun, before, after);

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.Contain("clash tests"));
            Assert.That(lines[0], Does.Not.Contain("clash results"));
        }

        [Test]
        public void ACountNeitherCensusCouldTakeIsNeverCalledAChange()
        {
            DocumentCensus before = new DocumentCensus(5, 61, 1830, 412, -1);
            DocumentCensus after = new DocumentCensus(5, 61, 1830, 412, 4);

            Assert.That(CensusRule.Lines(RunSteps.Nwd, before, after), Is.Empty);
        }

        [Test]
        public void NothingToCompareAgainstWritesNothingAndDoesNotThrow()
        {
            DocumentCensus census = new DocumentCensus(1, 1, 1, 1, 1);

            Assert.That(CensusRule.Lines(RunSteps.Nwd, null, census), Is.Empty);
            Assert.That(CensusRule.Lines(RunSteps.Nwd, census, null), Is.Empty);
            Assert.That(CensusRule.Reasons(RunSteps.Nwd, null, null), Is.Empty);
        }
    }
}
