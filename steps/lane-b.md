# Lane B, the cloud lane with no Navisworks

The closed items of this file moved to steps\history\lane-b-closed.md on 19 Oct 2026, whole and in their order, F242, Bader's round of 19 Oct b.

Set up by Bader's message of 7 Oct 2026 headed FAST TO A TEAM RELEASE. This lane has no
Navisworks and no add-in build, so it works Core only, src\Federator.Core and
tests\Federator.Core.Tests, and where an item needs an add-in change it does the Core half,
says so in the item's log entry, and leaves the add-in half for the laptop lane.

The laptop lane reads this page before taking any item, never takes one listed here, and
leaves the files the open branches change alone, a list now in steps\history\lane-b-closed.md with what merged. This page was kept true at every
merge: the item the lane is on, its branch, the files it changes, and what merged.

## The item the lane is on

None. The last item was FR-173, the catches a reader called swallowing, 34 of its 77 in files of src\Federator.Core, sorted under F123 below and merged as pull request 184. The lane stopped there, see Where the lane stopped. The pull requests merged are listed under What merged, in steps\history\lane-b-closed.md.

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

Lane B stopped on 8 Oct 2026, with the pull request that carries this section as its last. FR-173, the catches a reader called swallowing, was the last item it took, merged as 184.

What it was told. Bader's order of that morning, headed LANE B, GOOD MORNING, BADER, 8 OCT 2026, said in its second step: "Take the next open items a Core test proves, in this order: F128 generic models' Core part, any Core part still open of F115, F118, F119, F127 and F137, then wave 4 F121, then wave 5 F123. Skip any item that needs Navisworks or the add-in build, and any file an open branch of the loop changes." and in its sixth: "Keep going until no such item is left, then write in steps\lane-b.md where you stopped and why." His second message of that morning said: "Take F128, generic models, its Core part, next and first, before anything else on your list, and merge it once Actions is green. The laptop does its add-in part after you. Then carry on with your list as before." F128's Core part merged as 171, after 167, 168, 169 and 170, so the merges are not in the order of his list, and this page holds no time for his second message.

Why it stopped, and what that does not say. It stopped when the points it had read and not taken were each one of these: they need the add-in or an engine that lane B cannot build, they need a measurement, they need a choice nobody has written or one of Bader's that he may want to see again, they are members read only by a test, which are Bader's Q26, they are in files lane B may not edit or in files an open branch changes, or lane B read them and left them on purpose. It does not say that no point is left. A point it did not read, in a file it did not open, is possible, and two branches of the laptop lane now change Core files too, see below. The lists under the headings further down are what it knows is left.

How the open branches were read. At 12:41 UTC on 8 Oct 2026, after git fetch origin, with git diff --name-only in the three dot form, origin/main...origin/branch, so a branch behind main shows only what it added, files under src and tests only, and git rev-list --count of origin/main..origin/branch for the commits ahead. fix-F109 and fix-F114-probes change none. fix-F114 changes thirteen, eight in Core, Coverage and Views, CoverageRule, ToolViewMark, ViewNode, ViewpointRequest, ViewpointSettings, ViewsPart, ViewsSeconds and ViewsTreeCheck, and five tests, and no file of the add-in, so the rows that say the engine file is one fix-F114 changes record the plan of its add-in pass and not what was on the branch. fix-F128 changes twenty one, among them the add-in's FederationEngine, JobOutcome, SetBuilder and FederatorWindow and, in Core, RunLog, RunSteps, the Generic files, GenericWorkbook, ReportOptions, SetBuildOutcome and SetResult. The newest commit of each is dated about 12:30 UTC. This page showed until now a reading that found none under fix-F114 and said fix-F128 was never made as a branch of its own, and it is a branch eight commits ahead of main. fix-F118, fix-F120, fix-F132 and fix-T5-close-1007 are no commits ahead of main. fix-F65 to fix-F72, round-close-penetrations, round-scale and master have no merge base with main in this clone, which is shallow, so lane B read nothing of what they change and does not know it.

