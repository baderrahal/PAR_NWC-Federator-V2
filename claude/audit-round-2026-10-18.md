# Audit of the round of 18 Oct, F212 to F221

Bader's round of 18 Oct 2026, the end: the findings of the round, a full audit and QA of the repo listing every
contradiction with CLAUDE.md, and the proof steps. Read on main 026a144, after F219 299, and on the branch fix-F221.
This file is F221. A contradiction is a place where the repo and CLAUDE.md, or a rule file it points to, say different
things. Each is read off the files named, never remembered. Fixed means fix-F221 changed it. Listed means it waits for
Bader or a later fix.

## What the round did

| fix | what | pull request | proved here | waits for |
|---|---|---|---|---|
| F212 | Bader's answers to Q202 to Q206, the manual append rule in addin.md, 20 merged branches deleted, main kept | 292 | the rule read off src, nothing sets anything around an append | nothing |
| F213 | a bad NWC never stops an unattended run, its message box answered, its group FAILED | 293 | probes 5z-zh and 5z-zi, a run on a random file, a cut file and a held file | seen in F220 |
| F214 | a fresh Navisworks per package, Run pressed once | 296 | the flat C06 and C07 run, 42 of 42 DONE, C07 3642 pictures, 24.4 min | seen in F220 |
| F215 | the sky-only pictures measured, cause UNKNOWN | 297 | SkyProbe, scan.md 5z-zk | Q208 |
| F216 | the full path on every package line | 294 | Core tests | seen in F220 |
| F217 | the NWF page confirm counts the old jpg files | 298 | Core tests | seen in F220 |
| F218 | the far limit 30 m, tests at 29.9, 30 and 30.1 m | 295 | Core tests | seen in F220 |
| F219 | a group of one discipline runs its tests on neither page, Q209 counted | 299 | Core tests | seen in F220, Q209 |
| F220 | the test on multi2 | 300 | the NWC page 45 groups, 44 DONE, the corrupt one FAILED, 22.1 min, the NWF page 44 DONE, 19.6 min, 9029 clashes on both, the real files 0 lines differ | nothing |
| F221 | this audit, the rule texts the code has outgrown fixed | 301 | Core tests and the build | |

## Findings of the round

- F220, the NWC page on C06, C07 and Corrupt and the NWF page on C06 and C07, steps\logs 2026-10-18-C06-C07-F220:
  - F213 seen: the random file's message box pressed after 206 ms, its group FAILED naming the file, no person. Corrupt was
    the last package, so a package after it is not shown
  - F214 seen: each package in its own Navisworks, gone 1.6 to 2.4 s after it was told to quit. C07 2709 pictures on each
    page with no memory stop. C06 3605 and 3623, the memory stop firing 7 and 8 times from its 11th group of 21 on
  - F216 seen: every package line of PACKAGE TREE and PACKAGES TICKED ends with its full path
  - F217 seen: the NWF page's confirm counted 6314 old jpg files, the 3605 and 2709 the NWC page wrote
  - F218 seen: 49.055 m reads far and 23.963 m near in the discipline workbooks
  - F219 seen: both pages skip the same 12 groups, 6 of C06 and 6 of C07, each with its discipline, and no NWF read UNKNOWN
  - both pages 6207 clashes in C06 and 2822 in C07. Clash Detective's own counts were not read against the workbooks

- The desktop folder of real files: its NWC\C07 holds 1C07, 1D07 and 1A07 files dated 09:41 on 18 Oct and its folder
  09:42, where F210's copy held 1B06 files, a change no command of this session made. This round only read the folder,
  141 files and 25 folders before and after F220, 0 lines differ. F220 ran C07 as it stood at 12:37
- F214-R5: the memory stop still fires in C06, from its 11th group of 21 on in F220, where the row reads the last 6
- F215: 76 pictures of 1B06BC and 25 of 1B06P1 draw no item of their clash, cause UNKNOWN, Q208
- F219-R1: an NWF the NWF page refreshed before F219 keeps the results of its one-discipline tests, which COVERAGE reads
  as FAILED, about 3992 clashes on C06 and C07
- Q209: 284 tests of the pack set a set code against itself and 526 a team against itself, both pages run them, 360 of
  the 1914 run found 4910 of the 12414 clashes of the 30 groups of two or more disciplines

## The checks that pass

