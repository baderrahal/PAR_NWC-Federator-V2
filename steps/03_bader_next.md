# 03 Bader next

The closed items of this file moved to steps\history\03_bader_next-closed.md on 19 Oct 2026, whole and in their order, F242, Bader's round of 19 Oct b.

## The proof runs of the round of 20 Oct evening, F246 to F248, C02

Simple words. All three fixes are on main, so one build and one install prove them. Run 1 is one group, 1A02MM of C02, on the
NWC page, in a fresh folder whose outputs start empty. Run 2 is the whole C02 package in the same folder, the round's one full
run. You chose on 20 Oct that the proofs wait for you, since your project folder now holds your own outputs and every copy the
loop makes carries them. Plain Navisworks Manage 2025 from the Start menu, never the BIM360 one. Nothing here writes into your
project folder.

1. Install the build of main as the Build and install section below says.
   Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`, the install's last line saying it finished, and the stamp
   of main after pull request 330.
2. In File Explorer make C:\NWC-PC-Test\work\proof-2026-10-20b and in it four folders, NWC, NWF, NWD and Clash report, each
   holding an empty folder C02.
   Look for: the four folders, each with C02 and nothing in it.
3. Copy every NWC file of your project's NWC\C02 folder into proof-2026-10-20b\NWC\C02.
   Look for: the four 1A02MM files, AR, EL, ME and ST, among them, and NWF, NWD and Clash report still empty.
4. Start Navisworks Manage 2025 from the Start menu, open the tool, type C:\NWC-PC-Test\work\proof-2026-10-20b\NWC\C02 as the
   source folder of the NWC page and press Scan.
   Look for: the groups of C02 on the Grouping step, 1A02MM among them.
5. On the Grouping step untick every group but 1A02MM.
   Look for: one group ticked.
6. Point the NWF, NWD and Excel boxes of the Outputs step at proof-2026-10-20b\NWF\C02, NWD\C02 and Clash report\C02.
   Look for: the three boxes reading those folders.
7. On the Clash step tick `Mark by design connections as Reviewed`, leave the tolerance at 25 mm and every other box as it
   opens, viewpoints ticked.
   Look for: the by design box ticked and the tolerance reading 25 mm.
8. Press Run, then OK on the confirm, and click nothing more.
   Look for: the confirm counting 1869 tests from the built in matrix, one Navisworks opening and closing, and the run ending
   with no box.
9. Open the run's log and search for `PACK    the pack's rules`.
   Look for: `a test of one set code is left out for AR and ST alone`.
10. Search the log for `SELF    `.
   Look for: `SELF    39 tests of a set against itself added to the 1830 of the matrix, one for every set of Mechanical and
   Electrical, Mechanical 25, Electrical 14, each at priority A, 0 not made`, and the line naming BLD-EL-Telecom Fixtures and
   BLD-EL-Telephone Devices as one question.
11. Search the log for `the rule of two set codes`.
   Look for: left out codes AR and ST only, `the pack's rules leaving out AR and ST against themselves alone`, and the tests of
   one set code that run counted by code, EL and ME among them where this group holds those models.
12. Search the log for `BY DESIGN 1A02MM`.
   Look for: `pairs in the file : 38` and `moved from pairs of one set code :` with a number, a nought written as 0.
13. Search the log for `of the 39 tests of a set against itself given priority A`.
   Look for: `39 of the 39`.
14. Open proof-2026-10-20b\NWF\C02\ the 1A02MM NWF in Navisworks and open Clash Detective.
   Look for: the self tests by name, such as `BLD-EL-Conduits & Conduit Fittings-vs-BLD-EL-Conduits & Conduit Fittings`, each
   run with its count, tests of ME against ME and EL against EL run, and no test of AR against AR or ST against ST.
15. Open Saved Viewpoints, the folder Clash Status.
   Look for: an Architecture folder under the letter of each test of ST against AR with an open clash, no test of a service
   against AR in Architecture, the views of BLD-Security Devices under Electrical, Other empty, and the self tests' views under
   A, in Mechanical or Electrical.
16. Open the clash Excel in Clash report\C02 and the HTML report beside it.
   Look for: each self test with Priority A in the Excel, and the same clash count per self test in the Excel, the HTML and
   Clash Detective.
17. Tell Code run 1 has ended. Code reads the log, the NWF, the Excel, the HTML and the viewpoints, reconciles the counts,
   checks five random clashes by element ID, counts how many self test clashes are a pipe, duct, tray or conduit against a
   fitting it connects to, and reports.
   Look for: Code's report naming what passed, what failed and what was skipped with why, and no check it did not run.
18. Run 2. On the Grouping step tick every group of C02, then press Run and OK, and click nothing more.
   Look for: one Navisworks opening and closing per group, and the run ending with no box and a RESULT block.
