<#
    Reads a clash workbook back off the disk and writes a plain text read-out of it: one
    line per test, one line per clash row, the totals, and every doubt it had. The loop's
    log-reader and the check against the document read this, never the workbook itself.

    It shares NO code with the tool that wrote the workbook. It opens the xlsx as the zip it
    is and reads the cells, the shared strings and the hyperlinks straight off the XML. A
    check that reads a file through the code that wrote it proves nothing about the file.

    A TEST IS A ROW WITH SOMETHING IN COLUMN A, in both shapes a workbook can hold. The full
    block, which the client's export writes for every test, has the name in A beside the
    headings Tolerance to Status, its numbers on the row below, and its clash rows under a
    heading row whose A reads Image. The one row test, which this tool writes for a test
    that found nothing since Q73, has the name in A and its numbers on the same row, in the
    columns the headings would use, C to K. Read off samples\client-report and off
    src\Federator.Core\Report\WorkbookWriter.cs on 2026-09-27. A clash row has A empty and a
    Clash Name. Anything else found where one of those should be is written out as a doubt
    and never guessed at.

    Usage:
        powershell -ExecutionPolicy Bypass -File tools\loop\read-workbook.ps1 -Workbook <xlsx> -Out <txt under steps\runs>
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Workbook,
    [Parameter(Mandatory = $true)] [string] $Out
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$full = (Resolve-Path -LiteralPath $Workbook).Path
$outFull = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Out)) { $Out } else { Join-Path (Get-Location).Path $Out }))

