STATE NIGHT, 2026-10-12 20:59

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 39 | 1 | 0 | 0 | 22 | 5 | 67 |
| 2b | 28 | 2 | 0 | 1 | 9 | 0 | 40 |
| 2c | 9 | 2 | 0 | 0 | 0 | 1 | 12 |
| 3a | 19 | 0 | 0 | 0 | 2 | 1 | 22 |
| 3b | 14 | 1 | 0 | 0 | 6 | 0 | 21 |
| 4 | 6 | 1 | 0 | 0 | 51 | 0 | 58 |
| 5 | 18 | 4 | 0 | 0 | 50 | 0 | 72 |
| outside the waves | 175 | 16 | 0 | 2 | 60 | 6 | 259 |
| total | 327 | 27 | 0 | 3 | 200 | 13 | 570 |
<!-- the end of the counts -->

## Now
- Bader said go at 19:13 on 12 Oct and asked for a stop when safe in the evening. No agent and no Navisworks runs, every branch is pushed and clean
- Merged this evening: 216 F129c, 209 F127c, 218 F130c, and 211 F132c with this page
- Readers done: breaker re-reads of 211, 212, 213 and 214 and the narrow re-read of 217 found no fault the team sees. 215's first reading asks for changes

## Open pull requests, in Bader's order of merging
- 212 F115c 617d2be: ready. Take main in, add rows F115-R29 one set name in two folders and F115-R30 a null side count refusing every leftover, rows.py merged 212, merge
- 213 F114c a237ca2: ready. Take main in, add a row for a refused mirror pair whose clashes get no view, rows.py merged 213, merge
- 217 F147 88d8013: ready. Take main in, rows.py merged 217, merge
- 215 F120c 03573b2: split proof steps 692 and 697, fix stale words in core.md 1219, 1361, 2277 and addin.md 719, ClashHarvest.cs 647 and the unreachable singular of GapRule.cs 121. Its citation of item 157 holds once 214 merges, which records it
- 219 F119c 4ec1733: first reading left, main not yet taken in
- 214 F150 9f3f742: breaker found no fault. Take main in, install its head in place, run 1A02MM once with viewpoints ticked, check the pictures and the put back, then merge

## Next
1. Say go. In turn6 run mergemain.py then rows.py on 212, push, merge, then 213 and 217 the same way
2. 215's changes and its re-read, 219's first reading, then 214's check on 1A02MM and its merge
3. Renumber the proof steps once, build the new zip from main, then waves 3, 4 and 5, rows the team sees only

## Waiting for Bader
- His word to go
- Q38 said delete the per item harvest. 215 keeps the source file read since the merge key reads it, which may be his to decide
- His run on his PC and its log, from the team zip of 12 Oct, 69de6ed4

## Blockers and known bugs
- Proof step numbers collide across the open branches and are renumbered once, after the merges
- FR-067 is out of 213: P15 shows its sequence crashed Navisworks twice, so a probe answers it first
- T6-R-WPFBOX, F149-R1 and the coordinates box starting ticked, as before

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
