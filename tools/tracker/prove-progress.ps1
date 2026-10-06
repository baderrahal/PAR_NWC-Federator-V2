<#
    Proves check-progress.ps1 and the part of make-tracker.ps1 that writes the counts of
    steps\PROGRESS.md, on the fixtures under tools\tracker\progress-fixtures. F139, Bader's
    message of 6 Oct 2026, Q139. A check that only ever runs against a clean page proves nothing,
    so each fixture is the good one with one thing broken, and the check has to name exactly the
    line written beside it below and no other, with exit 1, or with exit 2 and the line it prints
    about itself when a file it reads is not there. The good one and the page of exactly 60 lines
    have to read clean with exit 0. Lines the check prints about itself, those starting
    check-progress:, are not compared for exit 0 and 1. The lines below are written out in full,
    the marker lines included, since a proof asserts the text a person reads.

    Then, on copies in a new folder under the temp folder, so no committed fixture is ever written
    over: the good page with CRLF line ends and a UTF-8 byte order mark, as a Windows checkout or
    editor may write it, has to read clean, and a page holding a byte that is not UTF-8 has to be
    refused by the check. make-tracker.ps1 has to refuse a folder with no PROGRESS.md, each page
    whose marker lines are wrong and a page that is not UTF-8, each time writing neither
    tracker.md nor PROGRESS.md, to leave the good page as it is, and to write the counts of the
    page whose csv changed so the check then reads it clean with every line outside the counts as
    it was. The folder is removed when the proof ends, a failed step included.

    Each run of the check and the maker is its own powershell.exe, so the exit code is the one
    Actions sees. Exits 0 when every case is right and 1 otherwise. Windows PowerShell 5.1.

    Usage:
        powershell -NoProfile -ExecutionPolicy Bypass -File tools\tracker\prove-progress.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$check = Join-Path $PSScriptRoot "check-progress.ps1"
$make = Join-Path $PSScriptRoot "make-tracker.ps1"
$fixtures = Join-Path $PSScriptRoot "progress-fixtures"
$start = "<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->"
$end = "<!-- the end of the counts -->"
$notThere = "is not there under {root}, so nothing was checked."
$notMade = "is not what make-tracker.ps1 makes from tracker.csv, run it and commit what it writes"
$notUtf8 = "holds bytes that are not UTF-8, so the file was not read"

function Want([int] $Code, [string[]] $Lines) { return @{ Code = $Code; Lines = $Lines } }

$cases = [ordered]@{
    "good"             = Want 0 @()
    "page-at-60"       = Want 0 @()
    "page-over-60"     = Want 1 @("PROGRESS.md is 61 lines, and its rule allows at most 60")
    "block-typed"      = Want 1 @("PROGRESS.md line 11 $notMade")
    "csv-changed"      = Want 1 @("PROGRESS.md line 12 $notMade")
    "csv-fault"        = Want 1 @("the counts of PROGRESS.md were not compared, since tracker.csv has 1 fault, which check-tracker.ps1 names")
    "start-missing"    = Want 1 @("PROGRESS.md has no line reading $start, the line above the counts")
    "end-missing"      = Want 1 @("PROGRESS.md has no line reading $end, the line below the counts")
    "start-twice"      = Want 1 @("PROGRESS.md line 17 is the line above the counts a second time, the first at line 3")
    "end-twice"        = Want 1 @("PROGRESS.md line 20 is the line below the counts a second time, the first at line 15")
    "end-before-start" = Want 1 @("PROGRESS.md line 3, the line below the counts, comes before line 15, the line above them")
    "page-missing"     = Want 2 @("check-progress: steps\PROGRESS.md $notThere")
    "csv-missing"      = Want 2 @("check-progress: steps\tracker.csv $notThere")
}

$wrong = 0
$right = 0
$found = @(Get-ChildItem -LiteralPath $fixtures -Directory | ForEach-Object { $_.Name })
foreach ($name in $found) {
    if (-not $cases.Contains($name)) { Write-Output "WRONG  progress-fixtures\$name has no case here, so nothing proves it"; $wrong++ }
}

function Run-Check([string] $Root) {
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $check -Root $Root)
    return @{ Said = $said; Code = $LASTEXITCODE }
}