19. Tell Code run 2 has ended. Code closes the round on the outputs: the NWFs, the Excel, the HTML and the viewpoint tree judged
   against Q229 to Q232, the counts reconciled, five random clashes checked by element ID, and what was skipped named with why.
   Look for: Code's close of the round, and its answers in the form Q233.

## The proof run of the round of 20 Oct, F243 to F245, C06 1B06PS and 1B06WL

Simple words. One run, two groups of C06, on the NWF page, in a copy of the whole 19 Oct run Code made, so the old dated and
Internal files are there to be cleaned and nothing of the 19 Oct run is touched. 1B06PS has its AR on Revit's internal origin and
no ME model. 1B06WL has no AR, so EL is its reference, its ME 30.2 m off, with CAD imports and Generic Models. This run replaces
the F243 and F244 runs above. Plain Navisworks Manage 2025 from the Start menu, never the BIM360 one. Code has already copied
final-2026-10-19 to C:\NWC-PC-Test\work\proof-2026-10-20 and listed Report\C06 in proof-2026-10-20\before.txt.

1. Install the build of main as the Build and install section below says.
   Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`, and the install's last line saying it finished.
2. Close every Excel window, so no file of the run is held open.
   Look for: no Excel on the taskbar.
3. Start Navisworks Manage 2025 from the Start menu and open the tool.
   Look for: the tool's window with the NWC page and the NWF page.
4. Open the NWF page, type C:\NWC-PC-Test\work\proof-2026-10-20\Report in the Report folder and press Read.
   Look for: C06 and C07 with their NWFs, nothing ticked.
5. Under C06 tick `1104-PAR-1B06PS-ZZZ-BM-MOD-000001` and `1104-PAR-1B06WL-ZZZ-BM-MOD-000001`, nothing else.
   Look for: two NWFs ticked, and every step box, Refresh, Run tests, Pictures, Viewpoints and Reports, ticked as the page opens.
6. Press Run, then OK on the confirm, and click nothing more.
   Look for: one Navisworks opening and closing for 1B06PS, then one for 1B06WL, and the run ending with no box.
7. Tell Code the run has ended. Code writes after.txt, the list of what was deleted, added and overwritten, checks the folder
   against F243, opens C06 ME checks.xlsx and C06 AR checks.xlsx against the run's coordinate list and log, and reports.
   Look for: Code's report naming what passed, what failed and the questions, and no check it did not run.

The NWFs of the copy link their NWC files by the path they were built with, final-2026-10-19\Parent. Whether Navisworks reads
those or the copies in proof-2026-10-20\Parent is UNKNOWN, and either holds the same files, so the run proves the same.

## F244, the discipline workbook copied from the mock

Simple words. One group, 1B06PH of C06, on the NWF page, on a copy of the 19 Oct Report folder, because the mock shows its ME
models: ME-000002 118.7 m from AR with two CAD files and one Generic Models family. The add-in change is the CHECKS line alone, so
one build, one install and one run prove it. Steps 1 and 2 need no Navisworks.

1. Install the build of branch fix-F244 as the Build and install section below says.
   Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`, and the install's last line saying it finished.
2. In File Explorer copy the folder C:\NWC-PC-Test\work\final-2026-10-19\Report to C:\NWC-PC-Test\work\f244\Report.
   Look for: f244\Report\C06 holding the Backup folder with `1104-PAR-1B06PH-ZZZ-BM-MOD-000001.nwf`.
3. Start Navisworks Manage 2025 from the Start menu, open the tool, open the NWF page, type C:\NWC-PC-Test\work\f244\Report
   in the Report folder and press Read.
   Look for: C06 and C07 with their NWFs, nothing ticked.
4. Tick the NWF `1104-PAR-1B06PH-ZZZ-BM-MOD-000001` of C06 alone, press Run and OK, and click nothing more.
   Look for: one Navisworks opening and closing, and the run ending with no box.
5. Open the run's log and search for `CHECKS   read back`.
   Look for: four lines, one each for AR, ST, ME and EL of C06, the ME one reading `CHECKS   read back N of N rows to fix and
   M of M element ID rows, ME of C06` with the two numbers of each pair alike, and no line with `DIFFER` in capitals.
6. In the same log search for `DISCIPLINE REPORT C06`.
   Look for: a line starting `ME  ` that says its rows to fix, its element IDs and, for each check, the models or buildings
   checked and how many have an issue, and the same numbers as the CHECKS line of ME.
7. Open f244\Report\C06\`C06 ME checks.xlsx` in Excel.
   Look for: four tabs in this order, Summary navy, Models cyan, To fix red, Element IDs grey, no gridlines, and on Summary
   `C06   ME model checks` in white on navy in B1.
8. Open docs\design\discipline-workbook-mock.pdf from the repo beside it.
   Look for: the same colours, fonts, column widths, chips and layout on every sheet. The mock's columns read a little
   narrower, about 0.7 of a character, since its script stored the widths the way openpyxl does.
