# 03 Bader next

One action per step. Do them in order.
Eight proofs are waiting: F5, F6, F7, F8, F9, F10, F17 and F22. One build, one install and one Navisworks session cover all eight.

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

10. Install:

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

11. Look for: the build finishes with no error about a missing name. F11 deleted dead code that the container cannot compile the add-in against, so this build is its only proof
12. Read the last lines of the install. It must say the bundle is complete. If it refuses, copy the whole output into the chat

## Proof F22 part one, the labels before the first run

13. Close Navisworks if it is open
14. Open Navisworks Manage 2025
15. Open the add-in from the ribbon
16. Look for: the window title carries today's commit and build time
17. Pick the NWC folder and press Scan
18. Go to the Grouping step
19. Look for: a column named Run as in the group list, reading Unknown until the NWF folder is picked
20. Go to the Outputs step and pick an NWF folder that holds no NWF yet, and the NWD folder
21. Go back to the Grouping step
22. Look for: every group reads First run
23. Go to the Clash step and pick the clash XML
24. Look for: every group still reads First run, because no NWF is there yet

## Proof F9, a changed folder is left alone

Do this after the F5 proof below has run once, so an NWF exists. Skip it on the first pass and come back to it after the F7 proof if that is easier.

25. Pick a building whose NWF already exists in the NWF folder
26. In Explorer note the modified time of that NWF
27. Copy one NWC of that building out of the NWC folder to the desktop, so the folder lost one file
28. Press Scan again and tick that building only
29. Press Run and press OK on the dialog
30. Look for: the log has a CHANGED block naming the removed file, then one line `GROUP    <building> skipped, the NWF on disk no longer matches the folder`
31. Look for: no UNITS line for that group
32. Look for: the GROUP finished line ends with `Skipped (changed on disk)` and PARTIAL, not FAILED
33. Look for: the RESULT block carries `skipped        : 1`
34. In Explorer look for: the NWF keeps the modified time from step 26
35. Copy the NWC back into the NWC folder

## Proof F10, each output on its own switch

The window has no box for the workbook or the page, both are fixed on, so this proof uses the two switches it has: the XML box and the image status boxes. Do it after the F5 proof below, on the same building.

36. Go to the Outputs step and open More
37. Tick Write a clash XML beside each workbook
38. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
39. Tick the same building and press Run, then OK
40. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
41. Look for: `IMAGES   skipped  images are switched off for this run`
42. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
43. Untick Write a clash XML and tick New, Active and Reviewed again
44. Press Run again, then OK
45. Look for: `XML      skipped  not wanted this run`
46. Look for: the pictures rendered again, the CLASH block counts them

## Proof F17, the pictures are numbered in the export order

This uses the run from step 44, the one with the pictures on. The rule: the picture on row N of a block is picture N of that block, and the blocks are numbered in the order the workbook lists the tests, most clashes first.

47. Stay on the run from step 44
48. Look for: one line `IMAGES   numbered in report order: <n> renamed, <n> already right, 0 missing` in the log after the CLASH block
49. Open the Clash Reports folder beside the NWF folder and open that building's workbook in Excel
50. In Navisworks open Clash Detective and select the test that is the first block of the workbook, the one with the most clashes
51. Look for: the clashes on the sheet are in the same order the Clash Detective panel lists them
52. Look for: the Image cell on row 1 of that block links to cd000001.jpg, row 2 to cd000002.jpg, and so on, the row number and the picture number the same
53. Look for: the second block's pictures start at cd010001.jpg, the third at cd020001.jpg
54. Click the Image cell on three rows in three different blocks
55. Look for: every link opens its picture, and the picture shows the two items named on that row
56. In Explorer open the `_files` folder beside the workbook and look for: no file ending `.moving`

## Proof F5, one building twice with the XML

57. Tick one building only
58. Press Run
59. Look for: the confirm dialog opens with `First run: 1` and says the document is cleared before each one
60. Press OK and wait for it to finish
61. Go to the Grouping step
62. Look for: that group now reads Weekly run plus XML, the others still First run
63. Press Run again on the same building, same folders, same XML
64. Look for: the confirm dialog says `Weekly run plus XML: 1` and `First run: 0`, and does not say cleared
65. Press OK and wait for it to finish
66. Look for: the log says OPENED for the group
67. Look for: the SETS block says the sets are already there
68. Look for: the line `CLASH    source   tests from XML` and the tests running after it

## Proof F8 on the scanned run, no XML

69. Clear the clash XML box on the Clash step
70. Go to the Grouping step
71. Look for: the run group reads Weekly run and the others First run
72. Keep the same building ticked and the same folders
73. Press Run
74. Look for: the confirm dialog says `Weekly run: 1`
75. Press OK and wait for it to finish
76. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
77. Look for: the CLASH block says the tests ran and the workbook was written
78. Look for: no SETS block, because no XML means the sets are left alone

## Proof F6, the open file report folder

79. In Navisworks open the NWF the F5 run wrote
80. Open the add-in from the ribbon
81. Go to the Clash step
82. Look for: the blue line above Run the open file starts with `Weekly run.` and names one Clash Reports folder beside the NWF
83. Leave the clash XML box empty
84. Press Run the open file and wait for it to finish
85. Open the NWF's folder in Explorer
86. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
87. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

88. Stay on the run from step 84
89. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
90. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

91. Stay in the same NWF folder in Explorer
92. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the press in step 74
93. Open that log in Notepad
94. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
95. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
96. Look for: a RESULT block at the end with groups done, partial and failed
97. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

98. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
99. Copy the four newest `run-*.log` files
100. Paste them into `steps\logs` in the repo folder
101. Rename each with the date, the building and the press, like `2026-09-08-1C07BC-first.log`, `2026-09-08-1C07BC-second.log`, `2026-09-08-1C07BC-noxml.log` and `2026-09-08-1C07BC-openfile.log`
102. Open GitHub Desktop
103. Write a summary like `logs from 1C07BC, F5 to F22 proofs`
104. Press Commit to main
105. Press Push origin

## Run the window probe

106. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File build\probe-window-defaults.ps1
```

107. Look for: it prints the window defaults and no error about a path
