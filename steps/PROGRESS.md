STATE OPEN, 2026-10-12 01:32, last run 09/item1-C04

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
| outside the waves | 167 | 14 | 0 | 1 | 58 | 6 | 246 |
| total | 314 | 24 | 0 | 2 | 209 | 7 | 556 |
<!-- the end of the counts -->

## Now
- Bader's order of 12 Oct, KEEP FIXING THE TIME UNTIL IT IS AT ITS MINIMUM: measure, fix the slowest step, measure again, until every target is met or two fixes in a row each gain under 10 percent
- The 150 times gap: Navisworks' hardware acceleration was off, hw_accel "3 0", so every picture was drawn by the Microsoft Basic Render Driver on the processor. Switched on at 01:06 by Bader's leave, "3 1", the old value in probes\gpu-fix-20261012. Roamer now draws on the NVIDIA RTX 2000 Ada
- F145 at 8aa6273, pull request 204, read by the reviewer and the breaker three times. F148 merged as 203. Merges of 202 and 204 wait: Claude Code's permission check refused the merge of 202, Bader to merge or allow
- F146, the renumber answering, parked on fix-F146

## Speed
Targets on this laptop: a picture 0.2 s or less, 1A02MM 3 min or less with its views, no freeze over 5 s, the full 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 11 Oct 21:42 | cdf8d4e, acceleration off | 7.76 s | 293 s | 32 s, RENUMBER | HARVEST 187 s |
| 12 Oct 01:20 | cdf8d4e, acceleration on | 0.168 s, 0.120 s with F145's rule | 253 s | 32 s, RENUMBER | HARVEST 150 s |

## Next
1. The slowest step of 1A02MM, HARVEST, its pictures 95 s rendering and 33 s saving: F145 merged and installed, then the fast checks
2. RENUMBER's 39 s and its 32 s freeze: F146, then the rename itself
3. The full 1A04PK once the fast checks meet the targets and no Navisworks of Bader's is open

## Waiting for Bader
- The merges of pull requests 202 and 204, which Claude Code's permission check refused for the loop
- The team package, F140, made only when he writes make the team package, Q149

## Blockers and known bugs
- A caption less WPF window of Navisworks came up as the tool's window closed in both of set 09's runs, its text UNKNOWN, first seen with acceleration on
- F143-R2: the window answers only between rows, so one long render freezes it
- T5-R-OLDKEY: his 22.0 key reads an older copy after each restart, the writer UNKNOWN

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
