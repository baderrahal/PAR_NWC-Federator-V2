# 03 Bader next

**The last run, 2026-09-07 09:34, was on the old build be0b9b37 of 1 Sep. Every fix from F5 to F70 has merged since. THE ADD-IN BUILDS as of 2026-09-19, proved by F69 with 0 errors and 0 warnings, where before that nothing anywhere had ever compiled it. Pull main and build before anything.**

One action per step. Do them in order. One build, one install and one Navisworks session cover every proof: the window checks first, then the C06 rebuild run, then one building twice, then the rest.

F34 comes first, because it is the largest add-in change of the round, the window, the XAML, the engine and the job all changed, and it is proved by opening the window and looking, before any run. If the window does not open or the Clash step is wrong, nothing after it can be read, so it is checked before a single group runs. F33 is checked in the same look, because it is one combo on the same window. F27 is next, because it costs one Scan and no run at all: its block is read off the 1B06PH run further down. Then the C06 run, which proves F24, F29, F30 and F33 in one press, then F28 and F31 together, because both are one building run twice. F32 and F35 need a different file open or a folder made for them, so they come after the ordinary runs.

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

10. Look for: `Build succeeded.` with `0 Error(s)` and `0 Warning(s)`. That was proved on this machine on 2026-09-19 by F69, which is the first build this repo has ever had, so anything else here is new and the whole output goes into the chat. `CreateCopy` and `CopyFrom` on `DocumentSelectionSets`, which F24 and F29 could not measure, are measured now and written into `docs\history\scan.md` 4d, so a build naming either of them is a real fault and no longer an open question
11. Look for: the build stamp. If it reads `nogit` where an eight character commit hash belongs, git is not on the PATH for the terminal that ran the build. The build itself is fine and only the stamp is affected, and the title bar and the log will read `nogit` until you build from a terminal that has git on it

### If the build fails on NuGet rather than on the code

This is the failure Bader hit on 2026-09-19 before he ever reached a compiler error, and it has nothing to do with the code:

```
C:\Program Files\dotnet\sdk\10.0.400\NuGet.targets(198,5): error Cannot create a file when that file already exists.
```

`dotnet build` restores all three projects at once, they collide on the same package folder, and it is a known race in NuGet's restore task. The four steps below go cheapest first.

12. Run the same build command again, exactly as it was:

```
dotnet build ParsonsNwcFederator.sln -c Release
```

13. Look for: the second build finishes with no error. The race is intermittent, so repeating the build is the first thing to try and it costs nothing
14. If the same error comes back, delete every `obj` and `bin` folder, because a half written package folder is what the race leaves behind:

```
Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory | Remove-Item -Recurse -Force
```

15. Look for: the command prints nothing. Run `Get-ChildItem -Path src,tests -Include obj,bin -Recurse -Directory` on its own afterwards and look for: it prints nothing either. There are six of these folders when they are all there, two per project
16. Restore on its own, one project at a time, which is what takes the race out:

```
dotnet restore ParsonsNwcFederator.sln --disable-parallel
```

17. Look for: three `Restored` lines and no error, one for `Federator.Core`, one for `Federator.Core.Tests` and one for `Federator.Addin`. If the `obj` folders were not deleted it says `All projects are up-to-date for restore` instead, which is also fine
18. Build without restoring again, so nothing can collide:

```
dotnet build ParsonsNwcFederator.sln -c Release --no-restore
```

19. Look for: the build finishes with no error. If it says a package or a reference assembly is missing, the restore at step 16 did not finish and that is the step to read again
20. Only if every step above failed, clear the NuGet cache. It is LAST because it re-downloads every package this solution uses, and nuget.org is a hard requirement for building at all:

```
dotnet nuget locals all --clear
```

21. Look for: it finishes with no error. Exactly what it prints was not measured here, because clearing the cache on this machine would cost the re-download it warns about. Then go back to step 8 and build again, which will pull every package down fresh

22. Install:

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

23. Read the last lines of the install. Look for: a line saying all the expected files are present and a line saying every reference is satisfied. If it stops with Install incomplete, copy the whole output into the chat

## Proof F34, the window, and F33, the units combo

24. Close Navisworks if it is open
25. Open Navisworks Manage 2025 with nothing open
26. Open the add-in from the ribbon
27. Look for: the window title carries today's commit and build time, not be0b9b37
28. Look for: the window has four tabs, Source, Grouping, Outputs, Clash
29. Go to the Outputs step
30. Look for: the Model units combo lists `Metres, which is what the models are set to`, then Millimetres, Centimetres, Feet, Inches, in that order, with the first chosen
31. Look for: the grey line under it says the report is always in metres
32. Go to the Clash step
33. Look for: there is no Outstanding counts combo on the Clash step. It used to sit under the tick boxes
34. Look for: the two buttons read `Sets into open model` and `Tests into open model`, under the line `Not part of the run. Try the file above against the model open right now:`
35. Pick the clash XML, 1104-PAR_CLASH_AllInOne
36. Look for: the line under the file box ends with `Every locator resolves against its sets.`
37. Look for: the log pane holds a block headed `HEALTH 1104-PAR_CLASH_AllInOne.xml` with `Tests: 1830`, `Sets: 61` and a `Locators resolved` line whose two numbers are equal
38. Clear the clash XML box

## Proof F27, the GROUPS block tells the truth

39. Go to the Source step, pick the same C06 NWC folder as last time and press Scan
40. Look for: the count line under Scan reads files found, files readable, groups and the findings by kind, then the findings themselves
41. Go to the Outputs step and pick the same NWF folder and NWD folder as last time
42. Go to the Grouping step
43. Untick two groups, 1B06PH and 1B06G1, which is the only thing in this tool that drops a group from a run
44. Do not press Run here. The GROUPS block is written when a run actually STARTS, so a Run followed by a Cancel leaves nothing to read, and the preview behind that Run opens each NWF and leaves the last one open, which would stop the next proof counting the rebuilt groups
45. Tick 1B06PH and 1B06G1 again, so the next proof runs every group
46. Look for, F27, later: the GROUPS block of the 1B06PH run at step 76 lists 1B06PH with `run`, the other thirteen with `unticked`, and the word `skipped` nowhere
47. Look for, F27, in the same block: one line after the list reads `13 groups unticked in the Run column, nothing else drops a group`
48. Go on to the next proof with every group ticked and nothing open in Navisworks

## Proof F24, the six CHANGED groups are rebuilt from the scan, with F29 and F30 in the same run

49. Leave the clash XML box empty
50. Tick every group and press Run
51. Look for: the label at the bottom reads Checking NWF 1 of 14 and counts up, then the confirm dialog opens
52. Look for: the dialog says `Rebuilt: 6` and that the NWF is cleared and rebuilt from the scan folder with its saved tests kept
53. Press Cancel
54. Look for: the log says `Run cancelled before anything was cleared.`
55. Go to the Grouping step
56. Look for: 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE read Rebuilt in the Run as column, and the other eight read Weekly run
57. Press Run again, then OK, and wait for it to finish. The confirm dialog now also says the NWF the preview opened will be discarded, which is right, and the preview does not run a second time for the same reason
58. Look for, F50: per rebuilt group the log has a CHANGED line with the counts, then a REBUILT block, `REBUILT <nwf>  1 added, 4 moved, 0 removed` for 1B06BC, then one line per file, then FOUR lines together, starting `SETS`, `TESTS`, `VIEWS` and `RESULTS`. Before F50 there were two and they read in two different shapes
59. Look for, F50: each of those four says `kept` or `none`, and never `LOST` and never `NOT COUNTED`. Any one of them saying either means the NWF on disk was left alone on purpose, which the line after them says in words
60. Look for, F29 and F50: the `SETS` line reads one of two shapes and BOTH are right. Where the clear kept the sets it carries TWO numbers, `kept: before clear N, after appends N, nothing to put back`, which is the ordinary case. Where the count dropped and the copy was put back it carries THREE, `kept: before clear N, after appends N, after restore N, put back from the copy`, and then the first and the third have to agree. The other three lines read the same way, because four things reading four ways is how a log stops being read. A line reading `LOST` or `NOT COUNTED` is the fault, and step 59 is where that is checked
61. Look for, F33: per group a `UNITS` line saying how many models were set and what the document shows, in the words `model set` or `models set`, and no line anywhere says DID NOT FOLLOW
62. Look for, F30 and F52: per group, after the models are in, the blocks come in this order: UNITS, then CLASH, then `ITEM IDS` where the report carries item ids, then `VIEWS`, then `NWF      attempt`, then XLSX, then NWD, then the final NWF line saying intact and the size. The `VIEWS` line sits between the clash step and the NWF because F52 makes the viewpoints there, so a viewpoint this run made is inside the file the NWD is published from. No SETS block follows UNITS on this run, because sets come from the clash XML and the box is empty. The four SETS, TESTS, VIEWS and RESULTS lines the rebuilt groups carry are written during the rebuild, before UNITS
63. Look for: the GROUP finished line of each of the six reads the building, then `DONE`, then the seconds, and ends with `Rebuilt`
64. Look for: the RESULT block carries `rebuilt        : 6` and `weekly run     : 8`
65. In Explorer open the NWF folder and open the NWF of 1B06BC in Navisworks
66. Look for: the Selection Tree lists 5 models, EL among them, all from the Published folder
67. Look for, F29: the Sets window holds the same sets as it did before the run, Mechanical with its four subfolders among them
68. Open the NWD of 1B06BC from the NWD folder
69. Look for: the same 5 models
70. Look for: no group in this run reads PARTIAL
71. Close the NWD and the NWF so nothing is open

