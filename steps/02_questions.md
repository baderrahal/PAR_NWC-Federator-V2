# 02 questions

Every question for Bader in one list.
Answered by Bader on 2026-09-06. The answer sits under each question.
Questions 16 to 19 were not in the first list. Bader answered them anyway and they are recorded here.

1. Which report shape is the ask. The brief says one sheet per test, all columns, images included. The repo says one sheet laid out as the client's HTML report, with the per test sheets, the Summary and the Matrix removed on purpose. Which one holds. See L8

   Answer: the report is the same as the Navisworks HTML tabular export dragged into Excel and saved. The repo shape holds.

2. Open file with no clash XML. CLAUDE.md says it runs the tests already in the document. Today it runs nothing. Should the worker make it run them, F8, or change the words

   Answer: yes. With no clash XML the tool runs the tests already saved inside the NWF, on both the scanned path and the open file path. F8 goes in.

3. Which six buildings are the done set. The last real folder had 22 groups. The done criterion says every ticked building, the brief says 6

   Answer: all buildings. The log must answer timing and counts.

4. May the dead code go. `ClientColumnsOnly`, `SheetNames.SummarySheet`, `SheetNames.MatrixSheet`, `ClashMatrix`, `TestReport.SheetName` and their tests. See B9 and F11

   Answer: yes, delete all dead code.

5. May CLAUDE.md be split into `.claude/rules`. Section 8 of `00_analysis.md` has a shape. Yes, no, or a different shape

   Answer: yes, split CLAUDE.md into rules.

6. Branch and merge. This pass used `analysis-pass` and merged it as asked. For the fixes, one branch per fix, or one branch for F1 to F7 together

   Answer: one PR per fix, F1 to F4 together in one PR, worker merges its own PR.

7. Is there a timing baseline. Any log from a run over all buildings on the current code. `docs/scan.md` has one clash step at 39.8 s and another at 1259.5 s with no cause found

   Answer: no baseline exists, the log must carry it.

8. Picture numbering. Ours counts tests in run order, the client's counts them by block in the report. The pictures still link right. Leave it, or match theirs, F17

   Answer: match the Navisworks export order, F17 goes in.

9. The 1A04WE export. The docs name it as the one with the Layer column. It is not in `samples/client-report`. Can it be committed

   Answer: Bader may upload a sample into samples/client-report later.

10. CI for the add-in. It cannot build without Navisworks. Is there a Windows machine with Navisworks that can be a self hosted runner, or does the add-in build stay local only

   Answer: add-in build stays local, F19 dropped.

11. Tests on Linux. 40 of 868 fail in the container on Windows path assumptions. Make them path neutral, F16, or accept that tests run on Windows only

   Answer: make the tests path neutral, F16 goes in.

12. Units on a CHANGED group. Today the units are still applied. Skip it, F9, or is applying them to a group nobody saves harmless enough to leave

   Answer: skip the units change on a CHANGED group, F9 goes in.

13. The confirm dialog. It says the document is cleared before each group. On a rerun it is not. Say it only for groups with no NWF yet, or drop the sentence

   Answer: say cleared only when it is true.

14. Dispose in `SetBuilder` and `ClashRunner.Resolve`. Both leak wrappers against the rule. Fix now, F15, or wait until a run shows it costs something. It is the riskiest change in the list

   Answer: fix now, F15 goes in, last of the code fixes.

15. Who runs the local proofs P1 to P3, and when. The worker cannot. Every done criterion waits on them

   Answer: Bader runs the proofs on his machine and drops logs into steps/logs.

16. Not in the first list. Branch and merge again.

   Answer: one PR for F1 to F4.

17. Not in the first list. Is there a weekly button.

   Answer: no weekly button exists, both paths must be right.

18. Not in the first list. A workflow on push to main.

   Answer: yes, add a push to main workflow. F20.

19. Not in the first list. Which fixes go in.

   Answer: fix everything, in the order in `01_next.md`.

20. B12, the NWD opened from ACC that showed an error beside the file. Four things are not known

   - the exact text of the error
   - where in the window it appeared
   - the path of the file as Navisworks shows it
   - whether Navisworks opens that file on its own with the add-in closed

   Answer:

21. Not in the first list. Skipped groups. The run log from 2026-09-07 dropped 12 of 26 groups before the run started, every group without AR, EL, ME or ST, and the rule is not shown in the window. See B15

   Answer: federate what Bader ticks. Every ticked group gets its NWF and NWD. Clash only where the group holds two or more disciplines. No hidden discipline rule. F25.

   Closed on 2026-09-12 by F27. The rule was never in the code. The GROUPS block printed skipped for a group unticked in the Run column, and now prints unticked with one line counting them. Clash only where two disciplines meet is F35.

22. Not in the first list. CHANGED. Six groups in the same log had an NWF built from an older folder with fewer files, and the tool left them alone. See B13

   Answer: rebuild the NWF from the scan folder by itself, keep the tests saved inside it, and say in the log what was added, what moved and what was removed. F24.

23. Not in the first list. Units. The same log says THE DOCUMENT DID NOT FOLLOW on 11 of 14 groups and the report went out in feet. See B14

   Answer: the report is always in meters, never feet. Force the document to meters and fail the group loudly if it will not follow. F26.

24. From the audit of 2026-09-12. The rules say a name cell typed over can be given back to the pattern, and `OutputNameRow.ReleaseToPattern` does that, but no control in the window reaches it, so nobody can. Wire a way in the Grouping step, a right click or a button per cell, or drop the sentence and the member with its test? See `04_audit.md`, F40

   Answer:

25. From the audit of 2026-09-12. Family, type name, material, source file and discipline are read off every clash item and written to no output, because the workbook became the client's one sheet with none of ours on it and the page never carried them. They cost a property lookup per item per run. Delete them and their harvest, or keep them for a workbook column that does not exist yet? See `04_audit.md`, F40 and F45

   Split on 2026-09-19 by F63, which made the run report these itself in the GAP block. One answer for five different properties is a decision nobody can make, so they are asked one at a time as Q35 to Q39, and Q40 is a sixth this question never covered. Answer them there. This one stays because the reasoning behind all five is here.

   Answer:

26. From F40 on 2026-09-12. The rule says a public member nothing in src calls is deleted with its tests. Thirty of them are read only by a test that pins a rule the repo states: the reference file holds one batchtest and 1830 tests, 61 distinct locators, linkage none and rules empty in every test, two set names ending in a space, the stamping actually ran, the picture name round trips, the report check names what it found. Deleting the member deletes that proof. F40 made those members `internal` instead, so they are off the public surface and the tests still reach them through the `InternalsVisibleTo` the Core project already carries. Is that the right answer, or should the member and its test both go? See the list in the F40 pull request

   Answer:

