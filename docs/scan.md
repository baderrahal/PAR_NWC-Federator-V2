# Scan

Everything below was observed on this machine in one session, on 2026-08-27, on branch
build-core. Nothing here is carried over from another machine or another session.
Where a check could not run it says UNKNOWN.

Machine: Windows 11 Enterprise 10.0.26200, user p003653k.

## Job 1: what this repo had before this session

State of the repo at the moment the session started, before anything was created.

```
ITEM                     | STATE   | PATH                                    | FACT
git repo                 | PRESENT | .git                                    | one commit, e179590 "rules file", branch master, no remote configured
.gitignore               | ABSENT  |                                         | nothing was ignored, so bin and obj would have been committed
CLAUDE.md                | PRESENT | CLAUDE.md                               | 4312 bytes, holds the target and the measured facts
solution file            | ABSENT  |                                         | no .sln anywhere in the tree
project file             | ABSENT  |                                         | no .csproj anywhere in the tree
test project             | ABSENT  |                                         | no test project and no test runner config
GitHub Actions workflow  | ABSENT  |                                         | no .github folder at all
hooks folder             | ABSENT  |                                         | .git/hooks held only the stock .sample files, none active
agents folder            | ABSENT  |                                         | no .claude folder and no agents folder
README                   | ABSENT  |                                         | no README of any extension
samples folder           | PRESENT | samples                                 | 3 files, untracked, 3.9 MB total
the three sample XML files | PRESENT | samples                               | present, but named differently from the brief, see below
any existing source      | ABSENT  |                                         | no .cs file anywhere in the tree
```

### The sample file names differ from the brief

The brief named the files with underscores. On disk they carry spaces and brackets.
These are the real names, byte for byte:

```
BRIEF SAID                              ON DISK                                  SIZE
1104-PAR_CLASH_AllInOne__2___1_.xml     1104-PAR_CLASH_AllInOne (2) (1).xml      1443262
Search_Set_Building.xml                 Search Set Building.xml                  1471275
Search_Set_Infra.xml                    Search Set Infra.xml                     1115828
```

The test project accepts either spelling, so a rename on the way in will not break the
tests. See `tests/Federator.Core.Tests/Samples.cs`.

## Job 2: the machine

### 1. Navisworks Manage 2025

PRESENT. `C:\Program Files\Autodesk\Navisworks Manage 2025`

Also on the machine, not used by this tool: `Navisworks Exporters 2025` and
`Navisworks Exporters 2024`, both under `C:\Program Files\Autodesk`.

### 2. The three DLLs

All three found, all file version 22.5.1433.58, all assembly version 22.0.0.0,
public key token d85e58fa5af9b484.

```
C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll         4261152 bytes
C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Clash.dll        508704 bytes
C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Automation.dll   184088 bytes
```

### 3. Document.AppendFile, SaveFile, PublishFile and Clear

Read by reflection out of `Autodesk.Navisworks.Api.dll` on this machine. These are the
real signatures, not what was expected.

`Autodesk.Navisworks.Api.Document`, base type `System.Object`.

```
public System.Void AppendFile(System.String fileName)

public System.Void SaveFile(System.String fileName)
public System.Void SaveFile(System.String fileName, Autodesk.Navisworks.Api.DocumentFileVersion fileVersion)

public System.Void PublishFile(System.String fileName, Autodesk.Navisworks.Api.PublishProperties properties)

public System.Void Clear()
```

Points that matter for the add-in:

- `AppendFile` takes one file at a time. There is also `AppendFiles`, and Try forms of
  all four: `TryAppendFile`, `TryAppendFiles`, `TryAppendSheet`, `TryPublishFile`,
  `TrySaveFile`. Every other method name on Document containing Save, Publish, Append
  or Export: `AppendFile`, `AppendFiles`, `AppendSheet`, `ExportAsDwf`, `PublishFile`,
  `SaveFile`, plus the Try forms and the `FileSaved` event accessors.
