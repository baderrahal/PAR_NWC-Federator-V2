param(
  [Parameter(Mandatory = $true)][string]$Work,
  [int]$LimitSeconds = 600
)
$ErrorActionPreference = "Stop"

# tools\loop\prove-life.ps1, F142. Proves the life check of run.ps1's monitor without Navisworks:
# LifeClass, LifeSpell, LifeWords and LifeStepOf on cases written out in full, each check that
# could pass on a wrong answer beside a case that breaks it, and LifeRead on two stand-ins, a
# window that answers and a window whose thread is blocked.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\loop\prove-life.ps1 -Work <an empty folder>
#
# THE STAND-IN IS COPIED UNDER ANOTHER NAME. tools\loop\StandIn builds Roamer.exe, and a process of
# that name starting while a loop run is going would read to that run's watchdog as another
# Navisworks, which stops its put back of Bader's settings. So the build goes into -Work and the
# exe is copied there as LifeStandIn.exe with its config, and only that copy is started. Each
# stand-in ends by itself, and the proof closes the two it started through their process objects
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

if (Test-Path -LiteralPath $Work) { Write-Output ("REFUSED: -Work " + $Work + " is there already. Nothing was run"); exit 2 }
New-Item -ItemType Directory -Force -Path $Work | Out-Null

# The functions, read the way prove-run.ps1 reads them: nw-guard.ps1 holds functions only and is
# dot-sourced, and run.ps1's function definitions are taken off its syntax tree, so its main body
# never runs.
$guard = Join-Path $PSScriptRoot "nw-guard.ps1"
$runPs = Join-Path $PSScriptRoot "run.ps1"
$tk = $null; $pe = $null
$runAst = [System.Management.Automation.Language.Parser]::ParseFile($runPs, [ref]$tk, [ref]$pe)
Check "run.ps1 parses with no error" ($pe.Count -eq 0) ($pe | Select-Object -First 1)
$tk2 = $null; $pe2 = $null
[void][System.Management.Automation.Language.Parser]::ParseFile($guard, [ref]$tk2, [ref]$pe2)
Check "nw-guard.ps1 parses with no error" ($pe2.Count -eq 0) ($pe2 | Select-Object -First 1)
. $guard
. ([scriptblock]::Create(((@($runAst.EndBlock.Statements | Where-Object { $_ -is [System.Management.Automation.Language.FunctionDefinitionAst] }) | ForEach-Object { $_.Extent.Text }) -join "`r`n")))

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

Write-Output "==== LifeSpell, one stretch from start to end ===="
$t0 = [DateTime]::new(2026, 10, 11, 12, 0, 0, [DateTimeKind]::Utc)
$st = NewLifeState
Check "an answer not read starts nothing" ((LifeSpell $st $null $t0 100 "step A" 120 "") -eq "" -and $null -eq $st.Since) ""
Check "an answer starts nothing" ((LifeSpell $st $true $t0 100 "step A" 120 "") -eq "" -and $null -eq $st.Since) ""
Check "no answer starts a stretch, in the step read then" ((LifeSpell $st $false $t0 100 "step B" 120 "") -eq "start" -and $st.Since -eq $t0 -and $st.SinceStep -eq "step B" -and $st.Spells -eq 1) ""
Check "an answer not read in a stretch neither ends nor classes it" ((LifeSpell $st $null $t0.AddSeconds(200) 300 "step C" 120 "busy") -eq "" -and $st.Since -eq $t0) ""
Check "60 s in, under 120, writes no class" ((LifeSpell $st $false $t0.AddSeconds(60) 200 "step C" 120 "busy") -eq "") ""
Check "120 s in, the class is written" ((LifeSpell $st $false $t0.AddSeconds(120) 300 "step C" 120 "busy") -eq "class" -and $st.Classed -eq "busy") ""
Check "the same class again is not written twice" ((LifeSpell $st $false $t0.AddSeconds(150) 300 "step C" 120 "busy") -eq "") ""
Check "a class that changes is written again" ((LifeSpell $st $false $t0.AddSeconds(180) 300 "step C" 120 "stuck") -eq "class" -and $st.Classed -eq "stuck") ""
Check "an answer ends the stretch, the longest taken with the step it began in" ((LifeSpell $st $true $t0.AddSeconds(210) 400 "step D" 120 "") -eq "end" -and $st.Longest -eq 210 -and $st.LongestStep -eq "step B" -and $null -eq $st.Since) ""
Check "a shorter second stretch leaves the longest as it was" (((LifeSpell $st $false $t0.AddSeconds(300) 500 "step E" 120 "") -eq "start") -and ((LifeSpell $st $true $t0.AddSeconds(330) 500 "step F" 120 "") -eq "end") -and $st.Longest -eq 210 -and $st.LongestStep -eq "step B" -and $st.Spells -eq 2) ""
Check "a stretch shorter than the class wait ends with no class" ($st.Classed -eq "") ""

