# The loop

STATE OPEN

TURN 5, THE FULL FIX ROUND, opened on 2026-10-04 by Bader's message headed 4 Oct 2026, Q98. No
run of C07 now: fix everything that is known, then prove the fixes on C06 in set 05, since
replaced by Bader's waves of 15:42, each tested on two buildings of C02 and C04. The plan
is the turn 5 entry at the top of steps\log.md. The fix list is steps\fix-round.md, built
before the first fix. Main at the start of the turn: f38edd5, installed e4484d15 in place since
2026-10-01 with be0b9b37 in bundle-backup. Why turn 4 stopped: Bader wrote "stop, i will close
the pc" at about 17:35 on 2026-10-01, and the pause, STATE WAITING, was pushed as 87d693e on
fix-T4-run-03c and not merged until PR 87 at the start of turn 5, so main read STATE OPEN all
through the pause. The System log reads a
shutdown at 19:30:49 on 2026-10-01 by shutdown.exe, and a restart for an update at 09:07:20 on
2026-10-04, the PC up again at 09:10:18. Get-Process Roamer read 0 at 09:51:56 on 2026-10-04.
MERGED IN TURN 5 SO FAR, 2026-10-04: PR 87, the pause and this opening, as 53c37b6. PR 84,
F108, the fresh copy for each run set, as c9b223b at 10:32, after main merged in and the
claim-checker's corrections to its entry and body. PR 88, F106, the window run, DONE for item
1, as 3449521 at 11:11, after the claim-checker's five corrections to its records. PR 84 and
PR 88 each green in Actions on their last commit before they merged, runs 37185873967 on
2ec693e and 37187977027 on bd3b9a8, and the merge commits naming #87, #84 and #88, all read
into turn5\actions-reads-pr84-88.txt. Then PR 89, the records with B4's note for the
modellers of the five FAILED groups of C06, steps\runs\03\for-modellers.md, as 51a0cb6 at
12:38. PR 83, Bader's answers in the loop rules, as 086a348 at 12:56, after four claim-checks.
PR 90, F105, as 2c89788 at 13:32. Each green in Actions on its last commit before it merged,
turn5\actions-reads-pr89-83-90.txt. PR 91, the fix list and the form, as 0e76b16 at 14:38,
after two claim-checks. PR 92, F107, the older machine's name masked, as f58c083 at 14:56. Its
final searches on 606283a: git grep for both forms, each exit 1, turn5\f107-grep-final.txt, and
the split search 0 hits in the 886 tracked files it read, exit 2 for the one broken fixture it
cannot open as a zip, turn5\f107-split-search-final.txt, whose text was read apart with 0 hits
on 8441dc9, turn5\f107-split-search-endswrong-2.txt. Both green in Actions on their last commit,
turn5\actions-reads-pr91-92.txt.
WITH DEVELOPERS at 14:57: F112 the alignment area, F113 the clash counts, F114 the views area's
first phase, each in its own worktree. F109, fix attempt 2, written at a0c3829: its reviewer
asked for changes with two blocking, the rule of what a junction is still also in run.ps1, and
one refusal with no case left, and its breaker found nothing that leaves a person without an
add-in and one blocking on words, a measurement said wider than it was taken, so three blocking
in all, turn5\f109-a2-reads.txt. Fix attempt 3 carries all three, the last before the form, its
developer planning and holding its changes until one of the three at work finishes, Bader's rule
allowing three developers at once. F104 part 2, the documents read, committed at
75e5dce with its records at 33732c9: its reviewer approved and its breaker found nothing blocking, both under Q93, their
notes for its register rows, turn5\f104p2-reads.txt. Its harness ran whole on its committed code
from 15:39 to 15:53, 102 passed, turn5\f104p2-prove-4.txt. F109's harness and the first real
documents read wait for the baseline.
B1 CHECKED: the plain prepare-copy.ps1 at 11:47:55 found the desktop folder changed and made
the source copy again, 142 files, 140 of them NWC, turn5\prepare-copy-plain-1.txt. Its
1104-PAR_CLASH_AllInOne_25mm_FIXED.xml is byte for byte the repo's exchange matrix, sha256
792b01fb, 1,446,076 bytes, asking ME-Ductwork 5 times and ME-DUCTWORK never, and Bader's old
one sits beside it renamed with -OLD, turn5\b1-xml-check.txt. So set 05's copy, once made
from this source copy, will ask ME-Ductwork, and its first run is to be given that XML by
name with -Xml, never the -OLD file beside it.
BADER'S NAVISWORKS RUNS, read at 13:19:51 on 2026-10-04: two Roamer.exe started by hand from
his desktop, explorer.exe their parent, at 12:53:23 and 12:54:16, each with the command line
-licensing AdLM. By his standing rule of 2026-10-04 there is no start, no install and no put
back while they run, and the developers were told at 13:21 to run no harness that starts a
stand-in named Roamer. Code, pull requests and merges go on. The waiter
turn4\wait-no-roamer.ps1 reads the processes every 10 minutes into turn5\wait-no-roamer.txt
and ends when none runs. It read one at 15:14:00, pid 37356, and NO ROAMER RUNS at 15:24:00,
so the loop's own starts may go again, each read again just before it starts.
BADER ANSWERED THE FORM at 15:23 on 2026-10-04, his message headed BADER'S ANSWERS, 4 OCT 2026,
TO THE FORM OF TURN 5: Q99 to Q109, Q24 and Q26, each answer under its question in
steps\02_questions.md and on its item in steps\fix-round.md, the lead's notes marked as the lead's.
THE WAVES, Bader's message of 15:42 the same day: the fixes go in waves of up to three areas that
touch different files, worst class first, every noise item, the docs and words and D1 last, and
each wave is tested at once on two buildings, the first run with the XML and the weekly run,
against a baseline of the installed add-in. It replaces the proof run of set 05 on C06, and C06
and C07 are parked until he says. The waves are written in steps\fix-round.md, under The waves.
WHAT NM FED HOLDS, copied whole by prepare-copy.ps1 at 15:44 with every file read back equal, and
the copy then listed by the lead: NWC\C02 holds one building, 1A02MM, four
models, AR, EL, ME and ST, and NWC\C04 one, 1A04PK, ten models, AR, EL, FP, HV, ME 000001 to
000004 and ST 000001 and 000002, with the corrected XML at the top. It also still holds C06, 67
NWC, and C07, 73 NWC, and the -OLD XML, 154 NWC in all, turn5\prepare-copy-plain-2.txt for the
copy and turn5\nmfed-listing-1544.txt for the listing. No folder named for C06 and C07 was on the
desktop when the lead listed it in the session at 15:46, a read kept in no file. The test runs
point at C02 and C04 only. Set 04's copy was made at 15:47, turn5\prepare-copy-set04.txt.
THE BASELINE, set 04, from main dd55e4b with the installed add-in reading 1.0.0.0 e4484d15, main's
product code since nothing under src, tests, build, bundle or exchange changed after e4484d1. Its
first run, item 1 on C02, started at 15:55:53 and stopped HUNG at 16:03:28: a floating Clash
Detective pane of the loop's Navisworks was up from its start, the window driver took it for a
dialog after pressing Run and stopped, and nobody answered the tool's confirm, record.txt lines
36, 48 to 50 and 80 of steps\runs\04\item1-C02-hung, the evidence masked at 18:32 by
turn4\mask-run-evidence.ps1 with no copy differing, turn5\mask-run04-item1-C02-hung.txt, and the
unmasked folder moved out of wt-main to runs\04\evidence-item1-C02-hung in the work folder so
the rerun's evidence folder starts empty. Nothing of Bader's was harmed: his settings and
AutoSave put back clean, and his logs folder as Q82 has it, one of his oldest logs pruned by the
tool and held in logs-backup and two loop logs added, both put back at the close of the loop,
record.txt lines 102 to 116. F125 fixes the driver before the baseline runs again.
MERGED, 2026-10-04: PR 95, F113, the clash counts of wave 1, FR-031 to FR-034, as 7b6df88 at
18:02:32, and PR 96, F104, the check of a workbook against its document, the wave test's
instrument outside the product waves, as c4fd0d4 at 18:14:56, after main merged into it at
ac3854c with both sides kept. Each green in Actions on its last commit before it merged, run
37210895243 on 9123e27 and run 37212128326 on ac3854c, turn5\actions-reads-pr95-96.txt. Their
branches are deleted. Nothing was installed since 2026-10-01, so the baseline still runs the
add-in run.ps1 -Mode Check read as 1.0.0.0 e4484d15 at 17:38:23,
turn5\f125-check-set04-item1-C02.txt.
THE READINGS, the journal of their workflow last written at 18:01:24,
turn5\w1-read-journal-time.txt. F125 by a reviewer and a breaker under Q93, both APPROVE with
nothing blocking, turn5\w1-read-review-F125.txt and turn5\w1-read-break-F125.txt. Three of their
points touch what the baseline will record, words that call a window not modal when that is
UNKNOWN, a stop line with no window state, and a monitor that writes a window only at first
sight, so they go to a second pass of F125 before the baseline runs again. Two more change a
verdict only in shapes no run has shown: a window after Run that the driver and the monitor
read differently, where the verdict is STOPPED, DRIVER either way, and a pane made again with a
new handle, which would call a finished run HUNG. They and the rest of their points are to
become F125 register rows for F122, the loop tools area of wave 4, and are read again if the
baseline shows either. F112 by a reviewer and a breaker under the house rules, both CHANGES, 3
and 5 blocking, turn5\w1-read-review-F112.txt and turn5\w1-read-break-F112.txt, so F112 goes to
fix attempt 2, whose brief also asks for Q110 and Q111 for Bader. The two developers of
F125's second pass and F112's attempt 2 started at 18:21:57 and 18:22:00 in workflow
wf_d55f67e0-13a, turn5\w2-workflow-start.txt, each told to merge main c4fd0d4 into its branch
first. F116, the clash XML, read at 18:39: FR-025,
FR-026, FR-008, FR-009 and FR-030 committed, the branch at 0157966 with main c4fd0d4 merged in.
F114 stays paused at f915396 for wave 2.

PAUSED FOR BADER'S SHUTDOWN, 2026-10-01 at about 17:35, on his word "stop, i will close the pc".
At the pause: Get-Process Roamer read 0 at 17:34:35, so no Navisworks of the loop runs and
nothing of his needs putting back. THE FIRST RUN OF C07 NEVER STARTED: launched at 17:28:36,
run.ps1 pid 39432 refused at its check 10 because the session read locked, Q85, "Nothing was
written", turn4\run03-item1-C07-console.txt. The F110 and F111 workflow was stopped part way,
so wt-f110 and wt-f111 may hold work not committed. The C07 waiter was stopped. Q98, whether a
group with a model 2,000 km away stays DONE, and Q99, which clash XML the runs use, are still to
be written for Bader, from findings 1 and 2 of steps\runs\03\findings.md.
Q98 and Q99 above are numbers the pause meant to use. Q98 went instead to Bader's message of
2026-10-04, which decided both: B2 for the far model and B1 for the XML.
WHEN HE SAYS GO, as written at the pause and since replaced by turn 5, which runs no C07:
Get-Process Roamer. If any Navisworks the loop did not start runs, stay WAITING. Otherwise
STATE OPEN, start the keep awake again, start the first run of C07 the same way with the
session unlocked, carry on with set 03, and start F110 and F111 again from their worktrees,
nothing installed until C07 ends.

THE FIRST RUN OF MAIN HAPPENS TODAY, Bader's message of 2026-10-01 12:10, Q96. Until the first
run's RESULT block is written only what that run needs is worked on, and everything else
waits: F109's workflow stopped by the lead at about 12:16, the F105 and F107 words round
stopped at about 12:45 part way, wt-f107 committed at 2063c29 and wt-f105 holding staged
changes not committed, and PR 83, the rules, and PR 84, F108, left open with the findings of
their claim-checks, which were returned to the lead in the session, all read into
turn4\reads-pr85.txt at 12:45:56.
- MAIN IS INSTALLED, in place, apart from F109, as Bader asked. wt-main at e4484d1, clean. The
  add-in built with the build command of build\install.ps1 line 39, 0 warnings and 0 errors,
  stamp e4484d15, turn4\install-build.txt, the command line itself kept in no file.
  build\install.ps1 -SkipBuild then staged and checked it against a throwaway APPDATA under the
  loop folder, every reference satisfied, 14 assemblies, exit 0, turn4\install-fake-run.txt.
  Get-Process Roamer read 0 at 12:20:07, turn4\roamer-reads.txt line 4, and again in the
  script at 12:20:09. turn4\install-in-place.ps1 read the installed bundle equal to
  bundle-backup, 15 files by name and sha256, copied the 15 checked files over the installed
  ones and read each back equal, removed none, every installed file being in the new bundle,
  and read the installed Federator.Addin.dll back as 1.0.0.0 e4484d15 built 2026-10-01
  12:19:09 at 12:20:10, turn4\install-in-place.txt. bundle-backup still holds be0b9b37
- THE COPY FOR SET 03 IS MADE, by F108's prepare-copy.ps1 -Set 03 from its branch, about
  12:20:43 to 12:20:50 by the lead's console, kept in no file: the source copy matched the
  desktop folder on all 141 files and 10 folders, and
  runs\03\NMFed holds 141 files and 12 folders, Clash Report\C06 and C07 made, every file read
  back by sha256, NMFed.manifest.txt written last, turn4\copy-set03.txt
- THE WINDOW RUN, F106, is being finished for item 1 by one developer the lead started after
  12:25, its brief turn4\f106-finish-brief.md. Before it, the F106 code was written in wt-f106
  by developers of the stopped workflow whose transcripts end interrupted, and none of it was
  committed. Its reading before the run is for harm to Bader's things only, Q96, which narrows
  Q93 for this run
- If the window cannot be driven today, one line says why here and Bader is told in the
  Claude tab, so he can press Run himself
- F106 FINISHED FOR ITEM 1 at 32dce24, the finishing developer's report in
  turn4\f106-finish-report.md, and READ FOR HARM ONLY by a reviewer and a breaker, both SAFE FOR
  THE FIRST RUN with no harm found, their other findings for the register, turn4 and the
  workflow journal wf_8100973e-871. Check for set 03, item 1, C06 read exit 0 at 13:35,
  turn4\check-set03-item1.txt
- THE FIRST RUN OF MAIN STARTED. The lead's first launch at 13:58:20 passed the arguments as
  one, and run.ps1 refused, "is not a parameter of run.ps1. Nothing was started and nothing
  was written", turn4\run03-item1-C06-refused-console.txt. Launched again at 13:59:01,
  run.ps1 pid 5152, turn4\run03-item1-C06-pid.txt. Navisworks pid 27500 from 13:59:17.
  THE WINDOW WAS DRIVEN WITH NO CLICK, runs\03\item1-C06\driver.txt: the tool's window opened
  at 14:00:39 and stayed open, the driver typed the C06 folder and pressed Scan, 67 NWC, 22
  groups, 0 unticked, typed the NWF, NWD and Clash Report folders of C06 and the XML, read
  every box back equal, found the tolerance box on Use the value in the XML, pressed Run, and
  answered the confirm with OK at 14:00:55.585, its texts naming no path outside the loop
  folder. The tool's log: RUN started, 22 groups, the Clash step picked the copy's XML, 61
  sets and 1830 tests, and GROUP started 100000 at 14:00:55
- THE FIRST RUN OF MAIN ENDED, VERDICT RAN, set 03 item 1, C06. RESULT, log line 8429 on, of
  steps\runs\03\item1-C06\run-20261001-140037.log: 17 groups done, 0 partial, 5 failed, 5679
  clashes across 22 groups, 6 of which found none, 88 files written. The tool's run took 1 h 44
  min 52 s, 59 min 52 s over the 45 minutes, VIEWS 72 percent of it. The window closed on the
  loop's WM_CLOSE after RESULT, Dispose at 15:46:30 and Navisworks gone 10.0 s after, nothing
  forced. Put back with no other Navisworks having run: 37 registry values and 2 files, and
  Q86 in his AutoSave, 78 files as they were, each read back. Q82: the tool pruned his oldest
  log, run-20260901-191711.log, which logs-backup holds. All 22 workbooks read out. The
  evidence masked by F102's tool, 30 files, none changed, into steps\runs\03\item1-C06
- THE FIRST FINDINGS, steps\runs\03\findings.md, read by four readers: the five FAILED groups
  each have a model on Revit's internal origin, four DONE groups each have a model about 2,000
  to 2,800 km away and still read DONE, duct, pipe and equipment sets find nothing because the
  copy's XML, which is not the corrected matrix in exchange, asks upper case worksets the models
  spell in mixed case, the WORKBOOK CHECK and the RESULT file sizes print wrong numbers, and the
  run is 2.3 times the 45 minutes, VIEWS alone 1.7 times. Bader's message of 15:30, Q97. For Bader
  three tests with their workbook counts to check against the Clash Detective panel
- ONEDRIVE, at Bader's request in the Claude tab to fix the error, what to remove chosen by
  the lead: Roamer.exe, the loop's stand-in left in the
  F103 worktree folder under .claude\worktrees, and testhost.exe, a build output of his
  RCRC-Green repo, were blocked by OneDrive. The F102 and F103 worktree folders were removed at
  about 16:46 and that one testhost.exe deleted. OneDrive put both folders back at 16:47 from
  its cloud copy, without the .exe, which it never held. No .exe is left under his GitHub
  folders. RCRC-Green's returns whenever its tests build inside OneDrive
- NEXT: the first run of C07 the same way, then the rest of set 03 tonight. Fixes may start
  while C07 runs, and nothing is installed until C07 ends

TURN 4, opened on 2026-10-01 by Bader's message headed 30 Sep 2026: code runs the whole of
NM Fed itself, and the form is answered. It replaces the window message of 30 Sep and
anything earlier that said Bader runs the tool by hand. The goal is real full runs of main on
every NWC in NM Fed, driven by the loop, then the fixes those runs show. His answers are Q82
to Q92 in steps\02_questions.md, his rule for the loop's own tools is Q93 and the goal is
Q94. The plan is the turn 4 entry of steps\log.md, the plan the lead wrote to Bader before
the first edit. Main at the start of the turn: 821ed6e. The main clone is on
fix-T4-records-1, never on main, so a push from a worktree passes the git wall, T3-W1. From
2026-10-01 10:25 every Get-Process Roamer read the lead cites is written first to
%LOCALAPPDATA%\NwcFederatorLoop\turn4\roamer-reads.txt.

THE RULE FOR THE LOOP'S OWN TOOLS, Q93. A reading of the loop's own scripts blocks a run only
for a fault that could harm Bader's things or make a run's evidence wrong. Words, polish and
edge cases that can do neither become register rows, not fix attempts. The product code
keeps every house rule as written.

WHILE A NAVISWORKS THE LOOP DID NOT START RUNS: no start, no install and no put back. The
waiter turn4\wait-no-roamer.ps1 reads the processes every 10 minutes and ends when none
runs, and the lead then carries on with no word from Bader. Runs may go while Bader is away and while he works at the machine, and he will not
click the loop's Navisworks. A locked screen that stops the window or the pictures is a
finding, and the next start waits until the session reads unlocked, Q85.

