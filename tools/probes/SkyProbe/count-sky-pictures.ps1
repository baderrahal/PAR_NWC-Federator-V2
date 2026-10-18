param(
  [Parameter(Mandatory = $true)][string]$Folder,
  [Parameter(Mandatory = $true)][string]$Out
)
# F215, F210-R3, 2026-10-18. Reads every cd*.jpg of one group's _files folder, writes nothing beside it, and writes one row a
# picture to -Out: the share of pixels, every 4th of every 4th row, that differ by more than 24 in a channel from their row's
# most common colour, the colour at x 512 y 300, whether that is the sky, 136,169,238, and how many pixels, every 2nd of
# every 2nd row, read as a clashing item's red or green, red or green above 180 with the other two below 110. A picture whose
# middle is sky and that holds no such pixel drew no item of its clash. Windows PowerShell 5.1:
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\probes\SkyProbe\count-sky-pictures.ps1 -Folder <..._files> -Out <rows.tsv>
Add-Type -AssemblyName System.Drawing
$rows = New-Object System.Collections.Generic.List[string]
$rows.Add("file`tpercent`tmiddle`tsky`titem pixels")
foreach ($f in (Get-ChildItem -LiteralPath $Folder -Filter 'cd*.jpg' | Sort-Object Name)) {
  $bmp = [System.Drawing.Bitmap]::FromFile($f.FullName)
  try {
    $diff = 0; $all = 0
    for ($y = 0; $y -lt $bmp.Height; $y += 4) {
      $count = @{}
      $line = New-Object System.Collections.Generic.List[int]
      for ($x = 0; $x -lt $bmp.Width; $x += 4) { $c = $bmp.GetPixel($x, $y).ToArgb(); $line.Add($c); $count[$c] = 1 + [int]$count[$c] }
      $mode = ($count.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 1).Key
      $mr = ($mode -shr 16) -band 255; $mg = ($mode -shr 8) -band 255; $mb = $mode -band 255
      foreach ($c in $line) {
        $all++
        $r = ($c -shr 16) -band 255; $g = ($c -shr 8) -band 255; $b = $c -band 255
        if ([math]::Abs($r - $mr) -gt 24 -or [math]::Abs($g - $mg) -gt 24 -or [math]::Abs($b - $mb) -gt 24) { $diff++ }
      }
    }
    $mid = $bmp.GetPixel([math]::Min(512, $bmp.Width - 1), [math]::Min(300, $bmp.Height - 1))
    $sky = ($mid.R -eq 136 -and $mid.G -eq 169 -and $mid.B -eq 238)
    $items = 0
    for ($y = 0; $y -lt $bmp.Height; $y += 2) {
      for ($x = 0; $x -lt $bmp.Width; $x += 2) {
        $p = $bmp.GetPixel($x, $y)
        if (($p.R -gt 180 -and $p.G -lt 110 -and $p.B -lt 110) -or ($p.G -gt 180 -and $p.R -lt 110 -and $p.B -lt 110)) { $items++ }
      }
    }
    $rows.Add($f.Name + "`t" + [math]::Round(100.0 * $diff / $all, 3) + "`t" + $mid.R + "," + $mid.G + "," + $mid.B + "`t" + $sky + "`t" + $items)
  } finally { $bmp.Dispose() }
}
[IO.File]::WriteAllLines($Out, $rows)
$drew = @($rows | Select-Object -Skip 1 | Where-Object { ($_ -split "`t")[3] -eq "True" -and ($_ -split "`t")[4] -eq "0" }).Count
"{0} pictures read, {1} with the sky at the middle and no item drawn, rows in {2}" -f ($rows.Count - 1), $drew, $Out
