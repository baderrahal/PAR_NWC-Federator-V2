param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F105, question 1, the half probe-viewpoints.ps1 does not read.
#
# probe-viewpoints.ps1 prints the .NET viewpoint types whole. It never opens the two COM
# DLLs, and src\Federator.Addin\Engine\SavedViewpoints.cs writes every clash viewpoint
# through them, InwOpView with ApplyHideAttribs, docs\history\scan.md 5m. So this reads,
# one line each, every Navisworks member that file calls, in the three assemblies it uses,
# and says FOUND with the signature read off the DLL, or NO MATCH with every member of that
# name that is there. Reflection only. Nothing is started and no member is called.
#
# Each DLL is tested by its one full path. The install folder is never searched.

$nw = $NavisworksPath
$paths = [ordered]@{
  Api     = (Join-Path $nw "Autodesk.Navisworks.Api.dll")
  ComApi  = (Join-Path $nw "Autodesk.Navisworks.ComApi.dll")
  Interop = (Join-Path $nw "Autodesk.Navisworks.Interop.ComApi.dll")
}
foreach ($k in $paths.Keys) {
  if (-not (Test-Path -LiteralPath $paths[$k])) { Write-Output ("UNKNOWN: no file at " + $paths[$k]); exit 1 }
}

$asm = @{}
foreach ($k in $paths.Keys) {
  $asm[$k] = [System.Reflection.Assembly]::LoadFrom($paths[$k])
  $fi = Get-Item -LiteralPath $paths[$k]
  Write-Output ("ASSEMBLY  " + $asm[$k].GetName().Name + " " + $asm[$k].GetName().Version + "   file " + $fi.VersionInfo.FileVersion + ", " + $fi.Length + " bytes")
}
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

function TypeName($t) {
  if ($null -eq $t) { return "null" }
  if ($t.IsByRef) { return (TypeName $t.GetElementType()) + "&" }
  if ($t.IsArray) { return (TypeName $t.GetElementType()) + "[]" }
  if ($t.IsGenericType) {
    $a = @(); foreach ($g in $t.GetGenericArguments()) { $a += (TypeName $g) }
    $n = $t.Name; $tick = $n.IndexOf('`'); if ($tick -ge 0) { $n = $n.Substring(0, $tick) }
    return $t.Namespace + "." + $n + "<" + ($a -join ", ") + ">"
  }
  if ($t.IsGenericParameter) { return $t.Name }
  return $t.FullName
}
function ParamText($m) {
  $ps = @(); foreach ($q in $m.GetParameters()) { $o = ""; if ($q.IsOptional) { $o = "optional " }; $ps += ($o + (TypeName $q.ParameterType) + " " + $q.Name) }
  return ($ps -join ", ")
}
function MethodText($m) {
  $st = ""; if ($m.IsStatic) { $st = "static " }
  if ($m -is [System.Reflection.ConstructorInfo]) { return ("public " + $m.DeclaringType.Name + "(" + (ParamText $m) + ")") }
  return ("public " + $st + (TypeName $m.ReturnType) + " " + $m.Name + "(" + (ParamText $m) + ")   declared on " + $m.DeclaringType.FullName)
}
function PropText($p) {
  $idx = @($p.GetIndexParameters())
  $ix = ""; if ($idx.Count -gt 0) { $a = @(); foreach ($q in $idx) { $a += ((TypeName $q.ParameterType) + " " + $q.Name) }; $ix = "[" + ($a -join ", ") + "]" }
  $acc = @(); if ($p.CanRead -and $null -ne $p.GetGetMethod()) { $acc += "get" }; if ($p.CanWrite -and $null -ne $p.GetSetMethod()) { $acc += "set" }
  $st = ""; $g = $p.GetGetMethod(); if ($null -ne $g -and $g.IsStatic) { $st = "static " }
  return ("public " + $st + (TypeName $p.PropertyType) + " " + $p.Name + $ix + " { " + ($acc -join "; ") + " }   declared on " + $p.DeclaringType.FullName)
}
# An interface in a COM interop assembly can inherit members, so every interface it
# implements is read too. A class is read with its inherited public members.
function Surface($t) {
  $all = @($t)
  if ($t.IsInterface) { $all += @($t.GetInterfaces()) }
  return $all
}

