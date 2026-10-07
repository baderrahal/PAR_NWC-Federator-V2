---
paths:
  - "src/Federator.Addin/**"
---

# Rules for Federator.Addin

The add-in is the only project that touches the Navisworks API, and it cannot be built
in the container, only on a machine with Navisworks Manage 2025 installed. Every change
to it is read twice before it is pushed and its proof is a run on the local machine,
written into steps/03_bader_next.md. Rules whose logic lives in Core are in core.md.
The reasons and the measurements behind every rule are in
docs/history/claude-md-history.md, kept whole.

## A parse is not a build, and where the add-in CAN be built

THE ADD-IN WAS BUILT FOR THE FIRST TIME ON 2026-09-19, by F69, on the machine that has
Navisworks Manage 2025 installed. Before that it had never been compiled anywhere that
this repo records, which is how F52 shipped CS0128, F61 shipped CS0246 and sixteen more
errors sat behind those two, unseen, across two rounds.

WHERE A SESSION RUNS DECIDES WHAT IT CAN PROVE, and a session says which it is rather
than assuming. A session on Bader's own machine has the install, so it runs
dotnet build ParsonsNwcFederator.sln -c Release and the add-in is PROVED to compile. A
session in a container has no install, cannot build the add-in at all, and everything
below applies to it.

Five log entries report that the add-in parses with the same error codes and not one
CS1xxx, and every one of them is true and none of them is a build. The check behind
those words passes -nostdlib with no references, and Roslyn then stops before it binds
a single method body, so it reads SYNTAX and nothing else. Measured on 2026-09-19:
adding the net48 reference assemblies and the built Federator.Core.dll makes it bind
every body whose signature it can resolve, and it still cannot see inside a method that
takes a Navisworks type, because Roslyn skips the body of any method whose signature it
cannot bind, which is most of the engine.

So the words are parses and never builds, a round says what it could not check rather
than leaving the reader to assume, and a session that CAN build says the build succeeded
with the error and warning counts beside it. The build is step 8 of
steps/03_bader_next.md and no runner anywhere has Navisworks, so Actions can never do it.

Two rules of the compiler's run without it, and there are two because two rounds each
shipped a different compiler error from here.

tools/checks/check-locals.sh refuses a local declared twice in one method scope, which is
CS0128, and that is the fault F52 shipped and nothing here saw for a day. It reads text and
not a program, it knows nothing about types or members, and it catches one shape and no
other.

tools/checks/check-imports.sh refuses a file that names a type and imports no namespace
that has it, which is CS0246, and that is the fault F61 shipped and nothing here saw until
Bader's build failed on it. It has no Autodesk DLL, so for a type this repo declares it
reads the namespace off the file that declares it, and for every other type it LEARNS from
what the rest of the tree imports. What it cannot do is written at the top of it: the FIRST
use of a brand new Autodesk type, in the first file that ever names it, is invisible to it.

A NEW ADD-IN FILE THAT NAMES AN AUTODESK TYPE COPIES ITS IMPORTS FROM THE FILE IN THIS REPO
THAT ALREADY USES THAT TYPE. Where a session cannot compile the add-in that is the only
evidence there is, and a namespace guessed at reads exactly like one that was measured
until a build says otherwise. Where a session CAN compile it, the build is run and the rule
costs nothing anyway.

The pre-commit hook runs both before the tests and Actions runs each of them twice, once
over src and once over tools/checks/broken, which is wrong on purpose in three ways, one for
each of these two and one for check-evidence-ids.sh, so each check is proved to refuse as
well as to pass.

## The live line, F62

- the engine hands progress out through ONE callback and always has. F62 widened what
  that callback carries rather than adding a second route, and everything stays on the
  plugin thread the run is on. Nothing here starts a thread
- `Federator.Core.Diagnostics.LiveLine` holds the shape and every rule about it. The
  add-in owns one per engine and hands it to `ClashRunner`, because the three steps that
  run once per test are opened in there
- `Say` renders every time and `Tick` renders at most once a second. The pieces that
  speak once per test, once per set and once per viewpoint get `Tick`, so a loop over
  1830 tests repaints the window about as often as a person can read it. Anything said
  through `Tick` is in the log as well, so a message the throttle skips is never lost
- RENDERING THE LINE IS WHAT MARKS IT AS SAID. A caller that renders the line itself and
  then hands the result to the throttle gets skipped by that throttle, which is how the
  step name nearly never reached the window. A caller that wants the throttle to render
  hands it a SENTENCE and never a line
- a step running longer than twice what the SAME step took on the group before says so
  on the line. The comparison reads the step records the log already keeps, so the pace
  on the line and the seconds in the timing block are the same numbers. Twice is a
  setting and a value at or below one is refused where it is set
- the line sits in its own row above the log box and outside it, so it never scrolls
  away while the log pane follows the log, and it is trimmed rather than wrapped so a
  long line cannot push the log box down the window mid run
