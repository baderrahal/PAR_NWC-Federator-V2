STATE OPEN, 2026-10-14 01:40, last run 94/item1-C02

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
| outside the waves | 196 | 16 | 1 | 7 | 59 | 7 | 286 |
| total | 399 | 24 | 1 | 7 | 160 | 18 | 609 |
<!-- the end of the counts -->

## Now
- Bader's message of 13 Oct, Q162, five things: all merged on 14 Oct, F151 232, F152 233, F153 234, F154 235, F155 236
- F156 237: build\install.ps1 refused every build since F152, found by the loop's install, fixed and installed
- Set 94, main 51d9f31d, 1A02MM through the window: RAN, no log beside the NWF, 4160 linked pictures and none embedded,
  130 blocks, the built in pack, 25 mm on every test. Excel read the 4160 as linked pictures. steps\history\log.md

## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 13 Oct 20:17, set 93 | cad45fd4, main after 230 | 0.028 s | 232 s, group FAILED on a model on Revit's internal origin | about 30 s, WORKBOOK | PICTURES 117 s |
| 14 Oct 01:28, set 94 | 51d9f31d, main after 237 | 0.027 s | 245 s, group FAILED on the same model | none in 10 readings | PICTURES 112.5 s |

## Next
1. Bader's proof of the round on his desktop PC, steps 1390 to 1513 of steps\03_bader_next.md: the build and install
   in VS Code, the four NWC files of 1A02MM copied by hand, and what only Excel shows, steps 1435 to 1446
2. His answers to Q163 to Q168
3. tools\loop\prove-run.ps1 stops at its install.ps1 trials on main, 28 FAIL, and its H12b trial of install.ps1 at
   0eb4ede exits 1 on any build since F152, so its window cases do not run
4. The register rows of 13 Oct: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- Q163 the picture click link, Q164 a second pack, Q165 ReadPicked and cut-xml.ps1, Q166 the Weekly run plus XML label,
  Q167 the two checks that still read the XML's tolerance, Q168 a saved test the matrix does not name is not run
- What Excel shows a person when it opens a workbook of 4160 linked pictures, steps 1435 to 1437

## Blockers and known bugs
- A saved test the built in matrix does not name is not run since F155, Q168
- One DIALOG finding at the close of set 94, a captionless Navisworks window after RESULT, UNKNOWN
- FR-067 is out: P15 shows its sequence crashed Navisworks twice, so a probe answers it first
- T6-R-WPFBOX, F149-R1 and the coordinates box starting ticked, as before

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
