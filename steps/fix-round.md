# Summary for Bader

The closed items of this file moved to steps\history\fix-round-closed.md on 19 Oct 2026, whole and in their order, F242, Bader's round of 19 Oct b.

Written when the round ends.

# Fix round, turn 5

Status lives in steps\tracker.csv, one row per FR item, and reads in steps\tracker.md. This file
keeps each item's narrative.

The fix list of Bader's full fix round, Q98, written on 2026-10-04 before the first fix, from
the returns of five readers: the findings of set 03, the confirmed bugs of turn 1 in two halves,
the register in steps\loop.md, and the readings of the fixes in flight. Their 196 raw items
come to 174 here. 22 were the same fault as another item and were folded into it, with every
source id kept on the item that stayed.

How it is ordered. By area: alignment, sets, clash counts, workbook, report, run log and
RESULT, views, harvest and pictures, install, loop tools, docs and words, rest. Inside an area
by class: silent wrong number, broken feature, slow, loud failure, noise. FR-001 is B2 of Q98,
the far model rule.

What each item holds. The heading gives its number and the key the readers used. Then the
line of what is wrong, its Sources, the Evidence, the Root cause, its Class and the Proof that
shows it fixed. Needs Bader was written only where a choice of his came first, 12 items, all
answered on 2026-10-04, each answer on its item.
Branch is written where the item rests on code merged from a branch, waits for a merge not yet
made, or cites lines that moved when a branch merged. Note carries what the readers added and
where the item meets others.

What main holds now, read with git on 2026-10-04.
- F108 merged into main as c9b223b (PR 84) and F106 as 3449521 (PR 88), both on 2026-10-04.
  F104, F105, F107 and F109 were not merged at 3449521. Since then PR 89, the records with the
  note for the modellers, merged as 51a0cb6, PR 83, the rules, as 086a348, and PR 90, F105, as
  2c89788, all on 2026-10-04. FR-125, FR-137 and FR-138 are closed by them and FR-139 in part,
  each saying so.
- src, tests, build, exchange, docs, CLAUDE.md, INSTALL.md, .claude and
  steps\03_bader_next.md read the same at 3449521 as at f38edd5, so every line cited in them
  still reads where it is cited.
- F106 rewrote tools\loop\run.ps1 and changed tools\loop\nw-guard.ps1 and
  tools\probes\drive-window-run.ps1. A line a reader cited in those three at f38edd5 has
  moved, and the item gives the function and its line on main where it was looked up. The
  lines readers cited at fix-F106 32dce24 read the same on main.
- F108 changed tools\loop\prepare-copy.ps1 and tools\loop\README.md. The lines cited at
  fix-F108 read the same on main.
- "rests on F106, merged as 3449521, lines unchanged" means the item is about F106's code and
  its cited lines read the same on main. The same words with F108 and c9b223b mean the same
  for F108.
- A profile folder name is written <profile> or <name> in this draft, because whether it is a
  trace to keep out of committed files is FR-109 and FR-149, which are Bader's.

Key to the source ids.
- S03-n is finding n of steps\runs\03\findings.md. C06-Jn is the nth finding in array order of
  turn4\c06-find.json, 30 in all. C06-DONE-n is rank n of the findings in turn4\c06-done.json,
  21 in all. FIND-nn and the F, A, B, L, P, step and LOOK ids are rows of
  turn4\c06-register.json, 218 rows, or of the register in steps\loop.md, its line given where
  a reader gave one. Register row B2 is not Q98 B2.
- T1-S, T1-B and T1-L ids are the confirmed bugs of the turn 1 read (turn4\c06-rows-a.json and
  steps\loop-read.md). T1-N, T1-UNCALLED and T1-CATCH are the three lists in steps\loop-read.md.
  T3-G, T3-P, T3-W and T3-B are register rows from the readings of F103.
- Q98 B1 to B5 are Bader's decisions at steps\02_questions.md:504.
- F106-HARM-REV-n and F106-HARM-BRK-n are the nth entries of the lists of the F106 harm
  reviewer ab4bf4ea39f7d1669 and breaker a2c8cbb853c41abcb, journal wf_8100973e-871.
  F108-REV-n and F108-BRK-n are the nth findings of a7c3aa7fb3982742a and a0c1e9264e4a0aaed,
  F105-REV-n and F105-BRK-n of aa1682651519f441d and a233e87838c44af6f, F107-REV-n and
  F107-BRK-n of a2eb40a6e1ac0f019 and aa9d7aa0e66b15452, in the order of their result lines.
  R1 to R4 of F106 are the finishing developer's, turn4\f106-finish-report.md.
- F108-R1 to F108-R5, F107-R1 and F107-R2 are the register rows PR 83 adds,
  turn5\pr83-diff-rest.txt lines 111 to 117. F103-W is the register row of the nine word
  entries, F103-W-1 to F103-W-9 are the entries of option A in
  steps\notes\f103-final-reading.md, and F103-W-wrap the three over long README lines it names.
- turn4 and turn5 are folders under %LOCALAPPDATA%\NwcFederatorLoop. log:N is line N of
  steps\runs\03\item1-C06\run-20261001-140037.log, and tsv:N, record.txt, driver.txt,
  watch.txt, outputs.txt and workbooks\ are files of the same folder.

## How the round works it

One pull request per area, each finding its own commit with the test that fails before it and
passes after, read by a reviewer and a breaker, its body by the claim-checker, and merged one at
a time once Actions is green, Q98. Up to three developers at once, on different files. An area
takes the F number The waves give it, and its section and DONE line in
steps\01_next.md say which FR items it closed. The lanes and the order are at the end of this
file, under Areas that can be worked at the same time.

- the product lane: alignment first with FR-001, then sets, clash counts, workbook, report, run
  log and RESULT, views, harvest and pictures and rest, as their files allow
- the loop tools lane: install with F109, then the loop tools, FR-092 first
- the words lane: the items that touch no code

Q93 holds in the loop tools lane: a reading of the loop's own scripts blocks only for a fault
that could harm Bader's things or make a run's evidence wrong, and the rest of that lane's items
are register rows, fixed where their file next changes. The product code keeps every house rule.

The 12 items that need Bader wait for the form, Q99 to Q109 in steps\02_questions.md, with Q24
and Q26, asked before. He answered them all on 2026-10-04 at 15:23, each answer on its item.
FR-001 is built with the 1 m he gave and its number is
Q99. Nothing of an item that needed him changed before he answered.

## The waves, Bader's order of 2026-10-04 at 15:42

Bader does not wait for all 174 items before a run. The fixes go in waves, each tested at once
on two buildings, 1A02MM of C02 and 1A04PK of C04, a first run with the XML and a weekly run,
against the baseline of set 04 and the wave before. The waves follow his wished order by area,
adjusted by the file table at the end of this list, so the silent wrong numbers sit in waves 1
to 4 with the areas that hold them. Inside that, every noise item goes to wave 5 with the docs
and words and D1, bar three kept early and named below with why. The lead departed from his
wished order in two places: F115 sets, 16 items his list did not name, in wave 2, and workbook
and report one area because they share WorkbookCheck.cs. FR-136, Q108's fix, and F117, the names,
moved from the waves 3 and 4 the lead first told him to wave 5, since both are noise by the class
this list gives them and noise comes last in his order. Five pairs in one wave or part
share a file or a rule and merge one after the other: in wave 1 F112 and F116 both
change ExportCheck.cs, F112 for FR-003 to FR-005 and F116 for the model worksets rows of
FR-008, F116 after F112, in 2a F127 and F132 which tests are created and run, F127 first,
in 2b F131 before F114, which reads its team pairs, in 3b F130 and F119
FederatorWindow.xaml.cs, and in wave 5 F123 and F117 docs\history\scan.md. F128 and F118 share the
workbook writer, so F118 sits in 2c after F128. The four finds of Q108, Q109, Q24
and Q26 write their test steps under their items and run them at the start of the wave that
holds them. Each area takes the F number shown.

- Wave 1:
  - F112 alignment: FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-028. Noise kept here: FR-028, started by F112 before the waves
  - F113 clash counts: FR-031, FR-032, FR-033, FR-034
  - F116 the clash XML: FR-008, FR-009, FR-025, FR-026, FR-030. Noise kept here: FR-030, Bader put the XML corrections of Q102 to Q104 in wave 1
- Wave 2, in three parts since Bader's views by team, Q114, each part at most three areas:
  - 2a, F127 coverage, Bader's request 2: FR-176, FR-200. First of 2a until Bader's order of 2026-10-05, Q132, put F132 before it
  - 2a, F132 mirrored tests, Q114: FR-182, FR-183. F127 and F132 both change which tests are created and run, so they merge one after the other, F132 first since Q132
  - 2a, F115 sets: FR-010, FR-011, FR-012, FR-013, FR-014, FR-015, FR-016, FR-017, FR-018, FR-019, FR-020, FR-021, FR-022, FR-023, FR-024, FR-027
  - 2b, F131 teams, Q114: FR-180, FR-181. It merges before F114, which reads the team pairs
  - 2b, F114 views: FR-065, FR-066, FR-067, FR-068, FR-069, FR-070, FR-071, FR-072, FR-073, and Q114's FR-184 to FR-188. Noise kept here: FR-073, committed by F114 before it paused
  - 2b, F128 generic models, Bader's request 3: FR-177
  - 2c, F118 workbook and report: FR-035, FR-036, FR-037, FR-040, FR-041, FR-199. F128 and F118 both add to the workbook writer, so F118 comes after F128 has merged
- Wave 3, in two halves the same way:
  - 3a, F129 start from an existing NWF, Bader's request 4: FR-178
  - 3a, F120 harvest and pictures: FR-075, FR-076, FR-077, FR-198
  - 3a, F109 install: FR-078, FR-079, FR-080
  - 3b, F130 the Shift range in the group list, Bader's request 5: FR-179. In the half after F129 because both change the window's files
  - 3b, F119 run log and RESULT: FR-043, FR-044, FR-045, FR-046, FR-047, FR-048, FR-049, FR-050, FR-051, FR-052, FR-053, FR-054, FR-055, FR-056, FR-057, and FR-189 found by F112's closing pass
- Wave 4:
  - F121 the rest: FR-150, FR-151, FR-152, FR-153, FR-154, FR-155, FR-156, FR-157, FR-158, FR-159, FR-160, FR-161, FR-162, FR-163, FR-164, FR-165, FR-166, FR-202. The find of Q24 on FR-160 and Q109 on FR-161 first
  - F122 the loop tools: FR-082, FR-083, FR-084, FR-085, FR-086, FR-087, FR-088, FR-089, FR-090, FR-091, FR-092, FR-093, FR-094, FR-095, FR-096, FR-097, FR-098, FR-099, FR-100, FR-101, FR-102, FR-103, FR-104
- Wave 5:
  - F123 docs and words, and the noise of every area: FR-007, FR-029, FR-038, FR-039, FR-042, FR-058, FR-059, FR-060, FR-061, FR-062, FR-063, FR-064, FR-074, FR-081, FR-105, FR-106, FR-107, FR-108, FR-111, FR-112, FR-113, FR-114, FR-115, FR-116, FR-117, FR-118, FR-119, FR-120, FR-121, FR-122, FR-123, FR-124, FR-126, FR-127, FR-128, FR-129, FR-130, FR-131, FR-132, FR-133, FR-134, FR-135, FR-136, FR-139, FR-140, FR-141, FR-142, FR-143, FR-144, FR-145, FR-146, FR-147, FR-148, FR-167, FR-168, FR-169, FR-170, FR-171, FR-172, FR-173, FR-203, FR-204. Closed in part, the part that stays: FR-139, FR-146. The finds of Q108 on FR-136 and Q26 on FR-172 first. Closed already: FR-125, FR-137, FR-138
  - F117 the names, by Bader's answer to Q105 to Q107: FR-109, FR-110, FR-149
  - then F124 D1, one public type per file: FR-174, moves only, last
- Before any test run, Bader's stop of 2026-10-05, Q130: F136 the viewpoints switch, FR-194. Until F114
  merges every test run has viewpoints switched off
- First of all since Bader's order of 2026-10-05, Q132: F136, then F131, then F132, then F114, each merged
  before the next starts where they share a file, then 1A02MM and 1A04PK with the new views on
- Before the test of wave 1, Bader's answers to Q111, Q125 and Q78: F137 no site and no clash groups, FR-195, F137 a shared site named DEFAULT, FR-201
- Before any probe or run starts again, Bader's message of 2026-10-05, Q135: F138 loop starts with Auto-Save off, FR-196
- Beside the waves, Bader's lane of 2026-10-05, Q129, worked by a fourth worker in its own worktree and
  never on a file a fix lane is changing at the time: F133 the work tracker, FR-191, F134 the code
  health gate, FR-192, and F135 the analyser settings that touch every project, FR-193, merged alone at
  a quiet moment. The gate's lists are the work list of waves 4 and 5
- Right after F133 merges, beside the viewpoints, Bader's message of 2026-10-06, Q139: F139 the one page progress file, FR-197

Bader's five requests of the evening of 2026-10-04, Q112, are added to the round as FR-175 to
FR-179, under their own heading below, each measured before it is written, test first, and
tested on 1A02MM and 1A04PK like the fixes. FR-175, no sleep, was done by the lead at once. Two
tools of the loop sit outside the product waves: F125, the window driver leaving a floating pane
alone, and F126, the driver unticking a named box, which the run of a building off its shared
coordinates with the rule switched off needs in the test of wave 1.

## Alignment, FR-001 to FR-007

## Sets, FR-008 to FR-030

### FR-029 set-builder-handles-never-disposed

SidesBySetName and WalkForLeftovers hold wrappers they never dispose, so they are left to the
finalizer.

- Sources: NEW-QA SetBuilder.cs:271 (steps\loop.md:555)
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:273, the ClashTest from tests.Tests[t] has
  no using, :280-281 SelectionA and SelectionB, :302 side.Selection, :309 sources[0].
  WalkForLeftovers at :224-226 reads set.Search twice and its SearchConditions, none disposed.
  .claude\rules\addin.md says what this tool creates or resolves it disposes.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:273, :280-281, :302, :309 and :224-226.
- Class: noise
- Proof: No Core test reaches it. Run line: a run with the rebuild box on in set 05 logs no
  ObjectDisposedException and the handle count at the end does not climb.
- Note: Q28, answered by Bader on 2026-10-06 to measure first and leave both reads until then,
  FR-202, asks the same for sub objects read off a handle this tool
  made, such as side.Selection and set.Search, because their ownership is not measured. Items
  read out of a document collection (the ClashTest, the SavedItem) are covered by the measured
  rule. Only runs with the rebuild box on reach this code. The same walk is FR-014, so the two
  go together.

## Clash counts, FR-031 to FR-034

## Workbook, FR-035 to FR-039

## Report, FR-040 to FR-042

### FR-041 grid-location-empty-on-345-rows

The Grid Location cell is empty on 345 of 5679 clash rows (13 of 13 in 100000, 40 of 42 in
1B06K1, 292 of 695 in 1B06PP), and in 100000, where all 13 are empty, the HTML report loses its
Grid Location column and the REPORT CHECK says so.

- Sources: S03-13, S03-17, C06-J14, C06-J20, FIND-06, C06-DONE-14
- Evidence: workbooks\Clash_Report_C06_1104-PAR-100000-ZZZ-BM-MOD-000001.xlsx.txt:1839-1851, all
  13 Grid Location cells empty, and the 1B06K1 read-out at :1872 and :1873 'D-1 : RO1' on 2 of
  42. log:474 'Column 5 of the clash table is wrong. Ours reads Description and the client's
  report reads Grid Location', while the other 21 REPORT CHECK blocks end 'Nothing wrong with
  it.' (log:935). log:453 and log:2075 'The Item 1 Layer cell is empty'. log:5410, 1B06PP reads
  clean with 292 rows empty.
- Root cause: UNKNOWN why the grid is not found. The cell is set at
  src\Federator.Addin\Engine\ClashHarvest.cs:244-251 from grid.ClosestIntersection(centre),
  which gave nothing for these rows. In the 1B06K1 read-out the two filled cells are at z
  694.491 and the others at other heights, so height against the grid levels is the first thing
  to measure, and not a finding. The missing HTML column follows from
  src\Federator.Core\Report\ClashReportXml.cs:249-254, which leaves out an empty gridlocation
  element, and the stylesheet's showGridLocation reading //gridlocation
  (docs\history\scan.md:1364, tests\Federator.Core.Tests\Report\HtmlTabularTests.cs:172).
- Class: broken feature
- Proof: Run line of set 05: a new count line 'Grid Location empty on N of M rows' per group with
  N well under 345 of 5679, and the 100000 REPORT CHECK reads 'Nothing wrong with it.' (set 03
  log:474). No Core test can fail before until the cause is measured, by a probe printing
  ClosestIntersection for the 345 points. Once known, a Core test in
  tests\Federator.Core.Tests\Report\ClashReportXmlTests.cs pins what the writer does for a group
  with no grid.
- Note: core.md says write the data and let the stylesheet decide, so the column dropping when
  no row has a grid is Autodesk's own behaviour and the real fault is the missing data. The
  WORKBOOK and REPORT CHECKs read the first clash row only, so a count of empty grid cells per
  group is what would have named 1B06PP. ClashHarvest.cs is also changed by FR-032 and FR-076.

## Run log and RESULT, FR-043 to FR-064

## Views, FR-065 to FR-074

### FR-067 clear-rebuild-drops-viewpoints

The clear and rebuild fallback copies the sets and the tests out and back but not the saved
viewpoints, so a group that carries viewpoints can never succeed on that path and the NWF is
left as it was.