## Proof F28 and F31, one building twice with the XML

72. Go to the Clash step and pick the clash XML
73. Go to the Grouping step and tick 1B06PH only
74. Press Run
75. Look for: the confirm dialog says `Weekly run plus XML: 1` and its line ends with `Nothing is cleared.`
76. Press OK and wait for it to finish
77. Note the time the run took, from the GROUP started line to the GROUP finished line
78. Look for: the log says OPENED for the group
79. Look for: the SETS block says how many sets were created and `already there     : 61, left alone, not copied again` if the NWF already held them from last week, or `sets created      : 61` if it did not
80. Look for: the line `CLASH    source   tests from XML` and the tests running after it
81. Press Run again on the same building, same folders, same XML
82. Press OK and wait for it to finish
83. Look for, F28: the SETS block reads `sets created      : 0` and `already there     : 61, left alone, not copied again`, and the line after it reads `put into the document: 0 created, 61 already there and left alone`
84. Look for, F28: the SETS block's put into the document line reads `0 created, 61 already there and left alone`. A second `NWF      attempt` line still follows the CLASH block, because the tests ran and their results went into the document
85. Look for, F31: the DRIFT block reports no locator difference on any test. A locator difference on every test would mean the source held since the sets were indexed does not compare equal to a fresh one, which is the one thing F31 could not measure here
86. Look for, F31: the CLASH block shows the same created, already there, run, skipped and passed counts as the first run
87. Look for, F31: the second run took no longer than the first

## Proof F35, a group of one discipline creates every test and runs none

88. In Explorer make a folder on the desktop named OneDiscipline
89. Copy the AR NWC of 1B06PH into it
90. Copy it again inside that folder and rename the copy so its number reads 000002 instead of 000001, so the folder holds two AR files of one building
91. In the add-in go to the Source step, pick the OneDiscipline folder and press Scan
92. Look for: the findings say SINGLE DISCIPLINE for 1B06PH
93. Go to the Grouping step
94. Look for: the status of 1B06PH reads `Ready. One discipline, so every test is created and none is run.`
95. Go to the Outputs step and pick a new empty folder on the desktop for the NWF and the NWD, so nothing under C06 is touched
96. Leave the clash XML picked and press Run, then OK, and wait for it to finish
97. Look for: the log line `CLASH    1B06PH holds one discipline, so every test is created and none is run. One discipline cannot clash with itself.`
98. Look for: the CLASH block says 1830 created and 1830 skipped, with the skip reason counted as one discipline and not as an empty side
99. Look for: the GROUP finished line reads the building, then `DONE`, then the seconds, then which path it took. `DONE` is never the last word on the line
100. Put the Source folder back to the C06 NWC folder and press Scan
101. Put the NWF folder and the NWD folder back to the C06 ones

## Proof F26, the report is always in metres

This one is its own small run, with the clash XML, because the conversion only happens where a report is written.

102. Go to the Outputs step
103. Set the Model units combo to Feet, on purpose, so the models are put into feet
104. Go to the Grouping step and tick 1B06PH only
105. Press Run, then OK, and wait for it to finish
106. Look for: the line `model units      : Feet, which is what the models are set to`
107. Look for: the line `report units     : Meters (m), always, converted before anything is written`
108. Look for: one line `UNITS    every number converted from ft to m, one ft is 0.3048 m, ...` before the XLSX line
109. Open the workbook in the Clash Reports folder beside the NWF folder
110. Look for: the Tolerance cell of the first block reads `0.075m`, not `0.246ft`
111. Look for: the Distance column holds metres, so a hard clash reads about -0.050, not -0.164
112. In Navisworks open Clash Detective and select that test
113. In Options, Interface, Display Units, set Linear Units to Metres
114. Look for: the panel tolerance reads 0.075 and one clash distance matches its workbook row to three decimals
115. Go back to the Outputs step and set Model units to Metres

## Proof F22, the labels before a run

116. Go to the Outputs step and pick an NWF folder that holds no NWF yet
117. Go back to the Grouping step
118. Look for: every group reads First run
119. Go to the Clash step and pick the clash XML
120. Look for: every group still reads First run, because no NWF is there yet
121. Put the NWF folder back to the C06 one
122. Look for: every group reads Weekly run plus XML

## Proof F71, the window says when an NWF is nearly matched

This is what the run of 2026-09-19 could not tell you. The tool reuses an NWF that is at
the output path and builds one when nothing is there, which is right, and it said nothing
about a file sitting beside it under a nearly identical name.

123. Go to the Outputs step and pick the C06 NWF folder, the one that already holds NWF files
124. Go to the Grouping step and look at the NWF Name column
125. Change the Level field on the Outputs step from `ZZZ` to `L01`, so every name a group would write now differs from the file on disk in one field only
126. Go back to the Grouping step
127. Look for, F71: every group whose NWF is on disk now reads `First run` in the Run as column, FOLLOWED BY `, but the NWF folder holds a similar name:` and the name of the file that is really there. Before F71 it read `First run` and nothing else, which is what cost the time
128. Look for, F71: one grey line under the group table reading `N groups are set to build a new NWF beside a file whose name is nearly the same.` and then `Type the NWF Name cell over to point at the existing file, or leave it to build a new one.` The number is how many groups are in that state
129. Look for, F71: nothing was changed for you. The Run column is still ticked, the NWF Name cells still hold the new name, and no name was corrected. The tool reports and you decide
130. Type the NWF Name cell of one group over with the name of the file that is really there, without its extension
131. Look for, F71: that row now reads `Weekly run` or `Weekly run plus XML` with no sentence after it, and the line under the table counts one group fewer
132. Put the Level field back to `ZZZ`
133. Look for, F71: every row reads `Weekly run` or `Weekly run plus XML` again and the line under the table is gone
134. Change the Level field to `L01` again and then pick an NWF folder that holds NO NWF at all
135. Look for, F71: every row reads `First run` with no sentence after it, and no line under the table. An empty folder holds nothing to be near, so there is nothing to say
136. Put the NWF folder back to the C06 one and the Level field back to `ZZZ`

## Proof F9, a changed folder is rebuilt and not run in feet

137. Copy one NWC of 1B06PH out of the NWC folder to the desktop, so the folder lost one file
138. Close whatever is open in Navisworks, so the preview can open each NWF, then press Scan again and tick 1B06PH only
139. Press Run
140. Look for: the dialog says `Rebuilt: 1`
141. Press OK
142. Look for: the REBUILT block names the removed file with `removed`
143. Look for: no UNITS line comes before the REBUILT block
144. Copy the NWC back into the NWC folder
145. Press Scan again, tick 1B06PH, press Run, press OK, so the NWF holds all its files again
146. Look for: the REBUILT block names the file with `added`

## Proof F10, each output on its own switch

