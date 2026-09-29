<#
    Sets a workbook's read-out beside the read-out of the document it was written from and
    writes down, test by test and row by row, where the two agree and where they part. F104,
    Bader's criterion 3: the workbook's numbers match what Clash Detective holds, test by
    test and status by status, every clash row has its picture in the order of the
    Navisworks export, and the units say metres.

    THREE PIECES THAT SHARE NO CODE. How a workbook is read stays in read-workbook.ps1, which
    opens the xlsx as a zip. How a document is read is tools\probes\DocumentReadProbe, which
    reads the NWF through the Navisworks API alone and converts through Navisworks' own
    UnitConversion.ScaleFactor. What agreeing means is here, and only the two read-out files
    pass between them, so this script is proved with no Navisworks and no xlsx, by
    prove-compare.ps1. A check that shares the code it checks proves nothing.

    WHAT IT CHECKS, for every test in both read-outs, paired by name, Ordinal, never trimmed.
    Every count is exact and a difference of one is a line.

        1  the rows under the block against the document's top level results
        2  Clashes against the clashes in the document, every clash under a group counted
        3  each of New to Resolved against the clashes at that status, the top level count
           shown in brackets. Each clash is counted at its own status, see GroupClashesAt
        4  row by row in sheet order against the top level results in document order,
           name then status
        5  the tolerance unit reads exactly m on every workbook test, and the number is
           within 0.0005 of the document's in metres, the workbook's rounding and nothing else
        6  a plain clash row's distance within 0.0005 of the document's in metres, which is
           what catches a row left in feet under a label reading m. Only where check 4 found
           the same name, because a distance beside another result's name means nothing
        7  the pictures, worked out from the workbook read-out alone and tied to the document
           through check 4: blocks in workbook order, the test number counting blocks with a
           picture link from 0, the clash number counting pictured rows from 1, every link
           <workbook name>_files/cd<test 00><clash 0000>.jpg, scan.md 4k, and every one on
           disk. With -PictureStatuses every row at those statuses carries a link and no
           other row does, with -PictureCap only the first N of them in each test
        8  the block order unless -PriorityPicked: among blocks with a clash, Clashes never
           rises down the sheet and equal Clashes keep the document's order. Blocks with no
           clash are not placed
        9  the totals against the document's, and inside every workbook test New to
           Resolved add up to Clashes

    A test in the workbook only whose every number is zero is one F77 did not create, and is
    counted, not judged. In the workbook only with a number above zero, or in the document
    only, is a line. A name on two tests on one side is a doubt and neither copy is compared.

    Shown and never judged: the test type and the test status, because the workbook carries
    the client's words and judging them would need a second copy of that wording rule, and
    the document's models, sets and viewpoints, for the log-reader beside the census lines.

    THE VERDICT. AGREE only when DISAGREEMENTS, NOT COMPARED and DOUBTS are all zero.
    Every finding is one line starting DIFFERS, NOT COMPARED or DOUBT and saying what it
    found, and the three count lines near the end are the word and a number alone.
    DISAGREE when any line differs. NOT PROVED otherwise. A count of -1 in the document
    read-out is UNKNOWN and never zero, so it is NOT COMPARED.

    It refuses with one COMPARISON FAILED line when either read-out lacks END OF READ-OUT or
    says READ-OUT FAILED or REFUSED, when a column word it reads is missing, or when -Out is
    not a new .txt under %LOCALAPPDATA%\NwcFederatorLoop or names an input. The comparison is
    written beside -Out and moved into place, and never written over one that is there.
    Exits 0 when a comparison was written, whatever its verdict, and 1 when it refused.

    Usage:
        powershell -ExecutionPolicy Bypass -File tools\loop\compare-document.ps1 -Workbook <read-out> -Document <read-out> -Out <.txt> [-PictureStatuses New,Active,Reviewed] [-PictureCap N] [-PriorityPicked]
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Workbook,
    [Parameter(Mandatory = $true)] [string] $Document,
    [Parameter(Mandatory = $true)] [string] $Out,
    [string[]] $PictureStatuses,
    [int] $PictureCap = 0,
    [switch] $PriorityPicked
)

$ErrorActionPreference = "Stop"

# THE ONE SWITCH that waits on PQ4 and PQ5. "own" counts every clash under a group at the
# clash's own status, which is what the document holds. If the panel and Clash Detective's
# own export file a group's clashes under the group's status, this becomes "group" and
# nothing else moves, the probe included.
$GroupClashesAt = "own"

$Statuses = @("New", "Active", "Reviewed", "Approved", "Resolved")
$Within = 0.0005
$Slack = 1e-9
$Invariant = [Globalization.CultureInfo]::InvariantCulture
$End = "END OF READ-OUT"

$WorkbookTestWords = @("shape", "row", "test", "tolerance", "clashes", "type", "status") + @($Statuses | ForEach-Object { $_.ToLowerInvariant() })
$WorkbookRowWords = @("test", "clash name", "status", "distance", "picture link")
$DocumentTestWords = @("position", "folder", "test", "type", "status", "tolerance", "tolerance m", "top level", "leaves") + @($Statuses | ForEach-Object { "top " + $_.ToLowerInvariant() }) + @($Statuses | ForEach-Object { $_.ToLowerInvariant() })
$DocumentResultWords = @("test position", "position", "kind", "name", "status", "leaves", "distance", "distance m") + @($Statuses | ForEach-Object { $_.ToLowerInvariant() })
$DocumentKeys = @("parameter", "units", "metres per unit", "pass 1", "pass 2")

