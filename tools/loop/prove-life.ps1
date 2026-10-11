param(
  [Parameter(Mandatory = $true)][string]$Work,
  [int]$LimitSeconds = 600
)
$ErrorActionPreference = "Stop"

# tools\loop\prove-life.ps1, F142. Proves the life check of run.ps1's monitor without Navisworks:
# LifeClass, LifeSpell, LifeWords, LifeCut and LifeStepOf on cases written out in full, each
# check that could pass on a wrong answer beside a case that breaks it, LifeRead on three
# stand-ins, the tool's WPF window answering, a window of the Navisworks main window's class and
# caption blocked, and a message box titled like the tool, which is never judged, its numbers held
# to what the process and the machine say, and LifePass and LifeSummary run in a runspace made the
# way run.ps1's MonitorScript makes the monitor's.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\loop\prove-life.ps1 -Work <an empty folder>
#
# THE STAND-IN IS COPIED UNDER ANOTHER NAME. tools\loop\StandIn builds Roamer.exe, and a process of
# that name starting while a loop run is going would read to that run's watchdog as another
# Navisworks, which stops its put back of Bader's settings. So the build goes into -Work and the
# exe is copied there as LifeStandIn.exe with its config, and only that copy is started. Each
# stand-in ends by itself, and the proof closes the ones it started through their process objects
# in its finally. -Work must not be there already, so no earlier proof's files are read as this
# one's. -LimitSeconds bounds the whole proof, the rule for every harness run, F138.
#
# Exit 0 when every check holds, 1 when one does not, 2 when it could not run.

$clock = [Diagnostics.Stopwatch]::StartNew()
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$right = 0; $wrong = 0
function Check($name, $ok, $why) {
  if ($ok) { $script:right++; Write-Output ("right  " + $name) } else { $script:wrong++; Write-Output ("WRONG  " + $name + $(if ($why) { ", " + $why } else { "" })) }
}
function Over { if ($clock.Elapsed.TotalSeconds -gt $LimitSeconds) { Write-Output ("TIME LIMIT: the proof passed " + $LimitSeconds + " s"); exit 2 } }
function Near($a, $b, $share) { return ($null -ne $a -and $null -ne $b -and [math]::Abs([double]$a - [double]$b) -le [math]::Max(16.0, [double]$b * $share)) }

if (Test-Path -LiteralPath $Work) { Write-Output ("REFUSED: -Work " + $Work + " is there already. Nothing was run"); exit 2 }
New-Item -ItemType Directory -Force -Path $Work | Out-Null

# The functions, read the way prove-run.ps1 and MonitorScript read them: nw-guard.ps1 holds
# functions only, and run.ps1's function definitions are taken off its syntax tree, so its main
# body never runs. Mask reads $loopRoot and $nw from its caller, as in the monitor.
$guard = Join-Path $PSScriptRoot "nw-guard.ps1"
$runPs = Join-Path $PSScriptRoot "run.ps1"
$tk = $null; $pe = $null
$runAst = [System.Management.Automation.Language.Parser]::ParseFile($runPs, [ref]$tk, [ref]$pe)
Check "run.ps1 parses with no error" ($pe.Count -eq 0) ($pe | Select-Object -First 1)
$tk2 = $null; $pe2 = $null
[void][System.Management.Automation.Language.Parser]::ParseFile($guard, [ref]$tk2, [ref]$pe2)
Check "nw-guard.ps1 parses with no error" ($pe2.Count -eq 0) ($pe2 | Select-Object -First 1)
$guardText = [System.IO.File]::ReadAllText($guard)
$runText = ((@($runAst.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] }) | ForEach-Object { $_.Extent.Text }) -join "`r`n")
. ([scriptblock]::Create($guardText))
. ([scriptblock]::Create($runText))
$loopRoot = $Work
$nw = "C:\Program Files\Autodesk\Navisworks Manage 2025"

