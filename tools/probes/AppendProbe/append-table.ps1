param(
  [Parameter(Mandatory = $true)][string[]]$Result,
  [Parameter(Mandatory = $true)][string]$Out
)
# F231. Reads the ROW lines the AppendProbe plugin wrote, from one or more of its output files, and writes the table of
# fix 1: per model, where the manual Append, the tool after the NWC page and the tool after the NWF page put it, and the
# largest difference in mm of the nine numbers, the translation x y z and the box's min and max x y z, of each tool
# column against the manual one. An NWD reads as one model, so its box is set against the union of the manual boxes.
# Labels: <group> manual, <group> tool nwc, <group> tool nwf, <group> tool nwd and <group> tool nwd after nwf.
$inv = [Globalization.CultureInfo]::InvariantCulture
$rows = @{}
foreach ($file in $Result) {
  foreach ($line in [IO.File]::ReadAllLines($file)) {
    if (-not $line.StartsWith("ROW`t")) { continue }
    $f = $line.Split("`t")
    $mm = @($f[18..26] | ForEach-Object { if ($_ -eq 'UNKNOWN') { [double]::NaN } else { [double]::Parse($_, $inv) } })
    $rows[$f[1] + "|" + $f[4]] = [pscustomobject]@{ Label = $f[1]; Name = $f[4]; Source = $f[5]; Units = $f[6]; DocUnits = $f[7]; Mm = $mm; Index = [int]$f[3] }
  }
}
function M([double]$v) { return ($v / 1000).ToString("0.000", $inv) }
function Cell($r) {
  if ($null -eq $r) { return "not read" }
  $m = $r.Mm
  return ($r.Units + ", T " + (M $m[0]) + " " + (M $m[1]) + " " + (M $m[2]) + ", box " + (M $m[3]) + " " + (M $m[4]) + " " + (M $m[5]) + " to " + (M $m[6]) + " " + (M $m[7]) + " " + (M $m[8]))
}
function DiffMm($a, $b, [int]$from) {
  if ($null -eq $a -or $null -eq $b) { return "UNKNOWN" }
  $d = 0.0
  for ($k = $from; $k -lt 9; $k++) { $x = [Math]::Abs($a[$k] - $b[$k]); if ([double]::IsNaN($x)) { return "UNKNOWN" }; if ($x -gt $d) { $d = $x } }
  return $d.ToString("0.000", $inv)
}
$script:worst = 0.0; $script:unknown = 0
function Count([string]$v) { if ($v -eq "UNKNOWN") { $script:unknown++ } else { $x = [double]::Parse($v, $inv); if ($x -gt $script:worst) { $script:worst = $x } } }
$groups = @($rows.Values | ForEach-Object { ($_.Label -split ' ')[0] } | Sort-Object -Unique)
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("Positions in metres, E N Z: the units, the translation T of the model's transform and its root item's box, min to max. A difference is the largest of the numbers set against the manual Append, in mm.")
$lines.Add("")
$lines.Add("model | manual Append | tool after the NWC page | tool after the NWF page | difference after the NWC page, mm | difference after the NWF page, mm")
foreach ($g in $groups) {
  $manual = @($rows.Values | Where-Object { $_.Label -eq ($g + " manual") } | Sort-Object Index)
  foreach ($m in $manual) {
    $c = $rows[$g + " tool nwc|" + $m.Name]; $w = $rows[$g + " tool nwf|" + $m.Name]
    $dc = DiffMm $m.Mm $(if ($null -ne $c) { $c.Mm } else { $null }) 0; $dw = DiffMm $m.Mm $(if ($null -ne $w) { $w.Mm } else { $null }) 0
    Count $dc; Count $dw
    $lines.Add(($m.Name + " | " + (Cell $m) + " | " + (Cell $c) + " | " + (Cell $w) + " | " + $dc + " | " + $dw))
  }
}
$lines.Add("")
$lines.Add("NWD | union of the manual boxes | the NWD after the NWC page | the NWD after the NWF page | difference after the NWC page, mm | difference after the NWF page, mm")
foreach ($g in $groups) {
  $manual = @($rows.Values | Where-Object { $_.Label -eq ($g + " manual") })
  if ($manual.Count -eq 0) { continue }
  $u = @([double]::NaN, [double]::NaN, [double]::NaN, [double]::MaxValue, [double]::MaxValue, [double]::MaxValue, [double]::MinValue, [double]::MinValue, [double]::MinValue)
  foreach ($m in $manual) { for ($k = 3; $k -lt 6; $k++) { $u[$k] = [Math]::Min($u[$k], $m.Mm[$k]); $u[$k + 3] = [Math]::Max($u[$k + 3], $m.Mm[$k + 3]) } }
  $n1 = @($rows.Values | Where-Object { $_.Label -eq ($g + " tool nwd") })
  $n2 = @($rows.Values | Where-Object { $_.Label -eq ($g + " tool nwd after nwf") })
  $d1 = if ($n1.Count -eq 1) { DiffMm $u $n1[0].Mm 3 } else { "not read" }
  $d2 = if ($n2.Count -eq 1) { DiffMm $u $n2[0].Mm 3 } else { "not read" }
  if ($d1 -ne "not read") { Count $d1 }; if ($d2 -ne "not read") { Count $d2 }
  $ub = "box " + (M $u[3]) + " " + (M $u[4]) + " " + (M $u[5]) + " to " + (M $u[6]) + " " + (M $u[7]) + " " + (M $u[8])
  $lines.Add(($g + " | " + $ub + " | " + $(if ($n1.Count -eq 1) { Cell $n1[0] } else { "not read" }) + " | " + $(if ($n2.Count -eq 1) { Cell $n2[0] } else { "not read" }) + " | " + $d1 + " | " + $d2))
}
$lines.Add("")
$lines.Add("largest difference " + $script:worst.ToString("0.000", $inv) + " mm, differences UNKNOWN " + $script:unknown)
[IO.File]::WriteAllLines($Out, $lines)
$lines
