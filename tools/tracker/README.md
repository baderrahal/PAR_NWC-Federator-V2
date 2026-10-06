# The work tracker

F133, Bader's message of 5 Oct 2026, Q129. One place for status: `steps\tracker.csv`, one row
per item, covering every FR item of `steps\fix-round.md`, every F area, Bader's requests and
every question waiting for him. A question keeps its row once he answers, so of the questions
the tracker holds those waiting for him and those that once had a row, and a question he
answered before any row was written for it has none. The rule is `.claude\rules\tracker.md`.

Windows PowerShell 5.1, measured on this machine at 5.1.26100.9444 on 2026-10-05, and carried
by the Actions windows-latest runner, whose workflow already runs `prove-compare.ps1` with it.

| File | What it does |
|---|---|
| `tracker-rules.ps1` | the one place the rules live: the nine columns, the seven statuses, the csv reader, the row rules, the FR items of fix-round.md with their class, area and wave, the shapes of a line of its waves section, the questions of 02_questions.md, whether each is answered and the area and the wave the FR items naming it give, Bader's requests there, and the shape of tracker.md. make-tracker.ps1 and check-tracker.ps1 read it with a dot |
| `make-tracker.ps1` | makes `steps\tracker.md` from `steps\tracker.csv`, and refuses, writing nothing, when the csv has a fault |
| `check-tracker.ps1` | refuses each fault its header lists, the one list of them, each naming its line. Exits 2 when the csv, fix-round.md or 02_questions.md is not there, and 1 for any fault, a tracker.md that is not there among them. Actions runs it on every pull request |
| `prove-tracker.ps1` | runs the check over every fixture and asserts the exact line and exit code of each, then, on copies, that the maker refuses a csv with a fault and a folder with no csv, that the good fixture with CRLF line ends and a byte order mark reads clean, and that it is refused with its fix-round.md in UTF-16, which no committed fixture may be. Actions runs it on every pull request |
| `fixtures\good` | a small tracker that reads clean, its tracker.md made by the maker |
| `fixtures\<kind>` | the good one with one thing broken, one folder per fault kind and per file that is not there, named for it, written by a script kept outside the repo that refuses an edit matching other than once |

To change a status: edit the row in `steps\tracker.csv`, run

    powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\make-tracker.ps1

and commit both files in the pull request that changed the status. A branch writes the rows it
finishes as in review with PR UNKNOWN, and the lead sets merged and the number before it merges,
by `.claude\rules\steps.md`.

Nothing here writes outside the repo but `prove-tracker.ps1`, which makes one folder under the
temp folder for its four cases on copies and removes it before it ends, a failed step included.
