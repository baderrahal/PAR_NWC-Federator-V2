# Set 03, the first run of main, C06, 2026-10-01

The first run of main on this PC. Main e4484d15 installed, set 03's fresh copy, community C06,
22 groups, the copy's clash XML with 1830 tests at 25 mm, every group ticked, through the real
window with no click. run.ps1 from 13:59:01 to about 15:53, VERDICT RAN. The tool's run took
1 h 44 min 52 s. Every line cited is in steps\runs\03\item1-C06: "log" is
run-20261001-140037.log, "tsv" its .tsv, "read-out" the file in workbooks\ for that group.

## Bader's three tests of done

1. GROUPS: 17 DONE, 0 PARTIAL, 5 FAILED of 22. The five FAILED are 1B06BC, 1B06G1, 1B06M1,
   1B06PS and 1C06M2, each because a model was exported on Revit's internal origin and not on
   a shared site, and each still wrote its NWF, NWD, workbook and report, log:967, 1762, 2774,
   5802 and 7899
2. TIME: 1 h 44 min 52 s against 45 min, over by 59 min 52 s, log:8426. The three slowest
   steps in the TIMING blocks are all VIEWS: 1B06G1 1769 s, log:1767, 1B06PK 1421 s, log:4584,
   and 1B06PP 694 s, log:5447. VIEWS is 72 percent of the whole run, log:8408. Without it the
   run would have taken 29 min 29 s
3. THREE TESTS FOR THE CLASH DETECTIVE PANEL. Open the NWF from
   %LOCALAPPDATA%\NwcFederatorLoop\runs\03\NMFed\NWF\C06, open Clash Detective, click the test,
   read its clash count and status counts, and do not press Run Test:
   - 1104-PAR-100000-ZZZ-BM-MOD-000001.nwf, BLD-AR-Curtain Mullions-vs-BLD-AR-Curtain Panels:
     the workbook says 4, all New, read-out line 7, log:342
   - 1104-PAR-1B06PH-ZZZ-BM-MOD-000001.nwf, BLD-ST-Framing-vs-BLD-ST-Columns: the workbook says
     11, all New, read-out line 9, log:4065
   - 1104-PAR-1B06PK-ZZZ-BM-MOD-000001.nwf, BLD-ST-Floors-vs-BLD-ST-Columns: the workbook says
     636, all New, read-out line 7, log:4423. This NWF is 11 MB and opens slowly

## Findings, worst first

Silent wrong numbers:

1. A model hundreds of kilometres from the rest of its group still ends DONE, so its clashes
   with the other disciplines are never found and the count reads as whole. 1B06K1 ST, 1B06P1
   ST and 1B06PH EL, ALIGNMENT. log:1834 "DIFFERENT dx -658144882.33 mm" then log:2128
   "1B06K1 DONE", and log:8358 "40 model(s) sit somewhere their group's reference model does
   not". Only a model on the internal origin fails a group
2. Duct, pipe and equipment sets find nothing because of letter case, so their clash tests are
   never made and the groups still read DONE. The XML asks ME-DUCTWORK, ME-EQUIPMENT and
   ME-PIPING and the models say ME-Ductwork, ME-Equipment and ME-Piping. 1B06PK, 100000,
   1B06K1, 1B06M1, 1B06P1, 1B06WL and 1C06M2, SETS. log:4293 "worksets seen: ME-Ductwork" and
   log:4327 "BLD-ME-Ducts & Duct Fittings ... 0 items ... equals ME-DUCTWORK"
3. The WORKBOOK CHECK says every workbook is short of the matrix, counting only the blocks that
   found clashes, while each workbook holds all 1830 tests. Every group, WORKBOOK CHECK.
   log:455 "BLOCKS 5 in the workbook against 1830 tests" against read-out line 1855 "tests 1830"
4. RESULT prints file sizes that are not the files': the .tsv as 0 bytes, really 1,499,250,
   and the .log as 678,363 bytes, really 1,097,850. RESULT. log:8582 and 8583 against log:8584
