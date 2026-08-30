# Testing the model side

For Bader. This covers the model side only: scan, group, federate, save NWF, write NWD.
There is no clash and no Excel yet, so do not look for them.

One action per step. Do them in order.

## Before you start

You need Navisworks Manage 2025 on the machine and a folder holding at least four NWC
files whose names follow the standard, for example
`1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc`.

If Navisworks is open right now, close it. The install writes into the folder Navisworks
reads at startup, and Navisworks only reads it once.

## Install it

1. Open PowerShell.

2. Go to the repo:

       cd "C:\Users\p003653k\source\repos\Parsons NWC Federator"

3. Run the installer:

       powershell -ExecutionPolicy Bypass -File build\install.ps1

4. Read the last few lines. You are looking for this, with your own user name in the path:

       Installed to:
         C:\Users\<you>\AppData\Roaming\Autodesk\ApplicationPlugins\ParsonsNwcFederator.bundle

       Files written:
         Contents\v22\Federator.Addin.dll  (33280 bytes)
         Contents\v22\Federator.Core.dll  (41472 bytes)
         PackageContents.xml  (1382 bytes)

       All three expected files are present.

5. If Navisworks sits somewhere other than `C:\Program Files\Autodesk\Navisworks Manage 2025`,
   run this instead, with your real path:

       powershell -ExecutionPolicy Bypass -File build\install.ps1 -NavisworksPath "D:\Autodesk\Navisworks Manage 2025"

**Worked:** the three files are listed and the script says all three are present.

**Failed:** the script throws instead. The three failures it can report, and what each
means:

- `Autodesk.Navisworks.Api.dll was not found under '...'` means the Navisworks path is
  wrong. Go back to step 5.
- `The build failed with exit code 1. Nothing was installed.` means the code did not
  compile. Nothing was copied, so the old bundle is untouched. Send me the build output.
- `Install incomplete. Missing: ...` means a file did not copy. Check nothing has the
  bundle folder open.

## Start it

6. Start Navisworks Manage 2025 normally.

7. Look at the ribbon and click the **Tool Add-ins** tab.

8. Look for a button called **Parsons NWC Federator**.

**Worked:** the button is there.

**Failed:** the tab is missing, or the tab is there but the button is not. Either means
Navisworks did not load the bundle. Check that the folder from step 4 exists and holds
`PackageContents.xml` plus `Contents\v22\Federator.Addin.dll`.

If it does and the button still does not appear, the next thing to compare is
`PackageContents.xml` against one that Navisworks already loads on the same machine.
`%APPDATA%\Autodesk\ApplicationPlugins\ParsonsGlbExporter.bundle\PackageContents.xml` is a
working Navisworks 2025 example. The two attributes that decide whether the bundle is even
considered are `Platform`, which must be `NAVMAN`, and `AppType`, which must be
`ManagedPlugin`. Both were wrong here once already, see docs\scan.md.

Navisworks does not appear to write a plugin load failure anywhere. Searched on
2026-08-30: the only Navisworks files under `%LOCALAPPDATA%` and `%APPDATA%` are licensing
logs at `%LOCALAPPDATA%\Autodesk\Logs\AdlSdk-Navisworks Manage 2025-*.log`, which are
encrypted and carry nothing about plugins, and Chromium web view caches. Nothing on disk
mentioned this bundle except its own manifest. There may still be a setting for this in
the application, but it could not be confirmed from outside, so do not go hunting for a
log that may not exist.

9. Click **Parsons NWC Federator**.

**Worked:** a window opens called Parsons NWC Federator with four tabs across the top,
`1. Source`, `2. Grouping`, `3. Outputs` and `4. Clash`, and a Run button, a progress line
and a log box along the bottom.

**Failed:** a message box appears saying "Parsons NWC Federator could not start" with an
error underneath. Send me that error text.

## Step 1, source

10. On the **1. Source** tab, click **Browse** and pick your folder of NWC files.

11. Leave **Include subfolders** ticked.

12. Click **Scan**.

