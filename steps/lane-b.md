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
| 2 | F127 the coverage sheet of Bader's request 2, FR-176 and FR-200, carried on from dccf351 | fix-F127 | Core steps 1 to 3 merged as pull request 145, the Coverage sheet, the check's sheet list, the Q127 lines and the records as 153 by the worktree session, the Core points of its readers as 167, the add-in half waits for the laptop lane |
| 3 | F137 no site and no clash groups end PARTIAL, FR-195, Q111 B and Q125 B | fix-F137 | part 1 merged as pull request 146, its records by the worktree session on fix-F137, a reference model that names no site or Internal said in the ALIGNMENT block as 173, Q125 B left for the laptop lane with the add-in half |
| 4 | F118 the workbook and report, FR-035, FR-036, FR-037, FR-040, FR-041 and FR-199 | fix-F118 | FR-035 and FR-037 merged as pull request 147, FR-199 and FR-040 with the records as 159 by the worktree session, FR-036 and FR-041 left for the laptop lane |
| 5 | F119 the run log and RESULT, FR-043 to FR-057 and FR-189 | fix-F119 | nine items merged as pull request 148 with FR-057 half, two more points as 168 and FR-057's disk half as 169, the rest left for the laptop lane |
| 6 | F128's Core part, generic models, FR-177 | fix-F128 | Core part merged as pull request 171 on Bader's order of 8 Oct, the add-in half and the measurement of the property wait for the laptop lane, see F128 below |
| 7 | F121 the rest, FR-150 to FR-166 and FR-202, wave 4 | fix-F121 | five items merged as pull request 150, the SINGLE DISCIPLINE finding of the groups gathered by discipline as 173, the rest left for the laptop lane |
| 8 | F123 docs and words and the noise of every area, wave 5 | fix-F123 | merged as 151, 154, 155, 161, 162, 163, 164 and 165 |

## The item the lane is on

The second night, 8 Oct 2026, by Bader's message GOOD MORNING, BADER: the Core points the readers of F127,
F119, F115, F137, F121 and F123 left, in that order after F128. F127's Core points merged as pull request 167, F119's first two as 168, FR-057's disk half of F119 as 169, F115's words as 170, F128's Core part, taken first by his second message of the morning, as 171, F115's R10 and R14 as 172, F137's and F121's as 173, F123's FR-171 entries as 174 and the small points a claim check found still open, the one row test order, Words.Counted and one comment, as 176.
FR-061's side, a failed write to the .tsv, merged as 178, FR-049, a second run in a window carrying the
first run's totals, as 179, and the FR-171 entries that fix-F132's merge as 177 freed as 180. Now on FR-038, FR-039
and FR-074, which the same merge freed. Branch claude/lane-b-release-plan-zztyvx, which for them changes
src\Federator.Core\Report and Views files and the tests beside them, listed in the row of pull request 181.
No open branch changes a file under src or tests, read with git diff origin/main...origin/branch against every branch that is open on 8 Oct 2026.

The branch claude/lane-b-release-plan-zztyvx is the one branch this session may push, restarted
from main after each merge. F115 merged as pull request 142, F127 as 145, F137 as 146, F118 as 147 and
F119 as 148.

A second session of the lane, the worktree session in .claude\worktrees\agent-a9ff34180e9235316
of the checkout on Bader's machine, writes the records the cloud session may not: F115's records
and two fixes of the breaker's third reading on fix-F115, pull request 144, merged as 86cc405,
then F127's Coverage sheet with its records on fix-F127, pull request 153, pushed once GitHub
stopped refusing every push with an internal server error, from 15:12 to 15:19 on 2026-10-07,
merged as a845fbb, then the records of F137, F118 and F119 each on its own branch, fix-F137 first.
fix-F115 and fix-F127 are deleted, their records on main.

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

Read with git diff --name-only on origin/main against each branch on 8 Oct 2026, the files under src and
tests only. Every branch of this lane also changes steps\lane-b.md, steps\tracker.csv, steps\tracker.md,
steps\PROGRESS.md, steps\01_next.md, steps\history\log.md and the rule in .claude\rules\core.md
where a rule changed. The laptop lane merges those files by taking main in, as every branch does.

- fix-F118 merged as pull request 159 on 8 Oct 2026 and is no longer open, so ReportOrder, ClashReportModel,
  WorkbookCheck, ClientFormat and ClientShapes are free to lane B again
- fix-F132, the mirrored tests, merged as pull request 177 on 8 Oct 2026 and is no longer open, so the files it
  changed, in Clash, Exchange, Health, Report and Sets and the add-in's ClashHarvest, ClashRunner, FederationEngine
  and SavedTests, are free to lane B again
