---
paths:
  - "build/**"
---

# Rules for build

The install of this machine, the team zip and the team install. Their rules sat in addin.md, whose paths line is
src/Federator.Addin/**, so they were never read where build is touched, claude\audit-round-2026-10-18.md item 19. They
are here since F228, moved and not copied. The reasons and the measurements are in docs\history\claude-md-history.md.

- install.ps1 copies the bundle CONTENTS, never the bundle folder. Copy-Item of a
  directory puts it inside the destination when the destination exists and creates it
  when it does not, so the same line does two different things depending on whether
  the remove before it has finished. That left ParsonsNwcFederator.bundle inside
  ParsonsNwcFederator.bundle, which Navisworks does not read at all, and it is checked
  for afterwards
- install.ps1 walks what is actually in the bundle after copying and refuses to finish
  if any reference is satisfied by neither the bundle, the framework, nor the
  Navisworks folder. A missing file is caught at install rather than ninety seconds
  into a run. The version mismatches are printed too, labelled as expected, so nobody
  reads them as faults. Since F109 that walk, the file list and the stamp read the
  INSTALLED bundle, never the staging copy, each assembly read from its bytes so the
  script holds no file of it open, and the installed Federator.Addin.dll must read the
  stamp the build wrote. SINCE F156 A REFERENCE IS ALSO READ BY ITS NAME AFTER BINDING POLICY,
  `AppDomain.ApplyPolicy`, because the reflection only context applies none. Core asks for
  System.IO.Compression 4.2.0.0 since F152, the version the .NET 4.8 reference assemblies
  carry, and the runtime loads the framework's 4.0.0.0 for it, so without it every install
  from F152 on stopped with Federator.Core needs System.IO.Compression, read on 2026-10-14
- install.ps1 ends one of four ways, each an exit code and a last line written once at
  the top of the script and copied word for word into INSTALL.md: 0 and the line telling
  a person to start Navisworks, 1 FAILED, 2 REFUSED, 3 LEFT, installed and checked with
  the add-in installed before still beside it. The installed line is printed only when
  nothing is left beside the add-in, so it always means a whole install. Every other end
  names each folder left beside the add-in on its own line first, F109, FR-080
- install.ps1 removes no folder through a junction or a link. Every recursive removal
  goes through RemoveTree, which reads every folder under it for one first, by the link
  type PowerShell reads off a reparse point, because every file OneDrive syncs carries
  the reparse point attribute with no link type. A link in the staging folder or in the
  installed bundle refuses before anything changes. A copy that fails on a first install
  takes the new folder out of the load path, removed or moved to .failed-, F109, FR-078
- install.ps1 reads the .replaced- and .failed- folders beside the bundle at its start,
  by one pattern each on the plugin folder and never a search below it. An empty or part
  filled load path with one whole add-in beside it gets that one put back, PUT BACK, and
  any other leftover refuses with each named, because which one a person keeps is theirs
  to say, F109, FR-079
- What install.ps1 and the team install both hold to is written once, in
  build\install-common.ps1, which each dot-sources: the place the add-in goes, the line
  telling a person to start Navisworks, the 16 files of a whole bundle, the runner of F214
  among them, the stamp and its
  parts, the Roamer refusal in one set of words, the walk from %APPDATA% down, and
  RemoveTree with LinksAt, so no junction or link is ever removed through by either. A copy
  of one of these in another script is a bug. INSTALL.md and INSTALL-TEAM.txt carry the
  start line for a person, and the Core test that no logo ships reads the file list there,
  F140
- The team zip is made by build\package.ps1 alone, and only when Bader has written "make
  the team package", Q149. It refuses with one REFUSED line and writes no zip on a checkout
  not on main, a git status that prints anything, a build of the solution with
  --no-incremental that fails, a stamp reading +edits or naming another commit, an output
  folder inside the repo, a zip of that name already there, or any end of install.ps1 but
  exit 0. A zip copied into the folder by a run that then fails is deleted, and where it
  cannot be the FAILED line says it was left and where. It stages and checks the bundle by running install.ps1 -SkipBuild against a
  throwaway APPDATA, so a bundle is assembled and checked one way, zips what that run
  installed, and names the zip and VERSION.txt from the stamp read off the built add-in,
  never typed. A zip is never committed, F140
- The team install, build\team-install.ps1 started by the two .cmd files of build\team,
  copies and never renames, because the rename of the installed bundle was refused on this
  machine with no Navisworks running on 2026-10-01 while a copy over it worked. It makes the
  installed folder hold exactly the zip's files, reads every file back by its SHA256, and
  takes the internet mark off what it wrote, since Roamer.exe.config names no
  loadFromRemoteSources, read on 2026-10-11, and whether Navisworks loads a marked DLL is
  UNKNOWN. The version installed before goes to ParsonsNwcFederator.bundle.previous, the
  one backup, but only when it is whole, not the very build being installed, and not left
  by a run that stopped part way, which ParsonsNwcFederator.bundle.copying beside it marks.
  An install that fails part way puts the kept one back and reads it back. Where nothing was
  kept, a whole add-in of the same build is left where it is and read again against the zip,
  and only a load path that held nothing, part of an add-in or what a stopped run left is
  cleared. Put back first copies the add-in installed aside to
  ParsonsNwcFederator.bundle.before-put-back and reads it back, then copies the kept one in.
  A put back that fails part way copies the one set aside back and reads it back, so the load
  path holds part of two versions only where both the put back and that copy back fail, and
  the line then says where each whole one is. The kept one stays at .previous. It ends DONE 0,
  FAILED 1 or REFUSED 2, each written once at the top of the script and carried word for
  word by INSTALL-TEAM.txt, and a FAILED line says what each folder holds. Whether Navisworks
  loads a folder whose name does not end in .bundle, such as .previous, is UNKNOWN, F140
- The .cmd files of the team zip check that team-install.ps1 and install-common.ps1 are
  beside them before they start PowerShell, and refuse in plain words when either is not,
  as when a file is started from inside the zip, F140
