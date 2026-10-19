# Audit of the round of 19 Oct 2026 b, F239 to F242

Bader's new way of building, 19 Oct 2026, from the NWC Federator's lessons. Four items, no Navisworks run and no change to how the
tool behaves. Main was a85e946 at the start and 72e5842 after the last fix merged. Written by F242b at the end of the round.

## The round against its own rules

- Three or four fixes per round: four, F239 to F242, and F242b the end of F242, the audit, the QA, the form and the entry
- Fixes that touch different files side by side in worktrees, the same files in order: items 1, 2 and 3 were each built in a
  worktree of their own, wt-F239 in the main clone, wt-F240 and wt-F241, and opened their pull requests side by side. They did
  not touch different files, since each wrote a line of CLAUDE.md and each one's records wrote the same five steps files, so they
  merged one at a time, F240, F241, F239, each taking main in first. F241 changed .claude\agents\developer.md only after F240,
  which wrote the same folder, had merged
- Prove each fix on the smallest real case, one group, and one full real run at the end: no fix changed what the tool does, so
  no run was made, by the round's own words, no Navisworks run. Skipped and why: the full run, since nothing under src changed
- A round closes on the outputs: no output of the tool changed. What was opened and judged against what Bader asked is each
  file the round wrote, below, read by a reviewer and a claim-checker. The counts across Clash Detective, Excel, HTML and the
  viewpoints and the random picks by element ID were not made, since no run was made, and stand as the full run of 19 Oct left
  them, claude\audit-round-2026-10-19.md
- One records commit per fix: each fix had one records commit, the log entry, the DONE line, the tracker row and the page.
  Each also had one or two commits after it on the readers' findings, which touched those files again
- Questions in one form at the end: Q225, with Q224 written by F239 as item 1 asked
- A fix on its third attempt stops: no fix reached a third attempt. Each had its first reading and a fix of what it found,
  then one re-read
- One PR per fix, merged on green, branch deleted, no PR left open: 320, 321, 322 and 323 merged green, their branches deleted
  on GitHub and here, the worktrees removed, and 324, F242b's, is merged the same way as the last act of the round
- The writing rule: no em dash or semicolon in the lines the round wrote, read by grep and by the readers. The commits of the
  round, 23 of them, carry no generated-by or co-author line, read back off main

## Each item

| item | fix | pull request | merge on main | what it did |
|---|---|---|---|---|
| 2, each agent's model by its job | F240 | 320 | 09721c1 | five model lines changed, three already right, one line in CLAUDE.md, and the plan of the round |
| 3, the round rules in the repo | F241 | 321 | cb01f32 | Rounds in CLAUDE.md, the older lines it replaces in CLAUDE.md, loop.md, steps.md and developer.md pointed at it, ai-max version 3 copied in, 39 files |
| 1, one rules file | F239 | 322 | b29848d | docs\rules.md, 105 rules, Q224, the copies in CLAUDE.md pointed at it |
| 4, records Bader can read | F242 | 323 | 72e5842 | the closed items of four files moved to steps\history, the tracker check taught them, the page in five parts |

### The agents' model lines, F240

| file | before | after |
|---|---|---|
| breaker.md | sonnet | opus |
| claim-checker.md | sonnet | sonnet |
| developer.md | inherit | opus |
| log-reader.md | sonnet | sonnet |
| prober.md | inherit | sonnet |
| reviewer.md | inherit | opus |
| runner.md | inherit | haiku |
| writer.md | sonnet | sonnet |

They load when a session starts, so they take effect from Bader's next session.

### The ai-max skill, F241

The repo held no copy at .claude\skills\ai-max before the round, in no commit, so there was no version 2 copy to replace. The
installed skill reads Version 3, 19 Oct 2026, and its 39 files were read whole, copied as they are and read back equal by sha256.

### docs\rules.md, F239

