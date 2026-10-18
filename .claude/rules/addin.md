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
  through `Tick` is in the log as well, so a message the throttle skips is never lost, bar
  one since F143: the harvest's sentence for each row reaches the log as a PROGRESS line
  at most every 30 s, since a line per row would be 1556 lines for one test
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
- EACH PROJECT HAS A BUILT IN MATRIX PACK THAT BADER SUPPLIES, F155, Bader's decision of
  2026-10-13. It replaced the clash test file picked at run time. The pack is a folder of
  embedded resources holding the XML, its team map, its list of corrections and the two CSVs,
  core.md THE CLASH MATRIX IS BUILT IN TOO. Nothing about any one file is written into the
  code, not names, not counts, not property internal names. Those appear in tests as sample
  data and in the pack as the project's data only. A file may hold sets, tests, or both, and
  all three are normal. There is no picker: the one pack is read and a PACK line names it,
  and since F166 a run chooses its pack by the project code of its NWC file names, PackChoice, Q164 B, and stops before anything is opened where none or two match
- Per group the order is append, save the NWF, build the sets, create the tests, run
  them, build the viewpoints, save the NWF again, publish the NWD, then take the pictures and write the
  reports that link them, F182. The sets, the tests and the results
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
  after the rebuild went right, in Federator.Core.Rerun.GroupJudgement. Q22. THE RESHAPE
  SAVES THE NWF THE SAME WAY, through WriteNwf once all four came back, FR-043, where it
  saved nothing and a group whose clash step and views put nothing in left the old file
  list in the NWF. Every save of the NWF counts on the outcome, `NwfSavesAsked` and
  `NwfSavesThatFailed`, a false return and a throw alike, and hands its bool to
  `RunLog.WriteFinished`, FR-044 and FR-045, and the publish of the NWD hands its own,
  FR-189
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
- ONE VIEW PER CLASH TEST OF ITS OPEN CLASHES goes into the NWF, in folders by priority and
  team pair, F114's add-in pass, built on 2026-10-08 on the probes P5 to P19 of
  docs\history\scan.md 5z-h to 5z-z, made after the clash step and before the NWF is saved
  again so the views are inside the file the NWD is published from. The add-in holds only
  the calls, and every rule and every word is Core's, core.md ONE VIEW PER CLASH TEST, ONLY
  WHAT THIS TOOL MADE and THE VIEWS TREE BLOCK. THE ORDER IS THE DESIGN'S S2, so a run that
  stops part way leaves the old picture or a checked new one and never neither: a fresh walk
  of the tree, `SavedViewpoints.ReadTree`, walk one, the plan, the views, a fresh walk and the
  inventory over it, the removals, the document put back, a last fresh walk for the block.
  WALK ONE KEEPS F132'S SHAPE, THE MIRRORED TESTS below: the rows off the merged report
  through `ReportClashes.Of`, each resolved by its `RowAddress` through
  `TestAddress.ResolveIn` and `ResultPath.ResultAt`, never by name off the document, and what
  each row becomes is `ReportClash.ToView`, a `ViewClash` with the two items' index paths,
  plain ints and never handles, the clash centre off `ClashResult.Center`, the model each
  item lives in by the topmost ancestor's `Model.FileName`, 5n, and the size of the larger
  service through `Penetrations.ServiceSizeOf` ONLY where the test's pair carries the size
  folder, row F114-K9, so `ViewTeams` is built before the walk. ONLY A TEST READ WHOLE GETS A
  VIEW, attempt 2 on the breaker's B1: `Federator.Core.Views.TestsRead` keeps per test whether
  it was found at its address, every IN SCOPE row led to its result and nothing threw in its
  walk, an in scope row with no recorded place making its test not whole too. Every call is
  keyed by the REPORT's test name of the row, the kept test for a clash only a mirror found,
  and a resolve that fails or throws marks every report test among the rows it serves, each
  test judged once every resolve is done, the second reading's N1, so a kept test is whole
  only where its own rows and its mirror's were read. A row outside the views' statuses is
  handed to the plan unread, its status checked before `ResultAt`, so a Resolved row Compact
  removed never makes its test not read, C1. A test the clash step ran with no row on the
  report is read whole with no row, `TestsRead.Ran`, N2, and its old view goes as no longer
  needed. A test the clash step skipped as FAILED is never read whole, and nor is the kept test
  of a merged mirror it failed on, `TestsRead.ClashStepFailed` handed the clash record's skipped
  tests and `ClashRunner.KeptOfMirror`, row F114-K38, since its harvest may have thrown after
  reading only closed rows, so its old views stay and it is counted failed with why. A result group row is always read and the clashes INSIDE it are planned each at its own
  status and name in its test's view, `ReportClash.ToViewOfMember`, F114-K36, Bader's answer B of
  2026-10-13, which retired the lead's F3. A test of groups alone is whole when its groups were
  read whole. A test not read whole hands the
  plan none of its rows, is counted FAILED on the VIEWS BUILT block with why, so the group is
  not DONE on it, and keeps its views of earlier runs, since the inventory is handed
  `TestsRead.WholeNames` as the tests read and never the tests the clash step ran, which
  stay the tree facts' `TestsRun`. The camera is read after the
  plan for each view's camera clash alone, `TestsViewpointForResult` on a copy, where F85
  read one per clash. A VIEW OF NOTHING IS NO VIEW, B2: a view none of whose clashing items'
  models could be read or named a model of the group is counted FAILED with
  `ShownModels.WhyNoView` before anything is hidden or dimmed for it. EVERY CAMERA IS READ
  FIRST, F141 attempt 2, P18's shape: once the hidden state is read and before the first view is
  touched, each planned view's camera clash is read once through `CameraOf`, `TestsViewpointForResult`
  on a copy, held by the view's key and disposed when the views end, each release in its own try,
  and a view with no camera is failed with why before anything is hidden or dimmed for it. Per view
  the held camera is framed, or copied for a view of one clash, after the hiding, the undim, the dim
  and the paint, so none of `CameraOf`'s three clash calls, the test resolved, the result resolved
  and `TestsViewpointForResult`, is made while a view's dim and paint stand. F85, which set 04 ran at
  e4484d1, read every camera in its walk too and read 2813 views back painted. Set 07's 1A02MM,
  making those calls per view between the paint and the record, wrote 69 views and read back no
  material override on any, the hidden models kept, and that one of them takes the dim and the
  paint off is read off that difference and not measured alone. PER VIEW, each stretch
  timed into its `ViewsPart`, FR-073: the hiding
  by `ShowOnlyModels` over the indexes of `ShownModels.Shown`, skipped where the view before
  showed the same models, P18, the undim of what the view before left, FR-065, one
  `DimAllBut` over the shown roots with ONE reset over the collection of every clashing item,
  one `OverrideTemporaryColor` per colour over the red and the green collections of
  `PaintPlan`, P17, `FramingBox` over the clash centres and `ZoomBox` on a copy of the camera
  clash's camera held since the cameras were read, P16, or a plain copy of it for a view of one
  clash, THE CAMERA A COPY IS MADE FROM KEPT UNTIL THE COPY IS DISPOSED, as P16 kept it, F141:
  set 06's run of 1A02MM on 2026-10-11 disposed it first and every one of its 47 framed views
  threw ObjectDisposedException on the copy's Position, steps\runs\06\item1-C02, while a copy
  made by `CameraOf` inside a using of the viewpoint `TestsViewpointForResult` gave read whole
  in all 14 views of one clash, and P18 held such a copy across 20 views and a removal and copied
  it per view, so which sources a copy outlives is read off those three and not assumed for any
  other, `EnsureFolders`
  REUSING a folder at its path and returning the depths it made so each folder the tool
  makes is marked and no other, row F114-K5, the record through the COM view with
  ApplyHideAttribs and ApplyMaterialAttribs, 5m, and the mark by
  `DocumentSavedViewpoints.AddComment` after the add with a comment from
  `Document.CreateCommentWithUniqueId` and the `MarkAuthor` setting, P9, on the view found as
  the child of its folder with its name and no mark, Ordinal, P12. WHERE A PERSON'S UNMARKED
  VIEW OF THAT NAME ALREADY SITS IN THAT FOLDER, read by `CountUnmarked` before the record,
  none is written there, the view is counted as failed with that reason, naming the folder
  and saying the unmarked view is a person's or one an earlier run could not mark, and that
  one is left as it is, P22 unrun and no new probe. A VIEW RECORDED THAT COULD NOT BE MARKED
  IS REMOVED AT ONCE, B3: `Mark` runs AddComment inside a try of its own and says what threw,
  and where it returns minus one `SavedViewpoints.RemoveUnmarked` takes out the one unmarked
  view of that name in its folder by `RemoveAt` with the parent resolved fresh, exactly one
  or it is said, and the view is counted FAILED naming whether it was removed, so no
  unmarked view of the tool's is left to block that test every later week. THE READ BACK IS
  COUNTS AND NEVER FLAGS, 5o,
  `SavedViewpoints.ReadBack` at the index the mark returned: the camera within
  `CameraReadBackTolerance`, the hidden roots as their `Model.FileName`, P19, the material
  overrides walked ONCE into a lookup by item path, the colour each clashing item will show,
  the override's or its own, 5p, and the comments and redlines for `ToolViewMark.Judge`,
  which must read the view as this run's. A view that fails any is FAILED with the reason, a
  `WrittenView` not read back, and the inventory removes it at once. THE INVENTORY is
  `ViewsInventory.Plan` over the fresh walk after, with the tests read whole, B1, so a test
  the clash step ran and walk one could not read keeps its views, row F114-K12, the test
  names of the document and the XML, the map's
  known codes and the folders that held nothing before off the walk before,
  `ViewNode.EmptyFolderKeys` and `HeldNothing`, its lines a VIEWS INVENTORY block. EACH
  REMOVAL, `SavedViewpoints.RemoveOne`, resolves the parent fresh, re-finds the item by its
  name, its kind and whether it carries a mark just before, at its index or once among its
  siblings, and calls `RemoveAt(parent, index)`, P13, a folder with everything under it in
  the one call, P14, and the folder's count after is read and said where it did not fall by
  one. Each removal's result is a `RemovalOutcome`, made, refused or thrown with why, one
  list handed to the tree facts as `Removals`, B5, and a RemoveAt that returned is removed
  only where its folder fell by exactly one, `RemovalOutcome.Counted`, anything else recorded
  as not removed with what happened, F4, the same rule `CountWords` gives the FAILED words of a
  view that could not be marked. A walk's comments, redlines or camera that
  would not read are counted and said, and the item handed with that part unread, which the
  judge keeps as a person's: an unread comment list is `ViewNode.CommentsNotRead`, null and
  never an empty list, B4, and the walk line says such items are kept as a person's because
  their comments could not be read. Items
  enumerated off a collection are not disposed one by one, P18's measured shape, since
  F85's fifth run threw on that. THE VIEWS TREE BLOCK is `ViewsTree.Lines` over
  `ViewsTreeCheck.Of` on the last walk, cut for the .log at `TreeLinesInLog` and whole in
  the .tsv, one `RunLog.Row` a line. A FAILED check is a FAILED line and the group keeps its
  own result. The hidden state the document held is read once off a capture that never
  enters the tree, 5k, and put back when the group's writing ends, whichever way it ends,
  the dimming and the paint taken off the roots, and read back as hidden. The window's view
  is never touched. The VIEWS step is the one step allowed to move the viewpoint count,
  `CensusRule`, and the design's census stop is NOT built, the VIEWS TREE checks and
  `CensusRule` cover it. `Penetrations.ReadSide` walks up inside its try and a parent that
  would not read is said through an out every caller counts, FR-072.
  `FederationEngine.BuildViewpoints` hands the builder the run's map, or `TeamMap.NoXml`
  said on a VIEWS line where none, the picked XML's sets, the group's models as `ModelTeam`
  in the document's order, which is the index a view hides by, the tests the clash step
  ran, the test names of the document and of the XML, the mirrors' run names off
  `MirrorRule.Pairs` through `ClashRunner.MirrorNames` and `JobOutcome.MirrorNames`, null
  where no rule ran and said, and whether the clash step was sound, and asks for the second
  NWF save where a view was written or an item removed. A group whose views failed is not
  DONE. THE SWITCH IS THE BOX, F136. Whether a group asks for its views is Core's
  `ViewpointRequest.WhyNone`, the box on the Clash step and no report,
  and BuildViewpoints calls it in place of the two checks it held before. Unticked, no
  view is made, ViewpointsRequested is false so the group cannot fail at them, and one VIEWS
  line names the box, in every group that reaches the viewpoints step. The box, x:Name
  MakeViewpoints, is set off `ReportOptions.MakeViewpoints` in the constructor, which runs
  at every open because the plugin makes the window new each time, the way the shared
  coordinates box is set, so it opens TICKED since F114's add-in pass, Bader's answer B to
  Q131, unticked until F114 merged and ticked once it does. Its state is named in the RUN
  SETTINGS lines, at the start of the open file run, and in one RESULT line where it was
  unticked, which the window takes off the run's engine, FederationEngine.MakesViewpoints,
  so RESULT names the state the groups read. It sits up front on the Clash step and NEVER
  under an expander, for the reason the tick box section below gives. The loop unticks it
  through run.ps1 -Untick MakeViewpoints for a run that is to make no view.
  SavedViewpoints.CanBuild is true since the viewpoints round on 2026-09-19 and nothing
  in src reads it, read on 2026-10-05