function Judge([string] $Name, $Got, [int] $Code, [string[]] $Want) {
    $compared = @(if ($Code -eq 2) { $Got.Said } else { $Got.Said | Where-Object { -not "$_".StartsWith("check-progress:") } })
    $same = ($Got.Code -eq $Code) -and ($compared.Count -eq $Want.Count)
    if ($same) { for ($k = 0; $k -lt $Want.Count; $k++) { if ($compared[$k] -cne $Want[$k]) { $same = $false } } }
    if ($same) {
        Write-Output "RIGHT  $Name  exit $($Got.Code), $($compared.Count) line(s) as written"
        $script:right++
    } else {
        Write-Output "WRONG  $Name  exit $($Got.Code) with $Code wanted, and these lines where the lines written here were wanted:"
        $Got.Said | ForEach-Object { Write-Output "         $_" }
        $script:wrong++
    }
}

foreach ($name in $cases.Keys) {
    $root = Join-Path $fixtures $name
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { Write-Output "WRONG  $name  the fixture is not there"; $wrong++; continue }
    $case = $cases[$name]
    Judge $name (Run-Check $root) $case.Code @($case.Lines | ForEach-Object { $_.Replace("{root}", $root) })
}

function Copy-Fixture([string] $Name, [string] $To) {
    New-Item -ItemType Directory -Path (Join-Path $To "steps") | Out-Null
    foreach ($file in @("tracker.csv", "PROGRESS.md")) {
        $from = Join-Path $fixtures "$Name\steps\$file"
        if (Test-Path -LiteralPath $from) { Copy-Item -LiteralPath $from -Destination (Join-Path $To "steps\$file") }
    }
}

function Read-Lf([string] $Path) { return [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8).Replace("`r`n", "`n") }

function Bytes-Of([string] $Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return "" }
    return [Convert]::ToBase64String([IO.File]::ReadAllBytes($Path))
}

# The maker refuses and writes nothing: exit 1, these exact lines, no tracker.md, PROGRESS.md as before.
function Refused([string] $Name, [string] $Root, [string[]] $Want) {
    $page = Join-Path $Root "steps\PROGRESS.md"
    $md = Join-Path $Root "steps\tracker.md"
    $before = Bytes-Of $page
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $make -Root $Root)
    $code = $LASTEXITCODE
    $same = ($code -eq 1) -and ($said.Count -eq $Want.Count) -and -not (Test-Path -LiteralPath $md) -and ((Bytes-Of $page) -ceq $before)
    if ($same) { for ($k = 0; $k -lt $Want.Count; $k++) { if ($said[$k] -cne $Want[$k]) { $same = $false } } }
    if ($same) {
        Write-Output "RIGHT  make-tracker refused $Name, exit 1, $($said.Count) line(s) as written, and wrote nothing"
        $script:right++
    } else {
        Write-Output ("WRONG  make-tracker answered $code on $Name with 1 wanted, tracker.md written " + (Test-Path -LiteralPath $md) + ", PROGRESS.md as before " + ((Bytes-Of $page) -ceq $before) + ", and it said:")
        $said | ForEach-Object { Write-Output "         $_" }
        $script:wrong++
    }
}

