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
  by another gets one line saying which fix closed it and when. A closed section moves to
  steps\history\01_next-closed.md at the end of a round, and the order at the top names the open
  sections and the open areas that have none
- Every pull request that changes an item's status, a DONE line among them, updates the
  item's row in steps\tracker.csv in the same pull request, Bader's rule of 5 Oct 2026,
  Q129: the status, the PR, the run that proved it and the date of the last change, then
  runs tools\tracker\make-tracker.ps1 and commits both files. The row goes in the fix's one
  records commit, CLAUDE.md under Rounds, made once its pull request is open, so the row
  reads merged with the pull request's number before the merge and reaches main that way. A pull request that adds an FR item to steps\fix-round.md adds its row, and one
  that writes a new question or Bader's answer under one in steps\02_questions.md adds or
  sets the question's row, in the same way, and one that writes a request of his there, an
  item whose text starts From Bader, adds its row. Status lives in the tracker, and
  .claude\rules\tracker.md says how a row is written
- steps\02_questions.md holds every question put to Bader, numbered once and never
  renumbered, with his answer under it and the fix that carried it out. A new question
  goes after the last one
- When Bader answers a question, the same round writes the Answer: line, the row's status in steps\tracker.csv and the remade counts, and tells him in the tab which rows changed, his rule of 13 Oct 2026
- steps\03_bader_next.md is what Bader does on the local machine, numbered, one
  action per step, in the order that proves the most with one build, one install and
  one Navisworks session. It names which proof comes first and why. It is rewritten,
  not appended, when the proofs change
- steps\PROGRESS.md is the one page a session starts from, at most 60 lines, and
  steps\START.md the start prompt of under 20 lines Bader pastes into a new tab, his
  message of 6 Oct 2026, Q139. The lead alone writes the page's lines outside its counts
  and rewrites them, never appends, by .claude\rules\loop.md, and its counts are made by
  tools\tracker\make-tracker.ps1, never typed, which the pre-commit runs in any commit
  staging steps\tracker.csv, by .claude\rules\tracker.md
- steps\history holds loop.md and log.md, the long history, moved there whole by F139
  and never read at the start of a session, only for a line a session needs. Since F242,
  Bader's round of 19 Oct 2026 b, item 4, it also holds the closed items of steps\01_next.md,
  steps\fix-round.md, steps\03_bader_next.md and steps\lane-b.md, each in a file of the same
  name ending -closed, moved whole and in their order, nothing deleted, and the page as it
  stood before a rewrite that dropped a part of it. Each of those four files is kept under 100 KB:
  at the end of a round its closed items move there, the way .claude\rules\tracker.md says for an
  FR item, its first lines say where they went, and a file whose open items alone pass 100 KB is
  put to Bader in the round's form.
  steps\history\log.md holds one entry per fix, newest at the top, in the shape of the
  F26 entry: What was done, What remains, Known bugs, What comes next. Every entry says
  what was proved here, with the Core test counts before and after, and what waits
  for the local machine. A finding goes in steps\tracker.csv and not in an entry
- steps\logs holds run logs Bader sent back. They are evidence and are never edited,
  and a hook refuses the edit
- Plain words, no em dash, no semicolon in prose, no emoji, no code identifier where a
  sentence would do. Say UNKNOWN rather than filling a gap