KEEP AWAKE FOR THE WHOLE LOOP, Bader's instruction of 2026-10-01, Q95. One background
powershell.exe, PID 21164, STARTED 2026-10-01 10:48:37, runs
%LOCALAPPDATA%\NwcFederatorLoop\turn4\keep-awake.ps1. On its main thread, native thread 27484,
it called SetThreadExecutionState with ES_CONTINUOUS and ES_SYSTEM_REQUIRED, 0x80000001, which
returned 0x80000000 at 10:48:38, so the request holds while that thread lives. No display
flag, and none of Bader's power settings changed. It reads the STATE line of this file once
a minute and stops itself when it reads STATE CLOSED or STATE WAITING, taking the request
back first. Its lines go to turn4\keep-awake.txt. STOPPED 2026-10-01 17:35:43, on reading
STATE WAITING, its release returning 0x80000001, turn4\keep-awake.txt.
KEEP AWAKE OF TURN 5, Bader's rule of 2026-10-04. One hidden powershell.exe, PID 1312, STARTED
2026-10-04 09:55:40 through WMI's Win32_Process Create, so its parent is pid 9844 WmiPrvSE.exe
and not Claude Code, in session 1. It runs %LOCALAPPDATA%\NwcFederatorLoop\turn5\keep-awake.ps1
and on its main thread, native thread 40512, called SetThreadExecutionState with ES_CONTINUOUS,
ES_SYSTEM_REQUIRED and ES_DISPLAY_REQUIRED, 0x80000003, which returned 0x80000000 at 09:55:53.
None of Bader's power, screen saver or lock settings changed. It stops itself, taking the
request back first, when this file reads STATE CLOSED or STATE WAITING, or has not been written
for 12 hours. Its lines go to turn5\keep-awake.txt. ENDED BY THE LEAD at 19:35:23, its last line
"still holding" at 19:26:01, to start the script as changed for Bader's message of the five
requests, Q112: it keeps running while this Claude Code session is open, STATE WAITING
included. The copy before the change is turn5\keep-awake-v1.ps1.
KEEP AWAKE SINCE Q112. keep-awake.ps1 now takes the session's claude.exe, its pid and start
time, and once a minute reads that it still runs. When it is gone it takes up any other
claude.exe of the Claude Code extension, and it stops, taking the request back first, only when
none runs or this file reads STATE CLOSED. Proved on a copy with its own mutex, 8 passed and 0
failed, turn5\keep-awake-test\prove-result.txt. Started through WMI as pid 10412 at 19:35:39,
ended by the lead at 19:37 to prove the check, and started again by
turn5\check-keep-awake.ps1 as pid 29740, STARTED at 19:36:56, parent pid 9844 WmiPrvSE.exe,
watching claude.exe pid 19148 started 17:03:32, its call returning 0x80000000,
turn5\keep-awake.txt and turn5\keep-awake-checks.txt. The check runs every 30 minutes from a
schedule of this session, and the lead runs it too at the start of every turn. SLEEP WHEN
PLUGGED IN: Bader allowed one change to his power settings, sleep when plugged in set to Never,
the old value saved and put back at the close. It already read Never, 0, with hibernate after
and turn off display after also 0 and no power policy key, so nothing was written and nothing
is put back, turn5\power-before.txt. The System log since 2026-10-01 holds no sleep and no wake,
turn5\sleep-wake-since-1oct.txt.

THE RUN SETS OF TURN 4. Each run set gets a fresh copy at
%LOCALAPPDATA%\NwcFederatorLoop\runs\NN\NMFed in Bader's own folder shape: NWC\C06 into
NWF\C06, NWD\C06 and Clash Report\C06, the same for C07, and the clash XML at the top. It is
named NMFed and not NM Fed because the paths wall refuses every command naming NM Fed. Clash
Report has C06 and C07 folders because group 100000 is in both communities, and the folders
the copy keeps are listed in steps\runs\00\source-listing.txt lines 147 to 157. Every run goes
through the real window, and a run of a folder ticks every group. A first run takes the XML
from the copy, with
the tolerance box left on Use the value in the XML, so each test the run creates from the
XML carries the XML's 25 mm. The weekly run and the later runs of the set take no XML, and a
test already saved in an NWF keeps its own tolerance. Nothing is written into NM Fed on the
desktop.

## Next action

Turn 5, the full fix round, Q98, now in waves by Bader's message of 2026-10-04 at 15:42. Merged so
far on 2026-10-04: PR 87 as 53c37b6, PR 84, F108, as c9b223b, PR 88, F106, as 3449521, PR 89 as
51a0cb6, PR 83 as 086a348, PR 90, F105, as 2c89788, PR 91, the fix list, as 0e76b16, PR 92, F107,
as f58c083, PR 93 as dd55e4b, PR 94 as 6689bad, PR 95, F113, as 7b6df88, PR 96, F104, as
c4fd0d4, and PR 97 as 68c870b. Bader's five requests of the evening, Q112, are added to the round
and planned in the turn 5 entry of steps\log.md headed with them.

1. The baseline, set 04: item 1 on C02 rerun from F125's commit 5fa98a8 at 18:55, then item 1 on
   C04, item 2 on C02 and item 2 on C04, each after a fresh Roamer read, then
   steps\runs\04\findings.md, short, in the shape of set 03's, merged within the hour. The hung
   first attempt is kept as steps\runs\04\item1-C02-hung
2. Wave 1: F125's second pass and F126, the driver unticking a named box, each read under Q93
   and merged. F112 fix attempt 2 read and merged. F116 fix attempt 2, and its merge once Bader
   answers Q113. Each with a reviewer, a breaker and the claim-checker, Actions green, merged one
   at a time
3. The test of wave 1: main installed in place as on 2026-10-01 and its stamp read back, both
   buildings run, item 1 and item 2, and a building whose models are off the shared coordinates
   run once more with the rule switched off by its tick box. Compared with the baseline through
   turn5\wave-compare.py and F104's documents read: each group's result, the clash count per
   test, the time per step and the VIEWS share, the workbook check, the RESULT numbers and the
   log size, each item of the wave marked proven or not with its line. A fix that makes the test
   worse is taken back at once, with one line why
4. Three short lines at the top of steps\fix-round.md and in the Claude tab, what the wave fixed,
   what the test proved and what got worse, merged within the hour. Three lines in the tab after
   each of Bader's five requests too
5. Measured from the baseline before wave 2: which property and value name Generic Models on
   1A02MM and 1A04PK, what the log, .tsv and workbook hold today for each test of the coverage,
   why the Shift range fails in the window code, and whether the driver can test a Shift click
   without real input
6. Wave 2 in two halves, 2a F127 coverage first with F115 sets and F114 views, 2b F128 generic
   models with F118 workbook and report. Wave 3 in two halves, 3a F129 start from an NWF with
   F120 harvest and pictures and F109 install, 3b F130 the Shift range with F119 run log and
   RESULT. Then waves 4 and 5, as steps\fix-round.md lists them. Every wave test after wave 2
   shows the Coverage sheet
Tests run only while no Navisworks of Bader's runs, the waiter reading every 10 minutes. The
keep-awake is checked every 30 minutes.

## The phases

| phase | state | where it is |
| --- | --- | --- |
| 0, the house | DONE in turn 1, read three times before it went out | PR 72 |
| 1, measure and a run with no click | F100 DONE in turn 3. F102 and F103 merged on 2026-10-01 on Bader's answers, F105 answered and with its developer. F104 part 2 after the first runs of set 03 | PR 74, 76 and 78 |
| 2, the register | DONE EARLY in turn 1, from a read of the whole repo | the register below, and steps\loop-read.md |
| 3, the baseline | open. In turn 4 it is set 03, every run through the real window, after F106 builds the window run | steps\runs\03 |
| 4, the loop | open | |
| 5, Bader's rules R1 to R6 | open. D3 builds them in this loop | |
| 6, the close | open | |

## Phase 1, in this order

1. The prober measures how Navisworks Manage 2025 can start a run with no click. The lead
   in the repo: tools\probes\ViewpointProbe was loaded "into a Navisworks started through
   the automation API with AddPluginAssembly and run with ExecuteAddInPlugin",
   ViewpointProbe.csproj lines 4 to 6, and Autodesk.Navisworks.Automation.dll is installed.
   No script in the repo starts Navisworks, and the launchers used then are UNKNOWN. It
   also measures Roamer.exe switches, whether UI Automation reaches the window
   (tools\probes\drive-window-run.ps1 already drives it once the window is open), and
   whether the installed API writes the native Clash Detective HTML tabular report
2. The prober runs tools\probes\probe-viewpoints.ps1 and tools\probes\probe-model-remove.ps1
   and appends both answers to docs\history\scan.md
3. The no-click entry, its own pull request. The jobs are built inline in the private OnRun
   of the window, FederatorWindow.xaml.cs from line 1681, so the job building moves to one
   place the window and the entry both call, and the run stays FederationEngine.Run. A
   second AddInPlugin with no ribbon button, reading a plain text settings file, one choice
   per line, parsed in Core with its tests, carrying every choice in steps\loop-read.md
   section 7 plus the log folder. RunLog.StartOrDisabled already has an overload that takes
   a folder. It refuses to run without a settings file
4. tools\loop\run.ps1, then the check of the workbook against a separate read of the
   document
5. Before the first run: copy %LOCALAPPDATA%\ParsonsNwcFederator\logs into
   %LOCALAPPDATA%\NwcFederatorLoop as a backup, and keep folders.txt from that folder, the
   one choice the tool remembers between runs, to put back after the loop. DONE in turn 2,
   into NwcFederatorLoop\logs-backup, 32 files, every one matching by sha256. folders.txt
   there names NM Fed for the NWC, NWF, NWD and workbook pickers since Bader's 16:37 run,
   and the 16:37 log shows it named ACC folders under DC\ACCDocs for two of them before. It is put back
   after the loop and never followed
6. Before the first install: copy the installed bundle into
   %LOCALAPPDATA%\NwcFederatorLoop\bundle-backup, and check no Navisworks the loop did not
   start is running. If one is, STATE WAITING and ask Bader to close it. The backup is DONE
   in turn 2, 15 files, every one matching by sha256, and its Federator.Addin.dll reads
   be0b9b37 built 2026-09-27 10:47. THE INSTALLED ADD-IN IS NOT MAIN, so nothing about main
   has run on this machine yet. A Navisworks the loop did not start, process 34668, was
   running from 09:33 on 2026-09-28, so the install waits for it to close

## Facts measured on this machine

- git fetch fails with CRYPT_E_NO_REVOCATION_CHECK unless http.sslBackend names schannel,
  set in this clone's .git\config only. gh 2.101.0 is at %LOCALAPPDATA%\Programs\gh and
  logged in as baderrahal, and C:\Users\p003653k\bin\gh lets Git Bash call it by name
- the add-in builds here with 0 errors and 0 warnings, and the Core tests read 1746
  passed, 0 failed, 0 skipped, so the 32 the container skips all run and pass here
- hooks in .claude\settings.json load mid session. Agents in .claude\agents load only when
  a session starts, measured both ways: not loaded in the session that wrote them, loaded
  in the next
- Claude Code finds Git for Windows' sh without help. Starting sh costs 2.3 to 2.9
  seconds, and each wall in .claude\hooks takes 2.4 to 3.6 seconds on an ordinary call and
  6.9 to 8.0 on a git commit, timed three times each on 2026-09-28
- Navisworks Manage 2025 is 22.5.1433.58 with Autodesk.Navisworks.Automation.dll
- one Navisworks, process 32472, was running before the loop and was never touched by it.
  It had gone by 17:09 on 2026-09-27
- Desktop Connector keeps ACC projects under C:\Users\p003653k\DC\ACCDocs
- the copy of NM Fed is at %LOCALAPPDATA%\NwcFederatorLoop\source, 141 files, 208.3 MB,
  every file matching by sha256 and marked whole, with its listing in
  steps\runs\00\source-listing.txt
- NM FED CHANGES UNDER THE LOOP. Bader ran the tool on 1B06BC at 16:37 on 2026-09-27 with
  NM Fed as its output folder, and 639 files arrived in it, an NWF and 638 pictures, then
  went again. The listing taken while they were there is steps\runs\00\source-listing-
  during-bader-run-1B06BC.txt. prepare-copy.ps1 remakes the copy whenever NM Fed differs
- BADER'S 16:37 RUN NEVER FINISHED. Its log, run-20260927-163731.log in his logs folder,
  ends at 17:00:36 on the second NWF save into NM Fed, with no RESULT block, no workbook and
  no NWD, and Navisworks was gone by 17:09. It ran the be0b9b37 build, not main. It found
  638 clashes on 1B06BC, created all 1830 tests and skipped 1164 for an empty side. Whether
  the save hung on the OneDrive folder is UNKNOWN. It is register row RUN-1637
- the client's two workbooks hold 65 and 66 pictures each, every one linked to an absolute
  file:/// path and none stored, read off the zip on 2026-09-28, which is what core.md says
- the NM Fed clash XML is Bader's 25 mm matrix with BLD-DRPipe Accessories renamed
  BLD-DR-Pipe Accessories, and it still asks for upper case worksets such as ME-DUCTWORK
- this machine ran the tool before, on 2026-09-07, over C06 through the ACC connector,
  logged in steps\log.md and the committed run-20260907-093440.log, although
  steps\03_bader_next.md says C06 never existed. It is the register row about steps 9 to 17
- read at 17:06:18 on 2026-09-30 into turn3\reads-170618.txt: the power plan, Balanced,
  never sleeps or hibernates on idle on mains power, and the laptop read on mains at 100
  percent. What closing the lid does could not be read, UNKNOWN
- the clash XML in the copy, sha256 36AB2739, holds 1830 clash tests, each with tolerance
  0.0820209974 in feet, 25 mm, read on 2026-10-01 into turn4\xml-reads.txt. The tool's
  tolerance box defaults to Use the value in the XML, ToleranceChoice.cs, under which each
  test keeps what its source gave it. So a first run that leaves the box alone gives each
  test it creates from this XML 25 mm. Not yet seen on a run of main

## The form

What waits on Bader's answer. A finding moves here when it survives three fix attempts,
with what was tried and what each attempt showed. The register rows marked needs Bader,
in the form are the questions already in steps\02_questions.md and are not repeated here.

OPEN IN THE FORM NOW, each in steps\02_questions.md with its evidence and its choices:
- Q113, F116's corrections of the clash XML shipped as data in Core against CLAUDE.md's rule
  that no project's names sit in src, and Q103 read wider than the matrix, with what Furniture
  and Site do to a landscape group. F116 does not merge until he answers
- Q110 and Q111 are held by F112's fix attempt 2 on its branch and reach the form when it merges.
  Q112 is his own message of the five requests and asks him nothing

THE FORM OF TURN 5, written on 2026-10-04 from the fix list, steps\fix-round.md, each question in
steps\02_questions.md with its evidence and its choices:
- Q99, the 1 m of B2, with what 1 m, 10 m, 100 m and 1 km each do to the 17 DONE groups of set 03
- Q100, a model named Internal that sits within 72.5 mm of its reference, FR-006
- Q101, what the 45 minutes counts, each group, each community or the whole folder, FR-070
- Q102, one workset spelling that does not fit every C06 building, FR-008
- Q103, AR sets that find other disciplines' items, FR-009
- Q104, the class that made the corrected matrix, FR-030
- Q105, his profile folder in committed evidence, FR-109
- Q106, a check that refuses the older machine's name, FR-110
- Q107, the older machine's account name, FR-149
- Q108, the 300 KB log bar, FR-136
- Q109, the date format, separator and part positions with no control in the window, FR-161
- and two asked on 2026-09-12 and still open: Q24, a name cell given back to the pattern,
  FR-160, and Q26, members read only by a test, FR-172

ANSWERED ON 2026-10-04 at 15:23, every question of the form of turn 5, each answer under its question in
steps\02_questions.md and on its item in steps\fix-round.md. Q99 and Q100: a group whose models
are not on the same shared coordinates has only its clash skipped and ends PARTIAL. Q101: VIEWS
faster with the same viewpoints, each group's time reported beside its NWC sizes and item
counts, the 45 minutes not judged in this round. Q102 to Q104: the corrections applied to the
picked XML before any set is built, with OR rows for both spellings and Source File contains
-AR- on the AR sets that need it. Q105 to Q107: the names masked and refused by the check, the
older ones held as a hash. Q108, Q109, Q24 and Q26: find the mistake first, with test steps
under each item, run before anything changes, which are orders to look and not decisions, and
Q26's members whose tests still pass after the break come back to the form. Nothing of the form
of turn 5 waits on him now. The older questions with no answer, Q25, Q27 to Q31, Q35 to Q40, Q45
to Q47, Q49 to Q51 and Q76 to Q78, are not part of it and wait as before.

ANSWERED ON 2026-10-01: Q82 to Q92, every one, each answer under its question in
steps\02_questions.md. Nothing from turn 3 waits in the form now. The two sections below are
kept as what Bader answered.

### F100, the probe of a start with no click, after three fix attempts

What it is: tools\probes\probe-automation-start.ps1, on branch fix-F100, whose attempt 3
is commit b01ad71, PR 74 open as a draft. It is the pattern tools\loop\run.ps1 will copy to start
and close every loop run.

What the attempts showed:

- the first version ran once, run 1 at 12:27 on 2026-09-28: the Automation API starts a new
  Navisworks here with no click, as Roamer.exe -Embedding, the constructor returning after
  110.02 s, and Dispose closes it, gone 8.5 s after, lines 226 and 264 of the result as
  committed in 464f79f. Its reviewer and breaker found it could adopt and force close a
  Navisworks it did not start, if one was opened by hand during the start and the start
  threw
- fix attempt 1 ran twice, runs 2 and 3 at 13:11 and 13:17: the constructor returned after
  85.36 s and 82.75 s, and the Navisworks was gone 9.3 s and 8.0 s after Dispose, lines 394
  and 432 of run 2's kept result and 395 and 433 of run 3's. Its readers found it put
  Bader's settings back while his own
  Navisworks ran, so it could revert a change of his, could empty a backup before a
  refusal, and still closed an unproved start at its deadline
- fix attempts 2 and 3 are not run with a Navisworks, because Bader's was open all day and
  the design forbids a run then. Their harness proved every write of the put back on a
  throwaway key and folder, and the refusals on the real probe
- the fourth reading, of attempt 3, found faults inside what attempt 3 changed, each one to
  five lines: settings could be put back after the probe fails to end itself at its
  deadline, one result line prints fixed text as if read, one registry write does not
  re-read its key, window reads after adoption go by pid alone, an unproved start is
  printed as written down before the write is tried, and a recorded start with no start
  time can block the loop when a Navisworks opened by hand later takes its pid. And the
  rule that no start is made while ANY Navisworks runs is kept by a person, not by code,
  so an unattended run.ps1 must enforce it. Each fault with the probe lines it rests on is
  in %LOCALAPPDATA%\NwcFederatorLoop\turn1\f100-fourth-reading.md, beside the three fix
  lists

THE RESULT FILE MUST NOT REACH MAIN AS IT IS. tools\probes\automation-start-result-20260928.txt
on the branch carries the licensing agent's analytics id and a session id, from the run
of the version before attempt 2. A squash merge still brings the branch's final files, so
the file must be replaced by a new run, or taken out, before anything of this branch merges.

Bader's choice:

- A. Allow a fourth fix attempt of exactly the faults above, then the one run once his
  Navisworks is closed, then merge. THE LEAD'S RECOMMENDATION, with run.ps1 refusing to
  start while any Roamer runs, which also closes the gap of an attach to his Navisworks
- B. Stop F100 here: keep runs 1 to 3 as the measurement in docs\history\scan.md 5z-d, take
  the result file out, merge that, and write run.ps1's start and close fresh from the
  design, read from the start
- C. Something else he names

ANSWERED A on 2026-09-29, Q79: one more fix attempt of exactly the faults in the fourth
reading, with no start while any Navisworks runs enforced in code, then the one run, then
merge. The last attempt. If the reading after it finds a new fault, F100 goes to B with no
question. No result file reaches main with a licensing id or a session id in it.

### Turn 3, what run.ps1 needs from Bader before a window opens on main, Q82 to Q87

The design of run.ps1, F103, is in steps\notes\f103-design.md once F103's pull request
carries it, and until then in %LOCALAPPDATA%\NwcFederatorLoop\turn3\f103-design.md. Its
parts needs_bader and log_folder_problem are the evidence. Each question is written out in
steps\02_questions.md with its options.

