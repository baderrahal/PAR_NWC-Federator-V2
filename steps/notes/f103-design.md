# F103 design: tools\loop\run.ps1

The merged design of two independent designers, safety first and fewest moving parts, judged and merged by a third, 2026-09-29. Design only, nothing built. Saved by the lead as returned.

## summary

tools\loop\run.ps1 is one Windows PowerShell 5.1 script with four modes, Check, Install, Run and CloseOwn. It is always started as powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\run.ps1.

Run does one item of the run set, Items 1 to 5. It also does Item 0, the measure start, which comes in two forms. With no window it is allowed before Bader answers the log question. With -OpenWindow it comes only after he answers.

run.ps1 is built on F100's start and close. Once F100 has run and merged, that code moves, unchanged, into tools\loop\nw-guard.ps1. The probe, run.ps1, the window driver and the proof harness all dot-source that one file, so no logic has two copies.

WHERE THE TWO DESIGNS DISAGREE, AND WHAT WAS PICKED:
- Reuse. Both designs move the probe's code into one shared file. Design 2's list is taken, because the backup, the adoption, the close and the put-back reasons are written inline in the probe today, not as functions. They must be wrapped as functions, and design 1 treated them as functions that already exist. The step 2 read of unproved-starts.txt is also inline, and both designs missed it, so it is wrapped too. The move lands after F100's one run and merge, so the probe that ran is the probe that merged.
- Modes. Design 2's Run with an -Item number is taken, plus design 1's CloseOwn. One item number is fewer moving parts than a list of kinds. CloseOwn is the only safe way to close the loop's own Navisworks after run.ps1 dies, and a stopped background task can kill it.
- Tests. Design 2's separate harness, tools\loop\prove-run.ps1, is taken over design 1's SelfTest mode, so run.ps1 has no switch that can move its paths or shorten its hang limit. Design 1's stand-in is taken over renamed copies of ping.exe or powershell.exe. The stand-in is one small net48 exe named Roamer.exe, built like tools\probes\ViewpointProbe. One program plays every role, and a renamed system program is what security tools on a managed machine flag.
- Pinning the process. Design 1's held handle is taken. After adoption run.ps1 opens the Process object's handle and reads the start ticks again, and every close goes through that handle. While the handle is open, Windows cannot give the pid to another process.
- His logs. Design 1's lock is taken over design 2's ReadOnly attribute. The lock holds his logs open for reading with no delete sharing. It writes nothing to his files, and Windows drops it by itself if run.ps1 dies, so nothing is left to clear.
- Watching. Design 2's one monitor runspace is taken, so the watching works whether or not ExecuteAddInPlugin blocks.
- Closing the tool window. Design 2's PostMessageW is taken, one line added to the emitted type. Design 1 used SendMessageTimeout with a dummy buffer.
- Hang rule. One rule in every mode, from the moment the tool's log exists, as design 1 has it. It lives in design 2's pure HangVerdict function, so the harness can call it with 20 s.
- Run 5b. Design 2 is right. FederatorWindow.xaml.cs lines 2348 to 2351 disable RunOpenButton for an NWD, and OpenDocumentLine carries the WhyNot sentence (OpenDocumentJob.cs lines 290 to 295), so there is no Warn dialog to press. Design 1's press of OK on a Warn is dropped.
- Evidence. Design 2 is taken: run.ps1 writes steps\runs\NN\item<K> itself, because read-workbook.ps1 writes only under steps\runs.
- Keep awake. The request is made just before the last check before the start and released in the outermost finally, so every way out releases it. CallNtPowerInformation and powercfg are dropped.
- The driver. Design 1 is taken. It finds windows only under the adopted pid, reads back every box, and refuses any path outside the loop folder. It loses Add-Type, FindWindow, GetTopWindow, the mouse click and -Tolerance, so nothing is compiled into %TEMP% and nothing clicks.

GUARDS BOTH DESIGNS MISSED, NOW ADDED:
- AutoSave. His folder %APPDATA%\Autodesk\Navisworks Manage 2025\AutoSave holds 196 NWF autosaves, 286 MB (read today, reading only). The newest was written at 17:00:28 on 2026-09-27, 8 s before the log of his 16:37 run ends. So a run's Navisworks writes there, under the same document names as his. Every file there that is not already in NwcFederatorLoop\autosave-backup is copied in before each start, and nothing is written there without Bader.
- The build stamp. Directory.Build.targets adds +edits whenever git status --porcelain prints anything, untracked files included. This clone has an untracked .claude\worktrees today, so a build here would not prove main. Install refuses unless HEAD is the commit asked for and the tree is clean, and the installed stamp must read exactly that commit.
- A run that died. A record.txt with no VERDICT line refuses the next Run until the lead writes one.
- The tool's log names his folders. At every window open the tool checks that each folder in his folders.txt still exists, and copies the list into its log. The list includes NM Fed, measured in steps\loop.md Phase 1 item 5. So F102 must mask that block before any loop log is committed, and Bader says whether that check of NM Fed is acceptable.
- A start with no window, before Bader's answer. The probe's run 3 wrote 0 files into his logs folder (automation-start-result-20260928.txt line 553). So a start that never calls ExecuteAddInPlugin can prove run.ps1's whole start, watch and close on a real Navisworks, and measure idle processor time for the hang rule, before any window opens on main.
- Run length. The whole folder is 45 buildings and 140 NWC files (steps\runs\00\source-listing.txt), so a first run may take hours. The design's ceiling is 12 hours, not 8.

THE LOG PROBLEM needs Bader whatever is chosen. On main the window can only log into his folder, so no path keeps every guard and D4 together. The least bad path is A: hold his logs open during the run, then copy the loop's own log and tsv out and remove exactly those two files. Until he answers, no window opens on main.

D4 ORDER:
1. F102.
2. F100's run and merge.
3. F103: run.ps1, nw-guard.ps1, the driver change, the stand-in and the harness, merged with the harness output in its body.
4. Item 0 with no window.
5. Install main from a clean checkout.
6. Bader's answer on the logs.
7. Item 0 with the window.
8. The baseline, items 1 to 5, through the real window.
9. Only then F101.

No change to src lands before the baseline.

CRITICAL FILES:
- tools\probes\probe-automation-start.ps1
- tools\probes\drive-window-run.ps1
- src\Federator.Core\Diagnostics\RunLog.cs
- src\Federator.Addin\FederatorPlugin.cs
- src\Federator.Addin\Ui\FederatorWindow.xaml.cs
- Directory.Build.targets
- build\install.ps1
- .claude\rules\loop.md
- new: tools\loop\run.ps1, tools\loop\nw-guard.ps1, tools\loop\prove-run.ps1, tools\loop\StandIn

## measurements first

