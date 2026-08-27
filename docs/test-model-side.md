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
`PackageContents.xml` plus `Contents\v22\Federator.Addin.dll`. If it does and the button
still does not appear, open **Tools** then **Global Options** then **Tools** then
**Developer** and tick **Display plug-in load errors**, restart Navisworks, and send me
what it says.

9. Click **Parsons NWC Federator**.

**Worked:** a window opens called Parsons NWC Federator with three tabs across the top,
`1. Source`, `2. Grouping` and `3. Outputs`, and a Run button, a progress line and a log
box along the bottom.

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

14. Untick any building you do not want in this run.

## Step 3, outputs

15. Click the **3. Outputs** tab.

16. Click the first **Browse** and pick the folder the NWF files go in.

17. Click the second **Browse** and pick the folder the NWD files go in.

**Worked:** the table shows the output name for every group before anything runs, for
example `1104-PAR-1C07BC-ZZZ-BM-MOD-000001`. Check one against a building you know. It
should be the project code, the originator and the building code from the input files,
then always `ZZZ`, `BM`, `MOD` and `000001`. Level, discipline, type and number are all
pinned, because the outputs overwrite on the next run.

## Run it

18. Click **Run**.

19. Read the warning box before clicking anything.

**Worked:** it names what you are about to lose, for example
`The open file C:\models\something.nwf, holding 4 models.` If nothing is open it says so
instead. This is your last chance, because the run clears the document before each group
and does not save it first.

20. Click **Cancel** the first time, on purpose.

**Worked:** the log says `Run cancelled before anything was cleared.` and nothing on disk
changed. Check the NWF folder is still empty.

21. Click **Run** again, then click **OK**.

**Worked:** the progress line moves through the groups, for example
`Group 1 of 3: 1C07BC (4 files)`, then `Saving NWF for 1C07BC`, then
`Publishing NWD for 1C07BC`. The log gets one line per group as each finishes:

    1C07BC  WRITTEN  appended 4 of 4  NWF on disk  NWD on disk

When it ends the progress line reads something like
`Run finished. 3 written, 0 partial, 0 failed.`

## Check what it actually wrote

22. Open the NWF folder.

**Worked:** one `.nwf` per group you ticked, named exactly what step 17 showed, with no
date and no version number on the end.

23. Open the NWD folder.

**Worked:** one `.nwd` per group, same names.

24. Open `ParsonsNwcFederator-run.log` in the NWF folder.

**Worked:** one timestamped line per group, the same lines you saw in the log box. The
word after the building code is `WRITTEN`, `PARTIAL` or `FAILED`.

25. Open one of the NWD files in Navisworks and check every discipline of that building is
    in it.

## What the three results mean

- `WRITTEN` means every file appended and both outputs are on disk. The tool checked the
  disk, it did not just assume the save worked.
- `PARTIAL` means at least one NWC would not append but the rest did, and both outputs are
  on disk. The line names the files that failed. The federation is real but incomplete, so
  go and look at those files.
- `FAILED` means nothing usable came out. Either no file appended, or an output is not on
  disk. The line says which.

## Run it twice

26. Click **Run** again with the same settings.

**Worked:** the same file names are overwritten in place. No second copy appears, no date
suffix, no `(2)`. The run log grows by one more block of lines rather than being replaced.

## What to send me if it goes wrong

The contents of the log box, the contents of `ParsonsNwcFederator-run.log`, and which step
number above it stopped at.

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

What is still UNKNOWN until you run steps 6 to 26: whether Navisworks loads the bundle,
whether the button appears on Tool Add-ins, whether an append, a save or a publish
succeeds against a real NWC, and how long a real run takes.
