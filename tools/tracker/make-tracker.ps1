<#
    Makes steps\tracker.md from steps\tracker.csv. F133, Bader's message of 5 Oct 2026, Q129.
    tracker.md is never edited by hand: change a row of the csv, run this, and commit both in
    the same pull request. check-tracker.ps1 refuses a tracker.md that is not what this makes.

    At the top, in under 15 lines, the counts by status and by wave, what is in progress now
    and what waits for Bader. Then one table per wave, the rows in the order of the csv.

    It refuses, and writes nothing, when the csv does not parse or a row breaks a rule of
    tracker-rules.ps1, naming each line. Windows PowerShell 5.1, which this machine and the
    Actions windows-latest runner both carry. Exits 0 when it wrote the file and 1 otherwise.

    Usage:
        powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\make-tracker.ps1 [-Root <a folder holding steps\tracker.csv>]
#>

[CmdletBinding()]
param([string] $Root)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "tracker-rules.ps1")

if (-not $Root) { $Root = Split-Path (Split-Path $PSScriptRoot) }
$csv = Join-Path $Root "steps\tracker.csv"
$md = Join-Path $Root "steps\tracker.md"

if (-not (Test-Path -LiteralPath $csv -PathType Leaf)) {
    Write-Output "make-tracker: REFUSED. $csv is not there, so nothing was written."
    exit 1
}

$read = Read-TrackerCsv $csv
$faults = @($read.Faults) + @(Test-TrackerRows $read.Rows)
if ($faults.Count -gt 0) {
    $faults | ForEach-Object { Write-Output $_ }
    Write-Output ("make-tracker: REFUSED. " + $faults.Count + " fault(s) in tracker.csv above, so tracker.md was not written.")
    exit 1
}

[IO.File]::WriteAllText($md, (Format-TrackerMarkdown $read.Rows), (New-Object System.Text.UTF8Encoding $false))
Write-Output ("make-tracker: wrote $md from " + $read.Rows.Count + " rows.")
exit 0
