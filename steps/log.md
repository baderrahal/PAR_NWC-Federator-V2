# log

Newest entry at the top.

## 2026-09-06 F8, run the saved tests when no XML is picked

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2 and B11 stay pending local proof and nothing about them was recorded
- B12 registered in `00_analysis.md`, OPEN: Bader opened an NWD from ACC and the tool showed an error beside the file. Q20 added with the four unknowns, answer blank. F23 added to `01_next.md` after F22
- F8 fixed. With no XML, `ClashStep` returned before anything ran, on both paths, while the open file line said the tests already in the document were running
- The clash step now does one of three things and the log names which in one line, `CLASH    source   ...`, the same on the scanned run and the open file run: tests from XML, tests saved in the document, or nothing. The rule is `ClashWork.SourceFor` in Core
- A second way to build a `ClashTestPlan`: `ClashTestPlan.FromDocument` in Core, from a list of `SavedClashTest`, every saved test by its address, no drift compare, no sets touched. The add-in fills the list in `SavedTests.Read` from `DocumentClashTests`, walked once, wrappers disposed
- `ClashRunner` runs a document plan through the same path as an XML plan. A test with an address is resolved where it sits and neither created nor compared. `SingleModel`, `EmptySide` and the `RepeatedFailureGuard` apply the same way. Harvest, images, workbook, page and the second NWF save follow exactly as the XML case
- With an XML picked nothing changed. The saved tests are not even counted then
- Words fixed to match the log line: CLAUDE.md in two places, the help under Run the open file in the window, the OPEN line and comment in `FederatorWindow.xaml.cs`, and the engine docstring
- Eleven Core tests added in `SavedTestPlanTests`
- Core tests under mono on Linux: 820 passed, 40 failed, 32 skipped, 892 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7 and F8 proofs in one session

### What remains

- F22 next, then F23 once Q20 is answered, then F1 to F4 in one PR, then the rest of `01_next.md` in order
- The F5, F6, F7 and F8 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- B12 OPEN, waiting on Q20
- B3 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F8 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the four proofs in this file
5. Worker starts F22 once Bader confirms the two workflow definitions in chat

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
