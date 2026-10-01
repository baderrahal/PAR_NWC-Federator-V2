param(
  [Parameter(Mandatory = $true)][int]$OwnerPid,
  [Parameter(Mandatory = $true)][long]$OwnerStartTicks,
  [Parameter(Mandatory = $true)][string]$Set,
  [Parameter(Mandatory = $true)][string]$Stamp,
  [Parameter(Mandatory = $true)][string]$Notes,
  [string]$Source = "",
  [string]$Nwf = "",
  [string]$Nwd = "",
  [string]$Excel = "",
  [string]$Xml = "",
  [switch]$OpenRun,
  [int]$WindowWaitSeconds = 600
)

# tools\probes\drive-window-run.ps1, F106. Drives the tool's window of ONE Navisworks, the one
# tools\loop\run.ps1 adopted, through UI Automation, for one run of the run set. run.ps1 starts
# it as a child, powershell -NoProfile -STA -ExecutionPolicy Bypass -File, with the adopted
# process's pid and start ticks, holds its handle, and reads how it ended off its exit code and
# the last line of -Notes. Its exit codes are DriverCodes in tools\loop\nw-guard.ps1, which it
# dot-sources for the window reads, the session lock and the path rule, and never copies.
#
# WHAT IT ACTS ON. Only windows of -OwnerPid, listed through EnumWindows and
# GetWindowThreadProcessId, after that process's start ticks read equal to -OwnerStartTicks
# through the handle it holds, read again before every action. The tool's window is the one
# window of that pid whose class starts HwndWrapper and whose caption starts Parsons NWC
# Federator, and its title must name -Stamp, the installed build. Every element it reads or
# presses is found under that window, or under a dialog of that pid, and its ProcessId is read
# equal to -OwnerPid first. It never searches the desktop, never moves or clicks the mouse,
# compiles nothing and writes nothing under %TEMP%.
#
# WHAT IT DOES, items 1 to 4. Selects the tab 1. Source, types -Source into SourceFolderBox and
# presses Scan. The scan is over when the window moves itself to 2. Grouping, which OnScan does
# last, and a dialog of the pid before that stops it. Back on 1. Source it records
# SourceSummary and FilesGrid's rows, on 2. Grouping GroupsGrid's rows and, where UI Automation
# answers, whether every group is ticked. It types -Nwf, -Nwd and -Excel into NwfFolderBox,
# NwdFolderBox and ExcelFolderBox on 3. Outputs and -Xml into ExchangeFileBox on 4. Clash, then
# reads every box back, each on its own tab. It presses RunButton only when each box reads
# exactly what was typed, ExchangeFileBox reading empty for a run with no -Xml, and ToleranceBox
# reads Use the value in the XML. MarkByDesign, MarkPenetrations and PriorityBox are read,
# recorded and left as the window opened them. The confirm after Run is a #32770 of the pid
# titled exactly Parsons NWC Federator whose text starts This run federates, IsConfirm in
# nw-guard.ps1, and its texts are recorded. It presses OK only when every text was read whole
# and none names a rooted path outside %LOCALAPPDATA%\NwcFederatorLoop, and Cancel otherwise.
# Any other dialog of the pid is recorded with its texts and left up, and nothing more is
# pressed.
#
# WHAT IT DOES, item 5, -OpenRun. On 4. Clash it reads OpenDocumentLine and RunOpenButton. A
# disabled button is the tool's own refusal, TOOL REFUSED, with the line's text. An enabled one
# is pressed only when ExchangeFileBox reads empty, ToleranceBox reads Use the value in the
# XML, and the line names no rooted path outside the loop folder.
#
# ONCE OK OR RUN THE OPEN FILE IS PRESSED, every way it ends is PRESSED, a fault after it
# included, with what happened written after the word. run.ps1 posts WM_CLOSE for a driver
# that stopped only when its exit says nothing that runs was pressed, so a run is never closed
# under the tool because the driver failed after starting it.
#
# THE SCREEN. Before every action it reads whether the session is locked. While it reads
# anything but unlocked it writes LOCKED, does nothing, and reads again every 30 s. The lock
# is never worked around, Q85.
#
# WHAT IT READS AND WRITES. Reads the windows of -OwnerPid and the session state. Writes
# -Notes only, a file it refuses to write outside %LOCALAPPDATA%\NwcFederatorLoop, one line per
# step and a last line DRIVER EXIT <code> <name>: <why>. Every path it types must lie under
# %LOCALAPPDATA%\NwcFederatorLoop\runs\<Set>. Starts nothing.
#
# MEASURED ON 2026-09-19, on the machine of 2026-09-19, by this script's first form, and still
# what it rests on:
# - the tool's window is owned by the Navisworks main window, so it is not a child of the
#   desktop root in the automation tree. It is found by its handle and handed to
#   AutomationElement.FromHandle
# - the confirm dialog is a #32770 titled exactly Parsons NWC Federator
# - every control is found by its x:Name, which WPF exposes as the AutomationId, and a control
#   on a tab that is not selected has no visual tree, so each tab is selected before its
#   boxes are set or read
# - SelectionItemPattern.Select on an item of the tolerance list does nothing and throws
#   nothing, measured on 2026-09-20, so this script never chooses a tolerance. It reads the box
#   and refuses unless it reads the window's own default
# - a start through Autodesk.Navisworks.Api.Automation that called ExecuteAddInPlugin saw
#   the window close by itself 8.5 s later with Window closed. and nothing else in the log, why
#   UNKNOWN. run.ps1 holds its object and disposes nothing until the run ends, and its monitor
#   records a window that closes by itself as WINDOW CLOSED BY ITSELF
# The control ids are those of src\Federator.Addin\Ui\FederatorWindow.xaml, read on
# 2026-10-01: SourceFolderBox, IncludeSubfolders, FilesGrid, GroupsGrid, NwfFolderBox,
# NwdFolderBox, ExcelFolderBox, ExchangeFileBox, ToleranceBox, PriorityBox, MarkPenetrations,
# MarkByDesign, RunButton, RunOpenButton, OpenDocumentLine, SourceSummary, the Scan button by
# its words, and the tabs 1. Source, 2. Grouping, 3. Outputs and 4. Clash. Whether each answers
# UI Automation as this script expects in the real window is UNKNOWN until the first window
# run, and a miss stops this script before anything that runs is pressed. The same ids on a
# small WPF window of the stand-in, tools\loop\StandIn, are what tools\loop\prove-run.ps1
# drives it against with no Navisworks.

