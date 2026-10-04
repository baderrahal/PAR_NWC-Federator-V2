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

# The one rule of what is a junction or a link, which tools\loop\prepare-copy.ps1 reads too.
. (Join-Path $PSScriptRoot "links.ps1")

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
        if ($null -ne (LinkKind (Get-Item -LiteralPath $walkUp -Force))) { Write-Host ("REFUSED: " + $walkUp + " is a junction or a link, so the add-in is not installed through it. Nothing was installed."); exit 2 }
    }
    if ($walkUp.TrimEnd('\') -ieq $env:APPDATA.TrimEnd('\')) { break }
    $walkUp = Split-Path $walkUp -Parent
}

# The sha256 of one file, read sharing read, write and delete.
function Sha($path) {
    $stream = New-Object System.IO.FileStream($path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]"ReadWrite, Delete")
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash($stream)).Replace("-", "") }
    finally { $sha.Dispose(); $stream.Dispose() }
}

# Every file under $dir by its path below $dir, with its sha256, every folder below it, and
# every file marked ReadOnly, Hidden or System and every folder marked ReadOnly, named with
# its marks. It reads one folder at a time and refuses a junction or a link, by the rule in
# build\links.ps1, before it reads anything beneath it, because what is removed through one is
# what it points at. A file that cannot be read is named.
function Listing($dir) {
    $root = (Get-Item -LiteralPath $dir -Force).FullName.TrimEnd('\')
    $files = @{}
    $folders = @()
    $marked = @()
    $todo = New-Object System.Collections.Generic.Stack[string]
    $todo.Push($root)
    while ($todo.Count -gt 0) {
        foreach ($item in @(Get-ChildItem -LiteralPath $todo.Pop() -Force)) {
            $rel = $item.FullName.Substring($root.Length + 1)
            if ($null -ne (LinkKind $item)) { throw ($rel + " is a junction or a link, and nothing is written or removed through one") }
            $marks = $item.Attributes -band [System.IO.FileAttributes]"ReadOnly, Hidden, System"
            if ($item.PSIsContainer) { $marks = $item.Attributes -band [System.IO.FileAttributes]::ReadOnly }
            if ($marks -ne 0) { $marked += ($rel + " marked " + $marks.ToString().Replace(", ", " and ")) }
            if ($item.PSIsContainer) { $folders += $rel; $todo.Push($item.FullName); continue }
            try { $files[$rel] = Sha $item.FullName } catch { throw ($rel + " could not be read, " + (Why $_.Exception)) }
        }
    }
    return [pscustomobject]@{ Files = $files; Folders = $folders; Marked = @($marked | Sort-Object) }
}

# The files and folders where the listing $now differs from the listing $want, by sha256 or by
# being in one and not the other, sorted.
function Differ($want, $now) {
    $d = @($want.Files.Keys | Where-Object { -not $now.Files.ContainsKey($_) -or $now.Files[$_] -ne $want.Files[$_] }) + @($now.Files.Keys | Where-Object { -not $want.Files.ContainsKey($_) }) + @($want.Folders | Where-Object { $now.Folders -notcontains $_ }) + @($now.Folders | Where-Object { $want.Folders -notcontains $_ })
    return ,@($d | Sort-Object)
}

# Makes the folder $to hold exactly the files and folders of $want, the listing of $from taken
# before, and returns the files it wrote and the files it removed. First each file of $from it
# is to write is read against $want by sha256, and when one no longer reads the same, or
# cannot be read, nothing is written. Then every folder of $want is made, each file is written
# over the one at the same place in $to unless that one already reads the same by sha256, and
# every file and every folder $want does not have is removed, the deepest first. Last, $to is
# read back against $want by sha256. Each file is opened sharing read, write and delete, so a
# reader holding it open with every share mode stops neither the write nor the removal,
# measured on 2026-10-01 and 2026-10-04. The first failure throws, naming the file.
function WriteOver($from, $want, $to) {
    $have = [pscustomobject]@{ Files = @{}; Folders = @(); Marked = @() }
    if (Test-Path -LiteralPath $to) { $have = Listing $to }
    $todo = @($want.Files.Keys | Where-Object { -not ($have.Files.ContainsKey($_) -and $have.Files[$_] -eq $want.Files[$_]) } | Sort-Object)
    foreach ($rel in $todo) {
        $now = $null
        try { $now = Sha (Join-Path $from $rel) } catch { throw ($rel + " could not be read at " + $from + ", " + (Why $_.Exception) + ", so nothing was written") }
        if ($now -ne $want.Files[$rel]) { throw ($rel + " at " + $from + " no longer reads by sha256 as it did when it was listed, so nothing was written") }
    }
    $wrote = @()
    $removed = @()
    New-Item -ItemType Directory -Force -Path $to | Out-Null
    foreach ($rel in $want.Folders) { New-Item -ItemType Directory -Force -Path (Join-Path $to $rel) | Out-Null }
    foreach ($rel in $todo) {
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
    $differ = Differ $want (Listing $to)
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
# When the rename is refused, the process list is read again. A process named Roamer, or a
# list that cannot be read, refuses as before. With none the bundle is replaced in place. On
# 2026-10-01 Windows refused that rename on Bader's machine three times with no Navisworks
# running, while every file of the bundle opened for delete and for write with every share
# mode, and what holds it is UNKNOWN. Measured on 2026-10-01 and again on 2026-10-04 in a
# throwaway folder: one file beneath a folder held open by another process refuses the
# rename of the folder with the same words, access to the path is denied, whatever the share
# mode, and a file held that way sharing read, write and delete can still be written over and
# removed. Measured on 2026-10-04 as well: Windows refuses to write over a file marked
# ReadOnly, Hidden or System and to remove a file or folder marked ReadOnly, with those same
# words, where the rename and Remove-Item -Force take every mark, and the Copy-Item below
# carries a file's marks into the bundle and not a folder's. So a bundle that carries such a
# mark is refused, each marked file and folder named.
#
# The installed bundle is first listed by sha256, a junction or a link inside it refused,
# then copied beside it under the name the rename would have given it and read back against
# that listing, and only then is each new file written over the old one. On any failure after
# that the copy is written back over the bundle against that same listing, never against what
# the copy holds by then, and read back, so a whole copy of the add-in installed before is on
# the disk at every moment. A stop part way, a closed window or a machine that goes off,
# leaves the bundle with some new files and some old ones and the copy whole beside it, which
# the IN PLACE line says, with how to finish or undo it.
$aside = $null
$inPlace = $false
$old = $null

# Removes the folder at $aside, the old bundle moved aside or its copy made beside it, and
# returns null once it is gone, or the words of why it could not be removed.
function RemoveCopy {
    try { Remove-Item -LiteralPath $aside -Recurse -Force -ErrorAction Stop; return $null } catch { return (Why $_.Exception) }
}

if (Test-Path -LiteralPath $target) {
    $aside = $target + ".replaced-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
    try { Rename-Item -LiteralPath $target -NewName (Split-Path $aside -Leaf) -ErrorAction Stop }
    catch {
        $moveError = Why $_.Exception
        $leftWhole = ". The installed add-in is left whole. Nothing was installed."
        $navisworks = NavisworksRefusal
        if ($null -ne $navisworks) { Write-Host ("REFUSED: the installed add-in could not be moved aside, " + $moveError + ", and " + $navisworks + $leftWhole); exit 2 }
        $noRoamer = "REFUSED: the installed add-in could not be moved aside, " + $moveError + ", no process named Roamer runs, and "
        if (Test-Path -LiteralPath $aside) { Write-Host ($noRoamer + $aside + " is there already, so no copy of it is made there" + $leftWhole); exit 2 }
        try { $old = Listing $target } catch { Write-Host ($noRoamer + "it is not replaced in place, " + (Why $_.Exception) + $leftWhole); exit 2 }
        if ($old.Marked.Count -gt 0) { Write-Host ($noRoamer + "it is not replaced in place, because Windows refuses to write over a file marked ReadOnly, Hidden or System and to remove a file or folder marked ReadOnly, measured on 2026-10-04, and " + $old.Marked.Count + " of its files and folders carry such a mark, " + ($old.Marked -join ", ") + ". Take each mark off with attrib -r -h -s and run this again" + $leftWhole); exit 2 }
        try { [void](WriteOver $target $old $aside) }
        catch {
            $copyError = Why $_.Exception
            $made = "nothing of it was made"
            if (Test-Path -LiteralPath $aside) {
                $notGone = RemoveCopy
                if ($null -eq $notGone) { $made = "what was made of it was removed" }
                else { $made = "what was made of it is at " + $aside + " and could not be removed, " + $notGone }
            }
            Write-Host ($noRoamer + "its copy beside it could not be made whole, " + $copyError + ", and " + $made + $leftWhole)
            exit 2
        }
        Write-Host ("IN PLACE: the installed add-in could not be moved aside, " + $moveError + ", and no process named Roamer runs, so its files are replaced where they are. A copy of it was made first at " + $aside + " and read back by sha256, and it is kept until every check of the new one has passed. If this stops before its last line, the add-in may hold some new files and some old ones, so start no Navisworks, and run this again to finish, or make the add-in hold exactly the files of that copy to undo.")
        $inPlace = $true
    }
}

# After a failure once the old bundle was moved aside or copied beside it, the old one is put
# back and where each thing ends up is printed and returned for the failure's message. Moved
# aside: the new one is removed, or moved aside when it will not go, and the old one is put
# back when its place is free. Replaced in place: the copy is written back over the bundle the
# way the new one was written, against the listing of the bundle taken before anything was
# written over, read back against that listing, and removed once it reads back whole. When the
# put back fails, the copy is read again against that listing, so the words say whether it is
# still whole.
function PutOldBack($why) {
    if ($inPlace) {
        $back = $null
        try { $back = WriteOver $aside $old $target }
        catch {
            $putError = Why $_.Exception
            $copyNow = "Its copy at " + $aside + " reads as the bundle did before anything was written over, by sha256"
            try {
                $changed = Differ $old (Listing $aside)
                if ($changed.Count -gt 0) { $copyNow = "Its copy at " + $aside + " no longer reads as the bundle did before anything was written over, " + $changed.Count + " differ by sha256, the first " + $changed[0] }
            } catch { $copyNow = "Its copy at " + $aside + " could not be read, " + (Why $_.Exception) }
            $text = $why.TrimEnd('.') + ". The add-in installed before could not be put back in place, " + $putError + ". " + $copyNow + ", what is at " + $target + " was not proved to be it, and the new one is at " + $staging + "."
            Write-Host $text
            return $text
        }
        $copyNow = "its copy beside it was removed"
        $notGone = RemoveCopy
        if ($null -ne $notGone) { $copyNow = "its copy is still at " + $aside + " and could not be removed, " + $notGone }
        $text = $why.TrimEnd('.') + ". The add-in installed before is at " + $target + ", put back in place with " + @($back.Wrote).Count + " written back and " + @($back.Removed).Count + " removed, and read back by sha256 against the bundle as it read before anything was written over, " + $copyNow + ", and the new one that failed is at " + $staging + "."
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
    if ($inPlace) { $writes = WriteOver $staging (Listing $staging) $target }
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

# Every check has passed, so the add-in installed before, moved aside or copied beside it, is
# removed only now. When it cannot be, the one LEFT line names it, and run.ps1 -Mode Install
# writes it into its verdict.
if ($null -ne $aside) {
    $leftWhy = RemoveCopy
    if ($null -ne $leftWhy) { Write-Host ("LEFT: the add-in installed before is at " + $aside + " and could not be removed, " + $leftWhy + ". Whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN. Remove it once Navisworks is closed.") }
}

if ($mismatched.Count -gt 0) {
    Write-Host ""
    Write-Host ("  {0} reference(s) bind to a version the shipped file does not carry." -f $mismatched.Count)
    Write-Host "  These are NOT faults. There is no application config to put a binding"
  Write-Host "  redirect in, so the add-in resolves them by name from this folder."
    foreach ($line in $mismatched) { Write-Host $line }
}

Write-Host "Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator."
