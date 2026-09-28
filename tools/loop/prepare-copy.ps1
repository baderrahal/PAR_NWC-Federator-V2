<#
    The one script that reads NM Fed. Every loop run works on a copy of it under
    %LOCALAPPDATA%\NwcFederatorLoop\source, and nothing here ever writes into NM Fed.

    Usage, from the repo root, one switch at a time:
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Listing steps\runs\00\source-listing.txt
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Remove <file name of one NWC>
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Restore <file name of one NWC>

    With no switch it makes the copy, or keeps the copy already there when it is WHOLE and
    holds exactly what NM Fed holds: every file under the same relative path, matched with
    case, with the same size and the same sha256, and every folder, the empty ones too. A
    copy is whole only once source.manifest.txt is written beside it, which happens after
    every copied file has been hashed and matched, so a copy cut off half way is never kept.

    -Listing writes what NM Fed holds, one line per file with its size, its sha256 and parts
    3 and 5 of its name, into a .txt inside the repo, and does nothing else.
    -Remove takes one NWC out of a whole copy for the run that loses a file, and writes down
    its sha256. -Restore copies that same file back from NM Fed for the run that gets it
    back, and refuses when NM Fed now holds a different file under that name.

    Written again on 2026-09-27 after the Phase 0 breaker read the first version. The keep
    test compared sizes alone, -Listing carried on into the keep test and remade the copy,
    -Listing could write anywhere, and the old copy was deleted before the room was checked.
#>

[CmdletBinding()]
param(
    [string] $Listing,
    [string] $Remove,
    [string] $Restore
)

$ErrorActionPreference = "Stop"

$repo     = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$source   = Join-Path ([Environment]::GetFolderPath('Desktop')) "NM Fed"
$work     = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$copy     = Join-Path $work "source"
$manifest = Join-Path $work "source.manifest.txt"
$removed  = Join-Path $work "source.removed.txt"
$ordinal  = [StringComparer]::Ordinal

# One switch at a time, and a switch given with no value is refused rather than read as
# no switch, because an empty -Remove would otherwise run the plain command and the run
# that should lose a file would lose nothing.
$given = @("Listing", "Remove", "Restore" | Where-Object { $PSBoundParameters.ContainsKey($_) })
if ($given.Count -gt 1) { throw ("One of -Listing, -Remove and -Restore at a time, and {0} were given. Nothing was done." -f ($given -join " and ")) }
foreach ($g in $given) {
    if ([string]::IsNullOrWhiteSpace([string]$PSBoundParameters[$g])) { throw "-$g was given no value. Nothing was done." }
}

