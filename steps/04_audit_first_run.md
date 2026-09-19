# 04 audit, the first run round

Audited 2026-09-19 against main at cd25c33. Read only. Nothing was changed.
The add-in WAS NOT built. This is a Linux container with no Navisworks on it: `dotnet build ParsonsNwcFederator.sln -c Release` stops at `Federator.Addin.csproj(70,5): error : Autodesk.Navisworks.Api.dll was not found`. CHECK 2 was done by reading, so nothing below about the add-in compiling is CONFIRMED.

## What was read

- The round's diff, `55e84c5..cd25c33`: 52 files under `src/Federator.Core` (28 added, 24 changed), 4 under `src/Federator.Addin` (`ClashImages.cs`, `ClashRunner.cs`, `FederationEngine.cs`, `FederatorWindow.xaml.cs`), and no change at all to `FederatorWindow.xaml`
- Every public type, method and property the round added, grepped for a caller across all of `src` and again across `src/Federator.Addin` alone
- The four add-in files by hand, for every Core signature they call that the round changed or removed
- `tools/checks/check-imports.sh` and `check-locals.sh`, both clean
- All 32 skipped tests with the reason each gives
- The 13 new test fixtures for assertions that cannot fail and thresholds with no boundary
- `.claude/rules/core.md`, `addin.md`, `tests.md`, `CLAUDE.md`, `docs/history/scan.md` 5e to 5i, `steps/02_questions.md` Q46 to Q52, `steps/03_bader_next.md` 353 to 380
- Every file basename across the checkout for a second file of the same name
- What I could not read: the add-in against a compiler, and any of the five measurements. Both need Navisworks

## Findings

### A1  Decide still rebuilds an NWF that read empty. F74's wait and refusal are never called
- severity  BREAKS A FEATURE
- file      src/Federator.Addin/Engine/FederationEngine.cs:689-695, src/Federator.Core/Rerun/ModelLoadWait.cs, src/Federator.Core/Rerun/NwfComparison.cs `ReadEmpty`
- what      The engine is unchanged at the read: `if (!document.TryOpenFile(job.NwfPath))` then `return NwfComparison.Compare(FilesInsideTheOpenDocument(job.Building), job.Files);`. `ModelLoadWait` has 0 references in `src/Federator.Addin`. `NwfComparison.ReadEmpty` has 0. `GroupFacts.NwfReadEmptyReason` is never set. `RerunDecision.Refused` and `RunPath.Stopped` are never produced by running code
- expected  `03_bader_next.md` step 365 says what the engine has to do. `core.md:696` and `:708` say it is done
- costs     The next run against the same seven buildings reads five NWFs as `0 unchanged, 4 added, 0 removed` again and rebuilds them again, throwing away every clash result and status decision inside them. This is the data loss the round was opened for, and it will happen again
- fix       Step 365 as written: poll through `ModelLoadWait` after `TryOpenFile`, write `wait.Line()`, and on a zero at the ceiling return `NwfComparison.ReadEmpty` and carry `Reason` into `GroupFacts.NwfReadEmptyReason`. The same in `PreviewRunPaths`
- proof     A run whose log carries no `LOADING` line and no `STOPPED` line on a group whose NWF was on disk. Only a run can prove it, which is why it was missed

### A2  The tolerance is still the document's, not the tool's. F76 has no drop down and nothing reads the setting
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Report/ReportOptions.cs:140, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1110-1112, src/Federator.Addin/Engine/ClashRunner.cs:406
- what      `public ToleranceChoice Tolerance { get; set; }` is set only by the constructor to `FromTheFile()`. The window's `ReportsWanted()` sets `ApplyFileSettings` and `MarkPenetrations` and nothing else new. `options.Tolerance` has 0 references in the add-in. The report still copies the plan: `report.Tolerance = planned.Tolerance;` at `ClashRunner.cs:406`, so `TestReport.ToleranceFrom` stays `Unknown` on every row and `ToleranceChoice.ReadFromLine` is never written
- expected  Steps 367 and 368. `core.md:216` and `:234`
- costs     The matrix says 25 mm, the NWFs hold 75, the run clashes at 75 and the report cell still says what the XML asked for. Exactly the fault F76 was briefed for, unchanged
- fix       Step 367 for the drop down and `options.Tolerance.For(...)` at both places a test's tolerance is set. Step 368 for reading the report's tolerance off the `ClashTest` in the document and setting `ToleranceFrom = Document`
- proof     No `TOLERANCE` line in any log. A workbook whose Tolerance cell disagrees with the Clash Detective panel on a weekly run

