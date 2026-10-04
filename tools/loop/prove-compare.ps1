<#
    Proves compare-document.ps1, in prove-hooks.sh's shape: every case is fed to the
    comparison and what it answered is printed against what it should answer. F104.

    THE PAIR is tools\loop\compare-proof, a workbook read-out and a document read-out written
    by hand in the shapes read-workbook.ps1 and DocumentReadProbe write: three tests, one of
    plain clashes with two pictures, one holding a group whose clashes sit at two statuses
    beside a plain clash and whose name ends in a space, and one that found nothing. The
    workbook is what WorkbookWriter writes for that document, so the group's two clashes are
    filed under the group's status, ClashHarvest.cs 166 and ClashReportModel.cs 396. The
    document is in feet at 0.3048 metres per foot, so every metre is converted. No workbook,
    no NWF and no picture is behind it. Under -GroupClashesAt unknown, the default, the good
    pair reads DISAGREEMENTS 0 and NOT PROVED, with three NOT COMPARED lines: the mixed test's
    New and Reviewed, the same two in the totals, which PQ4 decides, and the pictures on disk.

    THE CASES are sixteen broken copies, each one edit away from the good pair, and each
    has to produce exactly the lines written beside it here and no other line the good pair
    does not have, and lose only the lines of the good pair written beside it. A copy whose
    edit does not find exactly the line it edits is WRONG, so an edit that stopped matching
    the pair can never pass quietly. Then the three readings of a group's clashes, an empty
    group, a test name on two blocks and the picture numbering after it, and four runs that
    prove the switches and the one class of test that is counted rather than judged.

    The copies and the comparisons are written into a new folder under
    %LOCALAPPDATA%\NwcFederatorLoop, by default proof\compare-<stamp>, never emptied and
    never reused. Exits 0 when every case is right and 1 otherwise.

    Usage:
        powershell -ExecutionPolicy Bypass -File tools\loop\prove-compare.ps1 [-Work <new folder under %LOCALAPPDATA%\NwcFederatorLoop>]
#>

[CmdletBinding()]
param([string] $Work)

$ErrorActionPreference = "Stop"

$root = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"))
if (-not $Work) { $Work = Join-Path $root ("proof\compare-" + (Get-Date -Format "yyyyMMdd-HHmmss")) }
$workFull = [IO.Path]::GetFullPath($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Work))
if (-not $workFull.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { Write-Host "REFUSED: -Work is not under $root. Nothing was written."; exit 1 }
if (Test-Path -LiteralPath $workFull) { Write-Host "REFUSED: $workFull is there already, and a proof folder is never reused. Nothing was written."; exit 1 }

$compare = Join-Path $PSScriptRoot "compare-document.ps1"
$pair = Join-Path $PSScriptRoot "compare-proof"
$goodWb = [IO.File]::ReadAllLines((Join-Path $pair "proof-group-workbook.txt"))
$goodDoc = [IO.File]::ReadAllLines((Join-Path $pair "proof-group-document.txt"))
New-Item -ItemType Directory -Path $workFull | Out-Null

$A = "Architecture vs Structure"
$M = "Mechanical vs Plumbing "
$E = "Electrical vs Fire"
$wbTests = "-- tests: "
$wbRows = "-- clash rows: "
$docTests = "-- tests: "
$docResults = "-- results: "
$pictures = @{ PictureStatuses = @("New", "Active") }

# The rows of the one table under $Header, the table running to the first empty line.
function Get-Table([string[]] $Lines, [string] $Header) {
    $start = -1
    for ($i = 0; $i -lt $Lines.Count; $i++) {
        if ($Lines[$i].StartsWith($Header)) {
            if ($start -ge 0) { throw "two tables start with [$Header]" }
            $start = $i
        }
    }
    if ($start -lt 0) { throw "no table starts with [$Header]" }
    $end = $start
    while ($end + 1 -lt $Lines.Count -and $Lines[$end + 1] -ne "") { $end++ }
    return [pscustomobject]@{ Start = $start; End = $end; Words = @($Lines[$start].Substring($Header.Length) -split ', ') }
}

function Get-Column($table, [string] $word) {
    $c = [array]::IndexOf($table.Words, $word)
    if ($c -lt 0) { throw "the table has no column [$word]" }
    return $c
}

# One row, found by one column reading one value exactly, changed or taken out. Exactly
# one row has to match, or the edit is refused.
function Edit-Row([string[]] $Lines, [string] $Header, [string] $Key, [string] $Value, [hashtable] $Set, [switch] $Remove) {
    $t = Get-Table $Lines $Header
    $k = Get-Column $t $Key
    $out = New-Object System.Collections.Generic.List[string]
    $hits = 0
    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]
        if ($i -gt $t.Start -and $i -le $t.End) {
            $f = $line.Split([char]9)
            if ($f[$k] -ceq $Value) {
                $hits++
                if ($Remove) { continue }
                foreach ($w in $Set.Keys) { $f[(Get-Column $t $w)] = $Set[$w] }
                $line = $f -join "`t"
            }
        }
        $out.Add($line)
    }
    if ($hits -ne 1) { throw "the edit found $hits rows whose $Key reads [$Value], where it needs exactly one" }
    return ,$out.ToArray()
}

