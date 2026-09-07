# log

Newest entry at the top.

## 2026-09-07 F9, a CHANGED group is left alone before anything touches it

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11, L1, F22 and B7 stay pending local proof and nothing about them was recorded
- F9 done. A CHANGED group used to fall through Decide into the units change, the clash step, the workbook, the NWD publish and the survival check before anyone left it alone. Every model had its units set and a UNITS line was logged, an NWD was published off a federation that no longer matched the folder, and the log said `NWF reused`
- The CHANGED check now sits right after Decide and before anything touches the document in memory. Nothing is done: no units change, no sets, no tests, no save, no NWD. The NWF and NWD sizes are read off the disk for the RESULT block and nothing is recorded as written
- One log line, `NwfComparison.SkipLine` in Core, names the group, the files added and removed, and every step that was not done. The CHANGED block from Decide still lists each file above it
- Reading the file list is the one thing that happens before the check, and it opens the NWF to read it. That is a read, not a change to the NWF on disk
- `GroupJudgement` judges a CHANGED group PARTIAL right after the error and NWF on disk checks, before the NWD checks, so the NWD it no longer publishes cannot make it FAILED. The GROUP finished line carries `Skipped (changed on disk)` from F22 and the RESULT block counts it under skipped
- CLAUDE.md updated: the NWD is republished in the Build and Open cases, and the CHANGED rule says what the check comes before
- Three Core tests added, two in `GroupJudgementTests` and one in `NwfComparisonTests`. The order of the steps lives in the add-in and cannot be tested here
- Core tests under mono on Linux: 837 passed, 40 failed, 32 skipped, 909 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` has the F9 proof after the F22 proof

### What remains

- F23 once Q20 is answered, then F10 onward in `01_next.md` in order
- The F5, F6, F7, F8, F22, F1 to F4 and F9 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, B9, L3 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F9 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F10, or F23 if Q20 is answered first

## 2026-09-07 F1, F2 and F4, the small fixes

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11, L1 and F22 stay pending local proof and nothing about them was recorded
- F1, B8. The test `ALogoOnTheePageIsCheckedAgainstTheDisk` in `ReportCheckTests.cs` is now `ALogoOnThePageIsCheckedAgainstTheDisk`. Name only
- F2, B7. `build/probe-window-defaults.ps1` read `$repo = "C:\Users\p003653k\source\repos\Parsons NWC Federator"`. It now reads `$repo = Split-Path $PSScriptRoot`, the same line the scroll and labels probes use
- F4, B10. The `DocumentUnits` docstring said the units change is off unless asked for. The window sets it on for every run, so the sentence now says it is on for every run, because the window sets it on and every report the team sends is metric
- The same machine path sat in `docs/test-model-side.md` step 2 as a `cd` line. It now says to change into the folder the repo is checked out in. `docs/scan.md` line 7 still names the user as part of the machine record of the scan, which is a measurement and not a path, so it stays
- F3 was closed inside F22, so F1 to F4 are all done
- Core tests under mono on Linux: 834 passed, 40 failed, 32 skipped, 906 total. Unchanged, none new. The renamed test is one of the 32 that skip without a Navisworks install
- The add-in was not compiled. It cannot compile in the container. The probe runs on Windows only, against the built add-in
- `03_bader_next.md` has one step at the end to run the probe

### What remains

- F23 once Q20 is answered, then F9 onward in `01_next.md` in order
- The F5, F6, F7, F8 and F22 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B8 fixed. B10 fixed
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3 and B4 fixed in code inside F22, pending the same proof
- B12 OPEN, waiting on Q20
- B5, B6, B9, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F1 F2 F4 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md`, runs the probe at the end, and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F9, or F23 if Q20 is answered first

## 2026-09-07 F22, two clear workflows in the window, and F3 with it

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11 and L1 stay pending local proof and nothing about them was recorded
- The F8 entry sat in this file twice. The fuller one is kept and the other is gone
- F22 marked confirmed by Bader on 7 Sep 2026 in `01_next.md` with the two definitions
- F22 done. The rule is `Federator.Core.Rerun.RunPath`, beside Decide. Given the Decide result and whether an XML is picked it returns one of five labels: First run, Weekly run, Weekly run plus XML, Skipped (changed on disk), Unknown. Before the run the label is worked out from whether the NWF is already at its output path, because CHANGED is only known once the NWF is opened
- The group list has a Run as column, filled after Scan and refreshed when the NWF folder, a name, or the XML box changes. Blocked groups show nothing there
- The confirm dialog opens with the count per label and says cleared only for the First run groups. The Skipped line says it is only known once each NWF is opened. Then the existing lines about what is open and Carry on. That closes F3: B4 was the dialog saying cleared before every group, and B3 the NWD line naming a tick box that is gone, which is reworded too
- Run the open file: the blue line leads with Weekly run or Weekly run plus XML, and the help says there is no First run on that path
- The log carries the label on every GROUP finished line and in the GROUPS block before the run, and the RESULT block totals them: first run, weekly run, weekly plus XML, skipped
- CLAUDE.md has a Two workflows section with the two definitions and the labels. `README.md` created at the root with the same section. F14 stays open for the rest of a README
- The engine does the same work as before. Only what the person is told changed, plus one log string
- Fourteen Core tests added. Eleven in `RunPathTests`, one in `OpenDocumentJobTests`, two in `RunLogTests`
- Core tests under mono on Linux: 834 passed, 40 failed, 32 skipped, 906 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7, F8 and F22 proofs in one session