- WHAT IT CANNOT DO, said rather than papered over. It renders when something calls it.
  Every loop in the run ticks it, but a single Navisworks call with no loop inside,
  which is what publishing an NWD is, cannot be ticked from the thread it runs on, so
  the line sits at the seconds it last showed until that call returns

## Rules the code holds

- Each building writes its NWF, NWD and Excel before the next building starts.
  A failure part way through keeps everything already written
- Never clear and rebuild an NWF that already exists. This is the one rule most
  likely to be helpfully undone by someone who does not know why it is here.
  An NWF holds pointers to the NWC files, not copies, so a model updated in place
  needs no rebuild. The clash results live inside that NWF and are the only record
  of what has been fixed. Rebuilding it resets every clash to New and loses every
  Active and Resolved. This tool runs weekly, so that is a week of review thrown
  away each time. Three cases per group:
    no NWF at the output path, build it, there is no history to lose
    NWF there and its file list matches the group, open it, do not clear, do not
      re-append, log OPENED
    NWF there and the file list differs, log CHANGED with the counts, then REBUILT
      naming every file added, moved and removed, read the saved tests, clear the
      document, append the scan in scan order, put the saved sets and tests back
      where the clear dropped them, and only then save the NWF over. Bader decided
      this on 2026-09-07, Q22, so the rule above holds for a matching NWF and a
      differing one is rebuilt rather than left alone
  The file list is read out of the opened NWF. No side file records what went in,
  because a side file can disagree with the NWF and the NWF is the record
- The file list is read from Model.FileName, never from Model.SourceFileName.
  FileName is the NWC, which is what the scan holds. SourceFileName is the
  container the NWC was published from, which on this project is a Revit file in
  Autodesk Docs such as
    Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt
  That can never equal a scanned NWC path, so comparing on it reported CHANGED for
  22 of 22 groups on a run where nothing had changed, and because a CHANGED group is
  left alone entirely, no NWF was reused, no set was built and no test ran. Both
  names still go in the log whenever they disagree, which is what made this
  findable. The rule lives in Federator.Core.Rerun.ModelFileNames so it can be
  tested without Navisworks, which is why it went unnoticed in the first place
- If no test can resolve a set, say so and stop before creating anything. One line
  naming how many sets the document holds and how many the tests name. A run against
  a document holding zero sets once went ahead anyway and finished with 0 created and
  0 run. This is a guard, visible in the log and in the window, never a silent skip
- The Run button does the whole job for every ticked group, model side and clash
  together. With no XML picked, the tests saved in each group's NWF are run where they
  sit, and a group whose NWF holds none runs nothing and the log says so. The clash step
  does one of three things and the log names which, in the same words on the scanned run
  and on the open file run: tests from XML, tests saved in the document, or nothing. The
  rule lives in Federator.Core.Clash.ClashWork.SourceFor. Sets come from the XML and from
  nowhere else, so with no XML the sets in the document are left alone. The two buttons on
  the Clash step are for trying one open model by hand and are labelled as that. They
  are not steps in the run. Splitting one job across three presses is what let a user
  run clash against a document with no sets in it. Since F34 each button calls the one
  engine method the run calls per group, BuildSetsByHand and RunTestsByHand, so the SETS
  and CLASH lines in the log read the same whichever way the work was started, D4, and
  picking an XML runs HealthCheck on it, with the whole summary in the log under HEALTH
  and one sentence under the file in the window, D1
- The NWD publish sets `AllowResave` TRUE, and with it `EmbedDatabaseProperties` true and
  `PreventObjectPropertyExport` false. An NWD published without May be re-saved cannot be
  translated by the ACC and Forma viewer, which is why every NWD this tool published before
  F51 showed a processing error beside it up there, and the other two are the second thing
  Autodesk describe, an NWD reaching ACC with no properties and every object showing as
  solid. All three are on the list read off the installed DLL on 2026-08-29, section 4b,
  each with a getter and a setter, so none of them is assumed. Every publish property this
  run set goes in the log on ONE line before the publish, through
  Federator.Core.Diagnostics.PublishedProperties, because an NWD that will not open is
  diagnosed months later off the log and the file by somebody who cannot read the build
  that wrote it. It records what was SET and never what the type offers, because a name on
  the measured list that this code stopped setting would otherwise read as still set, and
  it is written BEFORE the publish so a publish that throws still leaves behind what it was
  asked to carry. Nothing appears beside the NWF in ACC and that is not a fault: ACC does
  not translate an NWF, because an NWF holds no geometry, only pointers to the NWCs, so
  there is no viewable file to make. Whether the NWF belongs up there at all is Q32
- Republishing the NWD happens in the Build and Open cases, and it is no longer a tick box. It was
  one, on by default, and a weekly run wanted it every time, so it is fixed on. That is the
  point of a rerun. The NWF pointers are unchanged, so reopening picks up whatever the NWC
  files now hold and the NWD is refreshed without the clash history being touched
