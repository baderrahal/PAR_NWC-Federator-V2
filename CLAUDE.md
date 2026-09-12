# Parsons NWC Federator

Navisworks Manage 2025 add-in. Reads a folder of discipline NWC files, gathers them
into federations, saves an NWF and an NWD per federation, imports a clash test XML,
runs the tests, and writes one Excel report per federation. Per building is the
default way of gathering them and there are three others.

## Host and target

- Navisworks Manage 2025 only. API series Nw22, bundle folder Contents\v22
- .NET Framework 4.8. Navisworks has been on 4.8 since 2021 and 2025 did not move
- References are the DLLs in the Navisworks install folder, copy local false:
  Autodesk.Navisworks.Api.dll and Autodesk.Navisworks.Clash.dll
- Installs to %APPDATA%\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle
- 27 people run it. No compiler on their machines, no admin step, no path that
  exists on one machine only

## Done means all three

1. One building federates, clashes and writes Excel with no errors
2. Every ticked building runs unattended in under 45 minutes
3. Clash counts in the Excel match the Clash Detective panel exactly

## File naming

Input:  1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc
Split on the hyphen. Part 3 is the building, part 5 is the discipline.
How the files are gathered is a CHOICE, made in the Grouping step, and per building
is only the DEFAULT. This used to be written here as a rule and it is not one. The
four choices are one file per building, one per building and discipline, one per
discipline across every building, and one file for everything. Per building puts
every discipline of a building into one federation, and under it discipline is read
for reporting only. Under the other three it decides the split as well.
Group on the full 6 character building code. 1C07BC and 1C07K1 are two buildings.

The output name is project, originator, building, level, discipline, type, number,
and it is a PATTERN with defaults rather than a fixed string. This used to be written
here as fixed and it is not. The first three come from the file names. The other four
are supplied, because the files in one group can disagree on all of them and outputs
overwrite, so copying from any one input would mean picking a winner.

The names are shown as a TABLE with one row per group, filled in by the scan, and every
cell in it can be typed over. A pattern is where a name starts and not where it ends,
because one group in twenty usually needs a name the pattern cannot give.

A name typed over is held per CELL, not per row. Changing a pattern refills only the
cells nobody has touched, so editing an NWF name does not freeze the NWD name beside it,
and the refill says how many rows it kept. A cell can be given back to the pattern.
Setting a cell to exactly what the pattern already gives still counts as typed over,
because the person meant that value.

The collision check runs on the table, not on the patterns, so two names typed to the
same thing is caught the same way two patterns colliding is. It names both groups and
the run does not start.

Every supplied field is shown in the Outputs step and can be changed there. The
defaults are ZZZ for the level, which is the ISO 19650 code for all levels, BM for
the discipline, which is a federated building model, MOD for the type, which is a
model, and 000001 for the number, which never advances because the file is
overwritten in place. A fifth, ZZZZZZ, is the building field for a group that covers
several buildings, following the same all convention.

The building field carries the group's own building code where the group is one
building, and the discipline field carries the group's own discipline where the group
is one discipline. The supplied value is only used where the group spans several.

There is one pattern each for the NWF, the NWD and the workbook, because a project
may want them to differ. They start identical. A set of patterns that would write two
groups to the same name is reported and the run does not start, because outputs
overwrite with no date suffix and the second would silently destroy the first.

The output name is built, not patched. That means a five part input still gives a
full seven field output name, and the readable-name floor is five parts. A six part
name reads fine.

If two files in one group disagree on the project code or the originator, report
it and skip the group. Do not pick one.

The split character, the part positions, the grouping choice and every supplied name
field are settings, never constants.

## Two workflows

Confirmed by Bader on 2026-09-07. The window shows which one each group will take,
the confirm dialog counts them, and the log carries them. The rule is
Federator.Core.Rerun.RunPath and it changes what the person is told, never what the
engine does.

First run. The NWC folder is scanned and grouped, output folders picked, a clash XML
picked, Run pressed. Per group the tool builds the NWF, builds the sets, adds and runs
the tests from the XML, writes Excel, HTML, images and the NWD.

Weekly run. The same NWC folder, Run pressed with no XML. Per group the tool finds the
NWF already there, opens it (OPENED), lets Navisworks reload the newer NWCs, runs the
tests saved inside the NWF, writes Excel, HTML, images and the NWD. An XML is optional
and only adds or updates tests.

The labels the window shows, one per group in the Run as column and counted in the
confirm dialog:

    First run                  no NWF at the output path yet
    Weekly run                 the NWF is there and no XML is picked
    Weekly run plus XML        the NWF is there and an XML is picked
    Rebuilt                    the NWF is there and points at a different file list,
                               so it is cleared and rebuilt from the scan folder with
                               its saved tests kept, then run as a Weekly run. Only
                               known once the NWF is opened, so the window opens each
                               NWF at Run where nothing open would be lost, and
                               otherwise the run settles it and the list shows it after
    Skipped (changed on disk)  a group whose file list differed and that was left
                               alone. Since F24 no group ends this way
    Unknown                    the NWF folder is not picked yet, or a case the rule
                               cannot name

Run the open file is always a Weekly run, or a Weekly run plus XML. There is no First
run on that path, because the NWF already exists and is the document.

## What the clash test XML holds

Measured from the real files on 2026-08-27 and corrected on 2026-08-28. Do not
re-derive this. An earlier version of this section said the reference file held no
sets. It holds 61. The numbers below are the measured ones.

1104-PAR_CLASH_AllInOne holds both parts, 61 sets and 1830 tests in one file, and
all 61 test locators resolve against its own sets. It is the reference file. When
this file and another disagree about anything, this one is right.

The tests:

- root exchange, units="ft", one batchtest, 1830 clashtest children
- every test: test_type="hard_conservative", status="new",
  tolerance="0.2460629921", merge_composites="1"
- that tolerance is 75 mm. Tolerance is in the file units, convert to document
  units before use
- each side is one clashselection holding one locator, a name path such as
  lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals
- 1830 is every pair of 61 with no self pairs
- linkage is none and rules are empty in every test here. Read both anyway,
  another project will use them

The sets:

- 61 sets, carrying real rules, not one rule repeated
- 102 conditions across the 61 sets. 30 sets carry one condition, 26 carry two,
  5 carry four
- paths nest. Mechanical has 4 subfolders. Walk the folders, never assume the
  sets tree is flat
- set names contain spaces and ampersands, and two of them end in a space. Match
  the exact string. Never trim a set name or a locator
- the rule vocabulary seen in real files:

      category LcRevitData_Element display Element
        property LcRevitPropertyElementCategory display Category, the Revit category
        property lcldrevit_parameter_-1002053 display Workset

      no category element at all
        property LcOaNodeSourceFile display Source File

- condition test values seen: equals and contains
- a condition can arrive with no category element. The reader must not assume one
- rebuilding a search through the API uses the internal strings, never the display
  words. Element and Category and Workset and Source File are what a person reads,
  LcRevitData_Element and the rest are what the API matches on

Search_Set_Building.xml and Search_Set_Infra.xml are damaged exports. Every
condition in both reads category Category, property Name, equals Floors. They are
kept as samples only, to prove HealthCheck catches them. Never treat either as a
reference for what a good file looks like.

Counting distinct rules. Two different numbers are both right about the reference
file and they answer different questions:

