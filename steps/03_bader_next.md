# 03 Bader next

One action per step. Do them in order.

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

## Run one building twice

12. Close Navisworks if it is open
13. Open Navisworks Manage 2025
14. Open the add-in from the ribbon
15. Check the window title carries today's commit and build time
16. Pick the NWC folder and press Scan
17. Tick one building only
18. Pick the NWF folder, the NWD folder and the clash XML
19. Press Run and wait for it to finish
20. Press Run again on the same building, same folders, same XML
21. The second run must say OPENED for the group, the sets already there, and the tests still run

## Send the log

22. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
23. Copy the two newest `run-*.log` files
24. Paste them into `steps\logs` in the repo folder
25. Rename each with the date and the building, like `2026-09-08-1C07BC-first.log` and `2026-09-08-1C07BC-second.log`
26. Open GitHub Desktop
27. Write a summary like `logs from 1C07BC twice`
28. Press Commit to main
29. Press Push origin
