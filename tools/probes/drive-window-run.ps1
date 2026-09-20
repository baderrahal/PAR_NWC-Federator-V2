param(
  [Parameter(Mandatory = $true)][string]$Source,
  [Parameter(Mandatory = $true)][string]$Nwf,
  [Parameter(Mandatory = $true)][string]$Nwd,
  [Parameter(Mandatory = $true)][string]$Excel,
  [string]$Xml = "",
  [string]$Pairs = "",
  [string]$Priority = "",
  [string]$Tolerance = "",
  [switch]$Penetrations,
  [string]$Notes = (Join-Path $env:TEMP "drive-window-run.txt"),
  [int]$WindowWaitSeconds = 600
)

# Drives the real window through UI Automation, F74 to F86 wiring round, 2026-09-19.
#
# WHAT IT IS FOR. PART 5 of a round is one run of one folder with the boxes set a known
# way, read back off the log. Doing that by hand is fine once and wrong every week, and
# this is how the wiring round did it from a session that could not press a button.
#
# WHAT IT NEEDS, measured on 2026-09-19 on DESKTOP-5VL7LTJ:
#
#   Navisworks Manage 2025 open the ordinary way, Roamer.exe, with the add-in window
#   ALREADY OPEN. Starting Navisworks through Autodesk.Navisworks.Api.Automation and
#   calling ExecuteAddInPlugin("ParsonsNwcFederator.PARS") ran the plugin, wrote a log,
#   and the window closed on its own 8.5 seconds later with "Window closed." and nothing
#   else in the log. Why is UNKNOWN. Started the ordinary way the window stays open.
#
#   The ribbon's Tool add-ins 1 tab is a Button with AutomationId RoamerGUI_AddIns_Tools_1
#   and an InvokePattern that does nothing. A real mouse click on its rectangle switches
#   the tab. The add-in's own button on that tab has NO element in the automation tree at
#   all, so it is pressed by hand or by a mouse click on where it is drawn.
#
#   The add-in window is NOT a child of the desktop root in the automation tree, because
#   it is owned by the Navisworks main window. It is found by its title through user32
#   and handed to AutomationElement.FromHandle. The confirm dialog is a #32770 titled
#   exactly "Parsons NWC Federator" and is found the same way.
#
#   Every control is found by its x:Name, which WPF exposes as the AutomationId, and a
#   control on a tab that is not selected has no visual tree, so each tab is selected
#   before its boxes are set.
#
# WHAT IT DOES. Selects each tab, sets the boxes, presses Scan, chooses the tolerance
# where one is given, ticks the by design box where a pairs file is given, presses Run,
# and presses OK on the confirm dialog. Then it leaves the run to the log and exits.
# Every step is written to the notes file with what the box read back.
#
# Run it with Windows PowerShell 5.1 in STA:
#
#   powershell -NoProfile -STA -ExecutionPolicy Bypass -File tools\probes\drive-window-run.ps1 `
#     -Source "D:\NWC" -Nwf "D:\NWF" -Nwd "D:\NWD" -Excel "D:\Excel" `
#     -Xml "exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml" `
#     -Pairs "samples\by-design-pairs.csv" -Priority "samples\clash-priority-map.csv" -Tolerance "25 mm"

$ErrorActionPreference = "Continue"
function Say($t) { $line = (Get-Date -Format "HH:mm:ss") + "  " + $t; Add-Content -Path $Notes -Value $line; Write-Output $line }
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class DriveWin32 {
  [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr hWnd, uint cmd);
  [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int max);
  [DllImport("user32.dll")] public static extern IntPtr GetTopWindow(IntPtr hWnd);
  [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr FindWindow(string cls, string title);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
  // A REAL MOUSE CLICK, because the tolerance list item answers no pattern that takes.
  public static void Click(int x, int y) {
    SetCursorPos(x, y);
    System.Threading.Thread.Sleep(150);
    mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
    System.Threading.Thread.Sleep(80);
    mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
  }
  public static IntPtr FindByPrefix(string prefix) {
    IntPtr h = GetTopWindow(IntPtr.Zero);
    var sb = new System.Text.StringBuilder(512);
    while (h != IntPtr.Zero) {
      sb.Clear();
      GetWindowText(h, sb, 512);
      string t = sb.ToString();
      if (t.StartsWith(prefix) && t.Length > prefix.Length) { return h; }
      h = GetWindow(h, 2);
    }
    return IntPtr.Zero;
  }
}
"@
$AE = [System.Windows.Automation.AutomationElement]
$TS = [System.Windows.Automation.TreeScope]

function ById($parent, $id) {
  return $parent.FindFirst($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition($AE::AutomationIdProperty, $id)))
}
# What a combo box actually reads, by whichever pattern answers, or UNKNOWN. Never a
# guess: the caller refuses to run on UNKNOWN the same way it refuses on a wrong value.
function ComboText($combo) {
  try { return $combo.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value } catch { }
  try {
    $sel = $combo.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection()
    if ($sel.Length -gt 0) { return $sel[0].Current.Name }
  } catch { }
  return "UNKNOWN"
}
function ByNameAndType($parent, $name, $type) {
  $c = New-Object System.Windows.Automation.AndCondition(@(
    (New-Object System.Windows.Automation.PropertyCondition($AE::NameProperty, $name)),
    (New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $type))))
  return $parent.FindFirst($TS::Descendants, $c)
}
function SetText($el, $text, $what) {
  if ($null -eq $el) { Say "UNKNOWN: no element for $what"; return $false }
  $el.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($text)
  Start-Sleep -Milliseconds 400
  Say ("set " + $what + " -> [" + $el.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value + "]")
  return $true
}
function Click($el, $what) {
  if ($null -eq $el) { Say "UNKNOWN: no element for $what"; return $false }
  $el.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
  Say ("clicked " + $what)
  return $true
}
function SelectTab($win, $header) {
  $tab = ByNameAndType $win $header ([System.Windows.Automation.ControlType]::TabItem)
  if ($null -eq $tab) { Say "UNKNOWN: no tab $header"; return $false }
  $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
  Start-Sleep -Milliseconds 600
  Say ("selected tab " + $header)
  return $true
}

