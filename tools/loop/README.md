# tools\loop

The scripts the loop runs on Bader's machine. The rules they keep are in
.claude\rules\loop.md. Each is run from the repo root with
powershell -ExecutionPolicy Bypass -File tools\loop\<name>.ps1

## prepare-copy.ps1

The only thing that reads NM Fed, the folder of real NWC files on Bader's desktop. The
wall in .claude\hooks refuses any other command that names it.

- reads: NM Fed, found as [Environment]::GetFolderPath('Desktop') plus NM Fed, because the
  desktop sits under OneDrive
- writes outside the repo: %LOCALAPPDATA%\NwcFederatorLoop\source, a copy of NM Fed with
  every file and every folder, empty ones included. It is kept while every file matches NM
  Fed by relative path and size, and made again from nothing when one does not
- deletes: only inside %LOCALAPPDATA%\NwcFederatorLoop\source, when the copy no longer
  matches, and one NWC there for the run that loses a file
- writes inside the repo: the NM Fed listing named by -Listing, one line per file with its
  size and parts 3 and 5 of its name, and the sha256 of every file that is not an NWC
- never writes into NM Fed

## read-workbook.ps1

Reads one clash workbook back off the disk and writes a text read-out: one line per test
block with its tolerance and its seven counts, one line per clash row with its status,
distance, grid location, clash point and picture link, and the totals. It shares no code
with the tool that wrote the workbook. It opens the xlsx as a zip and reads the XML, and it
finds a block by its heading cells, never by a row number.

- reads: the workbook named by -Workbook
- writes: the read-out named by -Out, inside the repo under steps\runs
- writes outside the repo: nothing

Proved on 2026-09-27 against the two client exports in samples\client-report, read only.
1A02WN reads 1830 test blocks with 1830 distinct names and 64 clash rows, and the block
headings add to 64 clashes, all New. 1A04WN reads 1830, 1830 and 65, adding to 65.

## run.ps1

NOT WRITTEN YET. It starts a run with no click, keeps the machine awake while it goes,
reads the log while it grows, calls a run hung only when the log has not grown and that
Navisworks has used no processor time for five minutes, records any dialog Navisworks
raises, and closes only the Navisworks it started, by its process id. How the installed
Navisworks can start a run with no click is Phase 1 of the loop and is measured by the
prober first, so this script is written after that answer and not before it.
