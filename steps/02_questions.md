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

   Answer:

26. From F40 on 2026-09-12. The rule says a public member nothing in src calls is deleted with its tests. Thirty of them are read only by a test that pins a rule the repo states: the reference file holds one batchtest and 1830 tests, 61 distinct locators, linkage none and rules empty in every test, two set names ending in a space, the stamping actually ran, the picture name round trips, the report check names what it found. Deleting the member deletes that proof. F40 made those members `internal` instead, so they are off the public surface and the tests still reach them through the `InternalsVisibleTo` the Core project already carries. Is that the right answer, or should the member and its test both go? See the list in the F40 pull request

   Answer:

27. From F40 on 2026-09-12. The two choice rule for what counts as still outstanding has lost every reader. The Summary and Matrix sheets went, D3 took the window setting, and F40 deleted `OpenClashes.All`, `Default`, `Describe` and `Of` along with `TestReport.OpenUnder` and `NewPlusActive`, which were the last callers. What is left is the enum and `StatusesFor`, which the image status filter reads to ask for what Navisworks counts as open. Should the rule stay in `.claude/rules/core.md` as a rule with one reader, or is the choice gone for good and the bullet with it?

   Answer:

28. From F41 on 2026-09-12. Two handle reads were left where they are, because neither is in the F41 list and neither is covered by what the history file measured. `search.Selection` in `SetBuilder`, read once a set to call `SelectAll` on a `Search` this tool owns. The source read out of a side's own collection in `ClashRunner.LocatorOf`, which sits inside the loop over the 61 indexed sets, so one side costs up to 61 reads of the same source. The measured rule says releasing a wrapper releases the wrapper and never the document's object, but it was measured on items read out of a document collection, not on a sub object read off a handle this tool created. Release both the same way as the rest, or leave them until the ownership of a sub object is measured off the installed DLL?

   Answer:
