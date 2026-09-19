---
paths:
  - "src/Federator.Core/**"
---

# Rules for Federator.Core

Core has no Navisworks type in it and every rule here is one a test can prove. What
the add-in must do with the Navisworks API is in addin.md. The reasons and the
measurements behind every rule are in docs/history/claude-md-history.md, kept whole.

## File naming

Input:  1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc
Split on the hyphen. Part 3 is the building, part 5 is the discipline.
How the files are gathered is a CHOICE, made in the Grouping step, and per building
is only the DEFAULT. This used to be written here as a rule and it is not one. The
four choices are one file per building, one per building and discipline, one per
discipline across every building, and one file for everything. Per building puts
every discipline of a building into one federation, and under it discipline is read
for reporting only. Under the other three it decides the split as well.
Group on the full 6 character building code. 1C07BC and 1C07K1 are two buildings.

The output name is project, originator, building, level, discipline, type, number,
and it is a PATTERN with defaults rather than a fixed string. This used to be written
here as fixed and it is not. The first three come from the file names. The other four
are supplied, because the files in one group can disagree on all of them and outputs
overwrite, so copying from any one input would mean picking a winner.

The names are shown as a TABLE with one row per group, filled in by the scan, and every
cell in it can be typed over. A pattern is where a name starts and not where it ends,
because one group in twenty usually needs a name the pattern cannot give.

A name typed over is held per CELL, not per row. Changing a pattern refills only the
cells nobody has touched, so editing an NWF name does not freeze the NWD name beside it,
and the refill says how many rows it kept. A cell can be given back to the pattern.
Setting a cell to exactly what the pattern already gives still counts as typed over,
because the person meant that value.

The collision check runs on the table, not on the patterns, so two names typed to the
same thing is caught the same way two patterns colliding is. It names both groups and
the run does not start.

Every supplied field is shown in the Outputs step and can be changed there. The
defaults are ZZZ for the level, which is the ISO 19650 code for all levels, BM for
the discipline, which is a federated building model, MOD for the type, which is a
model, and 000001 for the number, which never advances because the file is
overwritten in place. A fifth, ZZZZZZ, is the building field for a group that covers
several buildings, following the same all convention.

The building field carries the group's own building code where the group is one
building, and the discipline field carries the group's own discipline where the group
is one discipline. The supplied value is only used where the group spans several.

There is one pattern each for the NWF, the NWD and the workbook, because a project
may want them to differ. They start identical. A set of patterns that would write two
groups to the same name is reported and the run does not start, because outputs
overwrite with no date suffix and the second would silently destroy the first.

The output name is built, not patched. That means a five part input still gives a
full seven field output name, and the readable-name floor is five parts. A six part
name reads fine.

If two files in one group disagree on the project code or the originator, report
it and skip the group. Do not pick one.

The split character, the part positions, the grouping choice and every supplied name
field are settings, never constants.

## What the clash test XML holds

Measured from the real files on 2026-08-27 and corrected on 2026-08-28. Do not
re-derive this. An earlier version of this section said the reference file held no
sets. It holds 61. The numbers below are the measured ones.

1104-PAR_CLASH_AllInOne holds both parts, 61 sets and 1830 tests in one file, and
all 61 test locators resolve against its own sets. It is the reference file. When
this file and another disagree about anything, this one is right.

The tests:

- root exchange, units="ft", one batchtest, 1830 clashtest children
- every test: test_type="hard_conservative", status="new",
  tolerance="0.2460629921", merge_composites="1"
- that tolerance is 75 mm. Tolerance is in the file units, convert to document
  units before use
- each side is one clashselection holding one locator, a name path such as
  lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals
- 1830 is every pair of 61 with no self pairs
- linkage is none and rules are empty in every test here. Read both anyway,
  another project will use them

The sets:

- 61 sets, carrying real rules, not one rule repeated
- 102 conditions across the 61 sets. 30 sets carry one condition, 26 carry two,
  5 carry four
- paths nest. Mechanical has 4 subfolders. Walk the folders, never assume the
  sets tree is flat
- set names contain spaces and ampersands, and two of them end in a space. Match
  the exact string. Never trim a set name or a locator
- the rule vocabulary seen in real files:

      category LcRevitData_Element display Element
        property LcRevitPropertyElementCategory display Category, the Revit category
        property lcldrevit_parameter_-1002053 display Workset

      no category element at all
        property LcOaNodeSourceFile display Source File

- condition test values seen: equals and contains
- a condition can arrive with no category element. The reader must not assume one
- rebuilding a search through the API uses the internal strings, never the display
  words. Element and Category and Workset and Source File are what a person reads,
  LcRevitData_Element and the rest are what the API matches on

Search_Set_Building.xml and Search_Set_Infra.xml are damaged exports. Every
condition in both reads category Category, property Name, equals Floors. They are
kept as samples only, to prove HealthCheck catches them. Never treat either as a
reference for what a good file looks like.

Counting distinct rules. Two different numbers are both right about the reference
file and they answer different questions:

- 53 is the number of distinct conditions, comparing test, category, property and
  value, with flags left out. This is what HealthCheckResult.DistinctRuleCount counts,
  because it is the one that catches a damaged export: Search_Set_Infra has 2715
  conditions and 1 distinct condition
- 59 is the number of distinct sets, comparing each set's whole ordered list of
  conditions. It is 59 rather than 61 because two pairs of sets carry identical
  rule lists: Telecom Fixtures with Telephone Devices, and Electrical Fixtures
  with Devices

The set level number cannot be used for the damaged export check. On Infra it
gives 6, because the sets differ in how many copies of the one rule they hold,
and 6 does not read as broken.

## Rules the code holds

- A locator that does not resolve to a set: report the test by name, skip it.
  Never import a test with an empty side. It returns zero clashes and reads as passed
- A test where either side resolves to zero items in this model counts as skipped,
  not passed. Skipped and passed are counted as different numbers in the report
- A TEST WHOSE SIDE FINDS NOTHING IS NOT CREATED AT ALL, F77. The sets are built and
  resolved before any test is created, so how many items each locator finds is already in
  hand, and creating a test to discover a thing already known was 631 seconds of a 1424
  second run: 1830 created, 1619 of them thrown away moments later by the empty side
  check. `Federator.Core.Clash.CreationPlan` is the rule. It CALLS `ClashSideCheck` rather
  than carrying a second copy of it, and that check STAYS as the second line of defence
  before a test is run. A LOCATOR NOBODY COUNTED IS NOT A LOCATOR FINDING NOTHING: a
  count missing from what the plan is handed means the test is created and the run-time
  check answers, because the safe mistake is an extra test and the unsafe one is leaving
  a real test out of the NWF over a number nobody took. One counted line,
  `CLASH 1830 in the file, 211 created, 1619 not created, a side finds nothing`, and which
  tests they were is said by the ordinary skip block, a count and five examples, where
  every other skip reason is already said. THE WORKBOOK STILL CARRIES A BLOCK FOR EVERY
  TEST IN THE FILE. Not creating a test changes what goes in the DOCUMENT and never what
  goes in the report, because the client's report is the whole matrix and a test missing
  from it reads as a test nobody ran rather than as a test that could not clash
- The Revit container inside an NWC is often a different building from the NWC.
  Where the building code parsed from the NWC name differs from the code in the
  Revit source name, report SOURCE MISMATCH naming both, and where one Revit
  building code feeds more than one group, report SHARED SOURCE, because that means
  one building has been split in two by a naming error. Both codes are read with the
  same parser used on the NWC names, never a separate rule. This is information. It
  does not block, unpick or merge anything. Bader decides
- When tests skip for the same reason, log the count and at most five examples, then
  the total. Per test detail stays for tests that were created or run. One run wrote
  1830 near identical SKIPPED lines and a 1 MB log, which buries everything worth
  reading
- A test already in the document is left as it is, which means a tolerance changed in the
  XML never reaches it. That is right and it was silent, so now it is REPORTED. Every
  test in both is compared on the tolerance, the test type, merge composites, and per
  side the locator and the self intersect and primitive type flags, and every difference
  is named by test name with both values. Comparing changes nothing on either side.
  Tolerances are compared within an epsilon because the file's number travels through a
  unit conversion, and an exact comparison would report drift on all 1830. Locators are
  compared Ordinal and never trimmed, because two set names in the reference file end in
  a space. A side this tool could not read comes back as the word UNKNOWN, which is a real
  string and compares like any other, so it is left OUT of the comparison and counted as
  not compared. The block says how many, because not compared is not the same as matching.
  The marker lives in Federator.Core.Clash.TestSettings.UnknownLocator, where the
  comparison can read it
