# Testing the model side

For Bader. This covers scan, group, federate, save NWF, build the sets, create and run the
clash tests, write the workbook, write NWD. Images inside the workbook are not built yet.

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

**Also worked, and is new:** after the file list the installer checks every assembly the
bundle needs:

    Checking every assembly the bundle needs:
      every reference is satisfied, 14 assemblies checked. Navisworks supplies its own.

      6 reference(s) bind to a version the shipped file does not carry.
      These are NOT faults. There is no application config to put a binding
      redirect in, so the add-in resolves them by name from this folder.
      SixLabors.Fonts   wants System.Numerics.Vectors 4.1.3.0   the file is 4.1.4.0
      ...

Those six mismatches are why a run on aa163c9e wrote no workbook. Every file was already
in the bundle, and .NET Framework refuses a strong named assembly one build number away
from the one asked for. Read them and move on. If the installer ever says a reference is
satisfied by nothing, stop and send me that line, because that one is a real fault.

**Failed:** the script throws instead. The failures it can report, and what each
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

**Worked:** a window opens with four tabs across the top, `1. Source`, `2. Grouping`,
`3. Outputs` and `4. Clash`, and a Run button, a progress line and a log box along the
bottom.

**Check the title bar first.** It reads something like

    Parsons NWC Federator   [1.0.0.0 a1b2c3d4 built 2026-08-30 13:03:25]

That is the commit and the moment the binary was built, and it is the same string the log
prints in its SESSION block. Compare the time against when you last ran `install.ps1`. If
it is older, you are looking at a stale install and nothing below will reflect your latest
build. This has caught you out twice. A `+edits` after the commit means the working tree
had uncommitted changes when it was built.

**Failed:** a message box appears saying "Parsons NWC Federator could not start" with an
error underneath. Send me that error text.

## Step 1, source

10. On the **1. Source** tab, click **Browse** and pick your folder of NWC files.

11. Leave **Include subfolders** ticked.

12. Click **Scan**.

**Worked:** the table fills with one row per NWC. Each readable row is ticked and shows
the building code and the discipline pulled out of its name.

**Also worked, and is new:** the findings panel under the table now reports everything in
one place, starting with a plain count:

    73 files found, 71 readable, 2 that cannot be read.
    22 groups, one file per building, and 1 blocked because their files disagree.
    9 things worth a look. None of it stops a run.
           1  written differently from the rest
           1  two codes that look almost the same
           2  only one discipline, so nothing to clash
           5  missing a discipline others have

That panel used to be on the Grouping tab. Scan now reports what was found and what is
wrong with it together.

**Also worked, and is the point:** any file whose name does not follow the standard is
still in the table, on a pink row, unticked, with the reason spelled out, for example
`Split on "-" gave 1 parts, and 5 are needed to read the building at part 3 and the
discipline at part 5.` It is never hidden. You cannot tick it, because there is no
building code to group it under.

**Failed:** the table stays empty and the summary says `0 NWC found`. The folder holds no
`.nwc` files, or you picked the wrong folder.

13. Read the findings themselves, which are now written the way a person would say them.

**Worked:** they read as sentences rather than as code. Where it used to say
`100000 is the only code shaped 9A99AA` it now says:

    ODD SHAPE   The building code 100000 is written differently from every other code in
                this run. The other 21 codes look like 1B06PK, and 100000 does not follow
                that. That is often a typing error in the file name, and it is sometimes a
                real building named another way. Nothing is changed and the group still runs.

and where it used to say `Revit 0000KI feeds 2 groups` it now says:

    SHARED SOURCE  2 federations are being built from the same Revit building, 0000KI.
                   They are 1B06K1 and 1B06KI. That usually means one of the NWC file
                   names is wrong and these should be one building. Nothing is merged and
                   no group is unpicked, so the decision is yours.

If any of these still reads like code rather than like English, tell me which one.

14. Click **Copy findings**, then paste into Excel.

**Worked:** it lands as a table with a header row and four columns, `Code`, `Buildings`,
`What it means` and `Files`. The code is its own column so you can sort or filter on it,
and the sentence sits next to it. A run with nothing odd still copies a header and one row
saying so, because an empty clipboard reads as a failed copy.

**Still true:** it is worked out from the run itself, so it needs no list of known building
codes or known disciplines and will behave the same on a project it has never seen. Six
kinds appear. Nothing here blocks a run or unticks anything.

- `ODD SHAPE` a building code whose pattern of letters and digits no other code in the run
  shares, with its files named. In your run of 22 groups that would have caught `100000`
  sitting among 21 codes shaped like `1B06PK` and `1C06M2`
- `NEAR MATCH` two codes of the same length differing in one position, where the two
  characters are ones that are easy to misread: `1` `I` `l`, `0` `O`, `5` `S`, `8` `B`,
  `2` `Z`, `6` `G`. In your run that is `1B06K1` against `1B06KI` and nothing else. A
  plain one character rule reported about 45 pairs and buried everything else, because
  codes sharing a prefix differ by one character constantly and `1B06PE` against `1B06PG`
  is two real buildings. An inserted or missing character is not a near match either
- `SINGLE DISCIPLINE` a group holding one discipline, which cannot clash against anything.
  In your run that is `1B06BS` with only EL and `1C06PK` with only AR
- `MISSING` a group without disciplines that other groups in the run have

**Also worked:** if nothing is odd, the panel says so in one line rather than sitting
empty.

None of this blocks anything. Every group stays ticked and runnable, and the decision is
yours. The same lines go into the log in a `FINDINGS` block straight after the group list,
so the file tells the same story afterwards.

## Step 2, grouping

15. Click the **2. Grouping** tab and look at the new **Make** box.

**Worked:** four choices, and **One file per building** is already selected, because that
is what you run weekly. The other three are one file per building and discipline, one file
per discipline across every building, and one file for everything.

16. Change it to **One file per building and discipline** and watch the table.

**Worked:** the table redraws immediately. Where you had `1C07BC` with 4 files you now have
`1C07BC-AR`, `1C07BC-ST`, `1C07BC-ME` and `1C07BC-EL`, one file each, and the output names
on the Outputs step change with them. Put it back to **One file per building** when you
have looked.

Per building used to be written down as a rule. It is a default, and CLAUDE.md now says so.

17. With **One file per building** selected again, read the group table.

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

18. Untick any building you do not want in this run.

## Step 3, outputs

19. Click the **3. Outputs** tab.

20. Click the first **Browse** and pick the folder the NWF files go in.

21. Click the second **Browse** and pick the folder the NWD files go in.

22. Leave **Republish the NWD every run** ticked.

**Worked:** the line under the table says `The NWD is republished every run.` Untick it
only when you want the NWF opened and compared without a new NWD being written.

**Worked:** the table shows all three names for every group before anything runs, one
column each for the NWF, the NWD and the workbook, for example
`1104-PAR-1C07BC-ZZZ-BM-MOD-000001`. Check one against a building you know. The project
code, the originator and the building code come from the input files. The rest is supplied.

23. Look at the naming grid above the table.

**Worked:** fifteen boxes, five fields across and one row each for NWF, NWD and workbook,
filled in with `ZZZ`, `BM`, `MOD`, `000001` and `ZZZZZZ`. Those used to be fixed in the
code. They are defaults now and every one of them can be changed here.

They are what they are because the files inside one group can disagree on the level, the
type and the number, and outputs overwrite, so copying any one of them from an input file
would mean picking a winner. `ZZZ` is the ISO 19650 code for all levels, `BM` is a
federated building model, `MOD` is a model, `000001` never advances because the file is
overwritten in place, and `ZZZZZZ` is the building field when a group covers more than one
building.

24. Change the workbook row's **Type** from `MOD` to `RPT` and watch the table.

**Worked:** the Workbook name column changes and the NWF and NWD columns do not. The three
patterns are separate, because a project may want them to differ. The preview line above
the table shows the first group's three names as you type. Put it back to `MOD` when you
have looked.

**Worked, and is the guard:** if you ever set them so that two groups would end up with the
same name, the preview says `THE RUN CANNOT START` and names both groups, and pressing Run
refuses rather than starting. Outputs overwrite with no date suffix, so one would silently
destroy the other and you would only notice as a federation nobody can find.

## Run it

25. Click **Run**.

26. Read the warning box before clicking anything.

**Worked:** it names what you are about to lose, for example
`The open file C:\models\something.nwf, holding 4 models.` If nothing is open it says so
instead. This is your last chance, because the run clears the document before each group
and does not save it first.

27. Click **Cancel** the first time, on purpose.

**Worked:** the log says `Run cancelled before anything was cleared.` and nothing on disk
changed. Check the NWF folder is still empty.

28. Click **Run** again, then click **OK**.

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

29. Open the NWF folder.

**Worked:** one `.nwf` per group you ticked, named exactly what step 17 showed, with no
date and no version number on the end.

30. Open the NWD folder.

**Worked:** one `.nwd` per group, same names.

31. Look at the bottom of the log box in the window.

**Worked:** a `RESULT` block, which is the summary you do not have to scroll for. It reads
groups done, groups partial, groups failed, then every file written with the size that was
read back off the disk, then every error repeated in full, then the total elapsed. If
nothing went wrong the errors section is the single line `Nothing failed.`

32. Open one of the NWD files in Navisworks and check every discipline of that building is
    in it.

