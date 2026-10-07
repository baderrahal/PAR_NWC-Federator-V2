<#
    Makes steps\tracker.md from steps\tracker.csv, and the counts of steps\PROGRESS.md between its
    two marker lines. F133, Bader's message of 5 Oct 2026, Q129, and F139, his message of 6 Oct
    2026, Q139. Neither is ever edited by hand: change a row of the csv, run this, and commit what
    it writes in the same pull request. The pre-commit hook runs it whenever steps\tracker.csv or
    steps\PROGRESS.md is staged and stages what it wrote. check-tracker.ps1 refuses a tracker.md
    and check-progress.ps1 counts that are not what this makes.

    tracker.md: at the top, in under 15 lines, the counts by status and by wave, what is in
    progress now and what waits for Bader. Then one table per wave, the rows in the order of the
    csv. PROGRESS.md: only the lines between its two marker lines, the counts by wave under
    Bader's five words, every other line left as it is. The page is written only when its counts
    change, so a commit that changes no count leaves it alone.

    It refuses, and writes nothing, when the csv is not there, does not parse or a row breaks a
    rule of tracker-rules.ps1, and when PROGRESS.md is not there, holds bytes that are not UTF-8,
    or has no line or two lines reading a marker line, or the one below before the one above,
    naming each line. Windows PowerShell 5.1, which this machine and the Actions windows-latest
    runner both carry. Exits 0 when it wrote what it makes and 1 otherwise.

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
$progress = Join-Path $Root "steps\PROGRESS.md"

if (-not (Test-Path -LiteralPath $csv -PathType Leaf)) {
    Write-Output "make-tracker: REFUSED. $csv is not there, so nothing was written."
    exit 1
}

$read = Read-TrackerCsv $csv
$faults = @($read.Faults) + @(Test-TrackerRows $read.Rows)
if ($faults.Count -gt 0) {
    $faults | ForEach-Object { Write-Output $_ }
    Write-Output ("make-tracker: REFUSED. " + $faults.Count + " fault(s) in tracker.csv above, so tracker.md was not written, and neither was PROGRESS.md.")
    exit 1
}

if (-not (Test-Path -LiteralPath $progress -PathType Leaf)) {
    Write-Output "make-tracker: REFUSED. $progress is not there, so nothing was written."
    exit 1
}
$page = Read-TrackerText $progress "PROGRESS.md"
$pageFaults = New-Object System.Collections.Generic.List[string]
if ($null -ne $page.Fault) {
    $pageFaults.Add($page.Fault)
} else {
    $lines = $page.Text.Split("`n")
    $block = Find-ProgressBlock $lines
    foreach ($f in $block.Faults) { $pageFaults.Add($f) }
}
if ($pageFaults.Count -gt 0) {
    $pageFaults | ForEach-Object { Write-Output $_ }
    Write-Output ("make-tracker: REFUSED. " + (Format-TrackerCount $pageFaults.Count "fault" "faults") + " in PROGRESS.md above, so neither tracker.md nor PROGRESS.md was written.")
    exit 1
}

$made = New-Object System.Collections.Generic.List[string]
for ($k = 0; $k -le $block.Start; $k++) { $made.Add($lines[$k]) }
foreach ($line in @(Format-ProgressCounts $read.Rows)) { $made.Add($line) }
for ($k = $block.End; $k -lt $lines.Length; $k++) { $made.Add($lines[$k]) }
$text = $made -join "`n"

$utf8 = New-Object System.Text.UTF8Encoding $false
[IO.File]::WriteAllText($md, (Format-TrackerMarkdown $read.Rows), $utf8)
Write-Output ("make-tracker: wrote $md from " + $read.Rows.Count + " rows.")
if ($text -ceq $page.Text) {
    Write-Output "make-tracker: the counts in $progress are what the csv makes already, so it was left as it is."
} else {
    [IO.File]::WriteAllText($progress, $text, $utf8)
    Write-Output "make-tracker: wrote the counts into $progress."
}
exit 0
