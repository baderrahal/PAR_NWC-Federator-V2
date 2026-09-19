# 01 next

Fixes in the order Bader set on 2026-09-06. The F numbers are kept from the first list.
One PR per fix. F1 to F4 go in one PR. The worker merges its own PR.

`CONTAINER` means the fix can be built and tested here.
`LOCAL MACHINE ONLY` means it needs Navisworks to prove.
The add-in does not compile in the container at all, so every add-in edit
can be written here and only proved on the local machine.

## Order

Renumbered on 2026-09-18 when the feature round closed. F51, F50, F52, F53, F54 and F55 are all done and each carries its DONE line below. The three that were left before the round are left after it, and only the first of them can be started here.

Renumbered again on 2026-09-19 when the log round opened. Bader briefed six fixes as F56 to F61. F56 and F57 were already taken, so the six are F59 to F64 and they map onto the brief one for one and in order. F51, which the brief asks for as its own one line pull request, was already done and merged on 2026-09-18 and is not done twice. F58 is a seventh, found by the reading that opened the round and put first because the add-in does not compile and every one of the 251 steps waits behind the build.

1. F58, the add-in compiles again. `BuildViewpoints` declares one name twice, which is CS0128
2. F59, every step is named and timed
3. F60, the timing blocks, and F21 closes here
4. F61, the document census
5. F62, the live line in the window
6. F63, the report gap block
7. F64, the machine readable log
8. F57, five Look for lines older than the feature round, for Bader to judge
9. F18, when Bader uploads the 1A04WE sample, Q9
10. F23, when Q20 is answered

Nothing else is open. F19 is dropped.

## F47 What the chat audit of 2026-09-18 found

Three things, none of them a rule the code breaks at run time. The first is the one that matters, because it takes both walls out on the machine the tool is actually developed on.

### F47a The hooks do not run on a Windows checkout

- Files `.gitattributes` which is new, `steps/03_bader_next.md`
- CONTAINER for the fix and for both proofs
- The repo has no `.gitattributes`, so Git for Windows converts LF to CRLF on checkout, because its installer sets `core.autocrlf` to true. `sh` reads a carriage return as part of the word, so all three hook files die on their first `case` line before they reach a rule. `sh` exits 2 on a syntax error and 2 is the code that REFUSES, so the two Claude Code hooks then refuse every call rather than the protected ones, and the pre-commit stumbles past `set -e` and lets every commit through untested
- Size: small. One new file, and the proof is what takes the time
- DONE on 2026-09-18. `.gitattributes` added, `text=auto` by default, `eol=lf` forced on `*.sh` and on `.githooks/pre-commit` by path, `steps/logs` and `samples` marked `-text` so evidence is never normalised. `git add --renormalize` over the whole repo staged no file, because everything was already stored the right way here. Both walls proved on this machine in twelve cases, and the pre-commit in three. D7 in `03_bader_next.md` is how Bader sees it for himself

### F47b One doubled comment left

- Files `src/Federator.Addin/Engine/ClashRunner.cs`
- CONTAINER
- Two summary blocks stacked at line 1129. The first describes `Count`, twelve lines below, which F45 left with no comment of its own when it inserted `ResolvedInTheDocument` above it. F44 said the doubled comments were done and this one was left
- Size: one comment moved
- DONE on 2026-09-18. Moved rather than deleted, because it describes `Count`, which is still there and had lost its own comment. F44's own check rerun over the whole of src reads 0 stacked, where it read 1

### F47c Two names recorded wrongly in the F40 entry

- Files `src/Federator.Core/Report/WorkbookWriter.cs`, `steps/log.md`, `steps/02_questions.md`
- CONTAINER
- `WorkbookWriter.ClientColumns` is listed in the F40 entry as deleted and is still there, public, with no reference anywhere in src, the XAML or the tests. Its comment says it exists so a test can assert the header, and no test does, so the reason is not true and the rule decides. `ReportOptions.FolderFor` is listed in the same entry as deleted and was made internal. Both lines corrected in place, and every other name on that list checked the same way
- Size: one member, two log lines, and the check over the rest of the list
- DONE on 2026-09-18. `WorkbookWriter.ClientColumns` deleted, nothing referenced it anywhere. All 53 names on the F40 outright list read against the code and against the F40 commit itself. Five were recorded wrongly, not two, and each is marked in the list in place. The count of 45 counts five that did not go, which is Q31. Core tests before and after: 957 passed, 0 failed, 32 skipped, 989 total

## Done, in the order they were worked

F5, F6, F7, F8, F22, then F1 with F2 and F4 in one pull request, F3 inside F22, then F9, F10, F11, F17, F20, F24, F26, F27 which closes F25, F28, F29, F30, F31, F32, F33, F34, F35, F36, F37 which closes F12 and F14, F38 which closes F13, F39, then the second audit round: F46, F40, F41 which closes F15, F42, F43, F44, F45 and F16, then the third: F47a, F47b and F47c, then the feature round: F51, F50, F52, F53, F54 and F55.

Every one of them carries its DONE line in its own section below, and its entry in `log.md`. None of them is proved on a machine with Navisworks yet. That is `03_bader_next.md`.

## F5 Fix the sets built test

- Closes B1
- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Core/Sets/SetBuildOutcome.cs`
- LOCAL MACHINE ONLY to prove
- Size: one expression. Done in code on 2026-09-06, proof pending. See `log.md`

## F6 Fix the open file report folder

- Closes B2
- Files `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, maybe `src/Federator.Core/Report/ReportPaths.cs`
- CONTAINER for a Core test on the folder rule, LOCAL MACHINE ONLY to prove the run
- Size: small. Hand the engine the folder that already has the subfolder, or hand it the parent. Add one test in `OpenDocumentJobTests`

## F7 Write the RESULT block for the open file run

- Closes B11
- Files `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`
- LOCAL MACHINE ONLY to prove
- Size: small. Call `GroupFinished` in `RunOpenDocument` and `WriteTheResultAndCopyTheLog` in the finally, with the file's own folder

## F8 Run the existing tests when no XML is picked

