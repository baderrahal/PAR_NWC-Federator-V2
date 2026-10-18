# UI Automation helpers for the manual Append of F231, dot-sourced. Acts only on the windows of one process id.
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class W32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
  [DllImport("user32.dll")] public static extern IntPtr PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
}
"@
$AE = [System.Windows.Automation.AutomationElement]
$TS = [System.Windows.Automation.TreeScope]

function TopWindows([int]$procId) {
  $cond = New-Object System.Windows.Automation.PropertyCondition($AE::ProcessIdProperty, $procId)
  return @($AE::RootElement.FindAll($TS::Children, $cond))
}

function Describe($e) {
  try { $c = $e.Current; return ($c.ControlType.ProgrammaticName + " | name '" + $c.Name + "' | id '" + $c.AutomationId + "' | class '" + $c.ClassName + "' | rect " + $c.BoundingRectangle) } catch { return "UNREADABLE " + $_.Exception.Message }
}

function Dump($root, [int]$depth, [string]$out) {
  $lines = New-Object System.Collections.Generic.List[string]
  $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
  function Walk($e, [int]$d) {
    $lines.Add((" " * ($d * 2)) + (Describe $e))
    if ($d -ge $depth) { return }
    $child = $walker.GetFirstChild($e)
    while ($null -ne $child) { Walk $child ($d + 1); $child = $walker.GetNextSibling($child) }
  }
  Walk $root 0
  [System.IO.File]::WriteAllLines($out, $lines)
  return $lines.Count
}

function FindByName($root, [string]$name) {
  $cond = New-Object System.Windows.Automation.PropertyCondition($AE::NameProperty, $name)
  return @($root.FindAll($TS::Descendants, $cond))
}

function Invoke($e) {
  $p = $null
  if ($e.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$p)) { $p.Invoke(); return "invoked" }
  return "no invoke pattern"
}

function ClickAt([int]$x, [int]$y) {
  [W32]::SetCursorPos($x, $y) | Out-Null
  Start-Sleep -Milliseconds 150
  [W32]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  Start-Sleep -Milliseconds 60
  [W32]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
}

function ClickElement($e) {
  $r = $e.Current.BoundingRectangle
  ClickAt ([int]($r.X + $r.Width / 2)) ([int]($r.Y + $r.Height / 2))
}
