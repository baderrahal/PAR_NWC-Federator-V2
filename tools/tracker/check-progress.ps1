<#
    Checks steps\PROGRESS.md, the one page a session starts from. F139, Bader's message of 6 Oct
    2026, Q139. Run by the pre-commit hook after make-tracker.ps1 whenever steps\tracker.csv or
    steps\PROGRESS.md is staged, by Actions on every pull request, and by prove-progress.ps1 over
    every fixture under tools\tracker\progress-fixtures. This header is the one list of what it
    refuses. The rule, the README and the workflow point here.

    It refuses, each fault on its own line naming the line:
    - a PROGRESS.md longer than $ProgressMaxLines lines of tracker-rules.ps1, 60, counting every
      line, blank ones and the two marker lines among them
    - a PROGRESS.md holding bytes that are not UTF-8, a UTF-16 file among them
    - a PROGRESS.md with no line reading the marker line above the counts, or the one below them,
      either of them there a second time, or the one below before the one above, each read whole
      and case-sensitive. tracker-rules.ps1 holds the two lines
    - lines between the markers that are not what make-tracker.ps1 makes from steps\tracker.csv,
      naming the first line that differs. They are compared only when the csv has no fault the
      maker refuses, since nothing is made from a csv with one, and such a csv is one fault here,
      its faults named by check-tracker.ps1

    Lines it prints about itself start with check-progress:, and every other line is a fault.
    Exits 0 when the page reads clean, 1 when it found a fault, and 2 when steps\tracker.csv or
    steps\PROGRESS.md is not there. Windows PowerShell 5.1.

    Usage:
        powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\check-progress.ps1 [-Root <a folder holding steps>]
#>

[CmdletBinding()]
param([string] $Root)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "tracker-rules.ps1")

if (-not $Root) { $Root = Split-Path (Split-Path $PSScriptRoot) }
$csv = Join-Path $Root "steps\tracker.csv"
$progress = Join-Path $Root "steps\PROGRESS.md"

foreach ($name in @("steps\tracker.csv", "steps\PROGRESS.md")) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $name) -PathType Leaf)) {
        Write-Output "check-progress: $name is not there under $Root, so nothing was checked."
        exit 2
    }
}

$faults = New-Object System.Collections.Generic.List[string]
$count = 0
$rows = 0
$page = Read-TrackerText $progress "PROGRESS.md"
if ($null -ne $page.Fault) {
    $faults.Add($page.Fault)
} else {
    $count = Measure-ProgressLines $page.Text
    if ($count -gt $ProgressMaxLines) { $faults.Add("PROGRESS.md is $count lines, and its rule allows at most $ProgressMaxLines") }
    $lines = $page.Text.Split("`n")
    $block = Find-ProgressBlock $lines
    foreach ($f in $block.Faults) { $faults.Add($f) }
    $read = Read-TrackerCsv $csv
    $csvFaults = @($read.Faults) + @(Test-TrackerRows $read.Rows)
    $rows = $read.Rows.Count
    if ($csvFaults.Count -gt 0) {
        $faults.Add("the counts of PROGRESS.md were not compared, since tracker.csv has " + (Format-TrackerCount $csvFaults.Count "fault" "faults") + ", which check-tracker.ps1 names")
    } elseif ($block.Faults.Count -gt 0) {
        Write-Output "check-progress: the counts were not compared, since the page has no place for them"
    } else {
        $want = @(Format-ProgressCounts $read.Rows)
        $have = New-Object System.Collections.Generic.List[string]
        for ($k = $block.Start + 1; $k -lt $block.End; $k++) { $have.Add($lines[$k]) }
        $last = [Math]::Max($want.Length, $have.Count)
        for ($k = 0; $k -lt $last; $k++) {
            $w = if ($k -lt $want.Length) { $want[$k] } else { $null }
            $h = if ($k -lt $have.Count) { $have[$k] } else { $null }
            if ($w -cne $h) {
                $faults.Add("PROGRESS.md line " + ($block.Start + 2 + $k) + " is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes")
                break
            }
        }
    }
}

if ($faults.Count -gt 0) {
    $faults | ForEach-Object { Write-Output $_ }
    Write-Output ("check-progress: REFUSED, " + $faults.Count + " fault(s) above")
    exit 1
}
Write-Output "check-progress: PROGRESS.md reads clean, $count lines of the $ProgressMaxLines its rule allows, and its counts are what make-tracker.ps1 makes from the $rows rows of tracker.csv"
exit 0
