---
paths:
  - "steps/tracker.csv"
  - "steps/tracker.md"
  - "tools/tracker/**"
---

# Rules for the work tracker

Bader's message of 5 Oct 2026, Q129, built by F133. The scripts and the fixtures are described
in tools\tracker\README.md.

- Status lives in steps\tracker.csv only. steps\loop.md and steps\fix-round.md keep their
  narrative and point at the tracker for status
- Every pull request that changes an item's status updates its row in the same pull request:
  the status, the PR, the run that proved it and the date of the last change. A pull request
  writes the rows it finishes as merged with its own number, since a row reaches main only
  when its pull request merges. A pull request that adds an FR item to steps\fix-round.md
  adds its row
- One row per item: every FR item of steps\fix-round.md, every F area, each of Bader's
  requests and every question in steps\02_questions.md with no answer of his under it. The
  columns are id, short title, area, wave, class, status, PR, run that proved it and date of
  last change, in that order
- A status is one of open, in progress, in review, merged, proven by a run, waiting for
  Bader and dropped, and nothing else. In progress means work on it has begun, on a branch or
  in a measurement steps\loop.md records, in review that its pull request is open, merged that
  its fix is on main and no run since has proved it, proven by a run that a named run folder
  under steps\runs shows it, which goes in the run column
- Every value is read off the repo, never from memory: a DONE line in steps\01_next.md, a
  merge in git log of origin/main, a branch in git branch -r, a closing line in
  steps\fix-round.md, an answer in steps\02_questions.md. A value that cannot be read is
  UNKNOWN, which the check accepts in every column but the id and the status. No cell is
  empty
- No cell names the desktop folder of real files, since the paths wall refuses a file write
  and a command naming it, and the csv has to stay writable by the file tools. F108's title
  says the real files instead
- steps\tracker.md is made by tools\tracker\make-tracker.ps1 from the csv and never edited by
  hand. Run it after every change to the csv and commit both
- tools\tracker\check-tracker.ps1 runs in Actions on every pull request and refuses a csv
  that does not parse, an id twice, an UNKNOWN id, an empty cell, a status off the list, an
  FR item with no row and a tracker.md that is not what the maker makes, each naming its
  line. Every fault kind has a fixture under tools\tracker\fixtures that
  tools\tracker\prove-tracker.ps1 runs, and a new kind gets its fixture in the same pull
  request
- The rules live in tools\tracker\tracker-rules.ps1 alone, and the maker and the check read
  it with a dot. A second copy of the status list or the columns in code is a bug
