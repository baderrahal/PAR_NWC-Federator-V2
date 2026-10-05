<#
    The one place the tracker's rules live, read by make-tracker.ps1 and check-tracker.ps1
    with a dot. F133, Bader's message of 5 Oct 2026, Q129.

    steps\tracker.csv holds one row per item: every FR item of steps\fix-round.md, every F
    area, Bader's requests and every question waiting for him. Its columns are the nine
    below, in Bader's order. A status is one of the seven below and nothing else. UNKNOWN is
    written where a value cannot be read, and is refused in the id and the status. No cell
    is empty.

    The csv is read by the rules of RFC 4180 and nothing looser: a cell is quoted when it
    holds a comma, a quote or a line break, a quote inside a quoted cell is written twice,
    and every row has as many cells as the header. A UTF-8 byte order mark is read past and
    CRLF reads as LF, since a Windows checkout may convert either. Windows PowerShell 5.1.
#>

$TrackerColumns = @("id", "short title", "area", "wave", "class", "status", "PR", "run that proved it", "date of last change")
$TrackerStatuses = @("open", "in progress", "in review", "merged", "proven by a run", "waiting for Bader", "dropped")

# Reads the csv into its rows, each with the line it starts on. Faults name the line. A fault
# in the quoting ends the read, since nothing after it can be placed in a row.
function Read-TrackerCsv([string] $Path) {
    $faults = New-Object System.Collections.Generic.List[string]
    $rows = New-Object System.Collections.Generic.List[object]
    $text = [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8).Replace("`r`n", "`n")
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

# The faults of rows that parsed: an empty cell, an id twice, UNKNOWN in the id, and a
# status off the list. Each names the line and the id.
function Test-TrackerRows($Rows) {
    $faults = New-Object System.Collections.Generic.List[string]
    $seen = @{}
    foreach ($row in $Rows) {
        $id = $row["id"]
        $where = "tracker.csv line " + $row.Line + ", id " + $id
        if ($id -ceq "UNKNOWN") {
            $faults.Add("tracker.csv line " + $row.Line + ": the id is UNKNOWN, and UNKNOWN is accepted only outside the id and status columns")
        } elseif ($id.Length -gt 0) {
            if ($seen.ContainsKey($id)) { $faults.Add("${where}: the id is on line " + $seen[$id] + " already") } else { $seen[$id] = $row.Line }
        }
        foreach ($column in $TrackerColumns) {
            if ($row[$column].Length -eq 0) { $faults.Add("${where}: the $column cell is empty, write UNKNOWN when it cannot be read") }
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

# Every FR item of fix-round.md, by its heading, with the line it is on.
function Get-FixRoundItems([string] $Path) {
    $items = New-Object System.Collections.Generic.List[object]
    $lines = [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8).Replace("`r`n", "`n").Split("`n")
    for ($k = 0; $k -lt $lines.Length; $k++) {
        $m = [regex]::Match($lines[$k], '^### (FR-\d+)(\s|$)')
        if ($m.Success) { $items.Add(@{ Id = $m.Groups[1].Value; Line = $k + 1 }) }
    }
    return $items
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

# steps\tracker.md as make-tracker.ps1 writes it, LF line ends, from rows Test-TrackerRows passed.
function Format-TrackerMarkdown($Rows) {
    $out = New-Object System.Collections.Generic.List[string]
    $waves = Get-WaveOrder $Rows

    $byStatus = foreach ($status in $TrackerStatuses) { "$status " + @($Rows | Where-Object { $_["status"] -ceq $status }).Count }
    $byWave = foreach ($wave in $waves) { "$wave " + @($Rows | Where-Object { $_["wave"] -ceq $wave }).Count }
    $going = @($Rows | Where-Object { $_["status"] -ceq "in progress" })
    $goingNamed = @($going | Where-Object { -not $_["id"].StartsWith("FR-") } | ForEach-Object { $_["id"] + " " + $_["short title"] })
    $goingItems = @($going | Where-Object { $_["id"].StartsWith("FR-") }).Count
    $waiting = @($Rows | Where-Object { $_["status"] -ceq "waiting for Bader" } | ForEach-Object { $_["id"] })

    $out.Add("# The work tracker")
    $out.Add("")
    $out.Add("Made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never by hand. Status lives in steps\tracker.csv only.")
    $out.Add("")
    $out.Add("- By status, of " + $Rows.Count + " rows: " + ($byStatus -join ", "))
    $out.Add("- By wave: " + ($byWave -join ", "))
    if ($going.Count -eq 0) { $out.Add("- In progress now: nothing") }
    else { $out.Add("- In progress now: " + ($goingNamed -join ", ") + ", and $goingItems FR items") }
    if ($waiting.Count -eq 0) { $out.Add("- Waits for Bader: nothing") }
    else { $out.Add("- Waits for Bader, " + $waiting.Count + " rows: " + ($waiting -join ", ")) }

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
