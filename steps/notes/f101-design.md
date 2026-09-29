# F101 design: the no-click entry, Phase 1 item 3

Written by the planning agent on 2026-09-28, read off the working tree of branch fix-F100, and saved here by the lead as returned. Status: design only. Nothing is built. Everything that says UNKNOWN waits on section 5.

## 0. What it is for

The loop has to run the tool on Bader's machine with nobody there. 5z-d measured that a script can start Navisworks Manage 2025 through Autodesk.Navisworks.Api.Automation with no click, call AddPluginAssembly, and quit. NavisworksApplication has `int ExecuteAddInPlugin(string pluginId, params string[] parameters)`. When the tool's own plugin was run that way on [the machine of 2026-09-19], its window opened and closed on its own 8.5 s later, reason UNKNOWN (drive-window-run.ps1 lines 23 to 27, steps\log.md 2769). So the entry never opens the window. It is a second AddInPlugin that reads a settings file, builds the jobs through the same code the window uses, and calls FederationEngine.Run, with no dialog anywhere on its path. The window and the entry are two front ends onto one plan, one run and one ending.

## 1. The settings file

Format: plain text, one choice per line, `key = value`, read with File.ReadAllText (UTF-8 with or without BOM, so run.ps1 writes it with -Encoding UTF8). CRLF and LF, split as PriorityMap.Read splits. Keys are words with single spaces, case blind. Whitespace trimmed. A line whose first non-blank character is # is a comment, a # later belongs to the value. Blank lines ignored, order free. Choice words case blind, paths kept exactly as written after the trim. Five keys may repeat: skip file, skip group, nwf name, nwd name, workbook name. Every other key at most once. The shape follows folders.txt, which FolderMemory reads as Kind=value lines (FolderMemory.cs 94 to 114).

Every default is read from the Core object that holds it, never typed into the parser, and a test compares each one.

| key | type | default when missing | window control |
|---|---|---|---|
| log folder | folder path | none, the run is refused | none, the window always logs to %LOCALAPPDATA%\ParsonsNwcFederator\logs |
| source folder | folder path | none, refused by the scan | SourceFolderBox |
| include subfolders | yes or no | yes, a new Core default | IncludeSubfolders |
| skip file | path under the source folder as the scan lists it, repeatable | none | FilesGrid Use column |
| grouping | PerBuilding, PerBuildingAndDiscipline, PerDiscipline or Everything | GroupingModes.Default | GroupingModeBox |
| skip group | a group as the Building column shows it, repeatable | none | GroupsGrid Run column |
| nwf folder | folder path | none, refused by the job plan | NwfFolderBox |
| nwd folder | folder path | none, refused by the job plan | NwdFolderBox |
| excel folder | folder path or empty | empty, Clash Reports beside the NWF folder | ExcelFolderBox |
| model units | the EnumNames of UnitTable.Offered() | ReportOptions.DefaultUnits | UnitsBox |
| logo | file path or empty | InstallFiles.FindLogo | LogoBox |
| date the nwd | yes or no | OutputNaming default | DateTheNwd |
| write clash xml | yes or no | ReportOptions default | WriteClashXml |
| image thumbnail | yes or no | ImageOptions default | EmbedThumbnails |
| image size | whole number | ImageOptions.DefaultPixels | ImagePixelsBox |
| image cap | whole number, 0 for none | ImageOptions default | ImageCapBox |
| image statuses | comma list of the five, or none | ImageOptions default | ImageNew to ImageResolved |
| nwf, nwd, workbook: level, discipline, type, number, all buildings | text, not empty | NamePattern defaults | the fifteen naming boxes |
| nwf name, nwd name, workbook name | a group, a space, the name, repeatable | none | OutputsGrid cells typed over |
| clash xml | file path or empty | empty | ExchangeFileBox |
| tolerance | file, or millimetres, dot decimal | file | ToleranceBox and ToleranceOtherBox |
| priority file | file path or empty | empty | PriorityBox |
| by design file | file path or empty | empty | ByDesignBox |
| mark penetrations, mark by design, rebuild drifted sets, apply file settings, compact resolved | yes or no | their Core defaults | the five boxes |