- Resolved clashes stay in the file forever and that is the point of them, so the count
  is reported per test and as a total for the group. A test running weekly for months
  accumulates them without limit and nothing else prunes them
- Nothing removed and nothing asked to be removed are different, so the compacted count
  is minus one until a compact actually runs. Zero removed is a real answer. The count is
  the Resolved total read BEFORE less the Resolved total read again after the compact, so
  it is measured rather than assumed, and the log carries both numbers
- Per test, everything comes from the file and never from a constant: the name, the
  test type, the tolerance, merge composites, and per side the self intersect and the
  primitive type flags. A test whose tolerance attribute is not there at all is SKIPPED by
  name, the way an unknown test type is, because a missing tolerance became zero and zero
  reads as a real tolerance. A tolerance written as zero is a real one and is created
- THE TOLERANCE CAN BE CHOSEN IN THE TOOL AND THEN IT BEATS EVERYTHING, F76. The Clash
  step carries a drop down, `Federator.Core.Clash.ToleranceChoice`: Use the value in the
  XML, which is the default and changes nothing, then 25 mm, 50 mm, 75 mm, then Other
  with a number box. When anything but the default is chosen that value is set on EVERY
  test in the run, the ones created fresh from the XML and the ones already saved in the
  NWF alike, and it beats the XML and it beats the document. ApplyFileSettings keeps
  exactly the meaning it had, which is whether the FILE'S settings reach a saved test,
  and this wins over it, because a value a person typed beats a value read out of a file.
  The choice is in MILLIMETRES because that is what a person says, converted through
  UnitTable like any other, and a unit the table has not been taught throws rather than
  falling back. Zero is a real tolerance and is allowed. A negative one is refused where
  it is set rather than reaching 1830 tests. The confirm screen says the value, the
  counts, what it beats, and that changing a saved test RESETS its results, and says
  nothing at all under the default, because a line that reads the same on every run
  teaches people to skip the screen. One TOLERANCE line per group carries both counts and
  the converted value. Why it exists: the matrix is written at 25 mm, the NWFs on disk
  held tests at 75 mm, a test already in the document is left exactly as it is, so the
  run clashed at 75 while everybody believed it was clashing at 25
- THE REPORT READS ITS TOLERANCE OFF THE CLASH TEST IN THE DOCUMENT, F76, because the
  document is what produced the row. It used to be copied off the PLAN, which is the
  XML, so on every weekly run the cell said what the file asked for and the run had
  clashed at something else. `ClashReportModel.TestReport.ToleranceFrom` carries WHERE it
  was read, an origin of Document, File, Tool or Unknown, kept for the same reason the
  Item ID keeps which property supplied it: a number is only honest while what produced
  it is still visible. `ToleranceChoice.ReadFromLine` counts the four across the run, so
  a report reading the file rather than the document shows as a number rather than being
  trusted
- Tolerance is read per test and converted from the file units attribute into the
  units of the open document before it is set. There is no global tolerance setting
  in this tool. Both numbers and both unit names go in the log, because which units
  ClashTest.Tolerance is measured in is UNKNOWN until a test runs against a real model
- A test type that does not map to a value on the enum is reported by name and
  skipped. It is never approximated to the nearest one, because a hard test standing
  in for a clearance test reports a number that reads as real and is not
- A run that is failing everything stops the run, not the group. After the first 50 tests,
  if every one has failed for the same reason, the whole run stops and says so in one
  line. Every group of that nine hour run failed the same way, so a per group stop would
  have saved none of it. The 50 is a setting, ReportOptions.StopAfterFailures, and the one
  guard the whole run shares is built from it. A test skipped because a side finds nothing
  is the ordinary answer and counts neither way
- An exception repeating with the same heading and the same trace is written out in full
  once and counted after that, and the RESULT block carries the total beside the one
  trace. One run wrote the same stack tens of thousands of times into a 17.8 MB log
- Every picker returns a FOLDER to open at, including the one that chooses a file, and the
  caller never takes the parent of it. Doing that opened the clash XML picker one level
  above the folder it had remembered. The rule lives in
  Federator.Core.Diagnostics.PickerStart so all six can be proved rather than clicked
- Every picker remembers its own last folder, in a file beside the logs, and reopens
  there next time. Per picker and never one shared, because with one shared, picking an
  NWD folder moves the source picker to it and the next run reads the wrong folder. A
  remembered folder that has gone is not an error and is never cleared, the picker opens
  at the nearest parent still on disk and says so, which is what happens when a project
  drive is not mounted yet. Remembering never stops a run: an unwritable location is
  recorded as a reason and the session still remembers, it just does not survive a restart
- No code identifier and no framework message ever reaches a label. It said
  "Workbooks go in UNKNOWN" before a scan, with "Parameter name: nwfFolder" behind it,
  because an empty NWF folder threw and the message went straight to the window. Nothing was
  wrong at that point, the folder had not been picked. A failure DIALOG may carry the
  exception message, because that is what gets sent back. A label may not. Every wording a
  label carries lives in Core where a test can read it, and the test hands it the thing the
  label must not carry and asserts it is not in what comes back. ReportPaths.WhereTheyGo for
  where the workbooks go. RunLog.WhereTheLogIs, NoLogFileOpened, ErrorsAreInTheLog and
  TheLogSaysWhy for anything about the log, so a run that could not open one names the
  folders it tried and the log line keeps what threw. RepeatedFailureGuard.Reason for the
  log and ReasonInPlainWords for the label, so the stopped run says how many tests failed
  the same way and never what they threw
- Findings are written in the words a person would say, never in the words the tool
  thinks in. A shape is described by naming a real code that has it, not by printing
  9A99AA. A shared Revit source says two federations are being built from one Revit
  building, names both, and says that usually means one NWC file name is wrong. The
  short code stays as its own column so it can still be sorted on
- A group holding fewer than two disciplines cannot clash with anything, whatever the
  test list says, because every clash test is one discipline against another. One NWC is
  the plain case and two NWCs of the same discipline are the same case, D5 on
  2026-09-12, so the count is of disciplines and not of files, in
  Federator.Core.Grouping.BuildingGroup.CannotClashWith, read by the window row and the
  engine alike, and none of them is run. SINCE F77 THEY ARE NOT ALL CREATED EITHER,
  which is a change from the rule this bullet used to state. That rule said every test is
  still created so the NWF is complete and matches the other groups. It cost 631 seconds
  of a 1424 second run to create 1619 tests that were thrown away moments later, and Q46
  asks whether the completeness was worth it. The reason is SingleDiscipline and it is counted apart from EmptySide on
  purpose. A side finding nothing says a discipline was not exported. One discipline says
  the group was never going to clash and no export would change that. In the last real
  folder that was 1B06BS and 1C06PK, and both ran 1830 tests for nothing. The open file
  is never judged this way, because nothing was scanned and its count is UNKNOWN
- The reports never go inside the folder being scanned, whether it was picked or
  defaulted to. A run put the workbooks in the NWC folder it was reading, and the
  Clash step picks an XML at run time, so a report written there is a file a later run
  can be handed as its own input. A picked folder inside the source is refused and the
  default beside the NWF folder is used instead, saying so. Where the NWF folder is
  itself inside the source there is nowhere safe, and that is said rather than written
  somewhere surprising
- Outputs overwrite, the NWD every run and the NWF only when it is being built for
  the first time. No version suffix, and no date suffix unless the weekly record tick
  box is on, which is off by default and applies to the NWD alone
- The NWF and the NWD are dated differently on purpose, and this is the reason. The
  NWF holds the clash tests and every clash result inside it, so it IS the history and
  overwriting it in place is what keeps that history. A dated NWF would fork the
  history: this week's clashes would go in one file, last week's Active and Resolved
  would be stranded in another, and no file would hold the whole picture. The NWD
  carries no clash results at all, it is the model as it stood, so a dated NWD is a
  weekly record that costs nothing and loses nothing. One is a ledger and one is a
  photograph
- The date goes in the number field, because the number never advanced anyway. It is
  a format string and a setting, DateFormat, defaulting to yyyyMMdd. It is not a
  fixed string and never a fourth name pattern