- Q82, THE LOG FOLDER, blocks every window run on main. Measured 2026-09-29: his logs folder
  holds exactly 30 run logs and the tool keeps 30, so a window run deletes his oldest. The
  lead recommends A, hold his logs open with no delete sharing during a run, then remove only
  the loop's own log and tsv. Until he answers, NO WINDOW OPENS ON MAIN. The start with no
  window, run.ps1, the document read and the install of main go ahead, because none of them
  opens the window
- Q83, what counts as no processor time for the hang rule, measured by the first start
- Q84, whether a ceiling may close the loop's own Navisworks, 12 hours proposed
- Q85, the screen kept on, runs while he is away or at work, a locked screen
- Q86, the loop's autosaves in his AutoSave folder, 196 files and 286 MB, backed up first
- Q87, the tool reading his remembered folders at every window open, reading only
- Q88, from F102: main names the machine of 2026-09-19 in 9 places, and PR 74's refs on
  GitHub keep the licensing ids of fix-F100's first commits after the squash merge, which
  only the repository's owner can ask GitHub to purge
- Q89, F105 after three fix attempts: its four answers are backed line by line in all four
  readings, and the fourth reading still found faults of the same kind in the shared reader
  the fixes added. The lead recommends A, merge with three sentences of 5z-f narrowed
- Q90, from F102: the new check refuses a zip, while the loop rule says a run file over
  20 MB is committed zipped. Where may a zip of run evidence sit
- Q91, F102 after three fix attempts: the final reading found a workbook under samples with
  text appended still passes, because the rule allows the zip end record anywhere in the
  last 65557 bytes, while every real one holds it 22 bytes from the end. The lead recommends
  A, one more change to make the window 22 bytes, then merge
- Q92, F103 after three fix attempts: the final reading found one of the last attempt's items
  not fixed, in the words. The harness header says a line throws that only sets the error,
  named after attempts 1 and 2 as well. All three readers answered safe for item 0 and safe
  for install, one within its lens. The lead recommends A, one change of words only, no
  change of logic, at the nine entries of steps\notes\f103-final-reading.md, the harness run
  again, then merge, tonight's start standing as its run

Built in turn 1 from steps\01_next.md, steps\02_questions.md, steps\04_audit.md,
steps\04_audit_first_run.md, steps\03_bader_next.md, the known bugs of steps\log.md, the
chat audit of 19 Sep, the defaults, Bader's run of 16:37 and the read of the whole repo in
steps\loop-read.md. Existing F, Q and A numbers keep their IDs. A step number is a step of
03_bader_next. A T1 tag is a fault the read reported and nobody has confirmed, and it takes
an F number only when it becomes work. Most harmful first when the loop picks, and a silent
wrong number ranks above a loud failure. Done in code but not proved by a run means the
baseline run proves it or contradicts it.

322 rows, by status, after turn 4's eighteen additions and turn 5's eight:

- 125 done in code, not proved by a run
- 3 reported by the read, not verified, T1-N, T1-UNCALLED and T1-CATCH
- 86 read again by two readers on 2026-09-29, 76 CONFIRMED and 10 PARTLY, none refuted:
  60 silent wrong outputs, 13 broken features, 6 loud failures, 7 noise. Every one
  waits for the baseline, because no fix lands before it, steps\notes\turn1-read-verified.md
- 28 needs Bader, in the form
- 2 answered by Bader on 2026-10-04, Q24 and Q26, their finds in the waves
- 2 DONE in turn 4, F102 and F103, merged on Bader's answers
- 3 answered by Bader on 2026-10-01 and being carried out, F105, Q82 to Q87 as one row, and Q88
- 1 open in turn 4, F109
- 1 done in its pull request, F107
- 3 known limits or items, F102-L1, F102-L2 and Q88-IDS
- 5 open for later, register rows under Q93, F103-W and F105-R1 to F105-R4
- 2 merged in turn 5, F108, and F106 for item 1
- 7 register rows of turn 4's readings, F108-R1 to F108-R5, F107-R1 and F107-R2
- 19 open fault
- 4 DONE
- 19 open for F103 or after it, T3-G1 to T3-G16, T3-P, T3-P2 and T3-B
- 1 built and read safe, F104, its first real read waiting
- 7 register rows of F104's readings, F104-R1 to F104-R7, under Q93
- 1 seen on an old build, the baseline answers it for main
- 1 open, after the faults
- 1 closed, not there at 42499bf
- 1 open, the git wall, T3-W1