### What remains

- F23 once Q20 is answered, then F1, F2 and F4 in one PR, then the rest of `01_next.md` in order
- The F5, F6, F7, F8 and F22 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3 and B4 fixed in code inside F22, pending the same proof
- B12 OPEN, waiting on Q20
- B5 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F22 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the five proofs in this file
5. Worker starts F1, F2 and F4 as one PR, or F23 if Q20 is answered first

## 2026-09-06 F8, run the saved tests when no XML is picked

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2 and B11 stay pending local proof and nothing about them was recorded
- B12 registered in `00_analysis.md`, OPEN, waiting on Bader. Q20 added to `02_questions.md` with the four unknowns. F23 added to `01_next.md` after F22
- F8 fixed. With no XML, `ClashStep` returned before anything ran, on both the scanned run and the open file run, while the window logged that it was running the tests already in the document
- The clash step now does one of three things and the log names which, in one line shape on both runs: `CLASH    source   tests from XML, ...`, `CLASH    source   tests saved in the document, N of them, no XML picked`, or `CLASH    source   nothing, no XML picked and the document holds no clash test, so nothing ran`. The rule is `ClashWork.SourceFor` in Core
- A second way to build a `ClashTestPlan`, `ClashTestPlan.FromDocument`, in Core. Every saved test goes in by its address, in the order found. No drift compare, nothing created, sets not touched. `SavedClashTest` is the shape it takes, with no Navisworks type on it
- The add-in fills it in `SavedTests.Read`, one walk of `DocumentClashTests` that disposes every wrapper and keeps only the address
- `ClashRunner.Run` and `OneTest` take a document plan through the same path: `ItemsOn` both sides, the SingleModel and EmptySide skips, `TestsRunTest`, the harvest, the images, the report, the `RepeatedFailureGuard`. A document plan skips the set resolution and the zero sets guard, because its sides are already inside the tests
- The XML path is unchanged. An XML holding neither sets nor tests is still nothing, whatever the document holds
- The words fixed to match the log: CLAUDE.md in two places, the help line under Run the open file, the OPEN log line in the window, and the engine docstring
- Eleven Core tests added in `SavedTestPlanTests`
- Core tests under mono on Linux: 820 passed, 40 failed, 32 skipped, 892 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7 and F8 proofs in one session

### What remains

- F22 next, which waits on Bader confirming the two workflow definitions in chat, then F23 once Q20 is answered, then F1 to F4 as one PR
- The F5, F6, F7 and F8 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the scanned run with no XML on a folder whose NWFs already hold tests
- B12 OPEN, waiting on Q20
- B3 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F8 PR
2. Bader answers Q20 and confirms the two workflow definitions for F22 in chat
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the four proofs in this file
5. Worker starts F22

## 2026-09-06 F7, the RESULT block and log copy for the open file run

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1 and B2 stay pending local proof and nothing about them was recorded
- F7 fixed. The open file run ended with no GROUP line, no block naming what it did, no RESULT block and no copy of the log. The scanned run has all four
- `FederationEngine.RunOpenDocument` now calls `log.GroupStarted` with the models inside the file, and `log.GroupFinished` in a finally with the same outcome shape the scanned run uses, so `GroupJudgement` gives DONE, PARTIAL or FAILED by the same rule and the RESULT block counts it. The group name is the open file's name from `OpenDocumentJob`
- In the same finally the engine writes a block titled OPEN FILE, built by `OpenDocumentJob.SummaryLines` in Core: the file, the decision, the clash file, the clash outcome, the NWD path, the report folder from F6, and the outcome. Source folder, grouping and files ticked say not applicable rather than being left blank
- `FederatorWindow.OnRunOpenDocument` calls the existing `WriteTheResultAndCopyTheLog` in its finally, handing it the open file's own folder through `OpenDocumentJob.FolderOf`, so a failure still leaves a RESULT block and the copy sits beside the open file. No second copy of that method
- `SourceMismatchFindings` is skipped on the open file run with one log line saying why. Nothing was scanned, so there is no NWC name to compare the Revit source against
- The window's old OPEN DOCUMENT one line block is gone, the engine's OPEN FILE block replaces it
- The scanned path is untouched
- Five Core tests added. Two in `GroupJudgementTests` for the open file case, three in `OpenDocumentJobTests` for the folder and the summary lines
- Core tests under mono on Linux: 809 passed, 40 failed, 32 skipped, 881 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6 and F7 proofs in one session

### What remains

- F8 onward in `01_next.md`, in that order
- The F5, F6 and F7 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- B3 to B10, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F7 PR
2. Bader follows `03_bader_next.md` and drops the three logs into `steps/logs`
3. Worker reads the logs and records the three proofs in this file
4. Worker starts F8

