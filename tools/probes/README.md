# Probes

Seventeen PowerShell files. Sixteen are scripts that read facts off the machine they run
on: the installed Navisworks DLLs, and the real window once the add-in is built and
installed. They were how docs/history/scan.md was measured. The seventeenth,
`il-reader.ps1`, holds the IL reader and the helpers three of them dot-source, and reads
nothing by itself. Nothing here is part of the build or the install.

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

Three more read the install with no Navisworks started, written for F105 on 2026-09-29,
each with its result file beside it, and answer docs/history/scan.md section 5z-f. Their
runs are in `f105-run-record-20260929.txt`, with Get-Process Roamer read before and after.
All three load every assembly with ReflectionOnlyLoadFrom, which runs no code in it, and
print by kind every failed read they hand to IlFail in il-reader.ps1,
probe-viewpoint-calls.ps1 only when a built add-in is there. A read a probe hands to no
IlFail is in no count, see il-reader.ps1 below. probe-viewpoints.ps1 and
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
- `il-reader.ps1` is not a probe. It is the one copy of what the three dot-source, and read
  off their code they use different parts of it. TypeName, IlReason, IlFail and
  IlFailureLines are used by all three. The IL reader IlRead, with IlOpcodeLine, is used by
  probe-roamer-switches.ps1 and probe-viewpoint-calls.ps1, which decode IL with it.
  LoaderLines, and StringRuns, which matches the two string patterns, are used by
  probe-roamer-switches.ps1 and probe-clash-report-api.ps1. probe-viewpoint-calls.ps1 uses
  neither. The IL reader in it follows the approach of probe-automation-start.ps1, the
  start probe F100 merged, which keeps its own copy because that copy is the measurement
  F100 merged. il-reader.ps1 keeps a read that could not be made only when IlFail is
  handed it, by the probe or by IlRead and LoaderLines when the probe calls them: a body,
  a byte that is not an instruction or a token IlRead could not read, a GetTypes load
  LoaderLines is given, and whatever the probe hands it itself, a MemberRef row and the
  rest, each by kind with where it failed and what the runtime said. The probe that
  dot-sourced it counts and prints them with IlFailureLines, probe-viewpoint-calls.ps1
  only when a built add-in is there. IlFailureLines prints every kind with its count, a
  kind nothing was handed included, so a 0 is a reading only of the reads that probe
  hands to IlFail. A read handed to no IlFail is not kept, and docs/history/scan.md 5z-f
  says what such a read does when it fails

A probe that cannot find what it needs says UNKNOWN and the path it looked at, and
stops. Never search the install folder for a DLL, the path is built and tested directly,
which is the same rule the build uses.