- The clash test file is picked at run time, every run, and can be from any project.
  Nothing about any one file is written into the code, not names, not counts, not
  property internal names. Those appear in tests as sample data only. A file may hold
  sets, tests, or both, and all three are normal. There is one picker, because Bader's
  file holds both and two boxes meant picking the same file twice
- Per group the order is append, save the NWF, build the sets, create the tests, run
  them, save the NWF again, publish the NWD last. The sets, the tests and the results
  all live in the NWF, so an NWD published before the clash work ships without any of
  them. The NWD used to go first and that is what this ordering fixes. The second
  save only happens when the clash step actually put something into the document
- Nothing is created twice on a rerun. A clash test already in the document under the
  same name is left exactly as it is, because that is where its Active and Resolved
  clashes live, and a set already at its path is left alone too, because a second copy
  would leave two sets at one path and a locator resolving to whichever came first.
  Both are counted and reported as already there, separately from what was created
- A CHANGED group is REBUILT from the scan folder, keeping the tests saved inside it,
  and the rebuild comes BEFORE the units change and the clash step, so from there it is
  an opened group. It used to be left alone entirely, and on 2026-09-07 six groups had
  an NWF built from an older folder with fewer files, 1B06BC 4 of 5 with EL missing,
  1B06K1 1 of 4, so the NWDs went out missing models and nothing rebuilt the NWF. The
  plan lives in Federator.Core.Rerun.NwfRebuildPlan: a file on both sides under a
  different folder is a MOVE, a file only in the scan is an addition, a file only in
  the NWF is a removal, and the files to append are the scan in scan order. The log
  carries one shape, REBUILT with the three counts, one line per file, then one saved
  tests line with three numbers read off the document: how many were read before the
  clear, how many were there after the appends, how many after the copy was put back.
  The copy is DocumentClashTests.CreateCopy and CopyFrom for the tests and the same
  pair on DocumentSelectionSets for the sets, the sets first because a side points at
  a set. The sets are counted by walking the tree, before the clear, after the appends
  and after the copy is put back, and put back on their own count whether or not the
  tests dropped, on one SETS line with the three numbers. Whether Document.Clear keeps
  either is UNKNOWN until a run, so the numbers are read and never assumed. The NWF on disk is only saved over once every test read before
  the clear is back, and otherwise the group fails, says so, and the NWF keeps its file
  list and its tests. A rebuilt group ends as Rebuilt and is judged DONE when everything
  after the rebuild went right, in Federator.Core.Rerun.GroupJudgement. Q22
- The NWF is the record, so a rebuild counts FOUR things out and counts all four back.
  It carries the file list, the sets, the tests, every clash result with the status a
  person set on it, and since F52 the viewpoints, and not one of those has a second copy
  anywhere. A CHANGED group clears the document, so each of the four is counted before the
  clear, counted after the appends, and counted again after the copy is put back, and the
  NWF on disk is saved over only when every one of them came back. Any one short, or any
  one that could not be counted at all, leaves the NWF exactly as it was and fails the
  group. Not counted is not the same as kept: a count that could not be taken reports
  minus one and holds the NWF shut, because a viewpoint count that came back as zero
  would let a rebuild throw them away and report that it kept them all. The keep rule is
  Federator.Core.Rerun.RebuildTally, written ONCE and read by all four, where it used to
  be written twice in the same words for the sets and the tests. The four rows are SETS, TESTS, VIEWS and RESULTS, and the last of
  those is deliberately not called STATUS, because F54 writes STATUS lines about a clash
  moving and one prefix reading as two different things is how a log stops being trusted.
  Which statuses are worth keeping is Federator.Core.Clash.StatusesAPersonSet, which is everything except New,
  because New is where a clash starts and the next run makes it again while the other four
  are somebody's decision. A model CAN be taken out of an open document without a clear,
  measured on 2026-09-19, docs\history\scan.md 5c: Document.RemoveFile(int) and
  TryRemoveFile(int), on Document and not on DocumentModels, which is why searching the
  collection found nothing. WHAT IT COSTS WAS MEASURED ON 2026-09-20, docs\history\scan.md
  5x, and IT COSTS NOTHING: removing a model from 1A02MM left the sets at 62, the tests at
  1830, the results at 526, the statuses at 526 and the viewpoints at 509, and every one of
  them came back whole through a save and a reopen off the disk. So the dance is no longer
  the default. FederationEngine.ReshapeFromScan takes out what is gone and appends what is
  new WITHOUT clearing, and the clear and rebuild is the FALLBACK for a shape the reshape
  cannot do. THE COUNT OUT AND THE COUNT BACK STAYS EXACTLY AS IT WAS, because it is what
  proves nothing was lost and a path that needs no copying still has to prove it. The
  reshape declines BEFORE it changes anything, so the fallback always reads a document
  nothing has touched, and a failure part way through is a fault that stops both. A file is
  removed by its NAME and from the END, because 5x removed the LAST model of four and
  whether removing a middle one shifts the indexes after it is UNKNOWN. Q34