$ErrorActionPreference = "Stop"
$loopRoot = Join-Path $env:LOCALAPPDATA "NwcFederatorLoop"
$nw = "C:\Program Files\Autodesk\Navisworks Manage 2025"
. (Join-Path (Split-Path $PSScriptRoot -Parent) "loop\nw-guard.ps1")
$utf8 = New-Object System.Text.UTF8Encoding($false)
$script:NotesOk = $false
$script:Pressed = ""

function Note($t) {
  $line = [DateTime]::Now.ToString("HH:mm:ss.fff") + "  " + $t
  [Console]::Out.WriteLine($line)
  if ($script:NotesOk) { [System.IO.File]::AppendAllText($Notes, $line + "`r`n", $utf8) }
}
# The one way out. Once something that runs was pressed, every end is PRESSED, with what
# happened after it in the words.
function Done($name, $text) {
  if ($script:Pressed -ne "" -and $name -ne "PRESSED") { $text = $script:Pressed + " was pressed, then " + $name + ": " + $text; $name = "PRESSED" }
  $code = DriverCode $name
  Note ("DRIVER EXIT " + $code + " " + $name + ": " + $text)
  exit $code
}
function UnderRoot($path, $root) {
  $full = $null
  try { $full = [System.IO.Path]::GetFullPath($path).TrimEnd('\') } catch { return $false }
  return $full.StartsWith(([string]$root).TrimEnd('\') + "\", [StringComparison]::OrdinalIgnoreCase)
}

# The refusals, before anything is read or written.
$notesFull = $null
try { $notesFull = [System.IO.Path]::GetFullPath($Notes) } catch { $notesFull = $null }
if ($null -eq $notesFull -or -not (UnderRoot $notesFull $loopRoot)) {
  [Console]::Out.WriteLine("DRIVER EXIT " + (DriverCode "REFUSED") + " REFUSED: -Notes " + (Mask $Notes) + " is not a file under %LOCALAPPDATA%\NwcFederatorLoop, so nothing is written and nothing is done")
  exit (DriverCode "REFUSED")
}
$Notes = $notesFull
$script:NotesOk = $true
Note ("driver started, pid " + $PID + ", owner pid " + $OwnerPid + ", start ticks UTC " + $OwnerStartTicks + ", set " + $Set + ", stamp " + $Stamp + $(if ($OpenRun) { ", the open file run" } else { "" }))
if ($Set -notmatch '^\d\d$') { Done "REFUSED" ("-Set is " + $Set + ", not two digits") }
$setRoot = Join-Path $loopRoot ("runs\" + $Set)
$boxes = [ordered]@{ "SourceFolderBox" = $Source; "NwfFolderBox" = $Nwf; "NwdFolderBox" = $Nwd; "ExcelFolderBox" = $Excel }
if ($OpenRun) {
  foreach ($k in $boxes.Keys) { if ($boxes[$k] -ne "") { Done "REFUSED" ("the open file run types no folder, and the folder for " + $k + " was given") } }
  if ($Xml -ne "") { Done "REFUSED" "the open file run types no XML, and -Xml was given" }
} else {
  foreach ($k in $boxes.Keys) {
    if ($boxes[$k] -eq "") { Done "REFUSED" ("no folder was given for " + $k) }
    if (-not (UnderRoot $boxes[$k] $setRoot)) { Done "REFUSED" ("the folder for " + $k + ", " + (Mask $boxes[$k]) + ", is not under %LOCALAPPDATA%\NwcFederatorLoop\runs\" + $Set) }
  }
  if ($Xml -ne "" -and -not (UnderRoot $Xml $setRoot)) { Done "REFUSED" ("-Xml " + (Mask $Xml) + " is not under %LOCALAPPDATA%\NwcFederatorLoop\runs\" + $Set) }
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$AE = [System.Windows.Automation.AutomationElement]
$TS = [System.Windows.Automation.TreeScope]
$wt = NewWinTypes

# The owner, held through its handle, its start ticks read equal.
$script:Owner = $null
try { $script:Owner = Get-Process -Id $OwnerPid -ErrorAction Stop; [void]$script:Owner.Handle } catch { Done "OWNER" ("pid " + $OwnerPid + " cannot be opened, " + (Err $_.Exception)) }
$h0 = HeldRead $script:Owner $OwnerStartTicks
if ($h0.State -ne "same") { Done "OWNER" ("pid " + $OwnerPid + " reads " + $h0.State + " against the start ticks given, " + $h0.Why) }

# Before every action: the owner is still the one given, and the session reads unlocked.
function Gate($what) {
  $h = HeldRead $script:Owner $OwnerStartTicks
  if ($h.State -ne "same") { Done "WINDOW GONE" ("the owner process reads " + $h.State + " before " + $what + ", " + $h.Why) }
  $lt = SessionLockText $wt.WinType
  if ($lt -eq "unlocked") { return }
  Note ("LOCKED: the session reads " + $lt + " before " + $what + ". Nothing is done, it is read again every 30 s, and the lock is never worked around, Q85")
  $sw = [Diagnostics.Stopwatch]::StartNew()
  while ($lt -ne "unlocked") {
    Start-Sleep -Seconds 30
    $h = HeldRead $script:Owner $OwnerStartTicks
    if ($h.State -ne "same") { Done "WINDOW GONE" ("the owner process reads " + $h.State + " while the session was locked, " + $h.Why) }
    $lt = SessionLockText $wt.WinType
  }
  Note ("UNLOCKED after " + $sw.Elapsed.TotalSeconds.ToString("0") + " s, so " + $what + " goes on")
}
function Mine($el, $what) {
  if ($null -eq $el) { Done "FAULT" ("no element for " + $what + " in the tool's window") }
  if ($el.Current.ProcessId -ne $OwnerPid) { Done "FAULT" ("the element for " + $what + " belongs to process " + $el.Current.ProcessId + ", not " + $OwnerPid) }
  return $el
}
function ById($id) { return (Mine ($script:Win.FindFirst($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition($AE::AutomationIdProperty, $id)))) $id) }
function ByNameAndType($parent, $name, $type) {
  $c = New-Object System.Windows.Automation.AndCondition(@(
    (New-Object System.Windows.Automation.PropertyCondition($AE::NameProperty, $name)),
    (New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $type))))
  return $parent.FindFirst($TS::Descendants, $c)
}
function TabOf($header) { return (Mine (ByNameAndType $script:Win $header ([System.Windows.Automation.ControlType]::TabItem)) ("the tab " + $header)) }
function TabShown($header) { return [bool](TabOf $header).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Current.IsSelected }
function SelectTab($header) {
  Gate ("the tab " + $header)
  $sp = (TabOf $header).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
  $sp.Select()
  Start-Sleep -Milliseconds 700
  if (-not $sp.Current.IsSelected) { Done "BOX" ("the tab " + $header + " does not read selected after Select, so nothing was pressed") }
}
function BoxText($id) { return [string]((ById $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value) }
function TypeBox($id, $text) {
  Gate ("typing into " + $id)
  (ById $id).GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($text)
  Start-Sleep -Milliseconds 400
  Note ("typed into " + $id + ": " + (MaskLine $text) + ", it reads " + (MaskLine (BoxText $id)))
}
function Toggle($id) {
  $el = ById $id
  try { return [string]$el.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState } catch { return ("UNKNOWN, " + (Err $_.Exception)) }
}
# What a combo box reads, by whichever pattern it answers, or UNKNOWN, never a guess.
function ComboText($id) {
  $combo = ById $id
  $p = $null
  if ($combo.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$p)) { return [string]$p.Current.Value }
  if ($combo.TryGetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern, [ref]$p)) {
    $sel = $p.Current.GetSelection()
    if ($sel.Length -gt 0) { return [string]$sel[0].Current.Name }
    return "UNKNOWN, nothing is selected in " + $id
  }
  return "UNKNOWN, " + $id + " answers neither the value nor the selection pattern"
}
# $runs names what the press starts, OK on the confirm or Run the open file, and is empty for a
# press that starts nothing. It is set before the press, so a press that throws still counts.
function Press($el, $what, $runs) {
  Gate ("pressing " + $what)
  $el = Mine $el $what
  if (-not $el.Current.IsEnabled) { Done "BOX" ($what + " reads disabled, so nothing was pressed") }
  if ($runs -ne "") { $script:Pressed = $runs }
  $el.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
  Note ("pressed " + $what)
}
function Rows($id) {
  $el = ById $id
  try { return [string]$el.GetCurrentPattern([System.Windows.Automation.GridPattern]::Pattern).Current.RowCount } catch { return ("UNKNOWN, " + (Err $_.Exception)) }
}
# Whether every row of GroupsGrid is ticked in its Run column, the first column, where UI
# Automation answers. UNKNOWN when a row does not, and the tool's GROUPS block is the proof
# either way.
function GroupTicks {
  $g = ById "GroupsGrid"
  try {
    $gp = $g.GetCurrentPattern([System.Windows.Automation.GridPattern]::Pattern)
    $n = $gp.Current.RowCount
    $off = 0
    for ($i = 0; $i -lt [Math]::Min($n, 200); $i++) {
      $cell = $gp.GetItem($i, 0)
      if ($null -eq $cell) { return ("UNKNOWN from row " + $i + " of " + $n + ", the cell did not answer") }
      $cb = $cell.FindFirst($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::CheckBox)))
      if ($null -eq $cb) { return ("UNKNOWN from row " + $i + " of " + $n + ", the cell holds no tick box") }
      if ([string]$cb.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -ne "On") { $off++ }
    }
    if ($n -gt 200) { return ("UNKNOWN past row 200 of " + $n + ", " + $off + " of the first 200 unticked") }
    return ([string]$n + " groups, " + $off + " unticked")
  } catch { return ("UNKNOWN, " + (Err $_.Exception)) }
}
# Every visible window of the owner whose kind is the one asked for.
function OwnerWindows($kind, [bool]$allowMessages) { return @(WindowRecords $wt.WinType $wt.ProcType ([uint32]$OwnerPid) $true $allowMessages | Where-Object { (WindowKind $_.Class $_.Caption $_.OwnerHandle $_.OwnerVisible) -eq $kind }) }
# Records every dialog of the owner but the confirm, with its texts, and stops with each left up.
# A caller stops only on a dialog read on three passes running, a second apart, so a window
# that is up for a moment, or whose texts were not read on one pass, never stops it alone.
function StopOnDialogs($dialogs, $when) {
  foreach ($d in $dialogs) { Note ("a dialog of pid " + $OwnerPid + " " + $when + ": class " + $d.Class + ", caption `"" + $d.Caption + "`", every text: " + (MaskLine (@($d.Texts) -join " "))) }
  Done "DIALOG" ([string]$dialogs.Count + " dialogs that are not the confirm are up " + $when + ", the first captioned `"" + $dialogs[0].Caption + "`", and each is left up, so nothing more was pressed")
}

try {
  # The tool's window, the one window of the owner process whose class and caption are the tool's.
  $deadline = [DateTime]::UtcNow.AddSeconds($WindowWaitSeconds)
  $tw = @()
  while ($true) {
    Gate "finding the tool's window"
    $tw = OwnerWindows "WINDOW" $false
    if ($tw.Count -eq 1) { break }
    if ($tw.Count -gt 1) { Done "NO WINDOW" ("the owner process shows " + $tw.Count + " windows of the tool, so which one to drive is UNKNOWN") }
    if ([DateTime]::UtcNow -gt $deadline) { Done "NO WINDOW" ("no window of the tool in pid " + $OwnerPid + " within " + $WindowWaitSeconds + " s") }
    Start-Sleep -Seconds 2
  }
  Note ("the tool's window, handle " + $tw[0].Handle + ", class " + $tw[0].Class + ", caption `"" + $tw[0].Caption + "`", a window of pid " + $OwnerPid)
  $tm = [regex]::Match($tw[0].Caption, '\[([^\]]*)\]\s*$')
  if (-not $tm.Success -or -not (StampNames $tm.Groups[1].Value $Stamp)) { Done "STAMP" ("the window's title does not name the installed build " + $Stamp + ", so nothing was pressed") }
  $script:Win = $AE::FromHandle($tw[0].Handle)
  if ($script:Win.Current.ProcessId -ne $OwnerPid) { Done "NO WINDOW" ("the window's automation element belongs to process " + $script:Win.Current.ProcessId) }
  Start-Sleep -Seconds 2

  $tolDefault = "Use the value in the XML"
  if (-not $OpenRun) {
    SelectTab "1. Source"
    TypeBox "SourceFolderBox" $Source
    Note ("IncludeSubfolders reads " + (Toggle "IncludeSubfolders") + ", left as the window opened it")
    Press (ByNameAndType $script:Win "Scan" ([System.Windows.Automation.ControlType]::Button)) "Scan" ""
    # OnScan moves the window to 2. Grouping as its last step, so that is when the scan is over.
    # A dialog before it is the scan's own refusal, which stops this script.
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $seen = 0
    while (-not (TabShown "2. Grouping")) {
      $d = OwnerWindows "DIALOG" $true
      if ($d.Count -gt 0) { $seen++; if ($seen -ge 3) { StopOnDialogs $d "after Scan" } } else { $seen = 0 }
      if ($sw.Elapsed.TotalSeconds -gt 300) { Done "FAULT" "the window did not move to 2. Grouping within 300 s of Scan, so the scan's end is UNKNOWN and nothing was pressed" }
      Start-Sleep -Seconds 1
      Gate "waiting for the scan"
    }
    Note ("the scan ended after " + $sw.Elapsed.TotalSeconds.ToString("0.0") + " s, the window moved itself to 2. Grouping")
    SelectTab "1. Source"
    Note ("SourceSummary reads: " + (MaskLine ([string](ById "SourceSummary").Current.Name)) + " FilesGrid rows " + (Rows "FilesGrid"))
    SelectTab "2. Grouping"
    Note ("GroupsGrid rows " + (Rows "GroupsGrid") + ", every group ticked: " + (GroupTicks) + ". The tool's GROUPS block is the proof either way")
    SelectTab "3. Outputs"
    TypeBox "NwfFolderBox" $Nwf
    TypeBox "NwdFolderBox" $Nwd
    TypeBox "ExcelFolderBox" $Excel
    SelectTab "4. Clash"
    if ($Xml -ne "") { TypeBox "ExchangeFileBox" $Xml; Start-Sleep -Seconds 2 }
    # Every box read back, each on its own tab, before anything that runs is pressed.
    $read = [ordered]@{}
    SelectTab "1. Source"
    $read["SourceFolderBox"] = BoxText "SourceFolderBox"
    SelectTab "3. Outputs"
    foreach ($k in @("NwfFolderBox", "NwdFolderBox", "ExcelFolderBox")) { $read[$k] = BoxText $k }
    SelectTab "4. Clash"
    $read["ExchangeFileBox"] = BoxText "ExchangeFileBox"
    $want = [ordered]@{ "SourceFolderBox" = $Source; "NwfFolderBox" = $Nwf; "NwdFolderBox" = $Nwd; "ExcelFolderBox" = $Excel; "ExchangeFileBox" = $Xml }
    $wrong = New-Object System.Collections.Generic.List[string]
    foreach ($k in $want.Keys) {
      Note ("read back " + $k + ": " + $(if ($read[$k] -eq "") { "empty" } else { MaskLine $read[$k] }))
      if ($read[$k] -cne $want[$k]) { $wrong.Add($k + " reads " + $(if ($read[$k] -eq "") { "empty" } else { MaskLine $read[$k] }) + " and not " + $(if ($want[$k] -eq "") { "empty" } else { MaskLine $want[$k] })) }
    }
    $tol = ComboText "ToleranceBox"
    Note ("ToleranceBox reads " + $tol + ", MarkByDesign " + (Toggle "MarkByDesign") + ", MarkPenetrations " + (Toggle "MarkPenetrations") + ", PriorityBox " + $(if ((BoxText "PriorityBox") -eq "") { "empty" } else { MaskLine (BoxText "PriorityBox") }) + ", each left as the window opened it")
    if ($wrong.Count -gt 0) { Done "BOX" (($wrong -join ", and ") + ", so nothing was pressed") }
    if ($tol -cne $tolDefault) { Done "TOLERANCE" ("ToleranceBox reads " + $tol + " and not " + $tolDefault + ", so nothing was pressed") }
    Press (ById "RunButton") "RunButton" ""
    # The confirm: a #32770 of the owner process titled exactly Parsons NWC Federator. No time
    # limit, because the window may open every NWF before it asks. A tool that hangs is the
    # monitor's to end, and then the owner reads gone here.
    $seen = 0
    while ($true) {
      Gate "waiting for the confirm"
      if ((OwnerWindows "WINDOW" $false).Count -eq 0) { Done "WINDOW GONE" "the tool's window is gone after Run was pressed and before any confirm" }
      $dlgs = OwnerWindows "DIALOG" $true
      $other = @($dlgs | Where-Object { -not (IsConfirm $_.Class $_.Caption $_.Texts) })
      if ($other.Count -gt 0) {
        $seen++
        if ($seen -ge 3) { StopOnDialogs $other "after RunButton" }
      } elseif ($dlgs.Count -gt 0) {
        $d = $dlgs[0]
        $texts = @($d.Texts)
        Note ("the confirm of pid " + $OwnerPid + ": class " + $d.Class + ", caption `"" + $d.Caption + "`", every text: " + (MaskLine ($texts -join " ")))
        $dEl = $AE::FromHandle($d.Handle)
        # A text cut at the 2048 characters WindowRecords reads, or a child it did not read, is
        # a confirm not read whole, and that is answered the way an outside path is.
        $cut = @($texts | Where-Object { ([string]$_).Length -ge 2040 -or ([string]$_) -match '^and \d+ (more )?visible children not read|^and the walk of its children stopped' })
        $outside = PathsOutside ($texts -join "`n") $loopRoot
        if ($cut.Count -gt 0 -or $outside.Count -gt 0) {
          $why = $(if ($outside.Count -gt 0) { "names a rooted path outside %LOCALAPPDATA%\NwcFederatorLoop, " + (($outside | ForEach-Object { Mask $_ }) -join ", ") } else { "could not be read whole" })
          Press (ByNameAndType $dEl "Cancel" ([System.Windows.Automation.ControlType]::Button)) "Cancel on the confirm" ""
          Done "CANCELLED" ("the confirm " + $why + ", so Cancel was pressed and nothing runs")
        }
        Press (ByNameAndType $dEl "OK" ([System.Windows.Automation.ControlType]::Button)) "OK on the confirm" "OK on the confirm"
        $gone = $false
        for ($i = 0; $i -lt 15 -and -not $gone; $i++) { Start-Sleep -Seconds 1; $gone = (@(WindowRecords $wt.WinType $wt.ProcType ([uint32]$OwnerPid) $true $false | Where-Object { $_.Handle -eq $d.Handle }).Count -eq 0) }
        Done "PRESSED" ("Run, and OK on the confirm, whose texts name no path outside the loop folder. The confirm " + $(if ($gone) { "read gone after OK" } else { "still read up 15 s after OK, a finding" }))
      } else { $seen = 0 }
      Start-Sleep -Seconds 1
    }
  }

  # Item 5, the open file run.
  SelectTab "4. Clash"
  $line = [string](ById "OpenDocumentLine").Current.Name
  $btn = ById "RunOpenButton"
  $enabled = $btn.Current.IsEnabled
  Note ("OpenDocumentLine reads: " + (MaskLine $line) + ". RunOpenButton enabled: " + $enabled)
  if (-not $enabled) { Done "TOOL REFUSED" ("Run the open file reads disabled, and OpenDocumentLine reads: " + (MaskLine $line)) }
  $x = BoxText "ExchangeFileBox"
  $tol = ComboText "ToleranceBox"
  Note ("ExchangeFileBox reads " + $(if ($x -eq "") { "empty" } else { MaskLine $x }) + ", ToleranceBox " + $tol + ", MarkByDesign " + (Toggle "MarkByDesign") + ", MarkPenetrations " + (Toggle "MarkPenetrations") + ", PriorityBox " + $(if ((BoxText "PriorityBox") -eq "") { "empty" } else { MaskLine (BoxText "PriorityBox") }) + ", each left as the window opened it")
  if ($x -ne "") { Done "BOX" ("ExchangeFileBox reads " + (MaskLine $x) + " and not empty, so nothing was pressed") }
  if ($tol -cne $tolDefault) { Done "TOLERANCE" ("ToleranceBox reads " + $tol + " and not " + $tolDefault + ", so nothing was pressed") }
  $outside = PathsOutside $line $loopRoot
  if ($outside.Count -gt 0) { Done "OPEN LINE" ("OpenDocumentLine names a rooted path outside %LOCALAPPDATA%\NwcFederatorLoop, " + (($outside | ForEach-Object { Mask $_ }) -join ", ") + ", so nothing was pressed") }
  Press $btn "RunOpenButton" "Run the open file"
  Done "PRESSED" ("Run the open file, with OpenDocumentLine reading: " + (MaskLine $line))
} catch {
  Done "FAULT" ("the driver stopped on " + (Err $_.Exception) + " at line " + $_.InvocationInfo.ScriptLineNumber + ", so nothing more was pressed")
}
