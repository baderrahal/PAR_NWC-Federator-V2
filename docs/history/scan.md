# Scan

Measurement history, not current. Moved here on 2026-09-12, F37. Every path and line number in it is as it was on the date at the top, and the code has moved on since.

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

### 4b. What the model side needs, added 2026-08-29

The first scan recorded the four Document methods but not the types around them, so the
model side could not have been written without inventing something. Read by reflection off
the same install, same way, on 2026-08-29. Assembly `Autodesk.Navisworks.Api.dll`, version
22.0.0.0.

Reaching the running application. `Autodesk.Navisworks.Api.Application` is sealed with no
public constructor. Everything on it is static:

```
public static Autodesk.Navisworks.Api.Document ActiveDocument { get }
public static Autodesk.Navisworks.Api.Document MainDocument { get }
public static ReadOnlyCollection[Document] Documents { get }
public static Autodesk.Navisworks.Api.ApplicationParts.IApplicationGui Gui { get }
public static bool IsAutomated { get }
public static string Title { get }
public static Autodesk.Navisworks.Api.ApplicationParts.ApplicationVersion Version { get }
public static Autodesk.Navisworks.Api.Progress BeginProgress()
public static Autodesk.Navisworks.Api.Progress BeginProgress(System.String title)
public static Autodesk.Navisworks.Api.Progress BeginProgress(System.String title, System.String message)
public static System.Void EndProgress()
```

`ActiveDocument` is get only, so the add-in works on the document Navisworks already has
open. It never makes one.

Owning a window. `Application.Gui.MainWindow` is a `System.Windows.Forms.IWin32Window`,
not a WPF Window, so a WPF dialog is parented through its `Handle` with
`WindowInteropHelper`. That is why Federator.Addin references System.Windows.Forms.

```
System.Windows.Forms.IWin32Window MainWindow { get }
```

Document state, used to check what a Clear would throw away and to verify a write:

```
public string CurrentFileName { get }
public string FileName { get }
public string SuggestedFileName { get }
public string Title { get }
public bool IsClear { get }
public Autodesk.Navisworks.Api.DocumentParts.DocumentModels Models { get }
```

`DocumentModels.Count` is an int, which is how many models are loaded after an append.

The Try forms, which return a bool instead of throwing. These are the ones the engine
uses, because one bad NWC must not stop a group:

```
public System.Boolean TryAppendFile(System.String fileName)
public System.Boolean TryAppendFiles(System.Collections.Generic.IEnumerable`1[System.String] fileNames)
public System.Boolean TrySaveFile(System.String fileName)
public System.Boolean TrySaveFile(System.String fileName, Autodesk.Navisworks.Api.DocumentFileVersion fileVersion)
public System.Boolean TryPublishFile(System.String fileName, Autodesk.Navisworks.Api.PublishProperties properties)
```

`DocumentFileVersion` is an enum over int. Note that every year from 2016 to 2025 is the
same number, 448:

```
Current = 0
Navisworks2015 = 441
Navisworks2016 = 448   Navisworks2021 = 448
Navisworks2017 = 448   Navisworks2022 = 448
Navisworks2018 = 448   Navisworks2023 = 448
Navisworks2019 = 448   Navisworks2024 = 448
Navisworks2020 = 448   Navisworks2025 = 448
```

`PublishProperties` has a parameterless constructor, which is what `PublishFile` needs.
Base type `NativeHandle`, so it is disposable:

```
public PublishProperties()
public PublishProperties(Autodesk.Navisworks.Api.PublishProperties value)

public bool AllowResave { get; set }              public string Keywords { get; set }
public string Author { get; set }                 public bool PreventObjectPropertyExport { get; set }
public string Comments { get; set }               public datetime PublishDate { get; set }
public string Copyright { get; set }              public string PublishedFor { get; set }
public bool DisplayAtPassword { get; set }        public string Publisher { get; set }
public bool DisplayOnOpen { get; set }            public string Subject { get; set }
public bool EmbedDatabaseProperties { get; set }  public string Title { get; set }
public bool EmbedTextures { get; set }
public bool HasBeenResaved { get }                public bool HasExpiryDate { get }
public bool HasPassword { get }                   public bool IsReadOnly { get }
public datetime ExpiryDate { get; set }

public System.Void RemoveExpiryDate()
public System.Void RemovePassword()
public System.Void SetPassword(System.String password)
```

The plugin base class. `AddInPlugin` is abstract and derives from `Plugin`:

```
Autodesk.Navisworks.Api.Plugins.AddInPlugin, abstract, base Plugin
    public Autodesk.Navisworks.Api.Plugins.CommandState CanExecute()
    public System.Int32 Execute(System.String[] parameters)
    public System.Boolean TryShowHelp()

Autodesk.Navisworks.Api.Plugins.Plugin, abstract
    public string DeveloperId { get }
    public string Id { get }
    public string Name { get }
    public Autodesk.Navisworks.Api.Plugins.PluginRecord PluginRecord { get }
```

`Execute` returns an int, so the plugin returns 0 for done.

The two attributes a plugin carries:

```
Autodesk.Navisworks.Api.Plugins.PluginAttribute, sealed
    public PluginAttribute(string name, string developerId)
    DisplayName { get; set }   ToolTip { get; set }   ExtendedToolTip { get; set }
    Options { get; set }       SupportsIsSelfEnabled { get; set }
    Name { get }               DeveloperId { get }

Autodesk.Navisworks.Api.Plugins.AddInPluginAttribute, sealed
    public AddInPluginAttribute(Autodesk.Navisworks.Api.Plugins.AddInLocation location)
    Icon { get; set }          LargeIcon { get; set }   CanToggle { get; set }
    LoadForCanExecute { get; set }   Shortcut { get; set }
    ShortcutWindowTypes { get; set } CallCanExecute { get; set }
    Location { get }
```

The enums those attributes take:

```
AddInLocation   : None = 0, AddIn = 1, Import = 2, Export = 3, Help = 4,
                  CurrentSelectionContextMenu = 5, CurrentSelection2DContextMenu = 6
PluginOptions   : None = 0, SupportsControls = 1
CallCanExecute  : Always = 0, DocumentNotClear = 1,
                  CurrentSelectionSingle = 2, CurrentSelectionMultiple = 3
```

`AddInLocation.AddIn` is the value that puts a button on the Tool Add-ins tab.

`CommandState` is a class, not an enum, so `CanExecute` returns a new one:

```
public CommandState()
public CommandState(bool enabled)
public bool IsEnabled { get; set }   public bool IsChecked { get; set }
public bool IsVisible { get; set }   public string OverrideDisplayName { get; set }
```

### 4e. Reading what an NWF points at, added 2026-08-30

Needed so a rerun can tell whether an existing NWF still matches the group, without
keeping a side file. The NWF is the record.

```
Autodesk.Navisworks.Api.Document
    public System.Void OpenFile(System.String fileName)
    public System.Boolean TryOpenFile(System.String fileName)
    public System.Void OpenAggregate(System.String aggregateJson, System.String progressMedia)
    public System.Boolean TryOpenAggregate(System.String aggregateJson, System.String progressMedia)

Autodesk.Navisworks.Api.DocumentParts.DocumentModels
    public int Count { get }
    public Autodesk.Navisworks.Api.Model First { get }
    public Autodesk.Navisworks.Api.Model Item { get; set }
    public IEnumerator`1[Autodesk.Navisworks.Api.Model] GetEnumerator()

Autodesk.Navisworks.Api.Model, base NativeHandle
    public string SourceFileName { get }
    public string FileName { get }
    public string Creator { get }
    public guid SourceGuid { get }
    public Autodesk.Navisworks.Api.PublishProperties PublishProperties { get }
```

`Model` carries two names. This section used to say the difference between them was
UNKNOWN, and guessed that `SourceFileName` was the file that was appended. That guess was
wrong.

SETTLED on 2026-08-30 by a real run, and the log is the measurement:

- `FileName` holds the NWC that was appended, matching the scanned path exactly on every
  line of that log
- `SourceFileName` holds the container the NWC was published from, which on this project
  is a Revit file in Autodesk Docs:

      Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt

So the comparison uses `FileName`, and falls back to `SourceFileName` only when FileName
is empty. The wrong way round can never match a scanned NWC path, and it reported CHANGED
for 22 of 22 groups on a run where nothing had changed. Because a CHANGED group is left
alone entirely, that also meant no NWF was reused, no set was built and no test ran.

Logging both whenever they disagree is what made this findable, so that stays. The choice
itself is in `Federator.Core.Rerun.ModelFileNames` rather than in the add-in, so it can be
tested without Navisworks. It went unnoticed precisely because it was the one part of the
comparison no test could reach.

`SourceFileName` is now read for a second purpose. The Revit container inside an NWC is
often a different building from the NWC itself, so the run reports SOURCE MISMATCH where
the two building codes differ and SHARED SOURCE where one Revit building feeds two groups.
Measured pairs from that run, NWC against Revit: 1B06BC/0000BC, 1B06BS/1A02BS,
1B06G1/0000PG and 1B06PG, 1B06K1/0000KI, 1B06KI/0000KI, 1B06M1/1B06MM, 1B06P1/0000PW.
1C06PK carries 1C06PK on both sides and differs only in the number, so it is not a
mismatch. Both codes are read with the same parser used on the NWC names.

### 4f. Creating and running clash tests, added 2026-08-30

Section 4 recorded `DocumentClashTests`, `ClashTest` and the three enums, but not how a
document is reached from the clash side, not how a side is pointed at a saved selection
set, and not what the tolerance is measured in. Read by reflection off the same install,
same way, on 2026-08-30.

Reaching the clash tests from a document. It is an extension method, confirmed to carry
`ExtensionAttribute`, on a static class:

```
Autodesk.Navisworks.Api.Clash.DocumentExtensions
    public static DocumentClash GetClash(this Document doc)

Autodesk.Navisworks.Api.Clash.DocumentClash
    public Autodesk.Navisworks.Api.Clash.DocumentClashTests TestsData { get }
    public static DocumentClash ClashInstance(Document doc)
    public static DocumentClash CreateInstance(Document doc)
    public bool TryCalculateMinimumClearance(ModelItemCollection selection1,
        ModelItemCollection selection2, bool useCenterlines, out MinimumClearanceResult result)
```

So `document.GetClash().TestsData` is the `DocumentClashTests` that section 4 recorded.

Pointing a side at a saved selection set. This is the one that was missing, and it is why
a clash side does not need a copy of the items. `SelectionSource` has NO public
constructor, both its constructors are non public, so the only way to get one is to ask
the document for it:

```
Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets
    public SelectionSource CreateSelectionSource(SavedItem item)
    public SavedItem ResolveSelectionSource(SelectionSource source)
    public SavedItemReference CreateReference(SavedItem item)
    public SavedItem ResolveReference(SavedItemReference reference)

Autodesk.Navisworks.Api.SelectionSource, base NativeHandle
    public Search TryGetSearch(Document document)
    public ModelItemCollection TryGetSelectedItems(Document document)

Autodesk.Navisworks.Api.Selection
    public SelectionSourceCollection SelectionSources { get }
    public bool HasSelectionSources { get }
    public bool HasExplicitSelection { get }
    public ModelItemCollection ExplicitSelection { get }
    public System.Void Clear()
    public System.Void CopyFrom(Selection from)
    public System.Void CopyFrom(ModelItemCollection from)
    public ModelItemCollection GetSelectedItems(Document document)

Autodesk.Navisworks.Api.SelectionSourceCollection, base NativeHandle
    public SelectionSourceCollection()
    public int Count { get }
    public System.Void Add(SelectionSource item)
    public System.Void Clear()
```

There are two ways to fill a side and they are not the same thing:

- `Selection.SelectionSources.Add(document.SelectionSets.CreateSelectionSource(set))`
  points the side at the set itself, which is what Clash Detective does when a person
  picks a set in the panel. The side stays live, so the test re-resolves the set when it
  runs
- `Selection.CopyFrom(items)` copies a fixed list of items in, which is a snapshot taken
  at the moment the test was created

The runner uses the first. CLAUDE.md requires the clash counts in the report to match the
Clash Detective panel exactly, and a snapshot can drift from the set the panel shows.

Counting results. `ClashTest` is a `GroupItem`, so its results are its `Children`, and
both concrete result types implement `IClashResult`, confirmed by reflection:

```
ClashResult      : SavedItem, implements IClashResult
ClashResultGroup : GroupItem,  implements IClashResult
IClashResult
    public ClashResultStatus Status { get; set }
    public string DisplayName { get; set }
    public double Distance { get; set }
    public ModelItemCollection Selection1 { get }
    public ModelItemCollection Selection2 { get }
```

So walking `test.Children` and reading `Status` off each child as an `IClashResult`
counts a test by status, and a group counts as the one result the panel shows.

The tolerance and the units. `Document` carries the units it is displayed in:

```
Autodesk.Navisworks.Api.Document
    public Autodesk.Navisworks.Api.Units Units { get }      get only, no setter

Autodesk.Navisworks.Api.Units, over int, NOT a flags enum
    Meters = 0        Feet = 3        Yards = 5        Micrometers = 8
    Centimeters = 1   Inches = 4      Kilometers = 6   Mils = 9
    Millimeters = 2                   Miles = 7        Microinches = 10
```

`ClashTest.Tolerance` is a plain double with no units on it. Which units it is measured in
is NOT readable by reflection and is UNKNOWN until a test runs against a real model.
CLAUDE.md already records the rule as document units, so the runner converts the file
tolerance into `document.Units` before setting it, and logs both numbers with both unit
names on every test so the first real run settles it from the log alone rather than by
anyone guessing again.

`PrimitiveTypes`, which is what the file writes as `primtypes`, IS a flags enum:

```
Autodesk.Navisworks.Api.PrimitiveTypes, [Flags] over int
    None = 0   Triangles = 1   Lines = 2   Points = 4   SnapPoints = 8   Text = 16
```

`primtypes="1"` is Triangles. The runner casts the int through and reports any bit that is
not one of these rather than dropping it silently.

What is still UNKNOWN here and needs Navisworks running:

- which units `ClashTest.Tolerance` is in
- whether `TestsRunTest` blocks until the test has finished, or returns while it runs
- whether `TestsAddCopy` takes a copy the way `DocumentSelectionSets.AddCopy` does. Its
  name says so and section 4d proved it for sets, so the runner reads the test back out
  of `TestsData.Tests` by name after adding rather than holding the object it handed in

### 4g. How long a handle lives, measured 2026-08-31

A run created 1830 tests in each of 24 groups, ran for 8 hours 52 minutes and produced
nothing. Every test threw the same thing:

    System.ObjectDisposedException: Object has been Disposed (WeakRef)
    Object name: 'NativeHandle'
       at Autodesk.Navisworks.Api.GroupItem.get_Children()
       at Federator.Addin.Engine.ClashRunner.Count(ClashTest test, String name)

Read by reflection and by decoding the IL off the same install. This is the measurement,
not a theory about it.

`NativeHandle` is the base of every API object this tool touches. Its private fields:

```
LcUWeakReferenceHandle* m_weak_ref          a WEAK reference to the native object
void*                   m_native_ptr
NativeHandleOwnership   Ownership
static IObjectManager   m_s_object_manager  one table for the whole process
```

It has a finalizer, a `Dispose`, a `CleanupAnyWeakRef` and an `InvalidateObject`. So a
managed wrapper does not keep the native object alive. It watches it.

`Autodesk.Navisworks.Internal.ApiImplementation.NativeHandleOwnership`:

```
eINVALID = 0   eEXTERNAL = 1   eWRAPPER = 2   eREF_COUNTED = 3   eWEAK_REFERENCE = 4
```

Which one a wrapper gets was read out of the IL. `SavedItemCollection` indexer calls
`GroupItem.GetChild`, and `GroupItem.GetChild` ends:

```
    call     .LcOpGroupItem.GetChild
    ldc.i4.1
    call     SavedItem.InternalCreator
```

`ldc.i4.1` is the ownership argument, and 1 is `eEXTERNAL`. So:

**Every SavedItem read out of a SavedItemCollection is a borrowed view of an object the
document owns.** It is valid only while that native object is, and it is safe to dispose,
because disposing an eEXTERNAL wrapper releases the wrapper and never the document's
object.

What destroys the native object is any mutation through the owning document part. Every
mutator on `DocumentClashTests` is a copy form, `TestsAddCopy`, `TestsEditTestFromCopy`,
`TestsReplaceWithCopy`, exactly as `DocumentSelectionSets.AddCopy` is, and section 4d
already recorded for the sets that a handle held across an `AddCopy` does not see the
result. The same holds here, and `TestsRunTest` is a mutation too, because it writes the
results into the test.

So the rule, which the runner now follows:

- a `ClashTest` handed to `TestsRunTest` is DEAD when that call returns
- reading `Children` off it throws `ObjectDisposedException ... (WeakRef)`, which is
  exactly the stack above
- a test is addressed by where it sits, never held. Resolve it, use it, dispose it, and
  resolve it again after anything that mutates the tests

The run had actually run the tests. `TestsRunTest` is above `Count` in that stack and
returned normally, so the count of `0 run` is bookkeeping and not a description: the
outcome is only recorded after the results are counted, and counting threw first.

Two things this also explains, and both are now gone:

- the old code called `FindTest`, which walked the whole tests collection, once per test.
  Over 1830 tests that is about 1.7 million `SavedItem` wrappers built and thrown away per
  group, each one a finalizable native handle registered against the one static
  `IObjectManager`. It is now an index, built once, and `TestsAddCopy` appends at the root
  so the new test is at the index the count was before the add, checked by name
- nothing was ever disposed. Everything in the list below is `IDisposable`, and now what
  the tool creates or resolves is disposed:

```
ClashTest  ClashResult  ClashResultGroup  ClashSelection  ClashTestsData
SelectionSet  SelectionSource  SelectionSourceCollection  Selection
ModelItemCollection  Search  FolderItem  SavedItem
```

`SavedItemCollection` is NOT disposable, so it is a view and is never disposed.

Still UNKNOWN, and only a real run answers it:

- whether the tests that did run produced results, since nothing could read them
- whether sets, tests or results survive from one group into the next document. The
  evidence points at no, because every one of the 24 groups reported 1830 created rather
  than already there, which means the collection was empty each time. The runner now logs
  the count on every group even when it is zero, so the next log answers this outright
- what exactly made the clash step grow from 39.8 seconds to 1259.5 seconds. The two
  measured candidates above are both removed, so the next run either shows a flat time or
  shows that something else grows

### 4h. The clash report XML, and open against closed, measured 2026-08-31

Two things the Excel session had to settle before writing anything.

#### There is no clash report schema, but there is a definition

The `schemas` folder holds only `nw-exchange-*.xsd` and `nw-Takeoff*-10.0.xsd`, listed in
full in section 7. There is NO clash report XSD anywhere in the install. Searched the whole
install folder for `*.xsd`, and those are all of them.

The shape is still shipped, in the stylesheets Navisworks uses to render its own clash
reports. Three of them, in every language folder:

```
C:\Program Files\Autodesk\Navisworks Manage 2025\en-US\stylesheets\
    clash_report_html.xsl            19066 bytes
    clash_report_html_tabular.xsl    28748 bytes
    clash_report_text.xsl            14857 bytes
```

An XSL says exactly which elements and attributes the XML it transforms carries, so the
shape below is read rather than invented. Every name here came out of the `match` and
`select` expressions in those three files:

```
exchange @units
  batchtest @name @internal_name
    clashtests
      clashtest @name @test_type @status @tolerance
        summary @total @new @active @reviewed @approved @resolved
        clashresults
          clashgroup  @name @distance @href @status
          clashresult @name @distance @href
            both carry the same children:
              resultstatus                 text
              description                  text
              clashpoint / pos3f @x @y @z
              gridlocation                 text
              createddate / date @day @month @year, time @hour @minute @second
              approveddate, approvedby, assignedto
              clashobjects
                clashobject
                  layer                    text
                  pathlink / node          text, one node per path step
                  objectattribute          name, value
                  smarttags / smarttag     name, value
              clashtasklink
                starttime endtime taskname tasklink taskuid animatorscene animatoranim
              linkage, linkedanimation, clipplaneset, view / camera
```

`clashgroup` and `clashresult` are siblings under `clashresults`. A group holds the
clashes in it and the text stylesheet says of its fields: for group fields marked with an
asterisk, the most significant value from the group is shown.

#### The clash API does not distinguish open from closed

Searched both assemblies for any public member named `IsOpen`, `IsClosed`, `IsResolved`,
`IsActive`, `OpenCount`, `ClosedCount` or `Outstanding`. Nothing on `IClashResult`,
`ClashResult`, `ClashResultGroup`, `ClashTest` or `DocumentClashTests` has any of them.
The only hits anywhere were unrelated: `Document.IsActiveTransaction`, an animation
controller, a measure tool and a data reader.

`ClashResultStatus` is a flat enum of five values with nothing grouping them:

```
New = 0   Active = 1   Reviewed = 2   Approved = 3   Resolved = 4
```

Navisworks' own clash report does not have the notion either. None of the three
stylesheets mentions open, closed or outstanding anywhere.

So: **UNKNOWN from the API, and the tool never derives one.** Every status is reported as
itself, all five of them, everywhere. Where the matrix needs a single number for what is
still outstanding it uses New plus Active, because that is the rule Bader stated, and the
column is labelled as that sum rather than as "open" so nobody reads an API meaning into
it that the API does not have.

#### The workbook library

ClosedXML 0.105.1, restored from nuget.org, which section 5 already recorded as reachable.
It builds clean against net48 with no NU warnings. It ships twelve DLLs into the bundle:

```
ClosedXML.dll  ClosedXML.Parser.dll  DocumentFormat.OpenXml.dll
DocumentFormat.OpenXml.Framework.dll  ExcelNumberFormat.dll
Microsoft.Bcl.HashCode.dll  RBush.dll  SixLabors.Fonts.dll  System.Buffers.dll
System.Memory.dll  System.Numerics.Vectors.dll  System.Runtime.CompilerServices.Unsafe.dll
```

Checked each one against the Navisworks install folder: **none of them collide with a file
Navisworks ships**, so nothing is at risk of binding to the wrong version.

The writer lives in Federator.Core rather than in the add-in, so the tests write a real
xlsx and read it back without Navisworks.

### 4i. Why the workbook would not load, measured 2026-08-31

A run on aa163c9e did everything right and then failed writing the workbook:

    TypeInitializationException, SixLabors.Fonts.Tables.TableLoader
    inner FileNotFoundException: Could not load file or assembly
    'System.Numerics.Vectors, Version=4.1.3.0'

`System.Numerics.Vectors.dll` was already in the bundle. Every other file was too.

#### What is actually in the bundle, and at what version

Read with `AssemblyName.GetAssemblyName` off the installed bundle:

```
ClosedXML                                0.105.1.0
ClosedXML.Parser                         1.0.0.0
DocumentFormat.OpenXml                   3.1.1.0
DocumentFormat.OpenXml.Framework         3.1.1.0
ExcelNumberFormat                        1.1.0.0
Federator.Addin                          1.0.0.0
Federator.Core                           1.0.0.0
Microsoft.Bcl.HashCode                   1.0.0.0
RBush                                    4.0.0.0
SixLabors.Fonts                          1.0.0.0
System.Buffers                           4.0.3.0
System.Memory                            4.0.1.2
System.Numerics.Vectors                  4.1.4.0
System.Runtime.CompilerServices.Unsafe   4.0.6.0
```

NOT ONE FILE IS MISSING. Every reference resolves to a file that is present. What does
not match is the version, in six places:

```
ClosedXML         wants System.Buffers 4.0.2.0                       the file is 4.0.3.0
ClosedXML.Parser  wants System.Memory 4.0.1.1                        the file is 4.0.1.2
SixLabors.Fonts   wants System.Memory 4.0.1.1                        the file is 4.0.1.2
SixLabors.Fonts   wants System.Buffers 4.0.2.0                       the file is 4.0.3.0
SixLabors.Fonts   wants System.Numerics.Vectors 4.1.3.0              the file is 4.1.4.0
System.Memory     wants System.Runtime.CompilerServices.Unsafe 4.0.4.1  the file is 4.0.6.0
```

.NET Framework binds a strong named assembly by EXACT version, so a file one build number
away is refused. Adding files fixes none of this.

Where else a copy could come from, checked:

- Navisworks ships none of these. Searched the whole install folder for each name
- the GAC holds only `System.Numerics.Vectors 4.0.0.0`, which is the framework facade and
  is not 4.1.3.0 either
- `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Numerics.Vectors.dll` is also
  4.0.0.0

#### Why there is no binding redirect

This is what NuGet writes into the application config, and the build does write it. The
probe project below produced `loadprobe.exe.config` holding exactly the four redirects:

```
System.Buffers                          0.0.0.0-4.0.3.0 -> 4.0.3.0
System.Memory                           0.0.0.0-4.0.1.2 -> 4.0.1.2
System.Numerics.Vectors                 0.0.0.0-4.1.4.0 -> 4.1.4.0
System.Runtime.CompilerServices.Unsafe  0.0.0.0-4.0.6.0 -> 4.0.6.0
```

A Navisworks add-in has no config of its own. It runs inside Roamer.exe and that config
belongs to Autodesk, so there is nowhere to put one. That is also why the tests never
caught this: the test process has a config with those same four redirects in it.

#### The fix, and it is proved

An `AssemblyResolve` handler on the current AppDomain, matching on the simple name alone
and loading from the bundle folder. An assembly handed back from `AssemblyResolve` is
accepted without a version check, so it survives a version mismatch and a missing file the
same way. It is registered in the static constructor of `FederatorPlugin`, which runs
before `Execute` and long before anything reaches the workbook writer.

REPRODUCED OUTSIDE NAVISWORKS on 2026-08-31, which is what makes this proved rather than
argued. A net48 console exe referencing ClosedXML and Federator.Core, run three ways:

```
1. with its own config           WORKBOOK WRITTEN, 7276 bytes
2. config file deleted           FAILED TypeInitializationException,
                                 SixLabors.Fonts.Tables.TableLoader
                                 inner FileLoadException: System.Numerics.Vectors
                                 4.1.3.0, the located assembly's manifest definition
                                 does not match the assembly reference
3. config deleted, resolver on   WORKBOOK WRITTEN, 7276 bytes
```

Run 2 is the reported failure, reproduced by nothing more than deleting the config file,
which is the one thing that makes a process behave like a Navisworks add-in. Run 3 shows
the handler answering for `System.Numerics.Vectors 4.1.3.0`,
`System.Runtime.CompilerServices.Unsafe 4.0.4.1` and `System.Buffers 4.0.2.0`, and reusing
the already loaded `System.Memory`.

To repeat it: make a net48 exe with `PackageReference ClosedXML 0.105.1` and a reference
to the built `Federator.Core.dll`, build it, delete `<exe>.config` from the output, and
run. That is the whole recipe.

Run 2 reports `FileLoadException, the manifest does not match` where Bader's log reported
`FileNotFoundException, could not load`. Both name the same assembly and the same version,
and both are answered by the handler. Which of the two a given process reports is UNKNOWN
and does not change the fix.

#### The installer checks this now

`install.ps1` walks what is actually in the bundle after copying, reads what each file
references, and refuses to finish if any reference is satisfied by neither the bundle, the
framework, nor the Navisworks folder. It also prints the six version mismatches above,
labelled as expected rather than as faults, so the next person does not read them as one.

### 4j. A stale test marker, and whether Compact is reachable, measured 2026-08-31

Both read off `Autodesk.Navisworks.Clash.dll` 22.0.0.0 in the install by reflection over
every type in the assembly, public members and private ones alike. The probe is
`build\probe-clash-api.ps1`, kept in the repo so these can be re-read against a
later install rather than trusted.

#### The stale warning: there is a status, and its meaning is UNKNOWN

Clash Detective shows a warning triangle on a test when something about it has changed
since it was last run. The question was whether the API exposes that.

Searching every type whose full name holds `Clash`, for members naming stale, alter,
outofdate, outdate, dirty, uptodate, invalid, needsrun, rerun or expire, across public,
non public, instance and static members:

    none

So there is no `IsStale`, no `IsAltered`, no `IsOutOfDate` and no `NeedsRerun` anywhere.
The only candidate is `ClashTest.Status`:

    Autodesk.Navisworks.Api.Clash.ClashTestStatus Status   get and set, both public

    ClashTestStatus, underlying type Int32
      New      = 0
      Old      = 1
      Partial  = 2
      Complete = 3

`Old` is the only value that could carry the warning. What is NOT established, and cannot
be established from the DLL, is what puts a test into `Old`. Whether it means an option
was changed, a newer model revision was loaded, both, or something else again, is
**UNKNOWN**. The names carry no documentation and reflection shows no rule.

So the tool reports the status as itself, by test name, and puts no meaning on it. It says
what Navisworks says and never translates `Old` into "your models have changed", because
that sentence would be invented here rather than read off anything. `ClashRunner.ReportStatus`
is the whole of it, and it logs. It never decides.

The settable side matters too. `Status` has a public setter, so a caller can write `Old`
onto a test or wipe it. Nothing in this tool writes it. Setting the status would be
claiming to know the rule that is UNKNOWN above.

#### Compact: reachable, and it destroys history

    public void TestsCompactTest(ClashTest test)
    public void TestsCompactAllTests()

Both public on `DocumentClashTests`, so Compact IS reachable and the tick box is real.
Underneath, in the interop layer, `LcClClashTestRunner.CompactOneTest` and
`.CompactAllTests`.

`TestsCompactTest` takes a `ClashTest`, so it is subject to section 4g: it is a mutator on
`DocumentClashTests` and the handle passed in is dead when it returns. `TestsCompactAllTests`
takes nothing, which is the safer of the two, and it is the one this tool calls.

What Compact removes is Resolved clashes. Once removed they are gone from the NWF, and the
NWF is the only record of what has been fixed, so this is not undoable and no second copy
exists anywhere. It is off by default, it is announced in the log before it runs with the
count it is about to remove, and the count it removed is reported afterwards. Nothing
compacts on its own.

Why it is offered at all: a test reruns weekly for months, and every clash ever fixed stays
in the file as Resolved. The count grows without limit and nothing else prunes it. So it is
a decision Bader makes on a run, with the number in front of him, rather than a thing the
tool does quietly or a thing he cannot do at all.

#### For the record, alongside 4h

    ClashResultStatus, underlying type Int32
      New      = 0
      Active   = 1
      Reviewed = 2
      Approved = 3
      Resolved = 4

Still five flat values and still no open against closed anywhere on it, which is what 4h
recorded. The open count setting is a stated rule on top of this enum, never a reading of
it, and the workbook says which of the two it used.


### 4k. Clash images, and the format the client has accepted, measured 2026-08-31

Two things this section settles. How a picture of a clash is actually made, read off the
installed DLLs. And what the client's own report holds, read off the report itself rather
than off anyone's description of it.

#### How an image is made

Every method in either `Autodesk.Navisworks.Api.dll` or `Autodesk.Navisworks.Clash.dll`
that returns a `System.Drawing.Bitmap`, found by walking every type in both assemblies,
public members and private ones alike. The probe is `build\probe-clash-images.ps1`.

```
public  Bitmap DocumentClashTests.TestsImageForResult(
            IClashResult result, ImageGenerationStyle style, int width, int height)

public  Bitmap Document.GenerateImage(ImageGenerationStyle style,
            int width, int height, bool enableSectioning)
public  Bitmap Document.GenerateImage(ImageGenerationStyle style,
            int width, int height, double maxTimeHint, bool enableSectioning)
internal Bitmap Document.GenerateImage(ImageGenerationStyle style, View view,
            int width, int height, double maxTimeHint, bool enableSectioning)

public  Bitmap View.GenerateImage(ImageGenerationStyle style,
            int width, int height, bool enableSectioning)
public  Bitmap View.GenerateImage(ImageGenerationStyle style,
            int width, int height, double maxTimeHint, bool enableSectioning)
```

That is the complete list. `TestsImageForResult` is the only one of them that takes a clash
result, so it is the one this tool uses.

```
Autodesk.Navisworks.Api.ImageGenerationStyle, underlying Int32
  Scene              = 0
  SceneUsingRayTrace = 1
  ScenePlusOverlay   = 2
```

WHICH of the three Navisworks uses for its own report is **UNKNOWN**. It cannot be read
off the DLL. `ScenePlusOverlay` is what this tool asks for, because an overlay is where a
clash highlight would live and the accepted report shows the two clashing items picked
out, and it is a property on `ClashImages` rather than a constant, so it can be changed
once a real run shows which one matches.

#### How a clash's own viewpoint is applied first

```
public Viewpoint DocumentClashTests.TestsViewpointForResult(IClashResult result)

public void Document.CurrentViewpoint.CopyFrom(Viewpoint viewpoint)
       Document.CurrentViewpoint is a DocumentCurrentViewpoint, get only, and holds
         Viewpoint Value { get }
         Viewpoint ToViewpoint()
         Viewpoint CreateCopy()
         void      CopyFrom(Viewpoint viewpoint)

public void View.CopyViewpointFrom(Viewpoint viewpoint, ViewChange change)
public Viewpoint View.CreateViewpointCopy()
       Document.ActiveView is a View, get only. View is IDisposable.
```

`Viewpoint` derives from `NativeHandle` and is `IDisposable`, so section 4g applies to it
the same as to everything else the document owns.

Whether `TestsImageForResult` applies the clash's viewpoint internally is **UNKNOWN**. It
cannot be read off the DLL. What IS established is that it takes a result, a style and two
numbers and no camera of any kind, so the view it renders can only have come from the
result. This tool therefore calls it on its own and does not set a viewpoint first. If a
real run shows the pictures are not framed on the clash, the explicit route is the four
calls above: read the viewpoint, copy it onto the current viewpoint or the active view,
then `View.GenerateImage`.

`ClashResult.HasSavedViewpoint` is a public get only bool, which says whether a clash
carries its own saved viewpoint. Nothing here writes a viewpoint into the NWF.

#### How an image is written to a file

Nothing in the Navisworks API writes a clash image to a file. Searched both assemblies for
every method naming image, render, snapshot, thumbnail, bitmap, capture or export. What
exists is:

```
public void ApplicationAutomation.GenerateThumbnail(int width, int height, string fileName)
public void ApplicationAutomation.GenerateThumbnailByRayTrace(int width, int height, string fileName)
```

Both write a file and neither takes a clash. They are the document thumbnail, not a clash
view, so neither is usable here.

So the bitmap is written with .NET rather than with Navisworks:

```
public void Image.Save(string filename, System.Drawing.Imaging.ImageFormat format)
public void Image.Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
       System.Drawing.Imaging.ImageFormat.Jpeg exists
       System.Drawing.Imaging.Encoder.Quality exists
```

`Image.Save(path, ImageFormat.Jpeg)` is what this tool uses. The encoder form is there if a
quality setting is ever wanted. The bitmap is a native image handle and is disposed on
every path, because one leaked per clash across 1830 tests is how a run runs a machine out
of memory.

#### What the accepted report actually holds

Read off the two files Bader supplied and their sibling folder:

```
C:\00_NM\Clash report\1104-PAR-1A02WO-XXX-BM-RPT-000001.html    3124927 bytes
C:\00_NM\Clash report\1104-PAR-1A02WO-XXX-BM-RPT-000001.xlsx     893415 bytes
C:\00_NM\Clash report\1104-PAR-1A02WO-XXX-BM-RPT-000001_files\   61 jpg
```

and cross checked against a second, much larger export in the same folder,
`1104-PAR-1A04EP-ZZZ-BM-RPT-000001` with 2672 pictures, and against the stylesheet
Navisworks wrote them with:

```
Navisworks Manage 2025\en-US\stylesheets\clash_report_html_tabular.xsl   28748 bytes
```

The stylesheet matters because it says which of the columns are fields and which are joins,
which the rendered HTML alone cannot.

The per test header, nine cells, from the `clashtest` template:

```
Tolerance | Clashes | New | Active | Reviewed | Approved | Resolved | Type | Status
0.025m    | 13      | 13  | 0      | 0        | 0        | 0        | Hard (Conservative) | OK
```

All 1830 tests get a block, including the 1809 that found nothing.

The per clash columns, from the `mainTableHeader` template:

```
Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
      | Item 1: Item ID | Item Name | Item Type
      | Item 2: Item ID | Item Name | Item Type
```

Four of these are joins rather than columns, and this is the part a description gets wrong:

- **Tolerance** is `@tolerance` and `/exchange/@units` written one after the other with
  nothing between them. Hence `0.025m` and not `0.025 m`
- **Grid Location** is one cell. `B-1 : ROF` is the grid intersection, a spaced colon, then
  the level. Not two columns
- **Clash Point** is one cell carrying three literal prefixes and two commas,
  `x:31.643, y:-2.913, z:3.325`. The stylesheet gives that cell `colspan="3"`, which is why
  it can look like three columns and is not
- **Item ID** is `objectattribute/name` in italics, a colon, then `objectattribute/value`.
  So `Element ID` is read out of the model and is not a constant. A model from something
  other than Revit carries a different label

`Item Name` and `Item Type` are NOT fixed columns in the stylesheet. They come from
`$showQuickProperties`, which writes one column per `smarttag` and takes the heading from
`smarttag/name` in the data. In the accepted report the two quick properties configured
were Item Name and Item Type, so those are the words used here, and Item Type reads `Solid`
on all 120 item cells.

Numbers, measured across all 60 clash rows: every Distance and every coordinate is written
to three decimals, trailing zeros kept, `z:-0.050` among them.

#### The pictures

The folder is the report name with `_files` appended, beside the report. It holds loose jpg
plus a `logo.jpg` that Navisworks puts there.

The naming is **not** one running sequence, which is what the first dozen names suggest. It
is `cd`, the test, then the clash within that test:

```
cd000001.jpg   test 0,   clash 1        first thirteen are test 0
cd000013.jpg   test 0,   clash 13
cd010001.jpg   test 1,   clash 1
cd200001.jpg   test 20,  clash 1
cd1000001.jpg  test 100, clash 1        seven digits, from the 2672 picture export
cd001244.jpg   test 0,   clash 1244     the largest test in that export
```

So the test is formatted `00`, a floor of two digits with no ceiling, and the clash `0000`,
a floor of four. Test 100 gives three digits and the whole name grows to seven, which a
fixed six wide field would have got wrong.

On the 1830 block export the picture's test number equals the index of the test block, with
no gaps, because the blocks are sorted by clash count descending so every block that has
pictures is a contiguous run from zero. That makes "index of the block" and "index among
tests that have pictures" indistinguishable in both samples. This tool uses the second,
because it is the one that cannot leave a gap.

Sizes, measured: all 60 pictures are 1024 by 1024. Mean 187 KB, largest 318 KB, smallest
25 KB, 10.98 MB for the 60. That is where the 1024 default comes from.

#### The xlsx is Excel's own save of the HTML

Worth knowing, because it explains two things that look like faults:

- declared dimension `A1:BA14701`, which is 53 columns, but only 17 columns hold a value.
  The other 36 come from the colspans in the HTML being expanded on import
- it holds no embedded pictures at all, no `xl/media` part. It has 61 pictures and 61
  hyperlinks that are all EXTERNAL and ABSOLUTE, pointing at
  `file:///C:\00_NM\Clash report\..._files\cd000001.jpg`. So the xlsx on its own shows
  broken picture boxes on any machine but the one that made it

This tool writes a real workbook rather than a saved HTML page, and its links are relative,
so the workbook and its `_files` folder can be moved together and keep working.


### 4l. Our report against theirs, cell by cell, measured 2026-09-01

Bader committed a real run's output beside two real client exports, so for the first time
the two could be compared rather than described:

```
samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-00001.xlsx      54,240,144 bytes, 50 sheets
samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-00001_files\    213 jpg, 54 MB
samples\client-report\1104-PAR-1A02WE-XXX-BM-RPT-000001.xlsx     893,624 bytes, 1 sheet
samples\client-report\1104-PAR-1A02WO-XXX-BM-RPT-000001.xlsx     893,415 bytes, 1 sheet
docs\logs\run-20260901-093708.log                                523,133 bytes
```

Their first test block, read out of the file:

```
row 4   BLD-ST-Walls-vs-BLD-AR-Floors | Tolerance | Clashes | New | Active | Reviewed | Approved | Resolved | Type | Status
row 5                                 | 0.025m    | 13      | 13  | 0      | 0        | 0        | 0        | Hard (Conservative) | OK
row 7                                                                             Item 1 (L)         Item 2 (O)
row 8   Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point | Item ID | Item Name | Item Type | Item ID | Item Name | Item Type
row 9         | Clash1     | New    | -0.05    | B-1 : LGF     | Hard (Conservative) | x:-2.100, y:-2.726, z:-0.051 | Element ID: 2635048 | Concrete, Cast In Situ Fc' 35MPa | Solid | Element ID: 814542 | TRENCH | Solid
```

Ours, the same rows out of ours:

```
row 1   BLD-AR-Walls-vs-BLD-AR-Columns | Tolerance | Clashes | ... | Type | Status
row 2                                  | 0.2461ft  | 8       | ... | hard_conservative | (empty)
row 4   BLD-AR-Walls  against  BLD-AR-Columns   items 16 v 8   8 rows covering 8 raw clashes...
row 5   8 rows carry a picture, in the folder beside this workbook...
row 6   Back to Summary
row 9   Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point | Item ID | ...
row 10  cd000001.jpg | Clash1 | New | -0.328083992004395 | D-8 : LGF : LGF | Hard (Conservative) | x:33.171, y:-10.410, z:0.328 | Instance GUID: 00000000-0000-0000-0000-000000000000 | ...
```

Ten differences, and what each one turned out to be:

| Field         | Theirs                | Ours                                          | What it was |
|---------------|-----------------------|-----------------------------------------------|-------------|
| Item ID       | Element ID: 2635048   | Instance GUID: 00000000-0000-...-000000000000 | two faults, below |
| Layer         | NOT PRESENT           | not present                                   | nothing to fix |
| Type          | Hard (Conservative)   | hard_conservative                             | the file's token reached the cell |
| Status        | OK                    | empty                                         | never set |
| Grid Location | B-1 : LGF             | D-8 : LGF : LGF                               | the level said twice |
| Distance      | -0.05                 | -0.328083992004395                            | no number format |
| Tolerance     | 0.025m                | 0.2461ft                                      | NOT a fault, see below |
| Extra rows    | none                  | three plus Back to Summary                    | ours, on their sheet |
| Filters       | none                  | on every header                               | ours, on their sheet |
| Logo          | present               | absent                                        | left absent on purpose |

#### Layer: the brief said theirs has one. It does not

Searched both supplied exports. Neither the xlsx nor the HTML carries a Layer column. Their
header row is thirteen cells and Layer is not among them:

```
Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
      | Item ID | Item Name | Item Type | Item ID | Item Name | Item Type
```

The stylesheet does have one, behind `$showLayer`, and that flag was off for these exports.
So there is nothing to add. Section 4k already recorded that Item Name and Item Type are
not fixed columns either, they are the quick properties, which is the same kind of thing.

#### Item ID: two separate faults on top of each other

The label read `Instance GUID` and the value was all zeros, in all 426 item cells.

First, the id was never found. `ClashResult.Item1` is the geometry the clash was found on.
On a Revit sourced NWC that is a leaf whose display name is a material, `PAR-CONC-FOUNDATION`
in ours and `Concrete, Cast In Situ Fc' 35MPa` in theirs, and it carries no Revit properties
at all. The element it belongs to is `ClashResult.CompositeItem1`, and that is where the id,
the family and the type live. The search now runs over the item, then its composite item,
then up to eight ancestors, first hit winning.

Second, the fallback was worse than nothing. With no id found it wrote `item.InstanceGuid`,
which came back as `Guid.Empty` on every item in this model. A row of zeros behind the
words `Instance GUID:` reads like an identifier and identifies nothing. An all zero GUID is
now treated as no id at all and the cell is left empty, which says the same thing honestly.

#### Tolerance and Distance: the units are not a fault

Their document measures in metres and this one measures in feet. The log says so:

```
09:42:07.639  CLASH    the document measures in ft, every tolerance was converted into it
              CLASH    created  ...  tolerance 0.2460629921 ft is 0.2460629921 ft
```

0.2460629921 ft is 75 mm and 0.025 m is 25 mm, so the two projects also use different
tolerances. Both reports are correct for their own model, and the conversion did run, it
was just a no-op from feet into feet.

What WAS wrong is the formatting. The Distance cell carried no number format, so Excel
printed the whole double. It now carries `0.000`, which is what every distance and every
coordinate in both client exports is written to, and it stays a number so the column sorts.

#### The 54 MB workbook was asked for

The workbook holds 213 embedded pictures, 53,855,953 bytes of the 54,240,144 on disk.
Without them it would be about 0.31 MB, so pasting them made it roughly 170 times larger,
and the same 213 pictures are already in the folder beside it either way.

The default is off, and was off. The log records that this run asked for it:

```
09:42:07.633  CLASH    Images on, 1024 by 1024 pixels, New, Active, Reviewed, Resolved
                       only, no cap per test, with a thumbnail in the cell.
```

So nothing needed switching off. What was missing is that the tick box did not say what it
would cost, and now it does, with those measured numbers in it.

#### What the log settled about the NWF

```
09:42:06.359  NWF   written  ...1104-PAR-1C07BC-ZZZ-BM-MOD-00001.nwf    4,141 bytes
09:44:10.673  NWF   written  ...1104-PAR-1C07BC-ZZZ-BM-MOD-00001.nwf  165,844 bytes
09:44:16.657  NWD   written  ...                                    5,936,323 bytes
09:44:16.945  RESULT files written : 4, every size read back off the disk
              NWF   ...1104-PAR-1C07BC-ZZZ-BM-MOD-00001.nwf           4,141 bytes
```

The NWF did not shrink and the NWD is not implicated. `RunLog.WriteFinished` recorded a
path once and kept the FIRST size, so the RESULT block printed the 4,141 read at 09:42:06,
four minutes before the NWD was published at 09:44:16. The second save read 165,844 off the
disk and printed it correctly on its own line.

The de-duplication was there so that a catch which re-checks the outputs could not turn
"files written" into a count of checks. That catch uses `CheckOnDisk`, which records
nothing, so the only repeat caller of `WriteFinished` on one path is the NWF's two saves,
and keeping the first of those two is always wrong. The entry now takes the latest size and
there is still one entry per file.

Separately, nothing was looking at the NWF after the NWD was published, so a run that DID
destroy the clash history would have finished quietly. `RunLog.ConfirmStillWhole` now reads
it once more at the end of the group and logs intact with the size, or CHANGED with both
sizes, or GONE.

#### The naming boxes opened empty, which is where MOD-00001 came from

Measured by constructing the real window and reading the boxes, `build\probe-window-defaults.ps1`:

```
before   NwfNumber ''        0 characters      all fifteen boxes empty
after    NwfNumber '000001'  6 characters      all fifteen carry their default
```

`NamePattern.DefaultNumber` was always `000001` and is verified six characters by test. It
never reached the window. `FillGroupingModes()` sets `SelectedIndex`, which fires
`OnGroupingModeChanged`, which calls `Regroup()`, which calls `ReadNaming()`. With the boxes
still empty that read every default out of the patterns and replaced it with nothing, and
`ShowNaming()` then wrote the emptied patterns back into the boxes. Every one of the five
supplied fields had to be typed by hand, and a hand typed number is where a digit goes
missing. `ShowNaming()` now runs before the combos are filled.


### 4m. The client format is HTML Tabular, and our XML against its stylesheet, measured 2026-09-01

#### What the client's file actually is

Clash Detective cannot export an xlsx. Bader exports HTML (Tabular) and opens the page in
Excel, and that is the file the client accepted. It explains everything about the samples
that looked odd from the outside: a declared dimension of 53 columns with 17 populated, the
merged cells, and the absolute `file:///` picture links Excel writes when it saves an HTML
page.

So the layout is not ours to invent. It is defined by

```
Navisworks Manage 2025\en-US\stylesheets\clash_report_html_tabular.xsl   28748 bytes
```

which is Autodesk's file. No copy of it is in this repo. It is read from the install at run
time by `StylesheetLocator`, which builds each candidate path directly and tests it with
`File.Exists`, never searching or wildcarding the install folder. The candidates are the
language the application reports and then `en-US`. When neither is there the log names both
paths and writes no page, and the workbook and the XML are untouched.

#### The Layer column: the previous session was right about two files and wrong in general

A screenshot of `1104-PAR-1A04WE-XXX-BM-RPT-000001.xlsx` shows a Layer column on both items.
That file was never committed to `samples\client-report`, which holds only 1A02WE and
1A02WO. It does exist on Bader's machine and was read from there on 2026-09-01:

```
1A04WE row 8   Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
               Item ID | Layer | Item Name | Item Type
               Item ID | Layer | Item Name | Item Type
1A02WE row 8   Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
               Item ID | Item Name | Item Type
               Item ID | Item Name | Item Type
```

Both are true. The stylesheet writes a Layer column for
`boolean(//clashobjects/clashobject/layer)` and nothing else, so it appears when the export
carried layer data and not when it did not. On 1A04WE the column is present with the header
and the cells are empty.

This is the whole lesson of this section. Not one of these columns is fixed. Every one of
them is a boolean over an XPath, so what the page holds is decided entirely by what the XML
holds.

#### Every column test the stylesheet makes, and what our XML answers

Read off the variable block at the top of the stylesheet, and checked against our own XML
by `HtmlTabularTests.OurXmlAnswersEveryColumnTestTheStylesheetMakes`.

| Variable | XPath it tests | Ours before | Ours now |
|---|---|---|---|
| showSummary | `/exchange/batchtest/clashtests/clashtest/summary` | yes | yes |
| showClashPoint | `//clashpoint` | yes | yes |
| showDistance | `//@distance` | yes | yes |
| showStatus | `//resultstatus` | yes | yes |
| showGridLocation | `//gridlocation` | yes | yes |
| showLayer | `//clashobjects/clashobject/layer` | yes | yes |
| showItemID | `//clashobjects/clashobject/objectattribute` | yes | yes |
| showDateFound | `//clashresult/createddate` | yes | ours only |
| **showDescription** | `//description` | **NO** | yes |
| **showQuickProperties** | `//clashobjects/clashobject/smarttags` | **NO** | yes |
| **showImage** | `//@href` | **NO** | yes |
| showDateApproved | `//approveddate` | no | no |
| showApprovedBy | `//approvedby` | no | no |
| showAssignedTo | `//assignedto` | no | no |
| showClashGroup | `//parentgroup` | no | no |
| showComments | `//comments/comment` | no | no |
| showItemPath | `//clashobjects/clashobject/pathlink` | no | no |

So it very nearly matched. Three whole columns of the client's report could not be produced,
and one more was the wrong shape:

- **description** was missing, so no Description column
- **smarttags** was missing, so no Item Name and no Item Type. These are not fixed columns
  at all. The stylesheet writes one column per smarttag and takes the heading from
  `smarttag/name` in the data, so the client's Item Name and Item Type are the two quick
  properties their export was configured with
- **@href** was missing, so no Image column
- **objectattribute** was the wrong shape. The Item ID cell is

  ```xml
  <i><xsl:value-of select="./objectattribute/name"/></i>:
  <xsl:value-of select="./objectattribute/value"/>
  ```

  `xsl:value-of` over a node set takes the FIRST node. We wrote four per clashobject, Name,
  Family, Type, Material, Source File, Discipline and Element Id, so the id column would
  have rendered `Name: PAR-CONC-FOUNDATION`. Theirs carries exactly one, the id

The decision was to reshape our XML rather than write the HTML ourselves. Four small changes
against reimplementing a 700 line layout, and the page is then rendered by the same file
Navisworks renders theirs with, so it cannot drift from it.

One thing the stylesheet forces. `$colQuickProperties` is
`count((//clashobject/smarttags)[1]/smarttag)`, counted once off the FIRST clashobject and
used for every row. So every clashobject must carry the same list of smarttags, empty values
included, or every column after them slides sideways.

#### Where ours differs from theirs on purpose

Two elements are written only when the client layout has not been asked for, because the
client's own report has neither column:

- `createddate`, which gives a Date Found column
- our five extra quick properties, Family, Type Name, Material, Source File and Discipline

With **Client report layout only** ticked, the page is their fifteen columns and nothing
else. Without it, ours are appended after theirs and none of theirs move.

#### The logo

```xml
<td class="logoCell" colspan="3">
  <img alt="logo"><xsl:attribute name="src"><xsl:value-of select="//logo/@href"/></xsl:attribute></img>
</td>
```

The logo is DATA, read from `//logo/@href`, not something baked into the layout. Autodesk's
own `logo.jpg` sits in the Images folder of the install and is theirs, and nothing here
copies it or reaches for it.

The page carries the logo Navisworks puts on its own reports, because that is the report
the client accepts. Measured on 2026-09-01:

```
C:\Program Files\Autodesk\Navisworks Manage 2025\Images\logo.jpg   6137 bytes
MD5 b5df301defc444485c0461748c56e0d9
```

`Images` sits at the top of the install with no language folder, checked. That file is byte
for byte the same as the `logo.jpg` in the `_files` folder of BOTH reports Bader supplied,
same MD5, which is what proves Navisworks copies that exact file into the report folder.

So this tool does the same. It reads it off the install at run time and copies it into the
report's own `_files` folder beside the clash pictures, then links it relatively as
`<stem>_files/logo.jpg`. The report goes to a client and the page must not point at a path
on the machine that wrote it. The accepted xlsx carries absolute `file:///` links, which is
exactly why its pictures break on any other machine, and a test opens our written page and
reads the `src` back out of the file rather than off the object model.

No copy of the logo is in this repo, in the bundle, or in `install.ps1`. The only two in the
checkout are inside the client exports Bader committed, where Navisworks had already put
them.

The Browse box stays for a project that needs a different mark, and it starts filled with
the install's own. Clearing it means no logo, and a logo that cannot be found writes the
page without one and names every path it tried.

One consequence worth knowing. `$showImage` is `boolean(//@href)` and the logo carries an
`href` too, so a page with a logo shows the Image column even when there are no clash
pictures. That is Autodesk's own behaviour and their reports do the same.

#### The transform

`System.Xml.Xsl.XslCompiledTransform`, which is in the framework, so nothing new ships in
the bundle. The stylesheet declares `version="1.0"` and uses no `document()`, no
`msxsl:script`, no `xsl:import` and no `xsl:include`, checked on 2026-09-01, so it runs
under `XsltSettings.Default` with both scripts and document loading switched off.

#### The trailing space in every image link

The samples' image hrefs all end in a space, which looked like a fault in ours. It is the
stylesheet:

```xml
<xsl:attribute name="href">
  <xsl:value-of select="@href"/>
  <xsl:text> </xsl:text>
</xsl:attribute>
```

It confirms the samples were produced by exactly this file, and it is nothing for us to
reproduce or correct.


### 4n. Reading a property value of any kind, and four column faults, measured 2026-09-01

A real run compared against two real Navisworks reports. The files:

```
samples\client-report\1104-PAR-1A02WN-XXX-BM-RPT-000001.html   3265122 bytes
samples\client-report\1104-PAR-1A04WN-XXX-BM-RPT-000001.html   3266035 bytes
samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-000001.html      3904315 bytes
samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-000001.xml       1112490 bytes
docs\logs\run-20260901-191711.log                               198097 bytes
```

The header rows, read out of each:

```
theirs   general  Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
         item 1   Item ID | Layer | Item Name | Item Type
         item 2   Item ID | Layer | Item Name | Item Type

ours     general  Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
         item 1   Layer | Item Name | Item Type
         item 2   Layer | Item Name | Item Type
```

Both theirs are identical to each other. Ours is missing Item ID on both items and nothing
else, so seven general columns against seven and three item columns against four.

#### Which member returns a value regardless of kind

The log carries the cause, twice written out and the rest counted:

```
type     : System.NotSupportedException
message  : Not supported if '!IsDisplayString'
stack    : at Autodesk.Navisworks.Api.VariantData.ToDisplayString()
           at Federator.Addin.Engine.ClashHarvest.FirstProperty(ModelItem, String[], String&)
           at Federator.Addin.Engine.ClashHarvest.Describe(Document, ModelItem, ModelItem, ClashItem)
```

Every accessor on `VariantData` is kind specific and throws on any other kind. The complete
list, read off the installed DLL:

```
ToAnyDouble  ToBoolean  ToDateTime  ToDisplayString  ToDouble  ToDoubleAngle
ToDoubleArea ToDoubleLength  ToDoubleVolume  ToIdentifierString  ToInt32
ToNamedConstant  ToPoint2D  ToPoint3D  ToString
```

and the kinds:

```
VariantDataType   None=0 Double=1 Int32=2 Boolean=3 DisplayString=4 DateTime=5
                  DoubleLength=6 DoubleAngle=7 NamedConstant=8 IdentifierString=9
                  DoubleArea=10 DoubleVolume=11 Point3D=12 Point2D=13
```

**The one that returns a value regardless of kind is `VariantData.ToString()`.** It is an
override declared on `VariantData` itself, and its IL, decoded off the installed DLL,
proves what it does. It calls `NativeHandle.get_IsDisposed`, then `GetDataType`, then
switches to the matching accessor:

```
call VariantData.GetDataType
call VariantData.ToDouble        ... Double.ToString      ... String.Concat
call VariantData.ToInt32         ... Int32.ToString       ... String.Concat
call VariantData.ToBoolean       ... Boolean.ToString     ... String.Concat
call VariantData.ToDisplayString ...                      ... String.Concat
call VariantData.ToDateTime      ... DateTime.ToString    ... String.Concat
call VariantData.ToDoubleLength  ... Double.ToString      ... String.Concat
call VariantData.ToDoubleAngle   ... Double.ToString      ... String.Concat
```

So it never throws. There is a catch. Its string literals are

```
"Disposed" "None" "Double:" "Int32:" "Boolean:" "DisplayString:" "DateTime:"
"DoubleLength:" "DoubleAngle:" "<null>" "NamedConstant:" "IdentifierString:"
"Point3D:" "Point2D:" "Unknown"
```

so it prefixes the kind and hands back `Int32:702888`, not `702888`. It is a diagnostic
form rather than a value.

The tool therefore reads the KIND and calls the right accessor, which is what `ToString`
does internally minus the prefix, and keeps `ToString` as the fallback for a kind it does
not know, stripping the prefix with `Federator.Core.Report.VariantText.Clean`. That part is
in Core so it can be tested, because the rest needs Navisworks.

`ToAnyDouble` covers Double, DoubleLength, DoubleAngle, DoubleArea and DoubleVolume in one
case, which is what its name says and what saves five cases that would each throw on the
other four.

None of this could be tried outside Navisworks. `VariantData.FromInt32` throws
`AccessViolationException` in a bare process, because the native side is not initialised,
so the IL is the evidence rather than a run.

#### Why the source file and the discipline were empty as well

`Describe` read the family, then the type, then the material, then the item type, then the
element id, and only then the source file and the discipline. The element id is an Int32,
so `ToDisplayString` threw on it, and the single try around the whole method meant
everything after that point was never reached. One throw cost three columns, not one.

The source file and the discipline now sit in their own try, so a property that will not
read cannot take them with it.

#### Our columns were leaking into the client's page, and one measurement says otherwise

The stylesheet writes one column per smarttag, so anything of ours in the XML becomes a
column on the page the client receives. Our XML carried seven:

```
Item Name | Item Type | Family | Type Name | Material | Source File | Discipline
                                                        (empty)      (empty)
```

2982 smarttags over 426 clashobjects, exactly seven each.

The rendered page from that same run, however, carries only three item columns. Our extras
appear zero times in it. That is because the page was built with the client layout tick box
on while the standalone XML file ignores that setting and always wrote seven. So the leak
was real in the XML and had not yet reached that particular page.

Either way it is now impossible. The page IS the client's report, so the XML that feeds it
always carries exactly the two quick properties theirs has, whatever any tick box says.
Family, Type Name, Material, Source File and Discipline are workbook columns.

`createddate` goes with them. Neither supplied report has a Date Found column, and the
stylesheet writes one for any `createddate` it finds.

#### Tolerance and separator

```
theirs   0.025m                        ours   0.2460629921ft
theirs   ..._files\cd000001.jpg        ours   ..._files/cd000001.jpg
theirs   ..._files\logo.jpg            ours   ..._files/logo.jpg
```

The units differ because the two documents do, which is correct and the log says so. The
precision was ours: the stylesheet writes the `tolerance` attribute straight into the cell
followed by the units, and we were writing the file's own full precision. Three decimals
now, which is how both of theirs are written.

The separator is a backslash in both of theirs, for the clash pictures and for the logo,
and the client opens these in Excel on Windows. The workbook keeps a forward slash, because
a hyperlink there is a Uri and there is nothing of theirs to match: their own xlsx carries
no picture hyperlinks at all, only linked drawings.

#### Neither report embeds its pictures

Measured on the zip of each:

```
theirs 1A04WN   embedded pictures 0
theirs 1A02WN   embedded pictures 0
ours   1C07BC   embedded pictures 0
```

The native export never embeds. Pictures show only when the `_files` folder sits beside the
file, which is why the page and its folder are what gets sent, and why the tick box that
pastes them into cells says so.


### 4o. The reports check themselves, and where the client column set comes from, 2026-09-01

Bader has been opening every client report in Excel after a run and searching it by hand.
This tool wrote the file, so it can look and say.

#### Both checks read the FILE

`PageCheck` reads the written HTML back off the disk. `WorkbookCheck` opens the written
xlsx. Neither looks at the object that produced it, and that distinction has already caught
two faults that the object model reported as fine:

- a relative image link that ClosedXML stored as an INTERNAL address, so the cell jumped to
  a sheet instead of opening the picture, while the object model said the link was there
- a test that was silently skipping rather than running, because it counted `..\` steps to
  reach the samples folder

Neither check can fail a group. A page with a column too many is worth saying loudly and is
not a reason to mark a group failed, because the NWF, the NWD and the workbook were all
written. Judging a group on it would repeat the fault that once reported a clean 22 group
run as FAILED. They go into `JobOutcome.ReportWarnings`, which is a separate list from
`Errors` for exactly that reason.

#### What the page check reports

```
REPORT CHECK 1C07BC
CHECK    213 clash rows on the page.
         Item ID filled on 213 of 213 for item 1 and 213 of 213 for item 2.
         the first one reads Element ID: 702888
         no column the client's report does not have.
         the tolerance cell reads 0.246ft
         213 picture references, 213 with a backslash, 213 on disk.
         the logo is 1104-...-RPT-000001_files\logo.jpg and it is on disk.
         Nothing wrong with it.
```

Every number is counted off the file. A picture reference is resolved against the page's own
folder, so "on disk" means the `_files` folder really holds it, which is the thing that
breaks when a report is sent on. An id cell counts as filled when it holds a label, a colon
and something after it, because an empty one is exactly what a run wrote into all 426 of
them.

#### What the workbook check reports

```
WORKBOOK CHECK 1C07BC
CHECK    213 clash rows across 48 test sheets.
         Item 1 Family       213 of 213
         Item 1 Type Name    213 of 213
         Item 1 Source File    0 of 213   EMPTY ON EVERY ROW
         ...
         These columns are empty on every row, Item 1 Source File, Item 1 Discipline,
         Item 2 Source File, Item 2 Discipline. Either the model does not carry them or
         they are not being read.
```

This is the check that would have caught Source File and Discipline coming out empty
without anyone opening the workbook. Each column is found by its heading on each sheet
rather than by position, because the client only mode leaves ours out entirely.

#### One line in the window

The run view carries one line per group, in the words a person would use:

```
1C07BC. Client report: Item ID filled on 213 of 213 rows, no extra columns, all 213
        pictures on disk.
1C07BC. Workbook: 213 rows, every column of ours filled somewhere.
```

Anything failing shows the first problem there instead, so a bad report is visible without
opening the log.

#### Where the client column set came from

Not a list typed into the code. Two sources.

The stylesheet, `clash_report_html_tabular.xsl`, gives the complete vocabulary of columns it
can write as literals:

```
Approved By, Assigned To, Clash Group, Clash Name, Clash Point, Comments, Date Approved,
Date Found, Description, Distance, Grid Location, Image, Item ID, Layer, Path, Status,
and the Task block of Name, Start and End
```

The two exports in `samples\client-report` say which of that the client actually receives.
Both carry exactly the same header, checked:

```
general   Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
per item  Item ID | Layer | Item Name | Item Type
```

`Item Name` and `Item Type` are NOT in the stylesheet's literal list. They are quick
properties, written one column per smarttag with the heading taken from the data, which is
why they can be anything at all and why our own extras became columns when they were
written there.

`ClientReportColumnsTests` re-reads both sample exports and the stylesheet on every test run
and fails if the set no longer matches, so the constant is checked against the files rather
than trusted.

#### A dead template in Autodesk's own stylesheet

Worth recording, because it will mislead the next person who greps that file. There is a
template named `ItemHeaderCells` that NOTHING calls:

```xml
<xsl:template name="ItemHeaderCells">
  <xsl:param name="ItemNumber"/>
  <xsl:variable name="class">item<xsl:value-of select="$ItemNumber"/>Header</xsl:variable>
  <xsl:for-each select="(//clashresults/clashresults/clashobjects/clashobject)[1]/smarttags/smarttag">
    <td class="{$class}"><xsl:value-of select="./Name"/></td>
    <td class="{$class}">Item Type</td>
  </xsl:for-each>
  <td class="{$class}">Item ID</td>
  <td class="{$class}">Layer</td>
</xsl:template>
```

It writes `Item Type` as a literal, reads `./Name` with a capital where the rest of the file
reads `name`, and puts Item ID and Layer AFTER the quick properties rather than before. None
of that is what the reports actually carry. The live one is `mainTableHeader`, and it is the
only one the column tests read.


### 4p. Matching the original workbook, measured 2026-09-01

Bader changed his mind on the workbook and the reason is good. He answered the original
question before anyone had seen a real Navisworks report. With both side by side the ask
became one thing: our output matching theirs.

Files read:

```
samples\client-report\1104-PAR-1A02WN-XXX-BM-RPT-000001.html and .xlsx
samples\client-report\1104-PAR-1A04WN-XXX-BM-RPT-000001.html and .xlsx
samples\our-report\1104-PAR-1C07BC-ZZZ-BM-RPT-000001.html, .xlsx and .xml
```

The brief names ours as `-BM-MOD-000001`. The committed file is `-BM-RPT-000001`. Same
file, different middle field.

#### The sort rule

Both exports are strictly descending by clash count over all 1830 blocks. 1A02WN opens
12, 6, 6, 6 and 1A04WN opens 9, 7, 6, 6, and both end on 0.

The SECOND KEY was measured rather than assumed, because 1830 tests share few distinct
counts: 1A02WN has six distinct counts and 1A04WN eight, so the tie groups are enormous.
Each tie group was compared against the order the tests sit in
`samples\1104-PAR_CLASH_AllInOne (2) (1).xml`, which both reports were run from:

```
1A02WN   count 6      3 tests   original order: yes   alphabetical: no
         count 4      3 tests   original order: yes   alphabetical: no
         count 2      6 tests   original order: yes   alphabetical: no
         count 1     10 tests   original order: yes   alphabetical: no
         count 0   1807 tests   original order: yes   alphabetical: no
1A04WN   count 4      3 tests   original order: yes   alphabetical: no
         count 2      6 tests   original order: yes   alphabetical: no
         count 1     10 tests   original order: yes   alphabetical: no
         count 0   1806 tests   original order: yes   alphabetical: no
```

So the tie rule is the order the tests were created in, not the name. A group of 1807
holding that order by chance is not a possibility.

That makes it a STABLE sort by count descending. `List.Sort` is not stable and would
scramble the 1807, so `ClashReport.InReportOrder` carries the original index and sorts on
the pair.

**Is that order the document's or the report writer's? UNKNOWN.** Nothing in the files can
tell the two apart: a report writer that sorts, and a report writer that walks a collection
Clash Detective had already sorted, produce the same page. The OUTPUT is sorted here for a
second reason as well. Sorting the document would mean calling
`DocumentClashTests.TestsSortTests`, which is a mutator that reorders the tests inside the
NWF, and the NWF is the only record of what has been fixed.

#### The number format

Every distance and coordinate in both exports, 387 coordinates and 129 distances:

```
decimals seen, 1A02WN   3 on all 192 coordinates
decimals seen, 1A04WN   3 on 186, and 6, 10 or 16 on nine of them
```

Trailing zeros are kept, so 8.310 and -0.440 and 17.150 appear as they are. That rules out
significant figures as the general rule.

The nine outliers in 1A04WN are all values that would round to zero at three decimals:

```
0.0000000000000373   0.0000000000000243   0.0000000000000365
0.0000000000000486   0.0000000684         -0.000432
```

each of which is three significant figures, in plain decimal and never an exponent. And
`0.000` and `-0.000` appear nowhere in either file. So the rule is three decimals, with a
non-zero value that would read as zero written to three significant figures instead.

**Whether theirs rounds or truncates is UNKNOWN.** Both files carry only the formatted text
and nothing in either carries the value behind it, so there is nothing to compare. Ours
rounds, which is what three decimals ordinarily means.

#### The id label

Ours read `Id: 990299` and theirs reads `Element ID: 702888`.

WE set that label. `ClashHarvest.ElementIdNames` is
`{ "Id", "Element Id", "ElementId", "Element ID" }` and the label was the display name of
whichever matched. Id is first, so a Revit item whose property is displayed as Id gave the
label Id. Navisworks is not naming it for us.

So it is ours to match, and it is `Element ID`. The display name of the property that
actually supplied the value is kept on `ClashItem.IdFrom` and goes in the log, because
renaming a value is only honest while what was renamed is still visible.

#### What the workbook lost

Their xlsx is one sheet holding every test one after another. Ours had 50: a Summary, a
Matrix and one per test that found clashes. All three are gone.

The block layout, measured merge by merge off 1A04WN:

```
row 1        A1:C1 empty for the logo, D1:BA1 "Clash Report"
row start    A:B merged over two rows, the test name. C to K the nine headers
row start+1  C to K the nine values
row start+2  blank
row start+3  A:K empty, L:O "Item 1", P:S "Item 2"
row start+4  A:B "Image", C:D "Clash Name", E to H, I:K "Clash Point",
             L M N O and P Q R S the two item blocks
row start+5  one row per clash, with A:B, C:D and I:K merged
             then three blank rows before the next block
```

Column widths were taken off their sheet too, so a side by side comparison lines up.

#### The self check passed all of this

It reported nothing wrong while the order, the id label and both number formats differed
from the samples, because it counted PRESENCE. A column being there says nothing about
where it is, what shape its values are, or what order the blocks sit in.

It now compares all three and reports the first divergence with an example from each file:

```
Column 8 of the clash table is wrong. Ours reads "Layer" and the client's report reads
"Item ID". The whole order should be Image, Clash Name, ...

The Item ID cell is the wrong shape. Ours reads "Id: 990299" and the client's report
reads "Element ID: 707077".

The tests are in the wrong order. Block 1 holds 1 clashes and block 2 holds 9. The
client's report puts the most clashes first.
```

A measurement is checked against the rule above rather than a loose pattern, which is what
lets it catch `x:33.170986` while every column is present.

#### One difference this did not close, closed by F17 on 2026-09-07

The clash pictures WERE numbered by the order the tests RAN, because they are rendered
during the clash walk, and the report is sorted afterwards. Theirs numbers by block. So
`cd000001.jpg` was not necessarily the first block's first clash in ours. Every row linked to
its own file explicitly, so nothing was mislabelled, but the two numbering schemes were not
the same.

Since F17 the pictures are still rendered during the walk, under the run order number, and
renamed ONCE after the run by `Federator.Core.Report.ImageRenumbering`, in one pass, into
the order `ReportOrder` gives: tests most clashes first with ties in creation order, and
inside a test the clashes as Clash Detective lists them. The row, the workbook link, the
XML href and the page all follow the renamed file. The rename goes through a holding name
first so a swap between two tests cannot write one picture over another. The rule is
proved in `ReportOrderTests`, the add-in side is proved locally only.


### 4q. The workbook cell by cell, and the units, measured 2026-09-01

Every number below was read off the two files with the same reader, ours written by
`WorkbookWriter` and theirs `samples\client-report\1104-PAR-1A04WN-XXX-BM-RPT-000001.xlsx`.
Nothing here is estimated.

#### Can the document be put into metres

The document was in feet and the client works in metres. Whether the API can set units was
established by loading `Autodesk.Navisworks.Api.dll` with `LoadFrom` and walking all 4027
types. `ReflectionOnlyLoadFrom` was tried first and `GetTypes()` returned 0, so that pass
proved nothing and was redone.

    Document.Units                      read only, no setter at all
    Model.Units                         read only, no setter at all
    any settable Units property         none, across all 4027 types
    any Set/Convert/Change units method none, except the three below

    public void DocumentModels.SetModelUnitsAndTransform(
        Model model, Units units, Transform3D transform, bool transformReflected)

    Autodesk.Navisworks.Api.Interop.LcOpModel.SetOriginalUnits(Units)
    Autodesk.Navisworks.Api.Interop.LcVwDocument.SetModelUnitsAndTransform(...)

So the DOCUMENT's units cannot be set. Each MODEL's can, through the one public managed
member, reached as `Document.Models`. Whether `Document.Units` then follows from the models
it holds is **UNKNOWN** and cannot be read off the DLL. It needs a run.

`Federator.Addin.Engine.DocumentUnits` therefore sets every model, logs what the document
said before and after, and says plainly when the document did not follow. It never claims
the change worked. It runs BEFORE the clash step, so every tolerance and every distance is
read in the units the report goes out in. Nothing is converted afterwards: the tolerance,
the distances and the coordinates all come out of the document in the document's units, so
a report saying metres while the document says feet is a report that lies.

#### What differed, cell by cell

Nine things. Every VALUE already matched, which is why the previous check passed.

| what | ours was | theirs is | fixed |
|---|---|---|---|
| Distance value | `-0.328083992004395` behind format `0.000` | `-0.116`, rounded, no format | yes |
| Layer, both items | empty on every row | the level, `GRF` | yes |
| Image cell | `cd000001.jpg` | empty, picture behind it | yes |
| Status wording | `Complete` | `OK` | yes |
| Row 1 height | default | 45 | yes |
| Clash row height | default | 60 | yes |
| Column widths | default | nineteen measured values | yes |
| Fills | none anywhere | five colours, banded | yes |
| Borders | none anywhere | medium box, thick round the test header | yes |

The fills, read off their file:

    heading rows, clash columns A to K   EEEEEE
    heading rows, Item 1 block L to O    99CCFF
    heading rows, Item 2 block P to S    FFCCCC
    clash rows,   Item 1 block L to O    DDEEFF
    clash rows,   Item 2 block P to S    FFEEEE

The row heights, read off their file:

    row 1, the title and the logo        45
    the test heading row                 15.6
    the test values row                  15
    the blank between                    15.6
    the Item 1 and Item 2 row            15
    the column heading row               15
    every clash row                      60

Their test header is a table NINE columns wide that stops at Status, so L to S on those two
rows carry nothing at all and are not expected to. Their borders follow the merge runs:
the left edge on the first column of a run, the right on the last, top and bottom on all of
them, which is what `ClientStyle.Box` reproduces.

`ClosedXML` reports a column width with its own padding of 0.710625 already taken off, and
the file carries it with the padding on. Comparing the two directly reports all nineteen
columns wrong on a file that matches to six decimals.

#### What is still different, and why each one is left

Measured on the two files after every fix above.

| what | ours | theirs | why it is left |
|---|---|---|---|
| sheet width | stops at S | runs to BA, 53 columns | theirs is an HTML table declaring 53 columns. Ours fills the same nineteen and no report reads the empty ones |
| font colour | explicit `FF000000` | theme 1 | renders the same under every stock theme. ClosedXML has no theme colour to write |
| `pageSetup` element | written | absent | ClosedXML writes one whatever we do. An imported page carries none |
| page margins | now theirs | 1/1/0.75/0.75/0.5/0.5 | **fixed**, they were ClosedXML's four defaults |
| hyperlinks | one per picture | none at all | theirs has no picture links, which is exactly why its pictures break when it is moved. Ours is a stated rule and is worth more than the match |
| explicit `none` borders | written on all four sides | omitted | ClosedXML writes all four whenever one is set. An omitted side and `style="none"` are the same border |
| `horizontal="general"` | written | omitted | general IS the default. Same cell, two spellings |
| `vertical="bottom"` | written | omitted | bottom IS the default. Same cell, two spellings |
| picture numbering | by block, renamed once after the run | by block | **same** since F17. Rendered under the run order while the tests run, then ImageRenumbering renames every picture in one pass into report order. Was by the order the tests RAN, recorded in 4p |
| row 2 and row 3 | no cells | a styled blank at A2, row 3 at height 15 | nothing shows in either. Writing cells to match empty cells is noise |
| merged ranges | identical block for block | identical block for block | **same**, verified over rows 1 to 12: `A1:C1`, `A4:B5`, `A7:K7`, `L7:O7`, `P7:S7`, then `A:B`, `C:D`, `I:K` per table row |
| sheet name | the output name, 31 characters | the output name, 31 characters | **same** |
| freeze panes | none | none | **same** |
| auto filter | none | none | **same**, ours went with the Summary sheet |
| embedded pictures | none | none | **same**, the native export never embeds |

#### What the check can and cannot catch after this

`WorkbookCheck` now walks every cell of the first block and compares the value, the data
type, the number format, the fill, the borders, the row height and the column width,
printing both sides of whatever differs. Fourteen tests in `WorkbookCellCheckTests` break
one thing at a time and assert it is named, because a check nobody has watched fail is not
a check.

What it still cannot catch, written down because this is the third time a check here has
reported clean over a real difference:

- **the table being wrong.** The check compares against `ClientLayout` and the writer
  paints from `ClientLayout`. A number wrong in both is invisible. Only
  `ClientLayoutTests` catches that, by opening the sample and asserting every height,
  every fill and every width against their file. Six tests, all green
- **anything not in the list above.** Fonts, font colours, the sheet name, freeze panes,
  print setup and merged ranges are not compared. They are in the table above with both
  sides shown
- **whether a value is true.** It can see the Distance cell holds a number to three
  decimals. It cannot see the number came off the wrong clash
- **a block that was never written.** It checks the blocks it finds

The pass line says all four out loud, so a clean check does not read as approval of what it
did not look at.

#### The tick boxes, before and after

Fifteen. Read out of the real window by `build\probe-window-labels.ps1`, which now opens
every expander first and says which boxes are visible without opening anything.

| box | what it did | kept |
|---|---|---|
| `IncludeSubfolders` | whether the scan walks down | kept, up front, beside the folder it changes |
| `RepublishNwd` | write the NWD | removed, fixed ON. A weekly run wants it every time |
| `WriteHtmlReport` | write the client page | removed, fixed ON. It is the format the client accepts |
| `WriteImages` | render the photos | removed, fixed ON. The accepted report has them |
| `ClientColumnsOnly` | drop our extra columns | removed outright. Dead since the workbook became one sheet with none of ours on it |
| `DateTheNwd` | keep every week's NWD | moved, collapsed |
| `WriteClashXml` | an extra XML per building | moved, collapsed |
| `EmbedThumbnails` | paste photos into cells | moved, collapsed |
| `ImageNew` to `ImageResolved` | which statuses get a photo | moved, collapsed, defaults unchanged at New, Active, Reviewed |
| `ApplyFileSettings` | overwrite existing tests | kept, moved into "Things that destroy data" |
| `CompactResolved` | delete Resolved clashes | kept, moved into "Things that destroy data" |

Eleven boxes remain and ONE is visible without opening anything. A units combo replaced the
three that became fixed, defaulting to metres.

### 4c. The version string, added 2026-08-30

The diagnostic log header carries the Navisworks version as the API reports it. Read by
reflection off the same install on 2026-08-30.

`Autodesk.Navisworks.Api.ApplicationParts.ApplicationVersion` is sealed, base
`System.Object`, and every member is get only:

```
public int ApiMajor { get }          public string Runtime { get }
public int ApiMinor { get }          public string RuntimeLanguage { get }
public int Build { get }             public int RuntimeMajor { get }
public bool IsApiStable { get }      public int RuntimeMinor { get }
public bool IsRuntimeBeta { get }    public string RuntimeProductName { get }
```

It declares no public methods, and it does NOT override `ToString`. `ToString` resolves to
`System.Object.ToString`, which would log the type name and nothing else. The version
string therefore has to be built from the properties above, which is what
`NavisworksFacts.VersionString` does.

NOT CHECKED, still UNKNOWN, because all of it needs Navisworks actually running: whether
the button appears where expected, whether the bundle loads, whether an append or a
publish succeeds against a real NWC, and how long a real run takes.

### 4d. The search sets API, added 2026-08-31

What rebuilding a selection set through the API needs. Read by reflection off the same
install, same way, on 2026-08-31.

Reaching the tree, and putting things in it:

```
Autodesk.Navisworks.Api.Document
    public Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets SelectionSets { get }

Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets
    public Autodesk.Navisworks.Api.FolderItem RootItem { get }
    public System.Void AddCopy(Autodesk.Navisworks.Api.SavedItem item)
    public System.Void AddCopy(Autodesk.Navisworks.Api.GroupItem parent, Autodesk.Navisworks.Api.SavedItem item)
    public System.Void Clear()
    public System.Void EditDisplayName(Autodesk.Navisworks.Api.SavedItem item, System.String newDisplayName)
```

AddCopy takes a copy, so the item that ends up in the tree is not the object that was
handed in. To use a new folder as a parent it has to be found again in
`parent.Children` afterwards. `SavedItemCollection` has `Count` and an `Item` indexer.

```
Autodesk.Navisworks.Api.FolderItem : GroupItem : SavedItem
    public FolderItem()
    public string DisplayName { get; set }          [from SavedItem]
    public SavedItemCollection Children { get }     [from GroupItem]

Autodesk.Navisworks.Api.SelectionSet : SavedItem
    public SelectionSet()
    public SelectionSet(Autodesk.Navisworks.Api.Search search)
    public SelectionSet(Autodesk.Navisworks.Api.ModelItemCollection items)
    public bool HasSearch { get }
    public Autodesk.Navisworks.Api.Search Search { get }
    public Autodesk.Navisworks.Api.ModelItemCollection GetSelectedItems()
    public Autodesk.Navisworks.Api.ModelItemCollection GetSelectedItems(Autodesk.Navisworks.Api.Document document)
```

The search itself:

```
Autodesk.Navisworks.Api.Search
    public Search()
    public Autodesk.Navisworks.Api.SearchLocations Locations { get; set }
    public bool PruneBelowMatch { get; set }
    public Autodesk.Navisworks.Api.SearchConditionCollection SearchConditions { get }
    public Autodesk.Navisworks.Api.Selection Selection { get }
    public ModelItemCollection FindAll(Autodesk.Navisworks.Api.Document document, System.Boolean reportProgress)
    public ModelItem FindFirst(Autodesk.Navisworks.Api.Document document, System.Boolean reportProgress)

Autodesk.Navisworks.Api.Selection
    public System.Void SelectAll()

Autodesk.Navisworks.Api.SearchConditionCollection
    public Void Add(SearchCondition item)
    public Void AddGroup(IEnumerable`1 from)
```

One condition, built in one call:

```
Autodesk.Navisworks.Api.SearchCondition
    public SearchCondition(Autodesk.Navisworks.Api.NamedConstant categoryCombinedName,
                           Autodesk.Navisworks.Api.NamedConstant propertyCombinedName,
                           Autodesk.Navisworks.Api.SearchConditionOptions options,
                           Autodesk.Navisworks.Api.SearchConditionComparison comparison,
                           Autodesk.Navisworks.Api.VariantData value)

Autodesk.Navisworks.Api.NamedConstant
    public NamedConstant(string name)
    public NamedConstant(string name, string displayName)
    public string Name { get }          the internal string, what the API matches on
    public string DisplayName { get }   the word a person reads

Autodesk.Navisworks.Api.VariantData
    public static VariantData FromDisplayString(System.String value)
```

There is no overload of the SearchCondition constructor without a category, so a
condition that carried no category element has to pass null for
`categoryCombinedName`. Whether the API accepts null there is UNKNOWN, it needs
Navisworks running. The builder passes null, catches whatever comes back, and reports
that set as FAILED with the exception rather than approximating a category.

#### What flags 64 means, established not guessed

`Autodesk.Navisworks.Api.SearchConditionOptions` is a `[Flags]` enum over int, and its
bit values are the same numbers the exchange XML writes in its `flags` attribute:

```
None                            = 0
IgnoreCategoryDisplayName       = 1
IgnoreCategoryName              = 2
IgnorePropertyDisplayName       = 4
IgnoreDisplayNames              = 5
IgnorePropertyName              = 8
IgnoreNames                     = 10
IgnoreDisplayStringValueCase    = 16
NegateCondition                 = 32
StartGroup                      = 64
IgnoreDisplayStringValueAccents = 128
IgnoreDisplayStringCharWidths   = 256
```

So `flags="64"` is `SearchConditionOptions.StartGroup`. `SearchCondition` also carries a
matching `Options` property and a `StartGroup()` method, which is the same idea reached
the other way. This replaces the guess in the Search Set Infra notes above, which said
flags was the bit joining one condition to the next. StartGroup does begin a group, so
that reading was pointing the right way, but the name and the meaning are now measured.

The comparison values the two test strings map to:

```
Autodesk.Navisworks.Api.SearchConditionComparison, plain enum over int
    None = 0                     Equal = 6                     NumericGreaterThan = 11
    HasCategory = 1              NotEqual = 7                  DisplayStringContains = 12
    NotHasCategory = 2           NumericLessThan = 8           DisplayStringWildcard = 13
    HasProperty = 3              NumericLessThanOrEqual = 9    DateTimeWithinDay = 14
    NotHasProperty = 4           NumericGreaterThanOrEqual = 10 DateTimeWithinWeek = 15
    SameType = 5
```

`test="equals"` is `Equal`, and `test="contains"` is `DisplayStringContains`. Every other
value in that enum is a test string this tool has never seen in a real file, so a set
carrying one is reported by name and skipped rather than mapped on a guess.

```
Autodesk.Navisworks.Api.SearchLocations, Flags over int
    None = 0, Self = 1, Descendants = 2, DescendantsAndSelf = 3
```

#### Settled by a real run on 2026-08-31

Bader ran the reference file against an open model. 60 of the 61 sets were created and
resolved, so three things that were UNKNOWN above are now answered:

- **A null category is accepted.** `BLD-AR-Walls` and `BLD-AR-Stairs` each carry a
  condition with no category element, `LcOaNodeSourceFile contains "-AR-"`, and they
  returned 2564 and 19 items. Passing null for `categoryCombinedName` works and does not
  throw. There is no need to invent a category for a condition that has none.
- **A set created from a Search resolves.** `GetSelectedItems(document)` on the copy found
  in the tree returned real counts, and the fallback line for a set that could not be
  found again never appeared.
- **The folder tree rebuilds.** Sets landed under `Mechanical/Mechanical-HVAC` and the
  rest at the right depth.

#### AddCopy returns void, so there is nothing to hold on to

Checked on 2026-08-31 because the obvious fix for the folder failure below would have been
to keep whatever the add handed back. Nothing on `DocumentSelectionSets` hands anything
back. Every add shaped method returns void:

```
Void AddCopy(GroupItem parent, SavedItem item)
Void AddCopy(SavedItem item)
Void InsertCopy(GroupItem parent, Int32 index, SavedItem item)
Void InsertCopy(Int32 index, SavedItem item)
Void ReplaceWithCopy(GroupItem parent, Int32 index, SavedItem item)
Void ReplaceWithCopy(Int32 index, SavedItem item)
```

The only ways back to an item are `RootItem`, `Value`, `ToSavedItemCollection`,
`ResolveGuid`, `ResolveIndexPath` and `ResolveReference`. So the collection has to be read
again after an add. There is no alternative to reading it again, only a choice about what
to read it from, and the answer is a freshly read `RootItem`.

`SavedItemCollection` is a mutable `IList<SavedItem>` with `Add`, `AddRange`, `Insert` and
`IndexOfDisplayName`, and `GroupItem.Children` returns one. So a detached folder tree can
be built in memory before being added, which is worth knowing if the read-again approach
ever proves not to be enough.

#### A handle held across an AddCopy does not show the new child

The one failure in that run was ours:

```
FAILED  lcop_selection_set_tree/Architecture/BLD-AR-Floors  2 conditions
        InvalidOperationException: The folder "Architecture" was added but could not be found again.
```

`BLD-AR-Floors` is the first set in the file. It created the `Architecture` folder with
`AddCopy(parent, folder)` and then searched the same `parent` handle for it and did not
find it. Every later Architecture set worked, because each set re-read `sets.RootItem`
at the start and that fresh read did show the folder.

So the rule, which the builder now follows everywhere: after any `AddCopy`, resolve again
from a freshly read `DocumentSelectionSets.RootItem` rather than reusing the handle that
was passed to the add. Never search a `GroupItem` you were holding before the add.

Two things about this are still UNKNOWN and are not worth guessing at. Whether the
staleness is specific to the very first add into an empty tree, and why the same stale
handle worked for finding a `SelectionSet` immediately after adding one while failing for
a `FolderItem`. Re-reading from the root covers both cases, so the cause does not have to
be settled to be safe from it.

NOT CHECKED, still UNKNOWN: whether a set that finds zero items does so because the model
genuinely lacks that content or because the condition is wrong. The tool cannot tell those
apart, so a ZERO line now prints the question the set asked, in internal names, and leaves
the judgement to the reader.

#### CreateCopy and CopyFrom, the two F24 and F29 could not measure, measured 2026-09-19

Step 10 of `steps\03_bader_next.md` has carried a line since F24 asking Bader to paste the
error if a build ever named `CreateCopy` or `CopyFrom` on `DocumentSelectionSets`, because
neither could be read from where the code was being written. Both are read now, by
reflection off `C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll`
on the machine that has the install:

```
Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets
    public System.Collections.ObjectModel.Collection<Autodesk.Navisworks.Api.SavedItem> CreateCopy()
    public System.Void CopyFrom(Autodesk.Navisworks.Api.SavedItemCollection)
    public System.Void CopyFrom(System.Collections.Generic.IEnumerable<Autodesk.Navisworks.Api.SavedItem>)
```

THE COPY IS NOT A `SavedItemCollection`. It is a `Collection<SavedItem>`, which is an
ordinary BCL collection and not one of Navisworks' own. The rebuild held it in a
`SavedItemCollection` and that is CS0029, which is one of the eighteen errors F69 found and
fixed. `CopyFrom` has two overloads and the copy goes back in through the `IEnumerable`
one, so the round trip is `CreateCopy` then `CopyFrom` with nothing converted in between.

`Collection<SavedItem>` is not itself `IDisposable` and every `SavedItem` in it is, and
`CreateCopy` CREATES, so the rebuild disposes each item once the copy has been put back.
Section 4g is why that matters.

Two more members on the same type, not needed by anything today and recorded because they
were read in the same pass and the list above is what a reader will trust next time:

```
    public System.Boolean Remove(Autodesk.Navisworks.Api.SavedItem)
    public System.Void RemoveAt(System.Int32)
```

**CORRECTED ON 2026-09-20 BY 5z. THAT LIST OF TWO IS FOUR, AND THERE ARE TWO MORE BESIDE
IT.** What this section printed was the SINGLE ARGUMENT forms only. The parent scoped
forms exist on the installed 2025 DLL and were named nowhere except in prose about a
DIFFERENT collection, which is how a reader ends up believing they are not there. Read by
reflection off `DocumentSelectionSets` on 2026-09-20:

```
    Void    RemoveAt(GroupItem, Int32)
    Void    RemoveAt(Int32)
    Boolean Remove(GroupItem, SavedItem)
    Boolean Remove(SavedItem)
    Void    Move(GroupItem, Int32, GroupItem, Int32)
    Void    Move(Int32, Int32)
    Void    Clear()
```

`Move` is in that list too, on this collection, where no printed member list anywhere in
this document had recorded it.

**AND THE ONE ARGUMENT FORM IS A TRAP, NOT A CONVENIENCE.** `Remove(SavedItem)` addresses
the ROOT collection. Against a set nested in a folder it returns **False** and throws
nothing. A caller reads that false as "there was nothing to remove" when the truth is
"the removal did not happen", which is the same family of fault as a reader that returns
nothing and looks like an answer. Anything removing a nested item uses the parent scoped
form AND reads the tree back rather than trusting the return value.

Those are about the SETS tree. They say nothing about whether a MODEL can be taken out of
an open document, which is section 5a and WAS answered in 5c on 2026-09-19: both
Document.RemoveFile(int) and TryRemoveFile(int) are there. What it COSTS was the part
left open, and 5x answered that on 2026-09-20: nothing at all. This line said 5a was
still UNKNOWN while 5a own heading said it had been answered, and the two disagreed
for a day.

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

## Finding a Navisworks file: use a direct path test, never a search

Rule, and it holds whatever a future session thinks it has found: any check for a
Navisworks file tests that one full path directly. Never search, never recurse, never
wildcard the install folder. `Exists()` in MSBuild and `Test-Path` on a joined path in
PowerShell are both direct tests and are the only two forms used here. There is no search
anywhere in the build, and `Federator.Addin.csproj` now tests each referenced DLL by its
own full path so a missing file is named rather than falling through to a resolution
warning.

### What was investigated on 2026-08-30, and what was actually true

A build failure was reported, `Autodesk.Navisworks.Api.dll not found under NavisworksPath`,
together with a report that `Get-ChildItem -Recurse -Filter` returned nothing on the
Navisworks folder while a plain `dir -Name` listed everything. Two causes were suggested,
that the existence check was recursing, and that the spaces in the folder name were
mangling the path.

Measured on this machine. NEITHER symptom reproduced, and BOTH suggested causes are ruled
out:

```
File.Exists on the full path of each DLL          : True for all four, sizes read back
   Autodesk.Navisworks.Api.dll                    4261152 bytes
   Autodesk.Navisworks.Clash.dll                   508704 bytes
   Autodesk.Navisworks.Automation.dll              184088 bytes
   Roamer.exe                                      214296 bytes
Directory.GetFiles, top level                     : 428 files
Get-ChildItem -Recurse -Filter                    : 1 result, 0 errors
Get-ChildItem -Recurse, no filter                 : 21474 files, 0 errors
Directory.GetFiles with AllDirectories            : 1 result
Directory.GetDirectories, top level               : 42 subfolders, no reparse points
```

Recursive walks of that folder work. Nothing blocks them.

The existence check never recursed. `Exists()` is an MSBuild built in that tests one path.
MSBuild was asked directly what it evaluates:

```
env:NavisworksPath                                : not set
dotnet msbuild -getProperty:NavisworksPath        : C:\Program Files\Autodesk\Navisworks Manage 2025
MSBuild Exists() on the Api DLL                   : PRESENT
```

The spaces were not mangling anything either. The compiler command line from a clean
rebuild carries both references correctly quoted:

```
/reference:"C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll"
/reference:"C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Clash.dll"
```

`dotnet build ParsonsNwcFederator.sln -c Release` succeeded, both incrementally and after
deleting the add-in's bin and obj. Three ways of passing the override were tried. With a
trailing backslash it built. Without one it built. Unquoted it failed, but with
`MSB1008: Only one project can be specified`, which is a different error and not the one
reported.

So the reported failure is UNKNOWN in cause. It did not happen here on 2026-08-30 and no
measurement on this machine explains it. What was changed is worth having regardless: the
check now tests each referenced DLL by its own full path, names the file and the path it
looked for when one is missing, and trims a trailing slash off `NavisworksPath` so a
caller supplied path with one still builds a valid file path. If the failure returns,
`dotnet build -v:detailed` now prints the three resolved paths under
`CheckNavisworksPresent`, which is the first thing to read.

## Why the button did not appear, measured 2026-08-30

The bundle installed cleanly and the button still did not appear on any ribbon tab. Two
attributes in `PackageContents.xml` were wrong. Both were found by comparing our manifest
against the bundles that already load on this machine, not by reading documentation.

Every Navisworks targeting bundle on this machine, side by side:

```
BUNDLE                            Platform        SerMin SerMax AppType         ModuleName
CODIGEM Clash Detection Matrix    NAVMAN          Nw22   Nw22   ManagedPlugin   ./Contents/v22/COGMClashDetectionMatrix/...
ParsonsGlbExporter                NAVMAN|NAVSIM   Nw22   Nw22   ManagedPlugin   Contents\v22\ParsonsGlbExporter.dll
ParsonsNwcFederator, ours, BEFORE Navisworks      Nw22   Nw22   (none)          ./Contents/v22/Federator.Addin.dll
```

Across all 111 ComponentEntry blocks in every bundle on the machine, the distinct Platform
values were `NAVMAN` 6, `NAVMAN|NAVSIM` 1, `Revit` 28, `Civil3D` 10, `AutoCAD*` 6 and
others. `Navisworks` appeared exactly once and it was ours.

1. `Platform="Navisworks"` should be `NAVMAN`, the product token for Navisworks Manage.
   `NAVMAN` is a real token, it is present as a string inside the product's own
   `lcwebservices.dll`. This is believed to be the one that actually stopped the load,
   because `RuntimeRequirements` is what selects a `Components` block for the running
   product. A Platform the loader does not recognise means the block never matches, so the
   `ComponentEntry` inside it is never read and nothing else in the file gets a chance to
   matter.
2. `AppType="ManagedPlugin"` was missing. Every Navisworks bundle that loads here declares
   it. It tells the loader the module is a managed assembly holding plugin classes.

Also removed, because no working Navisworks bundle on this machine carries either and both
are AutoCAD demand loading concepts: `LoadOnCommandInvocation` and `LoadOnAutoCADStartup`.

### What was ruled out, so it is not searched again

The plugin class is correct and was never the problem. Reflected off the installed DLL:

```
class                  Federator.Addin.FederatorPlugin
public                 True        abstract  False       nested  False      generic  False
parameterless ctor     True, public
base class             Autodesk.Navisworks.Api.Plugins.AddInPlugin,
                       Autodesk.Navisworks.Api, Version=22.0.0.0, PublicKeyToken=d85e58fa5af9b484
Execute                Int32 Execute(string[]), public, declared on FederatorPlugin
PluginAttribute        Name "ParsonsNwcFederator", DeveloperId "PARS",
                       DisplayName "Parsons NWC Federator"
AddInPluginAttribute   AddInLocation = 1, which is AddInLocation.AddIn
assembly               Federator.Addin, Version=1.0.0.0, net48, MSIL, runtime v4.0.30319
```

The Navisworks plugin id is `ParsonsNwcFederator.PARS`, built as Name plus DeveloperId.
The manifest's `ComponentEntry AppName` is `ParsonsNwcFederator`. These are two different
things and they are not required to match. CODIGEM's AppName is `Clash Detection Matrix`
while its plugin id is something else entirely, and it loads.

Also ruled out:

- the bundle folder name. `ParsonsNwcFederator.bundle`, lower case suffix, correct. Two
  bundles on the machine use `.Bundle` with a capital B and are presumably ignored
- the ModuleName path. `./Contents/v22/Federator.Addin.dll` resolves to a file that exists
- mark of the web. None of the three installed files carry any alternate data stream, so
  nothing is blocked
- architecture and framework. MSIL and net48, correct for the x64 host
- a second Navisworks. Only Navisworks Manage 2025 has a Roamer.exe, the Exporters folders
  do not

### Navisworks does not write a plugin load failure anywhere findable

Searched `%LOCALAPPDATA%` and `%APPDATA%` on 2026-08-30. The only Navisworks files are
licensing logs at `%LOCALAPPDATA%\Autodesk\Logs\AdlSdk-Navisworks Manage 2025-*.log`, whose
contents are `ENCODEDv2{...}` and carry nothing about plugins, plus Chromium web view
caches. Nothing anywhere on disk mentioned `ParsonsNwcFederator` or `Federator.Addin`
except our own manifest. Roamer.exe contains the string `PackageContents`, so it does read
bundles, but no string matching `plugin load`, `plug-in load`, `load errors`,
`DisplayPluginLoadErrors` or `developer` in either UTF-16 or ASCII. Whether the application
has an in-product setting that reports plugin load errors is UNKNOWN and could not be
confirmed from outside a running Navisworks.

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

### SETTLED on 2026-08-28: this file is the reference file

The first version of CLAUDE.md said of this file "file holds no sets at all". Bader
confirmed on 2026-08-28 that the file is right and the note was wrong. CLAUDE.md now says
so: this file holds both parts, 61 sets and 1830 tests, all 61 of its test locators
resolve against its own sets, and it is the reference file. Every other fact the note gave
about it checked out exactly.

The knock-on effect is that the cross-file check still lands where the note said it would,
0 of 61, but for a reason the note did not give. See "Cross file" below.

The rule vocabulary in this file, which is what a good export looks like:

```
category LcRevitData_Element display Element
  property LcRevitPropertyElementCategory display Category, the Revit category
  property lcldrevit_parameter_-1002053 display Workset

no category element at all
  property LcOaNodeSourceFile display Source File

condition test values : equals and contains
condition flags values: 0 and 64
value data types      : wstring only
```

Rebuilding a search through the API matches on the internal string, never on the display
word, so the reader keeps both and never swaps one for the other.

### Counting distinct rules: 53 and 59 are both right

Bader counted 59 distinct condition tuples and this scan counted 53. Measured again on
2026-08-28, and the two numbers answer different questions. Neither is a mistake.

```
distinct CONDITION tuples, flags excluded                : 53
distinct CONDITION tuples, flags included                : 53
distinct CONDITION raw OuterXml                          : 53
distinct SETS by whole ordered condition list            : 59
```

53 is the number of distinct conditions. 59 is the number of distinct sets, comparing each
set's whole ordered list of conditions. It is 59 rather than 61 because two pairs of sets
carry byte for byte identical rule lists:

```
BLD-EL-Telecom Fixtures    and  BLD-EL-Telephone Devices
BLD-EL-Electrical Fixtures and  BLD-EL-Devices
```

The condition level definition is the one the code uses, and it is written into a comment
above `AnIndividualConditionIsTheUnitOfARuleAndThereAreFiftyThreeOfThem` in
`AllInOneFileTests.cs`. Both numbers are asserted, the 59 by
`CountingWholeSetsInsteadGivesFiftyNineBecauseTwoPairsMatch`, which also names the two
pairs so the difference stays visible.

The set level number cannot be used for the damaged export check. On Search Set Infra it
gives 6, because those 26 sets differ only in how many copies of the one rule they carry,
and 6 does not read as broken. The condition level number gives 1 there, which does.

### Search Set Building.xml

A DAMAGED EXPORT. Kept as a sample only, to prove HealthCheck catches it. Never a
reference for what a good file looks like.

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

A DAMAGED EXPORT. Kept as a sample only, to prove HealthCheck catches it. Never a
reference for what a good file looks like.

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

## Settled on 2026-08-28

1. The 1104 file holds 61 sets and the note that said otherwise was wrong. It is the
   reference file. CLAUDE.md now says so and the tests assert it.
2. The output name reads one way only: project, originator, building code, ZZZ, BM, the
   type code, 000001. Level and number are fixed because outputs overwrite, and because
   the files in one group may disagree on both. `ContainerNameSettings` now defaults
   `LevelPart` to 4 with `ForcedLevel` ZZZ and `NumberPart` to 7 with `ForcedNumber`
   000001. `ContainerNameSettings.WithoutFixedLevelAndNumber()` turns that off for reading
   a name apart rather than naming a federation.
3. Two files in one group that disagree on the project code or the originator are reported
   and skipped, never resolved by picking one. `BuildingGroupingResult.Skipped` carries the
   building, the reason, and both offending file names.
4. The 53 against 59 rule count is settled. Both are right and they answer different
   questions. See "Counting distinct rules" above.

### Consequence of fixing the level and the number

A name now has to split into at least 7 parts to be readable, because the output name
cannot be built without a level part and a number part to overwrite. A 6 part name is
reported unreadable rather than guessed at. Before this change the floor was 5 parts.

## Still open for Bader

1. The Clash Detective report defaults are still UNKNOWN. Reflection cannot read them,
   they need the running application. Tell me where to look or run it once and I will read
   it from there.
2. The output name carries the type code through from the input, MOD in every sample seen.
   Only the level and the number are fixed. If the type code should be pinned to MOD as
   well, say so and it becomes one more forced part.

## 5a. Can a model be taken out of an open document, asked 2026-09-18, ANSWERED in 5c on 2026-09-19

This section records a QUESTION and is not a measurement. Nothing below was read off a
DLL. It is here so the next person does not spend the search again, and so the answer has
somewhere to land.

F50 asks whether one model can be removed from an open document without clearing the
whole thing. It matters because the NWF is the record: it carries the file list, the
sets, the tests, every clash result with the statuses a person set by hand, and since F52
the viewpoints. A CHANGED group today clears the document and appends again, then puts
the sets and the tests back and fails loudly if either does not come back. Every new thing
the NWF carries makes that longer and riskier. If a model can be taken out on its own,
the rebuild becomes append what is missing and remove what is gone, nothing needs putting
back and nothing can be lost.

What this file already holds, and it is not enough:

- section 4b records `public Autodesk.Navisworks.Api.DocumentParts.DocumentModels Models { get }`
  on `Document`, and says `DocumentModels.Count` is an int
- section 4q records `public void DocumentModels.SetModelUnitsAndTransform(...)`
- nothing named Remove, Delete or Detach against a model appears anywhere in this file.
  The only two Remove members recorded in the whole of it are `RemoveExpiryDate` and
  `RemovePassword` on `PublishProperties`, which are nothing to do with models

So whether such a member exists is **UNKNOWN**. It was not searched for and found absent.
It was simply never read.

`tools\probes\probe-model-remove.ps1` is the probe that answers it. It prints every member
of `DocumentModels`, every member of `Model`, and every method anywhere in the API
assembly, public or not, whose name holds remove, delete, detach, unload, close, drop,
eject or discard AND which takes a `Model` or an index on a model or document type. If it
prints nothing under THE QUESTION, the answer is no and the clear stays.

Why it was not run when this section was written: there is no `Autodesk.Navisworks.Api.dll`
and no PowerShell in the container this repo is developed in, and the add-in has never
compiled there either. The probe is Bader's to run, as a numbered step in
`steps\03_bader_next.md`.

Until it is answered, F50 takes the other branch: the clear and restore stays and widens
from two things to four, so the viewpoints and the statuses are counted out and counted
back the same way the sets and the tests always were.

## 5b. The saved viewpoint API, asked 2026-09-18, ANSWERED in 5d on 2026-09-19

This section records a QUESTION and is not a measurement. Nothing below was read off a
DLL.

F52 puts one folder per discipline into the NWF with one saved viewpoint inside it. The
repo has never touched this collection. What this file holds about viewpoints is all of
section 4k and it is about a CLASH's own viewpoint, not a saved one:

```
public Viewpoint DocumentClashTests.TestsViewpointForResult(IClashResult result)
public void Document.CurrentViewpoint.CopyFrom(Viewpoint viewpoint)
public void View.CopyViewpointFrom(Viewpoint viewpoint, ViewChange change)
public Viewpoint View.CreateViewpointCopy()
public bool ClashResult.HasSavedViewpoint { get }
```

and the sentence "Nothing here writes a viewpoint into the NWF."

`DocumentSavedViewpoints` appears nowhere in this file and nowhere in the repo. So all
four things F52 needs are **UNKNOWN**: how a folder is made, how a viewpoint is added into
one, whether a name can be set, and whether anything has to be disposed.

`tools\probes\probe-viewpoints.ps1` answers it. It prints `DocumentSavedViewpoints`,
`SavedViewpoint`, `Viewpoint`, `FolderItem`, `GroupItem` and `SavedItem` whole, and prints
`DocumentSelectionSets` beside them, because the sets tree is the closest thing this tool
already builds and section 4d records its shape:

```
public Autodesk.Navisworks.Api.FolderItem RootItem { get }
public System.Void AddCopy(Autodesk.Navisworks.Api.SavedItem item)
public System.Void AddCopy(Autodesk.Navisworks.Api.GroupItem parent, Autodesk.Navisworks.Api.SavedItem item)
```

If the two collections have the same shape, that is a measurement and F52 follows it. If
they differ, the difference is what the probe exists to find. The shape is NOT assumed to
be the same on the strength of the pattern, because that is how a member gets written down
as if it had been read.

`src\Federator.Addin\Engine\SavedViewpoints.cs` is the one place in the add-in that rests
on this, and it says so at the top. A build error there means the assumption was wrong,
which is the whole reason it is one file and one method.

## 5c. The answer to 5a, MEASURED 2026-09-19

`tools\probes\probe-model-remove.ps1` was run on the machine with Navisworks Manage 2025
installed, against `Autodesk.Navisworks.Api 22.0.0.0`. Section 5a above is the question and
this is the answer. 5a is left standing because the reasoning in it is why the answer
matters.

**A MODEL CAN BE TAKEN OUT OF AN OPEN DOCUMENT. The member is on `Document` and not on
`DocumentModels`, which is why searching `DocumentModels` for it found nothing:**

```
Autodesk.Navisworks.Api.Document  ->  public void RemoveFile(int index)
Autodesk.Navisworks.Api.Document  ->  public bool TryRemoveFile(int index)
```

Both take an INDEX and not a `Model`. The Try form returns a bool, which this tool reads
and never discards.

What `DocumentModels` itself carries, for the record, is a removal pair that is not public
API in any useful sense:

```
public bool InternalRemove(Model item)
public void InternalRemoveAt(int index)
```

and `DocumentModels.IsReadOnly` is a get only property, so the list is not meant to be
edited through the collection.

The append side, printed in the same pass for the comparison:

```
public void AppendFile(string fileName)
public void AppendFiles(IEnumerable<string> fileNames)
public bool TryAppendFile(string fileName)
public bool TryAppendFiles(IEnumerable<string> fileNames)
public void Clear()
public bool IsClear { get }
```

WHAT THIS DOES AND DOES NOT SETTLE. It settles that the member exists, its name, where it
lives and what it takes. It settles NOTHING about what removing a file does to the sets,
the clash tests, the clash results or the saved viewpoints that point into that model, and
that is the only question F50's rebuild actually turns on. A rebuild that removed one file
and lost every clash result would be worse than the clear and copy it replaces. Whether to
change the rebuild is Bader's decision and it needs a run, not a reflection pass.

## 5d. The answer to 5b, MEASURED 2026-09-19

`tools\probes\probe-viewpoints.ps1` was run on the same machine and the same assembly.
Section 5b is the question and this is the answer.

**THE TYPE EXISTS AND THE NAME THE CODE ASSUMED IS RIGHT.**

```
Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints Document.SavedViewpoints { get }
```

The collection, with the members F52 needs marked:

```
Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints
    base type: System.Object
    IDisposable: False
    public FolderItem RootItem { get }                                  a folder tree, as assumed
    public SavedItemCollection Value { get }
    public SavedItem CurrentSavedViewpoint { get; set }
    public void AddCopy(SavedItem item)                                 puts one in at the root
    public void AddCopy(GroupItem parent, SavedItem item)               puts one in a folder
    public void EditDisplayName(SavedItem item, string newDisplayName)  a name CAN be set
    public void InsertCopy(GroupItem parent, int index, SavedItem item)
    public bool Remove(GroupItem parent, SavedItem item)
    public void RemoveAt(GroupItem parent, int index)
    public void ReplaceWithCopy(GroupItem parent, int index, SavedItem item)
    public void ReplaceFromCurrentView(SavedViewpoint savedViewpoint)
    public SavedViewpoint CaptureRuntimeOverrides()
    public Collection<SavedItem> CreateCopy()
    public void CopyFrom(SavedItemCollection value)
    public void CopyFrom(IEnumerable<SavedItem> value)
    public void Clear()
```

The item that goes in it:

```
Autodesk.Navisworks.Api.SavedViewpoint : SavedItem
    IDisposable: True                                   so it IS disposed
    public SavedViewpoint()
    public SavedViewpoint(Viewpoint viewpoint)          made from a viewpoint
    public Viewpoint Viewpoint { get }
    public bool ContainsVisibilityOverrides { get }
    public bool ContainsAppearanceOverrides { get }
    public VisibilityOverrides GetVisibilityOverrides()
    public AppearanceOverrides GetAppearanceOverrides()
```

`Autodesk.Navisworks.Api.Viewpoint` is `IDisposable` too, and `FolderItem` has a public
parameterless constructor, which is how a folder is made.

**THE TWO COLLECTIONS HAVE THE SAME SHAPE.** `DocumentSavedViewpoints` and
`DocumentSelectionSets` carry the same `RootItem`, `AddCopy`, `InsertCopy`, `Move`,
`Remove`, `RemoveAt` and `ReplaceWithCopy` members with the same signatures. 5b said the
shape was NOT to be assumed from the pattern. It was not assumed, it was read, and the
pattern held.

**CORRECTED ON 2026-09-20 BY 5z: THIS SENTENCE WAS RIGHT AND IT WAS IN THE WRONG PLACE.**
It is the only record anywhere in this document that `DocumentSelectionSets` carries the
parent scoped `Remove(GroupItem, SavedItem)` and `RemoveAt(GroupItem, Int32)`, or any
`Move` at all, and it says so in prose about the VIEWPOINT collection while the printed
member list for the SET collection, section 4b, showed only the single argument forms. A
reader who went to the list for sets found two members and concluded that was all there
is. Section 4b now carries the measured list, and this paragraph stays as the record of
how the gap happened: a fact recorded only as a comparison to something else is a fact a
reader will miss.

**ALL FOUR OF 5b's QUESTIONS ARE ANSWERED.** A folder is a `FolderItem`, made with its
public constructor and put in with `AddCopy(GroupItem, SavedItem)`. A viewpoint goes in
the same way. A name is set with `EditDisplayName`. `SavedViewpoint` and `Viewpoint` are
both `IDisposable` and both are disposed.

**HIDING IS THE HALF THAT IS STILL NOT SETTLED.** What the probe found for it:

```
Autodesk.Navisworks.Api.ModelItem.IsHidden                        get only
Autodesk.Navisworks.Api.DocumentParts.DocumentModels.SetHidden(IEnumerable<ModelItem>, bool)
Autodesk.Navisworks.Api.DocumentParts.DocumentModels.ResetAllHidden()
Autodesk.Navisworks.Api.SavedViewpoint.GetVisibilityOverrides()
Autodesk.Navisworks.Api.SavedViewpoint.ContainsVisibilityOverrides
```

So items are hidden through `DocumentModels.SetHidden` and a saved viewpoint can CARRY
visibility overrides. Whether a viewpoint saved while items are hidden RECORDS that
hiding, and whether it restores it when the viewpoint is pressed, is not readable off the
DLL and is still **UNKNOWN**. `ContainsVisibilityOverrides` is the thing to read on a real
run, because it answers that question directly.

WHAT WAS NOT DONE. `SavedViewpoints.CanBuild` is still false and nothing in
`src\Federator.Addin\Engine\SavedViewpoints.cs` was changed. Turning it on means writing
`Add`, `ShowOnly` and `ShowOnlyLargeItems` against the members above, which is the second
half of F52 and a feature rather than a build fix. The measurement is here so that work
starts from what was read rather than from what was assumed. Bader decides when.

## 5e. Does anything report that an opened document has finished loading, asked 2026-09-19, MEASURED 2026-09-19

THE ANSWER IS AT THE END OF THIS SECTION. The question is left standing above it.

WHY IT IS ASKED. On the first real run all five existing NWFs reported
`0 unchanged, 4 added, 0 removed` and were rebuilt, and `STEP DECIDE finished 0.248s`.
Every open in the one earlier log on record, run-20260907-093440.log, took between 2.3
and 4.8 seconds and produced a file list. So `Document.TryOpenFile` returning true does
not mean `Document.Models` is filled, and reading the model count the instant it returns
reads a document that is still filling.

WHAT TO READ, on a machine with Navisworks Manage 2025 installed, against
`Autodesk.Navisworks.Api.dll`:

    Document                  every member whose name holds Load, Ready, Busy, Progress,
                              State, Pending or Complete, and every event on it
    Document.Models           the same, plus whether the collection raises a changed event
    DocumentParts.DocumentModels   the same again
    Application               the same, because a progress or busy notion may live there
                              rather than on the document

HOW TO WRITE THE ANSWER. Paste the member list the way 5c and 5d paste theirs, then say
in one line whether any of them answers "the models are all in now". If one does,
`Federator.Core.Rerun.ModelLoadWait` becomes the fallback rather than the answer and the
add-in reads the member instead. If none does, the poll stands and this section says so,
and that sentence is the reason it stands.

WHAT IS ALREADY DECIDED AND DOES NOT WAIT ON THIS. A count of zero out of an NWF that
opened with no error is never a rebuild. `NwfComparison.ReadEmpty` stops the group with
a reason that names the file and says what to do. That is F74 and it holds whichever way
this measurement goes.

THE ANSWER, read by `tools\probes\probe-document-ready.ps1` on the machine of 2026-09-19 on
2026-09-19 against `Autodesk.Navisworks.Api 22.0.0.0`. The question above is left standing
because the reasoning in it is why the answer matters.

`Document` carries ONE member of the seven words and it is not about loading:

```
public LcOwDocument State { get; }
```

and fourteen events, none about loading:

```
ActiveViewChanged, ActiveViewChanging, ViewRemoved, ViewAdded, ActiveSheetChanged,
ActiveSheetChanging, TransactionEnded, TransactionBeginning, FilesUpdated, FilesUpdating,
FileSaved, FileSaving, UnitsChanged, FileNameChanged
```

`DocumentModels` carries the one member in the assembly that names the thing asked about,
and a private watcher behind it:

```
public event EventHandler<...> SceneLoaded
private SceneLoadedEventStateWatcher m_scene_loaded
```

beside six more events, `ModelItemPropertiesChanged`, `ModelTransformChanged`,
`ModelTransformChanging`, `ModelGeometryMaterialChanged`, `CollectionChanging` and
`CollectionChanged`. So the collection DOES raise a changed event, which was the second half
of the question.

`Application` carries the progress machinery and nothing about a document being loaded:
`BeginProgress` in three overloads, `EndProgress`, eight events from `ProgressBeginning` to
`ProgressEnded`, an `Idle` event, and `LoadDocumentInfo` and `TryLoadDocumentInfo`, which
read a file's info without opening it. Elsewhere in the assembly `DocumentDatabase` has
`Loaded` and `Unloading` and `IApplicationBim360` has `RefreshComplete`. None of those is
the models.

WHAT THIS SETTLES AND WHAT IT DOES NOT. There is a member that says the models are in,
`DocumentModels.SceneLoaded`. What a DLL cannot say is WHEN it fires against a call to
`Document.TryOpenFile`: inside the call before it returns, after it returns once the message
loop runs, or not at all for a file whose models are already on disk. A handler subscribed
after an event that already fired waits for ever, and a run that waits for ever is worse
than the run that rebuilt five NWFs. So THE POLL STANDS AS THE READER, because the poll
reads the count itself and needs no promise about timing, and this sentence is why it
stands. `Federator.Core.Rerun.ModelLoadWait` is the rule and the add-in reads
`Document.Models.Count` into it.

WHAT THE WIRING DOES ABOUT THE EVENT, so the next run measures what this could not. Step
365 subscribes to `SceneLoaded` before the open and unsubscribes when the wait ends, and
writes ONE line beside the LOADING line saying whether it fired and at what second on the
monotonic clock. That is information and the run acts on none of it. When a real run shows
it firing after the open returns and before the count settles, on every open, it becomes
the answer and the poll becomes the fallback, which is the order asked for above.

WHAT THE RUN THEN SHOWED, 2026-09-19 21:13, ten groups, seven of them opening an NWF. The
event line beside every LOADING line read the same way each time, for example:

```
LOADING  the NWF reported 4 models after 0.721s, steady over 3 reads 0.250s apart, over 3 readings in all
LOADING  the scene loaded event fired 4 times, first at 0.0s and last at 0.1s after the open began, and the open returned at 0.2s
```

So `SceneLoaded` fires ONCE PER MODEL, INSIDE `TryOpenFile`, before it returns. A handler
subscribed after the open would never hear it, which is the case this section warned
about. On every one of the seven opens the first reading of the count was already the
full count and the wait settled on the third reading, under a second. Nothing on this run
read empty, so the refusal was not exercised and neither was the ceiling.

WHAT THAT LEAVES. The poll stays as the reader, because it reads the thing itself and
costs half a second. The event is now MEASURED as usable, but only subscribed before the
open, and it could replace the poll as the primary signal with the poll as the fallback.
Whether to make that change is Bader's, and it is not made here. What the first real run
saw, five NWFs reading empty the instant the open returned, was NOT reproduced on this
machine with these files, so what caused it there is still UNKNOWN.

## 5f. What the property API offers for walking an item's properties, asked 2026-09-19, MEASURED 2026-09-19

THE ANSWER IS AT THE END OF THIS SECTION. The question is left standing above it.

WHY IT IS ASKED. F86 writes one CSV per NWC saying, per Revit category, which property
tabs and property names the items carry and which distinct values appear on each, so the
mechanical selection sets can be rewritten against what the models actually hold rather
than against what a set name implies. Nothing in this tool has ever walked every property
of an item. `ClashHarvest` reads named properties one at a time and stops at the first
that answers.

WHAT TO READ:

    ModelItem.PropertyCategories            the collection, and what one element is
    PropertyCategory.DisplayName             the tab a person sees
    PropertyCategory.Name                    the internal name the API matches on
    PropertyCategory.Properties              the collection under one tab
    DataProperty.DisplayName, .Name          the same pair for one property
    DataProperty.Value                       and every ToDisplayString or typed reader on
                                             VariantData, because a value that comes back
                                             as a type name rather than a value is the
                                             whole probe wasted
    Search / SearchCondition                 whether a whole model can be walked without
                                             recursing every item by hand

HOW TO WRITE THE ANSWER. The member list, then one line saying how a value is turned
into the text a CSV cell holds, and one line saying whether the walk is per item or
whether a search can do it in one pass. The Core half, `Federator.Core.Probe`, already
fixes the CSV columns, the cap and the sort, so only the reading is open.

THE ANSWER, read by `tools\probes\probe-properties.ps1` on the machine of 2026-09-19 on 2026-09-19
against `Autodesk.Navisworks.Api 22.0.0.0`. The question above is left standing because
it says what the answer is for.

The walk starts on `ModelItem`, whose members of interest are all get only:

```
PropertyCategoryCollection    PropertyCategories
ModelItemEnumerableCollection Children, Descendants, DescendantsAndSelf
ModelItem                     Parent
String                        DisplayName, ClassDisplayName, ClassName
Boolean                       IsComposite, IsInsert, IsLayer, HasGeometry
Model                         Model
```

`PropertyCategoryCollection` is `IEnumerable<PropertyCategory>` and `IDisposable`, with
finders by name, by display name and by combined name for a category and for a property.
One element of it is a `PropertyCategory`, which is `IDisposable` and carries the tab:

```
String                 Name            the internal name the API matches on
String                 DisplayName     the tab a person sees
DataPropertyCollection Properties      the collection under one tab
NamedConstant          CombinedName
```

`DataPropertyCollection` is an `IList<DataProperty>` with `Count` and an indexer and the
same three finders. One element is a `DataProperty`, `IDisposable`, with the same pair:

```
String      Name
String      DisplayName
VariantData Value
```

`VariantData` is `IDisposable` and carries a `DataType` of `VariantDataType`, fourteen
values:

```
None = 0, Double = 1, Int32 = 2, Boolean = 3, DisplayString = 4, DateTime = 5,
DoubleLength = 6, DoubleAngle = 7, NamedConstant = 8, IdentifierString = 9,
DoubleArea = 10, DoubleVolume = 11, Point3D = 12, Point2D = 13
```

with one `Is` property and one `To` reader per value: `ToDisplayString`,
`ToIdentifierString`, `ToNamedConstant`, `ToBoolean`, `ToInt32`, `ToDouble`,
`ToDoubleLength`, `ToDoubleArea`, `ToDoubleVolume`, `ToDoubleAngle`, `ToAnyDouble`,
`ToDateTime`, `ToPoint2D`, `ToPoint3D`. It also overrides `ToString`, and what that
returns is UNKNOWN off the DLL, so nothing here reads it as a cell.

HOW A VALUE BECOMES THE TEXT A CSV CELL HOLDS, in one line: switch on `DataType` and call
the one reader for it, a display string as itself, an identifier string as itself, a named
constant by its `DisplayName`, the five doubles through `ToAnyDouble` written invariant,
an integer and a boolean as themselves, a date written invariant round trip, a point as its
coordinates joined with a space, and None as an empty cell. A length is in the DOCUMENT'S
units, the same as the size properties F53 reads, and the probe writes the number as read
and does not convert it, because the probe reports what is there.

WHETHER THE WALK IS PER ITEM OR PER SEARCH, in one line: both exist and the wiring uses
the per item walk. `Search.FindAll(document, false)` with
`SearchCondition.HasPropertyByDisplayName(tab, property).EqualValue(...)` over
`SearchLocations.DescendantsAndSelf` finds every item of one category in one pass, which
is the shape `SetBuilder` already builds a set with. `Model.RootItem.DescendantsAndSelf`
walks a whole model item by item. The probe walks item by item and asks each item its
category through the same reader the penetration rule uses, `ClashHarvest.FirstPropertyOn`
over `ProbeSettings.CategoryNames`, because a search would match on a TAB name that the
settings do not carry and the probe exists to find out what the tabs are called. What that
walk costs is measured and written in the PROBE block, per file.

## 5g. Does Navisworks import a NEGATED search condition, asked 2026-09-19, MEASURED 2026-09-20
MEASURED on 2026-09-20 in the dimming round. The measurement and what it decided are
under 5g, measured, further down, after 5o. What follows here is what was asked.

THIS SECTION HOLDS NO MEASUREMENT. It is the question and how to answer it.

WHY IT IS ASKED. F87 rewrites `BLD-EL-Devices`, which asks for Electrical Fixtures and is
therefore the same set as `BLD-EL-Electrical Fixtures`. What it should ask for is category
CONTAINS "Devices" AND NOT the six named device categories. Whether the exchange format
carries a negation at all, and what the `test` attribute reads when it does, cannot be
read off the two sample files, because every condition in both is `equals` or `contains`.

WHAT TO DO. In Clash Detective, build one search set by hand with a negated condition,
export the selection sets to XML, and read the `<condition>` element back. Paste the
element here. Then import that same XML into a fresh document and confirm the set comes
back with the negation still on it, because a format that writes a thing it will not read
is worse than one that writes nothing.

WHAT WAS SHIPPED WITHOUT IT. The fallback, `Category equals "Nurse Call Devices"`, which
is in `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` today. `equals` is proved to
import, because the whole file uses it and the real run imported all 61 sets. The negated
form is the better set and is not written until this is measured.

WHAT REFLECTION CAN SAY, read by `tools\probes\probe-properties.ps1` on the machine of 2026-09-19
on 2026-09-19 against `Autodesk.Navisworks.Api 22.0.0.0`, which is not the measurement
above and does not close it:

```
public SearchCondition SearchCondition.Negate()
SearchConditionOptions.NegateCondition = 32
```

The API carries a negation, as a method on the condition and as a bit on the options enum,
between `IgnoreDisplayStringValueCase = 16` and `StartGroup = 64`. F78 measured that the
file's `flags` attribute IS this enum, because the five conditions carrying `flags="64"`
are the five that start a group. So IF the exporter writes a negated condition it would
carry `flags="32"`, or `96` where it also starts a group, and `SetBuilder.BuildCondition`
passes the flags through as `SearchConditionOptions` unchanged, so a file carrying 32
would build a negated condition through the API without a line changing.

WHAT IS STILL NOT MEASURED, which is the whole of the question: whether Navisworks WRITES
that bit when a person exports a set built with a negation, and whether it READS it back
on import. Neither can be read off a DLL. Both need the hand built set and the round trip
above, on a run. The fallback stays in `exchange\` until then.

## 5h. Can a comment be written on a clash result, asked 2026-09-19, MEASURED 2026-09-19

THE ANSWER IS AT THE END OF THIS SECTION. The question is left standing above it.

WHY IT IS ASKED. F72c wants the NWF itself to say WHY a clash was moved to Reviewed, so a
person reading the Clash Detective panel next week sees the reason without opening a log,
and wants an undo that touches only the clashes this tool moved.

WHAT TO READ, against `Autodesk.Navisworks.Clash.dll` and `Autodesk.Navisworks.Api.dll`:

    ClashResult                  every member whose name holds Comment, Note, Description,
                                 Tag, UserName, Status or Approved
    IClashResult                 the same
    SavedItem / Comment          whether the general Comments collection reaches a clash
                                 result at all, and whether a comment survives a save and
                                 reopen of the NWF
    ClashTest                    the same list, because a per test note may be the only
                                 writable text

HOW TO WRITE THE ANSWER. The member list, then one line saying whether a comment can be
written, and one saying whether it comes back after a save and reopen. IF IT CANNOT BE
DONE, the answer is one line in the log saying so and the status alone is set. Nothing is
faked and no second file stands in for a comment the NWF does not hold.

THE ANSWER, read by `tools\probes\probe-clash-comments.ps1` on the machine of 2026-09-19 on
2026-09-19 against `Autodesk.Navisworks.Api 22.0.0.0` and `Autodesk.Navisworks.Clash
22.0.0.0`. The question above is left standing because it says what the answer is for.

A COMMENT CAN BE WRITTEN ON A CLASH RESULT, through one member on the document part,
which is the same shape the status edit has:

```
public void DocumentClashTests.TestsEditResultComments(IClashResult result, CommentCollection comments)
public void DocumentClashTests.TestsEditResultStatus(IClashResult result, ClashResultStatus status)
```

The comments already on a result are read off it, and the same member is on the group,
the interface and every `SavedItem`:

```
public CommentCollection ClashResult.Comments        { get; }
public CommentCollection ClashResultGroup.Comments   { get; }
public CommentCollection IClashResult.Comments       { get; }
public CommentCollection SavedItem.Comments          { get; }
```

`CommentCollection` is a list with `Add`, `Insert`, `Remove`, `Clear`, `Count`, an indexer
and a copy constructor `CommentCollection(CommentCollection from)`. One `Comment` is made
two ways and every property on it is read only once it is made:

```
public Comment(string body, CommentStatus status)
public Comment(string body, CommentStatus status, string author)
public Comment Document.CreateCommentWithUniqueId(string body, CommentStatus status)
public Comment Document.CreateCommentWithUniqueId(string body, CommentStatus status, string author)

long         Id
DateTime     CreationDate
string       Author
CommentStatus Status        New = 0, Active = 1, Approved = 2, Resolved = 3
string       Body
```

The other writable text on a result, for the record, is `Description`, `ApprovedBy` and
`ApprovedTime`, each with its own `TestsEditResult` member, and `TestsEditResultAssignedTo`.
None of them is used for the record, because a description is the clash's own field in the
panel and a person may type in it, and the record has to be somewhere a person would not.
`ClashTest` itself has no text member of the seven words at all, only `Status`.

HOW THE RECORD IS WRITTEN, which step 372 wires: copy `result.Comments` into a new
`CommentCollection`, add a comment made by `Document.CreateCommentWithUniqueId` with
`AutoReviewRecord.Text()` as the body, `CommentStatus.New` and this tool's name as the
author, and call `TestsEditResultComments` with the result and the collection, BEFORE
`TestsEditResultStatus` on the same handle. Whether the handle survives the first edit for
the second is not readable off the DLL, for the reason at the top of section 5: every
mutator on `DocumentClashTests` is a copy form. If it does not, the status edit throws,
the log says so by clash name and the run goes on, and that line is the next measurement.

WHAT IS STILL NOT MEASURED. Whether the comment SURVIVES a save and a reopen of the NWF,
and whether it shows in the Clash Detective panel. Both wait for the run in PART 5 of the
wiring round, with the by design box on, and the answer goes here.

WHAT THE RUN THEN SHOWED, 2026-09-19 21:13 and the second run at 21:29, log
`steps\logs\run-20260919-211323.log`. 145 clashes were moved to Reviewed by rule B with a
record written on each through `TestsEditResultComments`, before the status and on the
same handle, and not one write threw: the handle survived the comment edit for the
status edit that followed. The second run opened every NWF off the disk, and the Undo
auto Reviewed button pressed on the last of them, 1A02WO, read the records back:
`10 put back of 69 looked at`, each `back to Active`, which is the status its record
named, and 59 `this tool never moved it`. So the comment SURVIVES a save and a reopen and
comes back readable. Whether it shows in the Clash Detective panel was not looked at and
is still UNKNOWN.

## 5i. Which category values are real Revit categories, asked 2026-09-19, NOT MEASURED

MEASURED on 2026-09-20 in the viewpoints round. The measurement and what it decided are
under 5i, measured, further down, after 5m. What follows here is what was asked.

THIS SECTION HOLDS NO MEASUREMENT. It is the question and how to answer it.

WHY IT IS ASKED. F84 warns about a selection set asking for a category value that is not
a Revit category at all, which is what `BLD-EL-Telecom Equipment` does: it asks for
"Telephone Equipment", and nothing in any model is in a category of that name, so the set
can never match anything and its 60 clash tests can never find a clash.

WHAT TO DO. Open one real federation, walk every item's category property, and write the
distinct values out. That is the same walk F86's probe does, so the probe answers this as
a side effect and the two should be measured on the same run. The list goes in a FILE
under `src\Federator.Core`, read by the health check, never typed into a method, because
it is a list read off the client's models and it will change when their models do.

UNTIL THEN the health check has nothing to compare against and that half of F84 reports
nothing rather than guessing. The other two halves, identical condition pairs and a set
name breaking its siblings' pattern, need no measurement and are built.

## 5j. Does a saved viewpoint record the hidden state it was saved with, MEASURED 2026-09-19

The one thing 5d could not read off the DLL, measured on a run. `tools\probes\ViewpointProbe`
is a plugin assembly with no Core reference, loaded into a Navisworks started through
`Autodesk.Navisworks.Api.Automation` with `AddPluginAssembly` and run with
`ExecuteAddInPlugin("ViewpointProbe.PARS", ...)`, on the machine of 2026-09-19 against a COPY of
`1104-PAR-1A0215-ZZZ-BM-MOD-000001.nwf`, three models, under
`C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints\probe`. The whole result file is
what follows, cut only where a line repeats.

```
models 3, saved viewpoints at the root 4
after SetHidden: IsHidden(two) = True, root0.IsHidden = True, root1.IsHidden = True
A  new SavedViewpoint(Viewpoint) made, adding it
probe A camera alone: ContainsVisibilityOverrides = False, ContainsAppearanceOverrides = False
   GetVisibilityOverrides() returned null
B  CaptureRuntimeOverrides() returned a SavedViewpoint, adding it
probe B runtime overrides: ContainsVisibilityOverrides = True, ContainsAppearanceOverrides = True
   GetVisibilityOverrides() returned Autodesk.Navisworks.Api.VisibilityOverrides
ResetAllHidden: IsHidden(two) = False
press probe A camera alone: IsHidden(two) = False, root0.IsHidden = False, root1.IsHidden = False
press probe B runtime overrides: IsHidden(two) = True, root0.IsHidden = True, root1.IsHidden = True
TrySaveFile = True, size on disk 66233 bytes
after Clear: models 0
reopened: models 3, saved viewpoints at the root 6
after reopen, before pressing anything: IsHidden(two) = False
probe A camera alone after reopen: ContainsVisibilityOverrides = False
press probe A camera alone: IsHidden(two) = False
probe B runtime overrides after reopen: ContainsVisibilityOverrides = True
press probe B runtime overrides: IsHidden(two) = True, root0.IsHidden = True, root1.IsHidden = True
```

**THE ANSWER IS YES, BY ONE OF THE TWO WAYS AND NOT THE OTHER.**

- `new SavedViewpoint(Viewpoint)` records the CAMERA ALONE. `ContainsVisibilityOverrides`
  reads false, `GetVisibilityOverrides()` returns null, and pressing it from a clean view
  hides nothing. A viewpoint written this way opens on the whole federation
- `DocumentSavedViewpoints.CaptureRuntimeOverrides()` records the current view WITH what
  is hidden. `ContainsVisibilityOverrides` reads true, the overrides carry a
  `ModelItemCollection` called `Hidden`, and pressing it from a clean view hides the two
  models again. It reads true and presses the same way after the NWF is saved, cleared
  and reopened, so the record is in the file and not in the session
- `ContainsAppearanceOverrides` reads true on the captured one too, so it carries the
  colour and transparency state of the moment as well. Nothing here set any, so what it
  carries is whatever the document had, and a writer that wants a clean viewpoint sets
  the view up before capturing rather than after

Two things read on the way. `ContainsVisibilityOverrides` THROWS `NullReferenceException`
inside its getter on a `SavedViewpoint` that is not yet in a document, so it is read off
the copy in the tree after `AddCopy` and never off the object handed to it. And the probe
copy went from 78,341 bytes to 66,233 after the API saved it with two more viewpoints in
it, so the size of a file this API writes is not the size the last save left, and every
size in the round is read off the disk rather than reasoned about.

WHAT THIS DECIDES. The writing half of F85 is built, capturing each clash viewpoint with
`CaptureRuntimeOverrides` after the other disciplines are hidden and the camera is set
from `TestsViewpointForResult`, and `SavedViewpoints.CanBuild` goes true once the run
shows the tree. The unknown named at 5d, in `SavedViewpoints.cs`, in `ViewpointBuilder.cs`
and in `03_bader_next.md` step 375 is closed by this section.


## 5k. How the hidden state the document holds is read and put back, MEASURED 2026-09-19

The review of the viewpoints round found that `ViewpointBuilder` put the hidden state back
with `DocumentModels.ResetAllHiddenToModelState`, whose XML doc reads "Resets the hidden
status to that defined in the constituent models", which is the NWC files' state and not
what the NWF held, and that nothing read the state before the writer hid anything. The
probe in `tools\probes\ViewpointProbe`, mode `restore`, measured four routes on a copy of
the 1A02MM NWF, four models, through the automation host, the same day. The result file
is `tools\probes\ViewpointProbe\5k-result-20260919.txt`.

```
SetHidden(root0): IsHidden(root0) = True
ResetAllHiddenToModelState: IsHidden(root0) = False   (false means the document level hide is LOST by that call)
GetAllHiddenAtModelState: 0 item(s) in 0 ms
route 1: CaptureRuntimeOverrides in 0 ms, returned a SavedViewpoint
route 1: GetVisibilityOverrides off the un-added capture returned an object, Hidden.Count = 1
route 1: ContainsVisibilityOverrides off the un-added capture = True
route 2: the setter threw ArgumentException: Argument 'item' is not in SavedViewpoints
route 2b: added, root count 7 -> 8
route 2b: pressed the tree copy, IsHidden(root0) = True
route 2b: Remove returned True, root count now 7
route 3: walked 2606 items in 7 ms, 1 hidden
route 3: SetHidden(kept, true) in 0 ms, IsHidden(root0) = True   (true means the walk puts the hide back)
```

**WHAT IT SAYS.**

- `ResetAllHiddenToModelState` LOSES a hide the document holds. The review was right and
  the builder no longer calls it anywhere
- a capture from `CaptureRuntimeOverrides` can be READ WITHOUT being put into the tree:
  `GetVisibilityOverrides().Hidden` is a `ModelItemCollection` of exactly the hidden
  items, and `ContainsVisibilityOverrides` reads true on it. 5j said that flag threw
  `NullReferenceException` on a viewpoint not yet in a document, and it did, on the
  camera-alone `new SavedViewpoint(Viewpoint)` which carries no overrides object. On a
  capture it reads
- a capture NOT in the tree cannot be pressed: `CurrentSavedViewpoint` refuses it with
  `ArgumentException`. Added to the tree it presses and `Remove(SavedItem)` takes it out
  again, so that route exists, and it is not the one used
- `ResetAllHidden` then `SetHidden(collection, true)` puts the same items back, whether
  the collection came off a capture or off a walk, and `IsHidden(collection)` reads true
  after, which is the check the builder writes to the log
- the walk of every item reading `IsHidden` cost 7 ms over 2,606 items on this file, so
  it is affordable, and it is not needed

**WHAT THIS DECIDES.** `SavedViewpoints.SnapshotHidden` captures once, before the first
viewpoint changes anything, and holds the capture and its `Hidden` collection without
adding either to the tree. `SavedViewpoints.RestoreHiddenState(document, snapshot)` is
`ResetAllHidden` then `SetHidden(snapshot.Hidden, true)` and returns `IsHidden` on that
collection, which the VIEWS block says. A group that hid nothing takes no snapshot and
restores nothing. The line in `SavedViewpoints.cs` that listed `ResetAllHiddenToModelState`
among the measured members lists it no longer.

## 5l. What a runtime capture records and what the clash viewpoint call returns, MEASURED 2026-09-19 and 2026-09-20

The first viewpoints run wrote 975 viewpoints into ten NWFs and the tree looked complete.
Pressed by hand, every one of them opened on the same empty top view, at grid A(-10)-2(14)
on level LGF, with the right disciplines hidden. The probe in `tools\probes\ViewpointProbe`,
mode `camera`, measured the pieces on a copy of the 1A02MM NWF. The result file is
`tools\probes\ViewpointProbe\5l-result-20260920.txt`, and the failing run is
`steps\logs\run-20260920-082641.log`.

- `DocumentClashTests.TestsViewpointForResult(result)` returns a DIFFERENT camera per
  clash, positioned beside the clash items' bounding boxes, focal distance 20 to 60 units,
  so the camera the writer asked for was right
- `Viewpoint.CreateCopy()` of it survives the disposal of the original, and
  `DocumentCurrentViewpoint.CopyFrom(copy)` reads back the same position, so the copy
  route the writer used was right too
- `DocumentSavedViewpoints.CaptureRuntimeOverrides()` returns a SavedViewpoint whose
  `Viewpoint.Position` THROWS `InvalidOperationException: Camera not set`, in the
  automation host and in the window alike. It records the overrides and NO camera. A
  viewpoint with no camera opens on a default view, which is the empty top view every
  pressed viewpoint showed. The API doc says only "Creates a view that captures current
  runtime overrides", and it means exactly that
- the second run, with the camera read back off every written viewpoint, failed every
  one of the 975 planned with that exception and created none, which is the read back
  doing its job. The seven groups with clashes came out FAILED and the three without
  came out DONE, the second NWF save, the workbook, the NWD and the confirm still ran in
  every group, and the run took 8 minutes 10 seconds, 490.483s, against 5 minutes 9
  seconds the first time, because the failure text was written 667 times

So the writer's two halves each recorded half: `new SavedViewpoint(Viewpoint)` the camera
alone, 5j, and `CaptureRuntimeOverrides` the hidden state alone. `SavedViewpoint.Viewpoint`
has no setter, and nothing on `SavedViewpoint` sets overrides.

## 5m. A saved viewpoint with BOTH the camera and the hidden state, MEASURED 2026-09-20

Two routes were measured on a copy of the 1A02MM NWF, modes `record` and `com` of the
probe. The result files are `tools\probes\ViewpointProbe\5m-replace-result-20260920.txt`
and `5m-com-result-20260920.txt`.

**`DocumentSavedViewpoints.ReplaceFromCurrentView`, whose doc reads "Viewpoint, Redlines
and visibility are updated to those in the current View", does NOT record the hidden
state.** A camera only viewpoint put in a folder, then replaced while a model root was
hidden, read back with `ContainsVisibilityOverrides` false and pressed without hiding
anything. Whether the window's own option, Save Hide/Required Attributes, would change
that is UNKNOWN and beside the point: 27 machines cannot depend on an option.

**The COM API's saved view records both.** `Autodesk.Navisworks.ComApi.dll` and
`Autodesk.Navisworks.Interop.ComApi.dll`, both in the install folder:

```
InwOpState10 state = ComApiBridge.State;
InwOpView view = (InwOpView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpView, null, null);
view.name = name;
view.ApplyHideAttribs = true;
view.ApplyMaterialAttribs = false;
view.anonview = ComApiBridge.ToInwOpAnonView(camera);
state.SavedViews().Add(view);
```

```
camera applied and root0 hidden: True
COM view added, root count 7 -> 8
read back through .NET at the root: ContainsVisibilityOverrides True, camera position (12.5, -34.25, 56.125)
copied into the folder, removing the root one: True
the folder copy: ContainsVisibilityOverrides True, camera position (12.5, -34.25, 56.125)
moved away: root0 hidden False, position (100, 100, 100)
pressed the folder copy: root0 hidden True, position (12.5, -34.25, 56.125)
TrySaveFile = True
after reopen, the folder copy: ContainsVisibilityOverrides True, camera position (12.5, -34.25, 56.125)
after reopen, pressed: root0 hidden True, position (12.5, -34.25, 56.125)
```

- the view is added at the ROOT of the tree. `AddCopy(folder, it)` puts a copy in the
  folder that keeps both the camera and the overrides, and `Remove(it)` takes the root
  one out, so the tree ends with one viewpoint where the plan put it
- read back through the .NET API it is an ordinary `SavedViewpoint`, and pressing it
  through `CurrentSavedViewpoint` hides what was hidden and moves the camera to what
  was given, before and after a save, a clear and a reopen
- `heightField` read 0.953 after pressing where 0.785 was given, so the field of view is
  the window's and not the recorded one. The position is exact

**WHAT THIS DECIDES.** `SavedViewpoints.Record` writes every clash viewpoint through the
COM view, the add-in references the two COM DLLs the same way it references the other
two, copy local false, and `SavedViewpoints.ReadBack` reads three things off every
written viewpoint before it is counted as created: that it is there, that its camera sits
within `ViewpointSettings.CameraReadBackTolerance` of the clash camera, and that it
carries visibility overrides where it was meant to hide a discipline. The window's view is
never touched, so nothing of it has to be put back. `CaptureRuntimeOverrides` stays for
one thing, reading the hidden state before the writer hides anything, 5k.

## 5i, measured. Every category value the C02 models carry, MEASURED 2026-09-20

The probe in `tools\probes\ViewpointProbe`, mode `walk`, opened the ten NWFs of the C02
folder one after another through the automation host and read, off every item of every
model, the first property whose display name is Category, Revit Category or Element
Category, the way the add-in's harvest and the penetration rule read one. The result
file is `tools\probes\ViewpointProbe\5i-result-20260920.txt`.

```
walked 482 items, 144 carrying a category, 2 models        1000BS
walked 31127 items, 9273 carrying a category, 3 models     1A0215
walked 828 items, 333 carrying a category, 6 models        1A02BS
walked 2606 items, 909 carrying a category, 4 models       1A02MM
walked 4505 items, 2196 carrying a category, 1 models      1A02MS
walked 568 items, 240 carrying a category, 4 models        1A02WE
walked 2860 items, 1041 carrying a category, 8 models      1A02WL
walked 2361 items, 898 carrying a category, 4 models       1A02WM
walked 716 items, 277 carrying a category, 4 models        1A02WN
walked 1418 items, 555 carrying a category, 4 models       1A02WO
DISTINCT 374
```

47,471 items across 40 models in 21 seconds, 374 distinct values. Around sixty of them
are Revit categories as a person would name them, Walls with 457 items, Floors with
7,035, Structural Framing with 1,745, Lighting Fixtures with 424, Cable Trays with 44,
Ducts with 36. The rest are family and type names, PAR-AR-DOR-SW-SG-MTL-EXT-900 and
three hundred like it, plant species such as Acacia farnesiana, and the names of linked
DWG files, each carried by one or two items, because on these NWCs a node above the
geometry carries a property called Category whose value is its own name.

**WHAT THIS DECIDES.** The list goes into `src\Federator.Core\Exchange\revit-categories.txt`
whole, family names and all, because a list that left them out would not be what was
measured and the tool reads them as a category. `RevitCategories.Measured` is true, the
HEALTH block reads `Revit categories known: 374`, and `SetWarnings.FindCategoriesNobodyHas`
runs. It is one folder of the project, so a category another building carries and these
do not reads as one nobody has until the walk is run over that building too. A test
proves the resource is exactly the probe result's CATEGORY lines.

## 5n. Which model a clashing item lives in, MEASURED 2026-09-20

Three runs of the viewpoint writer read no home model for any clash. Two shapes were
tried, `ClashResult.Selection1[0].Model` behind a `HasModel` check and `ClashResult.Item1.Model`
without one, and both read null on every clash. The probe, mode `home`, measured the
first results of the 1A02MM copy. The result file is
`tools\probes\ViewpointProbe\5n-result-20260920.txt`.

```
Item1 [Thorn Steel Cables] HasModel False, Model null, ancestors and self 9, the one with a model [...\1104-PAR-1A02MM-ZZZ-EL-MOD-000001.nwc] at depth 9
Item2 [Concrete, Cast-in-Place Fcu35 Mpa] HasModel False, Model null, ancestors and self 6, the one with a model [...\1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc] at depth 6
```

- on a clash leaf `HasModel` reads false and `Model` reads null. The API doc's "does
  this item refer to a model" means IS this item a model's root
- the one item of `AncestorsAndSelf` that carries the model is the TOPMOST, the file
  node, six to nine levels above the geometry, and its `Model.FileName` is the same
  string `document.Models[i].FileName` reads, the NWC path
- `ClashHarvest.SourceFileOf` reads `item.Model` off the clash leaf and so, on the
  evidence here, fills the source file column with nothing. That column is not this
  round's and is left for Bader, question 55

**WHAT THIS DECIDES.** `ViewpointBuilder.AddHome` walks `AncestorsAndSelf` to the item
that has a model and reads the file name off that, releasing every wrapper on the way.
A viewpoint of a pair whose code no model carries, DR vs ST, drainage against structure,
keeps the ME model the drainage pipe lives in as well as the structural one, which is what
the log line promised on three runs and only the sixth delivered, after the fifth threw on
every clash enumerating AncestorsAndSelf while disposing each item, and the writer went
back to the Parent walk the size reader uses.

## 5o. Does a saved viewpoint record that items are DIMMED, MEASURED 2026-09-20

F85 shipped 975 viewpoints that open on their clash with the other disciplines hidden.
Bader pressed two and could not see the clash: the camera Clash Detective computes sits
inside a solid beam, and Clash Detective only looks right because its own view dims
everything that is not the two clashing items. Q55 and Q56 answer the same way, dim them.
Whether a SAVED viewpoint can record a dimming at all, and whether the record survives a
save and a reopen, is what 5l caught the camera route failing, so it was measured before
anything was built. The probe is `tools\probes\ViewpointProbe` in its `dim` mode, run
through the automation host against a copy of the 1A02MM NWF, 4 models and 2,606 items.
The result file is `tools\probes\ViewpointProbe\5o-result-20260920.txt`.

**THE ANSWER IS YES, BY BOTH ROUTES, AND THE FLAGS THAT REPORT IT ARE WORTHLESS.**

```
CONTROL, no override at all:  ContainsAppearanceOverrides True, MaterialOverrides 0,   ContainsVisibilityOverrides True, Hidden 0
route T, the view just added: ContainsAppearanceOverrides True, MaterialOverrides 994, ContainsVisibilityOverrides True, Hidden 0
route T with a hide, added:   ContainsAppearanceOverrides True, MaterialOverrides 994, ContainsVisibilityOverrides True, Hidden 1
```

- `SavedViewpoint.ContainsAppearanceOverrides` reads TRUE on a viewpoint written with no
  override of any kind. It is not a report of anything. `GetAppearanceOverrides().MaterialOverrides.Count`
  is the real number: 0 with nothing overridden and 994 with the roots dimmed
- `SavedViewpoint.ContainsVisibilityOverrides` reads TRUE in the same session on every
  viewpoint, including one with nothing hidden. **F85's third read back, that a viewpoint
  carries visibility overrides where it hides a discipline, is a check that cannot fail
  and never could.** `GetVisibilityOverrides().Hidden.Count` is real in the same session,
  0 with nothing hidden and 1 with one model hidden, and this round moves the read back
  onto it
- after a save and a reopen both flags start telling the truth, which is no help to a
  writer that reads back before it saves

**THE OVERRIDE CASCADES AND TWO ITEMS CAN BE BROUGHT BACK, WHICH IS WHAT MAKES IT AFFORDABLE.**

```
route T: OverrideTemporaryTransparency 0.85 on 4 root(s) in 3 ms
   item 1 active 0.85    item 2 active 0.85    other active 0.85
route T: ResetTemporaryMaterials on the two items in 0 ms
   item 1 active 0       item 2 active 0       other active 0.85
route T: the SCOPED undo, ResetTemporaryMaterials on the roots, in 0 ms
   item 1 active 0       item 2 active 0       other active 0
```

An override on the four model roots reaches every leaf under them, and a reset on two
leaves brings exactly those two back while the rest stay dim. So a viewpoint costs TWO
calls and not one per item: dimming 2,606 items one at a time, 430 times, would be 1.1
million calls in the 1A02MM group alone.

**THE WRITER'S REAL SEQUENCE SURVIVES THE REOPEN.** Everything was put back before the
NWF was saved, so a viewpoint holding a reference to live state rather than a snapshot
would come back empty. The document read clean, was saved, cleared and reopened off the
disk:

```
AFTER THE REOPEN, probe dim none:             MaterialOverrides 0,   Hidden null
   after pressing:  item 1 active 0     item 2 active 0     other active 0
AFTER THE REOPEN, probe dim temporary:        MaterialOverrides 994, Hidden null
   after pressing:  item 1 active 0     item 2 active 0     other active 0.85
AFTER THE REOPEN, probe dim temporary hidden: MaterialOverrides 994, Hidden 1
   after pressing:  item 1 active 0     item 2 active 0     other active 0.85
```

Pressing a dimmed viewpoint off a reopened file leaves the two clashing items solid and
everything else at 0.85, and the one that also hid a model brings the hiding back as well.
Hiding and dimming live in one viewpoint and neither costs the other.

**TEMPORARY AND PERMANENT BOTH RECORD, AND TEMPORARY IS THE ONE USED.** Route P,
`OverridePermanentTransparency`, records the same 994 and presses the same way after a
reopen. It is not used, for three measured reasons: it took 8 ms against 3, it writes
`permanent 0.85` onto the item, which goes into the NWF if a save lands before the reset,
and its undo is `ResetAllPermanentMaterials`, which would clear an appearance override the
file already carried with nothing able to read those back first. That is 5k's trap in a
second shape, and the answer is the same. `OverrideTemporaryTransparency` writes nothing
permanent, and its undo is scoped to the roots this tool overrode rather than
`ResetAllTemporaryMaterials`, which would clear a temporary override this tool did not set.

**CLASH DETECTIVE'S OWN DIM VALUE IS NOT READABLE.** `Application.Options` on this install
exposes one member, `Grids`, and the COM state exposes no option member at all. So the
transparency is a SETTING whose default is 0.85, CHOSEN and not measured, and the setting
says so where it is declared.

**ONE NUMBER TO WATCH.** A dimmed viewpoint records 994 material overrides in this file,
one per item carrying a material, where an undimmed one records none. 430 viewpoints in
one group is 427,420 override records where there were none, so the NWF will grow. How far
is not predictable from here and is read off the disk on the run, per group, before and
after.

**WHAT THIS DECIDES.** `SavedViewpoints.Dim` overrides temporary transparency on the model
roots and resets it on the two clashing items, the viewpoint is recorded through the COM
view with `ApplyMaterialAttribs` true beside `ApplyHideAttribs`, the read back becomes
four counts and not three flags, and the temporary override is undone on the roots when
the group's writing ends, the way the hidden state already is.

## 5g, measured. Does Navisworks import a NEGATED search condition, MEASURED 2026-09-20

The question above was asked on 2026-09-19 and is answered here. The probe is
`tools\probes\ViewpointProbe` in its `negate` mode, run through the automation host
against a copy of the 1A02MM NWF, 2,606 items in 4 models. The result file is
`tools\probes\ViewpointProbe\5g-result-20260920.txt`.

**THE ANSWER IS YES, EXACTLY, AND IT SURVIVES THE FILE.**

```
flags=0,  Category equals Lighting Fixtures  found 49 item(s)
flags=32, the same condition negated,        found 4 item(s)
the built condition's Options read IgnoreDisplayNames, NegateCondition = 37, NegateCondition in it: True
TrySaveFile = True
reopened off the disk
after the reopen the set [probe negated set] is there, HasSearch True
   condition 0 Options IgnoreDisplayNames, NegateCondition = 37, NegateCondition in it: True
   it finds 4 item(s) after the reopen
```

The condition is built through the SAME constructor `SetBuilder.BuildCondition` calls,
with the same two Ignore bits it always adds, so 37 is 32 negate plus 1 and 4, and what
was measured is the tool's own call and not a near relative of it. The bit goes into the
NWF and comes back out of it.

**A NEGATION NEEDS A POSITIVE CONDITION BESIDE IT, and that is the whole of why the
standalone number reads 4.** The four are the four MODEL ROOTS:

```
NOT equals Lighting Fixtures, what the 4 are:
   [1104-PAR-1A02MM-ZZZ-AR-MOD-000001.nwc] geometry False, model True
   [1104-PAR-1A02MM-ZZZ-EL-MOD-000001.nwc] geometry False, model True
   [1104-PAR-1A02MM-ZZZ-ME-MOD-000001.nwc] geometry False, model True
   [1104-PAR-1A02MM-ZZZ-ST-MOD-000001.nwc] geometry False, model True
```

A root carries no category at all, so "category is not Lighting Fixtures" is true of it,
it matches, and the search prunes below a match. A negated condition written on its own
therefore selects four file nodes and nothing a person would call an item.

**WITH A POSITIVE BESIDE IT THE ARITHMETIC IS EXACT.** This is the shape F87 wants, a
contains and then one negated equals per category a sibling set already claims:

```
contains Devices                                          found 67 item(s)
   equals [Nurse Call Devices]    on its own 0,  and contains Devices NOT equals it 67
   equals [Data Devices]          on its own 4,  and contains Devices NOT equals it 63
   equals [Security Devices]      on its own 38, and contains Devices NOT equals it 29
   equals [Lighting Devices]      on its own 8,  and contains Devices NOT equals it 59
   equals [Communication Devices] on its own 2,  and contains Devices NOT equals it 65
   equals [Fire Alarm Devices]    on its own 15, and contains Devices NOT equals it 52
```

Every one comes down by exactly the number that named category holds. Conditions in one
group are ANDed, F78, so the negations stack and the set ends up holding the Devices
categories no sibling claims.

**AND THE FALLBACK IT REPLACES FOUND NOTHING.** `equals Nurse Call Devices`, which is
what `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` carried until today, matched ZERO
items in 1A02MM. The set was in every clash test in that building and could never clash
with anything.

**WHAT WAS NOT REACHABLE, said rather than left open.** Whether Navisworks' own EXPORTER
writes `flags="32"` when a person builds a negated set by hand and exports the sets is
still UNKNOWN, and it is not reachable from here: nothing in the .NET API exports a
search set to XML. `Document.ExportAsDwf` is the only export on the document and
`DocumentSelectionSets` has no writer at all, read off the installed
Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-20. It does not block anything: the tool
WRITES this file itself and READS it itself, and both halves are measured. A person who
exports a set by hand and compares is the only way to close the last part, and nothing
waits on it.

**WHAT THIS DECIDES.** `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` carries the real
negated form since 2026-09-20: `BLD-EL-Devices` holds one `contains` condition for
Devices and six `equals` conditions at `flags="32"`, one per category its siblings claim.
`Federator.Core.Exchange.ConditionsRewrite` is the rule that writes it, built off the
set's OWN first condition so nothing in the code names a category or a property of this
project, and the byte for byte test still proves the committed file is exactly what the
rule produces from the sample. The NOT MEASURED wording is gone from the question above.

## 5p. The CHEAPER WRITE ROUTE, and what a viewpoint records of a COLOUR, MEASURED 2026-09-20

Q59 answered d, take the cheap route and measure it. The dimming round found
`InwOpFolderView.SavedViews().Add` and deliberately did not take it. The probe is
`tools\probes\ViewpointProbe` in its `route` and `colour` modes, run through the
automation host against a copy of the 1A02WL NWF, 1,027 items in 8 models. The result
files are `tools\probes\ViewpointProbe\5p-route-20260920.txt` and
`5p-colour-20260920.txt`.

**THE CHEAP ROUTE RECORDS EVERYTHING THE OLD ONE DOES.** Twenty viewpoints of the same
scene, written both ways, saved, closed and reopened off the disk:

```
ROUTE A READ BACK of 20: found 20, with a camera 20, hiding something 20, dimming something 20
ROUTE B READ BACK of 20: found 20, with a camera 20, hiding something 20, dimming something 20
pressed A 1: 1 model(s) hidden carrying 12 item(s), 1015 dimmed at 0.85 and SOLID 2
pressed B 1: 1 model(s) hidden carrying 12 item(s), 1015 dimmed at 0.85 and SOLID 2
```

All four counts hold, and pressing one leaves exactly the two clashing items solid. So
the route is allowed on the rule the brief set.

**AND ON THIS GROUP IT SAVES ALMOST NOTHING.**

```
route A, root add then AddCopy then Remove:   158 ms, 151,743 bytes
route B, straight into the folder collection: 153 ms, 152,158 bytes
```

Three per cent over twenty viewpoints, and 415 bytes MORE on disk. That is not the
saving the dimming round expected, and the reason is that the cost is not the tree
operation count. The dimming round measured 1A02MM at 240 seconds for 430 viewpoints,
558 ms each, where this measures 7.9 ms each for twenty. Seventy times the cost per
viewpoint for two and a half times the items. WHAT GROWS IS THE TREE THE WRITE WALKS,
not the number of calls per write, so a route that makes three walks instead of one
saves three per cent and not two thirds. The real A against B on the real group is in
the round entry in `steps\log.md`.

**WHAT A VIEWPOINT RECORDS OF A COLOUR, which is PART 2's whole read back.** Three
colour pairs on the same two clashing items, each recorded into its own viewpoint,
saved, closed and reopened:

```
ORIGINAL colours: item 1 (0.969,0.969,0.969)   item 2 (0,1,0)

PAIR 1  item 1 red (1,0,0), item 2 green (0,1,0)
   item 1 NAMED, colour (1,0,0)      item 2 NOT NAMED by any override
PAIR 2  item 1 red (1,0,0), item 2 blue (0,0,1)
   item 1 NAMED, colour (1,0,0)      item 2 NAMED, colour (0,0,1)
PAIR 3  item 1 yellow, item 2 magenta
   item 1 NAMED, colour (1,1,0)      item 2 NAMED, colour (1,0,1)
```

**A COLOUR IS RECORDED ONLY WHERE IT DIFFERS FROM THE ITEM'S OWN COLOUR.** Item 2's
original colour IS green, so painting it green writes no override, and the viewpoint
names 1,027 items where pair 2 names 1,028. Pressing pair 1 still shows item 2 green,
because green is what it already was, so THE PICTURE IS RIGHT AND THE RECORD IS EMPTY.

That decides how the read back is written. A read back that insists the viewpoint names
both items would fail a viewpoint that is perfectly correct, roughly as often as a
clashing item happens to be the colour it is being given. So the read back asks what the
viewpoint WILL SHOW for that item: the override's colour where it names the item, and
the item's own colour where it does not. That is the same question a person answers by
looking, and it has one right answer either way.

Everything else about a colour holds. It survives the save and the reopen, pressing the
viewpoint puts it back, and a colour override and a transparency override live in the
same `MaterialOverride` list without either losing the other. `MaterialOverride` carries
`Item`, `Color` and a nullable `Transparency`, read off the installed
Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-20. The two colour overrides REPLACE the
dim entry for those items rather than adding to it, which is why the count goes 1,027
and not 1,029.


## 5q. What an NWC carries of the SHARED COORDINATE, the WORKSETS and the ELEMENT IDS, MEASURED 2026-09-20

Q64 answered: alignment is by shared coordinate, and by eye where that is not readable.
NOTHING WAS BUILT UNTIL THIS WAS READ, because the brief forbids falling back to a
bounding box and calling it an alignment check. The probe is
`tools\probes\ViewpointProbe` in its `survey` mode, which dumps every property of the
model root, of the first item with geometry and of the item above it, then walks every
item. The result file is `tools\probes\ViewpointProbe\5q-result-20260920.txt`.

**THE SHARED COORDINATE IS READABLE, ON THE MODEL ROOT, AND IT IS NAMED.** Every model
root carries a `[Location]` tab, internal `LcRevitPropertyLocation`:

```
[Location] internal LcRevitPropertyLocation
   Latitude         internal revit_Latitude         =  18.219
   Longitude        internal revit_Longitude        =  42.5
   Elevation        internal revit_Elevation        =  0
   ProjectLocation  internal revit_ProjectLocation  =  PW3_Shared_Location
```

`revit_ProjectLocation` is the NAME of the Revit shared site the model was exported on.
It is on the root and nowhere else, one per model, so a group can be compared model
against model with one read each and no walk.

**AND THE MODEL ROOT ALSO CARRIES ITS TRANSFORM**, tab `[Transform]`, internal
`LcOaTransform`, with `Translation.X`, `.Y` and `.Z`, a rotation axis and angle and a
scale. The same numbers come off `Model.Transform` in the .NET API as a `Transform3D`
with `IsIdentity()`, `IsTranslation()` and `Translation`, which is the cheaper read and
is what the tool uses.

**IT IS NOT THE IDENTITY ON ANY MODEL IN THIS PROJECT, so the identity is not the test.**
Across the two groups read, every single model returns `IsIdentity False`, including the
ones whose translation is exactly (0, 0, 0), because the export carries a scale of 3.281
as well. WHAT MATTERS IS WHETHER THE MODELS IN ONE GROUP AGREE WITH EACH OTHER, not
whether any one of them is the identity.

1A02MM, four models, agreeing in X and Y and differing in Z by up to 312 mm:

```
AR  Translation (0, -13.419, -0.509)   ProjectLocation SWLS-02-SharedCoordinate
EL  Translation (0, -13.419, -0.197)   ProjectLocation LTB2
ME  Translation (0, -13.419, -0.492)   ProjectLocation PW3_Shared_Location
ST  Translation (0, -13.419, -0.254)   ProjectLocation Internal
```

1A02WL, eight models, scattered over hundreds of metres:

```
ME-000001  (0, 0, 0)               PW3_Shared_Location
ME-000002  (169.665, -42.536, 0)   SWLS-02 MECH
ST-000001  (111.262, -33.034, 0.656)   LS
ST-000002  (60.529, -86.636, 0)    PWPS_FUEL TANK 01
ST-000003  (-96.606, -166.245, 5.366)  NHC-PUMP STATION
ST-000004  (104.856, -72.521, -9.022)  Internal
ST-000005  (157.279, -12.232, -8.18)   Internal
ST-000007  (-54.565, 160.105, 5.335)   NHC-PUMP STATION
```

**FOUR DIFFERENT SHARED LOCATIONS IN ONE GROUP AND TWO MODELS ON `Internal`.** A model
whose ProjectLocation reads `Internal` was exported on Revit's internal origin and not
on the project's shared coordinates at all, which is the fault Q64 is about. This is
reported and never acted on, Q65.

**WORKSETS AND ELEMENT IDS ARE ON THE REVIT ELEMENT, NOT ON THE GEOMETRY.** The first
survey counted them over items with geometry and read ZERO everywhere, against a real
run that reads 860 of 1,052 ids. The reason is the tree shape: a Revit element reaches
Navisworks as a COMPOSITE item carrying the `[Element]` tab, internal
`LcRevitData_Element`, with `Id` (internal `LcRevitPropertyElementId`) and `Workset`
(internal `lcldrevit_parameter_-1002053`) on it, and the geometry solids UNDER it carry
neither. Counted on the tab and not on the geometry:

```
1A02MM  AR  233 items, 102 with geometry, 86 elements. Workset 86 of 86, Element ID 86 of 86, 100%
1A02MM  EL  1375 items, 494 with geometry, 308 elements. Workset 308 of 308, Element ID 100%
1A02MM  ME  869 items, 347 with geometry, 236 elements. Workset 236 of 236, Element ID 100%
1A02MM  ST  129 items, 53 with geometry, 53 elements. Workset 53 of 53, Element ID 100%
```

So the brief's expectation that worksets would be missing is WRONG for these two groups,
and the real fault is a different one.

**THE WORKSET NAMES DO NOT MATCH WHAT THE MATRIX ASKS FOR, AND THAT IS WHY 33 SETS FIND
NOTHING.** The models carry `ME-Ductwork`, `ME-Piping` and `ME-Equipment`. The matrix
asks for `ME-DUCTWORK`, `ME-PIPING` and `ME-EQUIPMENT`. The condition carries
`flags="64"`, which is `StartGroup` and NOT a case flag: the one that would forgive this
is `IgnoreDisplayStringValueCase`, value 16, read off the installed
Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-20, and nothing sets it, not the client's
file and not `SetBuilder.BuildCondition`, which adds 1 and 4 only. So the comparison is
case sensitive and `ME-DUCTWORK` cannot match `ME-Ductwork`.

The run of 2026-09-20 bears it out. `BLD-ME-Ducts&Duct Fittings`, `BLD-ME-Duct Accessory`
and `BLD-ME-Mechanical Equipment` are all in the 33 that found nothing in every group,
and all three ask for an all capitals workset.

**AND THE CLIENT'S OWN WORKSET NAMES DISAGREE WITH EACH OTHER**, which no rule can fix
and a person has to see. Across two groups:

```
EL-Fire Alarm            and  EL-Fire alarm
EL-Lightning Protection  and  EL-Lightining Protection
EV-Cctv System           and  EV-Ccctv system
PL-Drainage equipment    and  PL-Drainage equipmen
```

Two spellings of the same workset, one of them a typo, in models that are meant to
federate. This is what the EXPORT CHECK block puts in front of a person every run.

## 5r. WHY THE PENETRATION RULE NEVER MOVED A CLASH, MEASURED 2026-09-20

Found by PART 8 of the alignment round, reading the run log of 2026-09-20 end to end.
The probe is `tools\probes\ViewpointProbe` in its `pen` mode, against a copy of the
1A02MM NWF the run had just written. The result file is
`tools\probes\ViewpointProbe\5r-result-20260920.txt`.

**THE SYMPTOM.** F72 shipped on 2026-09-19 and has never moved one clash. Two real runs,
the dimming round's of 11:33 and the alignment round's of 14:03, both read:

```
clashes looked at : 526
moved to Reviewed : 0
        0  the service is over the size, left alone
        0  no size could be read off the service, left alone
        0  a person had already set it, left alone
        0  both sides a service, left alone
        0  both sides a solid, left alone
      526  not a service against a solid, left alone
```

Every clash in one bucket and ZERO in all five others is not a rule deciding, it is a
rule reading nothing. A category that reads empty on both sides is "not a service
against a solid", and so is every other clash, so the block looked like a considered
answer and was an empty one.

**THE CAUSE. A CLASH HAS TWO WAYS TO HAND YOU ITS SIDES AND ONLY ONE OF THEM CAN BE
READ.** `Penetrations.ReadSide` took `ClashResult.Selection1` and `Selection2`.
`ClashHarvest` beside it takes `Item1` and `Item2`. The same clash, the same named item:

```
CLASH 1: BLD-EL-Lighting Fixtures-vs-BLD-ST-Floors  Clash19

--- Selection1 and Selection2, what the penetration rule read ---
   side 2 level 0 [Concrete, Cast-in-Place Fcu35 Mpa]   Category = []   reading threw NotSupportedException
   side 2 level 1 [Floor]                               Category = []   reading threw NotSupportedException
   side 2 level 2 [PAR-STR_FLR-250MM]                   Category = []   reading threw NotSupportedException

--- Item1 and Item2, what the harvest reads ---
   item 2 level 0 [Concrete, Cast-in-Place Fcu35 Mpa]   Category = []   none
   item 2 level 1 [Floor]        Category = [Floors]    [Element]=Floors [Level]=Levels [Revit Type]=Floors
   item 2 level 2 [PAR-STR_FLR-250MM]  Category = [Floors]   [Type]=Floors
```

**`ModelItem.PropertyCategories` THROWS `NotSupportedException` ON AN ITEM A CLASH
SELECTION HANDED BACK**, at level 0 and at every level of the walk up, on both sides of
every clash tried. The same item reached through `Item1` reads its properties perfectly.
Twelve clashes were read and all twelve behaved the same way.

That also explains the thing that looked like a contradiction in the log. The ITEM IDS
block on the same run reads `Id supplied 860 item ids of 1052`, so the tool plainly CAN
read a Revit property off a clash item. It can, through `Item1`. It never could through
`Selection1`.

**WHAT IT COST, beyond the rule not firing.** `Penetrations.ServiceSizeOf` reads a side
the same way, and F85's viewpoint tree asks it for the service size of every clash. So
the `Over 150mm` sub folder could never be reached either: every size read came back as
not read. That is two features off one line, and neither said a word, because both are
written so that a side which will not read is a side with no category and no size, which
is a real answer and never an error.

**THE FIX IS TWO WORDS**, `Item1` and `Item2` in place of `Selection1` and `Selection2`,
and nothing else in the rule changed. What it came to on the same ten groups is in the
alignment round's entry in `steps\log.md`.

**WHAT IS STILL UNKNOWN.** WHY the selection's item refuses `PropertyCategories` is not
readable off the DLL and this does not need it. Whether every clash side behaves this
way on every project, or only on a Revit sourced NWC of this shape, is UNKNOWN too, and
it does not matter: `Item1` reads on this project's files and the rule now uses the one
that reads. The lesson is the general one this repo keeps learning, which is that a rule
whose every clash lands in one bucket is reporting nothing, and that a count of zero
under every OTHER reason is the thing to look at, not the zero at the top.

## 5s. WHY 29 SERVICES HAD NO READABLE SIZE, MEASURED 2026-09-20

PART 1a of the worksets round. The probe is `tools\probes\ViewpointProbe` in its
`census` mode, which mirrors `Penetrations` exactly: `Item1` and `Item2` and never
`Selection1`, five levels up, the same three category names and the same six size
property names. The result file is `tools\probes\ViewpointProbe\5s-result-20260920.txt`.

**THE QUESTION WAS WHICH OF TWO THINGS IT WAS**, because 5r one day earlier had found a
reader that returned nothing and looked like an answer, and 29 services with no readable
size is the same shape.

**IT IS THE READER, AND IT IS A BUG.** The first pass asked only whether a size property
was on the chain at all and found that ALL 74 service against solid clashes carry one:

```
clashes in this document              : 526
a service against a solid             : 74
of those, with no size property at all: 0
```

The second pass asked the narrower question the rule actually asks, whether
`ItemSizes.Read` would get a NUMBER off it, and reproduced the run exactly:

```
of those, NO READABLE SIZE            : 29
by category:  [Conduits] 18   [Cable Tray Fittings] 10   [Pipe Fittings] 1
```

45 read plus 29 not read is the 74, and 45 and 29 are the two numbers the run of 14:24
printed. So the probe is measuring the same thing the rule measures.

**WHAT THE 29 ACTUALLY CARRY.** Every one of them has its size on the composite Revit
element, one level above the geometry, written as WORDS:

```
NO SIZE 1: BLD-EL-Conduits & Conduit Fittings-vs-BLD-ST-Floors  Clash11, service side [Conduits]
   level 0 []                          tabs Item, Geometry, TimeLiner      NONE of the six
   level 1 [Conduit without Fittings]  tabs Item, Element, ...             [Element] Size = 53 mmø <DisplayString>
   level 2 [ELE-CNF-70mm BARE COPPER]  tabs Item, Type, TimeLiner          NONE of the six
```

The three forms across the 29, read off the client's own models:

```
53 mmø                                                   a conduit diameter
600 mmx100 mm-600 mmx100 mm                              a cable tray fitting, one pair per connector
600 mmx100 mm-600 mmx100 mm-600 mmx100 mm-600 mmx100 mm  a four way fitting
```

**`ItemSizes.TryReadOne` TOOK ONLY TWO KINDS**, `DoubleLength` and `Double`, and a
`DisplayString` was dropped. Its own comment said a worded size was deliberately not
read because parsing "150 mm" would mean guessing at the unit. THAT REASONING WAS WRONG
ON THIS DATA: the text names its own unit, every time, so nothing has to be guessed.

**THE FIX.** `Federator.Core.Views.SizeText` reads every number in the text that is
FOLLOWED BY A UNIT this tool knows, through `UnitTable`, which is the one unit table in
this repo, and `UnitTable.FindByShortLabel` and `ShortLabels` are the two lookups added
for it. `ItemSizes` calls it for a `DisplayString` and puts the answer back into document
units, so every value in that dictionary still means the same thing and `SizeRule` stays
the one place that converts to millimetres.

Two things the client's own strings taught it, and both have tests:

- **A NUMBER WITH NO UNIT IS REFUSED AND NEVER GUESSED AT.** `300x300` carries no unit,
  so whether it is millimetres or inches is UNKNOWN. The penetration rule LEAVES ALONE a
  service it cannot measure, so refusing costs a clash a person looks at and guessing
  costs a hole in a wall nobody checked
- **`x` IS A DIMENSION SEPARATOR AND NOT A LETTER.** The first version read `600 mmx100 mm`
  as no measurement at all, because it refused a unit with a letter after it so that
  "minutes" could not be read as miles. No unit this tool knows contains an x, so the
  separator is the one exception and the guard still holds for every real word

**WHAT THIS MEANS FOR THE COUNT.** The 29 are not 29 services with no size. They are 18
conduits, 10 cable tray fittings and 1 pipe fitting whose size this tool could not read
until today. PART 5 reports what is left after the fix, on a run.


## 5t. EVERY WORKSET NAME IN C02, MEASURED 2026-09-20

PART 1b. The same probe pass, over every model of all ten groups. **39 distinct names.**
The full list and which models carry each is in the result file.

**TWO NAMES DIFFER FROM ANOTHER ONLY BY CASE**, which a correction rule can fix because
no judgement is involved:

```
EL-Fire Alarm       1A02MM EL          against  EL-Fire alarm     1A02WE, 1A02WM, 1A02WN, 1A02WO EL
EV-Access Control   1A02MM, 1A02BS x2, 1A02WM   against  EV-Access control   1A02WN EL
```

**FIVE PAIRS DIFFER BY MORE THAN CASE, AND ONLY THREE OF THE FIVE ARE TYPOS.** This is
the part that decides the shape of the rule, because an edit distance alone cannot tell
them apart:

```
EL-Lightining Protection  against  EL-Lightning Protection   ONE letter. A typo
EV-Ccctv system           against  EV-Cctv System            ONE letter. A typo
PL-Drainage equipmen      against  PL-Drainage equipment     ONE letter. A truncation
AR-EXTERIOR               against  AR-INTERIOR               TWO letters. TWO REAL WORKSETS
ST-SUB                    against  ST-SUP                    ONE letter. TWO REAL WORKSETS
```

Sub structure against super structure, and exterior against interior, are one letter
apart and both real. **SO NOTHING MAY BE MERGED ON DISTANCE ALONE**, and the rule this
round builds never does.

**AND THE MATRIX ASKS FOR ONLY SEVEN WORKSET VALUES**, which is what makes the rule safe:

```
PL-Drainage 6,  PL-Domestic Water 6,  ME-PIPING 5,  ME-DUCTWORK 5,
FP-PIPING 5,  FF-FIRE FIGHTING 2,  ME-EQUIPMENT 1
```

Matched against the 39 the models carry:

```
ME-DUCTWORK       -> ME-Ductwork        one candidate, case only.  CORRECT
ME-PIPING         -> ME-Piping          one candidate, case only.  CORRECT
ME-EQUIPMENT      -> ME-Equipment       one candidate, case only.  CORRECT
PL-Domestic Water -> PL-Domestic water  one candidate, case only.  CORRECT
PL-Drainage       -> PL-Drainage        exact.                     LEAVE ALONE
FP-PIPING         -> nothing            no model carries FP-.      LEAVE ALONE
FF-FIRE FIGHTING  -> nothing            no candidate.              LEAVE ALONE
```

Four corrections, covering 17 of the matrix's conditions. `AR-EXTERIOR`, `ST-SUB` and
every other name the models carry are never touched, because the rule only ever looks at
a value the matrix asks for.

**AND THE FIVE DISAGREEING PAIRS AFFECT NO CLASH SET AT ALL.** Not one of
`EL-Fire Alarm`, `EV-Access Control`, `EL-Lightning Protection`, `EV-Cctv System` or
`PL-Drainage equipment` is a value any set in the matrix filters on. So the Or row Q69
asks for is built and correctly produces NOTHING on this matrix, and the thing that does
the work is the export check naming them. They are a model hygiene problem and not a
clash problem, and saying so is more use than an Or row that changes no number.


## 5u. THE SHARED SITE OF EVERY MODEL IN EVERY GROUP, MEASURED 2026-09-20

PART 1c, and PART 4 was not built until this number was known.

```
1A02MM   reference AR-MOD-000001   names Internal: TRUE    no site at all: false
1A02WL   reference none            names Internal: TRUE    no site at all: false
1000BS   reference AR-MOD-003001   names Internal: false   no site at all: false
1A0215   reference none            names Internal: false   no site at all: false
1A02BS   reference AR-MOD-001000   names Internal: false   no site at all: false
1A02MS   reference none            names Internal: false   no site at all: false
1A02WE   reference AR-MOD-000001   names Internal: false   no site at all: false
1A02WM   reference AR-MOD-000001   names Internal: false   no site at all: false
1A02WN   reference AR-MOD-000001   names Internal: false   no site at all: false
1A02WO   reference AR-MOD-000001   names Internal: false   no site at all: false
```

**TWO OF THE TEN GROUPS WOULD FAIL UNDER Q70**, 1A02MM and 1A02WL, and NO model in C02
carries no site at all. Two is well under half, so PART 4 is built exactly as briefed
and the number is not uncomfortable enough to be worth arguing about.

**FOUR OF THE TEN CARRY NO ARCHITECTURE MODEL** and fall back to their first placed
model as the alignment reference, which the block already says in words. 1A0215 is
landscape, 1A02MS is one model, and 1A02WL is eight structural and mechanical models
with no architecture at all.

An eleventh file, `1104-PAR-1A0215-ZZZ-LS-MOD-000001.nwf`, opens with ZERO models. It is
in his NWF folder and is not one of the ten groups the run builds, and nothing in this
round touches it.

## 5v. CAN A SET BE REPLACED WITHOUT LOSING WHAT POINTS AT IT, MEASURED 2026-09-20

PART 1a of the drift round, and Q72's tick box was not written until this was read. The
probe is `tools\probes\ViewpointProbe` in its `drift` mode. The result file is
`tools\probes\ViewpointProbe\5v-result-20260920.txt`.

**THE ANSWER IS YES, ON ALL FIVE COUNTS, AND IT SURVIVES THE DISK.**
`DocumentSelectionSets.ReplaceWithCopy(GroupItem, int, SavedItem)`, the route 4d named:

```
the set measured: [BLD-EL-Lighting Fixtures] at index 5 under [Electrical]
chosen because the test [BLD-EL-Lighting Fixtures-vs-BLD-ST-Floors] holds 36 result(s)
  and its first side points at it
BEFORE the replace: 36 result(s), 1 at Reviewed

REOPENED OFF THE DISK:
   1. the clash test still points at a set : True, at [BLD-EL-Lighting Fixtures], WHICH IS THE RIGHT ONE
   2. the clash test still holds results   : 36 against 36 before, KEPT
   3. the Reviewed status survived         : 1 against 1 before, KEPT
   4. what the set asks now                : the NEW question, so the replace took
   5. the set is still in the tree         : True, index 5 under [Electrical], unmoved
```

**TWO THINGS THE FIRST ATTEMPT GOT WRONG, and both are worth recording because both are
the shape that makes a measurement worthless.**

The first pass replaced the first set in the tree, `BLD-AR-Floors`, and read back the
first test that had results, which pointed at `BLD-EL-Lighting Fixtures`. All five counts
came back clean AND THE MEASUREMENT PROVED NOTHING, because the test it watched was not
the test pointing at the set it replaced. The probe now finds a set that a test WITH
RESULTS actually points at, and replaces that one.

The first pass also threw `ArgumentException: Argument 'parent' has been Disposed`,
because the walk that found the parent folder disposed it on the way out and handed back
a dead wrapper. The parent is resolved from an INDEX PATH now, plain ints out and a fresh
wrapper in, which is the shape the viewpoint writer already uses for the same reason.

**WHAT THIS DECIDES.** The tick box Q72 asked for can rebuild a drifted set WITHOUT
refusing, because nothing that points at it is lost. The brief's fallback, a tick box
that refuses a set whose test holds results, is NOT needed and is not built.


## 5w. WHAT EVERY SET IN AN NWF IS ACTUALLY ASKING, MEASURED 2026-09-20

PART 1b. `SelectionSet.Search` is a getter and nothing in this tool had ever read it.
Read for every set in all ten of his groups. **Every one read: 0 unreadable anywhere.**

A set's question comes back in the shape `SetBuildPlan.Describe` already writes, so what
the SET asks and what the FILE asks can be put beside each other:

```
/Architecture/BLD-AR-Floors   asks
  LcRevitData_Element (Element)/LcRevitPropertyElementCategory (Category) Equal "Floors"
  and LcOaNodeSourceFile (Source File) DisplayStringContains "-AR-"
```

**SEVEN OF HIS TEN GROUPS CARRY 62 SETS WHERE THE MATRIX HOLDS 61, AND THE EXTRA ONE IS
A DUPLICATE.** 1A02MM's Mechanical-Drainage folder holds BOTH:

```
BLD-DRPipe Accessories    asks Category Equal "Pipe Accessories" and Workset Equal "PL-Drainage"   [None]
BLD-DR-Pipe Accessories   the same question                                                        [IgnoreDisplayNames]
```

The first is the name F87 corrected, still sitting in his file. The second is the one
this tool created later, because the corrected name was not there to find. F28 leaves a
set already at its path alone, so neither was ever touched again, and the clash tests
created before the correction still point at the broken one. THIS IS Q72 VISIBLE IN A
FILE and it is in 1A0215, 1A02MM, 1A02WE, 1A02WL, 1A02WM, 1A02WN and 1A02WO. The three
that are clean, 1000BS, 1A02BS and 1A02MS, are the three whose NWF was first built AFTER
the correction.

**AND THE CONDITION FLAGS SAY WHERE EACH SET CAME FROM.** Counted across every set of
every group:

```
1A02MM     61 conditions read [None]   and 1 reads [IgnoreDisplayNames]
every other group   0 read [None]      and 61 or 62 read [IgnoreDisplayNames]
```

`SetBuilder.BuildCondition` always adds `IgnoreCategoryDisplayName` and
`IgnorePropertyDisplayName`. So a set reading `[None]` was NOT built by this tool.
1A02MM's 61 sets are the ORIGINAL import, carried in the NWF from before this tool ever
ran on it, and the one exception is the single set the tool had to create. Every other
group's sets are this tool's own.

That is worth knowing beside Bader's own reading of his 1A02MM report, where 1,677 of
1,830 tests touch a set that finds nothing. It is not proof that the flags are the cause:
the `[None]` sets in that file DO find items, `BLD-AR-Floors` finds 4 and
`BLD-EL-Lighting Fixtures` finds 49. It is proof that 1A02MM's sets have a different
history from every other group's, which is the first thing to know about them.


## 5x. WHAT TAKING ONE MODEL OUT OF AN OPEN DOCUMENT COSTS, MEASURED 2026-09-20

PART 1c, and it closes Q34, open since 2026-09-18. 5c measured that
`Document.RemoveFile(int)` and `TryRemoveFile(int)` exist and scan.md said in as many
words that what they do to what points INTO that model was UNKNOWN.

**IT COSTS NOTHING.** On 1A02MM, four models, 526 results and 509 viewpoints, removing
the last model:

```
                              models  sets  tests  results  statuses  viewpoints
BEFORE the remove                  4    62   1830      526       526         509
AFTER the remove                   3    62   1830      526       526         509
AFTER a save and a reopen          3    62   1830      526       526         509
```

`TryRemoveFile` returned true, the model went, and the sets, the clash tests, every
clash result, every status a person set and every saved viewpoint came back whole,
through the disk.

**WHAT THIS DECIDES.** A CHANGED group does not have to clear and rebuild. It can append
what is missing and remove what is gone, and F50's count out and count back stays
exactly as it is, because that is what proves nothing was lost. Q34 is answered.

WHAT IS NOT MEASURED, said rather than left open: this removed the LAST model of four.
Whether removing a middle model shifts the indexes of the ones after it, and whether a
viewpoint's index path survives that shift, is UNKNOWN and is why PART 5 removes by
reading the file name back rather than trusting an index.


## 5y. WHAT SETTING A TOLERANCE ON A SAVED TEST ACTUALLY COSTS, MEASURED 2026-09-20

PART 1d. `ClashRunner` says in four places that setting a tolerance on a saved test
RESETS its results, and `FederatorWindow` says so in capitals on the confirm screen:
"YES, which RESETS the results of every test it changes".

**IT RESETS NOTHING.** Measured both ways on 1A02MM, on a test holding 36 results all of
which carry a status a person set:

```
                            tolerance  results  statuses  test status
at the start                    0.082       36        36          Old
CASE ONE, set to 0.082          0.082       36        36          Old
  after a save and a reopen     0.082       36        36          Old
CASE TWO, set to 0.164          0.164       36        36          Old
  after a save and a reopen     0.164       36        36          Old
```

Not the same value, not a different one. The results stay, the statuses stay, and both
survive the disk.

**WHAT IT HAS ALREADY COST HIM, counted off his own logs**, both
`%LOCALAPPDATA%\ParsonsNwcFederator\logs` and `steps\logs`, 16 logs, 140 lines:

```
saved tests that had a chosen tolerance set on them, across every run : 175,434
per run, over the last twelve runs                                    :  12,531
```

**NONE OF IT COST HIM ANYTHING**, and he has been told on every confirm screen, in
capitals, that it reset the results of all of them.

**WHAT THE WARNING SHOULD HAVE SAID.** Re-running a clash test replaces its results,
which is what running a test does whatever its tolerance, and a test whose tolerance
changed will find different clashes when it next runs. Neither of those is the tolerance
resetting anything, and the difference matters because the warning as written has been
making a person hesitate over an action that is free.


## 5z. WHAT REMOVING A SET COSTS, MEASURED 2026-09-20

PART 1a of the close round. 5v measured REPLACING a set's conditions and found everything
that points at it survives, and the drift round's tick box was built on that. REMOVING IS
NOT THE SAME THING and nothing had measured it. Nothing in `src` has ever called `Remove`,
`RemoveAt`, `Clear` or `Move` on `DocumentSelectionSets`.

Against a COPY of 1A02WE under the temp folder, never his own file.

**FIRST, THE MEMBERS, BECAUSE THE RECORD WAS INCOMPLETE.** scan.md quoted only the single
argument forms `Remove(SavedItem)` and `RemoveAt(Int32)` from a reflection dump, and named
the parent scoped forms only in prose about a DIFFERENT collection, so they were asserted
and not measured. Read off the installed 2025 DLL:

```
Collection`1 CreateIndexPath(SavedItem)
SavedItem   ResolveIndexPath(IEnumerable`1)
Void        RemoveAt(GroupItem, Int32)
Void        RemoveAt(Int32)
Boolean     Remove(GroupItem, SavedItem)
Boolean     Remove(SavedItem)
Void        Move(GroupItem, Int32, GroupItem, Int32)
Void        Move(Int32, Int32)
Void        Clear()
```

All of them are there, the parent scoped pair included, and `Move` is there too where no
dump anywhere had recorded it for this collection.

**AND THE ONE ARGUMENT FORM FAILS QUIETLY ON A NESTED SET.** `Remove(SavedItem)` against
`BLD-AR-Windows`, which sits under the `Architecture` folder, returned **False** and threw
nothing. It addresses the root collection. A caller reads a false back as "there was
nothing to remove" rather than as "you asked the wrong overload", which is the shape of
failure this repo keeps finding. `RemoveAt(parent, 7)` on a parent resolved fresh worked.

**WHAT IT COSTS.** `BLD-AR-Windows` removed from index 7 of a folder of 16, through a
save, a close and a reopen off the disk:

```
                          models  sets  tests  results  statuses  viewpoints
BEFORE the remove              4    62   1830       29        29          52
AFTER the remove               4    61   1830       29        29          52
AFTER a save and a reopen      4    61   1830       29        29          52
```

1. the clash test still EXISTS and keeps its identity
2. its results: **3 against 3, KEPT**
3. its Reviewed statuses: **3 against 3, KEPT**
4. the saved viewpoints: **52 against 52, KEPT**
5. the removed set is gone from the tree, and the fifteen left keep their identities

**THE MIDDLE CASE, WHICH 5x LEFT UNKNOWN FOR MODELS.** The eight sets behind the removal
all SHIFTED UP BY ONE, 8 to 7 through 15 to 14. So an index remembered across a removal
addresses a different set afterwards, which is why anything removing more than one goes
from the END or works by name.

**AND THE THING THAT ACTUALLY COSTS SOMETHING.** 60 clash tests now have ONE SIDE THAT
RESOLVES TO NOTHING, out of 1830. Not one has both sides dangling. Those 60 tests still
exist, still hold their results and still hold the status a person set on them, and they
can never find anything again.

```
tests whose two sides both resolve to a set : 1770
tests with ONE side resolving to nothing    : 60
tests with BOTH sides resolving to nothing  : 0
```

So a removal does not destroy review history. It orphans tests, silently, and the number
is 60 per set in a 61 set matrix because every set pairs with every other.

**WHAT POINTS AT WHAT, READ OFF HIS OWN SEVEN GROUPS.** Read only, nothing saved. In each
of the seven C02 groups that carry 1830 tests, **60 test sides point at
`BLD-DRPipe Accessories`**, the spelling with the missing hyphen, and **nothing at all
points at `BLD-DR-Pipe Accessories`**, the corrected spelling this tool created. 60 sides
point at `BLD-Security Devices` in the same groups, and 14 in 1A02BS.

THAT INVERTS WHAT REMOVAL WOULD DO. The broken name is the set doing the work. The
corrected name is the set sitting unused. Removing what the picked file no longer names
means removing the broken one, which orphans 420 test sides across seven groups. Removing
the unused duplicate changes nothing at all.


## 5z-b. DOES RENAMING A SET KEEP WHAT POINTS AT IT, MEASURED 2026-09-21

PART 1a of the close round, and PART 2 was blocked on it. THREE DIFFERENT FIELDS, THREE
DIFFERENT MEASUREMENTS: 5v changed a set's CONDITIONS, 5z REMOVED a set, and a DISPLAY
NAME is a third field that neither answers. A rename might be a label a `SelectionSource`
never notices, or a replace underneath, and which it is decides whether PART 2 can be
built at all.

Against a COPY of 1A02WE under the temp folder. `BLD-AR-Windows`, index 7 of 16 under
`Architecture`, the middle case, with **60 test sides pointing at it** before the rename.

The route is `EditDisplayName(SavedItem, String)`, the only rename member on the
collection, on an item resolved fresh at the moment of the call.

**IT IS A CLEAN YES ON ALL FIVE**, through a save, a close and a reopen off the disk:

```
                          models  sets  tests  results  statuses  viewpoints
BEFORE the rename              4    62   1830       29        29          52
AFTER a save and a reopen      4    62   1830       29        29          52
```

1. the test side still resolves, **to the right set**, and every one of the **60 sides
   followed the rename**. Sides still pointing at the old name: **0**
2. results: **3 against 3, KEPT**
3. Reviewed statuses: **3 against 3, KEPT**
4. saved viewpoints: **52 against 52, KEPT**
5. the set keeps its conditions unchanged, finds the same 1 item, and sits at **index 7
   under `Architecture`**, exactly where it was. Its folder still holds 16

And across the whole document afterwards, **1830 tests with both sides resolving and 0
dangling**, against the 60 dangling that 5z's removal produced on the same file.

**WHAT THIS DECIDES.** A rename is the right operation and a removal is the wrong one for
a set that is carrying tests. The clash test's `SelectionSource` follows the set through a
name change, so renaming the broken spelling to the corrected one keeps all 60 sides
working AND makes them ask the right question, where removing it would have orphaned them.
The unused duplicate can then be removed, which 5z already proved costs nothing because
nothing points at it.


## 5z-c. THE DISCIPLINE CODE IS UNIFORMLY UPPERCASE, MEASURED 2026-09-21

Recorded because a round planned a rule on the opposite belief and the belief was wrong.

The close round's first reading of C04 reported
`1104-PAR-1A04WO-ZZZ-El-MOD-000001.nwc` carrying a lowercase L, and planned a case blind
discipline compare on it. **THE FILE IS `EL`.** Read off the bytes with `od -c`: `E L`. A
case sensitive count over the folder gives **7 matches for `-EL-` and 0 for `-El-`**, and
the file's modification time is 23:31:37 on 2026-09-20 and has not moved, so nothing was
renamed underneath the reading. It was simply wrong, and an adversarial check that listed
the folder itself rather than trusting the reading is what caught it.

MEASURED ACROSS BOTH FOLDERS ON 2026-09-21: every discipline code in all 46 C04 file names
and in every C02 file name is UPPERCASE. C04 carries seven distinct codes, AR EL LS LT ME
ST SW, and not one lowercase or mixed case code exists in either folder.

**SO NO CASE BLIND COMPARE IS BUILT**, because a rule for a case that exists in none of
the files is the same shape as a public member nothing calls, and this repo deletes those.

**AND THE LOWERCASE `El` IS REAL, ONE LEVEL AWAY.** It is in the REVIT SOURCE NAME in
Autodesk Docs, not in any NWC name. `steps\logs\run-20260919-211323.log` carries it:

```
holds  ...\1104-PAR-1A02WO-ZZZ-EL-MOD-000001.nwc
[source Autodesk Docs://KSA_New Murabba/1104-PAR-1A02WO-ZZZ-El-MOD-000001.rvt
```

It reaches NOTHING this tool compares. `SourceMismatchFindings` and the shared source rule
both compare BUILDING CODES only and never the discipline, so the case never bites. Model
hygiene, worth Bader knowing, and nothing is built for it.

**IF A LOWERCASE CODE EVER DOES ARRIVE IN AN NWC NAME**, the reasoning is already done and
it is the opposite of Q68's. A DISCIPLINE CODE IS A CLOSED SET of seven values this project
defines, so matching it case blind cannot merge two things that were never the same and
would be safe. A WORKSET VALUE IS AN OPEN SET that anybody can add to, which is why Q68
refused the ignore case flag there: `AR-EXTERIOR` against `AR-INTERIOR` and `ST-SUB`
against `ST-SUP` are two pairs of real worksets one and two letters apart. The two cases
look alike and the answers are opposite, which is exactly why this is written down now
rather than re-argued later.


## 5z-d. CAN A SCRIPT START NAVISWORKS WITH NO CLICK AND CLOSE ONLY WHAT IT STARTED, MEASURED 2026-09-28

F100, Phase 1 item 1 of the loop, on Bader's Parsons machine. The first measurement of the
automation API on this machine. 5j used the same API on the machine of 2026-09-19, and how
it was started there was never recorded.

THE QUESTION. Can a PowerShell script start Navisworks Manage 2025 with no click through
`Autodesk.Navisworks.Api.Automation`, know the process id of the Navisworks it started,
open a copy of one NWC, load a plugin assembly with `AddPluginAssembly`, and quit? If it
can, `tools\loop\run.ps1` drives each run through this API with `ExecuteAddInPlugin`, and
the no-click entry is an AddInPlugin. If it cannot, run.ps1 starts Roamer.exe itself and
drives the window.

THREE RUNS, ONE KEPT.

- RUN 1 at 12:27, with the probe committed in 464f79f. Its result file was replaced. Its
  work folder is kept at `%LOCALAPPDATA%\NwcFederatorLoop\probes\automation-start-run1-20260928-1227`,
  and it holds the only registry export taken before any run. After it, BY HAND AND NOT BY
  THE PROBE, the Recent File List, MainWindow and PluginOptions keys were put back from
  that export with `reg import`, a write outside the loop folder. That probe compiled its
  window reader with Add-Type, and what the compiler wrote under %TEMP% was not recorded
- The reviewer and the breaker read run 1, and the probe was rewritten from their list:
  the ownership rule below, the settings backup and put back, and every claim read off
  the result
- RUN 2 at 13:11, with the rewrite, passed all six steps and showed two faults in the
  probe. Its folder, its result and the probe as it ran are kept at
  `%LOCALAPPDATA%\NwcFederatorLoop\probes\automation-start-run2-20260928-1311`, and the
  lines below are of `automation-start-result-run2.txt` there. It tried to copy its backup
  over `CommCenter\en-US\InfoCenter.log`, whose content it could not read at the end, and
  Windows refused the write as a sharing violation, lines 549 to 551, so nothing was
  written. And two watchdog passes threw "Cannot convert null to type System.DateTime",
  lines 441 and 459. Read off the probe as it ran, kept beside that result, each throw was
  one new process whose start time read as nothing, marked seen before the throw, so it
  was not recorded at that pass. A process that is not a Roamer was never recorded after
  that. A Roamer would be recorded again once its start time read, because its key held
  the start time. So up to two processes went unrecorded, and which is UNKNOWN. Both
  faults were fixed
- RUN 3 at 13:17 is `tools\probes\automation-start-result-20260928.txt`, the output of one
  run of the probe as it was then, untouched but for three lines masked on 2026-09-29 by
  F102's tools\loop\mask-evidence.ps1, pull request 76: the machine name on line 1 and the licensing agent's ids on
  lines 446 and 447. Its line 3 carries the sha256 of that probe,
  38C28E1C3C2756252523E4745FC5C79285E53CEBD6452AB85090117A4EC66EA5
- FIX LIST 2, later on 2026-09-28, changed the probe again, with the lead's decisions D1 to
  D4. Nothing is closed before adoption, and a start the probe cannot prove is written to
  `%LOCALAPPDATA%\NwcFederatorLoop\probes\unproved-starts.txt` for later runs to refuse on.
  Settings are put back only when no other Navisworks ran. The work folder is renamed,
  never emptied. THE CHANGED PROBE HAS NOT RUN. Under D4 it runs only when no Navisworks
  the loop did not start is running, and Bader's pid 34668 was running, so its run waits.
  Nothing below measures the changed probe
- FIX LIST 3, the third and last fix attempt, changed the probe once more with the lead's
  decisions E1 to E8. Its `-ReflectionOnly` output, which starts nothing, is
  `tools\probes\automation-start-reflection-20260928.txt`, and its line 3 carries that
  probe's sha256, B122822429D8F97D60DD2BF9E8D152905AAE5FA2ECEEAFCFD63E41BA974573D8. Lines
  below marked REFLECTION are of that file. Its full run still waits for 34668, which was
  running at 14:36:37
- F100 went to the form after three fix attempts. Bader answered A on 2026-09-29, Q79, and
  closed 34668. FIX ATTEMPT 4, commit 0a89d73, fixed the seven faults of the fourth reading
  and put the rule that no start is made while any Navisworks runs into code. A reviewer
  and a breaker read it and both approved, finding no fault of the seven left and no new
  fault inside attempt 4
- RUN 4 at 11:35 on 2026-09-29 is `tools\probes\automation-start-result-20260929.txt`, the
  output of one run of the attempt 4 probe, sha256 9CC1987B on its line 3, with the machine
  name masked on line 1 and nothing else changed. It is set out under RUN 4 below. Run 3's
  file is kept with the licensing agent's two ids and the machine name masked, every line
  where it was

Every claim below names the line of run 3's result behind it, or reads UNKNOWN, except under
RUN 4, whose lines are of run 4's result.

**RUN 4, THE ATTEMPT 4 PROBE, ALL SIX STEPS PASSED**, lines 551 to 556, 125 s in all.

```
                                                                              result line
step 2  no start recorded as unproved, no Roamer running                      396, 397
        the settings backed up: the 22.0 key, 303 keys and 1243 values, and
        9 files, 196 AutoSave files listed and not copied                     401 to 403
        the last read before the constructor, no Roamer running               406, 407
step 3  the constructor RETURNED after 83.16 s                                411
        one new Roamer, pid 33752, parent 1804 svchost.exe, -Embedding        413, 414
        adopted, all four conditions True                                     415, 416
        it started 0.06 s after the call began                                417
        Visible read False, set True read True, set False read False          420, 421, 424
step 4  OpenFile of the copy RETURNED after 1.73 s                            431
        SaveFile wrote 29765 bytes, after the call began                      434, 435
step 5  AddPluginAssembly RETURNED after 0.006 s                              442
        ExecuteAddInPlugin NOT called                                         444
step 6  Dispose RETURNED after 0.40 s                                         448
        pid 33752 gone 8.5 s after Dispose returned, not forced               449
the watchdog forced nothing, 138 passes, longest gap 10960 ms, no errors      453, 454
settings: no other Navisworks ran, 36 registry values put back, 0 keys
        made again, 0 failed, and 2 files put back reading their backup's
        sha256, each write after a last check                                 494, 531, 532, 536 to 538
AutoSave files added, changed, gone or unreadable: 0                          539
```

Run 4 agrees with runs 1 to 3 on every number they share: a constructor of 82.75 to 110.02 s
against 83.16 s, and a close by Dispose 8.0 to 9.3 s after it returned against 8.5 s. It is
the first run whose put back was made with no other Navisworks running, so it is the first
that measures D2 as written. It printed NO LICENSING ID: attempt 4 prints no command line of
a process that is not a Roamer. The two licensing agents the Roamer started, pids 40156 and
15536, are named by pid and parent only, lines 462 and 463. The result reads 15536 exited,
line 478, and whether 40156 had exited by the end UNKNOWN, line 477. A Get-Process by the
lead after the run did not list it, and that read is kept in no file.

HOW. `tools\probes\probe-automation-start.ps1`, Windows PowerShell 5.1, 64 bit, STA:

```
powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\probe-automation-start.ps1 -Out tools\probes\automation-start-result-20260928.txt
```

How the add-in it loads was built is not in the result. The result carries only its stamp
and its write time, line 424: `1.0.0.0 2cc8045c+edits built 2026-09-28 12:14:32`.
`+edits` says the working tree held a change when it was built, and not which change.

**THE ANSWER IS YES. ALL SIX STEPS PASSED**, lines 563 to 569.

```
                                                                              result line
step 2  Roamer 34668, started 09:33:44, command line holds -Embedding
        or -Automation: no                                                    382
step 3  the constructor RETURNED after 82.75 s                                395
        one new Roamer, pid 50204, parent id 1328 read as svchost.exe         397
        command line "...\Roamer.exe" -Embedding                              398
        adopted, all four conditions True                                     399, 400
        Visible read False, set True read True, set False read False          404, 405, 408
step 4  OpenFile of the copy RETURNED after 1.65 s                            415
        SaveFile wrote 29762 bytes, after the call began                      419
step 5  AddPluginAssembly RETURNED after 0.008 s, no exception                426
step 6  Dispose RETURNED after 0.29 s                                         432
        pid 50204 gone 8.0 s later, not forced                                433
end     pid 34668 state same, never touched                                   471
```

1. STEP 1, THE DLL, 184,088 bytes, 22.0.0.0, PE32Plus AMD64, lines 6 to 9. Two exception
   types, `AutomationException` and `AutomationDocumentFileException`, and one class that
   does anything, `NavisworksApplication`, IDisposable, lines 32 to 68:

   ```
   public NavisworksApplication()
   public System.Boolean Visible { get; set }
   public System.Void AddPluginAssembly(System.String fileName)
   public System.Void AppendFile(System.String fileName)
   public System.Void CreateCache(System.String fileNameToCache)
   public System.Void DisableProgress()
   public System.Void Dispose()
   public System.Void EnableProgress()
   public System.Int32 ExecuteAddInPlugin(System.String pluginId, params System.String[] parameters)
   public static NavisworksApplication GetRunningInstance()
   public System.Void OpenFile(System.String fileName, params System.String[] moreFiles)
   public System.Void Print(), and three more taking printer, driver and port
   public System.Void SaveFile(System.String fileName)
   public System.Void StayOpen()
   public static NavisworksApplication TryGetRunningInstance()
   ```

   The other public types are C++ runtime structs with no members.

2. WHICH BRANCH THE CONSTRUCTOR TAKES, off the IL. The public constructor pushes
   `ldc.i4.0`, false, into `Init`, line 140, and `GetRunningInstance` pushes `ldc.i4.1`,
   line 144. In `Init`, `ldarg.1` then `brfalse.s IL_0048`, lines 226 and 227, and
   IL_0048 loads "Failed to startup Navisworks" and calls the native
   `Bridge.StartupNavisworks`, lines 234 and 237. True falls through to
   `Bridge.GetRunningNaviswork`, line 231. So the public constructor takes the
   StartupNavisworks branch. What that native call does is not IL and is UNKNOWN beyond
   what follows

3. WHICH COM CLASS AND WHO SERVES IT. The DLL carries the 16 bytes of
   `{81920959-1e7e-5599-a1d1-e67aeced44df}` at offsets 49920 and 49952, line 332. Run 3
   searched the file for import NAMES, lines 333 to 337, and printed GetActiveObject as
   absent. That search cannot see an import made by ordinal. The DLL's import table,
   REFLECTION lines 340, 341, 345 and 346, shows `CoCreateInstance` imported by name from
   ole32.dll, AND `GetActiveObject` imported from OLEAUT32.dll as ordinal 35, named off
   OLEAUT32's own export table. That CLSID's ProgID is `Navisworks.Document.22`
   and its LocalServer32 is Roamer.exe, lines 328 and 330. In `CommandLineParser`,
   `ParseOption` stores `COMAutomationStartup` after matching `"Embedding"` or
   `"Automation"`, line 340, and that is the one store in CommandLineParser, line 359.
   In Roamer.exe, `NetRoamer.Program.MainImpl` hands it to
   `InitDll.InitialiseErrorHandling` as `is_automated`, line 363, and copies it into
   `InitialiseResourcesConfig.COMAutomationStartup`, line 367. THE READ COVERS
   CommandLineParser's methods and NetRoamer.Program's methods and nothing wider. What
   the native initialiser does with the field is UNKNOWN

4. WHY STEP 3 DID NOT REACH BADER'S NAVISWORKS ON THIS RUN. Pid 34668's command line holds
   neither option, read through Win32_Process, line 382, and the probe would have stopped
   before the constructor if it had held either. The one new Roamer came with `-Embedding`,
   and its parent id was held by svchost.exe when read, lines 397 and 398, which is the
   shape of a COM local server start. Run 3's probe named a parent without checking that
   it started before the child, so that svchost.exe was the parent is UNKNOWN.
   ON THIS RUN the constructor started a new Roamer and left 34668 alone. That it ALWAYS
   does is UNKNOWN. Runs 1 and 2 did the same. Run 1's result is in commit 464f79f and
   run 2's is in its kept folder

5. STEP 3, THE START. 82.75 s from the call to the return, line 395. Roamer started 0.04 s
   after the call and 82.71 s before the return, line 401. Two `AdskLicensingAgent` with
   `--no-gui` show pid 50204 as their parent, lines 446 and 447, and two
   `AdskLicensingInstHelper` show 26520 as theirs, read as `GenuineService.exe`, lines 445
   and 451. Run 3's probe named those parents off a read made before any check that the
   parent started before the child, so the names are what held those ids when read, and
   that they were the parents is UNKNOWN. All four had exited by the end, lines 465 to
   468. An untitled WinForms window of 50204 was visible from 13:18:36 to
   about 13:18:40, before Visible was touched, lines 448 and 449. Set to True, the main
   window `Untitled - Autodesk Navisworks Manage 2025` showed, lines 406 and 407, hid once
   and came back while the call setting Visible to True was running, lines 450 to 453, and
   went when Visible was set to False, lines 408, 409 and 454. From the read before setting
   Visible to the read back after setting it True took 20.8 s, lines 404 and 405, 1.5 s of
   it a sleep the probe makes before reading back.
   `Process.MainWindowHandle` read 0 every time it was read, even while the window showed,
   lines 402, 405 and 408. NO WINDOW OF CLASS #32770 WAS SEEN at any watchdog pass or any
   read of the main thread, lines 403 to 457. The watchdog made 170 passes, the longest
   8311 ms and the mean 217 ms, each followed by 500 ms of sleep, line 438, so a window
   shown for less than that can have been missed. The watchdog saw 50204 at 13:17:28.858
   with no readable start time, and at 13:17:29.403 with 13:17:20.475, lines 442 and 443.
   So a Roamer's start time can read as nothing at one pass and read at the next. Why is
   UNKNOWN. Adoption needs one that reads

6. THE PROCESS ID IS ADOPTED, NEVER GUESSED. Adopted only when all four hold: the
   constructor returned without throwing, exactly one Roamer is new since step 2, it
   started at or after the call, and its command line holds -Embedding. All four read True,
   line 399. Its parent id was held by svchost.exe when read, not by the script, line 397,
   and the main window handle reads 0, line 402, so neither can find it

7. STEP 4, THE OPEN. The smallest NWC in the source copy,
   `NWC\C06\1104-PAR-1B06PO-ZZZ-ST-MOD-000001.nwc`, 27,291 bytes, copied into the work
   folder with its sha256 matching, lines 412 and 413. `OpenFile` is void and returned
   after 1.65 s, line 415. What was open was saved with `SaveFile` to an NWD in the work
   folder, which was created at the start of that run, line 379, so it held nothing
   older: 29,762 bytes, written 13:19:13.466 after the call began at 13:19:13.442, line
   419. The copy's sha256 did not change, line 420. A
   window titled `Working...`, class `HwndWrapper[Roamer.exe;ProgressDialog;...]`, was
   visible during the open and during the save although Visible was False, lines 455 and
   457

8. STEP 5, THE PLUGIN. `AddPluginAssembly` with
   `src\Federator.Addin\bin\Release\net48\Federator.Addin.dll` returned after 0.008 s and
   threw nothing, line 426. It is void. `ExecuteAddInPlugin` was NOT called, line 428,
   because the tool's own plugin opens its window, so nothing here shows the assembly was
   loaded or its plugins registered

9. STEP 6, THE QUIT. `Dispose` returned after 0.29 s, line 432, and pid 50204 was gone
   8.0 s later without being forced, line 433. The finally found nothing to close, line
   436, and the watchdog forced nothing, line 437

**BADER'S SETTINGS, BACKED UP, COMPARED AND PUT BACK BY THE PROBE.** Before the start it
exported `HKCU\Software\Autodesk\Navisworks Manage\22.0` and read 303 keys and 1243 values,
lines 386 and 387, and copied 8 of the 9 files outside AutoSave under
`%APPDATA%\Autodesk\Navisworks Manage 2025`, line 390. The ninth,
`CommCenter\en-US\InfoCenter.log`, could not be read at the start, line 389, nor at the
end, and was not written, line 550. Its size and write time were the same at both ends.
After pid 50204 was gone, every value that differed was printed with its old and new
value and put back, lines 476 to 547:

```
                                        old                     new                     line
CER\22.5.1433.58  SessionStartCount     153                     154                     480
CER\22.5.1433.58  SessionCleanCloseCount 93                     94                      478
CER\22.5.1433.58  uptime, calUptime     both grew                                       476, 482
MainWindow  Placement                   "2,0,0,1632,969"        "0,-1920,-55,1632,877"  484
PluginOptions  DefaultPlugin            "LcOpNwcPlugin"         "lcodpody"              486
Recent File List 1                      his entry B5824C527A99  the probe's saved NWD   488 to 492
Recent File List 2                      his entry A0FAF7B3CDCD  the probe's NWC copy    500 to 504
Recent File List 3 to 10                each took the entry two above it                494 to 498, 506 to 546
```

The Recent File List paths print as their file name and a sha1 of the path, so where
each entry went can be followed without printing where it lives. His old entries 9 and
10, sha1 4C0D86461D9C and B1CCD34DBC72, are in no new entry, so they had fallen off the
list. 36 values or keys were put back, 0 failed, and 0 still differed when read again, line 548.
None of the 8 files backed up had changed content, line 551. AutoSave had no file added,
changed or gone, line 552. The tool's own logs folder had none, line 553.

WHO WROTE THOSE VALUES, AND WHEN, IS UNKNOWN. The probe compares before and after only,
and pid 34668 ran the whole time, lines 471 and 475. The Recent File List names the
probe's own NWD and NWC copy, which points at 50204, and that is still not a measurement of
the writer. Whether 34668 writes its own settings when it closes, over what was put back,
is UNKNOWN. Line 475 says so, and it is the probe's printed assumption, not a measurement.

RUN 3 PUT BACK WHILE 34668 WAS RUNNING, line 475, which the lead's D2 has since ruled out.
The probe cannot tell a change its own Navisworks made from one 34668 made, so a put back
with another Navisworks running can revert that one's change and log it as clean. Whether
run 3's put back reverted any change of 34668's is UNKNOWN. The changed probe writes
nothing unless no other Navisworks ran from the backup to the put back.

**NAMED LIMITS**, the lead's E8, said rather than fixed.

- A Navisworks that starts and exits inside one gap between two watchdog passes is not
  seen by the settings check. Run 3 did not print the gap. Its longest pass was 8311 ms,
  each followed by 500 ms of sleep, line 438, so its longest gap was at least 8811 ms. The
  changed probe prints the longest gap. A Navisworks start is measured at over ten
  seconds: run 3's constructor took 82.75 s, line 395, and its Navisworks process started
  0.04 s after the call and 82.71 s before the return, line 401
- The Automation DLL imports GetActiveObject, by ordinal, REFLECTION lines 341 and 346,
  and the IL of the public constructor passes false to Init, whose false branch calls
  StartupNavisworks, REFLECTION lines 141 and 227 to 238. That is all that is read of
  whether a Navisworks already running can be reached. Which native call StartupNavisworks
  makes is UNKNOWN
- When Dispose throws and the probe then closes the process by its id, the finalizer
  stays armed, and at the probe's exit it calls Bridge.Terminate, run 3 lines 272 to 286,
  against a process that is gone. What that does is UNKNOWN

**WHAT THIS DECIDES.**

- run.ps1 can start and quit Navisworks through this API with no click
- run.ps1 adopts its Navisworks by the four conditions of item 6 and by nothing weaker,
  never by parent and never by main window handle, and closes nothing it has not adopted,
  reading the start time again before any close. A start it cannot prove is never closed,
  it is written down, and later runs refuse while it runs, the lead's D1 and E3. It counts
  only new Roamers whose command line holds the word embedding, or cannot be read, and
  leaves one started by hand alone, the lead's E2. It sends no message to any window
  before adoption, the lead's E1
- the no-click entry can be an AddInPlugin called through
  `ExecuteAddInPlugin(string pluginId, params string[] parameters)`, which returns an int
  and exists on this machine, line 428. Whether it runs a plugin added with
  AddPluginAssembly here is the first thing the entry's own run shows
- run.ps1 backs up the settings named above around every run, and stops before its start
  when the backup is not whole, the lead's E5. It puts them back only when its watchdog's
  record proves no other Navisworks ran from the backup to the put back and its own is
  gone, the lead's D2 and E4, lists the Roamers again before each write and stops at any,
  writes only what still reads as the compare read, and never writes a file whose content
  it cannot read
- run.ps1 runs only when no Navisworks the loop did not start is running, the lead's D4

**STILL UNKNOWN.**

- whether the constructor ALWAYS starts a new Roamer. Three runs did, one kept
- whether a constructor reaches a Roamer already running with -Embedding or -Automation.
  The probe refuses before the constructor when one is, so it has never been tried
- whether any code outside CommandLineParser and NetRoamer.Program sets the automation
  start, and what the native initialiser does with it
- whether AddPluginAssembly loaded the assembly and registered its plugins
- whether ExecuteAddInPlugin runs a plugin added that way on this machine, and what its int
  means
- whether the installed bundle was loaded in the automation instance as well, and what
  happens when it and the added assembly carry the same plugin id
- what a start usually costs. The kept run took 82.75 s
- whether DisableProgress keeps the `Working...` window off the screen
- what the untitled window seen before Visible was touched is, and why the main window hid
  and came back once while Visible was being set to True
- whether a window shown for less than a watchdog pass and its sleep was missed
- whether the start works with the screen locked or nobody signed in
- what StayOpen does. It was not called
- who wrote each changed setting, and when, and whether run 3's put back reverted a change
  of 34668's
- everything about the changed probe of fix list 2. It has not run
- whether TerminateProcess on the probe's own process can return false. The probe says so
  once and stops its watchdog if it does, and that path has never run
- whether the constructor deadline, which ends the probe with no managed shutdown, leaves
  the Navisworks it could not prove running, and for how long
- what InfoCenter.log holds and whether 50204 wrote it. It could not be read at either end
- what `clash\rules`, `CommCenter\en-US\infocenter.xml` and `LastSession.xml` held before run
  1, which found them rewritten by write time and size. Nothing backed them up before run 1
- whether a constructor that throws leaves a finalizer that later reaches a Roamer. The
  probe cannot suppress the finalizer of an object it never received
- what Roamer.exe does with its own switches. Its parser matches 37 option names, line
  360, among them `NoGui`, `OpenFile`, `AddPluginAssembly`, `ExecuteAddInPlugin` and
  `Exit`, read off the IL and NOT RUN. They are the route if this one ever fails


## 5z-f. FOUR READS OFF THE INSTALL WITH NO NAVISWORKS STARTED, MEASURED 2026-09-29

F105, Phase 1 item 2 of the loop. Four questions answered off the files of Navisworks
Manage 2025, file version 22.5.1433.58, with NO Navisworks started. Nothing here ran
Roamer.exe, the automation API or any COM object, and no member found below was called.

HOW EACH FILE WAS READ, because not every probe read the same way:

- the two repo probes load `Autodesk.Navisworks.Api.dll` with `Assembly.LoadFrom`,
  `probe-viewpoints.ps1` line 20 and `probe-model-remove.ps1` line 21. LoadFrom loads an
  assembly to run, so for this mixed native and managed DLL its load code runs, and so
  does the load code of the native DLLs it needs. What that load code does is UNKNOWN.
  These two reads are NOT reflection only, and they ran as they are, last changed in
  76a181c on 2026-09-18
- the three new probes load every assembly with `ReflectionOnlyLoadFrom`, which runs no
  code in it, `probe-viewpoint-calls.ps1`, `probe-roamer-switches.ps1` and
  `probe-clash-report-api.ps1`. The second also reads Roamer.exe and
  navisworks.gui.roamer.dll as bytes, and the third reads all five DLLs as bytes for
  strings. All three dot-source `il-reader.ps1` beside them, the one copy of the IL reader
  and of the helpers they use, and read off their code they do not use the same parts of
  it. All three use TypeName, IlReason, IlFail and IlFailureLines. viewpoint-calls and
  roamer-switches, the two that read IL, use the IL reader IlRead and IlOpcodeLine,
  viewpoint-calls lines 362 and 399 and roamer-switches lines 434 and 676 among others.
  roamer-switches and clash-report-api use LoaderLines, lines 421 and 531 and lines 114
  and 181, and StringRuns, which matches the two string patterns, line 70 and line 164.
  viewpoint-calls uses neither LoaderLines nor the string patterns

WHAT A READ THAT FAILS DOES IN THE THREE NEW PROBES, read off their code and measured on
2026-10-01 with stand-in scripts, no Navisworks started:

- `il-reader.ps1` keeps a read that could not be made only when IlFail is handed it, by
  the probe or by IlRead and LoaderLines when the probe calls them, and keeps it by kind.
  A read handed to no IlFail is not kept. IlFailureLines prints every kind with its count,
  a kind nothing was handed included, so a 0 is a reading only of the reads that probe
  hands to IlFail
- each probe sets ErrorActionPreference to Stop, viewpoint-calls line 5 and the other two
  line 2, so a .NET method call that fails where no catch takes it, GetMethods or
  GetRawConstantValue for one, stops the probe. A stand-in that called GetRawConstantValue
  on a field that is not a constant stopped there with exit 1, and without Stop the same
  script went on to its last line. Each final run ran to its end, exit 0, run record lines
  70 to 72, and each result file ends on the line its probe prints last, viewpoint-calls
  line 297, roamer-switches line 1529 and clash-report-api line 607, so no such call
  failed in them
- a .NET property read that fails stops nothing and reaches no catch, even inside a try.
  Windows PowerShell 5.1 reads a property whose getter throws as null and goes on. A
  stand-in loaded System.Windows.Forms reflection only with no resolver and read the
  13,630 fields of the 1,896 types it could load. The 331 whose FieldType getter throws
  when called as a method each read as null through the property, none reached the catch
  when read inside a try the way viewpoint-calls line 350 reads one, and the script ran to
  its end with exit 0. So a failed FieldType, ReturnType, ParameterType or LocalType read
  is in no count in any of the three probes. It shows only where a probe prints the null, or
  stops the probe where a method is then called on the null outside a try, as
  probe-clash-report-api.ps1 does at lines 191 and 197, read off the code and not run by a
  stand-in. A failed read of a type's Assembly or its Location inside IsNw,
  probe-viewpoint-calls.ps1 line 305, is counted, because StartsWith is then called on the
  null inside the tries at lines 350, 355 to 358 and 365, which count it as a field, signature
  or locals failure
- the stand-ins and what they printed are kept outside the repo, in
  %LOCALAPPDATA%\NwcFederatorLoop\turn4\f105-read-failure

Each probe was run from Windows PowerShell 5.1.26100.9444, 64 bit, as

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -File <probe>
```

with its standard output kept as the result file, UTF-8 without a byte order mark, and
the machine name in its MACHINE line replaced with `[machine]`.

THE RUN RECORD, `f105-run-record-20260929.txt` beside the results. `Get-Process Roamer`
read 0 processes at every reading, from 12:07:54 to 13:26:27: in the first review round
at lines 4 and 5, 11 and 12, and 21 and 22, in the second fix round at lines 27 and 28,
33 and 34, and 41 and 42, and in the third at lines 47 and 48, 53 and 54, 61 and 62, 66
and 67, and 74 and 75. viewpoints-result and model-remove-result are the output of the
first round's final runs, lines 15 and 16. The other three are the output of the third
round's replacing runs, lines 70 to 72, which replace the second round's at lines 37 to
39 and the third round's first final runs at lines 57 to 59, whose failure lines printed
a false offset for the reason line 64 gives. Test runs came between readings, lines 6 to
9, 29 to 31 and 49 to 51, and their output was not kept. The runs of the morning, from
10:24, are replaced, and the Roamer readings made that morning are in no file.

```
probe                                  result file                             lines
tools\probes\probe-viewpoints.ps1      viewpoints-result-20260929.txt            388
tools\probes\probe-model-remove.ps1    model-remove-result-20260929.txt          134
probe-viewpoint-calls.ps1, new         viewpoint-calls-result-20260929.txt       297
probe-roamer-switches.ps1, new         roamer-switches-result-20260929.txt      1529
probe-clash-report-api.ps1, new        clash-report-api-result-20260929.txt      607

file                                   sha256 as it ran, run record lines 15, 16 and 69 to 72
tools\probes\probe-viewpoints.ps1      674527A738C91A41BDB9262182C78CD4715B0EB4765EF13EE80BC6F90DEE0F6F
tools\probes\probe-model-remove.ps1    B1986875DD5EE238E061AF301E60FE4CA8C8A6AED64407611DCA41C94D50F916
probe-viewpoint-calls.ps1              50A2D812BEB668460D49644DA9FCB56B096B496F1EE653F27C69A3D8E264FDCC
probe-roamer-switches.ps1              B9BB0211C9C8C96164799F985CA9D46FB86785659F5147D40B4E4E822DDF4ADA
probe-clash-report-api.ps1             706A74B6E92C20E95F55F5179168D165B05F0F788E00459F733F8706F18E661C
il-reader.ps1, dot-sourced by three    A89DD3A328D52342D2087345E4CEA9554058566556BD690FAB5E705C8D982F89
```

Each hash is of the file as it lay when it ran, the two repo probes with CRLF line ends
and the three new ones and the reader with LF, so a checkout that changes a file's line
ends changes its hash. The three probes that dot-source the reader print its hash in their
READER line, viewpoint-calls line 5, roamer-switches line 3 and clash-report-api line 2.
The three new probes, the reader, the five result files and the run record are in
tools\probes, committed with this section in F105. The add-in source compared against is
`src\Federator.Addin` at commit 30ae471.


**1. THE SAVED VIEWPOINT MEMBERS ON THIS INSTALL**

THE QUESTION. Does `probe-viewpoints.ps1` print on this install what 5d recorded, and is
every Navisworks member the add-in calls for saved viewpoints there with the shape it
calls? A member missing or different would change `SavedViewpoints.cs`.

`probe-viewpoints.ps1` never opens the two COM DLLs, and the add-in writes every clash
viewpoint through them, 5m. So `probe-viewpoint-calls.ps1` reads what
`src\Federator.Addin\Engine\SavedViewpoints.cs` calls, in `Autodesk.Navisworks.Api.dll`,
`Autodesk.Navisworks.ComApi.dll` and `Autodesk.Navisworks.Interop.ComApi.dll`, each by
its one full path, three ways. That file is where every hit of a search of
`src\Federator.Addin` for SavedViewpoint, CaptureRuntimeOverrides, ApplyHideAttribs and
InwOpView lands, apart from comments and calls into the add-in's own `SavedViewpoints`
class. Lines of viewpoint-calls-result.

- A, lines 7 to 91, is a LIST TYPED BY HAND from reading the file, 79 lines, each with one
  of three outcomes. FOUND is a member of that name AND the shape the file uses: its
  parameter types, its return or property type and the accessors it uses. Every method on
  it is checked by its parameter types, the two COM ones included, `ObjectFactory` and
  `InwSavedViewsColl.Add`, lines 77 and 86. DIFFERENT SHAPE is the name without that
  shape. NO MATCH is neither. Beside the members it checks that each of the nine types the
  file disposes implements IDisposable, and the five conversions the file relies on. It
  checks only what is on it
- B, lines 93 to 101, prints 5d's and 5c's generic members with their type arguments,
  which the two repo probes print by their bare names, `Collection`1`
- C, lines 103 to 226, is NOT typed by hand. It reads the IL, the locals, the fields and
  the signatures of the three classes compiled from `SavedViewpoints.cs`,
  `SavedViewpoints`, `ViewpointReadBack` and `HiddenSnapshot`, decodes the IL through
  `il-reader.ps1`, and resolves every reference to a Navisworks member or type against the
  install, which the runtime does by exact name and signature. The build read is the
  Release build in the main clone's own bin folder, 241,152 bytes, sha256
  57325D2629A1AA9B48C322D52106626B5311440F3D2D0464CC23E0B139F3BE94, stamped
  `1.0.0.0 30ae4715+edits built 2026-09-29 11:01:03`, lines 104 to 107

**EVERY MEMBER ON LIST A IS THERE WITH THE SHAPE THE FILE USES, AND EVERY NAVISWORKS
REFERENCE THE COMPILED FILE MAKES RESOLVES AGAINST THE INSTALL. NOTHING IS MISSING AND
NOTHING DIFFERS. EVERY MEMBER 5d RECORDS IS THERE WITH THE SAME SHAPE, ITS GENERIC
ARGUMENTS INCLUDED.**

```
list A      on the list 79, FOUND 79, DIFFERENT SHAPE 0, NO MATCH 0                    90
the IL      50 method bodies, 328 member and type tokens read                          109
            Navisworks members 65: 62 declared in an install assembly, all 62 on
            list A, and 3 declared on a framework generic over a Navisworks type       219
            Navisworks types 21                                                        220
            reads that failed 0, of the seven kinds this probe counts, each a
            count of the reads it hands to IlFail: IL bodies, opcode bytes,
            tokens, signatures, local variable lists, field types and constant
            values                                                                     204 to 206, 212 to 216, 220
            the other five print 0, and no line this probe can run, its own
            or il-reader.ps1's, counts a failure as one of those kinds, so
            their 0 is not a reading. They are MemberRef rows, a type's
            methods, a type's members, GetTypes and metadata                           207 to 211
            opcode table 191 one byte and 27 two byte instructions, the reserved
            bytes 0xF8 to 0xFD and 0xFF not among them                                 217
            every assembly from the install folder or the add-in's own folder         221 to 226
```

WHAT THE ZEROS OF THE IL SECTION LEAVE OUT, by what a read that fails does, above. Of the
five kinds it does not count, the probe makes some of those reads with no count, among
them a type's methods with GetMethods at its lines 130, 293, 352 and 420, and its
constructors, properties and fields with GetConstructors at 122 and 352, GetProperties at
148, 158 and 419 and GetFields at 349. It does not go through the MemberRef rows one by
one as roamer-switches does, calls no GetTypes and reads no metadata as bytes. Each of
those reads is a method call no catch takes, so a failure would have stopped the probe,
whose ErrorActionPreference is Stop at its line 5, and its final run ran to its end, exit
0, run record line 70. Of the kinds it counts, section C reads a type through a property
and prints nothing of it at four of its lines, a field's FieldType at 350, a return type
at 356, a parameter type at 357, and a body's LocalVariables and each local's LocalType
at 365. A failure there would reach no count and leave that reference out with nothing
printed. There is a fifth such line, 372, where a member reference's DeclaringType is read
through a property, and a failed read there would drop that reference out of the RESOLVED
list the same way. No stand-in called that getter, so whether it fails there is UNKNOWN. A
stand-in kept with the others called each getter of the four lines as a method on
2026-10-01, on the same build, its sha256 the one at result line 105, and the same three
classes, with the probe's own loading and resolver. It found none that throws: 8
FieldType, 48 ReturnType, 88 ParameterType, 50 LocalVariables and 83 LocalType, over the
same 50 method bodies. Where the probe prints a field, return, parameter or property
type, a failed read would print as null, and none reads null in this result. The word
stands in the result only in the label `the null checks`, result lines 73 and 74, and in
the add-in's `PublicKeyToken=null`, result line 107.

WHAT LIST A LEAVES OUT, named. The three framework members the compiled file reaches over
a Navisworks type, `Collection<MaterialOverride>.Count` and `GetEnumerator` and
`IEnumerator<MaterialOverride>.Current`, which section C resolves, lines 150, 155 and 156.
The calls through `System.IDisposable.Dispose` that a `using` compiles to, which it checks
instead as IDisposable on each of the nine types, lines 59 to 67. And anything outside
`SavedViewpoints.cs`. Its first version, committed in 69cff64, had 53 lines and missed
`Point3D.X`, `Y` and `Z`, `Color.R`, `G` and `B`, the `Dispose` on the wrappers and the
`==` and `!=` operators on `NativeHandle` that the null checks compile to, now lines 29 to
31, 52 to 58, 73 and 74. Section C reports no member of an install assembly missing from
the list, line 219.

WHAT THE BUILD SAYS AND DOES NOT. `+edits` in its stamp says the main clone's working
tree held a change when it was built, and not which. Its `SavedViewpoints.cs` is the same
file as this branch's, git blob c007d817fa09f8c2aaf9fdfa3d667bb5c7735cc5, and was last
written on 2026-09-27, before the build, so the three classes read were compiled from
that file. Whether a change elsewhere in that build matters to these classes is not read
here.

Against 5d, member by member, lines of viewpoints-result:

```
                                                                        5d    today   line
Document.SavedViewpoints { get }, a DocumentSavedViewpoints             yes   yes     6
DocumentSavedViewpoints, base System.Object, IDisposable False          yes   yes     10, 12
  CurrentSavedViewpoint { get; set }                                    yes   yes     13
  RootItem { get }, a FolderItem                                        yes   yes     15
  Value { get }                                                         yes   yes     16
  AddCopy(SavedItem), AddCopy(GroupItem, SavedItem)                     yes   yes     18, 19
  CaptureRuntimeOverrides(), returns SavedViewpoint                     yes   yes     20
  Clear(), CopyFrom twice, CreateCopy()                                 yes   yes     21 to 24
  EditDisplayName(SavedItem, String)                                    yes   yes     28
  InsertCopy(GroupItem, Int32, SavedItem)                               yes   yes     30
  Remove(GroupItem, SavedItem), returns Boolean                         yes   yes     34
  RemoveAt(GroupItem, Int32)                                            yes   yes     36
  ReplaceFromCurrentView(SavedViewpoint)                                yes   yes     37
  ReplaceWithCopy(GroupItem, Int32, SavedItem)                          yes   yes     38
SavedViewpoint, base SavedItem, IDisposable True                        yes   yes     47, 49
  SavedViewpoint(Viewpoint), SavedViewpoint()                           yes   yes     50, 51
  ContainsAppearanceOverrides, ContainsVisibilityOverrides { get }      yes   yes     52, 53
  Viewpoint { get }, no setter                                          yes   yes     56
  GetAppearanceOverrides(), GetVisibilityOverrides()                    yes   yes     58, 59
Viewpoint, IDisposable True                                             yes   yes     67
FolderItem, public FolderItem()                                         yes   yes     127
```

`probe-viewpoints.ps1` prints a generic type by its bare name, so its lines 21 to 24 show
`Collection`1 CreateCopy()` and `CopyFrom(IEnumerable`1 value)` and no more. Section B of
viewpoint-calls prints the arguments: `Collection<SavedItem> CreateCopy()` and
`CopyFrom(IEnumerable<SavedItem> value)`, lines 94 and 96, exactly as 5d records them.

5d's claim that the two collections have the same shape holds on this install:
`RootItem`, `AddCopy`, `InsertCopy`, `Move`, `Remove`, `RemoveAt` and `ReplaceWithCopy`
with the same signatures on both, viewpoints-result lines 15 to 39 and 166 to 189, and
the same generic `CreateCopy` and `CopyFrom`, viewpoint-calls lines 94 to 99. Where they
differ, the sets carry `CreateSelectionSource` and `ResolveSelectionSource`, lines 177
and 193, and the viewpoints carry `CurrentSavedViewpoint`, `CaptureRuntimeOverrides` and
`ReplaceFromCurrentView`. The hiding members 5d names are there too, `ModelItem.IsHidden`
with a getter and no setter, lines 207 and 208, and `DocumentModels.SetHidden` and
`ResetAllHidden`, lines 350 and 352, and so are the two 5k measured,
`ResetAllHiddenToModelState` and `GetAllHiddenAtModelState`, lines 353 and 354.

WHAT 5d DID NOT LIST AND THIS RUN PRINTS. 5d listed the members F52 needs, not all of
them. On `DocumentSavedViewpoints` viewpoints-result also prints `Id`, `AddComment`,
`CreateIndexPath`, `CreateReference`, `EditComments`, `InsertCopy(Int32, SavedItem)`, both
`Move`, `Remove(SavedItem)`, `RemoveAt(Int32)`, `ReplaceWithCopy(Int32, SavedItem)`,
`ResolveGuid`, `ResolveIndexPath`, `ResolveReference` and `ToSavedItemCollection`, lines
14 to 43, and on `SavedViewpoint` `IsReadOnly`, `Redlines`, `EditRedlines`,
`InternalCreator` and `InternalFactory`, lines 54 to 61. `Remove(SavedItem)`, line 33, is
the one the add-in calls to take the root copy out, and until this run it was measured
only on the runs of 5k and 5m and never printed off the DLL.

THE COM HALF, which 5m measured on a run and no probe had printed. Lines of
viewpoint-calls-result, list A and then the IL, each row in the order it names:

```
public static InwOpState10 ComApiBridge.State { get }                            75, 121
public static InwOpAnonView ComApiBridge.ToInwOpAnonView(Viewpoint viewpoint)   76, 126
public Object InwOpState10.ObjectFactory(nwEObjectType eType,
    optional Object ovReserved1, optional Object ovReserved2)                    77, 122
public InwSavedViewsColl InwOpState10.SavedViews()                              78, 130
nwEObjectType.eObjectType_nwOpView = 11                                         79
InwOpView.name, ApplyHideAttribs, ApplyMaterialAttribs, anonview, each
    { get; set }, the four setters the file calls                               80 to 83, then 123, 124, 125, 127
InwOpFolderView.SavedViews(), then InwOpFolderView.name                         84, 85, then 128, 176
InwSavedViewsColl.Add(Object), then Count { get }, then Item[Object] { get; set }
                                                                                 86, 87, 88, then 129, 177, 175
```

STILL UNKNOWN.

- which file version of the DLL 5c and 5d read. They record the assembly version
  22.0.0.0 only, which matches, line 1. Today's file is 22.5.1433.58
- that any member behaves as 5j to 5m measured. Those were runs and this is a reflection
  pass. A member being there says nothing about what it does on this install
- the rest of the add-in. Section C reads the three classes of `SavedViewpoints.cs` and
  nothing else, so `DocumentClashTests.TestsViewpointForResult` and
  `DocumentCurrentViewpoint.CopyFrom`, which 5l names and `ViewpointBuilder` calls, were
  not read


**2. RemoveFile AND TryRemoveFile ON THIS INSTALL**

THE QUESTION. Do `Document.RemoveFile(int)` and `TryRemoveFile(int)` exist here with the
signatures 5c records? `FederationEngine.ReshapeFromScan` calls `document.TryRemoveFile(at)`,
`FederationEngine.cs` line 1642, so a missing or changed member would break the reshape.
`probe-model-remove.ps1` loads the DLL with `Assembly.LoadFrom`, line 21, as said above.

**YES, BOTH, WITH THE SIGNATURES 5c RECORDS.** Lines of model-remove-result:

```
                                                                        5c    today   line
Document  ->  public Boolean TryRemoveFile(Int32 index)                 yes   yes     83
Document  ->  public Void RemoveFile(Int32 index)                       yes   yes     84
DocumentModels  public Boolean InternalRemove(Model item)               yes   yes     40
DocumentModels  public Void InternalRemoveAt(Int32 index)               yes   yes     41
DocumentModels.IsReadOnly  canread=True canwrite=False                  yes   yes     76
AppendFile, AppendFiles, Clear, IsClear, TryAppendFile, TryAppendFiles  yes   yes     125 to 131
```

The probe prints `AppendFiles(IEnumerable`1 fileNames)` by its bare name. Section B of
viewpoint-calls prints `AppendFiles(IEnumerable<String> fileNames)` and
`TryAppendFiles(IEnumerable<String> fileNames)`, lines 100 and 101, as 5c records them.

PRINTED BY THIS RUN AND NOT QUOTED IN 5c. Under the question, lines 85 to 95:
`RemoveAt(Int32)` and `RemoveAt(GroupItem, Int32)` on `DocumentSavedViewpoints`,
`DocumentSelectionSets` and `DocumentInfoPart`, and on the interop class `LcVwDocument`,
`UnloadModel(Int32)`, `UnloadModelImpl(Int32)` and `SimAddAddActionDataCacheRemove`. And
`AppendSheet` and `TryAppendSheet` beside the append forms, lines 127 and 132. 5c quoted
chosen lines and kept no result file, so whether its run printed these is UNKNOWN.

STILL UNKNOWN.

- 5x is a run and not a reflection pass. That removing the last model of four kept every
  set, test, result, status and viewpoint cannot be read off a DLL, and this run neither
  confirms nor contradicts it. Whether removing a middle model shifts the indexes after it
  is still UNKNOWN, as 5x says
- what `LcVwDocument.UnloadModel` does. A search of `src` finds no call to it


**3. ROAMER.EXE'S COMMAND LINE SWITCHES, READ OFF THE FILES**

THE QUESTION. Which command line switches do the binaries carry, read without running
Roamer.exe? The answer that would change code is a switch that runs an add-in plugin on an
ordinary start, because the tool's window could then open with no click.

`probe-roamer-switches.ps1` reads Roamer.exe as bytes for every ASCII and UTF-16 string of
four or more characters, reads its PE headers, import table and CLI metadata as bytes,
reads each assembly it references that sits in the install folder by the one full path
built from the reference, and then reads the IL of the parser by reflection only, which
runs no code, through `il-reader.ps1`. Every failed read it hands to IlFail, itself or
through IlRead and LoaderLines, is counted and printed in its section 9, lines 1477 to
1527, and where a section prints it, at the place it failed as well. A read it hands to
no IlFail is in no count, as said at the top of this section. Lines of
roamer-switches-result.

**ROAMER.EXE HOLDS NO SWITCH TABLE. navisworks.gui.roamer.dll DOES, AND ITS PARSER
MATCHES 39 SWITCHES.**

1. Roamer.exe, 214,296 bytes, is a managed PE32+ AMD64 exe with an empty PE import table,
   data directory 1 at rva 0, lines 64 to 70. None of its 759 strings is a switch or holds
   one, lines 8 to 12. Counted exactly, case counting, it holds none of
   ExecuteAddInPlugin, AddPluginAssembly, Embedding, regserver or OpenFile, lines 55 to
   60, and the verdict, which counts the same way, reads False, line 61. Read case blind
   it holds `RegServer` and `UnregServer`, lines 19, 40 and 41, which are the names of
   two `CommandLineConfig` fields its `MainImpl` reads, lines 967 and 969, and not a
   literal the parser matches. It holds NoGui only inside two log lines,
   `Program.Main - Before dispatch NoGui actions` and `After`, lines 48 and 49
2. Its CLI metadata, read as bytes, references 16 assemblies, lines 76 to 92, and a
   reflection only read of the same file lists the same 16, line 96. No reference failed
   to load, line 97. Seven of the 16 have a file of that name in the install folder and
   were read as bytes, lines 101 to 105, 109 and 112, and of those seven
   `navisworks.gui.roamer.dll`, 648,984 bytes, is the only one holding
   ExecuteAddInPlugin, NoGui, Embedding and regserver together, lines 103 and 116. The
   other nine were NOT READ as bytes, because the probe opens a reference only by its one
   full path in the install folder and none of the nine has a file of that name there:
   mscorlib, System, System.Xml.Linq, System.Xml, PresentationFramework,
   System.Windows.Forms, WindowsBase, PresentationCore and System.Core, lines 100, 106 to
   108, 110, 111 and 113 to 115. Whether any of the nine holds those four names, and so
   whether `navisworks.gui.roamer.dll` is the only reference of Roamer.exe that does, is
   UNKNOWN
3. In that DLL the names are UTF-16 literals stored WITHOUT a hyphen or a slash, lines 293
   to 302. The one string in it shaped like a switch, `-EsO`, line 124, is four bytes
   between binary data and no switch
4. `CommandLineParser::ParseOption` matches 39 names after a literal, 37 through
   `MatchOption` and 2 through `MatchShellOption`, line 410, the options at lines 411 to
   449:

   ```
   MatchOption       Embedding, Automation, regserver, Register, unregserver, Unregister,
                     RegisterPerUser, UnregisterPerUser, user, noreg, pure, log, dump,
                     dump_options, MemCheck, options, lang, nwd, nwc, bench, ShowGui,
                     HideGui, NoGui, NoCache, OpenFile, GenerateThumbnail,
                     GenerateThumbnailByRayTrace, CreateCache, Print, SaveFile, AppendFile,
                     Exit, ExecuteAddInPlugin, AddPluginAssembly, EnableProgress,
                     DisableProgress, Licensing
   MatchShellOption  p, pt
   ```

   The 37 are the names 5z-d read, in the same order. `p` and `pt` are new, and each adds
   an OpenFile action and a Print action and sets ExitAfterActions, lines 411 and 412
5. What counts as a switch, off the IL, lines 544 to 583. An argument is a switch when it
   starts with one hyphen or one slash and not two, `IsOption`. `MatchOption` compares
   the option it holds with StringComparison 5, OrdinalIgnoreCase, lines 576 and 577, so
   the match is case blind. `MatchShellOption` compares exactly, line 583. How the option
   it holds is cut from the argument is in `Parse`, which was not printed whole

**-ExecuteAddInPlugin NEEDS NO -Embedding IN THE PARSER, AND THE IL READ HOLDS A PATH FROM
IT, ON A START WITH THE WINDOW, AS FAR AS A MODULE LEVEL METHOD, s_std_dispatch, WHOSE IL
WAS NOT READ. THAT STEP IS UNKNOWN, AND SO IS EVERYTHING AFTER
ApplicationAutomation.ExecuteAddInPlugin.** Read off the IL and NOT RUN:

```
parser      -ExecuteAddInPlugin takes one argument or more and adds
            CommandLineAction("ExecuteAddInPlugin", args) to the action list          505 to 521
            -Embedding and -Automation set COMAutomationStartup and GuiState 2          452 to 462
            -NoGui sets GuiState 1                                                      463 to 469
            GuiState values DEFAULT 0, NONE 1, HIDE 2, SHOW 3                           1051
MainImpl    local 8 is GuiState equal to NONE                                           1202 to 1206
            local 8 true: every action dispatched at once, and RunGui jumped over       1158 to 1187
            local 8 false: the config and the actions handed to MainWindow, RunGui     1188 to 1192
OnIdle      while MainWindow holds actions, CommandLineActionDispatcher.DispatchOneAction  1207 to 1221
dispatcher  DispatchOneAction calls ApplicationImpl.DispatchAutomationAction            810
Api.dll     DispatchAutomationAction looks the action's name up with LookupMethod, a
            public static method of ApplicationAutomationImpl, BindingFlags 24, made
            into a delegate                                                             1324, 1456 to 1460
            and answers "Unknown action" where there is none                            1340
            and otherwise hands the delegate, the result and the arguments to
            s_std_dispatch, a module level method. ITS IL WAS NOT READ, so whether it
            calls the delegate is UNKNOWN                                               1362, 1372
            ApplicationAutomationImpl.ExecuteAddInPlugin(String[] args), the method
            LookupMethod finds for that name, throws with fewer than one argument,
            then calls ApplicationAutomation.ExecuteAddInPlugin(first argument, the
            rest). THAT METHOD'S IL WAS NOT READ                                         1436 to 1454
```

The probe prints a call as `null::` and a name where the method called has no declaring
type, which is how reflection shows a method defined at module level, outside any type,
lines 1405 and 1406. `s_std_dispatch` is one, and none of those methods' IL is read here.

So, as far as the IL was read, `Roamer.exe -ExecuteAddInPlugin <plugin id>` on a command
line with no other switch is written to hand the action from the window's idle handler to
`s_std_dispatch`, with a delegate of `ApplicationAutomationImpl.ExecuteAddInPlugin`. The
command line has to carry no other switch because the path turns on GuiState alone, and
besides -NoGui, -Embedding and -Automation the options dump_options, nwd, nwc, bench,
ShowGui, HideGui and Exit touch GuiState too, lines 426, 430 to 434 and 444. Whether
`s_std_dispatch` calls the delegate, and whether a start does any of this, is UNKNOWN. It
is the route 5z-d named for when the automation start fails.

WHAT COULD NOT BE READ. In navisworks.gui.roamer.dll 32 of its 3,042 MemberRef rows did
not resolve, each for lack of a generic context, and each token is listed, lines 741 and
742. What they name is UNKNOWN, so a use of a followed member through one of them would
be missed. The 12 followed members, lines 728 to 740, are the tokens section 6 searches
for, put on its list by construction, and the 12 at line 741 is the size of that list,
which says nothing about use. The DLL's own IL uses 7 of them, lines 743 to 775:
DispatchOneAction, ReportResult, AttachConsole, the action list and three config fields.
Four more appear only in Roamer.exe, the CommandLineParser constructor, Configure,
DispatchAllActions and ReportParseError, lines 778, 784, 786 and 788, and the
dispatcher's own constructor is used in neither. Roamer.exe's 235 rows all resolved, line
777. Section 9, lines 1477 to 1527, counts by kind every failed read the probe handed to
IlFail over the whole run. The 32 MemberRef rows are there, each with its token and
reason, lines 1485 to 1517, and the other six kinds this probe counts read 0: IL bodies,
bytes that are not an instruction, tokens, types whose methods could not be listed,
assemblies GetTypes could not load every type of, and metadata reads, lines 1482 to 1484,
1518, 1520 and 1521. The other five kinds print 0, types whose members could not be
listed, signatures, locals, field types and constant values, lines 1519 and 1522 to 1525,
and no line this probe can run, its own or il-reader.ps1's, counts a failure as one of
those kinds, so their 0 is not a reading. It makes some of those reads with no count,
among them constant values with GetRawConstantValue at its line 595, signatures with
GetParameters at its line 404, and field types through the FieldType property at its
lines 427 and 659. A failed method call among them would have stopped the probe, whose
ErrorActionPreference is Stop at its line 2, and its final run ran to its end, exit 0,
run record line 71. A failed FieldType at those two lines would not stop it and would
print as null, by what a read that fails does, at the top of this section, and no field
line of this result holds null. 32 in all, line 1526. No reference failed to load, line
1478. The IL reader counts the reserved bytes 0xF8 to 0xFD and 0xFF as not an
instruction, line 1527.

STILL UNKNOWN.

- what any switch does on a start. Nothing ran Roamer.exe
- what `s_std_dispatch` and `ApplicationAutomation.ExecuteAddInPlugin` do. Neither's IL
  was read
- whether `OnIdle` reaches its dispatch on a start, and when. Its instructions before
  IL_0058 read other fields first, section 7 of the result from line 851, and were not
  printed whole
- whether a plugin started this way is found when it comes from the installed bundle,
  what form of plugin id it wants, and what its int return means
- what a second Roamer started with this switch does while another Navisworks runs. No
  single instance logic was looked for
- what the dispatcher's Status values mean, which decide when `OnIdle` calls
  `ForceCleanExit`, line 1272. The enum was not printed
- where -log, -dump, -options and -lang values go past `ApplicationConfig` and
  `InitialiseResourcesConfig`, section 7. Nothing further was read

WHAT THIS WOULD CHANGE, once a start measures it. If `Roamer.exe -ExecuteAddInPlugin
ParsonsNwcFederator.PARS` opens the tool's window with no click, the loop's no-click entry
needs neither the automation API nor its -Embedding start. It is a candidate and not a
measurement.


**4. DOES THE INSTALLED API WRITE THE CLASH DETECTIVE HTML (TABULAR) REPORT**

THE QUESTION. Is there a member that writes the native Clash Detective report, the HTML
(Tabular) page Bader exports by hand, 4m? If there is, the page could come from Navisworks
rather than from this tool's XML through `clash_report_html_tabular.xsl`.

`probe-clash-report-api.ps1` reads by reflection only, each by its one full path,
`Autodesk.Navisworks.Api.dll`, `Autodesk.Navisworks.Clash.dll`,
`Autodesk.Navisworks.ComApi.dll`, `Autodesk.Navisworks.Interop.ComApi.dll` and
`Autodesk.Navisworks.Automation.dll`. It lists every public type and member whose name
holds report, html, tabular or export, case blind, every public COM type whose name holds
Clash or starts InwOcl, and every string in each file holding tabular, .xsl, clash_report
or reportformat, case blind, each string with the words it holds and each word counted on
its own. scan.md names no Clash Detective assembly beyond
`Autodesk.Navisworks.Clash.dll`, so no other was read. No reference failed to load, line
591, and of the three kinds this probe counts none failed: types whose members could not
be listed, assemblies GetTypes could not load every type of, and constant values, lines
598, 599 and 604, 0 in all at line 605. The other nine kinds print 0, IL bodies, bytes
that are not an instruction, tokens, MemberRef rows, types whose methods could not be
listed, metadata reads, signatures, locals and field types, lines 593 to 597 and 600 to
603, and no line this probe can run, its own or il-reader.ps1's, counts a failure as one
of those kinds, so their 0 is not a reading. It makes some of those reads with no count,
among them signatures with GetParameters at its lines 55, 187 and 197, a type's methods
with GetMethods at its line 183, and field types through the FieldType property at its
lines 73, 74 and 199. A failed method call among them would have stopped the probe, whose
ErrorActionPreference is Stop at its line 2, and its final run ran to its end, exit 0,
run record line 72. A failed FieldType would not stop it, by what a read that fails does,
at the top of this section. At its lines 73 and 74 it would print as null, and no line of
this result holds null. At its lines 197 to 199, where a return, property or field type
is only compared, a failed read would leave that member out of the members handing one
out with nothing printed, so whether one failed there is UNKNOWN. Such a failure could
only leave a member out, never put one in. Lines of clash-report-api-result.

**NO PUBLIC TYPE OR MEMBER IN Autodesk.Navisworks.Api.Clash, THE NAMESPACE THE ADD-IN
USES, AND NONE IN THE COM CLASH INTERFACES, HAS REPORT, HTML, TABULAR OR EXPORT IN ITS
NAME. ONE PUBLIC STATIC METHOD IN THE INTEROP NAMESPACE OF Autodesk.Navisworks.Clash.dll,
WHICH THE ADD-IN REFERENCES, IS NAMED FOR WRITING A CLASH REPORT, AND WHICH FORMATS IT
OFFERS IS NOT IN THE METADATA.**

```
class Autodesk.Navisworks.Api.Interop.LcClClashReport : NativeHandle                     131
  public static Boolean WriteReport(LcOpState state, LcClClashGUIProxy guiProxy,
                                    LcClashReportResultSelector selection,
                                    String name_annotation)                               163
  public static Boolean CanWriteReport(LcOpState state)                                    133
  public static Boolean FormatterIsViewpoints(Int32 index)                                 134
  public static Void GetReportDriverName(Int32 index, out String reportDriver)             141
  public static Void GetReportFormatterName(Int32 index, out String reportFormatter)       144
  public static Int32 NumReportDrivers()                                                   153
  public static Int32 NumReportFormatters()                                                155
  public static Void SetCurrentReportDriver(Int32 index)                                   159
  public static Void SetCurrentReportFormatter(Int32 index)                                160
```

- that `WriteReport` writes a report at all is read off its name and nothing else. It
  takes no file name. Where it writes, and whether it asks through its `guiProxy`, is
  UNKNOWN
- the report kinds and formats are counted and named at run time, `NumReportFormatters`
  and `GetReportFormatterName`, so they are NOT in the metadata, and whether HTML
  (Tabular) is one of them is UNKNOWN until a start reads them. The three enums whose names
  suggest kinds, `LcClClashReport+ReportFields`, `LcOclClashReportConfig+ReportContentFlags`
  and `LcOpPlugin+ExportStatus`, are native C++ types with no values in the metadata, each
  inside a type that is not public, lines 173 to 187
- its arguments can be had from public members: `LcOpState.GetActiveInstance()`, line
  573, and public parameterless constructors on `LcClClashGUIProxy` and
  `LcClashReportResultSelector`, lines 576 and 580. Whether a call from outside Clash
  Detective's own window works is UNKNOWN
- no string in any of the five files holds tabular or .xsl, the per word counts at lines
  114, 222, 244, 549 and 566. The one string holding clash_report, read case blind, is
  Api.dll's `ePLUGIN_CLASH_REPORT`, line 113, the name of the value 6 of
  `LcOpPluginType`, line 89, so the plugin types include one named for a clash report.
  Clash.dll's 16 strings all hold reportformat and none holds another of the four words,
  line 222: its formatter method names and one parameter name, lines 206 to 221. Which
  plugin holds an HTML (Tabular) formatter, if any does, and in which file, is UNKNOWN
- the thirteen COM clash types, lines 303 to 543, carry tests, results, pictures and
  viewpoints and no report member. `Autodesk.Navisworks.Automation.dll` holds nothing
  matching, lines 557 and 560, and `Autodesk.Navisworks.ComApi.dll` one native nested
  enum and no member, lines 235 and 238
- what `Document.ExportAsDwf(String)`, line 73, writes. It is the one member on `Document`
  whose name holds one of the four words, and nothing about clashes is in its name

WHAT THIS DECIDES. Nothing in the code yet. The page stays rendered from this tool's XML
through Autodesk's stylesheet, 4m. A start that reads `NumReportFormatters` and each
`GetReportFormatterName` would say whether `WriteReport` can write HTML (Tabular) at all.

## 5z-g. DOES THE API CARRY ITS OWN UNIT FACTOR, MEASURED 2026-09-29

PQ2 of F104. The check of a workbook against its document has to turn the document's
tolerances and distances into metres WITHOUT UnitTable, because UnitTable is the table the
harvest converted with, and a check that shares the code it checks proves nothing. The
names UnitConversion and ScaleFactor had been found in `Autodesk.Navisworks.Api.dll` by a
grep, and nothing said whether ScaleFactor is public, whether it is static, or what it
takes and returns.

Read off the metadata of the installed DLL by its one full path, with
`ReflectionOnlyLoadFrom`, so no line of it ran and no Navisworks was started:

    powershell -ExecutionPolicy Bypass -File tools\probes\probe-unit-scale.ps1

The whole output is `tools\probes\unit-scale-result-20260929.txt`, the machine's name
masked. The result line:

```
PQ2 YES   public static System.Double ScaleFactor(Autodesk.Navisworks.Api.Units from, Autodesk.Navisworks.Api.Units to)
```

**THE ANSWER IS YES.** `UnitConversion` is a public class based on NativeHandle, not a
static class, and ScaleFactor is a static member of it, 32 bytes of managed IL that go
through to the native LcOaUnitConversion. Its two parameters are named from and to, so
DocumentReadProbe calls `ScaleFactor(document units, Meters)` for metres per unit.

STILL UNKNOWN, because it needs Navisworks running and this read ran none of it:

- what ScaleFactor returns for Feet to Meters. DocumentReadProbe reads it inside Navisworks
  on its first run, beside Meters to the document's units and Millimeters to Meters
- whether from and to mean what their names say. A number times its inverse is one
  whichever way the factor runs, so the probe writes a doubt and every value in metres as
  UNKNOWN unless `ScaleFactor(Millimeters, Meters)` reads below one
- which units `ClashTest.Tolerance` and a clash's `Distance` are held in, PQ3 of F104

**WHAT THIS DECIDES.** No unit table is written for the check. DocumentReadProbe converts
through Navisworks' own factor, so the harvest's UnitTable and the check never share a
number, and `tools\loop\compare-document.ps1` marks the tolerances and distances NOT
COMPARED when the probe could not read the factor and the document is not in metres.

## 5z-h. CAN A BOUNDINGBOX3D BE BUILT FROM TWO POINT3D, MEASURED 2026-10-05

P5 of Q114, the views by team design, part 3. F114 frames one view per test on the box of
that test's open clash centres, FramingBox's two corners, and hands the box to
Viewpoint.ZoomBox. Nothing had read whether Autodesk.Navisworks.Api.BoundingBox3D can be
made from two Point3D. A yes means FramingBox's corners build the box. A no means only the
camera arithmetic route is probed in P16.

Read off the metadata of the installed DLL by its one full path, with
`ReflectionOnlyLoadFrom`, so no line of it ran and no Navisworks was started. Get-Process
Roamer read 0 processes before the read and 0 after, printed by the probe itself. Run from
Windows PowerShell 5.1 as

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-boundingbox-ctor.ps1

The whole output is `tools\probes\ViewpointProbe\p5-boundingbox-result-20261005.txt`, the
machine's name masked, exit 0. Autodesk.Navisworks.Api 22.0.0.0, file 22.5.1433.58. The
result lines:

```
  P5 YES   public .ctor(Autodesk.Navisworks.Api.Point3D minPoint, Autodesk.Navisworks.Api.Point3D maxPoint)
  first parameter minPoint, second maxPoint
  implementation Managed, 289 bytes of IL
```

**THE ANSWER IS YES.** BoundingBox3D is a public class based on NativeHandle with three
constructors: the public one from two Point3D named minPoint and maxPoint, a public one with
no parameters, and a protected one for the native handle. It also has a public static Empty.
Point3D has a public constructor from three doubles x, y and z. The same read prints
`public System.Void ZoomBox(Autodesk.Navisworks.Api.BoundingBox3D box)` on Viewpoint, the
one member of that name.

STILL UNKNOWN, because it needs Navisworks running and this read ran none of it:

- what the box holds once built, and whether the constructor accepts, swaps or refuses a
  minPoint that is not below maxPoint on every axis. FramingBox should hand the corners in
  min then max order until a run says otherwise
- which units the box is read in
- what ZoomBox does with the box, P16

**WHAT THIS DECIDES.** FramingBox's two corners build the box with
`new BoundingBox3D(new Point3D(x, y, z), new Point3D(x, y, z))`, so P16 probes ZoomBox with
a box built this way.

## 5z-i. CAN A COMMENT BE PUT ON THE COM VIEW BEFORE IT IS ADDED, MEASURED 2026-10-05

P6 of Q114, the views by team design, part 3. F114 marks every view the tool writes with a
comment, and writes each view through the COM API's InwOpView before
InwOpFolderView.SavedViews().Add puts it in its folder, 5m. 5z-f printed that InwOpView has
a Comments() method, viewpoint-calls-result line 244, and nothing had read what that
collection lets a caller do or what object goes in it. A yes means P9 tries the comment on
the COM view before the add first, which costs no extra call per view. A no means the mark
is DocumentSavedViewpoints.AddComment after the add.

Read off the metadata of the installed DLL by its one full path, with
`ReflectionOnlyLoadFrom`, so no line of it ran and no Navisworks was started. Get-Process
Roamer read 0 processes before the read and 0 after, printed by the probe itself. Run from
Windows PowerShell 5.1 as

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-com-view-comments.ps1

The whole output is `tools\probes\ViewpointProbe\p6-com-view-comments-result-20261005.txt`,
the machine's name masked, exit 0. Autodesk.Navisworks.Interop.ComApi 22.0.0.0, file
22.5.1433.58. The result lines:

```
  members of InwOpView and the interfaces it inherits that name or return a comment type:
    InwOpView.public Autodesk.Navisworks.Api.Interop.ComApi.InwCommentsColl Comments()   dispid 1610809347
    InwOpSavedView.public Autodesk.Navisworks.Api.Interop.ComApi.InwCommentsColl Comments()   dispid 1610809347
  members of InwCommentsColl that add, insert, replace or set:
    public System.Void Replace(System.Int32 ndx, System.Object p_newVal)   dispid 1610743816
    public System.Void Insert(System.Int32 ndx, System.Object p_newVal)   dispid 1610743817
    public System.Void Add(System.Object p_newVal)   dispid 1610743818
    property System.Object Item[System.Object vIndex] { get; set }   dispid 0
  nwEObjectType values that make a comment through ObjectFactory:
    eObjectType_nwOpComment = 8
  P6 YES   InwOpView hands out its comments collection, the collection has a member that adds, and ObjectFactory has a comment type to add
```

**THE ANSWER IS YES, ON THE METADATA.** InwOpView, inheriting InwOpSavedView, has
`InwCommentsColl Comments()` and no comment property with a setter. InwCommentsColl has
Add(Object), Insert(Int32, Object), Replace(Int32, Object), Remove(Int32), RemoveLast(),
Clear(), Last(), Count, an Item indexer with get and set, and a `ReadOnly { get }`. The
object to add is made by `InwOpState10.ObjectFactory(nwEObjectType.eObjectType_nwOpComment)`,
value 8, the same factory 5m makes the view with at value 11. The comment interfaces are
InwOpComment with Body, User and Date, each with get and set, InwOpComment2 adding
CommentID, and InwOpComment3 adding status of nwECommentStatus, NEW 0, ACTIVE 1, APPROVED 2
and RESOLVED 3. InwOpFolderView has the same Comments() method, so a folder can carry one
the same way.

STILL UNKNOWN, because it needs Navisworks running and this read ran none of it:

- which of InwOpComment, 2 or 3 the factory's object implements at run time
- what ReadOnly reads on the collection of a view that is not yet in a folder, and whether
  Add on it is refused
- whether a comment added before InwSavedViewsColl.Add is kept by the add, and whether it
  reads back off SavedItem.Comments with the same body and author after a save, a clear and
  a reopen, P9
- whether the User set here is what SavedItem.Comments reads as Author, P9

**WHAT THIS DECIDES.** P9 tries the COM route first: a comment made by ObjectFactory at
eObjectType_nwOpComment, its Body and User set, added to the COM view's Comments() before
the view is added to its folder. If P9 finds it is not kept or not read back, the mark is
AddComment after the add.

## 5z-j. DOES THE TYPE OF SAVEDVIEWPOINT.REDLINES EXPOSE A COUNT, MEASURED 2026-10-05

P7 of Q114, the views by team design, part 3. F114's judge of a view the tool wrote asks
whether a person has drawn on it, and the one place a saved viewpoint holds its redlines is
SavedViewpoint.Redlines, whose type 5z-f printed as LcOpRedlineList, viewpoints-result line
55. Nothing had read whether that type can say how many redlines it holds. A yes means P20
reads it on a tool view and again after Bader draws a redline on it. A no means redlines are
UNKNOWN in the mark's judge, and the log says so.

Read off the metadata of the installed DLL by its one full path, with
`ReflectionOnlyLoadFrom`, so no line of it ran and no Navisworks was started. The type was
read off the Redlines property itself, not looked up by name. Get-Process Roamer read 0
processes before the read and 0 after, printed by the probe itself. Run from Windows
PowerShell 5.1 as

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-redline-list-count.ps1

The whole output is `tools\probes\ViewpointProbe\p7-redline-list-count-result-20261005.txt`,
the machine's name masked, exit 0. Autodesk.Navisworks.Api 22.0.0.0, file 22.5.1433.58. The
result lines:

```
  public type of Redlines: Autodesk.Navisworks.Api.Interop.LcOpRedlineList
  public members of it, inherited ones included, that read a count:
    public System.Int32 Size()   declared on LcOpRedlineListBase
  how each counting member and ItemAt are implemented, read off the method body:
    LcOpRedlineListBase.Size   implementation Managed, 45 bytes of IL
    LcOpRedlineListBase.ItemAt   implementation Managed, 58 bytes of IL
  collection interfaces it implements:
    none
  P7 YES   the type of SavedViewpoint.Redlines has a public member that reads a count
```

**THE ANSWER IS YES, ON THE METADATA, AND THE COUNT IS A METHOD NAMED Size.** The type is
`Autodesk.Navisworks.Api.Interop.LcOpRedlineList`, defined in Autodesk.Navisworks.Api itself,
a public class based on LcOpRedlineListBase and then NativeHandle, implementing IDisposable
alone, result lines 13 to 16. It has no Count property and implements no ICollection, IList
or IEnumerable, so it cannot be counted with Count or walked with foreach. The count is the
public method `Int32 Size()` declared on LcOpRedlineListBase, line 48, and each redline is
read with `LcOpRedline ItemAt(Int32 n)`, line 49. LcOpRedlineList adds `Add(LcOpRedline)`,
`Clear()` and an `IsReadOnly { get }`, lines 22 to 25. On SavedViewpoint, `Redlines` has a
getter and no setter, line 7, and `EditRedlines()` returns the same type, line 9. The
assembly also has the public redline element types LcOpRedline, LcOpRedlineArrow,
LcOpRedlineCloud, LcOpRedlineEllipse, LcOpRedlineLine, LcOpRedlineText and
LcOpRedlinePointList, lines 85 to 97.

STILL UNKNOWN, because it needs Navisworks running and this read ran none of it:

- what Size() reads on a view the tool wrote, and whether it rises by one when Bader draws a
  redline on it, P20
- whether Size() counts a redline element or a group of them, so whether one drawn cloud
  with text reads 1 or 2
- whether reading Redlines on a viewpoint of a document read off an NWF throws, and whether
  the list must be disposed after the read

**WHAT THIS DECIDES.** P20 runs, since a count exists. It reads
`savedViewpoint.Redlines.Size()` on a tool view and again after Bader draws one redline on
it, and only after P20 may the mark's judge read redlines. Until then the judge says
redlines are UNKNOWN.

## 5z-k. DOES A TEST WITH ITS SIDES SWAPPED FIND THE SAME CLASHES, MEASURED 2026-10-05

P1 of Q114, the views by team design, part 3, and the first of its probes that runs inside
Navisworks. F132's mirror rule keeps one of two tests that ask the same pair of sets the
other way round, on the belief that a swap finds the same clashes. Nothing had measured it.
The question: does the swap of BLD-ST-Framing-vs-BLD-ST-Columns, created and run beside it,
find the same 25 clashes over the same unordered pairs of item index paths? A yes means the
rule stands as written. A no means both counts go in the MIRROR line and Bader sees it before
F132's add-in commit.

HOW. `tools\probes\ViewpointProbe\probe-mirror-swap.ps1` starts one Navisworks through the
automation API under the loop's guard, tools\loop\nw-guard.ps1 dot-sourced: the refusal while
any Roamer runs, read before anything and again before the constructor, the settings backup,
the adoption by AdoptStart's four conditions, Dispose, the one close through the held handle
when needed, and the put back by SettingsPutBack. Get-Process Roamer read 0 before each run.
It copies the C02 NWF of run set 04, `runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`,
41,317,271 bytes, sha256 0944C100, into a new folder under
`%LOCALAPPDATA%\NwcFederatorLoop\probes`, loads `ViewpointProbe.dll` with AddPluginAssembly and
runs its new mode `mirror` with one ExecuteAddInPlugin. The mode opens the copy, finds the test
by name, adds a test whose side A is the original's side B and whose side B is the original's
side A by ClashSelection.CopyFrom, clears its results, runs it with TestsRunTest, runs the
original again, and compares the results that are not Resolved as unordered pairs of the index
paths of Item1 and Item2, read with DocumentModels.CreateIndexPath. Run from Windows PowerShell
5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-mirror-swap.ps1 -Out tools\probes\ViewpointProbe\p1-mirror-swap-result-20261005.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf -TestName BLD-ST-Framing-vs-BLD-ST-Columns

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`.

TWO RUNS, BOTH KEPT, the machine name on line 1 of each masked by hand as `[machine]` and nothing
else changed.

- RUN 1 at 12:53, `p1-mirror-swap-run1-result-20261005.txt`, Navisworks pid 39216. The swap was
  made with the original's CreateCopy, and TestsAddCopy threw `ArgumentException: Contains an item
  whose GUID is already present in the group`, lines 95 and 96. So a copy made by CreateCopy keeps
  the original's Guid and cannot be added beside it. Nothing of P1 was measured. ExecuteAddInPlugin
  returned 1, line 48, Dispose returned and pid 39216 was gone 6.9 s later, not forced, lines 104
  and 105
- RUN 2 at 12:56, `p1-mirror-swap-result-20261005.txt`, Navisworks pid 54784, adopted on all four
  conditions, line 37. The swap is a new ClashTest built the way ClashRunner.Create builds one, with
  the original's type, tolerance, merge composites and simulation type. The original carries no
  ignore rules, line 66, so a new test misses none. ExecuteAddInPlugin returned 0 after 26.12 s,
  line 48. Dispose returned and pid 54784 was gone 8.0 s later, not forced, lines 175 and 176

The two sides, plugin lines in run 2's result, lines 66 to 68 and 97 to 99:

```
original  type HardConservative, tolerance 0.0820209974 ft, merge composites True, ignore rules 0
          side A  "BLD-ST-Framing"  27 items selected
          side B  "BLD-ST-Columns"   8 items selected
swap      the same settings, side A "BLD-ST-Columns" 8 items, side B "BLD-ST-Framing" 27 items
```

**THE ANSWER IS NO.** Lines 69, 104, 133 and 159 to 169 of run 2's result:

```
the original as the NWF holds it      25 results, New 25
the swap after its run                27 results, New 27
the original run again beside it      25 results, Active 25
the swap against the original         in both 25, in the swap only 2, in the original only 0
the original run again against itself in both 25, the same 25 pairs, largest distance change 3.7E-09
only in the swap   3.1.0.0.0.0.0 | 3.4.1.0.0.3.0   distance -1.3940146831272822
only in the swap   3.1.0.0.0.3.0 | 3.3.1.0.0.4.0   distance -1.3940188019622104
```

1. The swap finds every one of the original's 25 pairs and 2 more, so it is not a mirror of the
   original on this test. The original run again finds exactly the 25 it held, so the 2 come from
   the swap and not from a run that differs from the one before
2. On every one of the 25 pairs in both, Item1 is the item of side A, so the swap holds each
   pair the other way round, line 160
3. The distance of the same pair differs between the two, by up to 1.5331472298816213 ft, line 160.
   The swap's 27 distances are lines 105 to 131, the original's 25 lines 70 to 94. Run again, the
   original's distances moved by at most 3.7E-09 ft, line 168
4. Each of the 2 extra pairs has a distance of about -1.394 ft, far past the tolerance of 0.082 ft,
   so neither sits on the tolerance's edge
5. TryOpenFile of the copy took 4.667 s, TestsRunTest 6.161 s on the swap and 6.454 s on the
   original, and SaveFile of the copy with the swap 8.729 s, 41,319,419 bytes read back, lines 55,
   102, 132 and 171

**BADER'S THINGS.** The NWF the copy was made from read sha256 0944C100 at the start and at the end
of both runs. Run 1 put back 36 registry values and run 2 39, each read again with 0 still
differing, and each put back InfoCenter.log and LastSession.xml reading their backup's sha256. The
tool's own logs folder had nothing added or changed. Run 1 added no AutoSave file. Run 2's
Navisworks added two to his AutoSave folder, `1104-PAR-1A02MM-ZZZ-BM-MOD-000001.Autosave1.nwf` and
`.Autosave2.nwf`, lines 264 and 265, which the probe compares and does not write. After the probe,
by the rule of run.ps1's PutBackAutoSave, Q86, with no Roamer running and each reading the sha256
the compare read, the prober removed both and read each back gone, and the folder lists 199 files
again, the count the backup listed. That removal is in no result file.

**P1'S COPY FOR P2.** `%LOCALAPPDATA%\NwcFederatorLoop\probes\mirror-swap-20261005-125658\p1-copy-with-swap.nwf`
holds the 528 tests of the baseline and the swap last at the root, index 528, named
`BLD-ST-Framing-vs-BLD-ST-Columns P1 swap`, with its 27 New results. The original's 25 results
there read Active, not New, because it was run again.

**STILL UNKNOWN.**

- why the swap finds 2 more. Whether Hard Conservative is not symmetric by design, or what in
  these four items makes the difference, is not read here
- whether any other test swaps the same way. One test of one type in one NWF was measured
- whether the Clash Detective panel shows 27 for the swap. The panel was not read
- whether the swap's results read the same after a save and a reopen. The copy was saved and not
  reopened
- whether a test made by the XML import and swapped behaves as this new ClashTest did. The original
  came from the import, the swap from new ClashTest
- which way round a person's mirror test in a real NWF was written, and so which of the two the
  rule would remove

**WHAT THIS DECIDES.** By the row of P1, both counts go in the MIRROR line, and Bader sees before
F132's add-in commit that on BLD-ST-Framing-vs-BLD-ST-Columns the swap found 27 where the original
found 25, every one of the 25 among them. A test and its swap cannot be taken to find the same
clashes, so removing either one can lose clashes.

## 5z-l. DOES TESTSREMOVEAT TAKE ONE TEST AND NOTHING ELSE, MEASURED 2026-10-05

P2 of Q114, the views by team design, part 3. F132's MirrorRemover would remove a mirror test
with DocumentClashTests.TestsRemoveAt(GroupItem parent, int index). The member was printed off
the install and what it does was UNKNOWN. The question: does TestsRemoveAt(parent, index), with
the parent resolved fresh, remove exactly P1's swapped test with its results, while the models,
sets, other tests, results, statuses and viewpoints count the same after a save, a close and a
reopen, and how many seconds does the call take? A yes means MirrorRemover is built. A no means
no test is ever removed, and a mirror is left not run and named in the form.

HOW. `tools\probes\ViewpointProbe\probe-test-remove.ps1` is P1's `probe-mirror-swap.ps1` with the
new mode `testremove` of `ViewpointProbe.dll` in place of `mirror`, and `-Nwf` allowed under
`probes` as well as `runs`, because the file P2 works on is P1's saved copy. The guard is the
loop's, tools\loop\nw-guard.ps1 dot-sourced, with the Roamer refusal, the settings backup, the
adoption by AdoptStart's four conditions, Dispose, the close through the held handle only when
needed, and SettingsPutBack. Get-Process Roamer read 0 before the run and 0 after it. The probe
copied P1's `%LOCALAPPDATA%\NwcFederatorLoop\probes\mirror-swap-20261005-125658\p1-copy-with-swap.nwf`,
41,319,419 bytes, sha256 34831A9E, into the new folder `probes\test-remove-20261005-133231`, and
the mode, on that copy:

1. prints TestsRemoveAt and TestsRemove by reflection
2. opens the copy, finds the test by name over the whole test tree, and reads its census line
3. takes a snapshot of the document: every model by index and file name, every item of the set
   tree and of the viewpoint tree by path, name and folder or type, in order, and every test by
   path, name, type, tolerance, test status, result count, results by status, and a sha256 of
   every result's name and status in order
4. reads the parent fresh as `TestsData.Value.TestsRoot`, walked down the test's address when it
   sits in a folder, reads the child at the index and checks its name, then times
   `TestsRemoveAt(parent, index)` alone
5. takes the snapshot again and compares it line by line with the first less the removed test's
   line, and the result total with the first less the removed test's results
6. SaveFile into a new file in the work folder, Document.Clear, TryOpenFile of the saved file,
   and the same snapshot and compare, against the first and against step 5

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-test-remove.ps1 -Out tools\probes\ViewpointProbe\p2-test-remove-result-20261005.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\probes\mirror-swap-20261005-125658\p1-copy-with-swap.nwf -TestName "BLD-ST-Framing-vs-BLD-ST-Columns P1 swap"

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`.
One run at 13:32, `p2-test-remove-result-20261005.txt`, the machine name on line 1 masked by hand
as `[machine]` and nothing else changed. Navisworks pid 38964, adopted on all four conditions,
line 37. ExecuteAddInPlugin returned 0 after 25.41 s, line 48. Dispose returned and pid 38964 was
gone 10.4 s later, not forced, line 98.

The member, line 55: `Void TestsRemoveAt(GroupItem parent, Int32 index)`.

**THE ANSWER IS YES.** Lines 61 to 93 of the result:

```
the test removed      "BLD-ST-Framing-vs-BLD-ST-Columns P1 swap" at the root, index 528,
                      results 27, New 27
the parent            "TestRoot", a ClashTestFolder, 529 children, the child at 528 read
                      by name, resolve and check 0.001 s
TestsRemoveAt(parent, 528)                0.022 s
                      models  sets  tests  results  not New  viewpoints
before the call          4     61    529     2966       25        2847
after the call           4     61    528     2939       25        2847
after save and reopen    4     61    528     2939       25        2847
tests by that name    after the call 0, after the reopen 0
lines that differ     0 for models, sets, viewpoints and tests, after the call, after the
                      reopen, and the reopen against the call
```

1. The call took the one test and its 27 results, 2966 less 27 being 2939, and nothing else
   moved. Every one of the other 528 tests reads the same name, type, tolerance, status, result
   count, results by status and the same sha256 over its results' names and statuses in order
2. 25 results read not New before the call, after it and after the reopen, each in the same test
   with the same status by the sha256 of point 1. That they are the original's 25 Active from
   P1's run again is what 5z-k says. This run did not print the not New results by test
3. The set tree, 69 items of which 61 sets, and the viewpoint tree, 2869 items of which 2847
   viewpoints, read the same item by item in order
4. SaveFile took 10.613 s, 41,317,293 bytes read back, line 78. Document.Clear took 0.801 s and
   left 0 models and 0 tests, line 79. TryOpenFile of the saved file took 6.388 s, line 80

**BADER'S THINGS.** The NWF the copy was made from read sha256 34831A9E at the start and at the
end, lines 22 and 192. 40 registry values were put back, each read again with 0 still differing,
lines 180 and 181. InfoCenter.log and LastSession.xml were put back reading their backup's
sha256, lines 185 to 187. No AutoSave file was added, changed or gone, line 188, and the folder
listed 202 files before the run, line 18, and 202 when the prober read it after. The tool's own
logs folder had nothing added or changed, line 189. One AdskLicensingAgent, pid 45468, child of
pid 38964, read STILL RUNNING at the end, line 133. Read again by the prober after the run, no
process held pid 45468 and no AdskLicensingAgent ran.

**STILL UNKNOWN.**

- a removal from the middle. The swap was the last test at the root, so whether the tests after a
  removed one keep their results and statuses when they shift is not measured
- a removal inside a folder of tests. The parent here was the root
- whether removing a test takes the saved viewpoints made for its results. The swap had none in
  the tree, so the 2847 viewpoints say nothing about that case
- whether a test made by the XML import is removed the same way. The swap was a new ClashTest
- the close was Document.Clear inside the same Navisworks. A reopen in a new Navisworks was not
  read
- the Clash Detective panel was not read, and neither the Guids nor the comments of the other
  tests' results were compared
- the seconds in a tree of another size. One call in a tree of 529 tests was timed

**WHAT THIS DECIDES.** By the row of P2, MirrorRemover is built on TestsRemoveAt(parent, index)
with the parent read fresh and the index checked by name just before. The census out and back of
the design is still what stops a save on any difference, since a removal from the middle or from
a folder was not measured here.

## 5z-m. WHAT WORKSETS DO 1A04PK'S HV AND FP MODELS CARRY, MEASURED 2026-10-05

P4 of Q114, the views by team design, part 3. SilentMiss in F131 needs, for each model of a
group, its whole list of workset names, and the design said every 1A04PK line reads UNKNOWN
until the HV model's list and the FP model's list are read. The question: what are the workset
names of 1A04PK's HV model and of its FP model, each list whole? The answer is SilentMiss's first
real input and decides the spellings of the drafted also-ask lines.

HOW. The row names the census mode of 5t on a copy of 1A04PK's NWF, or F116's model worksets rows
of a 1A04PK run since F116 merged. No .log or .tsv under `%LOCALAPPDATA%\NwcFederatorLoop\runs` or
under steps\runs carries a model worksets row, the 1A04PK run of run set 04, run-20261004-211839,
included. So the probe was run. The census
mode's walk swallows a throw part way and says nothing of it, so a list it gives cannot be said to
be whole. The new mode `worksets` of `ViewpointProbe.dll` reads the same tab and property the
add-in's ModelFactsReader reads, the `Workset` property on the `LcRevitData_Element` tab. That is
the category and property every workset condition of the clash XML in samples asks,
`LcRevitData_Element` and `lcldrevit_parameter_-1002053`. Per model, the mode:

1. says whether the model was read from under the loop folder
2. walks every item under the model's root, counting the items, the items with geometry, the items
   with the Element tab and those with a Workset value on it, and every item whose read threw.
   Each item's read sits in its own try, and the walk's own throw is said
3. reads each value with no catch inside, by its data type, so a failed read is counted and not
   hidden
4. lists every other tab carrying a property whose display or internal name holds "workset"
5. prints each workset name in brackets with its length, the number of Element tabs carrying it,
   and any leading or trailing space or character outside printable ASCII

It calls the list WHOLE only when the walk finished, no item's read threw, and the model was read
from under the loop folder.

`tools\probes\ViewpointProbe\probe-model-worksets.ps1` is P2's `probe-test-remove.ps1` with this
mode in place of `testremove`, `-Codes` in place of `-TestName`, `-Nwf` allowed under `runs` only,
and nothing saved. The guard is the loop's, tools\loop\nw-guard.ps1 dot-sourced, with the Roamer
refusal, the settings backup, the adoption by AdoptStart's four conditions, Dispose, the close
through the held handle only when needed, and SettingsPutBack. Before the run the prober inflated
the NWF's body and read its ten model paths. All ten were absolute paths under
`%LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWC\C04`, so opening a copy reads nothing outside
the loop folder. Get-Process Roamer read 0 before the run and 0 after it. The probe copied
`%LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C04\1104-PAR-1A04PK-ZZZ-BM-MOD-000001.nwf`,
4,699 bytes, sha256 7ECA0ECA, into the new folder `probes\model-worksets-20261005-135921`, and
opened the copy.

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-model-worksets.ps1 -Out tools\probes\ViewpointProbe\p4-model-worksets-result-20261005.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C04\1104-PAR-1A04PK-ZZZ-BM-MOD-000001.nwf -Codes HV,FP

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`.
One run at 13:59, `p4-model-worksets-result-20261005.txt`, the machine name on line 1 masked by
hand as `[machine]` and nothing else changed. Navisworks pid 32088, adopted on all four conditions,
line 37. TryOpenFile of the copy returned True after 7.192 s with 10 models, every one read from
under the loop folder, lines 56 to 68. ExecuteAddInPlugin returned 0 after 34.94 s, line 48.
Dispose returned and pid 32088 was gone 6.6 s later, not forced, line 291.

**THE ANSWER.** Lines 70 to 286 of the result:

```
1104-PAR-1A04PK-ZZZ-HV-MOD-000001.nwc     LIST WHOLE: YES
  items 7884, with geometry 2992, with the Element tab 2409, with a Workset value 2409,
  items whose read threw 0, walk 3.652 s
  [ME-Ductwork]        on 2218 Element tabs
  [ME-PIPING]          on  179
  [PL-Drainage]        on   12
  3 names, each plain ASCII with no leading or trailing space

1104-PAR-1A04PK-ZZZ-FP-MOD-000001.nwc     LIST WHOLE: YES
  items 41130, with geometry 21422, with the Element tab 12127, with a Workset value 12127,
  items whose read threw 0, walk 23.962 s
  [FF-Fire Fighting]   on 12127 Element tabs
  1 name, plain ASCII with no leading or trailing space

the Workset property, in both models   display [Workset], internal
                                       [lcldrevit_parameter_-1002053], DisplayString
```

1. The HV model carries THREE workset names, `ME-Ductwork`, `ME-PIPING` and `PL-Drainage`, and no
   name starting HV-. The list is whole: every one of its 2409 Element tabs carries a Workset
   value, and no read threw
2. The FP model carries ONE workset name, `FF-Fire Fighting`, on every one of its 12127 Element
   tabs, and no name starting FP-. The list is whole on the same terms
3. The workset values the clash XML in samples asks, counted by the prober on 2026-10-05 in both
   `1104-PAR_CLASH_AllInOne_25mm.xml` and `1104-PAR_CLASH_AllInOne (2) (1).xml`, are the seven 5t
   names: `PL-Drainage` 6, `PL-Domestic Water` 6, `ME-PIPING` 5, `ME-DUCTWORK` 5, `FP-PIPING` 5,
   `FF-FIRE FIGHTING` 2, `ME-EQUIPMENT` 1. Compared as strings with the two lists:
   - `ME-PIPING` and `PL-Drainage` are carried by the HV model exactly as asked
   - `ME-DUCTWORK` is carried by the HV model as `ME-Ductwork`, the same letters in another case
   - `FF-FIRE FIGHTING` is carried by the FP model as `FF-Fire Fighting`, the same letters in
     another case
   - `FP-PIPING` is carried by neither model in any case. Under the design's 1.3, the text after
     the first hyphen, `PIPING`, matches the HV model's `ME-PIPING` case blind, and matches no
     name of the FP model
4. Other tabs carry a property named Workset too. In the FP model 5744 Level tabs, 11859 System
   Type tabs, 12127 Phase Created tabs and 1123 Symbol tabs carry one, with many Family and Type
   tabs among others. The HV model has the same kinds of tab. Their values are names such as `Shared Levels and Grids-ZZ`, `Piping System Types`, `Phase Settings` and
   `Family  : ...`. Three are names of the kind a team uses:
   - `FF-Fire Fighting` on one FamilyInstance tab of the FP model, line 91
   - `ME-Ductwork` on 12 FamilyInstance tabs of the HV model, line 210
   - `ME-Links-ZZ` on 2 RevitLinkInstance tabs of the HV model, line 213

   None of these is on the Element tab, so a workset condition of the clash XML does not read them

**BADER'S THINGS.** The NWF the copy was made from read sha256 7ECA0ECA at the start and at the
end, lines 22 and 379. 35 registry values were put back, each read again with 0 still differing,
line 368. InfoCenter.log and LastSession.xml were put back reading their backup's sha256, lines
372 to 374. No AutoSave file was added, changed or gone, line 375. The prober also listed the
AutoSave folder with each file's sha256 before the run and again after it: 202 files both times,
0 differing. The tool's own logs folder had nothing added or changed, line 376. Two
AdskLicensingAgent processes, pids 50232 and 52552, children of pid 32088, read exited, lines 321
and 322. Two others, pids 9444 and 41324, read STILL RUNNING, lines 323 and 324. They are children
of a Revit.exe, pid 37712, that the probe did not start, and the probe touched neither of them.

**STILL UNKNOWN.**

- whether a Navisworks search of `LcRevitData_Element` Workset `equals` matches across case. This
  run read the names and ran no search, so whether `ME-DUCTWORK` finds `ME-Ductwork` without a
  correction was not measured here
- whether the HV model holds items of the category a set filters on, which is F127's per-model
  count. Nothing here counted categories
- the worksets of 1A04PK's AR, EL, ME and ST models. Only HV and FP were walked
- whether these lists hold for any NWC of 1A04PK other than the run set 04 copy of 2026-10-04.
  The NWCs are files exported from Revit, and a new export can carry other worksets
- whether a model of another group with the code HV or FP carries the same names

**WHAT THIS DECIDES.** By the row of P4, these two lists are SilentMiss's first real input for
1A04PK, each list whole. The HV model carries ME- and PL- names and the FP model carries one FF-
name, so the also-ask spellings for these two models are drafted from the names above. Which
lines are drafted is the rule's work in F131, and is not decided here.

## 5z-n. WHICH SAVED VIEWPOINTS OF THE BASELINE NWF ARE F85'S, MEASURED 2026-10-05

P8 of Q114, the views by team design, part 3, read only. F114 removes the per-clash views F85
wrote, and those carry no mark, so LegacyClashView of the design's 1.9 tells them by their folders
and their name alone. The question: does the saved viewpoint tree of the baseline's 1A02MM NWF hold
2847 viewpoints, 2813 under code pair folders with leaves named test, two spaces, Clash and digits,
and 34 elsewhere? The dump gives every item's folder path, name, folder or viewpoint, comment count
and Guid. By the row of P8, exactly 2813 legacy and none of the 34 lets LegacyClashView's test run
over the committed dump. Any other count stops the legacy removal until the rule is fixed. It also
counts the leaves under Over 150mm per test.

HOW. `tools\probes\ViewpointProbe\probe-viewpoint-tree.ps1` is P4's `probe-model-worksets.ps1` with
the new mode `vptree` of `ViewpointProbe.dll` in place of `worksets` and `-Dump` in place of
`-Codes`. The guard is the loop's, tools\loop\nw-guard.ps1 dot-sourced, with the Roamer refusal,
the settings backup, the adoption by AdoptStart's four conditions, Dispose, the close through the
held handle only when needed, and SettingsPutBack. Get-Process Roamer read 0 before the run and 0
after it. The probe copied the C02 NWF of run set 04,
`%LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`,
41,317,271 bytes, sha256 0944C100, into the new folder `probes\viewpoint-tree-20261005-142446`, and
the mode, on that copy:

1. opens the copy and says for each model whether it was read from under the loop folder
2. reads the name of every ClashTest in the test tree, root and folders
3. walks the whole saved viewpoint tree from `SavedViewpoints.RootItem`, each item in its own try,
   and writes one dump row per item: index path, depth, folder or viewpoint, child count,
   `Comments.Count`, `SavedViewpoint.Redlines.Size()` on a viewpoint, `SavedItem.Guid`, the legacy
   verdict, the first condition failed, folder path and name. A tab, a line break, any other
   control character and a backslash are written as \uXXXX, so one row stays one row
4. judges every item by the five conditions of 1.9 as written there, with F85's defaults read off
   src\Federator.Core\Views: the priority words A, B, C and No priority as an optional first
   folder, then a pair folder of two of AR, ST, ME, FF, PL, DR, EL and UNKNOWN with " vs " between
   and the first not after the second Ordinal, then an optional Over 150mm, at depth 1 to 3. The
   item a viewpoint. The name a test name, two spaces, Clash and digits only. The test name one of
   the document's own. No comment and no redline
5. saves nothing. The script copies the dump out of the work folder and reads its sha256 back

This is the probe's own reading of 1.9's text. LegacyClashView is not written yet, so its test
has not run over the dump. Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-viewpoint-tree.ps1 -Out tools\probes\ViewpointProbe\p8-viewpoint-tree-result-20261005.txt -Dump tools\probes\ViewpointProbe\p8-viewpoint-tree-dump-20261005.tsv -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`.
One run at 14:24, `p8-viewpoint-tree-result-20261005.txt`, the machine name on line 1 masked by hand
as `[machine]` and nothing else changed. The dump is `p8-viewpoint-tree-dump-20261005.tsv`, 2869
rows and a header, 376,451 bytes, sha256 6B9D1DCE, line 214. Navisworks pid 13668, adopted on all
four conditions, line 37. TryOpenFile of the copy returned True after 6.362 s with 4 models, every
one read from under the loop folder, lines 55 to 60. ExecuteAddInPlugin returned 0 after 6.55 s,
line 48. Dispose returned and pid 13668 was gone 6.1 s later, not forced, line 218.

**THE ANSWER IS YES.** Lines 61 to 212 of the result:

```
tests in the document 528, distinct names 528
the root              a FolderItem with 16 children
items 2869            folders 22, viewpoints 2847, other kinds 0, reads that threw 0
viewpoints by depth   2780 at depth 1, 67 at depth 2
comments              0 on every item, 0 counts that threw
redlines              0 on every viewpoint, 0 reads that threw
Guids                 2869 read, every one 00000000-0000-0000-0000-000000000000
LEGACY BY THE RULE OF 1.9: 2813, NOT LEGACY: 56 of which viewpoints 34 and folders 22
   not legacy, 1 no sorted code pair folder where one belongs: 34
   not legacy, 2 not a viewpoint: 22
P8 YES   viewpoints 2847 against 2847, legacy 2813 against 2813, viewpoints not legacy 34 against 34
```

1. The tree holds 2847 viewpoints. 2813 are legacy by the rule, equal to the 2813 the baseline
   created, log line 438 of `runs\04\NMFed\NWF\C02\run-20261004-185652.log`, read by the prober.
   The prober also recounted the dump on its own after the run: 2813 viewpoint rows whose name
   reads test, two spaces, Clash and digits, all of them legacy, 34 viewpoints and 22 folders not
2. The 34 that are not legacy sit in four folders at the root the NWCs brought, lines 122 to 155:
   `PAR-AR-VEW-3D View` 11, `3D View` 8, `MEC-MEC-3D VIEW-PAR` 9 and `PAR-ST-3D View` 6. Each fails
   condition 1, a folder that is not a sorted code pair. None of them is legacy
3. The 22 folders are the 4 above, 12 pair folders at the root and 6 Over 150mm folders, one
   below each of 6 pairs, lines 77 to 98. The 2813 sit in 16 of them, lines 104 to 119, since
   ME vs ME and ME vs PL hold only their Over 150mm folder:
   AR vs AR 2617, EL vs ST 56 and 42 Over 150mm, ST vs ST 33, AR vs DR 17, EL vs EL 2 and 9 Over
   150mm, AR vs ME 8 and 8 Over 150mm, EL vs UNKNOWN 6 and 1 Over 150mm, ME vs ME 0 and 6 Over
   150mm, ST vs UNKNOWN 3, DR vs DR 2, DR vs PL 2, ME vs PL 0 and 1 Over 150mm. No priority folder,
   since the baseline picked no priority file, log line 426
4. Over 150mm holds 67 legacy viewpoints of 17 tests, lines 158 to 209. Each of those 17 tests has
   all its legacy viewpoints there and none in its pair folder, so each test's views sit in one
   folder: 51 tests hold legacy viewpoints, and there are 51 distinct pairs of test and folder,
   line 210. BLD-AR-Curtain Mullions-vs-BLD-AR-Windows alone holds 2568 of them, line 159
5. Every name of the tree is free of a leading or trailing space, line 72
6. EVERY GUID READ IS THE EMPTY GUID. `SavedItem.Guid` returned 00000000-0000-0000-0000-000000000000
   on all 2869 items, folders and viewpoints alike, of a document read off this NWF, line 71. The
   read threw on none. This is the read through the .NET SavedItem only. Nothing here asked
   ResolveGuid or the COM view for an id
7. The walk of 2869 items took 0.038 s, line 63

**BADER'S THINGS.** The NWF the copy was made from read sha256 0944C100 at the start and at the
end, lines 22 and 306. 36 registry values were put back, each read again with 0 still differing,
line 295. InfoCenter.log and LastSession.xml were put back reading their backup's sha256, lines 299
to 301. No AutoSave file was added, changed or gone, line 302. The prober also listed the AutoSave
folder with each file's sha256 before the run and again after it: 202 files both times, 0
differing. The tool's own logs folder had nothing added or changed, line 303. One
AdskLicensingAgent, pid 22100, child of pid 13668, read STILL RUNNING at the end, line 250. Read
again by the prober after the run, no process held pid 22100. The two AdskLicensingAgent processes
still running, pids 9444 and 41324, are the Revit children 5z-m names, and the probe touched
neither.

**STILL UNKNOWN.**

- whether LegacyClashView, once written, gives the same 2813. This is the probe's reading of 1.9's
  text, and the test over the dump runs only when the class exists
- condition 4 read the document's test names only. The test names of the picked XML were not read
- whether a viewpoint's Guid is empty in a document where the views were just made and not yet saved,
  and whether a Guid set by the tool survives a save. P10 asks that. Here every Guid read empty
  after the open, so a Guid read this way cannot tell two items of this tree apart
- whether a priority folder tree, from a run with a priority file, reads the same. The baseline had
  none, so condition 1's first folder was never met
- whether redlines of zero here mean no redline was drawn or that Size() reads 0 on a document read
  off an NWF. P20 asks that
- the Clash Detective panel and the Saved Viewpoints window were not read

**WHAT THIS DECIDES.** By the row of P8, the count is the one the row asks: 2847 viewpoints, 2813
legacy and none of the 34 the NWCs brought. The dump is committed for LegacyClashView's test to run
over, and that test has to read exactly 2813 legacy and 34 not, or the legacy removal stops until
the rule is fixed. The 67 leaves under Over 150mm in 17 tests are part 6's lower bound. The empty
Guid on every item is new, and goes to P10 and P11 before the Guid is put in the mark.

## 5z-o. DOES A COMMENT ON A SAVED VIEW AND ON A FOLDER SURVIVE A SAVE AND A REOPEN, MEASURED 2026-10-05

P9 of Q114, the views by team design, part 3. F114 marks every view and folder it makes with a
comment, so a later run knows its own. Nothing had read whether a comment written on a saved
viewpoint, rather than on a clash result, is kept. The question: does a comment written by
DocumentSavedViewpoints.AddComment, or put on the COM view before the add where P6 said yes, on a
viewpoint two folders deep and on a folder, read back with the same body and author off
SavedItem.Comments after a save, a clear and a reopen, do the view's Hidden count and
MaterialOverrides count read the same before and after the edit, and how many seconds does the
write take? Yes: the mark is the comment. The counts change: the mark is written before the read
back. No: B7 is put to Bader.

HOW. `tools\probes\ViewpointProbe\probe-view-comments.ps1` is P8's `probe-viewpoint-tree.ps1` with
the new mode `vpcomment` of `ViewpointProbe.dll` in place of `vptree` and no `-Dump`. The guard is
the loop's, tools\loop\nw-guard.ps1 dot-sourced, with the Roamer refusal, the settings backup, the
adoption by AdoptStart's four conditions, Dispose, the close through the held handle only when
needed, and SettingsPutBack. Get-Process Roamer read 0 before the run and 0 after it. The probe
copied the C02 NWF of run set 04,
`%LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`,
41,317,271 bytes, sha256 0944C100, into the new folder `probes\view-comments-20261005-144832`, and
the mode, on that copy:

1. opens it, hides model 0's root and paints the first clash pair with geometry red and green, so
   the new views record a hidden state and colours
2. makes the folders `P9 probe` and `P9 probe / P9 sub` the way the tool does, a .NET FolderItem by
   AddCopy, the parent read fresh
3. writes through the COM view, ApplyHideAttribs and ApplyMaterialAttribs true, into `P9 sub`, so
   two folders deep: V1 with a comment made by `ObjectFactory(eObjectType_nwOpComment)`, its Body
   set and then its User set, added to the view's `Comments()` before `InwSavedViewsColl.Add`. V2
   and V3 with no comment
4. writes a COM folder view `P9 com folder` into `P9 probe` with a comment in its `Comments()`
   before the add, F2
5. writes `AddComment(item, comment)`, the comment made by
   `Document.CreateCommentWithUniqueId(body, CommentStatus.New, "Parsons NWC Federator")`, on V2
   after its add, on the folder `P9 probe / P9 sub`, F1, and on the existing F85 viewpoint
   `AR vs ME / Over 150mm / BLD-ME-Ducts&Duct Fittings-vs-BLD-AR-Walls  Clash1`, L1, the first one
   two folders deep. Each item is re-found by its names from a fresh RootItem, and a viewpoint's
   Hidden and MaterialOverrides counts are read just before and just after the call. V3 is the
   control for V1
6. every body is the design's sentence, a line feed, and a marker line naming the stamp, the
   folder path, the name and the target. Each is read back off SavedItem.Comments and compared
   Ordinal, body and author, before any save, and again after SaveFile into
   `p9-copy-with-comments.nwf` in the work folder, Document.Clear and TryOpenFile of that file

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-comments.ps1 -Out tools\probes\ViewpointProbe\p9-view-comments-result-20261005.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`.
One run at 14:48, `p9-view-comments-result-20261005.txt`, the machine name on line 1 masked by hand
as `[machine]` and nothing else changed. Navisworks pid 596, adopted on all four conditions, line
37. ExecuteAddInPlugin returned 0 after 67.87 s, line 48. Dispose returned and pid 596 was gone
6.9 s later, not forced, line 161.

**THE ANSWER IS YES, BY ADDCOMMENT. THE COM ROUTE BEFORE THE ADD DOES NOT KEEP THE BODY AND
AUTHOR.** Lines 145 to 152 and 155 of the result, with the time cut from the start of each:

```
EACH TARGET:  label | route | written | write seconds | read back the same before the save | after the reopen | Hidden before edit, after edit, before save, after reopen | MaterialOverrides the same four
   V1 | the COM view's Comments() before the add | True | 0.001 s | False | False | -2, -2, 1, 1 | -2, -2, 2, 2
   V2 | AddComment after the add | True | 0.003 s | True | True | 1, 1, 1, 1 | 2, 2, 2, 2
   V3 | no comment, the control | True | UNKNOWN | True | True | -2, -2, 1, 1 | -2, -2, 2, 2
   F1 | AddComment on a folder one below the root folder | True | 0.000 s | True | True | a folder
   F2 | the COM folder view's Comments() before the add | True | 0.000 s | False | False | a folder
   L1 | AddComment on an existing viewpoint two folders deep | True | 0.000 s | True | True | 2, 2, 2, 2 | 4697, 4697, 4697, 4697
(-2 is not read, -1 is a read that threw)
P9 YES   a comment on a view two folders deep and on a folder read back with the same body and author after a save, a clear and a reopen, by at least one route each, with the counts held
```

1. AddComment kept the comment on all three items it was given: the new view two folders deep,
   the existing F85 view two folders deep and the folder. Each read back exactly one comment, its
   Body and Author equal to what was written, Ordinal, the line feed inside the body included, and
   Status New. That held before the save, lines 90 to 110, and after the save, the clear and the
   reopen, lines 122 to 142
2. AddComment left the counts alone. V2 read Hidden 1 and MaterialOverrides 2 before the call,
   after it, before the save and after the reopen. L1 read Hidden 2 and MaterialOverrides 4697 at
   all four
3. The write is cheap. AddComment returned after 0.003 s on V2 and under 0.001 s on F1 and L1,
   lines 76, 79 and 81. Making the comment and adding it to the COM view's collection took
   0.001 s on V1
4. THE COM ROUTE. The COM comment collection of a new view and of a new folder view read Count 0
   and ReadOnly False, lines 66 and 73. The factory's object is InwOpComment, InwOpComment2 and
   InwOpComment3, line 67. The comment was kept through the add, the save and the reopen, one
   comment each on V1 and F2. But it read back Body `Parsons NWC Federator`, the string given to
   User, and Author `p003653K`, a value no line of the probe wrote, which reads as this machine's
   login name, lines 86 to 87, 102 to 103, 118 to 119 and 134 to 135. The COM read of V1's
   comment after the add said the same, User `p003653K` and Body `Parsons NWC Federator`, line 70.
   The body the probe set never read back on either. V1 read the same Hidden and
   MaterialOverrides counts as the plain V3, 1 and 2
5. The tree went from 2847 viewpoints, 22 folders and 0 comments to 2850, 25 and 5, and read the
   same after the reopen, lines 61, 111 and 143. The five comments read Id 1 to 5 in the order they
   were made, the two COM ones included, and kept their Ids through the reopen
6. SaveFile took 12.823 s, 41,319,461 bytes read back, line 112. Document.Clear took 0.985 s,
   line 113. TryOpenFile of the saved file took 8.084 s, line 114. The saved copy is
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf`,
   sha256 869DD965B03E5A82B6E3AE3DF7E70B4F2462B752B698DFA1F178AD6FCF9A0921, line 157, the copy
   P10, P11 and P12 work on

A READ THIS PROBE DID NOT ASK FOR. Each COM add into a folder took about 9 s here: V1's
`InwSavedViewsColl.Add` 9.490 s, line 68, F2's 8.993 s, line 74, and V2 and V3 about 9.1 s and
9.2 s each, read off the timestamps of lines 69 to 72, those two with the view's making included.
5p measured 7.9 ms a view for twenty, and quoted the dimming round's 558 ms a view for 430 on
1A02MM. Why these took about 9 s each, in a document of 2847 viewpoints with model 0's root
hidden, is UNKNOWN. It bears on P18's rate and on part 6.

**BADER'S THINGS.** The NWF the copy was made from read sha256 0944C100 at the start and at the
end, lines 22 and 267. 40 registry values were put back, each read again with 0 still differing,
lines 255 and 256. InfoCenter.log and LastSession.xml were put back reading their backup's sha256,
lines 260 and 261. No AutoSave file was added, changed or gone, line 263. The prober also listed
the AutoSave folder with each file's sha256 before the run and again after it: 202 files both
times, 0 differing. The tool's own logs folder had nothing added or changed, line 264. Two
AdskLicensingAgent processes were children of pid 596, lines 172 and 173. Pid 39832 read exited,
line 207. Pid 40800's start time could not be read, so the probe said UNKNOWN whether it exited,
line 206. Read again by the prober after the run, no process held pid 40800. The two
AdskLicensingAgent processes still running, pids 9444 and 41324, are the ones 5z-n names, and the
probe touched neither.

**STILL UNKNOWN.**

- why the COM route reads back the User string as the body and the login name as the author. One
  order was tried, Body set and then User. Whether another order, or InwOpComment3's own members,
  keep the body was not measured
- whether a comment survives a reopen in a new Navisworks. The close was Document.Clear inside the
  same Navisworks
- whether AddComment on a folder at the root, or on a folder holding thousands of views, behaves
  the same. F1 was one folder below a root folder, holding three views
- the Comments window and the Saved Viewpoints window were not read, so what a person sees there
  is not measured
- the seconds of AddComment over many views. Three calls were timed, each one at most 0.003 s
- what P10 asks of the Guid. This probe did not read it
- why each COM add took about 9 s here

**WHAT THIS DECIDES.** By the row of P9, the mark is the comment, written by
DocumentSavedViewpoints.AddComment after the add with a comment from
Document.CreateCommentWithUniqueId. The COM route before the add, which P6 made the first try, is
not used, because the body written never read back. The counts held, so the mark need not be
written before the read back. B7 is not put to Bader.

## 5z-p. DOES A SAVED VIEWPOINT'S GUID HOLD THROUGH A COMMENT, A SAVE AND A REOPEN, MEASURED 2026-10-05

P10 of Q114, the views by team design, part 3. P8 read the empty Guid on every item of the
baseline's tree, 5z-n, and asked whether a Guid is empty before a save and whether one set by the
tool survives a save. The question: does a viewpoint's SavedItem.Guid read the same after the
comment edit and after a save and a reopen, is it unique in the tree, and does
DocumentSavedViewpoints.ResolveGuid return the item? Yes: the Guid goes in the mark and removal
re-finds by it. No: the Guid stays out, and removal re-finds by path, name and mark.

HOW. `tools\probes\ViewpointProbe\probe-view-guids.ps1` is P9's `probe-view-comments.ps1` with the
new mode `vpguid` of `ViewpointProbe.dll` in place of `vpcomment`, and `-Nwf` taken from under
`%LOCALAPPDATA%\NwcFederatorLoop\probes` in place of `runs`, because the row works on P9's saved
copy. The guard is the loop's, tools\loop\nw-guard.ps1 dot-sourced, with the Roamer refusal, the
settings backup, the adoption by AdoptStart's four conditions, Dispose, the close through the held
handle only when needed, and SettingsPutBack. Get-Process Roamer read 0 before the run and 0 after
it. The probe copied P9's
`%LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf`,
41,319,461 bytes, sha256 869DD965, into the new folder `probes\view-guids-20261005-152023`, and the
mode, on that copy:

1. opens it, walks the whole saved viewpoint tree and counts every item's Guid, the empty ones and
   the ones more than one item carries, and reads the Guid, the index path by CreateIndexPath and
   ResolveGuid of the Guid on six items P9 left: E1 and E2 the COM views `P9 view addcomment after
   add` and `P9 view plain` two folders deep, E3 the .NET folder `P9 probe / P9 sub`, E4 the COM
   folder `P9 probe / P9 com folder`, E5 the .NET folder `P9 probe` at the root, E6 F85's view
   `AR vs ME / Over 150mm / BLD-ME-Ducts&Duct Fittings-vs-BLD-AR-Walls  Clash1`. Then
   ResolveGuid of the empty Guid
2. makes five new items, each read back right after its add, re-found by its names from a fresh
   RootItem: N1 the folder `P10 probe` at the root by FolderItem and AddCopy, the Guid untouched,
   the tool's folder route. N2 a FolderItem whose Guid the probe set to Guid.NewGuid() before
   AddCopy. N3 a COM view by InwSavedViewsColl.Add into `P10 probe`, ApplyHideAttribs and
   ApplyMaterialAttribs true, the tool's view route of 5m. N4 a .NET `new SavedViewpoint(Viewpoint)`
   by AddCopy, the Guid untouched. N5 the same with its Guid set to Guid.NewGuid() before AddCopy
3. writes one comment on each of the eleven by DocumentSavedViewpoints.AddComment, the comment
   from Document.CreateCommentWithUniqueId as P9 made it, and reads each Guid again
4. walks the tree again and reads every item with ResolveGuid, saves into `p10-copy-saved.nwf` in
   the work folder, calls Document.Clear and TryOpenFile of the saved file, and walks and reads
   every item with ResolveGuid once more. ResolveGuid counts as giving the item only when what it
   returns has the same name, Ordinal, and the same index path, and the Guid is not empty

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-guids.ps1 -Out tools\probes\ViewpointProbe\p10-view-guids-result-20261005.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`.
One run at 15:20, `p10-view-guids-result-20261005.txt`, the machine name on line 1 masked by hand
as `[machine]` and nothing else changed. Navisworks pid 37232, adopted on all four conditions,
line 37. TryOpenFile of the copy returned True after 9.866 s with 4 models, every one read from
under the loop folder, lines 55 to 60. ExecuteAddInPlugin returned 0 after 51.11 s, line 48.
Dispose returned and pid 37232 was gone 7.2 s later, not forced, line 164.

**THE ANSWER IS NO ON THE TOOL'S ROUTES. A GUID HOLDS ONLY WHERE THE TOOL SETS IT BEFORE AN
ADDCOPY.** Lines 146 to 158 of the result, each Guid cut to its first 8 characters, each route shortened and\nthe six rows of E1 to E6 joined into one, since they read alike:

```
EACH TARGET:  label | the Guid at each stage | the same at every stage | not empty | unique before the save, after the reopen | ResolveGuid gave the item before the save, after the reopen | the set Guid kept | all
   E1 to E6, P9's items and F85's view | open, edit, save, reopen all 00000000 | YES | NO | NO, NO | NO, NO | not set | NO
   N1 .NET folder by AddCopy, the tool's folder route | add, edit, save, reopen all 00000000 | YES | NO | NO, NO | NO, NO | not set | NO
   N2 .NET folder, its Guid set before AddCopy | add, edit, save, reopen all b672e45a | YES | YES | YES, YES | YES, YES | YES | YES
   N3 COM view by InwSavedViewsColl.Add, the tool's view route | add, edit, save, reopen all 00000000 | YES | NO | NO, NO | NO, NO | not set | NO
   N4 .NET SavedViewpoint by AddCopy, the Guid untouched | add, edit, save, reopen all 00000000 | YES | NO | NO, NO | NO, NO | not set | NO
   N5 .NET SavedViewpoint, its Guid set before AddCopy | add, edit, save, reopen all 8e38aecf | YES | YES | YES, YES | YES, YES | YES | YES
P10 by route: the tool's view route, a COM view, NO. The tool's folder route, a .NET folder by AddCopy, NO. Any item of any route YES
P10 NO
```

1. THE TREE. At the open: 2875 items, 2875 empty Guids, 0 reads threw, line 63. That is P9's 2850
   viewpoints and 25 folders. Before the save: 2880 items, 2878 empty, 2 distinct Guids that are
   not empty and none carried by two items, line 115. After the reopen the same, line 132. The
   two that are not empty are N2's and N5's
2. AN ITEM NOBODY GAVE A GUID HAS NONE, BEFORE A SAVE TOO. N1, N3 and N4 read the empty Guid right
   after their add, before any save, lines 75, 81 and 84. A new FolderItem and a new
   SavedViewpoint read the empty Guid before their AddCopy too, lines 73, 82 and 85. So the empty
   Guid P8 read is not a loss in the NWF. No item is given one by AddCopy, by the COM add, by
   AddComment, by SaveFile or by the reopen
3. A GUID SET BEFORE ADDCOPY HOLDS. N2 and N5 read back the Guid the probe set, off the item
   before the add, lines 76 and 86, and off the document item after the add, lines 78 and 88,
   after AddComment, lines 106 and 112, before the save, lines 123 and 126, and after the save, the
   clear and the reopen, lines 140 and 143. Each was carried by exactly 1 item of the tree, and
   ResolveGuid returned that item, same name and same index path, 17.0 and 17.3, before the save
   and after the reopen, each call under 0.001 s
4. RESOLVEGUID OF THE EMPTY GUID RETURNS NULL, line 70, and returned null for every item whose Guid
   is empty, at every stage, lines 64 to 69, 116 to 122, 124, 125, 133 to 139, 141 and 142. It never threw
5. THE COMMENT EDIT MOVES NO GUID. On all eleven items the Guid read just before AddComment and
   just after it are equal, lines 91 to 112. Each AddComment took at most 0.008 s
6. A READ THE ROW DID NOT ASK. `CreateReference(item).SavedItemId` reads the item's folder names
   and its own name joined by line feeds, such as `P9 probe\u000AP9 sub\u000AP9 view plain`, on
   every item at every stage, lines 64 to 143. It is a path of names, not an id
7. A READ THE ROW DID NOT ASK. N3's InwSavedViewsColl.Add into a folder holding one folder took
   12.744 s, line 80, in a document of 2875 items. P9's took about 9 s each. Why is UNKNOWN

THE REFLECTION. Read in this session off
`C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll`: SavedItem has a
public `Guid` with a getter and a setter, and `CreateCopy()` and `CreateUniqueCopy()`.
DocumentSavedViewpoints has `ResolveGuid(Guid)`, `CreateReference(SavedItem)`,
`ResolveReference(SavedItemReference)`, `CreateIndexPath(SavedItem)`, `ResolveIndexPath`,
`EditDisplayName` and `EditComments`, and no member that sets the Guid of an item already in the
document. That is the member list, not a run.

SaveFile took 16.653 s, 41,319,767 bytes read back, line 127. Document.Clear took 1.412 s, line
128. TryOpenFile of the saved file took 10.149 s, line 129. The saved copy is
`%LOCALAPPDATA%\NwcFederatorLoop\probes\view-guids-20261005-152023\p10-copy-saved.nwf`, sha256
3CC51A5A3B4F8F027090FCF3EA1313D0D74C67747EB679D94D0FF9826D3DA75E, line 160.

**BADER'S THINGS. HIS SETTINGS WERE NOT PUT BACK.** The NWF the copy was made from read sha256
869DD965 at the start and at the end, lines 22 and 265. No AutoSave file was added, changed or
gone, line 261. The prober also listed the AutoSave folder with each file's sha256 before the run
and again after it: 202 files both times, 0 differing. The tool's own logs folder had nothing
added or changed, line 262. But the guard's PutBackReasons gave one reason, line 213: `Roamer 37232
was seen at a watchdog pass with no readable start time, and cannot be shown to be the adopted
one`. So by the guard's rule nothing was put back and nothing was written, lines 212 to 260, and
the backup is kept in `probes\view-guids-20261005-152023`, the registry export
`hkcu-navisworks-manage-22.0-before.reg` and the folder `appdata-before`. 40 registry values differ
from the backup, lines 215 to 255: the four AutoRecover values, four CER counters, MainWindow
Placement, PluginOptions DefaultPlugin and the 30 values of the ten Recent File List entries.
InfoCenter.log and LastSession.xml differ, lines 257 and 258. The watchdog's record holds one
Roamer only, pid 37232: seen with no readable start time on a pass whose line was written at
15:20:33.643, line 172, and at the next pass as new, started 15:20:33.620 with -Embedding, line
173, the process adopted. The guard skips such a sighting only when the time of the pass that saw
it is at or after the adopted start. Read off the guard's code, that time is taken at the start of
the pass, before its process list, so a pass that began before 15:20:33.620 and listed the process
after would fail the check. That this is what happened is the prober's reading of the code, and it
is UNKNOWN from the record, which writes the line's time and not the pass's. Two AdskLicensingAgent
processes were children of pid 37232, lines 176 and 177. Pid 52792 read exited, line 205. Pid
53748's start time could not be read, so the probe said UNKNOWN whether it exited, line 204. Read
again by the prober after the run, no process held pid 53748. The two AdskLicensingAgent processes
still running, pids 9444 and 41324, are the ones 5z-n names, and the probe touched neither.

**STILL UNKNOWN.**

- whether the tool's COM view can carry a Guid at all. N3 read empty, nothing on the COM view was
  tried to set one, and the DLL has no member that sets the Guid of an item already in the document.
  Whether a COM view copied by the .NET API, given a Guid and put back by ReplaceWithCopy, keeps
  its camera, its hidden state and its colours was not measured
- whether a Guid set before AddCopy survives a reopen in a new Navisworks. The close was
  Document.Clear inside the same Navisworks
- whether a copy of an item with a Guid, by AddCopy of SavedItem.CreateCopy, carries the same
  Guid. P11 asks that, and it bears on uniqueness, because two items with one Guid are then
  possible
- what ResolveGuid returns when two items carry one Guid. No such tree was read
- the Saved Viewpoints window was not read
- why Bader's settings could not be put back beyond the reading of the code above, and whether
  they are put back. That is the lead's, from the backup kept in the work folder
- why each COM add into a folder takes 9 to 13 s here

**WHAT THIS DECIDES.** By the row of P10, NO: on the tool's own routes, a COM view and a .NET
folder by AddCopy, the Guid reads empty at every stage and ResolveGuid returns null, so the Guid
stays out of the mark and removal re-finds by path, name and mark. The new fact for the design is
that a Guid the tool sets on a .NET item before AddCopy holds through the comment, the save and
the reopen, stays unique and resolves. It could carry a folder's identity, since folders are
written by AddCopy. It cannot carry a view's while views are written through COM.

## 5z-r. HOW OFTEN A MIRROR FINDS MORE ON 1A02MM, AND WHAT RUNNING BOTH COSTS, MEASURED 2026-10-05

Q133, probe Q133-1A02MM. Bader's answer D to Q133 keeps both tests of a mirrored pair, runs both
and merges their clashes by the pair of items, and asks: "Measure on 1A02MM and 1A04PK how often a
mirror finds more, and the extra time running both costs, and give both in the next record." This
section is 1A02MM. The question: on 1A02MM, how many of the pairs F132's rule finds would both be
created and what does each find, and over every test that finds a clash, how often does its swap
find more, fewer or other clashes and what do the two runs cost against the original's alone? A
swap that often finds more makes answer D worth its seconds. A swap that never finds more makes it
time for nothing on this building. The letter 5z-q is left to P11, whose unrun probe in this
worktree already names it.

HOW. Three parts.

1. THE RULE'S PAIRS, read off the XMLs with no Navisworks. `tools\probes\ViewpointProbe\q133-rule-pairs.py`
   reads each set's locator and its findspec, its rule list, with every text trimmed, and pairs two
   tests whose sides are the same two sets swapped, or the same two sets in one order, or, by
   Bader's answer B to Q121, the same two rule lists over other sets. It also names a test with one
   set or one rule list on both sides. It read the picked XML,
   `%LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`, sha256
   792B01FB, and the corrected one the rule writes, `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml`,
   sha256 94897667, whose list `.corrections.txt` sits beside it:

       python tools\probes\ViewpointProbe\q133-rule-pairs.py %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A02MM-rule-pairs.txt %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml

   Its output is kept as `q133-1a02mm-rule-pairs-result-20261005.txt`.
2. EVERY TEST THAT FINDS A CLASH, inside Navisworks. `tools\probes\ViewpointProbe\probe-mirror-count.ps1`
   is P1's `probe-mirror-swap.ps1` with the new mode `mirrorcount` of `ViewpointProbe.dll` and the
   pairs file in place of a test name, under the same guard, tools\loop\nw-guard.ps1 dot-sourced:
   the Roamer refusal before anything and again before the constructor, the settings backup, the
   adoption by AdoptStart's four conditions, Dispose, the close through the held handle only when
   needed, and SettingsPutBack. Get-Process Roamer read 0 before the run. It copied the C02 NWF of
   run set 04, `runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`, 41,317,271 bytes,
   sha256 0944C100, which holds the 528 tests of set 04 with their results, into the new folder
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\mirror-count-20261005-160417`. The mode opens the copy,
   reads every test's stored results, and runs part 1's pairs whose two tests are both in the NWF.
   Then, for every test whose stored results hold at least one clash that is not Resolved, it adds
   a new ClashTest with the sides swapped the way P1 made its swap, the original's type, tolerance,
   merge composites and simulation type, side A CopyFrom the original's side B and side B from its
   side A, with each side's self intersect and primitive types, appended at the root and checked
   by name, and reads its two sides' set names back to check they are the original's swapped. It
   clears the swap's results, runs the original with TestsRunTest, then the swap, each call timed
   alone with a Stopwatch, and compares the clashes not Resolved of the two by the unordered pair of
   the index paths of Item1 and Item2, as P1 did. A test's verdict is same, swap finds more (pairs
   only in the swap and none only in the original), swap finds fewer, or other clashes (pairs only
   in each)
3. EVERY OTHER TEST, the same, beyond the brief, since a swap of a test that finds nothing could
   find something

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-mirror-count.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A02MM-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf -PairsFile %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A02MM-rule-pairs.txt -PluginAssembly %LOCALAPPDATA%\NwcFederatorLoop\turn5\q133-1A02MM-build\out\ViewpointProbe.dll

with the probe built by `dotnet build ViewpointProbe.csproj -c Release -o out` in a copy of the
project at `%LOCALAPPDATA%\NwcFederatorLoop\turn5\q133-1A02MM-build`, because this worktree's
ViewpointProbePlugin.cs held P11's uncommitted lines. The source committed here is that copy's,
line for line, and the DLL's sha256 is 5D3481EF, line 24. One run at 16:04, kept as
`q133-1a02mm-mirror-count-result-20261005.txt`, the machine name on line 1 masked by hand as
`[machine]` and nothing else changed. Navisworks pid 52324, adopted on all four conditions, lines
36 and 38. TryOpenFile of the copy returned True after 7.651 s with 4 models, every one read from under
the loop folder, lines 55 to 60.

THE RUN WAS STOPPED BY THE GUARD AT ITS DEADLINE. Every TestsRunTest took about 9 to 11 s, so part
3 could not finish inside the adopted deadline of 3600 s. At 17:05:53 the watchdog closed pid 52324
through the held handle, lines 336 and 1087, ExecuteAddInPlugin threw, line 49, and step 6, Dispose,
was not reached. Parts 1 and 2 had finished and written their totals. Part 3 had run 123 of its 469
tests, lines 209 to 331, and wrote no total. The copy with the swaps was not saved.

**PART 1, THE RULE'S PAIRS: NONE CREATED ON 1A02MM.** Lines 1 to 8 of the pairs file, and lines 64
to 134 of the result:

```
                                        picked XML 792B01FB   corrected XML 94897667
tests, sets                             1830, 61              1830, 61
pairs by swapped or repeated sets       0                     0
pairs by the same rule lists            59                    59
tests with one rule list on both sides  1                     1
sets sharing one rule list              BLD-EL-Telecom Fixtures and BLD-EL-Telephone Devices, both
on 1A02MM                               every one of the 119 tests named is in the NWF 0 times,
                                        so not one pair has both tests created and none was run
```

So F132's rule, with Bader's answer B to Q121, finds 59 pairs and one self test in this XML, all
of them over the Telecom and Telephone sets, and on 1A02MM none of them is created. They cost 0 s
and find nothing here. That matches turn5\measure-mirrors.md, read only on 2026-10-04.

**PART 2, EVERY TEST THAT FINDS A CLASH: A SWAP FINDS MORE ON 4 OF 59 AND OTHER CLASHES ON 1.**
Lines 204 to 206 of the result:

```
tests whose stored results hold a clash   59, holding 2939 clashes
the swap finds the same                   54
the swap finds more                       4
the swap finds fewer                      0
the swap finds other clashes              1
UNKNOWN                                   0
clashes, the originals run again          2939, every test the same pairs as stored
clashes, the swaps                        2945
only the swap finds                       7
only the original finds                   1
TestsRunTest, the 59 originals            574.530 s
TestsRunTest, the 59 swaps                570.918 s
both                                      1145.449 s, 1.994 times the originals alone
making the 59 swaps                       1.049 s
```

The five tests where the two differ, lines 142 to 197:

```
test                                                    original  swap  only swap  only original
BLD-ST-Framing-vs-BLD-ST-Columns                              25    27          2              0
BLD-EL-Conduits & Conduit Fittings-vs-BLD-ST-Framing          40    41          1              0
BLD-EL-Conduits & Conduit Fittings-vs-BLD-ST-Walls             6     8          2              0
BLD-EL-Conduits & Conduit Fittings-vs-BLD-ST-Floors           17    18          1              0
BLD-EL-Fire Alarm Devices-vs-BLD-EL-Lighting Fixtures          1     1          1              1
```

1. BLD-ST-Framing-vs-BLD-ST-Columns finds 25 and its swap 27, the same two extra pairs at the same
   distances P1 read in 5z-k, lines 143 and 144. So P1's finding repeats in a second run
2. Every clash only a swap finds has a distance between -0.287 ft and -1.804 ft, lines 143, 144,
   180, 182 to 185 and 197, past the tolerance of 0.082 ft, so none sits on the tolerance's edge
3. THE OTHER CLASHES CASE. The original finds `1.2.7.3.0.0.0.0.2 | 1.2.9.0.0.2.0.0.0` at -1.247 ft
   and the swap `1.2.7.3.0.0.0.0.1 | 1.2.9.0.0.2.0.0.0` at -0.287 ft, lines 196 and 197. The second
   item is the same and the first items are two children of one parent, by their index paths. Which
   objects those are, and whether a person would read the two as one clash, is UNKNOWN, no name was
   read
4. On all 59 the original run again found exactly its stored pairs, run against stored differ 0, every
   item read, and the swap's sides read back as the original's swapped
5. Every test of the XML carries the same settings, turn5\measure-mirrors.md, so the swaps here differ
   from their originals in nothing but the order of the sides

**PART 3, 123 OF THE 469 TESTS THAT FIND NOTHING: EVERY SWAP FOUND NOTHING TOO.** Summed by the
prober over lines 209 to 331, since the deadline stopped the run before the mode wrote a total:
123 tests, original 0 and swap 0 clashes on every one, verdict same on every one, TestsRunTest
1211.341 s on the originals and 1212.978 s on the swaps. The other 346 tests were not reached and
are UNKNOWN.

**THE SECONDS, AND WHY THEY DO NOT CARRY OVER.** Over all 182 tests run, parts 2 and 3, the originals
took 1785.872 s and both 3569.769 s. A TestsRunTest here took 9.738 s on average over part 2's
originals and 9.848 s over part 3's, whether the test found 2568 clashes or none. In the tool's own
run of set 04 on the same building the TESTS RUN step took 118.480 s over 528 visits,
steps\runs\04\item1-C02\run-20261004-185652.log line 508, about 0.22 s a test. So a run in this probe
cost about 40 times what it cost in the tool's run. The document here held the 2847 saved
viewpoints and 2939 results run 04 left in it, where the tool's run built them as it went, and four
AutoSave files, below, were written while the last tests ran. Which of these, if any, makes the
difference is UNKNOWN. What carries over is the ratio, both runs costing 1.99 times the original
alone. What the extra is in the tool's run, about the TESTS RUN step's 118 s again if the ratio
holds there, is UNKNOWN until a run of the tool with answer D measures it.

**BADER'S THINGS. ONE OF HIS AUTOSAVE FILES IS GONE.** The NWF the copy was made from read sha256
0944C100 at the start and at the end, line 1163. 35 registry values were put back, each read again
with 0 still differing, line 1149. InfoCenter.log was put back reading its backup's sha256, line
1152. The tool's own logs folder had nothing added or changed, line 1160. The AutoSave folder was
listed by the prober by name, size and sha256 before the run, 199 files, and after it, 202, in
`turn5\probe-q133-1A02MM-autosave-before.txt` and `-after.txt`, and by the guard, lines 1154 to 1159:

- ADDED, by the probe's Navisworks, and LISTED, NOT DELETED, for the lead to remove:
  `1104-PAR-1A02MM-ZZZ-BM-MOD-000001.Autosave363.nwf` 41,464,359 bytes sha256 30564B2B,
  `.Autosave364.nwf` 41,464,363 bytes sha256 E7FAEEFA, `.Autosave365.nwf` 41,464,394 bytes sha256
  73471527, and `.Autosave366.nwf` 12,464,128 bytes sha256 834F7810, written 17:05:30 to 17:05:53,
  the last cut short when the guard closed pid 52324
- GONE: `1104-PAR-1A02MM-ZZZ-BM-MOD-000001.Autosave0.nwf`, 65,627 bytes, written 2026-09-20 15:13:20,
  sha256 8120CE8E6FEE08123648E9B689CE0652D7B3BD0DE415BC7689F7DA49001654B3. The guard lists the
  AutoSave folder and does not copy it, so no backup of it exists, and the prober found it in no
  Recycle Bin. It went while the probe's Navisworks ran, and the only Navisworks running then was
  pid 52324. That this Navisworks removed it, as autosaves of the same name were added, is the
  prober's reading, and what removed it is UNKNOWN from the record. It cannot be put back from
  anything this probe kept

THE PROGRAMS. One Navisworks, pid 52324, started by the probe at 16:04:25 and closed by the guard
through the held handle at the deadline, and gone, lines 335 and 336. AdskLicensingAgent pid 34240,
its child, read UNKNOWN at the end, line 1093, and no process held pid 34240 when the prober read it
after. No Roamer that was not there in step 2 ran at the end, line 1107. At 17:10 the prober read one
process named Roamer.exe, pid 31672, started 17:10:20 from
`%LOCALAPPDATA%\NwcFederatorLoop\test-f131-before\standin-bin\Roamer.exe` with the arguments
`sleep 300`, another session's stand-in and not Navisworks. The probe did not start it and touched
nothing of it.

**STILL UNKNOWN.**

- 1A04PK. This section is 1A02MM only
- the 346 tests of part 3 not reached
- why a TestsRunTest took about 10 s here against about 0.22 s in the tool's run, and so the extra
  seconds answer D costs in the tool's run
- why a swap finds more on these five. Whether Hard Conservative is not symmetric by design, or what
  in these items makes the difference, is not read here. Four of the five have a ST set on the
  original's side B, and the fifth is EL against EL
- whether the Clash Detective panel shows the same counts. The panel was not read
- whether the other clashes case is one clash seen on two children of one object
- what removed Bader's Autosave0.nwf of 1A02MM, and whether any copy of it exists anywhere

**WHAT THIS DECIDES.** For the record Bader asked for, 1A02MM: of the 59 tests that find a clash, a
swap finds more on 4 and other clashes on 1, so 5 of 59 find something the original does not, 7
clashes in all against the originals' 2939, and none of the 123 tests of part 3 reached found
anything either way. Running both cost 1145.449 s of TestsRunTest against 574.530 s for the
originals alone, here, 1.99 times. F132's rule, by swapped sets or by the same rule list, finds no
pair whose two tests are created on 1A02MM, so the merge by the pair of items has nothing to merge
here, and every clash only a swap finds comes from a swap no rule pair names.

## 5z-s. HOW OFTEN A MIRROR FINDS MORE ON 1A04PK, AND WHAT RUNNING BOTH COSTS, MEASURED 2026-10-07

Q133, probe Q133-1A04PK. Bader's answer D to Q133 keeps both tests of a mirrored pair, runs both
and merges their clashes by the pair of items, and asks: "Measure on 1A02MM and 1A04PK how often a
mirror finds more, and the extra time running both costs, and give both in the next record."
1A02MM is 5z-r. This section is 1A04PK, measured the same way. The question: on 1A04PK, which of
the pairs F132's rule finds would both be created and what does each find, and over every test the
tool would create that finds a clash, how often does its swap find more, fewer or other clashes,
and what do the two runs cost against the original's alone? A swap that often finds more makes
answer D worth its seconds. A swap that never finds more makes it time for nothing here.

THE NWF HOLDS NO SETS AND NO TESTS, SO THE XML WAS BROUGHT IN FIRST, THE ADD-IN'S WAY. The NWF of
set 04, `runs\04\NMFed\NWF\C04\1104-PAR-1A04PK-ZZZ-BM-MOD-000001.nwf`, 4,699 bytes, sha256
7ECA0ECA, read back 0 sets, 0 clash tests and 10 saved viewpoints at the root, line 69 of the
result. Neither the .NET API nor the COM API carries a clash XML import.
`tools\probes\ViewpointProbe\reflect-clash-import.ps1` read every public member whose name holds
Import or Xml in Autodesk.Navisworks.Api.dll, Clash.dll, ComApi.dll and Interop.ComApi.dll, with no
Navisworks started, and found 6 in the Api DLL, none of them about clash tests, and 0 in the other
three, `q133-1a04pk-reflect-import-result-20261007.txt`:

    powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\reflect-clash-import.ps1 -Out tools\probes\ViewpointProbe\q133-1a04pk-reflect-import-result-20261007.txt

So the probe brings the XML in with the add-in's own code, compiled into it from src and not
changed: `tools\probes\ViewpointProbe\Q133Import\Q133ImportProbe.csproj` builds every file of
Federator.Core and every file of Federator.Addin\Engine but FederationEngine.cs into one assembly,
Q133ImportProbe.dll, plugin Q133ImportProbe.PARS, so no second Federator.Core is loaded beside the
installed bundle's. Its mode q133import reads the XML's copy with MatrixCorrections.ReadPicked, the
call the window makes, with `exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt`, sha256
AFC463BE, copied beside it under the name the tool looks for, builds the sets with SetBuilder.Build
on SetBuildPlan.From, and chooses and makes the tests with ClashRunner's own PlanTheCreation and
Create, both private, called by reflection on a real ClashRunner. The tool's run log of the probe
went into the work folder, never into %LOCALAPPDATA%\ParsonsNwcFederator\logs.

HOW. Two parts, as for 1A02MM.

1. THE RULE'S PAIRS, read off the XMLs with no Navisworks, by `q133-rule-pairs.py` on the picked
   XML, sha256 792B01FB, and the corrected one in exchange\, sha256 94897667. Its output is byte
   for byte the 1A02MM one, the XML being the same, `q133-1a04pk-rule-pairs-result-20261007.txt`:

       python tools\probes\ViewpointProbe\q133-rule-pairs.py %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A04PK-rule-pairs.txt %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml

2. EVERY TEST THE TOOL WOULD CREATE, inside Navisworks. In XML order each test is made with the
   tool's Create and run with TestsRunTest timed alone, the tool's own call. Each one whose results
   hold at least one clash not Resolved gets a swap made beside it the way P1 and 5z-r made theirs,
   sides read back swapped, results cleared, run and timed, and the two compared by the unordered
   pair of the index paths of Item1 and Item2. No test starts after a cap of 10,800 s. Part 1 is
   then read off those same runs, so every test ran once

Run from Windows PowerShell 5.1 under the guard of P1, tools\loop\nw-guard.ps1 as merged from main
at de271bc, F138's Auto-Save switch included:

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-q133-import.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A04PK-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C04\1104-PAR-1A04PK-ZZZ-BM-MOD-000001.nwf -Xml %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml -Corrections exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt -PairsFile %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A04PK-rule-pairs.txt -PluginAssembly %LOCALAPPDATA%\NwcFederatorLoop\turn5\q133-1A04PK-build\out\Q133ImportProbe.dll

with the DLL built by `dotnet build Q133ImportProbe.csproj -c Release -o <that out folder>` from
the source committed here, DLL sha256 5B970457, script sha256 16C7DCEB, lines 26 and 3. One run at
11:31, kept as `q133-1a04pk-import-result-20261007.txt`, the machine name on line 1 masked as
`[machine]` and the account folder on line 5 as `%USERPROFILE%`, nothing else changed. Get-Process
Roamer read 0 before it. Navisworks pid 44956, adopted on all four conditions, line 41. TryOpenFile
of the copy True after 4.759 s with 10 models, every one read from under the loop folder, lines 57
to 68. The whole run took 209 s and every step passed, lines 1524 to 1530.

**THE IMPORT: 61 SETS, 39 FINDING ITEMS, AND 741 OF THE 1830 TESTS CREATED.** Lines 72 to 95:

```
MatrixCorrections.ReadPicked         61 sets, 1830 tests, 23 changes from the list beside it
SetBuilder.Build                     61 created, 39 finding items, 22 at zero, 0 failed, 2.432 s
ClashTestPlan.From                   1830 buildable, 0 skipped before the model
PlanTheCreation                      741 created, 1089 not created as a side finds nothing
```

Run 04 of the tool on the same NWF created 561 with 34 sets finding items,
steps\runs\04\item1-C04\run-20261004-211839.log lines 256 and 265, with the installed build
e4484d15, whose log carries no MATRIX line. The 180 more here follow from the corrections list,
which made 5 more sets find items. That is the prober's reading of the two counts. Which sets
they are was not read.

**PART 1, THE RULE'S PAIRS: NONE CREATED ON 1A04PK.** 60 pairs and self tests, all over the
Telecom Fixtures and Telephone Devices sets, and not one has its tests created, line 1415. They
cost 0 s and find nothing here, as on 1A02MM.

**PART 2, EVERY TEST THAT FINDS A CLASH: A SWAP FINDS MORE ON 1, FEWER ON 2 AND OTHER CLASHES ON
14 OF 174.** All 741 tests were measured, none left for the cap, lines 1339 to 1344:

```
tests the tool would create, all run           741
finding at least one clash                     174, holding 12971 clashes
the swap finds the same                        157
the swap finds more                            1
the swap finds fewer                           2
the swap finds other clashes                   14
UNKNOWN                                        0
clashes, the swaps                             12976
only the swap finds                            252
only the original finds                        247
TestsRunTest, the 174 originals                13.060 s
TestsRunTest, their 174 swaps                  12.643 s
both                                           25.703 s, 1.968 times the originals alone
TestsRunTest, the 567 that find nothing        34.910 s
every original                                 47.970 s
every original and every swap                  60.613 s, 1.264 times every original
making the 741 originals with Create           10.680 s
making the 174 swaps                           4.799 s
```

A TestsRunTest took 0.065 s on average over the 741 originals and at most 0.270 s, and 0.073 s on
average over the swaps, summed by the prober over the LINE lines.

**MOST OF WHAT ONE SIDE FINDS ALONE IS THE SAME CONTACT ON ANOTHER PART OF THE SAME OBJECT.**
`q133-sibling-check.py` paired, test by test, each clash only the original finds with one only the
swap finds, first as siblings, one item the same and the other two children of one parent, then as
near, the same distance to 1e-6 ft and each item the same or one index apart,
`q133-1a04pk-siblings-result-20261007.txt`:

    python tools\probes\ViewpointProbe\q133-sibling-check.py %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A04PK-result.txt %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-q133-1A04PK-siblings.txt

```
clashes only the original finds   247
clashes only the swap finds       252
paired as siblings                234
paired as near                    10
left only in the original         3
left only in the swap             8
```

The 17 tests where the two differ, with what is left after the pairing:

```
test                                                       original  swap  verdict  left orig  left swap
BLD-AR-Walls-vs-BLD-AR-Floors                                   996   996  other            0          0
BLD-AR-Railings-vs-BLD-AR-Stairs                                 20    20  other            0          0
BLD-AR-Railings-vs-BLD-AR-Walls                                   5     5  other            0          0
BLD-ST-Columns-vs-BLD-AR-Floors                                1956  1956  other            0          0
BLD-ST-Columns-vs-BLD-AR-Walls                                  102   101  fewer            1          0
BLD-ST-Framing-vs-BLD-AR-Floors                                  73    73  other            0          0
BLD-ST-Framing-vs-BLD-AR-Walls                                  482   480  fewer            2          0
BLD-ST-Framing-vs-BLD-ST-Columns                                175   181  more             0          6
BLD-ST-Floors-vs-BLD-AR-Walls                                   925   925  other            0          0
BLD-ST-Stair-vs-BLD-AR-Stairs                                    19    19  other            0          0
BLD-DR-Pipes & Pipe Fittings-vs-BLD-AR-Floors                   212   212  other            0          0
BLD-EL-Electrical Equipment-vs-BLD-AR-Walls                      30    30  other            0          0
BLD-EL-Electrical Equipment-vs-BLD-AR-Site                     1436  1436  other            0          0
BLD-EL-Electrical Equipment-vs-BLD-ST-Columns                     6     6  other            0          0
BLD-EL-Conduits & Conduit Fittings-vs-BLD-EL-Electrical Equipment
                                                                480   482  other            0          2
BLD-EL-Lighting Fixtures-vs-BLD-ST-Stair                         11    11  other            0          0
BLD-EL-Lighting Fixtures-vs-BLD-EL-Electrical Equipment           4     4  other            0          0
```

1. BLD-ST-Framing-vs-BLD-ST-Columns finds 175 and its swap 181, line 247, the same test P1 and 5z-r
   saw find more on 1A02MM. The 6 only the swap finds all share the item `8.2.0.0.0.55.0` and sit
   at -1.010 ft. The same item is the one clash BLD-ST-Columns-vs-BLD-AR-Walls finds only in its
   original, at -0.560 ft, line 222
2. The 2 left only in the swap of the Conduits test sit at -0.088 ft and -0.096 ft, just past the
   tolerance of 0.082 ft, lines 1150 to 1153. Every other clash left over is past -0.5 ft
3. On every other test the two sides list the same number of clashes and every difference pairs up.
   Electrical Equipment against Site alone carries 186 sibling pairs
4. Every swap's sides read back as the original's swapped, and no item failed to read, on all 174

So by the unordered pair of items, which is how answer D merges, the two runs together list 13,223
clashes on these 174 tests against the originals' 12,971, by the prober's sum of 12,971 and 252.
Of the 252 added, 244 pair with a clash the original already lists on another part of the same
object, by index path, and 8 do not. Whether a person reads a sibling pair as one clash or two, and
whether the Clash Detective panel shows one or both, is UNKNOWN, no name and no panel was read.

**THE SECONDS.** Here a TestsRunTest took 0.065 s on average against about 10 s on 1A02MM in 5z-r.
This document was fresh, 10 saved viewpoints and no results but the probe's, where 5z-r's held
2847 viewpoints and 2939 results. That the document's state made 5z-r slow is the prober's reading
and is UNKNOWN. Both readings agree on the ratio: running a test and its swap costs 1.97 times the
original alone on 1A04PK and 1.99 on 1A02MM, over the tests that find a clash. Running a swap only
beside a test that finds a clash cost 1.26 times every original here, 12.643 s more on 47.970 s.

**BADER'S THINGS. NOTHING ADDED, NOTHING GONE.** The NWF and the XML the copies were made from read
the same sha256 at the start and at the end, lines 1510 and 1512. The guard wrote the Auto-Save
switch "3 0" and read it back, line 31. 38 registry values were put back, enable among them, and
read again with 0 still differing, line 1499. InfoCenter.log and LastSession.xml put back reading
their backups' sha256, lines 1503 and 1504. The guard saw 0 AutoSave files added, changed or gone,
line 1506, and the tool's own logs folder had nothing added or changed, line 1507. The prober read
the switch and listed the AutoSave folder by name, size, write time and sha256 with
`read-autosave-state.ps1`, kept in %LOCALAPPDATA%\NwcFederatorLoop\turn5 as
`probe-q133-1A04PK-autosave-before.txt`, `-during.txt` and `-after.txt`: enable read String "0"
before the start, "3 0" at 11:33:47 while the probe ran, and "0" after the put back, and the folder
held the same 199 files with the same sizes, times and sha256 before and after. So Auto-Save off
held through this start, and no autosave was written.

THE PROGRAMS. One Navisworks, pid 44956, started by the probe at 11:31:58, quit by Dispose and gone
7.5 s after, not forced, lines 1421 and 1422. AdskLicensingAgent pid 45188, its child, read UNKNOWN
at the end, line 1451, and no process held pid 45188 when the prober read it after. No Roamer that
was not there in step 2 ran at the end, line 1454.

**STILL UNKNOWN.**

- whether a swap of a test that finds nothing finds something on 1A04PK. Those 567 swaps were not
  made, the brief asking for the tests that find a clash
- which 5 sets the corrections list made find items, and so which of the 741 a run of the tool
  today creates against run 04's 561, beyond what PlanTheCreation gave here
- why a TestsRunTest took about 10 s on 1A02MM in 5z-r and 0.065 s here
- why a swap finds more or fewer on the three tests above, and what object `8.2.0.0.0.55.0` is
- whether the Clash Detective panel shows the same counts, and whether a sibling pair is one
  clash to a reader

**WHAT THIS DECIDES.** For the record Bader asked for, 1A04PK: of the 174 tests that find a clash, a
swap finds more on 1, fewer on 2 and other clashes on 14, so 17 of 174 differ. Of the 252 clashes
only a swap finds, 244 are the same contact the original already lists on another part of the same
object, by index path, and 8 are not, 6 on BLD-ST-Framing-vs-BLD-ST-Columns and 2 on the Conduits
against Electrical Equipment test. Running both cost 25.703 s of TestsRunTest against 13.060 s for
those originals alone, 1.97 times, and 60.613 s against 47.970 s over every test the tool would
create. F132's rule finds no pair whose two tests are created on 1A04PK, as on 1A02MM, so every
clash a swap adds comes from a swap no rule pair names. A merge by the unordered pair of items, as
answer D has it, would count the 244 siblings as new clashes.

## 5z-q. DOES A COPY OF A MARKED VIEW GET A NEW GUID, AND DOES ITS COMMENT TRAVEL, MEASURED 2026-10-07

P11 of Q114, the views by team design, part 3. The letter 5z-q was left to P11 by 5z-r, so this
section stands after 5z-s. P9 made the mark a comment written by AddComment, 5z-o. P10 found the
tool's routes give no Guid, 5z-p. The question: does AddCopy of a view the tool marked give an
item with a new Guid, and does the comment travel with it? By the row of P11: the comment travels
and the Guid is new, so the copy fails the fingerprint and is a person's. The Guid is kept, so the
Guid stays out of the mark.

HOW. `tools\probes\ViewpointProbe\probe-view-copy.ps1` is P10's `probe-view-guids.ps1` with the
mode `vpcopy` of `ViewpointProbe.dll`, both written on 2026-10-05 and committed unrun at 51dd8c5.
Before this run main was merged at 647ce5d, and the one change made to the script was F138's
SwitchAutoSaveOff after the last Roamer read and before the constructor, as
`probe-q133-import.ps1` has it. tools\loop\nw-guard.ps1 read the same on main and on the branch.
The guard is the loop's, dot-sourced, with the Roamer refusal, the settings backup, the Auto-Save
switch, the adoption by AdoptStart's four conditions, Dispose, the close through the held handle
only when needed, and SettingsPutBack. Get-Process Roamer read 0 before the run and 0 after it.
The probe copied P9's
`%LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf`,
41,319,461 bytes, sha256 869DD965, into the new folder `probes\view-copy-20261007-122418`, and the
mode, on that copy:

1. opens it and reads three sources. S1 is P9's COM view `P9 probe / P9 sub / P9 view addcomment
   after add`, the tool's view route, marked by AddComment. S2 is F85's view `AR vs ME / Over 150mm /
   BLD-ME-Ducts&Duct Fittings-vs-BLD-AR-Walls  Clash1`, which P9 marked. S3 is made here, a copy
   of S1 by SavedItem.CreateCopy, renamed `P11 source guid set`, its Guid set to Guid.NewGuid()
   before AddCopy into `P11 probe / P11 sources`
2. makes the folder `P11 probe` at the root and one folder under it for S3 and for each copy, by
   FolderItem and AddCopy, and copies each source into its own folder: C1 AddCopy of S1 itself,
   C2 AddCopy of S1.CreateCopy(), C3 AddCopy of S1.CreateUniqueCopy(), C4 AddCopy of S2 itself,
   C5 AddCopy of S3 itself, C6 AddCopy of S3.CreateCopy(), C7 AddCopy of S3.CreateUniqueCopy()
3. reads every source and copy, its type, name, Guid, index path, Hidden and MaterialOverrides
   counts and every comment's Body, Author, Status, Id and CreationDate, right after the add,
   before the save, and after SaveFile into `p11-copy-saved.nwf` in the work folder,
   Document.Clear and TryOpenFile of that file, and counts every Guid of the whole tree at each
   stage. A copy's comments count as travelled only when Body, Author and Status equal the
   source's, Ordinal, at every stage

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-copy.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p11-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
DLL sha256 C1F6FBC3, script sha256 909F0C8E, lines 24 and 3. One run at 12:24, kept as
`p11-view-copy-result-20261007.txt`, the machine name on line 1 masked as `[machine]` and the
account folder on line 5 as `%USERPROFILE%`, nothing else changed. Navisworks pid 27660, adopted on
all four conditions, line 38. TryOpenFile of the copy returned True after 4.706 s with 4 models,
every one read from under the loop folder, lines 54 to 59. ExecuteAddInPlugin returned 0 after
18.59 s, line 48. Dispose returned and pid 27660 was gone 10.1 s later, not forced, line 190.

**THE ANSWER: THE COMMENT TRAVELS WHOLE ON EVERY COPY MADE. THE GUID IS NEVER KEPT ON A SECOND
ITEM, BUT ON THE TOOL'S VIEW THERE IS NO GUID TO KEEP OR RENEW.** Lines 155 to 184 of the result,
each Guid cut to its first 8 characters and the three stages of a copy joined into one row, since
each copy read the same at add, save and reopen:

```
copy | route | the copy's Guid | the source's | the Guid is | comments the same | Ids and dates the same | Hidden and MaterialOverrides the same | name the same
C1 | AddCopy(folder, S1 itself)               | 00000000 | 00000000 | empty, as the source's | YES (1 and 1) | YES | YES (1, 2) | YES
C2 | AddCopy(folder, S1.CreateCopy())         | 00000000 | 00000000 | empty, as the source's | YES (1 and 1) | YES | YES (1, 2) | YES
C3 | AddCopy(folder, S1.CreateUniqueCopy())   | 00000000 | 00000000 | empty, as the source's | YES (1 and 1) | YES | YES (1, 2) | YES
C4 | AddCopy(folder, S2 itself)               | 00000000 | 00000000 | empty, as the source's | YES (1 and 1) | YES | YES (2, 4697) | YES
C5 | AddCopy(folder, S3 itself)               | THREW ArgumentException: Argument 'item' contains a duplicate GUID
C6 | AddCopy(folder, S3.CreateCopy())         | THREW ArgumentException: Argument 'item' contains a duplicate GUID
C7 | AddCopy(folder, S3.CreateUniqueCopy())   | 718aaa97 | e689357b | NEW | YES (1 and 1) | YES | YES (1, 2) | YES
P11 on the tool's marked view, C1, AddCopy of S1 itself: the comment travels YES, the Guid empty, as the source's
P11 over all 7 copies: the comment travelled on 5 and not on 2. The Guid: 4 empty, as the source's, 2 UNKNOWN, 1 NEW
```

1. THE COMMENT TRAVELS. Every copy that was made, C1 to C4 and C7, carried exactly one comment,
   its Body, Author and Status equal to its source's, the marker line naming the SOURCE's folder
   path and name included, right after the add, before the save and after the reopen, lines 81
   to 104, 114 to 125 and 139 to 150. The two not counted, C5 and C6, are the two the API refused
   to make, below, so no comment was lost on any copy that exists
2. THE COPY KEEPS THE NAME. Every copy read the same DisplayName as its source, Ordinal, at every
   stage. Each sat in a folder of its own, so only the folder path differs from the source's
3. THE COMMENT'S ID AND DATE TRAVEL TOO. Each copy's comment read the same Id and CreationDate as
   its source's, Id 3 on S1, S3, C1, C2, C3 and C7, and Id 5 on S2 and C4, lines 65, 67, 77, 82 to
   104. So after a copy one comment Id is carried by more than one item. The tree went from 2875
   items, 2850 viewpoints, 25 folders and 5 comments to 2890 items, 2856 viewpoints, 34 folders and
   11 comments, and read the same after the reopen, lines 60, 61, 107, 132 and 152
4. AN EMPTY GUID STAYS EMPTY ON EVERY ROUTE. S1 and S2 read the empty Guid, as P10 found. AddCopy
   of the item itself, CreateCopy and CreateUniqueCopy each gave a copy with the empty Guid, before
   the add, lines 83 and 87, and after it at every stage. CreateUniqueCopy makes no Guid where the
   source has none
5. A GUID IS NEVER CARRIED BY TWO ITEMS. S3, its Guid set before AddCopy, held it through the
   save and the reopen and ResolveGuid returned it, lines 76, 112, 126, 137 and 151. AddCopy of S3
   itself and of S3.CreateCopy(), which read back S3's Guid before the add, line 97, each THREW
   `ArgumentException: Argument 'item' contains a duplicate GUID`, lines 94 and 98, and nothing was
   added, their folders holding 0, lines 96 and 100. S3.CreateUniqueCopy() gave a new Guid,
   718aaa97, before the add, line 101, kept through the save and the reopen. No Guid that is not
   empty was carried by more than one item at any stage, lines 107 and 132
6. THE HIDDEN AND MATERIALOVERRIDES COUNTS TRAVEL. Every copy read its source's counts, 1 and 2 off
   S1, 2 and 4697 off S2. The camera was not read
7. THE CALLS ARE CHEAP. Each AddCopy returned in 0.001 to 0.002 s, lines 75 to 102. SaveFile took
   8.382 s, 41,319,917 bytes read back, line 127. Document.Clear took 0.529 s, line 128.
   TryOpenFile of the saved file took 4.556 s, line 129. The saved copy is
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-copy-20261007-122418\p11-copy-saved.nwf`, sha256
   B2E5F0B9E06806C82F42A931B83FEE6F6B142DD3602B7FB0FD2C7397AF2E5FEF, line 186

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD.** The NWF the copy was made from read sha256
869DD965 at the start and at the end, lines 22 and 283. The guard wrote the Auto-Save switch
"3 0" and read it back, line 28. The watchdog saw no other Navisworks, so the put back ran, line
232. 38 registry values were put back, enable among them, line 237, and read again with 0 still
differing, line 272. InfoCenter.log and LastSession.xml were put back reading their backups'
sha256, lines 276 and 277. The guard saw 0 AutoSave files added, changed or gone, line 279, and
the tool's own logs folder had nothing added or changed, line 280. The prober read the switch and
listed the AutoSave folder by name, size, write time and sha256 with `read-autosave-state.ps1`,
kept in %LOCALAPPDATA%\NwcFederatorLoop\turn5 as `probe-p11-20261007-autosave-before.txt`,
`-during.txt` and `-after.txt`: enable read String "0" at 12:23:59 before the start, "3 0" at
12:24:59 while the probe ran, and "0" at 12:27:19 after the put back, and the folder held the same
199 files with the same sizes, times and sha256 before and after, 0 lines differing.

THE PROGRAMS. One Navisworks, pid 27660, started by the probe at 12:24:27, quit by Dispose and
gone 10.1 s after, not forced, lines 189 and 190. AdskLicensingAgent pid 34008, its child, and two
AdskLicensingInstHelper processes under GenuineService.exe, pids 42868 and 39708, all read exited
at the end, lines 223 to 226. No Roamer that was not there in step 2 ran at the end, line 227.

**STILL UNKNOWN.**

- a copy into the SAME folder as its source. Every copy here went into a folder of its own. What
  name AddCopy gives it there, and whether its mark then reads as the tool's, was not measured
- a copy a person makes in the Saved Viewpoints window. That is P21, Bader's hand step
- whether the camera travels. Only the Hidden and MaterialOverrides counts were read
- what a comment Id carried by several items does in the Comments window or to a later
  AddComment. Not read
- whether a copy of a marked folder carries its comment. No folder was copied
- whether any of this holds through a reopen in a new Navisworks. The close was Document.Clear
  inside the same Navisworks

**WHAT THIS DECIDES.** By the row of P11, the comment travels, and the Guid is neither kept nor
new on the tool's own view route, because a COM view has no Guid and its copy has none either.
Where a Guid exists the API never lets a second item carry it: AddCopy refuses it and
CreateUniqueCopy renews it. So, with P10's NO, the Guid stays out of the mark. Because the mark
travels whole, comment Id and date included, the comment alone cannot tell the tool's view from a
copy of it. What tells them apart is the folder path and name the marker line carries: C1 to C4
each sat in another folder while their marks named the source's, which is the design's test "a
copy in another folder gives ChangedByAPerson". A copy that keeps both the folder and the name
would pass the fingerprint, and whether the API or a person can make one is UNKNOWN.

## 5z-t. DOES A VIEW WHOSE NAME ENDS IN A SPACE READ BACK ITS NAME UNCHANGED, MEASURED 2026-10-07

P12 of Q114, the views by team design, part 3. Two set names of the clash XML end in a space,
core.md, so a test name built from them can, and the design names a test's view exactly by its
test, never trimmed. The question: does a view whose name ends in a space read back its
DisplayName unchanged, Ordinal? By the row of P12: No, that view is found by its mark alone and
VIEWS TREE says so. Yes, the name stands as written. The row runs on P9's saved copy and hangs on
no other probe's answer.

HOW. `tools\probes\ViewpointProbe\probe-view-name-spaces.ps1` is P11's `probe-view-copy.ps1`
with the mode `vpspace` of `ViewpointProbe.dll` in place of P11's mode, and its own header, work
folder prefix and save name. Nothing else in the script changed, F138's SwitchAutoSaveOff
included. The branch was pulled first and was up to date. tools\loop\nw-guard.ps1 read sha256
E29D2733, the same as in P11's run, line 4. The guard is the loop's, dot-sourced, with the Roamer
refusal, the settings backup, the Auto-Save switch, the adoption by AdoptStart's four
conditions, Dispose, the close through the held handle only when needed, and SettingsPutBack.
Get-Process Roamer read 0 before the run and 0 after it. The probe copied P9's
`%LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf`,
41,319,461 bytes, sha256 869DD965, into the new folder `probes\view-space-20261007-125742`, and
the mode, on that copy:

1. makes the folder `P12 probe` at the root and nine folders of plain name under it, V0 to V7
   and F1, by FolderItem and AddCopy, so every item is found by its position in a folder whose
   name holds no space at an end, and never by the name being read
2. writes, by the tool's routes:
   - V0, the control, `P12 V0 control`, a COM view added into its folder's own SavedViews, the
     route SavedViewpoints.Record takes with throughTheFolder true
   - V1, `P12-AR-XX_Alpha-vs-P12-ME-YY_Beta & Gamma ` with one space at the end, a test name's
     characters, the same route
   - V2, `P12 V2 root route one space `, the other route of Record: the COM view added at the
     root, the last root view named exactly so copied into its folder by AddCopy, the root one
     removed
   - V3, `P12 V3 two trailing spaces  `, the folder route
   - V4, ` P12 V4 leading and trailing `, a space at each end, the folder route
   - V5a `P12 V5 twin` and V5b `P12 V5 twin ` in the SAME folder, the two names differing only
     by the end space, the folder route
   - V6, `P12 V6 dotnet one space `, a .NET new SavedViewpoint(Viewpoint), DisplayName set,
     AddCopy into its folder
   - V7, a COM view added as `P12 V7 renamed`, then DocumentSavedViewpoints.EditDisplayName to
     `P12 V7 renamed `
   - F1, a FolderItem `P12 F1 folder end space `, the tool's folder route, and in it F1v, the COM
     view `P12 F1 view one space `, added into that folder's own InwOpFolderView
3. marks V1, V3 and F1 by AddComment after the add, as P9 found. V1's marker line carries the
   name in its middle. V3's and F1's end with the name, so their bodies end in a space
4. reads every item by position right after its add, before the save, and after SaveFile into
   `p12-space-saved.nwf` in the work folder, Document.Clear and TryOpenFile of that file: its
   .NET DisplayName with its length and its first and last code points, its COM name off the
   InwOpView or InwOpFolderView at the same position, whether ResolveNames over the written
   names, Ordinal, finds that same item by index path, what the names trimmed find, and the
   mark's Body and Author, Ordinal

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-name-spaces.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p12-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\probes\view-comments-20261005-144832\p9-copy-with-comments.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
DLL sha256 F5874016, script sha256 170A3C1A, lines 24 and 3. One run at 12:57, kept as
`p12-view-name-spaces-result-20261007.txt`, the machine name on line 1 masked as `[machine]` and
the account folder on line 5 as `%USERPROFILE%`, nothing else changed. Navisworks pid 43540,
adopted on all four conditions, line 38. TryOpenFile of the copy returned True after 4.571 s with
4 models, every one read from under the loop folder, lines 54 to 59. ExecuteAddInPlugin returned
0 after 76.90 s, line 48. Dispose returned and pid 43540 was gone 8.8 s later, not forced, line
219.

**THE ANSWER: YES. EVERY NAME WITH A SPACE AT AN END READ BACK EXACTLY AS WRITTEN, ON EVERY ROUTE,
AT EVERY STAGE.** Lines 200 to 213 of the result:

```
label | written | .NET DisplayName the same, add, save, reopen | COM name the same | the written names find this item | the mark the same, save, reopen
V0  | [P12 V0 control] length 14                              | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
V1  | [P12-AR-XX_Alpha-vs-P12-ME-YY_Beta & Gamma ] length 42  | YES, YES, YES | YES, YES, YES | YES, YES, YES | YES, YES
V2  | [P12 V2 root route one space ] length 28                | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
V3  | [P12 V3 two trailing spaces  ] length 28                | YES, YES, YES | YES, YES, YES | YES, YES, YES | YES, YES
V4  | [ P12 V4 leading and trailing ] length 29               | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
V5a | [P12 V5 twin] length 11                                 | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
V5b | [P12 V5 twin ] length 12                                | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
V6  | [P12 V6 dotnet one space ] length 24                    | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
V7  | [P12 V7 renamed ] length 15                             | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
F1  | [P12 F1 folder end space ] length 24                    | YES, YES, YES | YES, YES, YES | YES, YES, YES | YES, YES
F1v | [P12 F1 view one space ] length 22                      | YES, YES, YES | YES, YES, YES | YES, YES, YES | no mark
P12 over the 9 names with a space at an end: the .NET DisplayName read back unchanged at every stage on 9 and not on 0. The COM name the same on 9 and not on 0. A lookup by the written names found the item on 9 and not on 0. The marks read back the same on 3 of 3
P12 YES
```

1. THE NAME HOLDS ON EVERY ROUTE. The COM view into its folder, the COM view at the root copied
   into its folder, the .NET SavedViewpoint, EditDisplayName and the FolderItem each kept one end
   space, two end spaces and a space at each end, length and last code point U+0020 included,
   right after the add, before the save and after the reopen, lines 79 to 127, 135 to 162 and
   169 to 196. The name read back the same off the COM object before the add too, lines 77 to
   124, and off the .NET SavedViewpoint and FolderItem before their AddCopy, lines 108 and 118
2. THE COM SIDE AGREES. The COM name at the same position read the same as written, Ordinal, at
   every stage, and FindComFolderAt found the space-ended folder by its written name, Ordinal,
   line 123. So the throughTheFolder route reaches a folder whose name ends in a space
3. THE ROOT ROUTE FINDS ITS VIEW BY NAME. After the COM add at the root, the last root view named
   exactly as written was found, at 18 of 19, line 104, so Record's FindLastAtRoot by name does
   not lose a name that ends in a space
4. A NAME DIFFERING ONLY BY ITS END SPACE IS ANOTHER NAME. V5a and V5b sat in one folder at
   17.5.0 and 17.5.1, and the written names found each its own item at every stage, lines 102,
   152 and 186. The names TRIMMED found V5a for V5b, the wrong item, and found nothing for every
   other name with a space at an end, lines 84 to 127. So a trim anywhere on the path would find
   a person's view of the trimmed name, or nothing
5. THE MARK HOLDS A NAME THAT ENDS THE BODY. V3's and F1's comment bodies ended in the name, so
   in a space, and read back with the same length, last code point U+0020, Body and Author equal,
   Ordinal, before the save and after the reopen, lines 145, 159, 179 and 193. V1's, with the name
   in the middle, the same, lines 139 and 173
6. THE TREE. 2850 viewpoints, 25 folders and 5 comments at the open, line 60, and 2860, 36 and 8
   before the save and after the reopen, lines 163 and 197, the 10 views, 11 folders and 3 marks
   written here. SaveFile took 7.498 s, 41,319,983 bytes read back, line 164. Document.Clear took
   0.613 s, line 165. TryOpenFile of the saved file took 4.433 s, line 166. The saved copy is
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-space-20261007-125742\p12-space-saved.nwf`,
   sha256 C8E818F2C07C472B5B64803C26522B6BD9EA6E8B7CAE2B054DBB208D18225BCD, line 215
7. SEEN AND NOT ASKED. Each InwSavedViewsColl.Add of one COM view took 6.131 to 6.962 s in this
   tree of about 2850 viewpoints, lines 78 to 125, where the .NET AddCopy took 0.001 s and
   EditDisplayName 0.002 s, lines 109 and 115. The add was timed with nothing hidden and nothing
   painted. Why it costs that much is UNKNOWN here, and P18 is the probe that times the whole
   sequence

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD.** The NWF the copy was made from read sha256
869DD965 at the start and at the end, lines 22 and 313. The guard wrote the Auto-Save switch
"3 0" and read it back, line 28. The watchdog saw no other Navisworks, so the put back ran, line
262. 38 registry values were put back, enable among them, line 267, and read again with 0 still
differing, line 302. InfoCenter.log and LastSession.xml were put back reading their backups'
sha256, lines 306 and 307. The guard saw 0 AutoSave files added, changed or gone, line 309, and
the tool's own logs folder had nothing added or changed, line 310. The prober read the switch and
listed the AutoSave folder by name, size, write time and sha256 with `read-autosave-state.ps1`,
kept in %LOCALAPPDATA%\NwcFederatorLoop\turn5 as `probe-p12-20261007-autosave-before.txt`,
`-during.txt` and `-after.txt`: enable read String "0" at 12:57:09 before the start, "3 0" at
12:59:43 while the probe ran, and "0" at 13:01:22 after the put back, and the folder held the same
199 files with the same sizes, times and sha256 before and after, 0 lines differing.

THE PROGRAMS. One Navisworks, pid 43540, started by the probe at 12:57:49, quit by Dispose and
gone 8.8 s after, not forced, lines 218, 219 and 251. AdskLicensingAgent pid 32624, its child, and
AdskLicensingInstHelper pids 40760 and 17968 under GenuineService.exe read exited at the end.
AdskLicensingAgent pid 30160 and AdskLicensingInstHelper pid 41136 read UNKNOWN whether they
exited, their start times not readable when seen, lines 252 and 253. No Roamer that was not there
in step 2 ran at the end, line 257.

**STILL UNKNOWN.**

- a name ending in another white space, a tab, a no-break space U+00A0, or in a full stop. Only
  U+0020 was written
- what the Saved Viewpoints window shows for such a name, and whether a person's rename in the
  window keeps or trims an end space. That is a hand step
- whether the name holds through a reopen in a new Navisworks. The close was Document.Clear inside
  the same Navisworks
- whether it holds in an NWD. PublishFile was not called
- whether a clash test's own name read off the document keeps its end space. The view names here
  were written by the probe, not read off a test
- why one COM add costs about 6 s in this tree, point 7

**WHAT THIS DECIDES.** By the row of P12, YES: a view or folder whose name ends in a space is
found by its name as written, Ordinal, on both of Record's routes and through COM, so the No
branch, a view found by its mark alone and named in VIEWS TREE, is not needed for this case. The
design's test "a test name ending in a space is kept in the view name" holds on the install.
Point 4 adds one rule: nothing on the path from the test name to the lookup may trim, because the
trimmed name finds another item or none, and a mark that ends in the name keeps the space too.

## 5z-u. DOES REMOVEAT TAKE ONE VIEWPOINT TWO FOLDERS DEEP AND NOTHING ELSE, MEASURED 2026-10-07

P13 of Q114, the views by team design, part 3. F114 removes the views it made before, and the
2813 per-clash views F85 wrote, one RemoveAt(GroupItem, int) at a time from the end of each
parent, the parent resolved fresh and each target re-found by name just before. 5z measured
that call on sets only. The question: does RemoveAt(parent, index), the parent resolved fresh,
remove one viewpoint two folders deep, the count falling by exactly one and every other item
keeping its path, name and Guid through a save and a reopen, and how many seconds does one call
take in a tree of 2847? By the row of P13: Yes, removal is built, from the end within each
parent. No, nothing is removed, and stale views are only named. The row works on a fresh copy of
the baseline NWF and hangs on no other probe's answer.

THE MEMBERS, read off `C:\Program Files\Autodesk\Navisworks Manage 2025\Autodesk.Navisworks.Api.dll`
by reflection in this session, all declared on DocumentSavedViewpoints itself:

```
Void    RemoveAt(GroupItem parent, Int32 index)
Void    RemoveAt(Int32 index)
Boolean Remove(GroupItem parent, SavedItem item)
Boolean Remove(SavedItem item)
Void    Move(GroupItem oldParent, Int32 oldIndex, GroupItem newParent, Int32 newIndex)
Void    Move(Int32 oldIndex, Int32 newIndex)
Void    Clear()
```

HOW. `tools\probes\ViewpointProbe\probe-view-remove.ps1` is P12's `probe-view-name-spaces.ps1` with
the new mode `vpremove` of `ViewpointProbe.dll` in place of `vpspace`, its own header, work folder
prefix and save name, a line for the second saved copy, and `-Nwf` taken from under `runs` or
`probes` of the loop folder, since the baseline lies under `runs`. Nothing else in the script
changed, F138's SwitchAutoSaveOff included. The branch was pulled first and was up to date.
tools\loop\nw-guard.ps1 read sha256 E29D2733, the same as in P11's and P12's runs, line 4. The
guard is the loop's, dot-sourced, with the Roamer refusal, the settings backup, the Auto-Save
switch, the adoption by AdoptStart's four conditions, Dispose, the close through the held handle
only when needed, and SettingsPutBack. Get-Process Roamer read 0 before the run and 0 after it.
The probe copied the baseline's C02 NWF of run set 04,
`%LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`,
41,317,271 bytes, sha256 0944C100, the file P8 read, into the new folder
`probes\view-remove-20261007-133308`, and the mode, on that copy:

1. PART A. Reads the whole saved viewpoint tree from RootItem, one row per item: index path,
   folder names, name, kind, Guid and comment count. Counts models, sets, tests, results and
   statuses a person set
2. picks the first folder two deep, in tree order, with at least 3 children and a viewpoint at
   its middle child, and takes that middle viewpoint
3. resolves the parent fresh from RootItem by its two folder names, re-finds the target in it by
   its name, Ordinal, and calls `DocumentSavedViewpoints.RemoveAt(parent, index)`, timed by a
   Stopwatch around the call alone
4. reads the tree again and compares it row by row, in tree order, with the tree at the open
   less that one row, where only the later siblings' last index falls by one. Looks the target
   up by its names
5. saves into `p13-remove-saved.nwf` in the work folder, calls Document.Clear and TryOpenFile of
   the saved file, reads the tree and the counts once more and compares with the tree after the
   removal, exactly
6. PART B, because every Guid of the baseline reads empty, 5z-n: in the reopened document adds
   the folder `P13 probe` at the root, `P13 sub` in it and 12 .NET SavedViewpoints `P13 view 00`
   to `P13 view 11` in that, each folder and view with a Guid set before its AddCopy, the route
   P10 found keeps a Guid. Removes `P13 view 05` from the middle and then 5 from the end, the
   same way as in step 3, each timed. Compares the tree after each, reads every Guid set through
   ResolveGuid, saves into `p13-remove-saved-sentinels.nwf`, clears, reopens and reads again

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-remove.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p13-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
DLL sha256 05F25FD8, script sha256 FDC46E47, lines 24 and 3. One run at 13:33, kept as
`p13-view-remove-result-20261007.txt`, the machine name on line 1 masked as `[machine]` and the
account folder on line 5 as `%USERPROFILE%`, nothing else changed. Navisworks pid 35736, adopted on
all four conditions, line 38. TryOpenFile of the copy returned True after 4.993 s with 4 models,
every one read from under the loop folder, lines 54 to 59. ExecuteAddInPlugin returned 0 after
34.45 s, line 48. Dispose returned and pid 35736 was gone 8.8 s later, not forced, line 122.

**THE ANSWER: YES. ONE CALL TOOK THE ONE VIEWPOINT AND NOTHING ELSE, AND IT HELD THROUGH A SAVE AND
A REOPEN.** Lines 62 to 82 of the result:

```
                                     at the open   after RemoveAt   after save, clear, reopen
items in the viewpoint tree                 2869             2868                        2868
viewpoints                                  2847             2846                        2846
folders                                       22               22                          22
Guids not empty                                0                0                           0
models, sets, tests, results,     4, 61, 528, 2939, 0    the same                    the same
statuses a person set
rows that differ from the expected tree                          0                           0
the target found by its names                yes          nothing                     nothing
P13 YES
```

1. THE TARGET. `AR vs ME / Over 150mm / BLD-ME-Ducts&Duct Fittings-vs-BLD-AR-Walls  Clash5`, a
   SavedViewpoint at index path 6.0.4, child 4 of 8 in its folder, Guid empty, no comment, line
   64. The parent resolved fresh by its names was at 6.0, the name was found on 1 of its 8
   children, at index 4, the same index path the tree read at the open, lines 65 and 67. The
   parent resolved again after the call held 7, line 66
2. NOTHING ELSE MOVED BUT THE LATER SIBLINGS. After the call the tree read 2868 rows, and row by
   row in tree order every one had the same folder names, name, kind, Guid and comment count as
   the tree at the open less the target, 0 differing, line 71. The 3 later siblings, children 5 to
   7, each read one index lower, and no other index path changed, lines 70 and 71
3. IT HELD THROUGH A SAVE AND A REOPEN. After SaveFile, Document.Clear and TryOpenFile of the saved
   file the tree read the same 2868 rows, index paths included, 0 differing, line 78. The
   target's names found nothing after the call and after the reopen, lines 72 and 79
4. NOTHING ELSE IN THE DOCUMENT CHANGED COUNT. 4 models, 61 sets, 528 tests, 2939 results and 0
   statuses a person set at the open, after the call and after the reopen, lines 62, 69 and 77
5. THE SECONDS. RemoveAt took 0.002 s in the tree of 2847 viewpoints, line 65. In part B each of
   6 calls in a tree of 2852 to 2858 read 0.000 s, so under half a millisecond, line 114. Each walk of the
   whole tree took 0.002 to 0.006 s, lines 63 to 109
6. THE GUID HALF OF THE QUESTION, ON THE BASELINE, SAYS LITTLE. Every Guid of the baseline reads
   empty, line 63, as 5z-n found. So part A shows no item gained or lost a Guid, and cannot tell
   two items apart by Guid. PART B is where a Guid was there to keep, lines 84 to 115:

```
                                             items   viewpoints   Guids set   rows that differ   ResolveGuid gave the item at its index path
after the adds of 2 folders and 12 views      2882         2858          14                  -   14 of 14
after RemoveAt of P13 view 05, the middle     2881         2857          13                  0   -
after 5 RemoveAt from the end                 2876         2852           8                  0   8 of 8 kept
after a save, a clear and a reopen            2876         2852           8                  0   8 of 8 kept
P13 WITH GUIDS SET YES
```

   The middle removal moved the 6 later siblings one lower and nothing else, lines 91 and 92.
   Removing from the end moved nothing, line 104. The 6 removed Guids each resolved to null after
   the series and after the reopen, lines 105 and 111. Each kept Guid resolved to the item of
   its name at its index path at every stage, lines 87, 105 and 111
7. THE FILES. SaveFile took 8.890 s and wrote 41,287,793 bytes, 29,478 fewer than the copy it
   opened, line 73. Why it is smaller is UNKNOWN, since the probe did not save an unchanged copy
   to compare. Document.Clear took 0.572 s and the reopen 5.036 s, lines 74 and 75. The saved
   copies are
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-remove-20261007-133308\p13-remove-saved.nwf`, sha256
   24E447903F1E8074D1145C39285C516B930C2341E3407D18ACE334EC441860C1, line 117, holding 2846
   viewpoints and no P13 item, and `p13-remove-saved-sentinels.nwf` beside it, sha256
   069DA5B5C23F6EF540DAFE47E1E31076DBA9F84EDA929EDC01C38C0C5BD8C29B, line 118, which holds part
   B's folders and 6 views as well

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD.** The NWF the copy was made from read sha256
0944C100 at the start and at the end, lines 22 and 220. The guard wrote the Auto-Save switch
"3 0" and read it back, line 28. The watchdog saw Roamer 35736 once with no readable start time and
read it at the next pass, lines 130 and 131, saw no other Navisworks, and the put back ran, line
169. 38 registry values were put back, enable among them, line 174, and read again with 0 still
differing, line 209. InfoCenter.log and LastSession.xml were put back reading their backups'
sha256, lines 213 and 214. The guard saw 0 AutoSave files added, changed or gone, line 216, and the
tool's own logs folder had nothing added or changed, line 217. The prober read the switch and
listed the AutoSave folder by name, size, write time and sha256 with `read-autosave-state.ps1`,
kept in %LOCALAPPDATA%\NwcFederatorLoop\turn5 as `probe-p13-20261007-autosave-before.txt`,
`-during.txt` and `-after.txt`: enable read String "0" at 13:32:44 before the start, "3 0" at
13:35:43 while the probe ran, and "0" at 13:38:09 after the put back, and the folder held the same
199 files with the same sizes, times and sha256 before and after, 0 lines differing.

THE PROGRAMS. One Navisworks, pid 35736, started by the probe at 13:33:16, quit by Dispose and
gone 8.8 s after, not forced, lines 121, 122 and 161. AdskLicensingInstHelper pid 23512 under
GenuineService.exe read exited at the end. AdskLicensingInstHelper pid 38728 read UNKNOWN whether it
exited, its start time not readable when seen, line 163. No Roamer that was not there in step 2 ran
at the end, line 164.

**STILL UNKNOWN.**

- what 2813 calls cost in a row. One call in the tree of 2847 and six in a tree of 2852 to 2858 were
  timed, and nothing here says the rate holds across thousands of calls or with the Saved
  Viewpoints window open
- a view the tool records through COM, with its hidden state and colours, removed by RemoveAt.
  Part A's view is one of the baseline's per-clash views and part B's are .NET SavedViewpoints
- the one-argument Remove(SavedItem) and Remove(GroupItem, SavedItem) on a nested viewpoint. 5z
  found the first fails quietly on a nested set, and neither was called here
- a whole folder in one call, which is P14, on `p13-remove-saved.nwf`
- whether RemoveAt goes on the undo stack, and whether the Saved Viewpoints window shows the
  change at once. Neither was read
- why the saved file is 29,478 bytes smaller than the copy it opened
- a reopen in a new Navisworks. The close was Document.Clear inside the same Navisworks
- whether a removed view's name, reused by a later view in the same folder, finds anything of the
  removed one. Not tried

**WHAT THIS DECIDES.** By the row of P13, YES: removal is built. RemoveAt(parent, index), with the
parent resolved fresh from RootItem by its names and the target re-found by its name just before,
takes exactly the one viewpoint, leaves every other item with its folder, name, kind, Guid and
comments, holds through a save and a reopen, and touches no model, set, test, result or status.
The later siblings of the removed item each move one index lower, so an index read before a
removal names another item after it, as 5z found on sets. That is why the design removes from the
end within each parent and re-finds each target just before. A Guid set before AddCopy survives
removals around it and a save, and ResolveGuid follows the item to its new index path.

## 5z-v. DOES REMOVEAT TAKE A WHOLE FOLDER OF VIEWPOINTS IN ONE CALL, MEASURED 2026-10-07

P14 of Q114, the views by team design, part 3. P13, 5z-u, found that RemoveAt(parent, index), the
parent resolved fresh, takes one viewpoint and nothing else. F114 has 2813 per-clash views of F85's
to remove from the baseline, 2617 of them in the one top level folder `AR vs AR`, 5z-n. The
question: does RemoveAt(parent, index) on a FOLDER remove it with every view under it in one call,
and how many seconds does that take for the AR vs AR folder of 2617? By the row of P14: Yes, a
folder whose every item is the tool's goes in one call. No, one view at a time from the end, timed.
The row works on P13's copy. P13 said YES, so the row runs.

HOW. `tools\probes\ViewpointProbe\probe-folder-remove.ps1` is P13's `probe-view-remove.ps1` with the
new mode `vpfolder` of `ViewpointProbe.dll` in place of `vpremove`, its own header, work folder
prefix and save names. Nothing else in the script changed, F138's SwitchAutoSaveOff and the guard
included. The branch was pulled first and was up to date at ccd661e. tools\loop\nw-guard.ps1 read
sha256 E29D2733, the same as in P11 to P13, line 4. Get-Process Roamer read 0 before the run and 0
after it. The probe copied P13's saved file,
`%LOCALAPPDATA%\NwcFederatorLoop\probes\view-remove-20261007-133308\p13-remove-saved.nwf`,
41,287,793 bytes, sha256 24E44790, 2846 viewpoints, into the new folder
`probes\folder-remove-20261007-140536`, and the mode, on that copy:

1. PART A. Reads the whole saved viewpoint tree from RootItem, one row per item: index path,
   folder names, name, kind, Guid and comment count, as P13 did. Counts models, sets, tests,
   results and statuses a person set
2. picks the top level folder holding the most viewpoints under it. No folder is named in the
   code. It counts the rows under it off the tree
3. reads RootItem fresh, re-finds the folder in it by its name, Ordinal, and calls
   `DocumentSavedViewpoints.RemoveAt(root, index)`, timed by a Stopwatch around the call alone
4. reads the tree again and compares it row by row, in tree order, with the tree at the open less
   the folder and every row under it, where only the later siblings and the rows under them move
   one lower at that level. Looks the folder up by its name
5. saves into `p14-folder-saved.nwf` in the work folder, calls Document.Clear and TryOpenFile of the
   saved file, reads the tree and the counts once more and compares with the tree after the
   removal, exactly
6. PART B, because every Guid of the baseline reads empty: in the reopened document adds `P14 probe`
   at the root and in it `P14 keep` of 3 views, `P14 gone` of 12 views with `P14 gone inner` of 3
   views after them, and `P14 after` of 2 views, 25 items, each with a Guid set before its AddCopy.
   Removes `P14 gone` by one RemoveAt(parent, index), the parent resolved fresh by its name, timed.
   Compares the tree, reads every Guid through ResolveGuid, the kept ones counted when they give
   the item of their name at its index path and the removed ones when they give null, saves into
   `p14-folder-saved-sentinels.nwf`, clears, reopens and reads again
7. PART C, the other branch of the row, timed on the same folder: Document.Clear, TryOpenFile of the
   untouched copy, the tree compared with the first open, then the folder's views removed one at a
   time from the end, each round resolving the folder fresh by its name, reading the last child's
   kind and calling RemoveAt(parent, last), capped at 600 s. Then the emptied folder by its own
   RemoveAt, and the tree compared with part A's after its one call. Part C saves nothing

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-folder-remove.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p14-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\probes\view-remove-20261007-133308\p13-remove-saved.nwf

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
0 warnings and 0 errors, DLL sha256 8385AA5B, script sha256 9B3F4884, lines 24 and 3. One run at
14:05, kept as `p14-folder-remove-result-20261007.txt`, the machine name on line 1 masked as
`[machine]` and the account folder on line 5 as `%USERPROFILE%`, nothing else changed. Navisworks
pid 37348, adopted on all four conditions, line 38. TryOpenFile of the copy returned True after
5.034 s with 4 models, every one read from under the loop folder, lines 54 to 59. ExecuteAddInPlugin
returned 0 after 41.79 s, line 48. Dispose returned and pid 37348 was gone 7.4 s later, not forced,
line 127.

**THE ANSWER: YES. ONE CALL TOOK THE AR VS AR FOLDER AND ALL 2617 VIEWPOINTS IN IT IN 0.174 S, AND
NOTHING ELSE, AND IT HELD THROUGH A SAVE AND A REOPEN.** Lines 61 to 82 of the result:

```
                                     at the open   after RemoveAt   after save, clear, reopen
items in the viewpoint tree                 2868              250                         250
viewpoints                                  2846              229                         229
folders                                       22               21                          21
Guids not empty                                0                0                           0
models, sets, tests, results,     4, 61, 528, 2939, 0    the same                    the same
statuses a person set
rows that differ from the expected tree                          0                           0
the folder found by its name                 yes          nothing                     nothing
P14 YES
```

1. THE TARGET. `AR vs AR`, the top level folder at index path 4, 2617 direct children, all
   viewpoints, no folder under it, and 229 viewpoints elsewhere in the tree, line 64. The root read
   fresh held 16 children and the name was found on 1 of them, a folder, at the same index the tree
   read at the open, lines 65 and 67. The root read again after the call held 15, line 66
2. ONE CALL, 0.174 S. RemoveAt(root, 4) returned after 0.174 s in the tree of 2846 viewpoints and
   took 2618 items, the folder and its 2617 viewpoints, lines 65 and 81
3. NOTHING ELSE MOVED BUT THE LATER SIBLINGS. After the call the tree read 250 rows, and row by row
   in tree order every one had the same folder names, name, kind, Guid and comment count as the
   tree at the open less the folder and its rows, 0 differing, line 71. The 11 later top level
   folders and the rows under them, 212 rows, each read one lower at the top level, and no other index path changed,
   lines 70 and 71
4. IT HELD THROUGH A SAVE AND A REOPEN. After SaveFile, Document.Clear and TryOpenFile of the saved
   file the tree read the same 250 rows, index paths included, 0 differing, line 78. The folder's
   name found nothing after the call and after the reopen, lines 72 and 79
5. NOTHING ELSE IN THE DOCUMENT CHANGED COUNT. 4 models, 61 sets, 528 tests, 2939 results and 0
   statuses a person set at the open, after the call and after the reopen, lines 62, 69 and 77
6. TWO DEEP, WITH A FOLDER INSIDE AND GUIDS SET, THE SAME. Part B, lines 84 to 102:

```
                                             items   viewpoints   Guids set   rows that differ   ResolveGuid
after the adds of 5 folders and 20 views       275          249          25                  -   25 of 25 at their index path
after one RemoveAt on P14 gone                 258          234           8                  0   8 of 8 kept found, 17 of 17 removed null
after a save, a clear and a reopen             258          234           8                  0   8 of 8 kept found, 17 of 17 removed null
P14 TWO DEEP WITH GUIDS SET YES
```

   `P14 gone` sat at index path 15.1 with 13 direct children and 15 viewpoints under it. One
   RemoveAt took it, the 12 views, `P14 gone inner` and its 3 views, 17 items, in 0.002 s, line 88.
   Only `P14 after` and its 2 views moved, one lower, line 92. `P14 keep` and its views kept their
   index paths and Guids, lines 91, 93 and 99
7. ONE AT A TIME FROM THE END COST 115 TIMES AS LONG. Part C, lines 104 to 120. The untouched copy
   reopened read the same 2868 rows as the first open, 0 differing, line 108. The 2617 views of
   AR vs AR went by 2617 calls of RemoveAt(parent, last), none threw, every item removed was a
   viewpoint, line 109:

```
RemoveAt alone, 2617 calls        total 20.012 s   mean 0.007647 s   median 0.007474 s   least 0.001369 s   most 0.018294 s
each round, the folder resolved
fresh, the last child's kind
read and RemoveAt                 total 20.063 s   mean 0.007667 s   wall time of the series 20.064 s
the first 10 calls                mean 0.008074 s
the last 10 calls                 mean 0.006359 s
the emptied folder's own RemoveAt          0.007 s
```

   lines 110 to 115. After the series the tree read the open less the 2617 views with the folder
   kept, 0 differing, line 114. After the emptied folder's own RemoveAt the tree read exactly part
   A's after its one call, 0 differing, line 118. So the two routes end in the same tree, and one
   call on the folder took 0.174 s against 20.064 s for the series, about 115 times faster. The
   mean call in the series, 0.0076 s, is about 4 times P13's single call of 0.002 s in a tree of
   2847. Why is UNKNOWN
8. THE FILES. SaveFile after part A took 2.806 s and wrote 7,336,192 bytes, 33,951,601 fewer than
   the 41,287,793 bytes of the copy opened, about 12,973 bytes for each of the 2617 viewpoints
   removed, line 73. The reopen took 2.895 s against 5.034 s for the first open, lines 54 and 75.
   The saved copies are
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\folder-remove-20261007-140536\p14-folder-saved.nwf`,
   sha256 5617FD2F1C96EE3C6CB89FA271F86B74F2EC6B59135BE0E956C0A932B5ED52B4, line 122, holding 229
   viewpoints and no AR vs AR, and `p14-folder-saved-sentinels.nwf` beside it, sha256
   C028187432B13FCF3DFC84D9E9A15D195D3ED89BAEB5318AC65EDBA2C6D2C9A9, line 123, which also holds
   part B's `P14 probe` with `P14 keep` and `P14 after`

**THE CLOSE WAS RECORDED AS A CRASH.** Dispose returned after 0.89 s and pid 37348 was gone 7.4 s
later, not forced, lines 126 and 127. But the put back read Navisworks' own counter
`22.0\CER\22.5.1433.58 crashCount` gone from 40 to 41, line 170, with no change to
SessionCleanCloseCount. In P11, P12 and P13 the same counters read SessionCleanCloseCount 93 to 94
and no crashCount change. After the run the prober listed, read only, `%TEMP%\NavisWorksErrorReport.dmp`,
3,018,030 bytes, sha256 475AE3C9, written 14:08:00, which is between Dispose and the process
going, and created 2026-09-17 13:03:52. So that file was there before this run and Navisworks wrote
over it as it closed. What it held before is UNKNOWN. The guard put crashCount back to 40 with the
other values. Why this close crashed is UNKNOWN. One difference from P13 is seen and not tested:
P14 left the document with part C's removals unsaved when Dispose was called, and P13 left it as
just reopened.

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD.** The NWF the copy was made from read sha256
24E44790 at the start and at the end, lines 22 and 217. The guard wrote the Auto-Save switch "3 0"
and read it back, line 28. The watchdog saw no other Navisworks, and the put back ran, line 168. 36
registry values were put back, enable among them, line 173, and read again with 0 still differing,
line 206. InfoCenter.log and LastSession.xml were put back reading their backups' sha256, lines 210
and 211. The guard saw 0 AutoSave files added, changed or gone, line 213, and the tool's own logs
folder had nothing added or changed, line 214. The prober read the switch and listed the AutoSave
folder by name, size, write time and sha256 with `read-autosave-state.ps1`, kept in
%LOCALAPPDATA%\NwcFederatorLoop\turn5 as `probe-p14-20261007-autosave-before.txt`, `-during.txt` and
`-after.txt`: enable read String "0" at 14:05:12 before the start, "3 0" at 14:06:07 while the
probe ran, and "0" at 14:08:38 after the put back, and the folder held the same 199 files with the
same sizes, times and sha256 before and after, 0 lines differing.

THE PROGRAMS. One Navisworks, pid 37348, started by the probe at 14:05:44, quit by Dispose and gone
7.4 s after, not forced, lines 127 and 159. AdskLicensingAgent pid 37836, a child of pid 37348, read
STILL RUNNING at the end of the probe, line 161, and Get-Process -Id 37836 found nothing when the
prober read it after the run. AdskLicensingInstHelper pids 44616 and 14392 under GenuineService.exe
read exited, lines 160 and 162. No Roamer that was not there in step 2 ran at the end, line 163.

**STILL UNKNOWN.**

- why this close was recorded as a crash and wrote NavisWorksErrorReport.dmp, and whether a folder
  removal, part C's 2617 calls or a document left with unsaved removals causes it. Each was done
  once in this run and none alone
- why one call in part C's series took about 4 times P13's single call
- a folder whose views the tool recorded through COM, with hidden state and colours. Part A's are
  F85's per-clash views and part B's are .NET SavedViewpoints
- a folder that holds a person's view among the tool's. The row decides one call only where every
  item is the tool's, and no mixed folder was tried
- whether RemoveAt goes on the undo stack, and whether the Saved Viewpoints window shows the change
  at once. Neither was read
- a reopen in a new Navisworks. The close was Document.Clear inside the same Navisworks
- whether the 12,973 bytes per viewpoint holds for views the tool records

**WHAT THIS DECIDES.** By the row of P14, YES: a folder whose every item is the tool's goes in one
call. RemoveAt(parent, index) on a folder, with the parent read fresh and the folder re-found by its
name just before, takes the folder and everything under it, folders inside included, leaves every
other item with its folder, name, kind, Guid and comments, holds through a save and a reopen, and
touches no model, set, test, result or status. Every Guid under the removed folder resolves to
nothing after. On AR vs AR the one call took 0.174 s where one view at a time from the end took
20.064 s and ended in the same tree.

## 5z-w. DO THE VIEWPOINTS AND THEIR MARKS COME BACK THROUGH THE CLEAR AND REBUILD FALLBACK, NOT ANSWERED 2026-10-07

P15 of Q114, the views by team design, part 3. The question: after DocumentSavedViewpoints.CreateCopy,
Document.Clear, the appends and CopyFrom, does every viewpoint come back with its comments and its
Guid? By the row of P15: Yes, FR-067's fallback copies the viewpoints and the marks hold. No, the
fallback keeps refusing to save as today, and a rebuilt group's marks are named as lost.

Written up on 2026-10-07 by the prober of P16 from the two runs' files, as they stand. The prober of
P15 stopped on the guard's refused put back and wrote no section and committed nothing. Nothing below
was run again.

HOW. `tools\probes\ViewpointProbe\probe-rebuild-views.ps1`, sha256 E817D8E0, is P14's
`probe-folder-remove.ps1` with the new mode `vprebuild` of `ViewpointProbe.dll`, the method
MeasureRebuildViews, its own header, work folder prefix and save names. The guard is the same,
tools\loop\nw-guard.ps1 sha256 E29D2733, line 4. It works on a fresh copy of run set 03's
`runs\03\NMFed\NWF\C06\1104-PAR-1B06PE-ZZZ-BM-MOD-000001.nwf`, 173,526 bytes, sha256 EAB49AEF, line 22.
Part 1 marks the copy the way Q114's design marks the tool's views, saves it as `p15-marked.nwf`,
clears and reopens it. Part 2 runs FederationEngine's clear and rebuild in its order, and part 3 was to
save the rebuilt document, clear, reopen it and read every item again. Run as

    powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-rebuild-views.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p15b-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\03\NMFed\NWF\C06\1104-PAR-1B06PE-ZZZ-BM-MOD-000001.nwf

TWO RUNS, BOTH ENDED IN A NAVISWORKS FAILURE.

- RUN 1 at 14:43, work folder `probes\rebuild-views-20261007-144356`. Its plugin output ends at
  14:46:06.242 with `Document.Clear took 0.099 s, models now 0, viewpoints now 0`. The next step read
  the viewpoints copy after the clear, and nothing more was written. A WerFault, pid 46072, was seen
  at 14:46:17, and ExecuteAddInPlugin threw `Error calling method: -2147417851` after 139.28 s,
  probe-p15-result.txt lines 48 and 78. Its result is kept in turn5 and is not committed
- RUN 2 at 14:59, work folder `probes\rebuild-views-20261007-145943`. The only change is that the
  plugin no longer reads the copy after Document.Clear, plugin output line 42. Its result and its
  plugin output, the machine name and the account folder masked and nothing else changed, are
  `p15-rebuild-views-result-20261007.txt` and `p15-rebuild-views-plugin-output-20261007.txt`. The
  plugin output was copied after the run, because the guard's script could not read it while
  Navisworks held it, result line 53. The lines below are of those two files

The probe as it ran is committed as it stood after run 2: `ViewpointProbePlugin.cs` with the mode
vprebuild, built at 14:58:47 into `ViewpointProbe.dll` sha256 7FAF0E7D, result line 24, and the script
above.

**THE ANSWER: NOT ANSWERED. WHETHER THE COMMENTS AND THE GUIDS COME BACK IS UNKNOWN.** What run 2
measured, plugin output lines 11 to 53:

```
                                                  views  folders  comments  Guids set   models sets tests results
the marked copy reopened, before the copy            36       10         4          2        4   61   153      37
the viewpoints copy, before the clear, 0 rows differ 36       10         4          2
Document.Clear, 0.112 s                               0                                       0
after the 4 TryAppendFile calls, each True           17        4         0          0        4    0     0       0
after the CopyFrom of the sets and of the tests      17                                       4   61   153      37
after DocumentSavedViewpoints.CopyFrom, 0.005 s      36                                       4   61   153      37
```

1. PART 1 HELD. The marks were written and read back after a save, a clear and a reopen: a COM view
   with model 0 hidden and a pair painted, marked by AddComment, its folder marked, a .NET view with
   its Guid set and two comments, and a plain view. ResolveGuid found 2 of 2, lines 13 to 35
2. THE COPIES RETURNED. CreateCopy of the tests, the sets and the viewpoints returned in 0.001 s,
   0.001 s and 0.000 s, and the viewpoints copy read 46 of 46 rows the same as the document before the
   clear, lines 36 to 40
3. THE APPENDS BROUGHT 17 VIEWS OF THEIR OWN. After the clear and the 4 appends the document held 17
   viewpoints in 4 folders, with no comment and no Guid, line 48. Those are the views the NWCs carry
4. COPYFROM LEFT 36. After DocumentSavedViewpoints.CopyFrom the document read 36 viewpoints, the
   count before the copy and not 17 plus 36, line 53. That is consistent with CopyFrom replacing what
   the appends brought. It is read off the count alone and not item by item
5. THEN NAVISWORKS FAILED. The next step walks the document's own tree after CopyFrom, reading each
   item's name, Guid, comments, Hidden count and first hidden items and MaterialOverrides count. No line
   of that walk was written. A WerFault, pid 51252, a child of the probe's Roamer 49280, was seen at
   15:01:36, result line 79, and ExecuteAddInPlugin threw `Error calling method: -2147417851` after
   94.74 s, line 48. The save, the clear and the reopen of the rebuilt file never ran

Both runs failed at the same kind of step, the first read of viewpoint items after a Document.Clear
that were copied out before it: in run 1 the copy itself, in run 2 the document after CopyFrom put the
copy back. Which read fails is UNKNOWN.

**BADER'S THINGS.** The guard refused the put back in both runs, because a Roamer was seen at one
watchdog pass with no readable start time, pid 48540 in run 1 and pid 3976 in run 2, result lines 96 and 97,
and it cannot be shown to be the adopted one. 36 registry values differed and were not written, the
Auto-Save switch among them, left at "3 0", lines 98 to 137 of run 2's result. The AutoSave folder had
0 files added, changed or gone, line 141. The NWF the copy was made from read sha256 EAB49AEF at the
start and the end, lines 22 and 145. Steps\history\log.md on main, commit dc80e2b, records his settings put
back after the first crash. On 2026-10-07 at 15:22 the prober of P16 read the switch as String "0"
and the AutoSave folder as the same 199 files, names, sizes, times and sha256, as before run 2, in
`turn5\probe-p16-20261007-autosave-precheck.txt`. Who put the switch back after run 2 is not in any
file this section read.

THE PROGRAMS. In run 2 Roamer pid 49280 was started by the probe and adopted on all four conditions,
line 38, never reached Dispose, and was closed through the held handle by its own pid and read gone,
line 56. Roamer 3976 was seen once at 15:01:31, line 78, and no Roamer that was not there in step 2 ran at
the end, line 91.

**STILL UNKNOWN.**

- whether the comments and the Guids are on the views after CopyFrom
- which property read fails after a Document.Clear: DisplayName, Guid, Comments,
  GetVisibilityOverrides().Hidden and its items, or the material count
- whether the rebuilt document saves, and whether the views hold through a reopen
- whether the engine itself would fail. It does not walk the views after a fallback today, and
  F114's VIEWS TREE read would
- what Roamers 48540 and 3976 were. Each was seen once with no readable start time, as Navisworks
  was failing

**WHAT THIS DECIDES.** Nothing yet. The row of P15 is not answered, so FR-067's fallback is not known
to keep the marks. A next run would read the count and the names first, then each property in its own
call, writing a line before each read, to name the read that fails.

## 5z-x. DOES ZOOMBOX ON THE FIRST CLASH'S CAMERA FRAME EVERY OPEN CLASH OF A TEST, MEASURED 2026-10-07

P16 of Q114, the views by team design, part 3. The question: does Viewpoint.ZoomBox on a copy of the
first open clash's camera, with the box of the test's open clash centres padded by the margin, keep
the view direction and put every centre inside the recorded view after a reopen, each centre
projected with the window's HeightField, 5m? By the row of P16: Yes, FramingBox and ZoomBox. The
direction changes but every centre is in: used, and the change said. No: ViewFraming's arithmetic is
built and probed the same way. Neither: the view keeps its first clash's camera, VIEWS TREE says it is
not framed on all its clashes, and Bader is told point 13 is at risk. The row depends on P5, which
said YES, 5z-h, so it runs.

HOW. `tools\probes\ViewpointProbe\probe-view-framing.ps1` is P14's `probe-folder-remove.ps1` with the
new mode `vpframe` of `ViewpointProbe.dll`, the method MeasureFraming, a parameter `-MarginMm` handed
on to the mode, and the plugin's output read with the file shared, because P15's script could not read
it while a failing Navisworks held it, 5z-w. Nothing else in the script changed, F138's
SwitchAutoSaveOff and the guard included. The branch was pulled first, up to date at 5f6d0f0, and P15's
leftovers were committed as 5871d64 before this probe was written. tools\loop\nw-guard.ps1 read sha256
E29D2733, line 4. Get-Process Roamer read 0 before the run and 0 after it. The probe copied the
baseline `runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`, 41,317,271 bytes, sha256
0944C100, into the new folder `probes\view-framing-20261007-155007`, line 22, and the mode, on that
copy:

1. reads every test and counts its results and its open results, open being New or Active, the
   design's ViewStatuses. It takes the test with the most open clashes and the test with the fewest
   open clashes above one. No test is named in the code
2. for each, walks the results in tree order, reads every open result's Center, and takes the first
   open result's camera from `DocumentClashTests.TestsViewpointForResult`, copied by CreateCopy
3. builds the box over the open centres, each side padded by the margin turned into document units, by
   `new BoundingBox3D(Point3D, Point3D)`, and calls `ZoomBox(box)` on a copy of that camera
4. reads both cameras: position, rotation, the direction and up turned from (0, 0, -1) and (0, 1, 0)
   by the rotation, HeightField, AspectRatio, projection, focal distance and the extents at it.
   Counts the centres inside each, a centre being inside when it is in front of the camera and the
   tangent of its angle off the axis is within half the field up and down and aspect times that
   across, and prints the largest reach, 1 being the edge
5. records the zoomed camera and the first clash's own camera through the COM view, the tool's route,
   5m, into a folder `P16 probe` at the root, and reads each back
6. saves into `p16-framed.nwf` in the work folder, calls Document.Clear and TryOpenFile of the saved
   file, reads every open centre again, then reads each view as recorded, presses it through
   CurrentSavedViewpoint, reads the window's camera off Document.CurrentViewpoint, and counts the
   centres inside with the window's position, direction, field and aspect

The margin is 500 mm, chosen for this probe and not measured, the design's FramingMarginMillimetres
having no value yet. Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-framing.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p16-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf -MarginMm 500

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
0 warnings and 0 errors, DLL sha256 051157AC, script sha256 00CF5B4A, lines 24 and 3. One run at
15:50, kept as `p16-view-framing-result-20261007.txt`, the machine name on line 1 masked as
`[machine]` and the account folder on line 5 as `%USERPROFILE%`, nothing else changed. Navisworks pid
40876, adopted on all four conditions, line 38. TryOpenFile of the copy returned True after 4.293 s
with 4 models, every one read from under the loop folder, lines 54 to 59. ExecuteAddInPlugin returned
0 after 43.16 s, line 48. Dispose returned and pid 40876 was gone 8.5 s later, not forced, line 158.

**THE ANSWER: YES. ZOOMBOX KEPT THE VIEW DIRECTION EXACTLY AND PUT ALL 2568 OPEN CENTRES OF BLD-AR-CURTAIN
MULLIONS-VS-BLD-AR-WINDOWS INSIDE THE VIEW, AND IT HELD THROUGH A SAVE, A REOPEN AND A PRESS IN THE
WINDOW.** Lines 63 to 152 of the result:

```
                                         open    in view on the     in view, zoomed,   in view, zoomed view    largest reach,
                                      centres    first clash's       before the save    pressed in the window   1 is the edge
                                                 camera                                 after the reopen
many  BLD-AR-Curtain Mullions-          2568     114, 1514 behind        2568                 2568                  0.933344
      vs-BLD-AR-Windows
few   BLD-ST-Floors-vs-BLD-ST-Framing      2     2                       2                    2                     0.450532

direction moved by ZoomBox, both tests                     0.0000 deg, up 0.0000 deg
direction, first clash's camera to the window after the reopen, both tests    0.0000 deg
P16 YES
```

1. THE TESTS. 528 tests, 469 with no open clash, 11 with one and 48 with two or more, line 63. The
   test of most open clashes is BLD-AR-Curtain Mullions-vs-BLD-AR-Windows, 2568 results, all 2568
   open, line 64, the test the row names. The test of fewest above one is BLD-ST-Floors-vs-BLD-ST-
   Framing, 2 results, both open, line 65
2. THE CONVENTION IS MEASURED. On both first clash cameras the first clash's own centre lies 0.0000 deg
   off the axis turned from (0, 0, -1), by the rotation read as an axis and an angle and as a
   quaternion with A, B and C its vector part and D its scalar, the two agreeing to 0.0000 deg, lines
   72 and 88. So a camera looks along its rotation of (0, 0, -1), and TestsViewpointForResult points it
   straight at the clash. Every count below rests on that. Both cameras read field 0.785398, the
   field the extents at the focal distance imply, and AspectRatio 2.635569, equal to the extents'
   horizontal over vertical, lines 71 and 87. So HeightField is the full vertical angle in radians
3. THE FIRST CLASH'S CAMERA ALONE FRAMES 114 OF 2568. The fallback of the row, the view keeping its
   first clash's camera, shows 114 of the many test's open centres, with 1514 behind the camera, line
   73, and the same 114 when pressed after the reopen, line 124. On the few test it shows both, line 89
4. ZOOMBOX MOVES THE CAMERA BACK ALONG ITS OWN AXIS AND CHANGES NOTHING ELSE READ. On the many test the
   box was 101.932 by 101.824 by 91.547 units, line 74. ZoomBox returned in 0.001 s, line 75, and moved
   the position 229.139 units, with the direction, the up, the field and the aspect the same to 0.0000
   deg and to six places, lines 76 and 77. It left the focal distance at 3.729, the first clash's,
   line 76. On the few test it moved the position 105.049 units and nothing else, lines 92 and 93
5. EVERY CENTRE IN. The zoomed camera holds 2568 of 2568, the largest reach 0.933344 up and down and
   0.350665 across, line 78, and 2 of 2 on the few test, reach 0.450532, line 94
6. IT HELD THROUGH THE RECORD, A SAVE, A REOPEN AND A PRESS. Each view read back off its folder with
   the position moved 0 and the direction 0.0000 deg, lines 80, 82, 96 and 98. After SaveFile, 7.941 s,
   line 101, Document.Clear and TryOpenFile of the saved file, the open centres read the same 2568 and
   2 in the same order, a largest difference of 0, lines 107 and 132. Pressed, the window's camera read
   the recorded position, direction, field and aspect exactly, and held 2568 of 2568 and 2 of 2, lines
   110 to 113 and 135 to 138. The five centres nearest the edge on the many test all lie at X 130.756,
   reach 0.885 to 0.933, lines 114 to 118
7. THE WINDOW'S FIELD WAS THE RECORDED ONE IN THIS RUN. 5m read the window's field 0.953 after pressing
   a view given 0.785. Here the window read 0.785398 and the aspect 2.635569 on every press, the values
   the clash camera came with, lines 112, 123, 137 and 145. Whether a window of another shape or size
   shows the same is UNKNOWN. Off the numbers above, and only if Navisworks holds the vertical field
   when the window's shape changes, which is UNKNOWN, the many test's view stays whole across in any
   window of aspect at or above 0.924, 0.350665 times 2.635569, and up and down it has 7 per cent to
   spare
8. WHAT THE RECORD COST. Each of the 4 views took 6.143 s to 6.737 s to record through the COM view
   into its folder in a tree of 2847 viewpoints, lines 79, 81, 95 and 97, where ZoomBox took 0.001 s.
   That is read once per view and is not P18's measurement of the per view sequence
9. THE DOCUMENT. 4 models, 61 sets, 528 tests, 2939 results and 0 statuses a person set at the open
   and after the reopen, viewpoints 2847 then 2851, the 4 the probe recorded, lines 62, 100 and 104.
   The document is in feet, line 60. The saved copy is
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-framing-20261007-155007\p16-framed.nwf`, 41,317,755
   bytes, sha256 9DC99432238D1CBFD6183E39D0B16258C386DEB9E999E47BEC06D2B1A1E5FD6E, line 154. It holds
   the folder `P16 probe` with `P16 many framed`, `P16 many first clash camera`, `P16 few framed` and
   `P16 few first clash camera`

**BADER'S HAND STEP IS NOT DONE.** The row has Bader press both views by hand as one step. Nothing here
shows what the picture on his screen looks like. The four views above are the ones to press, in the
saved copy named in item 9, never in NM Fed.

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD.** The NWF the copy was made from read sha256
0944C100 at the start and at the end, lines 22 and 252. The guard wrote the Auto-Save switch "3 0" and
read it back, line 28. The watchdog saw no other Navisworks, and the put back ran, line 201. 38
registry values were put back, enable among them, line 206, and read again with 0 still differing,
line 241. SessionCleanCloseCount went 93 to 94 and no crashCount changed, line 203, so this close was
not recorded as a crash. InfoCenter.log and LastSession.xml were put back reading their backups'
sha256, lines 245 and 246. The guard saw 0 AutoSave files added, changed or gone, line 248, and the
tool's own logs folder had nothing added or changed, line 249. The prober read the switch and listed
the AutoSave folder by name, size, write time and sha256 with `read-autosave-state.ps1`, kept in
%LOCALAPPDATA%\NwcFederatorLoop\turn5 as `probe-p16-20261007-autosave-before.txt`, `-during.txt` and
`-after.txt`: enable read String "0" at 15:49:44 before the start, "3 0" at 15:50:49 while the probe
ran, and "0" at 15:53:21 after the put back, and the folder held the same 199 files with the same
sizes, times and sha256 before and after, 0 lines differing.

THE PROGRAMS. One Navisworks, pid 40876, started by the probe at 15:50:14, quit by Dispose and gone 8.5
s after, not forced, lines 158 and 161. AdskLicensingAgent pid 51192, a child of pid 40876, and
AdskLicensingInstHelper pids 41036 and 5088 under GenuineService.exe all read exited, lines 193 to
195. No Roamer that was not there in step 2 ran at the end, line 196.

**STILL UNKNOWN.**

- what the views look like on Bader's screen. His hand step has not run
- whether a window of another shape or size changes the field or the aspect a pressed view shows,
  and so whether a centre near the edge, reach 0.933, leaves the picture. This run's window read the
  recorded values. 5m's did not
- whether a centre inside the view is also in front of the near clipping plane, and whether the clash
  is drawn and not hidden behind other geometry. Only the centre was projected, not the items
- why ZoomBox leaves the focal distance at the first clash's value, and what that does to orbiting
  in the pressed view
- why one COM record took 6.1 s to 6.7 s here. P18 measures the per view sequence
- a test whose open clashes are spread so wide that the view from the first clash's direction sees
  them edge on. Two tests were tried, both from the same direction, (-0.577, 0.577, -0.577)
- a margin other than 500 mm
- that Center and the camera are in the document's units, feet here. The margin was turned into feet on
  that assumption, and it was not read

**WHAT THIS DECIDES.** By the row of P16, YES: FramingBox and ZoomBox. A copy of the first open
clash's camera from TestsViewpointForResult, zoomed to the box of the test's open clash centres padded
by the margin, keeps the clash camera's direction, up, field and aspect exactly, moves only back along
its own axis, and holds every open centre in view through the COM record, a save, a reopen and a press.
ViewFraming's camera arithmetic is not built.

## 5z-y. DO ONE RESET AND ONE PAINT PER COLOUR OVER EVERY CLASHING ITEM OF A TEST RECORD INTO ONE VIEW, MEASURED 2026-10-07

P17 of Q114, the views by team design, part 3. The question: do one ResetTemporaryMaterials over every
clashing item and one OverrideTemporaryColor per colour over the red and the green items of the
2568-clash test record into one view where every item reads back with the colour it will show, and
what are the seconds of each call and the NWF's bytes after a save? By the row of P17: Yes, one call
per colour per view. No, the calls go in chunks of a size that is a setting, and P17 runs again with
it. The row depends on no other probe.

HOW. `tools\probes\ViewpointProbe\probe-view-paint.ps1` is P16's `probe-view-framing.ps1` with the new
mode `vppaint` of `ViewpointProbe.dll`, the method MeasurePaint, its own header, the work folder prefix
`view-paint` and the save name `p17-painted.nwf`. Nothing else in the script changed, the guard and
F138's SwitchAutoSaveOff included. tools\loop\nw-guard.ps1 read sha256 E29D2733, line 4. The branch
was pulled first, up to date at f8ed445. Get-Process Roamer read 0 before each run and 0 after. The
probe copied the baseline `runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`, 41,317,271
bytes, sha256 0944C100, into a new work folder, line 22, and the mode, on that copy:

1. reads every test and takes the one with the most open clashes, open being New or Active, the
   design's ViewStatuses. No test is named in the code
2. walks its open results and reads each one's Item1 and Item2 index paths. Red is every distinct
   first item, green every distinct second item not already red, solid all of them, the design's
   PaintPlan. Each item's home model is the model whose root index path is a prefix of the item's
3. reads every item's OriginalColor and OriginalTransparency, makes the folder `P17 probe` at the
   root and saves the copy as `p17-plain.nwf`, so the next save differs from it by the one view
4. hides the models holding no clashing item by SetHidden on their roots, dims the others' roots by
   OverrideTemporaryTransparency 0.85, resolves every item once into three ModelItemCollections,
   then calls ResetTemporaryMaterials ONCE over the solid collection, OverrideTemporaryColor (1,0,0)
   ONCE over the red and OverrideTemporaryColor (0,1,0) ONCE over the green, each call timed
5. reads every item's ActiveColor and ActiveTransparency live, and every other geometry item of the
   shown models' ActiveTransparency
6. records one view, `P17 painted`, through the COM view with ApplyHideAttribs and ApplyMaterialAttribs
   true into the folder, on the first open clash's camera zoomed to the open centres padded by 500 mm
   as in 5z-x, and reads what the view WILL SHOW for each item, 5p: the colour and transparency of the
   view's own MaterialOverride where it names the item, the item's own where it does not
7. undoes the overrides and the hiding, saves as `p17-painted.nwf`, calls Document.Clear and
   TryOpenFile of the saved file, reads the view the same way again, presses it through
   CurrentSavedViewpoint and reads every item live again

An item is judged right when its colour is within 0.001 of (1,0,0) for red or (0,1,0) for green on
each channel, and solid when its transparency is within 0.001 of its own OriginalTransparency. Run
from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-paint.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p17-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf -MarginMm 500

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
0 warnings and 0 errors, script sha256 9C24A188, line 3.

TWO RUNS. Both are committed, the machine name on line 1 masked as `[machine]` and the account folder
on line 5 as `%USERPROFILE%`, nothing else changed.

- RUN 1 at 16:20, `p17-view-paint-run1-result-20261007.txt`, DLL sha256 29B10692, work folder
  `probes\view-paint-20261007-162024`, Navisworks pid 42692. Everything ran, but the read off the
  view threw NullReferenceException before the save and after the reopen, lines 84 and 91, so it
  ended P17 UNKNOWN, line 99. Its three calls, its live reads and its bytes read as run 2's do
- RUN 2 at 16:25, `p17-view-paint-result-20261007.txt`, DLL sha256 404B9295, line 24, work folder
  `probes\view-paint-20261007-162544`, Navisworks pid 43860 adopted on all four conditions, line 38.
  The only change to the mode: the read off the view reads MaterialOverride.Color as possibly null,
  and catches a throw per entry and counts it. ExecuteAddInPlugin returned 0 after 34.26 s, line 48.
  Dispose returned and pid 43860 was gone 7.7 s later, not forced, line 109. The lines below are run 2's

**THE ANSWER: YES. ONE RESET OVER 2423 ITEMS AND ONE PAINT PER COLOUR, 2335 RED AND 88 GREEN, RECORDED
INTO ONE VIEW WHERE EVERY ITEM READS BACK RED OR GREEN AND SOLID, LIVE, OFF THE VIEW, AND AFTER A SAVE,
A REOPEN AND A PRESS.** Lines 63 to 103 of the result:

```
the test BLD-AR-Curtain Mullions-vs-BLD-AR-Windows    results 2568, open 2568, 0 sides read null
red, distinct first items                              2335
green, distinct second items not already red             88    (no second item was first in any clash)
solid, all of them                                     2423    resolved 2423, without geometry 0
shown and dimmed: model 0, the AR model                hidden: models 1, 2 and 3

SetHidden on 3 model roots                                         0.050 s
OverrideTemporaryTransparency 0.85 on 1 root                       0.004 s
ResolveIndexPath of 2423 items into 3 collections                  0.016 s
ResetTemporaryMaterials, ONE call over 2423 items                  0.005 s
OverrideTemporaryColor (1,0,0), ONE call over 2335 items           0.003 s
OverrideTemporaryColor (0,1,0), ONE call over 88 items             0.000 s
the COM record of the view, a tree of 2847 viewpoints              6.487 s

                                      red right and solid   green right and solid   the rest of model 0 at 0.85
live, before the record                    2335 of 2335            88 of 88                730 of 730
off the view, before the save              2335 of 2335            88 of 88
off the view, after the reopen             2335 of 2335            88 of 88
live, the view pressed after the reopen    2335 of 2335            88 of 88                730 of 730

p17-plain.nwf, the empty folder and no view      41,317,276 bytes, SaveFile 8.799 s
p17-painted.nwf, the one painted view            41,329,885 bytes, SaveFile 8.243 s, 12,609 bytes more
P17 YES
```

1. THE TEST. 528 tests, and the one of most open clashes is BLD-AR-Curtain Mullions-vs-BLD-AR-Windows,
   2568 results, all open, line 63, the test the row names. Its 2568 open clashes name 2335 distinct
   first items and 88 distinct second items, and no second item is first in another clash, line 65.
   Every item resolves and has geometry, none of them is already red or green, and 88 carry their own
   transparency above 0, line 67. Every item lives in model 0, the AR model, so the view shows and
   dims that one and hides the other 3, line 66
2. THE CALLS. ResetTemporaryMaterials over 2423 items returned in 0.005 s, OverrideTemporaryColor
   over 2335 in 0.003 s and over 88 in 0.000 s, lines 76 to 78. Resolving the 2423 index paths into
   the collections took 0.016 s, line 75. 5o measured the reset on 2 items in 0 ms and 5p the paint on
   1. Here neither cost grew past a few milliseconds at a thousand times the items
3. LIVE. Every red item read ActiveColor (1,0,0) and every green (0,1,0), each at its own
   transparency, none at 0.85, and all 730 other geometry items of model 0 read 0.85, lines 80 to 82
4. THE RECORD. The view took 6.487 s through the COM view into a tree of 2847, line 83, the same
   order as P16's 6.1 s to 6.7 s, 5z-x, against 0.024 s for the whole sequence before it
5. WHAT THE VIEW HOLDS. 3153 MaterialOverrides, one per item, walked in 0.033 s: 2335 with colour
   (1,0,0), 88 with (0,1,0), both with no transparency, and 730 at 0.85 with NO COLOUR, Color reading
   null. Hidden reads 3, the three model roots, line 84. Every red and green item is named by the view
   with its colour, so none fell to 5p's case of an item already the colour it is given, lines 85 and 86
6. THROUGH THE SAVE AND THE REOPEN. After SaveFile, Document.Clear and TryOpenFile the document read
   the same 4 models, 61 sets, 528 tests and 2939 results, and 2848 viewpoints, the one the probe
   recorded more, line 92. The view read the same 3153 overrides, 730 with no colour and Hidden 3,
   line 93, and every red and green item right and solid, lines 94 and 95. Pressed, every item read
   live right and solid and the other 730 at 0.85, lines 96 to 100
7. THE BYTES. The same copy saved by the same session with the empty folder is 41,317,276 bytes, and
   with the one painted view 41,329,885 bytes, 12,609 more, lines 70 and 89. That is one view of
   3153 overrides and 3 hidden roots, read once

**A NULL COLOUR ON A DIM ENTRY.** A MaterialOverride written by the dim alone reads Color null in this
API, 730 of 730 here, before the save and after the reopen. Run 1's read off the view threw
NullReferenceException with Color read as never null, and run 2, whose only change was to allow it and
count it, read 730 such entries and 0 throws. That is consistent with the null Color being run 1's
throw, and run 1 printed no stack, so it is not proved. The design's read back walks MaterialOverrides
"into a lookup for Item and Color", part 2. Such a walk meets these entries on every view that dims.
The add-in today reads Color only on the entry that names a clashing item, which carries a colour here.
This is information, and nothing was changed outside tools\probes.

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD, IN BOTH RUNS.** The NWF the copy was made from read
sha256 0944C100 at the start and the end, lines 22 and 208. The guard wrote the Auto-Save switch
"3 0" and read it back, line 28. The watchdog saw no other Navisworks, and the put back ran. 38
registry values were put back, enable among them, line 162, and read again with 0 still differing,
line 197. SessionCleanCloseCount went 93 to 94, line 159, a clean close. InfoCenter.log and
LastSession.xml were put back reading their backups' sha256, lines 201 and 202. The guard saw 0
AutoSave files added, changed or gone, line 204, and the tool's own logs folder had nothing added or
changed, line 205. Run 1's result reads the same on each of these. The prober read the switch and listed
the AutoSave folder by name, size, write time and sha256 with `read-autosave-state.ps1`, kept in
%LOCALAPPDATA%\NwcFederatorLoop\turn5: for run 1 `probe-p17-run1-20261007-autosave-before.txt`,
`-during.txt` and `-after.txt`, enable String "0" at 16:20:07, "3 0" at 16:22:23 and "0" at 16:23:26,
and for run 2 `probe-p17-20261007-autosave-before.txt`, `-during.txt` and `-after.txt`, "0" at
16:25:28, "3 0" at 16:27:35 and "0" at 16:28:31. The folder held the same 199 files with the same
names, sizes, times and sha256 at all six reads, 0 lines differing between the first and the last.

THE PROGRAMS. Run 1: Navisworks pid 42692, started by the probe at 16:20:33, quit by Dispose and gone
8.8 s after, not forced. Run 2: Navisworks pid 43860, started by the probe at 16:25:50, quit by Dispose
and gone 7.7 s after, not forced, line 109. In run 2 AdskLicensingAgent pids 41992 and 37176, children
of 43860, and AdskLicensingInstHelper pids 52092 and 35056 under GenuineService.exe were seen, lines
120 to 134. 37176, 52092 and 35056 read exited, and 41992's start time could not be read when it was
seen, line 149. Get-Process -Id 41992 read no process after the run. No Roamer that was not there in
step 2 ran at the end of either run, line 152.

**STILL UNKNOWN.**

- the picture on Bader's screen. No hand step was asked by the row, and nothing here looked at the
  window. The saved copy with the view is
  `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-paint-20261007-162544\p17-painted.nwf`, 41,329,885
  bytes, sha256 02F815C814EF66CF698CF963CA53F781849D631EFD515135609EC30AEA8C019B, line 105, the view
  `P17 painted` in the folder `P17 probe`
- a test whose items span more than one model, or a third team's model. Every item here lived in the
  AR model, so one root was dimmed and three hidden
- an item that is already red or green, 5p's case. None here was
- an item without geometry, or a clash item that is a composite above its geometry. None here was
- how the bytes grow with the number of views and with a view that shows more models. One view was
  read. So whether the NWF shrinks with 59 to 109 such views in place of 2813, part 8 item 16, is
  still UNKNOWN. A view here cost 12,609 bytes with 3153 overrides
- why the COM record takes over 6 s in this tree. P18 measures the per view sequence
- whether run 1's NullReferenceException was the null Color. Run 1 printed no stack

**WHAT THIS DECIDES.** By the row of P17, YES: one call per colour per view. One ResetTemporaryMaterials
over every clashing item and one OverrideTemporaryColor over the red and one over the green, on 2423
items, each returned in at most 5 ms, and the view recorded every item with the colour and the
solidity it was given, through a save, a reopen and a press. No chunk size setting is built.

## 5z-z. HOW MANY SECONDS THE WHOLE PER VIEW SEQUENCE TAKES, IN THE TREE OF 2847 AND AFTER THE 2813 ARE REMOVED, MEASURED 2026-10-07

P18 of Q114, the views by team design, part 3, with P19 riding in it. The question: how many seconds
does one per-test view take, the whole sequence of hide, dim, paint, frame, record, mark and read back,
ten views into the copy holding 2847 viewpoints and the same ten into that copy after the 2813 per-clash
views of F85 are removed? By the row of P18, part 6 of the design is rewritten with this rate before
any add-in code, and it decides B8's cost. P19: does a recorded view's Hidden collection read back as
the hidden model roots, each giving its Model.FileName? Yes: check 3 reads the shown models off the NWF.
No: check 3 reads the plan's list and the block says so. Neither row hangs on another probe's answer.
P18 uses the routes P9 (the mark by AddComment), P14 (a folder in one RemoveAt), P16 (ZoomBox) and P17
(one reset and one paint per colour) found, and each of those said YES.

HOW. `tools\probes\ViewpointProbe\probe-view-rate.ps1` is P17's `probe-view-paint.ps1` with the new mode
`vprate` of `ViewpointProbe.dll`, the method MeasureViewRate, its own header, the work folder prefix
`view-rate` and the save name `p18-rate.nwf`. Nothing else in the script changed, the guard and F138's
SwitchAutoSaveOff included. tools\loop\nw-guard.ps1 read sha256 E29D2733, line 4. The branch was pulled
first, up to date at 64c9872. Get-Process Roamer read 0 before the run and 0 after it. The row names
P13's copy, which was a fresh copy of the baseline, so the probe copied the same baseline,
`runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`, 41,317,271 bytes, sha256 0944C100, into the
new folder `probes\view-rate-20261007-170120`, line 22, and the mode, on that copy:

1. reads every test and takes the 10 of the most open clashes, open being New or Active, the design's
   ViewStatuses, ties by name Ordinal. No test is named in the code
2. WALK ONE, once per test and in no view's seconds: the open results' first and second items and
   centres, the first open clash's camera from TestsViewpointForResult, copied, the paint plan as P17's
   (red every distinct first item, green every distinct second item not already red), and each item's
   home model. The homes are shown and dimmed and the other models hidden. That is a simplification of
   ShownModels, which needs the team map
3. ROUND A, into the tree as it opened. A folder `P18 probe A` at the root, made and marked once a round.
   Then per view, each part timed by its own Stopwatch and the whole by another:
   - undim: ResetTemporaryMaterials on the roots the view before dimmed
   - hide: ResetAllHidden and SetHidden on the hidden roots, skipped when the shown list equals the
     view before's
   - dim: OverrideTemporaryTransparency 0.85 on the shown roots
   - resolve: ResolveIndexPath per item into three ModelItemCollections
   - paint: one ResetTemporaryMaterials over every item, one OverrideTemporaryColor red, one green
   - frame: a copy of the clash camera, and ZoomBox on the box of the open centres padded by 500 mm
     where there are two or more, as 5z-x
   - folder: a subfolder `view NN` made by FolderItem and AddCopy under a fresh resolve, and marked
   - the record in three: make the COM view with ApplyHideAttribs and ApplyMaterialAttribs, find the
     COM folder, and SavedViews().Add
   - the mark: AddComment of the design's sentence and a marker line, made by
     CreateCommentWithUniqueId, on the view re-found by its names
   - the read back: the position, the comments and the mark's body, Hidden with each hidden item's
     HasModel and Model.FileName, and the MaterialOverrides walked once into a lookup by item, counting
     the right red, the right green, a wrong colour, and any coloured item that is not this view's
4. THE REMOVAL. Every top level folder whose every viewpoint reads as F85's by the probe's own reading
   of LegacyClashView, design 1.9, goes by one RemoveAt(root, index), from the end, its name re-read just
   before each call. The reading: a top level folder named X vs Y, a viewpoint at depth 1 to 3 under it,
   named T, two spaces, Clash and digits, T a test name of the document, and no comment. Redlines were
   not read
5. ROUND B, the same 10 views the same way, into `P18 probe B`
6. ResetTemporaryMaterials on every root and ResetAllHidden, SaveFile into `p18-rate.nwf` in the work
   folder, Document.Clear, TryOpenFile of the saved file, and all 20 views re-found by their names and
   read back again

Run from Windows PowerShell 5.1 as

    powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-rate.ps1 -Out %LOCALAPPDATA%\NwcFederatorLoop\turn5\probe-p18-result.txt -Nwf %LOCALAPPDATA%\NwcFederatorLoop\runs\04\NMFed\NWF\C02\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf -MarginMm 500

with the probe built by `dotnet build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release`,
0 warnings and 0 errors, DLL sha256 E1ACFEF5, script sha256 4CD4C4F2, lines 24 and 3. One run at 17:01,
kept as `p18-view-rate-result-20261007.txt`, the machine name on line 1 masked as `[machine]` and the
account folder on line 5 as `%USERPROFILE%`, nothing else changed. Navisworks pid 49396, adopted on all
four conditions, line 38. TryOpenFile of the copy returned True after 5.599 s with 4 models, every one
read from under the loop folder, lines 54 to 59. ExecuteAddInPlugin returned 0 after 82.78 s, line 48.
Dispose returned and pid 49396 was gone 6.4 s later, not forced, line 229.

**THE ANSWER: THE WHOLE SEQUENCE TOOK 6.447 S A VIEW IN THE TREE OF 2847, AND 0.112 S A VIEW AFTER THE
2813 WERE REMOVED, MEAN OF TEN. 99 PER CENT OF THE FIRST IS THE COM ADD.** Lines 100 to 115 and 176 to
191 of the result:

```
seconds a view, mean of 10 (median)    round A, tree 2847 to 2856    round B, tree 44 to 53
undim                                  0.003                         0.003
hide                                   0.020 (0.014)                 0.015 (0.013)
dim                                    0.003                         0.002
resolve                                0.002                         0.001
paint                                  0.001                         0.001
frame                                  0.000                         0.000
folder, made and marked                0.001                         0.000
record, make the COM view              0.017 (0.015)                 0.019 (0.003)
record, find the COM folder            0.000                         0.000
record, SavedViews().Add               6.385 (6.210)                 0.057 (0.057)
mark                                   0.000                         0.000
read back                              0.014 (0.008)                 0.013 (0.010)
THE WHOLE SEQUENCE                     6.447 (6.261)                 0.112 (0.099)
  least and most                       5.942 and 7.627               0.062 and 0.236
seconds no part holds                  0.000                         0.000
P18 MEASURED
P19 YES   live 20 of 20, after a save, a clear and a reopen 20 of 20
```

1. THE TEN TESTS. 528 tests, 59 with an open clash, line 62. The ten run from
   BLD-AR-Curtain Mullions-vs-BLD-AR-Windows, 2568 open, to BLD-EL-Conduits & Conduit
   Fittings-vs-BLD-EL-Electrical Equipment, 14 open, lines 65 to 74. Every item resolved with
   geometry and no side read null. Each view showed one or two of the 4 models. Walk one took 0.003 to
   0.054 s a test with its camera
2. THE COST IS THE ADD, AND IT FOLLOWS THE TREE. In round A, SavedViews().Add took 5.907 to 7.458 s a
   view, 6.385 of the 6.447 s mean, lines 79 to 98 and 110. Making the COM view took 0.017 s and finding
   its folder under 0.001 s. In round B the Add of the same 10 views took 0.039 to 0.077 s, about 110
   times less, lines 155 to 174 and 186. Every other part read the same in both rounds to a few
   milliseconds. The Add did not follow the view's overrides: the view of 71 took 6.736 s in round A
   and 0.040 s in round B, the view of 4626 took 6.024 s and 0.077 s, lines 83, 93, 159 and 169. Nor
   the clash count: the 2568 clash view took 7.458 s in round A, the first of the round, and 0.060 s in
   round B. Round A's record of the 2568 clash view sits with P16's 6.1 to 6.7 s and P17's 6.487 s in
   the same tree, 5z-x and 5z-y. Why the Add costs so much in a tree of 2847 is UNKNOWN
3. THE REMOVAL TOOK 1.253 S, AND THE NEXT EDIT 4.856 S. The judge read the 17 top level folders in
   0.004 s, lines 118 to 135: the 4 the NWCs brought, 34 viewpoints, kept, the 12 from AR vs AR to EL vs
   UNKNOWN, 2813 viewpoints, every one legacy, and `P18 probe A` with its 10, kept. No legacy viewpoint
   sat in a kept folder. The 12 RemoveAt calls from the end, each name re-read and held, took 1.253 s
   in all, AR vs AR 0.209 s against P14's 0.174 s, and DR vs DR, 2 viewpoints, 0.650 s, lines 136 to
   148. The tree then read 44 viewpoints, the count expected, and the models, sets, tests and results
   the same, lines 149 and 150. Then the first edit after the removal, making `P18 probe B` at the root
   by AddCopy and marking it, took 4.856 s, line 153, where the same two calls took 0.015 s in round A,
   line 77. Which of the two took it, and why, is UNKNOWN. The two were not timed apart. Round B's wall
   time of 5.984 s, line 175, holds those 4.856 s
4. EVERY VIEW READ BACK RIGHT, LIVE AND AFTER THE REOPEN. All 20 views read the position off by 0.000,
   one comment whose body is the mark written, Ordinal, every red and every green item named with its
   colour, no wrong colour, and no coloured item that is not the view's, lines 80 to 174. After SaveFile,
   Document.Clear and TryOpenFile of the saved file, the same 20 read the same, lines 200 to 219 and 221
5. P19 YES. Every view's Hidden read exactly its hidden model roots, 2 or 3 items, each HasModel true
   and its Model.FileName equal to the plan's hidden model's, Ordinal, none that is not a model root,
   live 20 of 20 and after the reopen 20 of 20, line 223
6. THE UNDIM ON THE ROOTS CLEARED THE VIEW BEFORE'S COLOURS, READ OFF THE COUNTS. The paint resets only
   this view's items, so a colour of the view before would stay unless the undim on the roots clears
   it. No view read a coloured item that was not its own. The plainest case is A 09 after A 08, both
   showing models 0 and 2, so the hide was skipped: A 08 painted 9 red and 12 green, and A 09 read 0
   coloured items of another view, lines 93 to 96. The probe did not check that the two views' items
   are disjoint, so this is read off the count and not item by item
7. THE OVERRIDES FOLLOW THE SHOWN MODELS, NOT THE CLASHES. Each view held its red, its green and one dim
   entry for every other item of its shown models: 3153 for model 0 alone (A 01 2335, 88 and 730, A 04 24,
   3 and 3126), 1138 for models 1 and 3, 71 for model 3, 4626 for models 0 and 2, and 1067 for model 1.
   Every dim entry read Color null, as in 5z-y. Part 6 read this off 5o and 5p, and here it is measured
   on 10 views
8. THE FILE. SaveFile took 1.878 s and wrote 724,283 bytes with 54 viewpoints, the 34 the NWCs brought
   and the 20 probe views, line 194, against the 41,317,271 bytes of the copy opened with 2847. The reopen
   took 2.706 s against 5.599 s for the first open, lines 196 and 54. The saved copy is
   `%LOCALAPPDATA%\NwcFederatorLoop\probes\view-rate-20261007-170120\p18-rate.nwf`, sha256
   C6784F82BF120614B9F4E72ECDB1ABBEE51C59140CB759C35F77F7CFF6543B74, line 225

**WHAT THE TWO MEANS GIVE FOR PART 6 AND B8, ARITHMETIC ON THE MEASURED MEANS AND THE DESIGN'S V OF 59 TO
109, NOT A MEASUREMENT OF A RUN.**

- On an NWF that holds no per-clash views, the sequence is 0.112 s a view: 6.6 s for 59 views, 12.2 s for
  109. The lowest recording rate of part 6 alone is 0.565 s. Walk one, the inventory, the removals and
  the fresh walk are outside this sequence and are not this probe's
- B8 A, the first run on an NWF holding the 2813, writes into the tree of 2847: 6.447 s a view, 380.4 s
  for 59 views and 702.7 s for 109, then the removal, 1.253 s here and 4.856 s on the next edit
- B8 B, the old removed first: the same removal, then 0.112 s a view, 6.6 s to 12.2 s
- So B8 A costs about 6.335 s a view more, once per NWF holding the per-clash views: 373.8 s at V 59 and
  690.5 s at V 109 on 1A02MM
- The rate between a tree of 53 and one of 2847 was not measured. A fresh NWF of the design holds the
  34 the NWCs bring, up to 109 views and their folders, near 150 items

**BADER'S THINGS. PUT BACK, AND AUTO-SAVE OFF HELD.** The NWF the copy was made from read sha256 0944C100
at the start and at the end, lines 22 and 323. The guard wrote the Auto-Save switch "3 0" and read it
back, line 28. The watchdog saw no other Navisworks, and the put back ran, line 272. 38 registry values
were put back, enable among them, line 277, and read again with 0 still differing, line 312.
SessionCleanCloseCount went 93 to 94 and no crashCount changed, line 274, a clean close. InfoCenter.log and
LastSession.xml were put back reading their backups' sha256, lines 316 and 317. The guard saw 0 AutoSave
files added, changed or gone, line 319, and the tool's own logs folder had nothing added or changed, line
320. The prober read the switch and listed the AutoSave folder by name, size, write time and sha256 with
`read-autosave-state.ps1`, kept in %LOCALAPPDATA%\NwcFederatorLoop\turn5 as
`probe-p18-20261007-autosave-before.txt`, `-during.txt` and `-after.txt`: enable read String "0" at
17:00:55 before the start, "3 0" at 17:03:17 while the probe ran, and "0" at 17:05:01 after the put back,
and the folder held the same 199 files with the same names, sizes, times and sha256 before and after, 0
lines differing.

THE PROGRAMS. One Navisworks, pid 49396, started by the probe at 17:01:28, quit by Dispose and gone 6.4 s
after, not forced, lines 38, 229 and 262. AdskLicensingAgent pid 47876, a child of 49396, read STILL
RUNNING at the end of the probe, line 263, and Get-Process -Id 47876 read no process when the prober read
it after the run. AdskLicensingAgent 51556 and AdskLicensingInstHelper 42536 and 35428 read exited, lines
264 to 266. No Roamer that was not there in step 2 ran at the end, line 267.

**STILL UNKNOWN.**

- the rate in a tree between 53 and 2847 viewpoints, so on a fresh NWF of the design near 150 items, and
  in C06's trees, which hold 4800 per-clash views over 16 groups
- why SavedViews().Add takes about 6 s in a tree of 2847 and about 0.06 s in a tree of 44
- why the first edit after the removal took 4.856 s, and whether it was the AddCopy or the AddComment
- views that show the models of ShownModels with the team map. Here the homes alone were shown. Within
  71 to 4626 overrides the Add did not follow the count, and nothing wider was tried
- the design's folder tree of priority, pair and size. Each view here sat in its own folder two deep
- the inventory's comment reads, the fresh walk and VIEWS TREE. They are outside the per view sequence
  and were not timed
- what the add-in adds around the calls: the window, the progress lines and the log
- whether A 08's and A 09's items are disjoint, so whether item 6 holds item by item
- whether the rate holds with the Saved Viewpoints window open
- the views on Bader's screen. The row asked no hand step

**WHAT THIS DECIDES.** By the row of P18, part 6 is rewritten with these rates before any add-in code:
0.112 s a view for the whole sequence in a tree of 44 to 53, and 6.447 s a view in the tree of 2847,
nearly all of it in the COM view's SavedViews().Add. By B8, writing first and removing after costs about
6.3 s a view more, once per NWF holding the per-clash views. By the row of P19, YES: check 3 reads the
shown models off the NWF, from each hidden root's Model.FileName.