| ID | came from | what it is | what proves it fixed | status | PR | run that proved it |
| --- | --- | --- | --- | --- | --- | --- |
| F97 | loop prompt, Phase 0 | The house: agents, hooks, rules, tools\loop, steps\runs, allow list | on main, every hook case answered on standard input, both walls refusing live | DONE | 72 | no Navisworks run applies, proved on standard input and live |
| F98 | turn 0, a read of steps\log.md | PR 71 dropped the close round heading in steps\log.md | the heading back above Core tests 1666 before the round and 1746 after | DONE | 73 | no run applies, the file from the heading down matches the one before PR 71 |
| F99 | turn 0, the git wall fired on Bash only | The git wall missed commits sent through PowerShell | a PowerShell commit on main refused on standard input, and git.exe read as git | DONE | 72 | no Navisworks run applies, proved on standard input and live |
| F100 | loop prompt, Phase 1 item 1 | Nothing measured how Navisworks starts and closes with no click on this machine | tools\probes\probe-automation-start.ps1 run once as committed, with no Navisworks the loop did not start running, and docs\history\scan.md 5z-d written off that run | DONE, fix attempt 4 on Bader's answer A, Q79, read by a reviewer and a breaker who both approved | 74 | run 4 at 11:35 on 2026-09-29, all six steps passed, tools\probes\automation-start-result-20260929.txt, scan.md 5z-d RUN 4 |
| F102 | turn 3, the lead's read of fix-F100 | A result committed from this machine can carry the machine name and the licensing agent's ids, and nothing read a file for them before a commit | tools\loop\mask-evidence.ps1 masks both, tools\checks\check-evidence-ids.sh refuses both in the pre-commit and in Actions, proved on the four fix-F100 files, refused before and passed after | DONE on 2026-10-01, merged as it is on Bader's answer Q91 B, its two gaps the known limits F102-L1 and F102-L2 | 76, merged as 4fa1040 | no Navisworks run applies, proof in turn3\f102\proof.txt |
| F105 | loop prompt, Phase 1 item 2 | Four facts off the install nobody had read on this machine: the saved viewpoint members, RemoveFile, Roamer's switches, the Clash Detective report | scan.md 5z-f off five result files, no Navisworks started | answered A by Bader on 2026-10-01, Q89: the three sentences of 5z-f narrowed, the three probe faults F105-R1 to F105-R3 | none, branch fix-F105 at 94a839b | no Navisworks run applies, the prober's reads on 2026-09-29 |
| F103 | loop prompt, Phase 1 item 3 | tools\loop\run.ps1 does not exist | the design in turn3\f103-design.md built, proved by its harness with no Navisworks, then one start with no window | DONE on 2026-10-01, part 1, merged as it is on Bader's answer Q92 B, the nine entries of steps\notes\f103-final-reading.md register row F103-W | 78, merged as 398b910 | two real starts with no window on 2026-09-30, on fix attempts 1 and 2, and the third on a63c284 on 2026-10-01 from 09:06 to 09:18, exit 0, VERDICT RAN, steps\runs\00\item0, 01\item0 and 02\item0 on fix-F103 |
| F104 | loop prompt, Phase 1 item 4 | No check of the workbook against a read of the document that shares no code with the harvest | the design in turn3\f104-design.md, part 1 on fix-F104, check-documents after F103, proved by prove-compare and then by 5a at the baseline | part 1 built at ef1fbdd, read by a reviewer and a breaker, its fixes at ebd8bb7. Part 2, run.ps1 -Mode Documents, committed at 75e5dce with its records at 33732c9, read safe under Q93 by a reviewer and a breaker, turn5\f104p2-reads.txt, its harness 102 passed and 0 failed on the committed code, turn5\f104p2-prove-4.txt. Its first real read waits for the baseline of 2026-10-04 | this pull request | none yet |
| T3-G1 | turn 3, the reviewer's reading of F100 attempt 4 | The probe's last check before each settings write, line 1885, also 1877 and NewRoamers at 1261, takes a process list it could not read as no Roamer, so the writes go on. Silent. Old, in b01ad71 | a list that cannot be read stops every write, in the guard code run.ps1 takes over, with a harness case | open, for F103 | none yet | none yet |
| T3-G2 | the same reading | After the deadline path runs and TerminateProcess returns False, nothing reads the deadline flag before adoption, line 1468, so steps 4 to 6 can run with no watchdog. Old | adoption refused once the deadline path ran, in the guard code, with a harness case | open, for F103 | none yet | none yet |
| T3-G3 | the same reading | A recorded start with start ticks refuses whenever any process holds that pid and its start time cannot be read, whatever its name, line 1239. Loud, refuses too much. Old | the name read as well, in the guard code | open, for F103 | none yet | none yet |
| T3-G4 | the same reading | A named limits line prints what the finalizer's IL does as this run's fact, line 1845, read off run 3 and checked by no code on the run. The shape F2 fixed at the line beside it. Old | the line says it was read off run 3 or reads UNKNOWN | open, for F103 | none yet | none yet |
| T3-G5 | turn 3, the breaker's reading of F100 attempt 4 | MainWindowHandle and MainWindowTitle after adoption, lines 1633 to 1648, read by pid alone. The breaker says MainWindowTitle sends WM_GETTEXT to whatever window holds that pid now, the reviewer that GetWindowText sends none to another process's window under the Win32 contract. Old | measured, then read after the same start ticks check WindowsOf makes | open, for F103 | none yet | none yet |
| T3-P | turn 3, the reviewer's reading of F100 attempt 4 | Polish: a sentence at 395 and 1836 that went stale when B3 landed, the always empty before set still read, the work folder moved aside before step 2 refuses, the gap between the last read and the constructor call not named as a limit | each fixed or named where the code moves in F103 | open, for F103 | none yet | none yet |
| T3-B | turn 3, the F103 design | The build stamp reads +edits whenever the tree holds any untracked file, Directory.Build.targets, so a build from a working clone cannot prove it is main | install from a clean checkout at the commit asked for, the installed stamp read back equal | open, for F103 | none yet | none yet |
| T3-W1 | turn 3, F102's developer | The git wall refuses a push from a worktree while the main clone has main checked out, even git push origin fix-F102:fix-F102, because it reads the main clone's branch. A false refusal on the safe side | the wall reads the branch of the tree the command runs in, proved through tools\loop\prove-hooks.sh and a breaker as the rules say | open, the wall's own round | none yet | none yet |
| T3-G6 | turn 3, the reading of F103 fix attempt 2 | A key under HKCU 22.0 that reads at the start and cannot be read at the end is compared by nothing and counted nowhere, so the put back reads whole and exits 0, nw-guard.ps1 238, 258 and 1239 to 1241 at 867697a. Silent | the key counted as not compared and the verdict saying so, with a harness case | open, for after F103 | none yet | none yet |
| T3-G7 | the same reading | Anything that reads watch.txt with a locking read during a run makes one watchdog append fail, which switches off the whole put back, and the reason blames the watchdog, nw-guard.ps1 837 to 840 and 1176. Silent until the end | a write that fails on a reader retried or told apart, with a harness case | open, for after F103 | none yet | none yet |
| T3-G8 | the same reading | ProcState turns a failed Get-Process into gone, and the put back guards read Get-Process -Name with errors silenced, so a process list that cannot be read at the end lets the put back run, nw-guard.ps1 522, 523, 773, 1182 to 1184, 1197 and 1208. The family of T3-G1 | an unreadable list read as UNKNOWN that stops the writes, with a harness case | open, for after F103 | none yet | none yet |
| T3-G9 | the same reading | AdoptStart runs after the constructor returned and before the pid is held, where neither the constructor deadline nor the call bound applies, so a hung process list or CIM read there has no bound, nw-guard.ps1 1091 to 1145 | a bound on that stretch, with a harness case | open, for after F103 | none yet | none yet |
| T3-G10 | the same reading | M5 walks %TEMP% and the Autodesk folders with no time limit, run.ps1 1062 to 1076, and the put back waits on it | a bound on the walk, written when it is reached | open, for after F103 | none yet | none yet |
| T3-G11 | the same reading | build\install.ps1 deletes recursively at 130 and 137 with no check for a junction inside the bundle, and on a first install with nothing moved aside a failed copy leaves a partial bundle at the load path, 134 | both refused or put right, with harness cases | open, for after F103 | none yet | none yet |
| T3-G12 | the same reading | Verdict wording: HUNG, CallForced and CEILING come before the end state, UNKNOWN wins when the end state later reads gone, one unreadable sample ends the hold, RAN with FORCED exits 0, a foreign Roamer that ran and changed no setting shows only in the put back reasons and never in the verdict, and $sync.LogAmbiguous and $sync.ToolLog are set and never read, run.ps1 446 to 455, 541 to 546, 552, 555 and 1015 | each verdict naming the end state, and the two fields read or removed | open, for after F103 | none yet | none yet |
| T3-G13 | turn 3, the final reading of F103, steps\notes\f103-final-reading.md, classed old by the reviewer and a breaker | run.ps1 903, 1014 and 1020 at a63c284 write an exception's text, which holds the full path of a file of Bader's, into the record unmasked. Run's record, where 1014 and 1020 write, is copied into steps\runs, and Install's, where 903 writes, stays in the loop folder. The kind item 12 of fix list 3 fixed, at lines it did not name. Neither real start's record holds such a line | the text masked the way install.ps1's REFUSED lines are, with a harness case | open, for after F103 | none yet | none yet |
| T3-G14 | the same reading, classed old by a breaker | A run.ps1 or install.ps1 killed or hung between the move aside and the removal leaves the load path empty or holding part of the new bundle, and the old bundle whole at .replaced-<time>, with no line saying so and nothing that puts it back, and run.ps1 923 waits on install.ps1 with no limit, install.ps1 119 to 260 at a63c284. Silent | that state named by Check and put right or refused by the next Install, and a bound on the wait, with harness cases | open, for after F103 | none yet | none yet |
| T3-G15 | the same reading, classed polish by both breakers, BundleLeftovers is attempt 3's own code | Install's leftovers: a failed removal of the old bundle after every check prints one LEFT line and exits 0, which INSTALL.md reads as success in a direct install, BundleLeftovers names old .replaced- and .failed- folders as the add-in installed before, so one stale folder makes every later Install exit 5, a wrong stamp returns before a leftover is named, a removal that stops part way leaves part of the old bundle, and the checks that gate the removal read the staging copy, run.ps1 784 to 789 and 947 to 948, install.ps1 178 to 262 at a63c284 | each named or refused with a harness case, and INSTALL.md's success line reading the LEFT line | open, for after F103 | none yet | none yet |
| T3-G16 | the same reading, classed polish and old by a breaker, CloseAtEnd's text is attempt 3's own code | A close that fails twice: CloseAtEnd writes CLOSED here when its own Kill also failed, run.ps1 493 at a63c284, and CloseOwn refuses any folder whose record holds a VERDICT line, run.ps1 687, so only a person can end that Navisworks. The verdict's side of it is T3-G12 | the text saying which close failed, and a closer for a process still running, with a harness case whose Kill fails | open, for after F103 | none yet | none yet |
| T3-P2 | the same reading, classed polish or old by two breakers, the probe's close left unclassed, OwnerText is attempt 3's own code | Polish: CloseOwn reads autosave-before.txt with no count or name check, dotnet build-server shutdown stops every build server of the account, an owner window destroyed mid read is written as another process's, item 8's harness case sets Forced by hand, and the probe closes outside the lock, each with its lines in steps\notes\f103-final-reading.md | each fixed or named where the code next moves | open, for after F103 | none yet | none yet |
| Q82 to Q87 | turn 3, the F103 design | The log folder, the hang rule's zero, a ceiling, the screen, AutoSave, the remembered folders | Bader's answers under each in steps\02_questions.md | answered by Bader on 2026-10-01, written into .claude\rules\loop.md in turn 4, kept in code by F106, and his logs put back at the close | none yet | none yet |
| Q88 | turn 3, F102's developer | Main names the machine of 2026-09-19 in 9 places across 4 files, and PR 74's refs on GitHub keep the licensing ids of its first commits | Bader's answer in steps\02_questions.md | answered by Bader on 2026-10-01: the older name masked on main by F107, the licensing ids register row Q88-IDS | none yet | none yet |
| F106 | turn 4, Bader's message of 30 Sep, Q94 | No run of main goes through the tool's window, and tools\probes\drive-window-run.ps1 finds windows among every window on the desktop and picks a tolerance with a real mouse click | the smallest change to run.ps1 for items 1 to 5 around the driver, read by the reviewer and one breaker under Q93, then the first run of set 03 | DONE for item 1, merged as 3449521 on 2026-10-04. Items 2 to 5 have not run, set 05 is their first run | 88 | set 03 item 1 C06 on 2026-10-01, steps\runs\03\item1-C06 |
| F107 | turn 4, Q88 | Main names the machine of 2026-09-19 on 9 lines in 4 files at 821ed6e, 8 lines in 3 files at 3449521 once F106 had masked the driver's | git grep for the name over the whole tree finds nothing | DONE in its pull request, every search after the last merge of main finding nothing, turn5\f107-grep-after-merge-3.txt | this pull request | no run applies |
| F109 | turn 4, the install of main refused three times on 2026-10-01 | build\install.ps1 since F103 moves the installed bundle aside by one rename, and Windows denies that rename on this machine with no Navisworks running, while every file of the bundle opens for delete and for write, no process of this user holds or maps one, and the loop's own throwaway folders and copies of the bundle rename freely in the same folder. What holds it is UNKNOWN. The refusal says Navisworks is running, which it was not. Loud, and it blocks every install, here and on any of the 27 machines where the same happens | the in-place path of turn4\f109-brief.md built and read, then the real install of main here reading the installed stamp back | open, turn 4, with its developer | none yet | none yet |
| F108 | turn 4, Bader's message of 30 Sep | No fresh copy of the source for each run set existed, in his folder shape | tools\loop\prepare-copy.ps1 -Set NN, proved on the real folder with throwaway set numbers, turn4\f108-proof.txt | DONE, merged as c9b223b on 2026-10-04 | 84 | set 03's copy made with it from its branch on 2026-10-01, turn4\copy-set03.txt |
| F108-R1 | the breaker of F108 | -Set NN -Remove and -Restore refuse a file whose name sits in both communities, which is all three files of group 100000, prepare-copy.ps1 lines 180 to 187 | the file named with its community, or the group for runs 3 and 4 chosen outside 100000 | open, register row under Q93 | none yet | none yet |
| F108-R2 | both readers of F108 | A short or changed source.manifest.txt is kept until the desktop folder next changes, so every -Set copies 208 MB, fails its read back and advises the next set number, which fails the same way, lines 290 to 308 and 342 to 350 | the keep check reading the manifest too | open, register row under Q93 | none yet | none yet |
| F108-R3 | both readers of F108 | -Remove writes its note before the delete, so a failed delete leaves a note for a file still there, lines 229 to 231, the order the source copy's -Remove has had since 2026-09-27 | the note written after the delete | open, register row under Q93 | none yet | none yet |
| F108-R4 | both readers of F108 | Words: the README's write list names only the source copy, the runner agent describes the plain -Remove and -Restore only, a comment and the README say Windows PowerShell 5.1 follows a junction when it recurses, which the developer's own test on 5.1 did not show, UNKNOWN which is right, the two digit set rule is written in prepare-copy.ps1 and run.ps1 in two forms, and -Remove on a copy that failed its read back points at a -Set that refuses | each made true where its file next changes | open, register row under Q93 | none yet | none yet |
| F108-R5 | the breaker of F108 | -Set copies whatever the desktop folder holds under NWF, NWD and Clash Report, so outputs of Bader's own runs there land in the copy and its manifest as inputs. Nothing in it reads for a running Navisworks, which reads only | F106's item 1 refusing output folders that hold any file, which it is built to do | open, register row under Q93 | none yet | none yet |
| F107-R1 | the breaker of F107 | Once masked, nothing in the repo refuses the old machine name, because F102's mask and check read for this machine's name only, and copies of files holding it sit in the loop folder | a rule that reads for it without writing it into the repo, UNKNOWN how | open, the fix list's FR-110 and Bader's Q106 | none yet | no run applies |
| F107-R2 | the breaker of F107 | F106 rewrites the header of tools\probes\drive-window-run.ps1 whose line 21 F107 masks, so when F106 merges main the conflict must keep the masked words, or the name comes back and every check stays green | the lead greps for the name after F106 merges main | done: F106 merged first, F107 took main's driver whole, and the searches after F107's last merge of main find nothing | none | no run applies |
| F102-L1 | Q91 B, the fourth reading of F102 | A workbook under samples with text appended inside its last 65557 bytes passes the evidence check unread, where every real one holds the zip's end record 22 bytes from its end | none, a known limit: the samples are never edited | known limit, Q91 B | 76 | no run applies |
| F102-L2 | Q91 B, the same reading | A committed hook holding the evidence check's line where it never runs, after an exit or in a function nobody calls, still gets the handover | none, a known limit: a hook of this repo is read before it merges | known limit, Q91 B | 76 | no run applies |
| F103-W | Q92 B, steps\notes\f103-final-reading.md | The nine entries in the words of F103, eleven places in six files, item 14 of fix list 3 among them | each entry's words made true where its file next changes | open, words only, Q93 | 78 | no run applies |
| F105-R1 | Q89 A, the fourth reading of F105 | tools\probes\il-reader.ps1 prints a zero for every failure kind in every probe, kinds a probe never counts included | each probe printing only the kinds it counts | open, register row under Q93 | none yet | no run applies |
| F105-R2 | the same reading | One of F105's probes prints its failure list only when a built add-in is there | the list printed on every run | open, register row under Q93 | none yet | no run applies |
| F105-R3 | the same reading | A resolver, roResolve, and a small helper, ParamText, sit in more than one of F105's probes, and a second helper the reading named is UNKNOWN | one copy of each | open, register row under Q93 | none yet | no run applies |
| F105-R4 | the reviewer of F105's words round, 2026-10-04 | Comments in the probes say nothing is dropped, il-reader.ps1 line 19, probe-clash-report-api.ps1 line 23 and probe-viewpoint-calls.ps1 lines 29, 30 and 396, printed at its result line 203, where a read handed to no IlFail is in no count | the comments made true where the probes next change, no probe or result file changing after its run | open, register row under Q93 | none yet | no run applies |
| F104-R1 | the reviewer of F104 part 2, 2026-10-04 | A documents read ends READ when nothing was read, run.ps1 line 1547, while its VERDICT, exit 1 and the steps that did not run say STOPPED | another word for that end, or the read-out count on summary line 1014 | open, register row under Q93 | none yet | no run applies |
| F104-R2 | the same reviewer | The picture and priority switches of a documents read are typed by hand, so a wrong -PriorityPicked turns off the block order check and a wrong -PictureStatuses judges the wrong rule, while the window run's log names the priority file, its line 64 | run.ps1 reading them off the window run's log and refusing a mismatch | open, register row under Q93 | none yet | no run applies |
| F104-R3 | the breaker of F104 part 2 | A group that wrote no NWF and no workbook is in no count, so a read can say RAN, exit 0, with fewer pairs than groups, and a read where every workbook is missing compares nothing and exits 0, run.ps1 763 to 778, 807 to 809, 1029, 2434 and 2443 to 2445 | the summary counting every group of the tsv and naming each not compared | open, register row under Q93 | none yet | no run applies |
| F104-R4 | the same breaker | AGREE can stand while the panel's count differs when a test holds a result group, PQ4, compare-document.ps1 541, 680 to 687 and 704 to 711, the one place a green line can sit on Bader's third test | PQ4 measured, then the -GroupClashesAt switch set from it | open, register row under Q93, PQ4 | none yet | none yet |
| F104-R5 | the same breaker | The probe's viewpoint walk is unmeasured and unbounded, 1165 viewpoints in 1B06G1 of C06, and on a busy processor only the 12 hour ceiling ends it | the walk timed on the first real read | open, register row under Q93 | none yet | none yet |
| F104-R6 | the same breaker | A documents read opens every NWF, so Bader's Recent File List fills with the loop's NWFs, put back only when no Navisworks of his ran meanwhile, and each open adds a Windows Recent link that M5 lists and never puts back | the first real read listing what it changed there | open, register row under Q93 | none yet | none yet |
| F104-R7 | both readers of F104 part 2 | The other notes: summary.txt written after the VERDICT, prove-run.ps1 line 281 failing on the new mode, pictures and read-outs not pinned to the window run, checks 7, 8 and 10 not run, the header claiming less than it writes, three lines of the finally outside a try, ReadPairs before the lock, four ties not made, the probe call unbounded if the monitor dies, a probe file throw read as a hang, compare-document.ps1's error text dropped, a picture status typo found late, a locked session not checked, and three loud nuisances, each with its line in turn5\f104p2-reads.txt | each made true where its file next changes | open, register rows under Q93 | none yet | no run applies |
| Q88-IDS | Q88 | GitHub keeps the commits 464f79f and c98c6f3 readable through PR 74's own refs, and they hold the licensing agent's analytics id and a session id | only the repository's owner can ask GitHub support to purge them | known item, Bader's, the repository is private | 74 | no run applies |
| D1 | loop prompt, the defaults | One public type per file, 46 files hold more than one top level type, steps\loop-read.md section 2 | core.md and addin.md say it, every file split, moves only, build, Core tests and a first run | open, after the faults | none yet | none yet |
| RUN-1637 | Bader's run of 2026-09-27 16:37, run-20260927-163731.log in his logs folder | The run ended at 17:00:36 on the second NWF save into NM Fed, no RESULT, no workbook, no NWD, on build be0b9b37 | the baseline first run of main writes RESULT, the workbook and the NWD for every group, with its NWF saved twice | seen on an old build, the baseline answers it for main | none yet | none yet |
| CHAT-19 | chat audit of 19 Sep | The doubled summary block above BuildViewpoints | NOT FOUND by the turn 1 read. Two other stacked summaries are T1-N items | closed, not there at 42499bf | none yet | none yet |
| F50 | chat audit of 19 Sep, also 01_next steps/01_next.md line 605, DONE at line ..., also log.md steps/log.md line 3126 (What ... | Model remove without a clear, said to be behind a switch | the turn 1 read found NO switch, every CHANGED group reshapes first and the log names the path. Proved by run 3 and run 4 | done in code, not proved by a run | none yet | none yet |
| F52 | chat audit of 19 Sep, also 01_next steps/01_next.md line 617, DONE at line ... | Viewpoints behind SavedViewpoints.CanBuild | the read found CanBuild true and READ BY NOTHING, T1-B2. Proved by the VIEWS block on run 1 | done in code, not proved by a run | none yet | none yet |
| F88 | 01_next steps/01_next.md line 88, DONE at line 9 ... | Restore the sample thirteen tests read | The thirteen Core tests that read the sample pass, and the DONE line records that. No Navisworks run applies to a restored sample. | done in code, not proved by a run | none yet | none yet |
| F87 | 01_next steps/01_next.md line 95, DONE at line 1 ... | Correct the matrix, the hyphen and BLD-EL-Devices | A run with exchange/1104-PAR_CLASH_AllInOne_25mm_FIXED.xml where the renamed set and BLD-EL-Devices resolve. Whether a negated condition imports is still UNKNOW ... | done in code, not proved by a run | none yet | none yet |
| F72a | 01_next steps/01_next.md line 103, DONE at line ... | Pipe Insulation is a service, and the matrix test | A penetration run where an insulated pipe and its insulation both move to Reviewed. The two arguable categories are Q47, Bader's. | done in code, not proved by a run | none yet | none yet |
| F73 | 01_next steps/01_next.md line 110, DONE at line ... | Appending brings the viewpoints in, which is not a fault | A real run where APPEND brings in viewpoints and the group still ends DONE. steps/log.md lines 2569 to 2574 record ten groups DONE with no CENSUS CHANGED on the ... | done in code, not proved by a run | none yet | none yet |
| F84 | 01_next steps/01_next.md line 188, DONE at line ... | The sets that cannot match anything | The Revit category list measured on the local machine, which steps/log.md line 2551 records as 5i and not measured. After that, a run where the category check n ... | done in code, not proved by a run | none yet | none yet |
| F78 | 01_next steps/01_next.md line 194, DONE at line ... | The log says or where the file says or | A real run whose SETS lines show OR where the file groups conditions. The section cites none. | done in code, not proved by a run | none yet | none yet |
| F79 | 01_next steps/01_next.md line 200, DONE at line ... | Which missing item ids are this run's | steps/log.md line 2634 says this split was first read off the 21:13 run, on 192 carried over ids in 1A02MM. This section does not cite that. | done in code, not proved by a run | none yet | none yet |
| F80 | 01_next steps/01_next.md line 206, DONE at line ... | The run time is the run and not the session | A real run whose RESULT block carries both numbers. The section marks this LOCAL MACHINE and cites no run. | done in code, not proved by a run | none yet | none yet |
| F81 | 01_next steps/01_next.md line 212, DONE at line ... | The .log is trimmed and the .tsv keeps everything | Both file sizes in a real run's RESULT block, with the .tsv holding every row. The section says LOCAL MACHINE to measure the file and cites no measurement. | done in code, not proved by a run | none yet | none yet |
| F82 | 01_next steps/01_next.md line 218, DONE at line ... | Sets across the run | The sets across the run block on a real run. The section cites none. | done in code, not proved by a run | none yet | none yet |
| F47a | 01_next steps/01_next.md line 228, DONE at line ... | The hooks do not run on a Windows checkout | On a Windows checkout, the hooks refuse only the protected calls, which is D7 in steps/03_bader_next.md. The line's own proof ran on the machine where the fix w ... | done in code, not proved by a run | none yet | none yet |
| F47b | 01_next steps/01_next.md line 236, DONE at line ... | One doubled comment left | The stacked comment check reads 0, and that is the whole proof. No run applies to a comment. | done in code, not proved by a run | none yet | none yet |
| F47c | 01_next steps/01_next.md line 244, DONE at line ... | Two names recorded wrongly in the F40 entry | Core tests 957 passed after the change, as recorded. No run applies, and the miscount is Q31. | done in code, not proved by a run | none yet | none yet |
| F5 | 01_next steps/01_next.md line 258, line 263 | Fix the sets built test | A run on the local machine where the sets built count is right, which closes B1. The section names no Look for line. | done in code, not proved by a run | none yet | none yet |
| F6 | 01_next steps/01_next.md line 265, DONE at line ... | Fix the open file report folder | An open file run that writes its report into the right folder. The section cites none. | done in code, not proved by a run | none yet | none yet |
| F7 | 01_next steps/01_next.md line 273, DONE at line ... | Write the RESULT block for the open file run | An open file run whose log carries a RESULT block. The section says LOCAL MACHINE ONLY to prove and cites no run. | done in code, not proved by a run | none yet | none yet |
| F8 | 01_next steps/01_next.md line 281, line 286 | Run the existing tests when no XML is picked | A weekly run with no XML picked that runs the tests saved in each NWF. | done in code, not proved by a run | none yet | none yet |
| F22 | 01_next steps/01_next.md line 288, line 297 | Two clear workflows in the window | The five labels seen per group in the window, the confirm dialog and the log. steps/log.md line 2569 reports First run and Weekly run plus XML groups on the 21: ... | done in code, not proved by a run | none yet | none yet |
| F1 | 01_next steps/01_next.md line 308, DONE at line ... | Fix the test name typo | The renamed test compiling and passing. No run applies. | done in code, not proved by a run | none yet | none yet |
| F2 | 01_next steps/01_next.md line 314, DONE at line ... | Fix the hardcoded probe path | A local run of tools/probes/probe-window-defaults.ps1. | done in code, not proved by a run | none yet | none yet |
| F4 | 01_next steps/01_next.md line 327, DONE at line ... | Fix the units docstring | This is a docstring, so there is nothing to run. | done in code, not proved by a run | none yet | none yet |
| F9 | 01_next steps/01_next.md line 333, DONE at line ... | Skip the units change on a CHANGED group | A CHANGED group on the local machine that skips the units change. | done in code, not proved by a run | none yet | none yet |
| F10 | 01_next steps/01_next.md line 339, DONE at line ... | Gate the page on its own flag | A run where the page is written or skipped on its own flag. | done in code, not proved by a run | none yet | none yet |
| F11 | 01_next steps/01_next.md line 345, DONE at line ... | Remove the dead code | An add-in build. F69 at line 813 records the first one, with 0 errors and 0 warnings on 2026-09-19. | done in code, not proved by a run | none yet | none yet |
| F17 | 01_next steps/01_next.md line 351, DONE at line ... | Picture numbering by block order | A run whose picture names follow the block order of the Navisworks export. | done in code, not proved by a run | none yet | none yet |
| F16 | 01_next steps/01_next.md line 387, DONE at line ... | Make the tests path neutral | The section asks for the Core tests to be confirmed on Windows. Later DONE lines such as F65's at line 770 record 1238 passed, 0 failed, 0 skipped on Windows. | done in code, not proved by a run | none yet | none yet |
| F18 | 01_next steps/01_next.md line 396, order line 37, also log.md steps/log.md lines 478-479, 935-936, 115 ... | Add the 1A04WE sample | The 1A04WE sample committed under samples/client-report. A Glob there found only 1104-PAR-1A04WN-XXX-BM-RPT-000001.html and .xlsx, and whether that is the file ... | needs Bader, in the form | none yet | none yet |
| F20 | 01_next steps/01_next.md line 403, DONE at line ... | Run the Core tests on every push to main | A passing Actions push run on main. None is quoted here. | done in code, not proved by a run | none yet | none yet |
| F24 | 01_next steps/01_next.md line 411, DONE at line ... | Rebuild a CHANGED NWF from the scan | A CHANGED group rebuilt on the local machine with its saved tests kept. F90 at line 878 later updates a CHANGED group without clearing it, and this file does no ... | done in code, not proved by a run | none yet | none yet |
| F26 | 01_next steps/01_next.md line 418, DONE at line ... | Units always meters | A run whose report reads in meters on every group. | done in code, not proved by a run | none yet | none yet |
| F27 | 01_next steps/01_next.md line 427, DONE at line ... | The GROUPS block tells the truth | The GROUPS block of a real run. | done in code, not proved by a run | none yet | none yet |
| F28 | 01_next steps/01_next.md line 434, DONE at line ... | A set already there is not counted as created | A weekly run where the second NWF save does not fire. | done in code, not proved by a run | none yet | none yet |
| F29 | 01_next steps/01_next.md line 441, DONE at line ... | The rebuild keeps the sets on their own count | A rebuild on the local machine that restores the sets on their own count. | done in code, not proved by a run | none yet | none yet |
| F30 | 01_next steps/01_next.md line 448, DONE at line ... | One tail for both run paths | Both run paths on the local machine writing through the one tail. | done in code, not proved by a run | none yet | none yet |
| F31 | 01_next steps/01_next.md line 455, DONE at line ... | The clash side lookup is built once per run | A run on the local machine. The section gives no specific Look for line. | done in code, not proved by a run | none yet | none yet |
| F32 | 01_next steps/01_next.md line 462, DONE at line ... | The open file is guarded | Seeing the refusal on the local machine for each refused path. | done in code, not proved by a run | none yet | none yet |
| F33 | 01_next steps/01_next.md line 469, DONE at line ... | One unit table | A run on the local machine that exercises the engine and the window. | done in code, not proved by a run | none yet | none yet |
| F34 | 01_next steps/01_next.md line 476, DONE at line ... | Window wiring | The window and a run on the local machine. | done in code, not proved by a run | none yet | none yet |
| F35 | 01_next steps/01_next.md line 483, DONE at line ... | Clash only where two disciplines meet | A single discipline group showing its row status and its CLASH line on a run. | done in code, not proved by a run | none yet | none yet |
| F36 | 01_next steps/01_next.md line 490, DONE at line ... | Dead code and copies out | Grep and the build only. No run applies. | done in code, not proved by a run | none yet | none yet |
| F37 | 01_next steps/01_next.md line 497, DONE at line ... | One type per file and one place per kind of file | Moves only, so tests and the build are enough. No run applies. | done in code, not proved by a run | none yet | none yet |
| F38 | 01_next steps/01_next.md line 504, DONE at line ... | CLAUDE.md under 200 lines, rules in .claude, walls in hooks | The hooks were tried with piped tool calls, as recorded. No run applies. | done in code, not proved by a run | none yet | none yet |
| F39 | 01_next steps/01_next.md line 511, DONE at line ... | The window compiles again | An add-in build. F69 at line 813 records one with 0 errors on 2026-09-19. | done in code, not proved by a run | none yet | none yet |
| F46 | 01_next steps/01_next.md line 520, DONE at line ... | The four the chat audit found | Docs and one test. No run applies. | done in code, not proved by a run | none yet | none yet |
| F40 | 01_next steps/01_next.md line 533, DONE at line ..., also steps/04_audit.md:63, also steps/04_audit.md:64 | Dead members out, second pass | Core tests and the add-in build. Q24, Q25, Q26 and Q27 stay with Bader. | done in code, not proved by a run | none yet | none yet |
| F41 | 01_next steps/01_next.md line 543, DONE at line ... | Every handle disposed | Line 549 gives the proof: one building run twice with the XML, where the SETS and CLASH blocks read as before and the run is not slower. | done in code, not proved by a run | none yet | none yet |
| F42 | 01_next steps/01_next.md line 553, DONE at line ... | No framework message in a label | The window on the local machine showing no framework message in any label. | done in code, not proved by a run | none yet | none yet |
| F43 | 01_next steps/01_next.md line 563, DONE at line ... | Three settings that are constants | A run on the local machine. What sets the three settings is Q29. | done in code, not proved by a run | none yet | none yet |
| F44 | 01_next steps/01_next.md line 572, DONE at line ..., also steps/04_audit.md:70, also steps/04_audit.md:98, also steps/04 ... | The docs and the comments agree with the code | Docs and comments only. The one path left over is Q30. | done in code, not proved by a run | none yet | none yet |
| F45 | 01_next steps/01_next.md line 581, DONE at line ... | The clash step keeps its rules | Line 585 asks for a local machine run to prove the compact count. | done in code, not proved by a run | none yet | none yet |
| F51 | 01_next steps/01_next.md line 596, DONE at line ... | The ACC warning | An NWD published by the tool reaching ACC with no processing error, which line 599 says is LOCAL MACHINE ONLY. | done in code, not proved by a run | none yet | none yet |
| F53 | 01_next steps/01_next.md line 629, DONE at line ... | The 150 mm rule and the sub groups | A run showing the SIZE block, with the large pipes in their sub groups. | done in code, not proved by a run | none yet | none yet |
| F54 | 01_next steps/01_next.md line 640, DONE at line ... | Clashes that cannot be solved become Reviewed | F72 at line 845 routes its status moves through this editor, and F72b at line 171 cites clashes moved on the 21:13 run. F54's own line cites no run. | done in code, not proved by a run | none yet | none yet |
| F55 | 01_next steps/01_next.md line 652, DONE at line ... | The rules and the docs catch up | Docs only. No run applies. | done in code, not proved by a run | none yet | none yet |
| F56 | 01_next steps/01_next.md line 660, DONE at line ... | What the real read of 03_bader_next.md found | Docs and one log label with its tests. No run applies. It is not listed in the order at the top. | done in code, not proved by a run | none yet | none yet |
| F57 | 01_next steps/01_next.md line 668, DONE at line ... | Five Look for lines older than the feature round | Docs only. The heading says five lines, but the order and the body say six. | done in code, not proved by a run | none yet | none yet |
| F58 | 01_next steps/01_next.md line 680, DONE at line ... | The add-in compiles again | The add-in build, which F69 at line 813 later records at 0 errors. | done in code, not proved by a run | none yet | none yet |
| F59 | 01_next steps/01_next.md line 691, DONE at line ... | Every step is named and timed | A real run that writes the step lines. The section cites none. | done in code, not proved by a run | none yet | none yet |
| F60 | 01_next steps/01_next.md line 701, DONE at line ... | The timing blocks, and F21 closes here | The TIMING blocks on a real run. steps/log.md line 2570 reports 4 minutes 54 seconds for the 21:13 run, but this section does not cite it. | done in code, not proved by a run | none yet | none yet |
| F61 | 01_next steps/01_next.md line 713, DONE at line ... | The document census | CENSUS lines on a real run. F75's proof at line 131 shows census lines on the 21:13 run, but F61 does not cite it. | done in code, not proved by a run | none yet | none yet |
| F62 | 01_next steps/01_next.md line 724, DONE at line ... | The live line in the window | Watching the live line move on the local machine. | done in code, not proved by a run | none yet | none yet |
| F63 | 01_next steps/01_next.md line 737, DONE at line ... | The report gap block | The GAP block on a real run. Q35 to Q40 are Bader's. | done in code, not proved by a run | none yet | none yet |
| F64 | 01_next steps/01_next.md line 749, DONE at line ... | The machine readable log | A .tsv beside a real run log. steps/log.md line 2565 says the 21:13 run has one, but this section does not cite it. | done in code, not proved by a run | none yet | none yet |
| F65 | 01_next steps/01_next.md line 761, DONE at line ... | The missing import | The add-in build. F69 at line 807 says F65 fixed the error Bader sent, and line 813 records the build at 0 errors. | done in code, not proved by a run | none yet | none yet |
| F66 | 01_next steps/01_next.md line 772, DONE at line ... | The check that would have caught it | The check refusing the broken folder, as recorded. No run applies. | done in code, not proved by a run | none yet | none yet |
| F67 | 01_next steps/01_next.md line 784, DONE at line ... | One doubled comment | The stacked summary check reads 0. No run applies. | done in code, not proved by a run | none yet | none yet |
| F68 | 01_next steps/01_next.md line 793, DONE at line ... | The build section learns what today cost | A real NuGet restore race recovered by following steps 11 to 21. The commands were run, but no race was recovered on record. | done in code, not proved by a run | none yet | none yet |
| F71 | 01_next steps/01_next.md line 825, DONE at line ... | Say when an NWF is nearly matched | Line 828 asks for it on the local machine: a group beside a nearly matching NWF showing the sentence in the Run as column. | done in code, not proved by a run | none yet | none yet |
| F90 | 01_next steps/01_next.md line 878, DONE at line ... | A CHANGED group is brought up to date without clearing it | A run that updates a CHANGED group, with the model count going out and coming back equal. F94 at line 906 says this path once deleted a model silently and repor ... | done in code, not proved by a run | none yet | none yet |
| F91 | 01_next steps/01_next.md line 884, DONE at line ... | The run tail carries the clash total and how many decisions the run made | A real run's RESULT block showing the clash total, with LOOKED AT beside MOVED. The section cites none. | done in code, not proved by a run | none yet | none yet |
| F94 | 01_next steps/01_next.md line 902, DONE at line ... | The reshape's three defects, and the one rule for a document that failed after it was changed | A run where a reshape or a set pair fails, showing the model count out and back and the damaged document rule. No run after the fix is cited. | done in code, not proved by a run | none yet | none yet |
| F96 | 01_next steps/01_next.md line 914, line 918, ord ..., also log.md steps/log.md lines 374-380 and 477 (clos ... | The restated facts check | tools/checks/check-restated-facts.sh exists and refuses a broken restatement of each of the four named facts: the solid list, the 150 mm threshold, the tick box ... | open fault | none yet | none yet |
| Q67 | 01_next steps/01_next.md line 870, also log.md steps/log.md lines 1799-1803 (Known bugs ... | Question 67, subject not stated in 01_next.md | Bader's answer recorded under it in steps/02_questions.md. The question itself is not quoted in 01_next.md. | needs Bader, in the form | none yet | none yet |
| Q68 | 01_next steps/01_next.md line 870 | Question 68, subject not stated in 01_next.md | Bader's answer recorded under it in steps/02_questions.md. The question itself is not quoted in 01_next.md. | needs Bader, in the form | none yet | none yet |
| Q9 | steps/02_questions.md:39 | Commit the 1A04WE client export (F18) | The 1A04WE export committed under samples/client-report and read by the tests that name it. | needs Bader, in the form | none yet | none yet |
| Q15 | steps/02_questions.md:63 | The local proofs P1 to P3 | A recorded Clash Detective panel count for three tests equal to their workbook block and ROWS line. A run over every ticked group whose run TIMING block reads i ... | needs Bader, in the form | none yet | none yet |
| Q24 | steps/02_questions.md:113, also log.md steps/log.md lines 4516 (What remains, t ... | Give a typed name cell back to the pattern, or drop the rule | Bader's answer, then a Grouping step control that calls ReleaseToPattern, or the core.md sentence and the member removed with its test. | answered by Bader on 2026-10-04, find the mistake first, FR-160 in wave 4 | none yet | none yet |
| Q26 | steps/02_questions.md:123, also log.md steps/log.md line 4871 (What remains, F4 ... | Members made internal because only a test reads them | Bader's answer, then the internal members kept with the decision written in a rule, or each deleted with its test. | answered by Bader on 2026-10-04, find the mistake first, FR-172 in wave 5 | none yet | none yet |
| Q27 | steps/02_questions.md:127, also log.md steps/log.md line 4871 (What remains, F4 ... | Keep or drop the two choice open count rule | Bader's answer, then core.md keeping the rule with its readers named, or the bullet and the unused choice removed. | needs Bader, in the form | none yet | none yet |
| Q28 | steps/02_questions.md:131, also log.md steps/log.md line 4841 (What remains, F4 ... | Release the two handle reads F41 left | Bader's answer, and if release is chosen, both reads in using blocks and a run with no ObjectDisposedException. | needs Bader, in the form | none yet | none yet |
| Q29 | steps/02_questions.md:135, also log.md steps/log.md line 4785 (What remains, F4 ... | A settings file for the stop after count, log count and report subfolder | Bader's answer, then a file read once when the log opens, with a refused value named in the log. | needs Bader, in the form | none yet | none yet |
| Q30 | steps/02_questions.md:139, also log.md steps/log.md line 4756 (What remains, F4 ... | May the hand written bundle manifest be edited | Bader's answer, then line 9 naming docs\history\scan.md, or the never edited rule restated to keep it. | needs Bader, in the form | none yet | none yet |
| Q31 | steps/02_questions.md:143, also log.md steps/log.md line 4516 (What remains, th ... | Restate the F40 counts of 45 and 64 | Bader's answer, and the F40 entry in steps/log.md restated by the chosen rule or left with its marks. | needs Bader, in the form | none yet | none yet |
| Q35 | steps/02_questions.md:165 | Keep or delete the Family harvest | Bader's answer, then a column of ours that carries it, or the property and its harvest deleted. | needs Bader, in the form | none yet | none yet |
| Q36 | steps/02_questions.md:169 | Keep or delete the Revit Type Name harvest | Bader's answer, then a column of ours that carries it, or the property and its harvest deleted. | needs Bader, in the form | none yet | none yet |
| Q37 | steps/02_questions.md:173 | Keep or delete the Material harvest | Bader's answer, then a column of ours that carries it, or the property and its harvest deleted. | needs Bader, in the form | none yet | none yet |
| Q38 | steps/02_questions.md:177 | Keep or delete the per item Source File harvest | Bader's answer. Deleting it, or keeping it as a stated always empty value, both need the GAP block question Q76 settled beside it. | needs Bader, in the form | none yet | none yet |
| Q39 | steps/02_questions.md:181 | Keep or delete the per item Discipline harvest | Bader's answer, then the harvest deleted or a place it is written. | needs Bader, in the form | none yet | none yet |
| Q40 | steps/02_questions.md:185 | Where the per item Id From belongs, if anywhere | Bader's answer, then either the block declared enough in core.md or a named place that carries the per item value. | needs Bader, in the form | none yet | none yet |
| Q45 | steps/02_questions.md:206, also log.md steps/log.md lines 3035-3045 and 3126 (W ... | A cell of ours for how many clashes this run moved to Reviewed | Bader's answer, then either the rule stands as written or a named cell of ours added with a workbook check. | needs Bader, in the form | none yet | none yet |
| Q46 | steps/02_questions.md:210 | Create the whole matrix once for a group run weekly with no XML (F77) | Bader's answer, then either the rule stated as it is or a first run that creates all 1830 once. | needs Bader, in the form | none yet | none yet |
| Q47 | steps/02_questions.md:214 | Are Air Terminals and Sprinklers services | Bader's answer, then the two lists changed or kept, with PenetrationRuleTests asserting the decision. | needs Bader, in the form | none yet | none yet |
| Q49 | steps/02_questions.md:222 | Priority order or the measured client order for the blocks | Bader's or the client's answer, then ReportOrder.Tests left or changed to match it. | needs Bader, in the form | none yet | none yet |
| Q50 | steps/02_questions.md:226 | Undo that resets a status, or one that only removes the record | Bader's answer. If the second shape is chosen, the undo removes records and sets no status, proved by a run. | needs Bader, in the form | none yet | none yet |
| Q51 | steps/02_questions.md:230 | Largest size for a viewpoint folder too, or report the difference | Bader's answer, then the reading kept or a reported difference, with a test. | needs Bader, in the form | none yet | none yet |
| Q56 | steps/02_questions.md:254 | The category line should say which folder its list came from (56b) | The HEALTH line naming the folder the 374 values were measured on, pinned by a Core test and seen in a run log. | open fault | none yet | none yet |
| Q76 | steps/02_questions.md:414, also log.md steps/log.md lines 910-918 (close round ... | GAP block cannot tell an output carries it from the run read nothing | Bader's answer, then GapRule saying which it is for a property read on every item that always came back empty, with a test. | needs Bader, in the form | none yet | none yet |
| Q77 | steps/02_questions.md:418, also log.md steps/log.md lines 335-337 and 410-411 ( ... | The clear and rebuild drops the viewpoints | CreateCopy and CopyFrom on the saved viewpoints measured first, then a forced fallback on a copy whose VIEWS row reads the same before the clear and after the r ... | open fault | none yet | none yet |
| Q78 | steps/02_questions.md:422, also log.md steps/log.md lines 550-554 (close round ... | Should a shared site named DEFAULT, in any spelling, fail a group | Bader's answer, then the ALIGNMENT rule changed or stated as report only, with a test. | needs Bader, in the form | none yet | none yet |
| A10 | steps/04_audit_first_run.md:101 | ClashImages comment said ViewpointBuilder writes viewpoints | No run applies. | done in code, not proved by a run | none yet | none yet |
| A11 | steps/04_audit_first_run.md:110 | A missing category resource read as an unmeasured list | Proved by the red test, no run applies. | done in code, not proved by a run | none yet | none yet |
| A12 | steps/04_audit_first_run.md:119 | The set name separator written twice | No run applies. | done in code, not proved by a run | none yet | none yet |
| A13 | steps/04_audit_first_run.md:128 | No priority written twice | No run applies. | done in code, not proved by a run | none yet | none yet |
| A14 | steps/04_audit_first_run.md:137 | The five examples rule written in seven places | No run applies. | done in code, not proved by a run | none yet | none yet |
| A15 | steps/04_audit_first_run.md:146 | MatrixCorrections has no caller in src and nothing at run time says which form a set carries | Either the tool applies the corrections at pick time and logs the outcome lines, or the class moves to tools as the audit said. | open fault | none yet | none yet |
| A17 | steps/04_audit_first_run.md:164 | A test named for nothing being written checked only the plan | No run applies. | done in code, not proved by a run | none yet | none yet |
| A18 | steps/04_audit_first_run.md:173 | A five examples test asserted only a count of eight | No run applies. | done in code, not proved by a run | none yet | none yet |
| A19 | steps/04_audit_first_run.md:182 | Thresholds never tested at the number | No run applies. | done in code, not proved by a run | none yet | none yet |
| A20 | steps/04_audit_first_run.md:191 | The undo said moved on about a clash moved back | No run applies. | done in code, not proved by a run | none yet | none yet |
| A21 | steps/04_audit_first_run.md:200 | The open file run fed the sets tally and wrote no block | An open file run log carrying that line. | done in code, not proved by a run | none yet | none yet |
| A22 | steps/04_audit_first_run.md:209 | One RESULT label a character short | A RESULT block with the by design line in the column. | done in code, not proved by a run | none yet | none yet |
| A23 | steps/04_audit_first_run.md:218 | Thirteen new files hold more than one type | One public type per file, which the loop plans as D1 in steps/log.md:87. | open fault | none yet | none yet |
| NEW-QA | src/Federator.Addin/Engine/SetBuilder.cs:284 | A failed side count reads as zero and lets a leftover set be removed | A failed side read refusing every removal and rename for that document, pinned by a Core test that hands SetLeftovers an uncounted side. | open fault | none yet | none yet |
| NEW-QA | src/Federator.Addin/Engine/SetBuilder.cs:271 | SidesBySetName and WalkForLeftovers hold handles they never dispose | Each read in a using block, and a run with the rebuild box on that logs no ObjectDisposedException. | open fault | none yet | none yet |
| step NEW, steps 9-17 and every step naming C0 ... | 03_bader_next | The file says C06 and the 1B06 groups never existed and are gone from it, yet about twenty steps still name th ... | 03_bader_next.md rewritten against the folders turn 0 measured on this machine (NM Fed C06 22 groups, C07 24 groups, no NWF), with no count derived from the 202 ... | open fault | none yet | none yet |
| step steps 20-21 (F68) | 03_bader_next | What clearing the NuGet cache prints was never measured. | A pasted output of dotnet nuget locals all --clear | done in code, not proved by a run | none yet | none yet |
| step steps 29-34 (F33, F34, D3, F57) | 03_bader_next | No run records the units combo list, its grey line, the missing Outstanding combo or the button row. Step 34 w ... | The window looked at, or probe-window-defaults.ps1 output | done in code, not proved by a run | none yet | none yet |
| step steps 35-37 and the HEALTH half of step ... | 03_bader_next | No committed run log carries a HEALTH block, because every driven run typed the XML path and the block is writ ... | The XML picked through the Browse dialog and a HEALTH block with Tests: 1830, Sets: 61 in the log | done in code, not proved by a run | none yet | none yet |
| step steps 39-48 (F27) | 03_bader_next | Only the zero form of the unticked line has appeared in a run. The 9 or 11 unticked case never ran, and steps ... | A run with one group ticked whose GROUPS block reads N groups unticked | done in code, not proved by a run | none yet | none yet |
| step steps 49-53, 55-60, 63-71 (F24, F29, F50 ... | 03_bader_next | The Rebuilt and reshape path has never run on a real CHANGED group, and no committed log carries a REBUILT or ... | A scanned run over a folder whose NWF differs from the scan, logging REBUILT, RESHAPE, the four kept lines and GROUP finished DONE ... Rebuilt | done in code, not proved by a run | none yet | none yet |
| step steps 62, 185 (F30, F52) | 03_bader_next | The block order is seen on the weekly scanned path, UNITS, ALIGNMENT, EXPORT CHECK, SETS, CLASH, PENETRATION, ... | A rebuild run and an open file run each showing this order | done in code, not proved by a run | none yet | none yet |
| step NEW, EMPTY SETS never written on a first ... | 03_bader_next | The only run with fresh-built sets had 24 and 58 sets at zero and wrote no EMPTY SETS block. The code records ... | A first run where a created set finds nothing writes an EMPTY SETS block and its set finding nothing rows | open fault | none yet | none yet |
| step 75 | 03_bader_next | The confirm dialog wording Weekly run plus XML: 1 and Nothing inside it is cleared. was never read on a run. | A screenshot or reading of the dialog text | done in code, not proved by a run | none yet | none yet |
| step steps 81-82, 86-87 (F31) | 03_bader_next | One building has never been run twice to compare counts and time. The only back-to-back pair on record ran ten ... | Two runs of one building showing equal CLASH counts and the second no slower | done in code, not proved by a run | none yet | none yet |
| step NEW, RESULT counts carry across Run pres ... | 03_bader_next | In one window session the second RESULT block counts both presses' groups. RunLog.RunStarted resets no group r ... | Two Run presses in one session, each RESULT block counting only its own groups | open fault | none yet | none yet |
| step steps 88-96, 100-101 (F35) | 03_bader_next | The OneDiscipline folder was never made or run. The SINGLE DISCIPLINE finding was seen on 1000BS and 1A02MS in ... | The two-copy AR folder scanned and run | done in code, not proved by a run | none yet | none yet |
| step 94, and the finding text at step 92 | 03_bader_next | The Grouping label and the SINGLE DISCIPLINE finding both say every test is created, which has been false sinc ... | Both strings say only the tests whose sides find something are created | open fault | none yet | none yet |
| step steps 102-115 (F26) | 03_bader_next | Every committed run is in Meters, so the Feet model units line never appeared and the panel comparison was nev ... | A run with the combo on Feet and the panel checked in metres against the workbook | done in code, not proved by a run | none yet | none yet |
| step steps 123-136 (F71) | 03_bader_next | No committed run log carries the similar name sentence, and the F71 entry proves it with Core tests only. | The window and GROUPS block with the level changed to L01 over an NWF folder that holds files | done in code, not proved by a run | none yet | none yet |
| step steps 137-146 (F9) | 03_bader_next | A folder that lost a file has never gone through the Rebuilt path on a run. | The REBUILT block naming removed then added, with no UNITS line before it | done in code, not proved by a run | none yet | none yet |
| step steps 147-155 (F10) | 03_bader_next | No run has had the XML on or the images off. | OUTPUTS reading XML on, images off, and the IMAGES skipped line | done in code, not proved by a run | none yet | none yet |
| step steps 160-167 (F17) | 03_bader_next | No record exists of comparing sheet order against the panel, the cd numbering, the links or a .moving file. | The workbook and Clash Detective compared by hand | done in code, not proved by a run | none yet | none yet |
| step NEW, the page test order warning | 03_bader_next | Seven groups of every committed C02 run have a REPORT CHECK saying the page's tests are out of order. log.md, ... | A REPORT CHECK with no wrong order line, or a recorded reason the check is wrong | open fault | none yet | none yet |
| step steps 168-174 (F8) | 03_bader_next | No run has used the tests saved in the NWF with no XML picked. | CLASH    source   tests saved in the document, 1830 of them, no XML picked | done in code, not proved by a run | none yet | none yet |
| step steps 175-178 (F32) | 03_bader_next | The refusal line for an open NWD was never seen. | The window with an NWD open | done in code, not proved by a run | none yet | none yet |
| step steps 179-191 (F6, F30, F8 on the open f ... | 03_bader_next | Run the open file has never been pressed on a recorded run. | An open file run log with the block order, one Clash Reports folder and the no XML source line | done in code, not proved by a run | none yet | none yet |
| step steps 192-198 (F7) | 03_bader_next | The log copy beside the NWF and the RESULT block are proved on scanned runs only. The OPEN FILE block and week ... | An open file run log carrying OPEN FILE and weekly run     : 1 | done in code, not proved by a run | none yet | none yet |
| step steps 209-212 | 03_bader_next | Neither window probe has a recorded run. log.md never names probe-window-labels.ps1. | Both probe outputs pasted, with 4 visible tick boxes | done in code, not proved by a run | none yet | none yet |
| step steps 213-214 (F41) | 03_bader_next | Nobody has read the Task Manager handle counts. | Handle counts at the end of two runs of one building | done in code, not proved by a run | none yet | none yet |
| step 219 (F45) | 03_bader_next | Compacting has never been ticked on a run. | A run with compacting on showing both Resolved counts | done in code, not proved by a run | none yet | none yet |
| step steps 228-233 (F51 in ACC) | 03_bader_next | The NWD's file properties and its ACC upload were never checked, and Bader has put ACC off until the tool is f ... | An NWD in ACC with no processing error and real properties | needs Bader, in the form | none yet | none yet |
| step steps 244-245, 250-252 (F53) | 03_bader_next | No run log since F85 names an Over 150mm folder, and the viewpoints round wrote none. Whether one exists since ... | A VIEWS BUILT path such as A/ME vs ST/Over 150mm, and the viewpoint pressed | done in code, not proved by a run | none yet | none yet |
| step 247 | 03_bader_next | SizeTally is still a public class in src, and only its tests call it. | SizeTally and its tests deleted, or a caller in src | open fault | none yet | none yet |
| step steps 260-262 (F72 tick box) | 03_bader_next | No run records the tick box label, its help line or where it sits. | The window or probe-window-labels.ps1 | done in code, not proved by a run | none yet | none yet |
| step steps 269, 269a | 03_bader_next | The second penetrations line and the second by design line are built in code, but no committed log carries eit ... | A RESULT block showing both lines | done in code, not proved by a run | none yet | none yet |
| step 269c (Q73) | 03_bader_next | WorkbookCheck counted 0 test blocks and shouted a failure over a workbook that held 1,830. The step calls the ... | WORKBOOK CHECK counting one-row blocks, so it agrees with BLOCKS and the run tail | open fault | none yet | none yet |
| step steps 271-274, 277-278 (F72) | 03_bader_next | No record exists of the workbook's Reviewed counts, the clash panel, or an Approved clash kept by hand. | The workbook and panel read after a box-on run | done in code, not proved by a run | none yet | none yet |
| step steps 279-280 (F72 cost) | 03_bader_next | The same building has not been run with the box on and then off, so what the penetration pass costs is still u ... | Two clash step took lines, box on and box off, for one building | done in code, not proved by a run | none yet | none yet |
| step 310 (criterion 3) | 03_bader_next | The three-way count of log, workbook and panel was never recorded. | One test's ROWS count, workbook rows and panel count read and written down | done in code, not proved by a run | none yet | none yet |
| step steps 286-287, 318-324 (F62) | 03_bader_next | Nobody has recorded watching the live line in the window during a run. | The window watched through one group of a multi-group run | done in code, not proved by a run | none yet | none yet |
| step steps 340-343 (D7 pre-commit) | 03_bader_next | No record shows the pre-commit wall switched on and printing its four lines on Bader's machine. | A terminal commit printing the four pre-commit lines | done in code, not proved by a run | none yet | none yet |
| step steps 346-352 (D6) | 03_bader_next | The remote branches have not been deleted, and the step itself says to reread the live list first. | git ls-remote --heads origin returning only refs/heads/main | needs Bader, in the form | none yet | none yet |
| step 364 | 03_bader_next | The mechanical CSV and its PROBE block have not been sent. |  | needs Bader, in the form | none yet | none yet |
| step 377 (F81) | 03_bader_next | The .tsv half holds, but no committed run log is under the 300 KB the step asks for. Whether the bar or the tr ... | A ten-group .log under 300 KB, or the step corrected to a measured bar | open fault | none yet | none yet |
| NEW-LOG | log.md steps/log.md lines 179-183 (What remains ... | INSTALL.md steps 5 and 6, the install command and starting Navisworks, never run | The install command from INSTALL.md prints the success line on a Windows machine and the add-in shows on the Tool Add-ins tab. Turn 0 records a build only, not ... | done in code, not proved by a run | none yet | none yet |
| B13 | log.md steps/log.md line 4981 (Known bugs, roun ... | A CHANGED NWF is left alone when the scan folder holds the right files | The same proof as the F50 reshape item: a real CHANGED group ends with its NWF holding every scanned file and its history kept. The one rebuild ever run, run-20 ... | done in code, not proved by a run | none yet | none yet |
| L2 | log.md steps/log.md line 4982 (Known bugs, roun ... | A CHANGED group still has its model units changed | A run log of a CHANGED group where no UNITS line comes before the REBUILT block. No later entry quotes one. | done in code, not proved by a run | none yet | none yet |
| B14 | log.md steps/log.md line 4981 (Known bugs, roun ... | The report went out in feet because the document did not follow the model units | A run whose report reads Meters (m), with one distance matched against the panel in metres. No later entry quotes that check. | done in code, not proved by a run | none yet | none yet |
| B2 | log.md steps/log.md line 4982 (Known bugs, roun ... | The open file run wrote its reports one folder too deep | One Run the open file press whose reports land in a single Clash Reports folder beside the NWF. No run of that button is recorded. | done in code, not proved by a run | none yet | none yet |
| B11 | log.md steps/log.md line 4982 (Known bugs, roun ... | The open file run wrote no RESULT block and copied no log | One Run the open file press whose log ends with a RESULT block and whose copy sits beside the NWF. | done in code, not proved by a run | none yet | none yet |
| L1 | log.md steps/log.md line 4982 (Known bugs, roun ... | With no clash XML the saved tests in the NWF must run, on the open file and on the scanned run | A run with no XML picked whose CLASH source line reads tests saved in the document and whose workbook is written. Every recorded run picked the matrix XML. | done in code, not proved by a run | none yet | none yet |
| L3 | log.md steps/log.md line 4982 (Known bugs, roun ... | Each output gated on its own flag, the page and pictures no longer riding on the workbook flag | The two runs of step L3, XML on with every image status unticked, then XML off with the statuses on, with the log lines it names. | done in code, not proved by a run | none yet | none yet |
| L4 | log.md steps/log.md line 4982 (Known bugs, roun ... | Pictures numbered in the export order, row N of a block linking to picture N | One group's workbook opened with row N of a block linking picture N in panel order, and one IMAGES numbered in report order line in the log. | done in code, not proved by a run | none yet | none yet |
| B7 | log.md steps/log.md line 4983 (Known bugs, roun ... | probe-window-defaults.ps1 carried a path that exists on one machine only | One run of tools/probes/probe-window-defaults.ps1 on a machine with the add-in built, printing the window defaults. No run of it is recorded. | done in code, not proved by a run | none yet | none yet |
| P2 | log.md steps/log.md line 5944 (What remains, an ... | Criterion 2, every ticked building unattended in under 45 minutes, holds on C02 and fails at C04 scale | A full run of every ticked group in C04, or in NM Fed at 46 groups, whose TIMING block says inside 45. C02 ran in 1041.789s (line 299), ten of C04's twelve grou ... | open fault | none yet | none yet |
| NEW-LOG | log.md steps/log.md lines 245-255 (close round ... | The viewpoints step does not scale, its cost is clashes times models with three read backs each | A VIEWS step on a group the size of 1A04EP near the fixture's 0.03 seconds a viewpoint, with no steady climb in working set. | open fault | none yet | none yet |
| P3 | log.md steps/log.md line 5944 (What remains, an ... | Criterion 3, clash counts in the Excel match the Clash Detective panel exactly | One NWF opened, the panel count read for three tests and matched to the workbook. The drift round compared 542 against Bader's own report (line 1021), not again ... | done in code, not proved by a run | none yet | none yet |
| NEW-LOG | log.md steps/log.md lines 3120-3124 (What remai ... | What the penetration pass costs on a real model, box off against box on | Steps 279 and 280, one building run with the box off then on, with the step seconds compared. Every later run had the box on and none with it off is recorded. | done in code, not proved by a run | none yet | none yet |
| Q35-Q40 | log.md steps/log.md line 3126 (What remains, pe ... | One question per GAP property, Family, Type Name, Material, Source File, Discipline and Id From | An answer line under each of Q35 to Q40. All six are blank in steps/02_questions.md, and the close round's 6h reconciliation result is not stated in log.md. | needs Bader, in the form | none yet | none yet |
| NEW-LOG | log.md steps/log.md lines 5231 (F30 What was do ... | A comment F30 moved says the units go before the clash step so the report reads in its units, stale since F26 | The comment reads what F26 does, the report converted to metres whatever the document shows. log.md quotes the comment only in paraphrase, so a grep of src coul ... | open fault | none yet | none yet |
| NEW-LOG | log.md steps/log.md line 4976 (What remains, ro ... | 180 findings of the 2026-09-12 audit carry no verifier | Each of the 180 findings in steps/04_audit.md re-checked by grep and marked verified or refuted. No later entry says that was done. | open fault | none yet | none yet |
| NEW-LOG | log.md steps/log.md lines 4277, 4517, 4660, 346 ... | The proof steps of steps/03_bader_next.md, never worked as a whole | Each step's Look for line matched on a run, or the step cut. Turn 0 Phase 2 puts every never proved step in the register. | done in code, not proved by a run | none yet | none yet |
| T1-S1 | turn 1 read, src/Federator.Addin/Engine/ByDesign.cs:148 | An unnamed clash judged Reviewed is counted as moved: ByDesignTally.Add adds its REVIEWED line and adds to Mov ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S2 | turn 1 read, src/Federator.Addin/Engine/ClashHarvest.cs:164 | A result group with no leaves is counted as one clash, and TestReport.Add feeds RawClashes into the report tal ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S3 | turn 1 read, src/Federator.Addin/Engine/ClashRunner.cs:465 | On a run with no XML and a chosen tolerance, ApplyChosenTolerance (line 617) changes the saved test's toleranc ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S4 | turn 1 read, src/Federator.Addin/Engine/ClashRunner.cs:1102 | Apply edits a saved test and never sets changedTheDocument. ApplyChosenTolerance does set it for the same kind ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S5 | turn 1 read, src/Federator.Addin/Engine/ClashRunner.cs:1106 | The APPLIED log line says the edit reset the test's results. The class's own comments (175-179, 1069-1070) and ... | a failing Core test, then a run | PARTLY settled by a third reader after two split, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S6 | turn 1 read, src/Federator.Addin/Engine/DocumentCensusReader.cs:93 | The class rule (lines 24-27) says a count that cannot be taken is minus one and never zero, but this overload ... | a failing Core test that passes after | PARTLY by two readers, loud failure, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S7 | turn 1 read, src/Federator.Addin/Engine/DocumentGuard.cs:54 | SafeFileName returns null both for an unsaved document and when reading the name threw (lines 79-81). In the s ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S8 | turn 1 read, src/Federator.Addin/Engine/DocumentUnits.cs:118 | When re-reading Document.Units throws, the empty catch at 124 leaves 'after' equal to 'before'. Line 130 then ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S9 | turn 1 read, src/Federator.Addin/Engine/FederationEngine.cs:1573 | A successful ReshapeFromScan never saves the NWF ('nothing on this path saves', 1529-1530). FinishTheGroup sav ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S10 | turn 1 read, src/Federator.Addin/Engine/FederationEngine.cs:2042 | In SaveTheNwf a false return or a throw (caught at 2047-2053) only writes a log line and never adds an error t ... | a failing Core test, then a run | CONFIRMED settled by a third reader after two split, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S11 | turn 1 read, src/Federator.Addin/Engine/FederationEngine.cs:3152 | When SaveTheNwfAgain's save returns false, nothing goes on the outcome (only a throw does, at 3159). WriteFini ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S12 | turn 1 read, src/Federator.Addin/Engine/Penetrations.cs:338 | This is outside the requested files but decides the viewpoint plan's unreadable size branch. An unknown docume ... | a failing Core test that passes after | PARTLY settled by a third reader after two split, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S13 | turn 1 read, src/Federator.Addin/Engine/PropertyProbe.cs:124 | This runs even after File.WriteAllText has thrown. WriteFinished records whatever file sits at the path as wri ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S14 | turn 1 read, src/Federator.Addin/Engine/PropertyProbe.cs:208 | A value that throws is tallied as a real empty distinct value (line 211), so the CSV cannot tell an unreadable ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S15 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:88 | Read maps every comparison except DisplayStringContains to 'equals' and never reads the condition's options. T ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S16 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:112 | ValueOf reads every kind except IdentifierString with ToDisplayString, which the add-in rule says throws on an ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S17 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:273 | SidesBySetName walks only the root of tests.Tests, and a Clash Detective folder is skipped by 'if (test == nul ... | a run that shows it before and not after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S18 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:291 | When reading the clash test sides throws, SidesBySetName logs that no set is removed or renamed and returns an ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S19 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:323 | The comment contradicts itself and the code. A side that throws is not counted, which lowers the set's count, ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S20 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:646 | This is outside the requested files. When FindSelectionSet returns null after the rebuild (line 665), found st ... | a run that shows it before and not after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S21 | turn 1 read, src/Federator.Addin/Engine/SetBuilder.cs:665 | After Rebuild calls ReplaceWithCopy(parent, at, made), the set is re-read through the same parent handle that ... | a run that shows it before and not after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S22 | turn 1 read, src/Federator.Addin/Engine/ViewpointBuilder.cs:668 | Nothing takes one viewpoint's temporary transparency and paint off before the next viewpoint is set up. Undim ... | a run that shows it before and not after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S23 | turn 1 read, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:95 | FolderMemory records a failed read or save in DisabledReason (FolderMemory.cs:78, 120, 285), but nothing in sr ... | a failing Core test that passes after | PARTLY by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S24 | turn 1 read, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:288 | Regroup runs on every FileRow Include change (OnFileRowChanged, line 237) and always builds a fresh name table ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S25 | turn 1 read, src/Federator.Core/Clash/ClashRunOutcome.cs:429 | SkipReasonsInOrder leaves out NoTolerance, which ClashTestPlan.cs:249 produces and ClashRunner.cs:364-366 feed ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S26 | turn 1 read, src/Federator.Core/Clash/PriorityMap.cs:136 | A test name that appears twice silently takes the last row's priority and no problem is recorded, unlike ByDes ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S27 | turn 1 read, src/Federator.Core/Clash/RepeatedFailureGuard.cs:61 | The guard counts consecutive failures and RecordSuccess (90-94) resets the count, and one guard covers the who ... | a failing Core test that passes after | CONFIRMED by two readers, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S28 | turn 1 read, src/Federator.Core/Clash/ToleranceChoice.cs:78 | The help line shown under the drop down (FederatorWindow.xaml.cs:1040) says changing a saved test resets resul ... | a failing Core test that passes after | CONFIRMED by two readers, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S29 | turn 1 read, src/Federator.Core/Clash/ToleranceChoice.cs:200 | ClashRunner.cs:396 writes this default line on every group, including the no-XML path where plan.Source is Doc ... | a failing Core test, then a run | CONFIRMED by two readers, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S30 | turn 1 read, src/Federator.Core/Clash/ToleranceChoice.cs:207 | 'Set on N' counts toleranceOnExisting, which ClashRunner.cs:1152 increments before it resolves the test. The s ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S31 | turn 1 read, src/Federator.Core/Clash/UndoAutoReview.cs:65 | The judgement only asks whether one of our records is on the clash and the clash is at Reviewed. ClashStatusEd ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S32 | turn 1 read, src/Federator.Core/Diagnostics/EventRow.cs:199 | The comment on Number (line 74-77) says it is text so that precision is not lost, but Exact rounds to at most ... | a failing Core test, then a run | PARTLY settled by a third reader after two split, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S33 | turn 1 read, src/Federator.Core/Diagnostics/FolderMemory.cs:242 | Remember discards Save's bool, and DisabledReason is never read anywhere in src. A folders.txt that cannot be ... | a failing Core test, then a run | PARTLY by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S34 | turn 1 read, src/Federator.Core/Diagnostics/LiveLine.cs:236 | OnTheGroupBefore sums every visit of the step in the group before. TESTS CREATE, TESTS RUN and HARVEST start t ... | a failing Core test that passes after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S35 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:268 | On the temp fallback, Start writes into the bare system temp folder and PruneOldLogs(folder, keepLogs) (358) d ... | a failing Core test that passes after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S36 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:482 | RunStarted only moves the mark. groupRecords, stepRecords, written, failures, collapsedLines and ClashesFound ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S37 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:514 | A run that started but never finished is treated as never marked. The window calls RunFinished inside the try ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S38 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:687 | This claim, and the same one at 718 and 1922, is written whether or not a row file exists. Row() returns when ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S39 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:1348 | AppendFinished writes a size of 0 into the .tsv Number column for an NWC that is not on disk, while the text l ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S40 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:1939 | This line is written even when no run was marked. WaitingSeconds is then Never(0.0), so RESULT states 0.000s o ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S41 | turn 1 read, src/Federator.Core/Exchange/ExchangeReader.cs:378 | A missing or unparsable flags, primtypes, selfintersect or merge_composites silently becomes 0 or false (lines ... | a failing Core test that passes after | CONFIRMED settled by a third reader after two split, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S42 | turn 1 read, src/Federator.Core/Exchange/MatrixCorrections.cs:643 | Under F78 (SetBuildPlan.cs:169-171 and Groups() at 212-229) a flags=64 condition starts a new OR group. Put st ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S43 | turn 1 read, src/Federator.Core/Exchange/MatrixCorrections.cs:898 | The CategoryRewrite replace runs over the whole set block, including the `<selectionset name="..."` attribute, ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S44 | turn 1 read, src/Federator.Core/Exchange/RevitWorksets.cs:131 | A missing resource gives the same empty list as an empty file and nothing records the difference. That is the ... | a failing Core test that passes after | PARTLY settled by a third reader after two split, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S45 | turn 1 read, src/Federator.Core/Findings/ScanFindings.cs:243 | Every scan finding is worked out from BuildingGroup.Building, which is the grouping key (BuildingGrouping.cs:1 ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S46 | turn 1 read, src/Federator.Core/Findings/ScanFindings.cs:366 | This sentence reaches the window and the log, and it contradicts the code since F77: ClashRunner.cs:694 says ' ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S47 | turn 1 read, src/Federator.Core/Findings/SourceMismatchFindings.cs:240 | GroupBuilding is the group key (FederatorWindow.xaml.cs:1723 passes group.Building into FederationJob, and Fed ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S48 | turn 1 read, src/Federator.Core/Health/ExportCheck.cs:66 | IdShare is rounded before the `model.IdShare < 100` test (line 136, and again at FederationEngine.cs:2152), so ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S49 | turn 1 read, src/Federator.Core/Health/ExportCheck.cs:127 | When WithWorkset is NotCounted (-1) this prints NONE. That contradicts ModelExport.NotCounted's own rule at li ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S50 | turn 1 read, src/Federator.Core/Health/ExportCheck.cs:242 | The only workset test is CarriesAWorkset, `WithWorkset > 0` (line 53), so one element with a workset passes as ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S51 | turn 1 read, src/Federator.Core/Health/InvisibleDifference.cs:158 | WithoutInvisibles also removes ordinary spaces, so 'EL-Fire alarm' against 'EL-Firealarm', a space anyone can ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S52 | turn 1 read, src/Federator.Core/Health/SetWarnings.cs:47 | FindCategoriesNobodyHas (lines 221-238) never looks at condition.Flags. A negated condition (32) on an unknown ... | a failing Core test that passes after | PARTLY settled by a third reader after two split, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S53 | turn 1 read, src/Federator.Core/Health/SetWarnings.cs:119 | The comment says the opposite of what the code does. SignatureOf (lines 172-189) joins RuleSignature values, w ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S54 | turn 1 read, src/Federator.Core/Naming/OutputNameTable.cs:209 | Refill counts a row as kept if any one of its three cells was typed over (lines 178-181), but it still refills ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S55 | turn 1 read, src/Federator.Core/Probe/ProbeVerdict.cs:134 | PropertyProbe accepts a category through settings.Asks, which trims and ignores case (PenetrationSettings.cs 2 ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S56 | turn 1 read, src/Federator.Core/Report/ClashReportModel.cs:726 | ClashHarvest.cs:334-335 fills ElementId with a real GUID and sets IdLabel = "Instance GUID", and IdFrom stays ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S57 | turn 1 read, src/Federator.Core/Report/ReportOrder.cs:274 | Pass one moves every changing picture to a .moving name before any final move, and nothing rolls back. If pass ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S58 | turn 1 read, src/Federator.Core/Report/ReportPaths.cs:130 | When the NWF folder is inside the scanned folder, Refuse throws on purpose (line 178) to say there is nowhere ... | a failing Core test that passes after | PARTLY by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S59 | turn 1 read, src/Federator.Core/Report/WorkbookWriter.cs:279 | WriteEmptyTestRow is one row and returns start + 1, but ClientStyle.TestHeader fills and borders both top and ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S60 | turn 1 read, src/Federator.Core/Report/WorkbookWriter.cs:594 | WriteImageCell sets 72 pt for a pasted thumbnail, then WriteBlock line 260 'sheet.Row(row).Height = ClashRowHe ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S61 | turn 1 read, src/Federator.Core/Rerun/RebuildTally.cs:65 | A Before count that could not be taken (minus one) reads as 'nothing to keep'. SavedViewpoints.Count returns - ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S62 | turn 1 read, src/Federator.Core/Sets/EmptySets.cs:100 | Property internal names are written into the code (line 103 as well, "lcldrevit_parameter_-1002053"), which CL ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S63 | turn 1 read, src/Federator.Core/Sets/SetBuildOutcome.cs:74 | Only PRESENT sets are ever judged: SetBuilder.cs:701 `if (found == 0 && !drift.CouldNotRead)` sits inside the ... | a failing Core test, then a run | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S64 | turn 1 read, src/Federator.Core/Sets/SetBuildOutcome.cs:207 | PutAnythingIn ignores RebuiltCount, and FederationEngine.cs:2456 `return sets.PutAnythingIn // sets.ActedOnLef ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S65 | turn 1 read, src/Federator.Core/Sets/SetBuildOutcome.cs:372 | This is printed whenever Drifted.Count == 0. A set whose search could not be read is never Drifted and never r ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S66 | turn 1 read, src/Federator.Core/Sets/SetBuildOutcome.cs:386 | FindingItemsCount, ZeroCount and TotalItems count CREATED sets only (Count(true, ...), IsZero, result.Created) ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S67 | turn 1 read, src/Federator.Core/Sets/SetDrift.cs:46 | The drift key carries no flags, so the StartGroup bit (the OR) is never compared. The comment at 97-99 says or ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S68 | turn 1 read, src/Federator.Core/Sets/SetDrift.cs:148 | AskedNow joins every condition with " and ", and so does WantedNow at line 161, with no StartGroup grouping. T ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S69 | turn 1 read, src/Federator.Core/Sets/SetRebuildSettings.cs:20 | With the box on, SetBuilder.Build (SetBuilder.cs:506-509) runs HandleLeftovers, which removes unused sets the ... | a failing Core test that passes after | CONFIRMED by two readers, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-S70 | turn 1 read, src/Federator.Core/Views/SizeText.cs:204 | Only the FIRST digit after a letter is skipped. The walk then advances one character (lines 50-53) and the nex ... | a failing Core test that passes after | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B1 | turn 1 read, src/Federator.Addin/Engine/ClashImages.cs:66 | The image guard belongs to one ClashImages, and FederationEngine.cs 2589 builds a new one per group ('runner.I ... | a run that shows it before and not after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B2 | turn 1 read, src/Federator.Addin/Engine/FederationEngine.cs:3179 | BuildViewpoints never reads SavedViewpoints.CanBuild, and nothing in src/ does. A grep for CanBuild finds only ... | none needed | CONFIRMED by two readers, noise, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B3 | turn 1 read, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:2590 | Only the Close button is guarded. There is no Closing handler anywhere in the add-in (grep for Closing finds n ... | a run that shows it before and not after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B4 | turn 1 read, src/Federator.Addin/Ui/GroupRow.cs:126 | CLAUDE.md says a typed-over cell can be given back to the pattern. OutputNameRow.ReleaseToPattern (Core Output ... | a failing Core test, then a run | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B5 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:34 | Retention lists only run-*.log (line 376). The .tsv that RowLog.PathFor (RowLog.cs:80) writes beside every log ... | a failing Core test that passes after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B6 | turn 1 read, src/Federator.Core/Naming/NamePattern.cs:69 | The project rules call DateFormat a setting whose mistakes should be visible in the preview. A grep of every s ... | a run that shows it before and not after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B7 | turn 1 read, src/Federator.Core/Naming/OutputNameTable.cs:84 | The project rules say a typed-over cell can be given back to the pattern, but nothing in src calls ReleaseToPa ... | a failing Core test, then a run | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B8 | turn 1 read, src/Federator.Core/Naming/OutputNameTable.cs:245 | When a pattern field is emptied, Refill stores this sentence as the name in every untouched cell, and the only ... | a failing Core test that passes after | PARTLY by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-B9 | turn 1 read, src/Federator.Core/Views/SizeTally.cs:77 | Nothing in src constructs SizeTally, and no other code writes a SIZE line (Grep "SIZE over src finds only this ... | a failing Core test, then a run | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L1 | turn 1 read, src/Federator.Addin/Engine/Penetrations.cs:187 | ServiceSizeOf calls ReadSide, which calls Upwards(item) at line 260 outside its try. walker.Parent at line 371 ... | a run that shows it before and not after | CONFIRMED settled by a third reader after two split, loud failure, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L2 | turn 1 read, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1508 | ReportsWanted calls ChosenTolerance, which throws ArgumentOutOfRangeException when Other is chosen with a blan ... | a failing Core test, then a run | CONFIRMED by two readers, loud failure, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L3 | turn 1 read, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1701 | The only name check before a run is for collisions. An emptied pattern box makes every name 'CANNOT BE NAMED: ... | a failing Core test that passes after | CONFIRMED by two readers, broken feature, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L4 | turn 1 read, src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1725 | Clearing an NWF or NWD name cell stores an empty by hand name (GroupRow.cs:118 and 126). The collision check a ... | a failing Core test, then a run | CONFIRMED by two readers, loud failure, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L5 | turn 1 read, src/Federator.Core/Clash/AutoReviewRecord.cs:150 | In() is documented to return null for a comment that is not ours, but a marker comment reading [was Reviewed/A ... | a failing Core test that passes after | CONFIRMED by two readers, loud failure, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L6 | turn 1 read, src/Federator.Core/Diagnostics/RunLog.cs:565 | WriteRaw has no try. A write or flush that throws, such as a full disk, or a LineWritten handler that throws ( ... | a failing Core test that passes after | CONFIRMED by two readers, loud failure, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-L7 | turn 1 read, src/Federator.Core/Health/AlignmentCheck.cs:255 | ModelPlacement has NotRead for X, Y and Z but no not-read state for the site. ModelFactsReader.cs:152-155 sets ... | a failing Core test, then a run | CONFIRMED by two readers, silent wrong output, steps\notes\turn1-read-verified.md | none yet | none yet |
| T1-N1 to T1-N93 | turn 1 read | Ninety three noise findings, steps\loop-read.md section 1 | worked after every silent, broken and loud finding | reported by the read, not verified | none yet | none yet |
| T1-UNCALLED | turn 1 read | 150 members nothing in src calls, steps\loop-read.md section 3 | each deleted with its tests, or kept by a decision in 02_questions | reported by the read, not verified | none yet | none yet |
| T1-CATCH | turn 1 read | 77 catches called swallowing, steps\loop-read.md section 4 | each sorted into the logging rule or a fault | reported by the read, not verified | none yet | none yet |

## Turn 0, 2026-09-27

Runs: none.

Findings: F98 and F99.

Fixed: nothing.

Programs started: git, the fetches and the fast forward. dotnet build of the solution and
dotnet test of the Core tests, with the MSBuild and compiler servers they started, both
shut down afterwards with dotnet build-server shutdown. gh.exe twice, for its version and
its login state. No Navisworks.

Files written outside the repo: %LOCALAPPDATA%\Programs\gh, the release zip at 15,473,232
bytes, LICENSE at 1,089 and bin\gh.exe at 42,755,384. Whether dotnet wrote to its own
caches under the user profile is UNKNOWN. Inside the clone and not tracked: http.sslBackend
schannel in .git\config, and the build output under bin and obj.

## Turn 1, 2026-09-27 to 2026-09-28, paused once for a restart

Runs: no Navisworks run by the loop. prepare-copy.ps1 was run many times on the real
folder, in three versions, and its every mode proved in the last, again on 2026-09-28 with
every answer kept in turn1\proof-scripts-2026-09-28.txt. Bader ran the tool himself at
16:37 on 2026-09-27, see RUN-1637.

Findings: the read in steps\loop-read.md, 179 faults reported and none confirmed, 70 of
them silent wrong outputs, 150 members nothing in src calls, 77 catches called swallowing,
46 files with more than one top level type, no Navisworks type in Core, and the doubled
summary block above BuildViewpoints not there. Three reviews of the house found faults in
the loop's own scripts and walls, all fixed before it went out, and the limits a wall that
reads words cannot close are written down. RUN-1637. The client's workbooks hold linked
pictures and no stored ones, as core.md says.

Fixed: F97 the house and F99 the git wall, in PR 72.

Still open: F98, then Phase 1, and every register row that is not DONE.

Programs started: git and gh. Windows PowerShell 5.1 for the scripts, and to write two test
workbooks through Federator.Core's WorkbookWriter. sh for the hooks and the proof harness,
which cloned the repo into a temp folder on each run and removed it. dotnet test by hand
and through the pre-commit hook, and dotnet build-server shutdown. Thirty agents inside
Claude Code, all read only: nineteen readers, ten reviewers and breakers over three rounds,
three of them stopped for the restart before they answered, and one claim-checker over this
entry, the log entry and the pull request body. No Navisworks.

Files written outside the repo:

    C:\Users\p003653k\bin\gh                                  a two line wrapper for Git Bash
    %LOCALAPPDATA%\NwcFederatorLoop\source                    the copy of NM Fed, 141 files, 208.3 MB, 10 folders
    %LOCALAPPDATA%\NwcFederatorLoop\source.manifest.txt       the copy's completion manifest
    %LOCALAPPDATA%\NwcFederatorLoop\source.removed.txt        written by each proof's remove and deleted by its restore
    %LOCALAPPDATA%\NwcFederatorLoop\turn1                     the raw reader results, the proof runs, the test workbooks and the drafts
    the session scratchpad under %LOCALAPPDATA%\Temp\claude  the same, from before the restart

Inside the clone and not tracked: .claude\hooks\.loop-gate-last once the Stop gate first
blocks, which git ignores.

## Turn 2, 2026-09-28, closed WAITING for Bader's answer on F100 and his Navisworks

Runs: none yet.

Findings: none new yet. The Stop gate sent this session back once, live, and the paths
wall refused one of the lead's own commands for naming NM Fed, both as designed.

Fixed: F98, the close round heading back in steps\log.md, in PR 73. From the heading down,
all 5747 lines match the file as it was before PR 71, line endings aside. Read by a
reviewer, a breaker and the claim-checker, who found the fix right and flagged wording in
the records, fixed before the merge.

Phase 1 so far: Bader's logs folder and the installed bundle backed up, items 5 and 6.
Item 1, F100: the Automation API starts Navisworks with no click on this machine, measured
three times by versions of the probe, and the committed version waits for its one run.

Findings: while a Navisworks the loop started runs, Bader's own Navisworks settings change,
in the registry and under his roaming profile, and the two guards in .claude\rules\loop.md
answer it. The first probe could have force closed a Navisworks it did not start, found by
review before anything went wrong.

Still open: F100's run, the rest of Phase 1, and every register row that is not DONE.

Programs started so far: dotnet build-server shutdown, for the servers the turn 1 tests
started. git and gh. dotnet test by hand on fix-F98 and through the pre-commit hook.
Windows PowerShell for the backups, the probe and its checks, and reg.exe for exports and
one import. dotnet build of the add-in, by the prober. THREE NAVISWORKS, each started by
the probe through the Automation API as Roamer.exe -Embedding and each closed by the API's
Dispose, none forced: pid 44888 at 12:27, pid 12336 at 13:11 and pid 50204 at 13:17, with
the licensing agents and helpers they started, all gone afterwards. Bader's pid 34668 was
never closed, attached to or sent anything. The prober's checks that start no Navisworks
also started three throwaway powershell windows of its own, each exiting by itself, and
made and deleted the throwaway key HKCU\Software\NwcFederatorLoopTest several times, twice
by hand after its deadline test ended its own process, and it is gone. Bader's 22.0 key
exported byte identical before and after every such check. Agents: the prober, the
reviewer, breaker and claim-checker on F98, four reviewers and four breakers on F100,
and one planning agent for the no-click entry, whose design is in turn1\f101-design.md.

Written into Bader's settings, outside the work folder, all while his Navisworks 34668
ran, so which Navisworks wrote each change is UNKNOWN: after run 1 the prober put back by
hand, with reg import, the Recent File List, MainWindow and PluginOptions keys from the
export taken before it. Runs 2 and 3 put back 36 values each by themselves, the CER
counters, MainWindow Placement, PluginOptions DefaultPlugin and the Recent File List 1 to
10, every one read back equal. The rule now forbids a put back while another Navisworks
runs. Three files under %APPDATA%\Autodesk\Navisworks Manage 2025, clash\rules,
CommCenter\en-US\infocenter.xml and LastSession.xml, were rewritten at 12:29 during run 1
with no copy taken before, so what they held is UNKNOWN.

Files written outside the repo so far:

    %LOCALAPPDATA%\NwcFederatorLoop\logs-backup                 Bader's logs folder, 32 files, 11,764,326 bytes
    %LOCALAPPDATA%\NwcFederatorLoop\bundle-backup               the installed bundle, 15 files, 10,621,229 bytes
    %LOCALAPPDATA%\NwcFederatorLoop\navisworks-settings-backup  Bader's Navisworks settings folder, 205 files,
                                                                and the 22.0 key, taken at 12:40, after run 1
                                                                and its put back by hand, before run 2
    %LOCALAPPDATA%\NwcFederatorLoop\probes                      the probe's work folders of runs 1 to 3, its
                                                                checks, and copies of one NWC and the NWD saved
                                                                from it
    %LOCALAPPDATA%\NwcFederatorLoop\turn1                       the F98 and F100 messages, bodies and fix lists,
                                                                steps\log.md before PR 71 and the fixed file,
                                                                and the session evidence file, items 11 to 17

## Turn 3, 2026-09-29 to 2026-09-30, ended when turn 4 opened

Opened by Bader's answers of 2026-09-29, Q79 to Q81. Main at e555619.

Runs: F100's run 4, the one run Bader's answer allowed, 11:35:48 to 11:37:53, 125 s by its
result's first and last lines, the attempt 4 probe, all six steps passed, result masked
into tools\probes\automation-start-result-20260929.txt. The lead launched it at 11:35:46.
ONE NAVISWORKS started, pid 33752, by the Automation API as Roamer.exe -Embedding,
adopted, closed by Dispose and gone 8.5 s after, not forced. What it brought, by the
result's lines 459 to 479: AdskLicensingAgent pids 40156 and 15536 under the Roamer, and
AdskLicensingInstHelper pids 36204 and 16872 under GenuineService. The result reads each
exited but 40156, UNKNOWN whether it exited. Bader's settings, 36 registry values and 2
files, were put back by the probe with no other Navisworks running, each read back.
READ BY THE LEAD WITH Get-Process AFTER THE RUN AND KEPT IN NO FILE: no Roamer, no
AdskLicensingInstHelper, no pid 40156, and four other AdskLicensingAgent processes started
09:37:38, 09:37:42, 10:18:59 and 10:19:03. What started the two at 10:18 and 10:19, when no
Navisworks ran, is UNKNOWN. From here, every such read is written to a file in the turn's
work folder before it is cited.

Findings so far:

- the result files on fix-F100 name this machine and carry the licensing agent's ids, and
  nothing masks either before a commit, F102
- origin/master's tip is be0b9b37, the build the installed add-in reads, which answers why
  the install is not main
- THE WINDOW CAN ONLY LOG INTO BADER'S FOLDER, which holds exactly 30 run logs, so any
  window run on main deletes his oldest, Q82
- loop runs will write autosaves into his AutoSave folder, 196 files, Q86
- the build stamp reads +edits when the tree holds any untracked file, so main is installed
  from a clean checkout only, from the F103 design
- F105, four reads off the install with no Navisworks started, by the prober: all 79
  members on its list of what SavedViewpoints.cs calls exist here, 53 in its first read, RemoveFile and TryRemoveFile exist as 5c
  says, navisworks.gui.roamer.dll parses 39 switches and off the IL
  Roamer.exe -ExecuteAddInPlugin <id> needs no -Embedding, so it may open the window with
  no click, UNKNOWN until a start, and no public member of the API the add-in uses writes
  the Clash Detective report. Draft section 5z-f and five result files in turn3\f105
- the copy of the source folder matched on all 141 files, the installed bundle matched
  bundle-backup on all 15

Fixed: F100, squash merged in PR 74 as 0eb4ede.

VERIFIED: the 86 silent, broken and loud faults of the turn 1 read, each read again on main
at 0eb4ede by two readers who did not see each other, a third settling 8 splits. 76
CONFIRMED, 10 PARTLY, none refuted, steps\notes\turn1-read-verified.md, and the register's
86 rows carry each verdict. 24 read only agents. No fix of them lands before the baseline.

Designs written: F103 run.ps1 by two designers and a judge, being built on fix-F103, and F104
the separate read of the document by a planner, part 1 built on fix-F104, read by a reviewer
and a breaker, its fixes at ebd8bb7, both designs in turn3.

Programs started so far: git and gh. Windows PowerShell for Get-Process Roamer, the hashes
and prepare-copy.ps1, which kept the copy. Agents: two developers, F100 attempt 4 in this
clone and F102 in a worktree under .claude\worktrees, the prober for F105, three planners
for F103, one for F104, a reviewer and a breaker reading attempt 4, a developer for F104
part 1 in a worktree. The F100 developer's harness started stand in Roamer.exe copies of
ping.exe, pids 46484, 45076, 33652, 41120, 39260, 32500, 46928, 30616 and 27984, ping
helpers 34840, 44104, 42724 and 39348, and throwaway powershell windows, and stopped each
stand in by its own pid. It made and deleted HKCU\Software\NwcFederatorLoopTest, read
absent at the end. Get-Process Roamer read 0 after it. The prober wrote two debug scripts
into the session scratchpad and deleted them.

Written outside the repo so far: %LOCALAPPDATA%\NwcFederatorLoop\turn3, the commit
messages, the pending findings, the form draft, the F103 and F104 designs, f105 with the
prober's scripts and results, f100-harness with the harness and its output, and the
harness's throwaway folders, removed by it. In this clone's .git\config, main's upstream,
branch.main.merge, is refs/heads/main where it was refs/heads/master.

PAUSED AT 17:48 ON 2026-09-29 FOR BADER'S SHUTDOWN. Stopped by the lead: the final reading of
F102's third fix attempt, before it answered, and the F103 developer part way through fix
attempt 1, whose work was saved unfinished as e0760ac on fix-F103 and pushed. Read at the
pause: Get-Process Roamer 0, no loop powershell running, HKCU\Software\NwcFederatorLoopTest
absent. Pushed at the pause: fix-F102 7d8d135, fix-F103 e0760ac, fix-F104 ebd8bb7, fix-F105
94a839b, fix-T3-records-2 with this file. Found since the last record and in the register
now: T3-W1, the git wall refuses a push from a worktree while the main clone has main checked
out, even with a refspec to another branch, a false refusal on the safe side, fixed only
through the prove-hooks flow. M1 measured by the F103 harness with no Navisworks: Bader's
logs held open with no delete sharing survive the tool's real prune, RETAIN keeping 30
logs, deleted 0, could not delete 1, which is what option A of Q82 rests on. M2 and M3
answered, turn3\f103.

RESUMED 2026-09-30. Bader opened Navisworks by hand twice in the morning, pid 38520 started
09:21:35 and gone by 09:27:11, and pid 36172 started 09:32:45 and gone at the read of 09:58:08,
read by the F103 developer and kept in %LOCALAPPDATA%\NwcFederatorLoop\turn3\f103\roamer-reads.txt
lines 10 to 14. Neither was touched. 36172 changed his own 22.0 key while prove-run.ps1 ran
its sixth time, so that run was thrown away, and the harness now stops when it finds a
Roamer it did not start. SO BADER USES NAVISWORKS WHILE THE LOOP RUNS, and every start the
loop makes has to wait for none of his to be running, which run.ps1 refuses in code.

THE WINDOW OF 2026-09-30, SO FAR. The reading of F103 fix attempt 1, fcd981b, finished at
11:27. The reviewer read twelve of the thirteen items fixed and item 8 UNKNOWN, the breaker on
Bader's things read item 13, the install race, not fixed, which fix list 2 takes up as item 10,
nobody found anything that starts, adopts, messages or closes a Navisworks or window of
Bader's, and they found new faults of their own, fix list 2. Judged safe for a
start with no window today, because what it found either cannot happen inside item 0's hold
while Bader keeps his Navisworks closed, or fails on the safe side. Measured first, with a
ping.exe and no Navisworks: an exited process is not found by Get-Process -Id or
Win32_Process while its handle is held, so a real run reads its own closed Navisworks as gone.

THE FIRST REAL START, run.ps1 -Mode Run -Set 00 -Item 0 -Stamp be0b9b37 on fcd981b, launched
11:29:29, ended 11:40:10, exit 5. Get-Process Roamer read 0 at 11:29:12 and after the end.
- ONE NAVISWORKS STARTED, pid 42064, by the Automation API, the constructor returning after
  92.55 s, adopted on all four conditions and held through its handle, Visible True, held
  360 s, Dispose returned after 0.49 s, the process gone 8.9 s after, nothing forced
- Bader's settings: 5 registry values and 2 files differed, all put back and read back
  equal, no other Navisworks having run. His logs folder read the same after as before, and
  his AutoSave folder unchanged, 196 files copied into autosave-backup first. Keep awake
  requested and released on native thread 25304, 0x80000000 then 0x80000003
- M4, the processor time of an idle Navisworks: never zero, 0.094 to 0.578 s a 15 s sample,
  about 1.2 to 1.6 s a minute. The hang rule as written can never fire on an idle Navisworks,
  which Q83 now says
- M5, what changed outside the loop folder while the start ran, written at or after the call:
  7 keys under HKCU\Software\Autodesk, and files, 11 under %APPDATA%\Autodesk, 1 under
  Recent, 22 under %LOCALAPPDATA%\Autodesk, 7 under %PROGRAMDATA%\Autodesk and 19 under
  %TEMP%, listed in the run folder's m5.txt, which stays out of the evidence until it is masked.
  NOT ALL OF IT IS THE START'S: 16 of the 19 under %TEMP% are Claude Code's own, and 10 are
  Desktop Connector's, so at least 26 of the 60 files came from other programs
- M6: the session read unlocked on every read
- THE EXIT 5 IS A FALSE FINDING. Its one DIALOG was the Navisworks main window, which has an
  owner, window 855918, so fix list 1 item 11's rule of MAIN only with no owner is wrong for
  the real window. Messages went only to the adopted process's own window. Item 21 of
  steps\notes\f103-fix-list-2.md
- the evidence, record.txt, watch.txt and settings.txt, is in the F103 worktree's
  steps\runs\00\item0, read by F102's mask at 7d8d135, which found nothing to mask, and rides
  in F103's pull request

Written outside the repo by the first start: %LOCALAPPDATA%\NwcFederatorLoop\autosave-backup,
196 files, 285,849,138 bytes, about 286 MB, and %LOCALAPPDATA%\NwcFederatorLoop\runs\00\item0, the run folder with the
settings backup, the listings, m5.txt, mypid.txt and the record. What changed outside the
loop folder while the start ran is M5's list above, the start's and other programs'.

F103 FIX ATTEMPT 2, the 21 items of steps\notes\f103-fix-list-2.md, sent to its developer at
11:42, asked to report by 12:50.

THE SECOND REAL START, run.ps1 -Mode Run -Set 01 -Item 0 -Stamp be0b9b37 on 867697a, F103
fix attempt 2, after its reading by a reviewer and two breakers, all three answering SAFE FOR
ITEM 0 first. Launched 13:37:37, ended 13:48:08, EXIT 0, VERDICT RAN, item 0 with no window:
started, adopted, held 360 s, closed by Dispose, put back. Get-Process Roamer read 0 at
13:37:25 and after the end. The record's run.ps1 and nw-guard.ps1 sha256 equal the header of
harness run 10, which ran the same code.
- ONE NAVISWORKS STARTED, pid 49604, the constructor returning after 83.30 s, adopted on all
  four conditions, Dispose returned after 0.40 s, the process gone 7.8 s after, nothing forced
- ITEM 21 MEASURED: the main window read MAIN, owner 725174 of class
  WindowsForms10.Window.0.app.0.27a2811_r7_ad1, empty caption, visible False, enabled True,
  in the adopted process, record line 33
- Bader's settings: 5 registry values and 2 files put back and read back equal, his logs
  folder the same after as before, his AutoSave unchanged. Keep awake released on its own
  thread. M5, what changed outside the loop folder while it ran: 7 keys and 35 files, 0 under
  %TEMP% this time and 11 of them Desktop Connector's
- M4 again: never zero, 0.016 to 0.391 s a 15 s sample and 0.406 to 0.875 s a minute, lower
  than the first start's, about 2 to 4.4 s in five minutes
- both starts' evidence, steps\runs\00\item0 and steps\runs\01\item0, read by F102's mask at
  7d8d135, which found nothing to mask, and by F102's check, 9 files, none carrying an id or
  the machine name, committed on fix-F103 as 912dedc

THE WINDOW OF 2026-09-30, WHAT IT WAS USED FOR, written at 13:57. Every Navisworks the loop
started was closed and Bader's settings put back and read back, the last at 13:48:08.
Get-Process Roamer read 0 at 13:57:32 and again at 14:38:32, before the window closed.
1. the reading of F103 fix attempt 1, then THE FIRST REAL START on it, 11:29 to 11:40, which
   measured M4, M5 and M6 and found the main window has an owner
2. F103 fix attempt 2 from what that start showed, its reading, and THE SECOND REAL START on
   it, 13:37 to 13:48, clean, which measured the owner hidden
3. NOT DONE IN THE WINDOW, AND WHY: F103's merge and the install of main. Its reading of fix
   attempt 2 found item 7 of fix list 2 still open and new faults, so fix attempt 3, the last,
   is needed before F103 can merge, and Install runs only from main with run.ps1 on it
WHAT IS LEFT FOR TONIGHT'S WINDOW, after Bader closes Navisworks and leaves the PC on:
1. F103 fix attempt 3, steps\notes\f103-fix-list-3.md, with its developer since 13:57, then
   its reading. A fault of the list left or a new one sends F103 to the form
2. if clean: a third real start on it, Set 02, then PR 78 merged, then run.ps1 -Mode Install
   from the clean checkout %LOCALAPPDATA%\NwcFederatorLoop\wt-main at main's new commit, the
   first install of main
3. then F104 part 2, check-documents.ps1 on main's run.ps1
4. anything that opens the tool's window still waits on Q82

AFTER THE WINDOW, 2026-09-30. Fix attempt 3 of F103 came back as a63c284 at 15:10, with
harness run 12, 242 passed and 0 failed on its files. Its final reading by a reviewer and two
breakers answered safe for item 0 and safe for install, one of them within its lens, classed
no finding as a new fault inside the attempt, and the reviewer read 18 of the 19 items fixed.
THE LEAD HELD ITEM 14 against the words it names:
the harness header's sentence that a line throws, where the line only sets the error, stands
word for word at prove-run.ps1 24 to 25, named after attempts 1 and 2 as well. So F103 went to
the form as Q92, steps\notes\f103-final-reading.md, and PR 78 stays a draft. Five register
rows added from the readers' other findings, classed old or polish, T3-G13 to T3-G16 and
T3-P2, three of them resting on code the attempt wrote. Actions on a63c284, run 36713756910,
had not run a test: "The job was not started because it repeatedly failed to be acquired (5
attempts)". Run again by the lead, green at 16:05:30, the Core tests 1720 passed, 0 failed,
26 skipped of 1746.

TONIGHT'S WINDOW CHANGES: the third real start on a63c284 is made as evidence for Q92, and
nothing merges or installs. Item 2 of what was left for tonight, the merge and the install of
main, waits on Q92, and so does item 3, F104 part 2.

Read at 16:53:24, and again at 17:06:19 into turn3\reads-170618.txt: no .replaced- or
.failed- folder sits beside the bundle in
%APPDATA%\Autodesk\ApplicationPlugins, where only one folder's name starts with
ParsonsNwcFederator, so T3-G15's stale folder would not stop the first Install.

Programs started after the window closed at 14:45. By the F103 developer: harness run 12
from 14:46:02 to 15:04:48, with a dotnet build of the stand-in and 43 stand-in Roamer.exe each
closed through its held handle, and what tools\loop\README.md says the harness starts, among
them git for scratch repositories and cmd.exe for one junction under its work folder, then
the copy of the probe's harness 4 from 15:05:53 to 15:08:22, then git to commit and push
a63c284, and gh to set PR 78's body, updated at 15:18:07. Anything else the developer ran
after 14:45 is UNKNOWN here. It wrote turn3\f103\prove-run-out-12.txt, turn3\f103\h4\h4-fix3-out.txt
with the five run files beside it written from 15:06:38 to 15:07:29, and in the session's
scratchpad pr78-fix3-section.md, commit-fix3.txt, pr78-body-before-fix3.md and
pr78-body-fix3.md, from 14:46:26 to 15:17:35. By the three readers and the claim-checker:
reads only. By the lead: git and gh, among them gh run rerun of run 36713756910, Windows
PowerShell for Get-Process Roamer, Get-Date, the hashes, powercfg, one Win32_Battery read and
two listings of ApplicationPlugins, run.ps1 -Mode Check for Set 02 at 16:23:39, pid 43368,
which reads only, dotnet test of the Core tests at 16:54:08 and again from 17:07:16 to
17:07:59, with the MSBuild and compiler servers they started, shut down at 17:08:00 with
dotnet build-server shutdown, and from 16:14:11 the waiter, turn3\wait-window.ps1, pid 51376,
which reads the clock, the processes named Roamer and the time since the last key or mouse
input once a minute, and starts no Navisworks. At each start the waiter compiles one small
C# class with Add-Type, which in Windows PowerShell 5.1 runs the C# compiler with its files
under %TEMP%. Those files were not read, UNKNOWN. Written outside the repo by the lead:
turn3\wait-window.ps1, turn3\window-wait.txt, turn3\check-set02-162337.txt,
turn3\reads-170618.txt, turn3\core-tests-170716.txt, turn3\pr-f103-form.md, and in the
session's scratchpad final-reading.txt, reading-of-attempt-1-fcd981b.txt,
reading-of-attempt-2-867697a.txt and claim-evidence.txt, the three readings copied from their
journals and the lead's git and gh reads.

TURN 3 ENDED with no start in tonight's window. The waiter wrote THE WINDOW IS OPEN at
18:00:14 on 2026-09-30 as its last line, and nothing was started in it. Why is UNKNOWN.
Bader's answers of 2026-10-01 opened turn 4.

## Turn 4, 2026-10-01, open

Opened by Bader's message headed 30 Sep 2026, read on 2026-10-01, Q82 to Q94. Main at
821ed6e. The plan is the turn 4 entry of steps\log.md.

Runs:

- THE THIRD REAL START, run.ps1 -Mode Run -Set 02 -Item 0 -Stamp be0b9b37 on a63c284, from
  the F103 worktree, Bader's answer Q92 B. Get-Process Roamer read 0 at 08:48:06 and at
  09:05:54, read by the lead and kept in no file, and run.ps1's own last check before the
  constructor read no Roamer, record lines 18 and 19, before the call at 09:06:28.883, line 21. Launched 09:06:16, run.ps1 began 09:06:18, ended
  09:18:23, EXIT 0, VERDICT RAN, item 0 with no window. ONE NAVISWORKS STARTED, pid 37988,
  adopted, held 360 s, Dispose returned after 0.48 s and the process read gone 6.2 s after,
  nothing forced. The watchdog saw two AdskLicensingAgent processes start under it, pids
  34260 and 22676, watch.txt lines 5 and 6. It reads only processes of the names it knows,
  so whether anything else started is UNKNOWN. The main window read MAIN with an owner that
  is not visible, as on the second start. M4 again, the processor time of an idle
  Navisworks: 0.031 to 0.344 s a 15 s sample and 0.359 to 0.594 s a minute, about 1.8 to 3.0 s
  in five minutes. Bader's settings: 5 registry values and 2 files differed and were put back
  and read back equal, no other Navisworks having run. The two files,
  CommCenter\en-US\InfoCenter.log and LastSession.xml, read written at 08:45:40 on
  2026-10-01 in the backup, before the start, and what wrote them then is UNKNOWN. His logs
  folder read the same after, his AutoSave unchanged. M5: 7 keys and 62 files written at or
  after the call, 33 of them under %TEMP%, by any program. The session read unlocked at
  every minute. Evidence steps\runs\02\item0, read by F102's mask at 7d8d135, which found
  nothing to mask, and by F102's check over steps\runs, 12 files, none carrying one,
  committed to fix-F103 as 19a3da7

Merged: PR 76, F102, as 4fa1040, Bader's answer Q91 B. Its Actions run 36827848274 read the
Core tests 1720 passed, 0 failed, 26 skipped of 1746, and its evidence check 457 files with
none carrying an id. PR 78, F103, as 398b910, Bader's answer Q92 B, after main was merged
into fix-F103 with two conflicts in steps\01_next.md and steps\log.md resolved by keeping
both sides. Its Actions run 36830150530 read the Core tests 1720 passed, 0 failed, 26
skipped of 1746, and its evidence check 473 files with none carrying an id. Both runs' lines
are kept in turn4\actions-reads.txt. Both fix branches are deleted on GitHub and here. Git no
longer lists either worktree, but Windows answered Permission denied when git removed git's
own folders for them under .git\worktrees, and the two worktree folders under
.claude\worktrees still hold their files. Why is UNKNOWN. The lead's own shell had its working
folder inside the F103 one for a time this morning, which may be why for that one.

THE INSTALL OF MAIN, REFUSED THREE TIMES. run.ps1 -Mode Install -Stamp 398b910e from wt-main,
clean at 398b910, at 10:58:35, 11:03:22 and 11:20:21, Get-Process Roamer 0 before each in
turn4\roamer-reads.txt. Each time build\install.ps1 built, staged and read no Roamer, then
Windows denied its rename of the installed bundle, "Access to the path ... is denied", and it
exited 2 with the installed add-in whole, be0b9b37 as before. Its refusal says Navisworks is
running, which it was not. Each install.txt is under installs. Then, reading only on the
installed bundle: Windows Restart Manager named no process holding any of its 15 files, the
folder, Contents, Contents\v22 and every file opened for delete with every share mode, every
file opened for write with no byte written and no write time changed, so none is mapped as an
image anywhere, no process of this user maps one, and 228 processes could not be opened,
among them CrowdStrike Falcon, Tanium, Avecto Defendpoint, Numecent Cloudpaging and Autodesk's
File System Monitor. Bader has full control of the folder. The loop's own throwaway folders,
made, renamed and removed in ApplicationPlugins, every one renamed freely: empty, with a text
file, with a DLL, named .bundle, a copy of the bundle's 14 DLLs, a copy with
PackageContents.xml, and one named ParsonsNwcFederatorLoopMeasure.bundle renamed to the
.replaced- shape, each listed before and after in turn4\rename-measure.txt, the folder reading
the same after. So the refusal belongs to that one folder, and what holds it is UNKNOWN. On
2026-09-27 the install.ps1 of then, which removed the bundle and copied the new one, installed
be0b9b37 here. Register row F109, with its developer in wt-f109 since 11:24. F106's developer
was told through turn4\install-done.txt at 11:17:59 not to wait for the install.

Programs started so far: git and gh. Windows PowerShell for Get-Process Roamer, the reads of
the records, the merges' conflict resolution, the reads and measurements above, and one
harmless Start-Process of a powershell told to sleep 90 s, pid 40796, read alive 13 s after
the tool call that started it had ended, by the lead and kept in no file. run.ps1 -Mode Run
for the third start, which started one Navisworks, pid 37988, through the Automation API, with
the two licensing agents above. run.ps1 -Mode Install three times, each running
build\install.ps1, a dotnet build and dotnet build-server shutdown. Git Bash's grep and
sha256sum for the XML reads. tasklist for the bundle's DLL names. F102's mask-evidence.ps1
three times and its check-evidence-ids.sh three times, from the F102 worktree and then from
the F103 tree. The pre-commit's dotnet test at every commit. Agents: through three workflows,
a developer for F105 in wt-f105 and one for F107 in wt-f107, a developer for F106 in wt-f106
and one for F108 in wt-f108, each then read by a reviewer and a breaker, and a developer for
F109 in wt-f109 then a reviewer and a breaker. The claim-checker on these records, twice. The
keep-awake, powershell.exe pid 21164 from 10:48:37, named at the head of this file.

Written outside the repo so far, all under %LOCALAPPDATA%\NwcFederatorLoop unless named:
turn4, holding the commit messages, the pull request bodies, the pre-commit outputs from
fe3c1fa on, item0-set02-masked with the three masked copies, the briefs f106-brief.md,
f108-brief.md and f109-brief.md, xml-reads.txt, actions-reads.txt, roamer-reads.txt,
detach-test-pid.txt, install-done.txt, the waiters wait-run.ps1 and wait-no-roamer.ps1, which
read only, keep-awake.ps1 and keep-awake.txt, the read only diagnostics who-holds-bundle.ps1,
which-level-held.ps1, which-file-mapped.ps1 and who-maps-bundle.ps1 with their outputs, the
measurements rename-measure.ps1, rename-measure-bundle.ps1 and rename-measure-copy.ps1 with
rename-measure.txt, the folder rename-test holding a renamed copy of the installed bundle,
and what the developers write there. runs\02\item0, the run folder of the third start.
installs\398b910e-20261001-105835, -110322 and -112021, one per install attempt. wt-f106,
wt-f107, wt-f108 and wt-f109, worktrees of their fix branches, and wt-main moved to 398b910.
In %APPDATA%\Autodesk\ApplicationPlugins, the eight throwaway folders above, each removed
within the second it was made, the folder listed the same before and after. In the session's
scratchpad, the drafts of this file's head, of the F102 and F103 log entries and of the rule
changes.
