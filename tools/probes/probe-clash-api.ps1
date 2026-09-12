param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"
$nw = $NavisworksPath
$clash = Join-Path $nw "Autodesk.Navisworks.Clash.dll"
if (-not (Test-Path $clash)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Clash.dll at $clash"; exit 1 }

$asm = [System.Reflection.Assembly]::LoadFrom($clash)
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version)
Write-Output ""

Write-Output "---- ClashTestStatus, every value ----"
$t = $asm.GetType("Autodesk.Navisworks.Api.Clash.ClashTestStatus")
if ($null -eq $t) { Write-Output "UNKNOWN: no ClashTestStatus type" } else {
  Write-Output ("underlying type: " + [Enum]::GetUnderlyingType($t).FullName)
  foreach ($n in [Enum]::GetNames($t)) {
    Write-Output ("  {0} = {1}" -f $n, [int]([Enum]::Parse($t,$n)))
  }
}
Write-Output ""

Write-Output "---- ClashTest.Status ----"
$ct = $asm.GetType("Autodesk.Navisworks.Api.Clash.ClashTest")
$p = $ct.GetProperty("Status")
if ($null -eq $p) { Write-Output "UNKNOWN: no Status property on ClashTest" } else {
  Write-Output ("  " + $p.PropertyType.FullName + " Status  canread=" + $p.CanRead + " canwrite=" + $p.CanWrite)
}
Write-Output ""

Write-Output "---- anything in the Clash namespace naming stale, altered, out of date, dirty, uptodate ----"
$words = @("stale","alter","outofdate","outdate","dirty","uptodate","invalid","needsrun","rerun","expire")
$hits = 0
foreach ($ty in $asm.GetTypes()) {
  if (-not $ty.FullName.Contains("Clash")) { continue }
  foreach ($m in $ty.GetMembers("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    $low = $m.Name.ToLowerInvariant()
    foreach ($w in $words) {
      if ($low.Contains($w)) { Write-Output ("  " + $ty.FullName + "." + $m.Name + "  (" + $m.MemberType + ")"); $hits++ ; break }
    }
  }
}
if ($hits -eq 0) { Write-Output "  none, public or not, across every type whose full name holds Clash" }
Write-Output ""

Write-Output "---- Compact on DocumentClashTests ----"
$d = $asm.GetType("Autodesk.Navisworks.Api.Clash.DocumentClashTests")
$any = $false
foreach ($m in $d.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
  if (-not $m.Name.ToLowerInvariant().Contains("compact")) { continue }
  $any = $true
  $ps = @()
  foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
  $vis = "private"
  if ($m.IsPublic) { $vis = "public" } elseif ($m.IsFamily) { $vis = "protected" } elseif ($m.IsAssembly) { $vis = "internal" }
  Write-Output ("  " + $vis + " " + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
}
if (-not $any) { Write-Output "  UNKNOWN: nothing named Compact on DocumentClashTests" }
Write-Output ""

Write-Output "---- Compact anywhere else in the assembly ----"
foreach ($ty in $asm.GetTypes()) {
  foreach ($m in $ty.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    if ($m.Name.ToLowerInvariant().Contains("compact") -and $ty.FullName -ne $d.FullName) {
      Write-Output ("  " + $ty.FullName + "." + $m.Name)
    }
  }
}
Write-Output ""

Write-Output "---- ClashResultStatus, for the record ----"
$rs = $asm.GetType("Autodesk.Navisworks.Api.Clash.ClashResultStatus")
foreach ($n in [Enum]::GetNames($rs)) { Write-Output ("  {0} = {1}" -f $n, [int]([Enum]::Parse($rs,$n))) }
