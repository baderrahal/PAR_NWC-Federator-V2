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
| 2 | F127 the coverage sheet of Bader's request 2, FR-176 and FR-200, carried on from dccf351 | fix-F127 | Core steps 1 to 3 merged as pull request 145, the Coverage sheet, the check's sheet list, the Q127 lines and the records as 153 by the worktree session, the add-in half waits for the laptop lane |
| 3 | F137 no site and no clash groups end PARTIAL, FR-195, Q111 B and Q125 B | fix-F137 | part 1 merged as pull request 146, Q125 B left for the laptop lane |
| 4 | F118 the workbook and report, FR-035, FR-036, FR-037, FR-040, FR-041 and FR-199 | fix-F118 | FR-035 and FR-037 merged as pull request 147, the rest left for the laptop lane |
| 5 | F119 the run log and RESULT, FR-043 to FR-057 and FR-189 | fix-F119 | nine items merged as pull request 148 with FR-057 half, the rest left for the laptop lane |
| 6 | F128's Core part, generic models, FR-177 | fix-F128 | not started, see below |
| 7 | F121 the rest, FR-150 to FR-166 and FR-202, wave 4 | fix-F121 | five items merged as pull request 150, the rest left for the laptop lane |
| 8 | F123 docs and words and the noise of every area, wave 5 | fix-F123 | three items in review |

## The item the lane is on

None. The branch claude/lane-b-release-plan-zztyvx is the one branch this session may push, restarted
from main after each merge. F115 merged as pull request 142, F127 as 145, F137 as 146, F118 as 147 and
F119 as 148. fix-F115 and fix-F127 stay on origin and are not deleted, because their records are theirs.

A second session of the lane, the worktree session in .claude\worktrees\agent-a9ff34180e9235316
of the checkout on Bader's machine, writes the records the cloud session may not: F115's records
and two fixes of the breaker's third reading on fix-F115, pull request 144, merged as 86cc405,
then F127's Coverage sheet with its records on fix-F127, pull request 153, pushed once GitHub
stopped refusing every push with an internal server error, from 15:12 to 15:19 on 2026-10-07,
then the records of F137, F118 and F119 each on its own branch. fix-F115 is deleted. fix-F127 at dccf351 is an
ancestor of main since PR 145 and is deleted once F127's records merge.

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
| F119 | FR-046 sizes read through a handle that shares the file and a file that exists is never said missing. FR-048 the .tsv row of a file not on disk carries no number. FR-050 a run that started and never finished is counted to now and says so, and RunStarted forgets an earlier run's finish. FR-051 RESULT states no waiting time where no run was marked. FR-052 NoTolerance is a skip reason row. FR-054 the fallback folder is never pruned. FR-055 a .tsv goes with its log. FR-056 the pace of the group before is the mean of its visits. FR-057 a listener that throws is named and removed once, and a log file that cannot be written is said once and the run goes on. FR-043 to FR-045, FR-053 and FR-189 need the engine or ClashRunner, FR-047 needs ClashRunner, FR-049 resets the whole state of a log across runs and is larger than a Core fix | 148 | merged, 4c33402 |
| F121 | FR-151 and FR-164 a count before the clear that could not be taken reads as unknown and holds the NWF shut. FR-158 the probe counts a category's elements by the rule that asked, trimmed and without case. FR-159 and FR-165 the check before a run names a name that cannot be used, an emptied pattern field or a cleared name cell. FR-154 the scan findings judge a building once by its building code and never call a discipline one | 150 | merged, 3884def |
| F123 | FR-007 two names that differ by an ordinary space are described by the space and its place and not as invisible characters. FR-061 the text log says a collapsed line is kept in the .tsv only where the .tsv opened. FR-064 the category list names the folder it was measured on off its own data file and the HEALTH lines carry it | 151 | merged, dfe0fb0 |
| F123 | Part 2, six wordings that said more or less than was known. FR-126 the single discipline detail says which tests are created and that none is run. FR-127 the tolerance help line says results and statuses are kept and never that they are reset. FR-129 a failure after good tests is no longer called one of the first tests. FR-130 the refill counts the names it kept, a name and not a row. FR-131 an NWF folder inside the scanned folder is said as that and not as unreadable. FR-132 the rebuild help line names the removal of an unused set. Each has a test that fails on the old words, 8 failing before the change and 0 after. The reader found a framework message could reach the Outputs line through the new catch, which is fixed and tested before the merge | 154 | merged, 46a6f68 |
| F123 | FR-168 a folders file that could not be used is named with why in the startup block and is never read as a first run, with a test that holds on every machine and one that needs a locked file and runs on Windows only. Core tests 2377 before and 2380 after, 0 failed | 155 | merged, 750cd51 |

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