- DateTime.ToString does NOT throw on a format string nobody can read, it treats what
  it does not recognise as literal text, so "not a real format" comes back as
  "noA a real 0or0aA". Measured, see docs\history\scan.md section 4j. So what is checked is
  what the format PRODUCED, not whether it threw. A result that is empty or holds a
  character Windows refuses in a file name falls back to the number. Anything else is
  used as typed, because the format is the person's setting and their mistakes should
  be visible in the preview rather than silently corrected
- A group ends in one of three states, and the test is always what was ASKED FOR,
  never what happens to be on disk. A step deliberately switched off is not a
  failure. Judging a group by whether an NWD existed, with republishing switched
  off, once reported all 22 groups of a clean run as FAILED.
    DONE     everything requested for this group succeeded
    PARTIAL  something requested did not complete, or the group was CHANGED and
             left alone, which since F24 means only a group whose rebuild never
             started, because a rebuild that ran ends as Rebuilt
    FAILED   something requested threw or produced nothing, a rebuild that appended
             nothing or could not keep its saved tests included
  The rule lives in Federator.Core.Rerun.GroupJudgement, with no Navisworks types
  in it, so it can be tested. The outcome and the reason for it come out of one
  pass, so the two can never disagree
- The counts in the RESULT block and the errors under it come from one list. A
  failed count with an empty error list is what the log printed once, saying
  "groups failed: 22" and "Nothing failed." in the same block. A group recorded as
  failed always carries a reason, and one is substituted rather than thrown over
  when a caller forgets, because logging never stops a run
- A file written twice reports the size of the SECOND write. The list holds one entry per
  file so the count stays a count of files, and the SIZE is the last one read. The NWF is
  saved twice, once after the append and once after the clash step, and keeping the first
  size made a run report it at 4,141 bytes, an empty federation, when the second save had
  read 165,844 off the disk four minutes earlier. Nothing had shrunk. It read exactly like
  an NWF that had lost every clash result in it, which is the most alarming thing this log
  can say, so it must never say it by accident
- Only a file this run actually wrote goes in the files written list. Outputs
  overwrite with no date suffix, so last week's NWF and NWD sit at exactly the
  paths this run uses. A group that threw before writing anything must not list
  them as its own, and a file that was checked rather than written is logged with
  CheckOnDisk, which reports the size and records nothing
- The workbook is ONE SHEET, laid out exactly as the report the client receives: every
  test one after another, most clashes first. The Summary sheet, the Matrix sheet and the
  sheet per test are GONE. All three were asked for in an earlier session, before anyone
  had put a real Navisworks report beside ours. Once both were seen together the ask
  became one thing, our output matching theirs, and theirs has none of them. Do not add
  them back because they seem useful. If it is not in theirs it is not in ours
- The tests are ordered MOST CLASHES FIRST, and where two hold the same count they keep
  the order they were created in. Measured off both exports in samples\client-report on
  2026-09-01: both are strictly descending over all 1830 blocks, and every tie group is in
  the order the tests sit in the exchange file rather than alphabetical, including one
  group of 1807. So it is a STABLE sort, and List.Sort is not one
- The OUTPUT is sorted and the document is not. Whether their report is sorted by the
  report writer or simply walks a collection Clash Detective had already sorted is
  UNKNOWN and cannot be read off the files. Sorting the document would mean calling
  TestsSortTests, a mutator that reorders the tests inside the NWF, and the NWF is the
  record of what has been fixed
- A distance or a coordinate is THREE DECIMALS with the trailing zeros kept, except that a
  value which is not zero but would round to 0.000 is written to three significant figures
  instead, in plain decimal and never an exponent. Measured across all 387 coordinates and
  129 distances in the two exports: nine are of that second kind and 0.000 appears nowhere
  in either file. Whether theirs rounds or truncates is UNKNOWN, because both files carry
  only the formatted text, so ours rounds
- The Item ID label is Element ID, and this tool CHOOSES it rather than reading it. It used
  to be the display name of whichever property matched, and since Id is first in the search
  list that came out as "Id: 990299" against their "Element ID: 702888". Which property
  actually supplied the value is kept on the item and goes in the log, because renaming a
  value is only honest while what was renamed is still visible
- A report check compares three things and not one: WHICH columns, in what SHAPE, in what
  ORDER. The check before this compared presence alone and passed while the test order, the
  id label and both number formats all differed from the samples. It reports the FIRST
  divergence with what ours holds and what theirs holds, because one fault has forty
  consequences and the first is the one to act on
- One workbook per group, written after the clash step and before the NWD is published,
  so the three outputs of a group agree with each other. Named like the group's other
  outputs with an xlsx extension, and it overwrites, the same as the NWF and the NWD. The
  Excel folder is picked on the Outputs step, and when it is empty the workbooks go beside
  the NWF folder in a subfolder called Clash Reports, which is a setting,
  ReportPaths.Subfolder. It is static because the open file run and the line under the
  Outputs step both work the folder out with no options object in front of them, and a
  name carrying either slash is refused where it is set, by name and never by asking the
  running platform which one it calls a separator
- Results are grouped, not one row per raw clash. A group is one row and its distance is
  the most severe clash in it, which is the minimum: a hard clash reports a negative
  overlap so the worst is the most negative, and a clearance test reports a gap so the
  worst is the smallest. Every row carries the raw count behind it, so the grouping hides
  nothing
- The clash API has no open against closed notion. Nothing on IClashResult, ClashResult,
  ClashResultGroup, ClashTest or DocumentClashTests names one, ClashResultStatus is a flat
  five value enum, and Navisworks' own report does not mention open or closed either. So
  every status is reported as itself, and where one number is needed for what is
  outstanding, WHICH statuses that number holds is a setting with two choices and never
  a constant, because neither answer is readable off the API and both are stated rules:
    Navisworks open  New, Active and Reviewed. The default, because it is the product's
                     own definition and the number then agrees with the panel
    New plus Active  the two only, for a project that treats Reviewed as dealt with
  Approved and Resolved are closed under both. The choice is written to the log, so
  nobody reads an API meaning into a number that has none. The word open never appears
  unqualified. Since the Summary and Matrix sheets went, no output shows that number,
  so the window no longer offers the choice and the report options no longer carry it,
  D3 on 2026-09-12. The two rules stay in OpenClashes with their tests, because the
  image filter reads Navisworks open off the same place
- The test sheet carries the CLIENT'S columns, in their order and in their words, because
  they have already accepted a report in that shape. Per test:
    Tolerance, Clashes, New, Active, Reviewed, Approved, Resolved, Type, Status
  Per clash:
    Image, Clash Name, Status, Distance, Grid Location, Description, Clash Point,
    then Item 1 and Item 2, each Item ID, Layer, Item Name, Item Type
  Four of these are joins and not columns, which is the part a description gets wrong and
  the stylesheet settles. Grid Location is ONE field, "B-1 : ROF", grid then a spaced colon
  then the level. Clash Point is ONE field, "x:31.643, y:-2.913, z:3.325", three decimals
  each with the trailing zeros kept. Item ID is ONE field, "Element ID: 1554240", and the
  tool CHOOSES that label rather than reading it off whichever property matched, which once
  wrote "Id: 990299" against their "Element ID: 702888". Which property supplied the value
  is kept on the item, because renaming a value is only honest while what was renamed is
  still visible.
  Tolerance carries its unit with no space, "0.025m". Distance is the raw signed number,
  negative on a hard clash, written as a number so it still sorts. Type reads
  "Hard (Conservative)". Measured, see docs\history\scan.md section 4k
- Our extra columns, family, type name, material, source file and discipline, come AFTER
  theirs and never in place of any of them, on the CLASH XML and nowhere else. The Client
  columns only tick box that used to switch them off is GONE, because the workbook became
  one sheet laid out as theirs with none of ours on it, so there was nothing left for it to
  remove and it sat in the window doing nothing. Ours says Type Name
  rather than Type, because the client already has an Item Type column and it holds
  something else, the Navisworks item type, which reads Solid
- The client's report DOES carry a Layer column, in both committed exports, one per item
  block. Read off the header row of each on 2026-09-12. This bullet said the opposite, from
  a measurement of two other exports, and the code has written fifteen headings all along.
  Item Name and Item Type are not fixed columns either, they are the quick properties, and
  the accepted report happens to carry those two. So the fifteen are the whole of it, seven
  before the item blocks and four in each of the two
