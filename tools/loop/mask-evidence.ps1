<#
    Writes a masked copy of one result file, so a result of a run or a probe can be
    committed without this machine's name or Bader's Autodesk licensing ids in it. It never
    changes -In.

    WHY. F102, measured on 2026-09-29. A probe result committed to a pushed branch carried
    the machine's name on its first line and the command lines of two AdskLicensingAgent
    processes, which carry the licensing agent's ids. tools\checks\check-evidence-ids.sh
    refuses such a file in the pre-commit and in Actions, and this is what makes the copy
    that passes it.

    WHAT IT MASKS is every kind in tools\checks\evidence-ids.txt, the one place that rule
    lives, which the check reads too. On each line, kind by kind in the file's order, a
    line the kind reads has every id of that kind replaced by the kind's mask, [id] or
    [machine]. A kind marked whole is masked only as a whole word. A GUID on a line no kind
    reads is left, a COM CLSID or a WPF window class name, because it names no licence and
    no machine.

    THE READ BACK IS THE CHECK'S OWN RULE. Once the copy is in place it is read again off
    the disk with the same kinds the check refuses, the whole word ones anywhere, even
    inside a longer word, and for a NUL byte. What is left there is refused: the copy is
    deleted, and the kind and the line number are printed, never the text. So a copy this
    writes is a copy the check passes, on the same machine.

    Bytes are kept. A file is read byte for byte as Latin-1, so every byte comes back out
    as it went in and only a masked span changes, line endings and any UTF-8 included. A
    UTF-16 file, known by its byte order mark, is written back as UTF-8, because the check
    cannot read UTF-16. A file holding a NUL byte with no UTF-16 mark is refused.

    Usage, from the repo root:
        powershell -ExecutionPolicy Bypass -File tools\loop\mask-evidence.ps1 -In <file> -Out <file> [-Replace]
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $In,
    [Parameter(Mandatory = $true)] [string] $Out,
    [switch] $Replace
)

$ErrorActionPreference = "Stop"

function Get-FullPath([string] $p) {
    return [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($p)) { $p } else { Join-Path (Get-Location).Path $p }))
}

