<#
    Proves check-tracker.ps1 and make-tracker.ps1 on the fixtures under tools\tracker\fixtures.
    F133. A check that only ever runs against a clean tree proves nothing, so each fixture is
    the good one with one thing broken, and the check has to name exactly the line written
    beside it below and no other, with exit 1, or with exit 2 and the line it prints about
    itself when a file it reads is not there. The good one has to read clean with exit 0.
    Lines the check prints about itself, those starting check-tracker:, are not compared for
    exit 0 and 1. The lines below are written out in full, status list and header included,
    since a proof asserts the text a person reads.

    Then, on copies in a new folder under the temp folder, so no committed fixture is ever
    written over, make-tracker.ps1 has to refuse a csv with a fault and a folder with no csv,
    writing nothing each time, the good fixture written with CRLF line ends and a UTF-8 byte
    order mark, as a Windows checkout or editor may write it, has to read clean, and the good
    fixture with its fix-round.md saved as UTF-16 has to be refused. That case is written here
    and not committed, since the evidence check of the pre-commit refuses a UTF-16 file in the
    repo. The folder is removed when the proof ends, a failed step included.

    Each run of the check is its own powershell.exe, so the exit code is the one Actions sees.
    Exits 0 when every case is right and 1 otherwise. Windows PowerShell 5.1.

    Usage:
        powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\prove-tracker.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$check = Join-Path $PSScriptRoot "check-tracker.ps1"
$make = Join-Path $PSScriptRoot "make-tracker.ps1"
$fixtures = Join-Path $PSScriptRoot "fixtures"
$statuses = "open, in progress, in review, merged, proven by a run, waiting for Bader, dropped"
$header = "id,short title,area,wave,class,status,PR,the run that proved it,the date of the last change"
$onlyOutside = "UNKNOWN is accepted only outside the id and status columns"
$notThere = "is not there under {root}, so nothing was checked."
$notUtf8 = "holds bytes that are not UTF-8, so the file was not read"
$notHeading = "looks like the heading of an FR item and is not in the shape ### FR-<number> <key>, so it was not read as one"
$noShape = "in the waves section, is in no shape the waves reader knows, so nothing on it was read"
$notPlaced = "where the waves reader places no item, so its area and wave were not read"
$answered = "holds Bader's answer, so the row reads merged with the number of the pull request that put it on main, or in review on the branch that records it"

function Want([int] $Code, [string[]] $Lines) { return @{ Code = $Code; Lines = $Lines } }