Not carried: the open file run and the four hand buttons (PR 6 below), the stop after count, log count and report subfolder (Q29 is unanswered and this must not answer it by the back door), the separator, part positions and date format (the window does not offer them), the picker memory (the entry never loads or writes folders.txt).

A bad or missing line: a missing line takes the default, except log folder, required by the entry, and the source, NWF and NWD folders, refused where the window refuses them. No =, an unknown key, a value that is not one of the key's words, a negative number, a unit the window does not offer, a single key given twice, an empty naming field, a named file or folder not on disk, and a skip or name line naming a file or group the scan or grouping did not make are each a problem naming the line. Every problem is listed. Any problem refuses the run before a document is touched, with one log line, returning Refused. The scan's findings never refuse, as in the window.

## 2. Core types and their tests

New folder Federator.Core\Entry, tests in tests\Federator.Core.Tests\Entry.

- EntrySettings (PR 5): PathFrom, WhyNotOneParameter, Read(text, path), LogFolderFor, Problems, CanRun, Lines(), FilesThatAreNotThere(), ApplyTo(SourceScan), ApplyTo(GroupPlan), Naming(), Reports(logo), and the read values. Tests 1 to 22: every key reads back, a missing key takes the Core default, unknown key refused, no equals, a key twice names both lines and list keys repeat, yes or no, tolerance file, number and negative, a unit the window does not offer, grouping not a mode, image statuses, image size not whole (the one deliberate difference: the window falls back, the entry refuses), empty naming field, no log folder, comments and a hash in a value, every problem listed, log folder fallback, no or two parameters, a named file not there, the echo, paths kept as written, a skip line naming what the scan did not find, a typed name held by hand and its collision refused.
- EntryResult (PR 5): Ran 0, Threw 1, NoSettingsFile 2, Refused 3, with EntryResults.FinishedLine and a test pinning the numbers.
- ScannedFile and SourceScan, moved (PR 4): SourceScan.Read(folder, includeSubfolders, settings, log), DefaultIncludeSubfolders = true. Tests: only nwc sorted, subfolders only when asked, unreadable counted, a missing folder writes nothing, started then finished, an unreadable file cannot be ticked on.
- PlannedGroup and GroupPlan, moved (PR 4): GroupPlan.From(files, mode, settings, naming), ReadExpectedRunPaths, GroupListLines. Tests including GroupListLinesKeepTheWindowsShape with pinned strings.
- FederationJob moved unchanged to Federator.Core.Rerun, and JobPlan.From(plan, nwfFolder, nwdFolder, log) with Refusal None, NoFolders, Collision, NothingTicked in OnRun's order. Tests on the order and the log line.
- RunHeader.Write, moved (PR 4): RUN SETTINGS, the eleven setting lines, GROUPS and FINDINGS word for word, with pinned string tests.
- RunLog.WriteResultAndCopyTo (PR 1): WriteTheResultAndCopyTheLog moved to Core, with tests.
- Small shared rules: ExchangeReader.IsPicked and ReadPicked, ToleranceChoice.OfTyped, ImageOptions.For, InstallFiles.DefaultLogo, each with tests.

## 3. What moves out of FederatorWindow

Every move is verbatim, the window calls the moved code in the same PR, and the window's log is the same lines in the same order with times masked, proved by one window run before and one after.