- Sources: Q77 (steps\loop.md:539)
- Evidence: steps\02_questions.md:418, Q77 'saved viewpoints LOST: before clear 52, after appends
  22, after restore 22' and REFUSED TO SAVE. src\Federator.Addin\Engine\FederationEngine.cs:1742-1743
  copy only testsCopy and setsCopy. :1805-1810, the comment says the viewpoints 'ride back inside
  the two copies above', which Q77 found false. Only the count is taken, at :1721 and :1811.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:1742-1743 and :1805-1812.
- Class: broken feature
- Proof: No Core test reaches it (Navisworks only, and RebuildTally already has tests). Proof run
  line: a forced clear and rebuild on a small copy, the log VIEWS tally reading 'saved
  viewpoints' before equal to after restore, and then the line that the NWF was saved over. Set
  05 reshapes and does not take the fallback, so it needs this one extra forced run.
- Note: Q77 had no answer written under it when this item was written, but the register class is
  open fault, and the rule
  that the NWF is the record already answers it. Whether DocumentSavedViewpoints.CreateCopy and
  CopyFrom work for this is measured first, as the row says, UNKNOWN until then. The viewpoint
  count before the clear goes through RebuildTally, whose minus one fault is FR-151.
- Answered by Bader on 2026-10-06, his words under Q77: yes, the clear and rebuild keeps the
  viewpoints, as this item plans, so a view a person made survives it, and CreateCopy and CopyFrom
  are measured first, as this item says

## Harvest and pictures, FR-075 to FR-077

## Install, FR-078 to FR-081

### FR-081 f103w-install-failure-claims-and-refused-text

Words say a failure after the move takes the new bundle out and puts the old one back, and that
a folder it cannot remove is named in the verdict of run.ps1 -Mode Install. Harness case ta2
shows the new bundle can stay at the load path with the old one still aside, and the record
names neither folder. The REFUSED text of install.ps1 says the rename was refused most likely by
a running Navisworks, right after the Roamer read found none.

- Sources: F103-W-5, F103-W-6, F103-W
- Evidence: build\install.ps1:113-114, tools\loop\README.md:162-164 at f38edd5 and
  .claude\rules\loop.md:70-72 against build\install.ps1:126-145 (PutOldBack leaves the old one
  aside when the new one can be neither removed nor moved) and prove-run.ps1:1055-1056 (case
  ta2). build\install.ps1:120 'most likely by a Navisworks that is running' after the read at
  :91-94. steps\loop.md:1089-1105 records the rename denied on 2026-10-01 with no Navisworks
  running.
- Root cause: build\install.ps1:113-114 and :120, tools\loop\README.md (:162-164 at f38edd5, moved
  by F108's change to the README) and .claude\rules\loop.md:70-72.
- Class: noise
- Proof: A read of the words after the change. The printed REFUSED line is checked by the
  prove-run.ps1 install trials (prove-run.ps1:1044-1065), which stay green. No Core test applies.
- Branch: taken with F109, which is about what holds the file, fix-F109 not merged at main
  3449521.
- Note: F103-W-6 changes a line that runs. What holds the file is UNKNOWN and is the subject of
  F109. The other F103-W entries are FR-140, FR-141 and FR-142.

## Loop tools, FR-082 to FR-124

### FR-082 clash-xml-copy-not-the-exchange-matrix

The first run read the desktop clash XML as it stood, asking ME-DUCTWORK in capitals, and not
the corrected matrix in exchange\, and nothing in the loop compared the two, so F87, F84 and Q68
were not tested and 14 sets found nothing in all 22 groups.

- Sources: FIND-04, S03-2, C06-J2, Q98 B1
- Evidence: log:146 'RUN the Clash step picked ...NMFed/1104-PAR_CLASH_AllInOne_25mm_FIXED.xml'
  names a path only, and driver.txt:12 typed it. FIND-04: line 28826 of the copy reads condition
  test equals where line 28826 of exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml reads
  contains, the copy has ME-DUCTWORK five times and flags 32 no times, and the exchange file
  ME-Ductwork five times and flags 32 six times. log:243 asks ME-DUCTWORK. log:8346 'found
  nothing in every group : 14'.
- Root cause: tools\loop\prepare-copy.ps1 copies the desktop folder as it stands and never reads
  the XML (:224-230 at f38edd5, the copy code moved with F108). On main 3449521 run.ps1 takes
  -Xml since F106 (RunPaths at :115-155, the file check at :365, handed to the driver at :1599),
  and a grep of run.ps1 and prepare-copy.ps1 on main finds no mention of the exchange file, so
  nothing compares the two. The data cause was the desktop file, which Bader renamed -OLD and
  replaced with the exchange file on 2026-10-04 (Q98 B1).
- Class: silent wrong number
- Proof: Run record of set 05: a line giving the sha256 of the copy's XML and whether it equals
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml, and the tool log SET line for
  BLD-ME-Ducts&Duct Fittings asking ME-Ductwork (set 03 log:243 asked ME-DUCTWORK). A case in
  tools\loop\prove-run.ps1 that a copy whose XML differs from the exchange file is named in the
  record. No Core test.
- Branch: rests on F106 (run.ps1), merged as 3449521, and F108 (prepare-copy.ps1), merged as
  c9b223b.
- Note: B1 allows either: the desktop file is the exchange file, or the corrected one goes into
  the copy and the turn says so. prepare-copy.ps1 already sees a changed desktop file by sha256
  and remakes the copy, so the set 05 copy will carry the new file. The corrected file is not
  enough on its own, FR-008.

### FR-083 f106-verdict-ran-ignores-group-results

A window run reads VERDICT RAN with exit 0 whenever the tool's log holds a RESULT block, the
installed stamp and no unticked group, and never looks at how many groups are done, partial or
failed, so a run in which every group FAILED still reads RAN.

- Sources: F106-HARM-BRK-1
- Evidence: tools\loop\run.ps1:810-835 (ToolLogVerdict reads the RESULT title, the SESSION
  plugin version and the unticked count only), :715-721 (RunVerdict) and :1784-1787. Run line:
  steps\runs\03\item1-C06\record.txt:1070 'VERDICT: RAN, item 1 through the window, the log's
  RESULT block read, closed by Dispose, put back' while steps\runs\03\findings.md:11 reads '17
  DONE, 0 PARTIAL, 5 FAILED of 22'.
- Root cause: tools\loop\run.ps1:810-835 and :715-721.
- Class: silent wrong number
- Proof: No Core test applies, nothing under src changes. A prove-run.ps1 case: ToolLogVerdict
  and RunVerdict given a log whose RESULT block reads 0 groups done and 22 failed do not return a
  plain RAN with exit 0. Set 05: the VERDICT line of record.txt carries the done, partial and
  failed counts.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Whether the counts go into the text or a FAILED group gives a non zero exit is the
  lead's design call. FR-105 is the separate ordering fault inside RunVerdict.

### FR-084 copy-set-takes-bader-outputs-as-inputs

-Set NN copies whatever NM Fed holds in NWF, NWD and Clash Report and reports a whole fresh copy,
so outputs that Bader's own runs left in NM Fed land in runs\NN\NMFed and in NMFed.manifest.txt
as if they were input files, and nothing refuses or flags it.

- Sources: F108-BRK-1, F108-R5
- Evidence: tools\loop\prepare-copy.ps1:282-288 check only that NWC, Clash Report and a folder
  under NWC exist, :273 reads, and :345-351 copy and mark every file whole.
  steps\runs\00\source-listing-during-bader-run-1B06BC.txt (780 files, an NWF at NWF\C06 and a
  pictures folder under Clash Report) is NM Fed holding outputs on 2026-09-27. The register row
  F108-R5 (turn5\pr83-diff-rest.txt:115) says the same.
- Root cause: tools\loop\prepare-copy.ps1:273-288 and :345-351, the -Set mode. The plain command
  reads and copies everything the same way (:188-192 and :224-230 at f38edd5).
- Class: silent wrong number
- Proof: No Core test applies. A case in the F108 proof script (turn4\f108-prove.sh, outside the
  repo) or a new tools\loop harness case: a file in NM Fed under NWF or Clash Report makes -Set
  refuse, or stay out of the manifest and say so. Set 05: NMFed.manifest.txt of the fresh copy
  names no file outside NWC and the XML.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: F106 check 9 (run.ps1:355-362, OutputsRefusal) refuses item 1 loudly when
  NMFed\NWF\C06, NWD\C06 or Clash Report\C06 hold a file, which is the way the F108-R5 row names,
  so the silent case left is a folder at the top of Clash Report or in another community.
  F108-R5 also sits on FR-101.

### FR-085 f106-autosave-putback-deletes-unowned-files

The AutoSave put back treats every file that appeared in his AutoSave folder during the run as
the run's own and deletes it with no copy taken first, and makes a gone file again from the
backup, so a file put there or taken away by anything but the adopted Navisworks is deleted or
made again.

- Sources: F106-HARM-BRK-2
- Evidence: tools\loop\run.ps1:557 labels any file absent before as 'ADDED by the run', :563-567
  delete it with [System.IO.File]::Delete after only a Roamer check (:559-560) and a hash read
  again (:565), and :571-581 copy a gone or changed one back from autosave-backup.
- Root cause: tools\loop\run.ps1:546-585 (PutBackAutoSave), mainly :557 and :563-567.
- Class: broken feature
- Proof: No Core test applies. A prove-run.ps1 case on fake folders: a file added to the AutoSave
  folder that is not an autosave of one of the run's own documents is left, or copied aside
  first, and the run's own autosaves are still removed and read back. Set 05: the AutoSave lines
  of record.txt name the rule that owned each file removed.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Low odds. Q86 lets the loop remove the autosaves its own runs added, so the fix stays
  inside that answer. Set 03 put back 78 of his files after Navisworks rotated them out
  (record.txt:877 on, steps\loop.md:80-81). The rule text that still says nothing is written
  into his AutoSave folder is FR-138.

### FR-086 guard-unreadable-process-list-reads-as-none

In the put back a process list that cannot be read counts as no Navisworks running, and
ProcState turns a failed read into gone, so a write can go ahead beside a Navisworks the loop
cannot see.

- Sources: T3-G1, T3-G8
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:1226 '$ListRoamers = { @(Get-Process -Name Roamer
  -ErrorAction SilentlyContinue) }' is the last check before each write, called at :627 and
  :693. :532-533, ProcState returns gone when the process is null. :1215 and :783 read the list
  the same way. The start refusal at :603 does it right, 'the process list could not be read'.
  On main 3449521 the list is at nw-guard.ps1:1318 in SettingsPutBack (:1311), ProcState at :623,
  PutBackRegistry at :713, PutBackFiles at :781, NewRoamers at :873, PutBackReasons at :1279 and
  RoamerRefusal at :692.
- Root cause: tools\loop\nw-guard.ps1, the ListRoamers block in SettingsPutBack and its use in
  PutBackRegistry and PutBackFiles, ProcState, and the reads in NewRoamers and PutBackReasons.
- Class: broken feature
- Proof: A new case in tools\loop\prove-run.ps1 that gives the put back a process list that
  throws and expects no write and an UNKNOWN reason. No Core test reaches PowerShell.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: T3-G1 and T3-G8 are one fault in two places (G8 is the family of G1), so one fix.
  steps\01_next.md:974 says F103 left T3-G1 to G5 unfixed on purpose. The harm needs a list that
  fails to read at the moment of the put back, and no run has shown it.

### FR-087 adopt-runs-after-deadline-path-with-watchdog-stopped

After the constructor deadline path ran and TerminateProcess returned false, the watchdog stops
itself and nothing checks that before adoption, so the hold, Visible and Dispose can run with no
watchdog, no ceiling and no call limit.

- Sources: T3-G2
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:1001-1002 '$sync.Stop = $true' then 'break'
  after the TerminateProcess line. DeadlineDone is read only at :964 and :1195 (grep). AdoptStart
  at :1109-1163 and run.ps1:1053-1055 never read it. On main 3449521 Stop is at
  nw-guard.ps1:1093, DeadlineDone is read at :1056 and :1287, AdoptStart is at :1201, and the
  adoption call at run.ps1:1544.
- Root cause: AdoptStart in tools\loop\nw-guard.ps1 reads neither $sync.DeadlineDone nor
  $sync.Stop, and neither does its caller in tools\loop\run.ps1.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 where the deadline path runs, TerminateProcess
  is made to return false, and adoption is refused. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: PutBackReasons already refuses the put back in this case, so his settings stay as they
  are. What is lost is the ceiling and the call limit.

### FR-088 adopt-start-has-no-time-bound

The stretch from the constructor returning to the pid being held has no time bound, so a hung
process list or WMI read inside AdoptStart can stall the run with nothing to end it.

- Sources: T3-G9
- Evidence: At f38edd5, tools\loop\run.ps1:1049 '$sync.CtorReturned = $true' comes before
  AdoptStart at :1053. The constructor deadline at nw-guard.ps1:964 needs CtorReturned false and
  the call limit at :1026 needs MyPid to be set, which happens only inside AdoptStart (:1160).
  AdoptStart calls NewRoamers and RoamerRecord, which read the process list and Win32_Process. On
  main 3449521 CtorReturned is set at run.ps1:1540, AdoptStart is called at :1544, the deadline
  is at nw-guard.ps1:1056 and AdoptStart at :1201.
- Root cause: neither the constructor deadline nor the call limit in tools\loop\nw-guard.ps1
  covers the stretch, with the order in tools\loop\run.ps1 and AdoptStart in nw-guard.ps1.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 where the WMI read inside adoption is made to
  hang and the run ends by a bound. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: The breaker on ownership found a WMI stall right after the constructor returns, noted in
  steps\notes\f103-final-reading.md:118-119.

### FR-089 put-back-key-unreadable-at-end-counted-nowhere

A key under HKCU Navisworks Manage 22.0 that read at the start and cannot be read at the end is
neither compared nor counted, so the put back reads whole and the run exits 0.

- Sources: T3-G6
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:238 adds the line 'could not read at the end ...
  Nothing is written under it' and :258 adds 'could not be read at the end, not compared, nothing
  written under it', and neither adds an entry to Changes. The summary at :1253-1259 counts
  Changes, appeared files, in use files and not backed up files only. tools\loop\run.ps1:1200 sets
  NotPutBack from NotWritten and AutoSave alone. On main 3449521 the first line is at
  nw-guard.ps1:260 in DiffRegistry (:258), and the summary is in SettingsPutBack (:1311).
- Root cause: DiffRegistry in tools\loop\nw-guard.ps1 leaves these keys out of Changes, the
  summary in SettingsPutBack leaves them out of the count, and run.ps1 sets NotPutBack without
  them.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 with a key made unreadable at the end,
  expecting NotWritten above 0 and exit 6. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: The register says silent. Nothing is written under such a key, so nothing is damaged.
  Only the claim of a whole put back is wrong.

### FR-090 watch-append-failure-switches-off-put-back

Anything that reads watch.txt with a locking read during a run can make one watchdog append
fail, which switches off the whole put back and blames the watchdog.

- Sources: T3-G7
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:849-850, the watchdog W function, catches a
  failed append into $sync.WriteErrors with no retry, and :1194 adds the reason 'the watchdog
  could not write N lines' whenever WriteErrors holds any. ReadShared (:558-561) protects only
  run.ps1's own heartbeat read at run.ps1:646. On main 3449521 the catches are at
  nw-guard.ps1:942 and :946, the reason is in PutBackReasons (:1279) and ReadShared is at :650.
- Root cause: the W function of the Watchdog in tools\loop\nw-guard.ps1 and the reason it feeds
  in PutBackReasons.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 that holds watch.txt open with a locking read
  during an append and expects the append retried or the reason told apart. No Core test
  applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: Silent until the end, when the put back is skipped and the backup is kept, so nothing of
  his is lost.

### FR-091 m5-walk-has-no-time-limit

M5 walks %TEMP% and the Autodesk folders with no time limit and the put back waits on it.

- Sources: T3-G10
- Evidence: At f38edd5, tools\loop\run.ps1:429-441 NewerFiles runs 'Get-ChildItem -LiteralPath
  $dir -Recurse -File -Force' over every root, and :1124-1125 hand it $env:TEMP and four
  Autodesk folders and wait for the list before the put back at :1163. On main 3449521
  NewerFiles is at run.ps1:645.
- Root cause: NewerFiles in tools\loop\run.ps1 and its call before the put back.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 with a walk made slow and a bound that cuts it
  and says so. No Core test applies.
- Branch: lines moved with F106 (3449521), the main line of NewerFiles is given above.
- Note: On the third real start M5 listed 62 files, 33 of them under %TEMP% (steps\loop.md, turn
  4), so the walk finishes today. Only the missing bound is the fault.

### FR-092 f106-harness-stale-after-window-run

prove-run.ps1 is still F103's. Now that F106 is merged, its H0 forbids PostMessageW and a file
delete in run.ps1 and nw-guard.ps1, which F106 adds on purpose, its H6 calls HangVerdict with
the old four arguments, and it holds no case for PutBackAutoSave, PostClose, Answer, PathsOutside
or IsConfirm.

- Sources: F106-HARM-REV-2, F106-HARM-BRK-13, R1 of F106, R4 of F106
- Evidence: tools\loop\prove-run.ps1:224, the word list holds 'PostMessageW', :242-243 flag a
  Delete call, :270 holds the PostMessageW bad case, and :579 and :586 call 'HangVerdict $row[1]
  $row[2] $tNow 20' against the five argument HangVerdict at run.ps1:668. F106's PostMessageW is
  at nw-guard.ps1:406, run.ps1:886 and drive-window-run.ps1:245, and its delete at run.ps1:566.
- Root cause: tools\loop\prove-run.ps1:224, :242-243, :254, :270, :579 and :586, unchanged on
  main 3449521, where they now disagree with F106's run.ps1.
