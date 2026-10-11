<#
    Builds the add-in, assembles the bundle, and copies it to the per user plugin folder.
    Nothing here needs admin rights and nothing is written outside the user's profile.

    Usage:
        powershell -ExecutionPolicy Bypass -File build\install.ps1
        powershell -ExecutionPolicy Bypass -File build\install.ps1 -Configuration Debug
        powershell -ExecutionPolicy Bypass -File build\install.ps1 -NavisworksPath "D:\Autodesk\Navisworks Manage 2025"
#>

[CmdletBinding()]
param(
    [string] $Configuration = "Release",
    [string] $NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025",
    [switch] $SkipBuild
)

$ErrorActionPreference = "Stop"

# How this script ends, written here once and read off here by INSTALL.md word for word. Each
# end is one exit code and one last line. Every end but the first names, each on a line of its
# own before the last line, the folders a run of this script left beside the add-in.
#
#   exit 0  the last line is the one in $InstalledLine. The new add-in is installed, every check
#           of it passed, and no folder is left beside it
#   exit 1  the last line starts FAILED:. The new add-in is not installed. The line says why,
#           where the add-in installed before is, and where the new one went
#   exit 2  the last line starts REFUSED:. Nothing was installed. The line says why and what a
#           person does
#   exit 3  the last line starts LEFT:. The new add-in is installed and every check of it passed,
#           and the add-in installed before is still beside it, named on that line
$InstalledExit = 0
$InstalledLine = "Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator."
$FailedExit    = 1
$FailedStart   = "FAILED: "
$RefusedExit   = 2
$RefusedStart  = "REFUSED: "
$LeftExit      = 3
$LeftStart     = "LEFT: "

$repo       = Split-Path -Parent $PSScriptRoot
$addinProj  = Join-Path $repo "src\Federator.Addin\Federator.Addin.csproj"
$bundleSrc  = Join-Path $repo "bundle\ParsonsNwcFederator.bundle"
$staging    = Join-Path $repo "artifacts\ParsonsNwcFederator.bundle"
$target     = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
$plugins    = Split-Path $target -Parent
$buildOut   = Join-Path $repo "src\Federator.Addin\bin\$Configuration\net48"

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

# The add-in installed before once it is moved aside, whether the copy of the new one into the
# place Navisworks loads from has begun, and whether every check of the new one has passed.
$aside = $null
$newIn = $false
$checked = $false
$undoing = $false

# The words of the innermost exception, without the wrapping PowerShell puts round a .NET call.
function Why($e) {
    while ($null -ne $e.InnerException) { $e = $e.InnerException }
    return $e.Message.TrimEnd('.')
}

function Now { return [DateTime]::Now.ToString("yyyyMMdd-HHmmss") }

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

# Every folder a run of this script leaves beside the add-in, ParsonsNwcFederator.bundle with
# .replaced- or .failed- and a time after it, read by one full path pattern each on the plugin
# folder alone, never a search below it.
function Leftovers {
    if (-not (Test-Path -LiteralPath $plugins)) { return }
    foreach ($kind in @(".replaced-*", ".failed-*")) {
        [System.IO.Directory]::GetFileSystemEntries($plugins, (Split-Path $target -Leaf) + $kind) | Sort-Object
    }
}

# Every end but the installed one first names each folder left beside the add-in on a line of
# its own, whatever stopped the install, then writes its last line and exits with its code.
function Finish($code, $line) {
    if ($code -ne $InstalledExit) {
        try { foreach ($l in @(Leftovers)) { Write-Host ("  left beside the add-in: " + $l) } }
        catch { Write-Host ("  the folders beside the add-in could not be read, " + (Why $_.Exception)) }
    }
    Write-Host $line
    exit $code
}
function Refuse($why) { Finish $RefusedExit ($RefusedStart + $why) }
function Fail($why) { Finish $FailedExit ($FailedStart + $why) }

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

# The new add-in at $target taken out of the place Navisworks loads from: removed, or moved
# aside to .failed- and the time when it will not go, each failure printed on its own line.
# Returns where it is, null once removed.
function TakeNewOut {
    if (-not (Test-Path -LiteralPath $target)) { return $null }
    $notGone = RemoveTree $target
    if ($null -eq $notGone) { return $null }
    Write-Host ("The new add-in could not be removed, " + $notGone)
    $failedAt = $target + ".failed-" + (Now)
    try { Rename-Item -LiteralPath $target -NewName (Split-Path $failedAt -Leaf) -ErrorAction Stop; return $failedAt }
    catch { Write-Host ("The new add-in could not be moved aside either, " + (Why $_.Exception)); return $target }
}

