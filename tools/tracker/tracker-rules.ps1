<#
    The one place the tracker's rules live, read by make-tracker.ps1, check-tracker.ps1 and
    check-progress.ps1 with a dot. F133, Bader's message of 5 Oct 2026, Q129, and for the counts
    of steps\PROGRESS.md, F139, his message of 6 Oct 2026, Q139.

    steps\tracker.csv holds one row per item: every FR item of steps\fix-round.md, every F
    area, Bader's requests and every question waiting for him, whose row stays once he answers.
    A question answered before it ever had a row has none. Its columns are the nine below, in
    Bader's order. A status is one of the seven below and nothing else. UNKNOWN is written
    where a value cannot be read, and is refused in the id and the status in any case. No cell
    is empty or blank, no cell holds a line break, since tracker.md gives each row one line, no
    id has a space before or after it, and only a row proven by a run names a run in the run
    column, every other row reading none there.

    The class, the area and the wave of an FR row are read off fix-round.md, so the check
    compares them with it, and an FR row with no item there is refused. So are the area and the
    wave of the row of each F area its waves section places, and of each row of class question,
    read off the FR items naming its question. A row of class question whose question shows
    Bader's answer in steps\02_questions.md reads merged with a number, or in review on the
    branch that records the answer, and one whose question has no answer reads waiting for
    Bader. An item of 02_questions.md whose text starts From Bader, is a request of his and not
    a question, and has a row of class Bader's request.

    Every file is read as UTF-8. A UTF-8 byte order mark is read past and CRLF reads as LF,
    since a Windows checkout may convert either. The csv is read by the rules of RFC 4180 and
    nothing looser: a cell is quoted when it holds a comma, a quote or a line break, a quote
    inside a quoted cell is written twice, and every row has as many cells as the header.
    Windows PowerShell 5.1.
#>

$TrackerColumns = @("id", "short title", "area", "wave", "class", "status", "PR", "the run that proved it", "the date of the last change")
$TrackerStatuses = @("open", "in progress", "in review", "merged", "proven by a run", "waiting for Bader", "dropped")

# steps\PROGRESS.md, the one page a session starts from, F139. It is at most this many lines, and
# its counts sit between these two marker lines, made by make-tracker.ps1 and never typed.
$ProgressMaxLines = 60
$ProgressStartMarker = "<!-- the counts below are made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never typed -->"
$ProgressEndMarker = "<!-- the end of the counts -->"
# Bader's five words in his order, each with the statuses of the seven it counts, then dropped
# beside them, so every row is counted once and each line adds up. Done is merged and proven by
# a run together, the lead's reading (a) under Q139.
$ProgressColumns = [ordered]@{
    "done"              = @("merged", "proven by a run")
    "in progress"       = @("in progress")
    "in review"         = @("in review")
    "waiting for Bader" = @("waiting for Bader")
    "open"              = @("open")
    "dropped"           = @("dropped")
}
# The line of the counts for every wave value that does not start with a product wave.
$ProgressOutside = "outside the waves"
# A product wave is a wave value that starts with this pattern, and the part it matches is the
# product wave the value names, so 2a and 2a and 2b both name 2a. Every reader of a product wave
# reads it through Get-ProductWave, the one place this rule is written.
$ProductWavePattern = '^\d+[a-z]*'

# A text file as UTF-8, past a byte order mark, with CRLF read as LF. Bytes that are not UTF-8,
# a file saved as UTF-16 or in a Windows code page among them, give a fault naming the first
# line that holds one, so a character that read wrong never reads clean. The decoder puts the
# replacement character where a byte is not UTF-8, so only a text holding one is read back.
function Read-TrackerText([string] $Path, [string] $Name) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    $skip = 0
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { $skip = 3 }
    $utf8 = New-Object System.Text.UTF8Encoding $false
    $text = $utf8.GetString($bytes, $skip, $bytes.Length - $skip)
    $fault = $null
    $at = $text.IndexOf([char]0xFFFD)
    if ($at -ge 0) {
        $back = $utf8.GetBytes($text)
        $same = $back.Length -eq ($bytes.Length - $skip)
        for ($k = 0; $same -and $k -lt $back.Length; $k++) { if ($back[$k] -ne $bytes[$skip + $k]) { $same = $false } }
        if (-not $same) { $fault = "$Name line " + $text.Substring(0, $at).Split("`n").Length + " holds bytes that are not UTF-8, so the file was not read" }
    }
    return @{ Text = $text.Replace("`r`n", "`n"); Fault = $fault }
}

