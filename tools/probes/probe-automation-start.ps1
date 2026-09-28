param(
  [string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025",
  [string]$Out = "",
  [string]$PluginAssembly = "",
  [string]$Nwc = "",
  [int]$QuitWaitSeconds = 60,
  [int]$DeadlineSeconds = 480,
  [switch]$ReflectionOnly
)
$ErrorActionPreference = "Stop"

# F100, Phase 1 item 1 of the loop, 2026-09-28. Can a script on this machine start
# Navisworks Manage 2025 with no click through Autodesk.Navisworks.Api.Automation, know
# the process id of the Navisworks it started, open a copy of one NWC, load a plugin
# assembly with AddPluginAssembly, and quit. If it can, tools\loop\run.ps1 drives each run
# through this API and ExecuteAddInPlugin. If it cannot, run.ps1 starts Roamer.exe itself.
#
# The six steps, in order, stopping at the first that fails:
#   1  reflection over Autodesk.Navisworks.Automation.dll, nothing started
#   2  every Roamer already running, with its id and start time. None of them is ever
#      closed, attached to or sent anything. No window of theirs is read
#   3  one Navisworks started through the API. Its id is the one Roamer that was not in
#      the list from step 2, written down at once, and its window and any dialog recorded
#   4  one NWC copied from %LOCALAPPDATA%\NwcFederatorLoop\source into the work folder
#      and opened through the API. Nothing else is ever read as a source
#   5  AddPluginAssembly with the add-in built from this repo. ExecuteAddInPlugin is NOT
#      called, because the tool's own plugin opens its window
#   6  quit through the API, then the id from step 3 must be gone within
#      -QuitWaitSeconds. If not, THAT id and no other is closed with Stop-Process
#
# Steps 3 to 6 sit in one try with a finally that closes the id from step 3 whenever it is
# still running. A watchdog on its own runspace records every new process and every
# visible window of the new Roamer each half second, so a dialog raised while a call
# blocks is still seen, and after -DeadlineSeconds it closes the id from step 3 so a
# blocked call returns.
#
# Everything written goes under %LOCALAPPDATA%\NwcFederatorLoop\probes\automation-start,
# bar the -Out file. Run it with Windows PowerShell 5.1, 64 bit:
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\probe-automation-start.ps1 -Out tools\probes\automation-start-result-20260928.txt
#
# -ReflectionOnly stops after step 1 and starts nothing.

$nw = $NavisworksPath.TrimEnd('\')
$autoDll = Join-Path $nw "Autodesk.Navisworks.Automation.dll"
$loopRoot = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$sourceRoot = Join-Path $loopRoot "source"
$work = Join-Path $loopRoot "probes\automation-start"
if ($PluginAssembly -eq "") {
  $PluginAssembly = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll"
}
$utf8 = New-Object System.Text.UTF8Encoding($false)
$T0 = [DateTime]::Now
if ($Out -ne "") {
  $Out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
  [System.IO.File]::WriteAllText($Out, "", $utf8)
}

function Say($t) {
  [Console]::Out.WriteLine($t)
  if ($Out -ne "") { [System.IO.File]::AppendAllText($Out, $t + "`r`n", $utf8) }
}
function Stamp { return ([DateTime]::Now.ToString("HH:mm:ss.fff") + "  t+" + ([DateTime]::Now - $T0).TotalSeconds.ToString("0.0") + "s") }

function TypeName($t) {
  if ($null -eq $t) { return "null" }
  if ($t.IsByRef) { return (TypeName $t.GetElementType()) + "&" }
  if ($t.IsArray) { return (TypeName $t.GetElementType()) + "[]" }
  if ($t.IsGenericType) {
    $args2 = @()
    foreach ($a in $t.GetGenericArguments()) { $args2 += (TypeName $a) }
    $n = $t.Name
    $tick = $n.IndexOf('`')
    if ($tick -ge 0) { $n = $n.Substring(0, $tick) }
    return $t.Namespace + "." + $n + "<" + ($args2 -join ", ") + ">"
  }
  if ($t.IsGenericParameter) { return $t.Name }
  return $t.FullName
}

function ParamList($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) {
    $pre = ""
    if ($q.GetCustomAttributes([System.ParamArrayAttribute], $false).Count -gt 0) { $pre = "params " }
    elseif ($q.IsOut) { $pre = "out " }
    elseif ($q.ParameterType.IsByRef) { $pre = "ref " }
    $s = $pre + (TypeName $q.ParameterType) + " " + $q.Name
    if ($q.IsOptional) { $s += " = " + [string]$q.DefaultValue }
    $ps += $s
  }
  return ($ps -join ", ")
}

function Visibility($m) {
  if ($m.IsPublic) { return "public" }
  if ($m.IsFamilyOrAssembly) { return "protected internal" }
  if ($m.IsFamily) { return "protected" }
  if ($m.IsAssembly) { return "internal" }
  return "private"
}

# A real IL walk, opcode by opcode, so an operand byte is never read as an opcode.
$script:op1 = @{}
$script:op2 = @{}
foreach ($f in [System.Reflection.Emit.OpCodes].GetFields("Public,Static")) {
  $oc = $f.GetValue($null)
  $v = [int]$oc.Value
  if ($oc.Size -eq 1) { $script:op1[$v -band 0xFF] = $oc } else { $script:op2[$v -band 0xFF] = $oc }
}

function IlWalk($method) {
  # Every constant, string and call in one method body, in order, and the methods called.
  $lines = New-Object System.Collections.Generic.List[string]
  $calls = New-Object System.Collections.Generic.List[object]
  $body = $null
  try { $body = $method.GetMethodBody() } catch { }
  if ($null -eq $body) { return @($lines, $calls) }
  $il = $body.GetILAsByteArray()
  $mod = $method.Module
  $i = 0
  while ($i -lt $il.Length) {
    $b = $il[$i]
    if ($b -eq 0xFE) { $oc = $script:op2[[int]$il[$i + 1]]; $i += 2 } else { $oc = $script:op1[[int]$b]; $i += 1 }
    if ($null -eq $oc) { $lines.Add("?? unknown opcode at " + $i); break }
    $ot = $oc.OperandType.ToString()
    if ($oc.Name.StartsWith("ldc.i4")) {
      if ($ot -eq "InlineNone") { $lines.Add($oc.Name) }
      elseif ($ot -eq "ShortInlineI") { $sv = [int]$il[$i]; if ($sv -gt 127) { $sv -= 256 }; $lines.Add($oc.Name + " " + $sv) }
      else { $lines.Add($oc.Name + " " + [BitConverter]::ToInt32($il, $i)) }
    }
    switch ($ot) {
      "InlineNone" { }
      "ShortInlineBrTarget" { $i += 1 }
      "ShortInlineI" { $i += 1 }
      "ShortInlineVar" { $i += 1 }
      "InlineVar" { $i += 2 }
      "InlineI8" { $i += 8 }
      "InlineR" { $i += 8 }
      "ShortInlineR" { $i += 4 }
      "InlineSwitch" { $n = [BitConverter]::ToInt32($il, $i); $i += 4 + 4 * $n }
      "InlineString" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try { $lines.Add("ldstr  `"" + $mod.ResolveString($tok) + "`"") } catch { $lines.Add("ldstr  token " + $tok) }
      }
      "InlineMethod" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try {
          $r = $mod.ResolveMethod($tok)
          $lines.Add($oc.Name.PadRight(9) + (TypeName $r.DeclaringType) + "::" + $r.Name + "(" + (ParamList $r) + ")")
          if ($r.Module -eq $mod) { $calls.Add($r) }
        } catch { $lines.Add($oc.Name.PadRight(9) + "token " + $tok) }
      }
      default { $i += 4 }
    }
  }
  return @($lines, $calls)
}

Say ("MACHINE   " + $env:COMPUTERNAME + "   " + $T0.ToString("yyyy-MM-dd HH:mm:ss"))
Say ("HOST      powershell " + $PSVersionTable.PSVersion + ", 64 bit process " + [Environment]::Is64BitProcess + ", apartment " + [System.Threading.Thread]::CurrentThread.ApartmentState + ", pid " + $PID)
Say ""

# =======================================================================================
Say "==== STEP 1. Reflection over Autodesk.Navisworks.Automation.dll, nothing started ===="
if (-not (Test-Path -LiteralPath $autoDll)) { Say ("UNKNOWN: no Autodesk.Navisworks.Automation.dll at " + $autoDll); exit 1 }
Say ("file      " + $autoDll + "   " + (Get-Item -LiteralPath $autoDll).Length + " bytes")
$asm = [System.Reflection.Assembly]::LoadFrom($autoDll)
Say ("assembly  " + $asm.FullName)
Say ("runtime   " + $asm.ImageRuntimeVersion)
$pek = [System.Reflection.PortableExecutableKinds]::NotAPortableExecutableImage
$mach = [System.Reflection.ImageFileMachine]::I386
$asm.ManifestModule.GetPEKind([ref]$pek, [ref]$mach)
Say ("pe kind   " + $pek + "   machine " + $mach)
Say "references:"
foreach ($r in ($asm.GetReferencedAssemblies() | Sort-Object Name)) { Say ("  " + $r.FullName) }
Say ""

$types = $null
try { $types = $asm.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] {
  Say "GetTypes threw ReflectionTypeLoadException, the types that did load follow, and the loader messages:"
  foreach ($le in $_.Exception.LoaderExceptions) { if ($null -ne $le) { Say ("  loader: " + $le.Message) } }
  $types = @($_.Exception.Types | Where-Object { $null -ne $_ })
}
$public = @($types | Where-Object { $_.IsPublic -or $_.IsNestedPublic } | Sort-Object FullName)
Say ("types in the assembly: " + $types.Count + ", public: " + $public.Count)
Say "Every public type follows. The ones named std. or _TP_ are C++ runtime structs the"
Say "compiler made public, with no members."
Say ""

foreach ($t in $public) {
  $kind = "class"
  if ($t.IsInterface) { $kind = "interface" } elseif ($t.IsEnum) { $kind = "enum" } elseif ($t.IsValueType) { $kind = "struct" } elseif ($null -ne $t.BaseType -and $t.BaseType.FullName -eq "System.MulticastDelegate") { $kind = "delegate" }
  $mods = @()
  if ($t.IsAbstract -and $t.IsSealed) { $mods += "static" } elseif ($t.IsAbstract -and -not $t.IsInterface) { $mods += "abstract" } elseif ($t.IsSealed -and $kind -eq "class") { $mods += "sealed" }
  Say ("---- " + (($mods + @("public", $kind)) -join " ") + " " + $t.FullName + " ----")
  if ($null -ne $t.BaseType) { Say ("  base type: " + (TypeName $t.BaseType)) }
  $ifs = @($t.GetInterfaces() | ForEach-Object { TypeName $_ })
  if ($ifs.Count -gt 0) { Say ("  implements: " + ($ifs -join ", ")) }
  foreach ($ca in $t.GetCustomAttributesData()) { Say ("  attribute: " + $ca.ToString()) }
  if ($t.IsEnum) {
    foreach ($n in [Enum]::GetNames($t)) { Say ("  " + $n + " = " + [Convert]::ToInt64([Enum]::Parse($t, $n))) }
    Say ""
    continue
  }
  $flags = [System.Reflection.BindingFlags]"Public,Instance,Static,DeclaredOnly"
  foreach ($c in $t.GetConstructors($flags)) { Say ("  public " + $t.Name + "(" + (ParamList $c) + ")") }
  foreach ($p in ($t.GetProperties($flags) | Sort-Object Name)) {
    $acc = @()
    $g = $p.GetGetMethod($false); $s = $p.GetSetMethod($false)
    if ($null -ne $g) { $acc += "get" }
    if ($null -ne $s) { $acc += "set" }
    $st = ""
    if (($null -ne $g -and $g.IsStatic) -or ($null -ne $s -and $s.IsStatic)) { $st = "static " }
    Say ("  public " + $st + (TypeName $p.PropertyType) + " " + $p.Name + " { " + ($acc -join "; ") + " }")
  }
  foreach ($e in ($t.GetEvents($flags) | Sort-Object Name)) { Say ("  public event " + (TypeName $e.EventHandlerType) + " " + $e.Name) }
  foreach ($f in ($t.GetFields($flags) | Sort-Object Name)) {
    $st = ""; if ($f.IsStatic) { $st = "static " }
    Say ("  public " + $st + (TypeName $f.FieldType) + " " + $f.Name)
  }
  foreach ($m in ($t.GetMethods($flags) | Where-Object { -not $_.IsSpecialName } | Sort-Object Name)) {
    $st = ""; if ($m.IsStatic) { $st = "static " }
    $vi = ""; if ($m.IsVirtual -and -not $m.IsFinal -and -not $t.IsInterface) { $vi = "virtual " }
    Say ("  public " + $st + $vi + (TypeName $m.ReturnType) + " " + $m.Name + "(" + (ParamList $m) + ")")
  }
  Say ""
}

$napp = $asm.GetType("Autodesk.Navisworks.Api.Automation.NavisworksApplication")
if ($null -eq $napp) { Say "UNKNOWN: no type Autodesk.Navisworks.Api.Automation.NavisworksApplication in the assembly. STOP"; exit 1 }

Say "---- NavisworksApplication, every member public or not, declared on it ----"
foreach ($m in ($napp.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly") | Sort-Object Name)) {
  $st = ""; if ($m.IsStatic) { $st = "static " }
  Say ("  " + (Visibility $m) + " " + $st + (TypeName $m.ReturnType) + " " + $m.Name + "(" + (ParamList $m) + ")")
}
foreach ($c in $napp.GetConstructors("Public,NonPublic,Instance")) { Say ("  " + (Visibility $c) + " .ctor(" + (ParamList $c) + ")") }
foreach ($f in $napp.GetFields("Public,NonPublic,Instance,Static,DeclaredOnly")) { Say ("  field " + (TypeName $f.FieldType) + " " + $f.Name) }
Say ""

# How the constructor starts Navisworks, off its IL, so it is known before anything
# starts whether it launches a new Roamer or reaches one already running.
Say "---- What the constructors, GetRunningInstance, Dispose and the finalizer call, off the IL, three levels inside this assembly ----"
$seen = @{}
$queue = New-Object System.Collections.Generic.Queue[object]
foreach ($c in $napp.GetConstructors("Public,NonPublic,Instance")) { $queue.Enqueue(@($c, 0)) }
foreach ($n in @("GetRunningInstance", "Dispose", "Finalize", "StayOpen")) {
  foreach ($m in $napp.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) { if ($m.Name -eq $n) { $queue.Enqueue(@($m, 0)) } }
}
while ($queue.Count -gt 0) {
  $item = $queue.Dequeue(); $m = $item[0]; $depth = $item[1]
  $key = [string]$m.MetadataToken
  if ($seen.ContainsKey($key)) { continue }
  $seen[$key] = $true
  Say ("  [" + $depth + "] " + (Visibility $m) + " " + (TypeName $m.DeclaringType) + "::" + $m.Name + "(" + (ParamList $m) + ")")
  $walk = IlWalk $m
  foreach ($line in $walk[0]) { Say ("        " + $line) }
  if ($depth -lt 3) { foreach ($r in $walk[1]) { $queue.Enqueue(@($r, ($depth + 1))) } }
}
Say ""

Say "---- Which COM class the native half creates ----"
$bytes = [System.IO.File]::ReadAllBytes($autoDll)
$docKey = "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Navisworks.Document\CLSID"
$docClsid = $null
try { $docClsid = (Get-ItemProperty -LiteralPath $docKey).'(default)' } catch { }
if ($null -eq $docClsid) { Say "  UNKNOWN: no HKLM Classes Navisworks.Document CLSID" } else {
  $needle = ([Guid]$docClsid).ToByteArray()
  $hits = @()
  for ($i = 0; $i -le $bytes.Length - 16; $i++) {
    if ($bytes[$i] -ne $needle[0]) { continue }
    $ok = $true
    for ($j = 1; $j -lt 16; $j++) { if ($bytes[$i + $j] -ne $needle[$j]) { $ok = $false; break } }
    if ($ok) { $hits += $i }
  }
  $ls = $null
  try { $ls = (Get-ItemProperty -LiteralPath ("Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + $docClsid + "\LocalServer32")).'(default)' } catch { }
  Say ("  Navisworks.Document CLSID " + $docClsid + ", LocalServer32 " + $ls)
  Say ("  that CLSID's 16 bytes appear in the DLL at offsets: " + $(if ($hits.Count -gt 0) { $hits -join ", " } else { "none" }))
}
$ascii = [System.Text.Encoding]::ASCII.GetString($bytes)
foreach ($w in @("CoCreateInstance", "GetActiveObject", "CoGetClassObject", "CreateProcessW", "ShellExecuteW")) {
  Say ("  import name " + $w.PadRight(18) + " in the DLL: " + $ascii.Contains($w))
}
Say ""

# Whether that COM class could be served by a Navisworks already running decides whether
# step 3 could reach one that is not ours. A Roamer serves it only when it registered as
# the COM server, and whether it does is read off Roamer's own command line parser, in
# reflection only, so no code in these assemblies runs.
Say "---- Which command line makes a Roamer the COM automation server, reflection only ----"
$roamerGui = Join-Path $nw "navisworks.gui.roamer.dll"
if (-not (Test-Path -LiteralPath $roamerGui)) { Say ("  UNKNOWN: no navisworks.gui.roamer.dll at " + $roamerGui) } else {
  $roResolve = [ResolveEventHandler]{
    param($s, $e)
    $n = New-Object System.Reflection.AssemblyName($e.Name)
    $cand = Join-Path $nw ($n.Name + ".dll")
    if (Test-Path -LiteralPath $cand) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand) }
    try { return [System.Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { return $null }
  }
  [AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($roResolve)
  $ga = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($roamerGui)
  $parser = $ga.GetType("Autodesk.Navisworks.Gui.Roamer.CommandLineParser")
  $options = New-Object System.Collections.Generic.List[string]
  foreach ($m in $parser.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    $body = $m.GetMethodBody(); if ($null -eq $body) { continue }
    $il = $body.GetILAsByteArray(); $mod = $m.Module; $i = 0
    $recent = New-Object System.Collections.Generic.List[string]
    $lastStr = $null
    while ($i -lt $il.Length) {
      $b = $il[$i]
      if ($b -eq 0xFE) { $oc = $script:op2[[int]$il[$i + 1]]; $i += 2 } else { $oc = $script:op1[[int]$b]; $i += 1 }
      if ($null -eq $oc) { break }
      $ot = $oc.OperandType.ToString()
      switch ($ot) {
        "InlineNone" { }
        "ShortInlineBrTarget" { $i += 1 }
        "ShortInlineI" { $i += 1 }
        "ShortInlineVar" { $i += 1 }
        "InlineVar" { $i += 2 }
        "InlineI8" { $i += 8 }
        "InlineR" { $i += 8 }
        "InlineSwitch" { $n = [BitConverter]::ToInt32($il, $i); $i += 4 + 4 * $n }
        "InlineString" { $lastStr = $mod.ResolveString([BitConverter]::ToInt32($il, $i)); $recent.Add($lastStr); $i += 4 }
        "InlineMethod" {
          $tok = [BitConverter]::ToInt32($il, $i); $i += 4
          try { $r = $mod.ResolveMethod($tok); if ($r.Name -eq "MatchOption" -and $null -ne $lastStr -and -not $options.Contains($lastStr)) { $options.Add($lastStr) } } catch { }
          $lastStr = $null
        }
        "InlineField" {
          $tok = [BitConverter]::ToInt32($il, $i); $i += 4
          try {
            $fld = $mod.ResolveField($tok)
            if ($oc.Name -eq "stfld" -and $fld.Name -eq "COMAutomationStartup") {
              Say ("  " + $parser.Name + "::" + $m.Name + " sets CommandLineConfig.COMAutomationStartup after matching the option names: " + (($recent | ForEach-Object { "`"" + $_ + "`"" }) -join ", "))
            }
            if ($oc.Name -eq "stfld") { $recent.Clear() }
          } catch { }
        }
        default { $i += 4 }
      }
    }
  }
  Say ("  every option name the parser matches, " + $options.Count + ": " + ($options -join ", "))
  $roamerExe = Join-Path $nw "Roamer.exe"
  $ra = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($roamerExe)
  $mainImpl = $ra.GetType("NetRoamer.Program").GetMethod("MainImpl", [System.Reflection.BindingFlags]"NonPublic,Public,Static")
  $readsIt = $false
  if ($null -ne $mainImpl) {
    $il = $mainImpl.GetMethodBody().GetILAsByteArray(); $i = 0
    while ($i -lt $il.Length) {
      $b = $il[$i]
      if ($b -eq 0xFE) { $oc = $script:op2[[int]$il[$i + 1]]; $i += 2 } else { $oc = $script:op1[[int]$b]; $i += 1 }
      if ($null -eq $oc) { break }
      $ot = $oc.OperandType.ToString()
      if ($ot -eq "InlineField") { try { if ($mainImpl.Module.ResolveField([BitConverter]::ToInt32($il, $i)).Name -eq "COMAutomationStartup") { $readsIt = $true } } catch { }; $i += 4 }
      elseif ($ot -eq "InlineNone") { }
      elseif ($ot -eq "ShortInlineBrTarget" -or $ot -eq "ShortInlineI" -or $ot -eq "ShortInlineVar") { $i += 1 }
      elseif ($ot -eq "InlineVar") { $i += 2 }
      elseif ($ot -eq "InlineI8" -or $ot -eq "InlineR") { $i += 8 }
      elseif ($ot -eq "InlineSwitch") { $n = [BitConverter]::ToInt32($il, $i); $i += 4 + 4 * $n }
      else { $i += 4 }
    }
  }
  Say ("  Roamer.exe NetRoamer.Program::MainImpl reads COMAutomationStartup off the parsed command line and hands it to the initialiser: " + $readsIt)
}
Say ""