- `PublishFile` has exactly one overload and `PublishProperties` is not optional.
- CLAUDE.md says `NwdExportOptions` is a 2026 class and does not exist in 2025.
  CONFIRMED on this install. No type matching `NwdExport*` exists in
  `Autodesk.Navisworks.Api.dll`. The 2025 way to write an NWD is `PublishFile` with
  `Autodesk.Navisworks.Api.PublishProperties`.

### 4. DocumentClashTests, ClashTest and ClashTestType

`Autodesk.Navisworks.Api.Clash.DocumentClashTests`, base type `System.Object`.

Properties:

```
public string Id { get }
public Autodesk.Navisworks.Api.SavedItemCollection Tests { get }
public Autodesk.Navisworks.Api.Clash.ClashTestsData Value { get }
```

Methods:

```
public System.Void CopyFrom(ClashTestsData value)
public ClashTestsData CreateCopy()
public SavedItemReference CreateReference(SavedItem item)
public SavedItem ResolveGuid(System.Guid value)
public SavedItem ResolveReference(SavedItemReference reference)
public System.Void TestsAddCopy(ClashTest test)
public System.Void TestsAddCopy(GroupItem parent, SavedItem item)
public System.Void TestsClear()
public System.Void TestsClearResults(ClashTest test)
public System.Void TestsCompactAllTests()
public System.Void TestsCompactTest(ClashTest test)
public System.Void TestsCopyFrom(System.Collections.Generic.IEnumerable`1[SavedItem] value)
public System.Void TestsCopyFrom(SavedItemCollection value)
public System.Void TestsEditDisplayName(SavedItem item, System.String name)
public System.Void TestsEditResultApprovedBy(IClashResult result, System.String approvedBy)
public System.Void TestsEditResultApprovedTime(IClashResult result, System.DateTime approvedTime)
public System.Void TestsEditResultAssignedTo(IClashResult result, System.String assignedTo)
public System.Void TestsEditResultBoundingBox(IClashResult result, BoundingBox3D boundingBox)
public System.Void TestsEditResultCenter(IClashResult result, Point3D center)
public System.Void TestsEditResultComments(IClashResult result, CommentCollection comments)
public System.Void TestsEditResultCreatedTime(IClashResult result, System.DateTime createdTime)
public System.Void TestsEditResultDescription(IClashResult result, System.String description)
public System.Void TestsEditResultDistance(IClashResult result, System.Double distance)
public System.Void TestsEditResultExternalLink(IClashResult result, ExternalLink externalLink)
public System.Void TestsEditResultStatus(IClashResult result, ClashResultStatus status)
public System.Void TestsEditTestFromCopy(ClashTest test, ClashTest copyFrom)
public System.Drawing.Bitmap TestsImageForResult(IClashResult result, ImageGenerationStyle style, System.Int32 width, System.Int32 height)
public System.Void TestsInsertCopy(System.Int32 index, ClashTest test)
public System.Void TestsInsertCopy(GroupItem parent, System.Int32 index, SavedItem item)
public System.Void TestsMove(System.Int32 oldIndex, System.Int32 newIndex)
public System.Void TestsMove(GroupItem oldParent, System.Int32 oldIndex, GroupItem newParent, System.Int32 newIndex)
public System.Void TestsRemove(ClashTest test)
public System.Void TestsRemove(GroupItem parent, SavedItem item)
public System.Void TestsRemoveAt(System.Int32 index)
public System.Void TestsRemoveAt(GroupItem parent, System.Int32 index)
public System.Void TestsReplaceWithCopy(GroupItem parent, System.Int32 index, SavedItem item)
public System.Void TestsReplaceWithCopy(System.Int32 index, ClashTest test)
public System.Void TestsRunAllTests()
public System.Void TestsRunTest(ClashTest test)
public System.Void TestsSortResults(ClashTest test, ClashResultSortMode sortBy, ClashSortDirection direction, Point3D proximityTo)
public System.Void TestsSortTests(ClashTestSortMode sortBy, ClashSortDirection direction)
public Autodesk.Navisworks.Api.Viewpoint TestsViewpointForResult(IClashResult result)
```

`Autodesk.Navisworks.Api.Clash.ClashTest`, base type `Autodesk.Navisworks.Api.GroupItem`.
One public constructor, `ClashTest()`, taking no arguments.

Properties, inherited ones marked:

```
public SavedItemReference AnimatorSimulation { get; set }
public SavedItemCollection Children { get }                 [from GroupItem]
public CommentCollection Comments { get }                   [from SavedItem]
public string CustomTestName { get; set }
public string DisplayName { get; set }                      [from SavedItem]
public guid Guid { get; set }                               [from SavedItem]
public RuleCollection IgnoreRules { get }
public bool IsDisposed { get }                              [from NativeHandle]
public bool IsGroup { get }                                 [from SavedItem]
public bool IsReadOnly { get }
public System.Nullable[datetime] LastRun { get }
public bool MergeComposites { get; set }
public GroupItem Parent { get }                             [from SavedItem]
public ClashSelection SelectionA { get }
public ClashSelection SelectionB { get }
public double SimulationStep { get; set }
public SimulationType SimulationType { get; set }
public ClashTestStatus Status { get; set }
public ClashTestType TestType { get; set }
public double Tolerance { get; set }
```

The only declared public methods are two internal-use statics, `InternalCreator` and
`InternalFactory`. A test is built by setting properties, not by calling methods.

`SelectionA` and `SelectionB` are get only, so a side is filled in by working on the
`ClashSelection` it already holds. `ClashSelection` public members:

```
public bool IsDisposed { get }
public bool IsReadOnly { get }
public Autodesk.Navisworks.Api.PrimitiveTypes PrimitiveTypes { get; set }
public Autodesk.Navisworks.Api.Selection Selection { get }
public bool SelfIntersect { get; set }
```

`ClashTestType` is an enum over int with these five values:

```
Hard             = 0
HardConservative = 1
Clearance        = 2
Duplicate        = 3
Custom           = 4
```

So `test_type="hard_conservative"` in the XML maps to `ClashTestType.HardConservative`,
value 1. That answers the CLAUDE.md question about which value matches.

Two related enums, read at the same time because the reader carries the raw strings:

```
ClashTestStatus   : New = 0, Old = 1, Partial = 2, Complete = 3
ClashResultStatus : New = 0, Active = 1, Reviewed = 2, Approved = 3, Resolved = 4
```

`status="new"` maps to `ClashTestStatus.New`. `ClashResultStatus` is the one the Excel
report counts by, and New and Active are the two the image option covers.

NOT CHECKED this session: the Clash Detective report defaults. That needs the running
application, not reflection over the assembly. Still UNKNOWN.

### 5. Is nuget.org reachable

YES. Tested by restoring ClosedXML into a scratch folder outside this repo, at
`%LOCALAPPDATA%\Temp\claude\...\scratchpad\nugettest\probe`.

`dotnet add package ClosedXML` succeeded with exit code 0. It resolved ClosedXML 0.105.1
and pulled ClosedXML.Parser 2.0.0, DocumentFormat.OpenXml 3.1.1,
DocumentFormat.OpenXml.Framework 3.1.1, ExcelNumberFormat 1.1.0, RBush.Signed 4.0.0,
SixLabors.Fonts 1.0.0 and System.IO.Packaging 8.0.1, all from
`https://api.nuget.org/v3/index.json`. Restore took 5.35 seconds. No error.

