# Audit of the round of 18 Oct evening, F222 to F230

Bader's round of 18 Oct 2026, the end: what each fix proved, the counts per package, the pictures against the clashes per
group, the run time against F220, the bugs found, and a full audit and QA of the repo after the last merge listing every
contradiction with CLAUDE.md. Read on main b5cecfe, after F229 310, and on the branch fix-F230. This file is F230. A
contradiction is a place where the repo and CLAUDE.md, or a rule file it points to, say different things, each read off the
files named. Fixed means fix-F230 changed it. Listed means it waits for Bader or a later fix.

## What the round did

| fix | what | pull request | proved here | waits for |
|---|---|---|---|---|
| F222 | a fresh Navisworks per group on both pages, Q211 | 302 | a copy of three C06 groups, each in its own Navisworks, 98 clashes on each page | seen in F229 |
| F223 | Auto-Save off while the tool works and put back as it was, the value before kept on disk until it reads back, Q207 | 303 | scan.md 5z-zl, two smoke runs on three C06 groups, '0' put back, no autosave, 10 Core tests on a fake registry and record | the switch in a Navisworks a person started, F223-R1 |
| F224 | only tests across two set codes run on both pages, Q209 | 304 | the built in matrix loses its 284, a smoke run 98 to 60 clashes, 0 FAILED | seen in F229 |
| F225 | a test a rule left out that an NWF still holds is not run by the rule and never FAILED, F219-R1, Q212 asked | 305 | NWFs built before F224 on the NWF page, 1B06BS 28, 1B06M1 14 with 22 clashes, 1B06PE 32 with 16 held, 0 FAILED | Q212 |
| F226 | a picture that draws no item of its clash taken again through the active view, no Image cell only sky, Q208 | 306 | scan.md 5z-zm, 76 and 25 on the probe, three smoke runs, 0 pictures of only the sky | seen in F229 |
| F227 | the eight numbers of Q210 settings with today's defaults, 27 members internal, the totals of TestRefresh deleted | 307 | 8 Core tests, a smoke run reading as before | the label in the window |
| F228 | docs\workflow.md for the two pages and the packages, CLAUDE.md's folder list, runner.md and build.md | 308 | Core tests, nothing of src changed | |
| FR-204 | the manifest's line 9 points at docs\history\scan.md, Q30, a pull request of its own | 309 | the diff one line | |
| F229 | the test of the round on multi3, C06, C07 and SKY, both pages | 310 | below | |
| F230 | this audit, the small text fixes, PROGRESS rewritten, the closing entry | this | Core tests | |

## What the test of the round proved, F229

The desktop folder of real files listed before and after with names, sizes, dates and SHA-256, 141 files and 25 folders, 0
lines differing. C06's 64 and C07's 70 NWCs copied into C:\NWC-PC-Test\work\multi3, 0 not matching the listing, the same files
F220 ran, the two listings equal. SKY held a copy of 1B06BC's and 1B06P1's 9 NWCs. Main e38344c installed, stamp e38344c7.

| | NWC page, run-20261018-202457 | NWF page, run-20261018-205401 | F220 |
|---|---|---|---|
| time by the launcher's clock | 28.2 min | 26.4 min | 22.1 min and 19.6 min |
| groups, each in its own Navisworks | 46, 46 DONE | 46, 46 DONE | 45 and 44, a package a Navisworks |
| starts and quits | 46 starts 199.0 s, 46 quits 92.7 s | 46 starts 202.3 s, 46 quits 92.8 s | 3 and 2 starts |
| C06 | 21 groups, 3904 clashes | 21 groups, 3904 clashes | 21 groups, 6207 clashes |
| C07 | 23 groups, 1549 clashes | 23 groups, 1549 clashes | 23 groups, 2822 clashes |
| SKY | 2 groups, 442 clashes | 2 groups, 442 clashes | not run |
| memory or GDI stop lines in the group logs | 0 | 0 | C06 stopping from its 11th group, 7 and 8 times |
| Auto-Save | '0' kept, '3 0' before every group, '0' put back and its record deleted | the same | the loop's switch |
| tests left out by Q209 | 284 of 1830 in each of 34 groups, 9656, the rule not read in the 12 of one discipline | the same | none |
| COVERAGE FAILED | 0 | 0, 257 tests held from the NWC page's run in the 12 groups of one discipline, 0 clashes | |
| pictures retaken | 57 drew no item, 45 drawing their items, 12 still none, 0 left with no picture | the same | |