- Class: broken feature
- Proof: No Core test applies. prove-run.ps1 itself runs green on the merged F106 code and gains
  the cases of turn4\f106-proof\prove-f106.ps1 (47 passed, outside the repo) for PutBackAutoSave,
  PostClose, Answer, PathsOutside and IsConfirm. R4 of F106: H0 allows PostMessageW in PostClose
  and the driver's Answer only.
- Branch: rests on F106, merged as 3449521. The harness reads the same on main as at f38edd5.
- Note: Without it the loop has no proof of its own guards since F106 merged. The other loop
  tool fixes are proved in this harness, so this goes first in the lane.

### FR-093 remove-restore-refuse-name-in-both-communities

-Remove and -Restore refuse any NWC whose file name sits in both communities, which is all three
files of group 100000, so items 3 and 4 of a run set cannot take a file out of that group or put
one back.

- Sources: F108-BRK-5, F108-R1
- Evidence: At f38edd5, tools\loop\prepare-copy.ps1:112 throws 'the name is under the folder 2
  times', from Find-One (:107-114), called at :152 for -Remove and :165 for -Restore. On main
  3449521, since F108, Find-One is at :180-187 and is called at :226 and :239.
  %LOCALAPPDATA%\NwcFederatorLoop\source.manifest.txt lines 2-4 and 69-71 hold the same three
  names with the same sha256 and size under NWC\C06 and NWC\C07.
- Root cause: Find-One in tools\loop\prepare-copy.ps1 and its two callers.
- Class: broken feature
- Proof: No Core test applies. A proof case in the F108 proof script (outside the repo) or a new
  harness case: -Remove of 1104-PAR-100000-ZZZ-AR-MOD-003000.nwc with a community or a relative
  path takes out exactly one copy, and -Restore puts it back by hash, the other community's copy
  untouched. Set 05: items 3 and 4 run on a group 100000 file. The F108-R1 row's way out: the
  file named with its community, or the group for runs 3 and 4 chosen outside 100000.
- Branch: on main since 2026-09-27 and in F108's code, merged as c9b223b.
- Note: The group is the one the per community Clash Report folders were made for. The F108
  proof picked its two names with uniq -u and never met this.

### FR-094 f106-no-hang-clock-when-log-not-single

When no new log, or more than one, names the installed stamp in its SESSION block, for example
because Bader opened the tool himself, the monitor starts no hang clock and posts no WM_CLOSE
after a RESULT block, so a run that finished waits for the 12 hour ceiling with the keep awake
request on and the put back waiting.

- Sources: F106-HARM-REV-6, F106-HARM-BRK-5, R2 of F106
- Evidence: tools\loop\run.ps1:962-986 takes a log only when exactly one new log names the stamp
  (:968), and more than one writes UNKNOWN to toollog-name.txt (:978-982) and starts no clock.
  The RESULT sighting that posts WM_CLOSE is set only from the log stream (:1010) and needed at
  :1097.
- Root cause: tools\loop\run.ps1:962-986 and :1095-1117.
- Class: slow
- Proof: No Core test applies. A prove-run.ps1 case with two stand-in logs naming the stamp
  inside one pass: the run still ends by a rule other than the 12 hour ceiling and says why.
  Set 05: record.txt has a log found line before any RESULT.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: RunLog.cs:1142 writes only 'plugin version :' in SESSION and no process id, so how to
  tell the loop's log from Bader's own is a design call. FR-113 is the same choice seen from the
  name file.

### FR-095 f106-monitor-fault-leaves-plugin-call-unbounded

A fault in the monitor sets RunOver FAULT but never calls EndArm, and the main thread sits in
ExecuteAddInPlugin with CallLimit 0, so nothing posts WM_CLOSE or applies the hang rule and only
the 12 hour ceiling frees the run, with the tool's window up on his screen and the keep awake
request on.

- Sources: F106-HARM-REV-7, F106-HARM-BRK-4
- Evidence: tools\loop\run.ps1:1137-1141, the catch sets MonitorFault and RunOver without
  EndArm, :1147-1151 EndArm sets the 120 s limit and is called only at :1089, :1091 and :1107,
  and :1593 '$sync.CallLimit = 0'.
- Root cause: tools\loop\run.ps1:1137-1141 and :1593.
- Class: slow
- Proof: No Core test applies. A prove-run.ps1 case: force a fault inside Monitor and assert
  CallLimit is bounded and WM_CLOSE or a forced end follows within the limit.
- Branch: rests on F106, merged as 3449521, lines unchanged.

### FR-096 f106-read-workbook-no-time-limit

read-workbook.ps1 runs through RunChild, which waits with no time limit, before the keep awake
release, so a read-out that never ends holds the run, the loop's lock and the keep awake
request.

- Sources: F106-HARM-REV-8
- Evidence: tools\loop\run.ps1:1832 calls RunChild, whose WaitForExit() at :1238 has no limit,
  and the keep awake release is at :1846-1853, after it. Set 03 read all 22 workbooks without a
  hang (record.txt:1046-1067).
- Root cause: tools\loop\run.ps1:1226-1240 (RunChild) and :1826-1835. RunChild was the same
  function at run.ps1:736-750 at f38edd5.
- Class: slow
- Proof: No Core test applies. A prove-run.ps1 case with a child that never ends: the call
  returns with a named timeout and the keep awake request is still released.
- Branch: the call rests on F106, merged as 3449521, lines unchanged. RunChild was on main
  before it.
- Note: The limit is a setting. The same RunChild change serves FR-079.

### FR-097 f106-foreign-window-stops-driver-and-close

Any visible window of the adopted Navisworks other than the tool's window and the main window, a
floating panel or a dialog of the tool among them, stops the driver as DIALOG after three passes
and keeps WM_CLOSE from being posted, so the run ends HUNG after 300 s and is closed through the
held handle instead of RAN.

- Sources: F106-HARM-REV-5, R3 of F106
- Evidence: tools\probes\drive-window-run.ps1:316-317 and :359-363 stop on three passes of other
  dialogs, tools\loop\run.ps1:1104 takes as busy any visible window whose kind is neither WINDOW
  nor MAIN, and :1110-1111 leave WM_CLOSE unposted. tools\loop\nw-guard.ps1:535-540 (WindowKind)
  returns DIALOG for any other window.
- Root cause: tools\loop\run.ps1:1104-1111, tools\probes\drive-window-run.ps1:281-284, :316-317
  and :359-363, and tools\loop\nw-guard.ps1:535-540.
- Class: loud failure
- Proof: No Core test applies. A prove-run.ps1 case with the stand-in's three window role: a
  floating panel and a tool dialog end the run with a named finding and no forced kill. Set 05:
  record.txt shows no HANG line and a VERDICT that names any dialog.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: R3 of F106 is the design side: a dialog of the tool is left up on purpose, because the
  rule says every dialog is a finding with its text, so it ends only by HUNG after 5 minutes.
  Whether to answer known harmless dialogs is the lead's call.

### FR-098 f106-confirm-child-read-timeout-not-seen

A child of the tool's confirm whose WM_GETTEXT read times out at 500 ms leaves no text and no
marker, so the driver's 'not read whole' rule cannot see it. The confirm has one text, and
losing it makes IsConfirm false, so the driver stops on it as a DIALOG and the run ends HUNG.

- Sources: F106-HARM-REV-4
- Evidence: tools\loop\nw-guard.ps1:472-476 reads each child with SendMessageTimeoutW and a 500
  ms limit and adds a text only when it is not empty. tools\probes\drive-window-run.ps1:370 looks
  only for a text of 2040 characters or more or the markers 'and N more visible children not
  read' and 'the walk of its children stopped'. IsConfirm (nw-guard.ps1:547-551) then reads
  false, and drive-window-run.ps1:359-363 stop the driver after three passes.
- Root cause: tools\loop\nw-guard.ps1:472-476 and tools\probes\drive-window-run.ps1:359-370.
- Class: loud failure
- Proof: No Core test applies. A prove-run.ps1 case with a stand-in confirm whose child never
  answers WM_GETTEXT: the driver answers Cancel or names the unread child, and does not stop as
  DIALOG. Set 05: driver.txt shows the confirm texts read whole.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Set 03 did not hit it (record.txt:81 'DRIVER exited 0, PRESSED').

### FR-099 unproved-start-refuses-on-any-process-holding-the-pid

A line of unproved-starts.txt that carries start ticks refuses the next run whenever any process
holds that pid and its start time cannot be read, whatever its name.

- Sources: T3-G3
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:768 adds 'pid N, start ticks N, a process holds
  that id now and its start time cannot be read' when the start time is null. The branch for a
  line with no ticks at :749 does test that the process is a Roamer, and this one does not. On
  main 3449521 the two are at :860 and :841, in UnprovedRefusal (:826).
- Root cause: UnprovedRefusal in tools\loop\nw-guard.ps1 (:765-769 at f38edd5).
- Class: loud failure
- Proof: Harness case in tools\loop\prove-run.ps1: a line with ticks whose pid is now a process
  other than a Roamer, with an unreadable start time, does not refuse. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: Old, from F100. It refuses on the safe side and a person looks.

### FR-100 git-wall-reads-main-clone-branch

The git wall judges a push by the branch of the main clone, so it refuses a push from a worktree
on a fix branch whenever the main clone has main checked out.

- Sources: T3-W1
- Evidence: .claude\hooks\refuse-git-on-main.sh:57-58 change to "${CLAUDE_PROJECT_DIR:-.}" and
  then read the branch with git rev-parse --abbrev-ref HEAD, the main clone, once. :120 skips the
  -C argument and does not follow it. :161 'main is checked out and nothing is pushed from it.'
- Root cause: .claude\hooks\refuse-git-on-main.sh:57-58, :120 and :161.
- Class: loud failure
- Proof: A new case in tools\loop\prove-hooks.sh: a clone with main checked out and a git
  worktree on a fix branch, where a push of that branch from the worktree exits 0 and a push to
  main from it exits 2.
- Note: A false refusal on the safe side. A wall change goes into its own folder, is proved with
  prove-hooks.sh, read by a breaker, and copied in by a command (.claude\rules\loop.md). The main
  clone is on a fix branch, never on main, so today's pushes pass.

### FR-101 copy-set-no-navisworks-check

prepare-copy.ps1 never looks for a running Navisworks. -Set reads and copies NM Fed, and -Remove
and -Restore change a set's copy whoever runs what, so a read of a file can make his own tool's
write to it fail inside his run while the loop sees success.

- Sources: F108-BRK-2, F108-R5
- Evidence: No Roamer or Get-Process anywhere in tools\loop\prepare-copy.ps1 at f38edd5 or in
  F108's version (read whole). Reads at :188 and :229 at f38edd5 and at :273, :323 and :345 in
  F108's version. Bader ran his tool with NM Fed as its output folder on 2026-09-27 from 16:37 to
  about 17:00 (steps\loop.md:238-246). The register row F108-R5 says the same, nothing in it
  reads for a running Navisworks.
- Root cause: tools\loop\prepare-copy.ps1, the plain command (:188-192 and :224-230 at f38edd5)
  and -Set, -Remove and -Restore (:273, :323, :345 and :223-254 on main 3449521).
- Class: loud failure
- Proof: No Core test applies. A proof case with a stand-in Roamer running: every mode refuses
  with one line naming the pid and writes nothing.
- Branch: rests on F108, merged as c9b223b, lines unchanged, and on the plain command, which was
  on main before.
- Note: It needs his write to hit the very file being read in that instant. The plain command
  has read NM Fed this way since 2026-09-27, and F108 only adds reads. F108-R5 also sits on
  FR-084.

### FR-102 copy-manifest-kept-short-or-changed

The plain command keeps the source copy when it matches NM Fed and never reads
source.manifest.txt, which is written with WriteAllLines and so not atomically. A short or
changed manifest is kept until NM Fed next changes, and every -Set then copies about 208 MB,
fails at the read back, and says to give the next set number, which fails the same way and
leaves a copy with no manifest each time.

- Sources: F108-REV-4, F108-BRK-3, F108-R2
- Evidence: tools\loop\prepare-copy.ps1:290-308 (the keep test), :329 (the manifest written),
  :342 and :346-350 (the read back against it), and :349 (the advice 'Give the next set
  number', read on main 3449521). Proof case A6 of F108, one sha256 zeroed in the manifest, ends
  REFUSED at the read back, turn4\f108-proof.txt.
- Root cause: tools\loop\prepare-copy.ps1:290-308, :329 and :349. At f38edd5 the same keep test
  was at :194-206 and the write at :239, and nothing read the manifest content, so the damage
  did not show there.
- Class: loud failure
- Proof: No Core test applies. A proof case: zero one sha256 in source.manifest.txt, then -Set
  NN finds it at the plain check and remakes the source copy, or refuses naming
  source.manifest.txt, and its advice does not send to the next set number. The F108-R2 row's
  way out: the keep check reading the manifest too.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Loud each time, and it touches nothing of Bader's.

### FR-103 remove-writes-note-before-delete

-Remove writes its note (source.removed.txt or NMFed.removed.txt) before it deletes the file. If
the delete fails, for example a read only file or one a scanner holds, the note names a file
still in the copy, -Remove says already out, -Restore says already in the copy, the plain
command and -Set NN refuse, and only deleting the note by hand ends it. A -Restore that fails
after the .partial copy leaves the .partial.

- Sources: F108-REV-3, F108-BRK-4, F108-R3
- Evidence: At f38edd5, tools\loop\prepare-copy.ps1:155 '[IO.File]::WriteAllLines($removed, ...'
  then :156 'Remove-Item -LiteralPath $f.FullName'. The same order on main 3449521 at :229-231
  (WriteAllLines of $targetRemoved, then Remove-Item, then the check that the file is gone), and
  the .partial at :247-251.
- Root cause: tools\loop\prepare-copy.ps1:155-156 and :173-177 at f38edd5, and :229-231 and
  :247-251 on main.
- Class: loud failure
- Proof: No Core test applies. A proof case: mark the NWC read only or hold it open, -Remove
  fails, and no note is left standing, or the failure removes the note. Set 05: item 3 reads
  NMFed.removed.txt only for a file really gone. The F108-R3 row's way out: the note written
  after the delete.
- Branch: on main since 2026-09-27 and copied into -Set by F108, merged as c9b223b.
- Note: F106 check 8 (run.ps1:347) names the state and refuses, so it is loud.

### FR-104 set-remove-advice-sends-to-a-refusal

-Set NN -Remove on a set whose copy failed its read back says to run -Set NN alone first, and
-Set NN then refuses because the folder is there.

- Sources: F108-BRK-8, F108-R4
- Evidence: tools\loop\prepare-copy.ps1:224 'Run $make first' with $make = '-Set $Set alone'
  (:85), against the refusal at :258-263. Lines 85 and 224 read so on main 3449521.
- Root cause: tools\loop\prepare-copy.ps1:85, :224 and :258-263.
- Class: loud failure
- Proof: No Core test applies. A proof case on a copy with no manifest: -Remove names the real
  next step.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Words only, the advice is wrong. The F108-R4 row's way out: each made true where its
  file next changes.

### FR-105 run-verdict-wording-and-dead-fields

Run verdict faults: HUNG, a forced call and CEILING are decided before the end state, UNKNOWN
wins when the end state later reads gone, one unreadable sample ends the hold, a forced close
still exits 0, a foreign Roamer that changed no setting shows only in the put back reasons, and
two sync fields were set and never read.

- Sources: T3-G12
- Evidence: At f38edd5, tools\loop\run.ps1:464-466 against :468-469 (the order), :472 'RAN ...
  closed, FORCED' with exit code 0, :559-565 the monitor ends the run on one 'unreadable'
  sample, and :571, :574 and :1067 set $sync.LogAmbiguous and $sync.ToolLog, and a grep found no
  read of either. tools\loop\nw-guard.ps1:1204-1209 put a foreign Roamer in the reasons only. On
  main 3449521 RunVerdict is at run.ps1:697-731, where HUNG, END FORCED, a forced call and
  CEILING are still decided before the end state and 'RAN ... closed, FORCED' still takes code
  0. $sync.ToolLog is now read (run.ps1:1753 and :1780 on), and $sync.LogAmbiguous is still only
  set (:979 and :1578).
- Root cause: RunVerdict and the Monitor in tools\loop\run.ps1, the field set and never read,
  and PutBackReasons in tools\loop\nw-guard.ps1 (:1204-1209 at f38edd5, PutBackReasons at :1279
  on main).
- Class: noise
- Proof: Harness cases in tools\loop\prove-run.ps1 H18 (RunVerdict is pure and the harness calls
  it) for each order, and LogAmbiguous read or removed. No Core test applies.
- Branch: F106 rewrote RunVerdict and the monitor, merged as 3449521, so the f38edd5 lines moved
  as given above.
- Note: The sharper edge is the forced close that exits 0 as RAN. The text side of a close that
  fails twice is FR-107, and FR-083 is the separate fault that RAN ignores the group results.

### FR-106 record-writes-unmasked-exception-text

run.ps1 writes an exception's text, which holds the full path of a file of Bader's, into the
record unmasked at three lines. Run's record is copied into steps\runs, and Install's stays in
the loop folder.

- Sources: T3-G13 (placed by the register reader and the readings reader, folded here)
- Evidence: At f38edd5, tools\loop\run.ps1:903 'Say ("STOP: the installed bundle cannot be read
  whole, " + $inst.Why', :1014 '$stopText = "STOP before the start: " + $lb.Why' and :1020 the
  same with $ab.Why. Why is built from Err at run.ps1:286, :289, :304 and :329 and
  nw-guard.ps1:37-41, which prints the exception message unmasked. run.ps1:1211-1215 copy
  record.txt into steps\runs. On main 3449521 the three lines are at run.ps1:1384, :1499 and
  :1505, and Err is at nw-guard.ps1:47.
- Root cause: the three STOP lines in tools\loop\run.ps1, with the Why texts built in
  ListFolder and BackupNew and by Err in tools\loop\nw-guard.ps1.
- Class: noise
- Proof: A new prove-run.ps1 case: make ListFolder or BackupNew fail on a file under a fake
  profile path and assert the record holds the path masked the way install.ps1's REFUSED lines
  are (MaskLine, at nw-guard.ps1:76 on main). No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: No real start's record holds such a line. The breaker on Bader's things called it noise
  and not a new leak, because the tree already holds the user name in 13 files. It is the kind
  item 12 of fix list 3 fixed, at lines it did not name. FR-109 is the wider masking question.

### FR-107 close-fails-twice-text-says-closed

When the watchdog's close fails and the end close's Kill fails too, CloseAtEnd still writes that
the process was CLOSED here, and CloseOwn then refuses the folder because it holds a VERDICT
line, so only a person can end that Navisworks.

- Sources: T3-G16 (placed by the register reader and the readings reader, folded here)
- Evidence: At f38edd5, tools\loop\run.ps1:493 '... so it was CLOSED here through the held
  handle: ' + $cr.Text, where $cr.Text can read 'was sent Kill through the held handle and still
  runs' (nw-guard.ps1:576), and :687 'has a VERDICT line, so its run finished. Nothing was
  closed'. On main 3449521 the text is at run.ps1:744 in CloseAtEnd (:732-755), the refusal at
  :1177 in CloseOwn (:1174), and CloseAdopted at nw-guard.ps1:659.
- Root cause: CloseAtEnd and CloseOwn in tools\loop\run.ps1.
- Class: noise
- Proof: A new prove-run.ps1 case with CloseAdopted returning State same: the end text says the
  process was not closed, and CloseOwn accepts a folder whose VERDICT line is STOPPED with the
  process still running. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: Classed polish and old by the breaker on ownership. The verdict side of it is FR-105.

### FR-108 run-ps1-polish-closeown-buildserver-ownertext

Polish in run.ps1, the harness and the probe: CloseOwn reads autosave-before.txt with no count
or name check, dotnet build-server shutdown after an install stops every build server of the
account, an owner window destroyed mid read is written as a window of another process with its
handle, item 8's harness case sets Forced by hand and never makes the watchdog's Kill fail, and
the probe closes the adopted process outside the close lock, which run.ps1's CloseAtEnd takes.

- Sources: T3-P2 (one register item and two readings items, folded here)
- Evidence: At f38edd5, tools\loop\run.ps1:727 'DiffAutoSave (ReadListing $abFile ...' with no
  AutoSaveBefore check (:367-377 does it for Run), :927 'RunChild "dotnet" "build-server
  shutdown" $repo', and :666-669 OwnerText, where OwnerPid reads 0 for a destroyed owner and so
  differs from the adopted pid (nw-guard.ps1:451-458). On main 3449521 these are at
  run.ps1:1217, :1408 and :1156-1159. tools\loop\prove-run.ps1:1353 '$sy2.Forced = "the adopted
  deadline of 5 s passed, the close of pid ... through the held handle threw"' (in :1348-1356).
  tools\probes\probe-automation-start.ps1:869 and :889 call CloseAdopted with no CloseLock. The
  parts are listed in steps\notes\f103-final-reading.md:111-116.
- Root cause: tools\loop\run.ps1 (CloseOwn, the Install flow, OwnerText), tools\loop\nw-guard.ps1
  (the owner read), tools\loop\prove-run.ps1:1348-1356 and
  tools\probes\probe-automation-start.ps1:869 and :889.
- Class: noise
- Proof: prove-run.ps1 cases: CloseOwn with a cut autosave-before.txt names the difference,
  Install says which servers it stops or stops none it did not start, a destroyed owner reads as
  owner gone, and the watchdog's own close fails for real (a stand-in that survives Kill, or a
  CloseAdopted that returns State same). The probe's two closes take the lock. Each part gets its
  own harness case only when it is taken. No Core test applies.
