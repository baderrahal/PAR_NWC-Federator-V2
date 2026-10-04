# tools\loop

The scripts the loop runs on Bader's machine. The rules they keep are in
.claude\rules\loop.md. Each PowerShell script is run from the repo root with
powershell -ExecutionPolicy Bypass -File tools\loop\<name>.ps1, bar run.ps1 and
prove-run.ps1, which also take -NoProfile -STA, and nw-guard.ps1, which is only
dot-sourced.

## prepare-copy.ps1

The only thing that reads NM Fed, the folder of real NWC files on Bader's desktop. It finds
the folder itself, so no command ever names it, and the wall in .claude\hooks refuses any
command that does.

- reads: NM Fed, found as [Environment]::GetFolderPath('Desktop') plus NM Fed, because the
  desktop sits under OneDrive. It refuses a file OneDrive holds online only, because
  reading one downloads it into NM Fed
- writes outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\source, a copy of NM Fed with
  every file and every folder, and source.manifest.txt beside it once every copied file
  has been hashed and matched. The copy is kept while it matches NM Fed on every file by
  exact name, size and sha256 and on every folder, and made again when it does not, after
  the room is checked and never before
- -Listing writes one .txt under steps\runs, one line per file with its size, its sha256
  and parts 3 and 5 of its name, and touches nothing else
- -Remove takes one NWC out of a whole copy and writes its sha256 down in
  source.removed.txt. -Restore copies that same file back from NM Fed, refuses when NM Fed
  now holds a different file under that name, and moves it into place only once its hash
  matches. While a file is out, the plain command refuses rather than remaking the copy,
  which would put the file straight back
- -Set NN, two digits, makes the fresh copy one run set works on,
  %LOCALAPPDATA%\NwcFederatorLoop\runs\NN\NMFed. It makes the plain command's check first,
  so the source copy is kept or made again as above, and refuses while source.removed.txt
  is there. Then it copies every file and folder of the source copy into runs\NN\NMFed,
  reads every file back by sha256 and size against source.manifest.txt, makes one empty
  folder under Clash Report for each folder under NWC, read off NWC and never named in the
  script, and writes NMFed.manifest.txt beside it last, in the shape of
  source.manifest.txt. A copy that fails its read back keeps no manifest and is left as it is
- a run set's copy is made once and never emptied, so -Set NN refuses, writing nothing,
  when runs\NN\NMFed, NMFed.manifest.txt or NMFed.removed.txt is there already, and when
  NM Fed has no NWC folder, no Clash Report folder or no folder under NWC
- -Set NN -Remove and -Set NN -Restore do on runs\NN\NMFed what -Remove and -Restore do on
  the source copy, with the note in runs\NN\NMFed.removed.txt. The window run of F106 is
  built to read runs\NN\NMFed, NMFed.manifest.txt and NMFed.removed.txt by those names. The
  copy is named NMFed and never NM Fed, because the wall refuses every command naming NM Fed
- it walks NM Fed one folder at a time and refuses a junction or a link inside it, because
  Windows PowerShell 5.1 follows one when it recurses
- what is a junction or a link, here and for the folders it guards, is the one rule in
  build\links.ps1, which it dot-sources and build\install.ps1 reads too, since F109: the
  link type PowerShell reads off a reparse point, so a file or folder OneDrive syncs, which
  carries the reparse point attribute and no link type, is not taken for one
- deletes: only inside %LOCALAPPDATA%\NwcFederatorLoop
- never writes into NM Fed, and refuses a work folder that overlaps it or is a junction,
  and with -Set a runs folder, a set folder or a set's copy that is a junction or a link

Proved on the real folder: on 2026-09-27 made, kept, and made again when NM Fed changed
under it, and on 2026-09-28 in this form kept, listed without touching the copy, one NWC
removed, restored by hash and kept again, with twelve calls refused with their reason and
nothing changed, among them the plain command while a file is out, a file that is not an
NWC, a wildcard, and a listing outside the repo, outside steps\runs, over steps\logs or
not a .txt.

