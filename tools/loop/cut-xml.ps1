param(
  [Parameter(Mandatory = $true)][string]$Xml,
  [Parameter(Mandatory = $true)][string]$OutFolder,
  [Parameter(Mandatory = $true)][string]$CoreDll,
  [int]$Tests = 50
)
$ErrorActionPreference = "Stop"

# tools\loop\cut-xml.ps1, Bader's order of 2026-10-11 headed NO MORE TWO HOUR TEST LOOPS, item 3.
# Writes a copy of a picked clash XML that keeps every selection set and only the first -Tests
# tests in the XML's order, mirrors included, so a fast check runs the same code paths in minutes.
# The full XML is read and never written: its sha256 is read before and after and must not move.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\loop\cut-xml.ps1 -Xml <the picked XML>
#     -OutFolder <a folder that is not there, under the run set's copy> -CoreDll <a Federator.Core.dll>
#
# MIRRORS INCLUDED. The pairs are the product's own, Federator.Core's MirrorRule over the whole file,
# read through -CoreDll loaded from its bytes, so no file of an installed bundle is held. For each
# test kept, the test its pair joins it to is kept too. Where the first -Tests hold no pair at all,
# the first pair in the XML's order is kept as well, both tests, so the mirror code runs. On the
# corrected matrix every pair is a Telecom Fixtures test beside a Telephone Devices one and none is
# among the first fifty, so that pair is how the fast check reaches the mirror rule. What was added
# and why is written in cut.txt.
#
# THE COPY IS NAMED AS THE FULL XML, in -OutFolder, which must not be there already, and the teams
# and corrections files beside the full XML are copied beside it under their own names, so the copy
# reads its map and its corrections as the full XML does. run.ps1 takes an -Xml under the run set's
# copy only, so -OutFolder sits there, such as runs\NN\NMFed\first50. Every file written is read
# back by sha256, and the copy is read again through Federator.Core: its sets, its tests by name in
# order, and its pairs. Exit 0 when all holds, 1 when a read back does not, 2 when it could not run.

if (Test-Path -LiteralPath $OutFolder) { Write-Output ("REFUSED: -OutFolder " + $OutFolder + " is there already. Nothing was written"); exit 2 }
if (-not (Test-Path -LiteralPath $Xml -PathType Leaf)) { Write-Output ("REFUSED: -Xml " + $Xml + " is not a file. Nothing was written"); exit 2 }
if (-not (Test-Path -LiteralPath $CoreDll -PathType Leaf)) { Write-Output ("REFUSED: -CoreDll " + $CoreDll + " is not a file. Nothing was written"); exit 2 }
if ($Tests -lt 1) { Write-Output "REFUSED: -Tests is below one. Nothing was written"; exit 2 }

$shaBefore = (Get-FileHash -LiteralPath $Xml -Algorithm SHA256).Hash
[void][System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($CoreDll))
$record = New-Object System.Collections.Generic.List[string]
function Say($t) { $record.Add($t); Write-Output $t }
$bad = 0

# The pairs over the whole file, as the run reads them on a first run: the XML's tests, its sets
# built from it, no priority file, the default mirror settings and no test saved in the document.
function RuleOf($path) {
  $doc = (New-Object Federator.Core.Exchange.ExchangeReader).ReadFile($path)
  $plan = [Federator.Core.Clash.ClashTestPlan]::From($doc, "m")
  $built = New-Object Federator.Core.Sets.SetBuildOutcome
  foreach ($s in $doc.Sets) { $built.AddCreated($s.Path, $s.Path.Substring($s.Path.LastIndexOf('/') + 1), 1, 5, $null) }
  $saved = [Federator.Core.Clash.ClashTestPlan]::FromDocument((New-Object 'System.Collections.Generic.List[Federator.Core.Clash.SavedClashTest]'), "m")
  $rule = [Federator.Core.Clash.MirrorRule]::Of($plan.Buildable, [Federator.Core.Clash.PriorityMap]::NothingPicked(), $doc.Sets, (New-Object Federator.Core.Clash.MirrorSettings), $saved, $built)
  return [pscustomobject]@{ Doc = $doc; Plan = $plan; Rule = $rule }
}

$full = RuleOf $Xml
$x = New-Object System.Xml.XmlDocument
$x.PreserveWhitespace = $true
$x.Load($Xml)
$all = @($x.SelectNodes("//clashtests/clashtest"))
$sets = @($x.SelectNodes("//selectionsets//selectionset"))
Say ("the full XML: " + $Xml + ", sha256 " + $shaBefore + ", " + $all.Count + " tests, " + $sets.Count + " sets, " + $full.Rule.Pairs.Count + " mirror pairs by the product's rule")
if ($all.Count -ne ($full.Plan.Buildable.Count + $full.Plan.Skipped.Count)) { Say ("NOTE: the file holds " + $all.Count + " clashtest elements and the plan reads " + $full.Plan.Buildable.Count + " buildable and " + $full.Plan.Skipped.Count + " skipped") }

