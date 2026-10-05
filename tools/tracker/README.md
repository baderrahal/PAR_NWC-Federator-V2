# The work tracker

F133, Bader's message of 5 Oct 2026, Q129. One place for status: `steps\tracker.csv`, one row
per item, covering every FR item of `steps\fix-round.md`, every F area, Bader's requests and
every question waiting for him. The rule is `.claude\rules\tracker.md`.

Windows PowerShell 5.1, measured on this machine at 5.1.26100.9444 on 2026-10-05, and carried
by the Actions windows-latest runner, whose workflow already runs `prove-compare.ps1` with it.

| File | What it does |
|---|---|
| `tracker-rules.ps1` | the one place the rules live: the nine columns, the seven statuses, the csv reader, the row rules, the FR items of fix-round.md and the shape of tracker.md. The other three read it with a dot |
| `make-tracker.ps1` | makes `steps\tracker.md` from `steps\tracker.csv`, and refuses, writing nothing, when the csv has a fault |
| `check-tracker.ps1` | refuses a csv that does not parse, an id twice, an UNKNOWN id, an empty cell, a status off the list, an FR item with no row, and a tracker.md that is not what the maker makes, each naming its line. Actions runs it on every pull request |
| `prove-tracker.ps1` | runs the check over every fixture and asserts the exact line each one is refused with, and that the maker refuses a csv with a fault. Actions runs it on every pull request |
| `fixtures\good` | a small tracker that reads clean, its tracker.md made by the maker |
| `fixtures\<kind>` | the good one with one thing broken, one folder per fault kind, named for it |

To change a status: edit the row in `steps\tracker.csv`, run

    powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\make-tracker.ps1

and commit both files in the pull request that changed the status.

Nothing here writes outside the repo but `prove-tracker.ps1`, which makes one folder under the
temp folder for the maker's refusal and removes it before it ends.