-Set proved on 2026-10-01, F108. On the real folder with the throwaway set 99: made in 8 s,
141 files and 12 folders, every file read back outside the script by sha256sum and size,
NMFed.manifest.txt byte for byte source.manifest.txt, Clash Report\C06 and Clash
Report\C07 made empty, one NWC removed and restored by hash, and 13 calls refused with
their reason, among them a second -Set 99 and -Set 99 while the NWC was out. runs\99 was
deleted after. In a work folder of its own inside the loop folder: the source copy made and
set 97 made from it, and -Set refused while a file was out of that source copy, at the read
back when one sha256 of its manifest was changed, before copying when a line of its
manifest was not one, and at a junction in place of runs\95. On a copy of the script whose
one changed line reads a fixture folder: Clash Report\K1 and Clash Report\K2 read off NWC,
and -Set refused for no NWC folder, no Clash Report folder, no folder under NWC, a file
marked offline, a changed file on -Restore and a work folder inside the folder it copies.
Not proved: the two refusals for too little room.

## read-workbook.ps1

Reads one clash workbook back off the disk and writes a text read-out: one line per test
with its shape, row, tolerance, clash count, five status counts, type, status and clash
rows, one line per clash row with its status, distance, grid location, clash point and
picture link, the totals, the pictures stored in the workbook and those in the sheet that
point at a file outside it, and every doubt it had. It
shares no code with the tool that wrote the workbook. It opens the xlsx as a zip, reads
the XML, and knows a test by column A in both shapes a workbook holds, the full block and
the one row test this tool writes for a test that found nothing, and by its numbers in C
to K when A is empty. A test or a clash row with no name, a sheet with no test and a
workbook with no sheet are each written down as a doubt, so an empty read-out never
looks like a clean one.

- reads: the workbook named by -Workbook
- writes: the read-out named by -Out, a .txt under steps\runs and never the workbook
  itself, written beside and moved into place, or a READ-OUT FAILED line when it fails,
  a missing workbook included
- writes outside the repo: nothing

Proved on 2026-09-27 and 2026-09-28, read only, on both client exports in
samples\client-report, 1830 tests each with 64 and 65 clash rows equal to their own
totals, on a workbook written by this tool's own WorkbookWriter holding two full blocks
and two one row tests, read as four tests and five clash rows with no doubt, and on the
same shape with one clash and one one row test left without a name, read as four tests
and five clash rows with both named as doubts.

## mask-evidence.ps1

Writes a masked copy of one result file of a run or a probe, so it can be committed without
this machine's name or Bader's Autodesk licensing ids in it. Every such file goes through it
before it is committed, and tools\checks\check-evidence-ids.sh refuses a file that still
carries one of the kinds it knows. It knows no other. F102.
powershell -ExecutionPolicy Bypass -File tools\loop\mask-evidence.ps1 -In <file> -Out <file>

WHAT IT MASKS is every kind in tools\checks\evidence-ids.txt, the one place the rule lives,
which the check reads too, by the same rules, so a rules file one refuses the other refuses.
The kinds are named there and nowhere else. Which kinds read a line is decided on the line
as it came in, before any of it is masked. It prints one line per kind with how many it
masked. Every byte but a masked span comes out as it went in, line endings and UTF-8
included, and a UTF-16 file is written back as UTF-8, because the check cannot read UTF-16.

WHERE EACH GUARD READS, which is narrower than it may look.
- the pre-commit reads only in a clone where it is switched on, git config core.hooksPath.
  On Bader's machine core.hooksPath is the ABSOLUTE path of the main clone's .githooks,
  measured on 2026-09-29, so a commit in any git worktree starts the main clone's copy of
  the hook. Since F102 a hook hands over to the committed tree's own copy when that is
  another file, so the tree is read by its own hook and its own check, once the copy that
  starts carries the handover. It hands over only to a copy holding the line that runs the
  evidence check, and refuses the commit when the tree's own copy does not. A main clone on
  a branch older than F102 runs its own old
  copy all through, and then a branch runs its own by hand, sh .githooks/pre-commit
- Actions reads the tree for the ids and for the RUNNER'S name, never for Bader's. Only the
  pre-commit on his machine reads for his, over what is staged
- the check cannot read a zip and refuses one, and no zip of run evidence is committed,
  Bader's answer Q90 A: a run file over 20 MB stays in the work folder and the turn names it
  with its size and sha256
- the check reads words. An id spelled in a way no kind names is not seen by either
- the check also reads a file's path under the folder it reads, which the mask does not, so
  a copy is named plainly and never after an id or the machine

- reads: the file named by -In, held open against every writer until the copy is in place,
  so no spelling of -Out that reaches the same file can change it, and the rules file
