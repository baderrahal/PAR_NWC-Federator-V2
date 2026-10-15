# 03 Bader next

## F201 and F202, the models of 04 PK did not line up

Simple words. PROVEN means a run showed it and I saw the result. TEST means only a test showed it. UNKNOWN means nothing showed it yet. Every run was on a COPY under C:\NWC-PC-Test\work\f201. The ten NWCs there are byte for byte the ones in NM fed\NWC\C04. NM fed was not written: its listing was the same before and after every run.

### The cause, PROVEN

The tool appends each NWC the same way the Append command does, Document.TryAppendFile, in name order. Then its UNITS step set every model to Meters, with the model's own transform handed back. That one call made each model 3.28 times larger about its own origin and moved it. On 1A04PK every one of the ten files was moved, all disciplines, from 6.3 m (ST-000002) to 666.9 m (ME-000001).

- Units: the cause. The NWCs are in Feet. Setting a model to Meters reads its feet as metres.
- Shared coordinates: not the cause. The transform each NWC carries is the same in the tool's model and in the manual one.
- The order or the way of the append: not the cause. Same call, same order.

I proved it three ways. A probe appended the ten files with nothing else, and they line up. The same probe then made the tool's one call, and the models moved. The tool's own NWD and NWF moved exactly the same, to the millimetre.

### The table before the fix, the tool's NWD (main 770e1c9a) against a manual Append

Centres of each file's box in metres, E N Z, then the difference and the width of the box. AR-01 is 1104-PAR-1A04PK-ZZZ-AR-MOD-000001.nwc, and so on. An NWD keeps no units of its own for each file, so both columns say Feet. The NWF built in the same run holds all ten models in Meters, and its centres are the same as these.

    file | units tool / manual | centre in the tool model, E N Z m | centre in the manual model, E N Z m | difference m | size x tool / manual m
    AR-01.nwc | Feet / Feet | 658447.871 2745776.879 735.600 | 658349.930 2745901.121 700.691 | 162.0095 | 380.570 / 115.998
    EL-01.nwc | Feet / Feet | 658438.315 2745763.360 731.961 | 658348.137 2745897.535 700.972 | 164.6067 | 358.708 / 109.334
    FP-01.nwc | Feet / Feet | 658445.402 2745769.745 733.516 | 658349.507 2745899.103 700.681 | 164.3398 | 338.485 / 103.170
    HV-01.nwc | Feet / Feet | 658437.489 2745788.123 742.248 | 658346.756 2745904.542 703.342 | 152.6425 | 333.712 / 101.715
    ME-01.nwc | Feet / Feet | 657690.515 2746022.447 694.130 | 658346.844 2745904.405 686.409 | 666.9040 | 353.360 / 107.704
    ME-02.nwc | Feet / Feet | 658316.636 2745613.433 676.435 | 658366.443 2745894.980 682.692 | 285.9875 | 167.531 / 51.063
    ME-03.nwc | Feet / Feet | 658201.445 2746096.795 688.891 | 658382.641 2745851.157 681.462 | 305.3277 | 572.890 / 174.617
    ME-04.nwc | Feet / Feet | 658432.472 2745788.540 733.907 | 658345.566 2745904.832 700.800 | 148.9046 | 342.744 / 104.468
    ST-01.nwc | Feet / Feet | 658345.033 2745884.704 728.988 | 658350.475 2745898.479 698.710 | 33.7067 | 384.469 / 117.186
    ST-02.nwc | Feet / Feet | 658342.315 2745852.002 686.684 | 658336.831 2745850.823 683.765 | 6.3244 | 17.377 / 5.296
    largest difference 666.9040 m, 666904.0 mm

### The fix, F201, pull request 281

- The tool no longer sets any model's units or transform. The Model units box on the Outputs step is gone, because that is all it did.
- The report is still in metres. Every number is converted when the report is written, as before. The clash tolerance is still put into the document's units.
- The log has one PLACE line for each file. It reads the file's units and box centre when the file is appended, then again just before the NWD is written. It says held (1 mm or less, a setting) or MOVED with the distance.
- A model whose units are not the document's units is named, with the advice to Rebuild. An NWF built before the fix has this.
- The notice about models that are not on the same shared coordinates is still there, unchanged.

### The table after the fix, the tool's NWD (main 9e3736c1) against a manual Append, PROVEN

    file | units tool / manual | centre in the tool model, E N Z m | centre in the manual model, E N Z m | difference m | size x tool / manual m
    AR-01.nwc | Feet / Feet | 658349.930 2745901.121 700.691 | 658349.930 2745901.121 700.691 | 0.0000 | 115.998 / 115.998
    EL-01.nwc | Feet / Feet | 658348.137 2745897.535 700.972 | 658348.137 2745897.535 700.972 | 0.0000 | 109.334 / 109.334
    FP-01.nwc | Feet / Feet | 658349.507 2745899.103 700.681 | 658349.507 2745899.103 700.681 | 0.0000 | 103.170 / 103.170
    HV-01.nwc | Feet / Feet | 658346.756 2745904.542 703.342 | 658346.756 2745904.542 703.342 | 0.0000 | 101.715 / 101.715
    ME-01.nwc | Feet / Feet | 658346.844 2745904.405 686.409 | 658346.844 2745904.405 686.409 | 0.0000 | 107.704 / 107.704
    ME-02.nwc | Feet / Feet | 658366.443 2745894.980 682.692 | 658366.443 2745894.980 682.692 | 0.0000 | 51.063 / 51.063
    ME-03.nwc | Feet / Feet | 658382.641 2745851.157 681.462 | 658382.641 2745851.157 681.462 | 0.0000 | 174.617 / 174.617
    ME-04.nwc | Feet / Feet | 658345.566 2745904.832 700.800 | 658345.566 2745904.832 700.800 | 0.0000 | 104.468 / 104.468
    ST-01.nwc | Feet / Feet | 658350.475 2745898.479 698.710 | 658350.475 2745898.479 698.710 | 0.0000 | 117.186 / 117.186
    ST-02.nwc | Feet / Feet | 658336.831 2745850.823 683.765 | 658336.831 2745850.823 683.765 | 0.0000 | 5.296 / 5.296
    largest difference 0.0000 m, 0.0 mm

The NWF reads the same. The log of the run (run-20261015-141146) says all 10 files held, 0 mm, in Feet.

### What else changed because of it, PROVEN

The clash results changed. The same ten NWCs gave 10170 clashes in 187 tests before the fix and 13286 clashes in 194 tests after it. The old clashes were found on models that were too big and in the wrong place. Every clash report the tool made this way before the fix is wrong in the same way. See Q186.

### What is UNKNOWN or not proven

- Which of your existing NWFs were built with models set to Meters. The tool did it whenever the document was not already in Meters. It is UNKNOWN for each NWF until it is opened.
- A weekly run on an old NWF naming its models in Meters and asking for a Rebuild: TEST only. I did not run the NWF page on an old NWF.
- The groups other than 04 PK. Only 1A04PK was measured.
- How it looks to you in Navisworks. I compared numbers read by a probe, not the picture on the screen.
- The viewpoints were switched off in both tool runs so the runs were short. They do not move models.

### Open questions

- Q186: which groups to rebuild, all now or one by one. A Rebuild starts the clash history again.
- Q182, Q184 and Q185 from round 6, as before.
## Round 6, F198 to F200, Clash Status and the discipline folders

Simple words. PROVEN means a run showed it and I saw the result. TEST means only a test showed it. UNKNOWN means nothing showed it yet. Both runs were on a COPY of C04 under C:\NWC-PC-Test\work\f199, with main 770e1c9a installed. NM fed was not written: its listing was the same 24940 files before and after each run.

### What was built

1. F198, pull request 278. The tool folder is named Clash Status. An empty Comments folder that stands is kept and no second one is made. The folder of the old name, NWC Federator (auto), is deleted as outside the tool folder.
2. F199, pull request 279. Inside Clash Status, under A, B and C, the tests sit in Architecture, Structure, Mechanical, Electrical or Other. Over 150mm is inside Mechanical and Electrical of C only. A discipline folder is made only where a test is in it.
3. One function decides the folder, DisciplineFolderFor, in Core. It reads the code of the first side of the test. A later switch to the team that fixes it changes that function only.

### The codes, read off the pack, Q184

- AR goes to Architecture. ST goes to Structure. ME, PL, FP and FF go to Mechanical. EL goes to Electrical. That is your list.
- HV and DR go to Mechanical, and EV to Electrical. Your list does not name them. Your team line in the pack does, so I used it.
- BLD-Security Devices has no code on its first side, so it goes to Other. 57 tests of the pack are like this.
- A saved test the pack does not name goes to Other. There were none in this NWF.
- The 1830 tests of the pack go 120 to Architecture, 111 to Structure, 850 to Mechanical, 692 to Electrical and 57 to Other. A test proves it.

### Over 150 mm in C outside Mechanical and Electrical, Q185

They stay in the test's one view, in C/Architecture, C/Structure or C/Other, with its other clashes. Nothing is dropped. The log counts and names them. In this NWF that was 1 test with 1 clash, BLD-Security Devices-vs-BLD-EL-Cable Tray&Cable Tray Fittings in C/Other.

### The two runs on the NWF page, PROVEN

Before run 1 the copy held the old folder NWC Federator (auto) with 190 views, two Comments folders, and the items I planted: a viewpoint at the root, a folder "Rami folder", a third Comments folder and a folder "Comments old". I also edited 9 clashes.

Run 1, 13.2 min, DONE, log run-20261015-131241:
- Clash Status was built new: 190 views, 7 of 7 tree checks hold.
- OUTSIDE deleted 200 items, 4 top level names: folder "NWC Federator (auto)", viewpoint "Rami coordination view", folder "Rami folder", folder "Comments old".
- The Comments folders were kept with their 2 items.
- PROOF: 741 tests and 10170 results before, 741 and 10170 after.

Run 2, 10.5 min, DONE, log run-20261015-132746:
- The old Clash Status was found by its name and its marker and deleted with its 190 items, then built again with 190.
- OUTSIDE deleted 0, the root held nothing else. The Comments folders were kept with their 2 items.
- PROOF: 741 and 10170 before and after, at both deletes.

### The tree, read out of the saved NWF after run 2 by a second Navisworks

Folder, views at any depth. The same after run 1, and the same as the VIEWS TREE block of both logs.

    Clash Status 190
      A 125: Architecture 6, Structure 29, Mechanical 39, Electrical 49, Other 2
      B 32: Architecture 1, Mechanical 21, Electrical 10
      C 33: Architecture 4, Mechanical 2 (Over 150mm 1), Electrical 22 (Over 150mm 8), Other 5
    Comments 2, Comments 0, Comments 0

B has no Structure and no Other folder because no test of B is in them.

### What is NOT proven

- The NWC page run with F199. Not run. The NWC page and the NWF page call the same function, FinishTheGroup then BuildViewpoints, read in the code. The NWF page alone was run. UNKNOWN by a run.
- How the tree looks to a person in the Saved Viewpoints panel. I read it with a probe, not with my eyes. UNKNOWN.
- HV, FP and EV are on no first side of a test in this pack. Their folders are TEST only.
- A saved test not in the pack going to Other. TEST only, there was none in this NWF.
- An empty Comments folder made where none stands. TEST here, PROVEN in round 5.
- The views took 401 s in run 1 and 283 s in run 2, against 215 s on 15 Oct at 03:56. Most of run 1 was adding the views, 297 s against 95 s. Why it differs from run to run is UNKNOWN.
- The PROOF counts say nothing of statuses.

### What I changed outside the repo

- Files: work\repo (the clone I built and installed from), work\f199 (the copy), and work\run-evidence\f199-before, f199-plant, f199-r1 and f199-r2, with both run logs.
- Programs: two Navisworks runs by the launcher and six probe sessions. All closed by themselves with exit code 0.
- The add-in installed before is kept in C:\NWC-PC-Test\backup\ParsonsNwcFederator.bundle.before-F199.

### Open questions for you

- Q182: 47 tests of A and B, 1652 clashes over 150 mm, stay in their one view.
- Q184: are HV, DR and EV right? Should BLD-Security Devices be Electrical? Should a run with no XML put every test in Other?
- Q185: should the clashes over 150 mm of C in Architecture, Structure or Other go another way?

## Round 5, F190 to F196, the three new rules, the viewpoint time and the runs

Simple words. PROVEN means a run showed it and I saw the result. TEST means only a test showed it. UNKNOWN means nothing showed it yet. All runs were on a COPY of C04 under C:\NWC-PC-Test\work (r5 for the build and the refresh, r5rb for the Rebuild). Your files in NM fed were only read, to make the copy. Evidence: C:\NWC-PC-Test\work\run-evidence\f195-r1, f195-r2, f196-r4, f196-r3d, and the tool's own logs in %LOCALAPPDATA%\ParsonsNwcFederator\logs, run ids 20261015-021103, 20261015-024833, 20261015-035619 and 20261015-041406. Navisworks was started by the launcher each time and closed by it. AutoSave was put back to 0.

### The short answer on the time

The views were the time, and one call was all of it. Saved viewpoints took 3294 s of a 61 minute run. 3089 s of that was one call, the one that lists what a saved view overrides, and it gets slower with every call because the objects of the earlier calls are not collected. The tool now collects them after each view. The same refresh of the same NWF took 215 s of views and 10.5 minutes in all. PROVEN by a timed run, before and after on the same NWF.

### What each rule did, PROVEN by the first run (NWC page, 30 min, DONE)

1. Pictures. Every test is capped by its letter, A 150, B 100, C 50. 19 tests were over their cap and each has its log line. 3052 pictures were made. The memory stop at 12 GB still fired at picture 3052 and 1585 were skipped. The NWF and the NWD were written before the pictures.
2. Outside the tool folder. 115 items were deleted outside it, 10 folders the NWC files brought. An empty Comments folder was made. The count of tests and results was 741 and 10170 before and after.
3. Over 150mm. The tree read out of the NWF is the tool folder with A (125), B (32) and C (23 tests and the folder Over 150mm with 10 tests). Over 150mm is in C and nowhere else.

### The weekly refresh (NWF page, 61 min before the fix, 10.5 min after it)

Before it I put in the copy a viewpoint of mine at the root, a root folder with a viewpoint, a folder named Comments with a viewpoint, a folder named Comments old with a viewpoint, and edited 6 clashes (Reviewed with an assignee and a comment, Approved with an approver, New with an assignee and a comment).

- Deleted, by the log: the root viewpoint, the root folder and Comments old, 5 items, and the PROOF counts were equal. PROVEN
- Kept: the folder Comments with its viewpoint. The tree read out of the NWF afterwards shows it. PROVEN
- The edited Reviewed and Approved clashes kept their status. PROVEN
- The tool folder was deleted with its 190 items and made again with 190. PROVEN
- The table of 187 tests was read from the real window after 61 minutes, each test with New, Active, Reviewed, Approved and Resolved before and after. A test that was New became Active, as it should, and the Reviewed and Approved ones did not move. PROVEN

### The Rebuild in a real run (NWC page, group with an NWF, views off, DONE)

I ticked Rebuild, the confirmation came and I read it whole (it names the group, says the clash history is not carried over, and says all outside the tool folder and Comments is deleted), I answered OK and the run built a new NWF and NWD in a new folder `run 2026-10-15 0414` beside the old ones. The old NWF and NWD have the same SHA-256 as before. PROVEN

### Your question Q182, the clashes over 150 mm in A and B

47 tests of priority A or B in a pair that carries the size folder have clashes whose larger service is over 150 mm, 35 of A and 12 of B, 1652 clashes in all. By your order they stay in the test's one view, so no Over 150mm view is made for them. 10 tests of priority C got an Over 150mm view. The same numbers came in both runs. PROVEN. Whether to do something else with the 1652 is yours to say.

### What is NOT proven

- The number in the confirmation of the NWF page is the one the last run deleted, 115 here. The real number is read when the NWF opens and written in the log, because it cannot be read before the file is open. The confirmation says so.
- Two Comments folders on the root, one empty and one with a viewpoint, are both kept. That is my choice and not yet your word, and it is TEST and the run above.
- Why a first build took 1342 s of views at 02:11 and 3182 s of views in the 14 Oct run, with the same code at that call. UNKNOWN. The cause is the same call, and how soon the objects are collected differs from run to run.
- The picture caps do not keep a run under the 12 GB stop on this PC. 4637 pictures were wanted and 3052 were made. A run on the desktop PC of 15.8 GB would be the same, UNKNOWN on a PC with more memory.
- How the pages look to a person and whether Excel shows the pictures right. UNKNOWN.
- The viewpoint confirmation says the number counted at the last run. TEST.

### What I changed outside the repo

- Programs I started, all closed: eight Navisworks sessions started by my launcher, seven probe sessions that read, timed or edited the copy, and PowerShell drivers. In four of the launcher sessions my driver stopped on its own guard before it ran anything, and I closed those Navisworks sessions myself or the watchdog did, only ones I had started. The launcher put AutoSave back to 0 each time.
- Files written: work\r5, work\r5rb and work\run-evidence under C:\NWC-PC-Test, the probe in work\probe, the scripts in the session scratchpad, and the tool's own logs and `nwf-places.tsv` under %LOCALAPPDATA%\ParsonsNwcFederator.
- Your NM fed was not written. The 0 byte NWD is still there for you to delete.

### What to try on the desktop PC

1. Put your own viewpoints in a folder named Comments. Everything else outside the tool folder is deleted on a run that makes viewpoints.
2. Press Run on the NWF page for a real NWF and read the confirmation. Cancel is the default.
## Round 4, second set, F189 the final run of Part 2, Task 8

Written for Bader. Simple words. PROVEN means a run showed it and I saw the result. TEST means only a test showed it. UNKNOWN means nothing showed it yet. The runs were on a COPY of C04 under C:\NWC-PC-Test\work\final, in your own layout: NWF\C04, NWD\C04, Log, Clash report and a Backup folder. Your files in NM fed were not touched. Evidence: C:\NWC-PC-Test\evidence\2026-10-15-f189-final. Build 9eb59ad0. Navisworks was started by the launcher and closed by it each time, none was left open, and AutoSave was put back to 0.

### The three runs

1. NWC page, first build, viewpoints on. 61 minutes, DONE. NWF 11.5 MB, NWD 136 MB, Excel, HTML, 187 viewpoints in the tool folder, 3156 pictures, the memory stop at 12 GB. The manifest and the places of the NWF, its NWD and its Log folder were written. PROVEN
2. I then did what a coordinator does on the copy: a Reviewed clash with an assignee and a comment, an Approved one with an approver, a New one with an assignee and a comment, on 3 tests, and one viewpoint of mine at the root and a root folder named A holding one viewpoint of mine
3. NWF page, nothing changed, all five steps on. 68 minutes, DONE. The manifest said 10 unchanged. The backup was made first. The tool folder was found by its name and marker and deleted with its 187 items, the PROOF line said 741 tests and 10170 results before and 741 and 10170 after, and 187 viewpoints were made again. The one time clean up read the root folder A, named my viewpoint in it as not the tool's naming, and left the folder. The NWF and NWD were written before the pictures. All 6 Reviewed and Approved clashes kept their status and every assignee, approver and comment was as it was. All 10170 matched by Guid. PROVEN
4. I wrote one NWC over with another file's content, planted a jpg named like the tool's that the run does not write, and a text note in the pictures folder. NWF page again. 35 minutes, DONE. The page's list and the manifest named exactly that NWC as changed. 443 new clashes were New, 1498 moved status, and 15 tests dropped clashes with a WARNING that names each. 125 viewpoints were made again in the tool folder. The 6 Reviewed and Approved kept their status. The planted jpg and 677 leftover pictures were removed, 678 in all, and the text note was kept. PROVEN

### What I checked after the runs, PROVEN

- The NWF and NWD folders hold only the NWF and the NWD. The Log folder holds the dated Excel and Internal Excel, the txt file, the run log and its TSV, and `nwc-manifest.tsv`. The Backup folder holds one timestamped copy of the NWF for each refresh and none overwrote another
- The NWC files were not changed by the runs, size and time. Nothing was moved or renamed
- After each run I read the saved viewpoint tree out of the NWF with a probe: the folders the NWCs brought are as they were, my viewpoint at the root and my folder A with its one viewpoint are there, and the tool folder holds A, B and C with its marker comment. Nothing outside the tool folder was touched
- Statuses and comments kept, new clashes New, gone clashes Resolved and kept, none removed
- The group result was DONE each time, with the NOTICE for the model on Internal and no FAILED

### What is NOT proven by these runs

- Whether the NWF page's table shows the numbers to a person. The earlier run in the real window showed 25 rows. In these runs my reader stopped after 25 minutes, before the run ended
- Why the views took 53 and 61 minutes in the first two runs and 29 minutes in the third, where fewer clashes were open. UNKNOWN. The run is over the 45 minute target when viewpoints are on
- Whether Excel shows the new workbook right to a person
- The Rebuild in a real run. TEST only

### One finding I fixed

The NWF page used to put the NWD and the Log folder beside the NWF, which is not where your NWC page build puts them. F189 remembers where each NWF's NWD and Log folder are, in `nwf-places.tsv` under %LOCALAPPDATA%\ParsonsNwcFederator, so the NWF page writes there. PROVEN by the runs above, in your layout. For an NWF built before F189 the list has no line, and the NWF page then uses the folders beside the file until one run has written it

## Round 4, second set, F188 the two pages, Task 9

Written for Bader. Simple words. PROVEN means a run in the real window showed it. TEST means only a test showed it. UNKNOWN means nothing showed it yet. The runs were on a COPY of C04 under C:\NWC-PC-Test\work. Evidence: C:\NWC-PC-Test\evidence\2026-10-14-f188-pages.

### What the window is now

1. A tab control. The first page is NWC and the second, next to it, is NWF
2. NWC page: the folder, the groups, the output folders, the tolerance drop down and the one Run button. It is for the first build of a group. A group whose NWF already exists is not ticked and cannot be ticked. A Rebuild box on the Grouping tab lets it be built again, after a confirmation that says it destroys the clash history. A rebuild goes into a new dated folder beside the old NWF and NWD, so nothing of yours is overwritten. PROVEN for the first part: with the NWF of the group already there, the window said "No group is ticked to run". TEST for the rest, the Rebuild box and the dated folder
3. NWF page: the NWF picker, a list of the NWFs last used, the list of NWC files changed since the last run, the tolerance drop down, the five steps Refresh, Run tests, Pictures, Viewpoints and Reports each with a tick box, a Run button, and after the run a table per test of New, Active, Reviewed, Approved and Resolved before > after. The folder grouping, the matrix pack and the search set build are not on this page. A step that needs another is greyed with the reason. PROVEN: the page opened in the real window, a picked NWF was run from it, the confirmation came up and was answered, and the table showed 25 tests that moved
4. The confirmation before the refresh lists the changed files, the number of tests, the viewpoint folder with its number of viewpoints that is deleted and made again, that a viewpoint moved by hand into it is deleted, and the number of old jpg files. Cancel is the default. A number that is not known says UNKNOWN. The NWF is copied into the Backup folder by the run before it opens anything. PROVEN that it comes up and OK starts the run. TEST for its words. The words of this run are in the log under CONFIRM from the next run on
5. The NWFs last used are kept in `recent-nwfs.txt` in `%LOCALAPPDATA%\ParsonsNwcFederator`, ten at most. TEST. The file is a thing I wrote outside the repo only when the window is used
6. Kept: the pack choice by project code, the tolerance drop down only, the Run as label, the Log and Clash report folders, the capped pictures and the stops. TEST, and PROVEN for the stops by the runs of F182 and F187

### What the NWF page run showed

- The picked NWF was a copy whose HV file I wrote over. The page listed it as changed, the run was DONE, all 6 Reviewed or Approved clashes kept their status, 10613 results were matched by Guid and 7 clashes were new. PROVEN
- The run order of F182 held on this page: the NWF and NWD before the pictures, then the pictures, which stopped at the memory limit. PROVEN
- The close on load option is still not reachable from the add-in. The NWF page says to set it by hand, Options, Model, Performance, in its grey line. PROVEN that the line is on the page

### UNKNOWN

- How the NWF page looks to a person. I read it with UI Automation and could not see it. The look is yours to judge, the layout is plain
- The Rebuild confirmation and the dated folder in a real run. Only tests read them
- The count of viewpoints in the confirmation until a run has written it, since it is kept in the manifest by the run

### Numbers

Core tests 3196 after, 0 failed. The Release build has 0 warnings and 0 errors.

## Round 4, second set, F187 the refresh and the overwrite, Task 7

Written for Bader. Simple words. PROVEN means a run showed it and I saw the result. TEST means only a test showed it. UNKNOWN means nothing showed it yet. The runs were on a COPY of C04 under C:\NWC-PC-Test\work. Your files in NM fed were not changed. Evidence: C:\NWC-PC-Test\evidence\2026-10-14-f187-refresh.

### What was built, on the route of a picked NWF, which is the NWF page's run

1. Before anything opens, the NWF is copied into a Backup folder beside the Log folder, named with the moment, never over an older copy. PROVEN: three runs made three copies, none overwrote another
2. The linked files are listened for while the NWF opens. A file Navisworks cannot find is named and the run stops with nothing saved. No model is removed. PROVEN by a run with one NWC renamed away: FileInteractiveResolving fired, the file was named, the group FAILED with that reason, and the NWF on disk was byte for byte as it was
3. The manifest. Each linked NWC: path, size, last write time, and a SHA-256 only where the size or the time differs from the last manifest. It is `nwc-manifest.tsv`, one fixed name in the Log folder beside the NWF's folder, overwritten by the tool on each run. A file written again with the same content is called touched, not changed. PROVEN: with nothing changed it said 10 unchanged, and with one file written over it listed exactly that file as changed
4. Document.UpdateFiles is asked once, then TryCloseFileHandles. It returned False in 0.0 s each time. A changed file had been read when the NWF opened, so a False is read as nothing left out of date, a NOTICE and never a failure. What it returns for a file written after the NWF is open is UNKNOWN
5. Every clash result is read before the tests run again in place and after: status, assigned to, comments, approver, group. Then a table per test of New, Active, Reviewed, Approved and Resolved before and after, and a WARNING with the test name where a test dropped clashes. The tests are run in place with TestsRunTest. Nothing is reset, compacted, removed or replaced, and a test says so
6. Tests the NWF lacks are added by TestsAddCopy only, and a tolerance is changed only by an edit from a copy. Same as F185

### What the runs showed

- Kept. I made a coordinator's edits in Clash Detective's own calls on 3 tests of the copy: a Reviewed clash with an assignee and a comment, an Approved one with an approver, a New one with an assignee and a comment. After the refresh all 6 Reviewed or Approved kept their status, and every assignee, approver and comment was as it was. All 10170 results were matched by Guid. PROVEN
- Changed NWC. I wrote one NWC over with another file's content. 443 new clashes were New and 1498 moved status, the ones that were gone from their test Resolved, 0 removed, so gone clashes become Resolved. PROVEN
- Units and origin. Document units and every model's origin were identical before and after the changed file was read. PROVEN for this case
- A New clash that is found again becomes Active, Navisworks's own rule. The table counts it and does not list it as a thing a coordinator wrote
- A refresh with a changed file of the same size and time but other content would be called touched only if the hash is the same, so it is called changed. TEST
- The run order, the memory stop, the 750 caps and F177's leftover removal apply on this route, since it runs the same group steps. The memory stop fired at picture 3331 in two of the runs. PROVEN

### Close NWC/NWD files on load

The option is `LcOpPerformanceProps.GetCloseFileAfterLoad` and `SetCloseFileAfterLoad`. No public member of the managed API returns that object, and `Application.Options` holds only Grids. So the add-in cannot read or set it. Whether the COM API reaches it is UNKNOWN. The NWF page tells the user to set it by hand, Options, Model, Performance. TryCloseFileHandles, which the tool calls after the refresh, releases the handles for that session, PROVEN to return True

### Things you should know

1. The route's Log folder and NWD folder are the open file's own, since F158: the Log folder beside the NWF's folder, and the NWD written beside the NWF, replacing the NWD there. A group built by the NWC page with the NWF in NWF\C04 and the NWD in NWD\C04 keeps its Log in NM fed\Log, so the first refresh of that NWF finds no manifest and writes one in NM fed\NWF\Log. F188 has to make the NWF page use the group's own Log, Backup and NWD folders
2. The build of a group still appends file by file with TryAppendFile, so a bad file is named one by one. Your order named AppendFiles, which gives one call and no name for the file that failed. I kept the one by one. Say if you want AppendFiles
3. Search sets only: the matrix builds search sets and no selection set is made, F155 and F168

### UNKNOWN

- What UpdateFiles returns for a file written after the NWF is open
- Whether the close-on-load option can be reached from the add-in
- Whether an NWF opened on another PC finds its NWC at the same path
- The refresh with viewpoints on. These runs had the viewpoints off to be short, so the folder rebuild of F186 is not shown on the route yet. The final run does it

### Numbers

Core tests 3176, 0 failed. The Release build has 0 warnings and 0 errors.

## Round 4, second set, F186 the viewpoint tree, Task 6

Written for Bader. Simple words. PROVEN means a run showed it. TEST means only a test showed it. UNKNOWN means nothing showed it yet.

### What is built

1. One top folder, NWC Federator (auto), at the root of the saved viewpoints, with the comment NWCF-AUTO run=<stamp>. Under it A, B, C by the priority of the test. A test with no priority goes in C. Then one view for each test, directly, named as the test. There is no team pair level and no Over 150mm folder any more. Every open clash of a test is in its one view, whatever its size. The team pair still decides which models a view shows. TEST
2. On each run the tool finds the top folder by its name AND its marker and deletes it with everything in it. Then it builds it again. A folder with that name and no marker is left as a coordinator's. The inventory and the seven checks are given only what is inside the top folder, so nothing outside it is read for removal, changed or deleted. TEST, by the source and by the rule that picks the nodes
3. The one time clean up. A root folder named A, B, C or No priority is deleted with all it holds only if every item inside is the old tree's own naming: a team pair folder of two team names, the size folder, a view named as a test or as the first version's per clash viewpoint. One item that is not leaves the whole folder and the log names the folder and that item. A root folder with nothing in it is left. The log gives the counts: root folders read, deleted, left. TEST
4. The NWF page text, that a viewpoint moved by hand into the tool folder is deleted, is written with the NWF page, F188. The tool folder rule is in place now
5. Does deleting saved viewpoints change the clash results. The log now has a PROOF line that reads the tests and results before and after the delete. A test checks its words. On a real NWF it is UNKNOWN until a run, and the PROOF line then says it. It compares counts, not statuses. Statuses are compared by the snapshot of the weekly run, F187

### What old NWFs hold

Still UNKNOWN. The clean up reads them when it runs and says what it found, folder by folder.

### Numbers

Core tests 3146 before and 3159 after, 0 failed. The Release build has 0 warnings and 0 errors.

## Round 4, second set, Part 2 begins, F185 the checks before anything is built

Written for Bader. Simple words. I read the code of main after PR 264 again and did not rely on last round's notes. PROVEN means code or a test showed it. UNKNOWN means nothing showed it yet.

### 1. How the tool applies the tolerance to a test today

- A test already in the NWF: `ApplyChosenTolerance` in ClashRunner. It reads the test's tolerance. If it is already at the chosen value it does nothing. Otherwise it takes `CreateCopy()` of that test, sets the tolerance on the copy, and calls `TestsEditTestFromCopy(test, copy)`. It never removes, replaces or re-adds the test, so clash results and statuses are kept. The log line says so and cites the measurement 5y. PROVEN by the code and by a test that reads the source, ClashHistorySourceTests
- The file's settings route, `Apply`, behind the tick box Apply the file's settings, off by default, also uses `TestsEditTestFromCopy`, with a new test object holding the file's two sides. It does not touch results by itself, measured 5y. The help text and one log line under it said it resets the clashes. I changed both to say the results and statuses are kept. Whether a test whose two search sets change drops clashes is as the research says, UNKNOWN until Part 2 tests it on a copy
- F76 is this same method

### 2. How tests the NWF lacks are added, and what else could lose history

- A test the NWF lacks is added by `TestsAddCopy`, one call, in ClashRunner. Nothing else calls it. A test saved in the NWF that the matrix does not name is run where it sits and is not added or changed
- The one call that lost history was `TestsCompactAllTests`, behind the tick box Delete Resolved clashes from the NWF, off by default. I REMOVED it, with the tick box and its setting, as you ordered. A Resolved clash now stays and keeps counting. The log says so
- There is no `Reset`, no `TestsRemove` and no `TestsReplaceWithCopy` of a clash test anywhere in the add-in. A test now fails the build of main if one is added, ClashHistorySourceTests
- Still there, and not a clash test: `Document.TryRemoveFile` in the route that rebuilds a changed group, which takes a MODEL out of the tree. Task 7 says never to delete and re-append a model, so that route is Task 7's to change

### 3. How the viewpoint tree is built today

- Root folders by priority: A, B, C, or a folder named by the setting for no priority. Under it a folder for each team pair. Under that, an Over 150mm folder where the pair carries it. Then one view for each clash test, named by the test. Clashes of a test with no open clash get no view
- A test with a side on a team the map does not name gets no view
- Every view and folder carries a mark comment. The tag is NWC-FEDERATOR-VIEW v1, then the run stamp, the folders, the name and the camera. A view the tool made and a person did not touch is the tool's. A moved, renamed, turned or commented view is a person's and is kept
- What old NWFs hold: root folders A, B, C and the no priority folder with team pair folders in them, each with that mark. UNKNOWN until one is opened, nothing in this session read an old NWF

### 4. Does the weekly run refresh the NWC files