9. On Models find the row of ME-000002.
   Look for: Status FIX in red, Compared with AR, Off the reference about 118.7 on the light red fill, CAD imports 2 and
   Generic Models 1 on the light amber fill, and ME-000001 of the same building with its own distance, about 8.1, with no fill.
10. On To fix read the first row.
   Look for: Priority High, 1B06PH, ME-000002, Coordinates, a Finding starting `118.7 m from the AR model. ME on site`, Value
   118.7 and Unit m.
11. On Element IDs filter Model to ME-000002.
   Look for: one row per element, the Element ID cells numbers with no green triangle, 1819523 among them.
12. Copy the tool's logs off this PC, as before, and send them with the workbook.

## F243, one name per output, overwritten, no dated and no internal files

Simple words. One group, 1B06BC of C06, on the NWF page, on a copy of the 19 Oct Report folder, so every old name is there to
be cleaned and nothing of the 19 Oct run is touched. It comes first because one build, one install and one run prove all
seven points. Steps 1 and 2 need no Navisworks.

1. Install the build of branch fix-F243 as the Build and install section below says.
   Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`, and the install's last line saying it finished.
2. In File Explorer copy the folder C:\NWC-PC-Test\work\final-2026-10-19\Report to C:\NWC-PC-Test\work\f243\Report.
   Look for: in f243\Report\C06\Log the four files of 1B06BC, `1104-PAR-1B06BC-ZZZ-BM-MOD-000001.xlsx`, the same name with
   Internal after it, and both again with 2026-10-19 062133 after the name, thirty files `Models not on the same shared coordinates, run ...txt`, and in
   f243\Report\C06 the files `C06 AR checks 2026-10-19 064154.xlsx` and `C06 clash count check 2026-10-19 064158.xlsx`.
3. Open f243\Report\C06\`C06 clash count check.xlsx` in Excel and leave it open.
   Look for: Excel showing it, and no change of its date in File Explorer.
4. Start Navisworks Manage 2025 from the Start menu, open the tool, open the NWF page, type C:\NWC-PC-Test\work\f243\Report
   in the Report folder and press Read.
   Look for: C06 and C07 with their NWFs, nothing ticked.
5. Tick the NWF `1104-PAR-1B06BC-ZZZ-BM-MOD-000001` of C06 alone, press Run and OK, and click nothing more.
   Look for: one Navisworks opening and closing, then a box ending `A file was open in another program and left as it was:` and
   `Close C:\NWC-PC-Test\work\f243\Report\C06\C06 clash count check.xlsx and run the report again`.
6. Open the run's log and search for `OLD      deleted`.
   Look for: `OLD      deleted` lines for the four 1B06BC files of Log, the thirty dated lists of models, `C06 AR checks 2026-10-19
   064154.xlsx` and the other three dated discipline workbooks, `C06 clash count check 2026-10-19 064158.xlsx`, and one
   `OLD      renamed ...Backup\1104-PAR-1B06BC-ZZZ-BM-MOD-000001 2026-10-19 062133.nwf to ...Backup\1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf`.
   Each deleted line names what the file was, such as `a dated internal workbook`. No line names a file of another group,
   since only 1B06BC is in this run, and one line `OLD      ...\C06 and 1 package folder read for the old names of 1 group and 4 disciplines of this run, N files deleted`.
7. Open the log of 1B06BC's own Navisworks, the one its GROUP block in RESULT names, and search for `IMAGES   deleted`.
   Look for: `IMAGES   deleted ...\Clash report\1104-PAR-1B06BC-ZZZ-BM-MOD-000001_files with its 173 pictures and logo.jpg,
   before this run's clash report of the group is written`.
8. In the same log search for `XLSX     links`.
   Look for: `XLSX     links    P pictures linked, E links expected, E read, 0 missing`, E twice P, a cell link and a click
   link a picture.
9. Open f243\Report\C06\Clash report.
   Look for: `1104-PAR-1B06BC-ZZZ-BM-MOD-000001.xlsx` beside its `.html` and its `_files` folder, both written now, and no
   name with a date in it.
10. Open that xlsx and click the picture of its first clash row.
   Look for: a jpg opening from Clash report\1104-PAR-1B06BC-ZZZ-BM-MOD-000001_files.
11. Open f243\Report\C06\Log.
   Look for: no xlsx of 1B06BC, no file with Internal in its name for 1B06BC, one `Models not on the same shared
   coordinates.txt` whose first line names this run's start and whose second reads `The check judged 1 of 1 group of this
   run.`, and the run-....log and .tsv of this run beside the old ones. In the log of 1B06BC's own Navisworks the line
   `LIST     skipped  this is a group's own Navisworks`, and in the run's log one `LIST     attempt` for the C06 Log folder.