147. Go to the Outputs step and open More
148. Tick Write a clash XML beside each workbook
149. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
150. Tick 1B06PH and press Run, then OK
151. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
152. Look for: `IMAGES   skipped  images are switched off for this run`
153. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
154. Untick Write a clash XML and tick New, Active and Reviewed again
155. Press Run again, then OK
156. Look for: `XML      skipped  not wanted this run`
157. Look for: the pictures rendered again, counted on their own `IMAGES` line after the CLASH block. The CLASH block counts clashes and never pictures

## Proof F17, the pictures are numbered in the export order

158. Stay on the run just finished
159. Look for: one line `IMAGES   numbered in report order: <n> renamed, <n> already right, 0 missing` after the CLASH block
160. Open the Clash Reports folder beside the NWF folder and open the 1B06PH workbook in Excel
161. In Navisworks open Clash Detective and select the test that is the first block of the workbook, the one with the most clashes
162. Look for: the clashes on the sheet are in the same order the Clash Detective panel lists them
163. Look for: the Image cell on row 1 of that block links to cd000001.jpg, row 2 to cd000002.jpg, and so on
164. Look for: the second block's pictures start at cd010001.jpg
165. Click the Image cell on three rows in three different blocks
166. Look for: every link opens its picture, and the picture shows the two items named on that row
167. In Explorer open the `_files` folder beside the workbook and look for: no file ending `.moving`

## Proof F8 on the scanned run, no XML

168. Clear the clash XML box on the Clash step
169. Go to the Grouping step
170. Look for: 1B06PH reads Weekly run
171. Press Run, then OK
172. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
173. Look for: the CLASH block says the tests ran and the workbook was written
174. Look for: no SETS block, because no XML means the sets are left alone

## Proof F32, an NWD open is refused

175. In Navisworks open the NWD of 1B06PH from the NWD folder
176. Open the add-in from the ribbon and go to the Clash step
177. Look for: the line above Run the open file reads `This document is a .nwd file and this tool runs an NWF, which is where the clash tests and their results live. Open the NWF instead.` and the Run the open file button is greyed
178. Close the NWD

## Proof F6, the open file report folder, with F30 and F32 on the NWF

179. In Navisworks open the NWF of 1B06PH
180. Open the add-in from the ribbon
181. Go to the Clash step
182. Look for, F32: the blue line above Run the open file starts with `Weekly run.`, names the NWD it will write beside the NWF and names one Clash Reports folder beside the NWF, and the button is enabled
183. Leave the clash XML box empty
184. Press Run the open file and wait for it to finish
185. Look for, F30: after the models are in, the blocks come in the same order as on the scanned run: UNITS, then CLASH, then `NWF      attempt`, then XLSX, then NWD, then the final NWF line
186. Open the NWF's folder in Explorer
187. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
188. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

189. Stay on the run just finished
190. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
191. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

192. Stay in the same NWF folder in Explorer
193. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the Run the open file press
194. Open that log in Notepad
195. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
196. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
197. Look for: a RESULT block at the end with groups done, partial and failed
198. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

199. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
200. Copy every `run-*.log` written today
201. Paste them into `steps\logs` in the repo folder
202. Rename each with the DATE OF THE RUN, the building and the press. The shape, with 2026-09-14 standing for whatever day you run them: `2026-09-14-C06-rebuilt.log`, `2026-09-14-1B06PH-first.log`, `2026-09-14-1B06PH-second.log`, `2026-09-14-onediscipline.log`, `2026-09-14-1B06PH-noxml.log` and `2026-09-14-1B06PH-openfile.log`
203. Open GitHub Desktop
204. Press New Branch, name it `logs-` and the date of the run, so `logs-2026-09-14` on the fourteenth, and press Create Branch
205. Write a summary like `logs from C06, F34 to F7 proofs`
206. Press Commit
207. Press Publish branch
208. Press Create Pull Request, and tell the worker in the chat that it is open. The worker merges it and reads the logs. Nothing goes to main directly, that is the wall F38 put up

## Run the window probes

209. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-defaults.ps1
```

210. Look for: it prints the window defaults and no error about a path
211. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-labels.ps1
```

212. Look for: it says how many tick boxes are visible without opening anything, which must be 2 since F72, and reports no label over eight words and no help line over twelve. It was 1 before F72 added `Mark penetrations as Reviewed` to the Clash step, which is visible on purpose because a weekly run genuinely has to choose whether this run writes statuses into the NWF

## Proof F41, the handles, off the two runs above

213. Before you press Run the second time in step 81, open Task Manager, the Details tab, right click a column heading, Select columns, and tick Handles
214. Look for: the handle count on the Navisworks row at the end of the second run, beside the one at the end of the first. Copy both into the chat. F41 releases every wrapper this tool creates where it is finished with it instead of leaving it to a finalizer, so a second run over the same 61 sets and 1830 tests should not sit higher than the first. A count that climbs run on run is the thing worth reporting
215. Look for, in the log of either run: no line saying a set was not at the index the count gave before the add. That line is written only when the tree is not the shape the fast read assumes, and it means the set was then looked for by name

## Proof F45, the clash step keeps its rules, off the two runs above

216. Look for, in the DRIFT block of either run: either no line about sides left out at all, or one saying how many sides were left out because which set one end points at could not be read. That line is new and says not compared is not the same as matching
217. Look for: a block headed `ITEM IDS 1B06PH`, one line per property that supplied an item id with a count out of the total, and never a line per item. Each line ends `written as "Element ID"`. Expect `Id` to supply most of them, because `Id` is the first name looked for and `Element ID` is the label this tool chooses
218. Look for, if any `CLASH    OLD` line appears: it reads the test name and `Navisworks has this test marked Old.` and nothing after that. It used to carry a sentence about what Old means, which is UNKNOWN
219. Look for, only if you tick compacting on the Clash step: the compacted line carries both numbers, the Resolved count before and the count read again after, and the number reported as removed is the difference between them

## Two probes, F50 and F52. BOTH ALREADY RUN on 2026-09-19

Both were run on this machine on 2026-09-19 and both answers are in
`docs\history\scan.md`, 5c and 5d, with the assembly version and the date. Sections 5a and
5b still hold the questions, because the reasoning in them is why the answers matter.

Nothing below is outstanding. The four steps are kept so the answers can be checked rather
than trusted, and because the probes are the right thing to rerun when Navisworks is
upgraded. Each takes about ten seconds, neither needs the add-in built and neither opens a
model.

220. Open PowerShell in the repo folder and run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-model-remove.ps1
```

221. Look for, under `THE QUESTION: anything anywhere that takes a Model or an index and removes it`: two members on `Autodesk.Navisworks.Api.Document`, `public Void RemoveFile(Int32 index)` and `public Boolean TryRemoveFile(Int32 index)`. So a model CAN be taken out without a clear, and the member is on `Document` and not on `DocumentModels`, which is why searching the collection for it found nothing. `DocumentModels` carries only `InternalRemove` and `InternalRemoveAt`. If this list is now EMPTY, the install has changed and `scan.md` 5c is wrong
222. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-viewpoints.ps1
```

223. Look for: a block headed `The viewpoint collection, every member` listing `Autodesk.Navisworks.Api.DocumentParts.DocumentSavedViewpoints`. It must NOT say `UNKNOWN: no type named ...DocumentSavedViewpoints`, and on 2026-09-19 it did not, so the name the code uses is right
224. Look for, in the same output: `public FolderItem RootItem`, `public Void AddCopy(GroupItem parent, SavedItem item)` and `public Void EditDisplayName(SavedItem item, String newDisplayName)`. All three were there, so a folder is made, a viewpoint goes in it and a name can be set. `SavedViewpoint` reads `IDisposable: True` and has a `SavedViewpoint(Viewpoint viewpoint)` constructor. The sets collection printed beside it carries the same members with the same signatures, which is the comparison the probe exists to make
225. Look for, at the end, under `How a viewpoint could be made to show one discipline and hide the rest`: `DocumentModels.SetHidden` and `SavedViewpoint.GetVisibilityOverrides`. That is the ONE thing still UNKNOWN. Items are hidden through `SetHidden`, and whether a viewpoint saved while they are hidden RECORDS that hiding cannot be read off a DLL. The answer is `SavedViewpoint.ContainsVisibilityOverrides` read on a real run, once the writing half of F52 is built. `SavedViewpoints.CanBuild` is still false and that is a decision waiting, not a measurement

