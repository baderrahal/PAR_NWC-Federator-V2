# steps

What this folder is.

This folder holds the worker's notes on the repo.
It is written for a phone screen.
Short lines. One action per step. Every command in a code block.

## Files

- `00_analysis.md`  what the code does, what is wrong, what is missing. History
- `01_next.md`  the fixes, numbered F, in the order they are worked
- `02_questions.md`  every question for Bader, numbered, with his answer under it
- `03_bader_next.md`  what Bader does on his machine, numbered, one action per step
- `04_audit.md`  the audit of 2026-09-12, what it found and which fix carries it
- `log.md`  one entry per fix, newest at the top
- `logs/`  the run logs Bader sends back. Evidence, never edited

## How to read it

Newest file wins.
Files are numbered. A higher number was written later.
Where two files disagree, trust the higher number.
`log.md` is always the latest word on where things stand.

Start at `log.md`, then `01_next.md`.
Read `03_bader_next.md` when you are at the machine with Navisworks on it.
Read `00_analysis.md` or `04_audit.md` when a fix needs the background.
Answer `02_questions.md` in any order.

## Marks used

- `LOCAL MACHINE ONLY`  needs Navisworks on the machine. Cannot run in the container
- `CONTAINER`  runs in the container, no Navisworks needed
- `UNKNOWN`  the worker could not find out and did not guess
- `B1, B2`  a bug in `00_analysis.md` section 3
- `L1, L2`  a logic problem in section 4
- `M1, M2`  a missing piece in section 5
- `F1, F2`  a fix in `01_next.md`
- `Q1, Q2`  a question in `02_questions.md`
