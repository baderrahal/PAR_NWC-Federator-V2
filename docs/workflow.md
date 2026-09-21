# The two workflows

Confirmed by Bader on 2026-09-07. The window shows which one each group will take, the
confirm dialog counts them, and the log carries them. The rule is
`Federator.Core.Rerun.RunPath` and it changes what the person is told, never what the
engine does.

## First run

The NWC folder is scanned and grouped, output folders picked, a clash XML picked, Run
pressed. Per group the tool builds the NWF, builds the sets, adds and runs the tests
from the XML, writes Excel, HTML, images and the NWD.

## Weekly run

The same NWC folder, Run pressed with no XML. Per group the tool finds the NWF already
there, opens it (OPENED), lets Navisworks reload the newer NWCs, runs the tests saved
inside the NWF, writes Excel, HTML, images and the NWD. An XML is optional and only adds
or updates tests.

## Rebuilt

The NWF is there and points at a different file list than the scan. It is cleared and
rebuilt from the scan folder, the sets and the tests saved inside it are kept, and from
there it is a Weekly run: units, the clash step, the reports, the NWD. Bader decided this
on 2026-09-07, Q22. The plan is `Federator.Core.Rerun.NwfRebuildPlan`.

## The labels

One per group in the Run as column, counted in the confirm dialog, carried in the log.

    First run                  no NWF at the output path yet
    Weekly run                 the NWF is there and no XML is picked
    Weekly run plus XML        the NWF is there and an XML is picked
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
result and whether an XML was picked.

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
    the viewpoints     one folder per discipline, since F52

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

## A viewpoint per discipline

Every federation this tool writes carries one folder per discipline, named with the
discipline code the scan reads off part 5 of the NWC name, with one saved viewpoint inside
it showing that discipline and hiding the others. They are made after the clash run and
before the NWF is saved again, so a viewpoint this run made is inside the file the NWD is
published from.

A group of one discipline still gets its folder and its viewpoint. It hides nothing,
because there is nothing else in the group to hide, and that is not the same as having no
viewpoint. Every NWF this tool writes then has the same shape in it, and a group that
quietly had none would read as one where the step failed.

A viewpoint already at its path is left exactly as it is and counted as already there. A
second copy at one path would leave the tree holding both, and whichever came first is
what anything resolving that path finds. That is the rule already in force for sets.

The log carries a VIEWS block in the shape the SETS block uses, one line per viewpoint
then the totals, and the counts reach the RESULT block and the judgement, so a group whose
viewpoints failed is not reported as DONE.

This is not the same as saving a clash as a viewpoint, which this tool does not do and has
never done. A discipline viewpoint shows a discipline. A clash viewpoint would show one
clash, and the pictures in the report are how a clash is shown.

**Not working yet, and the log says so.** How a saved viewpoint folder is made was never
read off the installed DLL. Until `tools\probes\probe-viewpoints.ps1` has been run, the run
plans the viewpoints, writes what it would have made into the log, and attempts nothing. A
step this tool cannot do is not a step that failed, so the group is not marked down for it.

## The 150 mm rule and the sub groups

Pipes, ducts, cable trays and their fittings over 150 mm are in the viewpoints. Smaller
ones are not. The large ones of Mechanical and Electrical sit in a sub group of their own,
so the tree reads:

    ME
      ME only
      Over 150mm
        ME over 150mm

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