function Test-Under([string] $child, [string] $parent) {
    $c = $child.TrimEnd('\'); $p = $parent.TrimEnd('\')
    return $c.Equals($p, [StringComparison]::OrdinalIgnoreCase) -or $c.StartsWith($p + '\', [StringComparison]::OrdinalIgnoreCase)
}

if (-not (Test-Path -LiteralPath $source -PathType Container)) { throw "NM Fed was not found at '$source'. Nothing was done." }
$sourceFull = (Get-Item -LiteralPath $source -Force).FullName.TrimEnd('\')
$workFull = [IO.Path]::GetFullPath($work).TrimEnd('\')
if ((Test-Under $workFull $sourceFull) -or (Test-Under $sourceFull $workFull)) {
    throw "The work folder '$workFull' and NM Fed '$sourceFull' overlap, so a copy or a delete could land in NM Fed. Nothing was done."
}
foreach ($p in @($workFull, $copy)) {
    if (Test-Path -LiteralPath $p) {
        if ((Get-Item -LiteralPath $p -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "'$p' is a junction or a link, and a delete through it could reach somewhere else. Nothing was done."
        }
    }
}

# A file OneDrive holds online only is downloaded into NM Fed the moment it is read, which
# changes NM Fed. Measured on 2026-09-27: every file there carries Archive and ReparsePoint
# and none carries these, so this refuses only a folder that has changed since.
$online = 0x400000 -bor 0x40000 -bor 0x1000

function Read-Folder([string] $root, [bool] $hash) {
    # Walked one folder at a time rather than with -Recurse, because Windows PowerShell 5.1
    # follows a junction when it recurses, and a junction inside NM Fed would pull a folder
    # from somewhere else into the copy. A junction or a link is refused by name. A folder
    # OneDrive syncs carries the reparse point attribute too, and has no LinkType, so it is
    # not mistaken for one.
    $files = New-Object 'System.Collections.Generic.Dictionary[string,object]' ($ordinal)
    $dirs = New-Object 'System.Collections.Generic.HashSet[string]' ($ordinal)
    $todo = New-Object System.Collections.Generic.Stack[string]
    $todo.Push($root)
    while ($todo.Count -gt 0) {
        $here = $todo.Pop()
        foreach ($item in Get-ChildItem -LiteralPath $here -Force) {
            $rel = $item.FullName.Substring($root.Length + 1)
            $link = [string]$item.LinkType
            if ($link -eq "Junction" -or $link -eq "SymbolicLink") { throw "'$rel' is a $link, which would pull something from elsewhere into the copy. Nothing was done." }
            if ($item.PSIsContainer) {
                [void]$dirs.Add($rel)
                $todo.Push($item.FullName)
                continue
            }
            if (([int]$item.Attributes) -band $online) { throw "'$rel' is held online only by OneDrive, so reading it would download it into NM Fed. Make NM Fed available offline first. Nothing was done." }
            $sha = if ($hash) { (Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash } else { "" }
            $files[$rel] = [pscustomobject]@{ Bytes = $item.Length; Sha = $sha }
        }
    }
    return [pscustomobject]@{ Files = $files; Dirs = $dirs }
}

function Find-One([string] $root, [string] $name) {
    if ($name -match '[\\/*?\[\]]') { throw "'$name' is not a plain file name. Give the name of one NWC, exactly as it is spelled. Nothing was done." }
    if ([IO.Path]::GetExtension($name) -ne ".nwc") { throw "'$name' is not an NWC. -Remove and -Restore move one NWC and nothing else. Nothing was done." }
    $hits = @(Get-ChildItem -LiteralPath $root -Recurse -File -Force | Where-Object { $_.Name -ceq $name })
    if ($hits.Count -eq 0) { throw "No file named exactly '$name' under '$root'. Nothing was done." }
    if ($hits.Count -gt 1) { throw "'$name' is under '$root' $($hits.Count) times. Nothing was done." }
    return $hits[0]
}

Write-Host "NM Fed        : $sourceFull"
Write-Host "copy          : $copy"

if ($Listing) {
    $out = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Listing)) { $Listing } else { Join-Path (Get-Location).Path $Listing }))
    $runs = Join-Path $repo "steps\runs"
    if (-not $out.StartsWith($runs + '\', [StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($out) -ne ".txt") {
        throw "The listing is a .txt under steps\runs. '$out' is not. Nothing was written."
    }
    $src = Read-Folder $sourceFull $true
    $bytes = 0; foreach ($v in $src.Files.Values) { $bytes += $v.Bytes }
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("NM Fed at $sourceFull, read $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $lines.Add(("{0} files, {1:N0} bytes, {2} folders" -f $src.Files.Count, $bytes, $src.Dirs.Count))
    $lines.Add("")
    $lines.Add("relative path | bytes | sha256 | part 3 | part 5 | parts")
    foreach ($rel in ($src.Files.Keys | Sort-Object)) {
        $parts = [IO.Path]::GetFileNameWithoutExtension($rel).Split('-')
        $isNwc = [IO.Path]::GetExtension($rel) -eq ".nwc"
        $p3 = if (-not $isNwc) { "-" } elseif ($parts.Count -ge 3) { $parts[2] } else { "none" }
        $p5 = if (-not $isNwc) { "-" } elseif ($parts.Count -ge 5) { $parts[4] } else { "none" }
        $lines.Add(("{0} | {1} | {2} | {3} | {4} | {5}" -f $rel, $src.Files[$rel].Bytes, $src.Files[$rel].Sha, $p3, $p5, $parts.Count))
    }
    $lines.Add("")
    $lines.Add("folders:")
    foreach ($d in ($src.Dirs | Sort-Object)) { $lines.Add("  " + $d) }
    $dir = Split-Path -Parent $out
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [IO.File]::WriteAllLines($out, $lines)
    Write-Host "listing       : $out, $($lines.Count) lines. The copy was not touched."
    exit 0
}

if ($Remove) {
    if (-not (Test-Path -LiteralPath $manifest)) { throw "There is no whole copy to take a file out of. Run the plain command first. Nothing was done." }
    if (Test-Path -LiteralPath $removed) { throw "'$(Get-Content -LiteralPath $removed -TotalCount 1)' is already out of the copy. Restore it first. Nothing was done." }
    $f = Find-One $copy $Remove
    $rel = $f.FullName.Substring($copy.Length + 1)
    $sha = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
    [IO.File]::WriteAllLines($removed, @($rel, $sha, [string]$f.Length))
    Remove-Item -LiteralPath $f.FullName
    if (Test-Path -LiteralPath $f.FullName) { throw "'$($f.FullName)' is still there after the remove." }
    Write-Host "removed       : $rel from the copy, sha256 $sha written down. NM Fed is untouched."
    exit 0
}

if ($Restore) {
    if (-not (Test-Path -LiteralPath $removed)) { throw "Nothing was taken out of the copy, so there is nothing to put back. Nothing was done." }
    $note = @(Get-Content -LiteralPath $removed)
    $from = Find-One $sourceFull $Restore
    $rel = $from.FullName.Substring($sourceFull.Length + 1)
    if (-not $rel.Equals($note[0], [StringComparison]::Ordinal)) { throw "The file taken out was '$($note[0])', not '$rel'. Nothing was restored." }
    if (([int]$from.Attributes) -band $online) { throw "'$rel' is held online only by OneDrive, so reading it would download it into NM Fed. Nothing was restored." }
    $sha = (Get-FileHash -LiteralPath $from.FullName -Algorithm SHA256).Hash
    if ($sha -ne $note[1]) { throw "NM Fed now holds a different '$rel' from the one taken out, sha256 $sha against $($note[1]). A changed file is not the same file back. Nothing was restored." }
    $to = Join-Path $copy $rel
    if (Test-Path -LiteralPath $to) { throw "'$rel' is already in the copy. Nothing was restored." }
    $partial = $to + ".partial"
    Copy-Item -LiteralPath $from.FullName -Destination $partial -Force
    if ((Get-FileHash -LiteralPath $partial -Algorithm SHA256).Hash -ne $sha) { Remove-Item -LiteralPath $partial; throw "'$rel' did not copy back whole. Nothing was restored." }
    Move-Item -LiteralPath $partial -Destination $to
    Remove-Item -LiteralPath $removed
    Write-Host "restored      : $rel, $($from.Length) bytes, the same sha256 as the file taken out"
    exit 0
}

# The plain command. While a file is out of the copy for the run that loses one, it
# refuses rather than remaking the copy, because remaking it puts the file straight back
# and the run that loses a file would then lose nothing. -Restore ends that state.
if (Test-Path -LiteralPath $removed) {
    throw "'$(Get-Content -LiteralPath $removed -TotalCount 1)' is out of the copy for the run that loses a file. Restore it with -Restore first. The copy was left exactly as it is."
}
$src = Read-Folder $sourceFull $true
$nwc = @($src.Files.Keys | Where-Object { [IO.Path]::GetExtension($_) -eq ".nwc" }).Count
$bytes = 0; foreach ($v in $src.Files.Values) { $bytes += $v.Bytes }
Write-Host ("NM Fed holds  : {0} files, {1} of them NWC, {2:N1} MB, {3} folders" -f $src.Files.Count, $nwc, ($bytes / 1MB), $src.Dirs.Count)
if ($nwc -eq 0) { throw "NM Fed holds no NWC file, which is nothing to run on. The copy was left exactly as it was." }

$whole = (Test-Path -LiteralPath $copy) -and (Test-Path -LiteralPath $manifest)
if ($whole) {
    $cp = Read-Folder $copy $true
    $same = ($cp.Files.Count -eq $src.Files.Count) -and $cp.Dirs.SetEquals($src.Dirs)
    if ($same) {
        foreach ($rel in $src.Files.Keys) {
            if (-not $cp.Files.ContainsKey($rel) -or $cp.Files[$rel].Bytes -ne $src.Files[$rel].Bytes -or $cp.Files[$rel].Sha -ne $src.Files[$rel].Sha) { $same = $false; break }
        }
    }
    if ($same) {
        Write-Host "kept          : the copy matches NM Fed on every file by name, size and sha256, and on every folder"
        exit 0
    }
    Write-Host "the copy differs from NM Fed, so it is made again"
} elseif (Test-Path -LiteralPath $copy) {
    Write-Host "the copy is not marked whole, so it is made again"
}

# Room first, and nothing deleted until there is room.
New-Item -ItemType Directory -Force -Path $workFull | Out-Null
$drive = New-Object IO.DriveInfo ([IO.Path]::GetPathRoot($workFull))
$freed = 0
if (Test-Path -LiteralPath $copy) { foreach ($f in Get-ChildItem -LiteralPath $copy -Recurse -File -Force) { $freed += $f.Length } }
$needed = $bytes + 1GB
Write-Host ("room on {0}    : {1:N1} GB free, {2:N1} GB more once the old copy goes, {3:N1} GB needed" -f $drive.Name, ($drive.AvailableFreeSpace / 1GB), ($freed / 1GB), ($needed / 1GB))
if ($drive.AvailableFreeSpace + $freed -lt $needed) { throw "Not enough room on $($drive.Name) for the copy. The old copy was left as it was." }

if (Test-Path -LiteralPath $manifest) { Remove-Item -LiteralPath $manifest }
if (Test-Path -LiteralPath $copy) { Remove-Item -LiteralPath $copy -Recurse -Force }

foreach ($d in $src.Dirs) { New-Item -ItemType Directory -Force -Path (Join-Path $copy $d) | Out-Null }
foreach ($rel in $src.Files.Keys) {
    $to = Join-Path $copy $rel
    $dir = Split-Path -Parent $to
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    Copy-Item -LiteralPath (Join-Path $sourceFull $rel) -Destination $to
}

$made = Read-Folder $copy $true
$short = @($src.Files.Keys | Where-Object { -not $made.Files.ContainsKey($_) -or $made.Files[$_].Sha -ne $src.Files[$_].Sha })
if ($short.Count -gt 0 -or $made.Files.Count -ne $src.Files.Count -or -not $made.Dirs.SetEquals($src.Dirs)) {
    throw ("The copy does not match NM Fed after copying, {0} file(s) short or different, first: {1}. It is not marked whole." -f $short.Count, $(if ($short.Count) { $short[0] } else { "a folder" }))
}
$m = New-Object System.Collections.Generic.List[string]
foreach ($rel in ($src.Files.Keys | Sort-Object)) { $m.Add(("{0} {1} {2}" -f $src.Files[$rel].Sha, $src.Files[$rel].Bytes, $rel)) }
[IO.File]::WriteAllLines($manifest, $m)
Write-Host ("copied        : {0} files and {1} folders, {2:N1} MB, every file hashed and matching NM Fed, marked whole" -f $made.Files.Count, $made.Dirs.Count, ($bytes / 1MB))