$cases = [ordered]@{
    "good"                           = Want 0 @()
    "area-line-no-item-no-row"       = Want 1 @("fix-round.md line 14 names area F5 and tracker.csv has no row for it")
    "area-named-no-row"              = Want 1 @("fix-round.md line 16 names area F6 and tracker.csv has no row for it")
    "area-no-row"                    = Want 1 @("fix-round.md line 15 names area F4 and tracker.csv has no row for it")
    "area-own-id-differs"            = Want 1 @("tracker.csv line 5, id F1: the area 'none' is not 'F1', its own id, since fix-round.md line 11 places it as an area")
    "area-two-waves"                 = Want 1 @("fix-round.md line 15 places area F5 at wave 2b, and line 14 at wave 2a")
    "area-wave-differs"              = Want 1 @("tracker.csv line 6, id F2: the wave '2b' is not '2a', which fix-round.md line 13 gives")
    "cell-blank"                     = Want 1 @("tracker.csv line 2, id FR-001: the PR cell is empty, write UNKNOWN when it cannot be read")
    "cell-empty"                     = Want 1 @("tracker.csv line 2, id FR-001: the PR cell is empty, write UNKNOWN when it cannot be read")
    "cell-line-break"                = Want 1 @("tracker.csv line 3, id FR-002: the short title cell holds a line break, which would split its row in tracker.md")
    "csv-cell-count"                 = Want 1 @("tracker.csv line 3, id FR-002: 8 cells where the header has 9")
    "csv-empty"                      = Want 1 @("tracker.csv line 1: the file is empty")
    "csv-header"                     = Want 1 @("tracker.csv line 1: the header is not $header")
    "csv-header-old-words"           = Want 1 @("tracker.csv line 1: the header is not $header")
    "csv-lone-cr"                    = Want 1 @("tracker.csv line 3: a carriage return with no line feed after it")
    "csv-missing"                    = Want 2 @("check-tracker: steps\tracker.csv $notThere")
    "csv-not-utf8"                   = Want 1 @("tracker.csv line 4 $notUtf8")
    "csv-quote-inside"               = Want 1 @("tracker.csv line 4: a quote inside a cell that does not start with one")
    "csv-quote-never-closes"         = Want 1 @("tracker.csv line 20: a quoted cell opens and never closes")
    "csv-text-after-quote"           = Want 1 @("tracker.csv line 3: text after the closing quote of a cell")
    "fix-round-missing"              = Want 2 @("check-tracker: steps\fix-round.md $notThere")
    "fix-round-not-utf8"             = Want 1 @("fix-round.md line 7 $notUtf8")
    "fr-area-differs"                = Want 1 @("tracker.csv line 4, id FR-003: the area 'F1' is not 'F2', which fix-round.md line 13 gives")
    "fr-class-differs"               = Want 1 @("tracker.csv line 2, id FR-001: the class 'silent wrong number' is not 'noise', which fix-round.md line 24 gives")
    "fr-heading-loose"               = Want 1 @("fix-round.md line 52 $notHeading")
    "fr-heading-shape"               = Want 1 @("fix-round.md line 30 $notHeading", "tracker.csv line 4, id FR-003: fix-round.md has no item headed with that id")
    "fr-heading-twice"               = Want 1 @("fix-round.md line 52 is a second heading of FR-003, whose first is at line 30")
    "fr-no-class-line"               = Want 1 @("tracker.csv line 9, id FR-006: the class 'noise' is not 'UNKNOWN', since its item at fix-round.md line 39 has no Class line")
    "fr-no-row"                      = Want 1 @("fix-round.md line 30 names FR-003 and tracker.csv has no row for it")
    "fr-none"                        = Want 1 @("fix-round.md has no heading of an FR item in the shape ### FR-<number> <key>, so no row was checked against it")
    "fr-not-in-waves"                = Want 1 @("tracker.csv line 9, id FR-006: the area 'F3' is not 'none', since the waves of fix-round.md name it in no area")
    "fr-row-no-item"                 = Want 1 @("tracker.csv line 21, id FR-009: fix-round.md has no item headed with that id")
    "fr-row-other-case"              = Want 1 @("fix-round.md line 30 names FR-003 and tracker.csv has no row for it", "tracker.csv line 4, id fr-003: fix-round.md has no item headed with that id")
    "fr-two-areas"                   = Want 1 @("fix-round.md line 13 names FR-001 in area F2, and line 11 in area F1")
    "fr-wave-differs"                = Want 1 @("tracker.csv line 8, id FR-005: the wave '2b' is not 'beside the waves', which fix-round.md line 18 gives")
    "id-space"                       = Want 1 @("tracker.csv line 21: the id 'F1 ' has a space before or after it", "tracker.csv line 21, id F1 : the id is on line 5 already")
    "id-twice"                       = Want 1 @("tracker.csv line 21, id FR-002: the id is on line 3 already")
    "id-twice-other-case"            = Want 1 @("tracker.csv line 21, id fr-002: the id is on line 3 already", "tracker.csv line 21, id fr-002: fix-round.md has no item headed with that id")
    "id-unknown"                     = Want 1 @("tracker.csv line 6: the id is UNKNOWN, and $onlyOutside", "fix-round.md line 13 names area F2 and tracker.csv has no row for it")
    "id-unknown-other-case"          = Want 1 @("tracker.csv line 5: the id is UNKNOWN, and $onlyOutside", "fix-round.md line 11 names area F1 and tracker.csv has no row for it")
    "md-missing"                     = Want 1 @("tracker.md is not there, make it with tools\tracker\make-tracker.ps1")
    "md-not-made"                    = Want 1 @("tracker.md line 14 is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes")
    "md-not-utf8"                    = Want 1 @("tracker.md line 14 $notUtf8")
    "question-answered-no-number"    = Want 1 @("tracker.csv line 12, id Q2: 02_questions.md line 11 $answered, and not 'merged' with PR 'none'")
    "question-answered-waiting"      = Want 1 @("tracker.csv line 12, id Q2: 02_questions.md line 11 $answered, and not 'waiting for Bader' with PR 'none'")
    "question-area-differs"          = Want 1 @("tracker.csv line 12, id Q2: the area 'F4' is not 'F2, F4', which fix-round.md gives off the items naming Q2, FR-003 at line 33, and FR-008 at line 50")
    "question-area-not-none"         = Want 1 @("tracker.csv line 7, id Q1: the area 'F2' is not 'none', since no item of fix-round.md in an area of its waves names Q1")
    "question-no-row"                = Want 1 @("02_questions.md line 5 asks question 1, which has no answer, and tracker.csv has no row Q1 of class question for it")
    "question-row-no-question"       = Want 1 @("tracker.csv line 21, id Q9: its class is question and 02_questions.md has no question by that number")
    "question-row-on-request"        = Want 1 @("tracker.csv line 21, id Q4: its class is question and 02_questions.md has no question by that number")
    "question-second-answer-waiting" = Want 1 @("tracker.csv line 13, id Q3: 02_questions.md line 17 $answered, and not 'waiting for Bader' with PR 'none'")
    "question-waiting-merged"        = Want 1 @("tracker.csv line 7, id Q1: 02_questions.md line 5 asks question 1, which has no answer, so the row reads waiting for Bader and not 'merged'")
    "question-waiting-other-class"   = Want 1 @("02_questions.md line 5 asks question 1, which has no answer, and tracker.csv has no row Q1 of class question for it")
    "questions-item-shape"           = Want 1 @("02_questions.md line 27 looks like the start of an item and is not in the shape <number>. at the margin, so it was not read as one")
    "questions-item-twice"           = Want 1 @("02_questions.md line 27 is a second item numbered 2, whose first is at line 9")
    "questions-missing"              = Want 2 @("check-tracker: steps\02_questions.md $notThere")
    "questions-not-utf8"             = Want 1 @("02_questions.md line 9 $notUtf8")
    "request-no-row"                 = Want 1 @("02_questions.md line 23 holds request 5 of Bader's, and tracker.csv has no row Q5, or Q5- and a number, of class Bader's request for it")
    "request-row-no-request"         = Want 1 @("tracker.csv line 21, id Q2-1: its class is Bader's request and 02_questions.md has no request of his by that number")
    "status-off-list"                = Want 1 @("tracker.csv line 3, id FR-002: the status 'done' is not one of $statuses")
    "status-unknown"                 = Want 1 @("tracker.csv line 4, id FR-003: the status 'UNKNOWN' is not one of $statuses, and $onlyOutside")
    "waves-area-line-shape"          = Want 1 @("fix-round.md line 14, $noShape")
    "waves-area-no-wave"             = Want 1 @("fix-round.md line 17 is an area line with no wave line above it, so its wave was not read")
    "waves-item-not-placed"          = Want 1 @("fix-round.md line 15 names FR-006 $notPlaced")
    "waves-lazy-line"                = Want 1 @("fix-round.md line 14, in the waves section, starts with a letter right under a wave, area or stage line, so Markdown reads it as part of that line and the waves reader does not, put a blank line above it or indent it")
    "waves-line-shape"               = Want 1 @("fix-round.md line 16, $noShape")
    "waves-missing"                  = Want 1 @("fix-round.md has no section headed ## The waves, so no FR row's area or wave was checked")
    "waves-stage-item-unpaired"      = Want 1 @("fix-round.md line 16 names FR-006 $notPlaced")
    "waves-top-line-shape"           = Want 1 @("fix-round.md line 16, $noShape")
    "waves-wave-line-shape"          = Want 1 @("fix-round.md line 12, $noShape")
    "waves-wave-twice"               = Want 1 @("fix-round.md line 14 places area F1 at wave 2b, and line 11 at wave 1", "fix-round.md line 14 names FR-002 at wave 2b, and line 11 at wave 1")
}