No. The weekly run opens the existing NWF with `TryOpenFile`, and Navisworks reloads the linked files when it opens. It never calls `Document.UpdateFiles`. A changed NWC with the same name and place comes through as Navisworks reloads. PROVEN by a search of the add-in's source for UpdateFiles, which finds none. Task 7 adds the one explicit call

### What changed in code

- The Compact resolved option is removed, F185, and the tests that read the source fail on any removing or replacing call
- Core tests 3143 before and 3146 after, 0 failed. The Release build has 0 warnings and 0 errors

## Round 4, second set, Part 1 built and the run on C04 that ENDED

Written for Bader. Simple words. PROVEN means the run showed it and I saw the result. TEST means only a test showed it. UNKNOWN means nothing showed it yet.

### The short version

Part 1 is built and merged, F181 to F183. The full run on C04 was made with that build, 25 mm, viewpoints on. It ended. No crash and no hang. The NWF and the NWD were written first, the NWD is 137 MB and not 0 bytes, and the pictures stopped by themselves at the 12 GB memory limit. The RESULT block, the txt files, the TSV and the run log copy were all written. The run took 50 minutes, and the target is 45. Part 2 is started because this run ended clean.

### What was done, one pull request each

- F181, PR 261, Task 1. A test with more than 750 clashes gets pictures for its first 150 at priority A, 100 at B, 50 at C. 750 is one named constant. The label of the box, the rules and the tests say 750
- F182, PR 262, Task 2. The picture step reads Navisworks's private bytes after each picture. Past 12 GB, one named constant, it stops, says the picture number, the memory and the skipped count, and goes on. The order of a group is now: tests, viewpoints, NWF saved, NWD published, NWF confirmed, then the pictures, then the Excel, HTML and XML, the coverage and the internal Excel. ORDER lines in the log say which step ended last
- F183, PR 263, Task 3. Q175 C and Q176 750 are in steps\02_questions.md and the tracker. The Part 2 notes now name F186 to F188
- This report is F184, Task 4

### What the checks of Task 2 found

1. Steps that need the pictures: the Excel, the HTML and the XML, because they carry the jpg names and links. So they stay after the pictures. The coverage and the internal Excel read the Excel back, so they follow it
2. The viewpoints need nothing from the picture step. They read the rows and where each row sits. So they moved before the NWD
3. The NWF is NOT saved again after the pictures. The picture step writes nothing into the document and puts the hiding back. Whether TestsImageForResult leaves the open document marked as changed is UNKNOWN, nothing read it
4. One step keeps its place and says why. With Compact resolved ticked, the pictures are taken inside the clash step as before, because a compact moves the index a picture is found by. The log says so. It is off by default
5. F177 is kept. A stop is a normal end of the picture step. This run's stop removed the leftovers, and a step that throws removes none

### The run on C04, PROVEN by the run

Evidence folder, outside the repo: C:\NWC-PC-Test\evidence\2026-10-14-run-c04-r5. Build 0947159a, started 18:26, ended 19:17, 1 group 1A04PK, 10 NWC files, 741 tests created, 10170 clashes.

1. THE ORDER. NWF saved at 19:12:33, 12.4 MB. NWD published at 19:12:47 in 13.8 s at 2.6 GB of private memory, 137,275,619 bytes. The NWF was confirmed whole. Only then did the pictures start at 19:12:47. The ORDER line says both files are written before the pictures
2. THE MEMORY STOP. Private memory rose from 2.6 GB to 12.0 GB, about 3.1 MB for each picture. The stop fired at picture 3043 with 12.0 GB. The GDI count was about 4100 then, so the GDI stop at 9000 did not fire. 3042 pictures were written in 218 s, 0.07 s each. The NOTICE says 4264 pictures were skipped, and the log has it with the picture number and the memory
3. THE 750 RULE. BLD-ST-Floors-vs-BLD-AR-Walls, priority A, has 800 clashes. It got 150 pictures and 650 were skipped, and the log line says A. That test has 750 to 1000 clashes, so the new trigger is shown by the run. BLD-AR-Walls-vs-BLD-AR-Floors, A, 1556 clashes, got 150. BLD-ST-Floors-vs-BLD-ST-Columns, A, 958 clashes, got 150
4. THE FOLDERS. The NWF folder holds only the NWF, the NWD folder holds only the NWD. The Log folder got 5 new files: the Excel, the internal Excel, the txt of models off the coordinates, the run log copy and the TSV, 4702 rows. The two older Excel files in Log did not change. The Clash report folder holds the HTML and its _files folder
5. THE OVERWRITE. This was the second run into the same Clash report folder. Before: 7904 cd jpgs. After: 3042 overwritten, 4862 leftover removed, 0 left that this run did not write. The log line says removed 4862. logo.jpg did not change. The NM fed listing before and after shows every missing file is one of those 4862 jpgs, and every changed file is one of the 3042 jpgs or the HTML. No other file was touched. AutoSave listing is the same before and after, and the switch is back to 0
6. THE LINKS. Every picture link in the HTML resolves, 6086 of 6086. In the Excel every linked picture resolves, 3042 of 3042, and every click and cell link, 6084 of 6084
7. THE NOTICE. A model on Internal, ST-MOD-000002, gave the NOTICE in the run log and the group line, and 8 models are not on the shared coordinates. No FAILED. Groups done 1, partial 0, failed 0. The coverage compared 741 and 741 agree
8. Windows wrote no Application error for Roamer.exe in the last 2 hours. Navisworks was closed through the launcher and read gone with exit code 0. The texture cache folder is empty after

### PROVEN ONLY BY A TEST

- The priority B and C caps at 800 clashes, and a priority A test of exactly 750 giving 750. Only A met the rule in this run. PictureLimitsTests
- That the first of the two stops ends the pass, with the GDI stop firing first. The run's memory stop fired first
- That a picture step which throws leaves the leftover pictures alone. The run did not throw
- That the NWD step is before any picture in the engine's own code. A test reads the source for it

### UNKNOWN

- Why the views took 43 minutes, 2586 s, where the last run took 25. They ran at about 25 s a view and then about 75 s a view. Nothing in the log says why. The total of 50 minutes is over the 45 minute target
- Whether the NWD still publishes quickly if a run holds many more clashes before the pictures. Here it was at 2.6 GB
- Whether Excel shows the pictures when a person opens the new workbook, and whether its click link that goes up one folder opens the jpg. Only a person can click
- An older Excel in Log, the one of 16:15, links 7904 jpgs and 4862 of them are now removed. Its links to them no longer resolve. The code and the order say this
- Whether TestsImageForResult marks the document changed, as above

### What I started and what I wrote outside the repo

Started, and each one has ended: dotnet build and dotnet test, git and gh, build\install.ps1, reg.exe for two exports, one launcher powershell, one driver powershell, one Navisworks Manage 2025 Roamer.exe pid 29424 that I closed through the launcher. Your own Navisworks was not open and I touched none.

Written outside the repo:
- the install, %APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle
- the registry value enable of GlobalOptions\general\autosave, written to "3 0" and put back to "0", read back as 0
- in NM fed: NWF\C04\run 2026-10-14 1826 with the NWF, NWD\C04\run 2026-10-14 1826 with the NWD, five files in Log, the HTML and 3042 jpgs overwritten in Clash report, and 4862 leftover jpgs removed from it, by your order
- %LOCALAPPDATA%\ParsonsNwcFederator\logs\run-20261014-182641.log and .tsv, made by the tool
- the evidence folder above, and scratch files under the session scratchpad
- Windows itself changed the Navisworks counters in the registry
- I did not delete the 0 byte NWD in NWD\C04\run 2026-10-14 1615. It is still there for you

### Next step

Part 2 follows, F185 to F189, in the order of your message.

## Round 4 of 14 Oct, Part 1 built, and the run on C04 that did NOT end

Written for Bader. Simple words. PROVEN means the run or a test showed it and I saw the result. UNKNOWN means nothing showed it yet.

### The short version

Part 1 is built, tested and merged, F175 to F179. The full run on C04 was made with that build, 25 mm, viewpoints on. It did not end. The pictures were stopped safely at the 9000 GDI limit, so the old crash at picture 8907 did not come back. But Navisworks's memory grew to 26 GB, and at the NWD step it stopped and did nothing for 8 minutes. The NWD file is 0 bytes. No RESULT block, no run log copy and no txt files were written. I closed that Navisworks at 17:02, a program I started. It was not a Windows crash, it was a hang. Because the run did not end, I did NOT start Part 2, as you ordered. No retry was made.

### What was done, one pull request each

- F175, PR 255, Task 1. A test with more than 1000 clashes gets pictures for its first 150 at priority A, 100 at B, 50 at C, per test. A saved test the matrix does not name is C. A test with no priority read is C too. The log line per capped test now says the letter. The viewpoints are not touched. The box and the 9000 stop stay, the least holds. The label of the box and the docs say the new numbers. After each picture the GDI count is read again, and the one Bitmap the picture step gets is disposed in a finally on every path. I read every use of Bitmap and Image in the add-in. Only one picture call returns a Bitmap, so that was the only one to check
- F176, PR 256, Task 2. A model that names no shared site is a NOTICE, the same as a model on Internal. It no longer makes the group FAILED and the clash always runs. The old failure code was dead after that, so I removed it with its tests
- F177, PR 257, Task 3. In the Clash report folder the HTML and its _files folder keep the plain name and are overwritten. The Log folder keeps its dated names. After the pictures, a cd and digits jpg in that group's _files folder that this run did not write is removed, top level only, and one log line says how many. A failed run, a run that did not reach the end of its picture step, or one that wrote no picture removes nothing. An older Excel in Log points at jpgs a later run overwrites, so it then shows the new pictures. This is said in the code and in the docs
- F178, PR 258, Task 4. Your answers are in steps\02_questions.md and the tracker. Q172 was not in that file, so I wrote it there with your yes
- F179, PR 259, Task 6. Your research is in steps\notes\nwc-nwf-design.md in your words
- This report is F180, Task 5

### The run on C04, what happened, PROVEN by the run

Evidence folder, outside the repo: C:\NWC-PC-Test\evidence\2026-10-14-run-c04-r4. Build 8e647af8, started 16:15, 1 group 1A04PK, 10 NWC files, 1830 tests from the matrix plus the tests saved in the NWF.

1. The pictures. One test went over 1000 clashes: BLD-AR-Walls-vs-BLD-AR-Floors, priority A, 1556 clashes. It got 150 pictures and 1406 were skipped, and the log line names the letter A. Every other test had 1000 clashes or less and got all its pictures
2. The 9000 stop fired at picture 7905, with 9001 GDI objects. 7904 pictures were written. 2266 clashes that wanted a picture have none. Navisworks never came near 10000 GDI objects. No GDI crash
3. WHERE THE GDI OBJECT IS MADE. The log's GDI split lines read +1 GDI object for each picture inside Navisworks's own TestsImageForResult and +0 in our save and dispose of the bitmap. So our code does not leak it. Navisworks's call does
4. THE MEMORY. Navisworks's private memory went from 0.8 GB at the start to 17 GB at 16:25 and 25.8 GB at 16:35, about 3.3 MB for each picture, and it never came back down. The texture cache folder is %TEMP%\ogs, not a folder named ogsN\N_TextureCache. It held 3.1 MB in 124 files before and after. It did not grow, and I did not delete it. No other folder named ogs was found under %TEMP% or %LOCALAPPDATA%
5. THE HANG. At 16:54:59 the NWD step began with 26.1 GB private memory. The NWD file was made at 0 bytes and stayed so. For 8 minutes Navisworks used 0 CPU seconds and wrote nothing to disk, and its main thread was waiting. Windows reported 53.4 GB of its 55.2 GB commit limit in use and 3.9 GB of RAM free. Earlier runs published the NWD in about 1 s. I closed that Navisworks with Stop-Process at 17:02:39
6. The steps before it did work. NWF saved, 187 views written, 292 viewpoints and 33 folders in the tree, workbook 17 s, HTML 2 s, Internal workbook 0.3 s. The views took 25 minutes. The run took 47 minutes up to the hang, and the target is 45
7. The NWF folder holds only the NWF, 11.6 MB, in a new dated folder `NWF\C04\run 2026-10-14 1615`. The NWD folder holds only a 0 byte NWD in `NWD\C04\run 2026-10-14 1615`. Your NWF in NWF\C04 and the earlier `run 2026-10-14 1052` folders are untouched
8. The Log folder holds the Excel and the Internal Excel and nothing else. There are no txt files, no TSV and no run log copy, because the run never reached the end. The tool's own run log is `%LOCALAPPDATA%\ParsonsNwcFederator\logs\run-20261014-161532.log` and the .tsv beside it
9. The Clash report folder is new, and holds the HTML and its _files folder, 7904 pictures and the logo, 329 MB
10. Links. Every picture link in the HTML resolves, 7905 of 7905. In the Excel every linked picture, click link and cell link resolves, 7904 of each. The click links go up one folder to Clash report. Whether Excel itself follows a link that goes up a folder is still UNKNOWN, only a person can click one
11. A model on Internal, ST-MOD-000002, gave the NOTICE in the log with its name and the words that its clash results may be wrong. The log holds no failure reason for where the models sit
12. Nothing of yours changed. The listing of NM fed before has 21885 files, and after it has 29795. The 7910 new files are the ones named above. None of the old 21885 changed in size or time
13. AutoSave: the AutoSave folder listing is the same before and after, 61 files, in 47 minutes. The switch is put back to its old value 0

### PROVEN ONLY BY A TEST, not by this run

- The overwrite and the removal of leftover pictures. The Clash report folder was new, so the log says removed 0. LeftoverPicturesTests proves the two runs, the overwrite, the removed leftover and the kept files. A failed run removes nothing
- A model that names no shared site as a NOTICE. C04 has no such model. Its NOTICE and the group staying out of FAILED are proved by AlignmentCheckTests and GroupJudgementTests
- The priority letters B and C, and a saved test the matrix does not name as C. Only A met the rule in this run
- Core tests 3124 pass, 0 fail. The Release build has 0 warnings and 0 errors. Actions was green on each PR

### UNKNOWN

- Why the NWD step stopped. The memory and the commit limit point at it, but nothing proved it. I took no memory dump, it would be 26 GB. A run that does the NWD before the pictures would answer it
- What Windows wrote at 16:29:24, a NonCritical Roamer.exe report in ReportArchive. It is from the minute the 9000 stop fired. I could not read its file, access was refused
- The group result line, the txt file of models off the coordinates, the RESULT block and the TSV rows of the end, since the run never wrote them
- Whether the Excel opens with the pictures visible. Only a person can see it

### What I propose, you decide, Q175 in steps\02_questions.md

The 9000 GDI stop is not enough. Memory is the next wall, 3.3 MB for each picture, and 7904 pictures used 26 GB on this PC with 15.8 GB of RAM. Options:
- A. Stop the pictures at a memory limit too, as the GDI stop does, for example when Navisworks's private memory passes 12 GB, about 3500 pictures. Smallest change
- B. Do the NWD and the NWF save before the pictures. The viewpoints come first, the pictures after, and the HTML and Excel last. The NWD would be published with low memory. A bigger change to the order
- C. Both A and B
- D. Take the pictures in several Navisworks sessions. The biggest change, and it keeps all the pictures

### What I started and what I wrote outside the repo

Started, and each one has ended: dotnet build and dotnet test, git and gh, powershell scripts for edits and checks, build\install.ps1, reg.exe for two exports, one launcher powershell, one driver powershell, one Navisworks Manage 2025 Roamer.exe pid 20612. I closed that Roamer with Stop-Process after the hang. Your own Navisworks was not open and I touched none.

Written outside the repo:
- the install, `%APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle`
- the registry value enable of GlobalOptions\general\autosave under Navisworks Manage 22.0, written to "3 0" and put back to "0", read back as 0
- in NM fed, new: `NWF\C04\run 2026-10-14 1615\` with the NWF, `NWD\C04\run 2026-10-14 1615\` with a 0 byte NWD that you can delete, `Log\` the two Excel files, `Clash report\` the HTML and 7904 jpgs and the logo
- `%LOCALAPPDATA%\ParsonsNwcFederator\logs\run-20261014-161532.log` and .tsv, made by the tool
- the evidence folder above, and scratch files under the session scratchpad and %TEMP%
- Windows itself wrote one WER report and the Navisworks counters in the registry

### Read in advance for Part 2, reading only, nothing built

While the run went I read the code for Task 7. No code was changed for it.
1. Tolerance: ApplyChosenTolerance already copies the test with CreateCopy, sets the tolerance on the copy and calls TestsEditTestFromCopy. A test already at the value is not touched. Nothing removes, replaces or copy-adds a test with results. The log text says results are kept, measured 5y
2. Tests the NWF lacks are added by TestsAddCopy only. One call is on your list to remove: TestsCompactAllTests, behind the Compact resolved tick box, off by default. There is no Reset, no TestsRemove and no TestsReplaceWithCopy of a clash test. The one remove is Document.TryRemoveFile in the Rebuilt route, which takes a model out of the tree
3. The viewpoint tree today is priority folder A, B, C or No priority, then the team pair folder, then Over 150mm where the pair has it, then one view for each test. Every view and folder carries a mark comment, the tag NWC-FEDERATOR-VIEW v1 with the run stamp, the folders and the name. What old NWFs hold is UNKNOWN until one is read in Navisworks
4. The weekly run opens the NWF and lets Navisworks reload the files. It never calls Document.UpdateFiles
5. The API has Document.UpdateFiles, DocumentSavedViewpoints.Remove and AddComment, Document.TryCloseFileHandles, and in its interop part LcOpPerformanceProps.SetCloseFileAfterLoad, which looks like the option Close NWC/NWD files on load. Whether it is reachable from the add-in is UNKNOWN

### Next step

Answer Q175. Then F181 to F185 follow, with the picture and memory rule first.

## Round 3 of 14 Oct, F170 to F173, and the run on C04 that was NOT done

Written for Bader. Simple words. PROVEN means a test or a run I saw showed it. UNKNOWN means nothing showed it yet.

### The short version

The four fixes are built, tested and merged. The full run on C04 was not made. Your own Navisworks was open the whole time (pid 23696, started 13:47 from Explorer, the 1A02MM NWD open, 811 MB). I did not close it and I did not start a second one beside it. The PC has 15.8 GB of RAM with 2.8 GB free, and the last C04 run grew to 28 GB. A second Navisworks could have frozen or killed yours with work in it. The repo's own loop refuses to run beside a Navisworks for the same reason. For the same reason I did not run `build\install.ps1`, which would replace the add-in files under a running Navisworks.

### What was done

- F170, PR 250, Q169. The HTML and its `NAME_files` folder of pictures go in a new folder `Clash report` beside `Log`. `Log` keeps the run log, the TSV, the shared coordinates txt, the Excel and the XML. The Excel linked pictures point at the jpgs in `Clash report`. The click link, the Image cell link and the XML href go up one folder, for example `../Clash%20report/NAME_files/cd000001.jpg`. A name already in `Log` or in `Clash report` gets a dated name, nothing is overwritten
- F171, PR 251, Q170. A model on Internal no longer ends a group FAILED. A NOTICE names the model and says its clash results may be wrong, in the run log, in the shared coordinates txt, in the RESULT lines and in the group result line. A model that names no shared site at all still fails its group, it is another reason and you did not say to change it
- F172, PR 252, Q171. A test with more than 1000 clashes gets pictures for its first 100 and the rest are skipped, per test. One log line per such test says its name, its clashes, the pictures made and the skipped. A backstop reads the GDI objects of Navisworks before every picture. Past 9000 no picture is made, a NOTICE says the picture number and the count, and the run goes on to the NWD, the HTML, the Excel and the viewpoints. The box `At most per test` is kept. It does another job, a cap of your own for every test, and the least of the box and the fixed rule holds. Its label now says that a test over 1000 clashes gets 100
- F173, PR 253, F163. The loop and the window driver no longer name the Excel folder box and the skip box. The stand-in window ticks MarkByDesign for the untick proof. Seven probes that start Navisworks did not switch Auto-Save off, and now do. The leftovers of the skip option are gone from the docs and from `build\team\INSTALL-TEAM.txt`, which still told the team to untick it. `steps\PROGRESS.md` is refreshed and matches the tracker

### PROVEN

1. Core tests 3111 pass, 0 fail. The Release build has 0 warnings and 0 errors. CI passed on each PR
2. With the Excel in `Log` and the jpgs in `Clash report`, every linked picture, every click link, every cell link, every XML href and every picture link of the HTML resolves to a jpg that exists. LogFolderLinksTests writes the files and follows each link, and the HTML is written through the installed Navisworks stylesheet
3. A test of 1500 clashes gets 100 pictures. A test of 1000 or less gets all of them. Two tests of 1200 get 200 in total. The box and the rule give the least of the two (PictureLimitsTests)
4. In a simulated pass of 16000 clashes in small tests the backstop fires at picture 8502 at 9001 GDI objects, never nears 10000, and the rest are left empty (PictureLimitsTests). This is a test of our rule, not of Navisworks
5. A group with a model on Internal gets no failure reason and is judged DONE where its files and viewpoints are there, and the NOTICE names the model. A model naming no site still fails the group (AlignmentCheckTests, GroupJudgementTests)
6. The window driver takes no `-Excel`, and `DriverArguments` of the loop gives none. The touched scripts parse and the stand-in builds

### UNKNOWN

- Whether a real run on C04 with this build ends without a crash. The backstop reads `GetGuiResources` of the Navisworks process. That it rises by one per picture was measured on the run of 11:10, and it is not measured with this build
- Whether the Excel itself follows a click link that goes up a folder, `../Clash%20report/...`. Only a person can click one
- Whether the real Navisworks window still takes the typing of the driver. The driver was changed and not run against Navisworks
- Whether `tools\loop\prove-run.ps1` passes. It refuses to start beside a Navisworks, and it did so at 14:21. Its window cases are UNKNOWN after F173
- Why Navisworks's private memory reached 28 GB on C04. F161 added the GDI split line that says where the GDI object is made, and no run has read it yet
- `Clash report` and `Clash Report` are one folder on Windows. NM fed already has a `Clash Report` of yours. The tool writes into it and gives a dated name where a name is taken. It never deletes or replaces. If you want them apart, say so and the folder gets another name

### What to do for the run

1. Close your Navisworks, or tell me it may be used
2. `build\install.ps1` from VS Code, then the run on `C:\Users\bader\Desktop\NM fed\NWC\C04` with tolerance 25 mm and viewpoints on
3. Then look in `NM fed\Log` for the txt files, the TSV, the run log and the Excel, and in `NM fed\Clash report` for the HTML and its `_files` folder. Look in the run log for the lines `IMAGES   ... has N clashes, more than 1000`, a `NOTICE   the pictures stopped at picture` if it fires, and the `GDI split` line

### What I started and what I wrote outside the repo

- Started: `dotnet build` and `dotnet test` many times, with their build servers. `git` and `gh` for the branches and PRs. Powershell scripts for edits and checks. `tools\loop\prove-run.ps1` once, which refused at once and did nothing. No Navisworks, no Roamer, no add-in install
- Outside the repo I wrote: scratch scripts and notes under `%TEMP%` and under `C:\Users\bader\AppData\Local\Temp\claude\...`, the `TestResults` folders under the test project (ignored by git), and the evidence copy of the earlier run at `C:\NWC-PC-Test\evidence\2026-10-14-run-c04`. I wrote nothing in `C:\Users\bader\Desktop\NM fed`, nothing in `%APPDATA%` and nothing in the registry
- I closed what I opened. Your Navisworks I did not open and did not touch

## Your six answers of 14 Oct, built as F164 to F169

Written for Bader. Simple words. Nothing here was run in Navisworks. What each does:

- F164, Q166 A. The label reads Weekly run plus matrix
- F165, Q165. ReadPicked and cut-xml.ps1 stay. The mirror check is recorded in `steps\01_next.md` as the next clash test round and is not built
- F166, Q164 B. The pack is chosen by part 1 of the NWC file names. A code with no pack stops the run before anything is opened and says "No matrix pack for project code XXXX. Send the clash matrix and a session will add it." Two packs with one code, and a group or a run with two codes, stop too
- F167, Q167 A. The tolerance comes only from the drop down. Nothing reads a tolerance or units from a matrix or an XML any more
- F168, Q168 A. A test saved in the NWF that the matrix does not name runs too, where it sits, after the matrix tests, and is in the HTML, the Excel and the Coverage sheet. Its viewpoints are under priority C unless the priority map names it
- F169, Q163 A. Each picture in the Excel has a click link to its jpg, relative, forward slashed, no stray %20

What to try, one action each:
1. Run a building with a hand made test saved in its NWF and look for it in the Excel and under priority C in the views
2. In the Excel, click one picture. It should open its jpg. Since F170 the jpgs are in `NAME_files` in the `Clash report` folder beside `Log`, so the link goes up one folder, `../Clash%20report/NAME_files/cdNNNNNN.jpg`. Whether Excel follows a relative link is UNKNOWN until you click one
3. Copy the NWC files of a building and rename part 1 of the names to another code. The run should stop at once with the words above

## The round of 2026-10-14, F157 to F161, and the run on C04

Written for Bader. Simple words. What the evidence proves is marked PROVEN. What is not known is marked UNKNOWN.

### What was done

- F157, PR 239. The picture step now writes a MEMORY line with GDI and USER object counts at the start, every 250 pictures and on every failure. It writes a RATE line every 250. Every IMAGE failure has a time, the clash, its test, the full path and the whole exception with its call trace. The log already flushed every line to disk
- F158, PR 240. Logs and reports go in one folder called Log beside the NWF and NWD folders. The Excel folder box is gone. Nothing in Log is overwritten or deleted, a name already there gets a date. Question 169 asks if the place is right
- F159, PR 241. The option "Skip clash when models sit off shared coordinates" is gone. The clash always runs. A NOTICE in the log and in the list file names the models and says the results may be wrong for them. Question 170 asks about the group ending FAILED
- F161, PR 242. Two fixes after the run below, the GDI split and the file name warning
- F160 is this report

### The run on C:\Users\bader\Desktop\NM fed\NWC\C04

Started 10:53 with the build 6c47bef4, tolerance 25 mm, viewpoints on, 1 group 1A04PK, 10 NWC files. It crashed at 11:10:26 in the picture step, 17 minutes in. The NWF was saved in a new folder, `NWF\C04\run 2026-10-14 1052`, because an NWF was already in `NWF\C04`. The NWD was not made. No HTML and no Excel was made.

### PROVEN by the evidence

1. The crash is in Navisworks. Windows Application log, Error, 11:10:26: Roamer.exe 22.5.1433.58, faulting module lcodwin.dll 22.5.1433.58, exception code 0xc0000005, fault offset 0x0000000000045344, Report Id 95c3d2cf-2b87-4e23-b1b0-e85a5f0b8115
2. GDI objects of Navisworks go up by exactly 1 for every picture. 1092 at the start, 3092 at picture 2000, 9092 at picture 8000, 9999 at picture 8907. The pictures in the _files folder are 8907
3. A process may hold 10,000 GDI objects. The first image failure came at 9999. The error is in Navisworks's own TestsImageForResult. Windows wrote GDIObjectLeak reports at 11:08:52 to 11:09:02, when the count passed about 9,000. The crash came 5 seconds after the failures began
4. The earlier crash at 02:34 is the same. Windows wrote GDIObjectLeak at 02:33:02 and 02:33:26. The log at 02:33:26 reads picture 7931, and 7931 plus about 1,100 is about 9,000. It is the same count
5. Clash35 and Clash36 naming one file was not two clashes on one name. A failed picture gives its number back, so the next clash gets the same file name for a file that was never written. In this run 10 failed clashes got cd350007.jpg in a row. The warning of F157 said wrongly that a picture would be replaced. F161 fixes that
6. Task C works. 8 models were off the coordinates. The clash tests were made and run, the NWF was saved, viewpoints were asked for, and the log says NOTICE. Nothing was skipped
7. Task B works for what ran. Log was made beside the NWF and NWD folders, and the 8907 pictures are in `Log\..._files`. The NWF folder holds the NWF alone. The log said `RUN logs and reports go in C:\Users\bader\Desktop\NM fed\Log`
8. A model on Internal, ST-MOD-000002, still made the group FAILED, as the old rule with the option off did

### UNKNOWN

- Whether the GDI object is made inside Navisworks's TestsImageForResult or in our save and dispose of the bitmap. F161 adds a GDI line that answers it on the next run
- Why Navisworks's private memory grew to 28 GB, about 2.6 MB for each picture, on a PC with 15.8 GB of RAM. It did not cause this crash as far as the evidence shows, the commit limit is 35.8 GB
- Whether the HTML, the Excel and the copy of the run log in Log work end to end. The run never reached them. The Core test LogFolderLinksTests proves the links on written files. The copy of the log into Log is proven by LogCopyTests only
- Whether the viewpoint step works after the pictures, it was not reached

### Evidence, kept outside the repo in C:\NWC-PC-Test\evidence\2026-10-14-run-c04

launcher.txt, life.txt (GDI, memory and CPU every 15 s), driver.txt, run-20261014-105308.log and .tsv, the two registry exports, the AutoSave lists. The crash dump is `C:\Users\bader\AppData\Local\CrashDumps\Roamer.exe.25436.dmp`, 71 MB. Windows report: `C:\ProgramData\Microsoft\Windows\WER\ReportArchive\AppCrash_Roamer.exe_1fdd64d565bd9f268c788b26c9e96a57183d58c_6d739cb3_b2ad21cc-952c-4f17-9823-294aa494f776`. RAM 15.8 GB, free 8.5 GB after the crash. Disk C: 98.8 GB free. Page file 20 GB, peak 3 GB.

Last 5 rows of the .tsv are the PICTURES `pictures models` rows of 11:10:06 to 11:10:21, the last for `BLD-PL-Pipe Accessories-vs-BLD-PL-Pipes & Pipe Fittings`. The last lines of the log are the IMAGE failures of Clash7 to Clash10 at 11:10:21, each with GDI objects 9999.

### What I started and what I wrote outside the repo

Started and closed: build\install.ps1 through powershell.exe, reg.exe twice, one powershell.exe launcher, one powershell.exe driver, one Navisworks Roamer.exe pid 25436, closed by Dispose, exit code 0. dotnet build, dotnet test, git and gh in the repo. The dotnet build servers were shut down at the end.

Written outside the repo:
- `%APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle`, the install
- `NM fed\NWF\C04\run 2026-10-14 1052\` with the NWF, 4,697 bytes
- `NM fed\NWD\C04\run 2026-10-14 1052\`, empty
- `NM fed\Log\1104-PAR-1A04PK-ZZZ-BM-MOD-000001_files\`, 8,907 jpg files, 335 MB
- the tool's own run-20261014-105308.log and .tsv and folders.txt in `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
- Windows: the crash dump and the report folder above
- Navisworks's own HKCU settings, the counters and the recent file list, 20 lines, see the two .reg exports
- evidence folder above, and the scratch files of this session under `%TEMP%\claude`

Nothing of yours was overwritten. All 12,983 files that were under NM fed before are unchanged.

Two things were removed, and not by me:
- the tool's keep 30 rule removed `run-20260920-220804.log` and `.tsv` at the start of the run. I put both back from the loop's logs-backup, the sha256 is equal
- Navisworks's own AutoSave rotation removed three old files, `...Autosave738.nwf` to `Autosave740.nwf`, and wrote three new, `Autosave1479` to `1481`. No copy of the three exists. The loop switches AutoSave off before a run and my launcher did not. That was a gap of mine

### Next step

Do not run the whole building again until the pictures are fixed, it will crash again at about picture 8900. My suggestion for the fix, F162, needs your answer: stop the picture step when GDI objects pass 9,000, say so, and let the run go on to the NWD, the HTML and the Excel with the rest of the pictures left empty. That makes a run finish. It does not give you all the pictures. Giving all of them needs one of these, none tried yet: take the pictures in several Navisworks sessions, or find out if the save of the bitmap is the leak, which the GDI line will say, or raise the Windows limit `GDIProcessHandleQuota`, which needs admin and a restart and is not for 27 people.

Run steps, one action each:
1. Install main at 64cb73d or later with `build\install.ps1`
2. In the tool, on 3. Outputs under More, set "At most per test, 0 for no cap" to 10. That keeps the run well under 8,900 pictures
3. Run C04 to its end. It should reach the NWD, the HTML and the Excel
4. Read the lines that start `GDI      PICTURES` in the log and send them with the log

### Left over

- `tools\loop` and `tools\probes\drive-window-run.ps1` still name the Excel folder box and the skip box. The harness will stop on them. Not changed in this round, it needs its own fix
- Q169, the place of the Log folder. Q170, the group with a model on Internal ends FAILED. Both are in `steps\02_questions.md`

**SINCE F155, 2026-10-14, THE CLASH XML BOX IS GONE.** There is no Clash XML box, no Browse beside it, no Teams line under it, and no entry `Use the value in the XML` or `Other` in the tolerance drop down. The clash matrix is built into the tool. Earlier steps in this file that use them, steps 422 to 504, 587, 597, 1045 to 1060, 1133 to 1146 and 1222, cannot be followed as written on a build with F155. Skip them. The steps from 1400 on are written for the build with F155.