- THE TEAMS, F131. The add-in holds only the calls, and every rule and every word is Core's,
  .claude\rules\core.md, The teams of the matrix. WHERE A RUN OR A PICK SAYS WHICH FILE IT
  READ, the window writes the TEAMS lines of its map, `TeamMap.Lines`, then a line for each set
  whose name carries no code, `TeamMap.SetLines`, Q117 answered C and A, and only then the MATRIX lines,
  all in one method, SayTheTeamsAndCorrections, so the TEAMS lines come before the MATRIX
  lines at the window's open, the two runs and both hand buttons. Since F155 no map is kept
  for a later run. A RUN WITH NO XML, the scanned run or the open file run,
  writes the kept map's TEAMS lines in their place, Q123 answered B. The map a run hands the
  engine is `TeamMapMemory.ForRun`, the one rule for it. THE KEPT MAP IS NAMED WHEN THE WINDOW
  OPENS, a TEAMS KEPT block beside FOLDERS REMEMBERED, the memory read once by the field
  initializer as FolderMemory is. The Clash XML box and its grey line, TeamsLine, are gone since F155. EACH GROUP
  writes a TEAMS block after the EXPORT CHECK block, in WhatTheModelsCarry, the one place both
  runs pass through, in a try of its own after the export check's, so a fault in it names its
  own step and never counts the group's finished export check as not read:
  `SilentMisses.Find` over the picked XML's sets and the models `ModelFactsReader.Exports`
  read, its `GroupLines` given the document's model count so a model Exports dropped is
  counted. No coverage count is handed in, because F127's COVERAGE block does not count the
  categories no set catches in this build, so no silent miss is named and each is counted as
  UNKNOWN until the coverage counts them. The
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
  of F127 written as the Core gives them, which the Coverage sheet's MIRRORED PAIRS section
  points at and does not copy, and a rule that threw is a
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
  `MergeTheMirrors`: `AddTo` per merge, its MIRROR MERGE block and the kept test's ROWS line with
  `AddedToTheKeptTest`, held back from the harvest for it alone. Then, in a finally so a throw
  out of the merge loses none, every picture, `RenderThePicturesWaiting`, F149: no picture is
  rendered while rows are read, `ClashHarvest.Recorded` records every row of every test with
  the index path of its result, `RowUnderTest`, and each row the report still holds is pictured
  under the test the report holds it under, the result resolved again by the test's address and
  that path, `PictureLater`, so a mirror taken out gets none and a row only a mirror found is
  pictured under the kept test. The BLOCKS check is
  handed `ClashReport.MirrorsMerged`. THE VIEWS READ THE MERGED REPORT AND NEVER THE DOCUMENT
  BY NAME, attempt 2 of 2026-10-08 on the breaker's finding R4: `ViewpointBuilder.Collect`
  takes `Federator.Core.Views.ReportClashes.Of(report)`, the rows the merged report holds under
  the test it holds them under, each at its row's status, which the merge restated by Q138 B,
  and resolves each row's result by the `RowAddress` the runner recorded for it as the row was
  read, `ClashRunner.RowAddresses`, handed to the views through `JobOutcome.RowAddresses`: the
  test once per address by `TestAddress.ResolveIn`, the one resolver the runner's `Resolve`
  uses, and the result by `ResultPath.ResultAt`, the one walk `PictureLater` uses, which reads
  the display name back and refuses a result not named as the row, since `TestsCompactAllTests`
  after the merge can move a result from under a recorded path. Since the reviewer's reading of
  attempt 2 the last level is `Federator.Core.Views.ResultSiblings.Pick`: the compact removes
  every Resolved result before the views run and moves every live result after one an index
  left, so a row not named at its recorded index is taken from the one sibling carrying its
  name, refused with the count where none or more than one does, and the rows so moved are
  counted on a VIEWS line. A ROW WHOSE RESULT HAD NO NAME OF ITS OWN, `ClashRow.NoNameOfItsOwn`,
  set by `ClashRow.NameAfter`, carries the word its kind is given, as every other unnamed
  sibling does, so a name match proves nothing about it, F149's second reading:
  `ResultSiblings.PickUnnamed` takes it at its recorded index only where `ResultPath.ResultAt` is
  told the index is exact, which the pictures, taken before the compact, say, and refuses it for
  the views, which say no, so it is counted and said on their line of rows not found, as it was
  before F149, and never framed and painted on the next unnamed result. So a mirror taken out of the
  report gets no view, a clash only a mirror found is viewed under the kept test and named as
  the workbook names it, and a row whose result is not found again, or with no address, is
  counted on a VIEWS line and gets none. A group row is one view framed on the group, as the
  workbook holds it, with no size read and not dimmed, since a group has no two items. THE BY
  DESIGN PASS ON A RUN WITH NO XML, attempt 2 on the breaker's finding R5, the lead's Q144:
  the saved tests' sides being the sets they point at since F132, `ByDesign.WantedFor` reaches
  the tests saved in the NWF, where before F132 the placeholders matched no pair. The pass
  stays, since the box is a person's instruction and F72b matches on the two set names of the
  test, and it is never silent: where the plan is the document's, `ClashRunner.Run` calls
  `ByDesignTally.SidesReadOffTheSavedTests` with the saved test count and the pairs in the file,
  so the BY DESIGN block's first line says the sides were read off the NWF and how many pairs
  that reached, and `FederatorWindow.ConfirmClear` carries `ByDesignPairs.ConfirmLine` with the
  box on, beside the rebuild line. The penetration pass reads no locator, `Penetrations.WantedFor`
  reads each clash's two items, so it reached the saved tests before F132 as it does now and
  nothing changes for it. The status guard, `StatusesThisToolMayMoveFrom`, is kept. No handle
  is held across `TestsEditDisplayName`, `TestsRunTest` or any mutator, and every wrapper is
  disposed. The mirror rule's ending is `ClashRunner.Mirrors`, a `MirrorSettings` at its default