## What the three results mean

Each one is judged against what was ASKED FOR, not against what happens to be on disk. A
step you deliberately switched off is not a failure.

- `DONE` means everything requested for that group succeeded. If you untick republishing,
  a group that federated cleanly is DONE, not FAILED. That mistake once reported all 22
  groups of a clean run as FAILED.
- `PARTIAL` means something requested did not complete, or the group was `CHANGED` and
  left alone on purpose. The log names the files that would not append, or what differs.
- `FAILED` means something requested threw, or produced nothing. The log always says
  which, on the group line and again in the RESULT block. A failed count can never appear
  with an empty error list.

The RESULT block's `files written` list only ever names files this run actually wrote.
Outputs overwrite with no date suffix, so last week's NWF and NWD are sitting at the same
paths, and a group that threw before writing anything will not claim them.

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

33. Click **Open log folder** at the bottom of the window.

**Worked:** Explorer opens with this run's log file already picked out.

34. Click **Copy log**.

**Worked:** the progress line says how many characters were copied. Paste it straight into
chat.

One thing worth knowing: while the run is still going, the log file is held open. Notepad
opens it fine and so does the **Copy log** button, but some tools refuse it with a sharing
error. If that happens, either wait for the run to finish or use **Copy log**.

## Step 4, the search sets

The **4. Clash** tab has one box and, boxed off below it, two buttons.

There used to be two boxes, one for a sets file and one for a clash test file. Your file
holds both, so that meant picking the same file twice. There is one picker now and the
tool reads whatever is in the file.

The box is what **Run** uses. Run does the whole job for every ticked group: append,
save the NWF, build the sets, create the tests, run them, save the NWF again, publish the
NWD last. Leave the box empty and Run does the model side only, which is a step
switched off rather than a failure.

The two buttons are not steps in the run. They act on whatever document is open right now,
for trying a file by hand before committing to a full run. That is why they are boxed off
and named after what they do.

There is no Excel yet, so do not look for it.

36. Open any NWD or NWF that has real content in it.

37. Click the **4. Clash** tab.

38. Click **Browse** beside **Clash XML** and pick your file. The reference file
    `1104-PAR_CLASH_AllInOne (2) (1).xml` holds both halves and is a good first try.

**Worked:** the line underneath says what the file actually holds, counted out of the file
itself, for example `Picked ... It holds 61 sets and 1830 tests.` A file holding only
tests reads `0 sets and 1830 tests` and is still fine, as long as the sets are already in
the model.

39. Click **Sets into open model**.

**Worked:** the box fills with one line per set, and the line under it summarises. Each
line carries the full folder path, the name, how many conditions it has and how many items
it found:

    ok      lcop_selection_set_tree/Architecture/BLD-AR-Floors  2 conditions  1,240 items
    ZERO    lcop_selection_set_tree/Electrical/BLD-EL-Devices   1 condition   0 items

Then the totals:

    ran against       : C:\models\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd  (4 models loaded)
    sets created      : 61
    sets finding items: 47
    sets at zero      : 14
    items found       : 88213

The `ran against` line is the first thing to check. A count means nothing without knowing
what it was counted against, and it is also written as a line before the first set so it
is at the top of the block as well as the bottom.

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

40. Open the Selection Sets window in Navisworks and check the folders.

**Worked:** the folders nest exactly as the file had them, so `Mechanical` holds
`Mechanical-HVAC` and the rest as real folders rather than as sets with long names.

**Failed:** a line reads `FAILED` with an exception on it, or `SKIPPED` with a reason. A
`SKIPPED` line means the file used a condition test this tool does not rebuild, and it
names the test. Only an unknown test value causes that. An internal property name nobody
has seen before is passed straight to Navisworks rather than being treated as a problem.

Nothing here is tied to one project. The file is picked every run, and none of its names,
folder names, counts or internal property names are written into the tool.

41. Try it with a file that holds only tests and no sets.

**Worked:** it says `This file holds no sets. Nothing to build.` and does nothing else. A
project that keeps its sets in the model and supplies only tests is a normal case, not an
error.

## Step 5, the clash tests

Same tab, same file. This creates the tests the picked file describes and runs them
against whatever is open. The sets have to be there first, either because you built them
in step 4 or because they already live in the model.

42. Leave the **Clash XML** box exactly as it is. It is the same file.

43. Click **Tests into open model**.

**Worked:** a line per test as it goes, then the totals. Expect this to take a while, and
expect most tests to be skipped, which is the normal answer and not a fault:

    clashes AR-Floors v ME-Ducts   items 1240 v 613   New 27   3.2s
    passed  AR-Walls v ME-Ducts    items 980 v 613    none     2.8s
    SKIPPED AR-Floors v EL-Devices  the right side "lcop_selection_set_tree/Electrical/
            BLD-EL-Devices" finds nothing in this model, the left finds 1240

Then the totals:

    ran against       : C:\models\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd
    tests in the file : 1830
    tests created     : 1830
    tests skipped     : 1784, not run and not passed
         1784  a side finds nothing in this model
    tests run         : 46
        passed        : 31, ran and found nothing
        with clashes  : 15
    clashes found     : 212
        New       : 212
        Active    : 0
        Reviewed  : 0
        Approved  : 0
        Resolved  : 0
    clash step took   : 412.5 seconds

**The two numbers to read carefully.** `tests skipped` and `passed` are different things
and are never added together. A skipped test never ran, because one of its two sides finds
nothing in this model. A passed test ran and found nothing wrong. Both would show zero
clashes, which is why they are kept apart everywhere they appear. Most groups hold two or
three disciplines, so most of the 1830 pairs have a side that cannot be there, and a large
skipped number is the expected answer rather than a problem.

`clash step took` is on its own line because it is the number nobody has yet, and it is
what says whether the whole run will fit in the 45 minutes.

44. While it runs, watch the progress line and the log.

**Worked:** a running count every 25 tests, so a run of well over a thousand is watchable
rather than silent:

    CLASH    250 of 1830 tests, 8 run, 242 skipped, 31 clashes so far

45. Open Clash Detective in Navisworks and compare.

**Worked:** the tests are there under the names the file gave them, each side shows the
selection set by name rather than a list of items, and the clash count on a test matches
the number in the block above. That last one is the point. If a count disagrees with the
panel, that is a real fault and the log line for that test has the numbers to send back.

46. Look at any line in the log that begins `CLASH    created`.

**Worked:** it carries the tolerance twice, in both units, for example
`tolerance 0.2460629921 ft is 74.9999999921 mm`. Which units Navisworks measures the
tolerance in is not something that could be read off the DLL, so both numbers are logged
and this is the line that settles it. Check one test in Clash Detective and tell me which
of the two numbers its tolerance box shows.

47. Try a file that holds tests but names sets that are not in this model.

**Worked:** each one is reported by name and skipped, and nothing is created:

    CLASH    SKIPPED  AR-Floors v ST-Beams  the right set "lcop_selection_set_tree/
             Structure/BLD-ST-Beams" is not in the document

A test is never created with an empty side. One would return zero clashes and read as
passed, which is worse than not being there at all.

**Worked, and is what stops the 1 MB log:** when a pile of tests skip for the same reason,
the log gives the count, at most five named examples, and then the number not listed:

    SKIPPED 1784 tests, a side finds nothing in this model
            AR-Floors v ME-Ducts   the right side "..." finds nothing in this model
            AR-Walls v ME-Ducts    the right side "..." finds nothing in this model
            and 1779 more skipped for the same reason, counted and not listed

Every one of them is still counted in the totals. Only the repetition is gone. Tests that
were created or run keep their own line each, because those are the ones worth reading.

## Step 6, the guard on a document with no sets

This one is quick and is worth doing deliberately, because the tool used to get it wrong.

48. Open a model with no selection sets in it at all, pick a clash test XML, and press
    **Tests into open model** without building the sets first.

**Worked:** it stops immediately, before creating anything, and says so in one line in the
log and on the line under the box:

    CLASH    STOPPED  the document holds 0 sets and the tests name 61 sets, so no test can
             resolve a set. Nothing was created and nothing was run.
    CLASH    build the sets first, or pick a file whose tests name the sets this document
             already holds

**Failed:** it goes ahead and reports 1830 skipped, 0 created, 0 run. That is what it used
to do, and it took a whole run and a 1 MB log to say one thing.

## A group that holds one model

1B06BS and 1C06PK each hold a single NWC. One model cannot clash with anything, whatever
the test list says, and both of them used to run the full 1830 tests for nothing.

**Worked:** the group row on the Grouping step says so before you run:

    Ready. One model, so every test is created and none is run.

and the log says it once per group:

    CLASH    1B06BS holds one model, so every test is created and none is run. One model
             cannot clash with anything.

**Worked:** every test is still created, so the NWF is complete and matches the other
groups and a later run against a fuller model finds them already there. The block reads:

    SKIPPED 1830 tests, the group holds one model, so nothing in it can clash

**The thing to check:** that reason is its own, and is counted apart from a side finding
nothing. They mean different things. A side finding nothing says a discipline was not
exported and could be. One model says the group was never going to clash and no export
would change that. If a single model group ever reports its tests as
`a side finds nothing in this model`, that is a real fault.

**Worked:** the workbook is still written, and shows every test as not run for that
reason, so the group has the same set of outputs as every other group.