- ONE SAVED VIEWPOINT PER CLASH goes into the NWF, three folders deep, F85, made after the
  clash run and before the NWF is saved again so the viewpoints are inside the file the NWD
  is published from. The plan is Federator.Core.Views.ClashViewpointPlan and nothing about
  it is decided in the add-in. ViewpointBuilder reads every clash the report holds rows for
  in one walk, the status, the two set names off the test's locators, the priority off the
  report, the service size through the same reader the penetration rule uses and the model
  each clashing item lives in, keeps a COPY of the camera Clash Detective frames the clash
  with, TestsViewpointForResult, runs the plan over all of them at once, and writes what
  the plan kept in a second walk. A viewpoint shows the two disciplines of its pair, every
  model of each, plus the model each clashing item lives in, and hides every other model.
  It is written through the COM API's saved view with ApplyHideAttribs on, which is the ONE
  route measured to record both the camera and the hidden state, docs\history\scan.md 5m,
  after the two .NET routes each recorded half: new SavedViewpoint(Viewpoint) the camera
  alone, 5j, and CaptureRuntimeOverrides the hidden state and no camera at all, 5l, which
  is why the first viewpoints run opened every viewpoint on sky. Every written viewpoint is
  read back three ways before it is counted, that it is there, that its camera sits within
  ViewpointSettings.CameraReadBackTolerance of the clash camera, and that it carries
  visibility overrides where it hides a discipline, and one that fails any of the three is
  FAILED with the reason. The hidden state the document held is read once off a capture
  that never enters the tree, 5k, and put back when the group's writing ends, whichever
  way it ends, and read back as hidden. The window's view is never touched. Each viewpoint
  is written with its own dimming and paint and no other, FR-065: what the viewpoint before
  left is taken off first, because both are temporary materials on the document, so one
  whose two items could not both be pointed at carries none and a model hidden for this
  pair carries no dimming from an earlier one, and the VIEWS lines say the fewest and the
  most material overrides the dimmed viewpoints read back. Every folder
  is made outermost first and re-resolved from a fresh RootItem after every AddCopy, which
  is the shape SetBuilder measured for the sets. A viewpoint already at its path is left
  exactly as it is and counted as already there, never made again, because a second copy
  at one path leaves the tree holding both, which is F28's rule for sets. The VIEWS step is
  timed like every other step and it is the one step allowed to move the viewpoint count,
  Federator.Core.Diagnostics.CensusRule. Inside it every call the builder and Record make
  is timed into its own part of Federator.Core.Views.ViewsSeconds, FR-073, so the folders,
  the view, its COM folder and the add each carry their own seconds. The VIEWS block names five and counts the rest,
  because a group puts hundreds in. A group whose viewpoints failed is not DONE.
  THE SWITCH IS THE BOX, F136. Whether a group asks for its viewpoints is Core's
  `ViewpointRequest.WhyNone`, the box on the Clash step, the clash skipped and no report,
  and BuildViewpoints calls it in place of the two checks it held before. Unticked, no
  viewpoint is made, ViewpointsRequested is false so the group cannot fail at them, and
  one VIEWS line names the box, in every group that reaches the viewpoints step. The box,
  x:Name MakeViewpoints, is set off `ReportOptions.MakeViewpoints` in the constructor,
  which runs at every open because the plugin makes the window new each time, the way the
  shared coordinates box is set, so it opens UNTICKED until F114 merges, Q131. Its state
  is named in the RUN SETTINGS lines, at the start of the open file run, and in one RESULT
  line where it was unticked, which the window takes off the run's engine,
  FederationEngine.MakesViewpoints, so RESULT names the state the groups read. It sits up
  front on the Clash step and NEVER under an expander, for the reason the tick box section
  below gives.
  SavedViewpoints.CanBuild is true since the viewpoints round on 2026-09-19 and nothing
  in src reads it, read on 2026-10-05
- THE TEAMS, F131. The add-in holds only the calls, and every rule and every word is Core's,
  .claude\rules\core.md, The teams of the picked file. WHERE A RUN OR A PICK SAYS WHICH FILE IT
  READ, the window writes the TEAMS lines of its map, `TeamMap.Lines`, then a line for each set
  whose name carries no code, `TeamMap.SetLines`, Q117 answered C and A, then, for the scanned
  run and the open file run, whether that map is now the one kept for a run with no XML,
  `TeamMapMemory.Remember`, and only then the MATRIX lines, all in one method,
  SayTheTeamsAndCorrections, so the TEAMS lines come before the MATRIX lines at the pick, the
  two runs and both hand buttons. A RUN WITH NO XML, the scanned run or the open file run,
  writes the kept map's TEAMS lines in their place, Q123 answered B. The map a run hands the
  engine is `TeamMapMemory.ForRun`, the one rule for it. THE KEPT MAP IS NAMED WHEN THE WINDOW
  OPENS, a TEAMS KEPT block beside FOLDERS REMEMBERED, the memory read once by the field
  initializer as FolderMemory is. THE GREY LINE under the Clash XML box, x:Name TeamsLine, is
  `TeamMap.WindowLine` of `TeamMapMemory.ForPick`, the map beside the XML in the box, or the
  kept map where none is there, set at the open and on every change of the box. EACH GROUP
  writes a TEAMS block after the EXPORT CHECK block, in WhatTheModelsCarry, the one place both
  runs pass through, in a try of its own after the export check's, so a fault in it names its
  own step and never counts the group's finished export check as not read:
  `SilentMisses.Find` over the picked XML's sets and the models `ModelFactsReader.Exports`
  read, its `GroupLines` given the document's model count so a model Exports dropped is
  counted. No coverage count is handed in until F127's COVERAGE block is on main, so no
  silent miss is named and each is counted as UNKNOWN until the coverage counts them. The
  two hand presses that federate no group, Undo and Probe, hand the engine no map