## Proof F51, the NWD carries May be re-saved and its properties reach ACC

Every NWD this tool has published so far shows a processing error beside it in ACC and
in Forma. Autodesk say an NWD published without May be re-saved cannot be translated by
that viewer, and they have a second article about an NWD reaching ACC with no properties
and every object showing as solid. F51 sets three things on the publish that were never
set: `AllowResave` true, `EmbedDatabaseProperties` true and `PreventObjectPropertyExport`
false. All three were already on the list read off the DLL on 2026-08-29.

This proof needs one NWD and an upload. It is the only thing in this file that needs a
browser as well as Navisworks.

226. Run any one building the ordinary way, so a fresh NWD is published
227. Look for: one line in the log reading `NWD      publish properties set:` and then seven names, ending `AllowResave=true, EmbedDatabaseProperties=true, PreventObjectPropertyExport=false`. If that line is missing, the build is older than F51 and nothing below proves anything
228. Open the published NWD in Navisworks on your own machine, then File, then look at the file properties
229. Look for: the NWD opens and the title, the publisher, the subject and the author read as the log line says they were set
230. Upload that NWD to ACC, into whatever folder the project uses
231. Look for: no processing error and no warning triangle beside it once the translation finishes. That is the whole point of `AllowResave`. A warning still there means the flag did not take and the answer is a screenshot of the warning and the log line from step 227
232. Open the NWD in the ACC viewer and click one object, any object
233. Look for: a properties panel with real Revit properties in it, not one row reading Solid. Object properties missing means `EmbedDatabaseProperties` did not carry them and `PreventObjectPropertyExport` is the next thing to look at
234. Q32 is whether the NWF needs to be in ACC at all. Nothing appears beside an NWF because ACC does not translate one: an NWF holds no geometry, only pointers to the NWCs, so there is no viewable file to make. Answer it in `steps/02_questions.md` when you have seen the NWD work

## Proof F52, a viewpoint per discipline

Two halves. The first is true today. The second no longer waits on a probe, because the
probe was run on 2026-09-19 and the API is measured in `docs\history\scan.md` 5d. It waits
on the WRITING half of F52 being built against what was measured, which is a decision and
not a measurement, so `SavedViewpoints.CanBuild` is still false. Do the first on the next
ordinary run and the second once that half exists and the add-in has been rebuilt.

235. Look for, on any run since F52: a `VIEWS` line per group reading `N viewpoints planned:` and then the paths. The disciplines come SORTED, so a group of AR, ME and EL reads AR, EL, ME and plans FIVE and not three, `AR/AR only, EL/EL only, EL/Over 150mm/EL over 150mm, ME/ME only, ME/Over 150mm/ME over 150mm`, because F53 gives Mechanical and Electrical a sub group for their large items and Architecture has no pipe in it
236. Look for: a group with only one discipline STILL gets a line, never none. One AR group plans one viewpoint and one ME group plans two, its own and its large items sub group. A single discipline group planning NONE is the fault to watch for, and the log line is the evidence
237. Look for, while `SavedViewpoints.CanBuild` is false: the line under it reading `VIEWS    not attempted.` then saying the API is measured, naming `docs\history\scan.md` 5d, and saying the one thing still UNKNOWN is whether a saved viewpoint records hidden state. That is the honest case and not a failure. No group should be marked down for it, so check the GROUP finished lines still read `DONE`
238. Once the writing half of F52 is built against `docs\history\scan.md` 5d and the add-in rebuilt, run one building again
239. Look for: a `VIEWS` block in the shape of the SETS block, one `VIEW` line per viewpoint then `views created     :` and `put into the document:`
240. Open the NWF in Navisworks and open the Saved Viewpoints panel
241. Look for: one folder per discipline, named with the discipline code. AR holds one viewpoint. ME and EL each hold one viewpoint and one sub folder named `Over 150mm` holding one more. Press the plain one and look for that discipline showing and the others hidden
242. Run the SAME building again without changing anything
243. Look for, F28's rule carried to viewpoints: every line reads `already there, left alone, not made again` and `views created     : 0`. A second copy of any viewpoint is a fault

## Proof F53, the 150 mm rule and the sub groups

This one is proved in Core here and in the tree on your machine. The Core half needs
nothing from you. The rest waits on the writing half of F52, because the sub group IS a
viewpoint and nothing builds one yet. The API it needs is measured, scan.md 5d.

244. Look for, on any run since F53: the `VIEWS` planned line now carries a sub group path per Mechanical and per Electrical group, reading `ME/Over 150mm/ME over 150mm`. A group with no ME or EL in it has none, and that is right
245. Look for: the number in the folder name is the threshold in use. If you change the threshold and it still reads `Over 150mm`, that is a fault and the folder and the rule have drifted apart
246. Once the writing half of F52 is built and the add-in rebuilt, run one Mechanical building
247. Look for: a `SIZE` block per group with three numbers, `over 150mm`, `not over`, and `size could not be read`
248. Look for, and this is the one most likely to be wrong: the line reading `N items are in the viewpoint because no size could be read off them. Nothing was dropped.` followed by every one of them named. That number is EXPECTED to be large, because a fitting usually carries no size property. A large number here is not a fault
249. Read a few of the names under it. If they are all fittings, elbows and tees, the rule is behaving. If real straight pipes are in that list, the size is on a property this tool is not looking for, and the answer is the property name so it can be added to the setting
250. Open the NWF and press the `ME over 150mm` viewpoint
251. Look for: large pipes, ducts and trays showing, small ones hidden, and the fittings still there. A fitting missing means the include on unknown rule is not working and the SIZE block is the evidence
252. Look for, on a document measured in FEET rather than millimetres: the same items in and out as the same building measured in millimetres. The conversion is what makes that true, and a difference means the raw number is being compared somewhere

## Proof F54 and F72, penetrations become Reviewed

Q33 is ANSWERED since 2026-09-19 and F72 supplies the list, so this section has two
halves now. The first is the run with the box OFF, which is the default and which has to
change nothing at all. The second is the run with it on.

253. Run any building the ordinary way, with the box on the Clash step left OFF
254. Look for: NO `STATUS` line and NO `PENETRATION` block anywhere in the log. With the box off nothing supplies a list, so either of those on an ordinary run means something is supplying one and that is a fault. A REBUILT group writes a `RESULTS` line, which is F50 counting the clash results it carried across the clear and is nothing to do with this. The two used to share the STATUS prefix and F56 separated them, because one prefix reading as two different things is how a log stops being trusted
255. Look for: the clash counts per test in the workbook still read New, Active, Reviewed, Approved and Resolved as they always did, and no clash has moved
256. Look for, in the RUN SETTINGS block at the top: `penetrations     : no, every clash keeps the status it has`
257. Look for: NO `penetrations   :` line in the RESULT block. A run that never asked for it has nothing to say, and a line reading zero on every run teaches people to skip it
258. Open `docs\workflow.md` and read the section What a status means
259. Look for: it says a clash carries five and a test carries four, that Old is a test word and never a clash word, and that this tool sets Reviewed and nothing else

### Now with the box on