- fix-F114, fix-F114-probes, fix-T5-close-1007 and fix-F109: no file under src or tests
- fix-F115, fix-F127, fix-F137 and fix-F119 are merged and their branches gone, and fix-F128 was never made as a branch of its own, its Core part going through the lane branch as pull request 171

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
| F121 | The two points the second reading of F121 left: a name typed by hand with a character Windows refuses, a colon, a slash, a control character, is refused by the check before a run and named, and a collision between many groups names the first five and counts the rest, as an unusable name does. The refused characters are the list FileNames holds, never the running platform's. Core tests 2400 run, 2366 passed, 0 failed, 34 skipped, and 3 failing on the old source | 160 | merged, 27c24b7 |
| F123 | FR-172 in part: two public members nothing in src or tests calls are deleted, the array of fire suppression words in ProbeVerdict, which was a second copy of a rule NamesFireSuppression holds, and LeftoverSet.TwinPath with its constructor argument. Core tests 2400 run, 2366 passed, 0 failed, 34 skipped, as before | 161 | merged, 304c585 |
| F123 | FR-171 in part, the Core noise entries of the turn 1 read that still held on main. T1-N72 the probe's grey line says one CSV per model. T1-N87 the Rebuilt confirm line names the shape it clears. T1-N69 the health line of a file with no sets says the sets are looked for and not checked. T1-N60 an identical ternary. T1-N71, N59, N53, N70, N73, N79, N67 comments that said the opposite of the code or called a constant a setting. Core tests 2402 run, 2368 passed, 0 failed, 34 skipped, and 3 failing on the old source | 162 | merged, b52e914 |
| F123 | FR-171 T1-N62, in the run timing block the by group section is labelled outside every group and says every second of the run is inside a group, or that the groups add up to more than the run took, where it said step. The by step section and the group block keep their words. The tests do not build against the old source, since the new label is a new member, so the old source was not run against them. Core tests 2404 run, 2370 passed, 0 failed, 34 skipped | 163 | merged, de2e558 |
| F123 | FR-171 T1-N89, the edit distance written line for line in the EMPTY SETS judge and in the workset disagreements is one routine, EditDistance, and each reader hands it its own cap. Four tests on the routine, and the two readers' own tests unchanged. The new tests do not build against the old source. Core tests 2408 run, 2374 passed, 0 failed, 34 skipped | 164 | merged, b99c736 |
| F123 | FR-171 T1-N56 and N57 in RunLog. The numbered line writes its machine readable row through Row, which it had copied line for line, and the seconds, visits and throws of a step in a group are added up in one place, TotalOf, where two loops did it. No behaviour changes, and one test added after the reader found the step sums unpinned at log level, a repeated step with one visit that threw. Core tests 2409 run, 2375 passed, 0 failed, 34 skipped | 165 | merged, 04e68b8 |
| F127 | The Core points the readers of 145 and 153 left. SetsAcrossTheRun counts a path once in a group, finds something there where any set of the path did, is at zero only where every set of the path was counted and found nothing, and leaves a path out of a group where one set of it was not counted and none found items. A set's line and its .tsv row say how many of the groups it was looked at in when that is fewer, the header says how many sets that is, a set never counted in any group is counted on a line of its own, and the all clear says every set that was counted, or that no set was counted. CountCheck reads a test the run left no record of, or never reached, with an empty block that the document does not return as not compared, where one the run knows it did not create stays held by neither side, and gives the FAILED line of a test the plan dropped before the model, that an earlier run left in the document, its cause. CoverageAcrossTheRun counts the tests the file does not name once by name across the run, as Q127 A counts every other test, and the headline says how many test places its groups hold and that a group not checked is in none of the counts. Words: the CountCheck summary, the Q126 test comment, one number tied to set 03's log line 297, and the design file said to be outside the repo. 10 new tests, all failing on main's source. Core tests here 2408 run, 2375 passed, 0 failed, 33 skipped before, 2418 run, 2385 passed, 0 failed, 33 skipped after. A reviewer and a breaker read it and their findings are fixed, two of lane B's own first lines among them: a cause that said a test the runner's walk had not found was in the document before the run, and a not compared rule that took in the tests the plan drops. The add-in half of F127 is unchanged | 167 | merged, 639804a |
| F119 | The two Core points the readers of 148 left that a test proves. The RETAIN line counted a .tsv it could not delete in the number of logs it could not delete, so two refused .tsv read as two logs that stayed. It now counts them apart, on the one seam that writes the sentence, RunLog.RetainLine, and adds nothing about a refused .tsv unless one was refused, so the part the loop's harness matches is unchanged, and the wiring is held by a Windows only test that holds a .tsv open. A run counted to now, one that started and never finished, printed an after the run finished row of 0.0 seconds in its timing block, which reads as a measurement. It now says the time after the run is not measured and prints no row, and a run that finished keeps its row. 3 new tests, the first not building against main's source because RetainLine is new, the second failing on it, and the third, which holds PruneOldLogs's wiring with a .tsv held open, running on Windows only and so unrun here. FR-057's disk half is not in this pull request. Core tests 2418 run, 2385 passed, 0 failed, 33 skipped before, 2421 run, 2387 passed, 0 failed, 34 skipped after, the one more skipped being the Windows only test | 168 | merged, a3681a3 |
| F119 | FR-057's disk half, second attempt. A write to the log file that threw, a full disk or a handle gone, came out of Line and stopped the run, and the failure lines that would have reported it hit the same write. Now the write is in a try in WriteRaw. The line is kept in memory and told to the window, one LOG line says the file stopped taking lines with what threw, IsWritingToDisk reads false and the label says the lines since then are in the window only without a framework message, and the run goes on. ReadAll gives the lines held in memory, TryCopyTo writes a copy from memory and says so, the RESULT size of the .log says the file is short, and Dispose closes the writer and the stream each on its own so a flush that fails leaves no handle open. A first line that cannot be written counts as a failed open, so StartOrDisabled tries the next folder as it did when the write threw out of Line, with the cause in the message and the empty file taken away, which no test shows, since a disk will not fail on the first line on demand. A copy of a log that was only closed now works, where main refused it. 5 new tests, made with a closed stream through the one private field and, for Dispose, a stream whose flush throws, all failing on main's source. Core tests 2421 run, 2387 passed, 0 failed, 34 skipped before, 2426 run, 2392 passed, 0 failed, 34 skipped after. One reviewer read it twice over and its findings are in | 169 | merged, 91fd78d |
| F115 | Words and polish the readers of 142 and 144 left, no behaviour changed. The Asked property of SetResult carried two summary elements in a row and now one. The comment in EmptySets that called the measured lists the whole project, 374 categories and 39 worksets, says the judge adds the worksets this group's models carry and the spellings of the list beside the picked file. Nine assertions that set a ReadOnlyCollection against an array or a list, in EmptySetsTests, SetBuildPlanTests, InfraSetsFileTests and MatrixCorrectionsTests, copy it into a list first, as the same files already do, so that the lists are compared element by element as the rules of the tests ask, and compare as strictly as before. No new test, since no behaviour moves and the sets tests hold it. Core tests 2426 run, 2392 passed, 0 failed, 34 skipped, as before | 170 | merged, 16d1e55 |
| F128 | The Core part of FR-177, generic models, taken first by Bader's order of 8 Oct 2026 over the blocked judgement of the night before. GenericModelsSettings holds the value of the Category property, the folder and the sheet name as settings with their defaults, each refused where it is set if it cannot work. GenericModelsPlan.For gives one planned set for each model of a group, named after the model's file, in the folder Generic Models, with the two conditions the client's own file writes, the category equals the value and the Source File contains the model's text, in one group so they are ANDed, and notes what it noticed, a model with no name, two models of one name and a text that finds another model's items, and changes none of it. ToBuildPlan hands the sets to the plan the set builder already takes, through the new SetBuildPlan.Of, so nothing reads a second shape. GenericModelsReport reads the sets' results, one count for each model, a model whose count nobody took being UNKNOWN with why and never nought, the total being at least that where any was not counted and being no count of items where the texts of some sets meet, a set already in the document that asks another question than the plan's being not counted, and writes the GENERIC MODELS block's lines with a line on what a nought can mean. GenericSheet.Rows and Write are the one shape of the sheet, models with items first, then the ones not counted, none for a model at nought. The conditions are held against the client's matrix by a test, so the plan answers to that file. 35 new tests in three files, none of which builds against main's source, since every class they read is new, and a reader and a breaker read it, whose findings are fixed with 14 more tests, five of which fail when the code behind them is taken out. Core tests 2427 run, 2393 passed, 0 failed, 34 skipped before, 2476 run, 2442 passed, 0 failed, 34 skipped after. UNKNOWN until the laptop lane measures it: the value of the Category property that Generic Models items carry in 1A02MM and 1A04PK, and whether their Source File holds the NWC's name or the Revit file's. No member is called from src yet, the add-in half being the laptop lane's, and Bader's order of 8 Oct keeps them past Q26 | 171 | merged, 38f0a92 |
| F115 | R10 and the Core half of R14 of the readers' points on 142 and 144, taken on 8 Oct 2026. R14: the EMPTY SETS judge was handed the models of a group and never read whether their walks finished, so for a group of the lists' own project it called a condition on a workset wrong, no model measured so far in this project carrying it, over a model whose worksets nobody read, which may carry it. ModelExport hands back no worksets for a model whose counts were not all taken, so the judge now names such models, three at most and the rest counted, in a list of its own, ModelsNotWalked, and for a workset no other model carries says it cannot tell, with the number of models and their file names. A workset the other models carry is still carried. The category verdict is unchanged, since ModelExport holds no category. R10: the branch that says the workset list inside Core was not read had no test, since the DLL's answer cannot be changed, so the judge takes it as an argument and For hands it RevitWorksets.ResourceFound. 4 new tests, each fails when the code behind it is taken out. Core tests 2476 run, 2442 passed, 0 failed, 34 skipped before, 2480 run, 2446 passed, 0 failed, 34 skipped after. The models are named as many as a repeated line keeps, RunLog.KeptOfARepeat. A reader and a breaker read it and their findings are fixed, among them a model counted at nought elements or with no workset being read and not counted unread, which a fourth test holds. The other half of R14, a model dropped from the exports in ModelFactsReader, is the add-in's | 172 | merged, b2292ee |
| F137 | A reader's point on 146: the ALIGNMENT block measures every model from a reference, the first model of the reference discipline that could be placed, and that model can itself name Revit's own origin or no shared site, when every model that sits where the project puts it reads far from it. The block now says so on a line under the reference line and changes nothing else. The reference is chosen as it was, the models off the coordinates are the same ones, and the clash skip and the group's result do not move. The line is said only where another model was placed, since it speaks of the distances below, and a reference whose site could not be read is not said to be off. The reference is the first model of the reference discipline that could be placed, or the first that could be placed at all. 3 new tests, each failing when the code behind it is taken out, counted with the next row | 173 | merged, a125258 |
| F121 | FR-154's note, the SINGLE DISCIPLINE finding for every group gathered by discipline. A group carries its discipline code only when it was gathered by discipline, so each said a building held only one kind of file and that its other disciplines might not be exported, which gathering by discipline makes no statement about. They are one finding now, with the groups it covers, saying each group holds one discipline, that the federation of each is still built and the clash tests whose sides both find something are still created and none is run, and which Grouping choices clash the disciplines. A group gathered by building keeps a finding of its own, and the clash tests are said to be created where a clash file with tests is picked, since the scan runs before one is. FR-167 has the same root in SourceMismatchFindings and is not changed. 2 new tests, the first failing when the code is taken out. A reader read both changes and its findings are fixed. Core tests for this row and the one above 2480 run, 2446 passed, 0 failed, 34 skipped before, 2485 run, 2451 passed, 0 failed, 34 skipped after | 173 | merged, a125258 |
| F123 | FR-171 in part, the entries of the turn 1 read that fix-F118's merge freed and that were read again on main on 8 Oct 2026. T1-N76: ClientFormat.Rounded kept a value below about 1e-13 whole, since Math.Round takes no more than fifteen decimals, so a Distance cell held 3.7312345e-14 beside their 0.0000000000000373. It is now the number the three significant figure text reads as. T1-N82: the workbook check ordered the blocks by the rows under them and said the tests were in the wrong order for a block of one group row holding ten clashes before a block of three single rows, though the writer sorts by the clashes a test holds. It reads the Clashes cell of each block, the rows only where a cell is no whole number. The helper of PriorityColumnTests that puts a block out of order raises that cell with the rows. T1-N80: the pass line of the workbook check said every column, fill and border matched, though only the first block is read cell by cell and only its first clash row for the shape of its values, so it says what was read. Comments: T1-N61 TimedThing is read by RunLog and not by nothing else, T1-N58 a stranded summary on PenetrationsWanted, T1-N86 four rerun cases where the enum holds five, T1-N91 what Shows holds and T1-N92 two stacked summaries on the VIEWS block. Read again and already true or closed on main: T1-N45, N74, N88, N90, N93 and N52, whose claim that a second Install logs the new folder is false since the return comes before the line, and N78 and N81, closed by FR-199 and FR-035. The pass line names what is read, the column headings of every block with clashes, the fill, border and row height of every cell of the first block, five values of its first clash row, the widths and the title row. The Clashes cell is read in one place, WorkbookTests.ClashesOfBlock, for the test count and the order check, and a cell that is no whole number sends the order check back to the rows and says so. 4 new tests that fail on main's source or on the code taken out, and the order test of the priority fixture changed to raise the Clashes cell with the rows it copies. A reader read it and its findings are fixed. Core tests 2485 run, 2451 passed, 0 failed, 34 skipped before, 2489 run, 2455 passed, 0 failed, 34 skipped after | 174 | merged, 25d1f68 |
| F118 | The order check of WorkbookCheck read the full blocks alone, so a test with no clash placed before a test with clashes was never named as out of order, the reader's point on 147. It now says so once, with the row of the first such test and the row of the first block it stands before. The other point of that reader, the Priority heading check not run on a sheet with no full block, is not a fault, since a sheet of one row tests has no heading row for it to read. 2 new tests, the first failing when the check is taken out and the second holding that a block with no clash may follow a one row test as a tie, and the pass line of the check now says that no test with no clash stands before one with clashes, where it said the place of such a test was not read. Counted with the next row | 176 | merged, 6aa7635 |
| F127 | Words, not taken, from the readers of 145 and 153: the helper that writes a number and its noun, written out in CountCheck, CoverageAcrossTheRun, TeamMap, MatrixCorrectionList and OffCoordinatesAcrossTheRun, four of them printing the number in the running culture, which differs only in the sign of a negative one, is Words.Counted, with two tests, one handing it a culture that signs a negative with a tilde. The helpers that pick a noun and print no number are not the same function and are left. The row count loop the reader named was not taken. T1-N63 of F123 goes with it, the summaries of TimingBlock.Clock said h:mm:ss and carried a stale second one, and now say what it writes. Core tests, counted with the row above, 2489 run, 2455 passed, 0 failed, 34 skipped before, 2493 run, 2459 passed, 0 failed, 34 skipped after | 176 | merged, 6aa7635 |
| F123 | FR-061's side, what the FR-057 fix in 169 left open, FR-061 itself being merged as 151. RowLog swallowed a write to the .tsv that threw with no line and no flag, so the text log went on saying the .tsv carries every collapsed line, on a full disk. The row file now keeps the first fault, with the full stop its message ended on taken off, and stops writing. RunLog says once, with what threw, that the .tsv stopped taking rows, holds the rows before that one, may have cut the one that failed short, and that the text log is unaffected. The three sentences about the .tsv and the line of its size say that the ones counted before are in it and any after are kept nowhere and the file is short, where they said it carried every one. A sentence already written, the one the sixth repeat of a line gets, keeps what it said when it was true and the RESULT block corrects it. A header that cannot be written is a file given up on, as the first line of the text log is, so the line that names the file says there is none and why, where it would have said UNKNOWN beside a file that exists, and RowLog.Dispose runs its three steps each on its own, so a flush that fails leaves no handle open. The text log is unaffected and the run goes on. 5 new tests, each failing when the code behind it is taken out, the guard that stops it writing, the trim, the give up and the per step dispose each tried in turn. Core tests on main with pull request 177 in it 2707 run, 2673 passed, 0 failed, 34 skipped before, 2712 run, 2678 passed, 0 failed, 34 skipped after. A reader read it and its findings are fixed. Not proved here: a flush that fails in the middle of a run, with a real disk, and the half row it can leave. Left for the laptop lane, not Core: .claude\rules\core.md near line 2129 still says a write to the .tsv that throws is swallowed, and two sentences built outside RunLog still say the .tsv holds what it may not, ViewsTree near line 54, the .tsv holds every one, and PenetrationTally near line 159, each is a row in the machine readable log, neither knowing whether it opened | 178 | merged, bfd9b35 |
| F119 | FR-049, a second run in one window carrying the first run's totals. A RESULT now closes the account. After it is written, in a finally, RunLog empties what the RESULT and the timing block read, the files written, the failures and their repeat map, the group and step records, the collapsed line counts, the clash, penetration and by design totals with their wanted flags and moved counts, and the priority tally, so the next RESULT counts what was recorded since the one before. The first design cleared at RunStarted, and the reader and the breaker showed that it lost a failure of the preview in the same press and a hand button's between presses from every RESULT but the first, and left the open file run, which marks no run, holding the other run's groups, clashes and files. RunStarted now keeps only the clock and a line saying this is run N. The runs before this one are RunClock's EarlierRunsSeconds, each RUN started to RUN finished, shown as a row of their own, earlier runs, and capped at the time before this run, so waiting for the person is what is left where it held the whole first run. A run whose end was never marked adds nothing, its time stays waiting, and the line says so. The footer of the RESULT names the fourth stretch. A run starts with a whole census, where it inherited the narrowing of the one before. 15 new tests in RunLogSecondRunTests. The whole file does not build on main's source, since the clock members are new, 9 of them fail on the first design, and thirteen pieces were taken out one at a time, each failing a test. Two existing tests, one in ResultBlockInvariantTests and one in RepeatedFailureLogTests, read the lists after the RESULT and now read them before it, with every assertion as it was. Core tests on main with pull request 178 in it 2712 run, 2678 passed, 0 failed, 34 skipped before, 2727 run, 2693 passed, 0 failed, 34 skipped after. Not proved here: the finally, a block that throws part way, which the tests cannot make, and a whole window run. Left for the laptop lane, not Core: the open file run's RESULT after a Run still prints that Run's marks, so its run time is the Run's. A hand button's clash, penetration and by design tallies land in the next RESULT, a run's or not. The .tsv has no run boundary row, so a sum over it counts every run of the window. PreviewRunPaths runs before the running flag is set, so a hand button might be clicked through it. .claude\rules\core.md near line 2115 says RunClock holds three stretches, addin.md says a tally of what one run did is the engine's and never kept on the log, and neither names the account a RESULT closes | 179 | merged, f37230b |
| F123 | FR-171 in part, the Core entries of the files fix-F132 freed by its merge as 177, read again on main on 8 Oct 2026, and one polish point of the sets. T1-N75: ClashReportXml.Filled and its class comment listed createddate and date as filled, which nothing writes, on purpose, since the stylesheet writes a Date Found column for any it finds and the client's report has none. They are in LeftOut with time, which the shape gives beside them, and node, name and value, which the objectattribute and smarttag elements carry, are in Filled where they were missing. The class comment points at the two arrays and does not write them out a second time. A test reads a written XML and holds the lists against it three ways, every element written is listed as filled, none written is left out, and every one listed as filled is written, and it fails on main's source naming createddate and date and name and value. T1-N47: the helper that says whether a side's set was counted, written line for line in CreationPlan and EmptySideCost and in another shape in CoverageRule.CountOf, is PlannedClashSide.Counted, matched Ordinal and never trimmed, with five tests. T1-N64's second place: ClashReportModel reads ExchangeReader.SelectionSetTreeRoot, held by a pairing test that did not fail before since the two were equal. T1-N84: the nine cells of a test's values, written by the same seven lines in WriteEmptyTestRow and WriteTestHeader, are WriteTestValues, held by a test reading both shapes back off a written workbook. T1-N83: ClientStyle names the Priority column among what is not copied. The second ContainsTest in SetWarnings is deleted and ExportCheck reads the plan's. 8 new tests, the Counted, nine values and lists ones each failing when the code behind them is taken out, which was tried, and two mutants of Counted the reader named, a locator tested for null and a trimmed or case blind match, each failing a test. Core tests on main with pull request 179 in it 2727 run, 2693 passed, 0 failed, 34 skipped before, 2735 run, 2701 passed, 0 failed, 34 skipped after. A reader read it and its findings are fixed. Read after 177 and closed: a test whose reason is built by a mirror bypassing ReasonFor, which no mirror code does. Left: T1-N64's first place in ClashRunner and a third literal of the root in SetBuilder.cs near line 223, which are the add-in's, and T1-N77, which is Bader's | 180 | merged, 8f3af33 |
| F123 | FR-038, FR-039 and FR-074, three noise items of the report and the views that WorkbookWriter and the views plan held for the laptop lane while fix-F132 was open, read again on main after 177. FR-038: the one row of a test that found nothing borrowed the painter of the two row test header, so the row under it came out grey and boxed, and after the last block of a sheet, which in the default order is an empty test on almost every workbook, nothing repainted it. ClientStyle.TestHeader takes the number of rows, one for the one row form, and paints no more, and that form closes its own box, a thick bottom on every column, name included, where the stray row used to. FR-039: with the thumbnail box ticked the 72 point row of a pasted picture was replaced by the clash row height of 60 as soon as the row was written, so the picture overhung the next row. The height is set before the row is written and the picture's wins, ThumbnailPoints is internal so the tests read it, and the workbook check expects it on a clash row holding a picture, where it would have named every such run's row as 72 high against the client's 60. The same height on a row with no picture is still named. FR-074: the VIEWS block counted the clashes whose pair had a set name with no known code and never said which set. It names the sets, each with the clashes it was in, a clash counted under each of its sets with no code and a clash between two sets of one name counted once, in the order they were met, as many as a repeated line keeps, RunLog.KeptOfARepeat, and counts the rest. A set whose name carries a code is never named, nothing is guessed, and the folder still says UNKNOWN. DisciplinePair says which side was unknown, where the plan decided it a second time. The item says up to ten and the one number is five. 9 new tests, each failing when the code behind it is taken out, which was tried for the empty row, the thumbnail's height, the workbook check, the bottom edge, the self pair, the cap and the known code rule, and the naming ones failing on main's source. Core tests on main with pull request 180 in it 2735 run, 2701 passed, 0 failed, 34 skipped before, 2744 run, 2710 passed, 0 failed, 34 skipped after. A reader read it and its findings are fixed. Read on main and closed: FR-042, the page check warning of a wrong order on a page written in priority order, since FR-199 keeps the blocks in the measured order with a priority file picked, so no page is in priority order, and its tracker row is the loop's to set. Left for the laptop lane: the add-in writes its own sentence for each pair that hides nothing, ViewpointBuilder near line 745, and it does not name the set. The VIEWS line is reached only with the viewpoints box ticked, so the proof step has to tick it. TestViewPlanOutcome names its unknown sets by team on a line each, so the two VIEWS blocks differ in layout and cap. The F85 bullet of .claude\rules\core.md says only that the count goes in the block. FR-036 and FR-041, FR-059, FR-060 and FR-063 need the engine, ClashRunner or a probe | 181 | in review |

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
  model that was dropped from the exports still reads as a wrong condition, a model whose walk stopped
  no longer doing so since pull request 172, and the HEALTH line still says no
  model carries a category. A model dropped in ModelFactsReader.Exports has no model worksets row