27. From F40 on 2026-09-12. The two choice rule for what counts as still outstanding has lost every reader. The Summary and Matrix sheets went, D3 took the window setting, and F40 deleted `OpenClashes.All`, `Default`, `Describe` and `Of` along with `TestReport.OpenUnder` and `NewPlusActive`, which were the last callers. What is left is the enum and `StatusesFor`, which the image status filter reads to ask for what Navisworks counts as open. Should the rule stay in `.claude/rules/core.md` as a rule with one reader, or is the choice gone for good and the bullet with it?

   Answer:

28. From F41 on 2026-09-12. Two handle reads were left where they are, because neither is in the F41 list and neither is covered by what the history file measured. `search.Selection` in `SetBuilder`, read once a set to call `SelectAll` on a `Search` this tool owns. The source read out of a side's own collection in `ClashRunner.LocatorOf`, which sits inside the loop over the 61 indexed sets, so one side costs up to 61 reads of the same source. The measured rule says releasing a wrapper releases the wrapper and never the document's object, but it was measured on items read out of a document collection, not on a sub object read off a handle this tool created. Release both the same way as the rest, or leave them until the ownership of a sub object is measured off the installed DLL?

   Answer:

29. From F43 on 2026-09-12. The stop after count, the log count and the report subfolder are settable properties now, each with the default it always had, and nothing outside the code sets any of them. A property can be changed where the code reads it, which still means a recompile, and the rule says these numbers can be changed without one. A settings file beside the logs, read once when the log opens and written by nothing, would do it with no new box in the window. Is that wanted, and if it is, which numbers go in it and what happens when it holds a value the code refuses?

   Answer:

30. From F44 on 2026-09-12. `bundle\ParsonsNwcFederator.bundle\PackageContents.xml` line 9 points at `docs\scan.md`, which moved to `docs\history` in F37. It is the one file left in the repo carrying that path. CLAUDE.md called the bundle folder build output written by the build, and it is not: it holds that one hand written manifest, `install.ps1` copies it into `artifacts`, and `artifacts` is what the build writes. F44 corrected the description and left the file alone, because the rule and the hook both say the folder is never edited. May the manifest be changed on its own, now that it is known to be hand written rather than generated, or does the rule stand and the stale path with it?

   Answer:

31. From F47c on 2026-09-18. The F40 entry says 45 members went outright, and five of the names in that list did not go under F40. `WorkbookWriter.ClientColumns` was never touched and was still public until this fix deleted it. `ReportOptions.FolderFor` and `BuildingGroupingResult.Find` were made internal by F40, so they belong in the 64 taken off the public surface and not in the 45. `ClashReportXml.QuickProperties` was made internal by F44 and `ClientFormat.DistanceFormat` was deleted by F44, so neither is F40's work at all. Each of the five is marked in the list and the entry now carries a line saying so. What the entry cannot say is the true count, because the 45 and the 64 were counted by a rule this round did not have. Counting every member declaration the F40 commit removed from src and did not add back gives 53, but that counts three private backing fields and one constructor that the list only mentions in passing, so it is a different rule and not a correction. Should the two figures be remeasured and restated, and if so by which rule, or does the marked list stand as the record with the figures left as they were written

   Answer:

32. From F51 on 2026-09-18. Every NWD this tool publishes showed a processing error beside it in ACC and in Forma, and F51 fixes that by setting `AllowResave`, which is what Autodesk say a viewer needs. The NWF is a different matter. ACC does not translate an NWF at all, because an NWF holds no geometry, only pointers to the NWC files, so there is no viewable file to make from one. Nothing beside an NWF up there is a fault. What the NWF IS good for up there is being the record: it carries the clash tests, the clash results and the statuses a person set by hand, and it is the only copy of any of that. Does the NWF need to be in ACC at all, as a backup of the record and a file another person can open in Navisworks, or is it a local file that never leaves the project drive and only the NWD goes up

   Answer: Bader, 2026-09-20. CLOSED. Nothing goes to ACC until the tool is finished and its bugs are fixed, so the question does not arise yet. The NWF stays on the project drive. The NWF size question that waited on this is closed with it, which is why the dimming round taking one group from 119,542 bytes to 23,327,744 is a question about the run and not about what ACC would do with it.

33. From F54 on 2026-09-18. A clash that cannot be solved should end up Reviewed rather than sitting at New or Active forever. The tool can set a status, `DocumentClashTests.TestsEditResultStatus` is measured and the method that applies it is written. What is missing is the input. How does the tool LEARN which clashes cannot be solved. Four shapes it could take, and each means a different feature: a list of clash names Bader supplies per run, a rule over the clash itself such as a distance under some number or a pair of disciplines that always overlap, a status carried in the clash XML that gets applied on import, or a person marking them in Navisworks once and the tool leaving them alone after. Which of those is the ask, and if it is the second, what is the rule

   Answer: the SECOND shape, a rule over the clash itself. Answered on 2026-09-19 and carried out by F72. A clash becomes Reviewed when all four are true: one side is a service by item category, the other is a solid by item category, the service measures 150 mm or less, and the clash is at New or Active. The other three shapes are NOT chosen and are recorded as such: no list is supplied per run, nothing is read off a status in the clash XML, and nothing watches what a person marked last week. Q41 to Q44 answer the four details this raised.