Say "driver started"
$win = $null
$deadline = (Get-Date).AddSeconds($WindowWaitSeconds)
while ((Get-Date) -lt $deadline) {
  $h = [DriveWin32]::FindByPrefix("Parsons NWC Federator")
  if ($h -ne [IntPtr]::Zero) { $win = [System.Windows.Automation.AutomationElement]::FromHandle($h); break }
  Start-Sleep -Seconds 3
}
if ($null -eq $win) { Say "UNKNOWN: no window whose title starts with Parsons NWC Federator within $WindowWaitSeconds s"; exit 1 }
Say ("window found: [" + $win.Current.Name + "]")

if (-not (SelectTab $win "1. Source")) { exit 1 }
SetText (ById $win "SourceFolderBox") $Source "SourceFolderBox" | Out-Null
Click (ByNameAndType $win "Scan" ([System.Windows.Automation.ControlType]::Button)) "Scan" | Out-Null
Start-Sleep -Seconds 8

if (-not (SelectTab $win "3. Outputs")) { exit 1 }
SetText (ById $win "NwfFolderBox") $Nwf "NwfFolderBox" | Out-Null
SetText (ById $win "NwdFolderBox") $Nwd "NwdFolderBox" | Out-Null
SetText (ById $win "ExcelFolderBox") $Excel "ExcelFolderBox" | Out-Null

if (-not (SelectTab $win "4. Clash")) { exit 1 }
if ($Xml.Length -gt 0) { SetText (ById $win "ExchangeFileBox") $Xml "ExchangeFileBox" | Out-Null; Start-Sleep -Seconds 6 }
# THE TOLERANCE IS CLICKED AND THEN READ BACK, and the run does not start if it did not
# take. SelectionItemPattern.Select on this list item does NOTHING and throws nothing,
# measured on 2026-09-20, so the line under it said "tolerance selected" on a box that
# still read the default. That is a check that could not fail, which is the one shape
# this repo refuses everywhere else, and it would have run ten groups at the wrong
# tolerance while the notes said otherwise. A real mouse click on the item's rectangle
# takes, and the box is read back afterwards because the click is the action and the
# read is the check.
if ($Tolerance.Length -gt 0) {
  $combo = ById $win "ToleranceBox"
  if ($null -eq $combo) { Say "UNKNOWN: no ToleranceBox"; exit 1 }
  $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
  Start-Sleep -Milliseconds 900
  $item = ByNameAndType $win $Tolerance ([System.Windows.Automation.ControlType]::ListItem)
  if ($null -eq $item) { Say "UNKNOWN: no list item [$Tolerance] in ToleranceBox"; exit 1 }
  $r = $item.Current.BoundingRectangle
  [DriveWin32]::Click([int]($r.X + $r.Width / 2), [int]($r.Y + $r.Height / 2))
  Start-Sleep -Milliseconds 700
  try { $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse() } catch { }
  Start-Sleep -Milliseconds 300
  $reads = ComboText $combo
  Say ("tolerance box READ BACK as [" + $reads + "]")
  if ($reads -ne $Tolerance) { Say ("REFUSED: the box reads [" + $reads + "] and not [" + $Tolerance + "], so nothing was run"); exit 1 }
}
if ($Pairs.Length -gt 0) {
  $tick = ById $win "MarkByDesign"
  if ($null -eq $tick) { Say "UNKNOWN: no MarkByDesign" } else {
    $tp = $tick.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    if ($tp.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::On) { $tp.Toggle() }
    Say ("MarkByDesign is " + $tick.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState)
  }
  SetText (ById $win "ByDesignBox") $Pairs "ByDesignBox" | Out-Null
}
if ($Priority.Length -gt 0) { SetText (ById $win "PriorityBox") $Priority "PriorityBox" | Out-Null }
if ($Penetrations) {
  $pen = ById $win "MarkPenetrations"
  if ($null -eq $pen) { Say "UNKNOWN: no MarkPenetrations" } else {
    $pp = $pen.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    if ($pp.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::On) { $pp.Toggle() }
    Say ("MarkPenetrations is " + $pen.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState)
  }
}

Click (ById $win "RunButton") "Run" | Out-Null

$deadline = (Get-Date).AddSeconds(300)
while ((Get-Date) -lt $deadline) {
  $d = [DriveWin32]::FindWindow("#32770", "Parsons NWC Federator")
  if ($d -ne [IntPtr]::Zero) {
    $el = [System.Windows.Automation.AutomationElement]::FromHandle($d)
    foreach ($t in $el.FindAll($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::Text)))) { Say ("dialog: " + $t.Current.Name) }
    $ok = ByNameAndType $el "OK" ([System.Windows.Automation.ControlType]::Button)
    if ($null -eq $ok) { Start-Sleep -Seconds 2; $ok = ByNameAndType $el "OK" ([System.Windows.Automation.ControlType]::Button) }
    if ($null -eq $ok) { Say "UNKNOWN: no OK button on the confirm dialog, press it by hand"; exit 1 }
    $ok.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Say "pressed OK, the run is going, read the log"
    exit 0
  }
  Start-Sleep -Seconds 2
}
Say "UNKNOWN: no confirm dialog within 300 s"
exit 1
