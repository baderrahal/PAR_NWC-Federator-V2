<#
    The one place the tracker's rules live, read by make-tracker.ps1 and check-tracker.ps1
    with a dot. F133, Bader's message of 5 Oct 2026, Q129.

    steps\tracker.csv holds one row per item: every FR item of steps\fix-round.md, every F
    area, Bader's requests and every question waiting for him. Its columns are the nine
    below, in Bader's order. A status is one of the seven below and nothing else. UNKNOWN is
    written where a value cannot be read, and is refused in the id and the status. No cell
    is empty or blank, and no cell holds a line break, since tracker.md gives each row one line.

    The class, the area and the wave of an FR row are read off fix-round.md, so the check
    compares them with it: the class with the first words of the item's Class line, up to a
    comma, or UNKNOWN when it has none, and the area and the wave with the line of the waves
    section that names the item, or none when no line names it.

    The csv is read by the rules of RFC 4180 and nothing looser: a cell is quoted when it
    holds a comma, a quote or a line break, a quote inside a quoted cell is written twice,
    and every row has as many cells as the header. A UTF-8 byte order mark is read past and
    CRLF reads as LF, since a Windows checkout may convert either. Windows PowerShell 5.1.
#>

$TrackerColumns = @("id", "short title", "area", "wave", "class", "status", "PR", "run that proved it", "date of last change")
$TrackerStatuses = @("open", "in progress", "in review", "merged", "proven by a run", "waiting for Bader", "dropped")

# A text file as UTF-8, past a byte order mark, with CRLF read as LF.
function Read-TrackerText([string] $Path) {
    return [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8).Replace("`r`n", "`n")
}

# Reads the csv into its rows, each with the line it starts on. Faults name the line. A fault
# in the quoting ends the read, since nothing after it can be placed in a row.
function Read-TrackerCsv([string] $Path) {
    $faults = New-Object System.Collections.Generic.List[string]
    $rows = New-Object System.Collections.Generic.List[object]
    $text = Read-TrackerText $Path
    if ($text.Length -eq 0) {
        $faults.Add("tracker.csv line 1: the file is empty")
        return @{ Rows = $rows; Faults = $faults }
    }
    $at = $text.IndexOf("`r")
    if ($at -ge 0) {
        $faults.Add("tracker.csv line " + ($text.Substring(0, $at).Split("`n").Length) + ": a carriage return with no line feed after it")
        return @{ Rows = $rows; Faults = $faults }
    }

    $i = 0
    $n = $text.Length
    $line = 1
    $records = New-Object System.Collections.Generic.List[object]
    while ($i -lt $n) {
        $startLine = $line
        $cells = New-Object System.Collections.Generic.List[string]
        while ($true) {
            $cell = New-Object System.Text.StringBuilder
            if ($i -lt $n -and $text[$i] -eq '"') {
                $i++
                $closed = $false
                while ($i -lt $n) {
                    $c = $text[$i]
                    if ($c -eq '"') {
                        if ($i + 1 -lt $n -and $text[$i + 1] -eq '"') { [void]$cell.Append('"'); $i += 2; continue }
                        $i++
                        $closed = $true
                        break
                    }
                    if ($c -eq "`n") { $line++ }
                    [void]$cell.Append($c)
                    $i++
                }
                if (-not $closed) {
                    $faults.Add("tracker.csv line ${startLine}: a quoted cell opens and never closes")
                    return @{ Rows = $rows; Faults = $faults }
                }
                if ($i -lt $n -and $text[$i] -ne ',' -and $text[$i] -ne "`n") {
                    $faults.Add("tracker.csv line ${line}: text after the closing quote of a cell")
                    return @{ Rows = $rows; Faults = $faults }
                }
            } else {
                while ($i -lt $n -and $text[$i] -ne ',' -and $text[$i] -ne "`n") {
                    if ($text[$i] -eq '"') {
                        $faults.Add("tracker.csv line ${line}: a quote inside a cell that does not start with one")
                        return @{ Rows = $rows; Faults = $faults }
                    }
                    [void]$cell.Append($text[$i])
                    $i++
                }
            }
            $cells.Add($cell.ToString())
            if ($i -ge $n) { break }
            if ($text[$i] -eq ',') { $i++; continue }
            $i++
            $line++
            break
        }
        $records.Add(@{ Line = $startLine; Cells = $cells.ToArray() })
    }

    $header = $records[0].Cells -join ","
    if ($header -cne ($TrackerColumns -join ",")) {
        $faults.Add("tracker.csv line 1: the header is not " + ($TrackerColumns -join ","))
        return @{ Rows = $rows; Faults = $faults }
    }
    for ($r = 1; $r -lt $records.Count; $r++) {
        $rec = $records[$r]
        if ($rec.Cells.Length -ne $TrackerColumns.Length) {
            $faults.Add("tracker.csv line " + $rec.Line + ", id " + $rec.Cells[0] + ": " + $rec.Cells.Length + " cells where the header has " + $TrackerColumns.Length)
            continue
        }
        $row = [ordered]@{ Line = $rec.Line }
        for ($k = 0; $k -lt $TrackerColumns.Length; $k++) { $row[$TrackerColumns[$k]] = $rec.Cells[$k] }
        $rows.Add($row)
    }
    return @{ Rows = $rows; Faults = $faults }
}

