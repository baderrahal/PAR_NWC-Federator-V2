# 03 Bader next

**The last run, 2026-09-07 09:34, was on the old build be0b9b37 of 1 Sep. Every fix from F5 to F46 has merged since, F16 and the eight of the second audit round among them. Pull main and build before anything.**

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

10. Look for: the build finishes with no error. If it names CreateCopy or CopyFrom on DocumentSelectionSets, copy the whole error into the chat, that is the one member F24 and F29 could not measure from the container
11. Install:

```
powershell -ExecutionPolicy Bypass -File build\install.ps1
```

12. Read the last lines of the install. Look for: a line saying all the expected files are present and a line saying every reference is satisfied. If it stops with Install incomplete, copy the whole output into the chat

## Proof F34, the window, and F33, the units combo

13. Close Navisworks if it is open
14. Open Navisworks Manage 2025 with nothing open
15. Open the add-in from the ribbon
16. Look for: the window title carries today's commit and build time, not be0b9b37
17. Look for: the window has four tabs, Source, Grouping, Outputs, Clash
18. Go to the Outputs step
19. Look for: the Model units combo lists `Metres, which is what the models are set to`, then Millimetres, Centimetres, Feet, Inches, in that order, with the first chosen
20. Look for: the grey line under it says the report is always in metres
21. Go to the Clash step
22. Look for: there is no Outstanding counts combo on the Clash step. It used to sit under the tick boxes
23. Look for: the two buttons read `Sets into open model` and `Tests into open model`, under the line `Not part of the run. Try the file above against the model open right now:`
24. Pick the clash XML, 1104-PAR_CLASH_AllInOne
25. Look for: the line under the file box ends with `Every locator resolves against its sets.`
26. Look for: the log pane holds a block headed `HEALTH 1104-PAR_CLASH_AllInOne.xml` with `Tests: 1830`, `Sets: 61` and a `Locators resolved` line whose two numbers are equal
27. Clear the clash XML box

## Proof F27, the GROUPS block tells the truth

28. Go to the Source step, pick the same C06 NWC folder as last time and press Scan
29. Look for: the count line under Scan reads files found, files readable, groups and the findings by kind, then the findings themselves
30. Go to the Outputs step and pick the same NWF folder and NWD folder as last time
31. Go to the Grouping step
32. Untick two groups, 1B06PH and 1B06G1, which is the only thing in this tool that drops a group from a run
33. Do not press Run here. The GROUPS block is written when a run actually STARTS, so a Run followed by a Cancel leaves nothing to read, and the preview behind that Run opens each NWF and leaves the last one open, which would stop the next proof counting the rebuilt groups
34. Tick 1B06PH and 1B06G1 again, so the next proof runs every group
35. Look for, F27, later: the GROUPS block of the 1B06PH run at step 65 lists 1B06PH with `run`, the other thirteen with `unticked`, and the word `skipped` nowhere
36. Look for, F27, in the same block: one line after the list reads `13 groups unticked in the Run column, nothing else drops a group`
37. Go on to the next proof with every group ticked and nothing open in Navisworks

## Proof F24, the six CHANGED groups are rebuilt from the scan, with F29 and F30 in the same run

38. Leave the clash XML box empty
39. Tick every group and press Run
40. Look for: the label at the bottom reads Checking NWF 1 of 14 and counts up, then the confirm dialog opens
41. Look for: the dialog says `Rebuilt: 6` and that the NWF is cleared and rebuilt from the scan folder with its saved tests kept
42. Press Cancel
43. Look for: the log says `Run cancelled before anything was cleared.`
44. Go to the Grouping step
45. Look for: 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE read Rebuilt in the Run as column, and the other eight read Weekly run
46. Press Run again, then OK, and wait for it to finish. The confirm dialog now also says the NWF the preview opened will be discarded, which is right, and the preview does not run a second time for the same reason
47. Look for, F50: per rebuilt group the log has a CHANGED line with the counts, then a REBUILT block, `REBUILT <nwf>  1 added, 4 moved, 0 removed` for 1B06BC, then one line per file, then FOUR lines together, starting `SETS`, `TESTS`, `VIEWS` and `RESULTS`. Before F50 there were two and they read in two different shapes
48. Look for, F50: each of those four says `kept` or `none`, and never `LOST` and never `NOT COUNTED`. Any one of them saying either means the NWF on disk was left alone on purpose, which the line after them says in words
49. Look for, F29 and F50: the `SETS` line carries three numbers, read before the clear, after the appends and after the copy is put back, and the first and the third agree. The other three lines read the same way, because four things reading four ways is how a log stops being read
50. Look for, F33: per group a `UNITS` line saying how many models were set and what the document shows, in the words `model set` or `models set`, and no line anywhere says DID NOT FOLLOW
51. Look for, F30 and F52: per group, after the models are in, the blocks come in this order: UNITS, then CLASH, then `ITEM IDS` where the report carries item ids, then `VIEWS`, then `NWF      attempt`, then XLSX, then NWD, then the final NWF line saying intact and the size. The `VIEWS` line sits between the clash step and the NWF because F52 makes the viewpoints there, so a viewpoint this run made is inside the file the NWD is published from. No SETS block follows UNITS on this run, because sets come from the clash XML and the box is empty. The four SETS, TESTS, VIEWS and RESULTS lines the rebuilt groups carry are written during the rebuild, before UNITS
52. Look for: the GROUP finished line of each of the six reads the building, then `DONE`, then the seconds, and ends with `Rebuilt`
53. Look for: the RESULT block carries `rebuilt        : 6` and `weekly run     : 8`
54. In Explorer open the NWF folder and open the NWF of 1B06BC in Navisworks
55. Look for: the Selection Tree lists 5 models, EL among them, all from the Published folder
56. Look for, F29: the Sets window holds the same sets as it did before the run, Mechanical with its four subfolders among them
57. Open the NWD of 1B06BC from the NWD folder
58. Look for: the same 5 models
59. Look for: no group in this run reads PARTIAL
60. Close the NWD and the NWF so nothing is open