### A3  The document is still not emptied between groups. F75's rule has no caller
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Diagnostics/CensusRule.cs `StartOfGroupLine`, `StartOfGroupReason`; src/Federator.Addin/Engine/FederationEngine.cs `RunOne`
- what      Both methods have 0 references anywhere in `src` outside their own file. `document.Clear()` is still called in exactly the two places it was, both after Decide has read the file list
- expected  Step 366. `core.md:719` says "THE DOCUMENT IS EMPTIED AT THE TOP OF EVERY GROUP, BEFORE DECIDE, F75"
- costs     Decide compares the scan against whatever the previous building left behind. Combined with A1 it is the same fault twice
- fix       Step 366: clear at the top of `RunOne` on the scanned path, take the census, write `StartOfGroupLine(census, true)`, put the reason on the group. Pass `false` on the open file path
- proof     No `CENSUS CLEAR` or `CENSUS DIRTY` line at the top of any group

### A4  Every test is still created. F77's plan has no caller and 631 seconds a run are still spent
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Clash/CreationPlan.cs, src/Federator.Addin/Engine/ClashRunner.cs
- what      `CreationPlan` has 0 references in `src/Federator.Addin`. The runner still creates every planned test and the empty side check still fires afterwards, so the line `CLASH 1830 in the file, 211 created, 1619 not created, a side finds nothing` is never written and `BlockCountLine` has no caller in `src` at all
- expected  Step 369. `core.md:164`
- costs     TESTS CREATE stays at 631 of 1424 seconds. Criterion 2's forty five minutes is not helped
- fix       Step 369: build the locator to item count map off the resolved sets, hand it and the plan to `CreationPlan.For`, create `plan.Create` only, feed `plan.NotCreated` to the skip machinery, write `CountedLine`
- proof     A log with no `CLASH 1830 in the file` line and a TESTS CREATE step still over 600 seconds

### A5  The priority never reaches anything. F83 has no picker, so the whole path is unreachable
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Report/ReportOptions.cs:125, src/Federator.Core/Report/ClashReportModel.cs `Priorities`, src/Federator.Addin/Ui/FederatorWindow.xaml
- what      `public string PriorityPath { get; set; }` is never set. `report.Priorities` is never assigned, so it is always `NothingPicked()`. `TestReport.Priority` is never set. `PickerKind.Priority` has 0 references in the add-in. `FederatorWindow.xaml` did not change at all in the round, so there is no picker. `WorkbookCheck.Of(path)` is still the one argument form at `FederationEngine.cs:1670`, and the two argument form has no caller in `src`. `PriorityTally` is never counted and its `ResultLine` never written
- expected  Step 370. `core.md:446` and `:458`
- costs     No Priority column, no A then B then C order, no `PRIORITY` line, no clashes by priority in RESULT. The person who wrote `clash-priority-map.csv` gets nothing from it
- fix       Step 370 as written, and pass `picked` into `WorkbookCheck.Of(path, picked)`
- proof     Pick the file, run, and find no Priority column in T

### A6  By design connections never move. F72b has no tick box, no picker and no caller
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Report/ReportOptions.cs:132 and :169, src/Federator.Core/Clash/ByDesignRule.cs
- what      `ByDesignPath` and `MarkByDesign` are never set and never read. `ByDesignPairs`, `ByDesignRule` and `ByDesignTally` have 0 references in the add-in. `ByDesignPairs.NotMatched` and `PairsSeen` have no caller in `src` at all. The line `REVIEWED rule B 47 set, 3 pairs in the file matched no test in this run` has no writer
- expected  Step 371. `core.md:903`
- costs     A person still looks at every column on its foundation and every door in its wall every week
- fix       Step 371: the box, the picker, `ByDesignRule.Judge` inside the one status pass, the block, `NotMatched` once per run
- proof     Tick the box, run, and find every by design clash still at New