- Core, left as designed by the loop: a present set whose search could not be read is never judged
  empty, so the SETS count at zero is higher than the EMPTY SETS header and the SETS block says
  how many could not be read. The HEALTH count of categories no model carries has no project guard.
  A failed list read is cached for the life of the window. The not-a-typo pairs are one project's.
  SETS ACROSS THE RUN no longer says every set found something over a run in which no set was
  counted, pull request 167
- Words and polish still open: the walk not disposing five sub objects, which FR-029 holds. The second
  ContainsTest in SetWarnings is the plan's since pull request 180
- Looked at again on 8 Oct 2026, each read off the code. Left: R9, ProjectOf calling ContainerName.Parse
  outside a guard, because the window holds one readonly settings object, parses every path with it at
  the scan outside any catch and hands the same object to the run, so the throw comes at the scan first.
  R5, the category verdict read against what a group's models carry, because ModelExport has no category
  member. R18 and R21, because they need the add-in to hand over the box and the sides. R17 is a
  HEALTH line with no project guard, narrowed by the folder it names since pull request 151, and a guard
  would need a project the pick does not know. Taken in a pull request of their own, not in 170: R10, a
  seam for the branch of the judge that says the workset list inside Core was not read, and the half of
  R14 that is Core, a group holding a model whose walk stopped, which the judge never reads though it is
  handed it. Both are pull request 172, and the rule line for .claude\rules\core.md under The sets
  compared, judged and counted is that the judge says it cannot tell about a workset no model read carries
  where some model of the group has its worksets unread, naming the models. The other half of R14, a model
  dropped from the exports, is the add-in's catch in ModelFactsReader. FederationEngine hands
  EmptySetJudge.For the list of models that were read and hands SilentMisses.GroupLines the count of
  models the document holds, so the judge cannot see a model that was dropped, and a group with one still
  reads a workset only that model carries as a condition no model carries. The repair is the add-in's
  to hand the count and a Core parameter for it, which was not added here because nothing in src would call
  it. Whether Navisworks gives a model that failed to load an empty root, which would make OneExport
  count it at nought and read, is UNKNOWN. A group with a model whose walk stopped also loses the hint
  that a workset differs from a carried one by letter case alone, since the verdict is then cannot tell,
  and the EXPORT CHECK block still names the pair
