# 00 analysis

Written 2026-09-06 from a full read of the repo at commit be0b9b3.
Nothing here was changed in the code.

Line numbers are from that commit. They move when the file is edited.

## 1 Workflow map

The real order the code runs, one line per stage.

### Scanned path, the Run button

1. Navisworks reads `bundle/PackageContents.xml` and loads the add-in
2. `FederatorPlugin` static ctor, `BundleAssemblies.InstallBesideThisAssembly`, hooks AssemblyResolve
3. `FederatorPlugin.Execute`, `RunLog.StartOrDisabled`, writes SESSION, deletes old logs to 30, shows `FederatorWindow`
4. Source step, `FederatorWindow.OnBrowseSource` then `OnScan`, lists the NWC files
5. `FederatorWindow.Regroup`, `ContainerName.Parse` per file, `BuildingGrouping.Group` by the chosen `GroupingMode`
6. `OutputNameTable` fills one row per group from the three `NamePattern` values
7. `ScanFindings.From` works out ODD SHAPE, NEAR MATCH, SINGLE DISCIPLINE, MISSING and shows them
8. Clash step, `OnBrowseExchangeFile`, `ExchangeReader.Read`, `HealthCheck.Run` on the XML
9. `FederatorWindow.OnRun`, builds one `FederationJob` per ticked group through `OutputPaths`
10. `OutputNameTable` collision check, then `ConfirmClear` dialog
11. `FederatorWindow.RunJobs`, writes RUN SETTINGS, `new FederationEngine(...)`, `engine.Run(jobs)`
12. `FederationEngine.Run` loops the jobs, `RunOne` per group, `log.GroupFinished` after each
13. `FederationEngine.Decide`, opens the NWF if there, `NwfComparison` on `Model.FileName`, Build, Opened or Changed
14. Build: `BuildFromScratch`, `AppendOne` per NWC, `WriteNwf` saves the NWF. Opened: nothing appended. Changed: nothing touched
15. `DocumentUnits.Apply`, `SetModelUnitsAndTransform` per model, before the clash step
16. `FederationEngine.ClashStep`, returns false with no clash XML
17. `BuildTheSets`, `SetBuilder.Build`, folders and search sets from the XML, existing ones left alone
18. `CreateAndRunTheTests`, `ClashRunner.Run`, index sets and tests once, guard for zero resolvable sets
19. `ClashRunner` per test: resolve by `TestAddress`, `TestsAddCopy` if new, `TestDrift` compare, `SingleModel` and `EmptySide` skips, `TestsRunTest`
20. `ClashHarvest.Into` reads results, items, ids, grid and level per clash into `TestReport`
21. `ClashImages.Write` renders one jpg per clash into the `_files` folder
22. `ClashRunner` Compact, only if ticked, after all tests
23. `FederationEngine.SaveTheNwfAgain`, second NWF save, only if the clash step put something in
24. `FederationEngine.WriteWorkbook`, `WorkbookWriter` in Core writes the one sheet, `WorkbookCheck` reads the file back
25. `WriteHtmlTabular`, `ClashReportXml` then Autodesk's stylesheet through `HtmlTabularWriter`, `PageCheck` reads the page back, logo copied by `CopyLogoIntoTheReportFolder`
26. Clash XML written if ticked, same `ClashReportXml`
27. `FederationEngine.WriteNwd`, `PublishFile`, the Try bool is read
28. `ConfirmTheNwfSurvived`, reads the NWF size once more
29. `GroupJudgement` in Core gives DONE, PARTIAL or FAILED from what was asked for
30. Back in the window, `SourceMismatchFindings`, `WriteTheResultAndCopyTheLog`, RESULT block and log copy beside the NWF

### Open file path, the Run the open file button

1. `FederatorWindow.OnRunOpenDocument`
2. `OpenDocumentJob` in Core names the NWD and the report folder after the open file
3. `FederationEngine.RunOpenDocument`, no Decide, no append, no clear
4. Then stages 15 to 28 above
5. No `GroupFinished`, no RESULT block, no log copy. See B11

## 2 Wiring map

Which class calls which.

### UI

- `FederatorWindow.xaml` is the one window. Four steps as tabs: Source, Grouping, Outputs, Clash
- `FederatorWindow.xaml.cs` holds every handler. 1806 lines. No view model, no commands, no binding beyond the two grids
- The window owns `ContainerNameSettings`, `OutputNaming`, `OutputNameTable`, `FolderMemory`, `ScanFindings`
- `ReportsWanted` and `ImagesWanted` read the tick boxes into `ReportOptions` and `ImageOptions`
- Two hand buttons on the Clash step, `OnBuildSets` and `OnRunTests`, call `SetBuilder` and `ClashRunner` on the open document directly. Not part of the run