### A7  Nothing is written on a clash and nothing can be undone. F72c has no caller
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Clash/AutoReviewRecord.cs, src/Federator.Core/Clash/UndoAutoReview.cs
- what      Both types have 0 references in `src/Federator.Addin`. `StatusesThisToolMaySet.AllowsAsUndo` and `WhyNotAsUndo` have no caller in `src`. There is no `Undo auto Reviewed` button because the XAML did not change
- expected  Steps 372 and 373, after the measurement in step 356
- costs     A clash the penetration rule moved carries nothing in the NWF saying why, and there is no way back except one at a time by hand. Partly waits on scan.md 5h, which is honest, but the undo half needs no measurement and is also absent
- fix       Step 356 first, then 372 and 373
- proof     Open the NWF after a run with penetrations on and find no comment on any moved clash

### A8  The property probe does not exist. F86 has no button and nothing calls the five Core types
- severity  BREAKS A FEATURE
- file      src/Federator.Core/Probe/ProbeSettings.cs, ProbeTally.cs, ProbeCsv.cs, ProbeVerdict.cs, ProbeRow.cs
- what      All five have 0 references in `src/Federator.Addin`. `ProbeSettings.MayRead` and `CsvPathFor` have no caller in `src` at all. `03_bader_next.md:665` says "The button reads `Probe model properties` and it is on the Clash step", and there is no such button in `FederatorWindow.xaml`
- expected  Steps 358 to 364 describe running it as if it were there. Step 354 is the measurement it needs first
- costs     No CSV, no `PROBE` block, no answer about FS or Fire Suppression, and the mechanical sets cannot be rewritten from anything. Steps 358 to 364 send a person looking for a button that does not exist
- fix       Step 354, then the walk in the add-in through `ProbeTally.Add` per property and `ProbeVerdict.Lines` per file, and the button. Until then, reword 358 to say the button is not there yet
- proof     Open the Clash step and look for the button

### A9  The rules file states eleven behaviours as done that the code does not carry out
- severity  NOISE, and the most expensive noise in the repo
- file      .claude/rules/core.md:164, :216, :234, :446, :458, :696, :708, :719, :793, :903, :923, :934, :952, :1040
- what      Each bullet is written in the present tense as a rule the run follows, for example `:719` "THE DOCUMENT IS EMPTIED AT THE TOP OF EVERY GROUP, BEFORE DECIDE, F75" and `:696` "DECIDE WAITS FOR THE MODELS BEFORE IT COUNTS THEM, F74" and `:1040` "VIEWS is F85's and it was untimed before that". A1 to A8 show none of them is wired, and `RunSteps.Views` has 0 references in the add-in so VIEWS is still untimed
- expected  CLAUDE.md: "When a rule here or in .claude\rules seems wrong, read the reason there before changing it." The rules are what the next session trusts. `01_next.md` says plainly "EVERY FIX IN THIS ROUND IS THE CORE HALF ONLY", and core.md says the opposite bullet by bullet
- costs     The next session reads core.md, believes the document is cleared and the wait exists, and misdiagnoses the next run's log against a rule that was never true of the running tool. That is the exact failure mode the round warned about in the other direction
- fix       One sentence at the top of each of those bullets: "THE CORE HALF ONLY, until 03_bader_next.md step N is done", or one paragraph at the top of core.md's Rules the code holds naming the eleven. Remove each sentence as its step lands
- proof     Nothing mechanical. A reader of the rule against a reader of the log

### A10  A comment in the add-in says viewpoints are written by ViewpointBuilder. They are not
- severity  NOISE
- file      src/Federator.Addin/Engine/ClashImages.cs:41-42
- what      "clash gets a viewpoint as well as a picture now, planned by ClashViewpointPlan and written by ViewpointBuilder, neither of which is this file". `ViewpointBuilder.Build` at `ViewpointBuilder.cs:59` still takes `IList<PlannedViewpoint>`, the old per discipline plan. `ClashViewpointPlan` has one reference in the add-in and it is this comment. `SavedViewpoints.CanBuild` is false, so nothing is written at all
- expected  `SavedViewpoints.cs:36`, in the same folder: "Nothing here creates or names a viewpoint. CanBuild is still false"
- costs     A reader is told a clash gets a viewpoint now. It does not, and will not until step 375 answers
- fix       "planned by ClashViewpointPlan and, once SavedViewpoints.CanBuild is true, written by ViewpointBuilder. Today neither happens"
- proof     Read the two comments side by side