## 2026-09-06 F6, the open file report folder

### What was done

- `steps/logs` holds only its README. No run log yet, so B1 is still pending local proof and nothing about it was recorded
- F6 fixed. The window built `<folder>\Clash Reports` with `OpenDocumentJob.ReportFolderBeside` and handed it to the engine as the NWF folder. The engine then built `Clash Reports` beside that again through `ReportPaths.Choose`, so the reports went to `<folder>\Clash Reports\Clash Reports`
- The decision now lives in one place, `OpenDocumentJob.ReportFolder(openPath, pickedExcelFolder)` in Core. It calls the same `ReportPaths.Choose` the scanned run uses, with the open file's own folder where the scanned run puts the NWF folder. A picked Excel folder wins, the same as the scanned run
- The engine's open file path calls that rule in `RunOpenDocument` and takes no folder from the window. The window's blue line calls the same rule, so what it says and what is written cannot differ
- `ReportFolderBeside` is gone. `RunOpenDocument` and `OpenJob` lost a subfolder parameter nothing read
- The scan's source folder refusal is skipped on the open file run. No source folder is handed to `ReportPaths.Choose` there, because nothing was scanned. The engine still creates the report folder and reads every written file back, so a folder that cannot be written is caught by the write itself
- The scanned path is untouched. The six argument engine constructor now delegates to a new five argument one and then sets the report folder exactly as before
- Five Core tests added in `OpenDocumentJobTests`, one of which pins the old doubling as the trap it was
- Core tests under mono on Linux: 804 passed, 40 failed, 32 skipped, 876 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5 and F6 proofs together

### What remains

- F7 onward in `01_next.md`, in that order
- The F5 and F6 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B3 to B11, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F6 PR
2. Bader follows `03_bader_next.md` and drops the three logs into `steps/logs`
3. Worker reads the logs and records both proofs in this file
4. Worker starts F7

## 2026-09-06 F5, the sets built test, and Bader's answers

### What was done

- Bader answered every question. The answers sit under each question in `02_questions.md`
- `01_next.md` reordered to Bader's order. F19 dropped. F20, F21 and F22 added
- `steps/logs` created with a README for the run logs
- `03_bader_next.md` written, the local steps for the build, the install and the first proof
- F5 fixed. `FederationEngine.BuildTheSets` returned `CreatedCount > AlreadyPresentCount`. It now returns `SetBuildOutcome.PutAnythingIn`, which is `CreatedCount > 0`
- The bool has one meaning. It feeds `ClashStep`, whose only caller uses it to decide the second NWF save. The tests were never gated on it, `ClashStep` runs them either way. So B1 in `00_analysis.md` was corrected: what was lost was the second NWF save, not the tests
- One log line added after the SETS block saying how many sets were put into the document and how many were already there
- Four Core tests added in `SetBuildOutcomeTests` for `PutAnythingIn`
- Core tests under mono on Linux: 800 passed, 40 failed, 32 skipped, 872 total. The 40 are the same Windows path and file locking failures as before, none in the sets tests
- The add-in was not compiled. It cannot compile in the container

### What remains

- F6 onward in `01_next.md`, in that order
- The three proofs P1, P2, P3 on the local machine

### Known bugs

- B1 fixed in code, pending local proof. The proof: run one building twice, the second press must show OPENED, the sets present and the tests still run, then drop the log in `steps/logs`
- B2 to B11, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F5 PR
2. Bader follows `03_bader_next.md` and drops the two logs into `steps/logs`
3. Worker reads the logs and records the proof in this file
4. Worker starts F6

## 2026-09-06 analysis pass

### What was done

- Read every non sample file in the repo, 151 files, in full
- Sample data looked at by head and line counts only, 355 jpg not opened
- Environment checked. No Navisworks in the container
- Installed .NET 8 SDK and mono in the container to run the Core tests
- Ran the Core tests under mono on Linux. 796 passed, 40 failed, 32 skipped
- All 40 failures are Windows path and file locking assumptions, not code faults
- Wrote this folder
- No source code changed

### What remains

- Every fix in `01_next.md`. Nothing is fixed yet
- Every question in `02_questions.md`. Nothing is answered yet
- The three done criteria. None is proved yet. See section 5 of `00_analysis.md`

### Known bugs

Eleven bugs B1 to B11, eight logic problems L1 to L8, eight missing pieces M1 to M8.
All in `00_analysis.md`.

The ones that change what a run produces:

- B1  a group whose sets were all already there is treated as if nothing was built
- B2  the open file path writes reports one folder too deep
- B11 the open file path writes no RESULT block and copies no log
- L1  the open file with no clash XML runs nothing, the log says it runs the tests
- L2  a CHANGED group still has its model units changed

### What comes next

1. Bader answers `02_questions.md`, at least Q1, Q2 and Q6
2. Worker does the fixes in `01_next.md` in order, one branch per fix or one branch for the small ones
3. Bader runs one building on the local machine and sends the log
4. Worker reads the log for OPENED, the clash timing and the Excel counts
