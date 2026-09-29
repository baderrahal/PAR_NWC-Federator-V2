param([string]$NavisworksPath = "C:\Program Files\Autodesk\Navisworks Manage 2025")
$ErrorActionPreference = "Stop"

# F105, question 3. Which command line switches do Roamer.exe and the assembly that parses
# its command line carry, read off the files and NEVER by running Roamer.exe.
#
# docs\history\scan.md 5z-d read 37 option names off CommandLineParser's IL, among them
# NoGui, OpenFile, AddPluginAssembly, ExecuteAddInPlugin and Exit, and said what Roamer.exe
# does with them is UNKNOWN. The answer that would change code is a switch that runs an
# add-in plugin on an ordinary start, because the tool's window could then open with no
# click. This probe reads what the binaries CARRY and what the parser STORES for each
# option. What a start then does with it is not readable here and stays UNKNOWN.
#
# How:
#   1. Roamer.exe as bytes: every ASCII and UTF-16 string of four or more printable
#      characters, with its offset. Listed are the ones that look like a switch, a word
#      of letters after a hyphen or a slash, and every one holding a key word
#   2. Roamer.exe's PE headers, import table and delay import table, by bytes. A managed
#      exe imports its assemblies through the CLI metadata and not the PE import table, so
#      its AssemblyRef, ModuleRef and ImplMap tables are read by bytes too, and checked
#      against a reflection only read of the same file
#   3. every referenced assembly that sits in the install folder, each by its one full
#      path built from the reference name, counted for the switch names
#   4. the one that holds the switch table, listed the same way as Roamer.exe
#   5. what its parser stores for each option, off the IL, reflection only, and which
#      methods in it and in Roamer.exe read the fields the add-in plugin options store
#
# Nothing here starts a process. Reflection only loads run no code in the assemblies.
# Each file is tested by its one full path. The install folder is never searched.

$nw = $NavisworksPath
$roamer = Join-Path $nw "Roamer.exe"
if (-not (Test-Path -LiteralPath $roamer)) { Write-Output ("UNKNOWN: no Roamer.exe at " + $roamer); exit 1 }

$latin1 = [System.Text.Encoding]::GetEncoding(28591)
$keyWords = @("ExecuteAddInPlugin", "AddIn", "NoGui", "Embedding", "regserver", "OpenFile", "log", "lang", "options", "dump", "memory")
$switchNames = @("ExecuteAddInPlugin", "AddPluginAssembly", "NoGui", "Embedding", "regserver", "OpenFile")