12. Open f243\Report\C06\Backup.
   Look for: `1104-PAR-1B06BC-ZZZ-BM-MOD-000001.nwf` with today's time and no date in its name, and the other groups' dated
   backups as they were.
13. Look at f243\Report\C06 in File Explorer.
   Look for: `C06 AR checks.xlsx`, `ST`, `ME` and `EL` written now, no dated checks and no dated clash count check, and
   `C06 clash count check.xlsx` still with its 19 Oct time, since it was open.
14. Close Excel, then on the NWF page tick the same NWF again, press Run and OK.
   Look for: no box at the end, `C06 clash count check.xlsx` written now, and in the log `OLD      ...\C06 and 1 package folder read for the
   old names of 1 group and 4 disciplines of this run, none of them was there`.
15. Copy the tool's logs off this PC, as before, and send them.

## The round of 19 Oct, F231 to F238

Simple words. One order for the round, F238's: first the full run's output, read where it sits with nothing to build or install,
then the steps of each fix on the build after F237, newest first, F236, F235, F234, F233 and F232, each opening the tool from
the Start menu on a copy of the files. F231 changed no code and has no steps. The steps of the round of 18 Oct below name step
5 and the NWF picker, which F232 took out: on this build step 5 is step 1. Packages and the picker is the NWF page's tree.

### The round of 19 Oct 2026, the full run on final-2026-10-19

Read off what the run left in C:\NWC-PC-Test\work\final-2026-10-19\Report, main 6faff29, the NWC page then the NWF page, C06
and C07. These come first, since they need nothing built or installed, steps 8 and 9 alone opening Navisworks. The steps of each
fix follow, F236 first.

1. In File Explorer open C:\NWC-PC-Test\work\final-2026-10-19\Report\C06 and open `C06 AR checks.xlsx`, `C06 ST checks.xlsx`,
   `C06 ME checks.xlsx` and `C06 EL checks.xlsx`, the four without a date in the name.
   Look for: in each, row 1 `C06 C06 itself <discipline> model checks`, the seven checks from Coordinates to Generic Models,
   and Issues AR 10, ST 28, ME 17, EL 14. In ST, rows 1 and 2 on a red fill, 1B06BC at `34.014 m, ST on T1, AR on Internal`
   and `34.472 m, ST on BCTANK, AR on Internal`. In EL, four One discipline group rows, 1B06BS, 1B06KI, 1B06PG and 1B06PW.
2. In the same folder open `C06 clash count check.xlsx`.
   Look for: no red row, the row `Total C06` reading 3904, 3904, 3904, 2121, 3904, 0, and Reviewed 1783, then under
   `Groups not built for one discipline` 1B06BS, 1B06KI, 1B06PG, 1B06PT, 1B06PW and 1C06PK.
3. Open Report\C07\`C07 clash count check.xlsx`.
   Look for: no red row, `Total C07` reading 1549, 1549, 1549, 986, 1549, 0, and Reviewed 563, and six groups not built,
   1A07PP, 1C07BS, 1C07KI, 1C07PK, 1C07PW and 1D07PT.
4. Open Report\C06\NWF.
   Look for: 15 NWF files, and none named 1B06BS, 1B06KI, 1B06PG, 1B06PT, 1B06PW or 1C06PK.
5. Open Report\C06\Log\`1104-PAR-1B06PK-ZZZ-BM-MOD-000001 2026-10-19 062626.xlsx`, the one the NWF page wrote.
   Look for: one sheet, the test BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors first, its Clashes 655.
6. Press Ctrl+F and find Clash91 under that test.
   Look for: the row Reviewed, Item 1 `Element ID: 6608970`, Item 2 `Element ID: 3910160`.
7. Click that row's picture.
   Look for: cd000098.jpg, a red pipe standing through a green floor.
8. Start Navisworks Manage 2025 from the Start menu and open Report\C06\NWF\`1104-PAR-1B06PK-ZZZ-BM-MOD-000001.nwf`.
   Look for: it opens with no message.
9. Open Clash Detective and pick the test BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors.
   Look for: 655 clashes, and Clash91 at Reviewed with its two items Element ID 6608970 and 3910160 on the Results tab.
10. Open Report\C07\Log\`1104-PAR-1C07P1-ZZZ-BM-MOD-000001 2026-10-19 063457.xlsx`.
    Look for: one sheet, BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Walls among its tests.
11. Press Ctrl+F and find Clash2 under that test.
    Look for: Reviewed, Item 1 `Element ID: 5709507`, Item 2 `Element ID: 456237`.
12. Click that row's picture.
    Look for: cd020002.jpg, a red pipe against a green wall.
13. Open steps\logs\`2026-10-19-C06-C07-final-comparison-summary.txt` in the repo.
    Look for: four lines, C06 NWC, C06 NWF, C07 NWC and C07 NWF, each with cd, excel and html the same on both sides, 3904 or
    1549, and only views, pictures and reviewed moving.