function Resolve-Full([string] $path) {
    return [IO.Path]::GetFullPath($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path))
}

# -Out is checked before anything is read, and a fault in it writes nothing, because there
# is nowhere it may write.
$work = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA "NwcFederatorLoop")) + [IO.Path]::DirectorySeparatorChar
$workbookFull = Resolve-Full $Workbook
$documentFull = Resolve-Full $Document
$outFull = Resolve-Full $Out
$outFault = $null
if (-not $outFull.StartsWith($work, [StringComparison]::OrdinalIgnoreCase)) { $outFault = "-Out is not under $work" }
elseif ([IO.Path]::GetExtension($outFull) -ne ".txt") { $outFault = "-Out is not a .txt" }
elseif ($outFull -ieq $workbookFull -or $outFull -ieq $documentFull) { $outFault = "-Out names one of the two read-outs" }
elseif ((Test-Path -LiteralPath $outFull) -or (Test-Path -LiteralPath ($outFull + ".partial"))) { $outFault = "a comparison is there already at -Out and is never written over" }
if ($outFault) {
    Write-Host "COMPARISON FAILED: $outFault. Nothing was written."
    exit 1
}

# The picture statuses arrive as one string when the script is run with -File, so each
# is split at its commas, and a word that is not one of the five is refused.
$pictureWords = @()
foreach ($p in @($PictureStatuses)) {
    foreach ($w in ([string]$p -split ',')) {
        $w = $w.Trim()
        if (-not $w) { continue }
        $known = @($Statuses | Where-Object { $_ -ieq $w })
        if ($known.Count -ne 1) { Write-Host "COMPARISON FAILED: -PictureStatuses holds '$w', which is not one of $($Statuses -join ', '). Nothing was written."; exit 1 }
        if ($pictureWords -notcontains $known[0]) { $pictureWords += $known[0] }
    }
}
if ($PictureCap -lt 0) { Write-Host "COMPARISON FAILED: -PictureCap is below zero. Nothing was written."; exit 1 }

# The leading comma hands the dictionary back whole, where PowerShell would unroll it.
function New-Ordinal { return ,(New-Object 'System.Collections.Generic.Dictionary[string,object]' -ArgumentList ([StringComparer]::Ordinal)) }

function Read-Count([string] $text) {
    $n = 0
    if ([int]::TryParse($text, [Globalization.NumberStyles]::AllowLeadingSign, $Invariant, [ref]$n)) { return $n }
    return $null
}

function Read-Whole([string] $text) {
    $n = 0
    if ([int]::TryParse($text, [Globalization.NumberStyles]::None, $Invariant, [ref]$n)) { return $n }
    return $null
}

function Read-Real([string] $text) {
    $d = 0.0
    if ([double]::TryParse($text, [Globalization.NumberStyles]::Float, $Invariant, [ref]$d)) { return $d }
    return $null
}

function Format-Real([double] $value) { return $value.ToString("R", $Invariant) }

function Read-Columns([string] $line, [string] $prefix, [string[]] $need, [string] $what) {
    $words = @($line.Substring($prefix.Length) -split ', ')
    $map = @{}
    for ($i = 0; $i -lt $words.Count; $i++) { if (-not $map.ContainsKey($words[$i])) { $map[$words[$i]] = $i } }
    foreach ($n in $need) { if (-not $map.ContainsKey($n)) { throw "the $what has no column '$n' on the line [$line]" } }
    return [pscustomobject]@{ Map = $map; Count = $words.Count }
}

function Read-Totals([string] $text) {
    $pairs = New-Object System.Collections.Generic.List[object]
    foreach ($part in ($text -split ', ')) {
        $at = $part.LastIndexOf(' ')
        if ($at -lt 1) { $pairs.Add([pscustomobject]@{ Key = $part; Value = "" }); continue }
        $pairs.Add([pscustomobject]@{ Key = $part.Substring(0, $at); Value = $part.Substring($at + 1) })
    }
    return $pairs
}

# One read-out into its lines, refused when it is not whole. The failure lines are looked
# for above the first table only, because a test or a clash may carry any name, and both
# read-workbook.ps1 and the probe write them there.
function Read-Lines([string] $path, [string] $what) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "there is no $what at $path" }
    $lines = [IO.File]::ReadAllLines($path)
    foreach ($line in $lines) {
        if ($line.StartsWith("-- ")) { break }
        if ($line.StartsWith("READ-OUT FAILED") -or $line.StartsWith("REFUSED")) { throw "the $what says $line" }
    }
    $last = ""
    for ($i = $lines.Count - 1; $i -ge 0; $i--) { if ($lines[$i] -ne "") { $last = $lines[$i]; break } }
    if ($last -cne $End) { throw "the $what does not end in $End" }
    return ,$lines
}

