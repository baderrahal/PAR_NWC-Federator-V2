STATE OPEN, 2026-10-11 19:05, last run 06/item1-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 37 | 1 | 0 | 0 | 28 | 0 | 66 |
| 2b | 28 | 2 | 0 | 1 | 9 | 0 | 40 |
| 2c | 7 | 1 | 0 | 0 | 3 | 1 | 12 |
| 3a | 19 | 0 | 0 | 0 | 3 | 0 | 22 |
| 3b | 13 | 1 | 0 | 0 | 7 | 0 | 21 |
| 4 | 6 | 1 | 0 | 0 | 51 | 0 | 58 |
| 5 | 18 | 4 | 0 | 0 | 50 | 0 | 72 |
| outside the waves | 165 | 14 | 0 | 0 | 59 | 6 | 244 |
| total | 312 | 24 | 0 | 1 | 210 | 7 | 554 |
<!-- the end of the counts -->

## Now
- Bader's orders of 11 Oct: 1A04PK IS FROZEN, the life check and the cause fixed, and NO MORE TWO HOUR TEST LOOPS, fast checks on 1A02MM and a cut down 1A04PK, the one full run only when he says
- Set 06's 1A04PK, started 14:25, still running, busy and not stuck, 25 of 741 tests by 16:41. Its 12 h ceiling is 02:25. Asked Bader at 16:50 and 18:20 whether to close it now, no answer yet
- Merged today: F141 views as PRs 196 and 197, F142 the life check as 198, F143 the harvest answering between rows as 199, F144 the first-50 copy as 200, main 89082a2
- Set 07 made at 18:56, the corrected matrix under runs\07\NMFed\exchange and its first-50 cut, 52 tests and one pair, under runs\07\NMFed\first50

## Next
1. Close the frozen run once Bader says, or at its ceiling. Then main 89082a2 installed in place and the fast checks on set 07: 1A02MM on the full matrix and 1A04PK on first50, the life check on, -Untick SkipClashOffCoordinates, three lines after each
2. The speed fix of the harvest's cost per row on 1A04PK, measured off the cut down run's IMAGES line, two attempts, the output the same
3. Ask Bader before the one full 1A04PK run. Then F140 prepared, waves 4 and 5

## Waiting for Bader
- Whether to close the frozen 1A04PK now. The team package, F140, made only when he writes make the team package, Q149

## Blockers and known bugs
- The frozen 1A04PK holds the only Navisworks, so no fast check can start
- F141-R1: every view of one clash failed as added without its dimming, to be read on the fast check
- T5-R-OLDKEY: his 22.0 key reads an older copy after each restart, the writer UNKNOWN
- F142-R1: one proof run of three died with no error, cause UNKNOWN

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