- Q209 removed 2303 clashes in C06 and 1273 in C07, 3576 of F220's 9029, the files the same. 1B06PK lost 1168, and 1B06PS
  and 1C07PS lost their 92 each, every clash of theirs being of one set code.
- 0 pictures of only the sky by count-sky-pictures.ps1 over the 173 of C06's 1B06BC, C07's 1C07BC and SKY's 1B06BC and the 269
  of SKY's 1B06P1, the groups where 5z-zk read 76 and 25. The 25 of 1B06P1 are gone with the tests of one set code.
- The pictures equal the clashes in every group but 1B06PK, whose 1748 clashes the caps of 150, 100 and 50 hold to 1036
  pictures, its private bytes at most 4330 MB, under the 12 GB stop.
- Each page took about 6 to 7 minutes more than F220, the 46 starts and quits being about 4.9 min of it, each page under the 45
  minutes of criterion 2.
- The settings of Q210 read with their defaults, the caps 150, 100 and 50 and the running count every 25 tests.

### Pictures against clashes, the NWC page, per group

| package and group | clashes | pictures read | drew no item | retaken drew | still none | tests left out by Q209 | F220 clashes |
|---|---|---|---|---|---|---|---|
| C06  1B06BC | 173 | 173 | 19 | 15 | 4 | 284 | 512 |
| C06  1B06BS | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C06  1B06G1 | 756 | 756 | 0 |  |  | 284 | 1038 |
| C06  1B06K1 | 11 | 11 | 0 |  |  | 284 | 40 |
| C06  1B06KI | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C06  1B06M1 | 8 | 8 | 0 |  |  | 284 | 30 |
| C06  1B06P1 | 269 | 269 | 0 |  |  | 284 | 374 |
| C06  1B06PE | 52 | 52 | 0 |  |  | 284 | 68 |
| C06  1B06PG | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C06  1B06PH | 30 | 30 | 0 |  |  | 284 | 59 |
| C06  1B06PK | 1748 | 1036 | 0 |  |  | 284 | 2916 |
| C06  1B06PO | 47 | 47 | 0 |  |  | 284 | 66 |
| C06  1B06PP | 427 | 427 | 0 |  |  | 284 | 534 |
| C06  1B06PS | 0 | 0, no picture asked | 0 |  |  | 284 | 92 |
| C06  1B06PT | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C06  1B06PW | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C06  1B06WL | 25 | 25 | 0 |  |  | 284 | 45 |
| C06  1B06WM | 251 | 251 | 0 |  |  | 284 | 285 |
| C06  1B06WO | 99 | 99 | 0 |  |  | 284 | 118 |
| C06  1C06M2 | 8 | 8 | 0 |  |  | 284 | 30 |
| C06  1C06PK | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C07  1A07PP | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C07  1C07BC | 173 | 173 | 19 | 15 | 4 | 284 | 512 |
| C07  1C07BS | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C07  1C07G1 | 264 | 264 | 0 |  |  | 284 | 512 |
| C07  1C07K1 | 11 | 11 | 0 |  |  | 284 | 40 |
| C07  1C07KI | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C07  1C07P1 | 269 | 269 | 0 |  |  | 284 | 374 |
| C07  1C07PG | 21 | 21 | 0 |  |  | 284 | 74 |
| C07  1C07PK | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C07  1C07PS | 0 | 0, no picture asked | 0 |  |  | 284 | 92 |
| C07  1C07PW | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| C07  1C07WE | 53 | 53 | 0 |  |  | 284 | 72 |
| C07  1C07WL | 23 | 23 | 0 |  |  | 284 | 181 |
| C07  1C07WM | 245 | 245 | 0 |  |  | 284 | 276 |
| C07  1C07WN | 20 | 20 | 0 |  |  | 284 | 29 |
| C07  1C07WO | 89 | 89 | 0 |  |  | 284 | 107 |
| C07  1D07MM | 7 | 7 | 0 |  |  | 284 | 29 |
| C07  1D07PE | 22 | 22 | 0 |  |  | 284 | 37 |
| C07  1D07PH | 58 | 58 | 0 |  |  | 284 | 87 |
| C07  1D07PN | 60 | 60 | 0 |  |  | 284 | 78 |
| C07  1D07PO | 58 | 58 | 0 |  |  | 284 | 77 |
| C07  1D07PP | 176 | 176 | 0 |  |  | 284 | 245 |
| C07  1D07PT | 0 | 0, no picture asked | 0 |  |  | not read, one discipline | 0 |
| SKY  1B06BC | 173 | 173 | 19 | 15 | 4 | 284 | not in F220 |
| SKY  1B06P1 | 269 | 269 | 0 |  |  | 284 | not in F220 |

