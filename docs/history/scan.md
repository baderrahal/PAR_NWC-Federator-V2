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

Those are about the SETS tree. They say nothing about whether a MODEL can be taken out of
an open document, which is section 5a and is still UNKNOWN.

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

THE ANSWER, read by `tools\probes\probe-document-ready.ps1` on DESKTOP-5VL7LTJ on
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

THE ANSWER, read by `tools\probes\probe-properties.ps1` on DESKTOP-5VL7LTJ on 2026-09-19
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

WHAT REFLECTION CAN SAY, read by `tools\probes\probe-properties.ps1` on DESKTOP-5VL7LTJ
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

THE ANSWER, read by `tools\probes\probe-clash-comments.ps1` on DESKTOP-5VL7LTJ on
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
`ExecuteAddInPlugin("ViewpointProbe.PARS", ...)`, on DESKTOP-5VL7LTJ against a COPY of
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
