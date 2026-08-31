# Parsons NWC Federator

Navisworks Manage 2025 add-in. Reads a folder of discipline NWC files, groups them
by building, saves an NWF and an NWD per building, imports a clash test XML, runs
the tests, and writes one Excel report per building.

## Host and target

- Navisworks Manage 2025 only. API series Nw22, bundle folder Contents\v22
- .NET Framework 4.8. Navisworks has been on 4.8 since 2021 and 2025 did not move
- References are the DLLs in the Navisworks install folder, copy local false:
  Autodesk.Navisworks.Api.dll and Autodesk.Navisworks.Clash.dll
- Installs to %APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle
- 27 people run it. No compiler on their machines, no admin step, no path that
  exists on one machine only

## Done means all three

1. One building federates, clashes and writes Excel with no errors
2. Every ticked building runs unattended in under 45 minutes
3. Clash counts in the Excel match the Clash Detective panel exactly

## File naming

Input:  1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc
Split on the hyphen. Part 3 is the building, part 5 is the discipline.
Group on the full 6 character building code. 1C07BC and 1C07K1 are two buildings.
Every discipline of a building goes into that building's federation. Discipline is
read for reporting only, it never splits a group.
The output name is project, originator, building code, ZZZ, BM, MOD, 000001.
Level, discipline, type code and number are all pinned, because outputs overwrite
and the files in a group may disagree on any of them. Only parts 1, 2 and 3 are
copied from the input, and part 5 is read for reporting.

The output name is built, not patched. That means a five part input still gives a
full seven field output name, and the readable-name floor is five parts. A six part
name reads fine.

If two files in one group disagree on the project code or the originator, report
it and skip the group. Do not pick one.

The split character, the part positions and all four pinned values are settings,
never constants.

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
  value, with flags left out. This is what HealthCheck.DistinctRuleCount counts,
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
  not passed. Skipped and passed appear as different numbers in the Summary sheet
- Each building writes its NWF, NWD and Excel before the next building starts.
  A failure part way through keeps everything already written
- Never clear and rebuild an NWF that already exists. This is the one rule most
  likely to be helpfully undone by someone who does not know why it is here.
  An NWF holds pointers to the NWC files, not copies, so a model updated in place
  needs no rebuild. The clash results live inside that NWF and are the only record
  of what has been fixed. Rebuilding it resets every clash to New and loses every
  Active and Resolved. This tool runs weekly, so that is a week of review thrown
  away each time. Three cases per group:
    no NWF at the output path, build it, there is no history to lose
    NWF there and its file list matches the group, open it, do not clear, do not
      re-append, log OPENED
    NWF there and the file list differs, log CHANGED naming every file added and
      every file removed, and touch nothing. Bader decides
  The file list is read out of the opened NWF. No side file records what went in,
  because a side file can disagree with the NWF and the NWF is the record
- The file list is read from Model.FileName, never from Model.SourceFileName.
  FileName is the NWC, which is what the scan holds. SourceFileName is the
  container the NWC was published from, which on this project is a Revit file in
  Autodesk Docs such as
    Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt
  That can never equal a scanned NWC path, so comparing on it reported CHANGED for
  22 of 22 groups on a run where nothing had changed, and because a CHANGED group is
  left alone entirely, no NWF was reused, no set was built and no test ran. Both
  names still go in the log whenever they disagree, which is what made this
  findable. The rule lives in Federator.Core.Rerun.ModelFileNames so it can be
  tested without Navisworks, which is why it went unnoticed in the first place
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
- If no test can resolve a set, say so and stop before creating anything. One line
  naming how many sets the document holds and how many the tests name. A run against
  a document holding zero sets once went ahead anyway and finished with 0 created and
  0 run. This is a guard, visible in the log and in the window, never a silent skip
- The Run button does the whole job for every ticked group, model side and clash
  together, and the model side alone when no clash file is picked. The two buttons on
  the Clash step are for trying one open model by hand and are labelled as that. They
  are not steps in the run. Splitting one job across three presses is what let a user
  run clash against a document with no sets in it
- Republishing the NWD is a tick box, on by default, and happens in all three
  cases. That is the point of a rerun. The NWF pointers are unchanged, so
  reopening picks up whatever the NWC files now hold and the NWD is refreshed
  without the clash history being touched
- The clash test file is picked at run time, every run, and can be from any project.
  Nothing about any one file is written into the code, not names, not counts, not
  property internal names. Those appear in tests as sample data only. A file may hold
  sets, tests, or both, and all three are normal. There is one picker, because Bader's
  file holds both and two boxes meant picking the same file twice
