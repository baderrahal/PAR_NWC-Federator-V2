# Can the document's units be set, and what controls them. Read off the installed DLL.
$ErrorActionPreference = "Stop"
$nw = "C:/Program Files/Autodesk/Navisworks Manage 2025"
$a = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom((Join-Path $nw "Autodesk.Navisworks.Api.dll"))

function Sig($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
  $vis = "private"
  if ($m.IsPublic) { $vis = "public" } elseif ($m.IsFamily) { $vis = "protected" } elseif ($m.IsAssembly) { $vis = "internal" }
  $ret = "void"
  if ($m -is [System.Reflection.MethodInfo]) { $ret = $m.ReturnType.Name }
  return ("    " + $vis + " " + $ret + " " + $m.Name + "(" + ($ps -join ", ") + ")")
}

Write-Output "==== Document, anything naming units"
$doc = $a.GetType("Autodesk.Navisworks.Api.Document")
foreach ($p in $doc.GetProperties("Public,NonPublic,Instance,Static,DeclaredOnly")) {
  if ($p.Name -match "(?i)unit") {
    Write-Output ("    property " + $p.PropertyType.Name + " " + $p.Name +
      "   canread=" + $p.CanRead + " canwrite=" + $p.CanWrite +
      "   setter=" + $(if ($p.GetSetMethod($true)) { if ($p.GetSetMethod($true).IsPublic) {"public"} else {"not public"} } else {"none"}))
  }
}
foreach ($m in $doc.GetMethods("Public,NonPublic,Instance,Static,DeclaredOnly")) {
  if ($m.Name -match "(?i)unit" -and -not $m.IsSpecialName) { Write-Output (Sig $m) }
}

Write-Output ""
Write-Output "==== Units, the enum"
$u = $a.GetType("Autodesk.Navisworks.Api.Units")
if ($u) { foreach ($f in $u.GetFields("Public,Static")) { Write-Output ("    {0,-14} = {1}" -f $f.Name, $f.GetRawConstantValue()) } }

Write-Output ""
Write-Output "==== Model, which is one appended file"
$model = $a.GetType("Autodesk.Navisworks.Api.Model")
foreach ($p in $model.GetProperties("Public,Instance,DeclaredOnly")) {
  Write-Output ("    " + $p.PropertyType.Name + " " + $p.Name + "  canwrite=" + $p.CanWrite)
}

Write-Output ""
Write-Output "==== anything about units on the application options"
$opt = $a.GetType("Autodesk.Navisworks.Api.Application")
foreach ($p in $opt.GetProperties("Public,Static,DeclaredOnly")) {
  Write-Output ("    Application." + $p.Name + " : " + $p.PropertyType.FullName)
}

Write-Output ""
Write-Output "==== every type with a settable Units property"
$types = @()
try { $types = $a.GetTypes() } catch { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }
foreach ($t in $types) {
  foreach ($p in $t.GetProperties("Public,NonPublic,Instance,Static,DeclaredOnly")) {
    if ($p.Name -match "(?i)^units?$" -and $p.CanWrite) {
      $s = $p.GetSetMethod($true)
      Write-Output ("    " + $t.FullName + "." + $p.Name + "  setter " + $(if ($s.IsPublic) {"PUBLIC"} else {"not public"}))
    }
  }
}

Write-Output ""
Write-Output "==== DocumentModels and anything about changing units"
foreach ($t in $types) {
  foreach ($m in $t.GetMethods("Public,Static,Instance,DeclaredOnly")) {
    if ($m.Name -match "(?i)(convert|change|set).*unit|unit.*(convert|change)") {
      Write-Output ("    " + $t.FullName + "." + $m.Name)
    }
  }
}