- writes: the file named by -Out, making its folder when it is not there, written beside
  and moved into place, then read back off the disk with the check's own rule for a line,
  the machine name anywhere, even inside a longer word, and for a NUL byte. Anything left
  there and the copy is deleted, the kind and the line number are printed and never the
  text, and it exits 1
- refuses, writing nothing: -In equal to -Out, a missing -In, an -Out under samples,
  steps\logs or bundle, an -Out already there without -Replace, no COMPUTERNAME or one the
  rules file does not allow, a rules file it cannot read, and a file holding a NUL byte with
  no UTF-16 byte order mark
- writes outside the repo: only the -Out it is given

Proved on 2026-09-29, in %LOCALAPPDATA%\NwcFederatorLoop\turn3\f102, on copies of the probe
result, the reflection file and scan.md of fix-F100, which the check refused on 12 lines
before and passed after, with only lines 1, 446 and 447 of the result changed. On fabricated
samples with all zero GUIDs and a made up machine name: every kind masked, the near misses
left, CRLF and a UTF-8 letter kept, UTF-16 written as UTF-8, a second pass masking nothing,
the name inside a longer word refused on the read back, and each refusal above made once,
among them an -Out reaching -In through a junction, refused by the hold with -In unchanged.
Proved again after the second reading: a line with an analytics id after -i and a second
GUID, which the mask had left half masked in a copy the check passed, is masked whole, and
the check and the mask refuse the same five faulty rules files and read the whole one.
Actions masks tools\checks\broken\EvidenceWithIds.txt on every run and compares the copy with
the one kept beside it, and puts each file of tools\checks\broken\rules in the place of the
rules file for both.

## prove-hooks.sh

Feeds every case to the three hooks on standard input and prints each answer against the
one it should give. sh tools/loop/prove-hooks.sh <hooks folder> <repo root>. It clones the
repo into a temp folder to prove the git wall with main checked out, and removes it.

How a change to a wall is proved with it and copied in is in .claude\rules\loop.md.

## run.ps1

F103 part 1, built from the design in steps\notes\f103-design.md. Always started as

    powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\run.ps1 -Mode <mode> ...

It refuses to run anywhere else: not Windows PowerShell 5.1, a 32 bit process, an MTA
thread, or a script that is not the one its own powershell.exe was started to run. Its
modes:

- Check, the default. Reads only and writes nothing. Prints every Roamer, unproved-starts.txt,
  every run folder whose record.txt has no VERDICT line, the installed add-in's stamp against
  bundle-backup and the newest loop install, his logs folder and his AutoSave folder against
  their backups by name and sha256, the source copy's marker files, and every refusal Run
  would give for a -Set, -Item and -Stamp. Exit 0 when Run would go, 2 when it would refuse
- Install -Stamp <8 hex>. Refuses while any Roamer runs, unless HEAD is that commit, and
  unless git status prints nothing, untracked files included, because the build stamp
  reads +edits for any of them. Copies an installed bundle that matches neither
  bundle-backup nor the last loop install into bundle-backup-yyyyMMdd-HHmmss and reads it
  back, runs build\install.ps1 as a child, shuts the build servers down, and reads the
  installed stamp back. build\install.ps1 itself refuses with one REFUSED line and exit 2
  while any Roamer runs, read immediately before it moves the installed bundle aside, so a
  Navisworks started during the build stops the install, and when a folder from %APPDATA%
  down to the bundle is a junction or a link. It moves the installed bundle aside by one
  rename, copies the new one in and checks it, and removes the one moved aside only once
  every check has passed. On a failure after the move the new one is taken out and the old
  one put back where it was, and where each ends up is printed. Since F109, when Windows
  refuses that rename, it reads the process list again and refuses, exit 2, while any Roamer
  runs or the list cannot be read. With none it refuses, exit 2 and nothing written, when the
  name the copy would take is taken already, when a file of the bundle cannot be read or a
  junction or a link is inside it, and when a file of it is marked ReadOnly, Hidden or System
  or a folder ReadOnly, each marked one named. Otherwise it lists the installed bundle by
  sha256, copies it beside it under the name the rename would have given it and reads the
  copy back against that listing, refusing with exit 2 when the copy cannot be made whole,
  then writes each new file over the old one, removes the files and folders the new one does
  not have, reads the bundle back against the new one by sha256 and runs every check, one IN
  PLACE line saying so and what a stop part way leaves. On a failure it writes the copy back
  over the bundle against that listing, never against what the copy holds by then, reads it
  back and names where each thing is, and when it cannot, says whether the copy still reads
  whole. On success it removes the copy, or names it in the LEFT line when it will not go.
  Install passes a refusal of install.ps1 on as exit 2, its refusal lines masked, and any
  other failure of it as exit 1, naming install.txt and no folder left beside the bundle,
  which it reads only after an exit 0, T3-G15. A Roamer found right after the install, and a
  folder install.ps1 could not remove left beside the bundle, each make the verdict a FINDING
  and the exit 5