- Per group the order is append, save the NWF, build the sets, create the tests, run
  them, save the NWF again, publish the NWD last. The sets, the tests and the results
  all live in the NWF, so an NWD published before the clash work ships without any of
  them. The NWD used to go first and that is what this ordering fixes. The second
  save only happens when the clash step actually put something into the document
- Nothing is created twice on a rerun. A clash test already in the document under the
  same name is left exactly as it is, because that is where its Active and Resolved
  clashes live, and a set already at its path is left alone too, because a second copy
  would leave two sets at one path and a locator resolving to whichever came first.
  Both are counted and reported as already there, separately from what was created
- A CHANGED group is left alone entirely, so no set is built into it and no test
  created, the same as its NWF not being touched
- Per test, everything comes from the file and never from a constant: the name, the
  test type, the tolerance, merge composites, and per side the self intersect and the
  primitive type flags
- Tolerance is read per test and converted from the file units attribute into the
  units of the open document before it is set. There is no global tolerance setting
  in this tool. Both numbers and both unit names go in the log, because which units
  ClashTest.Tolerance is measured in is UNKNOWN until a test runs against a real model
- A test type that does not map to a value on the enum is reported by name and
  skipped. It is never approximated to the nearest one, because a hard test standing
  in for a clearance test reports a number that reads as real and is not
- A clash side is pointed at the saved set through CreateSelectionSource, not filled
  with a copy of the set's items. That is what Clash Detective does when a person
  picks a set in the panel, and it is what keeps the counts matching the panel. A
  copy is a snapshot that can drift from the set the panel shows
- Everything that threw for one group is kept in a list, never in one slot. The model
  side can append cleanly and the clash step still throw, and one slot kept whichever
  wrote to it last and silently lost the other
- A handle onto anything the document owns is borrowed, never kept. Every SavedItem read
  out of a collection is created with eEXTERNAL ownership over a weak reference, so it
  dies the moment the document replaces the object behind it, and every mutator on
  DocumentClashTests is a copy form that does exactly that. TestsRunTest is one of them,
  so the test handed to it is dead when it returns and reading Children off it throws
  ObjectDisposedException (WeakRef). A test is addressed by where it sits, resolved again
  before every use, and disposed after. One run threw that exception once per test for
  8 hours 52 minutes and produced nothing
- Nothing walks a whole collection once per item. Looking a test up by name after every
  add was O(n squared) over 1830 tests and built about 1.7 million finalizable native
  handles per group. TestsAddCopy appends at the root, so the new test is at the index the
  count held before the add, checked by name rather than assumed
- What this tool creates or resolves, it disposes. All of ClashTest, ClashResult,
  ClashSelection, SelectionSet, SelectionSource, Selection, ModelItemCollection, Search
  and SavedItem are IDisposable. Disposing an eEXTERNAL wrapper releases the wrapper and
  never the document's object. SavedItemCollection is not disposable and is never disposed
- A run that is failing everything stops the run, not the group. After the first 50 tests,
  if every one has failed for the same reason, the whole run stops and says so in one
  line. Every group of that nine hour run failed the same way, so a per group stop would
  have saved none of it. The 50 is a setting. A test skipped because a side finds nothing
  is the ordinary answer and counts neither way
- An exception repeating with the same heading and the same trace is written out in full
  once and counted after that, and the RESULT block carries the total beside the one
  trace. One run wrote the same stack tens of thousands of times into a 17.8 MB log
- One picker, one file. The file can hold sets, tests, or both, and the tool reads what is
  in it. A file holding only tests still works against sets already in the model
- Outputs overwrite, the NWD every run and the NWF only when it is being built for
  the first time. No date suffix, no version suffix
- A group ends in one of three states, and the test is always what was ASKED FOR,
  never what happens to be on disk. A step deliberately switched off is not a
  failure. Judging a group by whether an NWD existed, with republishing switched
  off, once reported all 22 groups of a clean run as FAILED.
    DONE     everything requested for this group succeeded
    PARTIAL  something requested did not complete, or the group was CHANGED
    FAILED   something requested threw or produced nothing
  The rule lives in Federator.Core.Rerun.GroupJudgement, with no Navisworks types
  in it, so it can be tested. The outcome and the reason for it come out of one
  pass, so the two can never disagree
- The counts in the RESULT block and the errors under it come from one list. A
  failed count with an empty error list is what the log printed once, saying
  "groups failed: 22" and "Nothing failed." in the same block. A group recorded as
  failed always carries a reason, and one is substituted rather than thrown over
  when a caller forgets, because logging never stops a run
- Only a file this run actually wrote goes in the files written list. Outputs
  overwrite with no date suffix, so last week's NWF and NWD sit at exactly the
  paths this run uses. A group that threw before writing anything must not list
  them as its own, and a file that was checked rather than written is logged with
  CheckOnDisk, which reports the size and records nothing