function Test-Under([string] $child, [string] $parent) {
    $c = $child.TrimEnd('\'); $p = $parent.TrimEnd('\')
    return $c.Equals($p, [StringComparison]::OrdinalIgnoreCase) -or $c.StartsWith($p + '\', [StringComparison]::OrdinalIgnoreCase)
}

$machine = [string]$env:COMPUTERNAME

# Every path this prints goes through here first, so a path through a share on this machine
# never prints its name.
function Hide([string] $s) {
    if ($machine -eq "") { return $s }
    return [regex]::Replace($s, "(?i)" + [regex]::Escape($machine), "[machine]")
}

function Refuse([string] $why) {
    Write-Host ("mask-evidence: REFUSED. " + $why)
    exit 1
}

$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$rulesPath = Join-Path $repo "tools\checks\evidence-ids.txt"
$inFull = Get-FullPath $In
$outFull = Get-FullPath $Out

if ($machine -eq "") { Refuse "COMPUTERNAME is not set, so the machine name cannot be masked. Nothing was written." }
if ($inFull -ieq $outFull) { Refuse "-In and -Out are the same file, and -In is never changed. Nothing was written." }
if (-not (Test-Path -LiteralPath $inFull -PathType Leaf)) { Refuse ("there is no file at -In " + (Hide $inFull) + ". Nothing was written.") }
foreach ($kept in @("samples", "steps\logs", "bundle")) {
    if (Test-Under $outFull (Join-Path $repo $kept)) { Refuse ("-Out is under " + $kept + ", which is never written. Nothing was written.") }
}
if ((Test-Path -LiteralPath $outFull) -and -not $Replace) { Refuse ("-Out " + (Hide $outFull) + " is already there. Give -Replace to write over it. Nothing was written.") }
if (Test-Path -LiteralPath $outFull -PathType Container) { Refuse ("-Out " + (Hide $outFull) + " is a folder. Nothing was written.") }
if (-not (Test-Path -LiteralPath $rulesPath -PathType Leaf)) { Refuse ("there is no rules file at " + $rulesPath + ". Nothing was written.") }

# The rules file, read the way check-evidence-ids.sh reads it: a line is a key, a colon and
# a value, and a line that is blank or starts with # is skipped.
$guid = ""
$shape = ""
$kinds = New-Object System.Collections.Generic.List[object]
$current = $null
foreach ($raw in [IO.File]::ReadAllLines($rulesPath)) {
    $line = $raw.TrimEnd("`r")
    if ($line -match '^[ \t]*(#|$)') { continue }
    $at = $line.IndexOf(':')
    $key = $(if ($at -ge 0) { $line.Substring(0, $at) } else { $line })
    $value = $(if ($at -ge 0) { $line.Substring($at + 1).TrimStart(' ', "`t") } else { "" })
    if ($key -eq "guid") { $guid = $value; continue }
    if ($key -eq "machine") { $shape = $value; continue }
    if ($key -eq "kind") {
        $current = [pscustomobject]@{ Kind = $value; Lines = ""; Id = ""; Mask = ""; Whole = "no"; Count = 0; LinesRx = $null; IdRx = $null; MaskRx = $null }
        $kinds.Add($current)
        continue
    }
    if ($null -ne $current -and @("lines", "id", "mask", "whole") -contains $key) { $current.($key) = $value; continue }
    Refuse ("the rules file holds a line that is no key it knows, " + $key + ". Nothing was written.")
}
if ($guid -eq "" -or $shape -eq "" -or $kinds.Count -eq 0) { Refuse "the rules file has no guid line, no machine line or no kind. Nothing was written." }
if (-not [regex]::IsMatch($machine, '^(?:' + $shape + ')$')) { Refuse "COMPUTERNAME is not the shape the machine line of the rules file allows. Nothing was written." }

# An expression of the file, as .NET reads it. [:space:] is what it is in the C locale
# grep reads in, so both read the same bytes as space.
function Convert-Expression([string] $e) {
    return $e.Replace('{guid}', $guid).Replace('{machine}', $machine).Replace('[:space:]', ' \t\n\v\f\r')
}

$options = [Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [Text.RegularExpressions.RegexOptions]::CultureInvariant
foreach ($k in $kinds) {
    if ($k.Lines -eq "" -or $k.Id -eq "" -or $k.Mask -eq "" -or @("yes", "no") -notcontains $k.Whole) { Refuse ("the kind " + $k.Kind + " in the rules file lacks lines, id, mask or a whole of yes or no. Nothing was written.") }
    if ($k.Lines -ne "every") { $k.LinesRx = New-Object Text.RegularExpressions.Regex((Convert-Expression $k.Lines), $options) }
    $k.IdRx = New-Object Text.RegularExpressions.Regex((Convert-Expression $k.Id), $options)
    $k.MaskRx = $(if ($k.Whole -eq "yes") { New-Object Text.RegularExpressions.Regex(('(?<![A-Za-z0-9_])(?:' + (Convert-Expression $k.Id) + ')(?![A-Za-z0-9_])'), $options) } else { $k.IdRx })
}

$latin = [Text.Encoding]::GetEncoding(28591)

# One line per piece, each keeping its own line ending, so the pieces join back into the
# same bytes.
function Split-Lines([string] $text) {
    return @([regex]::Split($text, '(?<=\n)') | Where-Object { $_ -ne "" })
}

# -In is held open for reading and closed to every writer and every delete until the copy
# is in place, so no spelling of -Out that reaches the same file can change it.
$source = New-Object IO.FileStream($inFull, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
$partial = $null
try {
    $bytes = New-Object byte[] $source.Length
    $read = 0
    while ($read -lt $bytes.Length) {
        $n = $source.Read($bytes, $read, $bytes.Length - $read)
        if ($n -eq 0) { throw "-In ended before its length was read." }
        $read += $n
    }

    $preamble = New-Object byte[] 0
    $note = $null
    if ($bytes.Length -ge 2 -and (($bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) -or ($bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF))) {
        $utf16 = $(if ($bytes[0] -eq 0xFF) { New-Object Text.UnicodeEncoding($false, $false) } else { New-Object Text.UnicodeEncoding($true, $false) })
        $text = $utf16.GetString($bytes, 2, $bytes.Length - 2)
        $outEncoding = New-Object Text.UTF8Encoding($false)
        $note = "-In is UTF-16 and the copy is written as UTF-8, which the check can read"
    } else {
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { $preamble = [byte[]](0xEF, 0xBB, 0xBF) }
        $text = $latin.GetString($bytes, $preamble.Length, $bytes.Length - $preamble.Length)
        if ($text.IndexOf([char]0) -ge 0) { Refuse ("-In " + (Hide $inFull) + " holds a NUL byte and no UTF-16 byte order mark, so it is not text this can read. Nothing was written.") }
        $outEncoding = $latin
    }

    $sb = New-Object Text.StringBuilder
    foreach ($line in (Split-Lines $text)) {
        foreach ($k in $kinds) {
            if ($null -ne $k.LinesRx -and -not $k.LinesRx.IsMatch($line)) { continue }
            $found = $k.MaskRx.Matches($line).Count
            if ($found -gt 0) {
                $k.Count += $found
                $line = $k.MaskRx.Replace($line, $k.Mask.Replace('$', '$$'))
            }
        }
        [void]$sb.Append($line)
    }

    $body = $outEncoding.GetBytes($sb.ToString())
    $written = New-Object byte[] ($preamble.Length + $body.Length)
    [Array]::Copy($preamble, 0, $written, 0, $preamble.Length)
    [Array]::Copy($body, 0, $written, $preamble.Length, $body.Length)

    # Written beside and then moved, so a copy cut off half way never sits under -Out.
    $dir = Split-Path -Parent $outFull
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
    $partial = $outFull + ".masking-" + $PID + ".tmp"
    [IO.File]::WriteAllBytes($partial, $written)
    # [NullString]::Value and not $null, because PowerShell hands $null to a string
    # parameter as an empty string, and Replace refuses an empty backup path. Measured on
    # 2026-09-29 with -Replace: "The path is not of a legal form."
    if (Test-Path -LiteralPath $outFull) { [IO.File]::Replace($partial, $outFull, [NullString]::Value) } else { [IO.File]::Move($partial, $outFull) }
    $partial = $null
}
finally {
    $source.Dispose()
    if ($null -ne $partial -and (Test-Path -LiteralPath $partial)) { Remove-Item -LiteralPath $partial }
}

foreach ($k in $kinds) { Write-Host ("mask-evidence: {0}, {1} masked" -f $k.Kind, $k.Count) }
if ($null -ne $note) { Write-Host ("mask-evidence: " + $note) }

# Read back off the disk with the check's rule, as the header says.
$back = $latin.GetString([IO.File]::ReadAllBytes($outFull))
$left = New-Object System.Collections.Generic.List[string]
if ($back.IndexOf([char]0) -ge 0) { $left.Add("a NUL byte is in the copy") }
$number = 0
foreach ($line in (Split-Lines $back)) {
    $number++
    foreach ($k in $kinds) {
        if ($null -ne $k.LinesRx -and -not $k.LinesRx.IsMatch($line)) { continue }
        if ($k.IdRx.IsMatch($line)) { $left.Add(("{0} is still on line {1}" -f $k.Kind, $number)) }
    }
}

if ($left.Count -gt 0) {
    Remove-Item -LiteralPath $outFull
    foreach ($l in $left) { Write-Host ("mask-evidence: " + $l) }
    Refuse ("the copy still carried " + $left.Count + " of them when it was read back, so it was deleted. Mask those lines by hand or widen the rules file, and run it again.")
}

Write-Host ("mask-evidence: wrote {0}, {1} lines, {2} bytes, read back with nothing left" -f (Hide $outFull), $number, $back.Length)
exit 0
