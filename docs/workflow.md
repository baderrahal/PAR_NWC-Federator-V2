# The two pages and the workflows

The window has two pages, NWC and NWF, since F188 (Bader's answer 3 of Q174). The NWC page builds a
federation for the first time. The NWF page does the weekly work on an NWF that is already built. A
label on each group says which way it goes, the confirm dialog counts the labels, and the log carries
them. The labels come from `Federator.Core.Rerun.RunPath`, confirmed by Bader on 2026-09-07, and they
change what the person is told, never what the engine does.

Everything below is read off the code and the rules of 19 Oct 2026. What no run has shown yet is said
UNKNOWN, and the list of those is at the end.

## The NWC page, the first build of a group

The page has four steps in a row of tabs, `FederatorWindow.xaml`, packages first since F232, Bader's
round of 19 Oct 2026, fix 2.

    1. Packages    the parent folder with Browse and Read, the Report folder with Browse, the
                   tree of communities and packages with a tick box each, Tick all and Clear
                   all, and under the tree the findings over the NWC files of the ticked
                   packages. Nothing in the findings blocks a run
    2. Grouping    how the files are gathered into federations (per building is the default),
                   the Rebuild box, and one row per group of the ticked packages with its Run
                   box, its community, package, building, files, disciplines, the label it will
                   take, and its status. A group of one discipline reads One discipline, not
                   built, and its box is off and greyed
    3. Outputs     the name fields and the logo, nothing else
    4. Clash       the tolerance drop down, the viewpoints box, the two Reviewed rules, the
                   Run the open file button and the hand buttons

The one Run button, along the top of the bar under the page, runs the groups of the ticked packages
that are ticked on step 2, each group in a Navisworks of its own, below. There is no Source step, no
NWF or NWD folder box and no step 5 since F232. The photo size, the cap per test, the five status
boxes of the photos, the ghosted look, the dated NWD and the clash XML have no box since F232 and run
at their defaults, `ImageOptions` and `ReportOptions`.

The clash matrix is built into the tool, F155. Nobody picks an XML. A run chooses its pack by the
project code of the NWC file names, and stops before anything is opened where none or two match.

Per group a first build runs in this order: append the NWC files, save the NWF, build the sets,
create the tests, run them, build the viewpoints, save the NWF again where the clash step put
something in it, publish the NWD, look at the NWF once more, then take the pictures, write the Excel,
the HTML, the XML where its box is ticked, the coverage and the internal workbook. The NWD is published before the
pictures, F182, so a run that stops during the pictures still leaves the NWF and the NWD whole. Each
group writes its files before the next group starts.

### A group whose NWF is already there

The NWC page refuses it. A group whose NWF is at its output path cannot be ticked, its status reads
"NWF exists. Use the NWF page.", unless the Rebuild box on step 2 is ticked and confirmed
(`NwcPageRule.CanTick`). Run applies the same rule to each group and names the groups it left out in
the PACKAGE GROUPS block of the log (`FederatorWindow.Packages.cs`, `NwcPageRule.CanRun`).

### Rebuild and its dated folder

Ticking Rebuild opens a confirm with Cancel as the default. It names the groups that already have an
NWF and says that the clash history of the old NWF, every status, assigned to and comment a
coordinator wrote, is not carried into the new files. On Cancel the box unticks again. Unticking it
afterwards unticks every group that has an NWF.

A rebuild never writes over the old files. The new NWF and NWD go into a new folder under the NWF's
own folder and under the NWD's own folder, named `run` then the date and the minute, such as
`run 2026-10-18 1427`, and a second one in the same minute takes ` (2)` after it. The old NWF stays
where it was with its history. The log says it on a REBUILD line (`FederationEngine.cs`,
`TheDatedJobWhereTheNwfIsThere`).

The group is then built at a path where no NWF is, so Decide finds nothing there. By the code its
label at the end reads First run (`NwfComparison.NoNwfYet` gives Build and `RunPath.Label` turns Build
into First run). The Run as column before the run looks only at the undated path, so it reads
Weekly run plus matrix for such a group (`FederatorWindow.Packages.cs`, `RunPath.Expected`). What a real
press of Rebuild writes in that column and in the log is UNKNOWN until a run reads it.

## The NWF page, the weekly work

The NWF page has its packages first since F232: the Report folder with Browse and Read, a tree of
community, package and NWF read off that folder with a tick box each, Tick all and Clear all. Under
it sit the tolerance drop down, five steps with a box each, and one Run button. The words are
`PackageWords` and `NwfPageWords`. There is no NWF picker, no File or Folder button, no list of NWFs
last used and no panel on the right since F232.

- The tree. The first level under the Report folder is a community. A folder below it whose NWF
  folder holds NWF files is a package, at the path the NWC page wrote it under, and its NWFs are its
  lines (`PackageTree.ReadNwfs`). An NWF is judged by its extension alone, so an NWD, an NWC and a
  .nwf.bak are never taken. Ticking a community or a package ticks every NWF under it, and one NWF
  is ticked alone (`NwfTicks`). Nothing is ticked until you tick it.
- Changed files. With one NWF ticked, the list under Run is what changed among its NWC files since
  its last run, read off the manifest that run left in its Log folder and the disk. With no earlier
  manifest it says UNKNOWN, and the run writes one. With none or several ticked it says so, Q214.
- The five steps, each with a box, all ticked when the page opens: Refresh, Run tests, Pictures,
  Viewpoints, Reports. A step that needs another is greyed and the grey line says why. Viewpoints
  and Reports need Run tests, and Pictures needs Reports (`NwfPageSteps`).
- The penetration box is this page's own since F235, and the Clash step's by design box is read by this page's run too
  (`ReportsWanted`). The tolerance is
  the drop down of this page. The grouping, the pack and the search set build are not on this page.

### The confirm

Pressing Run opens a confirm with Cancel as the default, and nothing is opened or changed before OK.
It names how many NWFs of how many packages run, the steps that will run and the Backup folder of each
package where a copy of each NWF is made first. It states the tests that run again where they sit,
that a test whose two sides carry one set code is not run and one saved in the NWF keeps its earlier
results there, that the viewpoint folder is deleted and made again, with how many viewpoints it holds
and that one moved into it by hand goes with it, that everything else at the top of the viewpoint
panel is deleted except the folder Comments, and how many old jpg files in the Clash report folder
are written again or removed. A number it cannot read it says UNKNOWN (`RefreshConfirm.Text`). The
old jpg files are counted across the Clash report folders of every ticked NWF, F217. It asks nothing
about the document open, since every NWF opens in a Navisworks of its own and nothing open in the
Navisworks the person started is touched.

### What the refresh does

For each NWF the run copies it into a Backup folder beside the Log folder, under its name and the
moment to the second, and never over an earlier copy (`NwfBackupName`). It opens the NWF, asks
Navisworks once to update the linked files and compares them with the manifest by size and last write
time, a SHA-256 only where one of those differs. It reads every clash result before the tests run
again and after, by status, assigned to, comments, approver and group, and names a Reviewed or an
Approved that a coordinator set that is not the same after. The route never removes a model, appends
one again, moves or renames an NWC, or deletes a file. A linked file Navisworks cannot find stops the
run with nothing saved. When it ends, a table shows each test that moved, New, Active, Reviewed,
Approved and Resolved, before > after. The words are in `core.md` THE REFRESH OF A PICKED NWF.

The run's label for a picked NWF is Weekly run, or Weekly run plus matrix where the built in matrix
was read, which is every run since F155. A NWF that opens and reads empty is stopped, and one that
will not open or is refused before the open is counted as a failed group in RESULT (`RunPath.ForAnOpenFile`).

## Many communities at once, the packages

The NWC page's step 1 reads its tree off the parent folder and the NWF page reads its own off the
Report folder, F208 and F232. The Report folder typed on one page reads the same on the other.

- Read. The NWC page's tree is read off the parent folder. The first level under it is a community, and
  every folder below a community that holds NWC files directly is one package. A community holding its
  files itself is the package named `<community> itself`, and a parent folder holding NWC files itself
  is one package named after it, `<parent> itself` under the community of its name, F232. A folder under
  the parent with the parent's name leaves the parent's own files out and says so, Q213. Nothing is
  ticked until you tick it. Run refuses with the reason while nothing is ticked, and the log lists
  every package ticked and left out, each with its full path.
- Where a package writes. Under the Report folder and the package's own path from the parent, such as
  `Report\C06\Infrastructure`, its NWFs in a folder NWF and its NWDs in NWD, the Log and Clash report
  folders beside them, and the Backup folder for the NWF page's copies. A Report folder inside the
  parent folder is refused.
- On the NWC page Run builds the groups of the ticked packages that are ticked on step 2. On the NWF
  page Run runs the ticked NWFs, in the tree's order.

### A fresh Navisworks for every group

Pressing Run on either page, once, plans and confirms as before, with one confirm
that says each group runs in a fresh Navisworks of its own, F214 and F222 (Bader's answers to Q204
and Q211). Then `Federator.Runner.exe` starts one Navisworks for each group in turn, the next only
after the last has gone, runs the group in it, and quits it. A group that fails, crashes or hangs
past its ceiling never stops the next. The runner times each start and each quit, and the one
joined RESULT in the window's log names every group under its package, with one line for each group
that wrote no result. The window that started the run writes the four discipline workbooks of each
package, and RESULT is written last. A package whose groups wrote no result is said so and never
said as not run, because whether its groups wrote files before their Navisworks ended is UNKNOWN.

### The discipline workbooks

Each package gets one workbook per discipline, AR, ST, ME and EL, F244 (Bader's order of 19 Oct
2026), copied from the mock in docs\design, whose script gives every colour, size and word. Four
sheets. Summary: the title, `<community>   <discipline> model checks`, the run and the reference, one
sentence of what needs a fix, then the six checks, Coordinates, Missing model, Naming, File age, CAD
imports and Generic Models, each with what it checks, the models or buildings it checked, those with
an issue, a status chip and the words for the modeller. Models: one row per NWC of the discipline
and one per building that has none, with the model it was compared with and its distance, every
distance and not only the far ones. To fix: one row per thing to fix, High first, with the words to
send the modeller. Element IDs: one element a row, to paste into Revit's Select by ID. Coordinates
are measured as the ALIGNMENT block measures them, and a model more than 30 m from the reference is
High. A reference on Revit's internal origin is High itself, and every other model of its building a
Medium row until it is fixed. A group of one discipline is on Models as NOT CHECKED and nowhere else.
What a check could not read is UNKNOWN on Models and said in the log under DISCIPLINE REPORT, never
written as a pass, and the log reads the rows of To fix and Element IDs back off the file on a CHECKS
line.

### The clash count check

Every run writes `<community> clash count check.xlsx` in each community's report folder, F236
(Bader's round of 19 Oct 2026, fix 6). It holds one row per group per test with clashes:
the building, the test, its priority, its clashes in Clash Detective, its rows in the Excel and in
the HTML, its clashes in viewpoints, the pictures made and skipped with why, and its rows by status.
Each number is read off the files the group wrote, its .tsv, its Excel and its HTML. A row is red,
with a COUNTS DIFFER line in the log, where Clash Detective's count differs from the Excel rows or the
HTML rows, or the views differ from the open clashes, New and Active, the statuses a view shows. A
total follows each group and the community. Under the table come the groups not built for one
discipline and, per group, the tests left out by the rule of one discipline or of two set codes.

Since F232 each page has this one route. The run of the open file on the Clash step and the hand
buttons still run in the Navisworks the person started.

Whether a Navisworks the person started, and not the automation, can start the runner's Navisworks
has the steps of F222 in steps\03_bader_next.md as its proof. How long a fresh start and quit cost on
a whole community is UNKNOWN until they are run.

A bad NWC never stops an unattended run, F213. Navisworks opens a message box inside the append of a
random file, a file cut short or a file another program holds. The tool presses it as a person would,
the append returns false, and the group ends FAILED with the file named. Whether the append of a
large real NWC opens such a box is UNKNOWN until the proof run.

## What is left out of a run

The tool reports what it noticed and never acts on it, except for these two rules, which Bader
decided.

- A group of one discipline is not built, on either page, F233 (Bader's round of 19 Oct 2026, fix 3,
  after Q203, F77 and F219). A group of one NWC and a group of two NWCs of the same discipline are the
  same case. Nothing of it is appended and it gets no NWF, NWD, Excel, Internal Excel, HTML, picture
  folder or viewpoints. The log names it in one line, `NOT BUILT  <community>  <building>  <discipline>
  <files>  one discipline, not built`, and RESULT counts them, one `not built` line per community. An
  NWF of such a group an earlier run left is not deleted and not run, and a second NOT BUILT line
  names it. On the NWC page the group is greyed on step 2 and left out of the run. On the NWF page it
  is judged once its NWF opens, off each model's own file name, before the refresh, so nothing is
  refreshed, saved or written, its GROUP line reads `not built` and it is in no count of done, partial
  or failed. The Backup copy the page makes of every NWF before it opens it is made for such an NWF
  too. Where the disciplines cannot be read, or a file the NWF links is not on the disk, the count is
  UNKNOWN and the NWF runs as before, a NOT JUDGED line saying why. RESULT says how many groups were
  not built on its `one discipline :` line, and the line under the Run button says them apart from
  the groups that ran.
- A test whose two sides carry the same set code is neither created nor run, on both pages, in
  every group of two disciplines or more, Q209 (F224). BLD-ST against BLD-ST is left out. BLD-PL
  against BLD-DR still runs. A side whose set name carries no code, such as BLD-Security Devices,
  leaves its test running, so nothing is left out on a guess. The built in matrix loses exactly 284
  of its 1830 tests, AR 120, EL 78, ME 36, ST 15, FF 15, PL 10 and DR 10. Every group writes one
  CLASH line saying how many it left out, and how many of the tests that run have a side that reads
  no code, so a nought is never read as every test judged. A group of one discipline does not read
  this rule.

### What an NWF refreshed before still holds

An NWF refreshed before F219 or F224 can hold tests that a rule now leaves out, with their earlier
results. The tool deletes none of them. The count check reads such a test as not compared and not
failed, F225, and the COVERAGE block of the group says on one line how many tests the rules left
out, how many are still held in Clash Detective from an earlier run and with how many clashes, how
many are held by neither side, how many failed, and how many have a holding that is UNKNOWN. Before
F225 the check read them as FAILED, about 3992 clashes on C06 and C07 (F219-R1 of the audit of 18 Oct).
Measured on the NWF page on 18 Oct 2026 over NWFs built before F224: 1B06BS held 28 with no result,
1B06M1 held 14 with 22 clashes and 1B06PE held 32 with 16, and none read FAILED.

Whether these tests are left where they are, deleted once, or moved into a folder of their own is
Bader's, Q212, and it is pending. Nothing is done with them until he answers.

## Auto-Save, off while the tool works and put back as it was

F223, Bader's answer to Q207. The tool switches Navisworks Auto-Save off before the first append and
puts it back exactly as it was.

- Before it switches anything, the packages runner writes the value it found into
  `auto-save-to-put-back.txt` under %LOCALAPPDATA%\ParsonsNwcFederator. If the value or the record
  cannot be read, or the record cannot be written, nothing is switched for the whole run.
- Before each group's Navisworks starts it writes the switched off value and reads it back. At the
  end of the run it puts the first value back once, reads it back, and deletes the record only where
  the value reads as it was. A runner that died leaves the record, and the window puts it back.
- The run inside a group's own Navisworks and the open file run set the option off as the run starts
  where it reads on, and set it back before RESULT. The hand buttons switch
  nothing.
- The log has an AUTOSAVE line for each of these.

Not measured, row F223-R1: whether the option set off in a session started with Auto-Save on stops
that session's autosaves, and what a Navisworks writes into the registry as it quits. Measured in
scan.md 5z-zl: a value set in a session reaches no registry value while the session runs. Why is
UNKNOWN.

## The labels

One per group in the Run as column, counted in the confirm dialog, carried in the log. There are seven
(`RunPath.All`).

    First run                  no NWF at the output path yet (a Rebuild's dated path, by the code)
    Weekly run                 the NWF is there and no matrix is built in
    Weekly run plus matrix     the NWF is there and the built in matrix is read, every
                               weekly run since F155
    Rebuilt                    the NWF is there and points at a different file list. Only
                               known once the NWF is opened
    Skipped (changed on disk)  a group whose file list differed and was left alone. Since F24 no
                               group ends this way, and the label stays so an older log still counts
    Stopped (the NWF read empty)  the NWF opened with no error and reported no models, so it is
                               stopped and nothing in it is cleared, F74
    Unknown                    the NWF folder is not picked yet, or a case the rule cannot name

Three rules in `RunPath` give them. `Expected` is the label before any NWF is opened, from whether the
NWF is already at its output path, so Rebuilt never appears there. `AfterOpening` is the label once the
NWF has been opened and compared but not yet run, where a comparison that reads Changed shows as
Rebuilt. `Label` is the label once the group has ended. `ForAnOpenFile` labels the NWF page's groups.

Since the NWC page refuses a group whose NWF is there, and Rebuild goes into a dated folder, whether
any press of today's window still ends in Rebuilt is UNKNOWN. The engine's rule for it is described
under The NWF is the record.

## The clash statuses

Two different sets of words get confused because they share one of them.

A CLASH carries one of five:

    New        it has not been looked at
    Active     somebody is dealing with it
    Reviewed   somebody looked at it and moved on
    Approved   somebody with the authority to accept it accepted it
    Resolved   it is gone from the model

A TEST carries one of four:

    New   Old   Partial   Complete

They are different things. The only word they share is New. **Old is a test word and never a clash
word**, so no clash is ever at Old and nothing in this tool ever moves one from it. What puts a test
into Old is UNKNOWN and is not guessed at either, so the test status is written in the report exactly
as the API reports it.

This tool sets **Reviewed** and nothing else. A clash that cannot be solved should not sit at New or
Active forever, and Reviewed says it was looked at and passed over.

It never sets Approved or Resolved. Both are a person's statement about work that was actually done,
the NWF is the only record of what has been fixed, and there is nothing to check either claim against
afterwards. It does not set New or Active either, because running a test produces them and setting
one by hand would overwrite a decision somebody had already made.

A status is applied between the run and the report, never after, so the workbook and the page carry
the new status and not the one read before the change. Writing a status is a write, so the NWF is saved
again on it.

**How the tool learns which clashes cannot be solved was Q33 and is ANSWERED**, on 2026-09-19. It is a
rule over the clash itself, and it is called a PENETRATION.

A clash becomes Reviewed when all four of these are true:

    one side is a SERVICE by item category, thirteen of them (`PenetrationSettings`)
        Pipes, Pipe Fittings, Pipe Accessories, Pipe Insulation, Ducts, Duct Fittings,
        Duct Accessories, Flex Pipes, Flex Ducts, Cable Trays, Cable Tray Fittings,
        Conduits, Conduit Fittings
    the OTHER side is a SOLID by item category
        Walls, Floors, Roofs, Structural Foundations
        Structural Framing and Structural Columns are NOT on the list, deliberately. A
        service through a slab, a wall, a roof or a foundation is a hole somebody cuts.
        A service through a BEAM or a COLUMN is a structural decision an engineer makes,
        so it stays at New for a person to look at. Q63
    the service measures 150 mm OR LESS
        every size property on the item is read and the LARGEST is taken, so a 600 by 150
        duct is a 600 and stays where it is
    the clash is at New or Active
        Reviewed, Approved and Resolved are somebody's decision and are never overwritten

Pipe insulation is on the service list because an insulated 100 mm pipe through a wall makes two
clashes, the pipe and its insulation, and they are the same penetration (F72a). Four more categories
are decided not to be a service, Air Terminals, Mechanical Equipment, Plumbing Fixtures and
Sprinklers, so an air handling unit through a wall stays at New.

Everything else is left exactly as it is and counted by reason, and the reasons are in a PENETRATION
block in the log with one line per clash moved and one line per reason nothing moved. Both lists and the
150 are settings. The 150 is the same number the viewpoints read, named once, and the two read it in
opposite directions: a viewpoint takes an item OVER it and a penetration takes a service AT OR UNDER it.

It writes into the NWF, so it was off by default until F235 (Bader's answer to Q195), which ticks it
by default on both pages. The NWF page has a box of its own and its run reads that box. The by design
box is unchanged, off and on the NWC page alone. A service whose size cannot be read is LEFT
ALONE, which is the opposite of what the viewpoints do with the same unknown, and that is deliberate:
the safe mistake in a viewpoint is showing something unnecessary, and the safe mistake here is leaving a
clash for a person to look at.

## Where every output goes

    NWF folder      the NWF files and nothing else. A rebuild's files go in a dated folder under it
    NWD folder      the NWD files and nothing else. The same dated folder under it for a rebuild
    Log folder      beside the NWF and NWD folders, made if it is not there. The run log and its .tsv,
                    the notes on the shared coordinates, the Excel, the internal workbook, the XML
                    and the nwc-manifest.tsv of the NWF page. The Excel keeps a dated name if one
                    of that name is there
    Package folder  the four discipline workbooks of a package, in the package's report folder
    Clash report    beside the Log folder. The HTML with its _files folder of pictures, so the
                    picture links of the HTML work. A file there under a name already there is
                    overwritten and never dated, F177
    Backup          beside the Log folder. One copy of an NWF before the NWF page refreshes it,
                    named with the moment to the second, never over an earlier copy

Under a package, all of these sit in the package's one folder of the Report folder. Reports never go
inside the parent folder read. An NWF of the NWF page writes its NWD, its Log, its Backup and its
Clash report folders into its package's folder, F208, and the open file run writes beside the open
file, F158. A file this tool did not write is never listed as written. A size goes in
the log only after the file is there and its real size is read back.

The window keeps its own state in %LOCALAPPDATA%\ParsonsNwcFederator: `nwf-places.tsv` and, during a
run, `auto-save-to-put-back.txt`. The list of NWFs last used, `recent-nwfs.txt`, is no longer read or
written since F232.

## Where the models sit, F201

Each NWC is appended with `Document.TryAppendFile`, the call the Append command makes, and nothing is
done to it after: no model's units are set and no model is moved. This is Bader's standing rule of
18 Oct 2026, Q202. The tool never changes a setting to append. Until F201 the tool set every model to
Meters before the clash step, which made each model 3.28 times larger about its own origin and moved
the models apart, up to 667 m on 1A04PK, so the NWD did not line up the way a manual Append does.
The report is still in metres, converted when it is written, and the clash tolerance is put into the
units the document reports.

The log says it per file. Once the files are in, the UNITS step reads each model's units and the
centre of its box. Before the NWD is written they are read again, and one PLACE line a file says held,
moved within 1 mm or less, or MOVED with the distance. A model whose units are not the document's is
named: an NWF built before F201 holds its models set to Meters, and a Rebuild on the NWC page puts it
right. The notice about models off the shared coordinates is unchanged.

## The open file

A federation that already exists needs no scan. The Run the open file button on the Clash step runs
the document that is open, names the open file and the two paths it will write, and is the same flow
with the first step removed. There is no Decide, because the document is the file list, nothing is
appended and nothing is cleared, so the clash results inside it survive. It is always a Weekly run, or
a Weekly run plus matrix where the built in matrix is read. There is no First run on that path, because
the NWF already exists and is the document. Five things are checked before it runs and the first that
fails is named in the window and the log, in `Federator.Core.Rerun.OpenDocumentJob`. Each ticked NWF
of the NWF page runs the same route in a Navisworks of its own.

## The two hand buttons

Sets into open model and Tests into open model, on the Clash step, are for trying one open model by
hand and are labelled as that. They are not steps in the run. Each calls the one engine method the run
calls per group, so the SETS and CLASH lines in the log read the same whichever way the work was
started.

## The pictures

Pictures are taken after the NWD is published, F182, one per clash row that takes one.

- A cap per test, F172, F175 and F190, only on a test of more than 750 clashes since F235 (Bader's
  answer to Q187): at most the first 150 clashes of such a test at priority A, 100 at B and 50 at C,
  and a test of 750 or fewer gets a picture for every clash. The 750 is a setting, and the IMAGES line
  of a capped test names its clashes and the 750. A saved test the matrix does not name is C, and so is
  a test with no priority read.
  The cap per test of `ImageOptions.CapPerTest`, off by default and with no box since F232, keeps its
  own job, and the lesser of it and the rule holds.
  The clashes stay in the HTML, the Excel and the viewpoints. Only the picture is skipped.
- Order, F203: tests whose waiting rows are at or under their cap are pictured first, then the ones
  over, so a stop takes the last pictures of the largest tests and not every picture of the small
  ones. Measured on the C04 run of 15 Oct 14:11, the stop at 3275 pictures left 163 tests with none,
  and all 162 tests under 50 clashes were among them.
- Two stops, both a normal end of the picture step. One past 9000 GDI objects, and one when the
  private memory of Navisworks passes 12 GB, which grew about 3.3 MB for each picture, F182. A NOTICE
  in the log gives the picture number and how many were skipped. The NWF and the NWD are already
  written. How often the memory stop fires with a Navisworks for every group is UNKNOWN until the
  first run of a whole community since F222. Before it, in F220 on C06, it fired 7 and 8 times from
  the 11th group of 21 on.
- Hide Other, F150. Clash Detective draws a picture by its display settings, which are per user. For
  each group's picture pass the tool asks for Hide Other ON, or OFF where the ghosted look box under
  More is ticked, only where it could read the value, and puts the user's value back after, the value
  the run's first group read. Whether the pictures follow is UNKNOWN until a run's picture sizes show
  it.
- The clash step and the pictures both show every item, F204 and F206, because Clash Detective finds
  no clash on a hidden item. The tool reads what the document hid, shows everything, and puts the
  hidden state back after.
- Pictures that show only the sky: 76 pictures of 1B06BC and 25 of 1B06P1 in F220 drew no item of their
  clash, the cause UNKNOWN, scan.md 5z-zk. Since F226, Bader's answer to Q208, a picture that draws no item
  of its clash is taken again through the active view from the clash's own camera moved back to 30 ft,
  its two items red and green and the other models hidden, and no Image cell shows only the sky. Each
  group writes one RETAKE line with the count retaken and the count still empty. F229's run of C06 and
  C07 retook 57 pictures, 45 drawing their items, and left 0 of only the sky.

## The NWD in ACC, and why nothing appears beside the NWF

The NWD is the file that goes up. Every NWD this tool published before F51 showed a processing error
beside it in ACC and in Forma, because an NWD published without May be re-saved cannot be translated by
that viewer. Since F51 the publish sets `AllowResave` true, and it sets `EmbedDatabaseProperties` true
and `PreventObjectPropertyExport` false with it, which is the second thing Autodesk describe: an NWD
that reaches ACC with no properties, every object showing as solid. All three were already on the list
read off the installed DLL on 2026-08-29.

Every publish property this run set goes in the log on one line, because an NWD that will not open is
diagnosed months later off the log and the file, by someone who cannot read the build that wrote it.

Nothing appears beside the NWF up there and nothing is wrong. ACC does not translate an NWF at all. An
NWF holds no geometry, only pointers to the NWC files, so there is no viewable file to make from one and
no translation to succeed or fail. Whether the NWF needs to be in ACC at all is Q32.

## The NWF is the record

The NWD is the picture and the NWF is the ledger. Everything anyone has decided about a clash lives in
the NWF and nowhere else. There is no second copy on disk, none in the NWD, and none in the workbook,
which is a report of what the NWF held at the moment it was written.

The NWF carries five things:

    the file list      pointers to the NWC files, not copies
    the sets           the selection sets every clash test side points at
    the tests          the clash tests themselves
    the results        every clash, with the status a person set on it
    the viewpoints     one view per clash test of its open clashes, since F114, beside a person's own

A refresh on the NWF page touches none of that. It opens the NWF, lets Navisworks reload the newer
NWCs, runs the tests where they sit and saves. Nothing is cleared and nothing is at risk.

A group whose NWF points at a different file list than the scan is brought up to date by the
engine's reshape. It takes out what is gone and appends what is new without clearing, because
removing a model was measured to cost nothing, below. Only a shape the reshape declines is cleared
and rebuilt, and then each of the five is counted before the clear, counted after the appends, and
counted again after the saved copy is put back. The NWF on disk is saved over only when every one of
them came back. One short and the NWF is left exactly as it was, the group fails, and the log says
which of them went and how many. Bader decided this on 2026-09-07, Q22. The plan is
`Federator.Core.Rerun.NwfRebuildPlan`. Since the NWC page puts a rebuild in a dated folder, whether a
window press now reaches this path is UNKNOWN.

A count that could not be taken is not a count of zero. It reports as not counted and it holds the NWF
shut in the same way, because a rebuild that could not see the viewpoints must not report that it kept
them.

A model can be removed from an open document without a clear, and it costs nothing. Measured on
2026-09-19 and 2026-09-20 in docs\history\scan.md 5c and 5x: `Document.RemoveFile(int)` and
`TryRemoveFile(int)` exist, and removing a model from 1A02MM left the sets at 62, the tests at 1830,
the results at 526, the statuses at 526 and the viewpoints at 509, and every one came back whole
through a save and a reopen. It was the last model of four that was removed. Whether removing a
middle model shifts the indexes after it is UNKNOWN, so the tool removes a file by its name and from
the end. Q34.

## The saved viewpoints, one view per clash test

Since F114 every federation this tool writes carries one saved view per clash test of its open
clashes, the New and Active ones, in the Saved Viewpoints panel, in folders by the test's priority and
then its discipline. Clash Detective is where one clash is looked at close up, and the pictures in the
report are how one clash is shown there.

The tree, outermost first, since F186, F192, F198 and F199:

    Clash Status                         the tool's one folder, deleted and made again each run
      A, B or C                          the priority the clash matrix gives the test, none being C
        Architecture, Structure,         the discipline of the test's first side, made only
        Mechanical, Electrical or Other  where a test is in it
          BLD-ME-Ducts-vs-BLD-AR-Walls   the test, named exactly as Clash Detective names it
      C
        Mechanical or Electrical
          Over 150mm                     inside Mechanical and Electrical of C only
            BLD-ME-Ducts-vs-BLD-AR-Walls the same test's clashes whose larger service is over 150 mm

There is no team pair level. Everything outside the tool folder is deleted on a run that makes
viewpoints, except a folder named Comments, whatever its case, which is kept with all inside it and
made empty where there is none, F191. The folder was named NWC Federator (auto) until F198, and one of
that name an earlier run left is deleted as outside the tool folder.

The discipline of a test, F199, is decided in one place, `DisciplineFolders.DisciplineFolderFor`, off
the code of its first side. In the pack every test is named its left set, -vs-, its right set, so the
first side is its left set:

    AR, Architecture                     Architecture
    ST, Structure                        Structure
    ME, MEP, Mechanical, PL, FP, FF      Mechanical
    EL, Electrical                       Electrical
    HV, DR                               Mechanical, by the pack's team line, Q184
    EV                                   Electrical, by the pack's team line, Q184
    anything else, no code at all        Other, BLD-Security Devices among them, Q184
    a saved test the pack does not name  Other

The 1830 tests of the pack 1104-PAR go 120 to Architecture, 111 to Structure, 850 to Mechanical, 692
to Electrical and 57 to Other, read off the pack by a test. The 284 tests the rule of two set codes
leaves out, above, are in that count and have no view, since no run makes them.

Where a clash goes, by example:

- a duct of 600 by 150 against a wall, test BLD-ME-Ducts-vs-BLD-AR-Walls, priority C: in
  `C / Mechanical / Over 150mm / BLD-ME-Ducts-vs-BLD-AR-Walls`, because the larger side of the duct is
  600 and the pair carries the size folder
- a pipe of 100 mm against the same wall, same test: in `C / Mechanical / BLD-ME-Ducts-vs-BLD-AR-Walls`,
  with every clash of the test that is not over 150 mm
- the same duct in a test of priority A or B: in the test's one view in `A / Mechanical` or
  `B / Mechanical`. There is no Over 150mm folder there, and the log counts these clashes for Bader's
  decision, Q182
- the same duct in a test of C whose first side is Structure, BLD-ST-Columns-vs-BLD-ME-Ducts: in its
  one view in `C / Structure`. There is no Over 150mm folder in Architecture, Structure or Other, so no
  clash is dropped and the log counts them, Q185
- a fitting whose size could not be read: in the test's main view, never under Over 150mm, and named
  in the log's VIEWS block
- a test the priority file says nothing about: under `C`
- a clash Reviewed, Approved or Resolved: in no view, and counted in the VIEWS block
- a mirrored test, the same two sets the other way round: no view of its own. A mirror whose clashes
  the merge did not take keeps its rows under its own name and its open clashes are in no view. A clash
  only a mirror found, where the merge took it, is a row of the kept test and is viewed under it.
  Whether the open clashes of a mirror not merged should go into the kept test's view is Bader's, and
  no rule says so

Every view shows only the models its clashing items live in and hides the rest, dims the shown models
and leaves every clashing item solid, the first item of each clash red and the second green, and is
framed on all its clashes at once, or on Clash Detective's own camera for a view of one clash.

Every view and folder the tool makes carries one comment: a sentence saying it is the tool's and
replaced on the next run, and a line no person would type. The top folder carries
NWCF-AUTO run=<id>. Each run finds the top folder of the last run by its name and that marker and
deletes it with everything in it, a view a person moved into it included, then builds the tree again,
F186. Once the new tree is built, everything outside it is deleted except the Comments folder, F191.
Put your own viewpoints in Comments.

The log carries a VIEWS block with the plan, a VIEWS BUILT block with one line per view and the
totals, a VIEWS INVENTORY block saying what was kept and why and what went, and a VIEWS TREE block with
the tree as the document holds it after the run and seven checks on it. A failed check is a FAILED
line, and the group keeps its own result. A group whose views failed to write or read back is not
reported as DONE.

The box `Make saved viewpoints for the clashes` on the Clash step opens ticked. Unticked, no view is
made and the log says so in every group. On the NWF page the Viewpoints step decides it for that run.

## The 150 mm rule and the sub groups

Pipes, ducts, cable trays and their fittings over 150 mm are in the viewpoints. Smaller ones are not.
The large ones of Mechanical and Electrical sit in a sub folder of their own, Over 150mm, and since
F192 and F199 that folder exists inside Mechanical and Electrical of C only:

    Clash Status
      C
        Mechanical
          Over 150mm
            BLD-ME-Ducts-vs-BLD-AR-Walls   the test's clashes over 150 mm
        Electrical
          Over 150mm
            BLD-EL-Cable Trays-vs-BLD-AR-Walls

The sub folder is named from the threshold, so the folder and the rule that fills it can never
disagree. Which disciplines get a sub group is not a setting. The two are written in one place,
`DisciplineFolders.HoldsTheSizeFolder`, and they are Mechanical and Electrical, only under the
priority C. A project that codes its disciplines differently reaches them through the code of the
test's first side and the pack's team line, not through a setting of this folder.

150 is a setting, in millimetres, and over 150 means over: exactly 150 is out. Which properties carry a
size is a setting too: Diameter, Width, Height, Size, Nominal Diameter and Overall Size. Every one the
item carries is read and the largest is taken. Which one holds the size differs per kind and per
exporter, so reading one name would quietly drop every item that calls it something else.

The number a property gives back is in the document's units and is converted to millimetres before
anything is compared. A document in feet reporting 0.5 is 152.4 mm and is in. Comparing 0.5 against 150
would put it out, and the same model measured in millimetres would put it in, so one building would
produce two different sets of viewpoints depending on a setting nobody changed.

**Anything whose size cannot be read is IN.** A fitting usually carries no size property at all.
Leaving it out would mean a run quietly drops real geometry from a viewpoint with nothing in the output
to say it happened. So it goes in, and the SIZE block says how many did, says plainly that nothing was
dropped, and names every one of them. That number is expected to be large and it is the one most likely
to say the rule needs changing.

## What is UNKNOWN

Each of these is said UNKNOWN in the code or the rules and waits for a run or for Bader.

- Whether a Navisworks the person started can start the runner's Navisworks, and what a fresh start
  and quit of each group costs on a whole community. The F222 steps in steps\03_bader_next.md are the
  proof.
- Whether any press of today's window ends in the Rebuilt label or the reshape, and what the Run as
  column and the log read for a group built again by Rebuild.
- What happens to the indexes after a model removed from the middle, Q34.
- Whether the append of a large real NWC opens a message box the tool would press, F213-R1.
- Whether the Auto-Save option set off in a session started with it on stops that session's autosaves,
  and what Navisworks writes into the registry as it quits, F223-R1. Why no autosave file was written
  in the measured runs.
- Whether Hide Other draws the pictures as asked, and why some pictures draw no item of their clash, which
  F226 takes again.
- Whether the memory stop still fires on a community of 21 groups when each group has its own Navisworks.
- What puts a test into Old.
- Whether the NWF belongs in ACC at all, Q32.
- Whether the Close NWC/NWD files on load option can be set from the tool. The NWF page tells the user
  to set it by hand.
- Whether the tests a rule left out that an NWF still holds are left, deleted or moved, Q212, pending.
- Whether the clash counts of the Excel equal Clash Detective's own panel counts on the C06 and C07
  runs of F220. The panel's counts were not read against the workbooks.
