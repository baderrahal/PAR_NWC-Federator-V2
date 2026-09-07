# 03 Bader next

One action per step. Do them in order.
Five proofs are waiting: F5, F6, F7, F8 and F22. One build, one install and one Navisworks session cover all five.

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

11. Read the last lines. It must say the bundle is complete. If it refuses, copy the whole output into the chat

## Proof F22 part one, the labels before the first run

12. Close Navisworks if it is open
13. Open Navisworks Manage 2025
14. Open the add-in from the ribbon
15. Look for: the window title carries today's commit and build time
16. Pick the NWC folder and press Scan
17. Go to the Grouping step
18. Look for: a column named Run as in the group list, reading Unknown until the NWF folder is picked
19. Go to the Outputs step and pick an NWF folder that holds no NWF yet, and the NWD folder
20. Go back to the Grouping step
21. Look for: every group reads First run
22. Go to the Clash step and pick the clash XML
23. Look for: every group still reads First run, because no NWF is there yet

## Proof F9, a changed folder is left alone

Do this after the F5 proof below has run once, so an NWF exists. Skip it on the first pass and come back to it after the F7 proof if that is easier.

24. Pick a building whose NWF already exists in the NWF folder
25. In Explorer note the modified time of that NWF
26. Copy one NWC of that building out of the NWC folder to the desktop, so the folder lost one file
27. Press Scan again and tick that building only
28. Press Run and press OK on the dialog
29. Look for: the log has a CHANGED block naming the removed file, then one line `GROUP    <building> skipped, the NWF on disk no longer matches the folder`
30. Look for: no UNITS line for that group
31. Look for: the GROUP finished line ends with `Skipped (changed on disk)` and PARTIAL, not FAILED
32. Look for: the RESULT block carries `skipped        : 1`
33. In Explorer look for: the NWF keeps the modified time from step 25
34. Copy the NWC back into the NWC folder

## Proof F10, each output on its own switch

The window has no box for the workbook or the page, both are fixed on, so this proof uses the two switches it has: the XML box and the image status boxes. Do it after the F5 proof below, on the same building.

35. Go to the Outputs step and open More
36. Tick Write a clash XML beside each workbook
37. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
38. Tick the same building and press Run, then OK
39. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
40. Look for: `IMAGES   skipped  images are switched off for this run`
41. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
42. Untick Write a clash XML and tick New, Active and Reviewed again
43. Press Run again, then OK
44. Look for: `XML      skipped  not wanted this run`
45. Look for: the pictures rendered again, the CLASH block counts them

## Proof F5, one building twice with the XML

46. Tick one building only
47. Press Run
48. Look for: the confirm dialog opens with `First run: 1` and says the document is cleared before each one
49. Press OK and wait for it to finish
50. Go to the Grouping step
51. Look for: that group now reads Weekly run plus XML, the others still First run
52. Press Run again on the same building, same folders, same XML
53. Look for: the confirm dialog says `Weekly run plus XML: 1` and `First run: 0`, and does not say cleared
54. Press OK and wait for it to finish
55. Look for: the log says OPENED for the group
56. Look for: the SETS block says the sets are already there
57. Look for: the line `CLASH    source   tests from XML` and the tests running after it

## Proof F8 on the scanned run, no XML

58. Clear the clash XML box on the Clash step
59. Go to the Grouping step
60. Look for: the run group reads Weekly run and the others First run
61. Keep the same building ticked and the same folders
62. Press Run
63. Look for: the confirm dialog says `Weekly run: 1`
64. Press OK and wait for it to finish
65. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
66. Look for: the CLASH block says the tests ran and the workbook was written
67. Look for: no SETS block, because no XML means the sets are left alone

## Proof F6, the open file report folder

68. In Navisworks open the NWF the F5 run wrote
69. Open the add-in from the ribbon
70. Go to the Clash step
71. Look for: the blue line above Run the open file starts with `Weekly run.` and names one Clash Reports folder beside the NWF
72. Leave the clash XML box empty
73. Press Run the open file and wait for it to finish
74. Open the NWF's folder in Explorer
75. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
76. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

77. Stay on the run from step 73
78. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
79. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

80. Stay in the same NWF folder in Explorer
81. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the press in step 73
82. Open that log in Notepad
83. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
84. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
85. Look for: a RESULT block at the end with groups done, partial and failed
86. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

87. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
88. Copy the four newest `run-*.log` files
89. Paste them into `steps\logs` in the repo folder
90. Rename each with the date, the building and the press, like `2026-09-08-1C07BC-first.log`, `2026-09-08-1C07BC-second.log`, `2026-09-08-1C07BC-noxml.log` and `2026-09-08-1C07BC-openfile.log`
91. Open GitHub Desktop
92. Write a summary like `logs from 1C07BC, F5 to F22 proofs`
93. Press Commit to main
94. Press Push origin

## Run the window probe

95. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File build\probe-window-defaults.ps1
```

96. Look for: it prints the window defaults and no error about a path