### F236, the clash count check

1. After a run, open the Report folder's C06 folder and open `C06 clash count check.xlsx`.
   Look for: one row per group per test with clashes, its columns from Building to Resolved, a bold Total under each group
   and one Total C06 at the end.
2. Filter the table for rows with a red fill, then search the run's log for `COUNTS   read back`.
   Look for: no red row and no cell reading UNKNOWN, in the log no line starting `COUNTS   DIFFER` or `COUNTS   UNKNOWN`, and
   the read back line ending `as written, of C06`.
3. Pick one row and open that group's Excel and HTML on the same test.
   Look for: the same Excel rows, HTML rows and statuses as the row says, and in Clash Detective the same clash count.
4. Scroll below the table.
   Look for: the groups not built for one discipline by name, for each group the tests left out by one discipline and by
   two set codes, and no section `Groups whose counts could not be read`.

### F235, the picture caps over 750 clashes and penetrations on by default

1. Start Navisworks Manage 2025 from the Start menu and open the tool.
   Look for: on 4. Clash the box `Mark penetrations as Reviewed` ticked, and on the NWF page the same box, ticked, under the
   five step boxes. `Mark by design as Reviewed` unticked.
2. After a run of 1B06PK, open its log and search for `IMAGES` and `above which a test's pictures are capped`.
   Look for: a line only for tests of more than 750 clashes, each naming its clashes and the 750.
3. Open 1B06PK's Excel and look through every test, all of them 750 clashes or fewer in 1B06PK, and search its log for
   `NOTICE` near `IMAGES`.
   Look for: every clash row at New, Active or Reviewed, the statuses that take a picture, has its picture, and no NOTICE
   of the memory or the GDI stop. Approved and Resolved rows take none, Q194.
4. In Navisworks, open 1B06PK's NWF, Clash Detective, and pick a pipe through a wall of 150 mm or less.
   Look for: status Reviewed, and a larger pipe through a wall still New.

### F234, the discipline workbooks

1. After a run of the NWC page on C06, open the Report folder's C06 folder and open `C06 ST checks.xlsx`.
   Look for: row 1 `C06 C06 itself ST model checks`, row 2 the run date, row 3 `Reference discipline AR`, then the summary of
   seven checks from Coordinates to Generic Models, one blank row, then the issues table.
2. Click a cell of the issues table and scroll down.
   Look for: its header row stays in place, it carries filter arrows, and High rows are light red and Medium rows light amber.
3. Find the rows of building 1B06BC.
   Look for: two Coordinates rows, `34.014 m, ST on T1, AR on Internal` and `34.472 m, ST on BCTANK, AR on Internal`, each with
   `Export again on the project shared coordinates`.
4. Add up the Issues column of the summary.
   Look for: the sum equals the issue rows of the table, and each check's count equals its rows when you filter Check by it.
5. Open `C06 AR checks.xlsx`.
   Look for: a 1B06BC row `on Revit internal origin` with `Export on the project shared coordinates`.
6. Open `C06 EL checks.xlsx`.
   Look for: a row `only discipline in this group, not federated` for 1B06PG with its file name, and no row of a file that passed.
7. Open the Report folder's C07 folder and open each of its four workbooks, the names ending `checks.xlsx`, or the newest dated
   one where a second run left one beside it.
   Look for: in each, the Issues column of the summary adds up to the rows of the issues table.

### F233, one discipline groups are not built

1. On 1. Packages read a parent folder of copies holding a building of one discipline, such as 1B06PG of C06, and tick its
   community.
   Look for: on 2. Grouping its row reading `One discipline, not built` with its box off and greyed.
2. Press Run and OK, and click nothing more.
   Look for: in the log `NOT BUILT  C06  1B06PG  EL  1104-PAR-1B06PG-ZZZ-EL-MOD-000001.nwc  one discipline, not built`, and in
   RESULT `not built      C06 : 1 group not built for one discipline`.
3. Open the Report folder's NWF, NWD, Log and Clash report folders of that package.
   Look for: no file of the group of one discipline written by this run.
4. Put a copy of an NWF of a group of one discipline from an earlier run into the package's NWF folder, read the NWF page and
   tick it with the others, then press Run and OK.
   Look for: in the log the NOT BUILT line, the line `the NWF ... of an earlier run is there, and it is not deleted and not run`,
   the NWF the same size and date as before, RESULT counting it under `not built` and on its `one discipline :` line, the
   line under the Run button reading `... ran, 1 not built for one discipline`, and one new dated copy of that NWF in the
   Backup folder, which the page makes of every NWF before it opens it, Q217.
5. Rename one NWC of a building of two disciplines or more in the parent folder of copies, then run its NWF on the NWF page,
   then put the name back.
   Look for: in the log `NOT JUDGED` naming that group and `1 file it links is not on the disk`, and no NOT BUILT line for it.