- THE MIRRORED TESTS, F132's add-in half, built on 2026-10-07 under Bader's answer A to Q142.
  The add-in holds only the calls, and every rule and every word is Core's, core.md, BOTH
  TESTS OF A MIRRORED PAIR and THE ORDER THE ADD-IN HALF OF F132 KEEPS, which ClashRunner.Run
  follows in that order. THE SIDES: once the set index is built, every test the document holds
  is read again with each side as the set it points at, `SavedTests.Read(Document, Func)` with
  `SavedSideLocator`, which reads a side through the DRIFT block's own `LocatorOf`, the one
  reading of which set a source is, and hands UNKNOWN for a side holding no source or more
  than one, pointing at no set this document holds, or that would not read, counted once on a
  CLASH line. With no XML that plan, the same tests at the same addresses, is the plan run, so
  the report's side locators and the views' set names read the set's path and never the
  placeholder. THE RULE, `PairTheMirrors`: `MirrorRule.Of` over the tests to run, with an XML
  against that document plan and this group's `SetBuildOutcome`, `ClashRunner.SetsBuilt`, which
  the engine hands from `outcome.Sets` after the sets build and before the clash step, the
  priority file and the XML's sets, and with no XML over the saved tests alone, then its
  MIRROR block, its MIRROR RENAMES block and its MIRROR COVERAGE block, the coverage names
  of F127 written as the Core gives them until the sheet exists, and a rule that threw is a
  FAILURE line, no test paired, every test under the plan's name. THE RENAMES, `MakeTheRenames`,
  before any test is found by name or created: each by the saved test's address through
  `TestsEditDisplayName` on a handle resolved for it, its results counted by status before and
  read back after on a fresh handle, both said, since no measurement says what the rename
  keeps, and a test not at its address, not found under the new name after, or whose rename
  threw is left as it is, said, and the name it was to run under is skipped by name, never
  created beside it. Then `WithMirrorsNamed`, then the index of tests by name. EVERY CLASH
  HANDED: `MirrorMerge.Of` over the rule, indexed by the kept test's name and each mirror's run
  name, and the harvest of a test in a merge hands every clash as the rows are read,
  `ClashHarvest.MergeAsKept` and `MergeAsMirror`, each clash under a group on its own, for the
  kept test with the group's row and its items read for their keys alone, for a mirror with a
  row of its own read whole, `ClashItem.MergeKey`, the one key, read off the file, the id and
  the geometry name the harvest read, UNKNOWN with no id. A clash that would not hand is a
  FAILURE line and the merge fails closed by its own rule. AFTER THE RUN AND NEVER BEFORE,
  `MergeTheMirrors`: `AddTo` per merge, its MIRROR MERGE block, the kept test's ROWS line with
  `AddedToTheKeptTest`, held back from the harvest for it alone, then the pictures of every
  test in a merge, `RenderThePicturesWaiting`: no picture of such a test is rendered while its
  rows are read, `ClashHarvest.Deferred` records each row with the index path of its result,
  `RowUnderTest`, and after the merge each row the report still holds is rendered under the
  test the report holds it under, the result resolved again by the test's address and that
  path, `PictureLater`, so a mirror taken out gets none and a row only a mirror found is
  pictured under the kept test. The BLOCKS check is handed `ClashReport.MirrorsMerged`. The
  views run after the clash step and read the merged report. No handle is held across
  `TestsEditDisplayName`, `TestsRunTest` or any mutator, and every wrapper is disposed. The
  mirror rule's ending is `ClashRunner.Mirrors`, a `MirrorSettings` at its default
- SINCE F85 A VIEWPOINT IS PER CLASH, which REVERSES what this rule said before it. It
  said no clash is ever saved as a viewpoint and that a discipline viewpoint is not a
  clash viewpoint. The first real run answered the question the other way: the thing a
  person presses has to be the thing they are looking at, and a viewpoint showing a whole
  discipline does not take anybody to a clash. The plan is
  `Federator.Core.Views.ClashViewpointPlan`, three folders deep, written by ViewpointBuilder
  since the viewpoints round on 2026-09-19. Do not read the old sentence out of an older
  log or an older comment and undo this