function FileLine($p) {
  $fi = Get-Item -LiteralPath $p
  $sha = (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash
  return ("FILE      " + $p + "   " + $fi.Length + " bytes, file version " + $fi.VersionInfo.FileVersion + ", sha256 " + $sha)
}
Write-Output (FileLine $roamer)
Write-Output ("MACHINE   " + $env:COMPUTERNAME + "   " + (Get-Date -Format "yyyy-MM-dd HH:mm"))
Write-Output ""

# Every run of printable characters, ASCII one byte each or UTF-16 LE two bytes each. The
# file is mapped one byte to one char through Latin-1, so a regex index IS the byte offset.
function StringsOf([byte[]]$bytes) {
  $text = $latin1.GetString($bytes)
  $list = New-Object System.Collections.Generic.List[object]
  foreach ($m in [regex]::Matches($text, "[\x20-\x7E]{4,}")) { $list.Add([pscustomobject]@{ Offset = $m.Index; Enc = "ascii "; Text = $m.Value }) }
  foreach ($m in [regex]::Matches($text, "(?:[\x20-\x7E]\x00){4,}")) { $list.Add([pscustomobject]@{ Offset = $m.Index; Enc = "utf16 "; Text = $m.Value.Replace([string][char]0, "") }) }
  return ,$list
}
function OffText($o) { return ("offset " + $o.ToString().PadLeft(8) + " 0x" + $o.ToString("X6")) }

function ListStrings($label, [byte[]]$bytes) {
  $all = StringsOf $bytes
  Write-Output ("  strings of four or more printable characters in " + $label + ": " + $all.Count + ", ascii " + @($all | Where-Object { $_.Enc -eq "ascii " }).Count + ", utf16 " + @($all | Where-Object { $_.Enc -eq "utf16 " }).Count)
  Write-Output ""
  Write-Output ("  -- " + $label + ": every string that IS a switch, a hyphen or a slash and then a word of letters --")
  $whole = @($all | Where-Object { $_.Text -cmatch '^[-/][A-Za-z]+$' } | Sort-Object Offset)
  foreach ($s in $whole) { Write-Output ("    " + (OffText $s.Offset) + "  " + $s.Enc + $s.Text) }
  Write-Output ("    count " + $whole.Count)
  Write-Output ""
  Write-Output ("  -- " + $label + ": every other string holding a word of letters after a hyphen or a slash, at its start or after a space, quote or bracket --")
  $inside = @($all | Where-Object { $_.Text -notmatch '^[-/][A-Za-z]+$' -and $_.Text -match '(^|[\s"''(\[])[-/][A-Za-z]{2,}($|[\s"''=:)\],])' } | Sort-Object Offset)
  foreach ($s in $inside) { Write-Output ("    " + (OffText $s.Offset) + "  " + $s.Enc + $s.Text) }
  Write-Output ("    count " + $inside.Count)
  Write-Output ""
  Write-Output ("  -- " + $label + ": every string holding a key word, case blind: " + ($keyWords -join ", ") + " --")
  $per = @{}; foreach ($k in $keyWords) { $per[$k] = 0 }
  $hits = New-Object System.Collections.Generic.List[string]
  foreach ($s in ($all | Sort-Object Offset)) {
    $which = @(); foreach ($k in $keyWords) { if ($s.Text.IndexOf($k, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $which += $k; $per[$k]++ } }
    if ($which.Count -gt 0) { $hits.Add("    " + (OffText $s.Offset) + "  " + $s.Enc + $s.Text + "   [" + ($which -join ", ") + "]") }
  }
  foreach ($k in $keyWords) { Write-Output ("    strings holding " + $k.PadRight(19) + ": " + $per[$k]) }
  foreach ($h in $hits) { Write-Output $h }
  Write-Output ("    count " + $hits.Count)
  Write-Output ""
}

function SwitchCounts([byte[]]$bytes) {
  $text = $latin1.GetString($bytes)
  $r = [ordered]@{}
  foreach ($w in $switchNames) {
    $u16 = (($w.ToCharArray() | ForEach-Object { [regex]::Escape([string]$_) }) -join "\x00") + "\x00"
    $r[$w] = @([regex]::Matches($text, [regex]::Escape($w)).Count, [regex]::Matches($text, $u16).Count)
  }
  return $r
}
function HoldsTable($counts) {
  $n = 0; foreach ($w in @("ExecuteAddInPlugin", "NoGui", "Embedding", "regserver")) { if (($counts[$w][0] + $counts[$w][1]) -gt 0) { $n++ } }
  return ($n -eq 4)
}

# ---------------------------------------------------------------------------------------
Write-Output "==== 1. Roamer.exe as bytes ===="
$rb = [System.IO.File]::ReadAllBytes($roamer)
ListStrings "Roamer.exe" $rb
$rc = SwitchCounts $rb
Write-Output "  -- Roamer.exe: the switch names counted exactly, ascii and utf16 --"
foreach ($w in $switchNames) { Write-Output ("    " + $w.PadRight(19) + " ascii " + $rc[$w][0] + "   utf16 " + $rc[$w][1]) }
$roamerHolds = HoldsTable $rc
Write-Output ("  Roamer.exe holds the switch table, ExecuteAddInPlugin, NoGui, Embedding and regserver all present: " + $roamerHolds)
Write-Output ""

# ---------------------------------------------------------------------------------------
# The PE reader of probe-automation-start.ps1, the same functions.
function PeRead($file) {
  $b = [System.IO.File]::ReadAllBytes($file)
  $pe = [BitConverter]::ToInt32($b, 0x3C)
  $nsec = [BitConverter]::ToUInt16($b, $pe + 6)
  $optSize = [BitConverter]::ToUInt16($b, $pe + 20)
  $opt = $pe + 24
  $magic = [BitConverter]::ToUInt16($b, $opt)
  if ($magic -eq 0x20B) { $dd = $opt + 112; $thunk = 8 } else { $dd = $opt + 96; $thunk = 4 }
  $secs = New-Object System.Collections.Generic.List[object]
  for ($i = 0; $i -lt $nsec; $i++) { $s = $opt + $optSize + 40 * $i; $secs.Add(@([BitConverter]::ToUInt32($b, $s + 12), [BitConverter]::ToUInt32($b, $s + 8), [BitConverter]::ToUInt32($b, $s + 20), [BitConverter]::ToUInt32($b, $s + 16), [System.Text.Encoding]::ASCII.GetString($b, $s, 8).TrimEnd([char]0))) }
  return [pscustomobject]@{ Bytes = $b; Pe = $pe; Opt = $opt; Magic = $magic; Dd = $dd; Thunk = $thunk; Secs = $secs; Machine = [BitConverter]::ToUInt16($b, $pe + 4) }
}
function PeOff($pe, [uint64]$rva) {
  foreach ($s in $pe.Secs) { $size = [Math]::Max([uint64]$s[1], [uint64]$s[3]); if ($rva -ge $s[0] -and $rva -lt ($s[0] + $size)) { return [int]($rva - $s[0] + $s[2]) } }
  throw ("RVA 0x" + $rva.ToString("X") + " is in no section")
}
function PeStr($pe, [int]$o) { $e = $o; while ($pe.Bytes[$e] -ne 0) { $e++ }; return [System.Text.Encoding]::ASCII.GetString($pe.Bytes, $o, $e - $o) }
function ThunkNames($pe, [int]$t) {
  $b = $pe.Bytes; $names = @()
  while ($true) {
    if ($pe.Thunk -eq 8) { $v = [BitConverter]::ToUInt64($b, $t); $byOrd = (($v -shr 63) -eq 1) } else { $v = [uint64][BitConverter]::ToUInt32($b, $t); $byOrd = (($v -shr 31) -eq 1) }
    if ($v -eq 0) { break }
    if ($byOrd) { $names += ("#" + [int]($v -band 0xFFFF)) } else { $names += (PeStr $pe ((PeOff $pe ($v -band 0x7FFFFFFF)) + 2)) }
    $t += $pe.Thunk
  }
  return $names
}

Write-Output "==== 2. Roamer.exe's PE headers and import tables, by bytes ===="
$pe = PeRead $roamer
Write-Output ("  machine 0x" + $pe.Machine.ToString("X4") + ", optional header magic 0x" + $pe.Magic.ToString("X3") + $(if ($pe.Magic -eq 0x20B) { " PE32+" } else { " PE32" }) + ", subsystem " + [BitConverter]::ToUInt16($pe.Bytes, $pe.Opt + 68))
Write-Output ("  sections: " + (($pe.Secs | ForEach-Object { $_[4] }) -join ", "))
$ddNames = @{ 1 = "import"; 13 = "delay import"; 14 = "CLI header" }
foreach ($i in @(1, 13, 14)) { Write-Output ("  data directory " + $i + ", " + $ddNames[$i] + ": rva 0x" + [BitConverter]::ToUInt32($pe.Bytes, $pe.Dd + 8 * $i).ToString("X") + ", size " + [BitConverter]::ToUInt32($pe.Bytes, $pe.Dd + 8 * $i + 4)) }

Write-Output "  -- the PE import table --"
$impRva = [BitConverter]::ToUInt32($pe.Bytes, $pe.Dd + 8)
if ($impRva -eq 0) { Write-Output "    none" } else {
  $o = PeOff $pe $impRva
  while ($true) {
    $oft = [BitConverter]::ToUInt32($pe.Bytes, $o); $nameRva = [BitConverter]::ToUInt32($pe.Bytes, $o + 12); $ft = [BitConverter]::ToUInt32($pe.Bytes, $o + 16)
    if ($nameRva -eq 0) { break }
    $dll = PeStr $pe (PeOff $pe $nameRva)
    if ($oft -ne 0) { $t = PeOff $pe $oft } else { $t = PeOff $pe $ft }
    $fn = @(ThunkNames $pe $t)
    Write-Output ("    " + $dll + ", " + $fn.Count + ": " + ($fn -join ", "))
    $o += 20
  }
}
Write-Output "  -- the delay import table --"
$dlRva = [BitConverter]::ToUInt32($pe.Bytes, $pe.Dd + 8 * 13)
if ($dlRva -eq 0) { Write-Output "    none" } else {
  $o = PeOff $pe $dlRva
  while ($true) {
    $attr = [BitConverter]::ToUInt32($pe.Bytes, $o); $nameRva = [BitConverter]::ToUInt32($pe.Bytes, $o + 4); $intRva = [BitConverter]::ToUInt32($pe.Bytes, $o + 16)
    if ($nameRva -eq 0) { break }
    if (($attr -band 1) -eq 0) { Write-Output "    UNKNOWN: a delay descriptor uses the old VA form, not read"; break }
    $dll = PeStr $pe (PeOff $pe $nameRva)
    $fn = @(); if ($intRva -ne 0) { $fn = @(ThunkNames $pe (PeOff $pe $intRva)) }
    Write-Output ("    " + $dll + ", " + $fn.Count + ": " + ($fn -join ", "))
    $o += 32
  }
}

# The CLI metadata, by bytes: ECMA-335 II.24. Row sizes follow from the row counts and the
# heap size flags, so every table before the ones read has its size worked out, in order.
function MdRead($pe) {
  $b = $pe.Bytes
  $cliRva = [BitConverter]::ToUInt32($b, $pe.Dd + 8 * 14)
  if ($cliRva -eq 0) { return $null }
  $cli = PeOff $pe $cliRva
  $m = PeOff $pe ([BitConverter]::ToUInt32($b, $cli + 8))
  if ([BitConverter]::ToUInt32($b, $m) -ne 0x424A5342) { throw "no BSJB signature at the metadata root" }
  $vlen = [BitConverter]::ToUInt32($b, $m + 12)
  $ver = [System.Text.Encoding]::ASCII.GetString($b, $m + 16, $vlen).TrimEnd([char]0)
  $p = $m + 16 + $vlen
  $ns = [BitConverter]::ToUInt16($b, $p + 2); $p += 4
  $streams = @{}
  for ($i = 0; $i -lt $ns; $i++) {
    $so = [BitConverter]::ToUInt32($b, $p); $ss = [BitConverter]::ToUInt32($b, $p + 4); $p += 8
    $e = $p; while ($b[$e] -ne 0) { $e++ }
    $name = [System.Text.Encoding]::ASCII.GetString($b, $p, $e - $p)
    $p += ((($e - $p + 1) + 3) -band (-bnot 3))
    $streams[$name] = @(($m + $so), $ss)
  }
  $tk = $streams["#~"]; if ($null -eq $tk) { $tk = $streams["#-"] }
  if ($null -eq $tk) { throw "no #~ or #- stream" }
  $tp = $tk[0]
  $heap = $b[$tp + 6]
  $lo = [long][BitConverter]::ToUInt32($b, $tp + 8); $hi = [long][BitConverter]::ToUInt32($b, $tp + 12)
  $rows = New-Object long[] 64
  $q = $tp + 24
  for ($i = 0; $i -lt 64; $i++) {
    if ($i -lt 32) { $bit = ($lo -shr $i) -band 1 } else { $bit = ($hi -shr ($i - 32)) -band 1 }
    if ($bit -eq 1) { $rows[$i] = [long][BitConverter]::ToUInt32($b, $q); $q += 4 }
  }
  if (($heap -band 0x40) -ne 0) { $q += 4 }
  $strSz = 2; if (($heap -band 1) -ne 0) { $strSz = 4 }
  $guidSz = 2; if (($heap -band 2) -ne 0) { $guidSz = 4 }
  $blobSz = 2; if (($heap -band 4) -ne 0) { $blobSz = 4 }
  $ix = { param($t) if ($rows[$t] -lt 65536) { 2 } else { 4 } }
  $cd = { param($ts, $bits) $max = 0; foreach ($t in $ts) { if ($rows[$t] -gt $max) { $max = $rows[$t] } }; if ($max -lt (1 -shl (16 - $bits))) { 2 } else { 4 } }
  $TDOR = & $cd @(0x02, 0x01, 0x1B) 2
  $HC = & $cd @(0x04, 0x08, 0x17) 2
  $HCA = & $cd @(0x06, 0x04, 0x01, 0x02, 0x08, 0x09, 0x0A, 0x00, 0x0E, 0x17, 0x14, 0x11, 0x1A, 0x1B, 0x20, 0x23, 0x26, 0x27, 0x28, 0x2A, 0x2C, 0x2B) 5
  $HFM = & $cd @(0x04, 0x08) 1
  $HDS = & $cd @(0x02, 0x06, 0x20) 2
  $MRP = & $cd @(0x02, 0x01, 0x1A, 0x06, 0x1B) 3
  $HS = & $cd @(0x14, 0x17) 1
  $MDOR = & $cd @(0x06, 0x0A) 1
  $MF = & $cd @(0x04, 0x06) 1
  $CAT = & $cd @(0x06, 0x0A) 3
  $RS = & $cd @(0x00, 0x1A, 0x23, 0x01) 2
  $size = New-Object int[] 0x24
  $size[0x00] = 2 + $strSz + 3 * $guidSz
  $size[0x01] = $RS + 2 * $strSz
  $size[0x02] = 4 + 2 * $strSz + $TDOR + (& $ix 0x04) + (& $ix 0x06)
  $size[0x03] = (& $ix 0x04)
  $size[0x04] = 2 + $strSz + $blobSz
  $size[0x05] = (& $ix 0x06)
  $size[0x06] = 8 + $strSz + $blobSz + (& $ix 0x08)
  $size[0x07] = (& $ix 0x08)
  $size[0x08] = 4 + $strSz
  $size[0x09] = (& $ix 0x02) + $TDOR
  $size[0x0A] = $MRP + $strSz + $blobSz
  $size[0x0B] = 2 + $HC + $blobSz
  $size[0x0C] = $HCA + $CAT + $blobSz
  $size[0x0D] = $HFM + $blobSz
  $size[0x0E] = 2 + $HDS + $blobSz
  $size[0x0F] = 6 + (& $ix 0x02)
  $size[0x10] = 4 + (& $ix 0x04)
  $size[0x11] = $blobSz
  $size[0x12] = (& $ix 0x02) + (& $ix 0x14)
  $size[0x13] = (& $ix 0x14)
  $size[0x14] = 2 + $strSz + $TDOR
  $size[0x15] = (& $ix 0x02) + (& $ix 0x17)
  $size[0x16] = (& $ix 0x17)
  $size[0x17] = 2 + $strSz + $blobSz
  $size[0x18] = 2 + (& $ix 0x06) + $HS
  $size[0x19] = (& $ix 0x02) + 2 * $MDOR
  $size[0x1A] = $strSz
  $size[0x1B] = $blobSz
  $size[0x1C] = 2 + $MF + $strSz + (& $ix 0x1A)
  $size[0x1D] = 4 + (& $ix 0x04)
  $size[0x1E] = 8
  $size[0x1F] = 4
  $size[0x20] = 16 + $blobSz + 2 * $strSz
  $size[0x21] = 4
  $size[0x22] = 12
  $size[0x23] = 12 + 2 * $blobSz + 2 * $strSz
  $start = New-Object long[] 0x24
  $at = $q
  for ($i = 0; $i -lt 0x24; $i++) { $start[$i] = $at; $at += $rows[$i] * $size[$i] }
  $strHeap = $streams["#Strings"][0]
  $rd = { param($o, $sz) if ($sz -eq 2) { [long][BitConverter]::ToUInt16($b, $o) } else { [long][BitConverter]::ToUInt32($b, $o) } }
  $str = { param($i) $o = $strHeap + $i; $e = $o; while ($b[$e] -ne 0) { $e++ }; [System.Text.Encoding]::UTF8.GetString($b, $o, $e - $o) }
  $asmRefs = @()
  for ($r = 0; $r -lt $rows[0x23]; $r++) {
    $o = [int]($start[0x23] + $r * $size[0x23])
    $v = ([BitConverter]::ToUInt16($b, $o)).ToString() + "." + [BitConverter]::ToUInt16($b, $o + 2) + "." + [BitConverter]::ToUInt16($b, $o + 4) + "." + [BitConverter]::ToUInt16($b, $o + 6)
    $nameIdx = & $rd ($o + 12 + $blobSz) $strSz
    $asmRefs += [pscustomobject]@{ Name = (& $str $nameIdx); Version = $v }
  }
  $modRefs = @()
  for ($r = 0; $r -lt $rows[0x1A]; $r++) { $o = [int]($start[0x1A] + $r * $size[0x1A]); $modRefs += (& $str (& $rd $o $strSz)) }
  $implMaps = @()
  for ($r = 0; $r -lt $rows[0x1C]; $r++) {
    $o = [int]($start[0x1C] + $r * $size[0x1C])
    $imp = & $str (& $rd ($o + 2 + $MF) $strSz)
    $scope = & $rd ($o + 2 + $MF + $strSz) (& $ix 0x1A)
    $mod = "?"; if ($scope -ge 1 -and $scope -le $modRefs.Count) { $mod = $modRefs[$scope - 1] }
    $implMaps += ($mod + "!" + $imp)
  }
  return [pscustomobject]@{ Version = $ver; Streams = $streams; Rows = $rows; AsmRefs = $asmRefs; ModRefs = $modRefs; ImplMaps = $implMaps; HeapFlags = $heap }
}

Write-Output "  -- the CLI metadata, by bytes --"
$md = $null
$mdFailed = $false
try { $md = MdRead $pe } catch { $mdFailed = $true; Write-Output ("    UNKNOWN: the metadata could not be read, " + $_.Exception.Message) }
if ($null -eq $md) { if (-not $mdFailed) { Write-Output "    no CLI header, so Roamer.exe is not a managed assembly" } } else {
  Write-Output ("    metadata version " + $md.Version + ", streams " + (($md.Streams.Keys | Sort-Object) -join ", ") + ", heap flags 0x" + ([int]$md.HeapFlags).ToString("X2"))
  Write-Output ("    rows: TypeRef " + $md.Rows[0x01] + ", TypeDef " + $md.Rows[0x02] + ", MethodDef " + $md.Rows[0x06] + ", MemberRef " + $md.Rows[0x0A] + ", ModuleRef " + $md.Rows[0x1A] + ", ImplMap " + $md.Rows[0x1C] + ", AssemblyRef " + $md.Rows[0x23])
  Write-Output ("    AssemblyRef, " + $md.AsmRefs.Count + ":")
  foreach ($a in $md.AsmRefs) { Write-Output ("      " + $a.Name + " " + $a.Version) }
  Write-Output ("    ModuleRef, the native DLLs it calls into, " + $md.ModRefs.Count + ": " + ($md.ModRefs -join ", "))
  Write-Output ("    ImplMap, each native entry point it declares, " + $md.ImplMaps.Count + ": " + ($md.ImplMaps -join ", "))
}

$roResolve = [ResolveEventHandler]{
  param($s, $e)
  $n = New-Object System.Reflection.AssemblyName($e.Name)
  $cand = Join-Path $nw ($n.Name + ".dll")
  if (Test-Path -LiteralPath $cand) { return [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($cand) }
  try { return [System.Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { return $null }
}
[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($roResolve)
$ra = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($roamer)
$refl = @($ra.GetReferencedAssemblies() | ForEach-Object { $_.Name + " " + $_.Version })
Write-Output ("    the same file read by reflection only, " + $ra.GetName().Name + " " + $ra.GetName().Version + ", entry point " + $ra.EntryPoint.DeclaringType.FullName + "::" + $ra.EntryPoint.Name + ", references " + $refl.Count)
if ($null -ne $md) {
  $byBytes = @($md.AsmRefs | ForEach-Object { $_.Name + " " + $_.Version } | Sort-Object)
  $same = (($byBytes -join "|") -eq ((@($refl | Sort-Object)) -join "|"))
  Write-Output ("    the byte read and the reflection read list the same references: " + $same)
  if (-not $same) { Write-Output ("    reflection lists: " + (@($refl | Sort-Object) -join ", ")) }
}
Write-Output ""

# ---------------------------------------------------------------------------------------
Write-Output "==== 3. Every referenced assembly in the install folder, each by its one full path, counted for the switch names ===="
$holders = New-Object System.Collections.Generic.List[string]
if ($roamerHolds) { $holders.Add($roamer) }
$refNames = @(); if ($null -ne $md) { $refNames = @($md.AsmRefs | ForEach-Object { $_.Name }) } else { $refNames = @($ra.GetReferencedAssemblies() | ForEach-Object { $_.Name }) }
foreach ($n in $refNames) {
  $cand = Join-Path $nw ($n + ".dll")
  if (-not (Test-Path -LiteralPath $cand)) { Write-Output ("  " + $n.PadRight(44) + " no file at " + $cand + ", not read"); continue }
  $bytes = [System.IO.File]::ReadAllBytes($cand)
  $c = SwitchCounts $bytes
  $parts = @(); foreach ($w in $switchNames) { $parts += ($w + " " + $c[$w][0] + "/" + $c[$w][1]) }
  $h = HoldsTable $c
  Write-Output ("  " + $n.PadRight(44) + " " + $bytes.Length + " bytes, ascii/utf16: " + ($parts -join ", ") + "   holds the table: " + $h)
  if ($h) { $holders.Add($cand) }
}
Write-Output ("  files holding ExecuteAddInPlugin, NoGui, Embedding and regserver together: " + $holders.Count)
foreach ($h in $holders) { Write-Output ("    " + $h) }
Write-Output ""

# ---------------------------------------------------------------------------------------
Write-Output "==== 4. The file that holds the switch table, as bytes ===="
foreach ($h in $holders) {
  if ($h -eq $roamer) { Write-Output "  Roamer.exe itself, listed in 1"; continue }
  Write-Output (FileLine $h)
  ListStrings (Split-Path $h -Leaf) ([System.IO.File]::ReadAllBytes($h))
}
if ($holders.Count -eq 0) { Write-Output "  UNKNOWN: neither Roamer.exe nor any assembly it references directly holds the four switch names together" }
Write-Output ""

# ---------------------------------------------------------------------------------------
# The IL reader of probe-automation-start.ps1, the same function, opcode by opcode.
$script:op1 = @{}; $script:op2 = @{}
foreach ($f in [System.Reflection.Emit.OpCodes].GetFields("Public,Static")) {
  $oc = $f.GetValue($null); $v = [int]$oc.Value
  if ($oc.Size -eq 1) { $script:op1[$v -band 0xFF] = $oc } else { $script:op2[$v -band 0xFF] = $oc }
}
function TypeName($t) {
  if ($null -eq $t) { return "null" }
  if ($t.IsByRef) { return (TypeName $t.GetElementType()) + "&" }
  if ($t.IsArray) { return (TypeName $t.GetElementType()) + "[]" }
  if ($t.IsGenericType) { $a = @(); foreach ($g in $t.GetGenericArguments()) { $a += (TypeName $g) }; $n = $t.Name; $k = $n.IndexOf('`'); if ($k -ge 0) { $n = $n.Substring(0, $k) }; return $t.Namespace + "." + $n + "<" + ($a -join ", ") + ">" }
  if ($t.IsGenericParameter) { return $t.Name }
  return $t.FullName
}
function IlRead($method) {
  $list = New-Object System.Collections.Generic.List[object]
  $body = $null
  try { $body = $method.GetMethodBody() } catch { $list.Add([pscustomobject]@{ Offset = -1; Name = "error"; Text = ("GetMethodBody threw, " + $_.Exception.Message); Member = $null }); return ,$list }
  if ($null -eq $body) { $list.Add([pscustomobject]@{ Offset = -1; Name = "nobody"; Text = "no IL body"; Member = $null }); return ,$list }
  $il = $body.GetILAsByteArray(); $mod = $method.Module; $i = 0
  while ($i -lt $il.Length) {
    $at = $i; $b = $il[$i]
    if ($b -eq 0xFE) { $oc = $script:op2[[int]$il[$i + 1]]; $i += 2 } else { $oc = $script:op1[[int]$b]; $i += 1 }
    if ($null -eq $oc) { $list.Add([pscustomobject]@{ Offset = $at; Name = "error"; Text = ("unknown opcode byte " + $b); Member = $null }); break }
    $text = ""; $member = $null
    switch ($oc.OperandType.ToString()) {
      "InlineNone" { }
      "ShortInlineBrTarget" { $d = [int]$il[$i]; if ($d -gt 127) { $d -= 256 }; $i += 1; $text = "IL_" + ($i + $d).ToString("X4"); $member = ($i + $d) }
      "InlineBrTarget" { $d = [BitConverter]::ToInt32($il, $i); $i += 4; $text = "IL_" + ($i + $d).ToString("X4"); $member = ($i + $d) }
      "ShortInlineI" { $d = [int]$il[$i]; if ($d -gt 127) { $d -= 256 }; $text = [string]$d; $i += 1 }
      "ShortInlineVar" { $text = [string]$il[$i]; $i += 1 }
      "InlineVar" { $i += 2 }
      "InlineI" { $text = [string][BitConverter]::ToInt32($il, $i); $i += 4 }
      "InlineI8" { $i += 8 }
      "InlineR" { $i += 8 }
      "ShortInlineR" { $i += 4 }
      "InlineSwitch" { $n = [BitConverter]::ToInt32($il, $i); $i += 4 + 4 * $n; $text = "switch of " + $n }
      "InlineString" { $tok = [BitConverter]::ToInt32($il, $i); $i += 4; try { $member = $mod.ResolveString($tok); $text = "`"" + $member + "`"" } catch { $text = "string token unresolved" } }
      "InlineMethod" { $tok = [BitConverter]::ToInt32($il, $i); $i += 4; try { $member = $mod.ResolveMethod($tok); $text = (TypeName $member.DeclaringType) + "::" + $member.Name } catch { $text = "method token 0x" + $tok.ToString("X8") + " unresolved" } }
      "InlineField" { $tok = [BitConverter]::ToInt32($il, $i); $i += 4; try { $member = $mod.ResolveField($tok); $text = (TypeName $member.DeclaringType) + "::" + $member.Name } catch { $text = "field token 0x" + $tok.ToString("X8") + " unresolved" } }
      "InlineType" { $tok = [BitConverter]::ToInt32($il, $i); $i += 4; try { $member = $mod.ResolveType($tok); $text = TypeName $member } catch { $text = "type token unresolved" } }
      "InlineTok" {
        $tok = [BitConverter]::ToInt32($il, $i); $i += 4
        $text = "token 0x" + $tok.ToString("X8") + " unresolved"
        try { $member = $mod.ResolveType($tok); $text = "type " + (TypeName $member) } catch { try { $member = $mod.ResolveMember($tok); $text = [string]$member } catch { } }
      }
      default { $i += 4; $text = "operand not decoded" }
    }
    $list.Add([pscustomobject]@{ Offset = $at; Name = $oc.Name; Text = $text; Member = $member })
  }
  return ,$list
}

function MemberKey($x) {
  if ($x -is [System.Reflection.FieldInfo]) { return ("field " + (TypeName $x.DeclaringType) + "::" + $x.Name) }
  if ($x -is [System.Reflection.MethodBase]) { $ps = @($x.GetParameters() | ForEach-Object { TypeName $_.ParameterType }); return ("method " + (TypeName $x.DeclaringType) + "::" + $x.Name + "(" + ($ps -join ", ") + ")") }
  return $null
}
# Every store, call, object made, literal and field read from that instruction up to that
# offset, in order. A summary of what a stretch of IL touches, not a run of it.
function Summary($ins, [int]$fromIdx, [int]$uptoOffset) {
  $did = @()
  for ($z = $fromIdx; $z -ge 0 -and $z -lt $ins.Count; $z++) {
    $y = $ins[$z]
    if ($y.Offset -ge $uptoOffset) { break }
    if ($y.Name -eq "stfld" -or $y.Name -eq "stsfld") { $did += ("stores " + $y.Text) }
    elseif ($y.Name -eq "call" -or $y.Name -eq "callvirt" -or $y.Name -eq "newobj") { $did += ($y.Name + " " + $y.Text) }
    elseif ($y.Name -eq "ldstr") { $did += ("literal " + $y.Text) }
    elseif ($y.Name -eq "ldfld" -or $y.Name -eq "ldsfld") { $did += ("reads " + $y.Text) }
  }
  return $did
}
function IlLines($ins, [int]$fromIdx, [int]$uptoOffset, $indent) {
  for ($z = $fromIdx; $z -ge 0 -and $z -lt $ins.Count; $z++) {
    $y = $ins[$z]
    if ($y.Offset -ge $uptoOffset) { break }
    if ($y.Offset -lt 0) { Write-Output ($indent + $y.Name + ": " + $y.Text) } else { Write-Output ($indent + "IL_" + $y.Offset.ToString("X4") + "  " + $y.Name.PadRight(10) + " " + $y.Text) }
  }
}
function Sig($m) {
  $ps = @($m.GetParameters() | ForEach-Object { (TypeName $_.ParameterType) + " " + $_.Name })
  $vis = "private"; if ($m.IsPublic) { $vis = "public" } elseif ($m.IsFamilyOrAssembly) { $vis = "protected internal" } elseif ($m.IsFamily) { $vis = "protected" } elseif ($m.IsAssembly) { $vis = "internal" }
  $st = ""; if ($m.IsStatic) { $st = "static " }
  $ret = ""; if ($m -is [System.Reflection.MethodInfo]) { $ret = (TypeName $m.ReturnType) + " " }
  return ($vis + " " + $st + $ret + $m.Name + "(" + ($ps -join ", ") + ")")
}

Write-Output "==== 5. What the parser does with each option, off the IL, reflection only. Not what a start does ===="
$flagsAll = [System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static,DeclaredOnly"
$watch = @("Embedding", "Automation", "NoGui", "OpenFile", "Exit", "ExecuteAddInPlugin", "AddPluginAssembly")
$loaded = @{}
$clTypes = @()
foreach ($h in $holders) {
  $ga = $null
  if ($h -eq $roamer) { $ga = $ra } else { $ga = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($h) }
  $loaded[$h] = $ga
  $types = @()
  try { $types = @($ga.GetTypes()) } catch [System.Reflection.ReflectionTypeLoadException] { $types = @($_.Exception.Types | Where-Object { $null -ne $_ }); Write-Output ("  some types of " + (Split-Path $h -Leaf) + " could not load, " + $types.Count + " read") }
  $cl = @($types | Where-Object { $_.Name -match "CommandLine" })
  $clTypes += $cl
  Write-Output ("  types in " + (Split-Path $h -Leaf) + " whose name holds CommandLine: " + (($cl | ForEach-Object { $_.FullName }) -join ", "))
  foreach ($t in $cl) {
    Write-Output ("  " + $t.FullName + ", base " + $t.BaseType.FullName)
    foreach ($f in $t.GetFields($flagsAll)) { Write-Output ("      field  " + (TypeName $f.FieldType) + " " + $f.Name) }
    foreach ($c in $t.GetConstructors($flagsAll)) { Write-Output ("      ctor   " + (Sig $c)) }
    foreach ($m in ($t.GetMethods($flagsAll) | Sort-Object Name)) { Write-Output ("      method " + (Sig $m)) }
  }
  Write-Output ""
  foreach ($t in $cl) {
    foreach ($m in $t.GetMethods($flagsAll)) {
      $ins = IlRead $m
      $opts = @()
      for ($k = 1; $k -lt $ins.Count - 1; $k++) {
        $x = $ins[$k]
        if (-not (($x.Name -eq "call" -or $x.Name -eq "callvirt") -and $x.Member -is [System.Reflection.MethodBase] -and ($x.Member.Name -eq "MatchOption" -or $x.Member.Name -eq "MatchShellOption") -and $ins[$k - 1].Name -eq "ldstr")) { continue }
        $br = $ins[$k + 1]
        $label = [string]$ins[$k - 1].Member
        if ($x.Member.Name -eq "MatchShellOption") { $label = $label + " (MatchShellOption)" }
        $o = [pscustomobject]@{ Opt = $label; Kind = $br.Name; Start = -1; StartOff = -1; Upto = -1; Target = -1; Shares = "" }
        if ($br.Name -like "brfalse*") { $o.Start = $k + 2; $o.StartOff = $ins[$k + 2].Offset; $o.Upto = [int]$br.Member }
        elseif ($br.Name -like "brtrue*") { $o.Target = [int]$br.Member }
        $opts += $o
      }
      if ($opts.Count -eq 0) { continue }
      Write-Output ("  " + $t.Name + "::" + $m.Name + " matches " + $opts.Count + " options after a literal, " + @($opts | Where-Object { $_.Opt -notlike "*(MatchShellOption)" }).Count + " through MatchOption and " + @($opts | Where-Object { $_.Opt -like "*(MatchShellOption)" }).Count + " through MatchShellOption. For each, the IL its match runs, up to where a miss goes:")
      foreach ($o in $opts) {
        if ($o.Target -ge 0) { foreach ($q in $opts) { if ($q.StartOff -eq $o.Target) { $o.Shares = $q.Opt; $o.Start = $q.Start; $o.StartOff = $q.StartOff; $o.Upto = $q.Upto } } }
        $how = "falls through when matched"; if ($o.Target -ge 0) { $how = "jumps to IL_" + $o.Target.ToString("X4") + ", the body of " + $(if ($o.Shares) { $o.Shares } else { "UNKNOWN" }) }
        if ($o.Start -lt 0) { Write-Output ("    " + $o.Opt.PadRight(28) + " UNKNOWN, the instruction after MatchOption is " + $o.Kind); continue }
        $sum = @(Summary $ins $o.Start $o.Upto)
        Write-Output ("    " + $o.Opt.PadRight(28) + " " + $how + ": " + $(if ($sum.Count -gt 0) { $sum -join "; " } else { "touches nothing" }))
      }
      Write-Output ""
      Write-Output "  The whole IL of the bodies of the options this question turns on:"
      foreach ($o in $opts) {
        if ($watch -notcontains $o.Opt -or $o.Start -lt 0 -or $o.Target -ge 0) { continue }
        Write-Output ("    -- " + $o.Opt + ", IL_" + $o.StartOff.ToString("X4") + " up to IL_" + $o.Upto.ToString("X4") + " --")
        IlLines $ins $o.Start $o.Upto "      "
      }
      Write-Output ""
      # The helpers those bodies call on the same type, whole, since they decide where an
      # option's arguments go.
      $helpers = @{}
      foreach ($o in $opts) {
        if ($watch -notcontains $o.Opt -or $o.Start -lt 0) { continue }
        for ($z = $o.Start; $z -lt $ins.Count -and $ins[$z].Offset -lt $o.Upto; $z++) { $y = $ins[$z]; if ($y.Member -is [System.Reflection.MethodBase] -and $y.Member.DeclaringType -eq $t) { $helpers[$y.Member.Name] = $y.Member } }
      }
      # And the four that decide what counts as a switch at all: its prefix and its case.
      foreach ($nm in @("IsOption", "MatchOption", "MatchShellOption", "StripArg")) {
        foreach ($hm in @($t.GetMethods($flagsAll) | Where-Object { $_.Name -eq $nm })) { $helpers[$nm + (Sig $hm)] = $hm }
      }
      foreach ($hn in ($helpers.Keys | Sort-Object)) {
        $hm = $helpers[$hn]
        Write-Output ("  -- helper " + $t.Name + "::" + (Sig $hm) + ", whole --")
        $hi = IlRead $hm
        IlLines $hi 0 ([int]::MaxValue) "      "
      }
    }
  }
}
Write-Output ""

Write-Output "==== 6. Where the parsed actions go and who reads them, reflection only ===="
# Followed: the parser's action list and anything on the parser that hands it out, every
# member of the action dispatcher, and the three config fields a start reads, 5z-d.
$follow = @{}
foreach ($t in $clTypes) {
  if ($t.Name -eq "CommandLineParser") {
    foreach ($f in $t.GetFields($flagsAll)) { if ((TypeName $f.FieldType) -match "CommandLineAction") { $follow[(MemberKey $f)] = $f } }
    foreach ($m in $t.GetMethods($flagsAll)) { if ((TypeName $m.ReturnType) -match "CommandLineAction") { $follow[(MemberKey $m)] = $m } }
    foreach ($c in $t.GetConstructors($flagsAll)) { $follow[(MemberKey $c)] = $c }
  }
  if ($t.Name -eq "CommandLineActionDispatcher") {
    foreach ($m in $t.GetMethods($flagsAll)) { $follow[(MemberKey $m)] = $m }
    foreach ($c in $t.GetConstructors($flagsAll)) { $follow[(MemberKey $c)] = $c }
  }
  if ($t.Name -eq "CommandLineConfig") {
    foreach ($f in $t.GetFields($flagsAll)) { if (@("COMAutomationStartup", "ExitAfterActions", "GuiState") -contains $f.Name) { $follow[(MemberKey $f)] = $f } }
  }
}
Write-Output ("  followed, " + $follow.Count + ":")
foreach ($k in ($follow.Keys | Sort-Object)) { Write-Output ("    " + $k) }
$readerMethods = New-Object System.Collections.Generic.List[object]
function Readers($asmObj, $label) {
  $mod = $asmObj.ManifestModule
  $toks = @{}
  foreach ($k in $follow.Keys) { $x = $follow[$k]; if ($x.Module.Equals($mod)) { $toks[$x.MetadataToken] = $x } }
  # A member defined in another assembly is reached through a MemberRef token. Every
  # MemberRef row is resolved once and kept where it resolves to a followed member.
  $mr = 0; try { $mr = [int](MdRead (PeRead $asmObj.Location)).Rows[0x0A] } catch { Write-Output ("  UNKNOWN: the MemberRef count of " + $label + " could not be read, " + $_.Exception.Message) }
  for ($r = 1; $r -le $mr; $r++) {
    $tok = 0x0A000000 + $r
    try { $x = $mod.ResolveMember($tok); $k = MemberKey $x; if ($null -ne $k -and $follow.ContainsKey($k)) { $toks[$tok] = $x } } catch { }
  }
  Write-Output ("  " + $label + ": " + $toks.Count + " tokens reach a followed member, " + $mr + " MemberRef rows resolved")
  $types = @(); try { $types = @($asmObj.GetTypes()) } catch [System.Reflection.ReflectionTypeLoadException] { $types = @($_.Exception.Types | Where-Object { $null -ne $_ }) }
  # An ordinal list, not a hashtable, because a PowerShell hashtable compares its string
  # keys case blind and two byte patterns can differ only in a byte that is a letter.
  $pats = New-Object System.Collections.Generic.List[string]
  foreach ($tok in $toks.Keys) {
    $tb = [BitConverter]::GetBytes([int]$tok)
    $ops = @(0x7B, 0x7C); if ($toks[$tok] -is [System.Reflection.MethodBase]) { $ops = @(0x28, 0x6F, 0x73) }
    foreach ($op in $ops) { $pats.Add($latin1.GetString([byte[]](@([byte]$op) + $tb))) }
  }
  $n = 0; $hits = 0
  foreach ($t in $types) {
    $ms = @(); try { $ms = @($t.GetMethods($flagsAll)) + @($t.GetConstructors($flagsAll)) } catch { continue }
    foreach ($m in $ms) {
      $body = $null; try { $body = $m.GetMethodBody() } catch { continue }
      if ($null -eq $body) { continue }
      $n++
      $s = $latin1.GetString($body.GetILAsByteArray())
      $any = $false; foreach ($pt in $pats) { if ($s.Contains($pt)) { $any = $true; break } }
      if (-not $any) { continue }
      # A byte match is confirmed by decoding the method, so an operand that happens to
      # hold the same bytes is not counted.
      $ins = IlRead $m
      $mine = 0
      foreach ($x in $ins) {
        if ($x.Name -notmatch '^(ldfld|ldflda|call|callvirt|newobj)$') { continue }
        $k = MemberKey $x.Member
        if ($null -ne $k -and $follow.ContainsKey($k)) { Write-Output ("  " + $label + "  " + (TypeName $t) + "::" + $m.Name + "  IL_" + $x.Offset.ToString("X4") + " " + $x.Name + " " + $k); $hits++; $mine++ }
      }
      if ($mine -gt 0 -and -not ($t.Name -match "CommandLine")) { $readerMethods.Add([pscustomobject]@{ Label = $label; Type = $t; Method = $m }) }
    }
  }
  Write-Output ("  " + $label + ": " + $n + " method bodies read, " + $hits + " uses of a followed member")
}
foreach ($h in $holders) { if ($h -ne $roamer) { Readers $loaded[$h] (Split-Path $h -Leaf) } }
Readers $ra "Roamer.exe"
Write-Output ""

Write-Output "==== 7. The dispatcher, and every method outside the CommandLine types that uses a followed member, summarised off the IL ===="
foreach ($t in $clTypes) {
  if ($t.Name -ne "CommandLineActionDispatcher") { continue }
  foreach ($m in (@($t.GetConstructors($flagsAll)) + @($t.GetMethods($flagsAll)))) {
    $ins = IlRead $m
    $sum = @(Summary $ins 0 ([int]::MaxValue))
    Write-Output ("  " + $t.Name + "::" + (Sig $m))
    foreach ($line in $sum) { Write-Output ("      " + $line) }
    $holdsLiteral = @($ins | Where-Object { $_.Name -eq "ldstr" -and ([string]$_.Member) -eq "ExecuteAddInPlugin" }).Count -gt 0
    if ($holdsLiteral) { Write-Output "      -- this method holds the literal ExecuteAddInPlugin, its whole IL --"; IlLines $ins 0 ([int]::MaxValue) "        " }
  }
}
Write-Output ""
foreach ($r in $readerMethods) {
  $ins = IlRead $r.Method
  $sum = @(Summary $ins 0 ([int]::MaxValue))
  Write-Output ("  " + $r.Label + "  " + (TypeName $r.Type) + "::" + (Sig $r.Method) + ", " + $sum.Count + " things touched, in order:")
  foreach ($line in $sum) { Write-Output ("      " + $line) }
}
Write-Output ""
Write-Output "==== 8. The branches around the two places actions are dispatched, and where an action goes, whole IL, reflection only ===="
foreach ($t in $clTypes) {
  if ($t.Name -ne "CommandLineConfig") { continue }
  foreach ($nt in $t.GetNestedTypes($flagsAll)) {
    if (-not $nt.IsEnum) { continue }
    $vals = @(); foreach ($f in $nt.GetFields("Public,Static")) { $vals += ($f.Name + " = " + [Convert]::ToInt64($f.GetRawConstantValue())) }
    Write-Output ("  enum " + $nt.FullName + ": " + ($vals -join ", "))
  }
}
function IlAround($m, $label, $fromMatch, $toMatch) {
  $ins = IlRead $m
  $a = -1; $z = -1
  for ($k = 0; $k -lt $ins.Count; $k++) {
    if ($a -lt 0 -and $ins[$k].Text -like $fromMatch) { $a = $k }
    if ($a -ge 0 -and $ins[$k].Text -like $toMatch) { $z = $k; break }
  }
  if ($a -lt 0 -or $z -lt 0) { Write-Output ("  UNKNOWN: " + $label + " holds no stretch from " + $fromMatch + " to " + $toMatch); return }
  Write-Output ("  -- " + $label + ", IL_" + $ins[$a].Offset.ToString("X4") + " to IL_" + $ins[$z].Offset.ToString("X4") + " --")
  IlLines $ins $a ($ins[$z].Offset + 1) "      "
}
$prog = $ra.GetType("NetRoamer.Program")
if ($null -eq $prog) { Write-Output "  UNKNOWN: no NetRoamer.Program in Roamer.exe" } else {
  $mi = $prog.GetMethod("MainImpl", $flagsAll)
  if ($null -eq $mi) { Write-Output "  UNKNOWN: no NetRoamer.Program::MainImpl" } else {
    IlAround $mi "Roamer.exe NetRoamer.Program::MainImpl, from the parse to where the parsed GuiState is first read" "*CommandLineParser::Parse*" "*InitialiseResourcesConfig::HiddenGui*"
    IlAround $mi "Roamer.exe NetRoamer.Program::MainImpl, from LoadPlugins to RunGui" "*ApplicationImpl::LoadPlugins*" "*NetRoamer.Program::RunGui*"
    # Local 8 picks between dispatching every action now and handing them to the window.
    # Every store into it, with the instructions that compute what is stored.
    $ins = IlRead $mi
    for ($k = 0; $k -lt $ins.Count; $k++) {
      if (($ins[$k].Name -eq "stloc.s" -or $ins[$k].Name -eq "stloc") -and $ins[$k].Text -eq "8") {
        $from = [Math]::Max(0, $k - 12)
        Write-Output ("  -- Roamer.exe NetRoamer.Program::MainImpl, a store into local 8 at IL_" + $ins[$k].Offset.ToString("X4") + ", with the twelve instructions before it --")
        IlLines $ins $from ($ins[$k].Offset + 1) "      "
      }
    }
  }
}
foreach ($h in $holders) {
  if ($h -eq $roamer) { continue }
  $mw = $loaded[$h].GetType("Autodesk.Navisworks.Gui.Roamer.MainWindow")
  if ($null -eq $mw) { Write-Output ("  UNKNOWN: no Autodesk.Navisworks.Gui.Roamer.MainWindow in " + (Split-Path $h -Leaf)); continue }
  $oi = $mw.GetMethod("OnIdle", $flagsAll)
  if ($null -eq $oi) { Write-Output "  UNKNOWN: no MainWindow::OnIdle"; continue }
  IlAround $oi ((Split-Path $h -Leaf) + " MainWindow::OnIdle, from its first read of command_line_actions to its store of it") "*MainWindow::command_line_actions" "*CommandLineAction>::get_Count"
}
# DispatchOneAction hands the action's type and arguments to ApplicationImpl.DispatchAutomationAction,
# section 7. That method is in the API assembly, read by its one full path.
$apiPath = Join-Path $nw "Autodesk.Navisworks.Api.dll"
if (-not (Test-Path -LiteralPath $apiPath)) { Write-Output ("  UNKNOWN: no Autodesk.Navisworks.Api.dll at " + $apiPath) } else {
  $api = $null
  foreach ($x in [AppDomain]::CurrentDomain.ReflectionOnlyGetAssemblies()) { if ($x.GetName().Name -eq "Autodesk.Navisworks.Api" -and $x.Location -eq $apiPath) { $api = $x } }
  if ($null -eq $api) { $api = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($apiPath) }
  $impl = $api.GetType("Autodesk.Navisworks.Internal.ApiImplementation.ApplicationImpl")
  if ($null -eq $impl) { Write-Output "  UNKNOWN: no ApplicationImpl in the API assembly" } else {
    foreach ($m in @($impl.GetMethods($flagsAll) | Where-Object { $_.Name -match "AutomationAction|AddInPlugin|PluginAssembly" } | Sort-Object Name)) {
      $ins = IlRead $m
      Write-Output ("  Autodesk.Navisworks.Api.dll ApplicationImpl::" + (Sig $m) + ", " + $ins.Count + " instructions, whole:")
      IlLines $ins 0 ([int]::MaxValue) "      "
    }
  }
  # DispatchAutomationAction finds what to run with ApplicationAutomationImpl.LookupMethod.
  # Every member of that type, and the whole IL of LookupMethod and of any method named for
  # an add-in plugin or a plugin assembly.
  $aai = $api.GetType("Autodesk.Navisworks.Internal.ApiImplementation.ApplicationAutomationImpl")
  if ($null -eq $aai) { Write-Output "  UNKNOWN: no ApplicationAutomationImpl in the API assembly" } else {
    Write-Output ("  " + $aai.FullName + ", base " + $aai.BaseType.FullName)
    foreach ($f in $aai.GetFields($flagsAll)) { Write-Output ("      field  " + (TypeName $f.FieldType) + " " + $f.Name) }
    foreach ($m in ($aai.GetMethods($flagsAll) | Sort-Object Name)) { Write-Output ("      method " + (Sig $m)) }
    foreach ($m in @($aai.GetMethods($flagsAll) | Where-Object { $_.Name -match "LookupMethod|AddInPlugin|PluginAssembly" } | Sort-Object Name)) {
      $ins = IlRead $m
      Write-Output ("  Autodesk.Navisworks.Api.dll ApplicationAutomationImpl::" + (Sig $m) + ", " + $ins.Count + " instructions, whole:")
      IlLines $ins 0 ([int]::MaxValue) "      "
    }
  }
}
Write-Output ""
Write-Output "WHAT EACH SWITCH DOES ON A START IS UNKNOWN. Nothing here ran Roamer.exe. The lists above say what the files carry, what the parser stores and what the IL of the readers touches, in the order it is written, which is not the order a start runs it."