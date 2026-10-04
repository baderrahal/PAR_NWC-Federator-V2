param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F105, question 4. Does the installed API write the native Clash Detective report, the
# HTML (Tabular) page Bader exports by hand, docs\history\scan.md 4m.
#
# This tool renders that page itself from its own XML through Autodesk's stylesheet,
# clash_report_html_tabular.xsl, 4h and 4m. If a public member writes the report the way
# Clash Detective's Report tab does, the page could come from Navisworks itself. If none
# does, the stylesheet route stays the only one. The answer that would change code is such
# a member, with the report kinds and formats its enum or options offer.
#
# Reflection only over five assemblies, each by its one full path:
#   Autodesk.Navisworks.Api.dll, Autodesk.Navisworks.Clash.dll,
#   Autodesk.Navisworks.ComApi.dll, Autodesk.Navisworks.Interop.ComApi.dll,
#   Autodesk.Navisworks.Automation.dll
# Listed: every public type whose name holds Report, Html, Tabular or Export, whole, and
# every public member anywhere whose name holds one, with its full signature. In the COM
# interop, every public type whose name holds Clash or starts InwOcl, whole, because that
# is where a COM clash report would sit. Also listed: the types that are not public whose
# name holds one, names only, every string in each file holding tabular, .xsl,
# clash_report or reportformat, case blind, and where the arguments of each public
# WriteReport come from. A read that fails prints what failed and is never dropped.
# scan.md names no other Clash Detective assembly, so no other is read. Every assembly is
# loaded with ReflectionOnlyLoadFrom, which runs no code in it. Whether a member found
# runs, and what it writes, is UNKNOWN until a start measures it.

$nw = $NavisworksPath
$names = @("Autodesk.Navisworks.Api.dll", "Autodesk.Navisworks.Clash.dll", "Autodesk.Navisworks.ComApi.dll", "Autodesk.Navisworks.Interop.ComApi.dll", "Autodesk.Navisworks.Automation.dll")
$words = @("report", "html", "tabular", "export")

# TypeName, IlReason, LoaderLines, the list of reads that failed and StringRuns come from
# il-reader.ps1 beside this probe, the one copy the other two F105 probes dot-source too.
# This probe reads no IL, and uses the reader only for those shared helpers.
$reader = Join-Path $PSScriptRoot "il-reader.ps1"
if (-not (Test-Path -LiteralPath $reader)) { Write-Output ("UNKNOWN: no il-reader.ps1 at " + $reader); exit 1 }
. $reader

Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ("READER    il-reader.ps1, dot-sourced, sha256 " + (Get-FileHash -LiteralPath $reader -Algorithm SHA256).Hash)
Write-Output ("WORDS     " + ($words -join ", ") + ", case blind, in a type's name or a member's name")
Write-Output ""