# The faults of rows that parsed: an empty or blank cell, a line break in a cell, an id
# twice, UNKNOWN in the id, and a status off the list. Each names the line and the id. Two
# ids that differ only in case count as the same id twice.
function Test-TrackerRows($Rows) {
    $faults = New-Object System.Collections.Generic.List[string]
    $seen = New-Object 'System.Collections.Generic.Dictionary[string,int]' ([StringComparer]::OrdinalIgnoreCase)
    foreach ($row in $Rows) {
        $id = $row["id"]
        $where = "tracker.csv line " + $row.Line + ", id " + $id
        if ($id -ceq "UNKNOWN") {
            $faults.Add("tracker.csv line " + $row.Line + ": the id is UNKNOWN, and UNKNOWN is accepted only outside the id and status columns")
        } elseif ($id.Trim().Length -gt 0) {
            if ($seen.ContainsKey($id)) { $faults.Add("${where}: the id is on line " + $seen[$id] + " already") } else { $seen[$id] = $row.Line }
        }
        foreach ($column in $TrackerColumns) {
            $value = $row[$column]
            if ($value.Trim().Length -eq 0) { $faults.Add("${where}: the $column cell is empty, write UNKNOWN when it cannot be read") }
            elseif ($value.Contains("`n")) { $faults.Add("${where}: the $column cell holds a line break, which would split its row in tracker.md") }
        }
        $status = $row["status"]
        if ($status.Length -gt 0 -and $TrackerStatuses -cnotcontains $status) {
            $why = "${where}: the status '$status' is not one of " + ($TrackerStatuses -join ", ")
            if ($status -ceq "UNKNOWN") { $why += ", and UNKNOWN is accepted only outside the id and status columns" }
            $faults.Add($why)
        }
    }
    return $faults
}

