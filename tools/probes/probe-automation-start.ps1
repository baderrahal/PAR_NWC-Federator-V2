param(
  [string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025",
  [string]$Out = "",
  [string]$PluginAssembly = "",
  [string]$Nwc = "",
  [int]$QuitWaitSeconds = 60,
  [int]$ConstructorDeadlineSeconds = 300,
  [int]$AdoptedDeadlineSeconds = 300,
  [switch]$ReflectionOnly
)
$ErrorActionPreference = "Stop"

# F100, Phase 1 item 1 of the loop, 2026-09-28. Can a script on this machine start
# Navisworks Manage 2025 with no click through Autodesk.Navisworks.Api.Automation, know
# the process id of the Navisworks it started, open a copy of one NWC, load a plugin
# assembly with AddPluginAssembly, and quit. If it can, tools\loop\run.ps1 drives each run
# through this API and ExecuteAddInPlugin. If it cannot, run.ps1 starts Roamer.exe itself.
#
# NO START IS MADE WHILE ANY NAVISWORKS RUNS, the lead's decision D4 of 2026-09-28, and the
# code keeps that rule, not a person. Step 2 refuses while any process named Roamer runs,
# whatever its command line and whoever started it, names each by pid and start time, and
# writes nothing and starts nothing. The same read is made again after the backups,
# immediately before the Auto-Save switch and the constructor, and refuses the same way.
#
# IT REFUSES TO RUN when either deadline is below 60 seconds, and unless it is the script
# its own powershell.exe was started to run with -File, read off
# [Environment]::GetCommandLineArgs, so the deadline's TerminateProcess can never end a
# caller's process. Start ticks are UTC everywhere they are written or compared.
#
# THE OWNERSHIP RULE. A Navisworks this probe did not start is never closed, attached to or
# sent anything. A new Roamer whose command line does not hold the word embedding is a
# Navisworks someone started by hand: it is named as that, left alone, never written down,
# and it rules out the settings put back. A new Roamer whose command line holds the word
# embedding, or cannot be read, is a possible start. A Roamer is adopted as ours only when
# ALL of these hold: the constructor returned without throwing, exactly one possible start
# is new since step 2, its start time is at or after the call, and its command line holds
# the option -Embedding, which is how COM starts a local server. NOTHING IS EVER CLOSED
# BEFORE ADOPTION. When adoption fails, nothing is closed and nothing is called on the
# object, and its finalizer is suppressed when there is an object. When the constructor
# threw there is none, and run 3's IL shows what then: Init itself calls Bridge.__delDtor
# and Bridge.Terminate and zeroes m_bridge before it throws, result lines 244 to 254, and
# the finalizer then calls only Bridge.Terminate, lines 272 to 286. A process is only ever
# closed after its start time is read again and still matches.
#
# WHAT IS WRITTEN DOWN. %LOCALAPPDATA%\NwcFederatorLoop\probes\unproved-starts.txt gets every
# possible start that is not adopted and is still running at the constructor deadline or at
# the end, whether its start time was read or not, and the adopted Roamer if it is still
# running after every close path. One whose start time never read is written with its pid,
# the word Roamer and the time it was first seen. Step 2 of every later run refuses while a
# start named there runs with the same start ticks, or, on a line with no start time, while
# that pid runs as a process named Roamer, because a person has to look. A Roamer holding
# that pid whose start time reads LATER than the time the line says the start was first
# seen is another process, since the start ran before it was seen, so it is not refused on
# that line and the result says so. When its start time cannot be read, or the line's first
# seen time cannot be read, it is refused.
#
# The steps, in order, stopping at the first that fails:
#   1  reflection only, nothing started: the Automation DLL, its import table with every
#      ordinal named off the exporting DLL, the COM class its native half creates, and the
#      Roamer command line that makes a Roamer that class's server. Any verdict here that
#      comes out UNKNOWN stops the probe before anything starts
#   2  unproved-starts.txt first, then every process named Roamer. If any runs, whatever
#      its command line and whoever started it, each is named by pid and start time and the
#      probe stops, and a process list that cannot be read stops it too. Nothing is sent to
#      a Roamer, and its command line is not read
#   3  one Navisworks started through the API, adopted by the rule above
#   4  one NWC copied from %LOCALAPPDATA%\NwcFederatorLoop\source into the work folder and
#      opened through the API, then what is open saved as an NWD whose write time is
#      checked against the call
#   5  AddPluginAssembly with the add-in built from this repo. ExecuteAddInPlugin is NOT
#      called, because the tool's own plugin opens its window
#   6  quit through the API with Dispose. If the adopted id is still the same process
#      -QuitWaitSeconds later, that process and no other is closed through the handle the
#      adoption holds, CloseAdopted in tools\loop\nw-guard.ps1 since F103
#
# Steps 3 to 6 sit in one try whose finally closes the adopted id, if it is still the same
# process, BEFORE it writes anything, so a failed write cannot leave it running. So the
# adopted Roamer is quit by Dispose, and closed through its held handle only when Dispose
# leaves it running, a step failed, or the adopted deadline passed.
#
# THE WATCHDOG runs on its own runspace, from just after step 2. About every half second
# plus the time a pass takes, it records each process new since step 2 whose name starts
# with Roamer, Adsk, Autodesk, Navis, Lc, Genuine, AdSSO, WerFault, FNPLicensing or LMU,
# with its parent, named only when a read that succeeded shows that process started before
# the child, and whether its command line holds either word. A Roamer's command line is
# printed only in the automation form, the executable and -Embedding and nothing else. Its
# Win32_Process reads are cached by pid and start, and a read that failed is never cached
# and is made again at the next pass. It records every Roamer new since step 2 that any
# pass sees, which is how the settings rule knows whether another Navisworks ran. NOTHING IS
# SENT BEFORE ADOPTION: until a Roamer is adopted the watchdog reads only top level window
# handles, class names and captions, through GetClassName and GetWindowText, of the Roamers
# that meet the start time and -Embedding test, which sends no message into another
# process. After adoption it reads the adopted Roamer's windows and sends WM_GETTEXT to the
# visible children of each of its visible top level windows but the Navisworks main window,
# at most 20 children of one window and 2 s in all per pass, since F103 in
# tools\loop\nw-guard.ps1. It reads no window of any other process. It writes a line
# only when something changes, and it counts its passes, the longest, the gaps between them,
# its error lines and the passes that failed before their process loop.
#
# TWO DEADLINES, both at least 60 seconds. -ConstructorDeadlineSeconds runs from the call.
# Past it, with nothing adopted, the watchdog writes its block to the result once, with the
# settings changes it can read, old and new, and nothing put back, writes the possible
# starts still running to unproved-starts.txt, and ends this process through TerminateProcess
# with exit code 3, so that no finalizer of the half built object runs, and closes nothing.
# If TerminateProcess returns false it says so once and the watchdog stops, and then nothing
# is put back at the end, because no record covers what came after.
# -AdoptedDeadlineSeconds runs from adoption. Past it, the watchdog closes the adopted id
# after reading its start time again, so a blocked call returns.
#
# BADER'S SETTINGS. Before the constructor, every file under
# %APPDATA%\Autodesk\Navisworks Manage 2025 except AutoSave is copied into the work folder
# and HKCU\Software\Autodesk\Navisworks Manage\22.0 is exported and read key by key. The
# export must exit 0 and leave a file that is not empty, every copy must read back with the
# sha256 its source had, and every key must read, or the probe stops before the
# constructor. At the end every value, key and file that changed is printed with its old and
# new value. They are PUT BACK ONLY WHEN NO OTHER NAVISWORKS RAN, the lead's decision D2, and
# that is proved only by a watchdog record: passes above zero, no watchdog error line, and
# every failed read it writes counts as one, nothing in its runspace's error stream, no line it could not write and no pass that failed
# before its process loop, the constructor deadline's path never ran, and the watchdog was
# still running when the end stopped it. Then no Roamer at step 2, none new at any pass but
# the adopted one, none running at the end, and the adopted one reads gone. Otherwise
# nothing is written and the backup is kept. IMMEDIATELY BEFORE EACH WRITE the Roamers are
# listed again, and any Roamer at all stops every write that follows. Before each SetValue,
# DeleteValue, CreateSubKey or DeleteSubKeyTree the key is opened again and the value read
# again, and the write happens only when both still read what the compare read. A value is
# written only through its key as it stands, so a key that went since the compare is never
# made again, a value of a key that went is written only into the key this put back made
# again, and a key is made only under a parent that is there now, and when the compare read
# that parent as gone, only under one this put back made. Each write skipped for this is
# printed with why. A value delete is counted only when a value was deleted. Nothing is
# written under a key or a folder that could not be read at both ends, a
# key is removed only when the probe saw it appear while its parent read at both ends, a file
# that went is copied back only if it is still gone just before the copy, and a changed file
# only if it still holds what the compare read. A file whose content cannot be read at either
# end is never written. A file that appeared is left in place, because the loop deletes only
# from its own folder. AutoSave is compared and reported, never copied and never put back.
# A path in a printed value is shown in full only under the loop folder or the install
# folder, otherwise as its file name, if it has one, and a sha1 of the whole path.
#
# NAMED LIMITS, said here and in the result rather than fixed. A Navisworks that starts and
# exits inside one gap between watchdog passes is not seen by the D2 check, and the result
# prints the longest gap. A Navisworks start is measured at over ten seconds, and the result
# prints this run's. Of whether a running Navisworks can be reached, the result prints what
# step 1 read on that run of GetActiveObject in the Automation DLL's import table, UNKNOWN
# when step 1 did not read it, and says UNKNOWN for which call the constructor's IL takes
# when its argument is false, because no code reads that branch. When Dispose throws and
# the probe then closes the process through its held handle, the finalizer stays armed and at this
# process's exit calls Bridge.Terminate against a process that is gone. What that does is
# UNKNOWN.
#
# THE WORK FOLDER IS NEVER EMPTIED. A folder left by an earlier run is renamed
# automation-start-yyyyMMdd-HHmmss beside it, and a rename that fails stops the probe
# before step 2. The window reader is built in memory with Reflection.Emit, so no
# compiler runs and nothing is written under %TEMP%. Everything written goes under
# %LOCALAPPDATA%\NwcFederatorLoop\probes, bar the -Out file and what is put back. Run it
# with Windows PowerShell 5.1, 64 bit:
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\probe-automation-start.ps1 -Out tools\probes\automation-start-result-20260928.txt
#
# -ReflectionOnly stops after step 1 and starts, renames and writes nothing but -Out.

$nw = $NavisworksPath.TrimEnd('\')
$autoDll = Join-Path $nw "Autodesk.Navisworks.Automation.dll"
$loopRoot = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$sourceRoot = Join-Path $loopRoot "source"
$work = Join-Path $loopRoot "probes\automation-start"
$unproved = Join-Path $loopRoot "probes\unproved-starts.txt"
if ($PluginAssembly -eq "") {
  $PluginAssembly = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll"
}
$utf8 = New-Object System.Text.UTF8Encoding($false)
$T0 = [DateTime]::Now
$script:sayFailures = 0
$script:resolveFailures = New-Object System.Collections.Generic.List[string]
if ($Out -ne "") {
  $Out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
  [System.IO.File]::WriteAllText($Out, "", $utf8)
}

# Say never throws. A line that cannot reach -Out still reaches the console, and is counted.
function Say($t) {
  [Console]::Out.WriteLine($t)
  if ($Out -ne "") {
    try { [System.IO.File]::AppendAllText($Out, $t + "`r`n", $utf8) }
    catch { $script:sayFailures++; [Console]::Error.WriteLine("could not append to " + $Out + ": " + $_.Exception.Message) }
  }
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
  $qs = $null
  try { $qs = $m.GetParameters() } catch { return ("parameters unreadable, " + (Err $_.Exception)) }
  $ps = @()
  foreach ($q in $qs) {
    $pre = ""
    if ($q.GetCustomAttributesData() | Where-Object { $_.AttributeType.FullName -eq "System.ParamArrayAttribute" }) { $pre = "params " }
    elseif ($q.IsOut) { $pre = "out " }
    elseif ($q.ParameterType.IsByRef) { $pre = "ref " }
    $s = $pre + (TypeName $q.ParameterType) + " " + $q.Name
    if ($q.IsOptional) { $s += " = " + [string]$q.RawDefaultValue }
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

function Signature($m) {
  $st = ""; if ($m.IsStatic) { $st = "static " }
  return ((Visibility $m) + " " + $st + (TypeName $m.ReturnType) + " " + $m.Name + "(" + (ParamList $m) + ")")
}

# THE ONE IL READER. Opcode by opcode, so an operand byte is never read as an opcode, every
# operand decoded, branch targets as offsets. Anything it cannot resolve says so in the text.
$script:op1 = @{}
$script:op2 = @{}
foreach ($f in [System.Reflection.Emit.OpCodes].GetFields("Public,Static")) {
  $oc = $f.GetValue($null)
  $v = [int]$oc.Value
  if ($oc.Size -eq 1) { $script:op1[$v -band 0xFF] = $oc } else { $script:op2[$v -band 0xFF] = $oc }
}

function IlRead($method) {
  $list = New-Object System.Collections.Generic.List[object]
  $body = $null
  try { $body = $method.GetMethodBody() } catch {
    $list.Add([pscustomobject]@{ Offset = -1; Name = "error"; Text = ("GetMethodBody threw, " + (Err $_.Exception)); Member = $null })
    return ,$list
  }
  if ($null -eq $body) {
    $list.Add([pscustomobject]@{ Offset = -1; Name = "nobody"; Text = "no IL body: native, abstract or runtime implemented"; Member = $null })
    return ,$list
  }
  $il = $body.GetILAsByteArray()
  $mod = $method.Module
  $i = 0
  while ($i -lt $il.Length) {
    $at = $i
    $b = $il[$i]
    if ($b -eq 0xFE) { $oc = $script:op2[[int]$il[$i + 1]]; $i += 2 } else { $oc = $script:op1[[int]$b]; $i += 1 }
    if ($null -eq $oc) { $list.Add([pscustomobject]@{ Offset = $at; Name = "error"; Text = ("unknown opcode byte " + $b + ", the walk stops here"); Member = $null }); break }
    $text = ""; $member = $null
    switch ($oc.OperandType.ToString()) {
      "InlineNone" { }
      "ShortInlineBrTarget" { $d = [int]$il[$i]; if ($d -gt 127) { $d -= 256 }; $i += 1; $text = "IL_" + ($i + $d).ToString("X4") }
      "InlineBrTarget" { $d = [BitConverter]::ToInt32($il, $i); $i += 4; $text = "IL_" + ($i + $d).ToString("X4") }
      "ShortInlineI" { $d = [int]$il[$i]; if ($d -gt 127) { $d -= 256 }; $i += 1; $text = [string]$d }
      "ShortInlineVar" { $text = [string]$il[$i]; $i += 1 }
      "InlineVar" { $text = [string][BitConverter]::ToUInt16($il, $i); $i += 2 }
      "InlineI" { $text = [string][BitConverter]::ToInt32($il, $i); $i += 4 }
      "InlineI8" { $text = [string][BitConverter]::ToInt64($il, $i); $i += 8 }
      "InlineR" { $text = [string][BitConverter]::ToDouble($il, $i); $i += 8 }
      "ShortInlineR" { $text = [string][BitConverter]::ToSingle($il, $i); $i += 4 }
      "InlineSwitch" {
        $n = [BitConverter]::ToInt32($il, $i); $i += 4; $base = $i + 4 * $n; $ts = @()
        for ($k = 0; $k -lt $n; $k++) { $ts += ("IL_" + ($base + [BitConverter]::ToInt32($il, $i + 4 * $k)).ToString("X4")) }
        $i = $base; $text = "(" + ($ts -join ", ") + ")"
      }
      "InlineString" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try { $member = $mod.ResolveString($tok); $text = "`"" + $member + "`"" } catch { $text = "string token 0x" + $tok.ToString("X8") + " unresolved, " + (Err $_.Exception) }
      }
      "InlineMethod" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try { $member = $mod.ResolveMethod($tok); $text = (TypeName $member.DeclaringType) + "::" + $member.Name + "(" + (ParamList $member) + ")" } catch { $text = "method token 0x" + $tok.ToString("X8") + " unresolved, " + (Err $_.Exception) }
      }
      "InlineField" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try { $member = $mod.ResolveField($tok); $text = (TypeName $member.DeclaringType) + "::" + $member.Name } catch { $text = "field token 0x" + $tok.ToString("X8") + " unresolved, " + (Err $_.Exception) }
      }
      "InlineType" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try { $member = $mod.ResolveType($tok); $text = TypeName $member } catch { $text = "type token 0x" + $tok.ToString("X8") + " unresolved, " + (Err $_.Exception) }
      }
      "InlineTok" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        try { $member = $mod.ResolveMember($tok); $text = [string]$member } catch { $text = "token 0x" + $tok.ToString("X8") + " unresolved, " + (Err $_.Exception) }
      }
      default { $text = "operand " + $oc.OperandType + " not decoded"; $i += 4 }
    }
    $list.Add([pscustomobject]@{ Offset = $at; Name = $oc.Name; Text = $text; Member = $member })
  }
  return ,$list
}
function InsLine($ins) {
  if ($ins.Offset -lt 0) { return ($ins.Name + ": " + $ins.Text) }
  return ("IL_" + $ins.Offset.ToString("X4") + "  " + $ins.Name.PadRight(10) + " " + $ins.Text)
}

# The guard code the probe shares with tools\loop\run.ps1, one copy, F103. Every function the
# main thread and the watchdog both use is in it, and the watchdog gets its text through $sync.
$guardFile = Join-Path (Split-Path $PSScriptRoot -Parent) "loop\nw-guard.ps1"
. $guardFile
$guardText = [System.IO.File]::ReadAllText($guardFile)

Say ("MACHINE   " + $env:COMPUTERNAME + "   " + $T0.ToString("yyyy-MM-dd HH:mm:ss"))
Say ("HOST      powershell " + $PSVersionTable.PSVersion + ", 64 bit process " + [Environment]::Is64BitProcess + ", apartment " + [System.Threading.Thread]::CurrentThread.ApartmentState + ", pid " + $PID)
Say ("SCRIPT    " + $PSCommandPath + ", " + (Get-Item -LiteralPath $PSCommandPath).Length + " bytes, sha256 " + (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash)

# E6. Both deadlines at least 60 seconds, and this script the one its own powershell.exe was
# started to run, so the constructor deadline's TerminateProcess can only ever end this
# probe's own process and never a caller's.
$refusals = New-Object System.Collections.Generic.List[string]
if ($ConstructorDeadlineSeconds -lt 60) { $refusals.Add("-ConstructorDeadlineSeconds is " + $ConstructorDeadlineSeconds + ", below 60") }
if ($AdoptedDeadlineSeconds -lt 60) { $refusals.Add("-AdoptedDeadlineSeconds is " + $AdoptedDeadlineSeconds + ", below 60") }
OwnProcessRefusal $PSCommandPath $refusals
if ($refusals.Count -gt 0) {
  foreach ($r in $refusals) { Say ("REFUSED: " + $r) }
  exit 1
}
Say ""
$gate = New-Object System.Collections.Generic.List[string]

# =======================================================================================
Say "==== STEP 1. Reflection only, nothing started ===="
if (-not (Test-Path -LiteralPath $autoDll)) { Say ("UNKNOWN: no Autodesk.Navisworks.Automation.dll at " + $autoDll + ". STOP"); exit 1 }
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
    Say ("  public " + $st + (TypeName $p.PropertyType) + " " + $p.Name + " { " + ($acc -join ", ") + " }")
  }
  foreach ($e in ($t.GetEvents($flags) | Sort-Object Name)) { Say ("  public event " + (TypeName $e.EventHandlerType) + " " + $e.Name) }
  foreach ($f in ($t.GetFields($flags) | Sort-Object Name)) {
    $st = ""; if ($f.IsStatic) { $st = "static " }
    Say ("  public " + $st + (TypeName $f.FieldType) + " " + $f.Name)
  }
  foreach ($m in ($t.GetMethods($flags) | Where-Object { -not $_.IsSpecialName } | Sort-Object Name)) {
    $vi = ""; if ($m.IsVirtual -and -not $m.IsFinal -and -not $t.IsInterface) { $vi = "virtual " }
    $st = ""; if ($m.IsStatic) { $st = "static " }
    Say ("  public " + $st + $vi + (TypeName $m.ReturnType) + " " + $m.Name + "(" + (ParamList $m) + ")")
  }
  Say ""
}

$napp = $asm.GetType("Autodesk.Navisworks.Api.Automation.NavisworksApplication")
if ($null -eq $napp) { Say "UNKNOWN: no type Autodesk.Navisworks.Api.Automation.NavisworksApplication in the assembly. STOP"; exit 1 }
$exec = $napp.GetMethod("ExecuteAddInPlugin")
if ($null -eq $exec) { $execSig = "UNKNOWN, no public ExecuteAddInPlugin on NavisworksApplication" } else { $execSig = Signature $exec }

Say "---- NavisworksApplication, every member public or not, declared on it ----"
foreach ($m in ($napp.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly") | Sort-Object Name)) { Say ("  " + (Signature $m)) }
foreach ($c in $napp.GetConstructors("Public,NonPublic,Instance")) { Say ("  " + (Visibility $c) + " .ctor(" + (ParamList $c) + ")") }
foreach ($f in $napp.GetFields("Public,NonPublic,Instance,Static,DeclaredOnly")) { Say ("  field " + (TypeName $f.FieldType) + " " + $f.Name) }
Say ""

# Every instruction, branches included, of the constructors, GetRunningInstance, Dispose,
# the finalizer and StayOpen, and of what they call inside this assembly, three levels.
Say "---- The IL of the constructors, GetRunningInstance, Dispose, Finalize and StayOpen, and what they call inside this assembly, three levels ----"
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
  foreach ($ins in (IlRead $m)) {
    Say ("        " + (InsLine $ins))
    if ($depth -lt 3 -and $ins.Member -is [System.Reflection.MethodBase] -and $ins.Member.Module -eq $m.Module) { $queue.Enqueue(@($ins.Member, ($depth + 1))) }
  }
}
Say ""

Say "---- Which COM class the native half creates ----"
$bytes = [System.IO.File]::ReadAllBytes($autoDll)
$docClsid = $null
try { $docClsid = (Get-ItemProperty -LiteralPath "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Navisworks.Document\CLSID").'(default)' } catch { Say ("  reading HKLM Classes Navisworks.Document CLSID threw, " + (Err $_.Exception)) }
if ($null -eq $docClsid) { $gate.Add("the Navisworks.Document CLSID could not be read"); Say "  UNKNOWN: no HKLM Classes Navisworks.Document CLSID" } else {
  $clsKey = "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\" + $docClsid
  foreach ($sub in @("ProgID", "VersionIndependentProgID", "LocalServer32", "InprocServer32")) {
    $val = "absent"
    if (Test-Path -LiteralPath ($clsKey + "\" + $sub)) {
      try { $val = [string](Get-ItemProperty -LiteralPath ($clsKey + "\" + $sub)).'(default)' } catch { $val = "unreadable, " + (Err $_.Exception) }
    }
    Say ("  CLSID " + $docClsid + " " + $sub + ": " + $val)
  }
  $needle = ([Guid]$docClsid).ToByteArray()
  $hits = @()
  for ($i = 0; $i -le $bytes.Length - 16; $i++) {
    if ($bytes[$i] -ne $needle[0]) { continue }
    $ok = $true
    for ($j = 1; $j -lt 16; $j++) { if ($bytes[$i + $j] -ne $needle[$j]) { $ok = $false; break } }
    if ($ok) { $hits += $i }
  }
  Say ("  that CLSID's 16 bytes appear in the DLL at offsets: " + $(if ($hits.Count -gt 0) { $hits -join ", " } else { "none" }))
  if ($hits.Count -eq 0) { $gate.Add("the DLL does not carry the Navisworks.Document CLSID, so which class it creates is UNKNOWN") }
}
# The DLL's own import table, read off the file. A native import can be made by ordinal, and
# then it carries no name, so each ordinal is named off the exporting DLL's own export
# table, read from System32 by its one full path. A search of the file for a name, which
# this replaces, can never see an import by ordinal.
function PeRead($file) {
  $b = [System.IO.File]::ReadAllBytes($file)
  $pe = [BitConverter]::ToInt32($b, 0x3C)
  $nsec = [BitConverter]::ToUInt16($b, $pe + 6)
  $optSize = [BitConverter]::ToUInt16($b, $pe + 20)
  $opt = $pe + 24
  $magic = [BitConverter]::ToUInt16($b, $opt)
  if ($magic -eq 0x20B) { $dd = $opt + 112; $thunk = 8 } else { $dd = $opt + 96; $thunk = 4 }
  $secs = New-Object System.Collections.Generic.List[object]
  for ($i = 0; $i -lt $nsec; $i++) { $s = $opt + $optSize + 40 * $i; $secs.Add(@([BitConverter]::ToUInt32($b, $s + 12), [BitConverter]::ToUInt32($b, $s + 8), [BitConverter]::ToUInt32($b, $s + 20), [BitConverter]::ToUInt32($b, $s + 16))) }
  return [pscustomobject]@{ Bytes = $b; Dd = $dd; Thunk = $thunk; Secs = $secs }
}
function PeOff($pe, [uint64]$rva) {
  foreach ($s in $pe.Secs) { $size = [Math]::Max([uint64]$s[1], [uint64]$s[3]); if ($rva -ge $s[0] -and $rva -lt ($s[0] + $size)) { return [int]($rva - $s[0] + $s[2]) } }
  throw ("RVA 0x" + $rva.ToString("X") + " is in no section")
}
function PeStr($pe, [int]$o) { $e = $o; while ($pe.Bytes[$e] -ne 0) { $e++ }; return [System.Text.Encoding]::ASCII.GetString($pe.Bytes, $o, $e - $o) }
function PeImports($file) {
  $pe = PeRead $file; $b = $pe.Bytes
  $list = New-Object System.Collections.Generic.List[object]
  $rva = [BitConverter]::ToUInt32($b, $pe.Dd + 8)
  if ($rva -eq 0) { return $list }
  $o = PeOff $pe $rva
  while ($true) {
    $oft = [BitConverter]::ToUInt32($b, $o); $nameRva = [BitConverter]::ToUInt32($b, $o + 12); $ft = [BitConverter]::ToUInt32($b, $o + 16)
    if ($nameRva -eq 0) { break }
    $dll = PeStr $pe (PeOff $pe $nameRva)
    if ($oft -ne 0) { $t = PeOff $pe $oft } else { $t = PeOff $pe $ft }
    while ($true) {
      if ($pe.Thunk -eq 8) { $v = [BitConverter]::ToUInt64($b, $t); $byOrd = (($v -shr 63) -eq 1) } else { $v = [uint64][BitConverter]::ToUInt32($b, $t); $byOrd = (($v -shr 31) -eq 1) }
      if ($v -eq 0) { break }
      if ($byOrd) { $list.Add([pscustomobject]@{ Dll = $dll; Name = $null; Ordinal = [int]($v -band 0xFFFF) }) }
      else { $list.Add([pscustomobject]@{ Dll = $dll; Name = (PeStr $pe ((PeOff $pe ($v -band 0x7FFFFFFF)) + 2)); Ordinal = $null }) }
      $t += $pe.Thunk
    }
    $o += 20
  }
  return $list
}
function PeExports($file) {
  $pe = PeRead $file; $b = $pe.Bytes
  $map = @{}
  $rva = [BitConverter]::ToUInt32($b, $pe.Dd)
  if ($rva -eq 0) { return $map }
  $ed = PeOff $pe $rva
  $base = [BitConverter]::ToUInt32($b, $ed + 16)
  $nNames = [BitConverter]::ToUInt32($b, $ed + 24)
  $aNames = PeOff $pe ([BitConverter]::ToUInt32($b, $ed + 32))
  $aOrd = PeOff $pe ([BitConverter]::ToUInt32($b, $ed + 36))
  for ($i = 0; $i -lt $nNames; $i++) { $map[[int]($base + [BitConverter]::ToUInt16($b, $aOrd + 2 * $i))] = PeStr $pe (PeOff $pe ([BitConverter]::ToUInt32($b, $aNames + 4 * $i))) }
  return $map
}
Say "  the DLL's import table, every ordinal named off the exporting DLL's export table:"
# What this run read of each name below, kept for the named limits at the end. A name
# missing here was not read on this run.
$importRead = @{}
try {
  $imports = @(PeImports $autoDll)
  $exportMaps = @{}
  foreach ($dll in @($imports | Where-Object { $null -ne $_.Ordinal } | ForEach-Object { $_.Dll } | Sort-Object -Unique)) {
    $sysDll = Join-Path ([Environment]::SystemDirectory) $dll
    if (Test-Path -LiteralPath $sysDll) { $exportMaps[$dll] = PeExports $sysDll } else { Say ("  UNKNOWN: " + $dll + " is not at " + $sysDll + ", so its ordinals stay unnamed") }
  }
  $importNames = New-Object System.Collections.Generic.List[string]
  foreach ($dll in @($imports | ForEach-Object { $_.Dll } | Sort-Object -Unique)) {
    $items = @()
    foreach ($im in @($imports | Where-Object { $_.Dll -eq $dll })) {
      if ($null -ne $im.Name) { $items += $im.Name; $importNames.Add($dll + "!" + $im.Name) }
      else {
        $nm = "unnamed"
        if ($exportMaps.ContainsKey($dll) -and $exportMaps[$dll].ContainsKey($im.Ordinal)) { $nm = $exportMaps[$dll][$im.Ordinal]; $importNames.Add($dll + "!" + $nm) }
        $items += ("#" + $im.Ordinal + " " + $nm)
      }
    }
    Say ("    " + $dll + ", " + $items.Count + ": " + ($items -join ", "))
  }
  foreach ($w in @("CoCreateInstance", "GetActiveObject", "CoGetClassObject", "CreateProcessW", "ShellExecuteW")) {
    $hit = @($importNames | Where-Object { $_ -like ("*!" + $w) })
    $importRead[$w] = "imported: " + ($hit.Count -gt 0) + $(if ($hit.Count -gt 0) { ", from " + (($hit | ForEach-Object { $_.Split('!')[0] }) -join ", ") } else { "" })
    Say ("  " + $w.PadRight(18) + " " + $importRead[$w])
  }
  if (@($importNames | Where-Object { $_ -like "*!CoCreateInstance" }).Count -eq 0) { $gate.Add("CoCreateInstance is not in the DLL's import table") }
} catch {
  $gate.Add("the DLL's import table could not be read, " + (Err $_.Exception))
  Say ("  UNKNOWN: the import table could not be read, " + (Err $_.Exception))
}
Say ""

# Whether that COM class could be served by a Navisworks already running decides whether
# step 3 could reach one that is not ours. Read off Roamer's own command line parser and
# Roamer.exe, in reflection only, so no code in these assemblies runs. The coverage is
# CommandLineParser's methods and NetRoamer.Program's methods, and nothing wider.
Say "---- Which command line makes a Roamer the COM automation server, reflection only ----"
$roamerGui = Join-Path $nw "navisworks.gui.roamer.dll"
$roamerExe = Join-Path $nw "Roamer.exe"
$roResolve = [ResolveEventHandler]{
  param($s, $e)
  $n = New-Object System.Reflection.AssemblyName($e.Name)
  $cand = Join-Path $nw ($n.Name + ".dll")
  if (Test-Path -LiteralPath $cand) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand) }
  try { return [System.Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { $script:resolveFailures.Add($e.Name + ": " + $_.Exception.Message); return $null }
}
[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($roResolve)
if (-not (Test-Path -LiteralPath $roamerGui)) { $gate.Add("no navisworks.gui.roamer.dll"); Say ("  UNKNOWN: no navisworks.gui.roamer.dll at " + $roamerGui) } else {
  $ga = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($roamerGui)
  $parser = $ga.GetType("Autodesk.Navisworks.Gui.Roamer.CommandLineParser")
  if ($null -eq $parser) { $gate.Add("no CommandLineParser type"); Say "  UNKNOWN: no Autodesk.Navisworks.Gui.Roamer.CommandLineParser" } else {
    $options = New-Object System.Collections.Generic.List[string]
    $stores = 0
    $storeStrings = @()
    foreach ($m in $parser.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
      $ins = IlRead $m
      $recent = New-Object System.Collections.Generic.List[string]
      for ($k = 0; $k -lt $ins.Count; $k++) {
        $x = $ins[$k]
        if ($x.Name -eq "error" -or $x.Name -eq "nobody") {
          if ($x.Name -eq "error") { $gate.Add("the IL walk of CommandLineParser::" + $m.Name + " failed: " + $x.Text) }
          Say ("  " + $parser.Name + "::" + $m.Name + " " + $x.Text)
        }
        if ($x.Text -match "unresolved") {
          $gate.Add("CommandLineParser::" + $m.Name + " IL_" + $x.Offset.ToString("X4") + " has an unresolved token")
          Say ("  " + $parser.Name + "::" + $m.Name + " IL_" + $x.Offset.ToString("X4") + " " + $x.Text)
        }
        if ($x.Name -eq "ldstr" -and $x.Member -is [string]) { $recent.Add($x.Member) }
        if (($x.Name -eq "call" -or $x.Name -eq "callvirt") -and $x.Member -is [System.Reflection.MethodBase] -and $x.Member.Name -eq "MatchOption" -and $k -gt 0 -and $ins[$k - 1].Name -eq "ldstr" -and -not $options.Contains([string]$ins[$k - 1].Member)) { $options.Add([string]$ins[$k - 1].Member) }
        if ($x.Name -eq "stfld" -and $x.Member -is [System.Reflection.FieldInfo]) {
          if ($x.Member.Name -eq "COMAutomationStartup") {
            $stores++
            $storeStrings = @($recent)
            Say ("  " + $parser.Name + "::" + $m.Name + " IL_" + $x.Offset.ToString("X4") + " stores " + $x.Text + " after the strings, since the store before it: " + (($recent | ForEach-Object { "`"" + $_ + "`"" }) -join ", "))
            foreach ($y in $ins) { if ($y.Offset -ge ($x.Offset - 0x30) -and $y.Offset -le $x.Offset) { Say ("        " + (InsLine $y)) } }
          }
          $recent.Clear()
        }
      }
    }
    Say ("  stores of COMAutomationStartup in CommandLineParser: " + $stores)
    if ($stores -ne 1) { $gate.Add("CommandLineParser stores COMAutomationStartup " + $stores + " times, not once") }
    $exact = ($storeStrings.Count -eq 2 -and ($storeStrings -ccontains "Embedding") -and ($storeStrings -ccontains "Automation"))
    Say ("  the strings before that store are exactly Embedding and Automation: " + $exact)
    if (-not $exact) { $gate.Add("the strings before the COMAutomationStartup store are not exactly Embedding and Automation") }
    Say ("  every option name CommandLineParser matches, " + $options.Count + ": " + ($options -join ", "))
  }
}
if (-not (Test-Path -LiteralPath $roamerExe)) { $gate.Add("no Roamer.exe"); Say ("  UNKNOWN: no Roamer.exe at " + $roamerExe) } else {
  $ra = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($roamerExe)
  $prog = $ra.GetType("NetRoamer.Program")
  if ($null -eq $prog) { $gate.Add("no NetRoamer.Program in Roamer.exe"); Say "  UNKNOWN: no NetRoamer.Program in Roamer.exe" } else {
    $reads = 0
    foreach ($m in $prog.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
      $ins = IlRead $m
      for ($k = 0; $k -lt $ins.Count; $k++) {
        $x = $ins[$k]
        if ($x.Name -eq "error") { $gate.Add("the IL walk of NetRoamer.Program::" + $m.Name + " failed: " + $x.Text); Say ("  " + $prog.Name + "::" + $m.Name + " " + $x.Text) }
        if ($x.Name -match "^(ldfld|ldflda|stfld|ldsfld|ldsflda|stsfld)$" -and $x.Text -match "unresolved") { $gate.Add("NetRoamer.Program::" + $m.Name + " IL_" + $x.Offset.ToString("X4") + " has an unresolved field"); Say ("  " + $prog.Name + "::" + $m.Name + " IL_" + $x.Offset.ToString("X4") + " " + $x.Text) }
        if ($x.Member -is [System.Reflection.FieldInfo] -and $x.Member.Name -eq "COMAutomationStartup") {
          $reads++
          Say ("  Roamer.exe " + $prog.Name + "::" + $m.Name + " touches COMAutomationStartup, the instruction and the two after it:")
          for ($q = $k; $q -lt [Math]::Min($k + 3, $ins.Count); $q++) { Say ("        " + (InsLine $ins[$q])) }
        }
      }
    }
    Say ("  instructions in NetRoamer.Program that touch COMAutomationStartup: " + $reads)
    if ($reads -eq 0) { $gate.Add("NetRoamer.Program never touches COMAutomationStartup") }
  }
}
if ($script:resolveFailures.Count -gt 0) { Say ("  references that could not be loaded for reflection: " + ($script:resolveFailures -join " | ")) }
Say ""

Say "---- STEP 1 VERDICT ----"
if ($gate.Count -eq 0) { Say "  every verdict read, none UNKNOWN" } else { foreach ($g in $gate) { Say ("  UNKNOWN: " + $g) } }
Say ""
if ($ReflectionOnly) { Say "ReflectionOnly: stopped after step 1, nothing was started"; exit 0 }
if ($gate.Count -gt 0) { Say "STOP before anything starts, because a verdict above is UNKNOWN"; exit 1 }

# =======================================================================================
$wt = NewWinTypes
$procType = $wt.ProcType
$winType = $wt.WinType

# =======================================================================================
Say "==== THE WORK FOLDER ===="
if (-not $work.StartsWith($loopRoot + "\", [StringComparison]::OrdinalIgnoreCase)) { Say ("STOP before step 2, the work folder " + $work + " is not under " + $loopRoot); exit 1 }
if (Test-Path -LiteralPath $work) {
  $keptName = "automation-start-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
  try { Rename-Item -LiteralPath $work -NewName $keptName -ErrorAction Stop; Say ("  a folder from an earlier run was there, kept by renaming it " + $keptName) }
  catch { Say ("STOP before step 2: the work folder from an earlier run could not be renamed, " + (Err $_.Exception) + " Nothing was touched"); exit 1 }
}
try { New-Item -ItemType Directory -Path $work -ErrorAction Stop | Out-Null; Say "  created empty" }
catch { Say ("STOP before step 2: the work folder could not be created, " + (Err $_.Exception)); exit 1 }
$watchFile = Join-Path $work "watch.txt"
$pidFile = Join-Path $work "mypid.txt"
[System.IO.File]::WriteAllText($watchFile, "", $utf8)
Say ""

# =======================================================================================
Say "==== STEP 2. Starts this probe could not prove, then every Roamer running before anything starts ===="
$outcome = [ordered]@{ "1" = "passed, every verdict read"; "2" = "started, did not finish"; "3" = "not reached"; "4" = "not reached"; "5" = "not reached"; "6" = "not reached" }
$currentStep = "2"
$u = UnprovedRefusal $unproved
if ($u.Stop) { Say $u.Text; exit 1 }
# Any Roamer at all stops the probe here, so these stay empty. The watchdog and the end
# still read them.
$beforeStart = @{}
$beforeTicks = @{}
if (RoamerRefusal) {
  Say "STOP before the watchdog, the backup and the constructor, so nothing is written down and nothing is started: a Navisworks is running, and no start is made while any Navisworks runs, whatever its command line and whoever started it"
  exit 1
}
$beforeAll = @(Get-Process | ForEach-Object { $_.Id })
Say ("  every process id on the machine taken as the before set: " + $beforeAll.Count)
$outcome["2"] = "passed, no Roamer running"
Say ""

# =======================================================================================
# The watchdog starts now, before the backup, so every Roamer from the backup on is seen.
$sync = WatchSync $beforeAll $beforeTicks $T0 $loopRoot $nw $ConstructorDeadlineSeconds $AdoptedDeadlineSeconds $Out $unproved $watchFile $guardText $winType $procType
$watchScript = WatchdogScript
$wps = [PowerShell]::Create()
[void]$wps.AddScript($watchScript).AddArgument($sync)
$whandle = $wps.BeginInvoke()
function StopEarly($msg) {
  Say $msg
  $sync.Stop = $true
  try { [void]$wps.EndInvoke($whandle) } catch { Say ("  the watchdog ended with " + (Err $_.Exception)) }
  exit 1
}

# =======================================================================================
Say "==== BADER'S SETTINGS, backed up before anything starts ===="
$regSub = "Software\Autodesk\Navisworks Manage\22.0"
$nwAppData = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025"
$fedLogs = Join-Path $env:LOCALAPPDATA "ParsonsNwcFederator\logs"
$bs = BackupSettings $work $regSub $nwAppData $fedLogs
if (-not $bs.Ok) { StopEarly $bs.Why }
$regRoot = $bs.RegRoot
$regBefore = $bs.RegBefore
$appBackup = $bs.AppBackup
$filesBefore = $bs.FilesBefore
$autoBefore = $bs.AutoBefore
$notBacked = $bs.NotBacked
$fedBefore = $bs.FedBefore
$sync.RegSub = $regSub
$sync.RegRoot = $regRoot
$sync.RegBefore = $regBefore
$sync.FilesBefore = $filesBefore
$sync.NotBacked = $notBacked
$sync.AutoBefore = $autoBefore
$sync.NwAppData = $nwAppData
$sync.SettingsReady = $true
Say ""

$app = $null
$myPid = 0
$myStart = $null
$myTicks = $null
$goneAtUtc = $null
$disposed = $false
$suppressed = $false
$ctorSeconds = $null
$startAfterCall = $null
# The same read as step 2, made again after the backups, as the last read before the try
# that calls the constructor. It is outside that try, so a refusal runs no close and no put
# back, and writes nothing down. Only the Auto-Save switch comes after it, F138, Q135 point 2,
# so no stop before the start but the switch's own refusal comes after the write, and that
# refusal's line says what it left of his.
Say "==== THE LAST READ BEFORE THE CONSTRUCTOR, the same as step 2 ===="
if (RoamerRefusal) { StopEarly "STOP before the constructor, so nothing is started and nothing of Bader's is written, the backup stays in the work folder: a Navisworks is running, and no start is made while any Navisworks runs, whatever its command line and whoever started it" }
$ao = SwitchAutoSaveOff $regSub $regRoot $regBefore
if (-not $ao.Ok) { StopEarly ("STOP before the constructor, so nothing is started, the backup stays in the work folder: " + $ao.Line) }
Say ("  " + $ao.Line)
Say ""
try {
  # =====================================================================================
  Say "==== STEP 3. Start one Navisworks through the API ===="
  $currentStep = "3"
  $outcome["3"] = "started, did not finish"
  Say ("  " + (Stamp) + "  calling new NavisworksApplication()")
  $sync.CallStartUtc = [DateTime]::UtcNow
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app = [Activator]::CreateInstance($napp) } catch { $err = $_.Exception }
  $sync.CtorReturned = $true
  $sw.Stop()
  $ctorSeconds = $sw.Elapsed.TotalSeconds
  $returnedAtUtc = [DateTime]::UtcNow
  Say ("  " + (Stamp) + "  the constructor " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $ctorSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }

  $ad = AdoptStart $err $app $sync $pidFile
  if (-not $ad.Adopted) {
    if ($ad.Suppressed) { $suppressed = $true }
    $c1 = $ad.C1; $c2 = $ad.C2; $c3 = $ad.C3; $c4 = $ad.C4
    $outcome["3"] = "failed, not adopted: returned " + $c1 + ", one possible start " + $c2 + ", after the call " + $c3 + ", -Embedding " + $c4
    throw "step 3 stopped"
  }
  $myPid = $ad.Pid
  $myStart = $ad.Start
  $myTicks = $ad.Ticks
  $startAfterCall = ($myStart.ToUniversalTime() - $sync.CallStartUtc).TotalSeconds
  Say ("  ADOPTED, MY PID IS " + $myPid + ", started " + $myStart.ToString("yyyy-MM-dd HH:mm:ss.fff") + ", start ticks UTC " + $myTicks)
  Say ("  the process started " + $startAfterCall.ToString("0.00") + " s after the call began and " + ($returnedAtUtc - $myStart.ToUniversalTime()).TotalSeconds.ToString("0.00") + " s before the constructor returned")
  $mp = Get-Process -Id $myPid
  Say ("  MainWindowHandle " + $mp.MainWindowHandle + ", MainWindowTitle `"" + $mp.MainWindowTitle + "`"")
  WindowsOf $myPid $myTicks
  $v0 = $app.Visible
  Say ("  " + (Stamp) + "  Visible read BEFORE setting it: " + $v0)
  $app.Visible = $true
  Start-Sleep -Milliseconds 1500
  $v1 = $app.Visible
  $mp.Refresh()
  Say ("  " + (Stamp) + "  set Visible = True, read back: " + $v1 + ", MainWindowHandle " + $mp.MainWindowHandle + ", title `"" + $mp.MainWindowTitle + "`"")
  WindowsOf $myPid $myTicks
  $app.Visible = $false
  Start-Sleep -Milliseconds 1500
  $v2 = $app.Visible
  $mp.Refresh()
  Say ("  " + (Stamp) + "  set Visible = False, read back: " + $v2 + ", MainWindowHandle " + $mp.MainWindowHandle)
  WindowsOf $myPid $myTicks
  $outcome["3"] = "passed"
  Say ""

  # =====================================================================================
  Say "==== STEP 4. Copy one NWC from the loop's source copy and open the copy ===="
  $currentStep = "4"
  $outcome["4"] = "started, did not finish"
  if ($Nwc -eq "") {
    $pick = Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter *.nwc | Sort-Object Length, FullName | Select-Object -First 1
    if ($null -eq $pick) { $outcome["4"] = "failed, no NWC in the source copy"; throw ("no NWC under " + $sourceRoot) }
    $srcFile = $pick.FullName
    $pickWhy = "the smallest NWC in the source copy"
  } else { $srcFile = Join-Path $sourceRoot $Nwc; $pickWhy = "the NWC named by -Nwc" }
  $full = [System.IO.Path]::GetFullPath($srcFile)
  if (-not $full.StartsWith($sourceRoot + "\", [StringComparison]::OrdinalIgnoreCase)) { $outcome["4"] = "refused, not under the source copy"; throw ("refused, the file is not under " + $sourceRoot) }
  $copy = Join-Path $work (Split-Path $full -Leaf)
  Copy-Item -LiteralPath $full -Destination $copy -Force
  $h1 = (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash
  $h2 = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
  Say ("  source " + (Mask $full) + ", " + (Get-Item -LiteralPath $full).Length + " bytes, " + $pickWhy)
  Say ("  copy   " + (Mask $copy) + ", " + (Get-Item -LiteralPath $copy).Length + " bytes, sha256 " + $(if ($h1 -eq $h2) { "matches" } else { "DIFFERS" }))
  Say ("  " + (Stamp) + "  calling OpenFile(copy, no more files)")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.OpenFile($copy, [string[]]@()) } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  OpenFile " + $(if ($null -eq $err) { "RETURNED, it is void so there is no value" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }
  WindowsOf $myPid $myTicks
  if ($null -ne $err) { $outcome["4"] = "failed, OpenFile threw"; throw "step 4 stopped" }
  # OpenFile returns nothing, so the one outside sign that the model loaded is a save of
  # what is open, into the work folder created empty at the start, written after the call.
  $nwdOut = Join-Path $work "opened-copy-saved.nwd"
  Say ("  " + (Stamp) + "  check: SaveFile of what is open to " + (Mask $nwdOut))
  $saveStart = [DateTime]::Now
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.SaveFile($nwdOut) } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  SaveFile " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }
  $nwdOk = $false
  if ([System.IO.File]::Exists($nwdOut)) {
    $fi = New-Object System.IO.FileInfo($nwdOut)
    $nwdOk = ($fi.LastWriteTime -ge $saveStart)
    Say ("  the NWD exists, " + $fi.Length + " bytes read back off the disk, written " + $fi.LastWriteTime.ToString("HH:mm:ss.fff") + ", the call began " + $saveStart.ToString("HH:mm:ss.fff") + ", written after the call began: " + $nwdOk)
  } else { Say "  the NWD does NOT exist" }
  $h3 = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
  Say ("  the NWC copy after the open: sha256 " + $(if ($h3 -eq $h2) { "unchanged" } else { "CHANGED" }))
  if ($null -ne $err -or -not $nwdOk) { $outcome["4"] = "failed, the save check did not pass"; throw "step 4 stopped" }
  $outcome["4"] = "passed"
  Say ""

  # =====================================================================================
  Say "==== STEP 5. AddPluginAssembly with the add-in built from this repo ===="
  $currentStep = "5"
  $outcome["5"] = "started, did not finish"
  if (-not (Test-Path -LiteralPath $PluginAssembly)) { $outcome["5"] = "failed, no built add-in"; throw ("no built add-in at " + $PluginAssembly) }
  $pa = [System.Reflection.AssemblyName]::GetAssemblyName($PluginAssembly)
  $info = "UNKNOWN"
  try { $rpa = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($PluginAssembly); foreach ($cad in $rpa.GetCustomAttributesData()) { if ($cad.AttributeType.Name -eq "AssemblyInformationalVersionAttribute") { $info = [string]$cad.ConstructorArguments[0].Value } } } catch { $info = "UNKNOWN, " + (Err $_.Exception) }
  Say ("  " + $PluginAssembly)
  Say ("  " + (Get-Item -LiteralPath $PluginAssembly).Length + " bytes, written " + (Get-Item -LiteralPath $PluginAssembly).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + ", " + $pa.FullName + ", stamp " + $info)
  Say ("  " + (Stamp) + "  calling AddPluginAssembly")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.AddPluginAssembly($PluginAssembly) } catch { $err = $_.Exception }
  $sw.Stop()
  Say ("  " + (Stamp) + "  AddPluginAssembly " + $(if ($null -eq $err) { "RETURNED, it is void so there is no value" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.000") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }
  WindowsOf $myPid $myTicks
  Say ("  ExecuteAddInPlugin, read off the reflection: " + $execSig + ". It was NOT called")
  if ($null -ne $err) { $outcome["5"] = "failed, AddPluginAssembly threw"; throw "step 5 stopped" }
  $outcome["5"] = "passed"
  Say ""

  # =====================================================================================
  Say "==== STEP 6. Quit through the API ===="
  $currentStep = "6"
  $outcome["6"] = "started, did not finish"
  Say ("  " + (Stamp) + "  calling Dispose()")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app.Dispose(); $disposed = $true } catch { $err = $_.Exception }
  $sw.Stop()
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
    Say ("  " + (Stamp) + "  pid " + $myPid + " was STILL RUNNING " + $QuitWaitSeconds + " s after Dispose. It HAD TO BE FORCED through the held handle: " + $cr6.Text + ". State after: " + $stAfter)
    $outcome["6"] = "failed, had to be forced"
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
  # Read before the end stops the watchdog: if it has ended already, it stopped early.
  $watchEndedEarly = $whandle.IsCompleted
  $sync.Stop = $true
  Say "==== FINALLY ===="
  Say ("  " + (Stamp) + "  " + $closeNote)
  $wpsEndError = $null
  try { [void]$wps.EndInvoke($whandle) } catch { $wpsEndError = (Err $_.Exception); Say ("  the watchdog ended with " + $wpsEndError) }
  $wpsErrors = @($wps.Streams.Error)
  $wps.Dispose()
  Say ("  watchdog forced anything: " + $(if ($sync.Forced -eq "") { "no" } else { $sync.Forced }))
  Say ("  watchdog passes " + $sync.Passes + ", longest pass " + ([double]$sync.PassMaxMs).ToString("0") + " ms, mean pass " + $(if ($sync.Passes -gt 0) { ($sync.PassTotalMs / $sync.Passes).ToString("0") } else { "0" }) + " ms, longest gap between the starts of two passes " + ([double]$sync.MaxGapMs).ToString("0") + " ms, error lines " + $sync.ErrorCount + ", passes that failed before their process loop " + $sync.EarlyFails + ", errors in its runspace " + $wpsErrors.Count + ", lines it could not write " + $sync.WriteErrors.Count)
  foreach ($we in $wpsErrors) { Say ("    runspace error: " + $we.ToString()) }
  foreach ($we in $sync.WriteErrors) { Say ("    could not write: " + $we) }
  Say ""
  Say "---- the watchdog's record ----"
  try { foreach ($l in [System.IO.File]::ReadAllLines($watchFile)) { Say ("  " + $l) } } catch { Say ("  the watchdog's record could not be read, " + (Err $_.Exception)) }
  Say ""
  Say "---- every process the watchdog recorded, at the end ----"
  foreach ($s in $sync.Seen) {
    $state = "UNKNOWN"
    if ($s.Ticks -eq 0) { $state = "UNKNOWN whether it exited, its start time could not be read when it was seen" } else {
      $pp = Get-Process -Id $s.Id -ErrorAction SilentlyContinue
      if ($null -eq $pp) { $state = "exited" } else {
        try {
          $pst = $pp.StartTime
          if ($null -eq $pst) { $state = "UNKNOWN, a process with that id is running and its start time reads as nothing" }
          elseif ((UtcTicks $pst) -eq $s.Ticks) { $state = "STILL RUNNING" } else { $state = "exited, the id is now another process" }
        } catch { $state = "UNKNOWN, " + (Err $_.Exception) }
      }
    }
    Say ("  " + $s.Name + " pid " + $s.Id + ", parent " + $s.Parent + ": " + $state)
  }
  if ($sync.Seen.Count -eq 0) { Say "  none" }
  Say ""
  Say "---- the Roamers from step 2, at the end ----"
  foreach ($id in $beforeTicks.Keys) { Say ("  pid " + $id + " started " + $beforeStart[$id].ToString("yyyy-MM-dd HH:mm:ss") + ": state " + (ProcState $id $beforeTicks[$id]) + ", never touched") }
  if ($beforeTicks.Count -eq 0) { Say "  there were none" }
  $lateNew = @(NewRoamers)
  Say ("  Roamers running now that were not in step 2: " + $lateNew.Count)
  foreach ($p in $lateNew) { Say ("    pid " + $p.Id + ", not closed by this probe") }
  Say ""

  Say "---- STARTS THIS PROBE COULD NOT PROVE, written down for later runs ----"
  UnprovedAtEnd $unproved $myPid $myTicks $lateNew $sync
  Say ""

  Say "---- NAMED LIMITS ----"
  Say ("  the longest gap between the starts of two watchdog passes was " + ([double]$sync.MaxGapMs).ToString("0") + " ms. A Navisworks that started and exited inside one gap was not seen by the settings check")
  if ($null -ne $ctorSeconds) { Say ("  this run's constructor took " + $ctorSeconds.ToString("0.00") + " s" + $(if ($null -ne $startAfterCall) { ", and its Navisworks process started " + $startAfterCall.ToString("0.00") + " s after the call" } else { "" })) }
  $gao = "UNKNOWN, step 1 did not read it on this run"
  if ($importRead.ContainsKey("GetActiveObject")) { $gao = $importRead["GetActiveObject"] }
  Say ("  of whether a running Navisworks can be reached, step 1 read the Automation DLL's import table on this run for GetActiveObject, " + $gao + ". Which call the constructor's IL takes when its argument is false is UNKNOWN, because no code on this run reads that branch")
  if ($null -ne $app -and -not $disposed -and -not $suppressed) { Say "  Dispose did not complete and the finalizer was not suppressed, so at this process's exit it calls Bridge.Terminate, against a process that is gone if it was closed. What that does is UNKNOWN" }
  Say ""

  Say "---- BADER'S SETTINGS, compared, and put back only when no other Navisworks ran ----"
  try {
    $pr = PutBackReasons $sync $wpsErrors $wpsEndError $watchEndedEarly $beforeTicks $myPid $myTicks $goneAtUtc
    $why = $pr.Why
    $goneAtUtc = $pr.GoneAtUtc
    $putBack = ($why.Count -eq 0)
    $null = SettingsPutBack $putBack $why $work $regSub $regBefore $regRoot $nwAppData $filesBefore $notBacked $autoBefore $appBackup
  } catch { Say ("  the compare stopped, " + (Err $_.Exception) + ". Nothing more is written, the backup is kept in the work folder") }
  $fedAfter = SettingsRead $fedLogs
  $gch = 0
  foreach ($k in $fedAfter.Files.Keys) { if (-not $fedBefore.Files.ContainsKey($k) -or $fedBefore.Files[$k].Hash -ne $fedAfter.Files[$k].Hash) { $gch++ } }
  Say ("  files in %LOCALAPPDATA%\ParsonsNwcFederator\logs added or changed: " + $gch)
  Say ""
  Say "---- the work folder at the end, sorted by name ----"
  foreach ($f in (Get-ChildItem -LiteralPath $work -File | Sort-Object Name)) { Say ("  " + $f.Name + "  " + $f.Length + " bytes") }
  Say ("  and " + @(Get-ChildItem -LiteralPath $appBackup -Recurse -File -ErrorAction SilentlyContinue).Count + " files under appdata-before")
  Say ""
  Say "---- outcome ----"
  foreach ($s in $outcome.Keys) { Say ("  step " + $s + " " + $outcome[$s]) }
  Say ("  lines that could not be written to the result file: " + $script:sayFailures)
  Say ("  finished " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + ", " + ([DateTime]::Now - $T0).TotalSeconds.ToString("0") + " s in all")
}
