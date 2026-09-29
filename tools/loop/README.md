# tools\loop

The scripts the loop runs on Bader's machine. The rules they keep are in
.claude\rules\loop.md. Each PowerShell script is run from the repo root with
powershell -ExecutionPolicy Bypass -File tools\loop\<name>.ps1

## prepare-copy.ps1

The only thing that reads NM Fed, the folder of real NWC files on Bader's desktop. It finds
the folder itself, so no command ever names it, and the wall in .claude\hooks refuses any
command that does.

- reads: NM Fed, found as [Environment]::GetFolderPath('Desktop') plus NM Fed, because the
  desktop sits under OneDrive. It refuses a file OneDrive holds online only, because
  reading one downloads it into NM Fed
- writes outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\source, a copy of NM Fed with
  every file and every folder, and source.manifest.txt beside it once every copied file
  has been hashed and matched. The copy is kept while it matches NM Fed on every file by
  exact name, size and sha256 and on every folder, and made again when it does not, after
  the room is checked and never before
- -Listing writes one .txt under steps\runs, one line per file with its size, its sha256
  and parts 3 and 5 of its name, and touches nothing else
- -Remove takes one NWC out of a whole copy and writes its sha256 down in
  source.removed.txt. -Restore copies that same file back from NM Fed, refuses when NM Fed
  now holds a different file under that name, and moves it into place only once its hash
  matches. While a file is out, the plain command refuses rather than remaking the copy,
  which would put the file straight back
- it walks NM Fed one folder at a time and refuses a junction or a link inside it, because
  Windows PowerShell 5.1 follows one when it recurses
- deletes: only inside %LOCALAPPDATA%\NwcFederatorLoop
- never writes into NM Fed, and refuses a work folder that overlaps it or is a junction

Proved on the real folder: on 2026-09-27 made, kept, and made again when NM Fed changed
under it, and on 2026-09-28 in this form kept, listed without touching the copy, one NWC
removed, restored by hash and kept again, with twelve calls refused with their reason and
nothing changed, among them the plain command while a file is out, a file that is not an
NWC, a wildcard, and a listing outside the repo, outside steps\runs, over steps\logs or
not a .txt.

## read-workbook.ps1

Reads one clash workbook back off the disk and writes a text read-out: one line per test
with its shape, row, tolerance, clash count, five status counts, type, status and clash
rows, one line per clash row with its status, distance, grid location, clash point and
picture link, the totals, the pictures stored in the workbook and those in the sheet that
point at a file outside it, and every doubt it had. It
shares no code with the tool that wrote the workbook. It opens the xlsx as a zip, reads
the XML, and knows a test by column A in both shapes a workbook holds, the full block and
the one row test this tool writes for a test that found nothing, and by its numbers in C
to K when A is empty. A test or a clash row with no name, a sheet with no test and a
workbook with no sheet are each written down as a doubt, so an empty read-out never
looks like a clean one.

- reads: the workbook named by -Workbook
- writes: the read-out named by -Out, a .txt under steps\runs and never the workbook
  itself, written beside and moved into place, or a READ-OUT FAILED line when it fails,
  a missing workbook included
- writes outside the repo: nothing

Proved on 2026-09-27 and 2026-09-28, read only, on both client exports in
samples\client-report, 1830 tests each with 64 and 65 clash rows equal to their own
totals, on a workbook written by this tool's own WorkbookWriter holding two full blocks
and two one row tests, read as four tests and five clash rows with no doubt, and on the
same shape with one clash and one one row test left without a name, read as four tests
and five clash rows with both named as doubts.

## compare-document.ps1

Sets a workbook's read-out beside the read-out of the document it was written from, F104,
Bader's criterion 3. The workbook side is read-workbook.ps1's read-out, the document side
is what tools\probes\DocumentReadProbe writes, and only those two text files pass between
the three, so none of them shares code with the harvest that wrote the workbook. Tests are
paired by name, exactly and never trimmed. For every pair it checks the rows against the
top level results, Clashes against every clash under them, each status count, every row's
name and status in order, the tolerance unit and number in metres, every plain clash's
distance in metres, the picture links and their numbering, the block order, and the
totals. Every finding is a DIFFERS, NOT COMPARED or DOUBT line, and the verdict is AGREE
only when there are none at all. A count of -1 in the document read-out is NOT COMPARED,
never zero. The test type and status are shown and never judged.

- reads: the two read-outs named by -Workbook and -Document, and whether each picture the
  workbook links is on disk, when the workbook's folder is on this machine
- writes: the comparison named by -Out, a new .txt under %LOCALAPPDATA%\NwcFederatorLoop,
  written beside and moved into place and never written over. A read-out that is not
  whole, a column word it reads that is missing, or an -Out it may not write gives one
  COMPARISON FAILED line and exit 1