function Switch-Rows([string[]] $Lines, [string] $Header, [string] $Key, [string] $First, [string] $Second) {
    $t = Get-Table $Lines $Header
    $k = Get-Column $t $Key
    $at = @{}
    for ($i = $t.Start + 1; $i -le $t.End; $i++) {
        $v = $Lines[$i].Split([char]9)[$k]
        if ($v -ceq $First -or $v -ceq $Second) {
            if ($at.ContainsKey($v)) { throw "two rows whose $Key reads [$v]" }
            $at[$v] = $i
        }
    }
    if ($at.Count -ne 2) { throw "the swap found $($at.Count) of its two rows" }
    $copy = [string[]]$Lines.Clone()
    $copy[$at[$First]] = $Lines[$at[$Second]]
    $copy[$at[$Second]] = $Lines[$at[$First]]
    return ,$copy
}

# Text changed in one line found by how it starts, where the text is there exactly once.
function Edit-Text([string[]] $Lines, [string] $Starts, [string] $From, [string] $To) {
    $copy = [string[]]$Lines.Clone()
    $hits = @(for ($i = 0; $i -lt $Lines.Count; $i++) { if ($Lines[$i].StartsWith($Starts)) { $i } })
    if ($hits.Count -ne 1) { throw "the edit found $($hits.Count) lines starting [$Starts], where it needs exactly one" }
    $line = $Lines[$hits[0]]
    if (($line.Length - $line.Replace($From, "").Length) / $From.Length -ne 1) { throw "the line starting [$Starts] does not hold [$From] exactly once" }
    $copy[$hits[0]] = $line.Replace($From, $To)
    return ,$copy
}

# A name changed wherever it stands as a whole column, in every table of the read-out.
function Rename-Everywhere([string[]] $Lines, [string] $From, [string] $To) {
    $hits = 0
    $copy = foreach ($line in $Lines) {
        $f = $line.Split([char]9)
        if ($f.Count -gt 1 -and ($f -ccontains $From)) {
            $hits++
            (($f | ForEach-Object { if ($_ -ceq $From) { $To } else { $_ } }) -join "`t")
        } else { $line }
    }
    if ($hits -eq 0) { throw "no line holds [$From] as a column" }
    return ,[string[]]$copy
}

function Remove-Last([string[]] $Lines, [string] $Text) {
    if ($Lines[$Lines.Count - 1] -cne $Text) { throw "the last line is not [$Text]" }
    return ,[string[]]($Lines[0..($Lines.Count - 2)])
}

