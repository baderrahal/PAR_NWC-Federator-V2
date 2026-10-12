STATE OPEN, 2026-10-12 09:00, last run 09/item1-C04

<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->
## Counts

Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. Done is merged or proven by a run, and dropped stands beside the five so each line adds up.

| wave | done | in progress | in review | waiting for Bader | open | dropped | rows |
|---|---|---|---|---|---|---|---|
| 1 | 19 | 0 | 0 | 0 | 0 | 0 | 19 |
| 2a | 37 | 1 | 0 | 0 | 28 | 0 | 66 |
| 2b | 28 | 2 | 0 | 1 | 9 | 0 | 40 |
| 2c | 7 | 1 | 0 | 0 | 3 | 1 | 12 |
| 3a | 19 | 0 | 0 | 0 | 3 | 0 | 22 |
| 3b | 13 | 1 | 0 | 0 | 7 | 0 | 21 |
| 4 | 6 | 1 | 0 | 0 | 51 | 0 | 58 |
| 5 | 18 | 4 | 0 | 0 | 50 | 0 | 72 |
| outside the waves | 169 | 17 | 0 | 1 | 61 | 6 | 254 |
| total | 316 | 27 | 0 | 2 | 212 | 7 | 564 |
<!-- the end of the counts -->

## Now
- Bader's order of 12 Oct, item 153: measure, fix the slowest step, measure again, until every target is met or two fixes in a row each gain under 10 percent
- Q151 answered, item 154: Hide Other for every picture, set by the tool and put back, the ghosted look one setting off by default. Built next as F150, after one probe of how the API sets it
- 202 merged as 1bfe0d2. The merges of 204, 207, 205 and 206 stopped: Claude Code's auto mode classifier refused the push of fix-F145 and the deletion of fix-Q150 as Auto-Mode Bypass. No settings rule blocks them, item 154's lead note
- F149 at 137741a, pull request 207, read twice and approved. F146 at 0ad7226, 205. F147 at e20a451 stopped under Q148 B, Q155

## Speed
Targets on this laptop: a picture 0.2 s or less, 1A02MM 3 min or less with its views, no freeze over 5 s, the full 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 11 Oct 21:42 | cdf8d4e, acceleration off | 7.76 s | 293 s | 32 s, RENUMBER | HARVEST 187 s |
| 12 Oct 01:20 | cdf8d4e, acceleration on | 0.168 s, 0.120 s with F145's rule | 253 s | 32 s, RENUMBER | HARVEST 150 s |
| 12 Oct 03:47, probe only | cdf8d4e's code, Hide Other on for one start | 0.067 s, 0.059 s with F145's rule | not run | not run | not run |

## Next
1. The merges of 204, then 207, 205 and 206, once Bader allows the loop's pushes and merges again or makes them
2. F150, Hide Other set by the tool for its pictures, after one probe of the API
3. Main installed and the fast checks, a SPEED row
4. The full 1A04PK once the fast checks meet the targets and no Navisworks of Bader's is open

## Waiting for Bader
- The classifier's refusal of the merge sequence, Auto-Mode Bypass: his own merge of 204, 207, 205 and 206, or his word on how the loop may make them
- Q155, F147 stopped under Q148 B
- The team package, F140, made only when he writes make the team package, Q149

## Blockers and known bugs
- T6-R-WPFBOX: a caption less WPF window of Navisworks came up as the tool's window closed in both of set 09's runs
- F149-R1: the pictures' guard can stop a run only once every test of the group has run
- T5-R-OLDKEY: his 22.0 key reads an older copy after each restart, the writer UNKNOWN

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