**The last run, 2026-09-20 16:20, was the worksets round proving run against HIS OWN C02 folders, log `run-20260920-162006.log`, and it reads 8 groups DONE and 2 FAILED. The two are 1A02MM and 1A02WL, failed on purpose under Q70 because a structural model in each was exported on Revit`s internal origin, and BOTH still wrote their NWF, their NWD and their report. THE WORKSETS ROUND IS NOT ON MAIN YET: it is the branch `round-worksets` and has to be merged before a build here carries any of what these steps describe. THE ADD-IN BUILDS, 0 errors and 0 warnings.**

One action per step. Do them in order. One build, one install and one Navisworks session cover every proof: the window checks first, then the C06 rebuild run, then one building twice, then the rest.

F136 comes first of all, steps 400 to 416 right after the install, because that box is what switches the viewpoints off. Since F114 merged it opens TICKED, your answer B to Q131, and every run in this file makes the views of F114, one per clash test of its open clashes in the A, B, C folders by team pair, unless the loop unticks it through run.ps1 -Untick MakeViewpoints. Every step below that expected the old per clash viewpoints is rewritten to the new tree, and the F114 proof is steps 507 to 541. F131 is second, steps 417 to 458, your order of 2026-10-05 in Q132, because the team map it reads at the pick, in every group and for a run with no XML is what F114's views read next, and it costs two short runs of the same fixture and two runs of the open file on the NWF they saved, with the box ticked. Then F34, because it is the largest add-in change of the round, the window, the XAML, the engine and the job all changed, and it is proved by opening the window and looking, before any run. If the window does not open or the Clash step is wrong, nothing after it can be read, so it is checked before a single group runs. F33 is checked in the same look, because it is one combo on the same window. F27 is next, because it costs one Scan and no run at all: its block is read off the 1B06PH run further down. Then the C06 run, which proves F24, F29, F30 and F33 in one press, then F28 and F31 together, because both are one building run twice. F32 and F35 need a different file open or a folder made for them, so they come after the ordinary runs.

## THIS FILE NOW COVERS TWO FOLDERS, AND THE OLD ONE NEVER EXISTED

Everything below is done once on `C:\00-NM\Federation Task\C02 + 04\C02`, which holds TEN
groups and already holds NWF files so its groups are Weekly runs, and once on
`C:\00-NM\Federation Task\C02 + 04\C04`, which holds TWELVE groups and NO NWF at all so
every group there is a FIRST RUN.

THE `C06` FOLDER AND THE `1B06*` GROUP NAMES ARE GONE FROM THIS FILE. They were never on
his machine, and every count in here derived from fourteen groups was wrong for that
reason. The real groups are:

    C02, ten:    1000BS  1A0215  1A02BS  1A02MM  1A02MS
                 1A02WE  1A02WL  1A02WM  1A02WN  1A02WO
    C04, twelve: 1A0415  1A04EP  1A04KI  1A04MS  1A04PK  1A04PW
                 1A04WE  1A04WL  1A04WM  1A04WN  1A04WO  1WAW15

FOUR THINGS ABOUT C04 BEFORE YOU READ ANY NUMBER OFF IT. `1A0415` and `1WAW15` are NOT
buildings: they carry LS, LT and SW, and every set in the matrix is a `BLD-` set asking
for one of nine codes that does not include any of those three, so both find NOTHING and
that is correct rather than a fault. `1A04MS` and `1A04PK` hold ONE discipline each, both
ST, so there is no pair to clash. `1A04WL`'s ST files are numbered 1, 2, 3, 4, 5 and 7,
and SIX is missing, which is named and not guessed at. And `1A04WL`'s ST-000004 and
ST-000005 are on Revit's internal origin, which fails that group on purpose.

AND THE FIXTURES COME FIRST NOW. Before either live folder, run the two small copy groups
under `C:\Users\bader\AppData\Local\Temp\claude\round-close`, which take about twenty
seconds each and exercise sets, tests, clashes, by design, the workbook, the page and the
penetration rule, and the views of F114, since the viewpoints box opens ticked now.
`tools\probes\fixture.md` says what each one is, why the
penetration one is AR and ME rather than ST, and what neither can show. A live run is the
final proof and never the loop.

## Get the code

1. Open GitHub Desktop
2. Pick the repo PAR_NWC-Federator-V2
3. Make sure the current branch is main
4. Press Fetch origin
5. Press Pull origin if it appears
6. Open the repo folder in VS Code

## Build and install

7. In VS Code open a terminal
8. Build:

```
dotnet build ParsonsNwcFederator.sln -c Release
```

9. If the build says a Navisworks DLL is missing, run with the install path:

```
dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"
```

10. Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`. That was proved on this machine on 2026-09-19 by F69, which is the first build this repo has ever had, so anything else here is new and the whole output goes into the chat. `CreateCopy` and `CopyFrom` on `DocumentSelectionSets`, which F24 and F29 could not measure, are measured now and written into `docs\history\scan.md` 4d, so a build naming either of them is a real fault and no longer an open question
11. Look for: the build stamp. If it reads `nogit` where an eight character commit hash belongs, git is not on the PATH for the terminal that ran the build. The build itself is fine and only the stamp is affected, and the title bar and the log will read `nogit` until you build from a terminal that has git on it

### If the build fails on NuGet rather than on the code

This is the failure Bader hit on 2026-09-19 before he ever reached a compiler error, and it has nothing to do with the code:

```
C:\Program Files\dotnet\sdk\10.0.400\NuGet.targets(198,5): error Cannot create a file when that file already exists.
```

`dotnet build` restores all three projects at once, they collide on the same package folder, and it is a known race in NuGet's restore task. The four steps below go cheapest first.

12. Run the same build command again, exactly as it was:

```
dotnet build ParsonsNwcFederator.sln -c Release
```

13. Look for: the second build finishes with no error. The race is intermittent, so repeating the build is the first thing to try and it costs nothing
14. If the same error comes back, delete every `obj` and `bin` folder, because a half written package folder is what the race leaves behind:

```
Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory | Remove-Item -Recurse -Force
```

15. Look for: the command prints nothing. Run `Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory` on its own afterwards and look for: it prints nothing either. There are six of these folders when they are all there, two per project
16. Restore on its own, one project at a time, which is what takes the race out:

```
dotnet restore ParsonsNwcFederator.sln --disable-parallel
```

17. Look for: three `Restored` lines and no error, one for `Federator.Core`, one for `Federator.Core.Tests` and one for `Federator.Addin`. If the `obj` folders were not deleted it says `All projects are up-to-date for restore` instead, which is also fine
18. Build without restoring again, so nothing can collide:

```
dotnet build ParsonsNwcFederator.sln -c Release --no-restore
```

19. Look for: the build finishes with no error. If it says a package or a reference assembly is missing, the restore at step 16 did not finish and that is the step to read again
20. Only if every step above failed, clear the NuGet cache. It is LAST because it re-downloads every package this solution uses, and nuget.org is a hard requirement for building at all:

```
dotnet nuget locals all --clear
```

21. Look for: it finishes with no error. Exactly what it prints was not measured here, because clearing the cache on this machine would cost the re-download it warns about. Then go back to step 8 and build again, which will pull every package down fresh

22. Install:

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

23. Read the last lines of the install. Look for: a line saying all the expected files are present and a line saying every reference is satisfied. If it stops with Install incomplete, copy the whole output into the chat

## Proof F136 FIRST, the viewpoints box, added 2026-10-05

THESE STEPS COME BEFORE EVERY OTHER PROOF IN THIS FILE, right after the install. Your word
of 2026-10-05 was that every test run has viewpoints switched off until F114, the new
viewpoints, merged, and this box is how they are switched off. Your answer B to Q131 is
that it opened UNTICKED until F114 merged and TICKED once it did, and F114 has merged, so
these steps read the ticked state and the views it makes, and one run through the loop
with the box unticked, run.ps1 -Untick MakeViewpoints, reads the switch still stopping
them. They are numbered 400 to 416 because the numbers below were already taken.
The fixture is the one step 393 names, measured there on 2026-09-21. Whether it is still
on your machine is UNKNOWN. If it is not, use one small group of C02 instead and read the
same lines.

400. Close Navisworks if it is open
401. Open Navisworks Manage 2025 with nothing open
402. Open the add-in from the ribbon
403. Look for: the title bar carries the commit and the time of the build you made at step 8
404. Go to the `4. Clash` step
405. Look for: a tick box reading `Make saved viewpoints for the clashes`, TICKED, just under `Skip clash when models sit off shared coordinates`, with the grey line `Each viewpoint adds time, so a big run can take hours`
406. Untick it
407. Close the add-in window
408. Open the add-in from the ribbon again
409. Look for: the box is TICKED again. The window is made new at every open and sets the box off the setting each time, so an untick lasts one window and no longer
410. Fill the four folders for the fixture, on `1. Source` the folder `<temp>\round-close\fixture\NWC`, and on `3. Outputs` the folders `<temp>\round-close\fixture\NWF`, `<temp>\round-close\fixture\NWD` and `<temp>\round-close\fixture\Report`, then press Scan
411. On `4. Clash` pick the clash XML, 1104-PAR_CLASH_AllInOne, and look for: `Make saved viewpoints for the clashes` is TICKED. Leave it so
412. Press Run and answer the dialog with OK
413. Look for, in the lines under RUN SETTINGS before the GROUPS block: `viewpoints       : yes, saved viewpoints are made for the clashes`
414. Look for, in the group: the `VIEWS`, `VIEWS BUILT`, `VIEWS INVENTORY` and `VIEWS TREE` blocks, steps 509 to 515 say what each holds, and NO line `VIEWS    the box Make saved viewpoints for the clashes was unticked`
415. Look for, in the RESULT block straight under `groups failed  :`: NO `viewpoints     :` line, which is written only where the box was unticked
416. THE UNTICKED PATH, the loop's: run the same fixture through `tools\loop\run.ps1` with `-Untick MakeViewpoints` and look for `viewpoints       : no, the box was unticked, so no group makes a viewpoint` under RUN SETTINGS, `VIEWS    the box Make saved viewpoints for the clashes was unticked, so no viewpoint is made` in the group with NO `VIEWS BUILT` block, and `viewpoints     : none made, the box was unticked for this run` under `groups failed  :` in RESULT, then send both logs named on their `log file` lines

## Proof F131 SECOND, the team map, added 2026-10-05

THESE STEPS COME RIGHT AFTER F136 AND BEFORE EVERY OTHER PROOF, because your order of
2026-10-05, Q132, is F136, then F131, then F132, then F114, and F114's views read the team
map these steps prove. They prove your answers of 2026-10-05: Q115 A, the map in a file of its
own beside the XML, Q116 A, the team written beside the code, Q117 C, and A where the set tree
names no team, and Q123 B, the window keeps the last map used for a run with no XML. They use
the same fixture as steps 410 to 416, three models of 1A04WE, AR, ME and ST. THE VIEWPOINTS
BOX OPENS TICKED since F114 merged, your answer B to Q131, so each run below writes the
views of F114 as well, steps 509 to 515. What the
silent miss count of the ME model reads is UNKNOWN here, because the fixture's worksets were
not read for this, so the steps name the shape of its line and not its number. No line
starting `SILENT MISS` can appear, because the count that confirms one is the count of the
categories no set catches, which F127's COVERAGE block does not take in this build.

417. Close Navisworks if it is open
418. Open Navisworks Manage 2025 with nothing open
419. Open the add-in from the ribbon
420. Look for, in the log pane, a block headed `TEAMS KEPT` just under `FOLDERS REMEMBERED`, holding the one line `TEAMS    no clash XML was picked and no team map is kept from a run with one. Every discipline code is a team of its own and no pair carries the size folder`. This is the first build that keeps a map, so none is kept yet
421. Go to the `4. Clash` step
422. Look for: under the Clash XML box, a grey line reading `Teams: no XML picked, no map kept yet`
423. Look for: `Make saved viewpoints for the clashes` is TICKED. Leave it so for every run below
424. Press Browse beside the Clash XML box and pick `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` in the repo folder. Its map, `1104-PAR_CLASH_AllInOne_25mm_FIXED.teams.txt`, is beside it there
425. Look for: the grey line under the box now reads `Teams: 4 read from the map beside the XML`
426. Look for, in the log pane after the `PICK` line: `TEAMS    the teams are read from` with the full path of that teams file, then `, the team map beside this file. It holds 4 teams and 10 codes, and a code on no line is a team of its own named by its code`
427. Look for, under it: `TEAMS    Architecture is AR`, `TEAMS    Structure is ST`, `TEAMS    Mechanical is HV, PL, FP, ME, DR and FF`, `TEAMS    Electrical is EL and EV`, `TEAMS    a pair is written in the order Architecture, Structure, Mechanical, Electrical, then any other team by its name, then UNKNOWN` and `TEAMS    a pair holding Mechanical or Electrical carries the size folder`
428. Look for, next: `TEAMS    BLD-Security Devices carries no discipline code the map lists, so its team is Electrical, which a folder above it in the clash XML's set tree names, unless a model of its group carries a code its name holds`, and no other set named on a TEAMS line. That is your answer C to Q117
429. Look for: every `TEAMS` line of the pick stands BEFORE the first `MATRIX` line
430. Fill the four folders for the fixture as at step 410, then press Scan
431. Press Run and answer the dialog with OK
432. Look for, after `RUN      it holds`: the same TEAMS lines as steps 426 to 428, then `TEAMS    this map is now the one kept for a run with no clash XML, remembered in` with the full path of `team-map.txt` in the add-in's logs folder, and only then the `MATRIX` lines
433. Look for, in the group, straight after the `EXPORT CHECK 1A04WE` block, a block headed `TEAMS 1A04WE` with one line for each of the three models, the code first, then the file, then the code with its team: `AR in Architecture`, `ME in Mechanical` and `ST in Structure`
434. Look for: the AR line and the ST line end `no set of its team with another code was judged against it`, and the ME line ends with a count of the sets of its team with another code, either `none of the 16 set(s) of its team with another code is kept out of it by a workset or a file name it asks` or `N of the 16 set(s) of its team with another code cannot reach it`. The 16 are the sets of the picked XML of Mechanical whose code is not ME, FF 6, PL 5 and DR 5, counted off the XML
435. Look for: no line in that block starts `SILENT MISS`. Where the ME line counts sets that cannot reach it, the block holds a line ending `is UNKNOWN until the coverage counts them`
436. Look for, in the same group: the `VIEWS TREE` block's `team map   :` line naming the teams file beside the XML and its `models     :` line carrying each model's team, the same teams the TEAMS block gave
437. Clear the Clash XML box
438. Look for: the grey line under the box now reads `Teams: no XML picked, 4 read from the kept map`
439. Press Run and answer the dialog with OK
440. Look for, after `RUN      nothing picked in the Clash step`: `TEAMS    no clash XML was picked, so the teams are read from` with the full path of the same teams file, then `, the team map kept from the last run with one. It holds 4 teams and 10 codes`, then the four team lines, and no `MATRIX` line
441. Look for, in the group's `TEAMS 1A04WE` block: the three model lines with their teams again, each ending `whether a set of its team with another code cannot reach it is UNKNOWN, because no set was handed in`, and the line `no set was judged against the models of its team, because no set was handed in`, because a run with no XML reads no set of an XML
442. Close the add-in window
443. Open the add-in from the ribbon again
444. Look for: the `TEAMS KEPT` block now reads `TEAMS    no clash XML was picked, so the teams are read from` the same teams file, `the team map kept from the last run with one`, and the grey line under the empty Clash XML box reads `Teams: no XML picked, 4 read from the kept map`
445. Send the log file named on the `log file` line of the SESSION block, the one holding both runs

## Proof F131 on the open file, added 2026-10-07

THESE STEPS FOLLOW STEP 445 IN THE SAME NAVISWORKS SESSION. The open file run, the Run the
open file button on `4. Clash`, reads the team map by the same rule as Run, and steps 417 to
445 never press it. These steps were read off the add-in at 0e262d3, OnRunOpenDocument in the
window and RunOpenDocument in the engine. With an XML picked it names the map beside it after
its `OPEN     clash file` line and keeps that map. With none it names the kept map after its
`OPEN     no XML picked` line. Its one group is named by the NWF's name without `.nwf`, so its
TEAMS block is headed `TEAMS` and that name. What that name is was not read here, because it
is what the run of step 439 saved, so the steps name the folder and not the file. THE
VIEWPOINTS BOX STAYS UNTICKED, your answer B to Q131.

446. In Navisworks open the newest NWF in `<temp>\round-close\fixture\NWF`, the one the run of step 439 saved
447. Open the add-in from the ribbon
448. Go to the `4. Clash` step
449. Look for: `Make saved viewpoints for the clashes` is TICKED. Leave it so
450. Press Browse beside the Clash XML box and pick `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` in the repo folder
451. Press Run the open file and wait for it to finish
452. Look for, in the log pane: `OPEN     clash file` with the full path of that XML, then the same TEAMS lines as steps 426 to 428, then `TEAMS    this map is now the one kept for a run with no clash XML, remembered in` with the full path of `team-map.txt` in the add-in's logs folder, and only then the `MATRIX` lines
453. Look for, in the group, straight after its `EXPORT CHECK` block, a block headed `TEAMS` and the NWF's name without `.nwf`, holding the three model lines of steps 433 and 434
454. Clear the Clash XML box
455. Press Run the open file and wait for it to finish
456. Look for: `OPEN     no XML picked, so the tests saved in the document run, or nothing runs when it holds none`, then `TEAMS    no clash XML was picked, so the teams are read from` with the full path of the same teams file, then `, the team map kept from the last run with one. It holds 4 teams and 10 codes`, then the four team lines, and no `MATRIX` line
457. Look for, in the group's TEAMS block: the three model lines, each ending `whether a set of its team with another code cannot reach it is UNKNOWN, because no set was handed in`, and the line `no set was judged against the models of its team, because no set was handed in`
458. Send the log file named on the `log file` line of the SESSION block, the one holding both open file runs

## Proof F132 the mirrored tests, added 2026-10-07

THESE STEPS FOLLOW STEP 458 IN THE SAME NAVISWORKS SESSION, your order of 2026-10-05, Q132,
F136, then F131, then F132, then F114, under your answers B to Q121, D to Q133, A to Q136, A to
Q137, B to Q138, A to Q141 and A to Q142. They were read off the add-in at 8031c4c, attempt 2
of its add-in half on 2026-10-08, ClashRunner.Run with PairTheMirrors, MakeTheRenames and
MergeTheMirrors, and the views reading the merged report, and nothing of it has run on
Navisworks yet. They use the same fixture as steps 410 to 416. The corrected matrix
holds 59 pairs of tests whose two sets carry the same rule lists and no two tests with the
same two sets swapped, so the pairs the MIRROR block counts are among those 59, and how many
of them have both tests created, F77 creating no test whose side finds nothing, is UNKNOWN
until the run says it. THE VIEWPOINTS BOX STAYS UNTICKED, your answer B to Q131. The by design
box is ticked for the one run of step 477 alone, which writes Reviewed into the fixture's NWF,
the question Q144 asks you.

459. Delete the NWF the run of step 439 saved in `<temp>\round-close\fixture\NWF`, so the next run is a first run over a new NWF, the one case a pair merges under your answer A to Q142
460. In the add-in press Browse beside the Clash XML box and pick `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` in the repo folder
461. Press Run and answer the dialog with OK
462. Look for, in the group between the `CLASH    the document holds` line and the `CLASH    the document already holds` line: a block headed `MIRROR` and the NWF's name, whose first line reads `MIRROR   N pairs of tests that ask the same question among the M tests whose two sets were read, 0 with the same two sets swapped and N whose sets carry the same rule lists. Both tests of each pair are run. K mirrors are to be merged by the pair of items into the one kept once both tests of the pair run`, with N, M and K as the run reads them
463. Look for, in the same block: one line per pair reading the kept test `is kept,` then the mirror `is its mirror, its sets carry the same rule lists as` the kept test's, ending `created and run as` the mirror's own name with ` (mirror)` at the end, never the kept test's name
464. Look for, after it: a block headed `MIRROR COVERAGE` and the NWF's name, one line per test of a pair, a kept test's reading `kept of a mirrored pair, the clashes only its mirror` then the mirror's run name and `finds are to be added to it once both run`, and a mirror's reading `a mirror of` the kept test
465. Look for: no block headed `MIRROR RENAMES` and no `CLASH` line counting sides that `point at no one set this document holds`, since a new NWF holds no test
466. Look for, in the `CLASH    created` lines: each mirror created under its own name with ` (mirror)` at the end, and each kept test under its XML name
467. Look for, after the last test's `CLASH` line and before the `TOLERANCE` line: one block headed `MIRROR MERGE` and a kept test's name for each pair whose both tests ran, saying what the kept test found, what the mirror found, how many both found and how many the mirror only, each clash only the mirror found named with its two items, and that the mirror was taken out of the report, or the one line saying why nothing was merged
468. Look for, in the same place: the `ROWS` line of each such kept test, which where its mirror found a clash it did not reads `of the rows is a clash only its mirror found, which the panel shows under the mirror`
469. Look for, under `WORKBOOK CHECK`: a `BLOCKS` line ending `in the workbook, one for every test in the file, 1830 less the K mirrors merged into their kept tests`, K the mirrors the MIRROR MERGE blocks say were taken out, or the line word for word as before F132 where none was
470. In Clash Detective after the run: every mirror's name ends with ` (mirror)`, each sits in the tests list as any other test, and no test was removed
471. Open the workbook: a merged mirror has no block of its own, and its kept test's block holds one row ending `, found by the mirror only in` and the mirror's name for each clash only the mirror found, as the MIRROR MERGE block counted them
472. Press Run again on the same folders, the NWF now there, a Weekly run
473. Look for, in its `MIRROR` block: the count line now says `0 mirrors are to be merged` and every pair `keep their own clashes`, then one line per kept test reading the kept test `and its mirror` the mirror's run name `keep their own clashes under their own names, not merged, because the document already holds a test named` the kept test, ending `and a clash both find may be counted twice`, five named and the rest counted
474. Look for: no `MIRROR MERGE` block, every `ROWS` line in the shape before F132, and the `BLOCKS` line word for word as before F132, since no mirror was merged
475. Clear the Clash XML box and press Run again, a run with no XML
476. Look for, in its `MIRROR` block: `0 pairs among the` count `tests saved in the document, which pair by their sides and never by a name`, ending `No rule list of a set was read, so only a test with the same two sets swapped pairs`, then one line per saved mirror reading its name `ends with (mirror), and whether its sides ask the question of` a count of `saved tests is UNKNOWN, a side or a set's rule list not read, so which saved test it mirrors is UNKNOWN. It keeps its own clashes, and a clash both find may be counted twice`, five named and the rest counted, and before the block no `CLASH` line counting sides that `point at no one set this document holds` unless a saved side was not read, which that line then counts
477. If your answer to Q144 keeps the by design pass on a run with no XML: tick `Mark by design connections as Reviewed`, pick the pairs file of the proof of F72 beside it, keep the Clash XML box clear and press Run again, a run with no XML and the box on
478. Look for, on the confirm screen before that run, a paragraph beginning `Clashes between two sets the by design pairs file names are moved to Reviewed in the NWF, in the tests created from the picked file and in the tests already saved in the NWF, whose sides are read off the tests themselves on a run with no clash XML`, and answer it with OK
479. Look for, as the first line of each group's `BY DESIGN` block: `sides read off the NWF : the` N `tests saved in the document, no clash XML picked, so this pass acts on the saved tests, and they reached` K `of the` P `pairs in the file`, N the count on that group's `saved tests keep the sides they were saved with` line and P the pairs the file holds, every `REVIEWED` line after it naming a saved test, and the RESULT block's `by design` line counting the moves across the run
480. Only once your answer B to Q131 is lifted and the viewpoints box is ticked on a first run over a new NWF, the run of step 461 again with the box on: look for, in the `VIEWS` lines, no line reading `is on the report and not in the document`, no line counting rows that `have no recorded place` or `no longer lead to their result`, and in the saved viewpoints tree one viewpoint under the kept test's name for each clash only its mirror found, named the kept test, the separator, the clash's name and `, found by the mirror only in` the mirror's name, and none under the mirror's name
481. Send the log file named on the `log file` line of the SESSION block, the one holding the four runs, and say which of steps 462 to 480 did not read as written

## Proof F132-R3 a second clash of a mirror on one pair of items, added 2026-10-12

On the run of step 461 with fix-F132c merged, a first run over a new NWF, the one run that merges.

669. Look for, in every `MIRROR MERGE` block: each mirror's line `found N, the mirror M, B by both and O by the mirror only`, with B, O and the count of its `found by an earlier mirror` part adding up to M, and no line reading `repeats a pair of items it already gave`
670. Open the workbook: each kept test's block holds as many rows whose name holds `, found by the mirror only in` as its mirrors' MIRROR MERGE lines count by the mirror only
671. Look for, in every `MIRROR MERGE` block: any line ending `Which copy is the twin of which is UNKNOWN, so no status was carried across and the report keeps` the kept test's name, and any line ending `is the twin of this one is not weighed`. Send each one, since neither has been seen on a run

## Proof of F128, the Generic Models sets, count and workbook

THESE STEPS FOLLOW STEP 481 IN THE SAME NAVISWORKS SESSION, your request 3 under Q112 and your
order of 8 Oct 2026, Q145 item 2. They were read off the add-in at 43e3c5f on fix-F128 and nothing
of it has run on Navisworks yet. They use the loop's copies of run set 04, C02 with 1A02MM's four
NWCs and C04 with 1A04PK's ten, where the probe of 2026-10-08, `docs\history\scan.md` 5z-zb,
read 0 Generic Models items in 1A02MM and 572 in 1A04PK over five of ten models, so every
count below is the probe's and not a guess. The feature has no tick box, it is on for every
run. The set paths read `lcop_selection_set_tree/Generic Models/` and the model's NWC name
without its extension.

491. Scan the C04 folder of run set 04, tick 1A04PK alone, pick `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` in the Clash XML box, keep the viewpoints box unticked, and press Run
492. Look for, in the group before the first `SET` line of the picked file: `SETS     1A04PK Generic Models, 10 sets planned in the folder Generic Models, one search set a model asking Category equals "Generic Models" and its Source File contains the model's Revit file name, no clash test`, and no `SETS     Generic Models note` line, since no two of the ten share a name or a text
493. Look for, after the picked file's `SET` lines and a second `SET      ran against` line: ten `SET` lines whose path begins `lcop_selection_set_tree/Generic Models/`, five reading `ok` with `2 conditions` and `528 items` for ST-MOD-000001, `32 items` for ME-MOD-000003, `6 items` for ME-MOD-000001, `3 items` for FP-MOD-000001 and `3 items` for ST-MOD-000002, and five reading `ZERO` with `0 items  asked for` and the two conditions, the Source File one naming the model's Revit file with `.rvt`, which for FP-MOD-000001 is `1104-PAR-1A04PK-ZZZ-FP-MOD-000004.rvt`, for ME-MOD-000001 `1104-PAR-1A04IP-ZZZ-ME-MOD-000005.rvt`, for ME-MOD-000002 `1104-PAR-1A04IH-ZZZ-ME-MOD-000002.rvt` and for ME-MOD-000003 `1104-PAR-1A04IP-ZZZ-ME-MOD-000003.rvt`
494. Look for, after them: `SETS     1A04PK Generic Models put into the document: 10 created, 0 already there and left alone`
495. Look for: a block headed `GENERIC MODELS 1A04PK` whose lines read `asked   : Category equals "Generic Models", found in each model by its Source File`, `models  : 10 models in this group, 5 found some, 5 found none and are left out, 0 were not counted`, `items   : 572 in all`, one `nought  :` line saying what a nought can mean, the five models with their counts, 528 written `528`, and the last line `no clash test is made for these sets`. No line names a model at nought
496. Look for, in the `STEP` line that closes `SETS`: the picked file's summary, then `. Generic Models: 10 created (5 finding items, 5 at zero), 0 already there.`
497. Look for: no `EMPTY SETS` line naming a Generic Models set, and in the `SETS ACROSS THE RUN` block at the end of the run no set whose path holds `Generic Models`
498. Look for, after the `WORKBOOK`, `HTML`, `XML` and `COVERAGE` steps: a step `INTERNAL` with `INTERNAL attempt` and `INTERNAL written` naming a file in the Clash Reports folder whose name is the group's workbook name, a space and `Internal`, ending `.xlsx`, with its size read back, beside the group's own workbook, then `INTERNAL read back, it holds 2 sheets, Generic Models, Coverage, each found by the name it was written under`. Your answer B to Q146 of 11 Oct 2026 puts the Coverage sheet in this workbook beside the Generic Models sheet. If the group's clash was skipped under F112, the group's workbook is skipped and this one is still written with both sheets
499. Open that workbook: its first sheet is named `Generic Models` and its second `Coverage`, which steps 622 and 623 read. The first sheet's first cell `Generic Models, written by the NWC Federator and not part of the client's report`, `Group: 1A04PK` under it, the summary line `10 models: 5 found some, 5 found none and are left out, 0 not counted. 572 items in all`, the headings `Model`, `Items` and `Why not counted`, five rows in the order of the models with 528 as a number and no row for a model at nought, and under them the note on what a nought can mean and `No clash test is made for these sets`
500. Open the group's own workbook: ONE SHEET, the client's sheet and nothing of ours after it, the Navisworks export's layout, and the `WORKBOOK CHECK` block names no second sheet
501. In the Sets panel of Navisworks: a folder `Generic Models` holding ten search sets named after the ten NWCs, and in Clash Detective no test naming any of them
502. Look for, in `RESULT` under the `COVERAGE` lines: `generic models : 572 items in 5 models of 10 over 1 group, 5 at nought, 0 not counted`, and the internal workbook among the files written with its size, its kind `INTERNAL`
503. Press Run again on the same folder, the NWF now there, a Weekly run: the ten `SET` lines read `present` with the same item counts and `already there, left alone`, the line of step 494 reads `0 created, 10 already there and left alone`, the block of step 495 reads the same counts, no `REMOVED` or `RENAMED` line names a Generic Models set of a model still in the group, with the rebuild box unticked and, on one more run, ticked, and the NWF is not saved a second time for these sets alone. On the run with the box ticked, no line reads `the Generic Models plan was not made` or `holds no model`, since the ten models read. A set of a model that has left the group is stale: were one NWC taken out of the folder, the run with the box ticked would read `SET      REMOVED lcop_selection_set_tree/Generic Models/` and that model's name, `which nothing pointed at`, which is right and is not asked of this proof
504. Clear the Clash XML box and press Run again, a run with no XML, then scan the C02 folder, tick 1A02MM alone, pick the XML again and press Run: in the run with no XML the `SETS` step still runs, with the line of step 492 and the block of step 495, and its closing `STEP` line begins `no XML picked, so no set of a file. Generic Models:`. In 1A02MM the block reads `models  : 4 models in this group, 0 found some, 4 found none and are left out, 0 were not counted`, `items   : 0 in all`, two `nought  :` lines, the second saying every model was counted at nought, which is also the count a wrong value gives, the workbook holds no model row, and RESULT reads `generic models : 0 items in 0 models of 4 over 1 group, 4 at nought, 0 not counted`
505. Look for, on every run above: no group ends FAILED or PARTIAL on account of these sets, since a throw in their plan, build or workbook write is a `FAILED` line in the `GENERIC MODELS` block and the count `UNKNOWN`, never the group's error, and where none threw no such line is in any block
506. Send the log file named on the `log file` line of the SESSION block, say which of steps 492 to 505 did not read as written, and read off the `TIMING` block of 1A04PK the seconds of `SETS` and of `INTERNAL`, which was `GENERIC XLSX` before Q146 was answered, Q145 item 5's and UNKNOWN until this run

## Proof F127 the coverage of the picked tests and the internal workbook, added 2026-10-11

THESE STEPS READ THE SAME RUN AS STEPS 491 TO 506, 1A04PK of run set 04's C04 with the XML
picked, and one more group whose clash runs. They were read off the add-in on fix-F127b and
nothing of it has run on Navisworks yet. Your request 2 under Q112, FR-176 and FR-200, and your
answer B to Q146: the Coverage sheet is in the internal workbook beside the clash workbook, and
the clash workbook holds the client's sheet alone. Every number below that this run has not read
is named by its shape, because the counts of 1A04PK's tests are UNKNOWN until it runs. Where
1A04PK's clash is skipped under F112, as `.claude\rules\core.md` reads of both wave buildings,
no test runs there, no clash workbook is written and the count check compares none, which is
why step 624 runs a second group.

616. Look for, before the first group: `COVERAGE the picked XML` with the XML's full path, then `reads sha256 ` and 64 letters and digits
617. Look for, in 1A04PK after its `XML` step: a step `COVERAGE` and a block headed `COVERAGE 1A04PK` whose first line reads `the tests        : 1830 in the picked file` with the XML's path, its sha256 and the corrections line of the list beside it
618. Look for, in that block: a `counted          :` line reading `1830 tests in the picked file:` then how many were created this run, already there, not created, run, with clashes and without. Where the clash was skipped under F112, the `run` count reads 0
619. Look for, under `one reason each  :`: one line for each reason with its count, the counts adding up to 1830, `not run, the clash was skipped by the coordinates rule` among them where the clash was skipped, and under them at most five tests named for each reason that has no results, then `and N more of this reason, every one on the Coverage sheet of the internal workbook and in the .tsv`
620. Look for, where the clash was skipped: `workbook against Clash Detective : UNKNOWN, 0 compared` then the counts not compared and held by neither side, the first not compared tests saying `no workbook was read, no clash workbook was written` and why, no line starting `COVERAGE FAILED`, and the block's last line `the group keeps its own result, a coverage count never fails a group`
621. Look for, in the `.tsv` beside the log: 1830 rows whose event is `coverage test`, one for each test of the file, each carrying its position, whether it is in the document, whether it ran and its reason
622. Open the internal workbook of step 498 and its `Coverage` sheet: its first cell `Coverage, written by the NWC Federator and not part of the client's report`, the picked file with `sha256:` beside it, the models with their codes, the counts line of step 618, then a `TESTS` table of 1830 rows in the file's order, a test not created among them with `not created` and `no` and its reason, and no empty cell where a number was not taken, which reads `UNKNOWN`
623. Look for, on the same sheet: the `MIRRORED PAIRS` section reading `UNKNOWN on this sheet, the MIRROR and MIRROR COVERAGE blocks of the log name the pairs F132's rule judged`, never that the rule is not on main
624. Scan a folder holding a group of two disciplines or more whose models sit on the same shared coordinates, the fixture of steps 410 to 416, pick the same XML and press Run
625. Look for, in that group's `COVERAGE` block: `workbook against Clash Detective :` then a number compared, the same number agree and `0 FAILED`, the counts of Clash Detective's panel for each test that ran equal to the rows of its block in the clash workbook. A test that did not run is never among those agreeing, its row reading `NOT COMPARED, it did not run this run`. Where any line starts `COVERAGE FAILED`, it names the group, the test and all four numbers, and RESULT carries the same line
626. Look for, in `RESULT`: the `COVERAGE tests counted` lines, the counts of the tests once across the run, then `COVERAGE checked :` with how many were compared, agreed and FAILED over how many groups, that number of groups every group the run took, a group that ended before its `COVERAGE` step counted among the groups not checked, and never `COVERAGE checked : UNKNOWN, no coverage was taken in this run`
627. Look for, on every run above: no group ends FAILED or PARTIAL on account of the coverage, and each group's clash workbook holds ONE sheet, its `WORKBOOK CHECK` block naming no second sheet
628. Send the log file and the `.tsv` beside it, and say which of steps 616 to 627 did not read as written, with the seconds of the `COVERAGE` step off the `TIMING` block, UNKNOWN until this run

