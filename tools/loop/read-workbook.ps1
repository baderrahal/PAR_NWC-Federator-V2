<#
    Reads a clash workbook back off the disk and writes a plain text read-out of it: one
    line per test, one line per clash row, the totals, and every doubt it had. The loop's
    log-reader and the check against the document read this, never the workbook itself.

    It shares NO code with the tool that wrote the workbook. It opens the xlsx as the zip it
    is and reads the cells, the shared strings, the hyperlinks and the drawings straight off
    the XML. A check that reads a file through the code that wrote it proves nothing about
    the file.

    A TEST IS KNOWN BY COLUMN A OR BY ITS NUMBERS, in both shapes a workbook can hold. The
    full block, which the client's export writes for every test, has the name in A beside
    the headings Tolerance to Status, its numbers on the row below, and its clash rows under
    a heading row whose A reads Image. The one row test, which this tool writes for a test
    that found nothing since Q73, has the name in A and its numbers on the same row, in the
    columns the headings use, C to K. Read off samples\client-report and off
    src\Federator.Core\Report\WorkbookWriter.cs on 2026-09-27. A row with A empty that
    still carries a tolerance in C and six whole numbers in D to I is a test whose name is
    missing, and it is read as a test and named as a doubt, never as a clash row. A clash
    row has A empty and something in the clash columns, and one with no clash name is
    counted and named as a doubt. Anything the read-out could not place is a doubt, and a
    workbook with no sheet or a sheet with no test is a doubt too, so an empty read-out
    never looks like a clean one.

    Usage:
        powershell -ExecutionPolicy Bypass -File tools\loop\read-workbook.ps1 -Workbook <xlsx> -Out steps\runs\<NN>\<run>\<name>.txt
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Workbook,
    [Parameter(Mandatory = $true)] [string] $Out
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-FullPath([string] $p) {
    return [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($p)) { $p } else { Join-Path (Get-Location).Path $p }))
}

$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$runs = Join-Path $repo "steps\runs"
$full = Get-FullPath $Workbook
$outFull = Get-FullPath $Out

