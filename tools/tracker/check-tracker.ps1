<#
    Checks the tracker. F133, Bader's message of 5 Oct 2026, Q129. Run by Actions on every
    pull request, and by prove-tracker.ps1 over every fixture under tools\tracker\fixtures.

    It refuses, each fault on its own line naming the line and the id:
    - a tracker.csv that does not parse: a header that is not the nine columns, a quote out
      of place, or a row whose cell count is not the header's
    - an id written twice, an id that reads UNKNOWN, an empty cell, and a status that is not
      one of the seven, UNKNOWN included
    - an FR item of steps\fix-round.md, by its heading, with no row
    - a steps\tracker.md that is not what make-tracker.ps1 makes from the csv, naming the
      first line that differs. It is compared only when the csv has no fault, since nothing
      is made from a csv with one

    Lines it prints about itself start with check-tracker:, and every other line is a fault.
    Exits 0 when the tracker reads clean, 1 when it found a fault, and 2 when a file it reads
    is not there. Windows PowerShell 5.1.

    Usage:
        powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\check-tracker.ps1 [-Root <a folder holding steps>]
#>

[CmdletBinding()]
param([string] $Root)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "tracker-rules.ps1")

if (-not $Root) { $Root = Split-Path (Split-Path $PSScriptRoot) }
$csv = Join-Path $Root "steps\tracker.csv"
$md = Join-Path $Root "steps\tracker.md"
$round = Join-Path $Root "steps\fix-round.md"

foreach ($file in @($csv, $round)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        Write-Output "check-tracker: $file is not there, so nothing was checked."
        exit 2
    }
}

$read = Read-TrackerCsv $csv
$faults = New-Object System.Collections.Generic.List[string]
foreach ($f in $read.Faults) { $faults.Add($f) }
$parsed = $faults.Count -eq 0
if ($parsed) {
    foreach ($f in (Test-TrackerRows $read.Rows)) { $faults.Add($f) }

    $ids = @{}
    foreach ($row in $read.Rows) { $ids[$row["id"]] = $true }
    foreach ($item in (Get-FixRoundItems $round)) {
        if (-not $ids.ContainsKey($item.Id)) { $faults.Add("fix-round.md line " + $item.Line + " names " + $item.Id + " and tracker.csv has no row for it") }
    }
}

$csvClean = $faults.Count -eq 0
if ($csvClean) {
    if (-not (Test-Path -LiteralPath $md -PathType Leaf)) {
        $faults.Add("tracker.md is not there, make it with tools\tracker\make-tracker.ps1")
    } else {
        $want = (Format-TrackerMarkdown $read.Rows).Split("`n")
        $have = [IO.File]::ReadAllText($md, [Text.Encoding]::UTF8).Replace("`r`n", "`n").Split("`n")
        $last = [Math]::Max($want.Length, $have.Length)
        for ($k = 0; $k -lt $last; $k++) {
            $w = if ($k -lt $want.Length) { $want[$k] } else { $null }
            $h = if ($k -lt $have.Length) { $have[$k] } else { $null }
            if ($w -cne $h) {
                $faults.Add("tracker.md line " + ($k + 1) + " is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes")
                break
            }
        }
    }
} elseif ($parsed) {
    Write-Output "check-tracker: tracker.md was not compared, since tracker.csv has a fault"
} else {
    Write-Output "check-tracker: tracker.csv did not parse, so no row, no FR item and not tracker.md was checked"
}

if ($faults.Count -gt 0) {
    $faults | ForEach-Object { Write-Output $_ }
    Write-Output ("check-tracker: REFUSED, " + $faults.Count + " fault(s) above")
    exit 1
}
Write-Output ("check-tracker: the tracker reads clean, " + $read.Rows.Count + " rows, every FR item of fix-round.md has one, and tracker.md is what make-tracker.ps1 makes")
exit 0