- 53 is the number of distinct conditions, comparing test, category, property and
  value, with flags left out. This is what HealthCheck.DistinctRuleCount counts,
  because it is the one that catches a damaged export: Search_Set_Infra has 2715
  conditions and 1 distinct condition
- 59 is the number of distinct sets, comparing each set's whole ordered list of
  conditions. It is 59 rather than 61 because two pairs of sets carry identical
  rule lists: Telecom Fixtures with Telephone Devices, and Electrical Fixtures
  with Devices

The set level number cannot be used for the damaged export check. On Infra it
gives 6, because the sets differ in how many copies of the one rule they hold,
and 6 does not read as broken.

## Rules the code holds

- A locator that does not resolve to a set: report the test by name, skip it.
  Never import a test with an empty side. It returns zero clashes and reads as passed
- A test where either side resolves to zero items in this model counts as skipped,
  not passed. Skipped and passed are counted as different numbers in the report
- Each building writes its NWF, NWD and Excel before the next building starts.
  A failure part way through keeps everything already written
- Never clear and rebuild an NWF that already exists. This is the one rule most
  likely to be helpfully undone by someone who does not know why it is here.
  An NWF holds pointers to the NWC files, not copies, so a model updated in place
  needs no rebuild. The clash results live inside that NWF and are the only record
  of what has been fixed. Rebuilding it resets every clash to New and loses every
  Active and Resolved. This tool runs weekly, so that is a week of review thrown
  away each time. Three cases per group:
    no NWF at the output path, build it, there is no history to lose
    NWF there and its file list matches the group, open it, do not clear, do not
      re-append, log OPENED
    NWF there and the file list differs, log CHANGED with the counts, then REBUILT
      naming every file added, moved and removed, read the saved tests, clear the
      document, append the scan in scan order, put the saved sets and tests back
      where the clear dropped them, and only then save the NWF over. Bader decided
      this on 2026-09-07, Q22, so the rule above holds for a matching NWF and a
      differing one is rebuilt rather than left alone
  The file list is read out of the opened NWF. No side file records what went in,
  because a side file can disagree with the NWF and the NWF is the record
- The file list is read from Model.FileName, never from Model.SourceFileName.
  FileName is the NWC, which is what the scan holds. SourceFileName is the
  container the NWC was published from, which on this project is a Revit file in
  Autodesk Docs such as
    Autodesk Docs://KSA_New Murabba/1104-PAR-100000-ZZZ-AR-MOD-003000.rvt
  That can never equal a scanned NWC path, so comparing on it reported CHANGED for
  22 of 22 groups on a run where nothing had changed, and because a CHANGED group is
  left alone entirely, no NWF was reused, no set was built and no test ran. Both
  names still go in the log whenever they disagree, which is what made this
  findable. The rule lives in Federator.Core.Rerun.ModelFileNames so it can be
  tested without Navisworks, which is why it went unnoticed in the first place
- The Revit container inside an NWC is often a different building from the NWC.
  Where the building code parsed from the NWC name differs from the code in the
  Revit source name, report SOURCE MISMATCH naming both, and where one Revit
  building code feeds more than one group, report SHARED SOURCE, because that means
  one building has been split in two by a naming error. Both codes are read with the
  same parser used on the NWC names, never a separate rule. This is information. It
  does not block, unpick or merge anything. Bader decides
- When tests skip for the same reason, log the count and at most five examples, then
  the total. Per test detail stays for tests that were created or run. One run wrote
  1830 near identical SKIPPED lines and a 1 MB log, which buries everything worth
  reading
- If no test can resolve a set, say so and stop before creating anything. One line
  naming how many sets the document holds and how many the tests name. A run against
  a document holding zero sets once went ahead anyway and finished with 0 created and
  0 run. This is a guard, visible in the log and in the window, never a silent skip
- The Run button does the whole job for every ticked group, model side and clash
  together. With no XML picked, the tests saved in each group's NWF are run where they
  sit, and a group whose NWF holds none runs nothing and the log says so. The clash step
  does one of three things and the log names which, in the same words on the scanned run
  and on the open file run: tests from XML, tests saved in the document, or nothing. The
  rule lives in Federator.Core.Clash.ClashWork.SourceFor. Sets come from the XML and from
  nowhere else, so with no XML the sets in the document are left alone. The two buttons on
  the Clash step are for trying one open model by hand and are labelled as that. They
  are not steps in the run. Splitting one job across three presses is what let a user
  run clash against a document with no sets in it
- Republishing the NWD happens in the Build and Open cases, and it is no longer a tick box. It was
  one, on by default, and a weekly run wanted it every time, so it is fixed on. That is the
  point of a rerun. The NWF pointers are unchanged, so reopening picks up whatever the NWC
  files now hold and the NWD is refreshed without the clash history being touched
- The clash test file is picked at run time, every run, and can be from any project.
  Nothing about any one file is written into the code, not names, not counts, not
  property internal names. Those appear in tests as sample data only. A file may hold
  sets, tests, or both, and all three are normal. There is one picker, because Bader's
  file holds both and two boxes meant picking the same file twice
- Per group the order is append, save the NWF, build the sets, create the tests, run
  them, save the NWF again, publish the NWD last. The sets, the tests and the results
  all live in the NWF, so an NWD published before the clash work ships without any of
  them. The NWD used to go first and that is what this ordering fixes. The second
  save only happens when the clash step actually put something into the document
- Nothing is created twice on a rerun. A clash test already in the document under the
  same name is left exactly as it is, because that is where its Active and Resolved
  clashes live, and a set already at its path is left alone too, because a second copy
  would leave two sets at one path and a locator resolving to whichever came first.
  Both are counted and reported as already there, separately from what was created
- A CHANGED group is REBUILT from the scan folder, keeping the tests saved inside it,
  and the rebuild comes BEFORE the units change and the clash step, so from there it is
  an opened group. It used to be left alone entirely, and on 2026-09-07 six groups had
  an NWF built from an older folder with fewer files, 1B06BC 4 of 5 with EL missing,
  1B06K1 1 of 4, so the NWDs went out missing models and nothing rebuilt the NWF. The
  plan lives in Federator.Core.Rerun.NwfRebuildPlan: a file on both sides under a
  different folder is a MOVE, a file only in the scan is an addition, a file only in
  the NWF is a removal, and the files to append are the scan in scan order. The log
  carries one shape, REBUILT with the three counts, one line per file, then one saved
  tests line with three numbers read off the document: how many were read before the
  clear, how many were there after the appends, how many after the copy was put back.
  The copy is DocumentClashTests.CreateCopy and CopyFrom for the tests and the same
  pair on DocumentSelectionSets for the sets, the sets first because a side points at
  a set. The sets are counted by walking the tree, before the clear, after the appends
  and after the copy is put back, and put back on their own count whether or not the
  tests dropped, on one SETS line with the three numbers. Whether Document.Clear keeps
  either is UNKNOWN until a run, so the numbers are read and never assumed. The NWF on disk is only saved over once every test read before
  the clear is back, and otherwise the group fails, says so, and the NWF keeps its file
  list and its tests. A rebuilt group ends as Rebuilt and is judged DONE when everything
  after the rebuild went right, in Federator.Core.Rerun.GroupJudgement. Q22