## Bugs found in the round, each a row of steps\tracker.csv

- F222-R1 to R4: a cut NWF on a kill mid save, one nwc-manifest.tsv shared by a package's NWFs, the guard and the Hide Other
  baseline now one group's, the runner's edges.
- F223-R1 the switch in a Navisworks a person started unmeasured, R2 the hand buttons switch nothing, R3 a run under the loop
  leaves Auto-Save off, R4 a Navisworks started by hand during a run.
- F224-R1 a set name of two code parts, R2 the codes a side is read against differ by group, R3 the Coverage row of a test the
  rule left out has no sets, R4 a mirror of the alike kind.
- F225-R1 a name on a left out test and a run one, R2 RESULT says never in the document, R3 the kept tests fill the examples.
- F226-R1 the retake's red, green and sky read by measured colours, R2 the retake ignores the ghosted look, R3 a row left with no
  picture in no tally, R4 the retake's seconds in no part, R5 a camera not aimed at the clash.
- F229-R1 a run's window prunes the logs folder to 30 and deleted 18 group logs of the run before, the copies whole in C06's Log.

## The checks that pass

- No em dash, en dash or semicolon in CLAUDE.md, the eight rule files, README, INSTALL, docs\workflow.md, PROGRESS.md or this
  round's log entries.
- No syntax newer than C# 7.3, every csproj pins net48 and 7.3, the runner references Core only.
- The 20 named tick boxes, 11 up front, match addin.md. The bundle is 16 files, as build.md says. INSTALL.md's exit lines match
  install.ps1.
- Q210's 27 members are internal and TestRefresh's totals gone. The eight numbers of Q210 are settings with refusal tests.
- A scan of every public member of src found nothing new that src never calls.
- This round's new log lines and Look for lines check out against the code but one, number 21 below, fixed.
- Core tests 3441 passed, 0 failed, 0 skipped, on main after the last merge. The solution built in Release, 0 errors.

## Contradictions with CLAUDE.md and the rule files, read on main e38344c by the audit agent

### The round's own fixes

| n | rule or text | what is there instead | where | state |
|---|---|---|---|---|
| 1 | runner.md: every word the runner writes is Core's | the runner builds the words of a refused Automation DLL itself | runner.md:10, Program.cs:98 | listed |
| 2 | runner.md: addin.md's F222 section holds the ceiling of 30 minutes | the 30 minutes is core.md's | runner.md:33 | fixed, runner.md names core.md |
| 3 | addin.md: rules whose logic lives in Core are in core.md | AutoSaveKeeper's and AutoSaveSwitch's rules sit in addin.md alone | addin.md:707 | listed |
| 4 | AutoSaveSwitch's comment: put back once that Navisworks is gone | put back once as the run ends | AutoSaveSwitch.cs:30 | fixed |
| 5 | CLAUDE.md: every rule in Core has a test | WindowsAutoSaveRegistry, the real registry, has none | AutoSaveKeeper.cs:350 | listed |
| 6 | CLAUDE.md: a rule that can be tested lives in Core | StillRuns, TheOneNew and AutoSaveOff's on or never set rule sit in the runner and the add-in | Program.cs:146, AutoSaveOff.cs:125 | listed |
| 7 | PackageRunWords holds every line about a fresh start | words typed in the add-in, the group's outcome was not kept and four more | FreshStarts.cs:117, FederatorPlugin.cs:143 | listed |
| 8 | the words are Core's and the log count a setting | F224's CLASH line of saved tests built in the add-in with a typed 10 | ClashRunner.cs:617 | listed |
| 9 | the split character is a setting | the rule of two set codes reads a fresh default separator, as two older places do | ClashRunner.cs:591 | listed |
| 10 | Choose's comment: a retake not taken leaves the tool's unless only sky | the tool's whatever it shows | PictureRetake.cs:239 | fixed |
| 11 | addin.md F226: the retake matches models by display name | it matches Model.FileName off ModelFileOf | addin.md:963 | listed |
| 12 | BoxLabel: the label on the Clash step | it is on the Outputs step under More | PictureLimits.cs:124 | fixed in the comment |
| 13 | workflow.md: the NWF page's confirm says each group runs in a fresh Navisworks | only the NWC page's confirm carries that line | workflow.md:139 | listed |
| 14 | workflow.md: the sky pictures pending, Q208 | F226 merged, 0 of only the sky | workflow.md:396 | fixed |
| 15 | addin.md F214: a package's window writes its discipline reports | since F222 the parent writes them | addin.md:1163 | listed |
| 16 | addin.md F208 and comments: a Navisworks a package | a Navisworks a group since F222, the morning's 12 back | addin.md:1130, Packages.cs:284 | listed |
| 17 | addin.md F222: Browse and Read refuse during the run | only the Browse of the parent folder and Read | addin.md:1189 | fixed |
| 18 | loop.md: PROGRESS rewritten after every merge | it read this morning's state | PROGRESS.md | fixed, rewritten by F230 |
| 19 | steps.md: a section a fix, tracker.md: a row a fix | F229 and F230 had none | 01_next.md, tracker.csv | fixed |
| 20 | 01_next.md | a control byte in claude\audit-round | 01_next.md:1795 | fixed |
| 21 | 03 F227 step 1: the IMAGES line ending the caps' words | the line goes on after them | 03_bader_next.md:99 | fixed |
| 22 | steps.md: one action a step | the new steps hold several actions each | 03_bader_next.md | listed |
| 23 | core.md: the categories named per model a setting | CategoriesNamedPerModel internal and read by nothing in src | CoverageSettings.cs:87 | listed |
| 24 | one rule in one place | CLAUDE.md states install.ps1's rule beside build.md, and four more rules sit in two files | CLAUDE.md:154 | listed |
| 25 | one rule in one place, the Auto-Save switch | the key and "3 0" typed in Core and in the loop, Q207 keeping the loop's | AutoSaveSwitch.cs:36, nw-guard.ps1:1301 | kept by Q207 |