- Members with no caller that F115 did not add: RevitWorksets.DecidedCount, PlannedSet.GroupCount
  and Groups, and the shorter forms of AddCreated and EmptySets.Lines, the one of four arguments
  and the one of three, which only tests call, where the longer forms are called in src

## Where the lane stopped

Lane B stopped on 7 Oct 2026 after Bader's second order of the night, F128, then F121 the rest, then F123,
by the same limits. It took what a Core test proves in a file no open loop branch changes, and what it had
not taken when this was written is named under F123 below.

F128 was not started on the night of 7 Oct, and it was looked at twice. Bader's order of 8 Oct took it
first anyway, and its Core part is pull request 171, with what the laptop lane does with it and measures
first under F128 below. What follows in this paragraph is what stopped it the night before, and it still
holds for the add-in half. Bader's own item says to measure first which property and
value name Generic Models on 1A02MM and 1A04PK, and that needs a probe through the guarded start of
Navisworks, UNKNOWN until read. The counts per model file, the GENERIC block and the sheet all take a read
of the document that only the add-in makes, so a Core plan of the set folder, one search set per model on
the category and LcOaNodeSourceFile, would be called by no running code, which breaks the no member without
a caller rule. SetBuildPlan has no per model input, so the one place that could carry it without the add-in
is not a place the group's models reach, and the call that would hand it the models is in FederationEngine,
which is the add-in's. What can be written in Core once the probe has answered is the set plan and the
counts rule, with the add-in call in the same pull request.