- A test already in the document is left as it is, which means a tolerance changed in the
  XML never reaches it. That is right and it was silent, so now it is REPORTED. Every
  test in both is compared on the tolerance, the test type, merge composites, and per
  side the locator and the self intersect and primitive type flags, and every difference
  is named by test name with both values. Comparing changes nothing on either side.
  Tolerances are compared within an epsilon because the file's number travels through a
  unit conversion, and an exact comparison would report drift on all 1830. Locators are
  compared Ordinal and never trimmed, because two set names in the reference file end in
  a space
- Applying the file's settings to a test already in the document is a tick box, off by
  default, and it says plainly that changing a test RESETS its results and every clash
  in it goes back to New. That is the whole reason the default is to report and not to
  act. What drifted is worth knowing every week. Overwriting it is worth doing once
- There is no stale marker on the clash API. Nothing named stale, altered, out of date,
  dirty or needs rerun exists on any type in Autodesk.Navisworks.Clash, public or
  private, measured on 2026-08-31, see docs\scan.md section 4j. The only thing there is
  ClashTest.Status, a four value enum of New, Old, Partial and Complete. What puts a test
  into Old is UNKNOWN and cannot be read off the DLL, so the status is logged as itself
  and no sentence is put on it. Never translate Old into "your models have changed". The
  Status setter is public and nothing here writes it, because writing it would be
  claiming to know the rule that is UNKNOWN
- Resolved clashes stay in the file forever and that is the point of them, so the count
  is reported per test and as a total for the group. A test running weekly for months
  accumulates them without limit and nothing else prunes them
- Compact is reachable, DocumentClashTests.TestsCompactAllTests is public, and it is a
  tick box off by default that runs after the tests and before the workbook is written.
  Never compact silently. It removes every Resolved clash from the NWF, the NWF is the
  only record of what has been fixed, and there is no second copy anywhere, so it cannot
  be undone. The count is announced before it runs and the count removed is reported
  after. The no argument form is used, because TestsCompactTest takes a ClashTest and is
  a mutator, so the handle passed to it dies the way section 4g describes
- Nothing removed and nothing asked to be removed are different, so the compacted count
  is minus one until a compact actually runs. Zero removed is a real answer
- Per test, everything comes from the file and never from a constant: the name, the
  test type, the tolerance, merge composites, and per side the self intersect and the
  primitive type flags
- Tolerance is read per test and converted from the file units attribute into the
  units of the open document before it is set. There is no global tolerance setting
  in this tool. Both numbers and both unit names go in the log, because which units
  ClashTest.Tolerance is measured in is UNKNOWN until a test runs against a real model
- A test type that does not map to a value on the enum is reported by name and
  skipped. It is never approximated to the nearest one, because a hard test standing
  in for a clearance test reports a number that reads as real and is not
- A clash side is pointed at the saved set through CreateSelectionSource, not filled
  with a copy of the set's items. That is what Clash Detective does when a person
  picks a set in the panel, and it is what keeps the counts matching the panel. A
  copy is a snapshot that can drift from the set the panel shows
- Everything that threw for one group is kept in a list, never in one slot. The model
  side can append cleanly and the clash step still throw, and one slot kept whichever
  wrote to it last and silently lost the other
- A handle onto anything the document owns is borrowed, never kept. Every SavedItem read
  out of a collection is created with eEXTERNAL ownership over a weak reference, so it
  dies the moment the document replaces the object behind it, and every mutator on
  DocumentClashTests is a copy form that does exactly that. TestsRunTest is one of them,
  so the test handed to it is dead when it returns and reading Children off it throws
  ObjectDisposedException (WeakRef). A test is addressed by where it sits, resolved again
  before every use, and disposed after. One run threw that exception once per test for
  8 hours 52 minutes and produced nothing
- Nothing walks a whole collection once per item. Looking a test up by name after every
  add was O(n squared) over 1830 tests and built about 1.7 million finalizable native
  handles per group. TestsAddCopy appends at the root, so the new test is at the index the
  count held before the add, checked by name rather than assumed
- What this tool creates or resolves, it disposes. All of ClashTest, ClashResult,
  ClashSelection, SelectionSet, SelectionSource, Selection, ModelItemCollection, Search
  and SavedItem are IDisposable. Disposing an eEXTERNAL wrapper releases the wrapper and
  never the document's object. SavedItemCollection is not disposable and is never disposed
- A run that is failing everything stops the run, not the group. After the first 50 tests,
  if every one has failed for the same reason, the whole run stops and says so in one
  line. Every group of that nine hour run failed the same way, so a per group stop would
  have saved none of it. The 50 is a setting. A test skipped because a side finds nothing
  is the ordinary answer and counts neither way
- An exception repeating with the same heading and the same trace is written out in full
  once and counted after that, and the RESULT block carries the total beside the one
  trace. One run wrote the same stack tens of thousands of times into a 17.8 MB log
- One picker, one file. The file can hold sets, tests, or both, and the tool reads what is
  in it. A file holding only tests still works against sets already in the model
- The naming boxes are filled from the patterns BEFORE anything can read them back.
  Filling the grouping combo sets SelectedIndex, which fires its handler, which regroups,
  which reads the boxes into the patterns. With the boxes still empty that replaced every
  default with nothing, and the window then opened with all fifteen naming boxes blank, so
  every field had to be typed by hand. That is where MOD-00001 came from instead of
  MOD-000001. The defaults were always six digits in the code and never reached the window
- Every picker returns a FOLDER to open at, including the one that chooses a file, and the
  caller never takes the parent of it. Doing that opened the clash XML picker one level
  above the folder it had remembered. The rule lives in
  Federator.Core.Diagnostics.PickerStart so all five can be proved rather than clicked
- Every picker remembers its own last folder, in a file beside the logs, and reopens
  there next time. Per picker and never one shared, because with one shared, picking an
  NWD folder moves the source picker to it and the next run reads the wrong folder. A
  remembered folder that has gone is not an error and is never cleared, the picker opens
  at the nearest parent still on disk and says so, which is what happens when a project
  drive is not mounted yet. Remembering never stops a run: an unwritable location is
  recorded as a reason and the session still remembers, it just does not survive a restart
- A step whose content is taller than the window scrolls. Only the Outputs step is,
  measured at 1024x680, 1280x800 and 1600x1000 with build\probe-window-scroll.ps1 and
  cut off at all three, which is why the naming table could not be reached. The other three
  fit at all three sizes and are left alone, because a ScrollViewer around a step whose grid
  is meant to fill stops it filling
- No code identifier and no framework message ever reaches a label. It said
  "Workbooks go in UNKNOWN" before a scan, with "Parameter name: nwfFolder" behind it,
  because an empty NWF folder threw and the message went straight to the window. Nothing was
  wrong at that point, the folder had not been picked. A failure DIALOG may carry the
  exception message, because that is what gets sent back. A label may not. The wording lives
  in Federator.Core.Report.ReportPaths.WhereTheyGo so it can be proved
- The scan and its checks live in the Source step, so pressing Scan reports what was
  found and what is wrong with it in one place. A plain count line first, files found,
  files readable, groups and the findings by kind, then the findings themselves
- Findings are written in the words a person would say, never in the words the tool
  thinks in. A shape is described by naming a real code that has it, not by printing
  9A99AA. A shared Revit source says two federations are being built from one Revit
  building, names both, and says that usually means one NWC file name is wrong. The
  short code stays as its own column so it can still be sorted on