- M1, no Navisworks, answered by harness case H5. Hold every run-*.log of a stand-in folder open for reading, sharing read and write but not delete. Then call RunLog.Start(folder, now, 30) from the repo-built Federator.Core.dll. Does it write RETAIN keeping 30 logs, deleted 0, could not delete 1, and do all 30 files read back with their sha256? The control run without the handles must read deleted 1. Choice A and choice B both rest on this, and it is UNKNOWN until the real code runs against it.
- M2, no Navisworks, answered by H12. Does FileVersionInfo.ProductVersion of a built Federator.Addin.dll equal the AssemblyInformationalVersion read off a copy with ReflectionOnlyLoadFrom? If it does, run.ps1 reads the installed stamp without loading or locking the installed file. If it does not, run.ps1 reads the stamp off a copy made in the run folder.
- M3, no Navisworks, answered by H11. In a powershell -File process, does the release call of SetThreadExecutionState return exactly 0x80000003, on the same native thread id that made the request? UNKNOWN. If the thread changes, the request would lapse early.
- M4, the start with no window. Opening Get-Process on the adopted Roamer and reading .Handle asks for full access to a process of the same user that COM started. Does that succeed, and do the start ticks still read equal afterwards? Then Navisworks sits idle and Visible for 360 s with nothing open. What does TotalProcessorTime do, sampled every 15 s? If it is never flat for 300 s, the hang rule as written can never fire on an idle Navisworks, needs_bader 3.
- M5, the start with no window, repeated on the first window start and on run 1. What does a start write outside the loop folder? Record the name, size and write time of every file newer than the call under %TEMP%, %LOCALAPPDATA%\Autodesk, %APPDATA%\Autodesk, %PROGRAMDATA%\Autodesk and %APPDATA%\Microsoft\Windows\Recent. Record the LastWriteTime of every key under HKCU\Software\Autodesk, before and after. Everything is listed and masked, and nothing is copied or touched. This completes the outside list.
- M6, the start with no window and every run after it. Does the session lock while ES_DISPLAY_REQUIRED is held? Read once a minute through WTSQuerySessionInformation with WTSSessionInfoEx, reading only. PQ8 asks whether UI Automation and pictures still work on a locked screen, and this does not answer that.
- M7, the first window start, which needs Bader's answer on the logs. run.ps1 holds the object and never disposes it early, with Visible set to True. It calls ExecuteAddInPlugin("ParsonsNwcFederator.PARS") with an empty string array. Does the tool's window stay open? Does the call block until Execute returns, or return at once, and what int does it return? One reading of the 8.5 s close on 2026-09-19 is that the launcher disposed at once: 5z-d measured 8.0 to 9.3 s from Dispose to exit. That reading is unmeasured.
- M8, the same window start, PQ4. Does an Automation start load the installed bundle, with no AddPluginAssembly? The SESSION plugin version line and the window title must both carry the installed stamp. If the plugin is not found, nothing else is tried until the lead decides, because loading the DLL by hand changes what the baseline runs.
- M9, the same window start. Does WM_CLOSE, posted to the tool window of the adopted pid, end the dialog with Window closed. in the log? How many seconds later does ExecuteAddInPlugin return? After Dispose, how long until the pid is gone? Does Dispose raise a save prompt?
- M10, the same window start. What does TotalProcessorTime do while the tool window sits idle and open for 240 s, sampled every 15 s?
- M11, the same window start. Does the real log's RETAIN line read could not delete run-20260901-191711.log, with deleted 0? Do all 30 of his logs still read back with their sha256?
- M12, run 1, reading only until Run is pressed. Does AutomationElement.FromHandle, given a handle found by EnumWindows under the adopted pid, return an element whose ProcessId is that pid? The ids the driver uses were read off main's XAML today, not off a running window: SourceFolderBox, IncludeSubfolders (default on), NwfFolderBox, NwdFolderBox, ExcelFolderBox, ExchangeFileBox, RunButton, RunOpenButton, OpenDocumentLine, FilesGrid, and the tabs 1. Source, 3. Outputs and 4. Clash. Does FilesGrid answer GridPattern.RowCount after Scan? A miss stops the driver before Run is pressed, so it costs one start and nothing else.
- M13, run 1 and every run after it. Which files in his AutoSave folder does a loop run add, change or remove, by name? Does one of his change or go? Compared with DiffAutoSave against autosave-before.txt.
- M14, run 3, PQ6. When the weekly run opens an NWF that points at the removed NWC, does Navisworks raise a dialog? Of which class? Does its text come back through WM_GETTEXT to its child windows? The UI Automation fallback for dialog text is built only if a Navisworks dialog turns out to carry no child text.
- M15, run 5. Does $app.OpenFile open an NWF and an NWD, and which dialogs does it raise? So far it was measured on an NWC only. For the NWD, does RunOpenButton read disabled, and does OpenDocumentLine carry the WhyNot sentence, as the code says? Nothing about the refusal reaches the log beyond the SESSION open document line.
- M16, every window run. Does the tool's log land in his folder and never in the temp folder? StartOrDisabled falls back to the temp folder only when Start throws. If it does, the monitor finds no log, which is a finding, and the hang rule cannot start.

## modes

- CHECK, the default mode. It reads only and writes nothing, and it prints to the console. It prints:
- every Roamer, by pid and start time
- unproved-starts.txt
- every run folder whose record.txt has no VERDICT line
- the installed Federator.Addin.dll stamp, compared with bundle-backup and with the newest installs\*\installed.txt
- his logs folder against logs-backup, by name and sha256
- his AutoSave folder against autosave-backup
- source.manifest.txt and source.removed.txt
- for a given -Set and -Item, every refusal from 4 to 11 that Run would give, in Run's order.
Exit 0 when Run would go, 2 when it would refuse.
- INSTALL -Stamp <8 hex characters>. It takes the lock, then runs refusal 6. Then:
1. git rev-parse --short=8 HEAD must equal -Stamp, and git status --porcelain must print nothing, untracked files included. These are the two reads Directory.Build.targets makes, so the stamp can only come out as exactly -Stamp.
2. Every installed file is hashed and compared with bundle-backup and with the last loop install. A bundle that matches neither is copied into bundle-backup-yyyyMMdd-HHmmss and read back by sha256.
3. Refusal 6 again.
4. powershell -NoProfile -ExecutionPolicy Bypass -File build\install.ps1 runs as a child, with no -SkipBuild, so it builds the commit it installs. Its output goes into installs\<stamp>-yyyyMMdd-HHmmss\install.txt.
5. dotnet build-server shutdown, so no build server the install started keeps running.
6. Refusal 6 again, where a Roamer is a finding.
7. installed.txt gets every installed file with its sha256. The installed ProductVersion must name -Stamp with no +edits.
Exit 0 when installed, 2 when refused.
- RUN -Set NN -Item 0 -Stamp <8 hex>, the start with no window. Refusals and backups, then start, adopt, hold the handle, Visible True, 360 s idle with processor time sampled every 15 s, Dispose, put back. ExecuteAddInPlugin is never called, so nothing is written into his logs folder, as the probe's run 3 measured. That makes this start allowed before Bader answers the log question. It measures M4, M5 and M6 and proves run.ps1's start, watch and close on a real Navisworks.
- RUN -Set NN -Item 0 -OpenWindow -LogChoice <his answer> -Stamp <8 hex>, the start with the window. The same as the start with no window, plus the log lock, ExecuteAddInPlugin and 240 s with the window open. 240 s is fixed, so the hang rule's 300 s cannot end the start before its own WM_CLOSE. Then WM_CLOSE, Dispose and the log handling. No driver is started. It measures M7 to M11 and M16.
- RUN -Set NN -Item 1 -Xml <a file under %LOCALAPPDATA%\NwcFederatorLoop\source> -LogChoice ... -Stamp ..., the first run. runs\NN\out must be absent or hold no file, and source.removed.txt must be absent. The driver types:
- -Source %LOCALAPPDATA%\NwcFederatorLoop\source, with Include subfolders read back as on
- the out\NWF, out\NWD and out\Excel folders
- the XML.
Every group is ticked by the window's own default, and the GROUPS block of the log must read no group unticked.
- RUN -Item 2, the weekly run. The same folders, no XML. out\NWF must hold at least one .nwf, item 1's record must read VERDICT RAN, and source.removed.txt must be absent.
- RUN -Item 3, a file gone. The lead runs tools\loop\prepare-copy.ps1 -Remove <one NWC> first. source.removed.txt must be there. The building of the removed NWC, part 3 of its name, must hold two or more disciplines, part 5, in source.manifest.txt. Every other file must match the manifest. The same folders, no XML.
- RUN -Item 4, a file back. The lead runs prepare-copy.ps1 -Restore with the same NWC first. source.removed.txt must be gone and every file must match the manifest. The same folders, no XML.
- RUN -Item 5 -OpenFile <the plain name of an .nwf in runs\NN\out\NWF or an .nwd in runs\NN\out\NWD>. The file is copied into runs\NN\item5-nwf\open or item5-nwd\open and read back by sha256, so the outputs of runs 1 to 4 stay as they were. After adoption and before ExecuteAddInPlugin, $app.OpenFile opens the copy. The driver runs in its open mode on the 4. Clash tab. It presses RunOpenButton only when the button is enabled and OpenDocumentLine names only paths under the loop folder. A disabled button is the expected refusal for the NWD: the driver writes the reason and exits 10, and the verdict is TOOL REFUSED.
- CLOSEOWN -RunFolder <runs\NN\item...>. Closes the loop's own Navisworks after run.ps1 died. It acts only when all of these hold:
- that folder's record.txt has no VERDICT line
- its mypid.txt reads adopted
- the pid named there is a process named Roamer, with ExecutablePath the install's Roamer.exe, a command line in the automation form, and start ticks equal to mypid.txt's.
Then it opens a handle, reads the ticks again, and calls Kill, waiting up to 30 s. Once the pid is gone it handles the tool's log as Run does. It lists the settings compare and writes nothing, because no whole watchdog record exists. Last it writes VERDICT CLOSED BY CLOSEOWN. Any mismatch refuses, and nothing is closed.
- NOT BUILT:
- F101's entry
- any ribbon click
- a direct Roamer.exe start with its own switches, which needs an ownership rule in loop.md first
- -Tolerance, -Pairs, -Priority and -Penetrations passed by run.ps1
- a whole run set in one call
- retries, and a scheduler
- the masking, which is F102, and the separate read of the document, which is F104.
- EXIT CODES:
- 0: finished, and everything put back
- 1: a fault in run.ps1
- 2: refused
- 3: not adopted, or the constructor deadline
- 4: hung, or the ceiling
- 5: finished, but a dialog appeared
- 6: something of Bader's not put back
- 7: the window closed by itself or never appeared, the D4 fork
- 8: the driver failed or refused before Run was pressed.
When more than one applies, the first in the order 3, 6, 4, 7, 8, 5.

## start and ownership

ONE OPENER IS BUILT: the Automation start F100 measured, reused from nw-guard.ps1. Once refusals 1 to 18 pass, the steps run in this order.

