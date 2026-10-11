<#
    Makes the team zip, the add-in and its one click install for a person with no compiler, from
    a clean checkout of main on the machine that builds it. Run only when Bader writes "make the
    team package", Q149. Nothing here needs admin rights. The zip is written only into the folder
    named on the command line, never inside the repo, and is never committed.

    Usage:
        powershell -ExecutionPolicy Bypass -File build\package.ps1 -OutputFolder "C:\somewhere\outside\the\repo"
        powershell -ExecutionPolicy Bypass -File build\package.ps1 -OutputFolder "..." -NavisworksPath "D:\Autodesk\Navisworks Manage 2025"

    It builds the solution with --no-incremental, then stages and checks the bundle by running
    build\install.ps1 -SkipBuild against a throwaway APPDATA, so the bundle is assembled and
    checked the one way install.ps1 does it, and zips what that run installed and checked, with
    the team install beside it. The version, the commit and the day are read off the stamp of
    the built add-in, never typed.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $OutputFolder,
    [string] $NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025"
)

$ErrorActionPreference = "Stop"

# How this script ends, written here once.
#
#   exit 0  the last line starts PACKAGED: and names the zip, its size, and what it holds
#   exit 1  the last line starts FAILED:. Something threw. No zip was written to the folder
#   exit 2  the last line starts REFUSED:. No zip was written to the folder. The line says why
$PackagedStart = "PACKAGED: "
$FailedExit    = 1
$FailedStart   = "FAILED: "
$RefusedExit   = 2
$RefusedStart  = "REFUSED: "

. (Join-Path $PSScriptRoot "install-common.ps1")

$repo     = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repo "ParsonsNwcFederator.sln"
$install  = Join-Path $PSScriptRoot "install.ps1"
$team     = Join-Path $PSScriptRoot "team"
$builtDll = Join-Path $repo "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll"
$ps       = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
$nothing  = "No zip was written."
$scratch  = $null

function Refuse($why) { ClearScratch; Write-Host ""; Write-Host ($RefusedStart + $why); exit $RefusedExit }

# The scratch folder under the temp folder, removed on every end once it was made. A removal
# that fails is said and the end it came from stands.
function ClearScratch {
    if ($null -eq $script:scratch -or -not (Test-Path -LiteralPath $script:scratch)) { return }
    $notGone = RemoveTree $script:scratch
    if ($null -ne $notGone) { Write-Host ("The scratch folder " + $script:scratch + " could not be removed, " + $notGone) }
}

trap {
    $why = Why $_.Exception
    ClearScratch
    Write-Host ""
    Write-Host ($FailedStart + $why + ". " + $nothing)
    exit $FailedExit
}

# git in the repo, its output as lines, a failure thrown with its exit code. Its standard error
# goes to the window as it is, since Windows PowerShell 5.1 turns a redirected line of it into
# an error.
function GitLines {
    $lines = @(& git -C $repo @args)
    if ($LASTEXITCODE -ne 0) { throw ("git " + ($args -join " ") + " failed with exit code " + $LASTEXITCODE) }
    return @($lines | ForEach-Object { [string]$_ })
}

Write-Host "Parsons NWC Federator team package"
Write-Host ("  repo        : " + $repo)
Write-Host ("  output      : " + $OutputFolder)
Write-Host ("  navisworks  : " + $NavisworksPath)
Write-Host ""