- Branch: the run.ps1 lines moved with F106 (3449521) and are given above. prove-run.ps1 and
  probe-automation-start.ps1 read the same on main.
- Note: Q93: words and polish are register rows and not fix attempts. OwnerText was written by
  fix attempt 3 of F103. The probe is F100's measurement and keeps its own copy by design
  (tools\probes\README.md), so the lock change is the lead's call.

### FR-109 f106-evidence-carries-profile-paths

Evidence written into steps\runs before F102's mask carries the full profile path. The tool's
log copy masks only its FOLDERS REMEMBERED block, its .tsv and the first line of every workbook
read-out go in as written, and DIALOG and CONFIRM texts go into record.txt unmasked.

- Sources: F106-HARM-REV-15, F106-HARM-BRK-9
- Evidence: steps\runs\03\item1-C06\run-20261001-140037.log:1 'Log opened at
  C:\Users\<profile>\AppData\Local\ParsonsNwcFederator\logs\run-20261001-140037.log' and 720 more
  lines, the .tsv 177 lines, and line 1 of each of the 22 workbook read-outs 'workbook
  C:\Users\<profile>\AppData\Local\NwcFederatorLoop\runs\03\NMFed\Clash Report\C06\...xlsx', 920
  lines in 24 files, 987 times on them, and 22 more lines of the log hold it as the author on
  the NWD publish line, log:482 for one, 1009 times on 942 lines in all, read with grep on
  2026-10-04 into turn5\profile-name-count.txt. Code: tools\loop\run.ps1:1792-1796 (MaskRemembered only), :1802 (the
  tsv copied as is), :1059-1060 (the texts), and tools\loop\read-workbook.ps1:102.
- Root cause: tools\loop\run.ps1:1792-1802 and :1059-1060, and tools\loop\read-workbook.ps1:102.
  The kinds F102 masks are in tools\checks\evidence-ids.txt and name no profile folder.
- Class: noise
- Proof: No Core test applies. A check-evidence-ids.sh and mask-evidence.ps1 case: a line holding
  the profile folder is masked and refused. A prove-run.ps1 case that the record's DIALOG and
  CONFIRM lines go through MaskLine. Set 05: the committed log, tsv and read-outs hold no profile
  folder.
- Needed Bader, asked as Q105, answered on 2026-10-04, see the answered line of this item. Whether his Windows profile name is a trace to keep out of committed evidence
  was his call, answered on 2026-10-04, Q105, because F102 masks only the machine name and the licensing ids and the tree
  already holds the name in many files. Masking the record's own DIALOG and CONFIRM texts with
  the existing MaskLine needs no answer.
- Branch: the run.ps1 part rests on F106, merged as 3449521, lines unchanged. read-workbook.ps1
  was on main before.
- Answered by Bader on 2026-10-04 at 15:23, Q105, in short, his words being under the question in steps\02_questions.md: mask the profile folder name in paths and as the author, and the evidence check refuses this machine's name, the older machine's name and both account names, the older ones held as a hash. The lead's note: to be worked by F117, the names, in wave 5 with the noise.

### FR-110 evidence-check-blind-to-older-machine-name

Nothing in the repo refuses the older machine's name once F107 masks it, so it can come back and
every check stays green. F102's mask and check read for the name of the machine they run on and
for no other.

- Sources: F107-BRK-2, F107-R1
- Evidence: tools\checks\check-evidence-ids.sh:17 'The machine name is read for only when
  COMPUTERNAME' and :106 'machine=${COMPUTERNAME:-}', and tools\checks\evidence-ids.txt:67-71,
  where the machine name kind uses {machine}. Main named that machine on 9 lines at f38edd5, for
  example docs\history\scan.md:3123 and tools\probes\drive-window-run.ps1:21, and F106 has since
  rewritten that driver header (the readings reader saw 'the machine of 2026-09-19' at line 69 on
  fix-F106). The F107-R1 row (turn5\pr83-diff-rest.txt:116) adds that copies of files holding the
  name sit in the loop folder.
- Root cause: tools\checks\check-evidence-ids.sh:17 and :106 and tools\checks\evidence-ids.txt:67-71.
- Class: noise
- Proof: No Core test applies. A tools\checks\broken fixture that carries the older name and is
  refused, and a clean file that passes, run by Actions. The name itself must not go back into
  the rules file in clear, so a hash or another way is needed, UNKNOWN how, as the F107-R1 row
  says.
- Needed Bader, asked as Q106, answered on 2026-10-04, see the answered line of this item. Q88 asked only for the mask on main. Making the check refuse a second machine
  name is new scope and a design choice, and the F107 section itself says the check would never
  refuse that name.
- Branch: waits for F107's merge, fix-F107 not merged at main 3449521.
- Answered by Bader on 2026-10-04 at 15:23, Q106, in short, his words being under the question in steps\02_questions.md: the evidence check refuses the older machine's name, held as a hash. The lead's note: to be worked by F117, the names, in wave 5 with the noise.

### FR-111 f106-link-check-skips-output-folders

ParamRefusal looks for a junction or link only on the run folder, the NWC folder, the XML and
the open file. The copy's NWF, NWD and Clash Report folders, where the tool writes, are not
checked, and the driver's UnderRoot compares path text only, so a link made by hand there would
carry the tool's writes out of runs\NN.

- Sources: F106-HARM-REV-3, F106-HARM-BRK-7
- Evidence: tools\loop\run.ps1:231-240, the PathRefusal loop over Nwc, XmlFile and OpenSource
  only, :355-369 (OutputsRefusal checks the folders exist and are empty, not for a link), and
  tools\probes\drive-window-run.ps1:116-120 (UnderRoot).
- Root cause: tools\loop\run.ps1:231-241 and tools\probes\drive-window-run.ps1:116-120.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case: a junction at NMFed\NWF\<Folder> is refused
  by ParamRefusal or Check with a REFUSED line.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Not reachable now, F108's copy makes plain folders.

### FR-112 f106-logs-after-changed-file-counts-covered

LogsAfterWindow treats a file of his that CHANGED as covered whenever logs-backup holds its old
content, so a changed file of his stays changed and never counts as not put back. Q82 allows
only the pruning of his oldest logs, which reads as GONE.

- Sources: F106-HARM-REV-10
- Evidence: tools\loop\run.ps1:599-601, CHANGED is labelled a FINDING when the backup holds the
  old content, and Lost is counted only in the else.
- Root cause: tools\loop\run.ps1:592-609 (LogsAfterWindow), :599-601.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case on LogsAfterWindow: a CHANGED file of his
  counts as not put back.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Nothing in a run changes an existing log of his today, so low odds.

### FR-113 f106-toollog-name-first-log-wins

toollog-name.txt names the one new log that carries the installed stamp at the first pass that
sees exactly one, so a tool session of Bader's own whose SESSION line lands before the loop's
would be named as the loop's, and the close of the loop, which removes what this file names,
would take his log.

- Sources: F106-HARM-REV-9
- Evidence: tools\loop\run.ps1:966-972, when exactly one log is found toollog-name.txt is
  written. The removal of the loop's own log is not built yet, run.ps1:1788-1791 leaves both
  files for the close of the loop.
- Root cause: tools\loop\run.ps1:962-972.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case: a first log whose content is not the plugin
  call's session is not named, and the close reads the log's own content before it removes
  anything.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Unlikely inside one 15 s pass. FR-094 is the same choice of log seen from the hang clock.

### FR-114 f106-answer-handle-reuse-after-lock-wait

The driver's Answer is handed the confirm's handle before Gate, and after a long lock wait it
checks again only the pid and the button name, not that the window is still the confirm, so a
handle reused by another dialog of the same pid that has an OK button would be answered.

- Sources: F106-HARM-BRK-6
- Evidence: tools\probes\drive-window-run.ps1:235-247: Gate at :236, the pid check at :238-239,
  the button name at :242, WM_COMMAND posted at :245, with no IsConfirm read again.
- Root cause: tools\probes\drive-window-run.ps1:235-247.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case: after a lock wait the confirm is read again
  with IsConfirm and a different dialog is not answered.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Very low odds.

### FR-115 f106-confirm-buttons-matched-by-english-names

The confirm's OK and Cancel buttons are matched by their English UI Automation names, so on a
Windows display language other than English the driver sends nothing to the confirm and the run
ends only when the hang rule fires after about 5 minutes.

- Sources: F106-HARM-BRK-3
- Evidence: tools\probes\drive-window-run.ps1:242 'if ($b.Current.Name -cne $what)', with Answer
  called as 'OK' at :377 and 'Cancel' at :374.
- Root cause: tools\probes\drive-window-run.ps1:241-243.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case with a stand-in confirm whose buttons carry
  other names: the driver answers by button id 1 and 2 alone.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Bader's machine is English, so it bites only a loop run on another.

### FR-116 f106-no-free-space-check

run.ps1 makes no free space check on C: before the start, though the backups, the outputs and
the adopted Navisworks's autosaves all land there. A failed backup copy stops the run safely,
and a full disk during the run is not read.

- Sources: F106-HARM-BRK-10
- Evidence: tools\loop\run.ps1:1497-1506, checks 13 to 15 hold no DriveInfo.
  tools\loop\prepare-copy.ps1:214-219 at f38edd5 does check room for its copy.
- Root cause: tools\loop\run.ps1:1497-1521.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case with a stubbed free figure under the needed
  size: Check and Run refuse with one line. The needed size is a setting.
- Branch: rests on F106, merged as 3449521, lines unchanged.

### FR-117 watchdog-load-unmeasured

The watchdog lists every process every half second for the whole run and sends WM_GETTEXT reads
to the adopted Navisworks's windows, proved for minutes and never for hours, so its load on
Bader's machine while he works is unmeasured.

- Sources: F106-HARM-BRK-11
- Evidence: tools\loop\nw-guard.ps1:885 '$procs = @(Get-Process)' and :1048 'Start-Sleep
  -Milliseconds 500' at f38edd5, :977 and :1140 on main 3449521. Run line:
  steps\runs\03\item1-C06\record.txt:826 'watchdog passes 12073, longest pass 7763 ms, longest
  gap 8270 ms'. Its processor time is in no record.
- Root cause: the Watchdog loop in tools\loop\nw-guard.ps1 (:877-1049 at f38edd5, :969-1141 on
  main).
- Class: noise
- Proof: No Core test applies. The watchdog's end line (run.ps1:1144 at f38edd5) gains the
  watchdog's processor seconds against the run's seconds, a prove-run.ps1 case checks the line,
  and set 05 record.txt shows the figure.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: UNKNOWN whether it slows his machine or the tool's run. Item 0 runs show longest passes
  of 8.9 to 11.8 s (record.txt:86 of steps\runs\02\item0 and :85 of steps\runs\01\item0).

### FR-118 set-number-rule-and-readback-written-twice

The two digit set number rule is written as a strict pattern in prepare-copy.ps1 and as a looser
one in run.ps1, where \d also takes digits of other writing systems and $ takes a trailing
newline. The read back of the source copy and the read back of the set's copy are two near
copies of one check.

- Sources: F108-REV-6, F108-R4
- Evidence: tools\loop\prepare-copy.ps1:74 '\A[0-9]{2}\z', read on main 3449521, against the
  run.ps1 rule '^\d\d$' (run.ps1:147 at f38edd5, :195 on main), and prepare-copy.ps1:325-327
  against :347-349.
- Root cause: tools\loop\prepare-copy.ps1:74, :325-327 and :347-349, and tools\loop\run.ps1:195
  on main.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case that run.ps1 refuses a set number with a
  trailing newline and with digits outside ASCII.
- Branch: rests on F108, merged as c9b223b, and run.ps1's rule moved with F106 (3449521).
- Note: CLAUDE.md: one rule lives in one place. Every difference ends in a refusal, so nothing
  is harmed. Polish.

### FR-119 probe-finalizer-claim-printed-as-fact

A named limits line prints what the finalizer does as a fact of this run, but it was read off the
IL of run 3 and no code on the run checks it.

- Sources: T3-G4
- Evidence: tools\probes\probe-automation-start.ps1:951 'Dispose did not complete and the
  finalizer was not suppressed, so at this process's exit it calls Bridge.Terminate, against a
  process that is gone if it was closed. What that does is UNKNOWN'. The header at :147-149 says
  the same.
- Root cause: tools\probes\probe-automation-start.ps1:951, and :147-149.
- Class: noise
- Proof: No test applies. The line says it was read off run 3, or reads UNKNOWN, checked by the
  probe's -ReflectionOnly output and a reader.
- Note: Old, from F100. The probe is a one off measurement and run.ps1 does not print this line.
  probe-automation-start.ps1 reads the same on main 3449521 as at f38edd5.

### FR-120 probe-main-window-read-by-pid-alone

After adoption the probe reads MainWindowHandle and MainWindowTitle by pid alone, without the
start ticks check that WindowsOf makes.

- Sources: T3-G5
- Evidence: tools\probes\probe-automation-start.ps1:760-761 '$mp = Get-Process -Id $myPid' then a
  Say of MainWindowHandle and MainWindowTitle, again at :768-769 and :774-775.
- Root cause: tools\probes\probe-automation-start.ps1:760-761, :768-769 and :774-775.
- Class: noise
- Proof: No test applies. A reader checks the diff and the copy of the probe's harness 4 is run
  again.
- Note: The register records that the breaker says MainWindowTitle sends WM_GETTEXT to whatever
  holds the pid and the reviewer says GetWindowText sends none. Which is right is UNKNOWN. The
  adopted pid is held through a handle at that point, so no other process can have it.

### FR-121 probe-polish-stale-sentences-and-unnamed-limit

Probe polish: a sentence that went stale when B3 landed, an always empty before set still read,
the work folder moved aside before step 2 can refuse, and the gap between the last Roamer read
and the constructor call not named as a limit.