$seq = 0
function Invoke-Compare([string] $slug, [string[]] $wb, [string[]] $doc, [hashtable] $switches) {
    $script:seq++
    $stem = Join-Path $workFull ("{0:00}-{1}" -f $script:seq, $slug)
    [IO.File]::WriteAllLines("$stem-workbook.txt", $wb)
    [IO.File]::WriteAllLines("$stem-document.txt", $doc)
    $s = if ($switches) { $switches } else { @{} }
    & $compare -Workbook "$stem-workbook.txt" -Document "$stem-document.txt" -Out "$stem-compare.txt" @s 6>$null
    $code = $LASTEXITCODE
    $all = if (Test-Path -LiteralPath "$stem-compare.txt") { [IO.File]::ReadAllLines("$stem-compare.txt") } else { @() }
    # A finding line is one of these words and what it found. NOT COMPARED and a number
    # alone is the count line near the end, not a finding.
    $found = @($all | Where-Object { ($_.StartsWith("DIFFERS ") -or $_.StartsWith("NOT COMPARED ") -or $_.StartsWith("DOUBT ") -or $_.StartsWith("COMPARISON FAILED")) -and $_ -notmatch '^NOT COMPARED [0-9]+$' })
    $verdict = @($all | Where-Object { $_.StartsWith("VERDICT ") })
    return [pscustomobject]@{ Code = $code; All = $all; Found = $found; Verdict = $(if ($verdict.Count) { $verdict[0] } else { "no verdict" }) }
}

$results = New-Object System.Collections.Generic.List[string]
function Write-Case([bool] $right, [string] $title, [string[]] $show, [string[]] $wrong) {
    $mark = if ($right) { "ok   " } else { "WRONG" }
    $results.Add($mark)
    Write-Output ("{0} {1}" -f $mark, $title)
    foreach ($l in $show) { Write-Output ("        " + $l) }
    foreach ($l in $wrong) { Write-Output ("        WRONG: " + $l) }
}

Write-Output "prove-compare, the copies and the comparisons are in $workFull"
Write-Output ""
Write-Output "== the good pair, -GroupClashesAt unknown by default"
$good = Invoke-Compare "good" $goodWb $goodDoc $pictures
$goodFound = $good.Found
$pq4Test = "NOT COMPARED [$M]  New and Reviewed, which fit a group's clashes counted at the group's status, where group [Group1] is at Reviewed and holds clashes at New 1, Reviewed 1, and PQ4 decides which count the panel shows"
$pq4Totals = "NOT COMPARED totals  New and Reviewed, which fit a group's clashes counted at the group's status, where [$M] holds a group whose clashes sit at more than one status, and PQ4 decides which count the panel shows"
$expectGood = @($pq4Test, $pq4Totals, "NOT COMPARED pictures on disk, the workbook read-out names proof-group.xlsx, which is not a folder on this machine")
$wrong = @()
if ($good.Code -ne 0) { $wrong += "exit $($good.Code), where 0 is wanted" }
if ($good.Verdict -cne "VERDICT NOT PROVED") { $wrong += "$($good.Verdict), where VERDICT NOT PROVED is wanted" }
if (($goodFound -join "`n") -cne ($expectGood -join "`n")) { $wrong += "its findings are [" + ($goodFound -join " | ") + "]" }
foreach ($c in @("DISAGREEMENTS 0", "NOT COMPARED 3", "DOUBTS 0")) { if ($good.All -cnotcontains $c) { $wrong += "no line reads $c" } }
Write-Case ($wrong.Count -eq 0) "the good pair reads DISAGREEMENTS 0, NOT COMPARED 3, DOUBTS 0 and NOT PROVED" (@($goodFound) + @($good.Verdict)) $wrong

