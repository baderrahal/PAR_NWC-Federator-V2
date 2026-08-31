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
# with a file the Navisworks install ships, see docs\scan.md section 4h.
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
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
Copy-Item $staging $target -Recurse -Force

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
Write-Host "Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator."