$found = 0; $missing = 0
function Check($e) {
  $t = $asm[$e.Asm].GetType($e.Type)
  $label = ("[" + $e.Where + "] " + $e.Type + " " + $e.Kind + " " + $e.Name)
  if ($null -eq $t) { Write-Output ("  NO MATCH  " + $label + "   the type is not in " + $e.Asm); $script:missing++; return }
  $flags = [System.Reflection.BindingFlags]"Public,Instance,Static,FlattenHierarchy"
  switch ($e.Kind) {
    "ctor" {
      $hit = $null
      foreach ($c in $t.GetConstructors()) { $pn = @($c.GetParameters() | ForEach-Object { TypeName $_.ParameterType }); if (($pn -join ",") -eq ($e.Params -join ",")) { $hit = $c } }
      if ($null -ne $hit) { Write-Output ("  FOUND     " + $label + "   " + (MethodText $hit)); $script:found++ }
      else { Write-Output ("  NO MATCH  " + $label + "(" + ($e.Params -join ", ") + ")"); foreach ($c in $t.GetConstructors()) { Write-Output ("              there: " + (MethodText $c)) }; $script:missing++ }
    }
    "method" {
      $named = @(); foreach ($s in (Surface $t)) { $named += @($s.GetMethods($flags) | Where-Object { $_.Name -eq $e.Name }) }
      $hit = $null
      foreach ($m in $named) {
        $pn = @($m.GetParameters() | ForEach-Object { TypeName $_.ParameterType })
        if ($null -ne $e.Params) { if (($pn -join ",") -eq ($e.Params -join ",")) { $hit = $m } }
        elseif ($pn.Count -ge $e.Arity -and (@($m.GetParameters() | Where-Object { -not $_.IsOptional }).Count -le $e.Arity)) { $hit = $m }
      }
      $want = ""; if ($null -ne $e.Params) { $want = "(" + ($e.Params -join ", ") + ")" } else { $want = "(" + $e.Arity + " arguments)" }
      if ($null -ne $hit) {
        $ret = ""; if ($e.Returns -and (TypeName $hit.ReturnType) -ne $e.Returns) { $ret = "   RETURN TYPE DIFFERS, the add-in expects " + $e.Returns }
        Write-Output ("  FOUND     " + $label + $want + "   " + (MethodText $hit) + $ret); $script:found++
      } else { Write-Output ("  NO MATCH  " + $label + $want); foreach ($m in $named) { Write-Output ("              there: " + (MethodText $m)) }; $script:missing++ }
    }
    "property" {
      $hit = $null
      foreach ($s in (Surface $t)) { foreach ($p in $s.GetProperties($flags)) { if ($p.Name -eq $e.Name -and $null -eq $hit) { $hit = $p } } }
      if ($null -eq $hit) { Write-Output ("  NO MATCH  " + $label + "   no property of that name"); $script:missing++; return }
      $ok = $true; $why = @()
      if ($e.Need -match "get" -and -not ($hit.CanRead -and $null -ne $hit.GetGetMethod())) { $ok = $false; $why += "no public getter" }
      if ($e.Need -match "set" -and -not ($hit.CanWrite -and $null -ne $hit.GetSetMethod())) { $ok = $false; $why += "no public setter" }
      if ($e.Returns -and (TypeName $hit.PropertyType) -ne $e.Returns) { $why += ("type differs, the add-in expects " + $e.Returns) }
      if ($ok) { Write-Output ("  FOUND     " + $label + " (" + $e.Need + ")   " + (PropText $hit) + $(if ($why.Count -gt 0) { "   " + ($why -join ", ") } else { "" })); $script:found++ }
      else { Write-Output ("  NO MATCH  " + $label + " (" + $e.Need + ")   " + (PropText $hit) + "   " + ($why -join ", ")); $script:missing++ }
    }
    "indexer" {
      $hit = @(); foreach ($s in (Surface $t)) { $hit += @($s.GetProperties($flags) | Where-Object { @($_.GetIndexParameters()).Count -gt 0 }) }
      if ($hit.Count -gt 0) { foreach ($p in $hit) { Write-Output ("  FOUND     " + $label + "   " + (PropText $p)) }; $script:found++ }
      else {
        $dm = @($t.GetCustomAttributesData() | Where-Object { $_.AttributeType.Name -eq "DefaultMemberAttribute" })
        Write-Output ("  NO MATCH  " + $label + "   no property with an index parameter, DefaultMember attributes: " + $dm.Count); $script:missing++
      }
    }
    "field" {
      $f = $t.GetField($e.Name)
      if ($null -ne $f) { $v = ""; if ($t.IsEnum) { $v = " = " + [Convert]::ToInt64($f.GetRawConstantValue()) }; Write-Output ("  FOUND     " + $label + "   " + (TypeName $f.FieldType) + " " + $f.Name + $v); $script:found++ }
      else { Write-Output ("  NO MATCH  " + $label); $script:missing++ }
    }
  }
}

