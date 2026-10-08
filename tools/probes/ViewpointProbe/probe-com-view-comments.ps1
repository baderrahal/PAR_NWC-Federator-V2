param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# P6 of Q114, the views by team design, section 3. Does the COM saved view InwOpView expose a
# member through which comments can be set on the view before it is added to a folder.
#
# Yes means P9 tries that route first, which costs no extra call per view. No means the mark is
# DocumentSavedViewpoints.AddComment after the add.
#
# Read off the metadata alone with ReflectionOnlyLoadFrom, so no line of the DLL runs and no
# Navisworks is started. The DLL is tested by its one full path and never searched for. Whether
# a comment put on a view that is not yet in a folder survives the add is a fact of a running
# Navisworks and is not read here, P9.

$nw = $NavisworksPath
$interop = Join-Path $nw "Autodesk.Navisworks.Interop.ComApi.dll"
if (-not (Test-Path -LiteralPath $interop)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Interop.ComApi.dll at $interop"; exit 1 }

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
$asm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($interop)
$fi = Get-Item -LiteralPath $interop
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version + "   file " + $fi.VersionInfo.FileVersion + ", " + $fi.Length + " bytes, loaded reflection only: " + $asm.ReflectionOnly)
Write-Output ("FILE      " + $interop)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

$ns = "Autodesk.Navisworks.Api.Interop.ComApi."
$flags = [System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static,DeclaredOnly"

function TName($t) { if ($null -eq $t) { return "null" } ; if ($null -ne $t.FullName) { return $t.FullName } ; return $t.Name }

function DispId($m) {
  foreach ($a in [System.Reflection.CustomAttributeData]::GetCustomAttributes($m)) {
    if ($a.AttributeType.Name -eq "DispIdAttribute" -and $a.ConstructorArguments.Count -eq 1) { return ("dispid " + $a.ConstructorArguments[0].Value) }
  }
  return "dispid none"
}

function MethodLine($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) {
    $opt = ""
    if ($q.IsOptional) { $opt = "optional " }
    $dir = ""
    if ($q.IsOut) { $dir = "out " } elseif ($q.ParameterType.IsByRef) { $dir = "ref " }
    $ps += ($opt + $dir + (TName $q.ParameterType) + " " + $q.Name)
  }
  $vis = "not public"
  if ($m.IsPublic) { $vis = "public" }
  return ($vis + " " + (TName $m.ReturnType) + " " + $m.Name + "(" + ($ps -join ", ") + ")   " + (DispId $m))
}

function PropLine($p) {
  $acc = @()
  if ($null -ne $p.GetGetMethod($true)) { $acc += "get" }
  if ($null -ne $p.GetSetMethod($true)) { $acc += "set" }
  $ix = ""
  $ip = $p.GetIndexParameters()
  if ($ip.Count -gt 0) { $ix = "[" + (($ip | ForEach-Object { (TName $_.ParameterType) + " " + $_.Name }) -join ", ") + "]" }
  return ("property " + (TName $p.PropertyType) + " " + $p.Name + $ix + " { " + ($acc -join "; ") + " }   " + (DispId $p))
}

function DumpType($t) {
  if ($null -eq $t) { Write-Output "  UNKNOWN: no such type in this assembly"; return }
  $kind = "class"
  if ($t.IsInterface) { $kind = "interface" } elseif ($t.IsEnum) { $kind = "enum" } elseif ($t.IsValueType) { $kind = "struct" }
  $vis = "not public"
  if ($t.IsPublic) { $vis = "public" }
  $inh = @()
  foreach ($i in $t.GetInterfaces()) { $inh += $i.Name }
  if ($inh.Count -eq 0) { $inh = @("none") }
  $attrs = @()
  foreach ($a in [System.Reflection.CustomAttributeData]::GetCustomAttributes($t)) { $attrs += $a.AttributeType.Name }
  Write-Output ("  " + $vis + " " + $kind + ", inherits " + ($inh -join ", ") + ", attributes " + ($attrs -join ", "))
  Write-Output "  properties declared on it:"
  foreach ($p in $t.GetProperties($flags)) { Write-Output ("    " + (PropLine $p)) }
  Write-Output "  methods declared on it, accessors left out:"
  foreach ($m in $t.GetMethods($flags)) { if (-not $m.IsSpecialName) { Write-Output ("    " + (MethodLine $m)) } }
}

$view = $asm.GetType($ns + "InwOpView")
$saved = $asm.GetType($ns + "InwOpSavedView")
$coll = $asm.GetType($ns + "InwCommentsColl")
$etype = $asm.GetType($ns + "nwEObjectType")
$state = $asm.GetType($ns + "InwOpState10")

foreach ($pair in @(@("InwOpView", $view), @("InwOpSavedView", $saved), @("InwCommentsColl", $coll))) {
  Write-Output ("==== " + $ns + $pair[0])
  DumpType $pair[1]
  Write-Output ""
}

