---
paths:
  - "steps/tracker.csv"
  - "steps/tracker.md"
  - "tools/tracker/**"
---

# Rules for the work tracker

Bader's message of 5 Oct 2026, Q129, built by F133. The scripts and the fixtures are described
in tools\tracker\README.md. The rule that every pull request changing an item's status
updates its row in the same pull request lives in .claude\rules\steps.md, beside the DONE line
rule, since steps.md loads for every file under steps, the DONE lines and the csv among them.

- Status lives in steps\tracker.csv only. steps\loop.md and steps\fix-round.md keep their
  narrative and point at the tracker for status
- One row per item: every FR item of steps\fix-round.md, every F area, each of Bader's
  requests and every question in steps\02_questions.md waiting for his answer, whose row
  stays once he answers. The columns are id, short title, area, wave, class, status, PR, run
  that proved it and date of last change, in that order
- A question's row, class question and id Q followed by its number, reads waiting for Bader
  until his answer is under it in steps\02_questions.md. Once the record of his answer is on
  main it reads merged, with the number of the pull request that put the record there, PR 118
  for his answers of 2026-10-05, and that date. On the branch that records the answer it may
  read in review. The check refuses a question row whose question's Answer line holds his
  answer and that reads anything else, and a question row naming no question of the file. A
  request of his, class Bader's request, follows the work it asked for and not his answer
- A status is one of open, in progress, in review, merged, proven by a run, waiting for
  Bader and dropped, and nothing else. In progress means work on it has begun, on a branch or
  in a measurement steps\loop.md records, in review that its branch is finished and waits for
  its pull request to merge, merged that its fix is on main and no run since has proved it,
  proven by a run that a named run folder under steps\runs shows the code that merged, which
  goes in the run column. A run of an earlier pass of the same fix does not prove what merged,
  and the run column says which pass it showed
- Every value is read off the repo, never from memory: a DONE line in steps\01_next.md, a
  merge in git log of origin/main, a branch in git branch -r, a closing line in
  steps\fix-round.md, an answer in steps\02_questions.md. A value that cannot be read is
  UNKNOWN, which the check accepts in every column but the id and the status. No cell is
  empty or blank, and none holds a line break
- An FR row's class, area and wave are what steps\fix-round.md gives: the class the first
  words of the item's Class line up to a comma, and the area and the wave those of the line of
  its waves section that places the item. An area line under a wave line places the items of
  the first sentence after its colon, at its part or its wave. A stage line, such as Before any
  test run or Beside the waves, places each F<n> <title>, FR-<n> pair of the first sentence
  after its colon, with the stage's words as the wave, its first letter small. Both are none
  when no line places the item, or when a line names it after Closed already. The check
  compares them, so a pull request that moves an item between areas or waves changes its row
  too
- Every line of the waves section is in a shape tracker-rules.ps1 lists. A line in no such
  shape, an area line with no wave line above it, an FR id named where the reader places no
  item, and an item placed in two areas or at two waves are faults. So a new kind of line is
  refused until the reader knows it, and no item reads as none in silence
- No cell names the desktop folder of real files, since the paths wall refuses a file write
  and a command naming it, and the csv has to stay writable by the file tools. F108's title
  says the real files instead
- steps\tracker.md is made by tools\tracker\make-tracker.ps1 from the csv and never edited by
  hand. Run it after every change to the csv and commit both
- tools\tracker\check-tracker.ps1 runs in Actions on every pull request and refuses each
  fault its header lists, the one list of them, each naming its line. It exits 2 when the
  csv, fix-round.md or 02_questions.md is not there. Every fault kind and every exit of 2 has
  a fixture under tools\tracker\fixtures that tools\tracker\prove-tracker.ps1 runs, and a new
  kind gets its fixture and its line in that header in the same pull request
- The rules live in tools\tracker\tracker-rules.ps1 alone, and the maker and the check read
  it with a dot. A second copy of the status list or the columns in the maker or the check is
  a bug. prove-tracker.ps1 writes its expected lines out in full, the list and the header
  among them, since a proof asserts the text a person reads
