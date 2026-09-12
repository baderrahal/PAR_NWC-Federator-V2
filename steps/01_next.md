# 01 next

Fixes in the order Bader set on 2026-09-06. The F numbers are kept from the first list.
One PR per fix. F1 to F4 go in one PR. The worker merges its own PR.

`CONTAINER` means the fix can be built and tested here.
`LOCAL MACHINE ONLY` means it needs Navisworks to prove.
The add-in does not compile in the container at all, so every add-in edit
can be written here and only proved on the local machine.

## Order

Changed on 2026-09-12 from the audit in chat. F27 to F38 are new and go before F16, F21 and F15. F12, F13 and F14 are absorbed by F37 and F38 and close with them. F25 closes with F27, the rule was never in the code. F5 to F26 are done.

1. F5
2. F6
3. F7
4. F8
5. F22
6. F1, F2 and F4 as one PR, done. F3 was done inside F22
7. F9
8. F10
9. F11
10. F17
11. F20
12. F24
13. F26
14. F27, closes F25
15. F28
16. F29
17. F30
18. F31
19. F32
20. F33
21. F34
22. F35
23. F36
24. F37, closes F12 and F14
25. F38, closes F13
26. F16
27. F21
28. F15
29. F18 when the sample arrives
30. F23 when Q20 is answered

F19 is dropped.

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
- Files UNKNOWN until Q20 is answered
- LOCAL MACHINE ONLY to see, CONTAINER to read
- Before the fix, read every code path that touches a file path and list in the PR every place the add-in refuses or fails on a path, so the ACC case can be matched when the details arrive
- Waits on Q20

## F1 Fix the test name typo

- Closes B8
- Files `tests/Federator.Core.Tests/ReportCheckTests.cs`
- DONE on 2026-09-07 in one PR with F2 and F4

## F2 Fix the hardcoded probe path

- Closes B7
- Files `build/probe-window-defaults.ps1`, `docs/test-model-side.md`
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

- Closes B5 and B6
- Files `src/Federator.Addin/Engine/SetBuilder.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`
- LOCAL MACHINE ONLY to prove, and the proof is a run with no ObjectDisposedException
- Size: medium and the riskiest here. Disposing a wrapper the code still uses throws at run time. Last of the code fixes, one file at a time, one run each. Q14 answered fix now
- Read `steps/05_api_notes.md` first if it exists

## F12 Fix the docs that contradict the code

- Absorbed by F37 on 2026-09-12. The two docs move to `docs/history` with a first line saying they are measurement history, and `docs/workflow.md` becomes the one current description
- Closes L5, L6, L7
- Files `CLAUDE.md`, `docs/scan.md`, `docs/test-model-side.md`
- CONTAINER
- Size: medium, reading and cutting. No code

## F13 Split CLAUDE.md into rules

- Absorbed by F38 on 2026-09-12
- Closes M4, part of L5
- Files `CLAUDE.md`, new `.claude/rules/*.md`, `docs/`
- CONTAINER
- Size: large for reading, small for typing. Q5 answered yes

## F14 Add a root README

- Absorbed by F37 on 2026-09-12. README.md exists since F20 and F37 points it at `docs/workflow.md`
- Closes M6
- Files `README.md`
- CONTAINER
- Size: one page

## F16 Make the tests path neutral

- Closes M8
- Files about 15 test files
- CONTAINER, then confirm on Windows
- Size: medium. Q11 answered yes

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

## F27 The GROUPS block tells the truth