## Step 7, the run stops itself when everything is failing

This is the one that would have saved you nine hours. On 2026-08-31 a run went through 24
groups over 8 hours 52 minutes, created 1830 tests in every one of them and produced
nothing, because every single test threw the same exception and the run carried on
regardless.

That exception is fixed. This is the net underneath it.

49. Run normally. If anything has gone wrong in the same way for the first 50 tests,
    watch what happens.

**Worked:** the run stops. Not the test, not the group, the run:

    CLASH    RUN STOPPED  the first 50 tests all failed for the same reason, so the rest
             of the run was not attempted. ObjectDisposedException: ...
    RUN      STOPPED after 1 group. the first 50 tests all failed for the same reason ...
    RUN      23 groups were not attempted. Everything already written is kept.

Fifty is a setting. A test skipped because one of its sides finds nothing in this model is
the ordinary answer, not a failure, so it does not count towards the fifty either way. A
single test that works resets the count, because a run that does anything at all is not
uniformly broken.

50. Look at the size of the log.

**Worked:** it is small. The previous run left 17.8 MB, almost all of it the same stack
trace written out tens of thousands of times. The same failure is now written out once and
counted:

    FAILURE  clash test AR-Floors v ME-Ducts
             type     : System.ObjectDisposedException
             ...
    FAILURE  the same failure again for clash test AR-Floors v ME-Ducts. Every further
             repeat of this exact trace is counted, not written out.

and the RESULT block carries the total beside the one trace:

    [1] clash test AR-Floors v ME-Ducts   THIS HAPPENED 43920 TIMES, the trace is written once

Nothing is lost. Every repeat is counted. Only the repetition is gone.

51. Look at the `CLASH` lines at the top of each group.

**Worked:** every group says how many clash tests the document already held, even when
that is none:

    CLASH    the document already holds 0 clash tests, so everything created here is new

That line is there to answer a question I could not answer without a real run: whether
tests, sets or results survive from one group into the next document. If group 2 onwards
says a number other than zero, they do survive, and that is worth telling me.

## Step 8, the workbook

One workbook per group, written after the clash step and before the NWD is published, so
the three outputs of a group describe the same state.

52. Go to the **3. Outputs** tab and look at the new **Excel folder** box.

**Worked:** the line under the table says where the workbooks are going. Leave the box
empty and it reads something like `Workbooks go in C:\out\nwf\Clash Reports.` The folder
is made if it is not there.

53. Run a group that finds clashes, then open the workbook.

**Worked:** it opens in Excel and has a `Summary` sheet, a `Matrix` sheet, and one sheet
per test that found something, named `T0001` upward.

54. Read the top of the `Summary` sheet.

**Worked:** the three numbers that must never be merged are three separate lines:

    Tests in the file                 1830
    Ran                                666
    Passed, ran and found nothing      618
    Found clashes                       48
    Skipped, not run and not passed   1164
    Raw clashes                        213

618 plus 48 plus 1164 is 1830. If those ever fail to add up, something has merged skipped
with passed, which is the mistake this whole design is built to prevent.

55. Scroll down the `Summary` to the table.

**Worked:** one row per test in the file, not one per test that ran. A skipped test is in
there carrying why it was skipped. A test that found something has a link in the `Sheet`
column, and a test that found nothing has no link, because a link to a sheet that does not
exist opens an error box. The `Test name` column carries the full name, which is the whole
reason that column exists.

56. Open one `T####` sheet.

**Worked:** one row per group or per ungrouped clash, and the columns run: group or clash
name, status, distance, grid, level, then for each side the item name, family, type,
material, source file and discipline, then found date, position, the two element ids, and
the raw clash count.

**The three that matter most:** family, type and material. Without them whoever fixes the
clash has to open the model to find out what they are looking at. If they come back empty
on a real Revit model, tell me, because the tool looks for a property whose display name
is Family, Type or Material in any category and leaves the cell empty rather than guessing
when it finds none. The names it looks for are settings and I can change them.

57. Check a row that is a group.

**Worked:** its `Raw clashes` column says how many clashes are behind that one row, and
its distance is the most severe of them. Nothing is hidden by the grouping.

58. Open the `Matrix` sheet.

**Worked:** sides down and across, the disciplines read off the folder names in your own
file rather than off any list in the tool. A cell holds New plus Active, the clashes still
outstanding.

**The one to look hardest at:** a pair whose test was skipped reads `skipped`, not `0`. A
zero means the pair was tested and nothing clashed. A skip means nobody looked. If you
ever see a zero where a test did not run, that is a real fault and worth stopping for. A
pair no test covers at all is left blank, which is a third thing again.

59. Tick **Also write a Navisworks style clash XML** on the Outputs tab and run again.

**Worked:** an XML lands beside each workbook under the same name. It is off by default.
It is built from the same results in memory that the workbook is built from, so neither
reads the other and a fault in one cannot corrupt the other.