- THE PICKED NWFS RUN THE OPEN FILE RUN'S ROUTE, F129, Bader's request 4 under Q112, and
  every rule and every word is Core's, core.md, THE PICKED NWFS. Read off the code on
  2026-10-08 before anything was built: the scanned run reaches an NWF only through Decide,
  which compares its file list against a scan, and the open file run already runs an NWF where
  it sits with no scan and no Decide, through FinishTheGroup, the tail both share. So
  FederationEngine.RunOpenDocument and RunPickedNwfs both run each file through ONE method,
  RunAnOpenFile, and nothing of that route is copied: the check by OpenDocumentJob.CanRun
  before any open, the report folder, the OPEN lines, the census, Decision Open, nothing
  appended and nothing cleared, the tests of the XML where one is picked and otherwise the
  tests saved inside, the workbook, the NWD, the NWF looked at once more, the GROUP lines and
  the PICKED NWF or OPEN FILE block. For a picked NWF the one difference is the open,
  OpenThePickedNwf, timed as DECIDE as the weekly run's open is, the one step the census lets
  move every count, which an open does, through
  OpenAndWaitForTheModels, the one reader of an NWF, with no clear before it, since the open
  replaces the document as it does for the preview and the probe. One that will not open, or
  opens and reports no models for the whole wait, stops its group FAILED with Core's reason and
  nothing is run on it, F74, and its GROUP finished line is labelled off that by
  RunPath.ForAnOpenFile, Stopped for one that read empty, never Weekly run, the DECIDE phrase
  NwfPick.DecidePhrase. The run stops after a group as the scanned run stops, through the
  one StopsTheRunAfter, and the gaps line, the list for the modellers, the timing beside size
  and each file's block are written once after the last file, WriteTheEndOfAnOpenFileRun. The
  window, OnRunPickedNwfs, reads the run settings first, refuses a tolerance that is not one, a
  pick Core refuses and two NWFs writing one file before anything is opened, asks before the
  open document goes, marks RUN started and finished, writes RUN SETTINGS with Core's lines and
  the scanned run's own choices through the one SayTheRunsChoices, reads the clash file through
  the open file run's ReadTheXmlForAnOpenFileRun, writes the lines across the run through the
  one SayWhatAnOpenFileRunAddedUp and SETS ACROSS THE RUN, and RESULT with Core's line. Since
  F151 no copy of the log goes beside the picked NWFs or anywhere else. The box and
  its three buttons sit in their own row on the Source step under the NWC folder, the label and
  grey line set off Core in the constructor. What a run of it reads on Navisworks is UNKNOWN
  until the proof steps of steps\03_bader_next.md run
- SINCE F114 A VIEW IS PER CLASH TEST, which replaces F85's viewpoint per clash, which had
  reversed what this rule said before it, that no clash is ever saved as a viewpoint.
  Bader's Q114: the thing a person presses is a test's open clashes framed together, since F186
  and F199 in Clash Status, then A, B or C, then the test's discipline folder, Core's
  `DisciplineFolders`, and the add-in hands the plan the saved tests the matrix does not name
  and nothing else of it, and Clash Detective is where one clash is looked at
  close up, docs\workflow.md. The plan is `Federator.Core.Views.TestViewPlan`, written by
  ViewpointBuilder since 2026-10-08, and F85's unmarked per clash viewpoints are known by
  `LegacyClashView` and removed once the new tree is written and read back. Do not read an
  older sentence out of an older log or an older comment and undo this
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
  What drifted is worth knowing every week. Overwriting it is worth doing once. APPLYING IS
  A WRITE, FR-150: `ClashRunner.Apply` counts each test it edits on the outcome,
  `ClashRunOutcome.AddApplied`, and the engine asks for the NWF to be saved again through
  `ClashRunOutcome.SavesTheNwfAgain`, the one rule of what the clash step wrote, where the
  engine's own expression left the applied count out and a test applied and then skipped left
  the NWD carrying the edit and the NWF not, so the next run found the same drift
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
  constructor, and is never typed into the XAML as well. The photo size, the cap and the five
  status ticks are read from ImageOptions, the paste into cells tick gone with F152, which is where the
  1024 measured off the accepted report lives. Two copies of a default drift, and the XAML
  copy is the one nothing can test. The tick box that once skipped the clash for models off the
  shared coordinates started from its Core default beside its
  label, because the XAML once carried IsChecked True and that copy was the real default
- A tally of what one run did is the engine's, and the window hands it to that run's
  RESULT block, because the engine is made for one press of a button and the log lives as
  long as the window. The tally of models off the shared coordinates lived on the log until
  F112's second attempt, and the second run of a window listed the first run's groups again
  under the second run's rule state. RunJobs and OnRunOpenDocument hold the engine outside
  their try, so the finally passes engine.CoordinatesAcrossTheRun, or null where no engine
  was made, to WriteTheResult, which since F151 writes the RESULT block and no copy of the
  log, for the scanned run, the open file run and the picked NWFs alike
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
  reads them as faults. Since F109 that walk, the file list and the stamp read the
  INSTALLED bundle, never the staging copy, each assembly read from its bytes so the
  script holds no file of it open, and the installed Federator.Addin.dll must read the
  stamp the build wrote. SINCE F156 A REFERENCE IS ALSO READ BY ITS NAME AFTER BINDING POLICY,
  `AppDomain.ApplyPolicy`, because the reflection only context applies none. Core asks for
  System.IO.Compression 4.2.0.0 since F152, the version the .NET 4.8 reference assemblies
  carry, and the runtime loads the framework's 4.0.0.0 for it, so without it every install
  from F152 on stopped with Federator.Core needs System.IO.Compression, read on 2026-10-14
- install.ps1 ends one of four ways, each an exit code and a last line written once at
  the top of the script and copied word for word into INSTALL.md: 0 and the line telling
  a person to start Navisworks, 1 FAILED, 2 REFUSED, 3 LEFT, installed and checked with
  the add-in installed before still beside it. The installed line is printed only when
  nothing is left beside the add-in, so it always means a whole install. Every other end
  names each folder left beside the add-in on its own line first, F109, FR-080
- install.ps1 removes no folder through a junction or a link. Every recursive removal
  goes through RemoveTree, which reads every folder under it for one first, by the link
  type PowerShell reads off a reparse point, because every file OneDrive syncs carries
  the reparse point attribute with no link type. A link in the staging folder or in the
  installed bundle refuses before anything changes. A copy that fails on a first install
  takes the new folder out of the load path, removed or moved to .failed-, F109, FR-078
