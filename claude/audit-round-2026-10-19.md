# Audit of the round of 19 Oct, F231 to F238

Bader's round of 19 Oct 2026, read off multi3 on 18 Oct: six fixes in his order, the full run for him and the end of the
round. This file is begun by F231 with the append table his fix 1 asks for, and each later fix adds its part. F238 writes the
rest.

## F231, the append measured against a manual Append

The NWCs of 1B06BC, 1B06PS and 1B06K1 copied out of the desktop folder's NWC\C06 into C:\NWC-PC-Test\work\append-check, their
dates kept, the desktop folder 0 lines differing before and after. Each group appended by hand in the window of a plain
Navisworks Manage 2025, File, New, Append with the NWCs in the tool's order and Save as <group> manual.nwf, no option touched,
then built by the tool's NWC page of main f92023f into the same folder, then run on the NWF page, its Refresh and UpdateFiles.
tools\probes\AppendProbe read each NWF and NWD read only, every file the same SHA-256 after. docs\history\scan.md 5z-zn.

Positions in metres, E N Z, as Navisworks reports the units of each model. T is the translation of the model's transform, box
its root item's box, min to max. A difference is the largest of the nine numbers set against the manual Append, in mm.

| model | manual Append | tool after the NWC page | tool after the NWF page | difference after the NWC page, mm | difference after the NWF page, mm |
|---|---|---|---|---|---|
| 1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -0.372 to 11.416 7.827 8.457 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -0.372 to 11.416 7.827 8.457 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -0.372 to 11.416 7.827 8.457 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc | Feet, T 323886.396 2755364.307 11.033, box 323867.750 2755346.922 6.602 to 323922.019 2755377.178 18.729 | Feet, T 323886.396 2755364.307 11.033, box 323867.750 2755346.922 6.602 to 323922.019 2755377.178 18.729 | Feet, T 323886.396 2755364.307 11.033, box 323867.750 2755346.922 6.602 to 323922.019 2755377.178 18.729 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box -7.535 -9.170 -5.331 to 25.142 6.919 7.325 | Feet, T 0.000 0.000 0.000, box -7.535 -9.170 -5.331 to 25.142 6.919 7.325 | Feet, T 0.000 0.000 0.000, box -7.535 -9.170 -5.331 to 25.142 6.919 7.325 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc | Feet, T -33.976 -1.625 0.000, box -16.774 -15.145 -2.100 to 11.170 12.327 8.000 | Feet, T -33.976 -1.625 0.000, box -16.774 -15.145 -2.100 to 11.170 12.327 8.000 | Feet, T -33.976 -1.625 0.000, box -16.774 -15.145 -2.100 to 11.170 12.327 8.000 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-ST-MOD-000002.nwc | Feet, T 34.196 0.452 -4.331, box 19.325 -10.098 -6.381 to 34.946 1.202 0.094 | Feet, T 34.196 0.452 -4.331, box 19.325 -10.098 -6.381 to 34.946 1.202 0.094 | Feet, T 34.196 0.452 -4.331, box 19.325 -10.098 -6.381 to 34.946 1.202 0.094 | 0.000 | 0.000 |
| 1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc | Feet, T 658144.882 2746014.845 683.828, box 658136.196 2746027.197 683.632 to 658147.127 2746042.493 687.531 | Feet, T 658144.882 2746014.845 683.828, box 658136.196 2746027.197 683.632 to 658147.127 2746042.493 687.531 | Feet, T 658144.882 2746014.845 683.828, box 658136.196 2746027.197 683.632 to 658147.127 2746042.493 687.531 | 0.000 | 0.000 |
| 1104-PAR-1B06K1-ZZZ-ME-MOD-000001.nwc | Feet, T 658144.890 2746014.825 683.828, box 658136.693 2746039.006 682.678 to 658144.804 2746043.103 687.694 | Feet, T 658144.890 2746014.825 683.828, box 658136.693 2746039.006 682.678 to 658144.804 2746043.103 687.694 | Feet, T 658144.890 2746014.825 683.828, box 658136.693 2746039.006 682.678 to 658144.804 2746043.103 687.694 | 0.000 | 0.000 |
| 1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 12.875 6.258 3.639 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 12.875 6.258 3.639 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 12.875 6.258 3.639 | 0.000 | 0.000 |
| 1104-PAR-1B06PS-ZZZ-AR-MOD-000001.nwc | Feet, T 660063.927 2744376.693 675.667, box 660062.071 2744451.163 675.065 to 660077.795 2744466.295 681.568 | Feet, T 660063.927 2744376.693 675.667, box 660062.071 2744451.163 675.065 to 660077.795 2744466.295 681.568 | Feet, T 660063.927 2744376.693 675.667, box 660062.071 2744451.163 675.065 to 660077.795 2744466.295 681.568 | 0.000 | 0.000 |
| 1104-PAR-1B06PS-ZZZ-EL-MOD-000001.nwc | Feet, T 323886.396 2755364.307 11.033, box 323961.367 2755357.037 8.608 to 323976.914 2755373.546 15.981 | Feet, T 323886.396 2755364.307 11.033, box 323961.367 2755357.037 8.608 to 323976.914 2755373.546 15.981 | Feet, T 323886.396 2755364.307 11.033, box 323961.367 2755357.037 8.608 to 323976.914 2755373.546 15.981 | 0.000 | 0.000 |
| 1104-PAR-1B06PS-ZZZ-ST-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 88.197 11.033 6.467 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 88.197 11.033 6.467 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 88.197 11.033 6.467 | 0.000 | 0.000 |