34. From F50 on 2026-09-18. Whether one model can be taken out of an open document without clearing the whole thing was never measured, so F50 kept the clear and restore and widened it from two things to four. `tools\probes\probe-model-remove.ps1` answers the question. If the answer comes back yes, the rebuild could instead append what the scan has and the NWF does not, remove what the NWF has and the scan does not, and never clear at all, which would mean nothing needs putting back and nothing can be lost. That is a rewrite of the rebuild rather than a tweak, and the widened version now works and is tested. Once the probe has answered, is the rewrite wanted, or does the counted clear and restore stay because it is proved and the risk of changing it outweighs the tidiness

   Answer: Bader's brief of 2026-09-20 asked for the rewrite ONLY IF the measurement allowed it, and said in capitals that if it cost anything, nothing was to be built. 5x measured it and IT COSTS NOTHING, so THE REWRITE IS BUILT, in PART 5 of the drift round.

   WHAT 5x FOUND, on 1A02MM, four models, 526 results and 509 viewpoints. Removing a model left the sets at 62, the tests at 1830, the results at 526, the statuses at 526 and the viewpoints at 509, and every one of them came back whole through a save and a reopen off the disk. `TryRemoveFile` returned true and nothing that pointed into the removed model went with it.

   SO A CHANGED GROUP IS NO LONGER CLEARED. `FederationEngine.ReshapeFromScan` takes out what is gone and appends what is new, and the sets, the tests, the results, the statuses and the viewpoints never leave the document, which makes the whole copy out and copy back dance unnecessary.

   THREE THINGS THE ANSWER DELIBERATELY KEEPS. The count out and the count back stays EXACTLY as it was, because it is what proves nothing was lost and a path that needs no copying still has to prove that. The clear and rebuild stays as the FALLBACK for a shape the reshape cannot do, and the reshape declines BEFORE it changes anything, so the fallback always reads a document nothing has touched. And a file is removed by its NAME and from the END, never by a remembered index, because 5x removed the LAST model of four and whether removing a middle one shifts the indexes after it is UNKNOWN.

35. From F63 on 2026-09-19. The GAP block. Family is read off every clash item by `ClashHarvest` and reaches no output: not the workbook, which carries the client's fifteen columns and nothing of ours, not the page, which is theirs, and not the clash XML, which writes two quick properties and says so in its own comment. It costs a property lookup per item per run. Q25 asks about all five together and this asks it for one, because a single answer for five different properties is a decision nobody can make. Keep Family for a workbook column of ours, or delete it and its harvest

   Answer:

36. From F63 on 2026-09-19. The same question for Type Name, which is the REVIT type name and not the client's Item Type column. Their Item Type reads Solid on every one of the 120 item cells in the accepted report, so the two are different things and dropping ours changes nothing on their page. Keep it for a workbook column of ours, or delete it and its harvest

   Answer:

37. From F63 on 2026-09-19. The same question for Material. Of the five it is the one most likely to be empty on a real model, because it is read off the item and a great many items carry none, and the GAP block says how many of the item cells actually had one. Keep it for a workbook column of ours, or delete it and its harvest

   Answer:

38. From F63 on 2026-09-19. The same question for Source File. This one is different from the other four: the tool already reports the Revit source per NWC in the SOURCE FINDINGS block, so the building level answer is in the log and this is the per ITEM answer. Keep it for a workbook column of ours, delete it and its harvest, or is the SOURCE FINDINGS block enough

   Answer:

39. From F63 on 2026-09-19. The same question for Discipline. The discipline is already in every output name, in the grouping and in the viewpoint folders, all read off part 5 of the NWC name. This one is read off the ITEM, which can disagree with the file it came in. Keep it for a workbook column of ours, delete it and its harvest, or is it worth keeping only where it disagrees with the file

   Answer:

40. From F63 on 2026-09-19. Id From, which is not in Q25 and is a different shape from the other five. The client's cell says Element ID to match their own report, and the property the id ACTUALLY came from is held per item and written nowhere. Since F45 the ITEM IDS block counts it per PROPERTY with a total, which answers how a run behaved as a whole, and the per item answer is still held and still shown nowhere. Is the block enough, or does the per item value belong somewhere, and if so where

   Answer:


41. From F72 on 2026-09-19. Briefed as Q47 and recorded here as Q41, because this file runs to 40 and Q41 to Q46 do not exist. The solid side of a penetration. A service going through a WALL is the plain case. Does a service dropping through a FLOOR or up through a ROOF count as the same thing, or is the rule about walls alone

   Answer: floors and roofs count as well as walls. The default solid list is Walls, Floors, Roofs, and it is a setting. Carried out by F72.

42. From F72 on 2026-09-19. Briefed as Q48. Whether the solid side needs a discipline filter. A wall can arrive on an architecture file, a structural file or a coordination file, and reading part 5 of the NWC name would let the rule be limited to one of them

   Answer: NO discipline filter. ANY wall counts whichever file it came in. Nothing in the rule reads part 5 of a name and nothing in it knows what a discipline is, which is pinned by a test. Carried out by F72.

43. From F72 on 2026-09-19. Briefed as Q49. Which way round the 150 reads for a penetration. F53 puts an item in a viewpoint when it is OVER 150. Does a penetration mean a service over 150 or a service at 150 and under

   Answer: 150 OR LESS, so it is a CEILING and not a floor. A service over 150 stays at New, because a large service through a wall is a real coordination item. It is the same number F53 reads and the two read it in opposite directions with no gap and no overlap, which is written at the setting. Carried out by F72.

44. From F72 on 2026-09-19. Briefed as Q50. What happens when BOTH sides of the clash are a service. A pipe against a duct is not a penetration of anything, but it is also not obviously something to leave at New forever

   Answer: leave it alone. It is counted in the block under both sides a service, so a run says how many it saw rather than passing over them silently. Carried out by F72.

45. From F72 on 2026-09-19. The brief asks for the penetration count to go in the workbook, and it is there: the status is applied before the harvest reads it, so the Reviewed cell on every test header row already counts what this run moved, in the client's own column. What is NOT there is a count of ours saying how many THIS RUN moved, as against how many are Reviewed for any reason. The standing rule says the workbook is the client's one sheet laid out as theirs with none of ours on it, and that if it is not in theirs it is not in ours, so a column or a cell of ours would break it. The log carries the run total in the RESULT block and the per group detail in the PENETRATION block. Is the client's own Reviewed column enough, or does a cell of ours belong on that sheet after all, and if it does, where on it

   Answer:

46. From F77 on 2026-09-19. F77 vs the single discipline rule. Not creating a test whose side finds nothing means the NWF no longer carries every test in the matrix. The rule it replaces said every test is still created so the NWF is complete, matches the other groups, and a later run against a fuller model finds them already there. Under F77 that only holds while an XML is picked, because a weekly run with no XML creates nothing and the missing tests stay missing. Is 631 seconds a group worth that, or should a group that is going to be run weekly with no XML create the whole matrix once

    Answer:

47. From F72a on 2026-09-19. Four categories the service discipline sets ask for are NOT on the service list: Air Terminals, Mechanical Equipment, Plumbing Fixtures, Sprinklers. Mechanical Equipment and Plumbing Fixtures are plainly right to leave off, because an air handling unit through a wall is a real coordination item and not a penetration. Air Terminals and Sprinklers are arguable both ways. Which of the four are services

    Answer:

