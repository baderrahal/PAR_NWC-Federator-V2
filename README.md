# Parsons NWC Federator

Navisworks Manage 2025 add-in. Reads a folder of discipline NWC files, gathers them
into federations, saves an NWF and an NWD per federation, runs the clash tests, and
writes one report per federation.

The rules the code keeps are in `CLAUDE.md`. The worker's notes are in `steps`.

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
    Skipped (changed on disk)  a group whose file list differed and whose rebuild
                               did not finish, so it was left alone
    Unknown                    the NWF folder is not picked yet, or a case the rule
                               cannot name

Run the open file is always a Weekly run, or a Weekly run plus XML. There is no First
run on that path, because the NWF already exists and is the document.