| NWD | union of the manual boxes | the NWD after the NWC page | the NWD after the NWF page | difference after the NWC page, mm | difference after the NWF page, mm |
|---|---|---|---|---|---|
| 1B06BC | box -17.020 -15.391 -6.381 to 323922.019 2755377.178 18.729 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -6.381 to 323922.019 2755377.178 18.729 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -6.381 to 323922.019 2755377.178 18.729 | 0.000 | 0.000 |
| 1B06K1 | box -1.370 -1.140 -1.550 to 658147.127 2746043.103 687.694 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 658147.127 2746043.103 687.694 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 658147.127 2746043.103 687.694 | 0.000 | 0.000 |
| 1B06PS | box 77.005 -1.827 -1.400 to 660077.795 2755373.546 681.568 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 660077.795 2755373.546 681.568 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 660077.795 2755373.546 681.568 | 0.000 | 0.000 |

Largest difference 0.000 mm, 0 numbers UNKNOWN, the source file names and units of all 22 pairs the same.

So the tool's append matches the manual Append, and the far models are in the NWC files themselves. No code changed.

## F232, packages first, one route per page

The NWC page has four steps, 1. Packages, 2. Grouping, 3. Outputs and 4. Clash, and the one Run button in the bar under the
steps. The NWF page has its packages first, a tree of community, package and NWF read off the Report folder, then the
tolerance, the five step boxes and one Run. Gone: the Source step, step 5 and its Run button, the NWF and NWD folder boxes,
More, rarely changed, the NWF picker with its File and Folder buttons, the Recent box and the panel on the right, with every
member and test only they used: the scanned run of the window, the picked NWFs' plan, the list of NWFs last used, the names
typed over, four picker kinds, and the engine's scanned run constructor and preview. 47 files under src, tests and tools,
2175 lines added and 4452 taken out.

Proved on copies under C:\NWC-PC-Test\work\smoke232, a parent holding 1B06PS's three NWCs itself and C06 with 1B06BC's five
and 1B06PG's one, the build ab1c9167: the tree read the parent as community Parent with package Parent itself, Run with
nothing ticked was refused, the findings under the tree counted 9 files of the ticked packages, step 2 showed 1B06BC and 1B06PS
ticked and 1B06PG reading One discipline, not built with its box off and greyed, the NWC page ran 2 groups each in its own
Navisworks, 2 DONE, 1B06PG named left out, then the NWF page read 2 NWFs off the Report folder, ran both each in its own
Navisworks, 2 DONE. The desktop folder 0 lines differing, Navisworks' settings put back with 0 lines differing.

The reviewer and the breaker read the change. Fixed here: a Report folder that would put the parent's own package inside the
parent is refused, a ticked NWF gone at Run is named, a row its rule left unticked is no person's untick, a group whose NWF is
there reads NWF exists. Use the NWF page., the stand-in of the loop builds again, and the window's new words are Core's. Asked:
Q213 a parent and a folder of its name, Q214 the changed files list and the table on the NWF page, Q215 where the one Run
button sits. Open as rows F232-R1 to R9, most older than F232: the loop's window driver and two probes name the controls that
went, one manifest per package Log folder, rebuilt NWFs in dated folders the tree does not show, a run reading the last Read,
the two groupings that make one discipline groups, the discipline workbooks missing unread files, left out groups in no RESULT
line, a drive root parent and a wide Report folder, ticks reset by a package ticked again.

## F233, one discipline groups are not built

A group of one discipline is not appended and gets no NWF, NWD, Excel, Internal Excel, HTML, picture folder or viewpoints, on
both pages. The log names it in one line, `NOT BUILT  C06  1B06PG  EL  1104-PAR-1B06PG-ZZZ-EL-MOD-000001.nwc  one discipline,
not built`, an NWF of it an earlier run left in a second, never deleted and never run, and RESULT counts them, `not built
C06 : 1 group not built for one discipline`, with a `one discipline :` line so the counts of done, partial and failed add up.
On the NWF page the disciplines are read off the models once the NWF opens, before the refresh, and an NWF linking a file not
on the disk is not judged.

Proved on copies under C:\NWC-PC-Test\work, build 280e2f04, after the first pass on e909a622:

| run | what was read | what it showed |
|---|---|---|
| NWC page, a parent holding 1B06PG alone, its NWF of an earlier run in the Report folder | the log, the Report folder, the NWF's SHA-256 | both NOT BUILT lines, a RESULT with the not built C06 line, no file written, the NWF 01894da6... before and after |
| NWF page, 1B06BC and 1B06PG ticked, one NWC of 1B06BC renamed for the run | the log of the window that started the run and each group's | NOT JUDGED for 1B06BC, then FAILED on the missing file with nothing saved, both NOT BUILT lines for 1B06PG in the first log, one discipline : 1 group, both NWFs the same SHA-256, one new Backup copy of 1B06PG's NWF and no NWD |

Core tests 3403 before and 3415 after, 0 failed, 0 skipped. The desktop folder 0 lines differing after every run, Navisworks'
settings put back with 0 lines differing.

The reviewer and the breaker read the change and what they found of it is fixed: no RESULT on a run of such groups only, the
count repeated in each group's own RESULT, the earlier NWF named only in a group's own log, a missing link read as one
discipline, the counts not adding up and the live line calling such a group run, the community read by a second rule, a catch
that kept nothing, and a comment on the wrong member. Asked: Q217, the Backup copy the NWF page makes of every NWF before its
open, one discipline or not. Open as rows F233-R1 to R10: the Backup and Log folder copies of such an NWF, its discipline
facts and those of a package of such groups alone, which F234 takes, a partner file whose name does not read, the open file
run of the Clash step, the federated discipline a constant, the building of an NWF of several buildings, the skip paths no
longer reached, a link Navisworks leaves out with the refresh off, and the FINDINGS sentence with nothing between two words.

