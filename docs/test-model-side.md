# Testing the model side

For Bader. This covers scan, group, federate, save NWF, build the sets, create and run the
clash tests, write NWD. There is no Excel yet, so do not look for it.

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

15. Untick any building you do not want in this run.

## Step 3, outputs

16. Click the **3. Outputs** tab.

17. Click the first **Browse** and pick the folder the NWF files go in.

18. Click the second **Browse** and pick the folder the NWD files go in.

19. Leave **Republish the NWD every run** ticked.

**Worked:** the line under the table says `The NWD is republished every run.` Untick it
only when you want the NWF opened and compared without a new NWD being written.

**Worked:** the table shows the output name for every group before anything runs, for
example `1104-PAR-1C07BC-ZZZ-BM-MOD-000001`. Check one against a building you know. It
should be the project code, the originator and the building code from the input files,
then always `ZZZ`, `BM`, `MOD` and `000001`. Level, discipline, type and number are all
pinned, because the outputs overwrite on the next run.

## Run it

20. Click **Run**.

21. Read the warning box before clicking anything.

**Worked:** it names what you are about to lose, for example
`The open file C:\models\something.nwf, holding 4 models.` If nothing is open it says so
instead. This is your last chance, because the run clears the document before each group
and does not save it first.

22. Click **Cancel** the first time, on purpose.

**Worked:** the log says `Run cancelled before anything was cleared.` and nothing on disk
changed. Check the NWF folder is still empty.

23. Click **Run** again, then click **OK**.

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

24. Open the NWF folder.

**Worked:** one `.nwf` per group you ticked, named exactly what step 17 showed, with no
date and no version number on the end.

25. Open the NWD folder.

**Worked:** one `.nwd` per group, same names.

26. Look at the bottom of the log box in the window.

**Worked:** a `RESULT` block, which is the summary you do not have to scroll for. It reads
groups done, groups partial, groups failed, then every file written with the size that was
read back off the disk, then every error repeated in full, then the total elapsed. If
nothing went wrong the errors section is the single line `Nothing failed.`

27. Open one of the NWD files in Navisworks and check every discipline of that building is
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

28. Click **Open log folder** at the bottom of the window.

**Worked:** Explorer opens with this run's log file already picked out.

29. Click **Copy log**.

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

31. Open any NWD or NWF that has real content in it.

32. Click the **4. Clash** tab.

33. Click **Browse** beside **Clash XML** and pick your file. The reference file
    `1104-PAR_CLASH_AllInOne (2) (1).xml` holds both halves and is a good first try.

**Worked:** the line underneath says what the file actually holds, counted out of the file
itself, for example `Picked ... It holds 61 sets and 1830 tests.` A file holding only
tests reads `0 sets and 1830 tests` and is still fine, as long as the sets are already in
the model.

34. Click **Sets into open model**.

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

35. Open the Selection Sets window in Navisworks and check the folders.

**Worked:** the folders nest exactly as the file had them, so `Mechanical` holds
`Mechanical-HVAC` and the rest as real folders rather than as sets with long names.

**Failed:** a line reads `FAILED` with an exception on it, or `SKIPPED` with a reason. A
`SKIPPED` line means the file used a condition test this tool does not rebuild, and it
names the test. Only an unknown test value causes that. An internal property name nobody
has seen before is passed straight to Navisworks rather than being treated as a problem.

Nothing here is tied to one project. The file is picked every run, and none of its names,
folder names, counts or internal property names are written into the tool.

36. Try it with a file that holds only tests and no sets.

**Worked:** it says `This file holds no sets. Nothing to build.` and does nothing else. A
project that keeps its sets in the model and supplies only tests is a normal case, not an
error.

## Step 5, the clash tests

Same tab, same file. This creates the tests the picked file describes and runs them
against whatever is open. The sets have to be there first, either because you built them
in step 4 or because they already live in the model.

37. Leave the **Clash XML** box exactly as it is. It is the same file.

38. Click **Tests into open model**.

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

39. While it runs, watch the progress line and the log.

**Worked:** a running count every 25 tests, so a run of well over a thousand is watchable
rather than silent:

    CLASH    250 of 1830 tests, 8 run, 242 skipped, 31 clashes so far

40. Open Clash Detective in Navisworks and compare.

**Worked:** the tests are there under the names the file gave them, each side shows the
selection set by name rather than a list of items, and the clash count on a test matches
the number in the block above. That last one is the point. If a count disagrees with the
panel, that is a real fault and the log line for that test has the numbers to send back.

41. Look at any line in the log that begins `CLASH    created`.

**Worked:** it carries the tolerance twice, in both units, for example
`tolerance 0.2460629921 ft is 74.9999999921 mm`. Which units Navisworks measures the
tolerance in is not something that could be read off the DLL, so both numbers are logged
and this is the line that settles it. Check one test in Clash Detective and tell me which
of the two numbers its tolerance box shows.

42. Try a file that holds tests but names sets that are not in this model.

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

43. Open a model with no selection sets in it at all, pick a clash test XML, and press
    **Tests into open model** without building the sets first.

**Worked:** it stops immediately, before creating anything, and says so in one line in the
log and on the line under the box:

    CLASH    STOPPED  the document holds 0 sets and the tests name 61 sets, so no test can
             resolve a set. Nothing was created and nothing was run.
    CLASH    build the sets first, or pick a file whose tests name the sets this document
             already holds

**Failed:** it goes ahead and reports 1830 skipped, 0 created, 0 run. That is what it used
to do, and it took a whole run and a 1 MB log to say one thing.

## Step 7, the run stops itself when everything is failing

This is the one that would have saved you nine hours. On 2026-08-31 a run went through 24
groups over 8 hours 52 minutes, created 1830 tests in every one of them and produced
nothing, because every single test threw the same exception and the run carried on
regardless.

That exception is fixed. This is the net underneath it.

44. Run normally. If anything has gone wrong in the same way for the first 50 tests,
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

45. Look at the size of the log.

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

46. Look at the `CLASH` lines at the top of each group.

**Worked:** every group says how many clash tests the document already held, even when
that is none:

    CLASH    the document already holds 0 clash tests, so everything created here is new

That line is there to answer a question I could not answer without a real run: whether
tests, sets or results survive from one group into the next document. If group 2 onwards
says a number other than zero, they do survive, and that is worth telling me.

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

47. Run once so an NWF exists, then run again with the same settings and the same folder.

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

48. Now add one NWC to the source folder, or remove one, and run again.

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

49. After any run, find the `SOURCE FINDINGS` block near the end of the log.

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

50. Click **Run** again with the same settings.

**Worked:** the same NWF and NWD file names are overwritten in place. No second copy
appears, no date suffix, no `(2)`. You get a brand new log file, because logs are never
overwritten.

## The order within a group, which changed

51. Read one group's worth of log from `GROUP` to `GROUP`, and check the order.

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
test cannot open an NWF. Step 47 is the only thing that can prove it, and until you run it
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
