---
paths:
  - "steps/**"
---

# Rules for steps

- steps\00_analysis.md is the audit that started the plan and is history. A finding
  in it that has closed is marked closed there, with the fix that closed it
- steps\01_next.md is the plan. One section per fix, F followed by a number, with the
  order at the top. A fix that is done gets one DONE line with the date and what was
  proved, under its own section and never under the section after it. A fix absorbed
  by another gets one line saying which fix closed it and when
- Every pull request that changes an item's status, a DONE line among them, updates the
  item's row in steps\tracker.csv in the same pull request, Bader's rule of 5 Oct 2026,
  Q129: the status, the PR, the run that proved it and the date of the last change, then
  runs tools\tracker\make-tracker.ps1 and commits both files. A branch writes the rows it
  finishes as in review with PR UNKNOWN, since the number is not known before the pull
  request opens. The lead sets them to merged with the pull request's number on the branch,
  and runs make-tracker.ps1, before it merges, so a row reaches main as merged with its
  number. A pull request that adds an FR item to steps\fix-round.md adds its row. Status
  lives in the tracker, and .claude\rules\tracker.md says how a row is written
- steps\02_questions.md holds every question put to Bader, numbered once and never
  renumbered, with his answer under it and the fix that carried it out. A new question
  goes after the last one
- steps\03_bader_next.md is what Bader does on the local machine, numbered, one
  action per step, in the order that proves the most with one build, one install and
  one Navisworks session. It names which proof comes first and why. It is rewritten,
  not appended, when the proofs change
- steps\log.md holds one entry per fix, newest at the top, in the shape of the F26
  entry: What was done, What remains, Known bugs, What comes next. Every entry says
  what was proved here, with the Core test counts before and after, and what waits
  for the local machine
- steps\logs holds run logs Bader sent back. They are evidence and are never edited,
  and a hook refuses the edit
- Plain words, no em dash, no semicolon in prose, no emoji, no code identifier where a
  sentence would do. Say UNKNOWN rather than filling a gap
