param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F72c, scan.md 5h. Can a comment be written on a clash result.
#
# F72c wants the NWF itself to carry WHY a clash was moved to Reviewed, as a comment on
# the clash, so a person in the Clash Detective panel next week sees the reason without
# a log, and wants an undo that touches only the clashes carrying that record. If no
# comment can be written the tool says so in one line and sets the status alone.
#
# What is read: every member on ClashResult, IClashResult, ClashResultGroup and ClashTest
# whose name holds Comment, Note, Description, Tag, UserName, Status or Approved. Then
# every member on DocumentClashTests whose name holds Comment, because every mutator on
# that type is a copy form and a comment edit will be one too. Then SavedItem.Comments and
# the Comment type in the Api DLL, whole. Whether a comment SURVIVES a save and a reopen
# cannot be read off a DLL and waits for a run.

$nw = $NavisworksPath
$api = Join-Path $nw "Autodesk.Navisworks.Api.dll"
$clash = Join-Path $nw "Autodesk.Navisworks.Clash.dll"
if (-not (Test-Path $api)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Api.dll at $api"; exit 1 }
if (-not (Test-Path $clash)) { Write-Output "UNKNOWN: no Autodesk.Navisworks.Clash.dll at $clash"; exit 1 }

$apiAsm = [System.Reflection.Assembly]::LoadFrom($api)
$clashAsm = [System.Reflection.Assembly]::LoadFrom($clash)
Write-Output ("ASSEMBLY  " + $apiAsm.GetName().Name + " " + $apiAsm.GetName().Version)
Write-Output ("ASSEMBLY  " + $clashAsm.GetName().Name + " " + $clashAsm.GetName().Version)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

$all = "Public,NonPublic,Instance,Static,DeclaredOnly"
$public = "Public,Instance,Static,DeclaredOnly"

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

function Show($m) {
  $vis = Vis $m
  switch ($m.MemberType) {
    "Method" {
      if ($m.IsSpecialName) { return }
      $ps = @()
      foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
      Write-Output ("  " + $vis + " " + $m.ReturnType.Name + " " + $m.Name + "(" + ($ps -join ", ") + ")")
    }
    "Property" {
      Write-Output ("  " + $vis + " " + $m.PropertyType.Name + " " + $m.Name + "  canread=" + $m.CanRead + " canwrite=" + $m.CanWrite)
    }
    "Constructor" {
      $ps = @()
      foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
      Write-Output ("  " + $vis + " ctor(" + ($ps -join ", ") + ")")
    }
    "Field" {
      Write-Output ("  " + $vis + " field " + $m.FieldType.Name + " " + $m.Name)
    }
    default { }
  }
}

function Named($asm, $fullName, $words) {
  $ty = $asm.GetType($fullName)
  Write-Output ("---- " + $fullName + ", members whose name holds " + ($words -join ", ") + " ----")
  if ($null -eq $ty) { Write-Output "  UNKNOWN: no such type"; Write-Output ""; return }
  $base = $ty.BaseType
  if ($null -ne $base) { Write-Output ("  base " + $base.FullName) }
  $hits = 0
  foreach ($m in $ty.GetMembers($all)) {
    $low = $m.Name.ToLowerInvariant()
    $hit = $false
    foreach ($w in $words) { if ($low.Contains($w.ToLowerInvariant())) { $hit = $true; break } }
    if (-not $hit) { continue }
    Show $m
    $hits++
  }
  if ($hits -eq 0) { Write-Output "  none, public or not, declared on this type" }
  Write-Output ""
}

function Every($asm, $fullName) {
  $ty = $asm.GetType($fullName)
  Write-Output ("---- " + $fullName + ", every public member ----")
  if ($null -eq $ty) { Write-Output "  UNKNOWN: no such type"; Write-Output ""; return }
  $base = $ty.BaseType
  if ($null -ne $base) { Write-Output ("  base " + $base.FullName) }
  foreach ($m in $ty.GetMembers($public)) { Show $m }
  Write-Output ""
}

$words = @("Comment", "Note", "Description", "Tag", "UserName", "Status", "Approved")
Named $clashAsm "Autodesk.Navisworks.Api.Clash.ClashResult" $words
Named $clashAsm "Autodesk.Navisworks.Api.Clash.IClashResult" $words
Named $clashAsm "Autodesk.Navisworks.Api.Clash.ClashResultGroup" $words
Named $clashAsm "Autodesk.Navisworks.Api.Clash.ClashTest" $words
Named $clashAsm "Autodesk.Navisworks.Api.Clash.DocumentClashTests" @("Comment", "Note", "Status", "Approved", "Assign")

Write-Output "---- SavedItem, the members a clash result inherits, whole ----"
Every $apiAsm "Autodesk.Navisworks.Api.SavedItem"

Every $apiAsm "Autodesk.Navisworks.Api.Comment"
Every $apiAsm "Autodesk.Navisworks.Api.CommentCollection"
Every $apiAsm "Autodesk.Navisworks.Api.CommentStatus"

Write-Output "---- CommentStatus, every value ----"
$cs = $apiAsm.GetType("Autodesk.Navisworks.Api.CommentStatus")
if ($null -eq $cs) { Write-Output "  UNKNOWN: no CommentStatus" } else {
  foreach ($n in [Enum]::GetNames($cs)) { Write-Output ("  {0} = {1}" -f $n, [int]([Enum]::Parse($cs, $n))) }
}
Write-Output ""

Write-Output "---- Every type in either assembly whose name holds Comment, with its public members ----"
foreach ($pair in @(@($apiAsm, "Api"), @($clashAsm, "Clash"))) {
  $a = $pair[0]
  foreach ($ty in $a.GetTypes()) {
    if (-not $ty.Name.ToLowerInvariant().Contains("comment")) { continue }
    if ($ty.FullName -eq "Autodesk.Navisworks.Api.Comment" -or $ty.FullName -eq "Autodesk.Navisworks.Api.CommentCollection" -or $ty.FullName -eq "Autodesk.Navisworks.Api.CommentStatus") { continue }
    Every $a $ty.FullName
  }
}

Write-Output "---- Every member in either assembly, public or not, whose name holds Comment and is a METHOD that takes a SavedItem, a Comment or a clash result ----"
$hits = 0
foreach ($a in @($apiAsm, $clashAsm)) {
  foreach ($ty in $a.GetTypes()) {
    foreach ($m in $ty.GetMethods($all)) {
      if (-not $m.Name.ToLowerInvariant().Contains("comment")) { continue }
      $ps = @()
      foreach ($q in $m.GetParameters()) { $ps += ($q.ParameterType.Name + " " + $q.Name) }
      Write-Output ("  " + (Vis $m) + " " + $ty.FullName + "." + $m.Name + "(" + ($ps -join ", ") + ") : " + $m.ReturnType.Name)
      $hits++
    }
  }
}
if ($hits -eq 0) { Write-Output "  none" }
Write-Output ""
