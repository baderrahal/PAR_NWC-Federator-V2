# The IL reader and the helpers the three F105 probes share, dot-sourced by
# probe-roamer-switches.ps1, probe-viewpoint-calls.ps1 and probe-clash-report-api.ps1, so
# each helper is written once. It is not a probe and prints nothing when it is dot-sourced.
#
#   TypeName        a type's name with its generic arguments
#   IlReason        what the runtime said, unwrapped from PowerShell's own exception
#   IlRead          a method body decoded, see below
#   IlFail          keeps one read that could not be made, by kind
#   IlFailureLines  prints every kind with its count and each failure
#   LoaderLines     prints and keeps what GetTypes could not load
#   StringRuns      every ASCII and UTF-16 string of four or more printable characters
#
# The IL reader follows the approach of probe-automation-start.ps1, the start probe F100
# merged, which keeps its own copy because that copy is the measurement F100 merged.
#
# IlRead decodes a method body opcode by opcode, so an operand byte is never read as an
# opcode, and resolves every token it meets. It returns one entry per instruction with
#   Offset, Name, OperandType, Token, Text, Member, Failed
# NOTHING IT CANNOT READ IS DROPPED. A body that cannot be read, an opcode byte it does not
# know and a token that does not resolve each become an entry with Failed set to true AND a
# line in $IlFailures, kept once each, with its kind, the method and its signature, the
# offset and what the runtime said. A caller may skip a failed entry, and IlFailureLines
# still counts and prints it. A caller that reads something itself and fails records it
# with IlFail. An opcode byte counts as known only when it is a real instruction: the
# reserved bytes 0xF8 to 0xFD and 0xFF, which OpCodes lists as Prefix2 to Prefix7 and
# Prefixref, are not, and are counted as unknown. 0xFE starts every two byte opcode.
#
# Reading IL runs no code in the assembly it comes from, when the assembly was loaded with
# ReflectionOnlyLoadFrom, which is how the three probes load every assembly.

$IlOp1 = @{}; $IlOp2 = @{}
foreach ($ilField in [System.Reflection.Emit.OpCodes].GetFields("Public,Static")) {
  $ilCode = $ilField.GetValue($null)
  if ($ilCode.OpCodeType -eq [System.Reflection.Emit.OpCodeType]::Nternal) { continue }
  $ilValue = [int]$ilCode.Value
  if ($ilCode.Size -eq 1) { $IlOp1[$ilValue -band 0xFF] = $ilCode } else { $IlOp2[$ilValue -band 0xFF] = $ilCode }
}
$IlFailures = New-Object System.Collections.Generic.List[object]
$IlFailureSeen = New-Object 'System.Collections.Generic.HashSet[string]'

# The table as it stands, for a probe to print: how many one byte and two byte instructions
# it knows, and whether any reserved byte is among them.
function IlOpcodeLine {
  $reserved = @(0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFF)
  $held = @($reserved | Where-Object { $IlOp1.ContainsKey($_) })
  return ("opcode table: " + $IlOp1.Count + " one byte and " + $IlOp2.Count + " two byte instructions. Reserved bytes 0xF8 to 0xFD and 0xFF among them: " + $held.Count + ", so each reads as not an instruction")
}

function TypeName($t) {
  if ($null -eq $t) { return "null" }
  if ($t.IsByRef) { return (TypeName $t.GetElementType()) + "&" }
  if ($t.IsArray) { return (TypeName $t.GetElementType()) + "[]" }
  if ($t.IsGenericType) {
    $ga = @(); foreach ($g in $t.GetGenericArguments()) { $ga += (TypeName $g) }
    $tn = $t.Name; $tick = $tn.IndexOf('`'); if ($tick -ge 0) { $tn = $tn.Substring(0, $tick) }
    return $t.Namespace + "." + $tn + "<" + ($ga -join ", ") + ">"
  }
  if ($t.IsGenericParameter) { return $t.Name }
  return $t.FullName
}

# A .NET exception thrown inside a method PowerShell called arrives wrapped. The runtime's
# own exception is the one worth printing.
function IlUnwrap($ex) {
  if ($ex -is [System.Management.Automation.MethodInvocationException] -and $null -ne $ex.InnerException) { return $ex.InnerException }
  return $ex
}
function IlReason($ex) { $x = IlUnwrap $ex; return ($x.GetType().Name + ": " + $x.Message) }