### A11  A broken category resource reads as an unmeasured list, and the test for it cannot tell the two apart
- severity  WRONG OUTPUT, PLAUSIBLE
- file      src/Federator.Core/Exchange/RevitCategories.cs:117-119 and :138-142, tests/Federator.Core.Tests/Health/SetWarningsTests.cs:145-151
- what      `if (stream == null) { return known; }` and `catch (Exception) { known = new List<string>(); }` both give the empty list, and `Line()` then says "Revit categories known: none yet, so no set was checked against them" whichever it was. The test `WithNoCategoryListNothingIsReportedAndTheBlockSaysWhy` asserts `RevitCategories.Measured` is false and `Line()` contains "none yet", which is also true when the resource is missing from the DLL. Delete `Exchange\revit-categories.txt` from the csproj and every assertion in it still passes
- expected  CLAUDE.md: say UNKNOWN rather than filling a gap, and never report a check that did not run. A resource that could not be read is a different fact from a list nobody has filled. Today I confirmed the resource IS in the DLL (`strings Federator.Core.dll` finds `Federator.Core.Exchange.revit-categories.txt`), so this fires only when a later build drops it, which is what a test is for
- costs     After the list is measured and filled, a build that loses the resource reports "none yet" on every run and nobody knows the check stopped running
- fix       Keep a third state: `ResourceFound`. `Line()` says "the category list could not be read out of Federator.Core.dll" when the stream is null or the read throws. The test asserts `ResourceFound` is true and `Measured` is false, so the two can never be confused again
- proof     Remove the `EmbeddedResource` line from `Federator.Core.csproj` and run the fixture. It should go red and today it stays green

### A12  The set name separator is written twice
- severity  NOISE
- file      src/Federator.Core/Views/ViewpointSettings.cs:43, src/Federator.Core/Health/HealthCheck.cs:133
- what      `public const char DefaultSetNameSeparator = '-';` and `public const char SetNameSeparator = '-';`. Both split the same set names the same way, one for the pair folder and one for the health check
- expected  CLAUDE.md: "One rule lives in one place. A copy of a rule in a second file is a bug." The round wrote the rule about the 150 being named once and then named this twice
- costs     A project whose sets are named `BLD_EL_Devices` changes one and the health check and the viewpoint folders disagree about which sets are Electrical
- fix       One constant, read by both. `HealthCheck.cs:133` even says why it is a constant there rather than a setting
- proof     A test that asserts the two are the same object, or deleting one

### A13  "No priority" is written twice
- severity  NOISE
- file      src/Federator.Core/Views/ViewpointSettings.cs:58, src/Federator.Core/Clash/ClashPriority.cs:47
- what      `public const string DefaultNoPriorityFolder = "No priority";` and `case ClashPriority.None: return "No priority";`. `ClashViewpointPlan.FolderFor` reads the setting for None and `Priorities.Words` for A, B and C, so one folder name comes from each place
- expected  One rule in one place
- costs     Rename one and the viewpoint tree has a folder the log spells differently
- fix       `DefaultNoPriorityFolder = Priorities.Words(ClashPriority.None)`, or drop the setting and read `Words`
- proof     Change one string and grep the other

### A14  The five examples rule is now written in seven places, and one of them is ten
- severity  NOISE
- file      src/Federator.Core/Clash/PriorityMap.cs:36, Clash/TestDrift.cs:313, Probe/ProbeVerdict.cs:100, Health/HealthCheckResult.cs:188, Diagnostics/RunLog.cs:637, and pre-existing Clash/ClashRunOutcome.cs:207 and Views/SizeSettings.cs:62; Sets/SetsAcrossTheRun.cs:80
- what      Five constants added by the round each `= 5`, beside two that were already there. `SetsAcrossTheRun.ExamplesShown = 10` with no reason written for the difference
- expected  `core.md` "Rules the code holds": "log the count and at most five examples, then the total". One number
- costs     Someone changing the rule to three changes some and not others, and the log shows five here and three there
- fix       One setting the seven read, in the same file the rule is written in, with `SetsAcrossTheRun` either reading it or its comment saying why ten
- proof     grep `= 5` across `src/Federator.Core` after the change

