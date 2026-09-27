<#
    Reads a clash workbook back off the disk and writes a plain text read-out of it: one
    line per test block, one line per clash row, and the totals. The loop's log-reader and
    the check against the document read this, never the workbook itself.

    It shares NO code with the tool that wrote the workbook. It opens the xlsx as the zip it
    is and reads the cells, the shared strings and the hyperlinks straight off the XML, and
    it finds a test block by the heading cells Tolerance and Clashes and a clash row by the
    heading cell Clash Name, never by a row number. A check that reads a file through the
    code that wrote it proves nothing about the file.

    Usage:
        powershell -ExecutionPolicy Bypass -File tools\loop\read-workbook.ps1 -Workbook <xlsx> -Out <txt>
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Workbook,
    [Parameter(Mandatory = $true)] [string] $Out
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Read-Entry($zip, [string] $name) {
    $e = $zip.GetEntry($name)
    if ($null -eq $e) { return $null }
    $r = New-Object IO.StreamReader($e.Open())
    try { return $r.ReadToEnd() } finally { $r.Dispose() }
}

function Column-Of([string] $ref) {
    $letters = ($ref -replace '[0-9]', '')
    $n = 0
    foreach ($ch in $letters.ToCharArray()) { $n = $n * 26 + ([int][char]$ch - 64) }
    return $n
}

function Text-Of($node) {
    # A cell's text is its t element, or every t under its r runs for rich text.
    $parts = @()
    foreach ($t in $node.SelectNodes(".//*[local-name()='t']")) { $parts += $t.InnerText }
    return ($parts -join '')
}