So the test project uses NUnit, per the instruction. See "Which test framework" below.

### 6. dotnet SDKs, MSBuild, and whether net48 can be built

```
dotnet SDK in use : 8.0.424   (base path C:\Program Files\dotnet\sdk\8.0.424\)
MSBuild in the SDK: 17.11.48+02bf66295
host runtime      : 10.0.11, win-x64
SDKs installed    : 8.0.420, 8.0.423, 8.0.424
runtimes          : Microsoft.NETCore.App 8.0.25, 8.0.29, 8.0.30, 9.0.19, 10.0.11
                    Microsoft.WindowsDesktop.App 8.0.22, 8.0.25, 8.0.29, 8.0.30, 9.0.19, 10.0.11
                    Microsoft.AspNetCore.App 8.0.30, 9.0.19, 10.0.11
global.json       : not found
workloads         : none installed
```

Other MSBuild on the machine:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe   PRESENT
vswhere.exe                                                    ABSENT, so no Visual Studio is installed
C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework   ABSENT
```

That last line is the one that matters. There is no .NET Framework targeting pack on this
machine, so net48 cannot be built from the machine alone.

CAN THIS MACHINE BUILD net48: YES, through the
`Microsoft.NETFramework.ReferenceAssemblies` NuGet package, which every project here
references. Proved before any project was created, with a throwaway net48 NUnit project
in the scratchpad:

```
probe48 -> ...\net48\probe48.dll
Test run for ...\probe48.dll (.NETFramework,Version=v4.8)
VSTest version 17.11.1 (x64)
Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 65 ms - probe48.dll (net48)
```

This is why nuget.org matters twice over. Without it this machine cannot build net48 at
all, package or no package.

### 7. The schemas folder

PRESENT. `C:\Program Files\Autodesk\Navisworks Manage 2025\schemas` holds
`nw-exchange-12.0.xsd`, which is the schema all three sample files name in their
`xsi:noNamespaceSchemaLocation`.

Full contents of that folder:

```
nw-exchange-11.0.xsd            nw-TakeoffCatalog-10.0.xsd
nw-exchange-12.0.xsd            nw-TakeoffConfiguration-10.0.xsd
nw-exchange-4.0.1.xsd           nw-TakeoffProperties-10.0.xsd
nw-exchange-4.0.xsd             nw-TakeoffPropertyMap-10.0.xsd
nw-exchange-5.0.xsd             nw-TakeoffQuantity-10.0.xsd
nw-exchange-6.0.xsd
nw-exchange-7.0.xsd
nw-exchange-8.0.xsd
```

## Build

The command, now that the scan has run.

Build everything, which needs Navisworks on the machine because of the add-in project:

```
dotnet build ParsonsNwcFederator.sln -c Release
```

Build and test the parts that need no Navisworks, which is all of Federator.Core:

```
dotnet test tests\Federator.Core.Tests\Federator.Core.Tests.csproj
```

The add-in project finds Navisworks through the `NavisworksPath` MSBuild property, which
defaults to `C:\Program Files\Autodesk\Navisworks Manage 2025`. On a machine that put it
elsewhere:

```
dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"
```

The project fails with a clear message rather than a missing-reference error when that
path holds no `Autodesk.Navisworks.Api.dll`.

## Which test framework, and why

NUnit, with `Microsoft.NET.Test.Sdk` 17.11.1, `NUnit` 3.14.0 and `NUnit3TestAdapter`
4.6.0.

The instruction was to use NUnit if step 5 showed nuget.org reachable, and a plain console
exe with a hand-written assert helper if it did not. Step 5 showed nuget.org reachable, so
NUnit it is. The console fallback was not built and is not in the tree.

There was no real choice here in any case. With no .NET Framework targeting pack on the
machine, even the no-package console option would still have needed
`Microsoft.NETFramework.ReferenceAssemblies` from nuget.org to compile for net48.

## What the sample files actually hold

Every number here was measured this session, twice and by two different routes: once with
a standalone script reading the XML directly, and again by the tests in
`tests\Federator.Core.Tests`. They agree.

### 1104-PAR_CLASH_AllInOne (2) (1).xml

```
root exchange, units="ft", schema nw-exchange-12.0.xsd
1 batchtest, name and internal_name both "Clash Test Building", units="ft"
1830 clashtest children, all 1830 names unique
every test  : test_type="hard_conservative", status="new", merge_composites="1"
tolerance   : 0.2460629921 on every test, which is 75.0 mm exactly
linkage     : mode="none" on every test
rules       : empty on every test
each side   : one clashselection, selfintersect="0", primtypes="1", one locator
61 unique test locators, 0 self pairs, 1830 unique unordered pairs, which is 61*60/2
61 selection sets
folders     : Architecture 16 sets, Structure 6, Electrical 14,
              Mechanical 0 sets and 4 subfolders:
              Mechanical-HVAC 9, Mechanical-Fire Fighting 6,
              Mechanical-Water Supply 5, Mechanical-Drainage 5
