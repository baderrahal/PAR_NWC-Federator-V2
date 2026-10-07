STATE OPEN, 2026-10-07 11:46, last run 04/item2-C02

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
| outside the waves | 142 | 8 | 6 | 4 | 44 | 5 | 209 |
| total | 190 | 55 | 6 | 4 | 208 | 5 | 468 |
<!-- the end of the counts -->

## Now
- F131 teams: fix-F131 at 0e262d3, its harness 310 passed and 9 failed, the ninth failure still to read
- F132 mirrored tests: fix-F132 at 93b45b1, attempt 9 on the breaker's one blocking point under Q140
- F139 this page: attempt 2 on fix-F139, draft pull request 130, the Stop gate reading the page by content
- F114 views: fix-F114 at e77e8a7, waiting for F131's merge, P11 to P19 and F132's merge
- Probes and runs: paused by turn5\probes-pause.txt, and none starts while a Navisworks of Bader's runs

## Next
1. F131: read the ninth harness failure, fix it if new, open its pull request and merge it once green
2. F132: attempt 9 on the one point under Q140, then its readers
3. F139: the reviewer of attempt 2, then its merge, then Q141 written on main

## Waiting for Bader
- No question. Four rows wait for him: steps 228-233, 346-352 and 364 of steps\03_bader_next.md
- and F139-R8, steps\logs\README.md line 11 still naming steps/log.md

## Blockers and known bugs
- Whether Navisworks reads "3 0" as Auto-Save off is UNKNOWN until a real start writes no autosave
- T5-R-HELDOFF: a run that leaves "3 0" makes the next backup read it as his, put back by hand
- T5-R-OLDKEY: 40 values of his 22.0 key went back to older values, the writer UNKNOWN
- Until F114 merges every test run has the viewpoints box unticked, and the C02 weekly stays stopped, Q130
- The keep-awake reads steps\loop.md, which F139 moves, so it stops on STATE CLOSED only once it reads this page, F139-R1

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5
