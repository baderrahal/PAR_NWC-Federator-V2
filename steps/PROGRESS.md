STATE OPEN, 2026-10-14 17:30, round 4 Part 1 merged, last run C04 under NM fed 20261014-161532, pictures stopped at 9000, NWD step hung at 26 GB

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
| outside the waves | 228 | 16 | 1 | 1 | 64 | 7 | 317 |
| total | 431 | 24 | 1 | 1 | 165 | 18 | 640 |
<!-- the end of the counts -->

## Now
- Round 4 of 14 Oct, one PR each. Part 1 merged: F175 pictures cap by priority 255, F176 no shared site is a NOTICE 256,
  F177 Clash report overwrite and leftover cd jpgs 257, F178 answers recorded 258, F179 research note 259
- F180 the C04 run, report at the top of steps\03_bader_next.md. Pictures stopped at 9000, no GDI crash, then Navisworks
  grew to 26 GB and the NWD step hung with a 0 byte NWD. Run stopped, Part 2 NOT started, as ordered
- F160, the 8907 crash, is answered by F172 and F175 as far as GDI goes. Memory is the next wall, F180-R1
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 14 Oct 01:28, set 94 | 51d9f31d, main after 237 | 0.027 s | 245 s, group FAILED on the same model | none in 10 readings | PICTURES 112.5 s |
| 14 Oct 16:15, C04 1A04PK | 8e647af8, main after 259 | 0.08 s, 7904 in 637 s | not run, 1A04PK hung at the NWD after 47 min | UNKNOWN | VIEWS 1517 s |

## Next
1. Q175: Bader chooses what the tool does about memory, 3.3 MB for each picture. Then F181 to F185 of Part 2, which wait
2. Bader's proof of the round on his desktop PC, steps\03_bader_next.md
3. tools\loop\prove-run.ps1 was not run after F173, so its window cases are UNKNOWN
4. The register rows of 13 Oct: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- Q175, the memory limit and the order of the NWD, steps\02_questions.md
- What Excel shows a person when it opens a workbook of linked pictures, and whether its click link that goes up
  one folder to Clash report opens the jpg, steps\03_bader_next.md

## Blockers and known bugs
- A run of C04 does not end: 7904 pictures take 26 GB and the NWD step hangs. F180-R1, Q175
- One DIALOG finding at the close of set 94 UNKNOWN. FR-067 is out, a probe answers it first
- T6-R-WPFBOX and F149-R1, as before
- The 0 byte NWD in NWD\C04\run 2026-10-14 1615 is the hung run's, Bader may delete it
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