- install.ps1 reads the .replaced- and .failed- folders beside the bundle at its start,
  by one pattern each on the plugin folder and never a search below it. An empty or part
  filled load path with one whole add-in beside it gets that one put back, PUT BACK, and
  any other leftover refuses with each named, because which one a person keeps is theirs
  to say, F109, FR-079
- What install.ps1 and the team install both hold to is written once, in
  build\install-common.ps1, which each dot-sources: the place the add-in goes, the line
  telling a person to start Navisworks, the 16 files of a whole bundle, the runner of F214
  among them, the stamp and its
  parts, the Roamer refusal in one set of words, the walk from %APPDATA% down, and
  RemoveTree with LinksAt, so no junction or link is ever removed through by either. A copy
  of one of these in another script is a bug. INSTALL.md and INSTALL-TEAM.txt carry the
  start line for a person, and the Core test that no logo ships reads the file list there,
  F140
- The team zip is made by build\package.ps1 alone, and only when Bader has written "make
  the team package", Q149. It refuses with one REFUSED line and writes no zip on a checkout
  not on main, a git status that prints anything, a build of the solution with
  --no-incremental that fails, a stamp reading +edits or naming another commit, an output
  folder inside the repo, a zip of that name already there, or any end of install.ps1 but
  exit 0. A zip copied into the folder by a run that then fails is deleted, and where it
  cannot be the FAILED line says it was left and where. It stages and checks the bundle by running install.ps1 -SkipBuild against a
  throwaway APPDATA, so a bundle is assembled and checked one way, zips what that run
  installed, and names the zip and VERSION.txt from the stamp read off the built add-in,
  never typed. A zip is never committed, F140
- The team install, build\team-install.ps1 started by the two .cmd files of build\team,
  copies and never renames, because the rename of the installed bundle was refused on this
  machine with no Navisworks running on 2026-10-01 while a copy over it worked. It makes the
  installed folder hold exactly the zip's files, reads every file back by its SHA256, and
  takes the internet mark off what it wrote, since Roamer.exe.config names no
  loadFromRemoteSources, read on 2026-10-11, and whether Navisworks loads a marked DLL is
  UNKNOWN. The version installed before goes to ParsonsNwcFederator.bundle.previous, the
  one backup, but only when it is whole, not the very build being installed, and not left
  by a run that stopped part way, which ParsonsNwcFederator.bundle.copying beside it marks.
  An install that fails part way puts the kept one back and reads it back. Where nothing was
  kept, a whole add-in of the same build is left where it is and read again against the zip,
  and only a load path that held nothing, part of an add-in or what a stopped run left is
  cleared. Put back first copies the add-in installed aside to
  ParsonsNwcFederator.bundle.before-put-back and reads it back, then copies the kept one in.
  A put back that fails part way copies the one set aside back and reads it back, so the load
  path holds part of two versions only where both the put back and that copy back fail, and
  the line then says where each whole one is. The kept one stays at .previous. It ends DONE 0,
  FAILED 1 or REFUSED 2, each written once at the top of the script and carried word for
  word by INSTALL-TEAM.txt, and a FAILED line says what each folder holds. Whether Navisworks
  loads a folder whose name does not end in .bundle, such as .previous, is UNKNOWN, F140
- The .cmd files of the team zip check that team-install.ps1 and install-common.ps1 are
  beside them before they start PowerShell, and refuse in plain words when either is not,
  as when a file is started from inside the zip, F140
- The NWF is looked at ONCE MORE after the NWD is published, because the NWD is published
  and since F182 before the pictures, and the NWF is the only record of what has been fixed. The line says intact and the
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
- Anything that can throw per item goes in its own try. Since FR-198 the source file is the
  one property read off each item past the quick ones, in its own try, for the mirror merge
  key. Before it the source file and the discipline were read last and were lost to a throw
  three properties earlier, on all 426 items, one throw costing three columns
- NO MODEL'S UNITS OR TRANSFORM IS EVER SET, F201, the alignment of 04 PK. Until F201 every
  model was set to Meters before the clash step through
  DocumentModels.SetModelUnitsAndTransform(model, Meters, model.Transform, reflected), the one
  public member that sets units, measured on 2026-09-01. MEASURED on 15 Oct 2026 on a copy of
  the ten NWCs of 1A04PK: appended alone, as the Append command does, every model is in Feet
  and their boxes lie about one building near E 658350 N 2745900. Set to Meters with its own
  transform handed back, each model became 3.28 times larger about its own origin and the
  centres moved apart by up to 667 m, so the NWD's models did not line up and the clash ran on
  that geometry. The units step, DocumentUnits, Core's UnitsLines, the Model units box on the
  Outputs step and the options SetDocumentUnits and UnitsName are deleted. The tolerance is put
  into the units Document.Units reports, and the report into metres by ReportUnits, as before.
  The UNITS step now reads each model, `ModelBoxReader`, once the files are in, and the NWD
  step reads them again before the NWD is written, one PLACE line a file from Core's
  `PlacementCheck`, held within PlacementToleranceMillimetres, 1 mm, or named. A Core test reads
  this folder and fails on SetModelUnitsAndTransform or OverridePermanentTransform
- A MODEL IS APPENDED THE WAY A MANUAL APPEND DOES IT, Bader's standing rule of 18 Oct 2026, given with
  his answer to Q202. The tool never changes any setting to append: no units, no origin, no transform, no
  file option and no Navisworks option, before or during an append. An append is Document.Clear where the
  group is built or rebuilt, then Document.TryAppendFile once per file, AppendOne, and nothing else. Read
  off src on 2026-10-18 by F212: nothing sets anything around it, F201 having taken out the last setting,
  SetModelUnitsAndTransform. What the tool sets it sets after the append and puts back, the hidden state,
  the undo and Clash Detective's Hide Other, each in its own bullet here. A fix that wants an append to
  behave otherwise, a corrupt file or a slow one, works around the append and never changes a setting for it
- A BAD NWC NEVER STOPS AN UNATTENDED RUN, F213, F210-R6. TryAppendFile on a random file, a file cut short
  or a file another program holds opens a Navisworks message box inside the call and waits for a person,
  scan.md 5z-zh and 5z-zi. So `AppendOne` makes every TryAppendFile under an `AppendBoxWatch`, a
  DispatcherTimer on the run's own thread, every `ReportOptions.AppendBox.EveryMilliseconds`, 200, which
  reads the class of every visible window of the process, hands it to Core's `AppendBox.IsAMessageBox` and
  `IsTheBoxOf` with the children it read, and answers a box only where Core says it is the box of that file,
  a #32770 window with one button whose text names the file, by WM_COMMAND with that button's id, 2, the
  click a person makes on OK. IDOK did not close it, measured. A box is marked pressed only once the press
  was posted, pressed again on every tick while it stands and said once where it still stands after ten. The
  append then returns false, the APPEND line says what Navisworks said, and where the append returned false,
  or threw after the box was answered, `AppendBox.GroupError` goes on `JobOutcome.FilesNavisworksRefused`,
  never on the errors, so the group ends FAILED with the file named, every step after the append runs as it
  did with the file missing, and the run goes on. A box answered on a file that then appended is said and
  fails nothing. A box that is not the file's is left alone and said once a session. No thread is started, no
  Navisworks call is made by the watch and no setting is changed. A check of the file's first bytes was
  measured and not taken: every real NWC begins #LcUStream-V1.1, which tells the random file alone, and a file
  cut short or held open passes it. WHAT IS NOT MEASURED: whether the append of a large real NWC opens a one
  button box naming the file that the watch would press, so the proof run appends the largest NWC of C06 under
  the watch, and an NWF opened that links a bad NWC is not watched, row F213-R1
- DocumentGuard reads IsClear, the file name and the model count, and hands each read that
  threw on with its reason to Core's `DiscardWarning`, FR-169, which words the warning
- A tick box has to earn being a decision. How many there are and how many sit up front is
  counted once, in What a tick box says below. Republishing the NWD, writing the client page and rendering the
  photos are fixed ON, because a weekly run wants all three every time. Client columns only
  is gone outright, dead since the workbook became one sheet with none of ours on it, and so
  is the thumbnail box, F152, since the pictures are linked in the client's form and never
  pasted. Dating the NWD, the clash XML, the ghosted look and the five image status boxes are
  collapsed under More, rarely changed. The one that destroys data, ApplyFileSettings, is collapsed on its
  own under Things that destroy data, because it does not belong beside ordinary output
  options. Fewer decisions is the goal, not more words explaining them