### F232, packages first, one route per page

1. Start Navisworks Manage 2025 from the Start menu and open the tool.
   Look for: the NWC page with four tabs, 1. Packages, 2. Grouping, 3. Outputs, 4. Clash, and no Source tab.
2. On 1. Packages type a parent folder of copies that holds NWC files itself and one community folder, and an empty Report
   folder beside it, and press Read.
   Look for: the parent named as a community with one package `<parent> itself`, the community with its package, nothing
   ticked, and the Findings panel under the tree.
3. Press Run, in the bar under the steps, with nothing ticked.
   Look for: a box saying `Nothing is ticked. Tick a community or a package first.`
4. Tick both communities.
   Look for: the Findings panel counting the NWC files of the ticked packages and naming a group of one discipline.
5. Open 2. Grouping.
   Look for: one row per group with Community, Package, Building, Files, Disciplines, Run as and Status, and a group of one
   discipline reading `One discipline, not built` with its box off and greyed.
6. Open 3. Outputs.
   Look for: the name fields and the Logo box only, no NWF folder, no NWD folder and no More, rarely changed.
7. Press Run and OK on the confirm, and click nothing more.
   Look for: one Navisworks opening and closing per ticked group, and in the log a PACKAGE GROUPS block naming the group of one
   discipline `left out, One discipline, not built`.
8. Open the NWF page.
   Look for: the Report folder with Browse and Read at the top, then the tolerance, the five step boxes and one Run, and no
   NWF picker, no File or Folder button, no Last used box and no panel on the right.
9. Press Read on the NWF page.
   Look for: a tree of community, package and NWF, one line per NWF the run of step 7 wrote, nothing ticked.
10. Tick one community and one NWF of the other community, press Run and OK.
   Look for: one Navisworks per ticked NWF, and RESULT with one GROUP block per NWF.
11. Copy the tool's logs off this PC, as before, and send them.

## The round of 18 Oct evening, F222 to F230

Simple words. Each fix of this round adds its steps here as it merges, and F230 orders them for one build, one install and one
Navisworks session. F222 first, since every later run goes through it.

### F222, a fresh Navisworks for every group

1. Start Navisworks Manage 2025 from the Start menu, as you always do, and open the tool.
2. On the NWC page, step 5, type a parent folder of copies holding one community of three or more groups and an empty Report
   folder, press Read and tick the community.
   Look for: the tree naming the community with its files and groups, nothing ticked until you tick it.
3. Press Run ticked packages and OK on the one confirm, and click nothing more.
   Look for: a second Navisworks opening, working and closing once for each group, the next only after the last has gone, and
   the confirm saying each group runs in a fresh Navisworks of its own.
4. When the live line says the run finished, open the log with Open log folder.
   Look for: one line `group N of M, <package>  <group>, starting a fresh Navisworks` a group, each with `Navisworks started in`
   and `its Navisworks is gone ... after it was told to quit`, and the line `the runner ended after M groups of 1 package in ...,
   M starts took ... in all and M quits ...`.
5. Read RESULT at the end of that log.
   Look for: `fresh starts   : M groups of 1 package`, one `group` line a group with DONE and its clashes, and one `GROUP <package>
   <group> : its own RESULT` block a group.
6. Open the four checks workbooks in the package's folder of the Report folder.
   Look for: one Missing discipline line a discipline, and a group that did not run said `not read`, never `missing`.

### F223, Auto-Save off while the tool works and put back as it was

1. In Navisworks, open Options, General, Auto-Save, and write down whether Enable Auto-Save is ticked. Close Options.
2. Open the tool, and on the NWC page, step 5, run one community of copies with Run ticked packages, as in F222's steps 2 and
   3, clicking nothing more.
   Look for: each group's Navisworks opening and closing as in F222, and no autosave file of the copies in
   %APPDATA%\Autodesk\Navisworks Manage 2025\AutoSave dated during the run.
3. When the live line says the run finished, open the log with Open log folder.
   Look for: `AUTOSAVE the registry's switch reads '0', never set, Navisworks' own default, kept in
   auto-save-to-put-back.txt` near the start, one `AUTOSAVE the registry's switch read '0', switched off for this group's
   Navisworks` or `off already` line a group, and near the end `AUTOSAVE the registry's switch read '3 0', put back to '0', it
   reads '0', auto-save-to-put-back.txt deleted`.
4. Open the folder %LOCALAPPDATA%\ParsonsNwcFederator.
   Look for: no file named auto-save-to-put-back.txt.
5. Open one group's own log, the one its GROUP line names.
   Look for: `AUTOSAVE the option general.autosave.enable reads off as the run starts, left as it is`.
