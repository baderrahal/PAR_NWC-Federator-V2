<#
    Writes a masked copy of one result file, so a result of a run or a probe can be
    committed without this machine's name or Bader's Autodesk licensing ids in it. It never
    changes -In.

    WHY. F102, measured on 2026-09-29. A probe result committed to a pushed branch carried
    the machine's name on its first line and the command lines of two AdskLicensingAgent
    processes, which carry the licensing agent's ids. tools\checks\check-evidence-ids.sh
    refuses such a file in the pre-commit and in Actions, and this is what makes the copy
    that passes it.

    WHAT IT MASKS, on each line, in this order:
      an analytics agent id      the value after analyticsagentid=, up to the next space,
                                 ampersand or quote, to [id]
      an id after -i             the word after -i when it holds a GUID, to [id]
      a GUID on a licensing line any GUID on a line naming AdskLicensing, AdskIdentity or
                                 GenuineService, to [id]
      the machine name           the value of COMPUTERNAME as a whole word in any case, to
                                 [machine]
    A GUID on any other line is left, a COM CLSID or a WPF window class name, because it
    names no licence and no machine. The ids go first, so a machine name that reads as hex
    can never break a GUID in two before it is masked.

    THE READ BACK IS WIDER THAN THE MASK. Once the copy is in place it is read again for the
    machine name anywhere in any case, even inside a longer word, for analytics- followed by
    a GUID anywhere, and for the other two as masked. What the mask did not recognise is
    refused there: the copy is deleted, and the kind and the line number are printed,
    never the text.

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

$guid = '[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}'
$value = '[^\s&"'']'
$names = '(?i)AdskLicensing|AdskIdentity|GenuineService'
$latin = [Text.Encoding]::GetEncoding(28591)

$masks = @(
    [pscustomobject]@{ Kind = "an analytics agent id"; Pattern = '(?i)(?<=analyticsagentid=)(?!\[id\](?!' + $value + '))' + $value + '+'; With = "[id]"; OnLicensing = $false; Count = 0 },
    [pscustomobject]@{ Kind = "an id after -i"; Pattern = '(?<=(?:^|\s)-i[ \t]+["'']?)[^\s"'']*' + $guid + '[^\s"'']*'; With = "[id]"; OnLicensing = $false; Count = 0 },
    [pscustomobject]@{ Kind = "a GUID on a licensing line"; Pattern = $guid; With = "[id]"; OnLicensing = $true; Count = 0 },
    [pscustomobject]@{ Kind = "the machine name"; Pattern = '(?i)(?<![A-Za-z0-9_])' + [regex]::Escape($machine) + '(?![A-Za-z0-9_])'; With = "[machine]"; OnLicensing = $false; Count = 0 }
)

$wider = @(
    [pscustomobject]@{ Kind = "the machine name"; Pattern = '(?i)' + [regex]::Escape($machine); OnLicensing = $false },
    [pscustomobject]@{ Kind = "an analytics agent id"; Pattern = '(?i)(?<=analyticsagentid=)(?!\[id\](?!' + $value + '))' + $value + '|(?i)analytics-' + $guid; OnLicensing = $false },
    [pscustomobject]@{ Kind = "an id after -i"; Pattern = $masks[1].Pattern; OnLicensing = $false },
    [pscustomobject]@{ Kind = "a GUID on a licensing line"; Pattern = $guid; OnLicensing = $true }
)

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

    $lines = Split-Lines $text
    $sb = New-Object Text.StringBuilder
    foreach ($line in $lines) {
        $licensing = [regex]::IsMatch($line, $names)
        foreach ($m in $masks) {
            if ($m.OnLicensing -and -not $licensing) { continue }
            $found = [regex]::Matches($line, $m.Pattern).Count
            if ($found -gt 0) {
                $m.Count += $found
                $line = [regex]::Replace($line, $m.Pattern, $m.With)
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

foreach ($m in $masks) { Write-Host ("mask-evidence: {0}, {1} masked" -f $m.Kind, $m.Count) }
if ($null -ne $note) { Write-Host ("mask-evidence: " + $note) }

# Read back off the disk, wider than the mask, as the header says.
$back = [IO.File]::ReadAllBytes($outFull)
$left = New-Object System.Collections.Generic.List[string]
$number = 0
foreach ($line in (Split-Lines ($latin.GetString($back)))) {
    $number++
    $licensing = [regex]::IsMatch($line, $names)
    foreach ($w in $wider) {
        if ($w.OnLicensing -and -not $licensing) { continue }
        if ([regex]::IsMatch($line, $w.Pattern)) { $left.Add(("{0} is still on line {1}" -f $w.Kind, $number)) }
    }
}

if ($left.Count -gt 0) {
    Remove-Item -LiteralPath $outFull
    foreach ($l in $left) { Write-Host ("mask-evidence: " + $l) }
    Refuse ("the copy still carried " + $left.Count + " of them when it was read back, so it was deleted. Mask those lines by hand or widen the mask, and run it again.")
}

Write-Host ("mask-evidence: wrote {0}, {1} lines, {2} bytes, read back with nothing left" -f (Hide $outFull), $number, $back.Length)
exit 0