Its shape was read off the three stylesheets your Navisworks install ships in
`en-US\stylesheets\`, because there is no clash report schema anywhere in the install.
Filled: exchange, batchtest, clashtests, clashtest, summary, clashresults, clashgroup,
clashresult, resultstatus, clashpoint, gridlocation, createddate, clashobjects,
clashobject, layer and objectattribute. Left out, because the tool holds nothing to put in
them: approveddate, approvedby, assignedto, description, smarttags, clashtasklink and
everything under it, linkage, linkedanimation, clipplaneset, view and camera. Left out
rather than written empty, so a blank is never read as a measured blank.

**Not built yet.** Images are still not written. CLAUDE.md records them as off by default
and New and Active only, written as jpg beside the workbook with a link in the row. That
is a later session, so do not look for them.

## Run it twice, which is the weekly case

This is the behaviour that matters most, because the tool is used weekly and the clash
results inside an NWF are the only record of what has been fixed. An NWF holds pointers to
the NWC files, not copies, so a model updated in place needs no rebuild.

**Read this before you run it.** This is the step to watch hardest, because it has never
once worked. On the run of 2026-08-30 every one of the 22 groups reported CHANGED and not
one reported OPENED. The cause was the comparison reading `Model.SourceFileName`, which
holds the Revit container in Autodesk Docs, against the scanned NWC path, which it can
never equal. It now reads `Model.FileName`, which is the NWC and matched the scan exactly
on every line of that same log.

That fix is proved by tests against the real names off that log, but the tests cannot open
an NWF. **OPENED has still never appeared in a real log.** Whether a second run against
unchanged files actually produces it is UNKNOWN until you run this step, and it is the
single most valuable thing you can tell me.

60. Run once so an NWF exists, then run again with the same settings and the same folder.

**Worked:** the second run does not rebuild. Each group logs

    OPENED   C:\out\nwf\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf
             the file list matches the scan, 4 files, so it was not cleared and nothing
             was re-appended

and the NWD is republished. Open the NWD and the geometry is current. Open the NWF in
Clash Detective and every result you had marked Active or Resolved is still marked that
way. Nothing went back to New.

**Also worked, and is new:** nothing is created twice. The second run logs

    SET      already there, left alone  lcop_selection_set_tree/Architecture/BLD-AR-Floors
    CLASH    1830 tests are already in this document, they keep their results and are not
             recreated

and the totals carry an `already there` line separate from `created`. The tests are still
run, which is the point of a rerun, so their clashes are refreshed against the current
NWC files. What is not touched is the tests themselves, because that is where the Active
and Resolved statuses live. If you ever see the sets tree holding two of everything, or
every clash back at New after a second run, that is a real fault and worth stopping for.

61. Now add one NWC to the source folder, or remove one, and run again.

**Worked:** that group is left completely alone and logs

    CHANGED  C:\out\nwf\1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf points at a different set
             of files, so it was left exactly as it is
             3 unchanged, 1 added, 1 removed
             added   C:\in\1104-PAR-1C07BC-ZZZ-EL-MOD-000001.nwc
             removed C:\in\1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc

The NWF is not touched, so the decision is yours. If you want the new file in, delete the
NWF and let the next run rebuild it, knowing that throws away the clash history for that
building.

## The Revit source report

62. After any run, find the `SOURCE FINDINGS` block near the end of the log.

**Worked:** it reports where the building code on the NWC is not the building code inside
the Revit container it was published from. On the run of 2026-08-30 that is most of them:

    NWC and Revit source pairs compared: 73
    SOURCE MISMATCH   NWC 1B06BC was published from Revit 0000BC
                      The NWC name says building 1B06BC and the Revit container it came
                      from says building 0000BC. The group is built from the NWC name,
                      which is unchanged. Neither code is assumed right.
    SHARED SOURCE     Revit 0000KI feeds 2 groups: 1B06K1, 1B06KI
                      One Revit building is being federated into more than one output,
                      which reads as one building split in two by a naming error. Nothing
                      is merged and no group is unpicked.

The `SHARED SOURCE` line is the one worth acting on, because it means one real building is
being split across two outputs. Neither line changes anything. Both codes are read with
the same parser used on the NWC names, so there is no second naming rule to keep in step.

**Worked, and is normal:** a pair where the codes agree is not reported, even when the rest
of the name differs. `1C06PK` published from `1C06PK` numbered `000101` instead of `000001`
is silent, because a number is not a building.

## Outputs overwrite

63. Click **Run** again with the same settings.

**Worked:** the same NWF and NWD file names are overwritten in place. No second copy
appears, no date suffix, no `(2)`. You get a brand new log file, because logs are never
overwritten.

## The order within a group, which changed

64. Read one group's worth of log from `GROUP` to `GROUP`, and check the order.

**Worked:** it goes append, save the NWF, build the sets, create the tests, run them, save
the NWF again, publish the NWD last. The NWD is the last thing that happens.

This is worth checking once because it used to be wrong. The NWD went before any clash
work, which meant every NWD shipped with no sets and no results in it. The sets, the tests
and the results all live in the NWF, so the NWD has to be published after the second save
or it carries none of them.

**Worked:** with the sets and tests boxes both empty, the log says

    CLASH    no file picked in the Clash step, no set built and no test created

and the group still finishes DONE. Not picking a file is a step switched off, not a
failure, the same as unticking the NWD.

**Worked:** a CHANGED group builds no sets and creates no tests either, and says so:

    CLASH    1C07BC was left alone because its file list changed, so no set was built and
             no test created

## The pickers remember where you were

65. Close Navisworks completely after a run. Open it again and press the button, then open
    each of the five pickers in turn: source, NWF, NWD, Excel and the clash XML.

**Worked:** each one opens where you last left it, and they are five separate memories.
The clash picker opens in the folder the XML lives in, not at the XML itself.

66. The one worth checking properly: pick an NWD folder somewhere quite different, then
    open the source picker.

**Worked:** the source picker still opens at the NWC folder. It did not follow the NWD
picker. One shared memory is worse than none, because it walks every picker to whatever
you touched last.

67. Now rename or unmount a folder one of them remembers, and open that picker.

**Worked:** it opens at the nearest parent that is still there, and the Source step says

    FOLDERS  C:\out\nwf\test001 has gone, opening at C:\out\nwf instead

Nothing is cleared and nothing is an error. Put the folder back and it opens there again.
This is the drive-not-mounted-yet case, which happens most Mondays.

The file is `%LOCALAPPDATA%\ParsonsNwcFederator\logs\folders.txt` and you can delete it.
Deleting it forgets everything and breaks nothing.

## Step 9, the names as a table

This replaces reading the pattern and hoping. The names are now shown per group and every
one of them can be typed over.

68. Scan, then go to the Outputs step and look at the grid.

**Worked:** one row per group, with the NWF, NWD and workbook name already filled in, and
an **Edited** column that is empty on every row.

69. Click into one NWF name and type something else. Press Tab.

**Worked:** that row's Edited column says `by hand`. The NWD and workbook names on the SAME
row are untouched, and every other row is untouched.

70. Now change a pattern, for instance set the type code to FED, and watch the grid.

**Worked:** every name refills except the one you typed, and a line appears saying

    2 of 3 rows refilled. 1 row was typed over by hand and left alone.

This is the whole point of the table. A pattern is where a name starts, not where it ends,
and one group in twenty needs a name no pattern will give it.

71. Type a name onto one row that another row already has, and press **Run**.

**Worked:** the run does not start. It names both groups and says they would be written to
the same file. Outputs overwrite with no date suffix, so the second would destroy the first
silently, which is why this is a stop and not a warning.

## Step 10, what has drifted since last week

The clash tests already in the document keep their results and are never recreated, which
is right. It also means a tolerance you changed in the XML never reaches them. That was
silent. Now it is reported.

72. Change the tolerance on a few tests in the clash XML, then run against a model that
    already has those tests in it.

**Worked:** a `DRIFT` block naming each one:

    DRIFT    1830 tests were already in the document and were compared with the file.
             3 differ.
             AR-Floors v ME-Ducts: the file says the tolerance 0.075, the document has 0.05
             ...
             Nothing was changed. Changing a test resets its results.

**Worked:** nothing in the document changed. Open Clash Detective and the tolerances are
still what they were, and every Active and Resolved clash is still Active and Resolved.

73. Only if you actually want the file to win: tick **Apply the file's settings to tests
    already in the document** on the Clash step, and run again.

**Worked:** it says loudly that this RESETS the results of every test it changes and that
their clashes go back to New, then it changes them and says which. This is a once-a-quarter
thing, not a weekly one. The default is off for that reason.

## Step 11, the stale warning, and what it does not say

74. Look for `STATUS` lines in the log after the tests run.

**Worked:** it reports what Navisworks says and nothing more:

    STATUS   412 of 1830 tests report status Old

**This is deliberately not interpreted.** Clash Detective shows a warning triangle on a
test when something has changed. The API has no stale, altered or out-of-date member on
anything, measured across every type in `Autodesk.Navisworks.Clash`, public and private,
and recorded in docs\scan.md section 4j. The only thing that exists is `ClashTest.Status`,
which is New, Old, Partial or Complete. What puts a test into Old is **UNKNOWN**, so the
tool prints the word Old and stops there. It will never tell you "your models have
changed", because it cannot know that.

If you can tell me what Clash Detective shows next to a test the log called Old, that maps
the word to the triangle and I can then say something useful about it.

## Step 12, Resolved clashes, and Compact

75. After a run, look at the workbook Summary and at the group totals in the log.

**Worked:** the Resolved count per test and for the group. On a test that has been running
weekly for months this number only grows, because a Resolved clash stays in the file
forever. That is what makes the file the record.

76. **Read this before ticking anything.** Compact removes every Resolved clash from the
    NWF. The NWF is the only record of what has been fixed. There is no second copy and it
    cannot be undone.

Only when you have decided you want that: tick **Compact resolved clashes** on the Clash
step. It runs after the tests and before the workbook.

**Worked:** it says what it is about to do, with the count, before it does it:

    CLASH    COMPACTING. This removes Resolved clashes from the NWF permanently and
             cannot be undone. 4812 resolved clashes are in this document.
    CLASH    compacted, 4812 resolved clashes removed

**Worked, and matters more:** with the box unticked, no compact happens, and the log says
the resolved count without removing anything. Nothing compacts on its own, ever.

## Step 13, what counts as still open

77. On the Clash step, look at the open count box. It has two choices and the default is
    **Navisworks open**.

**Worked:** the default counts New, Active and Reviewed, which is what Navisworks itself
calls open, so the matrix number agrees with the panel. Switch it to **New plus Active**
and the same run gives a smaller number, short by exactly the Reviewed clashes.

**Worked:** the workbook says which one it used, on the sheet, along with what it counted
and what it did not. Approved and Resolved are closed under both.

The API has no open against closed notion at all, so neither of these is read off it. Both
are rules, the labelling is there so nobody reads an API meaning into the number, and the
sheet never says a bare "open" without saying which.

## Step 14, the weekly record

78. On the Outputs step, tick **Date the NWD**, and run.

**Worked:** the NWD is written as `1104-PAR-1C07BC-ZZZ-BM-MOD-20260904.nwd`, with today's
date where the number was, and last week's dated NWD is still sitting beside it. The NWF
is still `-000001` and still overwritten in place.

**That difference is on purpose.** The NWF holds the clash tests and every clash result
inside it, so it IS the history, and overwriting it in place is what keeps that history in
one file. A dated NWF would fork it: this week's clashes in one file, last week's Active
and Resolved stranded in another, and no file holding the whole picture. The NWD carries no
clash results at all, it is just the model as it stood, so a dated NWD is a free weekly
photograph. One is a ledger and one is a photograph.

79. The date format is a setting if yyyyMMdd is not what your project wants.

**Worked:** a format that would make a file name Windows refuses, such as one holding a
slash or a colon, falls back to the number rather than failing at the moment of writing.
A format that is merely odd is used exactly as typed and you can see it in the Outputs
grid before you run. It is your setting, so your mistakes stay visible rather than being
quietly corrected.

## Step 15, the workbook in the client's own shape

The columns are now the ones on the report the client has already accepted, in their order
and in their words. They were read off the two files you sent and off the stylesheet
Navisworks wrote them with, not off a description of them.

80. Run a group that finds clashes, then open a T sheet in the workbook.

**Worked:** the top of the sheet is their test header, the test name and then their nine:

    Tolerance | Clashes | New | Active | Reviewed | Approved | Resolved | Type | Status
    0.025m    | 13      | 13  | 0      | 0        | 0        | 0        | Hard (Conservative) | OK

and the clash table below it has their thirteen, with Item 1 and Item 2 merged over the two
blocks of three, exactly as their report has them:

    Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
          | Item ID | Item Name | Item Type | Item ID | Item Name | Item Type

81. Check the four fields that look like they could be several columns and are not.

**Worked:** Grid Location reads `B-1 : ROF` in one cell, not a grid column and a level
column. Clash Point reads `x:31.643, y:-2.913, z:3.325` in one cell, not three. Item ID
reads `Element ID: 1554240`. Tolerance reads `0.025m` with no space.

Those four are the part everyone gets wrong, including the brief I was given, so they are
worth ten seconds each. The stylesheet is what settles them and it is quoted in
docs\scan.md section 4k.

82. Check Distance.

**Worked:** the raw signed number, negative on a hard clash, and a NUMBER rather than text,
so the column still sorts and filters. Their own xlsx holds a number there too.

83. Look to the right of Item 2's Item Type.

**Worked:** ours start there and nowhere else. Item 1 Family, Item 1 Type Name, Item 1
Material, Item 1 Source File, Item 1 Discipline, then the same for Item 2, then Found and
Raw clashes.

Ours says **Type Name** and not Type, on purpose. The client already has a column called
Item Type and it holds something else, the Navisworks item type, which reads Solid. Two
columns called Type meaning two different things is how a report gets misread.

84. For a submission, tick **Client columns only** on the Outputs step and run again.

**Worked:** ours are gone and theirs are untouched, in the same order in the same columns.
Ours come off the right hand end, so nothing of theirs moves. The Summary sheet says which
of the two the workbook was written with.

## Step 16, the pictures where the client expects them

85. Run with the defaults. Images are **on** now, because the accepted report has them.

**Worked:** beside the workbook there is a folder named after it with `_files` on the end,
holding loose jpg. Exactly where your own report keeps them.

86. Look at the names.

**Worked:** `cd000001.jpg` upward, and the numbering is theirs. It is not one running
sequence, which is what the first dozen look like. It is cd, then the test, then the clash
within that test:

    cd000001.jpg   first test, first clash
    cd000013.jpg   first test, thirteenth clash
    cd010001.jpg   second test, first clash

Your own 2672 picture export is what proved this, because it reaches test 100 and the names
there are `cd1000001.jpg`, seven digits. A fixed width would have got that wrong.

We do not write a `logo.jpg`. Navisworks puts that one there and it is not a clash picture.

87. Click an Image cell in the workbook.

**Worked:** it opens the jpg. The link is relative, so moving the workbook and its `_files`
folder together keeps every one of them working. Move the workbook on its own and they
break, which is why the two are written side by side.

**Worth knowing about your own xlsx:** the one you sent has 61 pictures in it as links, and
every one is absolute, `file:///C:\00_NM\Clash report\...`. So that file shows broken
picture boxes on anyone else's machine. Ours does not have that problem.