### UI to engine

- `OnRun` to `RunJobs` to `FederationEngine.Run`
- `OnRunOpenDocument` to `FederationEngine.RunOpenDocument`
- Progress comes back through an `Action<string>` and the log through `RunLog.LineWritten`
- The engine is built new on every press with the log, the exchange document, the `ReportOptions` and the NWF folder

### Engine to Navisworks API

- `FederationEngine` uses `Document`, `DocumentModels.AppendFile` and `OpenFile`, `SaveFile`, `PublishFile`, `TrySave`
- `DocumentUnits` uses `DocumentModels.SetModelUnitsAndTransform`
- `SetBuilder` uses `DocumentSelectionSets`, `SelectionSet`, `Search`, `SearchCondition`, `AddCopy`, `RootItem`
- `ClashRunner` uses `DocumentClashTests` and its copy mutators, `ClashTest`, `ClashSelection`, `CreateSelectionSource`
- `ClashHarvest` uses `ClashResult`, `ClashResultGroup`, `ModelItem`, `PropertyCategoryCollection`, `VariantData`
- `ClashImages` uses `TestsImageForResult` and `Bitmap.Save`
- `NavisworksFacts` reads the install folder and the language

### Engine to Core

Core has no Navisworks reference. The add-in hands it plain values.

- Naming: `ContainerName.Parse`, `NamePattern`, `OutputNameTable`, `OutputPaths`
- Grouping: `BuildingGrouping.Group`, `GroupingMode`
- Exchange: `ExchangeReader.Read` gives `ExchangeDocument`, `ExchangeUnits` converts tolerance
- Health: `HealthCheck.Run` on the exchange
- Clash: `ClashTestPlan`, `ClashRunOutcome`, `ClashWork.Any`, `TestDrift`, `RepeatedFailureGuard`, `OpenClashes`
- Rerun: `ModelFileNames`, `NwfComparison`, `GroupJudgement`, `OpenDocumentJob`
- Findings: `ScanFindings`, `SourceMismatchFindings`, `FindingsTable`
- Diagnostics: `RunLog`, `BundleAssemblies`, `FolderMemory`, `PickerStart`, `BuildStamp`, `GroupOutcome`
- Report: `WorkbookWriter`, `WorkbookCheck`, `ClashReportXml`, `HtmlTabularWriter`, `PageCheck`, `ReportPaths`, `ImageNaming`, `LogoLocator`, `StylesheetLocator`, `ClientLayout`, `ClientStyle`, `ClientFormat`

### Excel writer

- `ClashHarvest` fills `ClashReport` and `TestReport` in `ClashReportModel.cs`
- `WorkbookWriter.Write` in Core takes the `ClashReport` and writes one sheet with ClosedXML
- `ClientLayout` and `ClientStyle` hold the colours, widths and heights read off the client's export
- `WorkbookCheck.Read` opens the written xlsx as a file and compares cell against cell on the first block
- The HTML page is not written from the workbook. Both come from the same `ClashReport`

## 3 Bugs found

One line each. File, line, what is wrong, what it breaks.

