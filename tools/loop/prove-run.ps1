param([string]$Work = "")
$ErrorActionPreference = "Stop"

# tools\loop\prove-run.ps1, F103 part 1. The proof of tools\loop\run.ps1 and
# tools\loop\nw-guard.ps1 with NO Navisworks started, the harness of the design's section
# proof without navisworks, H0 to H15 for the part 1 modes and M1 to M3, since fix attempt
# 1 H12b, H16 and H17, and since fix attempt 2 H18, each grown by the cases of fix attempt
# 3. Run it as
#
#   powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\prove-run.ps1 -Work <folder>
#
# -Work is a folder under %LOCALAPPDATA%\NwcFederatorLoop, by default its test folder, and
# everything the harness writes goes there, bar the throwaway registry key
# HKCU\Software\NwcFederatorLoopTest. Both are removed at the end.
#
# WHY IT CANNOT START NAVISWORKS. It never calls the Automation constructor. It loads the
# function definitions of run.ps1 through the parser, so run.ps1's main flow never runs in
# it. The only runs of the real run.ps1 are Check, which reads only, Run and Install calls
# made to be refused, each made only after the harness reads, just before it, a stand-in
# Roamer running and an installed stamp that is not the -Stamp passed, so check 6 and check
# 7 would each refuse it even if the check under test did not, and CloseOwn calls, which
# close only a process whose path is the install's own Roamer.exe, which no stand-in has.
# Before and after every case the harness reads that no Roamer it did not start runs. H17
# runs a COPY of run.ps1 under -Work whose constructor line is replaced by a line that
# throws, and checks the copy holds no call of the constructor before it runs it, in Run
# and, from a scratch git repository with a stub build\install.ps1, in Install. H12b runs
# copies of build\install.ps1 with -SkipBuild, and only after a child started the same way
# reads APPDATA as a folder under -Work, so what they install goes there. Since F109 H12c
# runs more of them the same way, each with one file of the fake bundle held open by a child
# powershell, which refuses the move aside, so the bundle is replaced in place. Since F109
# attempt 2 it also reads the bundle folder of the clone this checkout belongs to, which on
# this machine sits under OneDrive, for attributes, link types and sha256 alone, and writes
# nothing there.
#
# THE STAND-IN is tools\loop\StandIn, built here into -Work, a small net48 exe named
# Roamer.exe that is not Navisworks. Every stand-in is started by this harness, held through
# its Process handle, and closed through that handle at the end.
#
# After every case it reads again what is Bader's: his logs folder with sha256 and
# attributes, his AutoSave folder, an export of HKCU\Software\Autodesk\Navisworks Manage\22.0,
# the installed bundle with sha256, and the loop folder outside the turn folders, where
# other work of the loop is written. Each must read exactly as at the start.

