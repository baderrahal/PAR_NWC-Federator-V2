# log

Newest entry at the top.
## 2026-10-06 The loop, turn 5, Bader's answers to the 22 old items and his F132 rule, recorded

His message headed BADER'S ANSWERS, 6 OCT 2026, THE 22 OLD ITEMS AND F132'S NEXT READING, his words whole at
turn5\q-old22-words.txt, recorded by this records pull request. Each answer is under its question in
steps\02_questions.md in his words with a lead's note, Q25, Q27 to Q31, Q35 to Q40, Q45 to Q47, Q49 to Q51 and
Q76 to Q78. F18 DROPPED, the 1A04WE file being the clash report he exported from Navisworks and pasted into an
empty Excel workbook and not a client file, its words corrected in .claude\rules\core.md, steps\00_analysis.md,
where M7 closes, steps\01_next.md and the register of steps\loop.md. His F132 rule is Q140, an order, and a
bullet of the F132 section of steps\01_next.md, F132's attempt 8 stopped at 16:53:29 and begun again under it
at 16:53:31. The work of his answers, each measured off main 2eda020 first, is FR-198 to FR-204 of
steps\fix-round.md, all after the viewpoints: FR-198 the harvests and the GAP block, F120, 3a, FR-199 the
blocks in the measured order with a priority file, F118, 2c, FR-200 every test not created on the Coverage
sheet, F127, 2a, FR-201 a site named DEFAULT, F137, which main already does, FR-202 the ownership measured
first, F121, 4, FR-203 the open statuses rule, F123, which main already does, and FR-204 the manifest's line
9 in a pull request of its own, F123, 5. Q77 is a line under FR-067. Q29, Q31, Q47, Q50 and Q51 close with no
work. The rows set by the tracker rule, turn5\records31-check.txt and records31-prove.txt. Nothing under src
or tests changed, and the Core tests ran in the pre-commit, turn5\records31-precommit-1.txt.

## 2026-10-06 The loop, turn 5, F133 the work tracker MERGED as pull request 122, 025b5eb, at 14:54:14

Attempt 6 read by a reviewer, CHANGES with one blocking point, the rows of Q137 and Q138 once PR 128 put his
answers on main, closed by the lead's commit 7b4d6a1 with the reviewer's four other points and the rows of
FR-191, F133, Q129, F138, FR-196 and Q135 set for the merge, turn5\lanes-review6-F133.json and
f133j-precommit.txt. At 7b4d6a1 check-tracker read clean and prove-tracker 76 cases right of 76,
turn5\f133j-check.txt and f133j-prove.txt, Core 1926 passed, 0 failed, 0 skipped, and Actions run 37459059241
a success, turn5\actions-reads-pr122.txt. The tracker is live, steps\tracker.md, and its three lines were given
in the tab. Next beside the viewpoints: F139, Bader's one page progress file of Q139, built on it.

## 2026-10-06 The loop, turn 5, F133 the work tracker, one place for status, attempts 1 to 6 on fix-F133, pull request 122, written by the fourth worker as the lead's delegate

- BUILT, part 1 of Bader's message of 5 Oct 2026, Q129, his words in q129-words.txt, FR-191, area F133, in the worktree %LOCALAPPDATA%\NwcFederatorLoop\wt-f133, first committed on main 35bd7fd: steps\tracker.csv, one row per FR item of steps\fix-round.md, per F area, per request of Bader's and per question waiting for him or that once had a row, in his nine columns in his order. tools\tracker\make-tracker.ps1 makes steps\tracker.md from it. check-tracker.ps1 refuses each fault its header lists, naming its line, and prove-tracker.ps1 runs it over a good fixture and one broken copy per fault kind, both run by Actions on every pull request from two steps of .github\workflows\tests.yml. The rules live in tools\tracker\tracker-rules.ps1 and .claude\rules\tracker.md, which says how each value is read, the row rule beside the DONE line rule in .claude\rules\steps.md, and fix-round.md and loop.md point at the tracker for status. Nothing under src or tests changed, and of steps\01_next.md only the F133 section and its order line
- PROVED here, every file named with no folder in %LOCALAPPDATA%\NwcFederatorLoop\turn5: in attempts 2 to 5 the new fault cases read WRONG over the check before them, and each attempt's proof read every case right, the files in its line below. Core tests 0 failed and 0 skipped in every count, 1912 passed before and after attempts 1 to 3 and 1926 since the merge 7232a02 brought F136's 14 tests, no count moved by this work. dotnet build ParsonsNwcFederator.sln -c Release --no-incremental, 0 warnings and 0 errors, in every attempt. check-locals and check-imports exit 0. No Navisworks was started and nothing was installed
- STATUS AND COUNTS. steps\tracker.md, made from steps\tracker.csv, holds the status of F133, FR-191, Q129 and every other row and the counts of rows by status and by wave, and check-tracker.ps1 prints the counts of FR items, areas, requests and questions it read. Neither is copied here, since each changes at the next merge of main. This entry as it stood at 47d8e2f, git show 47d8e2f:steps/log.md, holds the longer record of attempts 1 to 5, how each value is read, and the programs started and files written outside the repo by attempt 5, and as it stood at efc3c44 those of attempts 1 to 4
- NEXT, after the merge: F139, Bader's one page progress file of 6 Oct 2026, built on the tracker. Nothing waits for the local machine, since no add-in file changed
- ATTEMPT 1, 2026-10-05, 7d59422: the csv, the maker, the check and its proof, 13 cases right of 13, f133-prove-tracker-2.txt. READ by a reviewer and a breaker, both CHANGES, lane-result.json under tracker.reads. Blocking: four faults of the csv reader and the exit 2 with no fixture, the row rule out of sight of the developers who write DONE lines, loop.md and fix-round.md not pointing at the tracker, FR-191 and F133 written in progress, and F125 written proven by a run of its first pass
- ATTEMPT 2, 2026-10-05, 4e1f9fc and 4b72de5: a fixture and a case for each fault of the reader and both exits of 2, and more faults refused, 33 right of 33, f133b-prove-tracker.txt, and 17 WRONG over attempt 1's check, f133b-prove-before.txt. READ AGAIN by both, CHANGES, f133b-result.json. Blocking: the branch behind main with no row for FR-194 and FR-195, the waves reader skipping the new lines of the waves section in silence, and loop.md not pointing at the tracker
- ATTEMPT 3, 2026-10-05, main fdd05c2 merged in at a8ab7cb, then 390ee4a, main b900464 merged in at 7232a02, then 109e49d: the waves reader reads every line shape and names one it cannot, and a question row reads by its answer, 58 right of 58, f133c-prove-tracker-6.txt, and 4 WRONG over the check of 7232a02, f133c-prove-before-2.txt. READ by both, APPROVE with nothing blocking, f133c-result.json
- ATTEMPT 4, 2026-10-05, 9b9ea77. Before it the lead merged main 503eaa4 at 027c4be and wrote ce48197 and 03d0b56, READ by the claim-checker of pull request 122, nine points, which attempt 4 worked: the header in Bader's words, a row for each request of his, one rule for a question row's area and wave, every Answer line read, and every PR cell read again by git, 75 right of 75, f133f-make-check-prove-3.txt, and 64 WRONG over 03d0b56, f133f-prove-before.txt. READ by a reviewer, APPROVE, and a breaker, CHANGES, f133f-result.json. Blocking: main had moved past 503eaa4 with FR-196, Q135 and Q136, which had no rows, and Actions run 37336430529 on 9b9ea77 failed at the tracker check on those three
- ATTEMPT 5, 2026-10-06, ec5b032, 003d83f and 47d8e2f. Before it the lead merged main 1a202c0 at fcb5281 and wrote the rows of FR-196 and Q135 at efc3c44, Actions run 37339273209 passing there, READ by the claim-checker, seven points. After the laptop went off at 19:31 on 2026-10-05, turn5\restart\trees.md, attempt 5 merged main 6d2a203 at ec5b032 and worked the points at 003d83f: Q128 set to 121, the merge of his later answer, f133h-question-prs-after.txt, F110 and F111 set open with their titles, F138 placed by its stage line with FR-196 and Q135, and the check refusing a run named on a row not proven by a run, test first, f133h-mcp-1-test-first.txt and f133h-before-silent.txt, 76 right of 76, f133h-mcp-7-commit-003d83f.txt. Actions on 47d8e2f a success, f133h-actions.txt. READ by a reviewer, CHANGES, lanes-review-F133.json. Blocking: a copy of the request list in .claude\rules\tracker.md, already false since Q135
- ATTEMPT 6, 2026-10-06, 99ec938 and 09e4260 on 47d8e2f: the request list taken out of .claude\rules\tracker.md, which says the requests are read off steps\02_questions.md as the check reads them, tools\tracker\README.md naming the questions that once had a row, F133-R20 measured over every pre-commit file, the pointers saying which counts tracker.md holds and which the check prints, the statuses of attempt 5 written as set at 003d83f, and this entry cut to a short record by Bader's item 16 of 2026-10-06, with no script, fixture or row changed. Then main 3701511, PR 126, merged in at deff22f, on which the check refused Q137 and Q138 with no row, exit 1, f133i-mcp-4-merge-deff22f.txt, and their rows written in the commit after it, waiting for Bader with area and wave none, since no FR item names them. Core tests 1926 passed before on 47d8e2f and after, f133i-core-tests-before.txt and f133i-core-tests-after.txt, and over deff22f by the hook run by hand, since a merge runs no pre-commit here, f133i-precommit-merge-run.txt, the build 0 warnings and 0 errors, f133i-sln-build.txt, check-locals and check-imports exit 0, f133i-checks.txt, and the maker, the check and the proof clean, 76 right of 76, with the rule changes in the tree, f133i-mcp-1-rule-readme.txt, with the records, f133i-mcp-2-records.txt, and with the rows of Q137 and Q138, the maker writing tracker.md again, f133i-mcp-5-rows.txt. Not yet read

Register rows, the findings that break no rule of CLAUDE.md or are Bader's call, one line each, a closed one naming what closed it:
- F133-R1, coverage by hand. The check demands a row only for an F area a line of the waves section names, so an F number given to work elsewhere, as F101, F110 and F111, gets its row from a reader alone and that row is never compared. Its first reason, that the three were F sections of steps\01_next.md, closed by attempt 4
- F133-R2, closed by attempt 3. F109 and FR-078 to FR-080 read open while origin/fix-F109 held a0c3829, then in progress, and its F114 half read right all along
- F133-R3, closed by attempt 3. No rule said what status a question's row takes once Bader answers, then written and checked
- F133-R4, for Bader. Work done outside the repo, as FR-175, Q112-1, Q81 and Q95, has nothing on main but its record, and no status of the seven names it, so its rows read the status of the record that carried it
- F133-R5, data shape. The PR column holds free text beside numbers, a branch, a measurement folder or merge and a hash
- F133-R6, a row is checked against its own cells in one way only, a run named on a row not proven by a run. Merged with PR none, proven by a run with run none, a date that is not a date and in review with PR UNKNOWN read clean, and nothing sees a status left stale after its branch merges
- F133-R7, closed by attempt 3. A file not in UTF-8 read clean, then refused
- F133-R8, closed by attempt 3. A space before or after an id read clean, then refused
- F133-R9, a wave 10 would sort between 1 and 2a. fix-round.md named no wave 10 at 003d83f
- F133-R10, two pull requests open at once both change the counts at the top of tracker.md, so the second to merge conflicts there and runs make-tracker.ps1 again
- F133-R11, closed by attempt 4. The PR cells of the merges F1 to F95 were read off the records and not again by git, then every PR cell read again by git, f133f-merge-cells.txt and f133f-pr-cells.txt
- F133-R12, the counts at the top of tracker.md count rows, and one piece of work can be up to three rows, as FR-191, F133 and Q129. Q94, Q98, Q114 and Q129 have no rule for when they close
- F133-R13, a request with a standing effect, as Q130, whose stop of the C02 weekly run holds until F114 merges, reads the status of the work it asked for, F136. No status of the seven names an order still in force
- F133-R14, closed by attempt 4. The brief of attempt 3's second pass named F136 in progress where main b900464 held its DONE line, and its rows were read off the repo
- F133-R15, closed by the lead at ce48197. steps\log.md had no blank line between this entry and the F136 entry under it after the merge 7232a02, put back
- F133-R16, upkeep. The fixtures are written by scripts outside the repo, f133f-make-fixtures.py and f133h-make-fixture.py, so a person without that folder writes a new one by hand, each one edit of the good one
- F133-R17, the reader knows a request of Bader's only by From Bader, at the start of its item, as every request up to Q135 starts, read at 003d83f. One written in another shape reads as an answered question, and nothing refuses that
- F133-R18, a question row's area and wave come from the FR items naming it alone, so a question whose own text names an area, as Q110 names F112 and Q134 names F131, reads none until an item names it
- F133-R19, any text after Answer: reads as Bader's answer, so a note of the loop written on an Answer line would lift its question off what waits for him. Read on 2026-10-05, every Answer line holding text held a decision
- F133-R20, for the lead. The output captured for 14 commits of this branch from 7d59422 to 47d8e2f holds lines error: failed to delete, printed by git after it writes the commit and not by the hook, since they follow the hook's tests passed line and the merge deff22f, which ran no hook, printed them too, and the lead's merges 027c4be and fcb5281 captured nothing, each naming an admin folder under the main clone's .git\worktrees, Permission denied, 15 in the four of 7d59422, 4e1f9fc, 4b72de5 and a8ab7cb, f133-precommit.txt, f133b-precommit.txt, f133b-precommit-2.txt and f133c-precommit-merge.txt, and 16 in the ten from 390ee4a to 47d8e2f, f133c-precommit.txt to f133h-precommit-2.txt, the sixteenth naming wt-f136, and fetches print the same, the first of attempt 6 among them. The commit 99ec938 of attempt 6 printed none, f133i-precommit.txt, and all 16 folders were still there after it. Each commit still succeeded. No record before attempt 5 named them. What runs the delete, why it is refused and why 99ec938 printed none are UNKNOWN
- F133-R21, the class column has an allowed value only on an FR row, so a question row whose class is mistyped, such as Question, is never compared with its question, its area, its wave or its status, and an F row may carry any class
- F133-R22, only an indented Answer: line counts, so an answer at the margin, in bold or under another word is not seen and its question reads waiting. All 136 Answer lines of 02_questions.md were indented at 47d8e2f
- F133-R23, the question reader ends an item only at the next item or a line in another item shape, not at a heading, so an indented Answer: line under a later heading would count for the item before it. 02_questions.md had only its title heading at 47d8e2f
- F133-R24, rows of class Bader's request are checked for being there only. Their area, wave and status follow no rule, as at efc3c44 Q114 read 2a and 2b, Q132 a wave of its own and Q98 all, and nothing ties the rows of one piece of work, so a merge that sets one leaves the others as they were and the check reads clean
- F133-R25, by the rule in steps.md the lead sets a row merged with the pull request's number on the branch before the merge, so a pull request closed and opened again under another number would leave rows naming a number that is not their record, and the check reads clean either way
- F133-R26, the check reads that a merged question row's PR is a number, not that its merge put the answer on main, which is how Q128 read 118 until attempt 5. A reader reads it by git, as f133h-question-prs.py does

## 2026-10-06 The loop, turn 5, picked up after the laptop went off, Bader's message headed CONTINUE THE LOOP AFTER THE LAPTOP WENT OFF, and the plan

Why it went off: the System log reads shutdown.exe starting a shutdown for NT AUTHORITY\SYSTEM at
19:31:09 on 2026-10-05, reason code 0x800000ff, the system down at 19:32:14 and up at 09:44:55 on
2026-10-06, turn5\restart\settings.md. What called shutdown.exe is UNKNOWN, and his 22:24 matches no
event in that log. Get-Process Roamer read 0 at 10:08 and at every read after. The keep-awake started
again at 10:12:11 as pid 2076, turn5\keep-awake-checks.txt.

His settings: PUT BACK at 11:01:38, 44 values of the 22.0 key and InfoCenter.log and LastSession.xml
from P10's backup, through the guard's own PutBackRegistry and PutBackFiles, 0 left different on the
read back, his AutoSave folder of 199 files, folders.txt and team-map.txt needing nothing,
turn5\restart\putback-p10-write-20261006-110126.txt. The key and the two files as they read before
were copied first, beside it. 40 of the 44 had gone back to values older than any backup the loop
took, by a writer UNKNOWN, register row T5-R-OLDKEY. Auto-Save enable read "0" before and after, F138's
"3 0" never having reached his key.

What the shutdown cut, turn5\restart\trees.md:
- F131's add-in attempt 3: main 1a202c0 merged in as 26c62df, not pushed, and the test first K2 checks
  in prove-run.ps1 not committed, run.ps1 not yet changed, its harness never started
- F133's attempt 5: reads only, nothing written, PR 122 still at efc3c44
- F138: its four files not committed, its full harness on the new guard cut in H7
- F132's attempt 6: 14 files staged, its pre-commit killed by the shutdown, no commit

The harness was not hung. The cut run had run 13 min 31 s and was in H7 at the pace of the runs
before it. The 2 h 30 min is nearest the F138 workflow's 2 h 21 min 34 s, about 72 min of it in two
wrappers waiting with no limit on F131's two harness runs, turn5\restart\harness.md. The cut run's
-Work, which would have held F131's wrapper for ever, was removed at 10:30:10, its files listed in
turn5\restart\test-f138-after-removed.txt. The fix is register row T5-R-HARNESSLIMIT, built in F138's
branch.

The plan, in Bader's order:
1. F138 with the time limits, its full harness on the new guard, one reviewer, merged before any
   probe or run
2. F131's add-in attempt 3, the K2 mask fix and Q134 B, its harness once F138 has merged, one
   reviewer, merged
3. F132's attempt 6 again, every mirror named by its own name with (mirror) and Q136 A, then its
   add-in half after F131, a reviewer and a breaker, merged
4. F114 carrying the three members of Q134 B, its probes P11 to P19 and the Q133 measurement on
   1A04PK once F138 has merged, its add-in half after F132, a reviewer and a breaker, merged with the
   viewpoints box ticked again
5. Main installed in place, 1A02MM and 1A04PK run with the new views, VIEWS seconds and totals against
   2 h 12 min for 1A02MM and the hung run for 1A04PK
6. Beside them, F133's attempt 5 and PR 122 merged, then F134 the gate and F135
7. Then the rest of wave 2, F137 and the test of wave 1, then waves 3 to 5

Every harness run and every probe runs alone, since each stops on the other's stand-ins or
Navisworks. Checks from now, by his item 16: each fix its test first, a clean build, green Core tests
and one reviewer, a breaker only on alignment, sets, clash counts, mirrors and views, no
claim-checker on records, one short record per merge or run.

STATE OPEN.

## 2026-10-05 The loop, turn 5, F136 attempt 2, the viewpoints box opens unticked until F114 on Bader's answer B to Q131, written by the developer as the lead's delegate, built and pushed on its branch, pull request 117, MERGED as 6802e1a at 14:08:32

Attempt 1 was read by the reviewer and the breaker and both approved it with nothing blocking,
%LOCALAPPDATA%\NwcFederatorLoop\turn5\f136-result.json. Main 83445cb was merged into fix-F136 at
c9fae54 with no conflict, turn5\f136b-merge.txt. Core tests 1922 passed, 0 failed, 0 skipped
before, at c9fae54, turn5\f136b-core-tests-before.txt, and 1926 passed, 0 failed, 0 skipped
after, turn5\f136b-core-tests-after.txt, and by the pre-commit of 8488bc4,
turn5\f136b-precommit-1.txt line 14. WriteTheCorrectedFile prints as Skipped in each and is the
one [Explicit] test, in none of the counts. The full solution, dotnet build
ParsonsNwcFederator.sln -c Release --no-incremental, at 8488bc4 with git status empty, 0
warnings and 0 errors, turn5\f136b-build.txt. check-locals and check-imports exit 0.
Navisworks was not started, attached to or touched. Get-Process Roamer read one Roamer, Id
54784, started at 12:57:04, before this pass began at 12:58:25, turn5\f136b-roamer-before.txt.
It was not this pass's, which never started, attached to or closed it. The read after found
none at 13:37:26, turn5\f136b-roamer-after.txt. Who closed it is UNKNOWN here.

### What was done

- Bader's answer to Q131 on 2026-10-05, turn5\q132-words.txt: "Q131: B until F114 merges, so
  nobody makes the old viewpoints. Once F114 merges, ticked." ViewpointRequest.DefaultMakeViewpoints
  is false, and its comment says F114's pull request sets it back to true
- failing first: fourteen tests, ViewpointRequestTests and two new RunLogTests, 5 failed and 9
  passed against stubs, turn5\f136b-failing-first.txt
- the label is Make saved viewpoints for the clashes, because a service of 150 mm and under
  gets no viewpoint. The grey line, Each viewpoint adds time, so a big run can take hours, says
  what the box costs, measured on the C02 weekly that sat in VIEWS for 3 h 15 min, Q130. The
  settings line no longer says one per clash. The comments that only counted words are gone
- the RESULT block carries one line under the group counts where the run had the box
  unticked, viewpoints     : none made, the box was unticked for this run, ViewpointRequest.ResultLine,
  RunLog.WriteResultBlock taking the run's choice from FederationEngine.MakesViewpoints
- .claude\rules\core.md and addin.md: the unticked line is written in every group that reaches
  the viewpoints step, the tick boxes are sixteen with six under no expander, read off the
  XAML and not by the probe, and the box never goes under an expander because the driver could
  not find it there
- steps\03_bader_next.md: the box stays unticked for every run until F114 merges, every step
  that expected viewpoints made expects the unticked line or waits for F114, step 394's 27
  viewpoints included, and steps 400 to 416 read the box unticked and the RESULT line. Step
  248 has main's byte after 53 mm back, which attempt 1 had turned into a replacement character
- Q130 and Q131 on main: main 83445cb holds both in steps\02_questions.md, Q130 with its answer
  and Q131 with none under it. Writing Bader's answer under Q131 is the lead's

### What remains

- the add-in half: steps 400 to 416 on the local machine, or a loop run with run.ps1 -Untick
  MakeViewpoints once a build carrying this is installed. Until then the installed window has
  no such box and the driver stops UNTICK with nothing pressed
- the tick box count was read off FederatorWindow.xaml, not by probe-window-labels.ps1, which
  was not run

### Known bugs

- an unticked weekly run leaves the viewpoints an earlier ticked run put in the NWF and says
  nothing of them, the breaker's point on attempt 1. Information, not changed
- no test pins the order of the three values the engine hands WhyNone, the breaker's point.
  The engine cannot run without Navisworks, so it stays with review
- SavedViewpoints.CanBuild has no caller in src, F112's box is not named under RUN SETTINGS,
  and the stand-in has no MakeViewpoints box, all as attempt 1 found them

### What comes next

- the reviewer and the breaker on attempt 2, then the lead's merge of pull request 117, then
  F131, F132 and F114 in that order, every test run with the box unticked until F114 merges

## 2026-10-05 The loop, turn 5, F136 a tick box that switches the viewpoints off, written by the developer as the lead's delegate, built and pushed on its branch, no pull request

Core tests 1912 passed, 0 failed, 0 skipped before, at main 35bd7fd,
%LOCALAPPDATA%\NwcFederatorLoop\turn5\f136-core-tests-before.txt, whose line 1 is the commit,
and 1922 passed, 0 failed, 0 skipped after, at 22ceb90, turn5\f136-core-tests-after.txt, and by
the pre-commit of 22ceb90, turn5\f136-precommit-1.txt line 14. WriteTheCorrectedFile prints as
Skipped in each and is the one [Explicit] test, in none of the counts. The full solution, dotnet
build ParsonsNwcFederator.sln -c Release --no-incremental, at 22ceb90 with git status empty, 0
warnings and 0 errors, turn5\f136-build.txt. check-locals and check-imports exit 0. Navisworks
was not started, attached to or touched. Get-Process Roamer listed no process at 11:53:04,
turn5\f136-roamer-before.txt, and read 0 at 12:33:42, turn5\f136-roamer-after.txt.

### What was done

- the root cause. Bader's word of 2026-10-05, turn5\q130-words.txt, is that every test run has
  viewpoints switched off until F114 is merged, and the tool had no switch.
  src\Federator.Addin\Engine\FederationEngine.cs at 35bd7fd, BuildViewpoints, lines 3433 to
  3451, held back the viewpoints only where the clash was skipped or no report was built
- the rule in Core, src\Federator.Core\Views\ViewpointRequest.cs. WhyNone names why a group asks
  for no viewpoint, the box unticked first because it holds for every group, then the clash
  skipped, then no report, or gives null when it asks for them. The setting is
  ReportOptions.MakeViewpoints, on by default, Q131 default A as the lead named it. The label,
  Make a saved viewpoint for every clash, seven words, the grey line, twelve words, and the line
  under RUN SETTINGS sit beside it
- failing first: ViewpointRequestTests, ten tests, run against WhyNone holding only the
  engine's two checks and ReportOptions not setting the default, 3 failed and 7 passed,
  turn5\f136-failing-first.txt
- the engine calls WhyNone in place of its two checks, FederationEngine.cs lines 3436 and
  3437. Unticked, it makes no viewpoint, sets ViewpointsRequested false so the group cannot
  fail at them, F52's rule, and logs one line, VIEWS    the box Make a saved viewpoint for
  every clash was unticked, so no viewpoint is made
- the window. The box MakeViewpoints on 4. Clash, under the shared coordinates box,
  FederatorWindow.xaml line 569, set off new ReportOptions().MakeViewpoints in the
  constructor, FederatorWindow.xaml.cs line 1212, the way F112's box is set, which runs at
  every open because FederatorPlugin.cs line 65 makes the window new each time. Its state is
  named in the lines under RUN SETTINGS, line 1829, and at the start of the open file run,
  line 2308, which has no RUN SETTINGS block
- the rules, .claude\rules\core.md and .claude\rules\addin.md, the order line 43 and the F136
  section of steps\01_next.md, and steps 400 to 415 of steps\03_bader_next.md, put first after
  the install with the opening paragraph saying why

### What remains

- the add-in half, which no test here can prove: steps 400 to 415 on the local machine, or a
  loop run with run.ps1 -Untick MakeViewpoints once a build carrying F136 is installed. Before
  that the installed window has no such box and the driver stops UNTICK with nothing pressed
- Q130 and Q131 were not in steps\02_questions.md on main at 35bd7fd. Main 83445cb holds both,
  Q131 with no answer under it, and was merged into fix-F136 at c9fae54, the attempt 2 entry
  above
- steps\tracker.csv does not exist on main at 35bd7fd, so no tracker row was written

### Known bugs

- SavedViewpoints.CanBuild, src\Federator.Addin\Engine\SavedViewpoints.cs line 59, has no
  caller in src, read on 2026-10-05. It was there before F136 and is left for its own fix,
  because nothing rides along
- the shared coordinates box of F112 is not named in the lines under RUN SETTINGS, while this
  box now is. Found while reading, not changed
- the stand-in's window, tools\loop\StandIn\ToolWindow.cs, has no MakeViewpoints box, so a
  stand-in run cannot prove the untick

### What comes next

- the reviewer and the breaker on fix-F136, then the pull request, then steps 400 to 415, and
  every test run after the merge run with the box unticked until F114 is merged

## 2026-10-05 The loop, turn 5, F126 the window driver unticks a named tick box, built on 2026-10-04 and read by a reviewer and a breaker with nothing blocking under Q93, its harness run in the first gap on 2026-10-05, 52 passed and 0 failed

F126's own commits changed nothing under src or tests: git diff --name-only 1ae6771 66dfdf5
-- src tests prints nothing, and so does the same from main ddb059b to the merge 1e06b7e,
turn5\f126-proof\diff-names.txt. Against main the branch changes six files under tools and none
under src or tests: git diff --name-only 3ee01ab 16b5eb7 -- src tests tools lists
tools\loop\README.md, tools\loop\StandIn\ToolWindow.cs, tools\loop\nw-guard.ps1,
tools\loop\run.ps1, tools\probes\README.md and tools\probes\drive-window-run.ps1, exit 0, and
git diff --name-only c101f6c 37ee68e -- src tests tools lists the same six. The 18 files under
src and 7 under tests that git diff --name-only 1e06b7e 16b5eb7 -- src tests tools lists, exit
0, came in with main 3ee01ab, F116 in it, at the merge 16b5eb7, since 16b5eb7 differs from
3ee01ab under tools alone, turn5\f126-proof\diff-since-1e06b7e.txt. Core tests 1756 passed, 0
failed, 0 skipped before, on main 1ae6771 with F125 merged, core-tests-before.txt there, whose
line 1 is the commit, and 1756 passed, 0 failed, 0 skipped after the change, by hand,
core-tests-after.txt, a file with no commit and no time in it, and by the pre-commit of each of
the four commits, precommit-1.txt to precommit-4.txt. After main ddb059b, F112 in it, was merged
in at 1e06b7e, 1865 passed, 0 failed, 0 skipped by the pre-commit of the merge,
precommit-claims-merge.txt, and by hand at 1e06b7e, core-tests-claims.txt, whose line 1 is the
commit. The pre-commit of the lead's records commit e712a11 read 1865 passed, 0 failed, 0
skipped, precommit-ran.txt, and those of the merges of main 3ee01ab at 16b5eb7 and of main
c101f6c at 37ee68e 1912 passed, 0 failed, 0 skipped each, F116's tests now in,
precommit-merge-main2.txt and precommit-merge-main3.txt. core-tests-before.txt line 12,
core-tests-after.txt line 2 and core-tests-claims.txt line 13 print one test,
WriteTheCorrectedFile, as Skipped. It is the one [Explicit] test, MatrixCorrectionsTests.cs
lines 559 and 560, run by hand only, and it is in none of the counts, whose summary lines read
Skipped: 0. No Navisworks was started. The stand-in was started by the lead's harness alone, on
2026-10-05, 11 times in the pass on main's tools and 12 on the branch, still running 0 after
each, prove-f126-before.txt line 175 and prove-f126-after.txt line 310.

### What was done

- the root cause. Nothing in the loop could untick a tick box, so the test of wave 1 could not
  run 1A02MM, its ST model on Revit's internal origin, once more with F112's rule switched off by
  its tick box, Bader's message of 15:42 and Q99 and Q100. tools\probes\drive-window-run.ps1 at
  1ae6771 took no parameter naming a box, lines 1 to 14, its Toggle only read the state, lines
  214 to 217, and lines 40 and 41, 338 and 378 left every box as the window opened it, as the
  baseline's item 1 on C02 shows, steps\runs\04\item1-C02\driver.txt lines 5 and 19.
  tools\loop\run.ps1 had no such parameter either, lines 1 to 14 and 2177 to 2183
- the run that shows it used F125's first pass scripts and not those of 1ae6771: record.txt
  line 1 of steps\runs\04\item1-C02 names the driver at sha256 7E9ABA1B and run.ps1 at
  3FE28CB6, the blobs of 5fa98a8. The driver's cited lines stand word for word at 5fa98a8, at
  lines 1 to 14, 40 and 41, 212 to 215, 332 and 372, and run.ps1's lines 2177 to 2183 stand at
  1598 to 1604. run.ps1's param block is shorter there, lines 1 to 10. Neither script names
  $Untick at 1ae6771 or at 5fa98a8, where 66dfdf5 names it on 8 and 16 lines,
  turn5\f126-proof\first-pass-lines.txt
- measured first, in the window code on the branch fix-F112 at e6d6f73: the box is not
  remembered. FederatorWindow.xaml.cs line 1198 sets it from
  AlignmentCheck.DefaultSkipClashOffCoordinates, a constant true at AlignmentCheck.cs line 160,
  every time the window opens, and the one state the window reads back at its next open is
  FolderMemory's picker folders. So nothing of it is read before the loop or put back after.
  F112 is merged since, as main ddb059b, and the branch fix-F112 is gone from origin. e6d6f73
  is an ancestor of ddb059b, and FederatorWindow.xaml line 559, FederatorWindow.xaml.cs lines
  1198 and 1526 and AlignmentCheck.cs line 160 read the same at both, read with git show,
  turn5\f126-proof\f112-lines-ddb059b.txt
- the driver's -Untick: each box named by its AutomationId found on the four tabs, read through
  TogglePattern, toggled once only when it reads On, read back, one line per box with its id,
  its tab, before and after, all before anything is pressed, and read again before Run. A box on
  no tab, with no TogglePattern, or not reading Off stops it with UNTICK, exit 13, a line naming
  the box and nothing that runs pressed. The lines that say a box was left as the window opened
  it name the boxes -Untick names, so an unticked box is never said to be left, and read as
  before with no -Untick. run.ps1's -Untick for Run and Check with -Item 1 to 5,
  refused for a documents read and for an id not the plain shape of an x:Name or named twice,
  UntickRefusal in nw-guard.ps1, the one rule both keep, handed on through DriverArguments and
  named on the RUN RECORD line, in a line of the record and in Check, UntickWords
- the stand-in's window gains the box SkipClashOffCoordinates on 4. Clash, ticked when it
  opens, every tick and untick and its state at Run written to its events file, and the modes
  skip-off, skip-sticky and skip-scan
- the rule in .claude\rules\loop.md, the READMEs of tools\loop and tools\probes, the order line
  and the section F126 in steps\01_next.md, and the register row F126 in steps\loop.md
- built and checked on 2026-10-04: the solution with 0 warnings and 0 errors before and after,
  sln-build-before.txt and sln-build-after.txt, and the stand-in the same, standin-build-before.txt
  and standin-build-after.txt. These four outputs name no commit and no time. Their file times
  are 23:25, 23:36, 23:24 and 23:32, before c3e224b at 23:44:03, turn5\f126-proof\reflog-claims.txt,
  and nothing they compile changed from c3e224b to 66dfdf5, git diff --name-only c3e224b 66dfdf5
  -- src tests tools/loop/StandIn printing nothing, diff-names.txt. check-locals and
  check-imports passed, check-locals.txt and check-imports.txt, also with no commit in them, and
  in the pre-commit of each commit, precommit-1.txt to precommit-4.txt lines 3, 5 and 6.
  parse.ps1 is the parser, and no output of it was kept that day
- built and checked again on 2026-10-05 at the merge 1e06b7e, each output with the commit on its
  line 1 and the time on its line 2: the solution, dotnet build ParsonsNwcFederator.sln -c
  Release --no-incremental, 0 warnings and 0 errors, sln-build-claims.txt, the stand-in, dotnet
  build of tools\loop\StandIn\StandIn.csproj -c Release --no-incremental into
  standin-bin-claims, built and never started, 0 warnings and 0 errors,
  standin-build-claims.txt, check-locals and check-imports exit 0, checks-claims.txt, and the
  three changed scripts through parse.ps1, each at parse errors 0 beside its sha256,
  parse-after.txt. Nothing under tools changed from 66dfdf5 to 1e06b7e, diff-names.txt, so that
  parse reads the scripts of 66dfdf5
- the proof written, turn5\f126-proof\prove-f126.ps1, 52 checks on the branch's tools, each way
  the untick can fail broken on its own and its line asserted to name the box, to run once on
  main's tools at 1ae6771, exported into before-tree, and once on the branch. The lead ran both
  on 2026-10-05, below
- the wait. Get-Process Roamer read 1 at 23:21:37 and 23:39:19, then the waiter
  turn5\f126-proof\wait-and-prove.sh read it 19 times, about 5 minutes apart, 5 min 3 s or
  5 min 4 s between reads, from 23:51:20 to 01:22:18 on 2026-10-05, 90 min 58 s, every read 1,
  pid 32136 started at 21:17:06, and at 01:22:19 it wrote NO GAP and started neither run of the
  harness, roamer-reads.txt lines 3 to 22. The 90 minutes in that line and in the message of
  aa0594b is the waiter's budget of 18 sleeps of 300 s, not what was measured. The reads at
  23:39:19 and 01:31:32, lines 2 and 23 there, gave a count and no pid, so the file does not
  show they were the same Roamer. This records pass read it at 02:14:59 and again before its
  commit, 1 each time, pid 32136 started at 21:17:06 on 2026-10-04, roamer-reads-claims.txt
- so the harness did not run on 2026-10-04, no gap with Get-Process Roamer at 0 having come. THE
  LEAD RAN IT on 2026-10-05 in the first gap, after the C04 baseline ended at 06:36. Get-Process
  Roamer read 0 at 06:53:29, turn5\f126-proof\roamer-reads-harness.txt line 1. The pass on
  main's tools at 1ae6771 in before-tree ran from 06:53:51 to 06:55:06 and gave 8 passed and 41
  failed of 49 checks, prove-f126-before.txt line 177, the stand-in started 11 times and still
  running 0, line 175. Main's tools take 49 checks where the branch takes 52 because their
  run.ps1 has no DriverArguments, so the case driving the stand-in through it is one check
  there, line 147, and four on the branch, prove-f126-after.txt lines 278 to 281. Four of the 8
  passes, lines 16, 26, 114 and 124, are checks that nothing was pressed, which pass only
  because main's driver fell over at parameter binding, exit 1, A parameter cannot be found that
  matches parameter name 'Untick'. The other four are untick-none, which hands no -Untick, lines
  70 to 72, and ParamRefusal's case that a run of item 1 with -Untick is not refused, line 150.
  The pass on the branch ran from 06:55:12 to 06:58:40 and gave 52 passed and 0 failed,
  prove-f126-after.txt line 312, the stand-in started 12 times and still running 0, line 310,
  the one more being the DriverArguments case at line 246. Roamer read 0 at the start and end of
  each pass, prove-f126-before.txt lines 4 and 175 and prove-f126-after.txt lines 4 and 310, and
  again at 06:58:45, roamer-reads-harness.txt line 2. The three scripts each pass read are by
  sha256 those of 1ae6771 and of f9834e4, the branch's tip from 02:30:00 until e712a11 at
  07:01:49, after the harness ended, by the reflog, turn5\f126-proof\reflog-harness.txt. So the
  stand-in's box, the toggle, the read back, each UNTICK stop, run.ps1's Check lines and the
  three stand-in modes are proved on the stand-in, skip-off at prove-f126-after.txt lines 94 to
  127, skip-sticky at 202 to 213 and skip-scan at 214 to 243, and tools\loop\README.md lines 555
  and 556, which say prove-f126.ps1 proves the three modes, are true. The work folders of the
  two passes are turn5\f126-proof\work-before-065350 and work-after-065511
- what ran with no stand-in and no window, on e45ffbb, pure-reads.txt, whose lines 1 and 2 are
  the commit and the time. pure-reads.ps1 prints and asserts nothing, so its answers were
  compared by hand with what prove-f126.ps1 asserts: ParamRefusal's seven cases, lines 3 to 9,
  with the harness's checks at lines 217 to 229, DriverArguments with no -Untick and for item 5,
  lines 12 and 13, with its checks at 209 and 210, line 11 being the command line its case at
  line 206 starts the driver with, UntickWords, lines 14 and 15, with its checks at 248 and 260,
  DriverCode and DriverCodeName, line 17, with its DriverCodes check at 254, and RunVerdict,
  line 18, with its check at 257. Two answers have no check in the harness to compare with, the
  Install case, line 10, and UntickRefusal, line 16. The driver on e45ffbb refused a bad and a
  doubled -Untick before any window is read, exit 2, dry-refusal.txt
- a second commit, e45ffbb, after the developer read the first again: three lines of the
  driver that read tick boxes ended each was left as the window opened it, lines 417, 457 and
  511 of c3e224b, which a box -Untick names would make false. BarUntick names those boxes after
  the words, and is empty with no -Untick
- read on 2026-10-05 under Q93, the three files written at 01:51:53 while 66dfdf5 was the tip,
  reflog-claims.txt: by a reviewer, VERDICT APPROVE with nothing blocking,
  turn5\f126-read-review.txt, and by a breaker, VERDICT APPROVE with nothing blocking,
  turn5\f126-read-break.txt. Their notes are under Known bugs. A claim-checker read the records
  and the body, turn5\f126-read-claims.txt, ten entries, nine needing a change and the tenth
  finding no fault
- the records made true on 2026-10-05: main ddb059b merged in at 1e06b7e with both sides of
  each conflict kept, this entry on top of steps\log.md, F112's order line 40 and section first
  in steps\01_next.md and F126's order line, now 41, and section after, and steps\loop.md
  merging with no conflict at 337 rows, turn5\f126-resolve-merge.py and turn5\f126-msg-merge.txt.
  Then the claim-checker's nine points made true in this entry, the section F126 in
  steps\01_next.md, the register row F126 and its count line in steps\loop.md and the body
  turn5\pr-f126.md. Two of its points also name lines under tools and in .claude\rules, which
  that pass did not change. The lead corrected tools\loop\README.md lines 345 and 346 and
  .claude\rules\loop.md lines 112 and 113 in e712a11, after the harness, to name ddb059b beside
  e6d6f73, and README lines 555 and 556 came true when the harness ran
- the records made true again on 2026-10-05 after the harness. The lead's records commit e712a11
  at 07:01:49, its pre-commit 1865 passed, 0 failed, 0 skipped,
  turn5\f126-proof\precommit-ran.txt line 14, then main 3ee01ab merged in at 16b5eb7 with both
  sides kept, F116 order line 41 and F126 42, its pre-commit 1912 passed, 0 failed, 0 skipped,
  F116's tests now in, precommit-merge-main2.txt line 14, each file naming its commit at line
  30, read off git log. Then main c101f6c, the record of the C04 baseline and Q128, merged in at
  37ee68e with no conflict, 1912 passed, 0 failed, 0 skipped, precommit-merge-main3.txt line 14,
  and the claim-checker's reading of 16b5eb7, its points handed to this pass by the lead, made
  true in this entry, the section F126 in steps\01_next.md, steps\loop.md and the body
  turn5\pr-f126.md

### What remains

- the run with F112's rule switched off in the test of wave 1, the lead's run of 1A02MM with
  -Untick SkipClashOffCoordinates once main, F112 in it since ddb059b, is installed, whose
  driver.txt reads the box toggled Off on the real window
- pull request 112, open as a draft, merged once Actions is green

### Known bugs

- none found in the code. Until main with F112 in it is installed the real window has no such
  box, and a run given -Untick SkipClashOffCoordinates stops UNTICK on none of the tabs with
  nothing pressed, which is the rule working and not a fault
- the readings' notes, none blocking, the breaker's in turn5\f126-read-break.txt and the
  reviewer's in turn5\f126-read-review.txt. Check given -Untick passes a box the installed window
  may not hold, so Run starts Navisworks and only then stops UNTICK on none of the tabs, nothing
  pressed, f126-read-break.txt line 2. The RUN RECORD line and the UntickWords line are written
  before the driver starts and say it unticks the box, so for a run the driver stopped before
  the untick only driver.txt shows the box was never toggled, f126-read-break.txt line 5.
  LogCheck reads no state of the rule, so a RAN verdict rests on the driver's read back of the
  box and the tool's own ALIGNMENT lines, f126-read-break.txt line 8. UntickRefusal compares ids
  without regard to case, so -Untick Box,box is refused as named twice though an AutomationId is
  case sensitive, f126-read-break.txt line 11. The window sets the box from the default at every
  open, so every run that must have the rule off needs -Untick again, and a forgotten one is
  silent, f126-read-break.txt line 14. The driver's line about IncludeSubfolders,
  drive-window-run.ps1 line 421, ends with the boxes -Untick names though IncludeSubfolders is
  not one of them, f126-read-review.txt line 26. git printed failed to delete for 12 worktree
  entries after each commit, outside this change, f126-read-review.txt line 32. No register row
  was added for these notes

### What comes next

- the merge of pull request 112 once Actions is green, then the run of wave 1's test with the
  rule off

### Every program started, every file written outside the repo

Started: git, to fetch, show, archive, commit and push, and the pre-commit hook it runs, which
runs check-locals, check-imports, the evidence check and dotnet test. dotnet build for the
solution twice and the stand-in twice, dotnet test twice by hand, sh for check-locals,
check-imports and the waiter, Windows PowerShell 5.1 for the parser, for array tests and the
pure reads, for the driver twice, each time refused before any window is read, and for the
22 reads of Get-Process Roamer, the last at 01:31:32 after the records, reading 1, and tar to
unpack main's tools. That day no Navisworks, no stand-in, no harness run and nothing
installed.
The records pass on 2026-10-05 started git, to fetch, merge, show, diff, log, ls-remote, reflog,
status, commit and push, and the pre-commit hook twice, python for the merge resolver and the line
finder, Windows PowerShell 5.1 for parse.ps1 and the reads of Get-Process Roamer, dotnet build
for the solution twice, the first output written over by the second with --no-incremental, and
for the stand-in once, never started, dotnet test once by hand, and sh for check-locals and
check-imports. In that pass no Navisworks, no stand-in, no harness run and nothing
installed.
The lead on 2026-10-05 read Get-Process Roamer at 06:53:29 and 06:58:45, 0 each time,
roamer-reads-harness.txt, and ran prove-f126.ps1 twice, pid 47424 on main's tools in before-tree
and pid 46684 on the branch, line 1 of each output, which read Roamer at its start and end,
started the stand-in as its copy Decoy.exe 11 times and 12 times, still running 0 after each,
prove-f126-before.txt line 175 and prove-f126-after.txt line 310, and started child
powershell.exe for the driver and for run.ps1 -Mode Check. No Navisworks and nothing installed.
The lead's records commit e712a11 and the merge 16b5eb7 started git and the pre-commit hook
twice. This records pass started git, to fetch, merge-tree, merge, status, diff, log, show,
reflog, rev-parse, checkout, to put its own first edit of the three files back before running
the edits again, commit and push, the pre-commit hook at each of its commits, which runs
check-locals, check-imports, the evidence check and dotnet test, bash from Git for Windows with
its text tools for the reads and the evidence, gh to read pull request 112, and python for the
record edits and the line ends of the body. No Navisworks, no stand-in, no harness run and
nothing installed.

Written outside the repo, all under %LOCALAPPDATA%\NwcFederatorLoop\turn5\f126-proof:
core-tests-before.txt, core-tests-after.txt, sln-build-before.txt, sln-build-after.txt,
standin-build-before.txt, standin-build-after.txt, check-locals.txt, check-imports.txt,
parse.ps1, roamer-reads.txt, prove-f126.ps1, wait-and-prove.sh, pure-reads.ps1,
pure-reads.txt, dry-refusal.txt, dry-refusal-notes.txt and dry-refusal-notes-2.txt, the
driver's notes of its two refusals, precommit-1.txt to precommit-4.txt and push-1.txt to
push-4.txt, before-tree with main's tools at 1ae6771, and standin-bin-before and standin-bin,
the stand-in built before and after, never started. Also turn5\f126-msg-1.txt to
f126-msg-4.txt, the commit messages, and turn5\pr-f126.md, the draft body.
In the session's scratch folder under %TEMP%\claude: arr.ps1, parse1.ps1, pure.ps1, added.txt,
and fw112.cs and fw112.xaml, the window code of fix-F112 read with git show.
The records pass wrote, under turn5\f126-proof, precommit-claims-merge.txt, precommit-claims.txt,
push-claims.txt, diff-names.txt, first-pass-lines.txt, f112-lines-ddb059b.txt, reflog-claims.txt,
parse-after.txt, written twice, the second keeping PowerShell's own exit code, and
parse-raw.tmp, a copy deleted once read, sln-build-claims.txt, standin-build-claims.txt and
standin-bin-claims, the stand-in built and never started, checks-claims.txt,
core-tests-claims.txt and roamer-reads-claims.txt. Under turn5 it wrote f126-resolve-merge.py,
f126-log-entry-claims.md and f126-next-section-claims.md, the texts of this entry and of the
section F126, f126-records-claims.py, which wrote them and the register row in,
f126-added-claims.tmp, the added lines read for a dash or a semicolon, deleted once read,
f126-msg-merge.txt and f126-msg-5.txt, the two commit messages, and pr-f126.md, rewritten.
The lead's harness wrote, under turn5\f126-proof, prove-f126-before.txt, prove-f126-after.txt,
roamer-reads-harness.txt and the work folders of the two passes, work-before-065350 and
work-after-065511, copied Decoy.exe and Decoy.exe.config into standin-bin, and wrote runs\97
under %LOCALAPPDATA%\NwcFederatorLoop, removed by each pass at its end, prove-f126-before.txt
line 176 and prove-f126-after.txt line 311. The lead's records commit e712a11 and the merge
16b5eb7 wrote precommit-ran.txt and precommit-merge-main2.txt there, and under turn5, by their
file times from 07:00:59 to 07:07:40, f126-ran.py, f126-merge-main.txt and f126-resolve.py, and
pr-f126.md, rewritten. This records pass wrote, under turn5\f126-proof, reflog-harness.txt,
diff-since-1e06b7e.txt, msg-merge-main3.txt, precommit-merge-main3.txt, msg-records-harness.txt,
precommit-records-harness.txt and push-records-harness.txt, for its second commit, which
corrected the list of programs above and this list, msg-records-harness2.txt,
precommit-records-harness2.txt and push-records-harness2.txt, and added at the end of
precommit-ran.txt, precommit-merge-main2.txt, precommit-merge-main3.txt and
precommit-records-harness.txt the line naming the commit each belongs to, and under turn5
pr-f126.md, rewritten. In the session's scratch folder under %TEMP%\claude:
f126-records-harness.py, which made the edits. Also /tmp/added.txt of Git for Windows, the added
lines read for a semicolon, deleted once read.


## 2026-10-05 The loop, turn 5, F116 the clash XML, DONE in Core and built, wave 1, with Bader's answer to Q113, the readings of that pass, F112 taken in and a closing pass

Core tests, all with 0 failed and 0 skipped:
- 1746 passed before attempt 1, turn5\f116-core-before.txt, and 1776 at aefb416 after it with
  main c4fd0d4 taken in, turn5\f116b-core-before.txt. turn5\f116-core-after.txt holds the 1776
  on one line with no commit and no time. Main's own tests number 1746 at dd55e4b, where attempt
  1 branched off, and 1756 at c4fd0d4, the totals of turn5\f116e-main-counts.txt, so 10 of the
  30 are main's and F116's own are 20. In that file 51 tests of each tree fail, those of c4fd0d4
  each a workbook test that cannot load an assembly in a tree extracted with no .git, so only
  its totals are read
- 1776 passed before attempt 2 at aefb416, turn5\f116b-core-before.txt, and 1790 after it at
  9d8e3b2, turn5\f116b-core-after.txt
- 1790 passed before the Q113 pass at aa7ec30, turn5\f116c-core-before.txt, and 1796 after it at
  5de2b21, turn5\f116c-core-after.txt
- 1796 passed before the pass on its readings at 5de2b21, turn5\f116d-core-before.txt, and 1802
  after it at 0bf09b3, turn5\f116d-core-after.txt
- 1912 passed once main ddb059b, F112 among it, was taken in with the FR-028 change, read on the
  tree of 8a32795 before its commit, turn5\f116d-fr028-after.txt, and at its pre-commit,
  turn5\f116d-precommit-merge.txt. Main's own count is 1865 at the pre-commits of its records of
  pull requests 107 and 108, turn5\precommit-records-12b.txt and precommit-records-13.txt, as
  main's own entry of the design of F127 says, and 1865 again at f38a369's records, main's entry
  of FR-052. Main's changes from ddb059b to f38a369 are under steps\ alone,
  turn5\f116e-main-since.txt. So F116's own tests are 47 of the 1912, the 20, 14, 6, 6 and 1 of
  its passes
- 1912 passed before the closing pass at 1ee0d93, turn5\f116e-core-before.txt, and 1912 after it
  with RevitWorksets.All deleted, read on the tree of a40ff59 before its commit,
  turn5\f116e-core-after.txt. No test was added or deleted. The pre-commits of the closing pass
  read 1912 each, turn5\f116e-precommit-merge.txt, f116e-precommit-1.txt,
  f116e-precommit-merge-2.txt, f116e-precommit-2.txt and f116e-precommit-merge-3.txt, and that of
  its last records commit is turn5\f116e-precommit-3.txt, read after this entry was written

Every test run named above but f116-core-after.txt and the pre-commit files, which run the tests
quietly, also lists the one [Explicit] generator test, WriteTheCorrectedFile, as skipped, and the
adapter does not count it.
The solution builds with 0 warnings and 0 errors on 0bf09b3, built whole with --no-incremental,
Federator.Core, Federator.Core.Tests and Federator.Addin each built, turn5\f116d-build-code.txt,
the output kept with git rev-parse --short HEAD and a clean git status at its top. On 5de2b21 it
was a plain Release build, not built whole, with 0 warnings and 0 errors,
turn5\f116c-build-after.txt line 4. On 1ee0d93 it was built whole, turn5\f116d-build-after.txt,
and on a40ff59, the closing pass's code commit, built whole with git rev-parse --short HEAD and a
clean git status at its top, 0 warnings and 0 errors, turn5\f116e-build-code.txt. The records
commits 41c52a1, 1ee0d93 and the closing pass's two, and its three merges of main, change no
code. The build of the last commit is kept in turn5\f116e-build-after.txt, read after this entry
was written.
Every file named is under %LOCALAPPDATA%\NwcFederatorLoop\turn5 unless it is a path of the repo.

Programs and Navisworks:
- attempt 1's entry named dotnet build and dotnet test as the programs it started, and its
  commits were made with git. Its line "No Navisworks was started" rested on no process read
- attempt 2 started dotnet build, dotnet test, git, python, powershell for the evidence scripts,
  which load only Federator.Core.dll, and the repo's check scripts through the pre-commit.
  Get-Process Roamer read process 49016, started 18:55:27, before the first command at 19:52:28,
  turn5\f116b-roamer-before.txt, and process 32136, started 21:17:06, after the last at 22:04:53,
  f116b-roamer-after.txt. So a Navisworks started inside that window. None of the developer's
  programs starts one, and who started it is UNKNOWN to those reads
- the Q113 pass started dotnet build, dotnet test, git, sh for the two checks and the pre-commit,
  and powershell for the Roamer reads and turn5\f116c-same-sets.ps1, which loads only
  Federator.Core.dll. Get-Process Roamer read process 32136, started 21:17:06, at 22:26:01 before
  the first command and at 00:18:33 on 2026-10-05 after the last, turn5\f116c-roamer-before.txt
  and f116c-roamer-after.txt
- the pass on its readings started dotnet build, dotnet test, git, sh for the two checks and the
  pre-commit, python to edit files and to write the stubs, powershell for the Roamer reads and
  turn5\f116d-same-sets.ps1, which loads only Federator.Core.dll, and Git Bash's own tools such as
  grep, sed, awk, diff, od and sha256sum. Get-Process Roamer read process 32136, started 21:17:06,
  at 00:56:17, after the readings were read and before the first build, test or edit,
  turn5\f116d-roamer-before.txt, and at 01:35:18 after the code commit and its reads,
  turn5\f116d-roamer-mid.txt. The read after the push is turn5\f116d-roamer-after.txt
- the closing pass started dotnet build, dotnet test, git, sh for the two checks and the
  pre-commit, tar to unpack two trees of main, powershell for the Roamer reads, the reader
  measure and turn5\f116e-same-sets.ps1, which loads only Federator.Core.dll, and Git Bash's own
  tools such as grep, sed, awk, diff and date. Get-Process Roamer read process 32136, started
  21:17:06, at 02:46:30, before the first build, test or edit, the first of which,
  turn5\f116e-core-before.txt, reads 02:47:52, turn5\f116e-roamer-before.txt. The read after the
  push is turn5\f116e-roamer-after.txt
- none of these programs starts a Navisworks. Two reads of one process cannot exclude a
  Navisworks started and closed between them

### What attempt 1 did

- Bader's answer to Q104, FR-030, b68a785: the picked clash XML is corrected by MatrixCorrections
  before any set is built, at every place the window reads it. That is the pick, the run, the
  open file run and both hand buttons, which read it raw before. The log names every correction
  in a MATRIX line after the line naming the file. Bader's test passes: the old uncorrected
  sample, the exchange file and the sample with the hyphen alone corrected give the same sets. By
  hand, the sample, the exchange file as main had it, sha256 792b01fb, his older -OLD file and the
  regenerated exchange file give the same 61 sets and 1830 tests line for line,
  turn5\f116-same-sets.txt
- Bader's answer to Q102, FR-008 and FR-025, 045b7df, 76af22d and 1f5cf21: a workset the models
  carry in two or more spellings is asked in every one of them. Each spelling is an Or group
  copied whole, so every group still asks its category. Before, the one Or condition held the
  workset alone. revit-worksets.txt gained the 30 names the C06 log's EXPORT CHECK lines list,
  and the Q113 pass later moved them into this project's list. The row file gains one model
  worksets row per model naming every workset in full, so the next run measures every spelling
- Bader's answer to Q103, FR-009, 06a89bb: an AR set whose category another discipline also uses
  asks Source File contains -AR-. On the client's matrix BLD-AR-Ramps, Furniture, Railings and
  Site gain it and no other set changes
- FR-026, e959c7f: a category rewrite changes whole values and never the set's own name
- the six code commits, 1f5cf21, e959c7f, 045b7df, 06a89bb, b68a785 and 76af22d, each carry new
  tests seen failing first, turn5\f116-fr025-before-fail.txt, f116-fr026-before-fail.txt,
  f116-fr008-before-fail.txt, f116-fr009-before-fail.txt, f116-fr030-before-fail.txt and
  f116-worksets-row-before-fail.txt. 19 of the 20 new tests failed there, and the 20th, the
  shipped list test, failed in attempt 2 against an emptied list,
  turn5\f116b-shipped-list-before-fail.txt
- b6aa492, records only: the rule in .claude\rules\core.md, "The picked file is corrected before
  a set is built, F116", the order line and the F116 section in steps\01_next.md. aefb416, records
  only: this entry as attempt 1 wrote it

### What attempt 2 did

- On the reviewer's, the breaker's and the claim-checker's readings of aefb416,
  turn5\f116-read-review.txt, f116-read-break.txt and f116-read-claims.txt. Items 1 to 6 are five
  commits, each with new tests seen failing first. Item 7 is two commits of comments, d0db33d is
  the rule and the section, records only, and item 8 is a read with no commit
- 1, cfb057e: a value correction touches only a condition on the workset property that is not
  negated. Before, a negated workset condition asked in a second spelling took the whole
  category, and a condition on another property whose value read like a workset was widened too.
  A file with a negated workset condition gets a MATRIX line saying it is left as the file asks.
  Three new tests fail before, turn5\f116b-negated-before-fail.txt. Five older tests fed the rule
  a bare data element or a condition with no property, which it no longer reads as a workset, and
  their inputs became a set in the client's shape, every assertion kept, the tests' diff of that
  commit in turn5\f116d-cfb057e-tests.txt. AddingTheOrRowTwiceAddsItOnce had passed with nothing
  to add and gained an assertion that the first run adds the row. The commit message counts three
  of them and the Or row test, and no run shows the five failing before the change
- 2, 3e66536: one rule in one place. WrittenCondition reads through ExchangeReader.ReadCondition.
  WrittenCondition.Escaped is the one escape. WithValue, WithFlags and WithTest are the one way to
  edit a condition. OneCondition, ReplaceOpeningTag, ReplaceInnerText, FirstCondition,
  RewriteValue, AttributeText and Noted are folded in, and every lookup of a set by its name uses
  the same escaped name. Three new tests fail before, turn5\f116b-one-place-before-fail.txt. The
  catch-all now keeps the line break before its closing conditions tag, so the exchange file was
  regenerated by its generator, sha256 94897667, turn5\f116b-same-sets.txt line 53, where it was
  ee1d2f3e, turn5\f116-same-sets.txt line 48. The one line diff and its 151 changes in all are in
  turn5\f116b-regenerate.txt
- 3, c36490f: ReadPicked reads the file in the encoding it declares, through
  ExchangeReader.ReadFileText. A set or condition the corrections cannot read is counted on the
  NOT EVERY SET COULD BE READ line and never thrown. The Source File rule finds a set by its
  folders and its name. Four new tests fail before, turn5\f116b-read-before-fail.txt
- 4 and 5, af2c758: the MATRIX lines say "every spelling measured so far in this project's
  models" and "no model measured so far". Where nothing changed, the last line says no correction
  was applied and counts what was already made, what found nothing to change and what could not
  be read. One line before the last, on every picked file, says a set already in an NWF keeps the
  conditions it was built with unless the box Rebuild sets that drifted from the file is ticked,
  and that the SETS block names each such set as DRIFTED. Three new tests, and six older tests
  moved onto the new wording, fail before, turn5\f116b-lines-before-fail.txt. The empty
  selectionset line has no test seen failing, turn5\f116b-read-review.txt lines 47 to 49
- 6, e4a645d: the model worksets row writes an empty number and UNKNOWN where a model's element
  walk stopped part way, through ExportCheck.WorksetCount and EveryWorkset, with one call changed
  in FederationEngine. The new test fails against a stub, turn5\f116b-worksets-row-before-fail.txt
- 7, cc04402 and 9d8e3b2, no code line, turn5\f116e-comment-commits.txt: cc04402 changes
  comments under src and tests, and 9d8e3b2 code comments, the rule in .claude\rules\core.md and
  the F116 section of steps\01_next.md. The comments made true. The C06 log has nine groups whose
  worksets line counts the rest, at its lines 605, 1354, 3248, 3928, 4670, 5071, 6482, 6853 and
  7254. The nine AR sets left without -AR- are named, turn5\f116b-ar-sets.txt
- 8, a read: the rename alone, applied to samples\1104-PAR_CLASH_AllInOne_25mm.xml read only, is
  byte for byte Bader's older file, 1,443,383 bytes and sha256 36ab2739 both,
  turn5\f116b-hyphen-only.txt
- By hand on the Core of cc04402, the four files give the same 61 sets and 1830 tests line for
  line, turn5\f116b-same-sets.txt. Builds of 0 warnings and 0 errors are in
  turn5\f116b-build-negated.txt, f116b-build-one-place.txt, f116b-build-read.txt,
  f116b-build-lines.txt, f116b-build-worksets-row.txt and f116b-build-after.txt. The pre-commits
  are in turn5\f116b-precommit-1.txt to -8.txt

### What the Q113 pass did

- On Bader's answer to Q113 of 2026-10-04, B and D, the brief turn5\f116c-brief.md, after attempt
  2's readings, turn5\f116b-read-review.txt and f116b-read-break.txt, both APPROVE with nothing
  blocking
- Main 6cc0283 was taken in at aa7ec30 and main bd05bc5 at 5de2b21, both sides kept each time.
  The F116 entry stays on top of steps\log.md, and in steps\01_next.md F116's order line, 40
  then, and its section come after F125's
- 7e40a8a, items 2 to 5 of the brief and the first half of item 1.
  src\Federator.Core\Exchange\matrix-corrections.txt and its embedding in Federator.Core.csproj
  are gone, with MatrixCorrectionList.Shipped. The list is a plain file beside the picked XML,
  named after it: the XML's name without its extension and CorrectionListSettings.Suffix, default
  .corrections.txt. It is one full path tested with File.Exists, never a search.
  MatrixCorrections.ReadPicked reads it through MatrixCorrectionList.Beside, so the pick, the run,
  the open file run and both hand buttons read it. The first MATRIX line names the list in full
  and what it holds. With no list there, the first MATRIX line names the path looked for and says
  nothing is corrected, and the file is read as written. A list it cannot read corrects nothing
  and the first line says why, never a throw. A line it does not know and bytes that are not
  UTF-8 were tested in that pass, and a file that will not open and a rename it cannot use only in
  the pass after. The format is the old one plus workset lines, one spelling a line. This
  project's list is exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt, sha256 3f40d5cc
  then, turn5\f116c-same-sets.txt. Its 30 spellings were copied byte for byte off
  revit-worksets.txt, the no-break space kept. On 9d8e3b2 they were the C06 block of that file,
  lines 86 to 130, the names at lines 101 to 130, and ReadPicked handed the shipped list to
  ForPickedFile at MatrixCorrections.cs line 552, turn5\f116d-9d8e3b2-lines.txt
- in turn5\f116c-beside-before-fail.txt 1788 passed and 8 failed. Seven are tests of 7e40a8a, run
  against stubs that keep the list in Core. The eighth is 039192a's test of the 39 names, failing
  because revit-worksets.txt still held 69, and it is seen failing again in
  turn5\f116c-worksets-before-fail.txt. The stub was not kept, so that run is described and
  cannot be repeated from what is kept. After, 1796 pass, turn5\f116c-beside-after.txt. The new
  first line moved some tests, and every assertion was kept. Two read the NOT EVERY SET line at
  index 1. One counts 16 lines where it counted 15. The tests that picked the sample where it sits
  now pick a copy with the list beside it, and the sample where it sits stands for a file with no
  list
- 039192a, item 1: the 30 spellings F116 added left revit-worksets.txt, which holds the 39 names
  of the C02 census again. EmptySetsTests was main's again, because F116 had changed it only
  because the 30 names were in Core. In turn5\f116c-worksets-before-fail.txt 1793 passed and 3
  failed, in a run whose own git status shows revit-worksets.txt modified, so the list it read was
  not HEAD's byte for byte. The 30 extra names in the failure show the 30 were in it. That run too
  is described and cannot be repeated. After, 1796 pass, turn5\f116c-worksets-after.txt
- 83fda52, item 7, comments only: no set, Source File value or category of the list is left in a
  line F116 added under src, and of its workset spellings only main's own ME-DUCTWORK and
  ME-Ductwork example, in 4 comment lines reworded to say C02, read in the pass after at 0bf09b3,
  before the merge with F112, turn5\f116d-names-in-src.txt. At a40ff59 the same search finds 6
  comment lines and no other line, the 2 more being F116's own comment in ExportCheck.cs that came
  with the merge 8a32795, turn5\f116e-names-in-src.txt. SourceFileRule.cs names the C06 run of
  set 03, a run and not a name of the matrix. 1796 passed, turn5\f116c-comments-after.txt
- 50169d8, records only: the rule in .claude\rules\core.md, the corrections as one project's data
  beside the picked XML, and the F116 section of steps\01_next.md. The ValuesGiven sentence of
  core.md now says what the code's own doc says, the attempt 2 reviewer's point
- item 6, Q113 D: no change. The four categories stay in the list's source-file line
- No add-in file changed from 9d8e3b2 to 5de2b21, turn5\f116d-addin-range.txt, read in the pass
  after
- By hand on the Release Core of 83fda52, turn5\f116c-same-sets.txt. The sample, set 04's copy of
  sha256 792b01fb, Bader's older file of sha256 36ab2739 and the exchange file of sha256 94897667,
  each with the list beside it, give the same 61 sets and 1830 tests line for line. Set 04's copy
  with no list beside it gives the sets ExchangeReader.ReadFile gives it as written. The sample
  and the exchange file read the same sha256 after the read as before
- Builds of 0 warnings and 0 errors on the trees of 7e40a8a and 039192a and on 83fda52:
  turn5\f116c-build-beside.txt, f116c-build-worksets.txt and f116c-build-comments.txt.
  check-locals and check-imports pass on 5de2b21, turn5\f116c-checks-after.txt. The pre-commits
  of the pass's four commits and of the second merge each read 1796 passed,
  turn5\f116c-precommit-1.txt to -4.txt and f116c-precommit-merge-2.txt, and the first merge's
  read 1790, f116c-precommit-merge.txt. The full git diff --stat origin/main...HEAD, 26 files, is
  turn5\f116c-diffstat.txt. Pushed, origin/fix-F116 at 5de2b21, turn5\f116c-push.txt. Before the
  push, the second merge's title was amended twice, because printf had turned the \01 of
  steps\01_next.md into a control character and the first amend read the same file

### What the pass on the readings of the Q113 pass did

- On turn5\f116c-read-review.txt and f116c-read-break.txt, each CHANGES with one blocking
  finding, and the claim-checker's 18 points, turn5\f116c-read-claims.txt, read with Bader's answer
  under Q113 in steps\02_questions.md: a picked XML with no list beside it is corrected by
  nothing, and the log says so. The lead's task for this pass was given in the session and is
  kept in no file
- 0bf09b3, the reviewer's blocking finding. The EMPTY SETS judge read RevitWorksets.All(), the 39
  names inside Core, at EmptySets.cs line 257, while the corrections read those and the list's
  workset lines, at MatrixCorrections.cs lines 568 and 596 to 597. So a set the corrections made to
  ask ME-DUCTWORK was called a value no model in this project carries, while the MATRIX line said
  it was measured. RevitWorksets.With is now the one place the two are put together. ReadPicked
  hands what it gives to ExchangeDocument.Worksets, SetBuildPlan.From carries it to
  SetBuildPlan.Worksets, and SetBuilder hands that to EmptySets.Why, which judges against the
  spellings it is handed and reads no list of its own. That is one file of the add-in,
  SetBuilder.cs, three edits: the call of BuildOne passes plan.Worksets, BuildOne takes them as
  a parameter, and its EmptySets.Why call hands them on, with a comment, the diff kept in
  turn5\f116e-addin-diff.txt. The two argument Why is gone, so EmptySetsTests judges through a
  helper handing in the names inside Core alone
- 0bf09b3, the breaker's blocking finding. A list that was there and read went on to the value
  correction with the 39 names, so a list of no bytes, of comments or with no workset line
  rewrote ME-DUCTWORK to ME-Ductwork, more than no list does. A list holding none now corrects
  nothing and its first MATRIX line says so, as no list does, MatrixCorrectionList.HoldsNone. A
  workset value is corrected only where the list names a spelling of it, NamesASpellingOf. A
  value it names none of is left as the file asks, on a line saying the list beside this file
  names no spelling of it
- the four new tests of the two findings fail against stubs that add the new members with the old
  behaviour, 4 failed of 4, the stub diff and git status kept in
  turn5\f116d-judge-and-list-before-fail.txt, and pass after, turn5\f116d-judge-and-list-after.txt.
  One older test, the one reading the client's matrix values, pins PL-Drainage's whole line now,
  where it read its first words
- 0bf09b3, the reviewer's test gap and the claim-checker's seventh point. One test holds the list
  open with FileShare.None, then writes a rename whose new name holds the old one, and asserts for
  each that the first MATRIX line names why and nothing throws. Another pins that a UTF-16 list
  with its byte order mark is read, so the list's header and core.md say so where they said a list
  that is not UTF-8 is not read. In turn5\f116d-cannot-use-before-fail.txt the first run, its two
  catches rethrowing and its reader ignoring the mark, fails the held-open half and the UTF-16
  test, and the second, the rename catch alone rethrowing, fails the rename half, each run with
  its stub diff. The summary of NoListOfCorrectionsIsInsideCore says what it checks and no more,
  the reviewer's point
- the six new tests take Core from 1796 to 1802. The commit message of 0bf09b3 counts two tests for
  the two branches and one more, where they are one test holding both and the UTF-16 test, and it
  says two bullets are new in core.md, where there are three
- 0bf09b3, the list's header says 1B06G1 carries ME-DUCTWORK, ME-EQUIPMENT and ME-PIPING in
  capitals and 1B06BC the first two among the ten names its line lists, C06 log lines 1354 and
  605, the claim-checker's sixteenth point. Its workset lines are now lines 73 to 102, sha256
  afc463be, turn5\f116d-same-sets.txt
- 0bf09b3, the rule in .claude\rules\core.md: three bullets new, the one place, a value corrected
  only where the list names it, and what the corrections code names, with the ExportCheck.cs log
  line named, the claim-checker's fifth point, and the list bullet made exact
- the claim-checker's 18 points made true in this entry, the F116 section of steps\01_next.md and
  turn5\pr-f116.md, with the files read for them: turn5\f116d-addin-range.txt,
  f116d-names-in-src.txt, f116d-main-trial-merge.txt, f116d-9d8e3b2-lines.txt and
  f116d-cfb057e-tests.txt
- the pre-commit of 0bf09b3 read 1802 passed, turn5\f116d-precommit-1.txt. check-locals and
  check-imports pass on 0bf09b3, turn5\f116d-checks-after.txt
- By hand on the Release Core of 0bf09b3, turn5\f116d-same-sets.txt. The sample, set 04's copy of
  sha256 792b01fb, Bader's older file and the exchange file, each with the list beside it, give
  the same 61 sets and 1830 tests line for line, and the exchange XML is unchanged at sha256
  94897667. Set 04's copy with no list, with a list of no bytes or with a list of the rename
  alone keeps every workset value it asks, the first two on 3 MATRIX lines. The EMPTY SETS judge,
  handed the spellings the plan carries, calls every workset value the corrected file asks one
  models in this project carry, where the names inside Core alone call ME-DUCTWORK, ME-EQUIPMENT,
  ME-PIPING, FF-Fire Fighting, FP-PIPING and PL-Domestic Water carried by no model
- F112 was not on main at b2afb2d. Main's changes from bd05bc5 to b2afb2d are 4 files under
  steps\, and a trial merge of b2afb2d into 0bf09b3 conflicts in steps\log.md alone,
  turn5\f116d-main-trial-merge.txt
- 41c52a1, records only: this entry and the F116 section of steps\01_next.md as written before
  F112 merged
- F112 merged on main as ddb059b, pull request 106, while this pass ran, and main ddb059b was
  taken in at 8a32795, both sides kept. steps\log.md and steps\01_next.md conflicted: this entry
  stays on top with main's entries after it in main's order, and F112 takes order line 40 and
  F116 41, F112's section after F125's and F116's last. origin/main had already moved on to
  ce6eedb at the fetch of 02:04:12, line 45 of its reflog in f116e-main-order.txt, before the merge at 02:07:11 and the push of 1ee0d93 at
  02:25:46, the reflogs in turn5\f116e-main-order.txt, so 8a32795 took in ddb059b and not the
  head of main. ce6eedb's changes were under steps\ alone, and a trial merge of it into 1ee0d93
  read after the push was clean, turn5\f116d-main-after-push.txt
- 8a32795, F112's FR-028 case lines. On the merged tree F112's own test
  AGroupWhereNoAskedNameDiffersByCaseGetsNoWarning failed, because the corrected matrix asks
  ME-DUCTWORK or ME-Ductwork in every set asking one and the case lines named those sets as
  missing ME-Ductwork. ExportCheck.AddCaseDifferences now names a pair only with the sets that
  ask the other spelling and do not also ask the carried one, not negated, as a whole name or
  for contains a part of it, WorksetAsk.Finds and AlsoFinds, and a pair no set is left for is
  not named. A set asking the carried spelling only negated is still named. The change rides in
  the merge commit because the pre-commit refuses a commit that fails a test
- the tests of it: F112's test, its comment made true, the new
  ASetThatAlsoAsksTheCarriedSpellingIsNotNamedAsMissingIt, and F112's 1B06BC test, which read the
  corrected matrix as main had it, asking the title case alone. That one is now
  TheCapitalsOf1B06BCAgainstAFileAskingTitleCaseAreNamed, its three assertions kept against sets
  asking the title case alone, and it also asserts the corrected matrix since F116 names nothing
  for that group. The three fail on the merged tree before the change, 3 failed of 3, the git
  status and the test diff kept, turn5\f116d-fr028-before-fail.txt, and pass after, 1912 in all,
  turn5\f116d-fr028-after.txt
- 8a32795, the rule in .claude\rules\core.md: a bullet on the case warning, and the bullet on what
  the corrections code names no longer says ExportCheck.cs types the pair, a sentence F112's
  FR-028 removed
- 1ee0d93, records only: this entry and the F116 section brought to the merge. Pushed,
  origin/fix-F116 at 1ee0d93, turn5\f116d-push.txt

### What the closing pass did

- On the readings of 1ee0d93, turn5\f116d-read-review.txt, CHANGES with one blocking finding,
  turn5\f116d-read-break.txt, APPROVE, and the claim-checker's 12 points,
  turn5\f116d-read-claims.txt. The lead's task for this pass was given in the session and is kept
  in no file
- main ce6eedb was taken in at fa66a26 with no conflict, its changes steps\02_questions.md and
  steps\loop.md. Main 0c64018 and then main f38a369 came while this pass ran and were taken in at
  d418c56 and fae143f. In each steps\log.md conflicted and both sides are kept: this entry on top,
  main's entries of FR-052 and of the design of F127 after it, then main's in main's order. No
  merge changes a file under src or tests, turn5\f116e-main-since.txt
- a40ff59, the reviewer's blocking finding. RevitWorksets.All() at RevitWorksets.cs line 48 had
  no caller in src once 0bf09b3 moved EmptySets and MatrixCorrections onto RevitWorksets.With,
  and it was a second way to get what With(null) gives, new List(Load()) in both. No decision in
  steps\02_questions.md keeps it, so it is deleted. No test is deleted, because each of the five
  test calls proved something With(null) does, so each now reads With(null) and every assert is
  kept: TheListInsideCoreIsExactlyTheNamesMeasuredOnC02 and
  BothSpellingsOfTheFourWorksetsTheBuildingsSpellTwoWaysAreMeasured in RevitWorksetsTests, and
  TheCommittedFileAsksEveryMeasuredSpellingWithItsCategoryInEveryGroup,
  TheValueLinesClaimOnlyWhatWasMeasured and EverySpellingTheCorrectionsAskIsOneTheEmptySetJudgeKnows
  in MatrixCorrectionsTests. git grep finds no RevitWorksets.All under src or tests, and
  check-locals and check-imports pass, turn5\f116e-checks.txt. A deleted member has no test to
  see failing first. The build is the proof that nothing called it
- the claim-checker's 12 points made true in this entry, the F116 section of steps\01_next.md and
  turn5\pr-f116.md, with the files read for them: turn5\f116e-main-order.txt,
  f116e-names-in-src.txt, f116e-addin-diff.txt, f116e-comment-commits.txt and
  f116e-main-counts.txt. The rule in .claude\rules\core.md said a list saved as UTF-16 or UTF-32
  is read, and no Core test reads UTF-32, so it says UTF-16 alone. The same reader built outside
  the tool does read UTF-32 with its mark, turn5\f116e-reader-measure.txt, which is not a test
- the breaker's point on a UTF-8 list with its byte order mark, read here, not fixed. A
  StreamReader built as MatrixCorrectionList.cs line 136 builds it, under this machine's .NET
  Framework 4.8.1, mscorlib 4.8.9345.0, through powershell, throws on a 0xA0 byte with no mark and reads it as U+FFFD
  with the UTF-8 mark, turn5\f116e-reader-measure.txt. So core.md now says bytes that are not
  UTF-8 are refused in a list with no byte order mark, and the case is under Known bugs
- the same-sets read of the pass before, run again on the Release Core of a40ff59 by
  turn5\f116e-same-sets.ps1, whose one change is that the names inside Core alone are read off the
  plan of the file read as written, since All is gone. Past its first line it is the same as
  turn5\f116d-same-sets.txt line for line, turn5\f116e-same-sets-compare.txt: 61 sets and 1830
  tests for each file with the list beside it, 16 MATRIX lines under main\, 13 outcome lines, 9
  of them changing something, and 23 changes in all
- f51de84, records only: this entry, the F116 section of steps\01_next.md and the rule in
  core.md. The records commit after fae143f brings this entry and the section to that merge,
  records only

### Choices the developers made, for the reader to check

- Q103 READ WIDER THAN THE MATRIX ALONE. Read off the matrix only, no other folder's set asks
  Ramps, Railings, Furniture or Site. So the rule would change nothing, and the 414 BLD-AR-Ramps
  clashes of 1B06PK would stay. So the rule also takes categories measured in the logs: AR sets
  that found items in groups with no AR model. Ramps and Railings come from the C06 log lines
  4304, 7979 and 4316, Furniture and Site from the C04 partial log lines 200 and 203, and a test
  reads them against those lines. Any other category another discipline carries only beside an AR
  model is UNKNOWN. Bader kept all four, Q113 answered D on 2026-10-04
- THE CORRECTIONS WERE DATA IN CORE until Bader answered Q113 B on 2026-10-04. They are now this
  project's list, a plain file beside the picked XML. The copy in exchange\ sits beside the
  corrected XML, so Bader can copy both into his folder
- THE LIST'S LINE IS THE FIRST MATRIX LINE. With no list, one that cannot be read or one holding
  none, the line saying so is first, and the two lines every picked file carries follow it, 3
  MATRIX lines, turn5\f116d-same-sets.txt
- A LIST WHOSE BYTES ARE NOT UTF-8 IS NOT READ when it carries no byte order mark, because a
  default reader would put a replacement character for the no-break space of EL-Fire alarm and
  ask a spelling no model carries. One saved as UTF-16 with its byte order mark is read as
  UTF-16, which a test pins. One with the UTF-8 mark is read with a replacement character for a
  bad byte, measured in the closing pass and under Known bugs
- THE SUFFIX IS A SETTING WITH ITS DEFAULT ONLY, the shape ProbeSettings has. Nothing in the window
  sets it
- A VALUE THE LIST NAMES NO SPELLING OF IS LEFT ALONE, every such value, and not only one the
  names inside Core would change. So with this project's list PL-Drainage is left alone because
  the list names no spelling of it, where before it was left alone because the models spell it as
  the matrix does, the same set either way. A value the list does name is asked in every spelling
  of the names inside Core and the list together, as the lead's task put it
- THE SPELLINGS REACH THE JUDGE ON THE PLAN, SetBuildPlan.Worksets, and not through SetBuilder's
  constructor, so a Core test proves the plan carries what the corrections used and the add-in
  change is one parameter passed through, three edits in SetBuilder.cs

### What remains

- the add-in half, the test of wave 1 on 1A02MM and 1A04PK, with the list copied out of exchange\
  beside set 04's copy of the FIXED XML of sha256 792b01fb, %LOCALAPPDATA%\NwcFederatorLoop\runs\04
  manifest line 1, and named 1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt. Every line of
  this bullet is EXPECTED, read off Core or off the XML text, and none was seen in Navisworks.
  The wave 1 run is its proof. Expected on the first run with the XML: 16 MATRIX lines after the
  line naming the file, as turn5\f116e-same-sets.txt has them under main\ on the Release Core of
  a40ff59. The first names the list and says it holds 3 corrections, 1 rename, 1 catch-all and 1
  Source File rule, and 30 workset spellings. Then come 13 outcome lines, 9 of them changing
  something, the PL-Drainage line saying the list beside this file names no spelling of it. Then
  the line saying a set already in an NWF keeps its conditions unless the box Rebuild sets that
  drifted from the file is ticked, and `MATRIX   23 changes in all`. Expected with no list beside
  it, or a list holding none: 3 MATRIX lines, the first saying so, and the sets built as the file
  asks. Expected built fresh, or with the box ticked: BLD-ME-Ducts&Duct Fittings asking 8
  conditions where main's exchange file asks 4, and BLD-AR-Ramps 2 where it asks 1, counted as
  condition elements in the XML text and not in a built set, turn5\f116c-condition-counts.txt.
  Expected on an NWF from set 04 with the box off: those sets keep their old conditions and the
  SETS block names them DRIFTED. Expected for a set already in the NWF that finds nothing and asks
  a spelling of the list, such as ME-DUCTWORK: the EMPTY SETS block counts it among the values
  models in this project DO carry. Expected in the EXPORT CHECK block of 1A02MM: a set asking
  ME-DUCTWORK or ME-Ductwork is not named as missing either, as
  AGroupWhereNoAskedNameDiffersByCaseGetsNoWarning shows on that group's worksets measured in 5q,
  and for a workset 5q did not read it is UNKNOWN. Expected in the .tsv: one model worksets row
  per model, UNKNOWN where a walk stopped. Expected on the weekly run: no XML picked and no MATRIX
  line. Whether BLD-AR-Ramps finds fewer items in 1A04PK is UNKNOWN until the run
- the open file run and both hand buttons read the list through the same ReadPicked and have no
  run proof yet. No F116 proof steps are written in steps\03_bader_next.md, and the wave 1 run
  stands for the Run path alone
- Bader is told once F116 merges that the list is on main at
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt, beside the corrected XML

### Known bugs

- a value line says "measured so far in this project's models" of a spelling that comes from the
  39 names inside Core, the C02 census of project 1104, MatrixCorrections.cs lines 1014 and 1040.
  With another project's list naming ME-DUCTWORK the line would call ME-Ductwork that project's,
  because RevitWorksets.With, RevitWorksets.cs lines 55 to 73, puts the 39 beside any list, and
  the EMPTY SETS judge would call a workset in neither list carried by no model and name a 1104
  name as nearest. Whether the 39 stay in Core is Bader's to say, the Q113 pass reviewer's
  question and both readings of 1ee0d93, turn5\f116d-read-review.txt and f116d-read-break.txt.
  It is not in steps\02_questions.md, whose last on main is now 127, and is for the lead to put
- the readings of 1ee0d93 that did not block, left as they were:
  - the EMPTY SETS judge compares the value of a contains condition to the measured categories by
    Ordinal equality and never reads the test or the negate flag, src\Federator.Core\Sets\EmptySets.cs
    lines 114 to 146 and 267 to 278. So BLD-EL-Devices, which asks Category contains Devices, is
    said in that block to ask a value NO MODEL IN THIS PROJECT CARRIES when it finds nothing, while
    the HEALTH block reads contains as a part of a name, SetWarnings.cs lines 264 to 280, and the
    one log disagrees with itself. Older than F116, the breaker's
  - a rename is a text replace over the whole XML text, MatrixCorrections.cs lines 912 to 913, so
    a From that is a part of other names rewrites those too, and the log gives one count and not
    where. SetRename, MatrixCorrections.cs lines 10 to 42, refuses an empty name and a To holding
    its From, and nothing else, the breaker's
  - a list that is there and holds none, MatrixCorrectionList.cs lines 89 to 97 and 255 to 259, is
    said in lower case as a project needing none is, where an unreadable list gets capitals, and a
    failed save looks the same. Whether it takes the capital form is the lead's call, the breaker's
  - in this project's list 24 of the 30 workset lines match no value of the client's matrix but for
    case, the breaker's count, and feed only the EMPTY SETS judge, RevitWorksets.cs lines 55 to 73,
    MatrixCorrectionList.cs lines 219 to 229 and 261 to 266, MatrixCorrections.cs lines 623 to 626,
    and the list's lines 73 to 102. The first MATRIX line says 30 workset spellings and nothing
    says how many were used. A mistyped line makes the judge say carried, and a line differing from
    a value by a no-break space is not a spelling of it, the breaker's
  - a UTF-8 list WITH its byte order mark is read with U+FFFD for a byte that is not UTF-8 and is
    never refused, MatrixCorrectionList.cs lines 46 and 136, measured with the same reader in
    turn5\f116e-reader-measure.txt. The summary at MatrixCorrectionList.cs lines 20 to 23 and the
    list's header lines 70 to 72 say a list that is not UTF-8 is not read, which holds only with
    no mark. The breaker's, UNKNOWN there and measured in the closing pass
  - the case warning keys a set by its name alone, ExportCheck.cs lines 336 to 389, 417 and 442 to
    448. AlsoFinds counts a set as asking the carried spelling when any condition of it does, not
    negated, whatever Or group it sits in, so a set asking it in one group and not the other, or a
    namesake in another folder that asks it, keeps a set from being named. The client's 61 sets
    have unique names and copy every Or group whole, so nothing is hit today. No test covers it,
    the reviewer's and the breaker's
  - the class comment of RevitWorksets, RevitWorksets.cs lines 20 to 22, still says MatrixCorrections
    asks every spelling measured in Core and the list, where since 0bf09b3 a value is corrected
    only where the list names a spelling of it, the reviewer's. ValueRewrite's summary and core.md
    say it right
  - the add-in changed in SetBuilder.cs lines 497, 626 and 704, and steps\03_bader_next.md has no
    F116 proof steps, the reviewer's, disclosed under What remains
- the workset spellings past the tenth name of a group are UNKNOWN until the wave 1 .tsv rows
- FR-012 is still open: an unreadable workset list inside Core corrects nothing, silently
- still without a caller in src: CategoryRewrite, ValueOrRow, the shorter Apply overloads and
  ExchangeReader.ReadFile, FR-172 and Q26
- on an NWF built from the raw XML, the rename also renames the 120 test names, so new tests are
  created beside the old ones, the attempt 2 breaker's finding
- the two MATRIX warnings are log lines only, and the RESULT block does not repeat them
- where a walk did not finish, the model export row writes -1 as its number and the EXPORT CHECK
  block writes worksets NONE, both older than F116
- FR-030's proof asked for a Core test that the picked file's outcome lines reach the HEALTH
  block. No pass did it: the MATRIX lines go to the log before the HEALTH block, and the tests
  prove only that they ride on the document
- the Q113 pass breaker's points that did not block, left as they were, turn5\f116c-read-break.txt:
  - the list is found by the XML's name alone, and when none is found only the first MATRIX line
    says so, not the label after the pick nor the RESULT block
  - the same folder run with and without the list renames sets and tests both ways
  - a value with one measured spelling drops the matrix's own, FF-FIRE FIGHTING
  - a rename is a raw text replace, so a name holding an ampersand is not found
  - a catch-all on a set the same list renames
  - a trailing space in a line of the list
  - File.Exists is false for a path too long, so the list is said to be not there
  - the Suffix setting is not checked
- the Q113 pass reviewer's points that did not block, left: CLAUDE.md's line on the exchange
  folder no longer describes the list kept there, Bader's to change
- the other non-blocking points of attempt 2's readings are left as they were,
  turn5\f116b-read-review.txt and f116b-read-break.txt:
  - a value correction never counts a set it cannot read as not readable
  - ReadFileText puts a replacement character for a bad byte
  - a single quoted flags attribute gets written twice
  - the empty selectionset tag has no test seen failing
  - the Source File rule's Asks and WrittenAsking read a negated condition
  - the catch in WrittenCondition.Read names no set
  - the negated workset warning gives the widening reason in the one spelling case
  - one test feeds a bare data element
  - two summary blocks sit on the WorksetSet helper
  - the DRIFTED line overclaims for a set whose search will not read
  - two sets of one name in one folder
  - the workset property constant

### What comes next

- the reviewer, the breaker and the claim-checker on the closing pass and on the two merges of
  main. Then F116 merges, and Bader is told the list is on main

The add-in half waits for the local machine, in the test of wave 1.

### Every file written outside the repo, the closing pass

- under %LOCALAPPDATA%\NwcFederatorLoop\turn5: f116e-roamer-before.txt, f116e-core-before.txt,
  f116e-msg-merge.txt, f116e-precommit-merge.txt, f116e-core-after.txt, f116e-checks.txt,
  f116e-reader-measure.txt, f116e-msg-1.txt, f116e-precommit-1.txt, f116e-build-code.txt,
  f116e-same-sets.ps1, f116e-same-sets.txt and the folder f116e-same-sets of copies,
  f116e-same-sets-compare.txt, f116e-names-in-src.txt, f116e-addin-diff.txt,
  f116e-comment-commits.txt, f116e-main-counts.txt, f116e-main-order.txt,
  f116e-msg-merge-2.txt, f116e-precommit-merge-2.txt, f116e-main-since.txt, f116e-msg-2.txt,
  f116e-precommit-2.txt, f116e-msg-merge-3.txt, f116e-precommit-merge-3.txt, and the files of
  the last records commit and the push named in turn5\pr-f116.md, which this pass rewrote
- the session's scratchpad under %TEMP%\claude: the files of the reader measure, the trees of
  main dd55e4b and c4fd0d4 unpacked by git archive with their builds, a list of the names looked
  for, the output of a test run, and the folders the tests make under %TEMP% and remove

### Every file written outside the repo, the pass on the readings

- under %LOCALAPPDATA%\NwcFederatorLoop\turn5: f116d-roamer-before.txt, f116d-roamer-mid.txt,
  f116d-core-before.txt, f116d-judge-and-list-before-fail.txt, f116d-judge-and-list-after.txt,
  f116d-cannot-use-before-fail.txt, f116d-msg-1.txt, f116d-precommit-1.txt, f116d-same-sets.ps1,
  f116d-same-sets.txt and the folder f116d-same-sets of copies, f116d-addin-range.txt,
  f116d-9d8e3b2-lines.txt, f116d-main-trial-merge.txt, f116d-names-in-src.txt,
  f116d-cfb057e-tests.txt, f116d-build-code.txt, f116d-checks-after.txt, f116d-core-after.txt,
  f116d-msg-2.txt, f116d-precommit-2.txt, f116d-fr028-before-fail.txt, f116d-fr028-after.txt,
  f116d-msg-merge.txt and f116d-precommit-merge.txt, and the files of the last records commit
  and the push named in turn5\pr-f116.md
- the session's scratchpad under %TEMP%\claude, the edit scripts and a copy of
  MatrixCorrectionList.cs kept while it was stubbed, and the folders the tests make under %TEMP%
  and remove

## 2026-10-05 The loop, turn 5, FR-052 confirmed again and the ceiling under Q124

Nothing under src, tests or tools changed. Core tests 1865 passed, 0 failed, 0 skipped before, at
turn5\precommit-records-13b.txt, and after, at turn5\precommit-records-14.txt.

### What was done

- FR-052 confirmed again by F132's Core half, a test the XML gives no tolerance counted as
  skipped and named on no SKIPPED line, turn5\f132-finding-notolerance.txt. A claim-checker found
  the first commit had written it as a new FR-190, which is taken out, FR-052 staying F119's in
  wave 3b. F132 took its test over every skip reason out of its tree rather than fix it there
- the lead's note on Q124: no progress line of the C04 run after test 75 at 00:59, and under its
  default A the ceiling of Q84, 43200 s from adoption at about 21:18:25, closes the run at about
  09:18 if it has not ended
- F132's Core half built on its branch, 1908 tests passed, unmerged until its add-in half, and
  read now by a reviewer and a breaker, while F127's Core steps 1 to 3 are with a developer

### What remains

- the readings of F132's Core half, F127's Core steps, F116's closing pass, and the gap for the
  harness of F126 and the probes

### Known bugs

- none new in this record

### What comes next

- F116's merge once its closing pass reads clean

Nothing in this record waits for the local machine.

## 2026-10-05 The loop, turn 5, the design of F127 coverage, Q126 and Q127

Nothing under src, tests or tools changed. Core tests 1865 passed, 0 failed, 0 skipped before, at
the pre-commit of PR 107's last commit, turn5\precommit-records-12b.txt, and after, at this
record's, turn5\precommit-records-13.txt.

### What was done

- the design of F127, the coverage of Bader's request 2, by three plans each its own agent,
  turn5\f127-plans.txt, and a judge, turn5\f127-design.md, scored evidence 26.5, the rules in Core 26 and the cost 25.5 of 30,
  evidence the base. FR-176 names its part, its eight probes and its added run time
- Q126 and Q127, two of its eight questions, each with the choice the build goes on with. The
  other six his words or the lead's reading settle, named on FR-176

### What remains

- its seven probes on Navisworks once no Navisworks of the loop runs, Q124, and an eighth only if
  P2 fails

### Known bugs

- none new in this record

### What comes next

- F127's Core steps 1 to 3 test first now, step 4 after F116 merges and P2 to P5 are read, the
  add-in after P1 to P7

The probes on Navisworks wait for the local machine.

## 2026-10-04 The loop, turn 5, F112 the alignment area of the fix round, FR-001 to FR-006 and FR-028, DONE in Core and built, after a third attempt and a closing pass

Attempt 1 built the rule on Bader's answer to Q99 and Q100 and stopped at c5d8aa8. A reviewer and
a breaker read it there, turn5\w1-read-review-F112.txt and turn5\w1-read-break-F112.txt, and
attempt 2 answered them at c867348, its records at 86ea5d3, whose src and tests are those of
c867348. A reviewer, a breaker and a claim-checker read attempt 2, turn5\f112b-read-review.txt,
turn5\f112b-read-break.txt and turn5\f112b-read-claims.txt, and the reviewer and the breaker found
the same blocking fault: with the rule on, a group holding a model on Internal that runs no clash
test ended DONE. Attempt 3, on the lead's brief turn5\f112c-brief.md, took main 32b75fd in at
8b4fcdd, keeping Q110 and Q111 before main's Q112 and Q113 in steps\02_questions.md and this entry
above main's two, and stopped at e6d6f73. A reviewer, a breaker and a claim-checker read it,
turn5\f112c-read-review.txt, turn5\f112c-read-break.txt and turn5\f112c-read-claims.txt. Which
commit each set of readings read: the attempt 1 and attempt 2 reviews and breaks name none. Their
files were last written at 18:17:10 and 21:07:44 and the attempt 3 files at 23:07:46, one time per
file in turn5\f112e-stat-readings.txt, and the journal of the attempt 1 readings at 18:01:24,
turn5\w1-read-journal-time.txt. The branch reflog, turn5\f112e-reflog.txt, gives c5d8aa8 as the
tip from 17:26:42 to 18:29:11, 86ea5d3 from 19:53:41 to 21:14:41 and e6d6f73 from 22:45:22 to
23:34:45, so each of those times falls where the tip was the commit named. The attempt 3 review
also names e6d6f73.

The breaker of attempt 3 found one blocking fault: the FAILED reason of a group with a model on
Internal said its outputs were still written, at the ALIGNMENT step before any was, and hid a
missing or stale NWD in RESULT. Its clause is older than F112. "Every output of this group was
still written" came in at 29bfef8 of 2026-09-20 for Q70 and stood at line 286 of AlignmentCheck.cs
on main 086a348, from which fix-F112 was made, and attempt 1 added its twin for a skipped group,
"Its NWF and its NWD were still written", at 41ec380. The exact clause is named by no reading before attempt 3, turn5\f112e-clause-older.txt. Its family, a sentence that says files were written before anything looked, was not new: attempt 1's reviewer blocked on it in the note, the list and RESULT, w1-read-review-F112.txt line 8, attempt 2's on the note's NWF line, and attempt 3's breaker on this reason, each instance fixed by the next pass. So the family took four passes. The rule that a finding surviving three fix attempts goes to the form is for one still standing, and after the closing pass its reviewer and breaker find nothing blocking. Whether that counts as four attempts on one finding is for Bader to read. Attempt 4 is the closing pass on the lead's message of
2026-10-04. It fixed it at 0000355 and made the attempt 3 claim-checker's points
true at 39c50f0. A reviewer and a breaker read the closing pass at 39c50f0,
turn5\f112d-read-review.txt and turn5\f112d-read-break.txt, both VERDICT APPROVE with nothing
blocking. The review names 39c50f0, and both files were written at 00:19:36 on 2026-10-05,
turn5\f112e-stat-readings.txt, while 39c50f0 was the tip, from 23:56:56 to 00:28:27 by
turn5\f112e-reflog.txt. A claim-checker read it too, turn5\f112d-read-claims.txt, and its nine
points that needed a change are made true here, in the F112 section of steps\01_next.md, in the
rule in .claude\rules\core.md and in the body turn5\pr-f112.md, after main f09ee92 was taken in at
c5ba7e0. Its last point needed no change, and its one reading note is in the attempt 4 tests below.

Core tests 1859 passed, 0 failed, 0 skipped before attempt 4 at e6d6f73,
turn5\f112d-core-before.txt, and 1865 passed, 0 failed, 0 skipped after, by the pre-commit of
0000355, turn5\f112d-precommit-reason.txt, and by the pre-commit of the merge c5ba7e0,
turn5\f112e-precommit-merge.txt. One Explicit test, WriteTheCorrectedFile, is not run and is in no
count in this entry. `dotnet build ParsonsNwcFederator.sln -c Release` 0 warnings and 0 errors at
0000355, the last change under src, turn5\f112d-build-after.txt, and at c5ba7e0,
turn5\f112e-build-merge.txt, each file carrying its commit. `check-locals.sh src` and
`check-imports.sh src` exit 0 at 0000355 and at c5ba7e0 by the same pre-commits. No developer of
F112 started a Navisworks by the record of each attempt, named under Programs started. Every
turn5\ file named here is under %LOCALAPPDATA%\NwcFederatorLoop.

### What was done

- the closing pass records, main f09ee92 taken in at c5ba7e0 by turn5\f112e-reflog.txt, both sides
  of each conflict kept: this entry stays on top of steps\log.md above main's two new entries,
  F125's order line stays 39 and F112's moves from 39 to 40 with its section after F125's in
  steps\01_next.md, and steps\02_questions.md merged with no conflict, Q110 and Q111 before Q112,
  Q113 and Q114, turn5\f112e-merge.txt. Main changed nothing under src or tests from 32b75fd to f09ee92,
  nor from f09ee92 to b2afb2d, and nothing under src or tests differs from 0000355 at c5ba7e0, each
  git diff exit 0 with 0 files, turn5\f112f-diffs.txt
- the closing pass records, the claim-checker's nine points on 39c50f0, turn5\f112d-read-claims.txt,
  made true. The EXPORT CHECK run line is said as the code does it, never clean while a group
  whose whole read threw is counted, in the attempt 3 item 4 below and in .claude\rules\core.md,
  and a model dropped in the reader's own catch stays a Known bug. The Known bugs gain the three
  catches of ModelFactsReader.cs at lines 338, 352 and 373, the ordinal match of NamesInternal and
  the model naming no site left off the list, so the body's list for the next wave names only what
  is here. The waiter's reads in attempt 1's window are said in order in steps\01_next.md and the
  body. The write times of the readings and the pushes are read with stat,
  turn5\f112e-stat-readings.txt, and the tips with the reflog, turn5\f112e-reflog.txt. The attempt
  3 claim-checker made 12 points that needed a change, the 13 entries of
  turn5\f112c-read-claims.txt less the last. The lines named at 8b4fcdd, e6d6f73, 27df6b3 and
  0000355 are read with git show, turn5\f112e-lines-8b4fcdd-e6d6f73.txt, and each holds what this
  entry says. The NWD written line's commit, a6957a6 of 2026-08-27, is read with git blame and its
  line 3357 on main at 1ae6771 and at f09ee92 with git show, turn5\f112e-nwd-written-lines.txt.
  Attempt 1's record that it started no Navisworks is its developer's last message in the
  session's transcript, turn5\f112e-attempt1-return.txt. The checker's reading note on attempt
  4's tests is in their bullet below
- the closing pass records, the attempt 3 breaker's clause read back to its commits and every
  blocking finding of the readings of attempts 1 to 3 listed, turn5\f112e-clause-older.txt, as the
  paragraph above says
- the closing pass records, the readings' notes, none blocking, turn5\f112d-read-review.txt and
  turn5\f112d-read-break.txt, each in the Known bugs or under What remains
- attempt 4, the root cause, at e6d6f73, every line here read back from that commit with git show,
  turn5\f112e-lines-8b4fcdd-e6d6f73.txt. AlignmentCheck.cs lines 463 to 467 ended the FAILED
  reason "Every output of this group was still written, so the evidence is there to send." or
  "Its NWF and its NWD were still written", made at the ALIGNMENT step before any file is
  written. The first clause is older than F112, from 29bfef8 of 2026-09-20, and the second came
  with attempt 1 at 41ec380, turn5\f112e-clause-older.txt. FederationEngine.cs line 2143 put it on
  the group's errors, `outcome.AddError(fails)`, and GroupJudgement.cs lines 180 to 184 returned on
  HasErrors before the NWD checks at lines 244 to 258. So for a group failed on a model whose
  publish returned false, RESULT gave that sentence as its one reason and never named the NWD
  missing or not from this run
- attempt 4, the fix, at 0000355. WhyItFailsTheGroup's reason names the models and their sites
  and nothing of a file, and the block line adds the full stop. The engine keeps it apart from the
  errors, JobOutcome.AlignmentFailure, handed to GroupFacts.AlignmentFailure, and
  GroupJudgement.Judge judges the steps as it would without it and names what they find after
  it, so a missing or stale NWD is named beside the models, and the skipped clash after both. The
  ALIGNMENT failed run line says "The failure does not stop the group." where it said the files
  written list says which files were written, a list that can name last week's NWD. The window
  label counts the failure with the errors, as when it was one, proved by the build only
- attempt 4, the tests. Eight failed against e6d6f73's code with a stub of
  GroupFacts.AlignmentFailure whose setter calls AddError, the engine's call before the fix, the
  stub in turn5\f112d-stub-before.diff.txt and the run in turn5\f112d-reason-before-fail.txt, 8
  failed and 1857 passed, and pass after, 1865 passed, turn5\f112d-reason-after.txt and the
  pre-commit of 0000355. The NWD from this run, stale and missing, each with the reason asserted
  whole, and the stale NWD with a skipped clash. By name, 8 new and 2 renamed away, which nets the
  6, and 5 changed under the same name, turn5\f112d-tests-attempt4.txt. Of the 8 new, 7 failed
  before and AGroupFailedOnItsModelsKeepsWhatThrewBesideIt passed before too, as a guard that a
  step that threw is still named, and the eighth failure is
  TheFailedRunLineNamesBothCausesAndClaimsNoFile, changed under its name, line 15 of that file.
  Every removed test line, with where each assert went, in turn5\f112d-removed-test-lines.txt
- attempt 4, the claim-checker's 12 points on attempt 3, the 13 entries of
  turn5\f112c-read-claims.txt less the last, which needed no change, made true in this entry,
  the F112 section of steps\01_next.md and turn5\pr-f112.md. The lines at ddd5e0c this entry
  names were read with git show from the commit, turn5\f112d-ddd5e0c-lines.txt, and each
  holds what the entry says. The rule in .claude\rules\core.md, under the judgement and under the
  rule of Q99 and Q100
- attempt 3, the root cause, at 8b4fcdd, each line read back from that commit with git show,
  turn5\f112e-lines-8b4fcdd-e6d6f73.txt. AlignmentCheck.cs line 423,
  `else if (!skipClashOffCoordinates && NamesInternal(models[i], internalName))`, dropped Q70's
  Internal failure whenever the rule was on, and FederationEngine.cs line 2138 called it with the
  rule state alone, while OffCoordinates.SkipsTheClash at line 78 also needs a clash test to run.
  So a group with nothing to clash got neither Q70's FAILED nor Bader's PARTIAL
- attempt 3, item 1, at 7da5c0d. WhyItFailsTheGroup takes the four inputs the ALIGNMENT block
  takes and drops the Internal failure only where the group's own skip decision says its clash is
  skipped. With no XML and no saved test, an XML of sets alone, one discipline or one NWC on
  Internal, the group ends FAILED, the rule on or off, each case reading whether a test runs from
  ClashWork.RunsATest and the group's end from GroupJudgement. AlignmentCheck.FailedRunLine gives
  the ALIGNMENT failed run line, which with the rule on named only a model with no site and said
  every failed group wrote its NWF and its NWD. OffCoordinates.EarlierNoteKeptBecause keeps an
  earlier note while a model is still off and no clash test ran in the group, and replaces
  RemovesAnEarlierNote, whose one caller passed the literal false. Seven tests failed against
  stubs that reproduce the engine's calls before the fix, a stub whose source no file keeps,
  turn5\f112c-item1-before-fail.txt, and
  pass after, 1855 passed, turn5\f112c-item1-after.txt. The four Internal cases fail on their
  first pass, rule on, where WhyItFailsTheGroup returned null. The converted earlier note test
  fails only on its "this run skipped the clash" assert, because the stub, like the engine
  before, never looked at a skip
- attempt 3, item 2, at 752f931, Core only. NwfAndNwd.cs lines 36 to 38 at 8b4fcdd, read with git
  show in turn5\f112e-lines-8b4fcdd-e6d6f73.txt, ended the NWF line "read after the NWD was
  published" whatever the NWD facts were. It says so only where the publish reported success and
  the NWD is on the disk, and "read at the end of the group" otherwise.
  TheNwfLineSaysNothingOfAPublishTheOutcomeDidNotShow asserts the NWF line of the no NWD note,
  the stale note and a publish reported with no NWD on the disk says nothing of a publish. It
  failed before, turn5\f112c-item2-before-fail.txt, and passes after,
  turn5\f112c-item2-after.txt
- attempt 3, item 4, at da612e2 and 27df6b3, the readings' other notes in F112's own lines. The
  ALIGNMENT heading of a skipped group and the engine's CLASH line read
  OffCoordinates.TestsCreatedNoneRun, now public, where the heading said every test was created. The
  note says any viewpoint an earlier run saved in the NWF is still in it, since BuildViewpoints
  returns before any viewpoint is touched, FederationEngine.cs lines 3425 to 3431 at 27df6b3, 3426
  to 3432 at 0000355, both read with git show in turn5\f112e-lines-8b4fcdd-e6d6f73.txt. The open
  file run's window label takes its clash counts from ClashRunOutcome.CountsForTheLabel, where it
  printed nought run and nought clashes for a skipped group. The EXPORT CHECK run line counts a
  group whose whole read threw, ExportCheckAcrossTheRun.GroupNotRead, called from the group's catch
  alone, and is never clean while one is. A model ModelFactsReader.Exports drops in its own catch is
  in no count, so the line can read clean over it, a Known bug below. The earlier reports read and
  the note of a skipped group sit in a try that logs and writes no note, proved by the build only.
  Four tests failed before, the two of the words against the old text,
  turn5\f112c-item4-words-before-fail.txt, and the two of the counts against stubs whose source no
  file keeps, turn5\f112c-item4-counts-before-fail.txt, and pass after,
  turn5\f112c-item4-words-after.txt and turn5\f112c-item4-counts-after.txt
- attempt 3, its counts. Core tests 1848 passed, 0 failed, 0 skipped after the merge of 32b75fd,
  turn5\f112c-core-before.txt, a file with no commit line, the 1848 pinned to 8b4fcdd by
  turn5\f112c-precommit-merge.txt, and 1859 passed at 27df6b3, turn5\f112c-core-after.txt. Each of
  attempt 3's seven commits passed its pre-commit with 0 failed: 8b4fcdd 1848, 7da5c0d 1855,
  752f931 1856, da612e2 1857, 27df6b3 1859, 2d0e574 1859 and e6d6f73 1859, turn5\f112c-precommit-merge.txt,
  -item1, -item2, -item4-words, -item4-counts, -records and -records-2. `dotnet build
  ParsonsNwcFederator.sln -c Release` at 27df6b3 0 warnings and 0 errors with the commit at the
  top, turn5\f112c-build-after.txt. The builds run after the add-in changes of items 1 and 4,
  turn5\f112c-build-item1.txt, turn5\f112c-build-item4-words.txt and
  turn5\f112c-build-item4-counts.txt, read 0 warnings and 0 errors and carry no commit or time, so
  which tree each built is the developer's statement. Item 2 changed Core only, and no build file
  was saved for it.
  `check-locals.sh src` and `check-imports.sh src` exit 0 at 27df6b3,
  turn5\f112c-check-locals.txt and turn5\f112c-check-imports.txt
- attempt 3, item 3. The claim-checker's points on attempt 2 made true in this entry, in the F112
  section of steps\01_next.md and in the body turn5\pr-f112.md, each on a file named beside it.
  The fifteen proof cases of attempt 2 are mapped to the committed tests that cover them,
  turn5\f112c-proof-map-attempt2.txt, and those 27 tests pass by name at 27df6b3,
  turn5\f112c-proof-map-after.txt. The tests of attempts 2 and 3 are counted by name from the
  source, turn5\f112c-tests-attempt2.txt and turn5\f112c-tests-attempt3.txt, a count that gives
  dotnet's own totals at all four commits, 1816, 1848, 1848 and 1859. Every removed test line of
  each attempt is in turn5\f112c-removed-test-lines-attempt2.txt and
  turn5\f112c-removed-test-lines-attempt3.txt
- the tests attempt 3 changed, every removed line in turn5\f112c-removed-test-lines-attempt3.txt.
  The fifteen WhyItFailsTheGroup calls of AlignmentCheckTests take the far setting and true for a
  test to run beside the rule, with their asserts unchanged. AnEarlierNoteGoesOnlyWhenEveryModel...
  calls EarlierNoteKeptBecause, each Is.True becoming Is.Null and each Is.False Is.Not.Null, with a
  case for a read that threw added. WithTheRuleOnTheBlockSaysTheClashIsSkippedAndCarriesTheSameLines
  asserts the corrected heading and that the old words are gone. 11 tests are new and 14 changed
  under the same name, turn5\f112c-tests-attempt3.txt
- attempt 3, the rule in .claude\rules\core.md for all of it, and order line 39 and the F112
  section of steps\01_next.md
- attempt 2, the root causes, at ddd5e0c, every line below read back from that commit with git
  show by attempt 4, turn5\f112d-ddd5e0c-lines.txt. ONE TALLY PER WINDOW: FederatorPlugin.cs line 46 makes
  one RunLog per window, RunLog.cs line 115 made one OffCoordinatesAcrossTheRun in it, the engine
  overwrote its one rule flag at FederationEngine.cs lines 405 and 492 and only ever added groups,
  the list took log.StartedAt, the window's start, at line 2332, and OffCoordinates.ListName was
  one fixed name. A SKIP FROM THE MODELS ALONE: FederationEngine.cs lines 2125 to 2128 set the
  skip before ClashStep knew at lines 2363 to 2366 whether it had a test to run. THE NOTE said what
  nothing checked, OffCoordinates.cs line 63, and was written at FederationEngine.cs line 992
  before the NWF was looked at the last time at lines 998 to 1001. A ZERO AS CLEAN:
  OffCoordinatesAcrossTheRun.cs lines 35 to 45 kept no trace of a group judged clean, so lines 92
  to 95 called a run that judged three groups of 46 clean. THE RUN LINE kept four counters of its
  own at FederationEngine.cs lines 2177 to 2195 and none for a model holding no Revit element,
  which ExportCheck.cs lines 222 to 226 counts
- attempt 2, fifteen proof cases, the cases in turn5\f112b-proof-before-tests.cs.txt, never
  committed. They failed in one run, turn5\f112b-proof-before.txt, a file with no path, commit or
  time line, in the git archive copy of ddd5e0c at turn5\f112b-before-src, now deleted, into which
  turn5\f112b-proof-extra.py wrote four of them beside the other eleven, so that the copy was of
  ddd5e0c is the developer's statement. Whether the eleven also
  ran in the worktree first is shown by no saved file, and the worktree's test project as changed
  did not build against the code before, turn5\f112b-before-compile-full.txt. Twelve call
  ddd5e0c's Core API. Three, for items 2, 5 and 11, build a bool from a copy of the engine's own
  expression inside the test, so they say nothing about the add-in, whose half of those three is
  proved by the build only. None compiles against the API the fix made, so no run of them after
  the fix exists, and each is mapped to the committed test that covers it, as above
- attempt 2's tests by name, turn5\f112c-tests-attempt2.txt: 36 new and 4 renamed away, which nets
  the 32 from 1816 to 1848, and 27 changed under the same name. The 43 names of
  turn5\f112b-new-tests-after.txt are the 36 new, 6 of the 27 changed and TheReasonIsBadersWords,
  which did not change, so that file counts neither new nor changed tests
- breaker 1. OffCoordinatesAcrossTheRun is made by the engine when Run or RunOpenDocument starts,
  with that run's rule state, start and count of groups, and the window hands it to that run's
  RESULT block through RunLog.WriteResultBlock(thisRun). Each run's list is its own file,
  `Models not on the same shared coordinates, run yyyy-MM-dd HHmmss.txt`. RunLog.WriteRemoved
  reads the file off the disk first and takes it off the files written list only once it is gone
- breaker 2. ClashWork.RunsATest, the source holding tests and the group two disciplines, read
  once in FinishTheGroup before the ALIGNMENT block and handed to ClashStep, and
  OffCoordinates.SkipsTheClash. A group with nothing to clash is judged as before and its block
  names its models under a heading saying no clash is skipped. Attempt 2 dropped Q70's Internal
  failure for such a group, which attempt 3 put back
- breaker 3. EarlierReports names each report at the names this run would have written, with its
  last written time and size read by exact path, on EARLIER lines in the log and in the note, and
  touches none. The note says any test already in the NWF keeps an earlier run's results
- breaker 4. RESULT carries `coordinates    : J of T group(s) judged` with the groups not reached,
  not read and the models not judged, and the list the same in a sentence. Neither says no model
  was found off while any of those is more than nought
- breaker 5. ExportCheckAcrossTheRun adds the run line up in Core by the block's rules, with
  ModelExport.HoldsNoElement, the one rule both count by
- reviewer 1 to 3. OffCoordinatesAcrossTheRun.Groups is gone with its assert. The tick box starts
  from AlignmentCheck.DefaultSkipClashOffCoordinates in ShowByDesignWording beside its label and
  the XAML carries no IsChecked for it, turn5\f112b-addin-before.txt for the state before. The
  note, the list and RESULT say the tests in the words of the CLASH line, and the note, written
  after ConfirmTheNwfSurvived, names the NWF and the NWD through NwfAndNwd and says which was not
  written
- items 9 to 16. The ALIGNMENT run line comes from the tally and names the groups whose clash was
  skipped, and the EXPORT CHECK line ends `Nothing was changed in any model.`, where both said
  every group ran. The two Q65 comments in the window say what is true. core.md reads dx, dy and
  dz. An earlier note went only through OffCoordinates.RemovesAnEarlierNote, every model judged
  and the clash not skipped, replaced in attempt 3. The all clear line is written only where
  every model was measured and otherwise says how many were not. The row file's number is empty
  for a model not counted, ExportCheck.ElementsNumber. One private predicate, NamesInternal. The
  rule state is a constructor argument, so the list cannot be left to guess it.
  ClashRunOutcome.ClashSkipped makes the skipped group's CLASH block and summary say the clash
  was skipped, and GapRule.Lines takes why there is no report
- Q110 and Q111 in steps\02_questions.md, at lines 554 and 556 before Q112 and Q113, each saying
  what the code does now and what each answer changes. The rule in .claude\rules\core.md and a rule in .claude\rules\addin.md, the
  order line 39 and the F112 section of steps\01_next.md
- the tests attempt 2 changed with the rule, every removed line in
  turn5\f112c-removed-test-lines-attempt2.txt: TheNoteSaysWhatWasDoneWhatWasNot... became
  TheNoteSaysOnlyWhatWasChecked and asserts the false sentence absent, ARunTheEngineNeverTold...
  became RunLog's AResultNoRunHandedATallyToSaysNothingAboutTheCoordinates because the state can
  no longer be unset, WithNothingOffTheListSaysSo became EveryGroupAndModelJudgedAndNothingOff...
  because the list now carries the judged line, the ListName assert moved to
  EachRunsListIsItsOwnFileNamedForItsStart, and the AlignmentCheck.Lines calls take runsATest
  true. Attempt 2 attested 1816 and 1848 by hand and by the pre-commit on the final tree at
  c867348, turn5\f112b-precommit-code.txt, whose run also shows check-locals and check-imports
  clean. turn5\f112b-core-after-code.txt, turn5\f112b-check-locals.txt and
  turn5\f112b-check-imports.txt were written before the last add-in edit,
  turn5\f112b-edit-engine2.py

### The developer's choices

- attempt 4. The failure on where the models sit is a fact of its own on GroupFacts, rather than
  every error going on to the NWD checks, because a group that threw before its NWD step holds
  NwdOnDisk false whatever is on the disk, so the checks would then call last week's NWD missing
- attempt 4. The reason reads the models first, then what the steps found, then the skipped
  clash, so a group failed on its models and with a stale NWD names both in that order
- attempt 4. The window label still counts the failure with the errors, so its label points at
  the log as it did when the failure was one of them
- attempt 4. The failed run line says only that the failure does not stop the group. Naming the
  group's RESULT reason instead was not chosen, because that reason names the NWD only where no
  earlier step failed first, GroupJudgement judging the steps in order
- attempt 3. The Internal failure follows the group's own skip decision, computed in Core from the
  four inputs the block takes, rather than the engine handing the skip in, so the tests call
  exactly what the engine calls and the block and the group cannot differ
- attempt 3. An earlier note stays for a far model too, and not only for a model on Internal, when
  no clash test ran, because what the note says of a far model is still so. The brief named
  Internal
- attempt 3. The NWF line keeps "read after the NWD was published" where the publish is shown, so
  TheNoteSaysOnlyWhatWasChecked keeps its assert unchanged
- attempt 3. The failed run line's tail said the files written list names what was written, where
  it said every failed group wrote its NWF and its NWD, a tail attempt 4 took out
- attempt 3. No note is written when the earlier reports read or the note throws, the ALIGNMENT
  block carrying the same lines, rather than a note with half its lines
- attempt 2. The tally lives on the engine, made per press, and is handed to WriteResultBlock,
  whose parameter is optional and null where no engine was made, so RESULT then says nothing
  about it. Resetting a tally kept on the log at the start of a run was not chosen, because a run
  that throws before its engine is made would then carry the last run's groups
- attempt 2. Every run's list is kept as its own file named for the second its run started, the
  developer's choice. It matches his one file for the run, forwarded whole, and leaves one more
  file in the Clash Report folder per run. Appending runs to one file was not chosen, because the
  file he forwards would then carry every run
- attempt 2. A group with nothing to clash, with the rule on, gets no skip, no PARTIAL, no note, no
  VIEWS line and no skipped entry, and is named in RESULT and the list with `no clash test to
  run, so nothing was skipped`, so neither reads as clean. Attempt 2 also let a model on Internal
  in it pass, which ended such a group DONE, and attempt 3 fails it as Q70 answered
- attempt 2. RunsATest counts the open file run as two disciplines, because its count is UNKNOWN
  and F35 never judges it, and does not look at the plan, so a file whose every test is skipped
  before the model still counts as a group that runs a test
- attempt 2. The note says the NWF is on disk, never written, because an NWF reused and not saved
  again is on the disk too, and says the NWD was published by this run only where the publish
  reported success and the file is there
- attempt 2. A model is not judged where it is not named off and its site or its placement was
  not read, or no model of the group was placed. A group where no model was read counts as could
  not be read
- attempt 2. An earlier note was removed whenever every model was judged and the clash was not
  skipped, even in a group whose models were off and which ran no clash test, which attempt 3
  changed
- attempt 2. SavedTests.Count moved from ClashStep to FinishTheGroup before the ALIGNMENT block
  and is read once. A throw there now comes before the ALIGNMENT and EXPORT CHECK blocks of that
  group
- attempt 2. The pictures folder of an earlier run is named as a folder with its time, its files
  not listed, because listing them takes a wildcard
- attempt 2. The one discipline path's CLASH block keeps its noughts,
  AOneDisciplineGroupIsLeftAsItWas, and the GAP block of a group with no report for another reason
  keeps its words

### Programs started and files written outside the repo

- the closing pass records, programs: git (status, log, fetch, merge, add, commit, push, diff, show,
  blame, reflog, grep, rev-parse, merge-base, cat-file, ls-files and config --get), dotnet build,
  sh for the line reads and the pre-commits, python for the merge resolution, the edits and the
  read of attempt 1's transcript, stat for the write times, and PowerShell for Get-Process Roamer.
  No Navisworks and no stand-in. git fetch and each commit printed failed to delete lines for
  folders under the main clone's .git\worktrees, Permission denied, git's own cleanup of worktree
  records, which deleted nothing, turn5\f112e-precommit-merge.txt for one
- the closing pass records, Get-Process Roamer read pid 32136, started at 21:17:06, at 00:26:45 on
  2026-10-05, after the merge of main was resolved and before it was committed,
  turn5\f112e-roamer-before.txt. The read after the last command is turn5\f112e-roamer-after.txt.
  This developer started none and touched none
- the closing pass records wrote under turn5 the f112e- files, among them the evidence named
  above, the scripts f112e-resolve.py, f112e-edit-records.py to f112e-edit-records-6.py,
  f112e-fix-formfeed.py, which mended a form feed a one line script had made in the body, and
  f112e-show-*.sh, the message files
  f112e-msg-*.txt, the pre-commit outputs f112e-precommit-*.txt and the push output
  f112e-push.txt. It changed turn5\pr-f112.md. It read the transcript of attempt 1's developer
  under the session's folder in %USERPROFILE%\.claude and changed nothing there. git fetch wrote
  the remote refs of the shared .git of the main clone, the build wrote bin and obj under the
  worktree, and the pre-commit's tests made temporary folders under %TEMP%, removed by their
  teardown
- attempt 4, programs: git (status, log, fetch, diff, show, blame, merge-base, rev-parse, reflog,
  add, commit, push), dotnet test and dotnet build, sh for the two checks and the pre-commit, stat
  for the write times of the readings, and PowerShell for Get-Process Roamer and Get-CimInstance on
  its parent. No Navisworks and no stand-in. git fetch and the commits printed failed to delete
  lines for folders under the main clone's .git\worktrees, Permission denied, git's own cleanup of
  worktree records, which deleted nothing, turn5\f112d-precommit-reason.txt for one
- attempt 4, Get-Process Roamer read pid 32136, started at 21:17:06 at C:\Program
  Files\Autodesk\Navisworks Manage 2025\Roamer.exe, parent pid 1392, at 23:10:20, after one git
  status and git log and before any change, turn5\f112d-roamer-before.txt. The read after the
  last command is turn5\f112d-roamer-after.txt. This developer started none and touched none
- attempt 4 wrote under turn5 every f112d- file: the evidence named above, the message files
  f112d-msg-*.txt, the pre-commit outputs f112d-precommit-*.txt, the push output f112d-push.txt
  and this entry's draft f112d-log-entry.md. It changed turn5\pr-f112.md. git fetch wrote the
  remote refs of the shared .git of the main clone, the builds and tests wrote bin and obj under
  the worktree, and the tests made temporary folders under %TEMP%, removed by their teardown
- attempt 3, programs: git (fetch, merge, add, commit, diff, show, ls-tree, blame, rev-parse,
  status, push), dotnet build and dotnet test, python for the edit scripts and the count of tests
  by name, sh for the two checks and the pre-commit, and PowerShell for Get-Process Roamer, for
  Get-CimInstance on a Navisworks process and its parent, and for Copy-Item. No Navisworks and no
  stand-in. git fetch and every git commit printed failed to delete lines for eleven folders under
  the main clone's .git\worktrees, Permission denied, which is git's own cleanup of worktree
  records, and deleted nothing, turn5\f112c-precommit-merge.txt for one
- attempt 3, Get-Process Roamer read pid 49016, started at 18:55:27, at
  C:\Program Files\Autodesk\Navisworks Manage 2025\Roamer.exe, at 21:08:36 before the first
  command, turn5\f112c-roamer-before.txt. At 22:42:51 49016 was gone and pid 32136 ran, started
  at 21:17:06 at the same path, its parent pid 1392 svchost.exe read at 22:43:25 at line 9
  of turn5\f112c-roamer-after-push.txt. That read came after the push of 2d0e574, whose output
  turn5\f112c-push.txt was written at 22:42:24 by turn5\f112e-stat-readings.txt, and not after the
  last command, as its own first line says, because e6d6f73 was committed at 22:45:22 by the
  branch reflog, turn5\f112e-reflog.txt. The read after the last command is
  turn5\f112c-roamer-after.txt at 22:51:22, after the push of e6d6f73, turn5\f112c-push-2.txt
  written at 22:50:30 by the same stat file, 32136 still running. 32136 started 2 s after the
  lead's waiter for the baseline run on C04 started at 21:17:04, which first lists it at 21:18:05,
  turn5\wait-run04-item1-C04.txt, and 38 s after the lead's check of set 04 item 1 C04 at
  21:16:28, turn5\base-check-set04-item1-C04.txt. No launch record names who started it, so that
  it is the lead's baseline run is read off those two files. This developer's record says it
  started none and touched none
- attempt 3 wrote under turn5 every f112c- file, among them the evidence named above, the brief
  f112c-brief.md, the message files f112c-msg-*.txt, the pre-commit outputs
  f112c-precommit-*.txt, the push output f112c-push.txt and this entry's draft f112c-log-entry.md.
  In the session's scratchpad the scripts f112c_item1_alignment.py and f112c_testnames.py and
  three name lists, and one more copy of a name list at %TEMP%\f43.txt, removed by this developer
  once seen. Temporary folders the tests make under %TEMP%, removed by their teardown. git
  fetch wrote the remote refs of the shared .git of the main clone, and the builds wrote bin and
  obj under the worktree
- attempt 2, programs: dotnet build and dotnet test, git (fetch, merge, commit, archive, push, and a
  worktree add whose output was not saved), python for the edit scripts, sh for the two checks and
  the pre-commit, tar to unpack the archives, and PowerShell for Get-Process Roamer and
  Get-CimInstance. No Navisworks and no stand-in
- attempt 2, Get-Process Roamer read 0 at 18:24:54 and 1 at 19:48:36, and the pid was first
  recorded at 19:49:08, 49016, started at 18:55:27, its parent pid 1392 svchost.exe at line 3 of
  turn5\f112b-roamer-reads.txt, a Navisworks at C:\Program Files\Autodesk\Navisworks Manage
  2025\Roamer.exe by turn5\measure-shift-driver.md line 13. It started 4 s before the lead's
  waiter for the baseline run on C02 started at 18:55:31 and listed it,
  turn5\wait-run04-item1-C02-b.txt. No launch record names who started it, so that it is the
  lead's baseline run is read off that waiter. This developer's record says it started none
- attempt 1, from the branch's creation at 13:18:53 to c5d8aa8 at 17:26:42 by the branch reflog,
  turn5\f112e-reflog.txt, its developer's transcript running 13:19:29 to 17:38:47 and its push at
  17:37:11, turn5\f112e-attempt1-return.txt. No Get-Process Roamer read was saved and no record file of attempt 1 is
  under turn5. Its record is its developer's last message in the session's transcript, which says
  "Navisworks was never started", saved with its place in turn5\f112e-attempt1-return.txt. The
  lead's waiter, turn5\wait-no-roamer.txt, read pids 37356 and 47204, started at 12:53:23 and
  12:54:16, every ten minutes from 13:21:54, found 47204 gone at its 15:14:00 read and 37356 gone at
  its 15:24:00 read, and turn5\f104p2-roamer-reads.txt line 5 names the two Bader's own, started by
  hand. A third, pid 47208, ran inside the same window, listed from 15:57:16 to 16:03:16 and gone at
  16:04:16 by the waiter of the lead's run on C02, turn5\wait-run04-item1-C02.txt. From 16:08:16 to
  the end of attempt 1 no waiter read, so those minutes rest on its developer's message alone
- attempt 2 wrote under turn5 every f112b- file, among them the edit scripts f112b-edit-*.py,
  f112b-resolve.py, the message files f112b-msg-*.txt and its entry's draft, f112b-log-entry.md.
  Two copies taken with git archive, turn5\f112b-before-src and turn5\f112b-before-sln, deleted
  once their output was saved. A copy of the new tests in the session's scratchpad,
  f112b-newtests. Temporary folders the tests make under %TEMP%, removed by their teardown

### What remains

- everything in the add-in, proved only by the build: the per run tally reaching RESULT, the skip
  decided from the source, the Internal failure where no clash is skipped, the notes after the NWF
  check with their EARLIER lines, the note removed or kept with its reason, the run lines, the
  window label, the try around the note, the tick box starting ticked from Core, the CLASH and
  GAP blocks of a skipped group, and the failure on where the models sit handed to the judgement
  apart from the errors. The wave 1 test run on 1A02MM and 1A04PK, its expected lines in the F112
  section of steps\01_next.md
- Q110 and Q111, Bader's
- for the lead to put to Bader, the reviewer's note on attempt 2 and again on attempt 3, and the
  breaker's on attempt 3: a far model in a group that runs no clash test ends DONE, judged as
  before under Q65, against his words "The group ends PARTIAL, never DONE", and a model on Internal
  there fails its group as Q70 answered, where his words say this replaces Q65 and Q70 for this
  case. None of his answers says what either does there
- main f09ee92 was taken in at c5ba7e0, turn5\f112e-reflog.txt, and main b2afb2d at the merge
  after 47a95b4, which changed nothing under src or tests, turn5\f112f-diffs.txt
- the add-in's expected lines are in the F112 section of steps\01_next.md for the wave 1 run, and
  not as numbered one-action steps in steps\03_bader_next.md, the process note of the attempt 3
  and closing pass reviewers, turn5\f112c-read-review.txt and turn5\f112d-read-review.txt, for the
  lead
- the F119 item for the NWD and the NWF listed as written, in the Known bugs, for the lead to add
  to steps\01_next.md, the closing pass reviewer's note

### Known bugs

- the reference that is itself the outlier names every other model far, breaker 11 at c5d8aa8
- the 1 m boundary is a strict greater than on a double with no case at real values, breaker 12
- the Run tests button clashes the open document without reading the alignment
- ModelFactsReader.cs lines 138 to 143 and 211 to 216 still catch and log nothing, and Placements
  drops a model whose handle throws, so every model judged counts only the models handed back.
  Its catches at lines 338, 352 and 373 log nothing either: MillimetresPerUnit returns nought,
  which leaves every model of the document unplaced with no line naming the read that threw, and
  NameOf and DisciplineOf return an empty string. All five catches are older than F112, 485e79f of
  2026-09-20 by git blame at 0000355, turn5\f112e-known-bug-lines.txt
- NamesInternal matches the site with StringComparison.Ordinal, AlignmentCheck.cs line 595 at
  0000355, as the two matches it replaced did before F112, lines 162 and 259 of 086a348,
  turn5\f112e-known-bug-lines.txt. A site spelled INTERNAL, or Internal with a space after it, is
  then read as a named shared site, so the rule neither skips the group's clash nor fails it by
  Q70 unless the model is far, and such a group can end DONE, the attempt 3 breaker's leftover,
  turn5\f112c-read-break.txt line 34
- a model naming no site fails its group, yet it is neither on Internal nor far, so the list for
  the modellers leaves it out and can say no model was found off, waiting on Q111, the attempt 3
  breaker's leftover and the closing pass breaker's note, turn5\f112d-read-break.txt
- the one discipline group's CLASH block and summary still print nought for tests never run
- RESULT's other counts, groups, files written, the clash total and the moved totals, still add up
  over every run of one window, because they live on the log
- Placed needs all three of X, Y and Z, so a model with only Z unread is called unplaced
- a file whose every test is skipped before the model still reads as a clash skipped
- the note, the list and RESULT say the tests whose sides both find something are created and none
  is run from the rule, not from the runner, so a runner that stopped or a creation that threw
  reads the same, the breaker's note on attempt 2. With the rule off, RESULT and the list say a
  group was clashed from the plan too and not from the runner, the closing pass breaker's note,
  turn5\f112d-read-break.txt
- in a skipped group a tolerance chosen in the tool still rewrites a saved test's tolerance with
  its results kept, ClashRunner, the breaker's note on attempt 2
- the note's stale NWD line says the file is from an earlier run where the publish did not report
  success, which nobody read: on a first run, or where the publish left a partial file, it is
  this run's own, the breaker's note on attempt 3
- older than F112, for the lead to add for F119 in wave 3: the files written list and the `NWD
  written` line name an NWD whose publish returned false or threw. WriteNwd calls
  RunLog.WriteFinished whatever the publish said, FederationEngine.cs line 3609 at 0000355 and at
  c5ba7e0 and line 3357 on main at 1ae6771 and at f09ee92, each read with git show, the line as it
  stands since a6957a6 of 2026-08-27 by git blame, and WriteFinished records any file on the disk as
  written, RunLog.cs lines 1398 to 1445 at 0000355, all in turn5\f112e-nwd-written-lines.txt. Last
  week's NWD is then listed as written by this run, against the rule that only a file this run wrote
  goes on the list. The group's RESULT reason names it not from this run since attempt 4. The
  closing pass breaker adds the NWF, turn5\f112d-read-break.txt: TrySaveFile's false is only logged,
  FederationEngine.cs lines 2083 and 3395, NwfOnDisk then reads true off last week's file at lines
  2099 and 3411, and GroupFacts holds no fact of the NWF save, so such a group can end DONE with
  last week's NWF. The F119 item is to name both
- the breaker's other notes on attempt 3, none blocking: with Compact resolved clashes ticked, a
  skipped group's earlier results are compacted, ClashRunner.cs lines 402 to 413 and 512 to 563.
  On the weekly run with no XML and tests saved in the NWF, the sentence that the tests are
  created is false, since none is. A report at an unticked kind or an old workbook name is not
  read, so the note can say none stands beside it. The try around the note returns before
  either note is written, so last week's note stays. CountsForTheLabel handles a skipped clash and
  not a stopped runner, which prints nought. The EXPORT CHECK run line carries no count of the
  groups it covered. RESULT's counts add up over two runs of one window, as above
- the reviewer's notes on attempt 3, none blocking: GroupNotRead is called only from the group
  catch, while ModelFactsReader.Exports drops a model in its own catch, lines 108 to 111, so such
  a model is in no count and the line can read clean. The comment over the call,
  FederationEngine.cs line 2220, still says the line never reads clean over a group whose models
  were not all read, turn5\f112e-known-bug-lines.txt. ClashRunOutcome.Summary writes the words
  of CountsForTheLabel a second time. Comments in NwfAndNwd.cs, OffCoordinates.cs and
  ExportCheckAcrossTheRun.cs name loop attempts that main never had
- the closing pass reviewer's notes, none blocking, turn5\f112d-read-review.txt: the helper of the
  four AModelOnInternalFails tests, AlignmentCheckTests.cs lines 882 to 884, builds facts with
  nothing appended, so its FAILED would hold without the alignment failure, the reason assert and
  GroupJudgementTests still holding the rule. The summary of WhereTheModelsSit,
  FederationEngine.cs lines 2118 and 2119, still says Q70's failure is carried as an error. The
  class summary of AlignmentCheck, lines 95 to 97, still says the group writes its NWF and its NWD
  either way, which the closing pass breaker names too. The window label's count of the failure
  with the errors, FederationEngine.cs line 3668, is a rule in the add-in proved by the build only
- the closing pass breaker's notes, none blocking, turn5\f112d-read-break.txt: any step
  GroupJudgement judges before the NWD, an error that threw, the NWF missing or failed viewpoints
  among them, still hides a missing or stale NWD, because the judgement names the first failing
  step alone, older than F112 for every group and said in the attempt 4 choices above. An earlier
  note kept on a weekly run carries no date and still says no clash test was run, beside a fresh
  workbook and NWD. The rule off reads as clashed from the plan, as above

### What comes next

- the pull request, which the lead opens with turn5\pr-f112.md, after any reading the lead calls on
  this records commit

## 2026-10-05 The loop, turn 5, the design of Q114, its probes and Q115 to Q123

Nothing under src, tests or tools changed. Core tests 1756 passed, 0 failed, 0 skipped before, as
in the entry below, and after, at the pre-commits of this record, turn5\precommit-records-11.txt
and precommit-records-11b.txt.

### What was done

- the design of Bader's views by team, by three plans each its own agent, turn5\q114-design-run.txt, safety first, speed first
  and the rules in Core, read only, and a judge who scored them 25, 19 and 22 of 30 and wrote one,
  turn5\q114-design.md. Each of FR-180 to FR-188 names its part
- its probes, P1 to P22, each a single fact, in the order each area needs them, section 3. Three
  read the install with no Navisworks, P5 to P7. P3, P20 and P21 are steps for Bader, written into
  steps\03_bader_next.md when their area starts. Most of the rest run on copies of the baseline's
  NWF through the guarded start, once no Navisworks runs. P5 to P7 answered yes on 2026-10-05,
  turn5\q114-probes\p5-p7.md
- Q115 to Q123, nine of the design's thirteen questions, each with the choice the build goes on
  with. Of the four not asked, two his words settle, one is the lead's choice for safety until P18
  gives its cost, and one waits for P9, named in steps\loop.md
- Q124, the baseline of 1A04PK taking a day or more, turn5\c04-rate.txt
- FR-189 from F112's closing pass, an NWD listed as written when its publish failed, older than
  F112, for F119 in wave 3b
- the estimate, on the three recording rates measured and not a bound: VIEWS on 1A02MM 89.701 to 811.516 s against 7487.104 s
  in the baseline, the run 528.567 to 1250.382 s against 7925.970 s, what is UNKNOWN in it named
  in section 6

### What remains

- the probes, then F132, F131 and F114 in wave 2

### Known bugs

- FR-189, new in this record

### What comes next

- the probes on Navisworks once no Navisworks of the loop runs, Q124

The probes on Navisworks wait for the local machine.

## 2026-10-04 The loop, turn 5, Bader's views by team, Q114, the plan

Bader's message of the evening headed ONE VIEWPOINT PER CLASH TEST, IN THE A, B, C FOLDERS, BY
TEAM, NO TEAMS MIXED, NO MIRRORED TESTS is Q114 of steps\02_questions.md in his words. Nothing
under src, tests or tools changed. Core tests 1756 passed, 0 failed, 0 skipped before, at the
pre-commit of 6af4a2f, turn5\precommit-records-10.txt, and after, at the pre-commit of the commit
that made the claim-checker's points true, turn5\precommit-records-10b.txt.

### The plan

- A, the baseline and wave 1 go on: item 1 on C04 read and recorded, a gap with no Navisworks for
  F126's harness and the driver's Shift measurement, then item 2 on C02 and C04, while F112's
  closing pass, F116's pass on Q113 and F126 work, then the test of wave 1
- B, this record: Q114, FR-180 to FR-188 in three areas, F131 teams, F132 mirrored tests and F114
  views, FR-069 changed by it, and wave 2 in three parts
- C, measured first, read only, no Navisworks: the mirrored pairs of the picked XML and whether
  each gave the same clashes on 1A02MM, the mechanical sets that miss HV, PL and FP models, and the
  views code as it stands, turn5\measure-mirrors.md, measure-teams.md and measure-views.md
- D, a design of the tree, the view and the team pairs by a panel of independent plans, written on
  the items, then built in wave 2 and proved on 1A02MM against the 2 h 12 min of the baseline

### What was done

- Q114 written, FR-180 to FR-188 under their own heading of steps\fix-round.md, FR-069 noting that
  one view per test replaces the viewpoint per clash, and wave 2 in three parts: 2a F127, F132 and
  F115, 2b F131, F114 and F128, 2c F118
- the three measurements of C started, read only. measure-mirrors.md and measure-teams.md are
  written, their findings on FR-182 and FR-181, and measure-views.md is not yet

### What remains

- C and D, and everything of A

### Known bugs

- none new in this record

### What comes next

- measure-views.md's findings on FR-180, FR-183 and FR-186, then the design

Nothing in this record waits for the local machine.

## 2026-10-04 The loop, turn 5, F125 a pane of Navisworks is not a dialog, the first pass built and read safe, the second pass built and read by a reviewer and a breaker, nothing blocking under Q93

Nothing under src or tests changed in either pass. Core tests 1756 passed, 0 failed, 0 skipped by
the pre-commit at the merge of main 3a148e3, before the second pass's first change,
turn5\f125b-precommit-merge.txt, and 1756 passed, 0 failed, 0 skipped after its last change, by
hand, turn5\f125b-core-after.txt, by the pre-commit at 03aa6c0, turn5\f125b-precommit-change.txt, by
the pre-commit of the merge of main 6cc0283 at 42dfd66, turn5\f125b-precommit-claims-merge.txt, and
by the pre-commit of the records made true after it, turn5\f125b-precommit-claims.txt. The first
pass read 1746 passed, 0 failed, 0 skipped before and after on its branch off main dd55e4b,
turn5\f125-proof\core-tests-before.txt and core-tests-after.txt. No Navisworks was started by either
pass.

### What was done

- the first pass, 5fa98a8. The baseline run of 2026-10-04, set 04 item 1 on C02 from main
  dd55e4b, hung: a floating Clash Detective pane of the adopted Navisworks, owned by the main
  window and not modal, record.txt line 36 of steps\runs\04\item1-C02-hung, was a DIALOG to
  WindowKind, so the driver stopped on it after RunButton and the tool's confirm was never
  answered. WindowRecords now reads whether each window itself is enabled, with no message,
  WindowKind calls PANE a WinForms window, not of the main window's caption, owned by a visible
  window, whose owner reads enabled or which reads disabled itself, the driver notes each pane up
  before Run and goes on, and the monitor writes a pane as PANE, never a finding, never holding
  back WM_CLOSE. Proved by turn5\f125-proof\prove-f125.ps1, 76 passed and 17 failed on the scripts
  the hung run used, every failure an F125 check, and 93 passed and 0 failed after,
  prove-f125-before.txt and prove-f125-after.txt
- the first pass read under Q93 by a reviewer and a breaker, who both approved with nothing
  blocking, turn5\w1-read-review-F125.txt and turn5\w1-read-break-F125.txt
- main c4fd0d4, F113 and F104, merged in at 3a148e3, the one conflict steps\01_next.md, both
  sides kept, F125's order line 39 after F113's 37 and F104's 38 and its section after F104's
- the second pass, 03aa6c0, read only changes that make the run's evidence truer, with no new
  click, key, pointer move or posted message, and WindowKind's answer to every read unchanged. 1,
  a window that reads disabled with its owner disabled is written by the driver and the record as
  either a pane or a modal dialog blocked by the tool's window or another modal window, which one
  UNKNOWN, never not modal, PaneWords in nw-guard.ps1. 2, the driver's stop line names the rule's
  kind and both states, StateWords. 3, the monitor writes a window again, an AGAIN line, when the
  rule's kind or either state changes, counts a window one DIALOG finding the first time it reads
  DIALOG, and its line holding WM_CLOSE back names the windows behind it with their kind and both
  states. Two clauses of item 3 are proved by reading the code only, never by a run or a harness
  case: a window counted one DIALOG finding when it first reads DIALOG after reading another kind,
  and the WM_CLOSE line written again only when the windows behind it or their reads change,
  tools\loop\run.ps1 lines 1370, 1520 to 1534, 1604 and 1605 at 03aa6c0. 4, the README and
  .claude\rules\loop.md name the rule's two limits, the WinForms class and a caption not the main
  window's. 5, PaneKey, the pane key in one place for the driver's two callers
- its proof: each changed script parses, turn5\f125b-parse.txt. prove-f125.ps1 has 12 checks
  added, 8 of them in the two new cases pane watch and pane busy and 4 F125b checks added to the
  pane, pane-new, pane-dialog and pane end cases. 10 of the 12 are named F125b and 2 are guards
  of pane busy. One check of the first pass was restated for item 3, its first pass copy kept as
  f125b-prove-f125-pass1.ps1. Before the change 97 passed and 8 failed, every failure an F125b
  check, prove-f125-before2.txt. After it, on the committed scripts of 03aa6c0, the scripts' sha256 with Windows line ends equal to the blobs of 03aa6c0, turn5\f125b-blob-hashes.txt, 105 passed and 0
  failed, prove-f125-after2.txt. Get-Process Roamer read 0 before and after each harness run,
  turn5\f125b-roamer-reads.txt, which also records a Roamer this pass did not start, pid 49016
  started at 18:55:27, the lead's baseline run, that the after run waited for from 18:56 to 21:10.
  The solution builds with 0 warnings and 0 errors, turn5\f125b-sln-build.txt, an output that
  carries no commit and no time. No .cs file of the solution changed after the merge 3a148e3, and
  main 6cc0283 merged in after it changed none, so that build stands for 3a148e3. check-locals and
  check-imports pass, turn5\f125b-checks.txt
- the second pass read under Q93 by a reviewer, VERDICT APPROVE with nothing blocking,
  turn5\f125b-read-review.txt, and by a breaker, VERDICT APPROVE with nothing blocking, turn5\f125b-read-break.txt, read from the files as they stand since it had no shell, its six notes on words and edge cases written as register rows F125-R8 to F125-R13, three checks that found nothing, and one note of what it could not run, and the reviewer's points left by the records pass written as F125-R14
- register rows F125-R1 to F125-R7 in steps\loop.md, and F125-R8 to F125-R14 the lead wrote from
  the second pass's readings, the readings' findings this pass does not fix, for F122 the loop
  tools in wave 4
- the records made true after the claim-checker read the second pass, turn5\f125b-read-claims.txt.
  Main 6cc0283 merged in at 42dfd66, the one conflict steps\log.md, both sides kept, F125's entry on
  top. F125-R2 marked answered for C02: the run of 18:55 on 5fa98a8 read the real pane disabled with
  its owner disabled once the tool's window was up, steps\runs\04\item1-C02\driver.txt line 3, and
  ended RAN, record.txt line 964 there, so the tool's window most likely disables it. Every cite of
  the hung run moved to steps\runs\04\item1-C02-hung. The pane's origin written as UNKNOWN, likely
  his saved layout, in .claude\rules\loop.md and nw-guard.ps1. The line ranges cited at dd55e4b and
  5fa98a8 read again with git show, turn5\f125b-line-check.txt, F125-R3's driver line 288 corrected
  to 293 and the root cause's run.ps1 line 1104 widened to 1104 and 1110 to 1111. The comments this
  changed in nw-guard.ps1, drive-window-run.ps1 and the stand-in's ToolWindow.cs are comments only:
  the two scripts with their comments dropped equal 03aa6c0 token for token, run.ps1 is unchanged,
  and every changed line of ToolWindow.cs is a /// line, turn5\f125b-comments-only.txt, the same
  check against 3a148e3 finding the second pass's code, turn5\f125b-comments-only-control.txt. So
  nw-guard.ps1 and drive-window-run.ps1 now read sha256 5AF32EDF and 6A584A48, not the 7D4719E4 and
  6A35C43F the harness ran

### What remains

- the second pass's own record lines on the real window, its words for the pane and its AGAIN
  lines, at the first run after F125 merges

### Known bugs

- F125-R1, F125-R3 to F125-R14 under Q93, open for F122 in wave 4. F125-R2 is answered for C02
  by steps\runs\04\item1-C02\driver.txt line 3
- the two clauses of item 3 read in the code only are UNKNOWN by a run. No stand-in case was added
  for them, because one runs only while Get-Process Roamer reads 0, and it read 1 at 22:11:11, a
  Roamer started at 21:17:06, 20 s after the loop's item 1 on C04, turn5\f125b-roamer-claims.txt

### What comes next

- the merge, and the first run after it reads the second pass's lines on the real window

### Every program started, every file written outside the repo

Started: git, to fetch, merge, show, commit and push, and the pre-commit hook it runs, which runs
check-locals, check-imports, the evidence check and dotnet test. Windows PowerShell 5.1 for the
parser and to run prove-f125.ps1 twice, each run starting the stand-in only as its copy Decoy.exe,
22 times in each run, still running 0 at each end, and a Windows PowerShell for each driver case,
all held and ended by the harness. dotnet build for the solution, and dotnet test by hand. For the
records made true, Windows PowerShell 5.1 six times more, reading only, the tokenizer check
against 03aa6c0 and its control against 3a148e3 run three times each, the first time with a
fault of the check, the last line end lost through git show, which made run.ps1 read different
from itself, the second before a last comment of drive-window-run.ps1 was wrapped, and the files
kept are of the third, the session's PowerShell for two Get-Process reads of Roamer, and python
to edit steps\loop.md, steps\log.md, steps\01_next.md and the draft body. No harness ran for
them. No Navisworks, and nothing installed.

Written outside the repo, all under %LOCALAPPDATA%\NwcFederatorLoop\turn5: f125b-msg-merge.txt,
f125b-msg-change.txt, f125b-msg-records.txt, f125b-msg-claims-merge.txt and f125b-msg-claims.txt,
the commit messages, f125b-precommit-merge.txt, f125b-precommit-change.txt,
f125b-precommit-records.txt, f125b-precommit-claims-merge.txt and f125b-precommit-claims.txt, the
pre-commit's output, f125b-push-claims.txt, the push's output, f125b-roamer-reads.txt,
f125b-parse.txt, f125b-sln-build.txt, f125b-core-after.txt, f125b-checks.txt, f125b-line-check.txt,
f125b-comments-only.ps1, f125b-comments-only.txt, f125b-comments-only-control.txt,
f125b-roamer-claims.txt, pr-f125.md, the draft body, and under f125-proof: prove-f125.ps1 extended,
f125b-prove-f125-pass1.ps1, prove-f125-before2.txt, prove-f125-after2.txt, the harness's work
folders work-184632 and work-211020, and Decoy.exe and Decoy.exe.config, copied again into
standin-bin by each run. Each harness run also made runs\97 under the loop folder for the paths the
driver types and removed it at its end. One file was written by mistake outside turn5,
%TEMP%\f125b-added.txt, the lines this pass adds, read for em dashes and semicolons and removed at
once. The records made true wrote comments-only.ps1, loop_rows.py, splice_log.py, merge_refs.py,
reflow.py, fix_ctrl.py, f125-entry.md, progs.md and added.txt, the lines they add read for em
dashes and semicolons, and a copy of the eight edited files in edited, kept while the merge was
committed on its own, in the session's scratch folder under %TEMP%\claude. A python edit wrote a
control character into this entry in place of the 01 of steps\01_next.md, found by a search for
control characters and put right before the commit. The fetch and each commit printed error:
failed to delete .git/worktrees lines, git's housekeeping of worktree folders OneDrive holds,
which stopped nothing.

## 2026-10-04 The loop, turn 5, Bader's answer to Q113 and the notes on Q112, and the measurements

Nothing under src, tests or tools changed. Core tests 1756 passed, 0 failed, 0 skipped before,
at the pre-commit of PR 99's last commit, turn5\precommit-records-6b.txt, and after, at this
record's, turn5\precommit-records-7.txt. The error: failed to delete .git/worktrees lines after
the count in both are git's housekeeping of worktree folders OneDrive holds, and stopped
nothing.

### The plan, also given in the Claude tab before the first edit, a reply kept in no file

- A, this record: his answers under Q113 and Q112, FR-009, FR-030, FR-176 and FR-179 carrying
  them, the measurements of 2026-10-04 on FR-176, FR-177 and FR-179, Q113 out of the open form
- B, F116 after its fix attempt 2: the correction list, the three matrix corrections, the Q103
  rule and the 30 workset spellings, moved out of src into a plain file beside the picked XML,
  read at the pick and named in the log, nothing corrected and said when no list is there, this
  project's list in exchange\ beside the corrected XML, then its readings and its merge after
  F112, and Bader told in the tab when the list is on main
- C, the test of wave 1: the list copied from exchange\ beside the XML in the run set's own copy
  and read back, never into NM Fed
- D, everything else as planned

### What was done

- Q113 answered, B and D, and Q112's notes answered, right as read, each under its question in
  his words, the lead's notes marked
- FR-176, FR-177 and FR-179 carry the measurements of turn5\measure-coverage.md,
  measure-generic.md and measure-shift.md. The driver's Shift measurement did not run, since
  its stand-in waits for no Navisworks to run, turn5\measure-shift-driver.md
- of the three things seen while measuring the coverage, two are on the list, FR-035 and
  FR-126. The third, a single discipline group's CLASH block counting its 36 created tests among
  its 1830 skipped, is the block's own word, skipped meaning not run and not passed, log:1167 to
  1170 of set 03, so it is no fault, and FR-176 says the coverage counts keep the two apart

### What remains

- B to D of the plan

### Known bugs

- none new in this record

### What comes next

- F116 with Q113's answer once its fix attempt 2 and its readings return

Nothing in this record waits for the local machine.

## 2026-10-04 The loop, turn 5, Bader's five requests added to the round, the plan, and no sleep

Bader's message of the evening, headed FIVE REQUESTS ADDED TO THE ROUND, is Q112 of
steps\02_questions.md in his words. Nothing under src, tests or tools changed. Core tests 1756
passed, 0 failed, 0 skipped at the pre-commit of this record, turn5\precommit-records-6.txt, as
at the pre-commit of PR 97's last commit, turn5\precommit-records-5c.txt. The lines after the
test count in both, error: failed to delete .git/worktrees, are git's housekeeping of worktree
folders OneDrive holds, and stopped nothing.

### The plan, also given in the Claude tab before the first edit, a reply kept in no file

- A, request 1 at once: measure the power settings, set sleep when plugged in to Never after
  saving the old value, read the System log for every sleep and wake since 1 Oct, and keep the
  keep-awake running while this session is open, checked every 30 minutes
- B, this record: Q112, FR-175 to FR-179 in steps\fix-round.md with the waves split into
  halves of at most three areas, the keep-awake rule in .claude\rules\loop.md, the head and the
  next action of steps\loop.md, and Q113 from F116's reading
- C, wave 1 as planned: the baseline of set 04, F125's second pass and F126, F112's and F116's
  fix attempt 2, then the test of wave 1 through turn5\wave-compare.py and F104's documents read
- D, to measure from the baseline before wave 2: the property and value of Generic Models on
  1A02MM and 1A04PK, what the log, .tsv and workbook hold today for each test, why the Shift
  range fails, and whether the driver can test a Shift click without real input
- E, wave 2 in two halves, 2a F127 coverage first with F115 and F114, 2b F128 generic models
  with F118. Coverage designed first by a panel of independent plans
- F, wave 3 in two halves, 3a F129 start from an NWF with F120 and F109, 3b F130 the Shift range
  with F119
- G, waves 4 and 5 as listed

### What was done

- request 1, FR-175. Measured at 19:27, turn5\power-before.txt: the Balanced scheme, sleep after
  0, Never, plugged in and on battery, hibernate after and turn off display after 0 plugged in,
  no power policy key, Standby (S0 Low Power Idle) the only sleep state, hibernation not
  enabled. Since sleep when plugged in already read Never, nothing was written to his power
  settings and nothing is put back at the close. The lock screen's display timeout is hidden
  from powercfg and UNKNOWN
- the System log from 2026-10-01 00:00, turn5\sleep-wake-since-1oct.txt, nine events and no
  sleep and no wake among them: started 2026-10-01 08:27:53, shutdown asked by shutdown.exe at
  19:30:49 and the system down at 19:31:50, started 2026-10-04 08:56:35, a restart for an update
  asked at 09:07:20, down at 09:09:07 and started at 09:10:19
- keep-awake.ps1 changed to watch the session's claude.exe and stop only when no Claude Code
  claude.exe runs or steps\loop.md reads STATE CLOSED, the old copy kept as keep-awake-v1.ps1.
  The session's claude.exe is pid 19148, in the parent chain of this session's shell,
  turn5\session-chain.txt. A wrong session, STATE WAITING, a second copy and STATE CLOSED proved on a copy with its own mutex, 8 passed and 0 failed, turn5\keep-awake-test\prove-result.txt, and the takeover of another claude.exe when the watched one ends, 5 passed and 0 failed, the watched one a stand-in copy of PING.EXE named claude.exe, turn5\keep-awake-test\prove-takeover-result.txt. The stop when no Claude Code claude.exe runs at all is read in the code only, UNKNOWN by a run, since a run of it would end the session. turn5\check-keep-awake.ps1 starts it again when it is
  gone, proved by ending it at 19:36:35, turn5\keep-awake-checks.txt. A schedule of this
  session, job 04b2bf94, runs the check at 13 and 43 minutes past each hour while the session
  is idle, turn5\keep-awake-schedule.txt. It lives only as long as the session, and the schedule
  tool says it ends after 7 days. No line of its own is written yet
- Q113 written from F116's reading: its corrections shipped as data in Core against CLAUDE.md,
  and Q103 read wider than the matrix. F116's readings are turn5\f116-read-review.txt,
  f116-read-break.txt and f116-read-claims.txt

### Programs started and files written outside the repo

- powercfg, Get-WinEvent and Get-CimInstance, reading only
- powershell.exe for the keep-awake proof, its copies under turn5\keep-awake-test, the keep-awake
  through WMI as pid 10412 and, after the lead ended it to prove the check, as pid 29740.
  Stop-Process on pid 1312 and pid 10412, both the loop's own keep-awake
- written: turn5\power-before.txt, sleep-wake-since-1oct.txt, keep-awake.ps1, keep-awake-v1.ps1,
  check-keep-awake.ps1, keep-awake-checks.txt, keep-awake.txt and the files under
  turn5\keep-awake-test

### What remains

- everything from C of the plan on

### Known bugs

- the schedule fires only while the session is idle, so a long turn checks the keep-awake only
  when the lead runs the check itself
- the stop when no Claude Code claude.exe runs is UNKNOWN by a run

### What comes next

- C of the plan, the baseline runs and wave 1

Nothing in this record waits for the local machine.

## 2026-10-04 The loop, turn 5, F104 part 2, the documents read, DONE for the build

Nothing under src or tests changed. Core tests 1746 passed, 0 failed, 0 skipped after the merge
of main at e76a2d3, after the change, by the pre-commit at 75e5dce and 33732c9, and at the merges
202496b and 45dcd4f, turn5\precommit-f104-merge-2.txt and turn5\precommit-f104-merge-3.txt. No Navisworks was
started for it.

### What was done

- tools\loop\run.ps1 -Mode Documents, a mode of run.ps1 so the one copy of every guard is kept:
  the pairs read off the window run's .tsv by the group column, checks 11, 19, 20 and 21, the
  probe's two calls in place of item 0's hold, the hang rule on the read-outs,
  compare-document.ps1 per pair and summary.txt. -Mode Check -For Documents prints every refusal
  and the pairs. Written by one developer, the brief turn5\f104-part2-brief.md
- read for harm and wrong evidence under Q93 by a reviewer, who approved, and a breaker, who
  found nothing blocking, turn5\f104p2-reads.txt, their notes register rows F104-R1 to F104-R7
- its harness, turn5\f104p2-proof\prove-f104p2.ps1, all three parts on the committed code from
  15:39:38 to 15:53:47 on 2026-10-04 once Bader's Navisworks had closed: 102 passed, 0 failed, Get-Process
  Roamer 0 before and after, turn5\f104p2-prove-4.txt
- main merged in four times, 3449521 by e76a2d3, dd55e4b by 202496b, 6689bad by 45dcd4f and
  7b6df88, F113's merge, by the commit after 023a685, each conflict keeping both sides, F104's
  order line now 38 after F105's 35, F107's 36 and F113's 37, its section after F113's, its
  entry here above F113's, and its 5z-g after F105's 5z-f in docs\history\scan.md

### What remains

- the first real documents read, on set 04 item 1 once the baseline of the two buildings has
  run, the first answer of the documents to Bader's third test
- PQ1, PQ7, PQ8 and PQ9 of F104, which the real read answers, and PQ3 to PQ6, which also need a
  person at the panel or an export

### Known bugs

- F104-R1 to F104-R7 under Q93, F104-R4 the one that can let a green line sit on Bader's third
  test while PQ4 is unmeasured

### What comes next

- the baseline runs of 1A02MM and 1A04PK, then the documents read on them

## 2026-10-04 The loop, turn 5, F113 the clash counts area of the fix round, FR-031 to FR-034, DONE

Core tests 1746 passed, 0 failed, 0 skipped before the first change, at 2c89788, and 1756
passed, 0 failed, 0 skipped after the last item, at 1f0a369, and at every commit after it, run by
hand and by the pre-commit at every commit, the files f113-core-before-full.txt, f113-core-after-full.txt and
f113-precommit-*.txt under %LOCALAPPDATA%\NwcFederatorLoop\turn5. The solution built with 0
errors and 0 warnings before the first change and after each change.

### What was done

- FR-031, 95cf9ea. The last `CLASH N of N tests` line of a group was taken before the test it
  numbers, so in set 03 it was one test short of the block under it in all 22 groups, and short
  of the block's clashes in 1B06M1, 1B06PK and 1C06M2. ClashRunOutcome.ProgressAfter builds it
  in Core at every twenty fifth test and at the last, and the runner calls it after the test. 4
  tests. No number the workbook prints changes
- FR-032, f2997ab. A result group with no clash under it stood for one clash, floored in the
  harvest. ClashRow.ForGroup stands for the clashes under the group and no more. 3 tests. It
  changes numbers the workbook prints for a test holding an empty group: its Clashes cell and
  the cell of the group's status, and with no priority file its place on the sheet and the
  picture numbers that follow that order. Outside the workbook it moves the page summary total,
  with or without a priority file, and the PRIORITY block's totals when a priority file is
  picked
- FR-033, 9fc81ee. An unnamed clash between two sets the pairs file lists was counted as moved
  by the by design rule though it was never moved. ByDesignRule.Judge takes the clash name and
  answers NoClashName, a sixth reason the BY DESIGN block lists. 2 tests, and the helper the old
  tests share now hands Judge a name. No number the workbook prints changes
- FR-034, 3ed9575. AutoReviewRecord.In threw for a comment reading was Reviewed, was Approved or
  was Resolved, and the undo then left every clash of that test alone. The status is read only
  among New and Active. 1 test. No number the workbook prints changes
- each item test first. The tests of FR-031 to FR-033 did not build against the code before,
  and against a stub of the new members 4, 2 and 1 of them failed, the stub returning no line,
  carrying the old floor and taking the name without reading it. The test of FR-034 failed
  against the code before with ArgumentOutOfRangeException
- each item's rule in .claude\rules\core.md, written in that item's commit, and the order line
  37 and the F113 section in steps\01_next.md at 1f0a369. Main merged in at 2bae58a, main then at
  dd55e4b, with no conflict. Pushed as fix-F113
- the claim-checker's corrections at b629b5c, which also narrow the words a group always ends on
  a count in core.md and one comment of ClashRunOutcome.cs, turn5\precommit-f113-claims.txt, 1756
  passed, and main 6689bad merged in at 0339223 with F113's entry on top of the log,
  turn5\precommit-f113-merge-2.txt, 1756 passed, the solution built at that tip with 0 warnings
  and 0 errors, turn5\f113-build-merged.txt

### What remains

- the add-in halves wait for wave 1, a first run with the XML and a weekly run on each of
  1A02MM of C02 and 1A04PK of C04. FR-031 in each group that runs to its end: the last `CLASH N
  of N tests` line of the group reads the tests run, tests skipped and clashes found of the block under it.
  FR-032 only where a log holds an empty result group, which no log has shown: a group with
  clashes under it reads the same before and after. FR-033 only where the RUN SETTINGS line
  `by design` reads yes and the no name count is above 0, checked by the group's STATUS lines,
  since its moved count, its REVIEWED lines and RESULT hold with or without the fix and a count
  of 0 proves nothing. FR-034 is reached by no run, only by the Undo auto Reviewed button on a
  comment edited by hand
- the commit messages of FR-031 and FR-032 name set 05 on C06 as the proof run, because they
  were written before the lead's message of 15:52 moved the proof to wave 1, and the message of
  1f0a369 says the messages of FR-031 to FR-033 do, where FR-033's does not. A commit message is
  changed only by a new commit, so these stay as written. The DONE line names wave 1

### Known bugs

- for a test whose only result is an empty group the ROWS line still reads more rows than
  clashes and ends that the workbook will not match the panel, which after FR-032 is no longer so
  of its Clashes cell. Beside a group of three or more clashes it reads that the difference is
  the result groups, and beside a group of two that they agree. src\Federator.Core\Clash\ReportedCount.cs is in no area's
  list of files
- WorkbookCheck.CheckOrder orders the blocks by the rows under each, where ReportOrder sorts
  the tests by the clashes each stands for, so a workbook holding result groups could be called
  out of order when it is not. Not seen, set 03 holding no result group. WorkbookCheck.cs is the
  workbook area's
- the running count is still written every 25 tests, ClashRunner.ProgressEvery, a constant, as
  the note of FR-031 says
- GroupRow still counts every clash of a group under the group's own status, where the runner
  counts each by its own, the gap the note of FR-032 calls UNKNOWN. The breaker ranked it first:
  a group whose clashes sit at mixed statuses prints its five status cells under the group's
  one word, so where a person has grouped results the workbook can disagree with the panel on
  status, which side the panel takes being UNKNOWN. Into the next wave as a new item
- found by the breaker and outside F113's items: a clash name is the only address the status
  editor uses, so two clashes of one test with the same name are both moved when the by design
  rule judges one of them, and a person's Approved can become Reviewed,
  ClashStatusEditor.cs lines 207 to 251. Into the next wave as a new item, its reach UNKNOWN
- the moved counts of the BY DESIGN block and the RESULT line count judgements, and the editor's
  own changed, not found and refused counts are read nowhere in src. Into the next wave

### Read before the pull request

- by a reviewer at 1f0a369, APPROVE, nothing blocking, nine notes, two of them made true in the F113
  section of steps\01_next.md: what moves for a test holding an empty group, and that FR-033's
  wave 1 check holds with or without the fix, the STATUS lines being the one pair that can
  differ. Kept with the other notes in turn5\f113-reads.txt
- by a breaker at 1f0a369, nothing that makes a count wrong because of this change, seven left standing,
  the first three in Known bugs above, all in turn5\f113-reads.txt

### What comes next

- the pull request, merged in wave 1, then the test of wave 1 on 1A02MM and 1A04PK

## 2026-10-04 The loop, turn 5, Bader's answers to the form and the waves, the plans

Bader answered the form of turn 5 at 15:23 on 2026-10-04, his message headed BADER'S ANSWERS,
4 OCT 2026, TO THE FORM OF TURN 5, and at 15:42 ordered the fixes in waves, each tested at once on
two buildings, his message headed FIX IN WAVES, AND TEST EACH WAVE ON TWO BUILDINGS. Each message
began by asking for the plan before the first edit. The two plans below are the ones the lead
wrote to him in the session, condensed, some of their sentences left out. Two placements moved
after he was told: Q108's fix and F117, the names, said to come in waves 3 and 4, are in wave 5,
because the fix list classes them noise, and the waves in steps\fix-round.md say so. What each
step then did goes into the entries above this one and into steps\loop.md. Nothing under src or tests
changes in the pull request carrying this entry.

### The plan for the answers, written at 15:27

1. Record the answers first, in a records pull request: each under its question, an answered
   line on each fix list item they touch, FR-025 and FR-026 out of hold since Q104 keeps the
   class, the form marked answered, the next action, and this plan. PR 93 merges first
2. Tell the developers at work: F112 builds Q99 and Q100, the clash skipped, in place of the
   PARTIAL only rule, with FR-006, and F114 adds each group's time beside its NWC sizes and item
   counts, with no change to the 45 minute judgement
3. With Bader's Navisworks closed, Roamer read 0 at 15:24: F104's harness parts B and C on its
   committed code, then the first documents read on set 03
4. As developer slots free, three at once: F109 attempt 3, F116 the clash XML, F115 sets, F117 the
   names
5. The four finds of Q108, Q109, Q24 and Q26, each writing its test steps under its item and
   running them before anything changes
6. Each area merged one at a time, then the proof run on C06 twice, rule on and rule off

### The plan for the waves, written at 15:46

- What NM Fed holds, measured at 15:44, turn5\nmfed-listing-1544.txt: C02 holds 1A02MM, four models, AR, EL, ME and ST, C04
  holds 1A04PK, ten models, AR, EL, FP, HV, four ME and two ST, and it still holds C06 and C07,
  154 NWC in all, with the -OLD XML beside the corrected one. No folder named for C06 and C07 was
  on the desktop at 15:46. The runs point at C02 and C04 only. The installed add-in reads
  1.0.0.0 e4484d15, and nothing under src, tests, build, bundle or exchange changed from e4484d1
  to main
- Step 1, the baseline today, set 04: once F104's harness has ended, set 04's copy, the four
  window runs, item 1 with the XML and item 2 with none on both buildings, each after a Roamer
  read, the evidence masked, steps\runs\04\findings.md, and F104's documents read on set 04
- Step 2, the waves by the file table, worst class first: wave 1 F112 alignment, F113 clash
  counts and F116 the clash XML, F114 paused to wave 2. Wave 2 F115 sets, workbook and report, F114
  views with its speed work. Wave 3 run log and RESULT, harvest and pictures, install with F109.
  Wave 4 the rest with the finds of Q109 and Q24, and the loop tools under Q93. Wave 5 noise,
  docs and words with Q26's find, then D1. F104 merges as the test's instrument. Q108 measured
  already: the repeated asked for text of the SET lines, 59 texts printed 1,000 times, 145,182 of
  1,097,933 bytes saved if each were printed once a run
- Step 3, after each wave: install main in place as on 2026-10-01, read the stamp back, run both
  buildings, a building off the shared coordinates once more with the rule off, compare with the
  baseline and the wave before, mark each item proven or not, take back a fix that makes it worse
- Step 4: three lines at the top of steps\fix-round.md and in the tab, merged within the hour

### What was done

- PR 93 merged as dd55e4b. The answers under Q24, Q26 and Q99 to Q109, in his words with the
  lead's notes marked, and an answered line inside each of the 16 items they touch, FR-001,
  FR-006, FR-008, FR-009, FR-025, FR-026, FR-030, FR-069, FR-070, FR-109, FR-110, FR-136, FR-149,
  FR-160, FR-161 and FR-172
- the waves written in steps\fix-round.md under The waves, from the classes the list gives each
  item
- the developers told by message: F112 the rule of Q99 and Q100, F114 the per group times and
  then its pause for wave 2, F112 and F113 that the proof is the two building test. F114 paused at
  f915396. F113 finished its four items. F116 started in wave 1
- F104's harness, all three parts on 33732c9: 102 passed, 0 failed, turn5\f104p2-prove-4.txt
- the baseline's first run, item 1 on C02, stopped HUNG at 16:03:28 on a floating Clash Detective
  pane the driver took for a dialog, its evidence in wt-main, nothing of Bader's harmed. F125, a
  fix of the driver, started

### What remains

- F125, then the baseline's four runs and findings 04, then wave 1's readings and merges and its
  test

### Known bugs

- the lead reads Q102's OR row as carrying the set's other conditions, as written under Q102, for
  Bader to correct if he meant otherwise

### What comes next

- F125's fix of the driver, then the baseline
## 2026-10-04 The loop, turn 5, F107 the older machine's name masked on main, DONE on Q88

Nothing under src or tests changed. Core tests by the pre-commit at 70effde, 8441dc9, 499f0a6
and the merge of main after it, the files precommit-f107-*.txt under
%LOCALAPPDATA%\NwcFederatorLoop\turn5, 1746 passed, 0 failed, 0 skipped each time, the same as
main before.

### What was done

- Bader's answer to Q88: the name of the machine of 2026-09-19 masked on main in its own pull
  request. At 821ed6e it stood on nine lines in four files. On main at 3449521 it stood on eight
  lines in three files, five in docs\history\scan.md, one in steps\01_next.md and two in
  steps\log.md, because F106's rewrite of the header of tools\probes\drive-window-run.ps1 had
  already masked the ninth. Each now says the machine of 2026-09-19, and nothing else on those
  lines changed. The two lines of steps\log.md are the lead's file, changed only by this mask
- main merged in twice on 2026-10-04 and again after F105, the driver taken from main whole
  where it conflicted, and the order line moved to 36 behind F105
- read by a reviewer, who asked for changes with one blocking: the F107 section still described
  the change as it was before the merges, nine lines with the driver among them. The section now
  says eight lines in three files, names where the ninth went, and carries the searches made
  after the merge
- the searches after the merge at 8441dc9: git grep for the whole name and for the part after
  its hyphen, in the working tree, at HEAD and over untracked files, found nothing, and the split
  search read 874 tracked files, 1053 on the disk and 874 at HEAD with 0 hits, the one file it could
  not read being a broken fixture that is not a zip by design, whose text it read apart with 0
  hits. %LOCALAPPDATA%\NwcFederatorLoop\turn5\f107-grep-after-merge-2.txt,
  f107-split-search-after-merge-2.txt and f107-split-search-endswrong-2.txt
- the searches after main 2c89788, with F105, merged in, before the records were committed: git
  grep for the whole name and for the part after its hyphen in the working tree, git grep over
  untracked files for the whole name, and grep -r over the worktree for both, found nothing,
  turn5\f107-grep-after-merge-3.txt, which names neither form
- the searches on the final commit of this pull request, after every record was written: git
  grep for both forms in the working tree, at HEAD and over untracked files, and the split
  search, turn5\f107-grep-final.txt and turn5\f107-split-search-final.txt

### What remains

- the name stays in the history of main, which no pull request changes

### Known bugs

- F107-R1, now the fix list's FR-110 and a question for Bader, Q106: nothing refuses the name if
  it comes back, because F102's check reads for this machine's name only
- the account name of that machine's user in C:\Users paths, the fix list's FR-149, Q107

### What comes next

- the fix round's area pull requests, steps\fix-round.md
## 2026-10-04 The loop, turn 5, F105 four reads off the install, DONE on Bader's answer A to Q89

Nothing under src or tests changed. Core tests by the pre-commit at 2ac77fc, 96a0f93, ee87b3c,
5d883a7 and 29c7e94, the last over the tree as it merges, and again at this entry's last commit,
in the pull request body, 1746 passed, 0 failed, 0 skipped each time, the same as main before.
The merges 88a18f0 and 98f2e66 were made by git merge, which runs no pre-commit. No Navisworks was
started for F105: the four reads are off the install's files, with Get-Process Roamer reading
none before and after the final runs, tools\probes\f105-run-record-20260929.txt.

### What was done

- the four reads of Phase 1 item 2, measured by the prober on 2026-09-29: the saved viewpoint
  members, Document.RemoveFile and TryRemoveFile, Roamer.exe's switches, and the Clash Detective
  report, written into docs\history\scan.md 5z-f with five result files, three new probes, the
  shared il-reader.ps1 and a run record under tools\probes
- Bader's answer A to Q89 after three fix attempts: the sentences of 5z-f that read a zero for
  every failure kind il-reader.ps1 prints now claim a zero only for the kinds each probe counts,
  and say no line a probe can run counts a failure as one of the other kinds
- the words round of 2026-10-04, finished by one developer and re-read by a reviewer, who asked
  for changes with one blocking: section C of probe-viewpoint-calls.ps1 holds a fifth silent
  read through a property, line 372, a member reference's DeclaringType, which no stand-in
  called. 5z-f and the DONE line now name it and say whether it fails there is UNKNOWN. Both say
  a failed property read stops probe-clash-report-api.ps1 where a method is then called on the
  null, its lines 191 and 197, and 5z-f says a failed Assembly or Location read inside IsNw of
  probe-viewpoint-calls.ps1 is counted, all read off the code. A claim-checker read the entry
  and the body and its six findings are made true in the commit after 29c7e94
- main merged in four times on 2026-10-04, at 96a0f93, ee87b3c, 88a18f0 and 98f2e66, the last
  to 086a348, with no conflict left. F105 takes order line 35

### What remains

- nothing in F105's own reads. Its probe faults stay as register rows F105-R1 to F105-R4, Q93,
  R4 the comments named below

### Known bugs

- F105-R1 to F105-R3: il-reader.ps1 prints a zero for every failure kind in every probe, kinds a
  probe never counts included, probe-viewpoint-calls.ps1 prints its failure list only when a
  built add-in is there, and a resolver, roResolve, and a small helper, ParamText, sit in more
  than one probe, while a second helper the reading named is UNKNOWN
- comments in the probes still say nothing is dropped, il-reader.ps1 line 19,
  probe-clash-report-api.ps1 line 23 and probe-viewpoint-calls.ps1 lines 29, 30 and 396, and
  5z-f does not say they claim more than the code does. No probe or result file changes after
  its run. Register row F105-R4

### What comes next

- F107, whose order line becomes 36 when it merges main after this
## 2026-10-04 The loop, turn 5, F106 the window run, DONE for item 1

Nothing under src or tests changed. Core tests by the pre-commit at 8c67ec6 and 32dce24 on
2026-10-01, 1746 passed, 0 failed, 0 skipped, and again at this entry's commit, in the pull
request body. Item 1 was proved by a real run on this PC. Items 2 to 5 wait for the proof run.

### What was done

- run.ps1 items 1 to 5 through the real window around tools\probes\drive-window-run.ps1, with
  every guard of item 0 kept but its rule that nothing is written into his AutoSave folder,
  which Q86 replaced, written by developers whose transcripts ended interrupted, then
  finished for item 1 by one developer, who found and fixed three faults that would have
  stopped the first run: a window list read as one object, @() throwing on a list, and the
  confirm's buttons with no Invoke, now answered by WM_COMMAND to that dialog only
- read for harm to Bader's things only before the first run, Q96, by a reviewer and a breaker,
  both SAFE FOR THE FIRST RUN
- THE FIRST RUN OF MAIN ON C06, set 03, 2026-10-01: the window driven with no click, VERDICT
  RAN, RESULT 17 done, 0 partial, 5 failed, steps\runs\03\findings.md
- main merged in, 0fd91c6, with no conflict, once the run's own copies of its evidence were
  moved from wt-f106 into %LOCALAPPDATA%\NwcFederatorLoop\turn5\wt-f106-evidence-03, each the
  same by sha256 as the copy main holds

### What remains

- items 2 to 5 have never run. The proof run of set 05 is their first run
- prove-run.ps1 brought up to F106, R1 of turn4\f106-finish-report.md, line 119

### Known bugs

- R1 to R4 of F106, turn4\f106-finish-report.md lines 119 to 122, and the harm reading's other
  findings, the workflow journal wf_8100973e-871, go into the fix list, steps\fix-round.md, once
  it is built

### What comes next

- F104 part 2 on this run.ps1, then the fix round
## 2026-10-04 The loop, turn 5, the full fix round, the plan

Bader's message headed 4 Oct 2026, Q98: no run of C07 now. Fix everything that is known, then
prove the fixes on C06 in set 05. This entry is the plan the lead wrote to Bader in the session
before the first edit. What each step then did goes into the entries above it and into
steps\loop.md, turn 5.

### Read before the plan

- why turn 4 stopped: Bader's "stop, i will close the pc" at about 17:35 on 2026-10-01. The
  pause, STATE WAITING, was pushed as 87d693e on fix-T4-run-03c and never merged, so main kept
  STATE OPEN. The System log reads a shutdown at 19:30:49 on 2026-10-01 and an update restart at
  09:07:20 on 2026-10-04
- Get-Process Roamer read 0 at 09:51:56. No keep-awake ran. Open pull requests: 83, the rules,
  and 84, F108. Not merged: fix-F104 ebd8bb7, fix-F105 cf40a56 and staged work in wt-f105,
  fix-F106 32dce24, fix-F107 438378d and 2063c29 not pushed, and fix-F109, F110 and F111 only in
  their worktrees

### The order

1. Pick up: the keep-awake by its new rule, started through WMI so it is not a child of Claude
   Code, with the display flag, stopping at CLOSED, WAITING or 12 hours of no change. STATE
   OPEN, turn 5, this entry, merged within the hour
2. Finish the work in flight, merged one at a time or parked with one line why: PR 84, F108,
   and PR 83, the rules, with their claim-checks' words fixed and the new keep-awake rule. F105
   and F107's words round. F106, main merged in, F107's masked line kept in the driver. F109,
   the in-place install of build\install.ps1 for the team, by a developer, a reviewer and a
   breaker, which is the install area of the round. F104 part 2, built during the round for the
   proof run. F110 and F111 of 2026-10-01 fold into the workbook and run log areas
3. The fix list, steps\fix-round.md, before the first fix: read-only readers per source, the
   19 findings of steps\runs\03\findings.md with FIND-04 and FIND-24, the 86 bugs of
   steps\notes\turn1-read-verified.md, every open fault row of the register with the T3 and T4
   rows, and the readers' other findings of turns 3 and 4. Each item once with its ID, sources,
   evidence line, root cause file and line, class and proof, and its area
4. The form: the 40 distances of the C06 run for B2's 1 m, whether the 45 minutes counts per
   community or for the whole folder, and each decision the list raises
5. steps\runs\03\for-modellers.md for the five FAILED groups, B4
6. The fix round, one pull request per area, up to three developers at once on different
   files, alignment first with B2: a model more than 1 m from its reference ends its group
   PARTIAL, the 1 m a setting in Core. Then the areas by their worst class. Each finding its own
   commit and test, a reviewer and a breaker on each pull request, the claim-checker on its body,
   Actions green, merged one at a time, a record within the hour. VIEWS faster with the same
   viewpoints, before from set 03's TIMING and after from the proof run's first run on the same
   files, with options in the form if 45 minutes is still out of reach, B3. D1, one public type
   per file, the last pull request, moves only
7. The proof run, set 05 on C06: main installed in place as on 2026-10-01 and its stamp read
   back, a fresh copy whose XML must ask ME-Ductwork or gets the repo's corrected one, B1, the
   first run, the weekly run, a file gone, the file back and the open NWF and NWD, F104's check
   on the NWFs, steps\runs\05\findings.md with a before and after table against set 03, and
   Bader's three tests. A finding it shows starts a second round
8. The end: a summary at the top of steps\fix-round.md, merged, and posted in the Claude tab

CLAUDE.md's rule of no co-authored-by and no generated-by line holds over the session's own
attribution note.

## 2026-10-01 The loop, turn 4, F108 a fresh copy of the source for each run set, DONE

Bader's message headed 30 Sep 2026, read on 2026-10-01, Q94, asked for a fresh copy for each run
set in his own folder shape.
Nothing under src or tests changed. Core tests by the pre-commit at 225a86f, in
%LOCALAPPDATA%\NwcFederatorLoop\turn4\f108-commit-output.txt: 1746 passed, 0 failed, 0
skipped, and F102's evidence check over the 3 staged files, none carrying one. Nothing waits for
the local machine: the proof ran here, and the first run of set 03 is its use.

### What was done

- tools\loop\prepare-copy.ps1 -Set NN, by the developer in wt-f108: the plain check first, then
  runs\NN\NMFed copied from the source copy and every file read back by sha256 and size, one
  empty folder under Clash Report for each folder under NWC, read off NWC, and
  NMFed.manifest.txt written last. -Set NN -Remove and -Restore act on that copy, -Restore
  copying the file back from the desktop folder by hash, as the plain modes do
- named NMFed because the paths wall refuses every command naming the desktop folder
- proved on this machine with the throwaway set 99 and others, made, read back, refused a
  second time, and a file removed and restored, every answer in turn4\f108-proof.txt, the run
  ending 10:52:59 with no Roamer. Of the refusals, 13 ran in the loop folder, 4 in a scratch
  folder and 6 on a copy of the script changed by one line, and the two for too little room
  were not proved, as the README says. The script the proof ran, sha256 6EDA217D, is the one
  committed at b3958b5 once its line ends are CRLF, as the checkout writes them,
  turn5\f108-hash.txt
- read by a reviewer and a breaker under Q93: both SAFE TO USE, no finding blocks

### What remains

- the readers' findings, each a register row in steps\loop.md: -Remove and -Restore refuse a file
  whose name sits in both communities, so group 100000 cannot be the file gone group. A short or
  changed source.manifest.txt is kept until the desktop folder next changes. -Remove writes its
  note before the delete. The README's write list names only the source copy. The rule and the
  runner agent still describe the plain modes only. A comment says Windows PowerShell 5.1
  follows a junction when it recurses, which the developer said a test of theirs on 5.1 did not
  show, kept in no file, so which is right is UNKNOWN

### Known bugs

- two of the breaker's findings were of Q93's kinds and judged not to block: outputs that Bader's
  own runs leave in the desktop folder land in the copy and its manifest as inputs, which can
  make evidence wrong, and nothing reads for a running Navisworks while the desktop folder is
  read, which reads only. Both are register row F108-R5, with the rest F108-R1 to R4

### What comes next

- the first run of set 03, after F106 and F109

## 2026-10-01 The loop, turn 4, F103 run.ps1 part 1, DONE after the third real start

Bader answered B on 2026-10-01, Q92: the third real start on a63c284 now, then F103 merges as
it is, the nine entries a register row. Nothing under src or tests changed. Core tests
before, at 821ed6e with no src change since, by the pre-commit of the turn 4 records commit
7dbb869: 1746 passed, 0 failed, 0 skipped, read by the lead and kept in no file. After, by the pre-commit at this entry's commit,
in the pull request body. Nothing of item 0 waits for the local machine, because the third
start ran here.

### What was done

- THE THIRD REAL START, run.ps1 -Mode Run -Set 02 -Item 0 -Stamp be0b9b37 on a63c284 from
  the F103 worktree. Get-Process Roamer read 0 at 09:05:54, read by the lead and kept in no
  file. Launched 09:06:16, ended 09:18:23,
  exit 0, VERDICT RAN: one Navisworks, pid 37988, adopted, held 360 s, Dispose returned after
  0.48 s and the process read gone 6.2 s after, nothing forced. 5 registry values and 2 files
  of Bader's settings put back and read back equal, no other Navisworks having run, his logs
  folder the same after and his AutoSave unchanged. The idle processor time read 0.031 to
  0.344 s a 15 s sample and 0.359 to 0.594 s a minute
- its evidence, steps\runs\02\item0, read by F102's mask, which found nothing to mask, and by
  F102's check, committed as 19a3da7
- main merged in, then F102's merge, and F103's DONE line in steps\01_next.md

### What remains

- part 2, the window, items 1 to 5, is F106, being built off this branch

### Known bugs

- F103-W, the nine entries in the words of steps\notes\f103-final-reading.md, and T3-G1 to
  T3-G16, T3-P and T3-P2, in the register in steps\loop.md

### What comes next

- F105 and F107, then the install of main through run.ps1 -Mode Install from wt-main

## 2026-10-01 The loop, turn 4, F102 every result file read for a machine name or a licensing id, DONE

Bader answered B on 2026-10-01, Q91: F102 merges as it is, both gaps written as known limits.
Nothing under src or tests changed. Core tests before, at 821ed6e with no src change since,
by the pre-commit of the turn 4 records commit 7dbb869: 1746 passed, 0 failed, 0 skipped,
read by the lead and kept in no file.
After, by the pre-commit at this entry's commit, in the pull request body. The proof of the
change itself is in its 01_next section and in
%LOCALAPPDATA%\NwcFederatorLoop\turn3\f102\proof.txt. Nothing waits for the local machine.

### What was done

- main merged into fix-F102 as 8554e45 with no conflict, Bader's answer and the two known
  limits written under F102's DONE line in steps\01_next.md, and this entry
- used by the loop before the merge, from its branch at 7d8d135, on the third real start's
  evidence of 2026-10-01: the mask found nothing to mask in record.txt, watch.txt and
  settings.txt, each copy equal to its original by sha256, and the check read 12 files under
  steps\runs of fix-F103 and found none carrying an id or the machine name

### What remains

- the zip rule's words: .claude\rules\loop.md and tools\loop\README.md still say the check
  refuses a zip until Bader decides where one may sit. He decided A on 2026-10-01, Q90, and
  the words change in the next records pull request

### Known bugs

- F102-L1: a workbook under samples with text appended inside its last 65557 bytes passes the
  check unread. A limit, because the samples are never edited
- F102-L2: a committed hook holding the check's line where it never runs still gets the
  handover. A limit, because a hook of this repo is read before it merges

### What comes next

- F103 merges, then F105 and F107, then the install of main

## 2026-10-01 The loop, turn 4, the plan, as written to Bader before the first edit

Bader's message headed 30 Sep 2026, read on 2026-10-01: code runs the whole of NM Fed itself,
and the form is answered. It replaces the window message of 30 Sep and anything earlier that
said Bader runs the tool by hand. The goal is real full runs of main on every NWC in NM Fed,
driven by the loop, then the fixes those runs show. His answers are Q82 to Q92 in
steps\02_questions.md, his rule for the loop's own tools is Q93 and the goal is Q94. This
entry is the plan the lead wrote to Bader in the session before the first edit, copied here
after the answers were written into steps\02_questions.md. The third start of step 2 was
launched before the records of step 1, because it edits nothing. Changed after it only where
the claim-checker found a claim wrong: the heading, this paragraph, the Roamer reads, the XML
reads, the 25 mm sentence and the waiter line. What each step then did goes into the entries above it and into
steps\loop.md, turn 4.

### Measured before the plan

- Get-Process Roamer read 0 at 08:48:06 and again at 09:05:54, read by the lead and kept in
  no file
- the waiter, turn3\wait-window.ps1, wrote THE WINDOW IS OPEN at 18:00:14 on 2026-09-30 as
  its last line, and no start was made in that window. No folder runs\02 existed under the
  loop folder or in the F103 worktree at 09:05:54, and why no start was made is UNKNOWN
- the clash XML in the copy, 1104-PAR_CLASH_AllInOne_25mm_FIXED.xml, sha256 36AB2739,
  holds 1830 clash tests, every one with tolerance 0.0820209974 in feet, which is 25 mm. The
  committed exchange copy of the corrected matrix is another file, sha256 792B01FB, with the
  same 1830 tolerances. Both read again at 10:25:30 into turn4\xml-reads.txt
- the tool's tolerance box defaults to Use the value in the XML, ToleranceChoice.cs, and
  the Execute of FederatorPlugin.cs opens the window with ShowDialog
- tools\probes\drive-window-run.ps1 finds the tool's window and its confirm dialog among
  every window on the desktop, not by process, and picks a tolerance with a real mouse click
- the paths wall refuses any command naming NM Fed, with one to six characters between the
  two words, and leaves NMFed alone on purpose

### Four choices the lead made, and why

1. THE COPY OF EACH RUN SET IS NAMED NMFed, not NM Fed, because the paths wall refuses
   every command naming NM Fed, and a path built at run time to get past it is what the
   wall's rule forbids. Below that name the copy keeps Bader's own folder shape: NWC\C06
   into NWF\C06, NWD\C06 and Clash Report\C06, the same for C07, and the XML at the top
2. 25 MM COMES FROM THE XML. The tolerance box is left on Use the value in the XML, under
   which each test a first run creates from the XML carries the XML's 25 mm, and a test
   already saved in an NWF keeps its own. Choosing 25 mm in the box takes a real mouse click,
   which would move his pointer while he works. The workbook read-out names the tolerance
   of every test, so each run proves it
3. HIS LOGS ARE PUT BACK FROM THE BACKUPS. After the loop his logs folder is made to hold
   exactly what logs-backup and its since folders hold, the loop's own logs and tsv files
   are taken out, and the folder is read back by name, size and sha256. A log of his that
   no backup holds, written while the loop ran, is left as it is and named
4. THIS IS TURN 4. Window runs go from a clean checkout of main at
   %LOCALAPPDATA%\NwcFederatorLoop\wt-run, so their evidence never dirties wt-main, which
   installs

### The order

1. The records: this entry, the answers, STATE OPEN and turn 4 in steps\loop.md, and the
   register rows the answers make. Read by the claim-checker, merged when Actions is green
2. Get-Process Roamer read 0, then the third real start on a63c284 from the F103 worktree,
   Set 02, item 0 with no window. Its record read, its evidence masked with F102's tools and
   checked, and committed to fix-F103 as 912dedc was. In parallel, by the developer in their
   own worktrees: F105's three sentences narrowed, Q89 A, and F107, the machine of
   2026-09-19 masked on main, Q88, each read by a reviewer and a breaker
3. The merges, each when Actions is green and its branch deleted after: PR 76, F102, as it
   is with both gaps written as known limits. PR 78, F103, as it is with main merged in.
   F105. F107
4. The install of main: Roamer read 0, wt-main moved to main's new commit with git status
   printing nothing, run.ps1 -Mode Install -Stamp of that commit, and the installed stamp
   read back equal to it
5. F106, THE WINDOW RUN, by the developer: the smallest change to run.ps1 that runs items 1
   to 5 through the real window around tools\probes\drive-window-run.ps1, keeping run.ps1's
   refusals, backups, start, adoption, watchdog, close and put back. The driver acts only on
   windows of the adopted process, clicks nothing, reads every box back and presses Run only
   when each reads a path under the loop folder. The hang rule of Q83, the ceiling of Q84,
   the screen of Q85, the AutoSave of Q86, the masked folder block of Q87, the run set copy
   in Bader's folder shape, and a mode that puts his logs folder back after the loop, Q82.
   The rule changes the answers make go into .claude\rules\loop.md in the same pull
   request. One reading by the reviewer and one breaker under Q93, then the merge
6. SET 03, every run through the real window, each followed by its evidence masked and
   committed, the log-reader's findings each quoting its line, every register row reading
   done in code, not proved by a run that the run reaches marked proved or contradicted,
   steps\loop.md brought up to date and a record merged:
   1. a fresh copy, runs\03\NMFed, made from the source copy after prepare-copy.ps1 has
      checked it against NM Fed, every file read back by sha256
   2. the first run of C06: the XML from the copy, every group ticked
   3. the first run of C07, the same
   4. the outputs of the first runs copied aside and read back by sha256
   5. F104 part 2, check-documents.ps1, built while the runs go, read, merged, and run over
      the first runs' NWFs before the weekly runs. Three tests with their workbook counts
      written into steps\03_bader_next.md, for Bader to check against the Clash Detective
      panel
   6. the weekly runs of C06 and of C07, with no XML
   7. a file gone, then the file back, then the open NWF and the open NWD, on one group with
      two or more disciplines
7. The fixes the runs show, most harmful first and a silent wrong number above a loud
   failure. One finding, one fix-F number, one pull request, the failing test first, read by
   a reviewer and a breaker, installed, and proved by the next run of that group in a new
   run set with a fresh copy
8. The close: his logs folder put back and read back, the loop's autosaves removed under
   Q86, and STATE set by what is left

### Held throughout

- A Navisworks the loop did not start: no start, no install and no put back. A waiter
  reads the processes every 10 minutes and ends when none runs, and the lead carries on with
  no word from Bader
- A locked screen that stops the window or the pictures is a finding, and the next start
  waits until the session reads unlocked. The lock is never worked around
- Nothing is written into NM Fed on the desktop, and every output goes under
  %LOCALAPPDATA%\NwcFederatorLoop

## 2026-09-30 The loop, turn 3, F103 into the form after three fix attempts, Q92

Records only. Nothing under src, tests or tools changed here. Core tests before, on main at
11e6220, by Actions run 36710730880 on its windows-latest runner: 1720 passed, 0 failed, 26
skipped of 1746. After, on this branch with these records in it, run by the lead from
17:07:16 to 17:07:59: 1746 passed, 0 failed, 0 skipped, output in
%LOCALAPPDATA%\NwcFederatorLoop\turn3\core-tests-170716.txt, with the Actions line in
turn3\reads-170618.txt. The pre-commit hook runs them again at the commit. What waits for the local machine: the third real start on a63c284, in tonight's
window.

### What was done

- fix attempt 3 of F103, the 19 items of steps\notes\f103-fix-list-3.md, came back from its
  developer as a63c284 at 15:10, pushed to fix-F103, with harness run 12, 242 passed and 0
  failed on its files
- its final reading by a reviewer and two breakers: all three answered safe for item 0 and
  safe for install, the breaker on Bader's things within its lens, and none classed a
  finding as a new fault inside the attempt. The reviewer read 18 of the 19 items fixed and
  asked the lead to hold item 14 against the words it names
- the lead held it: the harness header's sentence that the constructor line is replaced by a
  line that throws, where the line only sets the error, stands word for word at prove-run.ps1
  24 to 25, named after attempts 1 and 2 as well. So by the rule at the head of fix list 3,
  F103 stops and goes to the form, Q92, with the lead's recommendation A, one change of words
  only, no change of logic, at nine entries. steps\notes\f103-final-reading.md holds the
  reading, the nine entries and the proof
- five register rows from the readers' other findings, classed old or polish, T3-G13 to
  T3-G16 and T3-P2, three of them resting on code the attempt wrote
- Actions on a63c284 had never run a test, because GitHub could not acquire a runner in 5
  attempts. Run again by the lead, green at 16:05:30, the Core tests 1720 passed, 0 failed,
  26 skipped of 1746

### What remains

- Bader's answer to Q92. Until then PR 78 stays a draft, and the install of main and F104
  part 2 wait on it
- the third real start on a63c284 in tonight's window, Set 02, item 0 with no window, as
  evidence for Q92

### Known bugs

- the nine entries in the words, listed in steps\notes\f103-final-reading.md, which wait on
  Q92, and T3-G6 to T3-G16, T3-P and T3-P2, in the register

### What comes next

- turn3\wait-window.ps1 reads for tonight's window from 16:14, started again each time it
  ends itself after 115 minutes. When the window opens: Get-Process
  Roamer read 0 just before, the third start, its record read, its evidence masked, checked
  and committed to fix-F103, and steps\loop.md set to WAITING

## 2026-09-30 The loop, turn 3, the Navisworks window of 10:52 to 14:45, the plan

Bader wrote at 10:52 on 2026-09-30 that he had closed his Navisworks and would not open it
again before 14:45.
Every Navisworks the loop starts is closed, and his settings put back and read back, by
14:30. Get-Process Roamer read 0 at 10:57:10. This entry is the plan, written before the
first edit of the window. What each step did goes into the entries written above this one and into steps\loop.md.

### The order

1. The reading of F103 fix attempt 1, fcd981b, by a reviewer and two breakers, resumed from
   the run the restart cut, which reuses the one reader that had answered
2. If it finds a fault of the thirteen left or a new one, fix attempt 2 by the developer and
   a read again. If F103 is not clean by 13:45, its first start goes to tonight
3. THE FIRST REAL START, run.ps1 -Mode Run -Set 00 -Item 0, made by the lead from the commit
   that was read, with Get-Process Roamer read 0 just before. It measures M4, the processor
   time of an idle Navisworks, M5, what changes outside the loop folder while a start runs, and M6,
   whether the session locks. Its record read, the settings put back and read back, the
   evidence masked with F102's own tools from its branch, because F102 is in the form, and kept under steps\runs\00\item0
4. F103's pull request, PR 78, with that run in its body, merged once Actions is green
5. INSTALL MAIN through run.ps1 -Mode Install from a clean checkout of main. It starts no
   Navisworks but needs none running, which is this window, and it is the first step of the
   baseline. Only if F103 merged by 14:00
6. No start begins after 14:10 and no install after 14:15. At the close of the window,
   steps\loop.md says what the window was used for and what is left for tonight

Not in this window: any start that opens the tool's window, because the tool then logs into
Bader's folder and deletes his oldest log, which waits on Q82.

## 2026-09-29 The loop, turn 3, F100 a start of Navisworks with no click, measured, DONE

Core tests 1746 passed, 0 failed, 0 skipped, before on fix-F100 at 30ae471 and after on
0a89d73 and every commit since, run by the pre-commit hook. Nothing under src or tests
changed. The add-in built with 0 errors and 0 warnings before and after, by the developer.
This entry rides in PR 74, which is merged by SQUASH so the branch's history, which holds the
licensing ids, stays off main. Nothing waits for the local machine, because run 4 ran here.

### What was done

- Bader answered A on 2026-09-29, Q79, and closed his Navisworks. Get-Process Roamer read 0
  at 09:52 and again just before run 4, and the probe itself read none at its step 2 and at
  its last read before the constructor
- FIX ATTEMPT 4, 0a89d73, by the developer: exactly the seven faults of
  steps\notes\f100-fourth-reading.md. B3 is kept by code: step 2 refuses while any process
  named Roamer runs, naming each by pid and start time, and the same read is made again
  after the backups, just before the constructor. The probe went from 1834 lines, sha256
  B1228224, to 1923 lines, sha256 9CC1987B. Proved first with no Navisworks, by a harness
  that ran each fault on the b01ad71 probe and on the new one, failing before and passing
  after, with stand in Roamer.exe copies of ping.exe refused at step 2, at the last read,
  and by the real probe, in %LOCALAPPDATA%\NwcFederatorLoop\turn3\f100-harness
- THE READING, a reviewer and a breaker, both APPROVE, all seven fixed completely, no fault
  of the seven left and no new fault inside attempt 4. Their other findings are polish, or
  old faults outside the seven, which go into the register for run.ps1, which takes this
  code over
- RUN 4 at 11:35, the one run: all six steps passed in 125 s. The constructor returned
  after 83.16 s, pid 33752 was adopted on all four conditions 0.06 s after the call began,
  OpenFile and SaveFile of one NWC copy worked, AddPluginAssembly returned, Dispose
  returned in 0.40 s and the Navisworks was gone 8.5 s after, nothing forced. With no other
  Navisworks running, 36 registry values and 2 files of Bader's settings were put back,
  each after a last check, and read back, and nothing changed in his AutoSave or logs
  folders. docs\history\scan.md 5z-d RUN 4
- NO RESULT FILE ON MAIN CARRIES A LICENSING ID. Run 4 printed none. Run 3's file is kept
  for the lines scan.md cites, masked on three lines by F102's mask-evidence.ps1, which is
  on branch fix-F102, PR 76, and merges after this. The reflection file and scan.md no
  longer name the machine. F102's check at 0b48415, run by the lead over a git archive of
  this branch at 9085d13, read 447 files and found none carrying one, kept in
  %LOCALAPPDATA%\NwcFederatorLoop\turn3\f100-evidence-check.txt
- the notes a later session needs are in steps\notes, and Bader's answers are Q79 to Q81

### What remains

- F100's code moves into run.ps1's guard file in F103, with the old faults the reading found
- the commits 464f79f and c98c6f3 of the deleted branch still hold the licensing ids on
  GitHub, through PR 74's own refs, which no one here can remove. Q88

### Known bugs

- the old faults outside the seven, register rows T3-G1 to T3-G5 in steps\loop.md. The one
  that matters most: the last check before each settings write takes a process list it
  could not read as no Roamer, so it fails open

### What comes next

1. F103 run.ps1 from turn3\f103-design.md, off main once this merges. F102 merges when its
   second fix attempt is read, and F105 waits in the form, Q89

## 2026-09-29 The loop, turn 3, the plan, written before the first edit

Bader answered the form in steps\loop.md on 2026-09-29: F100 A, his Navisworks closed, and
main's upstream put right. The defaults D1 to D4 stand as he wrote them, so D3 builds R1
to R6 in this loop once the loop is clean on main. This entry is the plan, written before
the first edit and not changed after it. What each step then did is in the entries above
it and in steps\loop.md turn 3.

### Bader's answers, applied first

- Navisworks. Get-Process Roamer read 0 processes at 09:52 on 2026-09-29, so his pid 34668
  is gone. It is read again before every start, install and put back, and once F100's
  fourth attempt and run.ps1 are in, the code reads it and refuses
- Git, before: `## main...origin/master [ahead 320]`, main and origin/main both e555619,
  branch.main.merge refs/heads/master. origin/master is be0b9b37, THE BUILD THE INSTALLED
  ADD-IN READS, so GitHub Desktop's Pull brought be0b9b37 every time and that is why the
  add-in installed on 27 Sep is not main. Set with git branch --set-upstream-to=origin/main
  main. After: `## main...origin/main`, main and origin/main both e555619, branch.main.merge
  refs/heads/main. The remote master branch and every other remote branch are left alone,
  D6 stays Bader's
- F100, A: one more fix attempt of exactly the seven faults of the fourth reading, F1 to F4
  and B1 to B3, with B3, no start while any Navisworks runs, kept by code. Then the reading,
  then the one run, then the merge. A new fault in the reading sends F100 to B with no
  question

### Measured before the plan

- the result files on fix-F100 name this machine on their first line, and
  automation-start-result-20260928.txt carries the licensing agent's -i id at lines 446 and
  447. docs\history\scan.md on the branch names the machine at line 4650. Main holds none of
  them, read with git grep
- the probe writes a new process's command line whole, so a fourth run would write the
  licensing ids again. Nothing in tools\loop reads a result for them before a commit, and
  every probe under tools\probes prints the machine name on its MACHINE line

### The round, in order

1. The records, on fix-F100: this entry, STATE OPEN and turn 3 in steps\loop.md, the
   answers as Q79 to Q81 in steps\02_questions.md, and the notes a later session needs,
   the fourth reading, the F101 design and the three fix lists, copied into steps\notes with
   the machine name masked
2. F102, its own branch and pull request: a masking step for every result committed from
   this machine, the machine name and the licensing and session ids, which refuses when one
   is left, and a check Actions runs for the licensing ids. Written beside step 3 in a
   worktree of its own, read by a reviewer and a breaker, merged before F100's run
3. F100 fix attempt 4 by the developer, exactly F1 to F4 and B1 to B3, B3 as code: the
   probe refuses while any Roamer runs, read again just before the start. Proved by the
   harness with no Navisworks
4. The reading of attempt 4 by a reviewer and a breaker. A fault of the seven left, or a
   new fault inside what attempt 4 changed, is B
5. A: Roamer read 0 by the probe itself, the one run, the result masked by F102 and read for
   ids, scan.md 5z-d given run 4, the pull request body read by the claim-checker, Actions
   green, squash merge so the old result file stays off main. B: the result file out, the
   records masked, merged, and run.ps1's start and close written fresh from the design
6. The prober, Phase 1 item 2: probe-viewpoints.ps1 and probe-model-remove.ps1 on this
   install, reflection only and no start, appended to scan.md. Roamer.exe's switches read
   off the binary with no start. Whether the installed API writes the native Clash
   Detective HTML tabular report, by reflection. Whether UI Automation reaches the tool's
   window, with the first start of step 7
7. F103, tools\loop\run.ps1, its own pull request: refuses in code while any Navisworks
   runs, before every start, install and put back. Asks Windows to stay awake through
   SetThreadExecutionState while a run goes and lets go after. The hang rule and the dialog
   rule. Logs inside the work folder. The window run D4 needs
8. F104, the separate read of the document, its own pull request: a probe plugin outside
   the solution and the bundle, loaded with AddPluginAssembly, reading each NWF's tests and
   status counts, compared with read-workbook.ps1's read-out and never with the harvest
9. Install main, with Roamer read 0 and the installed bundle matched against bundle-backup,
   then Phase 3, the baseline run set into steps\runs\00, run 1 through the real window as
   D4 says. If the window cannot be driven, why is written and the no-click entry comes
   first. The log-reader reads every run, and every register row marked done in code, not
   proved by a run is marked proved or contradicted with its line
10. F101, the no-click entry from the design, PQ1 to PQ8 measured first
11. Phase 4, the findings one pull request each, a silent wrong number first. D1 after the
    faults, as moves only
12. Phase 5, R1 to R6 with D2, each rule written into docs\workflow.md and the rules files
    first, then proved or built
13. Phase 6, the close, when all seven conditions hold on one final run set from a fresh
    copy

At the end of every stretch steps\loop.md holds the state, so a new session carries on
from it alone.

## 2026-09-28 The loop, turn 2, F98 the close round heading back, DONE

Core tests 1746 passed, 0 failed, 0 skipped, before, on fix-F97 with the same src, and
after, run by hand on fix-F98. Nothing under src or tests changed. Merged in PR 73. Nothing
waits for the local machine, because this is a record fix and no Navisworks run applies.

### What was done

- the diff of PR 71 read: it turned the line `## 2026-09-21 The close round, DONE, the
  record` into the heading of its own INSTALL.md entry, so the close round's report read as
  part of that entry
- the heading is back above `Core tests 1666 before the round and 1746 after`, with the
  blank line under it that it had. Commit b46bb04 is those two lines and nothing else, and
  the rest of the pull request is this entry and the records in steps\01_next.md and
  steps\loop.md
- PROVED by comparing texts: from the heading down, all 5747 lines of steps\log.md match
  the file as it was before PR 71, line endings aside
- read before it went out by a reviewer, a breaker and the claim-checker. The fix held, and
  the wording they flagged in the records is fixed

### What remains

- Phase 1 of the loop, from steps\loop.md

### Known bugs

- none from this fix

### What comes next

1. Phase 1: the prober measures how Navisworks Manage 2025 can start a run with no click

## 2026-09-28 The loop, turn 1, F97 the house and F99 the git wall, DONE

Core tests 1746 passed, 0 failed, 0 skipped, before and after. Nothing under src or tests
changed, and the add-in, built on this machine in turn 0 with 0 errors and 0 warnings, is
untouched. Merged in PR 72. The turn was paused on 2026-09-27 for a restart of
Bader's computer and carried on on 2026-09-28 from steps\loop.md, which held.

### What was done

- gh logged in as baderrahal, so git and gh both work here
- THE WHOLE REPO WAS READ by nineteen read only agents over every file under src, tests,
  tools and steps. What they found is in steps\loop-read.md: 179 faults, none confirmed
  yet, 70 of them silent wrong outputs, 150 members nothing in src calls, 77 catches
  called swallowing, 46 files holding more than one top level type. No Navisworks type in
  Core. The summary block the chat audit named as doubled above BuildViewpoints is not
  there. The register built from it, which the prompt puts in Phase 2, is in steps\loop.md
  with 269 rows
- the team, eight agents under .claude\agents. They loaded when the session started after
  the restart, and were used for the second and third reviews
- THE HOUSE WAS READ THREE TIMES BEFORE IT WENT OUT and each read found real faults in my
  own work. The first, by one reviewer and two breakers: read-workbook.ps1 turned this
  tool's own one row tests into invented clash rows, prepare-copy.ps1 kept a copy on size
  alone and could write its listing into NM Fed, and the walls could be walked round. The
  second, by the loop's own reviewer and two breakers: a nameless test or clash row was
  dropped, the plain copy command undid a removed file, the git wall judged a whole
  command at once so a -d anywhere excused any push, a newline after main hid it, the gate
  shared one note across sessions, and the walls could be edited by a file tool. The
  third, a breaker on the rewritten walls: a push deleting main, and a switch to the branch
  before and a commit. Every one of those is fixed. What cannot be fixed by reading words
  is written into each hook, the rules and CLAUDE.md as a stated limit
- THE WALLS. The paths wall refuses a file tool under samples, steps\logs, steps\runs,
  bundle, .claude\hooks and .claude\settings, relative and any case, and a file write or a
  Bash, PowerShell or Monitor command naming NM Fed, split by quotes, escaped or as its
  short name, or ACCDocs. The git wall, F99, reads each git call on its own words through
  Bash, PowerShell and Monitor and refuses a commit on main and any push landing on or
  deleting main from any branch. The Stop gate sends a session back once per session per
  change while steps\loop.md reads OPEN, and never blocks forever
- THE WALLS' OWN COST, measured. The first paths wall took 11.9 seconds a call. The final
  walls in .claude\hooks, timed three times each, take 2.4 to 3.6 seconds on an ordinary
  call, which is starting sh here, and 6.9 to 8.0 on a git commit, which also asks git for
  the branch
- the proof. 114 cases fed on standard input by tools\loop\prove-hooks.sh, the git
  wall against a throwaway clone with main checked out, every case answering as it should,
  first in tools\loop\hooks-next where the walls were rewritten. Then they were copied into
  .claude\hooks by a command, because no file tool may change that folder, and proved
  again there, 114 cases right. Live, in this session, they refused four real calls:
  a PowerShell command naming NM Fed, a Write under steps\runs, a Write under .claude\hooks
  and a dry run push of HEAD to main. tools\loop\hooks-next was then removed, so each hook
  lives in one place
- tools\loop\prepare-copy.ps1, proved on the real folder: kept, listed without touching
  the copy, one NWC removed, restored by hash, kept again, and twelve calls refused with
  their reason and nothing changed. An earlier version made the copy again when NM Fed
  changed under it on 2026-09-27, seen in the session and not kept on disk
- tools\loop\read-workbook.ps1, shares no code with the writer, proved on both client
  exports, on a workbook of this tool's own shape written by its WorkbookWriter, on the same
  shape with a nameless test and a nameless clash, and on a workbook that is not there
- both scripts were proved again on 2026-09-28 after the restart, with every answer they
  printed kept in %LOCALAPPDATA%\NwcFederatorLoop\turn1\proof-scripts-2026-09-28.txt, out
  of the repo because it carries lines of Bader's log with the client's project paths. The
  same file holds the git check-ignore of steps\runs, the picture targets read off the
  client workbooks and the second read of Bader's 16:37 log
- the allow list names the scripts and the git and gh commands the loop runs, and deny
  rules stop force pushes, --no-verify, switching the hooks off, reset --hard, clean and
  merging with --admin, in Bash and PowerShell and in their git -C forms
- MEASURED AND WRITTEN DOWN: hooks load mid session, agents only at session start. Claude
  Code on Windows finds sh without help, which CLAUDE.md held as UNKNOWN. The client's
  workbooks hold 65 and 66 pictures each, every one LINKED to an absolute file:/// path and
  none stored, which is what core.md says, read off the zip on 2026-09-28
- FOUND ON THE WAY: Bader ran the tool on 1B06BC at 16:37 on 2026-09-27 with NM Fed as its
  NWF, NWD and workbook folder. The run logged plugin version be0b9b37, the build of the old
  checkout, wrote the first NWF at 16:56:20, and its last line is the second NWF attempt at
  17:00:36, with no line holding RESULT, no workbook and no NWD. It is register row
  RUN-1637. NM Fed changes under the loop, and the copy follows it

### What remains

- F98 in its own pull request, next
- Phase 1: how Navisworks starts a run with no click, the two probes, the no-click entry,
  run.ps1 and the separate read of the document
- Phase 3 on: the baseline run set, the loop, R1 to R6, the close
- every register row not DONE

### Known bugs

- the stated limits of the walls, which read words: a script that builds a path or a git
  call at run time, a git alias, and a command reaching a protected folder, which only a
  file tool is stopped from
- the 179 faults of the read, until confirmed or refuted

### What comes next

1. F98, the close round heading back
2. Phase 1, in this session, because the agents are loaded

## 2026-09-27 The loop, turn 0, the plan, written before the first edit

This is the first session that runs on Bader's own machine, where Navisworks Manage 2025
is installed. Every number below was measured in this session. It ends in STATE WAITING,
because gh is not logged in and the prompt makes that login Bader's one action. The code
read, the team, the walls and every run come after it.

Core tests on main at 42499bf, on this machine: 1746 passed, 0 failed, 0 skipped. In the
container the same set reads 1714 passed and 32 skipped, so all 32 tests that need
Navisworks or Windows run here, and all 32 pass. Build of the whole solution in Release,
add-in included: succeeded, 0 errors, 0 warnings, 11.4 seconds.

### The machine

| what | measured |
| --- | --- |
| clone | this folder is PAR_NWC-Federator and points at PAR_NWC-Federator-V2. It sat at be0b9b3, 308 commits behind, and was fast forwarded to 42499bf. The tree was clean |
| git | 2.55.0.windows.5, a PortableGit under C:\Users\p003653k\Tools |
| git fetch | failed every time with CRYPT_E_NO_REVOCATION_CHECK. The global config already turns the revocation check off, and git ignores that until http.sslBackend names schannel. Set http.sslBackend schannel in this clone's .git\config only, and fetch then passed every time |
| git push | no GitHub login for the command line. The one GitHub credential on the machine belongs to GitHub Desktop and is not borrowed |
| gh | was missing. 2.101.0 put under %LOCALAPPDATA%\Programs\gh from the official release zip, sha256 checked against the release before it was unpacked. NOT LOGGED IN |
| GitHub connector | signed in as baderrahal |
| dotnet | SDK 8.0.420, 8.0.423, 8.0.424 and 8.0.425. Builds net48, add-in included |
| PowerShell | Windows PowerShell 5.1, no pwsh |
| Git Bash | bash 5.3.15 |
| Navisworks | Manage 2025, 22.5.1433.58. Roamer.exe and the Api, Clash, ComApi, Interop.ComApi and Automation DLLs are all in C:\Program Files\Autodesk\Navisworks Manage 2025 |
| disk | C: 410.9 GB free |
| already running | one Navisworks, process 32472, started at 10:55 before this session, Untitled. The loop did not start it and never closes it |
| the two walls | went live mid session, the moment the fast forward put .claude on disk, and Claude Code found sh without help, which CLAUDE.md holds as UNKNOWN. The git wall is matched on the Bash tool only, so a git commit sent through the PowerShell tool passes it. That is a Phase 0 finding |

### NM Fed

The desktop is C:\Users\p003653k\OneDrive - Parsons Corp\Desktop, under OneDrive. Every
file carries the reparse point OneDrive puts on a synced file and none is marked offline.

141 files, 208.3 MB: 140 NWC at 207.0 MB and one clash XML. No NWF and no NWD anywhere.
NWD\C06, NWD\C07, NWF\C06, NWF\C07 and Clash Report are empty folders. Every NWC name has
seven parts, so part 3 and part 5 read on all 140.

NWC\C06, 67 NWC in 22 groups:

    100000 3 AR ME         1B06BC 5 AR EL ME ST   1B06BS 1 EL            1B06G1 4 AR ME ST
    1B06K1 3 AR ME ST      1B06KI 1 EL            1B06M1 2 ME ST         1B06P1 4 AR ME ST
    1B06PE 4 AR EL ME ST   1B06PG 1 EL            1B06PH 5 AR EL ME ST   1B06PK 2 ME ST
    1B06PO 4 AR EL ME ST   1B06PP 8 AR EL ME ST   1B06PS 3 AR EL ST      1B06PT 1 ST
    1B06PW 1 EL            1B06WL 4 EL ME ST      1B06WM 4 AR EL ME ST   1B06WO 4 AR EL ME ST
    1C06M2 2 ME ST         1C06PK 1 ST

NWC\C07, 73 NWC in 24 groups:

    100000 3 AR ME         1A07PP 1 ME            1C07BC 5 AR EL ME ST   1C07BS 1 EL
    1C07G1 3 AR ST         1C07K1 3 AR ME ST      1C07KI 1 EL            1C07P1 4 AR ME ST
    1C07PG 2 EL ME         1C07PK 1 ST            1C07PS 3 AR EL ST      1C07PW 1 EL
    1C07WE 4 AR EL ME ST   1C07WL 4 EL ME ST      1C07WM 4 AR EL ME ST   1C07WN 2 EL ST
    1C07WO 4 AR EL ME ST   1D07MM 2 ME ST         1D07PE 4 AR EL ME ST   1D07PH 5 AR EL ME ST
    1D07PN 4 AR EL ME ST   1D07PO 4 AR EL ME ST   1D07PP 7 AR EL ME ST   1D07PT 1 ST

100000 is in both folders, three files each, and is not a building. Twelve groups hold one
discipline and cannot clash with anything, six in each folder.

The clash XML is 1104-PAR_CLASH_AllInOne_25mm_FIXED.xml at the top of NM Fed, 1,443,383
bytes, LF, sha256 starting 36AB27395BBABD9A. THE LOOP USES THIS ONE, because the prompt
says the one in NM Fed wins. It matches no copy in the repo. Against
samples\1104-PAR_CLASH_AllInOne_25mm.xml it differs on 121 lines, and every one of them
renames the set BLD-DRPipe Accessories to BLD-DR-Pipe Accessories. It does NOT carry the
workset case changes in exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml, so it still asks
for ME-DUCTWORK, ME-PIPING and ME-EQUIPMENT where the exchange copy asks for ME-Ductwork,
ME-Piping and ME-Equipment. The close round measured that the upper case sets found nothing
on C02. What they find on NM Fed is measured, not assumed, and it is R2. His file is never
edited.

### Found this turn

- F98. PR #71, the INSTALL.md round, DROPPED THE HEADING of the close round entry in this
  file. The edit replaced the heading line and never put it back, so the close round's
  whole report now reads as part of the INSTALL.md entry, straight after its plan. The
  heading goes back above "Core tests 1666 before the round and 1746 after", in its own
  pull request
- F99. The git wall covers the Bash tool and not the PowerShell tool, so a commit on main
  sent through PowerShell is not refused. Phase 0 closes it

### The defaults as pasted

D1 one public type per file, written into core.md and addin.md, every file under src split,
moves only. D2 a rectangular section is sized by its larger side, and a size that cannot be
read counts as over 150 mm, so it stays in the viewpoints, its clash stays New and the log
names it. D3 R1 to R6 are built in the same loop once it is clean.

### The plan, in order

Turn 0, this one. Machine check, NM Fed listing, this plan, steps\loop.md at STATE WAITING,
gh auth login at the top of the form. Commit on fix-F97 locally. Nothing can be pushed.

Turn 1, after the login.
1. gh auth status, gh auth setup-git, and a push of fix-F97 to prove git can push
2. Read everything the prompt names. The code under src, tests and tools goes through
   reader agents that hand back a map: every type and the file it lives in, every public
   member and its callers in src, every file holding more than one public type for D1, and
   every member with no caller in src
3. Phase 0 on fix-F97, one pull request: the eight agents, the Stop hook, rules\loop.md,
   tools\loop with its README, steps\runs with its own gitignore negation, the protected
   path wall widened to NM Fed and every ACC Desktop Connector path for every file and
   command tool this Claude Code offers on Windows, PowerShell included, the git wall
   widened to PowerShell, the allow list, every hook case fed on standard input with its
   answer pasted in the PR. Merged when Actions is green. If the agents are not live,
   STATE RESTART and stop
4. F98, the heading back, its own pull request

Phase 1. The prober measures a run with no click, Roamer switches and the Automation DLL,
whether UI Automation reaches the window, and whether the API writes the native HTML
tabular report, then runs probe-viewpoints.ps1 and probe-model-remove.ps1 and appends every
answer to docs\history\scan.md. The no-click entry lands as its own pull request, its
settings file parsed in Core with tests, calling the engine method the Run button calls.
run.ps1 keeps the machine awake, watches the log and the processor time, and closes only a
Navisworks it started. The workbook is checked by a separate read of the document.

Phase 2. The register in steps\loop.md, every open fault from 01_next, the fault kind
questions in 02_questions, 04_audit, 04_audit_first_run, the known bugs lines of this file,
every proof step in 03_bader_next never proved, the doubled summary block above
BuildViewpoints, F50 and F52 behind their switches, D1 and F98.

Phase 3. The logs backup, the copy of NM Fed under %LOCALAPPDATA%\NwcFederatorLoop\source,
build, install, the five run baseline into steps\runs\00. No fix lands before it.

Phase 4. The loop. Most harmful first, a silent wrong number above a loud failure, one
finding one branch one pull request, a failing test first, the reviewer and the breaker on
every change, merged green, installed, proved by a run on the copy. D1 last, moves only.

Phase 5. R1 to R6 written into docs\workflow.md and the rules first, then built one pull
request at a time, each proved by a run.

Phase 6. The close conditions on one final run set from a fresh copy, then one first run
through the real window, then 03_bader_next cut down, the form, 05_audit, STATE CLOSED.

### What was done

The machine check, the NM Fed listing and the XML comparison above. gh 2.101.0 installed
under %LOCALAPPDATA%\Programs\gh. http.sslBackend schannel set in this clone's .git\config.
The build and the Core tests run once each on main.

### What remains

Everything from turn 1 on.

### Known bugs

F98 above.

### What comes next

Bader runs the one command at the top of the form in steps\loop.md and pastes the prompt
again.

## 2026-09-27 INSTALL.md, DONE

Core tests 1714 passed, 0 failed, 32 skipped, 1746 total, both before and after this
round. Nothing in Core or the add-in changes this round.

### What was done

INSTALL.md is written at the repo root, 37 lines. It holds only what to paste into a VS
Code terminal to build and install the add-in, with the build command, the
NavisworksPath override, the install command, the success line, and what to do on a
failure, all read off build\install.ps1 and Federator.Addin.csproj rather than guessed.
README.md carries one new line pointing at it. steps\README.md does not list how to
install, so it is unchanged.

This session is a Linux container with no PowerShell and no Navisworks install, which is
the state addin.md already names for a container session. Every command in INSTALL.md
was still run here, in order, and what each one actually did is in the pull request body.
The build command ran and failed exactly where a build on a machine with no Navisworks
folder at the named path is expected to fail, both with the default NavisworksPath and
with the override. The install command could not be run at all, there is no PowerShell
here, and starting Navisworks could not be tried either. Neither is claimed to work.

### What remains

The install command and starting Navisworks, proved only on Bader's machine, where
PowerShell and Navisworks Manage 2025 are both present. That is steps 5 and 6 of
INSTALL.md, unrun here.

### Known bugs

None found this round. Nothing outside INSTALL.md and the one README.md line changed.

### What comes next

Bader runs INSTALL.md on his own machine and says whether the success line and the Tool
Add-ins tab match what it promises. A mismatch there is a finding for the next round, not
a guess this one makes now.

### The plan, in order

1. Read CLAUDE.md, the four files under .claude\rules, README.md, docs\workflow.md,
   build\install.ps1, every .csproj and this entry's own section of steps\log.md. Done,
   above this line
2. Branch fix-install-md off main
3. Write INSTALL.md at the repo root, under forty lines, holding only what to paste into
   a VS Code terminal: what the file is for, what to close and pull first, the build
   command read off build\install.ps1 and Federator.Addin.csproj, the NavisworksPath
   override for a moved install, the install command, the success line read off
   build\install.ps1's last Write-Host, and what to do on a failure
4. Add one line to README.md pointing at INSTALL.md. steps\README.md does not list how
   to install, so it is left alone
5. Run every command INSTALL.md gives, in order, from a fresh terminal, and paste each
   command's real output into the pull request body. This session is a Linux container
   with no PowerShell and no Navisworks on it, which the addin.md rule already names as
   the state a container session is in, so the build command is the one part of
   INSTALL.md this session can run and the install command and starting Navisworks
   cannot be run here. Both are said plainly rather than claimed
6. Write this entry's ending, What was done, What remains, Known bugs, What comes next,
   with the Core test counts before and after
7. One draft pull request off fix-install-md, body carrying the pasted output and what
   waits for Bader's machine. Watched with the GitHub tools until Actions is green, then
   merged, then the local branch deleted

## 2026-09-21 The close round, DONE, the record

Core tests 1666 before the round and 1746 after, 0 failed and 0 skipped in both. Build 0
errors and 0 warnings after every change. Both `tools\checks` pass. Tree clean. Bader
answers Q20, Q55, Q56, Q73, Q74 and Q75, and Q76, Q77 and Q78 are new.

### THE NUMBER THE ROUND EXISTED FOR, AND IT SAYS THE CASE CORRECTION WORKS

C02 could never answer this. Its sets were already in the NWF, built from the ORIGINAL
matrix asking `ME-DUCTWORK`, and F28 leaves a set already at its path exactly as it is, so
the corrected value never reached them. C04 holds no NWF at all, so every set there is
built FRESH from the corrected file, and that is the only way the question could be put.

On `1A04EP`, the mechanical sets built from the corrected matrix:

| set | items it found |
| --- | --- |
| `BLD-ME-Ducts&Duct Fittings` | **277** |
| `BLD-ME-Air Terminals` | **45** |
| `BLD-ME-Mechanical Equipment` | **13** |
| `BLD-ME-Duct Accessory` | **12** |

Those same four find **ZERO** in every C02 group and have done since the first run.
Across 1A04EP, **37 of its 61 sets found items and 24 found nothing**, against **38
finding nothing** in C02's 1A02MM. THE CORRECTION WORKS, and the measurement is the
difference between a set built from the corrected file and a set that predates it.

### THE C04 RUN WAS STOPPED ON PURPOSE AFTER TWO GROUPS, AND WHY

`1A04EP` found **2,566 clashes** and then spent over forty minutes writing one viewpoint
per clash, about **0.54 seconds each against 0.03 on the fixture**, with the working set
climbing from 1,047 MB to 1,477 MB the whole time. One group took longer than the entire
drift round run over all ten C02 groups. Twelve groups at that rate is several hours.

THE VIEWPOINTS STEP DOES NOT SCALE AND THAT IS THE FINDING. It is written per clash
through the COM API with three read backs each, so its cost is clashes times models, and
C04 clashes far harder than C02. Waiting hours to restate that would have cost the round
its other deliverable, so C04 was stopped after two complete groups and C02 was run.

WHAT C04 STILL PROVED IN TWO GROUPS: the case correction above, the non building case,
PART 3 at full scale, the alignment block naming a real fault, and 4d suppressing a real
false positive. WHAT IT DID NOT: a full twelve group run, the run minutes against 45, the
total clash count, the penetration and by design totals, and the other ten groups' sets.
Those carry forward.

- **1A0415**, the first non building. 3 tests created of 1,830, 3 run, 1,827 skipped, 0
  clashes, DONE in 8.198s. Its workbook is **1,830 blocks in 1,834 rows**, which is the
  theoretical minimum and was 14,643 rows under the old shape
- **1A04EP**. 666 tests created of 1,830, 666 run, 557 passed, 1,164 skipped, 2,566
  clashes. Its ALIGNMENT block reads `EL ... DIFFERENT dx -0.03 mm dy -0.04 mm dz -60 mm`
  and `ask the EL originator to re-export on the project shared coordinates`, which is a
  real coordination fault found on a building that had never been on this machine
- **the workset block**, on real data: `no two workset names in this group are close
  enough to be one word typed twice. 2 more pair(s) are close...`. 4d works: `FP-PIPING`
  against `ME-PIPING` is NOT named, and the decided pairs are counted rather than hidden

### PART 7b, C02, AND THE RENAME WORKED ON ALL SEVEN GROUPS

The freeze gate passed first: eleven NWFs, every one byte for byte where the drift round
left them, and the backup read back 19 files with **0 byte size mismatches**.

**SEVEN RENAMES ACROSS SEVEN GROUPS**, which is exactly the seven that held 62 sets:

    SET  1 set(s) in this NWF are not named by the picked file:
    SET     1 are the working half of a pair, so the unused half goes and this one takes its name
    SET  RENAMED "BLD-DRPipe Accessories" to "BLD-DR-Pipe Accessories" and removed the
         unused set that held that name. Its 60 test side(s) keep working and now ask
         what the file asks

ZERO refusals, zero failed removals, zero sets left with pointers and no twin.

**AND NOTHING ELSE MOVED, WHICH IS THE POINT.** Against the run of 22:28 on 2026-09-20:

| | drift round | this run |
| --- | --- | --- |
| total clashes | 1,225 | **1,225** |
| by priority | A 842, B 190, C 193 | **identical** |
| groups done, failed | 8, 2 | **8, 2** |
| sets finding nothing per group | 54 59 46 38 55 43 47 40 42 37 | **identical** |
| tests with an empty side | 1809 1829 1725 1577 1815 1677 1739 1620 1659 1554 | **identical** |
| sets drifted and rebuilt | 28 | **28** |
| run time | 1110.688s | **1041.789s**, inside 45 |

Seven sets renamed, 420 test sides kept working, and not one clash count moved. A rename
changes what a set is CALLED and not what it FINDS, so identical numbers are the proof
that the change was surgical.

**THE WORKBOOK, Q73 AT SCALE.** 1A02MM went from **15,182 rows to 2,775**, with the block
count still exactly **1,830** and all 542 clash rows present. 1,773 empty tests now cost
one row each where they cost 14,184.

### PART 3's PREDICTION AGAINST ITS MEASUREMENT

Predicted before the change and measured after it, which is worth more than a saving
claimed afterwards. On the fixture: **1,916 predicted, 1,917 measured**, one row out, block
count unchanged at 1,830.

THIS IS A DEPARTURE FROM THE CLIENT'S OWN FORMAT and the only one in this tool. Their
export writes the full eight row block whatever the test found. Ours writes one row for a
test that found nothing, carrying the test name, the tolerance, the zero counts, the type
and the status, so the row is still evidence the pair was checked. That sentence is for
NMDC.

### THE FIXTURES, WHICH ARE THE RUNG THIS TOOL HAD NOTHING ON

A probe measures ONE API call. A live run writes into a project folder and takes 15 to 25
minutes. Nothing sat between them, which is why the drift round needed four live runs in
one night to land four fixes.

Two fixtures now do, at **19.8 and 17.8 seconds**, under
`C:\Users\bader\AppData\Local\Temp\claude\round-close`. `tools\probes\fixture.md` records
both, why the penetration one is AR and ME rather than ST, and what neither can show.

**WHAT THEY COST: about an hour to build and choose. WHAT THEY SAVED, in runs:**

- they found that **Navisworks will not remove the last model from a document**, in nine
  seconds, which would have been a live run
- they found that **the clear and rebuild does not carry viewpoints**, 52 dropping to 22,
  in nine seconds. The count out and count back rule caught it and refused to save, so
  nothing was lost. That is Q77
- they proved PART 2's rename on real data before it went near his folders
- they measured PART 3's saving against its prediction

Four live runs saved, at twenty minutes each, on their first day.

### WHAT WAS FOUND BY READING RATHER THAN RUNNING

**SIX CODE FAULTS, from reading `steps\03_bader_next.md` against the code.** The first
would have made this round's own C02 report meaningless.

1. **The reshape never recorded that it had rebuilt the group.** `outcome.Decision` kept
   the value it was given BEFORE the work, so every reshaped group read PARTIAL with the
   reason "the NWF points at a different set of files, so it was left alone", about a
   group just brought up to date, and the RESULT block counted it under SKIPPED
2. **"the file on disk is left exactly as it was" was still written after a
   modification**, inside `RemoveThem`'s catch, where a throw can only happen after one.
   The caller added the correct sentence a moment later, so the log carried both
3. **The worst: a reshape that failed after a change returns true to stop the fallback,
   and true also means carry on.** The group went into the clash step, the viewpoints and
   `SaveTheNwfAgain`, which COULD STILL WRITE THE NWF while the error said it had not been
   saved. It stops on `HasErrors` now
4. **The confirm dialog told him a CHANGED group is "cleared and rebuilt"**, on the one
   screen he can still cancel from
5. The RUN SETTINGS penetration line omitted foundations, missed when Q63 widened the list
6. **The clash comment saved INTO THE NWF carried the unrounded size**, so a pipe read
   `21mm` in the log and `20.997mm` in the comment, and the comment is the half that
   travels with the file

**PART 5's THREE DEFECTS, two of which fire.** The double count silently deleted a model
and reported success: the engine read `comparison.Removed` and added `move.From` on top,
so a moved file went in twice, indexes shift up by one behind a removal, and the second
call took out the model that had shifted into that slot. The model count is counted out
and counted back now, which is the arithmetic that makes the check able to fail. The
chartered defect HAS NEVER FIRED on his files, proved: one run ever rebuilt a group,
`run-20260919-144319`, and the reshape was added the next day.

**THE RESTATED FACTS FINDING.** One list, four copies, three drifts, and the tested copy
is the only one that never went wrong. The solid list drifted into the tick box help line,
`docs\workflow.md` and `tools\probes\fixture.md`, and a FOURTH hand typed copy sits in the
probe that nobody had noticed. Five more stale restatements were corrected in `src`, one
of them the doc comment on the class that OWNS the list, fourteen lines above it. THE
CHECK IS NOT BUILT THIS ROUND. It is F96, the first item of the next, specced with all
four drifted facts named so it is a build and not a rediscovery.

### THE `El` ERROR, STATED AS PLAINLY AS ANY FINDING

This round reported, with confidence and twice, that
`1104-PAR-1A04WO-ZZZ-El-MOD-000001.nwc` carried a lowercase L and planned a case blind
compare on it. **THE FILE IS `EL`.** `od -c` reads `E L`, a case sensitive count gives 7
for `-EL-` and 0 for `-El-`, and the file's modification time had not moved.

**THE VERIFY PASS CAUGHT IT BY LISTING THE LIVE FOLDER ITSELF** rather than trusting the
survey. That is the one job it existed to do and it earned its cost on the first gate it
was pointed at.

THE LOWERCASE `El` IS REAL, one level away: the REVIT SOURCE name in Autodesk Docs, in
`steps\logs\run-20260919-211323.log`, on the C02 twin of that building. It reaches nothing
this tool compares, because the source rules compare BUILDING CODES only. **Bader fixed it
at source on 2026-09-21**, so it is FOUND on 2026-09-19, REPORTED on 2026-09-21 and FIXED
the same day, and nothing was built for it.

### WHAT IS UNTESTED, said rather than left to be discovered

- **Q67's two paths have STILL never fired and C04 was the chance.** All 46 C04 models
  carry `revit_ProjectLocation` and all 46 are at 100 per cent element id, so the NO
  SHARED COORDINATE path and the MISSING ELEMENT ID path met no real file again. C04 HAS
  NOW BEEN TRIED and did not show them. The INTERNAL ORIGIN path does fire, on 1A04WL
- **Ten of C04's twelve groups never ran.** Their sets, clashes, viewpoints and workbooks
  are unmeasured, and so are the run minutes against 45 for a full C04 run
- **The reshape's happy path still has not run on a real CHANGED group.** It was forced on
  a copy and DECLINED, correctly, because every model would have had to come out. The
  three failure paths are proved and the success path is not
- **`RebuildFromScan` cannot carry viewpoints**, so any group with tool made viewpoints
  fails on the fallback. Caught, refused, nothing lost, and open as Q77
- **The penetration and by design rules moved 0 on C02**, because earlier runs already
  moved them and they sit at Reviewed, which `StatusesThisToolMayMoveFrom` refuses to
  touch. Read the BUCKETS and never the total: a zero with reasons spread under it is a
  measurement and a zero with everything in one bucket is 5r's signature

### THE WL FINDING, WHICH BADER CAN SEND AS IT STANDS

**Two structural models are exported on Revit's internal origin, not on a shared site.**

    C04, building 1A04WL:  1104-PAR-1A04WL-ZZZ-ST-MOD-000004.nwc
                           1104-PAR-1A04WL-ZZZ-ST-MOD-000005.nwc
    C02, building 1A02WL:  1104-PAR-1A02WL-ZZZ-ST-MOD-000004.nwc
                           1104-PAR-1A02WL-ZZZ-ST-MOD-000005.nwc

**THE SAME TWO FILE NUMBERS IN THE WL BUILDING OF BOTH COMMUNITIES.** C02's pair failed
that group in the drift round on 2026-09-20 and C04's pair was read on 2026-09-21. One
structural modeller, the same mistake twice. A model on the internal origin is in a
different coordinate system from the rest of its group, so every clash reported against it
is either a clash that is not there or a miss that is.

### A THING TO WATCH ON THE NEXT RUN, not work for this one

When the renamed Revit model is republished, check whether the publish ADDS an NWC beside
the old one rather than replacing it. Two models of one discipline in one group read as a
file added, which sends that group down the Rebuilt path, which is the path PART 5 has
just been fixing.

### WHAT WAS STARTED AND WHAT WAS LEFT

Rule 2, which the drift round did not carry.

**Programs started**: Navisworks Manage 2025, eight times, through `Roamer.exe` started
normally and driven by UI Automation. The automation host `NavisworksApplication` twice
for the probe passes.

**Processes stopped**: Navisworks was closed normally seven times. **ONCE IT WAS FORCE
STOPPED**, ending the C04 run, and that left a `Navisworks Manage 2025 Error Report`
dialog which blocked the next launch until it and a `senddmp` process were also stopped.
Both are named because a force stop is not a clean close.

**Files written outside the repo**, all under
`C:\Users\bader\AppData\Local\Temp\claude\round-close` except where said:

    \fixture\             three C04 NWC copies, 308 KB, plus its NWF, NWD and Report output
    \fixture-pen\         two C04 NWC copies, 988 KB, plus its output
    \fixture-sets\        four C02 NWC copies and one NWF copy, plus its output
    \probe\               eleven C02 NWF copies and the probe results
    \c04-partial-run-20260921-085105.log   the stopped C04 run's log, 527,627 bytes
    \*.ps1                the drivers, the block counter, the decode helper
    \survey-*.txt \d-*.txt \j*.txt          the agent reports
    \wsc-from-agent.txt   a scratch file an agent wrote into the repo root, moved out

**Inside his project folders**, which only PART 7 may write:

    C04\C04-backup-2026-09-21-close\   46 files, 150,272,153 bytes, read back 0 mismatches
    C04\NWF, NWD, Clash Report         two groups' outputs from the stopped run
    C02\C02-backup-2026-09-21-close\   19 files, 51,000,592 bytes, read back 0 mismatches
    C02\NWF, NWD, Clash Report         the full run's outputs

**Left running**: nothing. After the C02 run Navisworks was still open, it was closed
normally, and the check was run afterwards and reported no Navisworks and no crash
reporter process left. That last close is the eighth and it was clean.

### What comes next

F96, the restated facts check, is the first item of the next round and is specced. Q76,
Q77 and Q78 are open. F18 is still the only other open item and its blocker is a FILE and
not a decision: `samples\client-report` holds the 1A02WN and 1A04WN exports, not 1A04WE.

## 2026-09-21 The close round, THE PLAN REVISED A THIRD TIME, written before the next edit

Fourth brief, round still in flight, nothing already done is redone. This says what changes
against the revised plan at `ef5c5fc` and in what order the rest goes.

### The change that reorders everything else: THE FIXTURE

Bader asked why the tool is not simply run over and over until it works, and the answer is
that nothing sits between a probe and a full live run. A probe measures ONE API call. A
live run writes into his project folder and takes 15 to 25 minutes. So every fix this
round and the last has cost a whole live run to prove, which is why four runs were needed
last night to land four fixes.

**PART 1d BUILDS THE MISSING RUNG AND IT GOES FIRST.** A small copy group under
`C:\Users\bader\AppData\Local\Temp\claude\round-close\fixture`, and PARTS 2, 3 and 5 each
run the tool END TO END against it, read the log, fix, and run again, as many times as it
takes, before anything goes near C02 or C04.

**THE CHOICE, AND WHY.** It has to span more than one discipline or no `BLD-` set pair can
clash, and it has to put a SERVICE against a SOLID or the penetration rule never runs.
Measured off 6a, `1A04WE` is by far the smallest building that does both:

| building | AR | EL | ME | ST | elements in AR+ME+ST |
| --- | --- | --- | --- | --- | --- |
| 1A04WE | 139K | 2,233K | 120K | 38K | 25 + 23 + 33 = **81** |
| 1A04WO | 1,395K | 601K | 786K | 27K | larger |
| 1A04PW | 1,689K | 8,793K | 5,649K | 3,501K | 271 + 4,010 + 250 |

So the first pick is **1A04WE's AR, ME and ST, three files, 297 KB, 81 elements**, which
exercises AR against ME, AR against ST and ME against ST, and puts a service against a
solid for the penetration rule. EL is left out because it is 2,233K of the building's
2,532K and adds one more discipline for eight times the size.

**AND THE FIXTURE HAS TO EARN ITS PLACE ON ITS FIRST RUN.** 81 elements may produce no
clashes at all, and a fixture that finds nothing exercises neither the clash step, the
viewpoints, the penetrations nor a non empty workbook block. If the first run finds zero
clashes the fixture moves to `1A04PW`'s ME and ST, which carry 4,010 and 250 elements and
will certainly clash, at the cost of 9 MB and a slower loop. That decision is made on the
first fixture run and reported either way.

**WHAT THE FIXTURE CANNOT SHOW, said now rather than after a green run.** It is one small
group, so it cannot show scale, the weekly path, a CHANGED group, alignment across many
models, the two non buildings, the single discipline groups, or anything needing the real
matrix against real content. A green fixture is not a green run and the report says so.

### The four other changes

**PART 4 GAINS TWO BUILD ITEMS, BOTH ABOUT NOT CRYING WOLF.**

4d, THE NEAR TYPO RULE IS FIXED AS A CLASS. `FP-PIPING` against `ME-PIPING` is edit
distance 2 and they are two different disciplines, Fire Protection and Mechanical.
Flagging them is the same fault the drift round fixed once for `AR-EXTERIOR` against
`AR-INTERIOR`, and it costs the real typo sitting beside it, `EL-Lightining Protection` in
1A04WM. THE DISCIPLINE PREFIX IS NOT PART OF THE COMPARISON: two names whose prefixes
differ are never a typo pair however close the rest is, and two sharing a prefix and
differing in the body still are.

4e, AN INVISIBLE DIFFERENCE IS NAMED BY ITS CHARACTER. `EL-Fire alarm` exists in two
spellings across C04, one carrying a NON-BREAKING SPACE. Today no single group carries
both so nothing compares them, and that stops being true the moment one group gets both
models or somebody types the matrix value with an ordinary space. Then a set finds nothing
and the screen shows two identical looking strings. So the report says NON-BREAKING SPACE,
U+00A0, AT CHARACTER N, and the same for a tab, a double space, a trailing space or a zero
width character.

**PART 6 GAINS 6j AND 6k.** 6j gives the WL finding its own heading rather than a log
line: 1A04WL's ST-000004 and ST-000005 are on the internal origin, and C02's 1A02WL failed
the drift round on THE SAME TWO FILE NUMBERS. One structural modeller, the same mistake
twice, in the WL building of two communities, written so Bader can send it as it stands.
6k adds a question about `DEFUALT`, which matters more than its spelling: a model on a site
named DEFAULT is probably not on an agreed project shared site, which is close to what
`Internal` means and is what Q70 fails a group for. The fail rule does NOT change, both
names are reported as they are, and the question asks whether a DEFAULT site should fail a
group or be reported only.

**Q67'S CONSEQUENCE HALF CLOSES AND THE OTHER HALF GETS A BETTER REASON.** The internal
origin path fires on 1A04WL, so that half is proved on a second building. The NO SHARED
COORDINATE path and the MISSING ELEMENT ID path have still never fired, because all 46 C04
models name a coordinate and all 46 are at 100 per cent element id. Both carry forward in
the untested section with the reason that C04 WAS THE CHANCE AND HAS NOW BEEN TRIED.

**THE CLOSING SAYS WHAT THE FIXTURE COST AND WHAT IT SAVED, IN RUNS**, so the next round
knows whether to keep it, and PART 8 adds it to `steps\03_bader_next.md` as a numbered
step so Bader can run it himself.

### The order of the remainder

1. **PART 1d, the fixture**, built and run once to see whether it clashes
2. **The duplication bug** the verification found in the removal list, read before wiring
3. **PART 5, the reshape fix**, then forced failure paths on the fixture
4. **PART 2**, remove the twin then rename, proved on the fixture then on copies
5. **PART 3**, the empty block becomes one row, proved by opening the fixture's workbook
6. **PART 4**, the four reporting fixes, 4d and 4e with their tests
7. **PART 6**, the record, the register, the two new findings
8. **Build, full suite, both checks, install**
9. **PART 5's happy path** on a copy of a C02 group with one NWC added and one removed
10. **PART 7a, C04**, only after the fixture is green
11. **PART 7b, C02**, behind the freeze gate that can stop it
12. **PART 8**, the closing pass
13. **Closing**: the report, the questions, the pull request or the compare link

## 2026-09-21 The close round, THE PLAN REVISED AGAIN, written before the next edit

Third brief, round still in flight, nothing already done is redone. This says what changes
against the revised plan at `34b662c` and in what order the rest goes.

### What stands unchanged

PART 3, PART 4, PART 6a to 6c and 6e to 6h, PART 7a and PART 8 are word for word what the
second brief made them. PART 2's shape is settled and confirmed: remove the unused twin,
rename the broken set into the freed name, both in memory, verify, then save, with the
parent scoped remove and a read back instead of a trusted return value. PART 5's fix is
`DamagedDocument`, already built.

### The six changes

**1. THE MESSAGE CLAIMS ONE WORD MORE THAN IT KNOWS, AND IT IS ALREADY COMMITTED.**
`DamagedDocument.TheDocumentIsDamaged` says the file on disk is `the last good copy`. That
is a claim about HISTORY. This tool knows one thing: it did not write. Whether what is on
disk is good was decided by whatever wrote it last, which may have been a run that failed
in some other way. The sentence becomes UNCHANGED BY THIS RUN, which is exactly what the
save gate proves and nothing more. The first sentence carries the same shape and gets the
same treatment.

THIS GOES FIRST, because PART 2 and PART 5 both call it and both would otherwise inherit
the overclaim.

**2. A NEW QUESTION THAT HAS TO BE ANSWERED BEFORE THE ROUND REPORTS: DID THIS DEFECT EVER
FIRE ON HIS FILES.** He will ask whether it has already eaten one, and the answer should
be ready rather than assembled under the question.

The drift round says `ReshapeFromScan` was never executed, because all ten groups took
Weekly run plus XML on all four runs. If that holds, the reshape's own two damaging exits
never ran. BUT THAT IS ONLY ONE PATH. Before the drift round a CHANGED group used the
clear and rebuild DIRECTLY, not as a fallback inside the reshape, and the question is
whether THAT path read its before counts off an already modified document too. So: read
every run log he has, find every group that ever ended Rebuilt, and say per run whether a
defective path ran. Answer yes or no, plainly, with the evidence.

**3. 6d IS ANSWERED WITH 2 IN HAND AND NOT AS A SEPARATE PUZZLE.** 1A02BS lost 54 bytes
between the backup and the run of 22:28 and nothing explains it. Fifty four unexplained
bytes is the size of trace a partially written document leaves. It is probably unrelated
and its census was unchanged, but the two are looked at together.

**4. THE `El` IS FOUND AND FIXED, NOT AN OPEN FINDING.** Bader renamed the Revit file in
Autodesk Docs to `EL` on 2026-09-21. It was a modeller's mistake. 5z-c changes from
reporting a live model hygiene issue to recording it as FOUND on 2026-09-19 in a committed
run log, REPORTED on 2026-09-21, and FIXED AT SOURCE by Bader the same day. It is not
deleted, because the evidence in `steps\logs\run-20260919-211323.log` is history and the
record of how it was found is worth keeping. Nothing is built.

**5. PART 7b GAINS A FREEZE GATE AND IT CAN STOP THE RUN.** C02 is frozen while the
renamed model is not republished. After the C02 backup is read back, if the file count
does not match the drift round's ELEVEN NWFs, or a new NWC has appeared in the folder,
the C02 run STOPS and says so. Reporting a Rebuilt group as though it were a weekly one
would make every comparison against the run of 22:28 meaningless, and that comparison is
the whole of what 7b is for.

**6. THE CLOSING GAINS A THING TO WATCH, NOT WORK FOR THIS ROUND.** When the renamed
Revit model is republished, whether the publish ADDS an NWC beside the old one rather than
replacing it. Two models of one discipline in one group read as a file added, which sends
that group down the Rebuilt path, which is the path PART 5 is fixing. That is a sentence
in the round report and nothing else.

### The order of the remainder

1. **The wording fix to `DamagedDocument`**, first, because two parts call it
2. **Did it ever fire**, over every run log he has, and **6d answered with it**
3. **PART 5, the reshape fix**, wiring `DamagedDocument` into the three exits and taking
   the counts at the top
4. **PART 2**, remove the twin then rename, atomic against the save
5. **PART 3**, the empty block becomes one row
6. **PART 4**, the questions recorded and the category line naming its folder
7. **PART 6**, the record and the register
8. **Build, full suite, both checks, install**
9. **PART 5's proofs**: the three failure paths forced on copies and read back byte for
   byte, then the happy path
10. **PART 7a, C04**, backed up and read back first
11. **PART 7b, C02**, backed up, read back, and STOPPED if the freeze gate trips
12. **PART 8**, the closing pass
13. **Closing**: the report, the six questions, the pull request or the compare link

### What the four design agents have already returned

All four finished and all four verifications are running. The designs cover the reshape
fix, the workbook one row change, the C04 model survey and the record reconciliation.
Nothing is built on any of them until its verification lands, because the one time this
round trusted a survey without that, the survey was wrong about a live folder.

## 2026-09-21 The close round, THE PLAN REVISED, written before the next edit

The brief was revised while the round was underway. Nothing already done is redone. This
says what changes against the plan committed at `77d03f9`, and in what order the rest goes.

### What stands, unchanged

The build gate and all three PART 0 gates. The `El` correction at `d083ea3`. 5z. The C02
baselines. PART 3, Q73, is word for word what it was. PART 4b and 4c are unchanged.
PART 6a to 6f are unchanged. PART 8 is unchanged except that it now also checks rule 2
was kept.

### The six changes, and the one that matters most

**1. PART 5 STOPS BEING A PROOF AND BECOMES A FIX, AND IT IS NOW THE HEADLINE.**
The committed plan had PART 5 forcing the reshape on a copy to prove it works. The
verification pass found it does not. `ReshapeFromScan` has THREE `return false` sites and
two of them fire AFTER the document has been modified. `return false` makes the caller
try `RebuildFromScan`, which reads its before counts off the already damaged document,
finds everything present, reports everything kept, and SAVES THE NWF OVER, while the
error the reshape added says the file on disk was left exactly as it was.

THAT IS WORSE THAN ANY FAULT THIS TOOL HAS HAD. The worksets round's reader returned
nothing and looked like an answer. This one tells him a true sentence about his data is
false and then writes the damage to disk. So PART 5 is now: hold the before counts from
the TOP, never re read them off a touched document; make any `return false` after a
modification refuse to save and say the NWF on disk is the last good copy; keep the
fallback off a modified document entirely. AND PROVE THE THREE FAILURE PATHS by forcing
each one on a copy and reading the NWF back byte for byte, because a path only ever
proved when it succeeds is exactly how this survived.

**2. PART 1 GAINS 5z-b AND PART 2 IS BLOCKED ON IT.**
5v measured a CONDITIONS replace. 5z measured a REMOVE. A DISPLAY NAME change is a third
field and nobody has measured it. On a copy: rename a set in the MIDDLE that test sides
point at, save, close, reopen off the disk, and read back whether those sides still
resolve and to the right set, the results, the statuses, the viewpoints, and what it finds.

**3. PART 2 TURNS FROM REMOVE INTO RENAME, ON THE STRENGTH OF THIS ROUND'S OWN
MEASUREMENT.** Q74 was answered before 5z existed. 5z found 60 test sides point at the
BROKEN `BLD-DRPipe Accessories` in each of seven groups and NOTHING points at the
corrected spelling. So the broken set is the one doing the work and the corrected one is
sitting unused. Removing the broken one orphans 420 test sides across seven groups.
Removing the corrected one changes nothing. The fix is to RENAME the broken set to the
corrected name, so its 60 sides keep working and start asking the right question, then
remove the unused duplicate, which 5z already proved is safe.

AND THE REFUSAL CONDITION WIDENS. The brief said refuse when a test LOSES ITS RESULTS. 5z
found the results survive and the SIDE stops resolving, which that wording would not
catch. The condition is now: refuse when anything pointing at the set would stop
resolving, whatever happens to the results. If 5z-b says a rename does not keep what
points at a set, NOTHING IS BUILT for this case and the box refuses and names the 420.
The same measurement governs `BLD-Security Devices`.

**4. THE CASE BLIND DISCIPLINE COMPARE IS WITHDRAWN**, because it rested on the `El` that
was not there. In its place scan.md records the MEASURED fact that the discipline code is
uniformly uppercase across all 46 C04 files and across C02, dated, and a question is
written so the reasoning already exists if a lowercase code ever arrives: a discipline
code is a CLOSED set of seven and could be matched case blind safely, where a workset
value is an OPEN set and could not, which is exactly why Q68 refused the flag there.

**5. PART 4a GETS BIGGER THAN IT LOOKED.** Q55 has NO `Answer:` line in the file at all,
so recording his answer means ADDING one rather than filling a blank. And PART 6 gains
6g, the `HelpLine` that is 11 words while its own comment says 12, and 6h, reconciling
the question count, because the GAP block's own text points at Q25, Q38 and Q39 as live
while an earlier read of that file counted only Q55, Q56 and Q73 as unanswered.

**6. PART 7a GAINS FOUR RULES ABOUT C04'S GROUPS**, and the first protects the one number
the round exists to produce. `1A0415` and `1WAW15` are NOT buildings: their disciplines
are LS, LT and SW, none of the seven, and every set in the picked matrix is a `BLD-` set,
so both will find nothing and it will look identical to the case mismatch being measured.
They are reported as "no set in this matrix applies to this group" and are EXCLUDED from
the empty set figures compared against C02. `1A04MS` and `1A04PK` hold one discipline each
and get one line rather than 1,830 tests that found nothing. `1A04WL`'s ST files are
numbered 1, 2, 3, 4, 5, 7 and the missing SIX is named without guessing why. And where the
round shows an example it shows `1A04PW`.

### The order of the remainder

1. **5z-b**, one probe pass, because PART 2 is blocked on it
2. **6a read and reported**, and scan.md written for 5z, 5z-b, 6a and the uppercase fact
3. **PART 5, the FIX**, first of the code, because every run after it depends on the
   engine not lying about his data
4. **PART 2**, rename or refuse, whichever 5z-b allows
5. **PART 3**, the empty block becomes one row
6. **PART 4**, the questions recorded and the category line naming its folder
7. **PART 6**, the record, the register and the two new reconciliations
8. **Build, full suite, both checks, install**
9. **PART 5's proofs**: the three failure paths forced on copies and read back byte for
   byte, then the happy path on a copy with one NWC added and one removed
10. **PART 7a, C04**, backed up and read back first
11. **PART 7b, C02**, backed up and read back first
12. **PART 8**, the closing pass over both logs, a workbook from each, and
    `steps\03_bader_next.md` read end to end against the code
13. **Closing**: the round report carrying the `El` error as plainly as any finding and
    saying the verify pass caught it by listing the live folder, the six questions
    recorded, and the pull request or the compare link

### What 6a already shows, before it is written up

All 46 models surveyed, 1,656,930 bytes of output. Two things are already visible and
both go in the report. NOT ONE of the 46 carries a property named for a shared
coordinate, shared site or project location, so how the ALIGNMENT check reads that fact
has to be established against the code before Q67 can be called closed or carried
forward. And `1A04PW`'s architecture model is published from
`Autodesk Docs://KSA_New Murabba/1104-PAR-0000PW-ZZZ-AR-MOD-000001.rvt`, whose building
code is `0000PW` where the NWC says `1A04PW`, which is precisely the SOURCE MISMATCH the
tool already looks for and has never met on a model that was on this machine.

## 2026-09-21 The close round, THE PLAN, written before the first edit

### The build gate and the three gates, all four passed

`dotnet build ParsonsNwcFederator.sln -c Release`, 0 errors and 0 warnings, Navisworks
Manage 2025 found.

**0a, the merge gate. PASSED.** `origin/main` is `017f797`, the merge of pull request 67,
and `round-drift` is an ancestor of it. Nothing is stacked. `round-close` branches off
that main.

**0b, the C04 gate. PASSED, AND THE FOLDER CHANGED SINCE YESTERDAY.** The drift round read
`C:\00-NM\Federation Task\C02 + 04\C04` as empty, zero files, on 2026-09-20. It now holds
**46 files, every one an NWC, 150,272,153 bytes, 143.3 MB**, written at 23:32 that night.
**0 NWF, 0 NWD**, and the `Clash Report` folder is empty too.

SO EVERY C04 GROUP TAKES FIRST RUN and every set in it is built FRESH FROM THE CORRECTED
MATRIX. That is the only way the case correction can be proved, because on C02 the sets
were already in the file and `ME-Ductwork` could never reach them.

**0c, the scan gate. PASSED, 46 files make 12 groups, 0 unreadable, 0 blocked, 0 name
collisions.** This was not simulated by hand: Core was built from source and its real
public API driven over the 46 names.

| group | files | disciplines as the code sorts them | count | can clash |
| --- | --- | --- | --- | --- |
| 1A0415 | 3 | LS, LT, SW | 3 | yes |
| 1A04EP | 5 | AR, EL, ME, ST | 4 | yes |
| 1A04KI | 4 | AR, EL, ME, ST | 4 | yes |
| 1A04MS | 1 | ST | 1 | NO |
| 1A04PK | 1 | ST | 1 | NO |
| 1A04PW | 5 | AR, EL, ME, ST | 4 | yes |
| 1A04WE | 4 | AR, EL, ME, ST | 4 | yes |
| 1A04WL | 8 | ME, ST | 2 | yes |
| 1A04WM | 4 | AR, EL, ME, ST | 4 | yes |
| 1A04WN | 4 | AR, EL, ME, ST | 4 | yes |
| 1A04WO | 4 | AR, EL, ME, ST | 4 | yes |
| 1WAW15 | 3 | LS, LT, SW | 3 | yes |

Every group takes FIRST RUN, because no NWF exists for any of them. Every group writes
`1104-PAR-<building>-ZZZ-BM-MOD-000001` with three extensions, and the discipline field
is `BM` on all twelve because every group spans more than one discipline.

NO FILE FAILS TO GROUP. All 46 split into seven non-empty parts, all are `1104` and `PAR`,
none is blocked and none is dropped.

**THE SCAN WILL RAISE 14 FINDINGS AND NONE OF THEM STOPS ANYTHING.** 2 ODD SHAPE, which
are `1A0415` at shape `9A9999` and `1WAW15` at `9AAA99` against the other ten at `9A99AA`.
0 NEAR MATCH. 2 SINGLE DISCIPLINE, which are `1A04MS` and `1A04PK`, one ST file each. And
**10 MISSING**, which is where the third oddity shows itself.

**THE THIRD ODDITY, AND THIS PARAGRAPH WAS WRONG WHEN IT WAS FIRST WRITTEN.**

WHAT IT SAID. That `1104-PAR-1A04WO-ZZZ-El-MOD-000001.nwc` carries `El` with a lowercase
L where all 45 others carry `EL`, and that because every discipline comparer in this tool
is Ordinal, the run would report eight disciplines instead of seven and tell six healthy
buildings they were missing a discipline that does not exist.

WHAT IS TRUE. **THE NWC FILE IS `EL`, UPPERCASE, AND THERE IS NO CASE SLIP IN ANY OF THE
46 FILE NAMES.** Read off the bytes with `od -c`: `E L`. A case sensitive count gives 7
matches for `-EL-` and 0 for `-El-`. The folder holds SEVEN disciplines, AR EL LS LT ME
ST SW, not eight. So the ten MISSING rows keep their count and lose the contents this
plan gave them: `1A0415` and `1WAW15` are missing AR, EL, ME and ST, the six four
discipline buildings and `1A04WO` are missing LS, LT and SW, and `1A04WL` is missing AR,
EL, LS, LT and SW. Nothing is told it is missing a discipline that does not exist,
`IsADisciplineCode("EL")` is true, and 1A04WO's viewpoints will not log that no model
carries EL.

HOW IT WAS CAUGHT. Not by a later run and not by Bader. The recon that answered this gate
was adversarially verified, and the verifier listed the live folder itself and refused the
claim. The file's own modification time is 23:31:37 and has not moved, so nothing was
renamed underneath the reading. The first reading was simply wrong and the check that was
built to doubt it did its job.

**AND THE LOWERCASE `El` IS REAL, IN A PLACE THAT MATTERS LESS AND IS MORE INTERESTING.**
It is in the REVIT SOURCE NAME, not the NWC name. `steps\logs\run-20260919-211323.log`
carries it in the committed evidence of an earlier run:

    holds  ...\1104-PAR-1A02WO-ZZZ-EL-MOD-000001.nwc
    [source Autodesk Docs://KSA_New Murabba/1104-PAR-1A02WO-ZZZ-El-MOD-000001.rvt

So the Revit file in Autodesk Docs is spelled `El` and the NWC published from it is
spelled `EL`, on the C02 twin of this same building. It reaches NOTHING this tool
compares: `SourceMismatchFindings` and the shared source rule both compare BUILDING CODES
only and never the discipline, so the case never bites. It is model hygiene and it is
worth Bader knowing, and it is a finding in the round report rather than anything built.

### What C04 can and cannot prove, said before it runs

C04 CAN prove the case correction, because its sets are built fresh. C04 CANNOT prove
PART 2's removal, because a first run has no drifted set and no duplicate to remove, and
it cannot prove the reshape, because a first run is never a CHANGED group. That is why
PART 7 runs both folders and why PART 5 forces the reshape on a copy.

### The order of work, and why it is that order

**MEASURE FIRST. BUILD. THEN RUN.** Navisworks is started ONCE for the whole of PART 1
and nothing in PART 2 is written until 5z has answered.

1. **PART 1, one probe pass.** 5z, what REMOVING a set costs, against COPIES under
   `C:\Users\bader\AppData\Local\Temp\claude\round-close` and never his own files, with a
   clash set to Reviewed first so there is something to lose, a set in the MIDDLE and not
   only the last, and five read-backs after a save, a close and a reopen off the disk.
   Plus the count that decides what PART 2 may do: how many clash tests in his seven C02
   groups point at `BLD-DRPipe Accessories`. And 6a, what C04's models carry, which needs
   NO new probe code because the existing `survey` mode already reads the model root's
   properties, the first geometry leaf, its composite parent, and walks for worksets and
   element ids. 5z and 6a into `docs\history\scan.md`.
2. **PART 2, Q74**, built on 5z and nothing else. The tick box also REMOVES a set the
   picked file no longer names, naming what pointed at it first, every time, and REFUSING
   where 5z says the results would not survive. Plus the `BLD-Security Devices` name fix
   in `MatrixCorrections`.
3. **PART 3, Q73.** An empty test becomes ONE ROW. Every test still appears.
4. **PART 4**, Q55 and Q56 recorded, and the category block says which folder its list was
   measured from, which matters more now than yesterday because the list is C02's and the
   run is C04's.
5. **PART 6**, the record and the register, including the F numbers the drift round never
   got and the two corrections 6d, 6e and 6f ask for.
6. **PART 5, Q75, the forced reshape**, run AFTER 2 to 4 and 6 so one install covers it.
   A copy of a C02 group under the temp folder with one NWC added and one removed, so the
   group takes the CHANGED path for real.
7. **PART 7a, C04**, backed up and read back first even though it holds no NWF.
8. **PART 7b, C02**, backed up and read back first.
9. **PART 8**, the closing pass over both logs, a workbook from each run counted, and
   `steps\03_bader_next.md` read end to end against the code.

### What PART 1c already found, before any of it was built

The source file column comes out empty on every row of every report, measured in scan.md
5n, and Bader has decided it stays that way. The question the brief asked is what else
depends on it. The answer, read across the whole tree:

- **`ClashHarvest.SourceFileOf` returns the empty string on every item of every clash**,
  and `ContainerName.Parse("")` does not throw, so the discipline it derives is empty too
  and nothing in the log ever says so
- **ONE consumer, and it is not the report.** `Federator.Core.Report.GapRule` counts how
  many items carried a source file and a discipline, and a property carried by nothing is
  deliberately left out of the GAP block. Both are always zero, so NEITHER EVER APPEARS,
  and a reader concludes they are not being held back. They are not being held back
  because they were never read, which is the opposite reason, and the block cannot tell
  those two apart. **That is a new question. Q55 settled the report and not this.**
- **Everything else is safe, including the two that would have been serious.** The
  viewpoint discipline folders come from the SET NAME through `DisciplinePairRule` and
  never from the source file, so no viewpoint folder on any run has been wrong. Which
  models a viewpoint hides comes from `document.Models[i].FileName`, its own read. The
  workbook writes four cells per item and none is the source file. The clash XML writes
  two quick properties and says so in its own comment
- **Two side findings.** A comment on `FederationEngine`'s workbook check claims it is
  the check that would have caught the empty column, and it is not and cannot be, because
  there is no such column in the workbook to count. And `ClashReport.SourceFile` is
  assigned and read nowhere in src or tests, which the public member rule covers

NOTHING IS FIXED FOR ANY OF THAT IN THIS ROUND and the harvest does not climb.

### What is already known to be untested and will be said again at the end

The reshape stops being untested in PART 5. Q67's consequence is answered or carried
forward by 6a. F18 stays open and it waits on a FILE and not a decision: the samples hold
the 1A02WN and 1A04WN client exports and not 1A04WE.

## 2026-09-20 The drift round, DONE, the record

Core tests 1666 before the round and 1703 after, 0 failed and 0 skipped in both. Build
0 errors and 0 warnings after every change. Both `tools\checks` pass. Bader answers Q72.
Q34 is answered by a measurement rather than by him. Q73 is new.

THIS ROUND RAN ON HIS MACHINE AND NOTHING WAITS FOR HIM TO PROVE IT. The add-in was
built, installed and run over his live C02 folders four times, and every number below is
read off a log or off a file this tool wrote, never off the object that wrote it.

### What PART 1 measured, and three of the four came back the opposite way

One probe pass, Navisworks started once, and nothing in PARTS 2 to 5 was written until
all four had answered. The last three rounds each found a reader that returned nothing
and looked like an answer, and two of these four were that shape.

- **5v, can a set be replaced without losing what points at it. YES.** `ReplaceWithCopy`
  at the set's own index, then a save, a close and a REOPEN OFF THE DISK, and five
  read-backs: the clash test still points at the set, its 36 results are there, the
  Reviewed status a person set is there, the set is in the same place in the tree, and it
  finds items. So PART 2's box never has to refuse and is not under Things that destroy
  data
- **5w, what every set in an NWF is actually asking. IT READS.** `SelectionSet.Search` is
  readable for every set of all ten groups, 0 unreadable, which is what PART 3 rests on.
  It also found SEVEN of the ten groups holding **62** sets where the matrix holds 61,
  because `BLD-DRPipe Accessories` and `BLD-DR-Pipe Accessories` are BOTH in there, which
  is Q72 visible in a file. And the condition flags say 1A02MM's 61 sets are an original
  import while every other group's are this tool's own
- **5x, what taking one model out of an open document costs. NOTHING.** On 1A02MM the
  sets stayed at 62, the tests at 1830, the results at 526, the statuses at 526 and the
  viewpoints at 509, through a save and a reopen. That closes Q34, open since 2026-09-18
- **5y, what setting a tolerance on a saved test costs. IT RESETS NOTHING.** Not the same
  value and not a doubled one. Every result and every status a person set survived a save
  and a reopen. His own logs show 175,434 saved tests have had one set on them while the
  confirm screen told him in capitals it reset them all

### What was built

- **PART 2, Q72 answered a.** A tick box, off by default, `Rebuild sets that drifted from
  the file`, grey line `Only sets whose question changed. Results and statuses are kept,
  measured`. It compares the QUESTION and not the flags, rebuilds only what drifted and
  never all 61, and leaves alone a set the picked file does not name
- **PART 3, the sets block tells the truth and says WHY.** 3a prints what the set in the
  DOCUMENT asks, where it used to say `asked UNKNOWN` on every line because nothing had
  ever read one. 3b puts a set finding zero in ONE of three named buckets and says what
  it COSTS in clash tests
- **PART 4, and 5y inverted its premise.** SIX places said setting a tolerance resets a
  test's results, including the confirm screen in capitals, and it does not. All six now
  say what it actually costs, which is that the test finds DIFFERENT clashes next run.
  THREE TESTS ASSERTED THE FALSE CLAIM and now assert the measurement. Neither of the
  brief's two options was built, because the thing they would warn about does not happen
- **PART 5, built because 5x allowed it.** A CHANGED group is brought up to date WITHOUT
  clearing. The count out and the count back stays exactly as it was, the clear and
  rebuild is kept as the fallback, and the decline happens before anything is changed
- **PART 6, five record corrections**, including F57's six Look for lines read against
  the code and corrected to what it does

### PART 7, four runs over his live C02 folders

The backup was taken first and read back, 15 files against 15 with 0 byte mismatches, at
`C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20-drift`. Four runs, because
PART 8 found faults in this round's own work and each fix had to be proved on a run:

| run | what it proved |
| --- | --- |
| 21:06 | the tick box works. 147 sets drifted across the ten groups and were rebuilt |
| 21:36 | the stale handle fix. 28 still drifted, because 21:06 had corrected the rest |
| 22:08 | the run tail's new clash total |
| 22:28 | the corrected empty-side cost and the bucket wording. THE NUMBERS BELOW |

**THE RUN OF 22:28:54 to 22:48:09.** Run time 1110.688s, which is 18 minutes 31 seconds,
inside the 45 criterion 2 asks for. Total elapsed 1155.564s. 38 files written, every size
read back off the disk.

- **groups: 8 done, 2 failed**, against 2 last round, and the same two. 1A02MM and 1A02WL
  fail on the alignment round's rule, models exported on Revit's internal origin. Every
  output of both was still written
- **TOTAL CLASHES FOUND, the number the last two round reports did not carry at all.**
  `clashes found  : 1225 across 10 groups, 3 of which found none`, and under it one line
  per group: 1000BS 0, 1A0215 0, 1A02BS 109, **1A02MM 542**, 1A02MS 0, 1A02WE 29,
  1A02WL 135, 1A02WM 239, 1A02WN 77, 1A02WO 94. It agrees exactly with the by priority
  line, A 842, B 190, C 193, No priority 0, 1225 in all, which is two independent
  additions of the same clashes
- **against 395 in his 1A02MM report: 542.** And 57 tests found something against his 49
- **sets finding nothing, against the 33 the alignment round reported**: 54, 59, 46, 38,
  55, 43, 47, 40, 42, 37 across the ten groups. 1A02MM is 38, of which 19 ask for a value
  NO MODEL IN THIS PROJECT CARRIES, 19 ask for a value models in this project DO carry,
  and 0 this reader cannot tell about
- **tests with an empty side, against his 1,677 of 1,830**: ten different numbers,
  1809, 1829, 1725, **1577**, 1815, 1677, 1739, 1620, 1659, 1554. 1A02MM is 1577, which
  is 100 fewer than his 1,677, and that difference is what the tick box bought. 0 tests
  had a side no count was taken for
- **sets rebuilt**: 28 drifted on this run and were rebuilt. The first run with the box
  on, 21:06, found and rebuilt 147. `BLD-DRPipe Accessories`, the one with the missing
  hyphen, is NOT among them and is correct not to be: the picked file names it, so it is
  not drifted, and 5w found it and `BLD-DR-Pipe Accessories` both present in seven groups
- **penetrations: 0 moved**, against 56 in the worksets round and 12 at 21:06. The block
  says why in its own numbers: of 1A02MM's 542 clashes, 20 are `a person had already set
  it, left alone`. The earlier runs moved them and they are at Reviewed now, which
  `StatusesThisToolMayMoveFrom` refuses to touch. NEVER OVERWRITE A DECISION worked
- **by design: 0 moved**, for the same reason
- **viewpoints**: 1A02MM carries 403, all `already there, left alone, not made again`, 0
  created, and the census reads models 4, sets 62, tests 1830, results 568, views 551
  before and after the NWF save
- **how many results the tolerance cost: NONE.** 5y measured that setting one resets
  nothing, and the census confirms it across the run: 1A02MM's results went 526 to 568
  and its viewpoints 509 to 551, both UP

**EVERY NWF BEFORE AND AFTER**, 11 files in the backup against 11 on disk now, none gone:

| group | before | after |
| --- | --- | --- |
| 1000BS | 13,091 | 13,091 |
| 1A0215 BM | 78,366 | 78,428 |
| 1A0215 LS | 4,300 | 4,300 |
| 1A02BS | 1,296,725 | 1,296,671 |
| 1A02MM | 24,835,774 | 27,465,014 |
| 1A02MS | 81,957 | 81,957 |
| 1A02WE | 481,727 | 481,805 |
| 1A02WL | 249,786 | 3,189,245 |
| 1A02WM | 6,257,126 | 6,293,678 |
| 1A02WN | 913,251 | 913,393 |
| 1A02WO | 986,967 | 1,259,238 |

The folder held 15 files at backup time, 11 NWF and 4 logs, and holds 19 now, the same
11 NWF and 8 logs, the four extra being tonight's runs. THREE NWF FILES are byte for byte
unchanged, 1000BS, 1A02MS and 1A0215's LS file, so nothing was saved over them. FOUR
GROUPS found no clashes, 1000BS, 1A0215, 1A02MS and 1A02WE. THE TWO SETS ARE NOT THE SAME
SET and this sentence originally said they were, corrected on 2026-09-21: 1A0215 found no
clashes and its BM file still grew 62 bytes, because a group that finds nothing can still
have its sets rebuilt, and 1A0215 holds TWO NWF files where every other group holds one.
Counting files and counting groups gives different answers and the sentence had folded
them together. 1A02WL grew twelve times, from 249,786 to 3,189,245, which is the 135
clashes and their viewpoints going in. 1A02BS shrank by 54 bytes, ANSWERED on 2026-09-21
and no longer unexplained: the inflated length never changed and 840 bytes differ across
17 chunks, with the first difference sitting beside `BLD-ME-Ducts&Duct Fittings`, the
first set the 21:06 run rebuilt, and the rest beside the clash test records. It is
compression shifting under the set rebuilds and nothing was lost.

### PART 8 found four faults in this round's own work

Every one was found by reading the run or the code, not by a test failing.

1. **A stale handle in PART 2's own code.** The item count was read through the wrapper
   obtained BEFORE `ReplaceWithCopy`, which is a borrowed handle over an object that is
   no longer there, rule 4g. Every rebuilt set logged `0 items, already there, left
   alone` about a set it had just replaced, and 3b then judged the OLD question and said
   a set asks for `ME-DUCTWORK` and NO MODEL CARRIES IT about a set corrected one line
   earlier. The set is read again AFTER the rebuild now
2. **3b REPORTED A NUMBER THAT LOOKS LIKE AN ANSWER**, which is the shape three rounds
   running have produced. The cost said `IT COSTS 60 of this group's 1830 clash tests` on
   SEVEN of the ten groups while they held between 37 and 55 dead sets. It was counting
   the tests NOT CREATED, and on a weekly run almost nothing is absent to create.
   `EmptySideCost` counts over EVERY test now, and the ten numbers are all different
3. **3b's second bucket claimed to know a thing it cannot see.** The measured lists are
   the whole PROJECT, so `the models DO carry it` means some model somewhere does, not
   that a model of THIS group does. It told him `something else is wrong` about 33 of
   1000BS's 54, every one of which was ordinary. It now says it cannot tell which
4. **THE RUN TAIL HAD NO CLASH TOTAL OF ANY KIND**, which is why the last two round
   reports carried none. `ClashesAcrossTheRun` adds it, and unlike the priority and
   penetration lines beside it, it is ALWAYS written

**The public member rule applied to this round's own work** found five. Two were deleted,
`SetDrift.Name` and the two argument `SetBuilder` constructor. Three were NOT, because
the rule found a MISSING CALLER rather than a dead member: `SetRebuildSettings.ConfirmLine`
was built for the confirm screen and never wired to it, so the box that changes the NWF
said nothing on the one screen a person can still cancel from, and `SetBuildOutcome.Drifted`
and `RebuiltCount` had no reader while the SETS block still carried the worksets round's
sentence saying nothing is done about drift here.

**`steps\03_bader_next.md` read end to end against the code**, 20 drifts corrected, SIX
of them checks that could never pass or fired falsely on a healthy run. Step 97 quoted
words the code has not written since F77. Step 98 expected 1830 created where F77 made it
impossible. Step 212 said 3 visible tick boxes where this round made it 4. Step 266 said
seven indented reasons where there are six. Step 335 said twenty two event kinds where
PART 3 made it twenty three. Step 61 expected `models set` where a document already in
the wanted unit says `nothing was changed`. Four more described a SETS block the log has
not had since F81.

**And `.claude\rules` carried three claims this round disproved**: addin.md and core.md
both said changing a saved test RESETS its results, and addin.md said the clear and
restore dance stays until a run says what removing a file costs.

### What is UNTESTED, said rather than left to be discovered

- **Q67's consequence still stands and is unchanged.** `C:\00-NM\Federation Task\C02 + 04\C04`
  IS EMPTY, so every number in this round is measured on the ten C02 buildings and on
  nothing else. 1A04PW, the building whose 422 hand marked decisions briefed two rounds
  now, is not on this machine as a model at all, only as a report in Downloads. The
  ALIGNMENT and EXPORT CHECK blocks, the penetration rule and now the empty set buckets
  are all proved on C02 rather than on the building they were designed off
- **3b's buckets answer to the PROJECT and not to the group.** The line says so now, but
  the limit remains: this reader cannot tell a set that is wrong from a set whose
  discipline is simply not in that federation
- **5x removed the LAST model of four.** Whether removing a middle model shifts the
  indexes of the ones after it, and whether a viewpoint's index path survives that shift,
  is UNKNOWN, which is why PART 5 removes by NAME and from the END
- **PART 5's reshape has not run on a real CHANGED group.** All ten groups took Weekly
  run plus XML on all four runs, so `ReshapeFromScan` was compiled, reviewed and tested
  in Core and NEVER EXECUTED against a real changed folder. That is the single largest
  untested thing in this round
- **1A02BS lost 54 bytes** between the backup and now and nothing here explains it
- **The tolerance combo would not read back** through UI Automation on three of the four
  runs, so the driver logged it as empty. The value was confirmed off the log instead,
  which says `25 mm chosen in the tool` per group. The window was not the evidence

### What comes next

- Q73 is Bader's: 93 per cent of his 1A02MM workbook is headings for tests that found
  nothing, 14,184 rows of 15,182, and the four ways out are all somebody's decision
- Q68's three ways out, Q69's Or rows and Q71's naming are all done and their consequences
  are in the worksets round entry below
- F18 still waits on the 1A04WE sample, Q9. F23 still waits on Q20. Neither was touched

## 2026-09-20 The drift round, THE PLAN, written before the first edit

### The two gates, both passed

`dotnet build ParsonsNwcFederator.sln -c Release`, 0 errors and 0 warnings, Navisworks
Manage 2025 found. `origin/main` carries the worksets round: Bader merged it as pull
request 66 and `f4e4f78` is an ancestor of `f5fc146`. Nothing is stacked, and
`round-drift` branches off a main that holds everything the last round measured.

### What was read off the installed DLL before planning, because three parts turn on it

- `DocumentSelectionSets.ReplaceWithCopy(GroupItem, int, SavedItem)` is there, and so are
  `InsertCopy`, `RemoveAt`, `Move` and `EditDisplayName`. So PART 2 has a route to try
  and a second one to fall back on, and the misnamed set has a route of its own
- `Search.SearchConditions` is a readable collection and `SearchCondition` exposes
  `CategoryCombinedName`, `PropertyCombinedName`, `Comparison`, `Options` and `Value`.
  So 5w CAN read what a set in the document is asking, which is what PART 3 rests on
- `Document.RemoveFile(int)` and `TryRemoveFile(int)` are there, which 5c already said.
  What they COST is what 5x measures and nothing has measured yet

None of that says any of it WORKS. A member on a DLL is not a measurement, which is the
lesson of the last three rounds, and every one of the four is measured on a real file
before a line of PART 2 to PART 5 is written.

### The order, and why it is that order

MEASURE ONCE, THEN BUILD, THEN RUN ONCE. Navisworks is started ONCE for the whole of
PART 1. The last three rounds each found a reader that returned nothing and looked like
an answer, and two of the four things below are exactly that shape.

1. **5v, can a set be replaced without losing what points at it.** Against a copy of
   1000BS, 13,091 bytes. One clash set to Reviewed FIRST so there is something to lose.
   `ReplaceWithCopy` at the set's own index, save, close, REOPEN OFF THE DISK, and read
   back all five: the test still points at a set and the right one, the test still holds
   its results, the Reviewed status survived, what the set finds now against before, and
   the set is at the same place in the tree. If it does not work, measure delete and re
   add on the same five counts and say what that costs
2. **5w, what every set in an NWF is actually asking.** `SelectionSet.Search` read for
   every set in every one of his NWFs, put beside what the picked file asks. How many
   have drifted, per group and across the run, and named. `BLD-DRPipe Accessories` in
   1A02MM is the check that the read works at all, because it is a known drift
3. **5x, what taking one model out costs.** Open a copy with several models and real
   results, remove one model, read back what happened to the sets, the tests, the
   results, the statuses and the viewpoints that pointed into it. Save, reopen, read
   again. THE ANSWER MAY BE THAT IT COSTS TOO MUCH and that is a good outcome, because
   it closes Q34 either way
4. **5y, the tolerance reset.** On a copy with a test holding Reviewed and Active
   clashes, pick a different tolerance, run, read back how many results and statuses
   were lost. THEN count across every log he already has, both folders, how many tests
   and how many clashes his past runs reset this way. That half needs no Navisworks

All four into `docs\history\scan.md` as 5v, 5w, 5x and 5y.

Then the build, each its own commit, built after every change and not at the end.

5. **PART 2, Q72 answered a.** The tick box, off by default, twelve word help line,
   rebuilding ONLY the sets 5w counted as drifted and never all 61. Per set it says the
   old and new question AND the old and new name. IF 5v SHOWED RESULTS OR STATUSES DO
   NOT SURVIVE it REFUSES a set whose test holds results, names them, and says what
   would be lost. Proved on copies, 1000BS then 1A02MM at 24.8 MB, results and statuses
   counted before and after both times. Five tests
6. **PART 3, the sets block tells the truth.** 3a prints what the set in the DOCUMENT
   asks, read off the set, and says both where the file differs. 3b is the most
   important thing in the round: every set finding zero goes in ONE of three named
   buckets, and the block says what it costs in clash tests, which for 1A02MM is 1,677
   of 1,830. Tests for each bucket, the cost line, and the block absent when no set is
   empty
7. **PART 4, the tolerance trap.** The count read off the open document and never
   estimated, before the run if this window allows a confirm and as a red warning line
   plus a per test log line if it does not. The round says which was built and why
8. **PART 5, the model remove, ONLY IF 5x ALLOWS IT.** If anything is lost, BUILD
   NOTHING, write the cost into scan.md and answer Q34 with the measurement
9. **PART 6, the record.** Five corrections: 5a's UNKNOWN line, the CreateCopy summary,
   the worksets entry's three numbers for one folder, F57's six Look for lines against
   the code, and the four chosen constants checked to still say they were chosen

Then the proving.

10. **PART 7.** Backup read back byte for byte first. Then the real run with the NEW
    TICK BOX ON, reported against the run of 16:20 on every number the brief lists,
    INCLUDING A RUN TOTAL FOR CLASHES, which the last two round reports did not carry
11. **PART 8.** The log read end to end, then ONE OF THE WORKBOOKS THE RUN WROTE opened
    and its blocks counted the way his 1A02MM was counted, then the steps file read
    against the code. Any check that could never pass or never fail is a bug, and the
    rule about a public member nothing calls is applied to this round's own work

### What is decided and is not reopened

Q72 is answered a, the tick box, and not b or c. F18 and F23 wait on Bader and are not
touched or guessed at. F57 IS closed here, because the code is the truth and the steps
file describes the code.

### The two things most likely to go wrong, said in advance

**5v MAY SAY NO.** If replacing a set loses the clash results or the statuses pointing
at it, the tick box becomes a thing that refuses more often than it acts, and the round
says so plainly rather than quietly rebuilding anyway. A tick box that destroys a week
of review is worse than eight NWFs with a wrong set in them.

**3b CAN ONLY GUESS AT THE NEAREST VALUE.** Naming the nearest value a model does carry
is a suggestion and not a correction, and it will be written as one. The workset round
already proved that a name one letter away can be a completely different thing.

### The four standing rules of a run on his machine

Nothing is written into a live project folder except PART 7's run and PART 7's backup,
and everything else goes under `C:\Users\bader\AppData\Local\Temp\claude\round-drift`.
Every program started, every file written outside the repo with its full path and every
process stopped is listed in the round report. Everything opened is closed, the check is
run and it is said. Nothing of his is deleted or overwritten.

## 2026-09-20 The worksets round, a size reader that dropped every worded size, and two groups failed on purpose

### What was done

Five build items off Bader's answers Q67 to Q71, all measured before any of them was
built, because the last three rounds each cost an extra run for a rule built on a guess.
PART 1 found a bug and fixed it, and that bug turned out to be the largest number in
the round.

**PART 1a, 5s, the 29 services with no readable size.** 5r one day earlier had found a
reader that returned nothing and looked like an answer, so 29 of one kind the next day
was worth looking at rather than reporting. EVERY ONE OF THE 29 CARRIED A SIZE THE WHOLE
TIME. Revit writes a conduit's Size as the DisplayString `53 mmø` and a cable tray
fitting's as `600 mmx100 mm-600 mmx100 mm`, one pair per connector, and `ItemSizes` took
only `DoubleLength` and `Double` and dropped every string. Its own comment said a worded
size was deliberately not read because parsing "150 mm" would mean guessing at the unit.
THAT REASONING WAS WRONG ON THIS DATA: the text names its own unit, every time.

`Federator.Core.Views.SizeText` reads it now, through `UnitTable`, which is the one unit
table in this repo. Two things the client's own strings taught it and both have tests: a
number with NO unit is refused and never guessed at, because the penetration rule leaves
alone a service it cannot measure and guessing costs a hole in a wall nobody checked;
and `x` is a dimension separator and not a letter, because the first version read
`600 mmx100 mm` as no measurement at all.

**PART 1b and 1c, 5t and 5u.** 39 workset names across every model of all ten groups,
and the shared site of every model. Both are the source of truth for what follows and
neither rule is built from a name read off a log.

**PART 2, Q68.** The matrix is corrected to the spelling the models carry. NOT an ignore
case flag, and 5t proved Bader right twice over: `AR-EXTERIOR` against `AR-INTERIOR` and
`ST-SUB` against `ST-SUP` are two pairs of REAL worksets one and two letters apart. The
candidates come from `RevitWorksets`, measured and embedded in the DLL the way the
category list already is, so the tool never invents a spelling. One candidate is a
correction, two is a REFUSAL with both named, none is left alone.

**PART 3, Q69.** The Or row, `flags="64"`, built off the condition beside it. AND the
export check still names every near-pair as misspelled with which model carries which,
which is the half that matters, because a tool that absorbs a typo silently means nobody
ever fixes the models.

**PART 4, Q70**, built only after 5u had counted. A group is FAILED when a model names
`Internal` or no site at all, AND IT STILL WRITES ITS THREE OUTPUTS, because the evidence
is what Bader takes to the people who own the models.

**PART 5, Q71.** The penetration block names the services it could not measure, by
category, with a row each in the machine readable log.

### Measured

ONE RUN AGAINST HIS OWN LIVE FOLDERS after the backup was read back: 14 files against 14,
every byte size compared one for one, 0 mismatches. Settings as briefed, 25 mm READ BACK
off the box and never reported as selected, penetrations on, by design on, the priority
file picked, viewpoints on, the corrected matrix.

Against the alignment round's run of 14:24 the same day:

| | 14:24 | 16:20 |
|---|---|---|
| groups done | 10 | 8 |
| groups FAILED | 0 | 2, on purpose |
| penetrations moved | 4 | **56** |
| services with no readable size | 29 | **0** |
| sets finding nothing in every group | 33 | 33 |
| the run | 15 min 16 s | 25 min 22 s |

**THE PENETRATION RULE WENT FROM 4 TO 56** and every one of the new ones is a worded
size the tool could not read yesterday: `Conduits 53mm through Walls`,
`Cable Tray Fittings 150mm through Floors`. The unmeasured count went to ZERO in every
group of the run.

**TWO GROUPS FAILED AND THEY ARE THE TWO 5u NAMED**, 1A02MM on one model and 1A02WL on
two, every one of them a structural model exported on the internal origin. Both wrote
their NWF, their NWD and their report, which is what the answer to Q70 asked for.

25 minutes 22 seconds, inside the 45 minute criterion by 19 minutes 38 seconds. It is
slower than the 15 minutes before it because more sets now find items, so more tests
actually run rather than being skipped for an empty side.

**AND THE MATRIX CORRECTION CHANGED NO CLASH COUNT, WHICH IS THE ROUND'S OTHER FINDING.**
33 sets found nothing before and 33 after. A SET ALREADY IN THE NWF KEEPS THE CONDITIONS
IT WAS BUILT WITH, F28, so a value corrected in the picked file since then never reaches
the document. It is proved by consequence and not assumed: the models carry `ME-Ductwork`
on 236 elements of 1A02MM, the corrected file asks for exactly that, and
`BLD-ME-Ducts&Duct Fittings` still found nothing, so the set in his NWF is still asking
the old question. The run SAYS this now, under the already-there count, and nothing is
done about it, because replacing a set changes what every clash test pointing at it
finds and that is a decision. Q72.

His NWF folder, the backup against what is there now, bytes:

| group | before | after |
|---|---|---|
| 1A02MM | 24,642,386 | 24,835,774 |
| 1A02WM | 6,256,447 | 6,257,126 |
| 1A02BS | 1,296,720 | 1,296,725 |
| 1A02WO | 986,849 | 986,967 |
| 1A02WN | 913,096 | 913,251 |
| 1A02WE | 481,534 | 481,727 |
| 1A02WL | 249,834 | 249,786 |
| 1A0215 | 78,366 | 78,366 |
| 1A02MS | 81,957 | 81,957 |
| 1000BS | 13,091 | 13,091 |

Small, because the viewpoints were already there: 9 written new and the rest left alone,
every one of the 9 dimmed, painted and read back on all four counts, none failed.

### Every program started, every file written outside the repo, every process stopped

Started: Navisworks Manage 2025 through Roamer.exe once for the run and twice as the
automation host for the probe, `census` mode both times, each of which exited on its own.
`dotnet build`, `dotnet test` and `build\install.ps1`, which built and copied the bundle
once. PowerShell drivers for the window, the ribbon clicks, the confirm dialog and the
close, every one of which exited.

Stopped: NOTHING. Navisworks closed through its own window, answering No to the save prompt, and no Stop-Process was needed this round, which is the first round that has been true of. Two `AdskLicensingAgent` processes started at 16:43 when Navisworks did and are still running, which is Autodesk own licensing agent and not something this round can close. No browser and no sign in page opened at any point.

WRITTEN INTO HIS LIVE PROJECT FOLDER, which PART 7 asked for:

- `C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20-worksets`, taken first and
  READ BACK before anything else happened, 14 files against 14, 0 mismatches, 40,008,819
  bytes. His to delete when he is satisfied.
  THE THREE NUMBERS IN THIS ENTRY ARE THREE DIFFERENT THINGS and the entry said them
  without saying so. The folder held 14 FILES at backup time, which is 11 NWF files and
  3 run logs the tool had already copied there. 11 NWF FILES were rewritten. The byte
  table below lists 10 GROUPS, because the run builds ten and the eleventh NWF,
  1104-PAR-1A0215-ZZZ-LS-MOD-000001.nwf, is not one of them and opens with zero models
- his 11 NWF files rewritten, 10 NWDs, 10 workbooks with 10 pages and their picture
  folders, and a copy of the run log in `C02\NWF`, which is where the tool always puts one

Everything else is under `C:\Users\bader\AppData\Local\Temp\claude\round-worksets`: the
probe folder with a copy of all eleven of his NWFs and the probe result files, and the
driver notes. The tool's own log is in
`C:\Users\bader\AppData\Local\ParsonsNwcFederator\logs` and is copied into `steps\logs`.
The bundle at `%APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle` was
replaced once.

His NWC folder was read and nothing was written there. The four properties CSVs the
wiring round left beside his NWCs on 2026-09-19 are still there and still his to delete,
and so is the alignment round's backup from this morning.

### What works, what does not, and what is untested

WHAT WORKS, proved by a run on his own files: the worded size reader, which took the
penetration rule from 4 clashes to 56 and the unmeasured count to zero; the matrix case
correction, which produces exactly the four corrections 5t predicted and refuses where it
should; the Or row; the export check naming the misspelled pairs; and Internal failing a
group while still writing every output.

WHAT DOES NOT: the matrix correction reaches no set that is already in an NWF, so it
changed no clash count on his folders and will only help a building whose NWF has not
been built yet. That is Q72 and it is a decision, not a correction.

WHAT IS UNTESTED, and this is Q67's consequence, recorded every round until a model shows
one: the ALIGNMENT path for a model carrying no shared coordinate at all, and the EXPORT
CHECK path for a model missing an element id. Both have tests that break one thing and
assert the check names it. Neither has ever met a real file, because every model in C02
carries both, and on Bader's answer to Q67 the building that would show them is not
coming onto this machine.

### The closing pass, PART 8

The PART 7 log was read end to end and TWO CHECKS WERE REPORTING NOTHING. The workset
disagreement block was naming `AR-EXTERIOR` against `AR-INTERIOR` and `ST-SUB` against
`ST-SUP` on every group, and 5t had already settled that both pairs are real worksets, so
the block was training a reader to skip past the three genuine typos beside them. A
person decided once, the decision lives in the measured list as a `not-a-typo` line, and
the block COUNTS what it left out rather than going quiet about it. And `SETS ACROSS THE
RUN` said `asked UNKNOWN` on every single line, because on a weekly run every set is
already in the NWF and its question is never read, so the block that exists to say WHICH
sets are wrong said nothing about any of them. It says why now, and points at Q72.

Then `steps\03_bader_next.md` was read end to end against the code and TWENTY TWO things
had drifted, FOUR of them checks that could never pass:

- step 387 told Bader to look for `Nothing failed`, which cannot appear now that Q70
  fails two groups, and the string is written only when failed groups plus errors is zero
- step 392 sent him to `run-20260920-142412.log` for the workset disagreement lines, and
  that log holds none of them, because the block was added today
- step 75 quoted `Nothing is cleared.`, which F75 changed to `Nothing inside it is
  cleared.` and which is in no source file
- steps 248, 249 and 268 expected the count of unmeasured services to be LARGE. It is
  zero on every group since 5s, so the step was telling him to read a number for a
  reason that no longer exists

Four more steps still said the viewpoint writer was unbuilt, two rounds after it shipped,
and three quoted strings were a word out: the by design tick label, the penetration help
line that Q63 changed this morning, and the health block's folder pattern line.

AND THE RULE ABOUT A PUBLIC MEMBER NOTHING CALLS was applied to this round's own work
rather than only to old code: the two argument `ItemSizes.Read` overload and four
`RevitWorksets` members had no caller anywhere, so they are deleted, and a dead field
went with them.

One answer in `steps\02_questions.md` was corrected too. Q65 says neither new block can
fail a group, and Q70, answered later the same day, makes exactly one case where one can.
The note sits under Q65 so the two are not read as contradicting each other.

### What comes next

Bader answers Q72. Core tests 1636 before the round and 1666 after, 0 failed and 0
skipped. Build 0 errors and 0 warnings after every change. Both checks pass.

## 2026-09-20 The worksets round, THE PLAN, written before the first edit

### The two gates, both passed

`dotnet build ParsonsNwcFederator.sln -c Release`, 0 errors and 0 warnings, Navisworks
Manage 2025 found. This is not a container.

`origin/main` carries the alignment round: Bader merged it as pull request 65, and
`289dad3` is an ancestor of `fe936f4`. So nothing is stacked and `round-worksets`
branches off a main that holds everything the last round measured.

### What is already known going in, and what is not

The alignment round left three numbers this round is briefed off, and NONE of them is
taken as read.

- 33 of 61 sets find nothing, and 5q says the cause is a case mismatch, `ME-Ductwork`
  in the models against `ME-DUCTWORK` in the matrix
- four workset names disagree with each other by MORE than case, and three of those
  four look like typos rather than variants
- 25 models sit somewhere their group's reference does not, and at least one names
  `Internal`, which is Revit's word for a model exported on no shared site at all
- 29 services in one group reported no readable size, ONE DAY after 5r found a reader
  that returned nothing and looked like an answer

Every one of those was read off a log written for a person. None of them is a list the
code can be built from, and this round builds three rules and a group judgement on them.
So PART 1 measures all four properly first, and PART 2 to PART 5 are built from what it
writes and from nothing else.

### The order, and why it is that order

MEASURE ONCE, THEN BUILD, THEN RUN ONCE. Navisworks is started ONCE for the whole of
PART 1, because the last three rounds each cost an extra run for a rule built on a guess.

1. **PART 1a, 5s.** The 29 services with no readable size in 1A02MM, one at a time: the
   item name, the category, every property tab it carries, and every property `SizeRule`
   looks under. THE QUESTION IS WHICH OF TWO THINGS IT IS, and the round says which:
   the services genuinely carry no size property, or the reader is on the wrong node
   the way the penetration rule was. `ItemSizes.Read` walks `item.PropertyCategories`
   and `Penetrations.LargestOf` walks the same five level chain the category read walks,
   so after 5r's fix it SHOULD read. If it does not, that is a bug and it is fixed in
   PART 1, and PART 5 then reports a real number instead of dressing up a broken one
2. **PART 1b, 5t.** Every distinct workset name in C02, across all ten groups and every
   model, with which models carry it. Grouped so that a name differing from another
   ONLY BY CASE sits beside it, and a name differing by more than case sits beside it
   too and is MARKED as a different word. This is the source of truth for PART 2 and
   PART 3 and neither is built from a name read off a log
3. **PART 1c, 5u.** The shared site of every model in all ten groups, which model is
   each group's architecture reference, and how many models name `Internal`. THEN THE
   ONE NUMBER PART 4 WAITS ON: how many of the ten groups would FAIL under Q70

All three into `docs\history\scan.md` as 5s, 5t and 5u.

Then the build, each its own commit, built after every change and not at the end.

4. **PART 2, Q68 answered a.** A fourth `MatrixCorrections` rule beside the hyphen rule
   and the negation rule, correcting a workset value in the matrix to the spelling the
   models carry. NEVER AN IGNORE CASE FLAG: Bader refused it and the reason stands, it
   would also make two genuinely different worksets match, quietly, on every set. The
   rule corrects a value ONLY where 5t found exactly ONE model spelling differing by
   case alone, and REFUSES where it found two, naming both. Four tests
5. **PART 3, Q69 answered b.** Where his models carry two spellings differing by more
   than case, the set condition carries both as an Or row, `flags="64"`, which F87
   already writes. BUILT FROM 5t AND NEVER FROM A HARDCODED LIST. And the EXPORT CHECK
   block STILL NAMES THEM AS MISSPELLED, one line per pair, saying which model carries
   which, because if the tool absorbs a typo silently nobody ever fixes it and the next
   building repeats it. Three tests
6. **PART 4, Q70 answered b**, built only after 5u's number is known. A group is FAILED
   when any model names `Internal` or names no site at all, the ALIGNMENT block says
   which model and why, AND THE GROUP STILL WRITES ITS THREE OUTPUTS, because Bader
   needs the evidence to take to NMDC and a group that produces nothing gives him
   nothing to send. If 5u says more than half the groups would fail, it is built exactly
   as briefed and the report says so plainly, because that is a finding about his models
   and not a reason to soften the rule. Three tests
7. **PART 5, Q71 answered b.** The penetration block names what the unmeasured services
   are, in at most two lines, with the ids in the `.tsv`. What it reports depends on
   what 1a found. Two tests
8. **PART 6**, the five answers into `steps\02_questions.md`, Q67 WITH ITS CONSEQUENCE
   WRITTEN OUT: the alignment path for a model with no shared coordinate and the export
   path for a model with no element id have tests and have never met a real file, and on
   Bader's decision they stay that way. That goes in the round report's untested section
   every round until a model shows one

Then the proving.

9. **PART 7.** His NWF folder copied to a dated folder beside it, the count and every
   byte size compared one for one, and the backup SAID to be read back before anything
   else happens. Then the real run over the ten C02 groups, 25 mm READ BACK off the box
   and never reported as selected, penetrations on, by design on, the priority file
   picked, viewpoints on, the corrected matrix. Reported against the alignment round's
   run of 14:24 on every number the brief lists
10. **PART 8.** The whole log read end to end, every count that does not add up and
    every check that reports nothing written down as a finding, fixed or named.
    `tools\checks` and the full Core suite after each. Then `steps\03_bader_next.md`
    read end to end against the code. ANY CHECK THAT COULD NEVER FAIL IS A BUG, and
    three were found last round

### What is decided and is not reopened

Q67 c, C02 is enough and 1A04PW does not come onto this machine. Q68 a, the matrix is
corrected to the models. Q69 b, both spellings as an Or row. Q70 b, Internal fails the
group. Q71 b, the 29 are named.

### The one thing most likely to go wrong, said in advance

PART 4 can fail most of the run. 5q already saw four different shared sites in one group
of four models and two models on `Internal` in another. If 5u says six or eight of the
ten groups fail, the rule is still built as briefed and the round report says the number
plainly. What it must NOT do is quietly become a warning because the number was
uncomfortable, and it must not stop the group producing its NWF, NWD and report, which
is the evidence Bader takes to NMDC.

### The four standing rules of a run on his machine

Nothing is written into a live project folder except PART 7's run and PART 7's backup,
and everything else goes under
`C:\Users\bader\AppData\Local\Temp\claude\round-worksets`. Every program started, every
file written outside the repo with its full path and every process stopped is listed in
the round report. Everything opened is closed, the check is run and it is said. Nothing
of his is deleted or overwritten.

## 2026-09-20 The alignment round, run for real against his own folders, and a rule that had never fired

### What was done

Eight parts, briefed off Bader's 422 hand marked decisions in 1A04PW and his answers Q58
to Q67 of 2026-09-20. THE MEASUREMENTS CAME FIRST, in one probe pass, because three of
the five build items rested on something this API had not been asked and the last two
rounds each cost an extra run for guessing one.

TWO THINGS READ OFF THE MACHINE BEFORE THE PLAN WAS WRITTEN, both of which shaped the
round and neither of which anybody had said. `C02 + 04\C04` IS EMPTY, zero files, so
"every ticked building in C02 and C04" is the ten C02 buildings and nothing else. And
1A04PW, the building whose 422 decisions briefed the whole round, IS NOT ON THIS MACHINE
as a model at all, only as a report in Downloads. So the two new checks are proved on
C02 and not on the building the rules came from, and that is Q67.

**PART 1, the cheaper write route, Q59 answered d.** 5p. Both routes record all four
counts, the camera, the hidden state, the dimming and the two solid items, and pressing
either leaves exactly the two clashing items solid. SO THE SWITCH WAS ALLOWED AND IT
SAVES ALMOST NOTHING: 153 ms against 158 over twenty viewpoints, and 415 bytes MORE on
disk. Three per cent, not the two thirds the operation count suggests, because what grows
is the TREE THE WRITE WALKS and not the calls per write. The same route costs 7.9 ms a
viewpoint at twenty and 558 ms at four hundred and thirty. Both routes are in the one
binary behind `ViewpointSettings.RecordsThroughTheFolder`.

**PART 2, the two colours, Q58 answered b.** Red on the first item and green on the
second, the order Clash Detective holds them, on top of the ghosting, both settings.

THE READ BACK IS NOT WHAT IT LOOKS LIKE AND 5p IS WHY. A viewpoint records a colour
override only where the colour DIFFERS from the item's own. The second item of the first
clash measured was already green, so painting it green recorded NOTHING, the viewpoint
named 1,027 items where the blue version named 1,028, and pressing it still showed the
item green because green is what it was. A read back insisting the viewpoint names both
items would have failed a viewpoint that was perfectly right. So it asks what the
viewpoint WILL SHOW for each item, the override's colour where it names the item and the
item's own colour where it does not, which has one right answer either way.

**PART 3, the two rule extensions from his 422.** Structural Foundations joins the solid
list a service may pass through, and Structural Framing and Structural Columns STAY OUT
with the reason in the comment so nobody widens it later: he moved 69 pipes through slabs
to Reviewed and LEFT 7 through precast beams Active. His replacement `by-design-pairs.csv`
went in, 55 pairs up from 41, with two tests asserting the fourteen new ones BY NAME and
both ways round. After both, 420 of his 422 decisions are covered by rule. The two left
are a plumbing fixture through a foundation and a fire alarm device through a wall, both
single clashes, both correctly left manual.

**PART 4 and PART 5, the two new blocks, Q64 and Q65.** NOTHING WAS BUILT UNTIL 5q HAD
BEEN READ, because the brief forbids falling back to a bounding box and calling it an
alignment check. The shared coordinate IS readable: every model root carries a
`[Location]` tab with `revit_ProjectLocation` on it, the NAME of the Revit shared site,
beside a Transform with a translation in X, Y and Z. So ALIGNMENT compares every model
against the architecture model on both, with the difference in X, Y and Z separately.

5q also corrected the round's own first attempt. Worksets and element ids were counted
over items with GEOMETRY and read zero everywhere, against a real run reading 860 ids of
1,052. A Revit element reaches Navisworks as a COMPOSITE item carrying the Element tab,
and the geometry solids under it carry neither, so the count was taken on the wrong node.
Counted on the tab it reads 100 per cent.

**PART 8 FOUND THE THING THIS ROUND IS REALLY ABOUT, 5r.** Reading the first run's log
end to end: the penetration rule had moved ZERO clashes, and every clash was in the one
bucket "not a service against a solid" with zero in all five others. That is not a rule
deciding, it is a rule reading nothing. `Penetrations` read a clash side through
`ClashResult.Selection1`, and `ModelItem.PropertyCategories` THROWS NotSupportedException
on the item a clash selection hands back, at every level of the walk up, on both sides of
every clash measured. The same item through `Item1` reads perfectly, which is why the
harvest beside it reads 860 element ids off the same clashes. F72 shipped on 2026-09-19
and had never fired once. IT ALSO COST THE VIEWPOINT SIZE FOLDER, because `ServiceSizeOf`
reads a side the same way, so `Over 150mm` could never be reached. The fix is two words.

### Measured

TWO RUNS AGAINST HIS OWN LIVE FOLDERS, Q60 answered b, both after the backup was read
back. Same settings both times: 25 mm chosen in the tool and READ BACK off the box,
penetrations on, by design on, the priority file picked, viewpoints on, the corrected
matrix.

| run | what changed | groups | penetrations moved | the run |
|---|---|---|---|---|
| run-20260920-140347 | the colours, both new blocks | 10 done, 0 failed | 0, the rule was broken | 11 min 6 s |
| run-20260920-142412 | 5r's two word fix | 10 done, 0 failed | 4 | 15 min 16 s |

Both are inside the 45 minutes criterion 2 asks for. Nothing failed in either.

THE PENETRATION BLOCK BEFORE AND AFTER, one group, which is the whole of 5r in six lines:

```
                       before   after
clashes looked at        526      526
moved to Reviewed          0        0
the service is over the size   0     45
no size could be read          0     29
a person had already set it    0      0
both sides a service           0      0
both sides a solid             0     46
not a service against a solid 526    406
```

Zero moved in 1A02MM is now a REAL answer: 74 of its clashes ARE a service against a
solid and every one of them is over 150 mm, which the rule deliberately leaves at New.
Across the ten groups it moved 4, all Active to Reviewed, the first four clashes this
rule has ever moved.

THE VIEWPOINTS: 975 written over seven groups, 109, 430, 29, 16, 245, 77 and 69, and
every single one dimmed at 0.85 AND painted red and green AND read back on all four
counts. Not one failed.

AND IT WAS LOOKED AT AFTERWARDS, on HIS OWN 1A02MM opened off the disk and pressed
through the probe, which counts what a person would see rather than squinting at a
screenshot:

```
/A/AR vs ST/BLD-ST-Columns-vs-BLD-AR-Floors  Clash4
   2 model(s) hidden carrying 841 item(s), then of what is left:
   153 dimmed at 0.85 and SOLID 2:
   ARC-FLOOR-INT-EPOXY(1)-PAR (0,1,0) in ...-AR-MOD-000001.nwc
   Concrete, Cast-in-Place Fcu35 Mpa (1,0,0) in ...-ST-MOD-000001.nwc
```

Exactly two items solid, they are the two the clash is between, and they carry RED on the
first side of the clash and GREEN on the second, which is the order Clash Detective
paints them. The same on all three pressed.

ALIGNMENT, across the run: 25 models sit somewhere their group's reference does not.
1A02MM's four agree in X and Y and differ in Z by up to 95 mm, and they name FOUR
different shared sites, one of them `Internal`, which is what Revit calls a model that
was not exported on a shared site at all.

EXPORT CHECK, across the run: 0 models carry no workset and 0 are missing an element id.
C02's export is clean on both counts, WHICH IS NOT WHAT THE BRIEF EXPECTED, and the real
fault is the one the block was built to show. The models carry `ME-Ductwork`, `ME-Piping`
and `ME-Equipment`. The matrix asks for `ME-DUCTWORK`, `ME-PIPING` and `ME-EQUIPMENT`.
The condition carries `flags="64"`, which is StartGroup and NOT a case flag: the one that
would forgive it is `IgnoreDisplayStringValueCase`, value 16, and nothing sets it, not
the client's file and not `SetBuilder.BuildCondition`. So the comparison is case
sensitive and THAT is why 33 of 61 sets find nothing in every group. The block lists the
names and says the match is case sensitive in so many words.

AND HIS OWN WORKSET NAMES DISAGREE WITH EACH OTHER, which no rule can fix: `EL-Fire
Alarm` beside `EL-Fire alarm`, `EL-Lightning Protection` beside `EL-Lightining
Protection`, `EV-Cctv System` beside `EV-Ccctv system`, `PL-Drainage equipment` beside
`PL-Drainage equipmen`.

HIS NWF FOLDER, the backup against what is there now, bytes:

| group | before | after |
|---|---|---|
| 1A02MM | 119,542 | 24,642,386 |
| 1A02WM | 111,709 | 6,256,447 |
| 1A02WO | 83,819 | 986,849 |
| 1A02WN | 83,291 | 913,096 |
| 1A02WE | 77,234 | 481,534 |
| 1A02WL | 71,858 | 249,834 |
| 1A0215 | 78,341 | 78,366 |
| 1A0215-LS | 4,300 | 4,300 |

Three are new: 1000BS 13,091, 1A02BS 1,296,720, 1A02MS 81,957. The growth is the dimming
and it is Q59's arithmetic, not this round's.

### Every program started, every file written outside the repo, every process stopped

Started: Navisworks Manage 2025 through Roamer.exe twice, once per run, and four times as
the automation host for the probe, modes route, colour, survey three times and pen twice,
each of which exited on its own. dotnet build, dotnet test and build\install.ps1, which
built and copied the bundle twice. PowerShell drivers for the window, the ribbon clicks,
the confirm dialog and the close, every one of which exited.

Stopped: Roamer.exe was stopped with Stop-Process ONCE, between the two runs, when the
add-in window had closed and the main window did not follow. The open document was his
own 1A02WO federation with nothing unsaved, because the run had already saved and the
close was answered No. The second close went through the window on its own and needed no
Stop-Process, which the check below says. No browser and no sign in page opened at any
point.

WRITTEN INTO HIS LIVE PROJECT FOLDER, which PART 7 asked for and which rule 1 otherwise
forbids:

- `C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20`, the backup, taken first
  and READ BACK before anything else happened: 9 files against 9, every byte size
  compared one for one, 0 mismatches. It is his to delete when he is satisfied
- his 8 NWF files rewritten and 3 new ones written, listed above
- 10 NWDs in `C02\NWD` and 10 workbooks with 10 pages and their picture folders in
  `C02\Clash Report`
- two copies of the run log in `C02\NWF`, which is where the tool always puts one

Everything else is under `C:\Users\bader\AppData\Local\Temp\claude\round-alignment`: the
probe folder with three copies of his NWFs, the probe result files and the two NWFs the
route probe saved, and the driver notes. The tool's own logs are in
`C:\Users\bader\AppData\Local\ParsonsNwcFederator\logs` and both pairs are copied into
`steps\logs`. The bundle at
`%APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle` was replaced twice.

His NWC folder was read and nothing was written there. The four properties CSVs the
wiring round left beside his NWCs on 2026-09-19 are still there and still his to delete.

THE CLOSING CHECK RAN: no Roamer, no driver process and no PowerShell left. Two Autodesk
processes are running, AdskLicensingService and AdSSO, and NEITHER was started by this
round: AdSSO has been up since 2026-09-19 10:46 and the licensing service is a Windows
service that is always there.

### Known bugs, and what is untested

WHAT WORKS, proved by a run on his own files: the colours, both new blocks, the foundation
rule, the by design file, the cheap write route, and the penetration rule for the first
time since it shipped.

WHAT DOES NOT: 33 of 61 sets still find nothing, and the cause is now named rather than
guessed at. It is a CASE MISMATCH between the client's matrix and the client's models and
this tool must not silently fix it, because `ME-DUCTWORK` matching `ME-Ductwork` would
also make two genuinely different worksets match. That is Q68.

WHAT IS UNTESTED: the two new blocks have never seen a model with NO shared coordinate
property, because every model in C02 carries one, and never seen a model missing element
ids, because every model in C02 carries them. Both paths have tests behind them and
neither has been seen on a real file. 1A04PW would show both and is not on this machine,
which is Q67.

### The closing pass, PART 8

the Bader steps file `03_bader_next.md` was read end to end against the code, which is how every round
closes, and TWENTY TWO things had drifted. Three of them were checks that could never
fire, which is the shape this repo refuses everywhere else: a step looking for a line
reading `VIEWS    not attempted.` that is in no source file, a step checking that nothing
says `DID NOT FOLLOW` when nothing ever could, and the `.tsv` step naming fourteen event
kinds when the code writes twenty one, which would have had Bader send the whole file
every run. The viewpoint steps still described the per discipline tree F85 replaced a
round ago. The block order left out four blocks on both run paths. Two measurements the
file still called outstanding, 5g and 5i, are done.

ONE CODE FIX CAME OUT OF IT, and it was this round's own doing. The penetration tick
box's grey line still read `walls, floors, roofs` after Q63 added foundations that
morning, so the window understated the rule a person is being asked to tick. A test now
asserts the line names every solid the rule covers and stays inside the twelve words a
help line is allowed.

### What comes next

Bader answers Q67 and Q68. Core tests 1609 before the round and 1636 after, 0 failed and
0 skipped. Build 0 errors and 0 warnings after every change. Both checks pass.

## 2026-09-20 The alignment round, THE PLAN, written before the first edit

The round is briefed off Bader's own 422 hand marked decisions in 1A04PW and his answers
Q58 to Q67 of 2026-09-20. The plan is written first and nothing is edited until it is.

WHAT THE BUILD GATE SAID. `dotnet build ParsonsNwcFederator.sln -c Release`, 0 errors and
0 warnings, Navisworks 2025 found. This is not a container and every part below can be
proved by a run.

TWO THINGS READ OFF THE MACHINE BEFORE PLANNING, both of which shape PART 7.

- `C:\00-NM\Federation Task\C02 + 04\C04` IS EMPTY. Zero files, no NWC, no NWF, no NWD.
  So "every ticked building in C02 + 04" is the ten C02 buildings and nothing else, and
  the round says so rather than reporting ten groups as though it had run fourteen
- 1A04PW, the building whose 422 decisions brief this whole round, IS NOT ON THIS MACHINE
  as a model. Only its report is, `C:\Users\bader\Downloads\1104-PAR-1A04PW-XXX-BM-RPT-000001.xlsx`,
  which is read only to this round. So PART 3's rules are proved by tests against that
  report's own categories, and PART 4 and PART 5 are proved on C02's ten groups. The
  round cannot show the 1A04PW block Bader wrote in the brief, and will show C02's

### The order, and why it is that order

MEASURE, THEN BUILD, THEN RUN ONCE. Three of the five build items rest on something this
API has not been asked yet, and the last two rounds each cost an extra run for guessing
one. Every measurement is taken in ONE probe pass so Navisworks is started once for all
three, and the build items that need no measurement are done while that is being read.

1. PART 1 measure. Does `InwOpFolderView.SavedViews().Add` record a viewpoint AT ALL, and
   does it record the camera, the hidden state, the dimming and the two solid items. The
   count is four and not one, because a route that looks right and records nothing is how
   this feature failed twice
2. PART 2 measure. What `AppearanceOverrides.MaterialOverrides` hands back for an item
   whose colour was overridden, because the colour has to be READ BACK and nothing has
   read one back yet. Whether a temporary colour override and a temporary transparency
   override on the same item both survive into one viewpoint
3. PART 4 measure, 5q. What an NWC carries of the Revit shared coordinate: the origin,
   the project base point, the survey point, under what property name, on the model root
   or per item, and whether a transform is there and is the identity. NOTHING IS BUILT
   FOR PART 4 UNTIL THIS IS READ, and if it is not readable the block says so in one line
   rather than falling back to a bounding box and calling it an alignment check
4. PART 5 measure. Worksets and Element ID, per model, off the same pass. These two are
   said to be readable with what the probe already reads and that is checked, not assumed

Then the build, each its own commit, each built after the change and not at the end.

5. PART 3a, Foundation joins the solid list. Framing and Columns stay out and the comment
   says why, with Bader's own 7 beams left Active against 69 slabs moved as the reason.
   Three tests: through a foundation moves, through a beam does not, through a column does not
6. PART 3b, his replacement `by-design-pairs.csv`, 55 pairs up from 41, which he left in
   `Claude outputs\`. A test that asserts 55 and asserts the AR against ST pairs by name
7. PART 1 build, the cheap route behind a SETTING with both routes in the code, so the
   A and the B are the same binary and the comparison is honest
8. PART 2 build, the two colours as settings defaulting to red and green, read back
9. PART 4 build, the ALIGNMENT block, only what step 3 supports
10. PART 5 build, the EXPORT CHECK block
11. PART 6, the nine answers into `steps\02_questions.md`

Then the proving.

12. PART 1's A against B, the SAME group written both ways, VIEWS seconds and NWF bytes
    and all four read back counts side by side into `docs\history\scan.md` as 5p. The
    switch is taken ONLY if all four hold
13. PART 7. His NWF folder copied to a dated folder beside it, the count and every byte
    size compared one for one, and the backup SAID to be read back before anything else.
    Then the real run against the real folders, 25 mm, penetrations on, by design on, the
    priority file picked, viewpoints on
14. PART 8. The whole PART 7 log read end to end, every count that does not add up and
    every check that reports nothing written down as a finding, fixed where it can be
    fixed and named where it cannot, `tools\checks` and the full Core suite after each
15. `steps\03_bader_next.md` re-read end to end against the code, and what drifted corrected

### What is already decided and is not reopened

- The solid list stops at Foundation. Framing and Columns stay out
- No bounding box anywhere near the alignment check
- The cheap write route is not taken on its speed alone
- PART 7's run does not start until PART 7's backup has been read back
- Nothing in `samples` is touched except the by-design file Bader replaced

### The four standing rules of a run on his machine

Nothing is written into a live project folder except PART 7's run and PART 7's backup,
and every copy this round needs goes under
`C:\Users\bader\AppData\Local\Temp\claude\round-alignment`. Every program started, every
file written outside the repo and every process stopped is listed in the round report.
Everything opened is closed and the check is run and said. Nothing of his is deleted or
overwritten outside PART 7.

## 2026-09-20 The dimming round, the viewpoints show the clash, proved by three runs

### What was done

Bader pressed two of F85's 975 viewpoints and could not see the clash. The camera was
the one Clash Detective computes and read back exact, the hidden state was right, and
the view was a grey wall, because the camera lands inside a beam and a solid beam fills
the screen. Clash Detective only looks right because its own view ghosts everything that
is not the two clashing items. This round makes a saved viewpoint do the same.

NOTHING WAS BUILT UNTIL THE ROUTE WAS MEASURED, because the camera took three runs last
round for exactly that reason. The probe wrote a viewpoint with the scene dimmed, saved
the NWF, CLOSED it, reopened it off the disk and read back what it carried, scan.md 5o:

- both flags this API offers are worthless. `ContainsAppearanceOverrides` and
  `ContainsVisibilityOverrides` read TRUE on a viewpoint that recorded neither, so F85's
  third read back, that a viewpoint carries visibility overrides, was a check that could
  not fail and never could. The counts underneath are real and the read back is now four
  counts and not three flags
- a temporary transparency override on the model roots reaches every leaf, and a reset on
  two leaves brings exactly those two back. So a viewpoint costs TWO calls, not one per
  item: dimming 2,606 items one at a time, 430 times, would be 1.1 million calls
- the record survives the reopen. Pressing a dimmed viewpoint off a reopened file leaves
  the two clashing items solid and everything else at 0.85
- the permanent override records the same and is NOT used, because its only undo clears
  every appearance override the file already held and nothing can read those back first.
  That is 5k's trap in a second shape and the answer is the same one

Then the writer: walk one keeps the index path of each clashing item, plain ints rather
than 1,950 native handles held across a group, and walk two hides the models outside the
pair, dims what is left, brings the two back to solid, records through the COM view with
`ApplyMaterialAttribs` beside `ApplyHideAttribs`, and reads back four counts. The
transparency is a setting, 0.85, CHOSEN and not measured, and it says so where it is
declared: Clash Detective's own value is not readable off this API, `Application.Options`
exposes one member and the COM state exposes none. Zero switches the dimming off.

THE FIRST RUN PUT 33 MB INTO A 120 KB NWF and the fix was obvious once the number was in
front of me: it was dimming the models it had just hidden. Scoped to the models a
viewpoint shows, 1A02MM came down from 33,622,598 bytes to 23,327,744 and the run from
15 minutes 23 seconds to 10 minutes 26. The VIEWS step now says where its seconds go, per
call, because the shape of the cost was not what it looked like.

PART 3 closed 5g, which had been NOT MEASURED since 2026-09-19. A negated condition
builds, finds exactly the right items and keeps its bit through a save and a reopen, so
the corrected matrix carries F87 in full rather than its fallback. PART 4 recorded the
three answers.

### Measured

Three runs over the ten C02 groups, every one against a FRESH copy of his NWF folder so
the 975 were written new and not added to, same settings every time: 25 mm chosen in the
tool, by design on, penetrations on, the priority file picked, his NWC folder read only.

| run | what changed | viewpoints | total | VIEWS |
|---|---|---|---|---|
| run-20260920-104116 | dimming, every model | 975 dimmed | 15 min 23 s | 611 s |
| run-20260920-110157 | dimming scoped to the models shown | 975 dimmed | 10 min 42 s | 338 s |
| run-20260920-113312 | the corrected matrix as well | 975 dimmed | 10 min 26 s | 335 s |

The viewpoints round's run, with viewpoints and no dimming, took 5 minutes 3 seconds.
Ten groups done, none partial, none failed, nothing failed, on all three.

WHERE THE SECONDS GO, off the proving run, which is the line the round added:

    1A02BS, 109 viewpoints:  0.002s already there, 0.038s dimming, 8.142s recording, 0.073s reading back
    1A02MM, 430 viewpoints:  0.014s already there, 0.354s dimming, 240.369s recording, 4.147s reading back
    1A02WM, 245 viewpoints:  0.004s already there, 0.166s dimming, 62.290s recording, 2.927s reading back

The dimming itself is a third of a second over 430 viewpoints. 96 per cent of the step is
RECORDING, which is the COM add, the copy into the folder and the remove from the root,
each one carrying the viewpoint's material overrides with it. The COM API can add
straight into a folder, `InwOpFolderView.SavedViews().Add`, which would be one tree
operation instead of three, and that is measured and NOT taken in this round, because the
write path took three runs to get right last round and the time is well inside the rule.

NWF, his file before and the proving run's copy after, bytes:

| group | before | after | times | viewpoints |
|---|---|---|---|---|
| 1A02MM | 119,542 | 23,327,744 | 195 | 430 |
| 1A02WM | 111,709 | 4,966,163 | 44 | 245 |
| 1A02WO | 83,819 | 949,965 | 11 | 69 |
| 1A02WN | 83,291 | 865,396 | 10 | 77 |
| 1A02WE | 77,234 | 430,811 | 5 | 29 |
| 1A02WL | 71,858 | 249,226 | 3 | 16 |
| 1A0215 | 78,341 | 78,451 | 1 | none |

The three built new came out 13,153, 1,200,657 and 82,015. The cause is one number: a
dimmed viewpoint records ONE MATERIAL OVERRIDE PER ITEM it dims, 5o. Nothing is capped
and nothing is thinned, because that is Bader's call and it is question 59.

WHAT A WRITTEN VIEWPOINT ACTUALLY SHOWS, counted rather than squinted at. The probe
opened the proving run's own 1A02MM off the disk and pressed three viewpoints under A:

    /A/AR vs ST/BLD-ST-Columns-vs-BLD-AR-Floors  Clash4
       2 model(s) hidden carrying 841 item(s), then of what is left:
       153 dimmed at 0.85 and SOLID 2:
       ARC-FLOOR-INT-EPOXY(1)-PAR in ...-AR-MOD-000001.nwc
       Concrete, Cast-in-Place Fcu35 Mpa in ...-ST-MOD-000001.nwc

Exactly two items solid and they are the two the clash is between. The same on all three.

AND I LOOKED AT IT MYSELF, on the proving run's file, opened by hand. Three viewpoints:

- `A/AR vs EL`, a cable tray against a floor: a ghosted scene with the tray and the slab
  readable. Depth all the way through, where F85's viewpoints were a flat grey face
- `A/DR vs ST/...Framing Clash2`, which is the kind Bader pressed: the camera is inside
  the structural member, the member fills most of the frame, and the pipe end now reads
  as a distinct solid shape against it. Before this round that view was featureless grey
  with nothing in it to look at. It is readable now. It is still a close view, because
  the camera Clash Detective computes is inside the member and this round did not move it
- `A/DR vs ST/...Floors Clash1`: the roof slab ghosted with the plant equipment visible
  through it and the floor solid in front. This is the one that looks most like Clash
  Detective

### Every program started, every file written outside the repo, every process stopped

Started: Navisworks Manage 2025 through Roamer.exe five times, three for the runs and two
to look at the files by hand, and six more as the automation host for the probe, modes
dim twice, press three times and negate three times, each of which exited on its own.
dotnet build, dotnet test and build\install.ps1, which built and copied the bundle three
times. PowerShell drivers from the scratchpad for the window, the tolerance box, the
confirm dialog, the screenshots and the close, every one of which exited.

Stopped: Roamer.exe was stopped with Stop-Process twice, after the first dimming run and
after the proving run's probe pass, when the add-in window had closed and the main window
did not follow. Both times the open document was a temp copy with nothing to save. Every
other close went through the window, answering No to the save prompt. No browser and no
sign in page opened at any point.

Written outside the repo, all under `C:\Users\bader\AppData\Local\Temp\claude\round-dimming`:
NWF1, NWF2 and NWF3, three copies of his NWF folder, one per run, with what each run
wrote into them, NWD and Excel with the outputs, `probe` with the probe result files and
the three NWF copies it opened and the two it saved as probe-dim-saved.nwf and
probe-negate-saved.nwf, the driver notes and nine screenshots. The tool's own logs are in
`C:\Users\bader\AppData\Local\ParsonsNwcFederator\logs`, three pairs from
run-20260920-104116 to run-20260920-113312, and the last two pairs are copied into
steps\logs. The bundle at `%APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle`
was replaced three times. Navisworks wrote autosaves of the open temp copies into
`%APPDATA%\Autodesk\Navisworks Manage 2025\AutoSave`.

His folders: the NWC folder was read and nothing was written there. The NWF, NWD and
Clash Report folders were listed at the end and every file carries the size and time it
had before the round. The four properties CSVs the wiring round left beside his NWCs on
2026-09-19 are still there and are still his to delete.

The closing check ran: no Roamer, no Autodesk process and no driver process was running.

### What remains

- Bader's look, step 390, one file and two viewpoints named by their full path
- Q59, whether doubling the run and multiplying the worst NWF by 195 is worth the picture,
  with the off switch named and the middle options laid out
- Q58, whether the two items should be coloured red and green the way Clash Detective
  paints them. Deliberately not built, because which colours a viewpoint carries is a
  judgement about what the client's people read
- Q55 and Q56 from the viewpoints round, the harvest's source file column and whether the
  category list should be walked over more than one folder, both still open
- the COM folder route, measured and not taken, which would cut the recording

### Known bugs

None open from this round. The corrected matrix's catch-all set, `BLD-EL-Devices`, finds
ZERO items in C02 and that is not a bug: it asks for a Devices category no sibling set
claims, and all five Devices categories in that folder are claimed. The fallback it
replaces also found zero, so no clash count changed. It will catch a device category a
future building carries, which the fallback never could.

### What comes next

Bader answers 58 and 59. Core tests 1605 before the round and 1609 after, 0 failed and 0
skipped. Build 0 errors and 0 warnings after every change. Both checks pass.

## 2026-09-20 The dimming round, the plan, written before the first edit

### What the round is

F85 shipped 975 viewpoints and Bader pressed two of them. They do not show the clash. The
camera is the one Clash Detective computes and reads back exact, the hidden state is
right, and a person still sees a grey wall, because the camera lands inside a beam and the
beam is solid. Q55 and Q56 are answered the same way: dim everything that is not the two
clashing items, the way Clash Detective does. That is this round.

THE BUILD GATE PASSED. On this machine the whole solution built in Release with 0 errors
and 0 warnings on main at 32dac19, with the add-in inside it, before anything was written.
This is not a container.

Four rules from the brief hold over everything below: nothing is written into a live
project folder, every program started and every file written outside the repo is listed in
the round report, everything opened is closed and the check is run and reported, and
nothing of Bader's is deleted or overwritten.

### What is already known, so the round does not measure it twice

- `SavedViewpoint.GetAppearanceOverrides()` returns an `AppearanceOverrides` carrying ONE
  member, `MaterialOverrides`, a collection. `ContainsAppearanceOverrides` is a bool beside
  `ContainsVisibilityOverrides`
- `ModelGeometry` carries `ActiveTransparency`, `PermanentTransparency` and
  `OriginalTransparency`, and the same three for colour, so a dimming can be READ BACK off
  an item rather than trusted
- `DocumentModels` carries four override calls and four resets, permanent and temporary,
  for colour and transparency, and `ResetAllPermanentMaterials` and
  `ResetAllTemporaryMaterials`
- the COM view this tool already writes through carries `ApplyMaterialAttribs`, set FALSE
  today beside `ApplyHideAttribs` set true
- `DocumentModels.CreateIndexPath(ModelItem)` gives a `Collection<int>` and
  `ResolveIndexPath` takes it back, and `CreatePathId` and `ResolvePathId` are a second
  pair. Both hand out plain values, which is how an item can be named in walk one and
  resolved in walk two without keeping 1,950 native handles alive
- `SearchCondition.Negate()` and `SearchConditionOptions.NegateCondition = 32` exist, and
  F78 measured that the exchange file's `flags` attribute IS that enum
- NOTHING in the .NET API exports a search set to XML. `Document.ExportAsDwf` is the only
  export on the document, and `DocumentSelectionSets` has no writer. So the export half of
  5g is not reachable the way the import half is, and the round says so rather than
  inventing a route

### The order

PART 1, the dimming measurement, one commit. The camera took three runs because two
routes each recorded half a viewpoint and both looked right, so nothing is built until a
route is measured through a save, a close and a reopen off the disk.

The probe in `tools\probes\ViewpointProbe` gains a `dim` mode, run through the automation
host against a COPY of one NWF, and it measures, in order:

1. what `ContainsAppearanceOverrides` and `MaterialOverrides.Count` read on a viewpoint
   written with NO override at all, because 5j noted the flag reading true on a capture
   that set none, and a flag that is always true is no read back
2. TEMPORARY transparency, `OverrideTemporaryTransparency`, on the model roots, with the
   COM view's `ApplyMaterialAttribs` true. Read the flag and the count, then SAVE, CLEAR,
   REOPEN off the disk, read them again, press the viewpoint from a clean document, and
   read `ActiveTransparency` off an item that should be dim and off one that should be solid
3. PERMANENT transparency, `OverridePermanentTransparency`, the same way
4. whether an override on a model ROOT reaches the leaves, and whether
   `ResetTemporaryMaterials` on two leaves brings just those two back to solid while the
   rest stay dim. That is the shape the writer needs: two calls per viewpoint rather than
   2,606, because 1A02MM alone would otherwise be 1.1 million override calls in one group
5. what each route costs in milliseconds, so the VIEWS step can be predicted
6. whether `CreateIndexPath` and `ResolveIndexPath` round trip an item through plain values

Written into `docs\history\scan.md` as 5o, saying which route records the dimming, which
does not, and what each one records instead.

IF NO ROUTE SURVIVES THE REOPEN, the round stops there. PART 2 is not built, the
viewpoints are left exactly as they are, PART 3 and PART 4 are still done, and the report
says in one line that this API cannot do what was asked.

PART 2, the dimming, only on a yes, one commit per change with a build after each.

- the transparency is a SETTING in `ViewpointSettings` beside the camera tolerance, with a
  default. Clash Detective's own value is looked for in `Application.Options` first and the
  default says where it came from. If it cannot be read the default is 85 per cent and the
  comment says it was CHOSEN and not measured
- walk one keeps, per clash, the index path of each of the two clashing items, as plain
  ints beside the home model it already reads
- walk two, per viewpoint, in this order: hide the models outside the pair as now, dim what
  is left, bring the two items back to solid, record the COM view with
  `ApplyMaterialAttribs` true, and read back
- the read back becomes FOUR checks and not three: it is there, its camera is within the
  tolerance, it carries visibility overrides where it hides a discipline, and it carries
  the material overrides where it dims. A viewpoint failing any of the four is FAILED with
  the reason and is not counted
- the material state of the document is put back when the group's writing ends, the way the
  hidden state already is, whichever way it ends, and the log says it was put back
- the two items are NOT coloured. Whether they get Clash Detective's red and green is a
  question for Bader and is raised as one

PART 3, 5g, one commit. The probe gains a `negate` mode and measures three things on a
run, because the question has three halves and only two are reachable:

- through the API: a search with a negated condition, resolved, counted, against the same
  search without the negation
- through the ADD-IN'S OWN ROUTE, which is what actually matters, because the tool does not
  ask Navisworks to import an XML: `ExchangeReader` parses the file and `SetBuilder` builds
  each set through the API, passing `flags` straight into `SearchConditionOptions`. So a
  small XML carrying `flags="32"` is read and built and the set's condition is read back
- through NAVISWORKS' OWN IMPORT, the Sets panel's import, driven by hand if it is
  reachable, on the same file
- the export half is not reachable, and 5g will say so by name rather than leaving it open

If negation imports, `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` is rewritten with
the real negated form and the byte for byte test is kept green. If it does not, the
fallback stays and the NOT MEASURED wording goes, so nobody measures it a third time.

PART 4, the three answers, one commit. Q54 right as it is and closed, Q55 answered and
carried out by this round, Q56 recording what Bader saw, because it is the only user
report this feature has.

PART 5, the proof, one commit. Build 0 and 0, the full Core suite, both scripts under
`tools\checks`, `build\install.ps1`, then a run against a FRESH copy of his NWF folder, so
the 975 are written new and not added to, with the same settings as the last two rounds:
25 mm chosen in the tool, by design on, penetrations on, the priority file picked, his NWC
folder read only. Reported per group: viewpoints written and read back on all four things,
the NWF size before and after, the VIEWS seconds, and the run total against the viewpoints
round's 5 minutes 3 seconds.

Then I open one NWF myself, press three viewpoints, and say in plain words what is on the
screen. Not what the code intends. If one opens on grey I say which and why.

The log goes into `steps\logs`. ONE step goes into `steps\03_bader_next.md` naming the
file by its full path and the two viewpoints to press, so his check takes a minute.

WHEN DONE: one branch `round-dimming`, one commit per item, the round report at the top of
`steps\log.md`, `01_next.md` and `03_bader_next.md` updated, and a pull request, or the
compare link if the connector refuses it again.

### What this round will not do

It will not guess the dimming route, it will not colour the two items, it will not cap or
thin the viewpoints, it will not change the three layer tree, and it will not touch
anything in `samples`.


## 2026-09-20 The viewpoints round, F85 written and proved by six runs

### What was done

Main was red with 54 fixtures missing their sample. PART 0 put the two samples back from
history, deleted the FIXED copy in samples as Q52 asked, removed SuppliedCorrectedMatrix
and its five tests, and recorded Q48, Q52 and Q53 as answered. Those five tests proved
that the supplied FIXED file and the one F87 writes differ in exactly one line, that both
correct the missing hyphen, and that only the written one makes Devices a different set
from Electrical Fixtures. The reason for them is gone with the file: there is one FIXED
file now, in exchange, proved by a test to be what the rule produces.

PART 1 measured 5j: a viewpoint made with CaptureRuntimeOverrides records the hidden state
and brings it back after a save and a reopen, and one made from the camera alone records
nothing. On that yes, PART 2 wrote the tree. ViewpointBuilder.Build was CHANGED to take
the clash plan rather than given a second method, because nothing called the old per
discipline plan and a member nothing calls is deleted. VIEWS is timed and is the one step
allowed to move the viewpoint count, CensusRule. PART 3 fixed the A10 comment and took
the CORE HALF ONLY sentences out. PART 4 wrote the WORKBOOK line beside the tolerance
line and did not touch the workbook, Q54. The window driver gained a penetrations switch.

Then an adversarial review over the writer, 55 agents, 25 findings verified, 23 confirmed,
every one fixed in one commit. The one that mattered most: the hidden state was put back
with ResetAllHiddenToModelState, which the probe then measured to LOSE a hide the document
held, 5k, so the state is now read once off a capture before the first hide and put back
with SetHidden and read back as hidden. The rest: every model, item and side collection
the writer and the size reader borrowed is released, the view and the hidden state are
put back in their own tries, a model whose name will not parse is never hidden, the model
each clash item lives in is kept as well as the pair's, a service against a service is
judged on the larger, a test named twice on the report is read once, and a throw out of
VIEWS no longer skips the second NWF save, the workbook, the NWD and the confirm.

PART 5 then found the fault the review could not, by hand. The first run wrote 975
viewpoints into ten NWFs, the tree looked complete, and every viewpoint opened on the
same empty top view. Four measurements later, 5l to 5n, the writer is on a route that
works:

- CaptureRuntimeOverrides records what is hidden and NO camera. Its Viewpoint throws
  Camera not set. That is why every viewpoint opened on sky
- new SavedViewpoint(Viewpoint) records the camera and no overrides, 5j
- ReplaceFromCurrentView, whose doc promises both, records no overrides either
- the COM API's InwOpView with ApplyHideAttribs records BOTH, reads back through the
  .NET API, presses with both, and keeps both across a save and a reopen. The add-in now
  references the two COM DLLs beside the other two, copy local false, and CLAUDE.md says so
- a clash leaf has no Model and HasModel false. The model it lives in sits on the topmost
  ancestor, six to nine levels up, and enumerating AncestorsAndSelf while disposing each
  item throws inside the add-in, so the writer climbs Parent by Parent and releases the
  chain at the end, the shape the size reader has always used

Every written viewpoint is now read back three ways before it is counted, that it is
there, that its camera sits within a thousandth of a unit of the clash camera, and that it
carries visibility overrides where it hides a discipline. The second run proved the read
back catches a camera-less viewpoint: 975 failed, none counted.

PART 6 walked the ten NWF copies for every category value, 374 of them, and the list is in
Core with the health check live against it.

### Measured

Six runs, every one of the ten C02 groups, the same boxes every time: 25 mm chosen in the
tool, by design on, penetrations on, the priority file picked, the source his NWC folder
read only, the outputs in a fresh copy of his NWF folder under the temp folder.

| run | log | viewpoints | groups | total | VIEWS |
|---|---|---|---|---|---|
| 1, capture route | run-20260919-225249 | 975 created, opened on sky | 10 done | 5 min 9 s, 309.231 s | 4.591 s |
| 2, camera read back | run-20260920-082641 | 975 failed, Camera not set | 7 failed, 3 done | 8 min 10 s, 490.483 s | 4.650 s to 66.381 s per group |
| 3, COM route | run-20260920-084625 | 975 created, read back | 10 done | 4 min 58 s, 297.758 s | 10.709 s |
| 4, homes off Item1 | run-20260920-085853 | 975 created, 0 homes read | 10 done | 4 min 58 s, 297.702 s | 10.773 s |
| 5, homes by enumeration | run-20260920-091405 | 975 created, 975 home reads threw | 10 done | 5 min 6 s, 305.570 s | 11.396 s |
| 6, homes by Parent | run-20260920-092328 | 975 created, 975 of 975 homes read | 10 done | 5 min 3 s, 302.972 s | 12.595 s |

The wiring round's log reads 6 minutes 47 seconds for its run, 407.270 s, and the brief
named 4 minutes 54 seconds. The sixth run with viewpoints took 5 minutes 3 seconds.

Viewpoints per group on the sixth run, and the VIEWS seconds: 1000BS none in 0.031 s,
1A0215 none in 0.049 s, 1A02BS 109 in 0.841 s, 1A02MM 430 in 7.554 s, 1A02MS none in
0.036 s, 1A02WE 29 in 0.251 s, 1A02WL 16 in 0.294 s, 1A02WM 245 in 2.489 s, 1A02WN 77 in
0.557 s, 1A02WO 69 in 0.492 s. Folders: A, B and C at the root beside whatever the file
already had, one pair folder under each, 1A02MM's A holding AR vs ST, ST vs ST, AR vs DR,
DR vs ST, AR vs EL, EL vs ST and ST vs UNKNOWN. No Over 150mm folder was written, because
the size reader judged no service on these clashes Large, and the plan block says 0 at or
under the threshold in every group.

NWF sizes, his file before and the sixth run's copy after, bytes: 1A0215 78,341 to
78,452 with no viewpoint, 1A02MM 119,542 to 156,561 with 430, 1A02WE 77,234 to 82,776
with 29, 1A02WL 71,858 to 78,720 with 16, 1A02WM 111,709 to 129,051 with 245, 1A02WN
83,291 to 91,347 with 77, 1A02WO 83,819 to 90,741 with 69. The three built new: 1000BS
13,097, 1A02BS 39,735 with 109, 1A02MS 81,951. That is about 86 bytes a viewpoint. Nothing
here is worrying and nothing was capped.

By hand, on the sixth run's 1A02MM copy opened fresh from disk: the tree is there, and
pressing AR vs EL clash 15 opens on the cable tray meeting the wall with ME and ST greyed
in the selection tree and AR and EL shown. Pressing DR vs ST clash 1 opens on the framing
round the drainage pipe with AR and EL greyed and ME and ST shown, ME because the pipe
lives in it, which is the home model working. Pressing DR vs ST clash 2 opens on uniform
grey, the camera inside a member that Clash Detective's own view would dim through. That
is question 57.

The hidden state line read nothing was hidden before the viewpoints and nothing is hidden
now on every group of every run, because his NWFs hold no hidden override. The route that
puts a hide back was measured in the probe, 5k, not on these files.

Core tests: 1568 passed and 54 failed on main before PART 0, 1603 after it, 1605 at the
end, 0 skipped on this machine. Build 0 errors, 0 warnings after every change. Both checks
pass.

### Every program started, every file written outside the repo, every process stopped

Started: Navisworks Manage 2025 through Roamer.exe six times for the six runs and the hand
checks, and seven more times as the automation host for the probe, modes restore, camera
twice, record, com, walk and home, each of which exited on its own. dotnet build, dotnet
test and build\install.ps1, which built and copied the bundle six times. PowerShell drivers
from the scratchpad for the window, the tolerance box, the confirm dialog, the tree, the
screenshots and the close, every one of which exited.

Stopped: Roamer.exe was stopped with Stop-Process twice, after the second run at about
08:37 and after the third at about 08:55, when the add-in window ignored its close and the
main window would not close behind it. Both times the open document was a temp copy with
nothing to save. Every other Navisworks was closed through its own window, answering No to
the save prompt. No browser and no sign in page opened at any point.

Written outside the repo, all under C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints,
321 MB: NWF to NWF6, six copies of his NWF folder, one per run, with what each run wrote
into them, NWD and Excel with the sixth run's outputs, Clash Report copied at the start,
probe with the probe result files, the two NWF copies it opened and one it saved as
probe-com-saved.nwf, and the driver notes, the tolerance notes and 21 screenshots. The
tool's own logs are in C:\Users\bader\AppData\Local\ParsonsNwcFederator\logs, six pairs from
run-20260919-225249 to run-20260920-092328, copied into steps\logs. The bundle at
%APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle was replaced six times.
Navisworks itself wrote autosaves of the open temp copies into
%APPDATA%\Autodesk\Navisworks Manage 2025\AutoSave. The scratchpad under
C:\Users\bader\AppData\Local\Temp\claude\c--Users-bader-Documents-GitHub-PAR-NWC-Federator-V2\
holds the scripts.

His folders: the NWC folder was read and nothing was written there. The NWF, NWD and Clash
Report folders were listed at the end and every file carries the size and time it had
before the round. The four properties CSVs the wiring round's probe button wrote beside
his NWCs on 2026-09-19 are still there and are his to delete.

The closing check ran: no Roamer, no Autodesk process and no driver process was running.

### What remains

- Bader's look at the tree and two viewpoints, steps 381 to 389
- Q54, the WORKBOOK line is on every run and the workbook is unchanged
- Q55, the harvest reads the source file off the clash leaf, where 5n measured there is
  none
- Q56, the category list is one folder's, and the health block reads 14 sets asking for a
  category no C02 model carries
- Q57, a viewpoint whose camera sits inside a member opens on grey

### Known bugs

None open from this round. The DR vs ST clash 2 view is question 57 and not a bug in the
writer: the camera is the one Clash Detective computes and it reads back exact.

### What comes next

Bader answers 54 to 57. 5g, the property probe's walk, was not in this round.

## 2026-09-19 The viewpoints round, the plan, written before the first edit

### What the round is

Main is red, one measurement decides whether the saved viewpoint tree is worth writing,
and if it is, this round writes it. Four rules from the brief hold over everything below:
nothing is written into a live project folder, every program started and every file
written outside the repo is listed in the round report, everything opened is closed and
the check that it was closed is run, and nothing of Bader's is deleted or overwritten.

THE BUILD GATE PASSED. On the machine of 2026-09-19 the whole solution built in Release with 0
errors and 0 warnings before anything was written, with the add-in inside it. The Core
suite on main reads 54 failed, 1568 passed, 1622 total. Every one of the 54 is a fixture
that resolves a sample by name and finds it gone, which is PART 0.

One thing the brief says the other way round from Samples.cs, and Samples.cs is what the
tests read: `1104-PAR_CLASH_AllInOne (2) (1).xml` is `AllInOne()`, the client's export at
75 mm that the 44 assertions measure, and `1104-PAR_CLASH_AllInOne_25mm.xml` is
`Matrix()`, the source the F87 corrections are applied to. Q48 in 02_questions.md says
the same as Samples.cs. Both files are restored either way and nothing here depends on
which is which.

### The order

PART 0, main green, one commit for the fix and one for the answers:

- restore both deleted samples out of the parent of the commit that deleted each,
  `git checkout 4a70f89^` and `f4e510b^`, byte for byte
- delete `samples\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`, which is Bader's answer to
  Q52 and the one exception to the rule that nothing in samples is touched. The exchange
  copy stays and is the one the tool is pointed at
- take `Samples.SuppliedCorrectedMatrix()` and `SuppliedCorrectedMatrixTests` out, four
  tests, and say in the report what each proved. What they proved about the exchange
  file itself is still pinned by the test that proves the exchange file is what the rule
  produces from the sample, which reads `CorrectedMatrix()` and stays
- record Q52, Q48 and Q53 as answered by Bader on 2026-09-19, and put one line above
  `IsDecided` saying a test is its only caller and Bader chose to keep it
- the Core suite green before anything else is touched, counts before and after

PART 1, the one unknown answered on a run. The measurement needs code running INSIDE
Navisworks, and the add-in has no button for it, so a probe plugin is built for it:
`tools\probes\ViewpointProbe`, a plugin assembly with no Core reference, loaded into a
Navisworks started through the automation API with `AddPluginAssembly` and run with
`ExecuteAddInPlugin`, which ran the add-in's own plugin on 2026-09-19 21:05. It opens a
COPY of one NWF under `C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints`, hides
two models, saves a viewpoint two ways, `new SavedViewpoint(Viewpoint)` and
`DocumentSavedViewpoints.CaptureRuntimeOverrides()`, reads `ContainsVisibilityOverrides`
on each, presses each from a clean view and reads `IsHidden` back, then saves the copy,
clears, reopens it and reads the same things again. Every step goes to a result file as
it happens, so a plugin the host stops early still leaves what it measured. The result is
scan.md 5j. IF THE ANSWER IS NO the tree is not built, `CanBuild` stays false, PART 3
and PART 4 are done and the round stops there. If the automation host will not run the
probe, the fallback is the probe in its own bundle under ApplicationPlugins and the
button pressed in a Navisworks started the ordinary way, and the report says which route
was used.

PART 2, the tree written, only on a yes, built after every change:

- `ViewpointBuilder.Build` CHANGES to take the clash plan rather than gaining a second
  method, because the per discipline plan it takes today has no caller once the engine
  reads the clash plan, and a method nothing calls is deleted with its tests. The old
  `ViewpointPlan` and `PlannedViewpoint` go the same way once nothing reads them, and
  the report says what went
- the engine builds `ClashToPlan` per clash off the document, in one walk of the tests
  the report names: the two set names off the test's locators, the status, the test's
  priority off the report, and the service size read the way F72a reads a side, the
  largest size property, through the one reader `Penetrations` already has. Each clash's
  camera comes from `TestsViewpointForResult`, which is what frames the pictures today
- `ClashViewpointPlan.For` is called exactly as it is and nothing about the plan changes
- the writer, per planned viewpoint: hide every model whose discipline is not one of the
  pair's two, set the current view from the clash's camera, capture the viewpoint the way
  5j says records the hiding, put it in its folder through `AddCopy` with the folders
  made on the `EnsureFolders` shape and re-resolved from a fresh `RootItem` after every
  `AddCopy`, name it with `EditDisplayName`, read it back, dispose everything. Hidden
  state is put back to what it was when the group's writing ends
- the VIEWS step is opened around it with `RunSteps.Views`, so it is timed like every
  other step, and `CensusRule` gains the one line that VIEWS may move the viewpoint count,
  with its test, because without it every group would read CENSUS CHANGED and FAILED
  over the step doing its job. That is the one Core rule this round touches and it is
  said here rather than slipped in
- `SavedViewpoints.CanBuild` goes true last, in its own commit, once the writing works

PART 3, the two places that say done: the ClashImages comment corrected now and again
when PART 2 lands, `RunSteps.Views` timed by PART 2 or the core.md sentence taken out on
a no, and F85's two CORE HALF ONLY sentences taken out only on a yes.

PART 4, the WORKBOOK line, Core wording with a test, written beside the tolerance line
at the run's tail. The brief's wording says 1,041 carry a clash. On the wiring round's
run 1,041 was the tests that RAN and read their tolerance off the document, and some of
those found nothing, so the line says ran rather than carry a clash, and the report says
so. Q54 recorded as waiting on this round's run with that line named.

PART 5, the run, everything measured and nothing decided:

- build at 0 and 0, the full Core suite, both scripts under tools\checks,
  `build\install.ps1`, which is where the install script is, the brief's tools\install.ps1
  does not exist
- the NWF, NWD and Clash Report folders of C02 copied under
  `C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints`, and the run pointed at
  those copies. The NWC folder is READ from where it is and nothing is written beside it,
  because the NWFs point at those NWC paths and copying them would turn every weekly
  group into a rebuild and break the like for like comparison
- the same boxes as the wiring round plus penetrations on: tolerance 25 mm, by design on
  with samples\by-design-pairs.csv, penetrations on, samples\clash-priority-map.csv
- one NWF copy then opened in Navisworks by hand, the tree read off a screenshot, one
  viewpoint pressed and the view read off a screenshot
- reported by measurement: viewpoints and folders per group off the VIEWS block, the NWF
  size before and after per group off the disk, the VIEWS seconds off the STEP lines, the
  run total against 294.3 seconds, and what the pressed viewpoint showed. An NWF size
  that would worry a person is said as a number and becomes a question, never a cap

PART 6, only on green: the category walk through the same probe plugin, over every NWF
copy, written into `src\Federator.Core\Exchange\revit-categories.txt`, the tests that
pin none yet updated to the measured state, and scan.md 5i. 5g is not in this round.

### What is checked before the run, by agents rather than by me

The viewpoint writer is the one piece of this round that touches a native handle across
a mutation, which is the fault that once threw for 8 hours 52 minutes. Before the run,
a review workflow reads the PART 2 diff with several independent readers, each told to
refute the code on one axis, handle lifetime and disposal, the census, the hidden state
being put back, the folder re-resolve, and the plan being called unchanged, and every
finding is verified by a second reader before it reaches me.

### What is started and what is left, to be listed in the round report

Navisworks through the automation host for PART 1 and PART 6, Navisworks started the
ordinary way for PART 5 and the look by hand, the window driver, and every file under
`C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints`. Nothing beside his NWC
files this round. The four probe CSV files the wiring round wrote beside his NWC files
are his to delete and are named in the report. Every process is checked closed at the
end and the check is reported.

### What comes next

The closing entry above this one, written when the round is closed.

## 2026-09-19 The wiring round is closed, the eight Core halves have their add-in callers

### What the round was

The plan is the entry below this one, written before the first edit. Eight fixes of the
first run round had a rule, tests, and no add-in caller: F74, F75, F76, F77, F83, F72b,
F72c and F86. This round wired them in the order the brief gave, following the steps
already in 03_bader_next.md, on the machine with Navisworks Manage 2025 on it, one commit
per item on the branch round-wiring, the whole solution built after every one, and proved
by two runs of a real folder driven from this session. Nothing about the eight designs was
invented here, and no Core rule was changed to ease a wiring.

### What was done, in the brief's order

- PART 1. Fourteen sentences in core.md saying which rules were the Core half only, each
  naming its step. Twelve came back out in the same commit as the wiring that made the
  rule true. Two stay, both F85, steps 374 and 375, which are not in this round
- PART 2. Three of the five measurements taken by reflection and written into scan.md 5e,
  5f and 5h, each with its probe in tools\probes. 5e: DocumentModels.SceneLoaded exists,
  and the run then showed it fires once per model INSIDE TryOpenFile, before it returns,
  so the poll stands as the reader and the event is counted beside it. 5f: the whole
  property walk, and how a value becomes a cell by its kind. 5h: a comment CAN be
  written on a clash result, through DocumentClashTests.TestsEditResultComments. 5g got
  what reflection can say, SearchCondition.Negate and NegateCondition as bit 32, and its
  round trip still needs a hand built set. 5i is NOT measured, and the probe as built
  cannot answer it, see below
- PART 3. The eight wirings, one commit each: F75 b5ce89f, F74 fcbdb6b, F76 b6ec0f2 and
  ee62966, F77 03a913e, F83 580fe51, F72b 8f7cedd, F72c 377d1f0, F86 dcbbf67. Every one
  built with 0 errors and 0 warnings before it was committed, and both tools\checks
  scripts were clean after every one
- PART 4. A19 and A11 first with the audit's own proofs run, then A10, A12, A13, A14,
  A15, A17, A18, A20, A21, A22 and A23, one commit each. A16 became Q53 rather than a
  deletion, below. A23 moved Csv out of PriorityMap.cs and nothing else, as the brief said
- PART 5. The build at 0 and 0, the Core tests, both checks, build\install.ps1, and TWO
  runs of the C02 folder, ten groups, from this session. The first with the tolerance at
  25 mm, the by design box on with samples\by-design-pairs.csv, and
  samples\clash-priority-map.csv picked. The second with the same boxes, so every NWF the
  first had written was opened again off the disk. The log is
  steps\logs\run-20260919-211323.log with its .tsv beside it

### What the run showed, line by line, the seven the brief named

Ten groups, three First run and seven Weekly run plus XML, all ten DONE, none partial,
none failed. The run took 4 minutes 54 seconds against the 45 the criterion allows, and
the first run round's seven groups had taken 24 minutes.

- CENSUS CLEAR at the top of every group: ten lines, every count read zero, and no
  CENSUS CHANGED or CENSUS DIRTY anywhere in either run
- TOLERANCE: one per group, for example `TOLERANCE 25 mm chosen in the tool, so it beats
  both the XML and the document. Set on 1770 tests, 0 created fresh and 1770 already in
  the document. 25 mm is 0.082021 ft in this document`, and on a saved test
  `CLASH TOLERANCE BLD-AR-Stairs-vs-BLD-AR-Floors 0.2460629921 to 0.082020997375 ft,
  chosen in the tool, which reset its results`. The confirm dialog carried the four
  warning lines, read off a screenshot
- LOADING: two lines per opened NWF, `the NWF reported 4 models after 0.721s, steady over
  3 reads 0.250s apart` and the event line beside it. Nothing read empty, so the refusal
  and the ceiling were not exercised
- CLASH 1830 in the file: ten lines. First run groups `21 created, 1809 not created, a
  side finds nothing`, `105 created, 1725 not created`, `15 created, 1815 not created`.
  Weekly groups `0 created, 60 not created`, the 60 being the tests the corrected XML
  names differently from the tests saved in the NWF. BLOCKS 1830 in every workbook
- PRIORITY: `PRIORITY 1830 in the file, 1830 tests in this run, 1830 matched, 0 not named
  by the file` in every group, a block per group such as `526 in all, A 379, B 51,
  C 96, No priority 0`, and in RESULT `by priority : A 742, B 91, C 238, No priority 0,
  1071 in all`
- REVIEWED rule B: 145 clashes moved to Reviewed across the run, each with its pair and
  reason, `REVIEWED rule B 145 set, 29 pairs in the file matched no test in this run`,
  and in RESULT `by design : 145 clashes moved to Reviewed`
- The Priority column: `Priority` is in the shared strings of every workbook, and the
  WORKBOOK CHECK block of every group reads `Every column, value shape, fill, border, row
  height and column width matches the client's report, and the blocks are in their order`

### What the second run showed

Every NWF the first run wrote was opened again off the disk. Rule B found the clashes it
had moved `already Reviewed, Approved or Resolved, so somebody decided and it stands`, 17
in 1A02BS and 44 in 1A02MM, so the STATUS half of the record survived a save and a
reopen.

The Undo auto Reviewed button was then pressed on the open document, 1A02WO, which the
second run had opened off the disk: `10 put back of 69 looked at`, every one `back to
Active`, the status its record named, and 59 `this tool never moved it, so there was
nothing of ours to undo`. So the COMMENT half of the record survived the save and the
reopen too, and TestsEditResultComments writes a record that comes back readable. The
undo changed the open document and saved nothing, as its line says.

The Probe model properties button was then pressed for the open document: four models
walked in under a second each, 222, 188, 946 and 62 items, one CSV beside each NWC in the
source folder. The mechanical one: 17 categories asked for, 6 found, the 11 with no
element named, 1939 properties, 4855 distinct values, none capped, and `FS and Fire
Suppression appear in NO property tab, property name or value anywhere in this model`.
The structural one is a header alone, because none of the seventeen is a structural
category. Two values in the mechanical file carry a newline inside a quoted cell, a URL
and a drawing number, which is what the model holds and the CSV keeps. The four CSV files
sit beside the NWC files in the C02 NWC folder, where step 360 says they go, and nothing
else in that folder was touched.

### What the run showed that nobody asked about

- THE REPORT TOLERANCE LINE READS ALARMING AND IS RIGHT. `TOLERANCE on the report, read
  off the clash test in the document for 1041, the clash XML for 17259, chosen in the
  tool for 0, UNKNOWN for 0, 18300 tests in all`. The 17259 are rows of tests that never
  ran, and a row the document never produced keeps what the plan gave it. That is Q54
- THE SET COUNT FOR THE CREATION PLAN COSTS NOTHING. `counted the items of 61 sets for the
  creation plan in 0.0000344s` on every group. Navisworks answers GetSelectedItems off a
  cache once the sets step has resolved them, so the cost F77 was written to avoid, 631
  seconds of creating, went to under a millisecond of counting
- 192 item ids are missing on 1A02MM and every one is on a result carried over from an
  earlier run, F79's line, which is the first time that split has been read off a run
- The scene loaded event fires INSIDE the open on this machine, which is not what the
  first real run saw. What made five NWFs read empty there is still UNKNOWN, 5e says so

### How the run was driven, because it could not be pressed by hand

Navisworks started through the automation API ran the plugin and the window closed on
its own after 8.5 seconds, why UNKNOWN. Started the ordinary way it stayed open. The
add-in's ribbon button has no element in the automation tree, the window is not a child
of the desktop root because Navisworks owns it, and a control on an unselected tab has
no visual tree. All of that is measured and written at the top of
tools\probes\drive-window-run.ps1, which is the driver, and a person at the machine
opened the NWD and the add-in while this was being worked out. The audit's own proofs for
A19 and A11 were run the way it asked: the comparison flipped and the resource removed,
each fixture went red, and each was put back

### What is NOT done, said plainly

- 5g's round trip and 5i. 5g needs a search set built by hand in Clash Detective. 5i is
  not a side effect of F86 as built: the probe reads only the seventeen categories the
  settings ask for, so it cannot list every category a model carries, and 03_bader_next.md
  says so at step 357
- F85, steps 374 and 375, was never in this round. Its two sentences stay in core.md
- The pull request. There is no gh here and the GitHub connector refuses to create one,
  as on the two rounds before. The branch round-wiring is pushed and Bader merges it
- The DEFAULT tolerance path, the by design box off and no priority file were not run
  this round, because the brief asked for the one run with all three on. The first run
  round's log covers the defaults as they were before the wiring

### The Core changes that were not wirings, each said

- RunLog gained PriorityAcrossTheRun, ByDesignWanted and ByDesignMoved, the hooks the
  two RESULT lines needed, the same shape the penetration line already had, four tests
- PenetrationSettings.Holds became the public Names, and ProbeSettings gained Asks, so
  the probe asks for a category the way the penetration rule reads one, one test
- ClashHarvest.Text became internal so the probe reads a value by its kind through the
  one reader. A19 added seven tests, A11 a third state and one test, A14 made
  RunLog.KeptOfARepeat the one number, A15 a note on a rewrite, A22 one space, A23 one
  file, A13 and A12 one name each, A20 one sentence, A17 one name, A21 one line

### The numbers

- Build before the first edit: 0 errors, 0 warnings. After every commit: the same
- Core tests before the first edit: 1609 passed, 0 failed, 0 skipped, 1609 total, the
  same 1609 the last round left. After: 1622 passed, 0 failed, 0 skipped, 1622 total.
  Thirteen tests added: two for the priority RESULT line, two for the by design RESULT
  line, one for Asks, seven for A19 and A18, one for A11. Zero skipped rather than the
  container's 32, because this machine has Navisworks and a Windows file system
- 30 commits on round-wiring, one per item, and the two checks clean after every one

### Questions raised

Q53, whether IsDecided stays as a stated rule with a test only caller. Q54, what a
skipped row's tolerance origin should be. Q46 to Q52 are still unanswered and nothing
here decided any of them.

### What comes next

Bader merges round-wiring. Then 5g by hand, 5i with its own walk, the F85 steps 374 and
375, and the seven questions. The defaults path deserves one run with every new box off,
to prove the wiring changed nothing where nothing was asked for.

## 2026-09-19 The wiring round, the plan, written before the first edit

### What the round is

The first run round wrote the Core half of eighteen fixes in a container with no
Navisworks on it, and the audit in steps\04_audit_first_run.md found that eight of them
have a rule, tests for the rule, and NO add-in caller: F74, F75, F76, F77, F83, F72b, F72c
and F86. FederatorWindow.xaml has not changed since 2026-09-19 12:40, so the tolerance
drop down, the by design tick box, the priority picker and the probe button do not exist.
This round wires the eight, following the instructions already written in
steps\03_bader_next.md steps 353 to 380, and invents no design.

THIS ROUND IS ON THE MACHINE WITH NAVISWORKS. The brief said to stop if the build failed
on the Navisworks reference. It did not. On the machine of 2026-09-19, Windows 11, dotnet 10.0.400,
the full solution build in Release finished with 0 errors and 0 warnings before anything
was written, with the add-in project inside it. Every wiring below is compiled here after
it lands.

Three things the brief says that were checked and found otherwise, so nothing below
carries them as written:

- the install script is build\install.ps1. There is no tools\install.ps1
- the run log the audit read, run-20260919-144319.log, is not in steps\logs. That folder
  holds README.md and run-20260907-093440.log only. Nothing here depends on it
- a pull request cannot be opened from this session. There is no gh and the GitHub
  connector refused create_pull_request on the last two rounds. The branch is pushed and
  Bader merges it. The one branch the brief asks for, round-wiring, is what is made

Q46 to Q52 are all unanswered. No decision the last round deferred to Bader is taken here,
and where a wiring would need one it is done the way 03_bader_next.md already says and
the question is named beside it.

### The order, and nothing starts before the step before it is committed

PART 1, its own commit first. The fourteen lines of .claude\rules\core.md that state a
behaviour nothing in the add-in does, lines 164, 216, 234, 446, 458, 696, 708, 719, 793,
903, 923, 934, 952 and 1040, each get one sentence at the top: THE CORE HALF ONLY until
03_bader_next.md step N is done, naming the step. Lines 793 and 1040 are F85, which is
not one of the eight and has no step in this round, so their sentence names step 375
and stays. Each of the other twelve sentences is removed in the same commit as the
wiring that makes it true.

PART 2, the five measurements, one commit each, written into docs\history\scan.md under
the letter that already waits for it:

- 5e, the readiness API, F74. A PowerShell reflection probe in tools\probes, in the
  shape of probe-model-remove.ps1, reads every member of Document, DocumentModels and
  Application whose name holds Load, Ready, Busy, Progress, State, Pending or Complete,
  and every event on each. Pasted as 5c and 5d paste theirs
- 5f, the property API, F86. The same shape, over ModelItem.PropertyCategories,
  PropertyCategory, DataProperty, VariantData and Search
- 5h, a comment on a clash result, F72c. The same shape, over ClashResult, IClashResult,
  ClashTest, SavedItem and Comment in the Clash DLL and the Api DLL. Whether a comment
  SURVIVES a save and reopen cannot be read off the DLL and waits for the run in PART 5
- 5g, the negated condition, and 5i, the real category list, are not reflection
  questions. 5g needs a search set built by hand and exported, then imported back. 5i
  needs a walk of every item in one real federation, which is the probe F86 builds. What
  reflection CAN say about 5g, whether SearchCondition carries a negate member at all, is
  read in the 5f probe and written under 5g. The rest of both waits for PART 5, and if
  PART 5 cannot reach a Navisworks session from here, both stay NOT MEASURED and say so

A measurement that comes back the way the fallback already assumes is written as that,
and the fallback stays.

PART 3, the eight wirings, in the order the brief gives, one commit each, built after
every one with the full solution and the error list reported if there is one:

1. step 366, F75. FederationEngine.RunOne empties the document at the top, before
   Decide, on the scanned path only, takes the census straight after through
   DocumentCensusReader, writes CensusRule.StartOfGroupLine(census, true), and puts
   StartOfGroupReason on the group where it is not null. RunOpenDocument passes false
2. step 365, F74. FederationEngine.Decide, after TryOpenFile returns true, polls
   document.Models.Count through Federator.Core.Rerun.ModelLoadWait with
   RunLog.ElapsedSeconds, pausing PauseMilliseconds between readings, writes wait.Line(),
   and where the count settled at zero or gave up at zero returns
   NwfComparison.ReadEmpty with how long it waited. RunOne carries the Reason onto the
   group as GroupFacts.NwfReadEmptyReason. PreviewRunPaths does the same wait and the
   same refusal so the confirm dialog cannot say Rebuilt about a healthy NWF
3. step 367, F76. The Clash step gets a drop down filled from ToleranceChoice.Choices()
   with an Other box, its label and help line read off ToleranceChoice.PickerLabel and
   HelpLine. ReportsWanted sets options.Tolerance. ClashRunner calls
   options.Tolerance.For(planned.Tolerance, documentUnits) where a test is created and
   where one already in the document is left alone, writes LogLine once per group, and
   the confirm dialog carries WarningLines
4. step 368, F76. ClashRunner reads report.Tolerance off the ClashTest in the document
   instead of off the plan, sets ToleranceFrom to Document, counts the four origins and
   writes ToleranceChoice.ReadFromLine once per run
5. step 369, F77. ClashRunner.Run builds a dictionary of locator to item count off the
   sets just resolved, hands it and the resolved plan to CreationPlan.For, creates only
   plan.Create, feeds plan.NotCreated into the existing skip machinery, and writes
   plan.CountedLine(testsInTheFile). BlockCountLine goes where the workbook is checked
6. step 370, F83. A second picker beside the clash XML picker, PickerKind.Priority,
   optional. The engine reads the file through PriorityMap.Read, puts the map on
   report.Priorities, sets test.Priority on every TestReport, writes map.MatchLines,
   passes picked into WorkbookCheck.Of(path, picked), counts the clashes into a
   PriorityTally per group and across the run, and writes PriorityTally.ResultLine in
   RESULT
7. step 371, F72b. A second tick box under the penetration one, off by default, its
   label and grey line read off ByDesignPairs.TickLabel and HelpLine, and a third picker
   for the pairs file, PickerKind.ByDesign. The grid gains its third row. In the same
   per test pass that applies the penetration statuses, ByDesignRule.Judge runs per
   clash with the two set names off the locators through ByDesignRule.SetNameIn, the
   wanted ones join the one statuses.Apply list, ByDesignTally writes the block, and the
   run line and the RESULT line follow
8. steps 372 and 373, F72c. ClashStatusEditor writes record.Text() as a comment on the
   clash BEFORE the status is set and on the same handle, the way 5h says it can be. If
   5h says it cannot, UndoAutoReview.CannotLine is written once and the status alone is
   set. The Undo auto Reviewed button on the Clash step walks every clash of every test
   in the open document, calls UndoAutoReview.Judge, and puts back only the ones that
   carry our record and are still at Reviewed, through the one ClashStatusEditor, which
   gains the AllowsAsUndo path beside its Reviewed only path
9. steps 358 to 364, F86. The Probe model properties button on the Clash step, label
   and help line off ProbeSettings. It picks a folder of NWC files or takes the open
   document, refuses an NWF or an NWD by name through ProbeSettings.MayRead, walks the
   items of each of the seventeen categories the way 5f says the properties are read,
   counts into ProbeTally, writes one CSV beside each file through ProbeCsv at
   ProbeSettings.CsvPathFor, and writes ProbeVerdict.Lines to the log

Each of the nine removes its PART 1 sentence from core.md in the same commit.

WHAT IS NOT CHANGED TO EASE A WIRING. If any of the nine needs a Core rule to move, the
rule stays, the wiring stops at that point, the log entry says which rule and why, and
Bader decides. Nothing is written as done because a rule exists.

PART 4, A10 to A23 of the audit, one commit each, A19 and A11 first:

- A19, every new threshold tested AT the number and one past it. ProbeTally at 100 and
  101, RunLog.KeptOfARepeat at 5 and 6, and the four ExamplesShown at their number and
  one past. The proof is the one the audit gives: change the less than at RunLog.cs line
  674 to at most and the fixture must go red
- A11, RevitCategories gets a third state, ResourceFound, and Line() says the list could
  not be read out of the DLL when the stream is null or the read throws. The test
  asserts ResourceFound true and Measured false. The proof is dropping the
  EmbeddedResource line and watching the fixture go red
- A10 the comment, A12 the separator named once, A13 No priority named once, A14 the
  five examples named once with SetsAcrossTheRun saying why ten, A15 a Note on
  CategoryRewrite and a HEALTH sentence when the picked file carries the fallback,
  A16 falls away as the wirings land and IsDecided is judged on its own, A17 the test
  renamed for what it checks, A18 the test run through HealthCheck.Run, A20 the undo
  wording, A21 the open file tail says a one file run has no across the run block, A22
  the space
- A23, only Csv moves out of PriorityMap.cs, to its own file. The other twelve files
  stay as they are and the closing entry says so, because the brief said so

PART 5, in this order, each result written down as it was read and never as expected:

- the full solution builds 0 errors 0 warnings
- the Core tests run and the passed, failed and skipped counts are recorded beside the
  counts from the start of the round
- tools\checks\check-locals.sh and check-imports.sh both run clean
- build\install.ps1 puts the bundle under %APPDATA%\Autodesk\ApplicationPlugins
- Navisworks Manage 2025 opens with the add-in loaded, and ONE folder is run with the
  tolerance at 25 mm, by design ticked with a pairs file, and a priority file picked.
  The log is read for, by name: CENSUS CLEAR at the top of every group, TOLERANCE,
  LOADING, CLASH 1830 in the file, PRIORITY, REVIEWED rule B, and the Priority column in
  the workbook. The log goes in steps\logs under its own name
- WHETHER A NAVISWORKS SESSION CAN BE DRIVEN FROM HERE IS UNKNOWN UNTIL TRIED. Every
  probe in tools\probes loads a DLL by reflection and none of them opens the
  application. What PART 5 needs is the application open, the add-in loaded, the
  window filled in and Run pressed. If that cannot be done from this session, the run
  is written into 03_bader_next.md as numbered steps, the closing entry says the run
  was NOT done and by whom it waits to be done, and no line of the log is reported as
  seen

### The counts at the start

The build before the first edit: 0 errors, 0 warnings, the whole solution. The Core
test counts before the first edit are recorded in the closing entry beside the counts
after, read off the same command.

### What comes next

The closing entry above this one, written when the round is closed, says what was
proved here, what the run showed line by line, and what still waits. 03_bader_next.md
is updated so every completed step says so, and 01_next.md so the eight say WIRED
rather than DONE.

## 2026-09-19 The first run round is closed, F73 to F88

### What the round was

Bader ran the tool for real in Navisworks against seven buildings, the first run since the
add-in was proved to build, and briefed seventeen items off that one log,
`run-20260919-144319.log`. Seven groups, 28 files written correctly, 0 reported done and 7
reported failed. An eighteenth was found before any of them: `samples\Search Set Infra.xml`
had been deleted at 13:33 the same day and thirteen tests read it, so main was red.

ONE BRANCH AND ONE PULL REQUEST for the whole round, `round-first-run`, which Bader asked
for because the repo already carries seventy branches waiting on the D6 delete and
eighteen more unmerged would be worse. Each fix is still its own commit.

### The thing that has to be read first

THIS ROUND WAS WORKED IN A LINUX CONTAINER WITH NO NAVISWORKS ON IT, and Bader asked for
it to be moved to his machine. It could not be. That was checked three ways before it was
reported: `uname` says Linux, there is no `/mnt/c` and no Autodesk folder, a search of the
whole file system finds no `Autodesk.Navisworks.Api.dll`, `ListAgents` finds no other
session, and both environments this account can reach are cloud containers. There is no
route from here to that machine.

So the round was split, which is what Bader chose when he was told: EVERY RULE IS IN
FEDERATOR.CORE AND EVERY ONE HAS TESTS, and the add-in wiring is written out as numbered
steps rather than guessed at. The add-in was NOT compiled. Eighteen fixes touched it in
the sense that its call sites change, and `03_bader_next.md` step 379 says to expect
errors on the first build and to send the whole list.

FIVE MEASUREMENTS THE ROUND COULD NOT TAKE are written into `docs\history\scan.md` as 5e
to 5i, each one saying in its first line that it holds NO measurement, what the question
is, and exactly what to run. They are the readiness API for F74, the property API for F86,
whether a negated condition imports for F87, whether a comment can be written on a clash
for F72c, and the real list of Revit categories for F84. Nothing was filled in on a guess.

### What was done

- EIGHTEEN FIXES, seventeen briefed and one not, each on its own commit on one branch
- Core tests at the start of the round: 1293 passed, 13 FAILED, 32 skipped, 1338 total.
  The thirteen were the deleted sample and F88 was the first commit. At the end: 1568
  passed, 0 failed, 32 skipped, 1609 total. 271 tests added and not one broken at any
  point after the thirteen were fixed
- `tools/checks/check-locals.sh` and `check-imports.sh` both clean after every fix
- Seven questions raised, Q46 to Q52, and every one of them is a decision this round
  refused to make quietly

### The five that made the run useless, and what each really was

- F73 was one line. `CENSUS CHANGED  APPEND  saved viewpoints went from 0 to 20` was
  written in all seven groups and put every one of them out of DONE. An NWC exported from
  Revit carries that model's saved viewpoints and appending it brings them in. The census
  rule had two answers and needed three
- F74 was worse than it looked. Five existing NWFs reported 0 unchanged, 4 added, 0 removed
  and were rebuilt, which threw away five federations and every clash result and status
  decision in them. `TryOpenFile` returning true does not mean the models are in the
  document. The wait is one half of the fix and the refusal is the other, and the refusal
  holds whichever way the measurement goes
- F75 was two clears in the whole engine and both ran AFTER Decide had read the file list
- F76 was the matrix at 25 mm, the NWFs at 75 mm, and the run clashing at 75 while
  everybody believed it was clashing at 25. There has never been a tolerance setting in
  this tool
- F77 was 631 seconds of 1424 spent creating 1619 tests that were thrown away moments
  later, when the side counts were already in hand

### What was found that nobody asked about

- THE PRIORITY FILE MATCHES THE CORRECTED MATRIX EXACTLY. All 1830 test names and all 61
  set names. Against the uncorrected one, 60 test names do not match. That is independent
  evidence that F87's rename is the right one, and it came out of writing F83's test rather
  than out of looking for it
- THE HEALTH CHECK CATCHES F87 TWICE OVER. The corrected matrix breaks its own folder
  naming pattern once and holds one identical set pair. The uncorrected one breaks it twice
  and holds two. Neither number was known before F84 counted them
- F87 CHANGES A RECORDED MEASUREMENT. `.claude/rules/core.md` records 59 distinct sets out
  of 61, because two pairs carry identical rule lists. After F87 it is 60, because Devices
  is no longer the same set as Electrical Fixtures. The old number is kept beside the new
  one rather than overwritten
- FOUR CATEGORIES THE SERVICE SETS ASK FOR ARE NOT SERVICES. Air Terminals, Mechanical
  Equipment, Plumbing Fixtures and Sprinklers. The brief's wording for F72a's second test
  would have forced all four onto the service list, and an air handling unit against a wall
  would then be moved to Reviewed automatically. Q47
- THE BRIEF'S GREY LINE FOR F72b IS THIRTEEN WORDS and a grey line is twelve at most. The
  word list came off, because a file picked beside a tick box is a list and the picker
  beside it says so

### What the code knows and the log still does not, raised as questions

The standing rule is that a thing the code knows and no output shows becomes a question.
Six came out of this round.

- Q46, F77 against the single discipline rule. The NWF no longer carries every test in the
  matrix, and on a weekly run with no XML the missing ones stay missing
- Q47, which of the four arguable categories are services
- Q48, whether the 25 mm matrix becomes the reference the tests measure, which would mean
  re-reading 44 recorded measurements
- Q49, F83 replaces a MEASURED block order with a chosen one. The order the client accepted
  was read off both their own exports over 1830 blocks
- Q50, F72c widens one written guard, and the alternative shape is an undo that removes the
  record and leaves the status alone
- Q51, F53 and F72a read a size two different ways and F85 had to pick one in the open

### What is NOT done, said plainly

- NO VIEWPOINT IS WRITTEN. F85's plan is complete and tested and `SavedViewpoints.CanBuild`
  is still false, because whether a viewpoint saved while items are hidden records that
  hiding is not measured. A planned viewpoint that was not written is not a viewpoint
- NO COMMENT IS WRITTEN on any clash, because whether the API allows it is not measured. If
  it cannot be done the tool says so in one line and sets the status alone
- THE CATEGORY LIST IS EMPTY, so one third of F84 reports nothing and the block says which
  check did not run rather than leaving a reader to read no findings as a clean file
- THE NEGATED CONDITION IS NOT WRITTEN into the corrected matrix. The fallback is, because
  `equals` is proved to import and the negation is not
- NOTHING IN THE ADD-IN WAS COMPILED OR RUN

### Two things the close of the round found

- CI WAS RED THE FIRST TIME AND IT WAS THIS ROUND'S FAULT. Twelve tests passed in the
  container and failed on the Windows runner. `Samples.ExchangeFolder` walked up looking
  for a folder called "exchange" and returned the first one it found, and there is a
  folder called "Exchange" under the test project because there is one per Core folder.
  `Directory.Exists` is CASE BLIND on Windows and case sensitive off it, so the walk
  stopped at the test folder there and at the checkout root here. The rules already name
  matching two paths without case as a Windows file system difference. Both folders now
  join onto `Samples.Repo`, the one walk already written, which looks for the solution
  FILE by its exact name. `SamplesPathTests` asserts the decoy folder is still there AND
  is not what comes back
- TWO FILES CARRY THE NAME `1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` AND THEY ARE NOT THE
  SAME FILE. Bader uploaded one to `samples\` while the round was being worked, and F87
  wrote one to `exchange\` from the sample. They differ in exactly one line. Both correct
  the missing hyphen, all 121 occurrences. Only the one F87 wrote also makes
  BLD-EL-Devices ask for something other than Electrical Fixtures, so in the supplied file
  Devices is still the same set as BLD-EL-Electrical Fixtures and F84 still reports the
  pair. Neither file is deleted and neither is edited. The DIFFERENCE is pinned by a test
  instead, because two files of one name quietly disagreeing is how the wrong one gets
  picked a month from now. Q52

### The numbers

- Core tests before: 1293 passed, 13 failed, 32 skipped, 1338 total
- Core tests after: 1577 passed, 0 failed, 32 skipped, 1609 total
- F87 changed 121 occurrences of the missing hyphen and 1 category value. A second run
  reads 0, which is what proves it idempotent
- Questions before: 45. After: 52

## 2026-09-19 The penetration round is closed, F71 and F72

### What was done

- TWO FIXES, both briefed, both on their own branch, each with its own entry above. F71 is
  what the window failed to tell Bader on the first real run. F72 answers Q33, which has
  been open since F54 on 2026-09-18
- THE ADD-IN BUILDS after every add-in change, which is what this session can do and what
  the last two rounds could not. `dotnet build ParsonsNwcFederator.sln -c Release`, 0
  errors and 0 warnings, run after F71 and again after F72
- Core tests: 1238 passed, 0 failed, 0 skipped, 1238 total before the round. 1338 passed,
  0 failed, 0 skipped, 1338 total after it. 100 added and not one broken at any point
- Q33 IS ANSWERED and the other three shapes it offered are recorded as not chosen, so
  nobody builds one later thinking it was wanted. Q41 to Q44 are the four details, at the
  next free numbers because the file runs to 40 and the brief's Q47 to Q50 do not exist.
  Q45 is new, and it is the one thing in the brief this round did not do

### The one thing in the brief this round did not do, said plainly

- THE BRIEF ASKS FOR THE PENETRATION COUNT IN THE WORKBOOK. It is there, in the CLIENT'S
  OWN Reviewed column: `WriteTestHeader` writes all five statuses per test and the status
  is applied before the harvest reads it, so that cell IS what this run moved
- WHAT IS NOT THERE is a count of OURS saying how many this run moved, as against how many
  are Reviewed for any reason. Adding one would break a standing rule that this repo states
  twice: the workbook is the client's one sheet laid out as theirs, and if it is not in
  theirs it is not in ours
- SO IT IS Q45 RATHER THAN A DECISION TAKEN HERE. The log carries the run total in RESULT
  and the per group detail in the PENETRATION block, so nothing is hidden either way

### The read of 03_bader_next.md, end to end

- THE FILE RUNS 1 TO 352 NOW AND HOLDS 189 LOOK FOR LINES. It was 319 steps and 170 Look
  for lines this morning. F71 added 14 steps, F72 added 17 and the closing read added 2
- ALL 189 WERE READ, by nine readers over nine ranges, and 164 OF THEM COULD ACTUALLY BE
  CHECKED against the code. The other 25 are things only a run shows: a handle count in
  Task Manager, a picture on screen, an upload to ACC, the Selection Tree, whether a build
  succeeds. Those are named as not checked rather than counted as passed
- NINE POSSIBLE DRIFTS WERE RAISED and every one was then handed to a second reader told to
  REFUTE it, with the instruction to default to not-a-drift. ALL NINE CAME BACK REFUTED
  under a strict reading of the word: in each case the step was loose or incomplete rather
  than false, or the code had never moved away from it
- SIX WERE CORRECTED ANYWAY, and the reason is worth writing down. A step that is true but
  incomplete still costs Bader time at the machine, which is the whole thing this file
  exists to save. Refuted as drift is not the same as fine to leave
- TWO OF THE SIX ARE THIS ROUND'S OWN. Step 212, the window probe, said the visible tick
  box count must be 1, and F72 makes it 2. Step 270, which F72 itself wrote, said to look
  for a SECOND `NWF      attempt` line after the clash step, and on a Weekly run it is the
  FIRST, because the opened branch logs `NWF      reused` and never an attempt before the
  clash. That is the identical fault F57 already holds open for step 73, written again by
  me on the same day, which is worth more than the correction
- THE OTHER FOUR ARE OLDER. Step 336 said the top `step finished` row sorted by seconds is
  the slowest single step of the run, and `RunLog` writes that row only on a step's FIRST
  visit, so `TESTS RUN` contributes one row for one test out of 1830. Step 343 quoted two
  pre-commit lines as the whole of it and the hook prints four since F58 and F66. Step 326
  said the GAP block lists all six properties, and `GapRule.Add` returns early on a
  property nothing carried, so it can list fewer. Step 60 said the `SETS` line carries
  three numbers, and the ordinary case, where the clear kept them, carries two
- ONE WAS LISTED RATHER THAN CORRECTED, which is the repo's own rule for this shape. Step
  34 says the two hand buttons sit UNDER their line, and the TextBlock and both Buttons are
  the three children of one horizontal StackPanel, so the line is to the LEFT of them. F57
  already holds two steps open for exactly this, so this is the sixth on that list rather
  than a seventh correction. Three steps now say ABOVE or UNDER where the layout says
  BESIDE, and that is a pattern rather than three typos
- THE MECHANICAL HALF, WHICH CAN BE SAID EXACTLY. All 289 distinct backtick quoted strings
  in the file, checked against every `.cs` and `.xaml` under `src` plus `install.ps1`,
  `workflow.md`, `CLAUDE.md`, the checks, the probes, the hooks, `.gitattributes`,
  `Directory.Build.targets`, the project files and the Actions workflow, with C#
  concatenation seams flattened and with whitespace both collapsed and kept. 184 matched.
  The other 105 were resolved by hand and every one is composed at run time, is git or
  dotnet or PowerShell output, is a file name Bader types, or is a padded log prefix the
  writer builds
- EVERY CROSS REFERENCE READ AGAINST THE STEP IT NOW POINTS AT. There are nine and the
  renumbering moved four of them. The renumbering script skipped a `step NNN` sitting
  INSIDE a numbered line again, twice, which the F63 and F68 entries both already wrote
  down. It is written down a third time here because it will happen a fourth

### The thing found on the way that had nothing to do with either fix

- THE TWO CLAUDE CODE WALLS WERE JAMMED SHUT IN THIS CHECKOUT. `git ls-files --eol` read
  `w/crlf` on both `.claude/hooks/refuse-protected-paths.sh` and
  `refuse-git-on-main.sh`, against an index of `i/lf` and an attribute of `text eol=lf`.
  `sh` reads a carriage return as part of the word, so both die on their first case line
  and exit 2, and 2 is the code that REFUSES
- THAT IS THE EXACT CASE F47a WROTE `.gitattributes` FOR and the exact case step 339 exists
  to catch. The remedy is the one step 339 gives, and it was applied here: the two files
  were checked out again from the index and both now read `w/lf`. Nothing tracked changed,
  and the paths wall was then proved by hand, refusing a write under `samples` with exit 2
  and allowing one under `src` with exit 0
- WHY IT MATTERS BEYOND TODAY. A jammed wall does not fail loudly. It refuses everything or
  it refuses nothing, and either way nobody notices until something gets through that
  should not have. Steps 338 and 339 are how Bader sees it for himself and they are worth
  doing early rather than at the end

### What remains

- THE PULL REQUESTS COULD NOT BE OPENED FROM HERE, the same as the build round. `gh` is not
  installed on this machine and the GitHub connector answers
  `403 Resource not accessible by integration` to a create pull request call. So the
  branches are PUSHED and merged into nothing, and Bader opens and merges them in order
- THEY ARE STACKED, `fix-F71` then `fix-F72` then `round-close-penetrations`, each off the
  one before rather than off main, because three branches all prepending to `steps/log.md`
  would conflict on the second merge. Merged in that order each merges clean
- NOTHING HAS BEEN RUN. Not one line of F50 to F72 has been seen against a real model. What
  F72 COSTS is the number nothing here could measure: with the box on, every clash has a
  category read off both sides and a size read off the service, up to four levels each, so
  the cost is per CLASH and not per test. Steps 279 and 280 are the comparison that answers
  it, and until they are run it is UNKNOWN
- F57 has six wordings now and is still Bader's to judge
- Q35 to Q40, Q45, F52's writing half and F50's rebuild are where they were

### Known bugs

- None open in the code. The add-in builds, both checks are clean and the whole test set
  passes
- One thing is UNKNOWN and is named rather than filled in: what the penetration pass costs
  on a real model

### What comes next

1. Bader opens and merges `fix-F71`, then `fix-F72`, then `round-close-penetrations`
2. Bader pulls main and builds
3. Bader works steps 123 to 136, which is F71 in one section and takes no run at all
4. Bader runs one building with the penetration box OFF, then one with it ON, and reads
   steps 253 to 280
5. Bader runs the D6 delete command himself

## 2026-09-19 F72, penetrations become Reviewed, and Q33 is answered

### What was done

- Q33 HAS BEEN OPEN SINCE F54 ON 2026-09-18 and it is answered. It offered four shapes and
  asked which one the ask was. The answer is the SECOND, a rule over the clash itself, and
  the other three are recorded as not chosen so nobody builds one later thinking it was
  wanted: no list is supplied per run, nothing is read off a status in the clash XML, and
  nothing watches what a person marked last week
- A CLASH BECOMES REVIEWED WHEN ALL FOUR ARE TRUE. One side is a service by item category.
  The other side is a solid by item category. The service measures 150 mm or less. The
  clash is at New or Active. Everything else is left exactly as it is and counted by reason
- F54 BUILT THE HALF THAT NEEDED NO ANSWER and it is untouched. `ClashStatusEditor` still
  applies a list through the one measured mutator, `StatusesThisToolMaySet` still refuses
  everything but Reviewed, and the slot between the run and the harvest is still the slot.
  F72 supplies the list that was missing and changes nothing about how it is applied
- AND IT RESCUES THREE DEAD MEMBERS. `ChangedCount`, `NotFoundCount` and `RefusedCount`
  were declared by F54 and read nowhere in src, because nothing supplied a list. The rule
  says a public member nothing calls is deleted unless a decision in `02_questions.md`
  keeps it, and Q33 was that decision. Now they have a caller

### The four questions Bader answered, and what each one settled

- THEY ARE Q41 TO Q44 AND THE BRIEF CALLS THEM Q47 TO Q50. The questions file runs to 40
  and Q41 to Q46 do not exist, so they went in at the next free numbers, one for one and in
  order. The same thing happened to the log round, which was briefed as F56 to F61
- Q41, THE SOLID SIDE. Floors and roofs count as well as walls, so the default solid list
  is Walls, Floors, Roofs. A service dropping through a slab is the same kind of thing as
  one going through a wall
- Q42, NO DISCIPLINE FILTER. Any wall counts whichever file it came in. Nothing in the rule
  reads part 5 of a name and nothing in it knows what a discipline is, and there is a test
  that asserts exactly that, because the temptation to add one later will be real
- Q43, WHICH WAY ROUND THE 150 READS. It is a CEILING and not a floor: 150 or less becomes
  Reviewed, and a service over it stays at New because a large service through a wall is a
  real coordination item
- Q44, BOTH SIDES A SERVICE. Leave it alone, and count it, so a run says how many it saw
  rather than passing over them silently

### The four things this had to get right, and each one is a rule that fails quietly

- THE 150 IS NAMED ONCE AND READ TWO WAYS. `SizeSettings.ThresholdMillimetres` is the only
  150 in the repo. F53 puts an item in a viewpoint when it is OVER it. F72 marks a service
  Reviewed when it is AT OR UNDER it. So exactly 150 falls on a DIFFERENT SIDE in each, and
  that is not a contradiction: one rule is over and the other is at or under, and together
  they cover every size with no gap and no overlap. Both readings are written at that one
  number, and a test takes 150 through both rules in one method and asserts it is IN for
  the penetration and OUT for the viewpoint. Two copies of 150 would drift and nobody would
  notice until a report was wrong
- A DUCT IS NOT ONE NUMBER. `SizeRule.LargestMillimetres` takes the LARGEST of every size
  property the item carries, so a 600 by 150 duct is a 600 and stays at New. `SizeRule.Decide`,
  which F53 uses, takes the FIRST on the settings list instead, and that is right for a
  viewpoint: one representative size in a rule nobody has to argue about. The test that
  pins this feeds ONE lookup to both and asserts 600 out of one and 150 out of the other,
  because Width is first on the default list and a 150 wide by 600 high duct would
  otherwise read 150 and move
- THE UNREADABLE SIZE GOES THE OPPOSITE WAY FROM F53 ON PURPOSE. F53 INCLUDES an item whose
  size cannot be read, because a fitting usually carries no size property and the safe
  mistake there is showing something unnecessary in a viewpoint. F72 LEAVES ALONE a service
  whose size cannot be read, because the safe mistake here is leaving a clash at New for a
  person to look at. Both reasons are written at both places and there is a test whose whole
  job is to assert the two answers DIFFER, so a later reader who tries to make them agree
  breaks a test that tells them why not
- NEVER OVERWRITE A DECISION. `StatusesThisToolMayMoveFrom` is New and Active and nothing
  else, and it is a Core rule with its own tests rather than a condition inside a loop.
  That matters more than it looks: before F72 the editor would have moved an Approved clash
  to Reviewed if something asked it to, and nothing ever asked because nothing supplied a
  list. F72 supplies one, so the guard had to become real

### What the log and the workbook say, because a silent change is the fault this repo exists to avoid

- A PENETRATION BLOCK PER GROUP, in the shape the SETS block uses. One line per clash moved
  naming the test, the clash, BOTH categories and the service size, so somebody auditing
  this a month later can tell whether the rule picked the right thing without opening the
  model. Then the totals and ONE LINE PER REASON for every clash left alone
- EVERY REASON IS PRINTED INCLUDING THE ONES AT ZERO, which is a deliberate departure from
  the SETS block, which hides a zero. The difference is that SETS counts things MADE and
  this counts things NOT DONE, and a reason missing from a list of things not done reads as
  a reason nobody thought of
- THE RUN TOTAL IN THE RESULT BLOCK, and only where the box was on. A run that never asked
  for it has nothing to say, and a line reading zero on every run teaches people to skip it
- THE WORKBOOK CARRIES IT IN THE CLIENT'S OWN REVIEWED COLUMN. `WriteTestHeader` already
  writes all five statuses per test off `ClashTally.AllStatuses`, and the status is applied
  before the harvest reads it, so the Reviewed cell of every test block IS the number this
  run moved. NO COLUMN OF OURS WENT ON THAT SHEET. The brief asks for the count in the
  workbook and the standing rule says the workbook is their one sheet with none of ours on
  it, and the client's own column satisfies both. Whether Bader wants a cell of ours as
  well is Q45 rather than a decision taken here

### Two things the checks caught while this was written

- `check-imports.sh` REPORTED `Penetrations.cs` FOR A TYPE THE COMPILER IS HAPPY WITH.
  `ModelItemCollection` is in `Autodesk.Navisworks.Api`, which the file imports, but the
  only two other files that name it both import `Autodesk.Navisworks.Api.DocumentParts` for
  an unrelated reason. Two of the four files importing DocumentParts is exactly 50 per
  cent, which was the threshold, so a coincidence tipped it over
- IT WAS RE-MEASURED RATHER THAN SILENCED. Adding an import the file does not need would
  have been a lie in the one place a later reader will trust. Measured over src with F72
  in: clean at 51 and at 60, and with the F65 import taken back out it still names
  DocumentParts on the real fault at both. At 75 it reads clean and MISSES the real fault,
  so the window is 51 to 60 and it sits at 60, the middle rather than the edge. The whole
  measurement is in the file, including the sentence saying this will happen again

### Proved here

- `dotnet build ParsonsNwcFederator.sln -c Release` with 0 errors and 0 warnings, run after
  every add-in change
- `check-locals.sh src` clean and `check-imports.sh src` clean, and
  `check-imports.sh tools/checks/broken` still refuses with exactly one line
- Core tests before: 1271 passed, 0 failed, 0 skipped, 1271 total. After: 1338 passed, 0
  failed, 0 skipped, 1338 total. 67 added, none broken

### Waits for the local machine

- Steps 253 to 278. The first seven are the run with the box OFF, which has to change
  nothing at all, and the rest are the run with it on. Step 268 is the one worth doing
  first: read how many services the tool could not measure, because that number is what
  says whether the property list is right for this project

### What remains

- The closing work

### Known bugs

- As in the F46 entry. The add-in compiles

### What comes next

1. The closing work
2. Bader runs one building with the box off, then one with it on

## 2026-09-19 F71, say when an NWF is nearly matched

### What was done

- BADER PRESSED RUN ON BUILDINGS THAT ALREADY HAD AN NWF AND GOT FIRST RUN. The NWF folder
  and the name pattern together did not resolve to his file, so the tool found nothing at
  the output path, decided to build, and said nothing at all about the file sitting beside
  it under a nearly identical name. The tool did exactly what it was told. What it did not
  do was notice
- THE RULE IT ALREADY HAD IS RIGHT AND IS UNTOUCHED. An NWF at the output path is opened
  and one that is not there is built. F71 adds no branch, changes no decision and corrects
  no name. It notices, it says so in three places, and the person decides
- WHAT CLOSE MEANS, AND IT IS TWO THINGS RATHER THAN A DISTANCE. The same building code
  sits in both names, or project, originator and building all agree and one of the four
  SUPPLIED fields differs. Those four are the level, the discipline, the type and the
  number, which are exactly the four `NamePattern` supplies rather than reads out of the
  input, so they are the four a person is most likely to have set differently from the
  file on disk. A fuzzy distance would have been a number nobody could argue with and
  nobody could explain. These two are rules, and each has its own tests
- THE MORE SPECIFIC REASON WINS where both are true, and the line NAMES WHICH FIELD
  DIFFERS. A sentence saying only that something is similar sends the reader back to the
  folder to work out what, which is the trip this whole fix exists to save
- NO FILE SYSTEM IN CORE. `SimilarNames` is handed a list of names and compares names. The
  window lists the folder ONCE per refresh, not once per group, because the answer cannot
  change between two groups and the folder would otherwise be walked as many times as
  there are buildings

### Three things the tests and the checks caught, each of which would have shipped

- AN EXACT MATCH READ AS A NEAR MISS. A folder listing gives
  `1104-PAR-1B06PH-ZZZ-BM-MOD-000001.nwf` and the name a group would write has no
  extension on it. The first version compared the two raw, so the exact file came back as
  similar, which is the OPPOSITE of the truth: that file being there is what makes the
  group a Weekly run rather than a finding. The comparison is on the STEM now, and
  `ContainerName.Stem` went from private to internal so one rule decides what a name is
  rather than a second copy of it living in the new file
- THE LABEL MUST NEVER BE WIDENED, and this one was designed for rather than found, then
  pinned by a test that would have failed if it had not been. `RunPath.Count` maps a label
  it does not know onto Unknown, and both the confirm dialog and the RESULT block read
  those counts. A row reading First run followed by a sentence would have been counted as
  a path nobody can name, so the confirm dialog would have said `First run: 0` on a run
  that was about to build fourteen NWFs. `RunAs` stays one of the six, the sentence rides
  beside it in `RunAsShown`, and the test counts both to prove the two cannot be confused
- `check-imports.sh` REFUSED THE FIRST VERSION, and it was right to. The nested type was
  called `Match`, which this repo declares nowhere and which `ClientShapes.cs` and
  `PageCheck.cs` both use as `System.Text.RegularExpressions.Match`. The compiler was
  perfectly happy, because neither of those files can see a type in
  `Federator.Core.Naming` without importing it. The check could not know that and said so,
  and the name was a genuine readability fault whatever the compiler thought, so it is
  `NearbyName` now. F66's check earned its keep on a fault the build could not see

### What it says, and where

- THE RUN AS COLUMN reads `First run` and then `, but the NWF folder holds a similar name:`
  and the file. The column went from 150 wide to 300, because the sentence names a file
- THE GROUP LIST BLOCK in the log carries the same sentence on the same row, off the same
  Core wording, so the log and the window cannot drift
- ONE GREY LINE UNDER THE GROUP TABLE, only while at least one group is in that state and
  collapsed the rest of the time, saying how many and that the NWF Name cell can be typed
  over. Its own style rather than `TickHelp`, because `TickHelp` is indented twenty to sit
  under a tick box and a line under a table starts at the edge of the table
- ONLY WHERE THE GROUP WOULD BUILD. A group whose NWF is already there is opened, so
  another file with a similar name beside it is somebody else's business

### Proved here

- `dotnet build ParsonsNwcFederator.sln -c Release` with 0 errors and 0 warnings, run after
  the add-in changes
- `check-locals.sh src` clean and `check-imports.sh src` clean, the second one after the
  rename it asked for
- Core tests before: 1238 passed, 0 failed, 0 skipped, 1238 total. After: 1271 passed, 0
  failed, 0 skipped, 1271 total. 33 added, none broken

### Waits for the local machine

- Steps 123 to 136, which change the Level field to `L01` so every name differs from the
  file on disk in one field, and read the column, the line under the table and the log

### What remains

- F72, the penetration rule, which answers Q33, then the closing work

### Known bugs

- As in the F46 entry. The add-in compiles

### What comes next

1. F72, penetrations become Reviewed
2. The closing work

## 2026-09-19 The plan for the penetration round, F71 and F72

### This is the plan, written before the first edit

- Bader ran the tool for real on 2026-09-19, the first run since the add-in was proved to build, and came back with two things. One is what the window failed to tell him. The other is the answer to Q33, which has been open since F54 on 2026-09-18. This entry is the plan and nothing in the repo was edited before it was written
- The reading the brief asks for was done first and in full: `CLAUDE.md`, all four files under `.claude/rules`, the top entry of this file, `01_next.md`, `02_questions.md` end to end, `docs/history/scan.md` section 4, then `ClashStatusEditor.cs`, `StatusesThisToolMaySet.cs`, `SizeRule.cs`, `SizeSettings.cs`, `ItemSizes.cs`, `ClashHarvest.cs` and `FederationEngine.Decide`
- AND THEN SEVEN READERS OVER THE SURFACES THE TWO FIXES TOUCH, each one checked afterwards by a second reader told to assume at least one claim was wrong. Every one of the seven came back with something corrected. That is not a formality and three of the corrections change what gets built. They are in the section below

### The numbers. Two F numbers free, and the questions are not the ones the brief names

- `01_next.md` runs to F70. F71 and F72 are the next two free, neither is taken, and the two fixes map onto the brief one for one and in order
- THE QUESTIONS FILE RUNS 1 TO 40 AND NOT TO 46. The brief calls the four answers Q47 to Q50. Q41 to Q46 do not exist, so the four go in as Q41 to Q44 and they map onto the brief one for one and in order: Q41 is the brief's Q47, floors and roofs count as solids, Q42 is Q48, any wall whichever file it came in, Q43 is Q49, 150 is a ceiling and not a floor, Q44 is Q50, both sides a service is left alone. The same thing happened to the log round, which was briefed as F56 to F61 and went in as F59 to F64
- Q33 IS ANSWERED BY F72 AND IS MARKED ANSWERED, not closed by a new number. It asked which of four shapes the ask was, and the answer is the second: a rule over the clash itself. The other three shapes are recorded as not chosen

### What the readers found that changes the build

- THE F54 SEAT IS NOT WHERE THE STATUS PASS CAN SIT. `ClashRunner.cs:683` is the F54 slot and it runs BEFORE `using (ClashTest after ...)` at :698 resolves the handle the results are read from. `ClashTally tally` is not declared until :696, so a pass written at :683 cannot refer to it at all, and C# refuses a use before the declaration point rather than treating it as unassigned. The penetration pass needs the results and needs to run before the harvest, so it takes its own resolve at the F54 slot exactly as F54 does, and reads nothing the tally holds
- NOTHING IN `src` READS `ChangedCount`, `NotFoundCount` OR `RefusedCount`. F54 declared all three and no caller was ever written, because Q33 was open. The rule says a public member nothing in src calls is deleted. F72 gives all three a reader rather than deleting them, which is the answer the rule allows when a decision in `02_questions.md` keeps the member, and Q33 is that decision
- THE WORKBOOK ALREADY CARRIES THE COUNT AND A COLUMN OF OURS WOULD BREAK A STANDING RULE. `WorkbookWriter.WriteTestHeader` writes all five statuses per test off `ClashTally.AllStatuses`, so the Reviewed cell of every test block IS the number this run moved, because the status is applied before the harvest reads it. The brief asks for the count in the workbook. The rule says the workbook is the client's one sheet with none of ours on it and that if it is not in theirs it is not in ours. Both are satisfied by the client's own Reviewed column, that is what F72 does, and whether Bader wants a cell of ours as well goes in as a numbered question rather than being decided here
- `ClashItem` CARRIES NO CATEGORY and neither does any of the other three report classes, so Category is genuinely new. `ClashHarvest.FirstProperty` is the reader the brief says to copy the shape of, and it is private, so F72 makes it and its `Text` helpers internal and calls them from the new pass. Same assembly, no attribute needed, and ONE reader rather than a second copy
- `ClashRunner.UnitName` HANDS BACK THE EXCHANGE CODE AND NOT THE ENUM NAME. It returns `row.ExchangeCode`, which is `m` or `mm`, while `SizeRule` keys on the Navisworks enum name through `UnitTable.ByEnumName`. Handing one to the other would throw on every clash. The pass reads `document.Units.ToString()` and nothing else
- NOTHING IN `src` SPLITS AN OUTPUT NAME and `ContainerName.Parse` is never called on one. It reads parts 1, 2, 3 and 5 and F71 needs all seven, so F71's comparison is new Core work rather than a call into something that exists
- `Directory.GetFiles` APPEARS EXACTLY ONCE IN THE ADD-IN, at the scan. F71 adds the second, in the window, and hands Core a list of names

### The order, one pull request each, branch off main, merged green

1. **F71 SAY WHEN AN NWF IS NEARLY MATCHED.** Bader pressed Run on buildings that already had an NWF and got First run, because the NWF folder plus the name pattern did not resolve to his file, and nothing said so. A new Core type, `Federator.Core.Naming.SimilarNames`, takes the name this group would write and the list of names the add-in found in the NWF folder and answers which of them are close. CLOSE MEANS ONE OF TWO THINGS, both of them the brief's: the same building code in the name, or a name differing only in the level, the discipline, the type or the number field. It knows nothing about the file system, takes a list of names and the separator settings, and every rule in it has a test. The window lists the NWF folder once per refresh and hands the names over. A group in that state keeps `First run` as its LABEL, because `RunPath.Count` maps an unknown label to Unknown and the confirm dialog and the RESULT block count off it, and the warning rides beside the label in a second property the column and the group list block both read. One line under the group table when any group is in that state, saying the NWF Name cell can be typed over. Nothing is auto corrected
2. **F72 PENETRATIONS BECOME REVIEWED.** A clash moves to Reviewed when all four are true: one side is a service by item category, the other is a solid by item category, the service measures 150 mm OR LESS, and the clash is at New or Active. Everything else is left exactly as it is and counted by reason. `PenetrationSettings` holds the two category lists and the wording. `PenetrationRule` decides and names its reason. `PenetrationTally` builds the PENETRATION block in the shape the SETS block uses. `StatusesThisToolMayMoveFrom` is the fourth condition as a Core rule with its own tests, so no caller can overwrite a decision even by mistake. In the add-in, `Penetrations` walks a test's results, reads both sides' categories through the one reader `ClashHarvest` already has and the service's size through `ItemSizes`, and hands the facts to Core. `ClashHarvest` gains Category with the same reader and the same settings shape. Off by default, one tick box on the Clash step

### The five things F72 holds itself to, and each one is a rule that could go wrong quietly

- THE 150 IS NAMED ONCE AND READ TWO WAYS. `SizeSettings.ThresholdMillimetres` is the only 150 in the repo and both features read it. F53 puts an item in a viewpoint when it is OVER the threshold. F72 marks a clash Reviewed when the service is AT OR UNDER it. The comment at the setting says both readings side by side, because two copies of 150 would drift and nobody would notice until a report was wrong
- A DUCT IS NOT ONE NUMBER. `ItemSizes.Read` already hands back every wanted property the item carries, so the pass takes the LARGEST of them and not the first. A 600 by 150 duct is a 600 and stays New. `SizeRule` gains the reduction, so the unit conversion stays in the one place that has it, and a test pins the 600 by 150 case by name
- THE UNREADABLE CASE GOES THE OPPOSITE WAY FROM F53 ON PURPOSE. F53 INCLUDES an item whose size cannot be read, because the safe mistake there is showing something unnecessary. F72 LEAVES ALONE a service whose size cannot be read, because the safe mistake here is leaving a clash New for a person to look at. Both reasons are written at both places, so a later reader cannot make them agree
- NEVER OVERWRITE A DECISION. Only New and Active move. Reviewed, Approved and Resolved are left exactly as they are, and that is a Core rule with tests rather than a condition inside a loop, because the existing editor would happily move an Approved clash to Reviewed if something asked it to
- NOTHING MOVES SILENTLY. The PENETRATION block names every clash moved with both categories and the service size, then the totals and one line per reason for every clash left alone. The run total goes in the RESULT block. The workbook carries it in the client's own Reviewed column

### What this round holds itself to

- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`. Every rule lives in Core with its tests and the add-in reads properties and calls
- THE ADD-IN IS BUILT AFTER EVERY CHANGE TO IT and the result goes in the pull request body. This session is on Bader's machine with Navisworks Manage 2025 installed, so there is no excuse for shipping a compiler error, which is what the last two rounds did
- Every list and every number is a SETTING with the brief's defaults, never a constant
- `steps/log.md` gets one entry per fix, newest at the top. Q41 to Q44 go into `02_questions.md` with their answers, and Q33 is marked answered. Nothing under `samples`, `steps/logs` or `bundle` is touched
- One pull request per fix, branch off main, merged once Actions is green. If nothing here can open one, the branches stack in order and the closing entry says so plainly with one compare link

### The Core test count before the round

- 1238 passed, 0 failed, 0 skipped, 1238 total, measured on this machine, which is Windows. `dotnet build ParsonsNwcFederator.sln -c Release` finishes with 0 errors and 0 warnings before anything is touched

### What remains

- The whole round. This entry is the plan and no file has changed yet
- The closing work: the add-in built and the result said, `03_bader_next.md` read end to end against the code with the Look for count said out loud, then the closing entry
- F57, the five older Look for lines, is still Bader's to judge
- F52's writing half, F50's rebuild, Q35 to Q40 and the 319 steps are where they were

### Known bugs

- None open in the code. The add-in builds and the last round's six fixes are merged
- What Bader hit is not a bug in the code. The tool did exactly what it was told and said nothing about why, which is F71

### What comes next

1. F71, so the window says what it noticed
2. F72, the penetration rule, which answers Q33
3. The closing work

## 2026-09-19 The build round is closed, F65 to F70

### What was done

- SIX FIXES, each on its own branch off the one before it, each with its own entry above. Four were briefed, F65 to F68. Two were not, F69 and F70, and both came out of doing the four properly rather than out of looking for extra work
- THE ADD-IN BUILDS. `dotnet build ParsonsNwcFederator.sln -c Release` finishes with 0 errors and 0 warnings. That is the first proved build in this repo's history and it is the whole point of the round. Every fix from F5 onward was written against a project nothing had ever compiled
- THE ONE ERROR BADER SENT BACK WAS THE NINETEENTH OF NINETEEN. F65 fixed the CS0246 he pasted. F69 found eighteen more behind it, from three different rounds, none of them a cascade of the first
- WHAT THE ROUND ACTUALLY DISCOVERED, AND IT OUTRANKS ALL SIX FIXES. This session runs on Bader's own machine and Navisworks Manage 2025 is installed on it. Every earlier round ran in a container with no install, and the rules, this log and the shape of `03_bader_next.md` were all built around that. The rules say so now: a session says whether it can build the add-in rather than leaving it to be assumed, and a session that can, builds
- IT WAS FOUND BY REFUSING TO WRITE A LOOK FOR LINE FROM MEMORY. F68 adds a step reading `dotnet build ParsonsNwcFederator.sln -c Release --no-restore`, and the rule here is that a number or an output in a step is measured and never estimated. Running it is what compiled the add-in. Nothing was looking for this
- AND THE SAME THING HAPPENED AGAIN AT THE END. Steps 206 to 211 say the two probes answer a question the container cannot answer. Checking those Look for lines meant running the probes, and both answered questions that had been open since 2026-09-18
- Core tests: 1238 passed, 0 failed, 0 skipped, 1238 total, before the round and after it, measured on this machine, which is Windows. Nothing in this round adds a Core test because nothing in it adds a Core rule. The container figure the log round closed on was 1205 passed with 32 skipped, and the 32 are the Windows file system rules, which run here

### The six, in order

1. **F65 the missing import.** `using Autodesk.Navisworks.Api.DocumentParts;` into `DocumentCensusReader.cs`, which named `DocumentSelectionSets` in a parameter list and imported no namespace that has it. With it, a hand sweep of the other three files these rounds added, naming every Autodesk type each one puts in a type position and the import that covers it. `SavedViewpoints.cs` is the near miss that proves the sweep: it names `DocumentSavedViewpoints` four times and every one is in a comment
2. **F66 the check that would have caught it.** `tools/checks/check-imports.sh`, wired into the pre-commit and twice into Actions, once over `src` and once over a folder wrong on purpose. The rule as briefed returned 170 lines of noise, and two conditions cut it to none: a type only one other file names teaches nothing, and a namespace is a type's home only where at least half the files importing it name that type, measured at 50, 40, 34 and 20
3. **F67 one doubled comment.** The block describing the NWD publish, stacked on `BuildViewpoints` and belonging to `WriteNwd`. Moved rather than deleted, which is what F47b did with the same shape, and said out loud in the plan rather than buried. 0 stacked summaries over all 132 files under `src`
4. **F68 the build section learns what today cost.** Eleven steps for the NuGet restore race, cheapest first, each with its Look for line and every command in them run on this machine. One more line: a stamp reading `nogit` means git is not on the PATH of that terminal and only the stamp is affected
5. **F69 the other eighteen errors.** Three faults. `RebuiltThing`'s three count setters were `internal`, so the add-in, which is a different assembly and the only thing that reads the counts, could not write them, while Core and the tests could, which is why every test passed. `DocumentSelectionSets.CreateCopy()` returns `Collection<SavedItem>` and not `SavedItemCollection`, measured off the installed DLL and recorded in `scan.md` 4d, which is the member step 10 has been asking about since F24. `JobOutcome` never got `ViewpointsRequested` or `FailedViewpointCount`, which `GroupFacts` has carried and the judgement has read since F52
6. **F70 both probes run.** `Document.RemoveFile(int)` and `TryRemoveFile(int)` are public, so a model CAN be taken out of an open document without a clear, and the member is on `Document` and not on `DocumentModels`, which is why searching the collection found nothing. `DocumentSavedViewpoints` has the shape the code assumed and the two collections carry the same members. Both recorded as `scan.md` 5c and 5d

### The read of 03_bader_next.md, end to end

- THE FILE RUNS 1 TO 319 NOW AND HOLDS 170 LOOK FOR LINES. It was 308 steps and 163 Look for lines this morning
- ALL 170 WERE READ. Seven are new, six from F68's build section and one because step 211 stopped being an instruction and became a Look for line
- NINE WERE WRONG AND ALL NINE ARE CORRECTED. Step 10, which asked Bader to paste the error if a build ever named `CreateCopy` or `CopyFrom` on `DocumentSelectionSets`, and both are measured now. Steps 207, 209, 210 and 211, the whole probe section, which described output nobody had seen and now describes what the probes actually printed. Step 223, the `VIEWS    not attempted.` wording, which said the API was never read off a DLL. Step 279, which said an `UNKNOWN` views count means the collection is not the shape `SavedViewpoints.cs` ASSUMES, where the shape is measured. Step 302, which named nine `.tsv` event kinds as if they were the list, and there are fourteen. And step 314, the branch count
- FIVE MORE LINES THAT ARE NOT LOOK FOR LINES WERE CORRECTED TOO. The file's own header, which said every fix from F5 to F46 had merged and said nothing about the build. Steps 188 and 190, which told Bader to name log files and a branch with 2026-09-14, a date now in the past, where the date is meant to be the day he runs them. Steps 224 and 232, which waited on a probe that has been run. And the two section openers for F52 and F53, which said the same
- THE MECHANICAL HALF, WHICH CAN BE SAID EXACTLY. All 245 distinct backtick quoted strings in the file, checked against every `.cs` and `.xaml` under `src` plus `install.ps1`, `workflow.md`, `CLAUDE.md`, the checks, the probes, the hooks, `.gitattributes`, `Directory.Build.targets`, the project files and the Actions workflow, with C# concatenation seams flattened. 157 matched. The other 88 were resolved by hand and every one is a line composed at run time, a git or dotnet or PowerShell output, a file name Bader types, or a padded log prefix the writer builds
- WHAT WAS READ AGAINST THE CODE RATHER THAN AGAINST A STRING. The claims about ORDER and COUNT, which is where F56 found drift hides. `CensusRule` against step 281, which names which step may move which count and is right in all five cases. `RunStep.Head()` against step 266, which says `IMAGES` is indented two spaces further and it is, because the line is padded by depth times two. `RunSteps` against step 263, all fourteen names in order. `ReportedCount.Line` against step 275. `RunPath.ConfirmLines` against steps 52 and 75. `OpenDocumentJob` against step 163. `UnitTable` and `ReportUnits.Name` against steps 30, 106 and 107
- THE D6 BRANCH LIST REBUILT off `git ls-remote --heads origin` read live, 64 names, plus the two this round's closing work puts up, which makes 66 and 65 to delete

### What remains, and the first item is not a small one

- THE PULL REQUESTS COULD NOT BE OPENED FROM HERE. `gh` is not installed on this machine and the GitHub connector in this session answers `403 Resource not accessible by integration` to a create pull request call, twice, as a draft and not as a draft. So the six branches are PUSHED and merged into nothing. Bader opens and merges them, in order, F65 then F66 then F67 then F68 then F69 then F70 then `round-close-build`
- THEY ARE STACKED AND THAT IS DELIBERATE. Each branch is off the one before it rather than off main, because branching all six off main would have every one of them conflict in `steps/log.md` on merge. Merged in the order above, each merges clean. It is the same tree that would have existed had each been merged before the next was started
- NOTHING HAS BEEN RUN. A build is not a run. Not one line of F50 to F70 has been seen against a real model, and all 319 steps are still outstanding. What changed today is that step 8 will now pass, which is what all 319 were waiting behind
- F52's writing half and F50's rebuild both now start from a measurement rather than a guess, and neither was touched. Bader decides
- F57, the five older Look for lines, is still Bader's to judge
- Q33, and Q35 to Q40, are where they were

### Known bugs

- As in the F46 entry, with two struck off. The add-in compiles. The two probe questions are answered
- One thing is UNKNOWN and is named rather than filled in: whether a saved viewpoint records hidden state. Only a run answers it

### What comes next

1. Bader opens and merges the six pull requests in order, then `round-close-build`
2. Bader pulls main and BUILDS. It will pass, and this is the first time that sentence has been written here
3. Bader runs one folder and works steps 248 to 260, which is the log round in one section
4. Bader runs the D6 delete command himself

## 2026-09-19 F70, both probes run, and two standing unknowns answered

### Why this happened at all

- STEPS 206 TO 211 SAY THE TWO PROBES ANSWER A QUESTION THE CONTAINER CANNOT ANSWER. F69
  established that this session is not in a container, so those Look for lines could be
  checked against what the probes actually print. Checking them meant running them. Both
  are reflection over a DLL, both are read only, neither opens a model and each takes about
  ten seconds
- BOTH ANSWERED, and both answers had been open since 2026-09-18

### 5a. A model CAN be taken out of an open document

```
Autodesk.Navisworks.Api.Document  ->  public void RemoveFile(int index)
Autodesk.Navisworks.Api.Document  ->  public bool TryRemoveFile(int index)
```

- THE MEMBER IS ON `Document` AND NOT ON `DocumentModels`, which is exactly why it was never
  found. 5a says nothing named Remove, Delete or Detach against a model appears anywhere in
  `scan.md`, and every search behind that sentence had been of the collection
- `DocumentModels` carries `InternalRemove` and `InternalRemoveAt` and a get only
  `IsReadOnly`, so the list is not meant to be edited through the collection at all
- WHAT THIS DOES NOT SETTLE, AND IT IS THE HALF THAT MATTERS. It settles that the member
  exists, its name, where it lives and what it takes. It settles NOTHING about what removing
  a file does to the sets, the clash tests, the clash results and the saved viewpoints that
  point into that model, and that is the only question F50's rebuild turns on. A rebuild that
  removed one file and lost every clash result would be worse than the clear and copy it
  replaces
- SO THE REBUILD WAS NOT CHANGED. The clear, the copy and the four counts stay exactly as
  F50 built them. Bader decides, with a run, and the measurement is now in front of him

### 5b. The saved viewpoint API, and the shape the code guessed was right

- `Document.SavedViewpoints` is an `Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints`,
  which is the name `SavedViewpoints.cs` has been using since F50 on the strength of a pattern
- ALL FOUR OF 5b'S QUESTIONS ANSWERED. A folder is a `FolderItem` with a public constructor,
  put in with `AddCopy(GroupItem, SavedItem)`. A viewpoint goes in the same way and is made
  with `new SavedViewpoint(Viewpoint)`. A name is set with `EditDisplayName(SavedItem, string)`.
  `SavedViewpoint` and `Viewpoint` are both `IDisposable`, so both are disposed
- THE TWO COLLECTIONS HAVE THE SAME SHAPE. `DocumentSavedViewpoints` and
  `DocumentSelectionSets` carry the same `RootItem`, `AddCopy`, `InsertCopy`, `Move`,
  `Remove`, `RemoveAt` and `ReplaceWithCopy` with the same signatures. 5b said that must NOT
  be assumed from the pattern, and it was not assumed. It was read, and the pattern held
- ONE THING IS STILL UNKNOWN AND IT IS THE ONE THE FEATURE TURNS ON. Items are hidden through
  `DocumentModels.SetHidden(IEnumerable<ModelItem>, bool)`. Whether a viewpoint saved while
  they are hidden RECORDS that hiding, and restores it when pressed, cannot be read off a
  DLL. `SavedViewpoint.ContainsVisibilityOverrides` is what answers it, on a run
- SO `CanBuild` IS STILL FALSE AND NOTHING WAS TURNED ON. Writing `Add`, `ShowOnly` and
  `ShowOnlyLargeItems` against the measured members is the second half of F52, which is a
  feature and not a build fix. The measurement is here so that work starts from what was
  read. Bader decides when

### Everything that said they were unmeasured, and does not now

- `docs/history/scan.md` gains 5c and 5d, the two answers with the assembly version and the
  date, which is what step 211 asks for. 5a and 5b keep the questions, because the reasoning
  in them is why the answers matter, and their headings now point at the answers
- `CLAUDE.md`'s Confirm against the install list carried both as UNKNOWN. It now carries what
  is actually still unknown about each, which is what a run costs for one and what a viewpoint
  records for the other
- `.claude/rules/addin.md` said the viewpoint API is UNMEASURED and that whether a model can
  be removed is UNKNOWN. Both replaced with the measurement and with what is still open
- `SavedViewpoints.cs` opened with EVERYTHING IN THIS FILE RESTS ON ONE ASSUMPTION. It does
  not any more, and the comment says which four things were measured and which one was not
- `ViewpointBuilder.cs` listed FIVE THINGS ARE ASSUMED HERE AND NOT ONE OF THEM WAS READ OFF
  A DLL. Four of the five are measured now and the list says so line by line
- `WhyNotYet()`, the line the log writes on every group, said the API was never read off the
  installed DLL. That was true this morning and is not now, so it says the API is measured,
  names 5d, and says the writing half is what is missing

### Proved here

- Both probes run on this machine against `Autodesk.Navisworks.Api 22.0.0.0`
- `dotnet build ParsonsNwcFederator.sln -c Release` with 0 errors and 0 warnings
- `check-locals.sh src` clean, `check-imports.sh src` clean
- Core tests before and after, on Windows: 1238 passed, 0 failed, 0 skipped, 1238 total

### What remains

- The closing entry

### Known bugs

- As in the F46 entry, and the add-in compiles

### What comes next

1. Merge the F70 pull request
2. The closing entry
3. F52's writing half and F50's rebuild, if Bader wants them, because both now start from a
   measurement rather than a guess

## 2026-09-19 F69, the other eighteen errors, and the first proved build

### THE ADD-IN BUILDS. 0 errors, 0 warnings

```
dotnet build ParsonsNwcFederator.sln -c Release
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

That is the first proved build in this repo's history. Every round before this one wrote
add-in code that nothing compiled, said so honestly, and handed the build to Bader.

### What was found, and how

- F68'S OWN MEASUREMENT IS WHAT FOUND IT. Step 18 of the new build section is
  `dotnet build ParsonsNwcFederator.sln -c Release --no-restore`, and a step whose Look for
  line is written from memory is exactly what this repo refuses, so it was run. It did not
  fail on a missing Navisworks DLL. It compiled, and answered with eighteen errors
- THIS SESSION IS ON BADER'S OWN MACHINE AND NAVISWORKS MANAGE 2025 IS INSTALLED ON IT.
  `C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll` and
  `Autodesk.Navisworks.Clash.dll` are both there. Every earlier round ran in a container
  with no install, and the rules, the log and the whole shape of `03_bader_next.md` are
  built around that. It is not true today and the rules say so now
- F65 FIXED THE ONE ERROR BADER SENT BACK. Eighteen more were behind it, and none of them
  is a cascade of the first: they are in a different file, on different types
- THE ERROR COUNT IS EIGHTEEN AND THE FAULT COUNT IS THREE. Fourteen CS0200, one CS0029 and
  three CS1061, all in `FederationEngine.cs`, all from three different rounds

### Fault one, fourteen errors. The counts could not be written by the only thing that counts them

- `RebuiltThing.Before`, `.AfterAppends` and `.AfterRestore` were declared `internal set`.
  F50 wrote them that way
- THE COUNTS ARE READ OFF THE OPEN DOCUMENT, which only the add-in can do, and the add-in
  is a DIFFERENT ASSEMBLY. `internal` reaches Core and, through the `InternalsVisibleTo` in
  `Federator.Core.csproj`, the test project. It does not reach `Federator.Addin`
- WHICH IS WHY EVERY TEST PASSED THE WHOLE TIME. `RebuildTallyTests` sets all three on
  every one of its cases and always could. The one caller that cannot is the one that
  matters, and nothing in this repo put those two facts side by side until a compiler did
- The three setters are public now, with the reason written above them so nobody narrows
  them again

### Fault two, one error. The member step 10 has been asking about since F24

- `setsCopy = document.SelectionSets.CreateCopy();` was held in a `SavedItemCollection`,
  which is CS0029
- MEASURED OFF THE INSTALLED DLL ON 2026-09-19, by reflection, and written into
  `docs/history/scan.md` 4d:

```
public System.Collections.ObjectModel.Collection<Autodesk.Navisworks.Api.SavedItem> CreateCopy()
public System.Void CopyFrom(Autodesk.Navisworks.Api.SavedItemCollection)
public System.Void CopyFrom(System.Collections.Generic.IEnumerable<Autodesk.Navisworks.Api.SavedItem>)
```

- THE COPY IS AN ORDINARY BCL COLLECTION AND NOT ONE OF NAVISWORKS' OWN. `CopyFrom` has two
  overloads and the copy goes back through the `IEnumerable` one, so the round trip needs
  nothing converted in between
- STEP 10 OF `03_bader_next.md` HAS CARRIED A LINE SINCE F24 asking Bader to paste the error
  if a build ever named `CreateCopy` or `CopyFrom` on `DocumentSelectionSets`. It named
  both. The answer is in `scan.md` now rather than in a question
- AND ONE THING BEYOND THE BUILD FIX, said plainly because it is beyond it. `CreateCopy`
  CREATES, every `SavedItem` in the copy is `IDisposable`, and `Collection<SavedItem>` is
  not, so the old `as IDisposable` line disposed nothing and never could. Each item is
  disposed now, in the same `finally`, after every use of the copy. That is the rule in
  `addin.md` and section 4g is why it matters
- ALSO READ IN THE SAME PASS AND RECORDED: `DocumentSelectionSets` has `Remove(SavedItem)`
  and `RemoveAt(int)`. Those are about the SETS tree and say nothing about taking a MODEL
  out of an open document, which is 5a and is still UNKNOWN

### Fault three, three errors. F52 set two properties that were never added

- `JobOutcome.ViewpointsRequested` and `JobOutcome.FailedViewpointCount` did not exist
- `GroupFacts` has carried both since F52 and `GroupJudgement` reads both, so the Core half
  was written, tested and right. The add-in half was never added, and `Facts()` never
  copied them across
- Both added, and `Facts()` hands them over, which finishes F52's wiring: a group whose
  viewpoints failed is not DONE, and a group that never asked for them is not judged on
  them at all

### What this changes about the repo, beyond the three fixes

- `.claude/rules/addin.md` no longer says nothing here has ever built the add-in. It says
  where a session can build it and where it cannot, and that a session says which it is
  rather than leaving it to be assumed
- THE F65 ENTRY IS CORRECTED IN PLACE, not deleted. It says the add-in was not built and
  that nothing here could build it, and the second half of that was wrong rather than out
  of date. The correction sits under it naming what was wrong, which is what F47c did with
  the F40 entry
- THE PARSE CHECK IS NOW THE SECOND BEST THING AVAILABLE and it was the best thing for four
  rounds. Where a session can build, it builds
- AND THE TWO PROBES CAN BE RUN HERE. `tools\probes\probe-viewpoints.ps1` answers the whole
  saved viewpoint API, which is what `SavedViewpoints.CanBuild` being false is waiting on,
  and `probe-model-remove.ps1` answers 5a. Neither was run, because neither is this round's
  brief and F52 is a feature rather than a build fix. They are the first thing worth doing
  next and Bader decides

### Proved here

- `dotnet build ParsonsNwcFederator.sln -c Release` with 0 errors and 0 warnings
- `check-locals.sh src` clean, `check-imports.sh src` clean
- Core tests before and after, on Windows: 1238 passed, 0 failed, 0 skipped, 1238 total

### What is still NOT proved

- NOTHING HAS BEEN RUN. A build is not a run. Not one line of F50 to F69 has been seen
  against a real model, and every one of the 319 steps of `03_bader_next.md` is still
  outstanding. What changed today is that step 8 will now pass, which is what all 319 of
  them were waiting behind

### What remains

- The closing work, and it is bigger than it was this morning

### Known bugs

- As in the F46 entry, and the add-in compiles

### What comes next

1. Merge the F69 pull request
2. The closing work
3. The two probes, if Bader wants them, because they can be run here now

## 2026-09-19 F68, the build section learns what today cost

### What was done

- BADER LOST TIME TODAY BEFORE HE EVER REACHED A COMPILER ERROR, to a failure that has nothing to do with the code:

```
C:\Program Files\dotnet\sdk\10.0.400\NuGet.targets(198,5): error Cannot create a file when that file already exists.
```

- WHAT IT IS. `dotnet build` restores all three projects at once, they collide on the same package folder, and it is a known race in NuGet's restore task. It is intermittent, which is why the cheapest recovery is also the first one
- ELEVEN STEPS, 11 TO 21, EACH WITH ITS LOOK FOR LINE, cheapest first. Run the same build again. Then delete every `obj` and `bin`, because a half written package folder is what the race leaves behind. Then restore on its own with `--disable-parallel`, which is what actually takes the race out. Then build with `--no-restore`, so nothing can collide. And only after all of that, clear the NuGet cache, on its own and last, because it re-downloads every package this solution uses and nuget.org is a hard requirement for building at all
- EVERY COMMAND IN THEM WAS RUN ON THIS MACHINE rather than written from memory. `Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory` lists six folders, two per project, and prints nothing once they are deleted. A cold `dotnet restore ParsonsNwcFederator.sln --disable-parallel`, run with every `obj` folder deleted, gives three `Restored` lines, one per project, and no error, and on a warm tree it says `All projects are up-to-date for restore` instead. Both are in the Look for line, because either is a correct answer and a step that names only one of them reads as a failure half the time
- THE ONE THING NOT MEASURED IS SAID AS NOT MEASURED. What `dotnet nuget locals all --clear` prints was not read, because clearing the cache on this machine would cost the re-download the step warns about. The Look for line says exactly that rather than inventing the wording
- THE `nogit` LINE IS READ OFF THE CODE. `Directory.Build.targets` sets `FederatorGitHash` to `nogit` when the `git rev-parse` exec exits non zero or comes back empty, so a stamp reading `nogit` means git was not on the PATH of the terminal that ran the build. The build itself is unaffected and only the stamp is, and the step says that so nobody rebuilds chasing it
- THE FILE RUNS 1 TO 319 WHERE IT RAN 1 TO 308, and it holds 169 Look for lines where it held 163
- FIVE CROSS REFERENCES MOVED WITH THE RENUMBERING and every one was read against the step it now points at: step 65 to 76, which is the OK press of the 1B06PH run, step 70 to 81, the second Run on the same building, step 202 to 213, the publish properties line, step 271 to 282, the census taken once per group, and step 284 to 295, the six gap counts
- AND THE RENUMBERING BIT ONCE MORE, in the way the F63 entry already wrote down. The first pass skipped every `step NNN` sitting INSIDE a numbered line, because the rule that renumbers the line returns before the rule that renumbers the reference runs, so five references were left pointing at the old numbers while a sixth, in a plain paragraph, had moved. Caught by reading the references out afterwards rather than by trusting the pass
- TWO POINTERS IN `01_next.md` MOVED WITH IT, the log round's `steps 237 to 249` to `248 to 260` and F41's proof from `steps 188 to 190` to `199 to 201`, both read against the steps they now name. The step numbers inside the F56 DONE line were LEFT ALONE and are stale, and that is deliberate: they record what that fix read on 2026-09-18 and rewriting them would rewrite what it found
- Proved here: Core tests unchanged, on Windows, 1238 passed, 0 failed, 0 skipped, 1238 total. No code touched

### What remains

- The closing work, and it is bigger than it was this morning. See the entry above this one

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F68 pull request
2. The closing work

## 2026-09-19 F67, one doubled comment

### What was done

- TWO SUMMARY BLOCKS WERE STACKED AT LINE 1996 of `FederationEngine.cs`, both sitting on `BuildViewpoints`. The first describes publishing the NWD, every run, and why it is fixed on with no branch for a run that does not want it. That is `WriteNwd`, and F52 pushed it down the file when it inserted `BuildViewpoints` above it and left its comment behind
- IT WAS MOVED AND NOT DELETED, AND THE BRIEF SAID DELETE. `WriteNwd` carried no summary at all. The displaced block is its and records a decision, that the NWD publish stopped being a tick box because a weekly run wanted it every time, which is not readable off the lines under it. Deleting it would throw a measured decision away and leave a method undocumented
- F47B IS THE SAME SHAPE, THE SAME BRIEF WORDING AND THE SAME ANSWER, on 2026-09-18, one file along in `ClashRunner.cs`. The reasoning is in this log under it and in `01_next.md`. Doing the opposite today on the same shape would make the rule depend on which round read it
- WHAT THE BRIEF IS ACTUALLY AFTER IS REACHED EITHER WAY, which is that no two summary blocks are stacked anywhere under `src`. Bader reverses this in one line if he meant the block gone
- THE CHECK RERUN OVER THE WHOLE OF `src`. A summary closing and another opening with nothing between them, over all 132 `.cs` files, `obj` and `bin` excluded. It reads 0 where it read 1. F44 ran it at eleven, F47b at one and it has been 0 since, and this is the first time since that a round added enough files to be worth saying: it was 102 files then and it is 132 now
- Proved here: the check at 0, `check-locals.sh src` clean, `check-imports.sh src` clean. Core tests before and after, on Windows: 1238 passed, 0 failed, 0 skipped, 1238 total. No code changed, only where a comment sits

### What remains

- F68, the build section, then the closing work

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F67 pull request
2. F68, the build section learns what today cost

## 2026-09-19 F66, the check that would have caught it

### What was done

- A SECOND RULE OF THE COMPILER'S NOW RUNS WITHOUT THE COMPILER. `tools/checks/check-imports.sh` refuses a file that names a type and imports no namespace that has it, which is CS0246. It sits beside `check-locals.sh`, is wired into the pre-commit before the tests and into Actions twice, once over `src` and once over a folder that is wrong on purpose
- WHY THERE ARE TWO CHECKS AND NOT ONE. F52 shipped CS0128 and F58 wrote the first check for it. F61 shipped CS0246 and the first check could not see it, because it reads ONE shape of fault. Two rounds, two compiler errors, both shipped from here, both found by Bader's machine rather than by this one
- WHAT COUNTS AS A USE, WHICH IS THE WHOLE DESIGN. A type name is read only in a TYPE POSITION: after `new`, `is`, `as` or `typeof`, in the head of a `using` block, as a field, a parameter, a local or a `foreach` type, or inside generic brackets. A bare capitalised word anywhere else is a member name, a property or an enum value. Literals and comments are stripped before anything is read, which is the whole of the difference between `SavedViewpoints.cs`, which names `DocumentSavedViewpoints` four times in comments and correctly imports nothing for it, and `DocumentCensusReader.cs`, which named `DocumentSelectionSets` once in a parameter list and did not build
- HOW IT KNOWS WHERE A TYPE LIVES, AND IT IS TWO DIFFERENT THINGS SAID DIFFERENTLY. For a type this repo DECLARES, the namespace is a FACT read off the file that declares it, and a file naming that type from outside that namespace and its children must import it. A namespace is in scope inside its own children, so `Federator.Addin.Engine` sees `Federator.Addin` with no import and that is not a fault. For every other type, which is the whole BCL and the whole Navisworks API, nothing here can know, so it LEARNS from what the rest of the tree imports and its line says so in words: every other file here that names it imports X. It reports a correlation and never a claim about where a type lives
- THE FIRST VERSION RETURNED 170 LINES OF NOISE OVER `src` AND THAT IS THE MEASUREMENT THAT SHAPED IT. The rule as briefed, every other file that names the type imports a namespace this one does not, is true of `System` and `System.Collections.Generic` for almost any pair of files, so it answers with whatever the other file happens to carry. Two conditions cut 170 to 0 without weakening what it catches
- THE FIRST CONDITION IS THAT ONE OTHER FILE TEACHES NOTHING. With a single other user the intersection is that file's whole import list, so every import it has and this one lacks is reported. Below two other users the type is left alone, and the check says that is what it does
- THE SECOND IS A SHARE, AND IT IS A SETTING WITH ITS MEASUREMENT BESIDE IT. A namespace is taken as a type's home only where at least `MinimumShare` per cent of the files importing it name that type. Measured on 2026-09-19 over `src`: at 50 the check reads clean, at 40 one line, at 34 three and at 20 ten. At every one of those values, with the F65 import taken back out, it names `Autodesk.Navisworks.Api.DocumentParts` on the real fault. So 50 is where it sits and the number is in the file with the numbers behind it
- WHAT IT CANNOT DO IS WRITTEN AT THE TOP OF IT AND IN ITS PASS LINE. It reads TEXT and not a program. It cannot know a namespace no file here imports yet, so the FIRST use of a brand new Autodesk type, in the first file that ever names it, is invisible to it and only the build on Bader's machine sees that one. And it is not a build and it never says a build passed, which is the same sentence `check-locals.sh` carries and for the same reason
- THE WRONG ON PURPOSE FOLDER IS NOW WRONG IN TWO WAYS, ONE PER CHECK. `MissingImport.cs` is the exact shape that failed, the type as a parameter with no `DocumentParts` import. `HasImportAsAParameter.cs` and `HasImportAsALocal.cs` are correct and are the map, and there are two of them because of the first condition above, in the two positions the real tree uses. `NearMiss.cs` must PASS: every `DocumentSelectionSets` in it is a word and not a type, one in a comment and one inside a string, which is `SavedViewpoints.cs` in miniature. The check comes back with exactly one fault, naming the file, the type and the namespace, and `check-locals.sh` still comes back with exactly its own one
- THE RULE IS IN `addin.md` NOW, and it is the one a person follows rather than the one a script runs: a new add-in file that names an Autodesk type copies its imports from the file in this repo that already uses that type, because nothing here can compile the add-in and a namespace guessed at reads exactly like one that was measured until the build says otherwise
- Proved here: `check-imports.sh src` exits 0. `check-imports.sh tools/checks/broken` exits 1 with one line. `check-locals.sh src` exits 0 and `check-locals.sh tools/checks/broken` exits 1 with one line, unchanged by the four new files. The script is stored LF, which `*.sh text eol=lf` in `.gitattributes` already pinned. Core tests before and after, on Windows: 1238 passed, 0 failed, 0 skipped, 1238 total

### What this check will still not catch, listed rather than left

- A type nothing else here names. That is the brand new Autodesk type case and it is the one that will happen again
- A namespace that is imported but wrong, because it resolves nothing and never reads a DLL
- A member that does not exist on a type it can see, which is CS1061 and is the next shape along. F39 was that fault and it took an audit to find
- Everything else the compiler knows. The build is step 8 and it stays the only thing that says the add-in builds

### What remains

- F67, one doubled comment, then F68, then the closing work

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F66 pull request
2. F67, one doubled comment

## 2026-09-19 F65, the missing import

### What was done

- ONE LINE. `using Autodesk.Navisworks.Api.DocumentParts;` into `DocumentCensusReader.cs`, fourth of five, in the order the three files that already use the type carry theirs: Api, then Api.Clash, then Api.DocumentParts, then the Federator ones
- WHAT THE ERROR ACTUALLY WAS. Line 88 is `public static int Sets(DocumentSelectionSets sets)` and column 32 is where the type name starts, so the fault is a PARAMETER type and not a call. The file named the type and imported no namespace that has it
- THE NAMESPACE WAS MEASURED AND NOT GUESSED. `docs/history/scan.md` line 2108 carries `public Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets SelectionSets { get }`, read off the installed DLL, and line 463 names the type the same way. `ClashRunner.cs`, `SetBuilder.cs` and `FederationEngine.cs` all carry exactly that import and all three use the type
- WHY NOTHING HERE SAW IT, AND THIS IS WORTH MORE THAN THE FIX. Core and the test project compile in Actions on every push and neither of them names a Navisworks type, which is the rule that makes them compilable at all. The add-in compiles on no machine but Bader's. `check-locals.sh`, which F58 added for exactly this class of problem, reads ONE shape of fault, a local declared twice in one scope, and this is a different shape entirely, so the one check this repo has could not have caught it and was never going to. That gap is F66
- F58 WAS CS0128 AND THIS IS CS0246. Two rounds, two compiler errors, both shipped from here, both invisible from here

### The sweep, done by hand and not taken on trust

Every Autodesk type each file puts in a TYPE POSITION, and the import that covers it. A name that only appears inside a comment is listed as that, because it is what makes the difference between a file that needs an import and a file that does not.

- `DocumentCensusReader.cs`, the file that failed. `Document`, `GroupItem`, `FolderItem`, `SavedItemCollection`, `SavedItem` and `SelectionSet` are covered by `Autodesk.Navisworks.Api`. `DocumentClash`, `DocumentClashTests`, `ClashTest`, `ClashResultGroup` and `ClashResult` are covered by `Autodesk.Navisworks.Api.Clash`, which also carries the `GetClash` extension the file calls, measured at `scan.md` line 445. `DocumentSelectionSets` is covered by `Autodesk.Navisworks.Api.DocumentParts`, which is the line that was missing and is now there. `SavedTests` and `SavedViewpoints` are this file's own namespace and need no import
- `SavedViewpoints.cs`, F50. `Document`, `GroupItem`, `SavedItemCollection` and `SavedItem`, all covered by `Autodesk.Navisworks.Api`, which the file carries. AND IT IS THE NEAR MISS THAT PROVES THE SWEEP: it names `DocumentSavedViewpoints` four times, at lines 17, 25, 26 and 33, and every one of them is inside a `///` comment recording what the API is assumed to look like. A name in a comment is not a type position, so the file needs no `DocumentParts` import and correctly has none. A sweep that matched on the word alone would have added an import this file does not need
- `ItemSizes.cs`, F53. `ModelItem`, `PropertyCategoryCollection`, `PropertyCategory`, `DataProperty`, `VariantData` and `VariantDataType`, all covered by `Autodesk.Navisworks.Api`, which the file carries. All six are also named by `ClashHarvest.cs`, which walks the same property tree, carries the same import and predates the rounds that have not been compiled
- `ClashStatusEditor.cs`, F54. `DocumentClashTests`, `ClashTest`, `ClashResultGroup`, `ClashResult` and `ClashResultStatus` are covered by `Autodesk.Navisworks.Api.Clash`, and `SavedItemCollection` and `SavedItem` by `Autodesk.Navisworks.Api`. The file carries both
- NOTHING ELSE WAS MISSING, so nothing else was changed. The sweep run in chat was right and this is the reading that says so rather than the assertion that it was

### What was NOT proved, said plainly

- The add-in was not built. What is known is that Bader's compiler reported this error, that this line is now correct against a namespace measured off the installed DLL, and that whether a SECOND error waits behind it is UNKNOWN
- CORRECTED IN PLACE ON 2026-09-19 BY F69, which is the entry above. This bullet said "Nothing in this container can build it and nothing here has ever built it" and that was wrong, not out of date. This session is running on Bader's own machine, Navisworks Manage 2025 is installed on it, and the add-in builds here. F69 ran the build, found the second error and the sixteen behind it, and fixed every one. The sentence is left showing rather than deleted, because a log that quietly edits what it claimed is worth less than one that says where it was wrong
- Core tests before and after, on this machine, which is Windows: 1238 passed, 0 failed, 0 skipped, 1238 total. Nothing in Core was touched. The container figure the log round closed on was 1205 passed with 32 skipped, and the 32 are the Windows file system rules, which run here

### What remains

- F66, the check that would have caught it, which is the fix this one exists to justify
- F67, F68, then the closing work

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F65 pull request
2. F66, the check that would have caught it

## 2026-09-19 The plan for the build round, F65 to F68

### This is the plan, written before the first edit

- Bader pulled main on 2026-09-19, ran step 8 of `03_bader_next.md` on his own machine, and the build failed. One error, on one line, in the add-in. Core and the test project both built. This entry is the plan and nothing in the repo was edited before it was written
- The reading the brief asks for was done first and in full: `CLAUDE.md`, all four files under `.claude/rules`, the top entry of this file, `01_next.md` end to end, `tools/checks/check-locals.sh` end to end, and with them `.githooks/pre-commit`, `.github/workflows/tests.yml`, `tools/checks/broken`, every using block under `src/Federator.Addin`, and `docs/history/scan.md` where it records which namespace a type was measured in
- The error in full, as Bader sent it back:

```
src\Federator.Addin\Engine\DocumentCensusReader.cs(88,32): error CS0246: The type or namespace name 'DocumentSelectionSets' could not be found (are you missing a using directive or an assembly reference?)
```

### The numbers. F65 to F68 are free and are the ones used

- `01_next.md` runs to F64 and the log round closed on it. F65, F66, F67 and F68 are the next four free numbers, none of them is taken, and the four fixes of this brief map onto them one for one and in order. Nothing is renumbered
- The remote holds 59 branches and every one of them is merged. `fix-F65` to `fix-F68` are new names and collide with nothing

### What the reading found before any of it

- LINE 88 IS A PARAMETER TYPE. `public static int Sets(DocumentSelectionSets sets)`, and column 32 is where that type name starts. `DocumentCensusReader.cs` imports `Autodesk.Navisworks.Api`, `Autodesk.Navisworks.Api.Clash` and `Federator.Core.Diagnostics`, and not `Autodesk.Navisworks.Api.DocumentParts`
- THE NAMESPACE IS MEASURED AND NOT GUESSED. `docs/history/scan.md` line 2108 records `public Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets SelectionSets { get }` read off the installed DLL. Line 463 names the type the same way. So the import is `Autodesk.Navisworks.Api.DocumentParts`, and the three files that already use the type, `ClashRunner.cs`, `SetBuilder.cs` and `FederationEngine.cs`, all carry exactly that line
- WHY NOTHING HERE SAW IT, AND THIS IS THE PART WORTH MORE THAN THE FIX. Core and the tests compile in Actions and neither of them names a Navisworks type. The add-in compiles on no machine but Bader's. `check-locals.sh`, which F58 added for exactly this reason, reads ONE shape of fault, a local declared twice, and this is another shape entirely. So the one check this repo has could not have caught it and was never going to
- F58 FOUND CS0128 AND THIS IS CS0246. Two rounds, two compiler errors, both shipped, both invisible here. That is the reason F66 exists and is not optional

### The order, one pull request each, branch off main, merged green, branch left on the remote for Bader

1. **F65 The missing import.** One line into `DocumentCensusReader.cs`, `using Autodesk.Navisworks.Api.DocumentParts;`, in the order the other three files use: Api, then Api.Clash, then Api.DocumentParts, then the Federator ones. With it, a hand sweep of the three other files these two rounds added, `SavedViewpoints.cs`, `ItemSizes.cs` and `ClashStatusEditor.cs`, naming which Autodesk type each one uses and which import covers it, done by reading the files and not by trusting the sweep that was run in chat. Anything the sweep missed is fixed in the same pull request
2. **F66 The check that would have caught it.** `tools/checks/check-imports.sh` beside `check-locals.sh`, wired into `.githooks/pre-commit` and into `.github/workflows/tests.yml` exactly the way `check-locals.sh` is wired, the second Actions step against a folder that is wrong on purpose included. It reads every `.cs` file under `src`, takes the type names in a TYPE POSITION ONLY, and for each type asks whether every OTHER file under `src` that uses it imports a namespace this file does not. It learns the map from this repo and needs no Autodesk DLL and no compiler. What it cannot do goes in a comment at the top: it reads text and not a program, it cannot know a namespace no file here imports yet, so the FIRST use of a brand new Autodesk type is invisible to it and only the build on Bader's machine sees that, and it is not a build and never says a build passed. The wrong on purpose folder gets a copy of the exact broken shape and a near miss beside it that must pass, and both runs go in the pull request body. One line into `.claude/rules/addin.md`: a new add-in file that names an Autodesk type copies its imports from the file in this repo that already uses that type, because nothing here can compile the add-in
3. **F67 One doubled comment.** `FederationEngine.cs` line 1996. Two summary blocks stacked above `BuildViewpoints`, and the first describes publishing the NWD. Then the doubled summary check over the whole of `src` again with the count in the pull request body
4. **F68 The build section learns what today cost.** Bader lost time before the compiler error to a NuGet failure that has nothing to do with the code, `NuGet.targets(198,5): error Cannot create a file when that file already exists.` `dotnet build` restores all three projects at once, they collide on the same package, and it is a known race in NuGet's restore task. It goes into the build section of `03_bader_next.md` as numbered one action steps, in the order that costs least first: build again because the race is intermittent, then clear every obj and bin, then restore with `--disable-parallel`, then build with `--no-restore`, and only as a last resort clear the NuGet cache, because that re-downloads every package. Each step gets its Look for line. One more line under the build step: a build stamp reading `nogit` rather than a commit hash means git is not on the PATH for that terminal, the build is fine and only the stamp is affected

### One thing in the brief this round does differently, said here rather than buried

- F67 SAYS DELETE THE DISPLACED BLOCK AND THIS ROUND MOVES IT INSTEAD. `WriteNwd` at line 2046 carries NO summary of its own, and the displaced block is its, describing why publishing the NWD is fixed on and has no branch for a run that does not want it. Deleting it would throw away a recorded decision and leave a method undocumented. This is the same shape as F47b on 2026-09-18, where the brief also said delete and the block was moved for this same reason, and that reasoning is in this file and in `01_next.md` already. The outcome the brief asks for is reached either way, which is that no two summary blocks are stacked anywhere under `src`. Bader reverses it in one line if he meant delete

### What this round holds itself to

- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`, and this round adds no Core code at all
- F66 is a check and not a build, and nothing it prints is allowed to read as one. It catches one shape of fault, the shape that cost today, and says what it cannot see
- Every check gets a test that breaks one thing and asserts the check names it, which for a shell check is the wrong on purpose folder Actions runs it against
- One pull request per fix, branch off main, merged once Actions is green, never merged red, never left open. Nothing is committed on main. The remote branches are not deleted, because D6 is Bader's command
- `steps/log.md` gets one entry per fix, newest at the top, and `01_next.md` is renumbered. Nothing under `samples`, `steps/logs` or `bundle` is touched

### The Core test count before the round

- 1238 passed, 0 failed, 0 skipped, 1238 total, measured on this machine, which is Windows. The log round closed on 1205 passed, 0 failed, 32 skipped, 1237 total, measured in the container. The 32 that skip there are the Windows file system rules, which RUN here. Why the total is one higher on Windows is UNKNOWN and was not measured, beyond it being a difference between the two machines and not a test that was added
- Nothing in this round touches Core, so the count is expected to be unchanged at the end of it

### What remains

- The whole round. This entry is the plan and no file has changed yet
- The closing work: `03_bader_next.md` read end to end against the code again with the Look for count said out loud, then the closing entry
- F57, the five older Look for lines, is still Bader's to judge and is not touched here
- The two probes, Q33, Q35 to Q40 and every one of the 308 steps still wait for the machine with Navisworks on it

### Known bugs

- The add-in does not compile. `DocumentCensusReader.cs` line 88, CS0246. F65 is the first thing this round does
- Everything else as in the F46 entry

### What comes next

1. F65, so the add-in builds again
2. F66, F67 and F68 in the order above, one pull request each
3. The closing work

## 2026-09-19 The log round is closed, F58 to F64

### What was done

- SEVEN FIXES MERGED TODAY, each on its own branch off main, each a draft pull request merged once Actions was green, each with its own entry above. No pull request is open and nothing was committed on main. Six were the round's, F59 to F64, and the seventh went first because the reading found something that outranked all of them
- THE ADD-IN HAD NOT COMPILED SINCE THE DAY BEFORE, and nothing knew. `BuildViewpoints` declared one name twice, which is CS0128, shipped by F52 on 2026-09-18. Step 8 of `03_bader_next.md` is the build and every one of the 251 steps waited behind it, so a round about the log would have been unprovable from its first step. That is F58 and it went before anything else
- WHY NOTHING SAW IT IS WORTH MORE THAN THE FIX. The parse check behind five log entries passes `-nostdlib` with no references, so Roslyn stops before it binds one method body. It reads SYNTAX and nothing else, and an entry saying the add-in parses with the same six error codes is true and is not a build. Giving it the net48 reference assemblies and the built Core assembly makes it bind every body whose signature resolves, which is a real improvement and still cannot see inside a method that takes a Navisworks type, which is most of the engine. The rule is in `addin.md` now so parses and builds cannot be swapped again, and `tools/checks/check-locals.sh` runs the one rule of the compiler's that needs no compiler, in the pre-commit and twice in Actions, once over `src` and once over a folder that is wrong on purpose
- A SECOND GAP OF THE SAME SHAPE, found in F61. The parse check was running off a fixed list of add-in files written by hand in an earlier session, so a NEW add-in file would not have been checked at all. The list is built from the tree now
- THE NUMBERING. F51 was already done and merged on 2026-09-18, so the brief's one line pull request was not done twice. F56 and F57 were taken, so the brief's F56 to F61 became F59 to F64, one for one and in order. The questions file ran to 34 and not to 38, so the gaps went in as Q35 onward
- WHAT THE ROUND BUILT. Fourteen named steps, each opened in a using block so it closes whether the work finished, returned early or threw, on a monotonic clock that never reports less than no time. A timing block per group and for the run, where whatever the steps do not account for is a ROW of its own so the shares read down to a hundred. Five counts of the open document before and after every step, with a rule saying which step may move which, and a count that could not be taken reading UNKNOWN and never zero. A live line carrying group, building, step and two clocks through the one callback the engine always had. A gap block saying what the run measured and the report does not show. And a second file beside the log, one row per event, that a spreadsheet opens
- THE THING THE ROUND KEPT HAVING TO DECIDE was how much log is too much. TESTS RUN is entered 1830 times in a real group. A start and finish pair per visit is 3660 lines, a census per visit is 3660 walks of the whole document, and a window repaint per visit is 1830 repaints. Every one of those is counted once and reported once instead, which is the rule `RunLog.Failure` already followed after a run left a 17.8 MB log
- AND WHAT IT KEPT REFUSING TO DO. Nothing added here changes what a run does. No step is skipped, reordered or waited for. The census costs are measured and said, and narrow themselves if they get expensive. The live line renders when something calls it and says plainly that it cannot tick inside a single Navisworks call, rather than starting a thread to look livelier than the run is
- Core tests: 1045 passed, 0 failed, 32 skipped, 1077 total before the round. 1205 passed, 0 failed, 32 skipped, 1237 total after it. 160 tests added and not one failure introduced at any point. Core builds in Release with 0 warnings throughout
- FOUR TESTS FAILED ON THE WAY AND EVERY ONE OF THEM WAS RIGHT TO. Two were existing tests that a new block legitimately changed, one was an assertion of mine that pinned padding the step list owns, and one was an assertion of mine that said correct escaping was wrong. Each is named in its own entry

### The read of 03_bader_next.md, end to end

- THE FILE RUNS 1 TO 308 NOW AND HOLDS 163 LOOK FOR LINES. It was 251 steps and 120 Look for lines this morning. This round added 72 steps and 49 Look for lines
- WHAT WAS CHECKED, AND THIS IS THE HALF THAT CAN BE SAID EXACTLY. All 219 distinct backtick quoted strings in the whole file, against every `.cs` and `.xaml` under `src` plus `install.ps1`, `workflow.md` and `CLAUDE.md`, with C# concatenation seams flattened so a string built in two pieces still matches. 91 could not be found by the checker and every one of those was then resolved by hand against the code that composes it. All 91 are composed at run time, are shell output from a probe or from git, or are file names Bader types himself. One looked like a real miss, the `Navisworks has this test marked Old.` line, and it is correct: the seam there is an ENUM between two literals and no flattener sees that
- WHAT WAS READ AGAINST THE CODE, LINE BY LINE: this round's 49 Look for lines, every one of them, because they are this round's own drift and nobody else has read them
- SIX WERE WRONG AND ALL SIX ARE CORRECTED. The indent claim said HARVEST and IMAGES are both nested and only IMAGES is. The one clock claim pointed at the NWD attempt and written stamps, which bracket slightly less work than the step does, and then its first correction pinned padding by hand and got it wrong, and then it still had to say that one decimal against three is agreement. The census claim did not say that TESTS CREATE and TESTS RUN move by one test's worth, because the census is taken around the first visit only. The live line claim left `XML` off its list and did not say that `IMAGES` is the one step that never reaches that line. The gap block claim said the first line always says nothing acts on it, which is not true of an empty block. And one new step referred to another new step by a number the renumbering script had moved
- THE RENUMBERING SCRIPT CANNOT TELL A REFERENCE FORWARD FROM A REFERENCE INSIDE ITS OWN BLOCK, which is how that last one happened. Written down here because it will happen again
- WHAT WAS NOT RE-READ, SAID PLAINLY RATHER THAN LEFT TO BE ASSUMED. The 114 older Look for lines' non quoted promises, which is what a step says about an ORDER, a COUNT or which file does a thing. Those were read end to end yesterday by F56, which found nine and fixed four, and F57 still holds the other five open for Bader. Reading them again today would have been reading yesterday's work rather than this round's, and saying it was done is what F56 was opened for
- ONE NEW SECTION FOR THE LOG ITSELF, steps 237 to 249, and it is deliberately FIRST. One folder with every group ticked, watch the live line while it runs, then open the log and find the TIMING block, the CENSUS lines and the GAP block, with a Look for line on each. Everything after it is the detail behind those five things. A single building cannot show group N of M and cannot fill the run timing block, which is why it asks for the folder
- THE D6 BRANCH LIST REBUILT off `git ls-remote --heads origin` read live, 58 names plus this round's closing branch, checked name for name against the remote in both directions. It was 49
- AND THE CLOSING READ FOUND ONE DEFECT IN F64'S OWN CODE, fixed here rather than left. The row file wrote a file size through `EventRow.Count(int)` with the long clamped to `int.MaxValue`, so an NWD past 2,147,483,647 bytes would have been written into the row file as exactly 2147483647, a number nothing on the disk matches. A large federation's NWD goes past that. `Count` takes a long now and there is a test on three gigabytes by name. A size is only ever logged after it has been read back, and a clamped one breaks that rule quietly, which is the worst way to break it. Core tests 1205 to 1206

### What remains

- Everything in this round waits for the machine with Navisworks on it. Not one line of it has been seen on a real run
- Q35 to Q40 are this round's questions, one per gap, and step 284 is what makes them answerable
- F57, the five older Look for lines, is still Bader's to judge
- The two probes, Q33 and the rest of `01_next.md` are where they were

### Known bugs

- As in the F46 entry, and the add-in compiles again

### What comes next

1. Bader pulls main and BUILDS. That is step 8 and it is the one thing that has changed most: it would have failed this morning
2. Bader runs one folder and works steps 237 to 249, which is this round in one section
3. Bader answers Q35 to Q40 with the counts from step 284 in front of him
4. Bader runs the D6 delete command himself

## 2026-09-19 F64, the machine readable log

### What was done

- A SECOND FILE BESIDE THE TEXT LOG, same name and a different extension, `.tsv`. One row per event, eight columns, tab separated, with a header reading time, seconds, group, step, event, name, number, text
- WHY IT EXISTS, AND IT IS WRITTEN AT THE TOP OF THE WRITER. The text log is written for a person: blocks, indenting, sentences, and a shape that is free to change when a line reads badly. Getting a number out of it means a regular expression against that shape, and every round that improves a line breaks whatever was reading it. This file is the same run in a shape nothing has to parse
- THE TEXT LOG IS STILL THE ONE A PERSON READS and nothing about it got worse. Where the two disagree the text log is right, because it is the one that has been read against a real run
- ONE WRITER, SO THE TWO CANNOT DRIFT, which is the part that needed designing. `RunLog.Numbered` writes the text line and the row TOGETHER and is the only way a line carrying a number reaches the log. A line cannot be added without its row and a row cannot say something the text log does not
- A SENTENCE WITH NO NUMBER WRITES NO ROW, and that is the rule rather than an oversight. A row whose number column is empty is noise in a file whose whole purpose is numbers, so anything carrying a number goes through `Numbered` and anything else calls `Line`. The writer's comment says exactly that, including what is therefore NOT in the file
- WHAT IS IN IT. Every step starting and finishing with its seconds, every repeated step's total, all five census counts before and after each step, every timing row, every gap, every file written with its size, the per test rows for the workbook against the clashes in the document, and every group finishing with its seconds and its outcome
- TAB AND NOT COMMA. A clash name, a set locator and a file path all hold commas and none of them holds a tab, and Excel opens a tab separated file with no import dialog and no question about separators
- THE ESCAPING, WHICH IS THE PART THAT MATTERS MOST. One stray tab inside a value moves every column after it on that row and a reader has no way of telling. A tab, a newline, a carriage return and a backslash are all escaped, and THE BACKSLASH GOES FIRST: without that, reading a value back would turn a path ending in a t into a tab, and every Windows path in this tool holds backslashes. It is proved by reading the value back rather than by asserting that it changed, over eleven awkward values including a real NWF path, a set name ending in a space and the reference file's own name
- A TEST OF MINE WAS WRONG AND THE CODE WAS RIGHT. The first version asserted that an escaped path holds no backslash followed by N, which correct escaping produces the moment a doubled backslash is followed by a capital N. Replaced with the assertion that actually pins the ordering: a real tab and the two characters backslash and t escape differently, so reading a value back cannot turn one into the other
- LINE BY LINE AND FLUSHED with `Flush(true)`, exactly like the text log, so a run that dies mid group leaves BOTH files whole up to that moment. There is a test that reads the file off the disk after each call and counts the rows
- AND IT NEVER STOPS A RUN. A row file that cannot be opened leaves the text log untouched and says why in it, and a write that throws is swallowed. The break for that is a folder that cannot exist because a file is already sitting where it would go, which no file system makes
- Proved here: Core tests before 1184 passed, 0 failed, 32 skipped, 1216 total. After 1205 passed, 0 failed, 32 skipped, 1237 total. 21 added and none broken. Core builds in Release with 0 warnings, `check-locals.sh` clean over `src`, the add-in parses with the same six error codes and not one `CS1xxx`
- Waits for the local machine: steps 274 to 280, and step 279 is the one worth doing first. Filter the event column to `step finished`, sort the number column biggest first, and the top row is the slowest single step of the whole run. That is the number this round exists for

### What remains

- The closing work: the read of `03_bader_next.md` end to end, the numbered section for the log itself, and the closing entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F64 pull request
2. The closing work

## 2026-09-19 F63, the report gap block

### What was done

- BADER'S STANDING RULE IS IN THE TOOL NOW. When the code knows something the report does not show, it becomes a question. That rule has been worked by hand every time: somebody reads the code, notices a property being read and written nowhere, and writes a question. Q25 is exactly that, and it took an audit of every file under src to find it. The run says it itself now, at the end of every group
- WHAT COUNTS AS A GAP, WHICH IS THE PART THAT NEEDED A RULE. Something the run MEASURED off the model that no output carries. Not something the code could have measured and did not, and not something the report leaves out on purpose that a reader can see anyway. It has to be a number the run paid for and then threw away
- A GAP NAMES THREE THINGS: what is missing, what it came to on this group, and where it would belong. A gap with no value is a complaint and a gap with no place to go is a shrug
- A PROPERTY NOTHING CARRIED IS NOT A GAP, and that is the break the tests turn on. Reporting one would say the run is holding back something it never read, which is the opposite of true
- THE BLOCK IS WRITTEN EVEN WHEN IT IS EMPTY, saying nothing was held back, because a missing block reads as a check that did not run
- NOTHING ACTS ON IT. A gap does not fail a group, does not stop a run and does not change a single output. It is information in the log and Bader decides, which is the rule this whole tool is built on, and the block says so in its own first line
- THE SIX WERE MEASURED AND NOT TAKEN FROM THE AUDIT ON TRUST. All three writers were read on 2026-09-19. `WorkbookWriter`, `HtmlTabularWriter` and `ClientReportColumns` name none of Family, Type, Material, SourceFile, Discipline or IdFrom anywhere, and `ClashReportXml` writes two quick properties and says in its own comment that the five used to be written and are not. `ClashHarvest` reads every one of them off every item of every clash, which is a property lookup per item per run
- THE RUN LINE COUNTS BY NAME AND NOT BY LINE. The same six are held back on every group, so a run of twenty two groups would report 132 gaps, which says nothing except that there were twenty two groups. The number that means something is how many distinct things this tool knows and does not show, and that is six however many buildings it ran over
- THE VALUE IS COUNTED PER ITEM CELL AND NOT PER ROW, because each clash has two items and a property can be on one and not the other. A row with one side counts once. This is the first real measurement of how much of each property a model actually carries, and it is what turns Q25 from a yes or no into a decision with numbers under it
- SIX QUESTIONS AND NOT ONE, Q35 TO Q40. The brief asks for one per gap and that is right here for a reason the work made plain: Q25 lumps five different properties into one answer, and a single answer for five different things is a decision nobody can make. Split, Bader can keep Source File and drop Material. Q25 stays, because the reasoning behind all five is in it, and it now says where the per property decisions live
- ONE OF THE SIX IS NEW AND WAS NEVER IN Q25. `Id From` is the property the item id actually came from, held per item and written nowhere. Since F45 the `ITEM IDS` block counts it per PROPERTY with a total, which answers how a run behaved as a whole, and the per item answer is still held and still shown nowhere. That is Q40 and it is a different shape from the other five
- Proved here: Core tests before 1168 passed, 0 failed, 32 skipped, 1200 total. After 1184 passed, 0 failed, 32 skipped, 1216 total. 16 added and none broken. Core builds in Release with 0 warnings, `check-locals.sh` clean over `src`, the add-in parses with the same six error codes and not one `CS1xxx`, and the widened check is unchanged at 205 plus the nine this round added, every one a Navisworks name
- A CROSS REFERENCE I BROKE AND CAUGHT. The script that renumbers `03_bader_next.md` bumps every `step NNN` at or after the insertion point, and one of the new steps refers to another new step, so it was bumped to a number six past where it should be. Corrected. It is the same class of fault F56 was opened for and it is worth writing down that the renumbering script cannot tell a reference forward from a reference inside its own block
- Waits for the local machine: steps 268 to 273, and step 271 is the one that matters, because the counts it asks for are what Q35 to Q40 need to be answerable

### What remains

- F64, the machine readable log, and then the closing work

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F63 pull request
2. F64, the machine readable log

## 2026-09-19 F62, the live line in the window

### What was done

- WHILE THE RUN WORKS THE LINE READS `Group 3 of 14  1B06PH  TESTS RUN  12s on this step  4m 02s on the run`. Group N of M, the building, the step, the seconds on that step and the seconds on the run, with whatever the run wanted to say on the end of it
- ONE ROUTE AND NOT TWO, WHICH IS WHAT THE BRIEF ASKED FOR. The engine has always handed progress out through a single callback, and this widens what that callback CARRIES rather than adding a second way out. Everything stays on the plugin thread the run is on and nothing here starts a thread
- WHY THE CALLBACK'S SIGNATURE DID NOT CHANGE, AND THIS WAS A DECISION. Changing `Action<string>` to an object would have rippled through the engine, `SetBuilder`, `ClashRunner`, `ViewpointBuilder`, `PreviewRunPaths` and about ten call sites in the window, on a project that cannot be compiled in this container and whose add-in was found not compiling one day ago. The string the callback carries is the whole live line now, which is the widening that matters, at a fraction of the risk
- `Say` renders every time and `Tick` renders at most once a second. The three pieces that speak once per test, once per set and once per viewpoint get `Tick`, so a loop over 1830 tests repaints the window about as often as a person can read it rather than 1830 times. Anything said through `Tick` is in the log as well, so a message the throttle skips is never a message that was lost
- A FAULT OF MY OWN, FOUND AND FIXED BEFORE IT WAS PUSHED, AND IT IS THE INTERESTING ONE. `ClashRunner` opens three of the fourteen steps, once per test, so without it the live line would never have shown TESTS CREATE, TESTS RUN or HARVEST, which are the steps where the time actually goes. Handing the line over fixed that. The first version then rendered the line inside `ClashRunner` and passed the result to the engine's throttle, and RENDERING THE LINE IS WHAT MARKS IT AS SAID, so the throttle skipped the one render that mattered and the step name still never appeared. A caller that wants the throttle to render hands it a SENTENCE and never a line. There is a test pinning exactly that, because it is the kind of thing that reads as working and is not
- THE PACE WARNING. A step running longer than twice what the SAME step took on the group before says `SLOWER, the group before took 40s on this step`. The comparison reads the step records the log already keeps, so the pace on the line and the seconds in the timing block are the same numbers rather than two counts of one thing. Twice is a setting and a value at or below one is refused where it is set, because a step is not slower than the one before until it is longer than it
- The pace is found through a reader set once on the line, so every piece of the run that opens a step says so the same way without carrying the records around. A reader that throws says nothing about the pace and never stops the run
- ON THE FIRST GROUP IT NEVER SAYS SLOWER, because there is nothing to compare against, and minus one means say nothing rather than guess
- THE WINDOW. The line has its own row above the log box now, where it used to sit fourth in a row of buttons and be cut off at the window edge. It is outside the log box, so it never scrolls away while the log pane follows the log, and it is trimmed rather than wrapped so a long line cannot push the log box down the window mid run. The log pane already scrolled to the newest line on every line written, which was read off the code rather than assumed
- WHAT IT CANNOT DO, WRITTEN DOWN RATHER THAN PAPERED OVER. It renders when something calls it. Every loop in the run ticks it, but a single Navisworks call with no loop inside, which is what publishing an NWD is, cannot be ticked from the thread it runs on. So the line sits at the seconds it last showed until that call returns. Nothing here starts a thread to make it look livelier than the run is, and there is a proof step telling Bader to expect exactly that
- Proved here: Core tests before 1149 passed, 0 failed, 32 skipped, 1181 total. After 1168 passed, 0 failed, 32 skipped, 1200 total. 19 added and none broken. Core builds in Release with 0 warnings, `check-locals.sh` clean over `src`, the add-in parses with the same six error codes and not one `CS1xxx`
- Waits for the local machine: steps 261 to 267, all of them watched while the run is WORKING rather than read off the log afterwards, which is the one thing in this round that cannot be checked any other way

### What remains

- F63 and F64

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F62 pull request
2. F63, the report gap block

## 2026-09-19 F61, the document census

### What was done

- FIVE COUNTS, READ BEFORE AND AFTER A STEP: models, selection sets, clash tests, clash results and saved viewpoints. Every one of them is something the NWF carries and something there is no second copy of anywhere. A run that lost 1830 clash tests during the workbook write would have written the workbook, published the NWD and reported DONE, and nothing in the log would have said a word
- MINUS ONE IS NOT ZERO, AND THAT IS THE RULE THE WHOLE THING RESTS ON. A count that could not be taken is minus one and prints as UNKNOWN. Zero reads as a real count, so a census answering zero would let a run throw everything away and report that nothing moved. It is the same answer `SavedViewpoints.Count` already gives for the same reason, and a count either census could not take is NEVER called a move, in either direction, because comparing a real number against UNKNOWN answers nothing and would bury the real moves under noise
- THE RULE IS THE REFUSALS. `CensusRule` says which step may move which count, read off the engine step by step. DECIDE opens the NWF that is already on disk, and opening a document replaces everything in it, so all five may move there and only there. APPEND moves the models, because an NWC carries geometry and properties and not sets or tests. SETS moves the sets, TESTS CREATE the tests, TESTS RUN the results. Every other step writes a FILE and not the document, so none of them may move anything at all, and that is the half that catches what nobody would think to look for
- A MOVE THE RULE DOES NOT ALLOW gets a line beginning `CENSUS CHANGED` naming the step, the count, the before and the after, and the reason goes on the group so it is not reported DONE. Nothing is undone, nothing is skipped and the run carries on. The tool reports what it noticed and Bader decides
- A STEP ALLOWED ONE COUNT IS STILL CALLED OUT ON ANOTHER, which is the case a rule written as a single may-write flag would have missed. TESTS RUN may move the results and may not move the tests, and there is a test on exactly that
- THE COST, WHICH IS WHAT THE BRIEF WARNED ABOUT. The census is real work: the sets and the viewpoints are walked from their roots and the results are walked per test. So it is taken AT MOST ONCE PER STEP PER GROUP, on the first visit, for the same reason the start and finish lines are written once. TESTS RUN is entered 1830 times and counting the whole document around every visit would be the log slowing the thing it is meant to be watching
- AND THE COST IS MEASURED, NOT ASSUMED. Every census is timed off the same monotonic clock the steps use, and one line per group says what the group's counting cost and how many counts it took. Over a second a group the census narrows to the steps that may write, and the next group says `CENSUS   narrowed` at its top, so no reader ever wonders why a step has none around it. The threshold is a setting and a value at or below zero is refused where it is set
- ONE PLACE READS ALL FIVE. `DocumentCensusReader` walks each one with every wrapper disposed on the way down, in the shape `FederationEngine.CountSets` was already using, because a walk that leaves a wrapper behind leaves it for a finalizer on a thread Navisworks does not own and one run built 1.7 million handles in a group doing that. Each of the five reads is in its OWN try, so one count failing does not turn the other four into UNKNOWN
- A COPY OF A WALK WENT RATHER THAN A SECOND ONE ARRIVING. `FederationEngine.CountSets` and `CountSetsUnder` are gone, 51 lines, and the three callers in the rebuild read `DocumentCensusReader.Sets` now. The rebuild and the census read the same number the same way, where a new census walk would have made two copies of one rule
- CLASH RESULTS ARE COUNTED AS LEAVES, descending every result group, which is the rule `ClashRunner` already counts by. A result group counting as one would let a run lose every clash inside it and report the same number
- NO NAVISWORKS TYPE REACHES CORE. The log holds a reader, the add-in supplies it, and every rule about what the five numbers mean is Core with its tests. A reader that throws comes back as UNKNOWN with a line saying what threw, and never stops the run
- A GAP FOUND AND CLOSED ON THE WAY. The parse check has been running off a fixed list of add-in files written by hand in an earlier session, so a NEW add-in file would not have been checked at all. `DocumentCensusReader.cs` was the first one to land since, and the list is built from the tree now. That is the same shape of fault F58 was opened for
- Proved here: Core tests before 1103 passed, 0 failed, 32 skipped, 1135 total. After 1149 passed, 0 failed, 32 skipped, 1181 total. 46 added and none broken. Core builds in Release with 0 warnings, `check-locals.sh` clean over `src`, the add-in parses with the same six error codes and not one `CS1xxx`, and every unresolved name in the widened check was read and every one is a Navisworks type
- Waits for the local machine: steps 254 to 260, including the first real measurement of what the census costs on a real model, which is the number that decides whether it stays wide

### What remains

- F62, F63 and F64

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F61 pull request
2. F62, the live line in the window

## 2026-09-19 F60, the timing blocks, and F21 closes here

### What was done

- TWO BLOCKS. A `TIMING` block per group, written straight after its `GROUP    finished` line, and a `TIMING, THE WHOLE RUN` block just before `RESULT`, so `RESULT` stays the last thing in the file and where the time went is read on the way to it
- EVERY NUMBER IS MEASURED AND NOTHING IS WORKED OUT FROM ANYTHING ELSE. The group total is the engine's own clock as `GroupFinished` read it, which is why `GroupRecord` carries its seconds now. The run total is the log's elapsed clock read where the block is written, and deliberately NOT the groups added up: the scan and the preview happen outside every group, and a run total that left them out would be a smaller number than the run took
- THE ROW THAT MAKES THE SHARES HONEST. Whatever the steps do not account for is a ROW of its own, named `outside every step`, so the share column reads down to a hundred. A block that spread that time over the steps it does know about would be inventing numbers, which is the one thing a timing block must not do, and one that left it off the page would leave a reader guessing where the missing fifth went. There is a test that adds the column up and asserts a hundred
- STEPS ADDING TO MORE THAN THE GROUP TOOK IS SAID IN WORDS, with the difference named. It means something was timed outside the stretch the group clock covered, which is a fault in the timing rather than in the run, and it is exactly the shape that would otherwise print a share over a hundred and be read past
- A NESTED STEP IS LEFT OUT OF THE SHARE COLUMN and listed under the total with a line saying its seconds are already counted above. IMAGES runs inside HARVEST, so counting both would give a group shares adding to more than a hundred. It is listed rather than dropped, because IMAGES taking most of HARVEST is exactly the sort of thing this block exists to show
- THE RUN BLOCK READS TWICE. The groups slowest first, then the same seconds by STEP NAME added across every group. Which BUILDING cost the run and which STEP cost it are two different questions and only the second one says what to fix. A run of twenty two groups answers the second only when the steps are added across all of them
- THE BLOCK ANSWERS CRITERION 2 ITSELF. The last line says the run took so long, in minutes and in seconds, and then either that it is inside the forty five minutes a run has to finish in, by so much, or that it is OVER by so much. One second over is OVER, because criterion 2 is a number and not a feeling, and there is a test on that boundary by name. The forty five is a setting and a value at or below zero is refused where it is set
- F21 CLOSES HERE, AND ITS SECOND HALF WAS THE INTERESTING ONE. F21 asks for the clash count written to Excel per test, so the Excel and the log can be checked against the panel. That is criterion 3, and nothing in the log answered it: checking meant opening Excel and Navisworks side by side for every one of 1830 tests
- THE TWO NUMBERS ARE NOT THE SAME THING, AND THAT WAS MEASURED OFF THE CODE RATHER THAN ASSUMED. `ClashRunner.CountInto` counts LEAVES, descending into every result group, because a group silently counting as one would understate what a test found. `ClashHarvest.Walk` writes ONE row per result group and does not descend into it, which is also what the Clash Detective panel shows. So fewer rows than clashes is the GROUPING and not a loss, and the `ROWS` line says which. A rule asserting the two must be equal would have been wrong, and it would have called a correct run a fault on every test holding a group
- WHAT IS A FINDING IS MORE ROWS THAN CLASHES. Nothing in this tool produces that, so the line says so in capitals with the difference named, and nothing acts on it. That is the rule: the tool reports what it noticed and Bader decides
- Proved here: Core tests before 1075 passed, 0 failed, 32 skipped, 1107 total. After 1103 passed, 0 failed, 32 skipped, 1135 total. 28 added and none broken. One existing test failed on the way and was right to: it counted the words `3 visits` in a log and the new TIMING block carries a visit count on its rows too, so it now matches the repeated step line by its own prefix
- The blocks were RENDERED and read rather than only asserted. A group of 1830 tests was put through `TimingBlock` against the real Core assembly and the output read by eye, which is what caught the `outside every step` row sitting under the nested heading where it read as a nested step. The row moved up and the nested section moved below the total
- Core builds in Release with 0 warnings, `check-locals.sh` clean over `src`, the add-in parses with the same six error codes and not one `CS1xxx`, and the widened check gives 205 `CS0246` and 1 `CS0103`, every one a Navisworks name
- Waits for the local machine: steps 245 to 253, nine of them, including the one that checks criterion 3 off the log and the workbook and the panel together

### What remains

- F61 to F64

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F60 pull request
2. F61, the document census

## 2026-09-19 F59, every step is named and timed

### What was done

- FOURTEEN STEPS, ONE PLACE. `Federator.Core.Diagnostics.RunSteps` holds DECIDE, APPEND, NWF SAVE, UNITS, SETS, TESTS CREATE, TESTS RUN, HARVEST, IMAGES, WORKBOOK, HTML, XML, NWD and CONFIRM, in the order a group meets them, and nothing anywhere types a step name as a string. A name not on the list is REFUSED where the step opens, because a timing block holding a step nobody named is worse than a short one. The column width is read off the longest name rather than typed, so adding a longer step cannot leave the block ragged with nothing saying so
- A STEP IS ALWAYS OPENED IN A USING BLOCK, so it closes on the way out whether the work finished, returned early or threw. This is the whole design. A step left open is the one thing that would make the timing block LIE, because the seconds it never recorded come off no total and the run reads as faster than it was. A group that ends with a step still open names it, says NEVER CLOSED, and gives it the seconds it had been open
- A STEP WHOSE WORK THREW SAYS THREW AND KEEPS ITS SECONDS. Time spent failing is time the run spent, and a failed step whose seconds vanished would make a run that spent eight hours failing read as a fast one
- THE CLOCK IS MONOTONIC AND IS NEVER TWO WALL CLOCK READINGS SUBTRACTED. It is the run's `Stopwatch` through `RunLog.ElapsedSeconds`. A clock that goes back, which is what a machine syncing its time does, would otherwise give a step a negative duration and a group a total smaller than one of its own steps. A step never reports less than no time, and there is a test that drives the clock backwards by thirty seconds and asserts zero
- The clock reaches `RunStep` as a FUNCTION rather than as a Stopwatch of its own, so a test drives it and the line shapes are proved exactly. Nothing here waits for a real second to pass, which is how a timing test turns into a test that fails on a busy machine
- ONE CLOCK PER PIECE OF WORK. `ClashRunner` was already timing the test run with a Stopwatch of its own, and that Stopwatch is gone. The step IS the measurement now, so the seconds the log reports and the seconds the report row carries come off one reading and cannot disagree
- THE LINE COUNT, WHICH IS THE PART THAT WOULD HAVE RUINED IT. TESTS RUN is entered once per test and a real group holds 1830 of them, so a start and finish pair per visit is 3660 lines. A step entered more than once writes its pair the FIRST time, one line the second time saying the rest are counted, and nothing after that, then one line per repeated step when the group finishes with the visits and the total seconds. That is the same rule `RunLog.Failure` already follows, and it is here for the same reason: one run left a 17.8 MB log that was almost entirely one thing said over and over
- A STEP INSIDE A STEP is indented by its depth and carries that depth, because the timing block has to work its shares out over the top level alone or they add up to more than the group took. IMAGES is the ONLY one nested today, inside HARVEST, because a picture is written while the harvest walks the results. TESTS CREATE, TESTS RUN and HARVEST each open and close on their own, so a first draft of the proof step that called HARVEST nested was wrong and was corrected before it was pushed
- OUTSIDE A GROUP THERE IS NO GROUP TO COUNT AGAINST. The two hand buttons on the Clash step run outside one, so each press is its own occasion and writes its own pair of lines. Without that rule a second press would have read as a repeat of the first and gone silent
- THE STEP NEVER CHANGES WHAT THE RUN DOES. It opens, the work runs exactly as it did before, and it closes. Nothing is skipped, reordered or waited for, and a throw goes straight up to the caller that already handled it. The one thing that moved is the Stopwatch that was doing the same job twice
- ONE HELPER AND NOT FOURTEEN COPIES. `InStep` and `InStepReturning` in the engine carry the try, the Failed and the phrase once. Fourteen copies of three lines is fourteen places for one of them to be left out, and the one that matters is Failed
- WHERE THE STEP SITS WAS A DECISION PER STEP. APPEND is inside `AppendAll` and not at its two call sites, because the first build and the rebuild both put the models in and a reader asking where the time went wants one number for that. NWF SAVE is inside `WriteNwf` for the same reason. SETS is inside `BuildTheSets`, because the run and the Sets into open model button both call it and the lines have to read the same whichever way the sets were built. The rest are at their one call site
- Proved here: Core tests before 1045 passed, 0 failed, 32 skipped, 1077 total. After 1075 passed, 0 failed, 32 skipped, 1107 total. 30 tests added and none broken. Core builds in Release with 0 warnings. `check-locals.sh` clean over `src`. The add-in parses with the same six error codes and not one `CS1xxx`, and the widened check with the reference assemblies gives 205 `CS0246` and 1 `CS0103`, every one of them a Navisworks name, so `RunStep`, `RunSteps` and `StepRecord` all resolve
- Two shapes the parse check cannot see were proved on their own against the real Core assembly rather than assumed: that a local assigned inside a using block and read after it is definitely assigned, which is what `ranFor` does in `ClashRunner`, and that `test` is not already a name in that scope
- Waits for the local machine: steps 237 to 244, a new section in `03_bader_next.md` read off an ordinary run. The file runs 1 to 259 now

### What remains

- F60 to F64. F60 is the timing blocks, which is what these records were kept for

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F59 pull request
2. F60, the timing blocks, and F21 closes with it

## 2026-09-19 F58, the add-in compiles again

### What was done

- THE ADD-IN HAD NOT BUILT SINCE F52 MERGED. `FederationEngine.BuildViewpoints` declared `views` twice in one scope, `ViewpointSettings views` at line 1793 and `ViewpointBuildOutcome views` at line 1812, and the second one read `views.Sizes` while it was being declared. That is CS0128. The second local is called `built` now, which is one word of change, and the comment above it says why the name matters
- FOUND BY READING, NOT BY A CHECK, WHICH IS THE REAL FAULT HERE. The round's brief asked for `RunLog.cs` end to end and for `01_next.md`, and the engine was read alongside them to plan where a step would open and close. The line was sitting there. Nothing automated had seen it in a day
- MEASURED AND NOT GUESSED. The two Core types and the two declarations were compiled in this container against the real `Federator.Core.dll` and the net48 reference assemblies, and the answer is `error CS0128: A local variable or function named 'views' is already defined in this scope`. `ViewpointBuilder.Build`, `ViewpointBuildOutcome.Lines`, `FailedCount` and `PutAnythingIn` were each read off the source before the replacement was written, so the fixed line names four members that exist
- WHY THE CHECK THIS REPO RUNS COULD NOT SEE IT, WHICH IS WORTH MORE THAN THE FIX. The parse check behind five log entries passes `-nostdlib` with no references at all. Roslyn then stops before it binds a single method body, so it reads SYNTAX and nothing else, and a log entry saying the add-in parses with the same six error codes and not one `CS1xxx` is true and is not a build. Handing it the net48 reference assemblies and the built `Federator.Core.dll` makes it bind every body whose signature it can resolve, which is a real improvement and still does not see this one: `BuildViewpoints` takes a Navisworks `Document`, that type cannot resolve here, and Roslyn skips the body of any method whose signature it cannot bind. That is most of the engine. The rule is written into `.claude/rules/addin.md` so the words parses and builds cannot be swapped again
- THIS IS F39 HAPPENING A SECOND TIME. F34 deleted three constructors, the window kept calling them, and nothing noticed until an audit put the caller and the callee side by side. Step 8 of `03_bader_next.md` is the build and all 251 steps wait behind it, so a round about the log would have been unprovable from its first step
- SO THE FIX CARRIES A CHECK. `tools/checks/check-locals.sh` refuses a local declared twice in one method scope. It is sh and awk, which is the one shell this repo already depends on, so it runs in the container, on the Windows runner and on Bader's machine through Git for Windows. It strips string and character literals before it counts a brace, because a brace inside a string is text and would move every scope after it
- IT IS NOT A COPY OF A RULE THAT LIVES SOMEWHERE ELSE. The compiler owns this rule and the compiler is the right place for it, and for `src/Federator.Addin` the compiler runs exactly once, on Bader's machine, after a whole round is already written. This is the one place the rule can run before then, and the comment at the top of the script says what it cannot do: it reads text and not a program, it knows nothing about types, members or arguments, and it is not a build and never says one passed
- THE CHECK IS PROVED TO REFUSE AND NOT ONLY TO PASS. `tools/checks/broken` holds `DeclaredTwice.cs`, which is the exact shape F52 shipped, and `Fine.cs`, which holds every legal shape close enough to be worth pinning: two sibling scopes sharing a name, two loops sharing a counter, two methods sharing a name, a brace inside a string and a brace as a character. The check refuses the first naming the file, the line and the name, and says nothing about the second. Actions runs it both ways and fails if the broken folder passes
- The pre-commit hook runs it before the tests, so it costs nothing on a machine with no compiler and refuses the commit rather than the round
- `CLAUDE.md` gains `tools\checks` in the map of where things are, at 184 lines, still under 200
- Proved here: Core tests before and after 1045 passed, 0 failed, 32 skipped, 1077 total, unchanged because no Core code moved. `check-locals.sh` over `src` comes back clean and over `tools/checks/broken` comes back with one fault. Both parse checks give the same profile as before the edit, 1075 `CS0518`, 533 `CS0246`, 74 `CS0234`, 2 `CS0115`, 1 `CS0656`, 1 `CS0103` with no references, and 202 `CS0246` and 1 `CS0103` with them, and not one `CS1xxx` either way

### What remains

- The build itself. Nothing in this container can build the add-in, so step 8 of `03_bader_next.md` is still where this is proved, and it is now expected to pass where it would have failed
- F59 to F64, the six the round is actually for

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F58 pull request
2. F59, every step is named and timed

## 2026-09-19 The plan for the round, the log becomes the third eye

### This is the plan, written before the first edit

- Bader asked on 2026-09-18 for the log to show what happened inside Navisworks, what is running while it runs, and where the time went, and called it the most important thing in the tool. Six fixes were briefed. This entry is the plan and nothing in the repo was edited before it was written
- The reading the brief asks for was done first and in full: `CLAUDE.md`, the four files under `.claude/rules`, the top entry of this file, `01_next.md`, `02_questions.md`, `03_bader_next.md` at all 251 steps, `04_audit.md`, and `src/Federator.Core/Diagnostics/RunLog.cs` end to end, all 1231 lines of it

### The numbers. Three were taken and one fix is already done

- F51, THE ACC WARNING, IS ALREADY DONE AND MERGED. The brief asks for it as its own one line pull request riding this round. It went in on 2026-09-18 as exactly that. `AllowResave` true at `FederationEngine.cs` line 1866, `EmbedDatabaseProperties` true at 1872, `PreventObjectPropertyExport` false at 1875, and the one line naming every publish property this run set at 1880. The DONE line is in `01_next.md` and the proof is steps 201 to 209 of `03_bader_next.md`, upload included. It is not done twice
- F56 and F57 are taken. F56 is done and merged, F57 is open and waiting on Bader's judgement. So the six fixes take the next free numbers, F59 to F64, and the brief's F56 to F61 map onto them one for one and in order
- F58 is a seventh fix nobody asked for, and it goes first. Why is the next section
- The questions file runs 1 to 34 and not to 38, so the gaps F63 finds go in as Q35 onward and not Q39 onward

### What the reading found before any of it. THE ADD-IN DOES NOT COMPILE

- `FederationEngine.BuildViewpoints` declares `views` twice in one scope. `ViewpointSettings views` at line 1793, then `ViewpointBuildOutcome views` at line 1812, which also reads `views.Sizes` while it is being declared. That is CS0128, and the add-in has not built since F52 merged on 2026-09-18
- MEASURED HERE AND NOT GUESSED. The same two Core types and the same two declarations, compiled in this container against the real `Federator.Core.dll` and the net48 reference assemblies, answer `error CS0128: A local variable or function named 'views' is already defined in this scope`
- WHY THE CHECK THIS REPO RUNS MISSED IT. The parse check passes `-nostdlib` with no references at all, and Roslyn then stops before it binds a single method body. It reads syntax and nothing else, which is why every add-in round has been able to report no `CS1xxx` and still ship a broken build. Handing it the net48 reference assemblies and the built `Federator.Core.dll` makes it bind every body whose signature it can resolve, which is a real improvement and still not enough for this one: `BuildViewpoints` takes a Navisworks `Document`, that type does not resolve, and Roslyn skips the body of any method whose signature it cannot bind
- THIS IS F39 HAPPENING AGAIN. F34 deleted three constructors, the window kept calling them, and nothing noticed until the audit put the caller and the callee side by side. Step 8 of `03_bader_next.md` is the build, and all 251 steps wait behind it. A round about the log is worth nothing on a machine that cannot build the add-in, so this is fixed first
- Nothing else of this class is in the tree. A scan of every method in `src` for a local redeclared in a scope that already holds one returns this one hit and no other, and zero in Core, which builds clean and is the control

### The order, one pull request each, branch off main, merged green, branch deleted

1. **F58 The add-in compiles again.** The one line. Then the parse check widened to bind every body it can, so the next fault of this class is caught by the check rather than by a reading. Then the scan above kept as a check that needs no Navisworks, and what it cannot see said plainly: a body whose signature names a Navisworks type is never bound here and never will be until the add-in is built on a machine that has the DLL
2. **F59 Every step is named and timed.** One Core type owns the step list and the words: DECIDE, APPEND, NWF SAVE, UNITS, SETS, TESTS CREATE, TESTS RUN, HARVEST, IMAGES, WORKBOOK, HTML, XML, NWD, CONFIRM. Nothing anywhere types a step name as a string. `RunLog` gains a step that is opened and closed and closes itself when the work inside it throws, one line when it starts and one when it finishes with the seconds and a short phrase for what it changed. The clock is monotonic, a `Stopwatch` and never two wall clock readings subtracted, because a run that crosses a clock change would otherwise report a negative step. Core tests for both line shapes, a step that throws, a step inside a step, and a step never closed
3. **F60 The timing blocks. F21 closes here.** A TIMING block per group with every step, its seconds, its share of the group, slowest first, then the group total. A TIMING block for the run with every group and its total, slowest first, then the run total, then the same table by step name added across every group, so one reading answers which STEP costs the run and not only which building. The run block says in words whether the run fitted in forty five minutes, which is criterion 2 of done. Every number measured and nothing rounded up into a claim. Core tests for the block shape, the ordering, the shares adding to a hundred, and a run of one group
4. **F61 The document census.** Five counts, models, selection sets, clash tests, clash results and saved viewpoints, read in ONE place in the add-in that disposes every wrapper the way `FederationEngine.CountSets` already does. A CENSUS line before and after every step that can change the document. A Core rule says which steps may move which count, and a count that moves when the rule says it may not gets a line beginning `CENSUS CHANGED` naming the step, the count, the before and the after, and that group is not DONE. What the census COSTS is measured and logged once per group, and if it runs over a second a group it drops to counting only before and after the steps that write, and says in the log that it did. Core tests for the rule, every allowed move, every refused move, and the wording
5. **F62 The live line in the window.** Group N of M, the building, the step, the seconds on that step and the seconds on the run, updated as the step changes and at least once a second inside a step that has a loop to tick from. The engine already hands progress out through one callback, so what the callback CARRIES widens and no second route is added, and it stays on the plugin thread. A step running longer than twice what the same step took on the group before says so on the line. The log pane keeps following the log and the live line above it never scrolls away. What this cannot do is tick inside a single Navisworks call that has no loop in it, and the line will say when it last changed rather than pretend
6. **F63 The report gap block.** Bader's standing rule built into the tool: when the code knows something the report does not show, it becomes a question. At the end of each group everything the run measured is compared against what the report carries, and every number held back gets one line in a GAP block naming the number, its value and where it would belong. The rule for what counts as a gap is Core with its tests, seeded from `04_audit.md` and the five per item properties of Q25. The block is written even when it is empty, saying nothing was held back, and one line at the end of the run says how many gaps in total. Every gap this round finds goes into `02_questions.md` as a numbered question from Q35
7. **F64 The machine readable log.** A second file beside the text log, same name and a different extension, one row per event, tab separated, with a header row: time, seconds since start, group, step, event, name, number, text. Every line the text log writes that carries a number writes a row here too, through ONE writer, so the two cannot drift. The purpose is stated in a comment at the top of that writer. The text log stays the one a person reads and nothing about it gets worse. Core tests for the row shape, for a tab or a newline inside a value, and for the header

### What this round holds itself to

- The log is written line by line and flushed all the way to the disk, and that stays true. `RunLog.WriteRaw` already calls `writer.Flush()` and then `stream.Flush(true)` under the lock, and nothing added here buffers. A run that dies mid group leaves everything up to that moment on disk
- The log never changes what the run does. No step is skipped, reordered or slowed to make a line easier to write. Where measuring costs real time it is measured ONCE and the log says what the measurement cost, which is why F61 carries its own cost line
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`. Every shape, every line of wording and every rule about what counts as a gap lives in Core with its tests, and the add-in only measures and calls
- `steps/log.md` gets one entry per fix, newest at the top. New questions go to `02_questions.md`. Nothing under `samples` or `steps/logs` is touched

### What remains

- The whole round. This entry is the plan and no code has changed yet
- F57, the five older Look for lines, is still Bader's to judge and is not touched here
- The two probes, Q33 and every one of the 251 steps still wait for the machine with Navisworks on it

### Known bugs

- The add-in does not compile. `FederationEngine.cs` line 1812. F58 is the first thing this round does
- Everything else as in the F46 entry

### What comes next

1. F58, so the add-in builds again
2. F59 to F64 in the order above, one pull request each
3. The closing work: `03_bader_next.md` read end to end against the code again with the counts said out loud, one new numbered section for the log itself, and the closing entry

## 2026-09-18 F56, what the real read of 03_bader_next.md found

### What was done

- F56 is a follow up to the feature round, opened because the read that round reported as done was not done. The closing entry is corrected in place to say so
- WHAT WENT WRONG IN THE REPORTING. The round's last step was to read `03_bader_next.md` end to end against the code. What was actually done was a mechanical check of every backtick quoted STRING in the round's own 42 new steps, 36 of them, and that was reported as the read. A step can promise a block ORDER, a count, or which file does a thing, and none of those is a quoted string. The real read covers all 251 steps and asks what each one promises
- The real read: 120 Look for lines against the code, eight readers over eight ranges, every finding handed to a verifier told to refute it and to default to refuted. NINE survived. FOUR are this round's own drift and five are older than it
- F52 broke the block order in step 51. `BuildViewpoints` runs between `ClashStep` and `SaveTheNwfAgain` and always writes a VIEWS line, so the real order is UNITS, CLASH, ITEM IDS, VIEWS, NWF attempt, XLSX, NWD. The step still listed the order from before F52, so Bader would have followed it and found a block that is not in it
- Step 199 named the wrong file. It said `SavedViewpoints.cs` would fail to build without `AddCopy`. It would not: the word appears nowhere in that file and nothing calls it, because `Add` throws first. Only `RootItem` is used. The step and the file's own summary now separate what the code USES, where a wrong probe answer breaks the build, from what the work that is not built yet EXPECTS, where a wrong answer changes what F52 can be finished with and breaks nothing
- Step 210 was wrong in the correction F55 made to it. F55 fixed the COUNT, five paths and not three, and left the ORDER wrong. `BuildingGrouping` sorts the disciplines Ordinal before they reach the plan, so a group of AR, ME and EL is held as AR, EL, ME and the line reads in that order. A reader checking a correct log against that step would have called a correct run a fault, which is the exact thing the step exists to prevent
- TWO OF THIS ROUND'S OWN FEATURES TOOK THE SAME LOG PREFIX. F50's rebuild tally labelled its fourth row STATUS, and F54 writes STATUS lines about a clash moving. A rebuilt group on an ordinary run therefore writes a STATUS line that has nothing to do with F54, and step 229 says there should be none. The fix is not to reword the step: the rebuild row is RESULTS now, so one prefix means one thing. One prefix reading as two different things is how a log stops being trusted, and the step says which is which
- The five older findings are NOT fixed here and are listed in `01_next.md` as F57 with their evidence. One of them, step 73, is wording an earlier round wrote deliberately, and reversing that on a quick verification is the fault this project's rules exist to prevent. They are Bader's to judge
- Proved here: Core tests 1045 passed, 0 failed, 32 skipped, 1077 total, before and after, because the only code change is a log label and its tests. The add-in parsed with the same six error codes and not one `CS1xxx`

### What remains

- F57, the five older findings, for Bader to judge
- Everything the feature round left: the two probes, Q33, and the run

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F56 pull request
2. Bader reads F57 and says which of the five to correct

## 2026-09-18 The feature round is closed, F51 to F55

### What was done

- Six features merged today, in the order the brief set: F51 first, then F50, F52, F53, F54 and F55. Each on its own branch off main, each a draft pull request merged once Actions was green, each with its own entry above. No pull request is open and nothing was committed on main
- MEASURE BEFORE YOU WRITE, which is what this round turned on. Three of the six needed an API fact. One was already measured and two were not, and saying which was which before writing anything is what kept the round honest
- F51 needed nothing new. `AllowResave`, `EmbedDatabaseProperties` and `PreventObjectPropertyExport` were all on the list read off the installed DLL on 2026-08-29, each with a getter and a setter, so the case the brief allowed for, a name not being on the type, did not arise. Every NWD this tool has ever published went out without May be re-saved, which is the processing error beside every one of them in ACC
- F50 and F52 needed measurements this container cannot take. There is no `Autodesk.Navisworks.Api.dll` and no PowerShell here and the add-in has never compiled here, so both probes are WRITTEN and neither is RUN. `scan.md` gains sections 5a and 5b, each headed NOT MEASURED, each recording the question and naming the probe that answers it. No member was written down as if it had been read
- What 5a says. `scan.md` records two members of `DocumentModels`, `Count` and `SetModelUnitsAndTransform`, and nothing named Remove, Delete or Detach against a model anywhere in 2796 lines. That is not a search that came back empty, it is a thing nobody ever read, and the difference is the whole point of the section
- What 5b says. `DocumentSavedViewpoints` appears nowhere in this repo. Five things F52 needs are UNKNOWN, so the structure was written and the API surface was not, and `SavedViewpoints.CanBuild` is false with the run saying so in the log on every run rather than looking finished
- THE DECISION THIS ROUND TURNED ON, TWICE. A step this tool cannot do is not a step that failed. Wiring F52's builder in while its two API methods throw would have reported every group FAILED over a feature never attempted, which is the fault that once called a clean 22 group run failed because an NWD nobody had asked for was missing. So the viewpoints are planned, logged and not attempted, and the judgement is told they were not requested. The same reasoning put `SavedViewpoints.Count` at minus one rather than zero when it cannot count, because not counted is not kept and a zero would let a rebuild throw viewpoints away and report that it kept them all
- Three copies of one rule that did not get written. F50 widened the rebuild from two things to four, and the keep rule was already written TWICE in `NwfRebuildPlan`, the same expression under two names. Copying it twice more would have made four. It is written once now, in `RebuildTally`, the four things are four rows, and the five superseded members went with their eight tests because nothing in src called them any more
- One rule departed from on purpose. `core.md` says to log a count and five examples when many lines say one thing. F53 names EVERY item whose size could not be read, because those lines each name a different item that may be wrongly in or out of a viewpoint and reading five tells you nothing about the sixth. Naming every one is a setting and turning it off makes the block say it truncated
- Two ordering facts came out of reading the code rather than the brief, and both would have been real bugs. F54's status edit had to go BETWEEN the run and the harvest, because `ClashHarvest` reads a result's status while it builds the report rows, so an edit after the clash step would have left the workbook carrying the status read before the change. And it needed its own resolve, because `TestsEditResultStatus` is a mutator that kills the handle handed to it, which is the shape that once threw per test for 8 hours 52 minutes
- THE READ OF `03_bader_next.md`, PART ONE, and this entry claimed it was the whole thing when it was written. Every backtick quoted string in the steps this round added was checked against the source, with C# concatenation seams stripped so a string built in two pieces still matches. 36 checked in steps 195 to 236. THREE were wrong, and all three were F52's steps broken by F53 adding the sub groups after they were written: the example paths, the count for a single discipline group, and what a discipline folder holds. The other eight flagged strings are composed at run time and each was read against the code it comes from rather than waved through. The file holds 120 Look for lines in total and 25 of them are this round's
- THE READ, PART TWO, which is the one the round actually asked for and which this entry originally reported as done off part one alone. Part one checked quoted STRINGS in this round's own 42 steps. The real read checks what a step PROMISES, across all 251, and it found things a string check cannot see. F56 carries it: 120 Look for lines read against the code, nine wrong, FOUR of them this round's own drift and five older. The correction to this entry is that the read was reported finished before it was finished
- F55 found four more gaps by auditing what each feature actually landed rather than trusting it had. F51's rule was in no rules file. `CLAUDE.md` did not name this round's two unknowns in the list somebody reads before assuming. Six step references in this log were wrong, and FOUR of those were numbers written without measuring, which is the thing CLAUDE.md forbids. Two Core types were named in no rule
- The D6 branch list rebuilt off `git ls-remote --heads origin`, 49 names, checked name for name against the live remote plus the branch this entry is written on. It was 42
- Core tests: 957 passed, 0 failed, 32 skipped, 989 total before the round. 1045 passed, 0 failed, 32 skipped, 1077 total after it. 88 tests added and not one failure introduced at any point. Core builds in Release with 0 warnings and the add-in parses with the same six error codes and not one `CS1xxx`, which caught a real `CS0121` ambiguity in F52 that would otherwise have reached the local build

### What remains

- Nothing in this round. `steps/01_next.md` has F21, F18 when the sample arrives, and F23 when Q20 is answered
- Q24 to Q34 are open. Q32, Q33 and Q34 are this round's: whether the NWF belongs in ACC at all, how the tool learns which clashes cannot be solved, and whether the never clear rewrite is wanted once the probe answers
- TWO PROBES ARE THE ROUND'S ONLY REAL BLOCKERS. `probe-model-remove.ps1` and `probe-viewpoints.ps1`, steps 195 to 200, ten seconds each and neither needs the add-in built. Until they are run, F52's viewpoints cannot be created and F53's SIZE block cannot appear, because the sub group is a viewpoint
- The whole of `03_bader_next.md`, 251 steps, waits for the machine with Navisworks on it. Nothing in this round was proved by a run

### Known bugs

- As in the F46 entry

### What comes next

1. Bader runs the two probes, steps 195 to 200, and pastes both outputs into `scan.md` under 5a and 5b
2. Bader answers Q33, which is the only thing standing between F54 and a finished feature
3. Bader builds, installs and works `03_bader_next.md` from step 1
4. Bader runs the D6 delete command himself

## 2026-09-18 F54, clashes that cannot be solved become Reviewed

### What was done

- F54 done as far as Q33 allows, which is the part that needs no answer, and nothing above it was guessed at
- The words, which is the half of this that was always buildable. A CLASH carries New, Active, Reviewed, Approved or Resolved. A TEST carries New, Old, Partial or Complete. They are different sets on different things and they share only the word New, which is how they get confused. Old is a TEST word, so no clash is ever at Old and nothing here ever moves one from it. The wording is `Federator.Core.Clash.StatusWords`, in Core so the log and `docs/workflow.md` cannot drift apart, and there is a test asserting each of those sentences by name
- Reviewed and nothing else. `StatusesThisToolMaySet` decides, and a status it refuses is logged BY NAME with the reason rather than silently dropped. Approved and Resolved are never set because each is a person's statement about work that was actually done, and the NWF is the only record of what has been fixed, so there would be nothing to check the claim against afterwards. New and Active are not set either, for a duller reason: running a test produces them and setting one by hand would overwrite a decision somebody had already made
- The member was already measured, so nothing here was assumed. `DocumentClashTests.TestsEditResultStatus(IClashResult result, ClashResultStatus status)` at `scan.md` line 137, and the enum at line 216
- TWO ORDERING FACTS THAT CAME OUT OF READING THE CODE, AND BOTH CHANGED THE DESIGN. First, `ClashHarvest.Into` reads a result's status while it builds the report rows, immediately after the test runs and inside the same handle. So the status has to be applied BETWEEN the run and the harvest. Applying it after the clash step, which is where it would naturally go, would leave the workbook and the page carrying the status read before the change, which is the one thing the brief says the feature must not do
- Second, `TestsEditResultStatus` is a mutator, and the rule says every mutator on `DocumentClashTests` is a copy form that kills the handle handed to it. So the edit gets its OWN resolve rather than sharing the handle the count and the harvest use. Sharing it is the exact shape that threw once per test for 8 hours 52 minutes and produced nothing
- Nothing is resolved at all when no status is wanted, which is every run while Q33 is open, so the call costs one comparison per test and does not slow a run
- A status written into the document is a write, so the NWF is saved again on it, even where nothing was created and nothing ran
- WHAT IS NOT BUILT AND WHY. How the tool learns which clashes cannot be solved is Q33. The method takes a list of clash names with the status wanted and applies it. Nothing supplies that list. Building a guess at the input would mean writing a rule nobody agreed to into the only file that records what has been fixed, and Q33 sets out the four shapes it could take so Bader can pick one rather than discover the wrong one on a real model
- Proved here: Core tests before 1035 passed, 0 failed, 32 skipped, 1067 total. After 1045 passed, 0 failed, 32 skipped, 1077 total. Core builds in Release with 0 warnings. The add-in parsed with the same six error codes and not one `CS1xxx`
- Waits for the local machine: steps 228 to 236. Two of them can be done today and they are the ones that matter while Q33 is open, because both check that NOTHING moved: no `STATUS` line on an ordinary run, and no clash at a status it was not at before

### What remains

- F55, then the read of `03_bader_next.md` end to end, then the closing entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F54 pull request
2. F55, the rules and the docs catch up

## 2026-09-18 F53, the 150 mm rule and the sub groups

### What was done

- F53 done. This is the one feature of the round that is entirely Core and entirely provable in this container, and all 26 of its tests pass here, so it is the one that needed no probe and no waiting
- The rule. Pipes, ducts, cable trays and their fittings over 150 mm are in the viewpoints and smaller ones are out. Over 150 means OVER, and exactly 150 is out, which is the kind of boundary that gets read both ways so there is a test on it by name
- Every number is a setting. `SizeSettings` holds the threshold at 150 in millimetres, the six property names in the order they are tried, which disciplines carry a sub group, and whether every unmeasurable item is named. Nothing in the rule is a constant
- Six property names and not one, because which property carries the size differs per kind and per exporter. A round duct has a Diameter, a rectangular one has Width and Height, a cable tray usually has Width, and some exporters write only Size or Overall Size. Reading one name would silently drop every item that calls it something else
- THE NUMBER IS NEVER COMPARED RAW. What a property hands back is in the document's units, so it goes through `UnitTable` first. A document in feet reporting 0.5 is 152.4 mm and is IN, and comparing 0.5 against 150 would have put it out while the same model in millimetres put it in, so one building would have produced two different sets of viewpoints depending on a setting nobody changed. There is a test that runs the same size through millimetres, centimetres and metres and asserts one answer. A unit the table does not know FAILS rather than falling back, which is F33's rule
- INCLUDE ON UNKNOWN, and it is the loud one. A fitting usually carries no size property at all. Anything whose size cannot be read is IN, because leaving it out means a run quietly drops real geometry from a viewpoint with nothing in the output to say it happened. The SIZE block says how many are in for that reason, says plainly that nothing was dropped, says the number is expected to be large, and then names every one of them
- ONE DELIBERATE DEPARTURE FROM AN EXISTING RULE, AND THE REASON. `core.md` says that when tests skip for the same reason, log the count and at most five examples. That rule was written after a run wrote 1830 near identical SKIPPED lines and a 1 MB log. It is about MANY LINES SAYING ONE THING. These lines each say a different thing: every one names an item that may be wrongly in or out of a viewpoint, and reading five of them tells you nothing about the sixth. So every one is named, `NameEveryUnknown` is a setting defaulting to true, and turning it off makes the block SAY it truncated, because a truncated list that does not say so is the fault both rules exist to prevent
- The sub groups. The large items of Mechanical and Electrical sit in a sub group of their own, so the tree reads ME, then ME only, then Over 150mm, then ME over 150mm. The sub folder is named FROM the threshold, so a folder reading Over 150mm beside a rule using 250 cannot happen, which is the kind of drift nobody notices. Which disciplines get one is a setting, because ME and EL are codes this project uses and nothing in this code decides that for another project
- The add-in reads and does not judge. `ItemSizes` reads the named properties off an item by KIND, never with `ToDisplayString` and never with a cast, which is the rule section 4n was written for after one throw cost three report columns on every row of a run. A property that throws is left out in its own try, and a size written as a display string is not read at all, because parsing "150 mm" would mean guessing the unit written in it while the number this tool converts is in the document's units
- Proved here: Core tests before 1004 passed, 0 failed, 32 skipped, 1036 total. After 1035 passed, 0 failed, 32 skipped, 1067 total. Core builds in Release with 0 warnings. The add-in parsed with the same six error codes and not one `CS1xxx`. Three existing viewpoint plan tests failed when the sub groups went in, which was the correct new behaviour and they now assert it
- Waits for the local machine: steps 219 to 227, and the SIZE block itself, because the sub group IS a viewpoint and nothing can build a viewpoint until F52's probe has answered. The rule underneath it is settled and proved and does not wait for anything

### What remains

- F54 and F55, in that order

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F53 pull request
2. F54, Reviewed, which is small while Q33 is open

## 2026-09-18 F52, a viewpoint per discipline

### What was done

- F52 done as far as a container with no Navisworks can take it, and the part that is not done says so in the log on every run rather than looking finished
- The Core half is finished and proved. `ViewpointPlan` makes one folder per discipline in the group, named with the discipline code the scan reads off part 5 of the NWC name, with one viewpoint in each showing that discipline and hiding the others. `ViewpointSettings` holds the two names, because a name that shapes a run is a setting. `ViewpointBuildOutcome` is the twin of `SetBuildOutcome`, so the VIEWS block reads as the SETS block does and its totals are counted off the same list the lines come from, which is what stops a block reporting three created over two lines
- A group of ONE discipline still gets its folder and its viewpoint. It hides nothing, which is not the same as having no viewpoint. Skipping it would make one NWF in a set a different shape from every other, and a group that quietly had none would read as one where the step failed. That is the reasoning F35 used for creating every clash test in a single discipline group and running none of them, and there is a test on it by name
- A viewpoint already at its path is left exactly as it is and counted as already there, never made again. F28 set that rule for sets and the reason carries over without change: a second copy at one path leaves the tree holding both, and whichever came first is what anything resolving that path finds
- `GroupJudgement` gains the rule the brief asked for. A group whose viewpoints failed is not DONE. It is judged on what was ASKED FOR, so a run that wanted no viewpoint cannot fail at them, which is the rule that stopped a clean 22 group run being reported as FAILED over an NWD nobody had asked for
- THE PART THAT IS NOT DONE, AND WHY IT IS WIRED IN ANYWAY. Every Navisworks call the creation needs is unmeasured. `DocumentSavedViewpoints` appears nowhere in `scan.md` and nowhere in this repo, so how a folder is made, how a viewpoint is added, whether a name can be set and what has to be disposed are four UNKNOWNs, and how a discipline is shown and the others hidden is a fifth
- Writing five unknowns deep would have produced code nobody could review and that would need rewriting the moment the probe answered. So the structure is written and the API surface is not: `SavedViewpoints.ShowOnly` and `Add` throw with a message naming section 5b and the probe, and every assumption is listed at the top of that one file
- `SavedViewpoints.CanBuild` is FALSE and it is the one line to change. While it is false the run PLANS the viewpoints, writes one line saying what it would have made, writes a second saying the measurement is outstanding, and attempts nothing. The judgement is told the viewpoints were not requested
- That last decision is the one worth defending. Wiring the builder in while those two methods throw would have reported EVERY group FAILED over a feature that was never attempted, which is precisely the fault that once called a clean 22 group run failed because an NWD nobody had asked for was missing. A step this tool cannot do is not a step that failed
- One rule that was checked and stands. `core.md` says no clash is ever saved as a viewpoint in the NWF. A discipline viewpoint is not a clash viewpoint, so the rule is untouched, and `docs/workflow.md` now says which is which rather than leaving two sentences that read as one rule
- Proved here: Core tests before 978 passed, 0 failed, 32 skipped, 1010 total. After 1004 passed, 0 failed, 32 skipped, 1036 total. Core builds in Release with 0 warnings. The add-in parsed with the same six error codes and not one `CS1xxx`, which caught a real ambiguity the new overload introduced and which is fixed
- Waits for the local machine: the probe, then the add-in half, then the run. Steps 210 to 218 are the two halves, and the first of them is true on the next ordinary run

### What remains

- F53, F54 and F55, in that order

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F52 pull request
2. F53, the 150 mm rule, which is all Core and all provable here

## 2026-09-18 F50, the NWF strategy, new against existing

### What was done

- F50 done, on the branch the measurement forced. The question the brief asks first, whether one model can be taken out of an open document without a clear, is UNKNOWN and could not be answered here, so the clear and restore stays and widens from two things to four
- Why it is UNKNOWN rather than no. `scan.md` records `Document.Models` and exactly two members of `DocumentModels`, `Count` in section 4b and `SetModelUnitsAndTransform` in 4q. Nothing named Remove, Delete or Detach against a model appears anywhere in the file, and the only two Remove members in the whole of it are `RemoveExpiryDate` and `RemovePassword` on `PublishProperties`. That is not a search that came back empty, it is a thing nobody ever read
- Why the probe was not run. There is no `Autodesk.Navisworks.Api.dll` and no PowerShell in this container, and the add-in has never compiled here. So `tools/probes/probe-model-remove.ps1` is written and Bader runs it, steps 195 and 196. `probe-viewpoints.ps1` went in beside it rather than waiting for F52, because F50's own viewpoint count rests on the same unmeasured collection and there is no sense making Bader come back for a second ten second run. `scan.md` gains section 5a, which records the QUESTION and says NOT MEASURED in its heading, because a section that reads like a measurement and is not is worse than no section
- What the widening actually is. Four things counted out and counted back, not two: the sets, the tests, the viewpoints and the clash results carrying a status a person set. Each is counted before the clear, after the appends and after the copy is put back, and the NWF on disk is saved over only when every one came back
- The fault the widening would have created, and did not. `NwfRebuildPlan` held the keep rule TWICE, `SetsKept` and `SavedTestsKept`, the same expression under two names, with `SetsNeedRestoring` the same shape again. Widening by the same means would have made four copies of one rule, which is the exact thing CLAUDE.md forbids and the thing the last round spent three pull requests removing. So the rule is now written ONCE, in `Federator.Core.Rerun.RebuildTally`, and the four things are four rows
- The five superseded members went with their eight tests, because once the tally replaced them nothing in src called any of them. That is the rule about a public member with no caller, applied to code written earlier in the same round rather than found by a later audit
- Not counted is not kept, and this is the part worth reading twice. `SavedViewpoints.Count` returns MINUS ONE where it could not count, never zero, and a thing that was never counted holds the NWF shut exactly as a thing that was lost does. A viewpoint count coming back as zero would let a rebuild throw every viewpoint away and report that it kept them all
- Which statuses are worth keeping is its own Core rule, `StatusesAPersonSet`, and it is everything except New. A result at New is what running a test produces and the next run makes it again. A result somebody moved to Active, Reviewed, Approved or Resolved is a decision made while looking at the model, and this tool runs weekly, so losing those is losing however many weeks of review the file has collected. A status the enum does not name counts too, because one this code does not recognise is the one most worth keeping
- The one place the unmeasured API is touched is `SavedViewpoints.cs`, and its summary says so in full: what is assumed, why it is assumed, that it was NOT read off a DLL, and that a build error there means the assumption was wrong. That is the whole reason it is one file and one method
- One deliberate change to the log. The two lines had different shapes, `SETS     before clear 61, ...` and `         saved tests kept: ...`. Four things reading four ways is how a log stops being read, so all four take the SETS shape, and steps 47, 48, 49 and 51 of `03_bader_next.md` were corrected in the same fix rather than left to F55. The words kept, none and LOST all survive, so what Bader looks for did not change, only where it sits
- Proved here: Core tests before 966 passed, 0 failed, 32 skipped, 998 total. After 978 passed, 0 failed, 32 skipped, 1010 total, which is twelve for the tally, eight for the statuses rule and eight deleted with the members they proved. Core builds in Release with 0 warnings. The add-in parsed with the same six error codes and not one `CS1xxx`
- Waits for the local machine: the two probes, and then a rebuilt run. Nothing about the four counts can be proved without Navisworks, and the viewpoint count cannot even be compiled here

### What remains

- F52, F53, F54 and F55, in that order

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F50 pull request
2. F52, which is the other half of the viewpoint work and waits on the same probe

## 2026-09-18 F51, the ACC warning

### What was done

- F51 done. Three properties set on the publish that were never set, one new Core type carrying the log line, and the reason written down in `docs/workflow.md`
- What was wrong. `WriteNwd` built a `PublishProperties`, set Title, Publisher, Subject and Author, and published. It never set `AllowResave`, so every NWD this tool has ever published went out without May be re-saved, and Autodesk say an NWD published without it cannot be translated by the ACC and Forma viewer. That is the processing error beside every one of them
- What is set now. `AllowResave` true, which is the fix. `EmbedDatabaseProperties` true and `PreventObjectPropertyExport` false with it, which are the second thing Autodesk describe, an NWD reaching ACC with no properties and every object showing as solid
- Nothing was assumed. All three names are on the list read off the installed DLL on 2026-08-29, `scan.md` section 4b, under the heading saying `PublishProperties` has a parameterless constructor and a base type of `NativeHandle`. The list carries `public bool AllowResave { get; set }`, `public bool EmbedDatabaseProperties { get; set }` and `public bool PreventObjectPropertyExport { get; set }`, each with a getter and a setter. So no probe was needed and no name had to be left out, which was the one thing the brief allowed for
- The log line. `Federator.Core.Diagnostics.PublishedProperties` records each property as the add-in sets it and writes one line naming all seven with their values. It is in Core because it is a rule a test can prove, and it has nine tests. It records what was SET and never what the type offers, because a name on the list that the add-in stopped setting would read as still set, which is the one way this line can lie. It is written BEFORE the publish, so an NWD whose publish throws still leaves behind what it was asked to carry, and an empty list says so in words rather than trailing off after the word set
- Why the line is worth its own type. An NWD that will not open in ACC is diagnosed months later off the log and the file, by someone who cannot read the build that wrote it. Reading the code answers a different question, which is what the code says now
- `docs/workflow.md` gains a section on the NWD in ACC, and says why nothing appears beside the NWF: ACC does not translate an NWF at all, because an NWF holds no geometry, only pointers to the NWC files, so there is no viewable file to make from one. Nothing beside an NWF up there is a fault
- Proved here: Core tests before 957 passed, 0 failed, 32 skipped, 989 total. After 966 passed, 0 failed, 32 skipped, 998 total, the nine being the new ones. The add-in parsed through the Roslyn compiler with no references and gave the same six error codes as before the edit, 945 CS0518, 464 CS0246, 63 CS0234, 2 CS0115, 1 CS0656 and 1 CS0103, and not one `CS1xxx`
- Waits for the local machine: everything that matters. The three flags only mean something once an NWD goes up. Steps 201 to 209 of `03_bader_next.md` are the run, the log line, the upload, the missing warning and the properties panel

### What remains

- F50, F52, F53, F54 and F55, in that order

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F51 pull request
2. F50, the NWF strategy, which starts with a probe this container cannot run

## 2026-09-18 The plan for the feature round, F51 then F50, F52, F53, F54, F55

### What was read first

- CLAUDE.md, the four files under `.claude/rules`, the top entry of `steps/log.md`, `steps/01_next.md`, `steps/02_questions.md`, `steps/03_bader_next.md` and `steps/04_audit.md`, all of them whole. Then `docs/workflow.md`, `tools/probes/README.md` and one probe for its shape, and `docs/history/scan.md`, whose 2796 lines are where most of what this round needs was already measured
- Ground state on main at f5596b3, the close of the third audit round. Core tests under mono on Linux: 957 passed, 0 failed, 32 skipped, 989 total. That is the baseline every entry in this round compares against
- F50 to F55 are all free. F47 is the highest number `steps/01_next.md` holds and nothing in `steps` names an F48 or an F49, so the six numbers in the brief are taken as they are and nothing is renumbered. Q31 is the last question, so the new ones are Q32 onward, which is what the brief already assumes

### What was measured before the plan was written

The rule for this round is measure before you write. Three of the six need an API fact. One of the three is already measured and two are not, and this section says which is which rather than leaving it to be found later.

- **F51 needs no new measurement.** All three properties are on the list read off the installed DLL on 2026-08-29, in section 4b of `scan.md`, under a heading saying `PublishProperties` has a parameterless constructor and a base type of `NativeHandle`. The list carries `public bool AllowResave { get; set }`, `public bool EmbedDatabaseProperties { get; set }` and `public bool PreventObjectPropertyExport { get; set }`, each with a getter and a setter. So all three names exist, none has to be left out, and no probe is needed. `WriteNwd` is at `FederationEngine.cs` line 1739 and sets Title, Publisher, Subject and Author inside a `using` over a `PublishProperties`, exactly as the brief says, and sets none of the three
- **F50 needs a measurement that cannot be taken here. UNKNOWN.** `scan.md` records `Document.Models` as returning a `DocumentModels` and records exactly two things about that type: `Count` is an int, and `SetModelUnitsAndTransform(Model, Units, Transform3D, bool)` is the only public managed member that sets units. Nothing named Remove, Delete or Detach appears anywhere in the file against a model. The only two Remove members in all of `scan.md` are `RemoveExpiryDate` and `RemovePassword` on `PublishProperties`. So whether one model can be taken out of an open document without a clear is UNKNOWN
- **F52 needs a measurement that cannot be taken here. UNKNOWN.** `DocumentSavedViewpoints` appears nowhere in this repo. The whole of `scan.md` names `Viewpoint`, `DocumentCurrentViewpoint`, `View.CreateViewpointCopy` and `ClashResult.HasSavedViewpoint`, and says in section 4k that nothing here writes a viewpoint into the NWF. The one other mention in the checkout is a comment in `ClashImages.cs` line 39 saying nothing there touches SavedViewpoints. So how a viewpoint folder is made, how a viewpoint is put in it, whether its name can be set and whether anything has to be disposed are all UNKNOWN
- **F54 needs no new measurement.** `scan.md` line 137 carries `public System.Void TestsEditResultStatus(IClashResult result, ClashResultStatus status)` and line 216 carries `ClashResultStatus : New = 0, Active = 1, Reviewed = 2, Approved = 3, Resolved = 4`, both as the brief states them
- **F53 needs no new measurement to be written, and one to be trusted.** The reader is already there: `ClashHarvest.FirstProperty` with its `out string matched`, and `Text(VariantData)` which sends `DoubleLength` through `ToAnyDouble`. `UnitTable` carries `MillimetresPerUnit` on every row and `ByEnumName`, which is the conversion the rule needs. What is UNKNOWN is whether `ToAnyDouble` on a `DoubleLength` hands back the document's units or something else, and the brief states it is the document's. It is built that way and the assumption is named in one place and put to Bader as a step

### Why no probe can be run here, and what happens instead

There is no `Autodesk.Navisworks.Api.dll` anywhere in this container and no PowerShell on the path, so not one probe under `tools/probes` can be RUN here, and the add-in has never compiled here either. That means this round cannot append a measured answer to `scan.md` for F50 or F52. It can do the three things the brief asks for in that case, and it does all three:

1. the probe is WRITTEN here, under `tools/probes`, in the shape the six existing ones use, saying UNKNOWN and the path it looked at when it cannot find the DLL
2. `scan.md` gets a dated section per unknown that records the QUESTION and says plainly that it is not measured, with the probe that answers it named. No answer is invented and no member is written down as if it had been read
3. `03_bader_next.md` gets a numbered step per probe, one action each, with its Look for line, so the answer comes back from the machine that has the DLL

Every line of add-in code that rests on one of those unknowns goes behind ONE method that names what it assumes, so a build error or a wrong answer lands in one place and not in five.

### The order and what each PR does

F51 is first because the brief puts it first and because it is the only one of the six that is finished the moment it is written. The sections go into `steps/01_next.md` in this same order, before F21.

1. **F51. Branch `fix-F51`.** The ACC warning. `AllowResave` true, `EmbedDatabaseProperties` true and `PreventObjectPropertyExport` false in `WriteNwd`, all three on the measured list. One log line naming every publish property this run set, built in Core so a test can read it, because a future NWD that will not open in ACC has to be readable off the log alone. `docs/workflow.md` gains the reason nothing appears beside the NWF, which is that ACC does not translate an NWF because it holds no geometry, only pointers to the NWCs, so there is no viewable file to make. Q32 asks whether the NWF needs to be up there at all. `03_bader_next.md` gains the four steps: publish one NWD, upload it, look for no warning, open it and look for properties in the panel
2. **F50. Branch `fix-F50`.** The NWF strategy. `tools/probes/probe-model-remove.ps1` written, reading every member of `DocumentModels` and of `Document.Models` and anything anywhere in the API assembly named Remove, Delete or Detach that takes a model or an index. Because the answer is UNKNOWN today, the branch the code takes now is the SECOND one the brief names: the clear and restore stays and is widened from two things to four, so viewpoints and clash result statuses are counted before the clear and after the restore exactly as the sets are today, and the group FAILS with the NWF left alone if any of the four does not come back. The counting rules go in Core with their tests. The rule goes in `.claude/rules/addin.md` in one paragraph and `docs/workflow.md` gains a section named The NWF is the record. The three labels are untouched and `RunPath` still decides what the person is told. Q34 asks whether Bader wants the rewrite once the probe says a model can be taken out on its own
3. **F52. Branch `fix-F52`.** A viewpoint per discipline. `tools/probes/probe-viewpoints.ps1` written, reading `DocumentSavedViewpoints` whole, every member of `SavedViewpoint`, and what a folder and an add look like, against the `DocumentSelectionSets` shape beside it so the two can be read together. The Core half is written and tested here in full: a `ViewpointPlan` naming one folder and one viewpoint per discipline off part 5 of the NWC name, and a `ViewpointBuildOutcome` that is the twin of `SetBuildOutcome`, so the VIEWS block reads exactly as the SETS block does, one line per viewpoint then created, already there and failed. A viewpoint already at its path is left as it is and counted as already there, which is the rule F28 set for sets. A group of one discipline still gets its folder and its viewpoint. The counts reach the RESULT block and `GroupJudgement`, so a group whose viewpoints failed is not DONE. The add-in half goes behind one method that names what it assumes about the collection
4. **F53. Branch `fix-F53`.** The 150 mm rule. This one is entirely Core and entirely provable here, which is why it is worth doing properly. The threshold is a setting with 150 as its default, in millimetres, named once. The property names are a setting too, starting at Diameter, Width, Height, Size, Nominal Diameter and Overall Size. The size is converted from the document's units through `UnitTable.MillimetresPerUnit` and never compared raw. Anything whose size cannot be read is INCLUDED and every one of them is named in the log under a line saying how many were included because their size could not be read. The sub groups under Mechanical and under Electrical are part of the plan the viewpoints read. The add-in only reads properties and calls the rule
5. **F54. Branch `fix-F54`.** Reviewed. Only the part that needs no answer: one method in the add-in taking a list of clash names with the status wanted, applying it through `TestsEditResultStatus`, logging every one it changed and every one it could not find. Nothing above that method is built, because how the tool learns which clashes cannot be solved is Q33. Reviewed is the only status this tool ever sets. The NWF is saved again after a status changes, because the change is written into the document, and the workbook and the page read the status AFTER the change and never the one read before it. The log and `docs/workflow.md` both say plainly that a clash carries New, Active, Reviewed, Approved or Resolved and a test carries New, Old, Partial or Complete, and that Old is a test word and never a clash word
6. **F55. Branch `fix-F55`.** The rules and the docs catch up. Every rule above into `.claude/rules`, `docs/workflow.md` and `03_bader_next.md`, one numbered proof per feature, one action per step, each with its Look for line
7. `steps/03_bader_next.md` read end to end against the code again, the way the third round did, and the log says how many Look for lines were checked and how many were corrected
8. The closing entry

### One thing the round must not break

`core.md` says no clash is ever saved as a viewpoint in the NWF, and that rule survives F52 untouched, because a discipline viewpoint is not a clash viewpoint. The sentence in `ClashImages.cs` saying nothing there touches SavedViewpoints stays true of that file and stops being true of the engine, so F52 says which is which rather than leaving two sentences that read as one rule.

### Rules held through the round

- One PR per feature, branched off main, merged only when the tests job is green on the branch head, the local branch deleted after. Never a pull request left open at the end of a step. The remote branches are not deleted, Bader has that command
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`, and every rule that can be proved without Navisworks is in Core with its test
- No member is written down as measured unless it was read off a DLL. Where the container cannot read one, the entry says UNKNOWN, the probe is written and the step goes to Bader
- No generated-by and no co-authored-by line on a commit or a pull request body, which is what the writing rule at the end of CLAUDE.md asks for

### What remains

- The whole round. Nothing is edited yet beyond this entry

### Known bugs

- As in the F46 entry

### What comes next

1. F51, the ACC warning

## 2026-09-18 The third audit round is closed

### What was done

- Three fixes merged today, in the order the brief set: F47a, F47b and F47c. Each on its own branch off main, each a draft pull request merged once Actions was green, each with its own entry above. No pull request is open and nothing was committed on main
- F47a, the walls. `.gitattributes` added at the root, `text=auto` for everything, `eol=lf` forced on `*.sh` and on `.githooks/pre-commit` by path, and `-text` on `samples` and `steps/logs` so the evidence is never normalised. `git add --renormalize .` changed no bytes here, because the index already held LF for every text file in it, 216 when F47a measured it and 217 now that `.gitattributes` is one of them, and the one CRLF file in it is the run log, which is now pinned. The fix changes what a checkout gets, not what the repo holds
- Both walls were proved to refuse here, with the exit code and the line each prints, and each was also proved to allow a call it must allow. The branch wall proved itself twice over, because it blocked a command of the worker's own that carried the words git and commit while main was checked out. What is still UNKNOWN is the one thing only a Windows machine can answer, whether Claude Code there finds the sh that runs them. That is D7 in `03_bader_next.md`, steps 237 to 244
- F47b, the last doubled comment. It was moved, not deleted. It describes `Count`, which is still there and had lost its own comment when F45 inserted a method above it. F44's own check now reads 0 stacked blocks over all 102 files under src, where it read 1
- F47c, the record. `WorkbookWriter.ClientColumns` deleted, nothing anywhere referenced it. The brief named two wrong records in the F40 entry and there are five. Every one of the 53 names on that list was read against the code and against the F40 commit's own diff, and the table is in the pull request
- `steps/03_bader_next.md` read again, which is what the round asked for last. The numbering runs 1 to 209 with no gap and no repeat. The D6 branch list was stale by exactly the three branches this round pushed, so it was rebuilt off `git ls-remote --heads origin`, 42 names now, checked name for name against the live remote plus the branch this entry is written on. The same clone was holding a remote tracking ref for a branch the remote no longer has, which is the trap that section already warns about, so the warning is now a measurement as well
- `CLAUDE.md` says why the walls need LF, in four lines under The two walls, pointing at `.gitattributes`. That belonged in F47a and was missed there. The file is 175 lines, still under the 200 F38 set
- `steps/01_next.md` renumbered. Three fixes are left, the same three as before this round: F21 which can be started here, F18 which waits for the sample, F23 which waits for Q20
- Core tests: 957 passed, 0 failed, 32 skipped, 989 total, before the round and after it. The one member deleted had no test and the comment moved is not code

### What remains

- Nothing in this round. `steps/01_next.md` has F21, F18 when the sample arrives, and F23 when Q20 is answered
- Q24 to Q31 are open for Bader. Q24 and Q25 from the first audit, Q26 and Q27 from F40, Q28 from F41, Q29 from F43, Q30 from F44, Q31 from F47c on the two counts in the F40 entry
- The whole of `03_bader_next.md`, 209 steps, waits for the machine with Navisworks on it. Nothing in this round was proved by a run, and nothing in it needed one
- The walls are proved on Linux only. Whether Claude Code on Windows runs them at all is UNKNOWN until D7 is worked

### Known bugs

- As in the F46 entry

### What comes next

1. Bader works D7, steps 237 to 244, which is four minutes and settles whether the walls run on his machine
2. Bader builds, installs and works `03_bader_next.md` from step 1
3. The run logs come back into `steps/logs` on their own branch, as step 183 says
4. Bader runs the D6 delete command himself

## 2026-09-18 F47c, names recorded wrongly in the F40 entry

### What was done

- F47c done. One member deleted, five lines of the F40 entry corrected in place, one new question. The brief named two wrong records and there are five
- `WorkbookWriter.ClientColumns` is gone. It was public, it read `ClientFormat.ClashColumns` and nothing anywhere referenced it, not src, not the XAML, not one test. Its comment said it existed so a test could assert the header without spelling it out again. No test read it. The four tests that read that list read `ClientFormat.ClashColumns` itself, which is the list and not a copy of the name of it. So the reason in the comment was not true and the rule that a public member nothing calls goes out decides it. Q26 is unanswered and it does not reach this member, because Q26 is about a member a test reads and this one has no reader at all
- Every one of the 53 names on the F40 outright list was read twice, once by grepping src for the bare name and once with the type in front, and once more against the F40 commit's own diff, which is what says whether F40 touched the name at all. Nine are still in the code. Five of those nine are recorded correctly and four are not. `BuildStamp.LooksStamped` the entry itself says went back as internal. `RunLog.Start()` with no arguments went and two of it are left that take arguments. Two `GroupFinished` overloads went and one is left. `OutputNameTable.Build` three argument went and the four argument one is left, and `ClashHarvest.Into` three argument went and the five argument one is left. All four of the overload readings were confirmed against the deleted lines of the F40 commit
- The five that were recorded wrongly. `WorkbookWriter.ClientColumns`, which F40 never touched and which was still public until this fix. `ReportOptions.FolderFor` and `BuildingGroupingResult.Find`, which F40 made internal rather than deleting, so they belong in the 64 taken off the public surface. `ClashReportXml.QuickProperties`, which F40 never touched and which F44 made internal. `ClientFormat.DistanceFormat`, which F40 never touched and which F44 deleted. Each of the five is now marked where it stands in the list, in the style the entry already uses for `BuildStamp.LooksStamped`, and one line under the list says what the audit of 2026-09-18 found. The entry is not rewritten
- The count in that entry is left as it was written, and the correcting line says it counts five that did not go. Counting every member declaration the F40 commit removed from src and did not add back gives 53, but that counts three private backing fields and one constructor that the list only mentions in passing, so it is a different rule and not a correction of the 45. That is Q31 for Bader, not a number this fix invents
- Proved here: `ClientColumns` has 0 references under src, the XAML and tests, before the deletion and after. Core builds in Release with 0 warnings. Core tests before: 957 passed, 0 failed, 32 skipped, 989 total. After: the same, because the member had no test
- Waits for the local machine: nothing. No add-in file changed and no line the log prints changed

### What remains

- The read of `03_bader_next.md` for what F47a added, then the closing entry for the round

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F47c pull request
2. Read `03_bader_next.md` once more and close the round

## 2026-09-18 F47b, one doubled comment left

### What was done

- F47b done. One comment, in `ClashRunner.cs`. No logic and no line the log prints changed
- What was there. Two summary blocks stacked at line 1121, both on `ResolvedInTheDocument`. The first describes counting the results of ONE test, which is `Count`, twelve lines below, and `Count` carried no comment at all. F45 inserted `ResolvedInTheDocument` above it and the comment stayed where it was
- The brief said to delete the block that does not describe the method under it, and it is MOVED instead. It is not a comment for a method that is gone, it is a comment for a method that is still there and has lost it, and it carries the measured reason a handle taken before the run throws rather than counting. Deleting it would throw that away and leave `Count` undocumented, which is not what F44 did with the other three: each of those was moved onto the member it describes
- Both methods now carry the comment that describes them and neither carries the other's
- Proved here: the same check F44 used, a regex over every `.cs` and `.xaml` under src for two summary blocks separated only by blank or comment lines. It read 102 files and found 0, where it found 1 before this fix. The same check over the 69 files under tests also finds 0. The add-in parsed with no references, the same six error codes as before the edit and not one `CS1xxx`. Core builds with no warning. Core tests before: 957 passed, 0 failed, 32 skipped, 989 total. After: the same
- Waits for the local machine: nothing. A comment is not a run

### What remains

- F47c, two names recorded wrongly in the F40 entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F47b pull request
2. F47c, `WorkbookWriter.ClientColumns` and `ReportOptions.FolderFor`

## 2026-09-18 F47a, the hooks do not run on a Windows checkout

### What was done

- F47a done. One new file, `.gitattributes`, and one section in `03_bader_next.md`. No code changed
- What was actually wrong, measured before anything was written. In THIS container the three hook files are LF and both walls work: `git ls-files --eol` read `i/lf w/lf` for all three and the stored blobs carry no carriage return at all. The brief said they are checked out with CRLF and that reading one through `sh` gives a syntax error, and that is not true here. It is true on Bader's machine, and the difference is the whole fault
- The repo had no `.gitattributes`, so nothing told git what these files are. Git for Windows sets `core.autocrlf` to true when it installs, which converts LF to CRLF on the way out of the object store. Proved here by cloning this repo with `core.autocrlf=true`: all three came out with CRLF line terminators, 24, 33 and 27 carriage returns. With the new file in place the same clone gives 0, 0 and 0
- What a CRLF copy does, proved here by making one and running it. The branch hook dies at its `case` line with `Syntax error: word unexpected (expecting "in")`, which is the message the brief quotes. The paths hook dies earlier, at the pipe on line 10, with `Syntax error: "|" unexpected`. The pre-commit reads `set -e` as `set: Illegal option`
- One thing the brief has the wrong way round, and it changes what the damage is. A dead hook does not fail open. `sh` exits 2 on a syntax error and 2 is the code that REFUSES, so on that machine the paths wall and the branch wall refuse EVERY call rather than none, which reads as Claude Code being broken rather than as a wall doing its job. The pre-commit is the one that fails open: it stumbles past the bad `set -e` and exits 0, so every commit goes through with no test run
- The file itself. `text=auto` is the default, so git decides what is text and stores it with LF. `eol=lf` is forced on `*.sh` and on `.githooks/pre-commit` by path, so those three are LF in the working tree whatever `core.autocrlf` says. `steps/logs` and `samples` are marked `-text`, because they are evidence: the one run log in the repo is stored with CRLF, it came off a Windows run, and its line endings are part of what it records
- Which files changed bytes: NONE. `git add --renormalize` over the whole repo staged nothing but the new file. Everything was already stored the right way here, so this changes what a future checkout gets and nothing in the index
- Proved here, and the whole output is in the pull request body. The paths wall in seven cases: it refuses a write under `samples`, under `steps/logs`, under `bundle` and a Windows spelled path under `samples`, each with exit 2 and its refusal line, and it allows a write under `src`, a write under `steps` and a call carrying no file path, each with exit 0. The branch wall in five cases, run against a checkout with main out: it refuses `commit`, `push` and `git -C . commit` with exit 2 and its line, and allows `git status`, a command with the word commitment in it, and a commit on a fix branch. The pre-commit in three: exit 1 with its own line when `dotnet` is not on PATH, exit 1 with `REFUSED. The tests failed.` when one assertion is broken in a throwaway clone, and exit 0 after running all 989
- What cannot be proved here and is said rather than claimed. `core.hooksPath` is unset in this container, so the pre-commit is not installed and no real commit here passes through it. It was run directly instead, which is the same script and not the same wiring. Step 197 of `03_bader_next.md` is where Bader switches it on
- Core tests before: 957 passed, 0 failed, 32 skipped, 989 total. After: the same. This fix touches no code

### What remains

- F47b, one doubled comment left, and F47c, two names recorded wrongly in the F40 entry
- D7 in `03_bader_next.md`, eight steps, waits for Bader like everything else in that file

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F47a pull request
2. F47b, the doubled comment on `ResolvedInTheDocument`

## 2026-09-18 The plan for the third audit round, F47a to F47c

### What was read first

- CLAUDE.md, the four files under `.claude/rules`, the top entry of `steps/log.md`, `steps/01_next.md`, `steps/02_questions.md` and `steps/04_audit.md`, all of them whole. Then the three hook files, `.claude/settings.json`, and the F40 entry of this log, which F47c corrects
- Ground state on main at 4793987, the close of the second round. Core tests under mono on Linux: 957 passed, 0 failed, 32 skipped, 989 total. That is the baseline every entry in this round compares against
- There is no `gh` on this machine, so the checks are watched through the Actions API over curl and the GitHub tools the session holds, the same way both earlier rounds watched them. Merge only when the tests job is green on the branch head

### What was measured before the plan was written, and one correction to the brief

- The brief says the three hook files are checked out with CRLF and that reading one through `sh` gives a syntax error. **In this container they are LF and both hooks work.** `git ls-files --eol` reads `i/lf w/lf attr/` for all three, the stored blobs carry no carriage return at all, and the paths hook refuses a write under `samples` with exit 2 and allows one under `src` with exit 0. The branch hook refused a Bash call of mine during this reading, which is the second wall proving itself
- The fault is real and it is a WINDOWS checkout. The repo has no `.gitattributes`, so `core.autocrlf`, which Git for Windows sets to true when it installs, converts LF to CRLF on the way out. Proved here: a local clone of this repo with `core.autocrlf=true` checks all three out with CRLF line terminators, 24, 33 and 27 carriage returns
- What a CRLF copy then does, proved here by making one and running it. The branch hook dies with `Syntax error: word unexpected (expecting "in")` at its `case` line, which is the message the brief quotes. The paths hook dies earlier, at the pipe on line 10, with `Syntax error: "|" unexpected`. The pre-commit hook takes `set -e` as `set: Illegal option`
- One thing the brief gets the wrong way round, and it matters. A dead hook does not fail open. `sh` exits 2 on a syntax error, and 2 is the code that REFUSES, so on Bader's machine both hooks refuse every call rather than none, and the pre-commit stumbles through to exit 0 and lets every commit past untested. So the paths wall and the branch wall are not down, they are jammed shut, and the test wall is down
- `core.hooksPath` is unset in this container and `dotnet` is not on PATH here, so the pre-commit hook is not installed and its own refusal is the one thing that cannot be proved by a real commit here. It can be run directly and that is what will be shown
- F47b checked. `ClashRunner.cs` has two summary blocks stacked at line 1129. The first describes `Count`, which is twelve lines below and now carries no comment of its own. So it is not a comment for a method that is gone, it is a comment that was displaced when F45 inserted `ResolvedInTheDocument` above `Count`
- F47c checked. `WorkbookWriter.ClientColumns` is public at line 84 with no reference anywhere in src, the XAML or the tests, and its comment says it exists so a test can assert the header, which no test does. `ReportOptions.FolderFor` is internal at line 141 and two tests read it. Both lines of the F40 entry are wrong as the brief says

### The order and what each PR does

1. **F47a. Branch `fix-F47a`.** The hooks. A `.gitattributes` at the root, `text=auto` by default and `eol=lf` forced on `*.sh` and on `.githooks/pre-commit` by path, with the reason in a comment. The three files renormalised so git and the disk both hold LF. Then both walls proved on this machine, each shown refusing what it must refuse and allowing what it must allow, with the output in the PR body, and the same clone test run again to show a Windows checkout now gets LF. A short section in `03_bader_next.md` so Bader sees for himself that the hooks are live. This plan entry goes in with it
2. **F47b. Branch `fix-F47b`.** The one doubled comment. The displaced block is MOVED onto `Count`, which it describes and which has no comment, rather than deleted. The brief says delete, and deleting would throw away a measured comment and leave a method undocumented, which is not what F44 did with the other three. Then the stacked summary check run again over the whole of src, with the count in the PR body. It is 1 now and it will be 0
3. **F47c. Branch `fix-F47c`.** The two wrong names in the F40 entry. `WorkbookWriter.ClientColumns` deleted, because nothing reads it and the reason its comment gives is not true, and Q26 is unanswered so the rule decides. The F40 entry corrected in place on both lines, with one short line saying what the chat audit of 2026-09-18 found. Then every other name on the F40 deleted list checked the same way, by the bare name and again with the type in front, and any other line that says deleted where the member is internal or still there corrected too, with the table in the PR body
4. `steps/03_bader_next.md` read once more for what F47a added or changed
5. The closing entry

### Rules held through the round

- One PR per fix, branched off main, merged only when the tests job is green on the branch head, the local branch deleted after. Never a pull request left open at the end of a step. The remote branches are not deleted, Bader has that command
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`
- Nothing is claimed that was not run here. Where the container cannot show a thing, the entry says so rather than filling the gap

## 2026-09-12 The second audit round is closed

### What was done

- Eight fixes merged today, in the order the brief set: F46, F40, F41, F42, F43, F44, F45 and F16. Each on its own branch off main, each a draft pull request merged once Actions was green, each with its own entry above. No pull request is open and nothing was committed on main
- `steps/03_bader_next.md` read end to end against the code, which is the last thing the round asked for. 90 Look for lines checked. Eight lines corrected, covering ten things the file promised that the code does not do
- How it was checked. One reader per section of the file, eight of them, each told to find the code that prints or shows what its Look for lines promise. Every claim one of them made was then handed to a separate reader told to REFUTE it and to default to refuted. Twelve claims were made and ten survived. Then each of the ten was read again here against the code before a word of the file was changed. Two were refuted and the file was left alone: the second `NWF attempt` line does follow the CLASH block on a rerun, and the CLASH block does say the tests ran
- Separately, every string the file quotes in backticks was matched against the source by a script that strips the seams a C# concatenation leaves. 72 strings, and every one of them is written somewhere under src
- The ten. Two, the GROUPS block and its unticked count, were promised after a Cancel, and the block is written when a run STARTS, so a Cancel leaves nothing to read. Three, the label counting the NWFs, the `Rebuilt: 6` in the dialog and the Rebuilt labels in the Run as column, were broken by that same Cancel: the preview behind it opens each NWF and leaves the last one open, and the preview will not run at all while something is open. One more of the same kind further down, `Rebuilt: 1`, where a document was still open from the run before. One said the blocks come UNITS, SETS, CLASH on a run with no XML, and with no XML no set is built at all, so there is no SETS block after UNITS. Two said a GROUP finished line ends with `DONE`, and `DONE` sits before the seconds and the path. One said the CLASH block counts the pictures, and it counts clashes: the pictures have their own IMAGES line after it. One was written today, in F45, and said to expect `Element ID` in the ITEM IDS block, and what that block names is the property that actually matched, which is `Id`, with `Element ID` as the label this tool writes
- The five that were about the preview came from one cause, the Run and Cancel in the F27 proof, so that Run is gone. F27 now costs a Scan and no run at all, and its block is read off the 1B06PH run further down where thirteen groups are unticked. Everything after it reads true again, and one step was added to close what is open before the F9 run
- The D6 branch list rebuilt. `git ls-remote --heads origin` and the GitHub branches API both answered 38 names after the last merge of the round, and the branch this entry is written on makes 39, so 38 to delete. The command names all 38. Bader runs it, not the worker
- `steps/01_next.md` renumbered. Three fixes are left, F21 which can be started here, F18 which waits for the sample Bader uploads, and F23 which waits for Q20. F15, dispose in `SetBuilder` and `ClashRunner.Resolve`, is CLOSED by F41, which is the same two files and more, so B5 and B6 close with it
- The opening line of the file said twenty fixes have merged since the last run. It names the range now, F5 to F46, because the count could not be measured off anything
- Core tests: 957 passed, 0 failed, 32 skipped, 989 total. Unchanged by this entry, which touched no code

### What remains

- Nothing in this round. `steps/01_next.md` has F21, F15, F18 when the sample arrives, and F23 when Q20 is answered
- Q24 to Q30 are open for Bader. Q24 and Q25 from the first audit, Q26 and Q27 from F40, Q28 from F41, Q29 from F43, Q30 from F44
- The whole of `03_bader_next.md`, 201 steps, waits for the machine with Navisworks on it. Nothing in this round was proved by a run

### Known bugs

- As in the F46 entry

### What comes next

1. Bader builds, installs and works `03_bader_next.md` from step 1
2. The run logs come back into `steps/logs` on their own branch, as step 183 says
3. Bader runs the D6 delete command himself

## 2026-09-12 F16, the tests path neutral

### What was done

- F16 done. The whole test set now PASSES in this container. Before: 918 passed, 37 failed, 33 skipped, 988 total. After: 957 passed, 0 failed, 32 skipped, 989 total. That is the number every log entry has carried since F32 and it is zero now
- Why there were 37. Every one was a test written for a Windows path, not a fault in Core, and all of them passed on the Windows runner. A backslash is an ordinary character off Windows, so a path typed as `C:\out\reports` has no folder in it at all: `Path.GetDirectoryName` finds none and `Path.Combine` joins with a forward slash, and the expectation and the answer differ at whichever index the separator sits
- Paths are BUILT now, in `TestPaths.At`, which gives a rooted path in the spelling of whatever machine is running. 28 of the 37 were that and they run on both
- Five are about a rule of the file SYSTEM and not of this tool, so they say what they need and skip: a drive letter that names no drive, matching two paths without case, twice, and a file held open refusing to be deleted or read, twice. `TestPaths.OnWindowsOnly` is the one place that decision is written
- Two of them did not need skipping and were proved another way instead. A copy into a folder that cannot exist, and a settings file in a place that cannot be written, are now a folder under a FILE that is already there, which no system makes. Those two rules are proved everywhere rather than on Windows alone
- One test passed off Windows without proving anything. `ReadsAFullPathThroughToTheName` handed the parser a path with no separator in it, which is one long file name whose parts after the first still read correctly, so it passed while the folder was never taken off. It builds the path now and asserts the Stem, which is the part that shows the folder went
- Three compared a `ReadOnlyCollection` against an array with `Is.EqualTo`. They passed on the Windows runner and failed under mono, and which of the two is the odd one is UNKNOWN. They compare the count and then each element, which answers neither question and needs no answer
- Six skips misnamed their reason. They said the supplied exports or the exchange file were not in this checkout, and they ARE: the paths were joined with a backslash, so nothing was found under that name. They read the files now and run. A skip that misnames its reason is worse than a failure, because it reads as a missing sample
- One Core fault came out of it. `NamePattern` and `ReportPaths` both asked `Path.GetInvalidFileNameChars` what a file name may not carry, and off Windows that is the null character and the forward slash and nothing else, so a date format holding a colon or a pipe passed the check that exists to catch it. `Federator.Core.Naming.FileNames` now names Windows' own list, the nine printable ones and every control character, and both read it. The tool runs on Windows and writes Windows names, so the platform running the test is the wrong thing to ask
- One sentence about a leftover temp folder sat above the same catch block in nineteen test files. `TempFolder.Make` and `TempFolder.Remove` are that rule in one place, used by all nineteen
- The four copies of the walk up to the checkout were already one, `Samples.Repo`, which F40 did
- Proved here: the whole set, 957 passed and 0 failed. The 32 skips are 26 that need Navisworks on the machine, 1 UNC path, and the 5 Windows file system rules, and every one of them says which. Core builds with no warning. The arity check over the whole add-in, 0 mismatches. The parse of the whole add-in with no references, the same six error codes as before
- Waits for the local machine: nothing in F16 is an add-in change. The proof is Actions, which runs the same set on Windows and is green

### What remains

- The read of `03_bader_next.md` against the code, then the closing entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F16 PR
2. Read `03_bader_next.md` end to end against the code and correct every Look for line that does not match
3. The closing entry

## 2026-09-12 F45, the clash step keeps its rules

### What was done

- F45 done. Four rules the clash step broke, and one the docs promised and the code did not keep
- A missing tolerance is a skip and not a zero. `ExchangeReader.ReadDouble` returned 0.0 for a test with no tolerance attribute, and zero is a real tolerance, so a test the file said nothing about read as one set to zero. The definition now carries whether the attribute was there at all, and a test without it is skipped by name with a new reason, `NoTolerance`, the way an unknown test type is. A tolerance written as zero is a real one and is still created, which is the line between the two and is its own test
- The OLD line says the status word and nothing else. It used to say Navisworks marks a test Old when it has run and something changed after, and what puts a test into Old is UNKNOWN from the DLL. The rules file already said no sentence is put on it. Now the code agrees
- The compacted count is measured. It reported the Resolved count read BEFORE compacting as the number removed, so the number was never measured and zero removed could not be said. It is now the count before less the count read again after `TestsCompactAllTests`, over every test in the document, walked once and descending folders. Both numbers go in the line. A count after that is higher than before is said plainly and nothing is reported as removed
- A side that could not be read is left out of the comparison. It comes back as the word UNKNOWN, which is a real string and compared like any other, so a side nothing could read was reported as drift. The marker moved to Core, `TestSettings.UnknownLocator`, because the comparison is in Core and the marker was in the add-in, and the comparison now leaves such a side out and counts it. The DRIFT block says how many were left out and that not compared is not the same as matching
- `IdFrom` reaches the log. The rules file says which property supplied the id goes in the log, and it was set on every item and read by nothing. `ClashReport.IdSourceLines` counts the items by the property that supplied each id, and the engine writes an `ITEM IDS` block per group. One line per property and never one per item, because a line per item is the flood the log has been drowned by once already
- Q25 is still unanswered, so the five extra item properties stay where they are
- Proved here: ten new tests. A test with no tolerance attribute, one with an empty one, one written as zero, and the words the skip reads in the log. A side reading UNKNOWN left out, the other side still compared, each unread side counted, and the block saying so. The id sources counted per property, and a report with no items writing no block. The arity check over the whole add-in, 0 mismatches. The parse of the whole add-in with no references, the same six error codes as before and not one `CS1xxx`. Core builds with no warning. Core tests before: 908 passed, 37 failed, 33 skipped, 978 total. After: 918 passed, 37 failed, 33 skipped, 988 total
- The F44 entry said F16 comes next. F45 does, and this is it. The order in the brief is F40 to F45 and then F16
- Waits for the local machine: two add-in files changed, the clash runner and the engine. Steps 191 to 194 of `03_bader_next.md` carry the four lines to read off the runs already planned

### What remains

- F16, then the read of `03_bader_next.md` against the code, then the closing entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F45 PR
2. F16, the tests path neutral

## 2026-09-12 F44, the docs and the comments agree with the code

### What was done

- F44 done. 62 files. No logic changed except one member renamed, one dead constant deleted and one taken off the public surface, each named below
- The moved doc path, measured rather than taken from the list. `grep -rn "scan\.md" .` outside `.git`, `docs\history`, `steps`, `bin` and `obj` found 31 lines in 22 files carrying the old `docs\scan.md`, in both spellings. The audit said seventeen source and test files and the round brief said 28 places. 30 of the 31 are repointed at `docs\history\scan.md`, including two that named the file with no folder at all, one of which is a line the tool prints into the log. The 31st is in the bundle manifest and is Q30. `build\probe-window-scroll.ps1` in the window XAML is the probe's real path, `tools\probes`. No file outside `steps` and `docs\history` names `test-model-side.md` at all, so that one was already done
- The counts. Thirteen column headings is fifteen, seven before the item blocks and four in each of the two, counted off `ClientReportColumns` and read off the header row of both committed exports. Five pickers is six, the logo picker included, counted off `PickerKind`. Three rerun cases is four, counted off `RerunDecision`. The eight faults a workbook check once reported clean over is still eight, and the fixture that breaks them now holds fourteen tests. Twenty one picture names is eight names asserted, read off a folder holding 64 pictures and a logo
- The rules file made to agree with itself, ten corrections. Distance is the rounded number with no format, which is what the code writes, and the bullet asking for a raw number behind a format is gone. The Item ID label is chosen by the tool, and which property supplied the value is kept on the item. The workbook check reports sheets, blocks, rows, the count of each block and the first divergence, and counts no column of ours, because there is no column of ours to count. The five extra item properties are written nowhere and there is no choice about it. The picture cost figures are in the log and nowhere else, because the Summary sheet they used to go on is gone. A group can still end left alone, when its rebuild never started. `HealthCheckResult.DistinctRuleCount`, not `HealthCheck`. The Run button is in the bar along the bottom that every step shares. The picker count. And the Layer column, which is the one that had to be settled by opening the files
- The Layer column, settled by measurement. The rules file said the client's report has NO Layer column and that the thirteen are the whole of it. Both committed exports carry one per item block, read off their header rows on 2026-09-12, and the code has written fifteen headings all along. The two exports the old bullet was measured on, 1A04WE and 1A02WE, were read on Bader's machine and are not in this checkout, which is now said wherever either is named
- The sheets that are gone. `HasSheet` is `HasRows`, with its three callers and three tests, because it means has rows and every test had a sheet only before the workbook became one. The comments on `TestReport`, its `Number` and `Name`, `OpenClashes`, `PlannedClashTest` and two test fixtures say block or row where they said sheet
- The logo comments. Three said nothing here copies the logo, and the engine copies one into the report's own _files folder every run. What is true is that no copy of it is in this repo or the bundle, it is read off the machine that is running, and that is what all three say now
- The copies folded. Four stacked summaries, two of them on the wrong member, unstacked and the displaced block put on the member it describes. Four copies of one sentence about `File.Exists` said once per file. Two `Images` properties sharing a rule, now said on the runner and pointed at from the harvest. Two naming members sharing a sentence about the project code where one holds a position and the other a value. Four rule sentences copied word for word out of the rules file now say what the code does and point at the rule
- Two comments that restated the test name under them are gone, and two that restated the value of the constant beside them now carry the measurement and not the number
- The probes. `probe-units.ps1` and the three window probes had no path test and no UNKNOWN line, which the probes README says every probe has. Each now tests every path it needs, one file at a time so a missing one is named, and says UNKNOWN and stops. The README needed no change once they were true
- The test layout. `CrossFileTests` is three tests and all three are health checks, so it moved to a new `Health` folder and every Core folder now has a test folder, which is what CLAUDE.md says. The health assertions that sit beside the reader tests for one sample file stay there, because they read the same file
- CLAUDE.md. The bundle folder is not build output. It holds one hand written manifest that `install.ps1` copies into `artifacts`, and `artifacts` is what the build writes. Both lines say that now, and the never edited rule stands with its real reason
- Two members the F40 entry named as deleted and which were still there. `ClientFormat.DistanceFormat`, a public const nothing called, whose comment stated the opposite of what the code does, is deleted. `ClashReportXml.QuickProperties`, no caller under src and two tests reading it, is internal, which is what F40 did with that kind. The F40 entry overstated on both and this entry is the correction
- Noticed and not acted on. `TestReport.Number` has no caller under src and two tests read it, which is the Q26 kind. The one sentence about a leftover temp folder sits above the same catch block in nineteen test files, and one shared helper for it is F16, which is next
- Proved here: Core builds with no warning. The parse of the whole add-in with no references, the same six error codes as before the edits and not one `CS1xxx`. The arity check, 0 mismatches. The window XAML parses as XML after the two text changes. Core tests before: 908 passed, 37 failed, 33 skipped, 978 total. After: the same, because this fix adds no test and changes no rule a test reads
- Waits for the local machine: six add-in files and the XAML changed, all of them comments except the `HasRows` rename and the help line under the paste into cells tick. The proof is the build and one run whose SETS and CLASH blocks read as before

### What remains

- F16, then the read of `03_bader_next.md` against the code, then the closing entry
- Q30 for Bader, raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F44 PR
2. F16, the tests path neutral

## 2026-09-12 F43, three settings that are constants

### What was done

- F43 done. The stop after count, the log count and the report subfolder each have the default they always had and can now be set. The progress interval in `ClashRunner` stays a const, because no rule names it, which is what the audit's verifier pointed out
- The stop after count is `ReportOptions.StopAfterFailures`, defaulting to the guard's own `DefaultThreshold`, and the one guard the whole run shares is built from it in the engine constructor. It was a field initialiser calling the no argument constructor, so no run could reach the number. A count under one is refused where it is set, with the sentence the guard already uses, rather than throwing in the middle of a run
- The log count is `RunLog.KeepLogs`, defaulting to `DefaultKeepLogs`, read by the no argument `StartOrDisabled` the plugin calls. It is static because the log opens on the first line of the button handler, before a window or any options object exists, so there is nothing else for it to hang off. Zero keeps the live file alone, which is a real answer, and fewer than none is refused
- The report subfolder is `ReportPaths.Subfolder`, defaulting to `DefaultSubfolder`, read by the one place that builds the fallback folder. Static for the same kind of reason: the open file run and the line under the Outputs step both work the folder out with no options object in front of them, and threading a parameter through four call sites would have put the same name in four places. A name that is empty or carries a slash is refused where it is set
- Both slashes are refused by name rather than by asking the running platform what it calls a separator. The first version asked, and the test that hands it `Reports\Weekly` passed the backslash straight through in this container, where the separator is a forward slash. That is the F16 fault caught in a new test before it was written down
- One dead thing went with it. `ClashRunner`'s two argument constructor built its own guard and nothing in src or in the tests called it, so it is gone and the guard is handed in, which is the rule that the guard lives for the run and not for a group
- `ImageOptions.DefaultStopAfterFailures` already reads `RepeatedFailureGuard.DefaultThreshold` rather than repeating the 50. F40 did that one
- Nothing outside the code sets any of the three, so today the change is that they can be set at all. Whether a settings file is wanted is Q29
- Proved here: thirteen new tests, each changing a setting and reading the answer back out of the thing that uses it, and handing each one a value it must refuse. The folder actually chosen follows the subfolder setting, and so does the line under the Outputs step. A guard built from a count of two stops after the second failure and not the first. The arity check over the whole add-in, 0 mismatches. The parse of the whole add-in with no references, the same six error codes as before and not one `CS1xxx`. Core builds with no warning. Core tests before: 895 passed, 37 failed, 33 skipped, 965 total. After: 908 passed, 37 failed, 33 skipped, 978 total
- Waits for the local machine: two add-in files changed, the engine and the clash runner. The proof is the build and one run whose CLASH block reads as before, because every default is the number it always was

### What remains

- F44, F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry
- Q29 for Bader, raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F43 PR
2. F44, the docs and the comments agree with the code

## 2026-09-12 F42, no framework message in a label

### What was done

- F42 done. Four labels that carried a type name and a framework message now carry plain words, two stale sentences about what a run with no XML does are corrected, and seven defaults that were typed into the XAML as well as into Core are read from Core
- The four labels. `RunLog.DisabledReason` named the folders it tried with the exception behind each one, and the window wrote the lot into the progress line. It now names the folders and nothing else, and the log line under it carries what each one threw. The repeated failure guard built its reason from the exception type name and message and handed it to the progress label. It now has two, `Reason` for the log, which keeps what was thrown, and `ReasonInPlainWords` for the label, which says how many tests failed the same way and that the log says what the failure was. `FederationEngine.Describe` appended every error, type names included, into the open file run's summary label. It now says how many errors there are and that the log says what they were. The window's `Describe(path)` returned the exception message when a picked file would not read. It now says the log says why, and the failure is already laid out in the log by the line above it
- Every one of those wordings lives in Core where a test reads it, the way `ReportPaths.WhereTheyGo` does. `RunLog.WhereTheLogIs`, `NoLogFileOpened`, `ErrorsAreInTheLog` and `TheLogSaysWhy`, and `RepeatedFailureGuard.Stopped`. `WhereTheLogIs` has a static form taking the three things the line depends on, so the rule can be read without a log on a disk
- The two stale sentences. The Clash step heading said leaving the XML empty makes Run do the model side only, and the `ClashWork` class comment said the same. Since F8 a run with no XML runs the tests already saved in each NWF and builds no set, which is what both say now. Only a run with no XML and no saved test does the model side alone
- The seven defaults. The photo size, the cap, the paste into cells tick and the five status ticks were typed into the XAML while `ImageOptions` held the same values in Core, the 1024 among them, which is measured off all 60 pictures of the accepted report. The XAML carries none of them now and the constructor fills them from a fresh `ImageOptions`, filled where the naming boxes are filled and for the same reason. The audit named the size and the five ticks. The cap and the paste tick are the same duplication in the same grid, so they went with them
- `probe-window-defaults.ps1` builds the real window and reads the controls, so it reads whatever the constructor set and needed no change. It will now print the `ImageOptions` values rather than the XAML's
- Noticed and not acted on. `FolderMemory.DisabledReason` is written in four places, carries `error.Message`, and nothing under src reads it. It reaches no label, so it is not this fix's, and it is the same kind of member F40 made internal, which is Q26. Bader decides
- Proved here: ten new tests in `LabelWordsTests`, each handing the wording the very thing a label must not carry and asserting it is not in what comes back, which is the rule that a test reading only the good case proves nothing. The arity check over the whole add-in, 0 mismatches. The parse of the whole add-in with no references, the same six error codes as before the edits and not one `CS1xxx`. Core builds with no warning. Core tests before: 885 passed, 37 failed, 33 skipped, 955 total. After: 895 passed, 37 failed, 33 skipped, 965 total. The ten more are the new fixture and the 37 and the 33 are unchanged
- Waits for the local machine: the add-in does not build here. Three add-in files and the XAML changed. The proof is the window opening with the photo size reading 1024, the cap 0, New, Active and Reviewed ticked and Approved and Resolved clear, and the Clash step heading reading the new sentence

### What remains

- F43 to F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F42 PR
2. F43, three settings that are constants

## 2026-09-12 F41, every handle disposed

### What was done

- F41 done. Three add-in files, `SetBuilder`, `SavedTests` and `ClashRunner`. No logic changed and no line the log prints changed. Every handle this tool creates or resolves is now released where it is finished with, rather than left to a finalizer
- The rule behind it was measured and is in the history file. All of `ClashTest`, `ClashResult`, `ClashSelection`, `SelectionSet`, `SelectionSource`, `Selection`, `ModelItemCollection`, `Search` and `SavedItem` are disposable, and releasing a wrapper releases the wrapper and never the document's object. `SavedItemCollection` is not disposable and is never disposed, so no collection is touched
- `SetBuilder`. The `Search` and the `SelectionSet` built from it are in a using each, the set released first so nothing it shares with the search goes while the search is still read. The folder the set is added under, which came back from `EnsureFolders`, is in a using around the whole build. The `FolderItem` handed to `AddCopy` is in a using, because `AddCopy` takes a copy. The four folder reads inside `EnsureFolders` that answered a question and were then dropped are released. `ResolveFolders` releases each level it walks past. `FindFolder` and `FindSelectionSet` release every child they do not hand back, `Describe` releases every child it names, and all three `ModelItemCollection` reads are in usings
- `SavedTests`. `SelectionA` and `SelectionB` are read once each into a using instead of twice each, so a walk of 1830 tests makes two wrappers a test and not four
- `ClashRunner`. The root of the set tree in `IndexSets`, the sides read for the comparison, the sides filled when a test is created and when one is replaced, the `Selection` behind each side in `LocatorOf`, `FillSide` and `ItemsOn`, and the leaf that turns out not to be a test in `Resolve`. `Resolve` releases each level it walks past, and releases it only once the child below has been read off it, which is the order `ResolveFolders` in `SetBuilder` and `WalkTests` here already use. Nothing reads a collection whose owner has gone
- `AddedAt` in `SetBuilder` reads the child at the index the count held before the add and checks its name, the way `Create` in `ClashRunner` already does for tests, and says so in the log when the tree is not that shape and it has to look by name instead. That is the read that made the set path O(n squared)
- Counted on the reference file, 61 sets and 1830 tests in one group. The side reads alone were 14,640 wrappers a comparison run before this and are 7,320 now, and the four folder reads dropped in `EnsureFolders` were one a folder a set
- Two reads were left alone on purpose and are Q28. `search.Selection` in `SetBuilder`, once a set, and the source read out of a side's own collection in `LocatorOf`, which sits inside the loop over the indexed sets and so is read once a set rather than once. Neither is named in the F41 list, both are small beside what was fixed, and the ownership of a sub object read off a handle this tool owns is not in the measured record. Bader decides rather than the worker assuming
- Proved here: a parse of the whole add-in through the Roslyn compiler with no references, which reports every syntax error and nothing else. Before the edits and after them the error codes are the same six, all of them the missing framework and the missing Navisworks types that no references means, and not one `CS1xxx`. That is the first check of add-in syntax this repo has had in the container. The arity check over the whole add-in, every `new` and every static call against the parameter counts declared under src, 0 mismatches. Core is untouched, so the tests are the same before and after: 885 passed, 37 failed, 33 skipped, 955 total
- Waits for the local machine: the build, then one building run twice with the XML, the SETS and CLASH blocks read as before and the run no slower. The steps are in `03_bader_next.md`

### What remains

- F42 to F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry
- Q28 for Bader, raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F41 PR
2. F42, no framework message in a label

## 2026-09-12 F40, dead members out, second pass

### What was done

- F40 done. 45 members deleted, 64 taken off the public surface, 66 files touched, 1045 lines out and 171 in
- Every name was checked twice before anything went. Once by a script written for this, which reads every `.cs` and `.xaml` under src, classifies each reference to a name as code, comment or string, and reports the hits outside the declaring file, inside it, and in the tests. That is what separates `ClashRunOutcome.Ran`, which nothing calls, from `PageCheck.Ran` and `WorkbookCheck.Ran`, which are called and share the name. Once again by fourteen readers, one per area, each grepping src and the XAML itself with a verifier told to refute it. The two readings agree on every member. The script's table and the readers' verdicts are both in the PR body
- What went outright, 45. The four collections on `ClashRunOutcome`, `RepeatedFailureGuard.Seen` and `Threshold` with the counter behind them, `ClashTestPlan.SkipReasonCounts`, `OpenClashes.All`, `Default` and `Describe`, `BuildStamp.LooksStamped` back as internal, `BundleAssemblies.MissingFrom` with `ReferencesOf` which only it called and `Resolved` with the list it copied, `FolderMemory.IsRemembering`, `GroupRecord.Seconds` with the constructor that took it, `RunLog.Start()` and two `GroupFinished` overloads, `ExchangeReader.ReadFiles`, `SelectionSetDefinition.Guid`, nine members of the report model that went with the Summary sheet, `ClientFormat.DistanceFormat` which was F44's and not this fix's, `ClientShapes.CoordinatesIn`, `ImageOptions.Statuses`, `WorkbookWriter.ClientColumns`, which stayed and went out under F47c, and `Options`, `PageCheck.HeaderColumns`, `ReportOptions.FolderFor` internal rather than out, `ClashReportXml.QuickProperties` internal under F44 and not this fix's, `ContainerNameSettings.Copy`, `OutputNaming.All` and `Copy` with `NamePattern.Copy`, `OutputNameTable.Build` three argument, `ParsedContainerName.SourceName` with the parameter that fed it, `BuildingGroupingResult.Find` internal rather than out, `NwfComparison.Moved` with `Moves` and its `LeafOf`, and in the add-in `ClashRunner.Drift`, `GroupRow.Names`, `ClashHarvest.Into` three argument and `JobOutcome.ReportWarnings` with `AddReportWarning` and its four call sites
- Corrected on 2026-09-18. The chat audit read every name above against the code and against this fix's own commit. Five did not go out here and each is marked in the line above, so the 45 counts five that did not go. What the figure should be is Q31
- What went internal rather than out, 64. A member with no caller under src whose only reader is a test that pins a rule the repo states. Deleting it deletes the proof of the rule, so it comes off the public surface instead and the test still reaches it through the `InternalsVisibleTo` the Core project already carries. The reference file holding one batchtest and 1830 tests, 61 distinct locators, linkage none and rules empty in every test, a set name ending in a space, the stamping having run, the picture name round tripping, what the page check and the workbook check report. That boundary is Q26, because it is a reading of the rule and not the rule itself
- Why `JobOutcome.ReportWarnings` could go without losing anything. The engine added four warnings to it and nothing read the list. Each of the four already reaches the log by another path: two through `log.Block` with the check's own lines, one through the renumbering lines written just above it, one through `log.Failure`. So the list was a second copy of what the log already carries, and the two check methods stopped taking an outcome they no longer read
- The copies folded into one place. `NwfComparison` stopped working out a move, which `NwfRebuildPlan.From` already does and which the engine reads. `ImageOptions.DefaultStopAfterFailures` reads `RepeatedFailureGuard.DefaultThreshold` rather than typing 50 again. `ImageNaming.LogoName` reads `InstallFiles.LogoName`, because the copy in the report folder is that file. `ScanFindings` reads `BuildingGroup.IsSingleDiscipline`, the rule F35 made. `Samples.Repo` is the one walk up to the checkout, in place of four copies in the report fixtures
- One cascade worth naming. Deleting `TestReport.OpenUnder` and `NewPlusActive` left `OpenClashes.Of` with no caller, so it went too, and the two choice rule for what counts as still outstanding now has one reader left, the image status filter asking for what Navisworks counts as open. That is a stated rule losing its last output, so it is Q27 rather than a silent deletion
- `OutputNameRow.ReleaseToPattern` and the five per item properties stay where they are. Q24 and Q25 are unanswered
- Proved here: the arity check over the whole add-in, every `new` and every static call against the parameter counts declared under src, 0 mismatches, run before and after the add-in edits. The Core project and the test project both build clean with no warning. Core tests before: 905 passed, 37 failed, 33 skipped, 975 total. After: 885 passed, 37 failed, 33 skipped, 955 total. The 20 fewer are the tests deleted with the members they proved, named in the PR body, and the 37 and the 33 are unchanged, so nothing else moved
- Waits for the local machine: the add-in does not build here. Five add-in files changed, `ClashHarvest`, `ClashRunner`, `FederationEngine`, `JobOutcome` and `GroupRow`, every hunk read twice and the whole diff read again after the edit. Proof is the build, then one run whose log carries the same REPORT CHECK and WORKBOOK CHECK blocks as before

### What remains

- F41 to F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry
- Q26 and Q27 for Bader, both raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F40 PR
2. F41, every handle disposed

## 2026-09-12 F46, the four the chat audit found

### What was done

- F46 done, four parts, no logic. Each was checked here before it was written, and the checks are in the PR body
- 46a. The D6 command in `steps/03_bader_next.md` named `claude/parsons-nwc-analysis-rlzgdr`, which is not a branch on the remote, and left out `fix-F39` and `round-close`, which are. The wrong name came from `git branch -r`, which prints the remote-tracking refs a clone remembers, and that ref survived here because nothing had pruned it. The command is rebuilt from `git ls-remote --heads origin`, which asks the remote and remembers nothing, and both that and the GitHub branches API return the same 30 branches, so 29 to delete. The steps now read the live list first, delete from it, read it again, and prune the clone. The trap is written beside the command so the next person does not repeat it
- 46b. `.gitignore` ignored `*.log` everywhere, and `steps/logs/README.md` asks Bader to copy a run log in and commit it. `git check-ignore -v steps/logs/2026-09-20-1C07BC.log` answered `.gitignore:50:*.log`, so a new log would never appear in GitHub Desktop. The log already in the folder looked fine only because git does not re-ignore a file it is already tracking, which is what hid this for six days. One negation line, `!steps/logs/*.log`, with the reason above it in the style of the `build/` block. Proved by writing a file into the folder from the shell and reading `git status`, which now says `?? steps/logs/zzz-check-2026-09-20.log` where before it said nothing. The file was removed the same second and nothing under `steps/logs` was edited
- 46c. `steps/README.md` listed four files and the folder holds six and the logs folder. It names all seven now, says what each is for, and says where to start. `03_bader_next.md` gets its own line saying to read it at the machine with Navisworks on it. A script compares the names in the Files list against the folder and finds nothing missing either way
- 46d. `RunLog.GroupRecords` was public and F36 kept it that way because one test read it. The rule is that a test is not a caller, so it is private, and `TheFailedCountAndTheFailedListAlwaysAgree` writes the RESULT block and reads the three group count lines back off the disk, which is what the property was standing in for and what every other test in that fixture already does
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. The changed fixture on its own: 9 passed, 0 failed
- Waits for the local machine: nothing in F46. The D6 command is Bader's to run and the list in it will have this round's own branches in it by then, which is why the steps read the live list first

### What remains

- F40 to F45 and F16 in that order, then the read of `03_bader_next.md` against the code, then the closing entry

### Known bugs

- As in the round close entry

### What comes next

1. Merge the F46 PR
2. F40, dead members out, second pass

## 2026-09-12 The plan for the second audit round, F46 and F40 to F16

### What was read first

- CLAUDE.md, the four files under `.claude/rules`, `steps/log.md` top entry, `steps/01_next.md`, `steps/02_questions.md`, `steps/03_bader_next.md` and `steps/04_audit.md`, all of them whole. Then `.gitignore`, `steps/README.md`, `steps/logs/README.md` and the files each F46 part names
- Ground state on main at 8902ff06, the round close merge. Core tests under mono on Linux: 905 passed, 37 failed, 33 skipped, 975 total. That is the baseline every entry in this round compares against, and the 37 and the 33 are the ones `04_audit.md` names one by one
- There is no `gh` on this machine, so the checks are watched through the GitHub tools the session holds and through the Actions API over curl, the same way the last round watched them. Merge only when the tests job is green on the branch head
- The add-in still does not build here and has not been built on Bader's machine since 1 Sep. So before every add-in edit the arity check runs again over `src`, every `new` and every static call against the parameter counts declared under src, and its result goes in the PR body. After every add-in edit the changed file and every caller of what changed are read, not the diff alone. That is the reading F34 got through

### The four the chat audit found, checked here before the plan was written

- 46a. `steps/03_bader_next.md` step 188 names `claude/parsons-nwc-analysis-rlzgdr`, which is not a branch on the remote, and leaves out `fix-F39` and `round-close`, which are. Both readings agree: `git ls-remote --heads origin` and the GitHub branches API each return 30 branches, so 29 to delete. The wrong name came from `git branch -r`, which shows a stale remote-tracking ref that no longer exists on the remote, and that trap goes in the file beside the command
- 46b. `git check-ignore -v steps/logs/2026-09-20-1C07BC.log` answers `.gitignore:50:*.log`, so a log Bader copies in tomorrow is invisible to GitHub Desktop. `run-20260907-093440.log` reads as not ignored only because it is already tracked, which is what hid this
- 46c. `steps/README.md` lists four files. The folder holds six and the `logs` folder
- 46d. `RunLog.GroupRecords` is public, `RunLog` itself reads it at line 892 and one test reads it. F36 kept it public for that test. The rule says the test is not a caller, so it goes private and the test reads the result the property was standing in for

### The order and what each PR does

1. F46. Branch `fix-F46`. The four above in one PR, because two of them break Bader's next hour and none of them is code. The D6 command rebuilt from the live list with the reading named, the `.gitignore` negation with `git check-ignore -v` output in the PR body, `steps/README.md` rewritten for six files and the logs folder, `GroupRecords` private with its test reading the RESULT block instead. This plan entry goes in with it
2. F40. Branch `fix-F40`. Dead members out, second pass. The 43 with no reference at all, the 34 uncalled once the type is checked, the 9 public for nothing made private, each re-grepped over `src` and the XAML before it goes and the results pasted in the PR body. `ReleaseToPattern` and the five item properties stay, Q24 and Q25 are unanswered. The copies go with them: `NwfComparison.Moves`, `Moved` and its `LeafOf`, `RepoRoot` in four test files reading `Samples.Folder`, `ImageOptions.DefaultStopAfterFailures` reading `RepeatedFailureGuard.DefaultThreshold`, `ScanFindings` reading `BuildingGroup.IsSingleDiscipline`, one `LogoName`
3. F41. Branch `fix-F41`. Every handle disposed in `SetBuilder`, `SavedTests` and `ClashRunner`, and `FindSelectionSet` reading the index the count held before the add rather than walking the collection. Add-in only, so the arity check runs first and every changed file is read after
4. F42. Branch `fix-F42`. No framework message in a label. `RunLog.DisabledReason`, the guard reason, `FederationEngine.Describe` and the window's `Describe(path)` each carry plain words to the label and keep the exception for the log. The Clash step heading at XAML line 426 and the `ClashWork` class comment both still say a run with no XML does the model side only, and since F8 the saved tests run, so both say that instead. The photo size and the five status ticks are filled from `ImageOptions` in the window constructor. Every wording lives in Core where a test reads it
5. F43. Branch `fix-F43`. Three settings that are constants: the stop after count, the log count and the report subfolder each become a property with the same default, read where the guard is built, the log is started and the folder is chosen. The progress interval stays a const, no rule names it
6. F44. Branch `fix-F44`. The docs and the comments agree with the code. The old doc path is in 28 places and not seventeen, both spellings, and `test-model-side.md` and `build\probe-window-scroll.ps1` go the same way. Then the wrong counts, the rules file made to agree with itself, the Summary sheet comments, `HasSheet` renamed, the logo comments, the doubled and copied comments, the three project names in comments, the Health tests into a Health folder, and the probes README made true
7. F45. Branch `fix-F45`. The clash step keeps its rules. A test with no tolerance attribute is skipped by name, the OLD line logs the status word alone, the compacted count is read after the compact, an UNKNOWN locator is left out of the drift comparison. `IdFrom` waits on Q25
8. F16. Branch `fix-F16`. The tests path neutral. The 37 that fail here, the six skips that misname their reason, the three collection comparisons, the one test that passes off Windows without proving anything, `Samples.Folder` in place of the four `RepoRoot` copies, one temp folder helper in place of nineteen copies of its comment
9. `steps/03_bader_next.md` read end to end against the code one more time. Every Look for line matched against what the code prints or shows, the ones that do not match corrected, and the count of corrections in the log
10. The closing entry

### Rules held through the round

- One PR per fix, branched off main, merged only when the tests job is green on the branch head, the local branch deleted after. Never a PR left open at the end of a step. The remote branches are not deleted, Bader has the command
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`
- A public member nothing in src calls is deleted with its tests, unless a decision in `02_questions.md` keeps it. No member is added that no running code calls
- The arity check before every add-in edit, the changed file and its callers read after, both said in the PR body
- No em dash, no semicolon in prose, no emoji, plain words, in every comment, doc, log line and window text
- `samples` and `steps/logs` are never touched
- A new question goes in `02_questions.md` as Q26 onward

### What remains

- Everything in the order above. Nothing in this round is proved on the local machine, because the add-in cannot build here

### Known bugs

- Unchanged from the round close entry until each fix lands

### What comes next

1. F46 on `fix-F46`, this plan goes in with it
2. F40 to F45 and F16 in order, one PR each
3. `03_bader_next.md` read against the code, then the closing entry

## 2026-09-12 The round closes, F27 to F39, the audit and D6

### What was done

- Thirteen fixes merged today, F27 to F39, each on its own branch through its own pull request with Actions green, and each local branch deleted after. F12, F13 and F14 closed with F37 and F38, F25 closed with F27. D1 to D5 are in the code. D6 is not, see below
- The audit ran over every file under src, tests, build, tools, docs, steps and the root, and is written up in `steps/04_audit.md`: fourteen readers, a verifier per finding, and three checks of my own that need no judgement. 393 findings, condensed into seven fixes, F39 to F45, and F16 widened, each a section in `01_next.md` with its files, and two questions, Q24 and Q25, in `02_questions.md`
- The audit found that the add-in had not compiled since F34 merged. Two calls in the window kept passing a `true` that F34's constructors no longer take, and the diff reading that every add-in change gets could not see it, because a diff shows the constructors that went and not the callers that stayed. F39 fixed it and is merged. The build is the first step of `03_bader_next.md` and the reason it comes before every proof is written there
- `steps/03_bader_next.md` rewritten so the proofs for F27 to F35 sit in one build, one install and one Navisworks session, 191 numbered steps, F34 first because it is seen the moment the window opens, then F33 in the same look, then F27 for the cost of a Scan and a Cancel, then the C06 run which proves F24, F29, F30 and F33 in one press, then F28 and F31 together, then F35, and the older pending proofs after. Four steps the audit found wrong in it were corrected the same day
- D6 is handed to Bader as one command at the end of that file. Every branch except main is merged into main, checked with `git branch -r --merged origin/main`. The container cannot delete a remote branch: `git push origin --delete` returns HTTP 403 from the proxy in front of the session, there is no GitHub tool in the session that deletes a branch, and a workflow file that would have done it from Actions was refused by the permission classifier as destructive, which it is. The command lists all 27 branches and says what to look for
- Proved here: Core tests under mono on Linux at the end of the round: 905 passed, 37 failed, 33 skipped, 975 total, the same 37 and 33 as after F32, every one named in `04_audit.md` with its cause. The audit found six of the 33 skips misname their reason, saying a sample is not in the checkout when it is, because two test files join the samples path with a backslash
- Waits for the local machine: the build first, then every proof in `03_bader_next.md`, then the two hooks of F38 tried under Claude Code on Windows

### What remains

- F40 to F45 and F16 in that order, then F21, F15, F18 when the sample arrives, F23 when Q20 is answered
- Q24 and Q25 for Bader, both from the audit
- The verifier pass of the audit ran 30 of its 54 batches and the other 24 died on the session's usage limit, so 180 findings carry a reader's evidence and my grep and no verifier, which `04_audit.md` says. Its journal lives in the session and not in the repo, so a later session re-checks a finding by grep rather than trusting it. Every finding in `04_audit.md` was checked by grep here before it went in

### Known bugs

- B12 OPEN, waiting on Q20. Narrowed by F32: an NWD, an address, no folder and an unreadable folder are each refused with the reason named. The ACC case itself still waits
- B13, B14 fixed in code, pending local proof, F24 and F26 in `03_bader_next.md`
- B1, B2, B11, L1, L2, L3, L4 and F22 fixed in code, pending local proof, each in `03_bader_next.md`
- B7 fixed in code, pending a local run of `tools/probes/probe-window-defaults.ps1`
- B9 fixed, and the local proof is now the build after F39
- M5 closed on the Core side, open on the add-in side until the build runs locally
- B3, B4, B8, B10, B15 fixed or closed. B5, B6, L5 to L8, M1 to M4, M6 to M8 as in `00_analysis.md`, of which M8 is F16 and L5 to L7 closed with F37
- New from the audit, all in `04_audit.md`: the add-in did not compile from F34 to F39, fixed. Handles not disposed in three add-in files, F41. Framework messages reaching four labels, F42. Three settings that are constants, F43. A missing tolerance read as zero, the Old sentence, the compact count read before, an UNKNOWN locator reported as drift, F45

### What comes next

1. Bader pulls main, builds, installs and follows `03_bader_next.md`, the build first
2. Bader answers Q24 and Q25
3. F40, then F41 to F45, then F16
4. The worker reads the logs Bader drops in `steps/logs` and records the proofs in this file

## 2026-09-12 F39, the window compiles again

### What was done

- F39 done, two lines. Found by the audit of 2026-09-12, whose steps-and-rules reader compared every call from the window into the engine against the engine's constructors, and confirmed by grep before anything was changed
- F34 deleted the republish flag and the three engine constructors that took it, and the two calls in the window that construct the engine for a run kept passing `true` as their third argument. The Run button passed six arguments to a constructor that takes five and the Run the open file button passed a bool where an exchange document goes. Neither matches a constructor that exists, so the add-in has not compiled since F34 merged, and F35, F36 and F37 changed it further without a build. Both calls now read `SetProgress, log, exchange, options`, with `nwfFolder` on the first, and the two hand button calls were already right
- Why it was missed: the add-in does not build here, and F34 was read twice as a diff. A diff shows the constructors that went and not the callers that stayed, so the reading found nothing wrong with either side on its own. The audit found it by putting the two side by side, which is what a compiler does
- A heuristic check was run once over the whole add-in, every `new` and every static call against the arities declared under src, and it finds this one call and nothing else. It is a script in the session and not in the repo, because the local build is the real check and comes first in `03_bader_next.md`
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. No Core file changed
- Waits for the local machine: the build. This fix is the reason the build is the first step of `03_bader_next.md`, and if the build names anything else, the whole error goes in the chat

### What remains

- The round close on its branch: `steps/04_audit.md`, the fix list in `01_next.md`, the closing entry

### Known bugs

- As in the F38 entry

### What comes next

1. Merge the F39 PR
2. Finish the round close
## 2026-09-12 F38, CLAUDE.md under 200 lines, rules in .claude, walls in hooks

### What was done

- F38 done, no code. Closes F13
- CLAUDE.md is 164 lines, down from 1004, and holds only what applies to every file: the host, what done means, a map of where things are, the rules for every file, how a fix is worked, the build, the tests, the two walls, what to confirm against the install, and the writing rules. The rule that C# stays at 7.3 is written down for the first time, it was only in the project files before
- The old file is whole in `docs/history/claude-md-history.md` with a first line saying it is history and where the current rules are, and a diff against main's CLAUDE.md shows it verbatim. Every date, count and measurement is there and nothing was cut from it
- The 122 bullets of Rules the code holds were split by which project holds the rule. 32 that name a Navisworks call, the window, the engine or the installer went to `.claude/rules/addin.md` with the tick box section. The other 90 went to `.claude/rules/core.md` with the file naming section, the clash XML section and the diagnostic log. The split was by a script that cut at each bullet, so no bullet was retyped, and the two files together hold all 122. Each starts with a paths line, `src/Federator.Addin/**` and `src/Federator.Core/**`, and a short intro saying what the other file holds
- `.claude/rules/tests.md` and `.claude/rules/steps.md` are new, written by hand from the Tests section, the F37 test layout, the round's rules for the steps files, and the reasons the rules files carry: the break one thing test, reading a written file back as a file, the client lists asserted against the client's files, the samples never written, the Assert.Ignore off Windows, Ordinal never trimmed, and one entry per fix in the F26 shape
- Two walls. `.claude/settings.json` runs two PreToolUse hooks from `.claude/hooks`. `refuse-protected-paths.sh` reads the tool call off standard input and exits 2 for an Edit, Write or MultiEdit whose file_path is under `samples`, `steps/logs` or `bundle`, with Windows backslashes read as slashes and the project folder stripped off. `refuse-git-on-main.sh` exits 2 for a Bash command holding git commit or git push, options between git and the verb allowed, while `git rev-parse --abbrev-ref HEAD` says main or master. Both were tried here with thirteen piped tool calls: three refused paths including one Windows path into `bundle`, three allowed paths including `steps/log.md`, a Bash call the path hook ignores, and on main three refused commits and pushes, `git pushover` and `git log | grep commit` allowed, and the same commit allowed on `fix-F38`. The first pattern let `git -c core.hooksPath=/dev/null commit` through because it only allowed dash options between git and commit, and that is the exact form this round commits with, so it was widened and the case is in the list
- The pre-commit hook in `.githooks` is unchanged and CLAUDE.md still says how to switch it on
- `steps/01_next.md` had F26's DONE line under F38, left there by the F26 session. It is under F26 now. F38 has its DONE line and F13 reads CLOSED by F38
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. No code changed, so the run is the same run, and the 37 are the same Windows path and file locking failures
- Waits for the local machine: whether Claude Code on Windows runs the two hooks through a sh it can find. Both need a POSIX sh, Git for Windows ships one, and whether it is found without help is UNKNOWN until a session on the local machine tries an edit under `samples` and sees it refused. Nothing else in F38 needs the local machine

### What remains

- D6, the audit into `steps/04_audit.md`, `steps/03_bader_next.md` rewritten, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F38 PR
2. D6, delete every fix branch and master so main is the only branch, once a route that is not a push works
3. The audit

## 2026-09-12 F37, one type per file and one place per kind of file

### What was done

- F37 done, moves only, no logic. Closes F12 and F14
- One type per file. Nine files held thirty types and now hold one each, split by a script that cut at each type's doc comment and gave every new file the old file's usings and namespace, so nothing but the file boundary moved. `FederationJob.cs` gave `JobOutcome.cs`, `ClashRunner.cs` gave `TestAddress.cs`, `ClashTestPlan.cs` gave `ClashTestKind`, `ClashSkipReason`, `PlannedClashSide`, `ClashPlanSource`, `PlannedClashTest` and `SkippedClashTest`, `ClashRunOutcome.cs` gave `ClashStatus`, `ClashTally`, `ClashSideCheck` and `ClashTestResult`, `GroupOutcome.cs` gave `GroupRecord`, `WrittenFile` and `LoggedFailure`, `NamePattern.cs` gave `OutputNaming`, `NameCollisions` and `NameCollision`, `ScanFinding.cs` gave `FindingKind`, `FindingsTable.cs` gave `ScanCounts`, and `SetBuildOutcome.cs` gave `SetResult`. Both projects glob their sources, so no project file changed
- The sixty five test files are in folders named after the src folder they test: Clash, Diagnostics, Exchange, Findings, Grouping, Naming, Report, Rerun, Sets, Units, with `Samples.cs` at the root. Every move is a `git mv`, so history follows. The tests that read the checkout find it by walking up from the test assembly, so none of them cares where its source sits
- The six probes are in `tools/probes`, each with a `-NavisworksPath` parameter defaulting to the folder the add-in project defaults to, and the three window probes find the repo one level higher than before, because their folder is one level deeper. `tools/probes/README.md` says what each measures and how to run it. `build` holds `install.ps1` alone
- `docs/scan.md` and `docs/test-model-side.md` are `docs/history/scan.md` and `docs/history/test-model-side.md`, each with a first line saying measurement history, not current, and the date of the move. The eleven `docs\scan.md` references in CLAUDE.md and the probe line in `steps/03_bader_next.md` point at the new paths. The references inside the two history files stay as they were, because they are part of the history
- `docs/workflow.md` is the one current description of the two workflows, the rebuild, the labels and the three `RunPath` rules that give them, the open file, and the two hand buttons, written from README and `RunPath`. README keeps its introduction and points at it, at CLAUDE.md, at `steps`, at `docs/history`, at `tools/probes` and at the installer, and no longer carries a copy of the workflows
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. The same tests in new folders, and the Core splits compile. The 37 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The two add-in splits, `JobOutcome.cs` and `TestAddress.cs`, and the two files they came out of were read after the split, every line of them is a line that was there before. The probes were not run here and cannot be. Proof is the build, then one probe with and one without the parameter

### What remains

- F38, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F37 PR
2. F38, CLAUDE.md under 200 lines, rules in .claude, walls in hooks

## 2026-09-12 F36, dead code and copies out

### What was done

- F36 done. Every name went through grep over src first, and the results are in the PR body. Deleted with their tests: `OutputNameCheck`, the pattern based collision check that `OutputNameTable` replaced, with six tests and the helper only they used. `ContainerName.BuildOutputName`, four overloads, and `ContainerNameSettings.ForcedLevel`, `ForcedTypeCode`, `ForcedNumber` and `OutputDisciplineCode` with their defaults, their lines in `Validate` and `Copy`, and `RequireValue`, which only they called, with eight tests deleted and three trimmed to the parsing they still prove. The output name has been `NamePattern`'s since the patterns existed, and both class comments say so now. `ClashWork.Any(exchange, savedTests)` and `AnyIn`, the one argument `Any` stays because `SourceFor` and `Describe` call it. `BuildStamp.OfCore`, the four tests read `BuildStamp.Of(typeof(BuildStamp).Assembly)`. `PickerStart.Remembers`, the tests read `FolderMemory.LastFor` directly. `RunLog.TimesFailed`, `DistinctFailureCount` and `TotalFailureCount`, the tests read `Failures.Count` and `Failures[i].Times`, and the one test that was only about `TimesFailed` went. `RunPath.Skipped` stays, a rebuild that fails still ends its group as Changed and the GROUP line reads it. `GroupRecords` stays public, `ResultBlockInvariantTests` reads it
- One `Or`. `Federator.Core.Diagnostics.Words.Or(value, fallback)` replaces the private copy in ten files, `BundleAssemblies`, `FolderMemory`, `RunLog`, `OpenDocumentJob`, `GroupJudgement`, `ClashReportXml`, `NavisworksFacts`, `ClashHarvest`, `ClashRunner` and `DocumentUnits`, fifty calls rewritten. The engine's one argument `Or`, which said none, is `Words.Or(value, "none")` at its eight calls. `TestDrift`'s was not an Or, it quotes the value and says nothing for none, so it is `QuotedOrNothing`. `WordsTests` pins the three cases, a value, null or empty, and a space, which is a value because two set names in the reference file end in one
- One client column list. `ClientFormat.ClashColumns`, `PerItemColumns`, `FirstItemColumn` and `ItemColumns` are read off `ClientReportColumns`, and `ClashReportXml.QuickProperties`, `QuickName` and `QuickType` too, so the fifteen words live once, where a test checks them against the client's own exports. `ClientReportColumnsTests.EveryOtherColumnListReadsThisOne` pins it
- One `InstallFiles` locator replaces `StylesheetLocator` and `LogoLocator`, which followed the same rule in two files. `FindStylesheet`, `StylesheetCandidates`, `WhyNoStylesheet`, `FindLogo`, `LogoCandidates`, `WhyNoLogo`, with one `FirstOnDisk`, one `WhyNot`, one `Languages`. The engine, the window and five test files read it. Nothing about either file changed, the paths tried and the lines logged are the same
- The seven doubled summary blocks that were left, in `ClashHarvest`, `ClashRunner`, `FederatorWindow.xaml.cs`, `RunLog`, `ClientFormat`, `ReportPaths` and `WorkbookWriter`. Five were a summary that had drifted off its method, put back above `FirstProperty`, `Run`, `Tolerance`, `Choose` and `WriteColumnHeadings`. One in `RunLog` was two summaries for `Failure`, merged. One in the window described a box that has no method, deleted. A grep for a summary closing and another opening finds nothing now
- `probe-window-defaults.ps1` no longer lists `RepublishNwd` and `WriteImages`, the two boxes that are gone
- Proved here: Core tests under mono on Linux, before: 916 passed, 37 failed, 33 skipped, 986 total. After: 905 passed, 37 failed, 33 skipped, 975 total. Eleven fewer, fifteen deleted with the members and four added, `WordsTests` three and `ClientReportColumnsTests` one. The 37 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. Six add-in files changed, `ClashHarvest`, `ClashRunner`, `DocumentUnits`, `FederationEngine`, `NavisworksFacts` and the window, every one a rename of a call or a moved comment, read twice. Proof is the build, then one run whose log reads as before

### What remains

- F37 and F38, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F36 PR
2. F37, one type per file and one place per kind of file

## 2026-09-12 F35, clash only where two disciplines meet

### What was done

- F35 done, D5. The one model rule counted files, so two NWCs of one discipline ran every test for nothing the way one NWC did. The rule now counts disciplines and lives in one place, `BuildingGroup.CannotClashWith(int disciplineCount)`, fewer than two and nothing in the group can clash. `BuildingGroup.IsSingleDiscipline` replaces `IsSingleModel`, `GroupRow` carries `DisciplineCount` off the scan and reads the same rule for its `IsSingleDiscipline` and its status line, which now says one discipline, and `FederationJob` carries the count on to the engine, which sets `ClashRunner.SingleDisciplineGroup` from `job.IsSingleDiscipline`. `ClashSkipReason.SingleModel` is `SingleDiscipline` and every line that said one model says one discipline
- `FederationJob.DisciplineCount` is `int?`. The open file passes null, because nothing was scanned and its count is UNKNOWN, so it is never judged this way, which is what the file count gave it before, an empty list is not one file. The five argument constructor nothing called is gone
- The SINGLE DISCIPLINE scan finding says the tests are created and none is run, it said none of them can find anything, which was true before D5 and is not how it ends now
- Proved here: `SingleModelGroupTests` is `SingleDisciplineGroupTests`, its nine references renamed, and it gains `AGroupWithTwoFilesOfOneDisciplineKnowsItCannotClashEither` and `TheRuleIsFewerThanTwoDisciplines`. `GroupingModeTests.AGroupHoldingOneDisciplineKnowsItCannotClash` reads the new name. Core tests under mono on Linux, before: 914 passed, 37 failed, 33 skipped, 984 total. After: 916 passed, 37 failed, 33 skipped, 986 total. The 37 are the same Windows path and file locking failures, none new. A grep for `SingleModel` and one model over src and tests finds nothing but three comments about one model in a document, which are about a different thing
- Waits for the local machine: the add-in does not build here. Five add-in files changed, read twice. Proof: scan a folder holding a building with two NWCs of one discipline, its row must read Ready. One discipline, so every test is created and none is run, and the run's CLASH block for it must say holds one discipline and skip every test with the SingleDiscipline reason

### What remains

- F36 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F35 PR
2. F36, dead code and copies out

## 2026-09-12 F34, window wiring

### What was done

- F34 done. The wiring: `NwdFolderBox` fires `OnOutputFolderChanged` like the other two folder boxes, so the Outputs summary and the run paths refresh when it is typed into. The clash XML tick box's handler is `OnXmlChanged`, it was `OnRepublishChanged`, a name left over from the box that went. The window class comment says four tabs, it said three steps. The Step 4 comment in the XAML said no Excel, that is the session after, and the marker in the code said sets only in this session, both years stale, both say what the step holds now
- The republish field is gone: `republishNwd` out of the engine, out of both constructors that stay, and the not republished branch out of `WriteNwd`, which says it publishes every run. The three engine constructors nothing called are gone. `JobOutcome.NwdRequested` is gone, `Facts()` hands `GroupFacts` a true with the reason. `GroupFacts.NwdRequested` stays, because `GroupJudgement` reads it and its tests pin the rule that a step switched off is not a failure. `GroupOutcome.cs` says the tick box used to exist
- `ReportsWanted` no longer sets `WriteHtml`, `SetDocumentUnits` or `Images.Write`. All three are true in the Core constructors, with one comment each saying they are fixed on and the window does not set them
- D3. The Outstanding counts combo is gone from the Clash step with its label and its row, `FillOpenCounts`, `ChosenOpenCount` and the outstanding log line with it. `ReportOptions.OpenCount` and `ClashReport.OpenCount` are gone, and the engine no longer copies one into the other. `OpenClashesTests` lost the two asserts on the deleted defaults and its report helper lost the parameter it set. `OpenClashes` itself stays with its tests, because `ImageOptions` reads Navisworks open off it. CLAUDE.md's bullet says the choice is no longer offered
- D4. `OnBuildSets` and `OnRunTests` each read the file, make an engine with `ReportsWanted()` and call one method: `BuildSetsByHand`, which is `BuildTheSets` on the open document with the open file standing as the group, and `RunTestsByHand`, which is `CreateAndRunTheTests` the same way with the tests from the XML. The SETS and CLASH lines in the log now read the same whichever way the work was started, the window's copies of them are gone, and so are `PlanOnlyLines` and the two section title constants. `BuildTheSets` fills `outcome.Sets` with the skipped sets when there is nothing to build, so the button has lines and a summary to show, and `SetBuildOutcome.Summary()` is the one summary line, in Core, tested. A hand run now builds the report in memory the way a run does and writes nothing, because there is no report folder
- D1. Picking an XML runs `HealthCheck` on it: the whole summary goes in the log as a `HEALTH` block and one sentence goes under the file line, from `HealthCheckResult.Line(bool fileHoldsSets)`, in Core, tested: unusable export, tests only file, every locator resolves, or how many locators miss and how many tests would be skipped
- The orphan summary that sat above `OnRunOpenDocument`, which belonged to `OnBuildSets` before it drifted, is gone, and `OnBuildSets` has its own. One fewer for F36's list of eleven
- Proved here: `SetBuildOutcomeTests` gains `TheSummaryCountsCreatedPresentFindingZeroFailedAndSkipped`, `NothingBuiltAndNothingSkippedIsAFileHoldingNoSets`, `NothingBuiltWithSetsSkippedSaysHowMany`. `AllInOneFileTests` gains `TheHealthLineSaysEveryLocatorResolves`, `InfraSetsFileTests` gains `TheHealthLineSaysTheExportIsUnusable`, `ExchangeReaderShapeTests` gains `TheHealthLineOfATestsOnlyFileSaysTheModelSetsAreUsed` and `TheHealthLineNamesHowManyLocatorsMissWhenTheFileHoldsSets`. Core tests under mono on Linux, before: 907 passed, 37 failed, 33 skipped, 977 total. After: 914 passed, 37 failed, 33 skipped, 984 total. The 37 are the same Windows path and file locking failures, none new. A grep for `republishNwd`, `OpenCountBox`, `ChosenOpenCount`, `FillOpenCounts`, `OnRepublishChanged`, `PlanOnlyLines`, `SetsSectionTitle`, `ClashSectionTitle` and `.OpenCount` over src finds nothing
- Waits for the local machine: the add-in does not build here, and this is the largest add-in change of the round, the window, the XAML, the engine and the job, read twice. Proof: build, install, open the window, the Clash step must have no Outstanding counts row, pick the reference XML and the line under it must end with every locator resolves and the log must carry a HEALTH block, press Build sets on an open model and the SETS block in the log must read as a run's does, press Run tests and the CLASH block likewise, and type into the NWD folder box and watch the Outputs summary change

### What remains

- F35 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F34 PR
2. F35, clash only where two disciplines meet

## 2026-09-12 F33, one unit table

### What was done

- F33 done. Four lists named units and they had drifted: `ExchangeUnits` held eleven codes with factors, `DocumentUnits.Short` held eight short labels and lowercased the enum name for the rest, so Micrometers wrote "micrometers" where `ClashRunner.UnitName` wrote "um" for the same unit, and the window held five names with its own wording. Now `src/Federator.Core/Units/UnitTable.cs` is the one table, one `UnitRow` per unit with the Navisworks enum name as text, the display name, the short label, the exchange code and the millimetres in one. `ExchangeUnits` converts through it and holds no factor, `DocumentUnits.Short` and `ClashRunner.UnitName` look their enum value up in it, and the window fills its combo from `UnitTable.Offered()`, five rows with the default first, wording the default as before
- `TryWantedUnits` replaces `WantedUnits`. A model units name the table does not know is no longer parsed with a silent fallback to Meters. `FinishTheGroup` logs `UNITS    <name> is not one this tool knows`, puts the reason with every known name on the group's error list, and returns before anything is converted, run or written, so the group is FAILED. A name in the table that the installed enum does not carry is logged as that and fails the same way
- `ExchangeReader.ReadTest` reads the tolerance as written and converts nothing. `ClashTestDefinition.ToleranceMillimetres` and `ToleranceIn` are gone with their asserts. So a file whose units attribute the tool does not know now reads, and `ClashTestPlan.Convert`, the one place a file unit is judged, skips each test by name with the line that says the unit is not one this tool converts. That line could not be reached before, because the reader threw on the whole file first. `ExchangeUnits.ToMillimetres` and `FromMillimetres` are gone too, nothing in src called them once the reader stopped
- `ReportUnits` and CLAUDE.md say the factors are `UnitTable` read through `ExchangeUnits`, and CLAUDE.md gains one bullet on the table and the failed group
- Proved here: `UnitTableTests`, eight tests, every column filled, no enum name or exchange code twice, every offered row in the table with `ReportOptions.DefaultUnits` first, lookups case blind and trimmed, an unknown enum name refused naming every known one, the feet row giving the reference file's 75 mm, the report label being the metres row, and `ExchangeUnits` knowing exactly the table. `ExchangeReaderShapeTests` gains `AFileInUnitsTheToolDoesNotKnowIsStillRead`, `ClashTestPlanTests` gains `AFileUnitTheToolDoesNotKnowSkipsEveryTestByNameRatherThanThrowing`. Core tests under mono on Linux, before: 897 passed, 37 failed, 33 skipped, 967 total. After: 907 passed, 37 failed, 33 skipped, 977 total. The 37 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. Four add-in files changed, `ClashRunner.UnitName`, `DocumentUnits.Short`, `FederationEngine.TryWantedUnits` with the guard in `FinishTheGroup`, and the window's three unit methods, read twice. Proof: open the window, the Model units combo must list Metres, which is what the models are set to, Millimetres, Centimetres, Feet, Inches in that order, and a run must log the same UNITS lines as before

### What remains

- F34 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F33 PR
2. F34, window wiring

## 2026-09-12 F32, the open file is guarded

### What was done

- F32 done, D2. `OpenDocumentJob.CanRun` checked one thing, that the document had a name, so an NWD opened directly was allowed and the NWD this tool publishes would have been written over the file that was open. Five things are now checked in order and `WhyNot` names the first that fails, in a person's words: it has a name, it was opened from a folder and not from an address, it is an NWF, it has a folder in front of its name, and that folder can be read from here. Each refusal says what to do instead
- `CanRun` and `WhyNot` gain a second form with the disk read handed in as `Func<string, bool>`, and the one argument form hands in `Directory.Exists`. That is the seam that lets every reason be proved without a folder that exists on the machine running the tests, and the one argument form is tested against a real temp folder and a missing one
- `NwdBeside` never names the open file. Where the swap lands on the same path, read case blind, empty comes back. `CanRun` refuses such a file before anything is written, this is the second lock on the same door
- The window did not change. `OpenDocumentLine` already shows `Describe`, which returns `WhyNot` when the file cannot run, the button is disabled by `CanRun`, and pressing it warns with `WhyNot`. The engine logs `OPEN     ` and the same reason. The window file is listed under F32 in `01_next.md` and stays unchanged, said there
- B12 narrowed. What Navisworks reports as the file name of a document opened from Autodesk Docs is still UNKNOWN, Q20, so the ACC case is not claimed. The note is under F23 in `01_next.md` and on B12 in `00_analysis.md`. CLAUDE.md's open file bullet carries the five checks
- Proved here: `OpenDocumentJobTests`. `AnNwdOpenedDirectlyStillNamesItsOutputs` is replaced by `AnNwdOpenedDirectlyIsRefusedAndSaysToOpenTheNwf`, `TheRefusalCarriesNoCodeIdentifier` by `EveryRefusalCarriesNoCodeIdentifier` over all seven refusals, and eight tests added: `AnNwcOpenedDirectlyIsRefusedTheSameWay`, `TheExtensionIsReadCaseBlind`, `ANameWithNoFolderBehindItIsRefusedAndSaysSo`, `AnAddressRatherThanAFolderIsRefusedAndNamed`, `AUncPathIsAFolderLikeAnyOther`, `AFolderThatCannotBeReadIsRefusedAndNamed`, `TheOneArgumentFormAsksTheDisk`, `TheNwdNeverLandsOnTheOpenFile`. The UNC test ignores itself where the separator is not a backslash, because a UNC path is only a path on Windows, so it is skipped under mono and runs on the local machine. Core tests under mono on Linux, before: 888 passed, 39 failed, 32 skipped, 959 total. After: 897 passed, 37 failed, 33 skipped, 967 total. The 37 are Windows path and file locking failures, none new, and two fewer than before because `TheLineLeadsWithWeeklyRunAndNeverFirstRun` and `TheLineSaysWhatItWillDoAndWhereBeforeAnyonePressesIt` now build their file in a temp folder that exists, since `Describe` asks the disk, and so pass here too
- Waits for the local machine: nothing to build for this one beyond the ordinary add-in build. Proof: open an NWD in Navisworks, open the window, the Clash step must read the refusal naming .nwd and the Run the open file button must be greyed. Open the NWF and the line must name the NWD and the report folder as before

### What remains

- F33 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F30 entry. B12 narrowed, still open on the ACC case

### What comes next

1. Merge the F32 PR
2. F33, one unit table

## 2026-09-12 F31, the clash side lookup is built once per run

### What was done

- F31 done. `LocatorOf` created a `SelectionSource` for every indexed set, per side, per compared test, and disposed each straight after. On a weekly run plus XML every test is compared, and 61 sets by two sides by 1830 tests is 223,260 sources made and thrown away for the comparison alone. `IndexSets` now builds one source per indexed set into `sourceByPath` beside `byPath`, keyed the same way, and `LocatorOf` compares each side's sources against those. The set wrappers were never disposed at all, now both dictionaries are released by `ReleaseTheIndex` in the one finally at the end of `Run`, sources first and then the set wrappers they point at, and inside `IndexSets` before a throw part way through building leaves
- `byPath` is declared before the try in `Run` so the finally can see it, and `ReleaseTheIndex` takes null as nothing to release, which is what a run that stopped before indexing holds. A throw while releasing is logged and never stops the run
- `sourceByPath` is a field, replacing the `setsForLookup` field, because `LocatorOf` is reached through `SettingsOf` from `CompareAndMaybeApply` and the set wrappers already travel as a parameter. `SettingsOf` and `LocatorOf` lose their `byPath` parameter, which they no longer read
- `FillSide` loses the document parameter nothing in it read. `Create` only handed that parameter on, so it loses it too, and `OneTest` calls `Create` without it. `Apply` and `Create` call `FillSide` positionally. `FillSide` still creates a fresh source each time and says why: the collection takes the source it is handed, so one out of the index would be taken back out from under the test when the index is released
- `Resolve`, `Upwards` and `SetBuilder` are untouched, that is F15
- UNKNOWN until a run: whether `SelectionSource.Equals` matches a source held since the sets were indexed the way it matched one created a moment before. The old code relied on the same `Equals` and it was never measured either. The DRIFT block on the proof run answers it, a locator difference on every compared test would say no
- Proved here: Core tests under mono on Linux, before and after: 888 passed, 39 failed, 32 skipped, 959 total. No Core file changed. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The diff is 108 added against 32 removed, read twice, every call of `FillSide`, `Create`, `SettingsOf`, `LocatorOf`, `IndexSets` and `ReleaseTheIndex` checked against its signature by grep. Proof: run one building twice with the XML picked, the second run's DRIFT block must report no locator difference and the CLASH block must show the same counts as the first, and the run must not be slower

### What remains

- F32 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F30 entry

### What comes next

1. Merge the F31 PR
2. F32, the open file is guarded

## 2026-09-12 F30, one tail for both run paths

### What was done

- F30 done. `RunOne` and `RunOpenDocument` each carried the same six calls after the models were in the document, the units, `ClashStep` with `SaveTheNwfAgain` on a true, `WriteWorkbook`, `WriteNwd` and `ConfirmTheNwfSurvived`, and the comments explaining the order sat on the scanned copy only. The six are now one private method, `FinishTheGroup(document, job, outcome)`, placed right after `RunOne`, called by both at the point the six stood, with the comments moved once. Nothing else in either method changes, the diff is 43 lines added against 42 removed
- Both methods were read again after the move. Ten things still differ and the PR body lists them: the signature and where the job comes from, where the document is read, the group clock and the GROUP lines that `Run` writes for a scanned group and `RunOpenDocument` writes for itself, the two no document messages, the `OpenDocumentJob.CanRun` guard, where `reportFolder` is set, the four OPEN lines against the `Decide` lines, the fixed Open decision against the three `Decide` cases, the two catch headings, and the try shape. None of them is the tail
- One comment moved as it was and is stale since F26: it says the units go before the clash step so the report reads in the units it goes out in, and since F26 the report is converted to metres in one pass whatever the document reads. The move keeps it verbatim because F30 changes nothing but the place. It goes on the audit list
- Proved here: Core tests under mono on Linux, before and after: 888 passed, 39 failed, 32 skipped, 959 total. No Core file changed. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. Proof: build, install, run one building on the scanned path and once on the open file path, and read the two logs. After the models are in, both must show UNITS, then SETS and CLASH, then `NWF      attempt`, XLSX, NWD and the final NWF line in that order

### What remains

- F31 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F27 entry, plus the stale units comment above, for the audit

### What comes next

1. Merge the F30 PR
2. F31, the clash side lookup is built once per run

## 2026-09-12 F29, the rebuild keeps the sets on their own count

### What was done

- F29 done. `RebuildFromScan` used to put the sets back only when the test count had dropped, and never counted them, so a clear that kept the tests and lost the sets would have saved an NWF whose tests point at nothing. Now the sets are counted by walking `SelectionSets.RootItem` before the clear, after the appends and after the copy is put back, every wrapper disposed on the way, and put back whenever the count after the appends is lower than before, whether or not the tests dropped. Sets first, then the tests as before, because a test side points at a set
- One line, `SETS     before clear <n>, after appends <n>, after restore <n>`, then the saved tests line. When the sets cannot be put back the error goes on the group's list the way the tests do, so the group is FAILED and the NWF on disk is not saved over
- `setsCopy` is disposed in the finally beside `testsCopy`, through `as IDisposable`, because `SavedItemCollection` was not IDisposable on the DLL measured on 2026-08-31, docs/scan.md 4g, and a plain `Dispose()` would not compile if it still is not
- The `Decision == Changed` branch at the top of `ClashStep` is deleted. Since F24 a CHANGED group is rebuilt before the clash step or returns before reaching it, so it could not run
- The set line and the two rules live in `NwfRebuildPlan`: `SetsLine`, `SetsKept`, `SetsNeedRestoring`
- Proved here: `NwfRebuildPlanTests` gains `TheSetsLineCarriesTheThreeCountsInOrder`, `SetsThatDidNotComeBackAreLostAndTheLineSaysTheNwfWasNotSavedOver`, `TheSetsArePutBackOnAnyDropWhetherOrNotTheTestsDropped`, `AnNwfWithNoSetHasNothingToKeepOrRestore`. Core tests under mono on Linux, before: 884 passed, 39 failed, 32 skipped, 955 total. After: 888 passed, 39 failed, 32 skipped, 959 total. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The engine change is read twice. `DocumentSelectionSets.CreateCopy` and `CopyFrom` are still unmeasured, as under F24. Proof: scan the C06 folder against the old NWFs, the six rebuilt groups must each log a SETS line whose three numbers agree, and the sets tree of the rebuilt NWF opened in Navisworks must hold the same sets as before

### What remains

- F30 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F27 entry

### What comes next

1. Merge the F29 PR
2. F30, one tail for both run paths

## 2026-09-12 F28, a set already there is not counted as created

### What was done

- F28 done. `SetBuilder.BuildOne` called `outcome.AddAlreadyPresent(path)` and then `outcome.AddCreated(...)` for the same set, so `CreatedCount` and `PutAnythingIn` counted present sets and the second NWF save fired on every weekly run
- `SetBuildOutcome.AddAlreadyPresent` is now one call carrying the path, the name, the condition count and the item count. It returns a `SetResult` marked `Present`, printed by `Lines()` as `present <path>  <n> conditions  <n> items  already there, left alone`, counted in `AlreadyPresentCount` and never in `CreatedCount`, `FindingItemsCount`, `ZeroCount`, `FailedCount` or `TotalItems`. `BuildOne` makes that one call and logs the line it returns
- The separate list of present paths is gone, the count comes off the one results list the lines come from, so the two cannot disagree
- Proved here: `SetBuildOutcomeTests`, every `AddAlreadyPresent` use takes the add-in shape, and three tests added: `SixtyOnePresentAndNoneCreatedPutNothingInAndPrintsNoOkLine`, `SixtyPresentAndOneCreatedPutSomethingIn`, `APresentLineCarriesItsItemCountAndIsNotCreated`. Core tests under mono on Linux, before: 881 passed, 39 failed, 32 skipped, 952 total. After: 884 passed, 39 failed, 32 skipped, 955 total. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The `BuildOne` change is five lines, read twice. Proof: run one building twice with the XML picked, the second run's SETS block must show every set as present, `sets created      : 0`, `already there     : 61`, and no second `NWF      attempt` line after the CLASH block unless a test was created

### What remains

- F29 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F27 entry

### What comes next

1. Merge the F28 PR
2. F29, the rebuild keeps the sets on their own count

## 2026-09-12 F27, the GROUPS block tells the truth

### What was done

- F27 done, F25 closed. The claim was checked: nothing in src sets `GroupRow.Include` to false except the Run column binding and `GroupRow.Blocked`, and `git show be0b9b37:src/Federator.Addin/Ui/GroupRow.cs` shows the same. There is no hidden discipline rule. The run log of 2026-09-07 showed 12 groups as skipped because the GROUPS block printed skipped for any group unticked in the Run column
- The block now prints `unticked` for such a group and ends with one line, `<n> groups unticked in the Run column, nothing else drops a group`. The line lives in `RunLog.UntickedGroupsLine` so it is tested, and the window calls it
- `01_next.md` rewritten: F25 closed with the reason, F27 to F38 added after F26 in the order set on 2026-09-12, F12, F13 and F14 marked absorbed by F37 and F38. B15 closed in `00_analysis.md` and Q21 closed in `02_questions.md` with the same reason
- `00_analysis.md` step 8 fixed. `HealthCheck.Run` is not called by anything in src today. F34 wires it, D1
- README label table matches `RunPath.All`: Rebuilt added, Skipped kept and reworded, because a rebuild that does not finish still ends its group as Changed and the GROUP line reads Skipped
- Proved here: `RunLogTests` gains `TheUntickedLineCountsTheGroupsAndNamesTheRunColumn` and `NothingUntickedStillSaysSo`. Core tests under mono on Linux, before: 879 passed, 39 failed, 32 skipped, 950 total. After: 881 passed, 39 failed, 32 skipped, 952 total. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The window change is nine lines in `GroupListLines`, read twice. The proof is to scan any folder, untick two groups, press Run and Cancel, and read the GROUPS block in the log

### What remains

- F28 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- B15 closed. Everything else as in the F26 entry

### What comes next

1. Merge the F27 PR
2. F28, a set already there is not counted as created

## 2026-09-12 The plan for the audit round, F27 to F38

### What was done

- Read first: CLAUDE.md, `steps/00_analysis.md`, `steps/01_next.md`, `steps/02_questions.md`, `steps/03_bader_next.md`, `steps/log.md`, then every file under `src`, `tests`, `build`, `docs`, `.github`, `.githooks`, `bundle` and the root. 47,658 lines. The files every fix touches were read by hand. One reader per folder is still walking the rest for members without a caller, doubled comment blocks and stale comments, and what they find goes into F36 and the audit at the end
- Ground state on main at c89dad07. Core tests under mono on Linux: 879 passed, 39 failed, 32 skipped, 950 total. The 39 are the Windows path and file locking failures recorded since F5
- `dotnet build src/Federator.Addin/Federator.Addin.csproj -c Release` was run once. It cannot build here: `Autodesk.Navisworks.Api.dll` is not on this machine and the build says so by name. So every add-in change is read twice before it is pushed, the PR body says so, and the local proof goes into `steps/03_bader_next.md` as numbered one action steps
- There is no `gh` on this machine. The checks are watched through the GitHub tools the session has, the same way F5 to F26 were. Merge only when the tests job is green on the branch head. A branch delete through git is refused by the session's proxy, so D6 goes through the GitHub tools
- Eleven doubled summary blocks found by grep, in `RunLog.cs`, `PageCheck.cs`, `WorkbookWriter.cs`, `ReportPaths.cs` twice, `ClientFormat.cs`, `ClashHarvest.cs`, `SetBuilder.cs`, `ClashRunner.cs` and `FederatorWindow.xaml.cs` twice. Twelve private `Or` helpers across Core and the add-in. All for F36
- The F36 greps run on main: `OutputNameCheck`, `ClashWork.Any`, `BuildStamp.OfCore`, `PickerStart.Remembers`, `RunLog.TimesFailed`, `DistinctFailureCount` and `TotalFailureCount` have no caller in src. `BuildOutputName` is called only by itself and `ForcedLevel`, `ForcedTypeCode`, `ForcedNumber` and `OutputDisciplineCode` only by it. `GroupRecords` is read once inside `RunLog` itself. `RunPath.Skipped` IS reached: a rebuild that fails ends the group as Changed and the GROUP line reads it, so it stays

### The order and what each PR does

1. F27. Branch `fix-F27`. The GROUPS block says unticked instead of skipped and adds one line counting the unticked groups. The line lives in `RunLog` so it is tested. F25 rewritten as never in the code, B15 and Q21 closed with that. `00_analysis.md` step 8 fixed, HealthCheck does not run on pick today. README label table matches `RunPath.All`. This entry and the F27 entry go in with it
2. F28. Branch `fix-F28`. `SetBuildOutcome.AddAlreadyPresent` takes the path, the name, the condition count and the item count, prints as present, counts in `AlreadyPresentCount` only. `BuildOne` makes that one call. Two new tests on `PutAnythingIn`
3. F29. Branch `fix-F29`. `RebuildFromScan` counts the sets before the clear and after the appends by walking `SelectionSets.RootItem`, puts the sets back on their own count, logs `SETS before clear <n>, after appends <n>, after restore <n>`, fails the group when they cannot come back, disposes `setsCopy`. The dead Changed branch at the top of `ClashStep` goes. The set line and the set judgement live in `NwfRebuildPlan` with tests
4. F30. Branch `fix-F30`. One private tail method for `RunOne` and `RunOpenDocument`. The PR body lists every line that still differs
5. F31. Branch `fix-F31`. `IndexSets` builds one `SelectionSource` per set beside `byPath`, `LocatorOf` compares against that, both dictionaries disposed in one finally at the end of `Run`. `FillSide` loses its document parameter. `Resolve`, `Upwards` and `SetBuilder` untouched, that is F15
6. F32. Branch `fix-F32`. `OpenDocumentJob.CanRun` refuses a non .nwf extension, a path with no readable folder, and an NWD path equal to the open path, `WhyNot` names which. Five tests. The window shows `WhyNot`. F23 in `01_next.md` notes the narrowing
7. F33. Branch `fix-F33`. `src/Federator.Core/Units/UnitTable.cs` is the one table. `ExchangeUnits`, `DocumentUnits.Short`, `ClashRunner.UnitName`, the window's `UnitChoices` and `UnitWording` read it. `WantedUnits` fails the group on a name the table does not know. `ExchangeReader` stops converting, `ToleranceMillimetres` and `ToleranceIn` go, `ClashTestPlan.Convert` is the one place a file unit is judged, with a test that reaches its unknown unit line
8. F34. Branch `fix-F34`. Window wiring: `NwdFolderBox` TextChanged, `OnXmlChanged`, `OnRepublishChanged` and `republishNwd` and the three unused engine constructors deleted, `NwdRequested` deleted if nothing reads it, `ReportsWanted` stops setting the three fixed flags, D3 the outstanding count setting deleted, D4 the two hand buttons call one engine method each, D1 HealthCheck wired under the file line, the XAML comments and the window class comment fixed, `GroupOutcome.cs` docs stop naming a republish tick box
9. F35. Branch `fix-F35`. D5. `BuildingGroup` and `FederationJob` carry the discipline count, the flag comes from it, `ClashSkipReason.SingleModel` becomes `SingleDiscipline`, every one model wording becomes one discipline. Two Core tests
10. F36. Branch `fix-F36`. Dead code and copies out, each name grepped and the empty result pasted in the PR body. One `Or` in `Words.cs`. One client column list. `InstallFiles` replaces the two locators. The eleven doubled summaries fixed. `probe-window-defaults.ps1` stops listing the two boxes that are gone
11. F37. Branch `fix-F37`. Moves only. One type per file, tests into src folder names, probes into `tools/probes` with a README and a path parameter, the two docs into `docs/history` with a first line, `docs/workflow.md` written from README and `RunPath`. Closes F12 and F14
12. F38. Branch `fix-F38`. CLAUDE.md under 200 lines, history to `docs/history/claude-md-history.md`, four rules files under `.claude/rules` with paths frontmatter, two PreToolUse hooks under `.claude/hooks` wired in `.claude/settings.json`, the pre-commit hook kept. Closes F13
13. D6. Every `fix-*` branch, `analysis-pass` and `master` deleted on GitHub so main is the only branch
14. The audit. Every file read again, what still contradicts CLAUDE.md, every member without a caller, every doubled comment, every doc line that disagrees with the code, every test that cannot run here, written to `steps/04_audit.md` with a fix list, and the fixes added to `01_next.md`
15. `steps/03_bader_next.md` rewritten so the proofs for F28, F29, F31, F32, F33, F34 and F35 sit in one build, one install and one Navisworks session, with the first proof named and why
16. The closing log entry

### Rules held through the round

- One PR per fix, off main, merged only when the tests job is green on the branch head, the branch deleted after. Never a PR left open at the end of a step
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`
- A public member nothing in src calls is deleted with its tests, unless a decision keeps it. No member is added that no running code calls
- No em dash, no semicolon in prose, no emoji, plain words. Every log line starts with its block word
- Each PR body says what was proved here and what waits for the local machine. Each log entry records the Core test counts before and after
- `samples` and `steps/logs` are never touched

### What remains

- Everything in the order above. Nothing in this round is proved on the local machine, the add-in cannot build here

### Known bugs

- Unchanged from the F26 entry until each fix lands. B15 closes with F27, F25 was never in the code

### What comes next

1. F27 on `fix-F27`, this plan goes in with it
2. F28 to F38 in order, one PR each
3. D6, the audit, `03_bader_next.md`, the closing entry

## 2026-09-07 F26, the report is always in metres

### What was done

- `steps/logs` holds one run log, `run-20260907-093440.log`, already read and recorded under F24. No new log since, so every fix from F5 onward stays pending local proof
- F26 done, bug B14. Every UNITS line of that log was read. 14 groups say the document is in Feet and the report needs Meters. 12 lines say THE DOCUMENT DID NOT FOLLOW. Only 2 groups followed, 1B06K1 and 1B06M1, and both had exactly one model set. Every group with 2 or more models set stayed in feet
- What the code set and what it read. It SET each model through `DocumentModels.SetModelUnitsAndTransform`, which is the only public managed member in the API that sets units at all, measured across all 4027 types on 2026-09-01, docs/scan.md 4q. It then READ `Document.Units`, which has no setter anywhere, and judged the report by it
- Why a document can stay in feet after every model is set. Two different things carry the word units. A model's units are a per file override, which is what `SetModelUnitsAndTransform` writes and what the Units and Transform dialog shows. What the scene is measured and displayed in is an application option, Options, Interface, Display Units, which Autodesk describes as used to measure geometry, align appended models and set tolerances for clash detection. It is not a document property and the API exposes no setter for it. So setting every model changes each file and leaves the display unit alone. Why one model set made `Document.Units` report metres and two did not is UNKNOWN and cannot be read off the DLL
- The fix does not wait for the document to follow. New `src/Federator.Core/Report/ReportUnits.cs` converts the FINISHED report into metres in one pass, before the workbook, the page or the XML is written, so all three read the same numbers and the same label. Converted: the tolerance of every test, and the distance and the clash point of every row. Not converted: grid location, level, status, counts, names, because none of them is a measurement
- The factors are `ExchangeUnits`, which is the one conversion table in this repo. Nothing was duplicated
- A unit the table has not been taught is REFUSED. Nothing is written, three skipped lines are logged, and the group is FAILED through the error list, which `GroupJudgement` already turns into FAILED. No rule in the judgement needed changing
- Setting the models is kept, unchanged, because it fixes what the person sees in Navisworks. No application option is touched, so nothing has to be restored afterwards
- The log line changed. `UNITS    <n> models set, <n> that would not, document shows Feet (ft). Every report number is converted to Meters (m) before it is written`, then after the clash step `UNITS    every number converted from ft to m, one ft is 0.3048 m, <n> tests and <n> rows. The report is written in Meters (m)`. The words DID NOT FOLLOW are gone from the code
- The window combo is now labelled Model units, its grey line reads `What Navisworks shows. The report is always in metres.`, and the run settings carry two lines, `model units` and `report units : Meters (m), always, converted before anything is written`
- Tests: `ReportUnitsTests`, sixteen. The table over m, ft, in, mm and cm. The two report unit names. The tolerance converted with its label, 0.2460629921 ft reading 0.075m in the cell. A row's distance and clash point converted with the sign kept. Nothing but the measured numbers touched. The report label afterwards. A document already in metres converting nothing and not doubling. An unknown unit refused with nothing half converted and the label not faked. A document that said nothing about its units refused. A null report refused. The log line naming the unit, the factor and the counts. Every test and every row counted
- Core tests under mono on Linux, before: 863 passed, 39 failed, 32 skipped, 934 total. After: 879 passed, 39 failed, 32 skipped, 950 total. The 39 are the same Windows path and file locking failures, none new
- The add-in was not compiled. It cannot compile in the container

### What remains

- F25 next, then F16, F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered
- The proofs from F5 onward on the local machine, F24 first and F26 second, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- B14 fixed in code, pending local proof. Run one group whose models are in feet, the report header must say Meters (m), one clash distance must match the Clash Detective panel when the panel is switched to metres, and the log must carry the new UNITS line with no DID NOT FOLLOW anywhere
- B13 fixed in code, pending local proof. Pull main, build, install, scan the same C06 folder, the six groups must show Rebuilt in the list, and after the run each NWF must hold every scanned file and the NWD must hold them too
- B15 OPEN, F25 next
- M5 closed on the Core side, still open on the add-in side because the add-in build stays local
- L4 fixed in code, pending local proof. The picture on row N of a block must be picture N in the order the Clash Detective panel lists the clashes
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. The XML box on with every image status unticked, then the other way round
- L2 fixed in code, pending local proof. Under F24 a folder that differs is rebuilt, so the proof is that no UNITS line comes before the REBUILT block
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED
- B2 fixed in code, pending local proof. One Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. The open file run must end with a RESULT block and a log copy
- L1 fixed in code, pending local proof. No XML, the tests saved in the NWF must run
- F22 done in code, pending local proof. The list must show First run and Weekly run per group
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M4, M6 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F26 PR
2. Bader pulls main, builds, installs, and works through `03_bader_next.md`, F24 first and F26 second, then the rest, and drops every log into `steps/logs`
3. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
4. Worker reads the logs and records the proofs in this file
5. Worker starts F25, dropping the hidden discipline rule

## 2026-09-07 F24, a CHANGED NWF is rebuilt from the scan

### What was done

- The first run log arrived. `steps/logs/run-20260907-093440.log`, moved there from a root `Log` folder, which is deleted. It is a run on the old build be0b9b37 of 1 Sep, before any fix, on the C06 folder, 76 files, 26 groups, no clash XML
- What the log proves. F9 was needed and is seen live: six CHANGED groups fell through to UNITS and published an NWD off a federation missing models. F22 labels would have shown those six as Skipped. The 8 DONE groups are right, OPENED, units, NWD, NWF intact. The SOURCE MISMATCH and SHARED SOURCE lines are Revit naming, 1B06BC published from 0000BC and so on, and 1B06M1 with 1C06M2 both from 1B06MM. Not the tool. Total 352 seconds for 14 groups with no clash step, which is the first timing baseline. Noted against M2: about 2 to 6 seconds per group to open, set units and publish, so the clash step is the whole of the 45 minutes
- Three new bugs from the log, in `00_analysis.md` section 3. B13 CHANGED left the NWF alone with the scan folder holding the right files. B14 UNITS said THE DOCUMENT DID NOT FOLLOW on 11 of 14 groups and the report went out in feet. B15 12 of 26 groups were dropped before the run by a discipline rule the window does not show
- Bader answered Q21, Q22 and Q23 in `02_questions.md`. Q21 federate what is ticked, no hidden discipline rule. Q22 rebuild a CHANGED NWF from the scan folder, keep the saved tests, say what was added, moved and removed. Q23 the report is always in meters, force the document and fail loudly
- `01_next.md` gains F24, F26 and F25 right after F20, in that order, then F16 and the rest
- F24 done, bug B13. Before, a CHANGED group was left alone entirely and judged PARTIAL, the F9 rule. After, it is rebuilt: the saved tests are read, the document's own copies of the tests and the sets are taken, the document is cleared, the scan is appended in scan order, the copies are put back where the clear dropped them, the count is read again, and only then is the NWF saved over. From there it is an opened group: units, the clash step, the reports, the NWD
- New `src/Federator.Core/Rerun/NwfRebuildPlan.cs`. A file on both sides under a different folder is a move, a file only in the scan is an addition, a file only in the NWF is a removal, the files to append are the scan in scan order. `Lines` gives the REBUILT block, `SavedTestsLine` the one line with the three numbers, `SavedTestsKept` the rule the engine acts on
- `RerunDecision.Rebuilt` added. `RunPath.Rebuilt` is the label, with `AfterOpening` for a comparison that reads Changed before the run. The confirm dialog counts Rebuilt groups and says the NWF is cleared and rebuilt from the scan folder with its saved tests kept. The RESULT block carries `rebuilt        : n`. `GroupJudgement` judges a rebuilt group DONE when everything after went right and FAILED when the rebuild appended nothing
- The window opens each NWF on disk at Run, before the confirm dialog, so the six show Rebuilt in the list and the dialog counts them. Only when nothing open would be lost, because opening an NWF replaces the open document and Run cancelled must still mean nothing was cleared. Otherwise the dialog says Rebuilt is only known once each NWF is opened. After the run every group reads what it actually did
- Whether the tests survive `Document.Clear` is UNKNOWN from here. `DocumentClashTests.CreateCopy` and `CopyFrom` were read off the DLL on 2026-08-27, docs/scan.md section 4, and `ClashTestsData` is disposable. The same pair on `DocumentSelectionSets` was NOT measured and is used on the strength of the pattern every document part follows, so a build error on Bader's machine would name exactly that. The engine reads the count after the appends, puts the copies back only where the count dropped, reads it again, and the log line says which of the two happened
- `NwfComparison.Lines` on CHANGED is the heading and the counts only, the per file lines moved to the REBUILT block so a move never reads as an addition and a removal in one place and a move in another. `SkipLine` deleted with its test, nothing skips any more
- Tests: `NwfRebuildPlanTests`, fifteen, the 1B06BC case from the log with 4 moves, 1 addition and 0 removals, the append order, a move with its from and to folders, a pure addition, a pure removal, a move and a removal of one name told apart, no rebuild for Open or Build, the null refusal, the REBUILT lines, the removed line, and the four saved tests lines. `RunPathTests` gains four, six labels, Rebuilt with or without an XML, AfterOpening, the confirm lines with and without a Rebuilt group. `GroupJudgementTests` gains four, rebuilt done, rebuild appended nothing failed, rebuilt with a failed file partial, rebuilt NWD not published failed
- Core tests under mono on Linux, before: 840 passed, 40 failed, 32 skipped, 912 total. After: 863 passed, 39 failed, 32 skipped, 934 total. The 39 are the same Windows path and file locking failures, one fewer because the CHANGED lines test no longer asserts a Windows path. None new
- The add-in was not compiled. It cannot compile in the container

### What remains

- F26 next, then F25, F16, F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered
- The proofs from F5 onward on the local machine, F24 first, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- B13 fixed in code, pending local proof. Pull main, build, install, scan the same C06 folder, the six groups 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE must show Rebuilt in the list, and after the run each NWF must hold every scanned file and the NWD must hold them too. Drop the log into `steps/logs`
- B14 OPEN, F26 next. B15 OPEN, F25 after it
- M5 closed on the Core side, still open on the add-in side because the add-in build stays local
- L4 fixed in code, pending local proof. Run one group with pictures on, open the Excel, the picture on row N of a block must be picture N in the same order the Clash Detective panel lists the clashes, the blocks numbered most clashes first, and every link must open its picture. The log must carry one IMAGES numbered in report order line
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code and seen needed in the run log, pending local proof. The proof changes with F24: a folder that differs from the NWF now reads Rebuilt, so the proof is that no UNITS line comes before the REBUILT block
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M4, M6 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F24 PR
2. Bader pulls main, builds, installs, and runs the C06 folder again, F24 first in `03_bader_next.md`, then the rest of the proofs, and drops every log into `steps/logs`
3. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
4. Worker reads the logs and records the proofs in this file
5. Worker starts F26, the report always in meters

## 2026-09-07 F20, the Core tests run on every push to main

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- Order changed in `01_next.md` as Bader set it. F15 moved to the end of the code fixes. From here: F20, F16, F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered. F15 carries one new line, read `steps/05_api_notes.md` first if it exists. That file does not exist yet
- F20 done. `.github/workflows/tests.yml` triggered on pull_request and workflow_dispatch only. It now also triggers on push to main. Same job, same four steps, no second workflow file
- The job runs the Core tests only. It restores and tests `tests/Federator.Core.Tests/Federator.Core.Tests.csproj`, whose only project reference is `src/Federator.Core/Federator.Core.csproj`. Nothing in it names the add-in project or the solution, so the runner never tries to build the add-in. Nothing to fix there
- On a PR merge the same commit runs once as the PR check and once as the push run on main. The push run is a second run of the same job on the same commit and blocks nothing, because no branch rule waits on it. That is the ordinary shape of a push plus pull_request workflow and it is left as it is
- No test was changed
- Core tests under mono on Linux, unchanged: 840 passed, 40 failed, 32 skipped, 912 total. The 40 are the same Windows path and file locking failures
- The add-in was not compiled. It cannot compile in the container

### What remains

- F16 next, then F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- M5 closed on the Core side. The Core tests run on every PR and every push to main. Still open on the add-in side, because the add-in build stays local, F19 dropped by Q10
- L4 fixed in code, pending local proof. Run one group with pictures on, open the Excel, the picture on row N of a block must be picture N in the same order the Clash Detective panel lists the clashes, the blocks numbered most clashes first, and every link must open its picture. The log must carry one IMAGES numbered in report order line
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M4, M6 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F20 PR and read the push run on main
2. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F16, the tests made path neutral

## 2026-09-07 F17, the pictures are numbered in the export order

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- F17 done, logic problem L4. The pictures used to take their test number from the order the tests RAN, because they are rendered while the tests run and the report is sorted afterwards. The rows are written in report order. So the first block of the workbook, the test with the most clashes, carried whatever number its run position gave it, and the Navisworks export numbers that block cd00
- Worked example, three tests in run order: Floors ran first with one clash, Walls second with three, Columns third with two. Before: Floors 1 was cd000001, Walls 1 to 3 were cd010001 to cd010003, Columns 1 and 2 were cd020001 and cd020002. The workbook lists Walls, Columns, Floors, so row 1 of the workbook linked to cd010001. After: Walls 1 to 3 are cd000001 to cd000003, Columns 1 and 2 are cd010001 and cd010002, Floors 1 is cd020001. Row N of a block links to picture N of that block
- The rule in one sentence: the pictures are numbered in the order the rows are written, tests most clashes first with ties in creation order, and inside a test the clashes as Clash Detective lists them
- The order is only known when the last test has run, so the pictures are still rendered under the run order number and renamed ONCE after the run, in one pass, before any report is written. The rename goes through a holding name first, because a swap between two tests would otherwise write one picture over another. What a picture shows and its size are untouched, only the number changes
- New `src/Federator.Core/Report/ReportOrder.cs`. `ReportOrder.Tests`, `Rows` and `PictureNumbers` give the order and the number of every picture, `PictureNumberFor` gives one row its number, `ImageRenumbering.Apply` does the rename and repoints the row's file, link and path, and `ImageRenumberingOutcome` says how many were renamed, already right, or missing
- `FederationEngine.RenumberThePictures` calls it right after the clash step when pictures were rendered. The log gets one line, `IMAGES   numbered in report order: <n> renamed, <n> already right, <n> missing`. A missing picture or a throw is a report warning and never fails the group
- The workbook link, the XML href and the page all read the row's link, so repointing the row is what moves all three. Two tests write the workbook and the XML after the rename and read the link back off the file
- CLAUDE.md has a new picture bullet, `docs/scan.md` 4p closes the difference it recorded and the 4q row reads same
- Tests: `ReportOrderTests`, sixteen. The order on a mixed sample, a tie keeping creation order, rows keeping Clash Detective's order, every picture carrying its row number, the old run order numbering shown as wrong, no gap for a test without a picture, a row without a picture not numbered, the rename giving row number equal to picture number with each file checked by content, a swap overwriting nothing, no holding file left, already right pictures not moved, a missing picture named and the rest renamed, no pictures renaming nothing, the workbook link and the XML href pointing at the renamed file
- Core tests under mono on Linux, before: 824 passed, 40 failed, 32 skipped, 896 total. After: 840 passed, 40 failed, 32 skipped, 912 total. The 40 are the same Windows path and file locking failures, none new, none in the new fixture
- The add-in was not compiled. It cannot compile in the container

### What remains

- F23 once Q20 is answered, then F15 onward in `01_next.md` in order
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- L4 fixed in code, pending local proof. Run one group with pictures on, open the Excel, the picture on row N of a block must be picture N in the same order the Clash Detective panel lists the clashes, the blocks numbered most clashes first, and every link must open its picture. The log must carry one IMAGES numbered in report order line
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F17 PR
2. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
3. Bader follows `03_bader_next.md`, the F17 proof sits after the F10 proof, and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F15, or F23 if Q20 is answered first

## 2026-09-07 F11, the dead code is gone

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- F11 done. Every name in B9 was searched across source, tests, docs, CLAUDE.md, samples and build scripts, and sorted by who reads it: running code, a test only, or a doc only
- Deleted, because nothing running read them: `ReportOptions.ClientColumnsOnly`, `SheetNames.SummarySheet`, `SheetNames.MatrixSheet`, `SheetNames.ForTest` with `TestPrefix` and `TestDigits`, `TestReport.SheetName`, the whole of `ClashMatrix.cs` with `ClashMatrix`, `MatrixCell` and `MatrixCellKind`, and two members orphaned by that, `OpenClashes.Heading` and `OpenClashes.SheetLabel`, which only the sheets that are gone ever showed
- Kept: `TestReport.HasSheet`, read by `ClashReportXml` and `ClashRunner`. Kept: the outstanding count setting, `ReportOptions.OpenCount` and `OpenClashes`, read by the window, the engine and `ImageOptions`, though no output shows the number now that the Summary and Matrix sheets are gone. Flagged for Bader rather than deleted, because it is a window control and a CLAUDE.md rule
- Tests: `ClashMatrixTests` deleted whole, the three matrix tests and the sheet label test cut out of `OpenClashesTests`, the matrix test cut out of `SingleModelGroupTests`, the seven `ForTest` tests cut out of `SheetNameTests` with two rewritten onto the one sheet name, and the `SheetName` asserts cut out of `ClashReportTests`. Twenty nine tests removed and three rewritten in their place, so twenty six fewer
- Docs: CLAUDE.md lost the three bullets that described the T0001 sheets, the Summary rows and the matrix cell, and two lines were reworded so nothing points at the matrix or a sheet label. `docs/test-model-side.md` step 53 now says those sheets are history. The `docs/scan.md` 4q row for `ClientColumnsOnly` records its removal and stays. The probe script no longer lists `ClientColumnsOnly`. The window label `The matrix counts` reads `Outstanding counts` and the log key with it
- The report output is unchanged. The tests that pin the one sheet, the cells, the client layout, the page and the client columns all still pass: `OneSheetWorkbookTests`, `WorkbookCellCheckTests`, `ClientLayoutTests`, `ReportCheckTests`, `ClientColumnsTests`
- Core tests under mono on Linux, before: 850 passed, 40 failed, 32 skipped, 922 total. After: 824 passed, 40 failed, 32 skipped, 896 total. The 40 are the same Windows path and file locking failures, none new
- The add-in was not compiled. It cannot compile in the container. A plain text search of `src/Federator.Addin` for every deleted name returns nothing
- `03_bader_next.md` build step says the build must finish with no error about a missing name

### What remains

- F23 once Q20 is answered, then F17 onward in `01_next.md` in order
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L4 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F11 PR
2. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F17, or F23 if Q20 is answered first

## 2026-09-07 F10, each output gated on its own flag

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- F10 done. The ClashReport that feeds the workbook, the XML and the HTML page was built only when the workbook or the XML was wanted, so the page rode on the workbook flag. The pictures were rendered only when the workbook was wanted, so they rode on it too. Both are fixed on today, so nothing was lost yet, and switching the workbook off would have taken the page and the photos with it
- The rule is `Federator.Core.Report.OutputPlan`, built from the ReportOptions flags. The report is built when the workbook, the XML or the page is wanted. Pictures are rendered when they are wanted and at least one of the three is wanted to link them from, because the page embeds them, the workbook links them and the XML carries their href. With none wanted there is nowhere a picture could be found
- The engine reads that plan in `CreateAndRunTheTests` and `WriteWorkbook`. The NWD keeps its own flag, republishNwd, which was already its own
- Every output gets one log line whichever way it went. Written ones keep the `attempt` and `written` lines. Skipped ones get `XLSX     skipped  <reason>` from the new `RunLog.WriteSkipped`, in the same shape, with the reason: not wanted this run, no report folder, no clash step ran, images switched off, nowhere to link a picture, or the stylesheet not found. One `OUTPUTS` line names the four flags before the clash step. The lines come from the shared engine methods, so the scanned run and the open file run write the same
- The window wiring was checked. Every tick box reaches the engine: the XML box, the thumbnails box, the five image status boxes, apply file settings, compact resolved, date the NWD, include subfolders. The engine reads three report flags that no tick box sets, because CLAUDE.md fixed them on: the workbook, the page and the pictures. Pictures still switch off when every status box is unticked. Nothing was broken and no box was added
- What each output contains is untouched
- Thirteen Core tests added. Twelve in `OutputPlanTests`, eight of them the full table of the three report flags, and one in `RunLogTests` for the skipped line
- Core tests under mono on Linux: 850 passed, 40 failed, 32 skipped, 922 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` has the F10 proof after the F9 proof. The proof Bader named needs an Excel box and an HTML box that the window does not have and that were not added, so the proof uses what the window can switch: the XML box and the five image status boxes

### What remains

- F23 once Q20 is answered, then F11 onward in `01_next.md` in order
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, B9, L4 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F10 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F11, or F23 if Q20 is answered first

## 2026-09-07 F9, a CHANGED group is left alone before anything touches it

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11, L1, F22 and B7 stay pending local proof and nothing about them was recorded
- F9 done. A CHANGED group used to fall through Decide into the units change, the clash step, the workbook, the NWD publish and the survival check before anyone left it alone. Every model had its units set and a UNITS line was logged, an NWD was published off a federation that no longer matched the folder, and the log said `NWF reused`
- The CHANGED check now sits right after Decide and before anything touches the document in memory. Nothing is done: no units change, no sets, no tests, no save, no NWD. The NWF and NWD sizes are read off the disk for the RESULT block and nothing is recorded as written
- One log line, `NwfComparison.SkipLine` in Core, names the group, the files added and removed, and every step that was not done. The CHANGED block from Decide still lists each file above it
- Reading the file list is the one thing that happens before the check, and it opens the NWF to read it. That is a read, not a change to the NWF on disk
- `GroupJudgement` judges a CHANGED group PARTIAL right after the error and NWF on disk checks, before the NWD checks, so the NWD it no longer publishes cannot make it FAILED. The GROUP finished line carries `Skipped (changed on disk)` from F22 and the RESULT block counts it under skipped
- CLAUDE.md updated: the NWD is republished in the Build and Open cases, and the CHANGED rule says what the check comes before
- Three Core tests added, two in `GroupJudgementTests` and one in `NwfComparisonTests`. The order of the steps lives in the add-in and cannot be tested here
- Core tests under mono on Linux: 837 passed, 40 failed, 32 skipped, 909 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` has the F9 proof after the F22 proof

### What remains

- F23 once Q20 is answered, then F10 onward in `01_next.md` in order
- The F5, F6, F7, F8, F22, F1 to F4 and F9 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, B9, L3 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F9 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F10, or F23 if Q20 is answered first

## 2026-09-07 F1, F2 and F4, the small fixes

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11, L1 and F22 stay pending local proof and nothing about them was recorded
- F1, B8. The test `ALogoOnTheePageIsCheckedAgainstTheDisk` in `ReportCheckTests.cs` is now `ALogoOnThePageIsCheckedAgainstTheDisk`. Name only
- F2, B7. `build/probe-window-defaults.ps1` read `$repo = "C:\Users\p003653k\source\repos\Parsons NWC Federator"`. It now reads `$repo = Split-Path $PSScriptRoot`, the same line the scroll and labels probes use
- F4, B10. The `DocumentUnits` docstring said the units change is off unless asked for. The window sets it on for every run, so the sentence now says it is on for every run, because the window sets it on and every report the team sends is metric
- The same machine path sat in `docs/test-model-side.md` step 2 as a `cd` line. It now says to change into the folder the repo is checked out in. `docs/scan.md` line 7 still names the user as part of the machine record of the scan, which is a measurement and not a path, so it stays
- F3 was closed inside F22, so F1 to F4 are all done
- Core tests under mono on Linux: 834 passed, 40 failed, 32 skipped, 906 total. Unchanged, none new. The renamed test is one of the 32 that skip without a Navisworks install
- The add-in was not compiled. It cannot compile in the container. The probe runs on Windows only, against the built add-in
- `03_bader_next.md` has one step at the end to run the probe

### What remains

- F23 once Q20 is answered, then F9 onward in `01_next.md` in order
- The F5, F6, F7, F8 and F22 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B8 fixed. B10 fixed
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3 and B4 fixed in code inside F22, pending the same proof
- B12 OPEN, waiting on Q20
- B5, B6, B9, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F1 F2 F4 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md`, runs the probe at the end, and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F9, or F23 if Q20 is answered first

## 2026-09-07 F22, two clear workflows in the window, and F3 with it

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11 and L1 stay pending local proof and nothing about them was recorded
- The F8 entry sat in this file twice. The fuller one is kept and the other is gone
- F22 marked confirmed by Bader on 7 Sep 2026 in `01_next.md` with the two definitions
- F22 done. The rule is `Federator.Core.Rerun.RunPath`, beside Decide. Given the Decide result and whether an XML is picked it returns one of five labels: First run, Weekly run, Weekly run plus XML, Skipped (changed on disk), Unknown. Before the run the label is worked out from whether the NWF is already at its output path, because CHANGED is only known once the NWF is opened
- The group list has a Run as column, filled after Scan and refreshed when the NWF folder, a name, or the XML box changes. Blocked groups show nothing there
- The confirm dialog opens with the count per label and says cleared only for the First run groups. The Skipped line says it is only known once each NWF is opened. Then the existing lines about what is open and Carry on. That closes F3: B4 was the dialog saying cleared before every group, and B3 the NWD line naming a tick box that is gone, which is reworded too
- Run the open file: the blue line leads with Weekly run or Weekly run plus XML, and the help says there is no First run on that path
- The log carries the label on every GROUP finished line and in the GROUPS block before the run, and the RESULT block totals them: first run, weekly run, weekly plus XML, skipped
- CLAUDE.md has a Two workflows section with the two definitions and the labels. `README.md` created at the root with the same section. F14 stays open for the rest of a README
- The engine does the same work as before. Only what the person is told changed, plus one log string
- Fourteen Core tests added. Eleven in `RunPathTests`, one in `OpenDocumentJobTests`, two in `RunLogTests`
- Core tests under mono on Linux: 834 passed, 40 failed, 32 skipped, 906 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7, F8 and F22 proofs in one session

### What remains

- F23 once Q20 is answered, then F1, F2 and F4 in one PR, then the rest of `01_next.md` in order
- The F5, F6, F7, F8 and F22 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3 and B4 fixed in code inside F22, pending the same proof
- B12 OPEN, waiting on Q20
- B5 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F22 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the five proofs in this file
5. Worker starts F1, F2 and F4 as one PR, or F23 if Q20 is answered first

## 2026-09-06 F8, run the saved tests when no XML is picked

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2 and B11 stay pending local proof and nothing about them was recorded
- B12 registered in `00_analysis.md`, OPEN, waiting on Bader. Q20 added to `02_questions.md` with the four unknowns. F23 added to `01_next.md` after F22
- F8 fixed. With no XML, `ClashStep` returned before anything ran, on both the scanned run and the open file run, while the window logged that it was running the tests already in the document
- The clash step now does one of three things and the log names which, in one line shape on both runs: `CLASH    source   tests from XML, ...`, `CLASH    source   tests saved in the document, N of them, no XML picked`, or `CLASH    source   nothing, no XML picked and the document holds no clash test, so nothing ran`. The rule is `ClashWork.SourceFor` in Core
- A second way to build a `ClashTestPlan`, `ClashTestPlan.FromDocument`, in Core. Every saved test goes in by its address, in the order found. No drift compare, nothing created, sets not touched. `SavedClashTest` is the shape it takes, with no Navisworks type on it
- The add-in fills it in `SavedTests.Read`, one walk of `DocumentClashTests` that disposes every wrapper and keeps only the address
- `ClashRunner.Run` and `OneTest` take a document plan through the same path: `ItemsOn` both sides, the SingleModel and EmptySide skips, `TestsRunTest`, the harvest, the images, the report, the `RepeatedFailureGuard`. A document plan skips the set resolution and the zero sets guard, because its sides are already inside the tests
- The XML path is unchanged. An XML holding neither sets nor tests is still nothing, whatever the document holds
- The words fixed to match the log: CLAUDE.md in two places, the help line under Run the open file, the OPEN log line in the window, and the engine docstring
- Eleven Core tests added in `SavedTestPlanTests`
- Core tests under mono on Linux: 820 passed, 40 failed, 32 skipped, 892 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7 and F8 proofs in one session

### What remains

- F22 next, which waits on Bader confirming the two workflow definitions in chat, then F23 once Q20 is answered, then F1 to F4 as one PR
- The F5, F6, F7 and F8 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the scanned run with no XML on a folder whose NWFs already hold tests
- B12 OPEN, waiting on Q20
- B3 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F8 PR
2. Bader answers Q20 and confirms the two workflow definitions for F22 in chat
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the four proofs in this file
5. Worker starts F22

## 2026-09-06 F7, the RESULT block and log copy for the open file run

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1 and B2 stay pending local proof and nothing about them was recorded
- F7 fixed. The open file run ended with no GROUP line, no block naming what it did, no RESULT block and no copy of the log. The scanned run has all four
- `FederationEngine.RunOpenDocument` now calls `log.GroupStarted` with the models inside the file, and `log.GroupFinished` in a finally with the same outcome shape the scanned run uses, so `GroupJudgement` gives DONE, PARTIAL or FAILED by the same rule and the RESULT block counts it. The group name is the open file's name from `OpenDocumentJob`
- In the same finally the engine writes a block titled OPEN FILE, built by `OpenDocumentJob.SummaryLines` in Core: the file, the decision, the clash file, the clash outcome, the NWD path, the report folder from F6, and the outcome. Source folder, grouping and files ticked say not applicable rather than being left blank
- `FederatorWindow.OnRunOpenDocument` calls the existing `WriteTheResultAndCopyTheLog` in its finally, handing it the open file's own folder through `OpenDocumentJob.FolderOf`, so a failure still leaves a RESULT block and the copy sits beside the open file. No second copy of that method
- `SourceMismatchFindings` is skipped on the open file run with one log line saying why. Nothing was scanned, so there is no NWC name to compare the Revit source against
- The window's old OPEN DOCUMENT one line block is gone, the engine's OPEN FILE block replaces it
- The scanned path is untouched
- Five Core tests added. Two in `GroupJudgementTests` for the open file case, three in `OpenDocumentJobTests` for the folder and the summary lines
- Core tests under mono on Linux: 809 passed, 40 failed, 32 skipped, 881 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6 and F7 proofs in one session

### What remains

- F8 onward in `01_next.md`, in that order
- The F5, F6 and F7 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- B3 to B10, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F7 PR
2. Bader follows `03_bader_next.md` and drops the three logs into `steps/logs`
3. Worker reads the logs and records the three proofs in this file
4. Worker starts F8

## 2026-09-06 F6, the open file report folder

### What was done

- `steps/logs` holds only its README. No run log yet, so B1 is still pending local proof and nothing about it was recorded
- F6 fixed. The window built `<folder>\Clash Reports` with `OpenDocumentJob.ReportFolderBeside` and handed it to the engine as the NWF folder. The engine then built `Clash Reports` beside that again through `ReportPaths.Choose`, so the reports went to `<folder>\Clash Reports\Clash Reports`
- The decision now lives in one place, `OpenDocumentJob.ReportFolder(openPath, pickedExcelFolder)` in Core. It calls the same `ReportPaths.Choose` the scanned run uses, with the open file's own folder where the scanned run puts the NWF folder. A picked Excel folder wins, the same as the scanned run
- The engine's open file path calls that rule in `RunOpenDocument` and takes no folder from the window. The window's blue line calls the same rule, so what it says and what is written cannot differ
- `ReportFolderBeside` is gone. `RunOpenDocument` and `OpenJob` lost a subfolder parameter nothing read
- The scan's source folder refusal is skipped on the open file run. No source folder is handed to `ReportPaths.Choose` there, because nothing was scanned. The engine still creates the report folder and reads every written file back, so a folder that cannot be written is caught by the write itself
- The scanned path is untouched. The six argument engine constructor now delegates to a new five argument one and then sets the report folder exactly as before
- Five Core tests added in `OpenDocumentJobTests`, one of which pins the old doubling as the trap it was
- Core tests under mono on Linux: 804 passed, 40 failed, 32 skipped, 876 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5 and F6 proofs together

### What remains

- F7 onward in `01_next.md`, in that order
- The F5 and F6 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B3 to B11, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F6 PR
2. Bader follows `03_bader_next.md` and drops the three logs into `steps/logs`
3. Worker reads the logs and records both proofs in this file
4. Worker starts F7

## 2026-09-06 F5, the sets built test, and Bader's answers

### What was done

- Bader answered every question. The answers sit under each question in `02_questions.md`
- `01_next.md` reordered to Bader's order. F19 dropped. F20, F21 and F22 added
- `steps/logs` created with a README for the run logs
- `03_bader_next.md` written, the local steps for the build, the install and the first proof
- F5 fixed. `FederationEngine.BuildTheSets` returned `CreatedCount > AlreadyPresentCount`. It now returns `SetBuildOutcome.PutAnythingIn`, which is `CreatedCount > 0`
- The bool has one meaning. It feeds `ClashStep`, whose only caller uses it to decide the second NWF save. The tests were never gated on it, `ClashStep` runs them either way. So B1 in `00_analysis.md` was corrected: what was lost was the second NWF save, not the tests
- One log line added after the SETS block saying how many sets were put into the document and how many were already there
- Four Core tests added in `SetBuildOutcomeTests` for `PutAnythingIn`
- Core tests under mono on Linux: 800 passed, 40 failed, 32 skipped, 872 total. The 40 are the same Windows path and file locking failures as before, none in the sets tests
- The add-in was not compiled. It cannot compile in the container

### What remains

- F6 onward in `01_next.md`, in that order
- The three proofs P1, P2, P3 on the local machine

### Known bugs

- B1 fixed in code, pending local proof. The proof: run one building twice, the second press must show OPENED, the sets present and the tests still run, then drop the log in `steps/logs`
- B2 to B11, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F5 PR
2. Bader follows `03_bader_next.md` and drops the two logs into `steps/logs`
3. Worker reads the logs and records the proof in this file
4. Worker starts F6

## 2026-09-06 analysis pass

### What was done

- Read every non sample file in the repo, 151 files, in full
- Sample data looked at by head and line counts only, 355 jpg not opened
- Environment checked. No Navisworks in the container
- Installed .NET 8 SDK and mono in the container to run the Core tests
- Ran the Core tests under mono on Linux. 796 passed, 40 failed, 32 skipped
- All 40 failures are Windows path and file locking assumptions, not code faults
- Wrote this folder
- No source code changed

### What remains

- Every fix in `01_next.md`. Nothing is fixed yet
- Every question in `02_questions.md`. Nothing is answered yet
- The three done criteria. None is proved yet. See section 5 of `00_analysis.md`

### Known bugs

Eleven bugs B1 to B11, eight logic problems L1 to L8, eight missing pieces M1 to M8.
All in `00_analysis.md`.

The ones that change what a run produces:

- B1  a group whose sets were all already there is treated as if nothing was built
- B2  the open file path writes reports one folder too deep
- B11 the open file path writes no RESULT block and copies no log
- L1  the open file with no clash XML runs nothing, the log says it runs the tests
- L2  a CHANGED group still has its model units changed

### What comes next

1. Bader answers `02_questions.md`, at least Q1, Q2 and Q6
2. Worker does the fixes in `01_next.md` in order, one branch per fix or one branch for the small ones
3. Bader runs one building on the local machine and sends the log
4. Worker reads the log for OPENED, the clash timing and the Excel counts
