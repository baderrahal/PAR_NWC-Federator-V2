param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F74, scan.md 5e. Does anything on the API say an opened document has finished loading.
#
# On the first real run all five existing NWFs reported 0 unchanged, 4 added, 0 removed
# and were rebuilt, and DECIDE finished in 0.248 seconds against 2.3 to 4.8 seconds for
# every open in the one earlier log on record. So Document.TryOpenFile returning true does
# not mean Document.Models is filled. Federator.Core.Rerun.ModelLoadWait polls the count
# as the fallback. If a member on the API answers "the models are all in now", the add-in
# reads that member instead and the poll becomes the fallback.
#
# What is read: every member on Document, DocumentModels and Application whose name holds
# Load, Ready, Busy, Progress, State, Pending or Complete, public or not, and EVERY event
# on each, whatever it is called. Nothing is instantiated, because the API needs the
# application host for that, and this probe runs without Navisworks open.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }

$asm = [System.Reflection.Assembly]::LoadFrom($api)
Write-Output ("ASSEMBLY  " + $asm.GetName().Name + " " + $asm.GetName().Version)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

$words = @("load", "ready", "busy", "progress", "state", "pending", "complete")
$flags = "Public,NonPublic,Instance,Static,DeclaredOnly"

function Vis($m) {
  if ($m.MemberType -eq "Property") {
    $g = $m.GetGetMethod($true)
    if ($null -ne $g) { return (Vis $g) }
    $s = $m.GetSetMethod($true)
    if ($null -ne $s) { return (Vis $s) }
    return "UNKNOWN"
  }
  if ($m.MemberType -eq "Event") {
    $a = $m.GetAddMethod($true)
    if ($null -ne $a) { return (Vis $a) }
    return "UNKNOWN"
  }
  if ($m.IsPublic) { return "public" }
  if ($m.IsFamily) { return "protected" }
  if ($m.IsAssembly) { return "internal" }
  return "private"
}

function Show($ty, $m) {
  $vis = Vis $m
  switch ($m.MemberType) {
    "Method" {
      $ps = @()
      foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
      Write-Output ("  " + $vis + " " + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
    }
    "Property" {
      Write-Output ("  " + $vis + " " + $m.PropertyType.Name + " " + $m.Name + "  canread=" + $m.CanRead + " canwrite=" + $m.CanWrite)
    }
    "Event" {
      Write-Output ("  " + $vis + " event " + $m.EventHandlerType.Name + " " + $m.Name)
    }
    "Field" {
      Write-Output ("  " + $vis + " field " + $m.FieldType.Name + " " + $m.Name)
    }
    default {
      Write-Output ("  " + $vis + " " + $m.MemberType + " " + $m.Name)
    }
  }
}

function Probe($fullName) {
  $ty = $asm.GetType($fullName)
  if ($null -eq $ty) { Write-Output ("---- " + $fullName + " ----"); Write-Output "  UNKNOWN: no such type in this assembly"; Write-Output ""; return }

  Write-Output ("---- " + $ty.FullName + ", members whose name holds load, ready, busy, progress, state, pending or complete ----")
  $hits = 0
  foreach ($m in $ty.GetMembers($flags)) {
    $low = $m.Name.ToLowerInvariant()
    $hit = $false
    foreach ($w in $words) { if ($low.Contains($w)) { $hit = $true; break } }
    if (-not $hit) { continue }
    Show $ty $m
    $hits++
  }
  if ($hits -eq 0) { Write-Output "  none, public or not" }
  Write-Output ""

  Write-Output ("---- " + $ty.FullName + ", every event ----")
  $events = 0
  foreach ($e in $ty.GetEvents($flags)) { Show $ty $e; $events++ }
  if ($events -eq 0) { Write-Output "  none, public or not" }
  Write-Output ""
}

Write-Output "---- Document.Models, what it returns ----"
$doc = $asm.GetType("Autodesk.Navisworks.Api.Document")
$models = $doc.GetProperty("Models")
if ($null -eq $models) { Write-Output "  UNKNOWN: no Models property on Document" } else {
  Write-Output ("  " + $models.PropertyType.FullName + " Models  canread=" + $models.CanRead + " canwrite=" + $models.CanWrite)
}
Write-Output ""

Probe "Autodesk.Navisworks.Api.Document"
Probe "Autodesk.Navisworks.Api.DocumentParts.DocumentModels"
Probe "Autodesk.Navisworks.Api.Application"

Write-Output "---- Every OTHER type in the assembly with an event whose name holds load, ready, busy, progress, pending or complete ----"
$others = 0
foreach ($ty in $asm.GetTypes()) {
  if ($ty.FullName -eq "Autodesk.Navisworks.Api.Document" -or $ty.FullName -eq "Autodesk.Navisworks.Api.DocumentParts.DocumentModels" -or $ty.FullName -eq "Autodesk.Navisworks.Api.Application") { continue }
  foreach ($e in $ty.GetEvents($flags)) {
    $low = $e.Name.ToLowerInvariant()
    foreach ($w in @("load", "ready", "busy", "progress", "pending", "complete")) {
      if ($low.Contains($w)) { Write-Output ("  " + $ty.FullName + "." + $e.Name + "  (" + $e.EventHandlerType.Name + ")"); $others++; break }
    }
  }
}
if ($others -eq 0) { Write-Output "  none" }
Write-Output ""

Write-Output "---- The Progress type, if there is one, every public member ----"
$found = $false
foreach ($ty in $asm.GetTypes()) {
  if ($ty.Name -ne "Progress" -and $ty.Name -ne "ProgressReporter") { continue }
  $found = $true
  Write-Output ("  " + $ty.FullName)
  foreach ($m in $ty.GetMembers("Public,Instance,Static,DeclaredOnly")) { Show $ty $m }
}
if (-not $found) { Write-Output "  none named Progress or ProgressReporter" }
Write-Output ""
