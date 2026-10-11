<#
    What build\install.ps1 and the team install, build\team-install.ps1, both hold to, kept here
    once and dot-sourced by each: where the add-in goes, the files a whole bundle holds, the
    stamp the build writes, the refusal while Navisworks runs, and the rule that nothing is
    removed through a junction or a link. Each script sets $ErrorActionPreference before it
    dot-sources this file. It does nothing on its own.
#>

# The one place Navisworks Manage 2025 loads the add-in from, in the person's own profile.
$target  = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
$plugins = Split-Path $target -Parent

# The two the tool is, and the workbook library the report writer needs. ClosedXML pulls
# eleven more DLLs and every one of them has to travel, or the add-in loads and then
# throws the first time a group finishes. Checked on 2026-08-31: none of them collides
# with a file the Navisworks install ships, see docs\history\scan.md section 4h.
$carried = @("Federator.Addin.dll", "Federator.Core.dll")
$library = @(
    "ClosedXML.dll",
    "ClosedXML.Parser.dll",
    "DocumentFormat.OpenXml.dll",
    "DocumentFormat.OpenXml.Framework.dll",
    "ExcelNumberFormat.dll",
    "Microsoft.Bcl.HashCode.dll",
    "RBush.dll",
    "SixLabors.Fonts.dll",
    "System.Buffers.dll",
    "System.Memory.dll",
    "System.Numerics.Vectors.dll",
    "System.Runtime.CompilerServices.Unsafe.dll"
)
$expected = @("PackageContents.xml")
foreach ($name in ($carried + $library)) { $expected += (Join-Path "Contents\v22" $name) }

# The words of the innermost exception, without the wrapping PowerShell puts round a .NET call.
function Why($e) {
    while ($null -ne $e.InnerException) { $e = $e.InnerException }
    return $e.Message.TrimEnd('.')
}

# The kind of link $item is, Junction or SymbolicLink as PowerShell reads it off the reparse
# point, or null when it is neither. Every file and folder OneDrive syncs carries the reparse
# point attribute with no link type, measured on 2026-09-27 and again on 2026-10-04 on a clone
# under OneDrive, which is where artifacts is, so the attribute alone would take each of them
# for a link.
function LinkKind($item) {
    if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -eq 0) { return $null }
    $kind = [string]$item.LinkType
    if ($kind -eq "Junction" -or $kind -eq "SymbolicLink") { return $kind }
    return $null
}

# Every junction or link at $dir or under it, each by its full path. One folder is read at a
# time and a link is never read through, so nothing it points at is read. None when $dir is
# not there.
function LinksAt($dir) {
    if (-not (Test-Path -LiteralPath $dir)) { return }
    $top = Get-Item -LiteralPath $dir -Force
    if ($null -ne (LinkKind $top)) { return $top.FullName }
    $todo = New-Object System.Collections.Generic.Stack[string]
    $todo.Push($top.FullName)
    while ($todo.Count -gt 0) {
        foreach ($item in @(Get-ChildItem -LiteralPath $todo.Pop() -Force)) {
            if ($null -ne (LinkKind $item)) { $item.FullName; continue }
            if ($item.PSIsContainer) { $todo.Push($item.FullName) }
        }
    }
}

# The first folder from %APPDATA% down to $dir that is a junction or a link, in words, or null
# when none is, because removing a bundle through one would remove what it points at.
function LinkOnTheWay($dir) {
    $walkUp = $dir
    while ($null -ne $walkUp -and $walkUp.Length -ge $env:APPDATA.TrimEnd('\').Length) {
        if (Test-Path -LiteralPath $walkUp) {
            if ($null -ne (LinkKind (Get-Item -LiteralPath $walkUp -Force))) { return ($walkUp + " is a junction or a link, so the add-in is not installed through it") }
        }
        if ($walkUp.TrimEnd('\') -ieq $env:APPDATA.TrimEnd('\')) { break }
        $walkUp = Split-Path $walkUp -Parent
    }
    return $null
}

# Removes the folder $dir and everything in it, and returns null once it is gone, or the words
# of why it is not. A junction or a link at it or under it refuses the removal before anything
# is removed, because what a removal through one reaches is not proved. Measured on 2026-10-08,
# turn6\f109b-proof cases C and J, Windows PowerShell 5.1 on one machine removed a junction
# inside a folder and left what it points at. Whether a symbolic link, another build of Windows
# or another way of removing does the same is UNKNOWN. A removal that stops part way leaves
# part of the folder.
function RemoveTree($dir) {
    $links = @(LinksAt $dir)
    if ($links.Count -gt 0) { return ("it holds a junction or a link, " + ($links -join ", ") + ", and nothing is removed through one, so nothing of it was removed") }
    try { Remove-Item -LiteralPath $dir -Recurse -Force -ErrorAction Stop }
    catch { return (Why $_.Exception) }
    return $null
}

# The files of a whole bundle that $dir does not hold, by their path below it.
function MissingIn($dir) {
    foreach ($rel in $expected) { if (-not (Test-Path -LiteralPath (Join-Path $dir $rel))) { $rel } }
}

# Nothing is replaced while any Navisworks runs, whoever started it, because a running
# Navisworks may hold the bundle's files, and one that loaded the old build keeps running it.
# Returns why the install stops, or null.
function NavisworksRefusal {
    $roamers = $null
    try { $roamers = @([System.Diagnostics.Process]::GetProcessesByName("Roamer")) }
    catch { return ("Navisworks may be running, the process list could not be read, " + (Why $_.Exception) + ". Close Navisworks first and run this again") }
    if ($roamers.Count -gt 0) { return ("Navisworks is running, Roamer pid " + (($roamers | ForEach-Object { $_.Id }) -join ", ") + ", and must be closed first. Close it and run this again") }
    return $null
}

# The stamp Directory.Build.targets writes into the add-in, read off the file at $dll:
# the version, the eight hex of the commit with +edits after it when the tree had changes,
# and the moment it was built.
function StampOf($dll) {
    return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion
}

# The stamp in its parts, Version, Commit, Edits, Built and Day, the day as yyyyMMdd, or null
# when it is not in the shape Directory.Build.targets writes.
function StampParts($stamp) {
    if ($null -eq $stamp) { return $null }
    $m = [regex]::Match($stamp, '^(\S+) ([0-9a-f]{8}|nogit)(\+edits)? built (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})$')
    if (-not $m.Success) { return $null }
    $at = [DateTime]::ParseExact($m.Groups[4].Value, "yyyy-MM-dd HH:mm:ss", [System.Globalization.CultureInfo]::InvariantCulture)
    return [pscustomobject]@{
        Version = $m.Groups[1].Value
        Commit  = $m.Groups[2].Value
        Edits   = $m.Groups[3].Success
        Built   = $m.Groups[4].Value
        Day     = $at.ToString("yyyyMMdd")
    }
}
