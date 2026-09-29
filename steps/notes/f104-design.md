# F104 design: the separate read of the document

Written by a planning agent on 2026-09-29, read off the working tree of fix-F100, and saved
by the lead as returned, lightly shortened where it repeated itself. Design only, nothing
built. Every UNKNOWN waits on section 4.

## What the reading found first

1. What the workbook's per test numbers count, read off the code. Clashes counts LEAVES:
   each row carries RawClashes, the leaves under a group or 1 for a plain clash, and an
   EMPTY group counts as 1 (ClashHarvest.cs 164, ClashReportModel.cs 406 to 419,
   WorkbookWriter.cs 289 and 330). New to Resolved file every leaf of a group under the
   GROUP'S OWN STATUS, not the leaf's (ClashReportModel.cs 396, WorkbookWriter.cs 291 to 294
   and 332 to 335). The rows are the top level results, one row per group, the harvest does
   not descend (ClashHarvest.cs 126 to 149). So a group whose clashes carry mixed statuses is
   the one place the workbook's status cells and the document's own statuses part
2. What the PANEL shows for a result group has never been read off the panel. scan.md 4f
   line 517 says a group counts as the one result the panel shows, from reflection over the
   types. ReportedCount.cs says the same, measured off the code. The ROWS rule in core.md
   rests on both
3. read-workbook.ps1 writes only under steps\runs (lines 49 to 53). Left as it is, F104
   reads its read-outs where they are
4. The probe route, AddPluginAssembly then ExecuteAddInPlugin, was measured on the machine
   of 2026-09-19 (5j). On this machine 5z-d called AddPluginAssembly and never
   ExecuteAddInPlugin (result line 428), so it is UNKNOWN here
5. The names UnitConversion and ScaleFactor are in Autodesk.Navisworks.Api.dll, found by
   grep, signatures not read

## 1. The probe plugin

tools\probes\DocumentReadProbe\DocumentReadProbe.csproj in ViewpointProbe.csproj's shape:
net48, LangVersion 7.3, library, NavisworksPath, CheckNavisworks target, the reference
assemblies package, references Autodesk.Navisworks.Api and Autodesk.Navisworks.Clash only,
Private false, SpecificVersion false, plus WindowsBase for the pump. No reference to
Federator.Core or Federator.Addin, and a comment saying why. Not in the solution, the bundle
or install.ps1. Built with dotnet build tools\probes\DocumentReadProbe\DocumentReadProbe.csproj
-c Release. One file, DocumentReadProbePlugin.cs.

[Plugin("DocumentReadProbe", "PARS")], [AddInPlugin(AddInLocation.AddIn)], id
DocumentReadProbe.PARS. Execute(params string[]): parameter 0 the output folder, 1 onward
the NWF paths. Returns 0 every read-out whole, 1 one says READ-OUT FAILED or REFUSED, 2 fewer
than two parameters, 3 the output folder missing or not under
%LOCALAPPDATA%\NwcFederatorLoop, 4 two NWFs share a file name. Nothing written on 2 to 4.
The number is a hint, the read-out files are the proof.

Guards inside: the output folder and every NWF must sit under
LocalApplicationData\NwcFederatorLoop, compared case blind with a trailing separator, which
keeps every live folder out without naming one. An NWF must end in .nwf and exist. A
read-out that exists is never written over.

Per NWF, one read-out <name>-document.txt written as .partial with AutoFlush and moved into
place after END OF READ-OUT:
1. header: probe stamp, Application.Version, Application.IsAutomated, the parameter as it
   arrived, the NWF size, UTC write time and sha256 read before the open
2. TryOpenFile(path), false is READ-OUT FAILED, the seconds it took, Document.FileName after,
   a doubt when it is not the path asked for
3. Document.Units by name, and metres per unit from UnitConversion.ScaleFactor if PQ2 finds
   it, else UNKNOWN
4. two passes, the second after 5 s pumping the dispatcher every 250 ms, both totals written,
   any count differing between them a doubt, the tables written from pass 2
5. tests, results, totals, doubts, END OF READ-OUT

