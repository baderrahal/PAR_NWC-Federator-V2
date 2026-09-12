# Measures each step of the window against the height it actually gets, so which ones
# need a scrollbar is read rather than guessed. Bader reported the Outputs step cutting
# off its naming table.
param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025", [double]$w = 1280, [double]$h = 800)
$ErrorActionPreference = "Stop"
$bundle = "$env:APPDATA\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22"
$repo = Split-Path (Split-Path $PSScriptRoot)
$addin = Join-Path $repo "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll"
$core = Join-Path (Split-Path $addin) "Federator.Core.dll"
$api = Join-Path $NavisworksPath "Autodesk.Navisworks.Api.dll"

# Every path is built and tested directly, one file at a time so a missing one is named.
# Never search the install folder. The same rule the build uses.
if (-not (Test-Path $addin)) { Write-Output "UNKNOWN: no Federator.Addin.dll at $addin. Build in Release first"; exit 1 }
if (-not (Test-Path $core)) { Write-Output "UNKNOWN: no Federator.Core.dll at $core. Build in Release first"; exit 1 }
if (-not (Test-Path $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }
$nw = $NavisworksPath

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

# The size to check. Defaults to a laptop, which is what most of the 27 people have.
$window.Width = $w
$window.Height = $h

# Shown off screen, because an unshown Window does not lay its content out at all and
# every measurement off one comes back zero.
$window.WindowStartupLocation = "Manual"
$window.Left = -20000
$window.Top = -20000
$window.ShowInTaskbar = $false
$window.Show()
$window.UpdateLayout()

$steps = $winType.GetField("Steps", "NonPublic,Instance").GetValue($window)

Write-Output ("window {0} by {1}" -f $w, $h)
Write-Output ""
Write-Output "step                       given   wants   scrolls   verdict"

$bad = 0
for ($i = 0; $i -lt $steps.Items.Count; $i++) {
  $steps.SelectedIndex = $i
  $window.UpdateLayout()

  $item = $steps.Items[$i]
  $content = $item.Content

  # What the step is allowed. The tab strip and the tab's own border take some of it.
  $given = $steps.ActualHeight - 34

  # What it would take if nothing stopped it, at the width it really has.
  $width = $content.ActualWidth
  if ($width -lt 1) { $width = $steps.ActualWidth - 16 }
  $content.Measure((New-Object System.Windows.Size($width, [double]::PositiveInfinity)))
  $wants = $content.DesiredSize.Height

  # Only a ScrollViewer wrapping the step itself counts. A DataGrid brings its own and
  # that is not the same thing.
  $scrolls = "no"
  if ($content -is [System.Windows.Controls.ScrollViewer]) { $scrolls = "yes" }

  # Taller than the space is only a fault when nothing scrolls. A step that scrolls is
  # taller on purpose and every part of it can still be reached.
  $verdict = "fits"
  if ($wants -gt ($given + 1)) {
    if ($scrolls -eq "yes") {
      $verdict = "taller by " + [math]::Round($wants - $given) + " pixels, scrolls"
    } else {
      $verdict = "CUT OFF by " + [math]::Round($wants - $given) + " pixels"
      $bad++
    }
  }

  Write-Output ("{0,-25} {1,6} {2,7} {3,9}   {4}" -f `
    $item.Header, [math]::Round($given), [math]::Round($wants), $scrolls, $verdict)
}

Write-Output ""
Write-Output ("steps that do not fit: " + $bad)
$window.Close()
Write-Output "PROBE DONE"