1. On the main STA thread, $sync.CallStartUtc is set and [Activator]::CreateInstance runs on NavisworksApplication, loaded from the fixed install folder. The watchdog has been running since before the backups. Its constructor deadline is a constant 300 s. Past it, the deadline ends run.ps1's own process through TerminateProcess, which is safe only because refusal 1 proved run.ps1 is its own process.

2. AdoptStart, the probe's inline block wrapped as a function with its body unchanged. It calls NewRoamers and RoamerRecord, then checks the four conditions of loop.md:
- the constructor returned without throwing
- exactly one possible start is new
- it started at or after the call
- its command line holds -Embedding.
If they do not all hold, the start is NOT ADOPTED:
- nothing is called on the object
- SuppressFinalize runs when there is an object
- at the end, UnprovedWrite writes every possible start still running into NwcFederatorLoop\probes\unproved-starts.txt, and says a start was written down only after the write worked
- a new Roamer whose command line names neither embedding nor automation was started by hand. It is left alone, never written down, and rules out the put back.
Exit 3.

3. The held handle, new inside AdoptStart. Get-Process -Id on the adopted pid, then read .Handle, so the Process object holds an open handle. Then read StartTime again and keep the adoption only when the ticks still equal.
- While that handle is open, Windows gives the pid to no other process.
- $sync.MyProc, MyPid and MyTicks are set, and runs\NN\item<K>\mypid.txt is written with the pid, the ticks and the word adopted.
- If the handle cannot be opened, or the ticks differ, the adopted process counts as gone. Nothing is closed, and that is a finding.

4. CloseAdopted is the one close. It takes $sync.MyProc and calls Kill on it after the ticks read equal. .NET Framework's Process.Kill reuses the handle the object holds and throws if that process has exited, which H4 proves on the stand-in. The probe's three Stop-Process -Id calls become calls to CloseAdopted in the same commit, and Stop-Process -Id appears nowhere after.

5. $app.Visible = $true. 5z-d measured that this works.

6. Item 5 only: $app.OpenFile(copy, [string[]]@()). Its seconds, and any throw, are recorded.

7. Window runs only: the driver child starts, before the plugin call, because that call may block the main thread. The command is powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\drive-window-run.ps1, with -OwnerPid, -OwnerStartTicks, -Notes and the item's folders. It is started with -PassThru, and its Process handle is held so only that process can ever be stopped.

8. Window runs only: $app.ExecuteAddInPlugin("ParsonsNwcFederator.PARS", [string[]]@()) on the main thread, the thread that made the object. The id is PluginName.DeveloperCode off FederatorPlugin.cs. AddPluginAssembly is never called, because the baseline must run the installed build, and the SESSION line proves which build ran. Then the main thread waits on $sync.RunOver, whether the call blocked or returned early.

THE DRIVER, changed first in F103:
- -OwnerPid, -OwnerStartTicks and -Notes are mandatory. -Notes and every folder must be under %LOCALAPPDATA%\NwcFederatorLoop.
- It dot-sources nw-guard.ps1 for WinHandlesOf and the emitted type. Add-Type with source, FindWindow, GetTopWindow, SetCursorPos, mouse_event and -Tolerance are removed.
- Before every read, ProcState of the owner must read the same process. Windows are taken only from WinHandlesOf of that pid, then passed to AutomationElement.FromHandle, and Current.ProcessId must equal the pid.
- Every box is set and read back exactly, and IncludeSubfolders must read on.
- After Scan, it waits for FilesGrid's RowCount to be above 0 and unchanged over two reads 2 s apart, for up to 120 s.
- The confirm is a visible #32770 of that pid titled exactly Parsons NWC Federator. OK is pressed only when its text names no rooted path outside the loop folder. Otherwise Cancel is pressed and the driver exits 2.
- Exit codes: 0 when Run and OK were pressed or the open run started, 10 when the open run button reads disabled, 1 on any miss before Run is pressed.

WHAT NEVER HAPPENS:
- GetRunningInstance and TryGetRunningInstance appear nowhere, which H0 checks.
- The mutex Local\NwcFederatorLoop.run keeps a second run.ps1, an Install or a CloseOwn out.
- A direct Roamer.exe start is not built, because loop.md's ownership rule names the Automation start only.

## refusals

- RUN MODE. The checks run in the order below. Up to check 11, every refusal prints REFUSED: and its reason, writes nothing, starts nothing, and exits 2. From check 12 on, writes have begun, so the word is STOP, and every line also goes to record.txt.
- 1. The host. Refused unless this is Windows PowerShell 5.1, a 64 bit process, in STA, and E6 passes. E6 is the probe's check, wrapped as OwnProcessRefusal, that run.ps1 is the script its own powershell.exe was started to run with -File. Prints: REFUSED: run.ps1 runs only in Windows PowerShell 5.1, 64 bit and STA, as the script its own powershell.exe was started to run: powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\run.ps1 followed by its parameters. Its deadline ends its own process, so it never runs inside another. Nothing was started and nothing was written.
- 2. The parameters. Refused when any of these holds:
- -Mode is not Check, Install, Run or CloseOwn
- -Set is not two digits
- -Item is not 0 to 5
- -Stamp is not 8 hex characters
- -Xml is missing for item 1, given for another item, or not a file under %LOCALAPPDATA%\NwcFederatorLoop\source
- -OpenFile is missing for item 5, given for another item, or not a plain file name ending .nwf or .nwd
- -OpenWindow comes with an item other than 0
- -LogChoice is not one of the values that exist. Lock exists from the start. LockAndRemove exists only once Bader has said yes and loop.md says so, in the same pull request
- any path does not resolve under %LOCALAPPDATA%\NwcFederatorLoop, or passes a junction or a link on the way. Each folder from the loop root down is checked for a reparse point, and the loop root itself too.
This is an allow list, so run.ps1 never builds a path it must not touch. It never dot-sources prepare-copy.ps1, because that script's body runs on load and builds the NM Fed path. Prints: REFUSED: -<name> is <value>, <why>. Nothing was started and nothing was written.
- 3. The lock. Prints: REFUSED: another run.ps1 holds the loop's lock, Local\NwcFederatorLoop.run. Nothing was started and nothing was written.
- 4. A run that died. Any runs\*\*\record.txt with no VERDICT line. Prints: REFUSED: <run folder> holds a record with no VERDICT line, so an earlier run.ps1 ended before it finished and what it left is unknown. Read it, run CloseOwn if its Navisworks still runs, and append the VERDICT line the lead writes. Nothing was started and nothing was written.
- 5. Unproved starts. unproved-starts.txt is read by UnprovedRefusal, the probe's step 2 block wrapped with its body unchanged, including attempt 4's rule for a line with no start time. Prints: REFUSED: a start the loop could not prove is still running, pid <N>, start ticks <T>. A person has to look. Nothing was started and nothing was written. A line it cannot read, or a file it cannot read, refuses with that line named.
- 6. Any Navisworks. RoamerRefusal, unchanged: any process named Roamer, whatever its command line and whoever started it, and a process list that cannot be read. Prints: REFUSED: Navisworks is running, Roamer pid <N>, started <time>. Nothing is started, installed or put back while any Navisworks runs, whoever started it. Close it and run again. Nothing was written. The same read runs again at check 18, before each step of an install, and before each write of a put back.
- 7. The installed stamp. FileVersionInfo.ProductVersion of the installed Federator.Addin.dll, or the M2 fallback read off a copy, must name -Stamp with no +edits. Prints: REFUSED: the installed add-in reads <ProductVersion>, and this run is for <Stamp>. Install it with -Mode Install first. Nothing was written.
- 8. The copy. Every file under source is hashed against source.manifest.txt, and source.removed.txt is checked against what the item needs. Prints: REFUSED: the copy is not the one item <K> needs, <first reason>. Run tools\loop\prepare-copy.ps1 as its README says. Nothing was written. The first reason is one of: <file> differs from source.manifest.txt, source.removed.txt is there, source.removed.txt is not there, or the removed NWC's building held one discipline.
- 9. The outputs. Prints one of these:
- REFUSED: item 1 needs empty output folders, and runs\NN\out holds <n> files. Use a new -Set, nothing is ever emptied. Nothing was written.
- REFUSED: item <K> needs the NWFs item 1 wrote, and runs\NN\out\NWF holds none, or item 1's record does not read VERDICT RAN. Nothing was written.
- REFUSED: -OpenFile <name> is not in runs\NN\out\NWF or out\NWD. Nothing was written.
- 10. The window's log. Applies to items 1 to 5, and to item 0 with -OpenWindow, when -LogChoice is missing. Prints: REFUSED: opening the window on main writes its log into the tool's own logs folder, which needs Bader's answer on the log choice. Pass -LogChoice with what he chose. Nothing was written.
- 11. The evidence. Prints: REFUSED: steps\runs\NN\item<K> already holds files, and evidence is never written over. Nothing was written.
- 12. The run folder. runs\NN\item<K> is created fresh. A folder already there is renamed item<K>-aside-yyyyMMdd-HHmmss. Prints: REFUSED: the run folder from an earlier call could not be moved aside, <error>. Nothing was touched. From here every line also goes to record.txt, flushed as it is written. The watchdog starts after this check and before check 13, so every Roamer from the backups on is seen.
- 13. His logs. Every file of %LOCALAPPDATA%\ParsonsNwcFederator\logs is listed with name, size, write time and sha256 into logs-before.txt. Any file not already in logs-backup is copied into logs-backup\since-yyyyMMdd-HHmmss and read back. Prints: STOP before the start: <file> of the tool's logs folder could not be copied into logs-backup and read back with its sha256. Nothing of his was changed, and the partial copy stays in the loop folder.
- 14. His AutoSave. Every file not already in autosave-backup is copied there and read back, and the listing goes into autosave-before.txt. Prints: STOP before the start: <file> of the AutoSave folder could not be copied into autosave-backup and read back with its sha256. Nothing of his was changed.
- 15. The settings backup, BackupSettings, the probe's block wrapped. Prints: STOP before the start: the settings backup is not whole, <the probe's own reason, word for word>. Nothing of his was changed, and the backup stays in the run folder.
- 16. The log lock, window runs only. Every run-*.log of his is opened for reading with FileShare ReadWrite and no Delete. Prints: STOP before the start: <file> of the tool's logs folder could not be held open, so the tool's own cleanup could delete it. Every handle already taken was let go. Nothing was started.
- 17. Keep awake. Prints: STOP before the start: Windows did not take the request to stay awake, SetThreadExecutionState returned 0. Nothing was started.
- 18. The last read, as the last statements before the try that calls the constructor: checks 5 and 6 again. The texts are those of 5 and 6, with STOP before the constructor in front and the line: the backups stay in the run folder. A stop here runs no close and no put back. The outermost finally still lets go of the keep-awake request, the lock handles, the watchdog and the mutex.
- AFTER THE START THESE ARE STOPS, NOT REFUSALS:
- NOT ADOPTED prints the four conditions and exits 3.
- The constructor deadline prints the probe's block and exits 3 through TerminateProcess on run.ps1's own process.
- DRIVER REFUSED or DRIVER FAILED prints the box and what it read back, or the path in the confirm, and says that Run was not pressed. Then the tool window is closed, Dispose runs, and it exits 8.
- WINDOW CLOSED BY ITSELF, or WINDOW NEVER APPEARED, prints the seconds since the call and the log's last lines, and exits 7.
- INSTALL MODE. First checks 1, 2, 3 and 6. Then:
- REFUSED: HEAD is <x>, not -Stamp <y>. Nothing was installed.
- REFUSED: git status --porcelain prints <n> lines, untracked files included, so the build stamp would read <hash>+edits and would not prove main. Install from a clean checkout of main. Nothing was installed.
- STOP: the installed bundle matches neither bundle-backup nor the last loop install, and its copy into bundle-backup-<time> did not read back. Nothing was installed.
- STOP: build\install.ps1 exited <code>. Its output is in installs\<...>\install.txt.
- STOP after the install: the installed add-in reads <x>, not <stamp>.
- CLOSEOWN MODE. First checks 1 to 3. Then:
- REFUSED: <folder> has a VERDICT line, so its run finished. Nothing was closed.
- REFUSED: <folder> has no mypid.txt reading adopted, so nothing there was proved the loop's own. Nothing was closed. A person has to look.
- REFUSED: pid <N> is not the Navisworks this folder adopted, <the first of name, path, command line or start ticks that does not match>. Nothing was closed.
- CHECK MODE runs check 1, then reports checks 4 to 11 without refusing. It exits 2 when any of them would refuse.