$A = "Autodesk.Navisworks.Api."
$I = "Autodesk.Navisworks.Api.Interop.ComApi."
$items = "System.Collections.Generic.IEnumerable<Autodesk.Navisworks.Api.ModelItem>"
$calls = @(
  @{ Asm="Api"; Where="Count, ResolveFolders"; Type=($A+"Document"); Kind="property"; Name="SavedViewpoints"; Need="get"; Returns=($A+"DocumentParts.DocumentSavedViewpoints") },
  @{ Asm="Api"; Where="Count, ResolveFolders"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="property"; Name="RootItem"; Need="get"; Returns=($A+"FolderItem") },
  @{ Asm="Api"; Where="FindLastAtRoot"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="property"; Name="Value"; Need="get"; Returns=($A+"SavedItemCollection") },
  @{ Asm="Api"; Where="EnsureFolders, Record"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="method"; Name="AddCopy"; Params=@(($A+"GroupItem"), ($A+"SavedItem")) },
  @{ Asm="Api"; Where="Record"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="method"; Name="Remove"; Params=@(($A+"SavedItem")); Returns="System.Boolean" },
  @{ Asm="Api"; Where="SnapshotHidden"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="method"; Name="CaptureRuntimeOverrides"; Params=@(); Returns=($A+"SavedViewpoint") },
  @{ Asm="Api"; Where="EnsureFolders"; Type=($A+"FolderItem"); Kind="ctor"; Name=".ctor"; Params=@() },
  @{ Asm="Api"; Where="EnsureFolders, FindFolder"; Type=($A+"FolderItem"); Kind="property"; Name="DisplayName"; Need="get set"; Returns="System.String" },
  @{ Asm="Api"; Where="FindFolder, FindLeaf"; Type=($A+"GroupItem"); Kind="property"; Name="Children"; Need="get"; Returns=($A+"SavedItemCollection") },
  @{ Asm="Api"; Where="FindFolder"; Type=($A+"SavedItemCollection"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Api"; Where="FindFolder"; Type=($A+"SavedItemCollection"); Kind="indexer"; Name="this[]" },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"SavedViewpoint"); Kind="property"; Name="Viewpoint"; Need="get"; Returns=($A+"Viewpoint") },
  @{ Asm="Api"; Where="ReadBack, HiddenSnapshot"; Type=($A+"SavedViewpoint"); Kind="method"; Name="GetVisibilityOverrides"; Params=@(); Returns=($A+"VisibilityOverrides") },
  @{ Asm="Api"; Where="ReadBack, WillShow"; Type=($A+"SavedViewpoint"); Kind="method"; Name="GetAppearanceOverrides"; Params=@(); Returns=($A+"AppearanceOverrides") },
  @{ Asm="Api"; Where="CountOf, HiddenSnapshot"; Type=($A+"VisibilityOverrides"); Kind="property"; Name="Hidden"; Need="get"; Returns=($A+"ModelItemCollection") },
  @{ Asm="Api"; Where="CountOf, WillShow"; Type=($A+"AppearanceOverrides"); Kind="property"; Name="MaterialOverrides"; Need="get" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"MaterialOverride"); Kind="property"; Name="Item"; Need="get"; Returns=($A+"ModelItem") },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"MaterialOverride"); Kind="property"; Name="Color"; Need="get"; Returns=($A+"Color") },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"Viewpoint"); Kind="property"; Name="Position"; Need="get"; Returns=($A+"Point3D") },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"Document"); Kind="property"; Name="Models"; Need="get"; Returns=($A+"DocumentParts.DocumentModels") },
  @{ Asm="Api"; Where="ShowOnlyModels, RestoreHiddenState"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="ResetAllHidden"; Params=@() },
  @{ Asm="Api"; Where="ShowOnlyModels, RestoreHiddenState"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="SetHidden"; Params=@($items, "System.Boolean") },
  @{ Asm="Api"; Where="RestoreHiddenState"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="IsHidden"; Params=@($items); Returns="System.Boolean" },
  @{ Asm="Api"; Where="DimAllBut"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="OverrideTemporaryTransparency"; Params=@($items, "System.Double") },
  @{ Asm="Api"; Where="DimAllBut, Undim"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="ResetTemporaryMaterials"; Params=@($items) },
  @{ Asm="Api"; Where="PaintOne"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="OverrideTemporaryColor"; Params=@($items, ($A+"Color")) },
  @{ Asm="Api"; Where="Undim, RootsOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="CreateCollectionFromRootItems"; Params=@(); Returns=($A+"ModelItemCollection") },
  @{ Asm="Api"; Where="ItemAt"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="ResolveIndexPath"; Params=@("System.Collections.Generic.IEnumerable<System.Int32>"); Returns=($A+"ModelItem") },
  @{ Asm="Api"; Where="PathOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="CreateIndexPath"; Params=@(($A+"ModelItem")); Returns="System.Collections.ObjectModel.Collection<System.Int32>" },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="indexer"; Name="this[]" },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"Model"); Kind="property"; Name="RootItem"; Need="get"; Returns=($A+"ModelItem") },
  @{ Asm="Api"; Where="ShowOnlyModels, DimAllBut, PaintOne"; Type=($A+"ModelItemCollection"); Kind="ctor"; Name=".ctor"; Params=@() },
  @{ Asm="Api"; Where="ShowOnlyModels, DimAllBut, PaintOne"; Type=($A+"ModelItemCollection"); Kind="method"; Name="Add"; Params=@(($A+"ModelItem")) },
  @{ Asm="Api"; Where="ShowOnlyModels, CountOf"; Type=($A+"ModelItemCollection"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"ModelItem"); Kind="property"; Name="HasGeometry"; Need="get"; Returns="System.Boolean" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"ModelItem"); Kind="property"; Name="Geometry"; Need="get"; Returns=($A+"ModelGeometry") },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"ModelGeometry"); Kind="property"; Name="OriginalColor"; Need="get"; Returns=($A+"Color") },
  @{ Asm="Api"; Where="PaintOne"; Type=($A+"Color"); Kind="ctor"; Name=".ctor"; Params=@("System.Double", "System.Double", "System.Double") },
  @{ Asm="ComApi"; Where="Record"; Type="Autodesk.Navisworks.Api.ComApi.ComApiBridge"; Kind="property"; Name="State"; Need="get"; Returns=($I+"InwOpState10") },
  @{ Asm="ComApi"; Where="Record"; Type="Autodesk.Navisworks.Api.ComApi.ComApiBridge"; Kind="method"; Name="ToInwOpAnonView"; Params=@(($A+"Viewpoint")); Returns=($I+"InwOpAnonView") },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpState10"); Kind="method"; Name="ObjectFactory"; Params=$null; Arity=3 },
  @{ Asm="Interop"; Where="Record, FindComFolder"; Type=($I+"InwOpState10"); Kind="method"; Name="SavedViews"; Params=@(); Returns=($I+"InwSavedViewsColl") },
  @{ Asm="Interop"; Where="Record"; Type=($I+"nwEObjectType"); Kind="field"; Name="eObjectType_nwOpView" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="name"; Need="set"; Returns="System.String" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="ApplyHideAttribs"; Need="set"; Returns="System.Boolean" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="ApplyMaterialAttribs"; Need="set"; Returns="System.Boolean" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="anonview"; Need="set"; Returns=($I+"InwOpAnonView") },
  @{ Asm="Interop"; Where="Record, FindComFolder"; Type=($I+"InwOpFolderView"); Kind="method"; Name="SavedViews"; Params=@(); Returns=($I+"InwSavedViewsColl") },
  @{ Asm="Interop"; Where="FolderNamed"; Type=($I+"InwOpFolderView"); Kind="property"; Name="name"; Need="get"; Returns="System.String" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwSavedViewsColl"); Kind="method"; Name="Add"; Params=$null; Arity=1 },
  @{ Asm="Interop"; Where="FolderNamed"; Type=($I+"InwSavedViewsColl"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Interop"; Where="FolderNamed"; Type=($I+"InwSavedViewsColl"); Kind="indexer"; Name="this[]" }
)

Write-Output "---- Every Navisworks member src\Federator.Addin\Engine\SavedViewpoints.cs calls ----"
Write-Output "  [the method in SavedViewpoints.cs that calls it] type kind name, then what the DLL holds"
foreach ($e in $calls) { Check $e }
Write-Output ""
Write-Output ("  checked " + $calls.Count + ", FOUND " + $found + ", NO MATCH " + $missing)
Write-Output ""

Write-Output "---- The COM view and its collection whole, since the add-in writes through them ----"
foreach ($n in @("InwOpView", "InwOpFolderView", "InwSavedViewsColl", "InwOpSavedView")) {
  $t = $asm["Interop"].GetType($I + $n)
  if ($null -eq $t) { Write-Output ("  UNKNOWN: no type " + $I + $n); continue }
  Write-Output ("  " + $t.FullName + "   interface " + $t.IsInterface + ", inherits " + ((@($t.GetInterfaces()) | ForEach-Object { $_.Name }) -join ", "))
  foreach ($p in ($t.GetProperties() | Sort-Object Name)) { Write-Output ("      " + (PropText $p)) }
  foreach ($m in ($t.GetMethods() | Where-Object { -not $_.IsSpecialName } | Sort-Object Name)) { Write-Output ("      " + (MethodText $m)) }
}
Write-Output ""