# The kinds IlFailureLines prints, in its order. A kind not named here is printed after them.
$IlFailureKinds = [ordered]@{
  "body"      = "IL bodies that could not be read"
  "opcode"    = "opcode bytes that are not an instruction, each ending the read of its body"
  "token"     = "tokens that did not resolve"
  "memberref" = "MemberRef rows that did not resolve"
  "methods"   = "types whose methods could not be listed"
  "members"   = "types whose members could not be listed"
  "loader"    = "assemblies where GetTypes could not load every type"
  "metadata"  = "metadata that could not be read"
  "signature" = "signatures that could not be read"
  "locals"    = "local variable lists that could not be read"
  "field"     = "field types that could not be read"
  "value"     = "constant values that could not be read"
}
# Where a failure happened. A method is named with its declaring type AND its parameter
# types, so two overloads that fail for one reason are two failures and not one.
function IlWhere($method) {
  if ($null -eq $method) { return "null" }
  if ($method -is [string]) { return $method }
  if ($method -is [System.Type]) { return (TypeName $method) }
  $ps = ""
  try { $ps = (@($method.GetParameters() | ForEach-Object { TypeName $_.ParameterType }) -join ", ") } catch { $ps = "parameters unreadable, " + (IlReason $_.Exception) }
  return ((TypeName $method.DeclaringType) + "::" + $method.Name + "(" + $ps + ")")
}
function IlFail($kind, $method, $at, $what) {
  $where = IlWhere $method
  # Cast before comparing: -1 passed in argument mode can arrive as the string "-1", and a
  # string compared with 0 is compared as text, which reads "-1" as not below "0".
  $offset = [int]$at
  if ($offset -ge 0) { $where += " IL_" + $offset.ToString("X4") }
  if ($IlFailureSeen.Add($kind + "|" + $where + "|" + $what)) { $IlFailures.Add([pscustomobject]@{ Kind = $kind; Where = $where; What = $what }) }
  return ($kind + " not read, " + $what)
}
function IlFailureLines($indent) {
  $kinds = @($IlFailureKinds.Keys) + @($IlFailures | ForEach-Object { $_.Kind } | Where-Object { -not $IlFailureKinds.Contains($_) } | Sort-Object -Unique)
  foreach ($k in $kinds) {
    $these = @($IlFailures | Where-Object { $_.Kind -eq $k })
    $label = $k; if ($IlFailureKinds.Contains($k)) { $label = $IlFailureKinds[$k] }
    Write-Output ($indent + $label + ": " + $these.Count)
    foreach ($x in $these) { Write-Output ($indent + "  " + $x.Where + ": " + $x.What) }
  }
}

# What GetTypes could not load, said rather than dropped: how many, and each distinct
# reason, printed where it happened and kept as one failure of kind loader for the probe's
# own count.
function LoaderLines($label, $ex) {
  $all = @($ex.Types)
  $bad = @($all | Where-Object { $null -eq $_ }).Count
  Write-Output ("  UNKNOWN in part: " + $bad + " of the " + $all.Count + " types of " + $label + " could not be loaded for reflection, " + ($all.Count - $bad) + " were read. The reasons, each once:")
  $seen = @{}; $reasons = @()
  foreach ($le in @($ex.LoaderExceptions)) { if ($null -eq $le) { continue }; $t = IlReason $le; if (-not $seen.ContainsKey($t)) { $seen[$t] = $true; $reasons += $t; Write-Output ("    " + $t) } }
  $null = IlFail "loader" $label -1 ($bad.ToString() + " of " + $all.Count + " types not loaded. " + ($reasons -join " | "))
}

# Every run of printable characters in a file's bytes, ASCII one byte each or UTF-16 LE two
# bytes each. The bytes are mapped one to one onto chars through Latin-1, so a match's
# index IS its byte offset. Returned in the order ASCII first, then UTF-16, each in file
# order, with Offset, Enc and Text, the UTF-16 text without its zero bytes.
$Latin1 = [System.Text.Encoding]::GetEncoding(28591)
$StringRunPatterns = [ordered]@{ "ascii" = "[\x20-\x7E]{4,}"; "utf16" = "(?:[\x20-\x7E]\x00){4,}" }
function StringRuns([byte[]]$bytes) {
  $text = $Latin1.GetString($bytes)
  $list = New-Object System.Collections.Generic.List[object]
  foreach ($enc in $StringRunPatterns.Keys) {
    foreach ($m in [regex]::Matches($text, $StringRunPatterns[$enc])) {
      $v = $m.Value; if ($enc -eq "utf16") { $v = $v.Replace([string][char]0, "") }
      $list.Add([pscustomobject]@{ Offset = $m.Index; Enc = $enc; Text = $v })
    }
  }
  return ,$list
}

