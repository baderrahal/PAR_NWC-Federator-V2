STATE OPEN, 2026-10-17 15:50, F205 the audit round of C02 and C04, F203 283 and F204 284 merged, main 800c21c installed, Q187 to Q193 wait for Bader

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
| outside the waves | 267 | 16 | 0 | 11 | 60 | 7 | 361 |
| total | 470 | 24 | 0 | 11 | 161 | 18 | 684 |
<!-- the end of the counts -->

## Now
- 17 Oct audit round: F203 the tests at or under their cap get their pictures first, 283. F204 the clash step shows every hidden item
  and hides it again after, 284, found when a run on Bader's C04 NWF, saved with 8 models hidden, marked 10016 clashes Resolved.
  F205 the runs and the report at the top of steps\03_bader_next.md: the second run keeps every status, GUID, assignee and comment
- 15 Oct: F201 no model's units are set, 281, F202 04 PK within 0 mm of a manual Append. Round 6: F198 to F200
## Speed
Targets: a picture 0.2 s or less, 1A02MM 3 min or less, no freeze over 5 s, 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 15 Oct 13:12, C04 copy f199, NWF page twice | 770e1c9a, main after 279 | 3194 in 231 s, 2509 in 191 s | not run | none read | VIEWS 401 s and 283 s, the runs 13.2 and 10.5 min |
| 17 Oct 15:01, C04 copy, NWF page twice | 800c21c4, main after 284 | 3055 in 207 s, 3080 in 207 s | not run | none read | VIEWS 243 s, the runs 13.3 and 13.4 min |

## Next
1. Bader runs the NWF page on NM fed's C04 NWF with 800c21c4 and reads the CLASH HIDDEN lines, steps\03_bader_next.md
2. Bader answers Q187 to Q193, and Q182, Q184, Q185, Q186
3. C02: AR and ME of 1A02MM are not on shared coordinates and its NWF predates F201, Q191, then a C02 run
4. Why every test reads Old after a run, Q193
5. tools\loop\prove-run.ps1 was not run after F173, so its window cases are UNKNOWN

## Waiting for Bader
- Q187 the caps on every test, Q188 512 pixels so every capped picture fits, Q189 four by design pairs are service against service
- Q190 a run deletes a folder a person made outside Clash Status and Comments, Q191 C02, Q192 the numbers no report shows, Q193 Old
- Q182 Over 150mm in A and B, Q184 the discipline codes, Q185 Over 150mm of C outside M and E, Q186 which NWFs to rebuild

## Blockers and known bugs
- The memory stop still ends the pictures of the 20 largest C04 tests at about 3050, a picture keeps 3.1 MB inside Navisworks, Q188
- Every test reads Old after a run, cause UNKNOWN
- One DIALOG finding at the close of set 94 UNKNOWN. T6-R-WPFBOX and F149-R1, as before
- The 0 byte NWD in NWD\C04\run 2026-10-14 1615 is the hung run's, Bader may delete it
## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
