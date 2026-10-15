STATE OPEN, 2026-10-15 13:55, round 6 done, F198 278 and F199 279 merged and proven by two NWF page runs, F200 report, Q182 Q184 Q185 wait for Bader

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
| outside the waves | 262 | 16 | 0 | 3 | 60 | 7 | 348 |
| total | 465 | 24 | 0 | 3 | 161 | 18 | 671 |
<!-- the end of the counts -->

## Now
- Round 4 second set: F181 750 trigger 261, F182 memory stop and NWF and NWD before the pictures 262, F183 records 263, F184 the
  C04 run 264, F185 checks 265, F186 viewpoint tree 266, F187 refresh 267, F188 two pages 268, F189 the final run 269. Round 5 of 15 Oct: F190 caps 270, F191 delete outside the tool folder 271, F192 Over 150mm only inside C 272, F193 records 273, F194 view timing 274, F195 Rebuild box 275, F196 views speed 276, F197 report 277
- Round 6 of 15 Oct: F198 the tool folder named Clash Status 278, F199 the discipline folders under A, B and C 279, F200 the two NWF page runs
  that prove them, report at the top of steps\03_bader_next.md
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 15 Oct 00:15, C04 copy, NWF page | 9eb59ad0, main after 268 | 3066 in 225 s | not run | none read | VIEWS 1723 s, 3 runs 61, 68 and 35 min |
| 15 Oct 04:06, C04 copy, NWF page | 444b9c37, main after 275 | 3184 in 209 s | not run | none read | VIEWS 215 s, the run 10.5 min where it was 61 min |
| 15 Oct 13:12, C04 copy f199, NWF page twice | 770e1c9a, main after 279 | 3194 in 231 s, 2509 in 191 s | not run | none read | VIEWS 401 s and 283 s, the runs 13.2 and 10.5 min |

## Next
1. Bader reads round 6 at the top of steps\03_bader_next.md and answers Q184 and Q185
2. The NWC page run with F199 was not run, only the NWF page. UNKNOWN by a run
3. Bader's proof of the round on his desktop PC, steps\03_bader_next.md
4. tools\loop\prove-run.ps1 was not run after F173, so its window cases are UNKNOWN
5. The register rows of 13 Oct: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- Q182: 47 tests of A and B, 1652 clashes over 150 mm, have no Over 150mm folder. Bader says how to handle them
- Q184: HV and DR in Mechanical and EV in Electrical by the pack's team line, BLD-Security Devices in Other, a run with no XML
- Q185: 1 test of C in Other with 1 clash over 150 mm stays in its one view, as A and B do
- What Excel shows a person when it opens a workbook of linked pictures, and whether its click link that goes up
  one folder to Clash report opens the jpg, steps\03_bader_next.md

## Blockers and known bugs
- The views were the time, 88 to 95 percent, and F196 took them from 3294 s to 215 s, a run from 61 min to 10.5 min on the NWF page
- One DIALOG finding at the close of set 94 UNKNOWN. FR-067 is out, a probe answers it first
- T6-R-WPFBOX and F149-R1, as before
- The 0 byte NWD in NWD\C04\run 2026-10-14 1615 is the hung run's, Bader may delete it
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