## Proof F28 and F31, one building twice with the XML

61. Go to the Clash step and pick the clash XML
62. Go to the Grouping step and tick 1B06PH only
63. Press Run
64. Look for: the confirm dialog says `Weekly run plus XML: 1` and its line ends with `Nothing is cleared.`
65. Press OK and wait for it to finish
66. Note the time the run took, from the GROUP started line to the GROUP finished line
67. Look for: the log says OPENED for the group
68. Look for: the SETS block says how many sets were created and `already there     : 61, left alone, not copied again` if the NWF already held them from last week, or `sets created      : 61` if it did not
69. Look for: the line `CLASH    source   tests from XML` and the tests running after it
70. Press Run again on the same building, same folders, same XML
71. Press OK and wait for it to finish
72. Look for, F28: the SETS block reads `sets created      : 0` and `already there     : 61, left alone, not copied again`, and the line after it reads `put into the document: 0 created, 61 already there and left alone`
73. Look for, F28: the SETS block's put into the document line reads `0 created, 61 already there and left alone`. A second `NWF      attempt` line still follows the CLASH block, because the tests ran and their results went into the document
74. Look for, F31: the DRIFT block reports no locator difference on any test. A locator difference on every test would mean the source held since the sets were indexed does not compare equal to a fresh one, which is the one thing F31 could not measure here
75. Look for, F31: the CLASH block shows the same created, already there, run, skipped and passed counts as the first run
76. Look for, F31: the second run took no longer than the first

## Proof F35, a group of one discipline creates every test and runs none

77. In Explorer make a folder on the desktop named OneDiscipline
78. Copy the AR NWC of 1B06PH into it
79. Copy it again inside that folder and rename the copy so its number reads 000002 instead of 000001, so the folder holds two AR files of one building
80. In the add-in go to the Source step, pick the OneDiscipline folder and press Scan
81. Look for: the findings say SINGLE DISCIPLINE for 1B06PH
82. Go to the Grouping step
83. Look for: the status of 1B06PH reads `Ready. One discipline, so every test is created and none is run.`
84. Go to the Outputs step and pick a new empty folder on the desktop for the NWF and the NWD, so nothing under C06 is touched
85. Leave the clash XML picked and press Run, then OK, and wait for it to finish
86. Look for: the log line `CLASH    1B06PH holds one discipline, so every test is created and none is run. One discipline cannot clash with itself.`
87. Look for: the CLASH block says 1830 created and 1830 skipped, with the skip reason counted as one discipline and not as an empty side
88. Look for: the GROUP finished line reads the building, then `DONE`, then the seconds, then which path it took. `DONE` is never the last word on the line
89. Put the Source folder back to the C06 NWC folder and press Scan
90. Put the NWF folder and the NWD folder back to the C06 ones

## Proof F26, the report is always in metres

This one is its own small run, with the clash XML, because the conversion only happens where a report is written.

91. Go to the Outputs step
92. Set the Model units combo to Feet, on purpose, so the models are put into feet
93. Go to the Grouping step and tick 1B06PH only
94. Press Run, then OK, and wait for it to finish
95. Look for: the line `model units      : Feet, which is what the models are set to`
96. Look for: the line `report units     : Meters (m), always, converted before anything is written`
97. Look for: one line `UNITS    every number converted from ft to m, one ft is 0.3048 m, ...` before the XLSX line
98. Open the workbook in the Clash Reports folder beside the NWF folder
99. Look for: the Tolerance cell of the first block reads `0.075m`, not `0.246ft`
100. Look for: the Distance column holds metres, so a hard clash reads about -0.050, not -0.164
101. In Navisworks open Clash Detective and select that test
102. In Options, Interface, Display Units, set Linear Units to Metres
103. Look for: the panel tolerance reads 0.075 and one clash distance matches its workbook row to three decimals
104. Go back to the Outputs step and set Model units to Metres