6. On the NWF page pick one NWF of the run with File and press Run on the NWF page, in the Navisworks you started.
   Look for: `AUTOSAVE the option general.autosave.enable reads on as the run starts, switched off for the run` where step 1
   read ticked, and before RESULT `AUTOSAVE the option ... put back to on as the run ends`, then `... as it was before the run,
   ... auto-save-to-put-back.txt deleted`.
7. Close Navisworks, start it again from the Start menu and open Options, General, Auto-Save.
   Look for: Enable Auto-Save as you wrote it down in step 1.

### F224, only tests across two set codes run

1. On the NWC page, step 5, run one community of copies with Run ticked packages, into a new empty Report folder.
   Look for: the confirm as before, and every group ending as it would have.
2. Open one group's own log of a group of two disciplines or more, the one its GROUP line names.
   Look for: `CLASH    the rule of two set codes left out 284 of the 1830 tests planned, each setting a set code against itself,
   AR 120, DR 10, EL 78, FF 15, ME 36, PL 10, ST 15`, and how many of the tests that run have a side whose set name reads no
   set code.
3. Open the log of a group of one discipline, such as 1B06BS.
   Look for: `CLASH    the rule of two set codes is not read, the group holds one discipline`.
4. Open that group's clash workbook in its Log folder.
   Look for: no block of a test such as BLD-ST-Walls-vs-BLD-ST-Foundation, and BLD-PL-Pipes & Pipe Fittings-vs-BLD-DR-... blocks
   still there where they found clashes.
5. In Navisworks, open that group's NWF and Clash Detective.
   Look for: no test whose two sides carry one set code, and its clash counts equal to the workbook's.
6. On the NWF page, pick an NWF refreshed before this fix and press Run on the NWF page.
   Look for: the confirm saying a test whose two sides carry one set code is not run and one saved in the NWF keeps its earlier
   results there, and in Clash Detective those tests still there with their results.

### F225, the tests a rule left out that an NWF still holds

1. On the NWF page, pick a package whose NWFs were refreshed before F224 and press Run ticked packages.
   Look for: the run ending as before, and no COVERAGE FAILED line naming a test such as BLD-ST-Walls-vs-BLD-ST-Foundation.
2. Open one group's own log, the one its GROUP line names.
   Look for: `not run by the rule : 284 tests left out by the rule of one discipline or of two set codes, N still held in Clash
   Detective from an earlier run with M clashes`, then how many are held by neither side and `0 whose holding in Clash
   Detective is UNKNOWN`.
3. Open that group's NWF in Navisworks and Clash Detective.
   Look for: those N tests still there with their clashes, since the tool deletes no test.
4. Answer Q212 in steps\02_questions.md, or in the tab: leave them, delete them once, or move them into a folder of their own.

### F226, a picture that draws no item of its clash taken again

1. On the NWC page, step 5, run a community holding 1B06BC into a new empty Report folder, Run ticked packages, clicking nothing
   more.
   Look for: the run ending as before.
2. Open 1B06BC's own log, the one its GROUP line names.
   Look for: `IMAGES   RETAKE   N of the M pictures read drew no item of the clash, N taken again through the active view from
   9.144 m, 30 ft, at the nearest`, then how many drew their items and how many still none.
3. Open the Clash report folder's 1104-PAR-1B06BC-ZZZ-BM-MOD-000001_files folder and look at the pictures.
   Look for: no picture that shows only the sky and the grid label, and a few pictures with one item red and one green against
   other models' items in their own colours, the ones taken again.
4. Open the clash workbook in the Log folder and look at the Image column of a test such as
   BLD-FF-Pipes & Pipe Fittings-vs-BLD-ST-Walls.
   Look for: every Image cell linking a picture of that clash, none of only the sky.

### F227, the eight numbers are settings with today's defaults

1. Run any package on the NWC page as in F222's steps.
   Look for: the run reading as before, every group's IMAGES settings line holding `every test gets pictures for at most its
   first 150 clashes at priority A, 100 at priority B and 50 at priority C`.
2. Open one group's own log.
   Look for: the running count lines every 25 tests as before, `CLASH    ... 25 of`, and the EXPORT CHECK block listing at most
   10 names a group as before.

## The round of 18 Oct, F212 to F221

Simple words. Every fix of this round but two was seen on Navisworks by F220, F212 being a rule read off the code and F215 a measurement left UNKNOWN for Q208, started by the loop, on copies in C:\NWC-PC-Test\work\multi2, which stays on this PC. What no run of the loop can show is a Navisworks you start yourself giving each package its own Navisworks, so step 1 comes first. The whole report is claude\audit-round-2026-10-18.md. Use a new empty folder for Report, so nothing of F220 is written over.

1. Start Navisworks Manage 2025 from the Start menu, as you always do, and open the tool.
2. On the NWC page, step 5, type C:\NWC-PC-Test\work\multi2\Parent as the parent folder and a new empty folder as Report, and press Read.
   Look for: the tree showing C06 with 64 NWC files, C07 with 70 and Corrupt with 1.
3. Tick C06 and Corrupt, press Run ticked packages and OK on the one confirm, and click nothing more.
   Look for: a second Navisworks opening for C06, working and closing, then one for Corrupt, and no message box waiting for you. F214 and F213.
4. When the live line says the run finished, open the log with Open log folder.
   Look for: `group          Corrupt  1B06KI  FAILED` and its why line naming the file and saying it could not be appended, C06 with 21 groups, 21 done, and under PACKAGE TREE each package line ending with its full path, such as `C06  64 NWC files, at C:\NWC-PC-Test\work\multi2\Parent\C06`. F213 and F216.
5. Open C06's own log, the one the line PACKAGE C06 of that RESULT names.
   Look for: `CLASH    C06  1B06BS holds one discipline, EL, so no test of it is run, Bader's answer to Q203`. F219.