- Copy findings puts them on the clipboard as tab separated rows with a header, so
  pasting into Excel gives a table. A run with nothing odd still copies a header and
  one row saying so, because an empty clipboard reads as a failed copy
- A group holding one NWC cannot clash with anything, whatever the test list says. Every
  test is still created, so the NWF is complete and matches the other groups and a later
  run against a fuller model finds them already there, and none of them is run. The
  reason is SingleModel and it is counted apart from EmptySide on purpose. A side finding
  nothing says a discipline was not exported. One model says the group was never going to
  clash and no export would change that. In the last real folder that was 1B06BS and
  1C06PK, and both ran 1830 tests for nothing
- The add-in resolves its own assemblies from its bundle folder by simple name,
  ignoring the version, through an AssemblyResolve handler registered in the static
  constructor of FederatorPlugin. This is not belt and braces, it is the only thing
  that works. Six references bind to a version one build number away from the file
  that ships, .NET Framework binds a strong named assembly by exact version, and the
  binding redirects NuGet writes go in an application config that a Navisworks add-in
  does not have. Every file was already present when a run failed on this, so adding
  files fixes nothing. The rule lives in Federator.Core.Diagnostics.BundleAssemblies
  so it can be tested, and it is proved by a console exe with its config deleted, see
  docs\scan.md section 4i
- install.ps1 copies the bundle CONTENTS, never the bundle folder. Copy-Item of a
  directory puts it inside the destination when the destination exists and creates it
  when it does not, so the same line does two different things depending on whether
  the remove before it has finished. That left ParsonsNwcFederator.bundle inside
  ParsonsNwcFederator.bundle, which Navisworks does not read at all, and it is checked
  for afterwards
- install.ps1 walks what is actually in the bundle after copying and refuses to finish
  if any reference is satisfied by neither the bundle, the framework, nor the
  Navisworks folder. A missing file is caught at install rather than ninety seconds
  into a run. The version mismatches are printed too, labelled as expected, so nobody
  reads them as faults
- The reports never go inside the folder being scanned, whether it was picked or
  defaulted to. A run put the workbooks in the NWC folder it was reading, and the
  Clash step picks an XML at run time, so a report written there is a file a later run
  can be handed as its own input. A picked folder inside the source is refused and the
  default beside the NWF folder is used instead, saying so. Where the NWF folder is
  itself inside the source there is nowhere safe, and that is said rather than written
  somewhere surprising
- Outputs overwrite, the NWD every run and the NWF only when it is being built for
  the first time. No version suffix, and no date suffix unless the weekly record tick
  box is on, which is off by default and applies to the NWD alone
- The NWF and the NWD are dated differently on purpose, and this is the reason. The
  NWF holds the clash tests and every clash result inside it, so it IS the history and
  overwriting it in place is what keeps that history. A dated NWF would fork the
  history: this week's clashes would go in one file, last week's Active and Resolved
  would be stranded in another, and no file would hold the whole picture. The NWD
  carries no clash results at all, it is the model as it stood, so a dated NWD is a
  weekly record that costs nothing and loses nothing. One is a ledger and one is a
  photograph
- The date goes in the number field, because the number never advanced anyway. It is
  a format string and a setting, DateFormat, defaulting to yyyyMMdd. It is not a
  fixed string and never a fourth name pattern
- DateTime.ToString does NOT throw on a format string nobody can read, it treats what
  it does not recognise as literal text, so "not a real format" comes back as
  "noA a real 0or0aA". Measured, see docs\scan.md section 4j. So what is checked is
  what the format PRODUCED, not whether it threw. A result that is empty or holds a
  character Windows refuses in a file name falls back to the number. Anything else is
  used as typed, because the format is the person's setting and their mistakes should
  be visible in the preview rather than silently corrected
- A group ends in one of three states, and the test is always what was ASKED FOR,
  never what happens to be on disk. A step deliberately switched off is not a
  failure. Judging a group by whether an NWD existed, with republishing switched
  off, once reported all 22 groups of a clean run as FAILED.
    DONE     everything requested for this group succeeded
    PARTIAL  something requested did not complete, or the group was CHANGED and
             left alone, which since F24 no group is
    FAILED   something requested threw or produced nothing, a rebuild that appended
             nothing or could not keep its saved tests included
  The rule lives in Federator.Core.Rerun.GroupJudgement, with no Navisworks types
  in it, so it can be tested. The outcome and the reason for it come out of one
  pass, so the two can never disagree
- The counts in the RESULT block and the errors under it come from one list. A
  failed count with an empty error list is what the log printed once, saying
  "groups failed: 22" and "Nothing failed." in the same block. A group recorded as
  failed always carries a reason, and one is substituted rather than thrown over
  when a caller forgets, because logging never stops a run
- A file written twice reports the size of the SECOND write. The list holds one entry per
  file so the count stays a count of files, and the SIZE is the last one read. The NWF is
  saved twice, once after the append and once after the clash step, and keeping the first
  size made a run report it at 4,141 bytes, an empty federation, when the second save had
  read 165,844 off the disk four minutes earlier. Nothing had shrunk. It read exactly like
  an NWF that had lost every clash result in it, which is the most alarming thing this log
  can say, so it must never say it by accident
- The NWF is looked at ONCE MORE after the NWD is published, because the NWD is published
  last and the NWF is the only record of what has been fixed. The line says intact and the
  size, or CHANGED with both sizes, or GONE. Nothing was checking this, so a run that did
  destroy the clash history would have finished quietly. It writes nothing and changes
  nothing, so it cannot itself break a group
- Only a file this run actually wrote goes in the files written list. Outputs
  overwrite with no date suffix, so last week's NWF and NWD sit at exactly the
  paths this run uses. A group that threw before writing anything must not list
  them as its own, and a file that was checked rather than written is logged with
  CheckOnDisk, which reports the size and records nothing
- The Try forms return a bool and it is read, never discarded. For the NWD that
  bool is the only thing separating a fresh publish from last week's file at the
  same path, because File.Exists cannot tell them apart
- The workbook is ONE SHEET, laid out exactly as the report the client receives: every
  test one after another, most clashes first. The Summary sheet, the Matrix sheet and the
  sheet per test are GONE. All three were asked for in an earlier session, before anyone
  had put a real Navisworks report beside ours. Once both were seen together the ask
  became one thing, our output matching theirs, and theirs has none of them. Do not add
  them back because they seem useful. If it is not in theirs it is not in ours
- The tests are ordered MOST CLASHES FIRST, and where two hold the same count they keep
  the order they were created in. Measured off both exports in samples\client-report on
  2026-09-01: both are strictly descending over all 1830 blocks, and every tie group is in
  the order the tests sit in the exchange file rather than alphabetical, including one
  group of 1807. So it is a STABLE sort, and List.Sort is not one
- The OUTPUT is sorted and the document is not. Whether their report is sorted by the
  report writer or simply walks a collection Clash Detective had already sorted is
  UNKNOWN and cannot be read off the files. Sorting the document would mean calling
  TestsSortTests, a mutator that reorders the tests inside the NWF, and the NWF is the
  record of what has been fixed
