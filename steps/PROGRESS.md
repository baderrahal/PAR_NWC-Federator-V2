STATE OPEN, 2026-10-19 03:25, the round of F231 to F238, F233 merged in 314, F234 in review, proved on C06 and C07, last run 94/item1-C02


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
| outside the waves | 315 | 16 | 0 | 26 | 117 | 7 | 481 |
| total | 519 | 24 | 0 | 26 | 217 | 18 | 804 |
<!-- the end of the counts -->
## Now
- 19 Oct, the round of F231 to F238 open, Bader's six fixes, the full run on final-2026-10-19 and the end, plan in log.md.
  F231 measured the append against a manual Append on 1B06BC, 1B06PS and 1B06K1, 0.000 mm after both pages, the far
  models in the NWC files, no code changed, scan.md 5z-zn, claude\audit-round-2026-10-19.md. F232 packages first, merged
  in 313. F233 one discipline groups not built on both pages, merged in 314, rows F233-R1 to R10. F234 the discipline
  workbooks in Bader's layout, proved on copies of C06 and C07, 8 workbooks each summary equal to its rows, ST 34 m
- 18 Oct: F212 to F230, 292 to 311, a Navisworks per group, Auto-Save off, two set codes, the retake, the audits
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 18 Oct 13:30, multi2, C06, C07, Corrupt, both pages | 026a1447, main after 299 | C06 3605 and 3623, C07 2709 twice, C06 stops from group 11 | not run | none read | NWC page 22.1 min, NWF page 19.6 min |
| 18 Oct 20:24, multi3, C06, C07, SKY, both pages, a Navisworks a group | e38344c7, main after 309 | no stop in any of 46 groups, 1B06PK 4330 MB at most | not run | none read | NWC page 28.2 min, NWF page 26.4 min, 46 starts about 200 s and 46 quits 93 s a page |
## Next
1. F234 merged, then F235 Q187 and Q195 as answered in the round
2. F236 the clash count check
3. F237 the full run on final-2026-10-19, then F238 the audit, the proof steps and the closing entry

## Waiting for Bader
- Q213 a parent and a folder of its name, Q214 the changed list and the table of the NWF page, Q215 where Run sits
- Q216 the two groupings by discipline, Q217 the Backup of an NWF of one discipline, Q218 and Q219 the workbooks
- Q212 the tests a rule left out still held in an NWF refreshed before it, leave them, delete once or a folder of their own
- Q194 Approved pictures, Q195 the rule boxes on the NWF page, Q196 Layer, Q197 Item Name, Q198 logo, Q199 relative links
- Q200 foundations, Q201 the look of the pictures. Q187 caps, Q188 512 pixels, Q189 by design pairs, Q190 folders, Q191 C02, Q192 numbers
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