function Read-Workbook([string] $path) {
    $lines = Read-Lines $path "workbook read-out"
    $wb = [pscustomobject]@{
        Path = ""; Tests = New-Object System.Collections.Generic.List[object]
        Rows = New-Object System.Collections.Generic.List[object]
        Doubts = New-Object System.Collections.Generic.List[string]; Stated = -1; Own = New-Object System.Collections.Generic.List[string]
    }
    $mode = ""; $cols = $null
    for ($n = 0; $n -lt $lines.Count; $n++) {
        $line = $lines[$n]
        if ($mode -eq "tests" -or $mode -eq "rows") {
            if ($line -eq "") { $mode = ""; continue }
            $f = $line.Split([char]9)
            if ($f.Count -ne $cols.Count) { $wb.Own.Add("line $($n + 1) of the workbook read-out holds $($f.Count) fields and its column line names $($cols.Count), so it is left out"); continue }
            $m = $cols.Map
            if ($mode -eq "tests") {
                $t = [pscustomobject]@{ Name = $f[$m["test"]]; Shape = $f[$m["shape"]]; Row = $f[$m["row"]]; Tolerance = $f[$m["tolerance"]]; Clashes = $f[$m["clashes"]]; Type = $f[$m["type"]]; Status = $f[$m["status"]]; By = @{} }
                foreach ($s in $Statuses) { $t.By[$s] = $f[$m[$s.ToLowerInvariant()]] }
                $wb.Tests.Add($t)
            } else {
                $wb.Rows.Add([pscustomobject]@{ Test = $f[$m["test"]]; Name = $f[$m["clash name"]]; Status = $f[$m["status"]]; Distance = $f[$m["distance"]]; Link = $f[$m["picture link"]] })
            }
            continue
        }
        if ($mode -eq "doubts") {
            if ($line.StartsWith("  ")) { $wb.Doubts.Add($line.Substring(2)); continue }
            $mode = ""
        }
        if ($n -eq 0 -and $line.StartsWith("workbook ")) { $wb.Path = $line.Substring(9); continue }
        if ($line.StartsWith("-- tests: ")) { $cols = Read-Columns $line "-- tests: " $WorkbookTestWords "workbook read-out"; $mode = "tests"; continue }
        if ($line.StartsWith("-- clash rows: ")) { $cols = Read-Columns $line "-- clash rows: " $WorkbookRowWords "workbook read-out"; $mode = "rows"; continue }
        if ($line.StartsWith("DOUBTS: ")) { $wb.Stated = Read-Count $line.Substring(8); $mode = "doubts"; continue }
    }
    if (-not $wb.Path) { throw "the workbook read-out does not start with the workbook line" }
    return $wb
}