## Proof F141 the framed views, added 2026-10-11, rewritten for attempt 2

The loop runs these on its own copy, 1A02MM again from a fresh set, after main with F141 attempt 2 is installed in place. They are here so a run on the local machine reads the same. Attempt 1 was proved by set 07's run of 1A02MM on main 89082a2, no view threw. That run read all 69 views back with no material override, F141-R1, which attempt 2 takes.

629. Install main with F141 attempt 2 merged, then scan the C02 folder, pick the clash XML, untick Skip clash for models off shared coordinates, leave Make viewpoints ticked, tick 1A02MM alone and press Run
630. Look for, in the `VIEWS BUILT` block of 1A02MM: `views created     :` equal to the plan's views, 69 in set 07's run, NO `views that failed` line, and no line naming `ObjectDisposedException` or `Clash Detective gave no camera for its first clash`
631. Look for, on the same block: no view reading `it was added without its dimming`, where set 07's run read it on all 69
632. Look for, in the `VIEWS TREE` block: `CHECK 3` and `CHECK 4` each `0 broke it`, where set 07's run read `FAILED CHECK 4` with `the painted items of 69 of 69 views read back off the document`, and the last line `7 of 7 hold`
633. Send the log file and the `.tsv` beside it, with the `VIEWS` seconds off the `TIMING` block, against 7487.104 s in set 04's run of the same building

## Proof F143 the harvest answers between rows, added 2026-10-11

634. Install main with F143, then run 1A04PK on a cut down XML or the whole one, Make viewpoints ticked
635. Look for, in the log while a test with many clashes is read: a `PROGRESS HARVEST <test> row N of M` line at least every 30 s, the window's live line moving, and the window answering a click on its title bar between rows
636. Look for, where one picture or one test took 30 s or more: a `CALL     TestsImageForResult on <test> <clash> took Ns` or `CALL     TestsRunTest on <test> took Ns` line, and no CALL line under 30 s
637. Look for, on the run's TIMING block: VIEWS lines every 30 s where the step is long, and the same rows, pictures and workbook as before, the fix changing no output
638. Send the log and the `.tsv` beside it, with the longest gap between two lines of the harvest

## Proof F145 only the test's models shown for its pictures, added 2026-10-11, rewritten for attempt 2

639. Install main with F145 merged, then run 1A02MM as F141's steps 629 to 633 do, and look for the same: `views created     :` equal to the plan's views, `7 of 7 hold`, and the VIEWS line `hidden state put back: nothing was hidden before the views and nothing is hidden now`, which says nothing hidden for a picture was carried into the views, the NWF or the NWD
640. Look for, in the same log: one line `IMAGES   only the models of each test shown for its pictures:`, its tests with models hidden, the tests kept with every model shown, and the seconds reading the tests' sides and the seconds hiding and putting back, and no failure line naming `reading which models the sides`, `hiding the models other than`, `putting the hidden state back` or `reading the hidden state`. Open a picture of a test between two models, such as an ME test against AR: both models' items behind the clash
641. Run 1A04PK on the 50 test cut of the corrected matrix, Make viewpoints ticked, and look for the same `IMAGES` line and no such failure line
642. Open three pictures of BLD-AR-Walls-vs-BLD-AR-Floors in the group's picture folder: the same 1024 square, camera, grid label, red and green as before, and only the AR model's items behind the two clashing ones. Open a picture of the mirror pair's kept test where it has one
643. Look for, in the `IMAGES` lines of the group: the seconds each picture took, against the probe's 0.168 s a picture on that test with every model shown and 0.120 s with only the test's, with hardware acceleration on, `docs\history\scan.md` 5z-zd, and the group's seconds off its `GROUP finished` line
644. Compare the Grid Location and Level cells of the rows of BLD-AR-Ramps-vs-BLD-AR-Floors in this run's internal workbook with set 08's, `steps\runs\08\item1-C04`: the same cells. With F149 merged the harvest hides nothing and every row is read before any model is hidden for a picture, so these cells are read with every model shown, as set 08 read them
645. Send the log and the `.tsv` beside it, with the longest stretch the window did not answer and the peak memory off the run's `LIFE SUMMARY` where the loop ran it

## Proof F146 the renumber answers while it renames, added 2026-10-12

646. Install main with F146 merged, then run 1A02MM with its clash XML, Make viewpoints ticked
647. Look for, in the log while the pictures are renamed: one `PROGRESS RENUMBER 1A02MM` line about 30 s into the step, naming the pass it fell in, and the window's live line moving through picture checked, picture moved aside and picture renamed
648. Look for, in the run's LIFE readings where the loop ran it: no stretch not answering in RENUMBER, where sets 07 to 09 read 32 s, and the IMAGES line `numbered in report order: 2931 renamed` as before

## Proof F149 pictures taken in the report's order, added 2026-10-12, rewritten for attempt 2

The renumber line comes first, because it is the whole of the fix: a picture written under any other name than its final one shows there as renamed. The TIMING block next, since the pictures' seconds move from HARVEST to a step of their own, PICTURES, and a reading against set 09 that misses the move reads HARVEST as faster than it is.

649. Install main with F145 and F149 merged
650. Scan the C02 folder
651. Pick the clash XML
652. Untick `Skip clash when models sit off shared coordinates` on the Clash step, as set 09 ran, whose log says the rule was off. The box starts ticked on every open
653. Tick 1A02MM alone in the group list, leaving Make viewpoints ticked
654. Press Run
655. Look for, after the CLASH lines of 1A02MM: the IMAGES line `numbered in report order: 0 renamed, 2931 already right, 0 missing`, where set 09 read 2931 renamed
656. Look for, on the group's TIMING block: a `PICTURES` row among the top level steps and `IMAGES` among the nested ones, `RENUMBER` near nought against 38.930 s in set 09, and `HARVEST` lower than set 09's by about the pictures' seconds, which now sit under PICTURES
657. Look for, in the log while the pictures are taken: the `STEP     PICTURES` started line, then a `PROGRESS PICTURES <test> picture N of M` line at least every 30 s until its finished line, and the window's live line naming PICTURES
658. Look for, in the group's IMAGES lines: the line starting `IMAGES   2931 written,` with no line under it saying `failed. Their cells are empty and the run carried on`, since that line is written only where a picture failed, and no `IMAGE    failed for` line and no `IMAGE ` line saying `so no picture of the`
659. Open the clash workbook of 1A02MM: each Image cell carries its picture's name and its link opens that picture, as in set 09
660. Open cd000001.jpg, cd000002.jpg and cd010001.jpg of this run beside set 09's pictures of the same names: each shows the same two clashing items at the same camera, with only the test's models behind them where set 09 showed every model, F145
661. Look for, in the `VIEWS TREE` block: the last line `7 of 7 hold`
662. Look for, in the `COVERAGE` block: every count compared agreeing, 528 of 528 in set 09
663. Send the log and the `.tsv` beside it, with the PICTURES, IMAGES, HARVEST and RENUMBER seconds off the TIMING block

## Proof F150 Clash Detective asked to hide the other items for the pictures, added 2026-10-12, rewritten for attempt 2

Your own setting is written down first, because the run must leave it as it found it. Then the IMAGES line, which says what the tool read, set and read back, and then the picture sizes, which are the only thing that says whether the pictures followed: a read back says what Clash Detective holds and never what it drew.

900. Install main with F150 merged
901. Start Navisworks Manage 2025
902. Open any NWF
903. Open Clash Detective
904. Open its Results tab
905. Press Display Settings. Write down whether Hide Other under Isolation is ticked, and change nothing
906. Open the Parsons NWC Federator window and scan the C02 folder
907. Pick the clash XML
908. Untick `Skip clash when models sit off shared coordinates` on the Clash step, as set 09 ran
909. Open More, rarely changed and leave `Ghost the other items in clash photos` unticked, as it starts
910. Tick 1A02MM alone in the group list, leaving Make viewpoints ticked
911. Press Run
912. Look for, in the CLASH lines of 1A02MM: the line `CLASH    Images on,` ending `Clash Detective asked to hide the other items.`
913. Look for, after the PICTURES lines, where step 905 read Hide Other unticked: ONE line `IMAGES   Clash Detective's Hide Other asked on for the pictures, to hide the other items: it read off before the pictures, set on, and it read back on, then put back to off. It read off after the pictures, the user's own setting.` and then `Whether the pictures follow it is UNKNOWN until a run's picture sizes show it`
914. Where step 905 read Hide Other ticked, look for instead: `it read on before the pictures, already on, so nothing was set`, and no `put back`
915. Look for, in the whole log: NO failure line naming `Clash Detective's Hide Other`, no `left changed`, and no line `put back once more at the end of the run`
916. Open the group's picture folder beside the workbook
917. Sort it by size. Look for: files near 48 KB each, the look of `docs\history\scan.md` 5z-ze. Files near 550 KB mean the pictures did NOT follow the value set, which is the answer this proof is for, so send it
918. Open cd000001.jpg. Look for: the two clashing items in red and green on the background, and no other item drawn
919. Open Clash Detective in Navisworks
920. Open its Results tab
921. Press Display Settings. Look for: Hide Other under Isolation ticked or not exactly as step 905 wrote it down
922. Open More, rarely changed on the tool's window
923. Tick `Ghost the other items in clash photos`
924. Press Run
925. Look for, in the CLASH lines: the line `CLASH    Images on,` ending `Clash Detective asked to show the other items.`
926. Look for, after the PICTURES lines: `IMAGES   Clash Detective's Hide Other asked off for the pictures, to show the other items, since the box "Ghost the other items in clash photos" is ticked:`, then `already off, so nothing was set` where step 905 read it unticked, or `set off, and it read back off, then put back to on` where it read ticked
927. Open the group's picture folder and sort it by size. Look for: files clearly larger than step 917's, since the other items are drawn. How large is UNKNOWN, since with every model drawn the laptop wrote about 550 KB, scan.md 5z-zf, and since F145 only the test's models are shown
928. Open cd000001.jpg. Look for: the two clashing items with the other items drawn ghosted around them
929. Open Clash Detective in Navisworks
930. Open its Results tab
931. Press Display Settings. Look for: Hide Other exactly as step 905 wrote it down
932. Send the two logs and the `.tsv` beside each, with the picture folder's mean file size of each run and what steps 905, 685 and 931 read
## Proof F119 the NWF saves, the files written list and the tolerance rows, added 2026-10-12

The reshape comes first, because it is the one item a plain run shows: a group whose file list changed and whose clash step and views put nothing in. The read only files come after it, since they are what makes a save or a publish fail on purpose, and they are put back at the end.

720. Install main with F119's last six merged
721. In Explorer find a group of one discipline and two or more NWC files that already has an NWF from an earlier run of this tool. One discipline runs no clash test
722. Copy its NWC folder to a new folder on the desktop, with its NWF beside it where the run writes it
723. Move one of the NWC files out of the copy, so the group reads CHANGED
724. Write down the NWF's size in bytes off its Properties
725. In the tool on 1. Source pick the copy and press Scan
726. On 4. Clash leave the clash XML empty
727. Untick Make viewpoints
728. On 2. Grouping tick that group alone
729. Press Run and wait for it to finish
730. Look for, in the group's lines: a `RESHAPE` line, then `NWF      attempt` and `NWF      written` with a size different from the one written down, then the group DONE on its `GROUP` finished line, as Rebuilt in the RESULT block
731. Open that NWF in Navisworks
732. Look for, in the Selection Tree: no model of the NWC moved out of the copy, where before this fix the NWF on disk kept it
733. Run 1A02MM with the corrected 25 mm XML, `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`
734. Open the `.tsv` beside its log in Excel
735. Look for, on every `test created` row: the tolerance with all its decimals in the number column, 0.0820209974 where the document measures in feet, where set 03 read 0.082, or 0.025 where it measures in metres, the text column saying which
736. On 4. Clash choose 25 mm in the Clash tolerance list
737. Run 1A02MM again on the NWF the last run wrote
738. Look for, on the `TOLERANCE` line: `Set on N tests, 0 created fresh and N already in the document` where the saved tests were at another value, or `Not set on N saved tests: N already at the value` where they were at 25 mm, and no saved test counted twice
739. In Explorer tick Read-only in the Properties of the NWF of 1A02MM
740. Run 1A02MM with its XML
741. Look for: `NWF      NOT WRITTEN` with `the call that writes it did not report success`, and the group FAILED with the reason `the NWF save did not report success` or `of 2 saves of the NWF did not report success, the last among them`, naming the NWF's path
742. Write down whether a `the save after the clash work returned false` line or a failure line came before it
743. Clear the Read-only box of the NWF
744. Tick Read-only in the Properties of the NWD of 1A02MM
745. Run 1A02MM with its XML
746. Look for: `NWD      NOT WRITTEN` with `are not from this run`, no NWD of 1A02MM under `files written` in the RESULT block, and the group FAILED with the reason that the NWD publish did not report success
747. Clear the Read-only box of the NWD
748. Run 1A02MM with `Skip clash when models sit off shared coordinates` ticked, as it starts
749. Look for, in TIMING BESIDE SIZE: the row of 1A02MM reading `UNKNOWN clashes`, where set 05 read `0 clashes`, while RESULT reads `clashes found  : UNKNOWN`
750. Send the logs and the `.tsv` files beside them
## Proof F120 the GAP block after FR-198, added 2026-10-12

Core is proved by its tests. These steps read the GAP and ITEM IDS blocks of one run against set 09's, `steps\runs\09\item1-C02`. F120-R1, the pictures' guard on throws naming their path, has no step, since no run a person makes throws fifty times that way, and its proof is the Core tests.

760. Install main with fix-F120c merged
761. Run 1A02MM with its clash XML as set 09 ran, by steps 649 to 654
762. Look for, in the `GAP 1A02MM` block: the line `2 numbers are measured off every item and reach no output`, then a `Source File` line and an `Id From` line, and no `Family`, `Type Name` or `Material` line, where set 09 read four lines
763. Look for, on the `Source File` line: `read on N item cells and empty on every one`, N the run's item cells, 5,862 in set 09, which left the line out. A line reading `of N item cells carry it` instead means the source file was read on this run, and row F120-R2 is answered by it
764. Look for, in the `ITEM IDS 1A02MM` block: `Id supplied 5862 item ids of 5862, written as "Element ID"`, as set 09 read, the count the run's own
765. Look for, at the end of the run: `GAPS     2 things are measured on every item and reach no output, over 1 group: Source File, Id From`
766. Open the clash workbook of 1A02MM beside set 09's
767. Look for: the same columns and the same cells on the first block
768. Send the log with the HARVEST seconds off its TIMING block, beside set 09's

## Proof F147 long runs kept from slowing, added 2026-10-12, rewritten for attempt 3

These read one run of 1A02MM with F147 merged, the run of F149's steps 650 to 654. One group is enough for every line here. The memory a group releases shows only at the next group's start, so it is not read here.

933. Install main with F147 merged
934. Run 1A02MM as steps 650 to 654 say
935. Look for, in the log under the first `STEP     DECIDE` started line: `MEMORY   at the start of DECIDE, private bytes` a size in MB, `working set` a size in MB, `managed heap` a size in MB
936. Look for, above the `STEP     DECIDE` finished line, with the census lines between: `MEMORY   at the end of DECIDE,` the same three, each followed by `up`, `down` or `unchanged`
937. Look for: one MEMORY start line and one MEMORY end line for each STEP started line of the group, one pair only for TESTS RUN and one only for HARVEST however many tests ran, no MEMORY line holding `UNKNOWN`, and after the group's repeated step lines one `MEMORY   across the` N `visits of TESTS RUN in this group` for each step entered again
938. Look for, in the same log after the `STEP     PICTURES` line: `IMAGES   the undo for the pictures read on with` a step name `as the step Undo would take next before them, off with` the same name `during them, and on with` the same name `after`, and no line `THE UNDO IS LEFT OFF`
939. Look for, in the same log: `IMAGES   the texture cache at` the folder ogs under the temp folder, `held` a size and its files as the pictures began and as they ended, `against the limit of 5.00 GB (5368709120 bytes)` and `cleared 0 times`, or the line saying it `is not there`. Where a clearing line was written, it ends `cleared again only once it holds more than` a size, or carries no such words
940. Send the log and the `.tsv` beside it, with the MEMORY lines at the start of DECIDE and at the end of CONFIRM
## Proof F114c the views' sides, added 2026-10-12, rewritten for attempt 2

The first run reads that no view moved to another pair folder now a side is read by its whole locator. The second reads the same with no clash XML picked, where the views get no set tree. A test the clash step failed on, F114-K38, cannot be made to fail by hand, so no step forces it. FR-067 is not in this pass.

941. Install main with fix-F114c merged
942. Scan the C02 folder
943. Pick the clash XML
944. Untick `Skip clash when models sit off shared coordinates` on the Clash step
945. Tick 1A02MM alone in the group list, leaving Make viewpoints ticked
946. Press Run
947. Look for, in the `VIEWS` block of 1A02MM: the same pair folders as set 09's run of 1A02MM, `steps\runs\09\item1-C02`, and in the `VIEWS TREE` block the last line `7 of 7 hold`
948. Close Navisworks
949. Start Navisworks and open the tool
950. Scan the C02 folder again, picking no clash XML
951. Untick `Skip clash when models sit off shared coordinates` on the Clash step, which starts ticked on every open
952. Tick 1A02MM alone in the group list, leaving Make viewpoints ticked
953. Press Run
954. Look for, in the `VIEWS INVENTORY` block: the views of step 946 `removed, this tool's earlier view replaced at its place`, and in the `VIEWS` block the same pair folders as step 947, but for a test with a side whose set name carries no code, such as BLD-Security Devices, which with no clash XML reads UNKNOWN as it did before this pass
955. Send the logs of steps 946 and 953
## Proof F115 the register rows of the sets, added 2026-10-12

The weekly run with the box off comes first, because it is the run the team makes every week and it is where the leftovers are now reported and the SETS step may take longer. The run with the box ticked last, since it changes the NWF. A damaged document, two sets of one name in one folder, a folder as a clash source and a set lost after it was put in cannot be made to happen on a real folder, so those rows are read and not run.

956. Install main with the F115 register rows merged
957. Scan the C02 folder
958. Pick the clash XML
959. Leave `Rebuild sets that drifted from the file` unticked
960. Tick 1A02MM alone in the group list
961. Press Run
962. Look for, in the SET lines of 1A02MM: either no block about sets the file does not name, or one starting `set(s) in this NWF are not named by the picked file:` whose next line reads `NOTHING BELOW IS DONE, because "Rebuild sets that drifted from the file" is off`, and each line under it saying what `a run with the box ticked would` do, R20
963. Look for, on the group's TIMING block: the SETS seconds, to send beside the last run's, since the leftover walk now reads every clash test side with the box off too, R20
964. Look for, in the SETS step's finish phrase and the line under the SETS lines: a count `drifted` or `could not be read` wherever the SET DRIFT lines name a set drifted or not read, R7
965. Look for, after the CLASH block: the EMPTY SETS block as before, and a line `were not judged` only where a SET NOT READ line names a set at zero, R6
966. Look for: no FAILURE line naming `the side's selection sources read as none`, R21
967. Open the NWF of 1A02MM in Navisworks with the tool's window open and the clash XML picked
968. Press Build sets
969. Look for, in the log after the button's SET lines: an EMPTY SETS block ending `no clash test ran with these sets here, so what they cost in clash tests is not counted`, wherever the window's lines show a set at zero, R4
970. Close the NWF without saving it
971. Tick `Rebuild sets that drifted from the file`
972. Tick 1A02MM alone in the group list
973. Press Run
974. Look for, under the SET DRIFT lines of each drifted set: `REBUILT from the picked file`, and no line `read back after the rebuild it does NOT ask what the picked file asks`, R19. Where that line shows under an Or set, it is the answer to F115-R12
975. Look for: no line `NOT rebuilt, though "Rebuild sets that drifted from the file" is ticked`, R18. Where one shows, why is said where the rebuild was tried, before the set's present line
976. Look for: no line `nothing removed for`, `nothing done for` or `so it was NOT saved`. Where one shows, send it with the SET lines before it
977. Send the log and the `.tsv` beside it, with the SETS seconds of both runs

## Proof F121 wave 4, the NWF saved after an apply, the second undo and the folders file, added 2026-10-13

Everything runs on copies in two new folders on the desktop, so no NWF of the real folders is changed. The apply comes first, since it needs 1A02MM's clash skipped off the shared coordinates, as the box starts. The undo second, since it needs that clash run. The folders file last, since it needs Navisworks closed. FR-155 has no step: it is Core alone, proved by its tests, and a file every exporter writes whole reads as before.

800. Install main with fix-F121d merged
801. In Explorer make a new folder on the desktop named `F121 source`
802. Copy the NWC files of 1A02MM into `F121 source`
803. Make a second new folder on the desktop named `F121 out`
804. Copy `1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` and the two `.txt` files beside it from the `exchange` folder of the repo into `F121 out`
805. Open the copy in `F121 out` in Notepad
806. Replace every `tolerance="0.0820209974"` with `tolerance="0.1640419948"`, which is 50 mm where the file says 25 mm
807. Save and close Notepad
808. Start Navisworks and open the tool
809. On 1. Source pick `F121 source` and press Scan
810. On 3. Outputs pick `F121 out` for the NWF folder
811. Pick `F121 out` for the NWD folder
812. Pick `F121 out` for the Excel folder
813. On 4. Clash pick the clash XML `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` of the repo, not the copy
814. Untick Make viewpoints
815. Leave `Skip clash when models sit off shared coordinates` ticked, as it starts
816. On 2. Grouping tick 1A02MM alone
817. Press Run and wait for it to finish
818. Look for: the clash of 1A02MM skipped as not on the same shared coordinates, its tests created and none run. Where its tests ran instead, steps 819 to 830 cannot show the fault, and say so
819. In Explorer write down the modified time of the NWF of 1A02MM in `F121 out`
820. On 4. Clash pick the copy in `F121 out` as the clash XML
821. Tick `Apply the file's settings to existing tests`
822. Press Run and wait for it to finish
823. Look for, in the `DRIFT` block of 1A02MM: a difference in `the tolerance` across the tests, and the line starting `APPLYING these to the tests in the document`
824. Look for: a `CLASH    APPLIED` line for each test of the group, then each test skipped as not on the same shared coordinates
825. Look for, after the CLASH lines: a `STEP     NWF SAVE` line and an `NWF` line with the size written, where before this fix no NWF SAVE step came after an apply where nothing ran. Where a SET line of this run says a set was created or rebuilt, the save would have come anyway, and say so
826. In Explorer look at the NWF's modified time: later than the one of step 819
827. Press Run again with the same picks and wait for it to finish
828. Look for, in the `DRIFT` block: `Nothing has drifted. Every one of them matches the file.`, and no `CLASH    APPLIED` line, where before this fix the same drift was found and applied again
829. Untick `Apply the file's settings to existing tests`
830. Send the logs of steps 817, 822 and 827
831. On 4. Clash pick the clash XML `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` of the repo again
832. Untick `Skip clash when models sit off shared coordinates`
833. Tick `Mark penetrations as Reviewed`
834. Press Run and wait for it to finish
835. Look for, in the `PENETRATION` block of 1A02MM: a count of clashes moved to Reviewed above 0. Where it reads 0, steps 836 to 849 cannot be read, and say so
836. Open the NWF of 1A02MM in `F121 out` in Navisworks
837. In the tool on 4. Clash press `Undo auto Reviewed`
838. Look for, in the log: the `UNDO` block with `put back          :` the count of step 835, and a line `0  an undo already put it back, so the status it carries now is a person's and stands`
839. Open Clash Detective
840. Pick the test named on the first `UNDO` line of that block
841. Pick the clash named on the same line
842. Look for, in its Comments: `Parsons NWC Federator auto Reviewed [penetration] [was New]` and after it `Parsons NWC Federator put back [penetration] [to New] by Undo auto Reviewed`, and its status New, or Active in all three places where the clash was Active before the run. Whether the panel lists the two in that order is UNKNOWN, and write down the order shown
843. Set that clash to Reviewed by hand in Clash Detective
844. In the tool press `Undo auto Reviewed` again
845. Look for, in the new `UNDO` block: no line naming that clash, `put back          : 0`, and the line `an undo already put it back, so the status it carries now is a person's and stands` counting at least 1
846. Look for, in Clash Detective: that clash still Reviewed, where before this fix the second undo put it back to New
847. Close the NWF without saving it
848. Untick `Mark penetrations as Reviewed`
849. Send the log of steps 837 and 844
850. Close Navisworks
851. In Explorer open the folder holding the file named on the `log file` line of the last log
852. Open the Properties of `folders.txt` in that folder
853. Tick Read-only
854. Press OK
855. Start Navisworks and open the tool
856. On 1. Source press Browse and pick `F121 source`
857. Look for, in the log: `FOLDERS  the folder picked for Source is remembered for this session only, because` the path of `folders.txt` `could not be written:` and the reason
858. On 3. Outputs press Browse beside the NWF folder and pick `F121 out`
859. Look for: no second `FOLDERS` line
860. In Explorer open the Properties of `folders.txt` again
861. Clear Read-only
862. Press OK
863. On 3. Outputs press Browse beside the NWD folder and pick `F121 out`
864. Look for: no `FOLDERS` line
865. Open `folders.txt` in Notepad
866. Look for: a line `Nwd=` naming `F121 out`
867. Send the log of steps 856 to 864
## Proof F127-R5 a workbook named as another group's internal workbook, added 2026-10-12, rewritten for attempt 2

No group runs in these steps, so they can follow any install of main with fix-F127c merged.

978. Install main with fix-F127c merged
979. Start Navisworks Manage 2025 and open the tool's window
980. In Explorer make a new empty folder on the desktop named F127 outputs
981. On 1. Source press Browse beside Folder and pick a folder holding at least two groups
982. Press Scan
983. Open 3. Outputs
984. Press Browse beside NWF folder and pick F127 outputs
985. Press Browse beside NWD folder and pick F127 outputs
986. Copy the Workbook name of the first group in the table
987. Type it, with one space and Internal after it, into the Workbook name cell of the second group
988. Look for, in the name preview under the table: `THE RUN CANNOT START.` and `The internal workbook of <first group> and the workbook of <second group> would both be written as <that name>. One would overwrite the other, so the run does not start.`
989. Open 2. Grouping and see that the Run box of both groups is ticked, ticking any that is not
990. Press Run
991. Look for: a warning holding the same sentence, the window back on 3. Outputs, and in the log one line starting `RUN      refused before starting.` with it
992. Open 3. Outputs and clear the Workbook name cell of the second group, which gives it back to its pattern
993. Open 2. Grouping and see that the Run box of both groups is ticked, ticking any that is not
994. Open 4. Clash and pick Other in the Clash tolerance list, leaving the box beside it empty
995. Open 3. Outputs and type one letter at the end of the Workbook name cell of the first group
996. Look for: no error box appears and the name preview under the table shows the new name
997. Delete that letter again
998. Press Run
999. Look for: a warning about the clash tolerance, and nothing run
1000. In Explorer make a new empty folder on the desktop named F127 NWFs
1001. Copy into it one NWF an earlier run of the tool wrote, together with the NWD beside it
1002. Copy that NWF and its NWD into the same folder a second time
1003. Rename the first NWF and its NWD to A.nwf and A.nwd
1004. Rename the second NWF and its NWD to `A Internal.nwf` and `A Internal.nwd`
1005. In the tool open 1. Source and press Folder beside Existing NWF or folder of NWFs
1006. Pick the folder F127 NWFs
1007. Look for, in the summary under that row: the run refused, naming A and `A Internal` and the workbook name `A Internal` they would both write
1008. Send the log with the refusal of step 991 and the summary of step 1007

## Proof of F130, the Shift range in the group list

THESE STEPS RUN NO GROUP UNTIL STEP 578 and can follow step 541 in the same Navisworks session,
your request 5 under Q112. They were read off the add-in on fix-F130 and nothing of it has run on
Navisworks yet. The window driver cannot make a Shift click without real input, measured on
2026-10-05, `turn5\measure-shift-driver-result.md` under the loop's folder, so these are yours by
hand, with the mouse and the Shift key. They use the C02 folder of ten groups named at the top of
this file, whose groups the list shows in building order: 1000BS, 1A0215, 1A02BS, 1A02MM, 1A02MS,
1A02WE, 1A02WL, 1A02WM, 1A02WN and 1A02WO. A scan, a grouping change or a Use box makes the groups
again and ticks every one, so change nothing on the Source step unless a step says so.

542. Go to the Source step, pick the C02 folder of ten groups and press Scan
543. Go to the Grouping step
544. Look for: ten rows in the building order above, every Run box ticked, the first column headed `Run` and as narrow as before, and under the list the grey line `Shift click a Run box to tick or untick the rows between`
545. Click the Run box of 1A0215 once, holding no key
546. Look for: 1A0215 unticked on that one click and the other nine ticked. If the click only highlighted the row and the box stayed ticked, say so, since the flip on one click is UNKNOWN until here
547. Hold Shift and click the Run box of 1A02MS
548. Look for: 1A0215, 1A02BS, 1A02MM and 1A02MS unticked, both ends included, and 1000BS and the five from 1A02WE to 1A02WO ticked
549. Click the Run box of 1A02MM once, holding no key
550. Look for: 1A02MM ticked again and 1A0215, 1A02BS and 1A02MS still unticked
551. Hold Shift and click the Run box of 1000BS
552. Look for: 1000BS, 1A0215, 1A02BS and 1A02MM ticked, 1A02MS alone unticked, and the five after it ticked
553. Go to the Outputs step
554. Look for: the line at the foot of the Outputs step begins `9 groups ticked to run.`
555. Go to the Source step and press Scan again, which makes the groups again
556. Go to the Grouping step
557. Look for: all ten ticked again
558. Hold Shift and click the Run box of 1A02WE
559. Look for: 1A02WE alone unticked, because the groups were made again and no box was clicked since, so that Shift click was a plain click
560. Click the Run box of 1A02WO once, holding no key
561. Click the header `Run` of the first column once
562. Look for: 1A02WE and 1A02WO as the two top rows, both unticked, and the eight ticked rows under them. If the two sit at the bottom, click the header once more
563. Click the Run box of the fourth row from the top once, holding no key
564. Hold Shift and click the Run box of the top row
565. Look for: exactly the four rows shown at the top unticked, 1A02WE, 1A02WO and the third and fourth rows whichever buildings they are, and the six rows under them ticked. If a row moved when a box was clicked, say so
566. Go to the Source step and press Scan again
567. Go to the Grouping step
568. Click the header `Building` once
569. Look for: ten rows in building order, every one ticked
570. Click the Run box of 1000BS once, holding no key
571. Hold Shift and click the Run box of 1A02WO
572. Look for: all ten unticked
573. Go to the Outputs step
574. Look for: the line at the foot of the Outputs step begins `0 groups ticked to run.`
575. Go to the Grouping step
576. Click the Run box of 1A02MM once, holding no key
577. Look for: 1A02MM alone ticked
578. Press Run with the folders and the Clash step as the proofs above left them, and let the one group finish
579. Look for, in the log after the `RUN SETTINGS` lines: the `GROUPS` block with `run` beside 1A02MM, `unticked` beside the other nine, and the line `9 groups unticked in the Run column, nothing else drops a group`
580. Send the log file named on the `log file` line of the SESSION block and say which of steps 544 to 579 did not read as written

## Proof of F130-R1, the space bar on the Run cell, added 2026-10-12

These run no group and can follow step 580 in the same session, once fix-F130c has merged and is installed. They are yours by hand, with the arrow keys and the space bar, since the window driver cannot carry Shift. They use the C02 folder of ten groups of steps 542 to 580.

1009. Go to the Source step and press Scan
1010. Go to the Grouping step. Look for: ten rows, every Run box ticked
1011. Click the Building cell of 1A0215 once
1012. Press the Left arrow once. Look for: the Run cell of 1A0215 has the focus, its border drawn
1013. Press Space. Look for: 1A0215 unticked and the other nine ticked
1014. Go to the Outputs step. Look for, at its foot: `9 groups ticked to run.`
1015. Go back to the Grouping step and click the Building cell of 1A0215 once
1016. Press the Left arrow once
1017. Press the Down arrow three times. Look for: the Run cell of 1A02MS has the focus
1018. Hold Shift and press Space. Look for: 1A0215, 1A02BS, 1A02MM and 1A02MS unticked, both ends included, and the other six ticked
1019. Press Space again, holding no key. Look for: 1A02MS alone ticked again, the box flipping once and not twice
1020. Click the Run box of 1A02WE itself once. Look for: 1A02WE unticked by the click
1021. Press Space with no other key. Look for: 1A02WE ticked again by the Space, the box flipping once
1022. Say which of steps 1009 to 1021 did not read as written