## Proof F22, the labels before a run

105. Go to the Outputs step and pick an NWF folder that holds no NWF yet
106. Go back to the Grouping step
107. Look for: every group reads First run
108. Go to the Clash step and pick the clash XML
109. Look for: every group still reads First run, because no NWF is there yet
110. Put the NWF folder back to the C06 one
111. Look for: every group reads Weekly run plus XML

## Proof F9, a changed folder is rebuilt and not run in feet

112. Copy one NWC of 1B06PH out of the NWC folder to the desktop, so the folder lost one file
113. Close whatever is open in Navisworks, so the preview can open each NWF, then press Scan again and tick 1B06PH only
114. Press Run
115. Look for: the dialog says `Rebuilt: 1`
116. Press OK
117. Look for: the REBUILT block names the removed file with `removed`
118. Look for: no UNITS line comes before the REBUILT block
119. Copy the NWC back into the NWC folder
120. Press Scan again, tick 1B06PH, press Run, press OK, so the NWF holds all its files again
121. Look for: the REBUILT block names the file with `added`

## Proof F10, each output on its own switch

122. Go to the Outputs step and open More
123. Tick Write a clash XML beside each workbook
124. Untick all five image status boxes, New, Active, Reviewed, Approved and Resolved
125. Tick 1B06PH and press Run, then OK
126. Look for: one `OUTPUTS` line reading `workbook on, XML on, HTML on, images off`
127. Look for: `IMAGES   skipped  images are switched off for this run`
128. Look for: `XLSX`, `HTML` and `XML` each with an attempt line and a written line
129. Untick Write a clash XML and tick New, Active and Reviewed again
130. Press Run again, then OK
131. Look for: `XML      skipped  not wanted this run`
132. Look for: the pictures rendered again, counted on their own `IMAGES` line after the CLASH block. The CLASH block counts clashes and never pictures

## Proof F17, the pictures are numbered in the export order

133. Stay on the run just finished
134. Look for: one line `IMAGES   numbered in report order: <n> renamed, <n> already right, 0 missing` after the CLASH block
135. Open the Clash Reports folder beside the NWF folder and open the 1B06PH workbook in Excel
136. In Navisworks open Clash Detective and select the test that is the first block of the workbook, the one with the most clashes
137. Look for: the clashes on the sheet are in the same order the Clash Detective panel lists them
138. Look for: the Image cell on row 1 of that block links to cd000001.jpg, row 2 to cd000002.jpg, and so on
139. Look for: the second block's pictures start at cd010001.jpg
140. Click the Image cell on three rows in three different blocks
141. Look for: every link opens its picture, and the picture shows the two items named on that row
142. In Explorer open the `_files` folder beside the workbook and look for: no file ending `.moving`

## Proof F8 on the scanned run, no XML

143. Clear the clash XML box on the Clash step
144. Go to the Grouping step
145. Look for: 1B06PH reads Weekly run
146. Press Run, then OK
147. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
148. Look for: the CLASH block says the tests ran and the workbook was written
149. Look for: no SETS block, because no XML means the sets are left alone

## Proof F32, an NWD open is refused

150. In Navisworks open the NWD of 1B06PH from the NWD folder
151. Open the add-in from the ribbon and go to the Clash step
152. Look for: the line above Run the open file reads `This document is a .nwd file and this tool runs an NWF, which is where the clash tests and their results live. Open the NWF instead.` and the Run the open file button is greyed
153. Close the NWD

## Proof F6, the open file report folder, with F30 and F32 on the NWF

154. In Navisworks open the NWF of 1B06PH
155. Open the add-in from the ribbon
156. Go to the Clash step
157. Look for, F32: the blue line above Run the open file starts with `Weekly run.`, names the NWD it will write beside the NWF and names one Clash Reports folder beside the NWF, and the button is enabled
158. Leave the clash XML box empty
159. Press Run the open file and wait for it to finish
160. Look for, F30: after the models are in, the blocks come in the same order as on the scanned run: UNITS, then CLASH, then `NWF      attempt`, then XLSX, then NWD, then the final NWF line
161. Open the NWF's folder in Explorer
162. Look for: one folder named Clash Reports beside the NWF, with the workbook and the page inside it
163. Look for: no Clash Reports folder inside that Clash Reports folder

## Proof F8 on the open file, no XML