- Grid Location is built from the grid intersection name, which ALREADY carries the level.
  Appending the level to it wrote "D-8 : LGF : LGF" on a real run. The level is only added
  where the grid does not already end with it
- The Type cell carries the client's wording, "Hard (Conservative)", not the file's token,
  "hard_conservative". One pair is measured and the rule read off it is that the first word
  is the type and the rest is a qualifier in brackets. A value that already has a space or
  a bracket in it is left exactly as it is
- An all zero GUID is not an id. A run wrote
  "Instance GUID: 00000000-0000-0000-0000-000000000000" into all 426 item cells, which
  reads like an id and identifies nothing. The cell is left empty instead, and the real
  cause is fixed too: the clash gives back the geometry leaf, which on a Revit sourced NWC
  carries a material name and no Revit properties, so the id, the family and the type are
  looked for on the item, then on its composite item, then up its ancestors
- Distance is the ROUNDED signed number and the cell carries no number format, which is
  what theirs holds and what the code writes. This bullet asked for a raw number behind a
  format, which is the thing the later bullet says was wrong: the cell read -0.328 while
  anyone sorting, filtering or copying the column got -0.328083992004395. The VALUE
  differing between two reports is the two documents' units and not a fault: ours measured
  in feet and theirs in metres, and the tolerance says so too, 0.2461ft against 0.025m
- Client columns only means nothing of ours on the sheet at all, not just no extra columns.
  The notes above the table, the link back to the Summary and the filter arrows are ours
  and all three go. Their columns do not move
- The client's logo is not copied. It is an Autodesk logo that ships with their report and
  not with this install, and there is no right to redistribute it
- A link to an image is written as a relative Uri and never as a plain string. Handed a
  string, ClosedXML reads a relative path as an INTERNAL address, so the cell tries to
  jump to a sheet of that name instead of opening the picture. There is a test that opens
  the written xlsx as the zip it is and reads the relationship back, because the object
  model reported this as fine while the file was wrong
- The report still counts skipped, passed and found as three numbers that are never
  merged. On 1C07BC that is 1164 skipped, 618 passed and 48 with clashes, and those add
  to 1830. A skipped test says so and never reads as passed
- The client's report is HTML (Tabular), not a workbook. Clash Detective cannot export an
  xlsx at all, so what they accepted is a page exported from Clash Detective and opened in
  Excel. That is why the sample declares 53 columns with 17 populated, carries merged cells
  and has absolute file:/// links in it
- The page is rendered by Autodesk's own clash_report_html_tabular.xsl, read from the
  install at run time and never copied into this repo. The path is built and tested
  directly, the language the application reports and then en-US, never searched for. Where
  neither is there the log names both paths, no page is written, and the workbook and the
  XML are untouched. It is on by default, because it is the format the client accepts
- Not one column in that page is fixed. Every one is a boolean over an XPath in the
  stylesheet, so what the page holds is decided by what our XML holds. The Layer column is
  the proof: 1A04WE has it and 1A02WE does not, from the same tool, because one export
  carried layer data and the other did not. Those two were measured on Bader's machine and
  are recorded in docs\history, and they are not the exports in samples\client-report,
  which are 1A02WN and 1A04WN and both carry the column. Never write a column list for that
  page, write the data and let the stylesheet decide
- Our XML feeds that stylesheet, so its shape is not ours to choose either. Measured on
  2026-09-01, it answered every column test but three: description, smarttags and the href
  on a result, which are the Description column, the Item Name and Item Type columns, and
  the Image column. See docs\history\scan.md section 4m for the whole table
- Every report this tool writes is READ BACK off the disk and checked, and what the check
  found goes in the log as a block and in the window as one line. Bader was opening every
  report in Excel and searching it by hand, and the tool wrote the file. The FILE is read,
  never the object that produced it, because the object model has twice called a written
  file fine when it was not
- The page check reports the rows, how many carry an Item ID on each item, the first id in
  full so its shape is visible, any column that is not the client's named, the tolerance
  cell exactly as written, how many picture references there are and how many use a
  backslash and how many are really on disk, and whether the logo is there. The workbook
  check reports the sheets and their names, the blocks, the rows, the clash count of each
  block, and the first thing that differs from the client's layout with both sides printed.
  It counts no column of ours, because since the workbook became their one sheet there is
  no column of ours on it to count
- A report check NEVER fails a group. It is a warning on its own list, apart from the
  errors, because the NWF, the NWD and the workbook were all still written. Judging a group
  on it would repeat the fault that once reported a clean 22 group run as FAILED
- The client column set is read from clash_report_html_tabular.xsl and from the exports in
  samples\client-report, never typed into the code. The stylesheet gives what is possible
  and the samples give what is theirs. A test re-reads both on every run and fails if the
  set has drifted, so the list is checked against the files rather than trusted
- The stylesheet carries a DEAD template named ItemHeaderCells that nothing calls. It
  writes Item Type as a literal, reads ./Name with a capital where the rest of the file
  reads name, and puts Item ID and Layer after the quick properties rather than before.
  None of that is what the reports carry. The live one is mainTableHeader and it is the
  only one anything here reads. Measured, see docs\history\scan.md section 4o
- The page carries ONLY the two quick properties the client's report has, Item Name and
  Item Type, whatever any tick box says. The stylesheet makes a column out of every
  smarttag, so Family, Type Name, Material, Source File and Discipline would each become a
  column on the page the client receives. They are workbook columns. createddate goes the
  same way, because neither supplied report has a Date Found column
- Tolerance is written to THREE decimals, which is how both of theirs are written. Ours
  read 0.2460629921ft against their 0.025m. The units differing is the two documents
  differing and is correct. The precision was ours
- A picture reference on the page uses a BACKSLASH, for the clash pictures and for the
  logo, because that is what both supplied reports write and the client opens these in
  Excel on Windows. The workbook keeps a forward slash, because a hyperlink there is a Uri
  and their own xlsx has no picture hyperlinks at all to match
- Neither their report nor ours embeds a picture in the xlsx, measured on the zip of each.
  The native export never does. Pictures show only when the _files folder sits beside the
  file, so the page and its folder are what gets sent. That used to be said by a tick box
  and the box is gone, because the accepted report has photos and one without them is not
  the thing the client agreed to receive, so it is fixed on
- Exactly ONE objectattribute per clashobject, and it is the id. The Item ID cell is
  value-of over ./objectattribute/name, which takes the FIRST node, so writing several put
  the item's Name in the id column. Everything else about an item is a smarttag, which is
  how Item Name and Item Type reach the page at all
- Every clashobject carries the SAME list of smarttags, empty values included. The
  stylesheet counts them once off the first clashobject and uses that count for every row,
  so one item with fewer of them slides every column after it sideways
- Date Found and our five extra item properties are ours, not theirs. Their report has
  neither, and there is no longer a choice about it: the workbook is their one sheet with
  none of ours on it, so both are left out everywhere and neither is written at all
- The logo is DATA, read by the stylesheet from //logo/@href, not part of the layout, and
  the page carries the one Navisworks puts on its own reports because that is the report the
  client accepts. It is read off the install at run time, the same way the stylesheet is,
  from \Images\logo.jpg at the top of the install with no language folder, measured
  2026-09-01 at 6137 bytes and byte for byte the same file as the logo.jpg in both supplied
  reports. There is nothing to set up before a first run
- No copy of that logo is in this repo, in the bundle, or in install.ps1. It is read off
  the machine that is running, every run. The only two in the checkout are inside the client
  exports Bader committed, where Navisworks had already put them, and a test names that
  exception so one appearing anywhere else fails
- The logo is COPIED into the report's own _files folder beside the clash pictures and
  linked relatively, which is what Navisworks itself does. The report goes to a client, so
  the page must not point at a path on the machine that wrote it. The accepted xlsx carries
  absolute file:/// links and that is exactly why its pictures break everywhere else. A test
  opens the written page and reads the src back out of it, because the object model reported
  a broken link as fine once already
- A logo that cannot be found writes the page without one, names every path it looked at,
  and carries on. A missing picture never stops a report
- The Browse box stays, for a project that needs a different mark, and it starts filled with
  the install's logo. Clearing it means no logo. Most people never touch it
