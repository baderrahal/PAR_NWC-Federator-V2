# Audit of many communities and the discipline reports, C06 and C07

Bader's message of 17 Oct 2026 late evening. F208 many communities and nested packages, merged in 288. F209 one Excel per
discipline per package, merged in 289. This file is F210, the test of both on copies of C06 and C07, run on 18 Oct 2026
with main 14b434f4 installed, its title reading 14b434f4.

Simple words. PROVEN means a run on Navisworks showed it and I read the result. UNKNOWN means nothing showed it yet.

## The source and what was left alone

- The message names C:\Users\bader\Desktop\NWC Fed. That folder is not there. The folder is NM fed on the Desktop, holding
  NWC\C06 and NWC\C07 with 64 NWCs each, C07 the same names and sizes as C06, all coded 1B06. It was only read.
- Its listing, every file's name, size, date and SHA-256, was taken at 23:38 on 17 Oct and again at 04:52 on 18 Oct after
  the last run: 135 files and 25 folders both times, 0 lines differ. Both listings are in steps\logs, named
  2026-10-18-C06-C07-source-listing-before.tsv and -after.tsv. The launcher also listed it before and after each run, 135
  files every time.
- The message names claude\audit-c02-2026-10-17.md. It is on no branch, not on GitHub and nowhere on this PC. So the seven
  C02 bugs are read as the items of the chat audit of C02 kept at the top of steps\03_bader_next.md, all eleven checked
  below. Which seven the message means was UNKNOWN until Bader's message of 18 Oct gave the map, the section below.
- Nothing under samples or steps\logs was changed. The new logs were added to steps\logs as new files only.

## The layouts, all copies under C:\NWC-PC-Test\work\multi

| layout | parent | packages | dummy left unticked | Report before the runs |
|---|---|---|---|---|
| flat | flat\Parent | C06 itself 64 NWCs, C07 itself 64 NWCs | Dummy itself, 1 NWC, a copy of 1B06PE ST | empty C06 and C07 |
| ACC | acc\Parent | C06\Infrastructure 29, C06\Infrastructure and Landscape 35, the same for C07 | Dummy\Roads & Bridges, 1 NWC | empty C06 and C07 |
| fail | fail\Parent | A\Broken, one NWC of random bytes, B\Good, a copy of 1B06BS | none | empty A and B |

Infrastructure holds 1B06BS, K1, M1, PE, PH, PO, PS, PW, WM and 1C06M2, 29 files. Infrastructure and Landscape holds
1B06BC, G1, KI, P1, PG, PK, PP, PT, WL, WO and 1C06PK, 35 files. One copy in C07 of each layout,
1104-PAR-1B06PE-ZZZ-ST-MOD-000001.nwc, was dated back 10 days to 8 Oct so the file age check has an old file to find.

## The runs

Every run was started by the window driver on the real window, through UI Automation, the way a person presses the
buttons. Each log and its tsv are in steps\logs, named 2026-10-18-C06-C07 and the layout and page.

| run | log | layout and page | ticked | groups | done, partial, failed | clashes | pictures | time |
|---|---|---|---|---|---|---|---|---|
| 1 | run-20261018-010353 | flat, NWC page | C06 and C07 | 42 | 42, 0, 0 | 12414 | 3514 | 60.0 min |
| 2 | run-20261018-020553 | flat, NWF page | C06 and C07 | 42 | 42, 0, 0 | 16406 | 3597 | 51.6 min |
| 3 | run-20261018-030214 | ACC, NWC page | C06\Infrastructure and Landscape alone | 11 | 11, 0, 0 | 5537 | 3567 | 11.8 min |
| 4 | run-20261018-031559 | ACC, NWC page | C06 and C07 | 31, the 11 built by run 3 left out | 31, 0, 0 | 6877 | 3516 | 34.1 min |
| 5 | run-20261018-035152 | ACC, NWF page | C06 and C07 | 42 | 42, 0, 0 | 16406 | 3598 | 54.5 min |
| 6 | run-20261018-044816 | fail, NWC page | A and B | 2 | 1, 0, 1 | UNKNOWN for A, 0 for B | none | 1.8 min, 100.7 s of it a dialog |