164. Stay on the run just finished
165. Look for: the line `CLASH    source   tests saved in the document, 1830 of them, no XML picked`
166. Look for: the CLASH block says the tests ran and the workbook was written

## Proof F7, the RESULT block and the log copy

167. Stay in the same NWF folder in Explorer
168. Look for: a file named run-yyyyMMdd-HHmmss.log beside the NWF, from the Run the open file press
169. Open that log in Notepad
170. Look for: a GROUP started line and a GROUP finished line naming the NWF, the finished line ending with `Weekly run`
171. Look for: a block titled OPEN FILE naming the file, the NWD and the report folder
172. Look for: a RESULT block at the end with groups done, partial and failed
173. Look for: the RESULT block counts one group, not zero, and carries `weekly run     : 1`

## Send the logs

174. Open the folder `%LOCALAPPDATA%\ParsonsNwcFederator\logs`
175. Copy every `run-*.log` written today
176. Paste them into `steps\logs` in the repo folder
177. Rename each with the date, the building and the press, like `2026-09-14-C06-rebuilt.log`, `2026-09-14-1B06PH-first.log`, `2026-09-14-1B06PH-second.log`, `2026-09-14-onediscipline.log`, `2026-09-14-1B06PH-noxml.log` and `2026-09-14-1B06PH-openfile.log`
178. Open GitHub Desktop
179. Press New Branch, name it `logs-2026-09-14`, and press Create Branch
180. Write a summary like `logs from C06, F34 to F7 proofs`
181. Press Commit
182. Press Publish branch
183. Press Create Pull Request, and tell the worker in the chat that it is open. The worker merges it and reads the logs. Nothing goes to main directly, that is the wall F38 put up

## Run the window probes

184. In the VS Code terminal run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-defaults.ps1
```

185. Look for: it prints the window defaults and no error about a path
186. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-window-labels.ps1
```

187. Look for: it says how many tick boxes are visible without opening anything, which must be 1, and reports no label over eight words and no help line over twelve

## Proof F41, the handles, off the two runs above

188. Before you press Run the second time in step 70, open Task Manager, the Details tab, right click a column heading, Select columns, and tick Handles
189. Look for: the handle count on the Navisworks row at the end of the second run, beside the one at the end of the first. Copy both into the chat. F41 releases every wrapper this tool creates where it is finished with it instead of leaving it to a finalizer, so a second run over the same 61 sets and 1830 tests should not sit higher than the first. A count that climbs run on run is the thing worth reporting
190. Look for, in the log of either run: no line saying a set was not at the index the count gave before the add. That line is written only when the tree is not the shape the fast read assumes, and it means the set was then looked for by name

## Proof F45, the clash step keeps its rules, off the two runs above

191. Look for, in the DRIFT block of either run: either no line about sides left out at all, or one saying how many sides were left out because which set one end points at could not be read. That line is new and says not compared is not the same as matching
192. Look for: a block headed `ITEM IDS 1B06PH`, one line per property that supplied an item id with a count out of the total, and never a line per item. Each line ends `written as "Element ID"`. Expect `Id` to supply most of them, because `Id` is the first name looked for and `Element ID` is the label this tool chooses
193. Look for, if any `CLASH    OLD` line appears: it reads the test name and `Navisworks has this test marked Old.` and nothing after that. It used to carry a sentence about what Old means, which is UNKNOWN
194. Look for, only if you tick compacting on the Clash step: the compacted line carries both numbers, the Resolved count before and the count read again after, and the number reported as removed is the difference between them

## Two probes, F50 and F52. Run these BEFORE the run proofs above need them

Both answer a question the container cannot answer, because it has no Navisworks DLL and
no PowerShell on it. Neither needs the add-in built. Neither opens a model. Each takes
about ten seconds and each prints a block to paste back.

