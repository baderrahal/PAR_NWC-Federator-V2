using System;
using System.Collections.Generic;
using Federator.Core.Diagnostics;
using Federator.Core.Rerun;
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

        /// <summary>
        /// APPEND exists to bring the models in. It brings the saved viewpoints too, which
        /// is F73 and is NOTED rather than allowed, so the census is still taken around it
        /// for the reason it always was and the viewpoint move still writes a line.
        /// </summary>
        [Test]
        public void AppendingIsThereToMoveTheModels()
        {
            Assert.That(CensusRule.Judge(RunSteps.Append, CensusCount.Models),
                Is.EqualTo(CensusMove.Allowed));
            Assert.That(CensusRule.Judge(RunSteps.Append, CensusCount.Sets),
                Is.EqualTo(CensusMove.Refused));
            Assert.That(CensusRule.Judge(RunSteps.Append, CensusCount.Tests),
                Is.EqualTo(CensusMove.Refused));
            Assert.That(CensusRule.Judge(RunSteps.Append, CensusCount.Results),
                Is.EqualTo(CensusMove.Refused));
        }

        /// <summary>
        /// F73. The first real run raised the saved viewpoints from 0 to 20 during APPEND
        /// in all seven groups and every group was reported FAILED for it, while 28 files
        /// had been written correctly. An NWC exported from Revit carries that model's
        /// saved viewpoints and appending it brings them in.
        /// </summary>
        [Test]
        public void AppendingBringingTheViewpointsInIsNoted()
        {
            Assert.That(CensusRule.Judge(RunSteps.Append, CensusCount.Viewpoints),
                Is.EqualTo(CensusMove.Noted));
        }

        [Test]
        public void TheFourThatBuildMoveOneCountEach()
        {
            Assert.That(CensusRule.MayMove(RunSteps.Sets, CensusCount.Sets), Is.True);
            Assert.That(CensusRule.MayMove(RunSteps.TestsCreate, CensusCount.Tests), Is.True);
            Assert.That(CensusRule.MayMove(RunSteps.TestsRun, CensusCount.Results), Is.True);
            Assert.That(CensusRule.MayMove(RunSteps.Views, CensusCount.Viewpoints), Is.True);

            Assert.That(CensusRule.MayMove(RunSteps.Sets, CensusCount.Tests), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.TestsCreate, CensusCount.Results), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.TestsRun, CensusCount.Tests), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.Views, CensusCount.Results), Is.False);
            Assert.That(CensusRule.MayMove(RunSteps.Views, CensusCount.Models), Is.False);
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
        public void TheStepsThatMayWriteAreTheSixAndOnlyThose()
        {
            IList<string> writes = CensusRule.StepsThatMayWrite();

            Assert.That(writes.Count, Is.EqualTo(6));
            Assert.That(writes, Does.Contain(RunSteps.Decide));
            Assert.That(writes, Does.Contain(RunSteps.Append));
            Assert.That(writes, Does.Contain(RunSteps.Sets));
            Assert.That(writes, Does.Contain(RunSteps.TestsCreate));
            Assert.That(writes, Does.Contain(RunSteps.TestsRun));
            Assert.That(writes, Does.Contain(RunSteps.Views));
            Assert.That(writes, Does.Not.Contain(RunSteps.Nwd));

            // In the order a group meets them, so the narrowed line reads the same way
            // the run does.
            Assert.That(writes[0], Is.EqualTo(RunSteps.Decide));
            Assert.That(writes[4], Is.EqualTo(RunSteps.TestsRun));
            Assert.That(writes[5], Is.EqualTo(RunSteps.Views));
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

        // ---------- F73, the noted move ----------

        /// <summary>
        /// The whole of F73 in one test: the line is still written, it no longer reads as
        /// a fault, and no reason goes on the group.
        /// </summary>
        [Test]
        public void AppendRaisingTheViewpointsWritesALineAndNoReason()
        {
            DocumentCensus before = new DocumentCensus(0, 0, 0, 0, 0);
            DocumentCensus after = new DocumentCensus(4, 0, 0, 0, 20);

            IList<string> lines = CensusRule.Lines(RunSteps.Append, before, after);
            IList<string> reasons = CensusRule.Reasons(RunSteps.Append, before, after);

            Assert.That(lines.Count, Is.EqualTo(1), "the models were allowed, the views are noted");
            Assert.That(lines[0], Does.StartWith(CensusRule.NotedPrefix));
            Assert.That(lines[0], Does.Contain("saved viewpoints went from 0 to 20"));
            Assert.That(lines[0], Does.Contain("can still be DONE"));
            Assert.That(lines[0], Does.Not.Contain(CensusRule.ChangedPrefix));

            Assert.That(reasons, Is.Empty, "a noted move puts no group out of DONE");
        }

        /// <summary>
        /// The other half of the same fix. A group whose only census line is the noted one
        /// collects no error and is judged DONE, which is the thing the run got wrong.
        /// </summary>
        [Test]
        public void AGroupWhoseOnlyCensusLineIsTheNotedOneIsStillDone()
        {
            DocumentCensus before = new DocumentCensus(0, 0, 0, 0, 0);
            DocumentCensus after = new DocumentCensus(4, 0, 0, 0, 20);

            GroupFacts facts = new GroupFacts
            {
                Decision = RerunDecision.Build,
                NwfOnDisk = true,
                NwdRequested = true,
                NwdOnDisk = true,
                NwdPublishReportedSuccess = true,
                AppendedCount = 4,
                FileCount = 4,
                NwfPath = TestPaths.At("out", "a.nwf"),
                NwdPath = TestPaths.At("out", "a.nwd")
            };

            foreach (string reason in CensusRule.Reasons(RunSteps.Append, before, after))
            {
                facts.AddError(reason);
            }

            string why;

            Assert.That(GroupJudgement.Judge(facts, out why), Is.EqualTo(GroupOutcome.Done));
            Assert.That(why, Is.Null);
        }

        /// <summary>
        /// The break. A viewpoint count moving during a step that is NOT allowed to move
        /// it still writes CENSUS CHANGED and still puts the group out of DONE, so F73
        /// widened one pair and nothing else.
        /// </summary>
        [Test]
        public void ViewpointsMovingOutsideAppendStillFailsTheGroup()
        {
            DocumentCensus before = new DocumentCensus(4, 61, 1830, 412, 20);
            DocumentCensus after = new DocumentCensus(4, 61, 1830, 412, 0);

            IList<string> lines = CensusRule.Lines(RunSteps.Nwd, before, after);
            IList<string> reasons = CensusRule.Reasons(RunSteps.Nwd, before, after);

            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0], Does.StartWith(CensusRule.ChangedPrefix));
            Assert.That(lines[0], Does.Contain("is not DONE"));
            Assert.That(reasons.Count, Is.EqualTo(1));

            GroupFacts facts = new GroupFacts
            {
                Decision = RerunDecision.Build,
                NwfOnDisk = true,
                AppendedCount = 4,
                FileCount = 4,
                NwfPath = TestPaths.At("out", "a.nwf")
            };

            facts.AddError(reasons[0]);

            string why;

            Assert.That(GroupJudgement.Judge(facts, out why), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(why, Does.Contain("saved viewpoints"));
        }

        /// <summary>
        /// The noted line says WHY, because a line naming a count that moved and not
        /// saying why reads exactly like the fault it is not.
        /// </summary>
        [Test]
        public void TheNotedLineSaysWhyThatStepMovesThem()
        {
            string line = CensusRule.NotedLine(RunSteps.Append, CensusCount.Viewpoints, 0, 20);

            Assert.That(line, Does.Contain("NWC"));
            Assert.That(line, Does.Contain("Revit"));
            Assert.That(CensusRule.WhyNoted(RunSteps.Append, CensusCount.Viewpoints),
                Does.Contain("appending files brings them in"));
        }

        [Test]
        public void NothingElseAnywhereIsNoted()
        {
            foreach (string step in RunSteps.All)
            {
                foreach (CensusCount what in DocumentCensus.All)
                {
                    bool theOne = string.Equals(step, RunSteps.Append, StringComparison.Ordinal)
                        && what == CensusCount.Viewpoints;

                    Assert.That(CensusRule.Judge(step, what) == CensusMove.Noted,
                        Is.EqualTo(theOne), step + " and " + what);
                }
            }
        }

        // ---------- F75, the census at the top of a group ----------

        /// <summary>
        /// F75. Nothing cleared the document between groups. Both Clear calls in the whole
        /// engine ran AFTER Decide had read the file list, so Decide compared the scan
        /// against whatever the previous building had left behind.
        /// </summary>
        [Test]
        public void AGroupThatStartedEmptySaysSoAndCarriesNoReason()
        {
            DocumentCensus census = new DocumentCensus(0, 0, 0, 0, 0);

            Assert.That(CensusRule.NotClear(census), Is.Empty);
            Assert.That(CensusRule.StartOfGroupLine(census, true),
                Does.StartWith(CensusRule.ClearPrefix));
            Assert.That(CensusRule.StartOfGroupReason(census, true), Is.Null);
        }

        [Test]
        public void AGroupStillHoldingTheLastBuildingIsNamedCountByCount()
        {
            DocumentCensus census = new DocumentCensus(4, 61, 1830, 412, 20);

            IList<CensusCount> dirty = CensusRule.NotClear(census);

            Assert.That(dirty.Count, Is.EqualTo(5));

            string line = CensusRule.StartOfGroupLine(census, true);

            Assert.That(line, Does.StartWith(CensusRule.NotClearPrefix));
            Assert.That(line, Does.Contain("4 models"));
            Assert.That(line, Does.Contain("61 selection sets"));
            Assert.That(line, Does.Contain("1830 clash tests"));
            Assert.That(line, Does.Contain("412 clash results"));
            Assert.That(line, Does.Contain("20 saved viewpoints"));

            Assert.That(CensusRule.StartOfGroupReason(census, true), Does.Contain("4 models"));
        }

        [Test]
        public void OneCountLeftBehindIsStillNotClear()
        {
            DocumentCensus census = new DocumentCensus(0, 0, 0, 0, 20);

            Assert.That(CensusRule.NotClear(census).Count, Is.EqualTo(1));
            Assert.That(CensusRule.StartOfGroupLine(census, true),
                Does.StartWith(CensusRule.NotClearPrefix));
            Assert.That(CensusRule.StartOfGroupReason(census, true),
                Does.Contain("20 saved viewpoints"));
        }

        /// <summary>
        /// UNKNOWN is not a number. Calling a count that could not be taken dirty would
        /// report a reading that never happened, which is the same mistake as calling it
        /// zero.
        /// </summary>
        [Test]
        public void ACountThatCouldNotBeTakenIsNeverCalledDirty()
        {
            DocumentCensus census = new DocumentCensus(0, 0, 0, 0, -1);

            Assert.That(CensusRule.NotClear(census), Is.Empty);
            Assert.That(CensusRule.StartOfGroupReason(census, true), Is.Null);
            Assert.That(CensusRule.StartOfGroupLine(census, true), Does.Contain("UNKNOWN"));
        }

        /// <summary>
        /// The open file run empties nothing, because the document IS the file list there.
        /// It still says what it found and it is never a fault.
        /// </summary>
        [Test]
        public void TheOpenFileRunHoldingAWholeFederationIsNoFault()
        {
            DocumentCensus census = new DocumentCensus(4, 61, 1830, 412, 20);

            string line = CensusRule.StartOfGroupLine(census, false);

            Assert.That(line, Does.StartWith(CensusRule.ClearPrefix));
            Assert.That(line, Does.Contain("not emptied"));
            Assert.That(line, Does.Contain("4 models"));
            Assert.That(CensusRule.StartOfGroupReason(census, false), Is.Null);
        }

        [Test]
        public void NoCensusAtAllSaysUnknownRatherThanClear()
        {
            Assert.That(CensusRule.StartOfGroupLine(null, true), Does.Contain("UNKNOWN"));
            Assert.That(CensusRule.StartOfGroupReason(null, true), Is.Null);
            Assert.That(CensusRule.NotClear(null), Is.Empty);
        }

        /// <summary>
        /// The half that matters: a group that did not start clear is not DONE, because
        /// everything it goes on to read comes out of that document.
        /// </summary>
        [Test]
        public void AGroupThatDidNotStartClearIsNotDone()
        {
            DocumentCensus census = new DocumentCensus(4, 0, 0, 0, 0);

            GroupFacts facts = new GroupFacts
            {
                Decision = RerunDecision.Build,
                NwfOnDisk = true,
                AppendedCount = 4,
                FileCount = 4,
                NwfPath = TestPaths.At("out", "a.nwf")
            };

            facts.AddError(CensusRule.StartOfGroupReason(census, true));

            string why;

            Assert.That(GroupJudgement.Judge(facts, out why), Is.EqualTo(GroupOutcome.Failed));
            Assert.That(why, Does.Contain("4 models"));
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
