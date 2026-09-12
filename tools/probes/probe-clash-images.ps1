param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"
$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
$clash = Join-Path $nw "Autodesk.Navisworks.Clash.dll"
foreach ($f in @($api, $clash)) { if (-not (Test-Path $f)) { Write-Output "UNKNOWN: missing $f"; exit 1 } }

$aApi = [System.Reflection.Assembly]::LoadFrom($api)
$aClash = [System.Reflection.Assembly]::LoadFrom($clash)
Write-Output ("API   " + $aApi.GetName().Version + "   CLASH " + $aClash.GetName().Version)

function Sig($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.FullName + " " + $q.Name) }
  $vis = "private"
  if ($m.IsPublic) { $vis = "public" } elseif ($m.IsFamily) { $vis = "protected" } elseif ($m.IsAssembly) { $vis = "internal" }
  $ret = "void"
  if ($m -is [System.Reflection.MethodInfo]) { $ret = $m.ReturnType.FullName }
  return ("  " + $vis + " " + $ret + " " + $m.DeclaringType.FullName + "." + $m.Name + "(" + ($ps -join ", ") + ")")
}

Write-Output ""
Write-Output "==== 1. Everything returning a Bitmap or Image, both assemblies ===="
foreach ($asm in @($aApi, $aClash)) {
  foreach ($ty in $asm.GetTypes()) {
    foreach ($m in $ty.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
      $rt = $m.ReturnType.FullName
      if ($rt -eq "System.Drawing.Bitmap" -or $rt -eq "System.Drawing.Image") { Write-Output (Sig $m) }
    }
  }
}

Write-Output ""
Write-Output "==== 2. Everything whose name suggests an image or a render, both assemblies ===="
$words = @("image","render","snapshot","thumbnail","bitmap","capture","export","tofile","savefile")
foreach ($asm in @($aApi, $aClash)) {
  foreach ($ty in $asm.GetTypes()) {
    foreach ($m in $ty.GetMethods("Public,Static,Instance,DeclaredOnly")) {
      $low = $m.Name.ToLowerInvariant()
      foreach ($w in $words) {
        if ($low.Contains($w)) { Write-Output (Sig $m); break }
      }
    }
  }
}

Write-Output ""
Write-Output "==== 3. ImageGenerationStyle ===="
$t = $aClash.GetType("Autodesk.Navisworks.Api.Clash.ImageGenerationStyle")
if ($null -eq $t) { $t = $aApi.GetType("Autodesk.Navisworks.Api.ImageGenerationStyle") }
if ($null -eq $t) {
  Write-Output "  UNKNOWN: type not found by either name. Searching:"
  foreach ($asm in @($aApi, $aClash)) {
    foreach ($ty in $asm.GetTypes()) { if ($ty.Name -like "*ImageGeneration*") { Write-Output ("  found " + $ty.FullName) } }
  }
} else {
  Write-Output ("  " + $t.FullName + ", underlying " + [Enum]::GetUnderlyingType($t).Name)
  foreach ($n in [Enum]::GetNames($t)) { Write-Output ("    {0} = {1}" -f $n, [int]([Enum]::Parse($t,$n))) }
}

Write-Output ""
Write-Output "==== 4. The two clash methods, exact ===="
$d = $aClash.GetType("Autodesk.Navisworks.Api.Clash.DocumentClashTests")
foreach ($n in @("TestsImageForResult","TestsViewpointForResult")) {
  foreach ($m in $d.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    if ($m.Name -eq $n) { Write-Output (Sig $m) }
  }
}

Write-Output ""
Write-Output "==== 5. Viewpoint: how one is applied to the view ===="
$vp = $aApi.GetType("Autodesk.Navisworks.Api.Viewpoint")
Write-Output ("  Viewpoint base: " + $vp.BaseType.FullName + "  IDisposable: " + [bool]([System.IDisposable].IsAssignableFrom($vp)))
$doc = $aApi.GetType("Autodesk.Navisworks.Api.Document")
Write-Output "  Document members naming viewpoint or currentview:"
foreach ($m in $doc.GetMembers("Public,Instance,Static,DeclaredOnly")) {
  $low = $m.Name.ToLowerInvariant()
  if ($low.Contains("viewpoint") -or $low.Contains("currentview") -or $low.Contains("view")) {
    if ($m -is [System.Reflection.PropertyInfo]) {
      Write-Output ("    property " + $m.PropertyType.FullName + " " + $m.Name + "  canwrite=" + $m.CanWrite)
    } elseif ($m -is [System.Reflection.MethodInfo] -and -not $m.IsSpecialName) {
      Write-Output (Sig $m)
    }
  }
}

Write-Output ""
Write-Output "  DocumentView / anything holding a current viewpoint:"
foreach ($ty in $aApi.GetTypes()) {
  if ($ty.Name -notlike "*View*") { continue }
  foreach ($m in $ty.GetProperties("Public,Instance,DeclaredOnly")) {
    if ($m.Name.ToLowerInvariant().Contains("currentviewpoint")) {
      Write-Output ("    " + $ty.FullName + "." + $m.Name + " : " + $m.PropertyType.FullName + "  canwrite=" + $m.CanWrite)
    }
  }
}

Write-Output ""
Write-Output "==== 6. Anything on ClashResult naming a viewpoint or an image ===="
foreach ($n in @("Autodesk.Navisworks.Api.Clash.ClashResult","Autodesk.Navisworks.Api.Clash.IClashResult","Autodesk.Navisworks.Api.Clash.ClashResultGroup")) {
  $ty = $aClash.GetType($n)
  if ($null -eq $ty) { Write-Output ("  UNKNOWN: no type " + $n); continue }
  Write-Output ("  " + $ty.FullName)
  foreach ($m in $ty.GetProperties("Public,Instance,DeclaredOnly")) {
    Write-Output ("    " + $m.PropertyType.Name + " " + $m.Name + "  canwrite=" + $m.CanWrite)
  }
}
