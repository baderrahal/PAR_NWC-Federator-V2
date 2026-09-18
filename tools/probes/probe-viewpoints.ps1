param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F52. How a saved viewpoint is made, named, foldered and disposed.
#
# This repo has NEVER touched this collection. docs\history\scan.md names Viewpoint,
# DocumentCurrentViewpoint, View.CreateViewpointCopy and ClashResult.HasSavedViewpoint,
# and section 4k says nothing here writes a viewpoint into the NWF. So every one of the
# four things F52 needs is UNKNOWN and nothing about it may be assumed.
#
# DocumentSelectionSets is printed beside it on purpose. The sets tree is the closest
# thing this tool already builds, and SetBuilder's folder walk is what F52 would copy.
# If the two collections have the same shape, that is a MEASUREMENT and the code follows
# it. If they differ, the difference is what this probe exists to find.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

$asm = [System.Reflection.Assembly]::LoadFrom($api)
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

function Show-Type($name) {
  $ty = $asm.GetType($name)
  if ($null -eq $ty) { Write-Output ("  UNKNOWN: no type named " + $name); return }

  Write-Output ($name)
  Write-Output ("    base type: " + $ty.BaseType.FullName)
  $ifaces = ($ty.GetInterfaces() | ForEach-Object { $_.Name }) -join ", "
  Write-Output ("    implements: " + $(if ($ifaces) { $ifaces } else { "nothing" }))
  Write-Output ("    IDisposable: " + [bool]([System.IDisposable].IsAssignableFrom($ty)))

  foreach ($c in $ty.GetConstructors("Public,Instance,DeclaredOnly")) {
    $ps = @()
    foreach ($q in $c.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
    Write-Output ("    public " + $ty.Name + "(" + ($ps -join ", ") + ")")
  }
  foreach ($p in ($ty.GetProperties("Public,Instance,Static,DeclaredOnly") | Sort-Object Name)) {
    Write-Output ("    public " + $p.PropertyType.Name + " " + $p.Name + "  canread=" + $p.CanRead + " canwrite=" + $p.CanWrite)
  }
  foreach ($m in ($ty.GetMethods("Public,Instance,Static,DeclaredOnly") | Sort-Object Name)) {
    if ($m.IsSpecialName) { continue }
    $ps = @()
    foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
    Write-Output ("    public " + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
  }
  Write-Output ""
}

Write-Output "---- THE QUESTION: how a document reaches its saved viewpoints ----"
$doc = $asm.GetType("Autodesk.Navisworks.Api.Document")
$any = $false
foreach ($p in ($doc.GetProperties("Public,Instance,DeclaredOnly") | Sort-Object Name)) {
  if ($p.Name.ToLowerInvariant().Contains("viewpoint")) {
    Write-Output ("  " + $p.PropertyType.FullName + " Document." + $p.Name + "  canread=" + $p.CanRead + " canwrite=" + $p.CanWrite)
    $any = $true
  }
}
if (-not $any) { Write-Output "  UNKNOWN: no property on Document whose name holds Viewpoint" }
Write-Output ""

Write-Output "---- The viewpoint collection, every member ----"
Show-Type "Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints"

Write-Output "---- The saved viewpoint itself ----"
Show-Type "Autodesk.Navisworks.Api.SavedViewpoint"

Write-Output "---- The viewpoint it holds ----"
Show-Type "Autodesk.Navisworks.Api.Viewpoint"

Write-Output "---- The folder and the item, shared with the sets tree ----"
Show-Type "Autodesk.Navisworks.Api.FolderItem"
Show-Type "Autodesk.Navisworks.Api.GroupItem"
Show-Type "Autodesk.Navisworks.Api.SavedItem"

Write-Output "---- The sets collection beside it, so the two shapes can be compared ----"
Show-Type "Autodesk.Navisworks.Api.DocumentParts.DocumentSelectionSets"

Write-Output "---- Anything anywhere whose name holds SavedViewpoint ----"
foreach ($ty in $asm.GetTypes()) {
  if ($ty.FullName.Contains("SavedViewpoint")) { Write-Output ("  type " + $ty.FullName) }
}
Write-Output ""

Write-Output "---- How a viewpoint could be made to show one discipline and hide the rest ----"
# F52 needs to hide model items. Whatever does that is either on the viewpoint, on the
# document's state, or on a Search. Print every public member anywhere whose name holds
# hide, visib or override, so the answer is read rather than guessed.
$words = @("hide","hidden","visib","override")
foreach ($ty in $asm.GetTypes()) {
  foreach ($m in $ty.GetMembers("Public,Instance,Static,DeclaredOnly")) {
    $low = $m.Name.ToLowerInvariant()
    foreach ($w in $words) {
      if ($low.Contains($w)) { Write-Output ("  " + $ty.FullName + "." + $m.Name + "  (" + $m.MemberType + ")"); break }
    }
  }
}
Write-Output ""
Write-Output "Paste all of the above into docs\history\scan.md under the F52 section, with today's date."
