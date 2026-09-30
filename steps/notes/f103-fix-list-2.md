# F103 part 1, fix list 2

From the reading of fix attempt 1, fcd981b, on 2026-09-30 by a reviewer and two breakers,
one on ownership and close, one on Bader's things. The reviewer read twelve of fix list 1's thirteen items fixed and
item 8 UNKNOWN, and the breaker on Bader's things read item 13 not fixed, the install race this
list takes up as item 10. Neither breaker found anything that starts, adopts, messages or closes a Navisworks or
window of Bader's. What follows is fix attempt 2, the second of three. Line numbers are of
fcd981b.

NEW FAULTS INSIDE FIX ATTEMPT 1

1. run.ps1 1027. The AutoSave listing is read back inside the put back's try, before
   SettingsPutBack, so a throw in that report only read stops the put back and leaves exit 6.
   Read it after the put back, or in a try of its own, so no report read can stop a write
   back. A harness case where the listing cannot be read and the put back still happens
2. nw-guard.ps1 955 with run.ps1 480 and 966 to 971. After a ceiling the finally reads the
   held state without looking at Forced, so while the watchdog's Kill is in flight it can
   close a second time and write that the process was still running and closed here. The
   finally reads Forced first and never closes twice. A harness case
3. run.ps1 989 then 1000 to 1014. The watchdog is stopped before M5, so a Navisworks of
   Bader's that starts and ends while M5 scans is covered by no record, and the put back can
   revert his own changes. Stop the watchdog after M5, just before the put back reasons are
   read, so the record covers the whole stretch. A harness case with a stand in started and
   ended inside M5

OLD FAULTS OUTSIDE FIX ATTEMPT 1 THAT THIS PART CARRIES

4. run.ps1 438 to 450 and on. FindToolLog takes any new run log in Bader's logs folder as the
   loop's own. In item 0 the plugin is never called, so any log there is his by
   construction: item 0 reads no tool log at all and starts no hang clock on one, and says
   so. How items 1 to 5 tell the loop's own log from his is part 2's to settle
5. run.ps1 949 and 934. Nothing bounds a blocked Dispose or a blocked Visible but the 12 hour
   ceiling, and the monitor has stopped by then. Bound each with a timeout, 120 s, and
   after it close the adopted process through its held handle, which the rules allow for the
   loop's own, and write that Dispose did not return
6. run.ps1 482 to 486 and 421. Every held state but same becomes GONE, so an unreadable
   process reads as ended by itself, exit 7. Keep UNKNOWN as its own state and verdict
7. run.ps1 1065 to 1073, 256 to 260 and 858 to 862. A run stopped NOT RUN copies its
   evidence, and a retry with the same Set is then refused at check 11. Let a retry move a
   NOT RUN evidence folder aside, never emptied, the way the run folder is
8. prove-run.ps1 36 and 100. The default -Work is NwcFederatorLoop\test, which BaderState does
   not leave out, so with the default every nothing of Bader's changed check fails on the
   harness's own files. Make the two agree
9. prove-run.ps1 109 to 129. The harness looks for a foreign Roamer only after each case, and
   an unreadable process list reads as none. Look before and after each case, and stop on an
   unreadable list

INSTALL, BADER'S BUNDLE

10. build\install.ps1 91 to 96. Between the Roamer read and Remove-Item a Navisworks can start
    and hold the bundle's DLLs, and a recursive delete that fails part way leaves the bundle
    torn. Move the installed bundle aside by one rename first, which fails whole if a file in
    it is held, so the old bundle stays whole on any failure, then copy the new one in, then
    remove the one moved aside. Say it in one line when the rename is refused
11. build\install.ps1 96. Nothing checks whether the bundle folder, or any folder above it up
    to %APPDATA%, is a junction or a link, and a recursive delete through one can delete what
    it points at. Refuse when any is, the way run.ps1's PathRefusal does. Measured by the
    lead at 11:29 on 2026-09-30: the installed bundle folder is a plain directory

POLISH, EACH ONE LINE

12. The stale comments the reviewer names: nw-guard.ps1 884 to 885 and 351, the probe's 146 to
    147, prove-run.ps1 18 to 21
13. run.ps1 138 is a second copy of the mode list at 129
14. The listing's columns are parsed in four places, ReadListing 331 to 332, LastInstallMatch
    699, and the logs-before.txt reads at 892 and 1042. One reader
15. $sync.Closed is set at run.ps1 480, 483 and 538 and read nowhere
16. run.ps1 723 to 725 print matches: False when the bundle could not be listed. UNKNOWN
17. An install.ps1 refusal reaches Install as exit 1, a fault, where it is exit 2, refused
18. The verdict has no field for an adopted process still running at the end, so it can read
    closed by Dispose or closed, FORCED when it was not. Add it
19. A window of the main class with an empty caption reads DIALOG, and the 2 s cap bounds the
    sends but not the walk of its children. Bound the walk too, and read the first real
    start's MAIN and DIALOG lines before choosing how to class that window
20. .claude\rules\loop.md 84 to 87 and 94 to 95, and tools\loop\README.md 146 to 152, as the
    reviewer names them

MEASURED BY THE LEAD, NO FIX NEEDED

- An exited process is not found by Get-Process -Id or Win32_Process even while a handle to
  it is held: ping.exe pid 38992 killed at 11:29 on 2026-09-30 with its Process object held,
  Get-Process -Id threw ProcessCommandException and Win32_Process returned no row. So a real
  run reads its closed Navisworks as gone and can put back, as the breaker asked to check

THE FIRST REAL START, item 0 with no window, ran on fcd981b from 11:29:29 to 11:40:10 in
Bader's window of 2026-09-30, before this list was sent. Its record is
%LOCALAPPDATA%\NwcFederatorLoop\runs\00\item0\record.txt. What it measured bears on the list:

21. THE REAL NAVISWORKS MAIN WINDOW HAS AN OWNER. Record line 32: class
    WindowsForms10.Window.8.app.0.27a2811_r7_ad1, caption "Untitled - Autodesk Navisworks
    Manage 2025", owner 855918, owner enabled True. So item 11's rule, MAIN only with no owner,
    classed the main window a DIALOG, read its children, and turned a clean run into exit 5
    with a false DIALOG finding. No guard was broken: the messages went to the adopted
    process's own window. Class the main window by what tells it apart from a dialog of the
    same process, and write the owner's class, caption, visibility and process for every
    window the rule classes, so the next start measures what the rule rests on. Whether a message box
    of Navisworks is owned by the visible main window is NOT MEASURED, no run has shown one. A
    candidate rule is that the main window's owner is not visible. A harness case for each shape
- M4, the processor time of an idle Navisworks, read every 15 s over the 360 s hold: never
  zero, 0.094 to 0.578 s a sample, about 1.2 to 1.6 s a minute. So the hang rule as written,
  no processor time at all for five minutes, can never fire on an idle Navisworks. That is
  Q83, for Bader. Nothing in this list changes the rule
- the constructor returned after 92.55 s. Dispose returned after 0.49 s and the process read
  gone 8.9 s after. The watchdog made 749 passes, the longest 9643 ms and the longest gap
  10157 ms. No record times M5. Its m5.txt read files written at least 9.7 s after the watchdog stopped,
  so item 3's gap is at least that long and real