$resolveFailures = New-Object System.Collections.Generic.List[string]
$roResolve = [ResolveEventHandler]{
  param($s, $e)
  $n = New-Object System.Reflection.AssemblyName($e.Name)
  $cand = Join-Path $nw ($n.Name + ".dll")
  if (Test-Path -LiteralPath $cand) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand) }
  try { return [System.Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { $line = $e.Name + ": " + (IlReason $_.Exception); if (-not $resolveFailures.Contains($line)) { $resolveFailures.Add($line) }; return $null }
}
[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($roResolve)
function ParamText($m) {
  $ps = @()
  foreach ($q in $m.GetParameters()) {
    $pre = ""; if ($q.IsOut) { $pre = "out " } elseif ($q.ParameterType.IsByRef) { $pre = "ref " }
    $opt = ""; if ($q.IsOptional) { $opt = "optional " }
    $ps += ($opt + $pre + (TypeName $q.ParameterType) + " " + $q.Name)
  }
  return ($ps -join ", ")
}
function MemberText($x) {
  switch ($x.MemberType.ToString()) {
    "Method" { $st = ""; if ($x.IsStatic) { $st = "static " }; return ("method   public " + $st + (TypeName $x.ReturnType) + " " + $x.Name + "(" + (ParamText $x) + ")") }
    "Constructor" { return ("ctor     public " + $x.DeclaringType.Name + "(" + (ParamText $x) + ")") }
    "Property" {
      $acc = @(); $g = $x.GetGetMethod(); $sm = $x.GetSetMethod(); if ($null -ne $g) { $acc += "get" }; if ($null -ne $sm) { $acc += "set" }
      $ix = @($x.GetIndexParameters()); $it = ""; if ($ix.Count -gt 0) { $it = "[" + (($ix | ForEach-Object { (TypeName $_.ParameterType) + " " + $_.Name }) -join ", ") + "]" }
      $st = ""; if ($null -ne $g -and $g.IsStatic) { $st = "static " }
      return ("property public " + $st + (TypeName $x.PropertyType) + " " + $x.Name + $it + " { " + ($acc -join "; ") + " }")
    }
    "Field" {
      if ($x.IsLiteral) { $v = ""; try { $v = " = " + [string]$x.GetRawConstantValue() } catch { $why2 = IlReason $_.Exception; $null = IlFail "value" ((TypeName $x.DeclaringType) + "." + $x.Name) -1 $why2; $v = " = UNKNOWN, the value could not be read, " + $why2 }; return ("literal  " + (TypeName $x.FieldType) + " " + $x.Name + $v) }
      $st = ""; if ($x.IsStatic) { $st = "static " }; return ("field    public " + $st + (TypeName $x.FieldType) + " " + $x.Name)
    }
    "Event" { return ("event    public " + (TypeName $x.EventHandlerType) + " " + $x.Name) }
    "NestedType" { return ("nested   " + $x.FullName) }
    default { return ($x.MemberType.ToString() + " " + $x.Name) }
  }
}
function Holds($name) { $low = $name.ToLowerInvariant(); foreach ($w in $words) { if ($low.Contains($w)) { return $true } }; return $false }
function Kind($t) { if ($t.IsEnum) { return "enum" }; if ($t.IsInterface) { return "interface" }; if ($t.IsValueType) { return "struct" }; if ($t.IsSubclassOf([System.Delegate]) -or ($null -ne $t.BaseType -and $t.BaseType.FullName -eq "System.MulticastDelegate")) { return "delegate" }; return "class" }
function ShowType($t) {
  $base = ""; if ($null -ne $t.BaseType) { $base = " : " + (TypeName $t.BaseType) }
  Write-Output ("  " + (Kind $t) + " " + (TypeName $t) + $base)
  if ($t.IsNested) { Write-Output ("      nested in " + $t.DeclaringType.FullName + ", which is public: " + ($t.DeclaringType.IsPublic -or $t.DeclaringType.IsNestedPublic)) }
  $attrs = @($t.GetCustomAttributesData() | ForEach-Object { $_.AttributeType.Name }); if ($attrs.Count -gt 0) { Write-Output ("      attributes " + ($attrs -join ", ")) }
  $ifs = @($t.GetInterfaces() | ForEach-Object { $_.Name }); if ($ifs.Count -gt 0) { Write-Output ("      implements " + ($ifs -join ", ")) }
  if ($t.IsEnum) {
    $fs = @($t.GetFields("Public,Static"))
    foreach ($f in $fs) { $v = ""; try { $v = [string]$f.GetRawConstantValue() } catch { $why2 = IlReason $_.Exception; $null = IlFail "value" ((TypeName $t) + "." + $f.Name) -1 $why2; $v = "UNKNOWN, the value could not be read, " + $why2 }; Write-Output ("      " + $f.Name + " = " + $v) }
    if ($fs.Count -eq 0) { Write-Output "      no values in the metadata" }
    return
  }
  foreach ($x in ($t.GetMembers("Public,Instance,Static,DeclaredOnly") | Sort-Object MemberType, Name)) {
    if ($x.MemberType.ToString() -eq "Method" -and $x.IsSpecialName) { continue }
    Write-Output ("      " + (MemberText $x))
  }
}

$summary = @()
$script:loadedAsm = @{}
foreach ($dllName in $names) {
  $p = Join-Path $nw $dllName
  Write-Output ("==== " + $dllName + " ====")
  if (-not (Test-Path -LiteralPath $p)) { Write-Output ("  UNKNOWN: no file at " + $p); Write-Output ""; $summary += ($dllName + ": UNKNOWN, no file"); continue }
  $fi = Get-Item -LiteralPath $p
  $a = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($p)
  Write-Output ("  FILE      " + $p + "   " + $fi.Length + " bytes, file version " + $fi.VersionInfo.FileVersion)
  Write-Output ("  ASSEMBLY  " + $a.FullName)
  $types = @()
  try { $types = @($a.GetTypes()) } catch [System.Reflection.ReflectionTypeLoadException] {
    $types = @($_.Exception.Types | Where-Object { $null -ne $_ })
    LoaderLines $dllName $_.Exception
  }
  $public = @($types | Where-Object { $_.IsPublic -or $_.IsNestedPublic })
  Write-Output ("  types " + $types.Count + ", public " + $public.Count)
  Write-Output ""

  Write-Output "  ---- every public type whose name holds a word, whole ----"
  $tHits = @($public | Where-Object { Holds $_.Name } | Sort-Object FullName)
  foreach ($t in $tHits) { ShowType $t }
  Write-Output ("  public types found: " + $tHits.Count)
  Write-Output ""

  Write-Output "  ---- every public member of any other public type whose name holds a word ----"
  $mHits = 0
  foreach ($t in ($public | Sort-Object FullName)) {
    if (Holds $t.Name) { continue }
    $ms = @(); try { $ms = @($t.GetMembers("Public,Instance,Static,DeclaredOnly")) } catch { $why2 = IlReason $_.Exception; $null = IlFail "members" $t -1 $why2; Write-Output ("  UNKNOWN: the members of " + $t.FullName + " could not be read, " + $why2); continue }
    foreach ($x in $ms) {
      if (-not (Holds $x.Name)) { continue }
      if ($x.MemberType.ToString() -eq "Method" -and $x.IsSpecialName) { continue }
      Write-Output ("  " + (TypeName $t) + "   " + (MemberText $x))
      $mHits++
    }
  }
  Write-Output ("  public members found: " + $mHits)
  Write-Output ""

  if ($dllName -eq "Autodesk.Navisworks.Interop.ComApi.dll") {
    Write-Output "  ---- every public COM type whose name holds Clash or starts InwOcl, whole ----"
    $cHits = @($public | Where-Object { $_.Name.ToLowerInvariant().Contains("clash") -or $_.Name.StartsWith("InwOcl") } | Sort-Object FullName)
    foreach ($t in $cHits) { ShowType $t }
    Write-Output ("  clash COM types found: " + $cHits.Count)
    Write-Output ""
  }
  Write-Output "  ---- every type that is NOT public whose name holds a word, names only, for the record ----"
  $np = @($types | Where-Object { -not ($_.IsPublic -or $_.IsNestedPublic) -and (Holds $_.Name) } | Sort-Object FullName)
  foreach ($t in $np) { Write-Output ("  " + (Kind $t) + " " + $t.FullName) }
  Write-Output ("  not public types found: " + $np.Count)
  Write-Output ""

  # The file as bytes: every ASCII or UTF-16 string holding one of four words, tabular,
  # .xsl, clash_report or reportformat, case blind, because a native report writer reaches
  # a stylesheet, a tabular report, a clash report or a report format by name. Each string
  # is printed with the words it holds, and each word is counted on its own, so a count of
  # strings is never read as a count of one of the four.
  Write-Output "  ---- every string in the file holding tabular, .xsl, clash_report or reportformat, case blind ----"
  # The strings come from StringRuns in il-reader.ps1, the one copy of the two patterns.
  $sWords = @("tabular", ".xsl", "clash_report", "reportformat")
  $perWord = [ordered]@{}; foreach ($w in $sWords) { $perWord[$w] = 0 }
  $sHits = 0
  foreach ($run in (StringRuns ([System.IO.File]::ReadAllBytes($p)))) {
    $which = @(); foreach ($w in $sWords) { if ($run.Text.IndexOf($w, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $which += $w; $perWord[$w]++ } }
    if ($which.Count -gt 0) { Write-Output ("  offset " + $run.Offset.ToString().PadLeft(8) + "  " + $run.Enc + "  " + $run.Text + "   [" + ($which -join ", ") + "]"); $sHits++ }
  }
  $wordText = (($sWords | ForEach-Object { $_ + " " + $perWord[$_] }) -join ", ")
  Write-Output ("  strings found: " + $sHits + ". Strings holding each word: " + $wordText)
  Write-Output ""

  $summary += ($dllName + ": public types holding a word " + $tHits.Count + ", other public members holding a word " + $mHits + ", not public types holding a word " + $np.Count + ", strings holding " + $wordText)
  $script:loadedAsm[$dllName] = $a
}

# Where each parameter of a WriteReport comes from. A member is only usable if its
# arguments can be had, so each parameter type is shown whole, with every public member in
# the five assemblies that hands one out.
Write-Output "==== WHERE THE ARGUMENTS OF EVERY PUBLIC WriteReport COME FROM ===="
$allPublic = @()
foreach ($k in $script:loadedAsm.Keys) { try { $allPublic += @($script:loadedAsm[$k].GetTypes() | Where-Object { $_.IsPublic -or $_.IsNestedPublic }) } catch [System.Reflection.ReflectionTypeLoadException] { $allPublic += @($_.Exception.Types | Where-Object { $null -ne $_ -and ($_.IsPublic -or $_.IsNestedPublic) }); LoaderLines $k $_.Exception } }
$writers = @()
foreach ($t in $allPublic) { foreach ($m in $t.GetMethods("Public,Instance,Static,DeclaredOnly")) { if ($m.Name -eq "WriteReport") { $writers += $m } } }
Write-Output ("  public methods named WriteReport: " + $writers.Count)
foreach ($w in $writers) {
  Write-Output ("  " + (TypeName $w.DeclaringType) + "   " + (MemberText $w) + "   in " + $w.Module.Name)
  foreach ($q in $w.GetParameters()) {
    $pt = $q.ParameterType; if ($pt.IsByRef) { $pt = $pt.GetElementType() }
    if ($pt.Namespace -eq "System") { Write-Output ("    parameter " + $q.Name + ": " + (TypeName $pt)); continue }
    Write-Output ("    parameter " + $q.Name + ": " + (TypeName $pt) + ", in " + $pt.Module.Name)
    foreach ($c in $pt.GetConstructors()) { Write-Output ("      " + (MemberText $c)) }
    $givers = 0
    foreach ($t in $allPublic) {
      $ms = @(); try { $ms = @($t.GetMembers("Public,Instance,Static,DeclaredOnly")) } catch { $why2 = IlReason $_.Exception; $null = IlFail "members" $t -1 $why2; Write-Output ("      UNKNOWN: the members of " + $t.FullName + " could not be read, so it is not known whether it hands one out, " + $why2); continue }
      foreach ($x in $ms) {
        $gives = $false
        if ($x.MemberType.ToString() -eq "Method") { if ($x.ReturnType -eq $pt) { $gives = $true }; foreach ($pp in $x.GetParameters()) { if ($pp.IsOut -and $pp.ParameterType.GetElementType() -eq $pt) { $gives = $true } } }
        elseif ($x.MemberType.ToString() -eq "Property") { if ($x.PropertyType -eq $pt) { $gives = $true } }
        elseif ($x.MemberType.ToString() -eq "Field") { if ($x.FieldType -eq $pt) { $gives = $true } }
        if ($gives -and -not ($x.MemberType.ToString() -eq "Method" -and $x.IsSpecialName)) { Write-Output ("      handed out by " + (TypeName $t) + "   " + (MemberText $x)); $givers++ }
      }
    }
    Write-Output ("      members handing one out: " + $givers)
  }
}
Write-Output ""

Write-Output "==== SUMMARY ===="
foreach ($s in $summary) { Write-Output ("  " + $s) }
if ($resolveFailures.Count -eq 0) { Write-Output "  references that could not be loaded for reflection: none" } else { Write-Output ("  references that could not be loaded for reflection: " + $resolveFailures.Count); foreach ($x in $resolveFailures) { Write-Output ("    " + $x) } }
Write-Output "  what il-reader.ps1 kept over the whole run, each once, by kind:"
IlFailureLines "    "
Write-Output ("  reads that failed, all kinds together: " + ($IlFailures.Count + $resolveFailures.Count))
Write-Output ""
Write-Output "WHETHER ANY MEMBER LISTED RUNS, AND WHAT IT WRITES, IS UNKNOWN. Nothing here started Navisworks or called a member."