F121 the rest. What was open is the add-in or the window, FR-150, 152, 153, 156, 157, 160, 161, 163, 162 and
the add-in halves of 164, or needs a choice Bader has not written down, FR-155, or a parse nothing would
call, FR-166, or an IL read first, FR-202. The two Core points of the second reading, a hand typed name with
a character Windows refuses and a collision sentence naming every group, were taken in pull request 160.

F123. The entries that were Core and a test proves are taken: FR-007, FR-061 and FR-064 in 151, FR-126,
127, 129, 130, 131 and 132 in 154, FR-168 in 155, FR-172 in part in 161, and FR-171 in part in 162, 163 and
the pull request that carries this page. The rest of F123 is the loop's own files, tools\loop and
.claude, which lane B never edits, or the add-in, and FR-171 and
FR-172 are backlogs of which the entries left are named under F123 below, each as the entry states it. The
members read only by a test, at least 43, are Bader's Q26 and wait for the steps he asked for.

One rule file line to read again: the timing block bullets of .claude\rules\core.md call the remainder row
outside every step, which is true of the group block, and the by group section of the run block says outside
every group since 163. Lane B never edits .claude.

The pull requests lane B merged on the first part of the night are 142, 145, 146, 147, 148, 150 and 151,
and on the second 154, 155, 160, 161, 162, 163, 164, 165 and the pull request that carries this page. Each ran its Core tests under mono here
and on the Windows runner of Actions, each had one reader, and where a reader found a fault that
changes what the team sees it was fixed before the merge.