## Proof of F129, a picked NWF or a folder of NWFs, run where it sits

THESE STEPS FOLLOW STEP 580 IN THE SAME NAVISWORKS SESSION, your request 4 under Q112 and your
order of the evening of 8 Oct 2026, Q147 item 3, once F129 has merged and is installed and once
the timed runs of set 05 of Q145 item 5 have written the NWFs of 1A02MM and 1A04PK. They run on
COPIES of those two NWFs and never on the NWFs set 05 wrote. They were read off the add-in on
fix-F129 and nothing of it has run on Navisworks yet. Whether an NWF copied into another folder
still finds its NWCs is UNKNOWN until step 586: an NWF holds pointers to the NWCs and not copies,
and whether Navisworks reads them by the full path or beside the NWF is not measured.

581. Build and install main once F129 has merged, steps 7 to 12
582. In Explorer, make a folder `f129` under `%LOCALAPPDATA%\NwcFederatorLoop\runs\05`, and in it two folders, `one` and `two`
583. Copy into `one` the NWF set 05's run of C02 wrote for 1A02MM, the path named on that run's `files written` lines in RESULT, and nothing else
584. Copy into `two` the NWF set 05's run of C04 wrote for 1A04PK, the same way, and nothing else
585. Open the window, step 1. Source. Look for, under the Folder row: a row labelled `Existing NWF or folder of NWFs` with a box, the buttons `File`, `Folder` and `Run NWFs`, and under the box the grey line `Runs each where it sits, its NWD and report beside it`
586. Press `File`, pick the 1A02MM copy in `one`. Look for, at the bottom of the step: `1 NWF found in` the copy's path `, 1 can run. Each writes its NWD and report beside it.`
587. Clear the Clash XML box on step 4, leave every tick box as the runs of set 05 left it, and press `Run NWFs`. If a document is open, a question names what is discarded, press OK
588. Look for, in a block headed `RUN SETTINGS`: `NWFs picked       :` the copy's path `, one file`, `include subfolders: not applicable, one file was picked`, `NWFs found        : 1`, `NWFs refused      : 0`, a line beginning `each NWF          : opened where it sits and run the way the open file run runs it`, `NWD               : beside each NWF with the extension swapped`, `reports           : in Clash Reports beside each NWF`, `source folder     : not applicable, nothing is scanned`, `grouping          : not applicable, each NWF is one group`, then the lines from `apply file to old:` to `report units     :` the scanned run writes
589. Look for, after it: `OPEN     no XML picked, so the tests saved in the document run, or nothing runs when it holds none`, then the TEAMS lines of the kept map
590. Look for, in the group: `GROUP    started  1104-PAR-1A02MM` with the rest of the copy's name `  its files are read once its NWF opens`, then `OPEN     running a picked NWF, opened by this run where it sits, no scan`, `OPEN     file` with the copy's path, `OPEN     NWD` with the same path ending `.nwd`, `OPEN     report` ending `(beside the file, no Excel folder picked)`
591. Look for, then: a `DECIDE` step holding `OPEN     opening the picked NWF` and the load wait line, its closing `STEP` line ending `Weekly run`, then the census line naming what the document held. No `CLEAR` line and no `APPEND` line anywhere in the group. If instead the group reads `opened with no error and then reported no models at all`, the copy did not find its NWCs, which answers the UNKNOWN above: then the `DECIDE` step's closing `STEP` line ends `opened and read empty`, the `GROUP    finished` line reads `FAILED` and `Stopped (the NWF read empty)` and never `Weekly run`, RESULT reads `NWF read empty : 1` under the path counts, and the next step is to send the log
592. Look for: the clash step saying it runs the tests saved in the document, the same count of tests as set 05's run of 1A02MM left in its NWF, and the group's `GROUP    finished` line reading `DONE` and `Weekly run`
593. Look for, after the group: a block headed `PICKED NWF` whose lines read `decision      : picked NWF, opened by this run where it sits, its file list not compared, nothing appended and nothing cleared` and `grouping      : not applicable, each picked NWF is one group`, then `SOURCE   findings skipped, the picked NWFs have no scanned source folder to compare against`
594. Look for, in `RESULT`: `groups done    : 1`, and right under `groups failed  : 0` the line `NWFs picked    : 1 from` the copy's path `, each run as the open file run runs it`, and among the files written the NWD in `one` and the workbook in `one\Clash Reports`, and no path under set 05's own folders
595. In Explorer: `one` holds the copy, an NWD of the same name and a `Clash Reports` folder with the workbook, and the NWF and NWD set 05 wrote carry the times they had before step 587
596. Press `Folder`, pick `f129` itself, keep `Include subfolders` ticked. Look for, at the bottom of the step: `2 NWFs found in` the path of `f129` `, 2 can run.`
597. Pick in the Clash XML box the corrected XML set 05 picked and press `Run NWFs`
598. Look for: `NWFs picked       :` the path of `f129` `, a folder`, `include subfolders: yes`, `NWFs found        : 2`, then `OPEN     clash file` with the XML, then two groups, 1A02MM first and 1A04PK second, each with the lines of steps 590 and 591 and the tests of the XML found already there and left alone, since set 05 put them in
599. Look for, in the second group of that run, 1A04PK's: its census line straight after the `DECIDE` step names the same models, sets, tests, results and views as the census line of the same copy run on its own, which you get by pressing `File`, picking the copy in `two` and pressing `Run NWFs` once more after step 605, and its workbook in `two\Clash Reports` carries only the tests 1A04PK's NWF holds and none named only in 1A02MM's, so nothing of the first NWF leaks into the second, since nothing is cleared between the two opens
600. Look for, in `RESULT`: `groups done    : 2` and `NWFs picked    : 2 from` the path of `f129`, and a `SETS ACROSS THE RUN` block before it
601. Untick `Include subfolders` and press `Run NWFs` again. Look for: a dialog beginning `The run cannot start.` and the line `RUN      refused before starting. The folder` the path of `f129` ` holds no NWF, and its subfolders were not read, because the box to include them is unticked.`, and no group started
602. Tick `Include subfolders` again, make a folder `three` in `f129` and copy the 1A02MM copy of `one` into it
603. Pick a folder of your own in the Excel folder box on step 3 and press `Run NWFs`. Look for: the dialog and a `RUN      refused before starting.` line naming the copy in `one` and the copy in `three` `would both write` the workbook path in your Excel folder `, so the second would write over the first`, and no group started
604. Clear the Excel folder box and delete the folder `three`
605. Type into the box the path of the NWD step 587 wrote in `one` and press `Run NWFs`. Look for: the dialog and the line `RUN      refused before starting. This document is a .nwd file and this tool runs an NWF, which is where the clash tests and their results live. Open the NWF instead.`, and no group started
606. Send the log file named on the `log file` line of the SESSION block, say which of steps 585 to 605 did not read as written, and read off the `TIMING` blocks the `DECIDE` seconds of each picked NWF beside the `DECIDE` seconds of the same group in set 05's runs

## Proof F109 the install for the team, added 2026-10-11

The install's four ends, its links guard and its read of what an earlier run left were proved
on the laptop against a throwaway APPDATA, never yours, turn6\f109b-proof. These steps read the
real install once F109 has merged, with Navisworks closed. INSTALL.md is what a person on the
team follows, and these steps follow it.

607. Close Navisworks Manage 2025, pull main in GitHub Desktop once F109 has merged, and build, steps 7 to 11
608. Install, step 22
609. Before you type anything else, since the next step prints a line of its own, look for, as the terminal's last line: `Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator.`, and above it `The installed add-in reads the stamp` with main's eight character commit and `the one the build wrote`, then `every reference is satisfied, 14 assemblies checked. Navisworks supplies its own.`
610. Then run `$LASTEXITCODE` in the same terminal. Look for: `0`
611. Open File Explorer, paste `%APPDATA%\Autodesk\ApplicationPlugins` into its address bar. Look for: `ParsonsNwcFederator.bundle`, and NO folder whose name starts `ParsonsNwcFederator.bundle.replaced-` or `ParsonsNwcFederator.bundle.failed-`
612. If step 610 read `3`, the last line starts `LEFT:` and names two folders, the new add-in first, which stays, and the add-in installed before. The line above it starting `left beside the add-in:` names the old one alone, and that is the one to delete. Do INSTALL.md step 9 and send the whole output
613. If step 610 read `2`, the last line starts `REFUSED:`. Where it reads `the installed add-in could not be moved aside`, that is the refusal of 2026-10-01 with no Navisworks running, which this pass does not change, so send the whole output and stop. Otherwise do what it says, INSTALL.md step 10, and send the whole output
614. If step 610 read `1`, the last line starts `FAILED:`. Send the whole output and stop
615. Open Navisworks. Look for: Parsons NWC Federator on the Tool Add-ins tab, and main's commit in the title bar

## Proof F140 the team package, added 2026-10-11

NOTHING HERE HAPPENS until you write "make the team package", your order of 11 Oct, Q149, once the
proof runs show the main functions work. The package and its install were proved on the laptop
against throwaway folders, never your own add-in, turn6\f140-proof, and no team zip was made. These
steps try the zip on your own machine the way the team will get it, before anyone on the team gets
it. They replace the add-in you have installed, and step 1034 puts it back. A double click, the
question Windows may ask before it runs a file from the internet, and a policy set on your machine
are not proved on the laptop, so steps 1025, 632 and 1030 read them.

1023. Write "make the team package" in the chat. The lead runs `build\package.ps1` on the laptop from a clean checkout of main, into a folder outside the repo, and pastes its last line, which starts `PACKAGED:` and names the zip, `ParsonsNwcFederator-1.0.0.0-` then main's eight characters, a dash, the day's date and `.zip`
1024. Close Navisworks Manage 2025
1025. Open Windows PowerShell on your machine and run `Get-ExecutionPolicy -List`. Look for: `MachinePolicy` and `UserPolicy` both reading `Undefined`, as they read on the laptop on 2026-10-11. Any other value there is a Group Policy that comes before the Bypass the install starts PowerShell with, and whether the install then runs is UNKNOWN, so send what it reads before you go on
1026. Get the zip the way the team will: have the lead send it to you in a Teams chat, or send it to yourself there, and save it from Teams into your Downloads folder. Right click the saved zip and choose Properties. Say whether the General tab shows a line about the file coming from another computer with an Unblock box. Do not tick it, and close Properties
1027. Right click the zip and choose Extract All, then open the folder that makes
1028. Open `VERSION.txt`. Look for: `commit  :` with main's eight characters the lead named, and `date    :` with the day the zip was made
1029. Open `INSTALL-TEAM.txt`. Look for: a page you would hand a person who is not a developer. Say any line that is not plain to you
1030. Double click `Install Parsons NWC Federator.cmd`. If Windows asks whether to run it, choose Run, and say that it asked and what its window said. Look for, as the last line before `Press any key to continue`: `DONE: Parsons NWC Federator version 1.0.0.0, commit` with main's eight characters, `is installed at` your `AppData\Roaming\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle`, then `The version installed before,` with the commit you had, `is kept at` the same path ending `.previous`. If the window ends on red text that starts with none of DONE, FAILED and REFUSED, send the whole window and stop
1031. Press any key. Open File Explorer and paste `%APPDATA%\Autodesk\ApplicationPlugins` into its address bar. Look for: `ParsonsNwcFederator.bundle` and `ParsonsNwcFederator.bundle.previous`, and NO `ParsonsNwcFederator.bundle.copying`
1032. Start Navisworks Manage 2025. Look for: ONE Parsons NWC Federator on the Tool Add-ins tab, and main's commit in the title bar. Two of it would mean Navisworks loads the `.previous` folder too, which is UNKNOWN until this step, so say so
1033. Close Navisworks
1034. Double click `Put back the previous version.cmd`. Look for, as the last line before `Press any key to continue`: `DONE: the previous version,` with the commit you had before step 1030, `is back at` the same `ParsonsNwcFederator.bundle`, then `The version that was installed,` with main's commit, `was replaced and is not kept.`
1035. Press any key. In the File Explorer window of step 1031, press F5. Look for: `ParsonsNwcFederator.bundle` and `ParsonsNwcFederator.bundle.previous`, and NO `ParsonsNwcFederator.bundle.before-put-back` and NO `ParsonsNwcFederator.bundle.copying`
1036. Start Navisworks. Look for: the commit you had before step 1030 in the title bar. Close Navisworks
1037. Double click `Install Parsons NWC Federator.cmd` again. Look for: `DONE:` with main's commit, and the commit you had before step 1030 kept at `.previous`
1038. Send the text of every window whose last line did not read as written, and say whether the zip goes to the team. Nobody on the team gets it before you say so

## Proof F34, the window, and F33, the units combo

24. Close Navisworks if it is open
25. Open Navisworks Manage 2025 with nothing open
26. Open the add-in from the ribbon
27. Look for: the window title carries today's commit and build time, not be0b9b37
28. Look for: the window has four tabs, NUMBERED, reading `1. Source`, `2. Grouping`, `3. Outputs` and `4. Clash`
29. Go to the Outputs step
30. Look for: the Model units combo lists `Metres, which is what the models are set to`, then Millimetres, Centimetres, Feet, Inches, in that order, with the first chosen
31. Look for: the grey line under it says the report is always in metres
32. Go to the Clash step
33. Look for: there is no Outstanding counts combo on the Clash step. It used to sit under the tick boxes
34. Look for: the two buttons read `Sets into open model` and `Tests into open model`, BESIDE the line, on one row with the line to their LEFT, which the XAML settles: the TextBlock and the two Buttons are the three children of one horizontal StackPanel. The step said UNDER and the labels and the line are exactly right, which is F57. The line is `Not part of the run. Try the file above against the model open right now:`
35. Pick the clash XML, 1104-PAR_CLASH_AllInOne
36. Look for: the line at the very BOTTOM of the Clash step, just BELOW the sets pane and NOT under the file box, ends with `Every locator resolves against its sets.` The sentence goes into `SetsSummary`, which is docked Bottom, so it is below the pickers, the tick boxes and all four bordered panels
37. Look for: the log pane holds a block headed `HEALTH 1104-PAR_CLASH_AllInOne.xml` with `Tests: 1830`, `Sets: 61`, a `Locators resolved` line whose two numbers are equal F84 added four more lines to that block since this step was written: `Sets asking exactly the same question:`, `Revit categories known: 374`, `Sets asking for a category no model carries:` and `Set names breaking their folder's pattern:`, so the three above are no longer the whole block.
38. Clear the clash XML box

## Proof F27, the GROUPS block tells the truth

39. Go to the Source step, pick the same C06 NWC folder as last time and press Scan
40. Look for: the count line under Scan reads files found, files readable, groups and the findings by kind, then the findings themselves
41. Go to the Outputs step and pick the same NWF folder and NWD folder as last time
42. Go to the Grouping step
43. Untick two groups, 1B06PH and 1B06G1, which is the only thing in this tool that drops a group from a run
44. Do not press Run here. The GROUPS block is written when a run actually STARTS, so a Run followed by a Cancel leaves nothing to read, and the preview behind that Run opens each NWF and leaves the last one open, which would stop the next proof counting the rebuilt groups
45. Tick 1B06PH and 1B06G1 again, so the next proof runs every group
46. Look for, F27, later: the GROUPS block of the one building run at step 76 lists that building with `run`, the other NINE on C02 or ELEVEN on C04 with `unticked`, and the word `skipped` nowhere
47. Look for, F27, in the same block: one line after the list reads `9 groups unticked in the Run column, nothing else drops a group` on C02, or `11 groups` on C04
48. Go on to the next proof with every group ticked and nothing open in Navisworks

## Proof F24, the six CHANGED groups are rebuilt from the scan, with F29 and F30 in the same run

49. Leave the clash XML box empty
50. Tick every group and press Run
51. Look for: the label at the bottom reads `Checking NWF 1 of 10: 1000BS` on C02, or `1 of 12: 1A0415` on C04, and counts up with the BUILDING changing on each, then the confirm dialog opens. The count is the ticked group count and the name comes after a colon
52. Look for: the dialog says `Rebuilt: 6` and that the NWF is BROUGHT UP TO DATE, what is gone taken out and what is new appended WITHOUT clearing it, so the sets, the tests, the results and the viewpoints never leave it, with the clear and rebuild named as the fallback. IT SAID "cleared and rebuilt" UNTIL 2026-09-21, which stopped being true when PART 5 made the reshape the path a CHANGED group takes, and this is the one screen you can still cancel from
53. Press Cancel
54. Look for: the log says `Run cancelled before anything was cleared.`
55. Go to the Grouping step
56. Look for: 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE read Rebuilt in the Run as column, and the other eight read Weekly run
57. Press Run again, then OK, and wait for it to finish. The confirm dialog now also says the NWF the preview opened will be discarded, which is right, and the preview does not run a second time for the same reason
58. Look for, F50: per rebuilt group the log has a CHANGED line with the counts, then a REBUILT block, `REBUILT <nwf>  1 added, 4 moved, 0 removed` for 1B06BC, then one line per file saying added, moved or removed, then an `APPEND   attempt` line per file coming in, then ONE `RESHAPE` line saying how many were taken out and how many appended `without clearing the document`, then FOUR lines together, starting `SETS`, `TESTS`, `VIEWS` and `RESULTS`. The RESHAPE line is PART 5's and did not exist before the drift round: a CHANGED group is brought up to date without clearing now, and the clear and rebuild is only the fallback. Before F50 there were two tally lines and they read in two different shapes
59. Look for, F50: each of those four says `kept` or `none`, and never `LOST` and never `NOT COUNTED`. Any one of them saying either means the NWF on disk was left alone on purpose. The `LOST` line says so ITSELF, ending `The NWF on disk was NOT saved over, so it keeps them`, and only the clear and rebuild fallback adds a separate `NWF      NOT saved over` line under the four. On the reshape path that now runs, nothing is logged after the tally and the reason is on the group and in the RESULT block
60. Look for, F29 and F50: the `SETS` line reads one of two shapes and BOTH are right. Where the clear kept the sets it carries TWO numbers, `kept: before clear N, after appends N, nothing to put back`, which is the ordinary case. Where the count dropped and the copy was put back it carries THREE, `kept: before clear N, after appends N, after restore N, put back from the copy`, and then the first and the third have to agree. The other three lines read the same way, because four things reading four ways is how a log stops being read. A line reading `LOST` or `NOT COUNTED` is the fault, and step 59 is where that is checked
61. Look for, F33: per group a `UNITS` line. Where the document was NOT already in the wanted unit it says how many models were set and what the document shows, in the words `model set` or `models set`. Where it already was, the one line reads `the document is already in <unit>, so nothing was changed.` and that is not a fault, it is the branch a weekly run over NWFs this tool has already set takes, so expect that one. THE OLD SECOND HALF OF THIS STEP IS GONE. It said to check that no line anywhere reads DID NOT FOLLOW, and that string is in no source file, so it was a check that could never fire. The words went when it was measured that a MODEL can be set and a DOCUMENT cannot, and that what Document.Units reports is UNKNOWN
62. Look for, F30 and F52: per group, after the models are in, the blocks come in this order: UNITS, then `ALIGNMENT` and `EXPORT CHECK`, which the alignment round added and which are read where every model is open and nothing has clashed, then CLASH, then `EMPTY SETS` where any set found nothing, which PART 3 added and which on his folders is every group with the XML picked, then `PENETRATION` and `BY DESIGN` where their boxes are on, then `ITEM IDS` where the report carries item ids, then `VIEWS` and `VIEWS BUILT`, which are TWO blocks and not one, then `NWF      attempt`, then XLSX and `WORKBOOK CHECK`, then the page and `REPORT CHECK`, then XML where its box is on, then NWD, then CONFIRM, the final NWF line saying intact and the size, and the `GAP` block. It is XML there and NOT a second XLSX, which this step named twice. The `VIEWS` line sits between the clash step and the NWF because F52 makes the viewpoints there, so a viewpoint this run made is inside the file the NWD is published from. No SETS block follows UNITS on this run, because sets come from the clash XML and the box is empty. The four SETS, TESTS, VIEWS and RESULTS lines the rebuilt groups carry are written during the rebuild, before UNITS. SINCE F114 MERGED THE BOX OPENS TICKED, Q131, and the views blocks are FOUR, `VIEWS`, `VIEWS BUILT`, `VIEWS INVENTORY` and `VIEWS TREE`, steps 509 to 515
63. Look for: the GROUP finished line of each of the six reads the building, then `DONE`, then the seconds, and ends with `Rebuilt`. UNTIL 2026-09-21 IT READ `PARTIAL` AND `Skipped (changed on disk)`, because the reshape never said it had rebuilt the group and the outcome kept the value it was given BEFORE the work
64. Look for: the RESULT block carries `rebuilt        : 6` and `weekly run     : 4` on C02, which is ten groups in all. Until 2026-09-21 it read `rebuilt        : 0` and counted them under `skipped`
65. In Explorer open the NWF folder and open the NWF of 1B06BC in Navisworks
66. Look for: the Selection Tree lists 5 models, EL among them, all from the Published folder
67. Look for, F29: the Sets window holds the same sets as it did before the run, Mechanical with its four subfolders among them
68. Open the NWD of 1B06BC from the NWD folder
69. Look for: the same 5 models
70. Look for: no group in this run reads PARTIAL. Every RESHAPED group read PARTIAL until 2026-09-21 with the reason "the NWF points at a different set of files, so it was left alone", about a group that had just been brought up to date
71. Close the NWD and the NWF so nothing is open

## Proof F28 and F31, one building twice with the XML

72. Go to the Clash step and pick the clash XML
73. Go to the Grouping step and tick 1B06PH only
74. Press Run
75. Look for: the confirm dialog says `Weekly run plus XML: 1` and its line ends with `Nothing inside it is cleared.`, which is the wording since F75 and not the older `Nothing is cleared.`
76. Press OK and wait for it to finish
77. Note the time the run took, from the GROUP started line to the GROUP finished line
78. Look for: the log says OPENED for the group
79. Look for: one line per set starting `SET      `, each ending `already there, left alone` where the NWF already held it from last week, and one line `SETS     <building> put into the document: 0 created, 61 already there and left alone`. THERE IS NO PER GROUP SETS BLOCK IN THE LOG. F81 deleted it, because SetBuilder already writes a line per set as it goes, and the `sets created      :` and `already there     :` totals only reach the window's sets pane, from the Sets into open model button
80. Look for: the line `CLASH    source   tests from XML` and the tests running after it
81. Press Run again on the same building, same folders, same XML
82. Press OK and wait for it to finish
83. Look for, F28: the line `SETS     <building> put into the document: 0 created, 61 already there and left alone`, and a `SET      ` line for every one of the 61 ending `already there, left alone`. THE TOTALS AND THE Q72 LINES ARE NOT IN THE LOG. `sets created      : 0`, `already there     : 61, left alone, not copied again` and the three indented Q72 lines under them are in the window's SETS PANE and only when the Sets into open model button is pressed. In the pane, the third of those Q72 lines now says what this run FOUND, either `none of them drifted`, or `N of them DRIFTED and none was rebuilt, because the box is off`, or `N of them DRIFTED and M were REBUILT from the picked file`. It used to say a set finding nothing MAY be asking a question the file no longer asks, which stopped being true when PART 3 read the question and PART 2 rebuilt it.
84. Look for, F28: the line `SETS     <building> put into the document: 0 created, 61 already there and left alone`. It is a line of its own and not part of a SETS block, because there is no SETS block in the log since F81. ONE `NWF      attempt` line follows the CLASH block and not a second one, which is F57. This group is a Weekly run plus XML, so it takes the OPENED branch, and that branch logs `NWF      reused` and never an attempt BEFORE the clash step, so the one after it is the only one and their results went into the document
85. Look for, F31: the DRIFT block reports no locator difference on any test. A locator difference on every test would mean the source held since the sets were indexed does not compare equal to a fresh one, which is the one thing F31 could not measure here
86. Look for, F31: the CLASH block shows the same created, already there, run, skipped and passed counts as the first run
87. Look for, F31: the second run took no longer than the first

## Proof F35, a group of one discipline creates every test and runs none

88. In Explorer make a folder on the desktop named OneDiscipline
89. Copy the AR NWC of 1B06PH into it
90. Copy it again inside that folder and rename the copy so its number reads 000002 instead of 000001, so the folder holds two AR files of one building
91. In the add-in go to the Source step, pick the OneDiscipline folder and press Scan
92. Look for: the findings say SINGLE DISCIPLINE for 1B06PH
93. Go to the Grouping step
94. Look for: the status of `1A04MS` reads `Ready. One discipline, so every test is created and none is run.` THAT STRING IS NOW FALSE AND IT IS THE CODE THAT IS WRONG, not this step. Since F77 a test whose side finds nothing is never created, and in a one discipline group that is nearly all of them, which is what step 98 measures. The CLASH line was corrected and this window label was not. Report it
95. Go to the Outputs step and pick a new empty folder on the desktop for the NWF and the NWD, so nothing under C06 is touched
96. Leave the clash XML picked and press Run, then OK, and wait for it to finish
97. Look for: the log line `CLASH    1B06PH holds one discipline, so no test is run, and only the tests whose sides both find something are created. One discipline cannot clash with itself.` This step used to quote `so every test is created and none is run`, which the code has never written since F77, so it was a check that could never pass
98. Look for: one line `CLASH 1830 in the file, N created, M not created, a side finds nothing`, and in the CLASH block two skip reasons with counts, `the group holds one discipline, so nothing in it can clash` for every test that WAS created and `a side finds nothing in this model` for the rest. SINCE F77 THE CREATED COUNT IS FAR BELOW 1830, because a test whose side finds nothing is never created at all, so the two reasons split the 1830 between them. This step used to say 1830 created and 1830 skipped, which F77 made impossible
99. Look for: the GROUP finished line reads the building, then `DONE`, then the seconds, then which path it took. `DONE` is never the last word on the line
100. Put the Source folder back to the C06 NWC folder and press Scan
101. Put the NWF folder and the NWD folder back to the C06 ones

## Proof F26, the report is always in metres

This one is its own small run, with the clash XML, because the conversion only happens where a report is written.

102. Go to the Outputs step
103. Set the Model units combo to Feet, on purpose, so the models are put into feet
104. Go to the Grouping step and tick 1B06PH only
105. Press Run, then OK, and wait for it to finish
106. Look for: the line `model units      : Feet, which is what the models are set to`
107. Look for: the line `report units     : Meters (m), always, converted before anything is written`
108. Look for: one line `UNITS    every number converted from ft to m, one ft is 0.3048 m, ...` before the XLSX line
109. Open the workbook in the Clash Reports folder beside the NWF folder
110. Look for: the Tolerance cell of the first block reads `0.075m`, not `0.246ft`. This assumes the ORIGINAL 75 mm XML with the tolerance drop down left on `Use the value in the XML`. F76 added that drop down and Q66 made 25 mm the project standard, so a run at 25 mm reads `0.025m` here instead.
111. Look for: the Distance column holds metres, so a hard clash reads about -0.050, not -0.164. The same caveat as the step above: this is the 75 mm XML.
112. In Navisworks open Clash Detective and select that test
113. In Options, Interface, Display Units, set Linear Units to Metres
114. Look for: the panel tolerance reads 0.075 and one clash distance matches its workbook row to three decimals
115. Go back to the Outputs step and set Model units to Metres

## Proof F22, the labels before a run

116. Go to the Outputs step and pick an NWF folder that holds no NWF yet
117. Go back to the Grouping step
118. Look for: every group reads First run
119. Go to the Clash step and pick the clash XML
120. Look for: every group still reads First run, because no NWF is there yet
121. Put the NWF folder back to the C06 one
122. Look for: every group reads Weekly run plus XML

## Proof F71, the window says when an NWF is nearly matched

This is what the run of 2026-09-19 could not tell you. The tool reuses an NWF that is at
the output path and builds one when nothing is there, which is right, and it said nothing
about a file sitting beside it under a nearly identical name.

123. Go to the Outputs step and pick the C06 NWF folder, the one that already holds NWF files
124. Go to the Grouping step and look at the NWF Name column
125. Change the NWF row's Level field on the Outputs step, which is the box named `NwfLevel`, the FIRST box on the NWF row, and not the NWD or workbook Level box UNDER it, from `ZZZ` to `L01`, so every name a group would write now differs from the file on disk in one field only
126. Go back to the Grouping step
127. Look for, F71: every group whose NWF is on disk now reads `First run` in the Run as column, FOLLOWED BY `, but the NWF folder holds a similar name:` and the name of the file that is really there. Before F71 it read `First run` and nothing else, which is what cost the time
128. Look for, F71: one grey line under the group table reading `N groups are set to build a new NWF beside a file whose name is nearly the same.` and then `Type the NWF Name cell over to point at the existing file, or leave it to build a new one.` The number is how many groups are in that state
129. Look for, F71: nothing was changed for you. The Run column is still ticked, the NWF Name cells still hold the new name, and no name was corrected. The tool reports and you decide
130. Type the NWF Name cell of one group over with the name of the file that is really there, without its extension
131. Look for, F71: that row now reads `Weekly run` or `Weekly run plus XML` with no sentence after it, and the line under the table counts one group fewer
132. Put the Level field back to `ZZZ`
133. Look for, F71: every row reads `Weekly run` or `Weekly run plus XML` again and the line under the table is gone
134. Change the Level field to `L01` again and then pick an NWF folder that holds NO NWF at all
135. Look for, F71: every row reads `First run` with no sentence after it, and no line under the table. An empty folder holds nothing to be near, so there is nothing to say
136. Put the NWF folder back to the C06 one and the Level field back to `ZZZ`

## Proof F9, a changed folder is rebuilt and not run in feet

137. Copy one NWC of 1B06PH out of the NWC folder to the desktop, so the folder lost one file
138. Close whatever is open in Navisworks, so the preview can open each NWF, then press Scan again and tick 1B06PH only
139. Press Run
140. Look for: the dialog says `Rebuilt: 1`
141. Press OK
142. Look for: the REBUILT block names the removed file with `removed`
143. Look for: no UNITS line comes before the REBUILT block
144. Copy the NWC back into the NWC folder
145. Press Scan again, tick 1B06PH, press Run, press OK, so the NWF holds all its files again
146. Look for: the REBUILT block names the file with `added`

## Proof F10, each output on its own switch

147. Go to the Outputs step and open More
148. Tick Write a clash XML beside each workbook
149. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
150. Tick 1B06PH and press Run, then OK
151. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
152. Look for: `IMAGES   skipped  images are switched off for this run`
153. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
154. Untick Write a clash XML and tick New, Active and Reviewed again
155. Press Run again, then OK
156. Look for: `XML      skipped  not wanted this run`
157. Look for: the pictures rendered again, counted on their own `IMAGES` line after the CLASH block. The CLASH block counts clashes and never pictures

## Proof F17, the pictures are numbered in the export order

158. Stay on the run just finished
159. Look for: one line `IMAGES   numbered in report order: <n> renamed, <n> already right, 0 missing` after the CLASH block
160. Open the Clash Reports folder beside the NWF folder and open the 1B06PH workbook in Excel
161. In Navisworks open Clash Detective and select the test that is the first block of the workbook, the one with the most clashes
162. Look for: the clashes on the sheet are in the same order the Clash Detective panel lists them
163. Look for: the Image cell on row 1 of that block links to cd000001.jpg, row 2 to cd000002.jpg, and so on
164. Look for: the second block's pictures start at cd010001.jpg
165. Click the Image cell on three rows in three different blocks
166. Look for: every link opens its picture, and the picture shows the two items named on that row
167. In Explorer open the `_files` folder beside the workbook and look for: no file ending `.moving`

## Proof F8 on the scanned run, no XML

168. Clear the clash XML box on the Clash step
169. Go to the Grouping step
170. Look for: 1B06PH reads Weekly run
171. Press Run, then OK
172. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
173. Look for: the CLASH block says the tests ran. IT SAYS NOTHING ABOUT THE WORKBOOK, which is F57: the block is `ClashRunOutcome.Lines` and it names no output file at all. The workbook has its own `XLSX     attempt` and `XLSX     written` lines after it
174. Look for: no `SET      ` line of a set of a file INSIDE the group, because no XML means the sets of the document are left alone, and only the `SET` lines whose path begins `lcop_selection_set_tree/Generic Models/`, which F128 builds with an XML or without one. The `SETS ACROSS THE RUN` block at the END of the run is still written, unconditionally, and with no XML the sets step counts no set of a file, so it reads zeros and `No set was counted, so nothing is said of what the sets found.`, which is right and is not a failure

## Proof F32, an NWD open is refused

175. In Navisworks open the NWD of 1B06PH from the NWD folder
176. Open the add-in from the ribbon and go to the Clash step
177. Look for: the line BESIDE Run the open file, to its right, reads `This document is a .nwd file and this tool runs an NWF, which is where the clash tests and their results live. Open the NWF instead.` and the Run the open file button is greyed
178. Close the NWD

## Proof F6, the open file report folder, with F30 and F32 on the NWF

179. In Navisworks open the NWF of 1B06PH
180. Open the add-in from the ribbon
181. Go to the Clash step
182. Look for, F32: the blue line BESIDE Run the open file, to its right, starts with `Weekly run.`, names the NWD it will write beside the NWF and names one Clash Reports folder beside the NWF, and the button is enabled
183. Leave the clash XML box empty
184. Press Run the open file and wait for it to finish
185. Look for, F30: after the models are in, the blocks come in the same order as on the scanned run: UNITS, then `ALIGNMENT` and `EXPORT CHECK`, which the alignment round put on the one path BOTH runs pass through so the open file run writes them too, then CLASH, then `EMPTY SETS` where any set found nothing, then the `VIEWS` blocks, then `NWF      attempt`, then XLSX and `WORKBOOK CHECK`, then the page and `REPORT CHECK`, then XML where its box is on, then NWD, then the final NWF line. It is XML there and NOT a second XLSX, which this step named twice. SINCE F114 MERGED THE BOX OPENS TICKED, Q131, so the `VIEWS` blocks are the four of step 62
186. Open the NWF's folder in Explorer
187. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
188. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