**Worked:** the table fills with one row per NWC. Each readable row is ticked and shows
the building code and the discipline pulled out of its name. The line under the table
reads something like `12 NWC found, 11 readable, 1 that cannot be read.` The window then
moves itself to the Grouping tab.

**Also worked, and is the point:** any file whose name does not follow the standard is
still in the table, on a pink row, unticked, with the reason spelled out, for example
`Split on "-" gave 1 parts, and 5 are needed to read the building at part 3 and the
discipline at part 5.` It is never hidden. You cannot tick it, because there is no
building code to group it under.

**Failed:** the table stays empty and the summary says `0 NWC found`. The folder holds no
`.nwc` files, or you picked the wrong folder.

## Step 2, grouping

13. Click the **2. Grouping** tab.

**Worked:** one row per building. Each row shows the building code, how many files are in
it, and every discipline present, for example `AR, ME, ST`. All four disciplines of a
building are in one row. Discipline never splits a building into two rows.

**Also worked:** if two files under one building disagree on the project code or the
originator, that row is pink, unticked, cannot be ticked, and the Status column names both
files and both values, like this:

    BLOCKED: The files disagree on the project code.
    1104-PAR-1C07K1-ZZZ-AR-MOD-000001 says 1104 and
    1105-PAR-1C07K1-ZZZ-ST-MOD-000001 says 1105.

That is correct behaviour. The tool will not guess which of the two is right, so it skips
the whole building and tells you which two files to go and look at.

14. Read the **Findings** panel under the table, before you press anything.

**Worked:** it lists what the scan noticed about this run and nothing else. It is worked
out from the run itself, so it needs no list of known building codes or known disciplines
and will behave the same on a project it has never seen. Four kinds appear:

- `ODD SHAPE` a building code whose pattern of letters and digits no other code in the run
  shares, with its files named. In your run of 22 groups that would have caught `100000`
  sitting among 21 codes shaped like `1B06PK` and `1C06M2`
- `NEAR MATCH` two codes one character apart, with both file counts, which usually reads
  as a typing error. In your run that is `1B06K1` against `1B06KI`
- `SINGLE DISCIPLINE` a group holding one discipline, which cannot clash against anything.
  In your run that is `1B06BS` with only EL and `1C06PK` with only AR
- `MISSING` a group without disciplines that other groups in the run have

**Also worked:** if nothing is odd, the panel says so in one line rather than sitting
empty.

None of this blocks anything. Every group stays ticked and runnable, and the decision is
yours. The same lines go into the log in a `FINDINGS` block straight after the group list,
so the file tells the same story afterwards.

15. Untick any building you do not want in this run.

## Step 3, outputs

16. Click the **3. Outputs** tab.

17. Click the first **Browse** and pick the folder the NWF files go in.

18. Click the second **Browse** and pick the folder the NWD files go in.

**Worked:** the table shows the output name for every group before anything runs, for
example `1104-PAR-1C07BC-ZZZ-BM-MOD-000001`. Check one against a building you know. It
should be the project code, the originator and the building code from the input files,
then always `ZZZ`, `BM`, `MOD` and `000001`. Level, discipline, type and number are all
pinned, because the outputs overwrite on the next run.

## Run it

19. Click **Run**.

20. Read the warning box before clicking anything.

**Worked:** it names what you are about to lose, for example
`The open file C:\models\something.nwf, holding 4 models.` If nothing is open it says so
instead. This is your last chance, because the run clears the document before each group
and does not save it first.

21. Click **Cancel** the first time, on purpose.

**Worked:** the log says `Run cancelled before anything was cleared.` and nothing on disk
changed. Check the NWF folder is still empty.

22. Click **Run** again, then click **OK**.

**Worked:** the progress line moves through the groups, for example
`Group 1 of 3: 1C07BC (4 files)`, then `Saving NWF for 1C07BC`, then
`Publishing NWD for 1C07BC`.