## F234, the discipline workbooks rewritten

One workbook per discipline per package, one sheet, the same name, every cell Core's. Rows 1 to 3 the title, the run date and
the reference discipline, the summary of the seven checks, one blank row, then the issues table with its header frozen and
filtered, holding only what needs action, High light red, Medium light amber, Low no fill, sorted by severity, building and
file. Coordinates are ALIGNMENT's translation from the reference model, never the box gap, far over 30 m.

Proved on copies of C06 and C07, 134 NWC files, build 1597fe9b, the NWC page, run-20261019-025319, 32 groups DONE and 12 not
built for one discipline:

| workbook | issue rows read back | summary counts | the two agree |
|---|---|---|---|
| C06 AR | 10 | 10 | yes |
| C06 ST | 28 | 28 | yes |
| C06 ME | 17 | 17 | yes |
| C06 EL | 14 | 14 | yes |
| C07 AR | 10 | 10 | yes |
| C07 ST | 28 | 28 | yes |
| C07 ME | 21 | 21 | yes |
| C07 EL | 9 | 9 | yes |

ST of 1B06BC reads 34.014 m, ST on T1, AR on Internal and 34.472 m, ST on BCTANK, AR on Internal. Core tests 3415 before and
3421 after. Asked: Q218, the Action of five checks and what Files checked counts, and Q219, OK beside nought files, rows
measured from a reference on Internal, the words the message does not give, and the C02 case of scan.md 5z-zg. Open as rows
F234-R1 to R9.

The two routes the C06 and C07 run did not take were run on build a339b626, the final source. The NWC page on a parent holding
1B06PG alone, run-20261019-032430: the package's folders made and its four workbooks written, EL holding the 1B06PG row with its
file, row F233-R3. The NWF page on Report3, run-20261019-032541: 1B06PG not built, its NWF the same SHA-256, and its row in the
EL workbook off the files its result carried back, row F233-R2. Every workbook of both runs read back with its summary counts
equal to its rows. The NWF page run also showed row F234-R5, a pattern learned off the AR names of one building.

## F235, Q187 and Q195 as Bader answered them

Q187: the caps of A 150, B 100 and C 50 only on a test of more than 750 clashes, a setting, a test of 750 or fewer pictured for
every clash, the GDI and memory stops unchanged, the IMAGES line of a capped test naming its clashes and the 750. Q195: the
penetration box ticked by default on both pages, the NWF page's own box read by its run, by design unchanged.

Proved on a copy of 1B06PK, build 897b0008, run-20261019-034851:

| 1B06PK | F234 run, old caps, 1597fe9b | F235 run, 897b0008 |
|---|---|---|
| largest test | 655 clashes | 655 clashes |
| tests capped | 4 | 0 |
| pictures written | 1036 | 1748 |
| clashes left with no picture by the cap | 712 | 0 |
| penetrations moved to Reviewed | box off, none | 844 of 1748 |

Core tests 3421 before and 3424 after. Asked: Q220, the categories of the penetration lists his words do not name.

The NWF page on 1B06PK's NWF at 8442e4a6, with the readers' fixes, run-20261019-040532: its confirm said a small penetration is
moved to Reviewed, the box read YES in the group's own Navisworks, 0 moved since the NWC page had moved them, 1748 pictures, DONE.
The readers' findings: the confirm that said every status is kept, the RUN SETTINGS line with the tests step off, a garbled
message, six stale comments and a proof step of one test are fixed. The order of the pictures when a stop fires, now that every
test of 750 or fewer is uncapped, and the clashes counted toward the 750 are Q221. Rows F235-R1 to R6.

## F236, the clash count check

Every run writes "<community> clash count check.xlsx" in the community's report folder, written by the window that started the
run once every group has run, on both pages: one row per group per test with clashes in Bader's fifteen columns, a total per
group and per community, a red row and a COUNTS DIFFER line where the four counts differ, then the groups not built, the tests
the two rules left out a count a group, and the groups whose counts could not be read. Every number is read off the files the
group wrote, its .tsv, its clash workbook and its HTML page, and a count not read is UNKNOWN, never a nought. The views are held
to the rows at New and Active, Q222. Merged in 317 after three code commits, the reviewer's and the breaker's findings on the
first and their second readings on the second. Core tests 3424 before and 3451 after. Proved on copies of 1B06PK and 1B06PG on
both pages, 17 tests, 0 red. Asked: Q222, the views against the open rows, and Q223, a row with a count not read and the merged
mirrors. Rows F236-R1 to R10.

## F237, the full run on final-2026-10-19

Main 6faff29, stamp 6faff297. C06 and C07 copied from the desktop folder into final-2026-10-19\Parent, 134 NWC files, each the
same size, date and SHA-256 as its source. The NWC page with both ticked and Run pressed once, then the NWF page on the same
Report folder, each group in its own Navisworks.

| | NWC page, run-20261019-055704 | NWF page, run-20261019-062111 |
|---|---|---|
| groups DONE, partial, failed | 32, 0, 0 | 32, 0, 0 |
| groups not built for one discipline | 12 | none, they have no NWF |
| the runner | 1331.0 s, starts 135.2 s, quits 61.9 s | 1228.2 s, starts 133.4 s, quits 62.1 s |

The counts per community, read off the clash count checks, the same on both pages:

| community | groups | tests with clashes | red | UNKNOWN | Clash Detective | Excel rows | HTML rows | in views | pictures | skipped | New or Active | Reviewed |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| C06 | 15 built, 6 not | 290 | 0 | 0 | 3904 | 3904 | 3904 | 2121 | 3904 | 0 | 2121 | 1783 |
| C07 | 17 built, 6 not | 252 | 0 | 0 | 1549 | 1549 | 1549 | 986 | 1549 | 0 | 986 | 563 |

