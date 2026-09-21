# 03 Bader next

**The last run, 2026-09-20 16:20, was the worksets round proving run against HIS OWN C02 folders, log `run-20260920-162006.log`, and it reads 8 groups DONE and 2 FAILED. The two are 1A02MM and 1A02WL, failed on purpose under Q70 because a structural model in each was exported on Revit`s internal origin, and BOTH still wrote their NWF, their NWD and their report. THE WORKSETS ROUND IS NOT ON MAIN YET: it is the branch `round-worksets` and has to be merged before a build here carries any of what these steps describe. THE ADD-IN BUILDS, 0 errors and 0 warnings.**

One action per step. Do them in order. One build, one install and one Navisworks session cover every proof: the window checks first, then the C06 rebuild run, then one building twice, then the rest.

F34 comes first, because it is the largest add-in change of the round, the window, the XAML, the engine and the job all changed, and it is proved by opening the window and looking, before any run. If the window does not open or the Clash step is wrong, nothing after it can be read, so it is checked before a single group runs. F33 is checked in the same look, because it is one combo on the same window. F27 is next, because it costs one Scan and no run at all: its block is read off the 1B06PH run further down. Then the C06 run, which proves F24, F29, F30 and F33 in one press, then F28 and F31 together, because both are one building run twice. F32 and F35 need a different file open or a folder made for them, so they come after the ordinary runs.

## THIS FILE NOW COVERS TWO FOLDERS, AND THE OLD ONE NEVER EXISTED

Everything below is done once on `C:\00-NM\Federation Task\C02 + 04\C02`, which holds TEN
groups and already holds NWF files so its groups are Weekly runs, and once on
`C:\00-NM\Federation Task\C02 + 04\C04`, which holds TWELVE groups and NO NWF at all so
every group there is a FIRST RUN.

THE `C06` FOLDER AND THE `1B06*` GROUP NAMES ARE GONE FROM THIS FILE. They were never on
his machine, and every count in here derived from fourteen groups was wrong for that
reason. The real groups are:

    C02, ten:    1000BS  1A0215  1A02BS  1A02MM  1A02MS
                 1A02WE  1A02WL  1A02WM  1A02WN  1A02WO
    C04, twelve: 1A0415  1A04EP  1A04KI  1A04MS  1A04PK  1A04PW
                 1A04WE  1A04WL  1A04WM  1A04WN  1A04WO  1WAW15

FOUR THINGS ABOUT C04 BEFORE YOU READ ANY NUMBER OFF IT. `1A0415` and `1WAW15` are NOT
buildings: they carry LS, LT and SW, and every set in the matrix is a `BLD-` set asking
for one of nine codes that does not include any of those three, so both find NOTHING and
that is correct rather than a fault. `1A04MS` and `1A04PK` hold ONE discipline each, both
ST, so there is no pair to clash. `1A04WL`'s ST files are numbered 1, 2, 3, 4, 5 and 7,
and SIX is missing, which is named and not guessed at. And `1A04WL`'s ST-000004 and
ST-000005 are on Revit's internal origin, which fails that group on purpose.

AND THE FIXTURES COME FIRST NOW. Before either live folder, run the two small copy groups
under `C:\Users\bader\AppData\Local\Temp\claude\round-close`, which take about twenty
seconds each and exercise sets, tests, clashes, viewpoints, by design, the workbook, the
page and the penetration rule. `tools\probes\fixture.md` says what each one is, why the
penetration one is AR and ME rather than ST, and what neither can show. A live run is the
final proof and never the loop.

## Get the code

1. Open GitHub Desktop
2. Pick the repo PAR_NWC-Federator-V2
3. Make sure the current branch is main
4. Press Fetch origin
5. Press Pull origin if it appears
6. Open the repo folder in VS Code

## Build and install

7. In VS Code open a terminal
8. Build:

```
dotnet build ParsonsNwcFederator.sln -c Release
```

9. If the build says a Navisworks DLL is missing, run with the install path:

```
dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"
```

10. Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`. That was proved on this machine on 2026-09-19 by F69, which is the first build this repo has ever had, so anything else here is new and the whole output goes into the chat. `CreateCopy` and `CopyFrom` on `DocumentSelectionSets`, which F24 and F29 could not measure, are measured now and written into `docs\history\scan.md` 4d, so a build naming either of them is a real fault and no longer an open question
11. Look for: the build stamp. If it reads `nogit` where an eight character commit hash belongs, git is not on the PATH for the terminal that ran the build. The build itself is fine and only the stamp is affected, and the title bar and the log will read `nogit` until you build from a terminal that has git on it

### If the build fails on NuGet rather than on the code

This is the failure Bader hit on 2026-09-19 before he ever reached a compiler error, and it has nothing to do with the code:

```
C:\Program Files\dotnet\sdk\10.0.400\NuGet.targets(198,5): error Cannot create a file when that file already exists.
```

`dotnet build` restores all three projects at once, they collide on the same package folder, and it is a known race in NuGet's restore task. The four steps below go cheapest first.

12. Run the same build command again, exactly as it was:

```
dotnet build ParsonsNwcFederator.sln -c Release
```

13. Look for: the second build finishes with no error. The race is intermittent, so repeating the build is the first thing to try and it costs nothing
14. If the same error comes back, delete every `obj` and `bin` folder, because a half written package folder is what the race leaves behind:

```
Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory | Remove-Item -Recurse -Force
```

15. Look for: the command prints nothing. Run `Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory` on its own afterwards and look for: it prints nothing either. There are six of these folders when they are all there, two per project
16. Restore on its own, one project at a time, which is what takes the race out:

```
dotnet restore ParsonsNwcFederator.sln --disable-parallel
```

17. Look for: three `Restored` lines and no error, one for `Federator.Core`, one for `Federator.Core.Tests` and one for `Federator.Addin`. If the `obj` folders were not deleted it says `All projects are up-to-date for restore` instead, which is also fine
18. Build without restoring again, so nothing can collide:

```
dotnet build ParsonsNwcFederator.sln -c Release --no-restore
```

19. Look for: the build finishes with no error. If it says a package or a reference assembly is missing, the restore at step 16 did not finish and that is the step to read again
20. Only if every step above failed, clear the NuGet cache. It is LAST because it re-downloads every package this solution uses, and nuget.org is a hard requirement for building at all:

```
dotnet nuget locals all --clear
```

21. Look for: it finishes with no error. Exactly what it prints was not measured here, because clearing the cache on this machine would cost the re-download it warns about. Then go back to step 8 and build again, which will pull every package down fresh

22. Install:

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

23. Read the last lines of the install. Look for: a line saying all the expected files are present and a line saying every reference is satisfied. If it stops with Install incomplete, copy the whole output into the chat

## Proof F34, the window, and F33, the units combo

24. Close Navisworks if it is open
25. Open Navisworks Manage 2025 with nothing open
26. Open the add-in from the ribbon
27. Look for: the window title carries today's commit and build time, not be0b9b37
28. Look for: the window has four tabs, NUMBERED, reading `1. Source`, `2. Grouping`, `3. Outputs` and `4. Clash`
29. Go to the Outputs step
30. Look for: the Model units combo lists `Metres, which is what the models are set to`, then Millimetres, Centimetres, Feet, Inches, in that order, with the first chosen
31. Look for: the grey line under it says the report is always in metres
32. Go to the Clash step
33. Look for: there is no Outstanding counts combo on the Clash step. It used to sit under the tick boxes
34. Look for: the two buttons read `Sets into open model` and `Tests into open model`, BESIDE the line, on one row with the line to their LEFT, which the XAML settles: the TextBlock and the two Buttons are the three children of one horizontal StackPanel. The step said UNDER and the labels and the line are exactly right, which is F57. The line is `Not part of the run. Try the file above against the model open right now:`
35. Pick the clash XML, 1104-PAR_CLASH_AllInOne
36. Look for: the line at the very BOTTOM of the Clash step, just BELOW the sets pane and NOT under the file box, ends with `Every locator resolves against its sets.` The sentence goes into `SetsSummary`, which is docked Bottom, so it is below the pickers, the tick boxes and all four bordered panels
37. Look for: the log pane holds a block headed `HEALTH 1104-PAR_CLASH_AllInOne.xml` with `Tests: 1830`, `Sets: 61`, a `Locators resolved` line whose two numbers are equal F84 added four more lines to that block since this step was written: `Sets asking exactly the same question:`, `Revit categories known: 374`, `Sets asking for a category no model carries:` and `Set names breaking their folder's pattern:`, so the three above are no longer the whole block.
38. Clear the clash XML box

## Proof F27, the GROUPS block tells the truth

39. Go to the Source step, pick the same C06 NWC folder as last time and press Scan
40. Look for: the count line under Scan reads files found, files readable, groups and the findings by kind, then the findings themselves
41. Go to the Outputs step and pick the same NWF folder and NWD folder as last time
42. Go to the Grouping step
43. Untick two groups, 1B06PH and 1B06G1, which is the only thing in this tool that drops a group from a run
44. Do not press Run here. The GROUPS block is written when a run actually STARTS, so a Run followed by a Cancel leaves nothing to read, and the preview behind that Run opens each NWF and leaves the last one open, which would stop the next proof counting the rebuilt groups
45. Tick 1B06PH and 1B06G1 again, so the next proof runs every group
46. Look for, F27, later: the GROUPS block of the one building run at step 76 lists that building with `run`, the other NINE on C02 or ELEVEN on C04 with `unticked`, and the word `skipped` nowhere
47. Look for, F27, in the same block: one line after the list reads `9 groups unticked in the Run column, nothing else drops a group` on C02, or `11 groups` on C04
48. Go on to the next proof with every group ticked and nothing open in Navisworks

## Proof F24, the six CHANGED groups are rebuilt from the scan, with F29 and F30 in the same run