The log box fills as it happens, one line per action rather than one per group, each with
the wall clock time and the seconds since the run started:

    14:23:05.117  +0000.001s  Log opened at C:\Users\you\AppData\Local\ParsonsNwcFederator\logs\run-20260830-142305.log
    14:23:19.402  +0014.286s  GROUP    started  1C07BC  4 files
                              file     : C:\in\1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc
    14:23:19.404  +0014.288s  CLEAR    the document, before group 1C07BC
    14:23:19.410  +0014.294s  APPEND   attempt  C:\in\1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc
    14:23:41.882  +0036.766s  APPEND   ok       C:\in\1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc  8,412,160 bytes
    14:24:02.115  +0056.999s  NWF      attempt  C:\out\nwf\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf
    14:24:07.330  +0062.214s  NWF      written  C:\out\nwf\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf  1,204,736 bytes
    14:24:51.006  +0105.890s  GROUP    finished 1C07BC  DONE  91.604s

Every size in there was read back off the disk after the write. A size is never printed
for a file the tool did not find.

When it ends the progress line reads something like
`Run finished. 3 done, 0 partial, 0 failed.`

## Check what it actually wrote

23. Open the NWF folder.

**Worked:** one `.nwf` per group you ticked, named exactly what step 17 showed, with no
date and no version number on the end.

24. Open the NWD folder.

**Worked:** one `.nwd` per group, same names.

25. Look at the bottom of the log box in the window.

**Worked:** a `RESULT` block, which is the summary you do not have to scroll for. It reads
groups done, groups partial, groups failed, then every file written with the size that was
read back off the disk, then every error repeated in full, then the total elapsed. If
nothing went wrong the errors section is the single line `Nothing failed.`

26. Open one of the NWD files in Navisworks and check every discipline of that building is
    in it.

## What the three results mean

- `DONE` means every file appended and both outputs are on disk. The tool checked the
  disk, it did not just assume the save worked.
- `PARTIAL` means at least one NWC would not append but the rest did, and both outputs are
  on disk. The log names the files that failed. The federation is real but incomplete, so
  go and look at those files.
- `FAILED` means nothing usable came out. Either no file appended, or an output is not on
  disk. The log says which.

## The log

This is the thing to send me when anything goes wrong. It is written and flushed line by
line as the run happens, never held back to the end, so even if Navisworks dies mid append
everything up to that moment is already on disk.

It lands in two places, always:

1. The fixed path, which never depends on any folder you picked:

       %LOCALAPPDATA%\ParsonsNwcFederator\logs\run-yyyyMMdd-HHmmss.log

   In full that is
   `C:\Users\<you>\AppData\Local\ParsonsNwcFederator\logs\`. A new file per run, named
   for the moment you pressed the button. Even a run that fails on the very first step
   leaves one here, because the log is opened before anything else happens.

2. A copy next to the NWF folder, written at the end of the run, with the same file name.

If the second copy cannot be written, the first log says so and the run carries on.
Logging is never allowed to be the thing that stops a run.

27. Click **Open log folder** at the bottom of the window.

**Worked:** Explorer opens with this run's log file already picked out.

28. Click **Copy log**.

**Worked:** the progress line says how many characters were copied. Paste it straight into
chat.

One thing worth knowing: while the run is still going, the log file is held open. Notepad
opens it fine and so does the **Copy log** button, but some tools refuse it with a sharing
error. If that happens, either wait for the run to finish or use **Copy log**.

## Step 4, the search sets

This is the sets only. There are no clash tests and no Excel yet, so do not look for them.
It runs against whatever document is open at the time, so it does not need a run to have
happened first.

30. Open any NWD or NWF that has real content in it.

31. Click the **4. Clash** tab.

32. Click **Browse** and pick a sets XML or a combined one. The reference file
    `1104-PAR_CLASH_AllInOne (2) (1).xml` holds both halves and is a good first try.

33. Click **Build sets**.

**Worked:** the box fills with one line per set, and the line under it summarises. Each
line carries the full folder path, the name, how many conditions it has and how many items
it found:

    ok      lcop_selection_set_tree/Architecture/BLD-AR-Floors  2 conditions  1,240 items
    ZERO    lcop_selection_set_tree/Electrical/BLD-EL-Devices   1 condition   0 items

Then the totals:

    sets created      : 61
    sets finding items: 47
    sets at zero      : 14
    items found       : 88213

**Also worked, and is the point:** a set that finds nothing is marked `ZERO`, named, and
followed by the question it asked in internal names:

    ZERO    lcop_selection_set_tree/Architecture/BLD-AR-Roofs  1 condition  0 items
            asked for LcRevitData_Element/LcRevitPropertyElementCategory equals "Roofs"

It is not an error and it is not hidden. A zero means one of two things and the tool
cannot tell which, so it shows you the question and leaves the judgement to you. Either
the model genuinely has no roofs in it, or the search is asking for the wrong thing. If
most sets come back at zero, check what the ones that did work found. On the first real
run, Walls, Parking, Site, Doors, Ramps, Stairs and Railings all found items while Roofs,
Ceilings, Windows, Curtain Panels, Casework and Furniture did not, and every Structure,
Mechanical and Electrical set was at zero. That is the shape of a site and parking model
rather than a broken search.

34. Open the Selection Sets window in Navisworks and check the folders.

**Worked:** the folders nest exactly as the file had them, so `Mechanical` holds
`Mechanical-HVAC` and the rest as real folders rather than as sets with long names.

**Failed:** a line reads `FAILED` with an exception on it, or `SKIPPED` with a reason. A
`SKIPPED` line means the file used a condition test this tool does not rebuild, and it
names the test. Only an unknown test value causes that. An internal property name nobody
has seen before is passed straight to Navisworks rather than being treated as a problem.

Nothing here is tied to one project. The file is picked every run, and none of its names,
folder names, counts or internal property names are written into the tool.

35. Try it with a file that holds only tests and no sets.

**Worked:** it says `This file holds no sets. Nothing to build.` and does nothing else. A
project that keeps its sets in the model and supplies only tests is a normal case, not an
error.

## Run it twice

36. Click **Run** again with the same settings.

**Worked:** the same NWF and NWD file names are overwritten in place. No second copy
appears, no date suffix, no `(2)`. You get a brand new log file, because logs are never
overwritten.

## What to send me if it goes wrong

Send the whole log file, not a summary and not the last few lines. Use **Copy log** and
paste the lot, or attach the file from
`%LOCALAPPDATA%\ParsonsNwcFederator\logs\`.

The reason is that the useful part is almost never where you would expect. The header
carries the Navisworks version and what was open, the middle carries the exact file that
was being appended when it died, and the failures carry their full stack traces. Trimming
it to what looks relevant usually removes the line that says what happened.

Also tell me which step number above it stopped at.

## What I could not test

Everything from step 6 down needs Navisworks actually running, and I have no way to start
it here. What I did check on this machine, without Navisworks running:

- the add-in compiles against the real 2025 assemblies
- the plugin class is public, concrete, has a parameterless constructor, derives from
  `AddInPlugin`, overrides `Execute`, and carries both the `Plugin` and `AddInPlugin`
  attributes, all confirmed by reflection over the built DLL
- no Navisworks DLL is copied into the bundle
- the installer writes the three expected files and reports the path
- the window itself builds, the XAML parses, all fifteen named controls resolve, and the
  three tabs are there
- the scan, the parsing, the unreadable rows, the subfolder search, the grouping, the
  blocked group and the computed output names all work, driven against a folder of
  correctly named empty files
- the log file is created before any work starts, every line reaches the disk immediately
  rather than at the end, an exception is written with its full type name, message, inner
  exception and stack trace, the result block counts match what was logged, and a bad
  output folder still leaves the log in the fixed path with the reason the copy failed
- the log captured a real scan live, with wall clock times and seconds elapsed on every
  line

What is still UNKNOWN until you run steps 6 to 26: whether Navisworks loads the bundle,
whether the button appears on Tool Add-ins, whether an append, a save or a publish
succeeds against a real NWC, and how long a real run takes.
