param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# P7 of Q114, the views by team design, section 3. Does LcOpRedlineList, the type of
# SavedViewpoint.Redlines, expose a count.
#
# Yes means P20 reads it on a tool view and on one Bader draws a redline on. No means redlines
# are UNKNOWN in the mark's judge, and the log says so.
#
# Read off the metadata alone with ReflectionOnlyLoadFrom, so no line of the DLL runs and no
# Navisworks is started. The DLL is tested by its one full path and never searched for. The
# type of Redlines is read off the property itself, not assumed by name, and the assembly that
# defines it is named. What the count reads on a real view is a fact of a running Navisworks
# and is not read here, P20.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path -LiteralPath $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

$resolveFailures = New-Object System.Collections.Generic.List[string]
$roResolve = [ResolveEventHandler]{
  param($s, $e)
  $an = New-Object System.Reflection.AssemblyName($e.Name)
  foreach ($x in [AppDomain]::CurrentDomain.ReflectionOnlyGetAssemblies()) { if ($x.GetName().Name -eq $an.Name) { return $x } }
  $cand = Join-Path $nw ($an.Name + ".dll")
  if (Test-Path -LiteralPath $cand) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand) }
  try { return [System.Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { $resolveFailures.Add($e.Name + ": " + $_.Exception.Message); return $null }
}
[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($roResolve)

Write-Output ("ROAMER    Get-Process Roamer before the read: " + @(Get-Process Roamer -ErrorAction SilentlyContinue).Count + " processes")
$asm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($api)
$fi = Get-Item -LiteralPath $api
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version + "   file " + $fi.VersionInfo.FileVersion + ", " + $fi.Length + " bytes, loaded reflection only: " + $asm.ReflectionOnly)
Write-Output ("FILE      " + $api)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

$declared = [System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static,DeclaredOnly"
$publicAll = [System.Reflection.BindingFlags]"Public,Instance,Static,FlattenHierarchy"

function TName($t) { if ($null -eq $t) { return "null" } ; if ($null -ne $t.FullName) { return $t.FullName } ; return $t.Name }

function MethodLine($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) {
    $dir = ""
    if ($q.IsOut) { $dir = "out " } elseif ($q.ParameterType.IsByRef) { $dir = "ref " }
    $ps += ($dir + (TName $q.ParameterType).TrimEnd("&") + " " + $q.Name)
  }
  $vis = "not public"
  if ($m.IsPublic) { $vis = "public" }
  $st = ""
  if ($m.IsStatic) { $st = "static " }
  $ret = ""
  if ($m -is [System.Reflection.MethodInfo]) { $ret = (TName $m.ReturnType) + " " }
  return ($vis + " " + $st + $ret + $m.Name + "(" + ($ps -join ", ") + ")   declared on " + $m.DeclaringType.Name)
}

function PropLine($p) {
  $acc = @()
  $g = $p.GetGetMethod($true)
  $s = $p.GetSetMethod($true)
  if ($null -ne $g) { if ($g.IsPublic) { $acc += "public get" } else { $acc += "non-public get" } }
  if ($null -ne $s) { if ($s.IsPublic) { $acc += "public set" } else { $acc += "non-public set" } }
  $ix = ""
  $ip = $p.GetIndexParameters()
  if ($ip.Count -gt 0) { $ix = "[" + (($ip | ForEach-Object { (TName $_.ParameterType) + " " + $_.Name }) -join ", ") + "]" }
  return ("property " + (TName $p.PropertyType) + " " + $p.Name + $ix + " { " + ($acc -join "; ") + " }   declared on " + $p.DeclaringType.Name)
}

function DumpType($t) {
  if ($null -eq $t) { Write-Output "  UNKNOWN: no such type"; return }
  $kind = "class"
  if ($t.IsInterface) { $kind = "interface" } elseif ($t.IsEnum) { $kind = "enum" } elseif ($t.IsValueType) { $kind = "struct" }
  $vis = "not public"
  if ($t.IsPublic) { $vis = "public" }
  Write-Output ("  " + $vis + " " + $kind + " " + (TName $t) + ", defined in " + $t.Assembly.GetName().Name + " " + $t.Assembly.GetName().Version)
  $chain = @()
  $b = $t.BaseType
  while ($null -ne $b) { $chain += (TName $b) ; $b = $b.BaseType }
  if ($chain.Count -eq 0) { $chain = @("none") }
  Write-Output ("  base types, nearest first: " + ($chain -join " <- "))
  $ifs = @()
  foreach ($i in $t.GetInterfaces()) { $ifs += (TName $i) }
  if ($ifs.Count -eq 0) { $ifs = @("none") }
  Write-Output ("  interfaces: " + ($ifs -join ", "))
  Write-Output "  constructors:"
  foreach ($c in $t.GetConstructors($declared)) { Write-Output ("    " + (MethodLine $c)) }
  Write-Output "  public properties, inherited ones included:"
  foreach ($p in ($t.GetProperties($publicAll) | Sort-Object Name)) { Write-Output ("    " + (PropLine $p)) }
  Write-Output "  public methods, inherited ones included, accessors left out:"
  foreach ($m in ($t.GetMethods($publicAll) | Sort-Object Name)) { if (-not $m.IsSpecialName) { Write-Output ("    " + (MethodLine $m)) } }
}

$sv = $asm.GetType("Autodesk.Navisworks.Api.SavedViewpoint")
Write-Output "==== Autodesk.Navisworks.Api.SavedViewpoint, every member whose name or type holds Redline"
$redType = $null
$editType = $null
if ($null -eq $sv) { Write-Output "  UNKNOWN: no SavedViewpoint in this assembly" }
else {
  foreach ($p in $sv.GetProperties($declared)) {
    $l = PropLine $p
    if ($l -match "Redline") { Write-Output ("  " + $l) }
    if ($p.Name -eq "Redlines") { $redType = $p.PropertyType }
  }
  foreach ($m in $sv.GetMethods($declared)) {
    if ($m.IsSpecialName) { continue }
    $l = MethodLine $m
    if ($l -match "Redline") { Write-Output ("  " + $l) }
    if ($m.Name -eq "EditRedlines") { $editType = $m.ReturnType }
  }
}
Write-Output ("  type of Redlines read off the property: " + (TName $redType))
Write-Output ("  return type of EditRedlines: " + (TName $editType))
Write-Output ""

Write-Output "==== the type of SavedViewpoint.Redlines"
DumpType $redType
Write-Output ""

$baseTypes = @()
if ($null -ne $redType) {
  $b = $redType.BaseType
  while ($null -ne $b -and (TName $b) -ne "System.Object") { $baseTypes += $b ; $b = $b.BaseType }
}
foreach ($bt in $baseTypes) {
  Write-Output ("==== base type " + (TName $bt) + ", members declared on it alone")
  Write-Output ("  defined in " + $bt.Assembly.GetName().Name)
  foreach ($p in $bt.GetProperties($declared)) { Write-Output ("    " + (PropLine $p)) }
  foreach ($m in $bt.GetMethods($declared)) { if (-not $m.IsSpecialName) { Write-Output ("    " + (MethodLine $m)) } }
  Write-Output ""
}

Write-Output "==== every type in the assembly of Redlines whose name holds Redline"
$redAsm = $asm
if ($null -ne $redType) { $redAsm = $redType.Assembly }
$found = @()
try { $found = @($redAsm.GetTypes() | Where-Object { $_.Name -like "*Redline*" } | Sort-Object FullName) }
catch [System.Reflection.ReflectionTypeLoadException] { $found = @($_.Exception.Types | Where-Object { $null -ne $_ -and $_.Name -like "*Redline*" } | Sort-Object FullName) }
foreach ($t in $found) {
  $vis = "not public"
  if ($t.IsPublic) { $vis = "public" }
  Write-Output ("  " + $vis + "   " + (TName $t))
}
Write-Output ""

Write-Output "==== the answer"
$countHits = @()
$ifaceHits = @()
if ($null -ne $redType) {
  foreach ($p in $redType.GetProperties($publicAll)) {
    if ($p.Name -match "^(Count|Length|Size|Num)" -and $null -ne $p.GetGetMethod($false)) { $countHits += (PropLine $p) }
  }
  foreach ($m in $redType.GetMethods($publicAll)) {
    if ($m.IsSpecialName) { continue }
    if ($m.Name -match "^(Count|Get_?Count|Size|Length|Num)" -and $m.GetParameters().Count -eq 0) { $countHits += (MethodLine $m) }
  }
  foreach ($i in $redType.GetInterfaces()) {
    $n = TName $i
    if ($n -match "ICollection|IReadOnlyCollection|IList|IEnumerable") { $ifaceHits += $n }
  }
}
Write-Output ("  public type of Redlines: " + (TName $redType))
Write-Output "  public members of it, inherited ones included, that read a count:"
if ($countHits.Count -eq 0) { Write-Output "    none" } else { foreach ($h in $countHits) { Write-Output ("    " + $h) } }
Write-Output "  how each counting member and ItemAt are implemented, read off the method body:"
if ($null -ne $redType) {
  foreach ($m in $redType.GetMethods($publicAll)) {
    if ($m.IsSpecialName) { continue }
    if ($m.Name -notmatch "^(Size|Count|ItemAt)$") { continue }
    $impl = $m.GetMethodImplementationFlags()
    $body = $m.GetMethodBody()
    $il = "no body"
    if ($null -ne $body) { $il = ([string]$body.GetILAsByteArray().Length + " bytes of IL") }
    Write-Output ("    " + $m.DeclaringType.Name + "." + $m.Name + "   implementation " + $impl + ", " + $il)
  }
}
Write-Output "  collection interfaces it implements:"
if ($ifaceHits.Count -eq 0) { Write-Output "    none" } else { foreach ($h in $ifaceHits) { Write-Output ("    " + $h) } }

if ($null -eq $redType) {
  Write-Output "  P7 UNKNOWN   SavedViewpoint.Redlines was not found in this assembly"
} elseif ($countHits.Count -gt 0) {
  Write-Output "  P7 YES   the type of SavedViewpoint.Redlines has a public member that reads a count"
} else {
  Write-Output "  P7 NO    the type of SavedViewpoint.Redlines has no public member that reads a count"
}
Write-Output "  what the count reads on a tool view and after a redline is drawn: UNKNOWN here, nothing is invoked, P20"
Write-Output ""
if ($resolveFailures.Count -gt 0) { Write-Output "RESOLVE FAILURES" ; foreach ($r in $resolveFailures) { Write-Output ("  " + $r) } ; Write-Output "" }
Write-Output ("ROAMER    Get-Process Roamer after the read: " + @(Get-Process Roamer -ErrorAction SilentlyContinue).Count + " processes")
