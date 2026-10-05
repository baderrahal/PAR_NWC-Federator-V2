<#
    The one place the tracker's rules live, read by make-tracker.ps1 and check-tracker.ps1
    with a dot. F133, Bader's message of 5 Oct 2026, Q129.

    steps\tracker.csv holds one row per item: every FR item of steps\fix-round.md, every F
    area, Bader's requests and every question waiting for him. Its columns are the nine
    below, in Bader's order. A status is one of the seven below and nothing else. UNKNOWN is
    written where a value cannot be read, and is refused in the id and the status in any case.
    No cell is empty or blank, no cell holds a line break, since tracker.md gives each row one
    line, and no id has a space before or after it.

    The class, the area and the wave of an FR row are read off fix-round.md, so the check
    compares them with it, and an FR row with no item there is refused. A row of class question
    whose question shows Bader's answer in steps\02_questions.md reads merged with a number, or
    in review on the branch that records the answer.

    Every file is read as UTF-8. A UTF-8 byte order mark is read past and CRLF reads as LF,
    since a Windows checkout may convert either. The csv is read by the rules of RFC 4180 and
    nothing looser: a cell is quoted when it holds a comma, a quote or a line break, a quote
    inside a quoted cell is written twice, and every row has as many cells as the header.
    Windows PowerShell 5.1.
#>

$TrackerColumns = @("id", "short title", "area", "wave", "class", "status", "PR", "run that proved it", "date of last change")
$TrackerStatuses = @("open", "in progress", "in review", "merged", "proven by a run", "waiting for Bader", "dropped")

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
# space before or after it, an id twice, UNKNOWN in the id, and a status off the list. Each
# names the line and the id. Two ids that differ only in case or in a space around them count
# as the same id twice, and UNKNOWN in any case is refused in the id.
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
    }
    return $faults
}