88. If you want the picture in the cell as well, tick **Also paste a thumbnail**.

**Worked:** it appears in the Image cell. Off by default, because the accepted report links
rather than pasting and pasting makes the workbook many times larger.

## Step 17, what the pictures actually cost

Every number anyone has given for this, mine included, has been a guess. Now it is measured.

89. After a run, find the `IMAGES` lines in the log.

**Worked:** four numbers per group, all of them read off the run:

    IMAGES   60 written, 10.98 MB, 41.2 seconds in total.
             0.687 seconds each on average, quickest 0.412, slowest 1.884.
             187 KB each on average.

and the same figures on the Summary sheet, because the log is not what gets sent on.

**This is the step I most want the numbers from.** Rendering a picture needs the running
application, so I cannot time it here. Sixty pictures at whatever it turns out to be per
picture is what decides whether the cap needs a default other than off. Send me the IMAGES
lines from a real group.

90. Look at the counts of what was passed over.

**Worked:** clashes skipped on status and clashes skipped on the cap are counted
separately, because they are different decisions.

## Step 18, the image settings

91. On the Outputs step, look at the image block. The summary line under it says what the
    settings will actually do, before you run.

**Worked, the defaults:** on, 1024 by 1024, New Active and Reviewed only, no cap, linked
rather than pasted.

The 1024 is not a guess either. All 60 pictures in the report you sent measure 1024 by 1024,
so ours match what was accepted.

92. Set the cap to 5 and run against a test with more clashes than that.

**Worked:** five pictures for that test and no more, the rest counted as passed over on the
cap. The cap is per test, so a test with 1244 clashes cannot spend the whole run rendering.

93. Untick Reviewed and run.

**Worked:** Reviewed clashes get no picture and are counted as passed over on status.

94. Untick all five statuses.

**Worked:** it treats that as images off, and says so, rather than quietly writing nothing.
Those two are the same thing and one of them is legible in the log.

95. Type nonsense into the size or the cap box.

**Worked:** the summary line falls back to the default and shows what it actually
understood, so a typo is visible before Run is pressed rather than after.

## Step 19, a picture that fails

96. This one is hard to force on purpose, so mostly it is a thing to watch for.

**Should happen:** a clash whose picture fails is logged by name, its Image cell is left
empty, every other column on that row is still filled in, and the run carries on. The
workbook is the point of the run and a missing picture is not worth losing it over.

**Should also happen:** if fifty in a row fail the same way, the run stops and says so, the
same as the clash step does. The line will say images rather than tests, so you know where
to look.

97. If that ever fires, send me the line. It will name the first reason, and the fix will
    be to switch images off for that run while it is sorted out.

## Step 20, the NWF still holds its results after the NWD

Read this one first. It is the only step here that is about losing work rather than about
how a report looks.

Your run of 2026-09-01 ended with the RESULT block saying the NWF was 4,141 bytes, which is
an empty federation. **Nothing had shrunk.** The same file had been read at 165,844 bytes
four minutes earlier, on its own line, and the result block was reporting the size from the
FIRST of the two saves and had never updated it. The NWD had nothing to do with it.

That is fixed, and a real check has been added that was never there.

98. Run a group that finds clashes and read the end of its log.

**Worked:** two NWF written lines, the second much bigger than the first, and then after
the NWD a third line:

    NWF      written  ...1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf    4,141 bytes
    NWF      written  ...1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf  165,844 bytes
    NWD      written  ...1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwd  5,936,323 bytes
    NWF      intact   ...1104-PAR-1C07BC-ZZZ-BM-MOD-000001.nwf  165,844 bytes, unchanged
                      by publishing the NWD

and the RESULT block now says 165,844 for it, not 4,141.

99. **If you ever see this instead, stop and send it to me.**

    NWF      CHANGED  ...  was 165,844 bytes, is now 4,141 bytes after publishing the NWD.
                      It got SMALLER. The clash results live in this file.

That is the real version of what the old log only looked like. It would mean a week of
review had gone, and the group is marked failed with that reason on it.

100. The one thing a log still cannot tell you: open the NWF in Clash Detective after the
     run and check the results are there. The size says the file was not truncated. Only
     opening it proves the clashes are still in it, and that is UNKNOWN until you look.

## Step 21, the workbook against the client's own

Your two files are in `samples\client-report` now, and the workbook from your run is in
`samples\our-report`, so these were compared cell by cell rather than by eye.

101. Open a test sheet and check the four fields that were wrong.

**Worked:** Type now reads `Hard (Conservative)` and not `hard_conservative`. Status is
filled rather than blank. Grid Location reads `D-8 : LGF` once and not `D-8 : LGF : LGF`.
Distance reads `-0.328` and not `-0.328083992004395`, and it is still a number, so the
column sorts.

102. Look at Item ID.

**Worked:** a real element id where the model has one, and an EMPTY cell where it does not.
It will never again say `Instance GUID: 00000000-0000-0000-0000-000000000000`, which is
what all 426 of your item cells said. That was two faults at once. The clash hands back the
geometry, which on a Revit NWC is a leaf carrying a material name and no Revit properties,
so the id is now looked for on the element that leaf belongs to and then up its ancestors.
And an all zero GUID is not an id, so it is not written as one.

**This is the one I most want checked.** The search is right in principle and only a real
model shows whether the id is actually found. If the cells are still empty, send me one
clash and the Properties panel for the item, and I will see which category it is in.

103. Two things the brief said were wrong that turned out not to be.

**There is no Layer column in your report.** I checked both exports and the HTML. Their
header is thirteen cells and Layer is not one of them. Navisworks can write one, behind an
option that was off when yours was exported. So there is nothing to add.

**The tolerance is not wrong either.** Yours reads `0.2461ft` and theirs `0.025m` because
your model measures in feet and theirs in metres, and the two projects use different
tolerances. 0.2461 ft is 75 mm and 0.025 m is 25 mm. The log says
`the document measures in ft, every tolerance was converted into it`. Both reports are
right for their own model.

104. Tick **Client columns only** and run again.

**Worked:** the sheet is their thirteen columns and nothing else. The three notes above the
table, the Back to Summary link and the filter arrows are all gone, because they are ours.
Theirs have not moved.

105. The logo.

**Not done, on purpose.** Their report has an Autodesk logo in it. It ships with their
export, not with this install, and there is no right to redistribute it. If Parsons wants
its own logo there, send me the file and where it should sit.

## Step 22, the number has six digits again

106. Open the window and go to the Outputs step without typing anything.

**Worked:** all fifteen name fields are filled in. ZZZ, BM, MOD, 000001, ZZZZZZ, on all
three rows.

**They were all EMPTY before.** That is why your run produced `MOD-00001` with five digits.
The defaults were always six in the code and never reached the window, so every one of the
five fields had to be typed by hand, and a hand typed number is where a digit goes missing.
Filling the grouping list at startup fired a regroup, which read the still empty boxes back
over the defaults and wiped them.

107. Check the number field says exactly `000001`, six characters.

## Step 23, the pictures and the 54 MB

108. Your workbook came out at 54 MB. That was asked for, not a default.

The log records it: `Images on, 1024 by 1024 pixels, New, Active, Reviewed, Resolved only,
no cap per test, **with a thumbnail in the cell**`. Pasting the pictures in put 213 of them
inside the file, 51.4 MB of the 51.7 MB it came to, and the same 213 are in the folder
beside it anyway.

**Worked:** the default is off and always was. Untick it and the same workbook is about
0.3 MB, roughly 170 times smaller, and every Image cell still opens its picture.

