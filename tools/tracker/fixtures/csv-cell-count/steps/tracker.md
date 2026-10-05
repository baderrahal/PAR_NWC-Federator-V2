# The work tracker

Made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never by hand. Status lives in steps\tracker.csv only.

- By status, of 8 rows: open 0, in progress 2, in review 1, merged 2, proven by a run 1, waiting for Bader 1, dropped 1
- By wave: 1 3, 2a 2, beside the waves 1, none 2
- In progress now: F2 the second area, and 1 FR item
- Waits for Bader, 1 row: Q1

## Wave 1

| id | short title | area | class | status | PR | run that proved it | date of last change |
|---|---|---|---|---|---|---|---|
| FR-001 | first item | F1 | noise | merged | 12 | none | 2026-10-05 |
| FR-002 | second item, with a comma | F1 | noise | proven by a run | 13 | steps\runs\05 shows "it" | 2026-10-05 |
| F1 | the first area | F1 | fix | merged | 12 | none | 2026-10-05 |

## Wave 2a

| id | short title | area | class | status | PR | run that proved it | date of last change |
|---|---|---|---|---|---|---|---|
| FR-003 | third \| item | F2 | noise | in progress | none, branch fix-F2 | none | UNKNOWN |
| F2 | the second area | F2 | fix | in progress | none, branch fix-F2 | none | 2026-10-05 |

## beside the waves

| id | short title | area | class | status | PR | run that proved it | date of last change |
|---|---|---|---|---|---|---|---|
| FR-005 | fifth item | F3 | broken feature | in review | UNKNOWN | none | 2026-10-05 |

## none

| id | short title | area | class | status | PR | run that proved it | date of last change |
|---|---|---|---|---|---|---|---|
| Q1 | a question | F2 | question | waiting for Bader | none | none | 2026-10-04 |
| FR-006 | sixth item | none | UNKNOWN | dropped | none | none | 2026-10-05 |