- Closes L1
- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`, `src/Federator.Core/Clash/ClashWork.cs`
- CONTAINER for the Core side, LOCAL MACHINE ONLY to prove
- Size: medium. Done in code on 2026-09-06, proof pending. See `log.md`

## F22 Two clear workflows in the window

- Closes part of L1 and Q17, and closes F3 with it
- Files `src/Federator.Core/Rerun/RunPath.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `src/Federator.Addin/Ui/GroupRow.cs`, `src/Federator.Core/Diagnostics/RunLog.cs`
- CONTAINER for the label rule in Core, LOCAL MACHINE ONLY to see
- Confirmed by Bader on 7 Sep 2026 with these two definitions
- First run. The NWC folder is scanned and grouped, output folders picked, a clash XML picked, Run pressed. Per group the tool builds the NWF, builds the sets, adds and runs the tests from the XML, writes Excel, HTML, images and the NWD
- Weekly run. The same NWC folder, Run pressed with no XML. Per group the tool finds the NWF already there, opens it (OPENED), lets Navisworks reload the newer NWCs, runs the tests saved inside the NWF, writes Excel, HTML, images and the NWD. An XML is optional and only adds or updates tests
- The window shows one label per group after Scan and before Run, the confirm dialog counts them, and the log carries them. The labels are First run, Weekly run, Weekly run plus XML, Skipped (changed on disk), Unknown
- Done in code on 2026-09-07, proof pending. See `log.md`

## F23 Fix B12, the NWD opened from ACC

- Closes B12
- Narrowed by F32 on 2026-09-12. A document opened from an address rather than a folder, one that is not an NWF, one with no folder in front of its name, and one whose folder cannot be read are each refused with the reason in the window and the log. What Navisworks reports as the file name of a document opened from Autodesk Docs is still UNKNOWN, so the ACC case itself waits on Q20
- Files UNKNOWN until Q20 is answered
- LOCAL MACHINE ONLY to see, CONTAINER to read
- Before the fix, read every code path that touches a file path and list in the PR every place the add-in refuses or fails on a path, so the ACC case can be matched when the details arrive
- Waits on Q20

## F1 Fix the test name typo

- Closes B8
- Files `tests/Federator.Core.Tests/Report/ReportCheckTests.cs`
- DONE on 2026-09-07 in one PR with F2 and F4

## F2 Fix the hardcoded probe path

- Closes B7
- Files `tools/probes/probe-window-defaults.ps1`, `docs/history/test-model-side.md`, both moved there by F37
- DONE on 2026-09-07 in one PR with F1 and F4, pending a local run of the probe

## F3 Fix the two wrong messages

- Closes B3 and B4
- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- DONE on 2026-09-07 inside F22. The confirm dialog says cleared only for First run groups, Q13, and the NWD line no longer names a tick box that is gone
- No longer part of the F1 to F4 PR, which is now F1, F2 and F4

## F4 Fix the units docstring

- Closes B10
- Files `src/Federator.Addin/Engine/DocumentUnits.cs`
- DONE on 2026-09-07 in one PR with F1 and F2

## F9 Skip the units change on a CHANGED group

