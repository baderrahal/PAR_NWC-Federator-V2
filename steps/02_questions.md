# 02 questions

Every question for Bader in one list.
Answer by number. Short answers are fine.

1. Which report shape is the ask. The brief says one sheet per test, all columns, images included. The repo says one sheet laid out as the client's HTML report, with the per test sheets, the Summary and the Matrix removed on purpose. Which one holds. See L8

2. Open file with no clash XML. CLAUDE.md says it runs the tests already in the document. Today it runs nothing. Should the worker make it run them, F8, or change the words

3. Which six buildings are the done set. The last real folder had 22 groups. The done criterion says every ticked building, the brief says 6

4. May the dead code go. `ClientColumnsOnly`, `SheetNames.SummarySheet`, `SheetNames.MatrixSheet`, `ClashMatrix`, `TestReport.SheetName` and their tests. See B9 and F11

5. May CLAUDE.md be split into `.claude/rules`. Section 8 of `00_analysis.md` has a shape. Yes, no, or a different shape

6. Branch and merge. This pass used `analysis-pass` and merged it as asked. For the fixes, one branch per fix, or one branch for F1 to F7 together

7. Is there a timing baseline. Any log from a run over all buildings on the current code. `docs/scan.md` has one clash step at 39.8 s and another at 1259.5 s with no cause found

8. Picture numbering. Ours counts tests in run order, the client's counts them by block in the report. The pictures still link right. Leave it, or match theirs, F17

9. The 1A04WE export. The docs name it as the one with the Layer column. It is not in `samples/client-report`. Can it be committed

10. CI for the add-in. It cannot build without Navisworks. Is there a Windows machine with Navisworks that can be a self hosted runner, or does the add-in build stay local only

11. Tests on Linux. 40 of 868 fail in the container on Windows path assumptions. Make them path neutral, F16, or accept that tests run on Windows only

12. Units on a CHANGED group. Today the units are still applied. Skip it, F9, or is applying them to a group nobody saves harmless enough to leave

13. The confirm dialog. It says the document is cleared before each group. On a rerun it is not. Say it only for groups with no NWF yet, or drop the sentence

14. Dispose in `SetBuilder` and `ClashRunner.Resolve`. Both leak wrappers against the rule. Fix now, F15, or wait until a run shows it costs something. It is the riskiest change in the list

15. Who runs the local proofs P1 to P3, and when. The worker cannot. Every done criterion waits on them
