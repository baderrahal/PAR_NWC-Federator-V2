# F103 part 1, fix list 1

From the reading of 63ebd7b on 2026-09-29 by a reviewer and two breakers, one on ownership
and close, one on Bader's things. Sent to the F103 developer by the lead. The developer was
part way through it when Bader shut the machine down, and its unfinished work is commit
e0760ac on fix-F103, not run and not read. The next session resumes from that commit with
this list. This is fix attempt 1 of 3.

FROM THE REVIEWER

1. THE MOVE IS NOT PROVED FUNCTION BY FUNCTION. H1 runs only -ReflectionOnly, which exits
   before the moved guards run. Write a proof file mapping every line of nw-guard.ps1 at
   377cb1a to its line in the probe at 0eb4ede, or to wrapper, comment or changed, and list
   every changed line with both texts. nw-guard.ps1 line 6 says every function was MOVED
   UNCHANGED, untrue at 63ebd7b: say which moved unchanged and which F103 added or changed.
   The reg export at nw-guard.ps1 948 now exports ("HKCU\" + $regSub) while line 952 prints
   HKCU Navisworks Manage 22.0 exported whatever key went: print the key exported
2. run.ps1 line 520 reads watch.txt sharing Read only, so a watchdog append at nw-guard.ps1
   766 landing then throws, goes to WriteErrors, and 1083 refuses the put back. Open it
   sharing ReadWrite. A harness case where the monitor reads while the watchdog writes
3. A CEILING close is written as ended by itself: the monitor's GONE check at run.ps1 439
   comes before the CEILING check at 525, and the verdict at 997 never reads $sync.Forced.
   Make the verdict read what forced the end. A harness case with a ceiling of a few seconds
4. M5 is read at run.ps1 963 to 977 AFTER the put back at 931 to 948, so it lists run.ps1's
   own writes as the start's. Read it before the put back
5. Check 18, the last read before the constructor, and the STOP paths of checks 13 and 14
   have no harness case. Add them
6. Two copies: the install record read at run.ps1 658 and 736 with its compare at 660 and
   737, and the mode list at 130 and 699. One each
7. HeldState's catch at nw-guard.ps1 484 returns unreadable and loses the text, and run.ps1
   902 then skips the close with no line. Keep and print the reason
8. The probe's comment at 93 to 94 about WM_GETTEXT to #32770 children only, and
   nw-guard.ps1 17 to 19's list of caller variables missing $utf8. Correct both
9. run.ps1 234's second Roamer list read outside a try, nw-guard.ps1 1050 printing $pidFile
   unmasked, and the AutoSave end compare using the in-memory list rather than reading
   autosave-before.txt back as the design says. Fix each

FROM THE OWNERSHIP BREAKER

10. It says GetWindowTextW on another process's window sends WM_GETTEXT and can hang the
    watchdog before the deadline and ceiling checks. Microsoft's contract for GetWindowText
    says the opposite for a window of another process. MEASURE IT: a StandIn role that shows
    a visible window and then blocks its own UI thread, and a harness case proving
    WindowRecords returns within a bound on it. If it does not return, move the caption read
    behind SendMessageTimeout too
11. WindowKind at nw-guard.ps1 442 to 447 calls MAIN any WindowsForms10 window whose caption
    ends in Autodesk Navisworks Manage 2025, so a Navisworks message box titled that way is
    never counted as a DIALOG or read. Call MAIN only a window with no owner, GetWindow
    GW_OWNER zero, that also matches, and everything else a DIALOG. A harness case with an
    owned window of that class and caption
12. The child reads, up to 20 children at 500 ms each per window, run inside the one pass
    that must reach the deadline and ceiling checks. Cap the time spent reading children per
    pass at 2 s, and write what was not read

FROM THE BREAKER ON BADER'S THINGS

13. INSTALL CAN SWAP THE BUNDLE UNDER A NAVISWORKS STARTED DURING THE BUILD. run.ps1's last
    Roamer read before install.ps1 is before its dotnet build, install.ps1 then deletes and
    copies the bundle at its lines 87 and 95 with no check of its own, and the read after
    the install at run.ps1 763 changes neither the verdict nor the exit code. Make
    build\install.ps1 refuse, with one line saying Navisworks is running and must be closed
    first, when any process named Roamer runs immediately before it removes the installed
    bundle, which is right for every one of the 27 users too. Make a Roamer found after the
    install change run.ps1's verdict and exit code. One sentence in INSTALL.md

Then the whole harness again with its output kept, harness4 once more, and the report: each
fix with its lines and its case, the harness totals, the Core counts and the solution build
counts.
