# The IL reader the two F105 probes dot-source, probe-roamer-switches.ps1 and
# probe-viewpoint-calls.ps1, written once so both decode IL the same way. It is not a probe
# and prints nothing when it is dot-sourced.
#
# It follows the approach of probe-automation-start.ps1, the start probe F100 merged, which
# keeps its own copy because that copy is the measurement F100 merged.
#
# IlRead decodes a method body opcode by opcode, so an operand byte is never read as an
# opcode, and resolves every token it meets. It returns one entry per instruction with
#   Offset, Name, OperandType, Token, Text, Member, Failed
# NOTHING IT CANNOT READ IS DROPPED. A body that cannot be read, an opcode byte it does not
# know and a token that does not resolve each become an entry with Failed set to true AND a
# line in $IlFailures, kept once each, with its kind, the method, the offset and what the
# runtime said. A caller may skip a failed entry, and IlFailureLines still counts and prints
# it. A caller that reads a body or lists methods itself records its own failure with IlFail.
#
# Reading IL runs no code in the assembly it comes from, when the assembly was loaded with
# ReflectionOnlyLoadFrom, which is how both probes load every assembly.

$IlOp1 = @{}; $IlOp2 = @{}
foreach ($ilField in [System.Reflection.Emit.OpCodes].GetFields("Public,Static")) {
  $ilCode = $ilField.GetValue($null); $ilValue = [int]$ilCode.Value
  if ($ilCode.Size -eq 1) { $IlOp1[$ilValue -band 0xFF] = $ilCode } else { $IlOp2[$ilValue -band 0xFF] = $ilCode }
}
$IlFailures = New-Object System.Collections.Generic.List[object]
$IlFailureSeen = New-Object 'System.Collections.Generic.HashSet[string]'

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
  "opcode"    = "opcode bytes the reader does not know, each ending the read of its body"
  "token"     = "tokens that did not resolve"
  "methods"   = "types whose methods could not be listed"
  "signature" = "signatures that could not be read"
  "locals"    = "local variable lists that could not be read"
  "field"     = "field types that could not be read"
}
function IlFail($kind, $method, $at, $what) {
  $where = "null"
  if ($method -is [System.Type]) { $where = TypeName $method }
  elseif ($null -ne $method) { $where = (TypeName $method.DeclaringType) + "::" + $method.Name }
  if ($at -ge 0) { $where += " IL_" + ([int]$at).ToString("X4") }
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
      $t = IlFail "opcode" $method $at ("unknown opcode byte 0x" + ([int]$b).ToString("X2") + ", the rest of this body was not read")
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