109. Read the line under that tick box.

**Worked:** it now says what it costs, with the numbers off your own run, rather than
leaving you to find out by looking at the file afterwards.

## Step 24, the tick boxes say what they do

110. Read every tick box in the window.

**Worked:** each one names what changes in the output, then what happens with it off, and
then in capitals whether it makes a file bigger, makes the run slower, or destroys
something. Four of them carry a capital:

- pasting thumbnails, MAKES THE WORKBOOK ABOUT 170 TIMES BIGGER
- the pictures themselves, SLOWER, with the seconds and the megabytes
- applying the file's settings, DESTROYS RESULTS
- compacting, DESTROYS THE RECORD

111. If any of them still leaves you guessing what will change, tell me which and what you
     expected it to mean. That is the only test for this one.

## Step 25, every picker, one at a time

112. Point all five Browse buttons at five different folders: the source, the NWF, the NWD,
     the Excel folder and the clash XML. Close Navisworks completely. Open it again.

**Worked:** all five open where you left them, and they are five separate memories.

113. The one that was broken: the clash XML picker.

**Worked:** it opens in the folder the XML is in. It used to open one level ABOVE that,
because it took the parent of a value that was already a folder. If that is what you were
seeing when you said not all of them remember, this is it.

114. The file that holds them is
     `%LOCALAPPDATA%\ParsonsNwcFederator\logs\folders.txt`. Delete it to forget everything.
     Your run's first line said `Nothing remembered yet`, which is what a first run says.

## Step 26, the client report page

You were right that the client format is not a workbook. Clash Detective cannot export an
xlsx at all. You export HTML (Tabular) and open it in Excel, and that is why the file you
sent declares 53 columns with 17 filled in and has absolute file:/// links in it.

So this tool no longer invents that layout. It hands our own XML to Autodesk's own
clash_report_html_tabular.xsl out of your Navisworks install, which is the same file that
made yours.

115. Run a group. In the reports folder, beside the workbook, there is now an .html with
     the same name.

**Worked:** open it in Excel. The columns are theirs, in their order, in their words:

    Image | Clash Name | Status | Distance | Grid Location | Description | Clash Point
    Item ID | Layer | Item Name | Item Type      (Item 1)
    Item ID | Layer | Item Name | Item Type      (Item 2)

116. Tick **Client report layout only** and run again.

**Worked:** exactly the fifteen above and nothing else. Untick it and our five extra
properties per item and a Date Found column appear after theirs, and none of theirs move.

117. About the Layer column, and about a file that never reached me.

**You were right and the last session was wrong.** It reported that no supplied export has
a Layer column. That was true of the two files in samples\client-report, 1A02WE and 1A02WO,
and it is not true in general. Your screenshot is of
**1104-PAR-1A04WE-XXX-BM-RPT-000001.xlsx, which was never committed.** I read it off your
machine at C:\00_NM\Clash report and it has a Layer column on both items, with the header
present and the cells empty.

Both files are correct. The stylesheet writes a Layer column when the export carried layer
data and leaves it out when it did not. Ours always carries the level, so ours always has
the column and it is filled in.

118. If Navisworks is missing or its stylesheet is not where it should be, the log says so
     and names every path it looked at, and the workbook and the XML are written as normal.

## Step 27, the logo

119. The logo on your report comes from logo.jpg in the Images folder of the Navisworks
     install. It is Autodesk's and this tool does not touch it, copy it or ship it.

**Worked:** there is a Logo box on the Outputs step with its own Browse. Empty means no
logo, which is the default, and the page then has no picture on it.

120. Point it at the Parsons mark and run.

**Worked:** the file is copied beside the page and the page shows it. The two travel
together, the same way the clash pictures do.

## Step 28, the tick boxes

121. Look at every tick box in the window.

**Worked:** each one is a short label with one grey line under it saying what it costs.
Nothing runs off the edge, nothing is in capitals, and the off state is only described
where it changes what you would choose.

    Republish the NWD
        Adds about a second per building.

    Photo of every clash
        About 0.08 seconds each. 213 clashes took 17 seconds.

    Paste the photos into the cells
        Workbook goes from 0.3 MB to 52 MB. Already linked without this.

122. The two that destroy something carry a red exclamation mark beside the label, rather
     than shouting. When every label shouted, none of them did.

     Applying the file's settings, and deleting Resolved clashes.

123. If any label still leaves you guessing, tell me which one and what you expected it to
     mean. That is the only test for this.

## Step 29, the Outputs page scrolls

124. Go to the Outputs step and make the window short.

**Worked:** a scrollbar appears and the naming table and everything under it can be
reached. It was cut off before, by 384 pixels on a 1024 by 680 window and still by 87 on a
1600 by 1000 one, which is why the table was not visible whatever you did.

125. The other three steps were measured at those same three sizes and all fit, so none of
     them has a scrollbar. Source is the closest, wanting 326 pixels of the 389 it gets on
     the smallest window. If you ever find one cut off, tell me the window size.

## Step 30, two things that were leaking

126. Open the window and go to the Outputs step without scanning anything.

**Worked:** it says the workbooks go "beside the NWF folder, once one is picked on this
step". It used to say "Workbooks go in UNKNOWN", which reads as a fault when nothing is
wrong yet.

127. It also used to show "Parameter name: nwfFolder" behind that, which is a name out of
     the code and means nothing to anyone using this.

**Worked:** no label anywhere carries a code name or a framework message now. A failure
DIALOG still shows the message, because that is what you send back to me and it is what
makes a fault findable. A label does not.

## Step 31, the logo is the one you already send

The report now carries the same logo your reports carry today, automatically. Nothing to
pick, nothing to set up, no first run without it.

128. Open the window and go to the Outputs step.

**Worked:** the Logo box is already filled in with

    C:\Program Files\Autodesk\Navisworks Manage 2025\Images\logo.jpg

That is where Navisworks keeps it. It is 6137 bytes and it is byte for byte the same file
as the logo.jpg sitting in the _files folder of both reports you sent me, same MD5, which
is how I know it is the one Navisworks copies into a report folder.

129. Run a group and open the client report page.

**Worked:** the logo is at the top left, exactly as on the reports you send today.

130. Look in the report's _files folder.

**Worked:** logo.jpg is in there beside the clash pictures, which is where Navisworks puts
it too.

## Step 32, it survives being sent

131. This is the one that matters for a client. Copy the whole reports folder to a memory
     stick, or to a network share, or send it to someone else, and open the page there.

**Worked:** the logo and every clash picture still show. The page references them
relatively, so the folder works wherever it goes.

**Your own xlsx does not do this.** The one you sent carries absolute links,
`file:///C:\00_NM\Clash report\...`, so its pictures only ever show on your machine.
Anyone else opens it to broken boxes. Ours does not repeat that, and there is a test that
opens the written page and reads the reference back out of the file to make sure, because
the last broken image link was reported as fine by the object model while the file itself
was wrong.

## Step 33, nothing of Autodesk's ships with this tool

132. This is worth knowing if anyone asks about licensing.

The logo is never copied into this repo, never into the bundle, and install.ps1 does not
carry it. It is read off the Navisworks install on the machine that is running, every run.
So the picture that reaches a client was already on the machine that wrote the report.

The only two logo.jpg files in the repo are inside the two client exports you committed
yourself, where Navisworks had already put them. There is a test that fails if one turns
up anywhere else.

## Step 34, when the logo is not there

133. Hard to force on purpose, and mostly a thing to watch for. If Navisworks is installed
     somewhere unusual, or the Images folder is missing, the log says so and names every
     path it looked at:

    LOGO     the report carries the logo Navisworks puts on its own, read from the install
             at run time. It is not there, so the page was written without one. Everything
             else on it is unaffected.
             looked at C:\...\Images\logo.jpg
             looked at C:\...\en-US\Images\logo.jpg

**Should happen:** the report is still written, with everything else on it, and just no
picture at the top. A missing logo never stops a report.

134. If you want a different mark for a project, the Browse box is still there. Point it at
     a file and that one is used instead. Clear the box and there is no logo at all. Most
     people never touch it.

135. One thing that is not a fault. A page with a logo shows an Image column even when the
     photos are switched off, because the stylesheet turns that column on for any picture
     reference in the file and the logo is one. That is Autodesk's own behaviour and their
     reports do the same.

## Step 35, the Item ID column is back

136. Run a group and open the client report page. Look at the Item 1 and Item 2 blocks.

**Worked:** four columns each, the same as yours:

    Item ID | Layer | Item Name | Item Type

and Item ID reads `Element ID: 702888`. Ours had no such column at all before.

137. What was wrong, because the log told us exactly.

Your run threw 426 times with

    System.NotSupportedException: Not supported if '!IsDisplayString'
       at Autodesk.Navisworks.Api.VariantData.ToDisplayString()

Every way of reading a property value out of Navisworks is tied to the kind of value it
holds, and a Revit element id is a number, not a display string. So the read threw, and
because the whole of that method sat in one try, everything after it was lost as well.
That is why the source file and the discipline were empty too. One throw cost three
columns.

The value is now read by its kind. The source file and the discipline sit in their own
try, so a property that will not read can never take them with it again.

138. Check the two columns that were empty.

**Worked:** Source File and Discipline are filled in the workbook. They are not on the
client page, on purpose, see the next step.

