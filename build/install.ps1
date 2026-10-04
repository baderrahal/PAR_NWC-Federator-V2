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

$repo       = Split-Path -Parent $PSScriptRoot
$addinProj  = Join-Path $repo "src\Federator.Addin\Federator.Addin.csproj"
$bundleSrc  = Join-Path $repo "bundle\ParsonsNwcFederator.bundle"
$staging    = Join-Path $repo "artifacts\ParsonsNwcFederator.bundle"
$target     = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
$buildOut   = Join-Path $repo "src\Federator.Addin\bin\$Configuration\net48"

Write-Host "Parsons NWC Federator install"
Write-Host "  repo          : $repo"
Write-Host "  configuration : $Configuration"
Write-Host "  navisworks    : $NavisworksPath"
Write-Host ""

if (-not (Test-Path (Join-Path $NavisworksPath "Autodesk.Navisworks.Api.dll"))) {
    throw "Autodesk.Navisworks.Api.dll was not found under '$NavisworksPath'. Pass -NavisworksPath with the real install folder."
}

if (-not $SkipBuild) {
    Write-Host "Building..."
    & dotnet build $addinProj -c $Configuration -p:NavisworksPath="$NavisworksPath" --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "The build failed with exit code $LASTEXITCODE. Nothing was installed." }
    Write-Host ""
}

if (-not (Test-Path $buildOut)) {
    throw "Build output not found at '$buildOut'. Run without -SkipBuild."
}

# Assemble the bundle in artifacts first, so a half copied bundle never reaches the
# plugin folder where Navisworks would try to load it.
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
$contents = Join-Path $staging "Contents\v22"
New-Item -ItemType Directory -Force -Path $contents | Out-Null

Copy-Item (Join-Path $bundleSrc "PackageContents.xml") $staging -Force

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

foreach ($name in ($carried + $library)) {
    $from = Join-Path $buildOut $name
    if (-not (Test-Path $from)) { throw "Expected '$name' in '$buildOut' and it is not there." }
    Copy-Item $from $contents -Force
}

# The Navisworks assemblies are deliberately not carried. The running application already
# has them loaded, and a second copy would load a second set of types.
$strays = Get-ChildItem $contents -Filter "*Navisworks*" -ErrorAction SilentlyContinue
if ($strays) { throw "A Navisworks assembly ended up in the bundle: $($strays.Name -join ', ')" }

# The words of the innermost exception, without the wrapping PowerShell puts round a .NET call.
function Why($e) {
    while ($null -ne $e.InnerException) { $e = $e.InnerException }
    return $e.Message.TrimEnd('.')
}

# Nothing is replaced while any Navisworks runs, whoever started it, because a running
# Navisworks may hold the bundle's files, and one that loaded the old build keeps running
# it. Read here, after the build, which takes long enough for one to be started, and again
# when the move aside below is refused. Returns why the install stops, or null. Each
# refusal is one line and exit 2, and nothing is installed.
function NavisworksRefusal {
    $roamers = $null
    try { $roamers = @([System.Diagnostics.Process]::GetProcessesByName("Roamer")) }
    catch { return ("Navisworks may be running, the process list could not be read, " + (Why $_.Exception) + ". Close Navisworks first and run this again") }
    if ($roamers.Count -gt 0) { return ("Navisworks is running, Roamer pid " + (($roamers | ForEach-Object { $_.Id }) -join ", ") + ", and must be closed first. Close it and run this again") }
    return $null
}
$navisworks = NavisworksRefusal
if ($null -ne $navisworks) { Write-Host ("REFUSED: " + $navisworks + ". Nothing was installed."); exit 2 }

