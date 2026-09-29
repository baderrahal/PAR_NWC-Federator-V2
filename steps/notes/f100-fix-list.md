F100 fix list, from the reviewer and the breaker who read probe-automation-start.ps1, its result and scan.md 5z-d on 2026-09-28. Line numbers are of the committed probe, commit 464f79f on fix-F100. The run itself went cleanly. Every finding is about what the probe would do on a less clean day, or a claim the result file does not carry. tools\loop\run.ps1 will copy this probe to start every loop run, so a flaw here repeats on every run.

A. OWNERSHIP OF THE NAVISWORKS, the most serious. Rule: a Navisworks the loop did not start is never closed, attached to or sent anything.
1. Lines 605 and 616 to 623 adopt the one new Roamer as MY PID before line 623 checks whether the constructor threw. If Bader opens a Navisworks by hand during the 110 s constructor wait and the constructor throws or reaches something else, his Roamer is the one new Roamer, is written as MY PID, and the finally at 737 to 741 force closes it with a calm line.
   Fix: adopt a pid only when the constructor returned without throwing AND exactly one Roamer is new AND its StartTime is at or after the call start AND its command line holds -Embedding, the rule the watchdog already uses at line 559. Anything else: close nothing, call nothing on the object, suppress its finalizer, write why, stop.
2. Line 591 runs the constructor even when a Roamer from step 2 could serve the class. Read each step 2 Roamer's command line through Win32_Process, which touches nothing in that Navisworks, and refuse before the constructor when any holds -Embedding or -Automation. The reflection verdicts at 268, 294 and $readsIt at 366 are printed and never stop the probe. An UNKNOWN there should stop it, as the README says.
3. Lines 541 to 546, 405 and 416: the watchdog reads windows of every new Roamer and sends WM_GETTEXT to the children of a #32770 dialog of any of them, which includes a Navisworks Bader opens by hand. Once MyPid is set read only that id, and before that only the provable candidates of line 559. This contradicts line 503 and scan.md 4664 to 4665.
4. Line 607: with two or more new Roamers the finalizer is left live, so Dispose(false) can reach the Bridge destructor later against an unknown Roamer. Suppress it in every not-exactly-mine case.
5. Line 566 closes an id without checking it is still the same process. Check the StartTime still matches $sync.MyStart before Stop-Process. Line 429, Alive, returns true on a catch, which errs toward closing. Unreadable should mean do not close.
6. Lines 737 to 741: two Say calls run before Stop-Process in the finally. Say appends to $Out in the repo under OneDrive, and a throw there ends the finally with the probe's own Navisworks left running. Close first, write after. Lines 601 to 602: Get-CimInstance with no -ErrorAction and StartTime.ToString can throw before $myPid is set. Make them unable to stop the close.

B. BADER'S SETTINGS. Rule: nothing of Bader's is deleted or overwritten.
7. The probe exported HKCU\Software\Autodesk\Navisworks Manage\22.0 (line 493) but copied none of the 205 files under %APPDATA%\Autodesk\Navisworks Manage 2025, and three of them, clash\rules, CommCenter\en-US\infocenter.xml and LastSession.xml, were rewritten. Before the start, copy every file of that folder except AutoSave into the probe's work folder and export the key. After its own Navisworks is gone, put back the registry values that changed and the files that changed, and print each value and file put back. Print the OLD AND NEW VALUE of every registry value that changed, not the name only, so the result carries what 5z-d's table says. If a Navisworks the probe did not start is still running at the end, still put them back and say that it is running, because that Navisworks writes its own when it closes.
   The lead already took a backup at 13:0x on 2026-09-28 into %LOCALAPPDATA%\NwcFederatorLoop\navisworks-settings-backup (appdata and hkcu-navisworks-manage-22.0.reg). Do not touch it. It is the state after your first run.