6. Open C06 EL checks.xlsx in the C06 folder of your Report folder and filter the Check column on Coordinates.
   Look for: 1B06PO far, 49.055 m from AR, which the old limit of 1000 m read near. F218.
7. On the NWF page, type the same parent and Report folders, press Read, tick C06 and press Run ticked packages.
   Look for: the confirm's Pictures line counting the old jpg files of step 3, about 3605, and not UNKNOWN. F217.
8. Press OK, wait for the run to finish, open its log and then C06's own log, the one its line PACKAGE C06 names.
   Look for: `CLASH    C06  1104-PAR-1B06BS-ZZZ-BM-MOD-000001 holds one discipline, EL, so no test of it is run, Bader's answer to Q203`, the same groups skipped as on the NWC page. F219.
9. Answer Q207 to Q210 in steps\02_questions.md, or in the tab.

## F214, a fresh Navisworks for every package

Simple words. When you run ticked packages, your window plans them and asks once, as before. Then each package runs in a new Navisworks of its own, one after the other, and the next starts only once the last has closed, so a long run never fills one Navisworks's memory. One RESULT in your window's log joins them all. PROVEN on copies with Navisworks started by the loop. Not yet proven with a Navisworks you started yourself, which is step 1 and comes first because nothing else can show it.

1. Start Navisworks Manage 2025 from the Start menu, as you always do, and open the tool.
2. On the NWC page, step 5, pick a parent folder of copies and an empty Report folder, press Read, and tick two packages.
3. Press Run ticked packages and OK on the one confirm.
   Look for: the confirm saying each package runs in a fresh Navisworks of its own, so nothing open in this one is touched.
4. Wait without clicking anything.
   Look for: a second Navisworks window opening, working and closing, then another for the next package, and your own window still moving its live line.
5. When the live line says Run finished, open the log with Open log folder.
   Look for: RESULT starting with fresh starts : 2 packages, each in its own Navisworks, then the groups and one PACKAGE line for each with its own log.
6. Open the Log folder under one package's Report folder.
   Look for: that package's own run log, the one its PACKAGE line names.
7. If any package says NO RESULT, send me both logs.

## F210, the test on C06 and C07, 18 Oct

Simple words. PROVEN means a run on Navisworks showed it and I read the result. Every run was on copies under C:\NWC-PC-Test\work\multi, with main 14b434f4 installed. NM fed was only read, listed before and after with every file's SHA-256, 135 files both times and 0 lines differ. The whole report is claude\audit-multi-2026-10-17.md.

- Both features PROVEN on both layouts and both pages, five runs of 42, 42, 11, 31 and 42 groups, every group done, none partial, none failed, and a sixth run where a package of one corrupt NWC failed and the package after it still ran, once I answered the message box Navisworks opened about the corrupt file.
- The tree levels, Run refused with nothing ticked, one package ticked alone running alone, the dummy community left unticked never run and no folder made for it, the report folders mirroring the path from the parent, community and package on every group line with totals per package, per community and for the run.
- One Excel per discipline per package in the right folder, 5376 rows each in its own package's workbook, every check firing on real data, 7 ids found again in Navisworks by their Element Id, each as its line says.
- The chat audit of C02 read on C06 and C07: still there 4, 5, 7 and 8, still there by your rule or by default 2 and 3, cannot tell 1, 6, 9 and 11, and 10 its cause gone but 89 pictures empty for another reason. Which seven the message means is UNKNOWN, the file it names is nowhere.
- Found, not fixed: the memory stop leaves the later packages of a long run without pictures, Q204. A group of one discipline runs no test on the NWC page and finds clashes on the NWF page, Q203. 89 pictures of 1B06BC and 1B06P1 show only the sky, cause UNKNOWN. The log names each package by its path from the parent, not its full path. A corrupt NWC stops an unattended run at a message box. The far limit of the Coordinates check, Q202.

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
