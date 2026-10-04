# Probes

Thirteen PowerShell scripts that read facts off the machine they run on: the installed
Navisworks DLLs, and the real window once the add-in is built and installed. They were
how docs/history/scan.md was measured. Nothing here is part of the build or the install.

Every probe takes the Navisworks install folder as a parameter, defaulting to the same
folder the add-in project defaults to:

    powershell -ExecutionPolicy Bypass -File tools\probes\probe-units.ps1
    powershell -ExecutionPolicy Bypass -File tools\probes\probe-units.ps1 -NavisworksPath "D:\Autodesk\Navisworks Manage 2025"

The nine DLL probes need only the install:

- `probe-clash-api.ps1` reads every type and member of Autodesk.Navisworks.Clash.dll,
  which is how the copy forms, the eEXTERNAL ownership and the absence of any stale
  marker were measured
- `probe-clash-images.ps1` reads the members that render a clash picture
- `probe-units.ps1` reads which members can set a document's or a model's units, which
  is how DocumentModels.SetModelUnitsAndTransform was found to be the only one
- `probe-model-remove.ps1` answers F50: whether one model can be taken out of an open
  document without clearing the whole thing. Nothing named Remove, Delete or Detach
  against a model had ever been read off the DLL, so it was UNKNOWN rather than absent.
  See docs/history/scan.md section 5a
- `probe-viewpoints.ps1` answers F52: how a saved viewpoint folder is made, how a
  viewpoint goes in it, whether a name can be set and what has to be disposed. The repo
  had never touched DocumentSavedViewpoints and nothing about it was measured. It prints
  DocumentSelectionSets beside it, because the sets tree is the closest shape this tool
  already builds. See docs/history/scan.md section 5b
- `probe-document-ready.ps1` answers 5e for F74: whether anything on the API says an
  opened document has finished loading. Every member of Document, DocumentModels and
  Application whose name holds Load, Ready, Busy, Progress, State, Pending or Complete,
  and every event on each. See docs/history/scan.md section 5e
- `probe-properties.ps1` answers 5f for F86, the property API: the collection types under
  ModelItem.PropertyCategories, the two name pairs, every reader on VariantData and its
  fourteen data types, and whether a Search walks a model in one pass. It also reads what
  reflection can say about 5g, the negated condition. See sections 5f and 5g
- `probe-clash-comments.ps1` answers 5h for F72c: whether a comment can be written on a
  clash result. Every text member on ClashResult, IClashResult, ClashResultGroup and
  ClashTest, every Comment member on DocumentClashTests, and the Comment type whole. See
  section 5h
- `probe-unit-scale.ps1` answers PQ2 of F104: whether
  Autodesk.Navisworks.Api.UnitConversion has a public static ScaleFactor taking two Units
  and returning a double, which is what DocumentReadProbe converts through so that the
  check of a workbook never converts through UnitTable, the harvest's own table. It reads
  the metadata with ReflectionOnlyLoadFrom, so no line of the DLL runs. Measured on
  2026-09-29, the answer is yes,
  `public static System.Double ScaleFactor(Autodesk.Navisworks.Api.Units from, Autodesk.Navisworks.Api.Units to)`,
  kept whole in `unit-scale-result-20260929.txt` with the machine's name masked. What it
  gives for Feet to Meters is not read here, because that runs Navisworks code, and the
  probe below reads it inside Navisworks

The four window probes need the add-in built in Release and installed by
build\install.ps1. The first three construct the real window, and the fourth drives it
inside a running Navisworks:

- `probe-window-defaults.ps1` prints every box's text and state, which is how blank
  naming boxes were caught
- `probe-window-labels.ps1` reads every tick box and checks its label and help line
  against the word limits in CLAUDE.md, opening every expander first. Takes
  -maxLabelWords and -maxHelpWords as well
- `probe-window-scroll.ps1` measures each step against the height it gets, so which
  steps need a scrollbar is read rather than guessed. Takes -w and -h as well
- `drive-window-run.ps1`, since F106, is started by tools\loop\run.ps1 for a window run and
  drives the tool's window of the one Navisworks run.ps1 adopted, by -OwnerPid and
  -OwnerStartTicks, through UI Automation. It types every folder and the XML, presses Scan,
  reads every box back, presses Run only when each reads what was typed and every path lies
  under runs\NN, and answers the confirm OK, or Cancel when it names a path outside the loop
  folder. It never clicks, never sends a key, never moves the pointer and never searches the
  desktop. Its header says what was measured about the window

One probe starts a Navisworks of its own. It refuses to start one while any Navisworks
runs, whatever its command line and whoever started it, so the code keeps that rule, not
a person. It refuses to run when either deadline is below 60 seconds, or when
it is not the script its own powershell.exe was started to run with -File. It quits its
Navisworks through the API's Dispose. It closes it through the handle its adoption holds
only when it is the adopted one and Dispose left it running, a step failed, or the adopted
deadline passed. It never closes anything before adoption, and sends no message to any
window before it. Since F103 its guards are in tools\loop\nw-guard.ps1, one copy it
dot-sources with tools\loop\run.ps1:

