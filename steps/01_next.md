# 01 next

Fixes in the order Bader set on 2026-09-06. The F numbers are kept from the first list.
One PR per fix. F1 to F4 go in one PR. The worker merges its own PR.

`CONTAINER` means the fix can be built and tested here.
`LOCAL MACHINE ONLY` means it needs Navisworks to prove.
The add-in does not compile in the container at all, so every add-in edit
can be written here and only proved on the local machine.

## Order

1. F5
2. F6
3. F7
4. F8
5. F22
6. F23, once Q20 is answered
7. F1 to F4 as one PR
8. F9
9. F10
10. F11
11. F17
12. F15
13. F12
14. F13
15. F14
16. F16
17. F18
18. F20
19. F21

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

- Closes part of L1 and Q17
- Files `src/Federator.Addin/Ui/FederatorWindow.xaml`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`, `src/Federator.Core/Rerun/`
- CONTAINER for the wording rule in Core, LOCAL MACHINE ONLY to see
- The window must show the difference between the first run and the weekly run
- First run: build the NWF, the sets, the tests, the reports and the NWD from the XML
- Weekly run: open the NWF already there, reload the NWCs, run the saved tests, write the reports and the NWD, XML optional
- Before the confirm box, the person must be told which of the two will happen per group
- Waits on Bader confirming the two workflow definitions in chat

## F23 Fix B12, the NWD opened from ACC

- Closes B12
- Files UNKNOWN until Q20 is answered
- LOCAL MACHINE ONLY to see, CONTAINER to read
- Before the fix, read every code path that touches a file path and list in the PR every place the add-in refuses or fails on a path, so the ACC case can be matched when the details arrive
- Waits on Q20

## F1 Fix the test name typo

- Closes B8
- Files `tests/Federator.Core.Tests/ReportCheckTests.cs`
- CONTAINER
- Size: one word
- Goes in one PR with F2, F3 and F4

## F2 Fix the hardcoded probe path

- Closes B7
- Files `build/probe-window-defaults.ps1`
- LOCAL MACHINE ONLY to run, CONTAINER to edit
- Size: one line, copy the form the other probes use
- Goes in one PR with F1, F3 and F4

## F3 Fix the two wrong messages

- Closes B3 and B4
- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- LOCAL MACHINE ONLY to see, CONTAINER to edit
- Size: two strings. The confirm text says cleared only when it is true, Q13
- Goes in one PR with F1, F2 and F4

## F4 Fix the units docstring

- Closes B10
- Files `src/Federator.Addin/Engine/DocumentUnits.cs`
- CONTAINER to edit
- Size: one sentence
- Goes in one PR with F1, F2 and F3

## F9 Skip the units change on a CHANGED group

- Closes L2
- Files `src/Federator.Addin/Engine/FederationEngine.cs` around line 321
- LOCAL MACHINE ONLY to prove
- Size: one guard on `comparison.Decision`. Q12 answered yes

## F10 Gate the page on its own flag

- Closes L3
- Files `src/Federator.Addin/Engine/FederationEngine.cs` `CreateAndRunTheTests` and `WriteWorkbook`
- LOCAL MACHINE ONLY to prove
- Size: small. Build the `ClashReport` when workbook, XML or HTML is wanted

## F11 Remove the dead code

- Closes B9
- Files `src/Federator.Core/Report/ReportOptions.cs`, `SheetNames.cs`, `ClashMatrix.cs`, `ClashReportModel.cs`, their tests, `docs/scan.md` 4q
- CONTAINER
- Size: medium, a delete across six files and a test run. Q4 answered yes

## F17 Picture numbering by block order

- Closes L4
- Files `src/Federator.Addin/Engine/ClashRunner.cs`, `src/Federator.Core/Report/ImageNaming.cs`
- LOCAL MACHINE ONLY to prove
- Size: medium, and pictures would need renaming after the sort. Q8 answered match the Navisworks export order

## F15 Dispose in SetBuilder and ClashRunner.Resolve

- Closes B5 and B6
- Files `src/Federator.Addin/Engine/SetBuilder.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`
- LOCAL MACHINE ONLY to prove, and the proof is a run with no ObjectDisposedException
- Size: medium and the riskiest here. Disposing a wrapper the code still uses throws at run time. Last of the code fixes, one file at a time, one run each. Q14 answered fix now

## F12 Fix the docs that contradict the code

- Closes L5, L6, L7
- Files `CLAUDE.md`, `docs/scan.md`, `docs/test-model-side.md`
- CONTAINER
- Size: medium, reading and cutting. No code

## F13 Split CLAUDE.md into rules

- Closes M4, part of L5
- Files `CLAUDE.md`, new `.claude/rules/*.md`, `docs/`
- CONTAINER
- Size: large for reading, small for typing. Q5 answered yes

## F14 Add a root README

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
