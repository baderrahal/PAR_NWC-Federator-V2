STATE OPEN, 2026-10-11 11:25, last run 04/item2-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 32 | 4 | 0 | 0 | 27 | 0 | 63 |
| 2b | 28 | 2 | 0 | 1 | 9 | 0 | 40 |
| 2c | 7 | 1 | 0 | 0 | 3 | 1 | 12 |
| 3a | 19 | 0 | 0 | 0 | 3 | 0 | 22 |
| 3b | 13 | 1 | 0 | 0 | 7 | 0 | 21 |
| 4 | 6 | 1 | 0 | 0 | 51 | 0 | 58 |
| 5 | 18 | 4 | 0 | 0 | 50 | 0 | 72 |
| outside the waves | 161 | 14 | 0 | 0 | 48 | 6 | 229 |
| total | 303 | 27 | 0 | 1 | 198 | 7 | 536 |
<!-- the end of the counts -->

## Now
- 11 Oct: last shutdown the restart at 08:59:41, Riyadh time. Roamer none. His key differs from the backup in five values, an older copy back after the restarts of 10 Oct, T5-R-OLDKEY, nothing put back. Keep-awake pid 18216
- Bader's answers Q144 B, Q146 B, Q148 B. Q149: no release runs, the proof runs of 1A02MM and 1A04PK the last, the team package prepared and made only when he asks
- F130 Shift ticking merged as PR 192, first today. F129 start from an NWF merged as PR 193, the picked NWFs running the open file run's route. F109 the install merged as PR 194, install.ps1 refusing a link, finding a cut run's leftovers, and ending on one of four lines

## Next
1. F130, F129 and F109 merged in that order, Bader's item 6, done
2. The Coverage sheet moved into the internal workbook, F127-R2, one reviewer
3. F140 the team package prepared on its own branch, no team zip until Bader writes make the team package
4. Main installed in place and 1A02MM and 1A04PK run with the box ticked, steps\notes\item5-runs-brief.md, findings in set 03's shape

## Waiting for Bader
- No question. The team package, F140, is prepared and made only when Bader writes make the team package, Q149

## Blockers and known bugs
- T5-R-WALKRACE: the harness's walk of the loop folder races a lane's build and stops on a HARNESS FAULT
- T5-R-HELDOFF: a run that leaves "3 0" makes the next backup read it as his, put back by hand
- T5-R-OLDKEY: his 22.0 key reads an older copy after each restart, the writer UNKNOWN, seen again on 11 Oct
- T6-R-TESTTEMP: a test fails as its logs build up in the temp folder

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
