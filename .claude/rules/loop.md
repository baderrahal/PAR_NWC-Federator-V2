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
  read-out of every workbook. A file over 20 MB is never committed, zipped or not. It stays
  in its run folder under %LOCALAPPDATA%\NwcFederatorLoop and the turn names it with its
  size and sha256, Bader's answer Q90 A
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
  file write under it or an ACC folder, and every command naming NM Fed or ACCDocs, the
  Desktop Connector folder on this machine. DC\Autodesk Docs and DC\BIM 360, the older
  names, are caught only written plainly after DC
- THAT WALL READS WORDS AND IS NOT A SANDBOX. It catches the name however a command
  naturally spells it, split by quotes, escaped or as the short name NMFED~1, and it
  cannot catch a script that builds the path at run time. So no script but
  prepare-copy.ps1 builds that path, and prepare-copy.ps1 writes only into the work
  folder and a listing under steps\runs
- THE WALLS PROTECT THEMSELVES FROM A FILE TOOL. No file tool may change .claude\hooks or
  .claude\settings.json, nor samples, steps\logs, steps\runs or bundle. A command can,
  which is how a proved change is copied in, so this stops a slip and not a command. A
  change to a wall is written into a folder of its own, proved there with
  tools\loop\prove-hooks.sh, read by a breaker, and only then copied in by a command,
  and that folder is removed once the copy is in, so each hook lives in one place
- A commit message and a pull request body go in a file, git commit -F and gh pr create
  --body-file, never on the command line, and a title never names NM Fed. git and gh go
  through the Bash tool, where the allow list names them
- Installing replaces Bader's installed add-in, which the loop exists to do. So the
  installed bundle is copied into %LOCALAPPDATA%\NwcFederatorLoop\bundle-backup before the
  first install of a session, and nothing is installed while a Navisworks the loop did not
  start is running, because it may hold the bundle's DLLs. build\install.ps1 moves the
  installed bundle aside by one rename, which fails whole while a file in it is held, copies
  the new one in and checks it, and removes the one moved aside only once every check has
  passed. On a failure after the move the new one is taken out and the old one put back,
  and where each is is printed. A folder it cannot remove is named in the verdict of
  run.ps1 -Mode Install
- Every run works on a copy under %LOCALAPPDATA%\NwcFederatorLoop. Since F108 each run set
  has a fresh one at runs\NN\NMFed, made by tools\loop\prepare-copy.ps1 -Set NN in Bader's
  own folder shape, NWC\<community> into the NWF, NWD and Clash Report folders of the same
  name. The loop named it NMFed because the wall refuses any command naming NM Fed. Every
  output of a run goes under %LOCALAPPDATA%\NwcFederatorLoop, bar two kinds. The evidence
  run.ps1 copies into steps\runs of the clone it runs from, masking only the remembered
  folders block of the tool's log, which the lead masks whole with mask-evidence.ps1 before
  any commit, as the rule below says. And what the tool and its
  Navisworks write where the loop cannot point them: the tool's own log and tsv in his logs
  folder, autosaves in his AutoSave folder and Navisworks's own settings, each kept by a rule
  below, and whatever else changes outside the loop folder while the start runs, such as the
  files of Autodesk's licensing and analytics, which run.ps1 lists as M5, by any program, and
  leaves as they are. Nothing is ever written into NM Fed
- Nothing of Bader's is deleted or overwritten, bar four things. The installed add-in,
  backed up first as the install rule above says, whose files an install replaces or
  removes. His Navisworks settings, put back to what the backup holds under the settings
  rule below, which can remove a value or key the loop's own Navisworks added. His oldest
  run logs, which the tool itself prunes when a loop run opens its window, each held in
  logs-backup by sha256 and put back after the loop, Bader's answer Q82. And his AutoSave
  folder, where the loop removes the autosaves its runs added and puts back from the backup
  any of his a run changed, Q86. Outside %LOCALAPPDATA%\NwcFederatorLoop the loop deletes or
  overwrites nothing else of his, bar its own logs and tsv files, which it takes out of his
  logs folder after the loop. Once it went further with a file of his: on 2026-10-01 Bader
  asked it to fix an error OneDrive showed, and the lead chose to delete a testhost.exe of his
  other repo, its own choice of fix, recorded in steps\loop.md. From now on the loop names a
  removal of anything of his to Bader before it makes it, its own rule
- Before the first run his logs folder, %LOCALAPPDATA%\ParsonsNwcFederator\logs, is copied
  into logs-backup, and before every start each file of his the backup does not hold is
  copied into it and read back. The tool's window logs only into his folder and keeps 30
  logs, so a window run may prune his oldest, which Bader allowed on 2026-10-01, Q82. After
  the loop his folder is put back to exactly what the backups hold, the loop's own logs and
  tsv files are taken out, and it is read back by name, size and sha256. A log of his that
  no backup holds is left as it is and named, the loop's own choice. At every window open the
  tool writes his
  remembered folders into its log, reading only, which Bader allowed, Q87, and that block is
  masked in every copy of a loop log that is committed. Any choice the tool remembers
  between runs is read before the loop and put back after it