- Run -Set NN -Item 0 -Stamp <8 hex>, the start with no window. Its refusals in order: the
  host, the parameters, the loop's lock Local\NwcFederatorLoop.run, a record with no
  VERDICT line, a start in unproved-starts.txt still running, any Roamer, the installed
  stamp, and evidence already in steps\runs\NN\item0, bar the evidence of a run that was
  NOT RUN, which is moved aside as item0-aside-yyyyMMdd-HHmmss and never emptied. Then the
  run folder, the watchdog, the backups of his logs folder, his AutoSave folder and his
  Navisworks settings, the keep awake request, the last read of the unproved starts and the
  Roamers, and the start through the Automation API. The one start is adopted by the four
  conditions and held through its handle, set Visible, and held 360 s while the monitor
  reads its processor time every 15 s. Then Dispose, and the one close through the held
  handle if it still runs 60 s later. Visible and Dispose each have 120 s, and a call that
  has not returned by then is closed by the watchdog through the held handle and the run
  ends STOPPED. ExecuteAddInPlugin is never called, so the tool's window never opens,
  nothing is written into his logs folder and no log there is read as the tool's. The close
  at the end waits for any close the watchdog began, closes a process that still reads the
  same after it, and never closes one already gone. M5, what changed outside the loop folder
  while the start ran, by any program, is read before the put back, so the put back's own
  writes are never among it, and the watchdog runs until just before the put back reasons
  are read, so a Navisworks that starts while M5 is read is in its record. The AutoSave compare reads autosave-before.txt back off the disk,
  outside the put back's try, and holds it against the backup's own list by count and name.
  When it cannot be read or the two differ, the backup's list stands in and the record says
  so, naming every file in one and not the other. A process
  the watchdog closed is written CEILING or STOPPED, read off what the watchdog forced, and
  never as one that ended by itself, and a process that cannot be read is UNKNOWN, never
  GONE
- CloseOwn -RunFolder <runs\NN\item...>, after a run.ps1 died. Closes that folder's
  Navisworks only when its record has no VERDICT line, its mypid.txt reads adopted, and the
  pid is a process named Roamer with the install's Roamer.exe, the automation command line
  and the same start ticks. Lists the settings compare, writes nothing back, and appends
  VERDICT: CLOSED BY CLOSEOWN

- Run -Set NN -Item 1 to 5 -Folder <a folder of NMFed\NWC> -Stamp <8 hex>, F106, the window
  run on the run set's copy runs\NN\NMFed, which tools\loop\prepare-copy.ps1 makes. Item 1
  takes -Xml, a file of the copy, and item 5 -OpenFile, the plain name of an .nwf in
  NMFed\NWF\<Folder> or an .nwd in NMFed\NWD\<Folder>. Every path must resolve under the
  copy. Item 0's refusals, then the copy whole against NMFed.manifest.txt, the outputs as the
  item needs them, item 1's three output folders empty, and the session unlocked, Q85. The
  run folder is runs\NN\item<K>-<Folder>, item 5's ends -nwf or -nwd, and its evidence goes
  to steps\runs\NN under the same name. Item 0's backups, start, adoption, watchdog and
  Visible, then for item 5 the file copied into the run folder's open\ and opened there with
  OpenFile, then tools\probes\drive-window-run.ps1 started as a child with the adopted pid
  and start ticks, its handle held, then ExecuteAddInPlugin("ParsonsNwcFederator.PARS") on
  the main thread, which holds while the window is open. The monitor reads as the tool's log
  the one run-*.log made after that call whose SESSION names the installed stamp, and names it
  and its .tsv in toollog-name.txt. HUNG is the log still for 300 s with under 20 s of
  processor time in them, Q83, and the clocks start again while the session reads locked.
  Once the log holds its RESULT block and a COPY line and has been quiet 15 s, or the driver
  stopped with nothing that runs pressed and no run started, it posts WM_CLOSE to the tool's
  window of the adopted pid, and only while no other window of that pid is up. Then
  Dispose and item 0's put back, and his AutoSave folder by Q86: the autosaves the run added
  removed and his it changed put back from autosave-backup, each read back, only when no
  Navisworks the loop did not start ran, otherwise listed and left. The verdict is RAN only
  when the log on disk holds a RESULT block and a SESSION naming the stamp, and for item 1 a
  GROUPS block reading 0 groups unticked. Items 2 to 5 have run on no Navisworks yet

