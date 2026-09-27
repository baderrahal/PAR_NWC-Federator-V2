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
- -Listing writes one .txt INSIDE THE REPO, one line per file with its size, its sha256
  and parts 3 and 5 of its name, and touches nothing else
- -Remove takes one NWC out of a whole copy and writes its sha256 down in
  source.removed.txt. -Restore copies that same file back from NM Fed, refuses when NM Fed
  now holds a different file under that name, and moves it into place only once its hash
  matches
- deletes: only inside %LOCALAPPDATA%\NwcFederatorLoop
- never writes into NM Fed, and refuses a work folder that overlaps it or is a junction

Proved on 2026-09-27 on the real folder: made, kept, made again when NM Fed changed,
listed without touching the copy, one NWC removed and a listing taken with it out,
restored by hash, kept again, and eight calls refused with their reason and nothing
changed.

## read-workbook.ps1

Reads one clash workbook back off the disk and writes a text read-out: one line per test
with its shape, row, tolerance, seven counts, type, status and clash rows, one line per
clash row with its status, distance, grid location, clash point and picture link, the
totals, and every doubt it had. It shares no code with the tool that wrote the workbook.
It opens the xlsx as a zip, reads the XML, and knows a test by column A in both shapes a
workbook holds, the full block and the one row test this tool writes for a test that
found nothing.

- reads: the workbook named by -Workbook
- writes: the read-out named by -Out, a .txt inside the repo and never the workbook
  itself, written beside and moved into place, or a READ-OUT FAILED line when it fails
- writes outside the repo: nothing

Proved on 2026-09-27, read only, on both client exports in samples\client-report, 1830
tests each with 64 and 65 clash rows equal to their own totals, and on a workbook written
by this tool's own WorkbookWriter holding two full blocks and two one row tests, read as
four tests and five clash rows with no doubt.

## prove-hooks.sh

Feeds every case to the three hooks on standard input and prints each answer against the
one it should give. sh tools/loop/prove-hooks.sh <hooks folder> <repo root>. It clones the
repo into a temp folder to prove the git wall with main checked out, and removes it.

## hooks-next

The three hooks as rewritten after the Phase 0 review, WAITING to be proved and read a
second time before they replace the ones in .claude\hooks. Nothing runs them from here.

## run.ps1

NOT WRITTEN YET. It starts a run with no click, keeps the machine awake while it goes,
reads the log while it grows, calls a run hung only when the log has not grown and that
Navisworks has used no processor time for five minutes, records any dialog Navisworks
raises, and closes only the Navisworks it started, by its process id. How the installed
Navisworks can start a run with no click is Phase 1 of the loop and is measured by the
prober first, so this script is written after that answer and not before it.