# Every FR item of fix-round.md by its heading, ### FR-<number> <key>, with its line and its
# Class line. A heading that names an FR number in any other shape is a fault, and so is a
# file with no item at all, so a heading the reader cannot see never reads as nothing to check.
function Get-FixRoundItems([string[]] $Lines) {
    $items = New-Object System.Collections.Generic.List[object]
    $faults = New-Object System.Collections.Generic.List[string]
    $item = $null
    for ($k = 0; $k -lt $Lines.Length; $k++) {
        $line = $Lines[$k]
        $m = [regex]::Match($line, '^### (FR-\d+)(\s|$)')
        if ($m.Success) {
            $item = @{ Id = $m.Groups[1].Value; Line = $k + 1; Class = "UNKNOWN"; ClassLine = 0 }
            $items.Add($item)
            continue
        }
        if ([regex]::IsMatch($line, '^#{1,6}\s*\W*FR-\d', [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            $faults.Add("fix-round.md line " + ($k + 1) + " looks like the heading of an FR item and is not in the shape ### FR-<number> <key>, so it was not read as one")
            $item = $null
            continue
        }
        if ($line.StartsWith("#")) { $item = $null; continue }
        if ($null -ne $item -and $item.ClassLine -eq 0 -and $line.StartsWith("- Class:")) {
            $item.Class = $line.Substring("- Class:".Length).Split(",")[0].Trim()
            $item.ClassLine = $k + 1
        }
    }
    if ($items.Count -eq 0) { $faults.Add("fix-round.md has no heading of an FR item in the shape ### FR-<number> <key>, so no row was checked against it") }
    return @{ Items = $items; Faults = $faults }
}

# The area and the wave of every FR item the waves section of fix-round.md names, with the
# line that names it. A wave line is '- Wave <n>', an area line '  - [<n><letter>, ][then ]F<n>
# <title>: <items>', its items read from its first sentence, 'FR-a to FR-b' a range. The
# paragraph '- Beside the waves' gives each 'F<n> <title>, FR-<n>' pair the wave beside the
# waves. An item named in two areas is a fault, and so is a file with no waves section.
function Get-FixRoundWaves([string[]] $Lines) {
    $of = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $faults = New-Object System.Collections.Generic.List[string]
    $start = -1
    for ($k = 0; $k -lt $Lines.Length; $k++) { if ($Lines[$k].StartsWith("## The waves")) { $start = $k; break } }
    if ($start -lt 0) {
        $faults.Add("fix-round.md has no section headed ## The waves, so no FR row's area or wave was checked")
        return @{ Of = $of; Faults = $faults; Read = $false }
    }

    $named = New-Object System.Collections.Generic.List[object]
    $wave = "UNKNOWN"
    for ($k = $start + 1; $k -lt $Lines.Length -and -not $Lines[$k].StartsWith("## "); $k++) {
        $line = $Lines[$k]
        $m = [regex]::Match($line, '^- Wave (\d+)')
        if ($m.Success) { $wave = $m.Groups[1].Value; continue }
        if ($line.StartsWith("- Beside the waves")) {
            $text = $line
            for ($j = $k + 1; $j -lt $Lines.Length -and [regex]::IsMatch($Lines[$j], '^  [^ -]'); $j++) { $text += " " + $Lines[$j].Trim() }
            foreach ($pair in [regex]::Matches($text, '\b(F\d+) [^,]*, (FR-\d+)')) {
                $named.Add(@{ Id = $pair.Groups[2].Value; Area = $pair.Groups[1].Value; Wave = "beside the waves"; Line = $k + 1 })
            }
            continue
        }
        $m = [regex]::Match($line, '^  - (?:(\d+[a-z]), )?(?:then )?(F\d+) [^:]*: (.*)$')
        if (-not $m.Success) { continue }
        $w = if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $wave }
        $body = $m.Groups[3].Value
        $stop = $body.IndexOf(". ")
        $first = if ($stop -ge 0) { $body.Substring(0, $stop) } else { $body }
        $ids = New-Object System.Collections.Generic.List[string]
        foreach ($range in [regex]::Matches($first, 'FR-(\d+) to FR-(\d+)')) {
            for ($v = [int]$range.Groups[1].Value; $v -le [int]$range.Groups[2].Value; $v++) { $ids.Add("FR-" + $v.ToString("000")) }
        }
        foreach ($one in [regex]::Matches($first, 'FR-\d+')) { if (-not $ids.Contains($one.Value)) { $ids.Add($one.Value) } }
        foreach ($id in $ids) { $named.Add(@{ Id = $id; Area = $m.Groups[2].Value; Wave = $w; Line = $k + 1 }) }
    }

    foreach ($one in $named) {
        if (-not $of.ContainsKey($one.Id)) { $of[$one.Id] = $one; continue }
        $was = $of[$one.Id]
        if ($was.Area -cne $one.Area) { $faults.Add("fix-round.md line " + $one.Line + " names " + $one.Id + " in area " + $one.Area + ", and line " + $was.Line + " in area " + $was.Area) }
    }
    return @{ Of = $of; Faults = $faults; Read = $true }
}

# The FR rows against fix-round.md: every item has a row with its id written exactly, and the
# row's class, area and wave are what fix-round.md gives. Returns the faults and the count of
# items read.
function Test-TrackerFixRound($Rows, [string] $RoundPath) {
    $faults = New-Object System.Collections.Generic.List[string]
    $lines = (Read-TrackerText $RoundPath).Split("`n")
    $read = Get-FixRoundItems $lines
    $waves = Get-FixRoundWaves $lines
    foreach ($f in $read.Faults) { $faults.Add($f) }
    foreach ($f in $waves.Faults) { $faults.Add($f) }

    $byId = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    foreach ($row in $Rows) { if (-not $byId.ContainsKey($row["id"])) { $byId[$row["id"]] = $row } }

    foreach ($item in $read.Items) {
        if (-not $byId.ContainsKey($item.Id)) {
            $faults.Add("fix-round.md line " + $item.Line + " names " + $item.Id + " and tracker.csv has no row for it")
            continue
        }
        $row = $byId[$item.Id]
        $where = "tracker.csv line " + $row.Line + ", id " + $item.Id
        if ($row["class"] -cne $item.Class) {
            if ($item.ClassLine -gt 0) { $faults.Add("${where}: the class '" + $row["class"] + "' is not '" + $item.Class + "', which fix-round.md line " + $item.ClassLine + " gives") }
            else { $faults.Add("${where}: the class '" + $row["class"] + "' is not 'UNKNOWN', since its item at fix-round.md line " + $item.Line + " has no Class line") }
        }
        if (-not $waves.Read) { continue }
        foreach ($column in @("area", "wave")) {
            if ($waves.Of.ContainsKey($item.Id)) {
                $named = $waves.Of[$item.Id]
                $want = if ($column -ceq "area") { $named.Area } else { $named.Wave }
                if ($row[$column] -cne $want) { $faults.Add("${where}: the $column '" + $row[$column] + "' is not '$want', which fix-round.md line " + $named.Line + " gives") }
            } elseif ($row[$column] -cne "none") {
                $faults.Add("${where}: the $column '" + $row[$column] + "' is not 'none', since the waves of fix-round.md name it in no area")
            }
        }
    }
    return @{ Faults = $faults; Count = $read.Items.Count }
}

# A wave that starts with a digit sorts before one that does not, each group in ordinal order.
function Get-WaveOrder($Rows) {
    $waves = New-Object System.Collections.Generic.List[string]
    foreach ($row in $Rows) { if (-not $waves.Contains($row["wave"])) { $waves.Add($row["wave"]) } }
    $numbered = [string[]]@($waves | Where-Object { $_ -match '^\d' })
    $named = [string[]]@($waves | Where-Object { $_ -notmatch '^\d' })
    [Array]::Sort($numbered, [StringComparer]::Ordinal)
    [Array]::Sort($named, [StringComparer]::Ordinal)
    return @($numbered) + @($named)
}

function Format-TrackerCell([string] $Value) { return $Value.Replace("|", "\|") }

# A count with its noun, the noun for one when the count is one.
function Format-TrackerCount([int] $Count, [string] $One, [string] $Many) {
    if ($Count -eq 1) { return "1 $One" }
    return "$Count $Many"
}

# Words joined by commas, the last after and.
function Join-TrackerWords([string[]] $Words) {
    if ($Words.Length -le 1) { return ($Words -join "") }
    return ($Words[0..($Words.Length - 2)] -join ", ") + ", and " + $Words[-1]
}

# steps\tracker.md as make-tracker.ps1 writes it, LF line ends, from rows Test-TrackerRows passed.
function Format-TrackerMarkdown($Rows) {
    $out = New-Object System.Collections.Generic.List[string]
    $waves = Get-WaveOrder $Rows

    $byStatus = foreach ($status in $TrackerStatuses) { "$status " + @($Rows | Where-Object { $_["status"] -ceq $status }).Count }
    $byWave = foreach ($wave in $waves) { "$wave " + @($Rows | Where-Object { $_["wave"] -ceq $wave }).Count }
    $going = @($Rows | Where-Object { $_["status"] -ceq "in progress" })
    $goingWords = @($going | Where-Object { -not $_["id"].StartsWith("FR-") } | ForEach-Object { $_["id"] + " " + $_["short title"] })
    $goingItems = @($going | Where-Object { $_["id"].StartsWith("FR-") }).Count
    if ($goingItems -gt 0) { $goingWords += Format-TrackerCount $goingItems "FR item" "FR items" }
    $waiting = @($Rows | Where-Object { $_["status"] -ceq "waiting for Bader" } | ForEach-Object { $_["id"] })

    $out.Add("# The work tracker")
    $out.Add("")
    $out.Add("Made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never by hand. Status lives in steps\tracker.csv only.")
    $out.Add("")
    $out.Add("- By status, of " + (Format-TrackerCount $Rows.Count "row" "rows") + ": " + ($byStatus -join ", "))
    $out.Add("- By wave: " + ($byWave -join ", "))
    if ($goingWords.Length -eq 0) { $out.Add("- In progress now: nothing") }
    else { $out.Add("- In progress now: " + (Join-TrackerWords $goingWords)) }
    if ($waiting.Count -eq 0) { $out.Add("- Waits for Bader: nothing") }
    else { $out.Add("- Waits for Bader, " + (Format-TrackerCount $waiting.Count "row" "rows") + ": " + ($waiting -join ", ")) }

    foreach ($wave in $waves) {
        $out.Add("")
        if ($wave -match '^\d') { $out.Add("## Wave $wave") } else { $out.Add("## $wave") }
        $out.Add("")
        $out.Add("| " + ((@($TrackerColumns | Where-Object { $_ -cne "wave" })) -join " | ") + " |")
        $out.Add("|" + ("---|" * ($TrackerColumns.Length - 1)))
        foreach ($row in $Rows) {
            if ($row["wave"] -cne $wave) { continue }
            $cells = foreach ($column in $TrackerColumns) { if ($column -cne "wave") { Format-TrackerCell $row[$column] } }
            $out.Add("| " + ($cells -join " | ") + " |")
        }
    }
    return ($out -join "`n") + "`n"
}