# Where the read-out may go: a .txt under steps\runs, and never over the workbook it reads.
if (-not $outFull.StartsWith($runs + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw "The read-out goes under steps\runs. '$outFull' is not. Nothing was written."
}
if ([IO.Path]::GetExtension($outFull) -ne ".txt") { throw "The read-out is a .txt file. '$outFull' is not. Nothing was written." }
if ($outFull -ieq $full) { throw "The read-out would be written over the workbook it reads. Nothing was written." }

$RelationshipsNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
$Words = @("Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved", "Type", "Status")
$Counts = @("Clashes", "New", "Active", "Reviewed", "Approved", "Resolved")
$ClashColumns = @("Clash Name", "Status", "Distance", "Grid Location", "Description", "Clash Point")

function Read-Entry($zip, [string] $name) {
    $e = $zip.GetEntry($name)
    if ($null -eq $e) { return $null }
    $r = New-Object IO.StreamReader($e.Open())
    try { return $r.ReadToEnd() } finally { $r.Dispose() }
}

function Get-ColumnNumber([string] $ref) {
    $n = 0
    foreach ($ch in ($ref -replace '[0-9$]', '').ToUpperInvariant().ToCharArray()) { $n = $n * 26 + ([int][char]$ch - 64) }
    return $n
}

function Get-CellText($node) {
    # A cell's text is its t elements, the phonetic runs left out, with tabs and line
    # breaks read as spaces so one cell never splits a read-out line in two.
    $sb = New-Object Text.StringBuilder
    foreach ($t in $node.SelectNodes(".//*[local-name()='t'][not(ancestor::*[local-name()='rPh'])]")) { [void]$sb.Append($t.InnerText) }
    return ($sb.ToString() -replace "[`t`r`n]+", " ")
}

function Resolve-PartTarget([string] $base, [string] $target) {
    if ($target.StartsWith("/")) { return $target.TrimStart("/") }
    $dir = $base.Substring(0, $base.LastIndexOf("/") + 1)
    $parts = New-Object System.Collections.Generic.List[string]
    foreach ($p in ($dir + $target).Split("/")) { if ($p -eq "..") { if ($parts.Count) { $parts.RemoveAt($parts.Count - 1) } } elseif ($p -ne "" -and $p -ne ".") { $parts.Add($p) } }
    return ($parts -join "/")
}

function Test-WholeNumber([string] $text) {
    $n = 0
    return [int]::TryParse($text, [Globalization.NumberStyles]::None, [Globalization.CultureInfo]::InvariantCulture, [ref]$n)
}

$lines = New-Object System.Collections.Generic.List[string]
$doubts = New-Object System.Collections.Generic.List[string]
$grand = @{ Tests = 0; Full = 0; OneRow = 0; Nameless = 0; ClashRows = 0; Pictures = 0; Internal = 0; Pasted = 0; Linked = 0 }
foreach ($c in $Counts) { $grand[$c] = 0 }

try {
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { throw "There is no workbook at '$full'." }
    $info = Get-Item -LiteralPath $full
    $lines.Add("workbook $full")
    $lines.Add(("bytes {0}, written {1:yyyy-MM-dd HH:mm:ss} UTC, sha256 {2}" -f $info.Length, $info.LastWriteTimeUtc, (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash))

    $zip = [IO.Compression.ZipFile]::OpenRead($full)
    try {
        $wbRels = New-Object Xml.XmlDocument; $wbRels.LoadXml((Read-Entry $zip "xl/_rels/workbook.xml.rels"))
        $targets = @{}
        $sstPath = $null
        foreach ($rel in $wbRels.DocumentElement.SelectNodes("*[local-name()='Relationship']")) {
            $targets[$rel.GetAttribute("Id")] = Resolve-PartTarget "xl/workbook.xml" $rel.GetAttribute("Target")
            if ($rel.GetAttribute("Type") -like "*/sharedStrings") { $sstPath = Resolve-PartTarget "xl/workbook.xml" $rel.GetAttribute("Target") }
        }
        $shared = New-Object System.Collections.Generic.List[string]
        if ($sstPath) {
            $x = New-Object Xml.XmlDocument; $x.LoadXml((Read-Entry $zip $sstPath))
            foreach ($si in $x.DocumentElement.SelectNodes("*[local-name()='si']")) { $shared.Add((Get-CellText $si)) }
        }

        $wb = New-Object Xml.XmlDocument; $wb.LoadXml((Read-Entry $zip "xl/workbook.xml"))
        $sheets = @($wb.DocumentElement.SelectNodes(".//*[local-name()='sheet']"))
        $lines.Add("sheets $($sheets.Count): " + (($sheets | ForEach-Object { $_.GetAttribute("name") }) -join ", "))
        if ($sheets.Count -eq 0) { $doubts.Add("the workbook holds no sheet at all") }

        foreach ($sheet in $sheets) {
            $sheetName = $sheet.GetAttribute("name")
            $path = $targets[$sheet.GetAttribute("id", $RelationshipsNs)]
            $sheetText = if ($path) { Read-Entry $zip $path } else { $null }
            if (-not $sheetText) { $doubts.Add("sheet $sheetName names a part that is not in the workbook"); continue }
            $xml = New-Object Xml.XmlDocument; $xml.LoadXml($sheetText)

            # The sheet's relationships: its hyperlinks, and its drawing, which is where a
            # picture pasted into a cell lives.
            $relTargets = @{}
            $relsPath = ($path -replace '([^/]+)$', '_rels/$1.rels')
            $relsText = Read-Entry $zip $relsPath
            if ($relsText) {
                $rx = New-Object Xml.XmlDocument; $rx.LoadXml($relsText)
                foreach ($rel in $rx.DocumentElement.SelectNodes("*[local-name()='Relationship']")) {
                    $relTargets[$rel.GetAttribute("Id")] = $rel
                    if ($rel.GetAttribute("Type") -like "*/drawing") {
                        $drawing = Read-Entry $zip (Resolve-PartTarget $path $rel.GetAttribute("Target"))
                        if ($drawing) {
                            # A picture in a drawing is either stored in the workbook, a blip
                            # with r:embed, or points at a file outside it, a blip with r:link.
                            # The client's exports hold 65 and 66 of the second kind, each an
                            # absolute file:/// path, read on 2026-09-28, and none stored.
                            $dx = New-Object Xml.XmlDocument; $dx.LoadXml($drawing)
                            foreach ($blip in $dx.SelectNodes("//*[local-name()='blip']")) {
                                if ($blip.GetAttribute("embed", $RelationshipsNs)) { $grand.Pasted++ }
                                elseif ($blip.GetAttribute("link", $RelationshipsNs)) { $grand.Linked++ }
                            }
                        }
                    }
                }
            }

            # Hyperlinks by their first cell. A picture is an EXTERNAL relationship to a file.
            # A link to a place inside the workbook is what ClosedXML writes when it is
            # handed a path as a string, which opens no picture, so it is counted apart.
            $links = @{}
            foreach ($h in $xml.SelectNodes("//*[local-name()='hyperlink']")) {
                $ref = (($h.GetAttribute("ref") -split ':')[0] -replace '\$', '').ToUpperInvariant()
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
                    if ($ref) { $col = Get-ColumnNumber $ref } else { $col++; $ref = "(no ref, column $col)" }
                    $type = $c.GetAttribute("t")
                    $v = $c.SelectSingleNode("*[local-name()='v']")
                    if ($type -eq "s" -and $v) {
                        $i = [int]$v.InnerText
                        if ($i -ge 0 -and $i -lt $shared.Count) { $text = $shared[$i] }
                        else { $text = ""; $doubts.Add("sheet $sheetName, $ref points at shared string $i and the table holds $($shared.Count)") }
                    }
                    elseif ($type -eq "inlineStr") { $text = Get-CellText $c }
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
            $numbersRows = @{}

            foreach ($number in ($rows.Keys | Sort-Object)) {
                $row = $rows[$number]
                $cells = $row.Cells
                $a = if ($cells.ContainsKey(1)) { $cells[1] } else { "" }
                $values = @($cells.Values)
                $at = { param($col) if ($cells.ContainsKey($col)) { $cells[$col] } else { "" } }

                # The numbers row under a full block's headings was read with its block, and
                # would otherwise read as a test with no name. The Item 1 and Item 2 labels
                # over the two item blocks are part of the layout.
                if ($numbersRows.ContainsKey($number)) { continue }
                $filled = @($values | Where-Object { $_ })
                if ($filled.Count -gt 0 -and @($filled | Where-Object { $_ -ne "Item 1" -and $_ -ne "Item 2" }).Count -eq 0) { continue }

                if ($values -contains "Tolerance" -and $values -contains "Clashes") {
                    foreach ($col in $cells.Keys) { if ($Words -contains $cells[$col]) { $head[$cells[$col]] = $col } }
                    $headFromSheet = $true
                    $numbersRows[$number + 1] = $true
                    $below = if ($rows.ContainsKey($number + 1)) { $rows[$number + 1].Cells } else { $null }
                    if ($null -eq $below) { $doubts.Add("sheet $sheetName, the test on row $number has headings and no row $($number + 1) under them") }
                    $name = $a
                    if (-not $a) { $name = "(no name, row $number)"; $grand.Nameless++; $doubts.Add("sheet $sheetName, the test on row $number has no name in column A") }
                    $current = [pscustomobject]@{ Name = $name; Row = $number; Shape = "full block"; Values = $below; ClashRows = 0 }
                    $tests.Add($current)
                    $clashHead = $null
                    continue
                }
                if ($a -eq "Image" -and ($values -contains "Clash Name")) {
                    $clashHead = @{}
                    foreach ($col in $cells.Keys) { if (-not $clashHead.ContainsKey($cells[$col])) { $clashHead[$cells[$col]] = $col } }
                    if ($null -eq $current) {
                        $doubts.Add("sheet $sheetName, clash headings on row $number before any test")
                        $current = [pscustomobject]@{ Name = "(clash rows before any test)"; Row = $number; Shape = "none"; Values = $null; ClashRows = 0 }
                    }
                    continue
                }

                # A one row test: a name in A, or no name and a test's numbers where the
                # headings put them, a tolerance and six whole numbers. A clash row never
                # carries whole numbers in all six, because its Status column holds a word.
                $looksLikeTest = $true
                if (-not ((& $at $head["Tolerance"]) -match '^-?[0-9.]+[A-Za-z]+$')) { $looksLikeTest = $false }
                foreach ($c in $Counts) { if (-not (Test-WholeNumber (& $at $head[$c]))) { $looksLikeTest = $false } }
                if ($a -or $looksLikeTest) {
                    $name = $a
                    if (-not $a) { $name = "(no name, row $number)"; $grand.Nameless++; $doubts.Add("sheet $sheetName, row $number carries a test's numbers and no name in column A") }
                    $current = [pscustomobject]@{ Name = $name; Row = $number; Shape = "one row"; Values = $cells; ClashRows = 0 }
                    $tests.Add($current)
                    $clashHead = $null
                    continue
                }

                if ($clashHead) {
                    $any = $false
                    foreach ($h in $ClashColumns) { if ($clashHead.ContainsKey($h) -and (& $at $clashHead[$h])) { $any = $true } }
                    if (-not $any) { continue }
                    $pick = { param($h) if ($clashHead.ContainsKey($h)) { & $at $clashHead[$h] } else { "" } }
                    $clashName = & $pick "Clash Name"
                    if (-not $clashName) { $clashName = "(no clash name)"; $doubts.Add("sheet $sheetName, row $number is a clash row with no clash name, under $($current.Name)") }
                    $link = ""
                    if ($clashHead.ContainsKey("Image") -and $row.Refs.ContainsKey($clashHead["Image"])) {
                        $imgRef = $row.Refs[$clashHead["Image"]]
                        if ($links.ContainsKey($imgRef)) {
                            $l = $links[$imgRef]
                            if ($l.External) { $link = $l.Target; $grand.Pictures++ } else { $link = "NOT A PICTURE LINK " + $l.Target; $grand.Internal++ }
                        }
                    }
                    $current.ClashRows++
                    $clashLines.Add((@($current.Name, $clashName, (& $pick "Status"), (& $pick "Distance"), (& $pick "Grid Location"), (& $pick "Clash Point"), $link) -join "`t"))
                    continue
                }

                if ($filled.Count -gt 0 -and -not ($filled.Count -eq 1 -and $filled[0] -eq "Clash Report")) {
                    $doubts.Add("sheet $sheetName, row $number holds '$($filled[0])' and is neither a test nor a clash row")
                }
            }

            $lines.Add("")
            $lines.Add("== sheet $sheetName")
            if ($tests.Count -eq 0) { $doubts.Add("sheet $sheetName holds no test") }
            $lines.Add("-- tests: shape, row, test, tolerance, clashes, new, active, reviewed, approved, resolved, type, status, clash rows under it")
            $sheetTotals = @{}
            foreach ($c in $Counts) { $sheetTotals[$c] = 0 }
            foreach ($t in $tests) {
                if ($t.Shape -eq "none") { continue }
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
            $lines.Add(("sheet totals: tests {0}, clash rows {1}, from the tests' own numbers clashes {2}, new {3}, active {4}, reviewed {5}, approved {6}, resolved {7}" -f @($tests | Where-Object { $_.Shape -ne "none" }).Count, $clashLines.Count, $sheetTotals.Clashes, $sheetTotals.New, $sheetTotals.Active, $sheetTotals.Reviewed, $sheetTotals.Approved, $sheetTotals.Resolved))
            if (-not $headFromSheet -and $tests.Count -gt 0) { $lines.Add("no full block on this sheet, so the one row tests were read in columns C to K, which is where WorkbookWriter puts them") }
        }
    }
    finally { $zip.Dispose() }

    $lines.Add("")
    $lines.Add(("TOTALS: tests {0} ({1} full blocks, {2} one row, {3} with no name), clash rows {4}, picture links in cells {5}, links that open no picture {6}, pictures stored in the workbook {7}, pictures in the sheet that point at a file outside it {8}" -f $grand.Tests, $grand.Full, $grand.OneRow, $grand.Nameless, $grand.ClashRows, $grand.Pictures, $grand.Internal, $grand.Pasted, $grand.Linked))
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
    [IO.File]::WriteAllLines($outFull, @("workbook $full", "READ-OUT FAILED: " + ($_.Exception.Message -replace "[`r`n]+", " ").Trim()))
    throw
}