### A15  The exchange file carries the fallback form and nothing at run time says so
- severity  NOISE, PLAUSIBLE
- file      src/Federator.Core/Exchange/MatrixCorrections.cs:48, exchange/1104-PAR_CLASH_AllInOne_25mm_FIXED.xml
- what      `public CategoryRewrite(string setName, string from, string to)` carries no note. `MatrixCorrections` has 0 references in `src`, so its `CorrectionOutcome.Lines()` is never written by the tool. The only places that say BLD-EL-Devices is the fallback for an unmeasured negated form are a comment, `scan.md` 5g and `steps/log.md`. A person who picks the exchange file sees `SET` lines asking `Category equals "Nurse Call Devices"` and nothing saying a different form was intended
- expected  The brief asked that the log say which form was used. There is no log, because the file is static and the rule that made it is never run by the tool
- costs     Once 5g is measured and the negated form is written, nobody reading a run can tell which file a group was built from
- fix       The smallest: a `Note` on `CategoryRewrite` and the sentence in the HEALTH block when the picked file's Devices set asks for Nurse Call Devices, "this is F87's fallback, scan.md 5g". Or accept that a static file needs no log and say so in 5g. This is also a public type nothing in `src` calls, which CLAUDE.md forbids, so either the tool applies the corrections at pick time or the class moves to `tools`
- proof     None mechanical

### A16  Nine public members have no running caller
- severity  NOISE
- file      src/Federator.Core/Clash/CreationPlan.cs `BlockCountLine`; Clash/ToleranceChoice.cs `ReadFromLine`; Report/WorkbookCheck.cs `Of(string, bool)`; Clash/PenetrationSettings.cs `IsDecided`; Clash/StatusesThisToolMaySet.cs `WhyNotAsUndo`; Clash/ByDesignPairs.cs `NotMatched`, `PairsSeen`; Probe/ProbeSettings.cs `CsvPathFor`, `MayRead`
- what      Each is called only from `tests`. Grepped across `src` with definitions and comments excluded: 0 call sites
- expected  CLAUDE.md: "No member is added that no running code calls." Most of these are the wiring A1 to A8 will call, which is why they exist, but the rule has no exception for that
- costs     Nothing today. The cost is the rule being false in nine places, and a later reader not knowing which members are waiting on wiring and which are dead
- fix       Either the wiring lands and they gain callers, or each carries a one line comment naming the step in `03_bader_next.md` that will call it. `IsDecided` is different: only a test calls it and no step will, so it is a test helper living in `src`
- proof     grep, as above

### A17  A test named for what is not written checks only that the plan exists
- severity  NOISE
- file      tests/Federator.Core.Tests/Views/ClashViewpointPlanTests.cs:439-443
- what      `ThePlanIsBuiltAndNothingWritesItYet` asserts `Plan(false, Simple(...)).Planned.Count, Is.EqualTo(1)`. It says nothing about writing. Flip `SavedViewpoints.CanBuild` to true and it still passes
- expected  A test whose name promises "nothing writes it" checks that nothing writes it, or is named for what it checks
- costs     A reader trusts the name and believes the not writing is pinned. It is pinned nowhere in Core, because `CanBuild` is in the add-in
- fix       Delete it, or rename it `ThePlanIsBuiltFromOneClash` and put the CanBuild sentence in the fixture comment where it already is
- proof     Nothing in Core can assert `CanBuild`, which is the point

### A18  A test named for five examples and a count asserts only a count of eight
- severity  NOISE
- file      tests/Federator.Core.Tests/Health/SetWarningsTests.cs:300-322
- what      `AFindingListShowsFiveExamplesAndThenACount` builds eight odd names and asserts `odd.Count, Is.EqualTo(8)`. It never calls `HealthCheckResult.Summary()`, so `Examples()` at `HealthCheckResult.cs`, the five lines and the "and N more" line, is untested. Delete `Examples()` and print all eight and this passes
- expected  A test that breaks one thing and asserts the check names it
- costs     The truncation rule the block depends on can regress silently, which is the fault `SizeTally` was written to guard against
- fix       Run the eight through `HealthCheck.Run` and assert the block carries five names and "and 3 more, counted and not listed"
- proof     Break `Examples()` and watch this stay green

