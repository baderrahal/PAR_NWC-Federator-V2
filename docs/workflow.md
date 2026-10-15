# The two workflows

Confirmed by Bader on 2026-09-07. The window shows which one each group will take, the
confirm dialog counts them, and the log carries them. The rule is
`Federator.Core.Rerun.RunPath` and it changes what the person is told, never what the
engine does.

## First run

The NWC folder is scanned and grouped, output folders picked, Run pressed. Per group the
tool builds the NWF, builds the sets, adds and runs the tests of the clash matrix built
into the tool, writes Excel, HTML, images and the NWD. Each project has a built in matrix
pack that Bader supplies, F155, so no XML is picked.

## Weekly run

The same NWC folder, Run pressed. Per group the tool finds the NWF already there, opens
it (OPENED), lets Navisworks reload the newer NWCs, runs the tests of the built in matrix
saved inside the NWF where they sit, keeping their results, writes Excel, HTML, images and
the NWD. Since F155 the matrix is read on every run, so a test of it the NWF lacks is added
and the label reads Weekly run plus matrix. A test saved in the NWF that the matrix does not
name is run too, where it sits, after the tests of the matrix, at the tolerance of the drop
down, and it is in the HTML, the Excel and the Coverage sheet, its viewpoints under priority C
unless the priority map names it, F168, Q168 A. With no pack built in, every saved test runs.

## Rebuilt

The NWF is there and points at a different file list than the scan. It is cleared and
rebuilt from the scan folder, the sets and the tests saved inside it are kept, and from
there it is a Weekly run: units, the clash step, the reports, the NWD. Bader decided this
on 2026-09-07, Q22. The plan is `Federator.Core.Rerun.NwfRebuildPlan`.

## The labels

One per group in the Run as column, counted in the confirm dialog, carried in the log.

    First run                  no NWF at the output path yet
    Weekly run                 the NWF is there and no matrix is built in
    Weekly run plus matrix        the NWF is there and the built in matrix is read,
                               every weekly run since F155
    Rebuilt                    the NWF is there and points at a different file list.
                               Only known once the NWF is opened, so the window opens
                               each NWF at Run where nothing open would be lost, and
                               otherwise the run settles it and the list shows it after
    Skipped (changed on disk)  a group whose file list differed and whose rebuild did
                               not finish, so it was left alone
    Unknown                    the NWF folder is not picked yet, or a case the rule
                               cannot name

Three rules in `RunPath` give them. `Expected` is the label before any NWF is opened,
from whether the NWF is already at its output path, so Rebuilt never appears there.
`AfterOpening` is the label once the NWF has been opened and compared but not yet run,
where a comparison that reads Changed shows as Rebuilt, because that is what the person
is about to get. `Label` is the label once the group has ended, read off the Decide
result and whether a matrix was read.

## The open file

A federation that already exists needs no scan. The Run the open file button on the
Clash step runs the document that is open, names the open file and the two paths it
will write, and is the same flow with the first step removed. There is no Decide,
because the document is the file list, nothing is appended and nothing is cleared, so
the clash results inside it survive. It is always a Weekly run, or a Weekly run plus
XML. There is no First run on that path, because the NWF already exists and is the
document. Five things are checked before it runs and the first that fails is named in
the window and the log, in `Federator.Core.Rerun.OpenDocumentJob`.

## The two hand buttons

Sets into open model and Tests into open model, on the Clash step, are for trying one
open model by hand and are labelled as that. They are not steps in the run. Each calls the one engine method
the run calls per group, so the SETS and CLASH lines in the log read the same whichever
way the work was started.

## The NWD in ACC, and why nothing appears beside the NWF

The NWD is the file that goes up. Every NWD this tool published before F51 showed a
processing error beside it in ACC and in Forma, because an NWD published without
May be re-saved cannot be translated by that viewer. Since F51 the publish sets
`AllowResave` true, and it sets `EmbedDatabaseProperties` true and
`PreventObjectPropertyExport` false with it, which is the second thing Autodesk describe:
an NWD that reaches ACC with no properties, every object showing as solid. All three were
already on the list read off the installed DLL on 2026-08-29.

Every publish property this run set goes in the log on one line, because an NWD that will
not open is diagnosed months later off the log and the file, by someone who cannot read
the build that wrote it.

Nothing appears beside the NWF up there and nothing is wrong. ACC does not translate an
NWF at all. An NWF holds no geometry, only pointers to the NWC files, so there is no
viewable file to make from one and no translation to succeed or fail. Whether the NWF
needs to be in ACC at all is Q32.

## The NWF is the record

