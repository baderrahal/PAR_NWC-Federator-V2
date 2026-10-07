STATE OPEN, 2026-10-07 13:42, last run 04/item2-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 6 | 25 | 0 | 0 | 1 | 0 | 32 |
| 2b | 11 | 16 | 0 | 0 | 3 | 0 | 30 |
| 2c | 2 | 0 | 0 | 0 | 7 | 0 | 9 |
| 3a | 8 | 4 | 0 | 0 | 8 | 0 | 20 |
| 3b | 0 | 0 | 0 | 0 | 20 | 0 | 20 |
| 4 | 0 | 0 | 0 | 0 | 57 | 0 | 57 |
| 5 | 4 | 0 | 0 | 0 | 68 | 0 | 72 |
| outside the waves | 149 | 8 | 0 | 5 | 45 | 5 | 212 |
| total | 199 | 53 | 0 | 5 | 209 | 5 | 471 |
<!-- the end of the counts -->

## Now
- F139 this page: MERGED as PR 130, ce7fcf5, at 13:34, the keep-awake reading it since 13:38
- F132 mirrored tests: attempt 11 on fix-F132, one fail closed rule for every test of a pair, Q140
- F114 views: attempt 7 on fix-F114, one rule for a set's team, then its add-in pass
- Probes: Q133 1A04PK, P11 and P12 done, Auto-Save off held on each start, P13 to P19 go on
- F131 merged as PR 135 and F138 as PR 127, the switch proven by the Q133 probe's real start

## Next
1. The probes P13 to P19, one Navisworks at a time, then FR-196's part 2 in run.ps1 with its harness
2. F132: attempt 11 read, then its add-in half, then F114's add-in pass, in Bader's order
3. Main installed in place and the runs of 1A02MM and 1A04PK with the new views

## Waiting for Bader
- Q141: the status of a clash under a result group, under Q138 B, the build going on with A
- Four rows: steps 228-233, 346-352 and 364 of steps\03_bader_next.md, and F139-R8, steps\logs\README.md line 11

## Blockers and known bugs
- T5-R-WALKRACE: the harness's walk of the loop folder races a lane's build and stops on a HARNESS FAULT
- T5-R-HELDOFF: a run that leaves "3 0" makes the next backup read it as his, put back by hand
- T5-R-OLDKEY: his 22.0 key reads a July copy after each restart, the writer UNKNOWN, his own sessions run on it
- Until F114 merges every test run has the viewpoints box unticked, and the C02 weekly stays stopped, Q130
- The day closes from 18:40, before the company shutdown at about 19:30

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5