### A19  Every new threshold is tested under and over and not at the number
- severity  NOISE
- file      tests/Federator.Core.Tests/Probe/PropertyProbeTests.cs, Diagnostics/LogTrimmingTests.cs, Report/PriorityMapTests.cs, Health/SetWarningsTests.cs
- what      `ProbeTally(100)` is tested with 150 values and with 1 value, never with exactly 100, which is the case that must write no cap line, nor 101, which is the first that must. `RunLog.KeptOfARepeat = 5` is tested with 20 and with 1, never with exactly 5, which must write five lines and no "counted and not written" line, nor 6. `PriorityMap.ExamplesShown`, `TestDrift.ExamplesShown`, `HealthCheckResult.ExamplesShown` and `SetsAcrossTheRun.ExamplesShown` are each tested well over and never at. `ModelLoadWait`'s ceiling has at and under, and `ToleranceChoice.Of` has zero and below, which are the two done right
- expected  The brief for this audit: every threshold needs the value itself, one under and one over. The 150 has all three
- costs     An off by one in any of the six, `<` for `<=`, passes today
- fix       One test per threshold at the number and one past it
- proof     Change `already < KeptOfARepeat` to `<=` in `RunLog.NumberedRepeat` and run the fixture. It should go red and today it stays green

### A20  The undo says a person moved a clash on when they moved it back
- severity  NOISE, PLAUSIBLE
- file      src/Federator.Core/Clash/UndoAutoReview.cs:67 and :84
- what      Any clash carrying our record that is not at Reviewed gets `UndoVerdict.SomebodyMovedItOn`, described as "this tool moved it and somebody has since moved it on, so that stands". A clash at New or Active carrying the record was moved BACK, by hand or by an earlier undo, and the line says on
- expected  The verdict is right, leave it alone. The words are not
- costs     A person reading the UNDO block after undoing twice is told they moved clashes on that they moved back, and goes looking for who
- fix       "somebody has since moved it, to " + status + ", so that stands"
- proof     `AClashSomebodyHasSinceMovedOnIsLeftAlone` loops New and Active through the same verdict, so the wording is pinned as is

### A21  The open file run feeds the sets tally and never writes the block
- severity  NOISE
- file      src/Federator.Addin/Engine/FederationEngine.cs `BuildTheSets`, src/Federator.Addin/Ui/FederatorWindow.xaml.cs `RunJobs`
- what      `setsAcrossTheRun.Add(sets)` sits in `BuildTheSets`, which `RunOpenDocument` also calls, and `log.Block(SetsAcrossTheRun.BlockTitle, ...)` is written only by the window's `RunJobs`. The open file run accumulates a tally nothing prints
- expected  The map for F82 named this as a decision to make and say. It was not made
- costs     Nothing lost. A reader of an open file log finds no SETS ACROSS THE RUN block and does not know whether that is by design
- fix       Either write the block on the open file tail too, or say in one line at the open file tail that a one file run has no across the run to report
- proof     Run the open file and search the log for the block title

### A22  One RESULT label is a character short
- severity  NOISE
- file      src/Federator.Core/Clash/ByDesignTally.cs:233
- what      `"by design     : "` is 16 characters. Every other label in that block is 17: `"penetrations   : "`, `"by priority    : "`, `"first run      : "`, `"NWF read empty : "`
- expected  The block is a column
- costs     One number sits one space left of the rest
- fix       One more space
- proof     Print the block

### A23  Thirteen new files hold more than one type
- severity  NOISE
- file      src/Federator.Core/Views/ClashViewpointPlan.cs (five types), Clash/UndoAutoReview.cs (three), Health/SetWarnings.cs (four), Sets/SetsAcrossTheRun.cs (three), Clash/ByDesignPairs.cs, Clash/ByDesignTally.cs, Clash/AutoReviewRecord.cs, Clash/PriorityMap.cs (with `Csv`), Clash/ClashPriority.cs, Clash/ToleranceChoice.cs, Views/DisciplinePairRule.cs, Rerun/ModelLoadWait.cs, Diagnostics/CensusMove.cs is one
- what      `01_next.md:475` "F37 One type per file and one place per kind of file", done 2026-09-12, "Thirty files out of nine". The round put them back in
- expected  F37
- costs     `Csv` inside `PriorityMap.cs` is the one that will bite: `ByDesignPairs` reads it and nobody looking for a CSV reader will look in a file called PriorityMap
- fix       Move `Csv` to its own file first. The rest when each is next touched
- proof     None mechanical. F37 was a manual pass too

## Checked and clean