The NWD is the picture and the NWF is the ledger. Everything anyone has decided about a
clash lives in the NWF and nowhere else. There is no second copy on disk, none in the
NWD, and none in the workbook, which is a report of what the NWF held at the moment it
was written.

The NWF carries five things:

    the file list      pointers to the NWC files, not copies
    the sets           the selection sets every clash test side points at
    the tests          the clash tests themselves
    the results        every clash, with the status a person set on it
    the viewpoints     one view per clash test of its open clashes, since F114, beside a person's own

A Weekly run touches none of that. It opens the NWF, lets Navisworks reload the newer
NWCs, runs the tests where they sit and saves. Nothing is cleared and nothing is at risk.

A Rebuilt run is the one that has to be careful. The file list changed, so the document is
cleared and appended again from the scan, and a clear takes everything above with it. So
each of the five is counted before the clear, counted after the appends, and counted again
after the saved copy is put back. The NWF on disk is saved over only when every one of
them came back. One short and the NWF is left exactly as it was, the group fails, and the
log says which of them went and how many.

A count that could not be taken is not a count of zero. It reports as not counted and it
holds the NWF shut in the same way, because a rebuild that could not see the viewpoints
must not report that it kept them.

Whether a model can be removed from an open document without a clear at all, which would
make all of this unnecessary, is UNKNOWN. It has never been read off the installed DLL.
`tools\probes\probe-model-remove.ps1` answers it and Q34 is what happens when it does.

## The saved viewpoints, one view per clash test

Since F114 every federation this tool writes carries one saved view per clash test of its
open clashes, the New and Active ones, in the Saved Viewpoints panel, in folders by the
test's priority and then its discipline. Clash Detective is where one clash is looked at
close up, and the pictures in the report are how one clash is shown there.

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
viewpoints, except a folder named Comments, whatever its case, which is kept with all inside it
and made empty where there is none, F191. The folder was named NWC Federator (auto) until F198,
and one of that name an earlier run left is deleted as outside the tool folder.

The discipline of a test, F199, is decided in one place, `DisciplineFolders.DisciplineFolderFor`,
off the code of its first side. In the pack every test is named its left set, -vs-, its right
set, so the first side is its left set:

    AR, Architecture                     Architecture
    ST, Structure                        Structure
    ME, MEP, Mechanical, PL, FP, FF      Mechanical
    EL, Electrical                       Electrical
    HV, DR                               Mechanical, by the pack's team line, Q184
    EV                                   Electrical, by the pack's team line, Q184
    anything else, no code at all        Other, BLD-Security Devices among them, Q184
    a saved test the pack does not name  Other

The 1830 tests of the pack 1104-PAR go 120 to Architecture, 111 to Structure, 850 to
Mechanical, 692 to Electrical and 57 to Other, read off the pack by a test.

Where a clash goes, by example:

- a duct of 600 by 150 against a wall, test BLD-ME-Ducts-vs-BLD-AR-Walls, priority C: in
  `C / Mechanical / Over 150mm / BLD-ME-Ducts-vs-BLD-AR-Walls`, because the larger side of the
  duct is 600 and the pair carries the size folder
- a pipe of 100 mm against the same wall, same test: in
  `C / Mechanical / BLD-ME-Ducts-vs-BLD-AR-Walls`, with every clash of the test that is not over
  150 mm
- the same duct in a test of priority A or B: in the test's one view in `A / Mechanical` or
  `B / Mechanical`. There is no Over 150mm folder there, and the log counts these clashes for
  Bader's decision, Q182
- the same duct in a test of C whose first side is Structure, BLD-ST-Columns-vs-BLD-ME-Ducts:
  in its one view in `C / Structure`. There is no Over 150mm folder in Architecture, Structure or
  Other, so no clash is dropped and the log counts them, Q185
- a fitting whose size could not be read: in the test's main view, never under Over 150mm, and
  named in the log's VIEWS block
- a test the priority file says nothing about: under `C`
- a clash Reviewed, Approved or Resolved: in no view, and counted in the VIEWS block
- a mirrored test, the same two sets the other way round: no view of its own, its clashes
  are in the kept test's view

Every view shows only the models its clashing items live in and hides the rest, dims the
shown models and leaves every clashing item solid, the first item of each clash red and
the second green, and is framed on all its clashes at once, or on Clash Detective's own
camera for a view of one clash.

Every view and folder the tool makes carries one comment: a sentence saying it is the
tool's and replaced on the next run, and a line no person would type. The top folder carries
NWCF-AUTO run=<id>. Each run finds the top folder of the last run by its name and that marker
and deletes it with everything in it, a view a person moved into it included, then builds the
tree again, F186. Once the new tree is built, everything outside it is deleted except the
Comments folder, F191. Put your own viewpoints in Comments.

