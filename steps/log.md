# log

Newest entry at the top.

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