105 rules in eleven areas. 82 name a test, 13 name a test that proves part of the rule, written Test, in part, and 10 say NO
TEST: MA2, MA6, MA9, CT15, CT22, PI10, OF7, RE6, PR7 and PR8, put to Bader as Q224. Four lines say CONFLICT where a chat note and
an answer disagree: CT15, VP2, VP4 and SP4, put to him in the form.

## File sizes before and after

Bytes of the working copies, read with wc -c, before at main a85e946 or b29848d and after at 72e5842:

| file | before | after |
|---|---|---|
| steps\01_next.md | 602,344 | 27,488 |
| steps\fix-round.md | 379,484 | 144,071 |
| steps\03_bader_next.md | 364,612 | 47,652 |
| steps\lane-b.md | 95,023 | 51,844 |
| steps\history\01_next-closed.md | none | 576,646 |
| steps\history\fix-round-closed.md | none | 235,733 |
| steps\history\03_bader_next-closed.md | none | 317,292 |
| steps\history\lane-b-closed.md | none | 43,672 |
| steps\PROGRESS.md | 4,761, 60 lines | 4,042, 50 lines |
| docs\rules.md | none | 25,041 |
| CLAUDE.md | 13,822 | 15,066 |

fix-round.md stays over 100 KB, its 77 open items holding 104,165 bytes alone, part 6 of the form.

## What was proved

- Core tests 3451 passed, 0 failed, 0 skipped at the start and after every commit of the round, run by the pre-commit
- prove-tracker all 84 cases right, the 76 before and eight new, prove-progress all 26, check-tracker and check-progress clean
- No file under src or tests changed, so the add-in was not built and nothing of the tool was run

## Bugs found by the round

- The round's own three OPEN lines of the page left out last run, which the Stop gate reads, and F239's line carried a time
  not read off the clock. F242 put both right
- printf turned a backslash in records into a control byte twice and a carriage return into a commit message once, each caught
  and put right before it was pushed or merged, the message by an amend before its push
- The first move of F242 judged closed too widely and moved open sections, caught by its reviewer and made again

## The QA of main after the last merge

Read only, by an agent on main 72e5842 once F242 merged, git status printing nothing before and after. No file under src or tests
changed from 6faff29 to 72e5842, so every contradiction in the code stands as it was.

What it ran: the Core tests, 3451 passed, 0 failed, 0 skipped, with the one Explicit test WriteTheCorrectedFile skipped by
NUnit's own rule. check-tracker clean at 835 rows, check-progress clean at 50 lines, prove-tracker all 84 right, prove-progress
all 26 right. No pull request open. docs\rules.md read as 105 rules, 82 Test, 13 Test in part, 10 NO TEST and 4 CONFLICT lines,
all 93 distinct tests named there plain tests of their classes. One em dash in a tracked text file, a literal in a test that
asserts there is none. Semicolons in prose in open files only at steps\01_next.md lines 43 and 61 and steps\02_questions.md lines
328 and 356, the rest in history files. No control byte in any text file. The ai-max copy equal to its source, carriage returns
aside. Not run: the solution build, the probes, prove-hooks and any Navisworks.

Branches: fix-F233, fix-F234 and fix-F235 were merged into main and still on GitHub, and one local agent branch was merged. Each
was read as merged by git merge-base and deleted by F242b, Bader's answer to Q206. Left, as Q206 keeps them: fix-F109, fix-F145,
fix-F149, pc-test-2026-10-13, round-close-penetrations and round-scale.

### The 40 contradictions of 19 Oct, read again

Of the 40 listed open on 19 Oct, 50 is closed by F242's new order of 01_next.md, and 23 by Bader's answer to Q210, which kept the
members only tests read as internal with their tests. The rows F238 fixed as text read closed again: 15, 16 with m12, 20 with 49,
27, 33, 34, 36, 39, 40, 41, 51 and 53.

