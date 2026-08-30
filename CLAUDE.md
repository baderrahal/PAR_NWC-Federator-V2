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
- Outputs overwrite. No date suffix, no version suffix
- Excel sheet names stop at 31 characters and 1703 of the 1830 test names are
  longer. Sheets are T0001 upward. The Summary sheet carries the full test name,
  the counts by status, and a link to the sheet
- Images are off by default. When on, New and Active only, written as jpg beside
  the workbook with a link in the row, never pasted into cells

## The diagnostic log

The log is what Bader sends back when something goes wrong, so it is built to
survive the crash rather than to be tidy.

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