- THE SIDE WALK OF THE LEFTOVERS, SetBuilder.CountSide, F115: the side's selection and every
  source read out of it are disposed, R1, sources that read as none count the side as not read,
  R21, and a source resolving to a folder credits every set inside it, R8. IndexOfSet reads from
  the end as FindSelectionSet does, R3, so the set the drift was read off is the set replaced.
  The leftovers are walked and reported with the box off and acted on only with it ticked, R20.
  CarryOut finds every set again by its PATH, IndexPathsAt, and acts on nothing where a path
  holds no set or more than one, and nothing more is removed or renamed once the document is
  damaged, F115 attempt 2. A throw after the twin is gone declares the document damaged. With
  the box off a read of the walk that failed is one FINDING line and not a FAILURE. Every
  Search read off a set by the drift or the walk is disposed.
  The EMPTY SETS block is written by WriteTheEmptySets wherever the sets were judged, after the
  clash run and where no test ran, R4
- THE GENERIC MODELS SETS ARE BUILT IN THE SETS STEP OF EVERY GROUP, F128, with an XML picked
  or without one, since they come from the models and not from the file. `PlanTheGenericSets`
  reads Model.FileName and Model.SourceFileName off each model of the document, each wrapper
  disposed, and hands both to `GenericModelInput.From`, then `BuildTheGenericSets` builds
  `plan.ToBuildPlan()` through `SetBuilder.Build` with NO JUDGE and NO LEFTOVER WALK, because a
  walk handed that plan would remove every set of the picked file that no test points at, while
  the picked file's build is handed `GenericModelsPlan.SetNames` as wanted so its walk keeps
  the set of every model still in the group, and a set of a model that has left the group is
  stale and the walk removes it with the box on, which is right. THE WALK RUNS ONLY WHERE THE
  GENERIC MODELS PLAN WAS MADE AND HOLDS A MODEL, `GenericModelsPlan.WhyNoLeftoverWalk`, and
  where it is refused the engine writes that line and `SetBuilder.Build` is told not to walk, so
  a model's read that threw never removes last week's sets. Where the picked file's build
  declared the document damaged, Q74, the Generic Models sets are not built, one line says so
  and the step asks no save, so the OR of the two builds never saves a damaged NWF. The clash
  step then creates and runs no test and FinishTheGroup stops before the views, the NWF save,
  the workbook and the NWD, F115 R15 and R24, since a test or a view asked for the save over
  the damage. Their
  outcome is `JobOutcome.GenericSets`, never the picked file's, so the EMPTY SETS
  judge and SETS ACROSS THE RUN do not read them, a set created asks for the NWF save as any
  set does, FR-020, and a present set keeps the question it was built with unless the rebuild
  box is ticked, its Asked set to what the document's set asks as every present set's is. The
  plan is made before the file's build and its sets are built after, all inside the one SETS
  step, so the step's seconds show both. The GENERIC MODELS block and one row a model follow
  the build. Its sheet goes in the internal workbook, the INTERNAL step, below, written whether
  or not the clash ran, skipped with its own line where neither a count nor a coverage was
  taken, there is no report folder or no workbook is wanted this run, and where the write throws
  nothing is listed as written, through `RunLog.WriteThrew`, the size stays unread and one line
  says whether a file of that name is at the path, and that whether it is an earlier run's file
  untouched or one this run cut short is UNKNOWN, since a save that threw after opening the path
  leaves the old file cut short and File.Exists still reads true, because a file this tool did
  not write is never listed as written, and otherwise read back by size as every
  file is. The clash workbook's write that throws goes through WriteThrew the same way since
  F118-R3, and its WORKBOOK CHECK is not run, one line saying why, since it would read the
  file at the path as this run's. The settings are `ReportOptions.GenericModels`, read off Core and never typed in the
  window. A plan, a build or a workbook write that throws NEVER FAILS THE GROUP, the lead's
  decision on attempt 2: it logs the failure with what happens next, writes the GENERIC MODELS
  block with its FAILED line off `GenericModelsReport.FailedLines`, counts the group as not
  counted in RESULT, which the window hands the engine's `GenericModelsAcrossTheRun`, and the
  group keeps its own result. The SETS line says which text the sets look for off
  `GenericModelsPlan.LooksFor`, the stem of the NWC's name for a model whose source name was
  not read, which the plan also notes by name
- THE COVERAGE OF THE PICKED TESTS, F127's add-in half, built on 2026-10-11 on branch fix-F127b
  and not yet run on Navisworks. The add-in holds only the calls, and every rule and every word
  is Core's, core.md Coverage of the picked tests. CLASHRUNNER keeps the counts the creation
  plan decides on, `ClashRunOutcome.KeepItemsByLocator`, before `CreationPlan.For`, and each
  test's two side counts, `RecordSides`, as soon as its sides are read and before the
  coordinates, one discipline and side checks, so every skip reads the counts that made it. It
  hands out `RunPlan`, the plan it ran, each mirror under its run name and each saved test with
  its sides, which the engine keeps on `JobOutcome.ClashPlan` in place of the plan it handed in,
  and `InAMerge`, the tests of every mirrored pair whose clashes `MirrorMerge.AddTo` merged,
  read off what the merge did and never off the plan. THE COVERAGE STEP,
  `RunSteps.Coverage`, runs in FinishTheGroup after the clash workbook, its page and its XML and
  before the internal workbook and the NWD, so the document is still open and the clash
  workbook is on the disk, for the scanned run and the open file run alike: `CoverageRule.For`
  over the plan and the runner's record with the group's codes off the EXPORT CHECK's models
  and the run's off the ticked groups of this run, Clash Detective's two counts of every test in
  ONE WALK, `DocumentCensusReader.TestsWithTheirCounts`, the census's walk with every wrapper
  disposed and no handle kept, a test whose count throws logged and read minus one, the clash
  workbook read back BY THE CLIENT'S SHEET'S NAME, `WorkbookTests.ReadFile`, ONLY WHERE THIS RUN
  WROTE IT WHOLE, `JobOutcome.WorkbookNotReadBecause`, which the workbook step sets on every path
  through it and which says why the workbook is not read, a write that threw included, since
  last week's file at the same path would be read as this week's, then `CountCheck.Judge` with
  Compact's count and the merged tests, the COVERAGE block, `CoverageBlock.Lines`, one .tsv row
  a test, `coverage test`, and the group added to the run's `CoverageAcrossTheRun`, which the
  window hands to `RunLog.WriteResultBlock` on every run. A group whose clash step made no plan
  writes a block saying so and is counted not checked. EVERY GROUP THE RUN TOOK IS COUNTED ONCE,
  through `JobOutcome.InTheCoverage`, a `GroupInTheRun`: RunOne's finally and RunAnOpenFile's,
  the open file and F129's picked NWFs, say NotChecked when the group ends, which counts a group
  that ended early and changes nothing for one the COVERAGE step counted. A THROW ANYWHERE IN IT NEVER FAILS THE
  GROUP: it is logged with what happens next, the block says the coverage is UNKNOWN and the
  group is counted not checked, since a report check never fails a group. The picked XML's
  sha256 is read once when the run starts, one COVERAGE line, and a read that throws is logged
  and reads UNKNOWN. THE INTERNAL WORKBOOK, Bader's answer B to Q146 of 11 Oct 2026, the INTERNAL
  step after COVERAGE, `InternalWorkbook.Write` beside the clash workbook, named by
  `GenericModelsSettings.WorkbookNameFor`, the Generic Models sheet and the Coverage sheet in one
  save or the one the group has, each lack said on its own line, the Generic Models sheet left
  out with `InternalWorkbook.WhyNoGenericSheet`'s words where the two names are one to Excel,
  logged under its own kind, INTERNAL attempt and INTERNAL written, and never XLSX, so the
  loop's documents read pairs each NWF with its one clash workbook, then read back by size and
  by its sheets' names, `InternalWorkbook.Check`. A write that throws lists nothing as written
  and says whether a file of that name is at the path, as the Generic Models workbook did. THE
  CLASH WORKBOOK HOLDS THE CLIENT'S SHEET ALONE, the Navisworks export's layout, and its
  WORKBOOK CHECK calls a second sheet a problem. The settings are `ReportOptions.Coverage`, read
  off Core and never typed in the window. The two hand buttons take no coverage, they are not runs
