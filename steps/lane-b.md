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
| 1 | F115 the sets area, FR-010 to FR-024 and FR-027, carried on from f4dc480 | fix-F115 | next |
| 2 | F127 the coverage sheet of Bader's request 2, FR-176 and FR-200, carried on from dccf351 | fix-F127 | waits |
| 3 | F137 no site and no clash groups end PARTIAL, FR-195, Q111 B and Q125 B | fix-F137 | waits |
| 4 | F118 the workbook and report, FR-035, FR-036, FR-037, FR-040, FR-041 and FR-199 | fix-F118 | waits |
| 5 | F119 the run log and RESULT, FR-043 to FR-057 and FR-189 | fix-F119 | waits |
| 6 | F128's Core part, generic models, FR-177 | fix-F128 | waits |

## The item the lane is on

None yet. This page is the lane's first pull request, on the branch lane-b-start.

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

Nothing yet.

## Where the lane stopped

It has not stopped.
