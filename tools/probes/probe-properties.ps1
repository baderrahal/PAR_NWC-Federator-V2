param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F86, scan.md 5f. What the property API offers for walking every property of an item,
# and, for 5g, whether a search condition can be negated at all.
#
# Nothing in this tool has ever walked every property of an item. ClashHarvest reads named
# properties one at a time and stops at the first that answers. The probe F86 builds has
# to read every tab and every property under it and turn each value into the text a CSV
# cell holds, so this reads the collection types, the two name pairs, every reader on
# VariantData, and whether a Search can walk a whole model in one pass.
#
# Nothing is instantiated. The API needs the application host for that.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

$asm = [System.Reflection.Assembly]::LoadFrom($api)
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

$public = "Public,Instance,Static,DeclaredOnly"

function Show($m) {
  switch ($m.MemberType) {
    "Method" {
      if ($m.IsSpecialName) { return }
      $ps = @()
      foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
      $st = ""
      if ($m.IsStatic) { $st = "static " }
      Write-Output ("  " + $st + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
    }
    "Property" {
      Write-Output ("  " + $m.PropertyType.Name + " " + $m.Name + "  canread=" + $m.CanRead + " canwrite=" + $m.CanWrite)
    }
    "Constructor" {
      $ps = @()
      foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
      Write-Output ("  ctor(" + ($ps -join ", ") + ")")
    }
    "Event" {
      Write-Output ("  event " + $m.EventHandlerType.Name + " " + $m.Name)
    }
    default { }
  }
}

function Every($fullName) {
  $ty = $asm.GetType($fullName)
  Write-Output ("---- " + $fullName + ", every public member ----")
  if ($null -eq $ty) { Write-Output "  UNKNOWN: no such type in this assembly"; Write-Output ""; return }
  $base = $ty.BaseType
  if ($null -ne $base) { Write-Output ("  base " + $base.FullName) }
  $ifs = @()
  foreach ($i in $ty.GetInterfaces()) { $ifs += $i.Name }
  if ($ifs.Count -gt 0) { Write-Output ("  implements " + ($ifs -join ", ")) }
  foreach ($m in $ty.GetMembers($public)) { Show $m }
  Write-Output ""
}

function One($fullName, $member) {
  $ty = $asm.GetType($fullName)
  if ($null -eq $ty) { Write-Output ("  UNKNOWN: no type " + $fullName); return }
  $found = $false
  foreach ($m in $ty.GetMember($member, $public)) { Show $m; $found = $true }
  if (-not $found) { Write-Output ("  UNKNOWN: no public member " + $member + " on " + $fullName) }
}

Write-Output "---- ModelItem, the members the walk starts from ----"
foreach ($n in @("PropertyCategories", "Children", "Descendants", "DescendantsAndSelf", "Parent", "DisplayName", "ClassDisplayName", "ClassName", "IsComposite", "IsInsert", "IsLayer", "Model", "HasGeometry", "InstanceGuid")) {
  One "Autodesk.Navisworks.Api.ModelItem" $n
}
Write-Output ""

Every "Autodesk.Navisworks.Api.PropertyCategoryCollection"
Every "Autodesk.Navisworks.Api.PropertyCategory"
Every "Autodesk.Navisworks.Api.DataPropertyCollection"
Every "Autodesk.Navisworks.Api.DataProperty"
Every "Autodesk.Navisworks.Api.VariantData"
Every "Autodesk.Navisworks.Api.VariantDataType"

Write-Output "---- VariantDataType, every value ----"
$vdt = $asm.GetType("Autodesk.Navisworks.Api.VariantDataType")
if ($null -eq $vdt) { Write-Output "  UNKNOWN: no VariantDataType" } else {
  foreach ($n in [Enum]::GetNames($vdt)) { Write-Output ("  {0} = {1}" -f $n, [int]([Enum]::Parse($vdt, $n))) }
}
Write-Output ""

Every "Autodesk.Navisworks.Api.Search"
Every "Autodesk.Navisworks.Api.SearchCondition"
Every "Autodesk.Navisworks.Api.SearchConditionOptions"

Write-Output "---- SearchConditionOptions, every value, for 5g ----"
$sco = $asm.GetType("Autodesk.Navisworks.Api.SearchConditionOptions")
if ($null -eq $sco) { Write-Output "  UNKNOWN: no SearchConditionOptions" } else {
  foreach ($n in [Enum]::GetNames($sco)) { Write-Output ("  {0} = {1}" -f $n, [int]([Enum]::Parse($sco, $n))) }
}
Write-Output ""

Write-Output "---- Anything in the assembly whose name holds Negat or IgnoreString, for 5g ----"
$hits = 0
foreach ($ty in $asm.GetTypes()) {
  foreach ($m in $ty.GetMembers("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    $low = $m.Name.ToLowerInvariant()
    if ($low.Contains("negat") -or $low.Contains("ignorestring")) {
      Write-Output ("  " + $ty.FullName + "." + $m.Name + "  (" + $m.MemberType + ")"); $hits++
    }
  }
}
if ($hits -eq 0) { Write-Output "  none, public or not" }
Write-Output ""

Every "Autodesk.Navisworks.Api.ModelItemEnumerableCollection"
Every "Autodesk.Navisworks.Api.ModelItemCollection"

Write-Output "---- Model, the members that name the file and the root ----"
foreach ($n in @("FileName", "SourceFileName", "RootItem", "Units", "DisplayName")) { One "Autodesk.Navisworks.Api.Model" $n }
Write-Output ""
