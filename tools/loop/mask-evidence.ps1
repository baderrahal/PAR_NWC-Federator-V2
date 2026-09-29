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

    Which kinds read a line is decided on the line as it came in, before any of it is
    masked, the way the check reads it.

    THE READ BACK IS THE CHECK'S OWN RULE FOR A LINE. Once the copy is in place it is read
    again off the disk with the same kinds the check refuses, the whole word ones anywhere,
    even inside a longer word, and for a NUL byte. What is left there is refused: the copy
    is deleted, and the kind and the line number are printed, never the text. So on the
    same machine the check passes every line of a copy this writes. The check also reads
    the copy's path under the folder it reads, which this does not, so a copy is named
    plainly and never after an id or the machine.

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
if (-not (Test-Path -LiteralPath $rulesPath -PathType Leaf)) { Refuse ("there is no rules file at " + (Hide $rulesPath) + ". Nothing was written.") }

$latin = [Text.Encoding]::GetEncoding(28591)

# The rules file, read EXACTLY as check-evidence-ids.sh reads it, so a rules file one of
# them refuses the other refuses too, and a green check means the mask can read it. Bytes,
# split at each line feed, one carriage return taken off the end. A line that is blank or
# starts with # is skipped. Any other line is a key, a colon and a value, and every key is
# compared with its case. A key it does not know, a key of a kind before the first kind, a
# kind that lacks lines, id, mask or whole, a whole that is not yes or no, and a file with
# no guid, no machine or no kind are refused.
function Close-Kind($k) {
    if ($null -eq $k) { return }
    if ($k.Lines -ceq "" -or $k.Id -ceq "" -or $k.Mask -ceq "" -or $k.Whole -ceq "") { Refuse ("the rules file cannot be read, the kind " + $k.Kind + " lacks a lines, id, mask or whole line. Nothing was written.") }
    if (@("yes", "no") -cnotcontains $k.Whole) { Refuse ("the rules file cannot be read, the kind " + $k.Kind + " has a whole that is not yes or no. Nothing was written.") }
}

$guid = ""
$shape = ""
$kinds = New-Object System.Collections.Generic.List[object]
$current = $null
foreach ($raw in $latin.GetString([IO.File]::ReadAllBytes($rulesPath)).Split([char]10)) {
    $line = $(if ($raw.EndsWith([string][char]13)) { $raw.Substring(0, $raw.Length - 1) } else { $raw })
    if ($line -cmatch '^[ \t]*(#|$)') { continue }
    $at = $line.IndexOf(':')
    if ($at -lt 0) { Refuse "the rules file cannot be read, a line has no key. Nothing was written." }
    $key = $line.Substring(0, $at)
    $value = $line.Substring($at + 1).TrimStart(' ', "`t")
    if ($key -ceq "guid") { $guid = $value }
    elseif ($key -ceq "machine") { $shape = $value }
    elseif ($key -ceq "kind") {
        Close-Kind $current
        if ($value -ceq "") { Refuse "the rules file cannot be read, a kind has no name. Nothing was written." }
        $current = [pscustomobject]@{ Kind = $value; Lines = ""; Id = ""; Mask = ""; Whole = ""; Count = 0; LinesRx = $null; IdRx = $null; MaskRx = $null }
        $kinds.Add($current)
    }
    elseif (@("lines", "id", "mask", "whole") -ccontains $key) {
        if ($null -eq $current) { Refuse ("the rules file cannot be read, " + $key + " comes before any kind. Nothing was written.") }
        $current.($key) = $value
    }
    else { Refuse ("the rules file cannot be read, a line that is no key it knows, " + $key + ". Nothing was written.") }
}
Close-Kind $current
if ($guid -ceq "" -or $shape -ceq "" -or $kinds.Count -eq 0) { Refuse "the rules file cannot be read, it has no guid line, no machine line or no kind. Nothing was written." }
if (-not [regex]::IsMatch($machine, '^(?:' + $shape + ')$')) { Refuse "COMPUTERNAME is not the shape the machine line of the rules file allows. Nothing was written." }

# An expression of the file, as .NET reads it. [:space:] becomes the six characters it is
# in the C locale grep reads in, space, tab, line feed, vertical tab, form feed and carriage
# return, so both read the same bytes as space.
function Convert-Expression([string] $e) {
    return $e.Replace('{guid}', $guid).Replace('{machine}', $machine).Replace('[:space:]', ' \t\n\v\f\r')
}

$options = [Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [Text.RegularExpressions.RegexOptions]::CultureInvariant
foreach ($k in $kinds) {
    if ($k.Lines -cne "every") { $k.LinesRx = New-Object Text.RegularExpressions.Regex((Convert-Expression $k.Lines), $options) }
    $k.IdRx = New-Object Text.RegularExpressions.Regex((Convert-Expression $k.Id), $options)
    $k.MaskRx = $(if ($k.Whole -ceq "yes") { New-Object Text.RegularExpressions.Regex(('(?<![A-Za-z0-9_])(?:' + (Convert-Expression $k.Id) + ')(?![A-Za-z0-9_])'), $options) } else { $k.IdRx })
}

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

    # Which kinds read a line is decided on the line AS IT CAME, the way the check reads it,
    # and only then is it masked. Decided on a line an earlier kind had already masked, the
    # line other.exe -i analytics-GUID --session GUID lost its GUID after -i to the first
    # kind, so the kind for an id after -i no longer read it and its second GUID was left,
    # in a copy that passed the check. Measured on 2026-09-29, proof S1 to S4.
    $sb = New-Object Text.StringBuilder
    foreach ($original in (Split-Lines $text)) {
        $reads = @($kinds | Where-Object { $null -eq $_.LinesRx -or $_.LinesRx.IsMatch($original) })
        $line = $original
        foreach ($k in $reads) {
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
