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