deepest set : 2 folders down
102 conditions across the 61 sets, 1 or 2 or 4 per set
              30 sets have 1, 26 have 2, 5 have 4
53 distinct rules once the flags attribute is set aside
6 conditions carry no category element, only a property
every findspec: mode="all", disjoint="0", locator "/"
all 61 test locators resolve against this file own sets
```

### CORRECTION: the brief was wrong about this file

The brief said, of this file, "file holds no sets at all". That is not true of the file on
disk. It holds 61 selection sets, in a nested folder tree two deep, carrying 102
conditions and 53 distinct rules. The name of the file, AllInOne, says the same thing.

Everything else the brief said about this file checked out exactly.

Two possibilities, and this session cannot tell them apart: either the note was written
against a different export, or it was written from the clashtests half of this one. Either
way, the tests in `AllInOneFileTests.cs` assert what the file on disk holds, including
`TheFileAlsoHoldsSixtyOneSets`. FOR BADER: confirm which file was meant, and I will
change the assertion if this is the wrong one.

The knock-on effect is that the cross-file check still lands where the brief said it
would, but for a reason the brief did not give. See "Cross file" below.

### Search Set Building.xml

```
root exchange, units="ft"
1 batchtest "Clash Test Building", 1830 clashtest children, 61 selection sets
                                   both shapes, in one file