- Sources: T3-P
- Evidence: tools\probes\probe-automation-start.ps1:151-153 rename the work folder before step 2.
  :140-149, the named limits omit the gap between the last read at :728 and the constructor at
  :739. tools\loop\run.ps1:72-74 '$beforeTicks = @{}' at f38edd5 (:94 on main 3449521), always
  none, is still read at nw-guard.ps1:786, :894 and :1197 (:878, :986 and :1289 on main). The
  stale sentences were at lines 395 and 1836 of b01ad71, and which lines they are on main is
  UNKNOWN.
- Root cause: tools\probes\probe-automation-start.ps1:140-153 and the reads of $beforeTicks in
  tools\loop\nw-guard.ps1. The two stale sentences: UNKNOWN.
- Class: noise
- Proof: No test applies. The diff is read and the probe's -ReflectionOnly output is compared.
- Branch: the run.ps1 and nw-guard.ps1 lines moved with F106 (3449521) and are given above.
- Note: Q93 words and polish.

### FR-122 f105-il-reader-prints-zero-for-kinds-never-attempted

The shared IL reader prints a zero for every failure kind in every probe, kinds the probe never
attempts included, and 5z-f reads those zeros as clean.

- Sources: F105-R1, F105-REV-6 (the F105 reviewer's finding of the same code fault)
- Evidence: On main since F105 merged as 2c89788: tools\probes\il-reader.ps1:71-84, the $IlFailureKinds table,
  and :104-112, IlFailureLines walks every key of it and prints 'label: count' even at zero.
- Root cause: tools\probes\il-reader.ps1:104-112, on main since F105 merged as 2c89788.
- Class: noise
- Proof: No Core test. Each probe run again on this machine prints only the kinds it attempts,
  and a reviewer reads the diff.
- Branch: F105 merged as 2c89788 on 2026-10-04, so this item can be worked.
- Note: Plan step 2 merges F105 first. FR-146 corrects the register wording of this row.

### FR-123 f105-failure-list-only-printed-with-built-addin

One of F105's probes prints its failure list only when a built add-in is there.

- Sources: F105-R2
- Evidence: On main since F105 merged as 2c89788: tools\probes\probe-viewpoint-calls.ps1:318-320 test whether
  $AddinPath is there, and the list is printed inside the else at :396-402 'IlFailureLines'.
- Root cause: tools\probes\probe-viewpoint-calls.ps1:318-411, on main since F105 merged as 2c89788.
- Class: noise
- Proof: No Core test. The probe is run with no built add-in and still prints the list.
- Branch: F105 merged as 2c89788 on 2026-10-04, so this item can be worked.

### FR-124 f105-probes-share-resolver-and-helpers-in-copies

A reflection resolver and two small helpers still sit in more than one of F105's probes instead
of one copy.

- Sources: F105-R3
- Evidence: On main since F105 merged as 2c89788: add_ReflectionOnlyAssemblyResolve at
  probe-viewpoint-calls.ps1:66, probe-roamer-switches.ps1:332 and probe-clash-report-api.ps1:52,
  and function ParamText at probe-viewpoint-calls.ps1:77 and probe-clash-report-api.ps1:53.
- Root cause: The lines in the evidence, on main since F105 merged as 2c89788.
- Class: noise
- Proof: No Core test. A grep for each name finds one definition, in tools\probes\il-reader.ps1.
- Branch: F105 merged as 2c89788 on 2026-10-04, so this item can be worked.
- Note: Which second helper is meant is not written in the row. ParamText is one that is
  certainly doubled. FR-146 corrects the register wording of this row.

## Docs and words, FR-125 to FR-149

### FR-128 apply-line-says-results-reset-contradicts-kept

With Apply the file's settings on, one run writes a drift block saying results are KEPT and then
an APPLIED line per test saying the edit reset them, and the window help under the box says it
resets clashes to New, so at least one statement is wrong.

- Sources: T1-S5
- Evidence: src\Federator.Addin\Engine\ClashRunner.cs:1106-1108 'the file's settings were put
  onto the test in the document, which reset its results', against :175 'IT DOES NOT RESET
  ANYTHING BY ITSELF' and :1069-1070 'It RESETS NOTHING by itself, measured 5y'.
  src\Federator.Core\Clash\TestDrift.cs:307 'APPLYING these to the tests in the document. Their
  recorded results and the statuses on them are KEPT'.
  src\Federator.Addin\Ui\FederatorWindow.xaml:566 'Resets those clashes to New. Differences are
  listed either way.' The same split is inside ApplyChosenTolerance: the comment at :1145-1146
  'because an edit resets its results' against the log line at :1199 'Its results and its
  statuses are KEPT, measured 5y'.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:1106-1108 (Apply, called at :985 when
  ApplyFileSettings is on).
- Class: noise
- Proof: A measurement first, then words. Run: on a copy of an NWF with Apply the file's settings
  ticked, against an XML that differs from a saved test holding results with statuses, read the
  result count and the status count before the edit, after it, and after a save and a reopen.
  Then every statement above agrees with what the run shows. After that a Core test can pin the
  one wording once the APPLIED sentence, the drift block line and the help lines live in Core
  beside TestDrift and ToleranceChoice. This run is not in the standard five of set 05.
- Note: PARTLY: the contradiction is real but the APPLIED line is not known to be false. The 5y
  measurement covered TestsReplaceWithCopy on a copy that already held the test's results with
  only the tolerance changed. Apply edits from a fresh ClashTest that holds no results through
  TestsEditTestFromCopy and changes type, merge and both sides as well. Whether that keeps the
  results and statuses is UNKNOWN and nothing in the repo measured it. No count, number or file
  is wrong because of this line, so the harm is words, though about whether review history was
  lost. If the run shows the edit does reset results, whether the box stays is a question for
  Bader, UNKNOWN until then. Not reached in set 03: 'apply file to old: no'.

### FR-133 savedviewpoints-canbuild-switch-nothing

SavedViewpoints.CanBuild always returns true and nothing reads it, yet three places call it the
one switch for viewpoints.

- Sources: T1-B2
- Evidence: src\Federator.Addin\Engine\SavedViewpoints.cs:59-62 'public static bool CanBuild'
  with 'get { return true }', and the doc at :54-57 'While this was false the engine asked for no
  viewpoints'. A grep of src finds only that and
  src\Federator.Addin\Engine\FederationEngine.cs:3179 'SavedViewpoints.CanBuild is the one
  switch, true since the viewpoints round.' BuildViewpoints gates only on 'if (outcome.Report ==
  null)' (:3183). .claude\rules\addin.md:261 and CLAUDE.md:190 say the same. A grep of tests finds
  none.
- Root cause: src\Federator.Addin\Engine\SavedViewpoints.cs:59-62, with the comment at
  src\Federator.Addin\Engine\FederationEngine.cs:3179.
- Class: noise
- Proof: None needed, no number or count is wrong, and no Core test applies because the member is
  in the add-in. The proof is the add-in build with 0 errors and a grep of src, tests,
  .claude\rules and CLAUDE.md for CanBuild finding nothing. The VIEWS lines of the set 05 first
  run read as before.
- Note: CLAUDE.md says a public member nothing in src calls is deleted with its tests unless a
  decision in steps\02_questions.md keeps it, and none does. The CLAUDE.md and addin.md edits are
  rule edits.

### FR-134 stale-units-comment-in-finish-the-group

A comment F30 moved says the units are set before the clash step so the report reads in its
units, which F26 made untrue because ReportUnits converts the finished report.

- Sources: NEW-LOG, F30 comment stale since F26 (steps\loop.md:611)
- Evidence: src\Federator.Addin\Engine\FederationEngine.cs:915-917 'BEFORE the clash step, so
  every tolerance and every distance is read in the units the report is going out in. Converting
  afterwards would mean a report whose numbers and whose unit label disagree.' steps\log.md:5811
  says it is stale since F26.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:915-917.
- Class: noise
- Proof: No test reads a comment. The comment is reworded to what F26 does, and a grep for the
  old sentence reads 0.
- Note: src\Federator.Addin\Engine\DocumentUnits.cs:12-18 'Converting the numbers ourselves is not
  an option' and Core ReportOptions.cs:57-64 carry the same old reasoning and are read with it.
  DocumentUnits.cs is also FR-062's file.

### FR-135 docs-bader-next-says-c06-never-existed

steps\03_bader_next.md says the C06 folder and the 1B06 group names were never on his machine,
and about twenty steps still name groups from the old C02 and C04 folders, while the run scanned
C06 with 67 NWC and 22 groups from 100000 to 1C06PK.

- Sources: step NEW, steps 9-17 and every step naming C0 (steps\loop.md:556)
- Evidence: steps\03_bader_next.md:16-18 'THE C06 FOLDER AND THE 1B06* GROUP NAMES ARE GONE FROM
  THIS FILE. They were never on his machine, and every count in here derived from fourteen
  groups was wrong for that reason.' against log:58 'groups built : 22', driver.txt:7 '67 NWC
  found, 67 readable' and log:74-95. steps\runs\03\findings.md:11 '17 DONE, 0 PARTIAL, 5 FAILED
  of 22' on C06, and :121 'Steps 9 to 17, which say C06 never existed, while C06 has 22 groups
  here'.
- Root cause: steps\03_bader_next.md:9-31, the header block written on 2026-09-20 about the C02
  and C04 folders, and the steps that name C02 and C04 groups. Steps 39, 95, 100, 101, 121, 123,
  136 and 281 still name the C06 folder.
- Class: noise
- Proof: No test. A read of steps\03_bader_next.md:9-31 after the fix: the block states the
  folders the loop actually runs, C06 with 22 groups (one reader adds C07 with 24 groups and no
  NWF, measured on this machine), says no folder never existed, and carries no count derived
  from the old 14 groups. The claim-checker reads it against log:58.
- Note: The file is rewritten and not appended when the proofs change (the steps rules). A Look
  for line is never changed to match wrong output, and only the false statement about the
  machine is meant here. Q94 made code run the whole of NM Fed itself, so most of this file is
  what the loop's run set now does, and the lead may rewrite or retire it. The NEW-LOG row that
  the proof steps of 03_bader_next never worked as a whole (steps\loop.md:613) is left out, its
  fault part being this item.

### FR-136 docs-step-377-log-bar-300-kb

Step 377 asks for a .log under 300 KB and the 22 group log is 1,097,850 bytes, about 50 KB a
group, so the step reads CONTRADICTED, and on what the bar was set is UNKNOWN.

- Sources: step 377 (F81, steps\loop.md:595)
- Evidence: steps\03_bader_next.md:824-827 'check the .log is under 300 KB and the .tsv still
  carries a row for every test' against log:8584 'COPY written ... 1,097,850 bytes' and
  record.txt:809 'the log 1098049 bytes'. The tsv half holds, 3215 'test created' and 3024 'rows
  for the workbook' rows. steps\runs\03\findings.md:119-120 lists F81 and step 377 as
  CONTRADICTED.
- Root cause: steps\03_bader_next.md:826, a bar whose basis is UNKNOWN, while CLAUDE.md says a
  number in a doc is measured, never estimated. Which blocks make most of the 1.1 MB is not
  measured. The trimming is
  src\Federator.Core\Diagnostics\RunLog.cs:644-720 (collapsedLines).
- Class: noise
- Proof: No test. After the choice, steps\03_bader_next.md:826 states the bar, and the set 05
  RESULT line 'COPY written ... bytes' meets it.
- Needed Bader, asked as Q108, answered on 2026-10-04, see the answered line of this item. Two ways out and the choice is his: trim the log further, or move the bar to
  a size per group, measured at 49.9 KB a group here. A Look for line is never changed to match
  wrong output (.claude\rules\loop.md, how a finding is worked, rule 6), so the bar moves only if
  he accepts the size.
- Note: The RESULT sizes printing 0 and 678,363 bytes are a separate fault, FR-046.
- Answered by Bader on 2026-10-04 at 15:23, Q108, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Measure which blocks and lines make the C06 log 1.1 MB, find repeated or wasted lines, and fix those. The tsv keeps every line in full, R6. Then set the size bar from the measured result. The lead's note: a noise item, so the fix is in wave 5.

### FR-139 rule-and-runner-still-say-source-copy

The loop rule says every run works on the copy under ...\source, the runner agent names
prepare-copy.ps1 -Remove or -Restore with no -Set NN, and run.ps1 Check reports only
source.manifest.txt and source.removed.txt. All three read wrong now that F108 and F106 are
merged and runs work on runs\NN\NMFed.

- Sources: F108-REV-1, F108-BRK-6, F108-R4 (the runner agent part)
- Evidence: .claude\rules\loop.md:73 'Every run works on the copy under
  %LOCALAPPDATA%\NwcFederatorLoop\source', .claude\agents\runner.md:21-22, and CheckMode in
  tools\loop\run.ps1 (:847-848 at f38edd5), which on main 3449521 still prints '----
  source.manifest.txt and source.removed.txt ----' (CheckMode at :1296).
- Root cause: .claude\rules\loop.md:73, .claude\agents\runner.md:21-22 and CheckMode in
  tools\loop\run.ps1.
- Class: noise
- Proof: No Core test applies. A read of the three, and a prove-run.ps1 Check case that lists
  NMFed.manifest.txt and NMFed.removed.txt.
- Closed in part: rests on F108 (c9b223b) and F106 (3449521). PR 83, merged as 086a348 on
  2026-10-04, rewrote the rule line to name runs\NN\NMFed, and its change to runner.md touched
  only the hang rule line, so the runner part and the Check part stay.
- Note: A runner that follows the old words would take a file out of source with the plain
  -Remove, which makes -Set refuse. CLAUDE.md step 3 of how a fix is worked asks the rule to
  change with the code.

### FR-140 f103w-prove-run-header-words

Three sentences in prove-run.ps1 say more than the code. The header says H17's copy of run.ps1
has its constructor line replaced by a line that throws, where the line only sets the error. It
says check 6 and check 7 would each refuse an Install call, where the second guard of Install is
the HEAD check. A check label says the put back's own writes are never listed as the start's,
where M5 lists what any program wrote.

- Sources: F103-W-1, F103-W-3, F103-W-8, F103-W
- Evidence: tools\loop\prove-run.ps1:23-25 'whose constructor line is replaced by a line that
  throws' against :1167, which sets $err to a new System.Exception reading 'HARNESS COPY: the
  constructor line is removed from this copy, and this line was reached'. Lines 19-22 'so check 6
  and check 7 would each refuse it' against run.ps1:894-896 at f38edd5 (check 6 then
  TreeRefusal). Line 284 'never listed as the start's'. The sentence at 23-25 survived the
  readings of fix attempts 1, 2 and 3.
- Root cause: tools\loop\prove-run.ps1:19-25, :284 and :1167, unchanged on main 3449521.
- Class: noise
- Proof: No Core test applies. A read of the three places after the change, and prove-run.ps1 run
  again on the changed file with the same pass count (242 passed on a63c284).
- Note: F103-W is at steps\loop.md:432. Words only, no change of logic. FR-092 changes the same
  file and goes first or with it.

### FR-141 f103w-readme-run10-exit-codes-wrap

tools\loop\README.md lists the harness runs after fix attempts 1 and 3 and has no entry for run
10 after fix attempt 2. It says RunVerdict and InstallVerdict decide the exit codes, where
run.ps1's main flow sets codes 2, 1 and 3 itself. Three lines are over long.

- Sources: F103-W-2, F103-W-4, F103-W-wrap, F103-W
- Evidence: At f38edd5, tools\loop\README.md:315-319 (no run 10), :209 'RunVerdict and
  InstallVerdict decide them', and run.ps1:934, :936 and :966, where the main flow sets $code to
  2 and 1. Over long lines at README.md:186, :218 and :319. F108 changed the README, and on main
  3449521 the sentence is at :263. The main flow of F106's run.ps1 still sets $code itself (for
  example run.ps1:1384, :1397 and :1499 on main).
- Root cause: tools\loop\README.md (:186, :209, :218 and :315-319 at f38edd5).
- Class: noise
- Proof: No Core test applies. A read of the README after the change: an entry for run 10, a
  sentence that names the exit codes the main flow sets, and the three lines wrapped with no word
  changed.
- Note: The wrap was noticed by the claim-checker, not by the readers (F103-W-wrap).

### FR-142 f103w-probe-and-run-comment-words

The probe's header says it reads no window of any other process, where the guard enumerates
every top level window and reads each one's process id and an owner that may belong to another
process. A comment in run.ps1 ends on a comma.

- Sources: F103-W-7, F103-W-9, F103-W
- Evidence: tools\probes\probe-automation-start.ps1:96 'It reads no window of any other process'
  against tools\loop\nw-guard.ps1:363-367 and :414-427 (EnumWindows over every window, the owner
  read). The comment '# Keep awake, the session lock and what changed outside the loop folder
  while a start ran,' at tools\loop\run.ps1:380 at f38edd5, :612 on main 3449521.
- Root cause: tools\probes\probe-automation-start.ps1:96 and the comment in tools\loop\run.ps1,
  now at :612.
- Class: noise
- Proof: No Core test applies. A read of the two lines after the change. run.ps1 changes only a
  comment, so the harness shows the diff and nothing else.
- Note: The probe is F100's measurement, so the change keeps to the words. FR-143 is the same
  claim in F106's driver.

### FR-143 f106-driver-header-no-desktop-search-claim

The driver's header says it never searches the desktop, while the window reads it relies on
enumerate every top level window with EnumWindows and keep those of the adopted pid. Only each
window's pid is read, so this is wording and not a search.

- Sources: F106-HARM-REV-13
- Evidence: tools\probes\drive-window-run.ps1:29 'It never searches the desktop' against
  tools\loop\nw-guard.ps1:414-427 (WinHandlesOf over EnumWindows).
- Root cause: tools\probes\drive-window-run.ps1:29.
- Class: noise
- Proof: No Core test applies. A read of the header after the change.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: The finish brief's 'no desktop walk' holds only in that sense.