function Read-Document([string] $path) {
    $lines = Read-Lines $path "document read-out"
    $doc = [pscustomobject]@{
        Head = @{}; Tables = 0; Tests = New-Object System.Collections.Generic.List[object]
        Results = New-Object System.Collections.Generic.List[object]
        Doubts = New-Object System.Collections.Generic.List[string]; Stated = -1; Own = New-Object System.Collections.Generic.List[string]
    }
    $mode = "keys"; $cols = $null
    for ($n = 0; $n -lt $lines.Count; $n++) {
        $line = $lines[$n]
        if ($mode -eq "tests" -or $mode -eq "results") {
            if ($line -eq "") { $mode = ""; continue }
            $f = $line.Split([char]9)
            if ($f.Count -ne $cols.Count) { $doc.Own.Add("line $($n + 1) of the document read-out holds $($f.Count) fields and its column line names $($cols.Count), so it is left out"); continue }
            $m = $cols.Map
            if ($mode -eq "tests") {
                $t = [pscustomobject]@{ Position = Read-Count $f[$m["position"]]; Folder = $f[$m["folder"]]; Name = $f[$m["test"]]; Type = $f[$m["type"]]; Status = $f[$m["status"]]
                    Tolerance = $f[$m["tolerance"]]; ToleranceM = $f[$m["tolerance m"]]; TopLevel = $null; Leaves = $null; Top = @{}; By = @{}; Known = $true }
                $t.TopLevel = Read-Count $f[$m["top level"]]
                $t.Leaves = Read-Count $f[$m["leaves"]]
                foreach ($s in $Statuses) {
                    $t.Top[$s] = Read-Count $f[$m["top " + $s.ToLowerInvariant()]]
                    $t.By[$s] = Read-Count $f[$m[$s.ToLowerInvariant()]]
                }
                foreach ($v in @($t.TopLevel, $t.Leaves) + @($t.Top.Values) + @($t.By.Values)) {
                    if ($null -eq $v -or $v -lt 0) { $t.Known = $false }
                }
                if ($null -eq $t.Position) { $doc.Own.Add("line $($n + 1) of the document read-out has no whole number for its position, so it is left out"); continue }
                $doc.Tests.Add($t)
            } else {
                $r = [pscustomobject]@{ TestPosition = Read-Count $f[$m["test position"]]; Position = $f[$m["position"]]; Kind = $f[$m["kind"]]; Name = $f[$m["name"]]; Status = $f[$m["status"]]
                    Leaves = Read-Count $f[$m["leaves"]]; By = @{}; Distance = $f[$m["distance"]]; DistanceM = $f[$m["distance m"]] }
                foreach ($s in $Statuses) { $r.By[$s] = Read-Count $f[$m[$s.ToLowerInvariant()]] }
                $doc.Results.Add($r)
            }
            continue
        }
        if ($mode -eq "doubts") {
            if ($line.StartsWith("  ")) { $doc.Doubts.Add($line.Substring(2)); continue }
            $mode = ""
        }
        if ($line.StartsWith("-- tests: ")) { $cols = Read-Columns $line "-- tests: " $DocumentTestWords "document read-out"; $doc.Tables++; $mode = "tests"; continue }
        if ($line.StartsWith("-- results: ")) { $cols = Read-Columns $line "-- results: " $DocumentResultWords "document read-out"; $doc.Tables++; $mode = "results"; continue }
        if ($line.StartsWith("DOUBTS: ")) { $doc.Stated = Read-Count $line.Substring(8); $mode = "doubts"; continue }
        if ($mode -eq "keys" -and $line.Contains("`t")) {
            $at = $line.IndexOf("`t")
            $doc.Head[$line.Substring(0, $at)] = $line.Substring($at + 1)
        }
    }
    foreach ($k in $DocumentKeys) { if (-not $doc.Head.ContainsKey($k)) { throw "the document read-out has no '$k' line" } }
    if ($doc.Tables -ne 2) { throw "the document read-out does not hold both its tests and its results column lines" }
    return $doc
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("COMPARISON of a workbook read-out with a document read-out, compare-document.ps1")
$lines.Add("workbook read-out  $workbookFull")
$lines.Add("document read-out  $documentFull")

try {
    foreach ($p in @($workbookFull, $documentFull)) {
        if (Test-Path -LiteralPath $p -PathType Leaf) { $lines.Add("  sha256 of $([IO.Path]::GetFileName($p))  " + (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash) }
    }
    $wb = Read-Workbook $workbookFull
    $doc = Read-Document $documentFull
}
catch {
    $lines.Add("COMPARISON FAILED: " + ($_.Exception.Message -replace "[`r`n]+", " ").Trim())
    $dir = Split-Path -Parent $outFull
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [IO.File]::WriteAllLines($outFull + ".partial", $lines)
    Move-Item -LiteralPath ($outFull + ".partial") -Destination $outFull
    Write-Host $lines[$lines.Count - 1]
    exit 1
}

$docUnits = $doc.Head["units"]
$perUnit = Read-Real $doc.Head["metres per unit"]
$docInMetres = $docUnits -ceq "Meters"
$metresKnown = ($null -ne $perUnit) -or $docInMetres
$pass2 = Read-Totals $doc.Head["pass 2"]
$docTestsWalked = $true
foreach ($p in $pass2) { if ($p.Key -eq "tests" -and (Read-Count $p.Value) -lt 0) { $docTestsWalked = $false } }

$lines.Add("picture statuses   " + $(if ($pictureWords.Count) { $pictureWords -join ", " } else { "not given, so which rows should carry a picture is not judged" }))
$lines.Add("picture cap        " + $(if ($PictureCap -gt 0) { "$PictureCap per test" } else { "none" }))
$lines.Add("priority picked    " + $(if ($PriorityPicked) { "yes, so the block order is chosen and not judged" } else { "no, so the block order is judged" }))
$lines.Add("group clashes at   " + $(if ($GroupClashesAt -eq "own") { "each clash's own status" } else { "the status of the group they are in" }))
$lines.Add("workbook           " + $wb.Path)
$lines.Add("document           " + $doc.Head["parameter"] + ", units " + $docUnits + ", metres per unit " + $doc.Head["metres per unit"])
$shown = @($pass2 | Where-Object { @("models", "selection sets", "set folders", "saved viewpoints", "viewpoint folders") -contains $_.Key } | ForEach-Object { $_.Key + " " + $_.Value })
$lines.Add("document holds     " + ($shown -join ", ") + ", shown and not judged")

$findings = New-Object System.Collections.Generic.List[string]
$doubtLines = New-Object System.Collections.Generic.List[string]
$wholeLines = New-Object System.Collections.Generic.List[string]

foreach ($d in $wb.Doubts) { $doubtLines.Add("DOUBT workbook read-out: $d") }
foreach ($d in $doc.Doubts) { $doubtLines.Add("DOUBT document read-out: $d") }
foreach ($d in $wb.Own) { $doubtLines.Add("DOUBT $d") }
foreach ($d in $doc.Own) { $doubtLines.Add("DOUBT $d") }
if ($wb.Stated -ne $wb.Doubts.Count) { $doubtLines.Add("DOUBT the workbook read-out says DOUBTS $($wb.Stated) and lists $($wb.Doubts.Count)") }
if ($doc.Stated -ne $doc.Doubts.Count) { $doubtLines.Add("DOUBT the document read-out says DOUBTS $($doc.Stated) and lists $($doc.Doubts.Count)") }

# The two passes of the probe, compared here as well as in the probe, so a read-out whose
# own doubt went missing still shows that its document was not whole on the first read.
$pass1 = Read-Totals $doc.Head["pass 1"]
$moved = @()
$p1 = @{}; foreach ($p in $pass1) { $p1[$p.Key] = $p.Value }
foreach ($p in $pass2) { if (-not $p1.ContainsKey($p.Key)) { $moved += "$($p.Key) only in pass 2" } elseif ($p1[$p.Key] -cne $p.Value) { $moved += "$($p.Key) $($p1[$p.Key]) then $($p.Value)" } }
if ($pass1.Count -ne $pass2.Count) { $moved += "pass 1 names $($pass1.Count) counts and pass 2 names $($pass2.Count)" }
if ($moved.Count) { $doubtLines.Add("DOUBT the document read-out's two passes differ, " + ($moved -join ", ")) }

# Names, Ordinal and never trimmed. A name on two tests on one side is compared on neither.
$wbByName = New-Ordinal
foreach ($t in $wb.Tests) { if ($wbByName.ContainsKey($t.Name)) { $wbByName[$t.Name].Add($t) } else { $l = New-Object System.Collections.Generic.List[object]; $l.Add($t); $wbByName[$t.Name] = $l } }
$docByName = New-Ordinal
foreach ($t in $doc.Tests) { if ($docByName.ContainsKey($t.Name)) { $docByName[$t.Name].Add($t) } else { $l = New-Object System.Collections.Generic.List[object]; $l.Add($t); $docByName[$t.Name] = $l } }
foreach ($k in $wbByName.Keys) { if ($wbByName[$k].Count -gt 1) { $doubtLines.Add("DOUBT [$k] is the name of $($wbByName[$k].Count) workbook tests, and no copy is compared") } }
foreach ($k in $docByName.Keys) { if ($docByName[$k].Count -gt 1) { $doubtLines.Add("DOUBT [$k] is the name of $($docByName[$k].Count) document tests, and no copy is compared") } }

$rowsByTest = New-Ordinal
foreach ($r in $wb.Rows) {
    if (-not $wbByName.ContainsKey($r.Test)) { $doubtLines.Add("DOUBT a clash row [$($r.Name)] in the workbook read-out names [$($r.Test)], which is no test in it"); continue }
    if (-not $rowsByTest.ContainsKey($r.Test)) { $rowsByTest[$r.Test] = New-Object System.Collections.Generic.List[object] }
    $rowsByTest[$r.Test].Add($r)
}
$resultsByTest = @{}
$docPositions = @{}
foreach ($t in $doc.Tests) { $docPositions[$t.Position] = $t }
foreach ($r in $doc.Results) {
    if ($null -eq $r.TestPosition -or -not $docPositions.ContainsKey($r.TestPosition)) { $doubtLines.Add("DOUBT a result [$($r.Name)] in the document read-out names test position $($r.TestPosition), which is no test in it"); continue }
    if (-not $resultsByTest.ContainsKey($r.TestPosition)) { $resultsByTest[$r.TestPosition] = New-Object System.Collections.Generic.List[object] }
    $resultsByTest[$r.TestPosition].Add($r)
}
foreach ($t in $doc.Tests) {
    $listed = if ($resultsByTest.ContainsKey($t.Position)) { $resultsByTest[$t.Position].Count } else { 0 }
    if ($t.Known -and $listed -ne $t.TopLevel) { $doubtLines.Add("DOUBT the document read-out lists $listed results under [$($t.Name)] and its test line says $($t.TopLevel)") }
}

function Get-Rows([string] $name) { if ($rowsByTest.ContainsKey($name)) { return ,$rowsByTest[$name] } return ,(New-Object System.Collections.Generic.List[object]) }
function Get-Results($t) { if ($resultsByTest.ContainsKey($t.Position)) { return ,$resultsByTest[$t.Position] } return ,(New-Object System.Collections.Generic.List[object]) }

# What the document's counts at one status are under the switch above.
function Get-Target($t, [string] $s) {
    if ($GroupClashesAt -eq "own") { return $t.By[$s] }
    $n = 0
    foreach ($r in (Get-Results $t)) { if ($r.Kind -eq "group") { if ($r.Status -ceq $s) { $n += $r.Leaves } } elseif ($r.Status -ceq $s) { $n += $r.By[$s] } }
    return $n
}

function Get-Metres([string] $metres, [string] $inUnits) {
    $m = Read-Real $metres
    if ($null -ne $m) { return $m }
    if ($docInMetres) { return Read-Real $inUnits }
    return $null
}

# Information under a test that differs: every group whose clashes are not all at the
# group's status, and every empty group, because those are where the two sides can part.
function Get-Information($t) {
    $info = @()
    if ($null -eq $t) { return $info }
    foreach ($r in (Get-Results $t)) {
        if ($r.Kind -ne "group") { continue }
        if ($r.Leaves -eq 0) { $info += "  information: group [$($r.Name)] holds no clash, an empty group"; continue }
        $at = @($Statuses | Where-Object { $r.By[$_] -gt 0 } | ForEach-Object { "$_ $($r.By[$_])" })
        $mixed = @($Statuses | Where-Object { $_ -cne $r.Status -and $r.By[$_] -gt 0 })
        if ($mixed.Count) { $info += "  information: group [$($r.Name)] is at $($r.Status) and holds clashes at " + ($at -join ", ") }
    }
    return $info
}

$zeroOnly = 0
$wbBase = [IO.Path]::GetFileNameWithoutExtension($wb.Path)
$wbFolder = $null
if ([IO.Path]::IsPathRooted($wb.Path)) { $f = Split-Path -Parent $wb.Path; if (Test-Path -LiteralPath $f -PathType Container) { $wbFolder = $f } }
$pictureIndex = 0
$numbering = $true
$anyLink = $false

foreach ($t in $wb.Tests) {
    $name = $t.Name
    $own = New-Object System.Collections.Generic.List[string]
    $counts = @{ Clashes = Read-Whole $t.Clashes }
    foreach ($s in $Statuses) { $counts[$s] = Read-Whole $t.By[$s] }
    $rows = Get-Rows $name
    $dup = $wbByName[$name].Count -gt 1
    $match = $null
    if (-not $dup -and $docTestsWalked -and $docByName.ContainsKey($name) -and $docByName[$name].Count -eq 1) { $match = $docByName[$name][0] }

    # 5, the unit on every workbook test.
    $tolUnit = $null; $tolValue = $null
    if ($t.Tolerance -match '^(-?[0-9]*\.?[0-9]+)([A-Za-z]+)$') { $tolValue = Read-Real $Matches[1]; $tolUnit = $Matches[2] }
    if ($null -eq $tolUnit) { $own.Add("DIFFERS [$name]  tolerance: workbook reads '$($t.Tolerance)', which is no number and unit") }
    elseif ($tolUnit -cne "m") { $own.Add("DIFFERS [$name]  tolerance: workbook $($t.Tolerance), the unit is not m, so the number is not compared") }

    # 9, inside the test.
    $sum = 0; $whole = $null -ne $counts.Clashes
    foreach ($s in $Statuses) { if ($null -eq $counts[$s]) { $whole = $false } else { $sum += $counts[$s] } }
    if ($whole -and $sum -ne $counts.Clashes) { $own.Add("DIFFERS [$name]  New to Resolved add to $sum and Clashes reads $($counts.Clashes)") }

    if ($dup) {
        $lines.Add("PAIR NOT MADE [$name]  on more than one workbook test")
    } elseif (-not $docTestsWalked) {
        $lines.Add("PAIR NOT MADE [$name]  the document read-out could not walk its tests")
    } elseif ($null -eq $match) {
        $numbers = @($counts.Clashes) + @($Statuses | ForEach-Object { $counts[$_] })
        $allZero = @($numbers | Where-Object { $null -eq $_ -or $_ -ne 0 }).Count -eq 0
        if ($docByName.ContainsKey($name)) {
            $lines.Add("PAIR NOT MADE [$name]  on more than one document test")
        } elseif ($allZero -and $rows.Count -eq 0) {
            $zeroOnly++
            $lines.Add("WORKBOOK ONLY, EVERY NUMBER ZERO [$name]  a test the run did not create, counted and not judged")
        } else {
            $lines.Add("WORKBOOK ONLY [$name]")
            $own.Add("DIFFERS [$name]  in the workbook only, Clashes $($t.Clashes), " + (($Statuses | ForEach-Object { "$_ $($t.By[$_])" }) -join ", ") + ", $($rows.Count) rows")
        }
    } else {
        $results = Get-Results $match
        $lines.Add("PAIR [$name]  workbook: $($t.Shape) at row $($t.Row), Clashes $($t.Clashes), $($rows.Count) rows, type $($t.Type), status $($t.Status)  document: position $($match.Position), folder $($match.Folder), $($match.TopLevel) top level, $($match.Leaves) clashes, type $($match.Type), status $($match.Status)")

        if ($null -ne $tolUnit -and $tolUnit -ceq "m" -and $metresKnown) {
            $docTol = Get-Metres $match.ToleranceM $match.Tolerance
            if ($null -eq $docTol) { $own.Add("NOT COMPARED [$name]  tolerance, the document read-out holds no value in metres for it") }
            elseif ([Math]::Abs($tolValue - $docTol) -gt ($Within + $Slack)) { $own.Add("DIFFERS [$name]  tolerance: workbook $($t.Tolerance), document $(Format-Real $docTol) m") }
        }

        if (-not $match.Known) {
            $own.Add("NOT COMPARED [$name]  the document read-out counts this test as -1, UNKNOWN, so its rows, clashes, statuses and distances are not compared")
        } else {
            # 1 and 2.
            if ($rows.Count -ne $match.TopLevel) { $own.Add("DIFFERS [$name]  rows: workbook $($rows.Count), document $($match.TopLevel) top level results") }
            if ($null -ne $counts.Clashes -and $counts.Clashes -ne $match.Leaves) { $own.Add("DIFFERS [$name]  Clashes: workbook $($counts.Clashes), document $($match.Leaves) clashes") }
            # 3.
            foreach ($s in $Statuses) {
                $target = Get-Target $match $s
                if ($null -ne $counts[$s] -and $counts[$s] -ne $target) {
                    $how = if ($GroupClashesAt -eq "own") { "clashes at $s" } else { "clashes filed at $s under their group's status" }
                    $own.Add("DIFFERS [$name]  ${s}: workbook $($counts[$s]), document $target $how (top level $($match.Top[$s]))")
                }
            }
            # 4 and 6.
            $most = [Math]::Max($rows.Count, $results.Count)
            for ($i = 0; $i -lt $most; $i++) {
                $row = if ($i -lt $rows.Count) { $rows[$i] } else { $null }
                $res = if ($i -lt $results.Count) { $results[$i] } else { $null }
                $at = $i + 1
                if ($null -eq $row) { $own.Add("DIFFERS [$name]  row ${at}: workbook none, document [$($res.Name)] $($res.Status)"); continue }
                if ($null -eq $res) { $own.Add("DIFFERS [$name]  row ${at}: workbook [$($row.Name)] $($row.Status), document none"); continue }
                if ($row.Name -cne $res.Name -or $row.Status -cne $res.Status) { $own.Add("DIFFERS [$name]  row ${at}: workbook [$($row.Name)] $($row.Status), document [$($res.Name)] $($res.Status)") }
                if ($row.Name -cne $res.Name -or $res.Kind -ne "clash" -or -not $metresKnown) { continue }
                $wbDistance = Read-Real $row.Distance
                $docDistance = Get-Metres $res.DistanceM $res.Distance
                if ($null -eq $wbDistance) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] distance: workbook reads '$($row.Distance)', which is no number") }
                elseif ($null -eq $docDistance) { $own.Add("NOT COMPARED [$name]  row $at [$($row.Name)] distance, the document read-out holds no value in metres for it") }
                elseif ([Math]::Abs($wbDistance - $docDistance) -gt ($Within + $Slack)) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] distance: workbook $($row.Distance) m, document $(Format-Real $docDistance) m") }
            }
        }
    }

    # 7, from the workbook read-out alone.
    $linked = @($rows | Where-Object { $_.Link })
    if ($linked.Count) { $anyLink = $true }
    if ($dup -and $linked.Count -and $numbering) {
        $numbering = $false
        $wholeLines.Add("NOT COMPARED picture numbering from [$name] on, the name is on more than one workbook block")
    }
    if (-not $dup) {
        $clashIndex = 0; $atStatus = 0
        for ($i = 0; $i -lt $rows.Count; $i++) {
            $row = $rows[$i]; $at = $i + 1
            if ($row.Link) {
                $clashIndex++
                if ($row.Link.StartsWith("NOT A PICTURE LINK")) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] picture link opens no picture: $($row.Link)") }
                elseif ($numbering) {
                    $expected = $wbBase + "_files/cd" + $pictureIndex.ToString("00", $Invariant) + $clashIndex.ToString("0000", $Invariant) + ".jpg"
                    if ([Uri]::UnescapeDataString($row.Link) -cne $expected) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] picture link: workbook $($row.Link), the export's numbering gives $expected") }
                }
                if ($wbFolder -and -not $row.Link.StartsWith("NOT A PICTURE LINK")) {
                    $u = $null
                    $onDisk = if ([Uri]::TryCreate($row.Link, [UriKind]::Absolute, [ref]$u) -and $u.IsFile) { $u.LocalPath } else { Join-Path $wbFolder ([Uri]::UnescapeDataString($row.Link) -replace '/', '\') }
                    if (-not (Test-Path -LiteralPath $onDisk -PathType Leaf)) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] picture link $($row.Link) names no file on disk") }
                }
            }
            if ($pictureWords.Count) {
                $should = $false
                if ($pictureWords -ccontains $row.Status) { $atStatus++; $should = ($PictureCap -eq 0 -or $atStatus -le $PictureCap) }
                if ($should -and -not $row.Link) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] is at $($row.Status) and carries no picture link") }
                elseif (-not $should -and $row.Link) {
                    if ($pictureWords -ccontains $row.Status) { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] carries a picture link past the cap of $PictureCap") }
                    else { $own.Add("DIFFERS [$name]  row $at [$($row.Name)] is at $($row.Status) and carries a picture link, pictures were asked for " + ($pictureWords -join ", ")) }
                }
            } elseif ($PictureCap -gt 0 -and $row.Link -and $clashIndex -gt $PictureCap) {
                $own.Add("DIFFERS [$name]  row $at [$($row.Name)] carries a picture link past the cap of $PictureCap")
            }
        }
        if ($linked.Count) { $pictureIndex++ }
    }

    foreach ($o in $own) { $lines.Add($o) }
    if (@($own | Where-Object { $_.StartsWith("DIFFERS") }).Count) { foreach ($i in (Get-Information $match)) { $lines.Add($i) } }
}

