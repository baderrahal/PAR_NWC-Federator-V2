# 03 Bader next

**The last run, 2026-09-07 09:34, was on the old build be0b9b37 of 1 Sep. Twenty fixes have merged since, F5 to F38. Pull main and build before anything.**

One action per step. Do them in order. One build, one install and one Navisworks session cover every proof: the window checks first, then the C06 rebuild run, then one building twice, then the rest.

F34 comes first, because it is the largest add-in change of the round, the window, the XAML, the engine and the job all changed, and it is proved by opening the window and looking, before any run. If the window does not open or the Clash step is wrong, nothing after it can be read, so it is checked before a single group runs. F33 is checked in the same look, because it is one combo on the same window. F27 is next, because it costs one Scan and a Cancel. Then the C06 run, which proves F24, F29, F30 and F33 in one press, then F28 and F31 together, because both are one building run twice. F32 and F35 need a different file open or a folder made for them, so they come after the ordinary runs.

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
32. Untick two groups, 1B06PH and 1B06G1
33. Press Run
34. Press Cancel in the confirm dialog
35. Look for: the GROUPS block in the log lists 1B06PH and 1B06G1 with `unticked`, every other group with `run`, and the word `skipped` nowhere
36. Look for: one line after the list reads `2 groups unticked in the Run column, nothing else drops a group`
37. Tick 1B06PH and 1B06G1 again

## Proof F24, the six CHANGED groups are rebuilt from the scan, with F29 and F30 in the same run

38. Leave the clash XML box empty
39. Tick every group and press Run
40. Look for: the label at the bottom reads Checking NWF 1 of 14 and counts up, then the confirm dialog opens
41. Look for: the dialog says `Rebuilt: 6` and that the NWF is cleared and rebuilt from the scan folder with its saved tests kept
42. Press Cancel
43. Look for: the log says `Run cancelled before anything was cleared.`
44. Go to the Grouping step
45. Look for: 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE read Rebuilt in the Run as column, and the other eight read Weekly run
46. Press Run again, then OK, and wait for it to finish
47. Look for: per rebuilt group the log has a CHANGED line with the counts, then a REBUILT block, `REBUILT <nwf>  1 added, 4 moved, 0 removed` for 1B06BC, then one line per file, then one `saved tests` line
48. Look for: the `saved tests` line says `kept` or `none`, never `LOST`
49. Look for, F29: per rebuilt group one `SETS` line with three numbers, read before the clear, after the appends and after the copy is put back, and the first and the third agree
50. Look for, F33: per group a `UNITS` line saying how many models were set and what the document shows, in the words `model set` or `models set`, and no line anywhere says DID NOT FOLLOW
51. Look for, F30: per group, after the models are in, the blocks come in this order: UNITS, then SETS and CLASH, then `NWF      attempt`, then XLSX, then NWD, then the final NWF line saying intact and the size
52. Look for: the GROUP finished line of each of the six ends with `DONE` and `Rebuilt`
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
88. Look for: the GROUP finished line ends with `DONE`
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
113. Press Scan again and tick 1B06PH only
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
132. Look for: the pictures rendered again, the CLASH block counts them

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
192. Look for: a block headed `ITEM IDS 1B06PH`, one line per property that supplied an item id with a count out of the total, and never a line per item. On a Revit sourced model expect `Element ID` to supply most of them
193. Look for, if any `CLASH    OLD` line appears: it reads the test name and `Navisworks has this test marked Old.` and nothing after that. It used to carry a sentence about what Old means, which is UNKNOWN
194. Look for, only if you tick compacting on the Clash step: the compacted line carries both numbers, the Resolved count before and the count read again after, and the number reported as removed is the difference between them

## Delete the old branches, D6

Every branch except main is merged into main. The container cannot delete a branch: `git push origin --delete` comes back HTTP 403 from the proxy in front of it, and there is no GitHub tool in it that deletes a branch. So this is yours, one command from the repo folder in the VS Code terminal.

The list below was read on 2026-09-12 with the command in step 195, which asks the remote what it holds right now. Read it again yourself before you run the delete, because a branch may have come or gone since. Do not build the list from `git branch -r`. That prints remote-tracking refs your clone remembers, and a branch deleted by someone else is still in it until you prune, which is how a name that does not exist on the remote reached this file once already. `git ls-remote` asks the remote and remembers nothing.

195. Read the live list:

```
git ls-remote --heads origin
```

196. Look for: one line per branch, the name after `refs/heads/`. On 2026-09-12 there were 30 of them, so 29 to delete
197. Delete every one of them except main:

```
git push origin --delete analysis-pass fix-F27 fix-F28 fix-F29 fix-F30 fix-F31 fix-F32 fix-F33 fix-F34 fix-F35 fix-F36 fix-F37 fix-F38 fix-F39 fix-f1-f2-f4-small fix-f10-gate-outputs fix-f11-dead-code fix-f17-picture-order fix-f20-tests-on-push fix-f22-two-workflows fix-f24-rebuild-changed-nwf fix-f26-units-meters fix-f5-sets-built fix-f6-open-file-folder fix-f7-open-file-result fix-f8-run-saved-tests fix-f9-changed-skip-units master round-close
```

198. Look for: one `- [deleted]` line per branch and no error
199. Run `git ls-remote --heads origin` again and look for: one line, `refs/heads/main`. If a branch you did not expect is there, it was pushed after the list above was read, so read what it holds before deleting it
200. Run `git fetch --prune` so your own clone forgets the branches that are gone. Without it `git branch -r` keeps printing them
201. If the command refuses a branch, open github.com, the repo, Branches, and press the bin icon beside every branch that is not main