$work = Join-Path ([IO.Path]::GetTempPath()) ("prove-progress-" + [Guid]::NewGuid().ToString("N"))
try {
    # The good page with CRLF line ends and a byte order mark reads clean.
    $crlf = Join-Path $work "crlf"
    Copy-Fixture "good" $crlf
    foreach ($file in @("tracker.csv", "PROGRESS.md")) {
        $path = Join-Path $crlf "steps\$file"
        [IO.File]::WriteAllText($path, (Read-Lf $path).Replace("`n", "`r`n"), (New-Object System.Text.UTF8Encoding $true))
    }
    Judge "good with CRLF and a byte order mark" (Run-Check $crlf) 0 @()

    # A page holding a byte that is not UTF-8 on its line 18 is refused by the check.
    $wide = Join-Path $work "not-utf8"
    Copy-Fixture "good" $wide
    $path = Join-Path $wide "steps\PROGRESS.md"
    $text = (Read-Lf $path).Replace("- the one lane of this fixture, at work", "- the one lane of this fixture, at work XX")
    $bytes = [Text.Encoding]::UTF8.GetBytes($text)
    $at = [Text.Encoding]::UTF8.GetBytes($text.Substring(0, $text.IndexOf(" XX"))).Length
    $bytes[$at + 1] = 0xE9
    [IO.File]::WriteAllBytes($path, $bytes)
    Judge "a page that is not UTF-8" (Run-Check $wide) 1 @("PROGRESS.md line 18 $notUtf8")

    # The maker refuses a folder with no PROGRESS.md and writes nothing.
    $none = Join-Path $work "no-page"
    Copy-Fixture "page-missing" $none
    Refused "a folder with no PROGRESS.md" $none @("make-tracker: REFUSED. " + (Join-Path $none "steps\PROGRESS.md") + " is not there, so nothing was written.")

    # The maker refuses each page whose marker lines are wrong, and writes nothing.
    $marks = [ordered]@{
        "start-missing"    = "PROGRESS.md has no line reading $start, the line above the counts"
        "end-missing"      = "PROGRESS.md has no line reading $end, the line below the counts"
        "start-twice"      = "PROGRESS.md line 17 is the line above the counts a second time, the first at line 3"
        "end-twice"        = "PROGRESS.md line 20 is the line below the counts a second time, the first at line 15"
        "end-before-start" = "PROGRESS.md line 3, the line below the counts, comes before line 15, the line above them"
    }
    foreach ($name in $marks.Keys) {
        $root = Join-Path $work "make-$name"
        Copy-Fixture $name $root
        Refused "the page of $name" $root @($marks[$name], "make-tracker: REFUSED. 1 fault in PROGRESS.md above, so neither tracker.md nor PROGRESS.md was written.")
    }

    # The maker refuses a page that is not UTF-8, and writes nothing.
    $root = Join-Path $work "make-not-utf8"
    Copy-Fixture "good" $root
    Copy-Item -LiteralPath (Join-Path $wide "steps\PROGRESS.md") -Destination (Join-Path $root "steps\PROGRESS.md") -Force
    Refused "a page that is not UTF-8" $root @("PROGRESS.md line 18 $notUtf8", "make-tracker: REFUSED. 1 fault in PROGRESS.md above, so neither tracker.md nor PROGRESS.md was written.")

    # The maker leaves the good page as it is.
    $root = Join-Path $work "make-good"
    Copy-Fixture "good" $root
    $before = Read-Lf (Join-Path $root "steps\PROGRESS.md")
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $make -Root $root)
    $code = $LASTEXITCODE
    if ($code -eq 0 -and (Test-Path -LiteralPath (Join-Path $root "steps\tracker.md")) -and (Read-Lf (Join-Path $root "steps\PROGRESS.md")) -ceq $before) {
        Write-Output "RIGHT  make-tracker left the good page as it is, exit 0, and wrote tracker.md"
        $right++
    } else {
        Write-Output "WRONG  make-tracker answered $code on the good page, or changed it, or wrote no tracker.md, and it said:"
        $said | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }

    # The maker writes the counts of the page whose csv changed, the check then reads it clean,
    # and every line outside the counts is as it was.
    $root = Join-Path $work "make-csv-changed"
    Copy-Fixture "csv-changed" $root
    $page = Join-Path $root "steps\PROGRESS.md"
    $was = (Read-Lf $page).Split("`n")
    $said = @(& powershell -NoProfile -ExecutionPolicy Bypass -File $make -Root $root)
    $code = $LASTEXITCODE
    $now = (Read-Lf $page).Split("`n")
    $outside = ($was.Count -eq $now.Count)
    if ($outside) { foreach ($k in @(0..2) + @(14..($was.Count - 1))) { if ($was[$k] -cne $now[$k]) { $outside = $false } } }
    $after = Run-Check $root
    $faults = @($after.Said | Where-Object { -not "$_".StartsWith("check-progress:") })
    if ($code -eq 0 -and $outside -and $after.Code -eq 0 -and $faults.Count -eq 0 -and $now[11] -ceq "| 2b | 1 | 0 | 1 | 0 | 0 | 0 | 2 |") {
        Write-Output "RIGHT  make-tracker wrote the counts of the changed csv, the check reads the page clean, and no line outside the counts moved"
        $right++
    } else {
        Write-Output "WRONG  make-tracker answered $code on the changed csv, lines outside the counts as they were $outside, the check after it $($after.Code), and they said:"
        ($said + $after.Said) | ForEach-Object { Write-Output "         $_" }
        $wrong++
    }
} finally {
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
}

if ($wrong -gt 0) { Write-Output "prove-progress: $wrong case(s) WRONG, $right right"; exit 1 }
Write-Output "prove-progress: all $right cases right"
exit 0