# Where the read-out may go. Inside the repo, a .txt, and never over the workbook it reads.
if (-not $outFull.StartsWith($repo + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "The read-out goes inside the repo, under steps\runs. '$outFull' is not. Nothing was written."
}
if ([IO.Path]::GetExtension($outFull) -ne ".txt") { throw "The read-out is a .txt file. '$outFull' is not. Nothing was written." }
if ($outFull -ieq $full) { throw "The read-out would be written over the workbook it reads. Nothing was written." }

$RelationshipsNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
$Words = @("Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved", "Type", "Status")
$Counts = @("Clashes", "New", "Active", "Reviewed", "Approved", "Resolved")

function Read-Entry($zip, [string] $name) {
    $e = $zip.GetEntry($name)
    if ($null -eq $e) { return $null }
    $r = New-Object IO.StreamReader($e.Open())
    try { return $r.ReadToEnd() } finally { $r.Dispose() }
}

function Column-Of([string] $ref) {
    $n = 0
    foreach ($ch in ($ref -replace '[0-9$]', '').ToUpperInvariant().ToCharArray()) { $n = $n * 26 + ([int][char]$ch - 64) }
    return $n
}

function Text-Of($node) {
    # A cell's text is its t elements, the phonetic runs left out, with tabs and line
    # breaks read as spaces so one cell never splits a read-out line in two.
    $sb = New-Object Text.StringBuilder
    foreach ($t in $node.SelectNodes(".//*[local-name()='t'][not(ancestor::*[local-name()='rPh'])]")) { [void]$sb.Append($t.InnerText) }
    return ($sb.ToString() -replace "[`t`r`n]+", " ")
}

function Relative-Target([string] $base, [string] $target) {
    if ($target.StartsWith("/")) { return $target.TrimStart("/") }
    $dir = $base.Substring(0, $base.LastIndexOf("/") + 1)
    $parts = New-Object System.Collections.Generic.List[string]
    foreach ($p in ($dir + $target).Split("/")) { if ($p -eq "..") { if ($parts.Count) { $parts.RemoveAt($parts.Count - 1) } } elseif ($p -ne "" -and $p -ne ".") { $parts.Add($p) } }
    return ($parts -join "/")
}

$lines = New-Object System.Collections.Generic.List[string]
$doubts = New-Object System.Collections.Generic.List[string]
$info = Get-Item -LiteralPath $full
$lines.Add("workbook $full")
$lines.Add(("bytes {0}, written {1:yyyy-MM-dd HH:mm:ss} UTC, sha256 {2}" -f $info.Length, $info.LastWriteTimeUtc, (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash))

$grand = @{ Tests = 0; Full = 0; OneRow = 0; ClashRows = 0; Pictures = 0; Internal = 0 }
foreach ($c in $Counts) { $grand[$c] = 0 }

try {
    $zip = [IO.Compression.ZipFile]::OpenRead($full)
    try {
        $wbRels = New-Object Xml.XmlDocument; $wbRels.LoadXml((Read-Entry $zip "xl/_rels/workbook.xml.rels"))
        $targets = @{}
        $sstPath = $null
        foreach ($r in $wbRels.DocumentElement.SelectNodes("*[local-name()='Relationship']")) {
            $targets[$r.GetAttribute("Id")] = Relative-Target "xl/workbook.xml" $r.GetAttribute("Target")
            if ($r.GetAttribute("Type") -like "*/sharedStrings") { $sstPath = Relative-Target "xl/workbook.xml" $r.GetAttribute("Target") }
        }
        $shared = New-Object System.Collections.Generic.List[string]
        if ($sstPath) {
            $x = New-Object Xml.XmlDocument; $x.LoadXml((Read-Entry $zip $sstPath))
            foreach ($si in $x.DocumentElement.SelectNodes("*[local-name()='si']")) { $shared.Add((Text-Of $si)) }
        }

        $wb = New-Object Xml.XmlDocument; $wb.LoadXml((Read-Entry $zip "xl/workbook.xml"))
        $sheets = @($wb.DocumentElement.SelectNodes(".//*[local-name()='sheet']"))
        $lines.Add("sheets $($sheets.Count): " + (($sheets | ForEach-Object { $_.GetAttribute("name") }) -join ", "))

        foreach ($sheet in $sheets) {
            $sheetName = $sheet.GetAttribute("name")
            $path = $targets[$sheet.GetAttribute("id", $RelationshipsNs)]
            $xml = New-Object Xml.XmlDocument; $xml.LoadXml((Read-Entry $zip $path))

            # Hyperlinks by their first cell. A picture is an EXTERNAL relationship to a file.
            # A link to a place inside the workbook is what ClosedXML writes when it is
            # handed a path as a string, which opens no picture, so it is counted apart.
            $relTargets = @{}
            $relsText = Read-Entry $zip ($path -replace '([^/]+)$', '_rels/$1.rels')
            if ($relsText) {
                $rx = New-Object Xml.XmlDocument; $rx.LoadXml($relsText)
                foreach ($r in $rx.DocumentElement.SelectNodes("*[local-name()='Relationship']")) { $relTargets[$r.GetAttribute("Id")] = $r }
            }
            $links = @{}
            foreach ($h in $xml.SelectNodes("//*[local-name()='hyperlink']")) {
                $ref = ($h.GetAttribute("ref") -split ':')[0].ToUpperInvariant()
                $hid = $h.GetAttribute("id", $RelationshipsNs)
                if ($hid) {
                    if ($relTargets.ContainsKey($hid)) {
                        $rel = $relTargets[$hid]
                        $external = ($rel.GetAttribute("TargetMode") -eq "External") -and -not $rel.GetAttribute("Target").StartsWith("#")
                        $links[$ref] = [pscustomobject]@{ Target = $rel.GetAttribute("Target"); External = $external }
                    } else {
                        $doubts.Add("sheet $sheetName, hyperlink at $ref names relationship $hid and the sheet has no such relationship")
                        $links[$ref] = [pscustomobject]@{ Target = "(no relationship $hid)"; External = $false }
                    }
                } else {
                    $links[$ref] = [pscustomobject]@{ Target = "#" + $h.GetAttribute("location"); External = $false }
                }
            }

            # Every row by its number, as column number to text.
            $rows = @{}
            foreach ($row in $xml.SelectNodes("//*[local-name()='sheetData']/*[local-name()='row']")) {
                $number = [int]$row.GetAttribute("r")
                $cells = @{}; $refs = @{}; $col = 0
                foreach ($c in $row.SelectNodes("*[local-name()='c']")) {
                    $ref = $c.GetAttribute("r")
                    if ($ref) { $col = Column-Of $ref } else { $col++; $ref = "(no ref, column $col)" }
                    $type = $c.GetAttribute("t")
                    $v = $c.SelectSingleNode("*[local-name()='v']")
                    if ($type -eq "s" -and $v) {
                        $i = [int]$v.InnerText
                        if ($i -ge 0 -and $i -lt $shared.Count) { $text = $shared[$i] }
                        else { $text = ""; $doubts.Add("sheet $sheetName, $ref points at shared string $i and the table holds $($shared.Count)") }
                    }
                    elseif ($type -eq "inlineStr") { $text = Text-Of $c }
                    elseif ($v) { $text = $v.InnerText }
                    else { $text = "" }
                    $cells[$col] = $text
                    $refs[$col] = ($ref -replace '\$', '').ToUpperInvariant()
                }
                $rows[$number] = [pscustomobject]@{ Number = $number; Cells = $cells; Refs = $refs }
            }

            $tests = New-Object System.Collections.Generic.List[object]
            $clashLines = New-Object System.Collections.Generic.List[string]
            $current = $null
            $clashHead = $null
            $head = @{}
            for ($k = 0; $k -lt $Words.Count; $k++) { $head[$Words[$k]] = 3 + $k }
            $headFromSheet = $false

            foreach ($number in ($rows.Keys | Sort-Object)) {
                $row = $rows[$number]
                $cells = $row.Cells
                $a = if ($cells.ContainsKey(1)) { $cells[1] } else { "" }
                $values = @($cells.Values)

                if ($values -contains "Tolerance" -and $values -contains "Clashes") {
                    foreach ($col in $cells.Keys) { if ($Words -contains $cells[$col]) { $head[$cells[$col]] = $col } }
                    $headFromSheet = $true
                    $below = if ($rows.ContainsKey($number + 1)) { $rows[$number + 1].Cells } else { $null }
                    if ($null -eq $below) { $doubts.Add("sheet $sheetName, the test on row $number has headings and no row $($number + 1) under them") }
                    $current = [pscustomobject]@{ Name = $a; Row = $number; Shape = "full block"; Values = $below; ClashRows = 0 }
                    if (-not $a) { $doubts.Add("sheet $sheetName, the test on row $number has no name in column A") }
                    $tests.Add($current)
                    $clashHead = $null
                    continue
                }
                if ($a -eq "Image" -and ($values -contains "Clash Name")) {
                    $clashHead = @{}
                    foreach ($col in $cells.Keys) { if (-not $clashHead.ContainsKey($cells[$col])) { $clashHead[$cells[$col]] = $col } }
                    if ($null -eq $current) { $doubts.Add("sheet $sheetName, clash headings on row $number before any test") }
                    continue
                }
                if ($a) {
                    # A one row test. Its numbers are on this row, in the heading columns.
                    $current = [pscustomobject]@{ Name = $a; Row = $number; Shape = "one row"; Values = $cells; ClashRows = 0 }
                    $tests.Add($current)
                    $clashHead = $null
                    continue
                }
                if ($clashHead -and $clashHead.ContainsKey("Clash Name") -and $cells.ContainsKey($clashHead["Clash Name"]) -and $cells[$clashHead["Clash Name"]]) {
                    $pick = { param($h) if ($clashHead.ContainsKey($h) -and $cells.ContainsKey($clashHead[$h])) { $cells[$clashHead[$h]] } else { "" } }
                    $link = ""
                    if ($clashHead.ContainsKey("Image") -and $row.Refs.ContainsKey($clashHead["Image"])) {
                        $imgRef = $row.Refs[$clashHead["Image"]]
                        if ($links.ContainsKey($imgRef)) {
                            $l = $links[$imgRef]
                            if ($l.External) { $link = $l.Target; $grand.Pictures++ } else { $link = "NOT A PICTURE LINK " + $l.Target; $grand.Internal++ }
                        }
                    }
                    $current.ClashRows++
                    $clashLines.Add((@($current.Name, (& $pick "Clash Name"), (& $pick "Status"), (& $pick "Distance"), (& $pick "Grid Location"), (& $pick "Clash Point"), $link) -join "`t"))
                }
            }

            $lines.Add("")
            $lines.Add("== sheet $sheetName")
            $lines.Add("-- tests: shape, row, test, tolerance, clashes, new, active, reviewed, approved, resolved, type, status, clash rows under it")
            $sheetTotals = @{}
            foreach ($c in $Counts) { $sheetTotals[$c] = 0 }
            foreach ($t in $tests) {
                $f = foreach ($w in $Words) { if ($t.Values -and $t.Values.ContainsKey($head[$w])) { $t.Values[$head[$w]] } else { "" } }
                foreach ($c in $Counts) {
                    $text = if ($t.Values -and $t.Values.ContainsKey($head[$c])) { $t.Values[$head[$c]] } else { "" }
                    $n = 0
                    if ([int]::TryParse($text, [Globalization.NumberStyles]::None, [Globalization.CultureInfo]::InvariantCulture, [ref]$n)) { $sheetTotals[$c] += $n }
                    else { $doubts.Add("sheet $sheetName, row $($t.Row), $($t.Name): $c reads '$text', which is not a whole number, and is left out of the totals") }
                }
                $lines.Add(((@($t.Shape, $t.Row, $t.Name) + @($f) + @($t.ClashRows)) -join "`t"))
                $grand.Tests++
                if ($t.Shape -eq "one row") { $grand.OneRow++ } else { $grand.Full++ }
            }
            foreach ($c in $Counts) { $grand[$c] += $sheetTotals[$c] }
            $grand.ClashRows += $clashLines.Count
            $lines.Add("")
            $lines.Add("-- clash rows: test, clash name, status, distance, grid location, clash point, picture link")
            foreach ($c in $clashLines) { $lines.Add($c) }
            $lines.Add("")
            $lines.Add(("sheet totals: tests {0}, clash rows {1}, from the tests' own numbers clashes {2}, new {3}, active {4}, reviewed {5}, approved {6}, resolved {7}" -f $tests.Count, $clashLines.Count, $sheetTotals.Clashes, $sheetTotals.New, $sheetTotals.Active, $sheetTotals.Reviewed, $sheetTotals.Approved, $sheetTotals.Resolved))
            if (-not $headFromSheet -and $tests.Count -gt 0) { $lines.Add("no full block on this sheet, so the one row tests were read in columns C to K, which is where WorkbookWriter puts them") }
        }
    }
    finally { $zip.Dispose() }

    $lines.Add("")
    $lines.Add(("TOTALS: tests {0} ({1} full blocks, {2} one row), clash rows {3}, picture links {4}, links that open no picture {5}" -f $grand.Tests, $grand.Full, $grand.OneRow, $grand.ClashRows, $grand.Pictures, $grand.Internal))
    $lines.Add(("TOTALS from the tests' own numbers: clashes {0}, new {1}, active {2}, reviewed {3}, approved {4}, resolved {5}" -f $grand.Clashes, $grand.New, $grand.Active, $grand.Reviewed, $grand.Approved, $grand.Resolved))
    $lines.Add("DOUBTS: $($doubts.Count)")
    foreach ($d in $doubts) { $lines.Add("  " + $d) }
    $lines.Add("END OF READ-OUT")

    # Written beside and then moved, so a read-out that failed half way never leaves a
    # file that looks whole.
    $dir = Split-Path -Parent $outFull
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    $partial = $outFull + ".partial"
    [IO.File]::WriteAllLines($partial, $lines)
    Move-Item -LiteralPath $partial -Destination $outFull -Force
    Write-Host ("read-out      : {0}, {1} lines, {2} tests, {3} clash rows, {4} doubts" -f $outFull, $lines.Count, $grand.Tests, $grand.ClashRows, $doubts.Count)
}
catch {
    $dir = Split-Path -Parent $outFull
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [IO.File]::WriteAllLines($outFull, @("workbook $full", "READ-OUT FAILED: " + $_.Exception.Message))
    throw
}
