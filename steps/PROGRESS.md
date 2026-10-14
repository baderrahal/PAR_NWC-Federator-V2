STATE OPEN, 2026-10-14 15:30, round 4 started, last run C04 under NM fed 20261014-105308, crashed at picture 8907

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
| outside the waves | 228 | 16 | 0 | 0 | 63 | 7 | 314 |
| total | 431 | 24 | 0 | 0 | 164 | 18 | 637 |
<!-- the end of the counts -->

## Now
- Bader's order of 14 Oct, round 2, Q163 to Q168: F164 to F169, merged, 244 to 249
- Round 3 merged, F170 to F174, PRs 250 to 254. F174 the C04 run was not made, Bader's own Navisworks was open
- ROUND 4, Bader's order of 14 Oct, one PR each, Part 1 before Part 2. F175 pictures cap by priority A 150 B 100 C 50
  (Task 1, merged 255), F176 no shared site is a NOTICE (2, merged 256), F177 Clash report overwrite and leftover cd jpgs (3, merged 257),
  F178 answers recorded (4, in review), F179 research note steps\notes\nwc-nwf-design.md (6), F180 the C04 run and report (5)
- PART 2 only if the C04 run did not crash: F181 checks (7), F182 one viewpoint folder with a marker (8), F183 refresh
  and clash history kept (9), F184 two page window NWC and NWF (10), F185 final run (11)
- F160, C04 crash at picture 8907, GDI 9999, lcodwin.dll. F172 and F175 are its answer, unproven by a run
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 13 Oct 20:17, set 93 | cad45fd4, main after 230 | 0.028 s | 232 s, group FAILED on a model on Revit's internal origin | about 30 s, WORKBOOK | PICTURES 117 s |
| 14 Oct 01:28, set 94 | 51d9f31d, main after 237 | 0.027 s | 245 s, group FAILED on the same model | none in 10 readings | PICTURES 112.5 s |

## Next
1. Round 4 task 5: Core tests, Release build, install, one run on C04, report at the top of steps\03_bader_next.md.
   Check Roamer.exe first. If one runs, stop and write it down. Do not start Part 2 on a crash
2. Bader's proof of the round on his desktop PC, steps\03_bader_next.md
3. tools\loop\prove-run.ps1 was not run after F173, a Navisworks of Bader's was open, so its window cases are UNKNOWN
4. The register rows of 13 Oct: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- What Excel shows a person when it opens a workbook of linked pictures, and whether its click link that goes up
  one folder to Clash report opens the jpg, steps\03_bader_next.md

## Blockers and known bugs
- Navisworks crashed at about picture 8900, one GDI object a picture. F172 stops the pictures at 9000, UNKNOWN until a run
- One DIALOG finding at the close of set 94 UNKNOWN. FR-067 is out, a probe answers it first
- T6-R-WPFBOX and F149-R1, as before
- Clash report and Clash Report are one folder on Windows, so a run writes into the Clash Report of NM fed
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