What it merged on 8 Oct 2026, by the dates of the merge commits on main: pull requests 167, 168, 169, 170, 171, 172, 173, 174, 176, 178, 179, 180, 181, 182 and 184. The row of each under What merged, in steps\history\lane-b-closed.md, says what it took. Pull request 175 carried a first draft of this section and was closed unmerged after a claim check found it said more than lane B had read, among it that no item was left. A second draft, in the branch of 182, was held back after a claim check found FR-173 unsorted, which is why 184 exists. The commit of the first draft and the revert of it, 41dce3e and 7a8b65f, are in the history of main. Pull requests 177, 159, 183 and 157 are not the cloud session's: 177 is fix-F132, 183 is fix-F120, 157 is fix-T5-close-1007, and 159 is fix-F118 by the worktree session of the lane.

Proof, as far as it was read.
- The check run named test showed success on the final head of each of those fifteen, read through the GitHub tool on 8 Oct 2026 after the merges. The job ids, by pull request: 167 113173052318, 168 113176683349, 169 113183134272, 170 113197343947, 171 113207005616, 172 113213519038, 173 113220140196, 174 113229731054, 176 113239877605, 178 113249825989, 179 113260771415, 180 113266912174, 181 113274507090, 182 113291688422, 184 113314129183.
- The rows record a reader or reviewer in 167, 169, 171, 172, 173, 174, 178, 179, 180, 181 and 184 and in the F118 row of 176. They record none in 168, 170, 182 and the F127 row of 176, and the commit titles of 168 and 170 name a reviewer's changes, so the rows undercount those two. A breaker is recorded in 167, 171, 172 and 179. Where a row says findings were fixed it says so, and 184's says its fixes after the reader were not read again.
- Core tests were 2408 run, 2375 passed, 0 failed, 33 skipped before 167, as row 167 gives them, and 2774 run, 2739 passed, 0 failed, 35 skipped on the branch of 184, with main's 183 merged in, under mono here. Row 165 gave 2409 run, 2375 passed and 34 skipped for the main row 167 started from, and the one run and one skipped between them is UNKNOWN. Of the 366 more tests between 2408 and 2774, the rows of lane B account for 130, pull request 177 for 214 by rows 176 and 178, pull request 183 for 21 by main's count of 2766 against 2745 after 182, and one is not accounted for, between the end of row 170 and the start of row 171. The skipped went from 33 to 35, one when 168 added a test that skips off Windows and one when 184 did.
- The result file of this machine's run at that head lists 36 tests not executed: 26 that say Navisworks is not on this machine, 9 that are about a Windows file system rule or the path methods of .NET Framework, which only Windows does, and one that writes the corrected matrix by hand. The run's own summary counts 35 skipped, so one of the 36 is not in its count, and which one was not checked. None of the 26 ran here.
- On the Windows runner of Actions the set ran 2774, 2748 passed, 0 failed, 26 skipped at the head of 371edda in 184. The workflow runs on pull_request with no ref set, so it tests the pull request merged with main as main then stood, and main held 183 by then, whose 21 tests are the gap to the 2753 this machine ran at f729072, which has the same code. That is read from the workflow file and the arithmetic and not from the checkout of the run. Nine fewer were skipped there than here, which fits the nine Windows only tests.
- Nothing of the add-in half of any item was built or run by this lane, since it has no Navisworks. The laptop lane merged F132's add-in half as 177 and F120 as 183 the same day, by steps\PROGRESS.md.

What is left, in six kinds. The full lists are under the headings below.