195. Open PowerShell in the repo folder and run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-model-remove.ps1
```

196. Look for: a block headed `THE QUESTION: anything anywhere that takes a Model or an index and removes it`. Either it lists one or more members, or it says `none, public or not, anywhere in the assembly`. Both are real answers and the second is the one F50 is built for
197. Run:

```
powershell -ExecutionPolicy Bypass -File tools\probes\probe-viewpoints.ps1
```

198. Look for: a block headed `The viewpoint collection, every member`. If it says `UNKNOWN: no type named ...DocumentSavedViewpoints`, that name is wrong and F52 cannot be finished until the right one is found, so send the whole output either way
199. Look for, in the same output: whether `DocumentSavedViewpoints` carries `RootItem` and `AddCopy`, the two members the sets collection has. Of those two only `RootItem` is USED today, by `src\Federator.Addin\Engine\SavedViewpoints.cs`, which is the only file that would fail to build without it. `AddCopy` is what the creation will need and nothing calls it yet, so a missing `AddCopy` breaks no build and changes what F52 can be finished with. Both answers matter and neither is a failure of this probe
200. Paste both outputs into the chat, or into `docs\history\scan.md` under sections 5a and 5b where they say NOT MEASURED, with the date. Those two sections record the questions and deliberately hold no answer

## Proof F51, the NWD carries May be re-saved and its properties reach ACC

Every NWD this tool has published so far shows a processing error beside it in ACC and
in Forma. Autodesk say an NWD published without May be re-saved cannot be translated by
that viewer, and they have a second article about an NWD reaching ACC with no properties
and every object showing as solid. F51 sets three things on the publish that were never
set: `AllowResave` true, `EmbedDatabaseProperties` true and `PreventObjectPropertyExport`
false. All three were already on the list read off the DLL on 2026-08-29.

This proof needs one NWD and an upload. It is the only thing in this file that needs a
browser as well as Navisworks.

201. Run any one building the ordinary way, so a fresh NWD is published
202. Look for: one line in the log reading `NWD      publish properties set:` and then seven names, ending `AllowResave=true, EmbedDatabaseProperties=true, PreventObjectPropertyExport=false`. If that line is missing, the build is older than F51 and nothing below proves anything
203. Open the published NWD in Navisworks on your own machine, then File, then look at the file properties
204. Look for: the NWD opens and the title, the publisher, the subject and the author read as the log line says they were set
205. Upload that NWD to ACC, into whatever folder the project uses
206. Look for: no processing error and no warning triangle beside it once the translation finishes. That is the whole point of `AllowResave`. A warning still there means the flag did not take and the answer is a screenshot of the warning and the log line from step 202
207. Open the NWD in the ACC viewer and click one object, any object
208. Look for: a properties panel with real Revit properties in it, not one row reading Solid. Object properties missing means `EmbedDatabaseProperties` did not carry them and `PreventObjectPropertyExport` is the next thing to look at
209. Q32 is whether the NWF needs to be in ACC at all. Nothing appears beside an NWF because ACC does not translate one: an NWF holds no geometry, only pointers to the NWCs, so there is no viewable file to make. Answer it in `steps/02_questions.md` when you have seen the NWD work

## Proof F52, a viewpoint per discipline

Two halves. The first is true today and the second waits on the probe in the section
above, so do the first on the next ordinary run and the second only once the probe has
been run and the add-in rebuilt.

210. Look for, on any run since F52: a `VIEWS` line per group reading `N viewpoints planned:` and then the paths. The disciplines come SORTED, so a group of AR, ME and EL reads AR, EL, ME and plans FIVE and not three, `AR/AR only, EL/EL only, EL/Over 150mm/EL over 150mm, ME/ME only, ME/Over 150mm/ME over 150mm`, because F53 gives Mechanical and Electrical a sub group for their large items and Architecture has no pipe in it
211. Look for: a group with only one discipline STILL gets a line, never none. One AR group plans one viewpoint and one ME group plans two, its own and its large items sub group. A single discipline group planning NONE is the fault to watch for, and the log line is the evidence
212. Look for, while the probe is outstanding: the line under it reading `VIEWS    not attempted.` and naming section 5b and the probe. That is the honest case and not a failure. No group should be marked down for it, so check the GROUP finished lines still read `DONE`
213. Once the probe above has been run and the add-in rebuilt with what it found, run one building again
214. Look for: a `VIEWS` block in the shape of the SETS block, one `VIEW` line per viewpoint then `views created     :` and `put into the document:`
215. Open the NWF in Navisworks and open the Saved Viewpoints panel
216. Look for: one folder per discipline, named with the discipline code. AR holds one viewpoint. ME and EL each hold one viewpoint and one sub folder named `Over 150mm` holding one more. Press the plain one and look for that discipline showing and the others hidden
217. Run the SAME building again without changing anything
218. Look for, F28's rule carried to viewpoints: every line reads `already there, left alone, not made again` and `views created     : 0`. A second copy of any viewpoint is a fault

## Proof F53, the 150 mm rule and the sub groups

This one is proved in Core here and in the tree on your machine. The Core half needs
nothing from you. The rest waits on the viewpoint probe, because the sub group IS a
viewpoint and nothing can build one yet.

219. Look for, on any run since F53: the `VIEWS` planned line now carries a sub group path per Mechanical and per Electrical group, reading `ME/Over 150mm/ME over 150mm`. A group with no ME or EL in it has none, and that is right
220. Look for: the number in the folder name is the threshold in use. If you change the threshold and it still reads `Over 150mm`, that is a fault and the folder and the rule have drifted apart
221. Once the viewpoint probe has been run and the add-in rebuilt, run one Mechanical building
222. Look for: a `SIZE` block per group with three numbers, `over 150mm`, `not over`, and `size could not be read`
223. Look for, and this is the one most likely to be wrong: the line reading `N items are in the viewpoint because no size could be read off them. Nothing was dropped.` followed by every one of them named. That number is EXPECTED to be large, because a fitting usually carries no size property. A large number here is not a fault
224. Read a few of the names under it. If they are all fittings, elbows and tees, the rule is behaving. If real straight pipes are in that list, the size is on a property this tool is not looking for, and the answer is the property name so it can be added to the setting
225. Open the NWF and press the `ME over 150mm` viewpoint
226. Look for: large pipes, ducts and trays showing, small ones hidden, and the fittings still there. A fitting missing means the include on unknown rule is not working and the SIZE block is the evidence
227. Look for, on a document measured in FEET rather than millimetres: the same items in and out as the same building measured in millimetres. The conversion is what makes that true, and a difference means the raw number is being compared somewhere

## Proof F54, clashes that cannot be solved become Reviewed

Only the part that needs no answer is built, so only that part can be proved. How the
tool learns WHICH clashes cannot be solved is Q33 and nothing supplies a list yet, so no
run today changes a single status. What can be checked is that nothing changed one by
accident and that the words are right.

228. Run any building the ordinary way
229. Look for: NO `STATUS` line anywhere in the log. Nothing supplies a list while Q33 is open, so a `STATUS` line on an ordinary run means something is supplying one and that is a fault. A REBUILT group writes a `RESULTS` line, which is F50 counting the clash results it carried across the clear and is nothing to do with F54. The two used to share the STATUS prefix and F56 separated them, because one prefix reading as two different things is how a log stops being trusted
230. Look for: the clash counts per test in the workbook still read New, Active, Reviewed, Approved and Resolved as they always did, and no clash has moved
231. Open `docs\workflow.md` and read the section What a status means
232. Look for: it says a clash carries five and a test carries four, that Old is a test word and never a clash word, and that this tool sets Reviewed and nothing else. If Q33 is answered and that section still says nothing supplies a list, the doc and the code have drifted
233. When Q33 IS answered and the input is built, run one building and look for a `STATUS` block of five lines naming the five and the four, written once per run
234. Look for: one line per clash moved, reading the clash name then the old status then the new, and one line per name it could not find
235. Look for: a second `NWF      attempt` line after the clash step, because writing a status is a write and the NWF is saved again on it
236. Look for: the workbook shows the NEW status for every clash that moved. If it shows the old one, the status is being applied after the harvest instead of before it, which is the one thing this feature must not do

## The log round, F59 to F64. Every step named and timed

Nothing here needs a new folder or a new file. It is all read off the log of an ordinary
run, so do it on the next building you run for any other proof in this file.

237. Run any one building the ordinary way, with the clash XML picked
238. Open the log for that run
239. Look for, F59: a pair of lines per step, reading `STEP     ` then the step name then `started`, and later the same name then `finished`, the seconds and a few words for what it changed. The step names are `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `IMAGES`, `WORKBOOK`, `HTML`, `XML`, `NWD` and `CONFIRM`
240. Look for, F59: `STEP     TESTS RUN` appears ONCE as a started line and once as a finished line for the whole group, then one line reading `entered again. Every further visit in this group is counted, not written out`. A pair of lines per test would be 3660 lines for 1830 tests, which is the fault that once left a 17.8 MB log
241. Look for, F59: one line per repeated step at the end of the group, before the `GROUP    finished` line, reading the step name then the visits then the total seconds, like `STEP     TESTS RUN     1830 visits, 742.113s in total`
242. Look for, F59: `STEP       IMAGES` is indented two spaces further than every other step line. It is the only one that runs INSIDE another step, because a picture is written while the harvest is walking the results. `HARVEST` is NOT indented, because it opens and closes on its own
243. Look for, F59: NO line anywhere reads `NEVER CLOSED`. That line means a step was opened and nothing closed it, which would make every total after it wrong. If you see one, send the whole log
244. Look for, F59: the seconds on the `STEP     NWD     finished` line and the seconds between the NWD attempt and written lines are the same number. Two clocks reporting one piece of work differently is the thing this round removes
245. Look for, F60: a block headed `TIMING` and the building, straight after the `GROUP    finished` line of every group. Every step on its own row with its seconds and its share, slowest first, then a row reading `outside every step`, then `group total`
246. Look for, F60: the shares in that block add up to a hundred. They are meant to, because whatever the steps do not account for is a row of its own rather than a number left off the page. A column adding to less than a hundred means a row is missing and is worth sending
247. Look for, F60: the `group total` on that block is the SAME number as the seconds on the `GROUP    finished` line above it
248. Look for, F60: a line reading `inside another step, so these seconds are already counted above` at the bottom of the block, with `IMAGES` under it. Those seconds are inside `HARVEST`, so they are deliberately left out of the share column
249. Look for, F60: a block headed `TIMING, THE WHOLE RUN` just before the `RESULT` block at the end of the log. It reads the groups slowest first, then the run total, then the SAME steps added across every group, which is what answers which step costs the run rather than which building
250. Look for, F60: the last line of that block reads `The run took` and then the time in minutes and in seconds, and then either `inside the 45 minutes 0 seconds a run has to finish in` or `OVER` it. That sentence is criterion 2 answered by the log itself. Send it either way
251. Look for, F21 which closes with F60: one line per test reading `ROWS     ` then the test name then `N rows for the workbook, N clashes in the document`. Where the two agree it says `they agree`. Where there are FEWER rows than clashes it names the result groups as the reason, which is right, because the workbook and the Clash Detective panel both show one row per group
252. Look for, F21: NO line anywhere reads `THERE ARE MORE ROWS THAN CLASHES`. Nothing in this tool produces more rows than clashes, so that line means something is wrong and the whole log is worth sending
253. This is criterion 3 without opening Excel. Pick any test whose `ROWS` line says they agree, open the workbook at that test's block and count the rows, then open Clash Detective on the same test and read the panel count. All three should be the one number
254. Look for, F61: a pair of lines around each step reading `CENSUS   before ` then the step name, and `CENSUS   after  ` then the same name, each carrying five counts: `models`, `sets`, `tests`, `results` and `views`
255. Look for, F61: the `views` count reads `0` on every line, not `UNKNOWN`. `SavedViewpoints.Count` walks the saved viewpoints and returns minus one only where it could NOT count them, and minus one prints as `UNKNOWN`. An `UNKNOWN` here means the viewpoint collection is not the shape `SavedViewpoints.cs` assumes, which is the F52 measurement, and the whole log is worth sending
256. Look for, F61: NO line anywhere begins `CENSUS CHANGED`. That line means a count moved during a step that may not move it, which is a real fault, and the group carrying it will read FAILED rather than DONE. If you see one, send the whole log
257. Look for, F61: the counts move where they should. `sets` goes up across `SETS`, `tests` goes up across `TESTS CREATE`, `results` goes up across `TESTS RUN`, and `models` goes up across `APPEND`. None of them moves across `NWD`, `WORKBOOK`, `HTML`, `XML` or `CONFIRM`
258. Look for, F61: `CENSUS   before TESTS RUN` appears ONCE per group and not once per test. The census is taken around the first visit of a step and no more, because counting the whole document 1830 times would be the log making the run slower
259. Look for, F61: one line per group reading `CENSUS   cost ` then the seconds then the number of counts. This is the measurement of what the census itself costs, and it is the number that decides whether it stays wide
260. Look for, F61: that cost line says `inside the 1.000s a group is allowed`. If it says `over`, the census narrows from the next group on and the next group says `CENSUS   narrowed` at its top. Both are working as intended, but send the number either way, because it is the first real measurement of what the census costs on a real model
261. Look for, F62, while the run is WORKING and not after it: the line just above the log box reads `Group 3 of 14`, then the building, then the step, then how long that step has been going and how long the run has, like `Group 3 of 14  1B06PH  TESTS RUN  12s on this step  4m 02s on the run`
262. Look for, F62: that line sits on its own row above the log box. It has to stay put while the log box scrolls itself. If it is beside the buttons and cut off at the window edge, the build is older than F62
263. Look for, F62: the log box keeps scrolling to the newest line by itself the whole time, and the line above it never moves out of sight
264. Look for, F62: the line changes as the step changes. Watch one group through and you should see `DECIDE`, `APPEND`, `NWF SAVE`, `UNITS`, `SETS`, `TESTS CREATE`, `TESTS RUN`, `HARVEST`, `WORKBOOK`, `HTML`, `NWD` and `CONFIRM` go past
265. Look for, F62: inside the clash run the seconds on the run keep counting up while the tests go by. It is rendered at most once a second on purpose, so it should look like a clock and not like a flicker
266. Look for, F62, on the SECOND group onward: if a step runs over twice as long as the same step took on the group before, the line says `SLOWER, the group before took` and the time. On the first group it never says it, because there is nothing to compare against
267. Look for, F62: while the NWD is publishing the line stops counting and sits at the seconds it last showed. That is expected and it is written down: publishing is one Navisworks call with no loop inside it, so there is nothing to tick from without starting a thread, and nothing here starts one. If the line went blank instead, that is a fault
268. Look for, F63: a block headed `GAP` and the building at the END of every group, after every output has been written. That is the point: it answers what the outputs do NOT carry
269. Look for, F63: it lists `Family`, `Type Name`, `Material`, `Source File`, `Discipline` and `Id From`, each with how many item cells carried it out of how many there were, and where it would belong. These are read off every clash item on every run and reach no output at all
270. Look for, F63: the first line of the block says `nothing acts on it`. A gap is a question and never a fault. No group should read anything but DONE because of one
271. Read the counts on those six lines. They are the first real measurement of how much of each property a real model actually carries. `Material` is expected to be low and `Family` high, and if `Material` is at zero it may be worth deleting rather than keeping, which is Q37
272. Look for, F63: one line at the end of the run reading `GAPS     ` then a number then the names. It counts by NAME and not by line, so a run of fourteen groups says six and not eighty four
273. The six are now Q35 to Q40 in `steps\02_questions.md`, one per property, because one answer for five different properties is a decision nobody can make. Answer them with the counts from step 271 in front of you

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