Exit codes: 0 finished and everything put back, for item 5 on an NWD also the tool's own
refusal, TOOL REFUSED, 1 a fault in run.ps1, UNKNOWN whether the adopted Navisworks still
runs, one still running after every close path, or a window run whose log does not show it
RAN, 2 refused, for Install also a refusal of build\install.ps1, 3 not adopted or the
constructor deadline, 4 hung, the ceiling, a call into the adopted Navisworks that did not
return in 120 s, or the tool's window still open 120 s after WM_CLOSE, 5 finished but a
dialog appeared, or for Install installed but a Navisworks ran right after it or the add-in
installed before was left beside it, 6 something of Bader's not put back, 7 the adopted
Navisworks or the tool's window ended by itself, 8 the driver stopped before it pressed
anything that runs.
RunVerdict and InstallVerdict decide them, each a function the harness calls.

The numbers that shape a run are constants, not parameters, so no switch moves a path or
shortens a limit: the hang rule's 300 s and its 20 s of processor time, Q83, the constructor
deadline's 300 s, the ceiling of 12 hours from adoption, Q84, recorded as CEILING and never
as HUNG, the 120 s a call into the adopted Navisworks may take, which is also the time the
tool's window has to close after WM_CLOSE, the 600 s OpenFile may take, the hold of 360 s,
the 15 s the tool's log stays quiet after its RESULT block, the monitor's pass of 15 s and
its heartbeat of 60 s. The keep awake request, ES_CONTINUOUS,
ES_SYSTEM_REQUIRED and ES_DISPLAY_REQUIRED, is made on the main thread just before the last
read before the start and let go in the run's finally, and in the outermost finally if that
could not, and Windows drops it by itself when the process ends. Every part of the run's finally runs in its own try, so a fault in one never skips
the close, the watchdog's end, the put back, the keep awake release or the verdict.

- reads: the process list, unproved-starts.txt, the runs folder, the installed bundle, his
  logs folder and AutoSave folder, HKCU\Software\Autodesk\Navisworks Manage\22.0 and every
  file of %APPDATA%\Autodesk\Navisworks Manage 2025, and for M5 the write times of the keys
  under HKCU\Software\Autodesk and the files under %TEMP%, %LOCALAPPDATA%\Autodesk,
  %APPDATA%\Autodesk, %PROGRAMDATA%\Autodesk and %APPDATA%\Microsoft\Windows\Recent written
  at or after the start
- writes outside the repo, all under %LOCALAPPDATA%\NwcFederatorLoop: runs\NN\item0 with
  record.txt, watch.txt, mypid.txt, logs-before.txt, logs-after.txt, autosave-before.txt,
  hang-tail.txt at a hang, settings\ with the export, the copies and before.clixml,
  settings.txt and m5.txt, the run folder of an earlier call moved aside as
  item0-aside-yyyyMMdd-HHmmss and never emptied, logs-backup\since-yyyyMMdd-HHmmss and
  autosave-backup\since-yyyyMMdd-HHmmss for files the backups did not hold,
  probes\unproved-starts.txt for a start it could not prove, and for Install
  installs\<stamp>-yyyyMMdd-HHmmss and bundle-backup-yyyyMMdd-HHmmss
- writes of a window run, F106: runs\NN\item<K>-<Folder> with driver.txt, toollog-name.txt,
  toollog\ with the tool's log, its FOLDERS REMEMBERED block masked, Q87, and its .tsv,
  outputs.txt, item 5's open\, and what the tool writes into the copy's NWF, NWD and Clash
  Report folders. A read-out of every workbook by tools\loop\read-workbook.ps1 goes into the
  evidence, and a file over 20 MB is named with its size and sha256 and not copied, Q90.
  It starts the driver, powershell.exe, and read-workbook.ps1 once per workbook
