# F103 part 1, the final reading, of fix attempt 3

The reading of a63c284 on 2026-09-30, after fix attempt 3, the last the house rule allows,
by a reviewer and two breakers, one on ownership and close, one on Bader's things. Each was
given steps\notes\f103-fix-list-3.md, asked to answer SAFE FOR ITEM 0 and SAFE FOR INSTALL
first, and to class every finding as one of the 19 not fixed, a new fault inside fix attempt
3, an old fault outside it, or polish. Line numbers are of a63c284 unless a commit is named.

WHAT THEY ANSWERED

- the reviewer and the breaker on ownership: SAFE FOR ITEM 0 yes, SAFE FOR INSTALL yes. The
  breaker on Bader's things: yes to both in its lens
- a finding classed as a new fault inside fix attempt 3: none, by all three. Some findings
  they class as polish are in code the attempt wrote, see the register rows below
- the reviewer read 18 of the 19 items fixed, items 1 to 13 and 15 to 19, and item 14
  UNKNOWN, because the words it names were not handed to it. Neither breaker found one of
  the 19 not fixed in its lens. The breaker on ownership could not check item 14, and the
  breaker on Bader's things did not name it
- the reviewer's verdict: APPROVE, on one condition, that the lead holds item 14 against
  the words it names

ITEM 14, HELD BY THE LEAD AGAINST THE WORDS IT NAMES

Item 14 is the words the reviewer of fix attempt 2 named in its findings 11 and 12, at
867697a. At a63c284, four of its six places are mended. By their numbers at 867697a they are
the clause of prove-run.ps1 22, the pairing at .claude\rules\loop.md 95, the split at 99 to
100, and the short line at tools\loop\README.md 149, which is line 155 at a63c284. Two are not:

- finding 11 at 867697a: "prove-run.ps1 23 says 'replaced by a line that throws', but the
  line only sets $err". At a63c284 prove-run.ps1 24 to 25 still say H17 runs a copy of
  run.ps1 "whose constructor line is replaced by a line that throws". The replacement at
  prove-run.ps1 1167 sets $err to a new exception and throws nothing
- the same sentence was named by the reviewer of fix attempt 1 too, at prove-run.ps1 21 of
  fcd981b, "The line at 1063 throws nothing", item 12 of steps\notes\f103-fix-list-2.md. So
  it has survived fix attempts 2 and 3
- also of finding 12, the list of harness runs in tools\loop\README.md names the runs after
  fix attempts 1 and 3 and still has no entry for run 10, the run after fix attempt 2

So item 14 is NOT FIXED, and by the rule at the head of fix list 3, F103 stops and goes to
Bader's form, steps\02_questions.md question 92.

WHAT OPTION A OF QUESTION 92 WOULD CHANGE, AND NOTHING ELSE

Nine entries, eleven places in six files. Entries 1 and 2 are the places of item 14 not
mended. Entries 3 to 9 are what the final reading names in the words, sentences that say more
than the code and one comma. Words only, and no change of logic. Most are comments and
documents, but entry 6 is the text of a REFUSED message install.ps1 prints and entry 8 the
label of a harness check, so each changes a line that runs. Every file touched gets a new
sha256: run.ps1 for its comment at 380, install.ps1, prove-run.ps1 and the probe. So
prove-run.ps1 and the copy of the probe's harness 4, which reads the probe by sha256, are run
again after the change, and a start on a63c284 differs from the merged run.ps1 only by that
comment, which the diff shows.

1. prove-run.ps1 24 to 25, "replaced by a line that throws", above
2. the list of harness runs in tools\loop\README.md, no entry for run 10, above
3. prove-run.ps1 20 to 21, "so check 6 and check 7 would each refuse it". For the Install
   call the second guard is the HEAD check, not check 7. The reviewer
4. tools\loop\README.md 149, "RunVerdict and InstallVerdict decide them". The exit codes at
   run.ps1 934, 936 and 966 are set in the main flow. The reviewer
5. build\install.ps1 113 to 114, tools\loop\README.md 102 to 104 and .claude\rules\loop.md
   68 to 70 say a failure after the move takes the new bundle out and puts the old one back,
   and a folder it cannot remove is named in the verdict of run.ps1 -Mode Install. Harness
   case ta2 shows the new bundle can stay at the load path with the old one still aside, and
   on that exit 1 run.ps1 936 names neither folder in the record. The reviewer, and the
   breaker on Bader's things
6. build\install.ps1 120 says the rename was refused "most likely by a Navisworks that is
   running", right after the Roamer read at 91 to 94 found none. What holds the file is
   UNKNOWN. The reviewer, and the breaker on Bader's things
7. tools\probes\probe-automation-start.ps1 96, "It reads no window of any other process",
   the claim item 16 corrected in nw-guard.ps1. The reviewer
