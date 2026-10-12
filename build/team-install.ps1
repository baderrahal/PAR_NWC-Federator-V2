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

# How this script ends, written here once. Each end is one exit code and one last line starting
# with its word. build\team\INSTALL-TEAM.txt carries the three lines below word for word, from
# the word on, for a person to read, and the proof of F140 reads them off both.
#
#   exit 0  DONE:     It worked. The line says which version is installed and where, and where the version you had before is kept.
#   exit 1  FAILED:   It stopped part way. The line says what is in each folder and what to do.
#   exit 2  REFUSED:  Nothing was changed. The line says why and what to do.
$DoneExit     = 0
$DoneStart    = "DONE: "
$FailedExit   = 1
$FailedStart  = "FAILED: "
$RefusedExit  = 2
$RefusedStart = "REFUSED: "

. (Join-Path $PSScriptRoot "install-common.ps1")

$source   = Join-Path $PSScriptRoot "ParsonsNwcFederator.bundle"
$previous = $target + ".previous"

# A file beside the add-in, never inside it, that stands while a copy into the place
# Navisworks loads from is under way and is removed once that copy reads back whole. A run
# that finds it knows the add-in there is what a stopped run left, so it never keeps that one
# as the previous version.
$copying = $target + ".copying"

# Where a put back copies the add-in installed aside before it writes over it, so a put back
# that fails part way puts that one back. Its name does not end in .bundle.
$hold = $target + ".before-put-back"

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

# What the folder $dir holds, in words, read now: nothing, an add-in not whole, or a version
# whole by its file list. A read that throws is said with why.
function HoldsNow($dir) {
    try {
        if (-not (Test-Path -LiteralPath $dir)) { return ($dir + " holds nothing") }
        $gone = @(MissingIn $dir)
        if ($gone.Count -gt 0) { return ($dir + " holds an add-in that is not whole, " + $gone.Count + " of its " + $expected.Count + " files missing") }
        return ($dir + " holds " + (VersionAt $dir) + ", whole")
    }
    catch { return ($dir + " could not be read, " + (Why $_.Exception)) }
}

# What a person does after a failure: run this again, and put back where a whole version is kept.
function Advice {
    if ((Test-Path -LiteralPath $previous) -and @(MissingIn $previous).Count -eq 0) { return "Close Navisworks and run this again, or double click Put back the previous version.cmd" }
    return "Close Navisworks and run this again"
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

# Null when every file of $from reads the same in $to by its SHA256, or the words of the first
# that does not. A file $to holds that $from does not is not read.
function FromReadsSameIn($from, $to) {
    $want = FilesUnder $from
    $have = FilesUnder $to
    foreach ($rel in @($want.Keys | Sort-Object)) {
        if (-not $have.ContainsKey($rel)) { return ($rel + " is missing from " + $to) }
        if (-not (SameFile $want[$rel] $have[$rel])) { return ($rel + " in " + $to + " does not read the same as in " + $from) }
    }
    return $null
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
        $notSame = FromReadsSameIn $from $to
        if ($null -ne $notSame) { return ($notSame + " after the copy") }
        $count = (FilesUnder $to).Count
        if ($count -ne $want.Count) { return ($to + " holds " + $count + " files after the copy where " + $from + " holds " + $want.Count) }
    }
    catch { return (Why $_.Exception) }
    return $null
}