### FR-144 junction-recursion-claim-contradicted

A comment and the README say Windows PowerShell 5.1 follows a junction when it recurses, and the
F108 developer's scratch test on 5.1.26100.9444 says it did not. Which is right is UNKNOWN. If
the old claim is right, Find-One and the recursive deletes pass a junction planted inside a copy.

- Sources: F108-REV-2, F108-BRK-7, F108-R4
- Evidence: At f38edd5, tools\loop\prepare-copy.ps1:79-80 'because Windows PowerShell 5.1 follows
  a junction when it recurses' and tools\loop\README.md:30-31, with Get-ChildItem -Recurse at
  :110 and Remove-Item -Recurse at :222. On main 3449521 the comment is at
  prepare-copy.ps1:123-124 and the README line at :45. The scratch test is kept in no file.
- Root cause: the comment in tools\loop\prepare-copy.ps1 and the README line, with the recursion
  in Find-One and the deletes (:122-127, :183, :315 and :321 in F108's version).
- Class: noise
- Proof: No Core test applies. A measurement in docs\history\scan.md with its date: a scratch
  junction, Get-ChildItem -Recurse and Remove-Item -Recurse on Windows PowerShell 5.1. Then the
  words follow it, or Find-One refuses a link inside the copy if 5.1 does follow one.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Nothing in the loop plants a junction in a copy today, and the guard at
  prepare-copy.ps1:105-115 in F108's version refuses one at the folders above the copy.

### FR-145 readme-write-list-names-only-source-copy

The README's line of what prepare-copy.ps1 writes outside the repo names only the source copy and
source.manifest.txt. runs\NN\NMFed, NMFed.manifest.txt and NMFed.removed.txt appear only in the
-Set bullets further down.

- Sources: F108-REV-5, F108-R4
- Evidence: tools\loop\README.md:18-22 against :30-44, read the same on main 3449521. The lead's
  PR body for F108 (turn5\pr-f108.md:12-13) calls it register row F108-R4.
- Root cause: tools\loop\README.md:18-22.
- Class: noise
- Proof: No Core test applies. A read of the README line after the change.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Words only. CLAUDE.md says the tools\loop README says what each script writes outside the
  repo.

### FR-146 f105-register-wording-attempts-and-helpers

Register row F105-R1 and Q89 say the probes never attempt some failure kinds, and F105-R3 says
two small helpers sit in more than one probe. In plain words the probes do read those kinds and
only never count a failure of them, and by function name one helper plus the resolver repeats,
the second helper being UNKNOWN.

- Sources: F105-BRK-2, F105-BRK-5, F105-REV-5, F105-REV-1 (its part on main)
- Evidence: steps\loop.md:433 'kinds a probe never attempts included' and :435 'A resolver and
  two small helpers', and steps\02_questions.md:468 and :470 'kinds never attempted', all read
  the same on main 3449521. On main since F105 merged as 2c89788, probe-roamer-switches.ps1:595 reads constant values and
  probe-clash-report-api.ps1:73 reads field types with no count, ParamText is in
  probe-clash-report-api.ps1:53 and probe-viewpoint-calls.ps1:77, and $roResolve is in all three
  and in probe-automation-start.ps1:558.
- Root cause: steps\loop.md:433 and :435 and steps\02_questions.md:468 and :470. The same words
  in docs\history\scan.md 5z-f were narrowed by F105, merged as 2c89788.
- Class: noise
- Proof: No Core test applies. A read of the two register rows after the change, and a
  correction line under Q89 that does not alter his answer.
- Closed in part: F105, merged as 2c89788 on 2026-10-04, changed rows F105-R1 and F105-R3 to
  the narrowed words and added F105-R4. What stays is the correction line under Q89.
- Note: steps\loop.md is the lead's to write. The branch part of F105-REV-1 and F105-BRK-2 was
  fixed in F105, merged as 2c89788, and this is the part on main. F105-REV-1 also said the lead may want Bader
  told Q89 was partly wrong.

### FR-147 restated-facts-check-missing

The check that refuses a broken restatement of the four named facts does not exist, and the
facts have drifted three times in prose.

- Sources: F96 (steps\loop.md:513)
- Evidence: steps\01_next.md:46 'F96, the restated facts check, FIRST ITEM OF THE NEXT ROUND' and
  :925 'Files a new tools/checks/check-restated-facts.sh'. tools\checks holds check-imports.sh,
  check-locals.sh and check-evidence-ids.sh and no check-restated-facts.sh.
- Root cause: tools\checks\check-restated-facts.sh does not exist, and steps\01_next.md:925
  specifies it.
- Class: noise
- Proof: No Core test. tools\checks\check-restated-facts.sh run by Actions over the tree, and
  over a broken fixture under tools\checks\broken for each of the four facts (the solid category
  list, the 150 mm threshold, the tick box count, the event kinds), where it refuses.
- Note: The shape to copy is
  PenetrationRuleTests.TheHelpLineNamesEverySolidTheRuleCoversAndStaysWithinTwelveWords, which
  iterates the code value and asserts the restatement names every entry. steps\01_next.md gained
  17 lines since f38edd5, so its line numbers are read again.

### FR-148 audit-180-findings-without-verifier

180 of the 393 findings of the 2026-09-12 audit were never read by a second reader, because 24
of 54 verifier batches died on a usage limit.

- Sources: NEW-LOG, 180 findings of the 2026-09-12 audit carry no verifier (steps\loop.md:612)
- Evidence: steps\04_audit.md:171 'the other 24 batches died on the session's usage limit before
  they ran, so 180 findings ... carry a reader's evidence and my grep and no verifier'.
  steps\log.md:5556 says the same and that no journal is kept in the repo.
- Root cause: UNKNOWN as a code location. It is a reading backlog in steps\04_audit.md:171.
- Class: noise
- Proof: No Core test. Each of the 180 is checked again by grep and marked verified or refuted in
  steps\04_audit.md.
- Note: A backlog kept as an item. It is a reading job and not a code fault, and most of that
  audit became F39 to F46 and later rounds. The lead may choose to skip it, since the turn 1 read
  of the whole repo on 2026-09-27 read the code again.

### FR-149 older-machine-account-name-left-in-tree

F107 masks the older machine's name, which Q88 asked for, and leaves behind the account name of
that machine's user, in C:\Users\<name> paths in committed probe results, docs\history\scan.md
and steps\logs.

- Sources: F107-BRK-6
- Evidence: docs\history\scan.md:3465 holds a
  C:\Users\<name>\AppData\Local\Temp\claude\round-viewpoints\probe path. A read finds that
  account's C:\Users path in 37 files outside the worktrees, among them INSTALL.md:8,
  tools\probes\fixture.md and 17 files under steps\logs, which the paths wall keeps unedited.
- Root cause: docs\history\scan.md:3465, INSTALL.md:8, tools\probes\ViewpointProbe\*-result-*.txt
  and steps\logs\*.log on main.
- Class: noise
- Proof: No Core test applies. A grep of the tree for the account name over the files that may be
  edited, the 20 of the 37 outside steps\logs before and none after, as Bader's answer to Q107 asks.
- Needed Bader, asked as Q107, answered on 2026-10-04, see the answered line of this item. Whether the account name of the older machine counts as a trace he wants
  masked is his choice, Q88 named only the machine name. steps\logs cannot be edited.
- Branch: works after F107's merge, which masks the machine name in the same files, fix-F107 not
  merged at main 3449521.
- Note: The account name is written <name> here, so the draft adds no copy of it.
- Answered by Bader on 2026-10-04 at 15:23, Q107, in short, his words being under the question in steps\02_questions.md: mask the older account name in the 20 files outside steps\logs, the check refuses it, held as a hash, and steps\logs stays untouched. The lead's note: to be worked by F117, the names, in wave 5 with the noise.

## Rest, FR-150 to FR-174

### FR-156 probe-csv-listed-written-after-write-threw

When File.WriteAllText throws for the probe CSV, the old CSV at that path, for example last
week's held open in Excel, is still logged as written with its size and named in the verdict
block as written.

- Sources: T1-S13
- Evidence: src\Federator.Addin\Engine\PropertyProbe.cs:114 'File.WriteAllText(csv,
  ProbeCsv.Text(tally.Rows()), Encoding.UTF8)' sits in a try whose catch at :116-122 logs a
  failure and says 'kept going', then :124 log.WriteFinished("CSV", csv) runs unconditionally and
  adds the path to the written list. src\Federator.Core\Probe\ProbeVerdict.cs:186 adds 'written
  to           : ' plus the path. RunLog.CheckOnDisk (RunLog.cs:1497) exists for a file that was
  checked rather than written.
- Root cause: src\Federator.Addin\Engine\PropertyProbe.cs:112-124.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Probe\PropertyProbeTests.cs: move the write into
  Core as a ProbeCsv write that returns whether it wrote, write into a folder under a file
  (which no system makes) and assert the path is not in the written list. It fails today because
  the write and the record are separate and the record runs unconditionally. Otherwise a run
  with the CSV open in Excel proves it.
- Note: The trigger is ordinary: a person opens last week's CSV in Excel, Excel locks it, and the
  next probe of that model throws on the write. Both callers reach it, FederationEngine.cs:2254
  and :2314. Not reached in set 03: only Scan and Run were pressed and the log has no PROBE line.
  CLAUDE.md says a file this tool did not write is never listed as written.

### FR-157 probe-unread-value-written-as-empty-value

A property value that will not read is tallied as an empty value, so the probe CSV cannot tell a
value that threw from one that is empty in the model.

- Sources: T1-S14
- Evidence: src\Federator.Addin\Engine\PropertyProbe.cs:205-209, a catch whose comment says 'Its
  own try, so one value that will not read costs one cell.' sets the value to string.Empty, and
  :211 tally.Add(category, tabName, Words.Or(property.DisplayName, string.Empty), value).
  src\Federator.Addin\Engine\ClashHarvest.cs:606-607 also maps VariantDataType.None to
  string.Empty.
- Root cause: src\Federator.Addin\Engine\PropertyProbe.cs:198-211.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Probe\PropertyProbeTests.cs: a marker for an
  unread value beside ProbeTally.CappedMarker, and a test that adds one unread and one empty
  value for the same property and asserts two different rows. It fails today because both are
  the empty string. The add-in side then passes the marker from the catch.
- Note: The CSV exists to show what the model really holds. How often ClashHarvest.Text throws in
  practice is UNKNOWN, since every known kind has its accessor and the default uses ToString. A
  throw, a None kind and a real empty display string all land in one row. Not reached in set 03:
  no probe ran.

### FR-161 date-format-has-no-control

NamePattern.DateFormat is a setting nothing in the window sets, so a dated NWD is always
yyyyMMdd and the fallbacks in NumberOrDate cannot be reached by a person, although core.md calls
the format the person's setting whose mistakes should be visible in the preview.

- Sources: T1-B6
- Evidence: A grep of src for DateFormat finds only src\Federator.Core\Naming\NamePattern.cs:33,
  :42, :69 (the public DateFormat property) and :139.
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:543-547 set Level, Discipline, TypeCode, Number
  and AllBuildings only, and the XAML has no box. Set 03: log:66 'NWD naming : overwrites', and
  the NWD names carry a number and no date.
- Root cause: src\Federator.Core\Naming\NamePattern.cs:69, never set by
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:535-548.
- Class: broken feature
- Proof: Run. A box on the Outputs step whose value reaches the preview and the NWD name with the
  dated NWD tick on. The Core half is already tested (OutputNameTableTests.TheDateFormatIsASetting
  at :264). Not in the five run plan, so a driver step is needed, UNKNOWN.
- Needed Bader, asked as Q109, answered on 2026-10-04, see the answered line of this item. The separator and the part positions are in the same state
  (steps\notes\f101-design.md:42 records that the window does not offer them). So the choice is a
  box on the Outputs step, or core.md reworded to say it is a code setting, and that is his
  because the window is meant to carry fewer decisions.
- Answered by Bader on 2026-10-04 at 15:23, Q109, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Test that each setting, the date format, the separator and the part positions, changes the NWD name the way it should, that a wrong value is refused with a clear line, and find where it breaks. The lead's note: in wave 4 with the rest.

### FR-170 penetration-size-catch-swallows-unit-throw

A catch in the penetration size reader swallows a unit or property throw and its comment gives
only the F72 reason, though for the viewpoint caller the same null files the clash in its pair
folder, and it logs nothing.

- Sources: T1-S12
- Evidence: src\Federator.Addin\Engine\Penetrations.cs:335 'return
  SizeRule.LargestMillimetres(read, unitEnumName, sizes)', then :338-347, a catch whose comment
  says 'A unit the table does not know throws out of SizeRule, and so does a property that will
  not read. Either way no size could be read, which the rule answers by LEAVING THE CLASH ALONE
  ...', and :349 'return null'. src\Federator.Core\Units\UnitTable.cs:198-202 ByEnumName throws
  NotSupportedException for an unknown name.
- Root cause: src\Federator.Addin\Engine\Penetrations.cs:338-347 (LargestOf), reached by F72
  through WantedFor and by F85 through ServiceSizeOf from
  src\Federator.Addin\Engine\ViewpointBuilder.cs:393, with the unit read at
  Penetrations.cs:405-408.
- Class: noise
- Proof: No Core test fails today, because the unknown unit branch cannot be reached on 2025: the
  unit is document.Units.ToString() at Penetrations.cs:407, the enum has eleven members
  (docs\history\scan.md:526-528) and UnitTable.cs:62-77 carries all eleven. The guard is a Core
  test in tests\Federator.Core.Tests\Units\UnitTableTests.cs asserting each of the eleven names
  resolves through UnitTable.ByEnumName, which passes today and fails when a row is dropped
  (UnitTableTests.cs asserts only UnitTable.All.Count above 0 now). For a swallowed property read
  throw at ItemSizes.cs:48 the proof is UNKNOWN.
- Note: PARTLY. Real code but not the harm the finding named: no service is filed as size unknown
  for the unit reason today, and what is left is a comment and a catch that logs nothing. A throw
  at ItemSizes.cs:48 on the leaf ends the whole LargestOf loop so the ancestors are never read,
  and whether PropertyCategories on a ClashResult.Item1 item ever throws is UNKNOWN. Set 03:
  'size could not be read : 0' in all 22 groups on the viewpoint path. Penetrations.cs is also
  FR-072's file.

### FR-171 t1-noise-findings-93

93 noise findings from the turn 1 read, each one reader's claim at 42499bf and none read again
since.

- Sources: T1-N1 to T1-N93 (steps\loop.md:700)
- Evidence: steps\loop-read.md:110 '### noise, 93', entries at :112-204, counted 93 by grep of the
  T1-N tags. steps\loop.md:864-867 says the second reading of 2026-09-29 covered only the 86
  silent, broken and loud faults.
- Root cause: UNKNOWN as one place. Each entry carries its own file and line at 42499bf, none
  checked on main.
- Class: noise
- Proof: None as a group. Each entry that is taken gets its own Core test or a grep.
- Note: A backlog kept as an item, not opened one by one. Some entries are already other items
  here, for example T1-N82 is the same shape as FR-042, the page check order warning.

### FR-172 t1-uncalled-members-150

150 public members nothing in src calls, as counted at 42499bf.

- Sources: T1-UNCALLED (steps\loop.md:701)
- Evidence: steps\loop-read.md:255 '## 3. Members nothing in src calls, 150', entries at
  :260-409, counted 150 lines. steps\loop.md:701 'each deleted with its tests, or kept by a
  decision in 02_questions'.
- Root cause: UNKNOWN as one place. Each entry has its file and line at 42499bf, not counted
  again today.
- Class: noise
- Proof: None as a group. For each member a grep over src reading 0 and the member deleted with
  its tests, or a decision in steps\02_questions.md that keeps it.
- Needed Bader, asked as Q26 on 2026-09-12 and answered on 2026-10-04, see the answered line of this item. Q26 asked whether the ones read only by a test go internal
  instead, which decides about 30 of them.
- Note: A backlog kept as an item. CLAUDE.md deletes a member nothing in src calls unless a
  decision keeps it. SizeTally and MatrixCorrections, which are in this list, are separate items
  here (FR-068 and FR-030). Other uncalled members are separate items too:
  SavedViewpoints.CanBuild (FR-133), FolderMemory.DisabledReason (FR-162) and
  OutputNameTable.ReleaseToPattern (FR-160).
- Answered by Bader on 2026-10-04 at 15:23, Q26, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Break each member on purpose and check its test fails. A test that still passes proves nothing, and that member goes in the form. The lead's note: in wave 5.

### FR-173 t1-catch-swallowing-77

77 catches a reader called swallowing, as counted at 42499bf, some of which are the rule that
logging never stops a run.

- Sources: T1-CATCH (steps\loop.md:702)
- Evidence: steps\loop-read.md:411 '## 4. Catches a reader called swallowing, 77', entries at
  :416-492, counted 77 lines. :413 'Logging never stops a run, so some of these are the rule and
  not a fault.'
- Root cause: UNKNOWN as one place. Each entry has its file and line at 42499bf, not read again
  on main.
- Class: noise
- Proof: None as a group. Each catch is sorted into the logging rule or a fault, and a fault gets
  its own Core test.
- Note: A backlog kept as an item. Some of the faults inside it are already other items here, for
  example SetBuilder.cs:284 and :320 are FR-013.

### FR-174 one-public-type-per-file

46 files under src hold more than one top level type, and the one public type per file rule that
Bader answered in Q80 is written in no rule file.

