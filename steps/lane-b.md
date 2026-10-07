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
| 1 | F115 the sets area, FR-010 to FR-024 and FR-027, carried on from f4dc480 | fix-F115 | code merged as pull request 142, its records and two fixes of a third reading as 144 by the worktree session, the add-in half waits for the laptop lane |
| 2 | F127 the coverage sheet of Bader's request 2, FR-176 and FR-200, carried on from dccf351 | fix-F127 | Core half merged as pull request 145, the add-in half and the sheet writer wait |
| 3 | F137 no site and no clash groups end PARTIAL, FR-195, Q111 B and Q125 B | fix-F137 | part 1 merged as pull request 146, Q125 B left for the laptop lane |
| 4 | F118 the workbook and report, FR-035, FR-036, FR-037, FR-040, FR-041 and FR-199 | fix-F118 | FR-035 and FR-037 merged as pull request 147, the rest left |
| 5 | F119 the run log and RESULT, FR-043 to FR-057 and FR-189 | fix-F119 | nine items in review, the rest left |
| 6 | F128's Core part, generic models, FR-177 | fix-F128 | waits |

## The item the lane is on

F119, from the branch claude/lane-b-release-plan-zztyvx, the one branch this session may push,
restarted from main after each merge. F115 merged as pull request 142, F127 as 145, F137 as 146 and
F118 as 147. fix-F115 and fix-F127 stay on origin and
are not deleted, because their records are theirs.

A second session of the lane, the worktree session in .claude\worktrees\agent-a9ff34180e9235316
of the checkout on Bader's machine, writes the records the cloud session may not: F115's records
and two fixes of the breaker's third reading on fix-F115, pull request 144, then the records of
F127, F137, F118 and F119 each on its own branch, and the Core parts the cloud session left, the
Coverage sheet writer of FR-200 first. It deletes fix-F115 once 144 merges and fix-F127 once
F127's records merge.

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
| F115 | fix-F115 carried on: main merged in, source conflicts resolved, main's AlsoAskTests moved to the judge form the branch introduced, and three lines made true after the first reading, the EMPTY SETS wording, the window totals for a rebuilt set and the row of a stopped walk | 142 | merged, 6729b9e |
| F115 | the records of fix-F115 by the worktree session, the rules, the plan section, the log entry and the tracker rows, and two fixes of the breaker's third reading: the EMPTY SETS judge reads a set group by group, and one unused twin serves one leftover. Core tests 2203 before and 2207 after, 0 failed, 0 skipped | 144 | merged with this page |
| F127 | fix-F127 carried on, Core half only: main merged in, WriteResultBlock takes thisRun, makeViewpoints and coverage in that order, and after the first reading the headline no longer counts a test neither side holds as agreeing, says how many tests Clash Detective holds that the picked file does not name, judges a name on two tests of the file on neither, calls Compact a possible cause and never the cause, and keeps a FAILED line to one line. The add-in has to build the CoverageAcrossTheRun in the engine and hand it to the window's call of WriteResultBlock, call ClashRunOutcome.RecordSides and KeepItemsByLocator in ClashRunner near its two skip sites, and the Coverage sheet writer is not written | 145 | merged, 2348b58 |
| F137 | Part 1 of FR-195, Q111 B: a model whose site was read and names none is listed by AlignmentCheck as not on the same shared coordinates, so with the rule on and a test to run its group skips the clash and ends PARTIAL, its line says the model names no shared site at all, and it fails its group only where no clash is skipped. The grey line of the tick box and the failed run line say so. Q125 B, PARTIAL for a group that runs no clash test, needs JobOutcome and FederationEngine, which fix-F114's add-in pass changes, so it is left, and the test named StillFailsTheGroupUntilQ125IsWired flips when it lands | 146 | merged, f1557fe |
| F118 | FR-035: the workbook check counts the tests of one row and the full blocks, says how many are of each, reads row 1 and the widths of a workbook with no full block and says no block layout was compared. FR-037: a test the priority file names twice is named in the log with both lines and both letters, the last letter still wins, and the row count is the rows of the file. FR-036, FR-040 and FR-199 need ClashRunner, ClashReportModel or the engine's call of the workbook check, which an open branch changes, and FR-041 needs a probe of the grid first | 147 | merged, a174ca0 |
| F119 | FR-046 sizes read through a handle that shares the file and a file that exists is never said missing. FR-048 the .tsv row of a file not on disk carries no number. FR-050 a run that started and never finished is counted to now and says so, and RunStarted forgets an earlier run's finish. FR-051 RESULT states no waiting time where no run was marked. FR-052 NoTolerance is a skip reason row. FR-054 the fallback folder is never pruned. FR-055 a .tsv goes with its log. FR-056 the pace of the group before is the mean of its visits. FR-057 a listener that throws is named and removed once, and a log file that cannot be written is said once and the run goes on. FR-043 to FR-045, FR-053 and FR-189 need the engine or ClashRunner, FR-047 needs ClashRunner, FR-049 resets the whole state of a log across runs and is larger than a Core fix | in review | in review |

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

## F127 points the readers raised that lane B left, for the laptop lane

