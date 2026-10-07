# Lane B, the cloud lane with no Navisworks

Set up by Bader's message of 7 Oct 2026 headed FAST TO A TEAM RELEASE. This lane has no
Navisworks and no add-in build, so it works Core only, src\Federator.Core and
tests\Federator.Core.Tests, and where an item needs an add-in change it does the Core half,
says so in the item's log entry, and leaves the add-in half for the laptop lane.

The laptop lane reads this page before taking any item, never takes one listed here, and
leaves the files the open branches below change alone. This page is kept true at every
merge: the item the lane is on, its branch, the files it changes, and what merged.

## The items, in order

| Order | Item | Branch | State |
|---|---|---|---|
| 1 | F115 the sets area, FR-010 to FR-024 and FR-027, carried on from f4dc480 | fix-F115 | code merged as PR 142, its records and two fixes of a third reading in PR 144 |
| 2 | F127 the coverage sheet of Bader's request 2, FR-176 and FR-200, carried on from dccf351 | fix-F127 | waits |
| 3 | F137 no site and no clash groups end PARTIAL, FR-195, Q111 B and Q125 B | fix-F137 | part 1 made, Q125 B left for the laptop lane |
| 4 | F118 the workbook and report, FR-035, FR-036, FR-037, FR-040, FR-041 and FR-199 | fix-F118 | FR-035 and FR-037 made |
| 5 | F119 the run log and RESULT, FR-043 to FR-057 and FR-189 | fix-F119 | waits |
| 6 | F128's Core part, generic models, FR-177 | fix-F128 | waits |

## The item the lane is on

Two sessions of the lane write this page. The cloud session: F115, pull request 142, from the
branch claude/lane-b-release-plan-zztyvx, the one branch it may push, restarted from main after
each merge. The worktree session, in .claude\worktrees\agent-a9ff34180e9235316 of the checkout on
Bader's machine, which may write the records and the tracker: F115's records and two fixes of the
breaker's third reading, on fix-F115, pull request 144, then F127 on fix-F127 after the cloud
session's PR 145. fix-F115 is deleted once PR 144 merges, and fix-F127 once its records merge.

## How the worktree session records

By CLAUDE.md and .claude\rules\steps.md: the rule in .claude\rules\core.md where a rule changed,
the section and the DONE line in steps\01_next.md, the entry at the top of
steps\history\log.md, the rows in steps\tracker.csv with the page and the counts made again, and
this page, all in the item's pull request. Where the cloud session's pull request put an item's
code on main first, the worktree session's pull request carries what that one left out.

## How the cloud session records, which differs from the rule above

Bader's instruction of 7 Oct 2026 for this session is that it writes its record in this page only.
So its pull requests do not touch steps\tracker.csv, steps\tracker.md, steps\PROGRESS.md,
steps\history, steps\02_questions.md or anything under .claude, and not steps\01_next.md either.
What each item would put there waits for the laptop lane: the tracker rows from the table under
What merged, the rule lines of .claude\rules\core.md, the section of steps\01_next.md and the
log entry, which stay on origin/fix-F115 and origin/fix-F127 for the F115 and F127 items.

## The files each open branch changes

Every branch of this lane also changes steps\lane-b.md, steps\tracker.csv, steps\tracker.md,
steps\PROGRESS.md, steps\01_next.md, steps\history\log.md and the rule in .claude\rules\core.md
where a rule changed. The laptop lane merges those files by taking main in, as every branch does.

- fix-F115, at f4dc480 before this lane took it: src\Federator.Core\Sets\EmptySetJudge.cs,
  EmptySets.cs, SetBuildOutcome.cs, SetBuildPlan.cs, SetDrift.cs, SetLeftovers.cs, SetResult.cs,
  src\Federator.Core\Health\ExportCheck.cs, HealthCheck.cs, HealthCheckResult.cs, SetWarnings.cs,
  src\Federator.Core\Exchange\MatrixCorrections.cs, RevitCategories.cs, RevitWorksets.cs,
  revit-categories.txt, revit-worksets.txt, src\Federator.Addin\Engine\SetBuilder.cs and
  FederationEngine.cs, the add-in half built before this lane and not touched by it, and the
  tests under Sets, Health and Exchange
- fix-F127, at dccf351 before this lane took it: src\Federator.Core\Coverage\*,
  src\Federator.Core\Clash\ClashRunOutcome.cs, src\Federator.Core\Diagnostics\FileFingerprint.cs
  and RunLog.cs, src\Federator.Core\Health\ExportCheck.cs, src\Federator.Core\Report\WorkbookTest.cs
  and WorkbookTests.cs, src\Federator.Core\Sets\SetsAcrossTheRun.cs, and the tests under
  Coverage, Clash, Diagnostics and Report. The Coverage sheet writer goes in
  src\Federator.Core\Report