189. Stay on the run just finished
190. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
191. Look for: the CLASH block says the tests ran, and the workbook is on its own `XLSX     attempt` and `XLSX     written` lines after it, never in the CLASH block, which is F57

## Proof F7, the RESULT block and the log copy

192. Stay in the same NWF folder in Explorer
193. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the Run the open file press
194. Open that log in Notepad
195. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
196. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
197. Look for: a RESULT block at the end with groups done, partial and failed, and NO `viewpoints     :` line under them, which is written only where the box was unticked, and since F114 merged it opens ticked
198. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

199. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
200. Copy every `run-*.log` written today
201. Paste them into `steps\logs` in the repo folder
202. Rename each with the DATE OF THE RUN, the building and the press. The shape, with 2026-09-14 standing for whatever day you run them: `2026-09-14-C06-rebuilt.log`, `2026-09-14-1B06PH-first.log`, `2026-09-14-1B06PH-second.log`, `2026-09-14-onediscipline.log`, `2026-09-14-1B06PH-noxml.log` and `2026-09-14-1B06PH-openfile.log`
203. Open GitHub Desktop
204. Press New Branch, name it `logs-` and the date of the run, so `logs-2026-09-14` on the fourteenth, and press Create Branch
205. Write a summary like `logs from C06, F34 to F7 proofs`
206. Press Commit
207. Press Publish branch
208. Press Create Pull Request, and tell the worker in the chat that it is open. The worker merges it and reads the logs. Nothing goes to main directly, that is the wall F38 put up

## Run the window probes

209. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-defaults.ps1
```

210. Look for: it prints the window defaults and no error about a path
211. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-labels.ps1
```

212. Look for: it says how many tick boxes are visible without opening anything, which must be 4: `Include subfolders` on the Source step, and `Mark penetrations as Reviewed`, `Mark by design connections as Reviewed` and `Rebuild sets that drifted from the file` on the Clash step. It was 3 until the drift round added the rebuild box under the by design one, Q72, outside the destroy data expander on purpose because 5v measured that it destroys nothing. It was 2 until F72b added the by design box beside the penetration one, and reports no label over eight words and no help line over twelve. It was 1 before F72 added `Mark penetrations as Reviewed` to the Clash step, which is visible on purpose because a weekly run genuinely has to choose whether this run writes statuses into the NWF

## Proof F41, the handles, off the two runs above

213. Before you press Run the second time in step 81, open Task Manager, the Details tab, right click a column heading, Select columns, and tick Handles
214. Look for: the handle count on the Navisworks row at the end of the second run, beside the one at the end of the first. Copy both into the chat. F41 releases every wrapper this tool creates where it is finished with it instead of leaving it to a finalizer, so a second run over the same 61 sets and 1830 tests should not sit higher than the first. A count that climbs run on run is the thing worth reporting
215. Look for, in the log of either run: no line saying a set was not at the index the count gave before the add. That line is written only when the tree is not the shape the fast read assumes, and it means the set was then looked for by name

## Proof F45, the clash step keeps its rules, off the two runs above

216. Look for, in the DRIFT block of either run: either no line about sides left out at all, or one saying how many sides were left out because which set one end points at could not be read. That line is new and says not compared is not the same as matching
217. Look for: a block headed `ITEM IDS 1B06PH`, one line per property that supplied an item id with a count out of the total, and never a line per item. Each of THOSE lines ends `written as "Element ID"`, and a line reading `no id property` is the items that carried none, which is not a property name. WHERE ANY ID IS MISSING there is ONE MORE line under them, counting the missing ones as this run's, carried over, or on a row with no date at all, and that one does NOT end that way, it ends `which is UNKNOWN and not carried over`. Expect `Id` to supply most of them, because `Id` is the first name looked for and `Element ID` is the label this tool chooses
218. Look for, if any `CLASH    OLD` line appears: it reads the test name and `Navisworks has this test marked Old.` and nothing after that. It used to carry a sentence about what Old means, which is UNKNOWN
219. Look for, only if you tick compacting on the Clash step: the compacted line carries both numbers, the Resolved count before and the count read again after, and the number reported as removed is the difference between them

## Two probes, F50 and F52. BOTH ALREADY RUN on 2026-09-19

Both were run on this machine on 2026-09-19 and both answers are in
`docs\history\scan.md`, 5c and 5d, with the assembly version and the date. Sections 5a and
5b still hold the questions, because the reasoning in them is why the answers matter.

Nothing below is outstanding. The four steps are kept so the answers can be checked rather
than trusted, and because the probes are the right thing to rerun when Navisworks is
upgraded. Each takes about ten seconds, neither needs the add-in built and neither opens a
model.

220. Open PowerShell in the repo folder and run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-model-remove.ps1
```

221. Look for, under `THE QUESTION: anything anywhere that takes a Model or an index and removes it`: two members on `Autodesk.Navisworks.Api.Document`, `public Void RemoveFile(Int32 index)` and `public Boolean TryRemoveFile(Int32 index)`. So a model CAN be taken out without a clear, and the member is on `Document` and not on `DocumentModels`, which is why searching the collection for it found nothing. `DocumentModels` carries only `InternalRemove` and `InternalRemoveAt`. If this list is now EMPTY, the install has changed and `scan.md` 5c is wrong
222. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-viewpoints.ps1
```

223. Look for: a block headed `The viewpoint collection, every member` listing `Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints`. It must NOT say `UNKNOWN: no type named ...DocumentSavedViewpoints`, and on 2026-09-19 it did not, so the name the code uses is right
224. Look for, in the same output: `public FolderItem RootItem`, `public Void AddCopy(GroupItem parent, SavedItem item)` and `public Void EditDisplayName(SavedItem item, String newDisplayName)`. All three were there, so a folder is made, a viewpoint goes in it and a name can be set. `SavedViewpoint` reads `IDisposable: True` and has a `SavedViewpoint(Viewpoint viewpoint)` constructor. The sets collection printed beside it carries the same members with the same signatures, which is the comparison the probe exists to make
225. DONE in the viewpoints round on 2026-09-19, the way steps 374 and 375 read. This said `SavedViewpoints.CanBuild` is still false and that whether a saved viewpoint records hiding could not be read off a DLL. It was measured on a real run instead, scan.md 5j to 5m, and the answer is that NEITHER .NET route records both: `new SavedViewpoint(Viewpoint)` keeps the camera alone and `CaptureRuntimeOverrides` keeps the hidden state and no camera. The COM view with `ApplyHideAttribs` records both and is what the tool writes through. `CanBuild` has been a hard true since. The flags this step names are worthless as a read back, 5o: both read TRUE on a viewpoint that recorded nothing.

## Proof F51, the NWD carries May be re-saved and its properties reach ACC

Every NWD this tool has published so far shows a processing error beside it in ACC and
in Forma. Autodesk say an NWD published without May be re-saved cannot be translated by
that viewer, and they have a second article about an NWD reaching ACC with no properties
and every object showing as solid. F51 sets three things on the publish that were never
set: `AllowResave` true, `EmbedDatabaseProperties` true and `PreventObjectPropertyExport`
false. All three were already on the list read off the DLL on 2026-08-29.

This proof needs one NWD and an upload. It is the only thing in this file that needs a
browser as well as Navisworks.

226. Run any one building the ordinary way, so a fresh NWD is published
227. Look for: one line in the log reading `NWD      publish properties set:` and then seven names, ending `AllowResave=true, EmbedDatabaseProperties=true, PreventObjectPropertyExport=false`. If that line is missing, the build is older than F51 and nothing below proves anything
228. Open the published NWD in Navisworks on your own machine, then File, then look at the file properties
229. Look for: the NWD opens and the title, the publisher, the subject and the author read as the log line says they were set
230. Upload that NWD to ACC, into whatever folder the project uses
231. Look for: no processing error and no warning triangle beside it once the translation finishes. That is the whole point of `AllowResave`. A warning still there means the flag did not take and the answer is a screenshot of the warning and the log line from step 227
232. Open the NWD in the ACC viewer and click one object, any object
233. Look for: a properties panel with real Revit properties in it, not one row reading Solid. Object properties missing means `EmbedDatabaseProperties` did not carry them and `PreventObjectPropertyExport` is the next thing to look at
   Answer: Bader, 2026-10-07, his message headed BADER'S ANSWERS, 7 OCT 2026, THE SIX ROWS WAITING FOR HIM AND THE SHUTDOWN TIME: steps 228 to 233, the published NWD checked in ACC, Bader does himself at the final run, after F114 merges. The row waits for him until then.
234. Q32 is whether the NWF needs to be in ACC at all. Nothing appears beside an NWF because ACC does not translate one: an NWF holds no geometry, only pointers to the NWCs, so there is no viewable file to make. Answer it in `steps/02_questions.md` when you have seen the NWD work

## Proof F52, a viewpoint per discipline

Two halves. The first is true today. The second no longer waits on a probe, because the
probe was run on 2026-09-19 and the API is measured in `docs\history\scan.md` 5d. It waits
on the WRITING half of F52 being built against what was measured, which is a decision and
not a measurement, so `SavedViewpoints.CanBuild` is still false. Do the first on the next
ordinary run and the second once that half exists and the add-in has been rebuilt.
SINCE THE VIEWPOINTS ROUND ON 2026-09-19 the second half exists, `SavedViewpoints.CanBuild` is
true and the `not attempted` line of step 237 is gone. Steps 374 and 375 carry what was done
and the viewpoints round entry in `steps\log.md` carries the run.

