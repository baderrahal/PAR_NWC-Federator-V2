<#
    Proves check-tracker.ps1 and make-tracker.ps1 on the fixtures under tools\tracker\fixtures.
    F133. A check that only ever runs against a clean tree proves nothing, so each fixture is
    the good one with one thing broken, and the check has to name exactly the line written
    beside it below and no other, with exit 1. The good one has to read clean with exit 0.
    Lines the check prints about itself, those starting check-tracker:, are not compared.

    Then make-tracker.ps1 has to refuse a csv with a fault and write nothing, on a copy in a
    new folder under the temp folder, so no committed fixture is ever written over.

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

$cases = [ordered]@{
    "good"                   = @()
    "csv-cell-count"         = @("tracker.csv line 3, id FR-002: 8 cells where the header has 9")
    "csv-quote-never-closes" = @("tracker.csv line 7: a quoted cell opens and never closes")
    "csv-header"             = @("tracker.csv line 1: the header is not id,short title,area,wave,class,status,PR,run that proved it,date of last change")
    "id-twice"               = @("tracker.csv line 8, id FR-002: the id is on line 3 already")
    "id-unknown"             = @("tracker.csv line 7: the id is UNKNOWN, and $onlyOutside")
    "status-off-list"        = @("tracker.csv line 3, id FR-002: the status 'done' is not one of $statuses")
    "status-unknown"         = @("tracker.csv line 4, id FR-003: the status 'UNKNOWN' is not one of $statuses, and $onlyOutside")
    "cell-empty"             = @("tracker.csv line 2, id FR-001: the PR cell is empty, write UNKNOWN when it cannot be read")
    "fr-no-row"              = @("fix-round.md line 13 names FR-003 and tracker.csv has no row for it")
    "md-not-made"            = @("tracker.md line 14 is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes")
    "md-missing"             = @("tracker.md is not there, make it with tools\tracker\make-tracker.ps1")
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
    $faults = @($said | Where-Object { -not "$_".StartsWith("check-tracker:") })
    $want = @($cases[$name])
    $wantCode = if ($want.Count -eq 0) { 0 } else { 1 }
    $same = ($code -eq $wantCode) -and ($faults.Count -eq $want.Count)
    if ($same) { for ($k = 0; $k -lt $want.Count; $k++) { if ($faults[$k] -cne $want[$k]) { $same = $false } } }
    if ($same) {
        Write-Output "RIGHT  $name  exit $code, $($faults.Count) fault line(s) as written"
    } else {
        Write-Output "WRONG  $name  exit $code with $wantCode wanted, and these lines where the fault lines written here were wanted:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }
}

# make-tracker refuses a csv with a fault and writes nothing, on a copy.
$work = Join-Path ([IO.Path]::GetTempPath()) ("prove-tracker-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path (Join-Path $work "steps") | Out-Null
Copy-Item -LiteralPath (Join-Path $fixtures "status-off-list\steps\tracker.csv") -Destination (Join-Path $work "steps\tracker.csv")
$said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $make -Root $work)
$code = $LASTEXITCODE
$wrote = Test-Path -LiteralPath (Join-Path $work "steps\tracker.md")
if ($code -eq 1 -and -not $wrote -and ($said -join "`n").Contains("tracker.md was not written")) {
    Write-Output "RIGHT  make-tracker refused the csv with a status off the list, exit 1, and wrote nothing"
} else {
    Write-Output "WRONG  make-tracker answered $code on a csv with a fault, tracker.md written $wrote, and it said:"
    $said | ForEach-Object { Write-Output "         $_" }
    $wrong++
}
Remove-Item -LiteralPath $work -Recurse -Force

if ($wrong -gt 0) { Write-Output "prove-tracker: $wrong case(s) WRONG"; exit 1 }
Write-Output ("prove-tracker: all " + ($cases.Count + 1) + " cases right")
exit 0
