param(
  [Parameter(Mandatory = $true)][int]$ProcId,
  [Parameter(Mandatory = $true)][string]$Folder,
  [Parameter(Mandatory = $true)][string]$NamesFile,
  [Parameter(Mandatory = $true)][string]$SaveAs,
  [Parameter(Mandatory = $true)][string]$Out
)
# F231. The manual Append of one group, as a person makes it: File, New, then Append with the group's NWCs selected in the
# tool's order, then Save as an NWF. Acts on the windows of one Navisworks process alone and touches no option of Navisworks.
# The ribbon is pressed through UI Automation's Invoke, from a runspace of its own so a modal dialog never holds this one.
# The two file dialogs show their controls to UI Automation as panes with no pattern, so they are worked by the messages a
# person's typing and click send: WM_SETTEXT into the file name box, read back by WM_GETTEXT, and WM_COMMAND with the
# button's id, the way F213 answers a Navisworks box. -NamesFile holds the file names one a line, in the order to select them.
$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "uia.ps1")
Add-Type @"
using System; using System.Text; using System.Runtime.InteropServices; using System.Collections.Generic;
public static class Dlg { public delegate bool P(IntPtr h, IntPtr l);
 [DllImport("user32.dll")] public static extern bool EnumWindows(P p, IntPtr l);
 [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent, P p, IntPtr l);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
 [DllImport("user32.dll")] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
 [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")] public static extern int GetDlgCtrlID(IntPtr h);
 [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, string l);
 [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, StringBuilder l);
 [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
 public static List<IntPtr> Of(uint want) { var r = new List<IntPtr>(); EnumWindows((h, l) => { uint p; GetWindowThreadProcessId(h, out p); if (p == want && IsWindowVisible(h)) r.Add(h); return true; }, IntPtr.Zero); return r; }
 public static List<IntPtr> Children(IntPtr parent) { var r = new List<IntPtr>(); EnumChildWindows(parent, (h, l) => { r.Add(h); return true; }, IntPtr.Zero); return r; }
 public static string Cls(IntPtr h) { var c = new StringBuilder(256); GetClassName(h, c, 256); return c.ToString(); }
 public static string Txt(IntPtr h) { var c = new StringBuilder(1024); GetWindowText(h, c, 1024); return c.ToString(); }
 public static string EditText(IntPtr h) { var c = new StringBuilder(4096); SendMessage(h, 0x000D, (IntPtr)4096, c); return c.ToString(); }
 public static string ComboItem(IntPtr h, int i) { int n = (int)SendMessage(h, 0x0149, (IntPtr)i, IntPtr.Zero); var c = new StringBuilder(Math.Max(n, 0) + 2); SendMessage(h, 0x0148, (IntPtr)i, c); return c.ToString(); } }
"@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($Out, "", $utf8)
function Say($t) { $l = [DateTime]::Now.ToString("HH:mm:ss.fff") + "  " + $t; [Console]::Out.WriteLine($l); [System.IO.File]::AppendAllText($Out, $l + "`r`n", $utf8) }

function Main { return (TopWindows $ProcId | Where-Object { $_.Current.Name -like '*Autodesk Navisworks Manage 2025*' } | Select-Object -First 1) }
function Dialogs { return @([Dlg]::Of([uint32]$ProcId) | Where-Object { [Dlg]::Cls($_) -eq '#32770' }) }
function WaitDialog([string]$title, [int]$seconds) {
  $until = [DateTime]::Now.AddSeconds($seconds)
  do {
    foreach ($h in (Dialogs)) { if ([Dlg]::Txt($h) -eq $title) { return $h } }
    Start-Sleep -Milliseconds 250
  } while ([DateTime]::Now -lt $until)
  return [IntPtr]::Zero
}
function InvokeAsync($element) {
  $p = $null
  if (-not $element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$p)) { throw "no invoke pattern on " + $element.Current.Name }
  $rs = [RunspaceFactory]::CreateRunspace(); $rs.Open()
  $ps = [PowerShell]::Create(); $ps.Runspace = $rs
  [void]$ps.AddScript({ param($pat) $pat.Invoke() }).AddArgument($p)
  return @{ PS = $ps; Handle = $ps.BeginInvoke() }
}
function ById($root, [string]$id) {
  $cond = New-Object System.Windows.Automation.PropertyCondition($AE::AutomationIdProperty, $id)
  return @($root.FindAll($TS::Descendants, $cond))
}
function Child([IntPtr]$dialog, [string]$class, [int]$id) {
  return @([Dlg]::Children($dialog) | Where-Object { [Dlg]::Cls($_) -eq $class -and [Dlg]::GetDlgCtrlID($_) -eq $id -and [Dlg]::IsWindowVisible($_) })
}
function TypeName([IntPtr]$dialog, [string]$text) {
  $edits = @(Child $dialog 'Edit' 1148)
  if ($edits.Count -eq 0) { $edits = @(Child $dialog 'Edit' 1001) }
  if ($edits.Count -ne 1) { throw ("file name boxes found " + $edits.Count) }
  [void][Dlg]::SendMessage($edits[0], 0x000C, [IntPtr]::Zero, $text)
  return [Dlg]::EditText($edits[0])
}
function PressOk([IntPtr]$dialog) { [void][Dlg]::PostMessage($dialog, 0x0111, [IntPtr]1, [IntPtr]::Zero) }
function SayDialogs([string]$when) {
  foreach ($h in (Dialogs)) {
    $texts = @([Dlg]::Children($h) | Where-Object { [Dlg]::Cls($_) -eq 'Static' -or [Dlg]::Cls($_) -eq 'Button' } | ForEach-Object { [Dlg]::Txt($_) } | Where-Object { $_ -ne '' })
    Say ("DIALOG " + $when + ": '" + [Dlg]::Txt($h) + "', texts: " + ($texts -join ' | '))
  }
}

$names = @([System.IO.File]::ReadAllLines($NamesFile) | Where-Object { $_ -ne '' })
Say ("manual Append of " + $names.Count + " files from " + $Folder + " into " + $SaveAs + ", Navisworks pid " + $ProcId)
foreach ($nm in $names) { Say ("  in this order: " + $nm) }
$main = Main
if ($null -eq $main) { Say "STOP: no main window of that process"; exit 2 }
Say ("main window: " + $main.Current.Name)
SayDialogs "before New"

$new = @(ById $main 'RoamerGUI_FILE_NEW')
if ($new.Count -ne 1) { Say ("STOP: New buttons found " + $new.Count); exit 2 }
$n = InvokeAsync $new[0]
Start-Sleep -Seconds 3
SayDialogs "after New"
Say ("File, New pressed, the quick access bar's New, RoamerGUI_FILE_NEW, main window now: " + (Main).Current.Name)

$ap = @(ById (Main) 'RoamerGUI_FILE_APPEND_FlyoutButtonExecute')
if ($ap.Count -ne 1) { Say ("STOP: Append buttons found " + $ap.Count); exit 2 }
$a = InvokeAsync $ap[0]
$dlg = WaitDialog 'Append' 30
if ($dlg -eq [IntPtr]::Zero) { SayDialogs "no Append dialog"; Say "STOP: no Append dialog in 30 s"; exit 3 }
Say "Append pressed, the Home tab's Append, RoamerGUI_FILE_APPEND, and its dialog is up"
$types = @(Child $dlg 'ComboBox' 1136)
if ($types.Count -eq 1) { Say ("  its file type box reads '" + [Dlg]::Txt($types[0]) + "', left as it is") }
$typed = TypeName $dlg $Folder
Say ("typed the folder, the box reads " + $typed)
PressOk $dlg
Start-Sleep -Seconds 2
$dlg = WaitDialog 'Append' 5
if ($dlg -eq [IntPtr]::Zero) { SayDialogs "after the folder"; Say "STOP: the Append dialog closed on the folder"; exit 3 }
$list = ($names | ForEach-Object { '"' + $_ + '"' }) -join ' '
$typed = TypeName $dlg $list
Say ("typed the files, the box reads " + $typed)
if ($typed -ne $list) { Say "STOP: the box does not read what was typed"; exit 3 }
PressOk $dlg
$until = [DateTime]::Now.AddSeconds(180)
while ([DateTime]::Now -lt $until -and (WaitDialog 'Append' 0) -ne [IntPtr]::Zero) { Start-Sleep -Milliseconds 250 }
if ((WaitDialog 'Append' 0) -ne [IntPtr]::Zero) { SayDialogs "Append still up"; Say "STOP: the Append dialog is still up"; exit 3 }
Say "the Append dialog is gone, waiting 15 s for the models"
Start-Sleep -Seconds 15
SayDialogs "after the Append"
Say ("main window now: " + (Main).Current.Name)

$save = @(ById (Main) 'RoamerGUI_FILE_SAVE')
if ($save.Count -ne 1) { Say ("STOP: Save buttons found " + $save.Count); exit 4 }
$s = InvokeAsync $save[0]
$sdlg = WaitDialog 'Save As' 30
if ($sdlg -eq [IntPtr]::Zero) { SayDialogs "no Save As dialog"; Say "STOP: no Save As dialog in 30 s"; exit 4 }
Say "Save pressed, the quick access bar's Save, and for an untitled document its Save As dialog is up"
$types = @([Dlg]::Children($sdlg) | Where-Object { [Dlg]::Cls($_) -eq 'ComboBox' -and [int][Dlg]::SendMessage($_, 0x0146, [IntPtr]::Zero, [IntPtr]::Zero) -gt 0 })
if ($types.Count -ne 1) { Say ("STOP: save as type boxes holding items found " + $types.Count); exit 4 }
$count = [int][Dlg]::SendMessage($types[0], 0x0146, [IntPtr]::Zero, [IntPtr]::Zero)
$items = @(for ($i = 0; $i -lt $count; $i++) { [Dlg]::ComboItem($types[0], $i) })
$cur = [int][Dlg]::SendMessage($types[0], 0x0147, [IntPtr]::Zero, [IntPtr]::Zero)
Say ("  save as types: " + ($items -join ' | ') + ", chosen now: " + $(if ($cur -ge 0) { $items[$cur] } else { "none" }) + ", left as it is")
if ($cur -lt 0 -or $items[$cur] -notlike '*File Set (*.nwf)*') { Say "STOP: the type chosen is not an NWF file set, and no option is touched"; exit 4 }
$typed = TypeName $sdlg $SaveAs
Say ("typed the NWF path, the box reads " + $typed)
PressOk $sdlg
$until = [DateTime]::Now.AddSeconds(180)
while ([DateTime]::Now -lt $until -and (WaitDialog 'Save As' 0) -ne [IntPtr]::Zero) { Start-Sleep -Milliseconds 250 }
Start-Sleep -Seconds 5
SayDialogs "after the save"
Say ("main window now: " + (Main).Current.Name)
Say ("the NWF on disk: " + $(if (Test-Path -LiteralPath $SaveAs) { (Get-Item -LiteralPath $SaveAs).Length.ToString() + " bytes, sha256 " + (Get-FileHash -LiteralPath $SaveAs -Algorithm SHA256).Hash } else { "NOT THERE" }))
foreach ($j in @($n, $a, $s)) { try { if ($j.Handle.IsCompleted) { [void]$j.PS.EndInvoke($j.Handle) } } catch { Say ("an invoke ended with " + $_.Exception.Message) } }
Say "DONE"
exit 0
