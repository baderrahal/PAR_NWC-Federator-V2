# 03 Bader next

One action per step. Do them in order.
Four proofs are waiting: F5, F6, F7 and F8. One build, one install and one Navisworks session cover all four.

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

## Proof F5, one building twice with the XML

12. Close Navisworks if it is open
13. Open Navisworks Manage 2025
14. Open the add-in from the ribbon
15. Look for: the window title carries today's commit and build time
16. Pick the NWC folder and press Scan
17. Tick one building only
18. Pick the NWF folder, the NWD folder and the clash XML
19. Press Run and wait for it to finish
20. Press Run again on the same building, same folders, same XML
21. Look for: the second run says OPENED for the group
22. Look for: the SETS block says the sets are already there
23. Look for: the line `CLASH    source   tests from XML` and the tests running after it

## Proof F8 on the scanned run, no XML

24. Clear the clash XML box on the Clash step
25. Keep the same building ticked and the same folders
26. Press Run and wait for it to finish
27. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
28. Look for: the CLASH block says the tests ran and the workbook was written
29. Look for: no SETS block, because no XML means the sets are left alone

## Proof F6, the open file report folder

30. In Navisworks open the NWF the F5 run wrote
31. Open the add-in from the ribbon
32. Go to the Clash step
33. Look for: the blue line above Run the open file names one Clash Reports folder beside the NWF
34. Leave the clash XML box empty
35. Press Run the open file and wait for it to finish
36. Open the NWF's folder in Explorer
37. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
38. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

39. Stay on the run from step 35
40. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
41. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

42. Stay in the same NWF folder in Explorer
43. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the press in step 35
44. Open that log in Notepad
45. Look for: a GROUP started line and a GROUP finished line naming the NWF
46. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
47. Look for: a RESULT block at the end with groups done, partial and failed
48. Look for: the RESULT block counts one group, not zero

## Send the logs

49. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
50. Copy the four newest `run-*.log` files
51. Paste them into `steps\logs` in the repo folder
52. Rename each with the date, the building and the press, like `2026-09-08-1C07BC-first.log`, `2026-09-08-1C07BC-second.log`, `2026-09-08-1C07BC-noxml.log` and `2026-09-08-1C07BC-openfile.log`
53. Open GitHub Desktop
54. Write a summary like `logs from 1C07BC, F5 to F8 proofs`
55. Press Commit to main
56. Press Push origin