1. The add-in or the engine, where the root cause is there or Core has no caller until the add-in changes or hands it something. The fix-round file names the add-in in the root causes of FR-036, FR-047 at its two call sites, FR-053, FR-058, FR-062, FR-128, FR-133, FR-169 and FR-170, and FederationEngine first in those of FR-043, FR-044 and FR-045. FR-189's evidence is in FederationEngine.WriteNwd and its proof a Core member of RunLog that the add-in would have to call with the publish result. FR-059, FR-060 and FR-063 have Core files in theirs and need the engine or ClashRunner to hand Core a count or a source. FR-166 would have no caller. FR-162's Remember returns nothing and nothing in the window would read it. Among FR-173's 34, FolderMemory 283, which is FR-162, and WorkbookWriter 596, whose count the engine would have to print. The add-in halves of F127 and F128, the open file run's marks that FR-049 leaves, two sentences of ViewsTree and PenetrationTally that need the engine to say whether the .tsv opened, T1-N46's count that only ClashRunner writes, and the first and third places of the root literal in ClashRunner and SetBuilder.
2. A measurement first, or a machine this lane does not have. The value the Category property reads for Generic Models was probed on the laptop at 13:02 laptop time on 8 Oct, scan.md 5z-zb by steps\PROGRESS.md, and lane B did not see the result. The text a Source File holds, FR-041, needs a probe printing the intersection, FR-202 an IL read first, and FR-128's APPLIED line a measurement. Whether a model that failed to load gives an empty root is UNKNOWN, and so is the SizeOnDisk fallback figure that can lag and is not marked, the mechanism of whose lag is not measured, by FR-046's own text. The throw of the path methods was measured, for a bar, a quote and a tab, by the Windows runner in 184, and the two angle brackets were not.
3. A choice nobody has written down, or one of Bader's that he may want to see again. FR-155, whether the reader skips a test or refuses it, whether ClashItem.Category belongs in the list of gaps the report keeps back, T1-N77, a question for Bader by the standing rule GapRule writes down, which lane B may not write, which of two duration spellings the live line keeps, T1-N54, the model the ALIGNMENT block measures from, and where F128's sheet goes, which steps\PROGRESS.md says was put to Bader as Q146 with the lead's B applied meanwhile. Under F127, that RESULT lists every FAILED line by Bader's choice, which can be some 25 MB where one fault repeats over 46 groups.
4. Bader's Q26, the members read only by a test, counted under F123 below. Among the FR-171 ones the Judged of T1-N46, the OfKind of T1-N66 and the two members of DamagedDocument that only tests read, TheFileMayBeSaved and Line, T1-N85, whose comments say so since 182.
5. What lane B may not edit or take. The loop's own files, tools\loop, the rule lines of .claude\rules, which the rows name, among them core.md on the open file run's checks and on the date format, steps\tracker.csv, whose row for FR-173 reads open there because the lane does not write it, the rows of FR-038, FR-039, FR-042, FR-049 and FR-074 having been set to merged by 183 in its commit 2a6c347, steps\03_bader_next.md, which holds the proof steps the add-in changes need, and the files fix-F114 and fix-F128 change.
6. Core points lane B read and left on purpose. The four F115 left as designed by the loop, the constants AlignmentCheck passes as they stand, a group spanning buildings left out of two findings, two sets of one name in a picked file, and, from FR-173, the exception of a workset or category list that did not load, which is told as UNKNOWN and not kept, and nine fallbacks that answer for something that could not be read, a path the framework will not take apart or a size, each with the reason now at the catch. Each is under its heading below with its reason, and a reader may disagree with one.

Two commits of pull request 178, c4a3bd3 and 7c32faf, carry a Co-Authored-By trailer that CLAUDE.md forbids, found by reading the message of every commit of origin/main that this shallow clone holds. They are on main and history is not rewritten here. The same reading finds four more, 774b22b, 86abb46, 8031c4c and 4d549c0, committed under the name baderrahal in the work of fix-F132, which are not lane B's.

### What the page said on the night of 7 Oct 2026, kept for what it says of F121 and F123

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
- Closed by lane B in the pull request listed above, under What merged in steps\history\lane-b-closed.md: the hand typed name with a character Windows refuses and the collision sentence naming every group

## F123 points lane B left, for the laptop lane