The groups not built: C06 1B06BS, 1B06KI, 1B06PG, 1B06PT, 1B06PW and 1C06PK, C07 1A07PP, 1C07BS, 1C07KI, 1C07PK, 1C07PW and
1D07PT, none with a file under Report. No test holds more than 750 clashes, the largest 655 in 1B06PK, so every clash has its
picture. The penetration rule moved 2346 clashes to Reviewed, the 1783 and the 563.

The random picks, five a community, drawn from the newest clash workbook of each group with the seed 20261019, and each found
in its NWF by tools\probes\ClashPickProbe by its two element IDs. Every NWF read the same SHA-256 after the probe as before.

| pick | group | test | clash | Element IDs | status in Excel | found in Navisworks | picture |
|---|---|---|---|---|---|---|---|
| C06-1 | 1B06WM | BLD-ST-Walls-vs-BLD-AR-Floors | Clash4 | 1528426, 1083754 | Active | once, Active | cd040004.jpg, read |
| C06-2 | 1B06BC | BLD-ST-Floors-vs-BLD-AR-Curtain Mullions | Clash13 | 2284203, 1966788 | Active | once, Active | cd000013.jpg |
| C06-3 | 1B06PK | BLD-DR-Plumbing Fixtures-vs-BLD-ST-Floors | Clash29 | 6523865, 3910368 | Active | once, Active | cd060029.jpg |
| C06-4 | 1B06PP | BLD-EL-Conduits & Conduit Fittings-vs-BLD-AR-Walls | Clash22 | 2311490, 1771696 | Reviewed | once, Reviewed | cd020022.jpg |
| C06-5 | 1B06PK | BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors | Clash91 | 6608970, 3910160 | Reviewed | once, Reviewed | cd000098.jpg, read |
| C07-1 | 1C07WM | BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Walls | Clash12 | 2124723, 1259928 | Reviewed | once, Reviewed | cd050012.jpg |
| C07-2 | 1C07G1 | BLD-ST-Framing-vs-BLD-AR-Walls | Clash14 | 2269060, 499499 | Active | once, Active | cd010014.jpg |
| C07-3 | 1C07WM | BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Framing | Clash5 | 2124781, 1529912 | Active | once, Active | cd110005.jpg |
| C07-4 | 1C07P1 | BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Walls | Clash2 | 5709507, 456237 | Reviewed | once, Reviewed | cd020002.jpg, read |
| C07-5 | 1C07G1 | BLD-ST-Walls-vs-BLD-AR-Curtain Mullions | Clash3 | 2401607, 864612 | Active | once, Active | cd120003.jpg |

Every picture is on the disk. The three read each show the clash's first item red and its second green. Active is a New clash
the NWF page's run found again. A picture is numbered by its row's place in its test, so Clash91 is cd000098.

Against claude-check-2026-10-18\clash-count-check.tsv, the run of 18 Oct on multi3: the same 290 C06 tests and 252 C07 tests on
each page, every one with the same Clash Detective count, Excel rows and HTML rows. What differs, each explained:

| what | rows | why |
|---|---|---|
| clashes in views | 79 C06 and 63 C07 a page, down by 1783 and 563 | the penetration rule, on by default since F235, Q195, set them Reviewed, and a view holds New and Active alone, Q222. On every such row the gap is the row's Reviewed exactly |
| pictures | 4 C06 rows a page, all of 1B06PK, 150 then and 211, 655, 256 and 190 now | the caps of 18 Oct, 150 for priority A, and Q187, F235, which caps only a test of more than 750 clashes |
| Reviewed | the same rows as the views | the penetration rule, as above |
| the 18 Oct file's TOTAL rows | one a group | a total and not a test, left out of the comparison |

No mirror merged: the 1536 merges of the NWC page each read that the kept test, holding a BLD-EL-Telecom Fixtures side, did not
run. The desktop folder read 0 lines differing after both runs and the probe, every file with its SHA-256, and Navisworks'
settings were put back, 0 lines differing. The logs and tsv of both pages are in steps\logs as 2026-10-19-C06-C07-final.

## Bugs found in the round, each a row of steps\tracker.csv

- F236-R1 a result group a person made reads red, Clash Detective counting its clashes and the Excel and HTML one row.
- F236-R2 the check on the window thread with no pump, its seconds counted into the run time.
- F236-R3 two packages of one community holding one building read the same.
- F236-R4 Pictures made counts the link, not the jpg on the disk.
- F236-R5 a test the views did not read whole reads nought views.
- F236-R6 Why skipped names the 750 rule where the cap per test set the limit.
- F236-R7 the tests a rule left out still held from an earlier run are in no count.
- F236-R8 a .tsv cut short read as whole.
- F236-R9 the read back holds two counts only.
- F236-R10 the NWF page's Building cell reads the NWF's name, seen on this run's sheets.
- F237-R1 the NWF page's discipline workbooks have no One discipline group row for a group never built.
- F237-R2 no mirror merge ran on C06 and C07, so the merge path is proved by its tests alone.

## F238, the end of the round

This part, the proof steps at the top of steps\03_bader_next.md, the full run's output first, and the QA of the repo read by an
audit agent on main 6faff29, after PR 317, which PR 318 changed only under steps and tools\probes. Of its 65 rows F238 fixed 15
as text, about 13 problems, the rule files, workflow.md, the page and six control bytes under steps, and four more lines the
claim-checker found beside them, core.md on the _files folder twice and Compact, and addin.md on a step that moved. 9 rows
were closed before and one is kept by Q207, and the other 40 are listed. PR 318 changed 147 files under steps\logs, 5 under
steps and 3 under tools\probes\ClashPickProbe, read by git diff 6faff29 d5f34d1. No code changed.

