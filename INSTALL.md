# Install

What to paste into the VS Code terminal to build and install the add-in. Nothing else
is here.

Before you start: close Navisworks Manage 2025, and pull main in GitHub Desktop. The install refuses while Navisworks runs, and says so in one line.

Sometimes Windows will not let the install move the add-in installed before out of the way, even with Navisworks closed. Then it copies that add-in beside it, writes the new files over the old ones, and says so in one line starting IN PLACE. If anything fails it puts every old file back. Step 6 still tells you whether it worked.

1. Open VS Code on `C:\Users\bader\Documents\GitHub\PAR_NWC-Federator-V2`
2. Open a terminal
3. Build

```
dotnet build ParsonsNwcFederator.sln -c Release
```

4. If that names a missing Navisworks DLL, the install has moved. Point at it and
   build again:

```
dotnet build ParsonsNwcFederator.sln -c Release -p:NavisworksPath="D:\Autodesk\Navisworks Manage 2025"
```

5. Install

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

6. It worked if the terminal's last line reads:

```
Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator.
```

Open Navisworks and look on the Tool Add-ins tab for Parsons NWC Federator.

7. If a command fails, paste the whole error into the chat and stop.
