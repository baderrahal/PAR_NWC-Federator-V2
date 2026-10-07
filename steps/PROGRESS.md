STATE NIGHT, 2026-10-07 18:45, last run 04/item2-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 23 | 7 | 1 | 0 | 25 | 0 | 56 |
| 2b | 11 | 16 | 0 | 1 | 5 | 0 | 33 |
| 2c | 4 | 1 | 0 | 0 | 4 | 0 | 9 |
| 3a | 8 | 4 | 0 | 0 | 8 | 0 | 20 |
| 3b | 8 | 2 | 0 | 0 | 10 | 0 | 20 |
| 4 | 6 | 1 | 0 | 0 | 50 | 0 | 57 |
| 5 | 13 | 1 | 0 | 0 | 58 | 0 | 72 |
| outside the waves | 156 | 10 | 0 | 1 | 44 | 5 | 216 |
| total | 248 | 42 | 1 | 2 | 204 | 5 | 502 |
<!-- the end of the counts -->

## Now
- SAFE TO SHUT DOWN at 18:45. Closed by Bader's STOP SAFELY procedure, the keep-awake stopped, no Navisworks of the loop runs
- Q143, 2026-10-07: 9 product fixes merged today, 141, 142, 145, 146, 147, 148, 150, 151 and 154, with 144 and 152 the records and the fold, F132's add-in half, attempt 1 of two, pushed as fd936b1 and read CHANGES by the breaker on two faults the team would see, the views and a no XML by design pass, attempt 2 in the morning
- Expected release from the pace: 2026-10-13. Left for the release are about ten add-in items on the laptop lane,
  F132's add-in half, F114's add-in pass, F109, F129, F130, F120 and the add-in halves of F115, F127, F118 and F119,
  at two or three a day over 8, 9, 12 and 13 Oct, then C06 and C07 in full and his three tests of done
- Lane B, steps\lane-b.md: goes on alone tonight with F128's Core part and F121's rest

## Next
1. Morning: System log, Roamer, settings against the last backup, STATE OPEN, then F132's add-in half attempt 2 on F132-R4 and R5
2. F114's add-in pass, then install main and the timed runs of 1A02MM and 1A04PK with the new views
3. Fold lane B's night merges into the tracker, then F109, F129, F130, F120 in Q143's order

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
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5