235. Look for, and THIS STEP WAS REWRITTEN ON 2026-10-08 because F114 changed what a view is: a `VIEWS` block per group with the plan's lines, one view per clash test with an open clash, the open clashes counted and the clashes left out counted by status, then the tests whose side reads UNKNOWN and the clashes whose size could not be read, each named. There is ONE VIEW PER CLASH TEST now, not one per clash and not one per discipline, so a group plans tens and not hundreds, and the paths are in the `VIEWS BUILT` and `VIEWS TREE` blocks
236. Look for: a group with only one discipline plans NONE and says so, because no test is run there and a test with no open clash gets no view. The block still gets written, which is the part that has not changed: a group with nothing to plan says so rather than going quiet, and its `VIEWS BUILT` block reads `No view was planned for this group.`
237. GONE. It said to look for a line reading `VIEWS    not attempted.` while `SavedViewpoints.CanBuild` was false. That flag has been a hard true since the viewpoints round on 2026-09-19 and the string is in no source file, so this was a check that could never fire. Steps 383 to 385 and 391 are what prove the viewpoints now
238. DONE in the viewpoints round on 2026-09-19 and proved on every run since. This said to wait for the writing half of F52 to be built, and it was. Run one building again
239. Look for: a `VIEWS BUILT` block in the shape of the SETS block, a `VIEW` line per view reading `created, shows <codes>, hides N models`, CAPPED at `RunLog.KeptOfARepeat` with `VIEW     and N more created, counted and not listed` under them, then `views created     :` and `put into the document: N created`. No line reads `already there`, since the views are made fresh every run and the earlier ones removed after
240. Open the NWF in Navisworks and open the Saved Viewpoints panel
241. Look for: the tree of F114. Layer 1 is the priority off the clash matrix, `A`, `B`, `C` or `No priority`, ALWAYS there, `No priority` holding every test when no priority file was picked. Layer 2 is the two TEAMS of the test's sides in the team map's one order, so `Architecture vs Mechanical` holds the tests of both directions and a team against itself is one folder. Layer 3 is one view per test, named exactly as the test, and an `Over 150mm` folder only under a pair carrying Mechanical or Electrical, holding that test's view of its clashes whose larger service is over 150 mm. Steps 518 to 520 walk this shape on a real file
242. Run the SAME building again without changing anything
243. Look for, on the second run of the same group: the `VIEWS INVENTORY` block reading last week's views `removed, this tool's earlier view replaced at its place`, `views created     :` the same count as the first run, and in the panel ONE view per test and never two. A second copy of any view is a fault

## Proof F53, the 150 mm rule and the sub groups

This one is proved in Core here and in the tree on your machine. The Core half needs
nothing from you. The rest WAS waiting on the writing half of F52 and no longer is, because the sub group IS a
viewpoint and the writer has been building them since the viewpoints round on 2026-09-19, scan.md 5d and 5m.

244. Look for: the paths in the `VIEWS BUILT` block, five per group and then `and N more created, counted and not listed`, read `A/Architecture vs Mechanical/Over 150mm/<test>` for the large services and `A/Architecture vs Mechanical/<test>` for the rest, a TEAM pair and never a code pair such as `ME vs ST`, which was F85's shape, and the `VIEWS TREE` block lists every one. A group with no Mechanical or Electrical model has no `Over 150mm`, and that is right
245. Look for: the number in the folder name is the threshold in use. If you change the threshold and it still reads `Over 150mm`, that is a fault and the folder and the rule have drifted apart
246. DONE since the viewpoints round on 2026-09-19. It said to wait for the writing half of F52. Run one Mechanical building
247. GONE. It looked for a `SIZE` block per group that no run ever wrote, and the class that wrote it went in F114. What stands is the `VIEWS` block's lines naming each clash whose size could not be read in a pair carrying the size folder, step 510
248. Look for, and this was rewritten AGAIN by the worksets round because the number changed: `size could not be read : N`. It is EXPECTED TO BE ZERO now, on every group. It used to be large, 59 and 36 and 8 on the run before, because a fitting was thought to carry no size property. 5s found that every one of them did, written as words, `53 mm�` for a conduit and `600 mmx100 mm` per connector for a cable tray fitting, and `ItemSizes` was dropping every string. `Federator.Core.Views.SizeText` reads them now. A clash NAMED here is a size shape `SizeText` has not been taught and is worth sending, and since F114 the line names each one by its test and clash rather than counting them
249. GONE with the number it read. It said to read the names under that line and judge whether they were all fittings. No names are written under it, `ClashViewpointPlan.Lines` writes the count alone, and the count is zero on every group since 5s. Where one ever comes back non zero, the `.tsv` rows are where the names are.
250. Open the NWF and press any view under a folder named `Over 150mm`, which sits under a TEAM pair folder such as `A/Architecture vs Mechanical/Over 150mm`, and never under a code pair such as `A/ME vs ST`, which was F85's shape and which step 244 above already corrects
251. Look for: the view under `Over 150mm` painting only the large pipes, ducts and trays of its test red and green, the view of the same test in the pair folder painting the small ones and every fitting whose size could not be read, and no clash in both. A fitting in neither view means the include on unknown rule is not working and the `VIEWS` block's size lines are the evidence
252. Look for, on a document measured in FEET rather than millimetres: the same clashes under `Over 150mm` and in the pair view as the same building measured in millimetres, and the views framed the same. The conversion is what makes that true, and a difference means the raw number is being compared somewhere

## Proof F54 and F72, penetrations become Reviewed

Q33 is ANSWERED since 2026-09-19 and F72 supplies the list, so this section has two
halves now. The first is the run with the box OFF, which is the default and which has to
change nothing at all. The second is the run with it on.

253. Run any building the ordinary way, with the box on the Clash step left OFF
254. Look for: NO `STATUS` line and NO `PENETRATION` block anywhere in the log. With the box off nothing supplies a list, so either of those on an ordinary run means something is supplying one and that is a fault. A REBUILT group writes a `RESULTS` line, which is F50 counting the clash results it carried across the clear and is nothing to do with this. The two used to share the STATUS prefix and F56 separated them, because one prefix reading as two different things is how a log stops being trusted
255. Look for: the clash counts per test in the workbook still read New, Active, Reviewed, Approved and Resolved as they always did, and no clash has moved
256. Look for, in the RUN SETTINGS block at the top: `penetrations     : no, every clash keeps the status it has`
257. Look for: NO `penetrations   :` line in the RESULT block. A run that never asked for it has nothing to say, and a line reading zero on every run teaches people to skip it
258. Open `docs\workflow.md` and read the section What a status means
259. Look for: it says a clash carries five and a test carries four, that Old is a test word and never a clash word, and that this tool sets Reviewed and nothing else

### Now with the box on

260. Go to the Clash step
261. Look for, F72: a tick box reading `Mark penetrations as Reviewed`, unticked, with one grey line under it reading `Services 150mm and under through walls, floors, roofs, foundations. New and Active`, with foundations since Q63 and no trailing `only`, which the twelve word limit had to give up to name the fourth solid. The 150 is read off the setting, so if you ever change the threshold this line changes with it
262. Look for, F72: it is NOT inside `Things that destroy data`. Nothing is destroyed, only New and Active ever move and a person moves one back in one click
263. Tick it, and run ONE building that has mechanical or electrical services in it
264. Look for, F72: a `PENETRATION` block per group, after the `CLASH` block and after the `EMPTY SETS` block where any set found nothing, which PART 3 put between them
265. Look for, F72: one line per clash moved, starting `REVIEWED `, then the clash name, then the test it is in, then the service category, the size in millimetres, and the solid category. For example a `Pipes 100mm through Walls`. If that line names a category you would not call a service, the list is a setting and it is wrong for this project
266. Look for, F72: after the moved lines, `clashes looked at :` and `moved to Reviewed :`, then SIX INDENTED LINES, one per reason a clash was left alone, including the reasons at zero. The six are the service is over the size, no size could be read, a person had already set it, both sides a service, both sides a solid, and not a service against a solid. `moved to Reviewed` is NOT one of the six: the reason loop skips it on purpose, because it is the count line above them. This step said seven and named moved as one of them, so it could never pass SINCE Q71 TWO MORE LINES sit under the `no size could be read` reason WHEN IT IS ABOVE ZERO, naming what the services are by category and saying each is a row in the machine readable log. On his folders that reason is now zero, so neither line appears.
267. Look for, F72: the line `the size in use   : 150mm or under is a penetration, over it stays as it was`
268. Look for, F72: the number beside `no size could be read off the service, left alone` and READ IT. IT IS ZERO ON EVERY GROUP SINCE THE WORKSETS ROUND, where it read 29, 30, 5 and 1 the run before. That is the count of services this tool could not measure, and every one of them was LEFT AT NEW rather than moved. It goes the opposite way from F53, which INCLUDES an item it could not measure, and both are right: the safe mistake in a viewpoint is showing something unnecessary and the safe mistake here is leaving a clash for a person. If that number is large, the size is on a property this tool is not looking for, and the answer is the property name so it can be added to the setting
269. Look for, F72: TWO lines in the RESULT block beginning `penetrations   :`, not one. The first reads `penetrations   : N clashes moved to Reviewed` and N is the sum of every group's `moved to Reviewed`. The second, straight under it, reads `penetrations   : N clash(es) moved to Reviewed, of M looked at across K groups`, with one indented row under it per group that moved something. TWO LINES SHARING A PREFIX AND SAYING DIFFERENT THINGS is worth reporting on its own

269a. Look for, F72b, where the by design box was on: the same pair again under the `by design      :` label, a total line and then a moved-of-looked-at line with per group rows

269b. Look for, in the RESULT block on EVERY run whatever the boxes say: one line `clashes found  : N across K groups`, with `, J of which found none` on the end where any group found nothing, and then one indented row per group reading the building, the count, and `from N test(s) that found something`. UNLIKE THE PENETRATION AND PRIORITY LINES THIS ONE IS NEVER CONDITIONAL, so a run with no line here at all is a fault. Where no group reported a count it reads `clashes found  : UNKNOWN, no group reported a clash count`, which is not the same as zero

269c. Look for, Q73: the `WORKBOOK CHECK` block's `N test blocks` DISAGREES with the run tail's `WORKBOOK N test blocks`. `WorkbookCheck` finds a block by its `Clash Name` heading row and a test that found nothing no longer has one, so the check can only see the tests that found something. THE WORKBOOK IS RIGHT AND THE CHECK COUNT IS WRONG. Send both numbers

269d. Look for, Q74, where the rebuild box is on and the NWF holds a set the picked file no longer names: a block whose first line reads `N set(s) in this NWF are not named by the picked file:` then three indented counts, `N nothing points at, so they are removed`, `N are the working half of a pair, so the unused half goes and this one takes its name`, `N are pointed at with no twin, so nothing is done about them`, then one line per set. Then one `SET      REMOVED <path>, which nothing pointed at` per removal and one `SET      RENAMED "<old>" to "<new>" and removed the unused set that held that name. Its N test side(s) keep working and now ask what the file asks` per pair. The `SETS     <building> put into the document:` line ends with `, N of M set(s) the file no longer names brought up to date`. A set that IS pointed at and has no twin is NOT touched and says `NOTHING IS DONE, because removing it would leave those sides resolving to nothing`. THE LEFTOVER COUNTS ARE IN THE LOG ONLY and never in the window's sets pane
270. Look for, F72: an `NWF      attempt` line AFTER the clash step, because writing a status is a write and the NWF is saved again on it. SINCE F85 THE VIEWS STEP CAUSES THAT SECOND SAVE TOO, so the line appears on a weekly run with the penetration box off, and its absence is still the fault to report but its presence no longer proves the penetration pass ran. Whether it is the first or the second such line depends on the path this group took, so count them rather than looking for a second: a First run or a Rebuilt group writes one before the clash step as well, and a Weekly run does not, because the opened branch logs `NWF      reused` instead. On a Weekly run the line after the clash step is the ONLY one, and its absence is the fault to report. SINCE F114 MERGED the VIEWS step asks for that save where it wrote a view or removed an earlier one, so a weekly run with the penetration box off still makes it
271. Open the workbook for that building
272. Look for, F72: the Reviewed count on the test header rows has gone UP by the number the block said, and the New or Active count has gone down by the same. That is the count reaching the workbook, in the client's own column, because the status is applied before the harvest reads it. If the workbook shows the OLD status, the status is being applied after the harvest instead of before it, which is the one thing this feature must not do
273. In Navisworks open Clash Detective on that building and pick one clash the block named
274. Look for, F72: it reads Reviewed in the panel, and the two items are a small service and a wall, a floor, a roof or a structural foundation, which Q63 added to the solid list on 2026-09-20
275. Run the SAME building again with the box still on
276. Look for, F72: the second run moves FEWER, probably zero, because the ones it moved last time are now at Reviewed and Reviewed is not a status this tool moves off. They are counted under `a person had already set it, left alone`, which is the same line a clash somebody set by hand would appear on
277. Find a clash you set to Approved by hand, if there is one, and run again
278. Look for, F72: it is still Approved. Only New and Active ever move, and that is a Core rule with its own tests rather than a condition in a loop

## The log round, F59 to F64. One folder, watched and then read

THE ONE THAT ANSWERS THE ROUND. Everything below this is detail, and this section on its
own is what Bader asked for on 2026-09-18: show what happened inside Navisworks, show
what is running while it runs, and show where the time went. Do this one first, with the
whole folder ticked, because a single building cannot show group N of M and cannot fill
the run timing block.

279. Run the SAME building once more with the box OFF, and note the `clash step took` seconds on both runs
280. Look for, F72: what the penetration pass COSTS, which is the one number nothing here could measure. With the box on, every clash has a category read off both sides and a size read off the service, walking up to four levels for each, so the cost is per CLASH and not per test. It is UNKNOWN until this comparison is made. If the difference is small, leave it on. If it is large, that is worth saying, because the pass reads the same properties the harvest reads a moment later and the two could be made to share one read
281. Go to the Source step, pick the C06 NWC folder and press Scan
282. Go to the Outputs step and pick the NWF folder and the NWD folder
283. Go to the Clash step and pick the clash XML
284. Go to the Grouping step and make sure EVERY group is ticked
285. Press Run, then OK, and then WATCH the window rather than leaving it
286. Look for, F62: the line just above the log box reads the group, the building, the step, the seconds on that step and the seconds on the run, and it changes as the run works. That line is the whole of what is running while it runs
287. Look for, F62: the log box below it scrolls itself to the newest line, and the line above never moves out of sight
288. When it finishes, open the log
289. Look for, F60: a block headed `TIMING` after every group, and one headed `TIMING, THE WHOLE RUN` before `RESULT`. That is where the time went, per group and then by step across every group
290. Look for, F60: the last line of the run block says whether the run fitted in forty five minutes. That is criterion 2 answered by the log itself, and it is the single most important line in the file
291. Look for, F61: `CENSUS` lines either side of every step, carrying models, sets, tests, results and views. That is what happened inside Navisworks, counted rather than assumed
292. Look for, F63: a `GAP` block at the end of every group, saying what the run measured and the report does not show
293. Send the log and the `.tsv` beside it. Everything after this section is the detail behind these five things, and it is worth doing, but if you only do one section do this one

## The log round in detail, F59 to F64

Nothing here needs a new folder or a new file. It is all read off the log of the run
above, or of any ordinary run.

294. Run any one building the ordinary way, with the clash XML picked
295. Open the log for that run
296. Look for, F59: a pair of lines per step, reading `STEP     ` then the step name then `started`, and later the same name then `finished`, the seconds and a few words for what it changed. The step names are `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `IMAGES`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM`. There are SIXTEEN since F120 added `RENUMBER`, and `VIEWS` sits between `RENUMBER` and `WORKBOOK`. SINCE F114 MERGED THE BOX OPENS TICKED, Q131, so the `VIEWS` pair is there in every group with a report
297. Look for, F59: `STEP     TESTS RUN` appears ONCE as a started line and once as a finished line for the whole group, then one line reading `entered again. Every further visit in this group is counted, not written out`. A pair of lines per test would be 3660 lines for 1830 tests, which is the fault that once left a 17.8 MB log
298. Look for, F59: one line per repeated step at the end of the group, before the `GROUP    finished` line, reading the step name then the visits then the total seconds, like `STEP     TESTS RUN     1830 visits, 742.113s in total`
299. Look for, F59: `STEP       IMAGES` is indented two spaces further than every other step line. It is the only one that runs INSIDE another step, because a picture is written while the harvest is walking the results. `HARVEST` is NOT indented, because it opens and closes on its own
300. Look for, F59: NO line anywhere reads `NEVER CLOSED`. That line means a step was opened and nothing closed it, which would make every total after it wrong. If you see one, send the whole log
301. Look for, F59: one clock per piece of work. Find the FIRST `CLASH    ` line that reads `clashes` or `passed`, which carries a test name and its seconds, and find the `STEP` line for `TESTS RUN` that reads `finished`. Those two are the same reading, because the step IS the measurement now and the Stopwatch that used to time the same thing separately is gone. The `CLASH` line rounds to ONE decimal and the `STEP` line carries three, so 12.3 against 12.345 is them agreeing and not disagreeing. The step name is padded to the width of the longest one, so read the words and do not count the spaces
302. Look for, F60: a block headed `TIMING` and the building, straight after the `GROUP    finished` line of every group. Every step on its own row with its seconds and its share, slowest first, then a row reading `outside every step`, then `group total`
303. Look for, F60: the shares in that block add up to a hundred. They are meant to, because whatever the steps do not account for is a row of its own rather than a number left off the page. A column adding to less than a hundred means a row is missing and is worth sending
304. Look for, F60: the `group total` on that block is the SAME number as the seconds on the `GROUP    finished` line above it
305. Look for, F60: a line reading `inside another step, so these seconds are already counted above` at the bottom of the block, with `IMAGES` under it. Those seconds are inside `HARVEST`, so they are deliberately left out of the share column
306. Look for, F60: a block headed `TIMING, THE WHOLE RUN` just before the `RESULT` block at the end of the log. It reads the groups slowest first, then the run total, then the SAME steps added across every group, which is what answers which step costs the run rather than which building
307. Look for, F60: the last line of that block reads `The run took` and then the time in minutes and in seconds, and then either `inside the 45 minutes 0 seconds a run has to finish in` or `OVER` it. That sentence is criterion 2 answered by the log itself. Send it either way
308. Look for, F21 which closes with F60: one line per test reading `ROWS     ` then the test name then `N rows for the workbook, N clashes in the document`. Where the two agree it says `they agree`. Where there are FEWER rows than clashes it names the result groups as the reason, which is right, because the workbook and the Clash Detective panel both show one row per group
309. Look for, F21: NO line anywhere reads `THERE ARE MORE ROWS THAN CLASHES`. Nothing in this tool produces more rows than clashes, so that line means something is wrong and the whole log is worth sending
310. This is criterion 3 without opening Excel. Pick any test whose `ROWS` line says they agree, open the workbook at that test's block and count the rows, then open Clash Detective on the same test and read the panel count. All three should be the one number
311. Look for, F61: a pair of lines around each step reading `CENSUS   before ` then the step name, and `CENSUS   after  ` then the same name, each carrying five counts: `models`, `sets`, `tests`, `results` and `views`
312. Look for, F61: the `views` count is a NUMBER and not `UNKNOWN`. It reads `0` before `APPEND` and whatever the NWC files brought after it, which on the run of 2026-09-19 was 20. It used to say this count reads 0 on every line, and that was wrong: an NWC exported from Revit carries that model's saved viewpoints and appending it brings them in. `SavedViewpoints.Count` returns minus one only where it could NOT count, and minus one prints as `UNKNOWN`. An `UNKNOWN` here means the viewpoint collection is not the shape it was MEASURED to have on 2026-09-19, `docs\history\scan.md` 5d, and the whole log is worth sending
313. Look for, F73: NO line anywhere begins `CENSUS CHANGED`. That line means a count moved during a step that may not move it, which is a real fault, and the group carrying it will read FAILED rather than DONE. If you see one, send the whole log. A line beginning `CENSUS NOTED` is NOT that line and is not a fault: it is the saved viewpoints moving during `APPEND`, it says why, and it ends `can still be DONE`. On the run of 2026-09-19 the refused line was written in all seven groups and put every one of them out of DONE while 28 files had been written correctly
314. Look for, F61: the counts move where they should. `sets` goes up across `SETS`, `tests` goes up across `TESTS CREATE`, `results` goes up across `TESTS RUN`, and `models` goes up across `APPEND`. None of them moves across `NWD`, `WORKBOOK`, `HTML`, `XML` or `CONFIRM`. Expect `TESTS CREATE` and `TESTS RUN` to move by ONE test's worth and not by the whole group, because those two are entered once per test and the census is taken around the first visit only. That is step 315 and it is not a fault
315. Look for, F61: `CENSUS   before TESTS RUN` appears ONCE per group and not once per test. The census is taken around the first visit of a step and no more, because counting the whole document 1830 times would be the log making the run slower
316. Look for, F61: one line per group reading `CENSUS   cost ` then the seconds then the number of counts. This is the measurement of what the census itself costs, and it is the number that decides whether it stays wide
317. Look for, F61: that cost line says `inside the 1.000s a group is allowed`. If it says `over`, the census narrows from the next group on and the next group says `CENSUS   narrowed` at its top. Both are working as intended, but send the number either way, because it is the first real measurement of what the census costs on a real model
318. Look for, F62, while the run is WORKING and not after it: the line just above the log box reads `Group 3 of 14`, then the building, then the step, then how long that step has been going and how long the run has, like `Group 3 of 14  1B06PH  TESTS RUN  12s on this step  4m 02s on the run`
319. Look for, F62: that line sits on its own row above the log box. It has to stay put while the log box scrolls itself. If it is beside the buttons and cut off at the window edge, the build is older than F62
320. Look for, F62: the log box keeps scrolling to the newest line by itself the whole time, and the line above it never moves out of sight
321. Look for, F62: the line changes as the step changes. Watch one group through and you should see `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `RENUMBER`, `VIEWS`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM` go past, `VIEWS` reading `View A/<Team> vs <Team>/<test>` as each view is written. `IMAGES` is the one step that never appears on this line, because it is opened inside the picture writer and nothing there was given the line. It is in the log and in the timing block like every other step
322. Look for, F62: inside the clash run the seconds on the run keep counting up while the tests go by. It is rendered at most once a second on purpose, so it should look like a clock and not like a flicker
323. Look for, F62, on the SECOND group onward: if a step runs over twice as long as the same step took on the group before, the line says `SLOWER, the group before took` and the time. On the first group it never says it, because there is nothing to compare against
324. Look for, F62: while the NWD is publishing the line stops counting and sits at the seconds it last showed. That is expected and it is written down: publishing is one Navisworks call with no loop inside it, so there is nothing to tick from without starting a thread, and nothing here starts one. If the line went blank instead, that is a fault
325. Look for, F63: a block headed `GAP` and the building at the END of every group, after every output has been written. That is the point: it answers what the outputs do NOT carry
326. Look for, F63: one line per property the run MEASURED and no output carries, each with how many item cells carried it out of how many there were, and where it would belong. The six it can name are `Family`, `Type Name`, `Material`, `Source File`, `Discipline` and `Id From`. FEWER THAN SIX IS NOT A FAULT: a property no item carried at all is not a gap and is left out on purpose, because reporting it would say the run is holding back something it never read. Which ones appear is itself the measurement
327. Look for, F63: the first line of the block says `nothing acts on it`, or, where a group held nothing back at all, the block is one line reading `nothing measured this group reaches no output`. Either way a gap is a question and never a fault, and no group should read anything but DONE because of one
328. Read the counts on those six lines. They are the first real measurement of how much of each property a real model actually carries. `Material` is expected to be low and `Family` high, and if `Material` is at zero it may be worth deleting rather than keeping, which is Q37
329. Look for, F63: one line at the end of the run reading `GAPS     ` then a number then the names. It counts by NAME and not by line, so a run of fourteen groups says six and not eighty four
330. The six are now Q35 to Q40 in `steps\02_questions.md`, one per property, because one answer for five different properties is a decision nobody can make. Answer them with the counts from step 328 in front of you
331. Look for, F64: in `%LOCALAPPDATA%\ParsonsNwcFederator\logs`, beside every `run-*.log`, a `run-*.tsv` with the SAME name and a different extension
332. Look for, F64: near the top of the text log, one line reading `ROWS     the machine readable log is` and the path. If it says `no machine readable log` instead, the text log is unaffected and the reason is on that line, so send it
333. Open the `.tsv` in Excel. It should open straight into columns with no import dialog and no question about separators
334. Look for, F64: a header row reading time, seconds, group, step, event, name, number, text, and then one row per event under it
335. Look for, F64: every row has eight columns. Sort by the `event` column and read the kinds. THERE ARE TWENTY THREE. The worksets round added `service with no readable size`, which is Q71 and which is ABSENT from both proving runs because every service is now measured, and PART 3 of the drift round added `set finding nothing`, one row per set that found nothing, written beside the EMPTY SETS block. Before those two there were twenty one, not the fourteen this step used to name, and seven were added after it was written. From the log itself: `step started`, `step finished`, `step threw`, `step total`, `timing`, `timing nested`, `census before`, `census after`, `appended`, `append failed`, `written`, `group finished`. From the clash step, F76 and F81: `test created`, `test passed`, `test found clashes`, `tolerance set on a saved test`, `rows for the workbook`. From the blocks: `gap`, `set across the run` (F82), `set finding nothing` (PART 3 of the drift round), and `model placement` and `model export` (the alignment round). NOT EVERY KIND APPEARS ON EVERY RUN: a weekly run appends nothing so `appended` is absent, and nothing throwing means no `step threw`. The proving run of 2026-09-20 carried sixteen of the twenty one. A kind not on that list of twenty three is something new and the whole file is worth sending
336. Filter the `event` column to `step finished` and sort the `number` column biggest first. The top row is the slowest step of those entered ONCE in their group, in seconds. It is NOT the slowest step of the run, and that is worth knowing: a `step finished` row is written only on a step's first visit, so `TESTS RUN` and `TESTS CREATE`, which are entered once per test, each contribute one row for their first visit and reach the file again only as a single `step total` row per group. To find the slowest step of the run, read the `step total` rows beside these, or read the TIMING block in the text log, which adds both kinds up already
337. Look for, F64: nothing in the text log got worse. It still has its blocks, its indenting and its sentences, and it is still the one to read first and the one to send

## The two walls are live, D7

There are three files here that `sh` reads: the two hooks Claude Code runs before a tool
call, and the pre-commit hook that runs the tests. One refuses an edit under `samples`,
`steps\logs` or `bundle`, one refuses a commit or a push while main is checked out, and
the third refuses a commit while any test fails.

None of them survives a carriage return. `sh` reads one as part of the word, so a CRLF
copy dies on its first `case` line before it reaches a rule, and because `sh` exits 2 on
a syntax error, and 2 is the code that refuses, the two Claude Code hooks then refuse
EVERY call rather than the protected ones, while the pre-commit stumbles past `set -e`
and lets every commit through untested. Your machine is where that bites, because Git for
Windows sets `core.autocrlf` to true when it installs and the repo had nothing telling it
otherwise. F47a added `.gitattributes`, which pins these three to LF on every checkout.

This is a one time check per clone. Do it once and the rest of this file never needs it.

338. In the VS Code terminal, in the repo folder, run:

```
git ls-files --eol .claude/hooks .githooks
```

339. Look for: three lines, each reading `i/lf` and `w/lf` and `attr/text eol=lf`. A `w/crlf` on any of them means this checkout still holds the old copy, so run `git add --renormalize . ; git checkout -- .` and read it again
340. Switch the test wall on, which git needs told once per clone:

```
git config core.hooksPath .githooks
```

341. Look for: `git config core.hooksPath` answers `.githooks`
342. Make a branch, change one word in `steps\history\log.md`, and commit it from the VS Code terminal rather than from GitHub Desktop, so you see what the hook prints
343. Look for: the commit pauses and prints FOUR lines in this order, `pre-commit: checking the add-in for a local declared twice`, `pre-commit: checking the add-in for a type with no import`, `pre-commit: running the full test set`, then `pre-commit: tests passed`, and only then commits. That is the test wall, and the two checks in front of it are F58's and F66's. A commit refused by either of those prints `pre-commit: REFUSED. See the line above.` and never reaches the test set at all. Throw the branch away afterwards
344. Open Claude Code in this folder and ask it to write one word into any file under `samples`
345. Look for: it comes back refused, with the line `Refused. ... is under samples, steps/logs or bundle, which are never edited.` That is the paths wall, and the branch wall is the same hook file beside it, proved the same way by asking it to commit while main is checked out

## Delete the old branches, D6

Every branch except main is merged into main. This is yours, one command from the repo folder in the VS Code terminal. A session working in a container cannot do it: `git push origin --delete` comes back HTTP 403 from the proxy in front of it, and no GitHub tool in it deletes a branch.

The list below was read on 2026-09-19 with the command in step 346, after the penetration round, and `git ls-remote --heads origin` gave 67 names. Two more go up with this round's own closing work, `fix-F72` and `round-close-penetrations`, and neither was on the remote when the list was read, which makes 69 lines and 68 to delete by the time you run it. Read the live list again yourself before you delete, because a branch may have come or gone since. Do not build the list from `git branch -r`. That prints remote-tracking refs your clone remembers, and a branch deleted by someone else is still in it until you prune, which is how a name that does not exist on the remote reached this file once already. A clone showed it again on 2026-09-18, still holding `origin/claude/parsons-nwc-analysis-rlzgdr` after the remote had lost it. `git ls-remote` asks the remote and remembers nothing.

346. Read the live list:

```
git ls-remote --heads origin
```

347. Look for: one line per branch, the name after `refs/heads/`. It was 77 on 2026-09-21, so 76 to delete, READ OFF THE LIVE REMOTE just now and not off this clone. It was 66 after the build round and 69 after the penetration round, and the seven added since are `audit-first-run`, `round-wiring`, `round-first-run`, `round-viewpoints`, `round-dimming`, `round-alignment` and `round-worksets`. The delete command below names 68 and does NOT name those SEVEN, nor `round-drift`, which is an eighth once it is pushed, so read the live list first, which is what step 346 is for. This step said six while listing seven names
348. Delete every one of them except main:

```
git push origin --delete analysis-pass fix-F16 fix-F27 fix-F28 fix-F29 fix-F30 fix-F31 fix-F32 fix-F33 fix-F34 fix-F35 fix-F36 fix-F37 fix-F38 fix-F39 fix-F40 fix-F41 fix-F42 fix-F43 fix-F44 fix-F45 fix-F46 fix-F47a fix-F47b fix-F47c fix-F50 fix-F51 fix-F52 fix-F53 fix-F54 fix-F55 fix-F56 fix-F58 fix-F59 fix-F60 fix-F61 fix-F62 fix-F63 fix-F64 fix-F65 fix-F66 fix-F67 fix-F68 fix-F69 fix-F70 fix-F71 fix-F72 fix-f1-f2-f4-small fix-f10-gate-outputs fix-f11-dead-code fix-f17-picture-order fix-f20-tests-on-push fix-f22-two-workflows fix-f24-rebuild-changed-nwf fix-f26-units-meters fix-f5-sets-built fix-f6-open-file-folder fix-f7-open-file-result fix-f8-run-saved-tests fix-f9-changed-skip-units master round-close round-close-2 round-close-3 round-close-4 round-close-build round-close-log round-close-penetrations
```

349. Look for: one `- [deleted]` line per branch and no error
350. Run `git ls-remote --heads origin` again and look for: one line, `refs/heads/main`. If a branch you did not expect is there, it was pushed after the list above was read, so read what it holds before deleting it
351. Run `git fetch --prune` so your own clone forgets the branches that are gone. Without it `git branch -r` keeps printing them
352. If the command refuses a branch, open github.com, the repo, Branches, and press the bin icon beside every branch that is not main
   Answer: Bader, 2026-10-07, his message headed BADER'S ANSWERS, 7 OCT 2026, THE SIX ROWS WAITING FOR HIM AND THE SHUTDOWN TIME: the lead deletes the remote branches other than main whose pull requests are merged and whose commits are all on main, reading the live list first and listing each one deleted, keeps any branch with an open pull request or a commit not on main, and lists for Bader any delete GitHub refuses.

## The first run round, F73 to F88

**READ THIS FIRST. This round was worked in a LINUX CONTAINER with no Navisworks on it.
Every rule below is in Federator.Core, every one has tests, and the whole Core set passes.
NOTHING IN THE ADD-IN WAS COMPILED, because the add-in cannot be compiled without
Navisworks and this session had none. Five of the eighteen fixes turn on a measurement
only Navisworks can give and those five are steps 353 to 357. The add-in wiring each fix
still needs is steps 358 onward, and every one of them is written out so it can be done
without reading the brief again.**

### The five measurements, before any of the wiring

353. Open a Developer Command Prompt and dump the members of `Autodesk.Navisworks.Api.dll`
     looking for anything that says a document has finished LOADING. Every member on
     `Document`, `Document.Models`, `DocumentParts.DocumentModels` and `Application` whose
     name holds Load, Ready, Busy, Progress, State, Pending or Complete, and every event on
     each. Paste the list into `docs\history\scan.md` under 5e, where the question is
     already written, and say in one line whether any of them answers "the models are all
     in now". If one does, `ModelLoadWait` becomes the fallback and the add-in reads the
     member instead. If none does, the poll stands and that sentence is why
     DONE on 2026-09-19 in the wiring round by tools\probes\probe-document-ready.ps1, written into scan.md 5e. SceneLoaded exists on DocumentModels, its timing does not read off the DLL, so the poll stands and step 365 counts the event beside it.
354. Dump the property members the same way, for F86, into 5f: `ModelItem
     .PropertyCategories`, `PropertyCategory.DisplayName`, `.Name`, `.Properties`,
     `DataProperty.DisplayName`, `.Name`, `.Value`, every reader on `VariantData`, and
     whether a `Search` can walk a whole model in one pass rather than per item. One line
     saying how a value becomes the text a CSV cell holds, and one saying whether the walk
     is per item or per search
     DONE on 2026-09-19 in the wiring round by tools\probes\probe-properties.ps1, written into scan.md 5f.
355. DONE on 2026-09-20, in the alignment round's PART 3 predecessor. `docs\history\scan.md` carries a second 5g, "5g, measured", answering YES exactly: a negated condition builds, finds exactly the right items beside a positive one, and keeps its bit through a save and a reopen, proved through `tools\probes\ViewpointProbe` in its `negate` mode. What is still open is only whether Navisworks OWN EXPORTER writes flags="32", and nothing waits on it. The original step follows.

     355. In Clash Detective, build ONE search set by hand with a NEGATED condition, export the
     selection sets to XML, and paste the `<condition>` element into 5g. Then import that
     same XML into a fresh document and confirm the set comes back with the negation still
     on it. That answers whether `BLD-EL-Devices` can be written as category contains
     Devices AND NOT the six named device categories. Until it is answered the file in
     `exchange\` carries the fallback, `Category equals "Nurse Call Devices"`, which is
     proved to import because the whole file uses `equals`. THOSE TWO LINES ARE HISTORY
     AND NOT THE FILE TODAY: 5g was measured on 2026-09-20, the fallback is gone, and the
     committed file carries one `contains` and six negations at `flags="32"` for that set.
     The words "Nurse Call Devices" appear in it nowhere.
     NOT done in the wiring round. What reflection can say is under 5g: SearchCondition.Negate exists and NegateCondition is bit 32. The hand built set and the round trip still wait for you.
356. Dump the clash members for F72c into 5h: every member on `ClashResult`,
     `IClashResult` and `ClashTest` whose name holds Comment, Note, Description, Tag,
     UserName, Status or Approved, and whether the general `Comments` collection reaches a
     clash result. Then write one comment on one clash by hand, save the NWF, close it,
     open it again and see whether the comment is still there. IF IT CANNOT BE DONE the
     answer is one line in 5h saying so, and the tool then sets the status alone and fakes
     nothing
     DONE on 2026-09-19 in the wiring round by tools\probes\probe-clash-comments.ps1, written into scan.md 5h. TestsEditResultComments writes one, and step 372 is wired to it. Whether it survives a save and reopen is what the run log of the wiring round shows.
357. DONE on 2026-09-20, in the viewpoints round. `ViewpointProbe` in its `walk` mode read the ten C02 NWFs, 47,471 items, 374 distinct values, and the committed revit-categories.txt file under src, Federator.Core, Exchange, holds exactly those 374. The health check now compares against a real list. What is left is Q56, whether the walk should cover more than the C02 folder before the list is trusted project wide. The original step follows.

     357. Open one real federation and walk every item's category property, writing the
     distinct values out. That is F84's list and it is the same walk F86's probe does, so
     the two are measured on one run. Paste the values into
     `src\Federator.Core\Exchange\revit-categories.txt`, one per line, exactly as the model
     spells them, and leave the comment block at the top where it is. Until that file has
     names in it the health check compares against nothing and says so

### How to run the Property Probe, F86, and where its CSV goes
     NOT done in the wiring round. The probe as built reads only the seventeen categories the settings ask for, so it cannot list every category a model carries, and 5i needs its own walk.

358. The button reads `Probe model properties` and it is on the Clash step. It does not run
     as part of a federation run and it never touches an NWF
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
359. Press it and pick either a FOLDER of NWC files or the document you already have open.
     It reads NWC and refuses an NWF or an NWD by name, because opening an NWF replaces
     whatever is open and an NWF is where every clash result lives
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
360. It writes ONE CSV per file, beside that file, named after it with
     `-properties.csv` on the end. So `1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc` gives
     `1104-PAR-1C07BC-ZZZ-ME-MOD-000001-properties.csv` in the same folder
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
361. The CSV has five columns: category, property tab, property name, distinct value, how
     many elements. It covers seventeen categories, which are the thirteen this tool calls
     a service plus the four it has decided are not one, and that list is read off the
     client's own matrix rather than typed
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
362. A property carrying more than 100 distinct values keeps the commonest 100 and gets ONE
     extra row saying how many were left out and how many elements they covered. Nothing is
     dropped silently
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
363. Look for, F86: a `PROBE` block in the log, one per file, saying how many categories
     were asked for, how many were found, how many found no element at all, how many
     properties and distinct values there were, which properties were capped, and whether
     FS or Fire Suppression appears in any tab, name or value. IT SAYS SO AS PLAINLY WHEN
     IT DOES NOT, because a probe that only speaks up when it finds something reads as one
     that found nothing rather than as one that ran
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
364. Send the CSV for one mechanical NWC and the PROBE block beside it. That pair is what
     the mechanical sets get rewritten from, and rewriting them is a later round
   Answer: Bader, 2026-10-07, his message headed BADER'S ANSWERS, 7 OCT 2026, THE SIX ROWS WAITING FOR HIM AND THE SHUTDOWN TIME: the lead runs the property probe on one mechanical NWC, by the guards, and keeps its CSV and its PROBE block as the evidence. Bader sends nothing.

### The add-in wiring each fix still needs
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.

365. F74. In `FederationEngine.Decide`, after `document.TryOpenFile` returns true, poll
     `document.Models.Count` through `Federator.Core.Rerun.ModelLoadWait`, handing it the
     count and `RunLog.ElapsedSeconds` each time, pausing `PauseMilliseconds` between
     readings, and write `wait.Line()` when it ends. Where the wait SETTLES, carry on into
     `NwfComparison.Compare` exactly as today. Where it gives up at the ceiling AND the
     count is still zero, return `NwfComparison.ReadEmpty(job.NwfPath, job.Files,
     "after waiting " + seconds)` instead, and carry its `Reason` onto the group as
     `GroupFacts.NwfReadEmptyReason`. Do the SAME thing in `PreviewRunPaths`, which is a
     second reader of the same NWF, or the confirm dialog says Rebuilt about a healthy file
     WIRED on 2026-09-19 in the wiring round, F74, commit fcbdb6b. The seconds handed to the wait are counted from the open RETURNING, off a stopwatch started for it, not RunLog.ElapsedSeconds, because the ceiling is thirty seconds from the open and not from the session.
366. F75. Empty the document at the TOP of `RunOne`, before `Decide`, on the scanned path
     only. Take the census straight after and write `CensusRule.StartOfGroupLine(census,
     true)`, and put `StartOfGroupReason(census, true)` on the group where it is not null.
     The open file run passes `false` and empties nothing, because the document IS the file
     list there
     WIRED on 2026-09-19 in the wiring round, F75, commit b5ce89f.
367. F76. Read the drop down into `ReportOptions.Tolerance`. In `ClashRunner`, where a test
     is created and where a test already in the document is left alone, call
     `options.Tolerance.For(planned.Tolerance, documentUnits)` and set THAT. Write
     `LogLine(created, alreadyThere, documentUnits)` once per group. Put
     `WarningLines(groups, testsInTheFile)` on the confirm dialog
     WIRED on 2026-09-19 in the wiring round, F76, commit b6ec0f2.
368. F76 second half. `ClashRunner` line 406 sets `report.Tolerance = planned.Tolerance`,
     which is the XML and not the document. Read it off the `ClashTest` in the document
     instead and set `report.ToleranceFrom = ToleranceOrigin.Document`. Count the four
     origins across the run and write `ToleranceChoice.ReadFromLine`
     WIRED on 2026-09-19 in the wiring round, F76, commit ee62966.
369. F77. Before creating any test, build a dictionary of locator to item count off the
     sets just resolved, hand it and the plan to `CreationPlan.For`, create only
     `plan.Create`, feed `plan.NotCreated` into the existing skip machinery, and write
     `plan.CountedLine(testsInTheFile)`
     WIRED on 2026-09-19 in the wiring round, F77, commit 03a913e.
370. F83. A second picker beside the clash XML picker on the Clash step, optional, and
     `PickerKind.Priority` for where it opens. Read the file into `PriorityMap.Read`, put
     it on `report.Priorities`, set `test.Priority` on every `TestReport`, write
     `map.MatchLines(testNames)`, and pass `picked` into `WorkbookCheck.Of(path, picked)`.
     Count the clashes into a `PriorityTally` and write `ResultLine` in RESULT
     WIRED on 2026-09-19 in the wiring round, F83, commit 580fe51.
371. F72b. A second tick box under the penetration one, off by default, its label and grey
     line read off `ByDesignPairs.TickLabel` and `HelpLine` and never typed into the XAML.
     A third picker for the pairs file. In the same pass that applies the penetration
     statuses, and NOT a second write path, call `ByDesignRule.Judge` per clash, add the
     wanted ones to the one `statuses.Apply` list, and write the block. The XAML grid has
     two rows today and needs a third, with the Things that destroy data expander moved
     down
     WIRED on 2026-09-19 in the wiring round, F72b, commit 8f7cedd.
372. F72c. Write `record.Text()` as a comment on the clash BEFORE the status is set and on
     the same handle, because every mutator on `DocumentClashTests` is a copy form that
     kills the handle handed to it. If step 356 says a comment cannot be written, write
     `UndoAutoReview.CannotLine(why)` once and set the status alone
     WIRED on 2026-09-19 in the wiring round, F72c, commit 377d1f0.
373. F72c. The `Undo auto Reviewed` button reads every clash, calls `UndoAutoReview.Judge`
     and puts back only the ones that carry our record AND are still at Reviewed, each to
     the status the record names. It goes through the one `ClashStatusEditor` like
     everything else
     WIRED on 2026-09-19 in the wiring round, F72c, commit 377d1f0.
374. F85. `ViewpointBuilder` loops CLASHES and not disciplines now, reading
     `ClashViewpointPlan.For`. `SavedViewpoints.Exists` resolves one folder under the root
     today and cannot see a three deep path, so rebuild the folder walk on `SetBuilder`'s
     measured `EnsureFolders` shape, re-resolving from a FRESH `RootItem` after every
     `AddCopy`. Open the step as `RunSteps.Views`, which is new. LEAVE `CanBuild` FALSE
     until step 375 answers
     DONE on 2026-09-19 in the viewpoints round, commit 11d7ec2: Build takes the clash plan, the old per discipline plan is gone, the folder walk is EnsureFolders re-resolved from a fresh RootItem, and the VIEWS step is opened around it.
375. F85. On a run, save one viewpoint by hand while some items are hidden, then read
     `SavedViewpoint.ContainsVisibilityOverrides` on it and press it again from a clean
     view. That answers whether a viewpoint records the hiding, which is the last thing
     `CanBuild` waits on, and the answer goes in `docs\history\scan.md` 5d
     DONE on 2026-09-19 in the viewpoints round by tools\probes\ViewpointProbe, written into scan.md 5j and not 5d: a viewpoint captured with CaptureRuntimeOverrides records the hiding and brings it back after a save and a reopen, and CanBuild is true.
376. F80. Two lines in the window changed already: `log.RunStarted(jobs.Count)` and
     `log.RunFinished()` in place of the two `log.Line` calls that wrote the same
     sentences. Read them once and check the log reads `RUN      started, 7 groups` and
     `RUN      finished` exactly as before
377. F81. Four call sites changed already: three in `ClashRunner` now use
     `log.NumberedRepeat` and the per group SETS block in `FederationEngine` is gone. Read
     them once, then on the next run check the `.log` is under 300 KB and the `.tsv` still
     carries a row for every test
378. F82. `FederationEngine` feeds `SetsAcrossTheRun` beside `outcome.Sets` already and the
     window writes the block after SOURCE FINDINGS already. Read both once
379. Build it: `dotnet build ParsonsNwcFederator.sln -c Release`. EXPECT ERRORS the first
     time. Eighteen fixes touched the add-in and none of them was compiled. Send the whole
     error list if there is one
380. Run one building and send the log. Look for, in order: `LOADING` then `STOPPED` or a
     model count, `CENSUS CLEAR` at the top of the group, `CENSUS NOTED` and NOT `CENSUS
     CHANGED` around APPEND, `TOLERANCE`, `CLASH 1830 in the file`, `PRIORITY` if a file
     was picked, `REVIEWED rule B`, `VIEWS`, `SETS ACROSS THE RUN`, and a RESULT block
     carrying `run time`, `waiting for the person` and both file sizes

## Proof of the viewpoints round, F85 written, 2026-09-20

Six runs of ten groups were driven from the session on this machine against copies of the
C02 NWF folder, the last two clean, and every number is in the round's entry in
`steps\log.md`. What is left is the look a person gives it and the two folders nobody but
Bader may touch.

381. Pull `round-viewpoints` and read the round entry at the top of `steps\log.md` as that branch holds it, the list
     of every program it started, every file it wrote outside the repo and the two times
     Navisworks had to be stopped rather than closed
382. Open `C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints\NWF6\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`
     in Navisworks. It is a copy of the 1A02MM NWF after the sixth run, in the temp folder,
     so it can be deleted after
383. Look for, in Saved Viewpoints: the four folders the file already had, then `A`, `B` and
     `C`, and under `A` the pair folders `AR vs ST`, `ST vs ST`, `AR vs DR`, `DR vs ST`,
     `AR vs EL`, `EL vs ST` and `ST vs UNKNOWN`
384. Press one viewpoint under `A`, `AR vs EL`. Look for: the view jumps to the clash, framed
     the way its picture in the report frames it, and in the Selection Tree the ME and ST
     models grey out while AR and EL stay
385. Press one under `A`, `DR vs ST`, drainage against structure. Look for: the ME model
     stays shown as well as ST, because the drainage pipe lives in it, and only AR and EL
     grey out. Clash 2 of the framing test opens on grey, the camera inside a member,
     which is question 57
386. Beside his NWCs in `C:\00-NM\Federation Task\C02 + 04\C02\NWC` sit four files the
     WIRING round's probe button wrote on 2026-09-19, `1104-PAR-1A02WO-ZZZ-AR-MOD-000001-properties.csv`
     and its EL, ME and ST siblings. This round wrote nothing there and deleted nothing.
     They are Bader's to delete
387. When ready, run once on the real folders with the same boxes: 25 mm, by design on,
     penetrations on, the priority file. Look for, per group with clashes:
     `VIEWS    N of N planned view(s) written`, `VIEWS    N row(s) of the report read for
     the views`, `hidden state put back`, the `VIEWS TREE` block ending `7 of 7 hold`, and
     a `VIEWS` step of seconds and not hours in the timing block, since F114 writes one
     view per test where F85 wrote one per clash.

     DO NOT EXPECT `Nothing failed` ANY MORE, and this step said to until the worksets
     round. Since Q70 a group whose model names `Internal` as its shared site is FAILED,
     and two of the ten are: 1A02MM and 1A02WL. The run of 2026-09-20 16:20 reads
     `groups done : 8` and `groups failed : 2`, and `Nothing failed.` is written only
     when failed groups plus errors is zero, so on his folders it cannot appear at all.
     Expect instead `ALIGNMENT failed 2 group(s)` at the run tail with one line naming
     each, and remember that BOTH still wrote their NWF, their NWD and their report.
388. Look for, IN TWO DIFFERENT PLACES, which this step used to get wrong by putting both
     at the run tail. `Revit categories known: 374` and `Sets asking for a category no
     model carries: 14`, which is question 56, are in the `HEALTH <file>.xml` block, which
     is written WHEN THE CLASH XML IS PICKED and not at the end of the run. The
     `WORKBOOK 18,300 test blocks` line, which is question 54, IS at the run tail
389. Answer questions 54, 55 and 56 in `02_questions.md`

## Proof of the dimming round, one step, 2026-09-20

The viewpoints now dim everything but the two clashing items, the way Clash Detective
does, which is what you asked for when you pressed two and could not see the clash. One
step, about a minute.

390. SUPERSEDED BY STEP 391, and worth keeping for what it says about the dimming on its own. THIS FILE PREDATES THE COLOURS: it was written by the dimming round and its viewpoints ghost the scene without painting the two items red and green. Step 391 is the same check on your own NWF with the colours in. Open this file in Navisworks, which is a copy and yours to delete afterwards:

     `C:\Users\bader\AppData\Local\Temp\claude\round-dimming\NWF3\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`

     In Saved Viewpoints open `A`, then `DR vs ST`, and press these two:

     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Framing  Clash2`
     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors  Clash1`

     Look for: everything ghosted except the pipe and the thing it hits, which stay
     solid, and AR and EL greyed out in the Selection Tree. The first of those two is
     the one that was a featureless grey wall before this round. The second shows the
     roof ghosted with the plant visible through it.

     If the dimming is too strong or too weak, it is one number and no rebuild of your
     models: `ViewpointSettings.DimTransparency`, 0.85 today, and 0 switches it off.

     WHAT TO WEIGH WHILE YOU LOOK. Dimming is not free and the numbers are in the round
     entry in `steps\log.md`. The run went from 5 minutes 3 seconds to 10 minutes 26, and
     1A02MM's NWF went from 119,542 bytes to 23,327,744, which is 195 times, because a
     dimmed viewpoint records one material override per item it dims and that group
     writes 430 of them. Question 59 asks whether that is worth it to you. Nothing is
     capped and nothing is thinned, because that is your call and not mine.

## Proof of the alignment round, two steps, 2026-09-20

THIS ROUND RAN AGAINST YOUR OWN FOLDERS, not a copy, which is what you asked for in Q60.
The backup was taken first and read back before anything else happened, 9 files against
9 with every byte size compared one for one and no mismatch:

`C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20`

THERE IS A SECOND ONE NOW, taken by the worksets round before its own run on the live
folders the same day, 14 files, 40,008,819 bytes, read back the same way:

`C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20-worksets`

Both folders are yours to delete once you are satisfied. Nothing else of yours was touched.

391. Open your own 1A02MM federation, which now carries the viewpoints with the colours:

     `C:\00-NM\Federation Task\C02 + 04\C02\NWF\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`

     In Saved Viewpoints open `A`, then `DR vs ST`, and press these two:

     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Framing  Clash2`
     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors  Clash1`

     Look for: everything ghosted as before, and now the two clashing items RED and
     GREEN, red on the first side of the clash and green on the second, which is the
     order Clash Detective paints them so the viewpoint and the panel agree.

     Both colours are settings, `ViewpointSettings.FirstItemColour` and
     `SecondItemColour`, and `ColoursTheTwoItems` switches the painting off and leaves
     the ghosting.

     A THIRD SETTING CHANGED UNDER THIS ONE AND IS WORTH KNOWING. Every viewpoint on
     your machine is now written by the COM folder route, `RecordsThroughTheFolder`,
     which is Q59's answer. It records all four counts exactly as the old route did,
     measured both ways in scan.md 5p, and it saves three per cent rather than the two
     thirds the call count suggests. If a viewpoint ever comes back empty, that setting
     is the first thing to turn off, and it needs no rebuild of your models.

     ONE THING TO KNOW WHILE YOU LOOK. A viewpoint only records a colour where it
     DIFFERS from the item's own colour, so an item that already is the colour it is
     being given records nothing and still looks right. That is measured, scan.md 5p, and
     it is why the read back asks what the viewpoint will SHOW rather than what it wrote.

392. Read the ALIGNMENT and EXPORT CHECK blocks for 1A02MM in the run log, which is
     already beside your NWFs:

     `C:\00-NM\Federation Task\C02 + 04\C02\NWF\run-20260920-162006.log`

     Search it for `ALIGNMENT 1A02MM`. It says your four models agree in X and Y, differ
     in Z by up to 95 mm, and name FOUR different Revit shared sites, one of them
     `Internal`, which is what Revit calls a model that was not exported on a shared site
     at all. SINCE Q70 THAT FAILS THE GROUP, so 1A02MM now ends FAILED with the line `THIS GROUP IS FAILED` under the block naming the model. It still wrote its NWF, its NWD and its report, because the evidence is the point. Q65 said neither block can fail a group and Q70 answered later overrides it for this one case.

     Then search for `EXPORT CHECK 1A02MM`. Every model carries a workset on every
     element and an element id on every element, so the export itself is clean. THE LINE
     UNDER IT IS THE ONE THAT MATTERS: your models carry `ME-Ductwork` and the matrix
     asks for `ME-DUCTWORK`, the match is case sensitive, and that is why 33 of your 61
     sets find nothing in every group. QUESTION 68 IS ANSWERED and the matrix is corrected: it asks for `ME-Ductwork` now, four values and 17 conditions.
     AND IT CHANGED NO CLASH COUNT, which is Q72: a set already in your NWF keeps the conditions it was built with, so the corrected file never reaches it. The SETS block says so now, under the already-there count. What the correction cannot do is
     reach a set that is already there.

     THE OTHER THING IN THAT BLOCK, and it is only in the 16:20 log because the worksets
     round added it: your own workset names disagree with each other. The block NAMES the
     three that look like typos, `EL-Lightining Protection` against `EL-Lightning
     Protection`, `EV-Ccctv system` against `EV-Cctv System`, and `PL-Drainage equipmen`
     against `PL-Drainage equipment`, with which model carries which. It does NOT name
     `AR-EXTERIOR` against `AR-INTERIOR` or `ST-SUB` against `ST-SUP`, because those are
     two pairs of real worksets and a person has decided so once, in the measured list.
     It still COUNTS them, so nothing went quiet. No rule can fix the three typos, and
     the tool deliberately does not absorb them, because then nobody would fix the models.

## The fixtures, added 2026-09-21, and they come before either live folder

393. Install the bundle, then run the general fixture. It is three copies of C04's
     1A04WE models under `C:\Users\bader\AppData\Local\Temp\claude\round-close\fixture`,
     308 KB, and it takes about twenty seconds:

         build\install.ps1
         <temp>\round-close\open-addin.ps1
         <temp>\round-close\drive.ps1 -Source <temp>\round-close\fixture\NWC `
             -Nwf <temp>\round-close\fixture\NWF -Nwd <temp>\round-close\fixture\NWD `
             -Excel <temp>\round-close\fixture\Report

394. Look for: `groups done    : 1`, `groups failed  : 0`, `clashes found  : 27 across
     1 group`, 66 tests run and 1,764 skipped. SINCE F114 MERGED THE BOX OPENS TICKED, Q131,
     so the 27 per clash viewpoints created and read back on 2026-09-21 are not what is made:
     look instead for `views created     :` one per test with an open clash in the
     `VIEWS BUILT` block, `7 of 7 hold` at the end of `VIEWS TREE`, and NO `viewpoints     :`
     line under `groups failed  :`.
     Anything else and do not go near a live folder until it is understood

395. Look for, Q73: the workbook it wrote is about 1,917 rows holding 1,830 blocks, of
     which 8 found something. Under the old shape the same workbook was 14,667 rows

396. Run the penetration fixture, which is two copies of C04's 1A04WM models, 988 KB,
     about eighteen seconds, at `<temp>\round-close\fixture-pen`

397. Look for: `moved to Reviewed : 7` of `clashes looked at : 14`, and REAL SIZES in the
     moved lines, `Pipes 21mm through` and `41.7mm` and `82mm`, all through
     `BLD-AR-Floors`. IT IS AR AND ME AND NOT ST ON PURPOSE: the solid list is Walls,
     Floors, Roofs and Structural Foundations, and three of those four are ARCHITECTURE
     categories in these models. An ST fixture would fire only on foundations and would
     probably move nothing, reading exactly like the rule being broken

398. Look for, on either fixture: a penetration total of zero with all six buckets
     spread is a MEASUREMENT. A zero with everything in `not a service against a solid`
     and nothing in the other five is 5r's signature and IS a fault. Read the buckets,
     never the total

399. What the fixtures CANNOT show, so a green fixture is never read as a green run:
     scale, the weekly path, a CHANGED group, alignment across many models, the two non
     buildings, the single discipline groups, and anything needing the real matrix
     against real content

## Proof F120 the harvest and pictures, added 2026-10-08

Core is proved by its tests. These steps read what the add-in half changed off one run with
pictures on, the fixture of steps 410 to 416 or 1A02MM of the timed runs, and nothing here
needs the viewpoints box. The guard across groups cannot be read off a run where no picture
fails, step 489 says what it needs.

482. Build and install main once F120 has merged, steps 7 to 12
483. Run one group with pictures on, the fixture of steps 410 to 416, and open its log
484. Look for, in the group straight after its `IMAGES` tally lines, the `written`, `seconds each on average` and `KB each on average` lines: one line reading `IMAGES   the step's seconds went:` then `rendering`, `saving the JPEG` and `reading the file back`, each with its seconds in the form `0.000s`, then `in none of these`, then `of` the whole `over` N `visits to the step`, with N the clashes the harvest asked a picture for, which is the written count plus the failed count plus the `passed over` counts
485. Look for, in the same group's `TIMING` block: a row `RENUMBER` with its seconds, between `IMAGES` and `VIEWS`, and the `IMAGES` row's seconds the same as the whole on the line of step 484 to within a thousandth
486. Look for, in the `IMAGES` tally lines: the `written` count the same as the last run of the same group before F120, and the `KB each on average` the same
487. Open the pictures folder beside the workbook: the same count of `.jpg` files as `written`, no file ending `.moving`, and one picture opened reads 1024 by 1024 in its properties
488. Look for, in the `.tsv` log: where a picture failed, a line `IMAGE    failed for` the clash `. the render finished but no file arrived at` the path, the words before `at` the same on every such line, and where none failed no such line
489. Only with a render forced to fail across groups of under fifty clashes, UNKNOWN whether the lead adds one: look for `IMAGES   RUN STOPPED  the last 50 clash images all failed for the same reason, so the rest of the run was not attempted. Switch images off to run without them. the render finished but no file arrived`, inside the group the fiftieth fell in and not at its end, the label reading `The run was stopped. 50 clash images in a row all failed the same way, so the rest was not attempted. The log says what the failure was.`, and `RUN      STOPPED after` the count of groups in RESULT
490. On the timed runs of Q145 item 5 on 1A02MM and 1A04PK: the `IMAGES` line of step 484 per group, the `TIMING, THE WHOLE RUN` block's `IMAGES` seconds against set 03's 643.150 s with the same written count across the run, and send the log named on the `log file` line of the SESSION block