48. From F87 on 2026-09-19. The old reference file. `samples\1104-PAR_CLASH_AllInOne (2) (1).xml` is at 75 mm and 44 assertions across 7 fixtures measure it. `samples\1104-PAR_CLASH_AllInOne_25mm.xml` is the same matrix at 25 mm and is the source F87 corrects. Does the old file stay as the reference the tests measure, or does the 25 mm one become it, which means re-reading and rewriting 44 recorded measurements

    Answer: Bader, 2026-09-19. BOTH reference files stay. The 75 mm export, 1104-PAR_CLASH_AllInOne (2) (1).xml, stays as the reference the 44 assertions measure, and the 25 mm matrix, 1104-PAR_CLASH_AllInOne_25mm.xml, stays as the correction source. Both had been deleted through the GitHub website on 2026-09-19 and were restored out of git history in the viewpoints round, PART 0.

49. From F83 on 2026-09-19. F83 replaces a MEASURED order with a chosen one. The block order, most clashes first with ties in creation order, was read off both of the client's own exports over all 1830 blocks, and the tie rule is explicitly not alphabetical, including in one tie group of 1807. A priority file sorts A, then B, then C, then by test name, which is a different order from the one the client accepted. It only applies when a file is picked and the measured order is still the default. Is the client happy to receive the report in priority order, or is priority a column to sort on in Excel and the blocks stay where they were

    Answer:

50. From F72c on 2026-09-19. F72c widens one written guard. `StatusesThisToolMaySet` says Reviewed is the only status this tool ever sets, and an undo has to put a clash back to New or Active. It is written as `AllowsAsUndo`, a separate answer: an undo may set the exact status one of this tool's own records names in this file, and nothing else, so Approved and Resolved are still never set because no record can name one. Is that the right shape, or should an undo instead leave the status alone and only remove the record, so a person sees which clashes the tool had moved and decides one by one

    Answer:

51. From F85 on 2026-09-19. F53 and F72a read a size two different ways and F85 had to pick one. `SizeRule.Decide` takes the FIRST size property on the list, which is what a viewpoint wants, one representative number nobody argues about. `SizeRule.LargestMillimetres` takes the LARGEST, which is what a penetration wants, because a 600 by 150 duct has to fit a 600 through the wall. F85 reads a CLASH SIDE and so does F72a, so F85 reads it the way F72a does, the largest. Reading it the other way would let the same duct be set Reviewed by F72a as a large service and filed by F85 under Over 150mm, and the two rules would disagree about one clash with nothing saying so. Is the largest the right reading for a viewpoint folder as well, or should the two stay different and the difference be reported

    Answer:

52. From F87 on 2026-09-19, found when main was merged at the end of the round. TWO FILES CARRY THE NAME `1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` AND THEY ARE NOT THE SAME FILE. Bader uploaded one to `samples\` on 2026-09-19 while the round was being worked, and F87 wrote one to `exchange\` from the sample. They differ in exactly ONE line. Both correct the missing hyphen, all 121 occurrences. Only the one F87 wrote also makes `BLD-EL-Devices` ask for something other than Electrical Fixtures, which is the second half the brief asked for, so in the supplied file Devices is still the same set as `BLD-EL-Electrical Fixtures` and F84 still reports the pair. Neither file is deleted and neither is edited, because samples is evidence and exchange is proved by a test to be exactly what the rule produces, and `SuppliedCorrectedMatrixTests` pins the difference instead. Which of the two should the tool be pointed at, and should the one that is not it be renamed so two files of one name cannot be picked by mistake

    Answer: Bader, 2026-09-19. The exchange file is the one the tool is pointed at, and the supplied copy in samples is deleted. Done in the viewpoints round, PART 0, the one exception to samples being never touched, with SuppliedCorrectedMatrix() and its five tests gone because there is no second file left to pin a difference against.

53. From A16 of the audit, on 2026-09-19, judged in the wiring round. `PenetrationSettings.IsDecided` is named by the F72a rule in core.md as THE RULE, and its only caller is the test that asserts the decision against the client's matrix, `PenetrationRuleTests`. CLAUDE.md says a public member nothing in src calls is deleted with its tests unless a decision here keeps it. The other eight members the audit named gained a running caller when their fix was wired, and this one never will: no step in 03_bader_next.md calls it, and the probe's own membership test, `ProbeSettings.Asks`, reads a narrowable setting rather than the two default lists, so it is not the same question. Keep `IsDecided` as a stated rule with a test-only caller, or delete it and reword the F72a bullet to name the two lists and `PenetrationSettings.Names`

    Answer: Bader, 2026-09-19. IsDecided STAYS. It is a stated rule worth stating. One line above it says a test is its only caller and Bader chose to keep it, so the next audit does not raise it again.

54. From F76 on 2026-09-19, found on the wiring round's run. The line that says where every report row's tolerance was read came out as `TOLERANCE on the report, read off the clash test in the document for 1041, the clash XML for 17259, chosen in the tool for 0, UNKNOWN for 0, 18300 tests in all`. The 17259 are the rows of tests that never ran: not created because a side finds nothing, or already in the document with an empty side. Their row keeps the tolerance the plan gave it, read off the XML, because the document never produced that row. The rule as written says every row should read off the document and a count under any other origin is a report saying a number the run did not clash at. For a skipped row that is literally true and also the only honest number there is. Should a skipped row carry the XML value and its origin as now, carry no tolerance at all, or should the line count the skipped rows apart so the number that matters, rows that RAN and did not read off the document, stands on its own

    Answer: Bader, 2026-09-20. RIGHT AS IT IS, and CLOSED. The workbook keeps a block for every test in the matrix and a skipped row keeps the tolerance the picked file gave it. The client report shape does not change. The line the viewpoints round added, `WORKBOOK 18,300 test blocks, 1,041 ran and 17,259 are tests that never ran`, stays, because it is what made the split readable in the first place. Nothing more to build.

55. From the viewpoints round on 2026-09-20, docs\history\scan.md 5n. `ClashHarvest.SourceFileOf` reads `item.Model` off the clash leaf, and the probe measured that on a clash leaf `HasModel` reads false and `Model` reads null, the model sitting on the topmost ancestor six to nine levels up. On that evidence the source file column of the report, and the discipline read off it, come out empty on every row, and the wiring round's own note says the two came out empty on 426 items once before for a different reason. The viewpoint writer now climbs to the top and reads the file name there. Should the harvest climb the same way, which is one call in one place, or is the empty column what the client's report shows today and has always shown

56. From PART 6 of the viewpoints round on 2026-09-20, docs\history\scan.md 5i. The category list was measured off the ten C02 federations only, 374 values, and with it live the health block on the client's corrected matrix reads `Sets asking for a category no model carries: 14`, naming BLD-AR-Ramps, BLD-AR-Roofs, BLD-AR-Columns, BLD-AR-Casework, BLD-AR-Parking and nine more. Those are sets asking for a category no C02 building holds an item of, which is information about C02 and not a fault in the matrix. Should the walk be run over the other folders before the list is trusted project wide, and should the block say which folder the list was measured from so a reader does not take fourteen as a count of broken sets

57. From the viewpoints round on 2026-09-20, seen by hand on the sixth run's 1A02MM copy. A viewpoint carries the camera Clash Detective computes for its clash, `TestsViewpointForResult`, read back within 0.001 units, and the hidden state of every model outside its pair. Pressed, `AR vs EL` clash 15 opens on the cable tray meeting the wall, and `DR vs ST` clash 1 opens on the framing round the drainage pipe, both with the right models greyed. `DR vs ST` clash 2 opens on a uniform grey: the camera sits inside a structural member, which Clash Detective itself shows through because its own view dims or hides everything but the two items and reveals what is in the way. The viewpoint records no such dimming. The COM view carries `ApplyMaterialAttribs`, so a transparency put on everything but the two clashing items before the view is added would be recorded and applied when pressed, which is how Clash Detective looks and is another measurement and another run. Should a clash viewpoint dim everything but its two items, hide everything but its two items, or stay as it is with the pair's models shown in full

    Answer: Bader, 2026-09-20. DIM EVERYTHING THAT IS NOT THE TWO CLASHING ITEMS, the way Clash Detective does. Carried out by the dimming round on the same day, and the brief that asked for it called this question Q55 and the sighting under it Q56, which are two different questions in this file, so the numbers in that brief and the numbers here do not line up. This is the one it meant, by what it says.

    WHAT HE SAW, which is the only user report this feature has. He did step 381, opened the temp copy of 1A02MM the viewpoints round left at `C:\Users\bader\AppData\Local\Temp\claude\round-viewpoints\NWF6\1104-PAR-1A02MM-ZZZ-BM-MOD-000001.nwf`, and pressed two of its viewpoints. THEY DO NOT SHOW THE CLASH. The camera is the one Clash Detective computes and it reads back exact, the hidden state is right, and a person still cannot see the clash, because the camera lands inside a beam and the beam is solid. 975 viewpoints existed and none of them was usable.

    WHAT WAS BUILT. `SavedViewpoints.DimAllBut` overrides temporary transparency on the roots of the models the viewpoint shows and resets it on the two clashing items, so everything but those two goes to 0.85 and the two stay solid, and the COM view records it with `ApplyMaterialAttribs` beside `ApplyHideAttribs`, docs\history\scan.md 5o. The transparency is a setting, `ViewpointSettings.DimTransparency`, and 0.85 is CHOSEN and not measured because Clash Detective's own value is not readable off this API. Setting it to zero switches the dimming off and the viewpoints go back to what F85 shipped.

    THE TWO ITEMS ARE NOT COLOURED. Clash Detective paints them red and green. Whether a viewpoint should do the same is question 58 and was not decided here.

58. From the dimming round on 2026-09-20. Clash Detective paints the two clashing items red and green and dims everything else. The dimming round built the dimming and deliberately did not build the colouring, because which two colours a viewpoint should carry is a judgement about what the client's people read and not a thing this tool should decide. The COM view already records appearance overrides, `ApplyMaterialAttribs`, and `DocumentModels.OverrideTemporaryColor` is measured to exist beside the transparency call, so it is a small piece of work on a route already proved. Should a clash viewpoint colour its two items, and if so which colour goes on which side, the first locator or the second


    Answer: Bader, 2026-09-20. RED AND GREEN, like Clash Detective, the first item red and the second green, which is the order Clash Detective holds them so a person reading a viewpoint and reading the panel sees the same item in the same colour. Both are SETTINGS with those defaults, `ViewpointSettings.FirstItemColour` and `SecondItemColour`, in the file the viewpoint settings already live in. Done in PART 2 of the alignment round.

    THE READ BACK IS NOT WHAT IT LOOKS LIKE, and 5p is why. A viewpoint records a colour override only where the colour DIFFERS from the item's own. The second item of the first clash measured was already green, so painting it green recorded nothing at all, the viewpoint named 1,027 items where the blue version named 1,028, and pressing it still showed the item green because green is what it was. A read back insisting the viewpoint names both items would have failed a viewpoint that was perfectly right. So the read back asks what the viewpoint WILL SHOW for each item, which is the override's colour where it names the item and the item's own colour where it does not, and that has one right answer either way.

59. From the dimming round on 2026-09-20. The dimming works and it is not free, and both numbers are measured. The run over the ten C02 groups went from 5 minutes 3 seconds to 10 minutes 26, and the 1A02MM NWF went from 119,542 bytes to 23,327,744, which is 195 times, while 1A02WM went 44 times and the four small groups went 3 to 11 times. The cause is one number: a dimmed viewpoint records ONE MATERIAL OVERRIDE PER ITEM it dims, 5o, and 430 viewpoints over a group holding a few hundred visible items is a few hundred thousand records. The timing line says where the seconds go and 96 per cent of them are the recording, not the dimming or the read back. Three things could be done and none of them is mine to choose: leave it, because 10 minutes is well inside the 45 the criterion allows and the file only has to open. Turn the dimming off for the groups that cost the most, which is one setting. Or write fewer viewpoints, which means a cap and which this round was told not to do. Which

    Answer: Bader, 2026-09-20. A FOURTH THING, not one of the three: TAKE THE CHEAPER COM WRITE ROUTE AND MEASURE WHAT IT SAVES. The dimming round found `InwOpFolderView.SavedViews().Add`, one tree operation where the route it shipped costs three, and deliberately did not take it. Done in PART 1 of the alignment round, measured before it was switched to.

    WHAT IT ACTUALLY SAVED, 5p, which is not what the operation count suggests. Over twenty viewpoints of the same scene both routes record all four counts, the camera, the hidden state, the dimming and the two solid items, and pressing either leaves exactly the two clashing items solid. The cheap one took 153 ms against 158 and wrote 415 bytes MORE. Three per cent. The reason is that what grows is the TREE THE WRITE WALKS and not the number of calls per write: the same route costs 7.9 ms a viewpoint at twenty and 558 ms a viewpoint at four hundred and thirty. Both routes are in the one binary behind `ViewpointSettings.RecordsThroughTheFolder`, so the A against B on a real group is an honest comparison and a route that loses something on a shape not yet seen can be turned off without a build. The run total and the NWF sizes are in the alignment round's entry in `steps\log.md`.

60. From the alignment round on 2026-09-20. Every round so far has been proved against a COPY of Bader's NWF folder under the temp folder, because rule 1 says never write into a live project folder. That proves the tool writes what it says it writes and it does not prove the thing a weekly run actually does, which is open his own NWFs, add to the clash history already in them and save them back. Should the proving run keep going against a copy, or run for real against the live folders with a backup taken first

    Answer: Bader, 2026-09-20. FOR REAL, against the live folders, with the backup taken FIRST and READ BACK before anything else happens. Done in PART 7 of the alignment round. The backup matters more than usual here: the tolerance drop down at 25 mm resets every test in an NWF built at the SD stage's 75 mm and their results go with it.

61. From Bader's 422 hand marked decisions in 1A04PW, read on 2026-09-20. 33 of the 422 are electrical conduits passing through a structural foundation, which he marked Reviewed. That is a real penetration and the rule now moves it, Q63. The question was whether the 33 should also go to the structural engineer as a list, because a conduit through a foundation is a hole somebody has to cast in

    Answer: Bader, 2026-09-20. CLOSED. The 33 are Reviewed and that is the end of them. No list goes to the structural engineer. Nothing to build.

62. From Bader's 422 on 2026-09-20. 14 of the pairs he marked Reviewed are architecture meeting structure, which no rule in this tool covers: a floor finish lapping a structural wall, a ceiling board meeting a wall, a door frame in a structural wall, a wall sitting on a slab. Every one is a clash and none is a problem. Should they go into `samples\by-design-pairs.csv`, which is rule B and a list rather than a judgement, or does architecture against structure need a rule of its own

    Answer: Bader, 2026-09-20. INTO THE BY DESIGN FILE. He replaced `samples\by-design-pairs.csv` himself, 55 pairs up from 41. Done in PART 3b of the alignment round: no code change, and two tests that assert the twelve AR against ST pairs and the other two BY NAME and both ways round, so a later edit that drops them fails rather than keeping the count right with something else.

63. From Bader's 422 on 2026-09-20. `PenetrationSettings.DefaultSolidCategories` holds Walls, Floors and Roofs, and 69 of his Reviewed are services through a structural foundation, which the rule does not move. Should Structural Foundations join the list, and if so do Structural Framing and Structural Columns join it too, since a service through a beam is the same shape of thing

    Answer: Bader, 2026-09-20. FOUNDATIONS JOIN, FRAMING AND COLUMNS STAY OUT, deliberately. A service through a slab, a wall, a roof or a foundation is a hole somebody cuts and nobody needs to be told about it. A service through a BEAM or a COLUMN is a structural decision and an engineer has to make it, so it stays at New for a person to look at. He proved he reads it that way in the 422 themselves: he moved 69 pipes through slabs to Reviewed and LEFT 7 pipes through precast beams Active. Done in PART 3a of the alignment round, with that reasoning in the comment at the list so nobody widens it later, and three tests, one that a pipe through a foundation moves and two that a pipe through a beam and through a column do not.

64. From the alignment round on 2026-09-20. Models in one federation can be exported on different coordinates, and a model one storey out reads as nothing in plan and everything in section. What should the tool compare them on

    Answer: Bader, 2026-09-20. BY SHARED COORDINATE, and where that is not available leave it to visual inspection. NOT a bounding box: an EL model legitimately covers a smaller area than AR, so a box comparison would flag a correct model every time.

    IT IS READABLE, which was measured before anything was built, 5q. Every NWC this project exports carries, on its model ROOT and nowhere else, a `[Location]` tab with `revit_ProjectLocation` on it, the NAME of the Revit shared site, beside a Transform with a translation in X, Y and Z. So the ALIGNMENT block reports both, per model against the architecture model, with the difference in X, Y and Z separately. Done in PART 4 of the alignment round. A model carrying no shared coordinate is sent to be checked by eye in so many words, which is the second half of this answer and not a gap.

    WHAT IT FOUND ON HIS OWN FILES. 1A02MM's four models agree in X and Y and differ in Z by up to 312 mm, and they name four different shared sites, one of them `Internal`, which is what Revit calls a model that was not exported on a shared site at all. 1A02WL's eight models are scattered over hundreds of metres across eight different sites. Both are reported and neither is acted on.

65. From the alignment round on 2026-09-20. The alignment check and the export check can both find something wrong with a model before the group has clashed anything. Should a group with a fault in it be skipped, should the run stop, or should it be reported and run anyway

    Answer: Bader, 2026-09-20. REPORT IT AND RUN ANYWAY. Never skip a group and never stop a run for it. Both blocks are written where every model is open and nothing has clashed, and both name their run total even at zero, because a line that only appears when something is wrong reads as a check that did not run.

    SUPERSEDED IN ONE CASE ON THE SAME DAY, and the note is here so the two answers are not read as contradicting each other. Q70 answered b makes a group FAILED where a model names Internal as its shared site or names no site at all. That is the one thing either block may now do to a group, and the reason is that such a model is in a different coordinate system, so every clash against it is either one that is not there or a miss that is. Everything else in both blocks still reports and runs anyway, and even a failed group writes its NWF, its NWD and its report.

66. From the alignment round on 2026-09-20. The clash matrix has been exported at two tolerances and both files are in `samples`. Which is the project's tolerance from now on

    Answer: Bader, 2026-09-20. 25 mm IS THE STANDARD FROM NOW ON. 75 mm was the SD stage clash test and the DD stage is 25 mm. RECORDED HERE BECAUSE IT BITES: an NWF built at the SD stage still holds tests at 75 mm, a test already in the document is left exactly as it is, so a run against one of those clashes at 75 while everybody believes it is clashing at 25. The tolerance drop down at 25 mm is what beats it, F76, and choosing it RESETS the results of every test it changes, which is why PART 7 took a backup first.

67. From the alignment round on 2026-09-20, read off the machine before the round was planned. `C:\00-NM\Federation Task\C02 + 04\C04` IS EMPTY, zero files, no NWC, no NWF, no NWD, and 1A04PW, the building whose 422 decisions briefed this whole round, is not on this machine as a model at all. Only its report is, in Downloads. So "every ticked building in C02 + 04" is the ten C02 buildings and nothing else, and the ALIGNMENT and EXPORT CHECK blocks are proved on C02 rather than on the building they were designed off. Should the C04 models be put on this machine so a later round can run against the building the rules came from

    Answer: Bader, 2026-09-20. NO. C02 IS ENOUGH and 1A04PW does not come onto this machine. Closed.

    THE CONSEQUENCE, WRITTEN OUT, because this is a decision to leave two paths unproved and that has to be visible rather than discovered. TWO CODE PATHS HAVE TESTS AND HAVE NEVER MET A REAL FILE, and on this decision they stay that way:

    - the ALIGNMENT path for a model carrying NO shared coordinate at all, which says in words to check that model by eye. Every one of the 40 models in C02 carries one, 5u, so the words have never been printed by a run
    - the EXPORT CHECK path for a model missing an ELEMENT ID, which says to re-export with Convert element Ids switched on and counts how many rows reach the report with an empty id cell. Every model in C02 carries an id on 100 per cent of its elements, 5q, so that line has never been printed by a run either

    Both have unit tests that break one thing and assert the check names it, which is this repo's rule, and neither has been seen on a real model. THE ROUND REPORT SAYS SO IN ITS UNTESTED SECTION EVERY ROUND until a model shows one. That is the whole of what this answer costs.

68. From PART 8 of the alignment round on 2026-09-20, the cause of a number that has been in every log since the first real run. 33 of the client's 61 sets find nothing in every group, and it is a CASE MISMATCH and not a missing property. The models carry `ME-Ductwork`, `ME-Piping` and `ME-Equipment` on 100 per cent of their elements. The matrix asks for `ME-DUCTWORK`, `ME-PIPING` and `ME-EQUIPMENT`. The flag that would forgive it, `IgnoreDisplayStringValueCase`, value 16, is set by nobody: not the client's file, and not `SetBuilder.BuildCondition`, which adds the two Ignore display name bits and no others. THIS TOOL MUST NOT SILENTLY FIX IT, because setting that bit would also make two genuinely different worksets match and would change what every set in the file finds, quietly, on a rule nobody asked for. Three ways out and all three are somebody's decision, not this tool's: the matrix is corrected to the spelling the models use, which is one more `MatrixCorrections` rule beside the hyphen and the negation; the models are corrected to the spelling the matrix uses, which is 27 people renaming worksets in Revit; or the tool sets the ignore case bit on every value condition it builds, as a SETTING that is off by default and says in the log when it is on. Which

    Answer: Bader, 2026-09-20. THE FIRST OF THE THREE: the matrix is corrected to the spelling the models use. NOT the ignore case bit, and the reason he gave stands and is now proved twice over in his own models: `AR-EXTERIOR` against `AR-INTERIOR` and `ST-SUB` against `ST-SUP` are two pairs of REAL worksets, one and two letters apart, 5t. A flag that forgave case would not have merged those two pairs, but it would have changed what every set in the file finds on a rule nobody asked for, quietly.

    Done in PART 2 of the worksets round. `Federator.Core.Exchange.ValueRewrite` is the rule and `RevitWorksets` is where the spellings come from, 39 names measured off every model of all ten groups and embedded in the DLL the way the category list already is, so the tool never invents one. A value with exactly ONE case-only candidate is corrected. A value with TWO is REFUSED, left exactly as it was, and both named, because a rule that guesses between two real worksets is worse than a set that finds nothing. A value with none is left alone and says why.

    WHAT IT CAME TO on the client's own matrix: four values and 17 conditions. `ME-DUCTWORK` becomes `ME-Ductwork`, `ME-PIPING` becomes `ME-Piping`, `ME-EQUIPMENT` becomes `ME-Equipment` and `PL-Domestic Water` becomes `PL-Domestic water`. `PL-Drainage` is already exact. `FP-PIPING` and `FF-FIRE FIGHTING` are left alone because NO model in C02 carries a workset spelled anything like either, which is its own finding and is a different fault from the one this answer fixes.

69. From the alignment round on 2026-09-20. His own models disagree with each other about the name of a workset in five places, 5t, and only three of the five are typos: `EL-Lightining Protection` against `EL-Lightning Protection`, `EV-Ccctv system` against `EV-Cctv System`, and `PL-Drainage equipmen` against `PL-Drainage equipment`. The other two, `AR-EXTERIOR` against `AR-INTERIOR` and `ST-SUB` against `ST-SUP`, are real worksets one and two letters apart. A set asking for one spelling finds only the models that used it. Should the tool carry both spellings so the set finds everything, and if so what stops it merging two worksets that were never the same thing

    Answer: Bader, 2026-09-20. CARRY BOTH, as an Or row. Done in PART 3 of the worksets round, `flags="64"`, StartGroup, which F78 already measured and F87 already writes, built off the condition beside it so nothing in the code knows what a category or a property is called on this project.

    AND THE EXPORT CHECK STILL NAMES THE PAIR AS MISSPELLED, one line per pair saying which model carries which. That is the half that matters. If the tool absorbs a typo silently nobody ever fixes the models and the next building repeats it, so the clash test finds them AND the report says they are wrong.

    WHAT IT CAME TO, and this is worth knowing rather than assuming it did something: NOTHING, on this matrix, and correctly. Not one of the five disagreeing worksets is a value any set in the client's file filters on, so there is no set for an Or row to widen. The workset disagreements are a model hygiene problem and not a clash problem, and the export check naming them is the whole of what this round could usefully do about them. A test pins that, so a later matrix that DOES ask for one of them fails and gets looked at.

70. From the alignment round on 2026-09-20. 5q found a model in 1A02MM naming `Internal` as its shared site, which is what Revit calls a model exported on the internal origin and not on a shared site at all. Such a model is not slightly out of place, it is in a different coordinate system, so every clash reported against it is either a clash that is not there or a miss that is. Should a group holding one be reported, or failed

    Answer: Bader, 2026-09-20. FAILED. Done in PART 4 of the worksets round, built only after 5u had counted how many groups it would fail, which is TWO of the ten, 1A02MM and 1A02WL.

    FAILED DOES NOT MEAN THE GROUP PRODUCES NOTHING, and that is the half that makes the rule usable. The federation, the NWD and the clash report are all still written, because the evidence is what Bader takes to the people who own the models and a group that produces nothing gives him nothing to send. The ALIGNMENT block says which model and why, the run tail counts these apart from the models that merely sit somewhere else, and a group whose models name different REAL sites is reported and NOT failed, which is the case most of C02 is in.

71. From the alignment round's run on 2026-09-20. The penetration block said `29 no size could be read off the service` and nothing said which 29 they were. One day earlier 5r had found a reader that returned nothing and looked like an answer. Should the block name them

    Answer: Bader, 2026-09-20. NAME THEM. Done in PART 5 of the worksets round, by category in the block and one row per clash in the machine readable log.

    AND LOOKING WAS THE POINT, because 5s then found the same shape again. All 29 carried a size the whole time, written as words on the composite element, `53 mmo` for a conduit and `600 mmx100 mm` per connector for a cable tray fitting, and `ItemSizes` took only the two numeric kinds and dropped every one. 18 conduits, 10 cable tray fittings and 1 pipe fitting. `Federator.Core.Views.SizeText` reads them now, through the one unit table, and REFUSES a number with no unit rather than guessing at one.

72. From PART 7 and PART 8 of the worksets round on 2026-09-20, and it is the reason Q68's fix changed no clash count. A SET ALREADY IN THE NWF KEEPS THE CONDITIONS IT WAS BUILT WITH. F28 leaves a set already at its path exactly as it is, and the reason is good: adding another copy would leave two sets at one path and a clash locator resolving to whichever came first. But it means a value corrected in the picked file SINCE the set was first built never reaches the document. PART 2 corrected `ME-DUCTWORK` to `ME-Ductwork` in four values and 17 conditions, the run picked the corrected file, and the same 33 sets found nothing, because the sets in his NWFs are still asking the old question. That is proved by consequence rather than assumed: the models carry `ME-Ductwork` on 236 elements of 1A02MM, the corrected file asks for exactly that, and `BLD-ME-Ducts&Duct Fittings` still found nothing. The run now SAYS this under the already-there count rather than leaving it to be discovered. THIS TOOL WILL NOT FIX IT ON ITS OWN, because replacing a set changes what every clash test pointing at it finds, on every group, and that is a decision and not a correction. Three ways out, all somebody's: a tick box that rebuilds a present set from the picked file, off by default and saying what it changes, which is the shape F76 already uses for the tolerance; or the sets are deleted from the NWFs by hand once so the next run builds them fresh; or the sets stay as they are and the matrix correction is accepted as something that only helps a building whose NWF has not been built yet. Which

    Answer: Bader, 2026-09-20. THE FIRST OF THE THREE: a tick box that rebuilds a present set from the picked file, off by default, saying what it changes, the shape F76 already uses. NOT b, no deleting sets by hand. NOT c, the eight NWFs do not stay broken.

    Done in PART 2 of the drift round. `Federator.Core.Sets.SetDrift` compares what the set in the DOCUMENT asks against what the picked file asks, `SetRebuildSettings` holds the box and its wording, and `SetBuilder.Rebuild` replaces a drifted set through `DocumentSelectionSets.ReplaceWithCopy` in its own slot.

    IT NEVER HAS TO REFUSE, and that was measured BEFORE a line of it was written. 5v replaced a set a clash test points at and read back five things after a save, a close and a reopen off the disk: the test still points at the set, its 36 results are there, the Reviewed status a person set is there, the set is in the same place in the tree, and it finds items. So the box is not under Things that destroy data, because nothing is destroyed.

    WHAT IS COMPARED IS THE QUESTION AND NOT THE FLAGS. 5w found 1A02MM's 61 sets carrying no ignore display name bits where every other group's carry both, which is the fingerprint of an original import against this tool's own creations. Comparing on flags would replace 61 sets over something not shown to break anything, since those sets do find items.

    IT REBUILDS ONLY WHAT DRIFTED, never all 61, and a set the picked file does not name at all is LEFT ALONE, because removing one changes what every clash test pointing at it finds.

    WHAT IT CAME TO ON HIS OWN FOLDERS. The run of 21:06 was the first with the box on and it found 147 sets across the ten groups whose question no longer matched the file, and rebuilt them. The run of 21:36 over the same folders found 28, because the first run had already corrected the rest. 1A02MM went from 49 clash tests finding something to 57, and from 395 clashes to 542.

    AND THE FIRST RUN EXPOSED A FAULT IN THE REPORTING RATHER THAN IN THE REBUILD, found in PART 8 by reading the log. The item count was read through the wrapper obtained BEFORE `ReplaceWithCopy`, which is a borrowed handle over an object that is no longer there, rule 4g. Every rebuilt set logged `0 items, already there, left alone` about a set it had just replaced, and the empty set reader then judged the OLD question and called a set wrong that had been corrected one line earlier. The set is read again AFTER the rebuild now.

73. From PART 8 of the drift round on 2026-09-20, counted off a workbook this tool wrote rather than off his report. 1A02MM's workbook holds 1,830 test blocks in 15,182 rows. 57 of those blocks found something and carry 542 clash rows between them. The other 1,773 found nothing, and each one still costs EIGHT rows: the test name, the header values, a blank, the Item 1 and Item 2 labels, the fifteen column headings, and three blank rows after it. That is 14,184 rows of headings for nothing, which is 93 per cent of the file. A person opening it scrolls past two hundred screens of empty blocks to reach the 542 rows that say something.

    THE REASON IT IS LIKE THAT IS A DECIDED RULE AND NOT AN OVERSIGHT. `CreationPlan` already says it: the client's report is the WHOLE matrix, and a test missing from it reads as a test nobody ran rather than as a test that could not clash. Every block is evidence that the pair was checked. So this is not a bug to fix quietly, it is a trade between a report that proves the whole matrix was run and a report a person can read.

    Four ways out, and all four are somebody's decision and not this tool's. Leave it exactly as it is, because completeness is what the client agreed to receive. Keep every block and make the empty ones CHEAP, one row saying the pair was checked and found nothing rather than eight, which keeps the evidence and takes 1A02MM from 15,182 rows to about 2,300. Write two files, the full matrix as now and a second holding only the blocks that found something. Or keep one file and put the empty blocks on a second sheet, which loses the single sheet shape the client accepted.

    WHAT IS MEASURED AND WHAT IS NOT. The counts above are read off the file this run wrote, as a zip, by counting the test name cells in column A and the rows between them. What is NOT measured is what the client actually does with the empty blocks, whether anybody reads them, and whether the eight row shape is theirs or ours. The block layout is measured off their export, so shortening it would be a departure from their format and that is exactly why it is a question.
