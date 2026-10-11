STATE OPEN, 2026-10-11 21:16, last run 08/item1-C04

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
| outside the waves | 165 | 14 | 0 | 1 | 59 | 6 | 245 |
| total | 312 | 24 | 0 | 2 | 210 | 7 | 555 |
<!-- the end of the counts -->

## Now
- Bader's orders of 11 Oct: 1A04PK IS FROZEN, the life check and the cause fixed, and NO MORE TWO HOUR TEST LOOPS, fast checks on 1A02MM and a cut down 1A04PK, the one full run only when he says
- Set 07's 50 test cut of 1A04PK, 07/item1-C04, stopped by the lead at 20:35 after 37 min 38 s at test 16 of 52, row 305 of 1556, his settings put back, VERDICT STOPPED, F144-R1
- Set 08's three test cut, 08/item1-C04, main 89082a2: group 884.7 s, pictures 833.1 s of which 830.7 s rendering, 8.0 s a picture, 3 of 3 counts agree, both views failed on the dimming, F141-R1. Q150 to Bader on the pictures
- F141 attempt 2 on fix-F141c: every camera read before the first view, P18's and F85's shape, read by the reviewer and the breaker with nothing blocking, its pull request next

## Next
1. F141 attempt 2 merged, main installed in place, 1A02MM's fast check on set 08's C02, steps 629 to 633
2. Bader's answer to Q150, then the 50 test cut of 1A04PK with it
3. Ask Bader before the one full 1A04PK run. Then F140 prepared, waves 4 and 5

## Waiting for Bader
- Q150, the pictures of 1A04PK: a cap per test, smaller pictures, or as they are
- The team package, F140, made only when he writes make the team package, Q149

## Blockers and known bugs
- F141-R1: every view read back with no material override, attempt 2 waiting for its merge and fast check
- F143-R2: one render of 4 to 8.6 s a row on 1A04PK, the window answering only between rows
- F144-R1: the 50 test cut of 1A04PK is hours long while every picture is rendered, Q150
- T5-R-OLDKEY: his 22.0 key reads an older copy after each restart, the writer UNKNOWN

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
