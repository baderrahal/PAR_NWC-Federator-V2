# Lane B, the cloud lane with no Navisworks

The closed items of this file moved to steps\history\lane-b-closed.md on 19 Oct 2026, whole and in their order, F242, Bader's round of 19 Oct b.

Set up by Bader's message of 7 Oct 2026 headed FAST TO A TEAM RELEASE. This lane has no
Navisworks and no add-in build, so it works Core only, src\Federator.Core and
tests\Federator.Core.Tests, and where an item needs an add-in change it does the Core half,
says so in the item's log entry, and leaves the add-in half for the laptop lane.

The laptop lane reads this page before taking any item, never takes one listed here, and
leaves the files the open branches below change alone. This page is kept true at every
merge: the item the lane is on, its branch, the files it changes, and what merged.

## The item the lane is on

None. The last item was FR-173, the catches a reader called swallowing, 34 of its 77 in files of src\Federator.Core, sorted under F123 below and merged as pull request 184. The lane stopped there, see Where the lane stopped. The pull requests merged are listed under What merged.

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

