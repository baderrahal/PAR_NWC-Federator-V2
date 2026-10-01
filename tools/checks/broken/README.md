# Files that are meant to be wrong

Not a project and not compiled by anything. The folder is wrong on purpose in three ways,
one per check, and each check has to come back refusing it.

A check that only ever runs against a clean tree proves nothing, so every check here runs
twice in Actions, once over the real files and once over this folder. The two C# checks read
`src`. `check-evidence-ids.sh` reads the whole tree with this folder left out.

## For `check-locals.sh`

`DeclaredTwice.cs` holds the fault: a settings object and an outcome object both called
`views` in one method scope, which is CS0128 and is what F52 shipped and nothing here could
see until F58. `Fine.cs` holds every shape the check must leave alone, all of it legal C#
and all of it close enough to the fault to be worth pinning. The check has to come back with
exactly one fault, naming the file, the line and the name.

## For `check-imports.sh`

`MissingImport.cs` holds the fault: `DocumentSelectionSets` as a parameter type with no
`Autodesk.Navisworks.Api.DocumentParts` import, which is CS0246 and is what F61 shipped and
nothing here could see until F65 and F66.

`HasImportAsAParameter.cs` and `HasImportAsALocal.cs` are correct and are the map. The check
has no Autodesk DLL, so it learns where a type comes from by reading what every other file
that names it imports, and a type only one other file names teaches it nothing. That is why
there are two of them and not one, and why they use the type in the two positions the real
tree uses it in.

`NearMiss.cs` MUST PASS. Every `DocumentSelectionSets` in it is a word and not a type, one in
a comment and one inside a string, and the property the code reads is a member. That is
`SavedViewpoints.cs` in the real tree, which names `DocumentSavedViewpoints` four times in
comments and correctly carries no `DocumentParts` import. A check that matched on the word
alone would add an import that file does not need.

The check has to come back with exactly one fault, naming the file, the type and the
namespace.

## For `check-evidence-ids.sh`

What counts as an id is in `tools/checks/evidence-ids.txt`, which the check and
`tools/loop/mask-evidence.ps1` both read. Every id here is FABRICATED, a GUID of all zeros,
and the machine is a made up name, which Actions hands the check and the mask as
COMPUTERNAME for this folder alone. So this README never spells that name or a GUID, or it
would be one more fault.

`EvidenceWithIds.txt` holds the fault, one line per shape. Line 1 is a MACHINE line naming
the machine. Lines 2 and 3 are the command lines of two AdskLicensingAgent processes in the
shape F102 found on a pushed branch, with an analytics agent id and ids after `-i`. Line 4
is an AdskIdentity session GUID, line 5 a GenuineService GUID, line 6 an analytics agent id
with its key url encoded, and line 7 a GUID after `-i` on another program's line, which is
masked and refused on any line. Line 8 has an analytics agent id after `-i` and a second
GUID after `--session`, the line the second reading found the mask left half masked, because
it asked which kinds read the line after an earlier kind had masked part of it. The check
has to come back with exactly fourteen faults on it, each naming the file, the line and the
kind, and never the text.

`EvidenceUtf16.txt` is a line of fabricated text written as UTF-16, as Windows PowerShell
5.1 writes by default, so it holds a NUL byte after every letter. The check has to name it
and refuse it as binary, in words that say no line of the binary rule names it.

`samples/client-report/NotAPicture.jpg` and `NotAWorkbook.xlsx` beside it are TEXT files at
paths the binary rule names, each with the made up name on its line 2. The rule leaves a
file out only when its first and last bytes are its format's own too, so both are read as
text and the check has to refuse each on line 2.

`samples/client-report/EndsWrong.jpg` and `EndsWrong.xlsx` beside them start right and end
wrong, with fabricated bytes. The jpg opens FF D8 FF and holds FF D9, then has a line with
the made up name appended after it. The xlsx opens PK 03 04 and has no end of central
directory. The check has to refuse each as binary at a path the rule names whose first or
last bytes are not its format's.

`rules/` holds rules files for the test that the check and the mask read a rules file the
same way. `valid.txt` is whole and both have to read it. Every other file there is
`valid.txt` with one fault, and both have to refuse it. Actions puts each in the place of
`tools/checks/evidence-ids.txt` in a copy of the two scripts. None of them carries an id.

`EvidenceLeftByMask.txt` holds the name inside a longer word. The mask masks the name only
as a whole word, because a name inside a longer word may be part of another word, and the
check and the mask's read back refuse it anywhere, so a person looks. The check has to come
back with one fault on its line 2, and the mask has to refuse it on its read back and write
nothing.

`EvidenceWithIds.masked.txt` MUST PASS. It is written by hand and is byte for byte what the
mask writes from `EvidenceWithIds.txt`, which Actions proves on every run by masking that
file and comparing the two.

`EvidenceNearMiss.txt` MUST PASS. Its first two lines are masked lines. A GUID on a line no
kind reads, a COM CLSID and a WPF window class name, names no licence and no machine and is
left alone. So is a GUID after a word that only starts with `-i`, and `analytics-` with no
GUID after it.

Actions reads the whole tree with this folder left out, because the fault here is on
purpose, and the pre-commit leaves it out of what is staged for the same reason. Over this
folder the check has to come back with exactly twenty faults.
