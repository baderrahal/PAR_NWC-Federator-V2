# tools\loop\nw-guard.ps1, F103. The loop's guard code, in one copy. It is dot-sourced by
# tools\probes\probe-automation-start.ps1, tools\loop\run.ps1 and tools\loop\prove-run.ps1,
# and handed as text to every runspace they start. It is a file of functions with no main
# body, so dot-sourcing it runs nothing and writes nothing.
#
# WHERE IT CAME FROM. Commit 377cb1a moved these functions out of the probe as it merged with
# F100 at 0eb4ede, and changed nothing else. The probe wrote some of its guards inline, and
# each of those was wrapped as a function whose body is the probe's own lines:
# OwnProcessRefusal, UnprovedRefusal, NewWinTypes, WatchSync, Watchdog, BackupSettings,
# AdoptStart, UnprovedAtEnd, PutBackReasons and SettingsPutBack. What a wrapper changed is
# only what a function needs: a value the probe read from its own variables comes in as a
# parameter of the same name, a result the probe kept in a variable goes back in the object
# the function returns, and a stop that ended the probe with exit 1 comes back to the caller,
# who stops. Every line of this file at 377cb1a is mapped to its line of the probe, or named
# wrapper, comment or changed with both texts, in the move proof of F103.
#
# WHAT F103 CHANGED OR ADDED AFTER THE MOVE, so these are not the probe's lines:
# - moved unchanged since 377cb1a: every function not named below
# - changed: AdoptStart, which holds the handle and writes adopted into mypid.txt, the
#   watchdog's adopted deadline, which closes through CloseAdopted, WatchSync, which carries
#   MyProc, BackupSettings, which prints the key it exported, WindowLines, which reads
#   through WindowRecords, SettingsPutBack, which returns what it could not write, and
#   NewWinTypes, which emits seven more calls
# - added: HeldRead, HeldState, CloseAdopted, WindowRecords, WindowKind and ReadShared
# The rules these functions keep are written at the top of the probe and in
# .claude\rules\loop.md, and are not repeated here.
#
# A variable a function reads and no parameter carries is read from the caller's scope, as
# it was in the probe: $loopRoot and $nw in Mask, $winType and $procType in WindowsOf,
# $beforeTicks in NewRoamers, $utf8 in AdoptStart, and the caller's own Say.

# The functions both the main thread and the watchdog use, written once and handed to the
# watchdog through $sync. None of them writes to the result. They return lines, and each
# caller prints them.
function Err($e) {
  $parts = @(); $x = $e
  while ($null -ne $x) { $parts += ($x.GetType().FullName + ": " + $x.Message); $x = $x.InnerException }
  return ($parts -join " <- ")
}
function UtcTicks($dt) { return $dt.ToUniversalTime().Ticks }