- `probe-automation-start.ps1` answers F100: whether Autodesk.Navisworks.Api.Automation
  starts Navisworks with no click, which process id it started, whether that Navisworks
  opens a copy of one NWC from the loop's source copy, takes the add-in built from this
  repo through AddPluginAssembly, and quits. A new Roamer whose command line does not hold
  the word embedding was started by hand and is left alone. It adopts a Navisworks as its
  own only when the constructor returned, exactly one possible start is new, it started
  after the call and its command line holds -Embedding. A start it cannot prove, and its
  own if it will not die, is written to
  %LOCALAPPDATA%\NwcFederatorLoop\probes\unproved-starts.txt and left running, and every
  later run refuses to start while one named there is still running, because a person
  has to look. Any Roamer running at its step 2, and again just before the start, is
  named by id and start time and the probe stops, starting nothing, and never closes,
  attaches to or sends it anything. It backs up
  Bader's Navisworks settings first and prints every change with its old and new value.
  It puts them back only when its watchdog ran with passes, no error line, nothing in its
  runspace's error stream and no pass that failed early, and that record shows no other
  Navisworks from the backup on, none runs at the end, and its own reads gone. Then each
  write lists the Roamers again first and stops at any, and writes only what still reads
  as the compare read. Otherwise it writes nothing and keeps the backup. A Navisworks that
  starts and exits inside one gap between watchdog passes is not seen, and the result
  prints the longest gap. It never empties its work folder, it renames the last one
- `automation-start-result-20260929.txt` is the output of run 4 on 2026-09-29, the one full
  run of the probe as it merged with F100 at 0eb4ede, made with no other Navisworks
  running. All six steps passed and Bader's settings were put back. F103 moved its guards
  into tools\loop\nw-guard.ps1 and made its close go through the held handle, proved with no
  Navisworks by tools\loop\prove-run.ps1 and F100's own harness, and not yet by a run
- `automation-start-result-20260928.txt` is the output of run 3, made by the version of the
  probe BEFORE fix list 2. That version put back Bader's settings while his Navisworks, pid
  34668, was running, which the rules above now rule out. It is kept for the lines
  docs/history/scan.md cites, with the licensing agent's two ids and the machine name
  masked by F102's tools\loop\mask-evidence.ps1, pull request 76, and every line where it was
- `automation-start-reflection-20260928.txt` is the output of the attempt 3 probe's
  `-ReflectionOnly` mode, which starts nothing, the machine name masked. See
  docs/history/scan.md section 5z-d for all three

A probe that cannot find what it needs says UNKNOWN and the path it looked at, and
stops. Never search the install folder for a DLL, the path is built and tested directly,
which is the same rule the build uses.

## DocumentReadProbe

A plugin that runs INSIDE Navisworks, F104, in ViewpointProbe's shape: its own csproj,
net48 and C# 7.3, against the Api and Clash DLLs in the install, copy local false. It is
not in the solution, the bundle or install.ps1, and it references no project of this repo,
because it is the document side of the check of a workbook and a check that shares the
code it checks proves nothing. Built with

    dotnet build tools\probes\DocumentReadProbe\DocumentReadProbe.csproj -c Release

It is loaded with AddPluginAssembly into a Navisworks started through the Automation API
and run with ExecuteAddInPlugin("DocumentReadProbe.PARS", <output folder>, <NWF>, ...).
The folder and every NWF must sit under %LOCALAPPDATA%\NwcFederatorLoop. Per NWF it opens
the file with TryOpenFile, the one call it makes that changes what Navisworks holds, reads
the document twice with five seconds of the dispatcher between, and writes
<NWF name>-document.txt: the NWF's size, time and sha256 read before the open, the units
and metres per unit from UnitConversion.ScaleFactor, written UNKNOWN with a doubt unless
ScaleFactor(Millimeters, Meters) reads below one, because a number times its inverse is
one whichever way the factor runs, both passes' totals, one line per
clash test and one per top level result, every doubt, and END OF READ-OUT. A group is one
top level result at its own status with every clash under it counted at the clash's own
status. A count it could not take is -1. tools\loop\compare-document.ps1 reads it. It
hands back 0 when every read-out is whole, 1 when one failed, was refused or was there
already, 2 for too few parameters, 3 for a bad output folder and 4 for a path with no
usable name or two NWFs sharing one, and writes nothing on 2 to 4.

Built on 2026-09-29, 0 warnings and 0 errors, and NOT RUN. Since F104 part 2 the one thing
that runs it is tools\loop\run.ps1 -Mode Documents, through the guarded start and close of
nw-guard.ps1: it refuses a build whose stamp names no one commit with no edits, so build it
from a tree whose git status prints nothing, copies the DLL into its run folder, loads that
copy with AddPluginAssembly, and hands it the run folder's document\ and every NWF of the
pairs, as tools\loop\README.md says. The probe's own refusals are unchanged. What it cannot
say until it runs is PQ1, PQ3 and PQ6 to PQ9 of F104, and the lead's documents read of set 03
is its first run.
