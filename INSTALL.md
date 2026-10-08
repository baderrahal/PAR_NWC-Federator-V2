# Install

What to paste into the VS Code terminal to build and install the add-in. Nothing else
is here.

Before you start: close Navisworks Manage 2025, and pull main in GitHub Desktop. The install refuses while Navisworks runs, and says so in one line.

1. Open VS Code on your copy of the repo, the folder GitHub Desktop cloned PAR_NWC-Federator-V2 into
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

5. Install. If step 4 pointed at another folder, add the same `-NavisworksPath "..."` to the end of this line too

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

6. Read the exit code:

```
$LASTEXITCODE
```

7. Read the terminal's last line. The install ends one of four ways, each with its own exit
   code and last line, written once at the top of `build\install.ps1` and copied here word
   for word:

| exit | last line | what it means |
|---|---|---|
| 0 | `Start Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator.` | The new add-in is installed, every check of it passed, and no folder is left beside it |
| 1 | starts `FAILED:` | The new add-in is not installed. The line says why, where the add-in installed before is, and where the new one went |
| 2 | starts `REFUSED:` | Nothing was installed. The line says why and what a person does |
| 3 | starts `LEFT:` | The new add-in is installed and every check of it passed, and the add-in installed before is still beside it, named on that line |

   Every end but exit 0 names, each on a line of its own before the last line and starting
   `left beside the add-in:`, the folders an install left beside the add-in.

8. Exit 0, it worked. Open Navisworks and look on the Tool Add-ins tab for Parsons NWC Federator
9. Exit 3, installed but one folder left. Close Navisworks, open File Explorer, paste the
   folder the `LEFT:` line names into its address bar, go up one folder and delete that one
   folder. Whether Navisworks loads a folder whose name does not end in .bundle is UNKNOWN, so
   do this before you start Navisworks. Until it is gone every install refuses and names it.
   Then run step 5 again and look for exit 0
10. Exit 2, refused. Do what the `REFUSED:` line says, then run step 5 again. Where it names
    folders `left beside the add-in:`, close Navisworks and delete each one it tells you to
    in File Explorer the same way. Where it says which one goes back is for a person to say,
    keep the one you want and delete the others
11. A line starting `PUT BACK:` means an install was stopped part way before, the add-in
    installed before was found whole beside the place Navisworks loads from, and it was put
    back. Nothing to do, the install goes on from there
12. If the terminal was closed, the machine went off, or the install was stopped during step
    5, do not start Navisworks. Run step 5 again. It finds what was left, puts the add-in
    installed before back where it can, and names anything else
13. Exit 1, or if a command fails, paste the whole output into the chat and stop.
