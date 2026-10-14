# NWC and NWF design, the research of 14 Oct 2026

Saved from Bader's brief of round 4, task 6, in the same words. Part 2 of round 4 (F181 to F185) follows it.

REFERENCE (research of 14 Oct 2026). Each point is PROVEN (a source says so), INFERRED, or UNKNOWN. Much of the evidence comes from Autodesk forum users and not Autodesk staff.

## A. NWC and NWF logic

- PROVEN. The API has Document.AppendFile, AppendFiles, MergeFile, MergeFiles and UpdateFiles. UpdateFiles "updates loaded files to match files on disk" and is the API form of the Refresh button (F5). For discipline NWC files use Append. Merge is for combining NWF projects and ignores duplicates.
- PROVEN. An NWF reloads its linked files when it opens. Changes come through as long as the NWC has the same name and the same location. UpdateFiles reloads ALL files and is slow on big groups. No API refreshes a single file (UNKNOWN whether one exists).
- PROVEN. File lock: if the NWF holds the NWC open, the new NWC cannot be written ("Can't open ... nwc for writing"). The fix is Options, Model, Performance, "Close NWC/NWD files on load" (Autodesk Support article).
- PROVEN. Clash tests, search sets and saved viewpoints are stored in the NWF, not in the NWC, so they stay after a refresh. A test shows a warning icon after the model changes, and the results stay until the test is run again.
- PROVEN by community. Deleting an NWC from the tree and re-appending it loses everything: clashes go back to New and search sets and selection sets must be redone. Moving a file and repathing is reported to lose it in one thread and to keep it in another. Test repathing before relying on it.
- PROVEN. Selection sets are static and lose their geometry when the source is updated. Search sets keep the criteria. Use search sets only.
- PROVEN by community. Item IDs: if an exporter makes new unique IDs on every export, Navisworks sees different objects. Every reviewed clash becomes Resolved and a new copy appears as New. Revit NWC exports keep stable IDs. Some other tools do not.
- PROVEN by community. If an element changes class and leaves a search set, its clash disappears from the test.
- UNKNOWN. Units, shared coordinates and origin after a refresh. INFERRED: per-file units and transform set in the NWF stay, but a changed origin in the source NWC moves the geometry. Test it.
- PROVEN. A missing or renamed NWC gives a "Can't resolve external reference" dialog. Pointing to the new file keeps the item in the tree and keeps the clash results. The API has Application.FileResolving and FileInteractiveResolving events, and Document.TryOpenFile returns false on failure.
- INFERRED. Navisworks does not report which NWC changed. Keep a manifest (path, size, LastWriteTimeUtc, SHA-256) next to the work and compare before the refresh.
- INFERRED. Keep fixed NWC names with no revision numbers, one NWF per building with a stable name. Use UNC paths when many people share the NWF.

## B. Overwriting clash data safely

- PROVEN (Autodesk help, Results tab). New is a clash found for the first time in this run. Active was found before and not resolved. Reviewed was marked by someone. Approved stays Approved if the same clash is found again. Resolved was found before and not in this run. If a Resolved clash is found again it becomes New.
- INFERRED. Comments and Assigned To are not documented as kept, but they stay because the run reuses the same result object when the item pair still clashes. UNKNOWN: an official rule, and any API named ResultsReconciliation. So snapshot status, assigned to, comments, approver and group before each run.
- PROVEN. Run in place with DocumentClash.TestsData.TestsRunTest(test) on a test taken from TestsData.Tests. A test object not from the document throws "Argument is not in Clash Detective".
- PROVEN. To change a test without losing results: copy = (ClashTest)test.CreateCopy(), edit copy.Tolerance or copy.SelectionA/B, then TestsEditTestFromCopy(test, copy). Autodesk says this avoids replacing all contained results. Do not use TestsReplaceWithCopy, or TestsRemove plus TestsAddCopy, on tests with results. A copied test made every result Resolved in one report.
- PROVEN. Reset clears the results of a test. Compact deletes all Resolved clashes. Compact All does this for every test. Never call them from the add-in.
- INFERRED. If the tolerance goes down, pairs below it are no longer found and turn Resolved.
- Known limits. Grouping results through the API needs a per-clash copy-then-remove because of a GUID uniqueness limit (community). The .NET API cannot add clash rules (COM needed) and cannot remove a clash result's own viewpoint (wish NW-54140). UNKNOWN: duplicate test GUIDs and TestsData copy issues.

## C. Viewpoints

- PROVEN. SavedViewpoint.Guid returns zeros. Autodesk engineering said the full path (folder and name) is the unique key.
- Design (PROVEN APIs, INFERRED design). One top FolderItem with a reserved name "NWC Federator (auto)". A marker comment added with DocumentSavedViewpoints.AddComment. To delete, find the folder by name and marker and call DocumentSavedViewpoints.Remove(rootFolder). Rebuild with AddCopy or InsertCopy. Never delete outside it. Changed by Bader on 15 Oct 2026: everything outside the tool folder and the Comments folder is deleted (F191). The folder named Comments is kept with all inside it, and an empty one is made where there is none.
- INFERRED. A clash result keeps its own viewpoint inside Clash Detective, so deleting saved viewpoints should not change results. No Autodesk page says so. Test it once.

## D. Pictures and the GDI problem

- PROVEN absence. No Autodesk page or forum post names a GDI handle leak in GenerateImage or the clash image calls for 2019 to 2025. "TestsImageForResult" was not found as a public API (UNKNOWN).
- PROVEN. Related reports: after a few thousand images the process builds an ogsN\N_TextureCache folder of .dds files and fills the disk (about 11 GB of process memory). A workaround deletes that folder at a fixed size (5 GB). One report of an export that stops after about 110 of 600 images, with sample code that did not dispose the Bitmap.
- PROVEN. Windows: a default limit of 10,000 GDI handles per process, a theoretical limit of 65,536 per session. GDIProcessHandleQuota can be set between 256 and 65,536 but needs admin rights and a restart and does not add kernel memory.
- Workarounds ranked. 1 dispose every Bitmap (using). 2 read the GDI count after each picture and stop before 9,000. 3 work in batches of 500 to 1,000 and save the NWF between batches. 4 clear the OGS texture cache. 5 the registry quota only as a last resort.

## E. Interface

- PROVEN. The supported pattern for a pane is a DockPanePlugin with an ElementHost holding a WPF UserControl.
- INFERRED. A tab control with "NWC" first and "NWF" next to it. NWC page for the first build. NWF page for the weekly work: the NWF picker, the changed list, Refresh, Run, Pictures, Viewpoints, Reports. A confirmation before a refresh with Cancel as default and an automatic NWF backup. A table of before and after counts after the run.

## F. Risks

- Exporters that make new IDs turn reviewed clashes into Resolved plus New (PROVEN).
- Elements that leave a search set vanish from results (PROVEN by community).
- Someone deletes and re-appends a model by hand and all history is lost (PROVEN by community).
- UpdateFiles reloads every file, so it is slow on big groups (PROVEN).
- NWC locks block the overwrite (PROVEN).
- Picture runs fail near 10,000 GDI objects or from disk cache growth (limit PROVEN, Navisworks cause UNKNOWN).
- A coordinator stores a viewpoint inside the tool folder and it is deleted (INFERRED).
- Units or origin drift after re-export (UNKNOWN).