Write-Output "==== LifeClass, the rate of Q83 is 20 s in 300 s, so 8 s over 120 s ===="
$table = @(
  @("free physical under the line is memory, whatever the processor", 120, 60.0, 1000, 30000, "memory"),
  @("free commit under the line is memory", 120, 60.0, 30000, 1000, "memory"),
  @("8 s over 120 s, the rate exactly, is busy", 120, 8.0, 30000, 30000, "busy"),
  @("7.9 s over 120 s, under the rate, with both memory numbers read, is stuck", 120, 7.9, 30000, 30000, "stuck"),
  @("the processor not read is UNKNOWN, never stuck", 120, $null, 30000, 30000, "UNKNOWN"),
  @("under the rate with free physical not read is UNKNOWN, never stuck", 120, 1.0, $null, 30000, "UNKNOWN"),
  @("under the rate with free commit not read is UNKNOWN, never stuck", 120, 1.0, 30000, $null, "UNKNOWN"),
  @("busy with memory not read is busy", 120, 60.0, $null, $null, "busy"),
  @("a stretch of no length is UNKNOWN", 0, 60.0, 30000, 30000, "UNKNOWN"),
  @("2048 MB free exactly is not under the line", 120, 1.0, 2048, 2048, "stuck"))
foreach ($row in $table) {
  $got = LifeClass $row[1] $row[2] $row[3] $row[4] 2048 300 20
  Check ("LifeClass, " + $row[0]) ($got -ceq $row[5]) ("returned " + $got)
}

Write-Output "==== LifeSpell, stretches counted between the readings that saw them ===="
$t0 = [DateTime]::new(2026, 10, 11, 12, 0, 0, [DateTimeKind]::Utc)
$tw = "tool's window"; $nwin = "Navisworks window"
$st = NewLifeState
Check "an answer not read starts nothing" ((LifeSpell $st $null $tw $t0 100 "step A" 120 "") -eq "" -and $null -eq $st.Since) ""
Check "an answer starts nothing" ((LifeSpell $st $true $tw $t0 100 "step A" 120 "") -eq "" -and $null -eq $st.Since) ""
Check "no answer starts a stretch, in the step and on the window read then" ((LifeSpell $st $false $tw $t0 100 "step B" 120 "") -eq "start" -and $st.Since -eq $t0 -and $st.SinceStep -eq "step B" -and $st.SinceWindow -eq $tw -and $st.Spells -eq 1) ""
Check "a stretch one reading saw is the 2 s that reading waited" ((LifeOpenLength $st) -eq 2) ("read " + (LifeOpenLength $st))
Check "an answer not read in a stretch neither ends nor moves it" ((LifeSpell $st $null $tw $t0.AddSeconds(30) 300 "step C" 120 "busy") -eq "" -and $st.LastNot -eq $t0) ""
Check "60 s in, under 120, writes no class" ((LifeSpell $st $false $tw $t0.AddSeconds(60) 200 "step C" 120 "busy") -eq "" -and $st.LastNot -eq $t0.AddSeconds(60)) ""
Check "120 s in, the class is written" ((LifeSpell $st $false $tw $t0.AddSeconds(120) 300 "step C" 120 "busy") -eq "class" -and $st.Classed -eq "busy") ""
Check "the same class again is not written twice" ((LifeSpell $st $false $tw $t0.AddSeconds(150) 300 "step C" 120 "busy") -eq "") ""
Check "a class that changes is written again" ((LifeSpell $st $false $tw $t0.AddSeconds(180) 300 "step C" 120 "stuck") -eq "class" -and $st.Classed -eq "stuck") ""
Check "an answer ends the stretch at its last reading not answering, not at the answer" ((LifeSpell $st $true $tw $t0.AddSeconds(210) 400 "step D" 120 "") -eq "end" -and $st.EndedLength -eq 182 -and $st.Longest -eq 182 -and $st.LongestStep -eq "step B" -and $null -eq $st.Since) ("ended " + $st.EndedLength + ", longest " + $st.Longest)
Check "a blip one reading caught ends at 2 s and never at the 30 s to the next reading" (((LifeSpell $st $false $tw $t0.AddSeconds(300) 500 "step E" 120 "") -eq "start") -and ((LifeSpell $st $true $tw $t0.AddSeconds(330) 500 "step F" 120 "") -eq "end") -and $st.EndedLength -eq 2 -and $st.Longest -eq 182 -and $st.Spells -eq 2) ("ended " + $st.EndedLength)
Check "a stretch shorter than the class wait ends with no class" ($st.Classed -eq "") ""
$st2 = NewLifeState
[void](LifeSpell $st2 $false $tw $t0 100 "step G" 120 "")
[void](LifeSpell $st2 $false $tw $t0.AddSeconds(30) 100 "step G" 120 "")
Check "the window judged changing ends the stretch where it was, said" ((LifeSpell $st2 $true $nwin $t0.AddSeconds(60) 100 "step H" 120 "") -eq "moved" -and $st2.EndedLength -eq 32 -and $st2.EndedWhy.Contains("where the stretch began on the tool's window") -and $null -eq $st2.Since) ("ended " + $st2.EndedLength + ", " + $st2.EndedWhy)
[void](LifeSpell $st2 $false $tw $t0.AddSeconds(90) 100 "step I" 120 "")
Check "a change to a window not answering starts a stretch of its own on it" ((LifeSpell $st2 $false $nwin $t0.AddSeconds(120) 100 "step J" 120 "") -eq "moved" -and $st2.SinceWindow -eq $nwin -and $st2.Since -eq $t0.AddSeconds(120) -and $st2.Spells -eq 3) ("window " + $st2.SinceWindow + ", spells " + $st2.Spells)
Check "a stretch still open is counted to its last reading, not to now" ((LifeOpenLength $st2) -eq 2) ("read " + (LifeOpenLength $st2))

