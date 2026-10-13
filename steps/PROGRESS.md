STATE OPEN, 2026-10-13 16:32, last run 10/item1-C02

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 57 | 0 | 0 | 0 | 9 | 8 | 74 |
| 2b | 37 | 1 | 0 | 0 | 4 | 1 | 43 |
| 2c | 9 | 2 | 0 | 0 | 0 | 1 | 12 |
| 3a | 21 | 0 | 0 | 0 | 0 | 1 | 22 |
| 3b | 21 | 0 | 0 | 0 | 0 | 0 | 21 |
| 4 | 14 | 1 | 0 | 0 | 43 | 0 | 58 |
| 5 | 25 | 4 | 0 | 0 | 45 | 0 | 74 |
| outside the waves | 194 | 16 | 1 | 6 | 59 | 7 | 283 |
| total | 397 | 24 | 1 | 6 | 160 | 18 | 606 |
<!-- the end of the counts -->

## Now
- Bader said go at 09:08 on 13 Oct, under his order of 12 Oct headed BADER SAYS GO and the limits of Q143
- Merged on 13 Oct: 212, 213, 217, 215, 219, 214 F150, 221 the steps renumbered, 222 and 223 F121 wave 4, 224 F114d, 225 F123b
- The team zip of 8e100bf6, built 12:27, holds everything up to 214. 222 to 225 are on main, built and tested, not run on Navisworks
- No pull request is open and no agent or Navisworks runs

## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 12 Oct 01:20 | cdf8d4e, acceleration on | 0.168 s, 0.120 s with F145's rule | 253 s | 32 s, RENUMBER | HARVEST 150 s |
| 13 Oct 11:40, set 10 | 028c3a71, F150 Hide Other | 0.031 s, 35 KB | 169 s | about 30 s, WORKBOOK | clash step 140.5 s |
| 13 Oct 20:17, desktop PC | cad45fd4, main after 230 | 0.028 s | 232 s, 141 of 141 views, group FAILED on a model on Revit's internal origin | about 30 s, WORKBOOK | PICTURES 117 s |

## Next
1. A zip of main with 222 to 225 when Bader asks, and his run of steps 800 to 867, 1100 to 1158, 1200 to 1244 and 1300 to 1310
2. F114-R3, two clashes of one group with one display name share a key, an add-in counter on the name, then the team zip when Bader asks
3. Wave 4 and 5 rows the team sees that are left: FR-161 the date format control
4. The register rows of today: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- Nothing waits on a question. Bader answered F114-K32 B, F114-K36 B, F150-R2 B and gave his words on F114-K29, read as only the map's teams get pair folders, which he may correct
- Whether a Scan of the same folder keeps typed names and unticked groups. Q38: the source file read is kept for the merge key

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
