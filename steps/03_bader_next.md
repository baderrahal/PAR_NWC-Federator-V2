# 03 Bader next

**The last run, 2026-09-07 09:34, was on the old build be0b9b37 of 1 Sep. Pull main and build before anything.**

One action per step. Do them in order. One build, one install and one Navisworks session cover every proof so far: F24 first, because it is what you saw, then F22, F9, F10, F17, F5, F8, F6, F7.

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

10. Look for: the build finishes with no error. If it names CreateCopy or CopyFrom on DocumentSelectionSets, copy the whole error into the chat, that is the one member F24 could not measure from the container
11. Install:

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

12. Read the last lines of the install. It must say the bundle is complete. If it refuses, copy the whole output into the chat

## Proof F24, the six CHANGED groups are rebuilt from the scan

13. Close Navisworks if it is open
14. Open Navisworks Manage 2025 with nothing open
15. Open the add-in from the ribbon
16. Look for: the window title carries today's commit and build time, not be0b9b37
17. Pick the same C06 NWC folder as last time and press Scan
18. Pick the same NWF folder and NWD folder as last time
19. Leave the clash XML box empty
20. Tick every group and press Run
21. Look for: the label at the bottom reads Checking NWF 1 of 14 and counts up, then the confirm dialog opens
22. Look for: the dialog says `Rebuilt: 6` and that the NWF is cleared and rebuilt from the scan folder with its saved tests kept
23. Press Cancel
24. Look for: the log says `Run cancelled before anything was cleared.`
25. Go to the Grouping step
26. Look for: 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE read Rebuilt in the Run as column, and the other eight read Weekly run
27. Press Run again, then OK
28. Look for: per rebuilt group the log has a CHANGED line with the counts, then a REBUILT block, `REBUILT <nwf>  1 added, 4 moved, 0 removed` for 1B06BC, then one line per file, then one `saved tests` line
29. Look for: the `saved tests` line says `kept` or `none`, never `LOST`
30. Look for: the GROUP finished line of each of the six ends with `DONE` and `Rebuilt`
31. Look for: the RESULT block carries `rebuilt        : 6` and `weekly run     : 8`
32. In Explorer open the NWF folder and open the NWF of 1B06BC in Navisworks
33. Look for: the Selection Tree lists 5 models, EL among them, all from the Published folder
34. Open the NWD of 1B06BC from the NWD folder
35. Look for: the same 5 models
36. Look for: no group in this run reads PARTIAL

## Proof F26, the report is always in metres

This one is its own small run, with the clash XML, because the conversion only happens where a report is written.

37. Go to the Outputs step
38. Look for: the combo is labelled Model units and the grey line under it says the report is always in metres
39. Set that combo to Feet, on purpose, so the models are put into feet
40. Go to the Clash step and pick the clash XML
41. Go to the Grouping step and tick 1B06PH only
42. Press Run, then OK, and wait for it to finish
43. Look for: the line `model units      : Feet, which is what the models are set to`
44. Look for: the line `report units     : Meters (m), always, converted before anything is written`
45. Look for: a UNITS line saying how many models were set and what the document shows
46. Look for: NO line anywhere in the log says DID NOT FOLLOW
47. Look for: one line `UNITS    every number converted from ft to m, one ft is 0.3048 m, ...` before the XLSX line
48. Open the workbook in the Clash Reports folder beside the NWF folder
49. Look for: the Tolerance cell of the first block reads `0.075m`, not `0.246ft`
50. Look for: the Distance column holds metres, so a hard clash reads about -0.050, not -0.164
51. In Navisworks open Clash Detective and select that test
52. In Options, Interface, Display Units, set Linear Units to Metres
53. Look for: the panel tolerance reads 0.075 and one clash distance matches its workbook row to three decimals
54. Go back to the Outputs step and set Model units to Metres

## Proof F22, the labels before a run

55. Go to the Outputs step and pick an NWF folder that holds no NWF yet
56. Go back to the Grouping step
57. Look for: every group reads First run
58. Go to the Clash step and pick the clash XML
59. Look for: every group still reads First run, because no NWF is there yet
60. Put the NWF folder back to the C06 one
61. Look for: every group reads Weekly run plus XML