Write-Output "==== LifeWords, LifeCut and LifeStepOf ===="
$r = [pscustomobject]@{ Answers = $true; Hung = $false; Window = "tool's window"; WindowWhy = ""; Private = 6134; Ws = 6082; PeakWs = 6132; FreePhys = 34767; TotalPhys = 65221; FreeCommit = 38678; MemoryWhy = "" }
$w = LifeWords $r 19.8 $null "STEP HARVEST"
Check "a window that answers reads answers" ($w.Contains("the tool's window answers,") -and $w.Contains("hung no")) $w
Check "the numbers are written with their units" ($w.Contains("processor 19.8 s") -and $w.Contains("private 6134 MB") -and $w.Contains("working set 6082 MB") -and $w.Contains("free physical 34767 MB of 65221 MB") -and $w.Contains("free commit 38678 MB")) $w
Check "the step is written last" ($w.EndsWith("the tool's last step: STEP HARVEST")) $w
$r.Answers = $false; $r.Hung = $true; $r.Private = $null; $r.MemoryWhy = "the process's memory is not read, its refresh failed this pass"
$w = LifeWords $r $null 45.4 "x"
Check "no answer reads NOT ANSWERING for at least its seconds" ($w.Contains("NOT ANSWERING for at least 45 s") -and $w.Contains("hung yes")) $w
Check "a number not read is UNKNOWN with why, never nought" ($w.Contains("processor UNKNOWN") -and $w.Contains("private UNKNOWN") -and $w.Contains("(the process's memory is not read, its refresh failed this pass)")) $w
$r.Answers = $null; $r.Hung = $null; $r.WindowWhy = "no window of the adopted Navisworks is up"
$w = LifeWords $r 1.0 $null "x"
Check "an answer not read is UNKNOWN with why" ($w.Contains("UNKNOWN, no window of the adopted Navisworks is up") -and $w.Contains("hung UNKNOWN")) $w
$tail = New-Object System.Collections.Generic.List[string]
Check "no line read yet says so" ((LifeCut (LifeStepOf $tail)) -eq "none read yet") ""
$tail.Add("14:44:14.699  +1053.628s  CLASH    created  BLD-AR-Walls-vs-BLD-AR-Floors")
Check "with no step line the newest line is taken" ((LifeCut (LifeStepOf $tail)).Contains("CLASH created BLD-AR-Walls-vs-BLD-AR-Floors")) (LifeCut (LifeStepOf $tail))
$tail.Insert(0, "14:27:25.590  +0044.550s  STEP     HARVEST       entered again")
$tail.Add("14:44:15.000  +1054.000s  ROWS     something")
Check "the newest STEP line is taken over a newer line of another kind" ((LifeCut (LifeStepOf $tail)).StartsWith("14:27:25.590 +0044.550s STEP HARVEST")) (LifeCut (LifeStepOf $tail))
$tail.Add("14:45:00.000  +1100.000s  PROGRESS HARVEST BLD-AR-Walls-vs-BLD-AR-Floors clash 120 of 1186")
Check "a PROGRESS line is taken over an older STEP line" ((LifeCut (LifeStepOf $tail)).Contains("PROGRESS HARVEST BLD-AR-Walls-vs-BLD-AR-Floors clash 120 of 1186")) (LifeCut (LifeStepOf $tail))
$tail.Add("14:46:00.000  +1160.000s  CALL     TestsImageForResult on x took 34.2s")
Check "a CALL line is taken too" ((LifeCut (LifeStepOf $tail)).Contains("CALL TestsImageForResult on x took 34.2s")) (LifeCut (LifeStepOf $tail))
$tail.Add("14:47:00.000  +1220.000s  STEP     " + ("x" * 300))
Check "a long line is cut at 160" ((LifeCut (LifeStepOf $tail)).Length -eq 160) ("length " + (LifeCut (LifeStepOf $tail)).Length)
$pathLine = "14:48:00.000  +1280.000s  STEP     WORKBOOK  " + ("y" * 140) + " C:\Users\someone else\Documents\secret.xlsx"
$masked = LifeCut (MaskLine $pathLine)
Check "a path past the cut is masked whole before the line is cut" (-not $masked.Contains("someone else") -and -not $masked.Contains("C:\Users")) $masked
Over