One pass reads: models count. Selection sets walking SelectionSets.RootItem, a SelectionSet
counts one, any other GroupItem a folder. Saved viewpoints walking SavedViewpoints.RootItem.
Tests depth first over GetClash().TestsData.Tests, ClashTest tested before GroupItem because a
test is a GroupItem (4f), a non test GroupItem a test folder, anything else a doubt. Per test:
position across the tree, folder path, DisplayName, TestType, Status, Tolerance. Per test its
Children in order: a ClashResultGroup is one top level result of kind group with its leaves
counted by descending every group, each leaf at its own status. A ClashResult is one top level
result of kind clash and one leaf. Anything else kind other and a doubt. Nested and empty
groups counted and written. Per test tallies: top level and top level per status (a group at
its own status), leaves and leaves per status (each clash at its own), groups, empty groups,
nested groups, other. Per top level result: position, kind, name, status, leaves and leaves
per status, and for a plain clash the Distance in document units and in metres. A group's
distance is a dash, the probe does not copy the harvest's rule.

Format: tab separated with column words on header lines, a tab or line break in a name read as
one space as read-workbook does, invariant culture, round trip doubles.

Handles: every SavedItem in a using, both RootItems included, nothing kept past the loop that
read it, a pass holds strings and numbers only. DocumentClash not disposed, as
DocumentCensusReader.cs 133. NO MUTATOR: TryOpenFile is the one call that changes what
Navisworks holds. Never Clear, Save, Publish, Append, any DocumentClashTests member starting
Tests, any AddCopy, Remove, Edit or Move, SetHidden, CurrentViewpoint or
SetModelUnitsAndTransform. The pull request carries a grep for each reading zero, and the
driver's sha256 of every NWF before and after proves nothing on disk moved. No catch that
swallows: a throw is a doubt with its type and message and the counts it could not take are
-1, read as UNKNOWN and never zero. ViewpointProbe's WorksetsIn and SiteOn swallow and nothing
is copied from them.

Members, MEASURED with their scan.md section or UNKNOWN: the plugin route 5j on the other
machine only, UNKNOWN here (PQ1). NavisworksApplication constructor, AddPluginAssembly,
Dispose 5z-d, ExecuteAddInPlugin exists and never called here (PQ1). ActiveDocument 4b and 5j.
Version 4b 4c. IsAutomated member 4b, value UNKNOWN. TryOpenFile 4e, used in Automation 5j 5x
5z-b, whole on return 5e on window opens elsewhere, UNKNOWN here (PQ7). FileName member 4b.
Units 4f 4q. ScaleFactor UNKNOWN (PQ2). Models.Count 5j 5x. SelectionSets.RootItem 4d, counted
5z. SavedViewpoints.RootItem 5d, counted 5x. GetClash and TestsData 4f. Tests 4.
SavedItemCollection and eEXTERNAL disposal 4g. DisplayName and Children 4. TestType,
Tolerance, Status 4, Tolerance units UNKNOWN (PQ3). ClashResult, ClashResultGroup,
IClashResult Status, DisplayName, Distance 4f, Distance units UNKNOWN (PQ3), panel count of a
group UNKNOWN (PQ4). The pump is FederationEngine.cs 851's, needed or not UNKNOWN (PQ7).

## 2. The comparison

tools\loop\compare-document.ps1, its own script: how a workbook is read stays in
read-workbook.ps1, how a document is read is the probe, what agreeing means is here, and only
the two read-out files pass between them, so it is proved with no Navisworks and no xlsx.

compare-document.ps1 -Workbook <read-out> -Document <read-out> -Out <.txt>
[-PictureStatuses New,Active,Reviewed] [-PictureCap N] [-PriorityPicked]

Refuses with one COMPARISON FAILED line when either input lacks END OF READ-OUT or carries
READ-OUT FAILED or REFUSED, a header word is missing, or -Out is not a .txt under the work
folder or equals an input. Columns found by header words, never position. Written beside and
moved.

Tests paired by name, Ordinal, never trimmed. A name twice on either side is a doubt and
neither copy compared. Four classes: in both. In the workbook only with every number zero, the
tests F77 did not create, counted. In the workbook only with a number above zero, DIFFERS. In
the document only, DIFFERS with its leaves.

Checks for every test in both, every count exact, a difference of one is a line:
1. rows under the block against top level results
2. Clashes against leaves
3. each of New to Resolved against leaves at that status, each leaf at its own, the top level
   count shown in brackets
4. row by row in sheet order against top level results in document order, name then status
5. the tolerance unit reads exactly m on every workbook test, and the number against the
   probe's tolerance in metres within 0.0005, the workbook's rounding and nothing else