## settings backup and put back

These are reused from nw-guard.ps1 by name:
- BackupSettings, the probe's inline block wrapped, taking the run folder
- RegRead, RegWalk and DiffRegistry
- SettingsRead, DiffFiles and DiffAutoSave
- PutBackRegistry with ValueIn, which is F3's rule: the key is opened again and the value read again, and a key that went is never made again
- RegVerify, PutBackFiles and UnprovedWrite
- PutBackReasons, the probe's list of reasons not to put back, wrapped, with F1's DeadlineDone reason among them.

BEFORE EVERY START everything goes into runs\NN\item<K>\settings, which is never emptied. The watchdog is already running.
1. reg.exe export of HKCU\Software\Autodesk\Navisworks Manage\22.0. It must exit 0 and leave a file that is not empty.
2. RegRead, key by key. Every key must read.
3. SettingsRead of %APPDATA%\Autodesk\Navisworks Manage 2025. Every file but AutoSave is copied to appdata-before and read back with its source's sha256. A file whose content cannot be read is marked NOT BACKED UP and is never written.
4. New: AutoSave. Every file of the AutoSave folder that is not already in NwcFederatorLoop\autosave-backup, by name and sha256, is copied there and read back, and the listing goes into autosave-before.txt. The first time that is 196 files and 286 MB. After that only changed files are copied. This keeps the guard that anything of his a step could change is copied first. Neither design, nor the probe's rule of listing AutoSave and never copying it, kept that guard.
5. His logs folder. It is listed into logs-before.txt, and any new file is copied into logs-backup\since-yyyyMMdd-HHmmss. folders.txt is listed like the rest.

PUT BACK follows the D2 rule unchanged, and only after the adopted pid reads gone through ProcState. PutBackReasons must find nothing, which needs a whole watchdog record:
- passes above zero
- no error line, no runspace error and no line it could not write
- no pass that failed before its process loop
- the deadline path never ran
- no Roamer at the start, none new at any pass but the adopted one, and none running now.

Each write works like this:
- Before each SetValue, DeleteValue, CreateSubKey, DeleteSubKeyTree or file copy, the Roamers are listed again. Any Roamer stops that write and every one after it.
- A value or file is written only if it still reads what the compare read.
- RegVerify reads every write again.

When the record is not whole, nothing is written. Every change goes into record.txt and settings.txt with its old and new value, the backup stays for Bader, and the exit is 6.

NEVER WRITTEN BACK AFTER A RUN:
- folders.txt and his logs. Any change to them is a finding. The loop.md rule puts remembered choices back after the loop, from logs-backup, not after each run. The driver uses no picker, and the window writes folders.txt only from a picker, FederatorWindow.xaml.cs lines 130 to 2129.
- AutoSave. It is compared with DiffAutoSave against autosave-before.txt, and every file added, changed or gone is listed. Nothing is written there, needs_bader 6.

CloseOwn lists the compare and writes nothing, because no whole record exists after run.ps1 died.

## keep awake

Three lines are added to the emitted type in nw-guard.ps1: kernel32 SetThreadExecutionState, taking and returning a uint, GetCurrentThreadId, and user32 PostMessageW for the close. Nothing is compiled and nothing is written to %TEMP%.

ON. The request is made on the main thread after the log lock and just before the last read before the constructor, check 17. It asks for three flags together: ES_CONTINUOUS 0x80000000, ES_SYSTEM_REQUIRED 0x00000001 and ES_DISPLAY_REQUIRED 0x00000002. The value it returns and the native thread id go into record.txt. A return of 0 is STOP 17.

HOLDING. The request belongs to the thread and lasts while that thread lives. So it holds while the main thread is blocked in ExecuteAddInPlugin, and Windows drops it by itself if run.ps1's process ends in any way, TerminateProcess included. Nothing persists on the machine.

OFF. The request is let go in the outermost finally, on the same thread, with ES_CONTINUOUS alone. That call must return 0x80000003, and the thread id must equal the one at ON. Both returns and both ids go into every record, and that is the proof. A different return or a different id is a finding that the request may have lapsed. M3 measures this first with no Navisworks.

NOT USED: powercfg /requests, which needs an administrator, and CallNtPowerInformation.

The display flag keeps his screen on for the whole run, which can be hours for the whole folder. That is needs_bader 5. The request does not beat a lock the machine's policy forces, which M6 reads once a minute.

## watching the run

Three readers watch a run, and a heartbeat reports on them.

1. THE WATCHDOG runspace, reused from the probe and changed only where it closes, from before the backups. It runs about every half second plus the time a pass takes. It records:
- every new process whose name starts with Roamer, Adsk, Autodesk, Navis, Lc, Genuine, AdSSO, WerFault, FNPLicensing or LMU
- the Roamer set the put back reads
- after adoption, the adopted pid's visible top-level windows, into watch.txt.
It also holds the constructor deadline, 300 s, and the ceiling, the probe's adopted deadline set to a constant 12 hours. Its ceiling close is CloseAdopted.