tolerance   : 0.1640419948 on every test, which is 50.0 mm exactly
each side   : selfintersect="1", primtypes="1"
all 61 test locators resolve against this file own sets
61 conditions, exactly one per set
1 distinct condition, so all 61 are identical down to the flags attribute
that one rule: test="equals" flags="0", category Category/Category,
               property Name/Name, value wstring "Floors"
folders     : Architecture 16, Structure 6, Electrical 14,
              Mechanical 2 subfolders (Fire Protection 6, HVAC 9),
              Plumbing 2 subfolders (Sewer 5, Water Supply 5)
HEALTH      : UNUSABLE. Every set asks the model the same question.
```

A trap in this file, found because a test failed on it. Two of the 61 set names end in a
space:

```
"BLD-ME-FD_Flex Ducts "        (note the trailing space)
"BLD-SD_Security Devices "     (note the trailing space)
```

The 120 test locators that point at those two sets carry the trailing space as well, so
the file is internally consistent and all 61 resolve. The first version of the reader
trimmed locator text and broke exactly those two, resolving 59 of 61. CLAUDE.md says to
match the exact string, and this is why. The reader now reads locators and name elements
byte for byte with no trimming, and `BuildingSetsFileTests` has a test that pins this.

Checked across all three files: the only text carrying edge whitespace anywhere is those
120 locator elements and those 2 selectionset name attributes. No category, property or
value is affected.

### Search Set Infra.xml

```
root exchange, units="ft"
no batchtest and no clashtest at all, sets only
26 selection sets
2715 conditions in total
2 distinct condition elements, but only 1 distinct rule
    2689 conditions with flags="64" and 26 with flags="0"
    all 2715 are identical in test, category, property and value:
    test="equals", category Category/Category, property Name/Name, value wstring "Floors"
    flags is the bit that joins one condition to the next, it is not part of the rule