# After a failure once the copy of the new add-in began: the new one is taken out, and the one
# installed before, when it was moved aside, is put back once its place is free. Returns where
# each ended up, in words.
function Undo {
    if (-not $newIn) { return "Nothing was installed" }
    if ($checked) { return ("The new add-in passed every check and is at " + $target) }
    $newAt = TakeNewOut
    $new = "was removed"
    if ($newAt -eq $target) { $new = "is at " + $target + ". It is where Navisworks loads it, whole or in part, so delete it with Navisworks closed before Navisworks starts" }
    elseif ($null -ne $newAt) { $new = "is at " + $newAt + ", out of the place Navisworks loads from" }
    if ($null -eq $aside) { return ("No add-in was installed before, and the new one that failed " + $new) }
    $oldAt = $aside
    if (-not (Test-Path -LiteralPath $target)) {
        try { Rename-Item -LiteralPath $aside -NewName (Split-Path $target -Leaf) -ErrorAction Stop; $oldAt = $target }
        catch { Write-Host ("The add-in installed before could not be put back, " + (Why $_.Exception)) }
    }
    return ("The add-in installed before is at " + $oldAt + ", and the new one that failed " + $new)
}

# Whatever throws ends here, as one FAILED line saying why and where each thing is.
trap {
    $why = Why $_.Exception
    if ($script:undoing) { Fail ($why + ", while things were put back after a failure. What is at " + $target + " and beside it was not read") }
    $script:undoing = $true
    Fail ($why + ". " + (Undo) + ".")
}

Write-Host "Parsons NWC Federator install"
Write-Host "  repo          : $repo"
Write-Host "  configuration : $Configuration"
Write-Host "  navisworks    : $NavisworksPath"
Write-Host ""