2. THE MONITOR, one new runspace started after adoption, with a pass every 15 s. It never touches the COM object. Each pass it:
- finds the tool's log: the one run-*.log in his logs folder that is not in logs-before.txt, was created at or after the call, and whose first line holds Log opened at with that path. Its name goes into toollog-name.txt. None, or more than one, is a finding, and then nothing is ever removed from his folder.
- opens the log for reading with FileShare ReadWrite and Delete, reads new lines from the last offset, and takes the length off the open stream, never FileInfo.Length.
- echoes landmark lines into record.txt with their times: RETAIN, the SESSION plugin version and open document, RUN SETTINGS, each GROUP, RESULT, COPY and Window closed.
- reads TotalProcessorTime of $sync.MyProc after Refresh.
- reads the adopted pid's windows through WindowLines.
- reads the driver's exit through its held handle.
- reads the session lock once a minute.
It applies the hang rule and decides when the run ends.

3. THE DRIVER's notes, driver.txt, read as they grow.

HEARTBEAT. Every 60 s one line goes into record.txt and to the console, holding:
- the time and the seconds since the call
- the log's size and its growth
- the last log line, cut to 120 characters
- the processor seconds and their growth
- the seconds each clock has stood still, against 300
- the last watch.txt line.

THE RUN ENDS at the first of these:
- RESULT, then a COPY line, then 15 s with no new line. This is items 1 to 4 and 5 with the NWF.
- The driver's exit 10, with the refusal read. This is 5 with the NWD.
- The fixed hold reached, for item 0.
- Window closed. in the log before the driver pressed Run, or no tool window within 180 s, exit 7.
- The driver's exit 1 or 2 before Run was pressed, exit 8.
- A hang.
- The ceiling.
- The adopted process gone.

AFTER RESULT the monitor checks the log:
- the SESSION plugin version names the installed stamp
- the RUN SETTINGS source, NWF and NWD folders are under the loop folder
- every file in RESULT's written list is under the loop folder
- for item 1, the GROUPS block reads no group unticked.

The lead starts run.ps1 with run_in_background, follows record.txt, and never touches the loop's Navisworks. If that background task is stopped, run.ps1 dies, check 4 stops every later run, and CloseOwn is how its Navisworks is closed.

## hang rule

The monitor takes two samples every 15 s:
- L, the length of the tool's log, read off its open share-aware stream
- C, the TotalProcessorTime ticks of the adopted process, read through the held Process after Refresh.

A sample where L or C cannot be read is UNKNOWN. It is written down and restarts both clocks, so an unknown never counts toward a hang. Each clock is the time of the last sample where its number changed. Both clocks start when the tool's log is first found. Before that, the constructor deadline and the 180 s window wait govern.

HangVerdict(lastLChangeUtc, lastCChangeUtc, nowUtc, limitSeconds) is one pure function. It returns true when both numbers have stood still for limitSeconds at the same moment. run.ps1 always calls it with the constant 300, the rule's five minutes. Only the harness calls it with another number.

WHEN IT FIRES:
1. HANG goes into record.txt and the console, with the first flat time, L and C.
2. The last 200 lines of the log, read share-aware, go into hang-tail.txt. The adopted pid's visible windows go with them, with any dialog text, because a dialog is the likeliest cause.
3. CloseAdopted: Kill on the held handle, after the ticks read equal. Dispose is not called first, because a COM call into a hung server can block.
4. The main thread's blocked ExecuteAddInPlugin returns with an error. SuppressFinalize runs, then the ordinary finally. Exit 4, a finding.

Only the adopted pid is ever closed this way. Never the driver, and never any other process.

A run whose processor time keeps moving is never hung under this rule, however long it takes. Only the ceiling ends it. The ceiling is 12 hours from adoption, recorded as CEILING and never as HUNG, and it is needs_bader 4.

Whether an idle Navisworks, or one waiting at a dialog, is ever exactly flat for five minutes is UNKNOWN. M4 and M10 measure it. If it never is, the rule as written can never fire, needs_bader 3.

## dialogs

BEFORE ADOPTION nothing is sent to any process. This is the probe's rule: only class and caption are read, through GetClassName and GetWindowText, and only for Roamers that pass the start-time and -Embedding tests.

AFTER ADOPTION:
- Windows of the adopted pid alone are read, found with EnumWindows and GetWindowThreadProcessId, after ProcState reads same.
- WindowLines changes once, in the second commit: with messages allowed, it sends WM_GETTEXT with a 500 ms timeout to the visible children of every visible top-level window, not only of a #32770. Both the watchdog and the monitor get the text that way.
- A window of any other process is never read, and a message never goes to one.

CLASSIFYING WINDOWS. Each pass, the monitor sorts every new visible top-level window of the adopted pid:
- The tool window, class HwndWrapper with a caption starting Parsons NWC Federator, is WINDOW.
- The Navisworks main window, a WindowsForms10 class with a caption ending Autodesk Navisworks Manage 2025, is MAIN.
- Working..., class HwndWrapper[Roamer.exe;ProgressDialog;...], is PROGRESS. 5z-d measured both of these.
- The confirm the driver read and answered is a DRIVER STEP, and driver.txt keeps its text.
- Every other window is a DIALOG finding: its time, class, caption, owner, whether its owner is disabled, and its full text. A dialog whose children answer nothing is written with text UNKNOWN, no child window answered.
A UI Automation read of the text is built only if M14 shows a Navisworks dialog like that.

WHAT GETS PRESSED. run.ps1 presses nothing on any dialog. Only the driver presses, and only two things:
- OK on the confirm, a #32770 of the pid titled exactly Parsons NWC Federator, and only when its text names no rooted path outside the loop folder. Otherwise it presses Cancel and exits 2.
- RunOpenButton in item 5, only when enabled.
For the NWD the button is disabled and no dialog appears, so the evidence of 5b is the OpenDocumentLine text the driver reads.

WHAT AN UNPRESSED DIALOG MEANS. Any other dialog is left up and recorded, and the hang rule or the ceiling decides. A run that throws shows the tool's Warn before it writes RESULT (FederatorWindow.xaml.cs lines 2061 to 2071). So that run's log goes quiet and loses its RESULT block, and the Failure lines above the Warn are the evidence. A DIALOG after the driver pressed Run makes the exit 5 when nothing worse happened.

## close

DISPOSE FIRST. A forced close only of the adopted pid, through the held handle.

THE NORMAL END:
1. The monitor ends the run. It finds the tool window with WinHandlesOf after ProcState reads same, and posts WM_CLOSE to it through PostMessageW. Then it waits up to 60 s for Window closed. in the log. If that line does not appear, that is a finding and the close carries on.
2. ExecuteAddInPlugin returns on the main thread, if it blocked. Its int and its seconds are recorded, and nothing is judged by that int.
3. The driver, if it still runs, is ended through its own held handle.
4. Dispose, on the main thread. Then a wait of up to 60 s while ProcState reads same. 5z-d measured 8.0 to 9.3 s.
5. If the process still reads same, CloseAdopted runs, with 30 s more. That is FORCED, a finding.

A hang or the ceiling skips steps 1 to 4 and goes straight to CloseAdopted.

THE FINALLY closes first and writes after, as in the probe, so a failed write can never leave the adopted process running:
6. If an adopted process still reads same, CloseAdopted runs. SuppressFinalize runs when Dispose did not complete.
7. The watchdog and the monitor stop, and PutBackReasons reads their record.
8. UnprovedWrite writes down every possible start still running. The adopted Roamer is written too if it reads same or unreadable after every close path.
9. Only once the adopted pid reads gone and RoamerRefusal finds none: the D2 compare and put back, and the AutoSave compare.
10. His logs:
- the tool's log and tsv are copied into toollog and checked by sha256
- under choice A, exactly those two are removed from his folder
- the lock handles are let go
- logs-after.txt is taken and compared with logs-before.txt.
11. The source copy is hashed against the manifest again, which proves Navisworks wrote nothing into it. Then outputs.txt is written.
12. The keep-awake request is let go, with its return recorded, in the outermost finally.
13. VERDICT is written as the last line of record.txt.
14. The evidence is copied into steps\runs, and the mutex is released.

WHAT IS NEVER CLOSED. A Navisworks the loop did not adopt is never closed at any step. A start that was not adopted goes to unproved-starts.txt and is left running. Steps 9 and 10 never run while any Roamer runs.

THE WINDOW CLOSES BY ITSELF. If this happens before the driver pressed Run, the verdict is WINDOW CLOSED BY ITSELF, exit 7, then Dispose and the rest as above. That is D4's fork.

IF RUN.PS1 DIES MID RUN, its Navisworks keeps running, check 4 and check 6 stop every later run, and CloseOwn closes it once mypid.txt matches.

## log folder problem

WHAT IS MEASURED. Read again today, reading only: %LOCALAPPDATA%\ParsonsNwcFederator\logs holds exactly 30 run-*.log files, folders.txt, a Log subfolder and no .tsv. Its oldest log by write time is run-20260901-191711.log.