Every item above that lane B left, and every point its readers raised, is under its own heading below,
for the laptop lane to take in the order it chooses.

## F128 the Core part is on main, what the laptop lane does with it and measures first

Bader's order of 8 Oct 2026 took F128 first, over the judgement of the night before that it was blocked
for want of a measured property. The Core part does not wait for the measurement, because it carries the
unmeasured parts as settings and as one argument, and says UNKNOWN where it cannot know. What is on main:

- GenericModelsSettings, the value of the Category property, default Generic Models, the folder, default
  Generic Models, and the sheet, default Generic Models, each changed in one place
- GenericModelsPlan.For(models, settings), one GenericModelInput for each model of the group, the file name
  or path as the document holds it, and a MatchText that is null until the probe says which text the Source
  File of an item holds, the stem of the file name being used until then
- plan.ToBuildPlan(), which SetBuilder.Build takes as it takes the picked file's plan
- GenericModelsReport.From(plan, results), then Lines() for the block and BlockTitle, GENERIC MODELS, for
  the heading the add-in puts the building after
- GenericSheet.Rows(report) for the one shape of the sheet, Write for the sheet itself, which is internal
  and is reached from WorkbookWriter the way CoverageSheet is

What the laptop lane measures first, with tools\probes and the guarded start of Navisworks, UNKNOWN until read:

1. The value the Category property of an item reads for a Generic Models item in 1A02MM and 1A04PK.
   The tool's own list holds Generic Models and the probe of 2026-09-20 counted 62 such items over the
   ten C02 NWFs, reading the first property displayed as Category, Revit Category or Element Category on
   the item, which says nothing of these two buildings and does not show it was the property the client's
   file asks. If it differs, GenericModelsSettings.CategoryValue is the one place to change, and nothing
   else is.
2. Whether the Source File of an item holds the NWC's stem or the name of the Revit file it came from.
   The first is what the plan looks for, and a model whose items hold the second is found by handing the
   plan that text as the MatchText of its GenericModelInput, which the add-in reads off the model.
3. Whether the Category property is on the Element tab for the items of these two models, as the
   client's file asks it. The conditions copy that file's, LcRevitData_Element, and a test holds them to it.

What the add-in must be careful of, found by reading SetBuilder and SetLeftovers and not by running them:

- The leftover walk, in both directions. HandleLeftovers names the sets of the plan it is given and hands the
  document's sets to SetLeftovers.For, which treats every set that plan does not name as a leftover, and with
  the rebuild box ticked it removes or renames one. So the picked file's call finds the Generic Models sets,
  which the previous run saved in the NWF, and removes them, and a call that builds the Generic Models plan
  finds every set of the picked file that no created test points at and removes those, an empty Generic Models
  plan with nothing named removing every set with no sides. The second is the worse, since it takes sets of
  the client's matrix. The Generic Models sets have to be built through a call that does not walk the leftovers,
  or the walk has to be handed the names of both plans as wanted. Which of the two the builder allows is for
  the laptop lane to read, and it is read from SetBuilder.Build, lines 610 to 613 and 633 to 647 on 8 Oct 2026.
- A SetBuildOutcome of their own. The EMPTY SETS judge and SETS ACROSS THE RUN read the outcome of the picked
  file's sets, and a Generic Models set at nought is a model that found no item of the category with its
  text, which is also what a wrong category value or a wrong text gives, so it is not a finding about the
  set. Putting them in that outcome would name them as sets nobody should have left empty.
- The question of a set already there. A set in the NWF keeps the question it was built with unless the
  rebuild box is ticked, so after the category value or the text is changed it asks the old one.
  GenericModelsReport.From reads SetResult.Asked of a present set and counts it as not counted, with both
  questions, where it differs from the plan's, and where it is empty. SetBuilder sets it to what the
  document's set asks, SetBuilder.cs line 814, and the add-in must keep doing that for these sets.
- The model's text. A model's text is found by contains, and whether Navisworks compares it without case,
  and whether the Source File of an item is a bare name or a full path, are UNKNOWN. A path that holds a
  folder named like a model's stem would count every item under it. The block says what a nought can mean
  and the plan names the texts that meet, and neither can see a text that meets a path.