Standing, the code ones first: 8 and 31, typed numbers, now also against PR6 of docs\rules.md. 43 and 44, words typed in the
window. 46, a page with no clash table read as a nought. 37, the open file run, now also against PR2. 38, 56 and m13, public
members read only by tests. 1, 5, 6, 7, 9, 11, 26, 45, 54, m20 and m24 as listed on 19 Oct. In the words a person reads: 3, 13,
22, 24, 28, 29, 30, 42, 47, 48, 52, 55, m1, m17, now also against VP2, m22 and m23, now also against GR2 and CT3, and 32, 35 and
m21 in part.

### New, 57 to 73

57. CLAUDE.md under Rounds runs fixes on the same files in order, and the round built items 1, 2 and 3 side by side with pull
    requests 320 and 321 open together, since all three wrote CLAUDE.md and the records. They merged in order. Records
58. One records commit per fix: each fix had one, and then one or two more commits on the readers' findings that touched the
    records again, F239's 2a58e44, F241's 4ee9a5e and F242's c37ccc7, 5fdbe3a and ef8d9d7. Records
59. Questions in one form at the end: Q224 was written by F239, since item 1 asked for it, and the form is Q225. Records
60. steps.md keeps each of the four files under 100 KB, and fix-round.md is 144,071 bytes, part 6 of the form. Records
61. docs\rules.md RE6 reads NO TEST, while GapRuleTests.TheFourDeletedPropertiesAreNotOnTheItem and two tests of
    ClashReportXmlTests assert the properties are not read. Words
62. docs\rules.md GR3 names a test that asserts a group is or is not of one discipline and reaches neither federated nor clashed,
    so it is a test in part. Words
63. docs\rules.md cites CLAUDE.md as the source of GR1, LO2, PR6 and PR7, while CLAUDE.md points at those lines, so the source
    reads round in a circle. Words
64. ByDesignTally.MovedLines and PenetrationTally.MovedLines are public and read only by tests, in no earlier list. Code
65. ViewsTreeCheck.PairCarriesSizeFolder, internal, is called by nothing, a leftover of the pair level VP2 removed. Code
66. 46 internal members of Core are read only by tests, 19 of them in no question or audit, and whether the rule worded for
    public members covers them is UNKNOWN. Code
67. Six counts with no setting, against PR6: RunLog KeptOfARepeat 5, NwfPage FilesListed 12, OutsideTheToolFolder NamesListed
    50, ViewTimings Slowest 5, the runner's StatusAttempts 5 and SavedViewpoints HomeWalkBound 64. Whether each shapes a run is
    UNKNOWN. Code
68. Pointers into sections that moved or no longer read as written: tools\loop\README.md naming 01_next.md F104,
    steps\01_next.md naming FR-035 to FR-041 of fix-round.md, .claude\rules\tracker.md naming steps 228-233, 346-352 and 364,
    the property probe's Step 364, check-imports.sh's step 8 and FederationEngine.cs's step 10 of 03_bader_next.md. Some were
    stale before F242. Words and records
69. 01_next.md's order names F242 as this round while its section holds its DONE line. Records
70. 01_next.md's head still says F1 to F4 go in one pull request and the add-in does not compile in the container. Words
71. README.md names CLAUDE.md for the rules and not docs\rules.md, and repeats the old description of row 55. Words
72. Merged branches left on GitHub, fix-F233 to fix-F235, closed by F242b's deletion. State
73. docs\rules.md PR7 holds the 45 minutes as a part of done, while LO3 says it is not judged until the target is set. Words

### Other points

- The ai-max copy is a live project skill beside Bader's own. Where it differs, CLAUDE.md holds: CLAUDE.md is 239 lines, the rules
  files over 200, two pull requests open at once, a run sheet of more than 30 steps, a researcher agent the repo has none of, and
  START.md not naming docs\rules.md. Part 8 of the form
- loop.md counts each question as a part of the page, so the page reads four parts under its counts by that reading
- F242b is the end of F242 and not a fifth fix, its branch the fifth of the round
- The F118 row's title still names FR-036 in review on fix-F118c, while FR-036's own row reads merged in 210. The check does not
  read a title
- The QA's sweep of members matched names, not symbols, so a name shared with another can hide a dead member, and it is no build