- CHECK 0: the build was attempted with the exact path given and with the local path. Both fail before compiling on the Navisworks reference. Recorded at the top
- CHECK 1, wired end to end and reached on a run: F73 (`CensusRule.Judge` through `RunLog.CensusAfter` at `RunLog.cs:975-976`, and the engine still drains `CensusFaults`), F78 (`PlannedSet.Describe` at `SetBuilder.cs:138`), F79 (`IdSourceLines` at `FederationEngine.cs:1576`), F80 (`RunStarted` and `RunFinished` at the two window lines), F81 (four call sites in `ClashRunner` and `FederationEngine`, `TestDrift.Lines` at `ClashRunner.cs:342`, the CLASH block at `FederationEngine.cs:1556`), F82 (`FederationEngine.cs:78-83` and the window), F84's two measured thirds (`SetWarnings` through `HealthCheck.Run`, which the window's `Describe` calls), F87 and F88 (files), F72a (a list and a test)
- CHECK 1: dropping the per group SETS block lost no skipped set name. `SetBuilder.cs:47` writes `SET      SKIPPED` live per set
- CHECK 2 by reading, the four add-in files against the Core they call: `NumberedRepeat` takes six strings and is given six, `EventRow.Exact(double)` exists at `EventRow.cs:199`, `ClashTestResult.Passed` is public, `RunStarted(int)` is given `jobs.Count`, `SetsAcrossTheRun` and `SetRunRow` are under `Federator.Core.Sets` which both files import, `Block(string, IList<string>)` and `Row(string, string, string, string)` are public, `ViewpointPlan.For(IList<string>, ...)` still matches `FederationJob.Disciplines` which is `IList<string>`, `WorkbookCheck.Of(string)` was kept, `HealthCheckResult`'s widened constructor is internal with one caller, `RunPath.All` is not sized anywhere in the add-in, no local named `row`, `where` or `spent` shadows an enclosing one. `check-imports.sh` and `check-locals.sh` clean. PLAUSIBLE clean, not CONFIRMED, for the reason at the top
- CHECK 3: no Autodesk type in `Federator.Core`. One mention, `InstallFiles.cs:92`, is a string in a log line
- CHECK 3: `SizeOnDisk(null)` returns minus one, so the new file size lines cannot throw when the row log never opened
- CHECK 3: `RunLog.WriteResultBlock` reads the run clock once and reuses it, so the timing block and the RESULT lines cannot carry two session totals
- CHECK 4: no test walks folders by name any more. `Samples.Repo` walks for the solution file by exact name and both folders join onto it, and `SamplesPathTests` pins the decoy. `exchange/**` is `-text`, so the byte for byte comparison reads the same bytes on both runners
- CHECK 4: the 32 skips. 28 need Navisworks on the machine (24 for the stylesheet, 2 for the install, 2 named). 5 are Windows file system rules with `TestPaths.OnWindowsOnly`. All 33 reasons are written beside the skip and all still hold. On the Windows runner 6 of them ran and passed
- CHECK 5: `exchange/` is referenced from `Samples.cs`, `CLAUDE.md` and `.gitattributes` and nowhere else. `.gitattributes:37` covers it the way `samples/**` is covered
- CHECK 5: one pair of same named files that differ, `1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`, already Q52 with its difference pinned by `SuppliedCorrectedMatrixTests`. Every other duplicate basename is a picture inside the three committed report exports, which is what a report export is
- CHECK 5: `scan.md` 5e, 5f, 5g, 5h and 5i each say NOT MEASURED in the heading and again in the first line. Nothing under `src`, `steps`, `.claude` or `CLAUDE.md` says any of the five was measured
- CHECK 5: no file under `src` is in the wrong project. `revit-categories.txt` is in the DLL as an embedded resource, confirmed by reading the DLL
- Q46 to Q52 were read first. None of the findings above repeats one. A11 is about a resource, not the empty list, which is Q47's neighbour and scan.md 5i

## What I could not check

- Whether the add-in compiles. Only `dotnet build` with Navisworks on the machine settles it. Step 379 already says to expect errors and send the list
- Whether `Federator.Addin` and `Federator.Core` still agree at run time on anything under CHECK 1 that IS wired. A wired call is not a working feature until step 380's run
- Whether `Directory.Exists` on the Windows runner is the only case blind call left. `File.Exists` is case blind there too and I did not grep for a second folder walk that uses it. What would settle it: `grep -rn "File.Exists\|Directory.Exists" tests/` and reading each against a sibling that differs only in case
- Whether A11 can happen in a Release build. `strings` on the Debug DLL found the resource. The Release csproj is the same file, so it should, and a Release DLL was not built here