- Sources: D1 (steps\loop.md:437), A23 (steps\loop.md:553), Q98 B5
- Evidence: steps\loop-read.md:206 '## 2. D1, files holding more than one top level type, 46',
  listed at :208-253. The register reader's text count over src of namespace level type
  declarations also reads 46 files, for example Exchange\MatrixCorrections.cs and
  ExchangeModel.cs with 8 each, and Clash\PenetrationRule.cs, Views\ClashViewpointPlan.cs and
  Report\ClashReportModel.cs with 5. A grep for 'one public type' in .claude\rules and CLAUDE.md
  finds nothing. steps\04_audit_first_run.md:218, A23 names 13 files, and 11 of them still hold
  more than one type (Csv left PriorityMap.cs and CensusMove.cs holds one).
- Root cause: The 46 files in steps\loop-read.md:208-253, counted again on main. The rule text is
  missing from .claude\rules\core.md and addin.md.
- Class: noise
- Proof: A new Core test, tests\Federator.Core.Tests\SourceLayoutTests.cs, that reads
  src\**\*.cs and fails today on 46 files, and passes when every file holds one top level type.
  Plus the rule written into core.md and addin.md.
- Note: Answered in Q80 D1. Q98 B5 makes it the last pull request of the round, moves only and no
  logic. Splitting changes no behaviour, so the Core tests and the add-in build read the same
  counts before and after. It touches the files of every product area, so nothing else is open
  when it runs.

## Bader's five requests, FR-175 to FR-179

Added by Bader's message of the evening of 2026-10-04 headed FIVE REQUESTS ADDED TO THE ROUND,
in his words under Q112 of steps\02_questions.md. They are requests, not faults found, so they
sit outside the counts of the table below. After each one, three lines in the Claude tab: what
was done, what the test showed, and anything for Bader.

## Bader's views by team, FR-180 to FR-188

Added by Bader's message of the evening of 2026-10-04 headed ONE VIEWPOINT PER CLASH TEST, IN THE
A, B, C FOLDERS, BY TEAM, NO TEAMS MIXED, NO MIRRORED TESTS, in his words under Q114 of
steps\02_questions.md. Decisions, not faults found, so they sit outside the counts of the table
below. His points 3 and 5 ask for a measurement first, and his proof, points 18 and 19, is on
1A02MM. The lead's own rule adds that each item is measured before it is written and test first.

### FR-181 mechanical-sets-miss-hv-pl-fp-models

- Sources: Q114, point 3. Area F131, wave 2b
- What he asked: measure on 1A02MM and 1A04PK and on any model named with HV, PL or FP which sets of
  the picked XML find their items. If a mechanical set asks for ME in the file name or an ME
  workset, and so finds nothing in an HV, PL or FP model, that is a silent miss, said in the
  COVERAGE block and in the form with a correction for Bader to approve, built the way the
  both-spellings rows are
- Measured on 2026-10-04, read only, turn5\measure-teams.md: of the 154 models of the copy only
  1A04PK's FP and HV models carry HV, PL or FP, no model is coded PL, and 1A02MM has none, so it
  cannot have such a miss. None of the 25 mechanical sets asks Source File, each asks one
  workset value, so a set finds items in an HV or FP model only where that model carries the
  exact value. Whether 1A04PK's HV and FP models do is UNKNOWN until a run or a probe reads their
  worksets. Silent misses confirmed: none. A correction built the way the Q102 rows are is drafted
  in its section 6 and not applied
- Class: Bader's decision, a silent miss if the measurement finds one
- Designed on 2026-10-05, turn5\q114-design.md section 1.3. Probe P4 reads the worksets of 1A04PK's HV and FP models

## Bader's tracker and code health gate, FR-191 to FR-193

Added by Bader's message of the morning of 2026-10-05 headed A CLEAN TRACKER AND A CODE HEALTH GATE,
IN PARALLEL WITH THE FIXES, in his words under Q129 of steps\02_questions.md. Decisions, not faults
found, so they sit outside the counts of the table below. A lane of its own beside the waves, which
never holds back the viewpoints.

### FR-192 code-health-gate-lists-only-shrink

- Sources: Q129, part 2. Area F134, beside the waves
- What he decided: nine counts measured first and written as the starting line of
  steps\code-health.md: members with no caller in src, private members, locals, parameters and
  usings nothing uses from the compiler's own analysers turned on in the build, catches that swallow an error, files holding
  more than one public type, files over 1,000 lines, doubled or orphan summary blocks, window
  controls with no handler and handlers with no control, settings nothing reads, and test files with
  no source file and source files with no test file. Each gets a check in Actions on every pull
  request with today's offenders in a baseline list per check. A pull request that adds a new offender
  fails, one that removes one takes it off its list, and the lists only shrink. steps\code-health.md
  gets one row per merge with every count. Only what this machine and nuget.org give is used
- Measure first: his words, that each check finds a known offender before it is trusted. The lead adds
  that each count is measured on main 6fff40a in %LOCALAPPDATA%\NwcFederatorLoop\health and checked by
  a second reader that plants a known offender, before its check is written. Actions has no
  Navisworks, so what each check misses for the add-in is said
- Proof: each check refuses a fixture holding one new offender, and a pull request that removes one
  takes it off its list
- Class: Bader's decision

### FR-193 analyser-settings-merged-alone

- Sources: Q129, parts 2 and 3. Area F135, beside the waves
- What he decided: the compiler's analysers turned on in the build of every project, merged alone at
  a quiet moment so no fix branch is left on red
- Proof, the lead's: the full solution built on main and on each open fix branch merged with it, each
  with its warning and error counts
- Class: Bader's decision

## Bader's stop of the C02 weekly, FR-194

Added by Bader's message of 2026-10-05 headed STOP THE C02 WEEKLY RUN NOW, in his words under Q130 of
steps\02_questions.md. A decision, outside the counts of the table below.

## Bader's answers of 2026-10-05, FR-195

Added by Bader's answers to Q111 and Q125, in his words under them in steps\02_questions.md. Decisions,
outside the counts of the table below.

### FR-195 no-site-and-no-clash-groups-end-partial

- Sources: Q111 B and Q125 B. Area F137, before the test of wave 1
- What he decided: a model naming no site is not on the same coordinates, so its group skips its clash,
  ends PARTIAL, and its line in the note and the list says no site is named. A group that runs no clash
  test with a model off the shared coordinates, more than 1 m away or on Internal, for both, ends
  PARTIAL with the reason models not on the same shared coordinates, though no clash was skipped
- Proof: Core tests of both rules that fail first, and the test of wave 1
- Class: Bader's decision

## Bader's autosave copies, FR-196

Added by Bader's message of 2026-10-05 headed TOO MANY AUTOSAVE COPIES, in his words under Q135 of
steps\02_questions.md. A decision, outside the counts of the table below.

### FR-196 loop-runs-write-no-autosave-and-copy-his-folder-once

- Sources: Q135. Area F138, before any probe or run starts again
- What he decided: for every loop run Auto-Save is switched off, or its folder pointed at the run's own
  work folder, whichever the measurement shows is safe, inside the settings the run backs up and puts back,
  so a loop run never writes an autosave into his AutoSave folder. His AutoSave folder is copied once,
  the one backup kept, and each run compares names, sizes and times before and after, putting back from
  that backup what changed and saying so in one line
- Measured on 2026-10-05, turn5\q135\measure.md: the switch is enable under GlobalOptions\general\autosave
  of the 22.0 key, off the safer choice, written "3 0" by the form of the key's other booleans
- Part 1, in tools\loop\nw-guard.ps1, switches it off for every start, run and probe. Part 2, in run.ps1
  after F131 merges, makes the per-run copy a compare against the one backup
- Proof: the proof harness for the write and its read back, and the first real start writing no file into
  his AutoSave folder, by name, size and time
- Class: Bader's decision

## Bader's progress page, FR-197

Added by Bader's message of 2026-10-06 headed A ONE PAGE PROGRESS FILE THAT KEEPS ITSELF CURRENT, in his
words under Q139 of steps\02_questions.md. A decision, outside the counts of the table below.

## Bader's answers of 2026-10-06, FR-198 to FR-204

Added by Bader's message of 2026-10-06 headed BADER'S ANSWERS, 6 OCT 2026, THE 22 OLD ITEMS AND F132'S
NEXT READING, in his words under each question in steps\02_questions.md. Decisions, outside the counts of
the table below. His answers do not change the order of work, so every item here comes after the
viewpoints, F138, F131, F132 and F114, then main installed and the runs of 1A02MM and 1A04PK, each in the
wave the lead judged. Each was measured off main 2eda020 before it was written, and where main already
does what he says the item says so and plans no work. Q29, Q31, Q47, Q50 and Q51 close with no work and
have no item, Q77 is a line under FR-067, and F18 is dropped in steps\01_next.md.

### FR-202 sub-object-ownership-measured-before-two-reads-change

- Sources: Q28. Area F121, wave 4, after the viewpoints
- What he decided: the ownership of a sub object measured off the installed DLL first, both reads
  left as they are until then, then what the measurement shows done
- Measured on 2026-10-06 off main 2eda020: both reads stand. search.Selection is read off a Search
  this tool owns to call SelectAll and is not disposed, src\Federator.Addin\Engine\SetBuilder.cs
  lines 144 and 730. ClashRunner.LocatorOf disposes the side's Selection,
  src\Federator.Addin\Engine\ClashRunner.cs line 1049, and reads each source out of its collection by
  index inside the loop over the indexed sets, lines 1058 to 1062, none disposed. FR-029 holds the
  same question for side.Selection and its sources in SetBuilder. What is measured is
  docs\history\scan.md section 4g, an item read out of a document collection being a borrowed view
  whose release frees the wrapper alone, read in the IL of GroupItem.GetChild
- Measure first: the ownership each getter gives, read in the IL of Search.get_Selection and of
  SelectionSourceCollection's indexer off the installed Autodesk.Navisworks.Api.dll with
  tools\probes\il-reader.ps1, the way section 4g read GroupItem.GetChild, written into
  docs\history\scan.md. UNKNOWN until read
- What will be built: what the measurement shows. Where a release frees the wrapper alone, both reads
  are released the way every other handle this tool creates is, and FR-029's sub objects with them.
  Where it would free an object the parent still uses, both stay, a line at each saying why
- Proof: the IL read itself, and since no Core test reaches either read, review and the test of wave
  4, its log with no ObjectDisposedException
- Class: Bader's decision

## The areas at a glance

| Area | Items | Count | Silent wrong number | Broken feature | Slow | Loud failure | Noise | Needs Bader |
|---|---|---|---|---|---|---|---|---|
| Alignment | FR-001 to FR-007 | 7 | 5 | 0 | 0 | 1 | 1 | 1 |
| Sets | FR-008 to FR-030 | 23 | 19 | 1 | 0 | 0 | 3 | 3 |
| Clash counts | FR-031 to FR-034 | 4 | 3 | 0 | 0 | 1 | 0 | 0 |
| Workbook | FR-035 to FR-039 | 5 | 3 | 0 | 0 | 0 | 2 | 0 |
| Report | FR-040 to FR-042 | 3 | 1 | 1 | 0 | 0 | 1 | 0 |
| Run log and RESULT | FR-043 to FR-064 | 22 | 11 | 3 | 0 | 1 | 7 | 0 |
| Views | FR-065 to FR-074 | 10 | 2 | 2 | 3 | 1 | 2 | 1 |
| Harvest and pictures | FR-075 to FR-077 | 3 | 1 | 1 | 1 | 0 | 0 | 0 |
| Install | FR-078 to FR-081 | 4 | 0 | 3 | 0 | 0 | 1 | 0 |
| Loop tools | FR-082 to FR-124 | 43 | 3 | 9 | 3 | 8 | 20 | 2 |
| Docs and words | FR-125 to FR-149 | 25 | 0 | 0 | 0 | 1 | 24 | 2 |
| Rest | FR-150 to FR-174 | 25 | 9 | 5 | 0 | 3 | 8 | 3 |
| All | FR-001 to FR-174 | 174 | 57 | 25 | 7 | 16 | 69 | 12 |

Needed Bader, all answered on 2026-10-04: FR-006, FR-008, FR-009, FR-030, FR-070, FR-109, FR-110, FR-136, FR-149, FR-160,
FR-161 and FR-172.

Waiting for a merge not yet made: F107 for FR-110 and FR-149, and F109 for FR-078 to FR-081.
F105 merged as 2c89788, so FR-122, FR-123, FR-124 and FR-146 no longer wait. PR 83, merged as
086a348, closed FR-138, and with F106 FR-137, and the rule half of FR-139.

Resting on code merged on 2026-10-04: F106 (3449521) under FR-082, FR-083, FR-085, FR-092,
FR-094 to FR-098, FR-109, FR-111 to FR-116, FR-138, FR-139 and FR-143. F108 (c9b223b) under
FR-082, FR-084, FR-093, FR-101 to FR-104, FR-118, FR-139, FR-144 and FR-145. Lines that moved
with F106 and are given on main in the item: FR-079, FR-080, FR-086 to FR-091, FR-099, FR-105
to FR-108, FR-117, FR-118 and FR-121.

## The files each area changes