# No folder on the way from %APPDATA% down to the bundle may be a junction or a link,
# because removing a bundle through one would remove what it points at.
$walkUp = $target
while ($null -ne $walkUp -and $walkUp.Length -ge $env:APPDATA.TrimEnd('\').Length) {
    if (Test-Path -LiteralPath $walkUp) {
        $item = Get-Item -LiteralPath $walkUp -Force
        if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { Write-Host ("REFUSED: " + $walkUp + " is a junction or a link, so the add-in is not installed through it. Nothing was installed."); exit 2 }
    }
    if ($walkUp.TrimEnd('\') -ieq $env:APPDATA.TrimEnd('\')) { break }
    $walkUp = Split-Path $walkUp -Parent
}

# Every file under $dir by its path below $dir, with its sha256, and every folder below it.
# Each file is read sharing read, write and delete. A junction or a link inside is refused,
# because what is removed through one is what it points at.
function Listing($dir) {
    $root = (Get-Item -LiteralPath $dir -Force).FullName.TrimEnd('\')
    $files = @{}
    $folders = @()
    foreach ($item in @(Get-ChildItem -LiteralPath $root -Recurse -Force)) {
        $rel = $item.FullName.Substring($root.Length + 1)
        if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { throw ($rel + " is a junction or a link, so nothing is written or removed through it") }
        if ($item.PSIsContainer) { $folders += $rel; continue }
        $stream = New-Object System.IO.FileStream($item.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]"ReadWrite, Delete")
        $sha = [System.Security.Cryptography.SHA256]::Create()
        try { $files[$rel] = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace("-", "") }
        finally { $sha.Dispose(); $stream.Dispose() }
    }
    return [pscustomobject]@{ Files = $files; Folders = $folders }
}

# Makes the folder $to hold exactly what the folder $from holds, and returns the files it
# wrote and the files it removed. Every folder of $from is made, each file of $from is
# written over the one at the same place in $to unless that one already reads the same by
# sha256, and then every file and every folder $from does not have is removed, the deepest
# first. Last, $to is read back against $from by sha256. Each file is opened sharing read,
# write and delete, so a reader holding it open with every share mode stops neither the
# write nor the removal, measured on 2026-10-01 and 2026-10-04. The first failure throws,
# naming the file.
function WriteOver($from, $to) {
    $want = Listing $from
    $have = [pscustomobject]@{ Files = @{}; Folders = @() }
    if (Test-Path -LiteralPath $to) { $have = Listing $to }
    $wrote = @()
    $removed = @()
    New-Item -ItemType Directory -Force -Path $to | Out-Null
    foreach ($rel in $want.Folders) { New-Item -ItemType Directory -Force -Path (Join-Path $to $rel) | Out-Null }
    foreach ($rel in @($want.Files.Keys | Sort-Object)) {
        if ($have.Files.ContainsKey($rel) -and $have.Files[$rel] -eq $want.Files[$rel]) { continue }
        try {
            $src = New-Object System.IO.FileStream((Join-Path $from $rel), [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]"ReadWrite, Delete")
            try {
                $dst = New-Object System.IO.FileStream((Join-Path $to $rel), [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]"ReadWrite, Delete")
                try { $src.CopyTo($dst) } finally { $dst.Dispose() }
            } finally { $src.Dispose() }
        } catch { throw ($rel + " could not be written, " + (Why $_.Exception)) }
        $wrote += $rel
    }
    foreach ($rel in @($have.Files.Keys | Sort-Object)) {
        if ($want.Files.ContainsKey($rel)) { continue }
        try { [System.IO.File]::Delete((Join-Path $to $rel)) } catch { throw ($rel + " could not be removed, " + (Why $_.Exception)) }
        $removed += $rel
    }
    foreach ($rel in @($have.Folders | Sort-Object -Descending)) {
        if ($want.Folders -contains $rel) { continue }
        try { [System.IO.Directory]::Delete((Join-Path $to $rel), $false) } catch { throw ("the folder " + $rel + " could not be removed, " + (Why $_.Exception)) }
    }
    $now = Listing $to
    $differ = @($want.Files.Keys | Where-Object { -not $now.Files.ContainsKey($_) -or $now.Files[$_] -ne $want.Files[$_] }) + @($now.Files.Keys | Where-Object { -not $want.Files.ContainsKey($_) }) + @($want.Folders | Where-Object { $now.Folders -notcontains $_ }) + @($now.Folders | Where-Object { $want.Folders -notcontains $_ })
    if ($differ.Count -gt 0) { throw ($to + " does not read back the same by sha256, " + $differ.Count + " differ, the first " + $differ[0]) }
    return [pscustomobject]@{ Wrote = $wrote; Removed = $removed }
}

# The installed bundle is moved aside by one rename first. A rename fails whole when a file
# in the bundle is held, so the old bundle stays whole where it was. Measured on 2026-09-30
# with no Navisworks: a DLL that .NET loaded with Assembly.LoadFrom in another process
# blocks the rename of its folder. Whether Navisworks holds the add-in's DLLs that way is
# UNKNOWN. Then the new one is copied in and checked, and only once the last check has
# passed is the one moved aside removed. On a failure after the move the new one is taken
# out and the old one put back, and where each is is printed. Whether Navisworks loads a
# folder whose name does not end in .bundle is UNKNOWN.
#
# When the rename is refused, the process list is read again. A Navisworks running, or a list
# that cannot be read, refuses as before. With none the bundle is replaced in place. On
# 2026-10-01 Windows refused that rename on Bader's machine three times with no Navisworks
# running, while every file of the bundle opened for delete and for write with every share
# mode, and what holds it is UNKNOWN. Measured on 2026-10-01 and again on 2026-10-04 in a
# throwaway folder: one file beneath a folder held open by another process refuses the
# rename of the folder with the same words, access to the path is denied, whatever the share
# mode, and a file held that way sharing read, write and delete can still be written over and
# removed. So the installed bundle is first copied beside it, under the name the rename would
# have given it, and read back by sha256, and only then is each new file written over the old
# one. On any failure after that the copy is written back over the bundle the same way and
# read back, so a whole copy of the add-in installed before is on the disk at every moment.
$aside = $null
$inPlace = $false

# Removes the copy of the installed bundle made beside it, and returns null once it is gone,
# or the words of why it could not be removed.
function RemoveCopy {
    try { Remove-Item -LiteralPath $aside -Recurse -Force -ErrorAction Stop; return $null } catch { return (Why $_.Exception) }
}

if (Test-Path -LiteralPath $target) {
    $aside = $target + ".replaced-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
    try { Rename-Item -LiteralPath $target -NewName (Split-Path $aside -Leaf) -ErrorAction Stop }
    catch {
        $moveError = Why $_.Exception
        $navisworks = NavisworksRefusal
        if ($null -ne $navisworks) { Write-Host ("REFUSED: the installed add-in could not be moved aside, " + $moveError + ", and " + $navisworks + ". The installed add-in is left whole. Nothing was installed."); exit 2 }
        if (Test-Path -LiteralPath $aside) { Write-Host ("REFUSED: the installed add-in could not be moved aside, " + $moveError + ", no Navisworks runs, and " + $aside + " is there already, so no copy of it is made there. The installed add-in is left whole. Nothing was installed."); exit 2 }
        try { [void](WriteOver $target $aside) }
        catch {
            $copyError = Why $_.Exception
            $made = "nothing of it was made"
            if (Test-Path -LiteralPath $aside) {
                $notGone = RemoveCopy
                if ($null -eq $notGone) { $made = "what was made of it was removed" }
                else { $made = "what was made of it is at " + $aside + " and could not be removed, " + $notGone }
            }
            Write-Host ("REFUSED: the installed add-in could not be moved aside, " + $moveError + ", no Navisworks runs, and its copy beside it could not be made whole, " + $copyError + ", and " + $made + ". The installed add-in is left whole. Nothing was installed.")
            exit 2
        }
        Write-Host ("IN PLACE: the installed add-in could not be moved aside, " + $moveError + ", and no Navisworks runs, so its files are replaced where they are. A copy of it was made first at " + $aside + " and read back by sha256, and it is kept until every check of the new one has passed.")
        $inPlace = $true
    }
}

# After a failure once the old bundle was moved aside or copied beside it, the old one is put
# back and where each thing ends up is printed and returned for the failure's message. Moved
# aside: the new one is removed, or moved aside when it will not go, and the old one is put
# back when its place is free. Replaced in place: the copy is written back over the bundle the
# way the new one was written, read back by sha256, and removed once it reads back whole.
function PutOldBack($why) {
    if ($inPlace) {
        $back = $null
        try { $back = WriteOver $aside $target }
        catch {
            $text = $why.TrimEnd('.') + ". The add-in installed before could not be put back in place, " + (Why $_.Exception) + ". It is whole at " + $aside + ", read back by sha256 before anything was written over, what is at " + $target + " was not proved to be it, and the new one is at " + $staging + "."
            Write-Host $text
            return $text
        }
        $copyNow = "its copy beside it was removed"
        $notGone = RemoveCopy
        if ($null -ne $notGone) { $copyNow = "its copy is still at " + $aside + " and could not be removed, " + $notGone }
        $text = $why.TrimEnd('.') + ". The add-in installed before is at " + $target + ", put back in place with " + @($back.Wrote).Count + " written back and " + @($back.Removed).Count + " removed and read back by sha256 against its copy, " + $copyNow + ", and the new one that failed is at " + $staging + "."
        Write-Host $text
        return $text
    }
    $newAt = $null
    if (Test-Path -LiteralPath $target) {
        try { Remove-Item -LiteralPath $target -Recurse -Force -ErrorAction Stop }
        catch {
            Write-Host ("The new add-in could not be removed, " + $_.Exception.Message)
            $failedAt = $target + ".failed-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
            try { Rename-Item -LiteralPath $target -NewName (Split-Path $failedAt -Leaf) -ErrorAction Stop; $newAt = $failedAt }
            catch { Write-Host ("The new add-in could not be moved aside either, " + $_.Exception.Message); $newAt = $target }
        }
    }
    $oldAt = $aside
    if (-not (Test-Path -LiteralPath $target)) {
        try { Rename-Item -LiteralPath $aside -NewName (Split-Path $target -Leaf) -ErrorAction Stop; $oldAt = $target }
        catch { Write-Host ("The add-in installed before could not be put back, " + $_.Exception.Message) }
    }
    $text = $why.TrimEnd('.') + ". The add-in installed before is at " + $oldAt + ", and the new one that failed " + $(if ($null -eq $newAt) { "was removed" } else { "is at " + $newAt }) + "."
    Write-Host $text
    return $text
}

# The contents, never the folder. Copy-Item of a directory puts it INSIDE the destination
# when the destination already exists, and creates it when it does not, so the same line
# does two different things depending on whether the folder is there. That happened on
# 2026-08-31 and left ParsonsNwcFederator.bundle inside ParsonsNwcFederator.bundle, which
# Navisworks does not read at all. In place, each file is written over on its own.
$writes = $null
try {
    if ($inPlace) { $writes = WriteOver $staging $target }
    else {
        New-Item -ItemType Directory -Force -Path $target | Out-Null
        Copy-Item (Join-Path $staging "*") $target -Recurse -Force
    }
} catch {
    $copyError = $_.Exception.Message
    if ($null -ne $aside) { throw (PutOldBack ("The new add-in could not be copied in, " + $copyError)) }
    throw ("The new add-in could not be copied in, " + $copyError + ". It is at " + $target + ", and no add-in was installed before.")
}

try {
$nested = Join-Path $target (Split-Path -Leaf $target)
if (Test-Path $nested) {
    throw "The bundle ended up inside itself at '$nested'. Nothing was installed that Navisworks can read."
}

# Report only what is actually on disk.
$written = Get-ChildItem $target -Recurse -File | Sort-Object FullName
Write-Host "Installed to:"
Write-Host "  $target"
Write-Host ""
if ($inPlace) {
    Write-Host ("Replaced in place, {0} written over or added, and {1} removed that the new add-in does not have:" -f @($writes.Wrote).Count, @($writes.Removed).Count)
    foreach ($rel in $writes.Removed) { Write-Host ("  removed  " + $rel) }
    Write-Host ""
}
# In place, a file that already held the new bytes by sha256 is not written, so it is listed
# apart and never as written.
$kept = @()
Write-Host "Files written:"
foreach ($f in $written) {
    $rel = $f.FullName.Substring($target.Length + 1)
    if ($inPlace -and @($writes.Wrote) -notcontains $rel) { $kept += $f; continue }
    Write-Host ("  {0}  ({1} bytes)" -f $rel, $f.Length)
}
if ($kept.Count -gt 0) {
    Write-Host "Files not written, because the installed one already held the same bytes by sha256:"
    foreach ($f in $kept) { Write-Host ("  {0}  ({1} bytes)" -f $f.FullName.Substring($target.Length + 1), $f.Length) }
}
Write-Host ""

$expected = @("PackageContents.xml")
foreach ($name in ($carried + $library)) { $expected += (Join-Path "Contents\v22" $name) }
$missing = @()
foreach ($rel in $expected) {
    if (-not (Test-Path (Join-Path $target $rel))) { $missing += $rel }
}

if ($missing.Count -gt 0) {
    throw "Install incomplete. Missing: $($missing -join ', ')"
}

Write-Host ("All {0} expected files are present." -f $expected.Count)
Write-Host ""

# Every assembly the bundle references has to be in the bundle or in the framework.
# A run on aa163c9e got ninety seconds in and then failed to write the workbook, and
# a missing file and a version mismatch look identical from the outside. This walks
# what is actually in the folder rather than trusting the list above, so a package
# that gains a dependency is caught here rather than during a run.
Write-Host "Checking every assembly the bundle needs:"

$inBundle = @{}
foreach ($f in (Get-ChildItem $contents -Filter *.dll)) {
    $inBundle[[System.IO.Path]::GetFileNameWithoutExtension($f.Name)] = $f.FullName
}

$missing = @()
$mismatched = @()
$seen = @{}

foreach ($f in (Get-ChildItem $contents -Filter *.dll | Sort-Object Name)) {
    try { $asm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($f.FullName) }
    catch { continue }

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
    throw ("Install incomplete. {0} assembly reference(s) cannot be satisfied. Add the file(s) to the carried list in this script." -f $missing.Count)
}
} catch {
    # A check of the new bundle failed. The old one, whole since the move, goes back.
    if ($null -ne $aside) { throw (PutOldBack ("The new add-in failed its check, " + $_.Exception.Message)) }
    throw
}

Write-Host ("  every reference is satisfied, {0} assemblies checked. Navisworks supplies its own." -f $inBundle.Count)

# Every check has passed, so the add-in installed before is removed only now. When it cannot
# be, the one LEFT line names it, and run.ps1 -Mode Install writes it into its verdict.
if ($null -ne $aside) {
    try { Remove-Item -LiteralPath $aside -Recurse -Force -ErrorAction Stop }
    catch { Write-Host ("LEFT: the add-in installed before is at " + $aside + " and could not be removed, " + $_.Exception.Message + ". Whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN. Remove it once Navisworks is closed.") }
}

if ($mismatched.Count -gt 0) {
    Write-Host ""
    Write-Host ("  {0} reference(s) bind to a version the shipped file does not carry." -f $mismatched.Count)
    Write-Host "  These are NOT faults. There is no application config to put a binding"
  Write-Host "  redirect in, so the add-in resolves them by name from this folder."
    foreach ($line in $mismatched) { Write-Host $line }
}

Write-Host "Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator."