Write-Output "==== every type in the assembly whose name holds Comment"
$commentTypes = @()
foreach ($t in $asm.GetTypes()) { if ($t.Name -like "*Comment*") { $commentTypes += $t } }
foreach ($t in ($commentTypes | Sort-Object Name)) { Write-Output ("  " + (TName $t)) }
Write-Output ""
foreach ($t in ($commentTypes | Sort-Object Name)) {
  if ($t.FullName -eq $coll.FullName) { continue }
  Write-Output ("==== " + (TName $t))
  DumpType $t
  if ($t.IsEnum) {
    foreach ($f in $t.GetFields("Public,Static")) { Write-Output ("    " + $f.Name + " = " + $f.GetRawConstantValue()) }
  }
  Write-Output ""
}

Write-Output "==== nwEObjectType, every value whose name holds Comment, View or Saved"
if ($null -eq $etype) { Write-Output "  UNKNOWN: no nwEObjectType in this assembly" }
else { foreach ($f in $etype.GetFields("Public,Static")) { if ($f.Name -match "Comment|View|Saved") { Write-Output ("  " + $f.Name + " = " + $f.GetRawConstantValue()) } } }
Write-Output ""

Write-Output "==== InwOpState10, every member whose name or types hold Comment, and ObjectFactory"
if ($null -eq $state) { Write-Output "  UNKNOWN: no InwOpState10 in this assembly" }
else {
  foreach ($m in $state.GetMethods($flags)) {
    $line = MethodLine $m
    if ($m.Name -eq "ObjectFactory" -or $line -match "Comment") { Write-Output ("  " + $line) }
  }
}
Write-Output ""

Write-Output "==== every member of every type whose name, return or parameter types hold Comment, Comment types left out"
foreach ($t in ($asm.GetTypes() | Sort-Object FullName)) {
  if ($t.Name -like "*Comment*") { continue }
  foreach ($p in $t.GetProperties($flags)) { $l = PropLine $p ; if ($l -match "Comment") { Write-Output ("  " + $t.Name + "   " + $l) } }
  foreach ($m in $t.GetMethods($flags)) { if ($m.IsSpecialName) { continue } ; $l = MethodLine $m ; if ($l -match "Comment") { Write-Output ("  " + $t.Name + "   " + $l) } }
}
Write-Output ""

Write-Output "==== the answer"
$viewHits = @()
if ($null -ne $view) {
  $all = @($view) + @($view.GetInterfaces())
  foreach ($t in $all) {
    foreach ($p in $t.GetProperties($flags)) { if ($p.Name -like "*Comment*" -or (TName $p.PropertyType) -like "*Comment*") { $viewHits += ($t.Name + "." + (PropLine $p)) } }
    foreach ($m in $t.GetMethods($flags)) { if ($m.IsSpecialName) { continue } ; if ($m.Name -like "*Comment*" -or (TName $m.ReturnType) -like "*Comment*") { $viewHits += ($t.Name + "." + (MethodLine $m)) } }
  }
}
$collAdd = @()
if ($null -ne $coll) {
  foreach ($m in $coll.GetMethods($flags)) { if ($m.IsSpecialName) { continue } ; if ($m.Name -match "^(Add|Insert|Replace|Set)") { $collAdd += (MethodLine $m) } }
  foreach ($p in $coll.GetProperties($flags)) { if ($null -ne $p.GetSetMethod($true)) { $collAdd += (PropLine $p) } }
}
$factory = @()
if ($null -ne $etype) { foreach ($f in $etype.GetFields("Public,Static")) { if ($f.Name -like "*Comment*") { $factory += ($f.Name + " = " + $f.GetRawConstantValue()) } } }

Write-Output "  members of InwOpView and the interfaces it inherits that name or return a comment type:"
if ($viewHits.Count -eq 0) { Write-Output "    none" } else { foreach ($h in $viewHits) { Write-Output ("    " + $h) } }
Write-Output "  members of InwCommentsColl that add, insert, replace or set:"
if ($null -eq $coll) { Write-Output "    UNKNOWN: no InwCommentsColl" } elseif ($collAdd.Count -eq 0) { Write-Output "    none" } else { foreach ($h in $collAdd) { Write-Output ("    " + $h) } }
Write-Output "  nwEObjectType values that make a comment through ObjectFactory:"
if ($factory.Count -eq 0) { Write-Output "    none" } else { foreach ($h in $factory) { Write-Output ("    " + $h) } }

if ($null -eq $view) {
  Write-Output "  P6 UNKNOWN   InwOpView was not found in this assembly"
} elseif ($viewHits.Count -gt 0 -and $collAdd.Count -gt 0 -and $factory.Count -gt 0) {
  Write-Output "  P6 YES   InwOpView hands out its comments collection, the collection has a member that adds, and ObjectFactory has a comment type to add"
} elseif ($viewHits.Count -gt 0) {
  Write-Output "  P6 PARTIAL   InwOpView names a comment member, but the collection has no adding member or no comment object type exists, read the lists above"
} else {
  Write-Output "  P6 NO    InwOpView names no comment member"
}
Write-Output "  whether a comment added to a view that is not yet in a folder is kept by InwSavedViewsColl.Add and reads back off SavedItem.Comments: UNKNOWN here, nothing is invoked, P9"
Write-Output ""
if ($resolveFailures.Count -gt 0) { Write-Output "RESOLVE FAILURES" ; foreach ($r in $resolveFailures) { Write-Output ("  " + $r) } ; Write-Output "" }
Write-Output ("ROAMER    Get-Process Roamer after the read: " + @(Get-Process Roamer -ErrorAction SilentlyContinue).Count + " processes")
