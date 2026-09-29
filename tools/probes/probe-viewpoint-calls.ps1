param(
  [string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025",
  [string]$AddinPath = ""
)
$ErrorActionPreference = "Stop"

# F105, question 1, the half probe-viewpoints.ps1 does not read.
#
# probe-viewpoints.ps1 prints the .NET viewpoint types whole. It never opens the two COM
# DLLs, and src\Federator.Addin\Engine\SavedViewpoints.cs writes every clash viewpoint
# through them, InwOpView with ApplyHideAttribs, docs\history\scan.md 5m. This probe reads
# what that file calls in three ways, REFLECTION ONLY: every assembly is loaded with
# ReflectionOnlyLoadFrom, which runs no code in it, not its load code and not the native
# DLLs a mixed assembly needs.
#
#   A. A LIST TYPED BY HAND from reading SavedViewpoints.cs, one line each, with three
#      outcomes. FOUND means a member of that name AND that shape: the parameter types, the
#      return or property type and the accessors the file uses, for every method on the
#      list, the two COM ones included. DIFFERENT SHAPE means the name is there and the
#      shape is not. NO MATCH means nothing of that name. The list checks only what is on
#      it, and a member a reading missed is not on it
#   B. 5d's and 5c's generic members with their type arguments, which probe-viewpoints.ps1
#      and probe-model-remove.ps1 print by their bare names, Collection`1
#   C. THE IL. Given the add-in built from this repo, every Navisworks member and type the
#      compiled classes of SavedViewpoints.cs reference, read off their IL, their locals
#      and their signatures, and resolved against the install. A reference resolves only
#      where the install holds a member of that exact name and signature, which is the
#      check the runtime makes when it binds the call. This list is not typed by hand. The
#      IL is decoded by il-reader.ps1, dot-sourced, and everything it could not read is
#      counted and printed at the end of the section
#
# Each DLL is tested by its one full path. The install folder is never searched. A
# reference to an Autodesk.Navisworks assembly is only ever resolved from the install folder.

$nw = $NavisworksPath
if ($AddinPath -eq "") { $AddinPath = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll" }
$paths = [ordered]@{
  Api     = (Join-Path $nw "Autodesk.Navisworks.Api.dll")
  ComApi  = (Join-Path $nw "Autodesk.Navisworks.ComApi.dll")
  Interop = (Join-Path $nw "Autodesk.Navisworks.Interop.ComApi.dll")
}
foreach ($k in $paths.Keys) {
  if (-not (Test-Path -LiteralPath $paths[$k])) { Write-Output ("UNKNOWN: no file at " + $paths[$k]); exit 1 }
}

# The IL reader, TypeName and the list of everything the reader could not read come from
# il-reader.ps1 beside this probe, the one copy probe-roamer-switches.ps1 uses as well.
$reader = Join-Path $PSScriptRoot "il-reader.ps1"
if (-not (Test-Path -LiteralPath $reader)) { Write-Output ("UNKNOWN: no IL reader at " + $reader); exit 1 }
. $reader

$resolveFailures = New-Object System.Collections.Generic.List[string]
$addinDir = ""
$roResolve = [ResolveEventHandler]{
  param($s, $e)
  $an = New-Object System.Reflection.AssemblyName($e.Name)
  foreach ($x in [AppDomain]::CurrentDomain.ReflectionOnlyGetAssemblies()) { if ($x.GetName().Name -eq $an.Name) { return $x } }
  $cand = Join-Path $nw ($an.Name + ".dll")
  if (Test-Path -LiteralPath $cand) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand) }
  if ($addinDir -ne "" -and -not $an.Name.StartsWith("Autodesk.Navisworks")) {
    $cand2 = Join-Path $addinDir ($an.Name + ".dll")
    if (Test-Path -LiteralPath $cand2) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand2) }
  }
  try { return [System.Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { $resolveFailures.Add($e.Name + ": " + $_.Exception.Message); return $null }
}
[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($roResolve)

$asm = @{}
foreach ($k in $paths.Keys) {
  $asm[$k] = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($paths[$k])
  $fi = Get-Item -LiteralPath $paths[$k]
  Write-Output ("ASSEMBLY  " + $asm[$k].GetName().Name + " " + $asm[$k].GetName().Version + "   file " + $fi.VersionInfo.FileVersion + ", " + $fi.Length + " bytes, loaded reflection only: " + $asm[$k].ReflectionOnly)
}
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ("READER    il-reader.ps1, dot-sourced, sha256 " + (Get-FileHash -LiteralPath $reader -Algorithm SHA256).Hash)
Write-Output ""
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
  $acc = @(); if ($null -ne $p.GetGetMethod()) { $acc += "get" }; if ($null -ne $p.GetSetMethod()) { $acc += "set" }
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
function BaseNames($t) { $bn = @(); $b = $t.BaseType; while ($null -ne $b) { $bn += $b.FullName; $b = $b.BaseType }; return $bn }
function Key($x) { return ($x.Module.Name + ":" + $x.MetadataToken) }

# ============================================================================== A
$found = 0; $differ = 0; $missing = 0
$onHandList = @{}
function Mark($x) {
  if ($null -eq $x) { return }
  $onHandList[(Key $x)] = $true
  if ($x -is [System.Reflection.PropertyInfo]) { foreach ($acc in @($x.GetGetMethod(), $x.GetSetMethod())) { if ($null -ne $acc) { $onHandList[(Key $acc)] = $true } } }
}
function Say($outcome, $label, $text) {
  Write-Output ("  " + $outcome.PadRight(15) + " " + $label + "   " + $text)
  if ($outcome -eq "FOUND") { $script:found++ } elseif ($outcome -eq "DIFFERENT SHAPE") { $script:differ++ } else { $script:missing++ }
}
function Check($e) {
  $t = $asm[$e.Asm].GetType($e.Type)
  $label = ("[" + $e.Where + "] " + $e.Type + " " + $e.Kind + " " + $e.Name)
  if ($null -eq $t) { Say "NO MATCH" $label ("the type is not in " + $e.Asm); return }
  $flags = [System.Reflection.BindingFlags]"Public,Instance,Static,FlattenHierarchy"
  switch ($e.Kind) {
    "ctor" {
      $all = @($t.GetConstructors())
      $hit = $null
      foreach ($c in $all) { $pn = @($c.GetParameters() | ForEach-Object { TypeName $_.ParameterType }); if (($pn -join ",") -eq ($e.Params -join ",")) { $hit = $c } }
      if ($null -ne $hit) { Mark $hit; Say "FOUND" ($label + "(" + ($e.Params -join ", ") + ")") (MethodText $hit) }
      elseif ($all.Count -gt 0) { Say "DIFFERENT SHAPE" ($label + "(" + ($e.Params -join ", ") + ")") ("constructors there: " + (($all | ForEach-Object { MethodText $_ }) -join " | ")) }
      else { Say "NO MATCH" ($label + "(" + ($e.Params -join ", ") + ")") "no public constructor" }
    }
    "method" {
      $named = @(); foreach ($s in (Surface $t)) { $named += @($s.GetMethods($flags) | Where-Object { $_.Name -eq $e.Name }) }
      $want = "(" + ($e.Params -join ", ") + ")"
      $hit = $null
      foreach ($m in $named) {
        $pn = @($m.GetParameters() | ForEach-Object { TypeName $_.ParameterType })
        $shape = (($pn -join ",") -eq ($e.Params -join ","))
        if ($shape -and $e.Returns -and (TypeName $m.ReturnType) -ne $e.Returns) { $shape = $false }
        if ($shape -and $null -eq $hit) { $hit = $m }
      }
      if ($null -ne $hit) { Mark $hit; Say "FOUND" ($label + $want) (MethodText $hit) }
      elseif ($named.Count -gt 0) {
        $ret = ""; if ($e.Returns) { $ret = ", returning " + $e.Returns }
        Say "DIFFERENT SHAPE" ($label + $want + $ret) ("members of that name there: " + (($named | ForEach-Object { MethodText $_ }) -join " | "))
      }
      else { Say "NO MATCH" ($label + $want) "no method of that name" }
    }
    "property" {
      $hit = $null
      foreach ($s in (Surface $t)) { foreach ($p in $s.GetProperties($flags)) { if ($p.Name -eq $e.Name -and @($p.GetIndexParameters()).Count -eq 0 -and $null -eq $hit) { $hit = $p } } }
      if ($null -eq $hit) { Say "NO MATCH" ($label + " (" + $e.Need + ")") "no property of that name"; return }
      $why = @()
      if ($e.Need -match "get" -and $null -eq $hit.GetGetMethod()) { $why += "no public getter" }
      if ($e.Need -match "set" -and $null -eq $hit.GetSetMethod()) { $why += "no public setter" }
      if ($e.Returns -and (TypeName $hit.PropertyType) -ne $e.Returns) { $why += ("its type is not " + $e.Returns) }
      if ($why.Count -eq 0) { Mark $hit; Say "FOUND" ($label + " (" + $e.Need + ")") (PropText $hit) }
      else { Say "DIFFERENT SHAPE" ($label + " (" + $e.Need + ")") ((PropText $hit) + "   " + ($why -join ", ")) }
    }
    "indexer" {
      $hit = @(); foreach ($s in (Surface $t)) { $hit += @($s.GetProperties($flags) | Where-Object { @($_.GetIndexParameters()).Count -gt 0 }) }
      $good = @($hit | Where-Object { (@($_.GetIndexParameters() | ForEach-Object { TypeName $_.ParameterType }) -join ",") -eq ($e.Params -join ",") -and $null -ne $_.GetGetMethod() -and (-not $e.Returns -or (TypeName $_.PropertyType) -eq $e.Returns) })
      if ($good.Count -gt 0) { Mark $good[0]; Say "FOUND" ($label + "[" + ($e.Params -join ", ") + "] (get)") (($good | ForEach-Object { PropText $_ }) -join " | ") }
      elseif ($hit.Count -gt 0) { Say "DIFFERENT SHAPE" ($label + "[" + ($e.Params -join ", ") + "] (get)") ("indexers there: " + (($hit | ForEach-Object { PropText $_ }) -join " | ")) }
      else { Say "NO MATCH" ($label + "[" + ($e.Params -join ", ") + "]") "no property with an index parameter" }
    }
    "field" {
      $f = $t.GetField($e.Name)
      if ($null -ne $f) {
        Mark $f
        $v = ""; if ($t.IsEnum) { try { $v = " = " + [Convert]::ToInt64($f.GetRawConstantValue()) } catch { $v = " = UNKNOWN, the value could not be read: " + $_.Exception.Message } }
        Say "FOUND" $label ((TypeName $f.FieldType) + " " + $f.Name + $v)
      }
      else { Say "NO MATCH" $label "no public field of that name" }
    }
    "disposable" {
      $ifs = @($t.GetInterfaces() | ForEach-Object { $_.FullName })
      if ($ifs -contains "System.IDisposable") { Say "FOUND" ($label + " implements System.IDisposable") ("interfaces: " + ($ifs -join ", ")) }
      else { Say "DIFFERENT SHAPE" ($label + " implements System.IDisposable") ("it does not, interfaces: " + ($ifs -join ", ")) }
    }
    "assignable" {
      $bases = @(BaseNames $t) + @($t.GetInterfaces() | ForEach-Object { TypeName $_ })
      if ($bases -contains $e.To) { Say "FOUND" ($label + " converts to " + $e.To) ("bases and interfaces: " + ($bases -join ", ")) }
      else { Say "DIFFERENT SHAPE" ($label + " converts to " + $e.To) ("it does not, bases and interfaces: " + ($bases -join ", ")) }
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
  @{ Asm="Api"; Where="EnsureFolders, Record"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="method"; Name="AddCopy"; Params=@(($A+"GroupItem"), ($A+"SavedItem")); Returns="System.Void" },
  @{ Asm="Api"; Where="Record"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="method"; Name="Remove"; Params=@(($A+"SavedItem")); Returns="System.Boolean" },
  @{ Asm="Api"; Where="SnapshotHidden"; Type=($A+"DocumentParts.DocumentSavedViewpoints"); Kind="method"; Name="CaptureRuntimeOverrides"; Params=@(); Returns=($A+"SavedViewpoint") },
  @{ Asm="Api"; Where="EnsureFolders"; Type=($A+"FolderItem"); Kind="ctor"; Name=".ctor"; Params=@() },
  @{ Asm="Api"; Where="EnsureFolders, FindFolder"; Type=($A+"FolderItem"); Kind="property"; Name="DisplayName"; Need="get set"; Returns="System.String" },
  @{ Asm="Api"; Where="FindFolder, FindLeaf"; Type=($A+"GroupItem"); Kind="property"; Name="Children"; Need="get"; Returns=($A+"SavedItemCollection") },
  @{ Asm="Api"; Where="FindFolder"; Type=($A+"SavedItemCollection"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Api"; Where="FindFolder"; Type=($A+"SavedItemCollection"); Kind="indexer"; Name="this[]"; Params=@("System.Int32"); Returns=($A+"SavedItem") },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"SavedViewpoint"); Kind="property"; Name="Viewpoint"; Need="get"; Returns=($A+"Viewpoint") },
  @{ Asm="Api"; Where="ReadBack, HiddenSnapshot"; Type=($A+"SavedViewpoint"); Kind="method"; Name="GetVisibilityOverrides"; Params=@(); Returns=($A+"VisibilityOverrides") },
  @{ Asm="Api"; Where="ReadBack, WillShow"; Type=($A+"SavedViewpoint"); Kind="method"; Name="GetAppearanceOverrides"; Params=@(); Returns=($A+"AppearanceOverrides") },
  @{ Asm="Api"; Where="CountOf, HiddenSnapshot"; Type=($A+"VisibilityOverrides"); Kind="property"; Name="Hidden"; Need="get"; Returns=($A+"ModelItemCollection") },
  @{ Asm="Api"; Where="CountOf, WillShow"; Type=($A+"AppearanceOverrides"); Kind="property"; Name="MaterialOverrides"; Need="get"; Returns="System.Collections.ObjectModel.Collection<Autodesk.Navisworks.Api.MaterialOverride>" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"MaterialOverride"); Kind="property"; Name="Item"; Need="get"; Returns=($A+"ModelItem") },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"MaterialOverride"); Kind="property"; Name="Color"; Need="get"; Returns=($A+"Color") },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"Viewpoint"); Kind="property"; Name="Position"; Need="get"; Returns=($A+"Point3D") },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"Point3D"); Kind="property"; Name="X"; Need="get"; Returns="System.Double" },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"Point3D"); Kind="property"; Name="Y"; Need="get"; Returns="System.Double" },
  @{ Asm="Api"; Where="ReadBack"; Type=($A+"Point3D"); Kind="property"; Name="Z"; Need="get"; Returns="System.Double" },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"Document"); Kind="property"; Name="Models"; Need="get"; Returns=($A+"DocumentParts.DocumentModels") },
  @{ Asm="Api"; Where="ShowOnlyModels, RestoreHiddenState"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="ResetAllHidden"; Params=@(); Returns="System.Void" },
  @{ Asm="Api"; Where="ShowOnlyModels, RestoreHiddenState"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="SetHidden"; Params=@($items, "System.Boolean"); Returns="System.Void" },
  @{ Asm="Api"; Where="RestoreHiddenState"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="IsHidden"; Params=@($items); Returns="System.Boolean" },
  @{ Asm="Api"; Where="DimAllBut"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="OverrideTemporaryTransparency"; Params=@($items, "System.Double"); Returns="System.Void" },
  @{ Asm="Api"; Where="DimAllBut, Undim"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="ResetTemporaryMaterials"; Params=@($items); Returns="System.Void" },
  @{ Asm="Api"; Where="PaintOne"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="OverrideTemporaryColor"; Params=@($items, ($A+"Color")); Returns="System.Void" },
  @{ Asm="Api"; Where="Undim, RootsOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="CreateCollectionFromRootItems"; Params=@(); Returns=($A+"ModelItemCollection") },
  @{ Asm="Api"; Where="ItemAt"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="ResolveIndexPath"; Params=@("System.Collections.Generic.IEnumerable<System.Int32>"); Returns=($A+"ModelItem") },
  @{ Asm="Api"; Where="PathOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="method"; Name="CreateIndexPath"; Params=@(($A+"ModelItem")); Returns="System.Collections.ObjectModel.Collection<System.Int32>" },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"DocumentParts.DocumentModels"); Kind="indexer"; Name="this[]"; Params=@("System.Int32"); Returns=($A+"Model") },
  @{ Asm="Api"; Where="ShowOnlyModels, RootsOf"; Type=($A+"Model"); Kind="property"; Name="RootItem"; Need="get"; Returns=($A+"ModelItem") },
  @{ Asm="Api"; Where="ShowOnlyModels, DimAllBut, PaintOne"; Type=($A+"ModelItemCollection"); Kind="ctor"; Name=".ctor"; Params=@() },
  @{ Asm="Api"; Where="ShowOnlyModels, DimAllBut, PaintOne"; Type=($A+"ModelItemCollection"); Kind="method"; Name="Add"; Params=@(($A+"ModelItem")); Returns="System.Void" },
  @{ Asm="Api"; Where="ShowOnlyModels, CountOf"; Type=($A+"ModelItemCollection"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"ModelItem"); Kind="property"; Name="HasGeometry"; Need="get"; Returns="System.Boolean" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"ModelItem"); Kind="property"; Name="Geometry"; Need="get"; Returns=($A+"ModelGeometry") },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"ModelGeometry"); Kind="property"; Name="OriginalColor"; Need="get"; Returns=($A+"Color") },
  @{ Asm="Api"; Where="PaintOne"; Type=($A+"Color"); Kind="ctor"; Name=".ctor"; Params=@("System.Double", "System.Double", "System.Double") },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"Color"); Kind="property"; Name="R"; Need="get"; Returns="System.Double" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"Color"); Kind="property"; Name="G"; Need="get"; Returns="System.Double" },
  @{ Asm="Api"; Where="WillShow"; Type=($A+"Color"); Kind="property"; Name="B"; Need="get"; Returns="System.Double" },
  @{ Asm="Api"; Where="ResolveFolders"; Type=($A+"GroupItem"); Kind="method"; Name="Dispose"; Params=@(); Returns="System.Void" },
  @{ Asm="Api"; Where="FindFolder, FindLastAtRoot, FindLeafItem"; Type=($A+"SavedItem"); Kind="method"; Name="Dispose"; Params=@(); Returns="System.Void" },
  @{ Asm="Api"; Where="HiddenSnapshot.Dispose"; Type=($A+"SavedViewpoint"); Kind="method"; Name="Dispose"; Params=@(); Returns="System.Void" },
  @{ Asm="Api"; Where="HiddenSnapshot.Dispose"; Type=($A+"ModelItemCollection"); Kind="method"; Name="Dispose"; Params=@(); Returns="System.Void" },
  @{ Asm="Api"; Where="using, Count, Exists, EnsureFolders, ReadBack"; Type=($A+"GroupItem"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, EnsureFolders"; Type=($A+"FolderItem"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, FindLeaf, CountUnder"; Type=($A+"SavedItem"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, Record, ReadBack"; Type=($A+"SavedViewpoint"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, ReadBack"; Type=($A+"Viewpoint"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, ShowOnlyModels, CountOf, DimAllBut, PaintOne, Undim"; Type=($A+"ModelItemCollection"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, ShowOnlyModels, RootsOf"; Type=($A+"Model"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, ShowOnlyModels, RootsOf, WillShow"; Type=($A+"ModelItem"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="using, WillShow"; Type=($A+"ModelGeometry"); Kind="disposable"; Name="IDisposable" },
  @{ Asm="Api"; Where="Count, ResolveFolders, the RootItem held as a GroupItem"; Type=($A+"FolderItem"); Kind="assignable"; Name="base"; To=($A+"GroupItem") },
  @{ Asm="Api"; Where="EnsureFolders, AddCopy(parent, folder)"; Type=($A+"FolderItem"); Kind="assignable"; Name="base"; To=($A+"SavedItem") },
  @{ Asm="Api"; Where="FindFolder, child as GroupItem"; Type=($A+"GroupItem"); Kind="assignable"; Name="base"; To=($A+"SavedItem") },
  @{ Asm="Api"; Where="Record, FindLeafItem, item as SavedViewpoint"; Type=($A+"SavedViewpoint"); Kind="assignable"; Name="base"; To=($A+"SavedItem") },
  @{ Asm="Api"; Where="ShowOnlyModels, SetHidden(hide, true)"; Type=($A+"ModelItemCollection"); Kind="assignable"; Name="interface"; To=$items },
  @{ Asm="Api"; Where="Exists, EnsureFolders, Record, ReadBack, the null checks"; Type=($A+"NativeHandle"); Kind="method"; Name="op_Equality"; Params=@(($A+"NativeHandle"), ($A+"NativeHandle")); Returns="System.Boolean" },
  @{ Asm="Api"; Where="Exists, the null checks"; Type=($A+"NativeHandle"); Kind="method"; Name="op_Inequality"; Params=@(($A+"NativeHandle"), ($A+"NativeHandle")); Returns="System.Boolean" },
  @{ Asm="ComApi"; Where="Record"; Type="Autodesk.Navisworks.Api.ComApi.ComApiBridge"; Kind="property"; Name="State"; Need="get"; Returns=($I+"InwOpState10") },
  @{ Asm="ComApi"; Where="Record"; Type="Autodesk.Navisworks.Api.ComApi.ComApiBridge"; Kind="method"; Name="ToInwOpAnonView"; Params=@(($A+"Viewpoint")); Returns=($I+"InwOpAnonView") },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpState10"); Kind="method"; Name="ObjectFactory"; Params=@(($I+"nwEObjectType"), "System.Object", "System.Object"); Returns="System.Object" },
  @{ Asm="Interop"; Where="Record, FindComFolder"; Type=($I+"InwOpState10"); Kind="method"; Name="SavedViews"; Params=@(); Returns=($I+"InwSavedViewsColl") },
  @{ Asm="Interop"; Where="Record"; Type=($I+"nwEObjectType"); Kind="field"; Name="eObjectType_nwOpView" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="name"; Need="set"; Returns="System.String" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="ApplyHideAttribs"; Need="set"; Returns="System.Boolean" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="ApplyMaterialAttribs"; Need="set"; Returns="System.Boolean" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwOpView"); Kind="property"; Name="anonview"; Need="set"; Returns=($I+"InwOpAnonView") },
  @{ Asm="Interop"; Where="Record, FindComFolder"; Type=($I+"InwOpFolderView"); Kind="method"; Name="SavedViews"; Params=@(); Returns=($I+"InwSavedViewsColl") },
  @{ Asm="Interop"; Where="FolderNamed"; Type=($I+"InwOpFolderView"); Kind="property"; Name="name"; Need="get"; Returns="System.String" },
  @{ Asm="Interop"; Where="Record"; Type=($I+"InwSavedViewsColl"); Kind="method"; Name="Add"; Params=@("System.Object"); Returns="System.Void" },
  @{ Asm="Interop"; Where="FolderNamed"; Type=($I+"InwSavedViewsColl"); Kind="property"; Name="Count"; Need="get"; Returns="System.Int32" },
  @{ Asm="Interop"; Where="FolderNamed"; Type=($I+"InwSavedViewsColl"); Kind="indexer"; Name="this[]"; Params=@("System.Object"); Returns="System.Object" }
)

Write-Output "---- A. THE LIST TYPED BY HAND, from reading src\Federator.Addin\Engine\SavedViewpoints.cs ----"
Write-Output "  [the method or use in SavedViewpoints.cs] type kind name, then what the DLL holds"
Write-Output "  FOUND is that name AND that shape. DIFFERENT SHAPE is the name without the shape. NO MATCH is neither."
foreach ($e in $calls) { Check $e }
Write-Output ""
Write-Output ("  on the list " + $calls.Count + ", FOUND " + $found + ", DIFFERENT SHAPE " + $differ + ", NO MATCH " + $missing)
Write-Output "  This list checks only what is on it. Section C reads what the compiled file references."
Write-Output ""

# ============================================================================== B
Write-Output "---- B. 5d's and 5c's generic members, with their type arguments ----"
$gen = @(
  @(($A+"DocumentParts.DocumentSavedViewpoints"), "CreateCopy"),
  @(($A+"DocumentParts.DocumentSavedViewpoints"), "CopyFrom"),
  @(($A+"DocumentParts.DocumentSelectionSets"), "CreateCopy"),
  @(($A+"DocumentParts.DocumentSelectionSets"), "CopyFrom"),
  @(($A+"Document"), "AppendFiles"),
  @(($A+"Document"), "TryAppendFiles")
)
foreach ($g in $gen) {
  $t = $asm["Api"].GetType($g[0])
  if ($null -eq $t) { Write-Output ("  UNKNOWN: no type " + $g[0]); continue }
  $ms = @($t.GetMethods("Public,Instance,Static,DeclaredOnly") | Where-Object { $_.Name -eq $g[1] })
  if ($ms.Count -eq 0) { Write-Output ("  NO MATCH  " + $g[0] + "." + $g[1]) }
  foreach ($m in $ms) { Write-Output ("  " + $g[0] + "   " + (MethodText $m)) }
}
Write-Output ""

# ============================================================================== C
Write-Output "---- C. THE IL: every Navisworks reference the compiled classes of SavedViewpoints.cs make ----"
function IsNw($t) {
  if ($null -eq $t) { return $false }
  if ($t.IsByRef -or $t.IsArray -or $t.IsPointer) { return (IsNw $t.GetElementType()) }
  if ($t.IsGenericParameter) { return $false }
  if ($t.Assembly.Location.StartsWith($nw, [StringComparison]::OrdinalIgnoreCase)) { return $true }
  if ($t.IsGenericType) { foreach ($g in $t.GetGenericArguments()) { if (IsNw $g) { return $true } } }
  return $false
}
function IsNwHome($t) { return ($null -ne $t -and -not $t.IsGenericParameter -and $t.Assembly.Location.StartsWith($nw, [StringComparison]::OrdinalIgnoreCase)) }
function RefText($x) {
  if ($x -is [System.Type]) { return ("type     " + (TypeName $x)) }
  if ($x -is [System.Reflection.FieldInfo]) { return ("field    " + (TypeName $x.FieldType) + " " + (TypeName $x.DeclaringType) + "::" + $x.Name) }
  if ($x -is [System.Reflection.ConstructorInfo]) { return ("ctor     " + (TypeName $x.DeclaringType) + "::.ctor(" + (ParamText $x) + ")") }
  if ($x -is [System.Reflection.MethodInfo]) { $st = ""; if ($x.IsStatic) { $st = "static " }; return ("method   " + $st + (TypeName $x.ReturnType) + " " + (TypeName $x.DeclaringType) + "::" + $x.Name + "(" + (ParamText $x) + ")") }
  return [string]$x
}

if (-not (Test-Path -LiteralPath $AddinPath)) {
  Write-Output ("  UNKNOWN: no built add-in at " + $AddinPath + ", so section C did not run. Pass -AddinPath with a Release build of this repo")
} else {
  $addinDir = Split-Path $AddinPath -Parent
  $ai = Get-Item -LiteralPath $AddinPath
  $cut = $AddinPath.IndexOf("\src\")
  $shown = (Split-Path $AddinPath -Leaf); if ($cut -ge 0) { $shown = "..." + $AddinPath.Substring($cut) }
  Write-Output ("  the add-in read   " + $shown)
  Write-Output ("                    " + $ai.Length + " bytes, written " + $ai.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + ", sha256 " + (Get-FileHash -LiteralPath $AddinPath -Algorithm SHA256).Hash)
  Write-Output ("                    its stamp, the product version: " + $ai.VersionInfo.ProductVersion)
  $aa = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($AddinPath)
  Write-Output ("                    " + $aa.FullName + ", loaded reflection only: " + $aa.ReflectionOnly)
  $names = @("Federator.Addin.Engine.SavedViewpoints", "Federator.Addin.Engine.ViewpointReadBack", "Federator.Addin.Engine.HiddenSnapshot")
  $types = @()
  foreach ($n in $names) {
    $t = $aa.GetType($n)
    if ($null -eq $t) { Write-Output ("  UNKNOWN: no type " + $n + " in the add-in"); continue }
    $types += $t
    foreach ($nt in $t.GetNestedTypes("Public,NonPublic")) { $types += $nt }
  }
  Write-Output ("  classes read: " + (($types | ForEach-Object { $_.FullName }) -join ", "))
  $flagsAll = [System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static,DeclaredOnly"
  $refs = [ordered]@{}
  $methodsRead = 0; $tokensRead = 0
  $tokenKinds = @("InlineMethod", "InlineField", "InlineType", "InlineTok")
  function AddRef($x, $place) {
    $k = RefText $x
    if (-not $refs.Contains($k)) { $refs[$k] = [pscustomobject]@{ Member = $x; Places = New-Object System.Collections.Generic.List[string] } }
    if (-not $refs[$k].Places.Contains($place)) { $refs[$k].Places.Add($place) }
  }
  foreach ($t in $types) {
    foreach ($f in $t.GetFields($flagsAll)) {
      try { if (IsNw $f.FieldType) { AddRef $f.FieldType ($t.Name + "." + $f.Name + " field") } } catch { $null = IlFail "field" $t -1 ($f.Name + ", " + (IlReason $_.Exception)) }
    }
    $members = @($t.GetConstructors($flagsAll)) + @($t.GetMethods($flagsAll))
    foreach ($m in $members) {
      $place = $t.Name + "." + $m.Name
      try {
        if ($m -is [System.Reflection.MethodInfo] -and (IsNw $m.ReturnType)) { AddRef $m.ReturnType ($place + " return") }
        foreach ($q in $m.GetParameters()) { if (IsNw $q.ParameterType) { AddRef $q.ParameterType ($place + " parameter") } }
      } catch { $null = IlFail "signature" $m -1 (IlReason $_.Exception) }
      # The body through il-reader.ps1. A body it cannot read, an opcode it does not know
      # and a token that does not resolve are entries with Failed set, and each is kept in
      # its list, which the end of this section prints.
      $ins = IlRead $m
      if ($ins.Count -gt 0 -and $ins[0].Offset -lt 0) { continue }
      $methodsRead++
      try { foreach ($lv in $m.GetMethodBody().LocalVariables) { if (IsNw $lv.LocalType) { AddRef $lv.LocalType ($place + " local") } } } catch { $null = IlFail "locals" $m -1 (IlReason $_.Exception) }
      foreach ($x in $ins) {
        if ($tokenKinds -notcontains $x.OperandType) { continue }
        $tokensRead++
        if ($x.Failed) { continue }
        $y = $x.Member
        $nwRef = $false
        if ($y -is [System.Type]) { $nwRef = (IsNw $y) } else { $nwRef = (IsNw $y.DeclaringType) }
        if ($nwRef) { AddRef $y ($place + " IL_" + $x.Offset.ToString("X4")) }
      }
    }
  }
  $memberRefs = @($refs.Keys | Where-Object { -not ($refs[$_].Member -is [System.Type]) })
  $typeRefs = @($refs.Keys | Where-Object { $refs[$_].Member -is [System.Type] })
  Write-Output ("  method bodies read " + $methodsRead + ", member and type tokens read " + $tokensRead)
  Write-Output ""
  Write-Output "  Every Navisworks MEMBER referenced, resolved against the install. A member declared on a"
  Write-Output "  framework generic over a Navisworks type is marked framework. The last field says whether"
  Write-Output "  list A has the same member."
  $nOn = 0; $nOff = 0; $nFw = 0
  foreach ($k in $memberRefs) {
    $r = $refs[$k]; $x = $r.Member
    $origin = "install"; if (-not (IsNwHome $x.DeclaringType)) { $origin = "framework"; $nFw++ }
    $on = "not asked"
    if ($origin -eq "install") { if ($onHandList.ContainsKey((Key $x))) { $on = "yes"; $nOn++ } else { $on = "NO"; $nOff++ } }
    Write-Output ("  RESOLVED  " + $origin.PadRight(9) + " " + $k + "   at " + ($r.Places -join ", ") + "   on list A: " + $on)
  }
  Write-Output ""
  Write-Output "  Every Navisworks TYPE referenced, in an instruction, a local, a field or a signature:"
  foreach ($k in $typeRefs) { Write-Output ("  RESOLVED  " + $k + "   at " + ($refs[$k].Places -join ", ")) }
  Write-Output ""
  Write-Output "  Everything section C could not read, kept by il-reader.ps1, each once, with what the runtime said:"
  IlFailureLines "    "
  Write-Output ("  count " + $IlFailures.Count)
  Write-Output ""
  Write-Output ("  Navisworks members referenced " + $memberRefs.Count + ", declared in an install assembly " + ($memberRefs.Count - $nFw) + ", of those on list A " + $nOn + " and NOT on list A " + $nOff + ", declared on a framework generic " + $nFw)
  Write-Output ("  Navisworks types referenced " + $typeRefs.Count + ", reads that failed " + $IlFailures.Count)
  Write-Output "  Where each assembly the check touched was loaded from, reflection only:"
  foreach ($x in ([AppDomain]::CurrentDomain.ReflectionOnlyGetAssemblies() | Sort-Object FullName)) {
    $where = "elsewhere"
    if ($x.Location.StartsWith($nw, [StringComparison]::OrdinalIgnoreCase)) { $where = "the install folder" }
    elseif ($x.Location.StartsWith($addinDir, [StringComparison]::OrdinalIgnoreCase)) { $where = "the add-in's own folder" }
    elseif ($x.GlobalAssemblyCache) { $where = "the global assembly cache" }
    Write-Output ("    " + $x.GetName().Name + " " + $x.GetName().Version + "   " + $where)
  }
}
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
Write-Output ("  references that could not be loaded for reflection: " + $(if ($resolveFailures.Count -eq 0) { "none" } else { ($resolveFailures | Sort-Object -Unique) -join " | " }))