- B1 `src/Federator.Addin/Engine/FederationEngine.cs:653` returns `CreatedCount > AlreadyPresentCount`. A rerun that finds 60 sets present and creates 1 new says nothing was built. Corrected 2026-09-06 while fixing it: the tests are still created and run, because `ClashStep` does not short circuit on this bool. What is lost is the second NWF save, when the file holds sets only or no test was created or run, so the new set is never saved into the NWF
- B2 `src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1425` to `1432` with `FederationEngine.cs:86` to `89`. The open file path passes a folder already ending in `Clash Reports`, the engine appends `Clash Reports` again through `ReportPaths.Choose`. Reports land in `Clash Reports\Clash Reports`. The scan's source folder refusal is also applied to a run that had no scan
- B3 `src/Federator.Addin/Engine/FederationEngine.cs:1112` logs "not republished, the tick box is off". The box no longer exists. The line can only lie
- B4 `src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1168` the confirm dialog says "Before each group the document is cleared". On a rerun with an NWF present nothing is cleared. The person is told the wrong thing before pressing Yes
- B5 `src/Federator.Addin/Engine/ClashRunner.cs:903` to about `940` `Resolve` walks the address and keeps no wrapper for the folder levels it passes through. The intermediate `SavedItem` wrappers are never disposed. Contradicts the dispose rule in CLAUDE.md, leaks finalizable handles on nested tests
- B6 `src/Federator.Addin/Engine/SetBuilder.cs` never disposes any `SavedItem`, `SelectionSet`, `ModelItemCollection` or `Search` it creates or resolves. Same rule, same leak, 61 sets and their item counts per group
- B7 `build/probe-window-defaults.ps1:3` hardcodes `C:\Users\p003653k\source\repos\Parsons NWC Federator`. The other probes use `Split-Path $PSScriptRoot`. Runs on one machine only
- B8 `tests/Federator.Core.Tests/ReportCheckTests.cs:303` test named `ALogoOnTheePageIsCheckedAgainstTheDisk`. Typo only
- B9 `src/Federator.Core/Report/ReportOptions.cs:40` `ClientColumnsOnly` still exists and is set. CLAUDE.md says it is gone. `SheetNames.SummarySheet`, `SheetNames.MatrixSheet`, `ClashMatrix.cs` and `TestReport.SheetName` are dead too and still have tests, so the tests guard code nothing runs
- B10 `src/Federator.Addin/Engine/DocumentUnits.cs:40` docstring says the unit change is off unless asked for. `FederatorWindow.ReportsWanted` sets it on. The comment is wrong
- B11 `src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1390` to `1454` `OnRunOpenDocument` never calls `WriteTheResultAndCopyTheLog`, and `FederationEngine.RunOpenDocument` never calls `log.GroupFinished`. An open file run leaves no RESULT block and no log copy beside the file

- B12 OPEN, waiting on Bader. Bader opened an NWD from ACC and the tool showed an error beside the file. The exact text, the place in the window, the path, and whether Navisworks opens that file on its own are not known yet. See Q20. F23 fixes it once Q20 is answered

## 4 Logic problems

- L1 CLAUDE.md, the button help and `FederatorWindow.xaml.cs:1422` all say an open file with no clash XML runs the tests already in the document. `FederationEngine.ClashStep:605` returns false when `ClashWork.Any(exchange)` is false, and `ClashWork.Any(null)` is false by test. So nothing runs. The ordinary weekly case does nothing and the log says it did
- L2 A CHANGED group is meant to be left alone entirely. `FederationEngine.cs:321` calls `DocumentUnits.Apply` before `ClashStep` returns, so every model in a CHANGED group has its units changed and the document is then saved by nobody. Harmless on disk, wrong by the rule, and the log says units were applied to a group that was supposed to be untouched
- L3 The HTML page is fixed on but only written when `CreateAndRunTheTests` built a `ClashReport`, which needs `WriteWorkbook` or `WriteXml` true. The `WriteHtml` flag alone writes nothing. Today the workbook is always on so this is hidden. It breaks the day someone turns the workbook off
- L4 Picture numbers count tests in run order, the client's count tests by block order in the report. Known and recorded in `docs/scan.md` 4p. Pictures still match their clash by link, so the report reads right, but the file names are not theirs
- L5 CLAUDE.md contradicts itself. It says the Summary, Matrix and per test sheets are GONE and the workbook is ONE SHEET, and it still carries paragraphs on sheets T0001 upward, the Summary row per test and the matrix cell. A reader cannot tell which rule holds without reading the code
- L6 `docs/scan.md` in its Settled 2026-08-28 section says the name floor is seven parts with `LevelPart` and `NumberPart` settings. `ContainerNameSettings.MinimumParts` is 5 and neither setting exists. CLAUDE.md says five. The doc is stale
- L7 `docs/test-model-side.md` carries superseded steps next to the current ones. It says no Excel yet, images not written, logo not done, Summary and Matrix sheets, a Republish tick box and Client columns only. All of those changed. A person following it will look for things that are not there
- L8 The brief says "one sheet per test, all columns, clash images included". The repo says one sheet laid out as the client's report, with per test sheets removed on purpose. One of the two is the ask. See Q1

## 5 Missing pieces against the done criteria

Done means all three. None is proved.