49. Leave the clash XML box empty
50. Tick every group and press Run
51. Look for: the label at the bottom reads `Checking NWF 1 of 10: 1000BS` on C02, or `1 of 12: 1A0415` on C04, and counts up with the BUILDING changing on each, then the confirm dialog opens. The count is the ticked group count and the name comes after a colon
52. Look for: the dialog says `Rebuilt: 6` and that the NWF is BROUGHT UP TO DATE, what is gone taken out and what is new appended WITHOUT clearing it, so the sets, the tests, the results and the viewpoints never leave it, with the clear and rebuild named as the fallback. IT SAID "cleared and rebuilt" UNTIL 2026-09-21, which stopped being true when PART 5 made the reshape the path a CHANGED group takes, and this is the one screen you can still cancel from
53. Press Cancel
54. Look for: the log says `Run cancelled before anything was cleared.`
55. Go to the Grouping step
56. Look for: 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE read Rebuilt in the Run as column, and the other eight read Weekly run
57. Press Run again, then OK, and wait for it to finish. The confirm dialog now also says the NWF the preview opened will be discarded, which is right, and the preview does not run a second time for the same reason
58. Look for, F50: per rebuilt group the log has a CHANGED line with the counts, then a REBUILT block, `REBUILT <nwf>  1 added, 4 moved, 0 removed` for 1B06BC, then one line per file saying added, moved or removed, then an `APPEND   attempt` line per file coming in, then ONE `RESHAPE` line saying how many were taken out and how many appended `without clearing the document`, then FOUR lines together, starting `SETS`, `TESTS`, `VIEWS` and `RESULTS`. The RESHAPE line is PART 5's and did not exist before the drift round: a CHANGED group is brought up to date without clearing now, and the clear and rebuild is only the fallback. Before F50 there were two tally lines and they read in two different shapes
59. Look for, F50: each of those four says `kept` or `none`, and never `LOST` and never `NOT COUNTED`. Any one of them saying either means the NWF on disk was left alone on purpose. The `LOST` line says so ITSELF, ending `The NWF on disk was NOT saved over, so it keeps them`, and only the clear and rebuild fallback adds a separate `NWF      NOT saved over` line under the four. On the reshape path that now runs, nothing is logged after the tally and the reason is on the group and in the RESULT block
60. Look for, F29 and F50: the `SETS` line reads one of two shapes and BOTH are right. Where the clear kept the sets it carries TWO numbers, `kept: before clear N, after appends N, nothing to put back`, which is the ordinary case. Where the count dropped and the copy was put back it carries THREE, `kept: before clear N, after appends N, after restore N, put back from the copy`, and then the first and the third have to agree. The other three lines read the same way, because four things reading four ways is how a log stops being read. A line reading `LOST` or `NOT COUNTED` is the fault, and step 59 is where that is checked
61. Look for, F33: per group a `UNITS` line. Where the document was NOT already in the wanted unit it says how many models were set and what the document shows, in the words `model set` or `models set`. Where it already was, the one line reads `the document is already in <unit>, so nothing was changed.` and that is not a fault, it is the branch a weekly run over NWFs this tool has already set takes, so expect that one. THE OLD SECOND HALF OF THIS STEP IS GONE. It said to check that no line anywhere reads DID NOT FOLLOW, and that string is in no source file, so it was a check that could never fire. The words went when it was measured that a MODEL can be set and a DOCUMENT cannot, and that what Document.Units reports is UNKNOWN
62. Look for, F30 and F52: per group, after the models are in, the blocks come in this order: UNITS, then `ALIGNMENT` and `EXPORT CHECK`, which the alignment round added and which are read where every model is open and nothing has clashed, then CLASH, then `EMPTY SETS` where any set found nothing, which PART 3 added and which on his folders is every group with the XML picked, then `PENETRATION` and `BY DESIGN` where their boxes are on, then `ITEM IDS` where the report carries item ids, then `VIEWS` and `VIEWS BUILT`, which are TWO blocks and not one, then `NWF      attempt`, then XLSX and `WORKBOOK CHECK`, then the page and `REPORT CHECK`, then XML where its box is on, then NWD, then CONFIRM, the final NWF line saying intact and the size, and the `GAP` block. It is XML there and NOT a second XLSX, which this step named twice. The `VIEWS` line sits between the clash step and the NWF because F52 makes the viewpoints there, so a viewpoint this run made is inside the file the NWD is published from. No SETS block follows UNITS on this run, because sets come from the clash XML and the box is empty. The four SETS, TESTS, VIEWS and RESULTS lines the rebuilt groups carry are written during the rebuild, before UNITS
63. Look for: the GROUP finished line of each of the six reads the building, then `DONE`, then the seconds, and ends with `Rebuilt`. UNTIL 2026-09-21 IT READ `PARTIAL` AND `Skipped (changed on disk)`, because the reshape never said it had rebuilt the group and the outcome kept the value it was given BEFORE the work
64. Look for: the RESULT block carries `rebuilt        : 6` and `weekly run     : 4` on C02, which is ten groups in all. Until 2026-09-21 it read `rebuilt        : 0` and counted them under `skipped`
65. In Explorer open the NWF folder and open the NWF of 1B06BC in Navisworks
66. Look for: the Selection Tree lists 5 models, EL among them, all from the Published folder
67. Look for, F29: the Sets window holds the same sets as it did before the run, Mechanical with its four subfolders among them
68. Open the NWD of 1B06BC from the NWD folder
69. Look for: the same 5 models
70. Look for: no group in this run reads PARTIAL. Every RESHAPED group read PARTIAL until 2026-09-21 with the reason "the NWF points at a different set of files, so it was left alone", about a group that had just been brought up to date
71. Close the NWD and the NWF so nothing is open

## Proof F28 and F31, one building twice with the XML

72. Go to the Clash step and pick the clash XML
73. Go to the Grouping step and tick 1B06PH only
74. Press Run
75. Look for: the confirm dialog says `Weekly run plus XML: 1` and its line ends with `Nothing inside it is cleared.`, which is the wording since F75 and not the older `Nothing is cleared.`
76. Press OK and wait for it to finish
77. Note the time the run took, from the GROUP started line to the GROUP finished line
78. Look for: the log says OPENED for the group
79. Look for: one line per set starting `SET      `, each ending `already there, left alone` where the NWF already held it from last week, and one line `SETS     <building> put into the document: 0 created, 61 already there and left alone`. THERE IS NO PER GROUP SETS BLOCK IN THE LOG. F81 deleted it, because SetBuilder already writes a line per set as it goes, and the `sets created      :` and `already there     :` totals only reach the window's sets pane, from the Sets into open model button
80. Look for: the line `CLASH    source   tests from XML` and the tests running after it
81. Press Run again on the same building, same folders, same XML
82. Press OK and wait for it to finish
83. Look for, F28: the line `SETS     <building> put into the document: 0 created, 61 already there and left alone`, and a `SET      ` line for every one of the 61 ending `already there, left alone`. THE TOTALS AND THE Q72 LINES ARE NOT IN THE LOG. `sets created      : 0`, `already there     : 61, left alone, not copied again` and the three indented Q72 lines under them are in the window's SETS PANE and only when the Sets into open model button is pressed. In the pane, the third of those Q72 lines now says what this run FOUND, either `none of them drifted`, or `N of them DRIFTED and none was rebuilt, because the box is off`, or `N of them DRIFTED and M were REBUILT from the picked file`. It used to say a set finding nothing MAY be asking a question the file no longer asks, which stopped being true when PART 3 read the question and PART 2 rebuilt it.
84. Look for, F28: the line `SETS     <building> put into the document: 0 created, 61 already there and left alone`. It is a line of its own and not part of a SETS block, because there is no SETS block in the log since F81. ONE `NWF      attempt` line follows the CLASH block and not a second one, which is F57. This group is a Weekly run plus XML, so it takes the OPENED branch, and that branch logs `NWF      reused` and never an attempt BEFORE the clash step, so the one after it is the only one and their results went into the document
85. Look for, F31: the DRIFT block reports no locator difference on any test. A locator difference on every test would mean the source held since the sets were indexed does not compare equal to a fresh one, which is the one thing F31 could not measure here
86. Look for, F31: the CLASH block shows the same created, already there, run, skipped and passed counts as the first run
87. Look for, F31: the second run took no longer than the first

## Proof F35, a group of one discipline creates every test and runs none

88. In Explorer make a folder on the desktop named OneDiscipline
89. Copy the AR NWC of 1B06PH into it
90. Copy it again inside that folder and rename the copy so its number reads 000002 instead of 000001, so the folder holds two AR files of one building
91. In the add-in go to the Source step, pick the OneDiscipline folder and press Scan
92. Look for: the findings say SINGLE DISCIPLINE for 1B06PH
93. Go to the Grouping step
94. Look for: the status of `1A04MS` reads `Ready. One discipline, so every test is created and none is run.` THAT STRING IS NOW FALSE AND IT IS THE CODE THAT IS WRONG, not this step. Since F77 a test whose side finds nothing is never created, and in a one discipline group that is nearly all of them, which is what step 98 measures. The CLASH line was corrected and this window label was not. Report it
95. Go to the Outputs step and pick a new empty folder on the desktop for the NWF and the NWD, so nothing under C06 is touched
96. Leave the clash XML picked and press Run, then OK, and wait for it to finish
97. Look for: the log line `CLASH    1B06PH holds one discipline, so no test is run, and only the tests whose sides both find something are created. One discipline cannot clash with itself.` This step used to quote `so every test is created and none is run`, which the code has never written since F77, so it was a check that could never pass
98. Look for: one line `CLASH 1830 in the file, N created, M not created, a side finds nothing`, and in the CLASH block two skip reasons with counts, `the group holds one discipline, so nothing in it can clash` for every test that WAS created and `a side finds nothing in this model` for the rest. SINCE F77 THE CREATED COUNT IS FAR BELOW 1830, because a test whose side finds nothing is never created at all, so the two reasons split the 1830 between them. This step used to say 1830 created and 1830 skipped, which F77 made impossible
99. Look for: the GROUP finished line reads the building, then `DONE`, then the seconds, then which path it took. `DONE` is never the last word on the line
100. Put the Source folder back to the C06 NWC folder and press Scan
101. Put the NWF folder and the NWD folder back to the C06 ones

## Proof F26, the report is always in metres

This one is its own small run, with the clash XML, because the conversion only happens where a report is written.

102. Go to the Outputs step
103. Set the Model units combo to Feet, on purpose, so the models are put into feet
104. Go to the Grouping step and tick 1B06PH only
105. Press Run, then OK, and wait for it to finish
106. Look for: the line `model units      : Feet, which is what the models are set to`
107. Look for: the line `report units     : Meters (m), always, converted before anything is written`
108. Look for: one line `UNITS    every number converted from ft to m, one ft is 0.3048 m, ...` before the XLSX line
109. Open the workbook in the Clash Reports folder beside the NWF folder
110. Look for: the Tolerance cell of the first block reads `0.075m`, not `0.246ft`. This assumes the ORIGINAL 75 mm XML with the tolerance drop down left on `Use the value in the XML`. F76 added that drop down and Q66 made 25 mm the project standard, so a run at 25 mm reads `0.025m` here instead.
111. Look for: the Distance column holds metres, so a hard clash reads about -0.050, not -0.164. The same caveat as the step above: this is the 75 mm XML.
112. In Navisworks open Clash Detective and select that test
113. In Options, Interface, Display Units, set Linear Units to Metres
114. Look for: the panel tolerance reads 0.075 and one clash distance matches its workbook row to three decimals
115. Go back to the Outputs step and set Model units to Metres

## Proof F22, the labels before a run

116. Go to the Outputs step and pick an NWF folder that holds no NWF yet
117. Go back to the Grouping step
118. Look for: every group reads First run
119. Go to the Clash step and pick the clash XML
120. Look for: every group still reads First run, because no NWF is there yet
121. Put the NWF folder back to the C06 one
122. Look for: every group reads Weekly run plus XML

## Proof F71, the window says when an NWF is nearly matched

This is what the run of 2026-09-19 could not tell you. The tool reuses an NWF that is at
the output path and builds one when nothing is there, which is right, and it said nothing
about a file sitting beside it under a nearly identical name.

123. Go to the Outputs step and pick the C06 NWF folder, the one that already holds NWF files
124. Go to the Grouping step and look at the NWF Name column
125. Change the NWF row's Level field on the Outputs step, which is the box named `NwfLevel`, the FIRST box on the NWF row, and not the NWD or workbook Level box UNDER it, from `ZZZ` to `L01`, so every name a group would write now differs from the file on disk in one field only
126. Go back to the Grouping step
127. Look for, F71: every group whose NWF is on disk now reads `First run` in the Run as column, FOLLOWED BY `, but the NWF folder holds a similar name:` and the name of the file that is really there. Before F71 it read `First run` and nothing else, which is what cost the time
128. Look for, F71: one grey line under the group table reading `N groups are set to build a new NWF beside a file whose name is nearly the same.` and then `Type the NWF Name cell over to point at the existing file, or leave it to build a new one.` The number is how many groups are in that state
129. Look for, F71: nothing was changed for you. The Run column is still ticked, the NWF Name cells still hold the new name, and no name was corrected. The tool reports and you decide
130. Type the NWF Name cell of one group over with the name of the file that is really there, without its extension
131. Look for, F71: that row now reads `Weekly run` or `Weekly run plus XML` with no sentence after it, and the line under the table counts one group fewer
132. Put the Level field back to `ZZZ`
133. Look for, F71: every row reads `Weekly run` or `Weekly run plus XML` again and the line under the table is gone
134. Change the Level field to `L01` again and then pick an NWF folder that holds NO NWF at all
135. Look for, F71: every row reads `First run` with no sentence after it, and no line under the table. An empty folder holds nothing to be near, so there is nothing to say
136. Put the NWF folder back to the C06 one and the Level field back to `ZZZ`

## Proof F9, a changed folder is rebuilt and not run in feet

137. Copy one NWC of 1B06PH out of the NWC folder to the desktop, so the folder lost one file
138. Close whatever is open in Navisworks, so the preview can open each NWF, then press Scan again and tick 1B06PH only
139. Press Run
140. Look for: the dialog says `Rebuilt: 1`
141. Press OK
142. Look for: the REBUILT block names the removed file with `removed`
143. Look for: no UNITS line comes before the REBUILT block
144. Copy the NWC back into the NWC folder
145. Press Scan again, tick 1B06PH, press Run, press OK, so the NWF holds all its files again
146. Look for: the REBUILT block names the file with `added`