### Older, not in the morning's audit

| n | rule or text | what is there instead | where | state |
|---|---|---|---|---|
| 26 | a public member nothing in src calls is deleted | PenetrationSettings.IsDecided, read by tests alone | PenetrationSettings.cs:163 | listed |
| 27 | core.md F158 and F169: the HTML and _files in the Log folder | they are in the Clash report folder, as F177 says | core.md:1282, 1567 | listed |
| 28 | every wording a label carries is Core's, no code names a project | three window texts typed in the add-in, a help line naming Report\C06\Infrastructure | Packages.cs:384, PackageWords.cs:38 | listed |
| 29 | F155: no XML is picked | four window labels still speak of a picked file | FederatorWindow.xaml:595 | listed |
| 30 | a default never typed into the XAML | IsChecked on eleven boxes | FederatorWindow.xaml:334 | listed |
| 31 | every number that shapes a run is a setting | seven fixed values, LookUpLevels, RecentNwfs.Max and five more | Penetrations.cs:424 | listed |
| 32 | steps.md: 03_bader_next.md rewritten, not appended | 3164 lines, each round a section on top | 03_bader_next.md | listed |
| 33 | core.md: the document's side after Compact | nothing compacts since F185 | core.md:3110 | listed |
| 34 | core.md | the heading The machine readable log, F64 twice | core.md:3352 | listed |
| 35 | no semicolon in prose | three in the plan | 01_next.md:969 | listed |

### The morning's audit, claude\audit-round-2026-10-18.md

- Closed: 2 to 11, 9b, 14 by F227, 15 and 16 by F228, 18 by F228, 19 by F228's runner.md and build.md, 25 by FR-204.
- Closed then back: 12, the comments of a Navisworks a package, 16 above.
- Still open: 1 run.ps1's line of no log copy, frozen by Q143, 13 in part, SavedViewpoints.CanBuild, MarkToolFolder and
  TestAddress.IndexAt public and called by nothing, 17 the views tree by team pair in older bullets, 20 Core\Packs with no test
  folder, 21 PROGRESS's form against loop.md, 22 the tool never acts, against the two rules that leave tests out, 23 every test
  one discipline against another, 24 only the Outputs step scrolls.

## Proof steps

The steps of the round are at the top of steps\03_bader_next.md, one section a fix, F222 to F227, in the order to run them in
one build, one install and one Navisworks you start yourself: F222 first, since every later run goes through it, then F223,
F224, F225, F226 and F227. Each is numbered with a Look for line.