# One case: the copy, the lines it must add to the good pair's and the ones it must lose,
# the verdict and the exit.
function Test-Case([string] $title, [scriptblock] $wb, [scriptblock] $doc, [string[]] $expect, [string] $verdict, [int] $exit = 0, [hashtable] $switches = $pictures, [string[]] $mustHave = @(), [string[]] $mustLose = @()) {
    $wrong = @()
    $r = $null
    try {
        $wbCopy = if ($wb) { & $wb } else { $goodWb }
        $docCopy = if ($doc) { & $doc } else { $goodDoc }
        $r = Invoke-Compare (($title -replace '[^A-Za-z0-9]+', '-').Trim('-').ToLowerInvariant()) $wbCopy $docCopy $switches
    }
    catch {
        Write-Case $false $title @() @("the case could not be run: " + $_.Exception.Message)
        return
    }
    $new = @($r.Found | Where-Object { $goodFound -cnotcontains $_ })
    $gone = @($goodFound | Where-Object { $r.Found -cnotcontains $_ })
    foreach ($e in $expect) {
        $n = @($new | Where-Object { $_ -ceq $e }).Count
        if ($n -ne 1) { $wrong += "wanted once, found $n times: $e" }
    }
    foreach ($l in $new) { if ($expect -cnotcontains $l) { $wrong += "a line this case should not cause: $l" } }
    if ($exit -eq 0) {
        foreach ($l in $gone) { if ($mustLose -cnotcontains $l) { $wrong += "a line of the good pair went missing: $l" } }
        foreach ($l in $mustLose) { if ($gone -cnotcontains $l) { $wrong += "a line of the good pair should have gone and did not: $l" } }
    }
    if ($r.Code -ne $exit) { $wrong += "exit $($r.Code), where $exit is wanted" }
    if ($verdict -and $r.Verdict -cne "VERDICT $verdict") { $wrong += "$($r.Verdict), where VERDICT $verdict is wanted" }
    foreach ($h in $mustHave) { if ($r.All -cnotcontains $h) { $wrong += "no line reads: $h" } }
    $show = @($new) + @($mustHave | Where-Object { $r.All -ccontains $_ }) + @($mustLose | Where-Object { $gone -ccontains $_ } | ForEach-Object { "gone: $_" })
    if ($verdict) { $show += $r.Verdict }
    Write-Case ($wrong.Count -eq 0) $title $show $wrong
}

Write-Output ""
Write-Output "== sixteen broken copies, each one edit away from the good pair"

Test-Case "01 New raised by one" { Edit-Row $goodWb $wbTests "test" $A -Set @{ new = "2" } } $null @(
    "DIFFERS [$A]  New: workbook 2, document 1 clashes at New (top level 1)",
    "DIFFERS [$A]  New to Resolved add to 4 and Clashes reads 3",
    "DIFFERS totals  New and Reviewed: workbook New 2, Reviewed 2, document New 2, Reviewed 1 with each clash at its own status and New 1, Reviewed 2 with a group's clashes at the group's status, and the workbook fits neither") "DISAGREE" 0 $pictures @() @(
    $pq4Totals)

Test-Case "02 Clashes lowered by one" { Edit-Row $goodWb $wbTests "test" $M -Set @{ clashes = "2" } } $null @(
    "DIFFERS [$M]  Clashes: workbook 2, document 3 clashes",
    "DIFFERS [$M]  New to Resolved add to 3 and Clashes reads 2",
    "DIFFERS totals  Clashes: workbook 5, document 6 clashes") "DISAGREE"

Test-Case "03 a row removed" { Edit-Row $goodWb $wbRows "clash name" "Clash3" -Remove } $null @(
    "DIFFERS [$A]  rows: workbook 2, document 3 top level results",
    "DIFFERS [$A]  row 3: workbook none, document [Clash3] Resolved") "DISAGREE"

Test-Case "04 two rows swapped" { Switch-Rows $goodWb $wbRows "clash name" "Group1" "Clash5" } $null @(
    "DIFFERS [$M]  row 1: workbook [Clash5] Approved, document [Group1] Reviewed",
    "DIFFERS [$M]  row 2: workbook [Group1] Reviewed, document [Clash5] Approved") "DISAGREE"

Test-Case "05 a picture link given the next number" { Edit-Row $goodWb $wbRows "clash name" "Clash2" -Set @{ "picture link" = "proof-group_files/cd000003.jpg" } } $null @(
    "DIFFERS [$A]  row 2 [Clash2] picture link: workbook proof-group_files/cd000003.jpg, the export's numbering gives proof-group_files/cd000002.jpg") "DISAGREE"

Test-Case "06 a New row's link removed, with -PictureStatuses New,Active" { Edit-Row $goodWb $wbRows "clash name" "Clash2" -Set @{ "picture link" = "" } } $null @(
    "DIFFERS [$A]  row 2 [Clash2] is at New and carries no picture link") "DISAGREE"

