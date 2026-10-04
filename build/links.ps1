# build\links.ps1, F109. The one rule of what is a junction or a link, dot-sourced by
# build\install.ps1 and tools\loop\prepare-copy.ps1. A file of one function with no main body.

# The kind of link $item is, Junction or SymbolicLink as PowerShell's LinkType reads it off
# the reparse point, or null when it is neither. Every file and folder OneDrive syncs carries
# the reparse point attribute with no link type, measured on 2026-09-27 and again on
# 2026-10-04 on a clone under OneDrive, its folders and files alike, so the attribute alone
# would take each of them for a link.
function LinkKind($item) {
    if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -eq 0) { return $null }
    $kind = [string]$item.LinkType
    if ($kind -eq "Junction" -or $kind -eq "SymbolicLink") { return $kind }
    return $null
}
