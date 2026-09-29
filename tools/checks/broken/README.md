# Files that are meant to be wrong

Not a project and not compiled by anything. The folder is wrong on purpose in three ways,
one per check, and each check has to come back refusing it.

A check that only ever runs against a clean tree proves nothing, so every check here runs
twice in Actions, once over `src` and once over this folder.

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

`EvidenceWithIds.txt` holds the fault, in the shape F102 found on a pushed branch: a MACHINE
line naming the machine, and the command lines of two AdskLicensingAgent processes carrying
an analytics agent id and a licensing id after `-i`. Every id in it is FABRICATED, a GUID of
all zeros, and the machine is a made up name, which Actions hands the check as COMPUTERNAME
for this folder alone. The check has to come back with exactly four faults on lines 1 to 3,
each naming the file, the line and the kind, and never the text. So this README never
spells that name, or it would be a fifth.

`EvidenceNearMiss.txt` MUST PASS. Its first line and its licensing line are the same lines
as `tools/loop/mask-evidence.ps1` writes them masked. The name sits inside a longer word,
which is not the name as a whole word. And a GUID on a line that names no licensing program,
a COM CLSID, a WPF window class name and one after `-i` on another program's line, names no
licence and no machine and is left alone.

Actions reads the whole tree with this folder left out, because the fault here is on
purpose, and the pre-commit leaves it out of what is staged for the same reason.