- fix-F137, not yet made: src\Federator.Core\Health\AlignmentCheck.cs,
  src\Federator.Core\Health\OffCoordinates.cs, src\Federator.Core\Rerun\GroupJudgement.cs and
  their tests under Health and Rerun
- fix-F118, not yet made: src\Federator.Core\Report\WorkbookCheck.cs, WorkbookWriter.cs,
  ReportOrder.cs, ClashReportModel.cs, src\Federator.Core\Clash\ToleranceChoice.cs,
  PriorityMap.cs and their tests under Report and Clash
- fix-F119, not yet made: src\Federator.Core\Diagnostics\RunLog.cs, RowLog.cs, EventRow.cs,
  RunClock.cs, LiveLine.cs, src\Federator.Core\Clash\ClashRunOutcome.cs, ToleranceChoice.cs,
  src\Federator.Core\Rerun\GroupJudgement.cs and their tests under Diagnostics, Clash and Rerun
- fix-F128, not yet made: a new folder src\Federator.Core\Generic and its tests

## What merged

One line per item: its ID, what changed, the pull request and its status. A status is the laptop
lane's to set in the tracker.

| ID | What changed | PR | Status |
|---|---|---|---|
| F115 | fix-F115 carried on: main merged in, source conflicts resolved, main's AlsoAskTests moved to the judge form the branch introduced, and three lines made true after the first reading, the EMPTY SETS wording, the window totals for a rebuilt set and the row of a stopped walk | 142 | merged 2026-10-07 at 13:25, 6729b9e |
| F115 | the records of fix-F115, the rules, the plan section, the log entry and the tracker rows, and two fixes of the breaker's third reading: the EMPTY SETS judge reads a set group by group, and one unused twin serves one leftover. Core tests 2203 before and 2207 after, 0 failed, 0 skipped | 144 | merged with this page |

## Points the readers raised on F115 that lane B dropped, for the loop

Each needs the add-in or a measurement, or is older than F115, or changes no number the team sees.
Lane B fixed three: the EMPTY SETS wording that said no model in the project carries a value where
only the models measured so far were read, the window totals that called a rebuilt set left alone,
and the row that said how many worksets a stopped walk had seen.

- Add-in, UNKNOWN until a run with Navisworks: the EMPTY SETS block is written only after a test
  runs, so a picked file of sets alone and a clash step that throws write none, and the Build sets
  button never prints it. A set AddCopy put nowhere findable is still counted created. A set
  rebuilt and then a throw leaves RebuiltCount low. A rebuilt set is not checked against the file
  and what SearchCondition.Options reads back for an Or set is unmeasured, so such sets may read
  DRIFTED every run. Two sets of one name in one folder read the wrong one. A set the plan skipped
  but the file names can be removed as a leftover. A damaged document is still saved after Q74.
  A set with a search that is null reads as nothing at all. The Closing line of the window for a
  rebuilt set per set still says left alone
- Found at the second reading, older than lane B's change: the totals say a drifted set was not
  rebuilt because the box is off when the box is on and the rebuild failed, and with some of the
  drifted rebuilt nothing says the others still ask the old question. SetBuildOutcome has no way
  to know the box was on, so the add-in has to hand it that. Two sets of one name in a picked
  file are both built and the set flips between the two questions every run, while only the
  HEALTH line at the pick names duplicates. The new EMPTY SETS wording is true but a group with a
  model that was not read still reads as a wrong condition, and the HEALTH line still says no
  model carries a category. A model dropped in ModelFactsReader.Exports has no model worksets row
- Core, left as designed by the loop: a present set whose search could not be read is never judged
  empty, so the SETS count at zero is higher than the EMPTY SETS header and the SETS block says
  how many could not be read. The judge reads only the first value of a set and ignores Or groups,
  where HEALTH reads them. The HEALTH count of categories no model carries has no project guard.
  SETS ACROSS THE RUN says every set found something over a run with no group. A failed list read
  is cached for the life of the window. The not-a-typo pairs are one project's
- Words and polish: three comments say 374 categories and 39 worksets where the judge adds more,
  two summary elements in a row in SetResult, the test of Is.EqualTo on collections in five tests,
  a second ContainsTest in SetWarnings, the walk not disposing five sub objects which FR-029 holds
- Members with no caller that F115 did not add: RevitWorksets.DecidedCount, PlannedSet.GroupCount
  and Groups, and the long forms of AddCreated and EmptySets.Lines used by tests only

## Where the lane stopped

It has not stopped.
