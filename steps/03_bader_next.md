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

## Proof F5, one building twice

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
23. Look for: the tests still run, the CLASH lines show them running

## Proof F6, the open file report folder

24. In Navisworks open the NWF the F5 run wrote
25. Open the add-in from the ribbon
26. Go to the Clash step
27. Look for: the blue line above Run the open file names one Clash Reports folder beside the NWF
28. Leave the clash XML box empty
29. Press Run the open file and wait for it to finish
30. Open the NWF's folder in Explorer
31. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
32. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F7, the RESULT block and the log copy

33. Stay in the same NWF folder in Explorer
34. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the press in step 29
35. Open that log in Notepad
36. Look for: a GROUP started line and a GROUP finished line naming the NWF
37. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
38. Look for: a RESULT block at the end with groups done, partial and failed
39. Look for: the RESULT block counts one group, not zero

## Proof F8, the saved tests run with no XML

40. In step 28 the XML box was empty, so the press in step 29 was already this proof. Open the same log from step 35
41. Look for: a line `CLASH    source   tests saved in the document, N of them, no XML picked`, where N is the test count
42. Look for: CLASH lines showing tests run, with clash counts, after that line
43. Look for: the workbook in the Clash Reports folder has today's time on it
44. Now the scanned run with no XML. Open the add-in, pick the NWC folder, press Scan
45. Tick the same building as before, the one whose NWF now holds the tests
46. Pick the same NWF and NWD folders. Leave the clash XML box empty
47. Press Run and wait for it to finish
48. Look for: the group says OPENED
49. Look for: the same `CLASH    source   tests saved in the document` line, then tests running
50. Look for: a fresh workbook beside the NWF folder in Clash Reports

## Send the logs

51. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
52. Copy the four newest `run-*.log` files
53. Paste them into `steps\logs` in the repo folder
54. Rename each with the date, the building and the press, like `2026-09-08-1C07BC-first.log`, `2026-09-08-1C07BC-second.log`, `2026-09-08-1C07BC-openfile.log` and `2026-09-08-1C07BC-noxml.log`
55. Open GitHub Desktop
56. Write a summary like `logs from 1C07BC, F5 F6 F7 F8 proofs`
57. Press Commit to main
58. Press Push origin