# The document's tests the workbook does not hold.
if ($docTestsWalked) {
    foreach ($t in $doc.Tests) {
        if ($docByName[$t.Name].Count -gt 1 -or $wbByName.ContainsKey($t.Name)) { continue }
        $lines.Add("DOCUMENT ONLY [$($t.Name)]")
        $lines.Add("DIFFERS [$($t.Name)]  in the document only, " + $(if ($t.Known) { "$($t.Leaves) clashes" } else { "clashes UNKNOWN, counted as -1" }))
        foreach ($i in (Get-Information $t)) { $lines.Add($i) }
    }
}

# 8, the block order.
if (-not $PriorityPicked) {
    $prev = $null
    foreach ($t in $wb.Tests) {
        $c = Read-Whole $t.Clashes
        if ($null -eq $c -or $c -eq 0 -or $wbByName[$t.Name].Count -gt 1) { continue }
        if ($null -ne $prev) {
            if ($c -gt $prev.Clashes) { $wholeLines.Add("DIFFERS block order: [$($t.Name)] with Clashes $c comes after [$($prev.Name)] with Clashes $($prev.Clashes), and Clashes never rises down the sheet") }
            elseif ($c -eq $prev.Clashes -and $docByName.ContainsKey($t.Name) -and $docByName.ContainsKey($prev.Name) -and $docByName[$t.Name].Count -eq 1 -and $docByName[$prev.Name].Count -eq 1) {
                # A block in the workbook only has no place in the document to keep, and is
                # already a DIFFERS line of its own, so only blocks in both are held to it.
                if ($docByName[$t.Name][0].Position -lt $docByName[$prev.Name][0].Position) { $wholeLines.Add("DIFFERS block order: [$($t.Name)] comes after [$($prev.Name)], both Clashes $c, and the document holds [$($t.Name)] first") }
            }
        }
        $prev = [pscustomobject]@{ Name = $t.Name; Clashes = $c }
    }
}