$wrong = 0
$found = @(Get-ChildItem -LiteralPath $fixtures -Directory | ForEach-Object { $_.Name })
foreach ($name in $found) {
    if (-not $cases.Contains($name)) { Write-Output "WRONG  fixtures\$name has no case here, so nothing proves it"; $wrong++ }
}

foreach ($name in $cases.Keys) {
    $root = Join-Path $fixtures $name
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { Write-Output "WRONG  $name  the fixture is not there"; $wrong++; continue }
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $check -Root $root)
    $code = $LASTEXITCODE
    $case = $cases[$name]
    $want = @($case.Lines | ForEach-Object { $_.Replace("{root}", $root) })
    $compared = @(if ($case.Code -eq 2) { $said } else { $said | Where-Object { -not "$_".StartsWith("check-tracker:") } })
    $same = ($code -eq $case.Code) -and ($compared.Count -eq $want.Count)
    if ($same) { for ($k = 0; $k -lt $want.Count; $k++) { if ($compared[$k] -cne $want[$k]) { $same = $false } } }
    if ($same) {
        Write-Output "RIGHT  $name  exit $code, $($compared.Count) line(s) as written"
    } else {
        Write-Output "WRONG  $name  exit $code with $($case.Code) wanted, and these lines where the lines written here were wanted:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }
}