8. prove-run.ps1 284, "never listed as the start's", the framing item 19 removed elsewhere.
   The reviewer
9. the comment at run.ps1 380 ends on a comma. The reviewer

Noticed by the claim-checker of these records, not by the readers: tools\loop\README.md at
a63c284 has three over-long lines, 126, 158 and 259, so the wrap item 14 named at 149 is
mended there and found elsewhere. Option A rewraps them too, which changes no word.

THE READERS' OTHER FINDINGS, NEW REGISTER ROWS IN steps\loop.md

The readers classed each as old or polish, and none as a new fault inside the attempt. Three
rest on code the attempt wrote: BundleLeftovers of item 6c in T3-G15, CloseAtEnd's text of
item 8 in T3-G16, and OwnerText of item 16 in T3-P2.

- T3-G13. run.ps1 903, 1014 and 1020 write an exception's text into the record unmasked, and
  that text holds the full path of a file of Bader's. Run's record, where 1014 and 1020
  write, is copied into steps\runs. Install's, where 903 writes, stays in the loop folder.
  The kind item 12 fixed, at lines it did not name. Neither real start's record holds such a
  line. The breaker on Bader's things calls it noise and not a new leak, because the tree at
  a63c284 already holds the user name in 13 files. Classed old by the reviewer and by that
  breaker
- T3-G14. A run.ps1 or install.ps1 killed or hung between the move aside and the removal
  leaves the load path empty or holding part of the new bundle, and Bader's old bundle whole
  at ParsonsNwcFederator.bundle.replaced-<time>, with no line saying so and nothing that
  puts it back. run.ps1 923 waits on install.ps1 with no limit. install.ps1 119 to 260.
  Classed old by the breaker on Bader's things
- T3-G15. Install's leftovers. A failed removal of the old bundle after every check passed
  prints one LEFT line and exits 0, which INSTALL.md 29 to 33 reads as success in a direct
  install. BundleLeftovers, run.ps1 784 to 789, names every .replaced- and .failed- folder,
  old ones too, as the add-in installed before, so one stale folder makes every later
  Install exit 5. A wrong stamp returns at 947 before a leftover is named at 948. A removal
  that stops part way leaves a part of the old bundle named in LEFT. The checks that gate
  the removal read references from the staging copy, and run.ps1 reads the stamp after the
  old bundle is gone. Classed polish by both breakers
- T3-G16. A close that fails twice. When the watchdog's Kill fails, CloseAtEnd writes that
  the process was CLOSED here even when its own Kill also failed, run.ps1 493. CloseOwn
  refuses any folder whose record holds a VERDICT line, run.ps1 687, so then only a person
  can end that Navisworks. That the verdict reads the watchdog's exit 4 with no word that
  the process still runs, run.ps1 464 to 469, is T3-G12. Classed polish, the text, and old,
  the refusal, by the breaker on ownership
- T3-P2. Polish. CloseOwn reads autosave-before.txt with no count or name check, run.ps1
  727. dotnet build-server shutdown at run.ps1 927 stops every build server of the account.
  An owner window destroyed mid read is written as a window of another process, with its
  handle, run.ps1 666 to 669. Item 8's harness case sets Forced by hand and never makes the
  watchdog's Kill fail. The probe closes outside the lock at 869 and 889. Classed polish or
  old by the two breakers, the probe's close left unclassed

Already registered and still standing, by the readers: T3-G6 to T3-G12 and T3-P. The
breaker on ownership's finding of a WMI stall right after the constructor returns is T3-G9.

THE PROOF ON a63c284

- harness run 12, %LOCALAPPDATA%\NwcFederatorLoop\turn3\f103\prove-run-out-12.txt, 14:46:02
  to 15:04:48 on 2026-09-30: 242 passed, 0 failed, 43 stand-ins each closed through its held
  handle, Get-Process Roamer 0 at the start and the end. The sha256 of run.ps1, nw-guard.ps1
  and prove-run.ps1 in its header equal those files as checked out at a63c284 in the F103
  worktree, whose git status reads clean, and each was last written before the harness
  started
- the copy of the probe's harness 4, turn3\f103\h4\h4-fix3-out.txt, 15:05:53 to 15:08:22, on
  the same nw-guard.ps1 by sha256, Get-Process Roamer 0 after its cleanup
- Actions run 36713756910 on a63c284. Its first attempt never ran a test: "The job was not
  started because it repeatedly failed to be acquired (5 attempts)". The lead ran it again,
  and its second attempt ran from 16:04:04 to 16:05:30, green, the Core tests 1720 passed,
  0 failed, 26 skipped of 1746, and the checks
- the two real starts with no window on 2026-09-30 ran on fix attempts 1 and 2, not on
  a63c284. Their records are steps\runs\00\item0 and steps\runs\01\item0 on fix-F103