# Every refusal before the build writes nothing anywhere.
if (-not (Test-Path -LiteralPath $OutputFolder -PathType Container)) { Refuse ("the output folder " + $OutputFolder + " is not there. Make it, outside the repo, and run this again. " + $nothing) }
$out = (Get-Item -LiteralPath $OutputFolder -Force).FullName.TrimEnd('\')
$repoFull = (Get-Item -LiteralPath $repo -Force).FullName.TrimEnd('\')
if ($out -ieq $repoFull -or $out.StartsWith($repoFull + "\", [System.StringComparison]::OrdinalIgnoreCase)) { Refuse ("the output folder " + $out + " is inside the repo, and a zip is never committed. Name a folder outside it. " + $nothing) }
$branch = @(& git -C $repo symbolic-ref --quiet --short HEAD)
if ($LASTEXITCODE -ne 0 -or $branch.Count -ne 1 -or $branch[0] -ne "main") {
    $on = "a detached HEAD"
    if ($branch.Count -eq 1) { $on = "the branch " + $branch[0] }
    Refuse ("the checkout is on " + $on + ", and the team zip is made from main alone. Check out main, pull it, and run this again. " + $nothing)
}
$dirty = @(GitLines status --porcelain)
if ($dirty.Count -gt 0) { Refuse ("git status prints " + $dirty.Count + " line(s), the first " + $dirty[0].Trim() + ", and the team zip is made from a clean checkout of main alone. " + $nothing) }
$head = (GitLines rev-parse --short=8 HEAD)[0]

Write-Host ("Building main at " + $head + "...")
& dotnet build $solution -c Release --no-incremental -p:NavisworksPath="$NavisworksPath" --nologo -v minimal
if ($LASTEXITCODE -ne 0) { Refuse ("the build failed with exit code " + $LASTEXITCODE + ", its output above. " + $nothing) }
Write-Host ""

$stamp = StampOf $builtDll
$parts = StampParts $stamp
if ($null -eq $parts) { Refuse ("the built add-in reads the stamp " + $stamp + ", which is not in the shape the build writes. " + $nothing) }
if ($parts.Edits) { Refuse ("the built add-in reads the stamp " + $stamp + ", built from a tree with changes in it. " + $nothing) }
if ($parts.Commit -ne $head) { Refuse ("the built add-in reads the stamp " + $stamp + ", where main is at " + $head + ". " + $nothing) }
$name = "ParsonsNwcFederator-" + $parts.Version + "-" + $parts.Commit + "-" + $parts.Day
$zipAt = Join-Path $out ($name + ".zip")
if (Test-Path -LiteralPath $zipAt) { Refuse ($zipAt + " is there already, and a zip is never written over. " + $nothing) }

# install.ps1 stages and checks the bundle and installs it into this APPDATA, set for its one
# run and put back after, so the person's own add-in is never touched.
$script:scratch = Join-Path ([System.IO.Path]::GetTempPath()) ("ParsonsNwcFederator-package-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss"))
$appData = Join-Path $scratch "appdata"
New-Item -ItemType Directory -Path $appData | Out-Null
Write-Host ("Staging and checking the bundle with build\install.ps1 -SkipBuild, APPDATA " + $appData + "...")
$realAppData = $env:APPDATA
try {
    $env:APPDATA = $appData
    $lines = @(& $ps -NoProfile -ExecutionPolicy Bypass -File $install -SkipBuild -NavisworksPath $NavisworksPath)
    $installExit = $LASTEXITCODE
}
finally { $env:APPDATA = $realAppData }
foreach ($l in $lines) { Write-Host ("  | " + $l) }
$installLast = ""
$said = @($lines | Where-Object { ([string]$_).Trim() -ne "" })
if ($said.Count -gt 0) { $installLast = [string]$said[$said.Count - 1] }
if ($installExit -ne 0) { Refuse ("build\install.ps1 ended with exit " + $installExit + " and the line " + $installLast + " " + $nothing) }
$checked = Join-Path $appData "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
$checkedStamp = StampOf (Join-Path $checked "Contents\v22\Federator.Addin.dll")
if ($checkedStamp -ne $stamp) { Refuse ("the bundle install.ps1 checked reads the stamp " + $checkedStamp + " where the build wrote " + $stamp + ". " + $nothing) }
Write-Host ""

# What goes in the zip, laid out in the scratch folder: the checked bundle, the team install
# and the shared rules it dot-sources, the two .cmd files, the page, and VERSION.txt.
$lay = Join-Path $scratch $name
New-Item -ItemType Directory -Path $lay | Out-Null
Copy-Item -LiteralPath $checked -Destination (Join-Path $lay "ParsonsNwcFederator.bundle") -Recurse
foreach ($f in @((Join-Path $PSScriptRoot "team-install.ps1"), (Join-Path $PSScriptRoot "install-common.ps1"))) { Copy-Item -LiteralPath $f -Destination $lay }
foreach ($f in @(Get-ChildItem -LiteralPath $team -File)) { Copy-Item -LiteralPath $f.FullName -Destination $lay }
$version = @(
    "Parsons NWC Federator",
    ("version : " + $parts.Version),
    ("commit  : " + $parts.Commit),
    ("date    : " + $parts.Day),
    ("stamp   : " + $stamp)
)
[System.IO.File]::WriteAllLines((Join-Path $lay "VERSION.txt"), [string[]]$version)

# The zip is written in the scratch folder, read back entry by entry against what was laid out,
# and only then copied to the output folder and read back there by its SHA256.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipScratch = Join-Path $scratch ($name + ".zip")
[System.IO.Compression.ZipFile]::CreateFromDirectory($lay, $zipScratch, [System.IO.Compression.CompressionLevel]::Optimal, $false)
$laid = @{}
foreach ($f in @(Get-ChildItem -LiteralPath $lay -Recurse -File -Force)) { $laid[$f.FullName.Substring($lay.Length + 1).Replace('\', '/')] = $f.Length }
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipScratch)
try {
    $entries = @($zip.Entries | Where-Object { -not $_.FullName.EndsWith("/") })
    foreach ($e in $entries) {
        if (-not $laid.ContainsKey($e.FullName)) { throw ("the zip holds " + $e.FullName + ", which was not laid out") }
        if ($laid[$e.FullName] -ne $e.Length) { throw ("the zip holds " + $e.FullName + " at " + $e.Length + " bytes where the file is " + $laid[$e.FullName]) }
    }
    if ($entries.Count -ne $laid.Count) { throw ("the zip holds " + $entries.Count + " files where " + $laid.Count + " were laid out") }
}
finally { $zip.Dispose() }
Copy-Item -LiteralPath $zipScratch -Destination $zipAt
if ((Get-FileHash -LiteralPath $zipAt -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $zipScratch -Algorithm SHA256).Hash) { throw ("the zip copied to " + $zipAt + " does not read the same as the one made, and it is left there") }
$size = (Get-Item -LiteralPath $zipAt).Length
ClearScratch

Write-Host "In the zip:"
foreach ($k in @($laid.Keys | Sort-Object)) { Write-Host ("  " + $k + "  (" + $laid[$k] + " bytes)") }
Write-Host ""
Write-Host ($PackagedStart + $zipAt + ", " + $size + " bytes, " + $laid.Count + " files, version " + $parts.Version + ", commit " + $parts.Commit + ", built " + $parts.Built + ". It is not committed. Hand it to the team only when Bader says so.")
exit 0