260. Go to the Clash step
261. Look for, F72: a tick box reading `Mark penetrations as Reviewed`, unticked, with one grey line under it reading `Services 150mm and under through walls, floors, roofs. New and Active only`. The 150 is read off the setting, so if you ever change the threshold this line changes with it
262. Look for, F72: it is NOT inside `Things that destroy data`. Nothing is destroyed, only New and Active ever move and a person moves one back in one click
263. Tick it, and run ONE building that has mechanical or electrical services in it
264. Look for, F72: a `PENETRATION` block per group, straight after the `CLASH` block
265. Look for, F72: one line per clash moved, starting `REVIEWED `, then the clash name, then the test it is in, then the service category, the size in millimetres, and the solid category. For example a `Pipes 100mm through Walls`. If that line names a category you would not call a service, the list is a setting and it is wrong for this project
266. Look for, F72: after the moved lines, `clashes looked at :` and `moved to Reviewed :`, then ONE LINE PER REASON for everything left alone, including the reasons at zero. The seven are moved, over the size, no size could be read, a person had already set it, both sides a service, both sides a solid, and not a service against a solid
267. Look for, F72: the line `the size in use   : 150mm or under is a penetration, over it stays as it was`
268. Look for, F72: the number beside `no size could be read off the service, left alone` and READ IT. That is the count of services this tool could not measure, and every one of them was LEFT AT NEW rather than moved. It goes the opposite way from F53, which INCLUDES an item it could not measure, and both are right: the safe mistake in a viewpoint is showing something unnecessary and the safe mistake here is leaving a clash for a person. If that number is large, the size is on a property this tool is not looking for, and the answer is the property name so it can be added to the setting
269. Look for, F72: one line in the RESULT block reading `penetrations   : N clashes moved to Reviewed`, and N is the sum of every group's `moved to Reviewed`
270. Look for, F72: an `NWF      attempt` line AFTER the clash step, because writing a status is a write and the NWF is saved again on it. Whether it is the first or the second such line depends on the path this group took, so count them rather than looking for a second: a First run or a Rebuilt group writes one before the clash step as well, and a Weekly run does not, because the opened branch logs `NWF      reused` instead. On a Weekly run the line after the clash step is the ONLY one, and its absence is the fault to report
271. Open the workbook for that building
272. Look for, F72: the Reviewed count on the test header rows has gone UP by the number the block said, and the New or Active count has gone down by the same. That is the count reaching the workbook, in the client's own column, because the status is applied before the harvest reads it. If the workbook shows the OLD status, the status is being applied after the harvest instead of before it, which is the one thing this feature must not do
273. In Navisworks open Clash Detective on that building and pick one clash the block named
274. Look for, F72: it reads Reviewed in the panel, and the two items are a small service and a wall, a floor or a roof
275. Run the SAME building again with the box still on
276. Look for, F72: the second run moves FEWER, probably zero, because the ones it moved last time are now at Reviewed and Reviewed is not a status this tool moves off. They are counted under `a person had already set it, left alone`, which is the same line a clash somebody set by hand would appear on
277. Find a clash you set to Approved by hand, if there is one, and run again
278. Look for, F72: it is still Approved. Only New and Active ever move, and that is a Core rule with its own tests rather than a condition in a loop

## The log round, F59 to F64. One folder, watched and then read

THE ONE THAT ANSWERS THE ROUND. Everything below this is detail, and this section on its
own is what Bader asked for on 2026-09-18: show what happened inside Navisworks, show
what is running while it runs, and show where the time went. Do this one first, with the
whole folder ticked, because a single building cannot show group N of M and cannot fill
the run timing block.

279. Run the SAME building once more with the box OFF, and note the `clash step took` seconds on both runs
280. Look for, F72: what the penetration pass COSTS, which is the one number nothing here could measure. With the box on, every clash has a category read off both sides and a size read off the service, walking up to four levels for each, so the cost is per CLASH and not per test. It is UNKNOWN until this comparison is made. If the difference is small, leave it on. If it is large, that is worth saying, because the pass reads the same properties the harvest reads a moment later and the two could be made to share one read
281. Go to the Source step, pick the C06 NWC folder and press Scan
282. Go to the Outputs step and pick the NWF folder and the NWD folder
283. Go to the Clash step and pick the clash XML
284. Go to the Grouping step and make sure EVERY group is ticked
285. Press Run, then OK, and then WATCH the window rather than leaving it
286. Look for, F62: the line just above the log box reads the group, the building, the step, the seconds on that step and the seconds on the run, and it changes as the run works. That line is the whole of what is running while it runs
287. Look for, F62: the log box below it scrolls itself to the newest line, and the line above never moves out of sight
288. When it finishes, open the log
289. Look for, F60: a block headed `TIMING` after every group, and one headed `TIMING, THE WHOLE RUN` before `RESULT`. That is where the time went, per group and then by step across every group
290. Look for, F60: the last line of the run block says whether the run fitted in forty five minutes. That is criterion 2 answered by the log itself, and it is the single most important line in the file
291. Look for, F61: `CENSUS` lines either side of every step, carrying models, sets, tests, results and views. That is what happened inside Navisworks, counted rather than assumed
292. Look for, F63: a `GAP` block at the end of every group, saying what the run measured and the report does not show
293. Send the log and the `.tsv` beside it. Everything after this section is the detail behind these five things, and it is worth doing, but if you only do one section do this one

## The log round in detail, F59 to F64

Nothing here needs a new folder or a new file. It is all read off the log of the run
above, or of any ordinary run.

