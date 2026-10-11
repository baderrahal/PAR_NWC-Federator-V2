<#
    The install for a person on the team, carried inside the team zip that build\package.ps1
    makes, and started by one of the two .cmd files beside it there, never typed. It copies the
    add-in that sits beside it in the zip into the person's own plugin folder, with no admin
    rights and nothing written outside the person's profile, and keeps the version installed
    before at ParsonsNwcFederator.bundle.previous, the one backup, so one step puts it back.

        Install Parsons NWC Federator.cmd     installs the add-in beside this script
        Put back the previous version.cmd     runs this with -PutBack
#>

[CmdletBinding()]
param(
    [switch] $PutBack
)

$ErrorActionPreference = "Stop"

# How this script ends, written here once and read off here by build\team\INSTALL-TEAM.txt.
#
#   exit 0  the last line starts DONE:. It says which version is installed and where, and
#           where the version installed before is kept
#   exit 1  the last line starts FAILED:. Something stopped part way. The line says where each
#           version is and what a person does
#   exit 2  the last line starts REFUSED:. Nothing was changed. The line says why and what a
#           person does
$DoneExit     = 0
$DoneStart    = "DONE: "
$FailedExit   = 1
$FailedStart  = "FAILED: "
$RefusedExit  = 2
$RefusedStart = "REFUSED: "
$StartLine    = "Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator."

. (Join-Path $PSScriptRoot "install-common.ps1")

$source   = Join-Path $PSScriptRoot "ParsonsNwcFederator.bundle"
$previous = $target + ".previous"

# A file beside the add-in, never inside it, that stands while a copy into the place
# Navisworks loads from is under way and is removed once that copy reads back whole. A run
# that finds it knows the add-in there is what a stopped run left, so it never keeps that one
# as the previous version.
$copying = $target + ".copying"

function Finish($code, $line) { Write-Host ""; Write-Host $line; exit $code }
function Refuse($why) { Finish $RefusedExit ($RefusedStart + $why) }
function Fail($why) { Finish $FailedExit ($FailedStart + $why) }

# The version a bundle folder holds, in words, read off the stamp of its Federator.Addin.dll.
function VersionAt($dir) {
    $dll = Join-Path $dir "Contents\v22\Federator.Addin.dll"
    if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) { return "a version whose stamp could not be read" }
    $parts = StampParts (StampOf $dll)
    if ($null -eq $parts) { return ("a version stamped " + (StampOf $dll)) }
    return ("version " + $parts.Version + ", commit " + $parts.Commit + ", built " + $parts.Built)
}