# No folder on the way from %APPDATA% down to the bundle may be a junction or a link,
# because removing a bundle through one would remove what it points at.
$walkUp = $target
while ($null -ne $walkUp -and $walkUp.Length -ge $env:APPDATA.TrimEnd('\').Length) {
    if (Test-Path -LiteralPath $walkUp) {
        if ($null -ne (LinkKind (Get-Item -LiteralPath $walkUp -Force))) { Refuse ($walkUp + " is a junction or a link, so the add-in is not installed through it. Nothing was installed.") }
    }
    if ($walkUp.TrimEnd('\') -ieq $env:APPDATA.TrimEnd('\')) { break }
    $walkUp = Split-Path $walkUp -Parent
}

# A run stopped between the move aside below and the removal at its end, a closed window, a
# machine that went off or a kill, leaves the add-in installed before whole beside the place
# Navisworks loads from, and that place empty or holding part of the new one. So before
# anything else every folder a run left there is read. Where that place is empty or partial
# and one whole add-in sits beside it, that one is put back. Any other folder left there
# refuses the install, each named, because which one a person wants is theirs to say.
$left = @(Leftovers)
if ($left.Count -gt 0) {
    $loadMissing = $expected
    $was = "was empty"
    if (Test-Path -LiteralPath $target) {
        $loadMissing = @(MissingIn $target)
        if ($loadMissing.Count -gt 0) { $was = "held part of an add-in, " + $loadMissing.Count + " of its " + $expected.Count + " files missing, the first " + $loadMissing[0] }
    }
    $loadWhole = $loadMissing.Count -eq 0
    $whole = @($left | Where-Object { $_ -like "*.replaced-*" -and (Test-Path -LiteralPath $_ -PathType Container) -and @(MissingIn $_).Count -eq 0 })
    $putBack = $false
    if (-not $loadWhole -and $whole.Count -eq 1) {
        $navisworks = NavisworksRefusal
        if ($null -ne $navisworks) { Refuse ($navisworks + ". The add-in installed before sits whole at " + $whole[0] + " and was not put back. Nothing was installed.") }
        $cleared = ""
        if (Test-Path -LiteralPath $target) {
            $notGone = RemoveTree $target
            if ($null -ne $notGone) { Refuse ("the add-in installed before sits whole at " + $whole[0] + ", and " + $target + " " + $was + " and could not be removed, " + $notGone + ". Close Navisworks, delete " + $target + " and run this again, which puts the one beside it back. Nothing was installed.") }
            $cleared = ", and what was there was removed"
        }
        try { Rename-Item -LiteralPath $whole[0] -NewName (Split-Path $target -Leaf) -ErrorAction Stop }
        catch { Refuse ("the add-in installed before sits whole at " + $whole[0] + " and could not be put back, " + (Why $_.Exception) + ". Close Navisworks and run this again. Nothing was installed.") }
        Write-Host ("PUT BACK: " + $target + " " + $was + ", and the add-in installed before sat whole at " + $whole[0] + ", left by a run of this script that stopped part way. It is back at " + $target + $cleared + ".")
        $putBack = $true
        $left = @(Leftovers)
    }
    if ($left.Count -gt 0) {
        $unknown = "Whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN. Nothing was installed."
        if ($putBack) { Refuse ("the add-in installed before was put back, as the PUT BACK line says, and " + $left.Count + " more folder(s) left by earlier runs of this script sit beside it, each named above. Close Navisworks, delete each folder named above, and run this again. " + $unknown) }
        $sitting = "" + $left.Count + " folder(s) left by earlier runs of this script sit beside the add-in, each named above, "
        if ($loadWhole) { Refuse ($sitting + "and the add-in at " + $target + " holds every file it needs, so none of them is put back. Close Navisworks, delete each folder named above, and run this again. " + $unknown) }
        if ($whole.Count -gt 1) { Refuse ($sitting + $target + " " + $was + ", and " + $whole.Count + " of them hold a whole add-in, so which one goes back is for a person to say. Close Navisworks, delete each folder named above but the one to keep, and run this again, which puts that one back. " + $unknown) }
        Refuse ($sitting + $target + " " + $was + ", and none of them holds a whole add-in to put back. Close Navisworks, delete each folder named above, and run this again, which installs the add-in afresh. " + $unknown)
    }
}

if (-not (Test-Path (Join-Path $NavisworksPath "Autodesk.Navisworks.Api.dll"))) {
    throw "Autodesk.Navisworks.Api.dll was not found under '$NavisworksPath'. Pass -NavisworksPath with the real install folder"
}

if (-not $SkipBuild) {
    Write-Host "Building..."
    & dotnet build $addinProj -c $Configuration -p:NavisworksPath="$NavisworksPath" --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "The build failed with exit code $LASTEXITCODE" }
    Write-Host ""
}

if (-not (Test-Path $buildOut)) {
    throw "Build output not found at '$buildOut'. Run without -SkipBuild"
}

# Assemble the bundle in artifacts first, so a half copied bundle never reaches the
# plugin folder where Navisworks would try to load it.
$stagingLinks = @(LinksAt $staging)
if ($stagingLinks.Count -gt 0) { Refuse ("the staging folder " + $staging + " holds a junction or a link, " + ($stagingLinks -join ", ") + ", and nothing is removed through one. Take each out and run this again. Nothing was installed.") }
if (Test-Path -LiteralPath $staging) {
    $notGone = RemoveTree $staging
    if ($null -ne $notGone) { throw ("The staging folder " + $staging + " could not be cleared, " + $notGone) }
}
$contents = Join-Path $staging "Contents\v22"
New-Item -ItemType Directory -Force -Path $contents | Out-Null

Copy-Item (Join-Path $bundleSrc "PackageContents.xml") $staging -Force

foreach ($name in ($carried + $library)) {
    $from = Join-Path $buildOut $name
    if (-not (Test-Path $from)) { throw "Expected '$name' in '$buildOut' and it is not there" }
    Copy-Item $from $contents -Force
}

# The Navisworks assemblies are deliberately not carried. The running application already
# has them loaded, and a second copy would load a second set of types.
$strays = Get-ChildItem $contents -Filter "*Navisworks*" -ErrorAction SilentlyContinue
if ($strays) { throw "A Navisworks assembly ended up in the bundle: $($strays.Name -join ', ')" }

# Read here, after the build, which takes long enough for one to be started. Each refusal is
# one line and exit 2, and nothing is installed.
$navisworks = NavisworksRefusal
if ($null -ne $navisworks) { Refuse ($navisworks + ". Nothing was installed.") }

# A junction or a link inside the installed bundle refuses the install before it is moved
# aside, because the removal of the one moved aside would remove what it points at.
$installedLinks = @(LinksAt $target)
if ($installedLinks.Count -gt 0) { Refuse ("the installed add-in at " + $target + " holds a junction or a link, " + ($installedLinks -join ", ") + ", and nothing is removed through one. Take each out with Navisworks closed and run this again. Nothing was installed.") }

# The installed bundle is moved aside by one rename first. A rename fails whole when a file
# in the bundle is held, so the old bundle stays whole where it was. Measured on 2026-09-30
# with no Navisworks: a DLL that .NET loaded with Assembly.LoadFrom in another process
# blocks the rename of its folder. Whether Navisworks holds the add-in's DLLs that way is
# UNKNOWN. Then the new one is copied in and checked, and only once the last check has
# passed is the one moved aside removed. On a failure after the move the new one is taken
# out and the old one put back, and where each is is printed. A run stopped between the move
# and the removal is found by the next run, at its start. Whether Navisworks loads a folder
# whose name does not end in .bundle is UNKNOWN.
if (Test-Path -LiteralPath $target) {
    $movedTo = $target + ".replaced-" + (Now)
    try { Rename-Item -LiteralPath $target -NewName (Split-Path $movedTo -Leaf) -ErrorAction Stop }
    catch { Refuse ("the installed add-in could not be moved aside, a file in it is held, most likely by a Navisworks that is running, " + (Why $_.Exception) + ". The installed add-in is left whole. Close Navisworks and run this again. Nothing was installed.") }
    $aside = $movedTo
}

# The contents, never the folder. Copy-Item of a directory puts it INSIDE the destination
# when the destination already exists, and creates it when it does not, so the same line
# does two different things depending on whether the folder is there. That happened on
# 2026-08-31 and left ParsonsNwcFederator.bundle inside ParsonsNwcFederator.bundle, which
# Navisworks does not read at all. From here a failure takes the new one out of the place
# Navisworks loads from, whether or not an add-in was installed before.
$newIn = $true
try {
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item (Join-Path $staging "*") $target -Recurse -Force
} catch { throw ("The new add-in could not be copied in, " + (Why $_.Exception)) }

# Every check from here reads the installed bundle, never the staging copy, because the
# installed one is what Navisworks loads and what the removal below rests on.
# A check that fails throws, and its words carry what failed.
$installed = Join-Path $target "Contents\v22"
try {
$nested = Join-Path $target (Split-Path -Leaf $target)
if (Test-Path $nested) {
    throw "The bundle ended up inside itself at '$nested', which Navisworks does not read"
}

# Report only what is actually on disk.
$written = Get-ChildItem $target -Recurse -File | Sort-Object FullName
Write-Host "Installed to:"
Write-Host "  $target"
Write-Host ""
Write-Host "Files written:"
foreach ($f in $written) {
    Write-Host ("  {0}  ({1} bytes)" -f $f.FullName.Substring($target.Length + 1), $f.Length)
}
Write-Host ""

$missing = @(MissingIn $target)
if ($missing.Count -gt 0) {
    throw "The new add-in is incomplete. Missing: $($missing -join ', ')"
}

Write-Host ("All {0} expected files are present." -f $expected.Count)

# The stamp the build wrote into the add-in, read off the installed file and the built one,
# because a copy that ended on an older file reads as whole by its names alone.
$builtStamp = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $buildOut "Federator.Addin.dll")).ProductVersion
$installedStamp = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $installed "Federator.Addin.dll")).ProductVersion
if ($installedStamp -ne $builtStamp) { throw ("The installed add-in reads the stamp " + $installedStamp + " where the build wrote " + $builtStamp) }
Write-Host ("The installed add-in reads the stamp " + $installedStamp + ", the one the build wrote.")
Write-Host ""