$full = (Resolve-Path -LiteralPath $Workbook).Path
$zip = [IO.Compression.ZipFile]::OpenRead($full)
try {
    $shared = @()
    $sst = Read-Entry $zip "xl/sharedStrings.xml"
    if ($sst) {
        $x = New-Object Xml.XmlDocument; $x.LoadXml($sst)
        foreach ($si in $x.DocumentElement.SelectNodes("*[local-name()='si']")) { $shared += (Text-Of $si) }
    }

    $wb = New-Object Xml.XmlDocument; $wb.LoadXml((Read-Entry $zip "xl/workbook.xml"))
    $wbRels = New-Object Xml.XmlDocument; $wbRels.LoadXml((Read-Entry $zip "xl/_rels/workbook.xml.rels"))
    $targets = @{}
    foreach ($r in $wbRels.DocumentElement.SelectNodes("*[local-name()='Relationship']")) { $targets[$r.GetAttribute("Id")] = $r.GetAttribute("Target") }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("workbook $full")
    $lines.Add(("bytes {0}" -f (Get-Item -LiteralPath $full).Length))

    $sheets = $wb.DocumentElement.SelectNodes(".//*[local-name()='sheet']")
    $lines.Add("sheets $($sheets.Count): " + (($sheets | ForEach-Object { $_.GetAttribute("name") }) -join ", "))

    foreach ($sheet in $sheets) {
        $rid = $sheet.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
        $target = $targets[$rid] -replace '^/?xl/', ''
        $path = "xl/" + $target
        $xml = New-Object Xml.XmlDocument; $xml.LoadXml((Read-Entry $zip $path))

        # The hyperlinks of this sheet, by cell.
        $links = @{}
        $relsPath = ($path -replace '([^/]+)$', '_rels/$1.rels')
        $relsText = Read-Entry $zip $relsPath
        $relTargets = @{}
        if ($relsText) {
            $rx = New-Object Xml.XmlDocument; $rx.LoadXml($relsText)
            foreach ($r in $rx.DocumentElement.SelectNodes("*[local-name()='Relationship']")) { $relTargets[$r.GetAttribute("Id")] = $r.GetAttribute("Target") }
        }
        foreach ($h in $xml.SelectNodes("//*[local-name()='hyperlink']")) {
            $hid = $h.GetAttribute("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
            $links[$h.GetAttribute("ref")] = if ($hid -and $relTargets.ContainsKey($hid)) { $relTargets[$hid] } else { "#" + $h.GetAttribute("location") }
        }

        # Every row as column number to text, in sheet order.
        $rows = New-Object System.Collections.Generic.List[object]
        foreach ($row in $xml.SelectNodes("//*[local-name()='sheetData']/*[local-name()='row']")) {
            $cells = @{}
            $refs = @{}
            foreach ($c in $row.SelectNodes("*[local-name()='c']")) {
                $ref = $c.GetAttribute("r")
                $type = $c.GetAttribute("t")
                $v = $c.SelectSingleNode("*[local-name()='v']")
                $text = if ($type -eq "s" -and $v) { $shared[[int]$v.InnerText] }
                        elseif ($type -eq "inlineStr") { Text-Of $c }
                        elseif ($v) { $v.InnerText }
                        else { "" }
                $col = Column-Of $ref
                $cells[$col] = $text
                $refs[$col] = $ref
            }
            $rows.Add([pscustomobject]@{ Number = [int]$row.GetAttribute("r"); Cells = $cells; Refs = $refs })
        }

        $blocks = New-Object System.Collections.Generic.List[string]
        $clashes = New-Object System.Collections.Generic.List[string]
        $test = ""
        $clashHead = $null
        $sum = @{ Clashes = 0; New = 0; Active = 0; Reviewed = 0; Approved = 0; Resolved = 0 }
        $linked = 0

        for ($i = 0; $i -lt $rows.Count; $i++) {
            $cells = $rows[$i].Cells
            $values = @($cells.Values)
            if ($values -contains "Tolerance" -and $values -contains "Clashes") {
                # The block's heading row. The test name sits on this same row, to the left of
                # the headings, which is A4 and A24 in the client's own export, and the numbers
                # are on the row below it. Read off samples\client-report on 2026-09-27.
                $words = @("Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved", "Type", "Status")
                $test = ""
                foreach ($col in ($cells.Keys | Sort-Object)) {
                    if ($cells[$col] -and ($words -notcontains $cells[$col])) { $test = $cells[$col]; break }
                }
                $head = @{}
                foreach ($col in $cells.Keys) { $head[$cells[$col]] = $col }
                $num = if ($i + 1 -lt $rows.Count) { $rows[$i + 1].Cells } else { @{} }
                $get = { param($name) if ($head.ContainsKey($name) -and $num.ContainsKey($head[$name])) { $num[$head[$name]] } else { "" } }
                $f = @("Tolerance", "Clashes", "New", "Active", "Reviewed", "Approved", "Resolved", "Type", "Status") | ForEach-Object { & $get $_ }
                $blocks.Add(((@($test) + @($f)) -join "`t"))
                foreach ($s in @("Clashes", "New", "Active", "Reviewed", "Approved", "Resolved")) {
                    $n = 0; if ([int]::TryParse((& $get $s), [ref]$n)) { $sum[$s] += $n }
                }
                $clashHead = $null
                continue
            }
            if ($values -contains "Clash Name") {
                $clashHead = @{}
                foreach ($col in $cells.Keys) { if (-not $clashHead.ContainsKey($cells[$col])) { $clashHead[$cells[$col]] = $col } }
                continue
            }
            if ($clashHead -and $clashHead.ContainsKey("Clash Name")) {
                $name = $cells[$clashHead["Clash Name"]]
                if (-not $name) { continue }
                $pick = { param($h) if ($clashHead.ContainsKey($h) -and $cells.ContainsKey($clashHead[$h])) { $cells[$clashHead[$h]] } else { "" } }
                $imgRef = if ($clashHead.ContainsKey("Image")) { $rows[$i].Refs[$clashHead["Image"]] } else { $null }
                $link = if ($imgRef -and $links.ContainsKey($imgRef)) { $links[$imgRef] } else { "" }
                if ($link) { $linked++ }
                $clashes.Add((@($test, $name, (& $pick "Status"), (& $pick "Distance"), (& $pick "Grid Location"), (& $pick "Clash Point"), $link) -join "`t"))
            }
        }

        $lines.Add("")
        $lines.Add("== sheet $($sheet.GetAttribute('name')), rows $($rows.Count), test blocks $($blocks.Count), clash rows $($clashes.Count), clash rows with a picture link $linked")
        $lines.Add(("totals from the block headings: clashes {0}, new {1}, active {2}, reviewed {3}, approved {4}, resolved {5}" -f $sum.Clashes, $sum.New, $sum.Active, $sum.Reviewed, $sum.Approved, $sum.Resolved))
        $lines.Add("")
        $lines.Add("-- blocks: test, tolerance, clashes, new, active, reviewed, approved, resolved, type, status")
        foreach ($b in $blocks) { $lines.Add($b) }
        $lines.Add("")
        $lines.Add("-- clash rows: test, clash name, status, distance, grid location, clash point, picture link")
        foreach ($c in $clashes) { $lines.Add($c) }
    }
}
finally {
    $zip.Dispose()
}

$dir = Split-Path -Parent $Out
if ($dir -and -not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
[IO.File]::WriteAllLines($Out, $lines)
Write-Host "read-out      : $Out, $($lines.Count) lines"