# A path in a printed value is shown in full only under the loop folder or the install
# folder. Any other path shows as its file name, if it has one, and a sha1 of the whole
# path, so an entry that moved can be followed without printing where it lives.
function Mask($s) {
  if ($null -eq $s) { return "null" }
  $s = [string]$s
  if ($s -notmatch '[A-Za-z]:\\|\\\\') { return ("`"" + $s + "`"") }
  if ($s.StartsWith($loopRoot, [StringComparison]::OrdinalIgnoreCase)) { return ("`"%LOCALAPPDATA%\NwcFederatorLoop" + $s.Substring($loopRoot.Length) + "`"") }
  if ($s.StartsWith($nw, [StringComparison]::OrdinalIgnoreCase)) { return ("`"" + $s + "`"") }
  $sha = [System.Security.Cryptography.SHA1]::Create()
  $h = [BitConverter]::ToString($sha.ComputeHash((New-Object System.Text.UTF8Encoding($false)).GetBytes($s.ToLowerInvariant()))).Replace("-", "").Substring(0, 12)
  $what = "text holding a path"
  if ($s -match '^[A-Za-z]:\\[^;|<>"*?]*$') {
    $leaf = Split-Path $s -Leaf
    if ($leaf -match '\.[A-Za-z0-9]{2,4}$') { $what = "a path outside the loop folder, file " + $leaf } else { $what = "a folder outside the loop folder" }
  }
  return ("<" + $what + ", sha1 " + $h + ">")
}

# The command line tests. HoldsEmbedding is the option COM passes, used to adopt.
# EmbeddingWord is the word anywhere in any case, used to tell a possible start from a
# Navisworks started by hand. NamesAutomation is either word, printed by the watchdog.
function HoldsEmbedding($cmd) { return ($null -ne $cmd -and $cmd -match '(^|\s)[-/]Embedding(\s|$)') }
function AutomationForm($cmd) { return ($null -ne $cmd -and $cmd -match '^\s*"?[^"]*\\Roamer\.exe"?\s+[-/]Embedding\s*$') }
function EmbeddingWord($cmd) { return ($null -ne $cmd -and $cmd -match 'embedding') }
function NamesAutomation($cmd) { return ($null -ne $cmd -and $cmd -match 'embedding|automation') }
function WordText($cmd) {
  if ($null -eq $cmd) { return "UNKNOWN, the command line could not be read" }
  if (NamesAutomation $cmd) { return "YES" }
  return "no"
}

# One Win32_Process read. Ok says whether the query itself succeeded, so a process that is
# not there reads Ok with no row, and a read that threw reads not Ok.
function CimRead($id) {
  $r = [pscustomobject]@{ Ok = $false; Row = $null; Error = "" }
  try { $r.Row = Get-CimInstance Win32_Process -Filter ("ProcessId=" + $id) -ErrorAction Stop; $r.Ok = $true } catch { $r.Error = (Err $_.Exception) }
  return $r
}
function ParentText($child, $parentRead, $parentId) {
  if ($null -eq $child) { return "UNKNOWN, the process itself could not be read" }
  if ($null -eq $parentRead -or -not $parentRead.Ok) { return ([string]$parentId + " UNKNOWN, the read threw") }
  if ($null -eq $parentRead.Row) { return ([string]$parentId + " not running") }
  if ($null -ne $parentRead.Row.CreationDate -and $null -ne $child.CreationDate -and $parentRead.Row.CreationDate -lt $child.CreationDate) { return ([string]$parentId + " " + $parentRead.Row.Name) }
  return ([string]$parentId + " UNKNOWN, the process holding that id now did not start before this one")
}

# unproved-starts.txt, one line per start: pid, start ticks UTC or the word Roamer when the
# start time never read, the time it was first seen UTC, when the line was written UTC, and
# why.
function UnprovedLine($id, $ticks, $firstSeenUtc, $why) {
  $t = "Roamer"; if ($null -ne $ticks -and $ticks -ne 0) { $t = [string]$ticks }
  $f = "-"; if ($null -ne $firstSeenUtc) { $f = $firstSeenUtc.ToString("o") }
  return ([string]$id + "`t" + $t + "`t" + $f + "`t" + [DateTime]::UtcNow.ToString("o") + "`t" + $why)
}
# One append for every line, so a throw means none of them was written.
function AppendUnproved($path, $lines) {
  $u = New-Object System.Text.UTF8Encoding($false)
  $text = ""
  if (-not (Test-Path -LiteralPath $path)) { $text = "# pid`tstart ticks UTC, or Roamer when it never read`tfirst seen UTC`twritten UTC`twhy`r`n" }
  foreach ($l in $lines) { $text += $l + "`r`n" }
  [System.IO.File]::AppendAllText($path, $text, $u)
}
# The write is tried first, and each start is said written down only when it worked. When it
# did not, each is said NOT written down, with the reason, and that the next run will not
# refuse on it. Returns the lines for the caller to print.
function UnprovedWrite($path, $lines, $said) {
  $res = New-Object System.Collections.Generic.List[string]
  $werr = $null
  try { AppendUnproved $path $lines } catch { $werr = (Err $_.Exception) }
  if ($null -eq $werr) {
    foreach ($s in $said) { $res.Add($s + ", written down") }
    $res.Add([string]@($lines).Count + " starts written to " + (Mask $path))
  } else {
    foreach ($s in $said) { $res.Add($s + ", NOT written down") }
    $res.Add("could NOT write " + @($lines).Count + " starts to " + (Mask $path) + ", " + $werr.TrimEnd('.') + ". So the next run will not refuse on them, and a person has to look")
  }
  return ,$res
}

# Every Roamer in $running, new since step 2 and running now, bar the adopted one, with its
# start ticks when they read, when it was first seen, and its kind off its command line:
# embedding, unreadable, or hand for a Navisworks someone started by hand.
function PossibleStarts($running, $roamerSet, $zeroSightings, $exclPid, $exclTicks) {
  $res = New-Object System.Collections.Generic.List[object]
  foreach ($p in $running) {
    $ticks = $null; $note = ""
    try { $st = $p.StartTime; if ($null -ne $st) { $ticks = UtcTicks $st } else { $note = "its start time reads as nothing" } } catch { $note = "its start time could not be read, " + (Err $_.Exception) }
    if ($null -ne $exclPid -and $exclPid -ne 0 -and $p.Id -eq $exclPid -and ($null -eq $ticks -or $ticks -eq $exclTicks)) { continue }
    $first = $null
    if ($null -ne $ticks -and $roamerSet.ContainsKey([string]$p.Id + "|" + $ticks)) { $first = $roamerSet[[string]$p.Id + "|" + $ticks] }
    elseif ($zeroSightings.ContainsKey($p.Id)) { $first = $zeroSightings[$p.Id][0] }
    if ($null -eq $first) { $first = [DateTime]::UtcNow }
    $cr = CimRead $p.Id
    if ($cr.Ok -and $null -eq $cr.Row) { continue }
    $kind = "unreadable"
    if ($cr.Ok -and $null -ne $cr.Row.CommandLine) { if (EmbeddingWord $cr.Row.CommandLine) { $kind = "embedding" } else { $kind = "hand" } }
    elseif (-not $cr.Ok) { $note = ($note + " the Win32_Process read threw, " + $cr.Error).Trim() }
    $res.Add([pscustomobject]@{ Pid = $p.Id; Ticks = $ticks; FirstSeen = $first; Kind = $kind; Note = $note })
  }
  return $res
}

# The registry, read key by key. A key whose values or subkeys cannot be read is kept in
# Failed with the reason, and nothing is ever written under it.
function RegRead($sub) {
  $r = [pscustomobject]@{ Read = @{}; Failed = @{}; Missing = $false }
  $root = $null
  try { $root = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($sub, $false) } catch { $r.Failed["HKEY_CURRENT_USER\" + $sub] = (Err $_.Exception); return $r }
  if ($null -eq $root) { $r.Missing = $true; return $r }
  try { RegWalk $root $r } finally { $root.Close() }
  return $r
}
function RegWalk($k, $r) {
  $name = $k.Name
  $vals = @{}
  try {
    foreach ($n in $k.GetValueNames()) {
      $vals[$n] = [pscustomobject]@{ Kind = $k.GetValueKind($n); Data = $k.GetValue($n, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames) }
    }
  } catch { $r.Failed[$name] = "its values, " + (Err $_.Exception); return }
  $r.Read[$name] = $vals
  $subs = $null
  try { $subs = $k.GetSubKeyNames() } catch { $r.Failed[$name] = "its subkeys, " + (Err $_.Exception); return }
  foreach ($s in $subs) {
    $c = $null
    try { $c = $k.OpenSubKey($s, $false) } catch { $r.Failed[$name + "\" + $s] = (Err $_.Exception); continue }
    if ($null -eq $c) { $r.Failed[$name + "\" + $s] = "listed, then could not be opened"; continue }
    try { RegWalk $c $r } finally { $c.Close() }
  }
}
function UnderFailed($name, $failed) {
  foreach ($f in $failed.Keys) { if ($name -eq $f -or $name.StartsWith($f + "\", [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
function FailedBelow($name, $failed) {
  foreach ($f in $failed.Keys) { if ($f.StartsWith($name + "\", [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
function ParentKey($name) { return $name.Substring(0, $name.LastIndexOf('\')) }
function ShortKey($name, $root) { if ($null -eq $name) { return "" }; return $name.Replace($root, (Split-Path $root -Leaf)) }
function RegText($v) {
  if ($null -eq $v) { return "absent" }
  $d = $v.Data
  switch ([string]$v.Kind) {
    "Binary" { return ("Binary " + [BitConverter]::ToString([byte[]]$d)) }
    "MultiString" { return ("MultiString " + ((@($d) | ForEach-Object { Mask $_ }) -join " | ")) }
    "String" { return ("String " + (Mask $d)) }
    "ExpandString" { return ("ExpandString " + (Mask $d)) }
    default { return ([string]$v.Kind + " " + [string]$d) }
  }
}
function RegSame($a, $b) {
  if ($null -eq $a -or $null -eq $b) { return ($null -eq $a -and $null -eq $b) }
  if ([string]$a.Kind -ne [string]$b.Kind) { return $false }
  if ([string]$a.Kind -eq "Binary") { return ([BitConverter]::ToString([byte[]]$a.Data) -eq [BitConverter]::ToString([byte[]]$b.Data)) }
  if ([string]$a.Kind -eq "MultiString") { return ((@($a.Data) -join "`n") -ceq (@($b.Data) -join "`n")) }
  return ([string]$a.Data -ceq [string]$b.Data)
}
function HkcuSub($name) { return $name.Substring("HKEY_CURRENT_USER\".Length) }
function ValueLabel($n) { if ($null -eq $n) { return "" }; if ($n -eq "") { return "(default)" }; return $n }
# One value of an open key, or null when the key has no value by that name.
function ValueIn($k, $valueName) {
  if ($null -eq $valueName -or -not (@($k.GetValueNames()) -contains $valueName)) { return $null }
  return [pscustomobject]@{ Kind = $k.GetValueKind($valueName); Data = $k.GetValue($valueName, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames) }
}
# One value read again just before a write. Ok says the read itself worked.
function RegValueNow($keyName, $valueName) {
  $r = [pscustomobject]@{ Ok = $false; KeyExists = $false; Value = $null; Error = "" }
  try {
    $k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey((HkcuSub $keyName), $false)
    if ($null -eq $k) { $r.Ok = $true; return $r }
    try {
      $r.KeyExists = $true
      $r.Value = ValueIn $k $valueName
      $r.Ok = $true
    } finally { $k.Close() }
  } catch { $r.Error = (Err $_.Exception) }
  return $r
}
function TreeSame($a, $b) {
  if ($a.Count -ne $b.Count) { return $false }
  foreach ($k in $b.Keys) {
    if (-not $a.ContainsKey($k)) { return $false }
    $va = $a[$k]; $vb = $b[$k]
    if ($va.Count -ne $vb.Count) { return $false }
    foreach ($n in $vb.Keys) { if (-not $va.ContainsKey($n)) { return $false }; if (-not (RegSame $va[$n] $vb[$n])) { return $false } }
  }
  return $true
}
# Every difference between two reads, as lines to print and changes to put back. Each
# change carries what the compare read at the end, so a write can check it still reads so.
function DiffRegistry($before, $after, $root) {
  $d = [pscustomobject]@{ Lines = New-Object System.Collections.Generic.List[string]; Changes = New-Object System.Collections.Generic.List[object] }
  foreach ($f in $after.Failed.Keys) { $d.Lines.Add("could not read at the end " + (ShortKey $f $root) + ", " + $after.Failed[$f] + ". Nothing is written under it") }
  $keysAll = @(@($before.Read.Keys) + @($after.Read.Keys) | Sort-Object -Unique)
  foreach ($k in $keysAll) {
    $short = ShortKey $k $root
    $inB = $before.Read.ContainsKey($k)
    $inA = $after.Read.ContainsKey($k)
    $failA = UnderFailed $k $after.Failed
    if (-not $inB -and $inA) {
      $parent = ParentKey $k
      $vals = $after.Read[$k]
      $desc = (@($vals.Keys) | Sort-Object | ForEach-Object { (ValueLabel $_) + " = " + (RegText $vals[$_]) }) -join " | "
      if (-not $before.Read.ContainsKey($parent) -and $after.Read.ContainsKey($parent)) { $d.Lines.Add("key APPEARED " + $short + ", " + $vals.Count + " values: " + $desc + ". It goes with its parent"); continue }
      $ok = ($before.Read.ContainsKey($parent) -and $after.Read.ContainsKey($parent) -and -not (UnderFailed $parent $before.Failed) -and -not (UnderFailed $parent $after.Failed) -and -not $failA -and -not (FailedBelow $k $after.Failed))
      $tree = @{}
      foreach ($kk in $after.Read.Keys) { if ($kk -eq $k -or $kk.StartsWith($k + "\", [StringComparison]::OrdinalIgnoreCase)) { $tree[$kk] = $after.Read[$kk] } }
      $d.Lines.Add("key APPEARED " + $short + ", " + $vals.Count + " values: " + $desc + $(if ($ok) { "" } else { ". Not removable, it or its parent was not read at both ends" }))
      $d.Changes.Add([pscustomobject]@{ Op = "DeleteKey"; Key = $k; Name = $null; Old = $null; New = $null; Tree = $tree; Ok = $ok })
      continue
    }
    if ($inB -and -not $inA) {
      if ($failA) { $d.Lines.Add("key " + $short + " could not be read at the end, not compared, nothing written under it"); continue }
      $parent = ParentKey $k
      $ok = (-not (UnderFailed $parent $before.Failed) -and -not (UnderFailed $parent $after.Failed))
      $d.Lines.Add("key WENT " + $short + ", " + $before.Read[$k].Count + " values before" + $(if ($ok) { "" } else { ". Not writable, its parent was not read at both ends" }))
      $parentWent = ($before.Read.ContainsKey($parent) -and -not $after.Read.ContainsKey($parent))
      $d.Changes.Add([pscustomobject]@{ Op = "CreateKey"; Key = $k; Name = $null; Old = $null; New = $null; Tree = $null; Ok = $ok; ParentWent = $parentWent })
      foreach ($n in @($before.Read[$k].Keys | Sort-Object)) {
        $d.Lines.Add($short + "  " + (ValueLabel $n) + "  old " + (RegText $before.Read[$k][$n]) + "  new absent")
        $d.Changes.Add([pscustomobject]@{ Op = "Set"; Key = $k; Name = $n; Old = $before.Read[$k][$n]; New = $null; Tree = $null; Ok = $ok; KeyWent = $true })
      }
      continue
    }
    $bv = $before.Read[$k]; $av = $after.Read[$k]
    $ok = (-not $failA -and -not (UnderFailed $k $before.Failed))
    foreach ($n in @(@($bv.Keys) + @($av.Keys) | Sort-Object -Unique)) {
      if (RegSame $bv[$n] $av[$n]) { continue }
      $d.Lines.Add($short + "  " + (ValueLabel $n) + "  old " + (RegText $bv[$n]) + "  new " + (RegText $av[$n]) + $(if ($ok) { "" } else { ". Not writable, the key was not read at both ends" }))
      if ($null -eq $bv[$n]) { $d.Changes.Add([pscustomobject]@{ Op = "DeleteValue"; Key = $k; Name = $n; Old = $null; New = $av[$n]; Tree = $null; Ok = $ok; KeyWent = $false }) }
      else { $d.Changes.Add([pscustomobject]@{ Op = "Set"; Key = $k; Name = $n; Old = $bv[$n]; New = $av[$n]; Tree = $null; Ok = $ok; KeyWent = $false }) }
    }
  }
  return $d
}

# The settings folder, listed and hashed. A folder that cannot be listed is kept in Failed,
# and nothing is ever written under it.
function SettingsRead($dir) {
  $r = [pscustomobject]@{ Files = @{}; Failed = New-Object System.Collections.Generic.List[string]; Missing = $false; Messages = New-Object System.Collections.Generic.List[string] }
  if (-not (Test-Path -LiteralPath $dir)) { $r.Missing = $true; return $r }
  $ev = $null
  $files = @(Get-ChildItem -LiteralPath $dir -Recurse -File -Force -ErrorAction SilentlyContinue -ErrorVariable ev)
  foreach ($e in @($ev)) {
    if ($null -eq $e) { continue }
    $t = [string]$e.TargetObject
    if ($t -eq "") { $t = $dir }
    $r.Failed.Add($t)
    $r.Messages.Add("could not list " + $t + ", " + $e.Exception.Message)
  }
  foreach ($f in $files) {
    $rel = $f.FullName.Substring($dir.Length).TrimStart('\')
    $hash = $null
    try { $hash = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash } catch { $r.Messages.Add("could not hash " + $rel + ", " + (Err $_.Exception)) }
    $r.Files[$rel] = [pscustomobject]@{ Hash = $hash; Length = $f.Length; Write = $f.LastWriteTime }
  }
  return $r
}
function FolderFailed($path, $failed) {
  foreach ($f in $failed) { if ($path -eq $f -or $path.StartsWith($f.TrimEnd('\') + "\", [StringComparison]::OrdinalIgnoreCase)) { return $true } }
  return $false
}
function FileText($v) {
  if ($null -eq $v) { return "absent" }
  $hs = "unhashed"; if ($null -ne $v.Hash) { $hs = $v.Hash.Substring(0, 12) }
  return ([string]$v.Length + " bytes, sha256 " + $hs + ", written " + $v.Write.ToString("yyyy-MM-dd HH:mm:ss"))
}
function DiffFiles($filesBefore, $notBacked, $sAfter, $root) {
  $d = [pscustomobject]@{ Lines = New-Object System.Collections.Generic.List[string]; Changes = New-Object System.Collections.Generic.List[object] }
  foreach ($rel in @(@($filesBefore.Keys) + @($notBacked.Keys) + @($sAfter.Files.Keys | Where-Object { $_ -notlike "AutoSave\*" }) | Sort-Object -Unique)) {
    $b = $filesBefore[$rel]; $a = $sAfter.Files[$rel]
    if ($notBacked.ContainsKey($rel)) { $d.Lines.Add("file NOT BACKED UP at the start " + $rel + "  old " + (FileText $notBacked[$rel]) + "  new " + (FileText $a) + ". Never written"); continue }
    if ($null -eq $b) { $d.Lines.Add("file APPEARED " + $rel + ", " + (FileText $a) + ". Left in place, the loop deletes only from its own folder"); continue }
    if ($null -ne $a -and $null -eq $a.Hash) { $d.Lines.Add("file IN USE at the end " + $rel + "  old " + (FileText $b) + "  new " + (FileText $a) + ". Its content could not be read, so it is not compared and never written"); continue }
    if ($null -ne $a -and $a.Hash -eq $b.Hash) { continue }
    $dest = Join-Path $root $rel
    $ok = -not (FolderFailed (Split-Path $dest -Parent) $sAfter.Failed)
    $d.Lines.Add("file " + $(if ($null -eq $a) { "WENT" } else { "CHANGED" }) + " " + $rel + "  old " + (FileText $b) + "  new " + (FileText $a) + $(if ($ok) { "" } else { ". Not writable, its folder could not be listed at the end" }))
    $afterHash = $null; if ($null -ne $a) { $afterHash = $a.Hash }
    $d.Changes.Add([pscustomobject]@{ Rel = $rel; Dest = $dest; Went = ($null -eq $a); AfterHash = $afterHash; Old = $b; Ok = $ok })
  }
  return $d
}
function DiffAutoSave($autoBefore, $sAfter) {
  $lines = New-Object System.Collections.Generic.List[string]
  foreach ($rel in @(@($autoBefore.Keys) + @($sAfter.Files.Keys | Where-Object { $_ -like "AutoSave\*" }) | Sort-Object -Unique)) {
    $b = $autoBefore[$rel]; $a = $sAfter.Files[$rel]
    if ($null -ne $b -and $null -ne $a -and $null -ne $a.Hash -and $a.Hash -eq $b.Hash) { continue }
    $lines.Add("AutoSave " + $rel + "  old " + (FileText $b) + "  new " + (FileText $a) + ". Not backed up, never put back")
  }
  return ,$lines
}

# The window reader, in memory. EnumWindows needs a callback, so the delegate type is
# emitted too. It is compared by process id FIRST, so no window of any other process is
# read, and a message goes only to a child window of the process asked for.
function NewWinTypes {
$dynName = New-Object System.Reflection.AssemblyName("ProbeWinNative")
$dynAsm = [AppDomain]::CurrentDomain.DefineDynamicAssembly($dynName, [System.Reflection.Emit.AssemblyBuilderAccess]::Run)
$dynMod = $dynAsm.DefineDynamicModule("ProbeWinNative")
$dt = $dynMod.DefineType("ProbeEnumProc", [System.Reflection.TypeAttributes]"Public,Sealed,AutoClass", [System.MulticastDelegate])
$dctor = $dt.DefineConstructor([System.Reflection.MethodAttributes]"RTSpecialName,SpecialName,HideBySig,Public", [System.Reflection.CallingConventions]::Standard, [Type[]]@([object], [IntPtr]))
$dctor.SetImplementationFlags([System.Reflection.MethodImplAttributes]"Runtime,Managed")
$dinv = $dt.DefineMethod("Invoke", [System.Reflection.MethodAttributes]"Public,HideBySig,NewSlot,Virtual", [bool], [Type[]]@([IntPtr], [IntPtr]))
$dinv.SetImplementationFlags([System.Reflection.MethodImplAttributes]"Runtime,Managed")
$procType = $dt.CreateType()
$tb = $dynMod.DefineType("ProbeWin", [System.Reflection.TypeAttributes]"Public,Class,Abstract,Sealed")
# Two kernel32 calls beside them, so the constructor deadline can end this process with
# no managed shutdown, and so no finalizer runs against a Roamer nobody adopted.
foreach ($def in @(
    @("user32.dll", "EnumWindows", [bool], [Type[]]@($procType, [IntPtr])),
    @("user32.dll", "EnumChildWindows", [bool], [Type[]]@([IntPtr], $procType, [IntPtr])),
    @("user32.dll", "GetWindowThreadProcessId", [uint32], [Type[]]@([IntPtr], [uint32].MakeByRefType())),
    @("user32.dll", "IsWindowVisible", [bool], [Type[]]@([IntPtr])),
    @("user32.dll", "GetClassNameW", [int], [Type[]]@([IntPtr], [System.Text.StringBuilder], [int])),
    @("user32.dll", "GetWindowTextW", [int], [Type[]]@([IntPtr], [System.Text.StringBuilder], [int])),
    @("user32.dll", "SendMessageTimeoutW", [IntPtr], [Type[]]@([IntPtr], [uint32], [IntPtr], [System.Text.StringBuilder], [uint32], [uint32], [IntPtr].MakeByRefType())),
    @("kernel32.dll", "GetCurrentProcess", [IntPtr], [Type[]]@()),
    @("kernel32.dll", "TerminateProcess", [bool], [Type[]]@([IntPtr], [uint32])),
    # F103 for run.ps1: the owner of a dialog and whether it is enabled, the keep awake
    # request and the native thread it belongs to, the session lock, M6, and a key's write
    # time, M5. Each is read only, bar the keep awake request, which Windows drops by itself
    # when the thread that made it ends.
    @("user32.dll", "GetWindow", [IntPtr], [Type[]]@([IntPtr], [uint32])),
    @("user32.dll", "IsWindowEnabled", [bool], [Type[]]@([IntPtr])),
    @("kernel32.dll", "SetThreadExecutionState", [uint32], [Type[]]@([uint32])),
    @("kernel32.dll", "GetCurrentThreadId", [uint32], [Type[]]@()),
    @("wtsapi32.dll", "WTSQuerySessionInformationW", [bool], [Type[]]@([IntPtr], [int], [int], [IntPtr].MakeByRefType(), [uint32].MakeByRefType())),
    @("wtsapi32.dll", "WTSFreeMemory", [void], [Type[]]@([IntPtr])),
    @("advapi32.dll", "RegQueryInfoKeyW", [int], [Type[]]@([IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [IntPtr], [long].MakeByRefType())))) {
  $pm = $tb.DefinePInvokeMethod($def[1], $def[0], [System.Reflection.MethodAttributes]"Public,Static,PinvokeImpl,HideBySig", [System.Reflection.CallingConventions]::Standard, $def[2], $def[3], [System.Runtime.InteropServices.CallingConvention]::Winapi, [System.Runtime.InteropServices.CharSet]::Unicode)
  $pm.SetImplementationFlags($pm.GetMethodImplementationFlags() -bor [System.Reflection.MethodImplAttributes]::PreserveSig)
}
$winType = $tb.CreateType()
  return [pscustomobject]@{ ProcType = $procType; WinType = $winType }
}

function WinHandlesOf($winType, $procType, [uint32]$owner, [IntPtr]$parent) {
  $found = New-Object System.Collections.Generic.List[IntPtr]
  $cb = {
    param([IntPtr]$h, [IntPtr]$l)
    [uint32]$wp = 0
    [void]$winType::GetWindowThreadProcessId($h, [ref]$wp)
    if ($wp -eq $owner) { $found.Add($h) }
    return $true
  }.GetNewClosure()
  $del = [System.Management.Automation.LanguagePrimitives]::ConvertTo($cb, $procType)
  if ($parent -eq [IntPtr]::Zero) { [void]$winType::EnumWindows($del, [IntPtr]::Zero) } else { [void]$winType::EnumChildWindows($parent, $del, [IntPtr]::Zero) }
  [GC]::KeepAlive($del)
  return $found
}
# GetClassName and GetWindowText send no message into another process: for a window of
# another process GetWindowText reads the caption Windows keeps, measured by prove-run.ps1
# on a stand-in whose window thread is blocked. SendMessageTimeout with WM_GETTEXT does send
# one, and it is reached only when $allowMessages is true, which is only ever for the
# adopted Roamer. Since F103, with messages allowed, the text of the visible children of
# every visible top level window is read, not only of a #32770, so a WinForms or WPF dialog
# of Navisworks is read too. The Navisworks main window is left out, because its children
# are its panels and not a dialog's text. Each read has a 500 ms timeout that
# SMTO_ABORTIFHUNG cuts short once Windows counts the thread hung, at most 20 children of
# one window are read, and one call spends at most 2 s on them in all, so a pass of the
# watchdog or the monitor always reaches its deadline checks. What was not read is written.
function WindowRecords($winType, $procType, [uint32]$owner, [bool]$visibleOnly, [bool]$allowMessages) {
  $recs = New-Object System.Collections.Generic.List[object]
  $budget = [Diagnostics.Stopwatch]::StartNew()
  foreach ($h in (WinHandlesOf $winType $procType $owner ([IntPtr]::Zero))) {
    $vis = $winType::IsWindowVisible($h)
    if ($visibleOnly -and -not $vis) { continue }
    $c = New-Object System.Text.StringBuilder 256
    [void]$winType::GetClassNameW($h, $c, 256)
    $t = New-Object System.Text.StringBuilder 512
    [void]$winType::GetWindowTextW($h, $t, 512)
    $ownerWin = $winType::GetWindow($h, [uint32]4)
    $ownerOn = "none"
    if ($ownerWin -ne [IntPtr]::Zero) { $ownerOn = [string]$winType::IsWindowEnabled($ownerWin) }
    $rec = [pscustomobject]@{ Handle = $h; Class = $c.ToString(); Caption = $t.ToString(); Visible = $vis; Owner = [string]$ownerWin; OwnerHandle = $ownerWin; OwnerEnabled = $ownerOn; Texts = $null; TextNote = "" }
    if ($vis -and -not $allowMessages -and $rec.Class -eq "#32770") { $rec.TextNote = "not read, nothing is sent before adoption" }
    if ($vis -and $allowMessages -and (WindowKind $rec.Class $rec.Caption $ownerWin) -ne "MAIN") {
      $parts = @()
      $n = 0
      $late = 0
      foreach ($ch in (WinHandlesOf $winType $procType $owner $h)) {
        if (-not $winType::IsWindowVisible($ch)) { continue }
        $n++
        if ($n -gt 20) { continue }
        if ($budget.ElapsedMilliseconds + 500 -gt 2000) { $late++; continue }
        $sb = New-Object System.Text.StringBuilder 2048
        [IntPtr]$res = [IntPtr]::Zero
        [void]$winType::SendMessageTimeoutW($ch, [uint32]0x000D, [IntPtr]2048, $sb, [uint32]0x0002, [uint32]500, [ref]$res)
        $s = $sb.ToString().Replace("`r", " ").Replace("`n", " ").Trim()
        if ($s.Length -gt 0) { $parts += ("`"" + $s + "`"") }
      }
      if ($n -gt 20) { $parts += ("and " + ($n - 20) + " more visible children not read") }
      if ($late -gt 0) { $parts += ("and " + $late + " visible children not read, because the 2 s one pass may spend reading children was spent") }
      $rec.Texts = $parts
    }
    $recs.Add($rec)
  }
  return ,$recs
}
function WindowLines($winType, $procType, [uint32]$owner, [bool]$visibleOnly, [bool]$allowMessages) {
  $lines = New-Object System.Collections.Generic.List[string]
  foreach ($r in (WindowRecords $winType $procType $owner $visibleOnly $allowMessages)) {
    $line = "[" + $r.Class + "] `"" + $r.Caption + "`""
    if (-not $r.Visible) { $line += " hidden" }
    if ($r.TextNote -ne "") { $line += " text: " + $r.TextNote }
    elseif ($null -ne $r.Texts) { $line += " text: " + ($r.Texts -join " ") }
    $lines.Add($line)
  }
  return $lines
}
# What a top level window of the adopted Navisworks is: the tool's window, the Navisworks
# main window, its Working... progress dialog, or anything else, a DIALOG finding. The
# Working... dialog was measured on 2026-09-28, docs\history\scan.md 5z-d, and the main
# window's class and caption on 2026-09-29, tools\probes\automation-start-result-20260929.txt
# line 423. The main window is one with no owner, so a message box of Navisworks titled the
# same way, which has one, is a DIALOG. Whether the real main window has no owner is UNKNOWN
# until the first start, which writes MAIN or DIALOG for it. The tool's window is read off
# the design and is UNKNOWN until the first window start.
function WindowKind($class, $caption, $ownerHandle) {
  if ($class.StartsWith("HwndWrapper[Roamer.exe;ProgressDialog;")) { return "PROGRESS" }
  if ($class.StartsWith("HwndWrapper") -and $caption.StartsWith("Parsons NWC Federator")) { return "WINDOW" }
  if ($class.StartsWith("WindowsForms10") -and $caption.EndsWith("Autodesk Navisworks Manage 2025") -and $ownerHandle -eq [IntPtr]::Zero) { return "MAIN" }
  return "DIALOG"
}

function WindowsOf($id, $utcTicks) {
  # Only ever called for the adopted Roamer, so messages are allowed, and only once its pid
  # is read again with its start time, as the watchdog does, so a pid that went to another
  # process is never read or sent anything.
  $state = ProcState $id $utcTicks
  if ($state -ne "same") { Say ("  the windows of " + $id + " are NOT read, the pid reads " + $state + " against the adopted start ticks UTC " + $utcTicks); return }
  $vis = @(WindowLines $winType $procType ([uint32]$id) $true $true)
  $all = @(WindowLines $winType $procType ([uint32]$id) $false $true)
  $dialogs = @($vis | Where-Object { $_.StartsWith("[#32770]") })
  Say ("  visible top level windows of " + $id + ": " + $vis.Count + ", hidden: " + ($all.Count - $vis.Count) + ", dialogs of class #32770 visible: " + $dialogs.Count)
  foreach ($w in $vis) { Say ("    " + $w) }
}

# The state of a process id against start ticks read before, UTC: same, gone, other when the
# id now belongs to another process, or unreadable. Only same may ever be closed, and only
# gone counts as gone.
function ProcState($id, $utcTicks) {
  $p = Get-Process -Id $id -ErrorAction SilentlyContinue
  if ($null -eq $p) { return "gone" }
  $st = $null
  try { $st = $p.StartTime } catch { Say ("  the start time of pid " + $id + " could not be read, " + (Err $_.Exception) + ", so it is not closed"); return "unreadable" }
  if ($null -eq $st) { return "unreadable" }
  if ($null -ne $utcTicks -and (UtcTicks $st) -eq $utcTicks) { return "same" }
  return "other"
}

# F103. The adopted process is held through the handle AdoptStart opened, so Windows gives
# its pid to no other process while it is held, and every read below goes through that
# handle. State is same, gone, other, or unreadable with Why the reason, or none when
# nothing was adopted. HeldState is its State alone.
function HeldRead($proc, $ticks) {
  $r = [pscustomobject]@{ State = "none"; Why = "nothing was adopted" }
  if ($null -eq $proc) { return $r }
  try {
    if ($proc.HasExited) { $r.State = "gone"; $r.Why = "it has exited"; return $r }
    $now = UtcTicks $proc.StartTime
    if ($now -eq $ticks) { $r.State = "same"; $r.Why = "" } else { $r.State = "other"; $r.Why = "its start ticks read " + $now + " through the held handle, not " + $ticks }
  } catch { $r.State = "unreadable"; $r.Why = "the read through the held handle threw, " + (Err $_.Exception) }
  return $r
}
function HeldState($proc, $ticks) { return (HeldRead $proc $ticks).State }
# The text of a file another thread appends to, opened sharing read, write and delete, so a
# read never makes that append fail. File.ReadAllLines shares read only.
function ReadShared($path) {
  $fs = New-Object System.IO.FileStream($path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
  try { $sr = New-Object System.IO.StreamReader($fs); return $sr.ReadToEnd() } finally { $fs.Dispose() }
}
# THE ONE CLOSE. Kill on the Process the adoption holds, after its start ticks read equal
# through that same handle. .NET Framework's Process.Kill reuses the handle the object holds
# and throws when that process has exited, so it can never reach another process. Waits up
# to $waitSeconds for the process to end. Nothing else in the loop closes a Navisworks, bar
# CloseOwn after a run.ps1 died, which calls this too.
function CloseAdopted($proc, $ticks, $waitSeconds) {
  $r = [pscustomobject]@{ State = "none"; Text = "" }
  if ($null -eq $proc) { $r.Text = "nothing was adopted, so nothing is closed"; return $r }
  try {
    if ($proc.HasExited) { $r.State = "gone"; $r.Text = "pid " + $proc.Id + " has exited already, nothing to close"; return $r }
    $now = UtcTicks $proc.StartTime
    if ($now -ne $ticks) { $r.State = "other"; $r.Text = "pid " + $proc.Id + " reads start ticks " + $now + " through the held handle, not " + $ticks + ", so it is not closed"; return $r }
    $proc.Kill()
    if ($proc.WaitForExit([int]($waitSeconds * 1000))) { $r.State = "gone"; $r.Text = "pid " + $proc.Id + " closed through the held handle and gone" }
    else { $r.State = "same"; $r.Text = "pid " + $proc.Id + " was sent Kill through the held handle and still runs " + $waitSeconds + " s later" }
  } catch { $r.State = "unreadable"; $r.Text = "the close of pid " + $proc.Id + " through the held handle threw, " + (Err $_.Exception) }
  return $r
}

# What is known about one Roamer, read through the process list and Win32_Process only.
function RoamerRecord($p) {
  $r = [pscustomobject]@{ Id = $p.Id; Start = $null; Ticks = $null; Cmd = $null; CmdOk = $false; CmdNote = ""; Parent = "UNKNOWN"; Path = $null }
  try { $r.Start = $p.StartTime; if ($null -ne $r.Start) { $r.Ticks = UtcTicks $r.Start } } catch { Say ("  the start time of Roamer " + $p.Id + " could not be read, " + (Err $_.Exception)) }
  $cr = CimRead $p.Id
  if (-not $cr.Ok) { $r.CmdNote = "the Win32_Process read threw, " + $cr.Error; $r.Parent = "UNKNOWN, the read threw" }
  elseif ($null -eq $cr.Row) { $r.CmdNote = "it was gone when it was read"; $r.Parent = "UNKNOWN, it was gone when it was read" }
  else {
    $r.Cmd = $cr.Row.CommandLine; $r.CmdOk = ($null -ne $r.Cmd); $r.Path = $cr.Row.ExecutablePath
    if (-not $r.CmdOk) { $r.CmdNote = "its command line reads as nothing" }
    $r.Parent = ParentText $cr.Row (CimRead $cr.Row.ParentProcessId) $cr.Row.ParentProcessId
  }
  return $r
}

# No start is made while any Navisworks runs, and the code keeps that rule, not a person.
# Every process named Roamer, whatever its command line and whoever started it, is named by
# pid and start time, and a process list that cannot be read is named too. True means
# refuse. Step 2 calls it, and so does the last read before the constructor.
function RoamerRefusal {
  $lines = New-Object System.Collections.Generic.List[string]
  $ps = $null
  try { $ps = [System.Diagnostics.Process]::GetProcessesByName("Roamer") } catch { $lines.Add("the process list could not be read, " + (Err $_.Exception)) }
  foreach ($p in @($ps | Where-Object { $null -ne $_ } | Sort-Object Id)) {
    $st = ""
    try { $s = $p.StartTime; $st = "started " + $s.ToString("yyyy-MM-dd HH:mm:ss") + ", start ticks UTC " + (UtcTicks $s) } catch { $st = "its start time could not be read, " + (Err $_.Exception) }
    $lines.Add("Roamer pid " + $p.Id + ", " + $st + ". Not ours, never closed, attached to or sent anything")
  }
  foreach ($l in $lines) { Say ("  " + $l) }
  if ($lines.Count -eq 0) { Say "  no Roamer is running" }
  return ($lines.Count -gt 0)
}

# Every write of the put back lists the Roamers first. Then the key is opened again and the
# value read again, and the write happens only when both still read what the compare read.
# A value is written only through its key as it stands, never through CreateSubKey, so a key
# that went is never made again. A value of a key that went is written only into the key
# this put back made again. A key is made only under a parent that is there now, and when the
# compare read that parent as gone, only under one this put back made, so CreateSubKey never
# makes a parent with it.
function PutBackRegistry($changes, $root, $ListRoamers) {
  $r = [pscustomobject]@{ Done = 0; Failed = 0; Skipped = 0; Stopped = $false; ValueDeletes = 0; KeysMade = 0; KeysRemoved = 0; Written = New-Object System.Collections.Generic.List[object] }
  $made = @{}
  foreach ($c in $changes) {
    $label = (ShortKey $c.Key $root) + $(if ($null -ne $c.Name) { " " + (ValueLabel $c.Name) } else { "" })
    if ($r.Stopped -or -not $c.Ok) { $r.Skipped++; continue }
    $rs = @(& $ListRoamers)
    if ($rs.Count -gt 0) { $r.Stopped = $true; $r.Skipped++; Say ("  a Roamer is running just before the write of " + $label + ", pid " + (($rs | ForEach-Object { [string]$_.Id }) -join ", ") + ". It and every write after it are stopped"); continue }
    try {
      if ($c.Op -eq "Set" -or $c.Op -eq "DeleteValue") {
        if ($c.KeyWent -and -not $made.ContainsKey($c.Key)) { Say ("  " + $label + " not written, its key went and this put back did not make it again, so nothing is written into it"); $r.Skipped++; continue }
        $rk = $null
        try { $rk = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey((HkcuSub $c.Key), $true) } catch { Say ("  " + $label + " not written, its key could not be opened again, " + (Err $_.Exception)); $r.Skipped++; continue }
        if ($null -eq $rk) { Say ("  " + $label + " not written, its key is gone since " + $(if ($c.KeyWent) { "this put back made it" } else { "the compare read it" }) + ", and a key that went is never made again"); $r.Skipped++; continue }
        $skip = ""
        try {
          $now = $null
          try { $now = ValueIn $rk $c.Name } catch { $skip = "could not be read again, " + (Err $_.Exception) }
          if ($skip -eq "" -and -not (RegSame $now $c.New)) { $skip = "no longer reads what the compare read" }
          if ($skip -eq "") {
            if ($c.Op -eq "Set") { $rk.SetValue($c.Name, $c.Old.Data, $c.Old.Kind) }
            else {
              $rk.DeleteValue($c.Name, $false)
              if (@($rk.GetValueNames()) -contains $c.Name) { $skip = "no value was deleted" }
            }
          }
        } finally { $rk.Close() }
        if ($skip -ne "") { Say ("  " + $label + " not written, " + $skip); $r.Skipped++; continue }
        $r.Done++; $r.Written.Add($c)
        if ($c.Op -eq "DeleteValue") { $r.ValueDeletes++ }
      } elseif ($c.Op -eq "CreateKey") {
        $now = RegValueNow $c.Key $null
        if (-not $now.Ok) { Say ("  " + $label + " could not be read again, not made, " + $now.Error); $r.Skipped++; continue }
        if ($now.KeyExists) { Say ("  " + $label + " is there again, not made, and no value is written into it"); $r.Skipped++; continue }
        $parent = ParentKey $c.Key
        if ($c.ParentWent -and -not $made.ContainsKey($parent)) { Say ("  " + $label + " not made, its parent went and this put back did not make it again"); $r.Skipped++; continue }
        $pn = RegValueNow $parent $null
        if (-not $pn.Ok -or -not $pn.KeyExists) { Say ("  " + $label + " not made, its parent is gone or could not be read again, and a key is never made along with its parent"); $r.Skipped++; continue }
        $rk = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey((HkcuSub $c.Key)); $rk.Close()
        $made[$c.Key] = $true
        $r.KeysMade++; $r.Written.Add($c)
      } elseif ($c.Op -eq "DeleteKey") {
        $tree = RegRead (HkcuSub $c.Key)
        if ($tree.Missing) { Say ("  " + $label + " is gone already, nothing removed"); $r.Skipped++; continue }
        if ($tree.Failed.Count -gt 0) { Say ("  " + $label + " could not be read again, not removed"); $r.Skipped++; continue }
        if (-not (TreeSame $tree.Read $c.Tree)) { Say ("  " + $label + " no longer reads what the compare read, not removed"); $r.Skipped++; continue }
        [Microsoft.Win32.Registry]::CurrentUser.DeleteSubKeyTree((HkcuSub $c.Key), $false)
        $r.KeysRemoved++; $r.Written.Add($c)
      }
    } catch { $r.Failed++; Say ("  could not put back " + $label + ", " + (Err $_.Exception)) }
  }
  return $r
}
function RegVerify($written, $root) {
  $left = 0
  foreach ($c in $written) {
    $now = RegValueNow $c.Key $c.Name
    $differs = $true
    if ($now.Ok) {
      if ($c.Op -eq "Set") { $differs = -not (RegSame $now.Value $c.Old) }
      elseif ($c.Op -eq "DeleteValue") { $differs = ($null -ne $now.Value) }
      elseif ($c.Op -eq "CreateKey") { $differs = -not $now.KeyExists }
      elseif ($c.Op -eq "DeleteKey") { $differs = $now.KeyExists }
    }
    if ($differs) { $left++; Say ("  still different after putting back: " + (ShortKey $c.Key $root) + " " + (ValueLabel $c.Name)) }
  }
  return $left
}
function PutBackFiles($changes, $backupDir, $ListRoamers, $stopped) {
  $r = [pscustomobject]@{ Done = 0; Failed = 0; Skipped = 0; Stopped = $stopped }
  foreach ($fc in $changes) {
    if ($r.Stopped -or -not $fc.Ok) { $r.Skipped++; continue }
    $rs = @(& $ListRoamers)
    if ($rs.Count -gt 0) { $r.Stopped = $true; $r.Skipped++; Say ("  a Roamer is running just before the copy of " + $fc.Rel + ", pid " + (($rs | ForEach-Object { [string]$_.Id }) -join ", ") + ". It and every copy after it are stopped"); continue }
    try {
      if ($fc.Went) {
        if (Test-Path -LiteralPath $fc.Dest) { Say ("  " + $fc.Rel + " is back just before the copy, not written"); $r.Skipped++; continue }
        if (-not (Test-Path -LiteralPath (Split-Path $fc.Dest -Parent))) { Say ("  " + $fc.Rel + ", its folder went too, not written"); $r.Skipped++; continue }
      } else {
        if (-not (Test-Path -LiteralPath $fc.Dest)) { Say ("  " + $fc.Rel + " went just before the copy, not written"); $r.Skipped++; continue }
        $nowHash = (Get-FileHash -LiteralPath $fc.Dest -Algorithm SHA256).Hash
        if ($nowHash -ne $fc.AfterHash) { Say ("  " + $fc.Rel + " no longer holds what the compare read, not written"); $r.Skipped++; continue }
      }
      $src = Join-Path $backupDir $fc.Rel
      if (-not (Test-Path -LiteralPath $src)) { Say ("  the backup of " + $fc.Rel + " is not there, not written"); $r.Skipped++; continue }
      Copy-Item -LiteralPath $src -Destination $fc.Dest -Force
      $back = (Get-FileHash -LiteralPath $fc.Dest -Algorithm SHA256).Hash
      if ($back -eq $fc.Old.Hash) { Say ("  " + $fc.Rel + " put back, read back, sha256 matches the backup"); $r.Done++ } else { Say ("  " + $fc.Rel + " put back but the read back sha256 is " + $back.Substring(0, 12)); $r.Failed++ }
    } catch { Say ("  " + $fc.Rel + " could not be put back, " + (Err $_.Exception)); $r.Failed++ }
  }
  return $r
}

# E6, the probe's check that the caller is the script its own powershell.exe was started to
# run with -File, so a deadline's TerminateProcess can only ever end the caller's own process.
# The caller passes its own $PSCommandPath, because inside a function of a dot-sourced file
# that variable names the file the function is written in.
function OwnProcessRefusal($scriptPath, $refusals) {
$cl = [Environment]::GetCommandLineArgs()
$fileArg = $null
for ($i = 1; $i -lt $cl.Count - 1; $i++) { if ($cl[$i] -match '^[-/]File$') { $fileArg = $cl[$i + 1]; break } }
$hostName = (Get-Process -Id $PID).ProcessName
$fileFull = $null
if ($null -ne $fileArg) {
  try { $fileFull = [System.IO.Path]::GetFullPath($fileArg) } catch { $refusals.Add("the -File argument could not be read as a path, " + (Err $_.Exception)) }
}
$ownScript = ($hostName -eq "powershell" -and $null -ne $fileFull -and $fileFull -eq $scriptPath)
Say ("PROCESS   " + $hostName + ".exe pid " + $PID + ", started to run -File " + $(if ($null -ne $fileFull) { $fileFull } else { "none" }) + ", which is this script: " + $ownScript)
if (-not $ownScript) { $refusals.Add("this script is not the one its powershell.exe was started to run with -File, so the constructor deadline could end a caller's process") }
}

# Step 2's read of unproved-starts.txt. Stop is true when a start named there still runs, or
# the file or one of its lines cannot be read, and Text is the probe's STOP line.
function UnprovedRefusal($unproved) {
  $ur = [pscustomobject]@{ Stop = $false; Text = ""; Still = $null }
if (Test-Path -LiteralPath $unproved) {
  $ulines = $null
  try { $ulines = [System.IO.File]::ReadAllLines($unproved) } catch { $ur.Stop = $true; $ur.Text = ("STOP before the constructor: " + (Mask $unproved) + " could not be read, " + (Err $_.Exception) + ". A person has to look"); return $ur }
  $named = 0
  $still = New-Object System.Collections.Generic.List[string]
  foreach ($ul in $ulines) {
    if ($ul.Trim() -eq "" -or $ul.StartsWith("#")) { continue }
    $named++
    $parts = $ul.Split("`t")
    $upid = 0
    if ($parts.Count -lt 2 -or -not [int]::TryParse($parts[0], [ref]$upid)) { $ur.Stop = $true; $ur.Text = ("STOP before the constructor: a line of " + (Mask $unproved) + " cannot be read, `"" + $ul + "`". A person has to look"); return $ur }
    $up = Get-Process -Id $upid -ErrorAction SilentlyContinue
    if ($parts[1] -eq "Roamer" -or $parts[1] -eq "0") {
      if ($null -ne $up -and $up.ProcessName -eq "Roamer") {
        # A start runs before it is first seen, so a Roamer holding that pid that started
        # after the line's first seen time is another process. What cannot be read refuses.
        $first = [DateTime]::MinValue
        $firstOk = ($parts.Count -ge 3 -and [DateTime]::TryParseExact($parts[2], "o", [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind, [ref]$first))
        $ust = $null; $ustErr = "its start time reads as nothing"
        try { $ust = $up.StartTime } catch { $ustErr = "its start time could not be read, " + (Err $_.Exception) }
        if (-not $firstOk) { $still.Add("pid " + $upid + ", its start time never read, a process named Roamer holds that id now, and the line's first seen time cannot be read") }
        elseif ($null -eq $ust) { $still.Add("pid " + $upid + ", its start time never read, a process named Roamer holds that id now, and " + $ustErr) }
        elseif ((UtcTicks $ust) -gt $first.ToUniversalTime().Ticks) { Say ("  pid " + $upid + " is written down with no start time, first seen " + $first.ToUniversalTime().ToString("o") + ". The Roamer holding that id now started " + $ust.ToUniversalTime().ToString("o") + ", later, so it is another process and is not refused on this line") }
        else { $still.Add("pid " + $upid + ", its start time never read, and the process named Roamer holding that id now started " + $ust.ToUniversalTime().ToString("o") + ", not later than the line's first seen time " + $first.ToUniversalTime().ToString("o")) }
      }
      continue
    }
    $uticks = [long]0
    if (-not [long]::TryParse($parts[1], [ref]$uticks)) { $ur.Stop = $true; $ur.Text = ("STOP before the constructor: a line of " + (Mask $unproved) + " cannot be read, `"" + $ul + "`". A person has to look"); return $ur }
    if ($null -eq $up) { continue }
    $ust = $null
    try { $ust = $up.StartTime } catch { Say ("  the start time of pid " + $upid + " could not be read, " + (Err $_.Exception)) }
    if ($null -eq $ust) { $still.Add("pid " + $upid + ", start ticks " + $uticks + ", a process holds that id now and its start time cannot be read") }
    elseif ((UtcTicks $ust) -eq $uticks) { $still.Add("pid " + $upid + ", start ticks " + $uticks + ", still running") }
  }
  Say ("  " + (Mask $unproved) + " names " + $named + " starts this probe could not prove, still running: " + $still.Count)
  if ($still.Count -gt 0) {
    foreach ($s in $still) { Say ("    " + $s) }
    $ur.Stop = $true; $ur.Text = "STOP before the constructor: a start this probe could not prove is still running. A person has to look"; $ur.Still = $still
    return $ur
  }
} else { Say "  no start this probe could not prove is recorded" }
  return $ur
}

function NewRoamers {
  $r = @()
  foreach ($p in @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)) {
    $st = $null
    try { $st = $p.StartTime } catch { Say ("  the start time of Roamer " + $p.Id + " could not be read, " + (Err $_.Exception) + ", so it counts as new") }
    if ($null -ne $st -and $beforeTicks.ContainsKey($p.Id) -and $beforeTicks[$p.Id] -eq (UtcTicks $st)) { continue }
    $r += $p
  }
  return $r
}

# What the main thread and the watchdog share, made before the watchdog starts.
function WatchSync($beforeAll, $beforeTicks, $T0, $loopRoot, $nw, $ConstructorDeadlineSeconds, $AdoptedDeadlineSeconds, $Out, $unproved, $watchFile, $guardText, $winType, $procType) {
$sync = [hashtable]::Synchronized(@{})
$sync.Stop = $false
$sync.Before = New-Object 'System.Collections.Generic.HashSet[int]'
foreach ($id in $beforeAll) { [void]$sync.Before.Add([int]$id) }
$sync.BeforeRoamer = $beforeTicks
$sync.T0 = $T0
$sync.LoopRoot = $loopRoot
$sync.Nw = $nw
$sync.CallStartUtc = [DateTime]::MaxValue
$sync.CtorReturned = $false
$sync.CtorDeadline = $ConstructorDeadlineSeconds
$sync.AdoptedDeadline = $AdoptedDeadlineSeconds
$sync.AdoptedAtUtc = [DateTime]::MaxValue
$sync.MyPid = 0
$sync.MyTicks = [long]0
$sync.MyProc = $null
$sync.NoAdopt = $false
$sync.Forced = ""
$sync.DeadlineDone = $false
$sync.SettingsReady = $false
$sync.Out = $Out
$sync.Unproved = $unproved
$sync.WatchFile = $watchFile
$sync.GuardText = $guardText
$sync.WinType = $winType
$sync.ProcType = $procType
$sync.Seen = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
$sync.RoamerSet = [hashtable]::Synchronized(@{})
$sync.ZeroSightings = [hashtable]::Synchronized(@{})
$sync.WriteErrors = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
$sync.ErrorCount = 0
$sync.EarlyFails = 0
$sync.Passes = 0
$sync.PassMaxMs = 0.0
$sync.PassTotalMs = 0.0
$sync.MaxGapMs = 0.0
$sync.LastPassStart = $null
return $sync
}

# The watchdog. It runs on its own runspace, started with the text WatchdogScript returns,
# which dot-sources this file's text from $sync.GuardText and calls Watchdog.
function WatchdogScript { return 'param($sync) . ([scriptblock]::Create($sync.GuardText)); Watchdog $sync' }
function Watchdog($sync) {
  $loopRoot = $sync.LoopRoot
  $nw = $sync.Nw
  $utf = New-Object System.Text.UTF8Encoding($false)
  function W($t) {
    try { [System.IO.File]::AppendAllText($sync.WatchFile, ([DateTime]::Now.ToString("HH:mm:ss.fff") + "  t+" + ([DateTime]::Now - $sync.T0).TotalSeconds.ToString("0.0") + "s  " + $t + "`r`n"), $utf) }
    catch { [void]$sync.WriteErrors.Add($_.Exception.Message) }
  }
  function ToOut($t) {
    if ($sync.Out -eq "") { return }
    try { [System.IO.File]::AppendAllText($sync.Out, $t + "`r`n", $utf) } catch { [void]$sync.WriteErrors.Add($_.Exception.Message) }
  }
  # Win32_Process reads cached by pid and start. A read that failed is never cached, so it
  # is made again at the next pass.
  $rows = @{}
  function Row($id, $ticks) {
    $key = [string]$id + "|" + $ticks
    if ($ticks -ne 0 -and $rows.ContainsKey($key)) { return $rows[$key] }
    $cr = CimRead $id
    if (-not $cr.Ok) { $sync.ErrorCount = $sync.ErrorCount + 1; W ("watchdog error, Win32_Process for pid " + $id + " threw, read again at the next pass: " + $cr.Error) }
    elseif ($null -ne $cr.Row -and $ticks -ne 0) { $rows[$key] = $cr }
    return $cr
  }
  function StartOf($p) {
    $st = $null
    try { $st = $p.StartTime } catch { $sync.ErrorCount = $sync.ErrorCount + 1; W ("watchdog error, the start time of pid " + $p.Id + " could not be read: " + $_.Exception.Message); return $null }
    return $st
  }
  $seen = @{}
  $lastWin = @{}
  $watched = @{}
  $zeroLogged = @{}
  W "watchdog started"
  while (-not $sync.Stop) {
    $pass = [Diagnostics.Stopwatch]::StartNew()
    $passStart = [DateTime]::UtcNow
    if ($null -ne $sync.LastPassStart) { $gap = ($passStart - $sync.LastPassStart).TotalMilliseconds; if ($gap -gt $sync.MaxGapMs) { $sync.MaxGapMs = $gap } }
    $sync.LastPassStart = $passStart
    $loopDone = $false
    try {
      $nowUtc = [DateTime]::UtcNow
      $procs = @(Get-Process)
      $cands = @()
      $newRoamersNow = @()
      foreach ($p in $procs) {
        $isRoamer = ($p.ProcessName -eq "Roamer")
        $st = $null; $tk = [long]0
        if ($isRoamer) {
          $st = StartOf $p
          if ($null -ne $st) { $tk = UtcTicks $st }
          if ($tk -ne 0 -and $sync.BeforeRoamer.ContainsKey($p.Id) -and $sync.BeforeRoamer[$p.Id] -eq $tk) { continue }
          $newRoamersNow += $p
          if ($tk -eq 0) {
            # Counted by the settings rule, and looked at again at the next pass.
            $z = $sync.ZeroSightings[$p.Id]
            if ($null -eq $z) { $sync.ZeroSightings[$p.Id] = @($nowUtc, $nowUtc) } else { $sync.ZeroSightings[$p.Id] = @($z[0], $nowUtc) }
            if (-not $zeroLogged.ContainsKey($p.Id)) { $zeroLogged[$p.Id] = $true; W ("Roamer pid " + $p.Id + " seen with no readable start time, looked at again at the next pass") }
            continue
          }
          $key = [string]$p.Id + "|" + $tk
          if (-not $sync.RoamerSet.ContainsKey($key)) { $sync.RoamerSet[$key] = $nowUtc }
          if ($sync.CallStartUtc -ne [DateTime]::MaxValue -and $tk -ge $sync.CallStartUtc.Ticks) {
            $cr = Row $p.Id $tk
            if ($cr.Ok -and $null -ne $cr.Row -and (HoldsEmbedding $cr.Row.CommandLine)) { $cands += ,@($p.Id, $tk) }
          }
        } else {
          if ($sync.Before.Contains([int]$p.Id)) { continue }
          $key = [string]$p.Id
        }
        if ($seen.ContainsKey($key)) { continue }
        $seen[$key] = $true
        if ($p.ProcessName -notmatch '^(Roamer|Adsk|Autodesk|Navis|Lc|Genuine|AdSSO|WerFault|FNPLicensing|LMU)') { continue }
        # One process that cannot be recorded is named, and the pass goes on to the rest.
        try {
          if (-not $isRoamer) { $st = StartOf $p; if ($null -ne $st) { $tk = UtcTicks $st } }
          $cr = Row $p.Id $tk
          $parentText = "UNKNOWN, the read threw"; $cmd = $null
          if ($cr.Ok -and $null -ne $cr.Row) { $cmd = $cr.Row.CommandLine; $parentText = ParentText $cr.Row (CimRead $cr.Row.ParentProcessId) $cr.Row.ParentProcessId }
          elseif ($cr.Ok) { $parentText = "UNKNOWN, it was gone when it was read" }
          $form = ""
          if ($isRoamer) {
            if (AutomationForm $cmd) { $form = ", automation form " + $cmd }
            elseif ($cr.Ok -and $null -ne $cr.Row -and $null -ne $cmd -and -not (EmbeddingWord $cmd)) { $form = ", started by hand, left alone, not printed" }
            else { $form = ", not in the automation form, not printed" }
          }
          $stText = "UNKNOWN, it could not be read"; if ($null -ne $st) { $stText = $st.ToString("HH:mm:ss.fff") }
          $cmdText = "UNKNOWN, the read threw"; if ($cr.Ok) { $cmdText = WordText $cmd }
          [void]$sync.Seen.Add([pscustomobject]@{ Id = $p.Id; Name = $p.ProcessName; Ticks = $tk; Parent = $parentText })
          W ("new process " + $p.ProcessName + " pid " + $p.Id + ", parent " + $parentText + ", started " + $stText + ", command line holds the word embedding or automation: " + $cmdText + $form)
        } catch { $sync.ErrorCount = $sync.ErrorCount + 1; W ("watchdog error, new process " + $p.ProcessName + " pid " + $p.Id + " could not be recorded: " + $_.Exception.Message) }
      }
      $loopDone = $true
      # Windows. Before adoption only top level class names and captions, which send no
      # message. After adoption the adopted Roamer's dialog text too.
      $targets = @()
      $allow = $false
      if ($sync.MyPid -ne 0) { $targets += ,@($sync.MyPid, $sync.MyTicks); $allow = $true }
      elseif (-not $sync.NoAdopt) { $targets = $cands }
      foreach ($tg in $targets) {
        $id = $tg[0]
        $watched[$id] = $tg[1]
        $pp = Get-Process -Id $id -ErrorAction SilentlyContinue
        if ($null -eq $pp) { continue }
        $pst = StartOf $pp
        if ($null -eq $pst -or (UtcTicks $pst) -ne $tg[1]) { continue }
        $wins = (WindowLines $sync.WinType $sync.ProcType ([uint32]$id) $true $allow) -join " | "
        if ($wins -eq "") { $wins = "none" }
        if ($lastWin[$id] -ne $wins) { $lastWin[$id] = $wins; W ("Roamer " + $id + " visible top level windows: " + $wins) }
      }
      foreach ($id in @($watched.Keys)) {
        if ($lastWin[$id] -eq "GONE") { continue }
        $pp = Get-Process -Id $id -ErrorAction SilentlyContinue
        $gone = ($null -eq $pp)
        if (-not $gone) { $pst = StartOf $pp; $gone = ($null -ne $pst -and (UtcTicks $pst) -ne $watched[$id]) }
        if ($gone) { $lastWin[$id] = "GONE"; W ("Roamer " + $id + " has exited") }
      }
      # The constructor deadline. Nothing is adopted, so nothing is closed. Its block is
      # written once, with the settings changes, the possible starts still running are
      # written down, and this process ends itself with no managed shutdown.
      if (-not $sync.DeadlineDone -and -not $sync.CtorReturned -and $sync.MyPid -eq 0 -and $sync.CallStartUtc -ne [DateTime]::MaxValue -and ($nowUtc - $sync.CallStartUtc).TotalSeconds -gt $sync.CtorDeadline) {
        $sync.DeadlineDone = $true
        $block = New-Object System.Collections.Generic.List[string]
        $block.Add("")
        $block.Add("==== CONSTRUCTOR DEADLINE of " + $sync.CtorDeadline + " s passed with nothing adopted. Nothing is closed and nothing is put back ====")
        $ps = @(PossibleStarts $newRoamersNow $sync.RoamerSet $sync.ZeroSightings 0 0)
        $lines = @(); $said = @()
        foreach ($x in $ps) {
          if ($x.Kind -eq "hand") { $block.Add("  Roamer pid " + $x.Pid + " was started by hand, left alone, not written down"); continue }
          $lines += (UnprovedLine $x.Pid $x.Ticks $x.FirstSeen ("not adopted and still running at the constructor deadline, command line " + $x.Kind + " " + $x.Note).Trim())
          $said += ("Roamer pid " + $x.Pid + ", command line " + $x.Kind + ", still running and not adopted")
        }
        if ($ps.Count -eq 0) { $block.Add("  no Roamer new since step 2 is running") }
        if ($lines.Count -gt 0) { foreach ($l in (UnprovedWrite $sync.Unproved $lines $said)) { $block.Add("  " + $l) } }
        if ($sync.SettingsReady) {
          try {
            $block.Add("---- BADER'S SETTINGS, compared, NOTHING PUT BACK, the backup is kept in the work folder ----")
            $ra = RegRead $sync.RegSub
            $dr = DiffRegistry $sync.RegBefore $ra $sync.RegRoot
            foreach ($l in $dr.Lines) { $block.Add("  " + $l) }
            $block.Add("  registry: " + @($dr.Changes | Where-Object { $_.Op -ne "CreateKey" }).Count + " values or keys differ from the backup, nothing written")
            $sa = SettingsRead $sync.NwAppData
            foreach ($m in $sa.Messages) { $block.Add("  " + $m) }
            $df = DiffFiles $sync.FilesBefore $sync.NotBacked $sa $sync.NwAppData
            foreach ($l in $df.Lines) { $block.Add("  " + $l) }
            $block.Add("  files: " + $df.Changes.Count + " differ from the backup, nothing written")
            foreach ($l in (DiffAutoSave $sync.AutoBefore $sa)) { $block.Add("  " + $l) }
          } catch { $block.Add("  the settings compare stopped, " + $_.Exception.Message) }
        } else { $block.Add("  the settings backup was not finished, so nothing is compared") }
        W "CONSTRUCTOR DEADLINE passed, this process ends itself now with exit code 3"
        $block.Add("---- the watchdog's record ----")
        try { foreach ($l in [System.IO.File]::ReadAllLines($sync.WatchFile)) { $block.Add("  " + $l) } } catch { $block.Add("  the watchdog's record could not be read, " + $_.Exception.Message) }
        $block.Add("  this process ends itself now with exit code 3 through TerminateProcess")
        foreach ($l in $block) { ToOut $l }
        $term = $sync.WinType::TerminateProcess($sync.WinType::GetCurrentProcess(), [uint32]3)
        ToOut ("  TerminateProcess returned " + $term + ", so this process did not end. The watchdog stops here, and the main thread is left in the constructor")
        W ("TerminateProcess returned " + $term + ", the watchdog stops")
        $sync.Stop = $true
        break
      }
      # The adopted deadline, for the adopted Roamer only, closed through the held handle
      # after its start time is read again through it. run.ps1 calls this deadline the ceiling.
      if ($sync.Forced -eq "" -and $sync.MyPid -ne 0 -and ($nowUtc - $sync.AdoptedAtUtc).TotalSeconds -gt $sync.AdoptedDeadline) {
        # Written before the close, so a reader that finds the process gone already finds why.
        $sync.Forced = "the adopted deadline of " + $sync.AdoptedDeadline + " s passed, the close through the held handle has begun"
        $cr = CloseAdopted $sync.MyProc $sync.MyTicks 5
        $sync.Forced = "the adopted deadline of " + $sync.AdoptedDeadline + " s passed, " + $cr.Text
        W ($sync.Forced)
      }
    } catch {
      $sync.ErrorCount = $sync.ErrorCount + 1
      if (-not $loopDone) { $sync.EarlyFails = $sync.EarlyFails + 1 }
      W ("watchdog error: " + $_.Exception.Message)
    }
    $pass.Stop()
    $ms = $pass.Elapsed.TotalMilliseconds
    $sync.Passes = $sync.Passes + 1
    $sync.PassTotalMs = $sync.PassTotalMs + $ms
    if ($ms -gt $sync.PassMaxMs) { $sync.PassMaxMs = $ms }
    Start-Sleep -Milliseconds 500
  }
  W "watchdog stopped"
}

# The settings backup, before anything starts. Ok is false with Why the probe's STOP line when
# a step of it failed, and the backup stays in the work folder.
function BackupSettings($work, $regSub, $nwAppData, $fedLogs) {
  $bs = [pscustomobject]@{ Ok = $false; Why = ""; RegRoot = $null; RegFile = $null; RegBefore = $null; AppBackup = $null; FilesBefore = $null; AutoBefore = $null; NotBacked = $null; FedBefore = $null }
$regRoot = "HKEY_CURRENT_USER\" + $regSub
$regFile = Join-Path $work "hkcu-navisworks-manage-22.0-before.reg"
& reg.exe export ("HKCU\" + $regSub) $regFile /y | Out-Null
$regExit = $LASTEXITCODE
$regLen = 0
if (Test-Path -LiteralPath $regFile) { $regLen = (Get-Item -LiteralPath $regFile).Length }
Say ("  HKCU\" + $regSub + " exported to " + (Mask $regFile) + ", reg.exe exit " + $regExit + ", " + $regLen + " bytes")
if ($regExit -ne 0 -or $regLen -le 0) { $bs.Why = "STOP before the constructor: the registry export did not exit 0 or left no file that is not empty"; return $bs }
$regBefore = RegRead $regSub
$valCount = 0; foreach ($k in $regBefore.Read.Keys) { $valCount += $regBefore.Read[$k].Count }
Say ("  read key by key: " + $regBefore.Read.Count + " keys, " + $valCount + " values, " + $regBefore.Failed.Count + " keys that could not be read" + $(if ($regBefore.Missing) { ", the key is not there" } else { "" }))
if ($regBefore.Failed.Count -gt 0) {
  foreach ($f in $regBefore.Failed.Keys) { Say ("    could not read " + (ShortKey $f $regRoot) + ", " + $regBefore.Failed[$f]) }
  $bs.Why = "STOP before the constructor: a key under 22.0 could not be read at the start"; return $bs
}
$appBackup = Join-Path $work "appdata-before"
$filesBefore = @{}
$autoBefore = @{}
$notBacked = @{}
$sBefore = SettingsRead $nwAppData
foreach ($m in $sBefore.Messages) { Say ("  " + $m) }
if ($sBefore.Failed.Count -gt 0) { $bs.Why = "STOP before the constructor: a folder of the settings could not be listed at the start"; return $bs }
foreach ($rel in $sBefore.Files.Keys) {
  if ($rel -like "AutoSave\*") { $autoBefore[$rel] = $sBefore.Files[$rel]; continue }
  if ($null -eq $sBefore.Files[$rel].Hash) {
    # Its content could not be read, so another process holds it. It is never copied and
    # never written.
    $notBacked[$rel] = $sBefore.Files[$rel]
    Say ("  NOT BACKED UP " + $rel + ", its content could not be read at the start")
    continue
  }
  $to = Join-Path $appBackup $rel
  $copyHash = $null
  try {
    New-Item -ItemType Directory -Force -Path (Split-Path $to -Parent) | Out-Null
    Copy-Item -LiteralPath (Join-Path $nwAppData $rel) -Destination $to -Force
    $copyHash = (Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash
  } catch { $bs.Why = ("STOP before the constructor: the backup copy of " + $rel + " failed, " + (Err $_.Exception)); return $bs }
  if ($copyHash -ne $sBefore.Files[$rel].Hash) { $bs.Why = ("STOP before the constructor: the backup copy of " + $rel + " does not read back with the sha256 its source had"); return $bs }
  $filesBefore[$rel] = [pscustomobject]@{ Hash = $copyHash; Length = (Get-Item -LiteralPath $to).Length; Write = $sBefore.Files[$rel].Write }
}
Say ("  %APPDATA%\Autodesk\Navisworks Manage 2025: " + $filesBefore.Count + " files copied to the work folder and read back with their source's sha256, " + $notBacked.Count + " not backed up, " + $autoBefore.Count + " AutoSave files listed and not copied")
$fedBefore = SettingsRead $fedLogs
Say ("  the tool's own logs folder listed: " + $fedBefore.Files.Count + " files")
  $bs.RegRoot = $regRoot; $bs.RegFile = $regFile; $bs.RegBefore = $regBefore; $bs.AppBackup = $appBackup; $bs.FilesBefore = $filesBefore; $bs.AutoBefore = $autoBefore; $bs.NotBacked = $notBacked; $bs.FedBefore = $fedBefore
  $bs.Ok = $true
  return $bs
}

# The adoption, after the constructor returned or threw. Adopted is true only when all four
# conditions hold. When they do not, nothing is closed and nothing is called on the object,
# and its finalizer is suppressed when there is one.
function AdoptStart($err, $app, $sync, $pidFile) {
  $suppressed = $false
  $recs = @(foreach ($p in @(NewRoamers)) { RoamerRecord $p })
  Say ("  Roamer processes new since step 2: " + $recs.Count)
  foreach ($r in $recs) {
    $shown = "not in the automation form, not printed"
    if (AutomationForm $r.Cmd) { $shown = $r.Cmd }
    $kind = "a possible start, its command line holds embedding"
    if (-not $r.CmdOk) { $kind = "a possible start, " + $r.CmdNote }
    elseif (-not (EmbeddingWord $r.Cmd)) { $kind = "STARTED BY HAND, left alone, never written down" }
    Say ("    pid " + $r.Id + ", started " + $(if ($r.Start) { $r.Start.ToString("HH:mm:ss.fff") } else { "UNKNOWN" }) + ", parent " + $r.Parent + ", path " + (Mask $r.Path))
    Say ("    " + $kind + ", command line: " + $shown)
  }
  $possible = @($recs | Where-Object { -not $_.CmdOk -or (EmbeddingWord $_.Cmd) })
  $c1 = ($null -eq $err -and $null -ne $app)
  $c2 = ($possible.Count -eq 1)
  $c3 = ($c2 -and $null -ne $possible[0].Ticks -and $possible[0].Ticks -ge $sync.CallStartUtc.Ticks)
  $c4 = ($c2 -and $possible[0].CmdOk -and (HoldsEmbedding $possible[0].Cmd))
  Say ("  adopt only if all four: constructor returned without throwing " + $c1 + ", exactly one possible start new " + $c2 + ", it started at or after the call " + $c3 + ", its command line holds -Embedding " + $c4)
  if (-not ($c1 -and $c2 -and $c3 -and $c4)) {
    $sync.NoAdopt = $true
    if ($null -ne $app) { [GC]::SuppressFinalize($app); $suppressed = $true }
    Say ("  NOT ADOPTED. Nothing is closed and nothing is called on the object, not Dispose. " + $(if ($null -ne $app) { "Its finalizer is suppressed" } else { "There is no object, so there is no finalizer to suppress" }))
    Say "  every possible start still running at the end is written down then"
    return [pscustomobject]@{ Adopted = $false; C1 = $c1; C2 = $c2; C3 = $c3; C4 = $c4; Suppressed = $suppressed; Pid = 0; Start = $null; Ticks = $null }
  }
  $myPid = $possible[0].Id
  $myStart = $possible[0].Start
  $myTicks = $possible[0].Ticks
  # F103, the held handle. The Process object of the one start opens its handle, and the start
  # ticks are read again through it. While it is open Windows gives the pid to no other
  # process, and every close goes through it. A handle that cannot be opened, or ticks that
  # differ, and the start counts as gone: nothing is closed and nothing is called on it.
  $held = $null
  $heldWhy = ""
  try {
    $held = Get-Process -Id $myPid -ErrorAction Stop
    [void]$held.Handle
    $again = UtcTicks $held.StartTime
    if ($again -ne $myTicks) { $heldWhy = "its start ticks read " + $again + " through the handle, not " + $myTicks; $held = $null }
  } catch { $heldWhy = "its handle could not be opened, " + (Err $_.Exception); $held = $null }
  if ($null -eq $held) {
    $sync.NoAdopt = $true
    if ($null -ne $app) { [GC]::SuppressFinalize($app); $suppressed = $true }
    Say ("  NOT ADOPTED, the one possible start counts as gone, " + $heldWhy + ". Nothing is closed and nothing is called on the object. A finding")
    return [pscustomobject]@{ Adopted = $false; C1 = $c1; C2 = $c2; C3 = $c3; C4 = $c4; Suppressed = $suppressed; Pid = 0; Start = $null; Ticks = $null }
  }
  Say ("  the held handle is open on pid " + $myPid + ", and the start ticks read again through it are equal")
  $sync.MyProc = $held
  $sync.MyTicks = $myTicks
  $sync.AdoptedAtUtc = [DateTime]::UtcNow
  $sync.MyPid = $myPid
  try { [System.IO.File]::WriteAllText($pidFile, [string]$myPid + "`r`n" + [string]$myTicks + "`r`nadopted`r`n", $utf8) } catch { Say ("  could not write " + (Mask $pidFile) + ", " + (Err $_.Exception)) }
  return [pscustomobject]@{ Adopted = $true; C1 = $c1; C2 = $c2; C3 = $c3; C4 = $c4; Suppressed = $suppressed; Pid = $myPid; Start = $myStart; Ticks = $myTicks }
}

# At the end, every possible start still running, and the adopted Roamer if it still runs after
# every close path, written down to unproved-starts.txt for later runs to refuse on.
function UnprovedAtEnd($unproved, $myPid, $myTicks, $lateNew, $sync) {
  try {
    $endLines = @(); $endSaid = @()
    if ($myPid -ne 0) {
      $ms = ProcState $myPid $myTicks
      if ($ms -eq "same" -or $ms -eq "unreadable") {
        $endLines += (UnprovedLine $myPid $myTicks $null ("the adopted Roamer is still running after every close path, state " + $ms))
        $endSaid += ("the adopted Roamer " + $myPid + " reads " + $ms + " after every close path")
      }
    }
    foreach ($x in @(PossibleStarts $lateNew $sync.RoamerSet $sync.ZeroSightings $myPid $myTicks)) {
      if ($x.Kind -eq "hand") { Say ("  Roamer pid " + $x.Pid + " was started by hand, left alone, not written down"); continue }
      $endLines += (UnprovedLine $x.Pid $x.Ticks $x.FirstSeen ("not adopted and still running at the end, command line " + $x.Kind + " " + $x.Note).Trim())
      $endSaid += ("Roamer pid " + $x.Pid + ", command line " + $x.Kind + ", still running and not adopted")
    }
    if ($endLines.Count -gt 0) { foreach ($l in (UnprovedWrite $unproved $endLines $endSaid)) { Say ("  " + $l) } } else { Say "  none" }
  } catch { Say ("  the starts to write down could NOT be worked out, " + (Err $_.Exception).TrimEnd('.') + ". Nothing was written down here, so the next run will not refuse on a start this would have named, and a person has to look") }
}

# Every reason not to put Bader's settings back. The put back happens only when Why is empty.
function PutBackReasons($sync, $wpsErrors, $wpsEndError, $watchEndedEarly, $beforeTicks, $myPid, $myTicks, $goneAtUtc) {
    $why = New-Object System.Collections.Generic.List[string]
    if ($sync.Passes -le 0) { $why.Add("the watchdog made no pass, so no record shows that no other Navisworks ran") }
    if ($sync.ErrorCount -gt 0) { $why.Add("the watchdog wrote " + $sync.ErrorCount + " error lines") }
    if ($sync.EarlyFails -gt 0) { $why.Add([string]$sync.EarlyFails + " watchdog passes failed before their process loop") }
    if ($wpsErrors.Count -gt 0) { $why.Add("the watchdog's runspace holds " + $wpsErrors.Count + " errors") }
    if ($null -ne $wpsEndError) { $why.Add("the watchdog ended with an error") }
    if ($sync.WriteErrors.Count -gt 0) { $why.Add("the watchdog could not write " + $sync.WriteErrors.Count + " lines") }
    if ($sync.DeadlineDone) { $why.Add("the constructor deadline passed and its path ran, which ends the watchdog, so no record covers what came after") }
    if ($watchEndedEarly) { $why.Add("the watchdog had stopped before the end, so no record covers what came after it stopped") }
    foreach ($id in $beforeTicks.Keys) { $why.Add("Roamer " + $id + " was running at step 2") }
    if ($myPid -eq 0) { $why.Add("no Navisworks was adopted, so which change is the probe's is UNKNOWN") }
    else {
      $myState = ProcState $myPid $myTicks
      if ($myState -eq "gone" -and $null -eq $goneAtUtc) { $goneAtUtc = [DateTime]::UtcNow }
      if ($myState -ne "gone") { $why.Add("the adopted Roamer " + $myPid + " reads " + $myState + ", not gone, so the probe's own Navisworks may still be running") }
    }
    foreach ($key in @($sync.RoamerSet.Keys)) {
      $kp = $key.Split('|')
      $sid = [int]$kp[0]; $stk = [long]$kp[1]
      if ($myPid -ne 0 -and $sid -eq $myPid -and $stk -eq $myTicks) { continue }
      $why.Add("Roamer " + $sid + " started " + ([DateTime]::new($stk, [DateTimeKind]::Utc)).ToLocalTime().ToString("HH:mm:ss.fff") + " was new at a watchdog pass and is not the adopted one")
    }
    foreach ($zid in @($sync.ZeroSightings.Keys)) {
      $z = $sync.ZeroSightings[$zid]
      if ($myPid -ne 0 -and [int]$zid -eq $myPid -and $null -ne $goneAtUtc -and $z[0].Ticks -ge $myTicks -and $z[1] -le $goneAtUtc) { continue }
      $why.Add("Roamer " + $zid + " was seen at a watchdog pass with no readable start time, and cannot be shown to be the adopted one")
    }
    foreach ($p in @(Get-Process -Name Roamer -ErrorAction SilentlyContinue)) { $why.Add("Roamer " + $p.Id + " is running at the end") }
  return [pscustomobject]@{ Why = $why; GoneAtUtc = $goneAtUtc }
}
# The compare, and the put back when $putBack is true, each write after a last check.
function SettingsPutBack($putBack, $why, $work, $regSub, $regBefore, $regRoot, $nwAppData, $filesBefore, $notBacked, $autoBefore, $appBackup) {
    if ($putBack) { Say "  the watchdog's record shows no other Navisworks ran from the backup to now, and the adopted one is gone, so what changed is put back, each write after a last check" }
    else {
      Say "  NOTHING IS PUT BACK AND NOTHING IS WRITTEN, because:"
      foreach ($w in $why) { Say ("    " + $w) }
      Say ("  the backup is kept in " + (Mask $work))
    }
    $ListRoamers = { @(Get-Process -Name Roamer -ErrorAction SilentlyContinue) }
    $regAfter = RegRead $regSub
    $dr = DiffRegistry $regBefore $regAfter $regRoot
    foreach ($l in $dr.Lines) { Say ("  " + $l) }
    Say ("  registry: " + @($dr.Changes | Where-Object { $_.Op -ne "CreateKey" }).Count + " values or keys differ from the backup")
    $regStopped = $false
    if ($putBack) {
      $pr = PutBackRegistry $dr.Changes $regRoot $ListRoamers
      $regStopped = $pr.Stopped
      $left = RegVerify $pr.Written $regRoot
      Say ("  registry: " + $pr.Done + " values put back of which " + $pr.ValueDeletes + " were deletes of a value that appeared, " + $pr.KeysMade + " keys made again, " + $pr.KeysRemoved + " keys removed, " + $pr.Failed + " failed, " + $pr.Skipped + " not written, stopped by a Roamer " + $pr.Stopped + ", and read again " + $left + " still differ")
    } else { Say "  registry: nothing written" }
    $sAfter = SettingsRead $nwAppData
    foreach ($m in $sAfter.Messages) { Say ("  " + $m) }
    $df = DiffFiles $filesBefore $notBacked $sAfter $nwAppData
    foreach ($l in $df.Lines) { Say ("  " + $l) }
    Say ("  files: " + $df.Changes.Count + " differ from the backup")
    if ($putBack) {
      $pf = PutBackFiles $df.Changes $appBackup $ListRoamers $regStopped
      Say ("  files: " + $pf.Done + " put back and matching, " + $pf.Failed + " failed, " + $pf.Skipped + " not written, stopped by a Roamer " + $pf.Stopped)
    } else { Say "  files: nothing written" }
    $autoLines = DiffAutoSave $autoBefore $sAfter
    foreach ($l in $autoLines) { Say ("  " + $l) }
    Say ("  AutoSave files added, changed, gone or unreadable: " + $autoLines.Count)
    # F103, what run.ps1 reads for its exit code. Differ counts every difference found, a file
    # that appeared, one that could not be read at the end and one not backed up whose size or
    # write time changed included. NotWritten counts those left as they are.
    $appeared = @($df.Lines | Where-Object { $_.StartsWith("file APPEARED") }).Count
    $inUse = @($df.Lines | Where-Object { $_.StartsWith("file IN USE") }).Count
    $nbChanged = 0
    foreach ($rel in @($notBacked.Keys)) { $na = $sAfter.Files[$rel]; $nb = $notBacked[$rel]; if ($null -eq $na -or $na.Length -ne $nb.Length -or $na.Write -ne $nb.Write) { $nbChanged++ } }
    $differ = $dr.Changes.Count + $df.Changes.Count + $appeared + $inUse + $nbChanged
    $notWritten = $differ
    if ($putBack) { $notWritten = $pr.Failed + $pr.Skipped + $left + $pf.Failed + $pf.Skipped + $appeared + $inUse + $nbChanged }
    return [pscustomobject]@{ PutBack = $putBack; Differ = $differ; NotWritten = $notWritten; AutoSave = $autoLines.Count }
}
