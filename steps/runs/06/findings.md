# Set 06, main 4bc4c83, 1A02MM and 1A04PK with the clash run, 2026-10-11

Main 4bc4c83 installed in place at 13:05 and read back at 14:09. Set 06's copy made fresh at 14:10 by
prepare-copy.ps1, 156 files, 385.5 MB, the desktop folder having changed since set 05. Every group ticked, the
viewpoints box ticked as F114 left it and Skip clash for models off shared coordinates unticked through
run.ps1 -Untick, F126, so the clash runs as in set 04, which ran before the rule. Through the real window with
no click. A window run cannot pick a priority file, so the views tree has no priority layer, as in set 04.

- 1A02MM, item1-C02, the copy's own clash XML, the one set 04 picked, no corrections or teams file beside it.
  "C02" is item1-C02\run-20261011-141243.log
- 1A04PK, item1-C04, the repo's corrected matrix exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml with its teams
  and corrections files beside it, copied under runs\06\NMFed\exchange and read back by sha256,
  set06-exchange-copy.txt. "C04" is item1-C04\run-20261011-142641.log

## 1A04PK

1. GROUP: never finished. At 19:31:36 Windows logged Application Hang 1002, Roamer.exe "stopped interacting
   with Windows and was closed", which it writes when a window reading Not Responding is closed by a person
   from its prompt or from Task Manager, who is UNKNOWN. run.ps1 read the process gone at 19:31:35, VERDICT
   STOPPED, the adopted Navisworks ended by itself, and put every setting back, record.txt
2. TIME: 5 h 6 min to its close, 18,294.85 s inside the plugin call, record.txt. It reached test 75 of 741 with
   3272 clashes, C04:537, against 400 of 561 tests in 8.7 h in set 04, which hung
3. CLASH DETECTIVE: no count to compare, no group finished

Where the time went, read off the .tsv by the lead at 16:45 and in the diagnosis at 16:08,
runs\06\item1-C04\diag-1625\verdict.txt:
- Each clash call took 0.1 to 0.3 s. The 45 tests read to their rows by 16:45 took 8,084 s from created to rows,
  all of it after the call: BLD-AR-Walls-vs-BLD-AR-Floors 5,817 s for 1556 clashes, BLD-AR-Ramps-vs-BLD-AR-Floors
  1,009 s for 97, BLD-AR-Doors-vs-BLD-AR-Walls 675 s for 300, 1.4 to 10.5 s a clash
- Busy and not stuck: 163 s of processor in 120 s, 15 s of it the window's thread, 6.1 GB private against 34.8
  GB free, no dialog, every window hung. F143 gives the harvest pieces, a pump and PROGRESS lines

What 1A04PK proves:
- F131 the team map: PROVEN read, C04:70, Architecture AR, Structure ST, Mechanical HV PL FP ME DR FF
- F116 the corrections: PROVEN read and applied, C04:79 to 91, nothing changed since the matrix is corrected
  already, 6 made and 4 with nothing to change
- F112 and F137 with the box unticked: PROVEN, 9 models off the coordinates and the clash run, C04:187
- F115 the sets: PROVEN, 61 created, C04:309
- F128 Generic Models: PROVEN, 534 items in 3 of 10 models, C04:325 to 333. Set 05 read 572 in 5: both ME models
  changed between the two copies, ME-000001 489,050 to 490,559 bytes and ME-000003 5,617,535 to 4,501,778 bytes by
  the two manifests, and read nought now
- F132 the mirror rule: 59 pairs named, C04:347, the merge NOT REACHED
- The views, the coverage, the workbooks, the pictures and RESULT: NOT REACHED

## 1A02MM

1. GROUP: FAILED, C02:1983, for two reasons. Its ST model is on Revit's internal origin, which fails a group by
   the coordinates rule with the box unticked as with it ticked, as in set 04. And 61 views could not be put
   into the NWF
2. TIME: 4 min 59 s, 298.601 s, C02:2090, against 2 h 12 min 6 s, 7925.970 s, in set 04. Not a like
   comparison: VIEWS took 3.293 s here, C02:1527, with no view written, against 7487.104 s for 2813 views
   there. HARVEST 198.339 s against 218.845 s, IMAGES 178.478 s against 196.851 s, TESTS RUN 15.138 s against
   118.480 s, SETS 1.035 s against 0.912 s. RENUMBER 40.892 s, COVERAGE 1.157 s and INTERNAL 0.338 s have no
   baseline
3. CLASH DETECTIVE: 2931 clashes over 528 tests run, C02:915, against 2939 in set 04. Every test whose count
   moved touches ST, and the ST file is 52,887 bytes here against 51,608 there, so the file changed. The
   COVERAGE step compared the workbook against Clash Detective's own counts for every test that ran, 528
   compared, 528 agree, 0 FAILED, C02:1944

What 1A02MM proves:
- F127 the coverage: PROVEN. Reasons add to 1830, C02:1928 to 1931, 528 of 528 agree, RESULT carries the
  tally, the Coverage sheet in the internal workbook and none in the clash workbook, C02:1953
- F118 the workbook: PROVEN. The clash workbook one sheet, 1830 test blocks and 2931 clash rows, C02:1890
- F120 the renumber: PROVEN. 2931 renamed, C02:908, the RENUMBER row in TIMING
- F128 Generic Models: PROVEN on a building with none, 0 in 4 models, C02:247 and RESULT C02:2143
- F112 and F137 with the box unticked: PROVEN. The rule is off and says so, C02:134, the clash runs
- F115 the sets: PROVEN. 61 created, C02:252
- F119 the run log and RESULT: PROVEN. RESULT 0 done, 0 partial, 1 failed, matching the group
- The pictures: 2931 written, C02:911
- F114 the views: FAILED. 0 created, 61 failed, C02:1660. Fixed by F141, merged as PRs 196 and 197
- F132 the mirror merge: NOT REACHED. 59 pairs named, C02:263, and none merged, since each pair's mirror set
  finds nothing in this model
- F116 the corrections and F131's map: NOT REACHED, no file beside the XML. 1A04PK carries them

## Findings, worst first

Loud failures:

1. No view is written. 47 views of two clashes or more threw ObjectDisposedException at the framed copy's
   Position, C02:916 on, and the 14 views of one clash failed as added without its dimming, C02:1549 on.
   F141 for the 47, merged. Row F141-R1 for the 14

Silent wrong numbers:

2. The VIEWS TREE after line reads removed 0 at once, C02:1745, where the inventory removed the 14 at once
   with their folder in one RemoveAt, C02:1504 to 1520. Row T6-R-TREEREMOVED

Noise:

3. TESTS RUN took 15.138 s against 118.480 s in set 04 for nearly the same tests. Why is UNKNOWN. No row, since
   it is faster and nothing reads wrong
