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
    written over, make-tracker.ps1 has to refuse a csv with a fault and write nothing, and the
    good fixture written with CRLF line ends and a UTF-8 byte order mark, as a Windows checkout
    or editor may write it, has to read clean. The folder is removed when the proof ends, a
    failed step included.

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
$onlyOutside = "UNKNOWN is accepted only outside the id and status columns"
$notThere = "is not there under {root}, so nothing was checked."

function Want([int] $Code, [string[]] $Lines) { return @{ Code = $Code; Lines = $Lines } }

$cases = [ordered]@{
    "good"                   = Want 0 @()
    "cell-blank"             = Want 1 @("tracker.csv line 2, id FR-001: the PR cell is empty, write UNKNOWN when it cannot be read")
    "cell-empty"             = Want 1 @("tracker.csv line 2, id FR-001: the PR cell is empty, write UNKNOWN when it cannot be read")
    "cell-line-break"        = Want 1 @("tracker.csv line 3, id FR-002: the short title cell holds a line break, which would split its row in tracker.md")
    "csv-cell-count"         = Want 1 @("tracker.csv line 3, id FR-002: 8 cells where the header has 9")
    "csv-empty"              = Want 1 @("tracker.csv line 1: the file is empty")
    "csv-header"             = Want 1 @("tracker.csv line 1: the header is not id,short title,area,wave,class,status,PR,run that proved it,date of last change")
    "csv-lone-cr"            = Want 1 @("tracker.csv line 3: a carriage return with no line feed after it")
    "csv-missing"            = Want 2 @("check-tracker: steps\tracker.csv $notThere")
    "csv-quote-inside"       = Want 1 @("tracker.csv line 4: a quote inside a cell that does not start with one")
    "csv-quote-never-closes" = Want 1 @("tracker.csv line 7: a quoted cell opens and never closes")
    "csv-text-after-quote"   = Want 1 @("tracker.csv line 3: text after the closing quote of a cell")
    "fix-round-missing"      = Want 2 @("check-tracker: steps\fix-round.md $notThere")
    "fr-area-differs"        = Want 1 @("tracker.csv line 4, id FR-003: the area 'F1' is not 'F2', which fix-round.md line 10 gives")
    "fr-class-differs"       = Want 1 @("tracker.csv line 2, id FR-001: the class 'silent wrong number' is not 'noise', which fix-round.md line 18 gives")
    "fr-heading-shape"       = Want 1 @("fix-round.md line 24 looks like the heading of an FR item and is not in the shape ### FR-<number> <key>, so it was not read as one")
    "fr-no-class-line"       = Want 1 @("tracker.csv line 9, id FR-006: the class 'noise' is not 'UNKNOWN', since its item at fix-round.md line 32 has no Class line")
    "fr-no-row"              = Want 1 @("fix-round.md line 24 names FR-003 and tracker.csv has no row for it")
    "fr-none"                = Want 1 @("fix-round.md has no heading of an FR item in the shape ### FR-<number> <key>, so no row was checked against it")
    "fr-not-in-waves"        = Want 1 @("tracker.csv line 9, id FR-006: the area 'F3' is not 'none', since the waves of fix-round.md name it in no area")
    "fr-row-other-case"      = Want 1 @("fix-round.md line 24 names FR-003 and tracker.csv has no row for it")
    "fr-two-areas"           = Want 1 @("fix-round.md line 10 names FR-001 in area F2, and line 8 in area F1")
    "fr-wave-differs"        = Want 1 @("tracker.csv line 8, id FR-005: the wave '2b' is not 'beside the waves', which fix-round.md line 11 gives")
    "id-twice"               = Want 1 @("tracker.csv line 10, id FR-002: the id is on line 3 already")
    "id-twice-other-case"    = Want 1 @("tracker.csv line 10, id fr-002: the id is on line 3 already")
    "id-unknown"             = Want 1 @("tracker.csv line 7: the id is UNKNOWN, and $onlyOutside")
    "md-missing"             = Want 1 @("tracker.md is not there, make it with tools\tracker\make-tracker.ps1")
    "md-not-made"            = Want 1 @("tracker.md line 14 is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes")
    "status-off-list"        = Want 1 @("tracker.csv line 3, id FR-002: the status 'done' is not one of $statuses")
    "status-unknown"         = Want 1 @("tracker.csv line 4, id FR-003: the status 'UNKNOWN' is not one of $statuses, and $onlyOutside")
    "waves-missing"          = Want 1 @("fix-round.md has no section headed ## The waves, so no FR row's area or wave was checked")
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

    # The good fixture with CRLF line ends and a byte order mark reads clean.
    $crlf = Join-Path $work "crlf"
    New-Item -ItemType Directory -Path (Join-Path $crlf "steps") | Out-Null
    foreach ($file in @("tracker.csv", "fix-round.md", "tracker.md")) {
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
} finally {
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
}

if ($wrong -gt 0) { Write-Output "prove-tracker: $wrong case(s) WRONG"; exit 1 }
Write-Output ("prove-tracker: all " + ($cases.Count + 2) + " cases right")
exit 0