- The file name that two models share. Two models of one stem are one set, which the plan says, and the
  second model's text is not looked for when it differs, which the plan says too. Whether Navisworks keeps
  two sets whose names differ by case alone is UNKNOWN, and the plan treats the names as one without case
  while SetBuilder finds a set by its name exactly, so a model whose file changes its case between runs
  would be a second set.
- The cost. Each model is a search of the whole document, and FindSelectionSet walks a folder once for
  each set. With one file for everything the folder holds a set for every model of the run. Nothing here
  was timed, so the seconds are UNKNOWN until the sets step is timed on a real group.
- The category name. LcRevitData_Element is public in GenericModelsSettings since the readers found
  ModelFactsReader typing it a second time as ElementTab, and the add-in can read it from there.
- The NWF save. Creating a set asks for the save, FR-020, so the sets step must say that a Generic Models set
  created counts, or the NWF is saved without them and the next run creates them again.
- No clash test. Nothing here makes a test, and the block says so. The count is of items, which are the
  elements the set finds and not the Revit elements, and the block says items.
- The sheet. FR-200, his request 2 under Q112, makes the Coverage sheet the second and last sheet and the
  workbook check allows nothing after it. A third sheet breaks that, and a workbook of its own for
  the group, as Q126 B gives the Coverage sheet where the clash is skipped, breaks nothing. It is for Bader
  to choose and GenericSheet.Write takes the workbook it is handed, so either way is the same call.
  WorkbookCheck allows two sheets, so the workbook is the laptop lane's.
- The rule lines. .claude\rules\core.md has no line for this part, and lane B never edits that folder.
  The lines to write are that the sheet is named by GenericModelsSettings, that a count nobody took is
  UNKNOWN and never nought, that the total says at least over one and says it is not a count of items where
  the texts of some sets meet, and that a model at nought is left out of the list, counted in the line above
  it and followed by a line saying what a nought can mean.

The members have no caller in src until the add-in half, which the no member without a caller rule would
delete, and Bader's order of 8 Oct 2026 keeps them. They go with Q26's list until the add-in half calls them.

## F127 points the readers raised that lane B left, for the laptop lane

- Not in Core yet: the categories no set catches per model, since CoverageSettings.CategoriesNamedPerModel is
  read by no rule and ModelExport carries no count per category. FR-176's RESULT count of the tests, once
  across the run, is in TestLines since pull request 153
- A test whose reason is built by a mirror may bypass ReasonFor: read on 8 Oct 2026 after 177 and it does not,
  since no mirror code builds a reason and every one comes from ClashTestPlan and CoverageRule.ReasonFor. A test the plan
  dropped before the model, that an earlier run left in the NWF, now gets its cause from CountCheck and not
  from CoverageRule, which never looks for it in the document
- The workbook handed in carries no proof of which run wrote it, so a write that threw leaves last week's
  file at the same path to be read as this week's, and the fingerprint names the XML bytes and not the
  corrections list or the teams file that changed its sets. The add-in has to hand a stamp in
- A set with no code in its name reads UNKNOWN where the team map of F131 gives the team of its folder,
  Q117 C, and RESULT lists every FAILED line by default, Bader's choice, which can be some 25 MB where one
  fault repeats over 46 groups
- WorkbookTests.Read takes the first sheet and would read the Coverage sheet if that were inserted first,
  and the Coverage sheet is second and last by FR-200, so it is a guard and not a fault today
- Left after the second reading and this one: a group whose check is null still has its tests in no bucket,
  and the headline says so without counting them. Nothing in src\Federator.Addin builds a
  CoverageAcrossTheRun yet, which is the add-in half of F127
- Words, not taken: the copies of Count are one, Words.Counted, since pull request 176. The row count loop in the
  Coverage folder and a double blank line in RESULT that lane B did not reproduce are not taken

## What the F127 Core points ask of the laptop lane, found by their readers

- Step 174 of steps\03_bader_next.md says a run with no XML reads zeros and Every set found something
  somewhere, which is right. With no XML the sets step never calls SetsAcrossTheRun.Add, so the block now
  reads No set was counted, so nothing is said of what the sets found. Lane B does not edit that file.
  Step 174 has to say the new sentence or Bader reports a failure that is not one
- .claude\rules\core.md lines that wait for the pull request that merges this one, which lane B may not
  write: a test the run left no record of, or never reached, is not compared and one the run knows it did
  not create is held by neither side. A test the plan dropped before the model, that Clash Detective holds
  with results, names an earlier run as the cause, and the same is not said of a test F77 kept out, since
  the runner walks the document first and finds none of that name. The tests the file does not name are
  counted once by name across the run. The headline counts test places, a test once for each group it is
  in. A set is counted once per group, is not at zero where one set of its path was not counted, and the
  F82 section says a set never counted is named on its own line
- The reader's note that the denominator of SetsAcrossTheRun is the groups added and not the groups of the
  run holds: Add is called only for a group with work and a built outcome. The block says groups counted
  here and does not claim more

## F137 points the readers raised that lane B left, for the laptop lane

- ModelFactsReader.SharedCoordinateOn returns an empty site where the Location tab is missing or the
  property collection is null, not only where a model names none. Since Q111 B a read fault reads as a
  model naming no site and skips its group's clash where it used to fail the group and still clash it. A
  null tab should read as site not read, and whether any of the 27 machines names the tab differently is
  UNKNOWN
- The reference model can itself name no site, as it could already name Internal, and every model is
  then measured from it and listed far. The ALIGNMENT block says so since pull request 173 and the
  reference is still chosen as it was, because choosing another is a decision for Bader that changes the
  distances in the block. ModelsRead counts placements and not the document's models, so a
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
- The order check of WorkbookCheck read the full blocks only, so a one row test placed before a full
  block was not named as a wrong order, which pull request 176 closes. With a priority file picked the check does not
  run the Priority heading check on a sheet with no full block, and a sheet of one row tests has no heading row for it
- .claude\rules\core.md does not say that a repeated test name in the priority file is a named problem
  or that a workbook of one row tests is counted. The engine writes each priority problem once as a line
  and again under the match lines, which is older than this change

## F119 points the readers raised that lane B left, for the laptop lane