C. WHAT THE RESULT SAYS.
8. Line 318 of the result lists step1-reflection.txt, which this probe never writes, so the work folder held files of an earlier run and the probe never empties it. Line 681 would pass on an NWD left by an earlier run. Empty the probe's own work folder at the start, it is under %LOCALAPPDATA%\NwcFederatorLoop, and check the NWD's write time is after the SaveFile call. Say whether the committed result file is the untouched output of one run of the committed probe. Result lines 315 to 320 list opened-copy-saved.nwd after watch.txt, which one Get-ChildItem on NTFS would not do.
9. Printed as if read and never read: lines 641 and 704 print the ExecuteAddInPlugin signature as a literal. Print it off the reflection. Line 658 prints "the smallest NWC in the copy" even when -Nwc names another file. Line 347 loads Roamer.exe with no Test-Path.
10. Step 4, lines 646 to 654: a missing source folder throws before $outcome["4"] is set, so the outcome reads "not reached" for a step that ran and failed. Set it before the step can throw.
11. Empty catches, against the house rule that no catch swallows an error: lines 117, 267, 278, 328, 339, 357, 429, 469, 484, 522, 537 and 754. Each writes what it caught. 429 is item 5. 117 makes a failed GetMethodBody look like a native method. 328, 339 and 357 can drop an option or read COMAutomationStartup as absent, and 5z-d item 2 rests on those.
12. The IL opcode walk is written three times, lines 112 to 158, 311 to 343 and 352 to 364. One function.
13. Add-Type at line 379 compiles through PowerShell's C# compiler, which writes temporary files under %TEMP%. Either say so in the result or avoid it.
14. The watchdog records only process names matching the pattern at line 535, so "records every new process" (lines 32 to 33, result line 270, scan.md 4663) is not true. Say what it records. It writes only on a change and does not record how long a pass took.
15. So the result carries it: the name of the parent process of the new Roamer, and of the licensing helpers, and whether each helper the probe saw start had exited by the end.

D. SCAN.MD 5z-d. Every claim needs a line of the result behind it or reads UNKNOWN. The reviewer found no line behind these:
- svchost as the parent, 4682, 4724, 4736 and 4782. Result line 228 says only parent 1328. With item 15 the result can carry it
- GenuineService starting AdskLicensingInstHelper twice, 4728 to 4729. Result 274 and 276 say only parent 26520
- the licensing agent and both helpers gone afterwards, 4755 to 4756
- the settings table at 4762 to 4768, the old and new values, the CER counts, the recent files moving down two. With item 7 the result can carry these
- "Every change below was made by pid 44888", 4758, "at its exit" 4770 and "at exit" 4787. Bader's Navisworks 34668 ran the whole time and the snapshots are before and after only, so who wrote each change and when is UNKNOWN
- the reg import by hand and the compare after it, 4772 to 4775. It was done by hand after the probe, not by the probe, and must say so. The lead records it as a write outside the loop folder
- "The build read 0 warnings and 0 errors", 4672, and "the edits being this probe", 4673. +edits comes from git status --porcelain, which any modified or untracked file sets
- "which is Navisworks.Document.22", 4719. Result line 205 says Navisworks.Document
- "No window of class #32770 appeared at any point", 4728, and "each half second", 4664. What the result carries is that none was seen at any pass
- "which MainImpl hands to the initialiser", 4722, and "only on the option Embedding or Automation", 4721, which covers only CommandLineParser's methods
- "passes false to Init ... that calls the native Bridge.StartupNavisworks", 4716 to 4718. The IL walk prints no branch opcodes and Init calls both GetRunningNaviswork and StartupNavisworks, so which one false selects is UNKNOWN
- "So a Roamer started by hand never serves that class, and the constructor always starts a new Roamer", 4722 to 4723, and the heading at 4715. One start shows 34668 did not serve the class on that run. "always" is UNKNOWN
- 4780 to 4782 make "comparing Roamer ids and start times" the design for run.ps1. With item 1 the design is the stronger rule
- add to STILL UNKNOWN: whether a constructor reaches a Roamer already running with -Embedding
- item 3 leaves out result lines 281 to 282, the main window hid and came back once while Visible was set to True

E. README.
- line 60 says the probe closes it by its process id. The run closed it through Dispose, and the id is the fallback
- lines 65 to 66, "Every other Roamer is listed first and never touched", only true once A is fixed
- line 43, "The three window probes", heads four bullets, older than F100, fix it while there
- say the result file is kept at the top of tools\probes

F. THEN RUN IT ONCE MORE, from the fixed script, so the committed result is the untouched output of one run of the committed probe. Replace the result file with it, and write 5z-d off that run. Before you start: list the Roamers. If a Navisworks the loop did not start is running, the probe still runs, as the first did, and never touches it.
