# Bader's item 5 of Q145: main installed in place and the timed runs of 1A02MM and 1A04PK

Written 2026-10-08 at 16:50 for the morning after F114's add-in pass merges, since the runs go with the
viewpoints box ticked, Q145's last line, and every test run before that merge keeps it unticked.

## What is proved by these two runs

F114, F132, F128, F120 and everything merged since set 04 and not yet proven: the coordinates rule F112
and F137, the Coverage sheet F127, the XML corrections F116, the workbook F118, the run log F119 and the
sets F115. The proof steps are in steps\03_bader_next.md: F132's 459 to 481, F120's 482 to 490, F128's
491 to 506, and F114's as its pass writes them. The log-reader agent turns each run into findings against
those Look for lines.

## The baseline

Set 04's first run of the C02 folder, steps\runs\04\item1-C02, is 1A02MM alone: the run took 2 hours 12
minutes 6 seconds, 7925.970 s, with VIEWS 7487.104 s of it, F85's per clash views. Set 04's run of C04,
steps\runs\04\item1-C04, is 1A04PK and hung, so 1A04PK has no baseline, Q124. The new runs are read
against those two: the whole run and the VIEWS, SETS, IMAGES, RENUMBER and GENERIC XLSX seconds.

## The copy

Set 05 was made at 13:26 on 8 Oct by tools\loop\prepare-copy.ps1 -Set 05, turn6\prepare-set05.txt: 156
files, 385.5 MB, every file read back by sha256, four communities under
%LOCALAPPDATA%\NwcFederatorLoop\runs\05\NMFed\NWC, C02, C04, C06, C07, with the empty Clash Report
folders made and NMFed.manifest.txt written last. It is made once and never emptied, so -Set 05 refuses now.

## The install, by the loop's rule for this machine

build\install.ps1's move aside is refused on this machine, F109, so main is installed in place, Bader's Q96
and Q98: the loop's own script %LOCALAPPDATA%\NwcFederatorLoop\turn4\install-in-place.ps1 -Checked <the
bundle staged and checked by build\install.ps1 -SkipBuild against a throwaway APPDATA under the work
folder> -Stamp <main's eight hex> -Backup <bundle-backup>, from wt-main at main's head with git status
printing nothing. Read tools\loop\README.md's Install section and the rule in .claude\rules\loop.md whole
first. No Roamer may run, read before and again right before. The installed stamp is read back as main's
commit. The bundle installed now is e4484d15 in place since 2026-10-01 with bundle-backup-e4484d15 beside it.

## The runs

Each through the real window, F106: powershell -NoProfile -STA -ExecutionPolicy Bypass -File
tools\loop\run.ps1 -Mode Run -Set 05 -Item 1 -Folder <runs\05\NMFed\NWC\C02> -Stamp <main's eight hex>,
from the clone whose HEAD is that commit, the first run with the clash XML and every group ticked, NO
-Untick, so the viewpoints box stays as F114's merge sets it, ticked. Then the same for C04. The picked XML
is the corrected matrix with its corrections and teams files beside it, as set 04 picked. Each run's
evidence goes to steps\runs\05\item1-C02 and item1-C04 by run.ps1, masked by tools\loop\mask-evidence.ps1
before any commit, no NWC, NWF, NWD, workbook or picture committed, a file over 20 MB named with its size
and sha256. A run is hung only by Q83's two clocks, and a run still going at 12 hours is CEILING, Q84. The
Auto-Save switch is written "3 0" for each start by the guard and put back after. His settings and his
logs folder are put back by the guard's rules and read back.

## What to read off each run, before the log-reader

The RESULT block, the TIMING blocks and the TIMING BESIDE SIZE block, the VIEWS TREE block with its seven
checks, the MIRROR blocks, the GENERIC MODELS block with 572 items over five models on 1A04PK and nought
on 1A02MM, the IMAGES seconds line, the RENUMBER row, the SETS blocks with the Generic Models folder, the
COVERAGE lines, the workbook check, and the two workbooks per group in the Clash Reports folder.
