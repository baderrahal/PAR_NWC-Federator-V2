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

The three window probes need the add-in built in Release and installed by
build\install.ps1, because they construct the real window:

- `probe-window-defaults.ps1` prints every box's text and state, which is how blank
  naming boxes were caught
- `probe-window-labels.ps1` reads every tick box and checks its label and help line
  against the word limits in CLAUDE.md, opening every expander first. Takes
  -maxLabelWords and -maxHelpWords as well
- `probe-window-scroll.ps1` measures each step against the height it gets, so which
  steps need a scrollbar is read rather than guessed. Takes -w and -h as well
- `drive-window-run.ps1` drives the REAL window inside a running Navisworks through UI
  Automation: sets every box, presses Scan and Run and confirms, then leaves the run to
  the log. It is how PART 5 of the wiring round was done from a session that could not
  press a button, and its header says what was measured about the ribbon, the automation
  host and where the window sits in the automation tree. It needs the add-in window
  already open

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
and metres per unit from UnitConversion.ScaleFactor, both passes' totals, one line per
clash test and one per top level result, every doubt, and END OF READ-OUT. A group is one
top level result at its own status with every clash under it counted at the clash's own
status. A count it could not take is -1. tools\loop\compare-document.ps1 reads it. It
hands back 0 when every read-out is whole, 1 when one failed, was refused or was there
already, 2 for too few parameters, 3 for a bad output folder and 4 for a path with no
usable name or two NWFs sharing one, and writes nothing on 2 to 4.

Built on 2026-09-29, 0 warnings and 0 errors, and NOT RUN, because a start of Navisworks
needs the guarded start and close F103 writes. What it cannot say until it runs is PQ1,
PQ3 and PQ6 to PQ9 of F104.