function IlRead($method) {
  $list = New-Object System.Collections.Generic.List[object]
  $body = $null
  try { $body = $method.GetMethodBody() } catch {
    $t = IlFail "body" $method -1 (IlReason $_.Exception)
    $list.Add([pscustomobject]@{ Offset = -1; Name = "error"; OperandType = ""; Token = 0; Text = $t; Member = $null; Failed = $true })
    return ,$list
  }
  if ($null -eq $body) { $list.Add([pscustomobject]@{ Offset = -1; Name = "nobody"; OperandType = ""; Token = 0; Text = "no IL body: native, abstract or runtime implemented"; Member = $null; Failed = $false }); return ,$list }
  $il = $body.GetILAsByteArray(); $mod = $method.Module; $pos = 0
  while ($pos -lt $il.Length) {
    $at = $pos; $b = $il[$pos]
    if ($b -eq 0xFE) { $oc = $null; if ($pos + 1 -lt $il.Length) { $oc = $IlOp2[[int]$il[$pos + 1]] }; $pos += 2 } else { $oc = $IlOp1[[int]$b]; $pos += 1 }
    if ($null -eq $oc) {
      $byteText = "0x" + ([int]$b).ToString("X2"); if ($b -eq 0xFE -and $at + 1 -lt $il.Length) { $byteText = "0xFE 0x" + ([int]$il[$at + 1]).ToString("X2") }
      $t = IlFail "opcode" $method $at ("opcode byte " + $byteText + " is not an instruction, the rest of this body was not read")
      $list.Add([pscustomobject]@{ Offset = $at; Name = "error"; OperandType = ""; Token = 0; Text = $t; Member = $null; Failed = $true })
      break
    }
    $ot = $oc.OperandType.ToString()
    $text = ""; $member = $null; $tok = 0; $failed = $false
    switch ($ot) {
      "InlineNone" { }
      "ShortInlineBrTarget" { $d = [int]$il[$pos]; if ($d -gt 127) { $d -= 256 }; $pos += 1; $text = "IL_" + ($pos + $d).ToString("X4"); $member = ($pos + $d) }
      "InlineBrTarget" { $d = [BitConverter]::ToInt32($il, $pos); $pos += 4; $text = "IL_" + ($pos + $d).ToString("X4"); $member = ($pos + $d) }
      "ShortInlineI" { $d = [int]$il[$pos]; if ($d -gt 127) { $d -= 256 }; $text = [string]$d; $pos += 1 }
      "ShortInlineVar" { $text = [string]$il[$pos]; $pos += 1 }
      "InlineVar" { $text = [string][BitConverter]::ToUInt16($il, $pos); $pos += 2 }
      "InlineI" { $text = [string][BitConverter]::ToInt32($il, $pos); $pos += 4 }
      "InlineI8" { $text = [string][BitConverter]::ToInt64($il, $pos); $pos += 8 }
      "InlineR" { $text = [string][BitConverter]::ToDouble($il, $pos); $pos += 8 }
      "ShortInlineR" { $text = [string][BitConverter]::ToSingle($il, $pos); $pos += 4 }
      "InlineSwitch" { $n = [BitConverter]::ToInt32($il, $pos); $pos += 4 + 4 * $n; $text = "switch of " + $n }
      "InlineString" { $tok = [BitConverter]::ToInt32($il, $pos); $pos += 4; try { $member = $mod.ResolveString($tok); $text = "`"" + $member + "`"" } catch { $failed = $true; $text = IlFail "token" $method $at ("string token 0x" + $tok.ToString("X8") + ", " + (IlReason $_.Exception)) } }
      "InlineMethod" { $tok = [BitConverter]::ToInt32($il, $pos); $pos += 4; try { $member = $mod.ResolveMethod($tok); $text = (TypeName $member.DeclaringType) + "::" + $member.Name } catch { $failed = $true; $text = IlFail "token" $method $at ("method token 0x" + $tok.ToString("X8") + ", " + (IlReason $_.Exception)) } }
      "InlineField" { $tok = [BitConverter]::ToInt32($il, $pos); $pos += 4; try { $member = $mod.ResolveField($tok); $text = (TypeName $member.DeclaringType) + "::" + $member.Name } catch { $failed = $true; $text = IlFail "token" $method $at ("field token 0x" + $tok.ToString("X8") + ", " + (IlReason $_.Exception)) } }
      "InlineType" { $tok = [BitConverter]::ToInt32($il, $pos); $pos += 4; try { $member = $mod.ResolveType($tok); $text = TypeName $member } catch { $failed = $true; $text = IlFail "token" $method $at ("type token 0x" + $tok.ToString("X8") + ", " + (IlReason $_.Exception)) } }
      "InlineTok" {
        $tok = [BitConverter]::ToInt32($il, $pos); $pos += 4
        # A ldtoken names a type, a field or a method. The type is tried first, and only
        # when both reads fail is it a failure, with the second one's reason.
        try { $member = $mod.ResolveType($tok); $text = "type " + (TypeName $member) } catch {
          try { $member = $mod.ResolveMember($tok); $text = [string]$member } catch { $failed = $true; $text = IlFail "token" $method $at ("member token 0x" + $tok.ToString("X8") + ", " + (IlReason $_.Exception)) }
        }
      }
      "InlineSig" { $tok = [BitConverter]::ToInt32($il, $pos); $pos += 4; $text = "signature token 0x" + $tok.ToString("X8") }
      default { $pos += 4; $text = "operand " + $ot + " not decoded" }
    }
    $list.Add([pscustomobject]@{ Offset = $at; Name = $oc.Name; OperandType = $ot; Token = $tok; Text = $text; Member = $member; Failed = $failed })
  }
  return ,$list
}