Write-Output "==== LifeWords and LifeStepOf ===="
$r = [pscustomobject]@{ Answers = $true; Hung = $false; Window = "tool's window"; WindowWhy = ""; Private = 6134; Ws = 6082; FreePhys = 34767; TotalPhys = 65221; FreeCommit = 38678 }
$w = LifeWords $r 19.8 $null "STEP HARVEST"
Check "a window that answers reads answers" ($w.Contains("the tool's window answers,") -and $w.Contains("hung no")) $w
Check "the numbers are written with their units" ($w.Contains("processor 19.8 s") -and $w.Contains("private 6134 MB") -and $w.Contains("working set 6082 MB") -and $w.Contains("free physical 34767 MB of 65221 MB") -and $w.Contains("free commit 38678 MB")) $w
Check "the step is written last" ($w.EndsWith("the tool's last step: STEP HARVEST")) $w
$r.Answers = $false; $r.Hung = $true; $r.Private = $null
$w = LifeWords $r $null 45.4 "x"
Check "no answer reads NOT ANSWERING with its seconds" ($w.Contains("NOT ANSWERING for 45 s") -and $w.Contains("hung yes")) $w
Check "a number not read is UNKNOWN, never nought" ($w.Contains("processor UNKNOWN") -and $w.Contains("private UNKNOWN")) $w
$r.Answers = $null; $r.Hung = $null; $r.WindowWhy = "no window of the adopted Navisworks is up"
$w = LifeWords $r 1.0 $null "x"
Check "an answer not read is UNKNOWN with why" ($w.Contains("UNKNOWN, no window of the adopted Navisworks is up") -and $w.Contains("hung UNKNOWN")) $w
$tail = New-Object System.Collections.Generic.List[string]
Check "no line read yet says so" ((LifeStepOf $tail) -eq "none read yet") ""
$tail.Add("14:44:14.699  +1053.628s  CLASH    created  BLD-AR-Walls-vs-BLD-AR-Floors")
Check "with no step line the newest line is taken" ((LifeStepOf $tail).Contains("CLASH created BLD-AR-Walls-vs-BLD-AR-Floors")) (LifeStepOf $tail)
$tail.Insert(0, "14:27:25.590  +0044.550s  STEP     HARVEST       entered again")
$tail.Add("14:44:15.000  +1054.000s  ROWS     something")
Check "the newest STEP line is taken over a newer line of another kind" ((LifeStepOf $tail).StartsWith("14:27:25.590 +0044.550s STEP HARVEST")) (LifeStepOf $tail)
$tail.Add("14:45:00.000  +1100.000s  PROGRESS HARVEST BLD-AR-Walls-vs-BLD-AR-Floors clash 120 of 1186")
Check "a PROGRESS line is taken over an older STEP line" ((LifeStepOf $tail).Contains("PROGRESS HARVEST BLD-AR-Walls-vs-BLD-AR-Floors clash 120 of 1186")) (LifeStepOf $tail)
$tail.Add("14:46:00.000  +1160.000s  CALL     TestsImageForResult took 34.2 s on clash 121 of 1186")
Check "a CALL line is taken too" ((LifeStepOf $tail).Contains("CALL TestsImageForResult took 34.2 s")) (LifeStepOf $tail)
$long = "14:47:00.000  +1220.000s  STEP     " + ("x" * 300)
$tail.Add($long)
Check "a long line is cut at 160" ((LifeStepOf $tail).Length -eq 160) ("length " + (LifeStepOf $tail).Length)
Over