294. Run any one building the ordinary way, with the clash XML picked
295. Open the log for that run
296. Look for, F59: a pair of lines per step, reading `STEP     ` then the step name then `started`, and later the same name then `finished`, the seconds and a few words for what it changed. The step names are `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `IMAGES`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM`
297. Look for, F59: `STEP     TESTS RUN` appears ONCE as a started line and once as a finished line for the whole group, then one line reading `entered again. Every further visit in this group is counted, not written out`. A pair of lines per test would be 3660 lines for 1830 tests, which is the fault that once left a 17.8 MB log
298. Look for, F59: one line per repeated step at the end of the group, before the `GROUP    finished` line, reading the step name then the visits then the total seconds, like `STEP     TESTS RUN     1830 visits, 742.113s in total`
299. Look for, F59: `STEP       IMAGES` is indented two spaces further than every other step line. It is the only one that runs INSIDE another step, because a picture is written while the harvest is walking the results. `HARVEST` is NOT indented, because it opens and closes on its own
300. Look for, F59: NO line anywhere reads `NEVER CLOSED`. That line means a step was opened and nothing closed it, which would make every total after it wrong. If you see one, send the whole log
301. Look for, F59: one clock per piece of work. Find the FIRST `CLASH    ` line that reads `clashes` or `passed`, which carries a test name and its seconds, and find the `STEP` line for `TESTS RUN` that reads `finished`. Those two are the same reading, because the step IS the measurement now and the Stopwatch that used to time the same thing separately is gone. The `CLASH` line rounds to ONE decimal and the `STEP` line carries three, so 12.3 against 12.345 is them agreeing and not disagreeing. The step name is padded to the width of the longest one, so read the words and do not count the spaces
302. Look for, F60: a block headed `TIMING` and the building, straight after the `GROUP    finished` line of every group. Every step on its own row with its seconds and its share, slowest first, then a row reading `outside every step`, then `group total`
303. Look for, F60: the shares in that block add up to a hundred. They are meant to, because whatever the steps do not account for is a row of its own rather than a number left off the page. A column adding to less than a hundred means a row is missing and is worth sending
304. Look for, F60: the `group total` on that block is the SAME number as the seconds on the `GROUP    finished` line above it
305. Look for, F60: a line reading `inside another step, so these seconds are already counted above` at the bottom of the block, with `IMAGES` under it. Those seconds are inside `HARVEST`, so they are deliberately left out of the share column
306. Look for, F60: a block headed `TIMING, THE WHOLE RUN` just before the `RESULT` block at the end of the log. It reads the groups slowest first, then the run total, then the SAME steps added across every group, which is what answers which step costs the run rather than which building
307. Look for, F60: the last line of that block reads `The run took` and then the time in minutes and in seconds, and then either `inside the 45 minutes 0 seconds a run has to finish in` or `OVER` it. That sentence is criterion 2 answered by the log itself. Send it either way
308. Look for, F21 which closes with F60: one line per test reading `ROWS     ` then the test name then `N rows for the workbook, N clashes in the document`. Where the two agree it says `they agree`. Where there are FEWER rows than clashes it names the result groups as the reason, which is right, because the workbook and the Clash Detective panel both show one row per group
309. Look for, F21: NO line anywhere reads `THERE ARE MORE ROWS THAN CLASHES`. Nothing in this tool produces more rows than clashes, so that line means something is wrong and the whole log is worth sending
310. This is criterion 3 without opening Excel. Pick any test whose `ROWS` line says they agree, open the workbook at that test's block and count the rows, then open Clash Detective on the same test and read the panel count. All three should be the one number
311. Look for, F61: a pair of lines around each step reading `CENSUS   before ` then the step name, and `CENSUS   after  ` then the same name, each carrying five counts: `models`, `sets`, `tests`, `results` and `views`
312. Look for, F61: the `views` count is a NUMBER and not `UNKNOWN`. It reads `0` before `APPEND` and whatever the NWC files brought after it, which on the run of 2026-09-19 was 20. It used to say this count reads 0 on every line, and that was wrong: an NWC exported from Revit carries that model's saved viewpoints and appending it brings them in. `SavedViewpoints.Count` returns minus one only where it could NOT count, and minus one prints as `UNKNOWN`. An `UNKNOWN` here means the viewpoint collection is not the shape it was MEASURED to have on 2026-09-19, `docs\history\scan.md` 5d, and the whole log is worth sending
313. Look for, F73: NO line anywhere begins `CENSUS CHANGED`. That line means a count moved during a step that may not move it, which is a real fault, and the group carrying it will read FAILED rather than DONE. If you see one, send the whole log. A line beginning `CENSUS NOTED` is NOT that line and is not a fault: it is the saved viewpoints moving during `APPEND`, it says why, and it ends `can still be DONE`. On the run of 2026-09-19 the refused line was written in all seven groups and put every one of them out of DONE while 28 files had been written correctly
314. Look for, F61: the counts move where they should. `sets` goes up across `SETS`, `tests` goes up across `TESTS CREATE`, `results` goes up across `TESTS RUN`, and `models` goes up across `APPEND`. None of them moves across `NWD`, `WORKBOOK`, `HTML`, `XML` or `CONFIRM`. Expect `TESTS CREATE` and `TESTS RUN` to move by ONE test's worth and not by the whole group, because those two are entered once per test and the census is taken around the first visit only. That is step 315 and it is not a fault
315. Look for, F61: `CENSUS   before TESTS RUN` appears ONCE per group and not once per test. The census is taken around the first visit of a step and no more, because counting the whole document 1830 times would be the log making the run slower
316. Look for, F61: one line per group reading `CENSUS   cost ` then the seconds then the number of counts. This is the measurement of what the census itself costs, and it is the number that decides whether it stays wide
317. Look for, F61: that cost line says `inside the 1.000s a group is allowed`. If it says `over`, the census narrows from the next group on and the next group says `CENSUS   narrowed` at its top. Both are working as intended, but send the number either way, because it is the first real measurement of what the census costs on a real model
318. Look for, F62, while the run is WORKING and not after it: the line just above the log box reads `Group 3 of 14`, then the building, then the step, then how long that step has been going and how long the run has, like `Group 3 of 14  1B06PH  TESTS RUN  12s on this step  4m 02s on the run`
319. Look for, F62: that line sits on its own row above the log box. It has to stay put while the log box scrolls itself. If it is beside the buttons and cut off at the window edge, the build is older than F62
320. Look for, F62: the log box keeps scrolling to the newest line by itself the whole time, and the line above it never moves out of sight
321. Look for, F62: the line changes as the step changes. Watch one group through and you should see `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM` go past. `IMAGES` is the one step that never appears on this line, because it is opened inside the picture writer and nothing there was given the line. It is in the log and in the timing block like every other step
322. Look for, F62: inside the clash run the seconds on the run keep counting up while the tests go by. It is rendered at most once a second on purpose, so it should look like a clock and not like a flicker
323. Look for, F62, on the SECOND group onward: if a step runs over twice as long as the same step took on the group before, the line says `SLOWER, the group before took` and the time. On the first group it never says it, because there is nothing to compare against
324. Look for, F62: while the NWD is publishing the line stops counting and sits at the seconds it last showed. That is expected and it is written down: publishing is one Navisworks call with no loop inside it, so there is nothing to tick from without starting a thread, and nothing here starts one. If the line went blank instead, that is a fault
325. Look for, F63: a block headed `GAP` and the building at the END of every group, after every output has been written. That is the point: it answers what the outputs do NOT carry
326. Look for, F63: one line per property the run MEASURED and no output carries, each with how many item cells carried it out of how many there were, and where it would belong. The six it can name are `Family`, `Type Name`, `Material`, `Source File`, `Discipline` and `Id From`. FEWER THAN SIX IS NOT A FAULT: a property no item carried at all is not a gap and is left out on purpose, because reporting it would say the run is holding back something it never read. Which ones appear is itself the measurement
327. Look for, F63: the first line of the block says `nothing acts on it`, or, where a group held nothing back at all, the block is one line reading `nothing measured this group reaches no output`. Either way a gap is a question and never a fault, and no group should read anything but DONE because of one
328. Read the counts on those six lines. They are the first real measurement of how much of each property a real model actually carries. `Material` is expected to be low and `Family` high, and if `Material` is at zero it may be worth deleting rather than keeping, which is Q37
329. Look for, F63: one line at the end of the run reading `GAPS     ` then a number then the names. It counts by NAME and not by line, so a run of fourteen groups says six and not eighty four
330. The six are now Q35 to Q40 in `steps\02_questions.md`, one per property, because one answer for five different properties is a decision nobody can make. Answer them with the counts from step 328 in front of you
331. Look for, F64: in `%LOCALAPPDATA%\ParsonsNwcFederator\logs`, beside every `run-*.log`, a `run-*.tsv` with the SAME name and a different extension
332. Look for, F64: near the top of the text log, one line reading `ROWS     the machine readable log is` and the path. If it says `no machine readable log` instead, the text log is unaffected and the reason is on that line, so send it
333. Open the `.tsv` in Excel. It should open straight into columns with no import dialog and no question about separators
334. Look for, F64: a header row reading time, seconds, group, step, event, name, number, text, and then one row per event under it
335. Look for, F64: every row has eight columns. Sort by the `event` column and read the kinds. There are fourteen and they are all of them: `step started`, `step finished`, `step threw`, `step total`, `timing`, `timing nested`, `census before`, `census after`, `appended`, `append failed`, `written`, `gap`, `rows for the workbook` and `group finished`. A kind not on that list is something new and the whole file is worth sending
336. Filter the `event` column to `step finished` and sort the `number` column biggest first. The top row is the slowest step of those entered ONCE in their group, in seconds. It is NOT the slowest step of the run, and that is worth knowing: a `step finished` row is written only on a step's first visit, so `TESTS RUN` and `TESTS CREATE`, which are entered once per test, each contribute one row for their first visit and reach the file again only as a single `step total` row per group. To find the slowest step of the run, read the `step total` rows beside these, or read the TIMING block in the text log, which adds both kinds up already
337. Look for, F64: nothing in the text log got worse. It still has its blocks, its indenting and its sentences, and it is still the one to read first and the one to send

## The two walls are live, D7

There are three files here that `sh` reads: the two hooks Claude Code runs before a tool
call, and the pre-commit hook that runs the tests. One refuses an edit under `samples`,
`steps\logs` or `bundle`, one refuses a commit or a push while main is checked out, and
the third refuses a commit while any test fails.

None of them survives a carriage return. `sh` reads one as part of the word, so a CRLF
copy dies on its first `case` line before it reaches a rule, and because `sh` exits 2 on
a syntax error, and 2 is the code that refuses, the two Claude Code hooks then refuse
EVERY call rather than the protected ones, while the pre-commit stumbles past `set -e`
and lets every commit through untested. Your machine is where that bites, because Git for
Windows sets `core.autocrlf` to true when it installs and the repo had nothing telling it
otherwise. F47a added `.gitattributes`, which pins these three to LF on every checkout.

This is a one time check per clone. Do it once and the rest of this file never needs it.

338. In the VS Code terminal, in the repo folder, run:

```
git ls-files --eol .claude/hooks .githooks
```

339. Look for: three lines, each reading `i/lf` and `w/lf` and `attr/text eol=lf`. A `w/crlf` on any of them means this checkout still holds the old copy, so run `git add --renormalize . ; git checkout -- .` and read it again
340. Switch the test wall on, which git needs told once per clone:

```
git config core.hooksPath .githooks
```

