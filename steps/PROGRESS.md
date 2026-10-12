STATE OPEN, 2026-10-12 04:10, last run 09/item1-C04

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
| outside the waves | 167 | 16 | 1 | 1 | 60 | 6 | 251 |
| total | 314 | 26 | 1 | 2 | 211 | 7 | 561 |
<!-- the end of the counts -->

## Now
- Bader's order of 12 Oct, item 153: measure, fix the slowest step, measure again, until every target is met or two fixes in a row each gain under 10 percent
- The 150 times gap had two causes. Hardware acceleration was off, now on since 01:06, scan.md 5z-zd. And the bigger files are Clash Detective's display settings: with Hide Other on, the tool's call draws the two items alone at 0.067 s and 48 KB, the PC's size, scan.md 5z-ze. The accepted reports show other items ghosted, so the look is put to Bader as Q151
- F145 at 8aa6273, pull request 204. F146 at 3ca6e69, pull request 205. F149, pictures in the report's order so nothing is renamed, on fix-F149 over F145, its attempt 2 next on the reviewer's and the breaker's readings
- The probe's pull request 202 carries 5z-zc to 5z-ze

## Speed
Targets on this laptop: a picture 0.2 s or less, 1A02MM 3 min or less with its views, no freeze over 5 s, the full 1A04PK 45 min or less.

| date | build | a picture | 1A02MM | longest freeze | most time |
|---|---|---|---|---|---|
| Bader's PC, 11 Oct | cdf8d4e, DirectX 11 hardware | 0.049 s | 156 s | 0 s | UNKNOWN |
| 11 Oct 21:42 | cdf8d4e, acceleration off | 7.76 s | 293 s | 32 s, RENUMBER | HARVEST 187 s |
| 12 Oct 01:20 | cdf8d4e, acceleration on | 0.168 s, 0.120 s with F145's rule | 253 s | 32 s, RENUMBER | HARVEST 150 s |
| 12 Oct 03:47, probe only | cdf8d4e's code, Hide Other on for one start | 0.067 s, 0.059 s with F145's rule | not run | not run | not run |

## Next
1. F145 merged, Bader's order, then main installed and the fast checks, a SPEED row
2. F149 attempt 2, which takes RENUMBER's 39 s and its 32 s freeze away, then F146 for what is left of the renaming
3. F147, long runs kept from slowing, Q152 item 2
4. The full 1A04PK once the fast checks meet the targets and no Navisworks of Bader's is open

## Waiting for Bader
- Q151, the look of the pictures, and whether his PC has Hide Other ticked, with one picture it wrote
- The merges: Claude Code's permission check refused the loop's merge of pull request 202
- The team package, F140, made only when he writes make the team package, Q149

## Blockers and known bugs
- T6-R-WPFBOX: a caption less WPF window of Navisworks came up as the tool's window closed in both of set 09's runs
- F143-R2: the window answers only between rows, so one long render freezes it
- T5-R-OLDKEY: his 22.0 key reads an older copy after each restart, the writer UNKNOWN

## Where the long history is
- steps\history\loop.md: every turn, the form and the register table, its closed rows the record
- steps\history\log.md: one entry per fix, newest at the top
- Status of every item and finding: steps\tracker.csv, read as steps\tracker.md
- Questions and answers: steps\02_questions.md. The plan: steps\01_next.md and steps\fix-round.md
- Evidence outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\turn5 and turn6