## Proof F5, one building twice with the XML

62. Tick one building only, 1B06PH
63. Press Run
64. Look for: the confirm dialog says `Weekly run plus XML: 1` and does not say cleared
65. Press OK and wait for it to finish
66. Look for: the log says OPENED for the group
67. Look for: the SETS block says the sets were created
68. Look for: the line `CLASH    source   tests from XML` and the tests running after it
69. Press Run again on the same building, same folders, same XML
70. Press OK and wait for it to finish
71. Look for: the SETS block says the sets are already there, and the tests are already there

## Proof F9, a changed folder is rebuilt and not run in feet

72. Copy one NWC of 1B06PH out of the NWC folder to the desktop, so the folder lost one file
73. Press Scan again and tick 1B06PH only
74. Press Run
75. Look for: the dialog says `Rebuilt: 1`
76. Press OK
77. Look for: the REBUILT block names the removed file with `removed`
78. Look for: no UNITS line comes before the REBUILT block
79. Copy the NWC back into the NWC folder
80. Press Scan again, tick 1B06PH, press Run, press OK, so the NWF holds all its files again
81. Look for: the REBUILT block names the file with `added`

## Proof F10, each output on its own switch

82. Go to the Outputs step and open More
83. Tick Write a clash XML beside each workbook
84. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
85. Tick 1B06PH and press Run, then OK
86. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
87. Look for: `IMAGES   skipped  images are switched off for this run`
88. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
89. Untick Write a clash XML and tick New, Active and Reviewed again
90. Press Run again, then OK
91. Look for: `XML      skipped  not wanted this run`
92. Look for: the pictures rendered again, the CLASH block counts them

## Proof F17, the pictures are numbered in the export order

93. Stay on the run from step 90
94. Look for: one line `IMAGES   numbered in report order: <n> renamed, <n> already right, 0 missing` after the CLASH block
95. Open the Clash Reports folder beside the NWF folder and open the 1B06PH workbook in Excel
96. In Navisworks open Clash Detective and select the test that is the first block of the workbook, the one with the most clashes
97. Look for: the clashes on the sheet are in the same order the Clash Detective panel lists them
98. Look for: the Image cell on row 1 of that block links to cd000001.jpg, row 2 to cd000002.jpg, and so on
99. Look for: the second block's pictures start at cd010001.jpg
100. Click the Image cell on three rows in three different blocks
101. Look for: every link opens its picture, and the picture shows the two items named on that row
102. In Explorer open the `_files` folder beside the workbook and look for: no file ending `.moving`

## Proof F8 on the scanned run, no XML

103. Clear the clash XML box on the Clash step
104. Go to the Grouping step
105. Look for: 1B06PH reads Weekly run
106. Press Run, then OK
107. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
108. Look for: the CLASH block says the tests ran and the workbook was written
109. Look for: no SETS block, because no XML means the sets are left alone

## Proof F6, the open file report folder

110. In Navisworks open the NWF of 1B06PH
111. Open the add-in from the ribbon
112. Go to the Clash step
113. Look for: the blue line above Run the open file starts with `Weekly run.` and names one Clash Reports folder beside the NWF
114. Leave the clash XML box empty
115. Press Run the open file and wait for it to finish
116. Open the NWF's folder in Explorer
117. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
118. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

119. Stay on the run from step 115
120. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
121. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

122. Stay in the same NWF folder in Explorer
123. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the press in step 97
124. Open that log in Notepad
125. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
126. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
127. Look for: a RESULT block at the end with groups done, partial and failed
128. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

129. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
130. Copy every `run-*.log` written today
131. Paste them into `steps\logs` in the repo folder
132. Rename each with the date, the building and the press, like `2026-09-08-C06-rebuilt.log`, `2026-09-08-1B06PH-first.log`, `2026-09-08-1B06PH-second.log`, `2026-09-08-1B06PH-noxml.log` and `2026-09-08-1B06PH-openfile.log`
133. Open GitHub Desktop
134. Write a summary like `logs from C06, F24 to F7 proofs`
135. Press Commit to main
136. Press Push origin

## Run the window probe

137. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-defaults.ps1
```

138. Look for: it prints the window defaults and no error about a path
