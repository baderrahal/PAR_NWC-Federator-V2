param(
  [string]$Mode = "Check",
  [string]$Set = "",
  [string]$Item = "",
  [string]$Stamp = "",
  [string]$RunFolder = ""
)
$ErrorActionPreference = "Stop"

# tools\loop\run.ps1, F103 part 1. Starts, watches and closes one Navisworks for the loop with
# every guard kept in code. The design is steps\notes\f103-design.md and the rules it keeps
# are in .claude\rules\loop.md. The guard code it shares with the probe is
# tools\loop\nw-guard.ps1, one copy, dot-sourced here.
#
# Always started as
#
#   powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\run.ps1 -Mode <mode> ...
#
# PART 1 BUILDS FOUR MODES:
#   Check                      the default. Reads only, writes nothing, prints to the console.
#                              Exit 0 when Run would go, 2 when it would refuse
#   Install -Stamp <8 hex>     installs this clean checkout at that commit with
#                              build\install.ps1, after its refusals and the bundle backup
#   Run -Set NN -Item 0 -Stamp <8 hex>
#                              the start with no window: refusals, backups, one Navisworks
#                              started through the Automation API and adopted, held open and
#                              Visible for 360 s with its processor time read every 15 s,
#                              closed with Dispose, and Bader's settings put back by the D2
#                              rule. ExecuteAddInPlugin is never called, so the tool's window
#                              never opens and nothing is written into his logs folder
#   CloseOwn -RunFolder <runs\NN\item...>
#                              closes the loop's own Navisworks after a run.ps1 died, only
#                              when its mypid.txt, name, path, command line and start ticks all
#                              match
# NOT IN PART 1, waiting for Bader's answer to Q82: the window, items 1 to 5, -Xml, -OpenFile,
# -LogChoice, -OpenWindow, the log lock and the driver. Any of them is refused as a parameter.
#
# EXIT CODES: 0 finished and everything put back, 1 a fault in run.ps1, UNKNOWN whether the
# adopted Navisworks still runs, or one still running after every close path, 2 refused, for
# Install also a refusal of build\install.ps1, 3 not adopted or the constructor deadline, 4
# hung, the ceiling, or a call into the adopted Navisworks that did not return in 120 s, 5
# finished but a dialog appeared, or for Install, installed but a Navisworks ran right after
# it or the add-in installed before was left beside it, 6 something of Bader's not put back,
# 7 the adopted Navisworks ended by itself. When more than one applies, the first in the
# order 3, 6, 4, 7, 1, 5, which RunVerdict keeps.
#
# NUMBERS THAT ARE NOT PARAMETERS, so no switch can move a path or shorten a limit: the hang
# rule's 300 s, the constructor deadline's 300 s, the ceiling's 12 hours until Bader answers
# Q84, the 120 s a call into the adopted Navisworks may take, item 0's hold of 360 s, the
# monitor's pass of 15 s and its heartbeat of 60 s.

$HangLimitSeconds = 300
$CtorDeadlineSeconds = 300
$CeilingSeconds = 43200
$HoldSeconds = 360
$PassSeconds = 15
$BeatSeconds = 60
$CallLimitSeconds = 120

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

