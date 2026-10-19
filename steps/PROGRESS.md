STATE OPEN, 2026-10-19 09:40, the round of 19 Oct b, F239 to F242, in work, F241 merged in 321

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
| 5 | 26 | 4 | 0 | 0 | 44 | 0 | 74 |
| outside the waves | 323 | 16 | 0 | 28 | 135 | 7 | 509 |
| total | 527 | 24 | 0 | 28 | 235 | 18 | 832 |
<!-- the end of the counts -->
## Now
- 19 Oct b, four items, no Navisworks run, plan in log.md. F240 models 320, F241 Rounds and ai-max 321. Next F239, F242
- 19 Oct, the round of F231 to F238 done, Bader's six fixes, the full run on final-2026-10-19 and the end, F238 in 319.
  F231 the append measured, 0.000 mm, scan.md 5z-zn. F232 packages first, 313. F233 one discipline groups not built, 314.
  F234 the discipline workbooks, 315. F235 caps only over 750 clashes and penetrations on by default, 316, Q220, Q221.
  F236 the clash count check, 317, on both pages on 1B06PK at f8b485e4, the NWC page at b7e98128, 17 tests, 0 red,
  a count not read UNKNOWN, views read back, mirrors, groups not read, Q223, rows F236-R1 to R10. F237 the full run, both
  pages 32 DONE, 12 not built, 0 red, C06 3904 and C07 1549 clashes as on 18 Oct, 10 picks found, 318. F238 the audit,
  claude\audit-round-2026-10-19.md, the QA, 15 of 65 contradictions fixed as text, 40 listed, the proof steps, 319
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 18 Oct 20:24, multi3, C06, C07, SKY, both pages, a Navisworks a group | e38344c7, main after 309 | no stop in any of 46 groups, 1B06PK 4330 MB at most | not run | none read | NWC page 28.2 min, NWF page 26.4 min, 46 starts about 200 s and 46 quits 93 s a page |
| 19 Oct 05:57, final-2026-10-19, C06, C07, both pages, a Navisworks a group | 6faff297, main after 317 | no test over 750, every clash pictured, 3904 and 1549 | not run | none read | runner 1331 s NWC page, 1228 s NWF page, 32 starts about 134 s and 32 quits 62 s a page |
## Next
1. Bader's proof steps at the top of steps\03_bader_next.md, the full run's 13 first, step 1 opening final-2026-10-19\Report\C06
2. His answers to Q212 to Q223 and the older questions below
3. The 40 contradictions listed in claude\audit-round-2026-10-19.md, 43, 46, 37 and 38 the code ones first
## Waiting for Bader
- Q213 a parent and a folder of its name, Q214 the changed list and the table of the NWF page, Q215 where Run sits
- Q216 the groupings, Q217 the Backup, Q218, Q219 the workbooks, Q220 the lists, Q221 the picture order, Q222, Q223 the count check
- Q212 the tests a rule left out still held in an NWF refreshed before it, leave them, delete once or a folder of their own
- Q194 Approved pictures, Q196 Layer, Q197 Item Name, Q198 logo, Q199 relative links, Q200 foundations, Q201 the pictures
- Q188 512 pixels, Q189 by design pairs, Q190 folders, Q191 C02, Q192 numbers
- Q182 Over 150mm in A and B, Q184 the discipline codes, Q185 Over 150mm of C outside M and E, Q186 which NWFs to rebuild

## Blockers and known bugs
- F229-R1 a run's window prunes the logs folder to 30 and deleted 18 group logs of the run before, copies kept in the Log folders
- F223-R1 to R4 Auto-Save in a Navisworks a person started unmeasured, the hand buttons, a run under the loop, a hand start
- F224-R1 to R4, F225-R1 to R3, F226-R1 to R5 the retake's colour and sky assumptions, F222-R1 to R4, F213-R1, R2, F214-R1 to R4
- F207-R1 views check 4 fails on C04 since the NWF holds hidden models, 2 painted items not its clashes in each view, UNKNOWN which
- One DIALOG finding at the close of set 94 UNKNOWN. T6-R-WPFBOX and F149-R1. The 0 byte NWD in NWD\C04\run 2026-10-14 1615, yours to delete
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