if ($ReflectionOnly) { Say "ReflectionOnly: stopped after step 1, nothing was started"; exit 0 }

# =======================================================================================
New-Item -ItemType Directory -Force -Path $work | Out-Null
$watchFile = Join-Path $work "watch.txt"
$pidFile = Join-Path $work "mypid.txt"
[System.IO.File]::WriteAllText($watchFile, "", $utf8)
[System.IO.File]::WriteAllText($pidFile, "", $utf8)

Add-Type @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class ProbeWin {
  delegate bool EnumProc(IntPtr h, IntPtr l);
  [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc f, IntPtr l);
  [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent, EnumProc f, IntPtr l);
  [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessageTimeout(IntPtr h, uint msg, IntPtr w, StringBuilder l, uint flags, uint timeout, out IntPtr result);
  // The process id is compared FIRST, so no window of any other process is ever read, and
  // a message is only ever sent to a dialog of the process asked for.
  public static List<string> Top(int pid, bool visibleOnly) {
    var list = new List<string>();
    EnumWindows(delegate (IntPtr h, IntPtr l) {
      uint p; GetWindowThreadProcessId(h, out p);
      if (p != (uint)pid) return true;
      bool vis = IsWindowVisible(h);
      if (visibleOnly && !vis) return true;
      var c = new StringBuilder(256); GetClassName(h, c, 256);
      var t = new StringBuilder(512); GetWindowText(h, t, 512);
      string line = "[" + c + "] \"" + t + "\"" + (vis ? "" : " hidden");
      if (vis && c.ToString() == "#32770") line += " text: " + DialogText(h);
      list.Add(line);
      return true;
    }, IntPtr.Zero);
    return list;
  }
  static string DialogText(IntPtr dialog) {
    var parts = new List<string>();
    EnumChildWindows(dialog, delegate (IntPtr h, IntPtr l) {
      if (!IsWindowVisible(h)) return true;
      var sb = new StringBuilder(2048); IntPtr r;
      SendMessageTimeout(h, 0x000D, (IntPtr)2048, sb, 0x0002, 500, out r);
      string s = sb.ToString().Replace("\r", " ").Replace("\n", " ").Trim();
      if (s.Length > 0) parts.Add("\"" + s + "\"");
      return true;
    }, IntPtr.Zero);
    return string.Join(" ", parts);
  }
}
"@

function Alive($id, $start) {
  $p = Get-Process -Id $id -ErrorAction SilentlyContinue
  if ($null -eq $p) { return $false }
  try { if ($null -ne $start -and $p.StartTime -ne $start) { return $false } } catch { }
  return $true
}

function WindowsOf($id) {
  $vis = @([ProbeWin]::Top($id, $true))
  $all = @([ProbeWin]::Top($id, $false))
  $dialogs = @($vis | Where-Object { $_.StartsWith("[#32770]") })
  Say ("  visible top level windows of " + $id + ": " + $vis.Count + ", hidden: " + ($all.Count - $vis.Count) + ", dialogs of class #32770 visible: " + $dialogs.Count)
  foreach ($w in $vis) { Say ("    " + $w) }
}

function RegSnap($path) {
  $h = @{}
  if (-not (Test-Path -LiteralPath $path)) { return $h }
  $sha = [System.Security.Cryptography.SHA1]::Create()
  $keys = @(Get-Item -LiteralPath $path) + @(Get-ChildItem -LiteralPath $path -Recurse -ErrorAction SilentlyContinue)
  foreach ($k in $keys) {
    $lines = @()
    foreach ($n in ($k.GetValueNames() | Sort-Object)) {
      $v = $k.GetValue($n, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
      $lines += ($n + "=" + (@($v) -join ","))
    }
    $h[$k.Name] = [BitConverter]::ToString($sha.ComputeHash($utf8.GetBytes($lines -join "`n")))
  }
  return $h
}

function FileSnap($dir) {
  $h = @{}
  if (-not (Test-Path -LiteralPath $dir)) { return $h }
  foreach ($f in (Get-ChildItem -LiteralPath $dir -Recurse -File -Force -ErrorAction SilentlyContinue)) { $h[$f.FullName] = [string]$f.LastWriteTimeUtc.Ticks + "|" + $f.Length }
  return $h
}

# =======================================================================================
Say "==== STEP 2. Every Roamer running before anything starts ===="
$beforeRoamer = @(Get-Process -Name Roamer -ErrorAction SilentlyContinue | Sort-Object Id)
$beforeStart = @{}
foreach ($p in $beforeRoamer) {
  $st = $null; try { $st = $p.StartTime } catch { }
  $beforeStart[$p.Id] = $st
  Say ("  Roamer pid " + $p.Id + "  started " + $(if ($st) { $st.ToString("yyyy-MM-dd HH:mm:ss") } else { "UNKNOWN" }) + "  NOT MINE, never closed, attached to or sent anything")
}
if ($beforeRoamer.Count -eq 0) { Say "  none" }
$beforeAll = @(Get-Process | ForEach-Object { $_.Id })
Say ("  every process id on the machine taken as the before set: " + $beforeAll.Count)
$beforeTicks = @{}
foreach ($k in $beforeStart.Keys) { if ($null -ne $beforeStart[$k]) { $beforeTicks[[int]$k] = $beforeStart[$k].Ticks } else { $beforeTicks[[int]$k] = 0 } }

# A Roamer is new when its id is not in the step 2 list, or its id is there with another
# start time, so an id Windows handed out again cannot hide it.
function NewRoamers {
  $r = @()
  foreach ($p in @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)) {
    $st = $null; try { $st = $p.StartTime } catch { }
    if ($beforeStart.ContainsKey($p.Id) -and $beforeStart[$p.Id] -eq $st) { continue }
    $r += $p
  }
  return $r
}

$regPath = "Registry::HKEY_CURRENT_USER\Software\Autodesk\Navisworks Manage\22.0"
$regBackup = Join-Path $work "hkcu-navisworks-manage-22.0-before.reg"
& reg.exe export "HKCU\Software\Autodesk\Navisworks Manage\22.0" $regBackup /y | Out-Null
Say ("  HKCU Navisworks Manage 22.0 exported as a backup to " + $regBackup + ", exit " + $LASTEXITCODE)
$regBefore = RegSnap $regPath
$nwAppData = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025"
$filesBefore = FileSnap $nwAppData
$fedLogs = Join-Path $env:LOCALAPPDATA "ParsonsNwcFederator\logs"
$fedBefore = FileSnap $fedLogs
Say ("  snapshots: " + $regBefore.Count + " registry keys, " + $filesBefore.Count + " files under %APPDATA%\Autodesk\Navisworks Manage 2025, " + $fedBefore.Count + " files in the tool's own logs folder")
Say ""

# The watchdog. It reads windows of the NEW Roamer only.
$sync = [hashtable]::Synchronized(@{})
$sync.Stop = $false
$sync.Before = [int[]]$beforeAll
$sync.BeforeRoamer = $beforeTicks
$sync.T0 = $T0
$sync.CallStart = [DateTime]::MaxValue
$sync.MyPid = 0
$sync.MyStart = $null
$sync.Deadline = $DeadlineSeconds
$sync.Forced = ""
$sync.WatchFile = $watchFile
$watchScript = {
  param($sync)
  $utf = New-Object System.Text.UTF8Encoding($false)
  function W($t) { [System.IO.File]::AppendAllText($sync.WatchFile, ([DateTime]::Now.ToString("HH:mm:ss.fff") + "  t+" + ([DateTime]::Now - $sync.T0).TotalSeconds.ToString("0.0") + "s  " + $t + "`r`n"), $utf) }
  function IsNewRoamer($p) {
    if ($p.ProcessName -ne "Roamer") { return $false }
    if (-not $sync.BeforeRoamer.ContainsKey($p.Id)) { return $true }
    $t = 0; try { $t = $p.StartTime.Ticks } catch { }
    return ($t -ne $sync.BeforeRoamer[$p.Id])
  }
  $seen = @{}
  $lastWin = @{}
  W "watchdog started"
  while (-not $sync.Stop) {
    try {
      $procs = @(Get-Process)
      foreach ($p in $procs) {
        if (($sync.Before -contains $p.Id) -and -not (IsNewRoamer $p)) { continue }
        if ($seen.ContainsKey($p.Id)) { continue }
        $seen[$p.Id] = $true
        if ($p.ProcessName -match '^(Roamer|Adsk|Autodesk|Navis|Lc|Genuine|AdSSO|WerFault|FNPLicensing|LMU)') {
          $ci = Get-CimInstance Win32_Process -Filter ("ProcessId=" + $p.Id) -ErrorAction SilentlyContinue
          $st = ""; try { $st = $p.StartTime.ToString("HH:mm:ss.fff") } catch { }
          W ("new process " + $p.ProcessName + " pid " + $p.Id + ", parent " + $ci.ParentProcessId + ", started " + $st + ", command line: " + $ci.CommandLine)
        }
      }
      foreach ($p in $procs) {
        if (-not (IsNewRoamer $p)) { continue }
        $wins = ([ProbeWin]::Top($p.Id, $true)) -join " | "
        if ($wins -eq "") { $wins = "none" }
        if ($lastWin[$p.Id] -ne $wins) { $lastWin[$p.Id] = $wins; W ("Roamer " + $p.Id + " visible top level windows: " + $wins) }
      }
      foreach ($k in @($lastWin.Keys)) {
        if ($lastWin[$k] -ne "GONE" -and $null -eq (Get-Process -Id $k -ErrorAction SilentlyContinue)) { $lastWin[$k] = "GONE"; W ("Roamer " + $k + " has exited") }
      }
      if ($sync.Forced -eq "" -and ([DateTime]::Now - $sync.T0).TotalSeconds -gt $sync.Deadline) {
        $target = $sync.MyPid
        if ($target -eq 0) {
          # Not yet written down by the main thread. Only a Roamer new since step 2,
          # started after the call began and started by COM with -Embedding, is provably mine.
          $cands = @()
          foreach ($p in $procs) {
            if (-not (IsNewRoamer $p)) { continue }
            $ci = Get-CimInstance Win32_Process -Filter ("ProcessId=" + $p.Id) -ErrorAction SilentlyContinue
            if ($p.StartTime -ge $sync.CallStart -and $ci.CommandLine -match 'Embedding') { $cands += $p.Id }
          }
          if ($cands.Count -eq 1) { $target = $cands[0] }
          else { W ("DEADLINE passed and " + $cands.Count + " provable candidates, so nothing is closed"); $sync.Forced = "deadline, nothing provable" }
        }
        if ($target -ne 0) {
          W ("DEADLINE of " + $sync.Deadline + " s passed, closing pid " + $target + " with Stop-Process")
          Stop-Process -Id $target -Force -ErrorAction SilentlyContinue
          $sync.Forced = "deadline closed " + $target
        }
      }
    } catch { W ("watchdog error: " + $_.Exception.Message) }
    Start-Sleep -Milliseconds 500
  }
  W "watchdog stopped"
}
$wps = [PowerShell]::Create()
[void]$wps.AddScript($watchScript).AddArgument($sync)
$whandle = $wps.BeginInvoke()

$app = $null
$myPid = 0
$myStart = $null
$disposed = $false
$outcome = @{}
try {
  # =====================================================================================
  Say "==== STEP 3. Start one Navisworks through the API ===="
  Say ("  " + (Stamp) + "  calling new NavisworksApplication()")
  $sync.CallStart = [DateTime]::Now
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app = [Activator]::CreateInstance($napp) } catch { $err = $_.Exception }
  $sw.Stop()
  $returnedAt = [DateTime]::Now
  Say ("  " + (Stamp) + "  the constructor " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  $e = $err
  while ($null -ne $e) { Say ("    exception " + $e.GetType().FullName + ": " + $e.Message); $e = $e.InnerException }

  $newRoamer = @(NewRoamers)
  Say ("  Roamer processes not in the step 2 list: " + $newRoamer.Count)
  foreach ($p in $newRoamer) {
    $ci = Get-CimInstance Win32_Process -Filter ("ProcessId=" + $p.Id)
    Say ("    pid " + $p.Id + ", started " + $p.StartTime.ToString("HH:mm:ss.fff") + ", parent " + $ci.ParentProcessId + ", path " + $ci.ExecutablePath)
    Say ("    command line: " + $ci.CommandLine)
  }
  if ($newRoamer.Count -ne 1) {
    Say ("  STOP. " + $newRoamer.Count + " new Roamer processes, not exactly one, so none is provably mine by this rule and steps 4 to 6 do not run")
    if ($null -ne $app -and $newRoamer.Count -eq 0) {
      # Whatever the object reached, it was not a Navisworks this probe started. Nothing is
      # called on it, not even Dispose, and its finalizer is switched off.
      [GC]::SuppressFinalize($app)
      Say "  nothing is called on the object, not Dispose, and its finalizer is suppressed"
    }
    $outcome["3"] = "failed, " + $newRoamer.Count + " new Roamer processes"
    throw "step 3 stopped"
  }
  $myPid = $newRoamer[0].Id
  $myStart = $newRoamer[0].StartTime
  $sync.MyPid = $myPid
  $sync.MyStart = $myStart
  [System.IO.File]::WriteAllText($pidFile, [string]$myPid + "`r`n" + $myStart.ToString("o") + "`r`n", $utf8)
  Say ("  MY PID IS " + $myPid + ", started " + $myStart.ToString("yyyy-MM-dd HH:mm:ss.fff") + ", written to " + $pidFile)
  Say ("  the process started " + ($myStart - $sync.CallStart).TotalSeconds.ToString("0.00") + " s after the call began and " + ($returnedAt - $myStart).TotalSeconds.ToString("0.00") + " s before the constructor returned")
  if ($null -ne $err) { $outcome["3"] = "failed, the constructor threw"; throw "step 3 stopped" }
  $mp = Get-Process -Id $myPid
  Say ("  MainWindowHandle " + $mp.MainWindowHandle + ", MainWindowTitle `"" + $mp.MainWindowTitle + "`"")
  WindowsOf $myPid
  $v0 = $app.Visible
  Say ("  " + (Stamp) + "  Visible read BEFORE setting it: " + $v0)
  $app.Visible = $true
  Start-Sleep -Milliseconds 1500
  $v1 = $app.Visible
  $mp.Refresh()
  Say ("  " + (Stamp) + "  set Visible = True, read back: " + $v1 + ", MainWindowHandle " + $mp.MainWindowHandle + ", title `"" + $mp.MainWindowTitle + "`"")
  WindowsOf $myPid
  $app.Visible = $false
  Start-Sleep -Milliseconds 1500
  $v2 = $app.Visible
  $mp.Refresh()
  Say ("  " + (Stamp) + "  set Visible = False, read back: " + $v2 + ", MainWindowHandle " + $mp.MainWindowHandle)
  WindowsOf $myPid
  Say "  ExecuteAddInPlugin exists: public System.Int32 ExecuteAddInPlugin(System.String pluginId, params System.String[] parameters). NOT called"
  $outcome["3"] = "passed"
  Say ""

  # =====================================================================================
  Say "==== STEP 4. Copy one NWC from the loop's source copy and open the copy ===="
  if ($Nwc -eq "") {
    $pick = Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter *.nwc | Sort-Object Length, FullName | Select-Object -First 1
    if ($null -eq $pick) { $outcome["4"] = "failed, no NWC in the source copy"; throw ("no NWC under " + $sourceRoot) }
    $srcFile = $pick.FullName
  } else { $srcFile = Join-Path $sourceRoot $Nwc }
  $full = [System.IO.Path]::GetFullPath($srcFile)
  if (-not $full.StartsWith($sourceRoot + "\", [StringComparison]::OrdinalIgnoreCase)) { $outcome["4"] = "refused, not under the source copy"; throw ("refused, " + $full + " is not under " + $sourceRoot) }
  $copy = Join-Path $work (Split-Path $full -Leaf)
  Copy-Item -LiteralPath $full -Destination $copy -Force
  $h1 = (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash
  $h2 = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
  Say ("  source " + $full + ", " + (Get-Item -LiteralPath $full).Length + " bytes, the smallest NWC in the copy")
  Say ("  copy   " + $copy + ", " + (Get-Item -LiteralPath $copy).Length + " bytes, sha256 " + $(if ($h1 -eq $h2) { "matches" } else { "DIFFERS" }))
  Say ("  " + (Stamp) + "  calling OpenFile(copy, no more files)")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.OpenFile($copy, [string[]]@()) } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  OpenFile " + $(if ($null -eq $err) { "RETURNED, it is void so there is no value" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  $e = $err
  while ($null -ne $e) { Say ("    exception " + $e.GetType().FullName + ": " + $e.Message); $e = $e.InnerException }
  WindowsOf $myPid
  if ($null -ne $err) { $outcome["4"] = "failed, OpenFile threw"; throw "step 4 stopped" }
  # OpenFile returns nothing, so the one outside sign that the model loaded is a save of
  # what is open. An NWD of it is written into the work folder and its size read back.
  $nwdOut = Join-Path $work "opened-copy-saved.nwd"
  Say ("  " + (Stamp) + "  check: SaveFile of what is open to " + $nwdOut)
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.SaveFile($nwdOut) } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  SaveFile " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  $e = $err
  while ($null -ne $e) { Say ("    exception " + $e.GetType().FullName + ": " + $e.Message); $e = $e.InnerException }
  if ([System.IO.File]::Exists($nwdOut)) { Say ("  the NWD exists, " + (New-Object System.IO.FileInfo($nwdOut)).Length + " bytes read back off the disk") } else { Say "  the NWD does NOT exist" }
  $h3 = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
  Say ("  the NWC copy after the open: sha256 " + $(if ($h3 -eq $h2) { "unchanged" } else { "CHANGED" }))
  $outcome["4"] = "passed"
  Say ""

  # =====================================================================================
  Say "==== STEP 5. AddPluginAssembly with the add-in built from this repo ===="
  if (-not (Test-Path -LiteralPath $PluginAssembly)) { $outcome["5"] = "failed, no built add-in"; throw ("no built add-in at " + $PluginAssembly) }
  $pa = [System.Reflection.AssemblyName]::GetAssemblyName($PluginAssembly)
  $info = ""
  try { $ra = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($PluginAssembly); foreach ($cad in $ra.GetCustomAttributesData()) { if ($cad.AttributeType.Name -eq "AssemblyInformationalVersionAttribute") { $info = [string]$cad.ConstructorArguments[0].Value } } } catch { $info = "UNKNOWN, " + $_.Exception.Message }
  Say ("  " + $PluginAssembly)
  Say ("  " + (Get-Item -LiteralPath $PluginAssembly).Length + " bytes, written " + (Get-Item -LiteralPath $PluginAssembly).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + ", " + $pa.FullName + ", stamp " + $info)
  Say ("  " + (Stamp) + "  calling AddPluginAssembly")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.AddPluginAssembly($PluginAssembly) } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  AddPluginAssembly " + $(if ($null -eq $err) { "RETURNED, it is void so there is no value" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  $e = $err
  while ($null -ne $e) { Say ("    exception " + $e.GetType().FullName + ": " + $e.Message); $e = $e.InnerException }
  WindowsOf $myPid
  Say "  ExecuteAddInPlugin is public System.Int32 ExecuteAddInPlugin(System.String pluginId, params System.String[] parameters). It was NOT called"
  if ($null -ne $err) { $outcome["5"] = "failed, AddPluginAssembly threw"; throw "step 5 stopped" }
  $outcome["5"] = "passed"
  Say ""

  # =====================================================================================
  Say "==== STEP 6. Quit through the API ===="
  Say ("  " + (Stamp) + "  calling Dispose()")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.Dispose(); $disposed = $true } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  Dispose " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  $e = $err
  while ($null -ne $e) { Say ("    exception " + $e.GetType().FullName + ": " + $e.Message); $e = $e.InnerException }
  $sw = [Diagnostics.Stopwatch]::StartNew()
  while ((Alive $myPid $myStart) -and $sw.Elapsed.TotalSeconds -lt $QuitWaitSeconds) { Start-Sleep -Milliseconds 250 }
  if (Alive $myPid $myStart) {
    Say ("  " + (Stamp) + "  pid " + $myPid + " STILL RUNNING " + $QuitWaitSeconds + " s after Dispose. It HAD TO BE FORCED: Stop-Process -Id " + $myPid)
    Stop-Process -Id $myPid -Force
    $sw2 = [Diagnostics.Stopwatch]::StartNew()
    while ((Alive $myPid $myStart) -and $sw2.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 250 }
    Say ("  after Stop-Process, pid " + $myPid + " running: " + (Alive $myPid $myStart))
    $outcome["6"] = "failed, had to be forced"
  } else {
    Say ("  " + (Stamp) + "  pid " + $myPid + " GONE " + $sw.Elapsed.TotalSeconds.ToString("0.0") + " s after Dispose returned, not forced")
    $outcome["6"] = "passed"
  }
  Say ""
} catch {
  Say ("  " + (Stamp) + "  stopped: " + $_.Exception.Message)
  Say ""
} finally {
  Say "==== FINALLY ===="
  if ($myPid -ne 0) {
    if (Alive $myPid $myStart) {
      Say ("  " + (Stamp) + "  pid " + $myPid + " is still running, closing it by its id: Stop-Process -Id " + $myPid)
      Stop-Process -Id $myPid -Force -ErrorAction SilentlyContinue
      $sw = [Diagnostics.Stopwatch]::StartNew()
      while ((Alive $myPid $myStart) -and $sw.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 250 }
      Say ("  pid " + $myPid + " running after that: " + (Alive $myPid $myStart))
      if ($null -ne $app -and -not $disposed) { [GC]::SuppressFinalize($app) }
    } else { Say ("  pid " + $myPid + " is not running, nothing to close") }
  } else {
    $late = @(NewRoamers)
    Say ("  no pid was written down. Roamer processes not in the step 2 list now: " + $late.Count + ". None is closed by this probe")
    foreach ($p in $late) { $ci = Get-CimInstance Win32_Process -Filter ("ProcessId=" + $p.Id); Say ("    pid " + $p.Id + ", started " + $p.StartTime.ToString("HH:mm:ss.fff") + ", command line: " + $ci.CommandLine) }
  }
  Start-Sleep -Milliseconds 1200
  $sync.Stop = $true
  try { [void]$wps.EndInvoke($whandle) } catch { }
  $wps.Dispose()
  Say ("  watchdog forced anything: " + $(if ($sync.Forced -eq "") { "no" } else { $sync.Forced }))
  Say ""
  Say "---- the watchdog's record, every new process and every visible window of the new Roamer ----"
  foreach ($l in [System.IO.File]::ReadAllLines($watchFile)) { Say ("  " + $l) }
  Say ""
  Say "---- the Roamers from step 2, at the end ----"
  foreach ($id in $beforeStart.Keys) {
    $still = Alive $id $beforeStart[$id]
    Say ("  pid " + $id + " started " + $(if ($beforeStart[$id]) { $beforeStart[$id].ToString("yyyy-MM-dd HH:mm:ss") } else { "UNKNOWN" }) + ": " + $(if ($still) { "STILL RUNNING, same start time" } else { "NOT RUNNING" }))
  }
  if ($beforeStart.Count -eq 0) { Say "  there were none" }
  Say ""
  Say "---- what changed on the machine while the probe ran ----"
  $regAfter = RegSnap $regPath
  $changed = @()
  foreach ($k in $regAfter.Keys) { if (-not $regBefore.ContainsKey($k)) { $changed += ("added   " + $k) } elseif ($regBefore[$k] -ne $regAfter[$k]) { $changed += ("changed " + $k) } }
  foreach ($k in $regBefore.Keys) { if (-not $regAfter.ContainsKey($k)) { $changed += ("removed " + $k) } }
  Say ("  HKCU Navisworks Manage 22.0 keys whose values changed, by name only: " + $changed.Count)
  foreach ($c in ($changed | Sort-Object)) { Say ("    " + $c.Replace("HKEY_CURRENT_USER\Software\Autodesk\Navisworks Manage\22.0", "22.0")) }
  $filesAfter = FileSnap $nwAppData
  $fch = @()
  foreach ($k in $filesAfter.Keys) { if (-not $filesBefore.ContainsKey($k)) { $fch += ("added   " + $k) } elseif ($filesBefore[$k] -ne $filesAfter[$k]) { $fch += ("changed " + $k) } }
  foreach ($k in $filesBefore.Keys) { if (-not $filesAfter.ContainsKey($k)) { $fch += ("removed " + $k) } }
  Say ("  files under %APPDATA%\Autodesk\Navisworks Manage 2025 added, changed or removed: " + $fch.Count)
  foreach ($c in ($fch | Sort-Object)) { Say ("    " + $c.Replace($env:APPDATA, "%APPDATA%")) }
  $fedAfter = FileSnap $fedLogs
  $gch = 0
  foreach ($k in $fedAfter.Keys) { if (-not $fedBefore.ContainsKey($k) -or $fedBefore[$k] -ne $fedAfter[$k]) { $gch++ } }
  Say ("  files in %LOCALAPPDATA%\ParsonsNwcFederator\logs added or changed: " + $gch)
  Say "  files in the work folder:"
  foreach ($f in (Get-ChildItem -LiteralPath $work -File)) { Say ("    " + $f.Name + "  " + $f.Length + " bytes") }
  Say ""
  Say "---- outcome ----"
  Say "  step 1 passed, reflection read"
  Say ("  step 2 passed, " + $beforeStart.Count + " Roamer running before")
  foreach ($s in @("3", "4", "5", "6")) { Say ("  step " + $s + " " + $(if ($outcome.ContainsKey($s)) { $outcome[$s] } else { "not reached" })) }
  Say ("  finished " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + ", " + ([DateTime]::Now - $T0).TotalSeconds.ToString("0") + " s in all")
}
