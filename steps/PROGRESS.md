STATE OPEN, 2026-10-08 08:55, last run 04/item2-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 24 | 7 | 1 | 0 | 25 | 0 | 57 |
| 2b | 11 | 16 | 0 | 1 | 5 | 0 | 33 |
| 2c | 6 | 1 | 0 | 0 | 4 | 0 | 11 |
| 3a | 8 | 4 | 0 | 0 | 8 | 0 | 20 |
| 3b | 8 | 2 | 0 | 0 | 10 | 0 | 20 |
| 4 | 6 | 1 | 0 | 0 | 50 | 0 | 57 |
| 5 | 14 | 3 | 0 | 0 | 55 | 0 | 72 |
| outside the waves | 156 | 10 | 0 | 1 | 47 | 5 | 219 |
| total | 252 | 44 | 1 | 2 | 204 | 5 | 508 |
<!-- the end of the counts -->

## Now
- 8 Oct: the shutdown came at 19:30:51 on 7 Oct, Arab Standard Time, 46 min after the close at 18:45. A Roamer the loop did not start ran 08:36 to 08:43. His 22.0 key against the backup of 17:34 differs in two CER uptime counters only, Navisworks's own, nothing put back. Keep-awake pid 38340
- PR 157, the close of 7 Oct, finished with main taken in and lane B's night folded. PR 159, F118's FR-199 and FR-040 with its records, read APPROVE by its one reviewer, merged, FR-036 and FR-041 on the laptop
- Lane B, steps\lane-b.md: on again this morning under Bader's message to it, F127's Core points merged as 167 and F119's as 168, both folded here, now on F119 then F115, F137, F121 and F123
- F132 mirrored tests: the add-in half attempt 2 of two on F132-R4 and F132-R5 in work on fix-F132, one reviewer and one breaker next
- Expected release from the pace: 2026-10-13, Q143 item 14, about ten add-in items on the laptop lane at two or three a day over 8, 9, 12 and 13 Oct

## Next
1. F132's add-in half attempt 2 read and merged if no fault the team sees remains, then F114's add-in pass
2. Main installed in place and the timed runs of 1A02MM and 1A04PK with the new views on, against 2 h 12 min for 1A02MM and the hung run of 1A04PK
3. F109, F129, F130, F120 in Q143's order, three lines in the tab every hour, the close from 18:40 with STATE NIGHT and the one records PR

## Waiting for Bader
- No question. Steps 228 to 233, the published NWD in ACC, his at the final run after F114 merges

## Blockers and known bugs
- T5-R-WALKRACE: the harness's walk of the loop folder races a lane's build and stops on a HARNESS FAULT
- T5-R-HELDOFF: a run that leaves "3 0" makes the next backup read it as his, put back by hand
- T5-R-OLDKEY: his 22.0 key reads a July copy after each restart, the writer UNKNOWN, his own sessions run on it
- Until F114 merges every test run has the viewpoints box unticked, and the C02 weekly stays stopped, Q130
- The day closes from 18:40, before the company shutdown at about 19:30 Riyadh time. The Stop gate lets a lane-b branch stop

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