- WriteTheResultAndCopyTheLog (2077) to RunLog.WriteResultAndCopyTo, called by both finally blocks
- the four across-the-run lines, duplicated at 2037 to 2054 and 2259 to 2276, to FederationEngine.WriteAcrossTheRunLines, removing an existing second copy
- Pump (2511) to a static PluginThread.Pump. FederationEngine.cs 851 to 853 holds its own inline copy, an existing second copy for the register, left alone here
- PickedExchange (2189), XmlIsPicked (784) and the test at 2232 to ExchangeReader.ReadPicked and IsPicked
- RunJobs from 1973 to 2056 to an Addin ScannedRun.Run returning the outcomes, the source findings and whether XML was picked. The window keeps the button, ShowRunPathsAfterTheRun, the findings view and the progress line. Its catch calls ScannedRun.Stopped for the shared Failure line, then SetProgress and Warn unchanged. Its finally calls WriteResultAndCopyTo
- OnScan's non-window part to SourceScan.Read, the FileRow.Include rule to ScannedFile.Include, the XAML IncludeSubfolders default to SourceScan.DefaultIncludeSubfolders
- Regroup's pure part and PathsFor to GroupPlan.From. lastGroups (47, 283) is written and never read, flagged for the lead as a deletion
- GroupRow state to PlannedGroup, with GroupRow wrapping it and keeping INotifyPropertyChanged
- RefreshRunPaths and NwfNamesIn to GroupPlan.ReadExpectedRunPaths
- TickedGroupKeys and the filters to PlannedGroup.WillRun and JobPlan
- OnRun 1688 to 1737 to JobPlan.From, the window mapping each Refusal onto its Warn word for word
- OnRun 1767 to 1808 and GroupListLines to RunHeader.Write
- ChosenTolerance's Other branch to ToleranceChoice.OfTyped, ImagesWanted's status rule to ImageOptions.For, ShowTheInstallsLogo's second half to InstallFiles.DefaultLogo
- FederationJob.cs to Core unchanged
Stay in the window and are never on the entry's path: ConfirmClear, PreviewRunPaths, TestsInThePickedFile, ReadNaming, ChosenUnits, ChosenGrouping, ReportsWanted, every picker, Warn, SetProgress and the open file run.

## 4. The entry plugin

src\Federator.Addin\EntryPlugin.cs, [Plugin("ParsonsNwcFederatorEntry", "PARS", DisplayName = ...)], [AddInPlugin(AddInLocation.<MEASURED>)], a static constructor calling BundleAssemblies.InstallBesideThisAssembly(null). WHICH AddInLocation KEEPS IT OFF THE RIBBON IS UNKNOWN: None is the candidate, measured by PQ3, with AddIn plus a hidden CanExecute as the fallback.

Execute(params string[]): exactly one parameter, the settings file path. None returns NoSettingsFile and writes nothing, because the only folder known is Bader's logs folder. It never opens a window or raises a MessageBox. Order: path, read the text, EntrySettings.Read, RunLog.StartOrDisabled(LogFolderFor(...), DateTime.Now, RunLog.KeepLogs), the BUNDLE line and Session and the ENTRY SETTINGS block, problems refuse, the logo, SourceScan, GroupPlan, JobPlan, ReadExpectedRunPaths, one ENTRY line saying there is no preview and no confirm, one line naming what is open, RunHeader.Write, ScannedRun.Run with a progress that pumps, a catch writing ScannedRun.Stopped and returning Threw with nothing shown, and a finally writing RESULT and the copy when the NWF folder was set, then `ENTRY    finished, returning N, <words>`.

What ExecuteAddInPlugin hands back is UNKNOWN, so the int is a hint and the log is the proof: RESULT means it ran, an ENTRY refused line means refused, a finished line with neither means it threw before the run, no finished line means it died and the hang rule applies. The log goes to the log folder key, else beside the settings file, else %TEMP% as RunLog falls back, which run.ps1 treats as a finding.

The window's Warn before RESULT on a throw stays for the window. An unattended window run that throws waits for OK before RESULT, loop-read.md section 7 item 5, a register finding of its own.

## 5. What must be measured first, prober questions