## Step 36, our columns stay out of the client's page

139. Open the client report page and look for Family, Type Name, Material, Source File or
     Discipline.

**Worked:** none of them is there. Neither is Date Found. The page is exactly the fifteen
columns your own reports carry and nothing else.

That is now true whatever the tick boxes say. The page IS the client's report, so it
always carries only what theirs carries.

140. Open the workbook and look for the same five.

**Worked:** they are all there, after the client's columns. That is where ours belong.

141. Worth knowing why this mattered. The stylesheet makes a column out of every quick
     property it finds in the file. Ours was writing seven per item where yours writes
     two, so five extra columns would appear on a page going to a client, and two of them
     empty.

## Step 37, tolerance and separators

142. Look at the Tolerance cell on the page.

**Worked:** three decimals with the unit, `0.246ft`. It read `0.2460629921ft` before, which
was the file's own precision rather than a format. Yours reads `0.025m`.

**The unit difference is not a fault.** Your model measures in feet and theirs in metres,
and the log says `the document measures in ft, every tolerance was converted into it`.
0.246 ft is 75 mm and 0.025 m is 25 mm, so the two projects also use different tolerances.

143. Look at the picture references in the page source.

**Worked:** a backslash, `1104-..._files\cd000001.jpg`, which is what both of your reports
write. The logo too. Ours used a forward slash before.

The workbook keeps a forward slash in its hyperlinks, because a hyperlink there is a web
style address and your own xlsx has no picture hyperlinks at all to copy.

## Step 38, the pictures never live inside the file

144. This one is worth knowing before anyone asks why a workbook on its own shows nothing.

**Measured on your files and ours:** your 1A04WN xlsx has zero embedded pictures. Your
1A02WN has zero. Ours has zero. The native Navisworks export never embeds them.

Pictures show only when the `_files` folder sits beside the file. **The page and its
_files folder together is what gets sent.** Either one alone is not the report.

145. The line under the paste tick box now says exactly that, so nobody expects a workbook
     to carry pictures on its own.

## Step 39, the report checks itself so you do not have to

You have been opening every client report in Excel and searching it after a run. The tool
wrote the file, so it now reads it back and says what is in it.

146. Run a group and watch the run view.

**Worked:** two lines per group, in plain words:

    1C07BC. Client report: Item ID filled on 213 of 213 rows, no extra columns,
            all 213 pictures on disk.
    1C07BC. Workbook: 213 rows, every column of ours filled somewhere.

That is the whole check in a sentence. If it says that, you do not need to open anything.

147. If something is wrong, the line says what instead. For example

    1C07BC. Client report: The Item ID column is empty on some rows. Item 1 has one on
            0 of 213 and item 2 on 0 of 213. The client's report has one on every row.

So a bad report is visible without opening the log at all.

## Step 40, the full check in the log

148. Find the `REPORT CHECK` block for a group.

**Worked:** every number the check counted, off the file itself:

    CHECK    213 clash rows on the page.
             Item ID filled on 213 of 213 for item 1 and 213 of 213 for item 2.
             the first one reads Element ID: 702888
             no column the client's report does not have.
             the tolerance cell reads 0.246ft
             213 picture references, 213 with a backslash, 213 on disk.
             the logo is 1104-...-RPT-000001_files\logo.jpg and it is on disk.
             Nothing wrong with it.

The first Item ID is printed in full on purpose, so you can see its shape rather than take
my word that it is right.

149. The picture count is the one worth reading. **On disk** means the `_files` folder
     really holds the file the page points at. That is the thing that breaks when a report
     is sent on, so it is counted rather than assumed.

150. Find the `WORKBOOK CHECK` block.

**Worked:** how many rows filled each of our own columns:

    CHECK    213 clash rows across 48 test sheets.
             Item 1 Family       213 of 213
             Item 1 Source File    0 of 213   EMPTY ON EVERY ROW

A column that is empty on every row is named. That is exactly what happened to Source File
and Discipline, and nothing said so until you opened the workbook and looked.

## Step 41, a failing check never fails the group

151. Worth knowing so a warning does not read as a disaster.

**Worked:** a report check that finds something wrong says so loudly and the group is
still DONE. The NWF, the NWD and the workbook were all written. Only the report has a
fault in it, and losing a group over that would be worse than the fault.

152. If a check ever says the page could not be read at all, that IS worth sending to me
     with the log. It means the file was not written the way it should have been.

## Step 42, where the column list comes from

153. Nobody typed the client column list into the tool. It was read from two places, and
     the log says so at the end of every check:

    The client column set was read from clash_report_html_tabular.xsl in the Navisworks
    install and from the exports in samples\client-report, never from a list typed into
    this tool.

154. There is a test that re-reads both of your exports and the stylesheet on every build.
     If Autodesk change the layout, or if you send me an export with a different set of
     columns, that test fails and tells me before a report goes out wrong.

155. **If your client ever asks for a different column, send me one export that has it.**
     One file settles it. The set comes from the files, not from a decision.

## Step 43, the workbook is the client's report now

Everything the workbook used to have that theirs does not is gone. That was asked for
before either of us had seen a real Navisworks report. Now that they sit side by side the
ask is one thing, so the workbook is one sheet and nothing else.

156. Open the workbook.

**Worked:** one sheet, named after the report, holding every test one after another. No
Summary sheet, no Matrix sheet, no sheet per test. That is fifty sheets down to one.

**If you want any of those back, say so and I will add them as a separate file rather than
putting them back on this one.** They are not in the client's report, so they are not on
this sheet.

157. Put your own report and ours side by side and compare a block.

**Worked:** same columns in the same lettered columns. Image in A, Clash Name in C, Status
in E, Distance in F, Grid Location in G, Description in H, Clash Point in I, then Item ID,
Layer, Item Name and Item Type twice. Same merges, same widths.

## Step 44, the tests are in the right order

158. Look at the first block on the sheet, and the last.

**Worked:** most clashes first, empty tests last. Ours used to follow the order the tests
sat in the XML, so it opened with four empty tests and you scrolled past hundreds more.

159. The tie rule was measured off your own two reports rather than guessed. Where two
     tests hold the same number of clashes they keep the order they were created in, not
     alphabetical. Your 1A02WN has 1807 tests with no clashes and every one of them is in
     the original order, which is not something that happens by chance.

160. The order is applied to the page and the workbook alike.

## Step 45, the three cells

161. Look at an Item ID cell.

**Worked:** `Element ID: 702888`, the same label as yours. Ours read `Id: 990299`. That
label was never Navisworks naming it for us, it was this tool using the display name of
whichever property matched first, and Id happened to be first in the list. It is your
label now, and the property that actually supplied the number is in the log.

162. Look at Distance and Clash Point.

**Worked:** three decimals, `-0.123` and `x:8.241`. Ours carried the whole double.

163. One thing measured that is worth knowing. Your 1A04WN has nine coordinates that are
     NOT three decimals, like `0.0000000000000373`. Every one is a value that would read
     as `0.000` if it were cut to three, and `0.000` appears nowhere in either of your
     files. So Navisworks never shows a real value as zero, and ours does the same now.

     Whether yours rounds or cuts I could not establish. Both your files carry only the
     finished text and nothing carries the number behind it. Ours rounds.

## Step 46, the check would have caught all of it

164. The self check reported nothing wrong while the order, the label and both number
     formats differed from your files. It was counting whether a column was there and
     nothing else.

**Worked:** it now compares which columns, in what shape, in what order, and names the
first difference with an example from each:

    Client report: The Item ID cell is the wrong shape. Ours reads "Id: 990299" and the
    client's report reads "Element ID: 707077".

165. That is the line that should have found this instead of you opening files.

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

**The one that matters most, and has never worked once.** A second run against unchanged
files has never produced OPENED in a real log. On 2026-08-30 all 22 groups reported
CHANGED, because the comparison was reading the Revit source name rather than the NWC
name. That is fixed and is covered by tests built from the real names off that log, but a
test cannot open an NWF. Step 55 is the only thing that can prove it, and until you run it
the rerun path is UNKNOWN.

For the clash step added on 2026-08-30, everything that could be settled without
Navisworks was settled by reflection over the installed DLLs and is written up in
`docs\scan.md` section 4f. `document.GetClash().TestsData`, `CreateSelectionSource`,
`TestsRunTest`, `ClashTest.Children` as the results, and the numbers behind
`ClashTestType`, `ClashResultStatus`, `PrimitiveTypes` and `Units` are all read off the
real assemblies rather than assumed.

These four are UNKNOWN and only a real run answers them:

- which units `ClashTest.Tolerance` is measured in. There is nothing on the type that
  says, so both numbers go in the log and step 41 is the one that settles it
- whether `TestsRunTest` waits for the test to finish or returns while it is still
  running. If the seconds per test come back near zero and the clash counts are all zero,
  that is what happened, and it is worth stopping for
- whether `TestsAddCopy` takes a copy the way `DocumentSelectionSets.AddCopy` does. The
  name says so and it was proved for sets, so the runner reads every test back by name
  after adding it rather than trusting the object it handed in. If a test is created and
  then reported as `added but not found again by name`, that assumption is wrong
- whether a rerun really does preserve Active and Resolved on tests that are run again.
  Step 43 is the one that answers it, and it needs you to mark a clash Resolved, run
  again, and look

