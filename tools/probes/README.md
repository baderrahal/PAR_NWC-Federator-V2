# Probes

Sixteen PowerShell files. Fifteen are scripts that read facts off the machine they run on:
the installed Navisworks DLLs, and the real window once the add-in is built and installed.
They were how docs/history/scan.md was measured. The sixteenth, `il-reader.ps1`, is the IL
reader two of them dot-source, and reads nothing by itself. Nothing here is part of the
build or the install.

Every probe takes the Navisworks install folder as a parameter, defaulting to the same
folder the add-in project defaults to:

    powershell -ExecutionPolicy Bypass -File tools\probes\probe-units.ps1
    powershell -ExecutionPolicy Bypass -File tools\probes\probe-units.ps1 -NavisworksPath "D:\Autodesk\Navisworks Manage 2025"

The eight DLL probes need only the install:

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

Three more read the install with no Navisworks started, written for F105 on 2026-09-29,
each with its result file beside it, and answer docs/history/scan.md section 5z-f. Their
runs are in `f105-run-record-20260929.txt`, with Get-Process Roamer read before and after.
All three load every assembly with ReflectionOnlyLoadFrom, which runs no code in it, and
print every read that failed rather than dropping it. probe-viewpoints.ps1 and
probe-model-remove.ps1, which 5z-f ran as well, load the Api DLL with Assembly.LoadFrom,
which runs the load code of that mixed native and managed DLL and of the native DLLs it
needs, so their reads are not reflection only:

- `probe-viewpoint-calls.ps1` checks what src\Federator.Addin\Engine\SavedViewpoints.cs
  calls in the Api DLL and the two COM DLLs, because probe-viewpoints.ps1 never opens the
  COM half the add-in writes viewpoints through. Three ways: a list typed by hand, where
  FOUND means that name and that shape and the list checks only what is on it, the type
  arguments of 5d's and 5c's generic members, and, given -AddinPath or a Release build of
  this repo, every Navisworks reference in the IL of the classes compiled from that file,
  resolved against the install, which is the part not typed by hand
- `probe-roamer-switches.ps1` reads Roamer.exe and the DLL whose parser holds its command
  line switches as bytes and IL, never by running it, and lists every switch the parser
  matches and what the IL does with ExecuteAddInPlugin
- `probe-clash-report-api.ps1` lists every public type and member of the five Navisworks
  assemblies the tool can reach whose name holds Report, Html, Tabular or Export, every
  clash type of the COM interop, and every string in each file holding tabular, .xsl,
  clash_report or reportformat, each word counted on its own. It found no such member in
  Autodesk.Navisworks.Api.Clash, the namespace the add-in uses, and none in the COM clash
  interfaces. It found one public static LcClClashReport.WriteReport in the Interop
  namespace of Autodesk.Navisworks.Clash.dll, which the add-in references, whose report
  formats are named only at run time
- `il-reader.ps1` is not a probe. It is the one IL reader probe-roamer-switches.ps1 and
  probe-viewpoint-calls.ps1 dot-source, and it follows the approach of
  probe-automation-start.ps1, the start probe F100 merged, which keeps its own copy because
  that copy is the measurement F100 merged. Every read it cannot make, a body, an opcode
  or a token, is kept and counted, and the probe that dot-sourced it prints them

A probe that cannot find what it needs says UNKNOWN and the path it looked at, and
stops. Never search the install folder for a DLL, the path is built and tested directly,
which is the same rule the build uses.