6. a clash kind row's distance against the probe's in metres within 0.0005, which catches a
   row left in feet under a label reading m
7. pictures, worked out from the workbook read-out alone and tied to the document through
   check 4: blocks in workbook order, the test number counting blocks with a picture link from
   0, the clash number counting pictured rows from 1, every link
   <workbook name>_files/cd<test 00><clash 0000>.jpg (scan.md 4k) and on disk. With
   -PictureStatuses every row at those statuses carries a link, with -PictureCap only the
   first N per test
8. block order unless -PriorityPicked: among blocks with a clash, Clashes never rises, equal
   Clashes in document order, blocks with none not placed
9. totals against the document's, and inside each workbook test New to Resolved add to Clashes

Under a DIFFERS line, as information: every group whose leaves are not all at the group's
status, with both statuses, and every empty group. The one switch after PQ4 and PQ5: check 3
compares each clash at its own status, and if the panel and the export file a group's clashes
under the group's status, the target becomes that, one variable, the probe unchanged.

Shown and never judged: test type and test status, because the workbook carries the client's
words and judging them needs a second copy of that wording rule. Models, sets and viewpoints
counts in the header, for the log-reader beside the census lines.

Output: header naming both read-outs with sha256 and the switches, one line per test pair,
DIFFERS lines such as DIFFERS <test>  New: workbook 3, document 2 clashes at New (top level 2),
NOT COMPARED lines, doubts from both read-outs and its own, then DISAGREEMENTS N, NOT
COMPARED N, DOUBTS N, VERDICT AGREE, DISAGREE or NOT PROVED, END OF COMPARISON. AGREE only
when all three are zero. NOT COMPARED: a -1 count, a tolerance with no metres per unit, the
pictures when the workbook's folder is not on this machine, which rows should have a picture
when -PictureStatuses was not given. What it cannot catch, printed above the verdict: a fault
in the Navisworks .NET API itself, since both read through it, the export in 5a is the witness
for that. Whether a picture shows its clash. The clash point, grid and item columns. Anything in
4k's numbering both misread the same way.

## 3. How it is driven

tools\loop\check-documents.ps1, called by run.ps1 after a run or by the lead:
-Pairs <pairs.txt> [picture and priority switches], or -Nwf <path> ... to read only.
pairs.txt: one line per group, the NWF path, a tab, the workbook read-out path, a dash when the
group wrote none.

1. every NWF exists, is .nwf, under the work folder, every workbook read-out whole, the probe
   DLL there, NWF names unique, or stop before a start
2. a fresh folder NwcFederatorLoop\docread\<yyyyMMdd-HHmmss> with document and compare
3. sha256 of every NWF
4. start through F103's functions, its guards unchanged: refuse while any Roamer runs, read
   before the backup and again just before the start, refuse while unproved-starts.txt names a
   start still running, back up the settings, start, adopt or record and stop
5. AddPluginAssembly with the probe DLL, ExecuteAddInPlugin("DocumentReadProbe.PARS", folder,
   every NWF). The hang rule watches the .partial files grow and the adopted Roamer's processor
   time. The dialog rule as run.ps1 has it
6. Dispose, a close by pid only when adopted and still there, settings put back under the rule
7. sha256 again, each unchanged or CHANGED with both
8. every read-out whole
9. compare-document.ps1 per pair
10. check-documents.txt with all of it and one verdict line per pair. Exits 0 when every step
    ran, 1 when one could not. It never judges the verdicts

What it needs from run.ps1 and no more: the start and close as functions in one file with no
main body, dot-sourced by both, the one copy of the guards. The pairs file after each run, read
off the run's .tsv written rows named NWF and XLSX paired by the group column (EventRow.cs 92,
FederationEngine.cs 2057, 3109, 3167), never guessed from names. What the run was told about
pictures and priority. Its own Navisworks proved gone and settings put back first.

## 4. What is UNKNOWN and measured first

- PQ1. On this machine does ExecuteAddInPlugin run a plugin added with AddPluginAssembly, hand
  every parameter exactly, return only after Execute returns, and what does its number mean.
  Shared with F101's PQ1, PQ2, PQ4