- The Try forms return a bool and it is read, never discarded. For the NWD that
  bool is the only thing separating a fresh publish from last week's file at the
  same path, because File.Exists cannot tell them apart
- One workbook per group, written after the clash step and before the NWD is published,
  so the three outputs of a group agree with each other. Named like the group's other
  outputs with an xlsx extension, and it overwrites, the same as the NWF and the NWD. The
  Excel folder is picked on the Outputs step, and when it is empty the workbooks go beside
  the NWF folder in a subfolder called Clash Reports, which is a setting
- Results are grouped, not one row per raw clash. A group is one row and its distance is
  the most severe clash in it, which is the minimum: a hard clash reports a negative
  overlap so the worst is the most negative, and a clearance test reports a gap so the
  worst is the smallest. Every row carries the raw count behind it, so the grouping hides
  nothing
- The clash API has no open against closed notion. Nothing on IClashResult, ClashResult,
  ClashResultGroup, ClashTest or DocumentClashTests names one, ClashResultStatus is a flat
  five value enum, and Navisworks' own report does not mention open or closed either. So
  every status is reported as itself, and where the matrix needs one number for what is
  outstanding it is labelled New plus Active rather than open, because that is a rule
  Bader stated and not one the API holds
- One sheet per test that found something. A test that found nothing gets no sheet and
  appears in the Summary as its status. Sheets are T0001 upward because Excel stops a
  sheet name at 31 characters and 1703 of the 1830 test names are longer, so a sheet is
  never named after its test and the full name lives in the Summary
- The Summary carries one row per test in the file, not one per test that ran. Skipped,
  passed and found are three numbers and are never merged. On 1C07BC that is 1164 skipped,
  618 passed and 48 with clashes, and those add to 1830
- The matrix cell holds New plus Active. A skipped pair reads "skipped" and never "0",
  because a zero says the pair was tested and nothing clashed, and a skip says nobody
  looked. A pair no test covers is blank, which is a third thing again. The discipline
  grouping is read from the set folder names in the picked file, never from a list in the
  code
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
- Images are off by default. When on, New and Active only, written as jpg beside
  the workbook with a link in the row, never pasted into cells
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
  the file and the reason. 30 is a setting

Two things that look like mistakes and are not:

- while a run holds the log open, File.ReadAllText fails with a sharing error.
  Anything reading a live log opens it share-aware. Notepad and the Copy log
  button both work. There is a test pinning this
- the header is two blocks on purpose. SESSION is written at button press and
  proves the button fired even if the scan never ran. RUN SETTINGS is written at
  Run because the folders and the counts do not exist until the scan finishes.
  Do not merge them

## Build

Filled in from docs\scan.md after the scan ran on 2026-08-27.

Everything, which needs Navisworks on the machine because of the add-in project:

    dotnet build ParsonsNwcFederator.sln -c Release

The parts that need no Navisworks, which is all of Federator.Core:

    dotnet test tests\Federator.Core.Tests\Federator.Core.Tests.csproj

There is no Visual Studio and no .NET Framework targeting pack on this machine, so net48
compiles only through the Microsoft.NETFramework.ReferenceAssemblies package. Every
project references it. That makes nuget.org a hard requirement for building at all.

The add-in finds Navisworks through the NavisworksPath property, default
C:\Program Files\Autodesk\Navisworks Manage 2025. Override it when the install moved:

    dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"

Any check for a Navisworks file tests that one full path directly. Never search the
install folder, never recurse it, never wildcard it. Exists() in MSBuild and Test-Path
on a joined path are the only two forms used, and each referenced DLL is checked on
its own so a missing file is named. A recursive walk of that folder was investigated
on 2026-08-30 and works fine, so this rule is about keeping the check direct and
self-diagnosing, not about working around the machine. docs\scan.md holds the
measurements.

Tests are NUnit. The pre-commit hook runs them and refuses the commit on a failure. Turn
it on once per clone with:

    git config core.hooksPath .githooks

## Tests

Testable here, so these get real tests: the name parser, the building grouping,
the clash XML reader, the sets XML reader, the Excel writer.
Not testable here, so these get review instead of tests: anything calling the
Navisworks API. Write those in larger reviewed pieces, not many small attempts.

## Confirm against the install, do not assume

- exact signatures of Document.AppendFile, SaveFile, PublishFile and Clear
- the ClashTestType value that matches hard_conservative
- the Clash Detective report defaults, so the tool starts where Navisworks starts
- NwdExportOptions is a 2026 class. It does not exist in 2025

## Writing

No generated-by or co-authored-by line. No emoji. No em dash. No semicolon in
prose. No comment that repeats the line under it. Say UNKNOWN rather than filling
a gap. Never report a check that did not run.