It stopped on 7 Oct 2026 after F123, the night's list done as far as Core alone can take it. Waves 4 and 5 were read item by item after Bader's word to carry on, and what was left is the add-in, the loop's own files, or waits on F114 and F132, each recorded under its heading below. F128 was
not started, for three reasons. Bader's own item says to measure first which property and value name
Generic Models on 1A02MM and 1A04PK, and that needs a probe through the guarded start of Navisworks,
UNKNOWN until read. The counts per model file, the GENERIC block and the sheet all take a read of the
document that only the add-in makes, so a Core plan of the set folder, one search set per model on the
category and LcOaNodeSourceFile, would be called by no running code, which breaks the no member without a
caller rule. And the workbook writer and the engine are files an open branch of the laptop lane changes.
What can be written in Core once the probe has answered is the set plan and the counts rule, with the
add-in call in the same pull request.

Every item above that lane B left, and every point its readers raised, is under its own heading below,
for the laptop lane to take in the order it chooses. The pull requests that merged are 142, 145, 146, 147
and 148, and each ran its Core tests under mono here and on the Windows runner of Actions.

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

## F121 points lane B left, for the laptop lane

- Add-in or window work that no Core test proves: FR-150 and FR-153 and FR-156 and FR-157 and FR-160 and
  FR-161 and FR-163, the add-in halves of FR-164 (DocumentCensusReader returning zero for a null
  collection, which must land after FR-151's fix in this pull request, as the item says) and FR-162
  (Remember returning the reason, which nothing in the window reads yet), and FR-152, where the fix is an
  undo that writes its own record in ClashStatusEditor and Judge reading the last one
- FR-155 asks the reader to skip a test by name or refuse where merge_composites, selfintersect, primtypes
  or flags is missing or unreadable. Skipping needs a new skip reason through the plan, the coverage and
  the skip lines, and which of the two Bader wants is not written down, so lane B did not choose. FR-166
  needs a parse of the Other tolerance text that does not throw, which no running code would call until
  FederatorWindow does. FR-202 is an IL read first
- The scan findings still write a SINGLE DISCIPLINE finding for every group under both one discipline
  modes, FR-154's note, and FR-167 has the same root in SourceMismatchFindings. A group spanning buildings
  has no building code and is left out of the odd shape and near match findings
- The window's check before a run now refuses a name that cannot be used, and the preview still prints the
  sentence for such a row, FederatorWindow near line 621, and SafeName has no caller

## Found at the second reading of F121, for the laptop lane

- The window's preview reads WhyTheRunCannotStart off the whole name table, FederatorWindow near line 648,
  and OnRun reads it off the ticked groups only, near line 1764, so the two can give different counts for
  the same table, and a cleared name cell of an unticked group makes the preview say THE RUN CANNOT
  START where the run would start. The preview should read the same Only(TickedGroupKeys()) table
- A hand typed name with a character Windows refuses, such as a colon or a slash, still passes
  WhyTheRunCannotStart and reaches the write, and NameCollision.Sentence names every group of a
  collision where the repeat rule would name five

## F123 points lane B left, for the laptop lane

- FR-168 changed two lines of the FOLDERS REMEMBERED block. tools\loop\run.ps1 near line 1428 still matches
  the old first run line by string, StartsWith Nothing remembered yet, and the new lines do not match it. The
  loop's fallback masks any line within a second of the block's first, so nothing leaks today, and the string
  is the loop's to update. The block is written once at window open, so a folders file that fails to save
  later still never reaches the log, which is FR-162
- FR-061 says the .tsv holds a collapsed line where the .tsv opened. A write that fails after it opened is
  not seen by that sentence, so the line can still say the rows are in a file that stopped taking them. The
  log would have to tell the text log the moment the second file stops, which is RunLog state the add-in's
  window reads, and lane B did not widen the fix to it
- Three places in the add-in still say what Core no longer does, found by the reader of part 2. GroupRow.cs
  near line 257 says One discipline, so every test is created and none is run, and its comment near line 155
  says the same, where Core now says only the tests whose two sides both find something are created.
  FederationEngine.cs near line 499 says the run was stopped with StopTheRunReason, which is the guard's log
  wording and ends in what was thrown, where the window should say RepeatedFailureGuard.Stopped. Both are
  for the laptop lane to read off Core. The engine file is one fix-F114 changes, so lane B left both
- FR-059 and FR-063 and FR-060 need the engine or ClashRunner to hand Core the picture count, the plan
  source or the tests run. FR-038 and FR-039 are in WorkbookWriter.cs and FR-074 in Views, which open
  branches of the laptop lane change. FR-042 goes with FR-199
- The category line names the folder from a folder line lane B added to revit-categories.txt, C02, read
  off that file's own header, and the same list is not a list of the other folders

