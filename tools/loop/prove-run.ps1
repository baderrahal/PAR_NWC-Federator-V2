param([string]$Work = "")
$ErrorActionPreference = "Stop"

# tools\loop\prove-run.ps1, F103 part 1. The proof of tools\loop\run.ps1 and
# tools\loop\nw-guard.ps1 with NO Navisworks started, the harness of the design's section
# proof without navisworks, H0 to H15 for the part 1 modes and M1 to M3, and since fix
# attempt 1 H12b, H16 and H17. Run it as
#
#   powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\loop\prove-run.ps1 -Work <folder>
#
# -Work is a folder under %LOCALAPPDATA%\NwcFederatorLoop, by default its test folder, and
# everything the harness writes goes there, bar the throwaway registry key
# HKCU\Software\NwcFederatorLoopTest. Both are removed at the end.
#
# WHY IT CANNOT START NAVISWORKS. It never calls the Automation constructor. It loads the
# function definitions of run.ps1 through the parser, so run.ps1's main flow never runs in
# it. The only runs of the real run.ps1 are Check, which reads only, and Run, Install and
# CloseOwn calls made to be refused: each is made only after the harness reads, just before
# it, a stand-in Roamer running and an installed stamp that is not the -Stamp passed, so
# check 6 and check 7 would each refuse it even if the check under test did not. H17 runs a
# COPY of run.ps1 under -Work whose constructor line is replaced by a line that throws, and
# checks the copy holds no call of the constructor before it runs it. H12b runs copies of
# build\install.ps1 with -SkipBuild, and only after a child started the same way reads
# APPDATA as a folder under -Work, so what they install goes there.
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
  foreach ($f in @(Get-ChildItem -LiteralPath $loopRoot -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\NwcFederatorLoop\\(turn\d+|wt-[^\\]+)(\\|$)' } | Sort-Object FullName)) { $s.Add("loop " + $f.FullName.Substring($loopRoot.Length) + " " + $(if ($f.PSIsContainer) { "folder" } else { [string]$f.Length }) + " " + $f.LastWriteTimeUtc.Ticks) }
  return ($s -join "`n")
}
O "  reading Bader's state at the start"
$baderAtStart = BaderState
O ("    " + $baderAtStart.Split("`n").Count + " lines: his logs, his AutoSave, the installed bundle, the 22.0 export and the loop folder outside the turn folders")
# The harness never runs beside a Navisworks it did not start. It refuses at its start, and
# after every case it reads again for a Roamer that is not one of its own running stand-ins,
# and stops there if it finds one, closing only its own stand-ins in its cleanup.
function ForeignRoamer {
  foreach ($p in @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)) {
    $id = $p.Id
    if (@($script:Held | Where-Object { $_.Id -eq $id -and -not $_.HasExited }).Count -eq 0) {
      $st = "its start time could not be read"
      try { $st = "started " + $p.StartTime.ToString("yyyy-MM-dd HH:mm:ss") } catch { $st = "its start time could not be read, " + (Err $_.Exception) }
      return ("Roamer pid " + $id + ", " + $st)
    }
  }
  return $null
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
function NoWrites($label) {
  $runs = Join-Path $loopRoot "runs"
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
  O "==== H0, STATIC READS of run.ps1 and nw-guard.ps1 ===="
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
  O "==== H1, THE MOVE: the probe -ReflectionOnly before and after, which starts nothing ===="
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
  O "==== H2, THE REFUSALS ===="
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
  O "==== H3, ADOPTION, AdoptStart fed stand-ins ===="
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
  O "==== H4, THE HELD HANDLE ===="
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
  O "==== H5, M1, the real RunLog.Start prune against 30 fabricated logs held open with no delete sharing ===="
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
  O "==== H6, THE HANG RULE ===="
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
    $sy.Dialogs = 0; $sy.ToolLog = $null; $sy.LogAmbiguous = $false; $sy.Closed = ""
    $sy.LogsFolder = Join-Path $dir "logs"; $sy.LogsBefore = @{}
    $sy.HangLimit = 20; $sy.PassSeconds = 2; $sy.HoldSeconds = $holdSeconds; $sy.BeatSeconds = 10
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
  Check "live 1: the monitor calls a hang, writes hang-tail.txt and closes the stand-in through its held handle" ($r6.Adopted -and $r6.Sync.RunOver -eq "HUNG" -and (Test-Path -LiteralPath (Join-Path $h6 "hang\hang-tail.txt")) -and $r6.Proc.HasExited) ($r6.Sync.RunOver + ", " + $r6.Sync.Closed)
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
  $syC.MonitorStop = $false; $syC.RunOver = ""; $syC.MonitorFault = ""; $syC.MonitorWriteFails = 0; $syC.Dialogs = 0; $syC.ToolLog = $null; $syC.LogAmbiguous = $false; $syC.Closed = ""
  $syC.LogsFolder = Join-Path $dirC "logs"; $syC.LogsBefore = @{}; $syC.HangLimit = 20; $syC.PassSeconds = 2; $syC.HoldSeconds = 60; $syC.BeatSeconds = 10
  $mpsC = [PowerShell]::Create(); [void]$mpsC.AddScript((MonitorScript)).AddArgument($syC); $mhC = $mpsC.BeginInvoke()
  $sw = [Diagnostics.Stopwatch]::StartNew(); while ($syC.RunOver -eq "" -and $sw.Elapsed.TotalSeconds -lt 60) { Start-Sleep -Milliseconds 250 }
  $syC.MonitorStop = $true; [void]$mpsC.EndInvoke($mhC); $mpsC.Dispose()
  $syC.Stop = $true; [void]$wpsC.EndInvoke($whC); $wpsC.Dispose()
  foreach ($l in @(Get-Content -LiteralPath (Join-Path $dirC "watch.txt") | Where-Object { $_ -match 'deadline' })) { O ("    | watch.txt: " + $l) }
  $vC = RunVerdict ([pscustomobject]@{ StopText = ""; Called = $true; Adopted = $adC.Adopted; NotPutBack = $false; RunOver = [string]$syC.RunOver; Forced = [string]$syC.Forced; Fault = ""; MonitorFault = [string]$syC.MonitorFault; FinallyFaults = @(); Dialogs = 0; ClosedHere = ""; HoldSeconds = 60 })
  Check "live 4: at the 5 s ceiling the watchdog closes the stand-in through its held handle, and the monitor ends the run as CEILING, not as ended by itself" ($adC.Adopted -and $syC.RunOver -eq "CEILING" -and $pC.HasExited -and $syC.Forced -match 'closed through the held handle and gone') ($syC.RunOver + ", " + $syC.Forced)
  Check "live 4: the verdict of that run is STOPPED, CEILING, exit 4" ($vC.Code -eq 4 -and $vC.Text.StartsWith("STOPPED, CEILING")) ($vC.Code.ToString() + ", " + $vC.Text)
  function V($over, $forced, $adopted, $notPut, $dialogs) { return (RunVerdict ([pscustomobject]@{ StopText = ""; Called = $true; Adopted = $adopted; NotPutBack = $notPut; RunOver = $over; Forced = $forced; Fault = ""; MonitorFault = ""; FinallyFaults = @(); Dialogs = $dialogs; ClosedHere = ""; HoldSeconds = 360 })) }
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
  O "==== H7, DIALOGS ===="
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
  $sy.MonitorStop = $false; $sy.RunOver = ""; $sy.MonitorFault = ""; $sy.MonitorWriteFails = 0; $sy.Dialogs = 0; $sy.ToolLog = $null; $sy.LogAmbiguous = $false; $sy.Closed = ""
  $sy.LogsFolder = Join-Path $h7 "logs"; $sy.LogsBefore = @{}; $sy.HangLimit = 20; $sy.PassSeconds = 2; $sy.HoldSeconds = 10; $sy.BeatSeconds = 10
  $mps = [PowerShell]::Create(); [void]$mps.AddScript((MonitorScript)).AddArgument($sy); $mh = $mps.BeginInvoke()
  $sw = [Diagnostics.Stopwatch]::StartNew(); while ($sy.RunOver -eq "" -and $sw.Elapsed.TotalSeconds -lt 60) { Start-Sleep -Milliseconds 500 }
  $sy.MonitorStop = $true; [void]$mps.EndInvoke($mh); $mps.Dispose()
  $rec7 = @(Get-Content -LiteralPath $sy.RecordFile)
  foreach ($l in @($rec7 | Where-Object { $_ -match 'DIALOG|MAIN|WINDOW|PROGRESS' })) { O ("    | " + $l) }
  Check "the message box is a DIALOG with its exact text" (@($rec7 | Where-Object { $_ -match 'DIALOG: class #32770' -and $_.Contains($msg) }).Count -eq 1) ""
  Check "the WinForms dialog is a DIALOG with its label's exact text" (@($rec7 | Where-Object { $_ -match 'DIALOG: class WindowsForms10' -and $_.Contains($lab) }).Count -eq 1) ""
  Check "the record holds no line about the decoy started by hand" (@($rec7 | Where-Object { $_ -match ('pid ' + $d7.Id + '\b') -or $_.Contains("the decoy's label") }).Count -eq 0) ("decoy pid " + $d7.Id)
  $dl7 = @(Get-Content -LiteralPath $decoy7 -ErrorAction SilentlyContinue)
  Check "the decoy's log shows no message sent to it from another thread or process" ($dl7.Count -eq 0) (($dl7) -join " | ")
  StopStandins

  O "  AN OWNED WINDOW titled as the Navisworks main window: a WinForms window with no owner, and one owned by it with a label, both titled Untitled - Autodesk Navisworks Manage 2025"
  $mainCap = "Untitled - Autodesk Navisworks Manage 2025"
  $ownLab = "NwcFederatorLoop owned message 5521"
  $po = StartStandin "" ("owned|" + $mainCap + "|" + $ownLab + "|40") $null
  Start-Sleep -Seconds 2
  $mains = 0; $dialogs7 = 0; $ownedText = ""; $mainRead = $false
  foreach ($r in (WindowRecords $winType $procType ([uint32]$po.Id) $true $true)) {
    $k = WindowKind $r.Class $r.Caption $r.OwnerHandle
    O ("    | " + $k + ": class " + $r.Class + ", caption `"" + $r.Caption + "`", owner " + $r.Owner + ", text " + $(if ($null -ne $r.Texts) { $r.Texts -join " " } else { "not read" }))
    if ($k -eq "MAIN") { $mains++; if ($null -ne $r.Texts) { $mainRead = $true } }
    if ($k -eq "DIALOG") { $dialogs7++; $ownedText = ($r.Texts -join " ") }
  }
  Check "the window with no owner is MAIN and its children are not read, the owned one is a DIALOG with its label's text" ($mains -eq 1 -and -not $mainRead -and $dialogs7 -eq 1 -and $ownedText.Contains($ownLab)) ("MAIN " + $mains + ", DIALOG " + $dialogs7)
  $wkt = @(
    @("WindowsForms10.Window.8.app.0.x", $mainCap, [IntPtr]::Zero, "MAIN"),
    @("WindowsForms10.Window.8.app.0.x", $mainCap, [IntPtr]123, "DIALOG"),
    @("#32770", "Autodesk Navisworks Manage 2025", [IntPtr]::Zero, "DIALOG"),
    @("HwndWrapper[Roamer.exe;ProgressDialog;1]", "Working...", [IntPtr]::Zero, "PROGRESS"),
    @("HwndWrapper[Roamer.exe;;2]", "Parsons NWC Federator 1.0", [IntPtr]::Zero, "WINDOW"))
  foreach ($row in $wkt) { $k = WindowKind $row[0] $row[1] $row[2]; Check ("WindowKind of class " + $row[0] + ", owner " + $row[2] + ": " + $row[3]) ($k -eq $row[3]) $k }
  StopStandins
  BaderSame "H7"

  # =====================================================================================
  O ""
  O "==== H16, A WINDOW WHOSE THREAD IS BLOCKED: GetWindowText, and the 2 s the child reads may spend ===="
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
  O "==== H10, SETTINGS against HKCU\Software\NwcFederatorLoopTest\22.0 and a test folder ===="
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
  O "==== H11, M3, keep awake in a powershell -File process ===="
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
  O "==== H12, INSTALL AND THE STAMP, M2. install.ps1 never runs here ===="
  $h12 = Join-Path $Work "h12"
  New-Item -ItemType Directory -Path $h12 | Out-Null
  $bundleBefore = @(Get-ChildItem -LiteralPath $bundle -Recurse -File | ForEach-Object { $_.FullName + " " + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
  $s12 = StartStandin "sleep 120" $null $null
  O ("  " + (GuardsInPlace $s12))
  $r = RunReal "-Mode Install -Stamp 00000000"
  $ref = @(Refused $r)
  Check "Install refuses while a stand-in Roamer runs, exit 2" ($r.Exit -eq 2 -and $ref.Count -ge 1 -and $ref[0] -match 'Navisworks is running') $(if ($ref.Count -gt 0) { $ref[0] } else { $r.Out.Trim() })
  $bundleAfter = @(Get-ChildItem -LiteralPath $bundle -Recurse -File | ForEach-Object { $_.FullName + " " + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
  Check "the installed bundle's sha256 listing is the same before and after" ((($bundleBefore) -join "|") -eq (($bundleAfter) -join "|")) ([string]$bundleAfter.Count + " files")
  Check "no installs folder was made" (-not (Test-Path -LiteralPath (Join-Path $loopRoot "installs"))) ""
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
  O "==== H12b, build\install.ps1 REFUSES WHILE NAVISWORKS RUNS, a copy of it run with -SkipBuild against a fake APPDATA ===="
  $ir = Join-Path $h12 "irepo"
  $fakeApp = Join-Path $h12 "appdata"
  $fakeBundle = Join-Path $fakeApp "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"
  $buildOut = Join-Path $repo "src\Federator.Addin\bin\Release\net48"
  New-Item -ItemType Directory -Path (Join-Path $ir "build"), (Join-Path $ir "bundle\ParsonsNwcFederator.bundle"), (Join-Path $ir "src\Federator.Addin\bin\Release\net48") | Out-Null
  Copy-Item -LiteralPath (Join-Path $repo "bundle\ParsonsNwcFederator.bundle\PackageContents.xml") -Destination (Join-Path $ir "bundle\ParsonsNwcFederator.bundle")
  foreach ($f in @(Get-ChildItem -LiteralPath $buildOut -File -Filter "*.dll")) { Copy-Item -LiteralPath $f.FullName -Destination (Join-Path $ir "src\Federator.Addin\bin\Release\net48") }
  $newInstall = [System.IO.File]::ReadAllText((Join-Path $repo "build\install.ps1"))
  $oldInstall = (RunChild "git" "show 0eb4ede:build/install.ps1" $repo).Out
  $targetLine = '$target     = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle"'
  Check "both copies of install.ps1 install into `$env:APPDATA, which the harness points at its own folder" ($newInstall.Contains($targetLine) -and $oldInstall.Contains($targetLine)) ""
  $echo = EndChild (StartChildEnv $ps "-NoProfile -Command [Console]::Out.Write(`$env:APPDATA)" $h12 @{ APPDATA = $fakeApp }) 60
  Check "a child started the same way reads APPDATA as the harness's folder" ($echo.Out.Trim() -eq $fakeApp) (Mask $echo.Out.Trim())
  function InstallTrial($label, $text, [bool]$withRoamer) {
    StopStandins
    if (Test-Path -LiteralPath $fakeBundle) { Remove-Item -LiteralPath $fakeBundle -Recurse -Force }
    New-Item -ItemType Directory -Path $fakeBundle | Out-Null
    [System.IO.File]::WriteAllText((Join-Path $fakeBundle "marker.txt"), "the bundle installed before", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $ir "build\install.ps1"), $text, $utf8)
    if ($withRoamer) { [void](StartStandin "sleep 120" $null $null) }
    $roam = @(Get-Process -Name Roamer -ErrorAction SilentlyContinue).Count
    if ($echo.Out.Trim() -ne $fakeApp) { throw "the APPDATA of a child is not the harness's folder, so no install.ps1 copy runs" }
    $res = EndChild (StartChildEnv $ps ("-NoProfile -ExecutionPolicy Bypass -File `"" + (Join-Path $ir "build\install.ps1") + "`" -SkipBuild") $ir @{ APPDATA = $fakeApp }) 180
    $marker = Test-Path -LiteralPath (Join-Path $fakeBundle "marker.txt")
    $addin = Test-Path -LiteralPath (Join-Path $fakeBundle "Contents\v22\Federator.Addin.dll")
    $nav = @($res.Out.Split("`n") | Where-Object { $_ -match 'Navisworks is running' } | ForEach-Object { $_.Trim() })
    O ("    " + $label + ": Roamers running " + $roam + ", exit " + $res.Exit + ", the old bundle's marker still there " + $marker + ", the new add-in there " + $addin + $(if ($nav.Count -gt 0) { ", said: " + $nav[0] } else { "" }))
    StopStandins
    return [pscustomobject]@{ Exit = $res.Exit; Marker = $marker; Addin = $addin; Said = $nav; Lines = @($res.Out.Split("`n") | Where-Object { $_.Trim() -ne "" }).Count }
  }
  $t1 = InstallTrial "before, install.ps1 at 0eb4ede with a stand-in Roamer running" $oldInstall $true
  Check "before: install.ps1 at 0eb4ede replaces the bundle while a Roamer runs, the fault the breaker found" ($t1.Exit -eq 0 -and -not $t1.Marker -and $t1.Addin) ("exit " + $t1.Exit)
  $t2 = InstallTrial "after, install.ps1 now with a stand-in Roamer running" $newInstall $true
  Check "after: install.ps1 refuses with one line saying Navisworks is running and must be closed first, exit 1, and the bundle is left as it was" ($t2.Exit -eq 1 -and $t2.Marker -and -not $t2.Addin -and $t2.Said.Count -eq 1 -and $t2.Said[0] -match 'must be closed first') ("exit " + $t2.Exit)
  $t3 = InstallTrial "control, install.ps1 now with no Roamer running" $newInstall $false
  Check "control: with no Roamer running install.ps1 installs as before" ($t3.Exit -eq 0 -and -not $t3.Marker -and $t3.Addin) ("exit " + $t3.Exit)
  BaderSame "H12b"

  # =====================================================================================
  O ""
  O "==== H13, CLOSEOWN ===="
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
  O "==== H14, THE CONSTRUCTOR DEADLINE in a child powershell ===="
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
  O "==== H17, THE RUN FLOW of a copy of run.ps1 whose constructor line is removed, with LOCALAPPDATA and APPDATA pointed at the harness's folders: checks 13, 14, 15 and 18 stop, and a run that reaches the removed line ===="
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

  O "  RC1, no Roamer and nothing held: the copy runs every check, calls its removed constructor line, and ends"
  $rc1 = RunCopy "91" $null
  Check "RC1: every check passes, the removed constructor line is reached, the start is NOT ADOPTED, exit 3" ($rc1.Exit -eq 3 -and (Has $rc1.Rec 'HARNESS COPY: the constructor line is removed') -and (Has $rc1.Rec '^VERDICT: NOT ADOPTED')) ("exit " + $rc1.Exit)
  Check "RC1: the keep awake request is made and let go with 0x80000003 on the same thread" ((Has $rc1.Rec 'SetThreadExecutionState\(ES_CONTINUOUS, ES_SYSTEM_REQUIRED, ES_DISPLAY_REQUIRED\) returned 0x8') -and (Has $rc1.Rec 'keep awake OFF returned 0x80000003 .*Same thread True, returned 0x80000003 True')) ""
  Check "RC1: M5 is read before the settings compare, and the AutoSave compare reads autosave-before.txt back" ((At $rc1.Rec 'M5, what the start wrote') -ge 0 -and (At $rc1.Rec 'M5, what the start wrote') -lt (At $rc1.Rec "BADER'S SETTINGS, compared") -and (Has $rc1.Rec 'the AutoSave compare reads autosave-before.txt back, 1 files')) ""
  Check "RC1: his fake logs folder reads the same after as before" (Has $rc1.Rec 'logs-after.txt equals logs-before.txt, name for name, size, write time, sha256 and attributes: True') ""
  Check "RC1: the evidence is written into the copy's own steps\runs" (Test-Path -LiteralPath (Join-Path $rcRepo "steps\runs\91\item0\record.txt")) ""

  O "  RC2, a file of his fake logs folder held open with no sharing: check 13 must stop"
  $held2 = [System.IO.File]::Open((Join-Path $fl "ParsonsNwcFederator\logs\run-20260901-100000.log"), [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
  try { $rc2 = RunCopy "92" $null } finally { $held2.Dispose() }
  Check "RC2: STOP before the start at check 13, exit 2, no keep awake request, nothing started" ($rc2.Exit -eq 2 -and (Has $rc2.Rec "^STOP before the start: .*so the tool's logs folder could not be copied") -and -not (Has $rc2.Rec 'check 17') -and -not (Has $rc2.Rec 'HARNESS COPY') -and (Has $rc2.Rec '^VERDICT: NOT RUN')) ("exit " + $rc2.Exit)

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
  BaderSame "H17"

  # =====================================================================================
  O ""
  O "==== H15, CHECK MODE writes nothing ===="
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
