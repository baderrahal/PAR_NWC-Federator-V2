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

Build sets and Run tests on the Clash step are for trying one open model by hand and
are labelled as that. They are not steps in the run. Each calls the one engine method
the run calls per group, so the SETS and CLASH lines in the log read the same whichever
way the work was started.
