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
# RUN IT ONLY WHEN NO NAVISWORKS THE LOOP DID NOT START IS RUNNING. The lead's decision D4
# of 2026-09-28. The probe itself does not refuse a Navisworks started by hand, it records
# it and puts nothing back, so the person running it lists the Roamers first.
#
# THE OWNERSHIP RULE. A Navisworks this probe did not start is never closed, attached to or
# sent anything. A Roamer is adopted as ours only when ALL of these hold: the constructor
# returned without throwing, exactly one Roamer is new since step 2, its start time is at
# or after the moment the constructor was called, and its command line holds -Embedding,
# which is how COM starts a local server. NOTHING IS EVER CLOSED BEFORE ADOPTION. When
# adoption fails for any reason, nothing is closed, nothing is called on the object, its
# finalizer is suppressed, every new Roamer's pid and start ticks are written to
# %LOCALAPPDATA%\NwcFederatorLoop\probes\unproved-starts.txt with the reason, and it stops.
# A process is only ever closed after its start time is read again and still matches.
#
# The steps, in order, stopping at the first that fails:
#   1  reflection only, nothing started: the Automation DLL, the COM class its native half
#      creates, and the Roamer command line that makes a Roamer that class's server. Any
#      verdict here that comes out UNKNOWN stops the probe before anything starts
#   2  unproved-starts.txt is read first, and the probe refuses while any start named there
#      is still running with the same start ticks, because a person has to look. Then every
#      Roamer already running, with its id, start time, parent, and whether its command line
#      holds the word embedding or automation in any case anywhere, read through
#      Win32_Process, which touches nothing in that Navisworks. Its command line itself is
#      never printed. If any holds either word, or cannot be read, the probe stops
#   3  one Navisworks started through the API, adopted by the rule above
#   4  one NWC copied from %LOCALAPPDATA%\NwcFederatorLoop\source into the work folder and
#      opened through the API, then what is open saved as an NWD whose write time is
#      checked against the call
#   5  AddPluginAssembly with the add-in built from this repo. ExecuteAddInPlugin is NOT
#      called, because the tool's own plugin opens its window
#   6  quit through the API with Dispose. If the adopted id is still the same process
#      -QuitWaitSeconds later, that id and no other is closed with Stop-Process
#
# Steps 3 to 6 sit in one try whose finally closes the adopted id, if it is still the same
# process, BEFORE it writes anything, so a failed write cannot leave it running. So the
# adopted Roamer is quit by Dispose, and closed by its id only when Dispose leaves it
# running, a step failed, or the adopted deadline passed.
#
# THE WATCHDOG runs on its own runspace, from just after step 2. About every half second
# plus the time a pass takes, it records each process new since step 2 whose name starts
# with Roamer, Adsk, Autodesk, Navis, Lc, Genuine, AdSSO, WerFault, FNPLicensing or LMU,
# with its parent, named only when that process started before the child, and whether its
# command line holds either word. A Roamer's command line is printed only in the automation
# form, the executable and -Embedding and nothing else. It records every Roamer new since
# step 2 that any pass sees, which is how the settings rule below knows whether another
# Navisworks ran. It reads the visible top level windows of the adopted Roamer, or before
# adoption of the Roamers that meet the start time and -Embedding test, with the text of any
# #32770 dialog of theirs, and no window of any other process. It writes a line only when
# something changes, and it counts its passes and the longest. A Roamer shown for less than
# a pass and its sleep can be missed.
#
# TWO DEADLINES. -ConstructorDeadlineSeconds runs from the call. Past it, with nothing
# adopted, the watchdog writes every Roamer meeting the test to unproved-starts.txt, says
# so in the result, and ends this process through TerminateProcess with exit code 3, so
# that no finalizer of the half built object runs, and closes nothing.
# -AdoptedDeadlineSeconds runs from adoption. Past it, the watchdog closes the adopted id
# after reading its start time again, so a blocked call returns.
#
# BADER'S SETTINGS. Before the constructor, every file under
# %APPDATA%\Autodesk\Navisworks Manage 2025 except AutoSave is copied into the work folder
# and HKCU\Software\Autodesk\Navisworks Manage\22.0 is exported and read key by key. A key
# that cannot be read at the start stops the probe before the constructor. At the end
# every value, key and file that changed is printed with its old and new value. They are
# PUT BACK ONLY WHEN NO OTHER NAVISWORKS RAN, the lead's decision D2: no Roamer at step 2,
# none new at any watchdog pass but the adopted one, none running at the end, and the
# adopted one reads gone. Otherwise nothing is written and the backup is kept. Nothing is
# written under a key or a folder that could not be read at both ends, a key is removed
# only when the probe saw it appear while its parent read at both ends, a file that went is
# copied back only if it is still gone just before the copy, and a changed file only if it
# still holds what the compare read. A file whose content cannot be read at either end is
# never written. A file that appeared is left in place, because the loop deletes only from
# its own folder. AutoSave is compared and reported, never copied and never put back. A
# path in a printed value is shown in full only under the loop folder or the install
# folder, otherwise as its file name, if it has one, and a sha1 of the whole path.
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
function Err($e) {
  $parts = @(); $x = $e
  while ($null -ne $x) { $parts += ($x.GetType().FullName + ": " + $x.Message); $x = $x.InnerException }
  return ($parts -join " <- ")
}

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

