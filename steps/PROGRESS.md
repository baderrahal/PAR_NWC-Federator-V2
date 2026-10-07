STATE OPEN, 2026-10-06 17:26

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 6 | 25 | 0 | 0 | 1 | 0 | 32 |
| 2b | 9 | 18 | 0 | 0 | 3 | 0 | 30 |
| 2c | 2 | 0 | 0 | 0 | 7 | 0 | 9 |
| 3a | 8 | 4 | 0 | 0 | 8 | 0 | 20 |
| 3b | 0 | 0 | 0 | 0 | 20 | 0 | 20 |
| 4 | 0 | 0 | 0 | 0 | 57 | 0 | 57 |
| 5 | 4 | 0 | 0 | 0 | 68 | 0 | 72 |
| outside the waves | 142 | 8 | 3 | 0 | 48 | 4 | 205 |
| total | 190 | 55 | 3 | 0 | 212 | 4 | 464 |
<!-- the end of the counts -->

## Now
- F138 Auto-Save off and the harness time limits: MERGED as PR 127, 37f37d4, at 17:08, its part 2 in run.ps1 after F131
- F132 mirrored tests: attempt 7 for Q137 A and Q138 B at 34cd532 with main 2eda020 in, no pull request yet
- F131 teams: add-in attempt 3 at c436715, read APPROVE, its harness next now that F138 has merged
- F114 views: at e77e8a7 since 2026-10-05, its add-in half after F132's
- F139 this page: one draft pull request on fix-F139 with main 37f37d4 in, the lead's readings under Q139 his to correct
- Probes and runs: paused since 2026-10-05 17:08 until F138 merged, whether started again since is UNKNOWN here

## Next
1. A real start that writes no autosave into his folder, F138's proof, before any other run
2. F131's harness, then F131, F132 and F114 merged in the order of Q132
3. Main installed in place, then 1A02MM and 1A04PK run with the new views on, Q132
Beside them: F139 merged, then F134 the code health gate and F135

## Waiting for Bader
- Q25, Q27 to Q31, Q35 to Q40, Q45 to Q47, Q49 to Q51 and Q76 to Q78, as before
- F18, the 1A04WE sample, when he uploads it

## Blockers and known bugs
- Whether Navisworks reads "3 0" as Auto-Save off is UNKNOWN until a real start writes no autosave
- T5-R-HELDOFF: a run that leaves "3 0" makes the next backup read it as his, put back by hand
- T5-R-OLDKEY: 40 values of his 22.0 key went back to older values, the writer UNKNOWN
- Until F114 merges every test run has the viewpoints box unticked, and the C02 weekly stays stopped, Q130
- The keep-awake reads steps\loop.md, which F139 moves, so it stops on STATE CLOSED only once it reads this page

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5