- writes of Bader's: his Navisworks settings, put back only by the D2 rule in
  .claude\rules\loop.md, his AutoSave folder by Q86 under the same rule, for a window run his
  logs folder, where the tool writes its own log and .tsv and may prune his oldest logs,
  which logs-backup holds by sha256, Q82, and for Install the installed bundle, which build\install.ps1 moves
  aside as ParsonsNwcFederator.bundle.replaced-yyyyMMdd-HHmmss beside it until every check
  of the new one has passed, and a new one that failed and will not go, moved aside as
  ParsonsNwcFederator.bundle.failed-yyyyMMdd-HHmmss. When that move is refused with no
  process named Roamer running, it copies the installed bundle beside it under the same
  .replaced- name instead, and writes the new files over the old ones where they are
- writes in the repo: steps\runs\NN\item0 with record.txt, watch.txt and settings.txt, to
  be masked before any commit, and the evidence of a NOT RUN moved aside as
  steps\runs\NN\item0-aside-yyyyMMdd-HHmmss, never emptied
- deletes: nothing itself. build\install.ps1, run by Install, removes the bundle it moved
  aside once every check of the new one has passed, and a new one that failed. In place it
  removes the files and folders of the installed bundle the new one does not have, and its
  copy beside it once every check has passed or once the copy is written back after a
  failure

## nw-guard.ps1

The loop's guard code, in one copy, a file of functions with no main body. Every function
of the probe tools\probes\probe-automation-start.ps1 that its watchdog also uses, and every
guard it wrote inline, was moved into it in commit 377cb1a and nothing else changed there,
the inline ones wrapped as functions whose bodies are the probe's lines.
steps\notes\f103-move-proof.txt maps every line of it at that commit to its line of the
probe, or names it wrapper, comment, blank or changed with both texts. The probe, run.ps1
and prove-run.ps1 dot-source it, and each runspace they start gets its text.

What F103 changed or added after the move, which its header lists too: the held handle in
AdoptStart, HeldRead with the reason a read failed and HeldState, the one close
CloseAdopted, which the probe's three Stop-Process calls became, the watchdog's adopted
deadline writing why it closed before it closes, ReadShared for a file another thread
appends to, WindowRecords under WindowLines with the text of the visible children of every
visible top level window but the main window and the class, caption, visibility, state and
process of each window's owner, which run.ps1 writes masked for an owner of another process,
at most 2 s of such reads per call with what was not read written, ChildHandlesOf, the walk
of one window's children stopped at 200 of them or at the 2 s, WindowKind, which calls a
window MAIN when it has the main window's class and caption and no owner or an owner that is
not visible, the owner the second real start measured at 13:39:20 on 2026-09-30, record
steps\runs\01\item0 line 33, class WindowsForms10.Window.0.app.0.27a2811_r7_ad1, caption
"", visible False, enabled True, in the adopted process, the watchdog's deadline for a
call into the adopted Navisworks and the lock it holds for the whole of a close, which the
close at the end takes too, the key BackupSettings exported printed as that key, the
summary SettingsPutBack returns, and
seven calls in the emitted window type: GetWindow, IsWindowEnabled, GetCurrentThreadId,
WTSQuerySessionInformationW and RegQueryInfoKeyW, which read, WTSFreeMemory, which frees what
the session read returned, and SetThreadExecutionState, which asks Windows to stay awake.
Nothing is compiled and nothing is written under %TEMP%.

## prove-run.ps1 and StandIn

The proof of run.ps1 and nw-guard.ps1 with no Navisworks, the design's cases H0 to H15 for
the part 1 modes and M1 to M3, and since fix attempt 1: H12b, copies of build\install.ps1
run against a fake APPDATA, H16, a window whose thread is blocked, and H17, a copy of
run.ps1 whose constructor line is removed, run against fake LOCALAPPDATA and APPDATA folders
through checks 13, 14, 15 and 18 and to the removed line, since fix attempt 2: H18, the
end of a run, the call deadline, the verdict, the one listing reader and the bounded walk,
and since F109: H12c, more copies of build\install.ps1 against the fake APPDATA, each with one
file of the fake bundle held open by a child powershell, which refuses the move aside, so
the bundle is replaced in place, the first of them main's install.ps1 at 398b910, the one
Windows refused on 2026-10-01, and since F109 attempt 2 a junction inside the bundle, the
copy's name taken, marked files and folders, a copy that loses a file before the put back,
and the rule of build\links.ps1 read on a junction and on the clone's bundle folder under
OneDrive:

    powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\prove-run.ps1 -Work <a new folder under %LOCALAPPDATA%\NwcFederatorLoop>