- A CLASH carries one of five, New, Active, Reviewed, Approved or Resolved. A TEST carries
  one of four, New, Old, Partial or Complete. They are different sets on different things
  and they share only the word New, which is how they get confused. OLD IS A TEST WORD AND
  NEVER A CLASH WORD, so no clash is ever at Old and nothing here ever moves one from it.
  The wording is Federator.Core.Clash.StatusWords so the log and docs\workflow.md cannot
  drift apart. This tool sets REVIEWED and nothing else, which
  Federator.Core.Clash.StatusesThisToolMaySet decides and a refused status is logged by
  name rather than silently dropped. Approved and Resolved are never set, because each is a
  person's statement about work that was actually done and the NWF is the only record of
  what has been fixed, so there is nothing to check it against afterwards. New and Active
  are not set either, because running a test produces them and setting one by hand would
  overwrite what a person had already decided. The status is applied BETWEEN the run and
  the harvest and never after the clash step, because ClashHarvest reads a result's status
  while it builds the report rows, so a status applied later would leave the workbook and
  the page carrying the status read before the change. It gets its OWN resolve, because
  TestsEditResultStatus is a mutator and every mutator on DocumentClashTests is a copy form
  that kills the handle handed to it, and sharing a handle with the count is the fault that
  threw once per test for 8 hours 52 minutes. A status written is a write, so the NWF is
  saved again on it. HOW THE TOOL LEARNS WHICH CLASHES CANNOT BE SOLVED WAS Q33 AND IS
  ANSWERED, on 2026-09-19, by the second of the four shapes that question offered: a rule
  over the clash itself. F72 supplies the list. Penetrations in the add-in walks the
  results of one test, reads a category off each side with the reader ClashHarvest already
  has and the service size with ItemSizes, and hands the facts to
  Federator.Core.Clash.PenetrationRule, which decides. It is OFF by default, so a run that
  did not ask for it resolves nothing and walks nothing and costs what it cost before F72.
  The pass only READS, and the list it hands back goes through ClashStatusEditor, so there
  is still exactly one place that calls TestsEditResultStatus. Two walks rather than one is
  the price of that, and it is worth paying because the mutator kills the handle given to
  it and a walk that both read and wrote would be holding one across it
- Applying the file's settings to a test already in the document is a tick box, off by
  default. IT DOES NOT RESET ANYTHING, and this rule said for months that it did. 5y
  measured it on 2026-09-20, docs\history\scan.md 5y: setting a tolerance on a saved
  test, the same value or a doubled one, keeps every result and every status a person
  set, through a save and a reopen off the disk, and 175,434 saved tests across his runs
  had had one set on them while the confirm screen told him in capitals it reset them
  all. WHAT IT ACTUALLY COSTS is different and real: the test finds DIFFERENT clashes the
  next time it runs, so a clash somebody marked Reviewed may not come back, and a clash
  nobody has seen may appear. That is why the default is still to report and not to act.
  What drifted is worth knowing every week. Overwriting it is worth doing once
- There is no stale marker on the clash API. Nothing named stale, altered, out of date,
  dirty or needs rerun exists on any type in Autodesk.Navisworks.Clash, public or
  private, measured on 2026-08-31, see docs\history\scan.md section 4j. The only thing there is
  ClashTest.Status, a four value enum of New, Old, Partial and Complete. What puts a test
  into Old is UNKNOWN and cannot be read off the DLL, so the status is logged as itself
  and no sentence is put on it. Never translate Old into "your models have changed". The
  Status setter is public and nothing here writes it, because writing it would be
  claiming to know the rule that is UNKNOWN
- Compact is reachable, DocumentClashTests.TestsCompactAllTests is public, and it is a
  tick box off by default that runs after the tests and before the workbook is written.
  Never compact silently. It removes every Resolved clash from the NWF, the NWF is the
  only record of what has been fixed, and there is no second copy anywhere, so it cannot
  be undone. The count is announced before it runs and the count removed is reported
  after. The no argument form is used, because TestsCompactTest takes a ClashTest and is
  a mutator, so the handle passed to it dies the way section 4g describes
- A clash side is pointed at the saved set through CreateSelectionSource, not filled
  with a copy of the set's items. That is what Clash Detective does when a person
  picks a set in the panel, and it is what keeps the counts matching the panel. A
  copy is a snapshot that can drift from the set the panel shows
- Everything that threw for one group is kept in a list, never in one slot. The model
  side can append cleanly and the clash step still throw, and one slot kept whichever
  wrote to it last and silently lost the other
- A handle onto anything the document owns is borrowed, never kept. Every SavedItem read
  out of a collection is created with eEXTERNAL ownership over a weak reference, so it
  dies the moment the document replaces the object behind it, and every mutator on
  DocumentClashTests is a copy form that does exactly that. TestsRunTest is one of them,
  so the test handed to it is dead when it returns and reading Children off it throws
  ObjectDisposedException (WeakRef). A test is addressed by where it sits, resolved again
  before every use, and disposed after. One run threw that exception once per test for
  8 hours 52 minutes and produced nothing
