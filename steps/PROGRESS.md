STATE OPEN, 2026-10-13 11:57, last run 10/item1-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 57 | 0 | 0 | 0 | 9 | 8 | 74 |
| 2b | 34 | 1 | 0 | 1 | 5 | 1 | 42 |
| 2c | 9 | 2 | 0 | 0 | 0 | 1 | 12 |
| 3a | 21 | 0 | 0 | 0 | 0 | 1 | 22 |
| 3b | 21 | 0 | 0 | 0 | 0 | 0 | 21 |
| 4 | 14 | 1 | 0 | 0 | 43 | 0 | 58 |
| 5 | 25 | 4 | 0 | 0 | 43 | 0 | 72 |
| outside the waves | 180 | 16 | 0 | 0 | 60 | 6 | 262 |
| total | 380 | 24 | 0 | 1 | 160 | 17 | 582 |
<!-- the end of the counts -->

## Now
- Bader said go at 09:08 on 13 Oct, under his order of 12 Oct headed BADER SAYS GO and the limits of Q143
- Merged since: 216, 209, 218, 211 on 12 Oct, then 212, 213, 217, 215, 219 and 214 F150 with this page
- Set 10, the one 1A02MM check of F150, ran at 028c3a71: pictures 35 KB each showing only their two items, views 69 of 69
- The developer works FR-150, FR-152, FR-155 and FR-162 of F121, wave 4, on fix-F121d

## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 12 Oct 01:20 | cdf8d4e, acceleration on | 0.168 s, 0.120 s with F145's rule | 253 s | 32 s, RENUMBER | HARVEST 150 s |
| 13 Oct 11:40, set 10 | 028c3a71, F150 Hide Other | 0.031 s, 35 KB | 169 s | about 30 s, WORKBOOK | clash step 140.5 s |

## Next
1. Renumber the proof steps once on a records branch off main
2. Build the new zip from main and give Bader its path, size and what changed since 69de6ed4
3. Read and merge fix-F121d, then the window rows of F121: FR-153, FR-160, FR-163 and FR-166

## Waiting for Bader
- Q38 said delete the per item harvest. 215 kept the source file read since the merge key reads it
- F150-R2: Hide Other put back as off writes "3 0" where his PC held "0", a value never set. Off either way
- His run on his PC and its log, from the team zip

## Blockers and known bugs
- The WORKBOOK step leaves the window not answering for about 30 s on 1A02MM
- FR-067 is out: P15 shows its sequence crashed Navisworks twice, so a probe answers it first
- T6-R-WPFBOX, F149-R1 and the coordinates box starting ticked, as before

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
