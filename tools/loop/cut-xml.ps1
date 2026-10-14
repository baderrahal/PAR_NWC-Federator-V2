param(
  [Parameter(Mandatory = $true)][string]$Xml,
  [Parameter(Mandatory = $true)][string]$OutFolder,
  [Parameter(Mandatory = $true)][string]$CoreDll,
  [int]$Tests = 50
)
$ErrorActionPreference = "Stop"

# tools\loop\cut-xml.ps1, F144, Bader's order of 2026-10-11 headed NO MORE TWO HOUR TEST LOOPS, item 3.
# Writes a copy of a picked clash XML that keeps every selection set and only the first -Tests
# tests in the XML's order, mirrors included, so a fast check runs the same code paths in minutes.
# The full XML is read and never written: its sha256 is read before and after and must not move.
# It stays for the coming clash test work, by Bader's order of 14 Oct 2026, Q165, though a loop run takes no XML.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\loop\cut-xml.ps1 -Xml <the picked XML>
#     -OutFolder <a folder that is not there, under the run set's copy> -CoreDll <a Federator.Core.dll>
#
# READ AS THE RUN READS IT. The file is read through Federator.Core's MatrixCorrections.ReadPicked,
# the add-in's one reader of a picked XML, with its corrections and its team map beside it, and the
# pairs are MirrorRule.Of over its tests and its corrected sets as a first run hands them, -CoreDll
# loaded from its bytes so no file of an installed bundle is held. For each test kept, the test its
# pair joins it to is kept too. Where the first -Tests hold no pair at all, the first pair in the
# XML's order is kept as well, both tests, so the mirror code runs, a reading of his words said in
# cut.txt. A test is placed by its FileIndex and its name read there Ordinal, and a file holding two
# tests of one name is refused, since a pair by name could then join the wrong one.
#
# THE CUT IS MADE IN THE TEXT. Each clashtest element is found as a span of the file's own text, the
# spans read equal in number and in names to the parsed file, and the ones not kept are taken out
# with their line ending, so every other byte, the sets, the declaration and the line endings, is
# the full file's. The copy carries the full XML's name in -OutFolder, which must not be there
# already, and the corrections and teams files beside the full XML are copied beside it under the
# names Core's settings give, CorrectionListSettings and TeamMapSettings. run.ps1 takes an -Xml under
# the run set's copy only, so -OutFolder sits there, such as runs\NN\NMFed\first50.
#
# THE READ BACK: the copy's bytes against the bytes written, by sha256, each file beside it against
# its source, the copy read again through ReadPicked, its tests by FileIndex and name in the order
# kept, its sets against the full file's, and its pairs, name for name, the pairs the full file's
# rule makes among the tests kept, and cut.txt read back. Exit 0 when all of it holds, 1 when a read
# back does not, 2 when it refuses or throws, cut.txt naming why where -OutFolder was made.

$record = New-Object System.Collections.Generic.List[string]
function Say($t) { $record.Add($t); Write-Output $t }
$bad = 0
$made = $false