- Nothing walks a whole collection once per item. Looking a test up by name after every
  add was O(n squared) over 1830 tests and built about 1.7 million finalizable native
  handles per group. TestsAddCopy appends at the root, so the new test is at the index the
  count held before the add, checked by name rather than assumed
- What this tool creates or resolves, it disposes. All of ClashTest, ClashResult,
  ClashSelection, SelectionSet, SelectionSource, Selection, ModelItemCollection, Search
  and SavedItem are IDisposable. Disposing an eEXTERNAL wrapper releases the wrapper and
  never the document's object. SavedItemCollection is not disposable and is never disposed
- One picker, one file. The file can hold sets, tests, or both, and the tool reads what is
  in it. A file holding only tests still works against sets already in the model
- The naming boxes are filled from the patterns BEFORE anything can read them back.
  Filling the grouping combo sets SelectedIndex, which fires its handler, which regroups,
  which reads the boxes into the patterns. With the boxes still empty that replaced every
  default with nothing, and the window then opened with all fifteen naming boxes blank, so
  every field had to be typed by hand. That is where MOD-00001 came from instead of
  MOD-000001. The defaults were always six digits in the code and never reached the window
- Every default the window shows is read from the Core object that holds it, in the
  constructor, and is never typed into the XAML as well. The photo size, the cap, the paste
  into cells tick and the five status ticks are read from ImageOptions, which is where the
  1024 measured off the accepted report lives. Two copies of a default drift, and the XAML
  copy is the one nothing can test. The tick box that skips the clash for models off the
  shared coordinates starts from AlignmentCheck.DefaultSkipClashOffCoordinates beside its
  label, because the XAML once carried IsChecked True and that copy was the real default
- A tally of what one run did is the engine's, and the window hands it to that run's
  RESULT block, because the engine is made for one press of a button and the log lives as
  long as the window. The tally of models off the shared coordinates lived on the log until
  F112's second attempt, and the second run of a window listed the first run's groups again
  under the second run's rule state. RunJobs and OnRunOpenDocument hold the engine outside
  their try, so the finally passes engine.CoordinatesAcrossTheRun, or null where no engine
  was made, to WriteTheResultAndCopyTheLog
- A step whose content is taller than the window scrolls. Only the Outputs step is,
  measured at 1024x680, 1280x800 and 1600x1000 with tools\probes\probe-window-scroll.ps1 and
  cut off at all three, which is why the naming table could not be reached. The other three
  fit at all three sizes and are left alone, because a ScrollViewer around a step whose grid
  is meant to fill stops it filling
- The scan and its checks live in the Source step, so pressing Scan reports what was
  found and what is wrong with it in one place. A plain count line first, files found,
  files readable, groups and the findings by kind, then the findings themselves
- Copy findings puts them on the clipboard as tab separated rows with a header, so
  pasting into Excel gives a table. A run with nothing odd still copies a header and
  one row saying so, because an empty clipboard reads as a failed copy
- The add-in resolves its own assemblies from its bundle folder by simple name,
  ignoring the version, through an AssemblyResolve handler registered in the static
  constructor of FederatorPlugin. This is not belt and braces, it is the only thing
  that works. Six references bind to a version one build number away from the file
  that ships, .NET Framework binds a strong named assembly by exact version, and the
  binding redirects NuGet writes go in an application config that a Navisworks add-in
  does not have. Every file was already present when a run failed on this, so adding
  files fixes nothing. The rule lives in Federator.Core.Diagnostics.BundleAssemblies
  so it can be tested, and it is proved by a console exe with its config deleted, see
  docs\history\scan.md section 4i
- install.ps1 copies the bundle CONTENTS, never the bundle folder. Copy-Item of a
  directory puts it inside the destination when the destination exists and creates it
  when it does not, so the same line does two different things depending on whether
  the remove before it has finished. That left ParsonsNwcFederator.bundle inside
  ParsonsNwcFederator.bundle, which Navisworks does not read at all, and it is checked
  for afterwards
- install.ps1 walks what is actually in the bundle after copying and refuses to finish
  if any reference is satisfied by neither the bundle, the framework, nor the
  Navisworks folder. A missing file is caught at install rather than ninety seconds
  into a run. The version mismatches are printed too, labelled as expected, so nobody
  reads them as faults
- The NWF is looked at ONCE MORE after the NWD is published, because the NWD is published
  last and the NWF is the only record of what has been fixed. The line says intact and the
  size, or CHANGED with both sizes, or GONE. Nothing was checking this, so a run that did
  destroy the clash history would have finished quietly. It writes nothing and changes
  nothing, so it cannot itself break a group
- The Try forms return a bool and it is read, never discarded. For the NWD that
  bool is the only thing separating a fresh publish from last week's file at the
  same path, because File.Exists cannot tell them apart