274. In the VS Code terminal, in the repo folder, run:

```
git ls-files --eol .claude/hooks .githooks
```

275. Look for: three lines, each reading `i/lf` and `w/lf` and `attr/text eol=lf`. A `w/crlf` on any of them means this checkout still holds the old copy, so run `git add --renormalize . ; git checkout -- .` and read it again
276. Switch the test wall on, which git needs told once per clone:

```
git config core.hooksPath .githooks
```

277. Look for: `git config core.hooksPath` answers `.githooks`
278. Make a branch, change one word in `steps\log.md`, and commit it from the VS Code terminal rather than from GitHub Desktop, so you see what the hook prints
279. Look for: the commit pauses and prints `pre-commit: running the full test set`, then `pre-commit: tests passed`, and only then commits. That is the test wall. Throw the branch away afterwards
280. Open Claude Code in this folder and ask it to write one word into any file under `samples`
281. Look for: it comes back refused, with the line `Refused. ... is under samples, steps/logs or bundle, which are never edited.` That is the paths wall, and the branch wall is the same hook file beside it, proved the same way by asking it to commit while main is checked out

## Delete the old branches, D6

Every branch except main is merged into main. The container cannot delete a branch: `git push origin --delete` comes back HTTP 403 from the proxy in front of it, and there is no GitHub tool in it that deletes a branch. So this is yours, one command from the repo folder in the VS Code terminal.