# Every file under $dir by its path below it, the key read without regard to case.
function FilesUnder($dir) {
    $files = @{}
    foreach ($f in @(Get-ChildItem -LiteralPath $dir -Recurse -File -Force)) { $files[$f.FullName.Substring($dir.TrimEnd('\').Length + 1)] = $f.FullName }
    return $files
}

function SameFile($a, $b) {
    if ((Get-Item -LiteralPath $a -Force).Length -ne (Get-Item -LiteralPath $b -Force).Length) { return $false }
    return (Get-FileHash -LiteralPath $a -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath $b -Algorithm SHA256).Hash
}

# Makes $to hold exactly the files of $from. A file missing or different is copied, a file $to
# holds that $from does not is removed, and a folder $from does not have is removed once empty.
# A file that came from the internet carries a mark, which a copy keeps, and the mark is taken
# off every file in $to. Whether Navisworks loads a marked DLL is UNKNOWN, and Roamer.exe.config
# names no loadFromRemoteSources, read on 2026-10-11. Then every file is read back against
# $from by its SHA256. Returns null when $to reads equal to $from, or the words of why not.
# Neither folder may hold a junction or a link, which the caller reads first.
function SyncBundle($from, $to) {
    try {
        $want = FilesUnder $from
        if (-not (Test-Path -LiteralPath $to)) { New-Item -ItemType Directory -Path $to | Out-Null }
        foreach ($rel in @($want.Keys | Sort-Object)) {
            $dest = Join-Path $to $rel
            if ((Test-Path -LiteralPath $dest -PathType Leaf) -and (SameFile $want[$rel] $dest)) { continue }
            New-Item -ItemType Directory -Force -Path (Split-Path $dest -Parent) | Out-Null
            Copy-Item -LiteralPath $want[$rel] -Destination $dest -Force
        }
        $have = FilesUnder $to
        foreach ($rel in @($have.Keys | Sort-Object)) { if (-not $want.ContainsKey($rel)) { Remove-Item -LiteralPath $have[$rel] -Force } }
        $dirs = @(Get-ChildItem -LiteralPath $to -Recurse -Directory -Force | Sort-Object { $_.FullName.Length } -Descending)
        foreach ($d in $dirs) {
            if (-not (Test-Path -LiteralPath (Join-Path $from $d.FullName.Substring($to.TrimEnd('\').Length + 1)) -PathType Container)) { Remove-Item -LiteralPath $d.FullName -Force }
        }
        Get-ChildItem -LiteralPath $to -Recurse -File -Force | Unblock-File
        $have = FilesUnder $to
        foreach ($rel in @($want.Keys | Sort-Object)) {
            if (-not $have.ContainsKey($rel)) { return ($rel + " is missing from " + $to + " after the copy") }
            if (-not (SameFile $want[$rel] $have[$rel])) { return ($rel + " in " + $to + " does not read the same as in " + $from + " after the copy") }
        }
        if ($have.Count -ne $want.Count) { return ($to + " holds " + $have.Count + " files after the copy where " + $from + " holds " + $want.Count) }
    }
    catch { return (Why $_.Exception) }
    return $null
}

# Whatever throws past the steps below ends here, as one FAILED line.
trap {
    Fail ((Why $_.Exception) + ". What is at " + $target + " was not read after it. Close Navisworks and run this again, or double click Put back the previous version.cmd")
}

$what = "Install"
if ($PutBack) { $what = "Put back the previous version" }
Write-Host ("Parsons NWC Federator, " + $what)
Write-Host ("  add-in folder : " + $target)
Write-Host ("  kept before   : " + $previous)
Write-Host ""

# Nothing changes while Navisworks runs, through a junction or a link, or from a bundle that is
# not whole, and each of these refuses before anything is written.
$nothing = "Nothing was changed."
$onTheWay = LinkOnTheWay $target
if ($null -ne $onTheWay) { Refuse ($onTheWay + ". " + $nothing) }
$navisworks = NavisworksRefusal
if ($null -ne $navisworks) { Refuse ($navisworks + ". " + $nothing) }
$read = @($target, $previous)
if (-not $PutBack) { $read += $source }
foreach ($dir in $read) {
    $links = @(LinksAt $dir)
    if ($links.Count -gt 0) { Refuse ($dir + " holds a junction or a link, " + ($links -join ", ") + ", and nothing is removed or replaced through one. " + $nothing + " Send this window to whoever sent you the zip") }
}

if ($PutBack) {
    if (-not (Test-Path -LiteralPath $previous -PathType Container)) { Refuse ("no previous version is kept at " + $previous + ", so there is nothing to put back. " + $nothing) }
    $gone = @(MissingIn $previous)
    if ($gone.Count -gt 0) { Refuse ("the previous version kept at " + $previous + " is not whole, " + $gone.Count + " of its " + $expected.Count + " files missing, the first " + $gone[0] + ", so it was not put back. " + $nothing) }
    $was = "No version was installed"
    if (Test-Path -LiteralPath $target) { $was = "The version that was installed, " + (VersionAt $target) + ", was replaced and is not kept. To have it again, double click Install Parsons NWC Federator.cmd in the folder of its zip" }
    New-Item -ItemType Directory -Force -Path $plugins | Out-Null
    Set-Content -LiteralPath $copying -Value "A put back of the previous version is copying into the add-in folder."
    $notSame = SyncBundle $previous $target
    if ($null -ne $notSame) { Fail ("the previous version could not be put back whole, " + $notSame + ". " + $target + " may hold part of two versions, and the previous version is still whole at " + $previous + ". Close Navisworks and double click Put back the previous version.cmd again") }
    Remove-Item -LiteralPath $copying -Force
    Write-Host ("Put back : " + (VersionAt $target))
    Write-Host ("Where    : " + $target + ", every file read back the same as the copy kept at " + $previous)
    Finish $DoneExit ($DoneStart + "the previous version, " + (VersionAt $target) + ", is back at " + $target + ". " + $was + ". " + $StartLine)
}

if (-not (Test-Path -LiteralPath $source -PathType Container)) { Refuse ("there is no ParsonsNwcFederator.bundle folder beside this script at " + $source + ". Extract the whole zip and double click Install Parsons NWC Federator.cmd in the folder that makes. " + $nothing) }
$gone = @(MissingIn $source)
if ($gone.Count -gt 0) { Refuse ("the add-in in this zip at " + $source + " is not whole, " + $gone.Count + " of its " + $expected.Count + " files missing, the first " + $gone[0] + ". Extract the whole zip again. " + $nothing) }
Write-Host ("In zip   : " + (VersionAt $source))

# The version installed now is kept at .previous, the one backup, an older backup replaced, but
# only when it is a whole add-in that a stopped run did not leave and that is not the very build
# being installed. Otherwise the backup is left as it is, so a good previous version is never
# pushed out by a broken or an identical one.
$kept = $null
$keptWords = "No version was installed before, so none is kept"
if (Test-Path -LiteralPath $previous) { $keptWords = "No version was installed before, and the one kept at " + $previous + " was left as it was" }
if (Test-Path -LiteralPath $target) {
    $installedGone = @(MissingIn $target)
    $sameBuild = $false
    if ($installedGone.Count -eq 0) { $sameBuild = (StampOf (Join-Path $source "Contents\v22\Federator.Addin.dll")) -eq (StampOf (Join-Path $target "Contents\v22\Federator.Addin.dll")) }
    $leftOver = ", and none is kept"
    if (Test-Path -LiteralPath $previous) { $leftOver = ", and the one kept at " + $previous + " was left as it was" }
    if (Test-Path -LiteralPath $copying) { $keptWords = "The add-in at " + $target + " is what an install or a put back that stopped part way left, so it was not kept" + $leftOver }
    elseif ($installedGone.Count -gt 0) { $keptWords = "The add-in at " + $target + " was not whole, " + $installedGone.Count + " of its " + $expected.Count + " files missing, so it was not kept" + $leftOver }
    elseif ($sameBuild) { $keptWords = "The same build was installed already, so it was not kept again" + $leftOver }
    else {
        $installedWas = VersionAt $target
        if (Test-Path -LiteralPath $previous) {
            $older = VersionAt $previous
            $notGone = RemoveTree $previous
            if ($null -ne $notGone) { Fail ("the older version kept at " + $previous + ", " + $older + ", could not be removed to make room, " + $notGone + ". The version installed, " + $installedWas + ", is unchanged at " + $target + ". Nothing was installed. Close Navisworks and run this again") }
            Write-Host ("Removed  : the older version kept at " + $previous + ", " + $older)
        }
        $notSame = SyncBundle $target $previous
        if ($null -ne $notSame) { Fail ("the version installed could not be kept at " + $previous + ", " + $notSame + ". The version installed, " + $installedWas + ", is unchanged at " + $target + ", and what was copied of it is at " + $previous + ". Nothing was installed. Close Navisworks and run this again") }
        $kept = $previous
        $keptWords = "The version installed before, " + $installedWas + ", is kept at " + $previous
        Write-Host ("Kept     : " + $installedWas + ", at " + $previous + ", every file read back")
    }
}

New-Item -ItemType Directory -Force -Path $plugins | Out-Null
Set-Content -LiteralPath $copying -Value "An install is copying into the add-in folder."
$notSame = SyncBundle $source $target
if ($null -ne $notSame) {
    if ($null -eq $kept) {
        if (-not (Test-Path -LiteralPath $previous)) {
            $notGone = RemoveTree $target
            if ($null -eq $notGone) { Remove-Item -LiteralPath $copying -Force; Fail ("the new version could not be copied in, " + $notSame + ". What was copied was removed, so no version is installed. Close Navisworks and run this again") }
            Fail ("the new version could not be copied in, " + $notSame + ", and what was copied could not be removed, " + $notGone + ". " + $target + " holds part of it. Close Navisworks, delete that folder and run this again")
        }
        Fail ("the new version could not be copied in, " + $notSame + ". " + $target + " is not whole. " + $keptWords + ". Close Navisworks and run this again, or double click Put back the previous version.cmd")
    }
    $notBack = SyncBundle $kept $target
    if ($null -eq $notBack) { Remove-Item -LiteralPath $copying -Force; Fail ("the new version could not be copied in, " + $notSame + ". The version installed before is back at " + $target + ", every file read back the same as the copy kept at " + $kept + ". Close Navisworks and run this again") }
    Fail ("the new version could not be copied in, " + $notSame + ", and the version installed before could not be put back, " + $notBack + ". " + $target + " may hold part of two versions, and the version installed before is whole at " + $kept + ". Close Navisworks and double click Put back the previous version.cmd")
}
Remove-Item -LiteralPath $copying -Force

$installedVersion = VersionAt $target
Write-Host ("Installed: " + $installedVersion)
Write-Host ("Where    : " + $target + ", every file read back the same as in the zip")
Finish $DoneExit ($DoneStart + "Parsons NWC Federator " + $installedVersion + ", is installed at " + $target + ". " + $keptWords + ". " + $StartLine)