- Writing a logo turns the stylesheet's Image column on even with no clash pictures, because
  its test is boolean(//@href) and the logo carries an href too. That is Autodesk's own
  behaviour and their reports do the same. Pinned by a test so nobody reads it as a fault
- The clash XML is optional and off by default. It is built from the same results the
  workbook is built from, never by reading the workbook and never read by it, so a fault
  in one cannot corrupt the other. Its shape was read off the three stylesheets the
  Navisworks install ships, because there is no clash report schema anywhere in the
  install. What is filled and what is left out is listed in ClashReportXml, and a part
  with nothing to put in it is left out rather than written empty
- The workbook library is ClosedXML, and the writer lives in Federator.Core rather than in
  the add-in so the tests write a real xlsx and read it back without Navisworks. It ships
  twelve more DLLs into the bundle and install.ps1 carries every one of them, because the
  add-in would otherwise load and then throw the first time a group finished. None of them
  collides with a file the Navisworks install ships
- Images are ON by default, because the report the client has already accepted has them
  and one without them is not the thing they agreed to receive. They go where that report
  puts them, in a folder beside the workbook named after it with _files on the end, as
  loose jpg. The workbook links to them and does not paste them in. A thumbnail in the
  cell is a tick box, off, because pasting is not what the accepted report does and it
  makes the file many times larger. No clash is ever saved as a viewpoint in the NWF
- The picture names are theirs and are NOT one running sequence, which is what the first
  dozen look like. It is cd, then the test formatted 00, then the clash within that test
  formatted 0000. Test 0 clash 1 is cd000001.jpg and test 100 clash 1 is cd1000001.jpg,
  seven digits, measured on a real 2672 picture export. Two digits is a floor and not a
  width. The test number counts tests that have pictures, so it can never leave a gap, and
  a test whose first render fails hands its number back
- The pictures are NUMBERED IN THE EXPORT ORDER, which is the order the rows are written:
  tests most clashes first with ties in creation order, and inside a test the clashes as
  Clash Detective lists them. Measured off both exports in samples\client-report, where
  the first block is cd00 and the second cd01 whatever order the tests ran in. A picture is
  rendered while its test runs, under the run order number, because the report order is
  only known when the last test has run. So after the run every picture is renamed ONCE,
  in one pass, and the row, the workbook link, the XML href and the page all follow it. The
  rename goes through a holding name first, because a swap between two tests would
  otherwise write one picture over another. What a picture shows and its size are
  untouched. The rule lives in Federator.Core.Report.ReportOrder and the rename in
  Federator.Core.Report.ImageRenumbering, so both can be tested without Navisworks. A
  rename that fails is a warning on the report and never fails the group
- The image settings: on by default, a cap per test defaulting to off, a status filter
  defaulting to New Active and Reviewed, and a size defaulting to 1024 by 1024 because
  that is what all 60 pictures of the accepted report measure. An image that fails renders
  nothing, is logged by name, leaves its cell empty and never stops the run. Fifty
  failures of the same reason stop the run, the same way and with the same default as the
  clash step, and the line says images rather than tests
- What the pictures cost is MEASURED and logged per group: seconds per image, total
  seconds, how many, and how many megabytes. Every number anyone has given for this,
  including mine, was a guess until this existed. They are in the log and nowhere else,
  because the Summary sheet they used to go on is gone
- The report goes out in METERS, always, whatever the document measures in. Q23. It used
  to go out in the document's units, with the rule that converting a number ourselves
  while the document read feet would make the numbers and the label disagree. The run of
  2026-09-07 settled it the other way: 11 of 14 groups set every model to meters, the
  document still reported feet, and the report went out in feet with the tolerance at
  0.246ft. So the conversion is done, once, in one pass over the FINISHED report, in
  Federator.Core.Report.ReportUnits, before the workbook, the page or the XML is written.
  All three read the same converted report, and the same pass writes the unit label, so a
  number and its label still cannot disagree. What is converted is the tolerance of every
  test and the distance and the clash point of every row, and nothing else, because a grid
  location is text and a raw count is a count. The factors are UnitTable, the one unit
  table in this repo, read through ExchangeUnits, and a unit it has not been taught is
  REFUSED: nothing is written and the group FAILS, because a report in the wrong unit
  reads as real and is not
- Every unit this tool knows is one row of Federator.Core.Units.UnitTable, F33: the name
  on the Navisworks enum, the words the window shows, the short label the report and the
  log write, the exchange file's units code, and the millimetres in one. ExchangeUnits
  converts through it, DocumentUnits and ClashRunner name units through it, and the
  Outputs step offers the rows it offers. There is no other list, because the lists had
  drifted, the same unit was micrometers in one and um in another. A model units name the
  table does not know FAILS the group with nothing converted, it used to fall back to
  Meters without a word. The exchange reader reads the tolerance as written and converts
  nothing, so a file in a unit the tool does not know reads, and ClashTestPlan.Convert,
  the one place a file unit is judged, skips each of its tests by name
- DECIDE WAITS FOR THE MODELS BEFORE IT COUNTS THEM, F74. `Document.TryOpenFile`
  returning true does not mean the models are in the document. On the first real run all
  five existing NWFs reported 0 unchanged, 4 added, 0 removed and were rebuilt, and the
  step finished in 0.248 seconds against 2.3 to 4.8 seconds for every open in the one
  earlier log on record. `Federator.Core.Rerun.ModelLoadWait` is the rule: the add-in
  reads the count and the monotonic clock and hands both over, so the run and the preview
  read one rule and cannot disagree. A count above zero that stops moving over three
  readings a quarter second apart is settled. ZERO NEVER SETTLES, because zero cannot be
  told apart from a document that has not started filling, so it is only accepted at the
  thirty second ceiling. All three numbers are settings. One LOADING line is written
  either way. Whether the API reports readiness on its own is UNKNOWN, scan.md 5e, and
  the poll is written so a member can replace it without anything else moving
- AN NWF THAT OPENED AND READ EMPTY IS STOPPED, NEVER REBUILT AND NEVER OPENED, F74.
  `NwfComparison.ReadEmpty` gives `RerunDecision.Refused` and a reason naming the file
  and what to do. Rebuilding one threw five federations and every clash result and status
  decision in them away. Treating it as a match would run every test against an empty
  document, which reports every test as passing, and a wrong answer that reads as a good
  one is worse than a stop. `NwfComparison.Compare` must NOT try to answer this: handed
  an empty list it cannot tell an empty NWF from one that has not loaded, because both
  are an empty list, and only the caller knows the NWF was just opened and waited on.
  `RunPath.Stopped` is the label and both the confirm dialog and the RESULT block count
  it. The preview reads the same two rules, because a preview that says Rebuilt about a
  healthy NWF is the confirm dialog lying
- THE DOCUMENT IS EMPTIED AT THE TOP OF EVERY GROUP, BEFORE DECIDE, F75. Before this
  there were two clears in the whole engine and both ran AFTER Decide had read the file
  list, so Decide compared the scan against whatever the previous building had left
  behind. `CensusRule.StartOfGroupLine` and `StartOfGroupReason` are the Core half: the
  first census of a group must read models 0, sets 0, tests 0, results 0, views 0, and a
  group that was emptied and still holds something is named count by count and is not
  DONE, because everything it goes on to read comes out of that document. A count that
  could not be taken is never called dirty, the same way UNKNOWN is never called a move.
  THE OPEN FILE RUN EMPTIES NOTHING, because the document IS the file list there, so it
  passes cleared false, says what it found and is never a fault. The two weekly lines in
  the confirm dialog say nothing INSIDE the NWF is cleared, which is what a person is
  asking about, rather than the old "Nothing is cleared" that F75 made untrue
- A federation that already exists needs NO SCAN. In Navisworks a person opens a file,
  opens Clash Detective, presses Run and reads the results, and nothing asks them where
  their models came from. So there are two ways to run and they are told apart by what they
  name: the Run button in the bar along the bottom, which every step shares, names a count
  of ticked groups, and the Run the open file button on the Clash step names the open FILE
  and the two paths it will write. The open path is
  the same flow with the first step removed. There is no Decide, because the document IS
  the file list, nothing is appended and nothing is cleared, so the clash results inside it
  survive. The clash file is optional there, and without one the tests saved in the
  document are run where they sit, which is the ordinary weekly case, and a document
  holding none runs nothing and the log says so. The outputs are named after the open
  file with the extension swapped and the report in Clash Reports beside it, which is
  exactly where the scanned path puts them, so the same building run either way writes the
  same files. No pattern is applied, because the name is already on the file. Five things
  are checked before it runs and the first that fails is named in the window and the log,
  D2 on 2026-09-12: it has a name, because an unsaved document has nowhere to put an NWD
  beside it, it was opened from a folder and not from an address, it is an NWF, because
  that is where the clash tests and their results live and the NWD this tool publishes
  would otherwise be written over the file that is open, it has a folder in front of its
  name, and that folder can be read from here. The NWD path is never the open path, read
  case blind, as a second lock on the same door. The rule lives in
  Federator.Core.Rerun.OpenDocumentJob so it can be tested without Navisworks
- Distance is the ROUNDED number and carries no number format, because that is what theirs
  holds. Ours stored -0.328083992004395 behind a format of 0.000, so the cell read -0.328
  and anyone sorting, filtering or copying the column got the long value. Rounding the
  value and rounding the display are different things and theirs rounds the value
- The Layer column carries the LEVEL. It was an item property nothing ever filled, so the
  same clash read LGF on the page and nothing in the workbook. Where the item has its own
  layer that wins, and otherwise the clash's level is used
- The Image cell is EMPTY and carries the link. Theirs holds nothing there at all with the
  picture behind it, so writing the file name put a string where their report shows a photo
- A test status of Complete is written OK, because that is their wording. Anything else
  goes through as itself
- The workbook is BANDED, and the colours are theirs, read off the accepted export. Grey
  behind every heading, blue over Item 1 and pink over Item 2, paler on the clash rows than
  on the headings, every cell boxed medium and the test header ruled thick. Ours was plain
  white with no border anywhere, which is the first thing a person sees before they see a
  single number. The five colours, the seven row heights and the nineteen column widths are
  in Federator.Core.Report.ClientLayout, and ClientLayoutTests opens their file and asserts
  every one of them against it, so the table answers to their export rather than to whoever
  typed it
- Their test header is a table NINE columns wide that stops at Status, so L to S on those
  two rows are not cells of theirs and are not expected to carry anything. Their borders
  follow the merge runs, the left edge on the first column of a run and the right on the
  last, which is what keeps our cells identical to theirs where a run is merged
- The report check compares CELL AGAINST CELL: the value, the data type, the number format,
  the fill, the borders, the row height and the column width, with both sides printed. The
  check before this compared which columns, in what shape, in what order, and passed with
  eight visible differences sitting in the file, because none of them was in front of it.
  It walks the first block only, since every block is painted by the same code and 1830
  blocks by nineteen columns is a check nobody reads
- A check that reads the same table the writer wrote from CANNOT catch that table being
  wrong, and this is the third time a check here has reported clean over a real difference,
  so what it cannot catch is written into its pass line rather than left to be discovered.
  Four things. The table being wrong, which only ClientLayoutTests catches by reading their
  file. Anything not in the compared list, which is fonts, the sheet name, freeze panes,
  print setup and merged ranges, all listed in docs\history\scan.md 4q with both sides. Whether a
  value is TRUE, since a number to three decimals off the wrong clash still reads right.
  And a block that was never written at all
- Every check gets a test that BREAKS one thing and asserts the check names it. A test that
  only asserts the good file passes would have passed against all eight of the differences
  above. Fourteen of them live in WorkbookCellCheckTests
- Pipes, ducts, cable trays and their fittings OVER 150 mm go in the viewpoints and
  smaller ones do not, and the large ones of Mechanical and Electrical sit in a sub group
  of their own. 150 is a setting in millimetres, named once in
  Federator.Core.Views.SizeSettings, and over 150 means over: exactly 150 is out. Which
  properties carry a size is a setting too, Diameter, Width, Height, Size, Nominal
  Diameter and Overall Size, tried in that order with the first found winning, because
  which one holds the size differs per kind and per exporter and reading one name would
  drop every item that calls it something else. The number a property hands back is in the
  DOCUMENT'S units and is converted through UnitTable before anything is compared, never
  compared raw: a document in feet reporting 0.5 is 152.4 mm and is IN, and comparing 0.5
  against 150 would put it out while the same model in millimetres put it in, so one
  building would give two different sets of viewpoints depending on a setting nobody
  changed. A unit the table does not know FAILS rather than falling back, which is F33's
  rule. ANYTHING WHOSE SIZE CANNOT BE READ IS INCLUDED, because a fitting usually carries
  no size property at all and dropping it would leave real geometry out of a viewpoint with
  nothing in the output to say so. Every one of them is named in the SIZE block under a
  line saying how many are in for that reason and that nothing was dropped. That is a
  deliberate departure from the rule about logging a count and five examples, which is
  about many lines saying ONE thing: these lines each name a different item that may be
  wrongly in or out, and reading five tells you nothing about the sixth. Naming every one
  is a setting and turning it off makes the block SAY it truncated. The whole rule is
  Federator.Core.Views, SizeSettings, SizeRule and SizeTally, with its tests. The add-in
  reads the properties and calls it and has no opinion about any number in it
- A group about to BUILD an NWF beside a file whose name is nearly the one it would write
  says so, and does nothing about it. On the run of 2026-09-19 Bader got First run on
  buildings that already had an NWF, because the NWF folder and the name pattern together
  did not resolve to his file, and nothing anywhere said so. CLOSE MEANS ONE OF TWO
  THINGS: the same building code sits in both names, or project, originator and building
  all agree and one of the four SUPPLIED fields differs, which is the level, the
  discipline, the type or the number. Those four are exactly the ones NamePattern supplies
  rather than reads, so they are the four a person is most likely to have set differently
  from the file on disk. The rule is Federator.Core.Naming.SimilarNames, it is handed a
  list of names and knows nothing about a disk, and the add-in lists the folder ONCE per
  refresh and hands the names over. An EXACT match is never reported, compared on the stem
  so a name with an extension and one without compare the same, because that file being
  there is what makes the group a Weekly run and not a finding. THE LABEL IS NEVER
  WIDENED: RunPath.Count maps a label it does not know onto Unknown and both the confirm
  dialog and the RESULT block read those counts, so RunAs stays one of the six and the
  sentence rides beside it in RunAsShown, which is what the column and the group list
  block read. One grey line under the group table when any group is in that state, saying
  the NWF Name cell can be typed over. Nothing is auto corrected, nothing is unticked and
  no name is changed. Bader decides, which is the rule this tool keeps for every finding
- A CLASH BECOMES REVIEWED WHEN ALL FOUR ARE TRUE, F72, which answers Q33: one side is a
  SERVICE by item category, the other side is a SOLID by item category, the service
  measures the threshold OR LESS, and the clash is at New or Active. Everything else is
  left exactly as it is and counted by reason. The rule is
  Federator.Core.Clash.PenetrationRule, the lists are PenetrationSettings and the block is
  PenetrationTally, all three with their tests. OFF by default, because it writes into the
  NWF and a project has to say it wants that
- THE 150 IS THE SAME NUMBER F53 USES AND IT IS NAMED ONCE, in
  Federator.Core.Views.SizeSettings. F53 puts an item in a viewpoint when it is OVER the
  threshold, so exactly 150 is OUT there. F72 marks a service Reviewed when it is AT OR
  UNDER, so exactly 150 is IN here. That is not a contradiction: one rule is over and the
  other is at or under, and together they cover every size with no gap and no overlap. Two
  copies of 150 would drift and nobody would notice until a report was wrong, so the
  comment at that one number says both readings side by side
- A DUCT IS NOT ONE NUMBER. The penetration rule reads EVERY size property the item
  carries and takes the LARGEST, through SizeRule.LargestMillimetres, because a 600 by 150
  duct has to fit a 600 through the wall and is a real coordination item. F53 takes the
  FIRST property on the list instead, which is what a viewpoint wants: one representative
  size in a rule nobody has to argue about. The two readings live beside each other in
  SizeRule so neither can be mistaken for the other
- THE UNREADABLE SIZE GOES THE OPPOSITE WAY IN THE TWO RULES ON PURPOSE. F53 INCLUDES an
  item whose size cannot be read, because a fitting usually carries no size property and
  the safe mistake there is showing something unnecessary in a viewpoint. F72 LEAVES ALONE
  a service whose size cannot be read, because the safe mistake here is leaving a clash at
  New for a person to look at. Both reasons are written at both places. Do not make them
  agree
- NEVER OVERWRITE A DECISION. Only New and Active move, which is
  Federator.Core.Clash.StatusesThisToolMayMoveFrom, a rule with its own tests rather than a
  condition inside a loop. Reviewed, Approved and Resolved are all somebody's statement
  about a clash and the NWF is the only record that they made it.
  StatusesThisToolMaySet still answers the other half, which is that Reviewed is the only
  status this tool ever sets, and the two together are what stop a caller moving an
  Approved clash by mistake
- NOTHING MOVES SILENTLY. A PENETRATION block per group names every clash moved with both
  categories and the service size, then the totals and ONE LINE PER REASON for every clash
  left alone, including the reasons at zero, because a reason missing from the block reads
  as one nobody thought of. The run total goes in the RESULT block, but only where the box
  was on, because a line reading zero on every run teaches people to skip it. The workbook
  carries it in the CLIENT'S OWN Reviewed column, per test, because the status is applied
  before the harvest reads it. No column of ours goes on that sheet, which is the rule
  above about the workbook being their one sheet
- THE PROPERTY PROBE READS AND CHANGES NOTHING, F86. `Federator.Core.Probe` holds the
  whole of its rule: ProbeSettings for what is read and where the CSV goes, ProbeTally for
  the counting, the cap and the order, ProbeCsv for the file, ProbeVerdict for the block.
  It takes a folder of NWC files or the open document, and writes one
  `<file>-properties.csv` beside each file with five columns, category, property tab,
  property name, distinct value, how many elements. IT NEVER OPENS AN NWF, never saves and
  never publishes, and `ProbeSettings.MayRead` is what makes that enforceable rather than a
  promise in a comment: an NWF is refused by name because opening one replaces whatever is
  open and the NWF is where every clash result lives, and an NWD is refused because it is
  something this tool writes. THE SEVENTEEN CATEGORIES ARE NOT TYPED. They are the
  thirteen `PenetrationSettings.DefaultServiceCategories` plus the four
  `DefaultNotAServiceCategories`, which together are every category a service discipline
  set in the client's matrix asks for, and a test reads the matrix and asserts it on every
  run. Distinct values are capped at 100 per property, a setting, and what was left out is
  SAID in a line of its own in the CSV and again in the verdict block, never dropped
  silently. Rows are sorted by category, then property tab, then property name, then how
  many elements carry the value, highest first, with the value itself as the tie break so
  the same model gives the same file twice running. Nothing is trimmed, because a value
  with a space on the end is a real thing in these files and the probe exists to find out
  what is really there. The verdict block says whether FS or Fire Suppression appears in
  any tab, name or value, and says so as plainly when it does not, because a probe that
  only speaks up when it finds something reads as one that found nothing rather than as
  one that ran. FS is matched as a WHOLE TOKEN and never inside a word, or OFFSET and
  TRANSFER would both read as fire suppression
- A CATEGORY A SERVICE SET ASKS FOR IS EITHER A SERVICE OR DECIDED NOT TO BE ONE, F72a.
  `PenetrationSettings.IsDecided` is the rule and `DefaultNotAServiceCategories` is the
  second list: Air Terminals, Mechanical Equipment, Plumbing Fixtures, Sprinklers. The
  test that reads the client's matrix asserts the DECISION and not the membership, because
  the literal reading would force Mechanical Equipment onto the service list and an air
  handling unit against a wall would be moved to Reviewed automatically, which is the
  opposite of what the penetration rule is for. A condition written as CONTAINS is a STEM
  and not a category: Cable Tray matches Cable Trays and Cable Tray Fittings, Conduit
  matches Conduits and Conduit Fittings, and all four are on the service list. Air
  Terminals and Sprinklers are arguable both ways and are Q47
- The scan reports what it noticed and never acts on it. ODD SHAPE, NEAR MATCH,
  SINGLE DISCIPLINE and MISSING are information. Nothing is blocked, unticked or
  merged, and no code is assumed right. Bader decides
- Findings are worked out from the run itself. The set of disciplines and the set
  of code shapes come from the files in front of it, never from a list in the
  code, because every project differs. A shape is the pattern of letters and
  digits, so 1B06PK is 9A99AA. A code is odd only when its shape is held by no
  other code and some other shape is shared, because with every shape unique
  there is no majority to differ from
- Findings show in the Grouping step before Run is pressed, and go in the log in
  a FINDINGS block after the group list. Nothing odd is one line, not an empty
  panel

## The diagnostic log

The log is what Bader sends back when something goes wrong, so it is built to
survive the crash rather than to be tidy.

- every build stamps the assembly with the git commit and the moment it was built,
  into AssemblyInformationalVersion, and the log prints it in SESSION and the
  window shows it in its title bar. The plain assembly version is 1.0.0.0 and
  always will be, so on its own it cannot tell a fresh install from a stale one and
  Bader ran an old binary twice before this existed. Never fake this with a hand
  edited version number. It costs a full recompile on every build, which is about
  three seconds, and that is the trade

- every line is written and flushed all the way to the disk as it happens, with
  FileStream.Flush(true). Nothing is held back to the end, so a process that dies
  inside a Navisworks call still leaves everything up to that moment on disk
- it opens on the first line of the button handler, before the folder is read and
  before the window opens, so a run that dies at startup still produces a file
- two places, always. The fixed path
  %LOCALAPPDATA%\ParsonsNwcFederator\logs\run-yyyyMMdd-HHmmss.log never depends on
  a folder the user picked, and a copy goes next to the NWF folder at the end. If
  that copy fails, the reason goes in the first log and the run carries on
- logging is never the thing that stops a run. That holds for the copy, for
  retention, and for the log file itself, which falls back to the temp folder and
  then to window only output
- a size is only logged after File.Exists passes and the real size is read back.
  Never log a size that was not read
- on start, the oldest logs are deleted until 30 remain, the live file included.
  The live file is never a candidate. A delete that fails writes one line naming
  the file and the reason. 30 is a setting, RunLog.KeepLogs, static because the log opens
  on the first line of the button handler before a window or any options object exists.
  Zero keeps the live file alone and is a real answer, fewer than none is refused

### The steps, F59

Where the time went is the question the log exists to answer, and it can only answer
it if the same work carries the same name every time it is timed.

- the step names live in `Federator.Core.Diagnostics.RunSteps` and nowhere else. Nothing
  types a step name as a string. Fourteen of them, in the order a group meets them:
  DECIDE, APPEND, NWF SAVE, UNITS, SETS, TESTS CREATE, TESTS RUN, HARVEST, IMAGES,
  WORKBOOK, HTML, XML, NWD, CONFIRM. A name that is not on the list is refused where the
  step opens, because a timing block holding a step nobody named is worse than a short one
- a step is always opened in a using block, so it closes on the way out whether the work
  finished, returned early or threw. A step left open is the one thing that would make
  the block lie, because seconds it never recorded come off no total and the run reads as
  faster than it was. A group that ends with one still open names it and says NEVER CLOSED
- a step whose work threw says THREW and still carries its seconds. Time spent failing is
  time the run spent
- the clock is monotonic, `Stopwatch` through `RunLog.ElapsedSeconds`, and never two wall
  clock readings subtracted. A clock that goes back, which is what a machine syncing its
  time does, would otherwise give a step a negative duration and a group a total smaller
  than one of its own steps. A step never reports less than no time
- one clock per piece of work. The step IS the measurement, so where a Stopwatch was
  already timing the same thing it went, and the seconds the log reports and the seconds
  the report row carries come off one reading
- a step entered more than once in a group writes its start and finish pair the FIRST
  time, one line the second time saying the rest are counted, and nothing after that. The
  totals go in one line per repeated step when the group finishes, with the visits and the
  seconds. TESTS RUN is entered once per test and a real group holds 1830 of them, so the
  pair per visit would be 3660 lines. This is the same rule the repeated failure follows
  and it is here for the same reason, a run that once left a 17.8 MB log
- a step inside a step is indented by its depth and carries that depth, so the timing
  block can work its shares out over the top level alone. IMAGES is the only one nested
  today, inside HARVEST, because a picture is written while the harvest walks the results.
  TESTS CREATE, TESTS RUN and HARVEST each open and close on their own
- outside a group there is no group to count visits against, so each press of the two
  hand buttons on the Clash step is its own occasion and writes its own pair of lines
- the step never changes what the run does. It opens, the work runs exactly as it did,
  and it closes. Nothing is skipped, reordered or waited for

### The timing blocks, F60, which closes F21

- a TIMING block per group, written straight after its GROUP finished line, and a
  TIMING, THE WHOLE RUN block just before RESULT so RESULT stays the last thing in the
  file. Both are `Federator.Core.Diagnostics.TimingBlock` and nothing else builds them
- every number is measured. The group total is the engine's own clock as GroupFinished
  read it, and the run total is the log's elapsed clock read where the block is written,
  NOT the groups added up, because the scan and the preview happen outside every group
  and a total that left them out would be smaller than the run took
- whatever the steps do not account for is a ROW of its own, named outside every step,
  so the share column reads down to a hundred. A block that spread it over the steps it
  does know about would be inventing numbers, and one that left it off the page would
  leave a reader guessing. There is a test that adds the column up
- steps adding to MORE than the group took is said in words. It means something was
  timed outside the stretch the group clock covered, which is a fault in the timing and
  not in the run
- a nested step is left out of the share column and listed under the total, because its
  seconds are already inside its parent's and counting both would give a group shares
  adding to more than a hundred
- the run block reads twice: the groups slowest first, then the same seconds by STEP
  NAME added across every group. Which building cost the run and which step cost it are
  two questions and only the second one says what to fix
- the run block ends by saying whether the run fitted in the forty five minutes
  criterion 2 asks for, with the measured time in minutes and in seconds either way. One
  second over is OVER. The forty five is `TimingBlock.UnattendedSeconds`, a setting, and
  a value at or below zero is refused where it is set
- one line per test, `ROWS`, carries what the workbook got beside what the document
  holds, so criterion 3 is answered off the log rather than by opening Excel and
  Navisworks side by side for 1830 tests. The two numbers are not the same thing: the
  tally counts leaves, descending into every result group, and the harvest writes one
  row per group without descending, which is also what the panel shows. So FEWER rows
  than clashes is the grouping and the line says so. MORE rows than clashes is a finding
  nothing in this tool explains, said in capitals, and nothing acts on it

### The document census, F61

Five counts read out of the open document, before and after a step, so a count that
moved where nothing should have moved it is named rather than discovered a week later.

- the five are models, selection sets, clash tests, clash results and saved viewpoints.
  Every one is something the NWF carries and something there is no second copy of
- A COUNT THAT COULD NOT BE TAKEN IS MINUS ONE AND NEVER ZERO. Zero reads as a real
  count and would let a run throw 1830 clash tests away and report that nothing moved.
  A census holding a minus one says UNKNOWN for that count, and a count either census
  could not take is never called a move, in either direction
- `CensusRule` says which step may move which count, and the useful half is the
  refusals. DECIDE opens the NWF that is on disk, and opening a document replaces
  everything in it, so all five may move there and only there. APPEND moves the models.
  SETS the sets, TESTS CREATE the tests, TESTS RUN the results. Every other step writes
  a FILE and not the document, so none of them may move anything
- THE RULE HAS THREE ANSWERS AND NOT TWO, F73. `CensusRule.Judge` gives Allowed, Noted
  or Refused, and `CensusMove` is the enum. Allowed is the step doing the job it exists
  for and writes nothing. Refused is the fault. NOTED is a count a step moves while
  doing its own work, which writes a line and no reason. APPEND raising the saved
  viewpoints is the one noted pair today: an NWC exported from Revit carries that
  model's saved viewpoints and appending it brings them in. On the first real run that
  was refused, so all seven groups were reported FAILED while 28 files had been written
  correctly. `MayMove` is the narrow reading, Allowed only, and `StepsThatMayWrite` is
  built from it, so a side effect never widens what the census is taken around
- a move the rule REFUSES gets a line beginning CENSUS CHANGED, naming the step, the
  count, the before and the after, and the reason goes on the group so it is not
  reported DONE. A move it NOTES gets a line beginning CENSUS NOTED with the same
  numbers, a sentence saying why that step moves them, and no reason at all, so the
  group can still be DONE. Nothing is undone, nothing is skipped and the run carries on.
  The tool reports what it noticed and Bader decides
- THE CENSUS IS TAKEN AT MOST ONCE PER STEP PER GROUP, on the first visit, for the same
  reason the start and finish lines are written once. TESTS RUN is entered 1830 times
  and counting the whole document around every visit would be the log slowing the thing
  it is watching
- what the census COSTS is measured off the same monotonic clock and said once per
  group. Over `CensusCost.TooLongSeconds`, one second by default and a setting, the
  census narrows to the steps that may write and the next group says so at its top, so
  no reader ever wonders why a step has no census around it
- `DocumentCensusReader` in the add-in is the ONE place that reads all five. Each is
  walked and not asked for, every wrapper disposed on the way down, and each read is in
  its own try so one count failing does not turn the other four into UNKNOWN. The set
  walk `FederationEngine.CountSets` used to carry moved into it, so the rebuild and the
  census read the same number the same way
- clash results are counted as LEAVES, descending every result group, the same rule
  `ClashRunner` counts by. A result group counting as one would let a run lose every
  clash inside it and report the same number

### The gap block, F63

Bader's standing rule built into the tool: when the code knows something the report
does not show, it becomes a question. That rule was worked by hand every time, and Q25
is exactly it, found by an audit of every file under src. The run says it itself now.

- a GAP is something the run MEASURED off the model that no output carries. Not
  something the code could have measured and did not, and not something the report
  leaves out on purpose that a reader can see anyway. It has to be a number the run
  paid for and then threw away
- a gap names three things: what is missing, what it came to on this group, and where
  it would belong. A gap with no value is a complaint and one with no place to go is a
  shrug
- a property NOTHING carried is not a gap. Reporting it would say the run is holding
  back something it never read
- the block is written even when it is empty, saying nothing was held back, because a
  missing block reads as a check that did not run
- nothing acts on it. A gap does not fail a group, does not stop a run and does not
  change an output. It is information in the log, which is the rule this tool is built
  on
- the run line counts gaps by NAME and not by line. The same ones are held back on
  every group, so a run of twenty two groups would report 132 and say nothing except
  how many groups there were. The number that means something is how many distinct
  things this tool knows and does not show
- the six were MEASURED on 2026-09-19 by reading all three writers. WorkbookWriter,
  HtmlTabularWriter and ClientReportColumns name none of Family, Type, Material,
  SourceFile, Discipline or IdFrom anywhere, and ClashReportXml writes two quick
  properties and says in its own comment that the five used to be written and are not

### The machine readable log, F64

A second file beside the text log, same name and a different extension, `.tsv`. One row
per event, eight columns, tab separated, with a header.

- the text log is STILL the one a person reads and nothing about it got worse for this
  file existing. Where the two disagree the text log is right, because it is the one
  that has been read against a real run
- ONE WRITER, so the two cannot drift. `RunLog.Numbered` writes the text line and the
  row together and is the only way a line carrying a number reaches the log. A line
  cannot be added without its row and a row cannot say something the text log does not
- a sentence with no number in it writes NO row. That is the rule and not an oversight:
  a row whose number column is empty is noise in a file whose whole purpose is numbers
- tab separated and not comma, because a clash name, a set locator and a file path all
  hold commas and none of them holds a tab, and Excel opens a tab separated file with no
  import dialog
- a tab, a newline, a carriage return or a backslash inside a value is escaped. The
  BACKSLASH GOES FIRST, or reading a value back would turn a path ending in a t into a
  tab, and every Windows path in this tool holds backslashes. Escaping is proved by
  reading the value back and not by asserting that it changed
- line by line and flushed with `Flush(true)`, exactly like the text log, so a run that
  dies mid group leaves both files whole up to that moment
- it never stops a run. A row file that cannot be opened leaves the text log untouched
  and says why in it, and a write that throws is swallowed. The second log is a
  convenience and the first one is the record

Two things that look like mistakes and are not:

- while a run holds the log open, File.ReadAllText fails with a sharing error.
  Anything reading a live log opens it share-aware. Notepad and the Copy log
  button both work. There is a test pinning this
- the header is two blocks on purpose. SESSION is written at button press and
  proves the button fired even if the scan never ran. RUN SETTINGS is written at
  Run because the folders and the counts do not exist until the scan finishes.
  Do not merge them