- writes outside the repo: only that one comparison file
- -PictureStatuses and -PictureCap say which rows should carry a picture, as the run was
  told. Without -PictureStatuses which rows should carry one is NOT COMPARED. With
  -PriorityPicked the block order is chosen by the priority file and is not judged
- -GroupClashesAt says how a group's clashes are counted in New to Resolved and in the
  totals: own, each at its own status as the document holds them, group, all at the
  group's status as the harvest files them, ClashHarvest.cs 166 and ClashReportModel.cs
  396, or unknown, the default. Which the Clash Detective panel shows is PQ4 of F104 and
  UNMEASURED. Under unknown a status where the two readings give the same count is
  compared exactly, and one where a group's clashes sit at more than one status is NOT
  COMPARED when the workbook fits one reading, in one line naming the test and PQ4, and
  DIFFERS when it fits neither
- an empty group holds no clash and the harvest counts it as one, ClashHarvest.cs 164, so
  it reads DIFFERS, and the Clashes line and the line of its status name it as empty
- a test name on two workbook blocks leaves its own pictures unchecked, in one NOT
  COMPARED line saying how many tests, and holds the next pictured block's test number to
  the range the copies allow, NOT COMPARED for that test when it falls inside and DIFFERS
  when it falls outside. Its clash numbers are still checked, and every block after it
  is exact again
- the picture naming and the block order are restated in the script on purpose, a second
  copy by design, because a check that shares the writer's code proves nothing

Proved on 2026-09-29 by prove-compare.ps1 below, and on a read-out of the client export
1104-PAR-1A02WN in samples\client-report, made by an unchanged copy of read-workbook.ps1
run from a folder under the work folder, which read all 1830 tests with no doubt, counted
the 1807 with every number zero and read the other 23 blocks as never rising. No document
read-out of a real NWF exists yet, because the probe waits on F103.

## prove-compare.ps1

Feeds compare-document.ps1 the hand written pair in tools\loop\compare-proof and sixteen
broken copies of it, in prove-hooks.sh's shape, and prints each answer against the one it
should give. The pair is three tests in the shapes the two read-outs really have: plain
clashes with two pictures, a group whose clashes sit at two statuses beside a plain clash
under a name that ends in a space, and a test that found nothing, in a document in feet.
The workbook is what WorkbookWriter writes for that document, the group's clashes filed
under the group's status. No workbook, NWF or picture is behind it, so under the default
-GroupClashesAt unknown the good pair reads DISAGREEMENTS 0 and NOT PROVED, with the mixed
test's New and Reviewed, the same two in the totals, and the pictures on disk NOT
COMPARED. Each copy is one edit away from the good pair, and has to produce exactly its
own lines, no other line the good pair does not have, and lose only the lines of the good
pair written beside it. An edit that does not find exactly the line it edits is WRONG.
Then one case for each of the three readings of a group's clashes, an empty group, a test
name on two blocks with the picture numbering after it, and four runs that prove the
switches and the one class of test that is counted rather than judged. Actions runs it on
every pull request.

- reads: tools\loop\compare-proof
- writes outside the repo: one new folder under %LOCALAPPDATA%\NwcFederatorLoop, by
  default proof\compare-<stamp>, or the one -Work names, which must not be there yet. It
  holds every copy and every comparison and is never emptied or reused
- exits 0 when all 26 cases are right and 1 otherwise

Proved on 2026-09-29 on this machine in Windows PowerShell 5.1, 26 right and 0 wrong, the
copies under %LOCALAPPDATA%\NwcFederatorLoop\turn3\f104. Its first run was 18 right and 3
wrong, all three the harness counting the line NOT COMPARED 1 as a finding, which is how
the harness came to leave out a count line. After the review of the same day the good
workbook became what WorkbookWriter really writes, and the three readings, the empty group
and the name on two blocks were added.

## prove-hooks.sh

Feeds every case to the three hooks on standard input and prints each answer against the
one it should give. sh tools/loop/prove-hooks.sh <hooks folder> <repo root>. It clones the
repo into a temp folder to prove the git wall with main checked out, and removes it.

How a change to a wall is proved with it and copied in is in .claude\rules\loop.md.

## run.ps1

NOT WRITTEN YET. It starts a run with no click, keeps the machine awake while it goes,
reads the log while it grows, calls a run hung only when the log has not grown and that
Navisworks has used no processor time for five minutes, records any dialog Navisworks
raises, and closes only the Navisworks it started, by its process id. How the installed
Navisworks can start a run with no click is Phase 1 of the loop and is measured by the
prober first, so this script is written after that answer and not before it.

## check-documents.ps1

NOT WRITTEN YET. It comes with F103, because it starts a Navisworks through the guarded
start and close F103 writes as functions run.ps1 shares, loads DocumentReadProbe, has it
read every NWF of a run, checks each NWF's sha256 did not move, and runs
compare-document.ps1 on every NWF and workbook pair. F104's second part.