# 9, the totals.
if (-not $docTestsWalked) {
    $wholeLines.Add("NOT COMPARED every test and the totals, the document read-out could not walk its clash tests, tests reads -1")
} elseif (@($doc.Tests | Where-Object { -not $_.Known }).Count) {
    $wholeLines.Add("NOT COMPARED totals, the document read-out holds a test it could not count")
} else {
    $wbTotal = @{}; $unread = $false
    foreach ($k in @("Clashes") + $Statuses) { $wbTotal[$k] = 0 }
    foreach ($t in $wb.Tests) {
        $v = Read-Whole $t.Clashes; if ($null -eq $v) { $unread = $true } else { $wbTotal["Clashes"] += $v }
        foreach ($s in $Statuses) { $v = Read-Whole $t.By[$s]; if ($null -eq $v) { $unread = $true } else { $wbTotal[$s] += $v } }
    }
    if ($unread) {
        $wholeLines.Add("NOT COMPARED totals, a workbook test carries a count that is no whole number")
    } else {
        $docTotal = 0; foreach ($t in $doc.Tests) { $docTotal += $t.Leaves }
        if ($wbTotal["Clashes"] -ne $docTotal) { $wholeLines.Add("DIFFERS totals  Clashes: workbook $($wbTotal['Clashes']), document $docTotal clashes") }
        foreach ($s in $Statuses) {
            $n = 0; foreach ($t in $doc.Tests) { $n += (Get-Target $t $s) }
            if ($wbTotal[$s] -ne $n) { $wholeLines.Add("DIFFERS totals  ${s}: workbook $($wbTotal[$s]), document $n clashes at $s") }
        }
    }
}

