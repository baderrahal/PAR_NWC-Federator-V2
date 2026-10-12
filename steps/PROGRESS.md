STATE NIGHT, 2026-10-12 15:56

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 37 | 1 | 1 | 0 | 24 | 3 | 66 |
| 2b | 28 | 2 | 0 | 1 | 9 | 0 | 40 |
| 2c | 9 | 2 | 0 | 0 | 0 | 1 | 12 |
| 3a | 19 | 0 | 0 | 0 | 3 | 0 | 22 |
| 3b | 13 | 1 | 0 | 0 | 7 | 0 | 21 |
| 4 | 6 | 1 | 0 | 0 | 51 | 0 | 58 |
| 5 | 18 | 4 | 0 | 0 | 50 | 0 | 72 |
| outside the waves | 175 | 16 | 0 | 2 | 60 | 6 | 259 |
| total | 324 | 27 | 1 | 3 | 204 | 10 | 569 |
<!-- the end of the counts -->

## Now
- Bader closed the laptop at about 15:55 on 12 Oct and will say go. The fix round of waves 2 to 5 is unfinished. No agent runs, no Navisworks runs, every branch is pushed and clean. 210 is merged
- Each pull request is a draft on its own branch. Their state, 209 to 219, is in the next list

## Open pull requests, head and what is left
- 209 F127c bf1c414: attempt 2 read, APPROVE. Fix proof steps 687 to 696 first: add steps picking an NWF and an NWD folder, Scan again instead of give back, Folder under Existing NWF, then merge
- 211 F132c cae0f84, 212 F115c f7b2c5d, 213 F114c 7d29148, 214 F150 147ef46: attempt 2 pushed, one breaker re-read each is left. 214 also needs its 1A02MM check before it merges
- 215 F120c 03573b2: main merged in, first reading left. 216 F129c cbb3abd: records only, drop of F129-R1, main in, merge once its checks are green
- 217 F147 88d8013: attempt 3 fixes pushed, breaker's narrow re-read left. 218 F130c 233b455: reviewer wants proof steps 698 to 704 split to one action each, then merge
- 219 F119c 4ec1733: first reading left, and main not yet merged in

## Next
1. Say go. Run mergemain.py in turn6 on a branch to take main in, readers two at a time, smallest first
2. Merge each green pull request, then install main in place, run 1A02MM once with viewpoints ticked for the wave and for F150
3. From 17:00 or when done, build the new zip from main and give Bader its path, size and what changed

## Waiting for Bader
- His word to go
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