# Every assembly the bundle references has to be in the bundle or in the framework.
# A run on aa163c9e got ninety seconds in and then failed to write the workbook, and
# a missing file and a version mismatch look identical from the outside. This walks
# what is actually in the folder rather than trusting the list above, so a package
# that gains a dependency is caught here rather than during a run. Each one is read from
# its bytes, so this process holds no file of the installed bundle open and a failure can
# still take the new one out.
Write-Host "Checking every assembly the bundle needs:"

$inBundle = @{}
foreach ($f in (Get-ChildItem $installed -Filter *.dll)) {
    $inBundle[[System.IO.Path]::GetFileNameWithoutExtension($f.Name)] = $f.FullName
}

$missing = @()
$mismatched = @()
$seen = @{}

foreach ($f in (Get-ChildItem $installed -Filter *.dll | Sort-Object Name)) {
    try { $asm = [System.Reflection.Assembly]::ReflectionOnlyLoad([System.IO.File]::ReadAllBytes($f.FullName)) }
    catch { Write-Host ("  {0} could not be read as an assembly, so what it needs was not checked, {1}" -f $f.Name, (Why $_.Exception)); continue }

    foreach ($ref in $asm.GetReferencedAssemblies()) {
        if ($inBundle.ContainsKey($ref.Name)) {
            $have = [System.Reflection.AssemblyName]::GetAssemblyName($inBundle[$ref.Name]).Version
            if ($have -ne $ref.Version) {
                $key = "{0} {1} {2}" -f $asm.GetName().Name, $ref.Name, $ref.Version
                if (-not $seen.ContainsKey($key)) {
                    $seen[$key] = $true
                    $mismatched += ("  {0,-38} wants {1,-40} the file is {2}" -f $asm.GetName().Name, ($ref.Name + " " + $ref.Version), $have)
                }
            }
            continue
        }

        # Not ours. It is fine if the framework supplies it, and fine if Navisworks
        # does, because the running application already has those loaded and a second
        # copy in the bundle would load a second set of types.
        $ok = Test-Path (Join-Path $NavisworksPath ($ref.Name + ".dll"))

        if (-not $ok) {
            try { [System.Reflection.Assembly]::ReflectionOnlyLoad($ref.FullName) | Out-Null; $ok = $true } catch { }
        }

        if (-not $ok) {
            try { [System.Reflection.Assembly]::ReflectionOnlyLoad($ref.Name) | Out-Null; $ok = $true } catch { }
        }

        if (-not $ok -and -not $seen.ContainsKey($ref.Name)) {
            $seen[$ref.Name] = $true
            $missing += ("  {0} needs {1}, which is neither in the bundle nor in the framework" -f $asm.GetName().Name, $ref.Name)
        }
    }
}