The list below was read on 2026-09-18 with the command in step 282, after the last merge of the feature round F51 to F55, and `git ls-remote --heads origin` gave 49 names. The branch this round's own closing pull request came from, `round-close-4`, is in the delete list too and was not on the remote yet when the list was read, which makes 50 lines and 49 to delete by the time you run it. Read the live list again yourself before you delete, because a branch may have come or gone since. Do not build the list from `git branch -r`. That prints remote-tracking refs your clone remembers, and a branch deleted by someone else is still in it until you prune, which is how a name that does not exist on the remote reached this file once already. The container's own clone showed it again on 2026-09-18, still holding `origin/claude/parsons-nwc-analysis-rlzgdr` after the remote had lost it. `git ls-remote` asks the remote and remembers nothing.

282. Read the live list:

```
git ls-remote --heads origin
```

283. Look for: one line per branch, the name after `refs/heads/`. Expect 50 of them, so 49 to delete. It was 43 after the third audit round, and the feature round added six fix branches and its closing one
284. Delete every one of them except main:

```
git push origin --delete analysis-pass fix-F16 fix-F27 fix-F28 fix-F29 fix-F30 fix-F31 fix-F32 fix-F33 fix-F34 fix-F35 fix-F36 fix-F37 fix-F38 fix-F39 fix-F40 fix-F41 fix-F42 fix-F43 fix-F44 fix-F45 fix-F46 fix-F47a fix-F47b fix-F47c fix-F50 fix-F51 fix-F52 fix-F53 fix-F54 fix-F55 fix-f1-f2-f4-small fix-f10-gate-outputs fix-f11-dead-code fix-f17-picture-order fix-f20-tests-on-push fix-f22-two-workflows fix-f24-rebuild-changed-nwf fix-f26-units-meters fix-f5-sets-built fix-f6-open-file-folder fix-f7-open-file-result fix-f8-run-saved-tests fix-f9-changed-skip-units master round-close round-close-2 round-close-3 round-close-4
```

285. Look for: one `- [deleted]` line per branch and no error
286. Run `git ls-remote --heads origin` again and look for: one line, `refs/heads/main`. If a branch you did not expect is there, it was pushed after the list above was read, so read what it holds before deleting it
287. Run `git fetch --prune` so your own clone forgets the branches that are gone. Without it `git branch -r` keeps printing them
288. If the command refuses a branch, open github.com, the repo, Branches, and press the bin icon beside every branch that is not main
