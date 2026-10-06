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
- The columns are Bader's words, in his order: id, short title, area, wave, class, status, PR,
  the run that proved it, and the date of the last change. The date is the day the row's
  status, PR or run last changed
- One row per item, Bader's four kinds: every FR item of steps\fix-round.md, every F area,
  each of his requests, and every question in steps\02_questions.md waiting for his answer,
  whose row stays once he answers. A question he answered before any row was written for it
  has none, as Q136, asked by PR 123 and answered by PR 124 before the tracker reached main,
  and as every question answered before the tracker began. So of the questions the tracker
  holds only those waiting for him and those that once had a row. The check refuses an FR
  item, an F area that an area line or a stage line of the waves section of fix-round.md
  names, a request of his and a question with no answer when no row carries its id written
  exactly. An F area that no such line names is not read by the check, so a reader gives it
  its row by hand: every F section of steps\01_next.md, and an F number given to work
  elsewhere, such as F101 in the turn 3 plan of steps\log.md and F110 and F111 in
  steps\loop.md and the turn 5 plan
- A request of Bader's is an item of 02_questions.md whose text starts From Bader, as he
  wrote it, not a question put to him but a fault he found, a rule, an instruction or his
  decisions, Q81, Q93 to Q98, Q112, Q114, Q129, Q130 and Q132 today. Its row is class Bader's
  request, id Q and its number, or Q, its number, a dash and a number when one item holds
  several, as Q97-1 and Q97-2 and Q112-1 to Q112-5, and it follows the work it asked for and not
  his answer under it. An FR row may read class Bader's request too, since its class is the one
  fix-round.md gives, and the check does not read it as a request
- A question whose Answer line holds Bader's words is not waiting for him, even when those
  words hand the choice back as work, as Q24, Q26, Q108 and Q109 do, his find the mistake first,
  do not pick an answer. Such a question has no row of its own, since the work his answer asks
  is an FR item whose row carries its status, FR-160, FR-172, FR-136 and FR-161. When the find
  puts a new choice to him, that is a new question with its own row
- A question's row, class question and id Q followed by its number, reads waiting for Bader
  while every Answer line under it is empty or there is none. Once the record of his answer is
  on main it reads merged, with the number of the pull request whose merge put that record on
  main, read by git off origin/main, and that date. A question he answered twice reads the pull
  request whose merge put the later answer on main and that merge's date, since that is the
  row's last change, as Q128 reads 121, his leave to remove the three autosaves, where his A
  reached main with 118. On the branch that records the answer it may read in review. The
  check refuses a question row that reads otherwise either way, and a question row naming no
  question of the file or naming a request. It reads that the PR cell is a number and not
  which merge put the answer there, which a reader reads by git
- A question row's area and wave are read off fix-round.md, one rule for every question: the
  FR items whose section names it, Q and its number or a range Q<a> to Q<b>, give their areas
  in file order joined by a comma and their waves joined by and, leaving out an item in no
  area. With no such item both read none. A line outside an item does not count, such as the
  Source ids left out naming Q35 to Q40, so those six read none alike. The check compares them
- A status is one of open, in progress, in review, merged, proven by a run, waiting for
  Bader and dropped, and nothing else. In progress means work on it has begun, on a branch or
  in a measurement steps\loop.md records, in review that its branch is finished and waits for
  its pull request to merge, merged that its fix is on main and no run since has proved it,
  proven by a run that a named run folder under steps\runs shows the code that merged, which
  goes in the run column. A run of an earlier pass of the same fix does not prove what merged,
  and neither does a run that showed part of the work, as set 03 showed F106's item 1 alone.
  The run column holds only the run that proved the row, so a row with any other status reads
  none there, and the check refuses anything else, UNKNOWN included. What a run showed short
  of proving a row is told in steps\log.md and steps\loop.md
- Every value is read off the repo, never from memory: a DONE line in steps\01_next.md, a
  merge in git log of origin/main, a branch in git branch -r, a closing line in
  steps\fix-round.md, an answer in steps\02_questions.md. A PR cell written merge and a hash is
  a merge on origin/main's first parent with two parents, read by git. A value that cannot be
  read is UNKNOWN, which the check accepts in every column but the id, the status and the run
  column of a row not proven by a run. No cell is empty or blank, and none holds a line break
- An FR row's class, area and wave are what steps\fix-round.md gives: the class the first
  words of the item's Class line up to a comma, and the area and the wave those of the line of
  its waves section that places the item. An area line under a wave line places its area at
  its part or its wave, and the items of the first sentence after its colon, and may place no
  item. A stage line, such as Before any test run or Beside the waves, places each F<n> <title>,
  FR-<n> pair of the first sentence after its colon, with the stage's words as the wave, its
  first letter small, and any other F<n> of that sentence is named and not placed. Both are
  none when no line places the item, or when a line names it after Closed already. The row of
  an F area such a line places reads its own id as its area and the wave it is placed at. The
  check compares them, so a pull request that moves an item or an area between areas or waves
  changes its row too
- Every line of the waves section is in a shape tracker-rules.ps1 lists. A line of prose,
  starting with a letter after a blank line or another line of prose, is narrative and is not
  read for ids, so an FR id or an F area named there places nothing and is not checked. A line
  in no listed shape, a line starting with a letter right under a wave, area or stage line,
  which Markdown joins to that line, an area line with no wave line above it, an FR id named
  in a wave, area or stage line where the reader places no item, an item placed in two areas
  or at two waves, and an area placed at two waves are faults. So a new kind of line is refused
  until the reader knows it, and no item named on a line the reader reads is left as none in
  silence
- An item of 02_questions.md starts at the margin as its number, a full stop and a space. A
  line that looks like the start of an item in another shape, such as 134) or Q134. or **134.**
  or 134. indented by up to three spaces, and a second item of one number are faults, so an
  item the reader cannot see never reads as nothing there
- No cell names the desktop folder of real files, since the paths wall refuses a file write
  and a command naming it, and the csv has to stay writable by the file tools. F108's title
  says the real files instead
- steps\tracker.md is made by tools\tracker\make-tracker.ps1 from the csv and never edited by
  hand. Run it after every change to the csv and commit both
- tools\tracker\check-tracker.ps1 runs in Actions on every pull request and refuses each
  fault its header lists, the one list of them, each naming its line. It exits 2 when the
  csv, fix-round.md or 02_questions.md is not there, and 1 for any fault, a tracker.md that is
  not there among them. Every fault kind and every exit of 2 has a fixture under
  tools\tracker\fixtures that tools\tracker\prove-tracker.ps1 runs, and a new kind gets its
  fixture and its line in that header in the same pull request
- The rules live in tools\tracker\tracker-rules.ps1 alone, and the maker and the check read
  it with a dot. A second copy of the status list or the columns in the maker or the check is
  a bug. prove-tracker.ps1 writes its expected lines out in full, the list and the header
  among them, since a proof asserts the text a person reads