# Every path run.ps1 reads or writes, built here and nowhere else.
function RunPaths($loopRoot, $repo, $set, $item) {
  $p = [pscustomobject]@{
    LoopRoot = $loopRoot
    RunsRoot = Join-Path $loopRoot "runs"
    RunDir = $null
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
  }
  if ($set -ne "" -and $item -ne "") {
    $p.RunDir = Join-Path $loopRoot ("runs\" + $set + "\item" + $item)
    $p.Evidence = Join-Path $repo ("steps\runs\" + $set + "\item" + $item)
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

# The four modes, the one list of them. ModeOf returns the mode as it is written here, or null.
function Modes { return @("Check", "Install", "Run", "CloseOwn") }
function ModeOf($mode) {
  foreach ($m in (Modes)) { if ($m -eq $mode) { return $m } }
  return $null
}

# Check 2, the parameters, as an allow list. Returns each refusal as the text after REFUSED:.
function ParamRefusal($mode, $set, $item, $stamp, $runFolder, $extra, $loopRoot) {
  $why = New-Object System.Collections.Generic.List[string]
  foreach ($a in @($extra)) { $why.Add("the argument " + $a + " is not a parameter of run.ps1 part 1, because the window, items 1 to 5, -Xml, -OpenFile, -LogChoice and -OpenWindow wait for Bader's answer to Q82") }
  $m = ModeOf $mode
  if ($null -eq $m) { $why.Add("-Mode is " + $mode + ", not one of " + ((Modes) -join ", ")); return ,$why }
  $mode = $m
  $needSet = ($mode -eq "Run"); $needItem = ($mode -eq "Run"); $needStamp = ($mode -eq "Run" -or $mode -eq "Install"); $needFolder = ($mode -eq "CloseOwn")
  if ($set -ne "" -and $set -notmatch '^\d\d$') { $why.Add("-Set is " + $set + ", not two digits") }
  elseif ($set -eq "" -and $needSet) { $why.Add("-Set is missing, and -Mode Run needs two digits") }
  if ($item -ne "") {
    if ($item -match '^[1-5]$') { $why.Add("-Item is " + $item + ", and items 1 to 5 are part 2 of F103, not built, waiting for Bader's answer to Q82") }
    elseif ($item -ne "0") { $why.Add("-Item is " + $item + ", not 0 to 5") }
  } elseif ($needItem) { $why.Add("-Item is missing, and -Mode Run needs 0") }
  if ($stamp -ne "" -and $stamp -cnotmatch '^[0-9a-f]{8}$') { $why.Add("-Stamp is " + $stamp + ", not 8 lower case hex characters") }
  elseif ($stamp -eq "" -and $needStamp) { $why.Add("-Stamp is missing, and -Mode " + $mode + " needs the 8 hex characters of the commit") }
  if ($mode -ne "Check" -and $mode -ne "Run") {
    if ($set -ne "") { $why.Add("-Set is " + $set + ", and -Mode " + $mode + " takes none") }
    if ($item -ne "") { $why.Add("-Item is " + $item + ", and -Mode " + $mode + " takes none") }
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
  if ($mode -eq "Run" -and $why.Count -eq 0) {
    $pr = PathRefusal (Join-Path $loopRoot ("runs\" + $set + "\item" + $item)) $loopRoot
    if ($null -ne $pr) { $why.Add("-Set is " + $set + ", and the run folder it names " + $pr) }
  }
  return ,$why
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
# The stamp is one word of the version, so be0b9b37+edits never names be0b9b37.
function StampNames($productVersion, $stamp) { return (@([string]$productVersion -split '\s+') -ccontains $stamp) }

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

# Checks 4 to 11 for item 0, in Run's order. 8, 9 and 10 do not apply to item 0 with no
# window. Returns each refusal as the text after REFUSED:.
function RunRefusals($paths, $stamp, [bool]$stopAtFirst) {
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
  Say "  checks 8, 9 and 10, the copy, the outputs and the window's log, do not apply to item 0 with no window"
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
# What a backup folder holds, by file name and sha256, wherever it sits in the folder.
function BackupIndex($backupRoot) {
  $have = @{}
  $l = ListFolder $backupRoot
  if (-not $l.Ok) { throw ("the backup folder " + (Mask $backupRoot) + ", " + $l.Why) }
  foreach ($e in $l.Entries) { $have[$e.Name.ToLowerInvariant() + "|" + $e.Hash] = $true }
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
# THE ONE READER of a listing EntryLine wrote, logs-before.txt, logs-after.txt,
# autosave-before.txt and installed.txt: relative path, bytes, written UTC, sha256 and
# attributes, one line each. Returns an object per line, with the line itself as Line.
function ListingRows($listFile) {
  $rows = New-Object System.Collections.Generic.List[object]
  foreach ($l in [System.IO.File]::ReadAllLines($listFile)) {
    if ($l -ceq (ListingHeader) -or $l -ceq (ListingNoFolder) -or $l -eq "") { continue }
    $x = $l.Split("`t")
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

# =======================================================================================
# Keep awake, the session lock and what changed outside the loop folder while a start ran,
function KeepAwake($winType, [bool]$on) {
  $flags = [uint32]2147483648
  if ($on) { $flags = [uint32]2147483651 }
  $ret = $winType::SetThreadExecutionState($flags)
  return [pscustomobject]@{ Return = [uint32]$ret; Thread = [uint32]$winType::GetCurrentThreadId() }
}
function Hex($v) { return ("0x" + ([uint32]$v).ToString("X8")) }
function SessionLockText($winType) {
  $sid = [System.Diagnostics.Process]::GetCurrentProcess().SessionId
  $buf = [IntPtr]::Zero
  [uint32]$bytes = 0
  $ok = $winType::WTSQuerySessionInformationW([IntPtr]::Zero, [int]$sid, 25, [ref]$buf, [ref]$bytes)
  if (-not $ok) { return "UNKNOWN, WTSQuerySessionInformation with WTSSessionInfoEx returned false" }
  try {
    $level = [System.Runtime.InteropServices.Marshal]::ReadInt32($buf, 0)
    $rsid = [System.Runtime.InteropServices.Marshal]::ReadInt32($buf, 8)
    $flags = [System.Runtime.InteropServices.Marshal]::ReadInt32($buf, 16)
    if ($level -ne 1 -or $rsid -ne $sid) { return ("UNKNOWN, the answer did not read as level 1 of this session, level " + $level + ", session " + $rsid) }
    if ($flags -eq 0) { return "locked" }
    if ($flags -eq 1) { return "unlocked" }
    return ("UNKNOWN, the session flags read " + $flags)
  } finally { $winType::WTSFreeMemory($buf) }
}
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
# The hang rule. HangVerdict is pure: true only when both numbers have stood still for the
# limit at the same moment. run.ps1 always calls it with the constant 300. A sample that
# could not be read restarts both clocks, so an unknown never counts toward a hang.
function HangVerdict($lastLChangeUtc, $lastCChangeUtc, $nowUtc, $limitSeconds) {
  if ($null -eq $lastLChangeUtc -or $null -eq $lastCChangeUtc) { return $false }
  return ((($nowUtc - $lastLChangeUtc).TotalSeconds -ge $limitSeconds) -and (($nowUtc - $lastCChangeUtc).TotalSeconds -ge $limitSeconds))
}
# The verdict of a Run and its exit code, pure, from what the run recorded. The first that
# applies in the order refused, a fault before the constructor, 3 not adopted, 6 not put
# back, 4 hung, a blocked call or the ceiling, 7 ended by itself, 1 UNKNOWN, still running
# at the end or a fault, 5 a dialog, 0. The ceiling and a blocked call are read off what the
# watchdog forced, so a process it closed is never written as one that ended by itself, and
# never as HUNG. EndState is the adopted process's state read after every close path.
function RunVerdict($v) {
  $r = [pscustomobject]@{ Text = ""; Code = 1 }
  $end = [string]$v.EndState
  if ($v.StopText -ne "") { $r.Text = "NOT RUN, " + $v.StopText; $r.Code = 2 }
  elseif (-not $v.Called) { $r.Text = "NOT RUN, a fault before the constructor, " + $v.Fault; $r.Code = 1 }
  elseif (-not $v.Adopted) { $r.Text = "NOT ADOPTED"; $r.Code = 3 }
  elseif ($v.NotPutBack) { $r.Text = "STOPPED, something of Bader's was not put back, NOT PUT BACK" + $(if ($end -ne "gone") { ", and the adopted Navisworks reads " + $end + " after every close path" } else { "" }); $r.Code = 6 }
  elseif ($v.RunOver -eq "HUNG") { $r.Text = "STOPPED, HUNG"; $r.Code = 4 }
  elseif ([string]$v.CallForced -ne "") { $r.Text = "STOPPED, " + $v.CallForced; $r.Code = 4 }
  elseif ($v.RunOver -eq "CEILING" -or $v.Forced -ne "") { $r.Text = "STOPPED, CEILING, " + $v.Forced; $r.Code = 4 }
  elseif ($v.RunOver -eq "GONE") { $r.Text = "STOPPED, the adopted Navisworks ended by itself before the hold"; $r.Code = 7 }
  elseif ($v.RunOver -eq "UNKNOWN") { $r.Text = "STOPPED, UNKNOWN whether the adopted Navisworks still runs, it could not be read through the held handle"; $r.Code = 1 }
  elseif ($end -ne "gone") { $r.Text = "STOPPED, the adopted Navisworks reads " + $end + " after every close path, and is written down"; $r.Code = 1 }
  elseif ($v.Fault -ne "" -or $v.RunOver -ne "HOLD" -or @($v.FinallyFaults).Count -gt 0) { $r.Text = ("STOPPED, a fault in run.ps1, " + $v.Fault + " " + $v.MonitorFault + " " + (@($v.FinallyFaults) -join ", and ")).Trim(); $r.Code = 1 }
  elseif ($v.Dialogs -gt 0) { $r.Text = "RAN, with " + $v.Dialogs + " DIALOG findings"; $r.Code = 5 }
  else { $r.Text = "RAN, item 0 with no window: started, adopted, held " + $v.HoldSeconds + " s, closed" + $(if ($v.ClosedHere -ne "") { ", FORCED" } else { " by Dispose" }) + ", put back"; $r.Code = 0 }
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
function NewClocks($nowUtc) { return [pscustomobject]@{ L = $null; C = $null; LChange = $nowUtc; CChange = $nowUtc; Unknown = $false } }
function NextClocks($clk, $L, $C, $nowUtc) {
  $n = [pscustomobject]@{ L = $clk.L; C = $clk.C; LChange = $clk.LChange; CChange = $clk.CChange; Unknown = $false }
  if ($null -eq $L -or $null -eq $C) { $n.Unknown = $true; $n.LChange = $nowUtc; $n.CChange = $nowUtc; return $n }
  if ($L -ne $clk.L) { $n.L = $L; $n.LChange = $nowUtc }
  if ($C -ne $clk.C) { $n.C = $C; $n.CChange = $nowUtc }
  return $n
}

# The tool's log: the one run-*.log in the logs folder that was not there before, was made at
# or after the call, and whose first line names its own path.
function FindToolLog($folder, $before, $callStartUtc) {
  $found = New-Object System.Collections.Generic.List[string]
  if (-not (Test-Path -LiteralPath $folder)) { return ,$found }
  foreach ($f in @(Get-ChildItem -LiteralPath $folder -Filter "run-*.log" -File -Force)) {
    if ($before.ContainsKey($f.Name.ToLowerInvariant())) { continue }
    if ($f.CreationTimeUtc -lt $callStartUtc) { continue }
    $first = $null
    $fs = New-Object System.IO.FileStream($f.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
    try { $sr = New-Object System.IO.StreamReader($fs); $first = $sr.ReadLine() } finally { $fs.Dispose() }
    if ($null -ne $first -and $first.IndexOf("Log opened at " + $f.FullName, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $found.Add($f.FullName) }
  }
  return ,$found
}

# The monitor, on its own runspace from adoption to the end of the run. It never touches the
# COM object. Every 15 s it reads the adopted process's processor time through the held
# handle, the tool's log once there is one, and the adopted process's windows, and once a
# minute the session lock. It ends the run at the first of: the process gone, a hang, the
# ceiling the watchdog closed, or item 0's fixed hold.
function Monitor($sync) {
  $landmark = '^\S+\s+\+\S+\s+(RETAIN|SESSION|RUN SETTINGS|GROUP|RESULT|COPY)|Window closed\.'
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
  $lastBeat = [DateTime]::UtcNow
  $lastLock = [DateTime]::MinValue
  $beatC = $null; $beatL = $null; $lastC = $null; $lastLine = ""
  M ("MONITOR started, a pass every " + $sync.PassSeconds + " s, hang limit " + $sync.HangLimit + " s, fixed hold " + $sync.HoldSeconds + " s")
  # Item 0 calls no plugin, so any new log in the logs folder is Bader's by construction: no
  # tool's log is read and no hang clock starts on one. How items 1 to 5 tell the loop's own
  # log from his is part 2's to settle.
  if (-not $sync.ReadToolLog) { M "no tool's log is read and no hang clock starts, because this run calls no plugin, so any new log in the logs folder is Bader's" }
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
      if ($sync.ReadToolLog -and $null -eq $logPath) {
        $found = $null
        try { $found = FindToolLog $sync.LogsFolder $sync.LogsBefore $sync.CallStartUtc } catch { M ("the logs folder could not be read, UNKNOWN, " + (Err $_.Exception)) }
        if ($null -ne $found -and $found.Count -gt 1) { M ("FINDING: " + $found.Count + " new logs name themselves, so none is taken as the tool's and nothing is ever removed from the folder") ; $sync.LogAmbiguous = $true }
        elseif ($null -ne $found -and $found.Count -eq 1) {
          $logPath = $found[0]
          $sync.ToolLog = $logPath
          [System.IO.File]::WriteAllText((Join-Path $sync.RunDir "toollog-name.txt"), (Split-Path $logPath -Leaf) + "`r`n", (New-Object System.Text.UTF8Encoding($false)))
          $stream = New-Object System.IO.FileStream($logPath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
          $clk = NewClocks $now
          M ("the tool's log is found, " + (Split-Path $logPath -Leaf) + ". Both clocks of the hang rule start now")
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
              if ($ln -match $landmark) { M ("LOG " + $ln) }
            }
          }
        } catch { $L = $null; M ("the log's length UNKNOWN, " + (Err $_.Exception)) }
      }
      if ($null -ne $clk) {
        $clk = NextClocks $clk $L $c $now
        if ($clk.Unknown) { M "a sample could not be read, UNKNOWN, so both clocks start again" }
        if (HangVerdict $clk.LChange $clk.CChange $now $sync.HangLimit) {
          $flat = $clk.LChange; if ($clk.CChange -gt $flat) { $flat = $clk.CChange }
          M ("HANG: the log stood at " + $clk.L + " bytes and the processor time at " + ([double]$clk.C / 1e7).ToString("0.000") + " s, both flat since " + $flat.ToLocalTime().ToString("HH:mm:ss") + ", " + $sync.HangLimit + " s")
          $hangLines = New-Object System.Collections.Generic.List[string]
          foreach ($t in $tail) { $hangLines.Add($t) }
          $hangLines.Add("---- the adopted process's visible windows at the hang ----")
          foreach ($w in (WindowLines $sync.WinType $sync.ProcType ([uint32]$sync.MyPid) $true $true)) { $hangLines.Add($w) }
          [System.IO.File]::WriteAllLines((Join-Path $sync.RunDir "hang-tail.txt"), $hangLines.ToArray(), (New-Object System.Text.UTF8Encoding($false)))
          $cr = CloseAdopted $sync.MyProc $sync.MyTicks 30
          M ("the adopted process, closed through the held handle for the hang: " + $cr.Text)
          $sync.RunOver = "HUNG"
          break
        }
      }
      $cText = "UNKNOWN"; if ($null -ne $c) { $cText = ([double]$c / 1e7).ToString("0.000") + " s" }
      $grow = ""; if ($null -ne $c -and $null -ne $lastC) { $grow = ", " + ([double]($c - $lastC) / 1e7).ToString("0.000") + " s since the last pass" }
      $lText = ""; if ($null -ne $logPath) { $lText = ", the log " + $L + " bytes" }
      M ("SAMPLE processor " + $cText + $grow + $lText)
      $lastC = $c
      foreach ($r in (WindowRecords $sync.WinType $sync.ProcType ([uint32]$sync.MyPid) $true $true)) {
        $key = [string]$r.Handle + "|" + $r.Class + "|" + $r.Caption
        if ($seenWins.ContainsKey($key)) { continue }
        $seenWins[$key] = $true
        $kind = WindowKind $r.Class $r.Caption $r.OwnerHandle $r.OwnerVisible
        $texts = "UNKNOWN, no child window answered"
        if ($null -ne $r.Texts -and @($r.Texts).Count -gt 0) { $texts = (@($r.Texts) -join " ") }
        $ownerText = OwnerText $r ([uint32]$sync.MyPid)
        if ($kind -eq "DIALOG") {
          $sync.Dialogs = $sync.Dialogs + 1
          M ("DIALOG: class " + $r.Class + ", caption `"" + $r.Caption + "`", " + $ownerText + ", text " + $texts)
        } else { M ($kind + ": class " + $r.Class + ", caption `"" + $r.Caption + "`", " + $ownerText) }
      }
      if (($now - $lastLock).TotalSeconds -ge 60) { $lastLock = $now; M ("SESSION LOCK " + (SessionLockText $sync.WinType)) }
      if (($now - $lastBeat).TotalSeconds -ge $sync.BeatSeconds) {
        $lastBeat = $now
        $cg = ""; if ($null -ne $c -and $null -ne $beatC) { $cg = ", growth " + ([double]($c - $beatC) / 1e7).ToString("0.000") + " s" }
        $lg = ""; if ($null -ne $L -and $null -ne $beatL) { $lg = ", growth " + ($L - $beatL) + " bytes" }
        $still = "the clocks have not started, there is no tool's log"
        if ($null -ne $clk) { $still = "the log still for " + ($now - $clk.LChange).TotalSeconds.ToString("0") + " s and the processor for " + ($now - $clk.CChange).TotalSeconds.ToString("0") + " s, against " + $sync.HangLimit }
        $lw = ""
        try { $wl = @((ReadShared $sync.WatchFile).Split("`n") | ForEach-Object { $_.TrimEnd("`r") } | Where-Object { $_ -ne "" }); if ($wl.Count -gt 0) { $lw = $wl[$wl.Count - 1] } } catch { $lw = "UNKNOWN, " + (Err $_.Exception) }
        $ll = $lastLine; if ($ll.Length -gt 120) { $ll = $ll.Substring(0, 120) }
        M ("HEARTBEAT the log " + $(if ($null -ne $L) { [string]$L + " bytes" + $lg } else { "none" }) + ", its last line `"" + $ll + "`", processor " + $cText + $cg + ", " + $still + ", the watchdog's last line: " + $lw)
        $beatC = $c; $beatL = $L
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
# What the window rule rests on, for every window it classes: its owner's class, caption,
# visibility, state and process, read without a message. An owner of another process is
# masked, its class, caption and process left out, so no other program's window reaches
# the record.
function OwnerText($r, [uint32]$myPid) {
  if ($r.OwnerHandle -eq [IntPtr]::Zero) { return "owner none" }
  if ($r.OwnerPid -ne $myPid) { return ("owner " + $r.Owner + ", a window of another process, its class, caption and process not written") }
  return ("owner " + $r.Owner + ", class " + $r.OwnerClass + ", caption `"" + $r.OwnerCaption + "`", visible " + $r.OwnerVisible + ", enabled " + $r.OwnerEnabled + ", process the adopted one")
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
# A line another program printed, with the three profile folders written by their names, so
# no user's folder reaches a record. A line that still holds a path is masked whole.
function MaskLine($t) {
  $s = [string]$t
  foreach ($pair in @(@($env:LOCALAPPDATA, "%LOCALAPPDATA%"), @($env:APPDATA, "%APPDATA%"), @($env:USERPROFILE, "%USERPROFILE%"))) {
    if ([string]$pair[0] -ne "") { $s = [regex]::Replace($s, [regex]::Escape(([string]$pair[0]).TrimEnd('\')), $pair[1], [System.Text.RegularExpressions.RegexOptions]::IgnoreCase) }
  }
  if ($s -match '[A-Za-z]:\\|\\\\') { return (Mask $s) }
  return $s
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
function CheckMode($paths, $stamp) {
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
  Say "---- the refusals Run would give, in Run's order ----"
  $r = RunRefusals $paths $stamp $false
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
$paramWhy = ParamRefusal $Mode $Set $Item $Stamp $RunFolder $args $loopRoot
if ($paramWhy.Count -gt 0) {
  foreach ($w in $paramWhy) { Say ("REFUSED: " + $w + ". Nothing was started and nothing was written.") }
  exit 2
}
$Mode = ModeOf $Mode
$paths = RunPaths $loopRoot $repo $Set $Item

if ($Mode -eq "Check") { exit (CheckMode $paths $Stamp) }

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
  if ($Mode -eq "Run") {
    $sync = $null; $wps = $null; $whandle = $null; $mon = $null
    $app = $null; $myPid = 0; $myTicks = $null; $goneAtUtc = $null
    $disposed = $false; $suppressed = $false; $called = $false; $adopted = $false
    $ka = $null; $bs = $null; $logsBefore = $null; $keysBefore = $null
    $stopText = ""; $fault = ""; $forced = ""; $spb = $null; $logsChanged = $false; $putBackFailed = $false; $kaOff = $false
    do {
      Say "---- checks 4 to 11 ----"
      $rr = RunRefusals $paths $Stamp $true
      if ($rr.Count -gt 0) { Say ("REFUSED: " + $rr[0] + "."); $code = 2; break }
      $autoDll = Join-Path $nw "Autodesk.Navisworks.Automation.dll"
      if (-not (Test-Path -LiteralPath $autoDll)) { Say ("REFUSED: there is no Autodesk.Navisworks.Automation.dll at " + $autoDll + ". Nothing was started and nothing was written."); $code = 2; break }
      $napp = [System.Reflection.Assembly]::LoadFrom($autoDll).GetType("Autodesk.Navisworks.Api.Automation.NavisworksApplication")
      if ($null -eq $napp) { Say "REFUSED: the Automation DLL holds no NavisworksApplication. Nothing was started and nothing was written."; $code = 2; break }

      # check 12, the run folder. What is moved aside is said once the new record has started,
      # so the record and the evidence copied from it carry it.
      $asideLines = New-Object System.Collections.Generic.List[string]
      if (Test-Path -LiteralPath $paths.RunDir) {
        $aside = (Split-Path $paths.RunDir -Leaf) + "-aside-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
        try { Rename-Item -LiteralPath $paths.RunDir -NewName $aside -ErrorAction Stop } catch { Say ("REFUSED: the run folder from an earlier call could not be moved aside, " + (Err $_.Exception) + ". Nothing was touched."); $code = 2; break }
        $asideLines.Add("  the run folder from an earlier call was moved aside as " + $aside)
      }
      # The evidence of an earlier call that was NOT RUN, which check 11 let through, moved
      # aside the same way and never emptied.
      if ((EvidenceFiles $paths.Evidence) -gt 0 -and (EvidenceNotRun $paths.Evidence)) {
        $evAside = (Split-Path $paths.Evidence -Leaf) + "-aside-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
        try { Rename-Item -LiteralPath $paths.Evidence -NewName $evAside -ErrorAction Stop } catch { foreach ($al in $asideLines) { Say $al }; Say ("REFUSED: the evidence of an earlier call that was NOT RUN could not be moved aside, " + (Err $_.Exception) + ". Nothing else was touched."); $code = 2; break }
        $asideLines.Add("  the evidence of an earlier call that was NOT RUN was moved aside as steps\runs\" + $Set + "\" + $evAside)
      }
      New-Item -ItemType Directory -Path $paths.RunDir -ErrorAction Stop | Out-Null
      $script:RecordFile = Join-Path $paths.RunDir "record.txt"
      Say ("RUN RECORD, run.ps1 sha256 " + $runSha + ", nw-guard.ps1 sha256 " + $guardSha + ", -Mode Run -Set " + $Set + " -Item 0 -Stamp " + $Stamp + ", the start with no window")
      Say ("  the run folder " + (Mask $paths.RunDir) + ", every line from here is also in its record.txt")
      foreach ($al in $asideLines) { Say $al }

      $wt = NewWinTypes
      $procType = $wt.ProcType
      $winType = $wt.WinType
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
        $when = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
        Say "---- check 13, his logs folder ----"
        $lb = BackupNew $paths.HisLogs $paths.LogsBackup (Join-Path $paths.RunDir "logs-before.txt") $when
        if (-not $lb.Ok) { $stopText = "STOP before the start: " + $lb.Why + ", so the tool's logs folder could not be copied into logs-backup and read back with its sha256. Nothing of his was changed, and the partial copy stays in the loop folder"; Say $stopText; $code = 2; break }
        Say ("  " + $lb.Listed + " files listed into logs-before.txt, " + $lb.Copied + " copied into logs-backup and read back")
        $logsBefore = @{}
        foreach ($row in (ListingRows (Join-Path $paths.RunDir "logs-before.txt"))) { $logsBefore[$row.Name.ToLowerInvariant()] = $row.Line }
        Say "---- check 14, his AutoSave folder ----"
        $ab = BackupNew $paths.AutoSave $paths.AutoBackup (Join-Path $paths.RunDir "autosave-before.txt") $when
        if (-not $ab.Ok) { $stopText = "STOP before the start: " + $ab.Why + ", so the AutoSave folder could not be copied into autosave-backup and read back with its sha256. Nothing of his was changed"; Say $stopText; $code = 2; break }
        Say ("  " + $ab.Listed + " files listed into autosave-before.txt, " + $ab.Copied + " copied into autosave-backup and read back. Nothing is ever written into his AutoSave folder")
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
        # process through the held handle if the call has not returned in 120 s.
        $sync.CallLimit = $CallLimitSeconds; $sync.CallSinceUtc = [DateTime]::UtcNow; $sync.CallName = "Visible"
        try { $app.Visible = $true; Say ("  Visible set True, read back " + $app.Visible) } catch { Say ("  FINDING: setting Visible threw, " + (Err $_.Exception)) } finally { $sync.CallName = "" }
        if ($sync.CallForced -ne "") { Say ("  " + $sync.CallForced) }

        $sync.RunText = FunctionText $PSCommandPath
        $sync.MonitorStop = $false; $sync.RunOver = ""; $sync.MonitorFault = ""; $sync.MonitorWriteFails = 0
        $sync.Dialogs = 0; $sync.ToolLog = $null; $sync.LogAmbiguous = $false; $sync.ReadToolLog = $false
        $sync.LogsFolder = $paths.HisLogs; $sync.LogsBefore = $logsBefore
        $sync.HangLimit = $HangLimitSeconds; $sync.PassSeconds = $PassSeconds; $sync.HoldSeconds = $HoldSeconds; $sync.BeatSeconds = $BeatSeconds
        $mps = [PowerShell]::Create()
        [void]$mps.AddScript((MonitorScript)).AddArgument($sync)
        $mon = [pscustomobject]@{ Ps = $mps; Handle = $mps.BeginInvoke() }
        while ($sync.RunOver -eq "" -and -not $mon.Handle.IsCompleted) { Start-Sleep -Milliseconds 500 }
        Say ("==== THE RUN ENDED: " + $sync.RunOver + " ====")
        if ($sync.RunOver -eq "HOLD") {
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
              } catch { $putBackFailed = $true; Say ("  the compare stopped, " + (Err $_.Exception) + ". Nothing more is written, the backup is kept in the run folder") }
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
              $logsChanged = ((($beforeLines | Sort-Object) -join "`n") -ne (($afterLines | Sort-Object) -join "`n"))
              Say ("  logs-after.txt equals logs-before.txt, name for name, size, write time, sha256 and attributes: " + (-not $logsChanged))
              if ($logsChanged) {
                $relOf = @{}
                foreach ($row in $beforeRows) { $relOf[$row.Line] = $row.Rel }
                foreach ($row in $afterRows) { $relOf[$row.Line] = $row.Rel }
                foreach ($d in @(Compare-Object $beforeLines $afterLines)) { Say ("    FINDING " + $d.SideIndicator + " " + $relOf[[string]$d.InputObject]) }
              }
            } else { $logsChanged = $true; Say ("  UNKNOWN, his logs folder: " + $la.Why) }
          }
        } catch { $logsChanged = $true; $finallyFaults.Add("the logs compare, " + (Err $_.Exception)) }
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
        if ($null -ne $spb) { if ($spb.NotWritten -gt 0 -or $spb.AutoSave -gt 0) { $notPutBack = $true } }
        $endState = "gone"
        if ($adopted -and $null -ne $sync -and $null -ne $sync.MyProc) { $endState = HeldState $sync.MyProc $myTicks }
        $vd = RunVerdict ([pscustomobject]@{ StopText = $stopText; Called = $called; Adopted = $adopted; NotPutBack = $notPutBack; RunOver = [string]$sync.RunOver; Forced = [string]$sync.Forced; CallForced = [string]$sync.CallForced; Fault = $fault; MonitorFault = [string]$sync.MonitorFault; FinallyFaults = @($finallyFaults); Dialogs = [int]$sync.Dialogs; ClosedHere = $forced; HoldSeconds = $HoldSeconds; EndState = $endState })
        $code = $vd.Code
        Say ("  lines that could not be written to record.txt: " + $script:SayFailures + ", by the monitor: " + $sync.MonitorWriteFails)
        Say ("VERDICT: " + $vd.Text)
        $script:RecordFile = $null
        try {
          $ev = $paths.Evidence
          New-Item -ItemType Directory -Force -Path $ev | Out-Null
          foreach ($n in @("record.txt", "watch.txt", "settings.txt")) {
            $src = Join-Path $paths.RunDir $n
            if (Test-Path -LiteralPath $src) { Copy-Item -LiteralPath $src -Destination (Join-Path $ev $n); Say ("  evidence " + $n + ", " + (Get-Item -LiteralPath (Join-Path $ev $n)).Length + " bytes") }
          }
          Say ("  the evidence is in steps\runs\" + $Set + "\item0. Mask it with F102's tool before it is committed")
        } catch { Say ("  FAULT: the evidence could not be copied into steps\runs, " + (Err $_.Exception) + ". It is all in the run folder"); if ($code -eq 0) { $code = 1 } }
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