- M1 Criterion 1, one building end to end with no errors. No log in the repo shows a clean end to end run on the current code. The OPENED rerun path has never been seen in a real log. Needs a run. LOCAL MACHINE ONLY
- M2 Criterion 2, every ticked building unattended under 45 minutes. No timing for six buildings exists. `docs/scan.md` shows one clash step growing from 39.8 s to 1259.5 s across runs with no cause found. The per test cost of `GetSelectedItems` on both sides is not measured. Needs a timed run. LOCAL MACHINE ONLY
- M3 Criterion 3, Excel counts match the Clash Detective panel. `CreateSelectionSource` is used so the counts should agree. No comparison against the panel is recorded anywhere. Needs a run and a screenshot or a count from the panel. LOCAL MACHINE ONLY
- M4 No `.claude` folder, no `.claude/rules`. CLAUDE.md is 893 lines and holds the rules, the history and the measurements together. See section 8
- M5 CI runs the Core tests only, on `windows-latest`. The add-in is never built in CI, so a compile error in `Federator.Addin` reaches main unseen. 32 tests `Assert.Ignore` on the runner because it has no Navisworks
- M6 No README at the repo root. A person landing on GitHub sees CLAUDE.md or nothing
- M7 The 1A04WE client export, the one with the Layer column, is named in the docs and is not in `samples/client-report`. The two there are 1A02WN and 1A04WN. The Layer case has no sample to test against
- M8 The tests are Windows only. 40 of 868 fail on Linux under mono on path separators, case, file locking and the illegal character set. They pass on Windows per the docs. Not verified on Windows in this pass

## 6 Tests

### What exists

- 60 test files in `tests/Federator.Core.Tests`, 868 tests, NUnit 3.14, net48
- Run in the container under mono: 796 passed, 40 failed, 32 skipped, 16 seconds
- The 40 failures are all Linux artefacts. Expected `C:\out\nwf\x`, got `C:\out\nwf/x`. Expected an IOException reading a live log. Expected `Q:\nowhere` to be unwritable. Expected `yyyy:MM` to be refused. A literal `samples\` in a path. A locked file delete
- The 32 skips need a Navisworks install for the stylesheet, the logo or the language folder

### What it covers

- Name parsing, settings, the five part floor, the pattern and the name table with per cell overrides
- Grouping in all four modes and the collision check
- Exchange reader on the reference file, sets, tests, units, the health check on the damaged exports
- Clash plan, outcome, skip summary, drift compare, the repeated failure guard, the open count setting
- Run log: flush, share aware read, retention, dedupe of repeated traces, the RESULT block from one list, sizes read back, authorship
- Findings, source mismatch, shared source, the copy table
- Folder memory per picker and the picker start rule
- Rerun: `ModelFileNames`, `NwfComparison`, `GroupJudgement`, `OpenDocumentJob` naming
- Report: workbook writer, workbook check, cell check with 14 break one thing tests, client layout against the export, format, columns against the stylesheet and the samples, clash XML, HTML writer, logo locator, image naming, sheet names
- `BundleAssemblies` and the build stamp

### What is untested

- Everything in `src/Federator.Addin`. `FederationEngine`, `ClashRunner`, `ClashHarvest`, `ClashImages`, `SetBuilder`, `DocumentUnits`, `DocumentGuard`, `NavisworksFacts`, `FederatorPlugin`, `FederatorWindow`
- The rules that live only there: the Decide branch, the second NWF save, the NWD after the clash step, the dispose rule, the eEXTERNAL re-resolve rule, the units change
- The dead code in B9 is tested and nothing runs it
- B1, B2, L1, L2 and L3 are all in the add-in and all sit where no test can reach

## 7 Hooks and .github/workflows

### What exists

- `.githooks/pre-commit` runs the Core tests and refuses the commit on a failure or when dotnet is missing. Turned on per clone with `git config core.hooksPath .githooks`. Not on in this container until set
- `.github/workflows/tests.yml` on `pull_request` and `workflow_dispatch`, `windows-latest`, .NET 8, restore and `dotnet test` of the Core test project

### What is missing

- No build of `Federator.Addin` anywhere in CI. Needs Navisworks, so it needs a self hosted runner or stays LOCAL MACHINE ONLY
- No workflow on push to main. A direct push skips the tests
- No PR template
- No workflow for `install.ps1` or the probe scripts
- No `.claude` folder, no rules, no hooks for Claude
- The pre-commit hook is opt in, so a fresh clone commits without tests

## 8 CLAUDE.md

- 893 lines by `wc -l`, 64083 bytes
- It holds three kinds of text mixed together: rules the code must keep, the history of why each rule exists, and measurements with dates
- It contradicts itself in at least one place. See L5
- It needs splitting. Suggested shape, not done, waiting on Q5:
  - `CLAUDE.md` under 150 lines: what the tool is, host, done criteria, build, writing rules
  - `.claude/rules/naming.md` the file naming and the pattern rules
  - `.claude/rules/rerun.md` never clear an NWF, Decide, drift, compact, dispose
  - `.claude/rules/report.md` the one sheet, the page, the images, the checks
  - `.claude/rules/log.md` the diagnostic log
  - `.claude/rules/ui.md` what a tick box says
  - the measurements and the history move to `docs/`, which already holds `scan.md`