- The solution builds in Release with --no-incremental, 0 warnings and 0 errors, on fix-F221 with main 4e705ce merged in.
- Core tests 3392 passed, 0 failed, 0 skipped, on main before this branch and on it after.
- check-locals and check-imports pass over src, and so do check-tracker and check-progress.
- Every pull request of the round, 292 to 300, merged on green with its branch deleted, and none is left open.

- No Navisworks type in src\Federator.Core or src\Federator.Runner. The Runner names the Automation DLL and its type
  only as strings, loaded by reflection from a joined path.
- No syntax newer than C# 7.3 under src, tests or the tool projects, and every csproj pins net48 and LangVersion 7.3.
- The writing rule: no em dash, en dash or semicolon in CLAUDE.md, the six rule files, README.md, INSTALL.md,
  docs\workflow.md or steps\PROGRESS.md.
- The window holds 22 CheckBox elements, 20 named, 11 under no expander, as addin.md counts them since F212.
- The bundle carries 3 files of the tool, 12 library DLLs and PackageContents.xml, 16, as addin.md says.
- The far limit is 30 m everywhere, the setting, its test and core.md.

## Contradictions

| n | what CLAUDE.md or a rule says | what the repo holds | read off | fixed or listed |
|---|---|---|---|---|
| 1 | core.md and addin.md: since F151 no copy of the log is written and RESULT is the last thing a run writes | WriteTheResult copies the log and its .tsv into every Log folder after RESULT, F158, LogCopy.Into. In a package's own Navisworks one line naming its result file follows the LOGS lines, and the window that starts a run of packages copies nothing | FederatorWindow.xaml.cs 2479 and 2488, FederatorWindow.FreshStarts.cs 131 and 171 | fixed in core.md, addin.md, the RunLog, ImageTally and WrittenSizeTests words. tools\loop\run.ps1 1547 and tools\loop\StandIn\Program.cs 151 still say no copy is written, listed, since Q143 freezes the loop's scripts |
| 2 | core.md: Nineteen steps, DECIDE, APPEND ... NWD, CONFIRM | RunSteps.InOrder holds 20, REFRESH added by F187, NWD and CONFIRM before PICTURES since F182 | RunSteps.cs | fixed |
| 3 | addin.md: COVERAGE runs before the internal workbook and the NWD | COVERAGE runs after the NWD since F182 | RunSteps.cs and core.md F182 | fixed |
| 4 | core.md: THE UNITS LINES ARE CORE'S, Federator.Core.Units.UnitsLines | no UnitsLines anywhere in src or tests, deleted by F201 | grep | fixed, the bullet removed |
| 5 | addin.md: Compact is a tick box off by default | F185 removed the box and its call, nothing calls TestsCompactAllTests | ClashRunner.cs and core.md F182 | fixed |
| 6 | core.md and addin.md: the picked NWF box and its three buttons sit on the Source step | PickedNwfBox and two buttons, File and Folder, sit on the NWF page | FederatorWindow.xaml 763 to 767 | fixed, with the comments of FolderMemory, NwfPick, FederationEngine and the window |
| 7 | addin.md: the NWF page's packages route asks the open document's confirm | the same file says it asks nothing about the open document, and the code shows the refresh confirm alone | FederatorWindow.Packages.cs | fixed |
| 8 | core.md: the NWC page holds four steps | five since F208, Packages | FederatorWindow.xaml | fixed |
| 9 | core.md: F53 takes the first size property | F53's first property reading went with its tests in F114, the size is the largest everywhere | SizeRule, TestViewPlan | fixed |
| 9b | addin.md: RebuildExisting sits on the Source step | it sits on the Grouping step | FederatorWindow.xaml 179 and 189 | fixed |
| 10 | core.md counts: ReportClashesTests seven, WorkbookCellCheckTests fourteen, RunAs one of six, PickerStart all six, the census all six | 8 tests, 15 tests, seven labels, seven picker kinds, five counts | the test files, RunPath.All, PickerKind | fixed |
| 11 | CLAUDE.md: a public member nothing in src calls is deleted | NwfPageWords.NwcPageName and NwfPageName, read by nothing and with no test | grep over src, tests and tools | fixed, both deleted |
| 12 | Packages.cs comments: one engine for the whole run | since F214 one Navisworks and one engine per package, joined by WriteJoinedResult | FederatorWindow.Packages.cs | fixed |
| 13 | CLAUDE.md: a public member nothing in src calls is deleted | members called only by tests: ByDesignTally.SavedTestsRead, PenetrationTally.UnmeasuredCount, ReportedCount.NothingExplainsIt, CoverageSettings.CategoriesNamedPerModel, DocumentCensus.CouldNotCompare, EventRow.FieldCount, PublishedProperties.WasSet, RevitWorksets.DecidedCount, GenericModelsAcrossTheRun.GroupsNotCounted, MirrorMerge.FoundByBoth, DamagedDocument.AFallbackMayRun and TheFileMayBeSaved, ModelLoadWait.CeilingSeconds, Readings and SteadyReadings, SetsAcrossTheRun.GroupsWithItems, OutsideTheToolFolder.ItemsDeleted, SizeRule.LargestProperty, TestsRead.GroupsReadOf and seven of TestViewPlanOutcome and RefreshComparison.PerTest, 27. Called by nothing: TestRefresh.BeforeTotal and AfterTotal. In the add-in SavedViewpoints.CanBuild is called by nothing, MarkToolFolder only by ToolViewFolderTests, which reads its source as text, and TestAddress.IndexAt only by the Q133Import probe, which compiles the engine | the audit agent's sweep, CanBuild read on 2026-10-05 by addin.md itself | listed, Q210, since some are read by tests as the rule's proof |
| 14 | CLAUDE.md: every number that shapes a run is a setting, never a constant | PictureLimits.PicturesForA 150, B 100, C 50, GdiStop 9000 and MemoryStopBytes 12 GB are const, and so are ClashRunner.ProgressEvery 25, ExportCheck.NamesShown 10 and AppendBoxWatch.PressesBeforeSaying 10 | PictureLimits.cs | listed, Q210 |
| 15 | docs\workflow.md: the weekly run opens the NWF already there, OPENED, and a Rebuilt NWF is cleared and rebuilt | since F188 the NWC page refuses a group whose NWF is there unless Rebuild is ticked, and a rebuild goes into a new dated folder. workflow.md never names the NWF page or the packages | NwcPageRule.CanRun, GroupRow.CanInclude | listed, the writer's file, a fix of its own |
| 16 | docs\workflow.md: removing a model is UNKNOWN, a mirror's clashes are in the kept test's view, a sub group discipline is a setting, Weekly run plus XML, six labels, twelve service categories | RemoveFile measured 5c and 5x, a mirror not merged is in no view, HoldsTheSizeFolder names Mechanical and Electrical, plus matrix, seven labels, thirteen categories | CLAUDE.md, core.md, RunPath.cs, PenetrationSettings | listed, with 15 |
| 17 | core.md: the viewpoint tree is by priority and team pair, a folder No priority | F186 and F199: A, B and C under Clash Status, a test with none in C, then the discipline folder, no team pair level | core.md F186 and F199 bullets | listed, the older bullet is history kept beside the newer |
| 18 | CLAUDE.md Where things are: build\install.ps1 is the only way the bundle reaches the 27 | build\package.ps1 and build\team-install.ps1, the team zip, Q149. Not listed: tools\tracker, claude, build\team, .github, .githooks, steps\notes, steps\logs, Directory.Build.targets, the C# probes under tools\probes | git ls-files | listed, as F211's 3 and 5 |
| 19 | CLAUDE.md: the rules for one folder are in .claude\rules, one file per folder | src\Federator.Runner and build have no rule file, their rules sit in addin.md, whose paths line is src/Federator.Addin/** | the paths lines | listed |
| 20 | tests: one folder per Core folder | Core\Packs has no test folder, its tests are MatrixPackTests under Exchange | the tree | listed |
| 21 | loop.md: PROGRESS holds next, the next three steps, and a line per lane | PROGRESS.md lists five next steps, a Speed section, and no lane lines | steps\PROGRESS.md | listed |
| 22 | core.md: SINGLE DISCIPLINE is information and nothing is blocked, and CLAUDE.md: the tool never acts on what it noticed | a group of one discipline runs no test on either page, Bader's answer to Q203 | BuildingGroup.CannotClashWith | listed, his decision, the two rules read against each other |
| 23 | core.md: every clash test is one discipline against another | 284 tests set a set code against itself and run, Q209 | the pack's XML | listed, Q209 |
| 24 | addin.md: Only the Outputs step scrolls, the other three fit | the NWC page has five steps since F208 and the NWF page its own | FederatorWindow.xaml | listed, a probe run decides |
| 25 | the bundle's PackageContents.xml names docs\scan.md | docs\history\scan.md since F139 | the manifest comment | listed, the bundle changes on its own |

## Proof steps

The numbered steps are in steps\03_bader_next.md under the heading The round of 18 Oct, F212 to F221, each one action with its Look for line.