From the run of 2026-08-31, which threw `ObjectDisposedException (WeakRef)` once per test
for nearly nine hours, three more:

- whether the tests that did run actually produced results. They ran, because
  `TestsRunTest` is above the throw in every one of those stacks and returned normally,
  but nothing could read the results, so what they found is UNKNOWN. The NWF from that run
  may well hold real clash results that were never reported
- whether sets, tests or results survive from one group into the next document. The
  evidence says no, because all 24 groups reported 1830 created rather than already there,
  so the collection was empty each time. Every group now logs the count even at zero, so
  the next log answers this outright rather than by inference
- what made the clash step grow from 39.8 seconds to 1259.5 seconds. Two things that grew
  are measured and removed: a per test walk of the whole tests collection, which was
  O(n squared) and built about 1.7 million finalizable native handles per group, and a
  failure list that kept every one of 43920 stack traces. Whether that was all of it is
  UNKNOWN until a run is timed again

From the workbook added on 2026-08-31, four more. The whole of the writing side is tested,
because the writer sits in Federator.Core and the tests write a real xlsx and read it back.
What no test can reach is the harvesting, which needs a live model:

- whether family, type and material actually come back on your Revit models. The tool
  looks for a property whose display name is Family, Type or Material in any category and
  leaves the cell empty rather than guessing when it finds none, so an empty column means
  the property is named something else on your exports. Those names are settings and I can
  change them once you tell me what they are
- whether the grid and level columns fill. They come from
  `document.Grids.ActiveSystem.ClosestIntersection`, so a model carrying no grid system
  leaves both empty, which is not a fault
- whether ClosedXML loads inside the Navisworks process. It builds clean against net48 and
  none of its twelve DLLs collides with a file the Navisworks install ships, both checked,
  but whether the running application binds them is UNKNOWN until step 56
- how long writing 22 workbooks adds to a run

From the load fix of 2026-08-31, one thing is proved and one is not:

- PROVED, and not by a test. A net48 console exe with ClosedXML and Federator.Core, run
  with its own config file deleted, fails with exactly the error your run reported, and
  succeeds once the resolver is switched on. Deleting the config is what makes a process
  behave like a Navisworks add-in, because an add-in has no config of its own. The recipe
  is in docs\scan.md section 4i so it can be repeated
- UNKNOWN. Whether the handler is registered early enough inside Navisworks itself. It
  goes in the static constructor of the plugin type, which the runtime runs before Execute
  and long before anything reaches the workbook writer, but only a real run proves it. The
  log will say `BUNDLE   assemblies are resolved from ...` as its first lines if it is

From the five changes of 2026-08-31, two more. Everything about the grouping choice, the
naming patterns, the collision guard, the findings wording, the findings table and the
single model reason is tested in Federator.Core, so what is left is what the window and
the running application do with it:

- whether the four grouping modes read cleanly on a real folder, in particular whether
  one file per discipline across every building produces something you would actually
  want to open. Nothing tests how it looks
- whether a single model group really does end with a complete NWF, every test in it and
  none of them run. The rule is tested and the wiring is reviewed, but only a run against
  1B06BS proves it

From the seven changes of 2026-08-31, the ones a run has to answer. The folder memory, the
name table, the drift comparison and the open count are all tested in Federator.Core, so
what is left is what the running application does:

- what Clash Detective shows next to a test the log reported as status Old. That is the
  one measurement I cannot take from here and it is the whole of step 11. It maps the word
  to the warning triangle, or proves it does not
- whether Compact actually removes what it says it removes. Take a copy of the NWF first.
  The count comes back from the tally before and after, but only opening the file proves
  the Resolved clashes have gone and that nothing else went with them
- whether applying the file's settings really does reset the results of the tests it
  changes. The tool says it will, because changing a test is documented as doing that.
  Whether it resets only those tests, or something wider, is UNKNOWN until you try it on a
  copy
- whether a dated NWD and an undated NWF sit together the way step 14 describes across two
  real weeks. One run cannot show that, two can

From the client format work of 2026-08-31. The columns, the joins, the picture naming and
the settings are all tested in Federator.Core against the two files you sent, so what is
left needs the running application:

- how long ONE picture takes. This is the one number the whole image feature turns on and I
  cannot measure it from here. TestsImageForResult needs a document open and a renderer. If
  it is a tenth of a second, nobody needs the cap. If it is two seconds, 60 pictures is two
  minutes a group and 22 groups is three quarters of an hour on pictures alone
- whether the pictures are framed on the clash. TestsImageForResult takes a clash result
  and no camera at all, so the view can only have come from the result, but whether it
  applies the clash's own viewpoint internally is UNKNOWN from the DLL. If they come out
  framed on the whole model rather than on the clash, say so and the explicit route is
  written up in docs\scan.md section 4k
- which of the three image styles matches theirs. Scene, SceneUsingRayTrace and
  ScenePlusOverlay are the three that exist. This tool asks for ScenePlusOverlay because an
  overlay is where a clash highlight would live, and that is reasoning rather than a
  measurement. Comparing one of ours against one of theirs side by side settles it
- whether ClassDisplayName is really what fills their Item Type column. Theirs reads Solid
  on all 120 item cells of the report you sent, and ClassDisplayName is what the Item tab
  shows as the type, but only a run against a real model shows the two agreeing
- ANSWERED by the run of 2026-09-01. The Description column fills. Ours reads
  Hard (Conservative) on every row, the same as theirs, so ClashResult.Description does
  carry it
- ANSWERED by the same run. A picture takes 0.079 seconds, 213 of them took 16.9 seconds
  and came to 53.55 MB, so the cap does not need a default other than off

From the client match work of 2026-09-01, what is still open:

- whether the element id is now actually found. The search runs over the clashing item,
  then the element it belongs to, then up its ancestors, and only a real model shows
  whether one of them carries it. If the Item ID cells are still empty, send me one clash
  and the Properties panel for the item
- whether the NWF still holds its RESULTS after the NWD is published, not just its size.
  The size is checked now and reported as intact, changed or gone. Only opening the NWF in
  Clash Detective proves the clashes are in it
- whether Status should say OK. Theirs does, on all 1830 tests. OK is not a value on
  ClashTestStatus, which is New, Old, Partial or Complete, and what puts a test into any
  of them is UNKNOWN, so ours writes what the API reports and translates nothing

From the HTML Tabular work of 2026-09-01:

- ANSWERED. The client format is HTML (Tabular) rendered by Autodesk's own stylesheet, and
  our XML now feeds every column test that stylesheet makes. Proved by transforming with
  the real file out of the install and reading the header row back
- ANSWERED. A supplied export does have a Layer column. 1A04WE has one and 1A02WE does not,
  because the stylesheet writes it only when the XML carries layer data. Ours always does
- STILL MISSING FROM THE REPO. 1104-PAR-1A04WE-XXX-BM-RPT-000001.xlsx and its html and
  _files folder were never committed. Commit them and samples\client-report holds the case
  with a Layer column as well as the two without one
- whether the page opens in Excel the way theirs does. Ours is written by their own
  stylesheet from our data, so it should, and only opening one proves it
- whether the picture links resolve when the page is opened in Excel. Ours are relative and
  theirs are absolute, so ours should survive being moved and theirs should not

From the workbook cell work of 2026-09-01:

- ANSWERED. Status now writes OK where the API says Complete, and anything else goes
  through as itself. Their report says OK on all 1830 tests and ours said Complete, which
  is the enum value. Nothing is translated except that one pair
- ANSWERED. Distance is the rounded number with no format, Layer carries the level, the
  Image cell is empty with the link on it, the rows are 45 and 60 high, the nineteen
  columns are their widths, and every cell is banded and boxed the way theirs is. All
  measured off their export and pinned by ClientLayoutTests

Still open, and each needs a run:

- WHETHER THE DOCUMENT ACTUALLY GOES INTO METRES. This is the one to watch first. The API
  can set each MODEL's units through DocumentModels.SetModelUnitsAndTransform, and there is
  no way at all to set the DOCUMENT's. Whether Document.Units follows from the models it
  holds cannot be read off the DLL. The log says what the document reported before and
  after and says THE DOCUMENT DID NOT FOLLOW in capitals when it did not. Send that line
  back. If it did not follow, the report is still honest, because every number in it and
  the unit beside them are the document's, but the client gets feet
- whether setting the models' units MOVES anything. Each model is handed back its own
  transform and its own reflected flag, so it should not, and only looking at the model
  proves it. If geometry has shifted, the units step is the first thing to switch off
- whether Run the open file does the whole job. It is a new path through the engine and
  every line of it calls Navisworks, so none of it is unit tested. Open one federation,
  press it, and send back the log. What is testable, the naming, is covered by
  OpenDocumentJobTests and lands on the same paths the scanned run uses
- whether the open path leaves the clash history alone. It does not append, does not clear
  and does not Decide, so it should, and the INTACT line after the NWD is what proves it
- whether the banding survives being opened in Excel and saved again. Ours writes the fills
  as explicit RGB and theirs arrived as theme colours through an HTML import, so the two
  files should look identical and only opening both side by side settles it
- whether eleven tick boxes with one visible is too few rather than too many. Three became
  fixed behaviour, so if a week comes where the NWD should not be republished, or the page
  should not be written, or the photos should be skipped, that is a box coming back and it
  should come back rather than being worked around
