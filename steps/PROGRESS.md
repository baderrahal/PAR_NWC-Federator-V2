STATE OPEN, 2026-10-18 00:58, F208 288 merged and proved on a copy, F209 in review, run-20261018-004351 and 005237 on a copy, 3b6a16ab of fix-F209 installed


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
| outside the waves | 271 | 16 | 1 | 18 | 61 | 7 | 374 |
| total | 474 | 24 | 1 | 18 | 162 | 18 | 697 |
<!-- the end of the counts -->

## Now
- 18 Oct, Bader's message of 17 Oct late evening: F208 many communities and nested packages, F209 one Excel per discipline per
  package, F210 the test on C06 and C07 in two layouts, F211 the audit of the repo. The plan is the entry of 17 Oct in log.md
- 17 Oct evening, check round F207: the nine items of the chat audit. F206 the pictures taken after the save show every item, 286
- 17 Oct audit round: F203 small tests pictured first, 283. F204 the clash step shows every hidden item, 284. F205 the second run
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 17 Oct 15:01, C04 copy, NWF page twice | 800c21c4, main after 284 | 3055 in 207 s, 3080 in 207 s | not run | none read | VIEWS 243 s, the runs 13.3 and 13.4 min |
| 17 Oct 21:30, C04 copy, NWF page twice | 2ad21979, main after 286 | 3116 in 273 s, 3097 in 279 s | not run | none read | the runs 16.9 and 17.3 min, every model shown for the pictures |

## Next
1. F209 merged on green, then the F210 runs on copies of C06 and C07, flat and ACC, the NWC page then the NWF page
2. Bader runs the NWF page on NM fed's C04 NWF with 2ad21979 and reads the CLASH HIDDEN and IMAGES HIDDEN lines, steps\03_bader_next.md
3. Bader answers Q194 to Q201, Q187 to Q192, and Q182, Q184, Q185, Q186
4. F207-R1: which 2 items each view paints that are not its clashes, read on a copy of C04
5. C02: AR and ME of 1A02MM sit on Revit's internal origin and no newer NWC is on this PC, Q191, then a C02 run
6. tools\loop\prove-run.ps1 was not run after F173, so its window cases are UNKNOWN

## Waiting for Bader
- Q194 Approved pictures, Q195 the rule boxes on the NWF page, Q196 Layer, Q197 Item Name, Q198 logo, Q199 relative links
- Q200 foundations, Q201 the look of the pictures. Q187 caps, Q188 512 pixels, Q189 by design pairs, Q190 folders, Q191 C02, Q192 numbers
- Q182 Over 150mm in A and B, Q184 the discipline codes, Q185 Over 150mm of C outside M and E, Q186 which NWFs to rebuild

## Blockers and known bugs
- F207-R1 views check 4 fails on C04 since the NWF holds hidden models, 2 painted items not its clashes in each view, UNKNOWN which
- The memory stop still ends the pictures of the 20 largest C04 tests, 584 to 598 of 2350, a picture keeps 3.1 MB in Navisworks, Q188
- One DIALOG finding at the close of set 94 UNKNOWN. T6-R-WPFBOX and F149-R1, as before
- The 0 byte NWD in NWD\C04\run 2026-10-14 1615 is the hung run's, Bader may delete it
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