- The loop's run.ps1 near line 784 words a GONE file as the tool's own pruning of its oldest logs only
  when it matches run-*.log, so a pruned .tsv of Bader's now reads as a finding at the next loop run
  though logs-backup still holds it. Bader's .tsv files that earlier builds left beside logs already
  deleted are not removed, only growth from now on is stopped. Lane B took FR-055's own note that they go
  with the pruned logs as the decision, and Q82 does not name them
- What the FR-057 fix does not close: the figure SizeOnDisk falls back to for a file held with no sharing
  is the directory's and can lag, and is not marked, and a fault on the first line is shown by no test, only
  by the code falling to the next folder. In the window, which lane B does not touch, FederatorWindow near
  line 126 sets the progress line to WhereTheLogIs once when it opens, so after a fault in mid run it still
  says Log and the path, and the open log folder button goes to the default logs folder and not to the
  folder the log fell back to. A failed write to the .tsv is said once and the
  sentences about it follow, since pull request 178, and a header it cannot write gives the file up. The closing steps of Dispose swallow their failure with a reason in a comment and no line,
  as the old Dispose did
- .claude\rules\core.md lines about retention and the size rule do not say the .tsv goes with its log,
  that the temp fallback is not pruned, or that a file held with no sharing falls back to the directory's
  size, that the RETAIN line counts a refused .tsv apart from a refused log, or that the timing block of a
  run counted to now prints no after the run finished row, or that a write to the log file that throws
  stops the file, keeps every line in memory and in the window and copies the log from memory. It also still
  says, near line 2129, that a write to the .tsv that throws is swallowed, which is false since 178: the
  first one is kept, told once and the file stops taking rows. FR-049, a second run in one window carrying
  the first run's totals, is closed since pull request 179 by a RESULT closing its account, and what that leaves for the window is on its row

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
- The scan findings wrote a SINGLE DISCIPLINE finding for every group under both one discipline modes,
  FR-154's note, which is one finding since pull request 173. The window row of such a group still says
  every test is created and none is run, GroupRow near line 257, which F77 made false, and step 94 of
  steps\03_bader_next.md records it, so the row and the finding disagree on one screen until the add-in
  half changes the row. Whether Bader means no clash test to run at all in the two modes is still
  FR-154's open question, and the finding states only what the engine does. The Source step's count line
  reads the kind as one finding where it read one for each group. FR-167 has the same root in
  SourceMismatchFindings. A group spanning buildings has no building code and is left out of the odd shape
  and near match findings
- The window's check before a run now refuses a name that cannot be used, and the preview still prints the
  sentence for such a row, FederatorWindow near line 621, and SafeName has no caller

## Found at the second reading of F121, for the laptop lane

- The window's preview reads WhyTheRunCannotStart off the whole name table, FederatorWindow near line 648,
  and OnRun reads it off the ticked groups only, near line 1764, so the two can give different counts for
  the same table, and a cleared name cell of an unticked group makes the preview say THE RUN CANNOT
  START where the run would start. The preview should read the same Only(TickedGroupKeys()) table
- Closed by lane B in the pull request listed above: the hand typed name with a character Windows refuses and the collision sentence naming every group

## F123 points lane B left, for the laptop lane

- FR-171 is a backlog and most of its entries are in the add-in or the loop's files. Core entries lane B did
  not take, each as the entry states it and not read again on main by lane B: T1-N54 two duration formatters
  whose spellings differ, so merging them changes a line the log writes, and T1-N46, N48, N66 and N85 values
  and members the entry says nothing reads. The files fix-F118 changed were read again on 8 Oct 2026 and the
  entries in them are taken or closed in pull request 174, and those of the files fix-F132 changed on the
  same day: T1-N47, N75, N83, N84 and the second place of T1-N64, the set tree root typed as a literal in
  ClashReportModel, in 180. Left there: T1-N77, whether ClashItem.Category is a gap the report keeps back,
  which is Bader's to say, and the first place of T1-N64, in ClashRunner. T1-N78 and N81 were closed by FR-199 and
  FR-035, as 174 says. AlignmentCheck.DefaultToleranceMillimetres and DefaultInternalName are
  constants the add-in passes as they stand, so a person cannot move them without a build, which the
  every number is a setting rule asks to be a setting. The comments now say so and the settings are not made
- FR-172 is left in the main. Of its Core entries, the two above were the only ones no file under src, tests or
  tools reads. Those read only by a test, at least 43 of the entries by a word match over src and tests that skips the definition file, which counts a common word such as Count or Path as read, are Bader's Q26 and wait for the
  steps he asked for, breaking each on purpose and checking its test fails. PenetrationSide.ItemName and the
  other add-in entries are read by no code, but PenetrationSide takes the name as an argument the add-in
  passes in Penetrations.cs, so deleting it needs the add-in changed in the same pull request
- FR-168 changed two lines of the FOLDERS REMEMBERED block. tools\loop\run.ps1 near line 1428 still matches
  the old first run line by string, StartsWith Nothing remembered yet, and the new lines do not match it. The
  loop's fallback masks any line within a second of the block's first, so nothing leaks today, and the string
  is the loop's to update. The block is written once at window open, so a folders file that fails to save
  later still never reaches the log, which is FR-162
- FR-061 says the .tsv holds a collapsed line where the .tsv opened. A write that fails after it opened is
  said once and the sentences follow it since pull request 178. A sentence already written, the one the
  sixth repeat of a line gets, keeps what it said when it was written, and the RESULT block and the size line
  of the .tsv correct it
- Three places in the add-in still say what Core no longer does, found by the reader of part 2. GroupRow.cs
  near line 257 says One discipline, so every test is created and none is run, and its comment near line 155
  says the same, where Core now says only the tests whose two sides both find something are created.
  FederationEngine.cs near line 499 says the run was stopped with StopTheRunReason, which is the guard's log
  wording and ends in what was thrown, where the window should say RepeatedFailureGuard.Stopped. Both are
  for the laptop lane to read off Core. The engine file is one fix-F114 changes, so lane B left both
- FR-059 and FR-063 and FR-060 need the engine or ClashRunner to hand Core the picture count, the plan
  source or the tests run. FR-038 and FR-039 and FR-074 are taken in pull request 181 since fix-F132 merged,
  and FR-042 was closed by FR-199, read on main
- The category line names the folder from a folder line lane B added to revit-categories.txt, C02, read
  off that file's own header, and the same list is not a list of the other folders

