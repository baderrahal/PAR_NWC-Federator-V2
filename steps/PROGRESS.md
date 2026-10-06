STATE OPEN, 2026-10-06 16:59

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 5 | 24 | 0 | 0 | 0 | 0 | 29 |
| 2b | 8 | 18 | 0 | 1 | 3 | 0 | 30 |
| 2c | 0 | 0 | 0 | 0 | 6 | 0 | 6 |
| 3a | 0 | 4 | 0 | 0 | 7 | 0 | 11 |
| 3b | 0 | 0 | 0 | 0 | 20 | 0 | 20 |
| 4 | 0 | 0 | 0 | 0 | 56 | 0 | 56 |
| 5 | 0 | 0 | 0 | 1 | 67 | 0 | 68 |
| outside the waves | 133 | 10 | 3 | 20 | 43 | 3 | 212 |
| total | 165 | 56 | 3 | 22 | 202 | 3 | 451 |
<!-- the end of the counts -->

## Now
- F138 Auto-Save off and the harness time limits: attempt 2 records at 823866b with main 2eda020 in, PR 127 draft
- F132 mirrored tests: attempt 7 for Q137 A and Q138 B at 34cd532 with main 2eda020 in, no pull request yet
- F131 teams: add-in attempt 3 at c436715, read APPROVE, its harness after F138 merges
- F114 views: at e77e8a7 since 2026-10-05, its add-in half after F132's
- F139 this page: built on fix-F139 to 1887472 and after, one draft pull request, the lead's readings under Q139 his to correct
- Probes and runs: paused since 2026-10-05 17:08 until F138 merges, turn5\probes-pause.txt

## Next
1. F138 read by one reviewer, its full harness alone, PR 127 merged before any probe or run
2. F131's harness once F138 merges, then F131, F132 and F114 merged in the order of Q132
3. Main installed in place, then 1A02MM and 1A04PK run with the new views on, Q132
Beside them: F139 merged, then F134 the code health gate and F135

## Waiting for Bader
- Q25, Q27 to Q31, Q35 to Q40, Q45 to Q47, Q49 to Q51 and Q76 to Q78, as before
- F18, the 1A04WE sample, when he uploads it

## Blockers and known bugs
- No probe, run or install until F138 merges, Q135
- Whether Navisworks reads "3 0" as Auto-Save off is UNKNOWN until a real start
- T5-R-OLDKEY: 40 values of his 22.0 key went back to older values, the writer UNKNOWN
- Until F114 merges every test run has the viewpoints box unticked, and the C02 weekly stays stopped, Q130
- The keep-awake reads steps\loop.md, which F139 moves, so it stops on STATE CLOSED only once it reads this page

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5