Runs 1, 2, 4 and 5 each pressed Run with nothing ticked first. The counts agree across the layouts: C06 holds 6207 clashes on
the NWC page whether it is one package or two, 670 in Infrastructure and 5537 in Infrastructure and Landscape, and 8203 on
the NWF page, 688 and 7515.

## Feature 1, many communities and packages, PROVEN

- The tree levels. The flat layout reads 3 communities, each one package that is the community itself, C06 itself 64 NWC
  files and 21 groups, C07 itself the same, Dummy itself 1 file and 1 group. The ACC layout reads 3 communities and 5
  packages: C06 holds Infrastructure, 29 files and 10 groups, and Infrastructure and Landscape, 35 files and 11 groups, C07
  the same, Dummy holds Roads & Bridges, 1 file and 1 group. Names with spaces, the word and and an ampersand read whole.
  Every box was unticked after Read, read off the window by the driver.
- Run refused with nothing ticked, on both pages and both layouts, 4 of 4 tries: The run cannot start. Nothing is ticked.
  Tick a community or a package first. The log reads PACKAGES the run of packages refused before starting.
- Ticking a community ticked its packages: C06 and C07 ticked, every package under them read On. Ticking one package alone
  left its community Indeterminate, a dash, and the line under the tree read 1 package ticked.
- A single package ticked alone ran alone, run-20261018-030214: C06\Infrastructure and Landscape, 11 groups, 11 done.
  Folders made were Report\C06\Infrastructure and Landscape and its NWF and NWD, then Log and Clash report. Nothing was
  made for C06\Infrastructure, Report\C07 stayed empty and no Report\Dummy was made.
- The unticked community was never run and nothing was written for it. After all runs Report holds only C06 and C07 in
  both layouts, no Dummy folder, and no line of any log names a group of Dummy as run. The log names it left out with its
  path and count, left out Dummy\Roads & Bridges 1 NWC file.
- The report folders mirror the path from the parent: Report\C06 for flat, Report\C06\Infrastructure and
  Report\C06\Infrastructure and Landscape for ACC, each with NWF, NWD, Log, Clash report, the four discipline workbooks
  and, after the NWF page, Backup. Every folder made is logged, PACKAGES folder made, and one already there is logged as
  folder there.
- Each package ran in turn and RESULT names community and package on every group line, with totals per package, per
  community and for the run, run-20261018-010353: package C06 : 21 groups, 21 done, 0 partial, 0 failed, 6207 clashes, and
  run : 2 communities, 2 packages, 42 groups, 42 done, 0 partial, 0 failed, 12414 clashes.
- The NWC page leaves out a package whose NWFs already exist and says why, package C06\Infrastructure and Landscape : not
  run, every group of it has an NWF already. NWF exists. Use the NWF page. The other three packages of that run went on.
- The NWF page runs the NWFs each ticked package holds in its own NWF folder, makes one backup of each in the package's
  Backup folder, writes its Excel and discipline workbooks under dated names beside the earlier ones, and names community
  and package on every group line.
- One failure never stopped the others, run 6. A\Broken, one NWC of random bytes, ended FAILED, No file in this group
  appended, so nothing was written, and B\Good after it ran DONE. RESULT reads package A\Broken : 1 group, 0 done, 0
  partial, 1 failed, clashes UNKNOWN, and package B\Good : 1 group, 1 done. The discipline workbooks of A\Broken say not
  read, the group ended before its models were read. But the run only went on because I answered a Navisworks message box about the
  corrupt file, F210-R6 below.

## Feature 2, the discipline reports, PROVEN