It refuses while any Roamer runs and refuses a -Work folder that is there already, and
before and after every case it reads the process list again and stops on a Roamer it did
not start or a list it cannot read. It loads run.ps1's functions through the parser, so
run.ps1's main flow never runs in it, and calls the real run.ps1 only in Check, which reads
only, in Run and Install calls made to be refused, each made only after it reads a stand-in
Roamer running and an installed stamp that is not the one passed, so two other checks would
refuse it too, and in CloseOwn calls, which close only a process whose path is the
install's own Roamer.exe, which no stand-in has. After every case it reads his logs folder,
his AutoSave folder, the installed bundle, an export of his 22.0 key and the loop folder
outside the turn folders and -Work, and each must read as at the start. H12c also reads the
bundle folder of the clone the checkout belongs to, for attributes, link types and sha256
alone, and reads no file OneDrive holds online only.

StandIn is a small net48 exe named Roamer.exe that is not Navisworks, outside the solution
and the bundle, built by the harness with dotnet build into -Work. It takes its role from
its arguments or from NWCLOOP_STANDIN: sleep, spin, write a RunLog with the repo's own
Federator.Core, one RunLog.Start for M1, a message box and a WinForms dialog, a decoy that
logs every WM_GETTEXT and WM_CLOSE sent to it from another process, a window whose thread
blocks once it is shown, or three windows under the main window's caption, one with no
owner, one owned by a window that is not visible and one owned by the first. Every role
ends by itself.

- writes outside the repo: -Work, and the throwaway key HKCU\Software\NwcFederatorLoopTest
- deletes: -Work and that key at the end, and nothing else
- starts: dotnet build and dotnet build-server shutdown, the stand-ins, child powershell.exe
  processes, among them the copies of build\install.ps1 and of run.ps1 under -Work, one
  that loads a copy of Federator.Core.dll and one that holds a file of the fake bundle open,
  reg.exe export, git for scratch repositories under -Work, and cmd.exe for one junction
  under -Work

Proved on 2026-09-29 with no Navisworks started: 142 checks passed and 0 failed, 28
stand-ins each closed through its held handle, Bader's folders, bundle and key read the same
after every case, and Get-Process Roamer read 0 before and after. Proved again on
2026-09-30 after fix attempt 1: 189 checks passed and 0 failed in 670 s, 36 stand-ins, with
the new cases of H6, H7, H10, H12b, H16 and H17, and after fix attempt 3: 242 checks passed and 0 failed in 1126 s, 43 stand-ins, with the new cases of H7, H12, H12b, H17 and H18.
Proved again on 2026-10-04 for F109, on the files as committed before main was merged in:
254 checks passed and 0 failed in 1356 s, 44 stand-ins, with H12b item 10 read by the new
rule and the twelve checks of H12c. With main merged in, which brought F106's run.ps1 and
nw-guard.ps1: 244 passed and 10 failed in 1647 s. Every check of H12, H12b and H12c passed.
Seven failures are checks of H0 and H6, F106's R1, as its DONE line in steps\01_next.md
says. Three are the read of Bader's state after H17, H18 and H15, which found the loop
folder's source copy made again at 11:48:01 by something outside the harness.
Among them M1, M2 and M3: with 30 fabricated logs held open without delete sharing, the
real RunLog.Start prune wrote RETAIN keeping 30 logs, deleted 0, could not delete 1, and
lost nothing, where the same folder with no handles lost its oldest.
FileVersionInfo.ProductVersion equals the informational version on this repo's build and
the installed build. The keep awake request returned 0x80000000 and its release 0x80000003
on the same native thread.

The first two real starts, item 0 with no window, ran on 2026-09-30, their records in
steps\runs\00\item0 and steps\runs\01\item0. The first, on fcd981b from 11:29:29 to 11:40:10,
adopted the Roamer COM started through its handle and ended exit 5 on one DIALOG finding
that was the main window itself, which has an owner. The second, on 867697a from 13:37:37
to 13:48:08, ended exit 0, VERDICT RAN, the main window read MAIN by its owner that is not
visible, record line 33, and the session lock read unlocked at every minute.
