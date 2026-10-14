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
| outside the waves | 210 | 16 | 0 | 3 | 60 | 7 | 296 |
| total | 413 | 24 | 0 | 3 | 161 | 18 | 619 |
<!-- the end of the counts -->

## Now
- Bader's message of 13 Oct, Q162, five things: all merged on 14 Oct, F151 232, F152 233, F153 234, F154 235, F155 236
- Bader's order of 14 Oct, tasks A to D: F157 239 pictures trail, F158 240 Log folder, F159 241 skip option gone, F161 242
- F160, C04 under NM fed, main 6c47bef4: Navisworks crashed at picture 8907 of 10170, GDI objects 9999, lcodwin.dll
- Bader's six answers of 14 Oct, Q163 to Q168, one PR each: F164 label, F165 notes, F166 pack by code, F167 tolerance,
  F168 saved tests run, F169 picture click link

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
3. tools\loop\prove-run.ps1 stops at its install.ps1 trials on main, 28 FAIL, and its H12b trial of install.ps1 at
   0eb4ede exits 1 on any build since F152, so its window cases do not run
3. The register rows of 13 Oct: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- Q163 to Q168 answered on 14 Oct, built in F164 to F169 one by one, see the Now lines
- Q169 the Log place, Q170 a model on Internal ends FAILED, Q171 the GDI limit, the report is at the top of 03_bader_next
- What Excel shows a person when it opens a workbook of 4160 linked pictures, steps 1435 to 1437

## Blockers and known bugs
- Navisworks crashes at about picture 8900, one GDI object a picture, 10000 allowed, F160 and Q171
- A saved test the built in matrix does not name is not run since F155, Q168. F163: tools\loop names two boxes now gone
- One DIALOG finding at the close of set 94 UNKNOWN. FR-067 is out, a probe answers it first
- T6-R-WPFBOX and F149-R1, as before. Q169 the Log place and Q170 a model on Internal still FAILS, in 02_questions

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