if ($missing.Count -gt 0) {
    foreach ($line in $missing) { Write-Host $line }
    throw ("The new add-in is incomplete. {0} assembly reference(s) cannot be satisfied. Add the file(s) to the carried list in this script" -f $missing.Count)
}
} catch { throw ("The new add-in failed its check, " + (Why $_.Exception)) }

Write-Host ("  every reference is satisfied, {0} assemblies checked. Navisworks supplies its own." -f $inBundle.Count)
$checked = $true

if ($mismatched.Count -gt 0) {
    Write-Host ""
    Write-Host ("  {0} reference(s) bind to a version the shipped file does not carry." -f $mismatched.Count)
    Write-Host "  These are NOT faults. There is no application config to put a binding"
    Write-Host "  redirect in, so the add-in resolves them by name from this folder."
    foreach ($line in $mismatched) { Write-Host $line }
}

# Every check has passed, so the add-in installed before is removed only now. When it cannot
# be, the install ends on the LEFT line, never on the installed one.
if ($null -ne $aside) {
    $notGone = RemoveTree $aside
    if ($null -ne $notGone) { Finish $LeftExit ($LeftStart + "the new add-in is installed at " + $target + " and passed every check, and the add-in installed before is at " + $aside + ", whole or in part, and could not be removed, " + $notGone + ". Whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN. Close Navisworks and delete that folder. Until it is gone this script refuses to run and names it.") }
}

$after = @(Leftovers)
if ($after.Count -gt 0) { Finish $LeftExit ($LeftStart + "the new add-in is installed at " + $target + " and passed every check, and " + $after.Count + " folder(s) sit beside it, each named above. Whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN. Close Navisworks and delete each. Until they are gone this script refuses to run and names them.") }

Finish $InstalledExit $InstalledLine
