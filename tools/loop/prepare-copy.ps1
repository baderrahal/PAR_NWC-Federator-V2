<#
    The one script that reads NM Fed. Every loop run works on a copy of it under
    %LOCALAPPDATA%\NwcFederatorLoop\source, and nothing here ever writes into NM Fed.
    The wall in .claude\hooks refuses any other command that names NM Fed.

    Usage, from the repo root:
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Listing steps\runs\00\source-listing.txt
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Remove <file name of one NWC>
        powershell -ExecutionPolicy Bypass -File tools\loop\prepare-copy.ps1 -Restore <file name of one NWC>

    With no switch it makes the copy, or keeps the copy already there when every file in
    NM Fed is in it under the same relative path with the same size and nothing else is.
    -Remove takes one NWC out of the copy for the run that loses a file. -Restore copies
    that one NWC back from NM Fed for the run that gets it back. -Listing writes what NM Fed
    holds at every level into a text file, one line per file.
#>

[CmdletBinding()]
param(
    [string] $Listing,
    [string] $Remove,
    [string] $Restore
)

$ErrorActionPreference = "Stop"

$source = Join-Path ([Environment]::GetFolderPath('Desktop')) "NM Fed"
$work   = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$copy   = Join-Path $work "source"

if (-not (Test-Path -LiteralPath $source)) {
    throw "NM Fed was not found at '$source'. Nothing was copied."
}

function Get-Files([string] $root) {
    $list = @{}
    foreach ($f in Get-ChildItem -LiteralPath $root -Recurse -File -Force) {
        $list[$f.FullName.Substring($root.Length + 1)] = $f.Length
    }
    return $list
}

function Find-One([string] $root, [string] $name) {
    $hits = @(Get-ChildItem -LiteralPath $root -Recurse -File -Force | Where-Object { $_.Name -eq $name })
    if ($hits.Count -eq 0) { throw "No file named '$name' under '$root'." }
    if ($hits.Count -gt 1) { throw "'$name' is under '$root' $($hits.Count) times. Name it by a file that is there once." }
    return $hits[0]
}

$inSource = Get-Files $source
$sourceBytes = ($inSource.Values | Measure-Object -Sum).Sum

Write-Host "NM Fed        : $source"
Write-Host ("  files       : {0}, {1:N1} MB" -f $inSource.Count, ($sourceBytes / 1MB))
Write-Host "copy          : $copy"

if ($Listing) {
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("NM Fed at $source, read $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $lines.Add(("{0} files, {1:N0} bytes" -f $inSource.Count, $sourceBytes))
    $lines.Add("")
    $lines.Add("relative path | bytes | part 3 | part 5 | parts")
    foreach ($rel in ($inSource.Keys | Sort-Object)) {
        $name = [IO.Path]::GetFileNameWithoutExtension($rel)
        $parts = $name.Split('-')
        $p3 = if ($parts.Count -ge 3) { $parts[2] } else { "none" }
        $p5 = if ($parts.Count -ge 5) { $parts[4] } else { "none" }
        if ([IO.Path]::GetExtension($rel) -ne ".nwc") { $p3 = "-"; $p5 = "-" }
        $lines.Add(("{0} | {1} | {2} | {3} | {4}" -f $rel, $inSource[$rel], $p3, $p5, $parts.Count))
    }
    $lines.Add("")
    foreach ($x in (Get-ChildItem -LiteralPath $source -Recurse -File -Force | Where-Object { $_.Extension -ne ".nwc" })) {
        $lines.Add(("sha256 {0} {1}" -f (Get-FileHash -LiteralPath $x.FullName -Algorithm SHA256).Hash, $x.FullName.Substring($source.Length + 1)))
    }
    $dir = Split-Path -Parent $Listing
    if ($dir -and -not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [IO.File]::WriteAllLines($Listing, $lines)
    Write-Host "listing       : $Listing, $($lines.Count) lines"
}

if ($Remove) {
    if (-not (Test-Path -LiteralPath $copy)) { throw "There is no copy at '$copy' to take a file out of." }
    $f = Find-One $copy $Remove
    Remove-Item -LiteralPath $f.FullName
    if (Test-Path -LiteralPath $f.FullName) { throw "'$($f.FullName)' is still there after the remove." }
    Write-Host "removed       : $($f.FullName.Substring($copy.Length + 1)) from the copy. NM Fed is untouched."
    exit 0
}

if ($Restore) {
    $from = Find-One $source $Restore
    $rel = $from.FullName.Substring($source.Length + 1)
    $to = Join-Path $copy $rel
    if (Test-Path -LiteralPath $to) { throw "'$rel' is already in the copy. Nothing was restored." }
    Copy-Item -LiteralPath $from.FullName -Destination $to
    $back = (Get-Item -LiteralPath $to).Length
    if ($back -ne $from.Length) { throw "'$rel' came back at $back bytes against $($from.Length) in NM Fed." }
    Write-Host "restored      : $rel, $back bytes, copied from NM Fed into the copy"
    exit 0
}

# Keep the copy only when it is exactly NM Fed by name and size.
$reuse = $false
if (Test-Path -LiteralPath $copy) {
    $inCopy = Get-Files $copy
    $same = ($inCopy.Count -eq $inSource.Count)
    if ($same) {
        foreach ($rel in $inSource.Keys) {
            if (-not $inCopy.ContainsKey($rel) -or $inCopy[$rel] -ne $inSource[$rel]) { $same = $false; break }
        }
    }
    $reuse = $same
    if (-not $same) {
        Write-Host ("the copy differs from NM Fed, {0} files against {1}, so it is made again" -f $inCopy.Count, $inSource.Count)
        Remove-Item -LiteralPath $copy -Recurse -Force
    }
}

if ($reuse) {
    Write-Host "kept          : the copy already matches NM Fed by name and size on every file"
    exit 0
}

New-Item -ItemType Directory -Force -Path $work | Out-Null
$drive = New-Object IO.DriveInfo ([IO.Path]::GetPathRoot($work))
$needed = $sourceBytes + 1GB
Write-Host ("free on {0}   : {1:N1} GB, needed {2:N1} GB" -f $drive.Name, ($drive.AvailableFreeSpace / 1GB), ($needed / 1GB))
if ($drive.AvailableFreeSpace -lt $needed) {
    throw "Not enough room on $($drive.Name) for the copy. Nothing was copied."
}

foreach ($rel in ($inSource.Keys | Sort-Object)) {
    $to = Join-Path $copy $rel
    $dir = Split-Path -Parent $to
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    Copy-Item -LiteralPath (Join-Path $source $rel) -Destination $to
}

# Folders with nothing in them are part of what NM Fed looks like, so they come too.
foreach ($d in Get-ChildItem -LiteralPath $source -Recurse -Directory -Force) {
    $to = Join-Path $copy $d.FullName.Substring($source.Length + 1)
    if (-not (Test-Path -LiteralPath $to)) { New-Item -ItemType Directory -Force -Path $to | Out-Null }
}

$made = Get-Files $copy
$short = @($inSource.Keys | Where-Object { -not $made.ContainsKey($_) -or $made[$_] -ne $inSource[$_] })
if ($short.Count -gt 0) {
    throw ("The copy is short. {0} file(s) missing or a different size, first: {1}" -f $short.Count, $short[0])
}
Write-Host ("copied        : {0} files, {1:N1} MB, every size read back and matching NM Fed" -f $made.Count, (($made.Values | Measure-Object -Sum).Sum / 1MB))