## Proof F114 the views, added 2026-10-08

Core is proved by its tests. These steps read what the add-in pass changed off the timed
runs of Q145 item 5, each on a COPY of the run set's folders and never on a live folder.
The three runs are the design's, turn5\q114-design.md section 5, and the numbers they are
read against are the baseline's, steps\runs\04\item1-C02, VIEWS 7487.104 s of 7925.970 s.
The box opens ticked, so nothing here unticks it, and a run that is to make no view is the
loop's, run.ps1 -Untick MakeViewpoints, step 416. Steps 228 to 233 stay yours at the final
run, and F132's steps 459 to 481 stand.

507. Build and install main once F114 has merged, steps 7 to 12
508. RUN A, a first run on a fresh 1A02MM NWF: copy the baseline's NWC folder of 1A02MM to a new folder with no NWF beside it, pick the exchange XML `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` with its corrections and teams files beside it and the sample priority file `samples\clash-priority-map.csv`, and run the one group
509. Look for, in the group, in this order after the CLASH blocks: `VIEWS`, `VIEWS BUILT`, `VIEWS INVENTORY` and `VIEWS TREE`, FOUR blocks, then `NWF      attempt`
510. Look for, in the `VIEWS` block: the plan's lines, one view per test with an open clash, the open clashes and the clashes in views counted, the clashes left out counted by status, each clash whose size could not be read in a pair carrying the size folder named by its test and clash, and no mirror named, this matrix holding none
511. Look for, in the `VIEWS BUILT` block: `VIEW     A/<Team> vs <Team>/<test>  created, shows <codes>, hides N models` five deep then `VIEW     and N more created, counted and not listed`, `views created     : N` equal to the plan's views, and NO `views that failed` line
512. Look for, in the `VIEWS INVENTORY` block: `inventory : N views and M folders, read after this run wrote its own`, every view of this run `kept, written, marked and read back by this run`, the 34 viewpoints the NWCs brought `kept, not this tool's`, and `removals : 0` on a fresh NWF
513. Look for, in the `VIEWS TREE` block: `team map   :` naming the teams file with 4 teams, `models     : AR Architecture, EL Electrical, ME Mechanical, ST Structure`, a `before     :` line, the tree with `A`, `B`, `C` or `No priority` first, then `<Team> vs <Team>` in the map's order, then the views named exactly by their tests, `Over 150mm` only inside a pair carrying it, and SEVEN `CHECK` lines
514. Look for, on the seven: `CHECK 1` to `CHECK 4`, `CHECK 6` and `CHECK 7` each `0 broke it`, `CHECK 5` with the note `the mirror rule found 0 mirrors in this matrix, so this check proves nothing here`, and the last line `7 of 7 hold`. Any `FAILED CHECK` line or any `RAN IN PART` line: send the log
515. Look for, in the `.tsv` beside the log: a row per line of the `VIEWS TREE` block, happening `views tree`, numbered 1 upward and whole where the `.log` cut the tree at 300 lines and said so
516. Look for, in the `TIMING` block: the `VIEWS` seconds, and send the number against the baseline's 7487.104 s and the whole run's against 7925.970 s. Nothing here says what they read, a first run on a fresh NWF has not been measured, and P18 measured 0.112 s a view in a tree of 44 to 53
517. Look for, in the `VIEWS` seconds line: `the step's seconds went:` naming every part, among them `framing the camera on the clashes`, `marking the view and its folders`, `taking the inventory of the tree`, `removing the views of earlier runs` and `reading the tree for the VIEWS TREE block`, and the seconds no part holds near zero
518. Open the NWF it wrote in Navisworks and open Saved Viewpoints. Look for: the four folders the NWCs brought, then `A`, `B`, `C` or `No priority`, each holding `<Team> vs <Team>` folders, each holding one view per test and an `Over 150mm` folder only under a pair with Mechanical or Electrical
519. Right click any view the tool made and open Comments. Look for: ONE comment, by `Parsons NWC Federator`, reading `Made by the NWC Federator and replaced on its next run. Rename it, move it or add a comment to keep it.` then a line starting `NWC-FEDERATOR-VIEW v1 stamp=`. Do the same on one of its folders: the same shape
520. Press a view of many clashes, P16's press by hand: every clash of the test inside the view, the shown models dimmed, the clashing items solid, red and green, and the other models hidden. Press a view of one clash: it opens on Clash Detective's own camera for that clash
521. RUN B, a weekly run on a copy of the baseline NWF holding the 2813 per clash viewpoints of F85: copy `runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf` beside a copy of its NWCs under a new folder, and before the run, by hand in Navisworks, save three views of your own, one at the root, one inside `AR vs AR`, and one named exactly as a test inside a folder named `A`, add a comment to one per clash viewpoint two folders deep, then save and close
522. Run the group on that copy with the same XML, corrections, teams and priority files
523. Look for, in the `VIEWS INVENTORY` block: `removed, a per clash viewpoint of an earlier run` counted 2812, the commented one `kept, not this tool's` and named, your three views `kept, not this tool's`, the per clash pair folders `removed, a folder of this tool's that this run empties`, and `removals :` with the count, deepest first
524. Look for, in the `VIEWS` lines after it: `N of N RemoveAt call(s) made` with the two counts equal, no `was not removed` line, and no `not one fewer` line
525. Look for, in the `VIEWS TREE` block: `before     :` counting 2813 per clash viewpoints of earlier runs and 3 not this tool's beside the NWCs' 34, `after      :` with the per clash viewpoints removed, `CHECK 6` holding over every item kept, `CHECK 7` reading `1 left, 1 kept and named` naming the commented one, and `7 of 7 hold`
526. Look for, in the `TIMING` block: this run's `VIEWS` seconds against Run A's, which P18 says carry about 6.3 s a view more once, the writing into the old tree before the removal, and send both
527. Open the NWF and Saved Viewpoints. Look for: the per clash folders `AR vs AR` to `EL vs UNKNOWN` gone but for the one holding the commented viewpoint, your three views where you put them, and the new tree beside them
528. RUN B2, a second weekly run on Run B's output: before it, by hand, rename one view the tool made and add a comment to another, save and close, then run the group again
529. Look for, in the `VIEWS INVENTORY` block: last week's views `removed, this tool's earlier view replaced at its place`, the renamed one `kept, this tool's and changed by a person or not provable` with `it was written as` its old name, the commented one the same with `it carries another comment beside the mark`, both named, and this run's `kept, written, marked and read back by this run`
530. Look for, in the `VIEWS TREE` block: `7 of 7 hold`, and in the panel the renamed and the commented views beside this run's
531. Look for, across Runs A, B and B2: no `CENSUS CHANGED` line, and `CENSUS NOTED` only where the VIEWS step moved the viewpoint count
532. 1A04PK FIRST RUN: a fresh NWF of 1A04PK with the same XML and files. Look for, in `VIEWS TREE`: `models     :` naming HV and FP in `Mechanical`, the pair folders `Mechanical vs Mechanical` and the like holding those tests, and any view showing a third team's model named under `CHECK 3` as allowed by Q118 A
533. P20 BY HAND, unrun: on Run B2's NWF, draw a redline on one view the tool made, save, close and run once more. Look for: that view `kept, this tool's and changed by a person or not provable` with `it carries a redline`, and the view still there after
534. P21 BY HAND, unrun: in Run B2's NWF, press Update Viewpoint on one view the tool made with nothing else changed, save, close and run once more. Look for: whether the inventory keeps or replaces it, which row F114-K7 says is replaced by design, and send what it read
535. ATTEMPT 2, B1, on any run above: where a group's `VIEWS` lines say a test `is not at` its address, or that rows `no longer lead to their result` or `have no recorded place`, look in `VIEWS BUILT` for that test `FAILED, not read whole for the views` with why and in `VIEWS INVENTORY` its earlier views `kept, its test was not read this run`, and the group not DONE. Where no such line appears there is nothing to read here. With Compact ticked, a test holding Resolved clashes still gets its view, since a row outside New and Active needs nothing read. A test whose open clashes are all result groups reads `gets no view: its open clashes are all result groups` on a `VIEWS` line and is NOT failed. A test that ran with no clash this week has its earlier view `removed, this tool's earlier view of a test that needs no view there`
536. ATTEMPT 2, B2: look in `VIEWS BUILT` for any `FAILED, it would show no model`. On 1A02MM none is expected, every clashing item's model reading, and one named here is a home this tool could not tie, worth sending
537. ATTEMPT 2, B3: look in `VIEWS BUILT` for any `could not be marked`. None is expected. Where one is, it must end `so it was removed at once`, and the Saved Viewpoints panel holds no view of that name without a comment in that folder
538. ATTEMPT 2, B4: look in each `VIEWS    the tree read` line for `kept as a person's because its comments could not be read`. None is expected. Where one is, no view or folder so counted is among the removals of `VIEWS INVENTORY`
539. ATTEMPT 2, B5: in Run B's `VIEWS TREE` block, the `after      :` line reads `as the removals reported` and `0 not removed`, its per clash viewpoint count the same as the `N of N RemoveAt call(s) made` line of step 524, and no `is counted as not removed` line, which a folder count that did not fall by exactly one writes. A count above zero names each not removed with why, and is worth sending
540. ATTEMPT 2, B6: in Run B2's `VIEWS INVENTORY` block, no view of the tool's that nobody touched reads `its camera was moved`. Every untouched view of Run B reads `removed, this tool's earlier view replaced at its place`, and whether the NWF keeps a camera at single precision is read off this step
541. Send the three logs of Runs A, B and B2 and the 1A04PK log, each named on its `log file` line, and keep the NWFs on your machine

## Proof F118 the workbook and report, added 2026-10-12

Three things the add-in pass of F118 changed. FR-036 first, because it costs two short runs of one group of one discipline, then F118-R3 on the same group, which needs Excel to hold its workbook open, then FR-041, which needs a group that runs clash tests. Since F77 a test whose side finds nothing is never created, so in the group of one discipline only the tests the first run created are in the document, and only those carry the chosen value on the second run.

1039. Build and install main once fix-F118c has merged, steps 7 to 12
1040. In Explorer copy the NWC folder of 1A04MS, which holds one discipline, to a new folder on the desktop with no NWF beside it
1041. In the add-in go to the Source step and pick that folder
1042. Press Scan
1043. Go to the Outputs step and pick a new empty folder on the desktop for the NWF, the NWD and the reports
1044. Go to the Clash step and pick the exchange XML `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`
1045. Leave Clash tolerance on `Use the value in the XML`
1046. Press Run, then OK, and wait for it to finish
1047. Look for: one line `CLASH 1830 in the file, N created, M not created, a side finds nothing`, and write down N
1048. Set Clash tolerance to `50 mm`
1049. Press Run, then OK, and wait for it to finish
1050. Look for, near the end of the log: `TOLERANCE on the report, read off the clash test in the document for 0, the clash XML for` 1830 less N, `chosen in the tool for` N, with N from step 1047. Before this fix N read under the clash XML
1051. Look for, on the line above it: the line ending `Where each block's tolerance was read is on the line below`
1052. Open in Excel the clash workbook this run wrote in the reports folder of step 1043
1053. Look for: the Tolerance cell reading `0.050m` in the blocks of the N tests the first run created, and `0.025m`, the XML's 25 mm, in the blocks of the rest. Before this fix every block read `0.025m`
1054. Leave that workbook open in Excel
1055. Press Run, then OK, and wait for it to finish
1056. Look for, in the group: `XLSX     NOT WRITTEN` with the workbook's path and `its save threw, and a file of that name is at the path. Whether it is an earlier run's file untouched or one this run cut short is UNKNOWN`
1057. Look for, after it: `WORKBOOK CHECK 1A04MS not run, writing the clash workbook threw`, and NO `XLSX     written` line for that path
1058. Look for, in the RESULT block's files written list: NOT that workbook
1059. Close Excel
1060. Set Clash tolerance back to `Use the value in the XML`
1061. Go to the Source step and pick `C:\00-NM\Federation Task\C02 + 04\C02`
1062. Press Scan
1063. In the group list untick every group but 1A02MM
1064. Press Run, then OK, and wait for it to finish
1065. Look for, in the group, one line after the `UNITS` line: `GRID     Grid Location empty on N of M clash rows` with the heights of the empty and the filled rows, or `GRID     Grid Location filled on every one of M clash rows`, and the same words on a row of the `.tsv` beside the log, happening `grid location counted`
1066. Send the logs of steps 1046, 674, 680 and 1064, each named on its `log file` line. Why the Grid Location cells are empty is UNKNOWN, and the heights of step 1065 against the grid levels are the first thing to read

## Proof F121 wave 4 part two, the regroup, a cleared name cell, the tolerance box and the close mid run, added 2026-10-13

Everything runs on copies in two new folders on the desktop, so no NWF of the real folders is changed. The regroup first, then the cleared name cell on the same table, then the tolerance box, then one run that is closed from the title bar while it goes. 1A02WL is used beside 1A02MM because it holds more than one file, so unticking one of them leaves the group in the list.

1100. Install main once fix-F121e has merged, steps 7 to 12
1101. In Explorer make a new folder on the desktop named `F121e source`
1102. Copy the NWC files of 1A02MM from `C:\00-NM\Federation Task\C02 + 04\C02` into `F121e source`
1103. Copy the NWC files of 1A02WL from the same folder into `F121e source`
1104. Make a second new folder on the desktop named `F121e out`
1105. Start Navisworks and open the tool
1106. On 1. Source pick `F121e source`
1107. Press Scan
1108. On 3. Outputs pick `F121e out` for the NWF folder
1109. Pick `F121e out` for the NWD folder
1110. Pick `F121e out` for the Excel folder
1111. In the names table on 3. Outputs double click the NWF name cell of 1A02MM
1112. Type `F121E-TYPED-NWF` and press Enter
1113. Look for: the Edited cell of 1A02MM reading `by hand`
1114. On 2. Grouping untick the Run box of 1A02MM
1115. On 1. Source untick the Use box of one file of 1A02WL
1116. On 2. Grouping look for: the Run box of 1A02MM still unticked, where before this fix it came back ticked
1117. Look for: the Run box of 1A02WL ticked
1118. On 3. Outputs look for: the NWF name of 1A02MM still `F121E-TYPED-NWF` and its Edited cell `by hand`, where before this fix it went back to the pattern's name
1119. Double click the NWF name cell of 1A02MM
1120. Press Ctrl+A
1121. Press Delete
1122. Press Enter
1123. Look for: the cell holding a name with 1A02MM in it, built the way the NWD name beside it is, and the Edited cell empty, where before this fix the cell stayed empty and read `by hand`
1124. Look for, under the names: no line `THE RUN CANNOT START` and no `The NWF name of 1A02MM cannot be used, it is empty.`
1125. Look for: the NWD name and the Workbook name of 1A02MM as they were before step 1119
1126. On 4. Clash set Clash tolerance to `Other`
1127. Leave the number box beside it empty
1128. Press `Undo auto Reviewed`
1129. Look for: no window saying `The undo stopped.`, and the line beside the button saying how many were put back of how many looked at, or `Nothing was read.`
1130. Press `Probe model properties`
1131. Press No, to read the document that is open
1132. Look for: no window saying `The probe stopped.`, and `Probe finished.` on the progress line
1133. Press Browse beside Clash XML
1134. Pick `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` of the repo
1135. Press `Sets into open model`
1136. Look for: no window saying `Building the sets stopped.`, and `Sets finished.` on the progress line
1137. Press `Tests into open model`
1138. Look for: a window saying `The clash tolerance is set to Other and no number is typed beside it.`
1139. Press OK
1140. Look for, in the log: `TOLERANCE Tests into open model did not start.` with the same words, and no `CLASH    started by hand` line after it
1141. Type `abc` in the number box
1142. Press Run
1143. Look for: a window saying `A tolerance that is not a number is not a tolerance.` with no `Parameter name` line under it
1144. Press OK
1145. Look for, in the log: `TOLERANCE the run did not start. A tolerance that is not a number is not a tolerance.`
1146. Set Clash tolerance back to `Use the value in the XML`
1147. Press Run, then OK
1148. While the run is going, press the X at the top right of the tool's window
1149. Look for: a window saying `The run is still going. Let it finish.`, and the tool's window still open behind it, where before this fix the tool's window closed under the run
1150. Press OK
1151. Click on the tool's window and press Alt+F4
1152. Look for: the same window, and the tool still open
1153. Press OK
1154. Wait for the run to finish
1155. Look for: the RESULT block, with 1A02WL in it and not 1A02MM, which step 1114 unticked
1156. Press the X at the top right of the tool's window
1157. Look for: the tool's window closes with no message
1158. Send the log of steps 1105 to 1157, named on its `log file` line

## Proof F123b the log lines, the RESULT block and the warning before a clear, FR-058, FR-059, FR-060, FR-062, FR-063, FR-167 and FR-169, added 2026-10-13

Everything runs on copies in two new folders on the desktop, so no NWF of the real folders is changed. One run of 1A02MM with the clash XML, the same again with no XML, then one run with the XML per building and discipline, then the warning with the NWF open. The UNITS line after a read that throws and the warning after a file name that throws cannot be made to happen by hand, so steps 1218 and 1242 read the ordinary lines, which Core now writes.

1200. Install main once fix-F123b has merged, steps 7 to 12
1201. In Explorer make a new folder on the desktop named `F123b source`
1202. Copy the NWC files of 1A02MM from `C:\00-NM\Federation Task\C02 + 04\C02` into `F123b source`
1203. Make a second new folder on the desktop named `F123b out`
1204. Start Navisworks and open the tool
1205. On 1. Source pick `F123b source`
1206. Press Scan
1207. On 3. Outputs pick `F123b out` for the NWF folder
1208. Pick `F123b out` for the NWD folder
1209. Pick `F123b out` for the Excel folder
1210. On 4. Clash pick the clash XML `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` of the repo
1211. Untick `Make saved viewpoints for the clashes`
1212. Untick `Skip clash when models sit off shared coordinates`
1213. Press Run
1214. Press OK on the confirm
1215. Wait for the run to finish
1216. Look for, in the log: the first CLASH line of 1A02MM reading `CLASH    1A02MM, N in the file, S skipped before the model, B left, how many of them are created is said once the sets are counted`, where before this fix it read `B to create`
1217. Look for, a few lines on: `CLASH N in the file, C created, M not created, a side finds nothing`, the one count of the tests created
1218. Look for: the UNITS line of 1A02MM reading `the document is already in Meters (m)` or naming the models set and ending `so the report is metric whatever this says.`
1219. Look for: the TOLERANCE line of 1A02MM saying `read per test out of the XML`, which is right on a run with an XML
1220. Look for, in the RESULT block: a line `pictures       : N written beside the files listed below, X MB, every size read back off the disk` straight above `files written`, where before this fix no line counted the pictures
1221. Look for: N on that line the same as the number on the `IMAGES` line of 1A02MM
1222. On 4. Clash clear the Clash XML box
1223. Press Run
1224. Press OK on the confirm
1225. Wait for the run to finish
1226. Look for: the TOLERANCE line of 1A02MM starting `TOLERANCE as each test saved in the document holds it, since no file of clash tests was picked`, where before this fix it said `read per test out of the XML`
1227. Look for: the first CLASH line of 1A02MM reading `saved in the document` and `to run`
1228. On 4. Clash pick the clash XML `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` of the repo again
1229. On 2. Grouping set Make to `One file per building and discipline`
1230. Look for: 1A02MM listed as one group per discipline
1231. Press Run
1232. Press OK on the confirm
1233. Wait for the run to finish
1234. Look for, in the `SOURCE FINDINGS` block: no `SHARED SOURCE` line naming the groups of 1A02MM, where before this fix a building split by discipline read as split by a naming error
1235. Look for: every `SOURCE MISMATCH` line, where there is one, saying `This tool reads the building code off the NWC file name` and none saying `will be called`
1236. Look for, on the `clashes found` line of the RESULT block: `of which ran no test` wherever a group's CLASH block read `tests run : 0`, where before this fix those groups were counted under `found none`
1237. Look for: the row of each such group under it reading `no test ran`
1238. On 2. Grouping set Make back to `One file per building`
1239. In Explorer open the NWF of 1A02MM in `F123b out` in Navisworks
1240. In the tool press Run
1241. Look for: the confirm reading `This will be discarded without saving:`
1242. Look for, under it: `The open file` with the full path of that NWF and `holding N models.`, as before this fix
1243. Press Cancel
1244. Send the log of steps 1204 to 1243, named on its `log file` line

## Proof F114d the open clashes of a mirror not merged, added 2026-10-13

Core is proved by its tests and the add-in did not change. These steps read the VIEWS block
of a weekly run on an NWF that already holds both tests of each mirrored pair, which under
your answer A to Q142 merges no mirror, so each mirror keeps its own clashes and gets no
view. Rows F114-K34 and F114-R1. Everything runs on a copy.

1300. Install main once fix-F114d has merged, steps 7 to 12
1301. Copy the folder Run A of step 508 ran in, with the NWF it wrote, to a new folder on the desktop named `F114d mirrors`
1302. Run the group 1A02MM on `F114d mirrors` with the same XML, corrections, teams and priority files as Run A, the viewpoints box ticked
1303. Look for, in the `MIRROR` lines of the CLASH step: the 59 pairs, and write down the names of the mirrors whose clashes were not merged
1304. Look for, in the `VIEWS 1A02MM` block: the line starting `mirrored tests, so no view :` and its count
1305. Look for, where that count is above 0, the next line: `open clashes of those mirrored tests, in no view :` with a number equal to the sum of the first numbers on the mirror lines under it
1306. Look for, under it: one line per mirror shaped `<name> (mirror) : N open and in no view, M at a status no view shows`, each a mirror written down at step 1303
1307. Open the clash workbook of 1A02MM the run wrote and find the block of the first mirror named at step 1306
1308. Look for: its rows at New or Active counted equal to that mirror's N, and its other rows counted equal to its M
1309. Where step 1304 read 0, no mirror not merged held a clash on this run, so the new lines are proved by the Core tests alone. Say so when you send the log
1310. Send the log of steps 1302 to 1309, named on its `log file` line

## Proof of the round of 2026-10-13, F151 to F156, added 2026-10-13

One section for the five fixes of your message of 2026-10-13 and F156, which the install of this round needed. Core is proved by its tests on the laptop. What only Navisworks or Excel can show is here. Everything runs on copies in new folders on the desktop, so no NWF of the real folders is changed. The loop's window run of 1A02MM on its own copy, 2026-10-14, is in `steps\runs\94\item1-C02`, and what it proved is in steps\history\log.md.

FILES TO COPY BY HAND, once, before step 1401: the NWC files of 1A02MM, the four files whose names start `1104-PAR-1A02MM-`, from `C:\00-NM\Federation Task\C02 + 04\C02` into a new folder on the desktop named `F151 source`. Copy, never move. Nothing else is copied: the clash matrix, the priority list and the by design pairs are built into the tool since F154 and F155.

BUILD AND INSTALL ONCE FOR THE WHOLE ROUND, in VS Code, after fix-F156 has merged. Close Navisworks first.

1390. Open VS Code
1391. Press File, then Open Folder, and open `C:\Users\bader\Documents\GitHub\PAR_NWC-Federator-V2`
1392. Press Terminal, then New Terminal
1393. Type `git checkout main` and press Enter
1394. Type `git pull` and press Enter
1395. Type `dotnet build ParsonsNwcFederator.sln -c Release` and press Enter
1396. Look for: `Build succeeded.` with `0 Error(s)`. If it fails, do steps 9 to 21
1397. Type `powershell -ExecutionPolicy Bypass -File build\install.ps1` and press Enter
1398. Look for: `All 15 expected files are present.`, no line with `needs`, and `The installed add-in reads the stamp` with the first eight characters of the last commit of main. Then steps 1511 to 1513 are done too

### F151, no copy of the log beside the NWF

1400. Build and install main once for the round, steps 1390 to 1398, if not done yet
1401. Look for: `F151 source` on the desktop holds the four NWC files of 1A02MM, copied by hand as said above
1402. Look for: no other file in `F151 source`
1403. Make a second new folder on the desktop named `F151 out`
1404. Start Navisworks and open the tool
1405. On 1. Source pick `F151 source`
1406. Press Scan
1407. On 3. Outputs pick `F151 out` for the NWF folder, the NWD folder and the Excel folder
1408. Look for, on the grey line of 3. Outputs: `The log is written to` and a path under `AppData\Local\ParsonsNwcFederator\logs`, ending with a full stop and with no words `copied next to the NWF folder`
1409. Press Run
1410. Press OK on the confirm
1411. Wait for the run to finish
1412. In Explorer open `F151 out`
1413. Look for: no file whose name starts `run-` and ends `.log` or `.tsv`
1414. Open the log the tool names on its `log file` line
1415. Look for: the RESULT block is the last block, and no line after it starts with `COPY`
1416. Look for, in RESULT: the line under `pictures` reading `The logo copied beside each report is not counted`
1417. In the tool, on 1. Source, press File beside the NWFs box and pick the NWF of 1A02MM in `F151 out`
1418. Press Run NWFs
1419. Press OK on the confirm and wait for the run to finish
1420. Look for: still no `run-*.log` file in `F151 out`
1421. Send both logs, named on their `log file` lines

### F152, the workbook's pictures as linked pictures

Only Excel can show these two things: that the pictures show, and that Excel asks nothing when it opens a workbook of thousands of linked pictures. Write down the exact words of any question or bar Excel shows.

1430. Build and install main once for the round, steps 1390 to 1398, if not done yet
1431. Make a new folder on the desktop named `F152 out`
1432. Run 1A02MM from `F151 source` as in steps 1404 to 1411, with `F152 out` picked for the NWF, NWD and Excel folders
1433. Look for, in the log of that run: a line starting `XLSX     ` and a number, then `linked pictures in the workbook, each pointing at its jpg in`, the `_files` folder in `F152 out`, and `None is embedded, so they show only while that folder stays where it was written`
1434. Look for: that number the same as the number of pictures on the `IMAGES` line of 1A02MM
1435. In Explorer open `F152 out` and double click `1104-PAR-1A02MM-ZZZ-BM-MOD-000001.xlsx`
1436. Note how many seconds Excel takes to show the sheet
1437. Look for: no question, no yellow or red bar and no message about links, security or updating. Write down the exact words of any that shows
1438. Look for: a picture in column A of the first clash row of the first block, over the Image cell, running a little into the row below
1439. Look for: it is the same picture as `cd000001.jpg` in the `_files` folder
1440. Press Ctrl+End and scroll up to the last block's clash rows
1441. Look for: their pictures show too
1442. Click the part of an Image cell the picture does not cover
1443. Look for: the jpg of that row opens
1444. Close Excel and answer Do not save
1445. In Explorer rename the `_files` folder to `moved`, open the workbook again and look for: the pictures no longer show, which is what the client's own file does. Close Excel without saving and rename the folder back
1446. Send the log, the seconds of step 1436 and the words of any question from step 1437

### F153, no workbook rows for tests with no clash

1450. Build and install main once for the round, steps 1390 to 1398, if not done yet
1451. Make a new folder on the desktop named `F153 out`
1452. Run 1A02MM from `F151 source` as in steps 1404 to 1411, with `F153 out` picked for the NWF, NWD and Excel folders
1453. Look for, in the `WORKBOOK CHECK 1A02MM` block of the log: `CHECK    1 sheet,` then a number of test blocks `with clashes`, and no line saying a row holds a test with no clash
1454. Look for, under it: `BLOCKS   ` and the same number, `in the workbook, one for every test with a clash. The other` and how many found none
1455. Look for, in the `COVERAGE` block of 1A02MM: no `COVERAGE FAILED` line for a test that found no clash
1456. Open `1104-PAR-1A02MM-ZZZ-BM-MOD-000001.xlsx` in `F153 out` in Excel
1457. Press Ctrl+End
1458. Look for: the last row used is a clash row of the last block, and no row below it names a test
1459. Close Excel and answer Do not save
1460. Open `1104-PAR-1A02MM-ZZZ-BM-MOD-000001.html` in `F153 out` in a browser and look for: a block for every test, 1830, the tests with no clash among them
1461. Open the internal workbook, the one whose name ends `Internal`, in Excel, and look for: the Coverage sheet lists every test of the file, one row each
1462. Send the log

### F154, the priority and by design files built in

1470. Build and install main once for the round, steps 1390 to 1398, if not done yet
1471. Start Navisworks and open the tool
1472. On 4. Clash look for: no Priority CSV row and no By design CSV row
1473. Look for, under Mark by design connections as Reviewed: the grey line `Column on foundation, door in wall, valve in pipe. Built in list`
1474. Tick Mark by design connections as Reviewed
1475. Make a new folder on the desktop named `F154 out`
1476. Run 1A02MM from `F151 source` as in steps 1404 to 1411, with `F154 out` picked for the NWF, NWD and Excel folders
1477. Look for, in the log: `PACK     the built in matrix pack 1104-PAR, holding by-design-pairs.csv, clash-priority-map.csv`
1478. Look for: `PRIORITY read the built in matrix pack 1104-PAR, clash-priority-map.csv, 1830 rows` and no PRIORITY line naming a problem
1479. Look for: a line with `rule B reads the built in matrix pack 1104-PAR, by-design-pairs.csv, 55 pairs`
1480. Open the workbook of 1A02MM in `F154 out` in Excel and look for: the heading Priority in column T beside the first block, and A, B or C on its clash rows. Close Excel and answer Do not save
1481. Look for, in the RESULT block: the line of clashes by priority
1482. Send the log

### F155, the clash matrix built in, the tolerance from the drop down alone

1490. Build and install main once for the round, steps 1390 to 1398, if not done yet
1491. Start Navisworks and open the tool
1492. On 4. Clash look for: no Clash XML row, no Browse button and no Teams line
1493. Look for: the drop down Clash tolerance reading `25 mm`, and under it the grey line `Set on every test of the run. Results and statuses are kept`
1494. Open the drop down and look for: `25 mm`, `50 mm` and `75 mm` and nothing else
1495. Leave it on `25 mm`
1496. Look for, on 4. Clash: `Built in matrix 1104-PAR. It holds` then its sets and `1830 tests.`
1497. Make a new folder on the desktop named `F155 out`
1498. Run 1A02MM from `F151 source` as in steps 1404 to 1411, with `F155 out` picked for the NWF, NWD and Excel folders
1499. Look for, in the log: `MATRIX   the built in matrix pack 1104-PAR, 1104-PAR_CLASH_AllInOne_25mm_FIXED.xml holds` and `1830 tests.`
1500. Look for: `RUN      the clash matrix is the built in matrix pack 1104-PAR, 1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`
1501. Look for: `MATRIX   the corrections are read from the built in matrix pack 1104-PAR, 1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt`
1502. Look for: `TEAMS    the teams are read from the built in matrix pack 1104-PAR, 1104-PAR_CLASH_AllInOne_25mm_FIXED.teams.txt`
1503. Look for: a line starting `TOLERANCE 25 mm chosen in the tool, so it beats both the built in matrix and the document`
1504. Open the workbook of 1A02MM in `F155 out` in Excel and look for: the Tolerance cell of the first block reads 25 mm in the document's units, `0.025m` or `0.082ft`. Close Excel and answer Do not save
1505. Run 1A02MM again the same way, the NWF now in `F155 out`, and look for: the group reads `Weekly run plus XML`, and a TOLERANCE line naming `already in the document`
1506. Send the log

### F156, the install refused every build since F152

1510. Build and install main once for the round, steps 1390 to 1398, if not done yet
1511. Look for, in its output: `All 15 expected files are present.`
1512. Look for: no line with `needs System.IO.Compression`
1513. Look for: `The installed add-in reads the stamp` and the first eight characters of the last commit of main
