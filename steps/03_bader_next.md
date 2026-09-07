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

## Proof F5, one building twice with the XML

24. Tick one building only
25. Press Run
26. Look for: the confirm dialog opens with `First run: 1` and says the document is cleared before each one
27. Press OK and wait for it to finish
28. Go to the Grouping step
29. Look for: that group now reads Weekly run plus XML, the others still First run
30. Press Run again on the same building, same folders, same XML
31. Look for: the confirm dialog says `Weekly run plus XML: 1` and `First run: 0`, and does not say cleared
32. Press OK and wait for it to finish
33. Look for: the log says OPENED for the group
34. Look for: the SETS block says the sets are already there
35. Look for: the line `CLASH    source   tests from XML` and the tests running after it

## Proof F8 on the scanned run, no XML

36. Clear the clash XML box on the Clash step
37. Go to the Grouping step
38. Look for: the run group reads Weekly run and the others First run
39. Keep the same building ticked and the same folders
40. Press Run
41. Look for: the confirm dialog says `Weekly run: 1`
42. Press OK and wait for it to finish
43. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
44. Look for: the CLASH block says the tests ran and the workbook was written
45. Look for: no SETS block, because no XML means the sets are left alone

## Proof F6, the open file report folder

46. In Navisworks open the NWF the F5 run wrote
47. Open the add-in from the ribbon
48. Go to the Clash step
49. Look for: the blue line above Run the open file starts with `Weekly run.` and names one Clash Reports folder beside the NWF
50. Leave the clash XML box empty
51. Press Run the open file and wait for it to finish
52. Open the NWF's folder in Explorer
53. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
54. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

55. Stay on the run from step 51
56. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
57. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

58. Stay in the same NWF folder in Explorer
59. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the press in step 51
60. Open that log in Notepad
61. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
62. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
63. Look for: a RESULT block at the end with groups done, partial and failed
64. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

65. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
66. Copy the four newest `run-*.log` files
67. Paste them into `steps\logs` in the repo folder
68. Rename each with the date, the building and the press, like `2026-09-08-1C07BC-first.log`, `2026-09-08-1C07BC-second.log`, `2026-09-08-1C07BC-noxml.log` and `2026-09-08-1C07BC-openfile.log`
69. Open GitHub Desktop
70. Write a summary like `logs from 1C07BC, F5 to F22 proofs`
71. Press Commit to main
72. Press Push origin