Test-Case "07 a tolerance label ft" { Edit-Row $goodWb $wbTests "test" $A -Set @{ tolerance = "0.025ft" } } $null @(
    "DIFFERS [$A]  tolerance: workbook 0.025ft, the unit is not m, so the number is not compared") "DISAGREE"

Test-Case "08 a tolerance off by 0.001" { Edit-Row $goodWb $wbTests "test" $A -Set @{ tolerance = "0.026m" } } $null @(
    "DIFFERS [$A]  tolerance: workbook 0.026m, document 0.025 m") "DISAGREE"

Test-Case "09 a distance left in feet" { Edit-Row $goodWb $wbRows "clash name" "Clash1" -Set @{ distance = "-0.164" } } $null @(
    "DIFFERS [$A]  row 1 [Clash1] distance: workbook -0.164 m, document -0.05 m") "DISAGREE"

Test-Case "10 a test line removed from the workbook" { Edit-Row $goodWb $wbTests "test" $E -Remove } $null @(
    "DIFFERS [$E]  in the document only, 0 clashes") "DISAGREE"

Test-Case "11 a trailing space dropped from a test name, both sides named" { Rename-Everywhere $goodWb $M $M.TrimEnd() } $null @(
    "DIFFERS [$($M.TrimEnd())]  in the workbook only, Clashes 3, New 0, Active 0, Reviewed 2, Approved 1, Resolved 0, 2 rows",
    "DIFFERS [$M]  in the document only, 3 clashes") "DISAGREE" 0 $pictures @() @(
    $pq4Test)

Test-Case "12 two equal blocks swapped" { Switch-Rows $goodWb $wbTests "test" $A $M } $null @(
    "DIFFERS block order: [$A] comes after [$M], both Clashes 3, and the document holds [$A] first") "DISAGREE"

Test-Case "13 one result's status changed in the document read-out" $null { Edit-Row $goodDoc $docResults "name" "Clash3" -Set @{ status = "Active" } } @(
    "DIFFERS [$A]  row 3: workbook [Clash3] Resolved, document [Clash3] Active") "DISAGREE"

$minusOne = @{}
foreach ($w in @("top level", "top new", "top active", "top reviewed", "top approved", "top resolved", "leaves", "new", "active", "reviewed", "approved", "resolved", "groups", "empty groups", "nested groups", "other")) { $minusOne[$w] = "-1" }
Test-Case "14 a test's counts -1 in the document read-out" $null { Edit-Row $goodDoc $docTests "test" $A -Set $minusOne } @(
    "NOT COMPARED [$A]  the document read-out counts this test as -1, UNKNOWN, so its rows, clashes, statuses and distances are not compared",
    "NOT COMPARED totals, the document read-out holds a test it could not count") "NOT PROVED" 0 $pictures @() @(
    $pq4Totals)

Test-Case "15 pass 2 totals differing from pass 1" $null { Edit-Text $goodDoc "pass 2`t" "leaves 6," "leaves 7," } @(
    "DOUBT the document read-out's two passes differ, leaves 6 then 7") "NOT PROVED"

Test-Case "16 END OF READ-OUT removed" $null { Remove-Last $goodDoc "END OF READ-OUT" } @(
    "COMPARISON FAILED: the document read-out does not end in END OF READ-OUT") "" 1

Write-Output ""
Write-Output "== the three readings of a group's clashes, which PQ4 decides"

Test-Case "17 -GroupClashesAt unknown, given by name, reads as the good pair" $null $null @() "NOT PROVED" 0 @{ PictureStatuses = @("New", "Active"); GroupClashesAt = "unknown" } @(
    $pq4Test, $pq4Totals)

Test-Case "18 -GroupClashesAt own, each clash at its own status" $null $null @(
    "DIFFERS [$M]  New: workbook 0, document 1 clashes at New (top level 0)",
    "DIFFERS [$M]  Reviewed: workbook 2, document 1 clashes at Reviewed (top level 1)",
    "DIFFERS totals  New: workbook 1, document 2 clashes at New",
    "DIFFERS totals  Reviewed: workbook 2, document 1 clashes at Reviewed") "DISAGREE" 0 @{ PictureStatuses = @("New", "Active"); GroupClashesAt = "own" } @() @(
    $pq4Test, $pq4Totals)