$work = Join-Path ([IO.Path]::GetTempPath()) ("prove-tracker-" + [Guid]::NewGuid().ToString("N"))
try {
    # make-tracker refuses a csv with a fault and writes nothing, on a copy.
    $refuse = Join-Path $work "refuse"
    New-Item -ItemType Directory -Path (Join-Path $refuse "steps") | Out-Null
    Copy-Item -LiteralPath (Join-Path $fixtures "status-off-list\steps\tracker.csv") -Destination (Join-Path $refuse "steps\tracker.csv")
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $make -Root $refuse)
    $code = $LASTEXITCODE
    $wrote = Test-Path -LiteralPath (Join-Path $refuse "steps\tracker.md")
    if ($code -eq 1 -and -not $wrote -and ($said -join "`n").Contains("tracker.md was not written")) {
        Write-Output "RIGHT  make-tracker refused the csv with a status off the list, exit 1, and wrote nothing"
    } else {
        Write-Output "WRONG  make-tracker answered $code on a csv with a fault, tracker.md written $wrote, and it said:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }

    # make-tracker refuses a folder with no csv and writes nothing.
    $none = Join-Path $work "none"
    New-Item -ItemType Directory -Path (Join-Path $none "steps") | Out-Null
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $make -Root $none)
    $code = $LASTEXITCODE
    $wrote = Test-Path -LiteralPath (Join-Path $none "steps\tracker.md")
    $want = "make-tracker: REFUSED. " + (Join-Path $none "steps\tracker.csv") + " is not there, so nothing was written."
    if ($code -eq 1 -and -not $wrote -and $said.Count -eq 1 -and $said[0] -ceq $want) {
        Write-Output "RIGHT  make-tracker refused a folder with no csv, exit 1, and wrote nothing"
    } else {
        Write-Output "WRONG  make-tracker answered $code on a folder with no csv, tracker.md written $wrote, and it said:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }

    # The good fixture with CRLF line ends and a byte order mark reads clean.
    $crlf = Join-Path $work "crlf"
    New-Item -ItemType Directory -Path (Join-Path $crlf "steps") | Out-Null
    foreach ($file in @("tracker.csv", "fix-round.md", "02_questions.md", "tracker.md")) {
        $text = [IO.File]::ReadAllText((Join-Path $fixtures "good\steps\$file"), [Text.Encoding]::UTF8).Replace("`r`n", "`n").Replace("`n", "`r`n")
        [IO.File]::WriteAllText((Join-Path $crlf "steps\$file"), $text, (New-Object System.Text.UTF8Encoding $true))
    }
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $check -Root $crlf)
    $code = $LASTEXITCODE
    $faults = @($said | Where-Object { -not "$_".StartsWith("check-tracker:") })
    if ($code -eq 0 -and $faults.Count -eq 0) {
        Write-Output "RIGHT  good with CRLF and a byte order mark  exit 0, 0 line(s) as written"
    } else {
        Write-Output "WRONG  good with CRLF and a byte order mark  exit $code with 0 wanted, and it said:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }

    # The good fixture with its fix-round.md saved as UTF-16, with its byte order mark, is refused.
    $wide = Join-Path $work "utf16"
    New-Item -ItemType Directory -Path (Join-Path $wide "steps") | Out-Null
    foreach ($file in @("tracker.csv", "02_questions.md", "tracker.md")) {
        Copy-Item -LiteralPath (Join-Path $fixtures "good\steps\$file") -Destination (Join-Path $wide "steps\$file")
    }
    $text = [IO.File]::ReadAllText((Join-Path $fixtures "good\steps\fix-round.md"), [Text.Encoding]::UTF8)
    [IO.File]::WriteAllText((Join-Path $wide "steps\fix-round.md"), $text, [Text.Encoding]::Unicode)
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $check -Root $wide)
    $code = $LASTEXITCODE
    $faults = @($said | Where-Object { -not "$_".StartsWith("check-tracker:") })
    if ($code -eq 1 -and $faults.Count -eq 1 -and $faults[0] -ceq "fix-round.md line 1 $notUtf8") {
        Write-Output "RIGHT  good with fix-round.md in UTF-16  exit 1, 1 line(s) as written"
    } else {
        Write-Output "WRONG  good with fix-round.md in UTF-16  exit $code with 1 wanted, and it said:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }
} finally {
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
}

if ($wrong -gt 0) { Write-Output "prove-tracker: $wrong case(s) WRONG"; exit 1 }
Write-Output ("prove-tracker: all " + ($cases.Count + 4) + " cases right")
exit 0