- THE HARVEST ANSWERS BETWEEN ROWS, F143, Bader's order of 2026-10-11 headed 1A04PK IS FROZEN.
  Set 06's run of 1A04PK sat from 14:44 to 16:21 in the rows of BLD-AR-Walls-vs-BLD-AR-Floors, 1556
  clashes, with no line in the log and the window not answering, while TestsRunTest took 0.3 s, and
  1A02MM's harvest of set 06 was 90 percent pictures, 136.5 s of rendering of 198.3 s. The window pumps
  only when the engine's throttle renders the live line, FederatorWindow.Pump, so ClashHarvest now calls
  the runner's callback, ClashHarvest.Tick, through ClashHarvest.Piece after each top level row of a
  test, after the row's own wrapper is released, and after each item under a group row of a test in a
  merge, whose count of the whole is UNKNOWN. The test, its children and the grid stay held across the
  pump, as they are across every render, and so does the test's handle in RenderThePicturesWaiting,
  whose pictures are pieces the same way: the tool's window is modal over Navisworks and every handler
  of it that touches Navisworks returns while a run is going, Scan among them since F143. Piece writes a
  PROGRESS line with its .tsv row once ProgressSettings.EverySeconds has passed, StepProgress. The two
  calls that can be long, TestsRunTest and TestsImageForResult, are timed alone on the log's clock and
  named on a CALL line with its .tsv row where they took ProgressSettings.LongCallSeconds or more,
  LongCall.Say, in a finally, so one that throws is named too. Other passes over every clash, the
  penetration and by design passes, off by default, have no piece yet, row F143-R1. The runner's
  ProgressSettings is ClashRunner.Progress at its defaults, 30 s each, and it hands the render's to
  ClashImages
- THE RENUMBER ANSWERS WHILE IT RENAMES, F146: FederationEngine.RenumberThePictures hands
  ImageRenumbering.Apply a said that renders the live line through Tick, whose callback lets the window
  answer about once a second, and writes a PROGRESS line with its .tsv row through StepProgress once the
  runner's ProgressSettings says one is due, null settings writing none. Sets 07 to 09 read 1A02MM's window
  32 s not answering in RENUMBER. Since F149 every picture is written under its final name, so in a run
  that goes as planned the renumber checks every picture and renames none, and this paces the check
- ONLY THE TEST'S MODELS SHOWN FOR ITS PICTURES, F145, Bader's choice of Q150 on 2026-10-11, measured
  first, scan.md 5z-zc: 7.76 s a picture with every model shown and 1.78 s with only the test's on the
  first 20 clashes of 1A04PK's Walls-vs-Floors, the export's layout kept. TestModelsShown is one for the
  clash step, made in ClashRunner.Run where pictures are written and ended in its finally in a try of its
  own before the sets index is released, which puts back any hidden state still changed and writes the
  one IMAGES line of PictureModelsTally, which counts the tests by name since F149's second reading, so a
  test hidden for in two stretches is one test. Read takes the models off the test's OWN SelectionA and
  SelectionB, never off the file's set paths, since a test found in the document keeps its own sides,
  each item enumerated off GetSelectedItems, never disposed one by one, and walked to its model by
  SavedViewpoints.ModelFileOf, which releases the parents and never the item. Hide takes the hidden state
  once by SnapshotHidden, measured in 5k, adds hiding to every other model's root by HideTheOtherModels,
  which unlike ShowOnlyModels resets nothing, on a document F206's EverythingShown has already shown whole, and
  hands back what puts the state back by RestoreHiddenState after the stretch. NO CLASH HANDLE IS READ
  ACROSS THE HIDE: RenderThePicturesWaiting, where since F149 every picture is taken, reads the models off a
  handle released before the hide and pictures through one resolved after, the order 5z-zc ran in. A put
  back that fails is tried again before the next stretch and at the end of the step, since a hide only
  adds, and where it fails twice, or the hidden state cannot be read, the hiding stops for the step and
  every later stretch keeps every model shown. Which models stay shown is Core's PictureModels, and a test
  kept with every model shown is said with why on an IMAGES line. Since F149 the harvest hides nothing, so
  every cell of a row, its Grid Location and Level among them, is read with every model shown
- EVERY PICTURE TAKEN ONCE EVERY TEST HAS RUN, IN THE REPORT'S ORDER, F149, Bader's order of 2026-10-12 to
  fix the slowest step: set 09's 1A02MM spent 39 s of 253 s in RENUMBER. The harvest reads rows only and
  hides nothing. Run calls RenderThePicturesWaiting after MergeTheMirrors, whether or not a merge was made,
  and where the merge threw, through RenderThePicturesAfterTheMergeThrew before the merge's error goes on
  up. The pass runs inside a step of its own, PICTURES, opened as the engine's InStep opens one, with
  Live.StepStarted and one count of PROGRESS lines across the whole pass, `PROGRESS PICTURES`. It walks
  Core's PictureWalk over the rows the runner kept with their addresses, every row a harvest recorded
  whichever way it ended, while the views are handed the rows of a harvest that read its test whole, as
  before. A stretch is resolved once and hidden for by F145's rule only once a row of it takes a picture by
  ImageOptions.TakesAPictureOf, read before any result is resolved, and each picture is numbered by
  ClashReport.NextPictureOf, so it is written under its final name and the renumber renames none. A
  picture not asked for because its test or its clash was not found again, or a throw ended its stretch,
  is counted by ClashImages.NotReached, which reads Core's ImageOptions.CountTheRowsLeft: each row left by one
  fault counted as the writer would have counted it, a row of a status not asked for or past the cap as passed
  over and every other one as failed, and the fault told to the pictures' guard ONCE, the second reading, so one
  throw never reads as fifty failed renders, and each fault said on one IMAGE line with its counts. A group row
  is recorded as it is added to the report, before the clashes under it are handed to a merge, so a handing that
  throws leaves it its picture. A throw out of the merge leaves the pictures taken, and a throw out of them then
  is a failure line, so the merge's error is the one that goes on up. A pictures' guard firing after the tests'
  guard stopped the run is said on an IMAGES line. The pass breaks out once the pictures' guard fires and
  Run reads the guard when the pass ends, so the guard stops a run only once every test of the group has
  run, row F149-R1, and RunEach reads it once before a group's first test for a stop an earlier group's
  throw lost. A result is found again by its name read through ClashRow.NameOf, the rule the harvest
  names a row by. A result or a group with no display name is found again only at its recorded index and
  only where nothing can have moved it, the pictures, which run before Compact, and the views refuse it
- CLASH DETECTIVE'S HIDE OTHER IS SET FOR THE PICTURE PASS AND PUT BACK, F150, Bader's answer to Q151 and
  his order of 2026-10-12, item 157 of steps\02_questions.md. The picture call draws by the user's Clash
  Detective display settings, scan.md 5z-ze, so RenderThePicturesWaiting makes one PicturesHideOther in a using
  around the pass inside the PICTURES step. It touches nothing until TakeTheStretch calls Begin, once a row
  of the pass takes a picture, so a group that takes none reads and sets nothing. Begin reads Hide Other, sets
  it to the value Core's OtherItemsInPictures asks for, ON where ImageOptions.GhostTheOtherItems is unticked and
  OFF where it is ticked, and reads it back at once. Dispose, on every path out, writes back the user's value
  as the RUN'S FIRST GROUP read it, HideOtherAcrossTheRun, which the engine keeps for the run beside the
  pictures' guard and hands to each group's ClashImages, then reads it once more and writes the one IMAGES
  line. A put back that leaves it changed is one of the group's errors, so the window shows it, and is tried
  once more at the end of the run by PicturesHideOther.AtTheEnd. Every throw is a failure line and the pass
  goes on. NOT THE OPTION PATH: LcUOption.SetBoolean on tools.clash.displaysettings.hide_other reads back but
  the pictures did not follow, scan.md 5z-zf. The value is set on Clash Detective's own highlighter,
  LcOpState.GetActiveInstance, LcClClashResultHighlighter.GetInstance and its DisplaySettings, each
  undocumented, each a NativeHandle made eEXTERNAL, its IL in scan.md 5z-zf, so disposing one releases the
  wrapper and never the native object, and all three are made and disposed inside one read or one write.
  WHAT IS NOT KNOWN: the read back says what the highlighter holds and never what the picture call drew with,
  so whether the pictures follow is UNKNOWN until a run's picture sizes show it, near 48 KB against about
  550 KB, and the line says so. Dim Other and transparent dimming are left as the user has them, so the
  ghosted look holds only where they read on. A STOP BETWEEN THE SET AND THE PUT BACK, Navisworks closed or
  killed in the pass, leaves the value set, and whether Navisworks keeps it for the user's next start is
  UNKNOWN, row F150-R1
- LONG RUNS KEPT FROM SLOWING, F147, Bader's order of 2026-10-11, Q152 item 2. RenderThePicturesWaiting
  switches the undo off for the whole PICTURES step by Document.StartDisableUndo and, in its finally, on
  again by the matched EndDisableUndo, a counter by the install's Autodesk.Navisworks.Api.xml read on
  2026-10-12. The matching call is made only where the switch off returned, or where it threw and the
  undo reads off by IsUndoDisabled where it read on before, since the call then took the counter. One
  IMAGES line reads IsUndoDisabled and the name of the step Undo would take next before, during and
  after the pictures, a name and not the depth of the stack, so it says what it read, and an undo that
  reads off after where it read on before gets a line of its own, THE UNDO IS LEFT OFF. A put back
  retried by modelsShown.Dispose in Run's finally runs after the undo is on again. Core's
  TextureCacheWatch watches the folder ogs under Path.GetTempPath across the pass, built inside the
  step's try, a temp folder that cannot be read or joined said and the cache left unchecked, and it is
  checked between two stretches, core.md, THE TEXTURE CACHE. Every picture is already disposed as its
  save returns, and the pass moves no camera, so there is no camera change for a jump cut. Every call of
  the pass is on the main thread. The views step keeps the undo on
