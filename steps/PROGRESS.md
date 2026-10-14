STATE OPEN, 2026-10-14 20:00, round 4 second set, Part 1 merged and the C04 run ended clean, Part 2 starts with F185 the checks

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
| outside the waves | 235 | 16 | 1 | 0 | 63 | 7 | 322 |
| total | 438 | 24 | 1 | 0 | 164 | 18 | 645 |
<!-- the end of the counts -->

## Now
- Round 4 of 14 Oct, one PR each. Part 1 merged: F175 pictures cap by priority 255, F176 no shared site is a NOTICE 256,
  F177 Clash report overwrite and leftover cd jpgs 257, F178 answers recorded 258, F179 research note 259
- F180 the C04 run, report at the top of steps\03_bader_next.md. Pictures stopped at 9000, no GDI crash, then Navisworks
  grew to 26 GB and the NWD step hung with a 0 byte NWD. Run stopped, Part 2 NOT started, as ordered
- Round 4 second set: F181 750 trigger 261, F182 memory stop and NWF and NWD before the pictures 262, F183 records 263, F184 the C04
  run ended clean, report at the top of steps  run ended clean, report at the top of steps  Next F183 records, F184 the C04 run, and Part 2 only if that run ends clean. F160 is answered as far as GDI goes3_bader_next.md. Part 2 next, F185 to F189 in the order of Bader's message3_bader_next.md. Part 2 next, F185 to F189 in the order of Bader's message
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 14 Oct 01:28, set 94 | 51d9f31d, main after 237 | 0.027 s | 245 s, group FAILED on the same model | none in 10 readings | PICTURES 112.5 s |
| 14 Oct 16:15, C04 1A04PK | 8e647af8, main after 259 | 0.08 s, 7904 in 637 s | not run, 1A04PK hung at the NWD after 47 min | UNKNOWN | VIEWS 1517 s |
| 14 Oct 18:26, C04 1A04PK | 0947159a, main after 263 | 0.07 s, 3042 in 218 s | not run, 1A04PK ended at 50.4 min | none read | VIEWS 2586 s |

## Next
1. Part 2: F185 the checks, F186 viewpoint folder, F187 refresh and overwrite, F188 the two pages, F189 the final run
2. Bader's proof of the round on his desktop PC, steps\03_bader_next.md
3. tools\loop\prove-run.ps1 was not run after F173, so its window cases are UNKNOWN
4. The register rows of 13 Oct: F114-R2, F123-R1, F123-R2, F132-R7, F115-R29, F115-R30

## Waiting for Bader
- Nothing waits on a question. Q175 C and Q176 750 are answered and recorded by F183
- What Excel shows a person when it opens a workbook of linked pictures, and whether its click link that goes up
  one folder to Clash report opens the jpg, steps\03_bader_next.md

## Blockers and known bugs
- The C04 run now ends, F184 20261014-182641. Over the 45 minute target at 50 minutes, the views took 43 of them, UNKNOWN why
- One DIALOG finding at the close of set 94 UNKNOWN. FR-067 is out, a probe answers it first
- T6-R-WPFBOX and F149-R1, as before
- The 0 byte NWD in NWD\C04\run 2026-10-14 1615 is the hung run's, Bader may delete it
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
