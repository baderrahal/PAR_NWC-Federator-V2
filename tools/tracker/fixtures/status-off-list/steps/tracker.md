# The work tracker

Made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never by hand. Status lives in steps\tracker.csv only.

- By status, of 19 rows: open 3, in progress 5, in review 3, merged 5, proven by a run 1, waiting for Bader 1, dropped 1
- By wave: 1 3, 2a 5, 2a and before any test run 1, before any test run 2, beside the waves 2, none 6
- In progress now: F2 the second area, F4 the fourth area, Q4-2 the second of two requests, and 2 FR items
- Waits for Bader, 1 row: Q1

## Wave 1

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-001 | first item | F1 | noise | merged | 12 | none | 2026-10-05 |
| FR-002 | second item, with a comma | F1 | noise | proven by a run | 13 | steps\runs\05 shows "it" | 2026-10-05 |
| F1 | the first area | F1 | fix | merged | 12 | none | 2026-10-05 |

## Wave 2a

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-003 | third \| item | F2 | noise | in progress | none, branch fix-F2 | none | UNKNOWN |
| F2 | the second area | F2 | fix | in progress | none, branch fix-F2 | none | 2026-10-05 |
| Q3 | a question answered on a branch | F2 | question | in review | UNKNOWN | none | 2026-10-05 |
| F5 | the fifth area | F5 | fix | open | none | none | 2026-10-05 |
| Q4-2 | the second of two requests | F2 | Bader's request | in progress | none, branch fix-F2 | none | 2026-10-05 |

## Wave 2a and before any test run

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q2 | a question Bader answered | F2, F4 | question | merged | 18 | none | 2026-10-05 |

## before any test run

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-008 | eighth item | F4 | Bader's request | in progress | 14, open | none | 2026-10-05 |
| F4 | the fourth area | F4 | fix | in progress | 14, open | none | 2026-10-05 |

## beside the waves

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-005 | fifth item | F3 | broken feature | in review | UNKNOWN | none | 2026-10-05 |
| F3 | the third area | F3 | fix | in review | UNKNOWN | none | 2026-10-05 |

## none

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q1 | a question | none | question | waiting for Bader | none | none | 2026-10-04 |
| FR-006 | sixth item | none | UNKNOWN | dropped | none | none | 2026-10-05 |
| FR-007 | seventh item closed before the waves | none | noise | merged | 9 | none | 2026-10-03 |
| F6 | the sixth area | F6 | fix | open | none | none | 2026-10-05 |
| Q4-1 | the first of two requests | none | Bader's request | merged | 19 | none | 2026-10-05 |
| Q5 | a request of one row | none | Bader's request | open | none | none | 2026-10-05 |
