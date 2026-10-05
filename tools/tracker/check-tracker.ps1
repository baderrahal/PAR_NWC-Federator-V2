<#
    Checks the tracker. F133, Bader's message of 5 Oct 2026, Q129. Run by Actions on every
    pull request, and by prove-tracker.ps1 over every fixture under tools\tracker\fixtures.
    This header is the one list of what it refuses. The rule, the README and the workflow
    point here.

    It refuses, each fault on its own line naming the line and the id:
    - a file it reads holding bytes that are not UTF-8, a UTF-16 file among them
    - a tracker.csv that does not parse: an empty file, a carriage return with no line feed,
      a header that is not the nine columns, a quote out of place, or a row whose cell count
      is not the header's
    - an id with a space before or after it, an id written twice, counting two that differ
      only in case or in such a space as the same, an id that reads UNKNOWN in any case, an
      empty or blank cell, a cell holding a line break, and a status that is not one of the
      seven, UNKNOWN included
    - an FR item of steps\fix-round.md, by its heading, with no row written with its exact id,
      a heading naming an FR number in another shape, a second heading of one item, a
      fix-round.md with no item heading, and a row whose id starts FR- with no item headed so
    - in the waves section of fix-round.md: no such section, a line in no shape its reader
      knows, a line starting with a letter right under a wave, area or stage line, an area
      line with no wave line above it, an FR id named where the reader places no item, an
      item two lines place in two areas or at two waves, and an area two lines place at two
      waves. tracker-rules.ps1 lists the shapes
    - an F area that an area line or a stage line of the waves section names, with no row
      written with its exact id, and the row of an area such a line places whose area is not
      its own id or whose wave is not the one it is placed at
    - an FR row whose class, area or wave is not what fix-round.md gives
    - in steps\02_questions.md: a line that looks like the start of an item in another shape
      than <number>. at the margin, and a second item of one number
    - a row of class question that names no question of 02_questions.md, or names a request
      of Bader's, one whose area or wave is not what the items of fix-round.md naming its
      question give, one whose question shows Bader's answer and that reads neither merged
      with a pull request number nor in review, and one whose question has no answer and that
      does not read waiting for Bader
    - a question of 02_questions.md with no answer, its Answer lines empty or not there, and
      no row of class question written Q followed by its number
    - a row of class Bader's request that names no request of his, and a request of his, an
      item of 02_questions.md whose text starts From Bader, with no row of that class written
      Q and its number, or Q, its number, a dash and a number
    - a steps\tracker.md that is not what make-tracker.ps1 makes from the csv, naming the
      first line that differs. It is compared only when the csv has no fault, since nothing
      is made from a csv with one

    Lines it prints about itself start with check-tracker:, and every other line is a fault.
    Exits 0 when the tracker reads clean, 1 when it found a fault, a steps\tracker.md that is
    not there among them, and 2 when steps\tracker.csv, steps\fix-round.md or
    steps\02_questions.md is not there. Windows PowerShell 5.1.

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
$questions = Join-Path $Root "steps\02_questions.md"

foreach ($name in @("steps\tracker.csv", "steps\fix-round.md", "steps\02_questions.md")) {
    if (-not (Test-Path -LiteralPath (Join-Path $Root $name) -PathType Leaf)) {
        Write-Output "check-tracker: $name is not there under $Root, so nothing was checked."
        exit 2
    }
}

$read = Read-TrackerCsv $csv
$faults = New-Object System.Collections.Generic.List[string]
foreach ($f in $read.Faults) { $faults.Add($f) }
$parsed = $faults.Count -eq 0
$items = 0
$areas = 0
$asked = 0
$waiting = 0
$requests = 0
if ($parsed) {
    foreach ($f in (Test-TrackerRows $read.Rows)) { $faults.Add($f) }
    $against = Test-TrackerFixRound $read.Rows $round
    foreach ($f in $against.Faults) { $faults.Add($f) }
    $items = $against.Count
    $areas = $against.Areas
    $answers = Test-TrackerQuestions $read.Rows $questions $against.Places
    foreach ($f in $answers.Faults) { $faults.Add($f) }
    $asked = $answers.Count
    $waiting = $answers.Waiting
    $requests = $answers.Requests
    if ($null -eq $against.Places) { Write-Output "check-tracker: the area and the wave of no question row were compared, since fix-round.md was not read whole" }
}

$csvClean = $faults.Count -eq 0
if ($csvClean) {
    if (-not (Test-Path -LiteralPath $md -PathType Leaf)) {
        $faults.Add("tracker.md is not there, make it with tools\tracker\make-tracker.ps1")
    } else {
        $have = Read-TrackerText $md "tracker.md"
        if ($null -ne $have.Fault) {
            $faults.Add($have.Fault)
        } else {
            $want = (Format-TrackerMarkdown $read.Rows).Split("`n")
            $lines = $have.Text.Split("`n")
            $last = [Math]::Max($want.Length, $lines.Length)
            for ($k = 0; $k -lt $last; $k++) {
                $w = if ($k -lt $want.Length) { $want[$k] } else { $null }
                $h = if ($k -lt $lines.Length) { $lines[$k] } else { $null }
                if ($w -cne $h) {
                    $faults.Add("tracker.md line " + ($k + 1) + " is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes")
                    break
                }
            }
        }
    }
} elseif ($parsed) {
    Write-Output "check-tracker: tracker.md was not compared, since tracker.csv has a fault"
} else {
    Write-Output "check-tracker: tracker.csv did not parse, so no row, no FR item, no question and not tracker.md was checked"
}

if ($faults.Count -gt 0) {
    $faults | ForEach-Object { Write-Output $_ }
    Write-Output ("check-tracker: REFUSED, " + $faults.Count + " fault(s) above")
    exit 1
}
Write-Output ("check-tracker: the tracker reads clean, " + $read.Rows.Count + " rows, each of the $items FR items of fix-round.md has one with its class, area and wave, each of the $areas areas its waves section names has one, each of the $waiting questions of 02_questions.md with no answer has one, each of the $requests requests of Bader's there has one, the $asked rows of class question each name a question of 02_questions.md, read the area and the wave of the items naming it and read as its answer says, and tracker.md is what make-tracker.ps1 makes")
exit 0