- One Excel per discipline per package, in the package's report folder, one sheet each, never one workbook of many sheets:
  8 on the flat layout's first run, C06 AR checks.xlsx to C07 EL checks.xlsx, and 16 on the ACC layout, such as
  C06 Infrastructure and Landscape AR checks.xlsx in Report\C06\Infrastructure and Landscape. The NWF page wrote the same
  again under dated names and wrote over nothing. Every workbook was read back whole by the tool, CHECKS read back 1039
  of 1039 lines and so on, 0 element ids not readable.
- Every row is in the right place. Read off the files, 2684 rows of the flat layout and 2692 of the ACC layout: each row's
  package is its workbook's, its community is the package's first part, and its file is of the workbook's discipline.
- Every check fired on real data. Per community, the same on C06 and C07 and on both layouts:

| check | result per community | from |
|---|---|---|
| Coordinates | 7 far, 35 near, 22 reference | EL of 1B06BC 2774.306 km from AR, its site SITEWIDE PHASE 3, AR site Internal. EL of 1B06PH 2029.248 km. EL of 1B06PS 336.262 km. ST of 1B06K1 2823.784 km, of 1B06P1 twice 2823.723 km, of 1B06PS 2822.679 km |
| Missing discipline | 29 missing | AR missing in 10 of 21 groups, ST in 4, ME in 7, EL in 8, the same counts read off the file names |
| Naming | 52 follow the AR pattern, 0 break it | the pattern learned from 12 AR names, 7 parts split on -, part 3 shaped like 1B06BC, part 7 shaped like 000001. Every real name follows it. The break was proved in F209 on a renamed copy |
| File age | C06 64 within 7 days, C07 63 within and 1 older | the copy of 1B06PE ST dated back to 8 Oct reads older than the limit, the only one |
| CAD | 35 found, 59 none found | AR of 1B06PO and 1B06WO 10 each, ME of 1B06PH 3, 1B06PP 10, 1B06WL 2 |
| Generic Models | 982 found, 53 none found | 796 in 1C06PK ST, 101 in 1B06PT ST, 38 in 1B06PP ME and 47 more. 105 levels read not readable, 99 of them in 1B06PT |

- The C02 case is a Core test, EL and ST 2,823 km from AR and ME, and the same distance shows on C06 and C07 in ST of
  1B06K1, 1B06P1 and 1B06PS.
- Three CAD and three Generic Models ids found again in Navisworks by the Element tab's Id, by tools\probes\DisciplineProbe
  in its mode find on copies of the 64 C06 NWCs, discipline-probe-result-20261018-find.txt, nothing saved:

| id | the report line | what Navisworks holds |
|---|---|---|
| 784492 | CAD in AR of 1B06PO and 1B06WO, in PAR-AR-FXP-BASIN PEDESTAL of type PLU-SYM-BIM-3D-PR-Default, level LGF | 2 items, one per model, Plumbing Fixtures, that family and type, Level LGF, imports 10_127_127_127 and on under it |
| 6489702 | CAD sewage-pump.dwg in ME of 1B06PP, in PAR-ME-MEC-ME-PW-MEQ-Submersible Pump, level GROUND LEVEL | 1 item, Mechanical Equipment, that family, type Standard, Level GROUND LEVEL, sewage-pump.dwg under it |
| 2973244 | CAD Air Vent Valve.dwg in ME of 1B06WL, in PAR-ME-PPA-AIR VALVE SEWAGE, level ZZZ | 1 item, Pipe Accessories, that family and type, Level ZZZ, Air Vent Valve.dwg under it |
| 3607947 | Generic Models in ST of 1C06PK, PAR-ST-GEN-REC OPENING -PAR, STR-GEN-20000X2000-PAR, level No level | 1 item, Generic Models, that family and type, Layer No level |
| 3470102 | Generic Models in ST of 1B06PT, PAR-ST-GEN-MECH EQ BASE-PAR, level FL1 | 1 item, Generic Models, that family and type, Level FL1 |
| 3872693 | Generic Models in ST of 1B06PK, PAR-ST-GEN-REC OPENING -PAR, STR-GEN-1000X1000-PAR, level No level | 1 item, Generic Models, that family and type, Layer No level |
| 3470148 | Generic Models in ST of 1B06PT, REC-SLEEVE 5, SLEEVE, level not readable | 1 item, Generic Models, that family and type, Level, Schedule Level and Layer all empty, so not readable is right |

