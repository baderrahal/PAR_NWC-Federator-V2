STATE OPEN, 2026-10-11 13:37, last run 05/item1-C04

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
| outside the waves | 161 | 14 | 0 | 0 | 48 | 6 | 229 |
| total | 308 | 24 | 0 | 1 | 199 | 7 | 539 |
<!-- the end of the counts -->

## Now
- 11 Oct: last shutdown the restart at 08:59:41, Riyadh time. Roamer none. His key differs from the backup in five values, an older copy back after the restarts of 10 Oct, T5-R-OLDKEY, nothing put back. Keep-awake pid 18216
- Bader's answers Q144 B, Q146 B, Q148 B. Q149: no release runs, the proof runs of 1A02MM and 1A04PK the last, the team package prepared and made only when he asks
- F130 Shift ticking merged as PR 192, first today. F129 start from an NWF merged as PR 193, the picked NWFs running the open file run's route. F109 the install merged as PR 194, install.ps1 refusing a link, finding a cut run's leftovers, and ending on one of four lines
- F127 the coverage of the picked tests merged as PR 195: a COVERAGE block and rows per group, a tally in RESULT, the Coverage sheet in the internal workbook beside Generic Models, the clash workbook one sheet again
- Main 4bc4c83 installed in place at 13:05, its INSTALLED line naming it. Set 05's 1A02MM and 1A04PK ran 31 s and 58 s, RAN, PARTIAL, the clash skipped by Bader's coordinates rule, Q98 B2, the EL and ST models of 1A02MM 2,823 km from the reference. Views, the clash workbook, the merge and the counts not reached

## Next
1. F130, F129 and F109 merged in that order, Bader's item 6, done
2. The Coverage sheet moved into the internal workbook, F127-R2, merged as PR 195, done
3. F140 the team package, the reviewer's nine points made in attempt 1 and read once more, no team zip until Bader writes make the team package
4. 1A02MM and 1A04PK run again with -Untick SkipClashOffCoordinates, F126's switch, so the clash runs as in set 04's baseline, the viewpoints box ticked. F140's proof held until Navisworks is closed
5. Findings in set 03's shape for the four runs, the records, and three lines to Bader

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