if (-not $metresKnown) { $wholeLines.Add("NOT COMPARED tolerances and distances, the document read-out holds no metres per unit and the document is in $docUnits") }
if ($anyLink -and -not $wbFolder) { $wholeLines.Add("NOT COMPARED pictures on disk, the workbook read-out names $($wb.Path), which is not a folder on this machine") }
if (-not $pictureWords.Count) { $wholeLines.Add("NOT COMPARED which rows should carry a picture, -PictureStatuses was not given") }

$lines.Add("")
$lines.Add("-- the whole workbook")
$lines.Add("workbook tests with every number zero and no document test, counted and not judged: $zeroOnly")
foreach ($w in $wholeLines) { $lines.Add($w) }
$lines.Add("")
$lines.Add("-- doubts")
foreach ($d in $doubtLines) { $lines.Add($d) }

$lines.Add("")
$lines.Add("What this comparison cannot catch:")
$lines.Add("  a fault in the Navisworks .NET API itself, since both read-outs read through it. Clash Detective's own export, 5a, is the witness for that")
$lines.Add("  which units ClashTest.Tolerance and a clash's Distance are held in, PQ3, since both sides take them to be the document's")
$lines.Add("  whether a picture shows its clash")
$lines.Add("  the clash point, the grid location and the item columns")
$lines.Add("  anything in the picture numbering of scan.md 4k that both read the same wrong way")
$lines.Add("  the test type and the test status, shown and never judged")
if ($PriorityPicked) { $lines.Add("  the block order, which a picked priority file chooses") }

$disagreements = @($lines | Where-Object { $_.StartsWith("DIFFERS ") }).Count
$notCompared = @($lines | Where-Object { $_.StartsWith("NOT COMPARED ") }).Count
$doubtCount = @($lines | Where-Object { $_.StartsWith("DOUBT ") }).Count
$verdict = if ($disagreements -gt 0) { "DISAGREE" } elseif ($notCompared -gt 0 -or $doubtCount -gt 0) { "NOT PROVED" } else { "AGREE" }
$lines.Add("")
$lines.Add("DISAGREEMENTS $disagreements")
$lines.Add("NOT COMPARED $notCompared")
$lines.Add("DOUBTS $doubtCount")
$lines.Add("VERDICT $verdict")
$lines.Add("END OF COMPARISON")

$dir = Split-Path -Parent $outFull
if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
[IO.File]::WriteAllLines($outFull + ".partial", $lines)
Move-Item -LiteralPath ($outFull + ".partial") -Destination $outFull
Write-Host ("comparison    : {0}, DISAGREEMENTS {1}, NOT COMPARED {2}, DOUBTS {3}, VERDICT {4}" -f $outFull, $disagreements, $notCompared, $doubtCount, $verdict)
exit 0