The log carries a VIEWS block with the plan, a VIEWS BUILT block with one line per view
and the totals, a VIEWS INVENTORY block saying what was kept and why and what went, and a
VIEWS TREE block with the tree as the document holds it after the run and seven checks on
it. A failed check is a FAILED line, and the group keeps its own result. A group whose
views failed to write or read back is not reported as DONE.

The box `Make saved viewpoints for the clashes` on the Clash step opens ticked. Unticked,
no view is made and the log says so in every group.

## The 150 mm rule and the sub groups

Pipes, ducts, cable trays and their fittings over 150 mm are in the viewpoints. Smaller
ones are not. The large ones of Mechanical and Electrical sit in a sub folder of their own,
Over 150mm, and since F192 and F199 that folder exists inside Mechanical and Electrical of C
only:

    Clash Status
      C
        Mechanical
          Over 150mm
            BLD-ME-Ducts-vs-BLD-AR-Walls   the test's clashes over 150 mm
        Electrical
          Over 150mm
            BLD-EL-Cable Trays-vs-BLD-AR-Walls

The sub folder is named from the threshold, so the folder and the rule that fills it can
never disagree. Which disciplines get a sub group is a setting, because ME and EL are
codes this project uses and another project may code its disciplines differently.

150 is a setting, in millimetres, and over 150 means over: exactly 150 is out. Which
properties carry a size is a setting too, tried in order, Diameter then Width, Height,
Size, Nominal Diameter and Overall Size. Which one holds the size differs per kind and per
exporter, so reading one name would quietly drop every item that calls it something else.

The number a property gives back is in the document's units and is converted to
millimetres before anything is compared. A document in feet reporting 0.5 is 152.4 mm and
is in. Comparing 0.5 against 150 would put it out, and the same model measured in
millimetres would put it in, so one building would produce two different sets of
viewpoints depending on a setting nobody changed.

**Anything whose size cannot be read is IN.** A fitting usually carries no size property at
all. Leaving it out would mean a run quietly drops real geometry from a viewpoint with
nothing in the output to say it happened. So it goes in, and the SIZE block says how many
did, says plainly that nothing was dropped, and names every one of them. That number is
expected to be large and it is the one most likely to say the rule needs changing.

## What a status means, and which ones this tool sets

Two different sets of words get confused because they share one of them.

A CLASH carries one of five:

    New        it has not been looked at
    Active     somebody is dealing with it
    Reviewed   somebody looked at it and moved on
    Approved   somebody with the authority to accept it accepted it
    Resolved   it is gone from the model

A TEST carries one of four:

    New   Old   Partial   Complete

They are different things. The only word they share is New. **Old is a test word and never
a clash word**, so no clash is ever at Old and nothing in this tool ever moves one from it.
What puts a test into Old is UNKNOWN and is not guessed at either, so the test status is
written in the report exactly as the API reports it.

This tool sets **Reviewed** and nothing else. A clash that cannot be solved should not sit
at New or Active forever, and Reviewed says it was looked at and passed over.

It never sets Approved or Resolved. Both are a person's statement about work that was
actually done, the NWF is the only record of what has been fixed, and there is nothing to
check either claim against afterwards. It does not set New or Active either, because
running a test produces them and setting one by hand would overwrite a decision somebody
had already made.

A status is applied between the run and the report, never after, so the workbook and the
page carry the new status and not the one read before the change. Writing a status is a
write, so the NWF is saved again on it.

**How the tool learns which clashes cannot be solved was Q33 and is ANSWERED**, on
2026-09-19. It is a rule over the clash itself, and it is called a PENETRATION.

A clash becomes Reviewed when all four of these are true:

    one side is a SERVICE by item category
        Pipes, Pipe Fittings, Pipe Accessories, Ducts, Duct Fittings, Duct Accessories,
        Flex Pipes, Flex Ducts, Cable Trays, Cable Tray Fittings, Conduits, Conduit Fittings
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

Everything else is left exactly as it is and counted by reason, and the reasons are in a
PENETRATION block in the log with one line per clash moved and one line per reason nothing
moved. Both lists and the 150 are settings. The 150 is the same number the viewpoints read,
named once, and the two read it in opposite directions: a viewpoint takes an item OVER it
and a penetration takes a service AT OR UNDER it.

It is OFF by default, because it writes into the NWF. A service whose size cannot be read
is LEFT ALONE, which is the opposite of what the viewpoints do with the same unknown, and
that is deliberate: the safe mistake in a viewpoint is showing something unnecessary, and
the safe mistake here is leaving a clash for a person to look at.
