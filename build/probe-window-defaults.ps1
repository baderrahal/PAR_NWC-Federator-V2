$ErrorActionPreference = "Stop"
$bundle = "$env:APPDATA\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22"
$repo = "C:\Users\p003653k\source\repos\Parsons NWC Federator"
$addin = Join-Path $repo "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll"
$nw = "C:\Program Files\Autodesk\Navisworks Manage 2025"

[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
  param($s, $e)
  $name = (New-Object System.Reflection.AssemblyName($e.Name)).Name
  foreach ($dir in @($bundle, $nw, (Split-Path $addin))) {
    $p = Join-Path $dir ($name + ".dll")
    if (Test-Path $p) { return [System.Reflection.Assembly]::LoadFrom($p) }
  }
  return $null
})

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

$asm = [System.Reflection.Assembly]::LoadFrom($addin)
$core = [System.Reflection.Assembly]::LoadFrom((Join-Path (Split-Path $addin) "Federator.Core.dll"))

$logType = $core.GetType("Federator.Core.Diagnostics.RunLog")
$log = $logType.GetMethod("StartOrDisabled", [Type]::EmptyTypes).Invoke($null, @())

$winType = $asm.GetType("Federator.Addin.Ui.FederatorWindow")
$window = [Activator]::CreateInstance($winType, @($log))

$window.Measure((New-Object System.Windows.Size(1400, 1000)))
$window.Arrange((New-Object System.Windows.Rect(0, 0, 1400, 1000)))

function Box($name) {
  $f = $winType.GetField($name, "NonPublic,Instance")
  if ($null -eq $f) { return $null }
  return $f.GetValue($window)
}

Write-Output "==== what the naming boxes hold when the window opens ===="
foreach ($n in @("NwfLevel","NwfDiscipline","NwfType","NwfNumber","NwfAllBuildings",
                 "NwdLevel","NwdDiscipline","NwdType","NwdNumber","NwdAllBuildings",
                 "WorkbookLevel","WorkbookDiscipline","WorkbookType","WorkbookNumber","WorkbookAllBuildings")) {
  $b = Box $n
  if ($null -eq $b) { Write-Output ("  {0,-22} NOT FOUND" -f $n); continue }
  $t = $b.Text
  Write-Output ("  {0,-22} {1,-10} {2} characters" -f $n, ("'" + $t + "'"), $t.Length)
}

Write-Output ""
Write-Output "==== the tick boxes, their text and their state ===="
foreach ($n in @("RepublishNwd","WriteClashXml","DateTheNwd","WriteImages","EmbedThumbnails",
                 "ClientColumnsOnly","ApplyFileSettings","CompactResolved","IncludeSubfolders")) {
  $b = Box $n
  if ($null -eq $b) { Write-Output ("  {0,-20} NOT FOUND" -f $n); continue }
  Write-Output ("  {0,-20} checked={1,-6} {2}" -f $n, $b.IsChecked, $b.Content)
}

Write-Output ""
Write-Output "==== what the naming patterns themselves hold ===="
$nf = $winType.GetField("naming", "NonPublic,Instance")
$naming = $nf.GetValue($window)
foreach ($which in @("Nwf","Nwd","Workbook")) {
  $pat = $naming.GetType().GetProperty($which).GetValue($naming, $null)
  $vals = @()
  foreach ($f in @("Level","Discipline","TypeCode","Number","AllBuildings")) {
    $vals += ($f + "='" + $pat.GetType().GetProperty($f).GetValue($pat, $null) + "'")
  }
  Write-Output ("  " + $which + "  " + ($vals -join "  "))
}

Write-Output ""
Write-Output "==== calling ShowNaming again by hand ===="
$winType.GetMethod("ShowNaming", "NonPublic,Instance").Invoke($window, @())
foreach ($n in @("NwfLevel","NwfNumber","NwdNumber","WorkbookNumber")) {
  $b = Box $n
  Write-Output ("  {0,-18} {1}" -f $n, ("'" + $b.Text + "'"))
}

$window.Close()
Write-Output ""
Write-Output "PROBE DONE"
