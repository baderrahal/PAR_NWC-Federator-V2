# Reads every tick box in the window and checks its label and help line against the
# limits, and that no code identifier reaches a label. Bader called the old ones a
# nightmare: they ran off the edge, shouted in capitals, and explained the off state too.
param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025", [int]$maxLabelWords = 8, [int]$maxHelpWords = 12)
$ErrorActionPreference = "Stop"
$bundle = "$env:APPDATA\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle\Contents\v22"
$repo = Split-Path (Split-Path $PSScriptRoot)
$addin = Join-Path $repo "src\Federator.Addin\bin\Release\net48\Federator.Addin.dll"
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
$window.Width = 1280
$window.Height = 800
$window.WindowStartupLocation = "Manual"
$window.Left = -20000
$window.Top = -20000
$window.ShowInTaskbar = $false
$window.Show()
$window.UpdateLayout()

# Walk every step so all four are realised.
$steps = $winType.GetField("Steps", "NonPublic,Instance").GetValue($window)
$panels = @()
for ($i = 0; $i -lt $steps.Items.Count; $i++) {
  $steps.SelectedIndex = $i
  $window.UpdateLayout()
  $panels += $steps.Items[$i].Content
}

function Walk($node, $found, $type) {
  if ($null -eq $node) { return }
  $stack = New-Object System.Collections.Stack
  $stack.Push($node)
  while ($stack.Count -gt 0) {
    $n = $stack.Pop()
    if ($n -is $type) { [void]$found.Add($n) }
    $c = [System.Windows.Media.VisualTreeHelper]::GetChildrenCount($n)
    for ($k = 0; $k -lt $c; $k++) { $stack.Push([System.Windows.Media.VisualTreeHelper]::GetChild($n, $k)) }
  }
}

# What is visible WITHOUT opening anything. That is the number this window is judged on,
# because a decision behind an expander is one nobody is asked to make.
$upFront = New-Object System.Collections.ArrayList
foreach ($panel in $panels) { Walk $panel $upFront ([System.Windows.Controls.CheckBox]) }
$upFrontNames = @()
foreach ($b in $upFront) { if ($b.Name) { $upFrontNames += $b.Name } }

# Now open every expander, more than once because opening one can reveal another, so the
# boxes inside them are checked against the same limits as the ones in plain sight.
$opened = @()
for ($pass = 0; $pass -lt 4; $pass++) {
  $ex = New-Object System.Collections.ArrayList
  for ($i = 0; $i -lt $steps.Items.Count; $i++) {
    $steps.SelectedIndex = $i
    $window.UpdateLayout()
    Walk $steps.Items[$i].Content $ex ([System.Windows.Controls.Expander])
  }
  $more = $false
  foreach ($e in $ex) {
    if (-not $e.IsExpanded) { $e.IsExpanded = $true; $opened += [string]$e.Header; $more = $true }
  }
  $window.UpdateLayout()
  if (-not $more) { break }
}

$boxes = New-Object System.Collections.ArrayList
for ($i = 0; $i -lt $steps.Items.Count; $i++) {
  $steps.SelectedIndex = $i
  $window.UpdateLayout()
  Walk $steps.Items[$i].Content $boxes ([System.Windows.Controls.CheckBox])
}

# One word boxes that share a line and are not settings with a cost, so no help line.
$plain = @("ImageNew", "ImageActive", "ImageReviewed", "ImageApproved", "ImageResolved",
           "IncludeSubfolders")

# Acronyms a person would say out loud. Anything else in capitals is shouting.
$acronyms = @("NWD", "NWF", "NWC", "XML", "HTML", "MB", "KB", "GB", "PDF", "ID")

# camelCase or PascalCase run together, a parameter name, or an exception word. Every
# comparison below is case SENSITIVE, because -match on its own is not.
$identifier = "[a-z][A-Z]|Parameter name|nwfFolder|System\.|Exception|\bnull\b"

$bad = 0
Write-Output ("limits: label {0} words, help {1} words" -f $maxLabelWords, $maxHelpWords)
Write-Output ""

foreach ($box in $boxes) {
  $name = $box.Name
  if ([string]::IsNullOrEmpty($name)) { continue }
  $label = [string]$box.Content
  $words = ($label -split "\s+" | Where-Object { $_ -ne "" }).Count

  # The grey line under it. On the two that destroy something the box sits in an inner
  # horizontal StackPanel with the warning mark, so the help line is one level further out.
  $parent = [System.Windows.Media.VisualTreeHelper]::GetParent($box)
  while ($null -ne $parent -and -not ($parent -is [System.Windows.Controls.StackPanel])) {
    $parent = [System.Windows.Media.VisualTreeHelper]::GetParent($parent)
  }
  if ($null -ne $parent -and $parent.Orientation -eq "Horizontal") {
    $parent = [System.Windows.Media.VisualTreeHelper]::GetParent($parent)
    while ($null -ne $parent -and -not ($parent -is [System.Windows.Controls.StackPanel])) {
      $parent = [System.Windows.Media.VisualTreeHelper]::GetParent($parent)
    }
  }

  $help = ""
  if ($null -ne $parent) {
    foreach ($child in $parent.Children) {
      if ($child -is [System.Windows.Controls.TextBlock] -and $child.Text -ne "!") { $help = $child.Text }
    }
  }
  $helpWords = ($help -split "\s+" | Where-Object { $_ -ne "" }).Count

  $notes = @()
  if ($words -gt $maxLabelWords) { $notes += ("label " + $words + " words") }
  if ($helpWords -gt $maxHelpWords) { $notes += ("help " + $helpWords + " words") }

  foreach ($text in @($label, $help)) {
    foreach ($shout in ([regex]::Matches($text, "\b[A-Z]{2,}\b"))) {
      if ($acronyms -notcontains $shout.Value) { $notes += ("shouts " + $shout.Value) }
    }
  }

  if ($label -cmatch $identifier) { $notes += "code identifier in label" }
  if ($help -cmatch $identifier) { $notes += "code identifier in help" }
  if ($plain -notcontains $name -and $helpWords -eq 0) { $notes += "no help line" }

  $verdict = "ok"
  if ($notes.Count -gt 0) { $verdict = ($notes -join ", "); $bad++ }

  $where = "up front"
  if ($upFrontNames -notcontains $name) { $where = "behind an expander" }

  Write-Output ("{0,-20} {1,2}w {2,2}w  {3,-18}  {4}" -f $name, $words, $helpWords, $where, $verdict)
  if ($help -ne "") { Write-Output ("                              " + $help) }
}

Write-Output ""
Write-Output ("expanders opened: " + (($opened | Select-Object -Unique) -join ", "))
$hidden = $boxes.Count - $upFrontNames.Count
Write-Output ("tick boxes: " + $boxes.Count + "   up front: " + $upFrontNames.Count + "   behind an expander: " + $hidden + "   problems: " + $bad)
$window.Close()
Write-Output "PROBE DONE"