## The checks that pass

- Core tests: `dotnet test tests\Federator.Core.Tests\Federator.Core.Tests.csproj -c Release`, run once on the working tree,
  whose src and tests equal 6faff29: Passed 3451, Failed 0, Skipped 0, Total 3451. The logger also printed "Skipped
  WriteTheCorrectedFile", an [Explicit] test (MatrixCorrectionsTests.cs:922) that the totals do not count. 3451 is the count
  F236's entry gives.
- Named tick boxes: FederatorWindow.xaml holds 16 CheckBox elements, 12 with an x:Name (lines 171, 377, 387, 398, 409, 423,
  579, 581, 583, 585, 587, 591), ApplyFileSettings the one inside an Expander (415-431), so 11 sit under none, and the 2 with
  no name are the tree template's (55) and the Run column's (200). That is addin.md:1066-1072, twelve and eleven.
  probe-window-labels.ps1 was not run.
- Public members added by F232 to F236: the 293 added lines starting public under src in `git diff f92023f 6faff29`, each
  name counted in src with comments stripped. None is called by nothing. The only members called by nothing after the round
  are the ones in rows 38 and 56 and m13. The sweep matches names, not symbols, and is not a build.
- No Navisworks type in Core or the runner: no Autodesk name in src\Federator.Core, the runner names the Automation DLL and
  its type only as strings (Program.cs:32-33), and neither csproj references a Navisworks DLL.
- net48 and C# 7.3: every csproj under src, tests and tools reads TargetFramework net48 and LangVersion 7.3. A grep for ??=,
  is not, using var, switch expressions, ranges, records and init found none in src or tests, and Core and its tests
  compiled under 7.3 in the test run. The add-in was not built.
- Nothing names one project's file in a code line of src but the help line of row 28, PackageWords.cs:66.
- Writing: no em dash, no en dash, no emoji and no generated-by or co-authored-by line in CLAUDE.md, the eight rule files,
  README.md, INSTALL.md, docs\workflow.md, PROGRESS.md, 03_bader_next.md, 01_next.md, 02_questions.md and
  claude\audit-round-2026-10-19.md. The hits of that phrase are the rule itself, CLAUDE.md:218, and 01_next.md:1710 naming
  such lines. Semicolons outside code only in rows 35 and the old questions 02_questions.md:328 and :356.
- Control bytes: none in any text file under claude, docs or .claude at 6faff29. The six under steps are row 49.
- PROGRESS.md at 6faff29: 59 lines, at most 60. Three next steps. Five lines of blockers.
- 01_next.md: F231 to F236 each have one section and one DONE line under it, with the date, the pull request, what was proved
  and the Core test count, read at 6faff29 lines 1808 to 1836.
- tracker.csv at 6faff29: rows F231 to F236 read merged with their pull requests 312 to 317, their register rows R1 to R10
  read open or merged, every status among the seven of tracker.md:165, and the run column none on every row not proven by a
  run. Q187 and Q195 read merged 316, as their answers are on main. check-tracker.ps1 and check-progress.ps1 were not run.
- Folders: src\Federator.Core and tests\Federator.Core.Tests hold the same folders but Packs, row m20.
- core.md's counts that were read: twenty steps in RunSteps (RunSteps.cs:24-85), core.md:2793, and fifteen [Test] in
  WorkbookCellCheckTests.cs, core.md:2113.
- The round's Look for words read against the code: F236 step 2's `COUNTS   read back`, `COUNTS   DIFFER`, `COUNTS   UNKNOWN`
  and `as written, of C06` (ClashCountWorkbook.cs:228, ClashCountCheck.cs:658 and :666), F236 step 4's section name
  (ClashCountWorkbook.cs:32), F232 step 3's box (PackageWords.cs:72), the workbook's name `<community> clash count
  check.xlsx` (ClashCountWorkbook.cs:18 and :37) as core.md, workflow.md:170 and 03 give it.
- INSTALL.md:36-39 carries the start line and the FAILED:, REFUSED: and LEFT: starts of install.ps1:37-44 and
  install-common.ps1:15 word for word.
- Not run here: the solution build, the probes, the hooks' proofs, check-tracker.ps1, check-progress.ps1 and any Navisworks.

## Contradictions with CLAUDE.md and the rule files, read on main 6faff29 by the audit agent

### The 35 of 18 Oct and the morning's open ones, read again on main