- A distance or a coordinate is THREE DECIMALS with the trailing zeros kept, except that a
  value which is not zero but would round to 0.000 is written to three significant figures
  instead, in plain decimal and never an exponent. Measured across all 387 coordinates and
  129 distances in the two exports: nine are of that second kind and 0.000 appears nowhere
  in either file. Whether theirs rounds or truncates is UNKNOWN, because both files carry
  only the formatted text, so ours rounds
- The Item ID label is Element ID, and this tool CHOOSES it rather than reading it. It used
  to be the display name of whichever property matched, and since Id is first in the search
  list that came out as "Id: 990299" against their "Element ID: 702888". Which property
  actually supplied the value is kept on the item and goes in the log, because renaming a
  value is only honest while what was renamed is still visible
- A report check compares three things and not one: WHICH columns, in what SHAPE, in what
  ORDER. The check before this compared presence alone and passed while the test order, the
  id label and both number formats all differed from the samples. It reports the FIRST
  divergence with what ours holds and what theirs holds, because one fault has forty
  consequences and the first is the one to act on
- One workbook per group, written after the clash step and before the NWD is published,
  so the three outputs of a group agree with each other. Named like the group's other
  outputs with an xlsx extension, and it overwrites, the same as the NWF and the NWD. The
  Excel folder is picked on the Outputs step, and when it is empty the workbooks go beside
  the NWF folder in a subfolder called Clash Reports, which is a setting
- Results are grouped, not one row per raw clash. A group is one row and its distance is
  the most severe clash in it, which is the minimum: a hard clash reports a negative
  overlap so the worst is the most negative, and a clearance test reports a gap so the
  worst is the smallest. Every row carries the raw count behind it, so the grouping hides
  nothing
- The clash API has no open against closed notion. Nothing on IClashResult, ClashResult,
  ClashResultGroup, ClashTest or DocumentClashTests names one, ClashResultStatus is a flat
  five value enum, and Navisworks' own report does not mention open or closed either. So
  every status is reported as itself, and where one number is needed for what is
  outstanding, WHICH statuses that number holds is a setting with two choices and never
  a constant, because neither answer is readable off the API and both are stated rules:
    Navisworks open  New, Active and Reviewed. The default, because it is the product's
                     own definition and the number then agrees with the panel
    New plus Active  the two only, for a project that treats Reviewed as dealt with
  Approved and Resolved are closed under both. The choice is written to the log, so
  nobody reads an API meaning into a number that has none. The word open never appears
  unqualified. Since the Summary and Matrix sheets went, no output shows that number,
  and the setting is kept because the window and the report options still carry it
- The test sheet carries the CLIENT'S columns, in their order and in their words, because
  they have already accepted a report in that shape. Per test:
    Tolerance, Clashes, New, Active, Reviewed, Approved, Resolved, Type, Status
  Per clash:
    Image, Clash Name, Status, Distance, Grid Location, Description, Clash Point,
    then Item 1 and Item 2, each Item ID, Item Name, Item Type
  Four of these are joins and not columns, which is the part a description gets wrong and
  the stylesheet settles. Grid Location is ONE field, "B-1 : ROF", grid then a spaced colon
  then the level. Clash Point is ONE field, "x:31.643, y:-2.913, z:3.325", three decimals
  each with the trailing zeros kept. Item ID is ONE field, "Element ID: 1554240", and that
  label is read off whatever property carried the id rather than being a constant.
  Tolerance carries its unit with no space, "0.025m". Distance is the raw signed number,
  negative on a hard clash, written as a number so it still sorts. Type reads
  "Hard (Conservative)". Measured, see docs\scan.md section 4k
- Our extra columns, family, type name, material, source file and discipline, come AFTER
  theirs and never in place of any of them, on the CLASH XML and nowhere else. The Client
  columns only tick box that used to switch them off is GONE, because the workbook became
  one sheet laid out as theirs with none of ours on it, so there was nothing left for it to
  remove and it sat in the window doing nothing. Ours says Type Name
  rather than Type, because the client already has an Item Type column and it holds
  something else, the Navisworks item type, which reads Solid
- The client's report has NO Layer column. Measured on both supplied exports and on the
  stylesheet, which has one behind a flag that was off. Item Name and Item Type are not
  fixed columns either, they are the quick properties, and the accepted report happens to
  carry those two. So the thirteen are the whole of it
- Grid Location is built from the grid intersection name, which ALREADY carries the level.
  Appending the level to it wrote "D-8 : LGF : LGF" on a real run. The level is only added
  where the grid does not already end with it
- The Type cell carries the client's wording, "Hard (Conservative)", not the file's token,
  "hard_conservative". One pair is measured and the rule read off it is that the first word
  is the type and the rest is a qualifier in brackets. A value that already has a space or
  a bracket in it is left exactly as it is
- An all zero GUID is not an id. A run wrote
  "Instance GUID: 00000000-0000-0000-0000-000000000000" into all 426 item cells, which
  reads like an id and identifies nothing. The cell is left empty instead, and the real
  cause is fixed too: the clash gives back the geometry leaf, which on a Revit sourced NWC
  carries a material name and no Revit properties, so the id, the family and the type are
  looked for on the item, then on its composite item, then up its ancestors
- Distance is the raw signed number and the cell carries a three decimal number format, so
  it reads the way theirs does and still sorts. Without the format Excel printed
  -0.328083992004395 where theirs shows -0.050. The VALUE differing between two reports is
  the two documents' units and not a fault: ours measured in feet and theirs in metres, and
  the tolerance says so too, 0.2461ft against 0.025m
- Client columns only means nothing of ours on the sheet at all, not just no extra columns.
  The notes above the table, the link back to the Summary and the filter arrows are ours
  and all three go. Their columns do not move
- The client's logo is not copied. It is an Autodesk logo that ships with their report and
  not with this install, and there is no right to redistribute it
- A link to an image is written as a relative Uri and never as a plain string. Handed a
  string, ClosedXML reads a relative path as an INTERNAL address, so the cell tries to
  jump to a sheet of that name instead of opening the picture. There is a test that opens
  the written xlsx as the zip it is and reads the relationship back, because the object
  model reported this as fine while the file was wrong
- The report still counts skipped, passed and found as three numbers that are never
  merged. On 1C07BC that is 1164 skipped, 618 passed and 48 with clashes, and those add
  to 1830. A skipped test says so and never reads as passed
- The client's report is HTML (Tabular), not a workbook. Clash Detective cannot export an
  xlsx at all, so what they accepted is a page exported from Clash Detective and opened in
  Excel. That is why the sample declares 53 columns with 17 populated, carries merged cells
  and has absolute file:/// links in it
- The page is rendered by Autodesk's own clash_report_html_tabular.xsl, read from the
  install at run time and never copied into this repo. The path is built and tested
  directly, the language the application reports and then en-US, never searched for. Where
  neither is there the log names both paths, no page is written, and the workbook and the
  XML are untouched. It is on by default, because it is the format the client accepts
- Not one column in that page is fixed. Every one is a boolean over an XPath in the
  stylesheet, so what the page holds is decided by what our XML holds. The Layer column is
  the proof: 1A04WE has it and 1A02WE does not, from the same tool, because one export
  carried layer data and the other did not. Never write a column list for that page, write
  the data and let the stylesheet decide
- Our XML feeds that stylesheet, so its shape is not ours to choose either. Measured on
  2026-09-01, it answered every column test but three: description, smarttags and the href
  on a result, which are the Description column, the Item Name and Item Type columns, and
  the Image column. See docs\scan.md section 4m for the whole table
