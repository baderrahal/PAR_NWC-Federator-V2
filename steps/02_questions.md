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

22. Not in the first list. CHANGED. Six groups in the same log had an NWF built from an older folder with fewer files, and the tool left them alone. See B13

   Answer: rebuild the NWF from the scan folder by itself, keep the tests saved inside it, and say in the log what was added, what moved and what was removed. F24.

23. Not in the first list. Units. The same log says THE DOCUMENT DID NOT FOLLOW on 11 of 14 groups and the report went out in feet. See B14

   Answer: the report is always in meters, never feet. Force the document to meters and fail the group loudly if it will not follow. F26.
