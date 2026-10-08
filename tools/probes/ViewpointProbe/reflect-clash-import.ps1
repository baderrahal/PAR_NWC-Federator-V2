param([Parameter(Mandatory = $true)][string]$Out)
$ErrorActionPreference = "Stop"

# Q133 on 1A04PK, 2026-10-07. Is there a clash XML import in the API? Reads the four DLLs the add-in
# references, by reflection, with no Navisworks started, and lists every public member whose name
# holds Import or Xml. Each DLL is one full path, tested on its own, never searched for.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\reflect-clash-import.ps1 -Out <file>

$nw = "C:\Program Files\Autodesk\Navisworks Manage 2025"
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("READ " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss") + ", public members whose name holds Import or Xml")
foreach ($n in @("Autodesk.Navisworks.Api.dll", "Autodesk.Navisworks.Clash.dll", "Autodesk.Navisworks.ComApi.dll", "Autodesk.Navisworks.Interop.ComApi.dll")) {
  $p = Join-Path $nw $n
  if (-not (Test-Path -LiteralPath $p)) { $lines.Add($n + "  NOT THERE"); continue }
  $a = [System.Reflection.Assembly]::LoadFrom($p)
  $types = $null
  try { $types = $a.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $types = @($_.Exception.Types | Where-Object { $null -ne $_ }); $lines.Add($n + "  some types could not be loaded, " + $types.Count + " read") }
  $found = 0
  foreach ($t in $types) {
    $ms = $null
    try { $ms = $t.GetMembers([System.Reflection.BindingFlags]"Public,Instance,Static,DeclaredOnly") } catch { continue }
    foreach ($m in $ms) { if ($m.Name -match "Import|Xml") { $lines.Add($n + "  " + $t.FullName + "  " + $m.MemberType + "  " + $m.ToString()); $found++ } }
  }
  $lines.Add($n + "  types read " + @($types).Count + ", members found " + $found)
}
[System.IO.File]::WriteAllLines($Out, $lines, (New-Object System.Text.UTF8Encoding($false)))
$lines