Write-Output "==== LifeRead on the stand-in, copied as LifeStandIn.exe ===="
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
$started = New-Object System.Collections.Generic.List[System.Diagnostics.Process]
try {
  $ready = Join-Path $Work "hang-ready.txt"
  $decoy = Start-Process -FilePath (Join-Path $bin "LifeStandIn.exe") -ArgumentList @("decoy", ('"' + (Join-Path $Work "decoy-log.txt") + '"'), '"Parsons NWC Federator, the stand-in that answers"', "60") -PassThru
  $started.Add($decoy)
  $hang = Start-Process -FilePath (Join-Path $bin "LifeStandIn.exe") -ArgumentList @("hang", ('"' + $ready + '"'), "40", '"Parsons NWC Federator, the stand-in that is blocked"', "2") -PassThru
  $started.Add($hang)
  $until = [DateTime]::UtcNow.AddSeconds(30)
  while (-not (Test-Path -LiteralPath $ready) -and [DateTime]::UtcNow -lt $until) { Start-Sleep -Milliseconds 200 }
  Check "the blocked stand-in wrote its ready file" (Test-Path -LiteralPath $ready) ""
  # Windows counts a window hung once its thread has read no message for 5 s.
  Start-Sleep -Seconds 7
  $decoy.Refresh(); $hang.Refresh()
  $a = LifeRead ([pscustomobject]@{ WinType = $wt.WinType; ProcType = $wt.ProcType; MyPid = $decoy.Id; MyProc = $decoy })
  Check "LifeRead: the window that answers reads answers, not hung" ($a.Answers -eq $true -and $a.Hung -eq $false -and $a.Window -eq "tool's window") ("answers " + $a.Answers + ", hung " + $a.Hung + ", " + $a.Window)
  Check "LifeRead: its memory and the machine's are read" ($null -ne $a.Private -and $a.Private -gt 0 -and $null -ne $a.Ws -and $null -ne $a.FreePhys -and $a.FreePhys -gt 0 -and $null -ne $a.TotalPhys -and $null -ne $a.FreeCommit) ("private " + $a.Private + ", free " + $a.FreePhys)
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $h = LifeRead ([pscustomobject]@{ WinType = $wt.WinType; ProcType = $wt.ProcType; MyPid = $hang.Id; MyProc = $hang })
  Check "LifeRead: the blocked window reads not answering and hung" ($h.Answers -eq $false -and $h.Hung -eq $true -and $h.Window -eq "tool's window") ("answers " + $h.Answers + ", hung " + $h.Hung + ", " + $h.Window)
  Check "LifeRead: a hung window costs no 2 s wait" ($sw.Elapsed.TotalSeconds -lt 2) ($sw.Elapsed.TotalSeconds.ToString("0.000") + " s")
  $me = Get-Process -Id $PID
  $n = LifeRead ([pscustomobject]@{ WinType = $wt.WinType; ProcType = $wt.ProcType; MyPid = $PID; MyProc = $me })
  Check "LifeRead: a process with no such window reads UNKNOWN with why" ($null -eq $n.Answers -and $n.WindowWhy -ne "") ("answers " + $n.Answers)
  Check "LifeWords on that reading says UNKNOWN" ((LifeWords $n $null $null "x").Contains("window UNKNOWN, no window of the adopted Navisworks is up")) (LifeWords $n $null $null "x")
} finally {
  foreach ($p in $started) { try { if (-not $p.HasExited) { $p.Kill(); [void]$p.WaitForExit(10000) } } catch { } }
}
Over
Write-Output ("prove-life: " + $right + " right and " + $wrong + " WRONG, in " + $clock.Elapsed.TotalSeconds.ToString("0") + " s")
if ($wrong -gt 0) { exit 1 }
exit 0