## Proof F10, each output on its own switch

147. Go to the Outputs step and open More
148. Tick Write a clash XML beside each workbook
149. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
150. Tick 1B06PH and press Run, then OK
151. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
152. Look for: `IMAGES   skipped  images are switched off for this run`
153. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
154. Untick Write a clash XML and tick New, Active and Reviewed again
155. Press Run again, then OK
156. Look for: `XML      skipped  not wanted this run`
157. Look for: the pictures rendered again, counted on their own `IMAGES` line after the CLASH block. The CLASH block counts clashes and never pictures

## Proof F17, the pictures are numbered in the export order

158. Stay on the run just finished
159. Look for: one line `IMAGES   numbered in report order: <n> renamed, <n> already right, 0 missing` after the CLASH block
160. Open the Clash Reports folder beside the NWF folder and open the 1B06PH workbook in Excel
161. In Navisworks open Clash Detective and select the test that is the first block of the workbook, the one with the most clashes
162. Look for: the clashes on the sheet are in the same order the Clash Detective panel lists them
163. Look for: the Image cell on row 1 of that block links to cd000001.jpg, row 2 to cd000002.jpg, and so on
164. Look for: the second block's pictures start at cd010001.jpg
165. Click the Image cell on three rows in three different blocks
166. Look for: every link opens its picture, and the picture shows the two items named on that row
167. In Explorer open the `_files` folder beside the workbook and look for: no file ending `.moving`

## Proof F8 on the scanned run, no XML

168. Clear the clash XML box on the Clash step
169. Go to the Grouping step
170. Look for: 1B06PH reads Weekly run
171. Press Run, then OK
172. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
173. Look for: the CLASH block says the tests ran. IT SAYS NOTHING ABOUT THE WORKBOOK, which is F57: the block is `ClashRunOutcome.Lines` and it names no output file at all. The workbook has its own `XLSX     attempt` and `XLSX     written` lines after it
174. Look for: no `SET      ` line and no set line of any kind INSIDE the group, because no XML means the sets are left alone. The `SETS ACROSS THE RUN` block at the END of the run is still written, unconditionally, and with no XML it reads zeros and `Every set found something somewhere.`, which is right and is not the group's block

## Proof F32, an NWD open is refused

175. In Navisworks open the NWD of 1B06PH from the NWD folder
176. Open the add-in from the ribbon and go to the Clash step
177. Look for: the line BESIDE Run the open file, to its right, reads `This document is a .nwd file and this tool runs an NWF, which is where the clash tests and their results live. Open the NWF instead.` and the Run the open file button is greyed
178. Close the NWD

## Proof F6, the open file report folder, with F30 and F32 on the NWF

179. In Navisworks open the NWF of 1B06PH
180. Open the add-in from the ribbon
181. Go to the Clash step
182. Look for, F32: the blue line BESIDE Run the open file, to its right, starts with `Weekly run.`, names the NWD it will write beside the NWF and names one Clash Reports folder beside the NWF, and the button is enabled
183. Leave the clash XML box empty
184. Press Run the open file and wait for it to finish
185. Look for, F30: after the models are in, the blocks come in the same order as on the scanned run: UNITS, then `ALIGNMENT` and `EXPORT CHECK`, which the alignment round put on the one path BOTH runs pass through so the open file run writes them too, then CLASH, then `EMPTY SETS` where any set found nothing, then the `VIEWS` blocks, then `NWF      attempt`, then XLSX and `WORKBOOK CHECK`, then the page and `REPORT CHECK`, then XML where its box is on, then NWD, then the final NWF line. It is XML there and NOT a second XLSX, which this step named twice
186. Open the NWF's folder in Explorer
187. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
188. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

189. Stay on the run just finished
190. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
191. Look for: the CLASH block says the tests ran, and the workbook is on its own `XLSX     attempt` and `XLSX     written` lines after it, never in the CLASH block, which is F57

## Proof F7, the RESULT block and the log copy

192. Stay in the same NWF folder in Explorer
193. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the Run the open file press
194. Open that log in Notepad
195. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
196. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
197. Look for: a RESULT block at the end with groups done, partial and failed
198. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

199. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
200. Copy every `run-*.log` written today
201. Paste them into `steps\logs` in the repo folder
202. Rename each with the DATE OF THE RUN, the building and the press. The shape, with 2026-09-14 standing for whatever day you run them: `2026-09-14-C06-rebuilt.log`, `2026-09-14-1B06PH-first.log`, `2026-09-14-1B06PH-second.log`, `2026-09-14-onediscipline.log`, `2026-09-14-1B06PH-noxml.log` and `2026-09-14-1B06PH-openfile.log`
203. Open GitHub Desktop
204. Press New Branch, name it `logs-` and the date of the run, so `logs-2026-09-14` on the fourteenth, and press Create Branch
205. Write a summary like `logs from C06, F34 to F7 proofs`
206. Press Commit
207. Press Publish branch
208. Press Create Pull Request, and tell the worker in the chat that it is open. The worker merges it and reads the logs. Nothing goes to main directly, that is the wall F38 put up

## Run the window probes

209. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-defaults.ps1
```

210. Look for: it prints the window defaults and no error about a path
211. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-labels.ps1
```

212. Look for: it says how many tick boxes are visible without opening anything, which must be 4: `Include subfolders` on the Source step, and `Mark penetrations as Reviewed`, `Mark by design connections as Reviewed` and `Rebuild sets that drifted from the file` on the Clash step. It was 3 until the drift round added the rebuild box under the by design one, Q72, outside the destroy data expander on purpose because 5v measured that it destroys nothing. It was 2 until F72b added the by design box beside the penetration one, and reports no label over eight words and no help line over twelve. It was 1 before F72 added `Mark penetrations as Reviewed` to the Clash step, which is visible on purpose because a weekly run genuinely has to choose whether this run writes statuses into the NWF

## Proof F41, the handles, off the two runs above

213. Before you press Run the second time in step 81, open Task Manager, the Details tab, right click a column heading, Select columns, and tick Handles
214. Look for: the handle count on the Navisworks row at the end of the second run, beside the one at the end of the first. Copy both into the chat. F41 releases every wrapper this tool creates where it is finished with it instead of leaving it to a finalizer, so a second run over the same 61 sets and 1830 tests should not sit higher than the first. A count that climbs run on run is the thing worth reporting
215. Look for, in the log of either run: no line saying a set was not at the index the count gave before the add. That line is written only when the tree is not the shape the fast read assumes, and it means the set was then looked for by name

## Proof F45, the clash step keeps its rules, off the two runs above

216. Look for, in the DRIFT block of either run: either no line about sides left out at all, or one saying how many sides were left out because which set one end points at could not be read. That line is new and says not compared is not the same as matching
217. Look for: a block headed `ITEM IDS 1B06PH`, one line per property that supplied an item id with a count out of the total, and never a line per item. Each of THOSE lines ends `written as "Element ID"`, and a line reading `no id property` is the items that carried none, which is not a property name. WHERE ANY ID IS MISSING there is ONE MORE line under them, counting the missing ones as this run's, carried over, or on a row with no date at all, and that one does NOT end that way, it ends `which is UNKNOWN and not carried over`. Expect `Id` to supply most of them, because `Id` is the first name looked for and `Element ID` is the label this tool chooses
218. Look for, if any `CLASH    OLD` line appears: it reads the test name and `Navisworks has this test marked Old.` and nothing after that. It used to carry a sentence about what Old means, which is UNKNOWN
219. Look for, only if you tick compacting on the Clash step: the compacted line carries both numbers, the Resolved count before and the count read again after, and the number reported as removed is the difference between them

## Two probes, F50 and F52. BOTH ALREADY RUN on 2026-09-19

Both were run on this machine on 2026-09-19 and both answers are in
`docs\history\scan.md`, 5c and 5d, with the assembly version and the date. Sections 5a and
5b still hold the questions, because the reasoning in them is why the answers matter.

Nothing below is outstanding. The four steps are kept so the answers can be checked rather
than trusted, and because the probes are the right thing to rerun when Navisworks is
upgraded. Each takes about ten seconds, neither needs the add-in built and neither opens a
model.

220. Open PowerShell in the repo folder and run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-model-remove.ps1
```

221. Look for, under `THE QUESTION: anything anywhere that takes a Model or an index and removes it`: two members on `Autodesk.Navisworks.Api.Document`, `public Void RemoveFile(Int32 index)` and `public Boolean TryRemoveFile(Int32 index)`. So a model CAN be taken out without a clear, and the member is on `Document` and not on `DocumentModels`, which is why searching the collection for it found nothing. `DocumentModels` carries only `InternalRemove` and `InternalRemoveAt`. If this list is now EMPTY, the install has changed and `scan.md` 5c is wrong
222. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-viewpoints.ps1
```

223. Look for: a block headed `The viewpoint collection, every member` listing `Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints`. It must NOT say `UNKNOWN: no type named ...DocumentSavedViewpoints`, and on 2026-09-19 it did not, so the name the code uses is right
224. Look for, in the same output: `public FolderItem RootItem`, `public Void AddCopy(GroupItem parent, SavedItem item)` and `public Void EditDisplayName(SavedItem item, String newDisplayName)`. All three were there, so a folder is made, a viewpoint goes in it and a name can be set. `SavedViewpoint` reads `IDisposable: True` and has a `SavedViewpoint(Viewpoint viewpoint)` constructor. The sets collection printed beside it carries the same members with the same signatures, which is the comparison the probe exists to make
225. DONE in the viewpoints round on 2026-09-19, the way steps 374 and 375 read. This said `SavedViewpoints.CanBuild` is still false and that whether a saved viewpoint records hiding could not be read off a DLL. It was measured on a real run instead, scan.md 5j to 5m, and the answer is that NEITHER .NET route records both: `new SavedViewpoint(Viewpoint)` keeps the camera alone and `CaptureRuntimeOverrides` keeps the hidden state and no camera. The COM view with `ApplyHideAttribs` records both and is what the tool writes through. `CanBuild` has been a hard true since. The flags this step names are worthless as a read back, 5o: both read TRUE on a viewpoint that recorded nothing.

## Proof F51, the NWD carries May be re-saved and its properties reach ACC

Every NWD this tool has published so far shows a processing error beside it in ACC and
in Forma. Autodesk say an NWD published without May be re-saved cannot be translated by
that viewer, and they have a second article about an NWD reaching ACC with no properties
and every object showing as solid. F51 sets three things on the publish that were never
set: `AllowResave` true, `EmbedDatabaseProperties` true and `PreventObjectPropertyExport`
false. All three were already on the list read off the DLL on 2026-08-29.

This proof needs one NWD and an upload. It is the only thing in this file that needs a
browser as well as Navisworks.

226. Run any one building the ordinary way, so a fresh NWD is published
227. Look for: one line in the log reading `NWD      publish properties set:` and then seven names, ending `AllowResave=true, EmbedDatabaseProperties=true, PreventObjectPropertyExport=false`. If that line is missing, the build is older than F51 and nothing below proves anything
228. Open the published NWD in Navisworks on your own machine, then File, then look at the file properties
229. Look for: the NWD opens and the title, the publisher, the subject and the author read as the log line says they were set
230. Upload that NWD to ACC, into whatever folder the project uses
231. Look for: no processing error and no warning triangle beside it once the translation finishes. That is the whole point of `AllowResave`. A warning still there means the flag did not take and the answer is a screenshot of the warning and the log line from step 227
232. Open the NWD in the ACC viewer and click one object, any object
233. Look for: a properties panel with real Revit properties in it, not one row reading Solid. Object properties missing means `EmbedDatabaseProperties` did not carry them and `PreventObjectPropertyExport` is the next thing to look at
234. Q32 is whether the NWF needs to be in ACC at all. Nothing appears beside an NWF because ACC does not translate one: an NWF holds no geometry, only pointers to the NWCs, so there is no viewable file to make. Answer it in `steps/02_questions.md` when you have seen the NWD work

## Proof F52, a viewpoint per discipline

