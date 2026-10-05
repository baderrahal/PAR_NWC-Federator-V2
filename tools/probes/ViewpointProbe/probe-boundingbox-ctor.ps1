param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# P5 of Q114, the views by team design, section 3. Does Autodesk.Navisworks.Api.BoundingBox3D
# have a public constructor taking two Point3D.
#
# Yes means FramingBox's two corners can build the box ZoomBox takes. No means only the camera
# arithmetic route is probed in P16.
#
# Read off the metadata alone with ReflectionOnlyLoadFrom, so no line of the DLL runs and no
# Navisworks is started. The DLL is tested by its one full path and never searched for. What a
# box built this way holds, and what ZoomBox does with it, is not read here.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path -LiteralPath $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

Write-Output ("ROAMER    Get-Process Roamer before the read: " + @(Get-Process Roamer -ErrorAction SilentlyContinue).Count + " processes")
$asm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($api)
$fi = Get-Item -LiteralPath $api
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version + "   file " + $fi.VersionInfo.FileVersion + ", " + $fi.Length + " bytes, loaded reflection only: " + $asm.ReflectionOnly)
Write-Output ("FILE      " + $api)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

function Vis($m) {
  if ($m.IsPublic) { return "public" }
  if ($m.IsFamily) { return "protected" }
  if ($m.IsAssembly) { return "internal" }
  return "private"
}

function Sig($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.FullName + " " + $q.Name) }
  $static = ""
  if ($m.IsStatic) { $static = "static " }
  $ret = ""
  if ($m -is [System.Reflection.MethodInfo]) { $ret = $m.ReturnType.FullName + " " }
  return ((Vis $m) + " " + $static + $ret + $m.Name + "(" + ($ps -join ", ") + ")")
}

function TypeLine($t) {
  $tv = "not public"
  if ($t.IsPublic) { $tv = "public" }
  $kind = "class"
  if ($t.IsValueType) { $kind = "struct" }
  if ($t.IsAbstract -and $t.IsSealed) { $kind = "static class" } elseif ($t.IsAbstract) { $kind = "abstract class" }
  $base = "none"
  if ($null -ne $t.BaseType) { $base = $t.BaseType.FullName }
  return ($tv + " " + $kind + ", base " + $base)
}

$box = $asm.GetType("Autodesk.Navisworks.Api.BoundingBox3D")
$pt = $asm.GetType("Autodesk.Navisworks.Api.Point3D")
$vp = $asm.GetType("Autodesk.Navisworks.Api.Viewpoint")

foreach ($t in @(@("Autodesk.Navisworks.Api.BoundingBox3D", $box), @("Autodesk.Navisworks.Api.Point3D", $pt))) {
  Write-Output ("==== " + $t[0])
  if ($null -eq $t[1]) { Write-Output "  UNKNOWN: no such type in this assembly"; Write-Output ""; continue }
  Write-Output ("  " + (TypeLine $t[1]))
  Write-Output "  every constructor, public and not:"
  foreach ($c in $t[1].GetConstructors("Public,NonPublic,Instance,Static,DeclaredOnly")) { Write-Output ("    " + (Sig $c)) }
  Write-Output "  every public static method that returns this type:"
  foreach ($m in $t[1].GetMethods("Public,Static,DeclaredOnly")) {
    if ($m.ReturnType.FullName -eq $t[1].FullName) { Write-Output ("    " + (Sig $m)) }
  }
  Write-Output ""
}

Write-Output "==== Autodesk.Navisworks.Api.Viewpoint, every member named ZoomBox"
if ($null -eq $vp) { Write-Output "  UNKNOWN: no such type in this assembly" }
else { foreach ($m in $vp.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) { if ($m.Name -eq "ZoomBox") { Write-Output ("  " + (Sig $m)) } } }
Write-Output ""

Write-Output "==== the answer"
$found = $null
if ($null -ne $box -and $null -ne $pt) {
  foreach ($c in $box.GetConstructors("Public,Instance,DeclaredOnly")) {
    $ps = $c.GetParameters()
    if ($ps.Count -eq 2 -and $ps[0].ParameterType.FullName -eq $pt.FullName -and $ps[1].ParameterType.FullName -eq $pt.FullName) { $found = $c }
  }
}
if ($null -eq $box -or $null -eq $pt) {
  Write-Output "  P5 UNKNOWN   BoundingBox3D or Point3D was not found in this assembly"
} elseif ($null -ne $found) {
  Write-Output ("  P5 YES   " + (Sig $found))
  Write-Output ("  first parameter " + $found.GetParameters()[0].Name + ", second " + $found.GetParameters()[1].Name)
  $body = $found.GetMethodBody()
  $il = "UNKNOWN"
  if ($null -ne $body) { $il = [string]$body.GetILAsByteArray().Length + " bytes of IL" }
  Write-Output ("  implementation " + $found.GetMethodImplementationFlags() + ", " + $il)
  Write-Output "  what the box holds when built, and whether min must be below max: UNKNOWN here, the constructor is not invoked"
} else {
  Write-Output "  P5 NO    no public instance constructor BoundingBox3D(Point3D, Point3D)"
}
Write-Output ""
Write-Output ("ROAMER    Get-Process Roamer after the read: " + @(Get-Process Roamer -ErrorAction SilentlyContinue).Count + " processes")