- PQ2. Is there a public static UnitConversion.ScaleFactor(Units, Units) returning double, and
  what does it give Feet to Meters. Reflection only. If none, the lead chooses between a unit
  table in the comparison, which may be a second copy of UnitTable, and comparing only the
  unit label where the document is not in metres
- PQ3. Which units ClashTest.Tolerance and IClashResult.Distance are held in. A person reads
  one of each in the panel on a copy whose Document.Units the probe read
- PQ4. How the panel counts a result group, in the test list's Clashes and status columns and
  in the Results tab, as one, as its clashes, or both, and under whose status. Needs a copy of
  an NWF with one group of mixed statuses made by a person in the panel
- PQ5. What Clash Detective's own HTML tabular export writes for that copy, per test total and
  five statuses, one row or several for the group. If the API cannot write that export, Bader
  exports it once by hand
- PQ6. Whether ClashTest.Children, the Results tab and the export list results in one order,
  whether sorting the Results tab moves Children, and whether the export's equal count tests
  follow the document's order
- PQ7. Whether the document is whole on the first read after TryOpenFile in an Automation
  start here. The two passes answer it
- PQ8. Whether a group can hold a group, be empty, or a test hold a child that is neither. The
  probe counts all three
- PQ9. Whether Dispose after an NWF was opened raises a save prompt or any dialog, and whether
  opening an NWF whose NWC is missing raises one. Shared with F101's PQ6

Answers go in docs\history\scan.md after F101's 5z-e.

## 5. How it is proved

0. The probe builds with 0 errors and 0 warnings. Core tests unchanged, nothing under src.
5a. Where the answer is known, from neither the harvest nor the probe: Clash Detective's own
    HTML tabular export opened in Excel and saved as xlsx the way the client's samples were
    made, read by read-workbook.ps1, and a person reading three tests off the panel. On the
    smallest group that found clashes in main's first run and its largest. compare-document on
    the export against the probe shows no disagreement on counts, rows and row names, with the
    expected differences written down first (the unit label, the pictures). Then our workbook
    against the probe reads VERDICT AGREE, criterion 3 for that group, closing register rows
    step 310 and P3. The group case: in a copy, a person groups two clashes of one test and
    sets one Reviewed, the probe reads one fewer top level result and the same leaves. No NWF or
    workbook exists on this machine yet, so 5a needs main's first run. Either that window run
    comes before F104 merges, or F104 merges on 5b plus one whole probe read of any loop NWF and
    5a is the first act of the baseline
5b. tools\loop\prove-compare.ps1 in prove-hooks.sh's shape, sixteen broken copies each with
    one edit, each producing its own named line and nothing belonging to another: New raised
    by one, Clashes lowered by one, a row removed, two rows swapped, a picture link given the
    next number, a New row's link removed with -PictureStatuses, a tolerance label ft, a
    tolerance off by 0.001, a distance in feet, a test line removed, a trailing space dropped
    from a test name (both sides named), two equal blocks swapped, one result's status changed
    in the document read-out, a test's counts -1 (NOT COMPARED and NOT PROVED), pass 2 totals
    differing (a doubt and NOT PROVED), END OF READ-OUT removed (COMPARISON FAILED). Locally on
    5a's real pair, and in Actions on a small hand written pair under tools\loop\compare-proof,
    three tests, plain clashes, a mixed status group, an empty test, text only with no machine
    name or path, where the pictures are not on disk so the good pair reads DISAGREEMENTS 0 and
    NOT PROVED, which the harness expects

## 6. What it writes and where

In the repo: the probe csproj and plugin, compare-document.ps1, check-documents.ps1,
prove-compare.ps1, tools\loop\compare-proof, sections in tools\loop\README.md and
tools\probes\README.md, one step in tests.yml, the scan.md section once the PQs are measured,
F104's section and DONE line in steps\01_next.md, steps\loop.md.

On the machine, all under %LOCALAPPDATA%\NwcFederatorLoop: docread\<stamp>\pairs.txt,
check-documents.txt, document\<NWF>-document.txt, compare\<workbook>-compare.txt, proof\, the
settings backup where F103 puts it, probes\unproved-starts.txt only for an unproved start. A new
stamp folder every call, never emptied or reused. Into the repo by a command after the lead
reads them, through F102's mask: the read-outs, comparisons and check-documents.txt into
steps\runs\<set>\<run>\document. No NWF, workbook or picture copied.