Two halves. The first is true today. The second no longer waits on a probe, because the
probe was run on 2026-09-19 and the API is measured in `docs\history\scan.md` 5d. It waits
on the WRITING half of F52 being built against what was measured, which is a decision and
not a measurement, so `SavedViewpoints.CanBuild` is still false. Do the first on the next
ordinary run and the second once that half exists and the add-in has been rebuilt.
SINCE THE VIEWPOINTS ROUND ON 2026-09-19 the second half exists, `SavedViewpoints.CanBuild` is
true and the `not attempted` line of step 237 is gone. Steps 374 and 375 carry what was done
and the viewpoints round entry in `steps\log.md` carries the run.

235. Look for, and THIS STEP WAS REWRITTEN ON 2026-09-20 because F85 changed what a viewpoint is: a `VIEWS` block per group reading `clashes looked at : N` then `viewpoints planned: N`, with the number AFTER the words, then one line per reason a clash was left out. THE PATHS ARE DELIBERATELY NOT LISTED, because a real group plans hundreds. There is ONE VIEWPOINT PER CLASH now, not one per discipline, so the old arithmetic on this step, which said a group of AR, ME and EL plans five, is gone with the shape it described
236. Look for: a group with only one discipline plans NONE and says so, because every clash test is one discipline against another and a group that cannot clash has no clash to point a viewpoint at. That is the opposite of what this step said before F85, when a viewpoint was per discipline. The block still gets written, which is the part that has not changed: a group with nothing to plan says so rather than going quiet
237. GONE. It said to look for a line reading `VIEWS    not attempted.` while `SavedViewpoints.CanBuild` was false. That flag has been a hard true since the viewpoints round on 2026-09-19 and the string is in no source file, so this was a check that could never fire. Steps 383 to 385 and 391 are what prove the viewpoints now
238. DONE in the viewpoints round on 2026-09-19 and proved on every run since. This said to wait for the writing half of F52 to be built, and it was. Run one building again
239. Look for: a `VIEWS` block in the shape of the SETS block, a `VIEW` line per viewpoint, CAPPED at `RunLog.KeptOfARepeat` with `VIEW     and N more created or already there, counted and not listed` under them, because a real group writes hundreds, then `views created     :` and `put into the document:`
240. Open the NWF in Navisworks and open the Saved Viewpoints panel
241. Look for: THREE folder layers, not one per discipline, which is what this step said before F85. Layer 1 is the priority off the clash matrix, `A`, `B`, `C` or the no priority folder, and it is DROPPED ENTIRELY when no priority file was picked. Layer 2 is the two disciplines SORTED, so `AR vs ST` holds the clashes of both directions. Layer 3 is `Over 150mm` and appears only under a pair carrying Mechanical or Electrical. Steps 383 to 385 and 391 walk this shape on a real file
242. Run the SAME building again without changing anything
243. Look for, F28's rule carried to viewpoints: every line reads `already there, left alone, not made again` and `views created     : 0`. A second copy of any viewpoint is a fault

## Proof F53, the 150 mm rule and the sub groups

This one is proved in Core here and in the tree on your machine. The Core half needs
nothing from you. The rest WAS waiting on the writing half of F52 and no longer is, because the sub group IS a
viewpoint and the writer has been building them since the viewpoints round on 2026-09-19, scan.md 5d and 5m.

244. Look for, on any run since F53: the `VIEWS` planned line now carries a sub group path per Mechanical and per Electrical group, NO PATHS AT ALL. The planned line writes counts and never paths, which step 235 above says in so many words. The paths are in the `VIEWS BUILT` block instead, five per group and then `and N more created or already there, counted and not listed`, and the shape there is a PAIR folder and then the size folder, such as `A/ME vs ST/Over 150mm`, and never `ME/Over 150mm/ME over 150mm`, which was the shape before F85 made a viewpoint per clash. A group with no ME or EL in it has none, and that is right
245. Look for: the number in the folder name is the threshold in use. If you change the threshold and it still reads `Over 150mm`, that is a fault and the folder and the rule have drifted apart
246. DONE since the viewpoints round on 2026-09-19. It said to wait for the writing half of F52. Run one Mechanical building
247. GONE, and this is worth knowing rather than just deleting. It said to look for a `SIZE` block per group with three numbers. `Federator.Core.Views.SizeTally` still writes that block and NOTHING IN SRC CALLS IT, so no SIZE block is written on any run. What replaced it is one line inside the `VIEWS` block: `size could not be read : N, every one of them is in its pair folder and none was dropped`. The dead class is raised under the CLAUDE.md rule about a public member nothing in src calls
248. Look for, and this was rewritten AGAIN by the worksets round because the number changed: `size could not be read : N`. It is EXPECTED TO BE ZERO now, on every group. It used to be large, 59 and 36 and 8 on the run before, because a fitting was thought to carry no size property. 5s found that every one of them did, written as words, `53 mm�` for a conduit and `600 mmx100 mm` per connector for a cable tray fitting, and `ItemSizes` was dropping every string. `Federator.Core.Views.SizeText` reads them now. A number ABOVE zero here is a size shape `SizeText` has not been taught and is worth sending.
249. GONE with the number it read. It said to read the names under that line and judge whether they were all fittings. No names are written under it, `ClashViewpointPlan.Lines` writes the count alone, and the count is zero on every group since 5s. Where one ever comes back non zero, the `.tsv` rows are where the names are.
250. Open the NWF and press any viewpoint under a folder named `Over 150mm`, which sits under a PAIR folder such as `A/ME vs ST/Over 150mm`, and never a folder called `ME over 150mm`, which was the shape before F85 and which step 244 above already corrects.
251. Look for: large pipes, ducts and trays showing, small ones hidden, and the fittings still there. A fitting missing means the include on unknown rule is not working and that `size could not be read` line is the evidence
252. Look for, on a document measured in FEET rather than millimetres: the same items in and out as the same building measured in millimetres. The conversion is what makes that true, and a difference means the raw number is being compared somewhere

## Proof F54 and F72, penetrations become Reviewed

Q33 is ANSWERED since 2026-09-19 and F72 supplies the list, so this section has two
halves now. The first is the run with the box OFF, which is the default and which has to
change nothing at all. The second is the run with it on.

253. Run any building the ordinary way, with the box on the Clash step left OFF
254. Look for: NO `STATUS` line and NO `PENETRATION` block anywhere in the log. With the box off nothing supplies a list, so either of those on an ordinary run means something is supplying one and that is a fault. A REBUILT group writes a `RESULTS` line, which is F50 counting the clash results it carried across the clear and is nothing to do with this. The two used to share the STATUS prefix and F56 separated them, because one prefix reading as two different things is how a log stops being trusted
255. Look for: the clash counts per test in the workbook still read New, Active, Reviewed, Approved and Resolved as they always did, and no clash has moved
256. Look for, in the RUN SETTINGS block at the top: `penetrations     : no, every clash keeps the status it has`
257. Look for: NO `penetrations   :` line in the RESULT block. A run that never asked for it has nothing to say, and a line reading zero on every run teaches people to skip it
258. Open `docs\workflow.md` and read the section What a status means
259. Look for: it says a clash carries five and a test carries four, that Old is a test word and never a clash word, and that this tool sets Reviewed and nothing else

### Now with the box on

260. Go to the Clash step
261. Look for, F72: a tick box reading `Mark penetrations as Reviewed`, unticked, with one grey line under it reading `Services 150mm and under through walls, floors, roofs, foundations. New and Active`, with foundations since Q63 and no trailing `only`, which the twelve word limit had to give up to name the fourth solid. The 150 is read off the setting, so if you ever change the threshold this line changes with it
262. Look for, F72: it is NOT inside `Things that destroy data`. Nothing is destroyed, only New and Active ever move and a person moves one back in one click
263. Tick it, and run ONE building that has mechanical or electrical services in it
264. Look for, F72: a `PENETRATION` block per group, after the `CLASH` block and after the `EMPTY SETS` block where any set found nothing, which PART 3 put between them
265. Look for, F72: one line per clash moved, starting `REVIEWED `, then the clash name, then the test it is in, then the service category, the size in millimetres, and the solid category. For example a `Pipes 100mm through Walls`. If that line names a category you would not call a service, the list is a setting and it is wrong for this project
266. Look for, F72: after the moved lines, `clashes looked at :` and `moved to Reviewed :`, then SIX INDENTED LINES, one per reason a clash was left alone, including the reasons at zero. The six are the service is over the size, no size could be read, a person had already set it, both sides a service, both sides a solid, and not a service against a solid. `moved to Reviewed` is NOT one of the six: the reason loop skips it on purpose, because it is the count line above them. This step said seven and named moved as one of them, so it could never pass SINCE Q71 TWO MORE LINES sit under the `no size could be read` reason WHEN IT IS ABOVE ZERO, naming what the services are by category and saying each is a row in the machine readable log. On his folders that reason is now zero, so neither line appears.
267. Look for, F72: the line `the size in use   : 150mm or under is a penetration, over it stays as it was`
268. Look for, F72: the number beside `no size could be read off the service, left alone` and READ IT. IT IS ZERO ON EVERY GROUP SINCE THE WORKSETS ROUND, where it read 29, 30, 5 and 1 the run before. That is the count of services this tool could not measure, and every one of them was LEFT AT NEW rather than moved. It goes the opposite way from F53, which INCLUDES an item it could not measure, and both are right: the safe mistake in a viewpoint is showing something unnecessary and the safe mistake here is leaving a clash for a person. If that number is large, the size is on a property this tool is not looking for, and the answer is the property name so it can be added to the setting
269. Look for, F72: TWO lines in the RESULT block beginning `penetrations   :`, not one. The first reads `penetrations   : N clashes moved to Reviewed` and N is the sum of every group's `moved to Reviewed`. The second, straight under it, reads `penetrations   : N clash(es) moved to Reviewed, of M looked at across K groups`, with one indented row under it per group that moved something. TWO LINES SHARING A PREFIX AND SAYING DIFFERENT THINGS is worth reporting on its own

269a. Look for, F72b, where the by design box was on: the same pair again under the `by design      :` label, a total line and then a moved-of-looked-at line with per group rows

269b. Look for, in the RESULT block on EVERY run whatever the boxes say: one line `clashes found  : N across K groups`, with `, J of which found none` on the end where any group found nothing, and then one indented row per group reading the building, the count, and `from N test(s) that found something`. UNLIKE THE PENETRATION AND PRIORITY LINES THIS ONE IS NEVER CONDITIONAL, so a run with no line here at all is a fault. Where no group reported a count it reads `clashes found  : UNKNOWN, no group reported a clash count`, which is not the same as zero

269c. Look for, Q73 and F98: the `WORKBOOK CHECK` block's block count now AGREES with the run tail's `WORKBOOK N test blocks`, and its CHECK line reads `1 sheet, "<name>", 1830 test blocks of which N found something, M clash rows.` It used to find a block by its `Clash Name` heading row, which an empty block has none of, so on 1A0415 it read 0 blocks against 1830 while the workbook carried all 1830 of them. It finds a block by the test name in column A now, which both shapes carry. If the two numbers still differ, send both. Where every block on a sheet found nothing, look for a line beginning `EVERY BLOCK ON THIS SHEET FOUND NOTHING` and look for the sentence about every column, fill, border, height and width matching to be ABSENT, because there was no clash table to compare and saying it matched would be the check passing on work it never did

269c-b. Look for, F98: the `BLOCKS` line no longer says MUST. Where the counts agree it reads `BLOCKS   1830 in the workbook, one for every test in the file`. Where they do not it names how many tests have no block and says plainly that it does not fail the group, and a `BLOCKS` line in the RESULT block says how many groups of how many counted came up short. It is a finding for you and it fails nothing, which is Q79