- A property value is read by its KIND, never with ToDisplayString alone. Every accessor
  on VariantData is kind specific and throws on any other kind, and a Revit element id is
  an Int32, so ToDisplayString threw 426 times on one run and took the whole of Describe
  with it, losing the element id, the source file and the discipline together. The one
  member that returns a value regardless of kind is ToString, whose IL switches on
  GetDataType, but it prefixes the kind and hands back "Int32:702888". So the kind is read
  and the right accessor called, with ToString as the fallback for a kind nobody has seen
  and its prefix stripped. Measured, see docs\history\scan.md section 4n
- Anything that can throw per item goes in its own try. The source file and the discipline
  are read last and were lost to a throw three properties earlier, on all 426 items, which
  is one throw costing three columns
- Setting the models is still done, BEFORE the clash step, and it is kept because it fixes
  what the person sees in Navisworks. Measured on 2026-09-01 across all 4027 types:
  Document.Units is read only, Model.Units is read only, and the only public managed member
  that sets units at all is
  DocumentModels.SetModelUnitsAndTransform(Model, Units, Transform3D, bool). So each MODEL
  can be set and the DOCUMENT cannot. Whether Document.Units follows is UNKNOWN and no
  longer matters, because it no longer decides the report. What Document.Units actually
  reports is UNKNOWN too: setting a model writes a per file override, the same one the
  Units and Transform dialog shows, while what the scene is measured and displayed in is
  an application option, Options, Interface, Display Units, which Autodesk documents as
  what tolerances for clash detection are set in. Whether Document.Units reads that option
  is not readable off the DLL, so it is not claimed. The words DID NOT FOLLOW are gone from
  the log. The combo on the Outputs step is MODEL units, defaulting to Meters, and its help
  line says the report is always in metres
- A tick box has to earn being a decision. How many there are and how many sit up front is
  counted once, in What a tick box says below. Republishing the NWD, writing the client page and rendering the
  photos are fixed ON, because a weekly run wants all three every time. Client columns only
  is gone outright, dead since the workbook became one sheet with none of ours on it.
  Dating the NWD, the clash XML, the thumbnails and the five image status boxes are
  collapsed under More, rarely changed. The two that destroy data are collapsed on their
  own under Things that destroy data, because they do not belong beside ordinary output
  options. Fewer decisions is the goal, not more words explaining them

## What a tick box says

A tick box is a SHORT LABEL and one grey line under it. Bader called the long ones a
nightmare and he was right: they ran off the edge of the window, they shouted in capitals,
and they explained the off state as well as the on state, so nothing stood out.

- the label is at most EIGHT words and names the thing, not the mechanism
- one grey help line under it, at most TWELVE words, saying what it costs
- no capitals for emphasis anywhere. Acronyms a person says out loud are fine
- do not describe the off state unless it changes what someone would choose
- the two that destroy something carry a red exclamation mark beside the label. A mark,
  because when every label shouted, none of them did

The numbers in a help line are measured, never estimated. Photos are about 0.08 seconds
each and 213 took 17 seconds. Pasting them takes the workbook from 0.3 MB to 52 MB.

There were fifteen, then eleven, and F72 made twelve, of which two were visible without
opening anything. F72b, Q72, Q99 and F136 each added one up front on the Clash step, so
src\Federator.Addin\Ui\FederatorWindow.xaml holds SIXTEEN, of which SIX sit under no
expander, by x:Name IncludeSubfolders on the Source step and MarkPenetrations,
MarkByDesign, RebuildDriftedSets, SkipClashOffCoordinates and MakeViewpoints on the Clash
step. The
other ten are eight under More, rarely changed and two under Things that destroy data. Read
off the XAML on 2026-10-05 by its CheckBox and Expander lines. probe-window-labels.ps1 was
not run on this count. A box only stays if a normal weekly run genuinely has to choose, and
Mark penetrations as Reviewed earns it: it writes statuses into the NWF, which is the only
record of what has been fixed, so a run has to be told to do that rather than told not to. Everything else became a
fixed behaviour with the sensible answer chosen, or moved under an expander. The two that
destroy data are under one of their own, because they do not belong beside ordinary output
options. Which box went where and what each removed one was fixed to is in docs\history\scan.md 4q.

tools\probes\probe-window-labels.ps1 reads every tick box out of the real window and checks all
of this, so the limits are proved rather than remembered. It OPENS every expander first,
because a collapsed one has no visual tree behind it and the probe found one box and
reported no problems, and it says how many are visible without opening anything, which is
the number the window is judged on.

THE VIEWPOINTS BOX NEVER GOES UNDER AN EXPANDER, F136, for the same reason. The loop's test
runs leave the viewpoints off with run.ps1 -Untick MakeViewpoints, F126, and the driver,
tools\probes\drive-window-run.ps1 FindOnTabs, finds a box by its name in the visual tree of
each tab in turn. A collapsed expander has no visual tree behind it, so a box under one is
found on none of the tabs and the driver stops UNTICK with nothing pressed. Once F114 merges
the box opens ticked, Q131, and a test run that must leave the viewpoints off can only do it
through a box the driver can find.
