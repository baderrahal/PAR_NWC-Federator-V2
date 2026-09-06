# 01 next

Fixes ranked. Smallest and safest first.
Nothing here is done. Each one waits for Bader to say go.

`CONTAINER` means the fix can be built and tested here.
`LOCAL MACHINE ONLY` means it needs Navisworks to prove.
The add-in does not compile in the container at all, so every add-in edit
can be written here and only proved on the local machine.

## F1 Fix the test name typo

- Closes B8
- Files `tests/Federator.Core.Tests/ReportCheckTests.cs`
- CONTAINER
- Size: one word

## F2 Fix the hardcoded probe path

- Closes B7
- Files `build/probe-window-defaults.ps1`
- LOCAL MACHINE ONLY to run, CONTAINER to edit
- Size: one line, copy the form the other probes use

## F3 Fix the two wrong messages

- Closes B3 and B4
- Files `src/Federator.Addin/Engine/FederationEngine.cs`, `src/Federator.Addin/Ui/FederatorWindow.xaml.cs`
- LOCAL MACHINE ONLY to see, CONTAINER to edit
- Size: two strings. The confirm text should say cleared only when a group has no NWF yet

## F4 Fix the units docstring

- Closes B10
- Files `src/Federator.Addin/Engine/DocumentUnits.cs`
- CONTAINER to edit
- Size: one sentence

## F5 Fix the sets built test

- Closes B1
- Files `src/Federator.Addin/Engine/FederationEngine.cs` line 653
- LOCAL MACHINE ONLY to prove
- Size: one expression, `CreatedCount > 0`. Check what the caller does with false first

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
- Size: medium. `ClashStep` needs a branch with no exchange that runs every test already in the document. `ClashRunner.Run` takes a plan built from the XML today, so it needs a plan built from the document. Waits on Q2

## F9 Skip the units change on a CHANGED group

- Closes L2
- Files `src/Federator.Addin/Engine/FederationEngine.cs` around line 321
- LOCAL MACHINE ONLY to prove
- Size: one guard on `comparison.Decision`

## F10 Gate the page on its own flag

- Closes L3
- Files `src/Federator.Addin/Engine/FederationEngine.cs` `CreateAndRunTheTests` and `WriteWorkbook`
- LOCAL MACHINE ONLY to prove
- Size: small. Build the `ClashReport` when workbook, XML or HTML is wanted

## F11 Remove the dead code

- Closes B9
- Files `src/Federator.Core/Report/ReportOptions.cs`, `SheetNames.cs`, `ClashMatrix.cs`, `ClashReportModel.cs`, their tests, `docs/scan.md` 4q
- CONTAINER
- Size: medium, a delete across six files and a test run. Waits on Q4

## F12 Fix the docs that contradict the code

- Closes L5, L6, L7
- Files `CLAUDE.md`, `docs/scan.md`, `docs/test-model-side.md`
- CONTAINER
- Size: medium, reading and cutting. No code

## F13 Split CLAUDE.md into rules

- Closes M4, part of L5
- Files `CLAUDE.md`, new `.claude/rules/*.md`, `docs/`
- CONTAINER
- Size: large for reading, small for typing. Waits on Q5

## F14 Add a root README

- Closes M6
- Files `README.md`
- CONTAINER
- Size: one page

## F15 Dispose in SetBuilder and ClashRunner.Resolve

- Closes B5 and B6
- Files `src/Federator.Addin/Engine/SetBuilder.cs`, `src/Federator.Addin/Engine/ClashRunner.cs`
- LOCAL MACHINE ONLY to prove, and the proof is a run with no ObjectDisposedException
- Size: medium and the riskiest here. Disposing a wrapper the code still uses throws at run time. Do it last, one file at a time, one run each

## F16 Make the tests path neutral

- Closes M8
- Files about 15 test files
- CONTAINER, then confirm on Windows
- Size: medium. Waits on Q11. Only worth it if the container is to run the tests before every push

## F17 Picture numbering by block order

- Closes L4
- Files `src/Federator.Addin/Engine/ClashRunner.cs`, `src/Federator.Core/Report/ImageNaming.cs`
- LOCAL MACHINE ONLY to prove
- Size: medium, and pictures would need renaming after the sort. Waits on Q8

## F18 Add the 1A04WE sample

- Closes M7
- Files `samples/client-report/`
- Bader has the file. CONTAINER once it is in
- Size: a commit

## F19 CI for the add-in

- Closes M5
- Files `.github/workflows/`
- Needs a self hosted runner with Navisworks. Waits on Q10
- Size: UNKNOWN until Q10 is answered

## The three proofs

Not fixes. Runs on the local machine that no code change replaces.

- P1 one building, Run, send the log. Proves criterion 1 and shows OPENED on the second press. Closes M1
- P2 six buildings ticked, Run, send the log. The timing block per group answers criterion 2. Closes M2
- P3 open one NWF, read the panel count for three tests, compare to the Excel. Closes M3