# Every FR item of fix-round.md by its heading, ### FR-<number> <key>, with its line and its
# Class line. A heading that names an FR number in any other shape is a fault, such as
# ### FR-190: x, #### FR-190, ### FR190 or ### Item FR-190, and so are a second heading of one
# item and a file with no item at all, so a heading the reader cannot see never reads as
# nothing to check.
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
            $item = @{ Id = $id; Line = $k + 1; Class = "UNKNOWN"; ClassLine = 0 }
            $items.Add($item)
            continue
        }
        if ([regex]::IsMatch($line, '^#{1,6}\s*\W*FR\W?\d', $ignoreCase) -or [regex]::IsMatch($line, '^#{3,6}\s.*\bFR\W?\d', $ignoreCase)) {
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
# - a line of prose, starting with a letter, read as narrative, which places no item
# - a wave line, '- Wave <n>[, <words>]:', the wave of the area lines under it
# - an area line under a wave line, '  - [<part>, ][then ]F<n> <title>: <items>', its items
#   read from the first sentence after the colon, FR-a to FR-b a range, in that area at the
#   part when it has one and the wave when not
# - a stage line, '- <stage>, <words>: <body>', each 'F<n> <title>, FR-<n>' of the first
#   sentence of its body an item in that area at the wave '<stage>', its first letter small,
#   such as beside the waves or before any test run. A stage line may place no item
# - a line indented under a wave, area or stage line, which continues it
# Past the first sentence an area or a stage line may name again an item it places, and after
# 'Closed already:' the items closed before the waves, which read as none for both. An FR id
# named anywhere else in a wave, area or stage line is a fault, so no item reads as in no area
# in silence. So is an area line with no wave line above it, an item two lines place in two
# areas or at two waves, and a file with no waves section.
function Get-FixRoundWaves([string[]] $Lines) {
    $of = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $faults = New-Object System.Collections.Generic.List[string]
    $start = -1
    for ($k = 0; $k -lt $Lines.Length; $k++) { if ($Lines[$k].StartsWith("## The waves")) { $start = $k; break } }
    if ($start -lt 0) {
        $faults.Add("fix-round.md has no section headed ## The waves, so no FR row's area or wave was checked")
        return @{ Of = $of; Faults = $faults; Read = $false }
    }
    $noShape = ", in the waves section, is in no shape the waves reader knows, so nothing on it was read"

    $entries = New-Object System.Collections.Generic.List[object]
    $entry = $null
    for ($k = $start + 1; $k -lt $Lines.Length -and -not $Lines[$k].StartsWith("## "); $k++) {
        $line = $Lines[$k]
        if ($line.Trim().Length -eq 0 -or [regex]::IsMatch($line, '^[A-Za-z]')) { $entry = $null; continue }
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
            if ($e.Top) {
                foreach ($pair in [regex]::Matches($first, '\b(F\d+) [^,]*, (FR-\d+)\b')) {
                    $placed.Add($pair.Groups[2].Value)
                    $named.Add(@{ Id = $pair.Groups[2].Value; Area = $pair.Groups[1].Value; Wave = $at; Line = (Find-WavesLine $e $pair.Groups[2].Value) })
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
    return @{ Of = $of; Faults = $faults; Read = $true }
}

# The FR rows against fix-round.md: every item has a row with its id written exactly, every
# row whose id starts FR- in any case has its item, and the row's class, area and wave are what
# fix-round.md gives. Returns the faults and the count of items read.
function Test-TrackerFixRound($Rows, [string] $RoundPath) {
    $faults = New-Object System.Collections.Generic.List[string]
    $round = Read-TrackerText $RoundPath "fix-round.md"
    if ($null -ne $round.Fault) {
        $faults.Add($round.Fault)
        return @{ Faults = $faults; Count = 0 }
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
    if ($read.Items.Count -gt 0) {
        foreach ($row in $Rows) {
            $id = $row["id"].Trim()
            if ($id.StartsWith("FR-", [StringComparison]::OrdinalIgnoreCase) -and -not $itemIds.Contains($id)) {
                $faults.Add("tracker.csv line " + $row.Line + ", id " + $row["id"] + ": fix-round.md has no item headed with that id")
            }
        }
    }
    return @{ Faults = $faults; Count = $read.Items.Count }
}

# Every question of steps\02_questions.md by its number, a line '<n>. ' at the start, with its
# first Answer line and whether that line holds an answer.
function Get-Questions([string[]] $Lines) {
    $of = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $q = $null
    for ($k = 0; $k -lt $Lines.Length; $k++) {
        $m = [regex]::Match($Lines[$k], '^(\d+)\. ')
        if ($m.Success) {
            $q = @{ Line = $k + 1; AnswerLine = 0; Answered = $false }
            if (-not $of.ContainsKey($m.Groups[1].Value)) { $of[$m.Groups[1].Value] = $q }
            continue
        }
        $m = [regex]::Match($Lines[$k], '^\s+Answer:(.*)$')
        if ($null -ne $q -and $q.AnswerLine -eq 0 -and $m.Success) {
            $q.AnswerLine = $k + 1
            $q.Answered = $m.Groups[1].Value.Trim().Length -gt 0
        }
    }
    return $of
}

# The rows of class question against steps\02_questions.md: each names a question there as
# Q<number>, and a question whose Answer line holds Bader's answer reads merged with the number
# of the pull request that put the answer on main, or in review on the branch that records it.
function Test-TrackerQuestions($Rows, [string] $QuestionsPath) {
    $faults = New-Object System.Collections.Generic.List[string]
    $read = Read-TrackerText $QuestionsPath "02_questions.md"
    if ($null -ne $read.Fault) {
        $faults.Add($read.Fault)
        return @{ Faults = $faults; Count = 0 }
    }
    $questions = Get-Questions ($read.Text.Split("`n"))
    $count = 0
    foreach ($row in $Rows) {
        if ($row["class"] -cne "question") { continue }
        $where = "tracker.csv line " + $row.Line + ", id " + $row["id"]
        $m = [regex]::Match($row["id"], '^Q(\d+)$')
        if (-not $m.Success -or -not $questions.ContainsKey($m.Groups[1].Value)) {
            $faults.Add("${where}: its class is question and 02_questions.md has no question by that number")
            continue
        }
        $count++
        $q = $questions[$m.Groups[1].Value]
        if (-not $q.Answered) { continue }
        $status = $row["status"]
        if (($status -ceq "merged" -and $row["PR"] -match '^\d+$') -or $status -ceq "in review") { continue }
        $faults.Add("${where}: 02_questions.md line " + $q.AnswerLine + " holds Bader's answer, so the row reads merged with the number of the pull request that put it on main, or in review on the branch that records it, and not '$status' with PR '" + $row["PR"] + "'")
    }
    return @{ Faults = $faults; Count = $count }
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
