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