Write-Output "==== LifeRead on the stand-ins, copied as LifeStandIn.exe ===="
$bin = Join-Path $Work "standin-bin"
$b = Start-Process -FilePath "dotnet" -ArgumentList @("build", ('"' + (Join-Path $PSScriptRoot "StandIn\StandIn.csproj") + '"'), "-c", "Release", "-o", ('"' + $bin + '"'), "--nologo", "-v", "minimal") -WorkingDirectory $repo -NoNewWindow -PassThru -RedirectStandardOutput (Join-Path $Work "build.txt") -RedirectStandardError (Join-Path $Work "build-err.txt")
# The handle is read before the wait, or Windows PowerShell 5.1 gives a process started with
# -PassThru no exit code, measured on the first run of this proof on 2026-10-11.
[void]$b.Handle
if (-not $b.WaitForExit(300000)) { $b.Kill(); Write-Output "TIME LIMIT: the stand-in's build passed 300 s"; exit 2 }
if ($b.ExitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $bin "Roamer.exe"))) { Write-Output ("the stand-in did not build, exit " + $b.ExitCode + ", " + (Join-Path $Work "build.txt")); exit 2 }
Copy-Item -LiteralPath (Join-Path $bin "Roamer.exe") -Destination (Join-Path $bin "LifeStandIn.exe")
if (Test-Path -LiteralPath (Join-Path $bin "Roamer.exe.config")) { Copy-Item -LiteralPath (Join-Path $bin "Roamer.exe.config") -Destination (Join-Path $bin "LifeStandIn.exe.config") }
$wt = NewWinTypes
$exe = Join-Path $bin "LifeStandIn.exe"
$started = New-Object System.Collections.Generic.List[System.Diagnostics.Process]
function SyncOf($p) { return [pscustomobject]@{ WinType = $wt.WinType; ProcType = $wt.ProcType; MyPid = $p.Id; MyProc = $p; LifeSeconds = 30; LifeClassSeconds = 120; LifeLowMB = 2048; HangLimit = 300; HangCpuSeconds = 20; GuardText = $guardText; RunText = $runText; LoopRoot = $Work; Nw = $nw } }
try {
  $ready = Join-Path $Work "hang-ready.txt"
  $tool = Start-Process -FilePath $exe -ArgumentList @("window", ('"' + (Join-Path $Work "window-events.txt") + '"'), "abcdef12", "plain", "90", "-") -PassThru
  $started.Add($tool)
  $main = Start-Process -FilePath $exe -ArgumentList @("hang", ('"' + $ready + '"'), "60", '"1104-PAR-STAND-IN.nwf - Autodesk Navisworks Manage 2025"', "2") -PassThru
  $started.Add($main)
  $box = Start-Process -FilePath $exe -ArgumentList @("decoy", ('"' + (Join-Path $Work "decoy-log.txt") + '"'), '"Parsons NWC Federator"', "90") -PassThru
  $started.Add($box)
  $until = [DateTime]::UtcNow.AddSeconds(40)
  while (-not (Test-Path -LiteralPath $ready) -and [DateTime]::UtcNow -lt $until) { Start-Sleep -Milliseconds 200 }
  Check "the blocked stand-in wrote its ready file" (Test-Path -LiteralPath $ready) ""
  # Windows counts a window hung once its thread has read no message for 5 s, and the WPF window
  # needs its first seconds to come up.
  Start-Sleep -Seconds 8
  foreach ($p in $started) { $p.Refresh() }

  $a = LifeRead (SyncOf $tool) $true
  Check "LifeRead: the tool's WPF window is judged and answers, not hung" ($a.Answers -eq $true -and $a.Hung -eq $false -and $a.Window -eq "tool's window") ("answers " + $a.Answers + ", hung " + $a.Hung + ", " + $a.Window + ", " + $a.WindowWhy)
  $tool.Refresh()
  Check "LifeRead: its private bytes are the process's own, in MB" (Near $a.Private ($tool.PrivateMemorySize64 / 1MB) 0.1) ("read " + $a.Private + ", the process " + [math]::Round($tool.PrivateMemorySize64 / 1MB))
  Check "LifeRead: its working set and peak are the process's own, in MB" ((Near $a.Ws ($tool.WorkingSet64 / 1MB) 0.2) -and (Near $a.PeakWs ($tool.PeakWorkingSet64 / 1MB) 0.2)) ("read " + $a.Ws + " and " + $a.PeakWs)
  $ci = New-Object -ComObject "WbemScripting.SWbemLocator"
  $os = Get-CimInstance Win32_OperatingSystem
  Check "LifeRead: the machine's total is its own, in MB" (Near $a.TotalPhys ($os.TotalVisibleMemorySize / 1KB) 0.01) ("read " + $a.TotalPhys + ", the machine " + [math]::Round($os.TotalVisibleMemorySize / 1KB))
  Check "LifeRead: the machine's free physical and free commit are near its own, in MB" ((Near $a.FreePhys ($os.FreePhysicalMemory / 1KB) 0.15) -and (Near $a.FreeCommit ($os.FreeVirtualMemory / 1KB) 0.15)) ("read " + $a.FreePhys + " and " + $a.FreeCommit)

  $sw = [Diagnostics.Stopwatch]::StartNew()
  $h = LifeRead (SyncOf $main) $true
  Check "LifeRead: a window of the Navisworks main window's class and caption, blocked, is judged not answering and hung" ($h.Answers -eq $false -and $h.Hung -eq $true -and $h.Window -eq "Navisworks window") ("answers " + $h.Answers + ", hung " + $h.Hung + ", " + $h.Window)
  Check "LifeRead: a hung window costs no 2 s wait" ($sw.Elapsed.TotalSeconds -lt 2) ($sw.Elapsed.TotalSeconds.ToString("0.000") + " s")

  $d = LifeRead (SyncOf $box) $true
  Check "LifeRead: a message box titled like the tool is never judged, UNKNOWN with why" ($null -eq $d.Answers -and $d.WindowWhy -eq "no window of the adopted Navisworks is up") ("answers " + $d.Answers + ", " + $d.Window)
  $f = LifeRead (SyncOf $tool) $false
  Check "LifeRead: where the pass's refresh failed the process's memory is UNKNOWN with why" ($null -eq $f.Private -and $null -eq $f.Ws -and $f.MemoryWhy.Contains("refresh failed")) ("private " + $f.Private)
  Over

  Write-Output "==== LifePass and LifeSummary in a runspace made as MonitorScript makes the monitor's ===="
  $rs = [runspacefactory]::CreateRunspace()
  $rs.Open()
  try {
    $ps = [powershell]::Create()
    $ps.Runspace = $rs
    [void]$ps.AddScript({
      param($sync, $t0)
      . ([scriptblock]::Create($sync.GuardText)); . ([scriptblock]::Create($sync.RunText))
      $loopRoot = $sync.LoopRoot; $nw = $sync.Nw
      $life = NewLifeState
      $tail = New-Object System.Collections.Generic.List[string]
      $tail.Add("16:00:00.000  +0100.000s  STEP     HARVEST       entered again")
      $out = New-Object System.Collections.Generic.List[string]
      $sync.MyProc.Refresh(); $c = $sync.MyProc.TotalProcessorTime.Ticks
      foreach ($l in (LifePass $sync $life $tail $c $t0)) { $out.Add("1|" + $l) }
      foreach ($l in (LifePass $sync $life $tail $c $t0.AddSeconds(130))) { $out.Add("2|" + $l) }
      $out.Add("S|" + (LifeSummary $sync $life))
      $empty = NewLifeState
      $out.Add("E|" + (LifeSummary $sync $empty))
      return ,$out
    }).AddArgument((SyncOf $main)).AddArgument([DateTime]::UtcNow)
    $lines = @($ps.Invoke() | ForEach-Object { $_ })
    foreach ($e in $ps.Streams.Error) { Write-Output ("  runspace error: " + $e) }
    $ps.Dispose()
  } finally { $rs.Dispose() }
  $all = ($lines -join "`n")
  Write-Output ("  " + ($lines -join "`n  "))
  Check "LifePass runs in the runspace with no error and writes a LIFE line each reading" ((@($lines | Where-Object { $_ -match '^[12]\|LIFE  the Navisworks window NOT ANSWERING' })).Count -eq 2) ""
  Check "the second reading, 130 s on and not answering at both, writes a LIFE CLASS stuck line with its step" ($all -match '2\|LIFE CLASS stuck: the Navisworks window has not answered at any reading for 130 s' -and $all -match "step when it stopped answering: 16:00:00.000 \+0100.000s STEP HARVEST entered again") ""
  Check "the LIFE line counts the stretch from the first reading to the last, plus the wait" ($all -match '2\|LIFE  the Navisworks window NOT ANSWERING for at least 132 s') ""
  Check "LIFE SUMMARY names the longest stretch, its step and the peak working set" ($all -match 'S\|LIFE SUMMARY: 2 readings, 1 stretch\(es\) not answering, one still not answering at the last reading, for at least 132 s, the longest at least 132 s, in the tool''s step: 16:00:00.000 \+0100.000s STEP HARVEST entered again, peak working set \d+ MB as the process counts it') ""
  Check "LIFE SUMMARY with no reading says so" ($all -match 'E\|LIFE SUMMARY: no LIFE reading was taken before the monitor ended') ""
} finally {
  foreach ($p in $started) { try { if (-not $p.HasExited) { $p.Kill(); [void]$p.WaitForExit(10000) } } catch { } }
}
Over
Write-Output ("prove-life: " + $right + " right and " + $wrong + " WRONG, in " + $clock.Elapsed.TotalSeconds.ToString("0") + " s")
if ($wrong -gt 0) { exit 1 }
exit 0