- Close what you open. Every Navisworks the loop proves its own, by the rule below, is
  closed, and when the Automation API does not close it, through the handle its adoption
  holds, after its start ticks read equal through that handle. While the handle is open
  Windows gives its process id to no other process, so no close can reach another. One the
  loop started and cannot prove is left running and recorded, as the rule below says. A
  Navisworks the loop did not start is never closed, attached to or sent anything
- ONE COPY OF EVERY GUARD. The guards the probe and run.ps1 share live in
  tools\loop\nw-guard.ps1, a file of functions with no main body: the Roamer, own process
  and unproved start refusals, the adoption, the held handle, the watchdog with its
  deadlines, the settings backup and put back, the unproved starts and the one close,
  CloseAdopted. The probe, run.ps1 and the proof harness dot-source it and never copy from
  it. The refusals only run.ps1 makes, its host, parameter, path, evidence and tree checks,
  live in run.ps1 alone
- A Navisworks is the loop's own only when the Automation start returned without
  throwing, it is the one possible start, a new Roamer whose command line names embedding
  or cannot be read, it started after the call began, and its command line holds
  -Embedding. A new Roamer whose command line reads and does not name embedding, in any
  case and anywhere, is one started by hand, and is left alone. The adoption then opens that
  process's handle and reads its start ticks again through it, and a handle that cannot be
  opened or ticks that differ adopt nothing. Anything less and the loop closes nothing,
  calls nothing on that start, and says why. NOTHING IS CLOSED BEFORE IT IS ADOPTED, not
  even at a deadline: a start that cannot be proved is written to
  %LOCALAPPDATA%\NwcFederatorLoop\probes\unproved-starts.txt and left running, and every
  later start refuses while one named there still runs, so a person looks. A start is
  also refused while any Roamer runs, whatever its command line and whoever started it,
  read before the backup and again just before the start, and the code keeps that rule,
  not a person, run.ps1 at its checks 6 and 18. docs\history\scan.md 5z-d
- His AutoSave folder, %APPDATA%\Autodesk\Navisworks Manage 2025\AutoSave, is where the
  loop's Navisworks may autosave under his document names. Before every start each file of
  it that %LOCALAPPDATA%\NwcFederatorLoop\autosave-backup does not already hold, by name and
  sha256, is copied there and read back. What a run added, changed or removed there is
  listed. Once the loop's Navisworks is proved gone, the autosaves the run added are removed
  and any of his it changed is put back from the backup, each read back, Bader's answer Q86.
  The loop adds one guard of its own: this is done only when no Navisworks the loop did not
  start ran from the backup to then, because which Navisworks wrote a file is UNKNOWN while
  another runs. Otherwise nothing is written there and every change is listed
- Runs may go while Bader is away and while he works at the machine, he does not click the
  loop's Navisworks, and the display flag is allowed, Q85. The loop's driver, F106, acts only
  on windows of the Navisworks the loop adopted, never clicks, sends no key and never moves
  the pointer, the loop's own choice so that a run cannot reach what he works on. A locked
  screen that stops the window or the pictures is recorded as a finding and the loop waits,
  and the lock is never worked around, Q85
- While a Navisworks the loop did not start runs, there is no start, no install and no put
  back. The loop reads the processes every 10 minutes and carries on by itself once none
  runs, Bader's standing rule of 2026-10-04, Q98. run.ps1 only refuses. The waiting is the
  lead's, through its own waiter outside the repo, named in steps\loop.md
- The PC is kept awake for the whole loop, not only during runs, Q95, by the rule Bader set
  on 2026-10-04, Q98. keep-awake.ps1 runs as its own hidden process, not a child of Claude
  Code, and holds ES_CONTINUOUS, ES_SYSTEM_REQUIRED and ES_DISPLAY_REQUIRED. It changes none
  of his power, screen saver or lock settings, and stops itself, taking the request back
  first, when steps\loop.md reads STATE CLOSED or STATE WAITING or has not changed for 12
  hours. It is the lead's own script outside the repo. Its path, its process id and when it
  started and stopped are named in steps\loop.md, and it is checked alive at the start of
  every turn
