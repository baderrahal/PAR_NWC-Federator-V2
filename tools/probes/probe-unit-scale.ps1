param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F104, PQ2. Is there a public static Autodesk.Navisworks.Api.UnitConversion.ScaleFactor
# taking two Units and returning a double, and what is its exact signature.
#
# The document read probe writes metres per document unit from this member, so the check
# against the workbook converts through Navisworks' own factor and never through UnitTable,
# which is the table the harvest converted with. If the member is not there the probe
# writes metres per unit as UNKNOWN, and nothing here builds a table of its own.
#
# Read off the metadata alone with ReflectionOnlyLoadFrom, so no line of the DLL runs and
# no Navisworks is started. What the member returns for Feet to Meters is therefore not
# read here. The probe reads it inside Navisworks, per document.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"

# The path is built and tested directly, never searched for, the same rule the build uses.
if (-not (Test-Path -LiteralPath $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

$asm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($api)
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version + ", read only, no code run")
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
  $ret = "void"
  if ($m -is [System.Reflection.MethodInfo]) { $ret = $m.ReturnType.FullName }
  return ((Vis $m) + " " + $static + $ret + " " + $m.Name + "(" + ($ps -join ", ") + ")")
}

$unitsType = $asm.GetType("Autodesk.Navisworks.Api.Units")
$conv = $asm.GetType("Autodesk.Navisworks.Api.UnitConversion")

Write-Output "==== Autodesk.Navisworks.Api.UnitConversion"
if ($null -eq $conv) {
  Write-Output "  UNKNOWN: no such type in this assembly"
} else {
  $kind = "class"
  if ($conv.IsAbstract -and $conv.IsSealed) { $kind = "static class" }
  $tv = "not public"
  if ($conv.IsPublic) { $tv = "public" }
  Write-Output ("  " + $tv + " " + $kind + ", base " + $conv.BaseType.FullName)
  foreach ($m in $conv.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    if (-not $m.IsSpecialName) { Write-Output ("  " + (Sig $m)) }
  }
  foreach ($p in $conv.GetProperties("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    Write-Output ("  property " + $p.PropertyType.FullName + " " + $p.Name)
  }
  foreach ($f in $conv.GetFields("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    Write-Output ("  field " + $f.FieldType.FullName + " " + $f.Name)
  }
}
Write-Output ""

Write-Output "==== the answer"
$found = $null
if ($null -ne $conv -and $null -ne $unitsType) {
  foreach ($m in $conv.GetMethods("Public,Static,DeclaredOnly")) {
    $ps = $m.GetParameters()
    if ($m.Name -eq "ScaleFactor" -and $ps.Count -eq 2 -and
        $ps[0].ParameterType.FullName -eq $unitsType.FullName -and
        $ps[1].ParameterType.FullName -eq $unitsType.FullName -and
        $m.ReturnType.FullName -eq "System.Double") { $found = $m }
  }
}
if ($null -ne $found) {
  Write-Output ("  PQ2 YES   " + (Sig $found))
  Write-Output ("  first parameter " + $found.GetParameters()[0].Name + ", second " + $found.GetParameters()[1].Name)
  $body = $found.GetMethodBody()
  $il = "UNKNOWN"
  if ($null -ne $body) { $il = [string]$body.GetILAsByteArray().Length + " bytes of IL" }
  Write-Output ("  implementation " + $found.GetMethodImplementationFlags() + ", " + $il)
  Write-Output "  metres per Feet: UNKNOWN here, the member is not invoked. The probe reads it inside Navisworks"
} else {
  Write-Output "  PQ2 NO    no public static System.Double ScaleFactor(Units, Units) on UnitConversion"
  Write-Output "  metres per document unit: UNKNOWN, and the probe writes it as UNKNOWN"
}
Write-Output ""

Write-Output "==== Autodesk.Navisworks.Api.Units, the enum the probe names"
if ($null -eq $unitsType) { Write-Output "  UNKNOWN: no such type in this assembly" }
else { foreach ($f in $unitsType.GetFields("Public,Static")) { Write-Output ("  {0,-14} = {1}" -f $f.Name, $f.GetRawConstantValue()) } }