- Every report this tool writes is READ BACK off the disk and checked, and what the check
  found goes in the log as a block and in the window as one line. Bader was opening every
  report in Excel and searching it by hand, and the tool wrote the file. The FILE is read,
  never the object that produced it, because the object model has twice called a written
  file fine when it was not
- The page check reports the rows, how many carry an Item ID on each item, the first id in
  full so its shape is visible, any column that is not the client's named, the tolerance
  cell exactly as written, how many picture references there are and how many use a
  backslash and how many are really on disk, and whether the logo is there. The workbook
  check reports the rows and, per column of ours, how many rows filled it. A column filled
  zero times is NAMED, which is what would have caught Source File and Discipline
- A report check NEVER fails a group. It is a warning on its own list, apart from the
  errors, because the NWF, the NWD and the workbook were all still written. Judging a group
  on it would repeat the fault that once reported a clean 22 group run as FAILED
- The client column set is read from clash_report_html_tabular.xsl and from the exports in
  samples\client-report, never typed into the code. The stylesheet gives what is possible
  and the samples give what is theirs. A test re-reads both on every run and fails if the
  set has drifted, so the list is checked against the files rather than trusted
- The stylesheet carries a DEAD template named ItemHeaderCells that nothing calls. It
  writes Item Type as a literal, reads ./Name with a capital where the rest of the file
  reads name, and puts Item ID and Layer after the quick properties rather than before.
  None of that is what the reports carry. The live one is mainTableHeader and it is the
  only one anything here reads. Measured, see docs\scan.md section 4o
- A property value is read by its KIND, never with ToDisplayString alone. Every accessor
  on VariantData is kind specific and throws on any other kind, and a Revit element id is
  an Int32, so ToDisplayString threw 426 times on one run and took the whole of Describe
  with it, losing the element id, the source file and the discipline together. The one
  member that returns a value regardless of kind is ToString, whose IL switches on
  GetDataType, but it prefixes the kind and hands back "Int32:702888". So the kind is read
  and the right accessor called, with ToString as the fallback for a kind nobody has seen
  and its prefix stripped. Measured, see docs\scan.md section 4n
- Anything that can throw per item goes in its own try. The source file and the discipline
  are read last and were lost to a throw three properties earlier, on all 426 items, which
  is one throw costing three columns
- The page carries ONLY the two quick properties the client's report has, Item Name and
  Item Type, whatever any tick box says. The stylesheet makes a column out of every
  smarttag, so Family, Type Name, Material, Source File and Discipline would each become a
  column on the page the client receives. They are workbook columns. createddate goes the
  same way, because neither supplied report has a Date Found column
- Tolerance is written to THREE decimals, which is how both of theirs are written. Ours
  read 0.2460629921ft against their 0.025m. The units differing is the two documents
  differing and is correct. The precision was ours
- A picture reference on the page uses a BACKSLASH, for the clash pictures and for the
  logo, because that is what both supplied reports write and the client opens these in
  Excel on Windows. The workbook keeps a forward slash, because a hyperlink there is a Uri
  and their own xlsx has no picture hyperlinks at all to match
- Neither their report nor ours embeds a picture in the xlsx, measured on the zip of each.
  The native export never does. Pictures show only when the _files folder sits beside the
  file, so the page and its folder are what gets sent. That used to be said by a tick box
  and the box is gone, because the accepted report has photos and one without them is not
  the thing the client agreed to receive, so it is fixed on
- Exactly ONE objectattribute per clashobject, and it is the id. The Item ID cell is
  value-of over ./objectattribute/name, which takes the FIRST node, so writing several put
  the item's Name in the id column. Everything else about an item is a smarttag, which is
  how Item Name and Item Type reach the page at all
- Every clashobject carries the SAME list of smarttags, empty values included. The
  stylesheet counts them once off the first clashobject and uses that count for every row,
  so one item with fewer of them slides every column after it sideways
- Date Found and our five extra item properties are ours, not theirs. Their report has
  neither, so both are left out when the client layout is asked for and appended after
  theirs when it is not
- The logo is DATA, read by the stylesheet from //logo/@href, not part of the layout, and
  the page carries the one Navisworks puts on its own reports because that is the report the
  client accepts. It is read off the install at run time, the same way the stylesheet is,
  from \Images\logo.jpg at the top of the install with no language folder, measured
  2026-09-01 at 6137 bytes and byte for byte the same file as the logo.jpg in both supplied
  reports. There is nothing to set up before a first run
- No copy of that logo is in this repo, in the bundle, or in install.ps1. It is read off
  the machine that is running, every run. The only two in the checkout are inside the client
  exports Bader committed, where Navisworks had already put them, and a test names that
  exception so one appearing anywhere else fails
- The logo is COPIED into the report's own _files folder beside the clash pictures and
  linked relatively, which is what Navisworks itself does. The report goes to a client, so
  the page must not point at a path on the machine that wrote it. The accepted xlsx carries
  absolute file:/// links and that is exactly why its pictures break everywhere else. A test
  opens the written page and reads the src back out of it, because the object model reported
  a broken link as fine once already
- A logo that cannot be found writes the page without one, names every path it looked at,
  and carries on. A missing picture never stops a report
- The Browse box stays, for a project that needs a different mark, and it starts filled with
  the install's logo. Clearing it means no logo. Most people never touch it