Test-Case "19 -GroupClashesAt group, a group's clashes at the group's status" $null $null @() "NOT PROVED" 0 @{ PictureStatuses = @("New", "Active"); GroupClashesAt = "group" } @() @(
    $pq4Test, $pq4Totals)

Write-Output ""
Write-Output "== an empty group, and a test name on two blocks"

Test-Case "20 an empty group, which the harvest counts as one clash" $null {
    $d = Edit-Row $goodDoc $docTests "test" $A -Set @{ leaves = "2"; resolved = "0"; groups = "1"; "empty groups" = "1" }
    Edit-Row $d $docResults "name" "Clash3" -Set @{ kind = "group"; leaves = "0"; resolved = "0"; distance = "-"; "distance m" = "-" } } @(
    "DIFFERS [$A]  Clashes: workbook 3, document 2 clashes, and the document's [Clash3] is an empty group, which holds no clash",
    "DIFFERS [$A]  Resolved: workbook 1, document 0 clashes at Resolved (top level 1), and the document's [Clash3] is an empty group, which holds no clash",
    "DIFFERS totals  Clashes: workbook 6, document 5 clashes",
    "DIFFERS totals  Resolved: workbook 1, document 0 clashes at Resolved") "DISAGREE"

Test-Case "21 a test name on two blocks, and the picture numbering after it" {
    $w = Edit-Row $goodWb $wbTests "test" $E -Set @{ test = $A }
    $w = Edit-Row $w $wbRows "clash name" "Group1" -Set @{ "picture link" = "proof-group_files/cd010001.jpg" }
    Edit-Row $w $wbRows "clash name" "Clash5" -Set @{ "picture link" = "proof-group_files/cd010003.jpg" } } $null @(
    "DOUBT [$A] is the name of 2 workbook tests, and no copy is compared",
    "DIFFERS [$E]  in the document only, 0 clashes",
    "NOT COMPARED picture numbering of [$A], the name is on 2 workbook blocks and which of them its 2 picture links belong to cannot be known, so those 2 tests are not checked",
    "NOT COMPARED [$M]  the test number in its picture links, 1, can only be held to 0 to 1 after a test name on more than one block, and its clash numbers are checked",
    "DIFFERS [$M]  row 2 [Clash5] picture link: workbook proof-group_files/cd010003.jpg, the export's numbering gives proof-group_files/cd010002.jpg") "DISAGREE" 0 @{ PictureStatuses = @("New", "Active", "Reviewed", "Approved") }

Write-Output ""
Write-Output "== the switches, and the one class of test that is counted rather than judged"

Test-Case "the good pair with no -PictureStatuses" $null $null @(
    "NOT COMPARED which rows should carry a picture, -PictureStatuses was not given") "NOT PROVED" 0 @{}

Test-Case "the good pair with -PictureCap 1" $null $null @(
    "DIFFERS [$A]  row 2 [Clash2] carries a picture link past the cap of 1") "DISAGREE" 0 @{ PictureStatuses = @("New", "Active"); PictureCap = 1 }

Test-Case "copy 12 with -PriorityPicked, the order is chosen and not judged" { Switch-Rows $goodWb $wbTests "test" $A $M } $null @() "NOT PROVED" 0 @{ PictureStatuses = @("New", "Active"); PriorityPicked = $true } @(
    "priority picked    yes, so the block order is chosen and not judged")

Test-Case "the test that found nothing taken out of the document read-out" $null { Edit-Row $goodDoc $docTests "test" $E -Remove } @() "NOT PROVED" 0 $pictures @(
    "WORKBOOK ONLY, EVERY NUMBER ZERO [$E]  a test the run did not create, counted and not judged",
    "workbook tests with every number zero and no document test, counted and not judged: 1")

$right = @($results | Where-Object { $_ -eq "ok   " }).Count
$wrongCount = @($results | Where-Object { $_ -eq "WRONG" }).Count
Write-Output ""
Write-Output "cases right: $right, cases wrong: $wrongCount"
if ($wrongCount -gt 0 -or $right -ne 26) { exit 1 }
exit 0