- Not in Core yet: FR-176's RESULT count of the tests of the XML created, run, with clashes and without,
  with Q127's each test once across the run, and the categories no set catches per model, since
  CoverageSettings.CategoriesNamedPerModel is read by no rule
- A test the plan dropped that an earlier run left in the NWF reads not created with a FAILED line that
  names no cause, since CoverageRule never reads AlreadyPresentNames for it. Mirrors after fix-F132
  merges may bypass ReasonFor the same way
- The workbook handed in carries no proof of which run wrote it, so a write that threw leaves last week's
  file at the same path to be read as this week's, and the fingerprint names the XML bytes and not the
  corrections list or the teams file that changed its sets. The add-in has to hand a stamp in
- A set with no code in its name reads UNKNOWN where the team map of F131 gives the team of its folder,
  Q117 C, and RESULT lists every FAILED line by default, Bader's choice, which can be some 25 MB where one
  fault repeats over 46 groups
- SetsAcrossTheRun counts a path twice where one group holds two sets of one name, and says found nothing
  in every group over the groups it looked at only. WorkbookTests.Read takes the first sheet and would
  read the Coverage sheet if that were inserted first
- Found at the second reading, left open: a test whose presence is Unknown with an empty block lands under
  held by neither side and should be not compared, because CountCheck sends only CreatedThisRun and
  AlreadyThere there and the runner never looked at the others. The not named line counts names once per
  group, so one old test in 46 groups reads 46 tests. The headline never prints the number of tests, so
  its buckets cannot be added up by eye, and a group whose check is null has its tests in no bucket. Two
  numbers in comments, 1794 of 1830, were not measured by lane B and should be read off a run or dropped.
  The class summary of CountCheck still calls a test neither side holds AGREE in one sentence
- Words: Q126's default A where Q126 was answered B, a design file named in comments that is not in the
  repo, two copies of the row count loop and of Count, and a double blank line in RESULT

## F137 points the readers raised that lane B left, for the laptop lane

- ModelFactsReader.SharedCoordinateOn returns an empty site where the Location tab is missing or the
  property collection is null, not only where a model names none. Since Q111 B a read fault reads as a
  model naming no site and skips its group's clash where it used to fail the group and still clash it. A
  null tab should read as site not read, and whether any of the 27 machines names the tab differently is
  UNKNOWN
- The reference model can itself name no site, as it could already name Internal, and every model is
  then measured from it and listed far. ModelsRead counts placements and not the document's models, so a
  model whose read threw is never judged and does not keep an earlier note
- .claude\rules\core.md still says a model naming no site fails its group either way, in three
  places, and the comments of FederationEngine near line 2154 and 2300 and ModelFactsReader near line 154
  say the same. Lane B may not edit the rule file
- Q125 B: the tests of the group judgement that join a failure and a skip for one group describe a state
  the engine can no longer make

## F118 points the readers raised that lane B left, for the laptop lane

- FR-199 would take the priority sort out of ReportOrder.Tests, and then FederationEngine near line 3031
  must stop telling WorkbookCheck a priority order was asked for, or the check calls a measured order
  wrong. FR-040 and FR-036 are in ClashReportModel and ClashRunner, FR-041 needs a probe printing
  ClosestIntersection for the empty rows first
- The order check of WorkbookCheck reads the full blocks only, so a one row test placed before a full
  block is not named as a wrong order. With a priority file picked the check does not run the Priority
  heading check on a sheet with no full block
- .claude\rules\core.md does not say that a repeated test name in the priority file is a named problem
  or that a workbook of one row tests is counted. The engine writes each priority problem once as a line
  and again under the match lines, which is older than this change

## F119 points the readers raised that lane B left, for the laptop lane

- The loop's run.ps1 near line 784 words a GONE file as the tool's own pruning of its oldest logs only
  when it matches run-*.log, so a pruned .tsv of Bader's now reads as a finding at the next loop run
  though logs-backup still holds it. Bader's .tsv files that earlier builds left beside logs already
  deleted are not removed, only growth from now on is stopped. Lane B took FR-055's own note that they go
  with the pruned logs as the decision, and Q82 does not name them
- FR-057's disk half is left open: WriteRaw's file write still has no try, so a full disk or a handle gone
  still throws out of Line. Lane B wrapped it and took it out again after the second reading, because the
  first write at open then no longer fell back to the temp folder and the window said the log was on disk,
  and after a fault TryCopyTo and the RESULT size read a short file as whole and Dispose could leave the
  handle open. A fix has to make a fault at open count as a failed open, make IsWritingToDisk false, and
  mark every size and copy after a fault. Also the figure SizeOnDisk now falls back to for a file held
  with no sharing is the directory's and can lag, and is not marked
- The RETAIN line counts a .tsv that could not be deleted in the same number as a log that could not,
  and an unfinished run's timing block still prints an after the run finished row of 0.0 seconds
- .claude\rules\core.md lines about retention and the size rule do not say the .tsv goes with its log,
  that the temp fallback is not pruned, or that a file held with no sharing falls back to the directory's
  size. FR-049, a second run in one window carrying the first run's totals, is still open and a new log
  per run in the window would close it