- WHAT EACH STEP HOLDS IS RELEASED AS IT ENDS, F147, Bader's answer to Q155 on 2026-10-12, item 2,
  read off every reader in src, no speculative rewrite. ClashRunner's picturesWaiting, one entry a row,
  is read by the PICTURES step alone and emptied in its finally. JobOutcome.RowAddresses, where every row
  came from, is read by the VIEWS step alone and set to null in FinishTheGroup as BuildViewpoints
  returns. JobOutcome.Report, the group's report model with every row, is read last by the GAPS block
  and released with RowAddresses by LetGoOfTheRows in the finally of RunOne and RunAnOpenFile, whichever
  way the group ended, since the run keeps every outcome for RESULT and each group's rows stayed until
  the run ended. A new reader of either after the group must move that release. Every Navisworks wrapper
  of the clash step was already disposed where it was read, and the sets index in Run's finally. The
  engine sets RunLog.MemoryReader to ProcessMemory.Read beside the census reader, core.md, THE MEMORY
  AROUND EACH STEP
- THE RUN COLUMN OF THE GROUP LIST TAKES A SHIFT RANGE, F130, FR-179. The rule is Core's,
  core.md, A SHIFT CLICK ON A RUN BOX. The column is a DataGridTemplateColumn and never the
  stock DataGridCheckBoxColumn, whose box is not hit testable, so the cell took the click and
  Shift only widened the row selection, turn5\measure-shift.md. Its CheckBox is bound TwoWay
  to Include and raises Click into `OnRunBoxClicked`, which reads Keyboard.Modifiers at that
  click, hands `ShiftRange.Of` the rows of `GroupsGrid.Items`, the shown order, sets every row
  it hands back through Include, keeps the row clicked and its state as the anchor and calls
  `RefreshOutputsSummary` once. The column is read only so the grid begins no edit that would
  swap the box out under the click, and its SortMemberPath is Include so it sorts as before.
  The anchor is the window's and `Regroup` drops it, since every row is made again there. A
  plain click is meant to flip the box on the one click, where the stock column, read off its
  IL and never clicked, took a click to select the row before one that flipped it. Whether
  the template's box flips on the first click in Navisworks is UNKNOWN until step 546 of
  steps\03_bader_next.md. The grey line under the list, x:Name RunRangeHelp, is set
  off `ShiftRange.HelpLine` in the constructor. THE SPACE BAR ON THE RUN CELL TICKS AS THE
  STOCK COLUMN DID, F130-R1: the grid's PreviewKeyDown, `OnGroupsGridKeyDown`, takes Space on a
  DataGridCell of the column named RunColumn, flips its row through Include where CanInclude
  allows and goes on as a click does, through `RunBoxFlipped`, the one path both share, Shift
  making a range. Space with the box itself focused is left to the box, which raises Click.
  The box has no x:Name and no label, so it is
  not one of the twenty named tick boxes counted below. The window driver cannot carry Shift
  without real input, turn5\measure-shift-driver-result.md, so its proof is numbered steps in
  steps\03_bader_next.md
- A REGROUP KEEPS THE RUN TICKS AND THE TYPED NAMES, FR-153, the rule Core's, core.md, A
  regroup keeps what was typed. `Regroup` reads every row's Run box into `KeptRunTicks`
  before `groups.Clear()`, sets each usable row made again through Include off
  `StartsTicked`, and builds the name table with `KeepTypedFrom` the table before. It clears
  `lastRefill`, which described a refill of the table it replaced. A new Scan regroups too,
  so a group whose key is the same keeps its tick and its typed names across a Scan
- A CLEARED NAME CELL GOES BACK TO THE PATTERN, FR-160. `GroupRow.TypeOver` hands the cell to
  `OutputNameRow.Typed`, and `OnNameEdited`, after the commit, refills the table and reads
  the edited row back, so the cell shows the pattern's name at once
- THE WINDOW REFUSES TO CLOSE WHILE ANYTHING RUNS, FR-163. The Closing event, `OnClosing`, is
  the one rule: while `running` it cancels and warns The run is still going. Let it finish,
  for the title bar close, Alt+F4 and the Close button alike, which now only calls Close.
  Before, only the button refused, and the repaint pump let the other two close the window
  under a run
- ONLY A RUN THAT CREATES OR RUNS CLASH TESTS READS THE TOLERANCE, FR-166.
  `ToleranceOrSayWhyNot` reads the drop down through `ToleranceChoice.FromThePicker` and,
  where it does not read, logs a TOLERANCE line and warns before anything starts. The scanned
  run, the run of picked NWFs, the open file run and Run tests call it and hand the value to
  `ReportsWanted`. Undo, Probe and Build sets never read it and hand
  `ToleranceChoice.Default()`, 25 mm, so the drop down never stops them

## What a tick box says

A tick box is a SHORT LABEL and one grey line under it. Bader called the long ones a
nightmare and he was right: they ran off the edge of the window, they shouted in capitals,
and they explained the off state as well as the on state, so nothing stood out.

- the label is at most EIGHT words and names the thing, not the mechanism
- one grey help line under it, at most TWELVE words, saying what it costs
- no capitals for emphasis anywhere. Acronyms a person says out loud are fine
- do not describe the off state unless it changes what someone would choose
- a box that destroys something carries a red exclamation mark beside the label. A mark,
  because when every label shouted, none of them did

The numbers in a help line are measured, never estimated. Photos are about 0.08 seconds
each and 213 took 17 seconds.

There were fifteen, then eleven, and F72 made twelve, of which two were visible without
opening anything. F72b, Q72, Q99 and F136 each added one up front on the Clash step, F150
added GhostOtherItems under More, F152 took EmbedThumbnails out of it and F159 took SkipClashOffCoordinates out,
F188 added RebuildExisting and the five step boxes of the NWF page, so
src\Federator.Addin\Ui\FederatorWindow.xaml holds TWENTY named tick boxes, of which ELEVEN sit
under no expander, by x:Name IncludeSubfolders and RebuildExisting on the Source step, MarkPenetrations,
MarkByDesign, RebuildDriftedSets and MakeViewpoints on the Clash step, and NwfStepRefresh, NwfStepTests,
NwfStepPictures, NwfStepViewpoints and NwfStepReports on the NWF page, each picking a step of the weekly run.
The other nine are eight under More, rarely changed, DateTheNwd, WriteClashXml,
GhostOtherItems and the five Photo of boxes, and one under Things that destroy data,
ApplyFileSettings, CompactResolved having gone with F185. Two CheckBox elements carry no name, the Run column's
and the package tree's, and are not counted. Read off the XAML on 2026-10-18 by its CheckBox elements with an
x:Name and the Expander above each, Bader's answer to Q205: the window stays as it is, eleven boxes up front.
probe-window-labels.ps1 was not run on this count. A box only stays if a normal weekly run genuinely has to choose, and
Mark penetrations as Reviewed earns it: it writes statuses into the NWF, which is the only
record of what has been fixed, so a run has to be told to do that rather than told not to. Everything else became a
fixed behaviour with the sensible answer chosen, or moved under an expander. The one that
destroys data, since CompactResolved went, is under an expander of its own, because it does not belong beside ordinary output
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

## The clash step shows every item, F204

CLASH DETECTIVE FINDS NO CLASH ON A HIDDEN ITEM. Measured on 17 Oct 2026 on a copy of the C04 NWF saved on 15 Oct with 8
of its 10 models hidden whole: one test by TestsRunTest took 0.04 s and turned its 232 results Resolved, the same test on
the same NWCs with nothing hidden took 0.4 s and found the 232, and the same NWF with the 8 items shown in memory took
1.74 s and found the 232. The NWF page refresh of that NWF marked 10016 of 13286 clashes Resolved. So `ClashRunner.Run`
reads the hidden state by `SavedViewpoints.SnapshotHidden` and counts the models whose root item is hidden, says what it
found on one CLASH HIDDEN line, calls `ResetAllHidden` where anything was hidden, runs the plan, and in a finally puts the
snapshot back by `SavedViewpoints.RestoreHiddenState`, read back and said. Neither the showing nor the putting back stops
the run. The words are `Federator.Core.Clash.HiddenForTheClash`'s. Whether a model hidden whole that the snapshot does not
list is hidden again after is read off the run, and the line says when it is not.