5. Each group's last CLASH progress line is one test short, and in 1B06PK, 1B06M1 and 1C06M2
   its clash total too. CLASH. log:4411 "1624 clashes so far" against log:4455 "clashes found
   1629". The block under it is right and agrees with the workbook

Slow:

6. The run took 1 h 45 min against 45 min. log:8426
7. VIEWS is 72 percent of the run, up to 1.5 s a viewpoint in the big groups against 0.03 s in
   the small ones, 1165 viewpoints in 1B06G1 alone. log:8408, log:1669
8. The log is silent for up to 21 minutes inside VIEWS while Navisworks works, which only the
   processor time kept from reading as a hang. 1B06PK, log:4476 at 14:56:15 then log:4477 at
   15:17:24
9. HARVEST is 12 percent of the run, nearly all of it pictures, 643 s for 5679 of them. log:8409
   and 8424

Loud failures:

10. Five groups FAILED for a model on Revit's internal origin, the alignment rule doing what it
    says. 1B06BC AR, 1B06G1 AR, 1B06M1 ST, 1B06PS AR and ST, 1C06M2 ST. log:967, 1762, 2774,
    5802 and 7899. Those models need exporting again on the shared site

Broken features:

11. No EMPTY SETS block in any group, though every group had sets that found nothing. SETS.
    log:288 "61 created ... 9 finding items, 52 at zero", and no EMPTY SETS line anywhere
12. Group 100000's HTML report has no Grid Location column, and the REPORT CHECK says so. The
    other 21 have it. log:474

Noise:

13. The one discipline sentence says every clash test is still created, while the CLASH block
    of the same group says 36 of 1830. log:112 against log:1125
14. Each group's first CLASH line says all 1830 tests are to be made, the next says far fewer
    were. log:289 against log:297
15. The four parts VIEWS reports for its own seconds do not add up to the step. log:5343 against
    log:5349
16. Grid Location is empty on 345 of the 5679 clash rows, 13 of 13 in 100000, 40 of 42 in
    1B06K1 and 292 of 695 in 1B06PP. read-out of 100000 line 1839
17. RESULT says 88 files written, while the run wrote 5790, the 5679 pictures and 23 more not
    counted. log:8466 against outputs.txt
18. The tool pruned the oldest of Bader's logs, run-20260901-191711.log, as Q82 allows, and
    logs-backup holds it. log:3, record.txt:1037

## The confirmed bugs this run showed

Of the 86 read again on 2026-09-29, 76 CONFIRMED and 10 PARTLY, steps\notes\turn1-read-verified.md,
this run showed 4 and did not reach the other 82, most of them on paths a first run does not take:

- T1-S32, a number in the tsv rounded where the text says it keeps its precision, tsv:78
- T1-S46, the one discipline sentence that says every test is created, finding 13
- T1-S50, a model with one element on a workset passing as carrying worksets, log:205 and 208
- T1-S63, only sets already present judged, so no EMPTY SETS block, finding 11

## The register rows this run settled

- 49 PROVED WORKING, among them RUN-1637, the run on main writes RESULT, the workbook and the NWD
  for every group, F26 units in metres, F27 the GROUPS block, F33, F34, F35 one discipline
  groups, F52 the viewpoints, F59 and F60 the steps and the TIMING blocks, F61 the census, F63
  the GAP block, F64 the tsv, F73 the appended viewpoints, F78, F80, F82 and F91, and 28 Look
  for lines of steps\03_bader_next.md
- 8 CONTRADICTED: F81 and step 377, the RESULT sizes, finding 4. Steps 9 to 17, which say C06
  never existed, while C06 has 22 groups here. The EMPTY SETS row and Look for 62, finding 11.
  Steps 92 and 94, the one discipline sentence, finding 13. Step 269c, the workbook check,
  finding 3. Look for 387, the time
- 25 shown by the findings above, and 136 not reached by a first run

Every row with its line is in the reader's return, kept in
%LOCALAPPDATA%\NwcFederatorLoop\turn4\c06-register.json, and the confirmed bugs in
turn4\c06-rows-a.json.
