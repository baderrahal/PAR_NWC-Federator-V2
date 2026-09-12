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

if (Test-Path $target) { Remove-Item $target -Recurse -Force }

# The contents, never the folder. Copy-Item of a directory puts it INSIDE the destination
# when the destination already exists, and creates it when it does not, so the same line
# does two different things depending on whether the remove above has finished. That
# happened on 2026-08-31 and left ParsonsNwcFederator.bundle inside
# ParsonsNwcFederator.bundle, which Navisworks does not read at all.
New-Item -ItemType Directory -Force -Path $target | Out-Null
Copy-Item (Join-Path $staging "*") $target -Recurse -Force

$nested = Join-Path $target (Split-Path -Leaf $target)
if (Test-Path $nested) {
    throw "The bundle ended up inside itself at '$nested'. Nothing was installed that Navisworks can read."
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

Write-Host ("  every reference is satisfied, {0} assemblies checked. Navisworks supplies its own." -f $inBundle.Count)

if ($mismatched.Count -gt 0) {
    Write-Host ""
    Write-Host ("  {0} reference(s) bind to a version the shipped file does not carry." -f $mismatched.Count)
    Write-Host "  These are NOT faults. There is no application config to put a binding"
  Write-Host "  redirect in, so the add-in resolves them by name from this folder."
    foreach ($line in $mismatched) { Write-Host $line }
}

Write-Host "Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator."
