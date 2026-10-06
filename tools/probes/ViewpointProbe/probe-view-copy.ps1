param(
  [string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025",
  [string]$Out = "",
  [string]$Nwf = "",
  [string]$PluginAssembly = "",
  [int]$QuitWaitSeconds = 60,
  [int]$ConstructorDeadlineSeconds = 300,
  [int]$AdoptedDeadlineSeconds = 1800
)
$ErrorActionPreference = "Stop"

# P11 of Q114, scan.md 5z-q, 2026-10-05. Does AddCopy of a marked view give an item with a new
# Guid, and does its comment travel with it? The comment travels and the Guid is new: the copy
# fails the fingerprint and is a person's. The Guid is kept: the Guid stays out of the mark.
#
# It is probe-view-guids.ps1, P10's, with the mode vpcopy of tools\probes\ViewpointProbe in place
# of P10's mode, because the row of P11 works on P9's saved copy too, probes\view-comments-
# 20261005-144832\p9-copy-with-comments.nwf. The plugin reads P9's marked COM view and F85's view
# P9 marked, makes a marked view whose Guid it sets, copies each by AddCopy of the item itself, of
# CreateCopy and of CreateUniqueCopy into a folder of its own, saves the copy as p11-copy-saved.nwf
# in the work folder, clears the document and reopens the saved file, reading each copy's Guid and
# comments beside its source's at every stage. The guards are the loop's, from
# tools\loop\nw-guard.ps1, dot-sourced, in the order
# tools\probes\probe-automation-start.ps1 keeps them, and that probe's header says each rule in full:
#   - refused unless it is the script its own powershell.exe was started to run with -File,
#     and unless both deadlines are at least 60 s
#   - refused while a start written in probes\unproved-starts.txt still runs, and while ANY
#     Roamer runs, read before the watchdog and again just before the constructor. Nothing is
#     attached to or sent to a Navisworks this probe did not start
#   - Bader's settings backed up by BackupSettings before anything starts, and put back at
#     the end by SettingsPutBack only when PutBackReasons finds no other Navisworks ran, each
#     registry write read again after it. An AutoSave file added is listed, never deleted
#   - the one Navisworks adopted by AdoptStart's four conditions, quit by Dispose, and closed
#     through the handle the adoption holds only when it is still the same process after
#     Dispose, a step failed or the adopted deadline passed. Nothing is closed before adoption
#
# -Nwf must lie under %LOCALAPPDATA%\NwcFederatorLoop\probes. It is copied, the copy is opened,
# and the original's sha256 is read before and after. The models the copy names are read
# where the NWF points, and the plugin says for each whether that is under the loop folder.
# Everything written goes under %LOCALAPPDATA%\NwcFederatorLoop\probes\view-copy-
# yyyyMMdd-HHmmss, a new folder every call, bar -Out and what is put back. Run it with
# Windows PowerShell 5.1, 64 bit, STA:
#
#   powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\probe-view-copy.ps1 -Out <result> -Nwf <an NWF under probes>

$nw = $NavisworksPath.TrimEnd('\')
$autoDll = Join-Path $nw "Autodesk.Navisworks.Automation.dll"
$loopRoot = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$runsRoot = Join-Path $loopRoot "runs"
$probesRoot = Join-Path $loopRoot "probes"
$unproved = Join-Path $loopRoot "probes\unproved-starts.txt"
$T0 = [DateTime]::Now
$work = Join-Path $loopRoot ("probes\view-copy-" + $T0.ToString("yyyyMMdd-HHmmss"))
if ($PluginAssembly -eq "") { $PluginAssembly = Join-Path $PSScriptRoot "bin\Release\net48\ViewpointProbe.dll" }
$probeId = "ViewpointProbe.PARS"
$utf8 = New-Object System.Text.UTF8Encoding($false)
$script:sayFailures = 0
if ($Out -ne "") {
  $Out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
  [System.IO.File]::WriteAllText($Out, "", $utf8)
}

function Say($t) {
  [Console]::Out.WriteLine($t)
  if ($Out -ne "") {
    try { [System.IO.File]::AppendAllText($Out, $t + "`r`n", $utf8) }
    catch { $script:sayFailures++; [Console]::Error.WriteLine("could not append to " + $Out + ": " + $_.Exception.Message) }
  }
}
function Stamp { return ([DateTime]::Now.ToString("HH:mm:ss.fff") + "  t+" + ([DateTime]::Now - $T0).TotalSeconds.ToString("0.0") + "s") }

$guardFile = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "loop\nw-guard.ps1"
. $guardFile
$guardText = [System.IO.File]::ReadAllText($guardFile)

Say ("MACHINE   " + $env:COMPUTERNAME + "   " + $T0.ToString("yyyy-MM-dd HH:mm:ss"))
Say ("HOST      powershell " + $PSVersionTable.PSVersion + ", 64 bit process " + [Environment]::Is64BitProcess + ", apartment " + [System.Threading.Thread]::CurrentThread.ApartmentState + ", pid " + $PID)
Say ("SCRIPT    " + (Mask $PSCommandPath) + ", sha256 " + (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash)
Say ("GUARD     tools\loop\nw-guard.ps1, sha256 " + (Get-FileHash -LiteralPath $guardFile -Algorithm SHA256).Hash)

$refusals = New-Object System.Collections.Generic.List[string]
if ($ConstructorDeadlineSeconds -lt 60) { $refusals.Add("-ConstructorDeadlineSeconds is " + $ConstructorDeadlineSeconds + ", below 60") }
if ($AdoptedDeadlineSeconds -lt 60) { $refusals.Add("-AdoptedDeadlineSeconds is " + $AdoptedDeadlineSeconds + ", below 60") }
$nwfFull = $null
if ($Nwf -eq "") { $refusals.Add("-Nwf is empty") } else {
  $nwfFull = [System.IO.Path]::GetFullPath($Nwf)
  if (-not $nwfFull.StartsWith($probesRoot + "\", [StringComparison]::OrdinalIgnoreCase)) { $refusals.Add("-Nwf is not under " + $probesRoot) }
  elseif (-not [System.IO.File]::Exists($nwfFull)) { $refusals.Add("-Nwf is not there") }
}
if (-not [System.IO.File]::Exists($PluginAssembly)) { $refusals.Add("no built probe at " + $PluginAssembly + ", build tools\probes\ViewpointProbe\ViewpointProbe.csproj -c Release first") }
if (-not (Test-Path -LiteralPath $autoDll)) { $refusals.Add("no Autodesk.Navisworks.Automation.dll at " + $autoDll) }
if ([System.Threading.Thread]::CurrentThread.ApartmentState -ne "STA") { $refusals.Add("the thread is not STA, start powershell with -STA") }
OwnProcessRefusal $PSCommandPath $refusals
if ($refusals.Count -gt 0) { foreach ($r in $refusals) { Say ("REFUSED: " + $r + ". Nothing was started and nothing was written but this result.") }; exit 1 }
$napp = [System.Reflection.Assembly]::LoadFrom($autoDll).GetType("Autodesk.Navisworks.Api.Automation.NavisworksApplication")
if ($null -eq $napp) { Say "REFUSED: the Automation DLL holds no NavisworksApplication. Nothing was started."; exit 1 }
Say ""

$wt = NewWinTypes
$procType = $wt.ProcType
$winType = $wt.WinType

Say "==== STEP 2. Starts this probe could not prove, then every Roamer running before anything starts ===="
$outcome = [ordered]@{ "2" = "started, did not finish"; "3" = "not reached"; "4" = "not reached"; "5" = "not reached"; "6" = "not reached" }
$currentStep = "2"
$u = UnprovedRefusal $unproved
if ($u.Stop) { Say $u.Text; exit 1 }
$beforeStart = @{}
$beforeTicks = @{}
if (RoamerRefusal) {
  Say "STOP before the work folder, the watchdog, the backup and the constructor, so nothing is written down and nothing is started: a Navisworks is running, and no start is made while any Navisworks runs, whatever its command line and whoever started it"
  exit 1
}
$beforeAll = @(Get-Process | ForEach-Object { $_.Id })
Say ("  every process id on the machine taken as the before set: " + $beforeAll.Count)
$outcome["2"] = "passed, no Roamer running"
Say ""

Say "==== THE WORK FOLDER, new on every call ===="
if (Test-Path -LiteralPath $work) { Say ("STOP: " + (Mask $work) + " is there already, and a folder is never used twice. Call again"); exit 1 }
New-Item -ItemType Directory -Path $work -ErrorAction Stop | Out-Null
Say ("  " + (Mask $work) + " created empty")
$watchFile = Join-Path $work "watch.txt"
$pidFile = Join-Path $work "mypid.txt"
[System.IO.File]::WriteAllText($watchFile, "", $utf8)
Say ""

$sync = WatchSync $beforeAll $beforeTicks $T0 $loopRoot $nw $ConstructorDeadlineSeconds $AdoptedDeadlineSeconds $Out $unproved $watchFile $guardText $winType $procType
$wps = [PowerShell]::Create()
[void]$wps.AddScript((WatchdogScript)).AddArgument($sync)
$whandle = $wps.BeginInvoke()
function StopEarly($msg) {
  Say $msg
  $sync.Stop = $true
  try { [void]$wps.EndInvoke($whandle) } catch { Say ("  the watchdog ended with " + (Err $_.Exception)) }
  exit 1
}

Say "==== BADER'S SETTINGS, backed up before anything starts ===="
$regSub = "Software\Autodesk\Navisworks Manage\22.0"
$nwAppData = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025"
$fedLogs = Join-Path $env:LOCALAPPDATA "ParsonsNwcFederator\logs"
$bs = BackupSettings $work $regSub $nwAppData $fedLogs
if (-not $bs.Ok) { StopEarly $bs.Why }
$regRoot = $bs.RegRoot; $regBefore = $bs.RegBefore; $appBackup = $bs.AppBackup
$filesBefore = $bs.FilesBefore; $autoBefore = $bs.AutoBefore; $notBacked = $bs.NotBacked; $fedBefore = $bs.FedBefore
$sync.RegSub = $regSub; $sync.RegRoot = $regRoot; $sync.RegBefore = $regBefore; $sync.FilesBefore = $filesBefore
$sync.NotBacked = $notBacked; $sync.AutoBefore = $autoBefore; $sync.NwAppData = $nwAppData; $sync.SettingsReady = $true
Say ""

Say "==== THE COPIES, made before anything starts ===="
$nwfHashBefore = (Get-FileHash -LiteralPath $nwfFull -Algorithm SHA256).Hash
$nwfCopy = Join-Path $work (Split-Path $nwfFull -Leaf)
Copy-Item -LiteralPath $nwfFull -Destination $nwfCopy -ErrorAction Stop
$nwfCopyHash = (Get-FileHash -LiteralPath $nwfCopy -Algorithm SHA256).Hash
Say ("  the NWF " + (Mask $nwfFull) + ", " + (Get-Item -LiteralPath $nwfFull).Length + " bytes, sha256 " + $nwfHashBefore)
Say ("  its copy " + (Mask $nwfCopy) + ", sha256 " + $(if ($nwfCopyHash -eq $nwfHashBefore) { "matches" } else { "DIFFERS" }))
if ($nwfCopyHash -ne $nwfHashBefore) { StopEarly "STOP before the constructor: the copy of the NWF does not read back with its source's sha256. Nothing was started" }
$dllCopy = Join-Path $work "ViewpointProbe.dll"
Copy-Item -LiteralPath $PluginAssembly -Destination $dllCopy -ErrorAction Stop
$dllHash = (Get-FileHash -LiteralPath $PluginAssembly -Algorithm SHA256).Hash
$dllCopyHash = (Get-FileHash -LiteralPath $dllCopy -Algorithm SHA256).Hash
Say ("  the probe " + (Mask $PluginAssembly) + ", " + (Get-Item -LiteralPath $PluginAssembly).Length + " bytes, written " + (Get-Item -LiteralPath $PluginAssembly).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + ", sha256 " + $dllHash + ", its copy " + $(if ($dllCopyHash -eq $dllHash) { "matches" } else { "DIFFERS" }))
if ($dllCopyHash -ne $dllHash) { StopEarly "STOP before the constructor: the copy of the probe does not read back with its source's sha256. Nothing was started" }
$pluginOut = Join-Path $work "plugin-output.txt"
Say ""

$app = $null; $myPid = 0; $myStart = $null; $myTicks = $null; $goneAtUtc = $null
$disposed = $false; $suppressed = $false; $ctorSeconds = $null
Say "==== THE LAST READ BEFORE THE CONSTRUCTOR, the same as step 2 ===="
if (RoamerRefusal) { StopEarly "STOP before the constructor, so nothing is started and nothing of Bader's is written, the backup stays in the work folder: a Navisworks is running" }
Say ""
try {
  Say "==== STEP 3. Start one Navisworks through the API and adopt it ===="
  $currentStep = "3"; $outcome["3"] = "started, did not finish"
  Say ("  " + (Stamp) + "  calling new NavisworksApplication()")
  $sync.CallStartUtc = [DateTime]::UtcNow
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app = [Activator]::CreateInstance($napp) } catch { $err = $_.Exception }
  $sync.CtorReturned = $true
  $sw.Stop()
  $ctorSeconds = $sw.Elapsed.TotalSeconds
  Say ("  " + (Stamp) + "  the constructor " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $ctorSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }
  $ad = AdoptStart $err $app $sync $pidFile
  if (-not $ad.Adopted) {
    if ($ad.Suppressed) { $suppressed = $true }
    $outcome["3"] = "failed, not adopted: returned " + $ad.C1 + ", one possible start " + $ad.C2 + ", after the call " + $ad.C3 + ", -Embedding " + $ad.C4
    throw "step 3 stopped"
  }
  $myPid = $ad.Pid; $myStart = $ad.Start; $myTicks = $ad.Ticks
  Say ("  ADOPTED, MY PID IS " + $myPid + ", started " + $myStart.ToString("yyyy-MM-dd HH:mm:ss.fff") + ", start ticks UTC " + $myTicks)
  $app.Visible = $true
  Say ("  " + (Stamp) + "  Visible set True, read back " + $app.Visible)
  WindowsOf $myPid $myTicks
  $outcome["3"] = "passed"
  Say ""

  Say "==== STEP 4. AddPluginAssembly with the probe's copy ===="
  $currentStep = "4"; $outcome["4"] = "started, did not finish"
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.AddPluginAssembly($dllCopy) } catch { $err = $_.Exception }
  Say ("  " + (Stamp) + "  AddPluginAssembly " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.000") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)); $outcome["4"] = "failed, AddPluginAssembly threw"; throw "step 4 stopped" }
  $outcome["4"] = "passed"
  Say ""

  Say "==== STEP 5. ExecuteAddInPlugin, the mode vpcopy on the copy, which it edits and saves as a new file ===="
  $currentStep = "5"; $outcome["5"] = "started, did not finish"
  $saveAs = Join-Path $work "p11-copy-saved.nwf"
  Say ("  " + (Stamp) + "  calling ExecuteAddInPlugin(" + $probeId + ", vpcopy, the plugin's output file, the copy, the save path in the work folder)")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null; $ret = $null
  try { $ret = $app.ExecuteAddInPlugin($probeId, [string[]]@("vpcopy", $pluginOut, $nwfCopy, $saveAs)) } catch { $err = $_.Exception }
  Say ("  " + (Stamp) + "  ExecuteAddInPlugin " + $(if ($null -eq $err) { "RETURNED " + $ret } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }
  WindowsOf $myPid $myTicks
  Say "---- the plugin's output, every line ----"
  if ([System.IO.File]::Exists($pluginOut)) { foreach ($l in [System.IO.File]::ReadAllLines($pluginOut)) { Say ("  | " + (MaskLine $l)) } } else { Say "  the plugin wrote no output file" }
  if ([System.IO.File]::Exists($saveAs)) {
    Say ("  the saved copy " + (Mask $saveAs) + ", " + (Get-Item -LiteralPath $saveAs).Length + " bytes, sha256 " + (Get-FileHash -LiteralPath $saveAs -Algorithm SHA256).Hash)
  } else { Say "  the plugin saved no copy" }
  if ($null -ne $err) { $outcome["5"] = "failed, ExecuteAddInPlugin threw"; throw "step 5 stopped" }
  $outcome["5"] = "passed, returned " + $ret
  Say ""

  Say "==== STEP 6. Quit through the API ===="
  $currentStep = "6"; $outcome["6"] = "started, did not finish"
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.Dispose(); $disposed = $true } catch { $err = $_.Exception }
  Say ("  " + (Stamp) + "  Dispose " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }
  $sw = [Diagnostics.Stopwatch]::StartNew()
  while ((ProcState $myPid $myTicks) -eq "same" -and $sw.Elapsed.TotalSeconds -lt $QuitWaitSeconds) { Start-Sleep -Milliseconds 250 }
  $st6 = ProcState $myPid $myTicks
  if ($st6 -eq "gone") { $goneAtUtc = [DateTime]::UtcNow }
  if ($st6 -eq "same") {
    $cr6 = CloseAdopted $sync.MyProc $myTicks 30
    $stAfter = ProcState $myPid $myTicks
    if ($stAfter -eq "gone") { $goneAtUtc = [DateTime]::UtcNow }
    Say ("  " + (Stamp) + "  pid " + $myPid + " was STILL RUNNING " + $QuitWaitSeconds + " s after Dispose, closed through the held handle: " + $cr6.Text + ". State after: " + $stAfter)
    $outcome["6"] = "failed, had to be closed through the held handle"
  } else {
    Say ("  " + (Stamp) + "  pid " + $myPid + " state " + $st6 + ", " + $sw.Elapsed.TotalSeconds.ToString("0.0") + " s after Dispose returned, not forced")
    $outcome["6"] = "passed, state " + $st6
  }
  Say ""
} catch {
  if ($outcome.Contains($currentStep) -and $outcome[$currentStep] -eq "started, did not finish") { $outcome[$currentStep] = "failed, " + (Err $_.Exception) }
  Say ("  " + (Stamp) + "  stopped: " + (Err $_.Exception))
  Say ""
} finally {
  # Close first, write after, so nothing written can stop the close. Only the adopted.
  $closeNote = ""
  if ($myPid -ne 0) {
    $sf = ProcState $myPid $myTicks
    if ($sf -eq "same") {
      $crf = CloseAdopted $sync.MyProc $myTicks 30
      if ($null -ne $app -and -not $disposed) { [GC]::SuppressFinalize($app); $suppressed = $true }
      $sfAfter = ProcState $myPid $myTicks
      if ($sfAfter -eq "gone" -and $null -eq $goneAtUtc) { $goneAtUtc = [DateTime]::UtcNow }
      $closeNote = "pid " + $myPid + " was still the same process, CLOSED here through the held handle: " + $crf.Text + ", state after: " + $sfAfter
    } else {
      if ($sf -eq "gone" -and $null -eq $goneAtUtc) { $goneAtUtc = [DateTime]::UtcNow }
      $closeNote = "pid " + $myPid + " state " + $sf + ", nothing to close"
    }
  } else { $closeNote = "no pid was adopted, so nothing is closed here" }
  $watchEndedEarly = $whandle.IsCompleted
  $sync.Stop = $true
  Say "==== FINALLY ===="
  Say ("  " + (Stamp) + "  " + $closeNote)
  $wpsEndError = $null
  try { [void]$wps.EndInvoke($whandle) } catch { $wpsEndError = (Err $_.Exception); Say ("  the watchdog ended with " + $wpsEndError) }
  $wpsErrors = @($wps.Streams.Error)
  $wps.Dispose()
  Say ("  watchdog forced anything: " + $(if ($sync.Forced -eq "") { "no" } else { $sync.Forced }))
  Say ("  watchdog passes " + $sync.Passes + ", longest pass " + ([double]$sync.PassMaxMs).ToString("0") + " ms, longest gap between the starts of two passes " + ([double]$sync.MaxGapMs).ToString("0") + " ms, error lines " + $sync.ErrorCount + ", passes that failed before their process loop " + $sync.EarlyFails + ", errors in its runspace " + $wpsErrors.Count + ", lines it could not write " + $sync.WriteErrors.Count)
  foreach ($we in $wpsErrors) { Say ("    runspace error: " + $we.ToString()) }
  Say "---- the watchdog's record ----"
  try { foreach ($l in [System.IO.File]::ReadAllLines($watchFile)) { Say ("  " + $l) } } catch { Say ("  the watchdog's record could not be read, " + (Err $_.Exception)) }
  Say "---- every process the watchdog recorded, at the end ----"
  foreach ($s in $sync.Seen) {
    $state = "UNKNOWN"
    if ($s.Ticks -eq 0) { $state = "UNKNOWN whether it exited, its start time could not be read when it was seen" } else {
      $pp = Get-Process -Id $s.Id -ErrorAction SilentlyContinue
      if ($null -eq $pp) { $state = "exited" } else {
        try { $pst = $pp.StartTime; if ($null -eq $pst) { $state = "UNKNOWN, its start time reads as nothing" } elseif ((UtcTicks $pst) -eq $s.Ticks) { $state = "STILL RUNNING" } else { $state = "exited, the id is now another process" } } catch { $state = "UNKNOWN, " + (Err $_.Exception) }
      }
    }
    Say ("  " + $s.Name + " pid " + $s.Id + ", parent " + $s.Parent + ": " + $state)
  }
  if ($sync.Seen.Count -eq 0) { Say "  none" }
  $lateNew = @(NewRoamers)
  Say ("  Roamers running now that were not in step 2: " + $lateNew.Count)
  foreach ($p in $lateNew) { Say ("    pid " + $p.Id + ", not closed by this probe") }
  Say "---- STARTS THIS PROBE COULD NOT PROVE, written down for later runs ----"
  UnprovedAtEnd $unproved $myPid $myTicks $lateNew $sync
  Say ""
  Say "---- BADER'S SETTINGS, compared, and put back only when no other Navisworks ran ----"
  try {
    $pr = PutBackReasons $sync $wpsErrors $wpsEndError $watchEndedEarly $beforeTicks $myPid $myTicks $goneAtUtc
    $goneAtUtc = $pr.GoneAtUtc
    $null = SettingsPutBack ($pr.Why.Count -eq 0) $pr.Why $work $regSub $regBefore $regRoot $nwAppData $filesBefore $notBacked $autoBefore $appBackup
  } catch { Say ("  the compare stopped, " + (Err $_.Exception) + ". Nothing more is written, the backup is kept in the work folder") }
  $fedAfter = SettingsRead $fedLogs
  $gch = 0
  foreach ($k in $fedAfter.Files.Keys) { if (-not $fedBefore.Files.ContainsKey($k) -or $fedBefore.Files[$k].Hash -ne $fedAfter.Files[$k].Hash) { $gch++ } }
  Say ("  files in %LOCALAPPDATA%\ParsonsNwcFederator\logs added or changed: " + $gch)
  Say ""
  Say "---- the NWF the copy was made from, at the end ----"
  $nwfHashAfter = (Get-FileHash -LiteralPath $nwfFull -Algorithm SHA256).Hash
  Say ("  sha256 " + $nwfHashAfter + ", " + $(if ($nwfHashAfter -eq $nwfHashBefore) { "the same as before the start" } else { "CHANGED since before the start" }))
  Say "---- the work folder at the end, sorted by name ----"
  foreach ($f in (Get-ChildItem -LiteralPath $work -File | Sort-Object Name)) { Say ("  " + $f.Name + "  " + $f.Length + " bytes") }
  Say "---- outcome ----"
  foreach ($s in $outcome.Keys) { Say ("  step " + $s + " " + $outcome[$s]) }
  Say ("  lines that could not be written to the result file: " + $script:sayFailures)
  Say ("  finished " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + ", " + ([DateTime]::Now - $T0).TotalSeconds.ToString("0") + " s in all")
}