# Whatever throws past the steps below ends here, as one FAILED line saying what each folder
# holds when it ends.
trap {
    $why = Why $_.Exception
    $now = (HoldsNow $target) + ". " + (HoldsNow $previous) + "."
    if (Test-Path -LiteralPath $hold) { $now += " " + (HoldsNow $hold) + "." }
    Fail ($why + ". " + $now + " " + (Advice))
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
$read = @($target, $previous, $hold)
if (-not $PutBack) { $read += $source }
foreach ($dir in $read) {
    $links = @(LinksAt $dir)
    if ($links.Count -gt 0) { Refuse ($dir + " holds a junction or a link, " + ($links -join ", ") + ", and nothing is removed or replaced through one. " + $nothing + " Send this window to whoever sent you the zip") }
}

if ($PutBack) {
    if (-not (Test-Path -LiteralPath $previous -PathType Container)) { Refuse ("no previous version is kept at " + $previous + ", so there is nothing to put back. " + $nothing) }
    $gone = @(MissingIn $previous)
    if ($gone.Count -gt 0) { Refuse ("the previous version kept at " + $previous + " is not whole, " + $gone.Count + " of its " + $expected.Count + " files missing, the first " + $gone[0] + ", so it was not put back. " + $nothing) }

    # The add-in installed is copied aside first and read back, so a put back that fails part
    # way can put it back. A copy left by an earlier put back is removed to make room.
    $aside = $null
    $was = "No version was installed"
    if (Test-Path -LiteralPath $target) {
        $wasVersion = VersionAt $target
        if (Test-Path -LiteralPath $hold) {
            $notGone = RemoveTree $hold
            if ($null -ne $notGone) { Fail ("a copy an earlier put back left at " + $hold + " could not be removed, " + $notGone + ". The add-in at " + $target + " is unchanged and nothing was put back. Close Navisworks and run this again") }
        }
        $notSame = SyncBundle $target $hold
        if ($null -ne $notSame) {
            $notGone = RemoveTree $hold
            $holdWords = "what was copied of it was removed"
            if ($null -ne $notGone) { $holdWords = "what was copied of it is left at " + $hold + ", " + $notGone }
            Fail ("the version installed could not be copied aside before the put back, " + $notSame + ", and " + $holdWords + ". The add-in at " + $target + " is unchanged and nothing was put back. Close Navisworks and run this again")
        }
        $aside = $hold
        $was = "The version that was installed, " + $wasVersion + ", was replaced and is not kept. To have it again, double click Install Parsons NWC Federator.cmd in the folder of its zip"
        Write-Host ("Aside    : " + $wasVersion + ", at " + $hold + ", every file read back")
    }
    New-Item -ItemType Directory -Force -Path $plugins | Out-Null
    Set-Content -LiteralPath $copying -Value "A put back of the previous version is copying into the add-in folder."
    $notSame = SyncBundle $previous $target
    if ($null -ne $notSame) {
        $stillKept = "The previous version is still whole at " + $previous
        if ($null -eq $aside) {
            $notGone = RemoveTree $target
            if ($null -eq $notGone) { Remove-Item -LiteralPath $copying -Force; Fail ("the previous version could not be put back whole, " + $notSame + ". What was copied was removed, so no version is installed. " + $stillKept + ". Close Navisworks and double click Put back the previous version.cmd again") }
            Fail ("the previous version could not be put back whole, " + $notSame + ", and what was copied could not be removed, " + $notGone + ". " + $target + " holds part of it. " + $stillKept + ". Close Navisworks and double click Put back the previous version.cmd again")
        }
        $notBack = SyncBundle $aside $target
        if ($null -ne $notBack) { Fail ("the previous version could not be put back whole, " + $notSame + ", and the version that was installed could not be copied back, " + $notBack + ". " + $target + " may hold part of two versions. The version that was installed is whole at " + $aside + ". " + $stillKept + ". Close Navisworks and double click Put back the previous version.cmd again") }
        Remove-Item -LiteralPath $copying -Force
        $asideWords = "the copy taken aside was removed"
        $notGone = RemoveTree $aside
        if ($null -ne $notGone) { $asideWords = "the copy taken aside is left at " + $aside + ", " + $notGone }
        Fail ("the previous version could not be put back whole, " + $notSame + ". The version that was installed, " + $wasVersion + ", is back at " + $target + ", every file read back the same as the copy taken aside, and " + $asideWords + ". " + $stillKept + ". Close Navisworks and double click Put back the previous version.cmd again")
    }
    Remove-Item -LiteralPath $copying -Force
    if ($null -ne $aside) {
        $notGone = RemoveTree $aside
        if ($null -ne $notGone) { $was += ". A copy of it is left at " + $aside + " and could not be removed, " + $notGone + ". Delete that folder with Navisworks closed" }
    }
    Write-Host ("Put back : " + (VersionAt $target))
    Write-Host ("Where    : " + $target + ", every file read back the same as the copy kept at " + $previous)
    Finish $DoneExit ($DoneStart + "the previous version, " + (VersionAt $target) + ", is back at " + $target + ". " + $was + ". " + $StartLine)
}

if (-not (Test-Path -LiteralPath $source -PathType Container)) { Refuse ("there is no ParsonsNwcFederator.bundle folder beside this script at " + $source + ". Extract the whole zip and double click Install Parsons NWC Federator.cmd in the folder that makes. " + $nothing) }
$gone = @(MissingIn $source)
if ($gone.Count -gt 0) { Refuse ("the add-in in this zip at " + $source + " is not whole, " + $gone.Count + " of its " + $expected.Count + " files missing, the first " + $gone[0] + ". Extract the whole zip again. " + $nothing) }
Write-Host ("In zip   : " + (VersionAt $source))
if (Test-Path -LiteralPath $hold) { Write-Host ("Left     : " + $hold + ", a copy a put back that stopped part way left. This install does not use it and it can be deleted with Navisworks closed") }

# The version installed now is kept at .previous, the one backup, an older backup replaced, but
# only when it is a whole add-in that a stopped run did not leave and that is not the very build
# being installed. Otherwise the backup is left as it is, so a good previous version is never
# pushed out by a broken or an identical one.
$kept = $null
$wasThere = Test-Path -LiteralPath $target
$sameBuild = $false
$keptWords = "No version was installed before, so none is kept"
if (Test-Path -LiteralPath $previous) { $keptWords = "No version was installed before, and the one kept at " + $previous + " was left as it was" }
if ($wasThere) {
    $installedGone = @(MissingIn $target)
    $marked = Test-Path -LiteralPath $copying
    if ($installedGone.Count -eq 0 -and -not $marked) { $sameBuild = (StampOf (Join-Path $source "Contents\v22\Federator.Addin.dll")) -eq (StampOf (Join-Path $target "Contents\v22\Federator.Addin.dll")) }
    $leftOver = ", and none is kept"
    if (Test-Path -LiteralPath $previous) { $leftOver = ", and the one kept at " + $previous + " was left as it was" }
    if ($marked) { $keptWords = "The add-in at " + $target + " is what an install or a put back that stopped part way left, so it was not kept" + $leftOver }
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
    $couldNot = "the new version could not be copied in, " + $notSame
    if ($null -ne $kept) {
        $notBack = SyncBundle $kept $target
        if ($null -eq $notBack) { Remove-Item -LiteralPath $copying -Force; Fail ($couldNot + ". The version installed before is back at " + $target + ", every file read back the same as the copy kept at " + $kept + ". Close Navisworks and run this again") }
        Fail ($couldNot + ", and the version installed before could not be put back, " + $notBack + ". " + $target + " may hold part of two versions, and the version installed before is whole at " + $kept + ". Close Navisworks and double click Put back the previous version.cmd")
    }
    $prevNow = HoldsNow $previous
    if ($sameBuild) {
        # The same build was whole there before, so what was copied over it is the same files.
        # It is left where it is, and read again against the zip to say whether it is whole.
        $still = $null
        try { $still = FromReadsSameIn $source $target } catch { $still = Why $_.Exception }
        if ($null -eq $still) { Remove-Item -LiteralPath $copying -Force; Fail ($couldNot + ". The same build that was installed, " + (VersionAt $target) + ", is still at " + $target + " and was not removed, every file of the zip read back the same there. " + $prevNow + ". Close Navisworks and run this again") }
        Fail ($couldNot + ". The same build that was installed is still at " + $target + " and was not removed, but it no longer reads the same as the zip, " + $still + ". " + $prevNow + ". " + (Advice))
    }
    # Nothing was there, or what was there was not whole or left by a stopped run, so what
    # stands at the load path now is part of a version and it is removed.
    $before = "Nothing was installed before"
    if ($wasThere) { $before = "What was at " + $target + " before was not whole or was left by a run that stopped part way" }
    $notGone = RemoveTree $target
    if ($null -eq $notGone) { Remove-Item -LiteralPath $copying -Force; Fail ($couldNot + ". " + $before + ", and what is there now was removed, so no version is installed. " + $prevNow + ". " + (Advice)) }
    Fail ($couldNot + ". " + $before + ", and what is there now could not be removed, " + $notGone + ", so " + $target + " holds part of a version. " + $prevNow + ". Close Navisworks, delete that folder and run this again")
}
Remove-Item -LiteralPath $copying -Force

$installedVersion = VersionAt $target
Write-Host ("Installed: " + $installedVersion)
Write-Host ("Where    : " + $target + ", every file read back the same as in the zip")
Finish $DoneExit ($DoneStart + "Parsons NWC Federator " + $installedVersion + ", is installed at " + $target + ". " + $keptWords + ". " + $StartLine)