269d. Look for, Q74, where the rebuild box is on and the NWF holds a set the picked file no longer names: a block whose first line reads `N set(s) in this NWF are not named by the picked file:` then three indented counts, `N nothing points at, so they are removed`, `N are the working half of a pair, so the unused half goes and this one takes its name`, `N are pointed at with no twin, so nothing is done about them`, then one line per set. Then one `SET      REMOVED <path>, which nothing pointed at` per removal and one `SET      RENAMED "<old>" to "<new>" and removed the unused set that held that name. Its N test side(s) keep working and now ask what the file asks` per pair. The `SETS     <building> put into the document:` line ends with `, N of M set(s) the file no longer names brought up to date`. A set that IS pointed at and has no twin is NOT touched and says `NOTHING IS DONE, because removing it would leave those sides resolving to nothing`. THE LEFTOVER COUNTS ARE IN THE LOG ONLY and never in the window's sets pane
270. Look for, F72: an `NWF      attempt` line AFTER the clash step, because writing a status is a write and the NWF is saved again on it. SINCE F85 THE VIEWS STEP CAUSES THAT SECOND SAVE TOO, so the line appears on a weekly run with the penetration box off, and its absence is still the fault to report but its presence no longer proves the penetration pass ran. Whether it is the first or the second such line depends on the path this group took, so count them rather than looking for a second: a First run or a Rebuilt group writes one before the clash step as well, and a Weekly run does not, because the opened branch logs `NWF      reused` instead. On a Weekly run the line after the clash step is the ONLY one, and its absence is the fault to report
271. Open the workbook for that building
272. Look for, F72: the Reviewed count on the test header rows has gone UP by the number the block said, and the New or Active count has gone down by the same. That is the count reaching the workbook, in the client's own column, because the status is applied before the harvest reads it. If the workbook shows the OLD status, the status is being applied after the harvest instead of before it, which is the one thing this feature must not do
273. In Navisworks open Clash Detective on that building and pick one clash the block named
274. Look for, F72: it reads Reviewed in the panel, and the two items are a small service and a wall, a floor, a roof or a structural foundation, which Q63 added to the solid list on 2026-09-20
275. Run the SAME building again with the box still on
276. Look for, F72: the second run moves FEWER, probably zero, because the ones it moved last time are now at Reviewed and Reviewed is not a status this tool moves off. They are counted under `a person had already set it, left alone`, which is the same line a clash somebody set by hand would appear on
277. Find a clash you set to Approved by hand, if there is one, and run again
278. Look for, F72: it is still Approved. Only New and Active ever move, and that is a Core rule with its own tests rather than a condition in a loop

## The log round, F59 to F64. One folder, watched and then read

THE ONE THAT ANSWERS THE ROUND. Everything below this is detail, and this section on its
own is what Bader asked for on 2026-09-18: show what happened inside Navisworks, show
what is running while it runs, and show where the time went. Do this one first, with the
whole folder ticked, because a single building cannot show group N of M and cannot fill
the run timing block.

279. Run the SAME building once more with the box OFF, and note the `clash step took` seconds on both runs
280. Look for, F72: what the penetration pass COSTS, which is the one number nothing here could measure. With the box on, every clash has a category read off both sides and a size read off the service, walking up to four levels for each, so the cost is per CLASH and not per test. It is UNKNOWN until this comparison is made. If the difference is small, leave it on. If it is large, that is worth saying, because the pass reads the same properties the harvest reads a moment later and the two could be made to share one read
281. Go to the Source step, pick the C06 NWC folder and press Scan
282. Go to the Outputs step and pick the NWF folder and the NWD folder
283. Go to the Clash step and pick the clash XML
284. Go to the Grouping step and make sure EVERY group is ticked
285. Press Run, then OK, and then WATCH the window rather than leaving it
286. Look for, F62: the line just above the log box reads the group, the building, the step, the seconds on that step and the seconds on the run, and it changes as the run works. That line is the whole of what is running while it runs
287. Look for, F62: the log box below it scrolls itself to the newest line, and the line above never moves out of sight
288. When it finishes, open the log
289. Look for, F60: a block headed `TIMING` after every group, and one headed `TIMING, THE WHOLE RUN` before `RESULT`. That is where the time went, per group and then by step across every group
290. Look for, F60: the last line of the run block says whether the run fitted in forty five minutes. That is criterion 2 answered by the log itself, and it is the single most important line in the file
291. Look for, F61: `CENSUS` lines either side of every step, carrying models, sets, tests, results and views. That is what happened inside Navisworks, counted rather than assumed
292. Look for, F63: a `GAP` block at the end of every group, saying what the run measured and the report does not show
293. Send the log and the `.tsv` beside it. Everything after this section is the detail behind these five things, and it is worth doing, but if you only do one section do this one

## The log round in detail, F59 to F64

Nothing here needs a new folder or a new file. It is all read off the log of the run
above, or of any ordinary run.

294. Run any one building the ordinary way, with the clash XML picked
295. Open the log for that run
296. Look for, F59: a pair of lines per step, reading `STEP     ` then the step name then `started`, and later the same name then `finished`, the seconds and a few words for what it changed. The step names are `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `IMAGES`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM`. There are FIFTEEN since F85 added `VIEWS`, which sits between `IMAGES` and `WORKBOOK`
297. Look for, F59: `STEP     TESTS RUN` appears ONCE as a started line and once as a finished line for the whole group, then one line reading `entered again. Every further visit in this group is counted, not written out`. A pair of lines per test would be 3660 lines for 1830 tests, which is the fault that once left a 17.8 MB log
298. Look for, F59: one line per repeated step at the end of the group, before the `GROUP    finished` line, reading the step name then the visits then the total seconds, like `STEP     TESTS RUN     1830 visits, 742.113s in total`
299. Look for, F59: `STEP       IMAGES` is indented two spaces further than every other step line. It is the only one that runs INSIDE another step, because a picture is written while the harvest is walking the results. `HARVEST` is NOT indented, because it opens and closes on its own
300. Look for, F59: NO line anywhere reads `NEVER CLOSED`. That line means a step was opened and nothing closed it, which would make every total after it wrong. If you see one, send the whole log
301. Look for, F59: one clock per piece of work. Find the FIRST `CLASH    ` line that reads `clashes` or `passed`, which carries a test name and its seconds, and find the `STEP` line for `TESTS RUN` that reads `finished`. Those two are the same reading, because the step IS the measurement now and the Stopwatch that used to time the same thing separately is gone. The `CLASH` line rounds to ONE decimal and the `STEP` line carries three, so 12.3 against 12.345 is them agreeing and not disagreeing. The step name is padded to the width of the longest one, so read the words and do not count the spaces
302. Look for, F60: a block headed `TIMING` and the building, straight after the `GROUP    finished` line of every group. Every step on its own row with its seconds and its share, slowest first, then a row reading `outside every step`, then `group total`
303. Look for, F60: the shares in that block add up to a hundred. They are meant to, because whatever the steps do not account for is a row of its own rather than a number left off the page. A column adding to less than a hundred means a row is missing and is worth sending
304. Look for, F60: the `group total` on that block is the SAME number as the seconds on the `GROUP    finished` line above it
305. Look for, F60: a line reading `inside another step, so these seconds are already counted above` at the bottom of the block, with `IMAGES` under it. Those seconds are inside `HARVEST`, so they are deliberately left out of the share column
306. Look for, F60: a block headed `TIMING, THE WHOLE RUN` just before the `RESULT` block at the end of the log. It reads the groups slowest first, then the run total, then the SAME steps added across every group, which is what answers which step costs the run rather than which building
307. Look for, F60: the last line of that block reads `The run took` and then the time in minutes and in seconds, and then either `inside the 45 minutes 0 seconds a run has to finish in` or `OVER` it. That sentence is criterion 2 answered by the log itself. Send it either way
308. Look for, F21 which closes with F60: one line per test reading `ROWS     ` then the test name then `N rows for the workbook, N clashes in the document`. Where the two agree it says `they agree`. Where there are FEWER rows than clashes it names the result groups as the reason, which is right, because the workbook and the Clash Detective panel both show one row per group
309. Look for, F21: NO line anywhere reads `THERE ARE MORE ROWS THAN CLASHES`. Nothing in this tool produces more rows than clashes, so that line means something is wrong and the whole log is worth sending
310. This is criterion 3 without opening Excel. Pick any test whose `ROWS` line says they agree, open the workbook at that test's block and count the rows, then open Clash Detective on the same test and read the panel count. All three should be the one number
311. Look for, F61: a pair of lines around each step reading `CENSUS   before ` then the step name, and `CENSUS   after  ` then the same name, each carrying five counts: `models`, `sets`, `tests`, `results` and `views`
312. Look for, F61: the `views` count is a NUMBER and not `UNKNOWN`. It reads `0` before `APPEND` and whatever the NWC files brought after it, which on the run of 2026-09-19 was 20. It used to say this count reads 0 on every line, and that was wrong: an NWC exported from Revit carries that model's saved viewpoints and appending it brings them in. `SavedViewpoints.Count` returns minus one only where it could NOT count, and minus one prints as `UNKNOWN`. An `UNKNOWN` here means the viewpoint collection is not the shape it was MEASURED to have on 2026-09-19, `docs\history\scan.md` 5d, and the whole log is worth sending
313. Look for, F73: NO line anywhere begins `CENSUS CHANGED`. That line means a count moved during a step that may not move it, which is a real fault, and the group carrying it will read FAILED rather than DONE. If you see one, send the whole log. A line beginning `CENSUS NOTED` is NOT that line and is not a fault: it is the saved viewpoints moving during `APPEND`, it says why, and it ends `can still be DONE`. On the run of 2026-09-19 the refused line was written in all seven groups and put every one of them out of DONE while 28 files had been written correctly
314. Look for, F61: the counts move where they should. `sets` goes up across `SETS`, `tests` goes up across `TESTS CREATE`, `results` goes up across `TESTS RUN`, and `models` goes up across `APPEND`. None of them moves across `NWD`, `WORKBOOK`, `HTML`, `XML` or `CONFIRM`. Expect `TESTS CREATE` and `TESTS RUN` to move by ONE test's worth and not by the whole group, because those two are entered once per test and the census is taken around the first visit only. That is step 315 and it is not a fault
315. Look for, F61: `CENSUS   before TESTS RUN` appears ONCE per group and not once per test. The census is taken around the first visit of a step and no more, because counting the whole document 1830 times would be the log making the run slower
316. Look for, F61: one line per group reading `CENSUS   cost ` then the seconds then the number of counts. This is the measurement of what the census itself costs, and it is the number that decides whether it stays wide
317. Look for, F61: that cost line says `inside the 1.000s a group is allowed`. If it says `over`, the census narrows from the next group on and the next group says `CENSUS   narrowed` at its top. Both are working as intended, but send the number either way, because it is the first real measurement of what the census costs on a real model
318. Look for, F62, while the run is WORKING and not after it: the line just above the log box reads `Group 3 of 14`, then the building, then the step, then how long that step has been going and how long the run has, like `Group 3 of 14  1B06PH  TESTS RUN  12s on this step  4m 02s on the run`
319. Look for, F62: that line sits on its own row above the log box. It has to stay put while the log box scrolls itself. If it is beside the buttons and cut off at the window edge, the build is older than F62
320. Look for, F62: the log box keeps scrolling to the newest line by itself the whole time, and the line above it never moves out of sight
321. Look for, F62: the line changes as the step changes. Watch one group through and you should see `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `VIEWS`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM` go past. `IMAGES` is the one step that never appears on this line, because it is opened inside the picture writer and nothing there was given the line. It is in the log and in the timing block like every other step
322. Look for, F62: inside the clash run the seconds on the run keep counting up while the tests go by. It is rendered at most once a second on purpose, so it should look like a clock and not like a flicker
323. Look for, F62, on the SECOND group onward: if a step runs over twice as long as the same step took on the group before, the line says `SLOWER, the group before took` and the time. On the first group it never says it, because there is nothing to compare against
324. Look for, F62: while the NWD is publishing the line stops counting and sits at the seconds it last showed. That is expected and it is written down: publishing is one Navisworks call with no loop inside it, so there is nothing to tick from without starting a thread, and nothing here starts one. If the line went blank instead, that is a fault
325. Look for, F63: a block headed `GAP` and the building at the END of every group, after every output has been written. That is the point: it answers what the outputs do NOT carry
326. Look for, F63: one line per property the run MEASURED and no output carries, each with how many item cells carried it out of how many there were, and where it would belong. The six it can name are `Family`, `Type Name`, `Material`, `Source File`, `Discipline` and `Id From`. FEWER THAN SIX IS NOT A FAULT: a property no item carried at all is not a gap and is left out on purpose, because reporting it would say the run is holding back something it never read. Which ones appear is itself the measurement
327. Look for, F63: the first line of the block says `nothing acts on it`, or, where a group held nothing back at all, the block is one line reading `nothing measured this group reaches no output`. Either way a gap is a question and never a fault, and no group should read anything but DONE because of one
328. Read the counts on those six lines. They are the first real measurement of how much of each property a real model actually carries. `Material` is expected to be low and `Family` high, and if `Material` is at zero it may be worth deleting rather than keeping, which is Q37
329. Look for, F63: one line at the end of the run reading `GAPS     ` then a number then the names. It counts by NAME and not by line, so a run of fourteen groups says six and not eighty four
330. The six are now Q35 to Q40 in `steps\02_questions.md`, one per property, because one answer for five different properties is a decision nobody can make. Answer them with the counts from step 328 in front of you
331. Look for, F64: in `%LOCALAPPDATA%\ParsonsNwcFederator\logs`, beside every `run-*.log`, a `run-*.tsv` with the SAME name and a different extension
332. Look for, F64: near the top of the text log, one line reading `ROWS     the machine readable log is` and the path. If it says `no machine readable log` instead, the text log is unaffected and the reason is on that line, so send it
333. Open the `.tsv` in Excel. It should open straight into columns with no import dialog and no question about separators
334. Look for, F64: a header row reading time, seconds, group, step, event, name, number, text, and then one row per event under it
335. Look for, F64: every row has eight columns. Sort by the `event` column and read the kinds. THERE ARE TWENTY THREE. The worksets round added `service with no readable size`, which is Q71 and which is ABSENT from both proving runs because every service is now measured, and PART 3 of the drift round added `set finding nothing`, one row per set that found nothing, written beside the EMPTY SETS block. Before those two there were twenty one, not the fourteen this step used to name, and seven were added after it was written. From the log itself: `step started`, `step finished`, `step threw`, `step total`, `timing`, `timing nested`, `census before`, `census after`, `appended`, `append failed`, `written`, `group finished`. From the clash step, F76 and F81: `test created`, `test passed`, `test found clashes`, `tolerance set on a saved test`, `rows for the workbook`. From the blocks: `gap`, `set across the run` (F82), `set finding nothing` (PART 3 of the drift round), and `model placement` and `model export` (the alignment round). NOT EVERY KIND APPEARS ON EVERY RUN: a weekly run appends nothing so `appended` is absent, and nothing throwing means no `step threw`. The proving run of 2026-09-20 carried sixteen of the twenty one. A kind not on that list of twenty three is something new and the whole file is worth sending
336. Filter the `event` column to `step finished` and sort the `number` column biggest first. The top row is the slowest step of those entered ONCE in their group, in seconds. It is NOT the slowest step of the run, and that is worth knowing: a `step finished` row is written only on a step's first visit, so `TESTS RUN` and `TESTS CREATE`, which are entered once per test, each contribute one row for their first visit and reach the file again only as a single `step total` row per group. To find the slowest step of the run, read the `step total` rows beside these, or read the TIMING block in the text log, which adds both kinds up already
337. Look for, F64: nothing in the text log got worse. It still has its blocks, its indenting and its sentences, and it is still the one to read first and the one to send