# Reads the csv into its rows, each with the line it starts on. Faults name the line. A fault
# in the quoting ends the read, since nothing after it can be placed in a row.
function Read-TrackerCsv([string] $Path) {
    $faults = New-Object System.Collections.Generic.List[string]
    $rows = New-Object System.Collections.Generic.List[object]
    $read = Read-TrackerText $Path "tracker.csv"
    if ($null -ne $read.Fault) {
        $faults.Add($read.Fault)
        return @{ Rows = $rows; Faults = $faults }
    }
    $text = $read.Text
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

# The faults of rows that parsed: an empty or blank cell, a line break in a cell, an id with a
# space before or after it, an id twice, UNKNOWN in the id, a status off the list, and a run
# named in the run column of a row whose status is another of the seven than proven by a run,
# where only none is right, UNKNOWN included, since no run proved it. Each names the line and
# the id. Two ids that differ only in case or in a space around them count as the same id
# twice, and UNKNOWN in any case is refused in the id.
function Test-TrackerRows($Rows) {
    $faults = New-Object System.Collections.Generic.List[string]
    $seen = New-Object 'System.Collections.Generic.Dictionary[string,int]' ([StringComparer]::OrdinalIgnoreCase)
    foreach ($row in $Rows) {
        $id = $row["id"]
        $key = $id.Trim()
        $where = "tracker.csv line " + $row.Line + ", id " + $id
        if ($key -eq "UNKNOWN") {
            $faults.Add("tracker.csv line " + $row.Line + ": the id is UNKNOWN, and UNKNOWN is accepted only outside the id and status columns")
        } elseif ($key.Length -gt 0) {
            if ($id -cne $key) { $faults.Add("tracker.csv line " + $row.Line + ": the id '$id' has a space before or after it") }
            if ($seen.ContainsKey($key)) { $faults.Add("${where}: the id is on line " + $seen[$key] + " already") } else { $seen[$key] = $row.Line }
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
        $run = $row["the run that proved it"]
        if ($TrackerStatuses -ccontains $status -and $status -cne "proven by a run" -and $run.Trim().Length -gt 0 -and $run -cne "none") {
            $faults.Add("${where}: the run that proved it reads '$run' while the status is '$status', and only a row proven by a run names a run, so it reads none")
        }
    }
    return $faults
}

# Every FR item of fix-round.md by its heading, ### FR-<number> <key>, with its line, its
# Class line, and the questions its section names, Q<n> or each of a range Q<a> to Q<b>, each
# with the first line that names it. A heading that names an FR number in any other shape is a
# fault, such as ### FR-190: x, #### FR-190, ### FR190 or ### Item FR-190, and so are a second
# heading of one item and a file with no item at all, so a heading the reader cannot see never
# reads as nothing to check.
function Get-FixRoundItems([string[]] $Lines) {
    $items = New-Object System.Collections.Generic.List[object]
    $faults = New-Object System.Collections.Generic.List[string]
    $first = New-Object 'System.Collections.Generic.Dictionary[string,int]' ([StringComparer]::Ordinal)
    $ignoreCase = [Text.RegularExpressions.RegexOptions]::IgnoreCase
    $item = $null
    for ($k = 0; $k -lt $Lines.Length; $k++) {
        $line = $Lines[$k]
        $m = [regex]::Match($line, '^### (FR-\d+)(\s|$)')
        if ($m.Success) {
            $id = $m.Groups[1].Value
            $item = $null
            if ($first.ContainsKey($id)) {
                $faults.Add("fix-round.md line " + ($k + 1) + " is a second heading of $id, whose first is at line " + $first[$id])
                continue
            }
            $first[$id] = $k + 1
            $item = @{ Id = $id; Line = $k + 1; Class = "UNKNOWN"; ClassLine = 0; Asks = New-Object 'System.Collections.Generic.Dictionary[string,int]' ([StringComparer]::Ordinal) }
            $items.Add($item)
            continue
        }
        if ([regex]::IsMatch($line, '^#{1,6}\s*\W*FR\W?\d', $ignoreCase) -or [regex]::IsMatch($line, '^#{3,6}\s.*\bFR\W?\d', $ignoreCase)) {
            $faults.Add("fix-round.md line " + ($k + 1) + " looks like the heading of an FR item and is not in the shape ### FR-<number> <key>, so it was not read as one")
            $item = $null
            continue
        }
        if ($line.StartsWith("#")) { $item = $null; continue }
        if ($null -eq $item) { continue }
        if ($item.ClassLine -eq 0 -and $line.StartsWith("- Class:")) {
            $item.Class = $line.Substring("- Class:".Length).Split(",")[0].Trim()
            $item.ClassLine = $k + 1
        }
        foreach ($range in [regex]::Matches($line, '\bQ(\d+) to Q(\d+)\b')) {
            for ($v = [int]$range.Groups[1].Value; $v -le [int]$range.Groups[2].Value; $v++) {
                if (-not $item.Asks.ContainsKey("$v")) { $item.Asks["$v"] = $k + 1 }
            }
        }
        foreach ($one in [regex]::Matches($line, '\bQ(\d+)\b')) {
            if (-not $item.Asks.ContainsKey($one.Groups[1].Value)) { $item.Asks[$one.Groups[1].Value] = $k + 1 }
        }
    }
    if ($items.Count -eq 0) { $faults.Add("fix-round.md has no heading of an FR item in the shape ### FR-<number> <key>, so no row was checked against it") }
    return @{ Items = $items; Faults = $faults }
}

# The line of fix-round.md where an entry of the waves section first names an id, or the
# entry's first line when it names the id only inside a range.
function Find-WavesLine($Entry, [string] $Id) {
    for ($j = 0; $j -lt $Entry.Texts.Count; $j++) {
        if ([regex]::IsMatch($Entry.Texts[$j], '\b' + $Id + '\b')) { return $Entry.Line + $j }
    }
    return $Entry.Line
}

# The area and the wave of every FR item the waves section of fix-round.md names, with the
# line that names it. Every line of the section is one of these shapes, or a fault naming it:
# - a blank line
# - a line of prose, starting with a letter after a blank line or another line of prose, read
#   as narrative and not read for ids, so an FR id or an F area named in prose places nothing
#   and is not checked
# - a wave line, '- Wave <n>[, <words>]:', the wave of the area lines under it
# - an area line under a wave line, '  - [<part>, ][then ]F<n> <title>: <items>', which places
#   its area F<n> at the part when it has one and the wave when not, and the items of the first
#   sentence after the colon in it, FR-a to FR-b a range. It may place no item
# - a stage line, '- <stage>, <words>: <body>', each 'F<n> <title>, FR-<n>' of the first
#   sentence of its body placing the area and the item at the wave '<stage>', its first letter
#   small, such as beside the waves or before any test run. Any other F<n> of that sentence is
#   an area it names in an order and does not place. A stage line may place nothing
# - a line indented under a wave, area or stage line, which continues it
# Past the first sentence an area or a stage line may name again an item it places, and after
# 'Closed already:' the items closed before the waves, which read as none for both. An FR id
# named anywhere else in a wave, area or stage line is a fault, so no item reads as in no area
# in silence. So is a line starting with a letter right under a wave, area or stage line, which
# Markdown reads as part of that line, an area line with no wave line above it, an item two
# lines place in two areas or at two waves, an area two lines place at two waves, and a file
# with no waves section. Areas holds each F area an area line or a stage line names, with the
# first line that does, and AreaAt the wave of each it places, with the line.
function Get-FixRoundWaves([string[]] $Lines) {
    $of = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $areas = New-Object 'System.Collections.Generic.Dictionary[string,int]' ([StringComparer]::Ordinal)
    $areaAt = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $faults = New-Object System.Collections.Generic.List[string]
    $start = -1
    for ($k = 0; $k -lt $Lines.Length; $k++) { if ($Lines[$k].StartsWith("## The waves")) { $start = $k; break } }
    if ($start -lt 0) {
        $faults.Add("fix-round.md has no section headed ## The waves, so no FR row's area or wave was checked")
        return @{ Of = $of; Areas = $areas; AreaAt = $areaAt; Faults = $faults; Read = $false }
    }
    $noShape = ", in the waves section, is in no shape the waves reader knows, so nothing on it was read"

    $entries = New-Object System.Collections.Generic.List[object]
    $entry = $null
    for ($k = $start + 1; $k -lt $Lines.Length -and -not $Lines[$k].StartsWith("## "); $k++) {
        $line = $Lines[$k]
        if ($line.Trim().Length -eq 0) { $entry = $null; continue }
        if ([regex]::IsMatch($line, '^[A-Za-z]')) {
            if ($null -ne $entry) { $faults.Add("fix-round.md line " + ($k + 1) + ", in the waves section, starts with a letter right under a wave, area or stage line, so Markdown reads it as part of that line and the waves reader does not, put a blank line above it or indent it") }
            $entry = $null
            continue
        }
        if ($line.StartsWith("- ") -or $line.StartsWith("  - ")) {
            $entry = @{ Line = $k + 1; Top = $line.StartsWith("- "); Texts = New-Object System.Collections.Generic.List[string] }
            $entry.Texts.Add($line)
            $entries.Add($entry)
            continue
        }
        if ($null -ne $entry -and [regex]::IsMatch($line, '^ +[^ -]')) { $entry.Texts.Add($line); continue }
        $faults.Add("fix-round.md line " + ($k + 1) + $noShape)
        $entry = $null
    }

    $named = New-Object System.Collections.Generic.List[object]
    $wave = $null
    foreach ($e in $entries) {
        $text = $e.Texts[0]
        for ($j = 1; $j -lt $e.Texts.Count; $j++) { $text += " " + $e.Texts[$j].Trim() }
        $placed = New-Object System.Collections.Generic.List[string]
        $rest = $text
        if ($text.StartsWith("- Wave ")) {
            $m = [regex]::Match($text, '^- Wave (\d+)(, [^:]*)?:$')
            if (-not $m.Success) { $faults.Add("fix-round.md line " + $e.Line + $noShape); continue }
            $wave = $m.Groups[1].Value
        } else {
            if ($e.Top) {
                $m = [regex]::Match($text, '^- ([A-Za-z][^,:]*), [^:]*: (.+)$')
                if (-not $m.Success) { $faults.Add("fix-round.md line " + $e.Line + $noShape); continue }
                $wave = $null
                $body = $m.Groups[2].Value
                $at = $m.Groups[1].Value.Substring(0, 1).ToLowerInvariant() + $m.Groups[1].Value.Substring(1)
            } else {
                $m = [regex]::Match($text, '^  - (?:(\d+[a-z]), )?(?:then )?(F\d+) [^:]*: (.+)$')
                if (-not $m.Success) { $faults.Add("fix-round.md line " + $e.Line + $noShape); continue }
                if ($null -eq $wave) { $faults.Add("fix-round.md line " + $e.Line + " is an area line with no wave line above it, so its wave was not read"); continue }
                $body = $m.Groups[3].Value
                $at = if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $wave }
            }
            $stop = $body.IndexOf(". ")
            $first = if ($stop -ge 0) { $body.Substring(0, $stop) } else { $body }
            $rest = $text.Substring(0, $text.Length - $body.Length) + $(if ($stop -ge 0) { $body.Substring($stop) } else { "" })
            $placing = New-Object System.Collections.Generic.List[object]
            if ($e.Top) {
                foreach ($pair in [regex]::Matches($first, '\b(F\d+) [^,]*, (FR-\d+)\b')) {
                    $placed.Add($pair.Groups[2].Value)
                    $named.Add(@{ Id = $pair.Groups[2].Value; Area = $pair.Groups[1].Value; Wave = $at; Line = (Find-WavesLine $e $pair.Groups[2].Value) })
                    $placing.Add(@{ Area = $pair.Groups[1].Value; Line = (Find-WavesLine $e $pair.Groups[1].Value) })
                }
                foreach ($one in [regex]::Matches($first, 'FR-\d+')) {
                    if (-not $placed.Contains($one.Value)) { $rest += " " + $one.Value }
                }
            } else {
                foreach ($range in [regex]::Matches($first, 'FR-(\d+) to FR-(\d+)')) {
                    for ($v = [int]$range.Groups[1].Value; $v -le [int]$range.Groups[2].Value; $v++) { $placed.Add("FR-" + $v.ToString("000")) }
                }
                foreach ($one in [regex]::Matches($first, 'FR-\d+')) { if (-not $placed.Contains($one.Value)) { $placed.Add($one.Value) } }
                foreach ($id in $placed) { $named.Add(@{ Id = $id; Area = $m.Groups[2].Value; Wave = $at; Line = (Find-WavesLine $e $id) }) }
                $placing.Add(@{ Area = $m.Groups[2].Value; Line = $e.Line })
            }
            foreach ($p in $placing) {
                if (-not $areas.ContainsKey($p.Area)) { $areas[$p.Area] = $p.Line }
                if (-not $areaAt.ContainsKey($p.Area)) { $areaAt[$p.Area] = @{ Wave = $at; Line = $p.Line } }
                elseif ($areaAt[$p.Area].Wave -cne $at) { $faults.Add("fix-round.md line " + $p.Line + " places area " + $p.Area + " at wave $at, and line " + $areaAt[$p.Area].Line + " at wave " + $areaAt[$p.Area].Wave) }
            }
            if ($e.Top) {
                foreach ($one in [regex]::Matches($first, '\bF\d+\b')) {
                    if (-not $areas.ContainsKey($one.Value)) { $areas[$one.Value] = Find-WavesLine $e $one.Value }
                }
            }
        }

        $closed = New-Object System.Collections.Generic.List[string]
        foreach ($sentence in ($rest -split '\. ')) {
            if (-not $sentence.StartsWith("Closed already: ")) { continue }
            foreach ($one in [regex]::Matches($sentence, 'FR-\d+')) {
                if ($closed.Contains($one.Value)) { continue }
                $closed.Add($one.Value)
                $named.Add(@{ Id = $one.Value; Area = "none"; Wave = "none"; Line = (Find-WavesLine $e $one.Value) })
            }
        }
        $told = New-Object System.Collections.Generic.List[string]
        foreach ($one in [regex]::Matches($rest, 'FR-\d+')) {
            if ($placed.Contains($one.Value) -or $closed.Contains($one.Value) -or $told.Contains($one.Value)) { continue }
            $told.Add($one.Value)
            $faults.Add("fix-round.md line " + (Find-WavesLine $e $one.Value) + " names " + $one.Value + " where the waves reader places no item, so its area and wave were not read")
        }
    }

    foreach ($one in $named) {
        if (-not $of.ContainsKey($one.Id)) { $of[$one.Id] = $one; continue }
        $was = $of[$one.Id]
        if ($was.Area -cne $one.Area) { $faults.Add("fix-round.md line " + $one.Line + " names " + $one.Id + " in area " + $one.Area + ", and line " + $was.Line + " in area " + $was.Area) }
        elseif ($was.Wave -cne $one.Wave) { $faults.Add("fix-round.md line " + $one.Line + " names " + $one.Id + " at wave " + $one.Wave + ", and line " + $was.Line + " at wave " + $was.Wave) }
    }
    return @{ Of = $of; Areas = $areas; AreaAt = $areaAt; Faults = $faults; Read = $true }
}

# The rows against fix-round.md: every item has a row with its id written exactly, every row
# whose id starts FR- in any case has its item, and the row's class, area and wave are what
# fix-round.md gives. Every F area an area line or a stage line of the waves section names has a
# row with its id, and the row of each area it places reads that area and the wave it is placed
# at. Returns the faults, the count of items read, the count of areas, and Places, each
# question's number with the items naming it in file order, their line, area and wave, or null
# when a heading or a line of the waves could not be read, so no question row is compared with
# what was read in part. That fault is named on its own line, so nothing passes in silence.
function Test-TrackerFixRound($Rows, [string] $RoundPath) {
    $faults = New-Object System.Collections.Generic.List[string]
    $round = Read-TrackerText $RoundPath "fix-round.md"
    if ($null -ne $round.Fault) {
        $faults.Add($round.Fault)
        return @{ Faults = $faults; Count = 0; Areas = 0; Places = $null }
    }
    $lines = $round.Text.Split("`n")
    $read = Get-FixRoundItems $lines
    $waves = Get-FixRoundWaves $lines
    foreach ($f in $read.Faults) { $faults.Add($f) }
    foreach ($f in $waves.Faults) { $faults.Add($f) }

    $byId = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    foreach ($row in $Rows) { if (-not $byId.ContainsKey($row["id"])) { $byId[$row["id"]] = $row } }
    $itemIds = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)

    foreach ($item in $read.Items) {
        [void]$itemIds.Add($item.Id)
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
    foreach ($area in $waves.Areas.Keys) {
        if (-not $byId.ContainsKey($area)) { $faults.Add("fix-round.md line " + $waves.Areas[$area] + " names area $area and tracker.csv has no row for it"); continue }
        if (-not $waves.AreaAt.ContainsKey($area)) { continue }
        $row = $byId[$area]
        $at = $waves.AreaAt[$area]
        $where = "tracker.csv line " + $row.Line + ", id $area"
        if ($row["area"] -cne $area) { $faults.Add("${where}: the area '" + $row["area"] + "' is not '$area', its own id, since fix-round.md line " + $at.Line + " places it as an area") }
        if ($row["wave"] -cne $at.Wave) { $faults.Add("${where}: the wave '" + $row["wave"] + "' is not '" + $at.Wave + "', which fix-round.md line " + $at.Line + " gives") }
    }
    $places = $null
    if ($read.Items.Count -gt 0) {
        foreach ($row in $Rows) {
            $id = $row["id"].Trim()
            if ($id.StartsWith("FR-", [StringComparison]::OrdinalIgnoreCase) -and -not $itemIds.Contains($id)) {
                $faults.Add("tracker.csv line " + $row.Line + ", id " + $row["id"] + ": fix-round.md has no item headed with that id")
            }
        }
        if ($waves.Read -and $read.Faults.Count -eq 0 -and $waves.Faults.Count -eq 0) {
            $places = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
            foreach ($item in $read.Items) {
                $area = "none"
                $wave = "none"
                if ($waves.Of.ContainsKey($item.Id)) { $area = $waves.Of[$item.Id].Area; $wave = $waves.Of[$item.Id].Wave }
                foreach ($number in $item.Asks.Keys) {
                    if (-not $places.ContainsKey($number)) { $places[$number] = New-Object System.Collections.Generic.List[object] }
                    $places[$number].Add(@{ Id = $item.Id; Line = $item.Asks[$number]; Area = $area; Wave = $wave })
                }
            }
        }
    }
    return @{ Faults = $faults; Count = $read.Items.Count; Areas = $waves.Areas.Count; Places = $places }
}

# The area and the wave of a question, off the items of fix-round.md that name it: the areas of
# those placed in an area, in file order, joined by a comma, and their waves joined by and, or
# none and none when no item in an area names it. From says which items gave it.
function Get-QuestionPlace($Places, [string] $Number) {
    $areas = New-Object System.Collections.Generic.List[string]
    $waves = New-Object System.Collections.Generic.List[string]
    $from = New-Object System.Collections.Generic.List[string]
    if ($Places.ContainsKey($Number)) {
        foreach ($p in $Places[$Number]) {
            if ($p.Area -ceq "none") { continue }
            if (-not $areas.Contains($p.Area)) { $areas.Add($p.Area) }
            if (-not $waves.Contains($p.Wave)) { $waves.Add($p.Wave) }
            $from.Add($p.Id + " at line " + $p.Line)
        }
    }
    if ($areas.Count -eq 0) { return @{ Area = "none"; Wave = "none"; From = $null } }
    return @{ Area = ($areas -join ", "); Wave = ($waves -join " and "); From = (Join-TrackerWords $from.ToArray()) }
}

# Every numbered item of steps\02_questions.md by its number, a line '<n>. ' at the margin, with
# its Answer lines, those indented under it starting Answer:. It is answered when any of them
# holds text, and AnswerLine is the first that does, or the first Answer line when none does.
# An item whose text starts 'From Bader,' is a request of his and not a question put to him. A
# line that looks like the start of an item in another shape, such as 134) x, Q134. x, **134.**
# or 134. indented by up to three spaces, and a second item of one number are faults, so an item
# the reader cannot see never reads as nothing there.
function Get-Questions([string[]] $Lines) {
    $of = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $faults = New-Object System.Collections.Generic.List[string]
    $q = $null
    for ($k = 0; $k -lt $Lines.Length; $k++) {
        $m = [regex]::Match($Lines[$k], '^(\d+)\. (.*)$')
        if ($m.Success) {
            $number = $m.Groups[1].Value
            if ($of.ContainsKey($number)) {
                $faults.Add("02_questions.md line " + ($k + 1) + " is a second item numbered $number, whose first is at line " + $of[$number].Line)
                $q = $null
                continue
            }
            $q = @{ Line = $k + 1; AnswerLine = 0; Answered = $false; Request = $m.Groups[2].Value.StartsWith("From Bader,") }
            $of[$number] = $q
            continue
        }
        if ([regex]::IsMatch($Lines[$k], '^ {0,3}(\*\*)?Q?\d+\s*[.)]')) {
            $faults.Add("02_questions.md line " + ($k + 1) + " looks like the start of an item and is not in the shape <number>. at the margin, so it was not read as one")
            $q = $null
            continue
        }
        $m = [regex]::Match($Lines[$k], '^\s+Answer:(.*)$')
        if ($null -eq $q -or -not $m.Success -or $q.Answered) { continue }
        if ($m.Groups[1].Value.Trim().Length -gt 0) { $q.Answered = $true; $q.AnswerLine = $k + 1 }
        elseif ($q.AnswerLine -eq 0) { $q.AnswerLine = $k + 1 }
    }
    return @{ Of = $of; Faults = $faults }
}

# The rows of class question and of class Bader's request against steps\02_questions.md.
# A row of class question names a question there, not a request, as Q<number>, and reads the
# area and the wave Get-QuestionPlace gives off fix-round.md. Its question answered by Bader, it
# reads merged with the number of the pull request that put the answer on main, or in review on
# the branch that records it, and with no answer, an Answer line empty or none at all, it reads
# waiting for Bader. Every question with no answer has a row Q<number> of class question. A
# row of class Bader's request, but an FR row, whose class fix-round.md gives, names a request of
# his as Q<number>, or Q<number>-<k> when the item holds several, and every request has at least
# one. Places is null when fix-round.md could not be read, and then no area or wave is compared.
function Test-TrackerQuestions($Rows, [string] $QuestionsPath, $Places) {
    $faults = New-Object System.Collections.Generic.List[string]
    $read = Read-TrackerText $QuestionsPath "02_questions.md"
    if ($null -ne $read.Fault) {
        $faults.Add($read.Fault)
        return @{ Faults = $faults; Count = 0; Waiting = 0; Requests = 0 }
    }
    $got = Get-Questions ($read.Text.Split("`n"))
    foreach ($f in $got.Faults) { $faults.Add($f) }
    $questions = $got.Of
    $count = 0
    $asked = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    $met = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    foreach ($row in $Rows) {
        $where = "tracker.csv line " + $row.Line + ", id " + $row["id"]
        if ($row["class"] -ceq "Bader's request" -and -not $row["id"].StartsWith("FR-")) {
            $m = [regex]::Match($row["id"], '^Q(\d+)(-\d+)?$')
            if (-not $m.Success -or -not $questions.ContainsKey($m.Groups[1].Value) -or -not $questions[$m.Groups[1].Value].Request) {
                $faults.Add("${where}: its class is Bader's request and 02_questions.md has no request of his by that number")
            } else { [void]$met.Add($m.Groups[1].Value) }
            continue
        }
        if ($row["class"] -cne "question") { continue }
        $m = [regex]::Match($row["id"], '^Q(\d+)$')
        if (-not $m.Success -or -not $questions.ContainsKey($m.Groups[1].Value) -or $questions[$m.Groups[1].Value].Request) {
            $faults.Add("${where}: its class is question and 02_questions.md has no question by that number")
            continue
        }
        $count++
        $number = $m.Groups[1].Value
        [void]$asked.Add($number)
        $q = $questions[$number]
        if ($null -ne $Places) {
            $want = Get-QuestionPlace $Places $number
            foreach ($column in @("area", "wave")) {
                $value = if ($column -ceq "area") { $want.Area } else { $want.Wave }
                if ($row[$column] -ceq $value) { continue }
                if ($null -eq $want.From) { $faults.Add("${where}: the $column '" + $row[$column] + "' is not 'none', since no item of fix-round.md in an area of its waves names Q$number") }
                else { $faults.Add("${where}: the $column '" + $row[$column] + "' is not '$value', which fix-round.md gives off the items naming Q$number, " + $want.From) }
            }
        }
        $status = $row["status"]
        if (-not $q.Answered) {
            if ($status -cne "waiting for Bader") { $faults.Add("${where}: 02_questions.md line " + $q.Line + " asks question $number, which has no answer, so the row reads waiting for Bader and not '$status'") }
            continue
        }
        if (($status -ceq "merged" -and $row["PR"] -match '^\d+$') -or $status -ceq "in review") { continue }
        $faults.Add("${where}: 02_questions.md line " + $q.AnswerLine + " holds Bader's answer, so the row reads merged with the number of the pull request that put it on main, or in review on the branch that records it, and not '$status' with PR '" + $row["PR"] + "'")
    }
    $waiting = 0
    $requests = 0
    foreach ($number in $questions.Keys) {
        $q = $questions[$number]
        if ($q.Request) {
            $requests++
            if (-not $met.Contains($number)) { $faults.Add("02_questions.md line " + $q.Line + " holds request $number of Bader's, and tracker.csv has no row Q$number, or Q$number- and a number, of class Bader's request for it") }
            continue
        }
        if ($q.Answered) { continue }
        $waiting++
        if (-not $asked.Contains($number)) { $faults.Add("02_questions.md line " + $q.Line + " asks question $number, which has no answer, and tracker.csv has no row Q$number of class question for it") }
    }
    return @{ Faults = $faults; Count = $count; Waiting = $waiting; Requests = $requests }
}

# The product wave a wave value names, by $ProductWavePattern, or an empty string when it names none.
function Get-ProductWave([string] $Wave) {
    $m = [regex]::Match($Wave, $ProductWavePattern)
    if ($m.Success) { return $m.Value }
    return ""
}

# A wave value naming a product wave sorts before one that does not, each group in ordinal order.
function Get-OrderedWaves([string[]] $Waves) {
    $numbered = [string[]]@($Waves | Where-Object { (Get-ProductWave $_) -ne "" })
    $named = [string[]]@($Waves | Where-Object { (Get-ProductWave $_) -eq "" })
    [Array]::Sort($numbered, [StringComparer]::Ordinal)
    [Array]::Sort($named, [StringComparer]::Ordinal)
    return @($numbered) + @($named)
}

# The waves of the rows, each once, in the order Get-OrderedWaves gives.
function Get-WaveOrder($Rows) {
    $waves = New-Object System.Collections.Generic.List[string]
    foreach ($row in $Rows) { if (-not $waves.Contains($row["wave"])) { $waves.Add($row["wave"]) } }
    return Get-OrderedWaves $waves.ToArray()
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
        if ((Get-ProductWave $wave) -ne "") { $out.Add("## Wave $wave") } else { $out.Add("## $wave") }
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

# The wave a row of the csv is counted under in the counts of PROGRESS.md: the product wave its
# value names, so 2a and 2a and 2b both count under 2a, and for any other value, such as none or
# before the waves, outside the waves, the lead's reading (b) under Q139.
function Get-ProgressWave([string] $Wave) {
    $product = Get-ProductWave $Wave
    if ($product -ne "") { return $product }
    return $ProgressOutside
}

# The lines between the two marker lines of PROGRESS.md, from rows Test-TrackerRows passed: one
# line per product wave in the order Get-OrderedWaves gives, then outside the waves, then the
# total, each counting its rows under each column of $ProgressColumns and its rows in all.
function Format-ProgressCounts($Rows) {
    $at = New-Object System.Collections.Generic.List[string]
    $waves = New-Object System.Collections.Generic.List[string]
    foreach ($row in $Rows) {
        $wave = Get-ProgressWave $row["wave"]
        $at.Add($wave)
        if ($wave -cne $ProgressOutside -and -not $waves.Contains($wave)) { $waves.Add($wave) }
    }
    $order = @(Get-OrderedWaves $waves.ToArray()) + @($ProgressOutside)
    $columns = @($ProgressColumns.Keys)
    $out = New-Object System.Collections.Generic.List[string]
    $out.Add("## Counts")
    $out.Add("")
    # The sentence is read off $ProgressColumns: the first column and its statuses, then the last
    # column, which stands beside the others, and how many they are.
    $first = $columns[0]
    $beside = $columns[$columns.Length - 1]
    $others = $columns.Length - 1
    $numbers = @("no", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine")
    $many = if ($others -lt $numbers.Length) { $numbers[$others] } else { "$others" }
    $out.Add("Made from steps\tracker.csv by tools\tracker\make-tracker.ps1, never typed. " + $first.Substring(0, 1).ToUpper() + $first.Substring(1) + " is " + ($ProgressColumns[$first] -join " or ") + ", and $beside stands beside the $many so each line adds up.")
    $out.Add("")
    $out.Add("| wave | " + ($columns -join " | ") + " | rows |")
    $out.Add("|" + ("---|" * ($columns.Length + 2)))
    $sums = New-Object 'int[]' ($columns.Length + 1)
    foreach ($wave in $order) {
        $cells = New-Object 'int[]' ($columns.Length + 1)
        for ($r = 0; $r -lt $Rows.Count; $r++) {
            if ($at[$r] -cne $wave) { continue }
            for ($k = 0; $k -lt $columns.Length; $k++) { if ($ProgressColumns[$columns[$k]] -ccontains $Rows[$r]["status"]) { $cells[$k]++ } }
            $cells[$columns.Length]++
        }
        for ($k = 0; $k -lt $cells.Length; $k++) { $sums[$k] += $cells[$k] }
        $out.Add("| $wave | " + ($cells -join " | ") + " |")
    }
    $out.Add("| total | " + ($sums -join " | ") + " |")
    return $out.ToArray()
}

# The lines of a text as a person counts them: one for every line end, and one for a last line
# with no line end after it.
function Measure-ProgressLines([string] $Text) {
    $count = $Text.Split("`n").Length - 1
    if ($Text.Length -gt 0 -and -not $Text.EndsWith("`n")) { $count++ }
    return $count
}

# Where the counts sit in the lines of PROGRESS.md: Start and End are the indexes of the two
# marker lines, each read whole and case-sensitive. Faults names a marker line that is not there,
# one that is there a second time and an end before the start, each with its line, and with any
# of them the page has no place for the counts.
function Find-ProgressBlock([string[]] $Lines) {
    $faults = New-Object System.Collections.Generic.List[string]
    $first = @{}
    foreach ($mark in @(@{ Line = $ProgressStartMarker; Name = "the line above the counts" }, @{ Line = $ProgressEndMarker; Name = "the line below the counts" })) {
        $at = -1
        for ($k = 0; $k -lt $Lines.Length; $k++) {
            if ($Lines[$k] -cne $mark.Line) { continue }
            if ($at -lt 0) { $at = $k; continue }
            $faults.Add("PROGRESS.md line " + ($k + 1) + " is " + $mark.Name + " a second time, the first at line " + ($at + 1))
        }
        if ($at -lt 0) { $faults.Add("PROGRESS.md has no line reading " + $mark.Line + ", " + $mark.Name) }
        $first[$mark.Name] = $at
    }
    $start = $first["the line above the counts"]
    $end = $first["the line below the counts"]
    if ($faults.Count -eq 0 -and $end -lt $start) {
        $faults.Add("PROGRESS.md line " + ($end + 1) + ", the line below the counts, comes before line " + ($start + 1) + ", the line above them")
    }
    return @{ Start = $start; End = $end; Faults = $faults }
}