conditions per set: 1, 3, 9, 27, 81 and 324
    6 sets have 1, 3 have 3, 3 have 9, 3 have 27, 4 have 81, 7 have 324
    6*1 + 3*3 + 3*9 + 3*27 + 4*81 + 7*324 = 2715
folders     : Pressure Networks with 4 subfolders (District Cooling 3, Firefighting 3,
                Irrigation 3, Potable Water 3)
              GAS Networks with 1 subfolder (Natural Gas 3)
              Gravity Networks with 3 subfolders (Treated Sewage 4, Foul Sewerage 6,
                Storm Water 0, an empty folder)
three sets share a base name, differing only by a Navisworks copy suffix:
    INF-FS-MH_Manholes, INF-FS-MH_Manholes (1), INF-FS-MH_Manholes (2)
one set sits at the root with no folder above it: "Search Set"
HEALTH      : UNUSABLE. Every set asks the model the same question.
```

A note on "all identical". As literal strings the three Manholes names are distinct, and
the 2715 condition elements come in two shapes because of the flags attribute. The health
check therefore works on two ideas rather than one raw string compare. It groups names by
base name after stripping a trailing " (n)", which is how Navisworks writes a copy, and it
builds a rule signature from test, category, property and value while leaving flags out.
On that footing both statements in the brief hold: three sets share the name, and all 2715
conditions ask for the same thing.

### Cross file

```
1104 test locators against Search Set Building sets : 0 of 61 resolve
Building test locators against 1104 sets            : 0 of 61 resolve
1104 test locators against 1104 own sets            : 61 of 61
Building test locators against Building own sets    : 61 of 61
```

The brief said pairing the 1104 tests with the Building sets resolves 0 of 61, and it
does. The reason is the set names, not a missing sets section. The two exports use
different naming: the 1104 file has `BLD-AR-Floors` where the Building file has
`BLD-AR-FR_Floors`, `BLD-ME-Air Terminals` against `BLD-ME-AT_Air Terminals`, and so on
through all 61. The folder trees differ too. So the locators never line up.

## Job 6: what guards the tests

The pre-commit hook is what actually guards commits today. It lives at
`.githooks/pre-commit` and is turned on for this clone with:

```
git config core.hooksPath .githooks
```

That config was set this session. It is local to this clone and is not carried in the
repository, so anyone who clones this has to run that one line before the hook does
anything for them.

The hook runs `dotnet test` over the whole test project and exits non zero if anything
fails, which refuses the commit. It also refuses, rather than passing quietly, when
`dotnet` is not on PATH, so a missing toolchain can never read as a green run.

`.github/workflows/tests.yml` runs the same command on pull requests. It WILL NOT FIRE
until this repository has a remote on GitHub. Right now `git remote -v` is empty, so the
workflow is a file sitting in the tree and nothing more. Until a remote exists, the hook
is the only thing standing between a broken test and a commit.

The workflow builds and tests Federator.Core only. It does not build Federator.Addin,
because that project references the Navisworks DLLs by path and no hosted runner has
Navisworks on it. The add-in has to be built on a machine that has Navisworks Manage 2025,
which is what the `NavisworksPath` property is for.

## Open questions for Bader

1. The 1104 file holds 61 sets. The brief said it holds none. Confirm which file was
   meant. The tests currently assert what is on disk.
2. CLAUDE.md says an output name carries BM for the discipline, ZZZ for the level and
   000001 for the number. Job 4 said to swap the discipline and keep everything else. The
   sample name already has ZZZ and 000001, so it cannot tell the two apart. The code does
   what Job 4 said and swaps only the discipline. The level and number rule is there as
   settings that default to off: set `LevelPart`/`ForcedLevel` and
   `NumberPart`/`ForcedNumber` on `ContainerNameSettings` to turn it on, no code change.
   Say which you want and I will set the default.
3. The Clash Detective report defaults are still UNKNOWN. Reflection cannot read them,
   they need the running application. Tell me where to look or run it once and I will read
   it from there.