- Writing a logo turns the stylesheet's Image column on even with no clash pictures, because
  its test is boolean(//@href) and the logo carries an href too. That is Autodesk's own
  behaviour and their reports do the same. Pinned by a test so nobody reads it as a fault
- The clash XML is optional and off by default. It is built from the same results the
  workbook is built from, never by reading the workbook and never read by it, so a fault
  in one cannot corrupt the other. Its shape was read off the three stylesheets the
  Navisworks install ships, because there is no clash report schema anywhere in the
  install. What is filled and what is left out is listed in ClashReportXml, and a part
  with nothing to put in it is left out rather than written empty
- The workbook library is ClosedXML, and the writer lives in Federator.Core rather than in
  the add-in so the tests write a real xlsx and read it back without Navisworks. It ships
  twelve more DLLs into the bundle and install.ps1 carries every one of them, because the
  add-in would otherwise load and then throw the first time a group finished. None of them
  collides with a file the Navisworks install ships
- Images are ON by default, because the report the client has already accepted has them
  and one without them is not the thing they agreed to receive. They go where that report
  puts them, in a folder beside the workbook named after it with _files on the end, as
  loose jpg. The workbook links to them and does not paste them in. A thumbnail in the
  cell is a tick box, off, because pasting is not what the accepted report does and it
  makes the file many times larger. No clash is ever saved as a viewpoint in the NWF
- The picture names are theirs and are NOT one running sequence, which is what the first
  dozen look like. It is cd, then the test formatted 00, then the clash within that test
  formatted 0000. Test 0 clash 1 is cd000001.jpg and test 100 clash 1 is cd1000001.jpg,
  seven digits, measured on a real 2672 picture export. Two digits is a floor and not a
  width. The test number counts tests that have pictures, so it can never leave a gap, and
  a test whose first render fails hands its number back
- The pictures are NUMBERED IN THE EXPORT ORDER, which is the order the rows are written:
  tests most clashes first with ties in creation order, and inside a test the clashes as
  Clash Detective lists them. Measured off both exports in samples\client-report, where
  the first block is cd00 and the second cd01 whatever order the tests ran in. A picture is
  rendered while its test runs, under the run order number, because the report order is
  only known when the last test has run. So after the run every picture is renamed ONCE,
  in one pass, and the row, the workbook link, the XML href and the page all follow it. The
  rename goes through a holding name first, because a swap between two tests would
  otherwise write one picture over another. What a picture shows and its size are
  untouched. The rule lives in Federator.Core.Report.ReportOrder and the rename in
  Federator.Core.Report.ImageRenumbering, so both can be tested without Navisworks. A
  rename that fails is a warning on the report and never fails the group
- The image settings: on by default, a cap per test defaulting to off, a status filter
  defaulting to New Active and Reviewed, and a size defaulting to 1024 by 1024 because
  that is what all 60 pictures of the accepted report measure. An image that fails renders
  nothing, is logged by name, leaves its cell empty and never stops the run. Fifty
  failures of the same reason stop the run, the same way and with the same default as the
  clash step, and the line says images rather than tests
- What the pictures cost is MEASURED and logged per group: seconds per image, total
  seconds, how many, and how many megabytes. Every number anyone has given for this,
  including mine, was a guess until this existed. The same figures go on the Summary
  sheet, because the log is not what gets sent on
- The report goes out in METERS, always, whatever the document measures in. Q23. It used
  to go out in the document's units, with the rule that converting a number ourselves
  while the document read feet would make the numbers and the label disagree. The run of
  2026-09-07 settled it the other way: 11 of 14 groups set every model to meters, the
  document still reported feet, and the report went out in feet with the tolerance at
  0.246ft. So the conversion is done, once, in one pass over the FINISHED report, in
  Federator.Core.Report.ReportUnits, before the workbook, the page or the XML is written.
  All three read the same converted report, and the same pass writes the unit label, so a
  number and its label still cannot disagree. What is converted is the tolerance of every
  test and the distance and the clash point of every row, and nothing else, because a grid
  location is text and a raw count is a count. The factors are UnitTable, the one unit
  table in this repo, read through ExchangeUnits, and a unit it has not been taught is
  REFUSED: nothing is written and the group FAILS, because a report in the wrong unit
  reads as real and is not
- Every unit this tool knows is one row of Federator.Core.Units.UnitTable, F33: the name
  on the Navisworks enum, the words the window shows, the short label the report and the
  log write, the exchange file's units code, and the millimetres in one. ExchangeUnits
  converts through it, DocumentUnits and ClashRunner name units through it, and the
  Outputs step offers the rows it offers. There is no other list, because the lists had
  drifted, the same unit was micrometers in one and um in another. A model units name the
  table does not know FAILS the group with nothing converted, it used to fall back to
  Meters without a word. The exchange reader reads the tolerance as written and converts
  nothing, so a file in a unit the tool does not know reads, and ClashTestPlan.Convert,
  the one place a file unit is judged, skips each of its tests by name
- Setting the models is still done, BEFORE the clash step, and it is kept because it fixes
  what the person sees in Navisworks. Measured on 2026-09-01 across all 4027 types:
  Document.Units is read only, Model.Units is read only, and the only public managed member
  that sets units at all is
  DocumentModels.SetModelUnitsAndTransform(Model, Units, Transform3D, bool). So each MODEL
  can be set and the DOCUMENT cannot. Whether Document.Units follows is UNKNOWN and no
  longer matters, because it no longer decides the report. What Document.Units actually
  reports is UNKNOWN too: setting a model writes a per file override, the same one the
  Units and Transform dialog shows, while what the scene is measured and displayed in is
  an application option, Options, Interface, Display Units, which Autodesk documents as
  what tolerances for clash detection are set in. Whether Document.Units reads that option
  is not readable off the DLL, so it is not claimed. The words DID NOT FOLLOW are gone from
  the log. The combo on the Outputs step is MODEL units, defaulting to Meters, and its help
  line says the report is always in metres
- A federation that already exists needs NO SCAN. In Navisworks a person opens a file,
  opens Clash Detective, presses Run and reads the results, and nothing asks them where
  their models came from. So there are two ways to run and they are told apart by what they
  name: the Run button on the Source step names a count of ticked groups, and the Run the
  open file button names the open FILE and the two paths it will write. The open path is
  the same flow with the first step removed. There is no Decide, because the document IS
  the file list, nothing is appended and nothing is cleared, so the clash results inside it
  survive. The clash file is optional there, and without one the tests saved in the
  document are run where they sit, which is the ordinary weekly case, and a document
  holding none runs nothing and the log says so. The outputs are named after the open
  file with the extension swapped and the report in Clash Reports beside it, which is
  exactly where the scanned path puts them, so the same building run either way writes the
  same files. No pattern is applied, because the name is already on the file. Five things
  are checked before it runs and the first that fails is named in the window and the log,
  D2 on 2026-09-12: it has a name, because an unsaved document has nowhere to put an NWD
  beside it, it was opened from a folder and not from an address, it is an NWF, because
  that is where the clash tests and their results live and the NWD this tool publishes
  would otherwise be written over the file that is open, it has a folder in front of its
  name, and that folder can be read from here. The NWD path is never the open path, read
  case blind, as a second lock on the same door. The rule lives in
  Federator.Core.Rerun.OpenDocumentJob so it can be tested without Navisworks
- Distance is the ROUNDED number and carries no number format, because that is what theirs
  holds. Ours stored -0.328083992004395 behind a format of 0.000, so the cell read -0.328
  and anyone sorting, filtering or copying the column got the long value. Rounding the
  value and rounding the display are different things and theirs rounds the value
- The Layer column carries the LEVEL. It was an item property nothing ever filled, so the
  same clash read LGF on the page and nothing in the workbook. Where the item has its own
  layer that wins, and otherwise the clash's level is used
- The Image cell is EMPTY and carries the link. Theirs holds nothing there at all with the
  picture behind it, so writing the file name put a string where their report shows a photo
- A test status of Complete is written OK, because that is their wording. Anything else
  goes through as itself
- The workbook is BANDED, and the colours are theirs, read off the accepted export. Grey
  behind every heading, blue over Item 1 and pink over Item 2, paler on the clash rows than
  on the headings, every cell boxed medium and the test header ruled thick. Ours was plain
  white with no border anywhere, which is the first thing a person sees before they see a
  single number. The five colours, the seven row heights and the nineteen column widths are
  in Federator.Core.Report.ClientLayout, and ClientLayoutTests opens their file and asserts
  every one of them against it, so the table answers to their export rather than to whoever
  typed it
- Their test header is a table NINE columns wide that stops at Status, so L to S on those
  two rows are not cells of theirs and are not expected to carry anything. Their borders
  follow the merge runs, the left edge on the first column of a run and the right on the
  last, which is what keeps our cells identical to theirs where a run is merged
- The report check compares CELL AGAINST CELL: the value, the data type, the number format,
  the fill, the borders, the row height and the column width, with both sides printed. The
  check before this compared which columns, in what shape, in what order, and passed with
  eight visible differences sitting in the file, because none of them was in front of it.
  It walks the first block only, since every block is painted by the same code and 1830
  blocks by nineteen columns is a check nobody reads
- A check that reads the same table the writer wrote from CANNOT catch that table being
  wrong, and this is the third time a check here has reported clean over a real difference,
  so what it cannot catch is written into its pass line rather than left to be discovered.
  Four things. The table being wrong, which only ClientLayoutTests catches by reading their
  file. Anything not in the compared list, which is fonts, the sheet name, freeze panes,
  print setup and merged ranges, all listed in docs\scan.md 4q with both sides. Whether a
  value is TRUE, since a number to three decimals off the wrong clash still reads right.
  And a block that was never written at all
- Every check gets a test that BREAKS one thing and asserts the check names it. A test that
  only asserts the good file passes would have passed against all eight of the differences
  above. Fourteen of them live in WorkbookCellCheckTests
- A tick box has to earn being a decision. Fifteen went to eleven, of which ONE is visible
  without opening anything. Republishing the NWD, writing the client page and rendering the
  photos are fixed ON, because a weekly run wants all three every time. Client columns only
  is gone outright, dead since the workbook became one sheet with none of ours on it.
  Dating the NWD, the clash XML, the thumbnails and the five image status boxes are
  collapsed under More, rarely changed. The two that destroy data are collapsed on their
  own under Things that destroy data, because they do not belong beside ordinary output
  options. Fewer decisions is the goal, not more words explaining them
- The scan reports what it noticed and never acts on it. ODD SHAPE, NEAR MATCH,
  SINGLE DISCIPLINE and MISSING are information. Nothing is blocked, unticked or
  merged, and no code is assumed right. Bader decides
- Findings are worked out from the run itself. The set of disciplines and the set
  of code shapes come from the files in front of it, never from a list in the
  code, because every project differs. A shape is the pattern of letters and
  digits, so 1B06PK is 9A99AA. A code is odd only when its shape is held by no
  other code and some other shape is shared, because with every shape unique
  there is no majority to differ from
- Findings show in the Grouping step before Run is pressed, and go in the log in
  a FINDINGS block after the group list. Nothing odd is one line, not an empty
  panel

## What a tick box says

A tick box is a SHORT LABEL and one grey line under it. Bader called the long ones a
nightmare and he was right: they ran off the edge of the window, they shouted in capitals,
and they explained the off state as well as the on state, so nothing stood out.

- the label is at most EIGHT words and names the thing, not the mechanism
- one grey help line under it, at most TWELVE words, saying what it costs
- no capitals for emphasis anywhere. Acronyms a person says out loud are fine
- do not describe the off state unless it changes what someone would choose
- the two that destroy something carry a red exclamation mark beside the label. A mark,
  because when every label shouted, none of them did

The numbers in a help line are measured, never estimated. Photos are about 0.08 seconds
each and 213 took 17 seconds. Pasting them takes the workbook from 0.3 MB to 52 MB.

There were fifteen and there are eleven, of which ONE is visible without opening anything.
A box only stays if a normal weekly run genuinely has to choose. Everything else became a
fixed behaviour with the sensible answer chosen, or moved under an expander. The two that
destroy data are under one of their own, because they do not belong beside ordinary output
options. Which box went where and what each removed one was fixed to is in docs\scan.md 4q.

build\probe-window-labels.ps1 reads every tick box out of the real window and checks all
of this, so the limits are proved rather than remembered. It OPENS every expander first,
because a collapsed one has no visual tree behind it and the probe found one box and
reported no problems, and it says how many are visible without opening anything, which is
the number the window is judged on.

## The diagnostic log

The log is what Bader sends back when something goes wrong, so it is built to
survive the crash rather than to be tidy.

- every build stamps the assembly with the git commit and the moment it was built,
  into AssemblyInformationalVersion, and the log prints it in SESSION and the
  window shows it in its title bar. The plain assembly version is 1.0.0.0 and
  always will be, so on its own it cannot tell a fresh install from a stale one and
  Bader ran an old binary twice before this existed. Never fake this with a hand
  edited version number. It costs a full recompile on every build, which is about
  three seconds, and that is the trade

- every line is written and flushed all the way to the disk as it happens, with
  FileStream.Flush(true). Nothing is held back to the end, so a process that dies
  inside a Navisworks call still leaves everything up to that moment on disk
- it opens on the first line of the button handler, before the folder is read and
  before the window opens, so a run that dies at startup still produces a file
- two places, always. The fixed path
  %LOCALAPPDATA%\ParsonsNwcFederator\logs\run-yyyyMMdd-HHmmss.log never depends on
  a folder the user picked, and a copy goes next to the NWF folder at the end. If
  that copy fails, the reason goes in the first log and the run carries on
- logging is never the thing that stops a run. That holds for the copy, for
  retention, and for the log file itself, which falls back to the temp folder and
  then to window only output
- a size is only logged after File.Exists passes and the real size is read back.
  Never log a size that was not read
- on start, the oldest logs are deleted until 30 remain, the live file included.
  The live file is never a candidate. A delete that fails writes one line naming
  the file and the reason. 30 is a setting

Two things that look like mistakes and are not:

- while a run holds the log open, File.ReadAllText fails with a sharing error.
  Anything reading a live log opens it share-aware. Notepad and the Copy log
  button both work. There is a test pinning this
- the header is two blocks on purpose. SESSION is written at button press and
  proves the button fired even if the scan never ran. RUN SETTINGS is written at
  Run because the folders and the counts do not exist until the scan finishes.
  Do not merge them

## Build

Filled in from docs\scan.md after the scan ran on 2026-08-27.

Everything, which needs Navisworks on the machine because of the add-in project:

    dotnet build ParsonsNwcFederator.sln -c Release

The parts that need no Navisworks, which is all of Federator.Core:

    dotnet test tests\Federator.Core.Tests\Federator.Core.Tests.csproj

There is no Visual Studio and no .NET Framework targeting pack on this machine, so net48
compiles only through the Microsoft.NETFramework.ReferenceAssemblies package. Every
project references it. That makes nuget.org a hard requirement for building at all.

The add-in finds Navisworks through the NavisworksPath property, default
C:\Program Files\Autodesk\Navisworks Manage 2025. Override it when the install moved:

    dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"

Any check for a Navisworks file tests that one full path directly. Never search the
install folder, never recurse it, never wildcard it. Exists() in MSBuild and Test-Path
on a joined path are the only two forms used, and each referenced DLL is checked on
its own so a missing file is named. A recursive walk of that folder was investigated
on 2026-08-30 and works fine, so this rule is about keeping the check direct and
self-diagnosing, not about working around the machine. docs\scan.md holds the
measurements.

Tests are NUnit. The pre-commit hook runs them and refuses the commit on a failure. Turn
it on once per clone with:

    git config core.hooksPath .githooks

## Tests

Testable here, so these get real tests: the name parser, the building grouping,
the clash XML reader, the sets XML reader, the Excel writer.
Not testable here, so these get review instead of tests: anything calling the
Navisworks API. Write those in larger reviewed pieces, not many small attempts.

## Confirm against the install, do not assume

- exact signatures of Document.AppendFile, SaveFile, PublishFile and Clear
- the ClashTestType value that matches hard_conservative
- the Clash Detective report defaults, so the tool starts where Navisworks starts
- NwdExportOptions is a 2026 class. It does not exist in 2025

## Writing

No generated-by or co-authored-by line. No emoji. No em dash. No semicolon in
prose. No comment that repeats the line under it. Say UNKNOWN rather than filling
a gap. Never report a check that did not run.