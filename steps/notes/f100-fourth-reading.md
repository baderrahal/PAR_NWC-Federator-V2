F100, the fourth reading of fix attempt 3, 2026-09-28. The probe at commit b01ad71 on fix-F100, tools\probes\probe-automation-start.ps1, 1834 lines, sha256 B1228224...73D8. Written down by the lead from the two reports, a reviewer and a breaker, neither of whom wrote any of the probe. Line numbers are of that commit.

Both confirm E1 to E8 and the text items of f100-fix-list-3.md are in the code. These are the faults they found inside what attempt 3 changed.

FROM THE REVIEWER

F1. Settings can be put back without a whole watchdog record. Line 1391 prints that TerminateProcess returned false and the watchdog stops, and 1393 sets $sync.Stop. The reasons not to put back, 1764 to 1788, never read $sync.DeadlineDone. So if TerminateProcess on the probe's own process returns false and the constructor then returns and adoption passes, the put back can go ahead with no watchdog from the deadline on. Only reachable if TerminateProcess on the current process fails, which no run has shown.

F2. A result line prints fixed text as if read. Line 1757 says the Automation DLL imports GetActiveObject and the constructor's IL takes StartupNavisworks when false. The line is fixed text, not step 1's own reading at 808 to 811, and no code checks the IL branch. True on this install, and could contradict step 1 on another -NavisworksPath.

F3. One registry write does not re-read its key. Line 1038 compares only the value, then 1040 runs CreateSubKey. When the new value is null, a key the compare read as there and now gone gets made again, and values get written into a key CreateKey skipped as there again at 1057.

F4. Window reads after adoption go by pid alone. WindowsOf is called at 1552, 1560, 1566, 1595 and 1637 without reading the start time again, 1595 and 1637 even right after a call threw. If the adopted Roamer died and its pid is reused, WM_GETTEXT goes to a process not adopted. The watchdog checks the start time first at 1341, the main thread does not.

Also, not against the bar: .claude\rules\loop.md said "the one Roamer that was not running before the call" where E2 made it the one possible start, fixed by the lead. The result file at b01ad71 still carries both licensing ids, and a squash merge brings the branch's final files, so it must be replaced or taken out before a merge. Polish P1 to P9, D1 to D5 and O1 to O3 are in the reviewer's report and not repeated.

FROM THE BREAKER

B1. An unproved start is printed as written down before the write is tried. Lines 1742 and 1748 print "written down" and line 1750 then tries AppendUnproved. If that throws, the catch at 1751 prints one general line and the two claims stand. The next run's step 2 does not see that pid and does not refuse. The deadline path at 1369 does it in the right order.

B2. A recorded start with no start time is matched on pid and the name Roamer alone, lines 1143 to 1146. If that Roamer ends unnoticed and Windows later gives its pid to a Navisworks Bader starts by hand, every later run refuses until someone edits the file.

B3, named since list 1 and still kept by a person: step 2 refuses only a Roamer whose command line names embedding or automation, line 1172. A Navisworks started by hand is not refused, and the header lines 19 to 21 leave the check that none runs to the person. An unattended run.ps1 must enforce it. Runs 1 to 3 each started a new Navisworks with Bader's open and none attached, which is three samples, not a proof.

The breaker found nothing in writing Bader's settings when another Navisworks ran, and nothing in losing a backup.
