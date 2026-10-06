param(
  [string]$Mode = "Check",
  [string]$Set = "",
  [string]$Item = "",
  [string]$Stamp = "",
  [string]$RunFolder = "",
  [string]$Folder = "",
  [string]$Xml = "",
  [string]$OpenFile = "",
  [string]$For = "",
  [string]$PictureStatuses = "",
  [string]$PictureCap = "",
  [switch]$PriorityPicked,
  [string]$Untick = ""
)
$ErrorActionPreference = "Stop"

# tools\loop\run.ps1, F103 part 1, F106 and F104 part 2. Starts, watches and closes one
# Navisworks for the loop with every guard kept in code. The design is
# steps\notes\f103-design.md and, for the documents read, section 3 of steps\notes\f104-design.md,
# and the rules it keeps are in .claude\rules\loop.md. The guard code it shares with the probe
# and the driver is tools\loop\nw-guard.ps1, one copy, dot-sourced here.
#
# Always started as
#
#   powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\run.ps1 -Mode <mode> ...
#
# FIVE MODES:
#   Check                      the default. Reads only, writes nothing, prints to the console.
#                              Exit 0 when Run would go, 2 when it would refuse. With
#                              -For Documents and the parameters of a documents read, the same
#                              for Documents, and the pairs it would read are printed
#   Install -Stamp <8 hex>     installs this clean checkout at that commit with
#                              build\install.ps1, after its refusals and the bundle backup
#   Run -Set NN -Item 0 -Stamp <8 hex>
#                              the start with no window: refusals, backups, one Navisworks
#                              started through the Automation API and adopted, held open and
#                              Visible for 360 s with its processor time read every 15 s,
#                              closed with Dispose, and Bader's settings put back by the D2
#                              rule. ExecuteAddInPlugin is never called, so the tool's window
#                              never opens and nothing is written into his logs folder
#   Run -Set NN -Item 1 to 5 -Folder <a folder of NMFed\NWC> -Stamp <8 hex>, F106
#                              the window run on the run set's copy, runs\NN\NMFed: the same
#                              refusals, backups, start, adoption, watchdog and put back, then
#                              the driver tools\probes\drive-window-run.ps1 started as a child
#                              and ExecuteAddInPlugin called on the main thread. Item 1 takes
#                              -Xml, a file under runs\NN\NMFed. Item 5 takes -OpenFile, the
#                              plain name of an .nwf in NMFed\NWF\<Folder> or an .nwd in
#                              NMFed\NWD\<Folder>, copied into the run folder's open\ and
#                              opened there. F126: -Untick names tick boxes of the tool's window
#                              by their AutomationId, joined by commas, such as
#                              SkipClashOffCoordinates, and is handed to the driver, which
#                              unticks each before it presses anything. Check with -Item 1 to 5
#                              takes it too and names the boxes. The record names them
#   CloseOwn -RunFolder <runs\NN\item...>
#                              closes the loop's own Navisworks after a run.ps1 died, only
#                              when its mypid.txt, name, path, command line and start ticks all
#                              match
#   Documents -Set NN -Item 1 to 5 -Folder <a folder of NMFed\NWC>, F104 part 2
#                              reads the documents a window run wrote: the pairs of NWF and
#                              workbook read-out off that run's .tsv in steps\runs\NN\item<K>-<Folder>,
#                              then the same refusals, backups, start, adoption, watchdog and put
#                              back as item 0, with AddPluginAssembly of the probe
#                              tools\probes\DocumentReadProbe and one ExecuteAddInPlugin that
#                              reads every NWF in place of the driver, then each NWF's sha256
#                              again, every read-out, tools\loop\compare-document.ps1 per pair and
#                              summary.txt. Item 5 also takes -OpenFile, which names its run.
#                              -PictureStatuses, -PictureCap and -PriorityPicked are handed to
#                              compare-document.ps1 as the run was told them. It writes only a
#                              new folder runs\NN\item<K>-<Folder>-document-yyyyMMdd-HHmmss and
#                              nothing into steps\runs, which the lead fills through F102's mask
#
# EXIT CODES: 0 finished and everything put back, for item 5 on an NWD also the tool's own
# refusal, TOOL REFUSED, 1 a fault in run.ps1, UNKNOWN whether the adopted Navisworks still
# runs, one still running after every close path, a window run whose log does not show it
# RAN, or a documents read that did not do every step: a probe call that threw, a read-out not
# whole, a comparison that did not finish, or a file of the copy's NWF, NWD or Clash Report
# folders that does not read after the close as before the start, 2 refused, for Install also a refusal of build\install.ps1, 3 not adopted or the
# constructor deadline, 4 hung, the ceiling, a call into the adopted Navisworks that did not
# return in 120 s, or the tool's window still open 120 s after WM_CLOSE, 5 finished but a
# dialog appeared, or for Install, installed but a Navisworks ran right after it or the add-in
# installed before was left beside it, 6 something of Bader's not put back, 7 the adopted
# Navisworks or the tool's window ended by itself, 8 the driver stopped before it pressed
# anything that runs. When more than one applies, the first in the order 3, 6, 4, 7, 1, 8, 5,
# which RunVerdict keeps.
#
# NUMBERS THAT ARE NOT PARAMETERS, so no switch can move a path or shorten a limit: the hang
# rule's 300 s and its 20 s of processor time, Q83, the constructor deadline's 300 s, the
# ceiling's 12 hours, Q84, the 120 s a call into the adopted Navisworks may take, which is also
# the time the tool's window has to close after WM_CLOSE, the 600 s OpenFile may take, which no
# run has measured, item 0's hold of 360 s, the 15 s the tool's log stays quiet after its
# RESULT block before WM_CLOSE, the monitor's pass of 15 s and its heartbeat of 60 s.

$HangLimitSeconds = 300
$HangCpuSeconds = 20
$CtorDeadlineSeconds = 300
$CeilingSeconds = 43200
$HoldSeconds = 360
$PassSeconds = 15
$BeatSeconds = 60
$CallLimitSeconds = 120
$OpenLimitSeconds = 600
$QuietSeconds = 15
# The plugin's id, PluginName and DeveloperCode of src\Federator.Addin\FederatorPlugin.cs.
$PluginId = "ParsonsNwcFederator.PARS"
# The probe's id, PluginName and DeveloperCode of
# tools\probes\DocumentReadProbe\DocumentReadProbePlugin.cs, F104 part 2.
$ProbeId = "DocumentReadProbe.PARS"

$nw = "C:\Program Files\Autodesk\Navisworks Manage 2025"
$loopRoot = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$utf8 = New-Object System.Text.UTF8Encoding($false)
$T0 = [DateTime]::Now
$guardFile = Join-Path $PSScriptRoot "nw-guard.ps1"
. $guardFile
$guardText = [System.IO.File]::ReadAllText($guardFile)
$script:RecordFile = $null
$script:AlsoFile = $null
$script:RecordLock = New-Object System.Object
$script:SayFailures = 0
# NewRoamers, moved from the probe, reads this. No start is made while any Roamer runs, so
# the Roamers running before the start are always none.
$beforeTicks = @{}

# =======================================================================================
# Writing. Every line goes to the console and, once the run folder exists, to record.txt.
# A line that cannot be written still reaches the console, and is counted.
function RecordAppend($lock, $file, $t) {
  [System.Threading.Monitor]::Enter($lock)
  try { [System.IO.File]::AppendAllText($file, $t + "`r`n", (New-Object System.Text.UTF8Encoding($false))); return $true }
  catch { [Console]::Error.WriteLine("could not append to " + $file + ": " + $_.Exception.Message); return $false }
  finally { [System.Threading.Monitor]::Exit($lock) }
}
function Say($t) {
  [Console]::Out.WriteLine($t)
  if ($null -ne $script:RecordFile) { if (-not (RecordAppend $script:RecordLock $script:RecordFile $t)) { $script:SayFailures++ } }
  if ($null -ne $script:AlsoFile) { if (-not (RecordAppend $script:RecordLock $script:AlsoFile $t)) { $script:SayFailures++ } }
}