341. Look for: `git config core.hooksPath` answers `.githooks`
342. Make a branch, change one word in `steps\log.md`, and commit it from the VS Code terminal rather than from GitHub Desktop, so you see what the hook prints
343. Look for: the commit pauses and prints FOUR lines in this order, `pre-commit: checking the add-in for a local declared twice`, `pre-commit: checking the add-in for a type with no import`, `pre-commit: running the full test set`, then `pre-commit: tests passed`, and only then commits. That is the test wall, and the two checks in front of it are F58's and F66's. A commit refused by either of those prints `pre-commit: REFUSED. See the line above.` and never reaches the test set at all. Throw the branch away afterwards
344. Open Claude Code in this folder and ask it to write one word into any file under `samples`
345. Look for: it comes back refused, with the line `Refused. ... is under samples, steps/logs or bundle, which are never edited.` That is the paths wall, and the branch wall is the same hook file beside it, proved the same way by asking it to commit while main is checked out

## Delete the old branches, D6

Every branch except main is merged into main. This is yours, one command from the repo folder in the VS Code terminal. A session working in a container cannot do it: `git push origin --delete` comes back HTTP 403 from the proxy in front of it, and no GitHub tool in it deletes a branch.

The list below was read on 2026-09-19 with the command in step 346, after the penetration round, and `git ls-remote --heads origin` gave 67 names. Two more go up with this round's own closing work, `fix-F72` and `round-close-penetrations`, and neither was on the remote when the list was read, which makes 69 lines and 68 to delete by the time you run it. Read the live list again yourself before you delete, because a branch may have come or gone since. Do not build the list from `git branch -r`. That prints remote-tracking refs your clone remembers, and a branch deleted by someone else is still in it until you prune, which is how a name that does not exist on the remote reached this file once already. A clone showed it again on 2026-09-18, still holding `origin/claude/parsons-nwc-analysis-rlzgdr` after the remote had lost it. `git ls-remote` asks the remote and remembers nothing.

346. Read the live list:

```
git ls-remote --heads origin
```

347. Look for: one line per branch, the name after `refs/heads/`. Expect 69 of them, so 68 to delete. It was 66 after the build round, and the penetration round added F71, F72 and its closing one
348. Delete every one of them except main:

```
git push origin --delete analysis-pass fix-F16 fix-F27 fix-F28 fix-F29 fix-F30 fix-F31 fix-F32 fix-F33 fix-F34 fix-F35 fix-F36 fix-F37 fix-F38 fix-F39 fix-F40 fix-F41 fix-F42 fix-F43 fix-F44 fix-F45 fix-F46 fix-F47a fix-F47b fix-F47c fix-F50 fix-F51 fix-F52 fix-F53 fix-F54 fix-F55 fix-F56 fix-F58 fix-F59 fix-F60 fix-F61 fix-F62 fix-F63 fix-F64 fix-F65 fix-F66 fix-F67 fix-F68 fix-F69 fix-F70 fix-F71 fix-F72 fix-f1-f2-f4-small fix-f10-gate-outputs fix-f11-dead-code fix-f17-picture-order fix-f20-tests-on-push fix-f22-two-workflows fix-f24-rebuild-changed-nwf fix-f26-units-meters fix-f5-sets-built fix-f6-open-file-folder fix-f7-open-file-result fix-f8-run-saved-tests fix-f9-changed-skip-units master round-close round-close-2 round-close-3 round-close-4 round-close-build round-close-log round-close-penetrations
```

349. Look for: one `- [deleted]` line per branch and no error
350. Run `git ls-remote --heads origin` again and look for: one line, `refs/heads/main`. If a branch you did not expect is there, it was pushed after the list above was read, so read what it holds before deleting it
351. Run `git fetch --prune` so your own clone forgets the branches that are gone. Without it `git branch -r` keeps printing them
352. If the command refuses a branch, open github.com, the repo, Branches, and press the bin icon beside every branch that is not main

## The first run round, F73 to F88

**READ THIS FIRST. This round was worked in a LINUX CONTAINER with no Navisworks on it.
Every rule below is in Federator.Core, every one has tests, and the whole Core set passes.
NOTHING IN THE ADD-IN WAS COMPILED, because the add-in cannot be compiled without
Navisworks and this session had none. Five of the eighteen fixes turn on a measurement
only Navisworks can give and those five are steps 353 to 357. The add-in wiring each fix
still needs is steps 358 onward, and every one of them is written out so it can be done
without reading the brief again.**

### The five measurements, before any of the wiring

353. Open a Developer Command Prompt and dump the members of `Autodesk.Navisworks.Api.dll`
     looking for anything that says a document has finished LOADING. Every member on
     `Document`, `Document.Models`, `DocumentParts.DocumentModels` and `Application` whose
     name holds Load, Ready, Busy, Progress, State, Pending or Complete, and every event on
     each. Paste the list into `docs\history\scan.md` under 5e, where the question is
     already written, and say in one line whether any of them answers "the models are all
     in now". If one does, `ModelLoadWait` becomes the fallback and the add-in reads the
     member instead. If none does, the poll stands and that sentence is why
     DONE on 2026-09-19 in the wiring round by toolsprobesprobe-document-ready.ps1, written into scan.md 5e. SceneLoaded exists on DocumentModels, its timing does not read off the DLL, so the poll stands and step 365 counts the event beside it.
354. Dump the property members the same way, for F86, into 5f: `ModelItem
     .PropertyCategories`, `PropertyCategory.DisplayName`, `.Name`, `.Properties`,
     `DataProperty.DisplayName`, `.Name`, `.Value`, every reader on `VariantData`, and
     whether a `Search` can walk a whole model in one pass rather than per item. One line
     saying how a value becomes the text a CSV cell holds, and one saying whether the walk
     is per item or per search
     DONE on 2026-09-19 in the wiring round by toolsprobesprobe-properties.ps1, written into scan.md 5f.