- FR-171 is a backlog and most of its entries are in the add-in or the loop's files. The files fix-F118 changed
  were read again on 8 Oct 2026 and the entries in them are taken or closed in pull request 174, and those of
  the files fix-F132 changed on the same day: T1-N47, N75, N83, N84 and the second place of T1-N64, the set
  tree root typed as a literal in ClashReportModel, in 180. T1-N78 and N81 were closed by FR-199 and FR-035, as
  174 says. Read again on 8 Oct 2026 and not changed beyond words: T1-N48, whose claim of an unreachable
  branch is false since the views default to NewAndActive, and T1-N85, whose comments said callers pass through
  a gate that only tests read, both in 182. T1-N77's false comment, that the penetration rule turns on
  ClashItem.Category, is fixed in 182, and whether that category is a gap the report keeps back is, by the
  standing rule GapRule writes down, a question for Bader like Q35 to Q40, which lane B may not write. T1-N46 is a count only ClashRunner writes and nothing reads, so
  deleting it is an add-in change, and its Judged is read by tests, which is Q26. T1-N66's two OfKind are
  read by tests, which is Q26, and a deletion of one failed to build them. T1-N54, two duration formatters
  whose spellings differ, was not read again, and merging them changes a line the log writes. The first place
  of T1-N64 is in ClashRunner and a third literal of the root is in SetBuilder near line 223, both the
  add-in's. AlignmentCheck.DefaultToleranceMillimetres and DefaultInternalName are
  constants the add-in passes as they stand, so a person cannot move them without a build, which the
  every number is a setting rule asks to be a setting. The comments now say so and the settings are not made