Through a probe plugin in ViewpointProbe's shape, tools\probes\EntryProbe, outside the solution and the bundle, run through probe-automation-start.ps1 with a new step after AddPluginAssembly, answers to scan.md 5z-e. All wait on F100's run.

- PQ1. Does ExecuteAddInPlugin return only after Execute returns, over a long Execute that heartbeats and pumps? This also asks what ended the window 8.5 s in.
- PQ2. Does Execute receive the parameters exactly, a path with spaces included, and what int comes back?
- PQ3. Is an AddInLocation.None plugin in the records and run by ExecuteAddInPlugin, and is no button drawn? Bader looks at the Tool Add-ins tab once after PR 5 is installed.
- PQ4. Does an Automation start load the installed bundle, and what happens when AddPluginAssembly adds a second Federator.Addin.dll with the same ids?
- PQ5. With no window, is ActiveDocument set, do TryOpenFile, TryAppendFile, TrySaveFile and TryPublishFile work, does TestsRunTest run, does TestsImageForResult give a picture that is not blank, does ComApiBridge.State answer, with Visible false and true?
- PQ6. Which windows appear during those, during the open of an NWF missing one NWC, and during a publish over an existing NWD?
- PQ7. Does an exception escaping Execute raise a dialog in an Automation start?
- PQ8. Do the start and the pictures work with the screen locked? Bader's call whether it matters.

## 6. The pull requests, smallest first

- PR 0, measure: EntryProbe, the probe step, scan.md 5z-e. No src. A no on PQ1 or PQ5 changes the design, towards Roamer's own switches or towards driving the window.
- PR 1: RESULT and the log copy come from a private window method. RunLog.WriteResultAndCopyTo.
- PR 2: the four across-the-run lines are two copies. FederationEngine.WriteAcrossTheRunLines.
- PR 3: RUN started to RUN finished sits in a private window method. ScannedRun, PluginThread.Pump, ExchangeReader.ReadPicked and IsPicked.
- PR 4: the scan, the groups and the jobs can only be built by the window. The Core moves of section 2. Largest, splittable.
- PR 5, F101: nothing can start a run with no click. EntrySettings, EntryResult, EntryPlugin. Proved by three no-click runs: no parameter, a bad line, and run set item 1 on the whole copy.
- PR 6, later: the open file run with no click, for run set item 5.
PRs 1 to 4 are each proved by a window run before and after, with a person opening Navisworks and the window, which is Bader's time.

## 7. Risks

1. ExecuteAddInPlugin may return early or not survive 45 minutes. PQ1, and the finished line lets run.ps1 wait either way.
2. What closed the window 8.5 s in is UNKNOWN. If it is the host ending the plugin, the entry dies too, and the flushed log shows how far it got.
3. Pictures and COM viewpoints may fail or come out blank with Visible false or a locked screen, and fifty image failures stop the run. PQ5.
4. Navisworks' own prompts inside the Try calls are UNKNOWN, above all a missing NWC. Only the hang rule catches one. PQ6.
5. Two copies of the add-in in one start may collide. PQ4.
6. AddInLocation.None may hide the plugin from ExecuteAddInPlugin too, or hide nothing, and the entry ships to all 27. PQ3.
7. PR 4 moves code the 27 use weekly. Verbatim moves, pinned strings and a before and after run.
8. XAML still types seven defaults beside Core (DateTheNwd, WriteClashXml, MarkPenetrations, MarkByDesign, RebuildDriftedSets, ApplyFileSettings, CompactResolved, XAML 308 to 571), an existing breach of the addin.md defaults rule for the register.
9. The entry has no preview, so its GROUPS labels are expected ones, said in one ENTRY line.
10. A log folder that cannot be opened sends the log to %TEMP%.
11. What the int means is UNKNOWN. A run is never judged by it alone.
12. Pumping with no window may differ. UNKNOWN until PQ1.
13. The settings file becomes a second place where choices are written. If Q29 is answered with a settings file for the 27, the two are reconciled then.