- Closes L2
- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Core/Rerun/NwfComparison.cs`, `src/Federator.Core/Rerun/GroupJudgement.cs`
- DONE on 2026-09-07, proof pending. See `log.md`. Q12 answered yes

## F10 Gate the page on its own flag

- Closes L3
- Files `src/Federator.Core/Report/OutputPlan.cs`, `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`
- DONE on 2026-09-07, proof pending. See `log.md`

## F11 Remove the dead code

- Closes B9
- Files `src/Federator.Core/Report/ReportOptions.cs`, `SheetNames.cs`, `ClashMatrix.cs`, `ClashReportModel.cs`, `src/Federator.Core/Clash/OpenClashes.cs`, their tests, CLAUDE.md, `docs/test-model-side.md`
- DONE on 2026-09-07. The local proof is only that the add-in builds. Q4 answered yes

## F17 Picture numbering by block order

- Closes L4
- Files `src/Federator.Core/Report/ReportOrder.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Engine/ClashImages.cs`
- LOCAL MACHINE ONLY to prove
- Size: medium, and pictures would need renaming after the sort. Q8 answered match the Navisworks export order
- DONE on 2026-09-07, proof pending. See `log.md`. The pictures are rendered under the run order and renamed once after the run, in one pass

## F15 Dispose in SetBuilder and ClashRunner.Resolve

- CLOSED on 2026-09-12 by F41, which is the same two files and more. B5, the intermediate levels `Resolve` walks past, and B6, everything `SetBuilder` creates or resolves, are both done, and `SavedTests` went with them. The proof is the same one F15 asked for, a run with no ObjectDisposedException, and it is steps 188 to 190 of `03_bader_next.md`

## F12 Fix the docs that contradict the code

- CLOSED by F37 on 2026-09-12. The two docs move to `docs/history` with a first line saying they are measurement history, and `docs/workflow.md` becomes the one current description
- Closes L5, L6, L7
- Files `CLAUDE.md`, `docs/scan.md`, `docs/test-model-side.md`
- CONTAINER
- Size: medium, reading and cutting. No code

## F13 Split CLAUDE.md into rules

- CLOSED by F38 on 2026-09-12. CLAUDE.md holds the rules for every file in 164 lines, the rules per folder are under `.claude/rules`, and the old file is whole in `docs/history/claude-md-history.md`
- Closes M4, part of L5
- Files `CLAUDE.md`, new `.claude/rules/*.md`, `docs/`
- CONTAINER
- Size: large for reading, small for typing. Q5 answered yes

## F14 Add a root README

- CLOSED by F37 on 2026-09-12. README.md exists since F20 and F37 points it at `docs/workflow.md`
- Closes M6
- Files `README.md`
- CONTAINER
- Size: one page

## F16 Make the tests path neutral

- Closes M8
- Files about 15 test files
- CONTAINER, then confirm on Windows
- Size: medium. Q11 answered yes
- Widened by the audit of 2026-09-12, see `04_audit.md`. The 37 tests that fail here, the six that skip saying a sample is missing when it is there, the three ReadOnlyCollection against array comparisons, `ContainerNameTests.ReadsAFullPathThroughToTheName` which passes off Windows without proving anything, `Samples.Folder` in place of the four `RepoRoot` copies, and one temp folder helper in place of the nineteen copies of its comment
- DONE on 2026-09-12. The whole set passes in the container, 957 passed, 0 failed, 32 skipped, 989 total, where it was 37 failing. Five of the 37 skip because they are about a Windows file system rule and two were proved another way instead. One Core fault came out of it: both places that asked the running platform what a file name may not carry now read Windows' own list from `FileNames`

## F18 Add the 1A04WE sample

- Closes M7
- Files `samples/client-report/`
- Bader uploads the file later, Q9. CONTAINER once it is in
- Size: a commit

## F20 Run the Core tests on every push to main

- Closes part of M5
- Files `.github/workflows/tests.yml`
- CONTAINER to edit, GitHub to prove
- Size: add `push` with `branches: [main]` to the `on` block. Same job as today. Q18 answered yes
- DONE on 2026-09-07. The proof is the push run on main after the merge

## F24 Rebuild a CHANGED NWF from the scan

- Closes B13, Q22
- Files `src/Federator.Core/Rerun/NwfRebuildPlan.cs`, `NwfComparison.cs`, `RunPath.cs`, `GroupJudgement.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- CONTAINER for the plan, the label and the judgement, LOCAL MACHINE ONLY to prove the rebuild and whether the clear keeps the saved tests
- DONE on 2026-09-07, proof pending. See `log.md`. The proof is first in `03_bader_next.md`

## F26 Units always meters

- Closes B14, Q23
- Files `src/Federator.Addin/Engine/DocumentUnits.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Core/Rerun/GroupJudgement.cs`
- LOCAL MACHINE ONLY to prove, CONTAINER for the judgement
- The report is always in meters, never feet. Force the document to meters and fail the group loudly if it will not follow. The run log of 2026-09-07 says THE DOCUMENT DID NOT FOLLOW on 11 of 14 groups
- Size: medium. Which call moves Document.Units is UNKNOWN, docs/scan.md says Document.Units is read only and only the models can be set
- DONE on 2026-09-07, proof pending. See `log.md`. The numbers are converted in Core rather than waiting for the document to follow

## F27 The GROUPS block tells the truth

- Closes F25, B15 and Q21
- Files `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `README.md`, `steps/00_analysis.md`
- CONTAINER for the line, LOCAL MACHINE ONLY to see the block
- DONE on 2026-09-12. The block says unticked instead of skipped and ends with one line counting the unticked groups

## F28 A set already there is not counted as created

- Files `src/Federator.Core/Sets/SetBuildOutcome.cs`, `src/Federator.Addin/Engine/SetBuilder.cs`, `tests/Federator.Core.Tests/Sets/SetBuildOutcomeTests.cs`
- CONTAINER for the outcome, LOCAL MACHINE ONLY to see the second NWF save stop on a weekly run
- `BuildOne` calls `AddAlreadyPresent` and then `AddCreated` for the same set, so `CreatedCount` and `PutAnythingIn` count present sets and the second NWF save fires on every weekly run
- DONE on 2026-09-12, proof pending on the local machine

## F29 The rebuild keeps the sets on their own count

- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Core/Rerun/NwfRebuildPlan.cs`
- CONTAINER for the set line and the judgement, LOCAL MACHINE ONLY to prove the restore
- The sets are counted before the clear and after the appends and put back on their own count, `setsCopy` is disposed, the dead Changed branch at the top of `ClashStep` goes
- DONE on 2026-09-12, proof pending on the local machine

## F30 One tail for both run paths

- Files `src/Federator.Addin/Engine/FederationEngine.cs`
- LOCAL MACHINE ONLY, the add-in does not build here
- `RunOne` and `RunOpenDocument` share one private tail, the PR body lists every line that still differs
- DONE on 2026-09-12, proof pending on the local machine

## F31 The clash side lookup is built once per run

- Files `src/Federator.Addin/Engine/ClashRunner.cs`
- LOCAL MACHINE ONLY
- One `SelectionSource` per indexed set, built in `IndexSets`, disposed with the set wrappers in one finally at the end of `Run`. `FillSide` loses its unused document parameter. `Resolve`, `Upwards` and `SetBuilder` stay for F15
- DONE on 2026-09-12, proof pending on the local machine

## F32 The open file is guarded

- Files `src/Federator.Core/Rerun/OpenDocumentJob.cs`, its tests, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- CONTAINER for the rule, LOCAL MACHINE ONLY to see the refusal
- D2. A path whose extension is not .nwf, a path whose folder cannot be read, and an NWD path equal to the open path are refused with the reason named. Narrows B12, the ACC case still waits on Q20
- DONE on 2026-09-12. The window already shows `WhyNot` through `Describe` and disables the button, so no window line changed

## F33 One unit table

- Files new `src/Federator.Core/Units/UnitTable.cs`, `ExchangeUnits.cs`, `ExchangeReader.cs`, `ExchangeModel.cs`, `ClashTestPlan.cs`, `DocumentUnits.cs`, `ClashRunner.cs`, `FederationEngine.cs`, `FederatorWindow.xaml.cs`
- CONTAINER for the table, LOCAL MACHINE ONLY for the window and the engine
- One table, every other unit list deleted. `WantedUnits` fails the group on a name the table does not know. `ClashTestPlan.Convert` is the one place a file unit is judged
- DONE on 2026-09-12, proof pending on the local machine for the engine and the window

## F34 Window wiring

- Files `src/Federator.Addin/Ui/FederatorWindow.xaml`, `FederatorWindow.xaml.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `FederationJob.cs`, `src/Federator.Core/Report/ReportOptions.cs`, `ClashReportModel.cs`, `src/Federator.Core/Diagnostics/GroupOutcome.cs`
- LOCAL MACHINE ONLY for the window, CONTAINER for the option defaults
- D1 HealthCheck wired under the file line, D3 the outstanding count setting deleted, D4 the two hand buttons share the engine wording. The NWD folder box refreshes the summary, the XML box has a handler named for it, the republish field and the three unused engine constructors go
- DONE on 2026-09-12, proof pending on the local machine for the window and the engine. `NwdRequested` stays on `GroupFacts` because `GroupJudgement` reads it, `JobOutcome` no longer carries it

## F35 Clash only where two disciplines meet

- Files `src/Federator.Core/Grouping/BuildingGroup.cs`, `src/Federator.Core/Clash/ClashTestPlan.cs`, `src/Federator.Addin/Engine/FederationJob.cs`, `FederationEngine.cs`, `ClashRunner.cs`, `src/Federator.Addin/Ui/GroupRow.cs`
- CONTAINER for the count, LOCAL MACHINE ONLY to see the status
- D5. A group with fewer than two disciplines creates every test and runs none, the same as a one model group today. `ClashSkipReason.SingleModel` becomes `SingleDiscipline`
- DONE on 2026-09-12, proof pending on the local machine for the row status and the CLASH line

## F36 Dead code and copies out

- Files across `src` and `tests`, `build/probe-window-defaults.ps1`
- CONTAINER
- Every name grepped before it goes, one `Or` helper in `Words.cs`, one client column list, one `InstallFiles` locator, every doubled summary block fixed
- DONE on 2026-09-12. `RunPath.Skipped` stays, a failed rebuild reaches it. `ClashWork.Any(exchange)` stays, `SourceFor` and `Describe` call it, the two argument `Any` and `AnyIn` went. `GroupRecords` stays public, a test reads it

## F37 One type per file and one place per kind of file

- Moves only, no logic. Closes F12 and F14
- CONTAINER
- One type per file in nine files, the tests into src folder names, the probes into `tools/probes` with a README and a path parameter, the two docs into `docs/history`, `docs/workflow.md` written
- DONE on 2026-09-12. Thirty files out of nine, sixty five test files into ten folders, six probes moved with a `-NavisworksPath` parameter, two docs into `docs/history` with a first line, `docs/workflow.md` written and README pointing at it

## F38 CLAUDE.md under 200 lines, rules in .claude, walls in hooks

- Closes F13
- CONTAINER
- History to `docs/history/claude-md-history.md`, four rules files under `.claude/rules` with paths frontmatter, two PreToolUse hooks under `.claude/hooks` in `.claude/settings.json`, the pre-commit hook kept
- DONE on 2026-09-12. CLAUDE.md is 164 lines, `.claude/rules` holds addin, core, tests and steps with a paths line each, `.claude/settings.json` runs two hooks from `.claude/hooks` and both were tried with piped tool calls, the old CLAUDE.md is whole in `docs/history/claude-md-history.md`, the pre-commit hook is unchanged

## F39 The window compiles again

- Found by the audit of 2026-09-12, reader steps-and-rules, and confirmed by grep
- Files `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- CONTAINER to edit, LOCAL MACHINE ONLY to prove, because the add-in does not build here
- F34 deleted the republish flag and the three engine constructors that took it, and left the two calls in the window that passed `true` as the third argument. The Run button and the Run the open file button both construct the engine that way, so the add-in has not compiled since F34 merged and F35, F36 and F37 were read, never built. The fix is the two calls losing the `true`
- Size: two lines
- DONE on 2026-09-12. Both calls read `SetProgress, log, exchange, options` with `nwfFolder` on the first, matching the two constructors. A heuristic check of every `new` and every static call in the add-in against the declared arities finds nothing else. The proof is the build in `03_bader_next.md`, and it is the reason the build comes first

## F46 The four the chat audit found

- From the audit in chat on 2026-09-12, which read main after the round closed. Two of the four break Bader's next hour, so they come before F40
- Files `steps/03_bader_next.md`, `.gitignore`, `steps/README.md`, `src/Federator.Core/Diagnostics/RunLog.cs`, `tests/Federator.Core.Tests/Diagnostics/ResultBlockInvariantTests.cs`
- CONTAINER
- 46a. The D6 command named `claude/parsons-nwc-analysis-rlzgdr`, which is not on the remote, and left out `fix-F39` and `round-close`, which are. It was built from `git branch -r`, which prints remote-tracking refs the clone remembers rather than what the remote holds. Rebuilt from `git ls-remote --heads origin`, with that trap written beside it and a prune step after
- 46b. `.gitignore` hid every new `.log`, so the run log Bader is asked to copy into `steps/logs` could not reach the repo. A negation for that one folder, with the reason in the file
- 46c. `steps/README.md` listed four files. There are six and the logs folder
- 46d. `RunLog.GroupRecords` was public for one test. It is private, and the test reads the RESULT block off the disk instead
- Size: small, no logic
- DONE on 2026-09-12. The branch list read two ways and both agree on 30 branches. `git status` shows a new log under `steps/logs` as untracked rather than ignored. The README names every file in the folder, checked by a script. The Core tests are unchanged at 905 passed, 37 failed, 33 skipped, 975 total


## F40 Dead members out, second pass

- From the audit of 2026-09-12, `04_audit.md`, Members with no caller
- Files about 40 under `src/Federator.Core`, `ClashRunner.cs`, `JobOutcome.cs`, `GroupRow.cs`, and their tests
- CONTAINER for Core, the three add-in members read twice
- The 43 members referenced by nothing, the 34 uncalled once the type is checked, the 9 public and used only in their own file made private. `ReleaseToPattern` waits on Q24 and the five item properties on Q25. The copies: `NwfComparison.Moves` and `Moved` and its `LeafOf` go and `NwfRebuildPlan` keeps the one rule, `RepoRoot` in four test files reads `Samples.Folder`, `ImageOptions.DefaultStopAfterFailures` reads `RepeatedFailureGuard.DefaultThreshold`, `ScanFindings` reads `BuildingGroup.IsSingleDiscipline`, one `LogoName`
- Every name goes through grep over src and the XAML first and the results go in the PR body, as F36 did
- Size: large in count, small in thought
- DONE on 2026-09-12. 45 members deleted, 64 taken off the public surface and made internal, and every one checked twice: once by a script that classifies every reference under src as code, comment or string, and once by fourteen readers with a verifier each. A member read only by a test that pins a stated rule went internal rather than out, which is Q26. The copies folded: `NwfComparison.Moves`, `Moved` and its `LeafOf`, one `LogoName`, one stop after count, one single discipline rule, and one walk up to the checkout in `Samples.Repo` in place of four. `ReleaseToPattern` stays on Q24 and the five item properties on Q25. Deleting `TestReport.OpenUnder` and `NewPlusActive` left `OpenClashes.Of` with no caller, so the two choice rule has one reader left, which is Q27. Core tests 905 to 885 passed, 37 failed and 33 skipped unchanged, 20 tests removed with the members they proved

## F41 Every handle disposed

- From the audit, Contradictions with the rules, the handles list
- Files `src/Federator.Addin/Engine/SetBuilder.cs`, `SavedTests.cs`, `ClashRunner.cs`
- LOCAL MACHINE ONLY to prove, CONTAINER to edit, read twice
- `SetBuilder`: the `Search`, the `SelectionSet`, the three `ModelItemCollection`, every `SavedItem` in `Describe`, every child not returned in `FindFolder` and `FindSelectionSet`. `SavedTests`: `SelectionA` and `SelectionB` read once each into a using block. `ClashRunner`: the root `FolderItem` in `IndexSets`, the intermediate `GroupItem` in `Resolve`, every `SelectionA`, `SelectionB` and `Selection` read. And `FindSelectionSet` reads the child at the index the count held before `AddCopy` and checks its name, as `ClashRunner` does for tests
- Proof: one building run twice with the XML, the SETS and CLASH blocks read as before and the run is not slower
- Size: medium
- DONE on 2026-09-12, proof pending. See `log.md`. Two reads were left where they are and are Q28

## F42 No framework message in a label

- From the audit, Framework messages in labels, and the window words
- Files `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Core/Clash/RepeatedFailureGuard.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`, `FederationEngine.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml`, `FederatorWindow.xaml.cs`, `src/Federator.Core/Clash/ClashWork.cs`
- CONTAINER for Core and the wording, the window read twice
- `RunLog.DisabledReason` names the two folders tried in plain words and keeps the type and message for the log lines. The guard carries a plain reason for the label and the exception for the log. `FederationEngine.Describe` says how many errors the log holds and not what they say. `Describe(path)` in the window says the log says why. The Clash step heading and the `ClashWork` class comment say that with no XML the tests saved in each NWF run and no set is built. The photo size box and the five status ticks are filled from `ImageOptions` in the window constructor, the way the naming boxes are, and the XAML carries neither
- Every label wording lives in Core where a test can read it, as `ReportPaths.WhereTheyGo` does
- Size: medium
- DONE on 2026-09-12, proof pending. See `log.md`. The cap box and the paste into cells tick went with the photo size and the five ticks, and `FolderMemory.DisabledReason` is reported rather than changed

## F43 Three settings that are constants

- From the audit, Settings that are constants
- Files `src/Federator.Core/Clash/RepeatedFailureGuard.cs`, `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Core/Report/ReportPaths.cs`, `ReportOptions.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`, `FederationEngine.cs`, `FederatorPlugin.cs`
- CONTAINER for Core, the add-in read twice
- The stop after count, the log count and the report subfolder each become a settable property with the same default, read by the code that constructs the guard, starts the log and chooses the folder. The progress interval in `ClashRunner` stays a const, because no rule names it, which the audit's verifier pointed out. No new box in the window, a setting is a property with a default and not a tick box, and the rule is that the number can be changed without a recompile
- Size: small
- DONE on 2026-09-12, proof pending. See `log.md`. Nothing outside the code sets them yet, which is Q29, and `ClashRunner`'s dead two argument constructor went with the fix

## F44 The docs and the comments agree with the code

- From the audit, Doc lines that disagree with the code, and Doubled and copied comments
- Files about 45, most of them one comment line, plus `.claude/rules/core.md`, `README.md`, `docs/workflow.md`, `tools/probes/README.md`, `CLAUDE.md`, and the four probes that do not test their path
- CONTAINER
- The seventeen `docs\scan.md` paths and the four other moved paths. Thirteen to fifteen, five to six, three to four, eight to twelve. core.md made to agree with itself on Distance, the Item ID label, the workbook check, the five extra properties, the Summary sheet, the Layer column, the left alone group, `HealthCheckResult`, and the sample names. The Summary sheet comments in `ClashReportModel`, `HasSheet` renamed `HasRows`. The logo comments. The three stacked summaries, the copied sentences, the two comments that restate a test name, the three project names in comments. The Health tests into a `Health` folder. The probes README made true by giving `probe-units.ps1` and the three window probes the same Test-Path and UNKNOWN line as the other two
- Size: large in count, no logic
- DONE on 2026-09-12. 62 files. The old doc path was 31 lines in 22 files, measured rather than counted off the list, and 30 are repointed. The bundle manifest holds the 31st and is Q30. The nineteen copies of the temp folder sentence go with F16

## F45 The clash step keeps its rules

- From the audit, Rules the clash step breaks
- Files `src/Federator.Core/Exchange/ExchangeReader.cs`, `src/Federator.Core/Clash/ClashTestPlan.cs`, `TestDrift.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`, `ClashHarvest.cs`
- CONTAINER for Core, the add-in read twice, LOCAL MACHINE ONLY to prove the compact count
- A test whose tolerance attribute is missing is skipped by name the way an unknown test type is, never given 0.0. The OLD line logs the status word and nothing else. The compacted count is the Resolved count before less the count read after `TestsCompactAllTests`. A side whose locator reads UNKNOWN is left out of the drift comparison and counted as not compared. `IdFrom` goes in the log once per item, or goes with Q25
- Size: small
- DONE on 2026-09-12, proof pending. See `log.md`. `IdFrom` goes in the log counted per property rather than once per item, because a line per item is the flood the log has already been drowned by. The UNKNOWN marker moved to `TestSettings` so the comparison in Core can read it


## F25 Drop the hidden discipline rule

- CLOSED on 2026-09-12 by F27. The rule was never in the code. Nothing in src sets `GroupRow.Include` to false except the Run column and a blocked group, and it was the same at be0b9b37. The 12 groups the run log of 2026-09-07 showed as skipped were unticked by hand, and the GROUPS block printed skipped for an unticked group
- B15 and Q21 close with that. Clash only where two disciplines meet is F35

## F51 The ACC warning

- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Core/Diagnostics/PublishedProperties.cs` which is new, `docs/workflow.md`, `steps/03_bader_next.md`
- CONTAINER for the Core line and the parse, LOCAL MACHINE ONLY to see the NWD reach ACC
- Every NWD this tool publishes shows a processing error beside it in ACC and in Forma. Autodesk say an NWD published without May be re-saved cannot be translated by that viewer. `WriteNwd` builds a `PublishProperties` and sets Title, Publisher, Subject and Author, and never sets `AllowResave`. `EmbedDatabaseProperties` and `PreventObjectPropertyExport` go with it, because of the second article about an NWD reaching ACC with no properties and every object showing as solid. All three are on the list read off the DLL on 2026-08-29, section 4b, each with a getter and a setter, so none is assumed and none has to be left out
- One log line names every publish property this run set, so a future NWD that will not open in ACC can be read off the log
- Size: three lines of real change, one new Core type with its tests, and the proof is Bader's
- DONE on 2026-09-18. `AllowResave` true, `EmbedDatabaseProperties` true, `PreventObjectPropertyExport` false. `PublishedProperties` in Core carries the line and has nine tests. Core tests before: 957 passed, 0 failed, 32 skipped, 989 total. After: 966 passed, 0 failed, 32 skipped, 998 total. The add-in parses with the same six error codes as before the edit and not one `CS1xxx`. Q32 raised

## F50 The NWF strategy, new against existing

- Files `tools/probes/probe-model-remove.ps1` and `tools/probes/probe-viewpoints.ps1` which are new, `src/Federator.Core/Rerun/RebuildTally.cs` and `src/Federator.Core/Clash/StatusesAPersonSet.cs` which are new, `src/Federator.Addin/Engine/SavedStatuses.cs` and `src/Federator.Addin/Engine/SavedViewpoints.cs` which are new, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Core/Rerun/NwfRebuildPlan.cs`, `.claude/rules/addin.md`, `docs/workflow.md`
- CONTAINER for the counting rules in Core, LOCAL MACHINE ONLY for the measurement and the proof
- The NWF is the record. It carries the file list, the sets, the tests, the clash results, the statuses a person set by hand and, after F52, the viewpoints. Today a CHANGED group clears the document and appends again, then restores the sets and the tests and fails loudly if either does not come back. Every new thing the NWF carries makes that dance longer
- FIRST the measurement: can the installed DLL take one model out of an open document without a clear. `scan.md` records `Document.Models` and exactly two members of `DocumentModels`, `Count` and `SetModelUnitsAndTransform`, and nothing named Remove, Delete or Detach against a model anywhere in the file. So it is UNKNOWN and the probe answers it
- If it CAN: the rebuild appends what the scan has and the NWF does not, takes out what the NWF has and the scan does not, and never clears. Nothing then needs restoring and nothing can be lost. The log says added, removed and kept, with counts
- If it CANNOT: the clear and restore stays and widens to cover viewpoints and statuses as well as sets and tests, counted before and after the same way the sets are today, with the group FAILED and the NWF left alone if any of the four does not come back
- Either way the three labels stay as they are and `RunPath` keeps deciding what the person is told
- Size: medium either way. The widening is the branch taken while the answer is UNKNOWN
- DONE on 2026-09-18. The probe is written and cannot be run here, so the answer stays UNKNOWN and `scan.md` section 5a records the question rather than an answer. The widening is done and tested: four things counted, one keep rule in `Federator.Core.Rerun.RebuildTally` where there were two copies of it, and `Federator.Core.Clash.StatusesAPersonSet` deciding which statuses are worth keeping. `NwfRebuildPlan.SetsLine`, `SavedTestsLine`, `SetsKept`, `SavedTestsKept` and `SetsNeedRestoring` went with their eight tests, because nothing in src called them once the tally replaced them. Core tests before: 966 passed, 0 failed, 32 skipped, 998 total. After: 978 passed, 0 failed, 32 skipped, 1010 total. Q34 raised

## F52 A viewpoint per discipline, grouped by discipline

- Files Core under `src/Federator.Core/Views` which is new, `src/Federator.Addin/Engine/`, `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Core/Rerun/GroupJudgement.cs`
- CONTAINER for the plan, the outcome and the VIEWS block, LOCAL MACHINE ONLY for the measurement and the proof
- `DocumentSavedViewpoints` appears nowhere in this repo. `scan.md` names `Viewpoint`, `DocumentCurrentViewpoint`, `View.CreateViewpointCopy` and `ClashResult.HasSavedViewpoint` and says nothing here writes a viewpoint into the NWF. So how a folder is made, how a viewpoint is added into it, whether a name can be set and whether anything has to be disposed are all UNKNOWN and nothing about the collection may be assumed. `tools/probes/probe-viewpoints.ps1` went in with F50 rather than here, because F50's own viewpoint count rests on the same unmeasured collection and Bader runs both probes in one sitting
- After the clash run and before the NWF is saved again, one folder per discipline in the group, named with the discipline code the scan reads off part 5 of the NWC name, and one saved viewpoint inside it showing that discipline and hiding the others
- A group with one discipline still gets its folder and its viewpoint
- A viewpoint already at that path is left exactly as it is and counted as already there, the rule F28 set for sets. Never a second copy at one path
- The log gets a VIEWS block in the shape the SETS block uses: one line per viewpoint, then created, already there and failed. The counts go in the RESULT block and the judgement, so a group whose viewpoints failed is not reported as DONE
- Size: large. The Core half is all of the rule and the add-in half is the part that waits on the measurement
- DONE on 2026-09-18. The Core half is finished and tested: `ViewpointPlan` with its settings, `ViewpointBuildOutcome` as the twin of `SetBuildOutcome`, and the judgement rule that a group whose viewpoints failed is not DONE. The add-in half is written and wired in, and `SavedViewpoints.CanBuild` is FALSE, so the run plans the viewpoints, logs what it would have made and attempts nothing until `probe-viewpoints.ps1` has been run. A step this tool cannot do is not a step that failed, so no group is marked down for it. Core tests before: 978 passed, 0 failed, 32 skipped, 1010 total. After: 1004 passed, 0 failed, 32 skipped, 1036 total

## F53 The 150 mm rule and the sub groups

- Files Core under `src/Federator.Core/Views`, `src/Federator.Addin/Engine/ClashHarvest.cs`, `.claude/rules/core.md`
- CONTAINER for all of it, LOCAL MACHINE ONLY to see it pick the right pipes
- Pipes, ducts, cable trays and their fittings over 150 mm are in the viewpoints and smaller ones are out. The large ones sit as sub groups under Mechanical and under Electrical
- 150 is a setting with 150 as its default, in millimetres, named once in Core. The property names are a setting too, starting from Diameter, Width, Height, Size, Nominal Diameter and Overall Size, because which one carries the size differs per kind and per exporter
- The number the property returns is in the document's units, so it converts through `UnitTable` and never compares a raw double against 150
- A fitting usually carries no size property at all. Anything whose size cannot be read is INCLUDED, and every one is named in the log under a line saying how many were included because their size could not be read. Nothing disappears quietly. This is the rule most likely to be wrong on the first run, so it is loud
- Size: medium, and all of the rule is provable here
- DONE on 2026-09-18. `SizeSettings`, `SizeRule` and `SizeTally` in `Federator.Core.Views`, with 26 tests. The threshold, the six property names, which disciplines get a sub group and whether every unmeasurable item is named are all settings. The sub group folder is named from the threshold, so the folder and the rule cannot drift apart. `ItemSizes` in the add-in reads the properties by kind and decides nothing. Core tests before: 1004 passed, 0 failed, 32 skipped, 1036 total. After: 1035 passed, 0 failed, 32 skipped, 1067 total. The SIZE block appears once F52's API is measured, because the sub group is a viewpoint

## F54 Clashes that cannot be solved become Reviewed

- Files `src/Federator.Addin/Engine/ClashRunner.cs`, `docs/workflow.md`, `.claude/rules/addin.md`
- CONTAINER to read, LOCAL MACHINE ONLY to prove
- The member is already measured. `DocumentClashTests.TestsEditResultStatus(IClashResult result, ClashResultStatus status)` at `scan.md` line 137, and `ClashResultStatus : New = 0, Active = 1, Reviewed = 2, Approved = 3, Resolved = 4` at line 216
- A clash carries New, Active, Reviewed, Approved or Resolved. A TEST carries New, Old, Partial or Complete. Old is a test word and never a clash word, so nothing in this tool ever moves a clash from Old
- How the tool learns which clashes cannot be solved is Q33. Until it is answered, only the part that needs no answer is built: one method that takes a list of clash names with the status wanted, applies it through `TestsEditResultStatus`, and logs every one it changed and every one it could not find
- Changing a status writes into the document, so the NWF is saved again after it, and the workbook and the page show the new status and not the one read before the change
- Never Resolved and never Approved. Reviewed is the only status this tool sets, because the other two are a person's decision about work that was actually done
- Size: small while Q33 is open
- DONE on 2026-09-18. `StatusesThisToolMaySet` and `StatusWords` in Core with 11 tests, and `ClashStatusEditor` in the add-in. Reviewed and nothing else, and a refused status is logged by name. The editor runs BETWEEN the run and the harvest with its own resolve, because the harvest reads a result's status and because `TestsEditResultStatus` is a mutator that kills the handle handed to it. A status written asks for the NWF again. Nothing supplies a list while Q33 is open, so no run changes a status today. Core tests before: 1035 passed, 0 failed, 32 skipped, 1067 total. After: 1045 passed, 0 failed, 32 skipped, 1077 total

## F55 The rules and the docs catch up

- Files `.claude/rules/*.md`, `docs/workflow.md`, `steps/03_bader_next.md`
- CONTAINER
- Every rule from F50 to F54 into the rules files and the docs. One numbered proof per feature, one action per step, each with its Look for line, in the shape the file already uses
- Size: medium, and it is the one that keeps the next audit short
- DONE on 2026-09-18. Four gaps the per feature work left, found by auditing what each feature actually landed: F51's rule was in no rules file, `CLAUDE.md` did not name this round's two unknowns, six step references in the log were wrong and four of those were written without measuring, and two Core types were named in no rule. Then the end to end read: 36 backtick strings checked in the round's own steps, three wrong, and all three were F52's steps broken by F53 adding the sub groups after they were written. Core tests unchanged at 1045 passed, 0 failed, 32 skipped, 1077 total, because it touches no code

## F56 What the real read of 03_bader_next.md found

- Files `steps/03_bader_next.md`, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Engine/SavedViewpoints.cs`, `.claude/rules/addin.md`, `tests/Federator.Core.Tests/Rerun/RebuildTallyTests.cs`
- CONTAINER
- The feature round reported its end to end read as done off a mechanical check of quoted strings in its own 42 steps. The real read covers all 251 and asks what each step PROMISES. 120 Look for lines read, nine wrong, four of them the round's own drift
- Size: four steps, one log label and its tests
- DONE on 2026-09-18. Step 51's block order, which F52 broke. Step 199's claim about `AddCopy`, which names a member nothing calls. Step 210's ORDER, which F55 left wrong when it fixed the count, because the disciplines come sorted Ordinal. And step 229, where F50's rebuild row and F54's clash lines both wrote `STATUS`, now separated so the rebuild row is `RESULTS` and one prefix means one thing. Core tests before and after: 1045 passed, 0 failed, 32 skipped, 1077 total

## F57 Five Look for lines older than the feature round

- Files `steps/03_bader_next.md`
- CONTAINER
- Found by the same read that found F56's four, each confirmed by a verifier told to refute it. They are older than the feature round and NOT this round's drift, so they are listed rather than corrected, because one of them is wording an earlier round wrote on purpose and reversing that on a quick verification is the fault the rules exist to prevent
- Step 73 says a SECOND `NWF      attempt` line follows the CLASH block. The group in that proof is a Weekly run plus XML, which takes the OPENED branch, and that branch logs `NWF      reused` and never an attempt before the clash step. So there is one attempt line, not a second. The 2026-09-12 round wrote this wording deliberately and may have meant a different path
- Steps 148 and 166 both say the CLASH block says the workbook was written. It does not. The block is `ClashRunOutcome.Lines()` and names no output file. The workbook has its own `XLSX     attempt` and `XLSX     written` lines after it
- Steps 152 and 157 say a line sits ABOVE the Run the open file button. `OpenDocumentLine` and `RunOpenButton` are the two children of one horizontal StackPanel with the button first, so the line is beside it and to its right. What the line SAYS is right in both steps
- Size: five wordings, once Bader says which readings he meant

## F58 The add-in compiles again

- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `tools/checks/check-locals.sh` and `tools/checks/broken/` which are new, `.githooks/pre-commit`, `.github/workflows/tests.yml`, `.claude/rules/addin.md`, `CLAUDE.md`
- CONTAINER for the fix and for both proofs, LOCAL MACHINE ONLY for the build itself
- `FederationEngine.BuildViewpoints` declares `views` twice in one scope, `ViewpointSettings views` at line 1793 and `ViewpointBuildOutcome views` at line 1812, and the second one reads `views.Sizes` while it is being declared. That is CS0128 and the add-in has not built since F52 merged on 2026-09-18. Step 8 of `03_bader_next.md` is the build and all 251 steps wait behind it
- Measured here against the real `Federator.Core.dll` and the net48 reference assemblies, which answer `error CS0128: A local variable or function named 'views' is already defined in this scope`
- WHY NOTHING SAW IT. The parse check this repo has been running passes `-nostdlib` with no references, so Roslyn stops before it binds one method body. It reads syntax and nothing else. Adding the reference assemblies makes it bind every body whose signature resolves, and it still cannot see this one, because `BuildViewpoints` takes a Navisworks `Document` and Roslyn skips the body of any method whose signature it cannot bind
- So the fix carries a check as well as the line. `tools/checks/check-locals.sh` refuses a local declared twice in one method scope, the pre-commit runs it before the tests, and Actions runs it twice, once over `src` and once over `tools/checks/broken`, which is wrong on purpose so the check is proved to refuse and not only to pass
- Size: one line of real change, one check, one fixture and the rule
- DONE on 2026-09-19. The second local is `built`. `tools/checks/check-locals.sh` refuses a local declared twice in one method scope, the pre-commit runs it before the tests and Actions runs it over `src` and over `tools/checks/broken`, which it has to refuse. `.claude/rules/addin.md` now says what the container can and cannot check, so parses and builds cannot be swapped again. Core tests before and after: 1045 passed, 0 failed, 32 skipped, 1077 total

## F59 Every step is named and timed

- Files `src/Federator.Core/Diagnostics/RunStep.cs` and `RunSteps.cs` which are new, `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`
- CONTAINER for the type, the words and both line shapes, LOCAL MACHINE ONLY to see a real run wear them
- One Core type owns the step list and the words: DECIDE, APPEND, NWF SAVE, UNITS, SETS, TESTS CREATE, TESTS RUN, HARVEST, IMAGES, WORKBOOK, HTML, XML, NWD, CONFIRM. Nothing anywhere types a step name as a string
- `RunLog` gains a step that is opened and closed, and closes itself when the work inside it throws, so a step can never be left open by a failure. One line when it starts and one when it finishes, with the seconds and a short phrase for what it changed
- The clock is monotonic, a `Stopwatch` and never two wall clock readings subtracted, because a run crossing a clock change would otherwise report a step that took less than no time
- Size: medium, and all of the shape is provable here
- DONE on 2026-09-19. `RunSteps` holds the fourteen names and nothing types one as a string. `RunStep` opens in a using block, closes itself on a throw, says THREW and keeps its seconds, and a step nobody closed is named NEVER CLOSED when its group ends. The clock is the run's `Stopwatch` and never two wall clock readings subtracted. A step entered again is counted rather than written out, which is what keeps 1830 tests from writing 3660 lines. `ClashRunner`'s own Stopwatch went, so one piece of work is timed by one clock. Core tests before: 1045 passed, 0 failed, 32 skipped, 1077 total. After: 1075 passed, 0 failed, 32 skipped, 1107 total

## F60 The timing blocks, and F21 closes here

- Closes F21, M1, M2 and M3 as far as a log can
- Files `src/Federator.Core/Diagnostics/TimingBlock.cs` which is new, `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`
- CONTAINER for the block, LOCAL MACHINE ONLY for the numbers in it
- A TIMING block per group: every step, its seconds, its share of the group, slowest first, then the group total
- A TIMING block for the run: every group with its total, slowest first, then the run total, then the same table by STEP NAME added across every group, so one reading answers which step costs the run and not only which building
- The run block says in words whether the run fitted in forty five minutes, which is criterion 2 of done
- Every number measured. Nothing estimated and nothing rounded up into a claim
- Size: medium

## F61 The document census

- Files `src/Federator.Core/Diagnostics/DocumentCensus.cs` and `CensusRule.cs` which are new, `src/Federator.Addin/Engine/FederationEngine.cs`
- CONTAINER for the rule and the wording, LOCAL MACHINE ONLY for the counts and the cost
- Five counts: models, selection sets, clash tests, clash results and saved viewpoints. ONE place in the add-in reads all five and disposes every wrapper, the way `FederationEngine.CountSets` already does
- A CENSUS line before and after every step that can change the document
- A Core rule says which steps may move which count. A count that moves when the rule says it may not gets a line beginning `CENSUS CHANGED` naming the step, the count, the before and the after, and that group is not DONE
- What the census COSTS is measured and logged once per group. Over a second a group it drops to counting only before and after the steps that write, and says in the log that it did
- Size: large, because the rule is the whole of it

## F62 The live line in the window

- Files `src/Federator.Core/Diagnostics/RunProgress.cs` which is new, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`, `src/Federator.Addin/Engine/SetBuilder.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml`
- CONTAINER for the line and its rules, LOCAL MACHINE ONLY to watch it move
- While the run works the line says group N of M, the building, the step, the seconds on that step and the seconds on the run
- It updates as the step changes and at least once a second inside a step that has a loop to tick from
- The engine already hands progress out through ONE callback, so what that callback carries widens and no second route is added, and it stays on the thread the run is on
- A step running longer than twice what the same step took on the group before says so on the line
- The log pane keeps following the log and the live line above it never scrolls away
- What this cannot do is tick inside a single Navisworks call with no loop in it. The line says when it last changed rather than pretending
- Size: medium

## F63 The report gap block

- Files `src/Federator.Core/Diagnostics/ReportGap.cs` and `GapRule.cs` which are new, `src/Federator.Addin/Engine/FederationEngine.cs`, `steps/02_questions.md`
- CONTAINER for the rule and the block, LOCAL MACHINE ONLY for what a real run holds
- Bader's standing rule, built into the tool: when the code knows something the report does not show, it becomes a question in the next round
- At the end of each group, what the run holds is compared against what the report carries. Every number measured and not printed gets one line in a GAP block naming the number, its value and where it would belong
- The rule for what counts as a gap lives in Core with its tests, seeded from `04_audit.md` and the five per item properties of Q25
- The block is written even when it is empty, saying nothing was held back. One line at the end of the run says how many gaps in total
- Every gap this round finds goes into `02_questions.md` as a numbered question. The file runs 1 to 34, so they start at Q35
- Size: medium

## F64 The machine readable log

- Files `src/Federator.Core/Diagnostics/EventRow.cs` and `RowLog.cs` which are new, `src/Federator.Core/Diagnostics/RunLog.cs`
- CONTAINER for all of it
- A second file beside the text log, same name and a different extension, one row per event, tab separated, with a header row: time, seconds since start, group, step, event, name, number, text
- Every line the text log writes that carries a number writes a row here too, through ONE writer, so the two cannot drift
- The purpose is stated in a comment at the top of that writer
- The text log stays the one a person reads and nothing about it gets worse
- A tab or a newline inside a value is escaped, because one stray tab moves every column after it
- Size: medium

## F21 The log answers timing and counts

- ABSORBED on 2026-09-19. The log round asks for the same thing in more detail, so F21 closes with F60, the timing blocks, and the per test clash count goes in with it. The section stays because the reasons under it are the reasons F60 is built the way it is
- Closes M1, M2 and M3 as far as a log can
- Files `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`
- CONTAINER for the block shape in Core, LOCAL MACHINE ONLY to prove
- The run log carries a timing block per group and a total for the run
- Per test, the clash count written to Excel goes in the log, so the Excel and the log can be checked against the panel
- Size: medium. Q3 and Q7 answered the log must carry it

## F19 CI for the add-in

- DROPPED. Add-in build stays local, Q10
- M5 stays open on the add-in side

## The three proofs

Not fixes. Runs on the local machine that no code change replaces. Bader runs them, Q15.

- P1 one building, Run, send the log. Proves criterion 1 and shows OPENED on the second press. Closes M1
- P2 all buildings ticked, Run, send the log. The timing block per group answers criterion 2. Closes M2
- P3 open one NWF, read the panel count for three tests, compare to the Excel. Closes M3