# Every path run.ps1 reads or writes, built here and nowhere else. F106: a window run's paths
# are all under runs\NN, the run set's copy NMFed in Bader's folder shape and the run folder
# beside it, named so two communities never meet, item1-C06, and for item 5 by the kind of the
# file opened, item5-C06-nwf. F104 part 2: with $docStamp, the documents read of that window
# run, whose own folder is new at every call, runs\NN\item1-C06-document-yyyyMMdd-HHmmss, so a
# record with no VERDICT line in it is found by check 4 and CloseOwn reads it as a run folder.
# RunName and Evidence stay the window run's, which the documents read reads and never writes,
# and SourceRunDir is that run's own folder, whose open folder item 5's outputs.txt names.
function RunPaths($loopRoot, $repo, $set, $item, $folder, $xml, $openFile, $docStamp) {
  $p = [pscustomobject]@{
    LoopRoot = $loopRoot
    RunsRoot = Join-Path $loopRoot "runs"
    RunDir = $null
    RunName = $null
    Evidence = $null
    Unproved = Join-Path $loopRoot "probes\unproved-starts.txt"
    LogsBackup = Join-Path $loopRoot "logs-backup"
    AutoBackup = Join-Path $loopRoot "autosave-backup"
    BundleBackup = Join-Path $loopRoot "bundle-backup"
    Installs = Join-Path $loopRoot "installs"
    Manifest = Join-Path $loopRoot "source.manifest.txt"
    Removed = Join-Path $loopRoot "source.removed.txt"
    HisLogs = Join-Path $env:LOCALAPPDATA "ParsonsNwcFederator\logs"
    NwAppData = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025"
    AutoSave = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025\AutoSave"
    Bundle = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
    InstalledDll = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22\Federator.Addin.dll"
    RegSub = "Software\Autodesk\Navisworks Manage\22.0"
    SetRoot = $null; Copy = $null; CopyManifest = $null; CopyRemoved = $null
    Nwc = $null; Nwf = $null; Nwd = $null; Report = $null; XmlFile = $null; OpenSource = $null; OpenDir = $null
    SourceRunDir = $null; DocDir = $null; CompareDir = $null; ProbeCopy = $null
    ProbeDll = Join-Path $repo "tools\probes\DocumentReadProbe\bin\Release\net48\DocumentReadProbe.dll"
  }
  if ($set -ne "" -and $item -ne "") {
    $p.RunName = "item" + $item
    if ($item -match '^[1-5]$' -and [string]$folder -ne "") {
      $p.RunName = "item" + $item + "-" + $folder
      if ($item -eq "5") { $p.RunName = $p.RunName + "-" + ([System.IO.Path]::GetExtension([string]$openFile)).TrimStart('.').ToLowerInvariant() }
      $p.SetRoot = Join-Path $loopRoot ("runs\" + $set)
      $p.Copy = Join-Path $p.SetRoot "NMFed"
      $p.CopyManifest = Join-Path $p.SetRoot "NMFed.manifest.txt"
      $p.CopyRemoved = Join-Path $p.SetRoot "NMFed.removed.txt"
      $p.Nwc = Join-Path $p.Copy ("NWC\" + $folder)
      $p.Nwf = Join-Path $p.Copy ("NWF\" + $folder)
      $p.Nwd = Join-Path $p.Copy ("NWD\" + $folder)
      $p.Report = Join-Path $p.Copy ("Clash Report\" + $folder)
    }
    $p.RunDir = Join-Path $loopRoot ("runs\" + $set + "\" + $p.RunName)
    $p.Evidence = Join-Path $repo ("steps\runs\" + $set + "\" + $p.RunName)
    if ($null -ne $p.Copy) {
      if ([string]$xml -ne "") { if ([System.IO.Path]::IsPathRooted([string]$xml)) { $p.XmlFile = [string]$xml } else { $p.XmlFile = Join-Path $p.Copy $xml } }
      if ([string]$openFile -ne "") {
        $p.OpenDir = Join-Path $p.RunDir "open"
        if ($p.RunName.EndsWith("-nwd")) { $p.OpenSource = Join-Path $p.Nwd $openFile } else { $p.OpenSource = Join-Path $p.Nwf $openFile }
      }
    }
    if ([string]$docStamp -ne "") {
      $p.SourceRunDir = $p.RunDir
      $p.RunDir = Join-Path $loopRoot ("runs\" + $set + "\" + $p.RunName + "-document-" + $docStamp)
      $p.DocDir = Join-Path $p.RunDir "document"
      $p.CompareDir = Join-Path $p.RunDir "compare"
      $p.ProbeCopy = Join-Path $p.RunDir "probe\DocumentReadProbe.dll"
    }
  }
  return $p
}

# =======================================================================================
# Check 1, the host.
function HostRefusal($scriptPath) {
  $why = New-Object System.Collections.Generic.List[string]
  $v = $PSVersionTable.PSVersion
  if (-not ($v.Major -eq 5 -and $v.Minor -eq 1)) { $why.Add("this is PowerShell " + $v + ", not Windows PowerShell 5.1") }
  if (-not [Environment]::Is64BitProcess) { $why.Add("this process is 32 bit") }
  $apt = [string][System.Threading.Thread]::CurrentThread.ApartmentState
  if ($apt -ne "STA") { $why.Add("this thread's apartment is " + $apt + ", not STA") }
  OwnProcessRefusal $scriptPath $why
  return ,$why
}

# The five modes, the one list of them. ModeOf returns the mode as it is written here, or null.
function Modes { return @("Check", "Install", "Run", "CloseOwn", "Documents") }
function ModeOf($mode) {
  foreach ($m in (Modes)) { if ($m -eq $mode) { return $m } }
  return $null
}

# Check 2, the parameters, as an allow list. Returns each refusal as the text after REFUSED:.
# F106: items 1 to 5 take -Folder, a plain folder name, item 1 alone takes -Xml and item 5 alone
# -OpenFile, and every path they name must lie under runs\NN, the run set's own folder.
# F104 part 2: Documents, and Check -For Documents, take -Set, -Item 1 to 5 and -Folder as the
# window run they read, -OpenFile for item 5 alone, which names that run, no -Stamp and no
# -Xml, and alone take the three switches handed to compare-document.ps1. Their words are
# judged by compare-document.ps1, the one place that knows them, and only their shape here.
# F126: -Untick only for a window run, Run or Check with -Item 1 to 5 and never for a documents
# read, its ids judged by UntickRefusal in nw-guard.ps1, the one rule the driver keeps too.
function ParamRefusal($mode, $set, $item, $stamp, $runFolder, $folder, $xml, $openFile, $extra, $loopRoot, $repo, $for, $pictureStatuses, $pictureCap, [bool]$priorityPicked, $docStamp, $untick) {
  $why = New-Object System.Collections.Generic.List[string]
  foreach ($a in @($extra)) { $why.Add("the argument " + $a + " is not a parameter of run.ps1") }
  $m = ModeOf $mode
  if ($null -eq $m) { $why.Add("-Mode is " + $mode + ", not one of " + ((Modes) -join ", ")); return ,$why }
  $mode = $m
  if ([string]$for -ne "") {
    if ($mode -ne "Check") { $why.Add("-For is " + $for + ", and only -Mode Check takes it, to read the refusals of another mode") }
    elseif ($for -cne "Run" -and $for -cne "Documents") { $why.Add("-For is " + $for + ", not Run or Documents") }
  }
  $docs = ($mode -eq "Documents" -or ($mode -eq "Check" -and $for -ceq "Documents"))
  $needSet = ($mode -eq "Run" -or $docs); $needItem = ($mode -eq "Run" -or $docs); $needStamp = ($mode -eq "Run" -or $mode -eq "Install"); $needFolder = ($mode -eq "CloseOwn")
  $setWho = "-Mode Run"; if ($docs) { $setWho = "a documents read" }
  if ($set -ne "" -and $set -notmatch '^\d\d$') { $why.Add("-Set is " + $set + ", not two digits") }
  elseif ($set -eq "" -and $needSet) { $why.Add("-Set is missing, and " + $setWho + " needs two digits") }
  if ($item -ne "") {
    if ($item -notmatch '^[0-5]$') { $why.Add("-Item is " + $item + ", not 0 to 5") }
    elseif ($docs -and $item -eq "0") { $why.Add("-Item is 0, and item 0 starts no tool, so it wrote no NWF and no workbook for a documents read to read") }
  } elseif ($needItem) { $why.Add("-Item is missing, and " + $setWho + " needs " + $(if ($docs) { "1 to 5" } else { "0 to 5" })) }
  if ($stamp -ne "" -and $stamp -cnotmatch '^[0-9a-f]{8}$') { $why.Add("-Stamp is " + $stamp + ", not 8 lower case hex characters") }
  elseif ($stamp -eq "" -and $needStamp) { $why.Add("-Stamp is missing, and -Mode " + $mode + " needs the 8 hex characters of the commit") }
  if ($docs -and $stamp -ne "") { $why.Add("-Stamp is " + $stamp + ", and a documents read takes none, it calls only the probe and never the installed add-in") }
  if ($mode -ne "Check" -and $mode -ne "Run" -and $mode -ne "Documents") {
    if ($set -ne "") { $why.Add("-Set is " + $set + ", and -Mode " + $mode + " takes none") }
    if ($item -ne "") { $why.Add("-Item is " + $item + ", and -Mode " + $mode + " takes none") }
  }
  if ($docs) {
    if ([string]$pictureStatuses -ne "" -and $pictureStatuses -notmatch '^[A-Za-z]+(,[A-Za-z]+)*$') { $why.Add("-PictureStatuses is " + $pictureStatuses + ", not status words joined by commas, such as New,Active,Reviewed") }
    if ([string]$pictureCap -ne "" -and $pictureCap -notmatch '^\d{1,6}$') { $why.Add("-PictureCap is " + $pictureCap + ", not a whole number") }
  } else {
    if ([string]$pictureStatuses -ne "") { $why.Add("-PictureStatuses is " + $pictureStatuses + ", and only a documents read takes it") }
    if ([string]$pictureCap -ne "") { $why.Add("-PictureCap is " + $pictureCap + ", and only a documents read takes it") }
    if ($priorityPicked) { $why.Add("-PriorityPicked is given, and only a documents read takes it") }
  }
  if ($mode -eq "CloseOwn" -and $stamp -ne "") { $why.Add("-Stamp is " + $stamp + ", and -Mode CloseOwn takes none") }
  if ($runFolder -ne "" -and -not $needFolder) { $why.Add("-RunFolder is " + $runFolder + ", and -Mode " + $mode + " takes none") }
  if ($needFolder) {
    if ($runFolder -eq "") { $why.Add("-RunFolder is missing, and -Mode CloseOwn needs the run folder under %LOCALAPPDATA%\NwcFederatorLoop\runs") }
    else {
      $pr = PathRefusal $runFolder $loopRoot
      if ($null -ne $pr) { $why.Add("-RunFolder is " + $runFolder + ", " + $pr) }
      elseif (([System.IO.Path]::GetFullPath($runFolder)) -notmatch '\\runs\\\d\d\\item[^\\]+$') { $why.Add("-RunFolder is " + $runFolder + ", not a folder runs\NN\item... of the loop") }
    }
  }
  $window = ($item -match '^[1-5]$' -and ($mode -eq "Run" -or $mode -eq "Check" -or $mode -eq "Documents"))
  if ($window) {
    if ($folder -eq "") { $why.Add("-Folder is missing, and item " + $item + " needs the name of one folder of NMFed\NWC, such as C06") }
    elseif ($folder -notmatch '^[A-Za-z0-9_-]+$') { $why.Add("-Folder is " + $folder + ", not the plain name of one folder of NMFed\NWC") }
    if ($docs) { if ($xml -ne "") { $why.Add("-Xml is " + $xml + ", and a documents read takes none, it reads what the window run already wrote") } }
    elseif ($item -eq "1" -and $xml -eq "") { $why.Add("-Xml is missing, and item 1, the first run, needs the clash XML of the copy") }
    elseif ($item -eq "1" -and -not $xml.EndsWith(".xml", [StringComparison]::OrdinalIgnoreCase)) { $why.Add("-Xml is " + $xml + ", not an .xml file") }
    if (-not $docs -and $item -ne "1" -and $xml -ne "") { $why.Add("-Xml is " + $xml + ", and only item 1 takes the XML, the runs after it run with none") }
    if ($item -eq "5" -and $openFile -eq "") { $why.Add("-OpenFile is missing, and item 5 needs the plain name of an .nwf in NMFed\NWF\<Folder> or an .nwd in NMFed\NWD\<Folder>") }
    elseif ($item -eq "5" -and $openFile -notmatch '^[^\\/:*?"<>|]+\.(nwf|nwd)$') { $why.Add("-OpenFile is " + $openFile + ", not the plain name of an .nwf or an .nwd file") }
    if ($item -ne "5" -and $openFile -ne "") { $why.Add("-OpenFile is " + $openFile + ", and only item 5 takes it") }
    if ($docs -and [string]$untick -ne "") { $why.Add("-Untick is " + (MaskLine $untick) + ", and a documents read takes none, it presses nothing in the tool's window") }
    elseif ([string]$untick -ne "") { foreach ($u in (UntickRefusal $untick)) { $why.Add($u) } }
  } else {
    if ([string]$untick -ne "") { $why.Add("-Untick is " + (MaskLine $untick) + ", and only a window run, item 1 to 5, takes it") }
    foreach ($pair in @(@("-Folder", $folder), @("-Xml", $xml), @("-OpenFile", $openFile))) {
      if ([string]$pair[1] -ne "") { $why.Add($pair[0] + " is " + $pair[1] + ", and " + $(if ($item -eq "0") { "item 0" } else { "-Mode " + $mode }) + " takes none") }
    }
  }
  if (($mode -eq "Run" -or $mode -eq "Documents" -or ($mode -eq "Check" -and $set -ne "" -and $item -ne "")) -and $why.Count -eq 0) {
    $ds = ""; if ($docs) { $ds = $docStamp }
    $p = RunPaths $loopRoot $repo $set $item $folder $xml $openFile $ds
    $pr = PathRefusal $p.RunDir $loopRoot
    if ($null -ne $pr) { $why.Add("-Set is " + $set + ", and the run folder it names " + $pr) }
    foreach ($pair in @(@("-Folder", $p.Nwc, $p.Copy), @("-Xml", $p.XmlFile, $p.Copy), @("-OpenFile", $p.OpenSource, $p.Copy))) {
      if ($null -eq $pair[1]) { continue }
      $pr = PathRefusal $pair[1] $loopRoot
      if ($null -eq $pr) { $pr = UnderRefusal $pair[1] $pair[2] }
      if ($null -ne $pr) { $why.Add($pair[0] + " names " + (Mask $pair[1]) + ", which " + $pr) }
    }
  }
  return ,$why
}
# F106. A path must resolve under $root, the run set's copy NMFed, and not merely under the loop
# folder, so no parameter of a window run can name another set's files. Returns why not, or null.
function UnderRefusal($path, $root) {
  $full = $null
  try { $full = [System.IO.Path]::GetFullPath($path).TrimEnd('\') } catch { return ("cannot be read as a path, " + (Err $_.Exception)) }
  $r = [System.IO.Path]::GetFullPath($root).TrimEnd('\')
  if (-not $full.StartsWith($r + "\", [StringComparison]::OrdinalIgnoreCase)) { return "does not resolve under the run set's copy " + (Mask $r) }
  return $null
}

# A path is allowed only under the loop folder, and only when no folder from the loop folder
# down to it is a junction or a link. Returns why not, or null.
function PathRefusal($path, $loopRoot) {
  $full = $null
  try { $full = [System.IO.Path]::GetFullPath($path).TrimEnd('\') } catch { return ("it cannot be read as a path, " + (Err $_.Exception)) }
  $root = [System.IO.Path]::GetFullPath($loopRoot).TrimEnd('\')
  if (-not ($full.Equals($root, [StringComparison]::OrdinalIgnoreCase) -or $full.StartsWith($root + "\", [StringComparison]::OrdinalIgnoreCase))) { return "it does not resolve under %LOCALAPPDATA%\NwcFederatorLoop" }
  $walk = @($root)
  $cur = $root
  foreach ($part in @($full.Substring($root.Length).Trim('\').Split('\') | Where-Object { $_ -ne "" })) { $cur = Join-Path $cur $part; $walk += $cur }
  foreach ($d in $walk) {
    if (-not (Test-Path -LiteralPath $d)) { break }
    $it = Get-Item -LiteralPath $d -Force
    if (($it.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { return ("it passes " + (Mask $d) + ", a junction or a link") }
  }
  return $null
}

# Check 4. Every run folder whose record.txt has no VERDICT line.
function DiedRuns($runsRoot) {
  $died = New-Object System.Collections.Generic.List[string]
  if (-not (Test-Path -LiteralPath $runsRoot)) { return ,$died }
  foreach ($setDir in @(Get-ChildItem -LiteralPath $runsRoot -Directory -Force)) {
    foreach ($itemDir in @(Get-ChildItem -LiteralPath $setDir.FullName -Directory -Force)) {
      $rec = Join-Path $itemDir.FullName "record.txt"
      if (-not (Test-Path -LiteralPath $rec)) { continue }
      if (-not (HasVerdict $rec)) { $died.Add($itemDir.FullName) }
    }
  }
  return ,$died
}
function HasVerdict($recordFile) {
  foreach ($l in [System.IO.File]::ReadAllLines($recordFile)) { if ($l.StartsWith("VERDICT")) { return $true } }
  return $false
}

# Check 7. The installed add-in's stamp, read as FileVersionInfo.ProductVersion, which loads
# and locks nothing. M2 in the harness reads whether it equals AssemblyInformationalVersion.
function InstalledStamp($dll) {
  if (-not (Test-Path -LiteralPath $dll)) { return "UNKNOWN, there is no installed add-in" }
  return [string][System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion
}

# Check 11. The evidence folder must hold no file, bar the evidence of a run that was NOT
# RUN, which check 12 moves aside, never emptied, so a retry with the same set can go.
function EvidenceFiles($dir) {
  if ($null -eq $dir -or -not (Test-Path -LiteralPath $dir)) { return 0 }
  return @(Get-ChildItem -LiteralPath $dir -Recurse -File -Force).Count
}
function EvidenceNotRun($dir) {
  $rec = Join-Path $dir "record.txt"
  if (-not (Test-Path -LiteralPath $rec)) { return $false }
  $v = @([System.IO.File]::ReadAllLines($rec) | Where-Object { $_.StartsWith("VERDICT") })
  return ($v.Count -eq 1 -and $v[0].StartsWith("VERDICT: NOT RUN"))
}

# F106, check 8, the run set's copy for a window run. It must be whole, its NMFed.manifest.txt
# written last by the copy, it must have one NWC out for item 3 and none for the other items,
# every file the manifest names under NWC\<Folder> must be there with its size and sha256, and
# no NWC the manifest does not name may be, so the run's evidence is about the files the
# manifest names. A file of another kind is not read by the tool's scan, which reads *.nwc,
# and is not judged. Returns each reason, reading only.
function CopyRefusal($paths, $item) {
  $why = New-Object System.Collections.Generic.List[string]
  $sub = $paths.Nwc.Substring($paths.Copy.Length).TrimStart('\') + "\"
  if (-not (Test-Path -LiteralPath $paths.CopyManifest)) { $why.Add("the run set's copy is not whole, " + (Mask $paths.CopyManifest) + " is not there, so the copy was never finished. Make the set's copy first. Nothing was written"); return ,$why }
  $out = $null
  if (Test-Path -LiteralPath $paths.CopyRemoved) {
    $note = @([System.IO.File]::ReadAllLines($paths.CopyRemoved))
    if ($note.Count -lt 2 -or $note[0].Trim() -eq "") { $why.Add((Mask $paths.CopyRemoved) + " cannot be read as the name and sha256 of the file taken out. A person has to look. Nothing was written"); return ,$why }
    $out = $note[0].Trim()
    if ($item -ne "3") { $why.Add("item " + $item + " runs on the whole copy, and NMFed.removed.txt says " + $out + " is out of it. Put it back first. Nothing was written") }
    elseif (-not $out.StartsWith($sub, [StringComparison]::OrdinalIgnoreCase)) { $why.Add("item 3 takes a file of " + $sub.TrimEnd('\') + " out, and NMFed.removed.txt names " + $out + ". Nothing was written") }
  } elseif ($item -eq "3") { $why.Add("item 3, a file gone, needs one NWC taken out of the copy, and NMFed.removed.txt is not there. Nothing was written") }
  if ($why.Count -gt 0) { return ,$why }
  $want = @{}
  foreach ($l in [System.IO.File]::ReadAllLines($paths.CopyManifest)) {
    $x = $l.Split(' ', 3)
    if ($x.Count -lt 3) { continue }
    if ($x[2].StartsWith($sub, [StringComparison]::OrdinalIgnoreCase)) { $want[$x[2].ToLowerInvariant()] = [pscustomobject]@{ Rel = $x[2]; Hash = $x[0]; Length = $x[1] } }
  }
  if ($want.Count -eq 0) { $why.Add("NMFed.manifest.txt names no file under " + $sub.TrimEnd('\') + ", so -Folder names no folder of the copy. Nothing was written"); return ,$why }
  $l = ListFolder $paths.Nwc
  if (-not $l.Ok -or $l.Missing) { $why.Add($sub.TrimEnd('\') + " of the copy cannot be read whole, " + $(if ($l.Missing) { "it is not there" } else { $l.Why }) + ". Nothing was written"); return ,$why }
  $seen = @{}
  foreach ($e in $l.Entries) {
    $rel = $sub + $e.Rel
    $seen[$rel.ToLowerInvariant()] = $true
    $w = $want[$rel.ToLowerInvariant()]
    if ($null -eq $w) { if ($rel.EndsWith(".nwc", [StringComparison]::OrdinalIgnoreCase)) { $why.Add("the copy holds " + $rel + ", which NMFed.manifest.txt does not name. Nothing was written") } }
    elseif ($w.Hash -ne $e.Hash -or [string]$w.Length -ne [string]$e.Length) { $why.Add($rel + " of the copy differs from NMFed.manifest.txt by size or sha256. Nothing was written") }
  }
  foreach ($k in $want.Keys) {
    if ($seen.ContainsKey($k)) { if ($null -ne $out -and $k -eq $out.ToLowerInvariant()) { $why.Add("NMFed.removed.txt names " + $out + " as taken out, and it is still in the copy. Nothing was written") }; continue }
    if ($null -ne $out -and $k -eq $out.ToLowerInvariant()) { continue }
    $why.Add($want[$k].Rel + " is named by NMFed.manifest.txt and is not in the copy. Nothing was written")
  }
  return ,$why
}
# F106, check 9, the outputs of a window run. Item 1 needs its three output folders empty, the
# runs after it need the NWFs item 1 wrote, and item 5 the file it opens. Returns each reason.
function OutputsRefusal($paths, $item) {
  $why = New-Object System.Collections.Generic.List[string]
  foreach ($d in @($paths.Nwf, $paths.Nwd, $paths.Report)) {
    if (-not (Test-Path -LiteralPath $d -PathType Container)) { $why.Add((Mask $d) + " is not there, so the copy is not in Bader's folder shape. Nothing was written"); continue }
    if ($item -eq "1") {
      $n = @(Get-ChildItem -LiteralPath $d -Recurse -File -Force).Count
      if ($n -gt 0) { $why.Add("item 1, the first run, needs empty output folders, and " + (Mask $d) + " holds " + $n + " files. Use a new set, nothing is ever emptied. Nothing was written") }
    }
  }
  if ($why.Count -gt 0) { return ,$why }
  if ($item -eq "1" -and -not (Test-Path -LiteralPath $paths.XmlFile -PathType Leaf)) { $why.Add("-Xml names " + (Mask $paths.XmlFile) + ", which is not a file of the copy. Nothing was written") }
  if ($item -match '^[234]$' -and @(Get-ChildItem -LiteralPath $paths.Nwf -File -Force -Filter "*.nwf").Count -eq 0) { $why.Add("item " + $item + " runs on the NWFs the first run wrote, and " + (Mask $paths.Nwf) + " holds none. Nothing was written") }
  if ($item -eq "5" -and -not (Test-Path -LiteralPath $paths.OpenSource -PathType Leaf)) { $why.Add("-OpenFile names " + (Mask $paths.OpenSource) + ", which is not there. Nothing was written") }
  return ,$why
}
# F106, check 10, the session. A window run starts only while the session reads unlocked, Q85.
function LockRefusal($lockText) {
  if ($lockText -eq "unlocked") { return $null }
  return ("the session reads " + $lockText + ", and a locked screen may stop the window or its pictures, Q85. The loop waits and starts nothing until the session reads unlocked, and the lock is never worked around. Nothing was written")
}

# Checks 4 to 11, in Run's order. 8, 9 and 10 apply to the window runs, items 1 to 5. Returns
# each refusal as the text after REFUSED:. F104 part 2: for a documents read, $docs is what
# ReadPairs read and $probe what ProbeRead read, 8, 9 and 10 do not apply, check 11 is the
# window run's evidence and its pairs, and check 19 the probe.
function RunRefusals($paths, $stamp, [bool]$stopAtFirst, $item, $winType, $docs, $probe) {
  $r = New-Object System.Collections.Generic.List[string]
  Say "  check 4, a run that died"
  $died = DiedRuns $paths.RunsRoot
  foreach ($d in $died) { $r.Add((Mask $d) + " holds a record with no VERDICT line, so an earlier run.ps1 ended before it finished and what it left is unknown. Read it, run CloseOwn if its Navisworks still runs, and append the VERDICT line the lead writes. Nothing was started and nothing was written") }
  if ($died.Count -eq 0) { Say "    none" }
  if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
  Say "  check 5, unproved starts"
  $u = UnprovedRefusal $paths.Unproved
  if ($u.Stop) {
    if ($null -ne $u.Still -and $u.Still.Count -gt 0) { $r.Add("a start the loop could not prove is still running, " + $u.Still[0] + ". A person has to look. Nothing was started and nothing was written") }
    else { $r.Add($u.Text.Replace("STOP before the constructor: ", "") + ". Nothing was started and nothing was written") }
  }
  if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
  Say "  check 6, any Navisworks"
  if (RoamerRefusal) {
    $first = "UNKNOWN"
    $ps = @()
    try { $ps = @([System.Diagnostics.Process]::GetProcessesByName("Roamer") | Sort-Object Id) } catch { $first = "UNKNOWN, the process list could not be read again, " + (Err $_.Exception) }
    if ($ps.Count -gt 0) { $st = "its start time could not be read"; try { $st = "started " + $ps[0].StartTime.ToString("yyyy-MM-dd HH:mm:ss") } catch { $st = "its start time could not be read, " + (Err $_.Exception) }; $first = "pid " + $ps[0].Id + ", " + $st }
    $r.Add("Navisworks is running, Roamer " + $first + ". Nothing is started, installed or put back while any Navisworks runs, whoever started it. Close it and run again. Nothing was written")
  }
  if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
  Say "  check 7, the installed stamp"
  if ($stamp -eq "") { Say "    not judged, no -Stamp was given" }
  else {
    $pv = InstalledStamp $paths.InstalledDll
    Say ("    the installed Federator.Addin.dll reads " + $pv)
    if (-not (StampNames $pv $stamp)) { $r.Add("the installed add-in reads " + $pv + ", and this run is for " + $stamp + ". Install it with -Mode Install first. Nothing was written") }
  }
  if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
  if ($null -ne $docs) {
    Say "  checks 8, 9 and 10, the copy, the outputs and the session, apply to the window runs and not to a documents read, which starts no tool and opens no window of it"
    Say "  check 11, the window run's evidence and the pairs off its .tsv"
    foreach ($x in $docs.Why) { $r.Add($x) }
    if ($docs.Why.Count -eq 0) { Say ("    " + $docs.Pairs.Count + " pairs, every NWF reading as outputs.txt lists it and every workbook read-out whole") }
    if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
    Say "  check 19, the probe"
    if ($null -ne $probe.Why) { $r.Add($probe.Why) } else { Say ("    " + (Mask $paths.ProbeDll) + ", " + $probe.Length + " bytes, sha256 " + $probe.Hash + ", stamp " + $probe.Stamp) }
    return ,$r
  }
  if ([string]$item -match '^[1-5]$') {
    Say "  check 8, the run set's copy"
    $c8 = CopyRefusal $paths $item
    foreach ($x in $c8) { $r.Add($x) }
    if ($c8.Count -eq 0) { Say ("    whole, " + $(if ($item -eq "3") { "one NWC out as item 3 needs" } else { "nothing out" }) + ", and every file of " + (Mask $paths.Nwc) + " matches NMFed.manifest.txt") }
    if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
    Say "  check 9, the outputs"
    $c9 = OutputsRefusal $paths $item
    foreach ($x in $c9) { $r.Add($x) }
    if ($c9.Count -eq 0) { Say "    as the item needs" }
    if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
    Say "  check 10, the session"
    $lt = SessionLockText $winType
    Say ("    the session reads " + $lt)
    $c10 = LockRefusal $lt
    if ($null -ne $c10) { $r.Add($c10) }
    if ($stopAtFirst -and $r.Count -gt 0) { return ,$r }
  } else { Say "  checks 8, 9 and 10, the copy, the outputs and the session, apply to the window runs, items 1 to 5, and not to item 0" }
  Say "  check 11, the evidence"
  if ($null -eq $paths.Evidence) { Say "    not judged, no -Set and -Item were given" }
  else {
    $n = EvidenceFiles $paths.Evidence
    Say ("    " + $n + " files in steps\runs\" + (Split-Path (Split-Path $paths.Evidence -Parent) -Leaf) + "\" + (Split-Path $paths.Evidence -Leaf))
    if ($n -gt 0 -and (EvidenceNotRun $paths.Evidence)) { Say "    they are the evidence of a run that was NOT RUN, which check 12 moves aside, never emptied" }
    elseif ($n -gt 0) { $r.Add("steps\runs\" + (Split-Path (Split-Path $paths.Evidence -Parent) -Leaf) + "\" + (Split-Path $paths.Evidence -Leaf) + " already holds files, and evidence is never written over. Nothing was written") }
  }
  return ,$r
}

# =======================================================================================
# Listings and backups of Bader's folders. Reading only, the copies go under the loop folder.
function ListFolder($dir) {
  $r = [pscustomobject]@{ Ok = $false; Why = ""; Missing = $false; Entries = New-Object System.Collections.Generic.List[object] }
  if (-not (Test-Path -LiteralPath $dir)) { $r.Missing = $true; $r.Ok = $true; return $r }
  $ev = $null
  $files = @(Get-ChildItem -LiteralPath $dir -Recurse -File -Force -ErrorAction SilentlyContinue -ErrorVariable ev)
  if (@($ev).Count -gt 0) { $r.Why = "the folder could not be listed whole, " + @($ev)[0].Exception.Message; return $r }
  foreach ($f in @($files | Sort-Object FullName)) {
    $h = $null
    try { $h = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256 -ErrorAction Stop).Hash } catch { $r.Why = "its file " + $f.FullName.Substring($dir.Length).TrimStart('\') + " could not be read, " + (Err $_.Exception); return $r }
    $r.Entries.Add([pscustomobject]@{ Rel = $f.FullName.Substring($dir.Length).TrimStart('\'); Name = $f.Name; Full = $f.FullName; Length = $f.Length; WriteUtc = $f.LastWriteTimeUtc; Hash = $h; Attributes = [string]$f.Attributes })
  }
  $r.Ok = $true
  return $r
}
function EntryLine($e) { return ($e.Rel + "`t" + $e.Length + "`t" + $e.WriteUtc.ToString("o") + "`t" + $e.Hash + "`t" + $e.Attributes) }
# The two lines of a listing that are not a file, its header and the line for a folder that
# is not there. Any other line is a file, a file whose name starts with # too.
function ListingHeader { return "# relative path`tbytes`twritten UTC`tsha256`tattributes" }
function ListingNoFolder { return "# the folder is not there" }
# What a backup folder holds, by file name and sha256, wherever it sits in the folder, each
# with the full path of one copy.
function BackupIndex($backupRoot) {
  $have = @{}
  $l = ListFolder $backupRoot
  if (-not $l.Ok) { throw ("the backup folder " + (Mask $backupRoot) + ", " + $l.Why) }
  foreach ($e in $l.Entries) { $have[$e.Name.ToLowerInvariant() + "|" + $e.Hash] = $e.Full }
  return $have
}
# Checks 13 and 14. Lists the folder into the listing file, then copies every file the backup
# does not already hold, by name and sha256, into since-<when> and reads it back.
function BackupNew($srcDir, $backupRoot, $listFile, $when) {
  $r = [pscustomobject]@{ Ok = $false; Why = ""; Listed = 0; Copied = 0; Into = $null }
  $have = $null
  try { $have = BackupIndex $backupRoot } catch { $r.Why = (Err $_.Exception); return $r }
  $l = ListFolder $srcDir
  if (-not $l.Ok) { $r.Why = $l.Why; return $r }
  $lines = New-Object System.Collections.Generic.List[string]
  $lines.Add((ListingHeader))
  if ($l.Missing) { $lines.Add((ListingNoFolder)) }
  $into = Join-Path $backupRoot ("since-" + $when)
  foreach ($e in $l.Entries) {
    $lines.Add((EntryLine $e))
    if ($have.ContainsKey($e.Name.ToLowerInvariant() + "|" + $e.Hash)) { continue }
    $to = Join-Path $into $e.Rel
    $back = $null
    try {
      New-Item -ItemType Directory -Force -Path (Split-Path $to -Parent) | Out-Null
      Copy-Item -LiteralPath $e.Full -Destination $to -ErrorAction Stop
      $back = (Get-FileHash -LiteralPath $to -Algorithm SHA256 -ErrorAction Stop).Hash
    } catch { $r.Why = $e.Rel + " could not be copied, " + (Err $_.Exception); return $r }
    if ($back -ne $e.Hash) { $r.Why = $e.Rel + " was copied but reads back with another sha256"; return $r }
    $have[$e.Name.ToLowerInvariant() + "|" + $e.Hash] = $true
    $r.Copied++
    $r.Into = $into
  }
  [System.IO.File]::WriteAllLines($listFile, $lines.ToArray(), (New-Object System.Text.UTF8Encoding($false)))
  $r.Listed = $l.Entries.Count
  $r.Ok = $true
  return $r
}
# The fields of one tab separated line, the one place run.ps1 splits a line at its tabs: a
# listing's columns in ListingRows, and since F104 part 2 a row of the tool's .tsv in ReadPairs.
# The comma hands the array back whole, so a line with no tab is still one field in an array.
function TabFields($line) { return ,([string]$line).Split("`t") }
# THE ONE READER of a listing EntryLine wrote, logs-before.txt, logs-after.txt,
# autosave-before.txt and installed.txt: relative path, bytes, written UTC, sha256 and
# attributes, one line each. Returns an object per line, with the line itself as Line.
# F104 part 2: $lines, when given, are read in place of the file's, which is how ReadPairs
# reads the file lines of outputs.txt once the lines starting # that it writes are left out.
function ListingRows($listFile, $lines) {
  $rows = New-Object System.Collections.Generic.List[object]
  if ($null -eq $lines) { $lines = [System.IO.File]::ReadAllLines($listFile) }
  foreach ($l in $lines) {
    if ($l -ceq (ListingHeader) -or $l -ceq (ListingNoFolder) -or $l -eq "") { continue }
    $x = TabFields $l
    if ($x.Count -lt 4) { throw ("a line of " + (Mask $listFile) + " cannot be read, `"" + $l + "`"") }
    $w = [DateTime]::Parse($x[2], [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind)
    $rows.Add([pscustomobject]@{ Rel = $x[0]; Name = (Split-Path $x[0] -Leaf); Length = [long]$x[1]; WriteUtc = $w; Hash = $x[3]; Line = $l })
  }
  return ,$rows
}
# A listing read back into the shape SettingsRead gives, each relative path under $prefix.
# The AutoSave compare at the end reads autosave-before.txt this way, so it compares with
# what was written down before the start.
function ReadListing($listFile, $prefix) {
  $map = @{}
  foreach ($row in (ListingRows $listFile)) { $map[$prefix + $row.Rel] = [pscustomobject]@{ Hash = $row.Hash; Length = $row.Length; Write = $row.WriteUtc.ToLocalTime() } }
  return $map
}
# The AutoSave listing for the compare at the end, read in a try of its own, so no report
# read can stop a write back. What it reads back is held against the backup's own list,
# made just after it, by count and by name, because a listing cut short reads as whole.
# When it cannot be read or the two differ, the backup's list stands in, and Why says so
# and names every file in one and not the other.
function AutoSaveBefore($listFile, $fallback) {
  $r = [pscustomobject]@{ Map = $null; Why = "" }
  try { $r.Map = ReadListing $listFile "AutoSave\" } catch { $r.Why = "autosave-before.txt could not be read back, " + (Err $_.Exception) + ", so the AutoSave compare reads the backup's list made at the same time instead"; $r.Map = $fallback; return $r }
  $onlyList = @($r.Map.Keys | Where-Object { -not $fallback.ContainsKey($_) } | Sort-Object)
  $onlyBackup = @($fallback.Keys | Where-Object { -not $r.Map.ContainsKey($_) } | Sort-Object)
  if ($r.Map.Count -ne $fallback.Count -or $onlyList.Count -gt 0 -or $onlyBackup.Count -gt 0) {
    $r.Why = "autosave-before.txt reads back " + $r.Map.Count + " files and the backup's list holds " + $fallback.Count + ", in the listing only: " + $(if ($onlyList.Count -gt 0) { $onlyList -join ", " } else { "none" }) + ", in the backup's list only: " + $(if ($onlyBackup.Count -gt 0) { $onlyBackup -join ", " } else { "none" }) + ". So the AutoSave compare reads the backup's list instead"
    $r.Map = $fallback
  }
  return $r
}
# One file put back as it was before the run, the write PutBackAutoSave and PutBackTeamMap
# share. F131 moved it out of PutBackAutoSave with its lines unchanged but for the backup's
# name. $b and $a are what the compare read before the run and at the end, each with its Hash,
# null where the file was not there, and $src is the backup's copy of $b, null when the backup
# holds none. The Roamers are listed again and the file is read again just before the write,
# so a file that no longer reads what the compare read is left, and the write is read back, a
# removal by the file being gone and a copy by its sha256. Stopped is true when a Roamer runs,
# and the caller writes nothing after it.
function PutBackOne($k, $what, $b, $a, $dest, $src, $backupName) {
  $r = [pscustomobject]@{ Line = ""; Done = $false; Stopped = $false }
  $rs = @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)
  if ($rs.Count -gt 0) { $r.Stopped = $true; $r.Line = "a Roamer is running just before the write of " + $k + ", pid " + (($rs | ForEach-Object { [string]$_.Id }) -join ", ") + ". It and every write after it are stopped"; return $r }
  try {
    if ($null -eq $b) {
      if (-not (Test-Path -LiteralPath $dest)) { $r.Line = $k + " " + $what + " is gone already, nothing to remove"; $r.Done = $true; return $r }
      if ((Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash -ne $a.Hash) { $r.Line = $k + " " + $what + " no longer reads what the compare read, not removed"; return $r }
      [System.IO.File]::Delete($dest)
      if (Test-Path -LiteralPath $dest) { $r.Line = $k + " " + $what + " is still there after its removal" } else { $r.Line = $k + " " + $what + ", removed, read back gone"; $r.Done = $true }
      return $r
    }
    if ($null -eq $src) { $r.Line = $k + " " + $what + ", " + $backupName + " holds no copy of it from before the run, left as it is"; return $r }
    if ($null -ne $a) {
      if ((Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash -ne $a.Hash) { $r.Line = $k + " " + $what + " no longer reads what the compare read, not written"; return $r }
    } else {
      if (Test-Path -LiteralPath $dest) { $r.Line = $k + " " + $what + " is back just before the copy, not written"; return $r }
      if (-not (Test-Path -LiteralPath (Split-Path $dest -Parent))) { $r.Line = $k + " " + $what + ", its folder went too, not written"; return $r }
    }
    Copy-Item -LiteralPath $src -Destination $dest -Force
    $back = (Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash
    if ($back -eq $b.Hash) { $r.Line = $k + " " + $what + ", put back from " + $backupName + ", read back, sha256 matches"; $r.Done = $true } else { $r.Line = $k + " " + $what + ", put back but reads back sha256 " + $back.Substring(0, 12) }
  } catch { $r.Line = $k + " " + $what + " could not be put back, " + (Err $_.Exception) }
  return $r
}
# F106, Q86. His AutoSave folder at the end of a run. When the put back's reasons are all clear,
# which needs the adopted Navisworks gone and no Navisworks the loop did not start seen from
# the backup to then, each autosave the run added is removed, and each of his the run changed
# or removed is copied back from autosave-backup, each through PutBackOne. When a reason
# stands, nothing is written and each is listed. $before is keyed AutoSave\<path>, as
# AutoSaveBefore reads it. Left counts every file not as it was before the run.
function PutBackAutoSave($putBack, $before, $autoDir, $backupRoot) {
  $r = [pscustomobject]@{ Lines = New-Object System.Collections.Generic.List[string]; Done = 0; Left = 0 }
  $now = ListFolder $autoDir
  if (-not $now.Ok) { $r.Lines.Add("his AutoSave folder could not be read whole at the end, " + $now.Why + ". Nothing is written into it"); $r.Left = 1; return $r }
  $after = @{}
  foreach ($e in $now.Entries) { $after["AutoSave\" + $e.Rel] = $e }
  $index = $null
  $stopped = $false
  foreach ($k in @(@($before.Keys) + @($after.Keys) | Sort-Object -Unique)) {
    $b = $before[$k]; $a = $after[$k]
    if ($null -ne $b -and $null -ne $a -and $a.Hash -eq $b.Hash) { continue }
    $what = $(if ($null -eq $b) { "ADDED by the run" } elseif ($null -eq $a) { "GONE" } else { "CHANGED" })
    if (-not $putBack -or $stopped) { $r.Lines.Add($k + " " + $what + ", left as it is, nothing written"); $r.Left++; continue }
    $src = $null
    if ($null -ne $b) {
      try { if ($null -eq $index) { $index = BackupIndex $backupRoot }; $src = $index[(Split-Path $k -Leaf).ToLowerInvariant() + "|" + $b.Hash] }
      catch { $r.Lines.Add($k + " " + $what + " could not be put back, " + (Err $_.Exception)); $r.Left++; continue }
    }
    $one = PutBackOne $k $what $b $a (Join-Path $autoDir $k.Substring("AutoSave\".Length)) $src "autosave-backup"
    $r.Lines.Add($one.Line)
    if ($one.Done) { $r.Done++ } else { $r.Left++ }
    if ($one.Stopped) { $stopped = $true }
  }
  return $r
}
# F131, Q123 answered B. team-map.txt beside his logs, the one map the tool keeps for a run with
# no clash XML, the FileName of src\Federator.Core\Teams\TeamMapMemory.cs, which H19 of
# prove-run.ps1 reads off the source so the two never differ. It is the second choice the tool
# remembers between runs, after FolderMemory's folders.txt, and a window run that picks an XML
# rewrites it, so it is read before every start and put back after it, .claude\rules\loop.md.
function TeamMapName { return "team-map.txt" }
# The start of the one line of team-map.txt that names the kept map, KeptMarker of
# TeamMapMemory.cs, which H19 K2 of prove-run.ps1 reads off the source so the two never differ.
function TeamMapKept { return "kept:" }
# F131 attempt 3, the breaker's finding on attempt 2. The paths of Bader's a window run's log can
# name outside its TEAMS KEPT block: his logs folder, which the log names as its own and the
# TEAMS line of a run that keeps a map names, and the map each team-map.txt handed in keeps, the
# text after TeamMapKept and one space, which the TEAMS lines of a run with no clash XML name.
# A file that is not there names nothing. One that cannot be read throws, so the log is never
# copied with that map's path left in it.
function PathsOfHis($hisLogs, $teamMaps) {
  $logs = ([string]$hisLogs).TrimEnd('\')
  if ($logs -eq "") { throw "his logs folder is not named, so the lines naming it cannot be masked" }
  $r = New-Object System.Collections.Generic.List[string]
  $r.Add($logs)
  $marker = (TeamMapKept) + " "
  foreach ($f in @($teamMaps)) {
    if (-not (Test-Path -LiteralPath $f)) { continue }
    foreach ($l in (ReadShared $f).Split("`n")) {
      $l = $l.TrimEnd("`r")
      if (-not $l.StartsWith($marker, [StringComparison]::Ordinal)) { continue }
      $p = $l.Substring($marker.Length)
      if ($p -ne "" -and -not ($r -contains $p)) { $r.Add($p) }
    }
  }
  return $r.ToArray()
}
# Before the start: the file copied into the run folder's teammap and read back by its sha256,
# or named as not there, when no copy is made. Not Ok, with Why, stops the run.
function TeamMapBefore($hisLogs, $runDir) {
  $r = [pscustomobject]@{ Ok = $false; Why = ""; There = $false; Hash = $null; Line = "" }
  $name = TeamMapName
  $file = Join-Path $hisLogs $name
  if (-not (Test-Path -LiteralPath $file)) { $r.Ok = $true; $r.Line = $name + " is not there before the start, so one the run writes is taken out again after it"; return $r }
  $copy = Join-Path (Join-Path $runDir "teammap") $name
  try {
    $h = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
    New-Item -ItemType Directory -Force -Path (Split-Path $copy -Parent) | Out-Null
    Copy-Item -LiteralPath $file -Destination $copy
    $back = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
    $len = (Get-Item -LiteralPath $copy).Length
  } catch { $r.Why = $name + " could not be read or copied into the run folder, " + (Err $_.Exception); return $r }
  if ($back -ne $h) { $r.Why = $name + " was copied into the run folder but reads back with another sha256"; return $r }
  $r.Ok = $true; $r.There = $true; $r.Hash = $h
  $r.Line = $name + " is there, sha256 " + $h + ", " + $len + " bytes, copied into the run folder's teammap and read back with that sha256"
  return $r
}
# After the run: team-map.txt as it was before the start. When the put back's reasons are all
# clear, a file the run changed or took out is copied back from the run folder's teammap and
# one it added is taken out, through PutBackOne. When a reason stands nothing is written and
# the file is named. Left is 1 when the file is not as it was before the start.
function PutBackTeamMap($putBack, $before, $hisLogs, $copyDir) {
  $r = [pscustomobject]@{ Lines = New-Object System.Collections.Generic.List[string]; Done = 0; Left = 0 }
  $k = TeamMapName
  $dest = Join-Path $hisLogs $k
  $a = $null
  try { if (Test-Path -LiteralPath $dest) { $a = [pscustomobject]@{ Hash = (Get-FileHash -LiteralPath $dest -Algorithm SHA256).Hash } } }
  catch { $r.Lines.Add($k + " could not be read at the end, " + (Err $_.Exception) + ". Nothing is written into it"); $r.Left = 1; return $r }
  $b = $null
  if ($before.There) { $b = [pscustomobject]@{ Hash = $before.Hash } }
  if ($null -eq $b -and $null -eq $a) { $r.Lines.Add($k + " was not there before the start and is not there now"); return $r }
  if ($null -ne $b -and $null -ne $a -and $a.Hash -eq $b.Hash) { $r.Lines.Add($k + " reads as it did before the start, sha256 " + $b.Hash); return $r }
  $what = $(if ($null -eq $b) { "ADDED by the run" } elseif ($null -eq $a) { "GONE" } else { "CHANGED" })
  if (-not $putBack) { $r.Lines.Add($k + " " + $what + ", left as it is, nothing written"); $r.Left = 1; return $r }
  $src = $null
  if ($null -ne $b) {
    try { $src = (BackupIndex $copyDir)[$k.ToLowerInvariant() + "|" + $b.Hash] }
    catch { $r.Lines.Add($k + " " + $what + " could not be put back, " + (Err $_.Exception)); $r.Left = 1; return $r }
  }
  $one = PutBackOne $k $what $b $a $dest $src "the run folder's teammap"
  $r.Lines.Add($one.Line)
  if ($one.Done) { $r.Done = 1 } else { $r.Left = 1 }
  return $r
}
# F106, Q82. What a window run left in his logs folder, from the rows of logs-before.txt and
# logs-after.txt. The tool's own log and tsv are the loop's, named in toollog-name.txt for the
# close of the loop. A file of his gone or changed is a change logs-backup covers when it holds
# that file's content from before the run by name and sha256, the oldest log the tool's own
# pruning takes among them, and LOST when it does not, which the verdict counts as not put
# back. Pure on its inputs.
function LogsAfterWindow($beforeRows, $afterRows, $toolNames, $backupHave) {
  $r = [pscustomobject]@{ Lines = New-Object System.Collections.Generic.List[string]; Lost = 0 }
  $after = @{}; foreach ($a in $afterRows) { $after[$a.Rel.ToLowerInvariant()] = $a }
  $before = @{}; foreach ($b in $beforeRows) { $before[$b.Rel.ToLowerInvariant()] = $b }
  foreach ($b in $beforeRows) {
    $a = $after[$b.Rel.ToLowerInvariant()]
    if ($null -ne $a -and $a.Hash -eq $b.Hash) { continue }
    $what = $(if ($null -eq $a) { "GONE" } else { "CHANGED" })
    if ($backupHave.ContainsKey($b.Name.ToLowerInvariant() + "|" + $b.Hash)) { $r.Lines.Add($what + " " + $b.Rel + ", logs-backup holds it as it was before the run" + $(if ($what -eq "GONE" -and $b.Name -like "run-*.log") { ", the tool's own pruning of its oldest logs, Q82" } else { ", a FINDING" })) }
    else { $r.Lost++; $r.Lines.Add("LOST " + $b.Rel + ", " + $what.ToLowerInvariant() + ", and logs-backup holds no copy of it from before the run") }
  }
  foreach ($a in $afterRows) {
    if ($before.ContainsKey($a.Rel.ToLowerInvariant())) { continue }
    if (@($toolNames) -contains $a.Name) { $r.Lines.Add("ADDED " + $a.Rel + ", the loop's own, named in toollog-name.txt for the close of the loop, Q82") }
    else { $r.Lines.Add("ADDED " + $a.Rel + ", not the log or tsv read as the loop's, a FINDING, left as it is") }
  }
  return $r
}

# =======================================================================================
# F104 part 2, the documents read. Each function here reads, or writes only under the
# documents read's own run folder, and says what it could not do rather than pass over it.

# A path in the text column of the tool's .tsv. RunLog writes every field through
# EventRow.Escape, src\Federator.Core\Diagnostics\EventRow.cs, which doubles a backslash and
# writes a tab, a return and a line break as a backslash and a letter. No file path holds a
# tab, a return or a line break, so a doubled backslash is read as one and any other backslash
# makes the text no path at all, null. EventRow.Unescape is not copied here.
function TsvPath($text) {
  $t = [string]$text
  $sb = New-Object System.Text.StringBuilder
  for ($i = 0; $i -lt $t.Length; $i++) {
    if ($t[$i] -ne [char]92) { [void]$sb.Append($t[$i]); continue }
    if ($i + 1 -lt $t.Length -and $t[$i + 1] -eq [char]92) { [void]$sb.Append([char]92); $i++; continue }
    return $null
  }
  return $sb.ToString()
}
# Whether $list holds $path, compared as Windows compares paths. A plain foreach, never @() on
# a List, CpuUsedSince.
function HasPath($list, $path) {
  foreach ($x in $list) { if ([string]::Equals($x, $path, [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
# A path as a key, its letters compared as Windows compares a name. The paths keyed are the
# full paths the tool, run.ps1 and read-workbook.ps1 wrote, so they are compared as written,
# and one spelled another way is not found, which ends in a refusal and never in a wrong pair.
function PathKey($path) { return ([string]$path).ToLowerInvariant() }
# The last line of a file that is not empty, or empty when there is none.
function LastLine($lines) {
  for ($i = $lines.Count - 1; $i -ge 0; $i--) { if ($lines[$i] -ne "") { return $lines[$i] } }
  return ""
}

# The pairs a documents read reads, off the evidence of one window run, steps\runs\NN\<run>,
# reading only. The run's .tsv, named on the second line of its toollog-name.txt, gives every
# NWF and workbook the run wrote, in the rows RunLog.WriteFinished writes, event written and
# name NWF or XLSX, src\Federator.Core\Diagnostics\RunLog.cs, paired by the group column and
# never by a file name. The columns are found by their header words. A workbook's read-out is
# the file in workbooks whose first line names that workbook, as read-workbook.ps1 writes it,
# and it must end whole. Each NWF must read now as the run's outputs.txt lists it, by size and
# sha256, so what is read is the NWF the run wrote beside its workbook, and no later run and
# no person has changed it since. Why holds every refusal. Pairs holds each pair in the order
# the .tsv first names its group, with Workbook null for a group that wrote no workbook.
# NoNwf names each group that wrote a workbook and no NWF, which has no document to read.
function ReadPairs($paths) {
  $r = [pscustomobject]@{ Why = New-Object System.Collections.Generic.List[string]; Pairs = New-Object System.Collections.Generic.List[object]; NoNwf = New-Object System.Collections.Generic.List[string]; Verdict = ""; Tsv = $null; TsvName = ""; TsvHash = ""; Rows = 0 }
  $tail = ". Nothing was started and nothing was written"
  $ev = $paths.Evidence
  $evName = "steps\runs\" + (Split-Path (Split-Path $ev -Parent) -Leaf) + "\" + (Split-Path $ev -Leaf)
  if (-not (Test-Path -LiteralPath $ev -PathType Container)) { $r.Why.Add($evName + " is not there, so there is no window run whose documents could be read" + $tail); return $r }
  $rec = Join-Path $ev "record.txt"
  if (-not (Test-Path -LiteralPath $rec -PathType Leaf)) { $r.Why.Add($evName + " holds no record.txt, so whether its run finished is UNKNOWN" + $tail); return $r }
  $v = @([System.IO.File]::ReadAllLines($rec) | Where-Object { $_.StartsWith("VERDICT") })
  if ($v.Count -ne 1) { $r.Why.Add($evName + "\record.txt holds " + $v.Count + " VERDICT lines and not one, so whether its run finished is UNKNOWN" + $tail); return $r }
  $r.Verdict = $v[0]
  $tn = Join-Path $ev "toollog-name.txt"
  $tsvName = ""
  if (Test-Path -LiteralPath $tn -PathType Leaf) { $tl = @([System.IO.File]::ReadAllLines($tn)); if ($tl.Count -ge 2) { $tsvName = $tl[1].Trim() } }
  if ($tsvName -notmatch '^[^\\/:*?"<>|]+\.tsv$') { $r.Why.Add($evName + "\toollog-name.txt does not name the tool's .tsv on its second line, so the run's own record of what it wrote cannot be found" + $tail); return $r }
  $tsv = Join-Path $ev $tsvName
  if (-not (Test-Path -LiteralPath $tsv -PathType Leaf)) { $r.Why.Add($evName + " holds no " + $tsvName + ", the .tsv its toollog-name.txt names" + $tail); return $r }
  $r.Tsv = $tsv; $r.TsvName = $tsvName
  $r.TsvHash = (Get-FileHash -LiteralPath $tsv -Algorithm SHA256).Hash
  $lines = [System.IO.File]::ReadAllLines($tsv)
  if ($lines.Count -eq 0) { $r.Why.Add($tsvName + " is empty" + $tail); return $r }
  $head = TabFields $lines[0]
  $col = @{}
  for ($i = 0; $i -lt $head.Count; $i++) { if (-not $col.ContainsKey($head[$i])) { $col[$head[$i]] = $i } }
  foreach ($w in @("group", "event", "name", "text")) { if (-not $col.ContainsKey($w)) { $r.Why.Add($tsvName + " has no column " + $w + " on its first line, so its rows cannot be read" + $tail); return $r } }
  $groups = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
  $order = New-Object System.Collections.Generic.List[string]
  for ($n = 1; $n -lt $lines.Count; $n++) {
    if ($lines[$n] -eq "") { continue }
    $f = TabFields $lines[$n]
    if ($f.Count -ne $head.Count) { $r.Why.Add($tsvName + " line " + ($n + 1) + " holds " + $f.Count + " fields and its first line " + $head.Count + ", so the file cannot be read whole" + $tail); return $r }
    $r.Rows++
    if ($f[$col["event"]] -cne "written") { continue }
    $kind = $f[$col["name"]]
    if ($kind -cne "NWF" -and $kind -cne "XLSX") { continue }
    $g = $f[$col["group"]]
    if ($g -eq "") { $r.Why.Add($tsvName + " line " + ($n + 1) + " names an " + $kind + " written outside any group, so it belongs to no pair" + $tail); continue }
    $path = TsvPath $f[$col["text"]]
    if ($null -eq $path -or -not [System.IO.Path]::IsPathRooted($path)) { $r.Why.Add($tsvName + " line " + ($n + 1) + " names an " + $kind + " written whose text is not a full path" + $tail); continue }
    if (-not $groups.ContainsKey($g)) { $groups[$g] = [pscustomobject]@{ Nwf = New-Object System.Collections.Generic.List[string]; Xlsx = New-Object System.Collections.Generic.List[string] }; $order.Add($g) }
    $list = $groups[$g].Xlsx; if ($kind -ceq "NWF") { $list = $groups[$g].Nwf }
    if (-not (HasPath $list $path)) { $list.Add($path) }
  }
  # What the run left in its output folders, from outputs.txt, keyed by full path. Its lines
  # that start with # are run.ps1's own and every other line is a file, under the copy or,
  # for item 5, under the window run's open folder.
  $outMap = $null
  $outFile = Join-Path $ev "outputs.txt"
  if (-not (Test-Path -LiteralPath $outFile -PathType Leaf)) { $r.Why.Add($evName + " holds no outputs.txt, so whether each NWF is still the one the run wrote cannot be read" + $tail) }
  else {
    $outMap = @{}
    $fileLines = @([System.IO.File]::ReadAllLines($outFile) | Where-Object { -not $_.StartsWith("#") })
    foreach ($row in (ListingRows $outFile $fileLines)) {
      $base = $paths.Copy
      if ($row.Rel.StartsWith("open\", [StringComparison]::OrdinalIgnoreCase) -and $null -ne $paths.SourceRunDir) { $base = $paths.SourceRunDir }
      $outMap[(PathKey (Join-Path $base $row.Rel))] = $row
    }
  }
  # Every read-out in workbooks, keyed by the workbook its first line names.
  $wbMap = @{}; $wbTwice = @{}
  $wbDir = Join-Path $ev "workbooks"
  if (Test-Path -LiteralPath $wbDir -PathType Container) {
    foreach ($wf in @(Get-ChildItem -LiteralPath $wbDir -File -Filter "*.txt" -Force | Sort-Object Name)) {
      $all = [System.IO.File]::ReadAllLines($wf.FullName)
      if ($all.Count -eq 0 -or -not $all[0].StartsWith("workbook ")) { continue }
      $key = PathKey $all[0].Substring("workbook ".Length)
      if ($wbMap.ContainsKey($key)) { $wbTwice[$key] = $true; continue }
      $wbMap[$key] = [pscustomobject]@{ Path = $wf.FullName; Name = $wf.Name; Whole = ((LastLine $all) -ceq "END OF READ-OUT") }
    }
  }
  $stems = @{}
  foreach ($g in $order) {
    $e = $groups[$g]
    if ($e.Nwf.Count -eq 0) { $r.NoNwf.Add($g); continue }
    if ($e.Nwf.Count -gt 1) { $r.Why.Add("group " + $g + " names " + $e.Nwf.Count + " NWFs written, " + (($e.Nwf | ForEach-Object { Mask $_ }) -join " and ") + ", so which one its workbook was written beside is UNKNOWN" + $tail); continue }
    if ($e.Xlsx.Count -gt 1) { $r.Why.Add("group " + $g + " names " + $e.Xlsx.Count + " workbooks written, so which one was written beside its NWF is UNKNOWN" + $tail); continue }
    $nwf = $e.Nwf[0]
    $pair = [pscustomobject]@{ Group = $g; Nwf = $nwf; Name = [System.IO.Path]::GetFileName($nwf); Stem = [System.IO.Path]::GetFileNameWithoutExtension($nwf); Xlsx = $null; Workbook = $null; WorkbookName = ""; Length = $null; Hash = "" }
    if ($e.Xlsx.Count -eq 1) { $pair.Xlsx = $e.Xlsx[0] }
    $r.Pairs.Add($pair)
    $at = "group " + $g + " names the NWF " + (Mask $nwf) + ", and "
    $pr = PathRefusal $nwf $paths.LoopRoot
    if ($null -eq $pr -and $null -ne (UnderRefusal $nwf $paths.Copy) -and ($null -eq $paths.SourceRunDir -or $null -ne (UnderRefusal $nwf $paths.SourceRunDir))) { $pr = "it does not resolve under the run set's copy or the window run's own folder" }
    if ($null -ne $pr) { $r.Why.Add($at + $pr + $tail); continue }
    if (-not $nwf.EndsWith(".nwf", [StringComparison]::OrdinalIgnoreCase)) { $r.Why.Add($at + "it is not an .nwf" + $tail); continue }
    if (-not (Test-Path -LiteralPath $nwf -PathType Leaf)) { $r.Why.Add($at + "it is not there" + $tail); continue }
    if ($stems.ContainsKey($pair.Stem)) { $r.Why.Add($at + "group " + $stems[$pair.Stem] + " names an NWF of the same file name, and the probe names each read-out after its NWF, so it would refuse both" + $tail); continue }
    $stems[$pair.Stem] = $g
    $pair.Length = (Get-Item -LiteralPath $nwf).Length
    $pair.Hash = (Get-FileHash -LiteralPath $nwf -Algorithm SHA256).Hash
    if ($null -ne $outMap) {
      $o = $outMap[(PathKey $nwf)]
      if ($null -eq $o) { $r.Why.Add($at + "outputs.txt of the run does not list it, so whether it is the NWF the run wrote is UNKNOWN" + $tail) }
      elseif ($o.Length -ne $pair.Length -or $o.Hash -ne $pair.Hash) { $r.Why.Add($at + "it reads " + $pair.Length + " bytes, sha256 " + $pair.Hash + " now, where outputs.txt of the run lists " + $o.Length + " bytes, sha256 " + $o.Hash + ", so it is not the NWF the run wrote beside its workbook. A later run or a person changed it" + $tail) }
    }
    if ($null -ne $pair.Xlsx) {
      $wk = PathKey $pair.Xlsx
      $wbAt = "group " + $g + " wrote the workbook " + (Mask $pair.Xlsx) + ", and "
      if ($wbTwice.ContainsKey($wk)) { $r.Why.Add($wbAt + "two read-outs in workbooks name it, so which is its read-out is UNKNOWN" + $tail) }
      elseif (-not $wbMap.ContainsKey($wk)) { $r.Why.Add($wbAt + "no read-out in workbooks names it, so it has nothing to compare with" + $tail) }
      elseif (-not $wbMap[$wk].Whole) { $r.Why.Add($wbAt + "its read-out workbooks\" + $wbMap[$wk].Name + " does not end in END OF READ-OUT, so read-workbook.ps1 could not read it whole" + $tail) }
      else { $pair.Workbook = $wbMap[$wk].Path; $pair.WorkbookName = $wbMap[$wk].Name }
    }
  }
  if ($r.Pairs.Count -eq 0 -and $r.Why.Count -eq 0) { $r.Why.Add($tsvName + " names no NWF written in any group, so there is nothing to read" + $tail) }
  return $r
}

# The probe a documents read loads, read with FileVersionInfo, which loads and locks nothing.
# Its stamp must name one commit, its second word eight hex characters with no +edits, which
# Directory.Build.targets writes only for a tree whose git status prints nothing, so every
# read-out it writes names a probe that can be read back at that commit. Why is the refusal,
# or null.
function ProbeRead($dll) {
  $r = [pscustomobject]@{ Why = $null; Stamp = ""; Hash = ""; Length = 0 }
  if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) { $r.Why = "there is no probe at " + (Mask $dll) + ". Build it with dotnet build tools\probes\DocumentReadProbe\DocumentReadProbe.csproj -c Release from a tree whose git status prints nothing. Nothing was started and nothing was written"; return $r }
  $r.Stamp = [string][System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion
  $r.Hash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
  $r.Length = (Get-Item -LiteralPath $dll).Length
  $words = @($r.Stamp -split '\s+')
  if ($words.Count -lt 2 -or $words[1] -cnotmatch '^[0-9a-f]{8}$') { $r.Why = "the probe reads the stamp " + $r.Stamp + ", which names no one commit with no edits, so its read-outs could not be read back at a commit. Build it from a tree whose git status prints nothing. Nothing was started and nothing was written" }
  return $r
}

# What the probe has written so far into the document folder, read every pass for the hang
# rule as a window run reads the tool's log: the bytes of every file in it. Each .partial is
# read through a handle that shares read, write and delete, because a listing can show the
# size a file had when its writer opened it, and the delete share lets the probe move a
# finished read-out into place while it is read. Partial counts the read-outs still being
# written, Whole those in place, and Lines names each file and its bytes. Why says what could
# not be read, and then Bytes is null, which restarts both clocks.
function ReadOutProgress($dir) {
  $r = [pscustomobject]@{ Bytes = $null; Partial = 0; Whole = 0; Lines = New-Object System.Collections.Generic.List[string]; Why = $null }
  try {
    $bytes = [long]0
    foreach ($f in @(Get-ChildItem -LiteralPath $dir -File -Force -ErrorAction Stop | Sort-Object Name)) {
      $len = $f.Length
      if ($f.Name.EndsWith(".partial", [StringComparison]::OrdinalIgnoreCase)) {
        $r.Partial++
        $fs = New-Object System.IO.FileStream($f.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
        try { $len = $fs.Length } finally { $fs.Dispose() }
      } elseif ($f.Name.EndsWith("-document.txt", [StringComparison]::OrdinalIgnoreCase)) { $r.Whole++ }
      $bytes += $len
      $r.Lines.Add($f.Name + "`t" + $len + " bytes")
    }
    $r.Bytes = $bytes
  } catch { $r.Why = (Err $_.Exception) }
  return $r
}

# The probe's two calls into the adopted Navisworks, on the main thread, each named to the
# watchdog. AddPluginAssembly with the probe's copy in the run folder has the 120 s any call
# has. ExecuteAddInPlugin has no limit of its own, because it holds for as long as the probe
# reads, and the hang rule on the read-outs and the processor time, and the ceiling, end it
# instead. The parameters are the probe's: the document folder, then every NWF. Once neither
# call is still running, CallReturned tells the monitor, which ends the run when no read-out
# is still being written and the folder has been quiet. What came back is the probe's hint,
# and the read-outs are the proof. Fault says which call threw.
function ProbeCall($app, $sync, $dll, $docDir, $nwfs, [int]$limitSeconds, $probeId) {
  $r = [pscustomobject]@{ Added = $false; Called = $false; Returned = $null; Seconds = 0.0; Fault = "" }
  try {
    Say ("  " + [DateTime]::Now.ToString("HH:mm:ss.fff") + "  calling AddPluginAssembly(" + (Mask $dll) + ") on the main thread")
    $err = $null
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $sync.CallLimit = $limitSeconds; $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "AddPluginAssembly"
    try { $app.AddPluginAssembly($dll) } catch { $err = $_.Exception } finally { $sync.CallName = "" }
    Say ("  AddPluginAssembly " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.000") + " s")
    if ($null -ne $err) { $r.Fault = "AddPluginAssembly threw, " + (Err $err); Say ("    " + (Err $err)); return $r }
    $r.Added = $true
    $params = [string[]](@($docDir) + @($nwfs))
    Say ("  " + [DateTime]::Now.ToString("HH:mm:ss.fff") + "  calling ExecuteAddInPlugin(" + $probeId + ", the document folder and " + @($nwfs).Count + " NWFs) on the main thread")
    $perr = $null; $pret = $null
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $sync.CallLimit = 0; $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "ExecuteAddInPlugin"
    $sync.LogSinceUtc = [DateTime]::UtcNow
    try { $pret = $app.ExecuteAddInPlugin($probeId, $params) } catch { $perr = $_.Exception } finally { $sync.CallName = ""; $sync.CallLimit = $limitSeconds }
    $r.Called = $true
    $r.Seconds = $sw.Elapsed.TotalSeconds
    Say ("  ExecuteAddInPlugin " + $(if ($null -eq $perr) { "RETURNED " + $pret } else { "THREW" }) + " after " + $r.Seconds.ToString("0.00") + " s. What it returned is the probe's hint, 0 every read-out whole, and the read-outs are the proof")
    if ($null -ne $perr) { $r.Fault = "ExecuteAddInPlugin threw, " + (Err $perr); Say ("    " + (Err $perr)) } else { $r.Returned = $pret }
    return $r
  } finally { $sync.CallReturned = $true }
}

# One listing with sha256 of every folder a documents read must leave as it is: the copy's NWF,
# NWD and Clash Report folders of -Folder, and any other folder that holds an NWF it reads,
# keyed by the folder. ListFolder makes each listing.
function FoldersSnap($folders) {
  $snap = [ordered]@{}
  foreach ($d in $folders) { if (-not $snap.Contains($d)) { $snap[$d] = ListFolder $d } }
  return $snap
}
# Every file of a snap, its sha256 by PathKey.
function SnapFiles($snap) {
  $m = @{}
  foreach ($d in $snap.Keys) { foreach ($e in $snap[$d].Entries) { $m[(PathKey (Join-Path $d $e.Rel))] = $e.Hash } }
  return $m
}
# What differs between two snaps of the same folders, by relative path and sha256: each file
# CHANGED with both sha256, ADDED or GONE, and each folder that was not read whole at either
# end. Files says each file's state by PathKey. Differ counts the files not as they were and
# Unread the folders not read whole. Pure on its inputs.
function FoldersDiff($before, $after) {
  $r = [pscustomobject]@{ Lines = New-Object System.Collections.Generic.List[string]; Differ = 0; Unread = 0; Same = 0; Files = @{} }
  foreach ($d in $before.Keys) {
    $b = $before[$d]; $a = $null; if ($after.Contains($d)) { $a = $after[$d] }
    if ($null -eq $a -or -not $b.Ok -or -not $a.Ok) { $r.Unread++; $r.Lines.Add((Mask $d) + " was not read whole at both ends, " + $(if (-not $b.Ok) { "before: " + $b.Why } elseif ($null -eq $a) { "after: it was not read" } else { "after: " + $a.Why })); continue }
    $bm = @{}; foreach ($e in $b.Entries) { $bm[$e.Rel] = $e }
    $am = @{}; foreach ($e in $a.Entries) { $am[$e.Rel] = $e }
    foreach ($k in @(@($bm.Keys) + @($am.Keys) | Sort-Object -Unique)) {
      $x = $bm[$k]; $y = $am[$k]; $key = PathKey (Join-Path $d $k)
      if ($null -ne $x -and $null -ne $y -and $x.Hash -eq $y.Hash) { $r.Same++; $r.Files[$key] = "unchanged, sha256 " + $x.Hash; continue }
      $r.Differ++
      if ($null -eq $x) { $r.Files[$key] = "ADDED, sha256 " + $y.Hash; $r.Lines.Add("ADDED " + (Mask (Join-Path $d $k)) + ", " + $y.Length + " bytes, sha256 " + $y.Hash) }
      elseif ($null -eq $y) { $r.Files[$key] = "GONE, it read sha256 " + $x.Hash; $r.Lines.Add("GONE " + (Mask (Join-Path $d $k)) + ", it read " + $x.Length + " bytes, sha256 " + $x.Hash) }
      else { $r.Files[$key] = "CHANGED, sha256 " + $x.Hash + " before and " + $y.Hash + " after"; $r.Lines.Add("CHANGED " + (Mask (Join-Path $d $k)) + ", sha256 " + $x.Hash + " before the start and " + $y.Hash + " after the close") }
    }
  }
  return $r
}

# The probe's read-out of one NWF in the document folder: whole when it ends in END OF
# READ-OUT, which the probe writes last and only once every step of it ran, and otherwise why.
function ReadOutState($docDir, $stem) {
  $f = Join-Path $docDir ($stem + "-document.txt")
  if (Test-Path -LiteralPath ($f + ".partial")) { return "NOT WHOLE, its .partial is still there, so the probe stopped while it wrote it" }
  if (-not (Test-Path -LiteralPath $f -PathType Leaf)) { return "NOT THERE, the probe wrote no read-out of it" }
  $all = [System.IO.File]::ReadAllLines($f)
  if ((LastLine $all) -ceq "END OF READ-OUT") { return "whole" }
  foreach ($l in $all) { if ($l.StartsWith("READ-OUT FAILED") -or $l.StartsWith("REFUSED")) { return ("NOT WHOLE, it says " + $l) } }
  return "NOT WHOLE, it does not end in END OF READ-OUT"
}

# compare-document.ps1 on one pair, as a child, with the switches the run was told. Returns its
# exit code and its own lines read off the comparison it wrote: the VERDICT with the three
# counts, or its COMPARISON FAILED line, or what it printed when it wrote none. A verdict is
# passed on and never judged here.
function ComparePair($repo, $workbook, $document, $out, $pictureStatuses, $pictureCap, [bool]$priorityPicked) {
  $psExe = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
  $a = "-NoProfile -ExecutionPolicy Bypass -File `"" + (Join-Path $repo "tools\loop\compare-document.ps1") + "`" -Workbook `"" + $workbook + "`" -Document `"" + $document + "`" -Out `"" + $out + "`""
  if ([string]$pictureStatuses -ne "") { $a += " -PictureStatuses " + $pictureStatuses }
  if ([string]$pictureCap -ne "") { $a += " -PictureCap " + $pictureCap }
  if ($priorityPicked) { $a += " -PriorityPicked" }
  $c = RunChild $psExe $a $repo
  $text = ""
  if (Test-Path -LiteralPath $out -PathType Leaf) {
    $lines = [System.IO.File]::ReadAllLines($out)
    $failed = @($lines | Where-Object { $_.StartsWith("COMPARISON FAILED") })
    if ($failed.Count -gt 0) { $text = $failed[0] }
    else {
      $verdict = @($lines | Where-Object { $_ -cmatch '^VERDICT [A-Z ]+$' })
      $counts = @($lines | Where-Object { $_ -cmatch '^(DISAGREEMENTS|NOT COMPARED|DOUBTS) \d+$' })
      if ($verdict.Count -eq 1) { $text = $verdict[0] + ", " + ($counts -join ", ") } else { $text = "UNKNOWN, the comparison holds " + $verdict.Count + " VERDICT lines" }
    }
  } else {
    $said = @($c.Out.Split("`n") | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" })
    if ($said.Count -gt 0) { $text = $said[0] } else { $text = "UNKNOWN, it wrote no comparison and printed nothing" }
  }
  return [pscustomobject]@{ Exit = $c.Exit; Text = $text; Pid = $c.Pid }
}

# summary.txt of a documents read: what was read and with what, one line per pair, the NWF after
# the close, its read-out and the comparison's own VERDICT line as compare-document.ps1 wrote it,
# a count of each verdict that judges none of them, the steps that did not run whole, and the
# run's VERDICT. Pure on what it is given, it returns the lines.
function SummaryLines($s) {
  $l = New-Object System.Collections.Generic.List[string]
  $l.Add("DOCUMENTS READ SUMMARY, run.ps1 -Mode Documents")
  $l.Add("run folder        " + (Mask $s.RunDir))
  $l.Add("window run read   steps\runs\" + $s.Set + "\" + $s.RunName + ", its record's " + $s.Read.Verdict)
  $l.Add("its .tsv          " + $s.Read.TsvName + ", sha256 " + $s.Read.TsvHash + ", " + $s.Read.Rows + " rows")
  $l.Add("probe             stamp " + $s.Probe.Stamp + ", sha256 " + $s.Probe.Hash)
  $call = "not made"
  if ($null -ne $s.Call) {
    if ($s.Call.Fault -ne "") { $call = $s.Call.Fault }
    elseif ($s.Call.Called) { $call = "ExecuteAddInPlugin returned " + $s.Call.Returned + " after " + $s.Call.Seconds.ToString("0.00") + " s, the probe's hint, 0 when every read-out is whole" }
  }
  $l.Add("plugin call       " + $call)
  $l.Add("the run ended     " + $(if ([string]$s.RunOver -ne "") { $s.RunOver } else { "UNKNOWN" }))
  $l.Add("switches          " + $(if ([string]$s.Switches -ne "") { $s.Switches } else { "none" }) + ", and -GroupClashesAt left at compare-document.ps1's own unknown, PQ4")
  $l.Add("")
  $l.Add("-- pairs: group, NWF, the NWF after the close, its read-out, the comparison")
  $tally = [ordered]@{ "VERDICT AGREE" = 0; "VERDICT DISAGREE" = 0; "VERDICT NOT PROVED" = 0; "COMPARISON FAILED" = 0; "NOT COMPARED" = 0; "UNKNOWN" = 0 }
  foreach ($p in $s.Read.Pairs) {
    $after = "UNKNOWN, the folder was not listed after the close"
    if ($null -ne $s.Diff) { $a = $s.Diff.Files[(PathKey $p.Nwf)]; if ($null -ne $a) { $after = $a } }
    $state = [string]$s.States[$p.Stem]; if ($state -eq "") { $state = "UNKNOWN, not read" }
    $c = [string]$s.Compared[$p.Stem]; if ($c -eq "") { $c = "UNKNOWN, not compared" }
    $l.Add($p.Group + "`t" + $p.Name + "`t" + $after + "`t" + $state + "`t" + $c)
    $k = "UNKNOWN"
    foreach ($t in @($tally.Keys)) { if ($c.StartsWith($t + ",") -or $c -ceq $t -or ($t -eq "COMPARISON FAILED" -and $c.StartsWith($t))) { $k = $t } }
    $tally[$k] = $tally[$k] + 1
  }
  foreach ($g in $s.Read.NoNwf) { $l.Add($g + "`t-`t-`t-`tNOT READ, the group wrote a workbook and no NWF") }
  $l.Add("")
  $l.Add("the comparisons' own verdicts, counted and none judged: " + ((@($tally.Keys) | ForEach-Object { $_ + " " + $tally[$_] }) -join ", "))
  if ($null -ne $s.Diff) {
    $l.Add("the copy's NWF, NWD and Clash Report folders after the close: " + $s.Diff.Same + " files read the same sha256, " + $s.Diff.Differ + " differ, " + $s.Diff.Unread + " folders not read whole")
    foreach ($x in $s.Diff.Lines) { $l.Add("  " + $x) }
  } else { $l.Add("the copy's NWF, NWD and Clash Report folders after the close: UNKNOWN, not listed") }
  $l.Add("steps that did not run whole: " + $(if ($s.DocCheck.Count -gt 0) { $s.DocCheck -join ", and " } else { "none" }))
  $l.Add("VERDICT: " + $s.Verdict)
  $l.Add("END OF SUMMARY")
  return ,$l
}

# =======================================================================================
# Keep awake, the session lock and what changed outside the loop folder while a start ran,
function KeepAwake($winType, [bool]$on) {
  $flags = [uint32]2147483648
  if ($on) { $flags = [uint32]2147483651 }
  $ret = $winType::SetThreadExecutionState($flags)
  return [pscustomobject]@{ Return = [uint32]$ret; Thread = [uint32]$winType::GetCurrentThreadId() }
}
function Hex($v) { return ("0x" + ([uint32]$v).ToString("X8")) }
# The write time of every key under one HKCU key, by name, read with RegQueryInfoKey.
function KeyTimes($winType, $sub) {
  $map = @{}
  $root = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($sub, $false)
  if ($null -eq $root) { return $map }
  try { KeyTimesWalk $winType $root $map } finally { $root.Close() }
  return $map
}
function KeyTimesWalk($winType, $k, $map) {
  [long]$ft = 0
  $z = [IntPtr]::Zero
  $rc = $winType::RegQueryInfoKeyW($k.Handle.DangerousGetHandle(), $z, $z, $z, $z, $z, $z, $z, $z, $z, $z, [ref]$ft)
  if ($rc -eq 0) { $map[$k.Name] = [DateTime]::FromFileTimeUtc($ft).ToString("o") } else { $map[$k.Name] = "UNKNOWN, RegQueryInfoKey returned " + $rc }
  $subs = @()
  try { $subs = @($k.GetSubKeyNames()) } catch { $map[$k.Name + "\*"] = "UNKNOWN, its subkeys could not be listed, " + (Err $_.Exception); return }
  foreach ($s in $subs) {
    $c = $null
    try { $c = $k.OpenSubKey($s, $false) } catch { $map[$k.Name + "\" + $s] = "UNKNOWN, it could not be opened, " + (Err $_.Exception); continue }
    if ($null -eq $c) { continue }
    try { KeyTimesWalk $winType $c $map } finally { $c.Close() }
  }
}
# Every file written at or after the call under the folders a start may write to, by any
# program, so not the start's alone. Named by its folder's label and its path inside it,
# never its full path.
function NewerFiles($roots, $sinceUtc) {
  $lines = New-Object System.Collections.Generic.List[string]
  foreach ($label in @($roots.Keys | Sort-Object)) {
    $dir = $roots[$label]
    if (-not (Test-Path -LiteralPath $dir)) { $lines.Add($label + " is not there"); continue }
    $ev = $null
    $files = @(Get-ChildItem -LiteralPath $dir -Recurse -File -Force -ErrorAction SilentlyContinue -ErrorVariable ev | Where-Object { $_.LastWriteTimeUtc -ge $sinceUtc -or $_.CreationTimeUtc -ge $sinceUtc })
    foreach ($f in @($files | Sort-Object FullName)) { $lines.Add($label + "\" + $f.FullName.Substring($dir.Length).TrimStart('\') + "`t" + $f.Length + " bytes`twritten " + $f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")) }
    if (@($ev).Count -gt 0) { $lines.Add($label + ": " + @($ev).Count + " folders or files could not be listed, so this list of it is not whole") }
    $lines.Add($label + ": " + $files.Count + " files written at or after the call")
  }
  return ,$lines
}

# =======================================================================================
# The hang rule, Q83: the tool's log has not grown for the limit, and over that same limit the
# adopted Navisworks used less processor time than $cpuLimitSeconds. HangVerdict is pure.
# run.ps1 always calls it with the constants 300 and 20, and an idle Navisworks was measured at
# 2 to 8 s in five minutes on 2026-09-30. The processor time is read off the samples: the one
# taken at or before the start of the limit and the newest, so the time read spans the limit
# or a little more, and a hang can only ever be called late, never early. With no sample that
# old there is no verdict. A sample that could not be read restarts both clocks, so an unknown
# never counts toward a hang.
function HangVerdict($lastLChangeUtc, $samples, $nowUtc, $limitSeconds, $cpuLimitSeconds) {
  if ($null -eq $lastLChangeUtc -or $null -eq $samples) { return $false }
  if (($nowUtc - $lastLChangeUtc).TotalSeconds -lt $limitSeconds) { return $false }
  $used = CpuUsedSince $samples $nowUtc.AddSeconds(-$limitSeconds)
  if ($null -eq $used) { return $false }
  return ($used -lt $cpuLimitSeconds)
}
# The processor seconds from the newest sample at or before $sinceUtc to the newest of all, or
# null when no sample is that old. The samples are read with a plain foreach, because @() on a
# List[object] throws Argument types do not match in Windows PowerShell 5.1, measured on
# 2026-10-01, and a plain foreach reads a list that is empty or null as no sample.
function CpuUsedSince($samples, $sinceUtc) {
  $base = $null; $last = $null
  foreach ($s in $samples) { if ($s.At -le $sinceUtc) { $base = $s }; $last = $s }
  if ($null -eq $base -or $null -eq $last) { return $null }
  return ([double]($last.C - $base.C) / 1e7)
}
# The verdict of a Run and its exit code, pure, from what the run recorded. The first that
# applies in the order refused, a fault before the constructor, 3 not adopted, 6 not put
# back, 4 hung, a window that WM_CLOSE did not close, a blocked call or the ceiling, 7 ended
# by itself, 1 UNKNOWN, still running at the end, a fault, or a window run whose log does not
# show it RAN, 8 the driver stopped before it pressed anything that runs, 5 a dialog, 0. The
# ceiling and a blocked call are read off what the watchdog forced, so a process it closed is
# never written as one that ended by itself, and never as HUNG. EndState is the adopted
# process's state read after every close path. F106: Item 1 to 5 is a window run, whose clean
# ends are CLOSED, the monitor's WM_CLOSE after the run's RESULT block, and DRIVER, its WM_CLOSE
# after the driver stopped, which is TOOL REFUSED, exit 0, only when the driver read the tool's
# own refusal of an NWD item 5 opened, OpenKind .nwd. The same refusal of an NWF is a stop, 8.
# LogCheck is empty only when the tool's log on disk shows the run RAN. F104 part 2: Documents
# is a documents read, whose clean end is READ, the monitor's end once the probe's calls have
# returned and the read-outs are quiet. DocCheck names every step of it that could not run, or
# that found the read wrote into the copy's folders, and is empty when none did, and DocText
# is what the RAN line says it did. A documents read is never a window run.
function RunVerdict($v) {
  $r = [pscustomobject]@{ Text = ""; Code = 1 }
  $end = [string]$v.EndState
  $docs = ($v.Documents -eq $true)
  $window = (-not $docs -and [string]$v.Item -match '^[1-5]$')
  $clean = @("HOLD"); if ($window) { $clean = @("CLOSED", "DRIVER") }; if ($docs) { $clean = @("READ") }
  if ($v.StopText -ne "") { $r.Text = "NOT RUN, " + $v.StopText; $r.Code = 2 }
  elseif (-not $v.Called) { $r.Text = "NOT RUN, a fault before the constructor, " + $v.Fault; $r.Code = 1 }
  elseif (-not $v.Adopted) { $r.Text = "NOT ADOPTED"; $r.Code = 3 }
  elseif ($v.NotPutBack) { $r.Text = "STOPPED, something of Bader's was not put back, NOT PUT BACK" + $(if ($end -ne "gone") { ", and the adopted Navisworks reads " + $end + " after every close path" } else { "" }); $r.Code = 6 }
  elseif ($v.RunOver -eq "HUNG") { $r.Text = "STOPPED, HUNG"; $r.Code = 4 }
  elseif ($v.RunOver -eq "END FORCED") { $r.Text = "STOPPED, " + $v.EndForced; $r.Code = 4 }
  elseif ([string]$v.CallForced -ne "") { $r.Text = "STOPPED, " + $v.CallForced; $r.Code = 4 }
  elseif ($v.RunOver -eq "CEILING" -or $v.Forced -ne "") { $r.Text = "STOPPED, CEILING, " + $v.Forced; $r.Code = 4 }
  elseif ($v.RunOver -eq "GONE") { $r.Text = "STOPPED, the adopted Navisworks ended by itself" + $(if ($window) { "" } elseif ($docs) { " before the read was over" } else { " before the hold" }); $r.Code = 7 }
  elseif ($v.RunOver -eq "BY ITSELF") { $r.Text = "STOPPED, WINDOW CLOSED BY ITSELF, " + $(if ($v.DriverName -eq "PRESSED") { "after the driver pressed Run" } else { "before the driver pressed Run" }); $r.Code = 7 }
  elseif ($v.RunOver -eq "UNKNOWN") { $r.Text = "STOPPED, UNKNOWN whether the adopted Navisworks still runs, it could not be read through the held handle"; $r.Code = 1 }
  elseif ($end -ne "gone") { $r.Text = "STOPPED, the adopted Navisworks reads " + $end + " after every close path, and is written down"; $r.Code = 1 }
  elseif ($v.Fault -ne "" -or $clean -notcontains $v.RunOver -or @($v.FinallyFaults).Count -gt 0) { $r.Text = ("STOPPED, a fault in run.ps1, " + $v.Fault + " " + $v.MonitorFault + " " + (@($v.FinallyFaults) -join ", and ")).Trim(); $r.Code = 1 }
  elseif ($docs -and [string]$v.DocCheck -ne "") { $r.Text = "STOPPED, the documents read did not do every step, " + $v.DocCheck; $r.Code = 1 }
  elseif ($window -and $v.RunOver -eq "CLOSED" -and [string]$v.LogCheck -ne "") { $r.Text = "STOPPED, the tool's log does not show the run RAN, " + $v.LogCheck; $r.Code = 1 }
  elseif ($window -and $v.RunOver -eq "DRIVER" -and -not ($v.DriverName -eq "TOOL REFUSED" -and [string]$v.OpenKind -eq ".nwd")) { $r.Text = "STOPPED, DRIVER " + $v.DriverName + " before anything that runs was pressed, " + $v.DriverText; $r.Code = 8 }
  else {
    if ($docs) { $r.Text = "RAN, the documents read, " + $v.DocText + ", closed" + $(if ($v.ClosedHere -ne "") { ", FORCED" } else { " by Dispose" }) + ", put back" }
    elseif (-not $window) { $r.Text = "RAN, item 0 with no window: started, adopted, held " + $v.HoldSeconds + " s, closed" + $(if ($v.ClosedHere -ne "") { ", FORCED" } else { " by Dispose" }) + ", put back" }
    elseif ($v.RunOver -eq "DRIVER") { $r.Text = "TOOL REFUSED, " + $v.DriverText }
    else { $r.Text = "RAN, item " + $v.Item + " through the window, the log's RESULT block read, closed" + $(if ($v.ClosedHere -ne "") { ", FORCED" } else { " by Dispose" }) + ", put back" }
    $r.Code = 0
    if ($v.Dialogs -gt 0) { $r.Code = 5; if (-not $window -and -not $docs) { $r.Text = "RAN, with " + $v.Dialogs + " DIALOG findings" } else { $r.Text = $r.Text + ", with " + $v.Dialogs + " DIALOG findings" } }
  }
  return $r
}
# The close at the end of a run. It takes the lock the watchdog holds for the whole of any
# close it forces, so a close the watchdog began has ended by the time this reads, and it
# marks the end as begun, so the watchdog forces nothing after it. A process that still
# reads same through the held handle is closed here, whether or not the watchdog forced a
# close before, and one already gone is never closed a second time. Text says which close
# ended it. Forced is the close text when this closed it.
function CloseAtEnd($sync, $ticks, [int]$waitSeconds) {
  $r = [pscustomobject]@{ Text = ""; Forced = "" }
  if ($null -eq $sync.MyProc) { return $r }
  [System.Threading.Monitor]::Enter($sync.CloseLock)
  try {
    $sync.EndClose = $true
    $already = ""
    if ($sync.Forced -ne "") { $already = $sync.Forced } elseif ($sync.CallForced -ne "") { $already = $sync.CallForced }
    $held = HeldRead $sync.MyProc $ticks
    if ($held.State -eq "same") {
      $cr = CloseAdopted $sync.MyProc $ticks $waitSeconds
      $r.Forced = $cr.Text
      if ($already -ne "") { $r.Text = "the watchdog forced a close, " + $already + ", and the adopted process still read same after it, so it was CLOSED here through the held handle: " + $cr.Text }
      else { $r.Text = "the adopted process was still running at the end and was CLOSED here through the held handle: " + $cr.Text }
    }
    elseif ($held.State -eq "gone") { if ($already -ne "") { $r.Text = "the watchdog forced a close, " + $already + ", and the adopted process reads gone, so it is not closed a second time here" } }
    else { $r.Text = "the adopted process reads " + $held.State + " through the held handle at the end, " + $held.Why + ", so it is not closed here and is written down below" + $(if ($already -ne "") { ". The watchdog forced a close before, " + $already } else { "" }) }
  } finally { [System.Threading.Monitor]::Exit($sync.CloseLock) }
  return $r
}
# The two clocks of the hang rule: when the log's length last changed, and the processor time
# samples since the clocks last started. Both start again on a sample that could not be read,
# keeping the processor time of that sample when it was read, and so start again at that
# moment.
function NewClocks($nowUtc) { return [pscustomobject]@{ L = $null; LChange = $nowUtc; Samples = (New-Object System.Collections.Generic.List[object]); Unknown = $false } }
function NextClocks($clk, $L, $C, $nowUtc) {
  $n = [pscustomobject]@{ L = $clk.L; LChange = $clk.LChange; Samples = $clk.Samples; Unknown = $false }
  if ($null -eq $L -or $null -eq $C) {
    $n.Unknown = $true; $n.LChange = $nowUtc; $n.Samples = New-Object System.Collections.Generic.List[object]
    if ($null -ne $L) { $n.L = $L }
    if ($null -ne $C) { $n.Samples.Add([pscustomobject]@{ At = $nowUtc; C = [long]$C }) }
    return $n
  }
  if ($L -ne $clk.L) { $n.L = $L; $n.LChange = $nowUtc }
  $n.Samples.Add([pscustomobject]@{ At = $nowUtc; C = [long]$C })
  return $n
}

# The tool's log: every run-*.log in the logs folder that was not there before, was made at or
# after the call, and whose first line names its own path, each with the plugin version its
# SESSION block names, read off the first 400 lines, or null while that line is not written
# yet. F106: the monitor takes the one whose version names the installed stamp, and calls more
# than one UNKNOWN, because Bader may run the tool himself while a loop run goes.
function FindToolLog($folder, $before, $sinceUtc) {
  $found = New-Object System.Collections.Generic.List[object]
  if (-not (Test-Path -LiteralPath $folder)) { return ,$found }
  foreach ($f in @(Get-ChildItem -LiteralPath $folder -Filter "run-*.log" -File -Force)) {
    if ($before.ContainsKey($f.Name.ToLowerInvariant())) { continue }
    if ($f.CreationTimeUtc -lt $sinceUtc) { continue }
    $head = New-Object System.Collections.Generic.List[string]
    $fs = New-Object System.IO.FileStream($f.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
    try { $sr = New-Object System.IO.StreamReader($fs); while ($head.Count -lt 400) { $x = $sr.ReadLine(); if ($null -eq $x) { break }; $head.Add($x) } } finally { $fs.Dispose() }
    if ($head.Count -gt 0 -and $head[0].IndexOf("Log opened at " + $f.FullName, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $found.Add([pscustomobject]@{ Path = $f.FullName; Version = (LogPluginVersion $head) }) }
  }
  return ,$found
}
# The words of the tool's log a window run is judged by, read off RunLog.cs: Section writes a
# blank line, a line of 64 equals signs, the title and the equals signs again, and Line writes
# the time, two spaces, a plus, the seconds since the log opened to three places, s and two
# spaces before the text. Session writes plugin version : and the build stamp, the GROUPS
# block ends on UntickedGroupsLine, WriteResultBlock writes the RESULT section, and TryCopyTo
# writes a COPY line or the FAILURE of copying the log next to the NWF folder.
function LogText($line) { $m = [regex]::Match([string]$line, '^\d\d:\d\d:\d\d\.\d{3}  \+\d+\.\d{3}s  (.*)$'); if ($m.Success) { return $m.Groups[1].Value }; return $null }
function LogTitleAt($lines, $i) { return ($i -ge 1 -and $i + 1 -lt $lines.Count -and $lines[$i - 1] -match '^=+$' -and $lines[$i + 1] -match '^=+$') }
function LogPluginVersion($lines) {
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -cne "SESSION" -or -not (LogTitleAt $lines $i)) { continue }
    for ($j = $i + 2; $j -lt $lines.Count; $j++) {
      if ($lines[$j] -match '^=+$') { break }
      $t = LogText $lines[$j]
      if ($null -ne $t -and $t.StartsWith("plugin version : ")) { return $t.Substring("plugin version : ".Length) }
    }
  }
  return $null
}
# F106. Whether a window run's log on disk shows it RAN: a RESULT block, a SESSION whose plugin
# version names the installed stamp, and for item 1 a GROUPS block that reads no group
# unticked. Returns empty when all hold, else what failed. Pure on the lines.
function ToolLogVerdict($lines, $stamp, $item) {
  $why = New-Object System.Collections.Generic.List[string]
  $result = $false; $groups = $null
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if (-not (LogTitleAt $lines $i)) { continue }
    if ($lines[$i] -ceq "RESULT") { $result = $true }
    if ($lines[$i] -ceq "GROUPS") {
      $groups = "the GROUPS block holds no line of how many groups are unticked"
      for ($j = $i + 2; $j -lt $lines.Count; $j++) {
        if ($lines[$j] -match '^=+$' -or $lines[$j] -eq "") { break }
        $m = [regex]::Match([string](LogText $lines[$j]), '^(\d+) groups? unticked in the Run column, nothing else drops a group$')
        if ($m.Success) { $groups = [int]$m.Groups[1].Value }
      }
    }
  }
  if (-not $result) { $why.Add("it holds no RESULT block") }
  $pv = LogPluginVersion $lines
  if ($null -eq $pv) { $why.Add("its SESSION block names no plugin version") }
  elseif (-not (StampNames $pv $stamp)) { $why.Add("its SESSION plugin version reads " + $pv + ", which does not name " + $stamp) }
  if ($item -eq "1") {
    if ($null -eq $groups) { $why.Add("it holds no GROUPS block") }
    elseif ($groups -is [string]) { $why.Add($groups) }
    elseif ($groups -ne 0) { $why.Add("its GROUPS block reads " + $groups + " groups unticked") }
  }
  return ($why -join ", and ")
}
# F106, Q87. The tool's log as it goes into the evidence: the lines of its FOLDERS REMEMBERED
# block, the folders Bader's own pickers remember, written at every window open by
# FederatorWindow.xaml.cs through FolderMemory.Lines, each replaced by a line saying it was
# masked. F131, Q123: the lines of its TEAMS KEPT block too, written at the same open through
# TeamMap.Lines, which name the map his last run with an XML kept and its full path. A block's
# lines follow its title at once. A FOLDERS REMEMBERED line is a picker's name padded to ten
# characters and a folder, or the line that says nothing is remembered, and a TEAMS KEPT line
# starts TEAMS and four spaces. A line of another shape ends the block, unless it was written
# within a second of the block's first line, so a line of the block is never left unmasked
# because its shape was not foreseen. Masked counts the folder lines and MaskedTeams the others.
# F131 attempt 3, the breaker's finding on attempt 2: every other line naming one of $paths, in
# any case of its letters, wherever it sits, is masked too, its stamp or its indent kept, and
# MaskedPaths counts them. The run hands in PathsOfHis, his logs folder and the kept map, which
# the TEAMS lines of a run and the log's own lines name outside the TEAMS KEPT block.
function MaskRemembered($lines, $paths) {
  $names = @()
  if ($null -ne $paths) { $names = @(@($paths) | ForEach-Object { [string]$_ }) }
  foreach ($p in $names) { if ($p -eq "") { throw "MaskRemembered was handed an empty path, which every line would name" } }
  $out = New-Object System.Collections.Generic.List[string]
  $n = 0
  $nt = 0
  $np = 0
  $i = 0
  while ($i -lt $lines.Count) {
    $teams = ($lines[$i] -ceq "TEAMS KEPT")
    if (-not (($teams -or $lines[$i] -ceq "FOLDERS REMEMBERED") -and (LogTitleAt $lines $i))) {
      $l = [string]$lines[$i]
      $his = $false
      foreach ($p in $names) { if ($l.IndexOf($p, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $his = $true; break } }
      if ($his) {
        $m = [regex]::Match($l, '^(\d\d:\d\d:\d\d\.\d{3})  \+(\d+\.\d{3})s  ')
        if ($m.Success) { $out.Add($m.Groups[1].Value + "  +" + $m.Groups[2].Value + "s  <a line naming his logs folder or the kept team map, masked by run.ps1, F131>") }
        else { $out.Add([regex]::Match($l, '^\s*').Value + "<a line naming his logs folder or the kept team map, masked by run.ps1, F131>") }
        $np++
      } else { $out.Add($lines[$i]) }
      $i++
      continue
    }
    $out.Add($lines[$i])
    $out.Add($lines[$i + 1])
    $i += 2
    $first = $null
    while ($i -lt $lines.Count) {
      $m = [regex]::Match([string]$lines[$i], '^(\d\d:\d\d:\d\d\.\d{3})  \+(\d+\.\d{3})s  (.*)$')
      if (-not $m.Success) { break }
      $at = [double]::Parse($m.Groups[2].Value, [Globalization.CultureInfo]::InvariantCulture)
      if ($null -eq $first) { $first = $at }
      $t = $m.Groups[3].Value
      if ($teams) { $shape = $t.StartsWith("TEAMS    ") }
      else { $shape = ($t.Length -gt 10 -and $t.Substring(0, 10) -match '^[A-Za-z]+ +$' -and $t.Substring(10, 1) -ne " ") -or $t.StartsWith("Nothing remembered yet") }
      if (-not $shape -and ($at - $first) -gt 1.0) { break }
      if ($teams) { $out.Add($m.Groups[1].Value + "  +" + $m.Groups[2].Value + "s  <a line of the kept team map, masked by run.ps1, Q123>"); $nt++ }
      else { $out.Add($m.Groups[1].Value + "  +" + $m.Groups[2].Value + "s  <a remembered folder, masked by run.ps1, Q87>"); $n++ }
      $i++
    }
  }
  return [pscustomobject]@{ Lines = $out; Masked = $n; MaskedTeams = $nt; MaskedPaths = $np }
}
# F106. The driver's last line in its notes, which says how it ended.
function DriverLastLine($notes) {
  if ($null -eq $notes -or -not (Test-Path -LiteralPath $notes)) { return "UNKNOWN, the driver wrote no notes" }
  $l = @((ReadShared $notes).Split("`n") | ForEach-Object { $_.TrimEnd("`r") } | Where-Object { $_ -ne "" })
  if ($l.Count -eq 0) { return "UNKNOWN, the driver's notes are empty" }
  return $l[$l.Count - 1]
}
# F106. WM_CLOSE to the tool's window of the adopted Navisworks, the one place it is posted:
# only once the adopted process's start ticks read equal through the held handle, and to each
# window only after its own process id reads the adopted pid again. Returns what it did.
function PostClose($sync, $windows) {
  $said = New-Object System.Collections.Generic.List[string]
  $held = HeldRead $sync.MyProc $sync.MyTicks
  if ($held.State -ne "same") { $said.Add("WM_CLOSE NOT posted, the adopted process reads " + $held.State + ", " + $held.Why); return ,$said }
  foreach ($w in @($windows)) {
    [uint32]$wp = 0
    [void]$sync.WinType::GetWindowThreadProcessId($w.Handle, [ref]$wp)
    if ($wp -ne [uint32]$sync.MyPid) { $said.Add("WM_CLOSE NOT posted to window " + $w.Handle + ", it now belongs to process " + $wp + ", not the adopted one"); continue }
    $ok = $sync.WinType::PostMessageW($w.Handle, [uint32]16, [IntPtr]::Zero, [IntPtr]::Zero)
    $said.Add("WM_CLOSE posted to the tool's window " + $w.Handle + ", caption `"" + $w.Caption + "`", PostMessage returned " + $ok)
  }
  return ,$said
}

# The monitor, on its own runspace from adoption to the end of the run. It never touches the
# COM object. Every 15 s it reads the adopted process's processor time through the held
# handle, the tool's log once there is one, and the adopted process's windows, and once a
# minute the session lock. It ends the run at the first of: the process gone, a hang, the
# ceiling the watchdog closed, or item 0's fixed hold. F106, for a window run, $sync.Window:
# it reads the session lock at every pass and starts both clocks of the hang rule again at
# every pass while it reads anything but unlocked, Q85, reads the driver's exit through the
# driver's held handle, and ends the run when the tool's window closes: CLOSED or DRIVER after
# its own WM_CLOSE, BY ITSELF with none. It posts WM_CLOSE through PostClose once the log holds
# its RESULT block and a COPY line after it and has then been quiet for 15 s, or once the
# driver has stopped with nothing that runs pressed while the log shows no run started, and
# never while a window of the adopted process other than the tool's, the main window and a PANE
# is up, which is left up for the hang rule or the ceiling, and its line saying so names each
# window behind it with the rule's kind and both states. A PANE, F125, is written in the words
# PaneWords gives and never counted a finding. Each window is written at first sight and again,
# an AGAIN line, only when the rule's kind for it, its own enabled state or its owner's changes,
# and it counts one DIALOG finding the first time it reads DIALOG. The window takes WM_CLOSE between two
# groups of a run, because the run pumps the dispatcher and no Closing handler reads whether a
# run goes, FederatorWindow.xaml.cs Pump and OnClose, so a run that has started is only ever
# closed after its RESULT block. A tool's window still open 120 s after WM_CLOSE is closed
# through the held handle, why written first, END FORCED. The time a log must be made at or
# after is read at every pass, since the main thread sets it at the plugin call. F104 part 2,
# for a documents read, $sync.ReadFolder: the bytes of the read-outs in that folder stand for
# the tool's log in the hang rule, Q83, both clocks starting at the plugin call, and the run
# ends READ once the main thread says the probe's calls returned, no read-out is still being
# written and the folder has been quiet for 15 s. No WM_CLOSE is ever posted for it.
function Monitor($sync) {
  $landmark = '^\S+\s+\+\S+\s+(RETAIN|SESSION|RUN SETTINGS|GROUP|RESULT|COPY|RUN |plugin version|open document|FAILURE)|^(SESSION|RUN SETTINGS|GROUPS|RESULT|OPEN FILE)$|Window closed\.'
  function M($t) {
    $line = [DateTime]::Now.ToString("HH:mm:ss") + "  t+" + ([DateTime]::UtcNow - $sync.CallStartUtc).TotalSeconds.ToString("0") + "s  " + $t
    [Console]::Out.WriteLine($line)
    if (-not (RecordAppend $sync.RecordLock $sync.RecordFile $line)) { $sync.MonitorWriteFails = $sync.MonitorWriteFails + 1 }
  }
  $clk = $null
  $logPath = $null
  $stream = $null
  $offset = [long]0
  $pending = ""
  $tail = New-Object System.Collections.Generic.List[string]
  $seenWins = @{}
  $dialogWins = @{}
  $lastBeat = [DateTime]::UtcNow
  $lastLock = [DateTime]::MinValue
  $beatC = $null; $beatL = $null; $lastC = $null; $lastLine = ""
  $win = ($sync.Window -eq $true)
  $lockState = "unlocked"; $lastLen = $null; $lastGrowUtc = [DateTime]::UtcNow
  $prev = ""; $sawResult = $false; $sawCopy = $false; $sawClosedLine = $false; $runStarted = $false
  $candKey = ""; $toolSeen = $false; $posted = ""; $postedAt = [DateTime]::MinValue; $busyNoted = ""; $driverDone = $false; $startedNoted = $false
  $docs = ([string]$sync.ReadFolder -ne ""); $prog = $null; $readNoted = ""
  # Mask, which MaskLine calls, reads these two from its caller, as it does in the watchdog.
  $loopRoot = $sync.LoopRoot
  $nw = $sync.Nw
  M ("MONITOR started, a pass every " + $sync.PassSeconds + " s, hang limit " + $sync.HangLimit + " s" + $(if ($sync.ReadToolLog -or $docs) { " with under " + $sync.HangCpuSeconds + " s of processor time in it" } else { "" }) + ", fixed hold " + $sync.HoldSeconds + " s")
  # Item 0 calls no plugin, so any new log in the logs folder is Bader's by construction: no
  # tool's log is read and no hang clock starts on one. A window run reads as the tool's the
  # one log made after its plugin call whose SESSION names the installed stamp. A documents
  # read calls the probe alone, so any new log there is Bader's too, and its hang clocks run
  # on the read-outs.
  if ($docs) { M ("no tool's log is read, because this run calls the probe alone and never the tool, so any new log in the logs folder is Bader's. The read-outs the probe writes into " + (Mask $sync.ReadFolder) + " stand for the tool's log in the hang rule, Q83, and both clocks start at the plugin call") }
  elseif (-not $sync.ReadToolLog) { M "no tool's log is read and no hang clock starts, because this run calls no plugin, so any new log in the logs folder is Bader's" }
  try {
    while (-not $sync.MonitorStop) {
      $now = [DateTime]::UtcNow
      # The ceiling first: the watchdog's close ends the process, so a process read gone after
      # it is the ceiling's end and never one of its own.
      if ($sync.Forced -ne "") { M ("CEILING: " + $sync.Forced); $sync.RunOver = "CEILING"; break }
      $held = HeldRead $sync.MyProc $sync.MyTicks
      if ($held.State -ne "same") {
        if ($sync.Forced -ne "") { M ("CEILING: " + $sync.Forced); $sync.RunOver = "CEILING"; break }
        M ("the adopted process reads " + $held.State + " through the held handle, " + $held.Why + ", so the run ends")
        if ($held.State -eq "gone") { $sync.RunOver = "GONE" } else { $sync.RunOver = "UNKNOWN" }
        break
      }
      $c = $null
      try { $sync.MyProc.Refresh(); $c = $sync.MyProc.TotalProcessorTime.Ticks } catch { M ("processor time UNKNOWN, " + (Err $_.Exception)) }
      $lockNow = "unlocked"
      if ($win) {
        $lockNow = SessionLockText $sync.WinType
        if ($lockNow -ne $lockState) { M ("SESSION LOCK now reads " + $lockNow + $(if ($lockNow -ne "unlocked") { ", so both clocks of the hang rule start again at every pass while it reads so, and the loop waits, Q85" } else { "" })); $lockState = $lockNow }
      }
      # The time a log must be made at or after is read at every pass, because the main thread
      # sets it only when it makes the plugin call, after this monitor has started.
      $since = $sync.LogSinceUtc; if ($null -eq $since) { $since = $sync.CallStartUtc }
      if ($sync.ReadToolLog -and $null -eq $logPath -and $since -ne [DateTime]::MaxValue) {
        $found = $null
        try { $found = FindToolLog $sync.LogsFolder $sync.LogsBefore $since } catch { M ("the logs folder could not be read, UNKNOWN, " + (Err $_.Exception)) }
        if ($null -ne $found) {
          $mine = @($found | Where-Object { $null -ne $_.Version -and (StampNames $_.Version $sync.Stamp) })
          $key = (@($found | ForEach-Object { (Split-Path $_.Path -Leaf) + "=" + $_.Version }) -join "|")
          if ($mine.Count -eq 1) {
            $logPath = $mine[0].Path
            $sync.ToolLog = $logPath
            $tsvName = [System.IO.Path]::GetFileNameWithoutExtension($logPath) + ".tsv"
            [System.IO.File]::WriteAllText((Join-Path $sync.RunDir "toollog-name.txt"), (Split-Path $logPath -Leaf) + "`r`n" + $tsvName + "`r`n", (New-Object System.Text.UTF8Encoding($false)))
            $stream = New-Object System.IO.FileStream($logPath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
            $clk = NewClocks $now
            M ("the tool's log is found, " + (Split-Path $logPath -Leaf) + ", with its .tsv " + $tsvName + ", both named in toollog-name.txt. Its SESSION names " + $sync.Stamp + ". Both clocks of the hang rule start now")
          } elseif ($key -ne $candKey) {
            $candKey = $key
            if ($mine.Count -gt 1) {
              $sync.LogAmbiguous = $true
              $names = (@($mine | ForEach-Object { Split-Path $_.Path -Leaf }) -join ", ")
              [System.IO.File]::WriteAllText((Join-Path $sync.RunDir "toollog-name.txt"), "UNKNOWN, more than one log made after the plugin call names the installed stamp, so none is read as the loop's: " + $names + "`r`n", (New-Object System.Text.UTF8Encoding($false)))
              M ("FINDING: UNKNOWN which log is the loop's, " + $mine.Count + " new logs made after the plugin call name the installed stamp, " + $names + ". None is read as the tool's, so no hang clock starts and the end of the run is not read off a log")
            }
            foreach ($o in @($found | Where-Object { $null -ne $_.Version -and -not (StampNames $_.Version $sync.Stamp) })) { M ("FINDING: a new log made after the plugin call, " + (Split-Path $o.Path -Leaf) + ", whose SESSION plugin version reads " + $o.Version + ", which does not name " + $sync.Stamp + ", so it is not read as the loop's") }
          }
        }
      }
      $L = $null
      if ($null -ne $stream) {
        try {
          $L = $stream.Length
          if ($L -gt $offset) {
            $n = [int]($L - $offset)
            $buf = New-Object byte[] $n
            [void]$stream.Seek($offset, [System.IO.SeekOrigin]::Begin)
            $got = $stream.Read($buf, 0, $n)
            $offset = $offset + $got
            $text = $pending + [System.Text.Encoding]::UTF8.GetString($buf, 0, $got)
            $parts = $text.Split("`n")
            $pending = $parts[$parts.Count - 1]
            for ($i = 0; $i -lt $parts.Count - 1; $i++) {
              $ln = $parts[$i].TrimEnd("`r")
              $tail.Add($ln); if ($tail.Count -gt 200) { $tail.RemoveAt(0) }
              $lastLine = $ln
              if ($ln -match $landmark) { M ("LOG " + (MaskLine $ln)) }
              # A run has started once the log holds the RUN SETTINGS block OnRun writes after
              # the confirm, or a RUN or OPEN line, the open file run's first. From then on only
              # the RESULT block ends the run through WM_CLOSE, whatever the driver's exit says.
              if (-not $runStarted -and (($ln -ceq "RUN SETTINGS" -and $prev -match '^=+$') -or $ln -match '^\S+\s+\+\S+\s+(RUN      |OPEN     )')) { $runStarted = $true; M "the tool's log shows a run has started, so from here only its RESULT block ends the run through WM_CLOSE" }
              if ($ln -ceq "RESULT" -and $prev -match '^=+$') { $sawResult = $true }
              elseif ($sawResult -and $ln -match '^\S+\s+\+\S+\s+(COPY |FAILURE  copying the log next to the NWF folder)') { $sawCopy = $true }
              if ($ln -match '^\S+\s+\+\S+\s+Window closed\.$') { $sawClosedLine = $true }
              $prev = $ln
            }
          }
        } catch { $L = $null; M ("the log's length UNKNOWN, " + (Err $_.Exception)) }
        if ($null -ne $L -and $L -ne $lastLen) { $lastLen = $L; $lastGrowUtc = $now }
      }
      # F104 part 2. The read-outs stand for the tool's log. They are read once both clocks
      # have started at the plugin call, or once the probe's calls have returned, so the end
      # of the run is read even when the plugin call was never made.
      if ($docs) {
        if ($null -eq $clk -and $since -ne [DateTime]::MaxValue) { $clk = NewClocks $now; M "the plugin call is made, so both clocks of the hang rule start now, on the read-outs and the processor time" }
        if ($null -ne $clk -or $sync.CallReturned) {
          $prog = ReadOutProgress $sync.ReadFolder
          if ($null -ne $prog.Why) { M ("the read-outs could not be read, UNKNOWN, " + $prog.Why) }
          else { $L = $prog.Bytes; if ($L -ne $lastLen) { $lastLen = $L; $lastGrowUtc = $now } }
        }
      }
      if ($null -ne $clk) {
        if ($win -and $lockNow -ne "unlocked") { $clk = NewClocks $now }
        else {
          $clk = NextClocks $clk $L $c $now
          if ($clk.Unknown) { M "a sample could not be read, UNKNOWN, so both clocks start again" }
          if (HangVerdict $clk.LChange $clk.Samples $now $sync.HangLimit $sync.HangCpuSeconds) {
            $used = CpuUsedSince $clk.Samples $now.AddSeconds(-$sync.HangLimit)
            M ("HANG: " + $(if ($docs) { "the read-outs" } else { "the log" }) + " stood at " + $clk.L + " bytes since " + $clk.LChange.ToLocalTime().ToString("HH:mm:ss") + ", " + ($now - $clk.LChange).TotalSeconds.ToString("0") + " s, and the adopted Navisworks used " + $used.ToString("0.000") + " s of processor time in the last " + $sync.HangLimit + " s, under " + $sync.HangCpuSeconds + " s, Q83")
            $hangLines = New-Object System.Collections.Generic.List[string]
            foreach ($t in $tail) { $hangLines.Add($t) }
            if ($docs -and $null -ne $prog) { $hangLines.Add("---- the document folder at the hang ----"); foreach ($x in $prog.Lines) { $hangLines.Add($x) } }
            $hangLines.Add("---- the adopted process's visible windows at the hang ----")
            foreach ($w in (WindowLines $sync.WinType $sync.ProcType ([uint32]$sync.MyPid) $true $true)) { $hangLines.Add($w) }
            [System.IO.File]::WriteAllLines((Join-Path $sync.RunDir "hang-tail.txt"), $hangLines.ToArray(), (New-Object System.Text.UTF8Encoding($false)))
            $cr = CloseAdopted $sync.MyProc $sync.MyTicks 30
            M ("the adopted process, closed through the held handle for the hang: " + $cr.Text)
            $sync.RunOver = "HUNG"
            break
          }
        }
      }
      $cText = "UNKNOWN"; if ($null -ne $c) { $cText = ([double]$c / 1e7).ToString("0.000") + " s" }
      $grow = ""; if ($null -ne $c -and $null -ne $lastC) { $grow = ", " + ([double]($c - $lastC) / 1e7).ToString("0.000") + " s since the last pass" }
      $lText = ""; if ($null -ne $logPath) { $lText = ", the log " + $L + " bytes" }
      elseif ($docs -and $null -ne $prog -and $null -eq $prog.Why) { $lText = ", the read-outs " + $prog.Bytes + " bytes, " + $prog.Partial + " being written, " + $prog.Whole + " in place" }
      M ("SAMPLE processor " + $cText + $grow + $lText)
      $lastC = $c
      # WindowRecords returns its list as one object, so the call is put in brackets and piped,
      # which hands on each record. Wrapped bare in @() the list stayed one record, measured on
      # 2026-10-01 in Windows PowerShell 5.1, and the window rule threw on its arrays.
      $visRecs = @((WindowRecords $sync.WinType $sync.ProcType ([uint32]$sync.MyPid) $true $true) | ForEach-Object { $_ })
      # F125. A window is written at first sight and again only when the rule's kind for it, its
      # own enabled state or its owner's changes, so the record holds the pane's state while the
      # tool's window is up and stays bounded: a window whose reads hold still adds no line, and
      # one that changes adds at most one a pass. A window counts one DIALOG finding the first time
      # it reads DIALOG, at first sight or later, and never twice.
      foreach ($r in $visRecs) {
        $key = [string]$r.Handle + "|" + $r.Class + "|" + $r.Caption
        $kind = WindowKind $r.Class $r.Caption $r.OwnerHandle $r.OwnerVisible $r.OwnerEnabled $r.Enabled
        $state = $kind + "|" + [string]$r.Enabled + "|" + [string]$r.OwnerEnabled
        $again = ""
        if ($seenWins.ContainsKey($key)) {
          if ($seenWins[$key] -eq $state) { continue }
          $again = "AGAIN, its kind or state changed. It read " + (KindStateWords $seenWins[$key]) + " when last written, and reads " + (KindStateWords $state) + " now. "
        }
        $seenWins[$key] = $state
        if ($kind -eq "DIALOG" -and (IsConfirm $r.Class $r.Caption $r.Texts)) { $kind = "CONFIRM" }
        $texts = "UNKNOWN, no child window answered"
        if ($null -ne $r.Texts -and @($r.Texts).Count -gt 0) { $texts = (@($r.Texts) -join " ") }
        $ownerText = OwnerText $r ([uint32]$sync.MyPid)
        # A DIALOG line and a PANE line end with the window's own state, which the window rule
        # reads beside its owner's, so the record shows what decided between them.
        if ($kind -eq "DIALOG") {
          if (-not $dialogWins.ContainsKey($key)) { $dialogWins[$key] = $true; $sync.Dialogs = $sync.Dialogs + 1 }
          M ($again + "DIALOG: class " + $r.Class + ", caption `"" + $r.Caption + "`", " + $ownerText + ", text " + $texts + ", the window itself enabled " + $r.Enabled)
        } elseif ($kind -eq "CONFIRM") { M ($again + "CONFIRM, the tool's confirm, which the driver answers and is not a finding: class " + $r.Class + ", caption `"" + $r.Caption + "`", " + $ownerText + ", text " + $texts) }
        elseif ($kind -eq "PANE") { M ($again + "PANE, " + (PaneWords $r.OwnerEnabled) + ", left as it is and not a finding: class " + $r.Class + ", caption `"" + $r.Caption + "`", " + $ownerText + ", text " + $texts + ", the window itself enabled " + $r.Enabled) }
        else { M ($again + $kind + ": class " + $r.Class + ", caption `"" + $r.Caption + "`", " + $ownerText) }
      }
      if (($now - $lastLock).TotalSeconds -ge 60) { $lastLock = $now; M ("SESSION LOCK " + (SessionLockText $sync.WinType)) }
      if (($now - $lastBeat).TotalSeconds -ge $sync.BeatSeconds) {
        $lastBeat = $now
        $cg = ""; if ($null -ne $c -and $null -ne $beatC) { $cg = ", growth " + ([double]($c - $beatC) / 1e7).ToString("0.000") + " s" }
        $lg = ""; if ($null -ne $L -and $null -ne $beatL) { $lg = ", growth " + ($L - $beatL) + " bytes" }
        $noun = "the log"; if ($docs) { $noun = "the read-outs" }
        $still = "the clocks have not started, there is no tool's log"
        if ($docs) { $still = "the clocks have not started, the plugin call is not made yet" }
        if ($null -ne $clk) {
          $u = CpuUsedSince $clk.Samples $now.AddSeconds(-$sync.HangLimit)
          $still = $noun + " still for " + ($now - $clk.LChange).TotalSeconds.ToString("0") + " s against " + $sync.HangLimit + ", and the processor used " + $(if ($null -eq $u) { "an amount not known yet, the samples do not reach back that far," } else { $u.ToString("0.000") + " s" }) + " in the last " + $sync.HangLimit + " s against " + $sync.HangCpuSeconds
        }
        $lw = ""
        try { $wl = @((ReadShared $sync.WatchFile).Split("`n") | ForEach-Object { $_.TrimEnd("`r") } | Where-Object { $_ -ne "" }); if ($wl.Count -gt 0) { $lw = $wl[$wl.Count - 1] } } catch { $lw = "UNKNOWN, " + (Err $_.Exception) }
        $ll = $lastLine; if ($ll.Length -gt 120) { $ll = $ll.Substring(0, 120) }
        $lastText = ", its last line `"" + (MaskLine $ll) + "`""; if ($docs) { $lastText = "" }
        M ("HEARTBEAT " + $noun + " " + $(if ($null -ne $L) { [string]$L + " bytes" + $lg } else { "none" }) + $lastText + ", processor " + $cText + $cg + ", " + $still + ", the watchdog's last line: " + $lw)
        $beatC = $c; $beatL = $L
      }
      # F104 part 2. The end of a documents read: the probe's calls have returned, no read-out
      # is still being written, and the read-outs have been quiet for 15 s. A read-out still
      # being written after the calls returned is waited for, and the hang rule or the ceiling
      # decides when it never ends.
      if ($docs -and $sync.CallReturned -and $null -ne $prog -and $null -eq $prog.Why) {
        $quiet = ($now - $lastGrowUtc).TotalSeconds
        if ($prog.Partial -eq 0 -and $quiet -ge $sync.QuietSeconds) { M ("READ: the probe's calls have returned, no read-out is still being written, and the read-outs have not grown for " + $quiet.ToString("0") + " s, " + $prog.Whole + " read-outs in place, so the run is over"); $sync.RunOver = "READ"; break }
        if ($prog.Partial -gt 0 -and $readNoted -eq "") { $readNoted = "partial"; M ("the probe's calls have returned and " + $prog.Partial + " read-outs are still being written, so the run waits for them, and the hang rule or the ceiling decides if they never end") }
      }
      if ($win) {
        if (-not $driverDone -and $null -ne $sync.DriverProc) {
          $dx = $null
          try { if ($sync.DriverProc.HasExited) { $dx = [int]$sync.DriverProc.ExitCode } } catch { M ("the driver's exit could not be read, UNKNOWN, " + (Err $_.Exception)) }
          if ($null -ne $dx) { $driverDone = $true; $sync.DriverExit = $dx; $sync.DriverLine = DriverLastLine $sync.DriverNotes; M ("DRIVER exited " + $dx + ", " + (DriverCodeName $dx) + ", its last line: " + $sync.DriverLine) }
        }
        $toolNow = @((WindowRecords $sync.WinType $sync.ProcType ([uint32]$sync.MyPid) $false $false) | Where-Object { (WindowKind $_.Class $_.Caption $_.OwnerHandle $_.OwnerVisible $_.OwnerEnabled $_.Enabled) -eq "WINDOW" })
        if (-not $toolSeen -and $toolNow.Count -gt 0) { $toolSeen = $true; M ("the tool's window is open, caption `"" + $toolNow[0].Caption + "`"") }
        if (($toolSeen -and $toolNow.Count -eq 0) -or $sawClosedLine) {
          $how = $(if ($sawClosedLine) { "Window closed. is in the tool's log" } else { "the tool's window is gone" })
          if ($posted -eq "") { M ("WINDOW CLOSED BY ITSELF, " + $how + ", and WM_CLOSE was never posted. The driver had " + $(if ($sync.DriverExit -eq $sync.DriverPressed) { "pressed Run" } else { "not pressed Run" })); EndArm $sync; $sync.RunOver = "BY ITSELF"; break }
          M ("the tool's window is closed, " + $how + ", " + ($now - $postedAt).TotalSeconds.ToString("0") + " s after WM_CLOSE")
          EndArm $sync
          if ($posted -eq "RESULT") { $sync.RunOver = "CLOSED" } else { $sync.RunOver = "DRIVER" }
          break
        }
        if ($posted -eq "") {
          $why = ""
          if ($sawResult -and $sawCopy -and ($now - $lastGrowUtc).TotalSeconds -ge $sync.QuietSeconds) { $why = "RESULT" }
          elseif ($driverDone -and $sync.DriverExit -ne $sync.DriverPressed) {
            if (-not $runStarted) { $why = "DRIVER" }
            elseif (-not $startedNoted) { $startedNoted = $true; M ("the driver stopped, " + (DriverCodeName $sync.DriverExit) + ", after the tool's log shows a run has started, so WM_CLOSE is not posted for the driver's stop. It waits for the RESULT block, and the hang rule or the ceiling decides if none comes") }
          }
          if ($why -ne "") {
            $what = $(if ($why -eq "RESULT") { "the tool's log holds its RESULT block and a COPY line after it and has been quiet for " + ($now - $lastGrowUtc).TotalSeconds.ToString("0") + " s" } else { "the driver stopped, " + (DriverCodeName $sync.DriverExit) + ", with nothing that runs pressed" })
            $busy = @($visRecs | Where-Object { $k = WindowKind $_.Class $_.Caption $_.OwnerHandle $_.OwnerVisible $_.OwnerEnabled $_.Enabled; $k -ne "WINDOW" -and $k -ne "MAIN" -and $k -ne "PANE" })
            if ($toolNow.Count -eq 0) {
              M ("the run is over, " + $what + ", and no window of the tool is open to close")
              EndArm $sync
              if ($why -eq "RESULT") { $sync.RunOver = "CLOSED" } else { $sync.RunOver = "DRIVER" }
              break
            } elseif ($busy.Count -gt 0) {
              # F125. The windows behind it, each with the rule's kind and both states, and the
              # line written again only when those windows or their reads change.
              $behind = New-Object System.Collections.Generic.List[string]
              foreach ($b in $busy) { $behind.Add("window " + ($behind.Count + 1) + ", class " + $b.Class + ", caption `"" + $b.Caption + "`", the window rule reads it " + (WindowKind $b.Class $b.Caption $b.OwnerHandle $b.OwnerVisible $b.OwnerEnabled $b.Enabled) + ", " + (StateWords $b)) }
              $busyNow = $why + "|" + ($behind -join ". ")
              if ($busyNoted -ne $busyNow) { $busyNoted = $busyNow; M ("the run is over, " + $what + ", and " + $busy.Count + " windows of the adopted Navisworks that are not the tool's, the main window or a PANE are up, so WM_CLOSE is not posted, they are left as they are, and the hang rule or the ceiling decides. The windows behind it: " + ($behind -join ". ")) }
            } else {
              foreach ($pl in (PostClose $sync $toolNow)) { M $pl }
              $posted = $why; $postedAt = $now
              M ("WM_CLOSE was posted because " + $what + ". The window has " + $sync.EndCloseSeconds + " s to close")
            }
          }
        } elseif (($now - $postedAt).TotalSeconds -ge $sync.EndCloseSeconds) {
          [System.Threading.Monitor]::Enter($sync.CloseLock)
          try {
            if (-not $sync.EndClose -and $sync.Forced -eq "" -and $sync.CallForced -eq "") {
              $sync.EndForced = "the tool's window was still open " + $sync.EndCloseSeconds + " s after WM_CLOSE, the close through the held handle has begun"
              M ("END FORCED: " + $sync.EndForced)
              $cr = CloseAdopted $sync.MyProc $sync.MyTicks 30
              $sync.EndForced = "the tool's window was still open " + $sync.EndCloseSeconds + " s after WM_CLOSE, " + $cr.Text
              M ("END FORCED: " + $sync.EndForced)
            } elseif ([string]$sync.EndForced -eq "") { $sync.EndForced = "the tool's window was still open " + $sync.EndCloseSeconds + " s after WM_CLOSE, and another close had begun" }
          } finally { [System.Threading.Monitor]::Exit($sync.CloseLock) }
          $sync.RunOver = "END FORCED"
          break
        }
      }
      if ($sync.HoldSeconds -gt 0 -and ($now - $sync.AdoptedAtUtc).TotalSeconds -ge $sync.HoldSeconds) { M ("the fixed hold of " + $sync.HoldSeconds + " s from adoption is reached"); $sync.RunOver = "HOLD"; break }
      $until = $now.AddSeconds($sync.PassSeconds)
      while (-not $sync.MonitorStop -and [DateTime]::UtcNow -lt $until) { Start-Sleep -Milliseconds 200 }
    }
  } catch {
    $sync.MonitorFault = (Err $_.Exception)
    M ("MONITOR FAULT: " + $sync.MonitorFault)
    if ($sync.RunOver -eq "") { $sync.RunOver = "FAULT" }
  } finally { if ($null -ne $stream) { $stream.Dispose() } }
  M "MONITOR stopped"
}
# F106. Once the monitor has ended a window run, a plugin call that has not returned has the
# 120 s a call may take from now, and past them the watchdog closes the adopted process through
# the held handle, which ends the call. The call's name stays the main thread's to clear.
function EndArm($sync) {
  if ([string]$sync.CallName -ne "ExecuteAddInPlugin") { return }
  $sync.CallSinceUtc = [DateTime]::UtcNow
  $sync.CallLimit = $sync.EndCloseSeconds
}
# What the window rule rests on, for every window it classes: its owner's class, caption,
# visibility, state and process, read without a message. An owner of another process is
# masked, its class, caption and process left out, so no other program's window reaches
# the record.
function OwnerText($r, [uint32]$myPid) {
  if ($r.OwnerHandle -eq [IntPtr]::Zero) { return "owner none" }
  if ($r.OwnerPid -ne $myPid) { return ("owner " + $r.Owner + ", a window of another process, its class, caption and process not written") }
  return ("owner " + $r.Owner + ", class " + $r.OwnerClass + ", caption `"" + $r.OwnerCaption + "`", visible " + $r.OwnerVisible + ", enabled " + $r.OwnerEnabled + ", process the adopted one")
}
# F125. A window's reads as the monitor keeps them, the rule's kind, its own enabled state and its
# owner's, joined by a bar, in the words of an AGAIN line.
function KindStateWords($state) {
  $p = ([string]$state).Split("|")
  if ($p[2] -eq "none") { return ($p[0] + " with the window itself enabled " + $p[1] + " and no owner") }
  return ($p[0] + " with the window itself enabled " + $p[1] + " and its owner enabled " + $p[2])
}
# The text a runspace runs to be the monitor: this file's functions and the guard's.
function MonitorScript { return 'param($sync) . ([scriptblock]::Create($sync.GuardText)); . ([scriptblock]::Create($sync.RunText)); Monitor $sync' }
# The top level function definitions of run.ps1, for the monitor's runspace.
function FunctionText($file) {
  $t = $null; $e = $null
  $ast = [System.Management.Automation.Language.Parser]::ParseFile($file, [ref]$t, [ref]$e)
  if ($e.Count -gt 0) { throw ("run.ps1 does not parse, " + $e[0].Message) }
  return ((@($ast.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] }) | ForEach-Object { $_.Extent.Text }) -join "`r`n")
}

# =======================================================================================
# CloseOwn. Closes the loop's own Navisworks after run.ps1 died, only when every check holds.
# Writes nothing of Bader's: the settings compare is listed, never put back.
function CloseOwn($runFolder, $roamerPath) {
  $rec = Join-Path $runFolder "record.txt"
  if (-not (Test-Path -LiteralPath $rec)) { Say ("REFUSED: " + (Mask $runFolder) + " has no record.txt, so nothing there was proved the loop's own. Nothing was closed. A person has to look"); return 2 }
  if (HasVerdict $rec) { Say ("REFUSED: " + (Mask $runFolder) + " has a VERDICT line, so its run finished. Nothing was closed"); return 2 }
  $pidFile = Join-Path $runFolder "mypid.txt"
  $pl = @()
  if (Test-Path -LiteralPath $pidFile) { $pl = @([System.IO.File]::ReadAllLines($pidFile)) }
  [int]$cpid = 0; [long]$cticks = 0
  if ($pl.Count -lt 3 -or $pl[2].Trim() -ne "adopted" -or -not [int]::TryParse($pl[0].Trim(), [ref]$cpid) -or -not [long]::TryParse($pl[1].Trim(), [ref]$cticks)) { Say ("REFUSED: " + (Mask $runFolder) + " has no mypid.txt reading adopted, so nothing there was proved the loop's own. Nothing was closed. A person has to look"); return 2 }
  $script:RecordFile = $rec
  Say ""
  Say ("==== CLOSEOWN " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + " ====")
  $p = Get-Process -Id $cpid -ErrorAction SilentlyContinue
  $miss = $null
  if ($null -eq $p) { $miss = "no process holds that pid" }
  elseif ($p.ProcessName -ne "Roamer") { $miss = "its name is " + $p.ProcessName }
  if ($null -eq $miss) {
    $cr = CimRead $cpid
    if (-not $cr.Ok -or $null -eq $cr.Row) { $miss = "its path and command line could not be read" }
    elseif (-not [string]::Equals([string]$cr.Row.ExecutablePath, $roamerPath, [StringComparison]::OrdinalIgnoreCase)) { $miss = "its path is " + (Mask $cr.Row.ExecutablePath) }
    elseif (-not (AutomationForm $cr.Row.CommandLine)) { $miss = "its command line is not the automation form" }
  }
  if ($null -eq $miss) {
    try { if ((UtcTicks $p.StartTime) -ne $cticks) { $miss = "its start ticks differ" } } catch { $miss = "its start time could not be read, " + (Err $_.Exception) }
  }
  if ($null -eq $miss) {
    try { [void]$p.Handle; if ((UtcTicks $p.StartTime) -ne $cticks) { $miss = "its start ticks read through its handle differ" } } catch { $miss = "its handle could not be opened, " + (Err $_.Exception) }
  }
  if ($null -ne $miss) { Say ("REFUSED: pid " + $cpid + " is not the Navisworks this folder adopted, " + $miss + ". Nothing was closed"); return 2 }
  $cl = CloseAdopted $p $cticks 30
  Say ("  pid " + $cpid + ": " + $cl.Text)
  if ($cl.State -ne "gone") { Say "  it is not gone, so nothing more is done here and no VERDICT is written"; return 1 }
  Say "---- Bader's settings, compared with the run's backup, NOTHING WRITTEN, because no whole watchdog record exists after run.ps1 died ----"
  $bf = Join-Path $runFolder "settings\before.clixml"
  if (-not (Test-Path -LiteralPath $bf)) { Say "  the run's backup was not finished, so nothing is compared" }
  else {
    $b = Import-Clixml -LiteralPath $bf
    $dr = DiffRegistry $b.RegBefore (RegRead $b.RegSub) $b.RegRoot
    foreach ($l in $dr.Lines) { Say ("  " + $l) }
    $sa = SettingsRead $b.NwAppData
    $df = DiffFiles $b.FilesBefore $b.NotBacked $sa $b.NwAppData
    foreach ($l in $df.Lines) { Say ("  " + $l) }
    $abFile = Join-Path $runFolder "autosave-before.txt"
    if (Test-Path -LiteralPath $abFile) { foreach ($l in (DiffAutoSave (ReadListing $abFile "AutoSave\") $sa)) { Say ("  " + $l) } }
    else { Say "  the run wrote no autosave-before.txt, so its AutoSave folder is not compared" }
    Say ("  registry: " + @($dr.Changes | Where-Object { $_.Op -ne "CreateKey" }).Count + " values or keys differ, files: " + $df.Changes.Count + " differ, nothing written")
  }
  Say "VERDICT: CLOSED BY CLOSEOWN"
  return 0
}

# A program run.ps1 starts, with its output read back and its pid and exit code returned.
function RunChild($file, $arguments, $workDir) {
  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = $file
  $psi.Arguments = $arguments
  $psi.UseShellExecute = $false
  $psi.CreateNoWindow = $true
  $psi.RedirectStandardOutput = $true
  $psi.RedirectStandardError = $true
  if ($null -ne $workDir) { $psi.WorkingDirectory = $workDir }
  $p = [System.Diagnostics.Process]::Start($psi)
  $o = $p.StandardOutput.ReadToEndAsync()
  $e = $p.StandardError.ReadToEndAsync()
  $p.WaitForExit()
  return [pscustomobject]@{ Exit = $p.ExitCode; Out = $o.Result; Err = $e.Result; Pid = $p.Id }
}

# Install. HEAD must be the commit asked for and the tree clean, untracked files included,
# because those are the two reads Directory.Build.targets makes for the stamp.
function TreeRefusal($repo, $stamp) {
  $why = New-Object System.Collections.Generic.List[string]
  $rh = RunChild "git" ("-C `"" + $repo + "`" rev-parse --short=8 HEAD") $null
  if ($rh.Exit -ne 0) { $why.Add("git rev-parse did not answer, exit " + $rh.Exit + ". Nothing was installed"); return ,$why }
  $head = $rh.Out.Trim()
  if ($head -cne $stamp) { $why.Add("HEAD is " + $head + ", not -Stamp " + $stamp + ". Nothing was installed") }
  $rs = RunChild "git" ("-C `"" + $repo + "`" status --porcelain") $null
  if ($rs.Exit -ne 0) { $why.Add("git status did not answer, exit " + $rs.Exit + ". Nothing was installed"); return ,$why }
  $st = @($rs.Out.Split("`n") | Where-Object { $_.Trim() -ne "" })
  if ($st.Count -gt 0) { $why.Add("git status --porcelain prints " + $st.Count + " lines, untracked files included, so the build stamp would read " + $head + "+edits and would not prove main. Install from a clean checkout of main. Nothing was installed") }
  return ,$why
}
function SameListing($a, $b) {
  if ($a.Count -ne $b.Count) { return $false }
  for ($i = 0; $i -lt $a.Count; $i++) { if ($a[$i].Rel -ne $b[$i].Rel -or $a[$i].Hash -ne $b[$i].Hash) { return $false } }
  return $true
}
# The verdict of an install that read back the stamp asked for. A Navisworks running right
# after it may have started during it and loaded either build, and a bundle left beside the
# new one may be loaded too, so each is a finding that changes the verdict and the exit code.
function InstallVerdict($stamp, [bool]$lateRoamer, $leftovers) {
  $left = @($leftovers | Where-Object { $_ })
  $findings = New-Object System.Collections.Generic.List[string]
  if ($lateRoamer) { $findings.Add("a Navisworks is running right after the install, so which build it loaded is UNKNOWN. Close it before any run") }
  if ($left.Count -gt 0) { $findings.Add("the add-in installed before is left beside the new one, " + ($left -join ", ") + ", and whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN. Remove it with Navisworks closed") }
  if ($findings.Count -gt 0) { return [pscustomobject]@{ Text = "INSTALLED " + $stamp + ", with a FINDING: " + ($findings -join ", and a FINDING: "); Code = 5 } }
  return [pscustomobject]@{ Text = "INSTALLED " + $stamp; Code = 0 }
}
# The folders build\install.ps1 leaves beside the bundle when it cannot remove them, the one
# it moved aside and a new one that failed, each named as MaskLine writes it.
function BundleLeftovers($bundle) {
  $parent = Split-Path $bundle -Parent
  if (-not (Test-Path -LiteralPath $parent)) { return @() }
  $leaf = Split-Path $bundle -Leaf
  return @(Get-ChildItem -LiteralPath $parent -Directory -Force | Where-Object { $_.Name.StartsWith($leaf + ".replaced-") -or $_.Name.StartsWith($leaf + ".failed-") } | Sort-Object Name | ForEach-Object { MaskLine $_.FullName })
}
# The newest installed.txt a loop install wrote, and whether the bundle listed in $entries
# matches it by relative path and sha256. File is null when no loop install wrote one.
function LastInstallMatch($installsDir, $entries) {
  $r = [pscustomobject]@{ File = $null; Match = $false }
  if (-not (Test-Path -LiteralPath $installsDir)) { return $r }
  $last = @(Get-ChildItem -LiteralPath $installsDir -Recurse -File -Filter "installed.txt" | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1)
  if ($last.Count -eq 0) { return $r }
  $r.File = $last[0].FullName
  $want = @((ListingRows $r.File) | ForEach-Object { $_.Rel + "`t" + $_.Hash })
  $now = @($entries | ForEach-Object { $_.Rel + "`t" + $_.Hash })
  $r.Match = ((($want | Sort-Object) -join "|") -eq (($now | Sort-Object) -join "|"))
  return $r
}

# =======================================================================================
# Check mode. Reads only and writes nothing.
# F104 part 2. One pair as the record and Check print it: the group, the NWF with its bytes and
# sha256 as read, and the read-out of its workbook in the window run's workbooks folder.
function PairLine($pair) {
  $nwf = $pair.Name + ", " + $(if ($null -ne $pair.Length) { [string]$pair.Length + " bytes, sha256 " + $pair.Hash } else { "not read" })
  $wb = "- the group wrote no workbook"
  if ($pair.WorkbookName -ne "") { $wb = "workbooks\" + $pair.WorkbookName } elseif ($null -ne $pair.Xlsx) { $wb = "the read-out of its workbook is not usable, see the refusals" }
  return ($pair.Group + "`t" + $nwf + "`t" + $wb)
}
# F106, the driver's command line, moved into this function by F126 with the -Untick it hands on,
# so the harness builds it the way a run does: the adopted pid and start ticks, the set, the
# stamp and the notes file, then -OpenRun for item 5 or the four folders, with item 1's XML.
function DriverArguments($drv, $ownerPid, $ownerTicks, $set, $stamp, $notes, $item, $paths, $untick) {
  $a = "-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $drv + "`" -OwnerPid " + $ownerPid + " -OwnerStartTicks " + $ownerTicks + " -Set " + $set + " -Stamp " + $stamp + " -Notes `"" + $notes + "`""
  if ($item -eq "5") { $a += " -OpenRun" }
  else {
    $a += " -Source `"" + $paths.Nwc + "`" -Nwf `"" + $paths.Nwf + "`" -Nwd `"" + $paths.Nwd + "`" -Excel `"" + $paths.Report + "`""
    if ($item -eq "1") { $a += " -Xml `"" + $paths.XmlFile + "`"" }
  }
  $ids = @(UntickIds $untick)
  if ($ids.Count -gt 0) { $a += " -Untick " + ($ids -join ",") }
  return $a
}
# F126. The one line, for the record of a window run and for Check, naming the tick boxes the
# driver unticks, or saying it unticks none.
function UntickWords($untick) {
  $ids = @(UntickIds $untick)
  if ($ids.Count -eq 0) { return "the driver unticks no tick box, every one is left as the window opens it" }
  return ("the driver unticks, by AutomationId, before it presses anything: " + ($ids -join ", ") + ". Each is read, toggled only when it reads On, read back Off, and read Off again before Run or Run the open file is pressed, and any other reading stops the driver, UNTICK, with neither pressed. Every other tick box is left as the window opens it")
}
# With $docs, Check -For Documents: the same reads, then the refusals a documents read would
# give in its order, and the pairs it would read. F126: for a window run the tick boxes the driver
# would untick.
function CheckMode($paths, $stamp, $item, $docs, $probe, $untick) {
  Say "==== CHECK, reading only, writing nothing ===="
  Say "---- every Roamer ----"
  [void](RoamerRefusal)
  Say "---- unproved-starts.txt ----"
  [void](UnprovedRefusal $paths.Unproved)
  Say "---- run folders with no VERDICT line ----"
  $died = DiedRuns $paths.RunsRoot
  foreach ($d in $died) { Say ("  " + (Mask $d)) }
  if ($died.Count -eq 0) { Say "  none" }
  Say "---- the installed add-in ----"
  $pv = InstalledStamp $paths.InstalledDll
  Say ("  Federator.Addin.dll reads " + $pv)
  $inst = ListFolder $paths.Bundle
  $bk = ListFolder $paths.BundleBackup
  if ($inst.Ok -and $bk.Ok) { Say ("  the installed bundle, " + $inst.Entries.Count + " files, matches bundle-backup by name and sha256: " + (SameListing $inst.Entries $bk.Entries)) } else { Say ("  UNKNOWN, a bundle folder could not be read: " + $inst.Why + " " + $bk.Why) }
  $li = LastInstallMatch $paths.Installs $inst.Entries
  if ($null -eq $li.File) { Say "  no loop install has written installed.txt yet" }
  elseif (-not $inst.Ok) { Say ("  the newest installed.txt, " + (Mask $li.File) + ", matches the installed bundle: UNKNOWN, the bundle could not be listed, " + $inst.Why) }
  else { Say ("  the newest installed.txt, " + (Mask $li.File) + ", matches the installed bundle: " + $li.Match) }
  foreach ($pair in @(@("his logs folder", $paths.HisLogs, $paths.LogsBackup), @("his AutoSave folder", $paths.AutoSave, $paths.AutoBackup))) {
    Say ("---- " + $pair[0] + " against " + (Split-Path $pair[2] -Leaf) + ", by name and sha256 ----")
    $l = ListFolder $pair[1]
    if (-not $l.Ok) { Say ("  UNKNOWN, " + $l.Why); continue }
    if ($l.Missing) { Say "  the folder is not there"; continue }
    $have = @{}
    try { $have = BackupIndex $pair[2] } catch { Say ("  UNKNOWN, " + (Err $_.Exception)); continue }
    $missing = @($l.Entries | Where-Object { -not $have.ContainsKey($_.Name.ToLowerInvariant() + "|" + $_.Hash) })
    Say ("  " + $l.Entries.Count + " files, " + ($l.Entries.Count - $missing.Count) + " held by the backup, " + $missing.Count + " not yet, which Run copies before its start")
    if ($pair[0] -eq "his logs folder") { foreach ($e in $missing) { Say ("    not yet backed up: " + $e.Rel) } }
  }
  Say "---- source.manifest.txt and source.removed.txt ----"
  Say ("  source.manifest.txt there: " + (Test-Path -LiteralPath $paths.Manifest) + ", source.removed.txt there: " + (Test-Path -LiteralPath $paths.Removed))
  if ($null -ne $docs) {
    Say "---- the refusals Documents would give, in its order ----"
    $r = RunRefusals $paths "" $false $item $null $docs $probe
    foreach ($x in $r) { Say ("  would refuse: " + $x) }
    if ($r.Count -eq 0) { Say "  none, for what was given" }
    Say ("---- the pairs it would read, off " + $(if ($docs.TsvName -ne "") { $docs.TsvName + ", " + $docs.Rows + " rows" } else { "no .tsv" }) + ": " + $docs.Pairs.Count + " pairs ----")
    Say "  group`tNWF, as read now`tworkbook read-out"
    foreach ($p in $docs.Pairs) { Say ("  " + (PairLine $p)) }
    foreach ($g in $docs.NoNwf) { Say ("  " + $g + "`t- the group wrote a workbook and no NWF, so it has no document to read") }
    Say ("  the window run's record: " + $(if ($docs.Verdict -ne "") { $docs.Verdict } else { "not read" }))
    if ($r.Count -eq 0) { return 0 }
    return 2
  }
  if ([string]$item -match '^[1-5]$') {
    Say "---- the tick boxes the driver would untick ----"
    Say ("  " + (UntickWords $untick))
  }
  Say "---- the refusals Run would give, in Run's order ----"
  $wtc = $null; if ([string]$item -match '^[1-5]$') { $wtc = (NewWinTypes).WinType }
  $r = RunRefusals $paths $stamp $false $item $wtc
  foreach ($x in $r) { Say ("  would refuse: " + $x) }
  if ($r.Count -eq 0) { Say "  none, for what was given"; return 0 }
  return 2
}

# =======================================================================================
# MAIN
$runSha = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash
$guardSha = (Get-FileHash -LiteralPath $guardFile -Algorithm SHA256).Hash
Say ("run.ps1 -Mode " + $Mode + ", " + $T0.ToString("yyyy-MM-dd HH:mm:ss") + ", run.ps1 sha256 " + $runSha + ", nw-guard.ps1 sha256 " + $guardSha)

$hostWhy = HostRefusal $PSCommandPath
if ($hostWhy.Count -gt 0) {
  foreach ($w in $hostWhy) { Say ("  " + $w) }
  Say "REFUSED: run.ps1 runs only in Windows PowerShell 5.1, 64 bit and STA, as the script its own powershell.exe was started to run: powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\run.ps1 followed by its parameters. Its deadline ends its own process, so it never runs inside another. Nothing was started and nothing was written."
  exit 2
}
# F104 part 2. A documents read's own folder is named by the moment it was called, so every
# call has a new one and none is ever emptied or used again.
$docStamp = $T0.ToString("yyyyMMdd-HHmmss")
$paramWhy = ParamRefusal $Mode $Set $Item $Stamp $RunFolder $Folder $Xml $OpenFile $args $loopRoot $repo $For $PictureStatuses $PictureCap $PriorityPicked.IsPresent $docStamp $Untick
if ($paramWhy.Count -gt 0) {
  foreach ($w in $paramWhy) { Say ("REFUSED: " + $w + ". Nothing was started and nothing was written.") }
  exit 2
}
$Mode = ModeOf $Mode
$docsMode = ($Mode -eq "Documents" -or ($Mode -eq "Check" -and $For -ceq "Documents"))
$paths = RunPaths $loopRoot $repo $Set $Item $Folder $Xml $OpenFile $(if ($docsMode) { $docStamp } else { "" })
# What a documents read reads before anything else, once, so its refusals and its start read
# the same pairs and the same probe.
$docRead = $null; $probeRead = $null
if ($docsMode) { $docRead = ReadPairs $paths; $probeRead = ProbeRead $paths.ProbeDll }

if ($Mode -eq "Check") { exit (CheckMode $paths $Stamp $Item $docRead $probeRead $Untick) }

$mutex = New-Object System.Threading.Mutex($false, "Local\NwcFederatorLoop.run")
$haveLock = $false
try { $haveLock = $mutex.WaitOne(0) } catch [System.Threading.AbandonedMutexException] { $haveLock = $true; Say "  the loop's lock was left by a run.ps1 that ended without letting it go, and is taken now" }
if (-not $haveLock) {
  $mutex.Dispose()
  Say "REFUSED: another run.ps1 holds the loop's lock, Local\NwcFederatorLoop.run. Nothing was started and nothing was written."
  exit 2
}
$code = 1
try {
  if ($Mode -eq "CloseOwn") { $code = CloseOwn $RunFolder (Join-Path $nw "Roamer.exe") }

  # =====================================================================================
  if ($Mode -eq "Install") {
    do {
      Say "---- check 6, any Navisworks ----"
      if (RoamerRefusal) { Say "REFUSED: Navisworks is running. Nothing is started, installed or put back while any Navisworks runs, whoever started it. Close it and run again. Nothing was installed."; $code = 2; break }
      $tw = TreeRefusal $repo $Stamp
      if ($tw.Count -gt 0) { foreach ($w in $tw) { Say ("REFUSED: " + $w + ".") }; $code = 2; break }
      $when = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
      $idir = Join-Path $paths.Installs ($Stamp + "-" + $when)
      New-Item -ItemType Directory -Path $idir -ErrorAction Stop | Out-Null
      $script:RecordFile = Join-Path $idir "record.txt"
      Say ("RUN RECORD, run.ps1 sha256 " + $runSha + ", -Mode Install -Stamp " + $Stamp)
      $inst = ListFolder $paths.Bundle
      if (-not $inst.Ok) { Say ("STOP: the installed bundle cannot be read whole, " + $inst.Why + ". Nothing was installed."); $code = 2; break }
      $bk = ListFolder $paths.BundleBackup
      $matchBackup = ($bk.Ok -and (SameListing $inst.Entries $bk.Entries))
      $matchLast = (LastInstallMatch $paths.Installs $inst.Entries).Match
      Say ("  the installed bundle, " + $inst.Entries.Count + " files, matches bundle-backup " + $matchBackup + ", matches the last loop install " + $matchLast)
      if (-not $matchBackup -and -not $matchLast -and -not $inst.Missing) {
        $copyTo = Join-Path $paths.LoopRoot ("bundle-backup-" + $when)
        foreach ($e in $inst.Entries) {
          $to = Join-Path $copyTo $e.Rel
          New-Item -ItemType Directory -Force -Path (Split-Path $to -Parent) | Out-Null
          Copy-Item -LiteralPath $e.Full -Destination $to -ErrorAction Stop
        }
        $cb = ListFolder $copyTo
        if (-not ($cb.Ok -and (SameListing $inst.Entries $cb.Entries))) { Say ("STOP: the installed bundle matches neither bundle-backup nor the last loop install, and its copy into " + (Mask $copyTo) + " did not read back. Nothing was installed."); $code = 2; break }
        Say ("  copied into " + (Mask $copyTo) + " and read back by sha256")
      }
      if (RoamerRefusal) { Say "REFUSED: Navisworks is running. Nothing was installed."; $code = 2; break }
      $ps = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
      $itxt = Join-Path $idir "install.txt"
      Say ("  powershell -NoProfile -ExecutionPolicy Bypass -File build\install.ps1, its output into " + (Mask $itxt))
      $ri = RunChild $ps ("-NoProfile -ExecutionPolicy Bypass -File `"" + (Join-Path $repo "build\install.ps1") + "`"") $repo
      [System.IO.File]::WriteAllText($itxt, $ri.Out + "`r`n---- standard error ----`r`n" + $ri.Err, $utf8)
      $ie = $ri.Exit
      Say ("  build\install.ps1 ran as pid " + $ri.Pid + " and exited " + $ie)
      $rd = RunChild "dotnet" "build-server shutdown" $repo
      [System.IO.File]::AppendAllText($itxt, "`r`n---- dotnet build-server shutdown ----`r`n" + $rd.Out + $rd.Err, $utf8)
      Say ("  dotnet build-server shutdown ran as pid " + $rd.Pid + " and exited " + $rd.Exit)
      if ($ie -eq 2) {
        # install.ps1's own refusals, a Navisworks running, a bundle that could not be moved
        # aside whole, or a folder on the way that is a junction or a link, exit 2, refused.
        foreach ($l in @($ri.Out.Split("`n") | Where-Object { $_ -match '^REFUSED:' })) { Say ("  build\install.ps1 said: " + (MaskLine $l.Trim())) }
        Say ("REFUSED: build\install.ps1 refused, exit 2. Its output is in " + (Mask $itxt) + "."); $code = 2; break
      }
      if ($ie -ne 0) { Say ("STOP: build\install.ps1 exited " + $ie + ". Its output is in " + (Mask $itxt) + "."); $code = 1; break }
      # A Navisworks that runs right after the install may have started during it and loaded
      # either build, so which build it runs is UNKNOWN. It changes the verdict and the exit.
      $lateRoamer = RoamerRefusal
      $after = ListFolder $paths.Bundle
      $lines = New-Object System.Collections.Generic.List[string]
      $lines.Add((ListingHeader))
      foreach ($e in $after.Entries) { $lines.Add((EntryLine $e)) }
      [System.IO.File]::WriteAllLines((Join-Path $idir "installed.txt"), $lines.ToArray(), $utf8)
      $pv = InstalledStamp $paths.InstalledDll
      Say ("  installed.txt written, " + $after.Entries.Count + " files, and the installed add-in reads " + $pv)
      if (-not (StampNames $pv $Stamp)) { Say ("STOP after the install: the installed add-in reads " + $pv + ", not " + $Stamp + "."); $code = 1; break }
      $left = @(BundleLeftovers $paths.Bundle)
      foreach ($lf in $left) { Say ("  left beside the new bundle: " + $lf) }
      $iv = InstallVerdict $Stamp $lateRoamer $left
      Say ("VERDICT: " + $iv.Text)
      $code = $iv.Code
    } while ($false)
  }

  # =====================================================================================
  # F104 part 2. A documents read runs through this same block as item 0 does, every guard of
  # it kept, with the probe's two calls where item 0 holds and a window run starts its driver.
  if ($Mode -eq "Run" -or $Mode -eq "Documents") {
    $sync = $null; $wps = $null; $whandle = $null; $mon = $null
    $app = $null; $myPid = 0; $myTicks = $null; $goneAtUtc = $null
    $disposed = $false; $suppressed = $false; $called = $false; $adopted = $false
    $ka = $null; $bs = $null; $logsBefore = $null; $keysBefore = $null
    $stopText = ""; $fault = ""; $forced = ""; $spb = $null; $asp = $null; $tmb = $null; $tmp = $null; $logsChanged = $false; $putBackFailed = $false; $kaOff = $false
    # F106, the window run, items 1 to 5. A documents read reads what one wrote and is not one.
    $win = (-not $docsMode -and $Item -match '^[1-5]$')
    $dproc = $null; $dTicks = $null; $dErr = $null; $evPlan = New-Object System.Collections.Generic.List[object]
    # F104 part 2, what the documents read keeps for its steps after the close.
    $snapBefore = $null; $probeCall = $null; $watched = @()
    do {
      $wt = NewWinTypes
      $procType = $wt.ProcType
      $winType = $wt.WinType
      Say $(if ($docsMode) { "---- checks 4 to 7, 11 and 19 ----" } else { "---- checks 4 to 11 ----" })
      $rr = RunRefusals $paths $Stamp $true $Item $winType $docRead $probeRead
      if ($rr.Count -gt 0) { Say ("REFUSED: " + $rr[0] + "."); $code = 2; break }
      $autoDll = Join-Path $nw "Autodesk.Navisworks.Automation.dll"
      if (-not (Test-Path -LiteralPath $autoDll)) { Say ("REFUSED: there is no Autodesk.Navisworks.Automation.dll at " + $autoDll + ". Nothing was started and nothing was written."); $code = 2; break }
      $napp = [System.Reflection.Assembly]::LoadFrom($autoDll).GetType("Autodesk.Navisworks.Api.Automation.NavisworksApplication")
      if ($null -eq $napp) { Say "REFUSED: the Automation DLL holds no NavisworksApplication. Nothing was started and nothing was written."; $code = 2; break }

      # check 12, the run folder. What is moved aside is said once the new record has started,
      # so the record and the evidence copied from it carry it.
      $asideLines = New-Object System.Collections.Generic.List[string]
      # F104 part 2. A documents read's folder is named by the second it was called, so one
      # already there is a second call in that second, and is refused, never moved or used.
      if ($docsMode -and (Test-Path -LiteralPath $paths.RunDir)) { Say ("REFUSED: the documents read's own folder " + (Mask $paths.RunDir) + " is there already, and a folder is never used twice. Call again. Nothing was started and nothing was written."); $code = 2; break }
      if (Test-Path -LiteralPath $paths.RunDir) {
        $aside = (Split-Path $paths.RunDir -Leaf) + "-aside-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
        try { Rename-Item -LiteralPath $paths.RunDir -NewName $aside -ErrorAction Stop } catch { Say ("REFUSED: the run folder from an earlier call could not be moved aside, " + (Err $_.Exception) + ". Nothing was touched."); $code = 2; break }
        $asideLines.Add("  the run folder from an earlier call was moved aside as " + $aside)
      }
      # The evidence of an earlier call that was NOT RUN, which check 11 let through, moved
      # aside the same way and never emptied. A documents read reads that evidence and never
      # moves it.
      if (-not $docsMode -and (EvidenceFiles $paths.Evidence) -gt 0 -and (EvidenceNotRun $paths.Evidence)) {
        $evAside = (Split-Path $paths.Evidence -Leaf) + "-aside-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
        try { Rename-Item -LiteralPath $paths.Evidence -NewName $evAside -ErrorAction Stop } catch { foreach ($al in $asideLines) { Say $al }; Say ("REFUSED: the evidence of an earlier call that was NOT RUN could not be moved aside, " + (Err $_.Exception) + ". Nothing else was touched."); $code = 2; break }
        $asideLines.Add("  the evidence of an earlier call that was NOT RUN was moved aside as steps\runs\" + $Set + "\" + $evAside)
      }
      New-Item -ItemType Directory -Path $paths.RunDir -ErrorAction Stop | Out-Null
      $script:RecordFile = Join-Path $paths.RunDir "record.txt"
      if ($docsMode) { Say ("RUN RECORD, run.ps1 sha256 " + $runSha + ", nw-guard.ps1 sha256 " + $guardSha + ", compare-document.ps1 sha256 " + (Get-FileHash -LiteralPath (Join-Path $repo "tools\loop\compare-document.ps1") -Algorithm SHA256).Hash + ", -Mode Documents -Set " + $Set + " -Item " + $Item + " -Folder " + $Folder + $(if ($OpenFile -ne "") { " -OpenFile " + $OpenFile } else { "" }) + $(if ($PictureStatuses -ne "") { " -PictureStatuses " + $PictureStatuses } else { "" }) + $(if ($PictureCap -ne "") { " -PictureCap " + $PictureCap } else { "" }) + $(if ($PriorityPicked) { " -PriorityPicked" } else { "" }) + ", the documents read of steps\runs\" + $Set + "\" + $paths.RunName) }
      elseif ($win) { Say ("RUN RECORD, run.ps1 sha256 " + $runSha + ", nw-guard.ps1 sha256 " + $guardSha + ", drive-window-run.ps1 sha256 " + (Get-FileHash -LiteralPath (Join-Path $repo "tools\probes\drive-window-run.ps1") -Algorithm SHA256).Hash + ", -Mode Run -Set " + $Set + " -Item " + $Item + " -Folder " + $Folder + $(if ($Xml -ne "") { " -Xml " + (MaskLine $Xml) } else { "" }) + $(if ($OpenFile -ne "") { " -OpenFile " + $OpenFile } else { "" }) + $(if ($Untick -ne "") { " -Untick " + (@(UntickIds $Untick) -join ",") } else { "" }) + " -Stamp " + $Stamp + ", the window run") }
      else { Say ("RUN RECORD, run.ps1 sha256 " + $runSha + ", nw-guard.ps1 sha256 " + $guardSha + ", -Mode Run -Set " + $Set + " -Item 0 -Stamp " + $Stamp + ", the start with no window") }
      Say ("  the run folder " + (Mask $paths.RunDir) + ", every line from here is also in its record.txt")
      foreach ($al in $asideLines) { Say $al }
      if ($win) { Say ("  " + (UntickWords $Untick)) }
      if ($docsMode) {
        # The pairs as checks 11 and 19 read them, written into pairs.txt, one line per group,
        # the NWF, a tab and the workbook read-out, a dash when the group wrote none.
        Say ("  the window run's record: " + $docRead.Verdict)
        Say ("  its .tsv " + $docRead.TsvName + ", sha256 " + $docRead.TsvHash + ", " + $docRead.Rows + " rows, " + $docRead.Pairs.Count + " pairs")
        Say ("  the probe " + (Mask $paths.ProbeDll) + ", " + $probeRead.Length + " bytes, sha256 " + $probeRead.Hash + ", stamp " + $probeRead.Stamp)
        $pairLines = New-Object System.Collections.Generic.List[string]
        $pairLines.Add("# group`tNWF`tworkbook read-out, a dash when the group wrote none")
        foreach ($p in $docRead.Pairs) {
          Say ("  pair " + (PairLine $p))
          $pairLines.Add($p.Group + "`t" + $p.Nwf + "`t" + $(if ($null -ne $p.Workbook) { $p.Workbook } else { "-" }))
        }
        foreach ($g in $docRead.NoNwf) { Say ("  group " + $g + " wrote a workbook and no NWF, so it has no document to read") }
        [System.IO.File]::WriteAllLines((Join-Path $paths.RunDir "pairs.txt"), $pairLines.ToArray(), $utf8)
      }

      $watchFile = Join-Path $paths.RunDir "watch.txt"
      [System.IO.File]::WriteAllText($watchFile, "", $utf8)
      $beforeAll = @(Get-Process | ForEach-Object { $_.Id })
      $sync = WatchSync $beforeAll $beforeTicks $T0 $loopRoot $nw $CtorDeadlineSeconds $CeilingSeconds $script:RecordFile $paths.Unproved $watchFile $guardText $winType $procType
      $sync.MyProc = $null
      $sync.RecordLock = $script:RecordLock
      $sync.RecordFile = $script:RecordFile
      $sync.RunDir = $paths.RunDir
      $watchScript = WatchdogScript
      $wps = [PowerShell]::Create()
      [void]$wps.AddScript($watchScript).AddArgument($sync)
      $whandle = $wps.BeginInvoke()
      Say ("  the watchdog started, constructor deadline " + $CtorDeadlineSeconds + " s, ceiling " + $CeilingSeconds + " s from adoption")

      try {
        if ($docsMode) {
          # F104 part 2, before anything of his is touched: every folder the read must leave as
          # it is, listed with sha256, the NWFs among them, then the probe copied into the run
          # folder and read back, so the DLL Navisworks loads is the one recorded.
          Say "---- check 20, the copy's NWF, NWD and Clash Report folders before the start ----"
          $watched = New-Object System.Collections.Generic.List[string]
          foreach ($d in @(@($paths.Nwf, $paths.Nwd, $paths.Report) + @($docRead.Pairs | ForEach-Object { Split-Path $_.Nwf -Parent }))) { if (-not (HasPath $watched $d)) { $watched.Add($d) } }
          $snapBefore = FoldersSnap $watched
          foreach ($d in $snapBefore.Keys) { $s = $snapBefore[$d]; Say ("  " + (Mask $d) + ": " + $(if (-not $s.Ok) { "NOT READ WHOLE, " + $s.Why } elseif ($s.Missing) { "not there" } else { [string]$s.Entries.Count + " files listed with sha256" })) }
          if (@($snapBefore.Keys | Where-Object { -not $snapBefore[$_].Ok }).Count -gt 0) { $stopText = "STOP before the start: a folder of the copy could not be read whole, so whether the read leaves it as it was could not be shown. Nothing of his was changed"; Say $stopText; $code = 2; break }
          $nowFiles = SnapFiles $snapBefore
          $moved = @($docRead.Pairs | Where-Object { $nowFiles[(PathKey $_.Nwf)] -ne $_.Hash })
          foreach ($p in $moved) { Say ("  the NWF of group " + $p.Group + " read sha256 " + $p.Hash + " at check 11 and " + $(if ($null -ne $nowFiles[(PathKey $p.Nwf)]) { $nowFiles[(PathKey $p.Nwf)] } else { "nothing" }) + " now") }
          if ($moved.Count -gt 0) { $stopText = "STOP before the start: " + $moved.Count + " NWFs changed between check 11 and now, so what would be read is not what the checks read. Nothing of his was changed"; Say $stopText; $code = 2; break }
          Say ("  every NWF of the " + $docRead.Pairs.Count + " pairs reads the sha256 check 11 read")
          Say "---- check 21, the probe copied into the run folder ----"
          New-Item -ItemType Directory -Path (Split-Path $paths.ProbeCopy -Parent) -ErrorAction Stop | Out-Null
          Copy-Item -LiteralPath $paths.ProbeDll -Destination $paths.ProbeCopy -ErrorAction Stop
          $ph = (Get-FileHash -LiteralPath $paths.ProbeCopy -Algorithm SHA256).Hash
          Say ("  " + (Mask $paths.ProbeCopy) + ", read back sha256 " + $ph)
          if ($ph -ne $probeRead.Hash) { $stopText = "STOP before the start: the probe's copy reads back with another sha256 than check 19 read off the probe, so it is not loaded. Nothing of his was changed"; Say $stopText; $code = 2; break }
          New-Item -ItemType Directory -Path $paths.DocDir -ErrorAction Stop | Out-Null
          New-Item -ItemType Directory -Path $paths.CompareDir -ErrorAction Stop | Out-Null
        }
        $when = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
        Say "---- check 13, his logs folder ----"
        $lb = BackupNew $paths.HisLogs $paths.LogsBackup (Join-Path $paths.RunDir "logs-before.txt") $when
        if (-not $lb.Ok) { $stopText = "STOP before the start: " + $lb.Why + ", so the tool's logs folder could not be copied into logs-backup and read back with its sha256. Nothing of his was changed, and the partial copy stays in the loop folder"; Say $stopText; $code = 2; break }
        Say ("  " + $lb.Listed + " files listed into logs-before.txt, " + $lb.Copied + " copied into logs-backup and read back")
        $logsBefore = @{}
        foreach ($row in (ListingRows (Join-Path $paths.RunDir "logs-before.txt"))) { $logsBefore[$row.Name.ToLowerInvariant()] = $row.Line }
        Say "---- check 13b, team-map.txt, the team map the tool keeps between runs, Q123 ----"
        $tmb = TeamMapBefore $paths.HisLogs $paths.RunDir
        if (-not $tmb.Ok) { $stopText = "STOP before the start: " + $tmb.Why + ", so it could not be kept to put back after the run. Nothing of his was changed"; Say $stopText; $code = 2; break }
        Say ("  " + $tmb.Line)
        Say "---- check 14, his AutoSave folder ----"
        $ab = BackupNew $paths.AutoSave $paths.AutoBackup (Join-Path $paths.RunDir "autosave-before.txt") $when
        if (-not $ab.Ok) { $stopText = "STOP before the start: " + $ab.Why + ", so the AutoSave folder could not be copied into autosave-backup and read back with its sha256. Nothing of his was changed"; Say $stopText; $code = 2; break }
        Say ("  " + $ab.Listed + " files listed into autosave-before.txt, " + $ab.Copied + " copied into autosave-backup and read back. Nothing is written into his AutoSave folder before the put back at the end, Q86")
        Say "---- check 15, the settings backup ----"
        $sdir = Join-Path $paths.RunDir "settings"
        New-Item -ItemType Directory -Path $sdir -ErrorAction Stop | Out-Null
        $bs = BackupSettings $sdir $paths.RegSub $paths.NwAppData $paths.HisLogs
        if (-not $bs.Ok) { $stopText = "STOP before the start: the settings backup is not whole, " + $bs.Why.Replace("STOP before the constructor: ", "") + ". Nothing of his was changed, and the backup stays in the run folder"; Say $stopText; $code = 2; break }
        [pscustomobject]@{ RegSub = $paths.RegSub; RegRoot = $bs.RegRoot; RegBefore = $bs.RegBefore; NwAppData = $paths.NwAppData; FilesBefore = $bs.FilesBefore; NotBacked = $bs.NotBacked; AutoBefore = $bs.AutoBefore } | Export-Clixml -LiteralPath (Join-Path $sdir "before.clixml")
        $sync.RegSub = $paths.RegSub; $sync.RegRoot = $bs.RegRoot; $sync.RegBefore = $bs.RegBefore; $sync.FilesBefore = $bs.FilesBefore
        $sync.NotBacked = $bs.NotBacked; $sync.AutoBefore = $bs.AutoBefore; $sync.NwAppData = $paths.NwAppData; $sync.SettingsReady = $true
        Say "---- M5 before, the write times of the keys under HKCU\Software\Autodesk ----"
        $keysBefore = KeyTimes $winType "Software\Autodesk"
        Say ("  " + $keysBefore.Count + " keys read")
        Say "---- check 17, keep awake ----"
        $ka = KeepAwake $winType $true
        Say ("  SetThreadExecutionState(ES_CONTINUOUS, ES_SYSTEM_REQUIRED, ES_DISPLAY_REQUIRED) returned " + (Hex $ka.Return) + " on native thread " + $ka.Thread)
        if ($ka.Return -eq 0) { $stopText = "STOP before the start: Windows did not take the request to stay awake, SetThreadExecutionState returned 0. Nothing was started"; Say $stopText; $ka = $null; $code = 2; break }
        Say "---- check 18, the last read before the constructor ----"
        $u = UnprovedRefusal $paths.Unproved
        if ($u.Stop) { $stopText = "STOP before the constructor: " + $u.Text.Replace("STOP before the constructor: ", "") + ". The backups stay in the run folder"; Say $stopText; $code = 2; break }
        if (RoamerRefusal) { $stopText = "STOP before the constructor: Navisworks is running. Nothing is started while any Navisworks runs, whoever started it. The backups stay in the run folder"; Say $stopText; $code = 2; break }
        if ($win) {
          $lt18 = SessionLockText $winType
          Say ("  the session reads " + $lt18)
          $l18 = LockRefusal $lt18
          if ($null -ne $l18) { $stopText = "STOP before the constructor: " + $l18.Replace(" Nothing was written", "") + " The backups stay in the run folder"; Say $stopText; $code = 2; break }
        }

        Say "==== THE START ===="
        $called = $true
        $err = $null
        Say ("  " + [DateTime]::Now.ToString("HH:mm:ss.fff") + "  calling new NavisworksApplication() on the main thread")
        $sync.CallStartUtc = [DateTime]::UtcNow
        $sw = [Diagnostics.Stopwatch]::StartNew()
        try { $app = [Activator]::CreateInstance($napp) } catch { $err = $_.Exception }
        $sync.CtorReturned = $true
        $sw.Stop()
        Say ("  the constructor " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
        if ($null -ne $err) { Say ("    " + (Err $err)) }
        $ad = AdoptStart $err $app $sync (Join-Path $paths.RunDir "mypid.txt")
        if ($ad.Suppressed) { $suppressed = $true }
        if (-not $ad.Adopted) { Say "NOT ADOPTED"; $code = 3; break }
        $adopted = $true
        $myPid = $ad.Pid; $myTicks = $ad.Ticks
        Say ("  ADOPTED, pid " + $myPid + ", start ticks UTC " + $myTicks + ", held through its handle")
        # Every call into the adopted Navisworks is named to the watchdog, which closes the
        # process through the held handle if the call has not returned in its limit, 120 s,
        # 600 s for OpenFile, and for ExecuteAddInPlugin 120 s from the moment the monitor
        # ended the run, because the tool's window holds that call open for the whole run.
        $sync.CallLimit = $CallLimitSeconds; $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "Visible"
        try { $app.Visible = $true; Say ("  Visible set True, read back " + $app.Visible) } catch { Say ("  FINDING: setting Visible threw, " + (Err $_.Exception)) } finally { $sync.CallName = "" }
        if ($sync.CallForced -ne "") { Say ("  " + $sync.CallForced) }

        if ($win -and $Item -eq "5") {
          Say "---- the file item 5 opens, copied into the run folder and opened there ----"
          New-Item -ItemType Directory -Path $paths.OpenDir -ErrorAction Stop | Out-Null
          $openCopy = Join-Path $paths.OpenDir $OpenFile
          $srcHash = (Get-FileHash -LiteralPath $paths.OpenSource -Algorithm SHA256).Hash
          Copy-Item -LiteralPath $paths.OpenSource -Destination $openCopy -ErrorAction Stop
          $copyHash = (Get-FileHash -LiteralPath $openCopy -Algorithm SHA256).Hash
          Say ("  " + (Mask $paths.OpenSource) + ", sha256 " + $srcHash + ", copied to " + (Mask $openCopy) + ", read back " + $copyHash)
          if ($copyHash -ne $srcHash) { throw ("the copy of " + $OpenFile + " reads back with another sha256, so it is not opened") }
          $sync.CallLimit = $OpenLimitSeconds; $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "OpenFile"
          $oerr = $null
          Say ("  " + [DateTime]::Now.ToString("HH:mm:ss.fff") + "  calling OpenFile on the copy, on the main thread")
          $sw = [Diagnostics.Stopwatch]::StartNew()
          try { $app.OpenFile($openCopy, [string[]]@()) } catch { $oerr = $_.Exception } finally { $sync.CallName = ""; $sync.CallLimit = $CallLimitSeconds }
          Say ("  OpenFile " + $(if ($null -eq $oerr) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
          if ($null -ne $oerr) { throw ("OpenFile threw on the copy, " + (Err $oerr)) }
        }

        $sync.RunText = FunctionText $PSCommandPath
        $sync.MonitorStop = $false; $sync.RunOver = ""; $sync.MonitorFault = ""; $sync.MonitorWriteFails = 0
        $sync.Dialogs = 0; $sync.ToolLog = $null; $sync.LogAmbiguous = $false; $sync.ReadToolLog = $win
        $sync.LogsFolder = $paths.HisLogs; $sync.LogsBefore = $logsBefore
        $sync.HangLimit = $HangLimitSeconds; $sync.HangCpuSeconds = $HangCpuSeconds; $sync.PassSeconds = $PassSeconds; $sync.HoldSeconds = $(if ($win -or $docsMode) { 0 } else { $HoldSeconds }); $sync.BeatSeconds = $BeatSeconds
        $sync.Window = $win; $sync.Stamp = $Stamp; $sync.QuietSeconds = $QuietSeconds; $sync.EndCloseSeconds = $CallLimitSeconds; $sync.EndForced = ""
        $sync.DriverProc = $null; $sync.DriverExit = $null; $sync.DriverLine = ""; $sync.DriverNotes = Join-Path $paths.RunDir "driver.txt"; $sync.DriverPressed = (DriverCode "PRESSED")
        # F104 part 2. The folder the probe writes into, read by the monitor in place of the
        # tool's log, and whether the probe's calls have returned, which ProbeCall sets.
        $sync.ReadFolder = $(if ($docsMode) { $paths.DocDir } else { "" }); $sync.CallReturned = $false
        # No log is read as the tool's until the plugin call is made, so the time is set then.
        $sync.LogSinceUtc = [DateTime]::MaxValue
        $mps = [PowerShell]::Create()
        [void]$mps.AddScript((MonitorScript)).AddArgument($sync)
        $mon = [pscustomobject]@{ Ps = $mps; Handle = $mps.BeginInvoke() }
        if ($win) {
          Say "---- the driver and the plugin call ----"
          # The plugin call is named to the watchdog before the driver starts, with no limit
          # until the monitor ends the run, so a driver that ends at once still leaves a call
          # that cannot hold the run open.
          $sync.CallLimit = 0; $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "ExecuteAddInPlugin"
          $drv = Join-Path $repo "tools\probes\drive-window-run.ps1"
          $dargs = DriverArguments $drv $myPid $myTicks $Set $Stamp $sync.DriverNotes $Item $paths $Untick
          $psi = New-Object System.Diagnostics.ProcessStartInfo
          $psi.FileName = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
          $psi.Arguments = $dargs
          $psi.UseShellExecute = $false
          $psi.CreateNoWindow = $true
          $psi.RedirectStandardOutput = $true
          $psi.RedirectStandardError = $true
          $psi.WorkingDirectory = $repo
          $dproc = [System.Diagnostics.Process]::Start($psi)
          [void]$dproc.Handle
          $dTicks = UtcTicks $dproc.StartTime
          $null = $dproc.StandardOutput.ReadToEndAsync()
          $dErr = $dproc.StandardError.ReadToEndAsync()
          $sync.DriverProc = $dproc
          Say ("  the driver started as pid " + $dproc.Id + ", held through its handle: powershell.exe " + (MaskLine $dargs))
          if ($sync.RunOver -eq "") {
            $sync.LogSinceUtc = [DateTime]::UtcNow
            $pt0 = [DateTime]::Now
            Say ("  " + $pt0.ToString("HH:mm:ss.fff") + "  calling ExecuteAddInPlugin(" + $PluginId + ", no parameters) on the main thread. AddPluginAssembly is never called, so the installed build runs")
            $perr = $null; $pret = $null
            $sw = [Diagnostics.Stopwatch]::StartNew()
            try { $pret = $app.ExecuteAddInPlugin($PluginId, [string[]]@()) } catch { $perr = $_.Exception } finally { $sync.CallName = ""; $sync.CallLimit = $CallLimitSeconds }
            Say ("  ExecuteAddInPlugin " + $(if ($null -eq $perr) { "RETURNED " + $pret } else { "THREW" }) + ", called " + $pt0.ToString("HH:mm:ss.fff") + ", returned " + [DateTime]::Now.ToString("HH:mm:ss.fff") + ", after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s. Nothing is judged by what it returned")
            if ($null -ne $perr) { Say ("    " + (Err $perr)) }
          } else { $sync.CallName = ""; $sync.CallLimit = $CallLimitSeconds; Say ("  the run ended before the plugin call, " + $sync.RunOver + ", so ExecuteAddInPlugin is not called") }
        } elseif ($docsMode) {
          Say "---- the probe, AddPluginAssembly and the plugin call ----"
          if ($sync.RunOver -eq "") { $probeCall = ProbeCall $app $sync $paths.ProbeCopy $paths.DocDir @($docRead.Pairs | ForEach-Object { $_.Nwf }) $CallLimitSeconds $ProbeId }
          else { $sync.CallReturned = $true; Say ("  the run ended before the probe was loaded, " + $sync.RunOver + ", so neither call is made") }
        }
        while ($sync.RunOver -eq "" -and -not $mon.Handle.IsCompleted) { Start-Sleep -Milliseconds 500 }
        Say ("==== THE RUN ENDED: " + $sync.RunOver + " ====")
        if (@("HOLD", "CLOSED", "DRIVER", "BY ITSELF", "READ") -contains $sync.RunOver) {
          Say ("  " + [DateTime]::Now.ToString("HH:mm:ss.fff") + "  calling Dispose()")
          $sw = [Diagnostics.Stopwatch]::StartNew()
          $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "Dispose"
          try { $app.Dispose(); $disposed = $true } catch { Say ("  Dispose THREW, " + (Err $_.Exception)) } finally { $sync.CallName = "" }
          Say ("  Dispose returned after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
          if ($sync.CallForced -ne "") { Say ("  " + $sync.CallForced + ". Dispose did not return") }
          $sw = [Diagnostics.Stopwatch]::StartNew()
          while ((HeldState $sync.MyProc $myTicks) -eq "same" -and $sw.Elapsed.TotalSeconds -lt 60) { Start-Sleep -Milliseconds 250 }
          $st = HeldState $sync.MyProc $myTicks
          Say ("  the adopted process reads " + $st + " " + $sw.Elapsed.TotalSeconds.ToString("0.0") + " s after Dispose returned")
          if ($st -eq "same") { $cr = CloseAdopted $sync.MyProc $myTicks 30; $forced = $cr.Text; Say ("  FORCED: still running 60 s after Dispose, " + $cr.Text) }
        }
      } catch {
        $fault = (Err $_.Exception)
        Say ("  FAULT in run.ps1: " + $fault)
      } finally {
        # Close first, write after. Each part has its own try, so a fault in one part never
        # skips the parts after it: whatever happened above, the watchdog stops, the put back
        # is judged, the keep awake request is let go and the verdict is written.
        $finallyFaults = New-Object System.Collections.Generic.List[string]
        Say "==== FINALLY ===="
        # The close. CloseAtEnd waits for any close the watchdog began, and closes here only
        # a process that still reads same after it.
        try {
          $ce = CloseAtEnd $sync $myTicks 30
          if ($ce.Text -ne "") { Say ("  " + $ce.Text) }
          if ($ce.Forced -ne "") { $forced = $ce.Forced }
          if ($null -ne $app -and -not $disposed -and -not $suppressed) { [GC]::SuppressFinalize($app); $suppressed = $true }
        } catch { $finallyFaults.Add("the close, " + (Err $_.Exception)) }
        try {
          if ($null -ne $mon) {
            $sync.MonitorStop = $true
            try { [void]$mon.Ps.EndInvoke($mon.Handle) } catch { Say ("  the monitor ended with " + (Err $_.Exception)) }
            foreach ($e in @($mon.Ps.Streams.Error)) { Say ("  monitor runspace error: " + $e.ToString()) }
            $mon.Ps.Dispose()
          }
        } catch { $finallyFaults.Add("the monitor's end, " + (Err $_.Exception)) }
        # F106. The driver is closed through its own held handle if it still runs, and what it
        # wrote to standard error, a fault of its own, goes into the record.
        try {
          if ($null -ne $dproc) {
            if (-not $dproc.HasExited) { $dc = CloseAdopted $dproc $dTicks 10; Say ("  the driver still ran at the end and was closed through its held handle: " + $dc.Text) }
            elseif ($null -eq $sync.DriverExit) { $sync.DriverExit = [int]$dproc.ExitCode; $sync.DriverLine = DriverLastLine $sync.DriverNotes }
            Say ("  the driver exited " + $dproc.ExitCode + ", " + (DriverCodeName $dproc.ExitCode) + ", its last line: " + (DriverLastLine $sync.DriverNotes))
            $de = ""
            if ($dErr.Wait(10000)) { $de = [string]$dErr.Result } else { $de = "UNKNOWN, its standard error did not end in 10 s" }
            foreach ($l in @($de.Split("`n") | Where-Object { $_.Trim() -ne "" } | Select-Object -First 20)) { Say ("  the driver's standard error: " + (MaskLine $l.TrimEnd())) }
          }
        } catch { $finallyFaults.Add("the driver's end, " + (Err $_.Exception)) }
        # M5, what changed outside the loop folder while the start ran, from any program, is
        # read before the put back, so the put back's own writes are never among it, and while
        # the watchdog still runs, so its record covers M5 too.
        try {
          if ($called -and $null -ne $keysBefore) {
            Say "---- M5, what changed outside the loop folder while the start ran, by any program, read before the put back ----"
            $m5 = New-Object System.Collections.Generic.List[string]
            $keysAfter = KeyTimes $winType "Software\Autodesk"
            foreach ($k in @(@($keysBefore.Keys) + @($keysAfter.Keys) | Sort-Object -Unique)) {
              if ($keysBefore[$k] -ne $keysAfter[$k]) { $m5.Add("key " + $k + "`tbefore " + $keysBefore[$k] + "`tafter " + $keysAfter[$k]) }
            }
            $roots = [ordered]@{ "%TEMP%" = $env:TEMP; "%LOCALAPPDATA%\Autodesk" = (Join-Path $env:LOCALAPPDATA "Autodesk"); "%APPDATA%\Autodesk" = (Join-Path $env:APPDATA "Autodesk"); "%PROGRAMDATA%\Autodesk" = (Join-Path $env:ProgramData "Autodesk"); "%APPDATA%\Microsoft\Windows\Recent" = (Join-Path $env:APPDATA "Microsoft\Windows\Recent") }
            foreach ($l in (NewerFiles $roots $sync.CallStartUtc)) { $m5.Add($l) }
            [System.IO.File]::WriteAllLines((Join-Path $paths.RunDir "m5.txt"), $m5.ToArray(), $utf8)
            Say ("  keys under HKCU\Software\Autodesk whose write time changed, appeared or went: " + @($m5 | Where-Object { $_.StartsWith("key ") }).Count + ". Files listed in m5.txt of the run folder, which stays out of the evidence until it is masked")
            foreach ($l in @($m5 | Where-Object { $_ -match ' files written at or after the call$| could not be listed' })) { Say ("  " + $l) }
          }
        } catch { $finallyFaults.Add("M5, " + (Err $_.Exception)) }
        # The watchdog stops here, after M5 and just before the put back reasons are read, so
        # its record covers the whole stretch from the backup to the put back.
        $watchEndedEarly = $true
        $wpsEndError = "the watchdog's end was not read"
        $wpsErrors = @()
        try {
          $watchEndedEarly = $whandle.IsCompleted
          $sync.Stop = $true
          $wpsEndError = $null
          try { [void]$wps.EndInvoke($whandle) } catch { $wpsEndError = (Err $_.Exception); Say ("  the watchdog ended with " + $wpsEndError) }
          $wpsErrors = @($wps.Streams.Error)
          $wps.Dispose()
          Say "---- the watchdog's end ----"
          Say ("  watchdog passes " + $sync.Passes + ", longest pass " + ([double]$sync.PassMaxMs).ToString("0") + " ms, longest gap " + ([double]$sync.MaxGapMs).ToString("0") + " ms, error lines " + $sync.ErrorCount + ", early fails " + $sync.EarlyFails + ", runspace errors " + $wpsErrors.Count + ", lines it could not write " + $sync.WriteErrors.Count + ", forced: " + $(if ($sync.Forced -eq "") { "no" } else { $sync.Forced }) + $(if ($sync.CallForced -ne "") { ", " + $sync.CallForced } else { "" }))
          foreach ($we in $wpsErrors) { Say ("    runspace error: " + $we.ToString()) }
        } catch { $finallyFaults.Add("the watchdog's end, " + (Err $_.Exception)) }
        try {
          if ($called) {
            Say "---- starts the loop could not prove ----"
            $lateNew = @(NewRoamers)
            UnprovedAtEnd $paths.Unproved $myPid $myTicks $lateNew $sync
            if ($null -ne $bs -and $bs.Ok) {
              Say "---- BADER'S SETTINGS, compared, and put back only when no other Navisworks ran ----"
              $script:AlsoFile = Join-Path $paths.RunDir "settings.txt"
              # The AutoSave listing is read in a try of its own, so no report read can stop
              # a write back.
              $asb = AutoSaveBefore (Join-Path $paths.RunDir "autosave-before.txt") $bs.AutoBefore
              if ($asb.Why -ne "") { Say ("  FINDING: " + $asb.Why) } else { Say ("  the AutoSave compare reads autosave-before.txt back, " + $asb.Map.Count + " files") }
              try {
                $pr = PutBackReasons $sync $wpsErrors $wpsEndError $watchEndedEarly $beforeTicks $myPid $myTicks $goneAtUtc
                $why = $pr.Why
                $putBack = ($why.Count -eq 0)
                $spb = SettingsPutBack $putBack $why $sdir $paths.RegSub $bs.RegBefore $bs.RegRoot $paths.NwAppData $bs.FilesBefore $bs.NotBacked $asb.Map $bs.AppBackup
                Say "---- his AutoSave folder, Q86 ----"
                $asp = PutBackAutoSave $putBack $asb.Map $paths.AutoSave $paths.AutoBackup
                foreach ($l in $asp.Lines) { Say ("  " + $l) }
                Say ("  AutoSave: " + $asp.Done + " put back as they were and read back, " + $asp.Left + " not as they were before the run" + $(if ($putBack) { "" } else { ", because nothing is written while a reason above stands" }))
              } catch { $putBackFailed = $true; Say ("  the compare stopped, " + (Err $_.Exception) + ". Nothing more is written, the backup is kept in the run folder") }
              # F131, Q123. team-map.txt in a try of its own, after the AutoSave put back and before
              # his logs folder is listed again, so the listing reads it as it was put back.
              if ($null -ne $tmb) {
                Say "---- team-map.txt, the team map the tool keeps between runs, Q123 ----"
                try {
                  $tmp = PutBackTeamMap ($putBack -eq $true) $tmb $paths.HisLogs (Join-Path $paths.RunDir "teammap")
                  foreach ($l in $tmp.Lines) { Say ("  " + $l) }
                  Say ("  team map: " + $tmp.Done + " put back as it was and read back, " + $tmp.Left + " not as it was before the run" + $(if ($putBack -eq $true) { "" } else { ", because nothing is written while a reason above stands" }))
                } catch { $putBackFailed = $true; Say ("  the team map compare stopped, " + (Err $_.Exception) + ". Nothing is written, its copy is kept in the run folder's teammap") }
              }
              $script:AlsoFile = $null
            }
          } else { Say "  the constructor was never called, so nothing is closed, written down or put back" }
        } catch { $script:AlsoFile = $null; $putBackFailed = $true; $finallyFaults.Add("the starts written down or the put back, " + (Err $_.Exception)) }
        try {
          if ($null -ne $logsBefore) {
            Say "---- his logs folder, after ----"
            $la = ListFolder $paths.HisLogs
            if ($la.Ok) {
              $afterLines = @($la.Entries | ForEach-Object { EntryLine $_ })
              [System.IO.File]::WriteAllLines((Join-Path $paths.RunDir "logs-after.txt"), (@((ListingHeader)) + $afterLines), $utf8)
              $beforeRows = ListingRows (Join-Path $paths.RunDir "logs-before.txt")
              $afterRows = ListingRows (Join-Path $paths.RunDir "logs-after.txt")
              $beforeLines = @($beforeRows | ForEach-Object { $_.Line })
              if ($win) {
                # Q82: a window run may prune his oldest logs, which logs-backup holds by name
                # and sha256, and leaves the tool's own log and tsv for the close of the loop.
                $toolNames = @()
                if ($null -ne $sync.ToolLog) { $toolNames = @((Split-Path $sync.ToolLog -Leaf), ([System.IO.Path]::GetFileNameWithoutExtension($sync.ToolLog) + ".tsv")) }
                $lw = LogsAfterWindow $beforeRows $afterRows $toolNames (BackupIndex $paths.LogsBackup)
                foreach ($l in $lw.Lines) { Say ("    " + $l) }
                Say ("  his logs folder after the window run, Q82: " + $lw.Lines.Count + " differences, LOST, a file of his gone or changed with no copy from before the run in logs-backup: " + $lw.Lost)
                $logsChanged = ($lw.Lost -gt 0)
              } else {
                $logsChanged = ((($beforeLines | Sort-Object) -join "`n") -ne (($afterLines | Sort-Object) -join "`n"))
                Say ("  logs-after.txt equals logs-before.txt, name for name, size, write time, sha256 and attributes: " + (-not $logsChanged))
                if ($logsChanged) {
                  $relOf = @{}
                  foreach ($row in $beforeRows) { $relOf[$row.Line] = $row.Rel }
                  foreach ($row in $afterRows) { $relOf[$row.Line] = $row.Rel }
                  foreach ($d in @(Compare-Object $beforeLines $afterLines)) { Say ("    FINDING " + $d.SideIndicator + " " + $relOf[[string]$d.InputObject]) }
                }
              }
            } else { $logsChanged = $true; Say ("  UNKNOWN, his logs folder: " + $la.Why) }
          }
        } catch { $logsChanged = $true; $finallyFaults.Add("the logs compare, " + (Err $_.Exception)) }
        # F106. A window run's own evidence, read before the verdict so the record names it:
        # whether the tool's log shows the run RAN, outputs.txt, a read-out of every workbook,
        # the tool's log masked, and every file over 20 MB that is not copied, Q90.
        $logCheck = ""
        try {
          if ($win -and $called) {
            Say "---- the tool's log, the outputs and the workbooks ----"
            $tnFile = Join-Path $paths.RunDir "toollog-name.txt"
            if (-not (Test-Path -LiteralPath $tnFile)) { [System.IO.File]::WriteAllText($tnFile, "UNKNOWN, no log made after the plugin call named the installed stamp, so none was read as the loop's`r`n", $utf8) }
            if ($null -eq $sync.ToolLog) { $logCheck = "UNKNOWN, no log was read as the tool's"; Say ("  " + $logCheck + ", toollog-name.txt says why") }
            else {
              $tl = @((ReadShared $sync.ToolLog).Split("`n") | ForEach-Object { $_.TrimEnd("`r") })
              if ($tl.Count -gt 0 -and $tl[$tl.Count - 1] -eq "") { $tl = @($tl | Select-Object -First ($tl.Count - 1)) }
              if ($sync.RunOver -eq "CLOSED") {
                $logCheck = ToolLogVerdict $tl $Stamp $Item
                Say ("  the tool's log on disk " + $(if ($logCheck -eq "") { "shows the run RAN: its RESULT block, its SESSION naming " + $Stamp + $(if ($Item -eq "1") { ", and its GROUPS block reading no group unticked" } else { "" }) } else { "does not show the run RAN, " + $logCheck }))
              }
              # The log and its tsv are copied into the run folder, the log with its FOLDERS
              # REMEMBERED and TEAMS KEPT blocks masked and every other line naming his logs
              # folder or the kept map, the map the copy taken at check 13b keeps and the one
              # his team-map.txt keeps now, so what goes into the evidence and what stays when a
              # file is over 20 MB are both under the loop folder, Q87, Q90 and F131. The two in
              # his logs folder are left there for the close of the loop, Q82.
              $mr = MaskRemembered $tl (PathsOfHis $paths.HisLogs @((Join-Path (Join-Path $paths.RunDir "teammap") (TeamMapName)), (Join-Path $paths.HisLogs (TeamMapName))))
              $tdir = Join-Path $paths.RunDir "toollog"
              New-Item -ItemType Directory -Force -Path $tdir | Out-Null
              $mlog = Join-Path $tdir (Split-Path $sync.ToolLog -Leaf)
              [System.IO.File]::WriteAllLines($mlog, $mr.Lines.ToArray(), $utf8)
              $evPlan.Add([pscustomobject]@{ Name = (Split-Path $mlog -Leaf); From = $mlog })
              Say ("  the tool's log, " + $tl.Count + " lines, copied into the run folder's toollog with its FOLDERS REMEMBERED block masked, " + $mr.Masked + " lines, Q87, its TEAMS KEPT block masked, " + $mr.MaskedTeams + " lines, Q123, and every other line naming his logs folder or the kept team map masked, " + $mr.MaskedPaths + " lines, F131")
              $tsv = [System.IO.Path]::ChangeExtension($sync.ToolLog, ".tsv")
              if (Test-Path -LiteralPath $tsv) {
                $ctsv = Join-Path $tdir (Split-Path $tsv -Leaf)
                Copy-Item -LiteralPath $tsv -Destination $ctsv
                $evPlan.Add([pscustomobject]@{ Name = (Split-Path $ctsv -Leaf); From = $ctsv })
                Say ("  the tool's .tsv copied into the run folder's toollog, " + (Get-Item -LiteralPath $ctsv).Length + " bytes")
              } else { Say "  the tool's .tsv is not there" }
            }
            $ol = New-Object System.Collections.Generic.List[string]
            $ol.Add("# every file under the run's output folders" + $(if ($Item -eq "5") { " and its open folder" } else { "" }) + ", after the run")
            $ol.Add((ListingHeader))
            $xlsx = New-Object System.Collections.Generic.List[object]
            $outRoots = [ordered]@{}
            foreach ($d in @($paths.Nwf, $paths.Nwd, $paths.Report)) { $outRoots[$d.Substring($paths.Copy.Length).TrimStart('\')] = $d }
            if ($Item -eq "5") { $outRoots["open"] = $paths.OpenDir }
            $nOut = 0
            foreach ($label in $outRoots.Keys) {
              $lf = ListFolder $outRoots[$label]
              if (-not $lf.Ok) { $ol.Add("# " + $label + " could not be listed whole, " + $lf.Why); continue }
              if ($lf.Missing) { $ol.Add("# " + $label + " is not there"); continue }
              foreach ($e in $lf.Entries) {
                $ol.Add($label + "\" + (EntryLine $e)); $nOut++
                if ($e.Name -like "*.xlsx") { $xlsx.Add([pscustomobject]@{ Label = $label; Entry = $e }) }
              }
            }
            [System.IO.File]::WriteAllLines((Join-Path $paths.RunDir "outputs.txt"), $ol.ToArray(), $utf8)
            Say ("  outputs.txt lists " + $nOut + " files, " + $xlsx.Count + " of them workbooks")
            if ($xlsx.Count -gt 0) {
              $wbDir = Join-Path $paths.Evidence "workbooks"
              New-Item -ItemType Directory -Force -Path $wbDir | Out-Null
              $psExe = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
              foreach ($x in $xlsx) {
                $outName = (($x.Label + "\" + $x.Entry.Rel) -replace '[\\/:*?"<>| ]', '_') + ".txt"
                $rw = RunChild $psExe ("-NoProfile -ExecutionPolicy Bypass -File `"" + (Join-Path $repo "tools\loop\read-workbook.ps1") + "`" -Workbook `"" + $x.Entry.Full + "`" -Out `"" + (Join-Path $wbDir $outName) + "`"") $repo
                Say ("  read-workbook.ps1 on " + $x.Label + "\" + $x.Entry.Rel + " ran as pid " + $rw.Pid + " and exited " + $rw.Exit + ", its read-out is workbooks\" + $outName)
              }
            }
          }
          if ($win -and $null -ne $paths.RunDir -and (Test-Path -LiteralPath $paths.RunDir)) {
            foreach ($n in @("watch.txt", "settings.txt", "driver.txt", "toollog-name.txt", "outputs.txt")) { $src = Join-Path $paths.RunDir $n; if (Test-Path -LiteralPath $src) { $evPlan.Add([pscustomobject]@{ Name = $n; From = $src }) } }
            # A plain foreach, never @($evPlan): @() on a List[object] throws in 5.1, CpuUsedSince.
            foreach ($p in $evPlan) {
              $len = (Get-Item -LiteralPath $p.From).Length
              if ($len -gt 20MB) { Say ("  EVIDENCE NOT COPIED, over 20 MB: " + $p.Name + ", " + $len + " bytes, sha256 " + (Get-FileHash -LiteralPath $p.From -Algorithm SHA256).Hash + ". It stays in " + (Mask (Split-Path $p.From -Parent)) + ", Q90"); $p.From = $null }
            }
          }
        } catch { $finallyFaults.Add("the tool's log, the outputs and the workbooks, " + (Err $_.Exception)) }
        # F104 part 2. The documents read's own steps, once its Navisworks is closed and Bader's
        # things are put back, each in its own try, so a fault in one never skips the next: the
        # copy's folders listed again, each read-out read, and compare-document.ps1 on every
        # pair. Each writes only under the run folder. DocCheck gathers every step that could not
        # run, and the files of the copy's folders the read changed, which it must never do.
        $docCheck = New-Object System.Collections.Generic.List[string]; $docText = ""
        $folderDiff = $null; $states = @{}; $cmp = @{}
        if ($docsMode -and $called) {
          if ($null -ne $probeCall -and $probeCall.Fault -ne "") { $docCheck.Add($probeCall.Fault) }
          try {
            Say "---- check 20 again, the copy's NWF, NWD and Clash Report folders after the close ----"
            $folderDiff = FoldersDiff $snapBefore (FoldersSnap $watched)
            foreach ($l in $folderDiff.Lines) { Say ("  " + $l) }
            Say ("  " + $folderDiff.Same + " files read the sha256 they read before the start, " + $folderDiff.Differ + " differ, " + $folderDiff.Unread + " folders were not read whole")
            if ($folderDiff.Unread -gt 0) { $docCheck.Add([string]$folderDiff.Unread + " of the copy's folders could not be read whole after the close") }
            if ($folderDiff.Differ -gt 0) { $docCheck.Add([string]$folderDiff.Differ + " files of the copy's NWF, NWD and Clash Report folders differ after the read, which writes nothing there") }
          } catch { $docCheck.Add("the copy's folders could not be listed after the close, " + (Err $_.Exception)) }
          try {
            Say "---- the read-outs ----"
            $notWhole = 0
            foreach ($p in $docRead.Pairs) {
              $states[$p.Stem] = ReadOutState $paths.DocDir $p.Stem
              if ($states[$p.Stem] -cne "whole") { $notWhole++ }
              Say ("  " + $p.Group + "`t" + $p.Stem + "-document.txt`t" + $states[$p.Stem])
            }
            if ($notWhole -gt 0) { $docCheck.Add([string]$notWhole + " of " + $docRead.Pairs.Count + " read-outs are not whole") }
          } catch { $docCheck.Add("the read-outs could not be read, " + (Err $_.Exception)) }
          try {
            Say "---- compare-document.ps1 on every pair ----"
            $notDone = 0
            foreach ($p in $docRead.Pairs) {
              if ($null -eq $p.Workbook) { $cmp[$p.Stem] = "NOT COMPARED, the group wrote no workbook"; Say ("  " + $p.Group + "`t" + $cmp[$p.Stem]); continue }
              if ($states[$p.Stem] -cne "whole") { $cmp[$p.Stem] = "NOT COMPARED, its read-out is not whole"; Say ("  " + $p.Group + "`t" + $cmp[$p.Stem]); continue }
              $c = ComparePair $repo $p.Workbook (Join-Path $paths.DocDir ($p.Stem + "-document.txt")) (Join-Path $paths.CompareDir ($p.Stem + "-compare.txt")) $PictureStatuses $PictureCap $PriorityPicked.IsPresent
              $cmp[$p.Stem] = $c.Text
              if ($c.Exit -ne 0) { $notDone++ }
              Say ("  " + $p.Group + "`tcompare-document.ps1 ran as pid " + $c.Pid + " and exited " + $c.Exit + ", " + $c.Text)
            }
            if ($notDone -gt 0) { $docCheck.Add([string]$notDone + " comparisons did not finish, compare-document.ps1 exited other than 0") }
          } catch { $docCheck.Add("compare-document.ps1 could not be run on every pair, " + (Err $_.Exception)) }
          $whole = @($docRead.Pairs | Where-Object { $states[$_.Stem] -ceq "whole" }).Count
          $compared = @($docRead.Pairs | Where-Object { ([string]$cmp[$_.Stem]).StartsWith("VERDICT ") }).Count
          $docText = [string]$docRead.Pairs.Count + " pairs, " + $whole + " read-outs whole, " + $compared + " comparisons written, the copy's folders read the same after"
        }
        try {
          if ($null -ne $ka) {
            $off = KeepAwake $winType $false
            $kaOff = $true
            Say ("  keep awake OFF returned " + (Hex $off.Return) + " on native thread " + $off.Thread + ". ON was thread " + $ka.Thread + ". Same thread " + ($off.Thread -eq $ka.Thread) + ", returned 0x80000003 " + ($off.Return -eq [uint32]2147483651))
            if ($off.Return -ne [uint32]2147483651 -or $off.Thread -ne $ka.Thread) { Say "  FINDING: the keep awake request may have lapsed before the end" }
          }
        } catch { $finallyFaults.Add("the keep awake release, " + (Err $_.Exception)) }
        foreach ($ff in $finallyFaults) { Say ("  FAULT in the finally, " + $ff) }
        $notPutBack = $putBackFailed -or $logsChanged
        if ($null -ne $spb) { if ($spb.NotWritten -gt 0) { $notPutBack = $true } }
        if ($null -ne $asp) { if ($asp.Left -gt 0) { $notPutBack = $true } }
        if ($null -ne $tmp) { if ($tmp.Left -gt 0) { $notPutBack = $true } }
        $endState = "gone"
        if ($adopted -and $null -ne $sync -and $null -ne $sync.MyProc) { $endState = HeldState $sync.MyProc $myTicks }
        $dName = ""; $dText = ""
        if ($null -ne $sync -and $null -ne $sync.DriverExit) {
          $dName = DriverCodeName $sync.DriverExit
          $dm = [regex]::Match([string]$sync.DriverLine, 'DRIVER EXIT -?\d+ [A-Z ]+: (.*)$')
          if ($dm.Success) { $dText = $dm.Groups[1].Value } else { $dText = [string]$sync.DriverLine }
        }
        $vd = RunVerdict ([pscustomobject]@{ StopText = $stopText; Called = $called; Adopted = $adopted; NotPutBack = $notPutBack; RunOver = [string]$sync.RunOver; Forced = [string]$sync.Forced; CallForced = [string]$sync.CallForced; Fault = $fault; MonitorFault = [string]$sync.MonitorFault; FinallyFaults = @($finallyFaults); Dialogs = [int]$sync.Dialogs; ClosedHere = $forced; HoldSeconds = $HoldSeconds; EndState = $endState; Item = $Item; EndForced = [string]$sync.EndForced; DriverName = $dName; DriverText = $dText; LogCheck = $logCheck; OpenKind = $(if ($OpenFile -ne "") { [System.IO.Path]::GetExtension($OpenFile).ToLowerInvariant() } else { "" }); Documents = $docsMode; DocCheck = ($docCheck -join ", and "); DocText = $docText })
        $code = $vd.Code
        Say ("  lines that could not be written to record.txt: " + $script:SayFailures + ", by the monitor: " + $sync.MonitorWriteFails)
        Say ("VERDICT: " + $vd.Text)
        $script:RecordFile = $null
        # F104 part 2. A documents read writes summary.txt last, beside its read-outs and
        # comparisons, and nothing into steps\runs, where the window run's evidence is only read.
        # The lead copies what it wrote there through F102's mask.
        if ($docsMode) {
          try {
            if ($called) {
              $switches = (@($(if ($PictureStatuses -ne "") { "-PictureStatuses " + $PictureStatuses }), $(if ($PictureCap -ne "") { "-PictureCap " + $PictureCap }), $(if ($PriorityPicked) { "-PriorityPicked" })) | Where-Object { $_ }) -join " "
              $sl = SummaryLines ([pscustomobject]@{ Set = $Set; RunName = $paths.RunName; RunDir = $paths.RunDir; Read = $docRead; Probe = $probeRead; Call = $probeCall; RunOver = [string]$sync.RunOver; Diff = $folderDiff; States = $states; Compared = $cmp; Switches = $switches; DocCheck = $docCheck; Verdict = $vd.Text })
              $sf = Join-Path $paths.RunDir "summary.txt"
              [System.IO.File]::WriteAllLines($sf, $sl.ToArray(), $utf8)
              Say ("  summary.txt written, " + (Get-Item -LiteralPath $sf).Length + " bytes, " + $docRead.Pairs.Count + " pair lines")
            } else { Say "  the constructor was never called, so no document was read and no summary.txt is written" }
            Say ("  nothing was written into steps\runs. The read-outs, comparisons and summary are in " + (Mask $paths.RunDir) + ", for the lead to copy into steps\runs\" + $Set + "\" + $paths.RunName + "\document through F102's mask")
          } catch { Say ("  FAULT: summary.txt could not be written, " + (Err $_.Exception) + ". The read-outs and comparisons are in the run folder"); if ($code -eq 0) { $code = 1 } }
        } else {
          try {
            $ev = $paths.Evidence
            New-Item -ItemType Directory -Force -Path $ev | Out-Null
            if ($win) {
              Copy-Item -LiteralPath (Join-Path $paths.RunDir "record.txt") -Destination (Join-Path $ev "record.txt")
              Say ("  evidence record.txt, " + (Get-Item -LiteralPath (Join-Path $ev "record.txt")).Length + " bytes")
              foreach ($p in $evPlan) {
                if ($null -eq $p.From) { continue }
                $to = Join-Path $ev $p.Name
                Copy-Item -LiteralPath $p.From -Destination $to
                Say ("  evidence " + $p.Name + ", " + (Get-Item -LiteralPath $to).Length + " bytes")
              }
            } else {
              foreach ($n in @("record.txt", "watch.txt", "settings.txt")) {
                $src = Join-Path $paths.RunDir $n
                if (Test-Path -LiteralPath $src) { Copy-Item -LiteralPath $src -Destination (Join-Path $ev $n); Say ("  evidence " + $n + ", " + (Get-Item -LiteralPath (Join-Path $ev $n)).Length + " bytes") }
              }
            }
            Say ("  the evidence is in steps\runs\" + $Set + "\" + $paths.RunName + ". Mask it with F102's tool before it is committed")
          } catch { Say ("  FAULT: the evidence could not be copied into steps\runs, " + (Err $_.Exception) + ". It is all in the run folder"); if ($code -eq 0) { $code = 1 } }
        }
      }
    } while ($false)
  }
} finally {
  # The outermost finally lets go of the keep awake request if the run's own finally could not.
  if ($null -ne $ka -and -not $kaOff) { $off = KeepAwake $winType $false; [Console]::Out.WriteLine("  keep awake OFF in the outermost finally returned " + (Hex $off.Return) + " on native thread " + $off.Thread) }
  $mutex.ReleaseMutex()
  $mutex.Dispose()
}
exit $code