WHAT THE CODE DOES:
- RunLog.DefaultLogFolder reads Environment.GetFolderPath(LocalApplicationData), RunLog.cs lines 222 to 226.
- FederatorPlugin.Execute calls RunLog.StartOrDisabled() with no folder and ignores its parameters.
- KeepLogs is never set anywhere in src, so it stays 30.
- Start opens the new log with FileShare ReadWrite, then opens the tsv beside it, then prunes, RunLog.cs lines 313 to 360.
- PruneOldLogs lists only the run-*.log files at the top of the folder and sorts them newest first by LastWriteTimeUtc. It keeps 29 others besides the live file and deletes the rest one by one, each in its own try. A delete that throws writes RETAIN could not delete, lines 368 to 434.
- Nothing prunes the .tsv.
So on main every window open deletes his oldest log. The loop's .log and .tsv are also left in his folder, and that .log pushes one more of his out at his own next run.

NO TRICK MOVES THE LOG. GetFolderPath reads the known folder, not the LOCALAPPDATA variable. The loop's Roamer is started by COM under svchost pid 1328 (automation-start-result-20260928.txt line 397), so run.ps1's environment never reaches it. StartOrDisabled falls back to the temp folder only when Start throws, and making it throw would mean changing his folder.

THE LEAST BAD PATH, A, RECOMMENDED:
1. Before the start, with no Navisworks running: list his folder by name, size, write time and sha256 into logs-before.txt. Copy anything not already in logs-backup into logs-backup\since-yyyyMMdd-HHmmss.
2. From just before the constructor until the adopted Navisworks reads gone, hold every run-*.log of his open for reading, with FileShare ReadWrite and no Delete. Windows refuses to delete a file that is open without delete sharing, and PruneOldLogs catches that. So RETAIN should read deleted 0, could not delete 1, which is UNKNOWN until M1. This writes nothing to his files. If run.ps1 dies, Windows closes the handles and nothing is left set.
3. The monitor reads the RETAIN line as soon as it appears. A deleted count above 0 is LOG LOST, naming the file, whose copy is in logs-backup.
4. After the Navisworks is gone, find the one new run-*.log whose first line names its own path, and the .tsv with the same stem. Copy both into the run folder's toollog, check the sha256, and remove exactly those two.
5. logs-after.txt must equal logs-before.txt, name for name, size, write time and sha256. Any difference is a finding.
His folder then ends as it began, and none of his logs is pushed out, now or at his next run.

WHAT NEEDS BADER, PLAINLY: every option below does. On main the window can only log into his folder, so no path keeps every written guard and D4 together.
- A breaks two sentences of loop.md. Loop runs write their logs inside the work folder, broken for the length of each run. The only folder the loop deletes from is NwcFederatorLoop, broken for exactly the two files each loop run creates.
- B is the lock with no removal. His folder keeps the loop's logs, and his own next run pushes out one of his per loop log, each with a copy in logs-backup.
- E: he moves his logs aside himself before the loop, and moves them back and removes the loop's logs himself after. This keeps the delete guard to the letter and costs him two sittings.
- C is one src change so Execute takes a log folder. That breaks D4, and the baseline is then not main.
- D is F101 first, whose settings file names the log folder. That reverses D4.

Once he answers, the lead writes his answer into loop.md's guard text in the pull request that first uses it.

UNTIL HE ANSWERS, no window opens on main, not even for the measure start. The harness, Check, the start with no window and Install go ahead, because none of them opens the window.

## evidence collection

Evidence is collected after the close, the put back and the log handling, never before.

It goes into steps\runs\NN\item<K>, or item5-nwf, item5-nwd, item0 or item0-window. run.ps1 creates the folder and refuses one that already holds files (check 11). It holds:
- record.txt, run.ps1's own record. Its first line is RUN RECORD, with run.ps1's sha256, the mode, the set and the item. Then every check, the adoption, the heartbeats, and every finding: HUNG, CEILING, DIALOG, WINDOW CLOSED BY ITSELF, WINDOW NEVER APPEARED, DRIVER FAILED, DRIVER REFUSED, FORCED, LOG LOST, NOT PUT BACK. Also both keep-awake returns and thread ids, every program started with its pid and exit, and every file written outside the repo. Its last line is VERDICT: RAN, TOOL REFUSED, NOT RUN, STOPPED, NOT ADOPTED or CLOSED BY CLOSEOWN. It holds no machine name, and every path goes through the probe's Mask.
- The tool's run-*.log and run-*.tsv, taken from the run folder's toollog copy.
- driver.txt and watch.txt.
- outputs.txt: every file under runs\NN\out, and for item 5 the open folder, with its relative path, size, sha256, and whether it was written after the call.
- settings.txt: the compare lines for the key, the settings folder and AutoSave, through Mask.
- hang-tail.txt, after a hang.
- workbooks\<name>.txt: one read-out for each xlsx that outputs.txt lists. Each is made by a child powershell -NoProfile -ExecutionPolicy Bypass -File tools\loop\read-workbook.ps1 -Workbook <xlsx> -Out <that path>, with its exit code recorded. A READ-OUT FAILED line is kept as it is.

RAN is written only when the log on disk holds a RESULT block and its SESSION plugin version names the installed stamp. For item 1, the GROUPS block must also read no group unticked. Anything run.ps1 could not read is written UNKNOWN.

NEVER COPIED: NWC, NWF, NWD, xlsx, pictures, AutoSave files, and anything from logs-backup or autosave-backup. Every file's size is printed, and any file over 20 MB is named for the lead to zip.

F102 masks every file before a commit. The tool's log carries his folders.txt in its FOLDERS REMEMBERED block (FederatorWindow.xaml.cs line 95, FolderMemory.Lines). That block names NM Fed and any other folder he picked, in full, so F102 must cover it. Until it does, no loop log is committed.

## what it writes outside the repo

- UNDER %LOCALAPPDATA%\NwcFederatorLoop, WRITTEN BY RUN.PS1:
- runs\NN\item<K>\, holding record.txt, watch.txt, driver.txt, mypid.txt, toollog-name.txt, logs-before.txt, logs-after.txt, autosave-before.txt, settings\ with the .reg export and appdata-before\, toollog\ with the tool's log and tsv, outputs.txt and hang-tail.txt. For item 5 it also holds open\ with the copy that was opened. A folder already there is renamed item<K>-aside-yyyyMMdd-HHmmss and never emptied.
- runs\NN\out\NWF, NWD and Excel. run.ps1 creates them empty for item 1, and the tool fills them, pictures and the partial log copy TryCopyTo writes included. Item 5's NWD and Clash Reports land beside the copy in open\.
- logs-backup\since-yyyyMMdd-HHmmss, only for a file of his logs folder not already backed up.
- autosave-backup\, new, filled incrementally by name and sha256.
- probes\unproved-starts.txt, appended only for a start that could not be proved.
- installs\<stamp>-yyyyMMdd-HHmmss\ with install.txt and installed.txt, and bundle-backup-yyyyMMdd-HHmmss only for an installed bundle the loop has no backup of. These are Install mode only.
- test\, the harness only.
- IN BADER'S %LOCALAPPDATA%\ParsonsNwcFederator\logs:
- The tool itself, not run.ps1, writes run-*.log and run-*.tsv at every window open on main.
- Under choice A, run.ps1 removes exactly those two, after copying them out and checking their sha256.
- run.ps1 holds his run-*.log files open for reading, with no delete sharing, from just before the start until the adopted Navisworks reads gone.
- folders.txt is only read.
- HKCU\Software\Autodesk\Navisworks Manage\22.0, and every file of %APPDATA%\Autodesk\Navisworks Manage 2025 but AutoSave. The loop's Navisworks writes them. run.ps1 puts them back only under the D2 rule, through SetValue, DeleteValue, CreateSubKey, DeleteSubKeyTree and a copy of a backed up file.
- %APPDATA%\Autodesk\Navisworks Manage 2025\AutoSave. The loop's Navisworks writes there during a run, as his own run wrote one at 17:00:28 on 2026-09-27. run.ps1 copies the folder first and never writes there, needs_bader 6.
- %APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle. Replaced by build\install.ps1 in Install mode only, after the backup rule.
- HKCU\Software\NwcFederatorLoopTest. The harness only, created and then deleted. It is the one registry delete the loop makes.
- WRITTEN BY NAVISWORKS DURING A START, UNKNOWN UNTIL M5: files under %TEMP%, %LOCALAPPDATA%\Autodesk and %PROGRAMDATA%\Autodesk for licensing, Windows recent items, keys under HKCU\Software\Autodesk other than 22.0, and crash files by WerFault if Navisworks crashes. They are listed and masked, never deleted.
- WRITTEN BY WINDOWS POWERSHELL ITSELF, NOT BY THE SCRIPT: possibly files under %LOCALAPPDATA%\Microsoft\Windows\PowerShell, UNKNOWN with -NoProfile, listed by M5. run.ps1 and the driver write nothing under %TEMP%, because neither uses Add-Type with source.
- IN THE REPO: the evidence in steps\runs\NN\item<K>. Also artifacts\, bin\ and obj\ from install.ps1's build and from the stand-in's build, all of which git ignores.
- PROGRAMS STARTED, each recorded with its pid and times:
- reg.exe export, once per run.
- Roamer.exe -Embedding, started through COM with svchost as its parent, and what it starts: two AdskLicensingAgent and an AdskLicensingInstHelper under GenuineService, measured in 5z-d, and WerFault if it crashes.
- powershell.exe -STA for the driver, once per window run.
- powershell.exe for read-workbook.ps1, once per workbook.
- In Install mode: git rev-parse and git status, and powershell.exe for build\install.ps1. That starts dotnet build, its MSBuild nodes, VBCSCompiler, and the git calls in Directory.Build.targets. Then dotnet build-server shutdown runs, so none of them stays.
- In the harness only: dotnet build of the stand-in, the stand-in Roamer.exe and child powershell.exe processes.
Never started: prepare-copy.ps1 and csc.exe.

