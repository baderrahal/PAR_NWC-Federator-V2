# steps

What this folder is.

This folder holds the worker's notes on the repo.
It is written for a phone screen.
Short lines. One action per step. Every command in a code block.

## Files

- `00_analysis.md`  what the code does, what is wrong, what is missing
- `01_next.md`  the fixes, ranked, smallest and safest first
- `02_questions.md`  every question for Bader, numbered
- `log.md`  running log of what was done and what comes next

## How to read it

Newest file wins.
Files are numbered. A higher number was written later.
Where two files disagree, trust the higher number.
`log.md` is always the latest word on where things stand.

Start at `log.md`, then `01_next.md`.
Read `00_analysis.md` when a fix needs the background.
Answer `02_questions.md` in any order.

## Marks used

- `LOCAL MACHINE ONLY`  needs Navisworks on the machine. Cannot run in the container
- `CONTAINER`  runs in the container, no Navisworks needed
- `UNKNOWN`  the worker could not find out and did not guess
- `B1, B2`  a bug in `00_analysis.md` section 3
- `L1, L2`  a logic problem in section 4
- `M1, M2`  a missing piece in section 5
