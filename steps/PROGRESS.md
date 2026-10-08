STATE OPEN, 2026-10-08 10:29, last run 04/item2-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 30 | 4 | 0 | 0 | 27 | 0 | 61 |
| 2b | 11 | 16 | 0 | 1 | 5 | 0 | 33 |
| 2c | 6 | 1 | 0 | 0 | 4 | 0 | 11 |
| 3a | 8 | 4 | 0 | 0 | 8 | 0 | 20 |
| 3b | 9 | 1 | 0 | 0 | 10 | 0 | 20 |
| 4 | 6 | 1 | 0 | 0 | 50 | 0 | 57 |
| 5 | 14 | 3 | 0 | 0 | 55 | 0 | 72 |
| outside the waves | 156 | 11 | 0 | 2 | 47 | 5 | 221 |
| total | 259 | 41 | 0 | 3 | 206 | 5 | 514 |
<!-- the end of the counts -->

## Now
- 8 Oct: the shutdown came at 19:30:51 on 7 Oct, Arab Standard Time, 46 min after the close at 18:45. A Roamer the loop did not start ran 08:36 to 08:43. His 22.0 key against the backup of 17:34 differs in two CER uptime counters only, Navisworks's own, nothing put back. Keep-awake pid 38340
- The two the shutdown cut are merged: 157, the close of 7 Oct with main taken in, as 83deae0, and 159, F118's FR-199 and FR-040 with its records, as 1dd68a0, FR-036 and FR-041 on the laptop
- Lane B, steps\lane-b.md: on again this morning under Bader's message to it, 167, 168 and 169 merged, F127's and F119's Core points, 169 folded here, 170 open on F115 words, then F137, F121 and F123
- F132 mirrored tests: merged as PR 177, the add-in half in two attempts under Q143, Q144 put to Bader on the no XML by design pass, its proof the timed runs
- Expected release from the pace: 2026-10-13, Q143 item 14, about ten add-in items on the laptop lane at two or three a day over 8, 9, 12 and 13 Oct

## Next
1. F128 generic models by Q145: the one probe of which property names them on 1A02MM and 1A04PK, lane B's Core part, then the add-in part
2. F120 the pictures that fail and their speed, then F114's add-in pass with the viewpoints box back on at its merge, Q131
3. Main installed and the timed runs of 1A02MM and 1A04PK proving everything merged, then F129, F130, F109, the close from 18:40 with STATE NIGHT

## Waiting for Bader
- Q144, a run with no XML and the by design pass on the saved tests, the lead's choice B applied meanwhile. Steps 228 to 233, the published NWD in ACC, his at the final run after F114 merges

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