try {
  if ($Tests -lt 1) { Write-Output "REFUSED: -Tests is below one. Nothing was written"; exit 2 }
  if (Test-Path -LiteralPath $OutFolder) { Write-Output ("REFUSED: -OutFolder " + $OutFolder + " is there already. Nothing was written"); exit 2 }
  if (-not (Test-Path -LiteralPath $Xml -PathType Leaf)) { Write-Output ("REFUSED: -Xml " + $Xml + " is not a file. Nothing was written"); exit 2 }
  if (-not (Test-Path -LiteralPath $CoreDll -PathType Leaf)) { Write-Output ("REFUSED: -CoreDll " + $CoreDll + " is not a file. Nothing was written"); exit 2 }
  $Xml = (Resolve-Path -LiteralPath $Xml).ProviderPath

  $shaBefore = (Get-FileHash -LiteralPath $Xml -Algorithm SHA256).Hash
  [void][System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($CoreDll))

  # The pairs, as a first run makes them: the picked file read with its corrections, its tests, its
  # sets as this run's build created them, no priority file, the default mirror settings and no test
  # saved in the document.
  function RuleOf($path) {
    $doc = [Federator.Core.Exchange.MatrixCorrections]::ReadPicked($path)
    $plan = [Federator.Core.Clash.ClashTestPlan]::From($doc, "m")
    $built = New-Object Federator.Core.Sets.SetBuildOutcome
    foreach ($s in $doc.Sets) { [void]$built.AddCreated($s.Path, $s.Path.Substring($s.Path.LastIndexOf('/') + 1), 1, 5, $null) }
    $saved = [Federator.Core.Clash.ClashTestPlan]::FromDocument((New-Object 'System.Collections.Generic.List[Federator.Core.Clash.SavedClashTest]'), "m")
    $rule = [Federator.Core.Clash.MirrorRule]::Of($plan.Buildable, [Federator.Core.Clash.PriorityMap]::NothingPicked(), $doc.Sets, (New-Object Federator.Core.Clash.MirrorSettings), $saved, $built)
    return ,([pscustomobject]@{ Doc = $doc; Plan = $plan; Rule = $rule })
  }
  # The names of a plan's tests by their place in the file, buildable and skipped alike.
  function NamesByIndex($plan) {
    $d = New-Object 'System.Collections.Generic.SortedDictionary[int,string]'
    foreach ($t in $plan.Buildable) { $d[$t.FileIndex] = $t.Name }
    foreach ($t in $plan.Skipped) { $d[$t.FileIndex] = $t.Name }
    return ,$d
  }
  function PairKeys($rule) {
    $k = New-Object 'System.Collections.Generic.SortedSet[string]' ([StringComparer]::Ordinal)
    foreach ($p in $rule.Pairs) { [void]$k.Add($p.Kept.Name + " | " + $p.Mirror.Name) }
    return ,$k
  }

  $full = RuleOf $Xml
  $fullNames = NamesByIndex $full.Plan
  $bytes = [System.IO.File]::ReadAllBytes($Xml)
  $bom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
  $enc = New-Object System.Text.UTF8Encoding($bom)
  $skip = $(if ($bom) { 3 } else { 0 })
  $text = $enc.GetString($bytes, $skip, $bytes.Length - $skip)
  $spans = @([regex]::Matches($text, '(?s)[ \t]*<clashtest\b[^>]*?\bname="([^"]*)".*?</clashtest>[ \t]*(\r?\n)?'))
  Say ("the full XML: " + $Xml + ", sha256 " + $shaBefore + ", " + $spans.Count + " test elements in its text, " + $fullNames.Count + " tests read by ReadPicked, " + $full.Doc.Sets.Count + " sets, " + $full.Rule.Pairs.Count + " mirror pairs by the product's rule")
  if ($spans.Count -ne $fullNames.Count) { Say ("REFUSED: the text holds " + $spans.Count + " test elements and ReadPicked reads " + $fullNames.Count + " tests, so a cut of the text would not be the file the run reads"); throw "the text and the parsed file disagree" }
  $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
  for ($i = 0; $i -lt $spans.Count; $i++) {
    $n = [System.Net.WebUtility]::HtmlDecode($spans[$i].Groups[1].Value)
    if (-not $fullNames.ContainsKey($i) -or $fullNames[$i] -cne $n) { Say ("REFUSED: test element " + ($i + 1) + " of the text is named " + $n + " where ReadPicked reads " + $fullNames[$i]); throw "the text and the parsed file disagree" }
    if (-not $seen.Add($n)) { Say ("REFUSED: two tests of the file are named " + $n + ", so a pair by name could join the wrong one"); throw "a name twice" }
  }

  $keep = New-Object 'System.Collections.Generic.SortedSet[int]'
  $first = [math]::Min($Tests, $spans.Count)
  for ($i = 0; $i -lt $first; $i++) { [void]$keep.Add($i) }
  foreach ($p in $full.Rule.Pairs) {
    $k = $p.Kept.FileIndex; $m = $p.Mirror.FileIndex
    if ($k -lt $first -and -not $keep.Contains($m)) { [void]$keep.Add($m); Say ("kept the mirror " + $p.Mirror.Name + ", place " + ($m + 1) + ", of the kept test " + $p.Kept.Name) }
    elseif ($m -lt $first -and -not $keep.Contains($k)) { [void]$keep.Add($k); Say ("kept the test " + $p.Kept.Name + ", place " + ($k + 1) + ", whose mirror " + $p.Mirror.Name + " is among the first") }
  }
  $pairsIn = 0
  foreach ($p in $full.Rule.Pairs) { if ($keep.Contains($p.Kept.FileIndex) -and $keep.Contains($p.Mirror.FileIndex)) { $pairsIn++ } }
  if ($pairsIn -eq 0 -and $full.Rule.Pairs.Count -gt 0) {
    $firstPair = $null; $at = [int]::MaxValue
    foreach ($p in $full.Rule.Pairs) { $lo = [math]::Min($p.Kept.FileIndex, $p.Mirror.FileIndex); if ($lo -lt $at) { $at = $lo; $firstPair = $p } }
    [void]$keep.Add($firstPair.Kept.FileIndex); [void]$keep.Add($firstPair.Mirror.FileIndex)
    Say ("the first " + $first + " tests hold no mirror pair, so the first pair in the XML's order is kept too, so the mirror rule runs: " + $firstPair.Kept.Name + ", place " + ($firstPair.Kept.FileIndex + 1) + ", and its mirror " + $firstPair.Mirror.Name + ", place " + ($firstPair.Mirror.FileIndex + 1))
  }
  $wantPairs = New-Object 'System.Collections.Generic.SortedSet[string]' ([StringComparer]::Ordinal)
  foreach ($p in $full.Rule.Pairs) { if ($keep.Contains($p.Kept.FileIndex) -and $keep.Contains($p.Mirror.FileIndex)) { [void]$wantPairs.Add($p.Kept.Name + " | " + $p.Mirror.Name) } }

  $sb = New-Object System.Text.StringBuilder
  $from = 0
  for ($i = 0; $i -lt $spans.Count; $i++) {
    if ($keep.Contains($i)) { continue }
    [void]$sb.Append($text, $from, $spans[$i].Index - $from)
    $from = $spans[$i].Index + $spans[$i].Length
  }
  [void]$sb.Append($text, $from, $text.Length - $from)
  $outText = $sb.ToString()
  $body = $enc.GetBytes($outText)
  if ($bom) { $outBytes = New-Object byte[] ($body.Length + 3); $outBytes[0] = 0xEF; $outBytes[1] = 0xBB; $outBytes[2] = 0xBF; [Array]::Copy($body, 0, $outBytes, 3, $body.Length) } else { $outBytes = $body }

  New-Item -ItemType Directory -Force -Path $OutFolder | Out-Null
  $made = $true
  $copy = Join-Path $OutFolder (Split-Path $Xml -Leaf)
  [System.IO.File]::WriteAllBytes($copy, $outBytes)
  $sha = New-Object System.Security.Cryptography.SHA256Managed
  $shaWritten = [BitConverter]::ToString($sha.ComputeHash($outBytes)).Replace("-", "")
  $sha.Dispose()
  $shaCopy = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
  if ($shaCopy -ne $shaWritten) { $bad++ }
  Say ("the copy: " + $copy + ", " + $keep.Count + " tests kept, the first " + $first + " and " + ($keep.Count - $first) + " for the mirrors, sha256 " + $shaCopy + ", read back equal to the bytes written " + ($shaCopy -eq $shaWritten) + ", " + ($text.Length - $outText.Length) + " characters of test elements taken out and every other one the full file's")

  foreach ($beside in @((New-Object Federator.Core.Exchange.CorrectionListSettings), (New-Object Federator.Core.Teams.TeamMapSettings))) {
    $src = $beside.PathBeside($Xml)
    $dst = $beside.PathBeside($copy)
    if (Test-Path -LiteralPath $src -PathType Leaf) {
      Copy-Item -LiteralPath $src -Destination $dst
      $ok = (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash
      if (-not $ok) { $bad++ }
      Say ("copied beside it: " + (Split-Path $dst -Leaf) + ", read back equal " + $ok)
    } else { Say ("not beside the full XML, so not beside the copy: " + (Split-Path $src -Leaf)) }
  }

  $cut = RuleOf $copy
  $cutNames = NamesByIndex $cut.Plan
  $sameOrder = ($cutNames.Count -eq $keep.Count)
  if ($sameOrder) {
    $j = 0
    foreach ($i in $keep) { if (-not $cutNames.ContainsKey($j) -or $cutNames[$j] -cne $fullNames[$i]) { $sameOrder = $false; break }; $j++ }
  }
  $setsOk = ($cut.Doc.Sets.Count -eq $full.Doc.Sets.Count)
  $gotPairs = PairKeys $cut.Rule
  $pairsOk = $gotPairs.SetEquals($wantPairs)
  if (-not $sameOrder) { $bad++ }
  if (-not $setsOk) { $bad++ }
  if (-not $pairsOk) { $bad++ }
  Say ("read back through ReadPicked: " + $cutNames.Count + " tests, the order and names kept " + $sameOrder + ", " + $cut.Doc.Sets.Count + " sets against " + $full.Doc.Sets.Count + " " + $setsOk + ", " + $gotPairs.Count + " mirror pair(s), the pairs of the full file among the tests kept " + $pairsOk)
  foreach ($k in $gotPairs) { Say ("  pair: " + $k) }
  $shaAfter = (Get-FileHash -LiteralPath $Xml -Algorithm SHA256).Hash
  if ($shaAfter -ne $shaBefore) { $bad++ }
  Say ("the full XML after: sha256 " + $shaAfter + ", unchanged " + ($shaAfter -eq $shaBefore))
} catch {
  Say ("COULD NOT RUN: " + $_.Exception.GetType().FullName + ": " + $_.Exception.Message)
  if ($made) { [System.IO.File]::WriteAllLines((Join-Path $OutFolder "cut.txt"), $record.ToArray(), (New-Object System.Text.UTF8Encoding($false))) }
  exit 2
}
$cutTxt = Join-Path $OutFolder "cut.txt"
[System.IO.File]::WriteAllLines($cutTxt, $record.ToArray(), (New-Object System.Text.UTF8Encoding($false)))
$back = [System.IO.File]::ReadAllLines($cutTxt)
if ($back.Count -ne $record.Count) { $bad++ }
if ($bad -gt 0) { Write-Output ("cut-xml: " + $bad + " read back(s) did not hold"); exit 1 }
Write-Output "cut-xml: every read back holds, cut.txt read back too"
exit 0
