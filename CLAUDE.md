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
Outputs keep the same container name with BM as the discipline code, ZZZ for the
level, and the number 000001, because outputs overwrite.
The split character and the part positions are settings, never constants.

## What the clash test XML holds

Measured from the real file. Do not re-derive this.

- root exchange, units="ft", one batchtest, 1830 clashtest children
- every test: test_type="hard_conservative", status="new",
  tolerance="0.2460629921", merge_composites="1"
- that tolerance is 75 mm. Tolerance is in the file units, convert to document
  units before use
- each side is one clashselection holding one locator, a name path such as
  lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals
- paths nest. Mechanical has 4 subfolders. Walk the folders, never assume the
  sets tree is flat
- 61 unique sets. 1830 is every pair of 61 with no self pairs
- set names contain spaces and ampersands. Match the exact string
- linkage is none and rules are empty in every test here. Read both anyway,
  another project will use them

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

## Build

Fill this in from docs\scan.md once the scan has run. Do not guess the command.

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