- The loop installs the add-in only through tools\loop\run.ps1 -Mode Install, or by the
  in-place install at the end of this rule. run.ps1 -Mode Install runs
  build\install.ps1 from a checkout whose HEAD is the commit asked for and whose git status
  prints nothing, untracked files included, because the build stamp reads +edits for any
  of them. The installed stamp is read back and must name that commit. build\install.ps1
  refuses with one REFUSED line and exit 2, for every one who installs, while any Roamer
  runs, read immediately before it moves the installed bundle aside, when a folder from
  %APPDATA% down to the bundle is a junction or a link, and when the move aside fails
  because a file in the bundle is held. run.ps1 -Mode Install passes that refusal on as
  exit 2. A Roamer running right after a loop install, and a bundle left beside the new one,
  are each a finding that changes its verdict. That move aside is refused on this machine,
  F109, so Bader asked on 2026-10-01, Q96, and again on 2026-10-04, Q98, for main to be
  installed in place, apart from F109: built from a clean checkout of main, with no Roamer
  running, copied over the installed files, any installed file the new bundle lacks removed
  and named, and the installed stamp read back as main's commit. Before it, the loop stages
  and checks the bundle with build\install.ps1 -SkipBuild against a throwaway APPDATA under
  the work folder. The loop's own script for it,
  %LOCALAPPDATA%\NwcFederatorLoop\turn4\install-in-place.ps1, adds checks of its own: the
  installed bundle read equal to bundle-backup before anything is copied, the checked
  bundle's stamp read as the one asked, each copied file read back, the whole installed
  bundle read equal to the checked one after the copy, and on any failure every file put back
  from bundle-backup and read back
- While a Navisworks the loop started runs, the recent files, the window placement and the
  default plugin under HKCU\Software\Autodesk\Navisworks Manage\22.0 change, and files
  under %APPDATA%\Autodesk\Navisworks Manage 2025 can, measured on 2026-09-28. Which
  Navisworks wrote which change is UNKNOWN while another one runs. So before every start
  that key is exported and every file of that folder but AutoSave that can be read is
  copied into the work folder, and the work folder is moved aside, never emptied. Once the
  loop's Navisworks is proved gone, what changed is put back ONLY when no Navisworks the
  loop did not start ran at any point from the backup to then. Otherwise nothing is
  written, every change is listed with its old and new value in the turn's record, and the
  backup is kept for Bader
- No NWC, NWF, NWD, workbook or picture is ever committed
- No licensing id, session id or anything else that names Bader's Autodesk licence or his
  machine is ever committed. Every file of a run or a probe is read by
  tools\loop\mask-evidence.ps1 before it is committed, and the masked copy is what goes in.
  tools\checks\check-evidence-ids.sh refuses a commit whose staged files carry one of the
  KINDS NAMED IN tools\checks\evidence-ids.txt, the one place both read, and no other. F102.
  Where each reads, which is narrower than the guard:
  - the pre-commit reads only in a clone where it is switched on, git config
    core.hooksPath. On Bader's machine core.hooksPath is the ABSOLUTE path of the main
    clone's .githooks, measured on 2026-09-29, so a commit in any git worktree starts the
    main clone's copy of the hook. Since F102 a hook hands over to the committed tree's own
    copy when that is another file, so the tree is read by its own hook and its own check,
    once the copy that starts carries the handover. It hands over only to a copy holding
    the line that runs the evidence check, and refuses the commit when the tree's own copy
    does not. A main clone on a branch older than F102 runs its own old copy all through
  - Actions reads the tree for the ids and for the runner's name, NEVER FOR BADER'S, so only
    the pre-commit on his machine reads for his
  - the check cannot read a zip and refuses one, and no zip of run evidence is committed,
    Bader's answer Q90 A
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
9. From now on, a reading of the loop's own scripts blocks a run only for a fault that
   could harm Bader's things or make a run's evidence wrong. Words, polish and edge cases
   that cannot do either become register rows, not fix attempts. The product code keeps
   every house rule as written. Bader's words of 2026-10-01, Q93
10. Never report a check that did not run in this session
11. The writing rule at the end of CLAUDE.md holds everywhere
12. Every pull request is merged by the lead once Actions is green, watched with
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

A run is hung only when its log has not grown for five minutes AND its Navisworks used
under 20 s of processor time in those five minutes, Bader's answer Q83, where an idle one
was measured at 2 to 8 s. Then the last lines are saved, that Navisworks is closed through
the handle its adoption holds, and the hang is a finding. A sample that cannot be read
restarts both clocks, so it never counts toward a hang. A run still going 12 hours after
adoption is closed the same way and recorded as CEILING, never HUNG, Bader's answer Q84.
Any dialog Navisworks raises during a run is a finding with its text. A pane is not a dialog,
F125, such as the floating Clash Detective pane his saved layout opened on 2026-10-04: a window
of that Navisworks of the WinForms class, whose caption is not the main window's, owned by a
visible window, with its owner reading enabled or the window itself disabled. A message box, a
WPF window and a window of the main window's caption are never one. A modal dialog blocked by
the tool's window reads disabled with its owner disabled just as a pane does, so every line
naming such a window says it is either, and which one is UNKNOWN, and never that it is not
modal. The record writes a pane as PANE with its caption, its texts and both states, and writes
any window again when the rule's kind for it or either state changes. The driver notes one up
before Run and goes on, a window that comes up after Run and is not the confirm still stops the
driver unless it is that same pane, its line naming the rule's kind and both states, and a pane
never holds back the close at the end of a run. One rule in tools\loop\nw-guard.ps1 says what
is a pane, for the driver and the record alike.

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