- Closes F25, B15 and Q21
- Files `src/Federator.Core/Diagnostics/RunLog.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `README.md`, `steps/00_analysis.md`
- CONTAINER for the line, LOCAL MACHINE ONLY to see the block
- DONE on 2026-09-12. The block says unticked instead of skipped and ends with one line counting the unticked groups

## F28 A set already there is not counted as created

- Files `src/Federator.Core/Sets/SetBuildOutcome.cs`, `src/Federator.Addin/Engine/SetBuilder.cs`, `tests/Federator.Core.Tests/SetBuildOutcomeTests.cs`
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

## F32 The open file is guarded

- Files `src/Federator.Core/Rerun/OpenDocumentJob.cs`, its tests, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- CONTAINER for the rule, LOCAL MACHINE ONLY to see the refusal
- D2. A path whose extension is not .nwf, a path whose folder cannot be read, and an NWD path equal to the open path are refused with the reason named. Narrows B12, the ACC case still waits on Q20

## F33 One unit table

- Files new `src/Federator.Core/Units/UnitTable.cs`, `ExchangeUnits.cs`, `ExchangeReader.cs`, `ExchangeModel.cs`, `ClashTestPlan.cs`, `DocumentUnits.cs`, `ClashRunner.cs`, `FederationEngine.cs`, `FederatorWindow.xaml.cs`
- CONTAINER for the table, LOCAL MACHINE ONLY for the window and the engine
- One table, every other unit list deleted. `WantedUnits` fails the group on a name the table does not know. `ClashTestPlan.Convert` is the one place a file unit is judged

## F34 Window wiring

- Files `src/Federator.Addin/Ui/FederatorWindow.xaml`, `FederatorWindow.xaml.cs`, `src/Federator.Addin/Engine/FederationEngine.cs`, `FederationJob.cs`, `src/Federator.Core/Report/ReportOptions.cs`, `ClashReportModel.cs`, `src/Federator.Core/Diagnostics/GroupOutcome.cs`
- LOCAL MACHINE ONLY for the window, CONTAINER for the option defaults
- D1 HealthCheck wired under the file line, D3 the outstanding count setting deleted, D4 the two hand buttons share the engine wording. The NWD folder box refreshes the summary, the XML box has a handler named for it, the republish field and the three unused engine constructors go

## F35 Clash only where two disciplines meet

- Files `src/Federator.Core/Grouping/BuildingGroup.cs`, `src/Federator.Core/Clash/ClashTestPlan.cs`, `src/Federator.Addin/Engine/FederationJob.cs`, `FederationEngine.cs`, `ClashRunner.cs`, `src/Federator.Addin/Ui/GroupRow.cs`
- CONTAINER for the count, LOCAL MACHINE ONLY to see the status
- D5. A group with fewer than two disciplines creates every test and runs none, the same as a one model group today. `ClashSkipReason.SingleModel` becomes `SingleDiscipline`

## F36 Dead code and copies out

- Files across `src` and `tests`, `build/probe-window-defaults.ps1`
- CONTAINER
- Every name grepped before it goes, one `Or` helper in `Words.cs`, one client column list, one `InstallFiles` locator, every doubled summary block fixed

## F37 One type per file and one place per kind of file

- Moves only, no logic. Closes F12 and F14
- CONTAINER
- One type per file in nine files, the tests into src folder names, the probes into `tools/probes` with a README and a path parameter, the two docs into `docs/history`, `docs/workflow.md` written

## F38 CLAUDE.md under 200 lines, rules in .claude, walls in hooks

- Closes F13
- CONTAINER
- History to `docs/history/claude-md-history.md`, four rules files under `.claude/rules` with paths frontmatter, two PreToolUse hooks under `.claude/hooks` in `.claude/settings.json`, the pre-commit hook kept
- DONE on 2026-09-07, proof pending. See `log.md`. The numbers are converted in Core rather than waiting for the document to follow

## F25 Drop the hidden discipline rule

- CLOSED on 2026-09-12 by F27. The rule was never in the code. Nothing in src sets `GroupRow.Include` to false except the Run column and a blocked group, and it was the same at be0b9b37. The 12 groups the run log of 2026-09-07 showed as skipped were unticked by hand, and the GROUPS block printed skipped for an unticked group
- B15 and Q21 close with that. Clash only where two disciplines meet is F35

## F21 The log answers timing and counts

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