## The two walls are live, D7

There are three files here that `sh` reads: the two hooks Claude Code runs before a tool
call, and the pre-commit hook that runs the tests. One refuses an edit under `samples`,
`steps\logs` or `bundle`, one refuses a commit or a push while main is checked out, and
the third refuses a commit while any test fails.

None of them survives a carriage return. `sh` reads one as part of the word, so a CRLF
copy dies on its first `case` line before it reaches a rule, and because `sh` exits 2 on
a syntax error, and 2 is the code that refuses, the two Claude Code hooks then refuse
EVERY call rather than the protected ones, while the pre-commit stumbles past `set -e`
and lets every commit through untested. Your machine is where that bites, because Git for
Windows sets `core.autocrlf` to true when it installs and the repo had nothing telling it
otherwise. F47a added `.gitattributes`, which pins these three to LF on every checkout.

This is a one time check per clone. Do it once and the rest of this file never needs it.

338. In the VS Code terminal, in the repo folder, run:

```
git ls-files --eol .claude/hooks .githooks
```

339. Look for: three lines, each reading `i/lf` and `w/lf` and `attr/text eol=lf`. A `w/crlf` on any of them means this checkout still holds the old copy, so run `git add --renormalize . ; git checkout -- .` and read it again
340. Switch the test wall on, which git needs told once per clone:

```
git config core.hooksPath .githooks
```

341. Look for: `git config core.hooksPath` answers `.githooks`
342. Make a branch, change one word in `steps\log.md`, and commit it from the VS Code terminal rather than from GitHub Desktop, so you see what the hook prints
343. Look for: the commit pauses and prints FOUR lines in this order, `pre-commit: checking the add-in for a local declared twice`, `pre-commit: checking the add-in for a type with no import`, `pre-commit: running the full test set`, then `pre-commit: tests passed`, and only then commits. That is the test wall, and the two checks in front of it are F58's and F66's. A commit refused by either of those prints `pre-commit: REFUSED. See the line above.` and never reaches the test set at all. Throw the branch away afterwards
344. Open Claude Code in this folder and ask it to write one word into any file under `samples`
345. Look for: it comes back refused, with the line `Refused. ... is under samples, steps/logs or bundle, which are never edited.` That is the paths wall, and the branch wall is the same hook file beside it, proved the same way by asking it to commit while main is checked out

## Delete the old branches, D6

Every branch except main is merged into main. This is yours, one command from the repo folder in the VS Code terminal. A session working in a container cannot do it: `git push origin --delete` comes back HTTP 403 from the proxy in front of it, and no GitHub tool in it deletes a branch.

The list below was read on 2026-09-19 with the command in step 346, after the penetration round, and `git ls-remote --heads origin` gave 67 names. Two more go up with this round's own closing work, `fix-F72` and `round-close-penetrations`, and neither was on the remote when the list was read, which makes 69 lines and 68 to delete by the time you run it. Read the live list again yourself before you delete, because a branch may have come or gone since. Do not build the list from `git branch -r`. That prints remote-tracking refs your clone remembers, and a branch deleted by someone else is still in it until you prune, which is how a name that does not exist on the remote reached this file once already. A clone showed it again on 2026-09-18, still holding `origin/claude/parsons-nwc-analysis-rlzgdr` after the remote had lost it. `git ls-remote` asks the remote and remembers nothing.

346. Read the live list:

```
git ls-remote --heads origin
```

347. Look for: one line per branch, the name after `refs/heads/`. It was 77 on 2026-09-21, so 76 to delete, READ OFF THE LIVE REMOTE just now and not off this clone. It was 66 after the build round and 69 after the penetration round, and the seven added since are `audit-first-run`, `round-wiring`, `round-first-run`, `round-viewpoints`, `round-dimming`, `round-alignment` and `round-worksets`. The delete command below names 68 and does NOT name those SEVEN, nor `round-drift`, which is an eighth once it is pushed, so read the live list first, which is what step 346 is for. This step said six while listing seven names
348. Delete every one of them except main:

```
git push origin --delete analysis-pass fix-F16 fix-F27 fix-F28 fix-F29 fix-F30 fix-F31 fix-F32 fix-F33 fix-F34 fix-F35 fix-F36 fix-F37 fix-F38 fix-F39 fix-F40 fix-F41 fix-F42 fix-F43 fix-F44 fix-F45 fix-F46 fix-F47a fix-F47b fix-F47c fix-F50 fix-F51 fix-F52 fix-F53 fix-F54 fix-F55 fix-F56 fix-F58 fix-F59 fix-F60 fix-F61 fix-F62 fix-F63 fix-F64 fix-F65 fix-F66 fix-F67 fix-F68 fix-F69 fix-F70 fix-F71 fix-F72 fix-f1-f2-f4-small fix-f10-gate-outputs fix-f11-dead-code fix-f17-picture-order fix-f20-tests-on-push fix-f22-two-workflows fix-f24-rebuild-changed-nwf fix-f26-units-meters fix-f5-sets-built fix-f6-open-file-folder fix-f7-open-file-result fix-f8-run-saved-tests fix-f9-changed-skip-units master round-close round-close-2 round-close-3 round-close-4 round-close-build round-close-log round-close-penetrations
```

349. Look for: one `- [deleted]` line per branch and no error
350. Run `git ls-remote --heads origin` again and look for: one line, `refs/heads/main`. If a branch you did not expect is there, it was pushed after the list above was read, so read what it holds before deleting it
351. Run `git fetch --prune` so your own clone forgets the branches that are gone. Without it `git branch -r` keeps printing them
352. If the command refuses a branch, open github.com, the repo, Branches, and press the bin icon beside every branch that is not main

## The first run round, F73 to F88

**READ THIS FIRST. This round was worked in a LINUX CONTAINER with no Navisworks on it.
Every rule below is in Federator.Core, every one has tests, and the whole Core set passes.
NOTHING IN THE ADD-IN WAS COMPILED, because the add-in cannot be compiled without
Navisworks and this session had none. Five of the eighteen fixes turn on a measurement
only Navisworks can give and those five are steps 353 to 357. The add-in wiring each fix
still needs is steps 358 onward, and every one of them is written out so it can be done
without reading the brief again.**

### The five measurements, before any of the wiring

353. Open a Developer Command Prompt and dump the members of `Autodesk.Navisworks.Api.dll`
     looking for anything that says a document has finished LOADING. Every member on
     `Document`, `Document.Models`, `DocumentParts.DocumentModels` and `Application` whose
     name holds Load, Ready, Busy, Progress, State, Pending or Complete, and every event on
     each. Paste the list into `docs\history\scan.md` under 5e, where the question is
     already written, and say in one line whether any of them answers "the models are all
     in now". If one does, `ModelLoadWait` becomes the fallback and the add-in reads the
     member instead. If none does, the poll stands and that sentence is why
     DONE on 2026-09-19 in the wiring round by tools\probes\probe-document-ready.ps1, written into scan.md 5e. SceneLoaded exists on DocumentModels, its timing does not read off the DLL, so the poll stands and step 365 counts the event beside it.
354. Dump the property members the same way, for F86, into 5f: `ModelItem
     .PropertyCategories`, `PropertyCategory.DisplayName`, `.Name`, `.Properties`,
     `DataProperty.DisplayName`, `.Name`, `.Value`, every reader on `VariantData`, and
     whether a `Search` can walk a whole model in one pass rather than per item. One line
     saying how a value becomes the text a CSV cell holds, and one saying whether the walk
     is per item or per search
     DONE on 2026-09-19 in the wiring round by tools\probes\probe-properties.ps1, written into scan.md 5f.