## The chat audit of C02, read on C06 and C07

Each item of the chat audit is read off the runs of the flat layout, run-20261018-010353 on the NWC page and
run-20261018-020553 on the NWF page, and off the runs of the ACC layout where they add to it. Still there means the runs
show it on C06 and C07. Gone means they show it is not there. Cannot tell means these runs could not show it either way.

| n | item | C06 and C07 | what shows it |
|---|---|---|---|
| 1 | Approved clash with no picture | cannot tell | No clash is Approved in these copies. The second run read New 1996 and Active 6207 per community. The rule is unchanged, every run's log reads New, Active, Reviewed only. Q194 |
| 2 | Dated Excel in Log | still there, as you ruled in Q173 | The NWF page wrote a dated Excel beside each of the 42 of the NWC page, and the discipline workbooks the same way, 1104-PAR-1B06BC-ZZZ-BM-MOD-000001 2026-10-18 020610.xlsx and C06 AR checks 2026-10-18 025728.xlsx |
| 3 | Penetrations and By design on the NWF page | still there, off by default | The NWF page log reads penetrations no and by design no. Q195 |
| 4 | Excel columns | still there | Layer is the grid level on every row where both are filled, 5944 of 5944 per community, empty where Grid is, 263. Item Name empty on 568 Item 1 and 441 Item 2 cells of 6207. No logo at A1. Q196, Q197, Q198, Q201 |
| 5 | Picture links after a move | still there | All 3513 picture links in C06's workbooks of the first run are full file:/// paths. Q199 |
| 6 | Foundation in the penetration rule | cannot tell | Penetrations were not ticked in any run. The rule is unchanged in the repo. Q200 |
| 7 | C02 NWCs on Revit's internal origin | still there, in the models | RESULT names on Revit's internal origin AR of 1B06BC, 1B06G1 and 1B06PS and ST of 1B06M1, 1B06PS and 1C06M2. The discipline reports find 7 models per community 336 to 2,824 km from their AR. Q191 |
| 8 | Clash depth over 1 m | still there, geometry | 580 of 6207 clashes per community on the first run, 1655 of 8203 on the second, which adds the floors against columns of 1C06PK. Measured as geometry in F207 |
| 9 | Old status | cannot tell | No clash reads Old in any run. Not a bug by your answer, Q193 closed |
| 10 | F206, pictures left empty by a model saved hidden | the cause is gone, empty pictures are not | No model was hidden whole in any run, every IMAGES HIDDEN line reads nothing hidden or 2 items and 0 of 5 models. Yet 76 of 399 pictures of 1B06BC and 13 of 374 of 1B06P1, with 12 more nearly empty, show only the sky, the same pictures on the NWC page and the NWF page. F210-R3 |
| 11 | F207-R1, views check 4 | cannot tell | Check 4 held on every group of every run, 0 broke it. The C04 cause, models hidden in the NWF, is not in these NWFs, so whether the bug is gone is UNKNOWN |

## The seven C02 bugs, mapped by Bader on 18 Oct

The file claude\audit-c02-2026-10-17.md lives in Bader's chat project and not in the repo. His message of 18 Oct maps its
seven bugs to the items of the table above, so each was checked on C06 and C07 by its item.

| C02 bug | what it is | item above | on C06 and C07 |
|---|---|---|---|
| 1 | the tests show Old | 9 | cannot tell, and not a bug by his answer to Q193 |
| 2 | no picture for an Approved clash | 1 | cannot tell, no clash is Approved in these copies, Q194 |
| 3 | the dated Excel and XML | 2 | still there, as he ruled in Q173 |
| 4 | Penetrations and By design off on the NWF page | 3 | still there, off by default, Q195 |
| 5 | the Excel against the client's | 4 | still there, Q196, Q197, Q198 and Q201 |
| 6 | the picture links written as full paths | 5 | still there, Q199 |
| 7 | the foundation in the penetration rule | 6 | cannot tell, penetrations not ticked in any run, Q200 |

