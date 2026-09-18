param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F50. Can one model be taken out of an open document without clearing the whole thing.
#
# The NWF is the record. A CHANGED group clears the document and appends again, then puts
# the sets and the tests back and fails loudly if either does not come back. Every new
# thing the NWF carries makes that dance longer and riskier. If a model can be removed on
# its own, the rebuild becomes append what is missing and remove what is gone, nothing
# needs restoring and nothing can be lost.
#
# docs\history\scan.md records Document.Models as returning a DocumentModels and records
# exactly two things about that type, Count and SetModelUnitsAndTransform. Nothing named
# Remove, Delete or Detach against a model appears anywhere in that file. So this is
# UNKNOWN and this probe is what answers it.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

$asm = [System.Reflection.Assembly]::LoadFrom($api)
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

function Show-Member($m) {
  $vis = "private"
  if ($m.IsPublic) { $vis = "public" } elseif ($m.IsFamily) { $vis = "protected" } elseif ($m.IsAssembly) { $vis = "internal" }
  $ps = @()
  foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
  Write-Output ("  " + $vis + " " + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
}

Write-Output "---- Document.Models, what it actually returns ----"
$doc = $asm.GetType("Autodesk.Navisworks.Api.Document")
$models = $doc.GetProperty("Models")
if ($null -eq $models) { Write-Output "  UNKNOWN: no Models property on Document" } else {
  Write-Output ("  " + $models.PropertyType.FullName + " Models  canread=" + $models.CanRead + " canwrite=" + $models.CanWrite)
}
Write-Output ""

Write-Output "---- DocumentModels, EVERY member, public and not ----"
$dm = $asm.GetType("Autodesk.Navisworks.Api.DocumentParts.DocumentModels")
if ($null -eq $dm) { Write-Output "  UNKNOWN: no DocumentParts.DocumentModels type" } else {
  Write-Output ("  base type: " + $dm.BaseType.FullName)
  Write-Output ("  implements: " + (($dm.GetInterfaces() | ForEach-Object { $_.Name }) -join ", "))
  foreach ($m in ($dm.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly") | Sort-Object Name)) { Show-Member $m }
  foreach ($p in ($dm.GetProperties("Public,NonPublic,Instance,Static,DeclaredOnly") | Sort-Object Name)) {
    Write-Output ("  property " + $p.PropertyType.Name + " " + $p.Name + "  canread=" + $p.CanRead + " canwrite=" + $p.CanWrite)
  }
}
Write-Output ""

Write-Output "---- THE QUESTION: anything anywhere that takes a Model or an index and removes it ----"
$words = @("remove","delete","detach","unload","close","drop","eject","discard")
$model = $asm.GetType("Autodesk.Navisworks.Api.Model")
$hits = 0
foreach ($ty in $asm.GetTypes()) {
  foreach ($m in $ty.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    $low = $m.Name.ToLowerInvariant()
    $named = $false
    foreach ($w in $words) { if ($low.Contains($w)) { $named = $true; break } }
    if (-not $named) { continue }

    # Only the ones that could plausibly take a model out of a document: a Model
    # parameter, or an int index on a type whose name holds Model or Document.
    $takesModel = $false
    foreach ($q in $m.GetParameters()) {
      if ($null -ne $model -and $model.IsAssignableFrom($q.ParameterType)) { $takesModel = $true }
      if ($q.ParameterType.Name -eq "Int32" -and ($ty.Name.Contains("Model") -or $ty.Name.Contains("Document"))) { $takesModel = $true }
    }
    if ($m.GetParameters().Count -eq 0 -and $ty.Name.Contains("Model")) { $takesModel = $true }
    if (-not $takesModel) { continue }

    $vis = "private"
    if ($m.IsPublic) { $vis = "public" } elseif ($m.IsFamily) { $vis = "protected" } elseif ($m.IsAssembly) { $vis = "internal" }
    $ps = @()
    foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
    Write-Output ("  " + $ty.FullName + "  ->  " + $vis + " " + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
    $hits++
  }
}
if ($hits -eq 0) { Write-Output "  none, public or not, anywhere in the assembly. The answer is NO and the clear stays" }
Write-Output ""

Write-Output "---- Model itself, in case the model knows how to leave ----"
if ($null -eq $model) { Write-Output "  UNKNOWN: no Autodesk.Navisworks.Api.Model type" } else {
  Write-Output ("  base type: " + $model.BaseType.FullName)
  foreach ($m in ($model.GetMethods("Public,Instance,Static,DeclaredOnly") | Sort-Object Name)) { Show-Member $m }
}
Write-Output ""

Write-Output "---- Document.Clear and the append forms, for the comparison ----"
foreach ($m in ($doc.GetMethods("Public,Instance,DeclaredOnly") | Sort-Object Name)) {
  $low = $m.Name.ToLowerInvariant()
  if ($low.Contains("clear") -or $low.Contains("append")) { Show-Member $m }
}
Write-Output ""
Write-Output "Paste all of the above into docs\history\scan.md under the F50 section, with today's date."
