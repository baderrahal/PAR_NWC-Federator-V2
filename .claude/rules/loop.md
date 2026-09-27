---
paths:
  - "tools/loop/**"
  - "steps/runs/**"
  - "steps/loop.md"
  - ".claude/agents/**"
  - ".claude/hooks/**"
---

# Rules for the loop

The loop builds the add-in on Bader's machine, installs it, runs it on a copy of real NWC
files, reads what the run did, fixes what it got wrong one pull request at a time, and
proves every fix with another run. It started on 2026-09-27, the first session that ran
where Navisworks Manage 2025 is installed. A rule with no run behind it is not a feature,
and nothing is fixed until a real run on real files shows it fixed.

## Where the state lives

- steps\loop.md holds the loop state and is read FIRST by every session. Its first line
  after the title is STATE followed by one word: OPEN, WAITING, RESTART or CLOSED. It
  also holds the turn number, the main commit, the next action, the register, the form,
  and per turn the runs, the findings, what was fixed, what is still open, every program
  started and every file written outside the repo. A new session carries on from it
  alone. A finished phase is never done again
- The lead alone writes steps\loop.md and steps\log.md
- steps\runs\NN is one run set, 00 the baseline. Each run of the set has its own folder
  holding the text log, the tsv, a listing of every output file with its size, and a
  read-out of every workbook. A file over 20 MB is committed zipped and the turn says so
- .claude\hooks\loop-gate.sh is the Stop hook. ONLY OPEN SENDS A SESSION BACK, and only
  once per session per change to steps\loop.md. CLOSED, WAITING, RESTART, any other word,
  no STATE line and no file all let the stop through, and so does a note it cannot write,
  because a gate that cannot tell must never be what traps a session
- While the loop runs, a finding lives in the register in steps\loop.md. A finding that
  becomes a fix also gets its section and its DONE line in steps\01_next.md in the pull
  request that merges it, so the two never disagree about what is done

## Guards that never bend

- NM Fed on Bader's desktop, every live project folder and everything under an ACC
  Desktop Connector path is read only. The desktop is found with
  [Environment]::GetFolderPath('Desktop') because it sits under OneDrive. Only
  tools\loop\prepare-copy.ps1 reads NM Fed, and it finds the folder itself, so NO command
  ever names NM Fed and the wall in .claude\hooks\refuse-protected-paths.sh refuses every
  file write under it or an ACC folder and every command that names either
- THAT WALL READS WORDS AND IS NOT A SANDBOX. It catches the name however a command
  naturally spells it, the short name NMFED~1 included, and it cannot catch a script that
  builds the path at run time. So no script but prepare-copy.ps1 builds that path, and
  prepare-copy.ps1 writes only into the work folder and a listing inside the repo
- A commit message and a pull request body go in a file, git commit -F and gh pr create
  --body-file, never on the command line
- Installing replaces Bader's installed add-in, which the loop exists to do. So the
  installed bundle is copied into %LOCALAPPDATA%\NwcFederatorLoop\bundle-backup before the
  first install of a session, and nothing is installed while a Navisworks the loop did not
  start is running, because it may hold the bundle's DLLs and install.ps1 deletes the
  bundle before it copies the new one
- Every run works on the copy under %LOCALAPPDATA%\NwcFederatorLoop\source and every
  output of every run goes under %LOCALAPPDATA%\NwcFederatorLoop
- Nothing of Bader's is deleted or overwritten, bar the installed add-in, which is backed
  up first as the install rule above says. The only folder the loop deletes from is
  %LOCALAPPDATA%\NwcFederatorLoop
- Before the first run, %LOCALAPPDATA%\ParsonsNwcFederator\logs is copied into the work
  folder as a backup. Loop runs write their logs inside the work folder, so the thirty
  logs the tool keeps never push one of his out. Any choice the tool remembers between
  runs is read before the loop and put back after it
- Close what you open. Every Navisworks the loop starts is closed by its own process id.
  A Navisworks the loop did not start is never closed
- No NWC, NWF, NWD, workbook or picture is ever committed
- samples and steps\logs are never touched

## How a finding is worked

1. Measure before writing. No Navisworks member, property name, file layout or behaviour
   is assumed. It is measured and appended to docs\history\scan.md with the date
2. Root cause before fix, naming the run line or the test that shows the fault and the
   file and line that cause it
3. A Core test that fails before the fix and passes after, or for a fault only Navisworks
   shows, a run that shows it before and not after
4. One finding, one branch fix-F<n>, one pull request. Nothing rides along, except
   steps\loop.md and the steps\runs folders the pull request relies on
5. No member without a caller in src, no second copy of logic, no catch that swallows an
   error, no Navisworks type in Federator.Core, .NET Framework 4.8 and C# 7.3
6. Never weaken, skip or delete a test to get green. Never change a Look for line to
   match wrong output
7. The one who wrote a change never reviews it. The reviewer and the breaker read every
   change before its pull request, and the claim-checker reads every pull request body
   and every turn entry before it is final
8. A finding that survives three fix attempts stops, with what was tried and what each
   run showed, and moves to the form
9. Never report a check that did not run in this session
10. The writing rule at the end of CLAUDE.md holds everywhere
11. Every pull request is merged by the lead once Actions is green, watched with
    gh pr checks <number> --watch, and its branch deleted. Never merge red. Never stop
    with a pull request open. The add-in is built after every add-in change and the
    counts pasted in the body. A turn that changes no code ends with its own record pull
    request, so main is never behind the loop

## The run set

The baseline and the close run all five. Between them each turn runs what its fixes touch,
plus one first run of the whole folder.

1. First run. Empty output folders, the clash XML, every group ticked
2. Weekly run. The same folders again with no XML. Sets count as present, never created
3. A file gone. One NWC of a group with two or more disciplines removed from the copy.
   That group rebuilds and loses nothing it carried
4. A file back. The same NWC copied back from NM Fed. That group rebuilds with it added
5. Open document. One NWF from run 1 opened and the tool run on it. Then an NWD opened the
   same way, and the run refuses with its reason

A run is hung only when its log has not grown AND its Navisworks has used no processor
time for five minutes. Then the last lines are saved, that Navisworks is closed by its
process id, and the hang is a finding. Any dialog Navisworks raises during a run is a
finding with its text.

## The team

.claude\agents holds eight: prober, runner, log-reader, developer, reviewer, breaker,
claim-checker and writer. None of them is given the Agent tool, so none starts another,
and the lead calls each and hands it what it needs. The current sub-agents page says a
subagent CAN start others when its tools line names Agent, read on 2026-09-27, so that
is a choice here and not a limit. An agent that must not change anything has no shell,
because a scoped shell grant has been measured failing open, and anything that must
never happen is a hook.

Agents written during a session do NOT load in it. Measured on 2026-09-27: the eight were
written and log-reader was called in the same session, and the answer listed only the
built in agent types. Hooks do load mid session, the file watcher picks them up. So a
session that adds an agent ends at STATE RESTART.