THE PICTURES TAKEN AFTER THE SAVE SHOW EVERY ITEM TOO, F206. Since F182 the pictures are taken by
`ClashRunner.TakeTheDeferredPictures` after the NWF is saved, so after F204 has hidden again what the document hid. On the
audit round's run 2 of 17 Oct 2026, on a copy of C04 saved with 8 of 10 models hidden whole, 744 of the 3080 pictures showed
only the sky and the grid label and 2127 belonged to tests with a side in a hidden model. Both steps now go through
`EverythingShown`, one class that snapshots, counts the models hidden whole, says one line, calls `ResetAllHidden` where
anything was hidden and puts the snapshot back when disposed. The picture pass makes it before `TestModelsShown` and disposes
it after that one ends, so each test's other models are hidden on top of an all shown document and the person's hides come
back last. Its words are `Federator.Core.Clash.HiddenForThePictures`'s, IMAGES HIDDEN, and they differ from the clash
step's so the log says which step showed and which put back. A person's hide no longer reaches a picture, as it reaches
neither the clash nor a view.

## The packages, F208

MANY COMMUNITIES AT ONCE, Bader's message of 17 Oct 2026 late evening, feature 1. Every rule and every word is Core's,
core.md Communities and packages. The NWC page has a step 5. Packages and the NWF page a panel on its right, and both
are bound to ONE `PackagePanel`, so the parent and the Report folders typed and every tick read the same on both pages.
The tree is a TreeView of `PackageNode` lines whose tick reads and sets Core's `PackageTicks` and holds none of its own,
every line open so every tick box is in the visual tree, its AutomationProperties.Name its title. Read reads the tree off
the parent folder, writes the PACKAGE TREE block and shows nothing ticked. Each package's line carries its files and the
groups they make by steps 2 and 3, counted again when the step is shown.

THE NWC PAGE'S ROUTE, `OnRunPackages`: refused with Core's reason before anything is opened while no tree is read,
nothing is ticked or the Report folder is refused, then one `PackagePlan` per ticked package, a group whose NWF is there
left to the NWF page by `NwcPageRule`, the PACKAGES TICKED and PACKAGE GROUPS blocks, the pack choice and the confirm of
the scanned run, the folders of each package that runs made and said, then, since F214, each package handed to its own
Navisworks, below, whose window runs `RunJobs` with one engine for that package's groups.
Each job carries its package and its package's Log folder, `FederationJob.InPackage`, the copy of a dated rebuild keeps
both, and `RunOne` sets the Log folder off the job. The GROUP started and finished lines, the pace of the live line,
the coordinates tally and the clash totals read `FederationJob.Label`, so community and package are on every group line
and in RESULT. The list of models off the shared coordinates goes into the Log folder of every package the run wrote, as
the run log is copied into each.

THE NWF PAGE'S ROUTE, `OnRunNwfPackages`: the same refusals, the NWFs of each ticked package by `PackageNwfs`, a package
with none named and the others run, the open document's confirm and the refresh confirm, then, since F214, each package in
its own Navisworks, whose window runs `RunPickedNwfs` with each NWF's place, `PackageNwf`, so its NWD, its Backup and its
logs are written into its package's folders and not beside the file. One failure never stops the others: each group and each
NWF is run in its own try, and since F214 the guard that stops a run after its first 50 tests all fail the same way stops
only the package it fired in, since the next package runs in a fresh Navisworks with a fresh engine.

## A fresh Navisworks per package, F214

Bader's answer to Q204 of 18 Oct 2026: a fresh Navisworks start per package, Run pressed once, the next starting only after
the last closed, measured first in docs\history\scan.md 5z-zj. Every rule and every word of the files and the lines is Core's,
core.md A FRESH NAVISWORKS PER PACKAGE. THE WINDOW A PERSON RUNS plans and confirms the packages on either page as before,
every refusal, the pack choice and the one confirm, which says nothing open here is touched in place of what opening would
discard, and the NWF page asks nothing about the open document, since no package opens anything in this Navisworks. Then
`RunPackagesInFreshStarts` writes the order into a folder of its own under package-runs beside the tool's logs folder, every
control of the window by its name, the text boxes a person can type in, the tick boxes and the drop downs,
`TheControlsAsTheyStand`, but the drop down of the NWFs last used, `NeverCopied`, whose items are each machine's history and
whose choice reads another NWF, starts `Federator.Runner.exe` from the bundle with the folder of the Navisworks it runs in and
the order, and `WaitForTheRunner` answers the window every poll and copies each new line of the status file into the log. A
pass that throws is said once and the wait goes on, so the results are never read while the runner runs, and past every
package's ceiling and wait added up, `RunnerLimitMinutes`, the window ends the runner, the one process it started. The live
line says the packages run, in Core's words, and never a line of the runner's. Once the runner ends `JoinTheResults` reads
every package's result, fills the package tally from them, a package with no result counted by `PackageTally.NoResult` and
never said as not run, writes the one RESULT through `RunLog.WriteJoinedResult`, which closes the log's account as every
RESULT does, sets the live line to the packages that wrote a result, and warns where one did not. THE RUNNER,
src\Federator.Runner, references no Navisworks file, loads the Automation DLL from the install folder by reflection as the
probe's helper did, and for each package in turn starts one Navisworks and takes the one Roamer new since before as its own and
never any other. Where it cannot tell which is its own, more than one or none new, the package is not run, its Navisworks told
to quit and every Navisworks new since its start waited on and none ended. Otherwise it sets it visible, runs the installed
plugin with the mode package-run, the order and the index, quits it by Dispose and waits the order's GoneWithinSeconds for its
process to be gone, ending it past that, so the next starts only once the last has closed. A start that threw has the Navisworks
it brought up found and waited on too. The order's ceiling runs from before the start to after the wait, so a start, a run, a
quit or a wait that hangs is ended where its Navisworks is told, and said where it is not. A throw, a crash or the ceiling of
one package is said and the next runs. THE WINDOW OF A PACKAGE'S OWN NAVISWORKS, `FederatorPlugin` handed the mode, the order
and the index, is made with a `PackageRunChild` and runs as soon as it is shown: it sets the controls in the order's order and
runs nothing where any could not be set, reads the tree off the order's parent, ticks its one package and runs the route of its
page, its own engine, groups, discipline reports and log, then writes its result and closes. Its groups are the engine's
`OutcomesSoFar`, so a run that threw part way still names the groups before the throw. In it no box is shown: `Warn` logs what
it would have said and its first warning is the package's reason where no group ran, and the run's confirm, the confirm of what
opening a file discards, the refresh confirm of the NWF page and the Rebuild box's confirm each log that the window that started
the run confirmed it. A child whose order does not read runs nothing and says so. WHAT IS NOT MEASURED: whether a Navisworks a
person started, and not the Automation API, can start the runner's Navisworks, which is step one of the proof in
steps\03_bader_next.md. Rows F214-R1 to R4 hold what the readers found and this does not close.
## The discipline reports, F209

Every rule and every word is Core's, core.md The discipline reports. The add-in reads and writes only. FOR A GROUP OF A
PACKAGE ALONE, `FederationEngine.ReadTheDisciplineFacts` runs at the end of `WhatTheModelsCarry`, the one place both routes
pass through, in a try of its own, so a fault never fails the group and never counts its EXPORT CHECK as not read. It calls
`DisciplineFactsReader.Read`: each model's root box converted to metres through UnitTable, its shared site through
`ModelFactsReader.SharedCoordinateOn`, then ONE WALK of its items, each released as the walk leaves it, reading the Item tab
and the Element tab once per item by the names `DisciplineProperties` holds, measured in scan.md 5z-zg. An element of the
setting's category is a Generic Models line, and a topmost item of the Item tab Type Imports in Families is a CAD line whose
element is the nearest item above it carrying the Element tab's Id. A part that would not read goes on with why. One
DISCIPLINE line a group says what was read and in how long. The facts stay on `JobOutcome.DisciplineFacts`.

THE WINDOW WRITES THE WORKBOOKS ONCE THE PACKAGE'S GROUPS HAVE RUN, `WriteTheDisciplineReports`, each package in a try of its
own: on the NWC page every group of its plan, the facts of those that ran and the files alone of those left out, on the NWF
page every NWF that ran and its models' files, each NWC's date off the disk, then `DisciplineReport.For`, the DISCIPLINE
REPORT block, and the four workbooks in the package's report folder, each one CHECKS attempt and written line with its size
read back by the log and one line reading its rows back off the file.