- FR-058, FR-062, FR-128, FR-133, FR-169 and FR-170, as the fix-round file states their root causes, are in
  the add-in. FR-058 is the engine writing the count of tests buildable as the number to create before the
  creation plan runs. FR-062 is the second read of the document's units in DocumentUnits, whose catch writes
  nothing. FR-128 is the APPLIED line of ClashRunner against the drift block and the help under the box, and
  needs a measurement first. FR-133 is SavedViewpoints.CanBuild, which is always true and which nothing reads.
  FR-169 is DocumentGuard calling a name it could not read unsaved. FR-170 is a catch in Penetrations that
  swallows a unit or property throw, whose guard test, that every unit name resolves through the table, is in
  182 and passes today. FR-162's Remember still returns nothing, and nothing in the window would read what
  it returned
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
- FR-173, sorted on 8 Oct 2026 for the 34 catches in src\Federator.Core, by the line the entry gives at 42499bf
  and the member it names, read on main where the lines have moved. The reader of the change found parts of the
  first sort wrong and this is the sort after its findings:
  Mended before this reading, the cause kept or told. RowLog 152, a write that threw, by 178, which keeps the
  first and tells it once. FolderMemory 75 and 116, by 155, the startup block naming a folders file that could
  not be used. ReportPaths 128, a filter that keeps the first line of the refusal it throws on purpose.
  OutputNameTable 243, a name that could not be built carries a marker, CannotBeNamed, which one rule tells
  from a name. ReportPaths 128 has a second catch beside it now, for anything else, which shows the plain wording
  by the rule that a label carries no framework message and drops what was thrown, and has a line saying so.
  The effect is told and what it threw is not kept. RevitCategories 168 and RevitWorksets 159, a ResourceFound
  flag so a list that did not load reads UNKNOWN and not empty. RevitCategories had the flag already at
  42499bf, and RevitWorksets got it from F115's FR-012, merged as 142. The exception type and message of
  either are in no log, and whether a run can reach either is UNKNOWN, so the lane left them. This is not a
  repair, and a comment in RevitWorksets that said not swallowed was reworded to say what it threw is not kept.
  The rule that logging never stops a run, with its reason in a comment: RowLog 180 and RunLog 2101, the
  closing steps, each on its own since 169 and 178, LiveLine 112, a pace that cannot be read, ToleranceChoice
  317, a log line that must not throw, RunLog 2042, a copy made from the lines held in memory when the file
  cannot be read, which are the same lines, FolderMemory 202 and 231, a picker that opens where it always did,
  and BundleAssemblies 93, 149 and 236, a name that is not ours, an assembly with no location and a note that
  must not stop a load. In RowLog 180, RunLog 2042 and FolderMemory 202 the reason was away from the catch, at the call
  or in the method's summary, and this pull request puts a line at the catch.
  A fallback that is deliberate: ContainerName 135, InstallFiles 157, RunLog 448 and 2064, a path that cannot be
  expanded is compared as it came and a size that cannot be read after the directory's own figure is NOT ON
  DISK, ReportPaths 225, NwfComparison 245, NwfRebuildPlan 168 and 181, and PageCheck 484. The last four were
  first sorted as a wrong reason on a path Navisworks does not hand the tool, a premise nothing observed backed
  and one that was wrong for OpenDocumentJob. Read again, each answers for a path the framework will not take
  apart with the path whole, or with not on disk, which is an answer and not a lost cause. NwfComparison had its
  reason at the catch, and RunLog 448 and 2064, NwfRebuildPlan 168 and 181 and PageCheck 484 have one now.
  Fixed in this pull request, each with a Core test that fails on main's source except where it says so:
  RunLog 336, Start took every IOException for a name taken, see the table row. OpenDocumentJob 145 and 233,
  which the first sort called a wrong reason on a path Navisworks does not hand the tool: the catches are for
  a path the path methods of .NET Framework throw on, and WhyNot called one of them first, on the stem, so
  CanRun, WhyNot and Describe, which fill a label in the window, could throw on a path holding a quote, an
  angle bracket, a bar or a control character. A test that skips off Windows,
  TheMethodWhyNotCalledFirstThrowsOnACharacterNoPathMayHold, measured the method on the Windows runner at
  371edda. The run reports 2774 run, 2748 passed, 0 failed and 26 skipped, and the test is not among the
  skipped, so the net48 build threw on a bar, a quote and a tab. The two angle brackets are not measured.
  Under mono the method does not throw, so the test of the bar is red on main's source here only because mono
  then says the path has no folder. WhyNot now looks for the address and for such a character before any call
  that splits the path, and names the character. An address ending in a slash is now an address and no longer
  an unsaved document. The test of an address holding a bar cannot fail on main here. NamePattern 146, a date
  format DateTime.ToString throws on gave the number, so a dated NWD was named as last week's and written over
  it, where the rule says the format is a person's setting and its mistakes show, and now the name carries the
  sentence and the run does not start, which is a change of behaviour on a format nothing sets today, FR-161.
  The fallback for a format that makes a name Windows refuses stays, its test pins it. BundleAssemblies 192,
  a file found in the bundle that would not load now says so with what it threw, still handing back nothing.
  WorkbookCheck 455, a cell fill ClosedXML keeps as a theme colour throws when read and was called not filled,
  which names a banding fault that is not there, and now says the colour could not be read.
  Needs the engine or the window: FolderMemory 283, FR-162, where Remember drops the reason, and WorkbookWriter
  596, a thumbnail that would not embed, whose count the engine would have to print, the IMAGES block being
  printed before the workbook is written.
- .claude\rules\core.md, which this lane may not edit, says five things are checked before an open file runs
  where OpenDocumentJob checks seven in order, and its date format bullet says a result Windows refuses falls
  back to the number and says nothing of a format DateTime.ToString throws on, which now stops the run. For the
  laptop lane
- FR-059 and FR-063 and FR-060 have Core files in their root causes, RunLog, ToleranceChoice and
  ClashesAcrossTheRun, and need the engine or ClashRunner to hand Core the picture count, the plan source or
  the tests run. FR-038 and FR-039 and FR-074 are taken in pull request 181 since fix-F132 merged, and FR-042
  was closed by FR-199, read on main
- The category line names the folder from a folder line lane B added to revit-categories.txt, C02, read
  off that file's own header, and the same list is not a list of the other folders