| Area | Files |
|---|---|
| Alignment | src\Federator.Core\Health\AlignmentCheck.cs, ExportCheck.cs, InvisibleDifference.cs, src\Federator.Core\Rerun\GroupJudgement.cs, src\Federator.Addin\Engine\FederationEngine.cs (2090 to 2152), ModelFactsReader.cs, and their tests under Health and Rerun |
| Sets | src\Federator.Addin\Engine\SetBuilder.cs, FederationEngine.cs (2389, 2456, 2626 to 2636), src\Federator.Core\Sets\ (SetDrift, SetLeftovers, SetBuildOutcome, SetsAcrossTheRun, EmptySets, SetResult, SetBuildPlan), src\Federator.Core\Health\ (SetWarnings, HealthCheckResult, HealthCheck, ExportCheck, WorksetDisagreements), src\Federator.Core\Exchange\ (RevitWorksets, MatrixCorrections), exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml only by Bader's choice, and the tests under Sets, Health and Exchange |
| Clash counts | src\Federator.Addin\Engine\ClashRunner.cs (572 to 579), ClashHarvest.cs (164), ByDesign.cs, src\Federator.Core\Clash\ (ClashRunOutcome, ByDesignRule, ByDesignTally, AutoReviewRecord), src\Federator.Core\Report\ClashReportModel.cs, and the tests under Clash and Report |
| Workbook | src\Federator.Core\Report\ (WorkbookCheck, WorkbookWriter, ClientStyle), src\Federator.Core\Clash\ (CreationPlan, ToleranceChoice, PriorityMap), src\Federator.Addin\Engine\ClashRunner.cs (465 to 469), FederationEngine.cs (243, 2779 to 2784), and the tests under Report and Clash |
| Report | src\Federator.Core\Report\ (ClashReportModel, ClashReportXml, ClientShapes, WorkbookCheck, PageCheck), src\Federator.Addin\Engine\ClashHarvest.cs (244 to 335), FederationEngine.cs (2678, 2772, 2929), the tests under Report, and a probe for the grid |
| Run log and RESULT | src\Federator.Core\Diagnostics\ (RunLog, RowLog, EventRow, RunClock, LiveLine), src\Federator.Core\Rerun\GroupJudgement.cs, src\Federator.Core\Clash\ (ClashRunOutcome, ToleranceChoice, ClashesAcrossTheRun), src\Federator.Core\Exchange\RevitCategories.cs, src\Federator.Core\Health\HealthCheckResult.cs, src\Federator.Addin\Engine\ (FederationEngine, ClashRunner, DocumentUnits), src\Federator.Addin\Ui\FederatorWindow.xaml.cs, and the tests under Diagnostics, Rerun, Clash, Health and Units |
| Views | src\Federator.Addin\Engine\ (ViewpointBuilder, SavedViewpoints, Penetrations, ItemSizes, FederationEngine 1022 to 1031 and 1721 to 1840), src\Federator.Core\Views\ (SizeText, SizeTally, ClashViewpointPlan), src\Federator.Core\Diagnostics\TimingBlock.cs, and the tests under Views and Diagnostics |
| Harvest and pictures | src\Federator.Addin\Engine\ (ClashImages, ClashHarvest 382 to 393, FederationEngine 2589, 2706 and 2743 to 2761), src\Federator.Core\Report\ReportOrder.cs, and the tests under Report and Clash |
| Install | build\install.ps1, INSTALL.md, tools\loop\run.ps1 (RunChild, BundleLeftovers, InstallVerdict, the Install flow, CheckMode), tools\loop\prove-run.ps1, tools\loop\README.md and .claude\rules\loop.md for words |
| Loop tools | tools\loop\run.ps1, nw-guard.ps1, prepare-copy.ps1, prove-run.ps1, read-workbook.ps1, mask-evidence.ps1, prove-hooks.sh, tools\probes\drive-window-run.ps1, probe-automation-start.ps1, the F105 probes and il-reader.ps1, tools\checks\check-evidence-ids.sh and evidence-ids.txt, and .claude\hooks\refuse-git-on-main.sh through a proved copy |
| Docs and words | steps\runs\03\for-modellers.md (copied in by a command), steps\03_bader_next.md, steps\04_audit.md, steps\loop.md and steps\02_questions.md (the lead's), .claude\rules\loop.md, core.md and addin.md, CLAUDE.md, .claude\agents\runner.md, docs\history\scan.md, INSTALL.md, tools\loop\README.md, the headers and comments of tools\loop\prove-run.ps1, run.ps1 and prepare-copy.ps1 and of tools\probes\probe-automation-start.ps1 and drive-window-run.ps1, a new tools\checks\check-restated-facts.sh, and in src ScanFindings.cs, BuildingGroup.cs, GroupRow.cs, ToleranceChoice.cs, RepeatedFailureGuard.cs, ClashRunner.cs, TestDrift.cs, FederatorWindow.xaml, OutputNameTable.cs, ReportPaths.cs, SetRebuildSettings.cs, SavedViewpoints.cs, FederationEngine.cs (915 and 3179), DocumentUnits.cs and ReportOptions.cs |
| Rest | src\Federator.Addin\Ui\ (FederatorWindow.xaml.cs, FederatorWindow.xaml, GroupRow.cs), src\Federator.Core\Naming\ (OutputNameTable, NamePattern), src\Federator.Core\Rerun\ (OutputPaths, RebuildTally), src\Federator.Core\Diagnostics\FolderMemory.cs, src\Federator.Core\Clash\ (UndoAutoReview, ToleranceChoice, a ClashWork rule), src\Federator.Addin\Engine\ (ClashStatusEditor, UndoAutoReviewed, ClashRunner 1102 to 1108, FederationEngine 1396 to 1398 and 2725, PropertyProbe, DocumentCensusReader, DocumentGuard, Penetrations 338 to 347), src\Federator.Core\Probe\ (ProbeCsv, ProbeTally, ProbeVerdict), src\Federator.Core\Exchange\ExchangeReader.cs, src\Federator.Core\Findings\ (ScanFindings, SourceMismatchFindings), src\Federator.Core\Grouping\BuildingGrouping.cs, and for FR-171 to FR-174 any file in src |

## Areas that can be worked at the same time

Files two areas share, beyond FederationEngine.cs, which every product area but clash counts
changes in its own methods:
- alignment and sets: ExportCheck.cs (FR-003 to FR-005 and FR-028)
- alignment and run log: GroupJudgement.cs (FR-001 and FR-043 to FR-045)
- sets and run log: HealthCheckResult.cs (FR-023 and FR-064)
- sets and docs: SetRebuildSettings.cs (FR-132)
- clash counts with workbook, run log, docs and rest: ClashRunner.cs, and with run log also
  ClashRunOutcome.cs (FR-031 and FR-052)
- clash counts with report and harvest: ClashHarvest.cs, and with report ClashReportModel.cs
- clash counts and rest: AutoReviewRecord.cs (FR-034 and FR-152)
- workbook and report: WorkbookCheck.cs (FR-035 and FR-040)
- workbook, run log, docs and rest: ToleranceChoice.cs (FR-036, FR-053, FR-063, FR-127, FR-166)
- report and harvest: ClashHarvest.cs (FR-041 and FR-076)
- run log and rest: FederatorWindow.xaml.cs (FR-049, FR-050 and FR-153, FR-159 to FR-166)
- run log and docs: DocumentUnits.cs (FR-062 and FR-134)
- views and rest: Penetrations.cs (FR-072 and FR-170)
- views and docs: SavedViewpoints.cs (FR-133)
- docs and rest: ScanFindings.cs, GroupRow.cs and OutputNameTable.cs (FR-126, FR-130 with
  FR-153, FR-154, FR-159, FR-160, FR-165)
- install and loop tools: tools\loop\run.ps1 (RunChild, BundleLeftovers, CheckMode) and
  tools\loop\prove-run.ps1
- docs with loop tools and install: tools\loop\README.md, the prove-run.ps1 header, the
  drive-window-run.ps1 header, the prepare-copy.ps1 comment and .claude\rules\loop.md

What that leaves:
- Lane one, the loop's tools. Loop tools and install touch no file under src or tests, so they
  run beside every product area. Inside the lane they share run.ps1 and prove-run.ps1, so their
  pull requests go one after another. FR-092, the harness, goes first because the others are
  proved in it. Install goes with F109, and FR-122 to FR-124 can go since F105 merged. The docs items that
  change only tool headers and the loop README, FR-140 to FR-145, ride in this lane.
- Lane two, the product. Clash counts shares no file at all with alignment, sets or views, so
  those can run at once. Alignment, sets, views and harvest and pictures share only
  FederationEngine.cs, in different methods, so they can run at once if each pull request takes
  main in before it merges. Clash counts, workbook, report, run log and rest share ClashRunner.cs,
  ClashHarvest.cs, ClashReportModel.cs, ClashRunOutcome.cs, WorkbookCheck.cs, ToleranceChoice.cs
  and FederatorWindow.xaml.cs, so they go one after another by file.
- Lane three, words in steps and rules. FR-125, FR-135 to FR-139 and FR-146 to FR-149 touch no
  code, so they run beside both lanes, what is left of FR-139 among them. The docs
  items that
  change src, FR-126 to FR-134, go with the area whose file they change.

Order inside the round:
- FR-001 before FR-043 to FR-045, and FR-002 with FR-001 (GroupJudgement.cs and
  AlignmentCheck.cs).
- FR-003 to FR-005 and FR-028 in one pull request or in order (ExportCheck.cs).
- FR-015 before FR-016, with FR-023 and FR-024 sharing its flag rule.
- FR-010 and FR-011 before or with FR-027, whose judgement they set.
- FR-151 before FR-164.
- FR-073's split of the seconds before FR-069's speed work, and FR-069 before FR-070's options
  are measured.
- FR-092 first in the loop tools lane.
- FR-159 with FR-165, and FR-160 after the find Bader's answer to Q24 asks.
- FR-174 last, alone, by Q98 B5.

## Source ids left out

From the findings of set 03:
- C06-J18, FIND-15, C06-DONE-12: steps over twice the same step on the group before. Every pair
  follows the clash and viewpoint count of the later group, and the SLOWER text is a window line
  only, so no log line could show it. The real fault in it, the VIEWS cost per viewpoint, is
  FR-069, and the SLOWER compare is FR-056.
- C06-J24, FIND-21, C06-DONE-18: the 22 GAP blocks and log:8218. Information the tool is built to
  give, the log says 'Nothing here is a fault' (log:495), and Q35 to Q40 waited on the counts
  until Bader answered them on 2026-10-06, FR-198.
- C06-J25, FIND-17, C06-DONE-19: 22 CENSUS NOTED lines and no CHANGED. By design, F73, nothing
  moved that should not.
- C06-J26, FIND-20, C06-DONE-17: one confirm dialog answered OK (driver.txt:21) and 20 Navisworks
  progress windows (watch.txt:19 to 114), one with an empty caption (watch.txt:94). No error
  dialog, and nothing needed a person.
- C06-J27, C06-DONE-6: 27 SOURCE MISMATCH entries and 4 SHARED SOURCE pairs (log:8223 to 8339).
  The tool reports and Bader decides, nothing is merged or unticked (core.md). The pair 1B06M1
  and 1C06M2 is named in FR-125.
- C06-J30, FIND-16, FIND-18, FIND-19, C06-DONE-21: checked and clean. 290 steps started and 290
  finished, no exception, no step threw, the RESULT counts agree with the lists under them except
  the two size lines (FR-046), and the 22 workbook read-outs agree with the log test by test and
  status by status.
- FIND-07: no document read in the evidence folder, so workbook against document is UNKNOWN. An
  evidence gap and not a fault of the product. The panel side is Bader's three tests
  (findings.md, Q97), the tool's own ROWS lines agree 3024 of 3024 (log:8577), and a tool that
  reads the saved NWFs would be a new loop feature that Q93 puts under register rows.
- C06-DONE-16: the document reads Feet in 16 groups and Meters in 6 after every model was set to
  Meters. The report is converted whatever the document says (core.md, Q23), and the smallest
  distance in any read-out is 0.034 m against a 25 mm tolerance (register row F26).
- READ ME, a register row: not a finding.
- The section of findings.md on Bader's three tests of done and the three panel tests (4 at
  100000, 11 at 1B06PH, 636 at 1B06PK): the panel counts are Bader's to read, and nothing was
  read in this run.
- Register rows PROVED WORKING by set 03: RUN-1637, F52, F73, F78, F80, F82, F11, F26, F27, F33,
  F34, F35, F39, F58, F59, F60, F61, F63, F64, F65, F91, LOOK 11, LOOK 27, LOOK 61, LOOK 70, LOOK
  80, LOOK 92, LOOK 97 98 99, LOOK 107 108 110 111, LOOK 118 120, LOOK 156 157, LOOK 159 163 164
  167, LOOK 215, LOOK 217, LOOK 227, LOOK 235 236 239, LOOK 248, LOOK 254 255 256 257, LOOK 269b,
  LOOK 270, LOOK 289 to 292, LOOK 296 to 301, LOOK 302 to 307, LOOK 308 309, LOOK 311 to 317, LOOK
  325 326 327 329, LOOK 331 332 334 335, and LOOK 388 tail.
- Register F, A, B, L and P rows NOT REACHED by a first run, so set 03 shows nothing about them:
  F50, F88, F87, F72a, F84, F79, F47a, F47b, F47c, F5, F6, F7, F8, F22, F1, F2, F4, F9, F10, F17,
  F16, F20, F24, F28, F29, F30, F31, F32, F36, F37, F38, F46, F40, F41, F42, F43, F44, F45, F51,
  F53, F54, F55, F56, F57, F62, F66, F67, F68, F71, F90, F94, A10 to A14, A17 to A22, B13, L2,
  B14, B2 (the register row, not Q98 B2), B11, L1, L3, L4, B7 and P3. F84, LOOK 37 and LOOK 388
  HEALTH were NOT SHOWN because the driver typed the XML path (driver.txt:12) and the HEALTH block
  is written only from the Browse button (src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2129-2131,
  Describe at :2141-2166). A person who browses gets it, so it is not a fault of the run.
- Register step rows NOT REACHED: steps 20-21 (F68), 29-34, 35-37 and the HEALTH half, 39-48
  (F27), 49-71 (F24, F29, F50), 62 and 185 (LOOK 62 itself is on FR-027), 75, 81-82 and 86-87
  (F31), 88-96 and 100-101 (F35), 102-115 (F26), 123-136 (F71), 137-146 (F9), 147-155 (F10),
  160-167 (F17), 168-174 (F8), 175-178 (F32), 179-191, 192-198 (F7), 209-212, 213-214 (F41), 219
  (F45), 228-233, 244-245 and 250-252 (F53), 260-262, 269 and 269a, 271-274 and 277-278 (F72),
  279-280, 286-287 and 318-324 (F62), 310, 340-343, 346-352 and 364. Three rows the findings
  reader listed as not reached are on items from the register reader and are not left out: the
  RESULT counts across Run presses row (FR-049), the page test order row (FR-042) and step 247
  (FR-068).
- Register LOOK rows NOT SHOWN, not reached so no fault shown: LOOK 10, 13, 15, 17, 19, 21, 23,
  28, 30, 31, 33, 34, 36, 37, 40, 46, 47, 51, 52, 54, 56, 58, 59, 60, 63, 64, 66, 67, 69, 75, 78,
  79, 83 to 87, 94, 106, 112 to 114, 122, 127 to 129, 131, 133, 135, 140, 142, 143, 146, 151 to
  153, 162, 166, 170, 172 to 174, 177, 182, 185, 187, 188, 190, 191, 193, 195 to 198, 210, 212,
  214, 221, 223, 224, 363, 216, 218, 219, 229, 231, 233, 241, 243 to 245, 251, 252, 259, 261, 262,
  264 to 269, 269a, 269d, 272, 274, 276, 278, 280, 286, 287, 310, 318 to 324, 333, 337, 339 to
  350, 380, 383 to 385, 391, 394, 395, 397, 398 and 388 HEALTH.
- The look_for rows of turn4\c06-find.json and turn4\c06-done.json: the CONFIRMED rows are not
  faults and the NOT SHOWN rows were not reached. The CONTRADICTED ones are on items: step 62 on
  FR-027, step 377 on FR-046 and FR-136, step 387 part 2 on FR-069, and step 269c on FR-035.
- The NEW-LOG rows that INSTALL.md steps 5 and 6 never ran (steps\loop.md:596) and what the
  penetration pass costs with the box off against on (steps\loop.md:609): done in code and not
  proved by a run, a proof and a measurement owed on the local machine and not faults. Set 05
  gives the second only if it runs the box both ways.
- The NEW-LOG row that the proof steps of 03_bader_next never worked as a whole
  (steps\loop.md:613): not a fault by itself, its fault part is FR-135.

From the register:
- T3-B: already done on main. TreeRefusal in tools\loop\run.ps1 (:754-765 at f38edd5, :1244 on
  main) refuses a HEAD that is not the stamp asked for and any line git status prints, untracked
  files included, steps\01_next.md:974 says T3-B is kept by Install, and the in place install of
  e4484d15 on 2026-10-01 read its stamp back equal (steps\loop.md:44-46).
  Directory.Build.targets:34-45 still writes +edits by design. Its register status still reads
  open, for F103, and is the lead's to close.
- F107-R2: a check at a merge, not a fault. F106 rewrote the header of
  tools\probes\drive-window-run.ps1 whose line 21 F107 masks, so the merge must keep the masked
  words, and the lead greps for the name after it. F106 merged first, as 3449521, so the check
  now falls at F107's merge.
- F104, F106, F107 and F109 as register rows: work in flight of plan step 2, not faults of this
  round. F106 has since merged, and F104, F107 and F109 have not.
- The known limits F102-L1, F102-L2 and Q88-IDS: limits Bader accepted (Q91 B, Q88), not faults
  to fix.
- CHAT-19, the DONE rows, the rows done in code and not yet proved by a run, and the rows that
  wait for Bader in the form: outside the register reader's source and not faults of this round.

From the readings of the fixes in flight:
- F106-HARM-REV-1: a clean statement of what the reviewer read whole.
- F106-HARM-REV-11: measured by set 03. Navisworks rotated out his autosaves of the same document
  names (record.txt:877 on) and the Q86 put back restored 78 files (steps\loop.md:80-81). What
  is left is a request to Bader not to open C06 in his own Navisworks during a run, no code
  fault.
- F106-HARM-REV-12: UNKNOWN until a run with Bader typing at the machine. The driver holds no
  key, click or pointer call, so nothing changes without a measurement.
- F106-HARM-REV-14: items 2 to 5 are wired and not yet proved, and set 05 runs them.
- F106-HARM-REV-16: UNKNOWN build counts of the stand-in, and the real run does not use the
  stand-in.
- F106-HARM-BRK-8: the breaker itself calls it harmless, the read back at
  drive-window-run.ps1:335-336 gates Run.
- F106-HARM-BRK-14, F106-HARM-BRK-15, F106-HARM-BRK-16 and F106-HARM-BRK-17: clean statements,
  turned over and nothing found.
- F108-REV-7: a missing Known bugs heading in the developer's draft log entry, never committed.
  The lead's steps\log.md entry for F108 carries the heading.
- F108-REV-8: the reviewer's own NOT A FAULT row.
- F108-BRK-9: a coverage note.
- F105-REV-2 and F105-BRK-1: blocking findings of F105, fixed and merged with it as 2c89788
  (docs\history\scan.md:5025-5030 and tools\probes\README.md:136-152 say probe-viewpoint-calls.ps1
  uses neither LoaderLines nor the string patterns).
- F105-REV-3 and F105-BRK-6: fixed and merged with F105 as 2c89788, its order line 35 and its
  steps\log.md entry the lead's.
- F105-REV-4: already on main, steps\loop.md:402 and 433-435 hold the F105 row and F105-R1 to
  F105-R3.
- F105-REV-7 and F105-BRK-8: checks that found nothing. F105-BRK-8 notes that scan.md 5z-f cites
  branch commits such as 30ae471 that a squash merge drops, and that the blob id is the pointer
  that resolves.
- F105-REV-8 and F105-BRK-7: what the readers could not check, not faults.
- F105-BRK-3: its six places fixed and merged with F105 as 2c89788 (docs\history\scan.md:5035-5039, 5330-5333 and
  5438-5439, tools\probes\README.md:111-113 and 144-152, steps\01_next.md:984).
- F105-BRK-4: fixed and merged with F105 as 2c89788 (docs\history\scan.md:5348-5359 names the nine assemblies not
  read and says whether they hold the names is UNKNOWN).
- F107-REV-1 and F107-BRK-1: blocking findings of F107, fixed on fix-F107 at 2063c29 (wt-f107
  steps\01_next.md:985 says every line number as read at 821ed6e and finds each line by its
  heading).
- F107-REV-2 and F107-BRK-4: merge conflict notes for the lead's merge order at the end of
  steps\01_next.md.
- F107-REV-3: fixed on fix-F107, order line 33.
- F107-REV-4 and F107-BRK-5: already on main, steps\02_questions.md:466 carries Bader's answer to
  Q88 and steps\loop.md:426 and 428 hold the rows.
- F107-REV-5: the steps\log.md entry is the lead's at the merge.
- F107-REV-6, F107-REV-7, F107-REV-8, F107-REV-9, F107-REV-10, F107-REV-11 and F107-REV-12:
  checks that found nothing.
- F107-BRK-3: the conflict with F106's driver header is in the lead's plan (steps\log.md:28-30),
  and F106's driver already says the machine of 2026-09-19 (drive-window-run.ps1:69 on fix-F106,
  now on main).

Placed and not left out, for the record:
- The bug readers left nothing out. T1-B7 sits on FR-160 and T1-L3 on FR-159.
- S03-19, C06-J29 and FIND-23 sit on FR-137, and C06-J13 on FR-125, as notes that the part they
  name is not a fault. FIND-22, C06-J23 and C06-DONE-20 sit on FR-074.
- T1-S25 sits on FR-031 and FR-052, and T1-S38 on FR-046 and FR-061, both placements right.
- T3-G6 to T3-G12 and T3-P, which steps\notes\f103-final-reading.md names as already registered,
  sit on FR-089, FR-090, FR-086, FR-088, FR-091, FR-078, FR-105 and FR-121.
- F105-REV-1 sits on FR-146 for its part on main, its branch part fixed in F105, merged as 2c89788, and
  F105-REV-6 on FR-122 as the same code fault as F105-R1.
- F108-R1 to F108-R5 and F107-R1, the rows the register reader could not see, sit on FR-093,
  FR-102, FR-103, FR-104, FR-118, FR-139, FR-144, FR-145, FR-084, FR-101 and FR-110.