355. DONE on 2026-09-20, in the alignment round's PART 3 predecessor. `docs\history\scan.md` carries a second 5g, "5g, measured", answering YES exactly: a negated condition builds, finds exactly the right items beside a positive one, and keeps its bit through a save and a reopen, proved through `tools\probes\ViewpointProbe` in its `negate` mode. What is still open is only whether Navisworks OWN EXPORTER writes flags="32", and nothing waits on it. The original step follows.

     355. In Clash Detective, build ONE search set by hand with a NEGATED condition, export the
     selection sets to XML, and paste the `<condition>` element into 5g. Then import that
     same XML into a fresh document and confirm the set comes back with the negation still
     on it. That answers whether `BLD-EL-Devices` can be written as category contains
     Devices AND NOT the six named device categories. Until it is answered the file in
     `exchange\` carries the fallback, `Category equals "Nurse Call Devices"`, which is
     proved to import because the whole file uses `equals`. THOSE TWO LINES ARE HISTORY
     AND NOT THE FILE TODAY: 5g was measured on 2026-09-20, the fallback is gone, and the
     committed file carries one `contains` and six negations at `flags="32"` for that set.
     The words "Nurse Call Devices" appear in it nowhere.
     NOT done in the wiring round. What reflection can say is under 5g: SearchCondition.Negate exists and NegateCondition is bit 32. The hand built set and the round trip still wait for you.
356. Dump the clash members for F72c into 5h: every member on `ClashResult`,
     `IClashResult` and `ClashTest` whose name holds Comment, Note, Description, Tag,
     UserName, Status or Approved, and whether the general `Comments` collection reaches a
     clash result. Then write one comment on one clash by hand, save the NWF, close it,
     open it again and see whether the comment is still there. IF IT CANNOT BE DONE the
     answer is one line in 5h saying so, and the tool then sets the status alone and fakes
     nothing
     DONE on 2026-09-19 in the wiring round by tools\probes\probe-clash-comments.ps1, written into scan.md 5h. TestsEditResultComments writes one, and step 372 is wired to it. Whether it survives a save and reopen is what the run log of the wiring round shows.
357. DONE on 2026-09-20, in the viewpoints round. `ViewpointProbe` in its `walk` mode read the ten C02 NWFs, 47,471 items, 374 distinct values, and the committed revit-categories.txt file under src, Federator.Core, Exchange, holds exactly those 374. The health check now compares against a real list. What is left is Q56, whether the walk should cover more than the C02 folder before the list is trusted project wide. The original step follows.

     357. Open one real federation and walk every item's category property, writing the
     distinct values out. That is F84's list and it is the same walk F86's probe does, so
     the two are measured on one run. Paste the values into
     `src\Federator.Core\Exchange\revit-categories.txt`, one per line, exactly as the model
     spells them, and leave the comment block at the top where it is. Until that file has
     names in it the health check compares against nothing and says so

### How to run the Property Probe, F86, and where its CSV goes
     NOT done in the wiring round. The probe as built reads only the seventeen categories the settings ask for, so it cannot list every category a model carries, and 5i needs its own walk.

358. The button reads `Probe model properties` and it is on the Clash step. It does not run
     as part of a federation run and it never touches an NWF
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
359. Press it and pick either a FOLDER of NWC files or the document you already have open.
     It reads NWC and refuses an NWF or an NWD by name, because opening an NWF replaces
     whatever is open and an NWF is where every clash result lives
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
360. It writes ONE CSV per file, beside that file, named after it with
     `-properties.csv` on the end. So `1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc` gives
     `1104-PAR-1C07BC-ZZZ-ME-MOD-000001-properties.csv` in the same folder
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
361. The CSV has five columns: category, property tab, property name, distinct value, how
     many elements. It covers seventeen categories, which are the thirteen this tool calls
     a service plus the four it has decided are not one, and that list is read off the
     client's own matrix rather than typed
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
362. A property carrying more than 100 distinct values keeps the commonest 100 and gets ONE
     extra row saying how many were left out and how many elements they covered. Nothing is
     dropped silently
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
363. Look for, F86: a `PROBE` block in the log, one per file, saying how many categories
     were asked for, how many were found, how many found no element at all, how many
     properties and distinct values there were, which properties were capped, and whether
     FS or Fire Suppression appears in any tab, name or value. IT SAYS SO AS PLAINLY WHEN
     IT DOES NOT, because a probe that only speaks up when it finds something reads as one
     that found nothing rather than as one that ran
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
364. Send the CSV for one mechanical NWC and the PROBE block beside it. That pair is what
     the mechanical sets get rewritten from, and rewriting them is a later round

### The add-in wiring each fix still needs
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.

365. F74. In `FederationEngine.Decide`, after `document.TryOpenFile` returns true, poll
     `document.Models.Count` through `Federator.Core.Rerun.ModelLoadWait`, handing it the
     count and `RunLog.ElapsedSeconds` each time, pausing `PauseMilliseconds` between
     readings, and write `wait.Line()` when it ends. Where the wait SETTLES, carry on into
     `NwfComparison.Compare` exactly as today. Where it gives up at the ceiling AND the
     count is still zero, return `NwfComparison.ReadEmpty(job.NwfPath, job.Files,
     "after waiting " + seconds)` instead, and carry its `Reason` onto the group as
     `GroupFacts.NwfReadEmptyReason`. Do the SAME thing in `PreviewRunPaths`, which is a
     second reader of the same NWF, or the confirm dialog says Rebuilt about a healthy file
     WIRED on 2026-09-19 in the wiring round, F74, commit fcbdb6b. The seconds handed to the wait are counted from the open RETURNING, off a stopwatch started for it, not RunLog.ElapsedSeconds, because the ceiling is thirty seconds from the open and not from the session.
366. F75. Empty the document at the TOP of `RunOne`, before `Decide`, on the scanned path
     only. Take the census straight after and write `CensusRule.StartOfGroupLine(census,
     true)`, and put `StartOfGroupReason(census, true)` on the group where it is not null.
     The open file run passes `false` and empties nothing, because the document IS the file
     list there
     WIRED on 2026-09-19 in the wiring round, F75, commit b5ce89f.
367. F76. Read the drop down into `ReportOptions.Tolerance`. In `ClashRunner`, where a test
     is created and where a test already in the document is left alone, call
     `options.Tolerance.For(planned.Tolerance, documentUnits)` and set THAT. Write
     `LogLine(created, alreadyThere, documentUnits)` once per group. Put
     `WarningLines(groups, testsInTheFile)` on the confirm dialog
     WIRED on 2026-09-19 in the wiring round, F76, commit b6ec0f2.
368. F76 second half. `ClashRunner` line 406 sets `report.Tolerance = planned.Tolerance`,
     which is the XML and not the document. Read it off the `ClashTest` in the document
     instead and set `report.ToleranceFrom = ToleranceOrigin.Document`. Count the four
     origins across the run and write `ToleranceChoice.ReadFromLine`
     WIRED on 2026-09-19 in the wiring round, F76, commit ee62966.
369. F77. Before creating any test, build a dictionary of locator to item count off the
     sets just resolved, hand it and the plan to `CreationPlan.For`, create only
     `plan.Create`, feed `plan.NotCreated` into the existing skip machinery, and write
     `plan.CountedLine(testsInTheFile)`
     WIRED on 2026-09-19 in the wiring round, F77, commit 03a913e.
370. F83. A second picker beside the clash XML picker on the Clash step, optional, and
     `PickerKind.Priority` for where it opens. Read the file into `PriorityMap.Read`, put
     it on `report.Priorities`, set `test.Priority` on every `TestReport`, write
     `map.MatchLines(testNames)`, and pass `picked` into `WorkbookCheck.Of(path, picked)`.
     Count the clashes into a `PriorityTally` and write `ResultLine` in RESULT
     WIRED on 2026-09-19 in the wiring round, F83, commit 580fe51.
371. F72b. A second tick box under the penetration one, off by default, its label and grey
     line read off `ByDesignPairs.TickLabel` and `HelpLine` and never typed into the XAML.
     A third picker for the pairs file. In the same pass that applies the penetration
     statuses, and NOT a second write path, call `ByDesignRule.Judge` per clash, add the
     wanted ones to the one `statuses.Apply` list, and write the block. The XAML grid has
     two rows today and needs a third, with the Things that destroy data expander moved
     down
     WIRED on 2026-09-19 in the wiring round, F72b, commit 8f7cedd.
372. F72c. Write `record.Text()` as a comment on the clash BEFORE the status is set and on
     the same handle, because every mutator on `DocumentClashTests` is a copy form that
     kills the handle handed to it. If step 356 says a comment cannot be written, write
     `UndoAutoReview.CannotLine(why)` once and set the status alone
     WIRED on 2026-09-19 in the wiring round, F72c, commit 377d1f0.
373. F72c. The `Undo auto Reviewed` button reads every clash, calls `UndoAutoReview.Judge`
     and puts back only the ones that carry our record AND are still at Reviewed, each to
     the status the record names. It goes through the one `ClashStatusEditor` like
     everything else
     WIRED on 2026-09-19 in the wiring round, F72c, commit 377d1f0.
374. F85. `ViewpointBuilder` loops CLASHES and not disciplines now, reading
     `ClashViewpointPlan.For`. `SavedViewpoints.Exists` resolves one folder under the root
     today and cannot see a three deep path, so rebuild the folder walk on `SetBuilder`'s
     measured `EnsureFolders` shape, re-resolving from a FRESH `RootItem` after every
     `AddCopy`. Open the step as `RunSteps.Views`, which is new. LEAVE `CanBuild` FALSE
     until step 375 answers
     DONE on 2026-09-19 in the viewpoints round, commit 11d7ec2: Build takes the clash plan, the old per discipline plan is gone, the folder walk is EnsureFolders re-resolved from a fresh RootItem, and the VIEWS step is opened around it.
375. F85. On a run, save one viewpoint by hand while some items are hidden, then read
     `SavedViewpoint.ContainsVisibilityOverrides` on it and press it again from a clean
     view. That answers whether a viewpoint records the hiding, which is the last thing
     `CanBuild` waits on, and the answer goes in `docs\history\scan.md` 5d
     DONE on 2026-09-19 in the viewpoints round by tools\probes\ViewpointProbe, written into scan.md 5j and not 5d: a viewpoint captured with CaptureRuntimeOverrides records the hiding and brings it back after a save and a reopen, and CanBuild is true.
376. F80. Two lines in the window changed already: `log.RunStarted(jobs.Count)` and
     `log.RunFinished()` in place of the two `log.Line` calls that wrote the same
     sentences. Read them once and check the log reads `RUN      started, 7 groups` and
     `RUN      finished` exactly as before
377. F81. Four call sites changed already: three in `ClashRunner` now use
     `log.NumberedRepeat` and the per group SETS block in `FederationEngine` is gone. Read
     them once, then on the next run check the `.log` is under 300 KB and the `.tsv` still
     carries a row for every test
378. F82. `FederationEngine` feeds `SetsAcrossTheRun` beside `outcome.Sets` already and the
     window writes the block after SOURCE FINDINGS already. Read both once
379. Build it: `dotnet build ParsonsNwcFederator.sln -c Release`. EXPECT ERRORS the first
     time. Eighteen fixes touched the add-in and none of them was compiled. Send the whole
     error list if there is one
380. Run one building and send the log. Look for, in order: `LOADING` then `STOPPED` or a
     model count, `CENSUS CLEAR` at the top of the group, `CENSUS NOTED` and NOT `CENSUS
     CHANGED` around APPEND, `TOLERANCE`, `CLASH 1830 in the file`, `PRIORITY` if a file
     was picked, `REVIEWED rule B`, `VIEWS`, `SETS ACROSS THE RUN`, and a RESULT block
     carrying `run time`, `waiting for the person` and both file sizes

## Proof of the viewpoints round, F85 written, 2026-09-20

Six runs of ten groups were driven from the session on this machine against copies of the
C02 NWF folder, the last two clean, and every number is in the round's entry in
`steps\log.md`. What is left is the look a person gives it and the two folders nobody but
Bader may touch.

381. Pull `round-viewpoints` and read the round entry at the top of `steps\log.md`, the list
     of every program it started, every file it wrote outside the repo and the two times
     Navisworks had to be stopped rather than closed
382. Open `C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints\NWF6\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`
     in Navisworks. It is a copy of the 1A02MM NWF after the sixth run, in the temp folder,
     so it can be deleted after
383. Look for, in Saved Viewpoints: the four folders the file already had, then `A`, `B` and
     `C`, and under `A` the pair folders `AR vs ST`, `ST vs ST`, `AR vs DR`, `DR vs ST`,
     `AR vs EL`, `EL vs ST` and `ST vs UNKNOWN`
384. Press one viewpoint under `A`, `AR vs EL`. Look for: the view jumps to the clash, framed
     the way its picture in the report frames it, and in the Selection Tree the ME and ST
     models grey out while AR and EL stay
385. Press one under `A`, `DR vs ST`, drainage against structure. Look for: the ME model
     stays shown as well as ST, because the drainage pipe lives in it, and only AR and EL
     grey out. Clash 2 of the framing test opens on grey, the camera inside a member,
     which is question 57
386. Beside his NWCs in `C:\00-NM\Federation Task\C02 + 04\C02\NWC` sit four files the
     WIRING round's probe button wrote on 2026-09-19, `1104-PAR-1A02WO-ZZZ-AR-MOD-000001-properties.csv`
     and its EL, ME and ST siblings. This round wrote nothing there and deleted nothing.
     They are Bader's to delete
387. When ready, run once on the real folders with the same boxes: 25 mm, by design on,
     penetrations on, the priority file. Look for, per group with clashes:
     `VIEWS    read back on N of M created viewpoint(s)`, the two numbers being equal today because every one is read back, so a run where they differ is the thing to send, `VIEWS    the model each clash item
     lives in was read for N of N`, and `hidden state put back`, then a `VIEWS` step of a
     few seconds in the timing block.

     DO NOT EXPECT `Nothing failed` ANY MORE, and this step said to until the worksets
     round. Since Q70 a group whose model names `Internal` as its shared site is FAILED,
     and two of the ten are: 1A02MM and 1A02WL. The run of 2026-09-20 16:20 reads
     `groups done : 8` and `groups failed : 2`, and `Nothing failed.` is written only
     when failed groups plus errors is zero, so on his folders it cannot appear at all.
     Expect instead `ALIGNMENT failed 2 group(s)` at the run tail with one line naming
     each, and remember that BOTH still wrote their NWF, their NWD and their report.
388. Look for, IN TWO DIFFERENT PLACES, which this step used to get wrong by putting both
     at the run tail. `Revit categories known: 374` and `Sets asking for a category no
     model carries: 14`, which is question 56, are in the `HEALTH <file>.xml` block, which
     is written WHEN THE CLASH XML IS PICKED and not at the end of the run. The
     `WORKBOOK 18,300 test blocks` line, which is question 54, IS at the run tail
389. Answer questions 54, 55 and 56 in `02_questions.md`

## Proof of the dimming round, one step, 2026-09-20

The viewpoints now dim everything but the two clashing items, the way Clash Detective
does, which is what you asked for when you pressed two and could not see the clash. One
step, about a minute.

390. SUPERSEDED BY STEP 391, and worth keeping for what it says about the dimming on its own. THIS FILE PREDATES THE COLOURS: it was written by the dimming round and its viewpoints ghost the scene without painting the two items red and green. Step 391 is the same check on your own NWF with the colours in. Open this file in Navisworks, which is a copy and yours to delete afterwards:

     `C:\Users\bader\AppData\Local\Temp\claude\round-dimming\NWF3\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`

     In Saved Viewpoints open `A`, then `DR vs ST`, and press these two:

     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Framing  Clash2`
     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors  Clash1`

     Look for: everything ghosted except the pipe and the thing it hits, which stay
     solid, and AR and EL greyed out in the Selection Tree. The first of those two is
     the one that was a featureless grey wall before this round. The second shows the
     roof ghosted with the plant visible through it.

     If the dimming is too strong or too weak, it is one number and no rebuild of your
     models: `ViewpointSettings.DimTransparency`, 0.85 today, and 0 switches it off.

     WHAT TO WEIGH WHILE YOU LOOK. Dimming is not free and the numbers are in the round
     entry in `steps\log.md`. The run went from 5 minutes 3 seconds to 10 minutes 26, and
     1A02MM's NWF went from 119,542 bytes to 23,327,744, which is 195 times, because a
     dimmed viewpoint records one material override per item it dims and that group
     writes 430 of them. Question 59 asks whether that is worth it to you. Nothing is
     capped and nothing is thinned, because that is your call and not mine.

## Proof of the alignment round, two steps, 2026-09-20

THIS ROUND RAN AGAINST YOUR OWN FOLDERS, not a copy, which is what you asked for in Q60.
The backup was taken first and read back before anything else happened, 9 files against
9 with every byte size compared one for one and no mismatch:

`C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20`

THERE IS A SECOND ONE NOW, taken by the worksets round before its own run on the live
folders the same day, 14 files, 40,008,819 bytes, read back the same way:

`C:\00-NM\Federation Task\C02 + 04\C02\NWF-backup-2026-09-20-worksets`

Both folders are yours to delete once you are satisfied. Nothing else of yours was touched.

391. Open your own 1A02MM federation, which now carries the viewpoints with the colours:

     `C:\00-NM\Federation Task\C02 + 04\C02\NWF\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`

     In Saved Viewpoints open `A`, then `DR vs ST`, and press these two:

     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Framing  Clash2`
     `BLD-DR-Pipes & Pipe Fittings-vs-BLD-ST-Floors  Clash1`

     Look for: everything ghosted as before, and now the two clashing items RED and
     GREEN, red on the first side of the clash and green on the second, which is the
     order Clash Detective paints them so the viewpoint and the panel agree.

     Both colours are settings, `ViewpointSettings.FirstItemColour` and
     `SecondItemColour`, and `ColoursTheTwoItems` switches the painting off and leaves
     the ghosting.

     A THIRD SETTING CHANGED UNDER THIS ONE AND IS WORTH KNOWING. Every viewpoint on
     your machine is now written by the COM folder route, `RecordsThroughTheFolder`,
     which is Q59's answer. It records all four counts exactly as the old route did,
     measured both ways in scan.md 5p, and it saves three per cent rather than the two
     thirds the call count suggests. If a viewpoint ever comes back empty, that setting
     is the first thing to turn off, and it needs no rebuild of your models.

     ONE THING TO KNOW WHILE YOU LOOK. A viewpoint only records a colour where it
     DIFFERS from the item's own colour, so an item that already is the colour it is
     being given records nothing and still looks right. That is measured, scan.md 5p, and
     it is why the read back asks what the viewpoint will SHOW rather than what it wrote.