# The places to keep, by the file's own order, which FileIndex is.
$keep = New-Object 'System.Collections.Generic.SortedSet[int]'
$first = [math]::Min($Tests, $all.Count)
for ($i = 0; $i -lt $first; $i++) { [void]$keep.Add($i) }
$byName = @{}
$allNames = New-Object string[] $all.Count
for ($i = 0; $i -lt $all.Count; $i++) { $allNames[$i] = $all[$i].GetAttribute("name"); $byName[$allNames[$i]] = $i }
$joined = 0
foreach ($p in $full.Rule.Pairs) {
  $k = $byName[$p.Kept.Name]; $m = $byName[$p.Mirror.Name]
  if ($null -eq $k -or $null -eq $m) { Say ("NOTE: a pair whose test names the file does not hold as written, " + $p.Kept.Name + " and " + $p.Mirror.Name + ", left out"); continue }
  if ($k -lt $first -and -not $keep.Contains($m)) { [void]$keep.Add($m); $joined++; Say ("kept the mirror " + $p.Mirror.Name + ", place " + ($m + 1) + ", of the kept test " + $p.Kept.Name) }
  elseif ($m -lt $first -and -not $keep.Contains($k)) { [void]$keep.Add($k); $joined++; Say ("kept the test " + $p.Kept.Name + ", place " + ($k + 1) + ", whose mirror " + $p.Mirror.Name + " is among the first") }
}
$pairsIn = 0
foreach ($p in $full.Rule.Pairs) { if ($keep.Contains([int]$byName[$p.Kept.Name]) -and $keep.Contains([int]$byName[$p.Mirror.Name])) { $pairsIn++ } }
if ($pairsIn -eq 0 -and $full.Rule.Pairs.Count -gt 0) {
  $firstPair = $null; $at = [int]::MaxValue
  foreach ($p in $full.Rule.Pairs) { $lo = [math]::Min([int]$byName[$p.Kept.Name], [int]$byName[$p.Mirror.Name]); if ($lo -lt $at) { $at = $lo; $firstPair = $p } }
  [void]$keep.Add([int]$byName[$firstPair.Kept.Name]); [void]$keep.Add([int]$byName[$firstPair.Mirror.Name])
  Say ("the first " + $first + " tests hold no mirror pair, so the first pair in the XML's order is kept too, so the mirror rule runs: " + $firstPair.Kept.Name + ", place " + ([int]$byName[$firstPair.Kept.Name] + 1) + ", and its mirror " + $firstPair.Mirror.Name + ", place " + ([int]$byName[$firstPair.Mirror.Name] + 1))
}

for ($i = $all.Count - 1; $i -ge 0; $i--) { if (-not $keep.Contains($i)) { [void]$all[$i].ParentNode.RemoveChild($all[$i]) } }
New-Item -ItemType Directory -Force -Path $OutFolder | Out-Null
$copy = Join-Path $OutFolder (Split-Path $Xml -Leaf)
$settings = New-Object System.Xml.XmlWriterSettings
$settings.Encoding = New-Object System.Text.UTF8Encoding($false)
$w = [System.Xml.XmlWriter]::Create($copy, $settings)
try { $x.Save($w) } finally { $w.Dispose() }
Say ("the copy: " + $copy + ", " + $keep.Count + " tests kept, the first " + $first + " and " + ($keep.Count - $first) + " for the mirrors, sha256 " + (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash)

$stem = [System.IO.Path]::GetFileNameWithoutExtension($Xml)
foreach ($suffix in @(".teams.txt", ".corrections.txt")) {
  $src = Join-Path (Split-Path $Xml -Parent) ($stem + $suffix)
  if (Test-Path -LiteralPath $src -PathType Leaf) {
    $dst = Join-Path $OutFolder ($stem + $suffix)
    Copy-Item -LiteralPath $src -Destination $dst
    $ok = (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash
    if (-not $ok) { $bad++ }
    Say ("copied beside it: " + ($stem + $suffix) + ", read back equal " + $ok)
  } else { Say ("not beside the full XML, so not beside the copy: " + ($stem + $suffix)) }
}

# The read back, through the product's reader.
$cut = RuleOf $copy
$names = @($cut.Plan.Buildable | ForEach-Object { $_.Name })
$want = New-Object System.Collections.Generic.List[string]
foreach ($i in $keep) { $want.Add($allNames[$i]) }
$sameOrder = ($names.Count -eq $want.Count)
if ($sameOrder) { for ($i = 0; $i -lt $names.Count; $i++) { if ($names[$i] -cne $want[$i]) { $sameOrder = $false; break } } }
$setsOk = ($cut.Doc.Sets.Count -eq $full.Doc.Sets.Count)
if (-not $sameOrder) { $bad++ }
if (-not $setsOk) { $bad++ }
if ($full.Rule.Pairs.Count -gt 0 -and $cut.Rule.Pairs.Count -lt 1) { $bad++ }
Say ("read back: " + $names.Count + " tests, in the order kept " + $sameOrder + ", " + $cut.Doc.Sets.Count + " sets against " + $full.Doc.Sets.Count + " " + $setsOk + ", " + $cut.Rule.Pairs.Count + " mirror pair(s) by the product's rule")
$shaAfter = (Get-FileHash -LiteralPath $Xml -Algorithm SHA256).Hash
if ($shaAfter -ne $shaBefore) { $bad++ }
Say ("the full XML after: sha256 " + $shaAfter + ", unchanged " + ($shaAfter -eq $shaBefore))
[System.IO.File]::WriteAllLines((Join-Path $OutFolder "cut.txt"), $record.ToArray(), (New-Object System.Text.UTF8Encoding($false)))
if ($bad -gt 0) { Write-Output ("cut-xml: " + $bad + " read back(s) did not hold"); exit 1 }
Write-Output "cut-xml: every read back holds"
exit 0
