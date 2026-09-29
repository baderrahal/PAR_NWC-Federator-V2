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

## mask-evidence.ps1

Writes a masked copy of one result file of a run or a probe, so it can be committed without
this machine's name or Bader's Autodesk licensing ids in it. Every such file goes through it
before it is committed, and tools\checks\check-evidence-ids.sh refuses one that did not.
F102.
powershell -ExecutionPolicy Bypass -File tools\loop\mask-evidence.ps1 -In <file> -Out <file>

WHAT IT MASKS is every kind in tools\checks\evidence-ids.txt, the one place the rule lives,
which the check reads too: an analytics agent id, a GUID on a line naming analyticsagentid,
a GUID after -i on any line, a GUID on a line naming AdskLicensing, AdskIdentity or
GenuineService, each to [id], and COMPUTERNAME to [machine], only as a whole word. A GUID on
a line no kind reads is left, a COM CLSID or a WPF window class name, because it names no
licence and no machine. It prints one line per kind with how many it masked. Every byte but
a masked span comes out as it went in, line endings and UTF-8 included, and a UTF-16 file is
written back as UTF-8, because the check cannot read UTF-16.

WHERE EACH GUARD READS, which is narrower than it may look.
- Actions reads the tree for the ids and for the RUNNER'S name, never for Bader's. Only the
  pre-commit on his machine reads for his, over what is staged
- a commit made in a git worktree runs the MAIN CLONE'S pre-commit, not the one of the
  branch checked out in the worktree, measured on 2026-09-29. So a branch that changes the
  pre-commit has its own run by hand, sh .githooks/pre-commit, before it is committed
- a file is masked BEFORE it is zipped. The check cannot read a zip, so it refuses one, and
  where a zip may sit is written at the top of the check when Bader decides it
- the check reads words. An id spelled in a way no kind names is not seen by either

- reads: the file named by -In, held open against every writer until the copy is in place,
  so no spelling of -Out that reaches the same file can change it
- writes: the file named by -Out, written beside and moved into place, then read back off
  the disk with the check's own rule, the machine name anywhere, even inside a longer word,
  and for a NUL byte. Anything left there and the copy is deleted, the kind and the line
  number are printed and never the text, and it exits 1
- refuses, writing nothing: -In equal to -Out, a missing -In, an -Out under samples,
  steps\logs or bundle, an -Out already there without -Replace, no COMPUTERNAME or one the
  rules file does not allow, a rules file it cannot read, and a file holding a NUL byte with
  no UTF-16 byte order mark
- writes outside the repo: only the -Out it is given

Proved on 2026-09-29, in %LOCALAPPDATA%\NwcFederatorLoop\turn3\f102, on copies of the probe
result, the reflection file and scan.md of fix-F100, which the check refused on 12 lines
before and passed after, with only lines 1, 446 and 447 of the result changed. On fabricated
samples with all zero GUIDs and a made up machine name: every kind masked, the near misses
left, CRLF and a UTF-8 letter kept, UTF-16 written as UTF-8, a second pass masking nothing,
the name inside a longer word refused on the read back, and each refusal above made once,
among them an -Out reaching -In through a junction, refused by the hold with -In unchanged.
Actions masks tools\checks\broken\EvidenceWithIds.txt on every run and compares the copy with
the one kept beside it.

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
