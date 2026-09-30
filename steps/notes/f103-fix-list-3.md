# F103 part 1, fix list 3, THE LAST FIX ATTEMPT

From the reading of fix attempt 2, 867697a, on 2026-09-30 by a reviewer and two breakers,
one on ownership and close, one on Bader's things. All three answered SAFE FOR ITEM 0 first,
and the lead made the second real start on 867697a on that answer. Of fix list 2's 21 items,
18 read as fixed. By the house rule this is the third and last attempt: if the reading after
it finds one of these items not fixed, or a new fault inside it, F103 goes to Bader's form.
So this list holds only what attempt 3 must close. The old faults the readers found outside
attempt 2 are their own register rows, T3-G6 to T3-G12, and are NOT to be touched here.
Line numbers are of 867697a.

FROM FIX LIST 2, NOT YET FIXED

1. ITEM 7. run.ps1 924 to 935 say both moved aside lines, the run folder's at 927 and the NOT
   RUN evidence folder's at 934, before record.txt is set at 937, so they reach only the
   console. Start the record first, then say both lines, so the retry's record and evidence
   carry them. RC2b then passes as it stands. Do not loosen RC2b, and split it into its parts
2. ITEM 17. No run case for an install.ps1 refusal passed through as exit 2. Add one the way
   H17 works: a copy of run.ps1 in a clean scratch git repository under -Work, its
   build\install.ps1 a stub that prints one REFUSED line and exits 2, APPDATA pointed at
   -Work, run as -Mode Install -Stamp <its HEAD>, and it must exit 2 and name the line
3. ITEM 21, THE WORDS. prove-run.ps1 738 and 755, StandIn\Program.cs 223 to 224 and
   tools\loop\README.md 199 to 200 say the real main window's owner was measured as never
   shown. The first start measured only that it has an owner and that the owner is enabled.
   THE SECOND START MEASURED IT, at 13:39:20 on 2026-09-30, record steps\runs\01\item0 line
   33: MAIN, class WindowsForms10.Window.8.app.0.27a2811_r7_ad1, caption "Untitled -
   Autodesk Navisworks Manage 2025", owner 725174, class
   WindowsForms10.Window.0.app.0.27a2811_r7_ad1, caption "", visible False, enabled True,
   process the adopted one. Write exactly that, with its record line, and nothing wider
4. The check at prove-run.ps1 756 counts owner lines the harness makes from its own copy of
   run.ps1 616. Make a case that runs the monitor's own owner line on an owned window

NEW FAULTS INSIDE FIX ATTEMPT 2

5. THE AUTOSAVE LISTING, run.ps1 354 to 361 and 1103 to 1104, ListingRows 338. A file whose
   name starts with # is skipped as a comment, and a listing cut short reads as whole, so a
   file the run deleted shows no line and a file that is there shows as added. Compare the
   count read back with the count the backup holds, and name any difference. A case for a #
   name and a cut listing
6. INSTALL.PS1, THE FAILURE PATH, 128 to 139. Three faults in the move aside of item 10:
   a) at 130, removing the partial new bundle has no try, so a file held in it stops the
      script before 131 puts the old one back, and the message never says where the old one
      is. Guard it, move the partial aside if it will not go, put the old one back, and always
      print where each is
   b) at 136 to 139 the old bundle is removed BEFORE the nested check, the expected files
      check and the reference check at 141 to 166 and 224 to 227, so one of them failing
      leaves the old bundle gone and an incomplete new one live, which contradicts
      README.md 102 to 103 and .claude\rules\loop.md 65 to 67. Remove the old one only after
      the last check has passed
   c) at 138 a failed removal of the old bundle prints one line and exits 0, and run.ps1 902
      then writes INSTALLED and never names the leftover. Say it in the verdict
   A harness case for each, in the fake APPDATA of H12b
7. THE CALL NAME, nw-guard.ps1 1016. The watchdog re-reads CallName after CloseAdopted
   returns, while the main thread clears it at run.ps1 1010 and 1027 as the killed server ends
   the call, so the verdict can read did not return with no name. Take the name before the
   close. The case at prove-run.ps1 1298 to 1305 must clear it the way the main thread does
8. THE FORCED CLOSE THAT FAILS, run.ps1 469 to 476 with nw-guard.ps1 557 to 568 and 1000 to
   1018. Once the watchdog wrote Forced or CallForced, CloseAtEnd never closes, even when the
   watchdog's Kill threw or the process is still there after it. Let CloseAtEnd close
   through the held handle when the process still reads same, under the one lock, and say
   which close did it. A case

WORDS THE READERS NAMED, EACH ONE LINE

9. install.ps1 111 and 138 say Navisworks does not load a folder not ending in .bundle, and
   prove-run.ps1 968 and 1007 say a loaded DLL holds its file with read sharing only. Neither
   is measured. Say UNKNOWN, or measure it with no Navisworks where that is possible
10. The 120 s limit is written twice, nw-guard.ps1 802 and run.ps1 54. One place
11. run.ps1's header 38 to 46 lacks exit 4 for a call that did not return, exit 1 for
    UNKNOWN or still running, and the 120 s among its numbers
12. run.ps1 886 copies install.ps1's REFUSED text, which holds the whole APPDATA path, into
    the install record unmasked. Mask it
13. run.ps1 1089 prints FINALLY after the end close and M5, which are in the finally too
14. prove-run.ps1 23 and 22, .claude\rules\loop.md 95 and 99 to 100, and tools\loop\README.md
    149 and 245 to 257, as the reviewer names them
15. prove-run.ps1 919, no installs folder was made, reads the real loop folder and fails on
    every harness run after the first real install. Read the harness's own
16. nw-guard.ps1 339 to 341 say no window of another process is read, while WindowRecords
    reads the owner's class, caption, visibility and process, which may be another
    process's, into the record. Say so, and mask an owner of another process in the record

Then the whole harness with every case passing, harness4, and the report as before.
