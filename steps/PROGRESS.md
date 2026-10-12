STATE WAITING, 2026-10-12 10:45

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 37 | 1 | 0 | 0 | 28 | 0 | 66 |
| 2b | 29 | 1 | 3 | 1 | 5 | 1 | 40 |
| 2c | 7 | 1 | 0 | 0 | 3 | 1 | 12 |
| 3a | 19 | 0 | 0 | 0 | 3 | 0 | 22 |
| 3b | 13 | 1 | 0 | 0 | 7 | 0 | 21 |
| 4 | 6 | 1 | 0 | 0 | 51 | 0 | 58 |
| 5 | 18 | 4 | 0 | 0 | 50 | 0 | 72 |
| outside the waves | 175 | 16 | 0 | 2 | 60 | 6 | 259 |
| total | 323 | 25 | 3 | 3 | 207 | 8 | 569 |
<!-- the end of the counts -->

## Now
- Turn 6 closed on Bader's order of 12 Oct, WRAP UP NOW, NO MORE TESTS. No probe, fast check, speed loop or run until he says
- Merged: 202, 204 F145, 207 F149, 205 F146, 208 F140 the team package, and this records pull request
- The team zip: %USERPROFILE%\OneDrive - Parsons Corp\Documents\NWC Federator package\ParsonsNwcFederator-1.0.0.0-69de6ed4-20261012.zip, 3298080 bytes, 21 files
- The closing entry of steps\history\log.md holds what is proven by a run, what is built and not proven, and the known issues

## Speed
No measurement with F145, F146 or F149 merged. Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 11 Oct 21:42 | cdf8d4e, acceleration off | 7.76 s | 293 s | 32 s, RENUMBER | HARVEST 187 s |
| 12 Oct 01:20 | cdf8d4e, acceleration on | 0.168 s, 0.120 s with F145's rule | 253 s | 32 s, RENUMBER | HARVEST 150 s |
| 12 Oct 03:47, probe only | cdf8d4e's code, Hide Other on for one start | 0.067 s, 0.059 s with F145's rule | not run | not run | not run |

## Next
1. Bader installs the zip on his desktop PC: extract it and double click Install Parsons NWC Federator.cmd
2. He runs his own folder through the tool's window, the guide's steps 1 to 11
3. He sends back the run's .log and the .tsv beside it, and the loop reads them when he says

## Waiting for Bader
- His run on his PC and its log
- Q155, F147 stopped under Q148 B
- F150, Hide Other set by the tool, needs one probe of the API, which waits for his word

## Blockers and known bugs
- T6-R-WPFBOX: a caption less WPF window of Navisworks as the tool's window closed in set 09
- F149-R1: the pictures' guard stops a run only once every test of the group has run
- The coordinates box starts ticked, so a group whose models sit off shared coordinates is not clashed unless it is unticked

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