$loopRoot = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
if ($Work -eq "") { $Work = Join-Path $loopRoot "test" }
$Work = [System.IO.Path]::GetFullPath($Work).TrimEnd('\')
if (-not $Work.StartsWith($loopRoot + "\", [StringComparison]::OrdinalIgnoreCase)) { [Console]::Out.WriteLine("REFUSED: -Work " + $Work + " is not under " + $loopRoot); exit 2 }
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$runPs = Join-Path $PSScriptRoot "run.ps1"
$guardFile = Join-Path $PSScriptRoot "nw-guard.ps1"
$ps = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
$ps32 = Join-Path $env:SystemRoot "SysWOW64\WindowsPowerShell\v1.0\powershell.exe"
$nw = "C:\Program Files\Autodesk\Navisworks Manage 2025"
$utf8 = New-Object System.Text.UTF8Encoding($false)
$T0 = [DateTime]::Now
$script:Pass = 0
$script:Fail = 0
$script:Fails = New-Object System.Collections.Generic.List[string]
function O($t) { [Console]::Out.WriteLine($t) }
function Check($name, [bool]$ok, $detail) {
  if ($ok) { $script:Pass++; O ("  PASS  " + $name + $(if ($detail) { "  | " + $detail } else { "" })) }
  else { $script:Fail++; $script:Fails.Add($name); O ("  FAIL  " + $name + $(if ($detail) { "  | " + $detail } else { "" })) }
}

O ("prove-run.ps1, F103 part 1, " + $T0.ToString("yyyy-MM-dd HH:mm:ss") + ", pid " + $PID)
O ("  run.ps1 sha256 " + (Get-FileHash -LiteralPath $runPs -Algorithm SHA256).Hash + ", nw-guard.ps1 sha256 " + (Get-FileHash -LiteralPath $guardFile -Algorithm SHA256).Hash + ", this harness sha256 " + (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash)
$roamers = @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)
O ("  Get-Process Roamer at the start: " + $roamers.Count)
if ($roamers.Count -gt 0) { O "REFUSED: a Roamer is running, and the harness never runs beside any Navisworks. Nothing was done."; exit 2 }
if (Test-Path -LiteralPath $Work) { O ("REFUSED: " + $Work + " is there already, and the harness never writes into a folder it did not make. Nothing was done."); exit 2 }
New-Item -ItemType Directory -Path $Work | Out-Null

# The functions under test: the guard file, and run.ps1's top level functions read through
# the parser, never its main flow.
. $guardFile
$guardText = [System.IO.File]::ReadAllText($guardFile)
$tk = $null; $pe = $null
$runAst = [System.Management.Automation.Language.Parser]::ParseFile($runPs, [ref]$tk, [ref]$pe)
if ($pe.Count -gt 0) { O ("run.ps1 does not parse: " + $pe[0].Message); exit 1 }
$runText = ((@($runAst.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] }) | ForEach-Object { $_.Extent.Text }) -join "`r`n")
. ([scriptblock]::Create($runText))
$wt = NewWinTypes
$procType = $wt.ProcType
$winType = $wt.WinType
# NewRoamers, moved from the probe, reads this from its caller.
$beforeTicks = @{}
$script:RecordFile = $null
$script:AlsoFile = $null
$script:RecordLock = New-Object System.Object
$script:SayFailures = 0

# =======================================================================================
# Bader's state, read before every case and after it.
$hisLogs = Join-Path $env:LOCALAPPDATA "ParsonsNwcFederator\logs"
$autoSave = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025\AutoSave"
$bundle = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
function BaderState {
  $s = New-Object System.Collections.Generic.List[string]
  foreach ($f in @(Get-ChildItem -LiteralPath $hisLogs -Recurse -Force | Sort-Object FullName)) {
    $h = ""; if (-not $f.PSIsContainer) { $h = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash }
    $s.Add("logs " + $f.FullName.Substring($hisLogs.Length) + " " + $(if ($f.PSIsContainer) { "folder" } else { [string]$f.Length }) + " " + $f.LastWriteTimeUtc.Ticks + " " + $f.Attributes + " " + $h)
  }
  foreach ($f in @(Get-ChildItem -LiteralPath $autoSave -Recurse -Force -ErrorAction SilentlyContinue | Sort-Object FullName)) { $s.Add("autosave " + $f.FullName.Substring($autoSave.Length) + " " + $(if ($f.PSIsContainer) { "folder" } else { [string]$f.Length }) + " " + $f.LastWriteTimeUtc.Ticks + " " + $f.Attributes) }
  foreach ($f in @(Get-ChildItem -LiteralPath $bundle -Recurse -File -Force | Sort-Object FullName)) { $s.Add("bundle " + $f.FullName.Substring($bundle.Length) + " " + (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash) }
  $reg = Join-Path $Work "bader-22.0.reg"
  $rx = RunChild "reg.exe" ("export `"HKCU\Software\Autodesk\Navisworks Manage\22.0`" `"" + $reg + "`" /y") $null
  $s.Add("reg export exit " + $rx.Exit + " sha256 " + $(if (Test-Path -LiteralPath $reg) { (Get-FileHash -LiteralPath $reg -Algorithm SHA256).Hash } else { "none" }))
  if (Test-Path -LiteralPath $reg) { [System.IO.File]::Delete($reg) }
  foreach ($f in @(Get-ChildItem -LiteralPath $loopRoot -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\NwcFederatorLoop\\(turn\d+|wt-[^\\]+)(\\|$)' -and -not ($_.FullName -eq $Work -or $_.FullName.StartsWith($Work + "\", [StringComparison]::OrdinalIgnoreCase)) } | Sort-Object FullName)) { $s.Add("loop " + $f.FullName.Substring($loopRoot.Length) + " " + $(if ($f.PSIsContainer) { "folder" } else { [string]$f.Length }) + " " + $f.LastWriteTimeUtc.Ticks) }
  return ($s -join "`n")
}
O "  reading Bader's state at the start"
$baderAtStart = BaderState
O ("    " + $baderAtStart.Split("`n").Count + " lines: his logs, his AutoSave, the installed bundle, the 22.0 export and the loop folder outside the turn folders")
# The harness never runs beside a Navisworks it did not start. It refuses at its start, and
# before and after every case it reads again for a Roamer that is not one of its own running
# stand-ins, and stops there if it finds one or cannot read the process list, closing only
# its own stand-ins in its cleanup.
function ForeignRoamer {
  $all = $null
  try { $all = @([System.Diagnostics.Process]::GetProcessesByName("Roamer")) } catch { return ("the process list could not be read, " + (Err $_.Exception)) }
  foreach ($p in $all) {
    $id = $p.Id
    if (@($script:Held | Where-Object { $_.Id -eq $id -and -not $_.HasExited }).Count -eq 0) {
      $st = "its start time could not be read"
      try { $st = "started " + $p.StartTime.ToString("yyyy-MM-dd HH:mm:ss") } catch { $st = "its start time could not be read, " + (Err $_.Exception) }
      return ("Roamer pid " + $id + ", " + $st)
    }
  }
  return $null
}
function Case($title) {
  $foreign = ForeignRoamer
  if ($null -ne $foreign) { throw ("STOPPED before " + $title + ": a Navisworks the harness did not start is running, or the process list cannot be read, " + $foreign + ". The harness never runs beside one, so it stops here, never touches it, and closes only its own stand-ins") }
  O $title
}
function BaderSame($case) {
  $foreign = ForeignRoamer
  if ($null -ne $foreign) { throw ("STOPPED after " + $case + ": a Navisworks the harness did not start is running, " + $foreign + ". The harness never runs beside one, so it stops here, never touches it, and closes only its own stand-ins") }
  $now = BaderState
  if ($now -eq $baderAtStart) { Check ("nothing of Bader's changed, after " + $case) $true "" }
  else {
    $d = @(Compare-Object $baderAtStart.Split("`n") $now.Split("`n"))
    Check ("nothing of Bader's changed, after " + $case) $false ($d.Count.ToString() + " lines differ, first " + $d[0].SideIndicator + " " + $d[0].InputObject)
  }
}

# =======================================================================================
# The stand-ins, every one held through its Process handle.
$standinBin = Join-Path $Work "standin-bin"
$script:Held = New-Object System.Collections.Generic.List[object]
function StartStandin($arguments, $role, $exe) {
  if ($null -eq $exe) { $exe = Join-Path $standinBin "Roamer.exe" }
  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = $exe
  $psi.Arguments = $arguments
  $psi.UseShellExecute = $false
  $psi.CreateNoWindow = $true
  if ($null -ne $role) { $psi.EnvironmentVariables["NWCLOOP_STANDIN"] = $role }
  $p = [System.Diagnostics.Process]::Start($psi)
  [void]$p.Handle
  $script:Held.Add($p)
  Start-Sleep -Milliseconds 400
  return $p
}
function StopStandins {
  foreach ($p in $script:Held) { if (-not $p.HasExited) { $p.Kill(); [void]$p.WaitForExit(10000) } }
}
function Alive($p, $ticks) { return (-not $p.HasExited -and (UtcTicks $p.StartTime) -eq $ticks) }
# A child started with some environment variables pointed at the harness's own folders, read
# to its end by EndChild.
function StartChildEnv($file, $arguments, $workDir, $envs) {
  $psi = New-Object System.Diagnostics.ProcessStartInfo
  $psi.FileName = $file
  $psi.Arguments = $arguments
  $psi.UseShellExecute = $false
  $psi.CreateNoWindow = $true
  $psi.RedirectStandardOutput = $true
  $psi.RedirectStandardError = $true
  $psi.WorkingDirectory = $workDir
  foreach ($k in $envs.Keys) { $psi.EnvironmentVariables[$k] = $envs[$k] }
  $p = [System.Diagnostics.Process]::Start($psi)
  return [pscustomobject]@{ P = $p; O = $p.StandardOutput.ReadToEndAsync(); E = $p.StandardError.ReadToEndAsync() }
}
function EndChild($c, $seconds) {
  if (-not $c.P.WaitForExit($seconds * 1000)) { $c.P.Kill(); [void]$c.P.WaitForExit(10000); return [pscustomobject]@{ Exit = -1; Out = "the child did not end in " + $seconds + " s and was closed through its own handle"; Err = ""; Pid = $c.P.Id } }
  $c.P.WaitForExit()
  return [pscustomobject]@{ Exit = $c.P.ExitCode; Out = $c.O.Result; Err = $c.E.Result; Pid = $c.P.Id }
}
function RunReal($arguments) {
  return (RunChild $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $runPs + "`" " + $arguments) $repo)
}
function Refused($r) { return @($r.Out.Split("`n") | Where-Object { $_.StartsWith("REFUSED:") } | ForEach-Object { $_.Trim() }) }
# The refused calls use set 99. The runs folder itself may hold the lead's real runs, which
# BaderSame reads as part of the loop folder after every case.
function NoWrites($label) {
  $runs = Join-Path $loopRoot "runs\99"
  $ev = Join-Path $repo "steps\runs\99"
  Check ($label + " wrote no run folder and no evidence") (-not (Test-Path -LiteralPath $runs) -and -not (Test-Path -LiteralPath $ev)) ""
}
# Just before a call of the real run.ps1 that must be refused: a stand-in Roamer runs and the
# installed stamp is not 00000000, so checks 6 and 7 would each refuse it.
function GuardsInPlace($p) {
  $pv = InstalledStamp (Join-Path $bundle "Contents\v22\Federator.Addin.dll")
  $ok = ((Alive $p (UtcTicks $p.StartTime)) -and @(Get-Process -Name Roamer -ErrorAction SilentlyContinue).Count -ge 1 -and -not (StampNames $pv "00000000"))
  if (-not $ok) { throw "a guard is not in place, so the real run.ps1 is not called" }
  return ("a stand-in Roamer pid " + $p.Id + " runs, and the installed add-in reads " + $pv + ", not 00000000")
}

try {
  # =====================================================================================
  O ""
  O "==== THE STAND-IN, built from tools\loop\StandIn ===="
  $b = RunChild "dotnet" ("build `"" + (Join-Path $PSScriptRoot "StandIn\StandIn.csproj") + "`" -c Release -o `"" + $standinBin + "`" --nologo -v minimal") $repo
  O ("  dotnet build exit " + $b.Exit + ", pid " + $b.Pid)
  foreach ($l in @($b.Out.Split("`n") | Where-Object { $_ -match 'Warning|Error|->' })) { O ("    " + $l.Trim()) }
  $bs = RunChild "dotnet" "build-server shutdown" $repo
  O ("  dotnet build-server shutdown exit " + $bs.Exit)
  Check "the stand-in builds" ($b.Exit -eq 0 -and (Test-Path -LiteralPath (Join-Path $standinBin "Roamer.exe"))) ""
  Copy-Item -LiteralPath (Join-Path $standinBin "Roamer.exe") -Destination (Join-Path $standinBin "Decoy.exe")

  # =====================================================================================
  O ""
  Case "==== H0, STATIC READS of run.ps1 and nw-guard.ps1 ===="
  $nmFed = "NM" + " " + "Fed"
  $acc = "ACC" + "Docs"
  function StaticFaults($files) {
    $faults = New-Object System.Collections.Generic.List[string]
    $words = @('GetRunningInstance', 'TryGetRunningInstance', 'FindWindow', 'GetTopWindow', 'RootElement', 'SetCursorPos', 'mouse_event', 'PostMessageW', 'prepare-copy', $acc)
    foreach ($f in $files) {
      $text = [System.IO.File]::ReadAllText($f)
      $leaf = Split-Path $f -Leaf
      foreach ($w in $words) { if ($text.IndexOf($w, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $faults.Add($leaf + " holds " + $w) } }
      if ($text -cmatch ('NM.{0,6}Fed\b')) { $faults.Add($leaf + " names " + $nmFed) }
      if ($text -match "GetFolderPath\(\s*['""]Desktop['""]") { $faults.Add($leaf + " reads the desktop") }
      if ($text -match 'Add-Type\s+(-TypeDefinition|-MemberDefinition|-Source|@|")') { $faults.Add($leaf + " compiles source with Add-Type") }
      $t2 = $null; $e2 = $null
      $ast = [System.Management.Automation.Language.Parser]::ParseFile($f, [ref]$t2, [ref]$e2)
      if ($e2.Count -gt 0) { $faults.Add($leaf + " does not parse"); continue }
      foreach ($m in @($ast.FindAll({ param($a) $a -is [System.Management.Automation.Language.InvokeMemberExpressionAst] }, $true))) {
        $name = [string]$m.Member.Value
        $fn = $m.Parent; while ($null -ne $fn -and $fn -isnot [System.Management.Automation.Language.FunctionDefinitionAst]) { $fn = $fn.Parent }
        $in = $(if ($null -ne $fn) { $fn.Name } else { "the main flow" })
        if ($name -eq "Kill" -and $in -ne "CloseAdopted") { $faults.Add($leaf + " calls Kill in " + $in) }
        if ($name -eq "TerminateProcess" -and $in -ne "Watchdog") { $faults.Add($leaf + " calls TerminateProcess in " + $in) }
        if (($name -eq "DeleteValue" -or $name -eq "DeleteSubKeyTree" -or $name -eq "DeleteSubKey") -and $in -ne "PutBackRegistry") { $faults.Add($leaf + " calls " + $name + " in " + $in) }
        if ($name -eq "Delete" -and $m.Expression -is [System.Management.Automation.Language.TypeExpressionAst]) { $faults.Add($leaf + " deletes a file or folder in " + $in) }
        if ($name -eq "Delete" -and $m.Expression -isnot [System.Management.Automation.Language.TypeExpressionAst]) { $faults.Add($leaf + " calls Delete in " + $in) }
      }
      foreach ($c in @($ast.FindAll({ param($a) $a -is [System.Management.Automation.Language.CommandAst] }, $true))) {
        $cn = $c.GetCommandName()
        if ($cn -match '^(Stop-Process|spps|kill)$') { $faults.Add($leaf + " runs " + $cn) }
        if ($cn -match '^(Remove-Item|ri|rm|rmdir|del|erase|rd|Clear-Content)$') { $faults.Add($leaf + " runs " + $cn) }
      }
    }
    return ,$faults
  }
  $sf = StaticFaults @($runPs, $guardFile)
  Check "run.ps1 and nw-guard.ps1 hold none of the words, call Kill only in CloseAdopted, TerminateProcess only in the watchdog's deadline block, delete no file, stop no process by id, and delete a registry value or key only in PutBackRegistry" ($sf.Count -eq 0) (($sf) -join " | ")
  $h0 = Join-Path $Work "h0"
  New-Item -ItemType Directory -Path $h0 | Out-Null
  $bad = [ordered]@{
    "GetRunningInstance" = 'function Bad { [Autodesk.Navisworks.Api.Automation.NavisworksApplication]::GetRunningInstance() }'
    "FindWindow" = 'function Bad { $winType::FindWindow($null, "x") }'
    "a mouse click" = 'function Bad { $winType::mouse_event(2, 0, 0, 0, 0) }'
    "Add-Type with source" = 'Add-Type -TypeDefinition "public class X {}"'
    "the desktop" = "function Bad { [Environment]::GetFolderPath('Desktop') }"
    "the live folder" = ('# ' + $nmFed)
    "Kill outside CloseAdopted" = 'function Bad($p) { $p.Kill() }'
    "TerminateProcess outside the deadline" = 'function Bad { $winType::TerminateProcess($winType::GetCurrentProcess(), 1) }'
    "Stop-Process" = 'function Bad { Stop-Process -Id 4 -Force }'
    "Remove-Item" = 'function Bad { Remove-Item -LiteralPath "x" }'
    "a file delete" = 'function Bad { [System.IO.File]::Delete("x") }'
    "a registry delete outside the put back" = 'function Bad($k) { $k.DeleteValue("x") }'
    "PostMessageW" = 'function Bad { $winType::PostMessageW(0, 16, 0, 0) }'
  }
  $n = 0
  foreach ($k in $bad.Keys) {
    $n++
    $copy = Join-Path $h0 ("run-bad-" + $n + ".ps1")
    [System.IO.File]::WriteAllText($copy, [System.IO.File]::ReadAllText($runPs) + "`r`n" + $bad[$k] + "`r`n", $utf8)
    $f = StaticFaults @($copy)
    Check ("the static read names a copy of run.ps1 with one bad line added, " + $k) ($f.Count -ge 1) (($f) -join " | ")
  }
  $rt = [System.IO.File]::ReadAllText($runPs)
  Check "run.ps1 writes its four modes in one place" (([regex]::Matches($rt, [regex]::Escape('@("Check", "Install", "Run", "CloseOwn")'))).Count -eq 1) ""
  Check "run.ps1 reads the loop's installed.txt in one place" (([regex]::Matches($rt, [regex]::Escape('-Filter "installed.txt"'))).Count -eq 1) ""
  $iM5 = $rt.IndexOf('NewerFiles $roots $sync.CallStartUtc'); $iPut = $rt.IndexOf('$spb = SettingsPutBack')
  Check "run.ps1 reads M5 before the put back, so the put back's own writes are never listed as the start's" ($iM5 -gt 0 -and $iPut -gt 0 -and $iM5 -lt $iPut) ("M5 at " + $iM5 + ", the put back at " + $iPut)
  Check "the monitor reads watch.txt through ReadShared and never through ReadAllLines" ($rt.Contains('ReadShared $sync.WatchFile') -and -not $rt.Contains('ReadAllLines($sync.WatchFile)')) ""

  # =====================================================================================
  O ""
  Case "==== H1, THE MOVE: the probe -ReflectionOnly before and after, which starts nothing ===="
  $h1 = Join-Path $Work "h1"
  New-Item -ItemType Directory -Path (Join-Path $h1 "before\tools\probes"), (Join-Path $h1 "local"), (Join-Path $h1 "appdata") | Out-Null
  $g = RunChild "git" "show 0eb4ede:tools/probes/probe-automation-start.ps1" $repo
  $beforeProbe = Join-Path $h1 "before\tools\probes\probe-automation-start.ps1"
  [System.IO.File]::WriteAllText($beforeProbe, $g.Out, $utf8)
  function Reflect($probe, $out) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $ps
    $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"" + $probe + "`" -ReflectionOnly -Out `"" + $out + "`""
    $psi.UseShellExecute = $false; $psi.CreateNoWindow = $true; $psi.RedirectStandardOutput = $true; $psi.RedirectStandardError = $true
    $psi.EnvironmentVariables["LOCALAPPDATA"] = (Join-Path $h1 "local")
    $psi.EnvironmentVariables["APPDATA"] = (Join-Path $h1 "appdata")
    $psi.EnvironmentVariables["COMPUTERNAME"] = "the-machine-masked"
    $p = [System.Diagnostics.Process]::Start($psi)
    $o = $p.StandardOutput.ReadToEndAsync(); $e = $p.StandardError.ReadToEndAsync()
    $p.WaitForExit()
    $null = $o.Result + $e.Result
    return $p.ExitCode
  }
  $x1 = Reflect $beforeProbe (Join-Path $h1 "reflect-before.txt")
  $x2 = Reflect (Join-Path $repo "tools\probes\probe-automation-start.ps1") (Join-Path $h1 "reflect-after.txt")
  $ra = @([System.IO.File]::ReadAllLines((Join-Path $h1 "reflect-before.txt")))
  $rb = @([System.IO.File]::ReadAllLines((Join-Path $h1 "reflect-after.txt")))
  $diff = @(Compare-Object ($ra | Where-Object { $_ -notmatch '^(MACHINE|HOST|SCRIPT|PROCESS) ' }) ($rb | Where-Object { $_ -notmatch '^(MACHINE|HOST|SCRIPT|PROCESS) ' }))
  Check "the probe -ReflectionOnly before the move and after it read the same, bar the MACHINE time, the pids and the SCRIPT line" ($x1 -eq 0 -and $x2 -eq 0 -and $diff.Count -eq 0 -and $ra.Count -eq $rb.Count) ("exit " + $x1 + " and " + $x2 + ", " + $ra.Count + " and " + $rb.Count + " lines, " + $diff.Count + " other lines differ")
  Check "the throwaway LOCALAPPDATA of the reflection runs holds nothing" (@(Get-ChildItem -LiteralPath (Join-Path $h1 "local") -Recurse -Force).Count -eq 0) ""
  foreach ($f in @($runPs, $guardFile, (Join-Path $repo "tools\probes\probe-automation-start.ps1"))) { $t3 = $null; $e3 = $null; [void][System.Management.Automation.Language.Parser]::ParseFile($f, [ref]$t3, [ref]$e3); Check ((Split-Path $f -Leaf) + " parses with no error") ($e3.Count -eq 0) "" }
  $gAst = [System.Management.Automation.Language.Parser]::ParseFile($guardFile, [ref]$null, [ref]$null)
  Check "nw-guard.ps1 has no main body, every top level statement is a function" (@($gAst.EndBlock.Statements | Where-Object { $_ -isnot [System.Management.Automation.Language.FunctionDefinitionAst] }).Count -eq 0) ""
  # The move read line by line, apart from the generator that made it: every line of
  # nw-guard.ps1 at the move commit is a line of the probe at 0eb4ede, a comment, or one of the
  # wrapper and changed lines printed here. The move proof in steps\notes maps each line.
  $probeLines = @((RunChild "git" "show 0eb4ede:tools/probes/probe-automation-start.ps1" $repo).Out.Replace("`r`n", "`n").Split("`n"))
  $guardAtMove = @((RunChild "git" "show 377cb1a:tools/loop/nw-guard.ps1" $repo).Out.Replace("`r`n", "`n").Split("`n"))
  $probeSet = New-Object 'System.Collections.Generic.HashSet[string]'
  foreach ($x in $probeLines) { [void]$probeSet.Add($x) }
  $notProbe = New-Object System.Collections.Generic.List[string]
  for ($i = 0; $i -lt $guardAtMove.Count; $i++) { $x = $guardAtMove[$i]; if ($x -eq "" -or $probeSet.Contains($x) -or $x.TrimStart().StartsWith("#")) { continue }; $notProbe.Add(("{0,5} {1}" -f ($i + 1), $x)) }
  O ("  lines of nw-guard.ps1 at 377cb1a that are neither a line of the probe at 0eb4ede nor a comment, " + $notProbe.Count + ":")
  foreach ($x in $notProbe) { O ("    " + $x) }
  $shapes = @($notProbe | Where-Object { $_ -notmatch '^\s*\d+ \s*(function \w+|return |\$ur\.|\$bs\.|\$sync\.GuardText = \$guardText|\$suppressed = \$false|\$(ur|bs) = \[pscustomobject\]@\{|& reg\.exe export \("HKCU\\" \+ \$regSub\)|\$ownScript = .*\$scriptPath\)|try \{ \$ulines .*\$ur\.Stop|if \(.*\{ \$(ur|bs)\.|\} catch \{ \$bs\.Why)' })
  Check "every such line is a function line, a return, a result or stop line of a wrapper, the exported key or the caller's script path, 36 lines as the move proof maps them" ($notProbe.Count -eq 36 -and $shapes.Count -eq 0) ($notProbe.Count.ToString() + " lines, " + $shapes.Count + " of another shape: " + (($shapes) -join " | "))
  BaderSame "H0 and H1"

  # =====================================================================================
  O ""
  Case "==== H2, THE REFUSALS ===="
  $h2 = Join-Path $Work "h2"
  New-Item -ItemType Directory -Path $h2 | Out-Null
  $s2 = StartStandin "sleep 300" $null $null
  $guardNote = GuardsInPlace $s2
  O ("  before every call of the real run.ps1 below: " + $guardNote)
  $cases = [ordered]@{
    "1, a 32 bit powershell" = @($ps32, ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $runPs + "`" -Mode Run -Set 99 -Item 0 -Stamp 00000000"))
    "1, the MTA apartment" = @($ps, ("-NoProfile -MTA -ExecutionPolicy Bypass -File `"" + $runPs + "`" -Mode Run -Set 99 -Item 0 -Stamp 00000000"))
    "2, -Mode Bogus" = @($ps, "RUN -Mode Bogus")
    "2, -Set 9" = @($ps, "RUN -Mode Run -Set 9 -Item 0 -Stamp 00000000")
    "2, -Item 7" = @($ps, "RUN -Mode Run -Set 99 -Item 7 -Stamp 00000000")
    "2, -Item 1, part 2" = @($ps, "RUN -Mode Run -Set 99 -Item 1 -Stamp 00000000")
    "2, -Stamp in upper case" = @($ps, "RUN -Mode Run -Set 99 -Item 0 -Stamp 0000000A")
    "2, no -Stamp" = @($ps, "RUN -Mode Run -Set 99 -Item 0")
    "2, -OpenWindow" = @($ps, "RUN -Mode Run -Set 99 -Item 0 -Stamp 00000000 -OpenWindow")
    "2, -Xml" = @($ps, "RUN -Mode Run -Set 99 -Item 0 -Stamp 00000000 -Xml x.xml")
    "2, -RunFolder with Run" = @($ps, ("RUN -Mode Run -Set 99 -Item 0 -Stamp 00000000 -RunFolder `"" + $h2 + "`""))
    "2, CloseOwn outside the loop folder" = @($ps, "RUN -Mode CloseOwn -RunFolder C:\Windows\Temp")
  }
  foreach ($k in $cases.Keys) {
    $c = $cases[$k]
    $args2 = $c[1]
    if ($args2.StartsWith("RUN ")) { $args2 = "-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $runPs + "`" " + $args2.Substring(4) }
    [void](GuardsInPlace $s2)
    $r = RunChild $c[0] $args2 $repo
    $ref = @(Refused $r)
    Check ("refusal " + $k + ": exit 2 and a REFUSED line") ($r.Exit -eq 2 -and $ref.Count -ge 1) ("exit " + $r.Exit + ", " + $(if ($ref.Count -gt 0) { $ref[0] } else { "no REFUSED line, " + $r.Out.Trim() + " " + $r.Err.Trim() }))
  }
  # 1, a powershell that dot-sources run.ps1
  $wrap = Join-Path $h2 "wrap.ps1"
  [System.IO.File]::WriteAllText($wrap, ". `"" + $runPs + "`" -Mode Run -Set 99 -Item 0 -Stamp 00000000`r`nexit `$LASTEXITCODE`r`n", $utf8)
  [void](GuardsInPlace $s2)
  $r = RunChild $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $wrap + "`"") $repo
  $ref = @(Refused $r)
  Check "refusal 1, a powershell that dot-sources run.ps1: exit 2 and a REFUSED line" ($r.Exit -eq 2 -and $ref.Count -ge 1) ("exit " + $r.Exit + ", " + $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() }))
  # 2, a path through a junction made under -Work
  $target = Join-Path $h2 "target"
  New-Item -ItemType Directory -Path (Join-Path $target "runs\99\item0") | Out-Null
  $junction = Join-Path $h2 "junction"
  $jr = RunChild "cmd.exe" ("/c mklink /J `"" + $junction + "`" `"" + $target + "`"") $h2
  O ("  mklink /J exit " + $jr.Exit + ", the junction is a reparse point: " + (((Get-Item -LiteralPath $junction -Force).Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0))
  $pr = PathRefusal (Join-Path $junction "runs\99\item0") $loopRoot
  Check "PathRefusal names a path that passes a junction" ($null -ne $pr -and $pr -match 'junction') ([string]$pr)
  Check "PathRefusal lets a plain path under the loop folder through" ($null -eq (PathRefusal (Join-Path $target "runs\99\item0") $loopRoot)) ""
  Check "PathRefusal names a path outside the loop folder" ($null -ne (PathRefusal "C:\Windows\Temp" $loopRoot)) ([string](PathRefusal "C:\Windows\Temp" $loopRoot))
  [void](GuardsInPlace $s2)
  $r = RunReal ("-Mode CloseOwn -RunFolder `"" + (Join-Path $junction "runs\99\item0") + "`"")
  $ref = @(Refused $r)
  Check "refusal 2, CloseOwn with a run folder through the junction: exit 2" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'junction') $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  [System.IO.Directory]::Delete($junction, $false)
  O ("  the junction removed, there now: " + (Test-Path -LiteralPath $junction) + ", its target still there: " + (Test-Path -LiteralPath $target))
  # 3, the lock held by the harness
  $m = New-Object System.Threading.Mutex($false, "Local\NwcFederatorLoop.run")
  $gotLock = $m.WaitOne(0)
  O ("  the harness holds Local\NwcFederatorLoop.run: " + $gotLock)
  [void](GuardsInPlace $s2)
  $r = RunReal "-Mode Run -Set 99 -Item 0 -Stamp 00000000"
  $ref = @(Refused $r)
  Check "refusal 3, the loop's lock held by another process: exit 2" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'lock') $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  $m.ReleaseMutex(); $m.Dispose()
  # 6, any Navisworks: the plain stand-in started by hand runs, then one with -Embedding
  [void](GuardsInPlace $s2)
  $r = RunReal "-Mode Run -Set 99 -Item 0 -Stamp 00000000"
  $ref = @(Refused $r)
  Check "refusal 6, a stand-in Roamer started by hand running: exit 2" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'Navisworks is running, Roamer pid') $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  $s2e = StartStandin "-Embedding" "sleep|300" $null
  [void](GuardsInPlace $s2e)
  $r = RunReal "-Mode Run -Set 99 -Item 0 -Stamp 00000000"
  $ref = @(Refused $r)
  Check "refusal 6, a stand-in Roamer with -Embedding running as well: exit 2" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'Navisworks is running') $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  NoWrites "the real run.ps1 refused"
  # 7, a wrong -Stamp, through Check, which reads only, and as functions
  $r = RunReal "-Mode Check -Set 99 -Item 0 -Stamp 00000000"
  $w7 = @($r.Out.Split("`n") | Where-Object { $_ -match 'would refuse: the installed add-in reads' })
  Check "refusal 7 read by Check: a wrong -Stamp would be refused, exit 2" ($r.Exit -eq 2 -and $w7.Count -eq 1) $(if ($w7.Count -gt 0) { $w7[0].Trim() } else { $r.Out.Trim() })
  Check "StampNames: be0b9b37 is named by its own version" (StampNames "1.0.0.0 be0b9b37 built 2026-09-27 10:47:27" "be0b9b37") ""
  Check "StampNames: be0b9b37+edits does not name be0b9b37" (-not (StampNames "1.0.0.0 be0b9b37+edits built 2026-09-27 10:47:27" "be0b9b37")) ""
  Check "StampNames: no installed add-in names nothing" (-not (StampNames "UNKNOWN, there is no installed add-in" "be0b9b37")) ""
  StopStandins
  # 4, 5 and 11 as functions, handed paths under -Work
  $fake = RunPaths (Join-Path $h2 "loop") $h2 "99" "0"
  New-Item -ItemType Directory -Path (Join-Path $h2 "loop\runs\98\item0"), (Join-Path $h2 "loop\runs\97\item0"), (Join-Path $h2 "loop\probes") | Out-Null
  [System.IO.File]::WriteAllText((Join-Path $h2 "loop\runs\98\item0\record.txt"), "RUN RECORD`r`nsomething`r`n", $utf8)
  [System.IO.File]::WriteAllText((Join-Path $h2 "loop\runs\97\item0\record.txt"), "RUN RECORD`r`nVERDICT: RAN`r`n", $utf8)
  $d4 = DiedRuns $fake.RunsRoot
  Check "check 4 names the record with no VERDICT line and only it" ($d4.Count -eq 1 -and $d4[0].EndsWith("98\item0")) (($d4) -join ", ")
  $rr = RunRefusals $fake "" $true
  Check "check 4 is the first refusal Run gives for that folder" ($rr.Count -eq 1 -and $rr[0] -match 'holds a record with no VERDICT line') $(if ($rr.Count -gt 0) { $rr[0] } else { "" })
  [System.IO.File]::AppendAllText((Join-Path $h2 "loop\runs\98\item0\record.txt"), "VERDICT: STOPPED, written by the harness`r`n", $utf8)
  $s5 = StartStandin "sleep 300" $null $null
  [System.IO.File]::WriteAllText($fake.Unproved, "# header`r`n" + $s5.Id + "`t" + (UtcTicks $s5.StartTime) + "`t-`t" + [DateTime]::UtcNow.ToString("o") + "`ttest`r`n", $utf8)
  $rr = RunRefusals $fake "" $true
  Check "check 5 refuses on a start named in unproved-starts.txt that still runs" ($rr.Count -eq 1 -and $rr[0] -match 'could not prove is still running, pid ' + $s5.Id) $(if ($rr.Count -gt 0) { $rr[0] } else { "" })
  [System.IO.File]::WriteAllText($fake.Unproved, "# header`r`nnot a line`r`n", $utf8)
  $rr = RunRefusals $fake "" $true
  Check "check 5 refuses on a line it cannot read, and names it" ($rr.Count -eq 1 -and $rr[0] -match 'cannot be read, "not a line"') $(if ($rr.Count -gt 0) { $rr[0] } else { "" })
  [System.IO.File]::Delete($fake.Unproved)
  StopStandins
  New-Item -ItemType Directory -Path $fake.Evidence | Out-Null
  [System.IO.File]::WriteAllText((Join-Path $fake.Evidence "record.txt"), "x", $utf8)
  $rr = RunRefusals $fake "" $true
  Check "check 11 refuses an evidence folder that holds a file" ($rr.Count -eq 1 -and $rr[0] -match 'already holds files') $(if ($rr.Count -gt 0) { $rr[0] } else { "" })
  BaderSame "H2"

  # =====================================================================================
  O ""
  Case "==== H3, ADOPTION, AdoptStart fed stand-ins ===="
  $h3 = Join-Path $Work "h3"
  New-Item -ItemType Directory -Path $h3 | Out-Null
  $unp3 = Join-Path $h3 "unproved.txt"
  function NewTestSync($dir) {
    $wf = Join-Path $dir "watch.txt"
    [System.IO.File]::WriteAllText($wf, "", $utf8)
    $sy = WatchSync @(Get-Process | ForEach-Object { $_.Id }) @{} ([DateTime]::Now) $loopRoot $nw 300 300 "" $unp3 $wf $guardText $winType $procType
    return $sy
  }
  $obj = New-Object System.Object
  function Adopt($label, $err, $app, $starts, [bool]$startBeforeCall) {
    StopStandins
    $sy = NewTestSync $h3
    $pl = New-Object System.Collections.Generic.List[object]
    if ($startBeforeCall) { foreach ($s in $starts) { $pl.Add((StartStandin $s[0] $s[1] $null)) }; Start-Sleep -Milliseconds 300; $sy.CallStartUtc = [DateTime]::UtcNow }
    else { $sy.CallStartUtc = [DateTime]::UtcNow; foreach ($s in $starts) { $pl.Add((StartStandin $s[0] $s[1] $null)) } }
    O ("  " + $label)
    $ad = AdoptStart $err $app $sy (Join-Path $h3 "mypid.txt")
    return [pscustomobject]@{ Ad = $ad; Sync = $sy; Procs = $pl }
  }
  $a = Adopt "case A, one start with -Embedding after the call" $null $obj @(,@("-Embedding", "sleep|120")) $false
  Check "case A is adopted, all four conditions true, and held through its handle" ($a.Ad.Adopted -and $a.Ad.C1 -and $a.Ad.C2 -and $a.Ad.C3 -and $a.Ad.C4 -and $null -ne $a.Sync.MyProc -and $a.Sync.MyProc.Id -eq $a.Procs[0].Id) ""
  $mp = @([System.IO.File]::ReadAllLines((Join-Path $h3 "mypid.txt")))
  Check "case A writes mypid.txt with the pid, the start ticks and the word adopted" ($mp.Count -eq 3 -and [int]$mp[0] -eq $a.Procs[0].Id -and [long]$mp[1] -eq (UtcTicks $a.Procs[0].StartTime) -and $mp[2] -eq "adopted") (($mp) -join " | ")
  $cr = CloseAdopted $a.Sync.MyProc $a.Sync.MyTicks 10
  Check "case A closes through CloseAdopted and is gone" ($cr.State -eq "gone" -and $a.Procs[0].HasExited) $cr.Text
  $a = Adopt "case c1, the constructor threw" ([System.Exception]"a test throw") $null @(,@("-Embedding", "sleep|120")) $false
  Check "c1 false is not adopted, nothing closed" (-not $a.Ad.Adopted -and -not $a.Ad.C1 -and (Alive $a.Procs[0] (UtcTicks $a.Procs[0].StartTime))) ("C1 " + $a.Ad.C1)
  $a = Adopt "case c2, two possible starts" $null $obj @(@("-Embedding", "sleep|120"), @("-Embedding", "sleep|120")) $false
  Check "c2 false is not adopted, and neither start is closed" (-not $a.Ad.Adopted -and -not $a.Ad.C2 -and -not $a.Procs[0].HasExited -and -not $a.Procs[1].HasExited) ("C2 " + $a.Ad.C2)
  O "  the two possible starts are written down at the end, the file first held open with no sharing, then free"
  $late = @(NewRoamers)
  [System.IO.File]::WriteAllText($unp3, "# header`r`n", $utf8)
  $fs = [System.IO.File]::Open($unp3, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
  try { UnprovedAtEnd $unp3 0 0 $late $a.Sync } finally { $fs.Close() }
  $lines3 = @([System.IO.File]::ReadAllLines($unp3) | Where-Object { $_ -notmatch '^#' })
  Check "while the write fails, no start is said written down and none is in the file" ($lines3.Count -eq 0) ""
  UnprovedAtEnd $unp3 0 0 $late $a.Sync
  $lines3 = @([System.IO.File]::ReadAllLines($unp3) | Where-Object { $_ -notmatch '^#' })
  Check "once the write works, both starts are in the file" ($lines3.Count -eq 2 -and ($lines3 -join " ") -match ([string]$a.Procs[0].Id) -and ($lines3 -join " ") -match ([string]$a.Procs[1].Id)) (($lines3 | ForEach-Object { $_.Split("`t")[0] }) -join ", ")
  $a = Adopt "case c3, the start was made before the call" $null $obj @(,@("-Embedding", "sleep|120")) $true
  Check "c3 false is not adopted" (-not $a.Ad.Adopted -and -not $a.Ad.C3) ("C3 " + $a.Ad.C3)
  $a = Adopt "case c4, the word embedding in the command line but not the option -Embedding" $null $obj @(,@("--note=embedding sleep 120", $null)) $false
  Check "c4 false is not adopted" (-not $a.Ad.Adopted -and -not $a.Ad.C4 -and $a.Ad.C2) ("C2 " + $a.Ad.C2 + ", C4 " + $a.Ad.C4)
  $a = Adopt "case hand, a start by hand after the call" $null $obj @(,@("sleep 120", $null)) $false
  Check "a start by hand is never adopted" (-not $a.Ad.Adopted -and -not $a.Ad.C2) ("C2 " + $a.Ad.C2)
  [System.IO.File]::WriteAllText($unp3, "# header`r`n", $utf8)
  UnprovedAtEnd $unp3 0 0 @(NewRoamers) $a.Sync
  Check "a start by hand is never written down" (@([System.IO.File]::ReadAllLines($unp3) | Where-Object { $_ -notmatch '^#' }).Count -eq 0) ""
  StopStandins
  BaderSame "H3"

  # =====================================================================================
  O ""
  Case "==== H4, THE HELD HANDLE ===="
  $h4 = Join-Path $Work "h4"
  New-Item -ItemType Directory -Path $h4 | Out-Null
  $sy = NewTestSync $h4
  $sy.CallStartUtc = [DateTime]::UtcNow
  $own4 = StartStandin "-Embedding" "sleep|120" $null
  $ad = AdoptStart $null $obj $sy (Join-Path $h4 "mypid.txt")
  $t4 = $sy.MyTicks
  Check "the stand-in is adopted and held" ($ad.Adopted -and $null -ne $sy.MyProc) ""
  O "  the stand-in now ends, closed by the harness through its own Process object, a second handle, not the adoption's"
  $own4.Kill()
  $sw = [Diagnostics.Stopwatch]::StartNew(); while (-not $sy.MyProc.HasExited -and $sw.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 200 }
  $decoyLog = Join-Path $h4 "decoy.txt"
  $dec = StartStandin "" ("decoy|" + $decoyLog + "|NwcFederatorLoop decoy|60") $null
  Start-Sleep -Seconds 2
  $dt = UtcTicks $dec.StartTime
  Check "the held stand-in has exited, and the held handle reads it gone" ((HeldState $sy.MyProc $t4) -eq "gone") (HeldState $sy.MyProc $t4)
  $cr = CloseAdopted $sy.MyProc $t4 5
  Check "CloseAdopted refuses, the held process has exited" ($cr.State -eq "gone") $cr.Text
  $killErr = ""
  try { $sy.MyProc.Kill(); $killErr = "Kill did not throw" } catch { $killErr = (Err $_.Exception) }
  Check "Process.Kill on the held object throws because that process exited, so it reuses the held handle and never opens the pid again" ($killErr -match 'exited') $killErr
  Check "the decoy started by hand still runs with its own start ticks, and is not the held pid" ((Alive $dec $dt) -and $dec.Id -ne $sy.MyProc.Id) ("decoy pid " + $dec.Id + ", held pid " + $sy.MyProc.Id)
  $dl = @(Get-Content -LiteralPath $decoyLog -ErrorAction SilentlyContinue)
  Check "the decoy's message log is empty" ($dl.Count -eq 0) (($dl) -join " | ")
  StopStandins
  BaderSame "H4"

  # =====================================================================================
  O ""
  Case "==== H5, M1, the real RunLog.Start prune against 30 fabricated logs held open with no delete sharing ===="
  $h5 = Join-Path $Work "h5"
  function MakeLogs($dir) {
    New-Item -ItemType Directory -Path $dir | Out-Null
    $start = [DateTime]::Now.AddDays(-2)
    for ($i = 0; $i -lt 30; $i++) {
      $t = $start.AddMinutes($i)
      $f = Join-Path $dir ("run-" + $t.ToString("yyyyMMdd-HHmmss") + ".log")
      [System.IO.File]::WriteAllText($f, "a fabricated log, number " + $i + "`r`n", $utf8)
      [System.IO.File]::SetLastWriteTime($f, $t)
    }
    [System.IO.File]::WriteAllText((Join-Path $dir "folders.txt"), "fabricated`r`n", $utf8)
    return @(Get-ChildItem -LiteralPath $dir -Filter "run-*.log" | Sort-Object LastWriteTime | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash } })
  }
  function Prune($dir) {
    $res = Join-Path $h5 ("result-" + (Split-Path $dir -Leaf) + ".txt")
    $p = StartStandin "" ("prune|" + $dir + "|" + $res) $null
    [void]$p.WaitForExit(60000)
    $logPath = ([System.IO.File]::ReadAllText($res)).Trim()
    return @([System.IO.File]::ReadAllLines($logPath) | Where-Object { $_ -match 'RETAIN' })
  }
  $locked = Join-Path $h5 "locked"
  $orig5 = MakeLogs $locked
  $handles = New-Object System.Collections.Generic.List[object]
  foreach ($f in @(Get-ChildItem -LiteralPath $locked -Filter "run-*.log")) { $handles.Add([System.IO.File]::Open($f.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)) }
  O ("  " + $handles.Count + " logs held open for reading, sharing read and write, not delete")
  $ret = Prune $locked
  foreach ($h in $handles) { $h.Dispose() }
  foreach ($l in $ret) { O ("    | " + $l) }
  Check "M1: with the lock held, RETAIN reads keeping 30 logs, deleted 0, could not delete 1" (@($ret | Where-Object { $_ -match 'RETAIN\s+keeping 30 logs, deleted 0, could not delete 1' }).Count -eq 1) ""
  Check "M1: the one it could not delete is the oldest" (@($ret | Where-Object { $_ -match ('could not delete ' + [regex]::Escape($orig5[0].Name)) }).Count -eq 1) $orig5[0].Name
  $readBack = @($orig5 | Where-Object { $p5 = Join-Path $locked $_.Name; (Test-Path -LiteralPath $p5) -and (Get-FileHash -LiteralPath $p5 -Algorithm SHA256).Hash -eq $_.Hash })
  Check "M1: all 30 fabricated logs read back with their sha256" ($readBack.Count -eq 30) ([string]$readBack.Count)
  $control = Join-Path $h5 "control"
  $orig5c = MakeLogs $control
  $retc = Prune $control
  foreach ($l in $retc) { O ("    | " + $l) }
  Check "M1 control: with no lock, RETAIN reads deleted 1, and the oldest is gone" ((@($retc | Where-Object { $_ -match 'deleted 1, could not delete 0' }).Count -eq 1) -and -not (Test-Path -LiteralPath (Join-Path $control $orig5c[0].Name))) ""
  StopStandins
  BaderSame "H5"

  # =====================================================================================
  O ""
  Case "==== H6, THE HANG RULE ===="
  $tNow = [DateTime]::UtcNow
  $table = @(
    @("both flat 25 s, limit 20", $tNow.AddSeconds(-25), $tNow.AddSeconds(-25), $true),
    @("both flat exactly 20 s", $tNow.AddSeconds(-20), $tNow.AddSeconds(-20), $true),
    @("log flat 25 s, processor moved 10 s ago", $tNow.AddSeconds(-25), $tNow.AddSeconds(-10), $false),
    @("log grew 10 s ago, processor flat 25 s", $tNow.AddSeconds(-10), $tNow.AddSeconds(-25), $false),
    @("no clock yet", $null, $tNow.AddSeconds(-25), $false))
  foreach ($row in $table) { $v = HangVerdict $row[1] $row[2] $tNow 20; Check ("HangVerdict, " + $row[0]) ($v -eq $row[3]) ("returned " + $v) }
  $c = NewClocks $tNow
  $c = NextClocks $c 100 5 $tNow.AddSeconds(2)
  $c = NextClocks $c 100 5 $tNow.AddSeconds(12)
  $c = NextClocks $c $null 5 $tNow.AddSeconds(14)
  Check "an unreadable sample restarts both clocks" ($c.Unknown -and $c.LChange -eq $tNow.AddSeconds(14) -and $c.CChange -eq $tNow.AddSeconds(14)) ""
  $c = NextClocks $c 100 5 $tNow.AddSeconds(30)
  Check "after it, a sample equal to the last known one keeps the restarted clocks" (-not (HangVerdict $c.LChange $c.CChange $tNow.AddSeconds(30) 20) -and (HangVerdict $c.LChange $c.CChange $tNow.AddSeconds(34) 20)) ""
  $h6 = Join-Path $Work "h6"
  New-Item -ItemType Directory -Path $h6 | Out-Null
  function LiveMonitor($label, $dir, $args6, $role6, $holdSeconds, $feedLog, $fakeLog) {
    StopStandins
    New-Item -ItemType Directory -Path $dir, (Join-Path $dir "logs") | Out-Null
    $sy = NewTestSync $dir
    $sy.CallStartUtc = [DateTime]::UtcNow
    $p = StartStandin $args6 $role6 $null
    $third = StartStandin "sleep 180" $null $null
    $thirdTicks = UtcTicks $third.StartTime
    $ad = AdoptStart $null $obj $sy (Join-Path $dir "mypid.txt")
    $logFile = $null
    if ($fakeLog) {
      $logFile = Join-Path $dir ("logs\run-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")
      [System.IO.File]::WriteAllText($logFile, [DateTime]::Now.ToString("HH:mm:ss") + "  +0000.000s  Log opened at " + $logFile + "`r`n", $utf8)
    }
    $sy.RecordLock = New-Object System.Object
    $sy.RecordFile = Join-Path $dir "record.txt"
    $sy.RunDir = $dir
    $sy.RunText = $runText
    $sy.MonitorStop = $false; $sy.RunOver = ""; $sy.MonitorFault = ""; $sy.MonitorWriteFails = 0
    $sy.Dialogs = 0; $sy.ToolLog = $null; $sy.LogAmbiguous = $false
    $sy.LogsFolder = Join-Path $dir "logs"; $sy.LogsBefore = @{}
    $sy.HangLimit = 20; $sy.PassSeconds = 2; $sy.HoldSeconds = $holdSeconds; $sy.BeatSeconds = 10; $sy.ReadToolLog = $true
    O ("  " + $label + ", the adopted stand-in pid " + $p.Id + ", a third stand-in pid " + $third.Id + " the monitor is not given")
    $mps = [PowerShell]::Create()
    [void]$mps.AddScript((MonitorScript)).AddArgument($sy)
    $mh = $mps.BeginInvoke()
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sy.RunOver -eq "" -and $sw.Elapsed.TotalSeconds -lt 150) {
      if ($feedLog -and $null -ne $logFile) { [System.IO.File]::AppendAllText($logFile, [DateTime]::Now.ToString("HH:mm:ss") + "  +0000.000s  fed by the harness`r`n", $utf8) }
      Start-Sleep -Seconds 2
    }
    $sy.MonitorStop = $true
    [void]$mps.EndInvoke($mh)
    $errs = @($mps.Streams.Error)
    $mps.Dispose()
    O ("    the run ended " + $sy.RunOver + " after " + $sw.Elapsed.TotalSeconds.ToString("0") + " s, monitor runspace errors " + $errs.Count)
    foreach ($e in $errs) { O ("      " + $e.ToString()) }
    O "    the monitor's own lines above are its record.txt as it wrote it"
    return [pscustomobject]@{ Sync = $sy; Proc = $p; Third = $third; ThirdTicks = $thirdTicks; Adopted = $ad.Adopted; Errors = $errs.Count }
  }
  $r6 = LiveMonitor "live 1, a stand-in that writes its RunLog every 2 s for 30 s and then sleeps" (Join-Path $h6 "hang") "-Embedding" ("runlog|" + (Join-Path $h6 "hang\logs") + "|2000|30|120") 0 $false $false
  Check "live 1: the monitor calls a hang, writes hang-tail.txt and closes the stand-in through its held handle" ($r6.Adopted -and $r6.Sync.RunOver -eq "HUNG" -and (Test-Path -LiteralPath (Join-Path $h6 "hang\hang-tail.txt")) -and $r6.Proc.HasExited) ($r6.Sync.RunOver + ", the stand-in has exited " + $r6.Proc.HasExited)
  $moves = @(Get-Content -LiteralPath $r6.Sync.RecordFile | Where-Object { $_ -match 'SAMPLE processor' } | ForEach-Object { [regex]::Match($_, ', ([0-9.]+) s since the last pass').Groups[1].Value } | Where-Object { $_ -ne "" })
  O ("    M-sleep: the processor growth between passes of the stand-in, in s, over the run: " + ($moves -join ", "))
  Check "live 1: the third stand-in is untouched, its start ticks the same" (Alive $r6.Third $r6.ThirdTicks) ""
  Check "live 1: the monitor's runspace holds no error" ($r6.Errors -eq 0) ""
  $r6 = LiveMonitor "live 2, the log flat while the processor spins" (Join-Path $h6 "spin") "-Embedding" "spin|150" 40 $false $true
  Check "live 2: log flat with the processor spinning never hangs, the fixed hold of 40 s ends it" ($r6.Adopted -and $r6.Sync.RunOver -eq "HOLD") $r6.Sync.RunOver
  $r6 = LiveMonitor "live 3, the log growing while the processor is flat" (Join-Path $h6 "grow") "-Embedding" "sleep|150" 40 $true $true
  Check "live 3: log growing with the processor flat never hangs, the fixed hold of 40 s ends it" ($r6.Adopted -and $r6.Sync.RunOver -eq "HOLD") $r6.Sync.RunOver
  $beats = @(Get-Content -LiteralPath $r6.Sync.RecordFile | Where-Object { $_ -match 'HEARTBEAT the log \d+ bytes' })
  Check "live 3: the heartbeat, every 10 s here and every 60 s in run.ps1, names the log's size, its last line, the processor and both clocks" ($beats.Count -ge 2 -and $beats[0] -match 'the log still for \d+ s and the processor for \d+ s, against 20') $(if ($beats.Count -gt 0) { $beats[0] } else { "no HEARTBEAT line" })

  O "  live 4, THE CEILING: the watchdog running with its adopted deadline, run.ps1's ceiling, set to 5 s, and the monitor beside it"
  StopStandins
  $dirC = Join-Path $h6 "ceiling"
  New-Item -ItemType Directory -Path $dirC, (Join-Path $dirC "logs") | Out-Null
  $syC = NewTestSync $dirC
  $syC.AdoptedDeadline = 5
  $syC.CtorReturned = $true
  $syC.CallStartUtc = [DateTime]::UtcNow
  $pC = StartStandin "-Embedding" "sleep|120" $null
  $wpsC = [PowerShell]::Create(); [void]$wpsC.AddScript((WatchdogScript)).AddArgument($syC); $whC = $wpsC.BeginInvoke()
  $adC = AdoptStart $null $obj $syC (Join-Path $dirC "mypid.txt")
  $syC.RecordLock = New-Object System.Object; $syC.RecordFile = Join-Path $dirC "record.txt"; $syC.RunDir = $dirC; $syC.RunText = $runText
  $syC.MonitorStop = $false; $syC.RunOver = ""; $syC.MonitorFault = ""; $syC.MonitorWriteFails = 0; $syC.Dialogs = 0; $syC.ToolLog = $null; $syC.LogAmbiguous = $false
  $syC.LogsFolder = Join-Path $dirC "logs"; $syC.LogsBefore = @{}; $syC.HangLimit = 20; $syC.PassSeconds = 2; $syC.HoldSeconds = 60; $syC.BeatSeconds = 10; $syC.ReadToolLog = $false
  $mpsC = [PowerShell]::Create(); [void]$mpsC.AddScript((MonitorScript)).AddArgument($syC); $mhC = $mpsC.BeginInvoke()
  $sw = [Diagnostics.Stopwatch]::StartNew(); while ($syC.RunOver -eq "" -and $sw.Elapsed.TotalSeconds -lt 60) { Start-Sleep -Milliseconds 250 }
  $syC.MonitorStop = $true; [void]$mpsC.EndInvoke($mhC); $mpsC.Dispose()
  $syC.Stop = $true; [void]$wpsC.EndInvoke($whC); $wpsC.Dispose()
  foreach ($l in @(Get-Content -LiteralPath (Join-Path $dirC "watch.txt") | Where-Object { $_ -match 'deadline' })) { O ("    | watch.txt: " + $l) }
  $vC = RunVerdict ([pscustomobject]@{ StopText = ""; Called = $true; Adopted = $adC.Adopted; NotPutBack = $false; RunOver = [string]$syC.RunOver; Forced = [string]$syC.Forced; CallForced = [string]$syC.CallForced; Fault = ""; MonitorFault = [string]$syC.MonitorFault; FinallyFaults = @(); Dialogs = 0; ClosedHere = ""; HoldSeconds = 60; EndState = $(if ($pC.HasExited) { "gone" } else { "same" }) })
  Check "live 4: at the 5 s ceiling the watchdog closes the stand-in through its held handle, and the monitor ends the run as CEILING, not as ended by itself" ($adC.Adopted -and $syC.RunOver -eq "CEILING" -and $pC.HasExited -and $syC.Forced -match 'closed through the held handle and gone') ($syC.RunOver + ", " + $syC.Forced)
  Check "live 4: the verdict of that run is STOPPED, CEILING, exit 4" ($vC.Code -eq 4 -and $vC.Text.StartsWith("STOPPED, CEILING")) ($vC.Code.ToString() + ", " + $vC.Text)
  function V($over, $forced, $adopted, $notPut, $dialogs) { return (RunVerdict ([pscustomobject]@{ StopText = ""; Called = $true; Adopted = $adopted; NotPutBack = $notPut; RunOver = $over; Forced = $forced; CallForced = ""; Fault = ""; MonitorFault = ""; FinallyFaults = @(); Dialogs = $dialogs; ClosedHere = ""; HoldSeconds = 360; EndState = "gone" })) }
  $vt = @(
    @("the process read gone after the watchdog forced the ceiling, the fault the breaker found", (V "GONE" "the adopted deadline passed" $true $false 0), 4, "STOPPED, CEILING"),
    @("the process gone with nothing forced", (V "GONE" "" $true $false 0), 7, "STOPPED, the adopted Navisworks ended by itself"),
    @("a hang", (V "HUNG" "" $true $false 0), 4, "STOPPED, HUNG"),
    @("the hold, all put back", (V "HOLD" "" $true $false 0), 0, "RAN, item 0"),
    @("the hold with a dialog", (V "HOLD" "" $true $false 1), 5, "RAN, with 1 DIALOG"),
    @("the hold, something not put back", (V "HOLD" "" $true $true 0), 6, "STOPPED, something of Bader's"),
    @("not adopted", (V "" "" $false $false 0), 3, "NOT ADOPTED"))
  foreach ($row in $vt) { Check ("RunVerdict, " + $row[0] + ": exit " + $row[2]) ($row[1].Code -eq $row[2] -and $row[1].Text.StartsWith($row[3])) ($row[1].Code.ToString() + ", " + $row[1].Text) }

  O "  THE WATCH FILE: a writer appends as the watchdog's W does, while a reader reads it for 3 s, first with File.ReadAllLines, then with ReadShared"
  $wfile = Join-Path $h6 "watch-share.txt"
  function ShareTrial([bool]$useShared) {
    [System.IO.File]::WriteAllText($wfile, "", $utf8)
    $st = [hashtable]::Synchronized(@{ Stop = $false; Fails = 0; Writes = 0; File = $wfile })
    $w = [PowerShell]::Create()
    [void]$w.AddScript('param($s) $u = New-Object System.Text.UTF8Encoding($false); while (-not $s.Stop) { try { [System.IO.File]::AppendAllText($s.File, "a watchdog line`r`n", $u); $s.Writes = $s.Writes + 1 } catch { $s.Fails = $s.Fails + 1 } }').AddArgument($st)
    $wh = $w.BeginInvoke()
    $reads = 0; $readFails = 0
    $sw2 = [Diagnostics.Stopwatch]::StartNew()
    while ($sw2.Elapsed.TotalSeconds -lt 3) { try { if ($useShared) { [void](ReadShared $wfile) } else { [void][System.IO.File]::ReadAllLines($wfile) }; $reads++ } catch { $readFails++ } }
    $st.Stop = $true; [void]$w.EndInvoke($wh); $w.Dispose()
    return [pscustomobject]@{ Writes = $st.Writes; Fails = $st.Fails; Reads = $reads; ReadFails = $readFails }
  }
  $before6 = ShareTrial $false
  $after6 = ShareTrial $true
  O ("    File.ReadAllLines: " + $before6.Writes + " appends, " + $before6.Fails + " appends that failed, " + $before6.Reads + " reads, " + $before6.ReadFails + " reads that failed")
  O ("    ReadShared: " + $after6.Writes + " appends, " + $after6.Fails + " appends that failed, " + $after6.Reads + " reads, " + $after6.ReadFails + " reads that failed")
  Check "the watch file, before: a read through File.ReadAllLines makes the watchdog's append fail, which the put back counts as a line it could not write" ($before6.Fails -gt 0) ([string]$before6.Fails + " appends failed")
  Check "the watch file, after: a read through ReadShared makes no append fail and fails itself never" ($after6.Fails -eq 0 -and $after6.ReadFails -eq 0 -and $after6.Reads -gt 0 -and $after6.Writes -gt 0) ([string]$after6.Fails + " appends and " + $after6.ReadFails + " reads failed")

  O "  THE HELD READ keeps its reason"
  $pz = StartStandin "sleep 30" $null $null
  $tz = UtcTicks $pz.StartTime
  $pzCopy = [System.Diagnostics.Process]::GetProcessById($pz.Id)
  [void]$pzCopy.Handle
  $pzCopy.Dispose()
  $hr = HeldRead $pzCopy $tz
  Check "HeldRead of a Process object that cannot be read returns unreadable with the reason, which run.ps1's finally prints" ($hr.State -eq "unreadable" -and $hr.Why -match 'threw' -and (HeldState $pzCopy $tz) -eq "unreadable" -and $rt.Contains('so it is not closed here and is written down below')) ($hr.State + ", " + $hr.Why)
  StopStandins
  BaderSame "H6"

  # =====================================================================================
  O ""
  Case "==== H7, DIALOGS ===="
  $h7 = Join-Path $Work "h7"
  $decoy7 = Join-Path $Work "h7-decoy.txt"
  $msg = "NwcFederatorLoop test message text 7431"
  $lab = "NwcFederatorLoop test label text 2958"
  StopStandins
  New-Item -ItemType Directory -Path $h7, (Join-Path $h7 "logs") | Out-Null
  $sy = NewTestSync $h7
  $sy.CallStartUtc = [DateTime]::UtcNow
  [void](StartStandin "-Embedding" ("dialogs|" + $msg + "|" + $lab + "|60") $null)
  $d7 = StartStandin "" ("decoy|" + $decoy7 + "|NwcFederatorLoop test form|60") $null
  Start-Sleep -Seconds 2
  $ad = AdoptStart $null $obj $sy (Join-Path $h7 "mypid.txt")
  $sy.RecordLock = New-Object System.Object; $sy.RecordFile = Join-Path $h7 "record.txt"; $sy.RunDir = $h7; $sy.RunText = $runText
  $sy.MonitorStop = $false; $sy.RunOver = ""; $sy.MonitorFault = ""; $sy.MonitorWriteFails = 0; $sy.Dialogs = 0; $sy.ToolLog = $null; $sy.LogAmbiguous = $false
  $sy.LogsFolder = Join-Path $h7 "logs"; $sy.LogsBefore = @{}; $sy.HangLimit = 20; $sy.PassSeconds = 2; $sy.HoldSeconds = 10; $sy.BeatSeconds = 10; $sy.ReadToolLog = $false
  $fakeLog7 = Join-Path $h7 ("logs\run-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss") + ".log")
  [System.IO.File]::WriteAllText($fakeLog7, [DateTime]::Now.ToString("HH:mm:ss") + "  +0000.000s  Log opened at " + $fakeLog7 + "`r`n", $utf8)
  $mps = [PowerShell]::Create(); [void]$mps.AddScript((MonitorScript)).AddArgument($sy); $mh = $mps.BeginInvoke()
  $sw = [Diagnostics.Stopwatch]::StartNew(); while ($sy.RunOver -eq "" -and $sw.Elapsed.TotalSeconds -lt 60) { Start-Sleep -Milliseconds 500 }
  $sy.MonitorStop = $true; [void]$mps.EndInvoke($mh); $mps.Dispose()
  $rec7 = @(Get-Content -LiteralPath $sy.RecordFile)
  foreach ($l in @($rec7 | Where-Object { $_ -match 'DIALOG|MAIN|WINDOW|PROGRESS' })) { O ("    | " + $l) }
  Check "the message box is a DIALOG with its exact text" (@($rec7 | Where-Object { $_ -match 'DIALOG: class #32770' -and $_.Contains($msg) }).Count -eq 1) ""
  Check "the WinForms dialog is a DIALOG with its label's exact text" (@($rec7 | Where-Object { $_ -match 'DIALOG: class WindowsForms10' -and $_.Contains($lab) }).Count -eq 1) ""
  Check "item 0 reads no tool's log: with a new log that names itself in the logs folder, the monitor says it reads none and starts no hang clock" ((@($rec7 | Where-Object { $_ -match "no tool's log is read and no hang clock starts" }).Count -eq 1) -and (@($rec7 | Where-Object { $_ -match "the tool's log is found" }).Count -eq 0)) ""
  Check "the record holds no line about the decoy started by hand" (@($rec7 | Where-Object { $_ -match ('pid ' + $d7.Id + '\b') -or $_.Contains("the decoy's label") }).Count -eq 0) ("decoy pid " + $d7.Id)
  $dl7 = @(Get-Content -LiteralPath $decoy7 -ErrorAction SilentlyContinue)
  Check "the decoy's log shows no message sent to it from another thread or process" ($dl7.Count -eq 0) (($dl7) -join " | ")
  StopStandins

  O "  THE MAIN WINDOW'S OWNER, fix list 2 item 21 and fix list 3 item 4: the monitor's own lines on an adopted stand-in with three WinForms windows titled Untitled - Autodesk Navisworks Manage 2025, one with no owner, one owned by a window that is not visible, the shape the second real start measured for the real main window's owner, record steps\runs\01\item0 line 33, visible False, enabled True, in the adopted process, and one owned by the visible first window with a label, the shape a message box of Navisworks owned by its main window would have, which no run has shown"
  $mainCap = "Untitled - Autodesk Navisworks Manage 2025"
  $ownLab = "NwcFederatorLoop owned message 5521"
  $rt7 = [System.IO.File]::ReadAllText($runPs)
  $h7o = Join-Path $Work "h7-owned"
  New-Item -ItemType Directory -Path $h7o, (Join-Path $h7o "logs") | Out-Null
  $syO = NewTestSync $h7o
  $syO.CallStartUtc = [DateTime]::UtcNow
  [void](StartStandin "-Embedding" ("owned|" + $mainCap + "|" + $ownLab + "|40") $null)
  Start-Sleep -Seconds 2
  $adO = AdoptStart $null $obj $syO (Join-Path $h7o "mypid.txt")
  $syO.RecordLock = New-Object System.Object; $syO.RecordFile = Join-Path $h7o "record.txt"; $syO.RunDir = $h7o; $syO.RunText = $runText
  $syO.MonitorStop = $false; $syO.RunOver = ""; $syO.MonitorFault = ""; $syO.MonitorWriteFails = 0; $syO.Dialogs = 0; $syO.ToolLog = $null; $syO.LogAmbiguous = $false
  $syO.LogsFolder = Join-Path $h7o "logs"; $syO.LogsBefore = @{}; $syO.HangLimit = 20; $syO.PassSeconds = 2; $syO.HoldSeconds = 8; $syO.BeatSeconds = 10; $syO.ReadToolLog = $false
  $mpsO = [PowerShell]::Create(); [void]$mpsO.AddScript((MonitorScript)).AddArgument($syO); $mhO = $mpsO.BeginInvoke()
  $sw = [Diagnostics.Stopwatch]::StartNew(); while ($syO.RunOver -eq "" -and $sw.Elapsed.TotalSeconds -lt 60) { Start-Sleep -Milliseconds 500 }
  $syO.MonitorStop = $true; [void]$mpsO.EndInvoke($mhO); $mpsO.Dispose()
  $recO = @(Get-Content -LiteralPath $syO.RecordFile | Where-Object { $_.Contains("caption `"" + $mainCap + "`"") })
  foreach ($l in $recO) { O ("    | " + $l) }
  $capRx = [regex]::Escape($mainCap)
  $mainFree = @($recO | Where-Object { $_ -match ('MAIN: class WindowsForms10\S*, caption "' + $capRx + '", owner none$') }).Count
  $mainHidden = @($recO | Where-Object { $_ -match ('MAIN: class WindowsForms10\S*, caption "' + $capRx + '", owner \d+, class WindowsForms10\S*, caption "NwcFederatorLoop parked owner", visible False, enabled True, process the adopted one$') }).Count
  $dialogO = @($recO | Where-Object { $_ -match ('DIALOG: class WindowsForms10\S*, caption "' + $capRx + '", owner \d+, class WindowsForms10\S*, caption "' + $capRx + '", visible True, enabled \w+, process the adopted one, text .*' + [regex]::Escape($ownLab)) }).Count
  Check "item 21, the monitor's own lines: the window with no owner is MAIN, owner none, and the one owned by a window that is not visible is MAIN, written with its owner's class, caption, visibility, state and process" ($adO.Adopted -and $mainFree -eq 1 -and $mainHidden -eq 1) ("adopted " + $adO.Adopted + ", MAIN with no owner " + $mainFree + ", MAIN with its hidden owner written " + $mainHidden)
  Check "item 21, the monitor's own lines: the window owned by the visible first window is a DIALOG, written with its owner and its label's text, and counted" ($dialogO -eq 1 -and $syO.Dialogs -eq 1) ("DIALOG lines " + $dialogO + ", counted " + $syO.Dialogs)
  StopStandins
  O "  AN OWNER OF ANOTHER PROCESS, fix list 3 item 16: the owner text the monitor writes, for an owner in the adopted process, one in another process, and none"
  $ownSame = OwnerText ([pscustomobject]@{ OwnerHandle = [IntPtr]4242; Owner = "4242"; OwnerClass = "WindowsForms10.Window.0.app.0.x"; OwnerCaption = ""; OwnerVisible = $false; OwnerEnabled = "True"; OwnerPid = [uint32]777 }) ([uint32]777)
  $ownOther = OwnerText ([pscustomobject]@{ OwnerHandle = [IntPtr]4242; Owner = "4242"; OwnerClass = "Chrome_WidgetWin_1"; OwnerCaption = "Inbox of someone"; OwnerVisible = $true; OwnerEnabled = "True"; OwnerPid = [uint32]888 }) ([uint32]777)
  $ownNone = OwnerText ([pscustomobject]@{ OwnerHandle = [IntPtr]::Zero; Owner = "0"; OwnerClass = ""; OwnerCaption = ""; OwnerVisible = $false; OwnerEnabled = "none"; OwnerPid = [uint32]0 }) ([uint32]777)
  foreach ($l in @($ownSame, $ownOther, $ownNone)) { O ("    | " + $l) }
  Check "item 16: an owner in the adopted process is written whole, one in another process is masked with its class, caption and process left out, and no owner reads owner none" ($ownSame -eq 'owner 4242, class WindowsForms10.Window.0.app.0.x, caption "", visible False, enabled True, process the adopted one' -and $ownOther -eq "owner 4242, a window of another process, its class, caption and process not written" -and $ownNone -eq "owner none") ""
  Check "item 4: the monitor writes each window's owner through OwnerText, and the owner's fields are joined in that one place" (([regex]::Matches($rt7, [regex]::Escape('$ownerText = OwnerText $r ([uint32]$sync.MyPid)'))).Count -eq 1 -and ([regex]::Matches($rt7, [regex]::Escape('", class " + $r.OwnerClass'))).Count -eq 1) ""
  $wkt = @(
    @("WindowsForms10.Window.8.app.0.x", $mainCap, [IntPtr]::Zero, $false, "MAIN"),
    @("WindowsForms10.Window.8.app.0.x", $mainCap, [IntPtr]855918, $false, "MAIN"),
    @("WindowsForms10.Window.8.app.0.x", $mainCap, [IntPtr]123, $true, "DIALOG"),
    @("WindowsForms10.Window.8.app.0.x", "", [IntPtr]::Zero, $false, "DIALOG"),
    @("#32770", "Autodesk Navisworks Manage 2025", [IntPtr]::Zero, $false, "DIALOG"),
    @("HwndWrapper[Roamer.exe;ProgressDialog;1]", "Working...", [IntPtr]::Zero, $false, "PROGRESS"),
    @("HwndWrapper[Roamer.exe;;2]", "Parsons NWC Federator 1.0", [IntPtr]::Zero, $false, "WINDOW"))
  foreach ($row in $wkt) { $k = WindowKind $row[0] $row[1] $row[2] $row[3]; Check ("WindowKind of class " + $row[0] + ", caption `"" + $row[1] + "`", owner " + $row[2] + ", owner visible " + $row[3] + ": " + $row[4]) ($k -eq $row[4]) $k }
  StopStandins
  BaderSame "H7"

  # =====================================================================================
  O ""
  Case "==== H16, A WINDOW WHOSE THREAD IS BLOCKED: GetWindowText, and the 2 s the child reads may spend ===="
  $h16 = Join-Path $Work "h16"
  New-Item -ItemType Directory -Path $h16 | Out-Null
  $ready16 = Join-Path $h16 "ready.txt"
  $pb16 = StartStandin "" ("hang|" + $ready16 + "|20|NwcFederatorLoop blocked window|10") $null
  $sw = [Diagnostics.Stopwatch]::StartNew(); while (-not (Test-Path -LiteralPath $ready16) -and $sw.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 50 }
  $blocked = [Diagnostics.Stopwatch]::StartNew()
  $m16 = New-Object System.Collections.Generic.List[object]
  foreach ($round in @("just blocked", "blocked 7 s, when Windows counts it hung")) {
    if ($round.StartsWith("blocked 7")) { while ($blocked.Elapsed.TotalSeconds -lt 7) { Start-Sleep -Milliseconds 100 } }
    $a16 = [Diagnostics.Stopwatch]::StartNew(); $r1 = WindowRecords $winType $procType ([uint32]$pb16.Id) $true $false; $a16.Stop()
    $b16 = [Diagnostics.Stopwatch]::StartNew(); $r2 = WindowRecords $winType $procType ([uint32]$pb16.Id) $true $true; $b16.Stop()
    $cap = ""; if ($r1.Count -gt 0) { $cap = $r1[0].Caption }
    $txt = ""; if ($r2.Count -gt 0 -and $null -ne $r2[0].Texts) { $txt = ($r2[0].Texts -join " ") }
    O ("    " + $round + ": captions only, GetClassName and GetWindowText, " + $a16.ElapsedMilliseconds + " ms, caption `"" + $cap + "`". With the child reads " + $b16.ElapsedMilliseconds + " ms, text: " + $txt)
    $m16.Add([pscustomobject]@{ Round = $round; Caption = $cap; CaptionMs = $a16.ElapsedMilliseconds; ChildMs = $b16.ElapsedMilliseconds; Text = $txt })
  }
  Check "GetWindowText reads the caption of a window whose thread is blocked, under 1 s each time, so it waits on no message" ($m16[0].Caption -eq "NwcFederatorLoop blocked window" -and $m16[1].Caption -eq "NwcFederatorLoop blocked window" -and $m16[0].CaptionMs -lt 1000 -and $m16[1].CaptionMs -lt 1000) ([string]$m16[0].CaptionMs + " ms and " + $m16[1].CaptionMs + " ms")
  Check "just blocked, the child reads of one call stop within 2 s and write what they did not read" ($m16[0].ChildMs -le 2100 -and $m16[0].Text -match 'not read, because the 2 s one pass may spend reading children was spent') ([string]$m16[0].ChildMs + " ms")
  Check "blocked 7 s, SMTO_ABORTIFHUNG returns each child read at once" ($m16[1].ChildMs -lt 1000) ([string]$m16[1].ChildMs + " ms")
  StopStandins
  BaderSame "H16"

  # =====================================================================================
  O ""
  Case "==== H10, SETTINGS against HKCU\Software\NwcFederatorLoopTest\22.0 and a test folder ===="
  $h10 = Join-Path $Work "h10"
  $CU = [Microsoft.Win32.Registry]::CurrentUser
  $tkey = "Software\NwcFederatorLoopTest"
  $tsub = $tkey + "\22.0"
  $tapp = Join-Path $h10 "appdata\Autodesk\Navisworks Manage 2025"
  New-Item -ItemType Directory -Path $tapp, (Join-Path $tapp "AutoSave"), (Join-Path $h10 "fedlogs") | Out-Null
  function SetTree {
    $CU.DeleteSubKeyTree($tkey, $false)
    $k = $CU.CreateSubKey($tsub); $k.SetValue("A", "a1"); $k.SetValue("B", 1, [Microsoft.Win32.RegistryValueKind]::DWord); $k.Close()
    $k = $CU.CreateSubKey($tsub + "\K1"); $k.SetValue("V", "k1"); $k.Close()
    [System.IO.File]::WriteAllText((Join-Path $tapp "a.xml"), "a before", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $tapp "b.xml"), "b before", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $tapp "AutoSave\x.nwf"), "autosave", $utf8)
  }
  function Mutate {
    $k = $CU.OpenSubKey($tsub, $true); $k.SetValue("A", "a2"); $k.DeleteValue("B"); $k.SetValue("C", "c"); $k.Close()
    $CU.DeleteSubKeyTree($tsub + "\K1", $false)
    $k = $CU.CreateSubKey($tsub + "\K3"); $k.SetValue("X", "x"); $k.Close()
    [System.IO.File]::WriteAllText((Join-Path $tapp "a.xml"), "a changed", $utf8)
    [System.IO.File]::Delete((Join-Path $tapp "b.xml"))
  }
  function SyncWhole { $w = NewTestSync $h10; $w.Passes = 3; return $w }
  SetTree
  $wk = Join-Path $h10 "work1"; New-Item -ItemType Directory -Path $wk | Out-Null
  $bs10 = BackupSettings $wk $tsub $tapp (Join-Path $h10 "fedlogs")
  Check "BackupSettings backs up the test key and folder whole" ($bs10.Ok) $bs10.Why
  Mutate
  $pr10 = PutBackReasons (SyncWhole) @() $null $false @{} 999999 1 $null
  Check "with a whole record and the adopted pid gone, there is no reason not to put back" ($pr10.Why.Count -eq 0) (($pr10.Why) -join " | ")
  $spb = SettingsPutBack $true $pr10.Why $wk $tsub $bs10.RegBefore $bs10.RegRoot $tapp $bs10.FilesBefore $bs10.NotBacked $bs10.AutoBefore $bs10.AppBackup
  Check "everything is put back, nothing left unwritten" ($spb.PutBack -and $spb.Differ -gt 0 -and $spb.NotWritten -eq 0) ("differ " + $spb.Differ + ", not written " + $spb.NotWritten)
  Check "the test key reads exactly as before" (TreeSame (RegRead $tsub).Read $bs10.RegBefore.Read) ""
  Check "the test files read as before" (([System.IO.File]::ReadAllText((Join-Path $tapp "a.xml")) -eq "a before") -and ([System.IO.File]::ReadAllText((Join-Path $tapp "b.xml")) -eq "b before")) ""
  SetTree
  $wk2 = Join-Path $h10 "work2"; New-Item -ItemType Directory -Path $wk2 | Out-Null
  $bs10 = BackupSettings $wk2 $tsub $tapp (Join-Path $h10 "fedlogs")
  Mutate
  $after10 = RegRead $tsub
  [void](StartStandin "sleep 120" $null $null)
  $spb = SettingsPutBack $true @() $wk2 $tsub $bs10.RegBefore $bs10.RegRoot $tapp $bs10.FilesBefore $bs10.NotBacked $bs10.AutoBefore $bs10.AppBackup
  Check "with a stand-in Roamer started just before the first write, that write and every one after it stop" ($spb.NotWritten -eq $spb.Differ -and (TreeSame (RegRead $tsub).Read $after10.Read) -and [System.IO.File]::ReadAllText((Join-Path $tapp "a.xml")) -eq "a changed") ("differ " + $spb.Differ + ", not written " + $spb.NotWritten)
  StopStandins
  $src10 = Join-Path $h10 "src"; $bk10 = Join-Path $h10 "backup"
  New-Item -ItemType Directory -Path $src10, (Join-Path $src10 "Log") | Out-Null
  foreach ($nm in @("one.log", "two.log", "Log\three.log")) { [System.IO.File]::WriteAllText((Join-Path $src10 $nm), "text of " + $nm, $utf8) }
  $n1 = BackupNew $src10 $bk10 (Join-Path $h10 "list1.txt") "t1"
  $n2 = BackupNew $src10 $bk10 (Join-Path $h10 "list2.txt") "t2"
  [System.IO.File]::WriteAllText((Join-Path $src10 "two.log"), "changed", $utf8)
  $n3 = BackupNew $src10 $bk10 (Join-Path $h10 "list3.txt") "t3"
  Check "BackupNew copies every file the first time, read back" ($n1.Ok -and $n1.Copied -eq 3 -and $n1.Listed -eq 3) ("copied " + $n1.Copied)
  Check "BackupNew copies nothing the second time" ($n2.Ok -and $n2.Copied -eq 0) ("copied " + $n2.Copied)
  Check "BackupNew copies only the changed file the third time" ($n3.Ok -and $n3.Copied -eq 1 -and (Test-Path -LiteralPath (Join-Path $bk10 "since-t3\two.log"))) ("copied " + $n3.Copied)
  $asRoot = Join-Path $h10 "asroot"
  New-Item -ItemType Directory -Path (Join-Path $asRoot "AutoSave") | Out-Null
  foreach ($nm in @("a.nwf", "b.nwf")) { [System.IO.File]::WriteAllText((Join-Path $asRoot ("AutoSave\" + $nm)), "autosave " + $nm, $utf8) }
  $asList = Join-Path $h10 "autosave-before.txt"
  $nb = BackupNew (Join-Path $asRoot "AutoSave") (Join-Path $h10 "asbackup") $asList "t4"
  $rl = ReadListing $asList "AutoSave\"
  [System.IO.File]::WriteAllText((Join-Path $asRoot "AutoSave\b.nwf"), "changed by a run", $utf8)
  [System.IO.File]::WriteAllText((Join-Path $asRoot "AutoSave\c.nwf"), "added by a run", $utf8)
  $dAs = DiffAutoSave $rl (SettingsRead $asRoot)
  foreach ($l in $dAs) { O ("    | " + $l) }
  Check "the AutoSave compare reads autosave-before.txt back off the disk and names the changed file and the added one, and only those" ($nb.Ok -and $rl.Count -eq 2 -and $dAs.Count -eq 2 -and ($dAs -join "|") -match 'AutoSave\\b\.nwf' -and ($dAs -join "|") -match 'AutoSave\\c\.nwf' -and ($dAs -join "|") -notmatch 'a\.nwf') ([string]$dAs.Count + " lines")
  $CU.DeleteSubKeyTree($tkey, $false)
  Check "the test key is deleted" ($null -eq $CU.OpenSubKey($tkey)) ""
  BaderSame "H10"

  # =====================================================================================
  O ""
  Case "==== H11, M3, keep awake in a powershell -File process ===="
  $h11 = Join-Path $Work "h11"
  New-Item -ItemType Directory -Path $h11 | Out-Null
  $m3 = Join-Path $h11 "m3.ps1"
  $m3Text = '$ErrorActionPreference = "Stop"' + "`r`n" + '. "' + $guardFile + '"' + "`r`n" + '. ([scriptblock]::Create([System.IO.File]::ReadAllText("' + (Join-Path $h11 "run-functions.ps1") + '")))' + "`r`n" + '$wt = NewWinTypes' + "`r`n" + '$on = KeepAwake $wt.WinType $true' + "`r`n" + 'Start-Sleep -Seconds 2' + "`r`n" + '$off = KeepAwake $wt.WinType $false' + "`r`n" + '[Console]::Out.WriteLine("ON " + (Hex $on.Return) + " thread " + $on.Thread + " OFF " + (Hex $off.Return) + " thread " + $off.Thread)' + "`r`n"
  [System.IO.File]::WriteAllText((Join-Path $h11 "run-functions.ps1"), $runText, $utf8)
  [System.IO.File]::WriteAllText($m3, $m3Text, $utf8)
  $r = RunChild $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $m3 + "`"") $h11
  $mm = [regex]::Match($r.Out, 'ON (0x[0-9A-F]+) thread (\d+) OFF (0x[0-9A-F]+) thread (\d+)')
  O ("    | " + $r.Out.Trim() + " " + $r.Err.Trim())
  Check "M3: the request returns a value that is not 0" ($mm.Success -and $mm.Groups[1].Value -ne "0x00000000") $mm.Groups[1].Value
  Check "M3: the release returns exactly 0x80000003" ($mm.Success -and $mm.Groups[3].Value -eq "0x80000003") $mm.Groups[3].Value
  Check "M3: the native thread is the same at both" ($mm.Success -and $mm.Groups[2].Value -eq $mm.Groups[4].Value) ($mm.Groups[2].Value + " and " + $mm.Groups[4].Value)
  BaderSame "H11"

  # =====================================================================================
  O ""
  O "==== THE M5 AND M6 READERS, on the throwaway key and a folder under -Work ===="
  $m56 = Join-Path $Work "m56"
  New-Item -ItemType Directory -Path $m56 | Out-Null
  $k = $CU.CreateSubKey($tsub); $k.SetValue("A", "a1"); $k.Close()
  $kt1 = KeyTimes $winType $tkey
  Start-Sleep -Milliseconds 1100
  $k = $CU.OpenSubKey($tsub, $true); $k.SetValue("A", "a2"); $k.Close()
  $kt2 = KeyTimes $winType $tkey
  $kn = "HKEY_CURRENT_USER\" + $tsub
  Check "KeyTimes reads a key's write time through RegQueryInfoKey, and a value set a second later moves it" ($kt1.ContainsKey($kn) -and $kt1[$kn] -notmatch 'UNKNOWN' -and $kt1[$kn] -ne $kt2[$kn]) ([string]$kt1[$kn] + " then " + [string]$kt2[$kn])
  $CU.DeleteSubKeyTree($tkey, $false)
  [System.IO.File]::WriteAllText((Join-Path $m56 "old.txt"), "old", $utf8)
  [System.IO.File]::SetLastWriteTimeUtc((Join-Path $m56 "old.txt"), [DateTime]::UtcNow.AddHours(-1))
  [System.IO.File]::SetCreationTimeUtc((Join-Path $m56 "old.txt"), [DateTime]::UtcNow.AddHours(-1))
  $since = [DateTime]::UtcNow
  Start-Sleep -Milliseconds 100
  [System.IO.File]::WriteAllText((Join-Path $m56 "new.txt"), "new", $utf8)
  $nf = NewerFiles ([ordered]@{ "%TEST%" = $m56 }) $since
  Check "NewerFiles names a file written after the call by its folder's label, and not an older one" ((($nf) -join "|") -match '%TEST%\\new\.txt' -and (($nf) -join "|") -notmatch 'old\.txt' -and (($nf) -join "|") -match '1 files written at or after the call') (($nf) -join " | ")
  $lock = SessionLockText $winType
  Check "SessionLockText reads the session as locked or unlocked, with the answer's layout checked against this session's id" ($lock -eq "locked" -or $lock -eq "unlocked") $lock
  BaderSame "the M5 and M6 readers"

  # =====================================================================================
  O ""
  Case "==== H12, INSTALL AND THE STAMP, M2. install.ps1 never runs here ===="
  $h12 = Join-Path $Work "h12"
  New-Item -ItemType Directory -Path $h12 | Out-Null
  $bundleBefore = @(Get-ChildItem -LiteralPath $bundle -Recurse -File | ForEach-Object { $_.FullName + " " + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
  $s12 = StartStandin "sleep 120" $null $null
  O ("  " + (GuardsInPlace $s12))
  $fl12 = Join-Path $h12 "local"
  New-Item -ItemType Directory -Path $fl12 | Out-Null
  $r = EndChild (StartChildEnv $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $runPs + "`" -Mode Install -Stamp 00000000") $repo @{ LOCALAPPDATA = $fl12 }) 300
  $ref = @(Refused $r)
  Check "Install refuses while a stand-in Roamer runs, exit 2" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'Navisworks is running') $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  $bundleAfter = @(Get-ChildItem -LiteralPath $bundle -Recurse -File | ForEach-Object { $_.FullName + " " + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
  Check "the installed bundle's sha256 listing is the same before and after" ((($bundleBefore) -join "|") -eq (($bundleAfter) -join "|")) ([string]$bundleAfter.Count + " files")
  Check "no installs folder was made in the loop folder the call was given, the harness's own under -Work" (-not (Test-Path -LiteralPath (Join-Path $fl12 "NwcFederatorLoop\installs"))) ""
  StopStandins
  $scratch = Join-Path $h12 "scratch"
  New-Item -ItemType Directory -Path $scratch | Out-Null
  [void](RunChild "git" "init -q" $scratch)
  [System.IO.File]::WriteAllText((Join-Path $scratch "a.txt"), "a", $utf8)
  [void](RunChild "git" "add a.txt" $scratch)
  [void](RunChild "git" "-c user.name=harness -c user.email=harness@example.invalid -c commit.gpgsign=false commit -q -m scratch" $scratch)
  $head = (RunChild "git" "rev-parse --short=8 HEAD" $scratch).Out.Trim()
  $clean = TreeRefusal $scratch $head
  Check "the clean-tree check lets a clean scratch repository at its HEAD through" ($clean.Count -eq 0) (($clean) -join " | ")
  $wrong = TreeRefusal $scratch "00000000"
  Check "the clean-tree check refuses a HEAD that is not -Stamp" ($wrong.Count -ge 1 -and $wrong[0] -match 'HEAD is') $(if ($wrong.Count -gt 0) { $wrong[0] } else { "" })
  [System.IO.File]::WriteAllText((Join-Path $scratch "untracked.txt"), "u", $utf8)
  $dirty = TreeRefusal $scratch $head
  Check "the clean-tree check refuses one untracked file" ($dirty.Count -ge 1 -and $dirty[0] -match 'prints 1 lines, untracked files included') $(if ($dirty.Count -gt 0) { $dirty[0] } else { "" })
  foreach ($pair in @(@("the repo build", (Join-Path $repo "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll")), @("the installed build", (Join-Path $bundle "Contents\v22\Federator.Addin.dll")))) {
    if (-not (Test-Path -LiteralPath $pair[1])) { Check ("M2 on " + $pair[0]) $false "no DLL there"; continue }
    $copy = Join-Path $h12 ("m2-" + ($pair[0] -replace '\s', '-') + ".dll")
    Copy-Item -LiteralPath $pair[1] -Destination $copy
    $pv = [string][System.Diagnostics.FileVersionInfo]::GetVersionInfo($copy).ProductVersion
    $iv = (RunChild $ps ("-NoProfile -Command `"foreach (`$c in [System.Reflection.Assembly]::ReflectionOnlyLoadFrom('" + $copy + "').GetCustomAttributesData()) { if (`$c.AttributeType.Name -eq 'AssemblyInformationalVersionAttribute') { [Console]::Out.Write([string]`$c.ConstructorArguments[0].Value) } }`"") $h12).Out.Trim()
    Check ("M2 on " + $pair[0] + ": FileVersionInfo.ProductVersion equals AssemblyInformationalVersion") ($pv -ne "" -and $pv -eq $iv) ("ProductVersion `"" + $pv + "`", informational `"" + $iv + "`"")
  }
  $iv1 = InstallVerdict "abcdef12" $false
  $iv2 = InstallVerdict "abcdef12" $true
  Check "InstallVerdict: a Navisworks running right after the install changes the verdict to a FINDING and the exit to 5" ($iv1.Code -eq 0 -and $iv1.Text -eq "INSTALLED abcdef12" -and $iv2.Code -eq 5 -and $iv2.Text -match 'FINDING: a Navisworks is running right after the install') ($iv2.Code.ToString() + ", " + $iv2.Text)
  BaderSame "H12"

  # =====================================================================================
  O ""
  Case "==== H12b, build\install.ps1 REFUSES WHILE NAVISWORKS RUNS, a copy of it run with -SkipBuild against a fake APPDATA ===="
  $ir = Join-Path $h12 "irepo"
  $fakeApp = Join-Path $h12 "appdata"
  $fakeBundle = Join-Path $fakeApp "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
  $buildOut = Join-Path $repo "src\Federator.Addin\bin\Release\net48"
  New-Item -ItemType Directory -Path (Join-Path $ir "build"), (Join-Path $ir "bundle\ParsonsNwcFederator.bundle"), (Join-Path $ir "src\Federator.Addin\bin\Release\net48") | Out-Null
  Copy-Item -LiteralPath (Join-Path $repo "bundle\ParsonsNwcFederator.bundle\PackageContents.xml") -Destination (Join-Path $ir "bundle\ParsonsNwcFederator.bundle")
  # Since F109 attempt 2 install.ps1 dot-sources the one rule of what is a junction or a link
  # from beside it.
  Copy-Item -LiteralPath (Join-Path $repo "build\links.ps1") -Destination (Join-Path $ir "build")
  foreach ($f in @(Get-ChildItem -LiteralPath $buildOut -File -Filter "*.dll")) { Copy-Item -LiteralPath $f.FullName -Destination (Join-Path $ir "src\Federator.Addin\bin\Release\net48") }
  $newInstall = [System.IO.File]::ReadAllText((Join-Path $repo "build\install.ps1"))
  $oldInstall = (RunChild "git" "show 0eb4ede:build/install.ps1" $repo).Out
  $targetLine = '$target     = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"'
  Check "both copies of install.ps1 install into `$env:APPDATA, which the harness points at its own folder" ($newInstall.Contains($targetLine) -and $oldInstall.Contains($targetLine)) ""
  $echo = EndChild (StartChildEnv $ps "-NoProfile -Command [Console]::Out.Write(`$env:APPDATA)" $h12 @{ APPDATA = $fakeApp }) 60
  Check "a child started the same way reads APPDATA as the harness's folder" ($echo.Out.Trim() -eq $fakeApp) (Mask $echo.Out.Trim())
  $plugins = Join-Path $fakeApp "Autodesk\ApplicationPlugins"
  $elsewhere = Join-Path $h12 "elsewhere-plugins"
  function Aside { return @(Get-ChildItem -LiteralPath $plugins -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "*.replaced-*" }).Count }
  # Every file below $dir with its sha256 and every folder, one sorted line each joined by |.
  # The harness's own read, so no check leans on the code it checks.
  function TreeOf($dir) {
    if (-not (Test-Path -LiteralPath $dir)) { return "the folder is not there" }
    $root = (Get-Item -LiteralPath $dir -Force).FullName.TrimEnd('\')
    return ((@(Get-ChildItem -LiteralPath $root -Recurse -Force | ForEach-Object { $_.FullName.Substring($root.Length + 1) + $(if ($_.PSIsContainer) { "\" } else { " " + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }) }) | Sort-Object) -join "|")
  }
  # The bundle installed before for the in-place kinds, F109: three files the new one writes
  # over, one it already holds byte for byte, and a file and a folder it does not have.
  function OldBundle($dir) {
    New-Item -ItemType Directory -Path (Join-Path $dir "Contents\v22"), (Join-Path $dir "OldOnly") -Force | Out-Null
    foreach ($rel in @("PackageContents.xml", "Contents\v22\Federator.Addin.dll", "Contents\v22\Federator.Core.dll", "Contents\v22\Old.Only.dll", "OldOnly\note.txt")) { [System.IO.File]::WriteAllText((Join-Path $dir $rel), "the bundle installed before, " + $rel, $utf8) }
    Copy-Item -LiteralPath (Join-Path $buildOut "ClosedXML.dll") -Destination (Join-Path $dir "Contents\v22\ClosedXML.dll")
  }
  # A child powershell that opens $path for reading, sharing $share, writes a ready file and
  # sleeps 120 s, until EndChild ends it through its handle.
  $holdPs = Join-Path $h12 "hold.ps1"
  [System.IO.File]::WriteAllText($holdPs, "param([string]`$Path, [string]`$Share, [string]`$Ready)`r`n`$s = New-Object System.IO.FileStream(`$Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]`$Share)`r`n[System.IO.File]::WriteAllText(`$Ready, 'held')`r`nStart-Sleep -Seconds 120`r`n", $utf8)
  function HoldFile($path, $share) {
    $ready = Join-Path $h12 "holder-ready.txt"
    if (Test-Path -LiteralPath $ready) { Remove-Item -LiteralPath $ready }
    $c = StartChildEnv $ps ("-NoProfile -ExecutionPolicy Bypass -File `"" + $holdPs + "`" -Path `"" + $path + "`" -Share `"" + $share + "`" -Ready `"" + $ready + "`"") $h12 @{}
    $swh = [Diagnostics.Stopwatch]::StartNew(); while (-not (Test-Path -LiteralPath $ready) -and -not $c.P.HasExited -and $swh.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 100 }
    if (-not (Test-Path -LiteralPath $ready)) { [void](EndChild $c 1); throw ("the child powershell did not hold " + $path) }
    return $c
  }
  # Every file and folder below $dir with its attributes, one sorted line each joined by |.
  function MarksOf($dir) {
    if (-not (Test-Path -LiteralPath $dir)) { return "the folder is not there" }
    $root = (Get-Item -LiteralPath $dir -Force).FullName.TrimEnd('\')
    return ((@(Get-ChildItem -LiteralPath $root -Recurse -Force | ForEach-Object { $_.FullName.Substring($root.Length + 1) + " " + $_.Attributes }) | Sort-Object) -join "|")
  }
  # The folder a junction inside the fake bundle points at, and the folder made first at the
  # name of the copy beside it, F109 attempt 2.
  $linkTarget = Join-Path $h12 "linked-target"
  $takenAside = $fakeBundle + ".replaced-20000101-000000"
  # $how is roamer, held, junction or nothing, or since F109 an in-place kind. held loads a DLL
  # of the old bundle with Assembly.LoadFrom in a child powershell while install.ps1 runs, the
  # shape measured on 2026-09-30 with no Navisworks to refuse the rename of its folder.
  # junction makes ApplicationPlugins a junction to a folder elsewhere under the harness's
  # folder. An in-place kind writes the bundle of OldBundle and holds its
  # Contents\v22\Federator.Core.dll open in a child powershell while install.ps1 runs, sharing
  # read, write and delete for inplace-all and inplace-all-roamer, which also runs a stand-in
  # Roamer, read only for inplace-read, and nothing for inplace-none. Since F109 attempt 2,
  # sharing read, write and delete as well: inplace-junction puts a junction named Linked inside
  # the bundle, to a folder of its own holding one file, inplace-taken makes a folder with one
  # file at the name the copy beside the bundle would take, and inplace-marked marks a file
  # ReadOnly, a folder ReadOnly, a file Hidden and a file System. A $text of null is a copy of
  # install.ps1 that could not be made, and the trial does not run.
  function InstallTrial($label, $text, $how) {
    if ($null -eq $text) {
      O ("    " + $label + ": NOT RUN, its copy of install.ps1 could not be made")
      return [pscustomobject]@{ Exit = -2; Marker = $false; Addin = $false; Pkg = $false; Said = @(); Aside = -1; AsideMarker = $false; Failed = -1; Out = ""; Before = "not run"; After = "not run, after"; Staged = "not run, staged"; AsideTrees = @(); Roam = -1; LinkBefore = "not run"; LinkAfter = "not run, after"; TakenBefore = "not run"; MarksBefore = "not run"; MarksAfter = "not run, after" }
    }
    StopStandins
    if (Test-Path -LiteralPath $plugins) {
      if (((Get-Item -LiteralPath $plugins -Force).Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { [System.IO.Directory]::Delete($plugins, $false) }
      else { Remove-Item -LiteralPath $plugins -Recurse -Force }
    }
    $bundleNow = $fakeBundle
    if ($how -eq "junction") {
      if (Test-Path -LiteralPath $elsewhere) { Remove-Item -LiteralPath $elsewhere -Recurse -Force }
      $bundleNow = Join-Path $elsewhere "ParsonsNwcFederator.bundle"
      New-Item -ItemType Directory -Path $bundleNow | Out-Null
      New-Item -ItemType Directory -Path (Join-Path $fakeApp "Autodesk") -Force | Out-Null
      New-Item -ItemType Junction -Path $plugins -Target $elsewhere | Out-Null
    } else { New-Item -ItemType Directory -Path $fakeBundle | Out-Null }
    [System.IO.File]::WriteAllText((Join-Path $bundleNow "marker.txt"), "the bundle installed before", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $ir "build\install.ps1"), $text, $utf8)
    $share = @{ "inplace-all" = "ReadWrite, Delete"; "inplace-all-roamer" = "ReadWrite, Delete"; "inplace-read" = "Read"; "inplace-none" = "None"; "inplace-junction" = "ReadWrite, Delete"; "inplace-taken" = "ReadWrite, Delete"; "inplace-marked" = "ReadWrite, Delete" }[$how]
    if ($null -ne $share) { OldBundle $bundleNow }
    $oldDll = Join-Path $bundleNow "old\Federator.Core.dll"
    if ($how -eq "held") {
      New-Item -ItemType Directory -Path (Join-Path $bundleNow "old") | Out-Null
      Copy-Item -LiteralPath (Join-Path $buildOut "Federator.Core.dll") -Destination $oldDll
    }
    if (Test-Path -LiteralPath $linkTarget) { Remove-Item -LiteralPath $linkTarget -Recurse -Force }
    if ($how -eq "inplace-junction") {
      New-Item -ItemType Directory -Path $linkTarget | Out-Null
      [System.IO.File]::WriteAllText((Join-Path $linkTarget "elsewhere.txt"), "a file the junction inside the bundle points at", $utf8)
      New-Item -ItemType Junction -Path (Join-Path $bundleNow "Linked") -Target $linkTarget | Out-Null
    }
    if ($how -eq "inplace-taken") {
      New-Item -ItemType Directory -Path $takenAside | Out-Null
      [System.IO.File]::WriteAllText((Join-Path $takenAside "taken.txt"), "a folder already at the name of the copy", $utf8)
    }
    if ($how -eq "inplace-marked") {
      foreach ($pair in @(@("Contents\v22\Federator.Addin.dll", "ReadOnly"), @("OldOnly", "Directory, ReadOnly"), @("OldOnly\note.txt", "Hidden"), @("PackageContents.xml", "System"))) { [System.IO.File]::SetAttributes((Join-Path $bundleNow $pair[0]), [System.IO.FileAttributes]$pair[1]) }
    }
    $before = TreeOf $bundleNow
    $linkBefore = TreeOf $linkTarget
    $takenBefore = TreeOf $takenAside
    $marksBefore = MarksOf $bundleNow
    if ($how -eq "roamer" -or $how -eq "inplace-all-roamer") { [void](StartStandin "sleep 120" $null $null) }
    $loader = $null
    if ($how -eq "held") {
      $ready = Join-Path $h12 "loader-ready.txt"
      if (Test-Path -LiteralPath $ready) { Remove-Item -LiteralPath $ready }
      $loader = StartChildEnv $ps ("-NoProfile -Command `"[void][System.Reflection.Assembly]::LoadFrom('" + $oldDll + "'); [System.IO.File]::WriteAllText('" + $ready + "', 'loaded'); Start-Sleep -Seconds 60`"") $h12 @{}
      $swl = [Diagnostics.Stopwatch]::StartNew(); while (-not (Test-Path -LiteralPath $ready) -and $swl.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 100 }
      if (-not (Test-Path -LiteralPath $ready)) { throw "the child powershell did not load the old bundle's DLL" }
    }
    $roam = @(Get-Process -Name Roamer -ErrorAction SilentlyContinue).Count
    if ($echo.Out.Trim() -ne $fakeApp) { throw "the APPDATA of a child is not the harness's folder, so no install.ps1 copy runs" }
    $holder = $null
    try {
      if ($null -ne $share) { $holder = HoldFile (Join-Path $bundleNow "Contents\v22\Federator.Core.dll") $share }
      $res = EndChild (StartChildEnv $ps ("-NoProfile -ExecutionPolicy Bypass -File `"" + (Join-Path $ir "build\install.ps1") + "`" -SkipBuild") $ir @{ APPDATA = $fakeApp }) 180
    }
    finally {
      if ($null -ne $loader) { [void](EndChild $loader 1) }
      if ($null -ne $holder) { [void](EndChild $holder 1) }
    }
    $marker = Test-Path -LiteralPath (Join-Path $bundleNow "marker.txt")
    $addin = Test-Path -LiteralPath (Join-Path $bundleNow "Contents\v22\Federator.Addin.dll")
    $pkg = Test-Path -LiteralPath (Join-Path $bundleNow "PackageContents.xml")
    $after = TreeOf $bundleNow
    $linkAfter = TreeOf $linkTarget
    $marksAfter = MarksOf $bundleNow
    # The junction itself is removed, never what it points at, before anything is removed
    # recursively.
    $linked = Join-Path $bundleNow "Linked"
    if (Test-Path -LiteralPath $linked) { [System.IO.Directory]::Delete($linked, $false) }
    $staged = TreeOf (Join-Path $ir "artifacts\ParsonsNwcFederator.bundle")
    $aside = 0; $failed = 0; $asideMarker = $false; $asideTrees = @()
    if ($how -eq "junction") { $aside = @(Get-ChildItem -LiteralPath $elsewhere -Directory -Force | Where-Object { $_.Name -like "*.replaced-*" }).Count; [System.IO.Directory]::Delete($plugins, $false) }
    else {
      $asideDirs = @(Get-ChildItem -LiteralPath $plugins -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "*.replaced-*" })
      $aside = $asideDirs.Count
      foreach ($ad in $asideDirs) { if (Test-Path -LiteralPath (Join-Path $ad.FullName "marker.txt")) { $asideMarker = $true }; $asideTrees += (TreeOf $ad.FullName) }
      $failed = @(Get-ChildItem -LiteralPath $plugins -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "*.failed-*" }).Count
    }
    $said = @(Refused $res)
    O ("    " + $label + ": Roamers running " + $roam + ", exit " + $res.Exit + ", the old bundle's marker still there " + $marker + ", the new add-in there " + $addin + ", folders moved aside and left " + $aside + ", the old marker in one " + $asideMarker + ", new ones moved aside " + $failed + ", the bundle reads as before by sha256 " + ($after -eq $before) + ", as the new one " + ($after -eq $staged) + $(if ($said.Count -gt 0) { ", said: " + $said[0] } else { "" }))
    foreach ($l in @($res.Out.Split("`n") | Where-Object { $_ -match 'HARNESS COPY|installed before|failed|LEFT:|^IN PLACE:|^Replaced in place|^Files not written' })) { O ("      | " + $l.Trim().Replace($h12, "<h12>")) }
    StopStandins
    return [pscustomobject]@{ Exit = $res.Exit; Marker = $marker; Addin = $addin; Pkg = $pkg; Said = $said; Aside = $aside; AsideMarker = $asideMarker; Failed = $failed; Out = $res.Out; Before = $before; After = $after; Staged = $staged; AsideTrees = $asideTrees; Roam = $roam; LinkBefore = $linkBefore; LinkAfter = $linkAfter; TakenBefore = $takenBefore; MarksBefore = $marksBefore; MarksAfter = $marksAfter }
  }
  # A copy of $text, by default install.ps1 as it is now, with one line replaced, found exactly
  # once. When it is not, the line says so and null is returned, so that trial does not run
  # and its checks fail, and the cases after it still run.
  function InstallWith($from, $to, $text) {
    if ($null -eq $text) { $text = $newInstall }
    $n = ([regex]::Matches($text, [regex]::Escape($from))).Count
    if ($n -ne 1) { O ("    a replacement of the install.ps1 copy is found " + $n + " times, so that copy is not made: " + $from); return $null }
    return $text.Replace($from, $to)
  }
  # The lines of a trial's output that match $pattern, each without its line end, always as an
  # array, so one line is not handed back as a string.
  function Lines($t, $pattern) { return ,@($t.Out.Split("`n") | ForEach-Object { $_.TrimEnd() } | Where-Object { $_ -match $pattern }) }
  $t1 = InstallTrial "before, install.ps1 at 0eb4ede with a stand-in Roamer running" $oldInstall "roamer"
  Check "before: install.ps1 at 0eb4ede replaces the bundle while a Roamer runs, the fault the breaker found" ($t1.Exit -eq 0 -and -not $t1.Marker -and $t1.Addin) ("exit " + $t1.Exit)
  $t2 = InstallTrial "after, install.ps1 now with a stand-in Roamer running" $newInstall "roamer"
  Check "after: install.ps1 refuses with one REFUSED line saying Navisworks is running and must be closed first, exit 2, and the bundle is left as it was" ($t2.Exit -eq 2 -and $t2.Marker -and -not $t2.Addin -and $t2.Said.Count -eq 1 -and $t2.Said[0] -match '^REFUSED: Navisworks is running.*must be closed first') ("exit " + $t2.Exit)
  $t4 = InstallTrial "fix list 2 item 10, a DLL of the installed bundle loaded with Assembly.LoadFrom in a child powershell" $newInstall "held"
  # Since F109 a move aside refused with no Navisworks running goes in place. The loaded DLL
  # is one the new bundle does not have, and it cannot be removed, measured on 2026-10-01 and
  # 2026-10-04, so the install fails after its first writes and puts every file back.
  Check "item 10, since F109: with a DLL of the bundle loaded in another process and no Navisworks, the move aside is refused and the bundle is replaced in place, the loaded DLL cannot be removed, so every file and folder is put back and reads as before by sha256, exit 1, with nothing moved aside or left beside it" ($t4.Exit -eq 1 -and $t4.Marker -and -not $t4.Addin -and $t4.Aside -eq 0 -and $t4.Failed -eq 0 -and $t4.Before -eq $t4.After -and (Lines $t4 '^IN PLACE: the installed add-in could not be moved aside, ').Count -eq 1 -and (Lines $t4 '^The new add-in could not be copied in, old\\Federator\.Core\.dll could not be removed, .+ put back in place with \d+ written back and \d+ removed, and read back by sha256 against the bundle as it read before anything was written over, its copy beside it was removed').Count -eq 1) ("exit " + $t4.Exit)
  $t5 = InstallTrial "fix list 2 item 11, Autodesk\ApplicationPlugins a junction to a folder elsewhere" $newInstall "junction"
  Check "item 11: through a junction nothing is installed and nothing is removed, exit 2, one REFUSED line naming the junction" ($t5.Exit -eq 2 -and $t5.Marker -and -not $t5.Addin -and $t5.Aside -eq 0 -and $t5.Said.Count -eq 1 -and $t5.Said[0] -match '^REFUSED: .*ApplicationPlugins is a junction or a link') ("exit " + $t5.Exit)
  $copyLine = '    Copy-Item (Join-Path $staging "*") $target -Recurse -Force'
  $ta1 = InstallTrial "fix list 3 item 6a, the copy fails part way and the new one can be removed" (InstallWith $copyLine '    Copy-Item (Join-Path $staging "PackageContents.xml") $target -Force; throw "HARNESS COPY: the copy fails part way"') ""
  Check "item 6a: the copy fails, the new one is removed, the old one is put back where it was and the message says so, exit 1" ($ta1.Exit -eq 1 -and $ta1.Marker -and -not $ta1.Pkg -and $ta1.Aside -eq 0 -and $ta1.Failed -eq 0 -and $ta1.Out.Contains("The add-in installed before is at " + $fakeBundle + ", and the new one that failed was removed.")) ("exit " + $ta1.Exit)
  $ta2 = InstallTrial "fix list 3 item 6a, the copy fails part way and a file of the new one is held" (InstallWith $copyLine '    Copy-Item (Join-Path $staging "PackageContents.xml") $target -Force; $script:harnessHeld = [System.IO.File]::Open((Join-Path $target "PackageContents.xml"), "Open", "Read", "None"); throw "HARNESS COPY: the copy fails part way, a file of it held"') ""
  Check "item 6a: when the new one can be neither removed nor moved aside, the old one stays whole where it was moved, and the message names where each is, exit 1" ($ta2.Exit -eq 1 -and -not $ta2.Marker -and $ta2.Pkg -and $ta2.Aside -eq 1 -and $ta2.AsideMarker -and $ta2.Out -match ('The add-in installed before is at ' + [regex]::Escape($fakeBundle) + '\.replaced-\d{8}-\d{6}, and the new one that failed is at ' + [regex]::Escape($fakeBundle) + '\.')) ("exit " + $ta2.Exit)
  $tb = InstallTrial "fix list 3 item 6b, a check of the new one fails after the copy" (InstallWith '$nested = Join-Path $target (Split-Path -Leaf $target)' '$nested = Join-Path $target "PackageContents.xml"') ""
  Check "item 6b: a check that fails after the copy leaves the old one whole, put back where it was, the failed new one removed, exit 1" ($tb.Exit -eq 1 -and $tb.Marker -and -not $tb.Addin -and $tb.Aside -eq 0 -and $tb.Failed -eq 0 -and $tb.Out.Contains("The new add-in failed its check, The bundle ended up inside itself") -and $tb.Out.Contains("The add-in installed before is at " + $fakeBundle + ", and the new one that failed was removed.")) ("exit " + $tb.Exit)
  # Since F109 attempt 2 the last removal is the one RemoveCopy, so the copy replaces its call.
  $lastRemoval = '    $leftWhy = RemoveCopy'
  $tc = InstallTrial "fix list 3 item 6c, the add-in installed before cannot be removed after every check passed" (InstallWith $lastRemoval '    $leftWhy = "HARNESS COPY: the removal of the add-in installed before fails"') ""
  Check "item 6c: the install finishes, exit 0, with one LEFT line naming where the old one is" ($tc.Exit -eq 0 -and $tc.Addin -and $tc.Aside -eq 1 -and $tc.AsideMarker -and @($tc.Out.Split("`n") | Where-Object { $_ -match '^LEFT: the add-in installed before is at ' }).Count -eq 1) ("exit " + $tc.Exit)
  $leftC = @(BundleLeftovers $fakeBundle)
  $ivC = InstallVerdict "abcdef12" $false $leftC
  O ("    | " + $ivC.Code + ", " + $ivC.Text)
  Check "item 6c: run.ps1 names what was left beside the new bundle in its verdict, a FINDING with exit 5" ($leftC.Count -eq 1 -and $leftC[0] -match 'ParsonsNwcFederator\.bundle\.replaced-\d{8}-\d{6}$' -and $ivC.Code -eq 5 -and $ivC.Text.Contains("FINDING: the add-in installed before is left beside the new one, " + $leftC[0])) ([string]$leftC.Count + " left")
  $t3 = InstallTrial "control, install.ps1 now with no Roamer running" $newInstall ""
  Check "control: with no Roamer running install.ps1 installs as before, and the bundle it moved aside is removed" ($t3.Exit -eq 0 -and -not $t3.Marker -and $t3.Addin -and $t3.Aside -eq 0) ("exit " + $t3.Exit + ", moved aside and left " + $t3.Aside)
  Check "control: run.ps1 finds nothing left beside the bundle" (@(BundleLeftovers $fakeBundle).Count -eq 0) ""
  BaderSame "H12b"

  # =====================================================================================
  O ""
  Case "==== H12c, F109, build\install.ps1 REPLACES THE BUNDLE IN PLACE when its move aside is refused and no Navisworks runs, copies of it run with -SkipBuild against the fake APPDATA ===="
  # Measured on 2026-10-01 and again on 2026-10-04 in a throwaway folder, with no image
  # mapped: one file beneath a folder held open for reading by another process refuses the
  # rename of the folder, access to the path is denied, whatever the share mode. Held sharing
  # read, write and delete, the file can still be written over and removed, held sharing read
  # only it cannot be written over, and held sharing nothing it cannot be read. So each case
  # holds one file of the bundle installed before, and install.ps1 is the one committed, bar
  # the cases whose title names the line their copy replaces, and the first, which runs
  # main's install.ps1 before F109.
  $mainInstall = (RunChild "git" "show 398b910:build/install.ps1" $repo).Out
  $ip0 = InstallTrial "F109 before, install.ps1 at 398b910, the one Windows refused three times on 2026-10-01, a file of the installed bundle held open sharing read, write and delete, no Roamer" $mainInstall "inplace-all"
  Check "F109 before: install.ps1 at 398b910 refuses with no Roamer running, exit 2, saying a Navisworks most likely holds the bundle, and nothing is installed, the fault F109 fixes" ($ip0.Roam -eq 0 -and $ip0.Exit -eq 2 -and $ip0.Said.Count -eq 1 -and $ip0.Said[0] -match 'a file in it is held, most likely by a Navisworks that is running' -and $ip0.Before -eq $ip0.After -and $ip0.Aside -eq 0) ("exit " + $ip0.Exit + ", Roamers " + $ip0.Roam)
  $ip1 = InstallTrial "F109 a, a file of the installed bundle held open by another process sharing read, write and delete, no Roamer" $newInstall "inplace-all"
  $ip1In = Lines $ip1 '^IN PLACE: the installed add-in could not be moved aside, Access to the path .+ is denied, and no process named Roamer runs, so its files are replaced where they are\. A copy of it was made first at .+\.replaced-\d{8}-\d{6} and read back by sha256, and it is kept until every check of the new one has passed\. If this stops before its last line, the add-in may hold some new files and some old ones, so start no Navisworks, and run this again to finish, or make the add-in hold exactly the files of that copy to undo\.$'
  Check "F109 a: the move aside is refused, and one IN PLACE line says so with what Windows answered, that no process named Roamer runs, and what a stop part way leaves and how to finish or undo it, and no line says Navisworks is running" ($ip1In.Count -eq 1 -and (Lines $ip1 'Navisworks is running|most likely by a Navisworks').Count -eq 0) $(if ($ip1In.Count -gt 0) { $ip1In[0].Replace($h12, "<h12>") } else { "no IN PLACE line" })
  Check "F109 a: the install finishes, exit 0, and the installed bundle reads as the new one by sha256, file for file and folder for folder, so the marker, the file and the folder the new one does not have are gone" ($ip1.Exit -eq 0 -and $ip1.After -eq $ip1.Staged -and $ip1.Staged.Contains("Contents\v22\Federator.Addin.dll ") -and -not $ip1.Marker) ("exit " + $ip1.Exit)
  $ip1Head = Lines $ip1 '^Files not written, because the installed one already held the same bytes by sha256:$'
  $ip1Kept = Lines $ip1 '^  Contents\\v22\\ClosedXML\.dll  \(\d+ bytes\)$'
  Check "F109 a: the one file the bundle already held byte for byte is listed once, apart, as not written, and the three files it did not keep are named as removed" ($ip1Head.Count -eq 1 -and $ip1Kept.Count -eq 1 -and $ip1.Out.IndexOf($ip1Head[0]) -lt $ip1.Out.IndexOf($ip1Kept[0]) -and (Lines $ip1 '^  removed  (marker\.txt|Contents\\v22\\Old\.Only\.dll|OldOnly\\note\.txt)$').Count -eq 3) ""
  Check "F109 a: the copy beside it is removed once every check passed, nothing is left beside the bundle, and run.ps1 finds nothing there" ($ip1.Aside -eq 0 -and $ip1.Failed -eq 0 -and @(BundleLeftovers $fakeBundle).Count -eq 0) ""
  $ip2 = InstallTrial "F109 b, a file the new one writes over held open sharing read only, so a write fails mid way" $newInstall "inplace-read"
  Check "F109 b: a write fails mid way, exit 1, and every file and folder of the bundle installed before reads as before by sha256, those the new one added are gone, and nothing is left beside it" ($ip2.Exit -eq 1 -and $ip2.Before -eq $ip2.After -and $ip2.Aside -eq 0 -and $ip2.Failed -eq 0 -and (Lines $ip2 '^IN PLACE: ').Count -eq 1 -and (Lines $ip2 'Navisworks is running|most likely by a Navisworks').Count -eq 0) ("exit " + $ip2.Exit)
  $ip2Back = Lines $ip2 '^The new add-in could not be copied in, Contents\\v22\\Federator\.Core\.dll could not be written, .+\. The add-in installed before is at .+, put back in place with [1-9]\d* written back and [1-9]\d* removed, and read back by sha256 against the bundle as it read before anything was written over, its copy beside it was removed, and the new one that failed is at .+\.$'
  Check "F109 b: one line names the file that could not be written, how many files were written back and removed, and where each thing is" ($ip2Back.Count -eq 1) $(if ($ip2Back.Count -gt 0) { $ip2Back[0].Replace($h12, "<h12>") } else { "no such line" })
  $firstRead = 'if ($null -ne $navisworks) { Write-Host ("REFUSED: " + $navisworks + ". Nothing was installed."); exit 2 }'
  $ip3 = InstallTrial "F109 c, its first read of the process list replaced by a line that only prints it, a stand-in Roamer running and a file held sharing read, write and delete" (InstallWith $firstRead 'if ($null -ne $navisworks) { Write-Host ("HARNESS COPY: the first read of the process list is skipped, it found " + $navisworks) }') "inplace-all-roamer"
  Check "F109 c: the second read, made once the move aside is refused, finds the Roamer and refuses, exit 2, one REFUSED line naming its pid and what Windows answered, no IN PLACE line, and the bundle reads as before by sha256 with nothing beside it" ($ip3.Exit -eq 2 -and $ip3.Said.Count -eq 1 -and $ip3.Said[0] -match '^REFUSED: the installed add-in could not be moved aside, Access to the path .+ is denied, and Navisworks is running, Roamer pid \d+, and must be closed first\. Close it and run this again\. The installed add-in is left whole\. Nothing was installed\.$' -and (Lines $ip3 '^HARNESS COPY: the first read of the process list is skipped, it found Navisworks is running, Roamer pid \d+').Count -eq 1 -and (Lines $ip3 '^IN PLACE: ').Count -eq 0 -and $ip3.Before -eq $ip3.After -and $ip3.Aside -eq 0) ("exit " + $ip3.Exit)
  $ip4 = InstallTrial "F109 d, a file of the installed bundle held open sharing nothing, so the bundle cannot be read whole and no copy of it is made" $newInstall "inplace-none"
  Check "F109 d: the installed bundle cannot be read whole, so the install refuses before any copy is made, exit 2, one REFUSED line saying no process named Roamer runs and naming the file it could not read, no IN PLACE line, and the bundle reads as before by sha256 with nothing beside it" ($ip4.Exit -eq 2 -and $ip4.Said.Count -eq 1 -and $ip4.Said[0] -match '^REFUSED: the installed add-in could not be moved aside, Access to the path .+ is denied, no process named Roamer runs, and it is not replaced in place, Contents\\v22\\Federator\.Core\.dll could not be read, .+\. The installed add-in is left whole\. Nothing was installed\.$' -and (Lines $ip4 '^IN PLACE: ').Count -eq 0 -and (Lines $ip4 'Navisworks is running|most likely by a Navisworks').Count -eq 0 -and $ip4.Before -eq $ip4.After -and $ip4.Aside -eq 0) ("exit " + $ip4.Exit)
  $ip5 = InstallTrial "F109 e, in place, and its copy beside it cannot be removed after every check passed" (InstallWith $lastRemoval '    $leftWhy = "HARNESS COPY: the removal of the add-in installed before fails"') "inplace-all"
  $leftE = @(BundleLeftovers $fakeBundle)
  $ivE = InstallVerdict "abcdef12" $false $leftE
  O ("    | " + $ivE.Code + ", " + $ivE.Text)
  Check "F109 e: the install finishes, exit 0, with one LEFT line naming the copy beside it, which reads as the bundle installed before by sha256, and the bundle reads as the new one" ($ip5.Exit -eq 0 -and (Lines $ip5 '^LEFT: the add-in installed before is at .+\.replaced-\d{8}-\d{6} and could not be removed').Count -eq 1 -and $ip5.Aside -eq 1 -and $ip5.AsideTrees.Count -eq 1 -and $ip5.AsideTrees[0] -eq $ip5.Before -and $ip5.After -eq $ip5.Staged) ("exit " + $ip5.Exit)
  Check "F109 e: run.ps1 names the copy left beside the new bundle in its verdict, a FINDING with exit 5, as it names a bundle moved aside" ($leftE.Count -eq 1 -and $leftE[0] -match 'ParsonsNwcFederator\.bundle\.replaced-\d{8}-\d{6}$' -and $ivE.Code -eq 5 -and $ivE.Text.Contains("FINDING: the add-in installed before is left beside the new one, " + $leftE[0])) ([string]$leftE.Count + " left")
  $ip6 = InstallTrial "F109 f, a write fails mid way and its put back is replaced by a line that fails" (InstallWith '        try { $back = WriteOver $aside $old $target }' '        try { throw "HARNESS COPY: the put back fails" }') "inplace-read"
  $leftF = @(BundleLeftovers $fakeBundle)
  Check "F109 f: exit 1, the copy beside it is kept and reads as the bundle installed before by sha256, one line says so, read again at that moment, and names where it is, the bundle and the new one, and BundleLeftovers, which run.ps1 -Mode Install calls only after an exit 0, finds it" ($ip6.Exit -eq 1 -and $ip6.Aside -eq 1 -and $ip6.AsideTrees.Count -eq 1 -and $ip6.AsideTrees[0] -eq $ip6.Before -and (Lines $ip6 '^The new add-in could not be copied in, .+\. The add-in installed before could not be put back in place, HARNESS COPY: the put back fails\. Its copy at .+\.replaced-\d{8}-\d{6} reads as the bundle did before anything was written over, by sha256, what is at .+ was not proved to be it, and the new one is at .+\.$').Count -eq 1 -and $leftF.Count -eq 1) ("exit " + $ip6.Exit)
  # F109 attempt 2. The two guards fix attempt 1 added with no case, the rule of what is a
  # junction or a link, the marks Windows refuses the in-place write for, and a copy that
  # changes after it was read back.
  $ip7 = InstallTrial "F109 g, a junction named Linked inside the installed bundle, to a folder of the harness's holding one file, and a file held sharing read, write and delete" $newInstall "inplace-junction"
  Check "F109 g: the junction inside the bundle is refused by name before any copy is made, exit 2, one REFUSED line naming it and saying no process named Roamer runs, no IN PLACE line, and the bundle and the folder the junction points at read as before by sha256, with nothing beside the bundle" ($ip7.Exit -eq 2 -and $ip7.Said.Count -eq 1 -and $ip7.Said[0] -match '^REFUSED: the installed add-in could not be moved aside, Access to the path .+ is denied, no process named Roamer runs, and it is not replaced in place, Linked is a junction or a link, and nothing is written or removed through one\. The installed add-in is left whole\. Nothing was installed\.$' -and (Lines $ip7 '^IN PLACE: ').Count -eq 0 -and $ip7.Before -eq $ip7.After -and $ip7.Before.Contains("Linked\") -and $ip7.LinkBefore -eq $ip7.LinkAfter -and $ip7.LinkBefore.Contains("elsewhere.txt ") -and $ip7.Aside -eq 0) $(if ($ip7.Said.Count -gt 0) { $ip7.Said[0].Replace($h12, "<h12>") } else { "exit " + $ip7.Exit })
  $asideLine = '    $aside = $target + ".replaced-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")'
  $ip8 = InstallTrial "F109 h, the name of the copy beside the bundle already taken by a folder holding one file, its copy of install.ps1 naming the copy .replaced-20000101-000000, and a file held sharing read, write and delete" (InstallWith $asideLine '    $aside = $target + ".replaced-20000101-000000"') "inplace-taken"
  Check "F109 h: the name of the copy is taken, so the install refuses before any copy is made, exit 2, one REFUSED line naming that folder and saying no process named Roamer runs, no IN PLACE line, and the bundle and that folder read as before by sha256, with no other folder beside the bundle" ($ip8.Exit -eq 2 -and $ip8.Said.Count -eq 1 -and $ip8.Said[0] -match ('^REFUSED: the installed add-in could not be moved aside, .+, no process named Roamer runs, and ' + [regex]::Escape($takenAside) + ' is there already, so no copy of it is made there\. The installed add-in is left whole\. Nothing was installed\.$') -and (Lines $ip8 '^IN PLACE: ').Count -eq 0 -and $ip8.Before -eq $ip8.After -and $ip8.Aside -eq 1 -and $ip8.AsideTrees.Count -eq 1 -and $ip8.AsideTrees[0] -eq $ip8.TakenBefore -and $ip8.TakenBefore.Contains("taken.txt ")) $(if ($ip8.Said.Count -gt 0) { $ip8.Said[0].Replace($h12, "<h12>") } else { "exit " + $ip8.Exit })
  # F109 i. The rule in build\links.ps1, read on a junction the harness makes, on a plain
  # folder, and on every item of the bundle folder of the clone this checkout belongs to, which
  # on this machine sits under OneDrive, where every item carried the reparse point attribute
  # with no link type on 2026-10-04. Then install.ps1's Listing and prepare-copy.ps1's
  # Read-Folder of that folder, each read through the parser into a scope of its own. Read
  # only, and nothing held online only is read.
  $linksFile = Join-Path $repo "build\links.ps1"
  $gcd = (RunChild "git" "rev-parse --git-common-dir" $repo).Out.Trim()
  if (-not [System.IO.Path]::IsPathRooted($gcd)) { $gcd = Join-Path $repo $gcd }
  $clone = Split-Path ([System.IO.Path]::GetFullPath($gcd)) -Parent
  $odDir = Join-Path $clone "bundle"
  $odItems = @(Get-ChildItem -LiteralPath $odDir -Recurse -Force)
  $odPlain = @($odItems | Where-Object { ($_.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -eq 0 }).Count
  $odOnline = @($odItems | Where-Object { ([int]$_.Attributes -band (0x400000 -bor 0x40000 -bor 0x1000)) -ne 0 }).Count
  O ("    the clone's bundle folder, <clone>\bundle: " + $odItems.Count + " items, " + $odPlain + " without the reparse point attribute, " + $odOnline + " held online only")
  $ruleTarget = Join-Path $h12 "rule-target"
  $ruleJunction = Join-Path $h12 "rule-junction"
  New-Item -ItemType Directory -Path $ruleTarget | Out-Null
  New-Item -ItemType Junction -Path $ruleJunction -Target $ruleTarget | Out-Null
  $rule = & { . $linksFile; [pscustomobject]@{ Junction = (LinkKind (Get-Item -LiteralPath $ruleJunction -Force)); Plain = (LinkKind (Get-Item -LiteralPath $ruleTarget -Force)); OneDrive = @($odItems | Where-Object { $null -ne (LinkKind $_) }).Count } }
  [System.IO.Directory]::Delete($ruleJunction, $false)
  Check "F109 i: the rule in build\links.ps1 reads a junction as one, a plain folder as none, and none of the items of the clone's bundle folder under OneDrive as one, each of them carrying the reparse point attribute" ($rule.Junction -eq "Junction" -and $null -eq $rule.Plain -and $odItems.Count -ge 2 -and $odPlain -eq 0 -and $rule.OneDrive -eq 0) ("the junction reads " + $rule.Junction + ", the plain folder " + $(if ($null -eq $rule.Plain) { "none" } else { $rule.Plain }) + ", " + $rule.OneDrive + " of " + $odItems.Count + " OneDrive items as a link, " + $odPlain + " of them without the attribute")
  # The function definitions of a script, read through the parser, never its main flow.
  function FunctionsOf($text) {
    $tk3 = $null; $pe3 = $null
    $ast3 = [System.Management.Automation.Language.Parser]::ParseInput($text, [ref]$tk3, [ref]$pe3)
    return ((@($ast3.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] }) | ForEach-Object { $_.Extent.Text }) -join "`r`n")
  }
  $odWant = "not read"; $odInstall = "not read, an item is held online only or carries no reparse point attribute"; $odPrepare = $odInstall
  if ($odOnline -eq 0 -and $odPlain -eq 0 -and $odItems.Count -ge 2) {
    $odWant = (@($odItems | ForEach-Object { $_.FullName.Substring($odDir.Length + 1) + $(if ($_.PSIsContainer) { "\" } else { " " + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }) }) | Sort-Object) -join "|"
    $insFns = FunctionsOf $newInstall
    $odInstall = & { . $linksFile; . ([scriptblock]::Create($insFns)); try { $l = Listing $odDir; ((@($l.Files.Keys | ForEach-Object { $_ + " " + $l.Files[$_] }) + @($l.Folders | ForEach-Object { $_ + "\" })) | Sort-Object) -join "|" } catch { "refused: " + $_.Exception.Message } }
    $prepFns = FunctionsOf ([System.IO.File]::ReadAllText((Join-Path $PSScriptRoot "prepare-copy.ps1")))
    $odPrepare = & { . $linksFile; $ordinal = [StringComparer]::Ordinal; $online = 0x400000 -bor 0x40000 -bor 0x1000; . ([scriptblock]::Create($prepFns)); try { $r = Read-Folder $odDir $true; ((@($r.Files.Keys | ForEach-Object { $_ + " " + $r.Files[$_].Sha }) + @($r.Dirs | ForEach-Object { $_ + "\" })) | Sort-Object) -join "|" } catch { "refused: " + $_.Exception.Message } }
  }
  Check "F109 i: install.ps1's Listing reads the clone's bundle folder under OneDrive whole, every file by sha256 and every folder, as the harness reads it" ($odWant -ne "not read" -and $odInstall -eq $odWant) $odInstall.Replace($clone, "<clone>")
  Check "F109 i: prepare-copy.ps1's Read-Folder reads the same folder whole, through the same rule" ($odWant -ne "not read" -and $odPrepare -eq $odWant) $odPrepare.Replace($clone, "<clone>")
  $prepText = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot "prepare-copy.ps1"))
  $linksText = [System.IO.File]::ReadAllText($linksFile)
  $ruleForms = @("FileAttributes]::ReparsePoint", ".LinkType")
  Check "F109 i: the rule is in build\links.ps1 alone, which reads the reparse point attribute and the link type, and install.ps1 and prepare-copy.ps1 each dot-source it, call LinkKind and read neither themselves" (@($ruleForms | Where-Object { $linksText.Contains($_) }).Count -eq 2 -and @($ruleForms | Where-Object { $newInstall.Contains($_) -or $prepText.Contains($_) }).Count -eq 0 -and $newInstall.Contains('. (Join-Path $PSScriptRoot "links.ps1")') -and $prepText.Contains('. (Join-Path $repo "build\links.ps1")') -and $newInstall.Contains("LinkKind") -and $prepText.Contains("LinkKind")) ""
  $ip9 = InstallTrial "F109 j, the installed bundle with a file marked ReadOnly, a folder marked ReadOnly, a file marked Hidden and a file marked System, and a file held sharing read, write and delete" $newInstall "inplace-marked"
  $marked9 = @("Contents\v22\Federator.Addin.dll marked ReadOnly", "OldOnly marked ReadOnly", "OldOnly\note.txt marked Hidden", "PackageContents.xml marked System")
  Check "F109 j: the marks are refused before any copy is made, exit 2, one REFUSED line naming each marked file and folder with its mark, why, and what to do, no IN PLACE line, and the bundle reads as before by sha256 with every mark still on, with nothing beside it" ($ip9.Exit -eq 2 -and $ip9.Said.Count -eq 1 -and $ip9.Said[0] -match '^REFUSED: the installed add-in could not be moved aside, Access to the path .+ is denied, no process named Roamer runs, and it is not replaced in place, because Windows refuses to write over a file marked ReadOnly, Hidden or System and to remove a file or folder marked ReadOnly, measured on 2026-10-04, and 4 of its files and folders carry such a mark, .+\. Take each mark off with attrib -r -h -s and run this again\. The installed add-in is left whole\. Nothing was installed\.$' -and @($marked9 | Where-Object { $ip9.Said[0].Contains($_) }).Count -eq 4 -and (Lines $ip9 '^IN PLACE: ').Count -eq 0 -and $ip9.Before -eq $ip9.After -and $ip9.MarksBefore -eq $ip9.MarksAfter -and $ip9.MarksBefore.Contains("PackageContents.xml System") -and $ip9.Aside -eq 0) $(if ($ip9.Said.Count -gt 0) { $ip9.Said[0].Replace($h12, "<h12>") } else { "exit " + $ip9.Exit })
  $ip10 = InstallTrial "F109 k, a write fails mid way after Contents\v22\Federator.Addin.dll was taken out of the copy beside the bundle once the copy read back whole, the way a scanner may take a file, its copy of install.ps1 taking it" (InstallWith '        $inPlace = $true' '        $inPlace = $true; [System.IO.File]::Delete((Join-Path $aside "Contents\v22\Federator.Addin.dll")); Write-Host "HARNESS COPY: Contents\v22\Federator.Addin.dll taken out of the copy beside the bundle"') "inplace-read"
  Check "F109 k: the put back reads the copy against the bundle as it read before anything was written over, finds the file the copy lost, writes nothing back and says so, exit 1, the copy kept, and no line says the add-in was put back" ($ip10.Exit -eq 1 -and (Lines $ip10 '^HARNESS COPY: ').Count -eq 1 -and (Lines $ip10 '^The new add-in could not be copied in, Contents\\v22\\Federator\.Core\.dll could not be written, .+\. The add-in installed before could not be put back in place, Contents\\v22\\Federator\.Addin\.dll could not be read at .+\.replaced-\d{8}-\d{6}, .+, so nothing was written\. Its copy at .+\.replaced-\d{8}-\d{6} no longer reads as the bundle did before anything was written over, 1 differ by sha256, the first Contents\\v22\\Federator\.Addin\.dll, what is at .+ was not proved to be it, and the new one is at .+\.$').Count -eq 1 -and (Lines $ip10 'put back in place with').Count -eq 0 -and $ip10.Aside -eq 1) ("exit " + $ip10.Exit)
  BaderSame "H12c"

  # =====================================================================================
  O ""
  Case "==== H13, CLOSEOWN ===="
  $h13 = Join-Path $Work "h13"
  $rf = Join-Path $h13 "runs\99\item0"
  $tapp13 = Join-Path $h13 "appdata"
  New-Item -ItemType Directory -Path (Join-Path $rf "settings"), $tapp13 | Out-Null
  $k = $CU.CreateSubKey($tsub); $k.SetValue("A", "a1"); $k.Close()
  [System.IO.File]::WriteAllText((Join-Path $tapp13 "a.xml"), "a", $utf8)
  $bs13 = BackupSettings (Join-Path $rf "settings") $tsub $tapp13 (Join-Path $h13 "fedlogs")
  [pscustomobject]@{ RegSub = $tsub; RegRoot = $bs13.RegRoot; RegBefore = $bs13.RegBefore; NwAppData = $tapp13; FilesBefore = $bs13.FilesBefore; NotBacked = $bs13.NotBacked; AutoBefore = $bs13.AutoBefore } | Export-Clixml -LiteralPath (Join-Path $rf "settings\before.clixml")
  $k = $CU.OpenSubKey($tsub, $true); $k.SetValue("A", "a2"); $k.Close()
  $standinExe = Join-Path $standinBin "Roamer.exe"
  function Setup13($p, $ticks, $word, $verdict) {
    [System.IO.File]::WriteAllText((Join-Path $rf "record.txt"), "RUN RECORD, made by the harness`r`n" + $(if ($verdict) { "VERDICT: RAN`r`n" } else { "" }), $utf8)
    [System.IO.File]::WriteAllText((Join-Path $rf "mypid.txt"), [string]$p.Id + "`r`n" + [string]$ticks + "`r`n" + $word + "`r`n", $utf8)
  }
  $pa = StartStandin "-Embedding" "sleep|120" $null
  $ta = UtcTicks $pa.StartTime
  $refusals13 = @(
    @("mismatched ticks", $pa, ($ta + 1), "adopted", $false, $standinExe),
    @("a record that has a VERDICT", $pa, $ta, "adopted", $true, $standinExe),
    @("no mypid.txt reading adopted", $pa, $ta, "noted", $false, $standinExe),
    @("the path is not the install's Roamer.exe", $pa, $ta, "adopted", $false, (Join-Path $nw "Roamer.exe")))
  foreach ($c in $refusals13) {
    Setup13 $c[1] $c[2] $c[3] $c[4]
    $code13 = CloseOwn $rf $c[5]
    Check ("CloseOwn refuses " + $c[0] + ", and the stand-in still runs") ($code13 -eq 2 -and (Alive $pa $ta)) ("returned " + $code13)
  }
  $pb = StartStandin "sleep 120" $null $null
  Setup13 $pb (UtcTicks $pb.StartTime) "adopted" $false
  Check "CloseOwn refuses a command line with no -Embedding" ((CloseOwn $rf $standinExe) -eq 2 -and (Alive $pb (UtcTicks $pb.StartTime))) ""
  $pc = StartStandin "-Embedding" "sleep|120" (Join-Path $standinBin "Decoy.exe")
  Setup13 $pc (UtcTicks $pc.StartTime) "adopted" $false
  Check "CloseOwn refuses a process with another name" ((CloseOwn $rf $standinExe) -eq 2 -and (Alive $pc (UtcTicks $pc.StartTime))) ""
  Setup13 $pa $ta "adopted" $false
  [void](GuardsInPlace $pa)
  $r = RunReal ("-Mode CloseOwn -RunFolder `"" + $rf + "`"")
  $ref = @(Refused $r)
  Check "the real run.ps1 -Mode CloseOwn refuses the stand-in by its path, and it still runs" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'its path is' -and (Alive $pa $ta)) $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  Setup13 $pa $ta "adopted" $false
  $code13 = CloseOwn $rf $standinExe
  $rec13 = @([System.IO.File]::ReadAllLines((Join-Path $rf "record.txt")))
  foreach ($l in @($rec13 | Select-Object -Last 6)) { O ("    | " + $l) }
  Check "CloseOwn closes the stand-in that matches in every read, lists the compare and writes VERDICT CLOSED BY CLOSEOWN" ($code13 -eq 0 -and $pa.HasExited -and $rec13[$rec13.Count - 1] -eq "VERDICT: CLOSED BY CLOSEOWN" -and @($rec13 | Where-Object { $_ -match '22.0  A  old String "a1"  new String "a2"' }).Count -eq 1) ("returned " + $code13)
  Check "CloseOwn wrote nothing back, the test key still reads a2" ((RegRead $tsub).Read[("HKEY_CURRENT_USER\" + $tsub)]["A"].Data -eq "a2") ""
  $CU.DeleteSubKeyTree($tkey, $false)
  StopStandins
  BaderSame "H13"

  # =====================================================================================
  O ""
  Case "==== H14, THE CONSTRUCTOR DEADLINE in a child powershell ===="
  $h14 = Join-Path $Work "h14"
  New-Item -ItemType Directory -Path $h14 | Out-Null
  $p14 = StartStandin "-Embedding" "sleep|120" $null
  $t14 = UtcTicks $p14.StartTime
  $out14 = Join-Path $h14 "out.txt"; $unp14 = Join-Path $h14 "unproved.txt"; $wf14 = Join-Path $h14 "watch.txt"
  $child = Join-Path $h14 "deadline.ps1"
  $ct = @(
    '$ErrorActionPreference = "Stop"',
    ('. "' + $guardFile + '"'),
    ('$loopRoot = "' + $loopRoot + '"; $nw = "' + $nw + '"'),
    ('$guardText = [System.IO.File]::ReadAllText("' + $guardFile + '")'),
    '$wt = NewWinTypes',
    ('[System.IO.File]::WriteAllText("' + $out14 + '", ""); [System.IO.File]::WriteAllText("' + $wf14 + '", "")'),
    ('$all = @(Get-Process | ForEach-Object { $_.Id } | Where-Object { $_ -ne ' + $p14.Id + ' })'),
    ('$sync = WatchSync $all @{} ([DateTime]::Now) $loopRoot $nw 60 300 "' + $out14 + '" "' + $unp14 + '" "' + $wf14 + '" $guardText $wt.WinType $wt.ProcType'),
    '$sync.CallStartUtc = [DateTime]::UtcNow.AddSeconds(-61)',
    '$w = [PowerShell]::Create(); [void]$w.AddScript((WatchdogScript)).AddArgument($sync); $h = $w.BeginInvoke()',
    'Start-Sleep -Seconds 60',
    '[Console]::Out.WriteLine("THE MAIN THREAD WOKE, the deadline did not end this process")',
    'exit 0')
  [System.IO.File]::WriteAllText($child, ($ct -join "`r`n") + "`r`n", $utf8)
  $r = RunChild $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $child + "`"") $h14
  $o14 = @(Get-Content -LiteralPath $out14)
  foreach ($l in @($o14 | Where-Object { $_ -match 'DEADLINE|Roamer pid|written|TerminateProcess' })) { O ("    | " + $l) }
  Check "the deadline ends the child with exit code 3 through TerminateProcess" ($r.Exit -eq 3 -and $r.Out -notmatch 'THE MAIN THREAD WOKE') ("exit " + $r.Exit)
  Check "it writes its block" (@($o14 | Where-Object { $_ -match 'CONSTRUCTOR DEADLINE of 60 s passed' }).Count -ge 1) ""
  $u14 = @(Get-Content -LiteralPath $unp14 -ErrorAction SilentlyContinue | Where-Object { $_.StartsWith([string]$p14.Id + "`t") })
  Check "it writes the possible start down to the test file" ($u14.Count -eq 1) (($u14) -join " | ")
  Check "it closes nothing, the stand-in still runs" (Alive $p14 $t14) ""
  StopStandins
  BaderSame "H14"

  # =====================================================================================
  O ""
  Case "==== H17, THE RUN FLOW of a copy of run.ps1 whose constructor line is removed, with LOCALAPPDATA and APPDATA pointed at the harness's folders: checks 13, 14, 15 and 18 stop, and a run that reaches the removed line ===="
  $h17 = Join-Path $Work "h17"
  $rcRepo = Join-Path $h17 "copy"
  $fl = Join-Path $h17 "local"
  $fa = Join-Path $h17 "appdata"
  New-Item -ItemType Directory -Path (Join-Path $rcRepo "tools\loop") | Out-Null
  Copy-Item -LiteralPath $guardFile -Destination (Join-Path $rcRepo "tools\loop\nw-guard.ps1")
  $hookOn = Join-Path $h17 "hook-enabled.txt"; $hookWait = Join-Path $h17 "hook-waiting.txt"; $hookGo = Join-Path $h17 "hook-go.txt"
  $check18 = 'Say "---- check 18, the last read before the constructor ----"'
  $reps17 = @(
    @('try { $app = [Activator]::CreateInstance($napp) } catch { $err = $_.Exception }', '$err = New-Object System.Exception("HARNESS COPY: the constructor line is removed from this copy, and this line was reached")'),
    @('RegSub = "Software\Autodesk\Navisworks Manage\22.0"', 'RegSub = "Software\NwcFederatorLoopTest\22.0"'),
    @($check18, ('if (Test-Path -LiteralPath "' + $hookOn + '") { [System.IO.File]::WriteAllText("' + $hookWait + '", "waiting"); $hw = [Diagnostics.Stopwatch]::StartNew(); while (-not (Test-Path -LiteralPath "' + $hookGo + '") -and $hw.Elapsed.TotalSeconds -lt 120) { Start-Sleep -Milliseconds 100 } }' + "`n        " + $check18)))
  $c17 = [System.IO.File]::ReadAllText($runPs)
  foreach ($rp in $reps17) { $n17 = ([regex]::Matches($c17, [regex]::Escape($rp[0]))).Count; if ($n17 -ne 1) { throw ("a replacement of the run.ps1 copy is found " + $n17 + " times: " + $rp[0]) }; $c17 = $c17.Replace($rp[0], $rp[1]) }
  $runCopy = Join-Path $rcRepo "tools\loop\run.ps1"
  [System.IO.File]::WriteAllText($runCopy, $c17, $utf8)
  O "  every line of the copy that differs from run.ps1, < run.ps1, > the copy:"
  foreach ($d in @(Compare-Object @([System.IO.File]::ReadAllLines($runPs)) @([System.IO.File]::ReadAllLines($runCopy)))) { O ("    " + $d.SideIndicator + " " + $d.InputObject.Trim().Replace($h17, "<h17>")) }
  Check "the copy holds no call of the Automation constructor, so no run of it can start Navisworks" (-not $c17.Contains("CreateInstance") -and -not $c17.Contains("Activator")) ""
  # The fake folders: his logs, his settings with AutoSave, and an installed add-in whose stamp
  # is the installed build's, a copy of the installed DLL read off his bundle.
  New-Item -ItemType Directory -Path (Join-Path $fl "ParsonsNwcFederator\logs"), (Join-Path $fa "Autodesk\Navisworks Manage 2025\AutoSave"), (Join-Path $fa "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22") | Out-Null
  foreach ($i in 1..3) { [System.IO.File]::WriteAllText((Join-Path $fl ("ParsonsNwcFederator\logs\run-2026090" + $i + "-100000.log")), "a fake log " + $i, $utf8) }
  [System.IO.File]::WriteAllText((Join-Path $fl "ParsonsNwcFederator\logs\folders.txt"), "fake", $utf8)
  [System.IO.File]::WriteAllText((Join-Path $fa "Autodesk\Navisworks Manage 2025\a.xml"), "a fake setting", $utf8)
  [System.IO.File]::WriteAllText((Join-Path $fa "Autodesk\Navisworks Manage 2025\AutoSave\x.nwf"), "a fake autosave", $utf8)
  Copy-Item -LiteralPath (Join-Path $bundle "Contents\v22\Federator.Addin.dll") -Destination (Join-Path $fa "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22")
  $stamp17 = @([string][System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $fa "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22\Federator.Addin.dll")).ProductVersion -split '\s+')[1]
  $k = $CU.CreateSubKey($tsub); $k.SetValue("A", "a1"); $k.Close()
  $env17 = @{ LOCALAPPDATA = $fl; APPDATA = $fa }
  $echo17 = EndChild (StartChildEnv $ps "-NoProfile -Command [Console]::Out.Write(`$env:LOCALAPPDATA + '|' + `$env:APPDATA)" $h17 $env17) 60
  if ($echo17.Out.Trim() -ne ($fl + "|" + $fa)) { throw "a child does not read the harness's LOCALAPPDATA and APPDATA, so no copy of run.ps1 runs" }
  O ("  a child reads LOCALAPPDATA and APPDATA as the harness's folders, and the stamp is " + $stamp17)
  function RunCopy($set, $hookAction) {
    foreach ($f in @($hookOn, $hookWait, $hookGo)) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f } }
    if ($null -ne $hookAction) { [System.IO.File]::WriteAllText($hookOn, "on", $utf8) }
    $c = StartChildEnv $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $runCopy + "`" -Mode Run -Set " + $set + " -Item 0 -Stamp " + $stamp17) $rcRepo $env17
    if ($null -ne $hookAction) {
      $sw3 = [Diagnostics.Stopwatch]::StartNew()
      while (-not (Test-Path -LiteralPath $hookWait) -and -not $c.P.HasExited -and $sw3.Elapsed.TotalSeconds -lt 300) { Start-Sleep -Milliseconds 100 }
      O ("    the copy reached the hook before check 18: " + (Test-Path -LiteralPath $hookWait) + " after " + $sw3.Elapsed.TotalSeconds.ToString("0.0") + " s")
      & $hookAction
      [System.IO.File]::WriteAllText($hookGo, "go", $utf8)
    }
    $res = EndChild $c 600
    $recFile = Join-Path $fl ("NwcFederatorLoop\runs\" + $set + "\item0\record.txt")
    $rec = @(); if (Test-Path -LiteralPath $recFile) { $rec = @([System.IO.File]::ReadAllLines($recFile)) }
    foreach ($l in @($rec | Where-Object { $_ -match '^(STOP|VERDICT|NOT ADOPTED)|HARNESS COPY|keep awake|check 1[3-8]|logs-after|M5, what|BADER.S SETTINGS|AutoSave compare' })) { O ("    | " + $l.Replace($h17, "<h17>")) }
    return [pscustomobject]@{ Exit = $res.Exit; Rec = $rec; Out = $res.Out }
  }
  function Has($rec, $pattern) { return (@($rec | Where-Object { $_ -match $pattern }).Count -gt 0) }
  function At($rec, $pattern) { for ($i = 0; $i -lt $rec.Count; $i++) { if ($rec[$i] -match $pattern) { return $i } }; return -1 }

  O "  RC0, fix list 2 item 16: Check mode with the fake installed add-in's DLL held open with no sharing, and an installed.txt a loop install wrote"
  $inst0 = Join-Path $fl "NwcFederatorLoop\installs\20260930-000000"
  New-Item -ItemType Directory -Path $inst0 -Force | Out-Null
  [System.IO.File]::WriteAllText((Join-Path $inst0 "installed.txt"), "# relative path`tbytes`twritten UTC`tsha256`tattributes`r`nContents\v22\Federator.Addin.dll`t1`t2026-09-30T00:00:00.0000000Z`tAAAA`tArchive`r`n", $utf8)
  $dll0 = Join-Path $fa "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22\Federator.Addin.dll"
  $held0 = [System.IO.File]::Open($dll0, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
  try { $rc0 = EndChild (StartChildEnv $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + $runCopy + "`" -Mode Check -Set 90 -Item 0 -Stamp " + $stamp17) $rcRepo $env17) 300 } finally { $held0.Dispose() }
  $said0 = @($rc0.Out.Split("`n") | Where-Object { $_ -match 'newest installed.txt' } | ForEach-Object { $_.Trim().Replace($h17, "<h17>") })
  foreach ($l in $said0) { O ("    | " + $l) }
  Check "RC0, item 16: with the bundle unreadable, Check writes UNKNOWN for the match with the newest installed.txt, not False" ($said0.Count -eq 1 -and $said0[0] -match 'matches the installed bundle: UNKNOWN, the bundle could not be listed') ("exit " + $rc0.Exit)
  Remove-Item -LiteralPath (Join-Path $fl "NwcFederatorLoop\installs") -Recurse -Force

  O "  RC1, no Roamer and nothing held: the copy runs every check, calls its removed constructor line, and ends"
  $rc1 = RunCopy "91" $null
  Check "RC1: every check passes, the removed constructor line is reached, the start is NOT ADOPTED, exit 3" ($rc1.Exit -eq 3 -and (Has $rc1.Rec 'HARNESS COPY: the constructor line is removed') -and (Has $rc1.Rec '^VERDICT: NOT ADOPTED')) ("exit " + $rc1.Exit)
  Check "RC1: the keep awake request is made and let go with 0x80000003 on the same thread" ((Has $rc1.Rec 'SetThreadExecutionState\(ES_CONTINUOUS, ES_SYSTEM_REQUIRED, ES_DISPLAY_REQUIRED\) returned 0x8') -and (Has $rc1.Rec 'keep awake OFF returned 0x80000003 .*Same thread True, returned 0x80000003 True')) ""
  Check "RC1: M5 is read before the settings compare, and the AutoSave compare reads autosave-before.txt back" ((At $rc1.Rec 'M5, what changed outside the loop folder while the start ran') -ge 0 -and (At $rc1.Rec 'M5, what changed outside the loop folder while the start ran') -lt (At $rc1.Rec "BADER'S SETTINGS, compared") -and (Has $rc1.Rec 'the AutoSave compare reads autosave-before.txt back, 1 files')) ""
  Check "RC1: his fake logs folder reads the same after as before" (Has $rc1.Rec 'logs-after.txt equals logs-before.txt, name for name, size, write time, sha256 and attributes: True') ""
  Check "RC1: the evidence is written into the copy's own steps\runs" (Test-Path -LiteralPath (Join-Path $rcRepo "steps\runs\91\item0\record.txt")) ""

  O "  RC2, a file of his fake logs folder held open with no sharing: check 13 must stop"
  $held2 = [System.IO.File]::Open((Join-Path $fl "ParsonsNwcFederator\logs\run-20260901-100000.log"), [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
  try { $rc2 = RunCopy "92" $null } finally { $held2.Dispose() }
  Check "RC2: STOP before the start at check 13, exit 2, no keep awake request, nothing started" ($rc2.Exit -eq 2 -and (Has $rc2.Rec "^STOP before the start: .*so the tool's logs folder could not be copied") -and -not (Has $rc2.Rec 'check 17') -and -not (Has $rc2.Rec 'HARNESS COPY') -and (Has $rc2.Rec '^VERDICT: NOT RUN')) ("exit " + $rc2.Exit)
  O "  RC2b, fix list 2 item 7: the same set run again once nothing is held. The evidence of RC2's NOT RUN is moved aside, never emptied, and the run goes"
  $ev92 = Join-Path $rcRepo "steps\runs\92\item0"
  $ev92Before = $false
  if (Test-Path -LiteralPath (Join-Path $ev92 "record.txt")) { $ev92Before = (@([System.IO.File]::ReadAllLines((Join-Path $ev92 "record.txt")) | Where-Object { $_.StartsWith("VERDICT: NOT RUN") }).Count -eq 1) }
  Check "RC2 left its evidence in the copy's steps\runs\92\item0, with one VERDICT: NOT RUN line" $ev92Before ""
  $rc2b = RunCopy "92" $null
  $aside92 = @(Get-ChildItem -LiteralPath (Join-Path $rcRepo "steps\runs\92") -Directory | Where-Object { $_.Name -like "item0-aside-*" })
  $asideRec = $false
  if ($aside92.Count -eq 1 -and (Test-Path -LiteralPath (Join-Path $aside92[0].FullName "record.txt"))) { $asideRec = (@([System.IO.File]::ReadAllLines((Join-Path $aside92[0].FullName "record.txt")) | Where-Object { $_.StartsWith("VERDICT: NOT RUN") }).Count -eq 1) }
  $evRec = @(); if (Test-Path -LiteralPath (Join-Path $ev92 "record.txt")) { $evRec = @([System.IO.File]::ReadAllLines((Join-Path $ev92 "record.txt"))) }
  foreach ($l in @($rc2b.Rec | Where-Object { $_ -match 'moved aside' })) { O ("    | " + $l.Replace($h17, "<h17>")) }
  Check "RC2b: the run goes past check 11 to the removed constructor line, exit 3" ($rc2b.Exit -eq 3 -and (Has $rc2b.Rec 'HARNESS COPY: the constructor line is removed')) ("exit " + $rc2b.Exit)
  Check "RC2b: the record says the run folder of RC2 was moved aside" (Has $rc2b.Rec '^  the run folder from an earlier call was moved aside as item0-aside-\d{8}-\d{6}$') ""
  Check "RC2b: the record says the NOT RUN evidence was moved aside as item0-aside" (Has $rc2b.Rec '^  the evidence of an earlier call that was NOT RUN was moved aside as steps\\runs\\92\\item0-aside-\d{8}-\d{6}$') ""
  Check "RC2b: exactly one evidence folder is moved aside, and it holds RC2's record whole, with its VERDICT: NOT RUN line" ($aside92.Count -eq 1 -and $asideRec) ("folders aside " + $aside92.Count)
  Check "RC2b: the new evidence is written, and it carries both moved aside lines" ($evRec.Count -gt 0 -and @($evRec | Where-Object { $_ -match 'was moved aside as ' }).Count -eq 2) ([string]$evRec.Count + " lines")

  O "  RC3, a file of his fake AutoSave folder held open with no sharing: check 14 must stop"
  $held3 = [System.IO.File]::Open((Join-Path $fa "Autodesk\Navisworks Manage 2025\AutoSave\x.nwf"), [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
  try { $rc3 = RunCopy "93" $null } finally { $held3.Dispose() }
  Check "RC3: STOP before the start at check 14, exit 2, no keep awake request, nothing started" ($rc3.Exit -eq 2 -and (Has $rc3.Rec '^STOP before the start: .*so the AutoSave folder could not be copied') -and -not (Has $rc3.Rec 'check 17') -and -not (Has $rc3.Rec 'HARNESS COPY')) ("exit " + $rc3.Exit)

  O "  RC4, a folder of his fake settings that cannot be listed: check 15 must stop"
  $barrier = Join-Path $fa "Autodesk\Navisworks Manage 2025\locked"
  New-Item -ItemType Directory -Path $barrier | Out-Null
  [System.IO.File]::WriteAllText((Join-Path $barrier "inside.txt"), "inside", $utf8)
  $acl = Get-Acl -LiteralPath $barrier
  $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule([System.Security.Principal.WindowsIdentity]::GetCurrent().User, [System.Security.AccessControl.FileSystemRights]::ListDirectory, [System.Security.AccessControl.AccessControlType]::Deny)))
  Set-Acl -LiteralPath $barrier -AclObject $acl
  try { $rc4 = RunCopy "94" $null } finally {
    $acl = Get-Acl -LiteralPath $barrier
    foreach ($rule in @($acl.Access | Where-Object { $_.AccessControlType -eq "Deny" -and -not $_.IsInherited })) { [void]$acl.RemoveAccessRule($rule) }
    Set-Acl -LiteralPath $barrier -AclObject $acl
    Remove-Item -LiteralPath $barrier -Recurse -Force
  }
  Check "RC4: STOP before the start at check 15, the settings backup is not whole, exit 2, no keep awake request" ($rc4.Exit -eq 2 -and (Has $rc4.Rec '^STOP before the start: the settings backup is not whole') -and -not (Has $rc4.Rec 'check 17') -and -not (Has $rc4.Rec 'HARNESS COPY')) ("exit " + $rc4.Exit)

  O "  RC5, a stand-in Roamer started after the backups, while the copy waits at the hook before check 18"
  $rc5 = RunCopy "95" { [void](StartStandin "sleep 120" $null $null); O ("    started a stand-in Roamer, Roamers now " + @(Get-Process -Name Roamer -ErrorAction SilentlyContinue).Count) }
  StopStandins
  Check "RC5: check 18 stops before the constructor on the Roamer, exit 2, and the keep awake request made at check 17 is let go" ($rc5.Exit -eq 2 -and (Has $rc5.Rec '^STOP before the constructor: Navisworks is running') -and (Has $rc5.Rec 'keep awake OFF returned 0x80000003') -and -not (Has $rc5.Rec 'HARNESS COPY')) ("exit " + $rc5.Exit)

  O "  RC6, a start named in unproved-starts.txt that still runs, written while the copy waits at the hook, with no Roamer running"
  $fakeUnproved = Join-Path $fl "NwcFederatorLoop\probes\unproved-starts.txt"
  $rc6 = RunCopy "96" {
    $dec6 = StartStandin "sleep 120" $null (Join-Path $standinBin "Decoy.exe")
    New-Item -ItemType Directory -Force -Path (Split-Path $fakeUnproved -Parent) | Out-Null
    [System.IO.File]::WriteAllText($fakeUnproved, "# header`r`n" + $dec6.Id + "`t" + (UtcTicks $dec6.StartTime) + "`t-`t" + [DateTime]::UtcNow.ToString("o") + "`ttest`r`n", $utf8)
    O ("    wrote a line naming Decoy pid " + $dec6.Id + ", Roamers now " + @(Get-Process -Name Roamer -ErrorAction SilentlyContinue).Count)
  }
  StopStandins
  Check "RC6: check 18 stops before the constructor on the unproved start, exit 2, with no Roamer running" ($rc6.Exit -eq 2 -and (Has $rc6.Rec '^STOP before the constructor: a start this probe could not prove is still running') -and -not (Has $rc6.Rec 'HARNESS COPY')) ("exit " + $rc6.Exit)
  $CU.DeleteSubKeyTree($tkey, $false)
  O "  RC7, fix list 3 items 2 and 12: -Mode Install of the copy in a clean scratch git repository, whose build\install.ps1 is a stub that prints one REFUSED line naming a folder under APPDATA and exits 2"
  StopStandins
  $ir7 = Join-Path $h17 "irepo7"
  New-Item -ItemType Directory -Path (Join-Path $ir7 "tools\loop"), (Join-Path $ir7 "build") | Out-Null
  Copy-Item -LiteralPath $runCopy -Destination (Join-Path $ir7 "tools\loop\run.ps1")
  Copy-Item -LiteralPath $guardFile -Destination (Join-Path $ir7 "tools\loop\nw-guard.ps1")
  [System.IO.File]::WriteAllText((Join-Path $ir7 "build\install.ps1"), ('Write-Host ("REFUSED: " + (Join-Path $env:APPDATA "Autodesk\ApplicationPlugins") + " is a junction or a link, so the add-in is not installed through it. Nothing was installed. HARNESS STUB")' + "`r`n" + 'exit 2' + "`r`n"), $utf8)
  [void](RunChild "git" "init -q" $ir7)
  [void](RunChild "git" "-c core.autocrlf=false add -A" $ir7)
  [void](RunChild "git" "-c user.name=harness -c user.email=harness@example.invalid -c commit.gpgsign=false -c core.autocrlf=false commit -q -m scratch" $ir7)
  $head7 = (RunChild "git" "rev-parse --short=8 HEAD" $ir7).Out.Trim()
  $st7 = (RunChild "git" "-c core.autocrlf=false status --porcelain" $ir7).Out.Trim()
  O ("    the scratch repository's HEAD " + $head7 + ", git status prints " + $(if ($st7 -eq "") { "nothing" } else { $st7 }))
  $r7 = EndChild (StartChildEnv $ps ("-NoProfile -STA -ExecutionPolicy Bypass -File `"" + (Join-Path $ir7 "tools\loop\run.ps1") + "`" -Mode Install -Stamp " + $head7) $ir7 $env17) 600
  $irec7 = @(Get-ChildItem -LiteralPath (Join-Path $fl "NwcFederatorLoop\installs") -Recurse -File -Filter "record.txt" -ErrorAction SilentlyContinue | Where-Object { $_.Directory.Name.StartsWith($head7 + "-") })
  $rec7i = @(); if ($irec7.Count -eq 1) { $rec7i = @([System.IO.File]::ReadAllLines($irec7[0].FullName)) }
  foreach ($l in @($rec7i | Where-Object { $_ -match 'install\.ps1|REFUSED|VERDICT' })) { O ("    | " + $l.Replace($h17, "<h17>")) }
  Check "RC7, item 17: an install.ps1 refusal reaches Install as exit 2, and the record names install.ps1's REFUSED line" ($r7.Exit -eq 2 -and (Has $rec7i '^  build\\install\.ps1 said: REFUSED: .* is a junction or a link, so the add-in is not installed through it\. Nothing was installed\. HARNESS STUB$') -and (Has $rec7i '^REFUSED: build\\install\.ps1 refused, exit 2')) ("exit " + $r7.Exit + ", records " + $irec7.Count)
  Check "RC7, item 12: the REFUSED line reaches the record masked, %APPDATA% for the folder and no path of the APPDATA the call was given" ((Has $rec7i '^  build\\install\.ps1 said: REFUSED: %APPDATA%\\Autodesk\\ApplicationPlugins is a junction') -and -not (($rec7i -join "`n").Contains($fa))) ""
  BaderSame "H17"

  # =====================================================================================
  O ""
  Case "==== H18, FIX LIST 2: the end of a run, the call deadline, the verdict, the one reader and the bounded walk ===="
  $h18 = Join-Path $Work "h18"
  New-Item -ItemType Directory -Path $h18, (Join-Path $h18 "fedlogs") | Out-Null
  $rt = [System.IO.File]::ReadAllText($runPs)

  O "  item 1, the AutoSave listing cannot be read at the end, and the put back still happens"
  StopStandins
  SetTree
  $wkA = Join-Path $h18 "work"; New-Item -ItemType Directory -Path $wkA | Out-Null
  $bsA = BackupSettings $wkA $tsub $tapp (Join-Path $h18 "fedlogs")
  Mutate
  $lstA = Join-Path $h18 "autosave-before.txt"
  [System.IO.File]::WriteAllText($lstA, "# relative path`tbytes`twritten UTC`tsha256`tattributes`r`nx.nwf`t15`t2026-09-30T00:00:00.0000000Z`tAAAA`tArchive`r`n", $utf8)
  $fsA = [System.IO.File]::Open($lstA, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
  $oldThrew = $false
  try { try { [void](ReadListing $lstA "AutoSave\") } catch { $oldThrew = $true }; $asbA = AutoSaveBefore $lstA $bsA.AutoBefore } finally { $fsA.Dispose() }
  Check "item 1, before: the listing read throws while the file is held, which inside the put back's try stopped the put back" $oldThrew ""
  Check "item 1, after: AutoSaveBefore does not throw, says why, and hands the backup's own list" ($asbA.Why -match 'could not be read back' -and $null -ne $asbA.Map) $asbA.Why
  $spbA = SettingsPutBack $true @() $wkA $tsub $bsA.RegBefore $bsA.RegRoot $tapp $bsA.FilesBefore $bsA.NotBacked $asbA.Map $bsA.AppBackup
  Check "item 1, after: the put back still happens, the test key reads as before and nothing is left unwritten" ((TreeSame (RegRead $tsub).Read $bsA.RegBefore.Read) -and $spbA.NotWritten -eq 0) ("not written " + $spbA.NotWritten)
  $CU.DeleteSubKeyTree($tkey, $false)
  Check "item 1: run.ps1 reads the AutoSave listing outside the put back's try" ($rt.IndexOf('$asb = AutoSaveBefore') -gt 0 -and $rt.IndexOf('$asb = AutoSaveBefore') -lt $rt.IndexOf('$pr = PutBackReasons')) ""

  O "  fix list 3 item 5, a file whose name starts with # is a file, and a listing cut short is named"
  $lst5 = Join-Path $h18 "autosave-5.txt"
  [System.IO.File]::WriteAllText($lst5, ((ListingHeader) + "`r`n#notes.nwf`t10`t2026-09-30T00:00:00.0000000Z`tBBBB`tArchive`r`nx.nwf`t15`t2026-09-30T00:00:00.0000000Z`tAAAA`tArchive`r`n"), $utf8)
  $rows5 = ListingRows $lst5
  Check "item 5: ListingRows reads a file named #notes.nwf as a file, and skips only the header" ($rows5.Count -eq 2 -and $rows5[0].Rel -eq "#notes.nwf" -and $rows5[1].Rel -eq "x.nwf") ([string]$rows5.Count + " rows")
  $fb5 = @{ "AutoSave\#notes.nwf" = [pscustomobject]@{ Hash = "BBBB"; Length = 10; Write = [DateTime]::Now }; "AutoSave\x.nwf" = [pscustomobject]@{ Hash = "AAAA"; Length = 15; Write = [DateTime]::Now } }
  $a5 = AutoSaveBefore $lst5 $fb5
  Check "item 5: a whole listing that holds what the backup's list holds is read, and nothing is said" ($a5.Why -eq "" -and $a5.Map.Count -eq 2 -and $a5.Map.ContainsKey("AutoSave\#notes.nwf")) $a5.Why
  $cut5 = Join-Path $h18 "autosave-cut.txt"
  [System.IO.File]::WriteAllText($cut5, ((ListingHeader) + "`r`nx.nwf`t15`t2026-09-30T00:00:00.0000000Z`tAAAA`tArchive`r`n"), $utf8)
  $c5 = AutoSaveBefore $cut5 $fb5
  O ("    | " + $c5.Why)
  Check "item 5: a listing cut short is named by its count and by the file only the backup's list holds, and the backup's list is read instead" ($c5.Why -match "reads back 1 files and the backup's list holds 2" -and $c5.Why -match "in the listing only: none, in the backup's list only: AutoSave\\#notes\.nwf" -and $c5.Map.Count -eq 2) ""

  O "  item 2 and fix list 3 item 8, the close at the end: never a second close of a process already gone, and a close here of one the watchdog's close left running"
  $sy2 = NewTestSync $h18; $sy2.CallStartUtc = [DateTime]::UtcNow
  $p2 = StartStandin "-Embedding" "sleep|120" $null
  $ad2 = AdoptStart $null $obj $sy2 (Join-Path $h18 "mypid2.txt")
  $t2 = $sy2.MyTicks
  $sy2.Forced = "the adopted deadline of 5 s passed, the close of pid " + $p2.Id + " through the held handle threw"
  $ce2 = CloseAtEnd $sy2 $t2 10
  O ("    | " + $ce2.Text)
  Check "item 8: when the watchdog forced a close and the process still reads same after it, the end close closes it through the held handle and says so" ($ad2.Adopted -and $ce2.Forced -match 'closed through the held handle and gone' -and $ce2.Text -match '^the watchdog forced a close, .*still read same after it, so it was CLOSED here through the held handle' -and $p2.HasExited) ""
  StopStandins
  $sy2d = NewTestSync $h18; $sy2d.CallStartUtc = [DateTime]::UtcNow; $sy2d.CtorReturned = $true; $sy2d.AdoptedDeadline = 2
  $p2d = StartStandin "-Embedding" "sleep|120" $null
  $ad2d = AdoptStart $null $obj $sy2d (Join-Path $h18 "mypid2d.txt")
  $w2d = [PowerShell]::Create(); [void]$w2d.AddScript((WatchdogScript)).AddArgument($sy2d); $wh2d = $w2d.BeginInvoke()
  $sw2d = [Diagnostics.Stopwatch]::StartNew(); while ($sy2d.Forced -notmatch 'closed|still runs|threw' -and $sw2d.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 100 }
  $ce2d = CloseAtEnd $sy2d $sy2d.MyTicks 10
  $sy2d.Stop = $true; [void]$w2d.EndInvoke($wh2d); $w2d.Dispose()
  O ("    | " + $ce2d.Text)
  Check "item 2: when the watchdog's close ended the process, the end close says which close did it and closes nothing a second time" ($ad2d.Adopted -and $p2d.HasExited -and $sy2d.Forced -match 'closed through the held handle and gone' -and $ce2d.Forced -eq "" -and $ce2d.Text -match 'reads gone, so it is not closed a second time here') ""
  StopStandins
  $sy3 = NewTestSync $h18; $sy3.CallStartUtc = [DateTime]::UtcNow
  $p3 = StartStandin "-Embedding" "sleep|120" $null
  $ad3 = AdoptStart $null $obj $sy3 (Join-Path $h18 "mypid3.txt")
  $ce3 = CloseAtEnd $sy3 $sy3.MyTicks 10
  Check "item 2: with nothing forced, the end close closes a process still running through its held handle" ($ad3.Adopted -and $ce3.Forced -match 'closed through the held handle and gone' -and $p3.HasExited) $ce3.Text
  $sy4 = NewTestSync $h18; $sy4.CallStartUtc = [DateTime]::UtcNow; $sy4.CtorReturned = $true; $sy4.AdoptedDeadline = 2
  $p4b = StartStandin "-Embedding" "sleep|120" $null
  $ad4 = AdoptStart $null $obj $sy4 (Join-Path $h18 "mypid4.txt")
  $t4b = $sy4.MyTicks
  [System.Threading.Monitor]::Enter($sy4.CloseLock); try { $sy4.EndClose = $true } finally { [System.Threading.Monitor]::Exit($sy4.CloseLock) }
  $w4 = [PowerShell]::Create(); [void]$w4.AddScript((WatchdogScript)).AddArgument($sy4); $wh4 = $w4.BeginInvoke()
  Start-Sleep -Seconds 6
  $sy4.Stop = $true; [void]$w4.EndInvoke($wh4); $w4.Dispose()
  Check "item 2: once the end close has begun, the watchdog forces nothing at its deadline" ($ad4.Adopted -and $sy4.Forced -eq "" -and (Alive $p4b $t4b)) ("forced `"" + $sy4.Forced + "`"")
  StopStandins

  O "  item 3, the watchdog runs until just before the put back reasons are read, so a Roamer that starts and ends while M5 scans is in its record"
  $iM5b = $rt.IndexOf('NewerFiles $roots $sync.CallStartUtc'); $iStop = $rt.IndexOf('$watchEndedEarly = $whandle.IsCompleted'); $iWhy = $rt.IndexOf('$pr = PutBackReasons')
  Check "item 3: in run.ps1 M5 is read, then the watchdog stops, then the put back reasons are read" ($iM5b -gt 0 -and $iM5b -lt $iStop -and $iStop -lt $iWhy) ("M5 " + $iM5b + ", the watchdog's stop " + $iStop + ", the reasons " + $iWhy)
  $sy5 = NewTestSync $h18
  $w5 = [PowerShell]::Create(); [void]$w5.AddScript((WatchdogScript)).AddArgument($sy5); $wh5 = $w5.BeginInvoke()
  Start-Sleep -Seconds 2
  $p5 = StartStandin "sleep 5" $null $null
  $id5 = $p5.Id
  $sw5 = [Diagnostics.Stopwatch]::StartNew(); while (-not $p5.HasExited -and $sw5.Elapsed.TotalSeconds -lt 20) { Start-Sleep -Milliseconds 200 }
  Start-Sleep -Seconds 2
  $sy5.Stop = $true; [void]$w5.EndInvoke($wh5); $w5.Dispose()
  $pr5 = PutBackReasons $sy5 @() $null $false @{} 999999 1 ([DateTime]::UtcNow)
  Check "item 3: a stand-in that starts and ends while the watchdog runs is in its record, and the put back is refused on it" ($p5.HasExited -and (($pr5.Why -join " | ") -match ("Roamer " + $id5 + " started"))) (($pr5.Why) -join " | ")

  O "  item 5, a call into the adopted Navisworks that does not return is closed by the watchdog at its limit, here 3 s"
  $sy6 = NewTestSync $h18; $sy6.CallStartUtc = [DateTime]::UtcNow; $sy6.CtorReturned = $true
  $p6 = StartStandin "-Embedding" "sleep|120" $null
  $ad6 = AdoptStart $null $obj $sy6 (Join-Path $h18 "mypid6.txt")
  $sy6.CallLimit = 3; $sy6.CallSinceUtc = [DateTime]::UtcNow; $sy6.CallName = "Dispose"
  $w6 = [PowerShell]::Create(); [void]$w6.AddScript((WatchdogScript)).AddArgument($sy6); $wh6 = $w6.BeginInvoke()
  # The main thread clears the name as soon as the killed call returns, which can be while
  # the close is still in flight. So the name is cleared here the moment the close has begun.
  $sawBegun = ""
  $sw6 = [Diagnostics.Stopwatch]::StartNew()
  while ($sw6.Elapsed.TotalSeconds -lt 40) {
    $cf = [string]$sy6.CallForced
    if ($cf -match 'has begun$') { $sy6.CallName = ""; $sawBegun = $cf; break }
    if ($cf -ne "") { break }
    [System.Threading.Thread]::Sleep(0)
  }
  while ($sy6.CallForced -notmatch 'closed|still runs|threw' -and $sw6.Elapsed.TotalSeconds -lt 40) { Start-Sleep -Milliseconds 100 }
  $sy6.Stop = $true; [void]$w6.EndInvoke($wh6); $w6.Dispose()
  O ("    | when the name was cleared: " + $sawBegun)
  O ("    | " + $sy6.CallForced)
  Check "fix list 3 item 7: the name was cleared while the close was in flight, as the main thread clears it when the killed call returns" ($sawBegun -eq "Dispose did not return in 3 s, the close through the held handle has begun") $sawBegun
  Check "item 5: Dispose that has not returned in 3 s is closed through the held handle, and why is written" ($ad6.Adopted -and $sy6.CallForced -match '^Dispose did not return in 3 s, pid \d+ closed through the held handle and gone' -and $p6.HasExited) $sy6.CallForced
  $v6 = RunVerdict ([pscustomobject]@{ StopText = ""; Called = $true; Adopted = $true; NotPutBack = $false; RunOver = "HOLD"; Forced = ""; CallForced = [string]$sy6.CallForced; Fault = ""; MonitorFault = ""; FinallyFaults = @(); Dialogs = 0; ClosedHere = ""; HoldSeconds = 360; EndState = "gone" })
  Check "item 5: the verdict of that run is STOPPED, Dispose did not return, exit 4" ($v6.Code -eq 4 -and $v6.Text.StartsWith("STOPPED, Dispose did not return")) ($v6.Code.ToString() + ", " + $v6.Text)
  StopStandins

  O "  items 6 and 18, the verdict for a process that could not be read, and for one still running after every close path"
  function V2($over, $notPut, $end) { return (RunVerdict ([pscustomobject]@{ StopText = ""; Called = $true; Adopted = $true; NotPutBack = $notPut; RunOver = $over; Forced = ""; CallForced = ""; Fault = ""; MonitorFault = ""; FinallyFaults = @(); Dialogs = 0; ClosedHere = ""; HoldSeconds = 360; EndState = $end })) }
  $vr = @(
    @("the held process could not be read, UNKNOWN, not ended by itself", (V2 "UNKNOWN" $false "unreadable"), 1, "STOPPED, UNKNOWN whether"),
    @("the hold ended, and the process still reads same after every close path", (V2 "HOLD" $false "same"), 1, "STOPPED, the adopted Navisworks reads same after every close path"),
    @("not put back, with the process still running", (V2 "HOLD" $true "same"), 6, "STOPPED, something of Bader's was not put back, NOT PUT BACK, and the adopted Navisworks reads same"),
    @("the hold ended and the process is gone", (V2 "HOLD" $false "gone"), 0, "RAN, item 0"))
  foreach ($row in $vr) { Check ("RunVerdict, " + $row[0] + ": exit " + $row[2]) ($row[1].Code -eq $row[2] -and $row[1].Text.StartsWith($row[3])) ($row[1].Code.ToString() + ", " + $row[1].Text) }
  Check "item 6: the monitor keeps a process it cannot read as UNKNOWN, not GONE" ($rt.Contains('if ($held.State -eq "gone") { $sync.RunOver = "GONE" } else { $sync.RunOver = "UNKNOWN" }')) ""

  O "  items 13, 14 and 15, one list of modes, one reader of a listing, and no field set that nothing reads"
  Check "item 13: run.ps1 writes the mode names once, and builds the refusal from that list" (([regex]::Matches($rt, [regex]::Escape('"Check", "Install", "Run", "CloseOwn"'))).Count -eq 1 -and -not $rt.Contains("Check, Install, Run or CloseOwn")) ""
  Check "item 14: run.ps1 splits a listing's columns in one place, ListingRows" (([regex]::Matches($rt, [regex]::Escape('.Split("`t")'))).Count -eq 1 -and $rt.Contains('function ListingRows(')) ([string]([regex]::Matches($rt, [regex]::Escape('.Split("`t")'))).Count + " splits")
  Check "item 15: run.ps1 sets no Closed field that nothing reads" (-not $rt.Contains('$sync.Closed')) ""
  Check "fix list 3 item 10: the 120 s a call may take is written once, in run.ps1, and nw-guard.ps1's WatchSync sets no limit" (([regex]::Matches($rt, '= 120\b')).Count -eq 1 -and $rt.Contains('$CallLimitSeconds = 120') -and -not $guardText.Contains('CallLimit = 120') -and $guardText.Contains('$sync.CallLimit = 0')) ""
  Check "fix list 3 item 13: run.ps1 says FINALLY before the end close and M5, which are in the finally" ($rt.IndexOf('Say "==== FINALLY ===="') -gt 0 -and $rt.IndexOf('Say "==== FINALLY ===="') -lt $rt.IndexOf('$ce = CloseAtEnd') -and $rt.IndexOf('$ce = CloseAtEnd') -lt $rt.IndexOf('NewerFiles $roots $sync.CallStartUtc')) ""
  $rowsT = Join-Path $h18 "rows.txt"
  [System.IO.File]::WriteAllText($rowsT, (ListingHeader) + "`r`nsub\a.log`t12`t2026-09-30T08:00:00.0000000Z`tABCDEF`tArchive`r`n", $utf8)
  $rowsR = ListingRows $rowsT
  Check "item 14: ListingRows reads the relative path, bytes, write time, sha256 and the line itself" ($rowsR.Count -eq 1 -and $rowsR[0].Rel -eq "sub\a.log" -and $rowsR[0].Name -eq "a.log" -and $rowsR[0].Length -eq 12 -and $rowsR[0].Hash -eq "ABCDEF" -and $rowsR[0].WriteUtc.Hour -eq 8) ""

  O "  item 19, the walk of one window's children stops at its count and at its time"
  $ready19 = Join-Path $h18 "ready19.txt"
  $p19 = StartStandin "" ("hang|" + $ready19 + "|30|NwcFederatorLoop many children|10") $null
  $sw19 = [Diagnostics.Stopwatch]::StartNew(); while (-not (Test-Path -LiteralPath $ready19) -and $sw19.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 50 }
  $h19 = [IntPtr]::Zero
  foreach ($r in (WindowRecords $winType $procType ([uint32]$p19.Id) $true $false)) { if ($r.Caption -eq "NwcFederatorLoop many children") { $h19 = $r.Handle } }
  $c19 = ChildHandlesOf $winType $procType ([uint32]$p19.Id) $h19 3 ([Diagnostics.Stopwatch]::StartNew()) 2000
  $spent = [Diagnostics.Stopwatch]::StartNew(); Start-Sleep -Milliseconds 30
  $d19 = ChildHandlesOf $winType $procType ([uint32]$p19.Id) $h19 200 $spent 10
  Check "item 19: the walk stops at 3 children when 3 is its count, and says so" ($h19 -ne [IntPtr]::Zero -and $c19.Handles.Count -eq 3 -and $c19.Cut -eq "at 3 children") ([string]$c19.Handles.Count + " handles, " + $c19.Cut)
  Check "item 19: the walk stops at once when the time of the pass is spent, and says so" ($d19.Handles.Count -eq 0 -and $d19.Cut -eq "at 10 ms") ([string]$d19.Handles.Count + " handles, " + $d19.Cut)
  StopStandins
  BaderSame "H18"

  # =====================================================================================
  O ""
  Case "==== H15, CHECK MODE writes nothing ===="
  $r = RunReal "-Mode Check -Set 99 -Item 0 -Stamp be0b9b37"
  foreach ($l in @($r.Out.Split("`n") | Where-Object { $_ -match 'reads|matches|files,|there:|would refuse|none, for' } | Select-Object -First 20)) { O ("    | " + $l.TrimEnd()) }
  Check "Check with the installed stamp exits 0 or 2 with no fault" ($r.Exit -eq 0 -or $r.Exit -eq 2) ("exit " + $r.Exit)
  NoWrites "Check"
  BaderSame "H15"
} catch {
  $script:Fail++
  $script:Fails.Add("the harness stopped on a fault")
  O ("HARNESS FAULT: " + (Err $_.Exception) + " at line " + $_.InvocationInfo.ScriptLineNumber)
} finally {
  O ""
  O "==== CLEANUP ===="
  StopStandins
  $gone = @($script:Held | Where-Object { -not $_.HasExited }).Count
  O ("  stand-ins started " + $script:Held.Count + ", every one closed through its held handle, still running: " + $gone)
  [Microsoft.Win32.Registry]::CurrentUser.DeleteSubKeyTree("Software\NwcFederatorLoopTest", $false)
  O ("  HKCU\Software\NwcFederatorLoopTest there now: " + ($null -ne [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey("Software\NwcFederatorLoopTest")))
  $j = Join-Path $Work "h2\junction"
  if (Test-Path -LiteralPath $j) { [System.IO.Directory]::Delete($j, $false) }
  $lockedLeft = Join-Path $Work "h17\appdata\Autodesk\Navisworks Manage 2025\locked"
  if (Test-Path -LiteralPath $lockedLeft) {
    $acl = Get-Acl -LiteralPath $lockedLeft
    foreach ($rule in @($acl.Access | Where-Object { $_.AccessControlType -eq "Deny" -and -not $_.IsInherited })) { [void]$acl.RemoveAccessRule($rule) }
    Set-Acl -LiteralPath $lockedLeft -AclObject $acl
    O "  the deny rule left on the H17 barrier folder removed"
  }
  Start-Sleep -Seconds 1
  try { Remove-Item -LiteralPath $Work -Recurse -Force } catch { $script:Fail++; $script:Fails.Add("the cleanup could not remove the work folder"); O ("  the work folder could NOT be removed whole, " + (Err $_.Exception)) }
  O ("  " + $Work + " removed, there now: " + (Test-Path -LiteralPath $Work))
  O ("  Get-Process Roamer at the end: " + @(Get-Process -Name Roamer -ErrorAction SilentlyContinue).Count)
  O ""
  O ("==== RESULT: " + $script:Pass + " passed, " + $script:Fail + " failed ====")
  foreach ($f in $script:Fails) { O ("  failed: " + $f) }
  O ("finished " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + ", " + ([DateTime]::Now - $T0).TotalSeconds.ToString("0") + " s")
}
if ($script:Fail -gt 0) { exit 1 }
exit 0