392. Read the ALIGNMENT and EXPORT CHECK blocks for 1A02MM in the run log, which is
     already beside your NWFs:

     `C:\00-NM\Federation Task\C02 + 04\C02\NWF\run-20260920-162006.log`

     Search it for `ALIGNMENT 1A02MM`. It says your four models agree in X and Y, differ
     in Z by up to 95 mm, and name FOUR different Revit shared sites, one of them
     `Internal`, which is what Revit calls a model that was not exported on a shared site
     at all. SINCE Q70 THAT FAILS THE GROUP, so 1A02MM now ends FAILED with the line `THIS GROUP IS FAILED` under the block naming the model. It still wrote its NWF, its NWD and its report, because the evidence is the point. Q65 said neither block can fail a group and Q70 answered later overrides it for this one case.

     Then search for `EXPORT CHECK 1A02MM`. Every model carries a workset on every
     element and an element id on every element, so the export itself is clean. THE LINE
     UNDER IT IS THE ONE THAT MATTERS: your models carry `ME-Ductwork` and the matrix
     asks for `ME-DUCTWORK`, the match is case sensitive, and that is why 33 of your 61
     sets find nothing in every group. QUESTION 68 IS ANSWERED and the matrix is corrected: it asks for `ME-Ductwork` now, four values and 17 conditions.
     AND IT CHANGED NO CLASH COUNT, which is Q72: a set already in your NWF keeps the conditions it was built with, so the corrected file never reaches it. The SETS block says so now, under the already-there count. What the correction cannot do is
     reach a set that is already there.

     THE OTHER THING IN THAT BLOCK, and it is only in the 16:20 log because the worksets
     round added it: your own workset names disagree with each other. The block NAMES the
     three that look like typos, `EL-Lightining Protection` against `EL-Lightning
     Protection`, `EV-Ccctv system` against `EV-Cctv System`, and `PL-Drainage equipmen`
     against `PL-Drainage equipment`, with which model carries which. It does NOT name
     `AR-EXTERIOR` against `AR-INTERIOR` or `ST-SUB` against `ST-SUP`, because those are
     two pairs of real worksets and a person has decided so once, in the measured list.
     It still COUNTS them, so nothing went quiet. No rule can fix the three typos, and
     the tool deliberately does not absorb them, because then nobody would fix the models.

## The fixtures, added 2026-09-21, and they come before either live folder

393. Install the bundle, then run the general fixture. It is three copies of C04's
     1A04WE models under `C:\Users\bader\AppData\Local\Temp\claude\round-close\fixture`,
     308 KB, and it takes about twenty seconds:

         build\install.ps1
         <temp>\round-close\open-addin.ps1
         <temp>\round-close\drive.ps1 -Source <temp>\round-close\fixture\NWC `
             -Nwf <temp>\round-close\fixture\NWF -Nwd <temp>\round-close\fixture\NWD `
             -Excel <temp>\round-close\fixture\Report

394. Look for: `groups done    : 1`, `groups failed  : 0`, `clashes found  : 27 across
     1 group`, 66 tests run and 1,764 skipped, and 27 viewpoints created and read back.
     Anything else and do not go near a live folder until it is understood

395. Look for, Q73: the workbook it wrote is about 1,917 rows holding 1,830 blocks, of
     which 8 found something. Under the old shape the same workbook was 14,667 rows

396. Run the penetration fixture, which is two copies of C04's 1A04WM models, 988 KB,
     about eighteen seconds, at `<temp>\round-close\fixture-pen`

397. Look for: `moved to Reviewed : 7` of `clashes looked at : 14`, and REAL SIZES in the
     moved lines, `Pipes 21mm through` and `41.7mm` and `82mm`, all through
     `BLD-AR-Floors`. IT IS AR AND ME AND NOT ST ON PURPOSE: the solid list is Walls,
     Floors, Roofs and Structural Foundations, and three of those four are ARCHITECTURE
     categories in these models. An ST fixture would fire only on foundations and would
     probably move nothing, reading exactly like the rule being broken

398. Look for, on either fixture: a penetration total of zero with all six buckets
     spread is a MEASUREMENT. A zero with everything in `not a service against a solid`
     and nothing in the other five is 5r's signature and IS a fault. Read the buckets,
     never the total

399. What the fixtures CANNOT show, so a green fixture is never read as a green run:
     scale, the weekly path, a CHANGED group, alignment across many models, the two non
     buildings, the single discipline groups, and anything needing the real matrix
     against real content

400. THE VIEWPOINT CEILING, F99. On the Outputs step, open `More, rarely changed`. Under
     the photo settings look for a row reading `Viewpoints`, then
     `At most per group, 0 for no cap`, then a box. IT MUST START AT 0, which is off, so a
     run that touches nothing behaves exactly as it did. It is a box and NOT a tick box,
     so step 212's count of four visible tick boxes is unchanged

401. Type 300 into that box and run C04. Look for, in RUN SETTINGS,
     `viewpoint ceiling: 300 per group at most, so a group with more clashes than that
     leaves the rest without one`. With the box at 0 the same line reads
     `none, every clash in scope gets a viewpoint`. A SETTING THAT SHAPES A RUN AND IS
     MISSING FROM THIS BLOCK IS THE FAULT THIS BLOCK EXISTS TO STOP, and the ceiling was
     missing from it on the first run that used it

402. Look for, in the VIEWS plan block of every group while a ceiling is set, one line
     reading `over the ceiling of 300 viewpoint(s) for this group`. It ends
     `. THE CEILING WAS REACHED and those clashes have no viewpoint` where it bit and
     `, which was not reached` where it did not. IT IS WRITTEN EITHER WAY ON PURPOSE,
     because a block silent about a ceiling reads as a run that had none

403. Look for, F97, in the SET DRIFT block of a group whose sets have drifted: FOUR lines
     per set and not three. After `it asks` and `file asks` comes
     `why      : they differ only in the CASE of a value, the set asks "ME-DUCTWORK" and
     the file asks "ME-Ductwork"`. Two sentences differing in one letter's case are what
     three people have already had to compare character by character

404. Look for, F97, THE THING THAT PROVES THE REBUILD NOW STICKS. On a C02 run, `1000BS`
     and `1A02MS` are the two groups that used to report `NWF      checked` with the same
     byte count they opened with, 13,091 and 81,957. They must now report
     `NWF      written` with a DIFFERENT count. If they still say `checked`, the rebuild
     is still being thrown away and the same fourteen sets will drift again next week

405. Run all three checks and not two. `sh tools/checks/check-locals.sh src`, then
     `sh tools/checks/check-imports.sh src`, then `sh tools/checks/check-facts.sh`. The
     third is F96 and it reads 13 lines across 5 facts. The pre-commit hook still runs
     only the first two and the test set, which is unchanged and correct

406. Prove the third one REFUSES, which is the half that matters:
     `sh tools/checks/check-facts.sh tools/checks/broken/facts-broken.tsv`. It must name
     five faults by file and line and exit 1. A check over prose that only ever passes
     cannot be told apart from a check that reads nothing, and three of its own first
     five facts did exactly that before it counted what it examined