## Bugs and findings of this round

Each is a row of steps\tracker.csv and a line of steps\01_next.md. None was fixed in this round, F210 only tests.

- F210-R1, the memory stop across packages. One Navisworks keeps the memory of the groups it ran. On the flat layout's
  first run, 42 groups in one start, the memory stop fired first in C06's 11th group, 1B06PK, at 12.0 GB, then in 6 of the
  7 later C06 groups with clashes and in all 15 C07 groups with clashes. 3513 of C06's 6207 clashes were pictured and 1 of
  C07's 6207. On the NWF page run C07 got none. The run went on and wrote every Excel, HTML and viewpoint, each stop named
  in the log. A run of one package of 11 groups hit it 3 times. Q204.
- F210-R2, a group of one discipline runs no test on the NWC page and does on the NWF page. The rule of D5 and F77 says one
  discipline cannot clash with itself, but 284 of the 1830 tests of the pack 1104-PAR set a code against itself. The NWC
  page ran no test on 1B06BS, 1B06KI, 1B06PG, 1B06PT, 1B06PW and 1C06PK, and the NWF page found 1996 clashes in them from
  the same NWCs, 1928 in 1C06PK alone. Q203.
- F210-R3, pictures that show only the sky. 76 of 399 pictures of 1B06BC and 13 of 374 of 1B06P1, with 12 more under 0.2
  percent of the picture, on the NWC page's first run and the same pictures again on the NWF page. They fall in many tests,
  BLD-EL-Conduits & Conduit Fittings-vs-BLD-EL-Electrical Equipment 30 and BLD-FF-Pipe Accessories-vs-BLD-FF-Pipes & Pipe
  Fittings 13 among them, near the origin and far from it. Both groups hold a model about 2,800 km off, but so does 1B06K1
  with 0 of 40 empty. No model is hidden. The cause is UNKNOWN.
- F210-R4, the log names each package by its path from the parent, under one line with the parent's full path, in PACKAGE
  TREE and PACKAGES TICKED. The message asks for the full path on each package's line.
- F210-R5, on the NWF page the confirm of a run of packages says Pictures: UNKNOWN old jpg files in the Clash report folder
  are written again or removed, where the confirm of one NWF counts them.
- F210-R6, a corrupt NWC stops an unattended run. Document.TryAppendFile on it opens a Navisworks message box, the file
  could not be opened because the contents are corrupt or it is currently unavailable, with one OK button, and does not
  return until a person answers. In run 6 the run stood still for 100.7 s until I answered it, then went on as above.
  Overnight it would wait until morning. Measured by the run and written into docs\history\scan.md 5z-zh.

Two things found that are not bugs of the tool.

- The probe's mode find returned 2 twice before it ran. The runner built its arguments in an if, which PowerShell hands back
  as an object[], and a params string[] then receives one joined string. Measured with a small method, the untyped array
  reached it as 1 argument and the typed one as 5. Fixed on this branch, 9e1dad5, the third start found all 7 ids.
- The 20 CAD lines of AR in 1B06PO and 1B06WO all carry the element id 784492, ten lines in each model. That is one basin
  pedestal family holding 10 imports named 0__127_127_127 to 10_127_127_127, measured in F209 and found again by the
  probe. The file name of such an import is not readable because the import carries none.

## What stays UNKNOWN

- Why 89 pictures of 1B06BC and 1B06P1 show only the sky.
- Whether the memory comes back between groups in any way the tool can call, so many packages can run in one start.
- How a CAD file linked or imported into a project itself shows, none being in C06, C07 or C02.
- Whether views check 4 still fails on C04, since these NWFs hold no hidden model.
- Whether a truncated real NWC, or one another program holds open, shows the same message box as the random file.
