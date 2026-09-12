# Parsons NWC Federator

Navisworks Manage 2025 add-in. Reads a folder of discipline NWC files, gathers them
into federations, saves an NWF and an NWD per federation, imports a clash test XML,
runs the tests, and writes one Excel report per federation. Per building is the
default way of gathering them and there are three others.

This file holds the rules for every file in the repo. The rules for one folder are in
.claude\rules, one file per folder, read when a file under that folder is touched.
The reasons and the measurements behind every rule, with the dates they were read
off a run or a DLL, are in docs\history\claude-md-history.md, kept whole. When a
rule here or in .claude\rules seems wrong, read the reason there before changing it.

## Host and target

- Navisworks Manage 2025 only. API series Nw22, bundle folder Contents\v22
- .NET Framework 4.8 and C# 7.3. Navisworks has been on 4.8 since 2021 and 2025
  did not move. No syntax newer than 7.3 anywhere, because the add-in cannot be
  built with a newer compiler than the one Navisworks binds to
- References are the DLLs in the Navisworks install folder, copy local false:
  Autodesk.Navisworks.Api.dll and Autodesk.Navisworks.Clash.dll
- Installs to %APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle
- 27 people run it. No compiler on their machines, no admin step, no path that
  exists on one machine only

## Done means all three

1. One building federates, clashes and writes Excel with no errors
2. Every ticked building runs unattended in under 45 minutes
3. Clash counts in the Excel match the Clash Detective panel exactly

## Where things are

    src\Federator.Core      every rule a test can prove, no Navisworks type in it
    src\Federator.Addin     the window, the engine and every call into Navisworks
    tests\Federator.Core.Tests  NUnit, one folder per Core folder, Samples.cs at the root
    build\install.ps1       the only way the bundle reaches the 27 machines
    tools\probes            PowerShell that measures the install or the window
    docs\workflow.md        the two workflows, the labels and the open file run
    docs\history            measurements and old rules, never current
    samples                 real files from the project, read by tests, never edited
    steps                   the plan, the questions, what Bader does next, the log
    bundle                  what ships, written by the build, never by hand
    .claude\rules           the rules per folder, each with a paths line at the top
    .claude\hooks           two walls, run by Claude Code before a tool call

## Rules for every file

- Federator.Core holds no Navisworks type and every rule in it has a test. The
  add-in holds every call into Navisworks and cannot be built without Navisworks
  on the machine, so a change to it is read twice and proved by a run on the local
  machine, written into steps\03_bader_next.md as numbered one-action steps
- A rule that can be tested lives in Core, even when only the add-in reads it. That
  is how the file list rule, the group judgement, the open file guard and the units
  table were found and fixed without Navisworks
- Every number that shapes a run is a setting, never a constant: the split character,
  the part positions, the grouping choice, the name fields, the date format, the
  stop after count, the log count, the open statuses, the image size
- Nothing in the code names any one project's file. Names, counts and property
  internal names from the clash XML appear in tests as sample data only
- The tool reports what it noticed and never acts on it. Findings, mismatches and
  drift are information in the log and the window. Bader decides
- Logging never stops a run, and a report check never fails a group
- Say UNKNOWN rather than filling a gap. Confirm against the install and never
  assume. A number in a help line or a doc is measured, never estimated
- A public member nothing in src calls is deleted with its tests, unless a decision
  in steps\02_questions.md keeps it. No member is added that no running code calls
- One rule lives in one place. A copy of a rule in a second file is a bug, and the
  test for a list read off the client's files reads those files, never a copy
- No code identifier and no framework message ever reaches a label in the window.
  A failure dialog may carry the exception message. A label may not
- A file this tool did not write is never listed as written, and a size is only
  logged after File.Exists passes and the real size is read back
- samples, steps\logs and bundle are never edited. The first two are evidence and
  the third is build output. A hook refuses the edit
- Nothing is committed or pushed on main. Every fix goes on its own branch, named
  fix-FNN for the fix in steps\01_next.md, reaches main through one pull request
  merged once Actions is green, and the local branch is deleted after. A hook
  refuses a commit or a push while main is checked out

## How a fix is worked

1. Read steps\01_next.md for the fix and steps\02_questions.md for the decisions
2. Branch fix-FNN off main
3. Change the code, and the rule in .claude\rules where the rule changed
4. Run the Core tests and keep the before and after counts
5. Write the entry at the top of steps\log.md, the DONE line in steps\01_next.md,
   and the proof steps in steps\03_bader_next.md where the add-in changed
6. One draft pull request, whose body says what was proved here and what waits for
   the local machine. Merge it when Actions is green. Never merge red, never stop
   with a pull request open
7. A new question goes in steps\02_questions.md, numbered after the last one

## Build

Everything, which needs Navisworks on the machine because of the add-in project:

    dotnet build ParsonsNwcFederator.sln -c Release

The parts that need no Navisworks, which is all of Federator.Core:

    dotnet test tests\Federator.Core.Tests\Federator.Core.Tests.csproj

There is no Visual Studio and no .NET Framework targeting pack on the build machine,
so net48 compiles only through the Microsoft.NETFramework.ReferenceAssemblies package.
Every project references it, so nuget.org is a hard requirement for building at all.

The add-in finds Navisworks through the NavisworksPath property, default
C:\Program Files\Autodesk\Navisworks Manage 2025. Override it when the install moved:

    dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"

Any check for a Navisworks file tests one full path directly. Never search the install
folder, never recurse it, never wildcard it. Exists() in MSBuild and Test-Path on a
joined path are the only two forms used, and each DLL is checked on its own so a
missing file is named.

Every build stamps the assembly with the git commit and the moment it was built, and
the log and the title bar show it. Never fake this with a hand edited version.

install.ps1 copies the bundle contents, never the bundle folder, and refuses to finish
if any reference is satisfied by neither the bundle, the framework, nor the Navisworks
folder.

## Tests

Tests are NUnit and live in tests\Federator.Core.Tests. Anything in Core gets a real
test. Anything calling the Navisworks API gets review instead, written in larger
reviewed pieces, not many small attempts.

Every check gets a test that breaks one thing and asserts the check names it. A test
that only asserts the good file passes proves nothing.

The pre-commit hook runs the full test set and refuses the commit on a failure. It is
off until switched on, once per clone:

    git config core.hooksPath .githooks

In a container without Windows the same tests report a fixed set of path and file
locking failures. That count is recorded in every log entry, before and after, so a
new failure shows as a change in the number.

## The two walls

.claude\settings.json runs two PreToolUse hooks from .claude\hooks. One refuses an
Edit, Write or MultiEdit under samples, steps\logs or bundle. The other refuses a git
commit or a git push while main or master is checked out. Both read the tool call off
standard input and exit 2 with one line saying why. They need a POSIX sh, which Git
for Windows provides. Whether Claude Code on Windows finds it without help is UNKNOWN
until tried.

## Confirm against the install, do not assume

- exact signatures of Document.AppendFile, SaveFile, PublishFile and Clear
- the ClashTestType value that matches hard_conservative
- the Clash Detective report defaults, so the tool starts where Navisworks starts
- NwdExportOptions is a 2026 class. It does not exist in 2025

## Writing

No generated-by or co-authored-by line. No emoji. No em dash. No semicolon in
prose. No comment that repeats the line under it. Say UNKNOWN rather than filling
a gap. Never report a check that did not run. A tick box is a label of at most
eight words and one grey line of at most twelve, with no capitals for emphasis.
