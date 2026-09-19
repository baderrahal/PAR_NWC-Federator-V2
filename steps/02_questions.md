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

   Answer:

33. From F54 on 2026-09-18. A clash that cannot be solved should end up Reviewed rather than sitting at New or Active forever. The tool can set a status, `DocumentClashTests.TestsEditResultStatus` is measured and the method that applies it is written. What is missing is the input. How does the tool LEARN which clashes cannot be solved. Four shapes it could take, and each means a different feature: a list of clash names Bader supplies per run, a rule over the clash itself such as a distance under some number or a pair of disciplines that always overlap, a status carried in the clash XML that gets applied on import, or a person marking them in Navisworks once and the tool leaving them alone after. Which of those is the ask, and if it is the second, what is the rule

   Answer: the SECOND shape, a rule over the clash itself. Answered on 2026-09-19 and carried out by F72. A clash becomes Reviewed when all four are true: one side is a service by item category, the other is a solid by item category, the service measures 150 mm or less, and the clash is at New or Active. The other three shapes are NOT chosen and are recorded as such: no list is supplied per run, nothing is read off a status in the clash XML, and nothing watches what a person marked last week. Q41 to Q44 answer the four details this raised.

34. From F50 on 2026-09-18. Whether one model can be taken out of an open document without clearing the whole thing was never measured, so F50 kept the clear and restore and widened it from two things to four. `tools\probes\probe-model-remove.ps1` answers the question. If the answer comes back yes, the rebuild could instead append what the scan has and the NWF does not, remove what the NWF has and the scan does not, and never clear at all, which would mean nothing needs putting back and nothing can be lost. That is a rewrite of the rebuild rather than a tweak, and the widened version now works and is tested. Once the probe has answered, is the rewrite wanted, or does the counted clear and restore stay because it is proved and the risk of changing it outweighs the tidiness

   Answer:

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

    Answer:

49. From F83 on 2026-09-19. F83 replaces a MEASURED order with a chosen one. The block order, most clashes first with ties in creation order, was read off both of the client's own exports over all 1830 blocks, and the tie rule is explicitly not alphabetical, including in one tie group of 1807. A priority file sorts A, then B, then C, then by test name, which is a different order from the one the client accepted. It only applies when a file is picked and the measured order is still the default. Is the client happy to receive the report in priority order, or is priority a column to sort on in Excel and the blocks stay where they were

    Answer:

50. From F72c on 2026-09-19. F72c widens one written guard. `StatusesThisToolMaySet` says Reviewed is the only status this tool ever sets, and an undo has to put a clash back to New or Active. It is written as `AllowsAsUndo`, a separate answer: an undo may set the exact status one of this tool's own records names in this file, and nothing else, so Approved and Resolved are still never set because no record can name one. Is that the right shape, or should an undo instead leave the status alone and only remove the record, so a person sees which clashes the tool had moved and decides one by one

    Answer:

51. From F85 on 2026-09-19. F53 and F72a read a size two different ways and F85 had to pick one. `SizeRule.Decide` takes the FIRST size property on the list, which is what a viewpoint wants, one representative number nobody argues about. `SizeRule.LargestMillimetres` takes the LARGEST, which is what a penetration wants, because a 600 by 150 duct has to fit a 600 through the wall. F85 reads a CLASH SIDE and so does F72a, so F85 reads it the way F72a does, the largest. Reading it the other way would let the same duct be set Reviewed by F72a as a large service and filed by F85 under Over 150mm, and the two rules would disagree about one clash with nothing saying so. Is the largest the right reading for a viewpoint folder as well, or should the two stay different and the difference be reported

    Answer:

52. From F87 on 2026-09-19, found when main was merged at the end of the round. TWO FILES CARRY THE NAME `1104-PAR_CLASH_AllInOne_25mm_FIXED.xml` AND THEY ARE NOT THE SAME FILE. Bader uploaded one to `samples\` on 2026-09-19 while the round was being worked, and F87 wrote one to `exchange\` from the sample. They differ in exactly ONE line. Both correct the missing hyphen, all 121 occurrences. Only the one F87 wrote also makes `BLD-EL-Devices` ask for something other than Electrical Fixtures, which is the second half the brief asked for, so in the supplied file Devices is still the same set as `BLD-EL-Electrical Fixtures` and F84 still reports the pair. Neither file is deleted and neither is edited, because samples is evidence and exchange is proved by a test to be exactly what the rule produces, and `SuppliedCorrectedMatrixTests` pins the difference instead. Which of the two should the tool be pointed at, and should the one that is not it be renamed so two files of one name cannot be picked by mistake

    Answer:

53. From A16 of the audit, on 2026-09-19, judged in the wiring round. `PenetrationSettings.IsDecided` is named by the F72a rule in core.md as THE RULE, and its only caller is the test that asserts the decision against the client's matrix, `PenetrationRuleTests`. CLAUDE.md says a public member nothing in src calls is deleted with its tests unless a decision here keeps it. The other eight members the audit named gained a running caller when their fix was wired, and this one never will: no step in 03_bader_next.md calls it, and the probe's own membership test, `ProbeSettings.Asks`, reads a narrowable setting rather than the two default lists, so it is not the same question. Keep `IsDecided` as a stated rule with a test-only caller, or delete it and reword the F72a bullet to name the two lists and `PenetrationSettings.Names`

    Answer:

54. From F76 on 2026-09-19, found on the wiring round's run. The line that says where every report row's tolerance was read came out as `TOLERANCE on the report, read off the clash test in the document for 1041, the clash XML for 17259, chosen in the tool for 0, UNKNOWN for 0, 18300 tests in all`. The 17259 are the rows of tests that never ran: not created because a side finds nothing, or already in the document with an empty side. Their row keeps the tolerance the plan gave it, read off the XML, because the document never produced that row. The rule as written says every row should read off the document and a count under any other origin is a report saying a number the run did not clash at. For a skipped row that is literally true and also the only honest number there is. Should a skipped row carry the XML value and its origin as now, carry no tolerance at all, or should the line count the skipped rows apart so the number that matters, rows that RAN and did not read off the document, stands on its own

    Answer:
