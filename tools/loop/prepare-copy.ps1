<#
    The one script that reads NM Fed. Every loop run works on a copy of it under
    %LOCALAPPDATA%\NwcFederatorLoop, and nothing here ever writes into NM Fed.

    Usage, from the repo root, one of -Listing, -Remove and -Restore at a time, and -Set
    alone or with -Remove or -Restore:
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Listing steps\runs\00\source-listing.txt
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Remove <file name of one NWC>
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Restore <file name of one NWC>
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Set NN
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Set NN -Remove <file name of one NWC>
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Set NN -Restore <file name of one NWC>

    With no switch it makes the copy under source, or keeps the copy already there when it
    is WHOLE and holds exactly what NM Fed holds: every file under the same relative path,
    matched with case, with the same size and the same sha256, and every folder, the empty
    ones too. A copy is whole only once source.manifest.txt is written beside it, which
    happens after every copied file has been hashed and matched, so a copy cut off half way
    is never kept.

    -Listing writes what NM Fed holds, one line per file with its size, its sha256 and parts
    3 and 5 of its name, into a .txt inside the repo, and does nothing else.
    -Remove takes one NWC out of a whole copy for the run that loses a file, and writes down
    its sha256. -Restore copies that same file back from NM Fed for the run that gets it
    back, and refuses when NM Fed now holds a different file under that name.

    -Set NN, two digits, makes the fresh copy one run set works on, runs\NN\NMFed. First the
    plain command's check, so the source copy is kept or made again exactly as with no
    switch. Then every file and folder of the source copy is copied into runs\NN\NMFed,
    every file is read back by sha256 against source.manifest.txt, and one empty folder is
    made under Clash Report for each folder under NWC, read off NWC. NMFed.manifest.txt is
    written beside it last, so a copy cut off half way is never read as whole. A run set's
    copy is made once and never emptied, so -Set NN refuses when runs\NN\NMFed or a note
    beside it is there already. With -Remove or -Restore, -Set NN works on runs\NN\NMFed as
    they work on the source copy, and the note is NMFed.removed.txt beside it. The name is
    NMFed and never NM Fed, because the wall in .claude\hooks refuses every command naming
    NM Fed, and a run names its copy in its commands.

    Written again on 2026-09-27 after the Phase 0 breaker read the first version. The keep
    test compared sizes alone, -Listing carried on into the keep test and remade the copy,
    -Listing could write anywhere, and the old copy was deleted before the room was checked.
    -Set added on 2026-10-01 for the run sets of turn 4, F108.
#>

[CmdletBinding()]
param(
    [string] $Listing,
    [string] $Remove,
    [string] $Restore,
    [string] $Set
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
# that should lose a file would lose nothing. -Set goes alone or with -Remove or -Restore.
$given = @("Listing", "Remove", "Restore" | Where-Object { $PSBoundParameters.ContainsKey($_) })
if ($given.Count -gt 1) { throw ("One of -Listing, -Remove and -Restore at a time, and {0} were given. Nothing was done." -f ($given -join " and ")) }
foreach ($g in @($given) + @("Set" | Where-Object { $PSBoundParameters.ContainsKey($_) })) {
    if ([string]::IsNullOrWhiteSpace([string]$PSBoundParameters[$g])) { throw "-$g was given no value. Nothing was done." }
}
if ($Set) {
    if ($Listing) { throw "-Listing reads NM Fed alone and takes no -Set. Nothing was done." }
    if ($Set -notmatch '\A[0-9]{2}\z') { throw "-Set takes the run set's number in two digits, such as 03, and '$Set' is not. Nothing was done." }
}

# The copy -Remove and -Restore work on, with its manifest and its note. With -Set it is the
# run set's own copy, and the window run reads these three names.
if ($Set) {
    $setDir         = Join-Path $work ("runs\" + $Set)
    $target         = Join-Path $setDir "NMFed"
    $targetManifest = Join-Path $setDir "NMFed.manifest.txt"
    $targetRemoved  = Join-Path $setDir "NMFed.removed.txt"
    $which          = "the copy of set $Set"
    $make           = "-Set $Set alone"
} else {
    $target         = $copy
    $targetManifest = $manifest
    $targetRemoved  = $removed
    $which          = "the copy"
    $make           = "the plain command"
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
# Every folder from the work folder down to the copy written into, so a write or a delete
# can never pass through a junction to somewhere else.
$guarded = @($workFull, $copy)
if ($Set) { $guarded += @((Join-Path $work "runs"), $setDir, $target) }
foreach ($p in $guarded) {
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

function Copy-Tree([string] $from, [string] $into, $dirs, $files) {
    # Every folder first, the empty ones too, then every file under the same relative path.
    New-Item -ItemType Directory -Force -Path $into | Out-Null
    foreach ($d in $dirs) { New-Item -ItemType Directory -Force -Path (Join-Path $into $d) | Out-Null }
    foreach ($rel in $files) {
        $to = Join-Path $into $rel
        $dir = Split-Path -Parent $to
        if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
        Copy-Item -LiteralPath (Join-Path $from $rel) -Destination $to
    }
}

function Write-Manifest([string] $path, $files) {
    # One line per file, its sha256, its size and its relative path, sorted by the path.
    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($rel in ($files.Keys | Sort-Object)) { $lines.Add(("{0} {1} {2}" -f $files[$rel].Sha, $files[$rel].Bytes, $rel)) }
    [IO.File]::WriteAllLines($path, $lines)
}

function Read-Manifest([string] $path) {
    $files = New-Object 'System.Collections.Generic.Dictionary[string,object]' ($ordinal)
    foreach ($line in [IO.File]::ReadAllLines($path)) {
        $p = $line.Split([char]' ', 3)
        if ($p.Count -ne 3 -or $p[0] -notmatch '\A[0-9A-F]{64}\z' -or $p[1] -notmatch '\A[0-9]+\z') { throw "'$path' holds a line that is not a sha256, a size and a path: '$line'. Nothing more was done." }
        $files[$p[2]] = [pscustomobject]@{ Bytes = [long]$p[1]; Sha = $p[0] }
    }
    return $files
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
if ($Set) { Write-Host ("copy of set {0}: {1}" -f $Set, $target) }

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
    if (-not (Test-Path -LiteralPath $targetManifest)) { throw "There is no whole copy to take a file out of. Run $make first. Nothing was done." }
    if (Test-Path -LiteralPath $targetRemoved) { throw "'$(Get-Content -LiteralPath $targetRemoved -TotalCount 1)' is already out of $which. Restore it first. Nothing was done." }
    $f = Find-One $target $Remove
    $rel = $f.FullName.Substring($target.Length + 1)
    $sha = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
    [IO.File]::WriteAllLines($targetRemoved, @($rel, $sha, [string]$f.Length))
    Remove-Item -LiteralPath $f.FullName
    if (Test-Path -LiteralPath $f.FullName) { throw "'$($f.FullName)' is still there after the remove." }
    Write-Host "removed       : $rel from $which, sha256 $sha written down. NM Fed is untouched."
    exit 0
}

if ($Restore) {
    if (-not (Test-Path -LiteralPath $targetRemoved)) { throw "Nothing was taken out of $which, so there is nothing to put back. Nothing was done." }
    $note = @(Get-Content -LiteralPath $targetRemoved)
    $from = Find-One $sourceFull $Restore
    $rel = $from.FullName.Substring($sourceFull.Length + 1)
    if (-not $rel.Equals($note[0], [StringComparison]::Ordinal)) { throw "The file taken out was '$($note[0])', not '$rel'. Nothing was restored." }
    if (([int]$from.Attributes) -band $online) { throw "'$rel' is held online only by OneDrive, so reading it would download it into NM Fed. Nothing was restored." }
    $sha = (Get-FileHash -LiteralPath $from.FullName -Algorithm SHA256).Hash
    if ($sha -ne $note[1]) { throw "NM Fed now holds a different '$rel' from the one taken out, sha256 $sha against $($note[1]). A changed file is not the same file back. Nothing was restored." }
    $to = Join-Path $target $rel
    if (Test-Path -LiteralPath $to) { throw "'$rel' is already in $which. Nothing was restored." }
    $partial = $to + ".partial"
    Copy-Item -LiteralPath $from.FullName -Destination $partial -Force
    if ((Get-FileHash -LiteralPath $partial -Algorithm SHA256).Hash -ne $sha) { Remove-Item -LiteralPath $partial; throw "'$rel' did not copy back whole. Nothing was restored." }
    Move-Item -LiteralPath $partial -Destination $to
    Remove-Item -LiteralPath $targetRemoved
    Write-Host "restored      : $rel, $($from.Length) bytes, the same sha256 as the file taken out"
    exit 0
}

# The plain command, and the first half of -Set. -Set refuses first when its copy or a note
# beside it is there already, so that refusal writes nothing, not even the source copy.
if ($Set) {
    $there = @($target, $targetManifest, $targetRemoved | Where-Object { Test-Path -LiteralPath $_ })
    if ($there.Count -gt 0) {
        throw ("Set {0} has these already: '{1}'. A run set's copy is made fresh once and nothing is ever emptied, so give a set number that has none. Nothing was written." -f $Set, ($there -join "', '"))
    }
}

# While a file is out of the copy for the run that loses one, it refuses rather than
# remaking the copy, because remaking it puts the file straight back and the run that
# loses a file would then lose nothing. -Restore ends that state. -Set refuses too, because
# the source copy it would copy from is short of that file.
if (Test-Path -LiteralPath $removed) {
    $none = if ($Set) { ", and no copy of set $Set was made" } else { "" }
    throw "'$(Get-Content -LiteralPath $removed -TotalCount 1)' is out of the copy for the run that loses a file. Restore it with -Restore first. The copy was left exactly as it is$none."
}
$src = Read-Folder $sourceFull $true
$nwc = @($src.Files.Keys | Where-Object { [IO.Path]::GetExtension($_) -eq ".nwc" }).Count
$bytes = 0; foreach ($v in $src.Files.Values) { $bytes += $v.Bytes }
Write-Host ("NM Fed holds  : {0} files, {1} of them NWC, {2:N1} MB, {3} folders" -f $src.Files.Count, $nwc, ($bytes / 1MB), $src.Dirs.Count)
if ($nwc -eq 0) { throw "NM Fed holds no NWC file, which is nothing to run on. The copy was left exactly as it was." }

# Bader's shape for a run set. Clash Report gets one folder for each folder under NWC, read
# off NWC and never named here, because a group can sit in two of them and the report of one
# would be written over by the other. Read before anything is written.
if ($Set) {
    foreach ($d in @("NWC", "Clash Report")) {
        if (-not $src.Dirs.Contains($d)) { throw "NM Fed holds no folder named exactly '$d', so the copy of set $Set cannot be in Bader's shape. Nothing was written." }
    }
    $reports = @($src.Dirs | Where-Object { $_.StartsWith("NWC\", [StringComparison]::Ordinal) -and $_.IndexOf([char]'\', 4) -lt 0 } | ForEach-Object { "Clash Report\" + $_.Substring(4) } | Sort-Object)
    if ($reports.Count -eq 0) { throw "NWC in NM Fed holds no folder, so there is no folder to make under Clash Report for set $Set. Nothing was written." }
}

$whole = (Test-Path -LiteralPath $copy) -and (Test-Path -LiteralPath $manifest)
$kept = $false
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
        $kept = $true
    } else {
        Write-Host "the copy differs from NM Fed, so it is made again"
    }
} elseif (Test-Path -LiteralPath $copy) {
    Write-Host "the copy is not marked whole, so it is made again"
}

if (-not $kept) {
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

    Copy-Tree $sourceFull $copy $src.Dirs $src.Files.Keys
    $made = Read-Folder $copy $true
    $short = @($src.Files.Keys | Where-Object { -not $made.Files.ContainsKey($_) -or $made.Files[$_].Sha -ne $src.Files[$_].Sha })
    if ($short.Count -gt 0 -or $made.Files.Count -ne $src.Files.Count -or -not $made.Dirs.SetEquals($src.Dirs)) {
        throw ("The copy does not match NM Fed after copying, {0} file(s) short or different, first: {1}. It is not marked whole." -f $short.Count, $(if ($short.Count) { $short[0] } else { "a folder" }))
    }
    Write-Manifest $manifest $src.Files
    Write-Host ("copied        : {0} files and {1} folders, {2:N1} MB, every file hashed and matching NM Fed, marked whole" -f $made.Files.Count, $made.Dirs.Count, ($bytes / 1MB))
}
if (-not $Set) { exit 0 }

# The copy of the run set, made from the source copy just matched to NM Fed, so NM Fed is
# read once. Room first. A copy that fails its read back keeps no manifest, so it is never
# read as whole, and it is left as it is, because a run set's copy is never emptied.
$drive = New-Object IO.DriveInfo ([IO.Path]::GetPathRoot($workFull))
$needed = $bytes + 1GB
Write-Host ("room on {0}    : {1:N1} GB free, {2:N1} GB needed for the copy of set {3}" -f $drive.Name, ($drive.AvailableFreeSpace / 1GB), ($needed / 1GB), $Set)
if ($drive.AvailableFreeSpace -lt $needed) { throw "Not enough room on $($drive.Name) for the copy of set $Set. Nothing was written for it." }

$want = Read-Manifest $manifest
$dirs = New-Object 'System.Collections.Generic.HashSet[string]' ($ordinal)
foreach ($d in @($src.Dirs) + $reports) { [void]$dirs.Add($d) }
Copy-Tree $copy $target $dirs $src.Files.Keys
$made = Read-Folder $target $true
$short = @($want.Keys | Where-Object { -not $made.Files.ContainsKey($_) -or $made.Files[$_].Sha -ne $want[$_].Sha -or $made.Files[$_].Bytes -ne $want[$_].Bytes })
if ($short.Count -gt 0 -or $made.Files.Count -ne $want.Count -or -not $made.Dirs.SetEquals($dirs)) {
    throw ("The copy of set {0} does not match source.manifest.txt after copying, {1} file(s) short or different, first: {2}. It is not marked whole, and it is left as it is, because a run set's copy is never emptied. Give the next set number." -f $Set, $short.Count, $(if ($short.Count) { $short[0] } elseif ($made.Files.Count -ne $want.Count) { "a file the manifest does not name" } else { "a folder" }))
}
Write-Manifest $targetManifest $made.Files
Write-Host ("copied        : {0} files and {1} folders into the copy of set {2}, {3:N1} MB, every file read back by sha256 against source.manifest.txt" -f $made.Files.Count, $made.Dirs.Count, $Set, ($bytes / 1MB))
Write-Host ("folders made  : {0}, one under Clash Report for each folder under NWC" -f ($reports -join ", "))
Write-Host ("marked whole  : {0}, {1} lines, written last" -f $targetManifest, $made.Files.Count)