| n | still stands or closed | where | how | state |
|---|---|---|---|
| 1 | stands | runner.md:59-60, src\Federator.Runner\Program.cs:98 and :118, :207, :247, :291 | runner.md says every word the runner writes is Core's. Program.cs:98 builds "loading ... from ... threw" itself, and the four Threw calls hand typed words, "the runner", "its ceiling", "its run", "telling its Navisworks to quit" | listed |
| 2 | closed | runner.md:82-83 | runner.md names core.md, A FRESH NAVISWORKS PER GROUP, for the 30 minutes | closed before F238 |
| 3 | stands | addin.md:11 against addin.md:706 | core.md holds no line naming AutoSaveKeeper or AutoSaveSwitch (grep), while both classes are Core's, src\Federator.Core\Diagnostics | listed |
| 4 | closed | src\Federator.Core\Diagnostics\AutoSaveSwitch.cs:29-30 | the comment reads "puts it back once, as the run ends" | closed before F238 |
| 5 | stands | src\Federator.Core\Diagnostics\AutoSaveKeeper.cs:351 | WindowsAutoSaveRegistry has no test. grep finds it only in AutoSaveOff.cs:45, :118 and Program.cs:106 | listed |
| 6 | stands | Program.cs:146 StillRuns, :192, :220, :254 TheOneNew, AutoSaveOff.cs:125-130 | the rules sit in the runner and the add-in with no test | listed |
| 7 | stands, wider | src\Federator.Addin\Ui\FederatorWindow.FreshStarts.cs:119-120, :138-139, :150, :178, :222-223 | typed words beside PackageRunWords. F232 added :119-120, a copy of :138-139 for the NWF page | listed |
| 8 | stands | src\Federator.Addin\Engine\ClashRunner.cs:618-626 | the CLASH line of saved tests is built in the add-in with a typed 10 | listed |
| 9 | stands | ClashRunner.cs:591 | reads `new ViewpointSettings().SetNameSeparator`, a fresh default, and so does FederationEngine.cs:2989 | listed |
| 10 | closed | src\Federator.Core\Report\PictureRetake.cs:239 | "keeps the tool's, whatever it shows" | closed before F238 |
| 11 | stands | addin.md:964 against src\Federator.Addin\Engine\ClashImages.cs:655-656 and :723 | the rule says models are matched by display name, the code matches Model.FileName through ModelFileOf | listed |
| 12 | closed | src\Federator.Core\Report\PictureLimits.cs | BoxLabel was deleted by F232 with the box | closed before F238 |
| 13 | stands | docs\workflow.md:145-146 against src\Federator.Core\Communities\PackageRunWords.cs:21-22 and FederatorWindow.xaml.cs:1044 | NothingHereIsTouched is added to the NWC page's confirm alone, RefreshConfirm.cs names no Navisworks (grep) | listed |
| 14 | closed | workflow.md | no line says the sky pictures are pending | closed before F238 |
| 15 | stands | addin.md:1178 | "runs the route of its page, its own engine, groups, discipline reports and log", while addin.md:1197-1199 says the window that started the run writes them | fixed by F238 |
| 16 | stands in addin.md, closed in the code | addin.md:1134-1135 and :1151-1152 | addin.md still says each package is handed to its own Navisworks. The comments of FederatorWindow.Packages.cs:26-27, :506, :721, :1238, :1389 now say each group | fixed by F238 in addin.md |
| 17 | closed | addin.md:1197 | "the Browse of the parent folder and Read refuse during the run" | closed before F238 |
| 18 | closed | steps\PROGRESS.md at 6faff29 | rewritten for F236, its STATE line names the merge of 317 | closed before F238 |
| 19 | closed | steps\01_next.md at 6faff29, lines 1808 to 1836 | F231 to F236 each have a section and a DONE line, and so do their rows in tracker.csv | closed before F238 |
| 20 | stands again | steps\01_next.md:1805 and :1806 at 6faff29 | a BEL byte, 0x07, sits in "claude\audit-round-2026-10-18b.md" in F230's own section, the fix that called the byte fixed | fixed by F238 |
| 21 | closed | 03_bader_next.md | F227's step reads as the IMAGES line does | closed before F238 |
| 22 | stands | 03_bader_next.md at 6faff29, F233 step 4 (line 67), step 5 (line 73), F232 step 10 (line 102), F236 step 2 (line 14) | steps.md:30-31, one action a step. Each of these asks for two to four actions | listed, and steps 1, 6, 8 and 9 of the full run's steps hold more than one action too |
| 23 | stands | src\Federator.Core\Coverage\CoverageSettings.cs:87 | CategoriesNamedPerModel is internal and read only by CoverageSettingsTests.cs:25 and :75-77 | listed |
| 24 | stands | CLAUDE.md:154-156 against build.md:12-24 | install.ps1's rule is written in both | listed |
| 25 | kept by Q207 | AutoSaveSwitch.cs, tools\loop\nw-guard.ps1 | unchanged, Q207 keeps the loop's copy | kept by Q207 |
| 26 | stands | src\Federator.Core\Clash\PenetrationSettings.cs:163 | IsDecided is read by PenetrationRuleTests.cs:799 and PropertyProbeTests.cs:438 alone | listed |
| 27 | stands | core.md:1287 and :1571 | both still put the HTML and the _files folder in the Log folder, core.md F177 puts them in the Clash report folder | fixed by F238 |
| 28 | stands in part | src\Federator.Core\Communities\PackageWords.cs:66, FederatorWindow.xaml.cs:852, :1290, :1305, :1312 | the help line names Report\C06\Infrastructure, and four label texts are typed in the add-in. The three texts of Packages.cs:384 are gone with F232's rewrite | listed |
| 29 | stands | src\Federator.Addin\Ui\FederatorWindow.xaml:449, :459, :499, comment :384-385 | "No XML means the saved tests run", "Try the file above", "No file picked.", "beside the clash XML", while F155 picks no XML and step 4 holds no file box | listed |
| 30 | stands in part | FederatorWindow.xaml:387, :398, :423, :579, :581, :583, :585, :587 | eight boxes carry IsChecked in the XAML where eleven did. The five NwfStep boxes copy NwfPageSteps' true, src\Federator.Core\Rerun\NwfPage.cs:27-31 | listed |
| 31 | stands in part | src\Federator.Addin\Engine\Penetrations.cs:425 | LookUpLevels is still a const 4. RecentNwfs.Max went with F232 | listed |
| 32 | stands, wider | steps\03_bader_next.md at 6faff29, 3267 lines | a section a round on top again, and 03:5-7 says the 18 Oct steps below still name step 5 and the picker F232 removed | listed |
| 33 | stands | core.md:3195 | "what the NWF holds after Compact", nothing compacts since F185 | fixed by F238 |
| 34 | stands | core.md:3433 and :3435 | the heading The machine readable log, F64 twice | fixed by F238 |
| 35 | stands | steps\01_next.md:969, :1248, :1271 | semicolons in prose | listed |
| m1 | stands, frozen by Q143 | tools\loop\run.ps1:1548 | says the tool writes no copy of its log | listed |
| m12 | closed in the code, stands in addin.md | see 16 | | fixed by F238 in addin.md |
| m13 | stands in part | src\Federator.Addin\Engine\SavedViewpoints.cs:80 CanBuild, :1600 MarkToolFolder, src\Federator.Addin\Engine\TestAddress.cs:59 IndexAt | CanBuild called by nothing, MarkToolFolder read as text by ToolViewFolderTests.cs:115, IndexAt called by the Q133Import probe alone | listed |
| m17 | stands | addin.md:245, core.md:2183 | "in folders by priority and team pair", against core.md F186 and F199 | listed |
| m20 | stands | src\Federator.Core\Packs | the one Core folder with no test folder, compared by `git ls-tree` | listed |
| m21 | stands | steps\PROGRESS.md:1, :28-35 at 6faff29 | no lane lines, a Speed section loop.md:22-31 does not list, and the STATE line puts "the round of F231 to F238, F236 merged in 317, F237 the full run next" between the time and "last run" | listed |
| m22 | stands, wider | CLAUDE.md:92-93, core.md:2497-2499 against core.md F233 bullet and FederatorWindow.xaml grid | core.md says SINGLE DISCIPLINE is information and nothing is blocked or unticked, F233 greys the box off and builds nothing | listed |
| m23 | stands in part | core.md:1012 | "every clash test is one discipline against another". F224 left out the 284 tests of one set code, Q209 counts 526 of one team against itself, and those across two codes of one team still run | listed |
| m24 | stands, unmeasured | addin.md:636-640, FederatorWindow.xaml:247 | only the Outputs step has a ScrollViewer. The count of three other steps is right again since F232, but Packages and the NWF page's tree are new since the three sizes were measured, and no probe was run on them | listed |

