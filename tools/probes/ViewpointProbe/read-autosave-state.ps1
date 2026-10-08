param(
  [Parameter(Mandatory = $true)][string]$Out,
  [Parameter(Mandatory = $true)][string]$Label
)
$ErrorActionPreference = "Stop"

# Q133 on 1A04PK, 2026-10-07. Reads, and writes nothing but -Out: the value enable of
# GlobalOptions\general\autosave under Bader's 22.0 key, the switch F138 writes off for a loop
# start, and his AutoSave folder listed by name, size, write time and sha256, so a file a
# probe's Navisworks adds there is seen by name. Run it before the probe, while it runs after
# the guard wrote the switch, and after the put back:
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\ViewpointProbe\read-autosave-state.ps1 -Out <file> -Label <before|during|after>

$utf8 = New-Object System.Text.UTF8Encoding($false)
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("READ " + $Label + " at " + [DateTime]::Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
$sub = "Software\Autodesk\Navisworks Manage\22.0\GlobalOptions\general\autosave"
$k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($sub, $false)
if ($null -eq $k) { $lines.Add("enable: the key HKCU\" + $sub + " is not there") } else {
  try {
    $names = @($k.GetValueNames())
    if ($names -notcontains "enable") { $lines.Add("enable: no such value under HKCU\" + $sub) }
    else { $lines.Add("enable: kind " + $k.GetValueKind("enable") + ", data """ + $k.GetValue("enable") + """") }
  } finally { $k.Close() }
}
$folder = Join-Path $env:APPDATA "Autodesk\Navisworks Manage 2025\AutoSave"
if (-not (Test-Path -LiteralPath $folder)) { $lines.Add("AutoSave folder: not there") } else {
  $files = @(Get-ChildItem -LiteralPath $folder -File -Force | Sort-Object Name)
  $lines.Add("AutoSave folder: " + $files.Count + " files")
  foreach ($f in $files) {
    $h = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
    $lines.Add($f.Name + "`t" + $f.Length + "`t" + $f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") + "`t" + $h)
  }
}
[System.IO.File]::WriteAllLines($Out, $lines, $utf8)
$lines[0]; $lines[1]; $lines[2]