355. In Clash Detective, build ONE search set by hand with a NEGATED condition, export the
     selection sets to XML, and paste the `<condition>` element into 5g. Then import that
     same XML into a fresh document and confirm the set comes back with the negation still
     on it. That answers whether `BLD-EL-Devices` can be written as category contains
     Devices AND NOT the six named device categories. Until it is answered the file in
     `exchange\` carries the fallback, `Category equals "Nurse Call Devices"`, which is
     proved to import because the whole file uses `equals`
     NOT done in the wiring round. What reflection can say is under 5g: SearchCondition.Negate exists and NegateCondition is bit 32. The hand built set and the round trip still wait for you.
356. Dump the clash members for F72c into 5h: every member on `ClashResult`,
     `IClashResult` and `ClashTest` whose name holds Comment, Note, Description, Tag,
     UserName, Status or Approved, and whether the general `Comments` collection reaches a
     clash result. Then write one comment on one clash by hand, save the NWF, close it,
     open it again and see whether the comment is still there. IF IT CANNOT BE DONE the
     answer is one line in 5h saying so, and the tool then sets the status alone and fakes
     nothing
     DONE on 2026-09-19 in the wiring round by toolsprobesprobe-clash-comments.ps1, written into scan.md 5h. TestsEditResultComments writes one, and step 372 is wired to it. Whether it survives a save and reopen is what the run log of the wiring round shows.
357. Open one real federation and walk every item's category property, writing the
     distinct values out. That is F84's list and it is the same walk F86's probe does, so
     the two are measured on one run. Paste the values into
     `src\Federator.Core\Exchange\revit-categories.txt`, one per line, exactly as the model
     spells them, and leave the comment block at the top where it is. Until that file has
     names in it the health check compares against nothing and says so

### How to run the Property Probe, F86, and where its CSV goes
     NOT done in the wiring round. The probe as built reads only the seventeen categories the settings ask for, so it cannot list every category a model carries, and 5i needs its own walk.

358. The button reads `Probe model properties` and it is on the Clash step. It does not run
     as part of a federation run and it never touches an NWF
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
359. Press it and pick either a FOLDER of NWC files or the document you already have open.
     It reads NWC and refuses an NWF or an NWD by name, because opening an NWF replaces
     whatever is open and an NWF is where every clash result lives
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
360. It writes ONE CSV per file, beside that file, named after it with
     `-properties.csv` on the end. So `1104-PAR-1C07BC-ZZZ-ME-MOD-000001.nwc` gives
     `1104-PAR-1C07BC-ZZZ-ME-MOD-000001-properties.csv` in the same folder
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
361. The CSV has five columns: category, property tab, property name, distinct value, how
     many elements. It covers seventeen categories, which are the thirteen this tool calls
     a service plus the four it has decided are not one, and that list is read off the
     client's own matrix rather than typed
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
362. A property carrying more than 100 distinct values keeps the commonest 100 and gets ONE
     extra row saying how many were left out and how many elements they covered. Nothing is
     dropped silently
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
363. Look for, F86: a `PROBE` block in the log, one per file, saying how many categories
     were asked for, how many were found, how many found no element at all, how many
     properties and distinct values there were, which properties were capped, and whether
     FS or Fire Suppression appears in any tab, name or value. IT SAYS SO AS PLAINLY WHEN
     IT DOES NOT, because a probe that only speaks up when it finds something reads as one
     that found nothing rather than as one that ran
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.
364. Send the CSV for one mechanical NWC and the PROBE block beside it. That pair is what
     the mechanical sets get rewritten from, and rewriting them is a later round

### The add-in wiring each fix still needs
     WIRED on 2026-09-19 in the wiring round, F86, commit dcbbf67.

365. F74. In `FederationEngine.Decide`, after `document.TryOpenFile` returns true, poll
     `document.Models.Count` through `Federator.Core.Rerun.ModelLoadWait`, handing it the
     count and `RunLog.ElapsedSeconds` each time, pausing `PauseMilliseconds` between
     readings, and write `wait.Line()` when it ends. Where the wait SETTLES, carry on into
     `NwfComparison.Compare` exactly as today. Where it gives up at the ceiling AND the
     count is still zero, return `NwfComparison.ReadEmpty(job.NwfPath, job.Files,
     "after waiting " + seconds)` instead, and carry its `Reason` onto the group as
     `GroupFacts.NwfReadEmptyReason`. Do the SAME thing in `PreviewRunPaths`, which is a
     second reader of the same NWF, or the confirm dialog says Rebuilt about a healthy file
     WIRED on 2026-09-19 in the wiring round, F74, commit fcbdb6b. The seconds handed to the wait are counted from the open RETURNING, off a stopwatch started for it, not RunLog.ElapsedSeconds, because the ceiling is thirty seconds from the open and not from the session.
366. F75. Empty the document at the TOP of `RunOne`, before `Decide`, on the scanned path
     only. Take the census straight after and write `CensusRule.StartOfGroupLine(census,
     true)`, and put `StartOfGroupReason(census, true)` on the group where it is not null.
     The open file run passes `false` and empties nothing, because the document IS the file
     list there
     WIRED on 2026-09-19 in the wiring round, F75, commit b5ce89f.
367. F76. Read the drop down into `ReportOptions.Tolerance`. In `ClashRunner`, where a test
     is created and where a test already in the document is left alone, call
     `options.Tolerance.For(planned.Tolerance, documentUnits)` and set THAT. Write
     `LogLine(created, alreadyThere, documentUnits)` once per group. Put
     `WarningLines(groups, testsInTheFile)` on the confirm dialog
     WIRED on 2026-09-19 in the wiring round, F76, commit b6ec0f2.
368. F76 second half. `ClashRunner` line 406 sets `report.Tolerance = planned.Tolerance`,
     which is the XML and not the document. Read it off the `ClashTest` in the document
     instead and set `report.ToleranceFrom = ToleranceOrigin.Document`. Count the four
     origins across the run and write `ToleranceChoice.ReadFromLine`
     WIRED on 2026-09-19 in the wiring round, F76, commit ee62966.
369. F77. Before creating any test, build a dictionary of locator to item count off the
     sets just resolved, hand it and the plan to `CreationPlan.For`, create only
     `plan.Create`, feed `plan.NotCreated` into the existing skip machinery, and write
     `plan.CountedLine(testsInTheFile)`
     WIRED on 2026-09-19 in the wiring round, F77, commit 03a913e.
370. F83. A second picker beside the clash XML picker on the Clash step, optional, and
     `PickerKind.Priority` for where it opens. Read the file into `PriorityMap.Read`, put
     it on `report.Priorities`, set `test.Priority` on every `TestReport`, write
     `map.MatchLines(testNames)`, and pass `picked` into `WorkbookCheck.Of(path, picked)`.
     Count the clashes into a `PriorityTally` and write `ResultLine` in RESULT
     WIRED on 2026-09-19 in the wiring round, F83, commit 580fe51.
371. F72b. A second tick box under the penetration one, off by default, its label and grey
     line read off `ByDesignPairs.TickLabel` and `HelpLine` and never typed into the XAML.
     A third picker for the pairs file. In the same pass that applies the penetration
     statuses, and NOT a second write path, call `ByDesignRule.Judge` per clash, add the
     wanted ones to the one `statuses.Apply` list, and write the block. The XAML grid has
     two rows today and needs a third, with the Things that destroy data expander moved
     down
     WIRED on 2026-09-19 in the wiring round, F72b, commit 8f7cedd.
372. F72c. Write `record.Text()` as a comment on the clash BEFORE the status is set and on
     the same handle, because every mutator on `DocumentClashTests` is a copy form that
     kills the handle handed to it. If step 356 says a comment cannot be written, write
     `UndoAutoReview.CannotLine(why)` once and set the status alone
     WIRED on 2026-09-19 in the wiring round, F72c, commit 377d1f0.
373. F72c. The `Undo auto Reviewed` button reads every clash, calls `UndoAutoReview.Judge`
     and puts back only the ones that carry our record AND are still at Reviewed, each to
     the status the record names. It goes through the one `ClashStatusEditor` like
     everything else
     WIRED on 2026-09-19 in the wiring round, F72c, commit 377d1f0.
374. F85. `ViewpointBuilder` loops CLASHES and not disciplines now, reading
     `ClashViewpointPlan.For`. `SavedViewpoints.Exists` resolves one folder under the root
     today and cannot see a three deep path, so rebuild the folder walk on `SetBuilder`'s
     measured `EnsureFolders` shape, re-resolving from a FRESH `RootItem` after every
     `AddCopy`. Open the step as `RunSteps.Views`, which is new. LEAVE `CanBuild` FALSE
     until step 375 answers
     DONE on 2026-09-19 in the viewpoints round, commit 11d7ec2: Build takes the clash plan, the old per discipline plan is gone, the folder walk is EnsureFolders re-resolved from a fresh RootItem, and the VIEWS step is opened around it.
375. F85. On a run, save one viewpoint by hand while some items are hidden, then read
     `SavedViewpoint.ContainsVisibilityOverrides` on it and press it again from a clean
     view. That answers whether a viewpoint records the hiding, which is the last thing
     `CanBuild` waits on, and the answer goes in `docs\history\scan.md` 5d
     DONE on 2026-09-19 in the viewpoints round by toolsprobesViewpointProbe, written into scan.md 5j and not 5d: a viewpoint captured with CaptureRuntimeOverrides records the hiding and brings it back after a save and a reopen, and CanBuild is true.
376. F80. Two lines in the window changed already: `log.RunStarted(jobs.Count)` and
     `log.RunFinished()` in place of the two `log.Line` calls that wrote the same
     sentences. Read them once and check the log reads `RUN      started, 7 groups` and
     `RUN      finished` exactly as before
377. F81. Four call sites changed already: three in `ClashRunner` now use
     `log.NumberedRepeat` and the per group SETS block in `FederationEngine` is gone. Read
     them once, then on the next run check the `.log` is under 300 KB and the `.tsv` still
     carries a row for every test
378. F82. `FederationEngine` feeds `SetsAcrossTheRun` beside `outcome.Sets` already and the
     window writes the block after SOURCE FINDINGS already. Read both once
379. Build it: `dotnet build ParsonsNwcFederator.sln -c Release`. EXPECT ERRORS the first
     time. Eighteen fixes touched the add-in and none of them was compiled. Send the whole
     error list if there is one
380. Run one building and send the log. Look for, in order: `LOADING` then `STOPPED` or a
     model count, `CENSUS CLEAR` at the top of the group, `CENSUS NOTED` and NOT `CENSUS
     CHANGED` around APPEND, `TOLERANCE`, `CLASH 1830 in the file`, `PRIORITY` if a file
     was picked, `REVIEWED rule B`, `VIEWS`, `SETS ACROSS THE RUN`, and a RESULT block
     carrying `run time`, `waiting for the person` and both file sizes