# A path in a printed value is shown in full only under the loop folder or the install
# folder. Any other path shows as its file name, if it has one, and a sha1 of the whole
# path, so an entry that moved can be followed without printing where it lives.
$script:sha1 = [System.Security.Cryptography.SHA1]::Create()
function Mask($s) {
  if ($null -eq $s) { return "null" }
  $s = [string]$s
  if ($s -notmatch '[A-Za-z]:\\|\\\\') { return ("`"" + $s + "`"") }
  if ($s.StartsWith($loopRoot, [StringComparison]::OrdinalIgnoreCase)) { return ("`"%LOCALAPPDATA%\NwcFederatorLoop" + $s.Substring($loopRoot.Length) + "`"") }
  if ($s.StartsWith($nw, [StringComparison]::OrdinalIgnoreCase)) { return ("`"" + $s + "`"") }
  $h = [BitConverter]::ToString($script:sha1.ComputeHash($utf8.GetBytes($s.ToLowerInvariant()))).Replace("-", "").Substring(0, 12)
  $what = "text holding a path"
  if ($s -match '^[A-Za-z]:\\[^;|<>"*?]*$') {
    $leaf = Split-Path $s -Leaf
    if ($leaf -match '\.[A-Za-z0-9]{2,4}$') { $what = "a path outside the loop folder, file " + $leaf } else { $what = "a folder outside the loop folder" }
  }
  return ("<" + $what + ", sha1 " + $h + ">")
}

Say ("MACHINE   " + $env:COMPUTERNAME + "   " + $T0.ToString("yyyy-MM-dd HH:mm:ss"))
Say ("HOST      powershell " + $PSVersionTable.PSVersion + ", 64 bit process " + [Environment]::Is64BitProcess + ", apartment " + [System.Threading.Thread]::CurrentThread.ApartmentState + ", pid " + $PID)
Say ("SCRIPT    " + $PSCommandPath + ", " + (Get-Item -LiteralPath $PSCommandPath).Length + " bytes, sha256 " + (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash)
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
$ascii = [System.Text.Encoding]::ASCII.GetString($bytes)
foreach ($w in @("CoCreateInstance", "GetActiveObject", "CoGetClassObject", "CreateProcessW", "ShellExecuteW")) {
  Say ("  import name " + $w.PadRight(18) + " in the DLL: " + $ascii.Contains($w))
}
if (-not $ascii.Contains("CoCreateInstance")) { $gate.Add("no CoCreateInstance import name in the DLL") }
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
# The window reader, in memory. EnumWindows needs a callback, so the delegate type is
# emitted too. It is compared by process id FIRST, so no window of any other process is
# read, and a message goes only to a child of a dialog of the process asked for.
$dynName = New-Object System.Reflection.AssemblyName("ProbeWinNative")
$dynAsm = [AppDomain]::CurrentDomain.DefineDynamicAssembly($dynName, [System.Reflection.Emit.AssemblyBuilderAccess]::Run)
$dynMod = $dynAsm.DefineDynamicModule("ProbeWinNative")
$dt = $dynMod.DefineType("ProbeEnumProc", [System.Reflection.TypeAttributes]"Public,Sealed,AutoClass", [System.MulticastDelegate])
$dctor = $dt.DefineConstructor([System.Reflection.MethodAttributes]"RTSpecialName,SpecialName,HideBySig,Public", [System.Reflection.CallingConventions]::Standard, [Type[]]@([object], [IntPtr]))
$dctor.SetImplementationFlags([System.Reflection.MethodImplAttributes]"Runtime,Managed")
$dinv = $dt.DefineMethod("Invoke", [System.Reflection.MethodAttributes]"Public,HideBySig,NewSlot,Virtual", [bool], [Type[]]@([IntPtr], [IntPtr]))
$dinv.SetImplementationFlags([System.Reflection.MethodImplAttributes]"Runtime,Managed")
$procType = $dt.CreateType()
$tb = $dynMod.DefineType("ProbeWin", [System.Reflection.TypeAttributes]"Public,Class,Abstract,Sealed")
# Two kernel32 calls beside them, so the constructor deadline can end this process with
# no managed shutdown, and so no finalizer runs against a Roamer nobody adopted.
foreach ($def in @(
    @("user32.dll", "EnumWindows", [bool], [Type[]]@($procType, [IntPtr])),
    @("user32.dll", "EnumChildWindows", [bool], [Type[]]@([IntPtr], $procType, [IntPtr])),
    @("user32.dll", "GetWindowThreadProcessId", [uint32], [Type[]]@([IntPtr], [uint32].MakeByRefType())),
    @("user32.dll", "IsWindowVisible", [bool], [Type[]]@([IntPtr])),
    @("user32.dll", "GetClassNameW", [int], [Type[]]@([IntPtr], [System.Text.StringBuilder], [int])),
    @("user32.dll", "GetWindowTextW", [int], [Type[]]@([IntPtr], [System.Text.StringBuilder], [int])),
    @("user32.dll", "SendMessageTimeoutW", [IntPtr], [Type[]]@([IntPtr], [uint32], [IntPtr], [System.Text.StringBuilder], [uint32], [uint32], [IntPtr].MakeByRefType())),
    @("kernel32.dll", "GetCurrentProcess", [IntPtr], [Type[]]@()),
    @("kernel32.dll", "TerminateProcess", [bool], [Type[]]@([IntPtr], [uint32])))) {
  $pm = $tb.DefinePInvokeMethod($def[1], $def[0], [System.Reflection.MethodAttributes]"Public,Static,PinvokeImpl,HideBySig", [System.Reflection.CallingConventions]::Standard, $def[2], $def[3], [System.Runtime.InteropServices.CallingConvention]::Winapi, [System.Runtime.InteropServices.CharSet]::Unicode)
  $pm.SetImplementationFlags($pm.GetMethodImplementationFlags() -bor [System.Reflection.MethodImplAttributes]::PreserveSig)
}
$winType = $tb.CreateType()

$winFuncs = @'
function WinHandlesOf($winType, $procType, [uint32]$owner, [IntPtr]$parent) {
  $found = New-Object System.Collections.Generic.List[IntPtr]
  $cb = {
    param([IntPtr]$h, [IntPtr]$l)
    [uint32]$wp = 0
    [void]$winType::GetWindowThreadProcessId($h, [ref]$wp)
    if ($wp -eq $owner) { $found.Add($h) }
    return $true
  }.GetNewClosure()
  $del = [System.Management.Automation.LanguagePrimitives]::ConvertTo($cb, $procType)
  if ($parent -eq [IntPtr]::Zero) { [void]$winType::EnumWindows($del, [IntPtr]::Zero) } else { [void]$winType::EnumChildWindows($parent, $del, [IntPtr]::Zero) }
  [GC]::KeepAlive($del)
  return $found
}
function WindowLines($winType, $procType, [uint32]$owner, [bool]$visibleOnly) {
  $lines = New-Object System.Collections.Generic.List[string]
  foreach ($h in (WinHandlesOf $winType $procType $owner ([IntPtr]::Zero))) {
    $vis = $winType::IsWindowVisible($h)
    if ($visibleOnly -and -not $vis) { continue }
    $c = New-Object System.Text.StringBuilder 256
    [void]$winType::GetClassNameW($h, $c, 256)
    $t = New-Object System.Text.StringBuilder 512
    [void]$winType::GetWindowTextW($h, $t, 512)
    $line = "[" + $c.ToString() + "] `"" + $t.ToString() + "`""
    if (-not $vis) { $line += " hidden" }
    if ($vis -and $c.ToString() -eq "#32770") {
      $parts = @()
      foreach ($ch in (WinHandlesOf $winType $procType $owner $h)) {
        if (-not $winType::IsWindowVisible($ch)) { continue }
        $sb = New-Object System.Text.StringBuilder 2048
        [IntPtr]$res = [IntPtr]::Zero
        [void]$winType::SendMessageTimeoutW($ch, [uint32]0x000D, [IntPtr]2048, $sb, [uint32]0x0002, [uint32]500, [ref]$res)
        $s = $sb.ToString().Replace("`r", " ").Replace("`n", " ").Trim()
        if ($s.Length -gt 0) { $parts += ("`"" + $s + "`"") }
      }
      $line += " text: " + ($parts -join " ")
    }
    $lines.Add($line)
  }
  return $lines
}
'@
. ([scriptblock]::Create($winFuncs))


# The command line tests, the parent rule and the unproved starts file, written once and
# handed to the watchdog the way the window reader is.
$sharedFuncs = @'
function HoldsEmbedding($cmd) { return ($null -ne $cmd -and $cmd -match '(^|\s)[-/]Embedding(\s|$)') }
function AutomationForm($cmd) { return ($null -ne $cmd -and $cmd -match '^\s*"?[^"]*\\Roamer\.exe"?\s+[-/]Embedding\s*$') }
function NamesAutomation($cmd) { return ($null -ne $cmd -and $cmd -match 'embedding|automation') }
function WordText($cmd) {
  if ($null -eq $cmd) { return "UNKNOWN, the command line could not be read" }
  if (NamesAutomation $cmd) { return "YES" }
  return "no"
}
function ParentText($child, $parent, $parentId) {
  if ($null -eq $child) { return "UNKNOWN" }
  if ($null -eq $parent) { return ([string]$parentId + " not running") }
  if ($null -ne $parent.CreationDate -and $null -ne $child.CreationDate -and $parent.CreationDate -lt $child.CreationDate) { return ([string]$parentId + " " + $parent.Name) }
  return ([string]$parentId + " UNKNOWN, the process holding that id now did not start before this one")
}
function UnprovedLine($id, $ticks, $why) { return ([string]$id + "`t" + [string]$ticks + "`t" + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + "`t" + $why) }
function AppendUnproved($path, $lines) {
  $u = New-Object System.Text.UTF8Encoding($false)
  if (-not (Test-Path -LiteralPath $path)) { [System.IO.File]::WriteAllText($path, "# pid`tstart ticks`twritten`twhy`r`n", $u) }
  foreach ($l in $lines) { [System.IO.File]::AppendAllText($path, $l + "`r`n", $u) }
}
'@
. ([scriptblock]::Create($sharedFuncs))

function WindowsOf($id) {
  $vis = @(WindowLines $winType $procType ([uint32]$id) $true)
  $all = @(WindowLines $winType $procType ([uint32]$id) $false)
  $dialogs = @($vis | Where-Object { $_.StartsWith("[#32770]") })
  Say ("  visible top level windows of " + $id + ": " + $vis.Count + ", hidden: " + ($all.Count - $vis.Count) + ", dialogs of class #32770 visible: " + $dialogs.Count)
  foreach ($w in $vis) { Say ("    " + $w) }
}

# The state of a process id against a start time read before: same, gone, other when the
# id now belongs to another process, or unreadable. Only same may ever be closed, and only
# gone counts as gone.
function ProcState($id, $start) {
  $p = Get-Process -Id $id -ErrorAction SilentlyContinue
  if ($null -eq $p) { return "gone" }
  $st = $null
  try { $st = $p.StartTime } catch { Say ("  the start time of pid " + $id + " could not be read, " + (Err $_.Exception) + ", so it is not closed"); return "unreadable" }
  if ($null -eq $st) { return "unreadable" }
  if ($null -ne $start -and $st -eq $start) { return "same" }
  return "other"
}

function CimRow($id) {
  try { return (Get-CimInstance Win32_Process -Filter ("ProcessId=" + $id) -ErrorAction Stop) } catch { Say ("  Win32_Process for pid " + $id + " threw, " + (Err $_.Exception)); return $null }
}

# What is known about one Roamer, read through the process list and Win32_Process only.
function RoamerRecord($p) {
  $r = [pscustomobject]@{ Id = $p.Id; Start = $null; Cmd = $null; Parent = "UNKNOWN"; Path = $null }
  try { $r.Start = $p.StartTime } catch { Say ("  the start time of Roamer " + $p.Id + " could not be read, " + (Err $_.Exception)) }
  $ci = CimRow $p.Id
  if ($null -ne $ci) {
    $r.Cmd = $ci.CommandLine; $r.Path = $ci.ExecutablePath
    $r.Parent = ParentText $ci (CimRow $ci.ParentProcessId) $ci.ParentProcessId
  }
  return $r
}

# The registry, read key by key. A key whose values or subkeys cannot be read is kept in
# Failed with the reason, and nothing is ever written under it.
function RegRead($sub) {
  $r = [pscustomobject]@{ Read = @{}; Failed = @{}; Missing = $false }
  $root = $null
  try { $root = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($sub, $false) } catch { $r.Failed["HKEY_CURRENT_USER\" + $sub] = (Err $_.Exception); return $r }
  if ($null -eq $root) { $r.Missing = $true; return $r }
  try { RegWalk $root $r } finally { $root.Close() }
  return $r
}
function RegWalk($k, $r) {
  $name = $k.Name
  $vals = @{}
  try {
    foreach ($n in $k.GetValueNames()) {
      $vals[$n] = [pscustomobject]@{ Kind = $k.GetValueKind($n); Data = $k.GetValue($n, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames) }
    }
  } catch { $r.Failed[$name] = "its values, " + (Err $_.Exception); return }
  $r.Read[$name] = $vals
  $subs = $null
  try { $subs = $k.GetSubKeyNames() } catch { $r.Failed[$name] = "its subkeys, " + (Err $_.Exception); return }
  foreach ($s in $subs) {
    $c = $null
    try { $c = $k.OpenSubKey($s, $false) } catch { $r.Failed[$name + "\" + $s] = (Err $_.Exception); continue }
    if ($null -eq $c) { $r.Failed[$name + "\" + $s] = "listed, then could not be opened"; continue }
    try { RegWalk $c $r } finally { $c.Close() }
  }
}
function UnderFailed($name, $failed) {
  foreach ($f in $failed.Keys) { if ($name -eq $f -or $name.StartsWith($f + "\", [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
function FailedBelow($name, $failed) {
  foreach ($f in $failed.Keys) { if ($f.StartsWith($name + "\", [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
function ParentKey($name) { return $name.Substring(0, $name.LastIndexOf('\')) }
function ShortKey($name) { return $name.Replace("HKEY_CURRENT_USER\Software\Autodesk\Navisworks Manage\22.0", "22.0") }
function RegText($v) {
  if ($null -eq $v) { return "absent" }
  $d = $v.Data
  switch ([string]$v.Kind) {
    "Binary" { return ("Binary " + [BitConverter]::ToString([byte[]]$d)) }
    "MultiString" { return ("MultiString " + ((@($d) | ForEach-Object { Mask $_ }) -join " | ")) }
    "String" { return ("String " + (Mask $d)) }
    "ExpandString" { return ("ExpandString " + (Mask $d)) }
    default { return ([string]$v.Kind + " " + [string]$d) }
  }
}
function RegSame($a, $b) {
  if ($null -eq $a -or $null -eq $b) { return ($null -eq $a -and $null -eq $b) }
  if ([string]$a.Kind -ne [string]$b.Kind) { return $false }
  if ([string]$a.Kind -eq "Binary") { return ([BitConverter]::ToString([byte[]]$a.Data) -eq [BitConverter]::ToString([byte[]]$b.Data)) }
  if ([string]$a.Kind -eq "MultiString") { return ((@($a.Data) -join "`n") -ceq (@($b.Data) -join "`n")) }
  return ([string]$a.Data -ceq [string]$b.Data)
}
function HkcuSub($name) { return $name.Substring("HKEY_CURRENT_USER\".Length) }
function ValueLabel($n) { if ($n -eq "") { return "(default)" }; return $n }

# The settings folder, listed and hashed. A folder that cannot be listed is kept in Failed,
# and nothing is ever written under it.
function SettingsRead($dir) {
  $r = [pscustomobject]@{ Files = @{}; Failed = New-Object System.Collections.Generic.List[string]; Missing = $false }
  if (-not (Test-Path -LiteralPath $dir)) { $r.Missing = $true; return $r }
  $ev = $null
  $files = @(Get-ChildItem -LiteralPath $dir -Recurse -File -Force -ErrorAction SilentlyContinue -ErrorVariable ev)
  foreach ($e in @($ev)) {
    if ($null -eq $e) { continue }
    $t = [string]$e.TargetObject
    if ($t -eq "") { $t = $dir }
    $r.Failed.Add($t)
    Say ("  could not list " + $t + ", " + $e.Exception.Message)
  }
  foreach ($f in $files) {
    $rel = $f.FullName.Substring($dir.Length).TrimStart('\')
    $hash = $null
    try { $hash = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash } catch { Say ("  could not hash " + $rel + ", " + (Err $_.Exception)) }
    $r.Files[$rel] = [pscustomobject]@{ Hash = $hash; Length = $f.Length; Write = $f.LastWriteTime }
  }
  return $r
}
function FolderFailed($path, $failed) {
  foreach ($f in $failed) { if ($path -eq $f -or $path.StartsWith($f.TrimEnd('\') + "\", [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
function FileText($v) {
  if ($null -eq $v) { return "absent" }
  $hs = "unhashed"; if ($null -ne $v.Hash) { $hs = $v.Hash.Substring(0, 12) }
  return ([string]$v.Length + " bytes, sha256 " + $hs + ", written " + $v.Write.ToString("yyyy-MM-dd HH:mm:ss"))
}

# =======================================================================================
Say "==== THE WORK FOLDER ===="
if (-not $work.StartsWith($loopRoot + "\", [StringComparison]::OrdinalIgnoreCase)) { Say ("STOP before step 2, the work folder " + $work + " is not under " + $loopRoot); exit 1 }
if (Test-Path -LiteralPath $work) {
  $keptName = "automation-start-" + [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
  try { Rename-Item -LiteralPath $work -NewName $keptName -ErrorAction Stop; Say ("  a folder from an earlier run was there, kept by renaming it " + $keptName) }
  catch { Say ("STOP before step 2: the work folder from an earlier run could not be renamed, " + (Err $_.Exception) + ". Nothing was touched"); exit 1 }
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
if (Test-Path -LiteralPath $unproved) {
  $ulines = $null
  try { $ulines = [System.IO.File]::ReadAllLines($unproved) } catch { Say ("STOP before the constructor: " + (Mask $unproved) + " could not be read, " + (Err $_.Exception) + ". A person has to look"); exit 1 }
  $named = 0
  $still = New-Object System.Collections.Generic.List[string]
  foreach ($ul in $ulines) {
    if ($ul.Trim() -eq "" -or $ul.StartsWith("#")) { continue }
    $named++
    $parts = $ul.Split("`t")
    $upid = 0; $uticks = [long]0
    if ($parts.Count -lt 2 -or -not [int]::TryParse($parts[0], [ref]$upid) -or -not [long]::TryParse($parts[1], [ref]$uticks)) { Say ("STOP before the constructor: a line of " + (Mask $unproved) + " cannot be read, `"" + $ul + "`". A person has to look"); exit 1 }
    $up = Get-Process -Id $upid -ErrorAction SilentlyContinue
    if ($null -eq $up) { continue }
    if ($uticks -eq 0) { $still.Add("pid " + $upid + ", its start ticks were never read, and a process holds that id now"); continue }
    $ust = $null
    try { $ust = $up.StartTime } catch { Say ("  the start time of pid " + $upid + " could not be read, " + (Err $_.Exception)) }
    if ($null -eq $ust) { $still.Add("pid " + $upid + ", start ticks " + $uticks + ", a process holds that id now and its start time cannot be read") }
    elseif ($ust.Ticks -eq $uticks) { $still.Add("pid " + $upid + ", start ticks " + $uticks + ", still running") }
  }
  Say ("  " + (Mask $unproved) + " names " + $named + " starts this probe could not prove, still running: " + $still.Count)
  if ($still.Count -gt 0) {
    foreach ($s in $still) { Say ("    " + $s) }
    Say "STOP before the constructor: a start this probe could not prove is still running. A person has to look"
    exit 1
  }
} else { Say "  no start this probe could not prove is recorded" }
$beforeRoamer = @(Get-Process -Name Roamer -ErrorAction SilentlyContinue | Sort-Object Id)
$beforeStart = @{}
$refuse = New-Object System.Collections.Generic.List[string]
foreach ($p in $beforeRoamer) {
  $rec = RoamerRecord $p
  $beforeStart[$p.Id] = $rec.Start
  $flag = WordText $rec.Cmd
  Say ("  Roamer pid " + $p.Id + ", started " + $(if ($rec.Start) { $rec.Start.ToString("yyyy-MM-dd HH:mm:ss") } else { "UNKNOWN" }) + ", parent " + $rec.Parent + ", command line holds the word embedding or automation: " + $flag + ". Not ours, never closed, attached to or sent anything")
  if ($flag -ne "no") { $refuse.Add([string]$p.Id + " " + $flag) }
  if ($null -eq $rec.Start) { $refuse.Add([string]$p.Id + " start time unreadable") }
}
if ($beforeRoamer.Count -eq 0) { Say "  none" }
$beforeAll = @(Get-Process | ForEach-Object { $_.Id })
Say ("  every process id on the machine taken as the before set: " + $beforeAll.Count)
if ($refuse.Count -gt 0) {
  Say ("STOP before the constructor: a Roamer already running names embedding or automation, or could not be read: " + ($refuse -join ", "))
  exit 1
}
$beforeTicks = @{}
foreach ($k in $beforeStart.Keys) { $beforeTicks[[int]$k] = $beforeStart[$k].Ticks }
function NewRoamers {
  $r = @()
  foreach ($p in @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)) {
    $st = $null
    try { $st = $p.StartTime } catch { Say ("  the start time of Roamer " + $p.Id + " could not be read, " + (Err $_.Exception) + ", so it counts as new") }
    if ($beforeStart.ContainsKey($p.Id) -and $null -ne $st -and $beforeStart[$p.Id] -eq $st) { continue }
    $r += $p
  }
  return $r
}
$outcome["2"] = "passed, " + $beforeRoamer.Count + " Roamer running before, none names embedding or automation"
Say ""

# =======================================================================================
# The watchdog starts now, before the backup, so every Roamer from the backup on is seen.
$sync = [hashtable]::Synchronized(@{})
$sync.Stop = $false
$sync.Before = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($id in $beforeAll) { [void]$sync.Before.Add([int]$id) }
$sync.BeforeRoamer = $beforeTicks
$sync.T0 = $T0
$sync.CallStart = [DateTime]::MaxValue
$sync.CtorReturned = $false
$sync.CtorDeadline = $ConstructorDeadlineSeconds
$sync.AdoptedDeadline = $AdoptedDeadlineSeconds
$sync.AdoptedAt = [DateTime]::MaxValue
$sync.MyPid = 0
$sync.MyTicks = [long]0
$sync.NoAdopt = $false
$sync.Forced = ""
$sync.Out = $Out
$sync.Unproved = $unproved
$sync.WatchFile = $watchFile
$sync.WinFuncs = $winFuncs
$sync.SharedFuncs = $sharedFuncs
$sync.WinType = $winType
$sync.ProcType = $procType
$sync.Seen = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
$sync.RoamerSet = [hashtable]::Synchronized(@{})
$sync.ZeroSightings = [hashtable]::Synchronized(@{})
$sync.WriteErrors = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
$sync.Passes = 0
$sync.PassMaxMs = 0.0
$sync.PassTotalMs = 0.0
$watchScript = {
  param($sync)
  . ([scriptblock]::Create($sync.WinFuncs))
  . ([scriptblock]::Create($sync.SharedFuncs))
  $utf = New-Object System.Text.UTF8Encoding($false)
  function W($t) {
    try { [System.IO.File]::AppendAllText($sync.WatchFile, ([DateTime]::Now.ToString("HH:mm:ss.fff") + "  t+" + ([DateTime]::Now - $sync.T0).TotalSeconds.ToString("0.0") + "s  " + $t + "`r`n"), $utf) }
    catch { [void]$sync.WriteErrors.Add($_.Exception.Message) }
  }
  function ToOut($t) {
    if ($sync.Out -eq "") { return }
    try { [System.IO.File]::AppendAllText($sync.Out, $t + "`r`n", $utf) } catch { [void]$sync.WriteErrors.Add($_.Exception.Message) }
  }
  $rows = @{}
  function Row($id) {
    if ($rows.ContainsKey($id)) { return $rows[$id] }
    $ci = $null
    try { $ci = Get-CimInstance Win32_Process -Filter ("ProcessId=" + $id) -ErrorAction Stop } catch { W ("Win32_Process for pid " + $id + " threw: " + $_.Exception.Message) }
    $rows[$id] = $ci
    return $ci
  }
  function StartOf($p) {
    $st = $null
    try { $st = $p.StartTime } catch { W ("the start time of pid " + $p.Id + " could not be read: " + $_.Exception.Message); return $null }
    return $st
  }
  $seen = @{}
  $lastWin = @{}
  $watched = @{}
  $zeroLogged = @{}
  W "watchdog started"
  while (-not $sync.Stop) {
    $pass = [Diagnostics.Stopwatch]::StartNew()
    try {
      $now = [DateTime]::Now
      $procs = @(Get-Process)
      $cands = @()
      foreach ($p in $procs) {
        $isRoamer = ($p.ProcessName -eq "Roamer")
        $st = $null
        if ($isRoamer) {
          $st = StartOf $p
          if ($null -ne $st -and $sync.BeforeRoamer.ContainsKey($p.Id) -and $sync.BeforeRoamer[$p.Id] -eq $st.Ticks) { continue }
          if ($null -eq $st) {
            # Counted by the settings rule, and looked at again at the next pass.
            $z = $sync.ZeroSightings[$p.Id]
            if ($null -eq $z) { $sync.ZeroSightings[$p.Id] = @($now, $now) } else { $sync.ZeroSightings[$p.Id] = @($z[0], $now) }
            if (-not $zeroLogged.ContainsKey($p.Id)) { $zeroLogged[$p.Id] = $true; W ("Roamer pid " + $p.Id + " seen with no readable start time, looked at again at the next pass") }
            continue
          }
          $key = [string]$p.Id + "|" + $st.Ticks
          if (-not $sync.RoamerSet.ContainsKey($key)) { $sync.RoamerSet[$key] = $now }
          if ($st -ge $sync.CallStart) {
            $ci = Row $p.Id
            if ($null -ne $ci -and (HoldsEmbedding $ci.CommandLine)) { $cands += ,@($p.Id, $st.Ticks) }
          }
        } else {
          if ($sync.Before.Contains([int]$p.Id)) { continue }
          $key = [string]$p.Id
        }
        if ($seen.ContainsKey($key)) { continue }
        $seen[$key] = $true
        if ($p.ProcessName -notmatch '^(Roamer|Adsk|Autodesk|Navis|Lc|Genuine|AdSSO|WerFault|FNPLicensing|LMU)') { continue }
        # One process that cannot be recorded is named, and the pass goes on to the rest.
        try {
          if (-not $isRoamer) { $st = StartOf $p }
          $ci = Row $p.Id
          $parentText = "UNKNOWN"; $cmd = $null
          if ($null -ne $ci) { $cmd = $ci.CommandLine; $parentText = ParentText $ci (Row $ci.ParentProcessId) $ci.ParentProcessId }
          $form = ""
          if ($isRoamer) { if (AutomationForm $cmd) { $form = ", automation form " + $cmd } else { $form = ", not in the automation form, not printed" } }
          $stText = "UNKNOWN, it could not be read"; $ticks = [long]0
          if ($null -ne $st) { $stText = $st.ToString("HH:mm:ss.fff"); $ticks = $st.Ticks }
          [void]$sync.Seen.Add([pscustomobject]@{ Id = $p.Id; Name = $p.ProcessName; Ticks = $ticks; Parent = $parentText })
          W ("new process " + $p.ProcessName + " pid " + $p.Id + ", parent " + $parentText + ", started " + $stText + ", command line holds the word embedding or automation: " + (WordText $cmd) + $form)
        } catch { W ("new process " + $p.ProcessName + " pid " + $p.Id + " could not be recorded: " + $_.Exception.Message) }
      }
      $targets = @()
      if ($sync.MyPid -ne 0) { $targets += ,@($sync.MyPid, $sync.MyTicks) }
      elseif (-not $sync.NoAdopt) { $targets = $cands }
      foreach ($tg in $targets) {
        $id = $tg[0]
        $watched[$id] = $tg[1]
        $pp = Get-Process -Id $id -ErrorAction SilentlyContinue
        if ($null -eq $pp) { continue }
        $pst = StartOf $pp
        if ($null -eq $pst -or $pst.Ticks -ne $tg[1]) { continue }
        $wins = (WindowLines $sync.WinType $sync.ProcType ([uint32]$id) $true) -join " | "
        if ($wins -eq "") { $wins = "none" }
        if ($lastWin[$id] -ne $wins) { $lastWin[$id] = $wins; W ("Roamer " + $id + " visible top level windows: " + $wins) }
      }
      foreach ($id in @($watched.Keys)) {
        if ($lastWin[$id] -eq "GONE") { continue }
        $pp = Get-Process -Id $id -ErrorAction SilentlyContinue
        $gone = ($null -eq $pp)
        if (-not $gone) { $pst = StartOf $pp; $gone = ($null -ne $pst -and $pst.Ticks -ne $watched[$id]) }
        if ($gone) { $lastWin[$id] = "GONE"; W ("Roamer " + $id + " has exited") }
      }
      # The constructor deadline. Nothing is adopted, so nothing is closed. What meets the
      # test is written down, and this process ends itself with no managed shutdown.
      if (-not $sync.CtorReturned -and $sync.MyPid -eq 0 -and $sync.CallStart -ne [DateTime]::MaxValue -and ($now - $sync.CallStart).TotalSeconds -gt $sync.CtorDeadline) {
        $lines = @()
        foreach ($c in $cands) { $lines += (UnprovedLine $c[0] $c[1] ("the constructor passed its deadline of " + $sync.CtorDeadline + " s with nothing adopted")) }
        $wrote = "nothing to write"
        if ($lines.Count -gt 0) { try { AppendUnproved $sync.Unproved $lines; $wrote = "written to unproved-starts.txt" } catch { $wrote = "NOT written to unproved-starts.txt, " + $_.Exception.Message } }
        $msg = "CONSTRUCTOR DEADLINE of " + $sync.CtorDeadline + " s passed with nothing adopted. Roamers meeting the test: " + $cands.Count + $(if ($cands.Count -gt 0) { ", pid " + (($cands | ForEach-Object { [string]$_[0] }) -join ", ") } else { "" }) + ", " + $wrote + ". Nothing is closed and nothing is put back. This process ends itself now with exit code 3"
        W $msg
        ToOut ""
        ToOut ("==== " + $msg + " ====")
        ToOut "---- the watchdog's record ----"
        try { foreach ($l in [System.IO.File]::ReadAllLines($sync.WatchFile)) { ToOut ("  " + $l) } } catch { ToOut ("  the watchdog's record could not be read, " + $_.Exception.Message) }
        ToOut "  the settings backup is kept in the work folder and nothing was put back"
        [void]$sync.WinType::TerminateProcess($sync.WinType::GetCurrentProcess(), [uint32]3)
      }
      # The adopted deadline, for the adopted Roamer only, after its start time is read again.
      if ($sync.Forced -eq "" -and $sync.MyPid -ne 0 -and ($now - $sync.AdoptedAt).TotalSeconds -gt $sync.AdoptedDeadline) {
        $pp = Get-Process -Id $sync.MyPid -ErrorAction SilentlyContinue
        $pst = $null
        if ($null -ne $pp) { $pst = StartOf $pp }
        if ($null -ne $pst -and $pst.Ticks -eq $sync.MyTicks) {
          try { Stop-Process -Id $sync.MyPid -Force -ErrorAction Stop; $sync.Forced = "the adopted deadline of " + $sync.AdoptedDeadline + " s passed, closed the adopted pid " + $sync.MyPid } catch { $sync.Forced = "the adopted deadline passed, Stop-Process on pid " + $sync.MyPid + " threw: " + $_.Exception.Message }
        } else { $sync.Forced = "the adopted deadline passed, pid " + $sync.MyPid + " is gone, is another process or cannot be read, nothing closed" }
        W ($sync.Forced)
      }
    } catch { W ("watchdog error: " + $_.Exception.Message) }
    $pass.Stop()
    $ms = $pass.Elapsed.TotalMilliseconds
    $sync.Passes = $sync.Passes + 1
    $sync.PassTotalMs = $sync.PassTotalMs + $ms
    if ($ms -gt $sync.PassMaxMs) { $sync.PassMaxMs = $ms }
    Start-Sleep -Milliseconds 500
  }
  W "watchdog stopped"
}
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
$regFile = Join-Path $work "hkcu-navisworks-manage-22.0-before.reg"
& reg.exe export "HKCU\Software\Autodesk\Navisworks Manage\22.0" $regFile /y | Out-Null
Say ("  HKCU Navisworks Manage 22.0 exported to " + (Mask $regFile) + ", reg.exe exit " + $LASTEXITCODE)
$regBefore = RegRead $regSub
$valCount = 0; foreach ($k in $regBefore.Read.Keys) { $valCount += $regBefore.Read[$k].Count }
Say ("  read key by key: " + $regBefore.Read.Count + " keys, " + $valCount + " values, " + $regBefore.Failed.Count + " keys that could not be read" + $(if ($regBefore.Missing) { ", the key is not there" } else { "" }))
if ($regBefore.Failed.Count -gt 0) {
  foreach ($f in $regBefore.Failed.Keys) { Say ("    could not read " + (ShortKey $f) + ", " + $regBefore.Failed[$f]) }
  StopEarly "STOP before the constructor: a key under 22.0 could not be read at the start"
}
$nwAppData = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025"
$appBackup = Join-Path $work "appdata-before"
$filesBefore = @{}
$autoBefore = @{}
$notBacked = @{}
$sBefore = SettingsRead $nwAppData
if ($sBefore.Failed.Count -gt 0) { StopEarly "STOP before the constructor: a folder of the settings could not be listed at the start" }
foreach ($rel in $sBefore.Files.Keys) {
  if ($rel -like "AutoSave\*") { $autoBefore[$rel] = $sBefore.Files[$rel]; continue }
  if ($null -eq $sBefore.Files[$rel].Hash) {
    # Its content could not be read, so another process holds it. It is never copied and
    # never written.
    $notBacked[$rel] = $sBefore.Files[$rel]
    Say ("  NOT BACKED UP " + $rel + ", its content could not be read at the start")
    continue
  }
  $to = Join-Path $appBackup $rel
  try {
    New-Item -ItemType Directory -Force -Path (Split-Path $to -Parent) | Out-Null
    Copy-Item -LiteralPath (Join-Path $nwAppData $rel) -Destination $to -Force
    $filesBefore[$rel] = [pscustomobject]@{ Hash = (Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash; Length = (Get-Item -LiteralPath $to).Length; Write = (Get-Item -LiteralPath (Join-Path $nwAppData $rel)).LastWriteTime }
  } catch {
    $notBacked[$rel] = $sBefore.Files[$rel]
    Say ("  NOT BACKED UP " + $rel + ", " + (Err $_.Exception))
  }
}
Say ("  %APPDATA%\Autodesk\Navisworks Manage 2025: " + $filesBefore.Count + " files copied to the work folder with their sha256, " + $notBacked.Count + " not backed up, " + $autoBefore.Count + " AutoSave files listed and not copied")
$fedLogs = Join-Path $env:LOCALAPPDATA "ParsonsNwcFederator\logs"
$fedBefore = SettingsRead $fedLogs
Say ("  the tool's own logs folder listed: " + $fedBefore.Files.Count + " files")
Say ""

$app = $null
$myPid = 0
$myStart = $null
$goneAt = $null
$disposed = $false
try {
  # =====================================================================================
  Say "==== STEP 3. Start one Navisworks through the API ===="
  $currentStep = "3"
  $outcome["3"] = "started, did not finish"
  Say ("  " + (Stamp) + "  calling new NavisworksApplication()")
  $sync.CallStart = [DateTime]::Now
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $err = $null
  try { $app = [Activator]::CreateInstance($napp) } catch { $err = $_.Exception }
  $sync.CtorReturned = $true
  $sw.Stop()
  $returnedAt = [DateTime]::Now
  Say ("  " + (Stamp) + "  the constructor " + $(if ($null -eq $err) { "RETURNED" } else { "THREW" }) + " after " + $sw.Elapsed.TotalSeconds.ToString("0.00") + " s")
  if ($null -ne $err) { Say ("    " + (Err $err)) }

  $recs = @(foreach ($p in @(NewRoamers)) { RoamerRecord $p })
  Say ("  Roamer processes new since step 2: " + $recs.Count)
  foreach ($r in $recs) {
    $shown = "not in the automation form, not printed"
    if (AutomationForm $r.Cmd) { $shown = $r.Cmd }
    Say ("    pid " + $r.Id + ", started " + $(if ($r.Start) { $r.Start.ToString("HH:mm:ss.fff") } else { "UNKNOWN" }) + ", parent " + $r.Parent + ", path " + (Mask $r.Path))
    Say ("    command line holds the word embedding or automation: " + (WordText $r.Cmd) + ", command line: " + $shown)
  }
  $c1 = ($null -eq $err -and $null -ne $app)
  $c2 = ($recs.Count -eq 1)
  $c3 = ($c2 -and $null -ne $recs[0].Start -and $recs[0].Start -ge $sync.CallStart)
  $c4 = ($c2 -and (HoldsEmbedding $recs[0].Cmd))
  Say ("  adopt only if all four: constructor returned without throwing " + $c1 + ", exactly one new Roamer " + $c2 + ", it started at or after the call " + $c3 + ", its command line holds -Embedding " + $c4)
  if (-not ($c1 -and $c2 -and $c3 -and $c4)) {
    $sync.NoAdopt = $true
    if ($null -ne $app) { [GC]::SuppressFinalize($app) }
    $why = "not adopted: returned " + $c1 + ", one new " + $c2 + ", after the call " + $c3 + ", -Embedding " + $c4
    Say "  NOT ADOPTED. Nothing is closed, nothing is called on the object, not Dispose, and its finalizer is suppressed if there is an object"
    $lines = @()
    foreach ($r in $recs) {
      $tk = [long]0; if ($null -ne $r.Start) { $tk = $r.Start.Ticks }
      $lines += (UnprovedLine $r.Id $tk $why)
      Say ("  pid " + $r.Id + " is LEFT RUNNING, because it is not proved ours")
    }
    if ($lines.Count -gt 0) {
      try { AppendUnproved $unproved $lines; Say ("  written to " + (Mask $unproved) + ": " + $lines.Count + " starts. Later runs refuse while any of them is still running") }
      catch { Say ("  could NOT write " + (Mask $unproved) + ", " + (Err $_.Exception)) }
    }
    $outcome["3"] = "failed, " + $why
    throw "step 3 stopped"
  }
  $myPid = $recs[0].Id
  $myStart = $recs[0].Start
  $sync.MyTicks = $myStart.Ticks
  $sync.AdoptedAt = [DateTime]::Now
  $sync.MyPid = $myPid
  try { [System.IO.File]::WriteAllText($pidFile, [string]$myPid + "`r`n" + $myStart.ToString("o") + "`r`n", $utf8) } catch { Say ("  could not write " + $pidFile + ", " + (Err $_.Exception)) }
  Say ("  ADOPTED, MY PID IS " + $myPid + ", started " + $myStart.ToString("yyyy-MM-dd HH:mm:ss.fff"))
  Say ("  the process started " + ($myStart - $sync.CallStart).TotalSeconds.ToString("0.00") + " s after the call began and " + ($returnedAt - $myStart).TotalSeconds.ToString("0.00") + " s before the constructor returned")
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
  WindowsOf $myPid
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
  WindowsOf $myPid
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
  while ((ProcState $myPid $myStart) -eq "same" -and $sw.Elapsed.TotalSeconds -lt $QuitWaitSeconds) { Start-Sleep -Milliseconds 250 }
  $st6 = ProcState $myPid $myStart
  if ($st6 -eq "gone") { $goneAt = [DateTime]::Now }
  if ($st6 -eq "same") {
    $forceErr = $null
    try { Stop-Process -Id $myPid -Force -ErrorAction Stop } catch { $forceErr = $_.Exception }
    $sw2 = [Diagnostics.Stopwatch]::StartNew()
    while ((ProcState $myPid $myStart) -eq "same" -and $sw2.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 250 }
    $stAfter = ProcState $myPid $myStart
    if ($stAfter -eq "gone") { $goneAt = [DateTime]::Now }
    Say ("  " + (Stamp) + "  pid " + $myPid + " was STILL RUNNING " + $QuitWaitSeconds + " s after Dispose. It HAD TO BE FORCED with Stop-Process -Id " + $myPid + $(if ($null -ne $forceErr) { ", which threw " + (Err $forceErr) } else { "" }) + ". State after: " + $stAfter)
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
    $sf = ProcState $myPid $myStart
    if ($sf -eq "same") {
      $fe = $null
      try { Stop-Process -Id $myPid -Force -ErrorAction Stop } catch { $fe = $_.Exception }
      $swf = [Diagnostics.Stopwatch]::StartNew()
      while ((ProcState $myPid $myStart) -eq "same" -and $swf.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 250 }
      if ($null -ne $app -and -not $disposed) { [GC]::SuppressFinalize($app) }
      $sfAfter = ProcState $myPid $myStart
      if ($sfAfter -eq "gone" -and $null -eq $goneAt) { $goneAt = [DateTime]::Now }
      $closeNote = "pid " + $myPid + " was still the same process, CLOSED here with Stop-Process -Id " + $myPid + $(if ($null -ne $fe) { ", which threw " + (Err $fe) } else { "" }) + ", state after: " + $sfAfter
    } else {
      if ($sf -eq "gone" -and $null -eq $goneAt) { $goneAt = [DateTime]::Now }
      $closeNote = "pid " + $myPid + " state " + $sf + ", nothing to close"
    }
  } else { $closeNote = "no pid was adopted, so nothing is closed here" }
  $sync.Stop = $true
  Say "==== FINALLY ===="
  Say ("  " + (Stamp) + "  " + $closeNote)
  try { [void]$wps.EndInvoke($whandle) } catch { Say ("  the watchdog ended with " + (Err $_.Exception)) }
  $wps.Dispose()
  Say ("  watchdog forced anything: " + $(if ($sync.Forced -eq "") { "no" } else { $sync.Forced }))
  Say ("  watchdog passes " + $sync.Passes + ", longest pass " + ([double]$sync.PassMaxMs).ToString("0") + " ms, mean pass " + $(if ($sync.Passes -gt 0) { ($sync.PassTotalMs / $sync.Passes).ToString("0") } else { "0" }) + " ms, each followed by a 500 ms sleep, lines it could not write " + $sync.WriteErrors.Count)
  foreach ($we in $sync.WriteErrors) { Say ("    " + $we) }
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
          elseif ($pst.Ticks -eq $s.Ticks) { $state = "STILL RUNNING" } else { $state = "exited, the id is now another process" }
        } catch { $state = "UNKNOWN, " + (Err $_.Exception) }
      }
    }
    Say ("  " + $s.Name + " pid " + $s.Id + ", parent " + $s.Parent + ": " + $state)
  }
  if ($sync.Seen.Count -eq 0) { Say "  none" }
  Say ""
  Say "---- the Roamers from step 2, at the end ----"
  foreach ($id in $beforeStart.Keys) { Say ("  pid " + $id + " started " + $beforeStart[$id].ToString("yyyy-MM-dd HH:mm:ss") + ": state " + (ProcState $id $beforeStart[$id]) + ", never touched") }
  if ($beforeStart.Count -eq 0) { Say "  there were none" }
  $lateNew = @(NewRoamers)
  Say ("  Roamers running now that were not in step 2: " + $lateNew.Count)
  foreach ($p in $lateNew) { Say ("    pid " + $p.Id + ", not closed by this probe") }
  Say ""

  Say "---- BADER'S SETTINGS, compared, and put back only when no other Navisworks ran ----"
  try {
    $why = New-Object System.Collections.Generic.List[string]
    foreach ($id in $beforeStart.Keys) { $why.Add("Roamer " + $id + " was running at step 2") }
    if ($myPid -eq 0) { $why.Add("no Navisworks was adopted, so which change is the probe's is UNKNOWN") }
    else {
      $myState = ProcState $myPid $myStart
      if ($myState -eq "gone" -and $null -eq $goneAt) { $goneAt = [DateTime]::Now }
      if ($myState -ne "gone") { $why.Add("the adopted Roamer " + $myPid + " reads " + $myState + ", not gone, so the probe's own Navisworks may still be running") }
    }
    foreach ($key in @($sync.RoamerSet.Keys)) {
      $kp = $key.Split('|')
      $sid = [int]$kp[0]; $stk = [long]$kp[1]
      if ($myPid -ne 0 -and $sid -eq $myPid -and $stk -eq $sync.MyTicks) { continue }
      $why.Add("Roamer " + $sid + " started " + ([DateTime]$stk).ToString("HH:mm:ss.fff") + " was new at a watchdog pass and is not the adopted one")
    }
    foreach ($zid in @($sync.ZeroSightings.Keys)) {
      $z = $sync.ZeroSightings[$zid]
      if ($myPid -ne 0 -and [int]$zid -eq $myPid -and $null -ne $goneAt -and $z[0] -ge $myStart -and $z[1] -le $goneAt) { continue }
      $why.Add("Roamer " + $zid + " was seen at a watchdog pass with no readable start time, and cannot be shown to be the adopted one")
    }
    foreach ($p in @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)) { $why.Add("Roamer " + $p.Id + " is running at the end") }
    $putBack = ($why.Count -eq 0)
    if ($putBack) { Say "  no other Navisworks ran from the backup to now and the adopted one is gone, so what changed is put back" }
    else {
      Say "  NOTHING IS PUT BACK AND NOTHING IS WRITTEN, because:"
      foreach ($w in $why) { Say ("    " + $w) }
      Say ("  the backup is kept in " + (Mask $work))
    }

    $regAfter = RegRead $regSub
    foreach ($f in $regAfter.Failed.Keys) { Say ("  could not read at the end " + (ShortKey $f) + ", " + $regAfter.Failed[$f] + ". Nothing is written under it") }
    $keysAll = @(@($regBefore.Read.Keys) + @($regAfter.Read.Keys) | Sort-Object -Unique)
    $changes = New-Object System.Collections.Generic.List[object]
    foreach ($k in $keysAll) {
      $short = ShortKey $k
      $inB = $regBefore.Read.ContainsKey($k)
      $inA = $regAfter.Read.ContainsKey($k)
      $failA = UnderFailed $k $regAfter.Failed
      if (-not $inB -and $inA) {
        $parent = ParentKey $k
        $vals = $regAfter.Read[$k]
        $desc = (@($vals.Keys) | Sort-Object | ForEach-Object { (ValueLabel $_) + " = " + (RegText $vals[$_]) }) -join " | "
        if (-not $regBefore.Read.ContainsKey($parent) -and $regAfter.Read.ContainsKey($parent)) { Say ("  key APPEARED " + $short + ", " + $vals.Count + " values: " + $desc + ". It goes with its parent"); continue }
        $ok = ($regBefore.Read.ContainsKey($parent) -and $regAfter.Read.ContainsKey($parent) -and -not (UnderFailed $parent $regBefore.Failed) -and -not (UnderFailed $parent $regAfter.Failed) -and -not $failA -and -not (FailedBelow $k $regAfter.Failed))
        Say ("  key APPEARED " + $short + ", " + $vals.Count + " values: " + $desc + $(if ($ok) { "" } else { ". Not removable, it or its parent was not read at both ends" }))
        $changes.Add([pscustomobject]@{ Op = "DeleteKey"; Key = $k; Name = $null; Old = $null; Ok = $ok })
        continue
      }
      if ($inB -and -not $inA) {
        if ($failA) { Say ("  key " + $short + " could not be read at the end, not compared, nothing written under it"); continue }
        $parent = ParentKey $k
        $ok = (-not (UnderFailed $parent $regBefore.Failed) -and -not (UnderFailed $parent $regAfter.Failed))
        Say ("  key WENT " + $short + ", " + $regBefore.Read[$k].Count + " values before" + $(if ($ok) { "" } else { ". Not writable, its parent was not read at both ends" }))
        $changes.Add([pscustomobject]@{ Op = "CreateKey"; Key = $k; Name = $null; Old = $null; Ok = $ok })
        foreach ($n in @($regBefore.Read[$k].Keys | Sort-Object)) {
          Say ("  " + $short + "  " + (ValueLabel $n) + "  old " + (RegText $regBefore.Read[$k][$n]) + "  new absent")
          $changes.Add([pscustomobject]@{ Op = "Set"; Key = $k; Name = $n; Old = $regBefore.Read[$k][$n]; Ok = $ok })
        }
        continue
      }
      $bv = $regBefore.Read[$k]; $av = $regAfter.Read[$k]
      $ok = (-not $failA -and -not (UnderFailed $k $regBefore.Failed))
      foreach ($n in @(@($bv.Keys) + @($av.Keys) | Sort-Object -Unique)) {
        if (RegSame $bv[$n] $av[$n]) { continue }
        Say ("  " + $short + "  " + (ValueLabel $n) + "  old " + (RegText $bv[$n]) + "  new " + (RegText $av[$n]) + $(if ($ok) { "" } else { ". Not writable, the key was not read at both ends" }))
        if ($null -eq $bv[$n]) { $changes.Add([pscustomobject]@{ Op = "DeleteValue"; Key = $k; Name = $n; Old = $null; Ok = $ok }) }
        else { $changes.Add([pscustomobject]@{ Op = "Set"; Key = $k; Name = $n; Old = $bv[$n]; Ok = $ok }) }
      }
    }
    Say ("  registry: " + @($changes | Where-Object { $_.Op -ne "CreateKey" }).Count + " values or keys differ from the backup")
    if ($putBack) {
      $done = 0; $bad = 0; $skipped = 0
      foreach ($c in $changes) {
        if (-not $c.Ok) { $skipped++; continue }
        try {
          if ($c.Op -eq "DeleteKey") { [Microsoft.Win32.Registry]::CurrentUser.DeleteSubKeyTree((HkcuSub $c.Key), $false) }
          elseif ($c.Op -eq "CreateKey") { $rk = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey((HkcuSub $c.Key)); $rk.Close() }
          elseif ($c.Op -eq "Set") { $rk = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey((HkcuSub $c.Key)); try { $rk.SetValue($c.Name, $c.Old.Data, $c.Old.Kind) } finally { $rk.Close() } }
          elseif ($c.Op -eq "DeleteValue") { $rk = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey((HkcuSub $c.Key), $true); if ($null -ne $rk) { try { $rk.DeleteValue($c.Name, $false) } finally { $rk.Close() } } }
          if ($c.Op -ne "CreateKey") { $done++ }
        } catch { $bad++; Say ("  could not put back " + (ShortKey $c.Key) + " " + (ValueLabel $c.Name) + ", " + (Err $_.Exception)) }
      }
      $regCheck = RegRead $regSub
      $left = 0
      foreach ($c in $changes) {
        if (-not $c.Ok) { continue }
        $has = $regCheck.Read.ContainsKey($c.Key)
        $differs = $false
        if ($c.Op -eq "DeleteKey") { $differs = $has }
        elseif ($c.Op -eq "CreateKey") { $differs = -not $has }
        elseif ($c.Op -eq "Set") { $differs = (-not $has -or -not (RegSame $c.Old $regCheck.Read[$c.Key][$c.Name])) }
        elseif ($c.Op -eq "DeleteValue") { $differs = ($has -and $null -ne $regCheck.Read[$c.Key][$c.Name]) }
        if ($differs) { $left++; Say ("  still different after putting back: " + (ShortKey $c.Key) + " " + (ValueLabel $c.Name)) }
      }
      Say ("  registry: " + $done + " put back, " + $bad + " failed, " + $skipped + " not writable, and read again " + $left + " still differ")
    } else { Say "  registry: nothing written" }

    $sAfter = SettingsRead $nwAppData
    $fchanges = New-Object System.Collections.Generic.List[object]
    foreach ($rel in @(@($filesBefore.Keys) + @($notBacked.Keys) + @($sAfter.Files.Keys | Where-Object { $_ -notlike "AutoSave\*" }) | Sort-Object -Unique)) {
      $b = $filesBefore[$rel]; $a = $sAfter.Files[$rel]
      if ($notBacked.ContainsKey($rel)) { Say ("  file NOT BACKED UP at the start " + $rel + "  old " + (FileText $notBacked[$rel]) + "  new " + (FileText $a) + ". Never written"); continue }
      if ($null -eq $b) { Say ("  file APPEARED " + $rel + ", " + (FileText $a) + ". Left in place, the loop deletes only from its own folder"); continue }
      if ($null -ne $a -and $null -eq $a.Hash) { Say ("  file IN USE at the end " + $rel + "  old " + (FileText $b) + "  new " + (FileText $a) + ". Its content could not be read, so it is not compared and never written"); continue }
      if ($null -ne $a -and $a.Hash -eq $b.Hash) { continue }
      $dest = Join-Path $nwAppData $rel
      $ok = -not (FolderFailed (Split-Path $dest -Parent) $sAfter.Failed)
      Say ("  file " + $(if ($null -eq $a) { "WENT" } else { "CHANGED" }) + " " + $rel + "  old " + (FileText $b) + "  new " + (FileText $a) + $(if ($ok) { "" } else { ". Not writable, its folder could not be listed at the end" }))
      $afterHash = $null; if ($null -ne $a) { $afterHash = $a.Hash }
      $fchanges.Add([pscustomobject]@{ Rel = $rel; Dest = $dest; Went = ($null -eq $a); AfterHash = $afterHash; Old = $b; Ok = $ok })
    }
    Say ("  files: " + $fchanges.Count + " differ from the backup")
    if ($putBack) {
      $fput = 0; $fbad = 0; $fskip = 0
      foreach ($fc in $fchanges) {
        if (-not $fc.Ok) { $fskip++; continue }
        try {
          if ($fc.Went) {
            if (Test-Path -LiteralPath $fc.Dest) { Say ("  " + $fc.Rel + " is back just before the copy, not written"); $fskip++; continue }
            if (-not (Test-Path -LiteralPath (Split-Path $fc.Dest -Parent))) { Say ("  " + $fc.Rel + ", its folder went too, not written"); $fskip++; continue }
          } else {
            if (-not (Test-Path -LiteralPath $fc.Dest)) { Say ("  " + $fc.Rel + " went just before the copy, not written"); $fskip++; continue }
            $nowHash = (Get-FileHash -LiteralPath $fc.Dest -Algorithm SHA256).Hash
            if ($nowHash -ne $fc.AfterHash) { Say ("  " + $fc.Rel + " changed again after the compare, not written"); $fskip++; continue }
          }
          Copy-Item -LiteralPath (Join-Path $appBackup $fc.Rel) -Destination $fc.Dest -Force
          $back = (Get-FileHash -LiteralPath $fc.Dest -Algorithm SHA256).Hash
          if ($back -eq $fc.Old.Hash) { Say ("  " + $fc.Rel + " put back, read back, sha256 matches the backup"); $fput++ } else { Say ("  " + $fc.Rel + " put back but the read back sha256 is " + $back.Substring(0, 12)); $fbad++ }
        } catch { Say ("  " + $fc.Rel + " could not be put back, " + (Err $_.Exception)); $fbad++ }
      }
      Say ("  files: " + $fput + " put back and matching, " + $fbad + " failed, " + $fskip + " not written")
    } else { Say "  files: nothing written" }
    $ach = 0
    foreach ($rel in @(@($autoBefore.Keys) + @($sAfter.Files.Keys | Where-Object { $_ -like "AutoSave\*" }) | Sort-Object -Unique)) {
      $b = $autoBefore[$rel]; $a = $sAfter.Files[$rel]
      if ($null -ne $b -and $null -ne $a -and $null -ne $a.Hash -and $a.Hash -eq $b.Hash) { continue }
      $ach++
      Say ("  AutoSave " + $rel + "  old " + (FileText $b) + "  new " + (FileText $a) + ". Not backed up, never put back")
    }
    Say ("  AutoSave files added, changed, gone or unreadable: " + $ach)
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