## proof without navisworks

- THE HARNESS. Everything below runs in tools\loop\prove-run.ps1:
- It refuses to start while any Roamer runs.
- It loads the function definitions from nw-guard.ps1 and run.ps1 through [System.Management.Automation.Language.Parser]::ParseFile, so run.ps1's main flow never runs inside it.
- It builds tools\loop\StandIn, a net48 exe named Roamer.exe. The stand-in is outside the solution and the bundle, references the repo-built Federator.Core.dll, and takes its role from its arguments: sleep, spin, write a RunLog, show a MessageBox, show a WinForms dialog, show a WPF window carrying the real AutomationIds and tab names, and log every message its window receives.
- After every case it checks that nothing of Bader's changed: his logs folder listing with attributes, his AutoSave listing, and a reg export of 22.0, each compared byte for byte, and the loop folder outside test\.
- It closes every stand-in through a held handle at the end.
- H0, STATIC READS. run.ps1, nw-guard.ps1 and the driver must hold none of these: GetRunningInstance, TryGetRunningInstance, FindWindow, GetTopWindow, RootElement, SetCursorPos, mouse_event, Add-Type with source, GetFolderPath('Desktop'), NM Fed, ACCDocs, a dot-source of prepare-copy.ps1. Other rules checked:
- Kill and TerminateProcess appear only in CloseAdopted, CloseOwn and the deadline block. Stop-Process -Id appears nowhere.
- Remove-Item and Delete appear only in the helper that removes the loop's own two logs and in the harness cleanup.
- PostMessageW appears only in the close helper.
Each check is shown to fail on a copy with one bad line added, kept under test\.
- H1, THE MOVE. tools\probes\probe-automation-start.ps1 -ReflectionOnly -Out is run before and after the move into nw-guard.ps1. The two outputs must be equal except for the SCRIPT sha line. Both files must parse with no errors. git diff of the first commit must show only moved lines, the six wrappers and the dot-source line. F100's harness runs again on the probe after the move, and reads the same.
- H2, THE REFUSALS. Refusals 1 to 11 are provoked on the real run.ps1, each once, and each must print its REFUSED line, exit 2 and write nothing. The provocations:
- a 32 bit powershell from SysWOW64, and a powershell that dot-sources run.ps1
- each bad parameter
- a path that goes through a junction made under test\
- the mutex held by a harness process
- a stand-in Roamer running, once started by hand and once with -Embedding
- a wrong -Stamp.
The checks that read fixed paths, 4, 5, 8, 9 and 11, are run as functions handed paths under test\, never the real marker files or unproved-starts.txt.
- H3, ADOPTION. AdoptStart is fed made-up Roamer records, with each of the four conditions made false once. A start by hand is never adopted. Two possible starts are never adopted, and both are written to a test unproved file only after the write works. A start from before the call is never adopted.
- H4, THE HELD HANDLE. A stand-in started with -Embedding is adopted through the handle function, and its ticks are read again. It then exits, and a decoy stand-in starts. CloseAdopted refuses because the held process has exited, and the decoy's message log stays empty. This proves Process.Kill uses the held handle.
- H5, M1, THE LOG LOCK WITH THE REAL PRUNE. test\logs holds 30 dummy run-*.log files, a minute apart, plus a folders.txt. With the lock held, the stand-in calls RunLog.Start(folder, now, 30) and the record must show RETAIN keeping 30 logs, deleted 0, could not delete 1, with all 30 reading back by sha256. The control without the lock shows deleted 1. Then the new .log and .tsv are copied out, matched and removed, and the after listing equals the before listing.
- H6, THE HANG RULE.
- HangVerdict is checked against a table of made-up times.
- The monitor runs with limit 20 against a stand-in that writes its RunLog every 2 s for 30 s and then sleeps. It must call a hang about 20 s after both clocks stop, write hang-tail.txt, and close the stand-in through its held handle.
- Log flat with the processor spinning never hangs. Log growing with the processor flat never hangs. An unreadable sample restarts both clocks.
- A third stand-in the monitor was not given is never touched, and its start ticks read the same before and after.
- This also measures whether a sleeping .NET process shows exactly zero processor time.
- H7, DIALOGS. After adoption, the stand-in raises a MessageBox with a known text and a WinForms dialog with a known label. The record shows both as DIALOG with that exact text. A decoy stand-in started by hand raises a form whose window logs every WM_GETTEXT it receives. The record holds no line about the decoy, and the decoy's log shows no message received.
- H8, THE DRIVER BY PID. The stand-in's WPF window carries the real ids. The driver, with -OwnerPid, must:
- set and read back every box
- read IncludeSubfolders as on
- wait on FilesGrid
- press Run, read the confirm and press OK.
A decoy with the same title in a stand-in started by hand keeps an empty event log. A confirm naming a path outside the loop root gets Cancel and exit 2. A box that will not take its value stops the driver before Run, with exit 1. A -Notes path outside the loop root is refused. The open mode reads a disabled button and exits 10.
- H9, THE END OF A RUN. The stand-in writes RESULT, then COPY, then goes quiet. The monitor posts WM_CLOSE to the window of the pid it was given, and that window closes. A second stand-in's window with the same title stays open.
- H10, SETTINGS. BackupSettings, DiffRegistry, PutBackRegistry, RegVerify, SettingsRead, DiffFiles and PutBackFiles run against HKCU\Software\NwcFederatorLoopTest\22.0 and test\appdata. The stand-in sets, changes and deletes a value, adds and removes a key, and changes and deletes a file. With nothing else running, everything is put back and RegVerify reads 0. With a stand-in Roamer started just before the first write, that write and every one after it stop. The AutoSave and logs backup functions copy only new files, and read them back. The test key is deleted at the end, and Bader's 22.0 export reads byte identical.
- H11, M3, KEEP AWAKE. In a child powershell -File: the request returns a value that is not 0, the release returns 0x80000003, and the thread id is the same at both.
- H12, INSTALL AND THE STAMP. With a stand-in Roamer running, Install refuses, and the bundle folder's sha256 listing is equal before and after. The clean-tree check refuses on a scratch git repository under test\ that holds one untracked file. M2 reads ProductVersion on the repo build. install.ps1 itself never runs in the harness.
- H13, CLOSEOWN. A mypid.txt naming a stand-in with matching ticks and -Embedding closes it, with the Roamer path check handed the stand-in's path. Each of these refuses: mismatched ticks, a missing -Embedding, another name, a record that has a VERDICT.
- H14, THE CONSTRUCTOR DEADLINE. In a child powershell, the watchdog scriptblock runs with $sync set as if a call began 61 s ago with a 60 s deadline. It writes its block, writes the possible start down to a test file, and ends that child with exit code 3.
- H15, EVIDENCE AND CHECK. read-workbook.ps1 runs on a copy of a samples\client-report workbook placed under test\out\Excel, writing into steps\runs\zz-harness, which is removed at the end and never committed. Check mode runs, and the listings and the reg export read the same before and after, which shows it wrote nothing.

## proof with navisworks