### New contradictions

| n | rule or text | what is there instead | where (file:line) | why it matters | state |
|---|---|---|---|---|
| 36 | loop.md:29, Waiting for Bader lists each question waiting | Q187 and Q195 are listed as waiting, while 02_questions.md:1083 and :1103 hold his answers and tracker.csv reads both merged, 316 | steps\PROGRESS.md:45-46 at 6faff29 | a session starting from the page asks him again what he answered | fixed by F238, the page rewritten |
| 37 | addin.md:1118-1119 and :1206, workflow.md:181, each page has one route | step 4 of the NWC page still holds Run the open file, OnRunOpenDocument, which runs in the Navisworks a person started, and workflow.md:181-182 says so in the next sentence | FederatorWindow.xaml:441-449, FederatorWindow.xaml.cs OnRunOpenDocument | the one route claim is false, and the open file run skips the fresh Navisworks of F222, row F233-R4 | listed |
| 38 | CLAUDE.md:97-98, a public member nothing in src calls is deleted | ImageOptions.OnlyFor and NwcManifest.Fact lost their only callers with F232, FederatorWindow.xaml.cs:1855 and FederatorWindow.NwfPage.cs:395 and :414 at f92023f, and are read by tests alone | src\Federator.Core\Report\ImageOptions.cs:355, src\Federator.Core\Rerun\NwcManifest.cs:52 | members made dead by this round, kept by no decision in 02_questions.md | listed |
| 39 | core.md:1924-1925 | the rule names `OtherItemsInPictures.TickLabel` and `HelpLine` as the box's label and grey line. F232 deleted both with the box | src\Federator.Core\Report\OtherItemsInPictures.cs (the diff f92023f to 6faff29 removes them) | the rule describes members and a box that are gone | fixed by F238 |
| 40 | core.md:1079-1080, no date suffix unless the weekly record tick box is on | the box is gone since F232, addin.md:764, and OutputNaming.DateTheNwd is set by nothing in src | src\Federator.Core\Naming\OutputNaming.cs:37 | the rule offers a choice the window no longer has | fixed by F238 |
| 41 | addin.md:620-623, the photo size, the cap and the five status ticks are read from ImageOptions in the constructor | no such control is in the window since F232, as addin.md:764 says | addin.md:620-623 against addin.md:764-766 | the rule file contradicts itself | fixed by F238 |
| 42 | addin.md:1206, the scanned run and the picked NWFs are gone since F232, and core.md F155, no XML is picked | rule text still describes both: "With no XML picked" and "picking an XML runs HealthCheck", "as the scanned run stops", "One picker, one file", "for the scanned run, the open file run and the picked NWFs alike", "for the scanned run and the open file run alike", "the Clash step picks an XML at run time", "the label the scanned run gives", "a name the scanned run's guard refuses" | addin.md:136, :147, :510, :612, :627-635, :834, core.md:1073, :2072, :2562, src\Federator.Core\Communities\PackagePlan.cs comment of PackagePlan "by the guard the scanned run uses" | a reader cannot tell which route the rule is about, and the next fix may build on a route that no longer exists | listed |
| 43 | addin.md:1244-1245, every rule and every word of the clash count check is Core's, and CLAUDE.md:83-85, a rule that can be tested lives in Core | the words of every not read reason and COUNTS line are typed in the window, and which group is not read and why, and whether the workbook is read, is decided there with no test | src\Federator.Addin\Ui\FederatorWindow.Packages.cs:830, :843, :855, :864, :875, :887, :892, :899, :939-955 | these reasons are written into the community workbook's section of groups not read, and no test proves them | listed |
| 44 | core.md:999-1000, every wording a label carries lives in Core, the XAML comment at FederatorWindow.xaml:97-99, and CLAUDE.md:99, one rule in one place | F232 typed new label text into the XAML and new words into the window | FederatorWindow.xaml:164 (step 2 heading), :321 (logo help), FederatorWindow.Packages.cs:550, FreshStarts.cs:119-120 a copy of :138-139 | text no test reads, and a second copy of one line | listed |
| 45 | CLAUDE.md:86-88, the date format is a setting, and CLAUDE.md:99, one rule in one place | "Run date " and yyyy-MM-dd are typed in the clash count workbook as a copy of CheckWords.RunDate | src\Federator.Core\Communities\ClashCountWorkbook.cs:53 and src\Federator.Core\Disciplines\DisciplineIssue.cs:157 | two copies of one line drift, and the format is no setting | listed |
| 46 | CLAUDE.md:95, UNKNOWN never a nought, and core.md F236, a count not read is UNKNOWN, a page that would not read and a block whose clash table was not read among them | PageTests gives nought rows for a block with no clash table, and a page of another shape gives no test at all, which ClashCountCheck reads as nought HTML rows. The workbook side reads minus one for the same case | src\Federator.Core\Report\PageTests.cs:69-76, src\Federator.Core\Communities\ClashCountCheck.cs:522 | a page the reader did not understand makes rows red with a nought instead of UNKNOWN | listed |
| 47 | addin.md:1050 and CLAUDE.md:221-222, one grey line of at most twelve words | the viewpoints box's grey line is HelpLine and DeleteLine joined, 23 words, the Rebuild box's is 30 words, the Apply box's typed in the XAML is 14 | FederatorWindow.xaml.cs:701 with src\Federator.Core\Views\ViewpointRequest.cs:40-43, FederatorWindow.NwfPage.cs:72 with src\Federator.Core\Rerun\NwfPage.cs:282, FederatorWindow.xaml:428 | three named boxes break the tick box rule, F191, F188 and older, in no earlier list | listed |
| 48 | CLAUDE.md:204-208, what removing one model costs is unknown and only a run answers it | addin.md says it was measured on 2026-09-20 and costs nothing, scan.md 5x | addin.md:233-234 | CLAUDE.md lists as unconfirmed what a rule file says is measured | listed |
| 49 | control bytes, none in a text file | 0x0F in "tools\loop\compare-document.ps1", 0x07 in "claude\audit-round-2026-10-18b.md", 0x03 in "steps\03_bader_next.md" | steps\01_next.md:1604 (0x0F), :1805 and :1806 (0x07), steps\history\log.md:2483 (0x0F), steps\tracker.csv:665 and steps\tracker.md:759 (0x03) | a backslash read as an escape when the text was written. None in claude, docs or .claude | fixed by F238, the six bytes put back as the paths they were |
| 50 | steps.md:10-11, the order at the top of 01_next.md | the Order list ends at its item 37, F82 | steps\01_next.md:11 to the end of that list | F83 to F236 are in no order line | listed |
| 51 | addin.md:551-553, the NWF page's run reads its own penetration box since F235 | workflow.md says the Clash step's Reviewed boxes are read by the NWF page's run, and 256 lines later that the NWF page reads its own | docs\workflow.md:91 against docs\workflow.md:347 and FederatorWindow.Packages.cs:1260 | the person's guide says two things about one box | fixed by F238 |
| 52 | workflow.md:28-29, no NWF folder box since F232 | the Unknown label is still explained as the NWF folder not picked yet, and the page says it is read off the rules of 18 Oct while it describes 19 Oct | docs\workflow.md:271 and :9 | a label explained by a control that is gone | fixed in part by F238, line 9, line 271 listed |
| 53 | steps.md:31-32, 03_bader_next.md names which proof comes first and why | the round's section says F238 orders them, so on main no first proof is named and the steps are in merge order, F236 first | steps\03_bader_next.md:5-7 at 6faff29 | the person cannot tell where to start, F238 still to write it | fixed by F238, its section names the full run first |
| 54 | core.md F236, the run's view setting handed in, and CLAUDE.md:99 | the window makes its own `new ViewpointSettings().ViewStatuses` and the engine makes another, so the two are one default written twice and not one setting handed through | FederatorWindow.Packages.cs:799, src\Federator.Addin\Engine\FederationEngine.cs:4562 | a view setting changed for the engine would not reach the check | listed |
| 55 | CLAUDE.md:3-7, reads a folder of NWC files and imports a clash test XML | since F208 and F232 it reads a parent folder of communities and packages, and since F155 the matrix is built in and no XML is imported | CLAUDE.md:3-7 against core.md F155 and F232 bullets | the first lines a session reads describe the tool as it was | listed |
| 56 | CLAUDE.md:97-98, older members | StatusesThisToolMaySet.AllowsAsUndo and ClientFormat.ClashColumns are read by tests alone, GapTally.Distinct by tests alone, CensusCost.Taken by nothing, while ClashStatusEditor.cs:141 says the undo is judged by AllowsAsUndo and calls WhyNotAsUndo | src\Federator.Core\Clash\StatusesThisToolMaySet.cs:57, src\Federator.Core\Report\ClientFormat.cs:44, src\Federator.Core\Report\GapTally.cs:45, src\Federator.Core\Diagnostics\CensusCost.cs:64 | in no earlier list. Found by a name sweep that strips comments, so a member whose name another identifier shares can hide from it | listed |


## Proof steps

At the top of steps\03_bader_next.md, one order for the round: the full run's output in final-2026-10-19\Report first, 13 steps,
step 1 opening Report\C06 and its AR, ST, ME and EL workbooks, then the steps of F236, F235, F234, F233 and F232, newest first.
