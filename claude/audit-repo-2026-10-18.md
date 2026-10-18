# Audit of the repo against CLAUDE.md, after the last merge of 18 Oct

Bader's message of 17 Oct 2026 late evening, the end: a full audit and QA after the last merge, listing every
contradiction with CLAUDE.md. Read on main 0a22fe7, after F208 288, F209 289 and F210 290. This file is F211.

Simple words. A contradiction is a place where the repo and CLAUDE.md, or a rule file it points to, say different things.
Each is read off the files named, never remembered. Fixed means this branch changed it. Listed means it waits for Bader.

## The checks that pass

- The solution builds in Release with --no-incremental, 0 warnings and 0 errors, on main 0a22fe7 and on this branch.
- Core tests 3345 passed, 0 failed, 0 skipped, before and after this branch.
- Every project is C# 7.3 on net48, the eleven csproj files read one by one. Federator.Core references no Navisworks
  assembly and no file under it imports an Autodesk namespace. The word Autodesk shows in Core only in comments and in
  sample text such as a Revit source path.
- check-locals and check-imports pass over src. check-evidence-ids passes on the tracked files, as Actions did on 290. On
  this PC it refuses 9340 lines, every one in files git ignores and never commits, the TestResults .trx files and the
  artifacts folder, which carry the machine name.
- prove-tracker 76 of 76 cases right, prove-progress 26 of 26, check-tracker and check-progress clean.
- No pull request is open. Each of 288, 289 and 290 merged on green with its branch deleted, on this PC and on GitHub.
- The writing rule over everything F208 to F211 added: no em dash, no emoji, no semicolon in a comment, a string or prose.
  The one em dash is a test that asserts the window's words hold none.
- No public member of the folders F208 and F209 added is left that no running code reads, once 8 below is fixed.
- NM fed, the folder the message calls NWC Fed, was only read: 135 files before the round and after it, 0 lines differ in
  name, size, date or SHA-256. Nothing under samples changed and steps\logs gained new files only.

## Contradictions

| n | what CLAUDE.md or a rule says | what the repo holds | read off | fixed or listed |
|---|---|---|---|---|
| 1 | addin.md: the window holds fifteen named tick boxes, five under no expander, and in another place sixteen | FederatorWindow.xaml holds 20 named CheckBox elements, 11 under no expander, IncludeSubfolders, RebuildExisting, MarkPenetrations, MarkByDesign, RebuildDriftedSets, MakeViewpoints and the five NwfStep boxes of the NWF page, plus 2 with no name, the Run column's and the package tree's | the XAML, every CheckBox with its Expander ancestor | listed, Q205, since how many may sit up front is your limit |
| 2 | Writing: no generated-by or co-authored-by line | 89 commits on main carry Co-Authored-By, the newest 8069bc1, F174, 14 Oct. None since | git log main | listed, history is not rewritten |
| 3 | Where things are: build\install.ps1 the only way the bundle reaches the 27 machines | build\package.ps1 makes a team zip and build\team-install.ps1 inside it installs with no compiler, Q149 | build\ | listed |
| 4 | Nothing in the code names any one project's file, project data lives in its pack | src\Federator.Core\Exchange\revit-categories.txt and revit-worksets.txt name project 1104 and folder C02 in their headers and lines | the two files, lines 4, 19, 34, 41 and 42 of the first | listed, worth a look |
| 5 | Where things are lists every folder | not named: tools\tracker, steps\logs, steps\notes, build\team, build\package.ps1, .github, .githooks, Claude outputs, and claude, new in F210 | git ls-files and the root | listed |
| 6 | One PR per fix, merged, then the branch deleted | 21 branches on this PC merged and kept, fix-F65 to fix-F72, fix-F170 to fix-F174 and 8 round branches. 28 on GitHub merged and kept, fix-F65 to fix-F72, fix-F170 to fix-F174, fix-F198 to fix-F207, fix-Q150, fix-F114-probes, fix-F148, master and one claude lane branch. 6 more on GitHub are not merged and stay | git branch --merged after a fetch with prune | listed, Q206, deleting is yours to approve |
| 7 | samples, steps\logs and bundle are never edited | steps\logs\README.md was edited in cb3d500 on 7 Oct | git log on the file | listed |
| 8 | A public member nothing in src calls is deleted with its tests | PackageWords.AcrossTheRunTitle of F208, read by nothing | grep over src and tests | fixed here |
| 9 | Where things are: .claude\rules, the rules per folder | the six rule files cover src\Federator.Addin, src\Federator.Core, tests, steps, tools\loop and tools\tracker. No rule file covers build, docs, exchange, samples, bundle, tools\probes, tools\checks, .github, .githooks or claude | the paths lines of the six rule files | listed, worth a look |
| 10 | The two walls refuse a file tool under steps\logs and any write naming NM Fed | they run from the repo's .claude\settings.json, so a session started outside the repo, as this round's in C:\NWC-PC-Test, runs without them. This round kept them by hand | this session | listed, worth a look |
| 11 | core.md: a group of one discipline cannot clash, every test is one discipline against another | 284 of the 1830 tests of the pack 1104-PAR set a code against itself, and the NWF page found 1996 clashes per community in such groups on C06 and C07 | the pack's XML and F210's runs | listed, Q203 |

## What stays UNKNOWN

- probe-window-labels.ps1 was not run on the tick box count of 1, so the count is read off the XAML, not off the window.
- tools\loop\prove-run.ps1 has not been run since F173, so its window cases are UNKNOWN, as PROGRESS says.
- Whether the six branches on GitHub that are not merged, fix-F109, fix-F145, fix-F149, pc-test-2026-10-13,
  round-close-penetrations and round-scale, still hold work anyone wants.