- P0, THE ORDER:
1. F102 merged.
2. F100's one run and its squash merge.
3. F103 merged: nw-guard.ps1, run.ps1, the driver change, the stand-in and the harness, with the H0 to H15 output pasted into its body. It is read before merge by the reviewer, the breaker and the claim-checker.
Then Roamer must read 0 before every step below.
- P1, CHECK, with Navisworks closed. Look for: no Roamer, the installed stamp be0b9b37 matching bundle-backup, 30 logs plus folders.txt and Log, logs-backup covering them, and no record without a VERDICT.
- P2, ITEM 0 WITH NO WINDOW, -Set 00 -Stamp be0b9b37, before Bader's answer on the logs. Look for:
- two RoamerRefusal reads of 0
- the backups whole, AutoSave copied
- keep-awake ON not 0, with its thread id
- all four adoption conditions True
- the handle held and the ticks equal (M4)
- Visible True
- 360 s of processor samples every 15 s (M4)
- the lock-state lines (M6)
- Dispose, then the pid gone about 9 s later with no forced close
- the D2 put back with RegVerify 0, or nothing written with the reasons listed
- his logs listing equal before and after, as the probe's run 3 read
- the AutoSave compare
- M5's listings
- OFF returning 0x80000003 with the same thread id
- VERDICT, exit 0.
- P3, INSTALL MAIN, from a clean checkout of main with -Stamp set to main's 8-character commit. Look for:
- HEAD equal to -Stamp and a clean tree
- the installed bundle matching bundle-backup
- install.ps1 exit 0
- the build servers shut down
- installed.txt written
- the installed stamp naming main's commit, with no +edits
- his logs and AutoSave unchanged.
Then Check again.
- P4, AFTER BADER ANSWERS: ITEM 0 -OpenWindow -LogChoice <his answer>. Look for:
- the window found by pid, its title carrying main's stamp
- the SESSION plugin version equal to main's stamp (M8)
- RETAIN could not delete run-20260901-191711.log and deleted 0 (M11)
- the window still open at 240 s with no Window closed. line (M7)
- the processor samples (M10)
- WM_CLOSE, then Window closed., then ExecuteAddInPlugin returning with its seconds and int, then Dispose, then the pid gone (M9)
- the log and tsv copied out and, under A, removed
- his folder listing equal to the before listing
- D2, OFF, VERDICT, exit 0.
- P5, IF P4 EXITS 7, stop there. P4 already tests the one explanation on record, the early Dispose, so a second guess costs a start and a log in his folder for no new answer. The lead writes why in steps\loop.md. That is the D4 fork: F101 comes first, unless Bader chooses to click the add-in button himself in each start the loop adopted, needs_bader 2.
- P6, IF P4 PASSES, THE BASELINE, set 00, in order, each item its own start and close:
1. prepare-copy.ps1 with no switch, which must keep the copy.
2. -Item 1 -Xml with Bader's matrix in the source copy.
3. -Item 2.
4. prepare-copy.ps1 -Remove with one NWC of a building holding two or more disciplines, then -Item 3.
5. -Restore with the same NWC, then -Item 4.
6. -Item 5 with an NWF, then again with an NWD.
Look for, on each run: the VERDICT, the RESULT block, no group unticked on item 1, outputs.txt, the source copy unchanged by its manifest, the settings put back, his logs folder equal before and after, the AutoSave compare (M13), every dialog with its text (M14), and the driver's proof by pid (M12). For 5b look for TOOL REFUSED with OpenDocumentLine's text (M15). The log-reader reads every run.
- P7, THE HANG AND DIALOG RULES are never provoked on purpose on a real Navisworks. Their harness proofs, H6 and H7, stand until a real hang or dialog happens. Item 3 is the likeliest place for one (M14).

## needs bader

- 1. THE LOG FOLDER, before any window opens on main. A, recommended: hold his logs open during each run, then copy out and remove only the loop's own log and tsv. B: the same hold with no removal, and his oldest logs pushed out at his next run, copies kept. E: he moves his logs aside and back himself. C: one src change before the baseline, which breaks D4. D: the no-click entry first, which reverses D4. Every option needs his yes, because on main the window can only log into his folder. The details are in log_folder_problem.
- 2. IF THE WINDOW CANNOT BE DRIVEN WITH NO CLICK, which is P4 exiting 7 or run 1's driver failing on a real miss, D4 says the no-click entry comes first. The one way to keep the baseline through the window is Bader clicking the add-in button once in each Navisworks the loop started and adopted, about six times for the baseline, while run.ps1 waits for the adopted pid's window. Only he can offer that.
- 3. THE HANG RULE. M4 and M10 may show that an idle Navisworks, or one waiting at a dialog, never uses exactly no processor time for five minutes. If so, his rule as written can never fire, and only the ceiling ends a stuck run. He restates what counts as no processor time, for example under one second in five minutes.
- 4. THE CEILING. His rule names none. run.ps1 closes its own Navisworks 12 hours after adoption and records CEILING, never HUNG. The whole folder is 45 buildings and 140 NWC files, and one building took 23 minutes on the old build before that run died, so a shorter ceiling could cut a healthy run. Whether any ceiling may exist, and how long, is his call.
- 5. THE SCREEN. ES_DISPLAY_REQUIRED keeps his screen on for runs that may last hours. Visible True puts the loop's Navisworks and the tool window on his screen, where a click of his changes the run. May runs go while he is away? Does a locked screen matter for the pictures (PQ8, M6)?
- 6. AUTOSAVE. His own run wrote an autosave at 17:00:28 on 2026-09-27, so loop runs will write there too, under the same document names as his. run.ps1 copies the folder first and writes nothing there. May the loop remove the autosaves its own runs added? May it put back, from autosave-backup, any of his that a run changed or removed? Until he says, each one is listed and left.
- 7. HIS REMEMBERED FOLDERS. At every window open, the tool checks that each folder in his folders.txt still exists and writes the list into its log, NM Fed among them. That is the tool on main reading whether NM Fed exists. The loop cannot stop it without writing his folders.txt or changing src. He says whether that is acceptable, and F102 masks the block before any log is committed.

## risks

- IMPOSSIBLE BY CONSTRUCTION while run.ps1 lives: closing or messaging a process it did not adopt.
- Every close goes through the Process object opened after adoption and checked against the adopted ticks, and Windows gives no pid to a new process while a handle to the old one is open.
- Every window read and every WM_CLOSE starts from WinHandlesOf of that pid after ProcState reads same.
- The driver works only under -OwnerPid.
- IMPOSSIBLE BY CONSTRUCTION: a close before adoption. CloseAdopted takes the held Process, which exists only after adoption. The constructor deadline ends only run.ps1's own process, and only after the E6 check.
- IMPOSSIBLE BY CONSTRUCTION: attaching through the API, because GetRunningInstance and TryGetRunningInstance are never called (H0). A real mouse click, because the driver loses mouse_event and -Tolerance. Building the NM Fed path, because run.ps1 takes paths only from an allow list and never dot-sources prepare-copy.ps1.
- ONLY UNLIKELY: a Navisworks started by hand between the last read and the COM activation. Adoption then fails, because there are two possible starts or one hand start, so nothing is called, the finalizer is suppressed, and the watchdog record stops the put back. What StartupNavisworks does with GetActiveObject is UNKNOWN.
- ONLY UNLIKELY: a Navisworks started by hand between Install's last read and install.ps1's delete of the bundle. The Roamers are read again after the install, and one found then is a finding.
- ONLY UNLIKELY: run.ps1 dying mid run, for example when a background task is stopped or it crashes. Its Navisworks keeps running, and the log lock handles and the keep-awake request go with the process. Every later start refuses on the Roamer and on the record with no VERDICT. CloseOwn closes the Navisworks by mypid.txt, with checks of name, path, command line and start ticks, which are strong but no longer a held handle. If run.ps1 died after ExecuteAddInPlugin and before the tool's prune, the prune could delete his oldest log, whose copy is in logs-backup.
- ONLY UNLIKELY, a named limit carried over from the probe: the put back could revert a change made by a Navisworks started by hand that started and exited inside one watchdog gap. The longest gap is printed.
- KNOWN: the window on main writes its log and tsv into his folder, needs_bader 1. Whether the lock stops the prune is UNKNOWN until M1. The RETAIN line is read on every run, and a deleted count above 0 is LOG LOST, with that file's copy in logs-backup.
- KNOWN: the loop's Navisworks writes into his AutoSave folder, under his document names. The folder is copied before every start, and needs_bader 6 covers the rest.
- KNOWN: at every window open the tool checks that his remembered folders exist, NM Fed among them, and writes them into its log. That is needs_bader 7, and F102 must mask it.
- KNOWN: a build from a tree with any untracked file stamps +edits. Install refuses such a tree, and because this clone has an untracked .claude\worktrees, main is installed from a clean checkout.
- KNOWN: the hang rule may never fire (M4, M10). A run whose processor time keeps moving is never hung, and then the 12 hour ceiling is the end, needs_bader 3 and 4. The length of a whole-folder run is UNKNOWN and may be hours.
- KNOWN: a run that throws shows the tool's Warn before it writes RESULT, and nothing presses that Warn. The log goes quiet, the hang rule or the ceiling ends the run, and that run loses its RESULT block. The Failure lines above the Warn are the evidence. This is a register finding.
- KNOWN: 5b's refusal reaches no log line. The disabled button and OpenDocumentLine's text, read by the driver, are its only evidence.
- KNOWN: Visible True and ES_DISPLAY_REQUIRED put the loop's Navisworks on his lit screen, needs_bader 5.
- KNOWN: the keep-awake request belongs to one thread. M3 and the two thread ids in every record show whether it held.
- KNOWN: until M5, the list of what a start writes under %TEMP% and the licensing, crash and recent-items places is incomplete.
- KNOWN: moving the probe's code into nw-guard.ps1 changes the probe after its one run. The first commit is moves only, proved by H1. If F100 goes to B instead, nw-guard.ps1 is written fresh from this design, a larger pull request read from the start.
- KNOWN: nothing is known about what ExecuteAddInPlugin's int means, so a run is judged by its log and never by that int. If the tool falls back to logging in its temp folder, the monitor finds no log and only the ceiling ends the run.
