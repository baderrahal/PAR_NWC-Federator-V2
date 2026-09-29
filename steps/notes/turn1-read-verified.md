# The turn 1 read, verified

The 86 faults of steps\loop-read.md section 1 that the read called silent, broken or loud, each read again on main at 0eb4ede on 2026-09-29 by two readers who did not write the first read and did not see each other, one starting from the cited line and one from its callers and tests. Where the two disagreed, or one gave no verdict, a third read the code and settled it. Nobody changed anything. 76 CONFIRMED, 10 PARTLY, 0 REFUTED. By the readers' own judgement of harm: 60 silent wrong outputs, 13 broken features, 6 loud failures, 7 noise. Proof: 50 by a Core test alone, 27 by a Core test and a run, 8 by a run alone, 1 needs none.

Each entry below is the verdict of the first reader of the pair, or of the third where one settled it. Kind is harm, cause is the file and line on main, proof is what would show it fixed. Line numbers are of 0eb4ede. What a reader marked UNKNOWN about Navisworks is UNKNOWN.

## T1-S1, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ByDesign.cs:148, with src/Federator.Core/Clash/ByDesignTally.cs:96-99 and :138-145
- proof: Core test
- why: The fault is real as described. The verdict is counted as a move and the clash is left out of the wanted list, so the BY DESIGN block, the RESULT line and the rule B run line each count one move that never happened for every unnamed clash. How often that happens is UNKNOWN. The rule can move into Core, for example with Judge taking the clash name and never returning Reviewed for an empty one. A ByDesignRuleTests case with an empty name at New between a listed pair would fail today and pass after.
- evidence: ByDesign.cs:148 `tally.Add(testName, clashName.Length == 0 ? "an unnamed clash" : clashName, verdict, pair)`. Only the wanted list checks the name, at :150 `if (verdict == ByDesignVerdict.Reviewed && clashName.Length > 0)`. ByDesignRule.Judge (ByDesignRule.cs:36-74) never sees the name, so an unnamed clash between a listed pair at New or Active gets Reviewed. ByDesignTally.MovedCount is `get { return Of(ByDesignVerdict.Reviewed) }` (98), and Add writes a REVIEWED line for it (143-145). FederationEngine.cs:2666-2670 writes the block and adds MovedCount to log.ByDesignMoved and byDesignAcrossTheRun. Those feed ByDesignTally.ResultLine (RunLog.cs:1838) and RunLine (FederationEngine.cs:377). Penetrations.cs:150-155 counts the same case as `NotAPenetration()` and returns. Whether Navisworks ever hands back a ClashResult with an empty DisplayName is UNKNOWN.

## T1-S2, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ClashHarvest.cs:164
- proof: both
- why: The fault is real in the code. An empty group becomes one row with RawClashes 1, while the document tally counts zero for it. So the Clashes cell and the group's status cell each read one higher than the panel. The ROWS line compares rows with leaves, not RawClashes, so the workbook carries the extra one without a word. The floor can become a Core rule on ClashRow with a test. Only a run on a test holding an empty group shows whether the case happens.
- evidence: ClashHarvest.cs:160 `int raw = CountLeaves(group.Children, distances)`, then :164 `row.RawClashes = raw < 1 ? 1 : raw`. ClashReportModel.cs:396 `tally.Add(row.Status, row.RawClashes)`, and :406-418 adds up RawClashes. WorkbookWriter.cs:289 and :330 write `test.RawClashes` into Clashes, and :293 and :334 write `test.Tally.Of(status)` into the status columns. ClashRunner.CountInto (ClashRunner.cs:1608-1636) adds nothing for a group with no ClashResult under it. RawClashes is also read by the report sort (ClashReportModel.cs:591) and the priority count (FederationEngine.cs:291). Whether a saved test can hold an empty result group is UNKNOWN. A related gap outside this finding: GroupRow gives every leaf the group's own status (`row.Status = (CoreClashStatus)(int)group.Status`, line 166), while CountInto counts each leaf by its own status. Whether a leaf's status can differ from its group's in Navisworks is UNKNOWN.

## T1-S3, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ClashRunner.cs:465-469, with the early returns at 692-703 and 707-722
- proof: both
- why: The fault is real as described. For a saved test that was changed and then skipped, the workbook block and the TOLERANCE line say a value was read off the document that the document no longer holds. The value and origin a skipped row should carry can be decided in Core, for example by ToleranceChoice returning the chosen value with origin Tool, and a Core test of that would fail today. A run with a chosen tolerance on a single discipline group proves the workbook cell.
- evidence: BuildReports sets `report.Tolerance = planned.Tolerance` (465) and `report.ToleranceFrom = plan.Source == ClashPlanSource.Document ? ToleranceOrigin.Document : ToleranceOrigin.File` (467-469). On a run with no XML, planned.Tolerance is `test.Tolerance` as SavedTests.Read read it (SavedTests.cs:73), before anything runs. PlanTheCreation hands every saved test to OneTest (ClashRunner.cs:1690-1694). OneTest calls `ApplyChosenTolerance(clashTests, address, planned.Name)` (617), which does `copy.Tolerance = wanted` and `clashTests.TestsEditTestFromCopy(existing, copy)` (1189-1190). The single discipline skip then returns at 697-702, and the side check returns at 707-722, without touching the row. Only a test that ran reaches `summary.Tolerance = after.Tolerance` (856). WriteEmptyTestRow writes `test.ClientTolerance()` (WorkbookWriter.cs:288) for every block, and CountToleranceOrigins (FederationEngine.cs:185-211) counts the row under Document. The XML path does the same for a test already in the document (630, then a skip), where the row says File with the XML value.

## T1-S4, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ClashRunner.cs:1102-1108 (Apply), with src/Federator.Addin/Engine/FederationEngine.cs:2725
- proof: both
- why: The fault is real as described, on a narrow path. The APPLIED line is written and the NWD carries the edit, but the NWF on disk does not. The next run finds the same drift and applies it again. The decision to save again can become one Core rule over the counts, with the applied count added, and a Core test can prove it. A run on a single discipline rerun with the box ticked proves what the NWF holds.
- evidence: Apply calls `clashTests.TestsEditTestFromCopy(existing, replacement)` (1102) and then only writes a log line. It never sets changedTheDocument. ApplyChosenTolerance does set it, `changedTheDocument = true` (1193). CreateAndRunTheTests returns `clash.CreatedCount > 0 || clash.RanCount > 0 || runner.ChangedAStatus` (FederationEngine.cs:2725). ClashStep returns `CreateAndRunTheTests(document, job, outcome, source) || changed` (2231). There, changed comes from BuildTheSets, which returns `sets.PutAnythingIn || sets.ActedOnLeftovers > 0` (2456), and that is false on a rerun where every set is already present. FinishTheGroup saves again only `if (clashPutSomethingIn || viewsPutSomethingIn)` (959), and it publishes the NWD at 972-975 either way. Apply is reached only when an XML is picked, the apply file settings box is ticked and drift is found (967-980). The test is then skipped at 692-703 or 707-722.

## T1-S5, PARTLY, noise

- how: a third reader settled it, reader A CONFIRMED, reader B PARTLY
- cause: src\Federator.Addin\Engine\ClashRunner.cs:1106-1108, in Apply, the private method CompareAndMaybeApply calls at line 985 when ApplyFileSettings is on
- proof: both
- why: The contradiction is real. With Apply on, one run writes the drift block saying results are KEPT and then one APPLIED line per test saying the edit reset them, and the window help under the box says it resets clashes to New. So at least one of those statements is wrong. The finding goes too far when it says the APPLIED line is known to be false. The 5y measurement covered a different call, TestsReplaceWithCopy, on a copy that already held the test's results, with only the tolerance changed. Apply edits the test from a fresh ClashTest that holds no results, through TestsEditTestFromCopy, and it changes the type, merge and both sides as well. Whether that keeps the results and statuses is UNKNOWN, and nothing in the repo measured it. The comments at 175-179 and 1069-1070, and the rule in .claude\rules\addin.md, stretch 5y past what it measured. No number, count or file is wrong because of this line, so the harm is words. That is noise, though it is noise about whether review history was lost. To settle it, run a copy of an NWF with Apply the file's settings ticked, against an XML that differs from a saved test holding results with statuses. Read the result count and the status count before the edit, after it, and after a save and a reopen. Then make every statement agree with what the run shows. After that, a Core test can pin the one wording once the APPLIED sentence, the drift block line and the help lines live in Core beside TestDrift and ToleranceChoice. It should assert that none of them contradicts the measured answer.
- evidence: ClashRunner.cs:1106-1108 `log.Line("CLASH    APPLIED  " + planned.Name + "  the file's settings were put onto the test in the document, which reset " + "its results");`. The same class says the opposite at 175-179 `IT DOES NOT RESET ANYTHING BY ITSELF` and at 1069-1070 `It RESETS NOTHING by itself, measured 5y`. The same run's log says the opposite too: TestDrift.cs:306-309 `APPLYING these to the tests in the document. Their recorded results and the statuses on them are KEPT` and FederatorWindow.xaml.cs:1779 `It resets nothing by itself, measured 5y`. But 5y did not measure this route. tools\probes\ViewpointProbe\ViewpointProbePlugin.cs:5811-5814 `using (ClashTest copy = (ClashTest)((ClashTest)tests.Tests[at]).CreateCopy())`, then `copy.Tolerance = value;` and `tests.TestsReplaceWithCopy(at, copy);`. That is the test's own copy, which carries its own results, put back through TestsReplaceWithCopy with only the tolerance changed. Apply at 1081-1102 builds `new ClashTest()` holding no results, sets DisplayName, TestType, Tolerance, MergeComposites and both sides, and calls `clashTests.TestsEditTestFromCopy(existing, replacement);`. scan.md:138 records only the signature `TestsEditTestFromCopy(ClashTest test, ClashTest copyFrom)` and nothing about what it does to results. The contradiction goes further than either reader said. The window's own help line for this box, FederatorWindow.xaml:566, reads `Resets those clashes to New. Differences are listed either way.` The same split is inside ApplyChosenTolerance: comment 1145-1146 `because an edit resets its results` against log line 1199 `Its results and its statuses are KEPT, measured 5y`. The window help ToleranceChoice.cs:79 reads `Changing a saved test resets results`.

## T1-S6, PARTLY, loud failure

- how: two readers agree
- cause: src/Federator.Addin/Engine/DocumentCensusReader.cs:89-100, and the real hole at src/Federator.Core/Rerun/RebuildTally.cs:65 and :75
- proof: Core test
- why: The code facts are right, but the harm is not silent. A throw fails the group loudly through the catch in RunOne and nothing is saved. At 1396 it only costs the fallback to the clear and rebuild. The silent half needs a null collection, which is UNKNOWN. Changing this overload alone to return minus one would make things worse. RebuildTally reads a Before it could not take as nothing to keep, so a loud failure would become a silent keep. A Core test with Before minus one, AfterAppends 0 and AfterRestore 0 that expects EverythingKept false fails today. That test belongs with any fix here.
- evidence: `public static int Sets(DocumentSelectionSets sets)` returns `0` for null (91-94) and has no try (96-99). That breaks the class rule at 24-27, and the other overload, Sets(Document) at 71-86, does have a try. The calls at FederationEngine.cs:1396, 1510, 1719 and 1767 have no try around them. The call at 1775 is inside the try at 1772-1783. A throw at any of the four goes up through ReshapeFromScan or RebuildFromScan to the catch in RunOne (732-749), which does `outcome.AddError(error.Message)` and saves nothing. RebuildTally cannot tell a Before of minus one from zero. Its Kept rule is `return Before <= 0 || AfterRestore >= Before` (RebuildTally.cs:65), and WasCounted reads only AfterAppends and AfterRestore (75). So a Before that could not be taken reads "none, the NWF held none before the clear" (93) and counts as kept. The line beside it, `tests.Before = SavedTests.Read(document).Count` (1397), has no try either. Whether document.SelectionSets can ever be null is UNKNOWN.

## T1-S7, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/DocumentGuard.cs:79-82 and :52-55
- proof: Core test
- why: The fault is real. The person is still warned, because the text is not null. But a saved file whose name could not be read is called unsaved, and the throw is swallowed with no log line, which also breaks the rule against a catch that swallows. The harm is small. The wording can move to Core, taking a name, no name or a name that could not be read, and a Core test that the third case does not say unsaved fails today.
- evidence: SafeFileName has `catch (Exception) { return null }` (79-82) and logs nothing. WhatClearWouldDiscard then takes the branch `what.Append("The current unsaved document")` (54). That text is shown in the MessageBox at FederatorWindow.xaml.cs:1405-1411, "This will be discarded without saving:", in the confirm text at 1941-1942, and in the PREVIEW log line at 841-842. Whether CurrentFileName or FileName can throw is UNKNOWN.

## T1-S8, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/DocumentUnits.cs:118-127 and :129-132
- proof: Core test
- why: The fault is real. When the second read throws, the log says the document shows the units it had before. That reads like a measurement that Document.Units did not follow, and whether it follows is exactly what the class says is UNKNOWN and only a run can answer. The catch logs nothing, which also breaks the rule against a catch that swallows. The report is not affected, because ReportUnits converts to metres anyway. The line can be built in Core from a value that was not read, and a Core test can assert it says UNKNOWN rather than naming a unit.
- evidence: `Units after = before` (118), then `catch (Exception) { // The line below still says what was attempted. }` (124-127), then `"... document shows " + Name(after) + "..."` (129-132). The one caller throws away the return value, `() => new DocumentUnits(log).Apply(document, wanted)` (FederationEngine.cs:941), so only the log line is affected.

## T1-S9, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/FederationEngine.cs:1573, with :959
- proof: both
- why: The fault is real as described. The NWD goes out with the new file list while the NWF on disk keeps the old one. The group reads Rebuilt and DONE, and the next run reads CHANGED again and reshapes again. Adding a GroupJudgement fact for whether this run saved the NWF makes a Core test possible: a Rebuilt group without a save must not be DONE. A run on a reshaped single discipline group, or one with no saved tests, proves what the NWF on disk holds.
- evidence: ReshapeFromScan says "They return TRUE now, which stops the fallback, and nothing on this path saves." (1529-1530) and sets `outcome.Decision = RerunDecision.Rebuilt` (1573). RunOne then reads the old file, `outcome.NwfSize = SizeOnDiskOrMinusOne(job.NwfPath)` (704-705), beside the comment "the reshape deliberately does not save" (700-701). FinishTheGroup saves only `if (clashPutSomethingIn || viewsPutSomethingIn)` (959), and it publishes the NWD at 972-975 either way. ClashStep returns false for source Nothing (2218-2221). A run with no XML creates nothing, and a single discipline group or one where every side is empty runs nothing, so 2725 returns false. BuildViewpoints returns false when there is no report (3183-3187). GroupJudgement never checks that a Rebuilt group's NWF was saved (GroupJudgement.cs:161-231). It checks only AppendedCount (189).

## T1-S10, CONFIRMED, silent wrong output

- how: a third reader settled it, reader A CONFIRMED, reader B none
- cause: src/Federator.Addin/Engine/FederationEngine.cs:2042-2058 (SaveTheNwf). The bool from TrySaveFile and the catch never reach the outcome, and nothing on GroupFacts records whether the NWF save succeeded (src/Federator.Core/Rerun/GroupJudgement.cs:11-116).
- proof: both
- why: The fault is real on main as described. When the save after a rebuild returns false or throws, the group is judged DONE and Rebuilt, and the NWF from the earlier run is listed as written by this run with its old size. The finding leaves out five things. First, the stale NWF only stays on disk when the second save does not fix it. SaveTheNwfAgain runs only when the clash step or the viewpoints put something in (959-965). When it runs and succeeds, the NWF on disk ends up right, and the only fault left is the misleading written line. The same cause, for example a locked or read only file, would likely make the second save fail as well, and a false return there is also silent. That second fault is T1-S11 at 3152. Whether TrySaveFile returns false or throws on a locked or read only NWF is UNKNOWN until a run shows it. Second, a throw is not fully silent. log.Failure puts it in the log's failures list, but the group judgement still says DONE. A false return leaves one plain line and nothing more. Third, the NWD is published from the rebuilt document that is open, so this run's NWD carries the new file list while the NWF carries the old one. The next run's Decide will see CHANGED again. Fourth, ConfirmTheNwfSurvived (2795-2823) compares the file against the stale size it was handed and reports the NWF as intact. Fifth, on the Build path the file is missing, WriteFinished logs MISSING and the group fails with 'the NWF is not on disk', so the silent case is the rebuild only. Proof: add an NWF save fact to GroupFacts, set from the TrySaveFile bool and the catch, and have GroupJudgement fail a group whose NWF save did not report success. A Core test that breaks only that fact on an otherwise DONE Rebuilt group, and asserts the reason names the NWF save, fails before the change and passes after it. Then a run on the local machine with the NWF of a CHANGED group made read only, to show which way TrySaveFile goes and that the log and RESULT name the failure.
- evidence: FederationEngine.cs:2042-2045 `if (!document.TrySaveFile(job.NwfPath)) { log.Line("NWF      the save returned false for " + job.Building); }`. The catch at 2047-2053 only calls `log.Failure(...)` with "kept going, the disk is checked next". Then 2057-2058 `outcome.NwfSize = log.WriteFinished("NWF", job.NwfPath); outcome.NwfOnDisk = outcome.NwfSize >= 0;`. RunLog.cs:1372-1421: WriteFinished adds the path to `written` and logs "written" whenever `SizeOnDisk(path)` is not negative, so an older file at the path counts as written. The rebuild path reaches this with an NWF already on disk: Decide returns Build only when `!File.Exists(job.NwfPath)` (1124), and RebuildFromScan runs `WriteNwf(document, job, outcome); outcome.Decision = RerunDecision.Rebuilt; return true;` (1840-1842). RunOne then reads the size again with `outcome.NwfSize = SizeOnDiskOrMinusOne(job.NwfPath); outcome.NwfOnDisk = outcome.NwfSize >= 0;` (704-705). No error was added, so the `if (outcome.HasErrors)` check at 692 lets the group through. GroupJudgement.Judge passes a Rebuilt group with NwfOnDisk true and AppendedCount above 0 as DONE. The NWD gets a check of its own, `if (facts.NwdRequested && !facts.NwdPublishReportedSuccess)` (GroupJudgement.cs:213), fed from `outcome.NwdPublishReportedSuccess = published;` (FederationEngine.cs:3355). The NWF has no matching fact, and the Core tests have none (a search of tests for NwfSave or SaveReported finds only step name lists and the written size test). The comment at 2039-2041, "the bool is what says the save happened rather than the file being there", does not match the code, because the bool only writes one log line.

## T1-S11, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/FederationEngine.cs:3152-3155 and :3167-3168
- proof: both
- why: The fault is real as described. The sets, tests, statuses and viewpoints this run put in reach the NWD, which is published from the document, but not the NWF. The log says the NWF was written with a size, and the group can be judged DONE. A throw and a false return give two different answers for the same failed save. The proof is the same as for T1-S10: a Core fact in GroupJudgement, and a run with the NWF made read only between the two saves.
- evidence: `if (!document.TrySaveFile(job.NwfPath)) { log.Line("NWF      the save after the clash work returned false for " + job.Building) }` (3152-3155) puts nothing on the outcome. The catch does `outcome.AddError("saving the NWF after the clash work threw " ...)` (3159-3160). Then `outcome.NwfSize = log.WriteFinished("NWF", job.NwfPath)` and `outcome.NwfOnDisk = outcome.NwfSize >= 0` (3167-3168). On an OPENED group this run never wrote that file at all (724-725). GroupJudgement then passes its NWF check at 161, and nothing else looks at the save.

## T1-S12, PARTLY, noise

- how: a third reader settled it, reader A PARTLY, reader B REFUTED
- cause: src/Federator.Addin/Engine/Penetrations.cs:338-347, the silent catch in LargestOf, reached by F72 through WantedFor (82, 158) and by F85 through ServiceSizeOf (190-213) from ViewpointBuilder.cs:393, with the unit read at Penetrations.cs:405-408
- proof: Core test
- why: Real code but not the harm the finding names. The unknown unit branch is unreachable on 2025 because every measured Units member is a table row, so no service is filed as size unknown for that reason and no silent wrong output happens today. Reader B is right that nothing is wrong at run time. Reader A is right that the contradiction is real and only latent. What is left is a comment and a catch: the comment at 340-346 gives only the F72 reason, `which the rule answers by LEAVING THE CLASH ALONE`, while for the F85 caller the same null files the clash in its pair folder, and the catch logs nothing. That makes it noise, so PARTLY. The Core test that would guard it asserts that each of the eleven names in scan.md 5, Meters through Microinches, resolves through UnitTable.ByEnumName, so a row dropped from the table fails a test before this catch could hide it. A side note outside this finding: a throw at ItemSizes.cs:48 on the leaf ends the whole LargestOf loop, so the ancestors are never read. Whether PropertyCategories on a ClashResult.Item1 item ever throws is UNKNOWN, and scan.md 5r measured it reading fine.
- evidence: The swallow exists as described. Penetrations.cs:335 `return SizeRule.LargestMillimetres(read, unitEnumName, sizes);` then 338 `catch (Exception)` with the comment `A unit the table does not know throws out of SizeRule`, and 349 `return null;`. SizeRule.cs:179 `UnitRow unit = UnitTable.ByEnumName(unitEnumName);` throws NotSupportedException for an unknown name (UnitTable.cs:198-202). The null becomes SizeUnknown at SizeRule.cs:144-146 and is counted at ClashViewpointPlan.cs:413-415 under `size could not be read`. That does contradict SizeRule.cs:62 `A unit the table does not know FAILS rather than falling back`. But the trigger cannot occur on Navisworks 2025. The unit is Penetrations.cs:407 `document.Units.ToString()`, the enum was measured at eleven members, docs/history/scan.md:526-528 `Meters = 0 ... Microinches = 10`, and UnitTable.cs:62-77 carries every one of the eleven names, matched case blind at UnitTable.cs:181. Both callers guard a null document first, Penetrations.cs:77 and ViewpointBuilder.cs:153, so the empty string branch at 407 is never handed on. What the catch can really take is a property read throw, ItemSizes.cs:48 `PropertyCategoryCollection categories = item.PropertyCategories;` which sits outside the per-property try at ItemSizes.cs:98, and for F85 that ends as SizeUnknown in the pair folder and counted, which is the designed answer in core.md: `A size that could not be read goes in the PAIR folder and is COUNTED`. No Core test pins the precondition: UnitTableTests.cs:21 asserts only `UnitTable.All.Count, Is.GreaterThan(0)`, and nothing asserts the eleven measured names.

## T1-S13, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/PropertyProbe.cs:112-124
- proof: both
- why: Real as described. The trigger is ordinary: a person opens last week's CSV in Excel, Excel locks it, the next probe of that model throws on the write, and the old CSV is listed in the files written list with its size and named in the verdict block as written. Both callers, FederationEngine.cs:2254 and 2314, reach it. The write and the record carry no Navisworks type, so they can move into Core (a ProbeCsv write that returns whether it wrote) and a Core test can write into a folder under a file, which no system makes, and assert the path is not in the written list. Otherwise a run with the CSV open in Excel proves it.
- evidence: PropertyProbe.cs:114 `File.WriteAllText(csv, ProbeCsv.Text(tally.Rows()), Encoding.UTF8)` sits in a try whose catch at 116-122 logs a failure and says `kept going`, then 124 `log.WriteFinished("CSV", csv)` runs unconditionally. RunLog.cs:1372-1421 reads the size off the disk, adds the path to `written` and logs `CSV      written  path  size`. ProbeVerdict.cs:186 adds `written to           : ` plus the path. core.md: `a file that was checked rather than written is logged with CheckOnDisk`, and RunLog.cs:1497 CheckOnDisk exists for exactly this.

## T1-S14, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/PropertyProbe.cs:198-211
- proof: Core test
- why: Real as described. The CSV exists to show what the model really holds, and a value that would not read is written as an empty value with a count, indistinguishable from one that is empty in the model. How often ClashHarvest.Text throws in practice is UNKNOWN, since every known kind has its accessor and the default uses ToString. The fix belongs in Core beside ProbeTally.CappedMarker: a marker for an unread value, with a test that adds one unread and one empty value for the same property and asserts two different rows. The add-in side then passes the marker from the catch.
- evidence: PropertyProbe.cs:205-209 `catch (Exception) { // Its own try, so one value that will not read costs one cell. value = string.Empty }` then 211 `tally.Add(category, tabName, Words.Or(property.DisplayName, string.Empty), value)`. ProbeTally.cs:66-68 counts the empty string as a distinct value like any other. ClashHarvest.cs:606-607 also maps VariantDataType.None to string.Empty, so a throw, a None and a real empty display string all land in one row.

## T1-S15, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:88 and 93-99, with the key shape in src/Federator.Core/Sets/SetDrift.cs:44-47
- proof: both
- why: Real, and wider than described. The flags carry NegateCondition as well as StartGroup, and this tool's own corrected file writes both, so a set that differs from the file only by a negation, by its grouping, or by a comparison such as not equals reads as the same key and is never called drifted. SetDrift.cs:63-69 justifies leaving flags out only for the Ignore display name bits and says the flags are REPORTED, but nothing reads them. KeyOf is also a second copy of ReadCondition.Key's shape. Proof: one key builder in Core carrying the comparison word and the 32 and 64 bits, with a test that two conditions differing only by flags 32, only by flags 64, or by a comparison other than equals and contains give different keys, which fails today. The add-in reading Options and every comparison value is then proved by a run.
- evidence: SetBuilder.cs:88 `condition.Comparison == SearchConditionComparison.DisplayStringContains ? "contains" : "equals"`, and KeyOf at 95-98 builds category, property, contains or equals, value, with no Flags. SetDrift.cs:46 `return CategoryInternalName + "|" + PropertyInternalName + "|" + Test + "|" + Value`. A grep of src for `.Options` or SearchConditionOptions finds only BuildCondition writing them at SetBuilder.cs:1029-1032, nothing reads them back. The committed corrected matrix exchange/1104-PAR_CLASH_AllInOne_25mm_FIXED.xml carries flags="64" five times and flags="32" six times, and MatrixCorrections.cs:858-859 names 32 as NegateCondition.

## T1-S16, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:101-118
- proof: both
- why: Real as described. A condition value of any kind other than DisplayString or IdentifierString throws in ToDisplayString, becomes an empty string, and the set is reported DRIFTED as asking for an empty value, then replaced when the box is ticked, which breaks the promise at SetBuilder.cs:42-43. Sets this tool builds carry VariantData.FromDisplayString (SetBuilder.cs:1051) and read fine. Whether a saved set of another origin, such as 1A02MM's original import, carries another kind is UNKNOWN, and 5w's 0 unreadable cannot rule it out because this catch hides the throw from the whole search read. Proof: a Core marker on ReadCondition for a value that would not read, with a test that SetDrift.Compare then gives CouldNotRead true and Drifted false, which fails today, and the add-in switched to ClashHarvest.Text, proved by a run.
- evidence: SetBuilder.cs:110-112 `return value.DataType == VariantDataType.IdentifierString ? value.ToIdentifierString() : value.ToDisplayString()` and 114-116 `catch (Exception) { return string.Empty }`. SetDrift.cs:103 sets CouldNotRead only when `asked == null`, so a value read as empty compares as a real question and 118-121 marks Drifted. SetBuilder.cs:648-651 then rebuilds when the box is on. addin.md: `A property value is read by its KIND, never with ToDisplayString alone`. ClashHarvest.cs:602-650 is the kind based reader, internal static in the same assembly.

## T1-S17, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:271-281 and 302-309
- proof: a run
- why: Real as described and the disposal gap is wider than the finding says. With the rebuild box on, a set the file no longer names that is pointed at only by tests a person moved into a Clash Detective folder, or only by the second source of a side, counts 0 sides and is removed, which orphans those sides (5z measured the side stops resolving). None of the wrappers read here is disposed: the SavedItem at 273, the two ClashSelection wrappers at 280-281, the Selection at 302 and the SelectionSource at 309, against the add-in rule that everything created or resolved is disposed. The walk is Navisworks only, so a run with a test in a folder pointing at a leftover set, box on, proves it refused rather than removed.
- evidence: SetBuilder.cs:273 `ClashTest test = tests.Tests[t] as ClashTest` then 275-278 `if (test == null) { continue }` with no descent into a folder, and 309 `ResolveSelectionSource(sources[0])` reads only the first source of a side. Every other test walk descends folders: SavedTests.cs:84-91, ClashRunner.cs:1573-1578, ViewpointBuilder.cs:912-925, DocumentCensusReader.cs:213. SavedTests.cs:67-68 wraps SelectionA and SelectionB in using. SetLeftovers.cs:159-161 gives Remove at 0 sides and SetBuilder.cs:579-580 logs `REMOVED ... which nothing pointed at`.

## T1-S18, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:284-291, with the decision in src/Federator.Core/Sets/SetLeftovers.cs:159-161
- proof: both
- why: Real as described. A throw part way through the tests also throws away the sides already counted. The log line says no set is removed while every leftover the file does not name is then removed as pointed at by nothing. Core has no way to say a count is unknown, since Sides at minus one is also Remove today. Proof: a Core test that a DocumentSet whose side count could not be taken gives Refuse, which fails today, and the add-in passing that state or stopping HandleLeftovers when the read fails, proved by review or a run.
- evidence: SetBuilder.cs:286-291 `log.Failure("reading what the clash tests point at", error, "no set is removed or renamed and the run goes on")` then `return new Dictionary<string, int>(StringComparer.Ordinal)`. ReadEverySetInTheDocument at 184 carries on with it and 243 gives every set `sides.TryGetValue(...) ? pointing : 0`. SetLeftovers.cs:159-161 `if (set.Sides <= 0) { leftovers.Add(... LeftoverAction.Remove ...) }` and CarryOut at 574-581 removes and logs REMOVED. Only reached when `rebuilds.RebuildDriftedSets` at 506.

## T1-S19, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:320-325
- proof: both
- why: Real as described. The comment argues both ways in one sentence and the code does the unsafe one: a side that throws while resolving could point at any set, so every leftover loses a side it may have had and one that falls to 0 is removed and logged as pointed at by nothing. How often a side read throws is UNKNOWN. The same Core test as T1-S18, an unknown side count gives Refuse, is the proof, with the add-in marking the whole count unknown when any side throws.
- evidence: SetBuilder.cs:322-324 `A side this tool cannot read is a side it does not count, which is the safe direction: an uncounted side makes a set look SAFER to remove, so it is never counted and the refusal errs towards leaving things alone.` An uncounted side lowers the count at 317, and SetLeftovers.cs:159 removes at `set.Sides <= 0`.

## T1-S20, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:646 and 665-704
- proof: a run
- why: Real as described. A present set that could not be found again is recorded as finding 0 items rather than UNKNOWN, judged as empty on the old question, and counted at zero across the run. Without a rebuild the second find cannot plausibly miss, since no mutator sits between the two finds on the same parent. After a rebuild it depends on T1-S21, which is UNKNOWN. A run that rebuilds a drifted set shows whether the miss happens, and otherwise the fix, starting at minus one, is proved by review.
- evidence: SetBuilder.cs:646 `int found = 0` and 665-678 set it only `if (now != null)`, then 683-684 `outcome.AddAlreadyPresent(planned.Path, planned.Name, planned.ConditionCount, found)` and 701-704 `if (found == 0 && !drift.CouldNotRead) { outcome.AddEmpty(EmptySets.Why(planned.Path, asking)) }`, where `asking` is still `drift.Asked`, the question read before the rebuild. SetsAcrossTheRun.cs:134 skips only `result.ItemCount < 0` and 152-154 counts 0 as a group at zero. SetResult.cs:43 `Minus one when it never resolved`.

## T1-S21, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:155 and 665
- proof: a run
- why: Real as a code shape: a handle is held across a mutator, against the add-in rule and against the create path in the same method. What ReplaceWithCopy does to a parent handle held across it is UNKNOWN. If it behaves like AddCopy, the count, AskedNow and the 3b judgement after a rebuild come off the old set, the fault 655-661 says was fixed, and if the handle dies the outer catch reports a set FAILED that was in fact rebuilt. Only a run answers it, comparing the count after REBUILT with a count read after re-resolving the folder from a fresh RootItem.
- evidence: SetBuilder.cs:631 `using (GroupItem parent = EnsureFolders(sets, planned.Folders))`, the same `parent` passed to Rebuild at 650, used by 155 `sets.ReplaceWithCopy(parent, at, made)`, then read again at 665 `using (SelectionSet now = FindSelectionSet(parent, planned.Name))`. The create path re-resolves instead: 750 `using (GroupItem fresh = ResolveFolders(sets, planned.Folders, planned.Folders.Count))`, and 809-812 `A handle held across an AddCopy does not show the new child`. Rebuild's own doc at 126-128 says the parent is resolved FRESH, but it is the one resolved at 631 before the mutator. No log in steps/logs carries the line `and it now finds`, so no run has read this path since it was written.

## T1-S22, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ViewpointBuilder.cs:668-708 and 782-789, with src/Federator.Addin/Engine/SavedViewpoints.cs:221 and 433-441
- proof: a run
- why: Real in the code as described. A viewpoint whose two items are not both placed is recorded with whatever dimming and red and green paint the previous viewpoint left, and counted as undimmed. When the kept models change between pairs, models dimmed for an earlier viewpoint stay dimmed while hidden for the rest of the group, which is the shape SavedViewpoints.cs:433-437 says put 33 MB into a 120 KB NWF. Whether a new root transparency replaces an earlier leaf colour is UNKNOWN, and the read back at 760 only checks a count above zero, so the extra overrides are invisible to it. A run proves it: the material override count on an undimmed viewpoint and on viewpoints after a pair change, and the NWF size, before and after an Undim at the top of each viewpoint.
- evidence: ViewpointBuilder.cs:668 `if (views.DimsAnything && place != null && place.BothPlaced)` is the only block that touches temporary materials, and it calls Undim only at 698-705 when its own dim failed. Nothing else between viewpoints resets them: ShowOnlyModels at SavedViewpoints.cs:362 resets hidden state only. SavedViewpoints.cs:221 `view.ApplyMaterialAttribs = true` on every record. 438-441 dim only `RootsOf(document, shown)`. ViewpointBuilder.cs:786-789 counts the skipped case as `notDimmed++`, logged at 223 as `written undimmed`. Undim runs at the end of the group in PutBack at 819-837.

## T1-S23, PARTLY, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Ui/FederatorWindow.xaml.cs:95 with src/Federator.Core/Diagnostics/FolderMemory.cs:116-121 and 312-315
- proof: Core test
- why: The unreadable half is exactly as described. A folders.txt that throws on ReadAllLines leaves the memory empty, and the startup block says nothing was remembered yet, which reads as a first run rather than a file that would not read. The unwritable half works differently. The block is written before any Save, so it cannot know about a later write failure, and a readable but unwritable file lists its folders normally. What goes wrong there is that the failed Save is never logged, which is T1-S33. Where Load() itself fails, Path is null, nothing survives a restart, and nothing says so either. The fix can live in Lines(), which a Core test can call with a memory whose read threw and assert it names the reason.
- evidence: FederatorWindow.xaml.cs:95 `log.Block("FOLDERS REMEMBERED", folders.Lines())`, written once in the constructor. FolderMemory.cs:118-120 "A settings file that will not read is not worth a message." then `memory.DisabledReason = error.Message`. FolderMemory.cs:312-314 adds "Nothing remembered yet. Every picker opens where it always did." when no folder is held. A grep of src for DisabledReason finds only the setters in FolderMemory.cs (61, 78, 120, 280, 285) and RunLog's own unrelated property. The only reader anywhere is FolderMemoryTests.cs:212.

## T1-S24, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Ui/FederatorWindow.xaml.cs:237-243, 254, 288 and src/Federator.Addin/Ui/GroupRow.cs:39
- proof: both
- why: Unticking one file in one building rebuilds every row. Every hand typed cell in every other building is lost, and every group the person had unticked in Run is ticked again. The comment is false for any group whose key did not change. Regroup also leaves lastRefill as it was, so the preview at 638-640 can still say rows were typed over by hand and left alone after they were thrown away. The only thing that might catch a reverted NWF name is F71's near NWF sentence (GroupRow.NearbyNwf), and only when the two names are close by that rule. Keeping hand typed cells by group key can live in Core, in OutputNameTable, with a test that types a name, rebuilds after removing one file of another group, and asserts the name survives. Keeping the Run tick is in GroupRow in the add-in and needs a run.
- evidence: FederatorWindow.xaml.cs:239-241 `if (e.PropertyName == "Include")` then `Regroup()`. Line 254 `groups.Clear()`. Line 288 `nameTable = OutputNameTable.From(result.Groups, naming, settings)` under the comment at 285-287 "any hand edit belonged to groups that no longer exist". GroupRow.cs:38-39 `GroupRow row = new GroupRow(...)` then `row.include = true`. FederatorWindow.xaml:127-128 binds the Files grid "Use" column to FileRow Include, and 184-185 binds the Groups grid "Run" column to GroupRow Include. OutputNameTable.cs:151-159 builds fresh rows with no by hand flag.

## T1-S25, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Clash/ClashRunOutcome.cs:429-440
- proof: Core test
- why: A NoTolerance skip is counted in the total but has no reason row in Lines() and no SKIPPED group in SkipLines(). The reason rows then add up to less than tests skipped, and the promise at 1810 is broken for that reason. LogSkip still names the first five as they happen, because it uses Describe, which knows NoTolerance. SingleDisciplineGroupTests.EverySkipReasonStillHasWordsForIt checks Describe only, and no test puts a NoTolerance skip into an outcome. A Core test that adds one NoTolerance skip and asserts SkipLines names it and the rows sum to SkippedCount, or loops every enum value, fails today.
- evidence: The array in SkipReasonsInOrder lists SingleDiscipline, EmptySide, LocatorNotResolved, UnknownTestType, NoLocator, UnknownUnits, NoName, Failed. ClashSkipReason.cs:26 declares NoTolerance. ClashTestPlan.cs:245-253 adds `ClashSkipReason.NoTolerance` for a test with no tolerance attribute. ClashRunner.cs:364-366 `outcome.AddSkipped(test)` for every resolved.Skipped. ClashRunOutcome.cs:281 "tests skipped     : " + SkippedCount counts it. FederationEngine.cs:2613 logs `clash.Lines()`. ClashRunner.cs:1810 "The block at the end carries the total."

## T1-S26, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Clash/PriorityMap.cs:136 and 60-64
- proof: Core test
- why: A CSV that gives one test A on one line and C on a later line gives it C. No problem line is written, so the workbook priority column, the block order and the viewpoint folder all follow a row nobody was told about. RowCount also leaves out problem rows, so the logged row count is the number of distinct usable names, not rows. No Core test covers a repeated name. A test that reads two rows with one name and different letters, and asserts a problem is recorded and RowCount matches the file, fails today.
- evidence: PriorityMap.cs:136 `map.byTestName[name] = priority`, with no check for a name already held. Compare ByDesignPairs.cs:181-185 `if (found.byKey.ContainsKey(pair.Key))` then the problem "names the same pair as an earlier line". PriorityMap.cs:60-63 doc "How many rows the file holds." over `get { return byTestName.Count }`. FederationEngine.cs:243-244 logs "read " + path + ", " + priorities.RowCount + " rows". MatchLines at 198 writes the same number as "in the file".

## T1-S27, CONFIRMED, noise

- how: two readers agree
- cause: src/Federator.Core/Clash/RepeatedFailureGuard.cs:61 and 81
- proof: Core test
- why: The guard counts failures in a row anywhere in the run, including across groups. After 199 good tests, 50 failures in a row stop the run, and both the log and the window then say the first 50 failed, which tells the reader nothing ever worked. Stopping is right, per the class summary at 9-11 "enough tests in a row", so only the words are wrong. core.md says "After the first 50 tests", which is the same wrong word in the rule. A Core test that records successes and then 50 failures, and asserts Reason and ReasonInPlainWords do not say first, fails today.
- evidence: RepeatedFailureGuard.cs:61 "the first " + consecutive + " tests all failed for the same reason". Line 81 "The run was stopped. The first " + consecutive. RecordSuccess at 90-94 sets `consecutive = 0`. FederationEngine.cs:143 builds one guard for the run and hands it to every ClashRunner at 2516. ClashRunner.cs:908 calls `guard.RecordSuccess()` after every test that ran. ClashImages.cs:104 "the last " + guard.Consecutive.

## T1-S28, CONFIRMED, noise

- how: two readers agree
- cause: src/Federator.Core/Clash/ToleranceChoice.cs:78-79
- proof: Core test
- why: The grey line under the drop down tells a person the opposite of what 5y measured and what the confirm screen then says, and the class's own comment says the wrong wording made a person hesitate over an action that costs nothing. The same old claim also survives in ClashRunner.cs:1145-1146, "because an edit resets its results", and in the log line at 1106-1108, "which reset its results". Both are outside this finding. A Core test asserting HelpLine does not say results are reset fails today.
- evidence: HelpLine = "Beats the XML and the document. Changing a saved test resets results". FederatorWindow.xaml.cs:1040 `ToleranceHelp.Text = ToleranceChoice.HelpLine`. The same class at 50-52: "Changing the tolerance on a test already in the document RESETS NOTHING". WarningLines at 239: "Every recorded result and every status a person set is KEPT." ClashRunner.cs:1199 "Its results and its statuses are KEPT, measured 5y". ToleranceChoiceTests.cs:216-223 checks the word count and one identifier only.

## T1-S29, CONFIRMED, noise

- how: two readers agree
- cause: src/Federator.Core/Clash/ToleranceChoice.cs:198-204, called from src/Federator.Addin/Engine/ClashRunner.cs:396 and 1225
- proof: both
- why: On a weekly run with no XML picked, every group logs that the tolerance was read per test out of the XML, then says N were left as the document has them. The first half is false, and the log is the evidence Bader reads. The wording is in Core, so a LogLine that is told the source can be proved by a Core test. The add-in call at 1225 then has to pass plan.Source, which is reviewed and seen in a run.
- evidence: ToleranceChoice.cs:200-203 `Prefix + " read per test out of the XML, which is what this tool does "` ... " left as the document has them". ClashRunner.cs:396 `WriteTheToleranceLine()` runs on every group with no condition. On the no XML path FederationEngine.cs:2504 `plan = ClashTestPlan.FromDocument(SavedTests.Read(document), units)`, and ClashRunner.cs:610-617 `if (planned.IsFromDocument)` goes to `ApplyChosenTolerance(clashTests, address, planned.Name)`, which counts at 1152. LogLine takes no plan source.

## T1-S30, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ClashRunner.cs:1152
- proof: both
- why: The line says the chosen tolerance was set on every saved test it counted. A test that could not be resolved, whose copy was not a clash test, or whose edit threw does not carry the chosen tolerance, and the unresolved case writes nothing at all. A test already at the value does carry it, so counting that one is arguable. The finding missed the copy not a clash test case at 1181-1187. A Core tally with one count per outcome, and a line that says them apart, can be Core tested. The counting at each return is in the add-in and needs a run.
- evidence: `toleranceOnExisting++` is the first line of ApplyChosenTolerance, before `if (!Tolerance.ChosenInTheTool)`. Early exits: 1165-1168 `if (existing == null)` returns with no line, 1172-1175 returns when already within TestDrift.ToleranceEpsilon, 1181-1187 returns when the copy is not a ClashTest, 1206-1211 catches a throw. Only 1189-1190 `copy.Tolerance = wanted` and `clashTests.TestsEditTestFromCopy(existing, copy)` sets it. ToleranceChoice.cs:206-208 "Set on " + total + " tests, " ... " already in the document."

## T1-S31, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Clash/UndoAutoReview.cs:55-71 together with src/Federator.Addin/Engine/ClashStatusEditor.cs:240-245
- proof: both
- why: After one undo the clash is at New and still carries our record. If a person then sets it to Reviewed by hand, the next undo reads the same record, sees Reviewed, and moves it to New. The UNDO block counts that as put back where this tool found it. Nothing on the clash tells the tool's Reviewed apart from a person's, which breaks core.md "ONLY WHERE NOBODY HAS MOVED IT SINCE" and "NEVER OVERWRITE A DECISION". Whether Navisworks keeps or adds comments when a person changes a status in the panel is UNKNOWN. That does not change the verdict, because OurComment reads only comments with the marker. AutoReviewRecord.MayUndo at 162 and WhyNotUndone at 171 hold a second copy of the same judgement with the same gap, and nothing in src calls either. The judgement can be fixed in Core, for example an undo leaves its own record and Judge reads the last one, with a Core test. Writing that record is in ClashStatusEditor and needs a run.
- evidence: UndoAutoReview.cs:60-70: no record gives NotOurs, `if (now != ClashStatus.Reviewed)` gives SomebodyMovedItOn, otherwise `putBackTo = record.WasAt` and PutBack. ClashStatusEditor.cs:240-242 "An undo writes none: the record it is undoing stays as the history of what happened", with `if (wanted.Record != null && !wanted.AsUndo)`. UndoAutoReviewed.cs:184-209 OurComment returns the last comment carrying the marker. UndoAutoReview.cs:32-34 "pulling it back to New would throw that person's decision away".

## T1-S32, PARTLY, silent wrong output

- how: a third reader settled it, reader A PARTLY, reader B CONFIRMED
- cause: C:\Users\p003653k\OneDrive - Parsons Corp\Documents\GitHub\PAR_NWC-Federator\src\Federator.Addin\Engine\ClashRunner.cs:1202 and 1328, which pass a clash tolerance through C:\Users\p003653k\OneDrive - Parsons Corp\Documents\GitHub\PAR_NWC-Federator\src\Federator.Core\Diagnostics\EventRow.cs:199-202. EventRow.Exact itself is not the fault.
- proof: both
- why: The consequence the finding describes is real on main. The .tsv number column holds a clash tolerance rounded to three decimals. 0.2460629921 ft becomes 0.246, 25 mm in a feet document (0.0820209974) becomes 0.082, and 25.4 mm in metres becomes 0.025. The attribution is wrong. The finding puts the fault in Exact at EventRow.cs:199, saying it contradicts the Number comment. It does not. Exact does what its own summary and test say, and it is right for the four duration callers in RunLog. The fault is the two add-in call sites choosing a size or duration formatter for a tolerance. The harm differs between the two sites. At 1328 the same row's text column carries the full value, so only the number column is lossy. At 1202 the new tolerance is in that row only in rounded form. From the sixth saved test on, NumberedRepeat writes no text line. The value is the same for every test in a group, though, so the first five text lines and the group TOLERANCE line (ToleranceChoice.Converted, six decimals) still carry it more precisely. Nothing in src reads these rows back, so the harm is limited to a person or script reading the .tsv. That reader sees a tolerance that differs from what was set by far more than TestDrift.ToleranceEpsilon. Reader A has this right. Reader B's CONFIRMED accepts the finding's cause in EventRow, which is the part that does not hold. The fix proof can be a Core test on a tolerance formatter that keeps the full value, which fails if 0.246 comes back for 0.2460629921. Moving the two ClashRunner call sites to it is proved by review and a run, because ClashRunner is add-in code.
- evidence: EventRow.cs:198-201 `/// <summary>A size or a duration, with three decimals and no separators.</summary>` over `return number.ToString("0.###", CultureInfo.InvariantCulture);`. EventRow.cs:74-76 says Number is text because forcing a count, a size and a duration "through one type would lose either the precision or the meaning". That is about the column type and makes no promise about a formatter. EventRowTests.cs:151 `Assert.That(EventRow.Exact(742.1255), Is.EqualTo("742.126"));` pins the three decimals on purpose. Its other four callers are all seconds: RunLog.cs:950 `EventRow.Exact(step.Seconds)`, 1128, 1310 and 1326. The two misuses are ClashRunner.cs:1202 `EventRow.Exact(wanted),` whose text column is only `"was " + Plain(already)` at 1203, and ClashRunner.cs:1328 `EventRow.Exact(set),` whose text column at 1329 is `DescribeSetTolerance(planned, set)`. That call gives PlannedClashTest.Format `"0.##########"` or Plain `"0.############"`. RunLog.NumberedRepeat at RunLog.cs:668 writes the row every time but writes the text line only for the first five of a key.

## T1-S33, PARTLY, broken feature

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/FolderMemory.cs:241-242, with the eight window callers at src/Federator.Addin/Ui/FederatorWindow.xaml.cs:130, 913, 926, 1157, 1220, 1458, 1471, 2129
- proof: both
- why: The fault is real. When folders.txt cannot be written, every folder remembered in the session is gone at the next restart, and nothing in the log or the window says why. The two rules cited are misread, though. The Try forms rule in addin.md is about Navisworks Try calls, "For the NWD that bool is the only thing separating a fresh publish from last week's file", and FolderMemory.Save is not one of them. core.md asks only that "an unwritable location is recorded as a reason", and line 285 does record it. What this does break is CLAUDE.md "The tool reports what it noticed" and "A public member nothing in src calls is deleted with its tests", because DisabledReason has no reader in src. It has the same root as T1-S23. Returning the reason from Remember can be Core tested with the unwritable path the test at 196-213 already builds. The window logging it is in the add-in and needs a run.
- evidence: FolderMemory.cs:241-242 `folders[kind] = value` then `Save()`, with the bool dropped and Remember returning void. Save at 283-286 catches, sets `DisabledReason = error.Message` and returns false. No src code reads DisabledReason. FolderMemoryTests.cs:196-213 proves the reason is set but never that it is reported.

## T1-S34, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/LiveLine.cs:231-241 (OnTheGroupBefore), fed once per test from src/Federator.Addin/Engine/ClashRunner.cs:636, 732 and 865 through FederationEngine.cs:146 and 1038-1043
- proof: Core test
- why: One test's seconds are compared with twice the whole of the same step on the group before. If that group ran 200 tests, one test has to take longer than 400 tests' worth before SLOWER shows. It can only fire when the group before had a single visit of the step. The steps opened once per group, such as NWD and APPEND, compare like with like and are fine. A Core test that gives OnTheGroupBefore a group with many visits and asserts what a single visit is compared with fails today.
- evidence: LiveLine.cs:236 `seconds += record.Seconds` runs over every record of the group before, and the method returns the sum. RunLog.cs:931 adds one record per visit: `stepRecords.Add(new StepRecord(currentGroup, step.Name, step.Depth, step.StartedAt, step.Seconds, step.Threw))`. ClashRunner.cs:732 `StepStarted(RunSteps.TestsRun)` runs once per test, and LiveLine.cs:130 `stepStartedAt = Now()` resets the clock, so SecondsOnStep is one test. LiveLine.cs:172 `return lastGroupSeconds > 0 && SecondsOnStep() > lastGroupSeconds * SlowerThan`. LiveLineTests.cs:283-294 pins the sum (10 plus 15 gives 25), and no test compares one visit with a per visit figure.

## T1-S35, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:268 and 358, with the pattern at 34
- proof: Core test
- why: This only happens on the fallback, when %LOCALAPPDATA%\ParsonsNwcFederator\logs cannot be opened. The folder is then the shared %TEMP%, and another program's run-*.log there counts as one of ours and is deleted once more than 30 are present. Whether any program on the 27 machines writes run-*.log into %TEMP% is UNKNOWN. A smaller second fault: the .tsv files beside logs in temp are never pruned. Proof: a Core test that makes the preferred folder unusable and puts a foreign run-other.log among 35 in the fallback folder, then asserts it survives. It could also assert that the fallback folder belongs to this tool.
- evidence: RunLog.cs:268 `foreach (string folder in new[] { preferredFolder, System.IO.Path.GetTempPath() })` leads to `return Start(folder, startedAt, keepLogs)`. Start ends with `log.PruneOldLogs(folder, keepLogs)`, which lists `new DirectoryInfo(folder).GetFiles(LogFileSearchPattern)`, where `LogFileSearchPattern = FileNamePrefix + "*" + FileNameExtension` is run-*.log. It then deletes every one past the newest keepLogs minus one. The comment at 30-33 says "nothing else that happens to be sitting in the folder is touched". RunLogRetentionTests.cs:260-270 names the fallback but passes its own folder as the preferred one, so the bare system temp folder is never tested.

## T1-S36, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:482-490, with the lists it never resets at 44-45, 66, 77, 644 and 113-115, and src/Federator.Addin/Ui/FederatorWindow.xaml.cs:2073
- proof: Core test
- why: If Run is pressed a second time in the same window, the RESULT covers both runs. That includes groups done, partial and failed, files written, errors, clashes found, the penetration and by design lines, and the collapsed counts. The run timing block divides both runs' group seconds by the second run's time. Waiting for the person includes the whole first run, because StartedAt is measured from the session start. A penetration line also stays after a second run with the box off. No test puts two runs through one log. A Core test with two cycles of RunStarted, GroupFinished and RunFinished on one log, asserting the second RESULT counts only the second run's groups, fails today. If the fix goes in the window instead, a run proves it.
- evidence: RunStarted only sets `runStartedAt = ElapsedSeconds`. The only Clear calls in RunLog are visitsInGroup at 1118 and 1235 and censusFaults at 1236. The log lives as long as the window: FederatorPlugin.cs:46 `RunLog log = RunLog.StartOrDisabled()`, then ShowDialog, then `log.Dispose()` at 93. The RunJobs finally sets `RunButton.IsEnabled = true`. The engine only ever adds: FederationEngine.cs:2619 `log.ClashesFound.Add(job.Building, clash.TotalClashes, clash.WithClashesCount)`, 2648 `log.PenetrationsMoved += penetrationTally.MovedCount`, and 2537 `log.PenetrationsWanted = true`, which is never set back. RunLog.cs:1761 `TimingBlock.ForRun(GroupRecords, StepRecords, where.RunSeconds)`.

## T1-S37, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:514-516, with the call at src/Federator.Addin/Ui/FederatorWindow.xaml.cs:2056 inside the try
- proof: Core test
- why: A scanned run that throws after RUN started gets a timing block and a RESULT that say no run was marked and blame the open file run or the hand buttons. Every share is then worked off the session, waiting included. A second run in the same window that throws behaves differently: runFinishedAt still holds the first run's finish, so the block says a run was marked and gives it 0 seconds, through RunClock.Never on a negative. LogTrimmingTests.cs:164-177 only covers a run that never started. A Core test that calls RunStarted and then WriteResultBlock with no RunFinished, and asserts the block does not say no run was marked, fails today.
- evidence: RunLog.cs:514 `return runStartedAt < 0 || runFinishedAt < 0 ? RunClock.NotMarked(ElapsedSeconds) : RunClock.From(ElapsedSeconds, runStartedAt, runFinishedAt)`. RunJobs calls `log.RunStarted(jobs.Count)` at 1973 and `log.RunFinished()` at 2056, both inside the try. The finally at 2071 writes the result. RunClock.cs:115-116 then writes "no run was marked, so the run time below IS the session time. That is what the open file run and the two hand buttons give", and RunLog.cs:1938 adds ", which is the session, no run was marked". PickedExchange at 1977 reads the picked XML and can throw between the two marks.

## T1-S38, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:687-688, 716-718 and 1922
- proof: Core test
- why: When the .tsv did not open, or a write to it failed partway through the run, the text log still says every collapsed line is in the .tsv. The start line from RowLog.WhereItIs does say there is no machine readable log, and RunLog.cs:1950 prints NOT ON DISK for it, so the log contradicts itself rather than hiding the problem. Proof: a Core test that puts a folder at the .tsv path before Start, writes six NumberedRepeat lines with one key plus the RESULT block, and asserts neither claim appears. It fails today.
- evidence: NumberedRepeat writes "every further line of this kind is counted and not written out. The machine readable log carries every one of them" without checking the row file. CollapsedLines writes "Every one is in the machine readable log". WriteResultBlock writes "lines collapsed in this file, all of them kept in the .tsv beside it:". RunLog.Row returns at 743-746 when its RowLog is null, which is the disabled log. RowLog.WriteLine returns at 138-141 on `if (closed || writer == null)`, and StartBeside at 107-111 returns a RowLog with no writer when the .tsv will not open. A write that throws is swallowed at RowLog.cs:152-156.

## T1-S39, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:1348
- proof: Core test
- why: The .tsv row for an NWC that is not on disk says 0 bytes, a size that was never read. That breaks the rule that a size is only logged after File.Exists passes and the real size is read back, and the row says something the text log does not. A Core test that calls AppendFinished on a missing file and asserts the .tsv number column is not 0 fails today.
- evidence: RunLog.cs:1342 `long size = SizeOnDisk(file)`, then the row gets `EventRow.Count(size < 0 ? 0L : size)`, while the text line uses DescribeSize(size), which gives "NOT ON DISK" for minus one. WriteFinished at 1376-1380 and CheckOnDisk at 1497-1504 write no size row for a missing file. RunLogTests.cs:469-479 checks only the text line. The caller is FederationEngine.cs:1298 `log.AppendFinished(file, appended)`, once per file.

## T1-S40, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:1939-1941
- proof: Core test
- why: Every open file run and every hand button press ends with a RESULT that states 0.000s of waiting. Nobody measured that number, and it sits beside a run time that already counts the waiting. LogTrimmingTests.cs:164-177 checks the unmarked RESULT but not this line. A Core test that calls WriteResultBlock with no RunStarted and asserts the waiting line is absent or says UNKNOWN fails today.
- evidence: `Line("waiting for the person : " + spent.WaitingSeconds.ToString("0.000", CultureInfo.InvariantCulture) + "s, which is not work this tool did")` is written every time. For an unmarked clock, RunClock.cs:45 sets `StartedAt = marked ? Never(started) : 0.0`, so WaitingSeconds is 0. RunClock.Lines at 113-118 leaves that row out when not marked. The run time line at 1937-1938 adds ", which is the session, no run was marked", and the waiting line adds nothing.

## T1-S41, CONFIRMED, silent wrong output

- how: a third reader settled it, reader A PARTLY, reader B CONFIRMED
- cause: C:\Users\p003653k\OneDrive - Parsons Corp\Documents\GitHub\PAR_NWC-Federator\src\Federator.Core\Exchange\ExchangeReader.cs lines 373-390 (ReadFlag and ReadInt), used at 139 (merge_composites), 184-185 (selfintersect, primtypes) and 262 (flags)
- proof: Core test
- why: Every claim in the finding holds on main at 0eb4ede. A missing or unreadable merge_composites, selfintersect or primtypes becomes false or 0 with no word said, while a missing tolerance is skipped by name and a bad one throws, which is exactly the asymmetry the finding names and the core.md rule forbids. It is latent, since every sample file writes all four, but a file from another project or another exporter would hit it. The worst case is primtypes: a missing attribute sets a side to 0 with no log line, and whether Navisworks finds any clash on a side with no primitive types is UNKNOWN, so a created test could come back with zero clashes and read as passed. For a test already in the document, TestDrift would report the file as asking for 0, which names a value the file never wrote. For flags the finding is also true, and the harm is smaller: flags is a condition option and not a per test setting, and a missing flags reading as 0 may be the right default, since the first group carries no bit, though whether an exporter ever omits it is UNKNOWN. An unreadable flags is clearly wrong, because it silently drops bit 64 or 32 and turns an OR into an AND or loses a negation. Reader A's PARTLY rests on the flags nuance and on latency, and neither contradicts what the finding says, so CONFIRMED. Proof: a Core test that reads a clashtest with no primtypes, and one with merge_composites="x", and asserts the test is skipped by name or the reader refuses, the way a missing tolerance is. It fails today because both are read as 0 and false and planned as buildable.
- evidence: ReadInt: `if (string.IsNullOrEmpty(text)) { return 0; }` then `return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;`. ReadFlag: `return ReadInt(text) != 0;`. Callers: `ReadFlag(Attribute(test, "merge_composites"))`, `ReadFlag(Attribute(selection, "selfintersect"))`, `ReadInt(Attribute(selection, "primtypes"))`, `ReadInt(Attribute(condition, "flags"))`. The tolerance, by contrast: `!string.IsNullOrEmpty(toleranceText)` becomes HasTolerance at line 136, and ReadDouble at 406-411 does `throw new InvalidDataException("Could not read the " + attributeName ...)`. ClashTestDefinition and ClashSideDefinition in ExchangeModel.cs carry no HasMergeComposites, HasPrimitiveTypes or HasSelfIntersect. ClashTestPlan.Plan skips on `if (!test.HasTolerance)` with ClashSkipReason.NoTolerance and has no check for the other three, and Side() at 339 passes `side.SelfIntersect, side.PrimitiveTypes` straight on. The add-in sets them on the new test: ClashRunner.cs 1300 `test.MergeComposites = planned.MergeComposites`, 1445-1446 `side.SelfIntersect = planned.SelfIntersect` and `side.PrimitiveTypes = PrimitiveTypesFrom(planned.PrimitiveTypes, planned.Locator)`, and PrimitiveTypesFrom logs only unknown bits, so 0 passes with no line. SetBuilder.cs 1030 casts `(SearchConditionOptions)condition.Flags`. core.md: "Per test, everything comes from the file and never from a constant: the name, the test type, the tolerance, merge composites, and per side the self intersect and the primitive type flags." The only reader test on these, ExchangeReaderShapeTests, asserts good values only (`Right.PrimitiveTypes, Is.EqualTo(3)`, `Flags, Is.EqualTo(32)`), nothing with the attribute missing or garbled. Samples: all 1830 clashtests in the two AllInOne files carry `merge_composites="1"`, all 3660 clashselections carry `selfintersect="0" primtypes="1"`, and all 102 conditions carry a numeric flags.

## T1-S42, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Exchange/MatrixCorrections.cs:643-644
- proof: Core test
- why: For Category X and Workset V the output is Cat X, Work V, then Work V' with 64, which reads (X and V) or (V'). The set would then take every element on V' whatever its category. On a four condition set, each added workset row becomes a group of its own. What the doc means needs the whole group copied with the other spelling: (X and V) or (X and V'). This is latent. There is no src caller, the committed matrix is made without Or rows, and MatrixCorrectionsTests.cs:637-654 pins that no disagreeing workset is a value the matrix filters on. ME-DUCTWORK in the finding's example is handled by the value rewrite, not an Or row, so that example is illustrative. A Core test that adds an Or row to a two condition set, reads it back through ExchangeReader and SetBuildPlan.Groups, and asserts every group still holds the category condition, fails today.
- evidence: `string or = OneCondition(template, "equals", StartGroup, row.AlsoAccept)`, then `text = text.Substring(0, closes) + or + text.Substring(closes)`, which puts one flags="64" condition straight after the matching condition. SetBuildPlan.cs:219-223 starts a new group on every StartGroup condition: `if (current == null || condition.StartsAGroup)` builds a new list and adds it to groups. The doc at 252-255 promises "or the workset equals the other spelling". The only Or row tests, MatrixCorrectionsTests.cs:585-629, use the one condition set built by Set() at 145-152. The generator at 564-565 passes no Or rows, and nothing in src calls MatrixCorrections.

## T1-S43, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Exchange/MatrixCorrections.cs:898, and the CategoryRewrite constructor at 55-71
- proof: Core test
- why: If From appears in the set's own name, the set is renamed inside its block while the test locators outside it keep the old name, so they stop resolving. If To contains From, for example Fixtures to Electrical Fixtures, the second pass finds From again and the value grows every time. Where From is also in the name, the second pass instead finds no set. This is latent: nothing in src calls MatrixCorrections, and the generator at MatrixCorrectionsTests.cs:564-565 passes null for these rewrites, so only tests use CategoryRewrite. Two Core tests prove it. One uses a set whose name holds From and asserts the name is unchanged. The other applies a rewrite whose To holds From twice and asserts TotalChanged is 0 the second time, or that the constructor refuses it. Both fail today.
- evidence: The block runs from `text.IndexOf(SetOpens + rewrite.SetName + "\"", StringComparison.Ordinal)` to `</selectionset>`, and the method returns `text.Substring(0, at) + block.Replace(rewrite.From, rewrite.To) + text.Substring(ends)`, so the name attribute inside the block is replaced as well. SetRename refuses at 26-31 with `if (to.IndexOf(from, StringComparison.Ordinal) >= 0)`, but the CategoryRewrite constructor only checks for empty strings. The class doc at 382-385 says "SAFE TO RUN TWICE IS THE WHOLE POINT". ARewriteChangesTheNamedSetAndNoOther at MatrixCorrectionsTests.cs:221-222 asserts on a different set's name, so the rewritten set's own name is never checked.

## T1-S44, PARTLY, silent wrong output

- how: a third reader settled it, reader A CONFIRMED, reader B PARTLY
- cause: src/Federator.Core/Exchange/RevitWorksets.cs:131-134, the null stream return, and the catch at 159-163. Neither sets a flag and neither leaves a record. RevitWorksets has no ResourceFound and no Line, unlike RevitCategories.cs:94-120
- proof: Core test
- why: I agree with reader B. The code fault is real on main. A missing or unreadable list looks exactly like a list with nothing in it; the catch swallows the error with no record, and nothing tells the person the decided pairs list was not read. The result is a normal looking export check that names two pairs a person already decided are not typos. That is the harm, a quiet wrong output. The description is wrong about the route. It says a build that lost revit-worksets.txt would quietly bring back every decided pair, but that build fails two Core tests that Actions and the pre-commit hook run. The resource sits inside the same DLL as the code that reads it. So the one route left on a shipped build is a read that throws inside the catch at run time. Nothing can show how likely that is, so it is UNKNOWN. The EmptySets half is honest but gives no reason: it says cannot tell, which is true, and never says why. A fix can be proved without Navisworks. Add a seam that loads the list from a given stream, the way the ResourceFound rule in RevitCategories is shaped. Then write a Core test that hands it a null stream and a stream that throws, and asserts ResourceFound false and a line saying UNKNOWN in the export check block. That test fails today because neither member exists.
- evidence: RevitWorksets.cs:131-134 `if (stream == null) { return known; }` with known and decided already set to empty lists at 122-123, and 159-163 `catch (Exception) { known = new List<string>(); decided = new List<string[]>(); }` with no comment, no flag and no line. RevitCategories.cs sets `resourceFound = false` at 146 and 173, exposes `public static bool ResourceFound` at 94 and writes "Revit categories known: UNKNOWN, the category list could not be read out of Federator.Core.dll" at 112. The two callers in src: WorksetDisagreements.cs:171 `if (RevitWorksets.DecidedDifferent(names[i], names[j]))`, which is false for every pair once decided is empty, so AR-EXTERIOR against AR-INTERIOR and ST-SUB against ST-SUP come back as "ONE OR TWO LETTERS APART, so this may be a typo" (line 54), and ExportCheck.cs:186 then writes no "NOT listed, because a person has already decided" sentence because DecidedInLastRead is 0. EmptySets.cs:257 `return RevitWorksets.All();` gives an empty list, and Why at 123-126 skips it, so a workset condition becomes "THIS READER CANNOT TELL WHY" with no reason given. Against the description: a BUILD that lost the resource is not quiet. Federator.Core.csproj:28 `<EmbeddedResource Include="Exchange\revit-worksets.txt" />` is the only thing that puts it there, and dropping it fails ExportCheckTests.cs:198 `Assert.That(Federator.Core.Exchange.RevitWorksets.DecidedCount, Is.EqualTo(2));` and APairAPersonHasDecidedAboutIsCountedAndNotNamed at 162-176. Both run in .github/workflows/tests.yml:28 `dotnet test tests/Federator.Core.Tests/...` and in the pre-commit hook.

## T1-S45, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Findings/ScanFindings.cs:243, :302 and :325 read group.Building, which is the grouping key built at src/Federator.Core/Grouping/BuildingGrouping.cs:118-121. src/Federator.Addin/Ui/FederatorWindow.xaml.cs:313 runs it in every mode.
- proof: Core test
- why: Under PerBuildingAndDiscipline a mistyped code such as 1C7BC with AR and ME gives keys 1C7BC-AR and 1C7BC-ME. Both have shape 9A9AA-AA, so the bucket holds 2 and :279 skips it. The same code is flagged under PerBuilding. A confusable pair such as 1B06PK and 1806PK differs at one position in each discipline's key, so NEAR MATCH is written once per shared discipline. Under PerDiscipline the keys are discipline codes, and :302 and :336 call them building codes and buildings.
- evidence: BuildingGrouping.cs:119 `return name.Building + settings.Separator + name.Discipline` and :121 `return name.Discipline`. ScanFindings.cs:243 `string shape = Shape(group.Building)`, :279 `if (holders.Count != 1)` then continue, :302 `"The building code " + odd.Building`, :325 `DescribeConfusion(left.Building, right.Building)`. FederatorWindow.xaml.cs:282 `BuildingGrouping.Group(ticked, mode, settings)` and :313 `findings = ScanFindings.From(result)` with no check on the mode. ScanFindingsTests.cs:30 only groups through GroupNames, which is the default mode, so no test covers the other modes. Also worth knowing: under both modes every group holds one discipline, so :357-370 writes a SINGLE DISCIPLINE finding for every group, saying the other disciplines are in 'other buildings'.

## T1-S46, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Findings/ScanFindings.cs:366-368. The same stale claim is at src/Federator.Core/Grouping/BuildingGroup.cs:55-57, and also in the window row at src/Federator.Addin/Ui/GroupRow.cs:257 and its comment at :155.
- proof: Core test
- why: In a one discipline group, a test whose other side names a discipline that is not there finds nothing, so CreationPlan never creates it. Only tests whose two sides both find something are created. The sentence in the findings panel and in the log, and the group row status, tell the person the NWF holds every test, and it does not. A Core test can assert that the SINGLE DISCIPLINE detail no longer makes that claim. GroupRow.Status is add-in text and needs review.
- evidence: ScanFindings.cs:366-368 'The federation is still built and every clash test is still created, and none of them is run'. ClashRunner.cs:1708 `CreationPlan creation = CreationPlan.For(absent, itemsByLocator)` leaves out every test with a side that finds nothing, before creation and before the single discipline check at :692. ClashRunner.cs:694 'Created because both its sides find something, F77'. ClashRunner.cs:1690-1693: with no XML, the tests come from the document and nothing is created. BuildingGroup.cs:56 'clash step still creates every test so the NWF matches the others'. GroupRow.cs:257 'Ready. One discipline, so every test is created and none is run.' core.md: 'SINCE F77 THEY ARE NOT ALL CREATED EITHER'.

## T1-S47, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Findings/SourceMismatchFindings.cs:240, fed with GroupBuilding = the group key from src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1723 through src/Federator.Addin/Engine/FederationEngine.cs:1193 and :1216
- proof: Core test
- why: Under PerBuildingAndDiscipline, 1C07BC AR and 1C07BC ME become groups 1C07BC-AR and 1C07BC-ME. Both Revit sources parse to building 1C07BC, so that source maps to two groups and SHARED SOURCE is reported. This happens for every building with more than one discipline. Under PerDiscipline the groups are AR and ME, and the same thing happens. The finding blames a file name for what is only the chosen split. As a side effect, under PerDiscipline the SOURCE MISMATCH detail at :209-210 also says the federation 'will be called' the building code, which is wrong.
- evidence: FederatorWindow.xaml.cs:1722-1723 `jobs.Add(new FederationJob(group.Building, ...`, where the GroupRow's Building is BuildingGroup.Building (window :292-293). FederationEngine.cs:1193 `RecordSource(building, cached, source)` and :1216 `sourcePairs.Add(new SourcePair(building, nwcPath, sourceName))`. SourceMismatchFindings.cs:240-242 `if (!groups.Contains(one.GroupBuilding)) groups.Add(one.GroupBuilding)`. :250 then reports any source with 2 or more groups, and :267 says 'That usually means one of the NWC file names is wrong'. The window calls it at :2015 in every mode.

## T1-S48, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Health/ExportCheck.cs:66, rounding before the test at :136, and again at src/Federator.Addin/Engine/FederationEngine.cs:2152
- proof: Core test
- why: 1051 of 1052 is 99.9, which rounds to 100, so :136 is false. There is no re-export line, the 1 element with an empty id cell is never counted, and the closing sentence says every element carries an id. Any model at 99.5 per cent or above reads as whole. On a 10,000 element model, up to 50 blank id cells pass as none.
- evidence: ExportCheck.cs:66 `return (int)Math.Round(100.0 * WithElementId / Elements, MidpointRounding.AwayFromZero)`. :136 `if (model.IdShare != ModelExport.NotCounted && model.IdShare < 100)`. :242 'all ' + models + ' model(s) carry a workset on every element and an element id on every element'. FederationEngine.cs:2152 has the same test feeding modelsMissingAnId. ExportCheckTests.cs covers 100 per cent and 76 per cent, and nothing between 99.5 and 100.

## T1-S49, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Health/ExportCheck.cs:127
- proof: Core test
- why: A model whose walk threw prints 'elements UNKNOWN   worksets NONE   element id UNKNOWN'. NONE states a count nobody took. Because the partly gathered names are kept, :143 and :230 can then list those same names under 'worksets seen', so the one block says NONE and names worksets for the same model.
- evidence: ExportCheck.cs:13 'A count that could not be taken. Never zero, because zero reads as a real count.' :53 `get { return WithWorkset > 0; }`. :127 `+ "   worksets " + (model.CarriesAWorkset ? model.Worksets.Count.ToString(...) : "NONE")`. ModelFactsReader.cs:209 returns all three counts as NotCounted when the walk throws, and still passes the partly gathered `worksets` list. ExportCheckTests.cs:103-115 asserts 'elements UNKNOWN' and 'element id UNKNOWN' and never checks the worksets cell.

## T1-S50, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Health/ExportCheck.cs:242, reached through the tests at :130 and :136
- proof: Core test
- why: A model where 1 of 1000 elements carries a workset passes :130 and adds nothing. A model whose walk threw fails `Elements > 0` and has IdShare NotCounted, so neither counter moves. A model with 0 Revit elements behaves the same way. In each case the block ends by saying all N models carry both on every element, and no number in the block supports 'every element' for worksets.
- evidence: :53 `WithWorkset > 0` is the only workset test. :130 `if (!model.CarriesAWorkset && model.Elements > 0)`. :136 `if (model.IdShare != ModelExport.NotCounted && model.IdShare < 100)`. :240-242 `if (withoutAnyWorkset == 0 && withoutEveryId == 0) return "all " + models + " model(s) carry a workset on every element and an element id on every element"`. WithWorkset is never printed in the block. ModelFactsReader.cs:209 gives Elements = -1 and WithElementId = -1 when the walk throws.

## T1-S51, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Health/InvisibleDifference.cs:158, with the fallback text at :91
- proof: Core test
- why: 'EL-Fire alarm' against 'EL-Firealarm' have the same EL prefix and are one edit apart, so WorksetDisagreements.In pairs them. The lengths differ, and with spaces removed both read EL-Firealarm, so Between returns the fallback. The line then calls a missing space that anyone can see identical. For a double, leading or trailing ordinary space, the same path names neither the character nor where it sits.
- evidence: :158 `if (!IsInvisible(one) && one != ' ')` removes ordinary spaces as well as invisible ones. IsInvisible at :100-105 does not include ' ', so FirstInvisibleIn at :131-150 returns null when the only difference is ordinary spaces. :91 `return extra ?? "they differ only in invisible characters"`. WorksetDisagreements.cs:51 `"   THEY LOOK IDENTICAL AND ARE NOT: " + invisible`. The class doc at :7-8 promises 'NAMED BY THE CHARACTER THAT DIFFERS AND WHERE IT SITS', and :21-23 lists the double, trailing and leading space. InvisibleDifferenceTests.cs:53 and :59 only assert Is.Not.Null.

## T1-S52, PARTLY, silent wrong output

- how: a third reader settled it, reader A PARTLY, reader B CONFIRMED
- cause: src/Federator.Core/Health/SetWarnings.cs:221-238, the loop in FindCategoriesNobodyHas, never reads condition.Flags, so a negated condition is reported as the value the set asks for. The class doc at SetWarnings.cs:47-48 and the comment at HealthCheckResult.cs:204-205 then describe every such set as one that cannot match anything.
- proof: Core test
- why: It is real, but it differs from the description in two ways. First, the description says a negated condition on an unknown category matches nearly everything. That is not what was measured. scan.md:3857-3859 says a negation on its own selects the four model roots and nothing a person would call an item, and MatrixCorrections.cs:97-100 records the same. The case in the file is a negation beside a positive, which excludes nothing, so the set finds what contains Devices finds. What a clash test does with a set holding only model roots is UNKNOWN. Second, the OR half is latent in the committed file, and for an OR set the printed line is literally true of one group. There only the class doc and the comment claiming the set can never match are wrong, because the other groups can still match. The negation half is live, and reader A was wrong to say no false line is printed today. Telephone Devices is not in the measured list, so the pinned 14 holds one false entry and should read 13. The fix proof is a Core test that fails today: a set with 'contains Devices' and a flags="32" 'equals Telephone Devices' is not reported, and the count in SetWarningsTests.cs:344 moves from 14 to 13. A second test should cover a set of two flags="64" groups where only one names an unknown category, and assert whichever wording the rule settles on, so the set is not described as one that can never match.
- evidence: SetWarnings.cs:231-238 'string asked = condition.Value == null ? string.Empty : condition.Value.Data;' then 'if (asked.Length == 0 || Known(condition.Test, asked)) continue;' then 'found.Add(new CategoryNobodyHas(set, asked));'. Nothing in the loop reads Flags. SetWarnings.cs:65 prints 'Set.Name + " asks for \"" + Category + "\""'. SetWarnings.cs:47-48 says 'It can never match anything, so every clash test that names it can never find a clash.' This is live on the committed file. exchange/1104-PAR_CLASH_AllInOne_25mm_FIXED.xml:28823-28902 holds BLD-EL-Devices as 'contains Devices' flags="0" and six 'equals' flags="32" conditions, the last at :28892-28900 on 'Telephone Devices'. revit-categories.txt holds Communication Devices :45, Data Devices :51, Fire Alarm Devices :63, Lighting Devices :81 and Security Devices :390, and no Telephone Devices line. A grep of the file for the unknown values finds exactly 14 hits, and one of them is the negated condition at :28900. SetWarningsTests.cs:344 pins 'Sets asking for a category no model carries: 14', so BLD-EL-Devices is counted as asking for Telephone Devices while it only excludes it. The log carries this through FederatorWindow.xaml.cs:2163 'log.Block("HEALTH " + ...health.Summary())'. The scan note docs/history/scan.md:3866 measured that a negated equals on a value with no items, beside contains Devices, excludes nothing: 'equals [Nurse Call Devices] on its own 0, and contains Devices NOT equals it 67'. The five flags="64" conditions at :27932, :28099, :28268, :28466 and :28635 all name categories in the list, so the OR case prints nothing wrong today.

## T1-S53, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Health/SetWarnings.cs:119-121, with the behaviour at :181-188 and src/Federator.Core/Exchange/ExchangeModel.cs:184-195
- proof: Core test
- why: Two sets holding the same four conditions, one with flags 64 on the third and one without, produce the same signature and are reported as identical. The first is an OR of two groups and the second is one AND that nothing can satisfy. The comment says the opposite of what the code does. The same loss applies to flags 32: a set asking Category equals X and one asking Category not equal X would also sign alike. Neither committed matrix carries such a pair today, so the named pairs on the real files are right.
- evidence: SetWarnings.cs:119-121 'THE SIGNATURE LEAVES THE FLAGS OUT ... two sets that differ only in how their conditions are grouped ask a different question and are not identical'. :183 `parts.Add(condition.RuleSignature)`. ExchangeModel.cs:181-182 'Flags is left out on purpose', and :189-193 append only the test, category, property and value. HealthCheckResult.cs:208 'Sets asking exactly the same question: '. No SetWarningsTests case varies flags. The test helper at SetWarningsTests.cs:34 always writes flags="0".

## T1-S54, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Naming/OutputNameTable.cs:209-211 and :206
- proof: Core test
- why: core.md makes per cell refill the rule, so the refill is right and the message is wrong. A row with only its NWF name typed has its NWD and workbook names rebuilt, yet the window says the row was left alone. With one row and nothing kept, it says '1 name refilled.' when three names were refilled, while every other branch counts rows.
- evidence: :178-181 `if (row.WasEdited) { kept++ }` then :183-191 still `row.Fill(kind, Build(...))` for every cell that is not by hand. :209-211 '... rows refilled. ' + kept + ' rows were typed over by hand and left alone.' :206 `rowCount + (rowCount == 1 ? " name refilled." : " rows refilled.")`. It reaches the window through FederatorWindow.xaml.cs:580 and :638-640. OutputNameTableTests.cs:130-136 types one cell in each of two rows and asserts '2 rows were typed over by hand and left alone', which pins the wrong wording.

## T1-S55, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Probe/ProbeVerdict.cs:134 against src/Federator.Core/Probe/ProbeTally.cs:84-88, fed by src/Federator.Addin/Engine/PropertyProbe.cs:149-154
- proof: Core test
- why: An element whose category reads 'Pipe Fittings ' or 'pipe fittings' is accepted and walked. Its rows go to the CSV and its category shows among the categories found. ElementsIn("Pipe Fittings") then returns 0, so the same block lists Pipe Fittings under 'categories with no elements at all'. UNKNOWN whether the client's models carry such a spelling. The mismatch is in Core and a test can prove it.
- evidence: PropertyProbe.cs:149 `if (category.Length == 0 || !settings.Asks(category))`. ProbeSettings.cs:85-87 `return PenetrationSettings.Names(Categories, category)`, which does `category.Trim()` and compares OrdinalIgnoreCase at PenetrationSettings.cs:219-228. PropertyProbe.cs:154 `tally.AddElement(category)` records the raw value, and ClashHarvest.FirstProperty and Text do not trim. ProbeTally.cs:42 uses an Ordinal dictionary and :84-88 `ElementsIn` looks up the exact string. ProbeVerdict.cs:134 `if (tally.ElementsIn(category) == 0) missing.Add(category)` with the configured names from PropertyProbe.cs:126. PenetrationSettings.cs:207 gives 'Pipe Fittings ' as the example. No PropertyProbeTests case uses a trailing space or a different case.

## T1-S56, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Report/ClashReportModel.cs:726 and :736, fed by src/Federator.Addin/Engine/ClashHarvest.cs:322 and :334-335
- proof: Core test
- why: Real as described. An item whose id came from the GUID fallback has a filled cell and an empty IdFrom, and all three readers key on IdFrom or the Element ID label, so the ITEM IDS block over counts missing ids, names the wrong label, and the report checks raise a false wrong shape warning when such a cell is first. A Core test can build a ClashItem with ElementId set, IdLabel Instance GUID and IdFrom empty and assert it is not counted missing and not described as written as Element ID.
- evidence: ClashHarvest.cs:310 `into.ElementId = FirstProperty(lookIn, ElementIdNames, out idFrom)` leaves idFrom empty when nothing matched (FirstProperty sets `matched = string.Empty` at :508). Then :322 `into.IdFrom = idFrom` and :334-335 `into.ElementId = guid` / `into.IdLabel = "Instance GUID"`. ClientFormat.ItemId writes the cell as "Instance GUID: <guid>". ClashReportModel.cs:726 `return item != null && string.IsNullOrEmpty(item.IdFrom) ? 1 : 0` counts that filled cell as missing in MissingIdLines. :736 `string from = string.IsNullOrEmpty(item.IdFrom) ? NoIdProperty : item.IdFrom` and :643-645 then log it as `no id property supplied ... written as "Element ID"` while the cell says Instance GUID. ClientShapes.cs:29-30 builds `"^" + ClientFormat.DefaultIdLabel + @": \S"`, so WorkbookCheck.cs:516 and PageCheck.cs:536 call a first Item ID cell of that kind the wrong shape. The only caller of IdSourceLines is FederationEngine.cs:2678, the ITEM IDS block. ClashReportTests.cs covers IdFrom present and absent but never an item carrying a GUID with an empty IdFrom.

## T1-S57, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Report/ReportOrder.cs:272-288, with the wrong log words at src/Federator.Addin/Engine/FederationEngine.cs:2754-2759
- proof: Core test
- why: Real as described. The two pass rename has no undo, so a failure part way leaves pictures under holding names with links pointing at files that are not there, and the log line describes a disk state that does not exist. A Core test can force a move to fail, for example a folder sitting at one target path, and assert that no .moving file is left and every row still points at a file on disk, or that the outcome names what was left.
- evidence: ReportOrder.cs:272-275 `foreach (Move move in moves) { File.Move(move.From, move.From + Holding) }` with Holding = ".moving" (:222), then :278-288 moves each to its final name and only then calls `Point(move.Number, workbookPath)`. Nothing catches or rolls back inside Apply. The one caller, FederationEngine.RenumberThePictures, catches and logs `"kept going, the pictures that were not yet renamed keep their run order numbers"`. If pass two throws at move k, moves k onwards sit on disk as <old name>.moving while their rows still carry the old ImageFile, ImageLink and ImagePath, because Point never ran for them. A throw in pass one leaves the moves before it at .moving too. The workbook and page are written afterwards from those rows. ReportOrderTests.cs covers swap, missing file and no holding file left on success, and has no test where a move throws.

## T1-S58, PARTLY, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Report/ReportPaths.cs:128-131, catching the throw at :178-183
- proof: Core test
- why: The label under the Outputs step does give the wrong reason, it says the NWF folder cannot be read when the real reason is that it sits inside the scanned folder. The log half of the finding is wrong: RunLog.cs:141 is only a comment, nothing logs WhereTheyGo, and the run itself stops with the correct reason in the log and the dialog. A Core test calling WhereTheyGo with the NWF folder under the source folder can assert the words name the scanned folder, and do not carry Parameter name.
- evidence: ReportPaths.cs:178 `throw new ArgumentException("The NWF folder " + ... + " is inside the folder being scanned, ...", "nwfFolder")` and WhereTheyGo :128-131 `catch (Exception) { return "not worked out yet, the NWF folder on this step cannot be read" }`. The only caller is FederatorWindow.xaml.cs:1641 WorkbookFolderOrWhyNot, read into OutputsSummary.Text at :1673, a window label. RunLog.cs:141 is a doc comment, `the way ReportPaths.WhereTheyGo is`, and writes nothing. At run time FederatorWindow.xaml.cs:1993 `options.ChooseFor(nwfFolder)` reaches the same throw, and the catch at :2061-2065 logs it through log.Failure("the run", ...) and shows the real message in a dialog. WindowWordingTests.cs has no case where the NWF folder is inside the source folder.

## T1-S59, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Report/WorkbookWriter.cs:279 with :229-230, painting through src/Federator.Core/Report/ClientStyle.cs:120-123 and :151-156
- proof: Core test
- why: Real as described. The one row empty test borrows the two row header painter, so the row under it is filled and boxed, which is harmless in the middle of the sheet because the next block overwrites it, and leaves a stray grey boxed row after the final block, which in the default order is an empty test on almost every workbook. The empty row's name cells carry no bottom edge, so at the end they read as merged into the stray row. Cosmetic in the client's deliverable. A Core test writing a report whose last test is empty can read the saved file back and assert the row below carries no fill and no border.
- evidence: WorkbookWriter.cs:229-230 `WriteEmptyTestRow(sheet, start, test, priority)` then `return start + 1`. :279 `ClientStyle.TestHeader(sheet, row, ColumnTestHeader - 1, LastTestHeaderColumn)`. ClientStyle.cs:120-121 fills `top` and `top + 1` grey, :123 `for (int row = top; row <= top + 1; row++)` borders both, the second row with a Thick bottom at :163. :154-156 gives the name cells of the top row `BottomBorder = inTheName ? XLBorderStyleValues.None : ...`. A next block starting on start + 1 repaints that row, so only the last block leaves the grey boxed empty row, and the measured order (ClashReportModel.InReportOrder, most clashes first) puts empty tests last. OneSheetWorkbookTests.AnEmptyTestIsOneRowCarryingTheFiveFacts only checks that row + 1 has no Clash Name heading, and no test reads the fill or border of that row.

## T1-S60, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Report/WorkbookWriter.cs:260 overwriting :594
- proof: Core test
- why: Real as described. The 72 point height set for a pasted thumbnail is always replaced by 60 points once WriteClashRow returns, so a 95 pixel picture sits over a row of about 80 pixels and overhangs the next one. It only happens with the thumbnail box ticked, which is off by default. A Core test with EmbedThumbnail on can read the saved row height back.
- evidence: WriteImageCell :590-594 `sheet.AddPicture(clash.ImagePath).MoveTo(cell).WithSize(ThumbnailPixels, ThumbnailPixels)` then `sheet.Row(cell.Address.RowNumber).Height = ThumbnailPoints` with ThumbnailPixels = 95 (:604) and ThumbnailPoints = 72.0 (:606). WriteBlock calls WriteClashRow at :247 and then :260 `sheet.Row(row).Height = ClashRowHeight` with ClashRowHeight = 60.0 (:189). EmbedThumbnail defaults to false (ImageOptions.cs:37). MatchClientReportTests.TickingThumbnailsOnDoesPasteThem counts pictures only and never reads the row height.

## T1-S61, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Rerun/RebuildTally.cs:65 and :75, with Before never defaulted to minus one at :20-26
- proof: Core test
- why: Real as described. A viewpoint count that failed before the clear goes into Before as -1, Kept reads it as nothing to keep, WasCounted ignores Before, so EverythingKept is true, the log says the NWF held none, and the clear and rebuild path saves the NWF over without knowing whether viewpoints came back. That is exactly what the rule forbids. A Core test with Before -1 and both after counts at 0 must see WasCounted or EverythingKept false and a NOT COUNTED line.
- evidence: RebuildTally.cs:65 `get { return Before <= 0 || AfterRestore >= Before }` and :75 `get { return AfterAppends >= 0 && AfterRestore >= 0 }`. :91-94 `if (Before <= 0) { return head + Name + ": none, the NWF held none before the clear" }`. Constructor :20-26 sets AfterAppends and AfterRestore to -1 and leaves Before at 0. SavedViewpoints.cs:88-93 returns -1 when the count throws. FederationEngine.cs:1721 `views.Before = SavedViewpoints.Count(document)` and :1811-1812 fill the after counts, then :1833 `if (!tally.EverythingKept)` is the only thing between the tally and `WriteNwf(document, job, outcome)` at :1840. The reshape path does the same at :1398 and :1512. addin.md: `a count that could not be taken reports minus one and holds the NWF shut`. RebuildTallyTests.cs has no case with Before at -1.

## T1-S62, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Sets/EmptySets.cs:100 and :103, judged through KnownFor at :248-258
- proof: Core test
- why: All three parts hold. The internal names are written into Core against the rule, the category one is a second copy of HealthCheck's, and the lists are measured off this project's models and built into the DLL, so on a project with other worksets or categories a set asking for a value that project's models do carry is told no model carries it. On this project today the lists are right, so the harm is latent and becomes a false log line the day another project's XML is picked. The two names are standard Revit exporter names rather than this project's, which makes the rule breach a letter one, the lists are the real fault. A Core test can assert a value absent from the embedded lists is judged CannotTell unless the lists are known to belong to the run's project.
- evidence: EmptySets.cs:100 `public const string CategoryProperty = "LcRevitPropertyElementCategory"` and :103 `public const string WorksetProperty = "lcldrevit_parameter_-1002053"`. HealthCheck.cs:129 `public const string CategoryPropertyInternalName = "LcRevitPropertyElementCategory"`, whose own comment says it is `named here once so the warning and the reader cannot drift`. KnownFor :252 `return RevitCategories.Measured ? RevitCategories.All() : null` and :257 `return RevitWorksets.All()`. RevitWorksets.cs:9 `Every workset name a Revit model in this project really carries`, shipped as an embedded resource (:21-22, :27). EmptySet.Line :51 writes `NO MODEL IN THIS PROJECT CARRIES IT`. The caller is SetBuilder.cs:703, printed in the EMPTY SETS block at FederationEngine.cs:2626-2635. CLAUDE.md: property internal names from the clash XML appear in tests as sample data only.

## T1-S63, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Addin/Engine/SetBuilder.cs:701-704, reached only inside the already there branch, with the created path at :756-758 never calling AddEmpty
- proof: both
- why: Real as described. Only a set already in the NWF is ever judged, so a set this run created at zero is listed as a ZERO line and counted in the totals but never gets the which of three things is wrong judgement, and on a first run, when every set is created, the EMPTY SETS block is not written at all. If the judgement moves into SetBuildOutcome a Core test can prove a created set at zero lands in Empty, and a first run on the local machine shows the block.
- evidence: SetBuilder.cs:638 `if (existing != null)` opens the present branch, :701-704 `if (found == 0 && !drift.CouldNotRead) { outcome.AddEmpty(EmptySets.Why(planned.Path, asking)) }`, and :720 `return`. The created path :756-757 `outcome.AddCreated(planned.Path, planned.Name, planned.ConditionCount, items, planned.Describe())` with no AddEmpty. AddEmpty has one caller in src, SetBuilder.cs:703. FederationEngine.cs:2626 `if (outcome.Sets != null && outcome.Sets.Empty.Count > 0)` gates the EMPTY SETS block. SetBuildOutcome.cs:74-75 describes Empty as `Every set that found NOTHING`.

## T1-S64, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Sets/SetBuildOutcome.cs:205-208 and src/Federator.Addin/Engine/FederationEngine.cs:2456
- proof: Core test
- why: Real as described. A rebuild changes the document and the log says REBUILT, yet the sets step does not ask for the save. In the usual weekly run tests run and the save happens anyway, so the gap shows only where nothing is created or run and no status or viewpoint is written, such as a one discipline group whose tests all exist, or a file holding sets only. There the NWD is published from a document with the rebuilt sets while the NWF on disk keeps the old ones. A Core test with a drift added as rebuilt and nothing created can assert PutAnythingIn is true.
- evidence: SetBuildOutcome.cs:205-208 `public bool PutAnythingIn { get { return CreatedCount > 0 } }`, while RebuiltCount (:138-141) is kept apart. FederationEngine.cs:2456 `return sets.PutAnythingIn || sets.ActedOnLeftovers > 0`. SetBuilder.cs:648-651 rebuilds through Rebuild, which calls `sets.ReplaceWithCopy(parent, at, made)` at :155. ClashStep :2225-2231 returns `CreateAndRunTheTests(...) || changed`, and FinishTheGroup :959 saves the NWF only on `clashPutSomethingIn || viewsPutSomethingIn`. CreateAndRunTheTests returns false at :2485 for a file with no tests and otherwise `clash.CreatedCount > 0 || clash.RanCount > 0 || runner.ChangedAStatus` at :2725.

## T1-S65, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Sets/SetBuildOutcome.cs:370-373
- proof: Core test
- why: Real as described. A present set whose search could not be read, or that holds no search at all, is neither drifted nor counted anywhere, so the sentence says every set matches the file when some were never compared. Its reach is the window text after Build sets by hand, not the run log. A Core test needs the outcome to learn of an unread set and then assert the line no longer claims every set matches.
- evidence: SetBuildOutcome.cs:370-372 `if (Drifted.Count == 0) { lines.Add("      none of them drifted. Every set in the document asks what the file asks") }`. SetDrift.cs:92-93 Drifted is `False where it could not be read`, and :103 marks CouldNotRead when asked is null. SetBuilder.cs:51-68 sets asked null when `existing.HasSearch` is false or the read throws. SetBuilder.cs:706 `if (drift.Drifted) { outcome.AddDrift(drift, rebuilt) }` is the only route into Drifted, and the outcome records no count of unread sets. SetBuildOutcome.Lines() reaches the window only, through ShowSetLines at FederatorWindow.xaml.cs:2408 after the Build sets button, since F81 removed the per group SETS block from the log.

## T1-S66, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Sets/SetBuildOutcome.cs:313-319 and :386-399, counting through :196, :211-214, :217-233 and :258-274
- proof: Core test
- why: Real as described. The counts are deliberately of created sets, but nothing in the words says so, so a weekly run where every set is already there logs 0 finding items and 0 at zero while present sets found thousands and 38 found nothing. The items found line reads 0 in the window after Build sets. The counts can stay, the labels are the fault, and a Core test can assert an all present outcome no longer reads as zero sets finding items.
- evidence: FindingItemsCount :213 `get { return Count(true, true) }`, ZeroCount counts `result.IsZero`, which SetResult.cs:66 defines as `Created && ItemCount == 0`, TotalItems :266 `if (result.Created)`. Summary :313-316 writes `CreatedCount + " created, " + AlreadyPresentCount + " already there, " + FindingItemsCount + " finding items, " + ZeroCount + " at zero"`. Lines :386-387 and :399 write `sets finding items: `, `sets at zero      : ` and `items found       : `. Summary is the SETS step finish phrase in the log on every run (FederationEngine.cs:2389) and the window after Build sets (FederatorWindow.xaml.cs:2409-2410). Lines reaches the window only (:2408). SetBuildOutcomeTests.cs:313-314 pins the counting on purpose, `only the created set counts as finding items`, and no test reads the words.

## T1-S67, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Sets/SetDrift.cs:44-47, with src/Federator.Addin/Engine/SetBuilder.cs:83-99 and src/Federator.Core/Sets/SetLeftovers.cs:207-223
- proof: both
- why: Two sets with the same conditions in the same order but different grouping, A and B and C and D against (A and B) or (C and D), give identical key lists. Compare calls them not drifted and SameQuestion calls them twins. So a set whose OR grouping differs from the file is never rebuilt and never reported. With the box on, a leftover can be renamed onto the file's name while asking a differently grouped question, and every later run reports it as matching. Whether any NWF holds such a set is UNKNOWN. The client's five OR sets carry flags 64 in the file and this tool passes the flags through, so a set this tool built matches. Core test: SetDrift.Compare on four conditions where only the file's third carries StartGroup asserts Drifted, and SetLeftovers.For asserts two sets differing only by that bit are not twins. Both fail today. A run is also needed, because what SearchCondition.Options returns on a set saved in an NWF is UNKNOWN.
- evidence: SetDrift.cs:46 `return CategoryInternalName + "|" + PropertyInternalName + "|" + Test + "|" + Value` (no flags). SetBuilder.cs:85-89 Read builds ReadCondition from CategoryCombinedName, PropertyCombinedName, Comparison and Value only, never condition.Options. SetBuilder.cs:95-98 KeyOf joins category, property, test and value, never condition.Flags. SetBuilder.cs:1029-1030 passes `(SearchConditionOptions)condition.Flags` when BUILDING, so the bit goes in but is never read back. SetLeftovers.cs:207-216 SameQuestion compares the same flagless keys, while its doc at 110-112 says the pair was measured as 'same flags'. SetDrift.cs:97-99 gives StartGroup as the reason order matters, yet the bit is not compared. docs/history/scan.md:2204 records that SearchCondition has an Options property, so the bit can be read. SetDriftTests and SetLeftoversTests have no case that differs only by the group bit.

## T1-S68, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Sets/SetDrift.cs:148 and 161, fed by src/Federator.Addin/Engine/SetBuilder.cs:73-77, carried by SetBuilder.cs:663 and 693 into src/Federator.Core/Sets/SetsAcrossTheRun.cs:198
- proof: both
- why: The real 5w case is exactly an OR set, BLD-ME-Ducts&Duct Fittings asking ME-DUCTWORK where the file asks ME-Ductwork. Its SET DRIFT lines print both sides as one four way AND chain, the question no element can answer that F78 fixed in PlannedSet.Describe. On a weekly run every set is present, so SETS ACROSS THE RUN prints the same AND chain for an OR set that found nothing anywhere. The same set reads correctly when created and wrongly when already there. AskedNow cannot group without the bit T1-S67 finds missing from ReadCondition. WantedNow could group today if the add-in passed the planned set. Core test: SetDrift fed a four condition set with the group bit on the third asserts AskedNow and WantedNow both read as two bracketed groups joined by or. It fails today. A run then shows the line on a real OR set.
- evidence: SetDrift.cs:148 `return string.Join(" and ", parts.ToArray())` and 161 `return string.Join(" and ", array)`. SetBuilder.cs:76 `described.Add(condition.Describe())` hands one string per condition, so the file's grouping is lost before WantedNow sees it. SetBuilder.cs:663 `string askedNow = drift.AskedNow()` and 693 `present.Asked = askedNow`. SetsAcrossTheRun.cs:198 `: "asks " + set.Asked`. By contrast PlannedSet.Describe (SetBuildPlan.cs:181-206) brackets the groups and joins them with " or ", and a CREATED set gets it at SetBuilder.cs:757 `planned.Describe()`.

## T1-S69, CONFIRMED, noise

- how: two readers agree
- cause: src/Federator.Core/Sets/SetRebuildSettings.cs:39-40
- proof: Core test
- why: The grey line under the box says only drifted sets are touched, but ticking it also removes every set the file does not name that nothing points at, and renames others. That is a change to the NWF the window text leaves out. The confirm screen names it before the run, so a person is told once. That is why this is noise and not a silent wrong output. The only test on HelpLine counts words (SetDriftTests.TheLabelIsAtMostEightWordsAndTheHelpLineAtMostTwelve). Core test: assert HelpLine names the removal and stays at twelve words or fewer. It fails today.
- evidence: HelpLine `"Only sets whose question changed. Results and statuses are kept, measured"`, shown under the box at FederatorWindow.xaml.cs:1189 `RebuildDriftedSetsHelp.Text = SetRebuildSettings.HelpLine`. SetBuilder.cs:506-509 `if (rebuilds.RebuildDriftedSets) { HandleLeftovers(document, sets, plan, outcome) }`, and CarryOut removes unused leftovers (SetBuilder.cs:574-583) and renames pointed at ones (597-614). ConfirmLine at SetRebuildSettings.cs:65 does say 'an unused one is removed'. Two comments also say the old narrower scope: SetRebuildSettings.cs:20-22 'never a set the file does not name at all, which is a leftover and is reported instead', and FederatorWindow.xaml:542-545 'it destroys nothing'.

## T1-S70, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Core/Views/SizeText.cs:204, with the walk at 50-53
- proof: Core test
- why: A size inside a word is read as its tail, not refused. A 200 mm service reads as 0 mm, which the penetration rule treats as at or under 150 and may move to Reviewed, and the viewpoint rule keeps out of the tree. The design says a size it cannot read is refused so the clash is left for a person. Whether any model writes a size property shaped like DN150 mm is UNKNOWN. The real strings in the tests are "53 mmø" and "600 mmx100 mm". Core test: SizeText.Millimetres("DN150 mm") is empty and LargestMillimetres("DN200 mm") is null. Today they give 50 and 0.
- evidence: SizeText.cs:204 `return at == 0 || !IsLetter(text[at - 1]) || IsSeparator(text[at - 1])`. SizeText.cs:50-53 `if (!IsNumberStart(text, at)) { at++ continue }`. Walking "DN150 mm": the 1 follows N and is refused, the walk moves one character, the 5 follows a digit and starts a number, "50" then " mm" gives 50. "DN200 mm" gives "00", which is 0 mm. The comment at 201-203 says a digit after a letter is 'inside a word already being read', which the code does not do past the first digit. Used at ItemSizes.cs:118 `SizeText.LargestMillimetres(data.ToDisplayString())`, read by Penetrations.cs:328 and by ViewpointBuilder.cs:393 through Penetrations.ServiceSizeOf. SizeTextTests has no case of digits after a letter.

## T1-B1, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Addin/Engine/FederationEngine.cs:2589 with src/Federator.Addin/Engine/ClashImages.cs:66-68, ClashHarvest.cs:382-393 and FederationEngine.cs:2706-2710
- proof: a run
- why: The image failure count starts at zero in every group, so a run where every picture fails the same way but no group reaches fifty in a row never stops. Once the guard fires, every remaining clash of that group is still rendered, and the stop only lands when the group ends. One more gap: the reason 'the render finished but no file arrived at ' + path (ClashImages.cs:184) carries the path, so that failure never repeats with the same reason and never counts past one. All of it is in the add-in, so a run with renders forced to fail across groups of under fifty clashes is the proof.
- evidence: FederationEngine.cs:2589 `runner.Images = new ClashImages(log, reports.Images)`, inside the per group clash step. ClashImages.cs:66-68 `this.guard = this.options.StopAfterFailures > 0 ? new RepeatedFailureGuard(this.options.StopAfterFailures) : null`, a guard per instance. ClashImages.Write (124-208) and ClashHarvest.cs:382-393 never read ShouldStopTheRun. FederationEngine.cs:2706 `if (runner.Images.ShouldStopTheRun && stopTheRun == null)` runs only after `runner.Run(plan)` at 2602 returns. The test guard is handed in for the whole run, ClashRunner.cs:104-109, and stops at once, ClashRunner.cs:585-592. core.md says fifty image failures stop the run 'the same way and with the same default as the clash step'.

## T1-B2, CONFIRMED, noise

- how: two readers agree
- cause: src/Federator.Addin/Engine/SavedViewpoints.cs:59-62, with the comment at src/Federator.Addin/Engine/FederationEngine.cs:3179
- proof: none needed
- why: Nothing reads it, so setting it false would still build viewpoints. It does no harm at run time today, because it returns true and viewpoints build whenever a report exists. It is a public member nothing in src calls, which CLAUDE.md says is deleted, and three places describe a switch that switches nothing. Deleting the member and fixing the three sentences is proved by the build and a grep. No run is needed.
- evidence: SavedViewpoints.cs:59-62 `public static bool CanBuild { get { return true } }`. A grep of src for CanBuild finds only that and FederationEngine.cs:3179 `/// SavedViewpoints.CanBuild is the one switch, true since the viewpoints round.` A grep of tests finds none. BuildViewpoints gates only on `if (outcome.Report == null)` at 3183. Its own doc at SavedViewpoints.cs:54-57 says 'While this was false the engine asked for no viewpoints', and addin.md calls it 'the one switch'.

## T1-B3, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Addin/Ui/FederatorWindow.xaml.cs:2588-2597, with Pump at 2511-2514 and no Closing handler anywhere
- proof: a run
- why: A same thread Invoke below Send priority pushes a nested frame that dispatches window messages, so the title bar X and Alt+F4 reach Close mid run with nothing to cancel them. RunJobs keeps running on the stack. It is standard WPF that a closing dialog re-enables its owner, so the Navisworks main window would come back to life while the run still drives the document. That is not measured on this install, and whether a later Warn(this, ...) on the closed window throws is UNKNOWN. Proof is a run: press the title bar close during a run and expect a refusal like the Close button's.
- evidence: OnClose 2590-2594 `if (running) { Warn("The run is still going. Let it finish.") return }` guards only the Close button, which is wired at FederatorWindow.xaml:61 `Click="OnClose"`. A grep for Closing in src/Federator.Addin finds nothing, and the Window element at FederatorWindow.xaml:1-7 has no Closing attribute. Pump 2513 `Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }))` runs on every SetProgress and Log, and the engine is handed SetProgress at 2004-2005. FederatorPlugin.cs:74 `window.ShowDialog()` with the Navisworks main window as owner.

## T1-B4, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Addin/Ui/GroupRow.cs:111-129, with src/Federator.Core/Naming/OutputNameTable.cs:84-87
- proof: both
- why: core.md says 'A cell can be given back to the pattern' and no control or path does it. Clearing a cell pins an empty name as typed over, so the next pattern change skips it. The only way back is a regroup that throws away every hand edit, and pressing Run first throws out of the click handler. This is the same fault as T1-B7. Q24 in steps/02_questions.md:113 asks Bader to wire it or drop the rule and the member, and it has no answer, which is why the member survives. If it is wired: a Core test that a cell typed empty goes back to the pattern on the next Refill, then a run for the grid. If it is dropped: the build and a grep.
- evidence: GroupRow.cs:118 `string tidied = value == null ? string.Empty : value.Trim()` then 126 `names.SetByHand(kind, tidied)`, for any value including empty. OutputNameTable.cs:84-87 `public void ReleaseToPattern(OutputKind kind) { byHand[(int)kind] = false }` is called only by tests/Federator.Core.Tests/Naming/OutputNameTableTests.cs:163. A grep of src finds no caller. FederatorWindow.xaml.cs:288 `nameTable = OutputNameTable.From(result.Groups, naming, settings)` is the only reset, and the comment above it says hand edits are dropped. Also, an emptied NWF or NWD cell reaches FederatorWindow.xaml.cs:1725 `OutputPaths.Nwf(nwfFolder, group.NwfName)`, which throws 'An output needs a name.' at OutputPaths.cs:38-41, and OnRun has no try around it.

## T1-B5, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:34 and 376
- proof: Core test
- why: Every run writes a .tsv beside its .log in %LOCALAPPDATA%\ParsonsNwcFederator\logs. Retention prunes the .log files back to 30 and never matches a .tsv, so the .tsv files grow without limit on each of the 27 machines. How large they get is UNKNOWN here. Core test: a temp folder holding forty old run logs, each with its .tsv, then Start with keep 30, asserting at most 30 .tsv files remain and the live one is kept. It fails today.
- evidence: RunLog.cs:34 `public const string LogFileSearchPattern = FileNamePrefix + "*" + FileNameExtension`, with FileNameExtension ".log" at 22. RunLog.cs:376 `all = new DirectoryInfo(folder).GetFiles(LogFileSearchPattern)`. RunLog.cs:353 `log.rows = RowLog.StartBeside(path)`, and RowLog.cs:80 `System.IO.Path.GetFileNameWithoutExtension(logPath) + EventRow.FileNameExtension` with ".tsv" at EventRow.cs:33. A grep of src for Delete finds nothing else that touches the logs folder. RunLogRetentionTests only covers .log files.

## T1-B6, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Core/Naming/NamePattern.cs:69, never set by src/Federator.Addin/Ui/FederatorWindow.xaml.cs:535-548
- proof: a run
- why: The format the rules call a person's setting cannot be changed by anyone, so a dated NWD is always yyyyMMdd and the fallbacks in NumberOrDate cannot be reached. The default is sound, so no name comes out wrong. The harm is a promised setting that does not exist. The Core half is already tested. The missing piece is a box on the Outputs step whose value reaches the preview, and only a run shows that.
- evidence: A grep of src for DateFormat finds only NamePattern.cs:33 `DefaultDateFormat = "yyyyMMdd"`, 42 `DateFormat = DefaultDateFormat`, 69 `public string DateFormat { get; set; }` and 139. FederatorWindow.xaml.cs:543-547 sets `pattern.Level`, `pattern.Discipline`, `pattern.TypeCode`, `pattern.Number` and `pattern.AllBuildings` only. The XAML has no box for it. CLAUDE.md lists 'the date format' among the settings, and core.md says 'their mistakes should be visible in the preview'. OutputNameTableTests.TheDateFormatIsASetting sets it from a test only.

## T1-B7, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Core/Naming/OutputNameTable.cs:84-87, with src/Federator.Addin/Ui/GroupRow.cs:118-126
- proof: both
- why: This is the same fault as T1-B4, reported from the Core side. Clearing a cell makes it a typed over empty name instead of giving it back to the pattern, and nothing in the window can release it. It waits on Q24, which is unanswered. If the rule is kept, a Core test that an emptied cell is refilled by the next Refill, then a run for the grid. If it is dropped, the member and the core.md sentence go and the build proves it.
- evidence: OutputNameTable.cs:84 `public void ReleaseToPattern(OutputKind kind)` has no caller in src, only OutputNameTableTests.cs:163. GroupRow.cs:118 `string tidied = value == null ? string.Empty : value.Trim()` and 126 `names.SetByHand(kind, tidied)`, and OutputNameTable.cs:79-80 stores the empty name and sets byHand true. Refill at OutputNameTable.cs:185-188 then skips that cell for good.

## T1-B8, PARTLY, broken feature

- how: two readers agree
- cause: src/Federator.Core/Naming/OutputNameTable.cs:243-246 turns a refused pattern into a name, and OutputNameTable.cs:310-327 WhyTheRunCannotStart checks collisions only. It is the one check OnRun makes, at src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1701
- proof: Core test
- why: The main fault is real. An emptied pattern box turns the sentence into the file name, and with one group ticked that passes the only check. The preview detail is wrong: the preview shows the sentence itself, and the method holding No name yet has no caller. The fix belongs in Core. A test can empty naming.Nwf.Level on a one row table and assert that WhyTheRunCannotStart names the empty field. Today it returns null
- evidence: NamePattern.cs:189-194 throws `new InvalidOperationException(unusable)` when any of the five fields is empty, and OutputNameTable.cs:245 catches it: `return "CANNOT BE NAMED: " + error.Message`. WhyTheRunCannotStart returns null when `collisions.Count == 0`. OnRun then builds the job at FederatorWindow.xaml.cs:1722-1730 with `OutputPaths.Nwf(nwfFolder, group.NwfName)`. OutputPaths.cs:38 only refuses `string.IsNullOrEmpty(outputName)`. FileNames.cs:18 `RefusedPrintable = "<>:\"/\\|?*"` holds the colon, and the only caller of FileNames.CanBeAName in src is NamePattern.cs:158. With several groups ticked, NameCollision.cs:28-31 gives "N groups would be written to the same NWF name, CANNOT BE NAMED: The level is empty, so the name would have a hole in it.. They are ... One would overwrite the other". So the empty field is inside the sentence, but it is worded as a collision. Where it differs from the finding: the preview does NOT say No name yet. RefreshNamePreview at FederatorWindow.xaml.cs:621-623 prints `group.NwfName`, which is the CANNOT BE NAMED sentence. "No name yet" is only in SafeName at FederatorWindow.xaml.cs:324-343, and a grep of src finds no caller of SafeName, so it is dead code. Only the kind whose box was emptied is hit. Emptying the NWF level changes the NWF names alone. OutputNameTableTests.cs has no test of the CANNOT BE NAMED path. What SaveFile does with a colon in the name is UNKNOWN

## T1-B9, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Core/Views/SizeTally.cs:16-129. Nothing in src constructs it, so SizeSettings.NameEveryUnknown and ExamplesWhenNotNamingEvery at SizeSettings.cs:88-91 are read by nothing that runs
- proof: both
- why: The block that names every clash side whose size could not be read is never written, and the two settings that shape it change nothing. It is also a public class with no caller in src, which CLAUDE.md says is deleted with its tests unless a decision keeps it. If the naming moves into ClashViewpointPlanOutcome.Lines, a Core test can feed it SizeUnknown clashes and assert each one is named. A run then shows the block in the log
- evidence: A grep of src for SizeTally finds only SizeTally.cs, and a grep for "SIZE finds only SizeTally.cs:89, 93, 98 and 104. The two settings are read only at SizeTally.cs:111 `bool everyOne = settings.NameEveryUnknown` and at 114. The live VIEWS block only counts: ClashViewpointPlan.cs:267 `"size could not be read : " + SizeUnknownCount`. The only per item rows are the penetration rule's, at FederationEngine.cs:2655-2658 `log.Row("service with no readable size", ...)`. That is F72, which is off by default, and not the F85 viewpoint rule. Only SizeTallyTests.cs constructs SizeTally, 9 times. core.md says "Every one of them is named in the SIZE block". steps/03_bader_next.md step 247 already records that no SIZE block is written on any run

## T1-L1, CONFIRMED, loud failure

- how: a third reader settled it, reader A PARTLY, reader B CONFIRMED
- cause: C:\Users\p003653k\OneDrive - Parsons Corp\Documents\GitHub\PAR_NWC-Federator\src\Federator.Addin\Engine\Penetrations.cs:260 calls Upwards before the try that starts at 262. Line 371 walks Parent with no catch. The summary at 187-188 promises the method never throws.
- proof: a run
- why: Every fact in the finding holds on main at 0eb4ede, at the same line numbers. The finding says the throw escapes to ViewpointBuilder.cs 393, and that is exactly what happens. It does not claim the throw takes down the step or the run, so Reader A's narrower harm does not contradict it. The harm is limited to one test at a time. The log gets one Failure line, and the rest of that test's clashes get no viewpoint, or no penetration decision. There is one more cost Reader A and Reader B did not name. In the penetration pass, the clashes judged before the throw still move to Reviewed, while the Failure line says every clash of the test was left as it was. The fix is add-in code with no Navisworks-free part, so there is no Core test to write. The proof is a reviewed change that puts the walk inside the try, or catches and logs inside Upwards the way ClashHarvest does, and disposes the chain on every path. Then a run whose VIEWS and PENETRATION blocks match the run before it.
- evidence: Penetrations.cs:187-188 "Never throws: a side that will not read is a side with no size, which is SizeUnknown, in and said." Line 260 `IList<ModelItem> lookIn = Upwards(item);` sits outside the `try` at 262. Line 264 `string name = Words.Or(item.DisplayName, string.Empty);` is inside a try at 262-277 that has only a `finally` and no catch. Line 371 `walker = walker.Parent;` is in Upwards, and `chain` is a local at 358. So if Parent throws at the second level or later, the ancestors already in `chain` never reach the finally at 270-277 and are not disposed. That breaks the method's own rule at 244-246, "the item and the chain above it are disposed on the way out". Lines 198-199 also read `result.Item1` and `result.Item2` outside any try. The caller ViewpointBuilder.cs:392-394 `? Penetrations.ServiceSizeOf(result, penetrations, sizes, unitEnumName)` has no try of its own inside CollectResults. ClashHarvest.cs:424-441 guards the same Parent walk with `catch (Exception error) { log.Failure("walking up from a clashing item to find its properties", ...` so the two readers of one tree treat a throw differently. Where the throw ends up: ViewpointBuilder.cs:297-303 catches it once per test and logs "kept going, the clashes read before it threw are planned and the rest are not". Penetrations.cs:89-95 catches it for the penetration pass and logs "kept going, every clash this test holds is left exactly as it was". That second line is not true when the throw lands part way through a test. `wanted` already holds the clashes judged before the throw, WantedFor returns it at 97, and ClashRunner.cs:811-815 applies it through statuses.Apply. No doc under docs\history records Parent throwing, and a grep for `.Parent` there finds nothing, so whether it ever fires on a clash item is UNKNOWN.

## T1-L2, CONFIRMED, loud failure

- how: two readers agree
- cause: src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1508 `options.Tolerance = ChosenTolerance()` inside ReportsWanted, with ToleranceChoice.cs:107-118 throwing
- proof: both
- why: A tolerance box on the Clash step that the undo, the probe and building the sets never use makes all three fail, and the open file run fails inside its try instead of refusing before it starts. Moving the parse into Core as a call that does not throw can be proved by a Core test. The buttons are proved by a run with Other chosen and the box left blank
- evidence: ChosenTolerance at FederatorWindow.xaml.cs:1087-1100 sets `typed = double.NaN` when the Other box does not read, then calls `ToleranceChoice.Of(typed)`. That throws ArgumentOutOfRangeException for NaN, for infinity and for below zero (ToleranceChoice.cs:107-118). ReportsWanted is called by Undo at 1261, Probe at 1361, the open file run at 2243, Build sets by hand at 2405 and Run tests by hand at 2455. Each catch treats the throw as a stop: "The undo stopped.", "The probe stopped.", "Building the sets stopped.", "The clash tests stopped.". The open file run logs Failure("running the open document") at 2283 and then writes RESULT. Only OnRun refuses first, at 1743-1751. The undo, the probe and building the sets never use the tolerance

## T1-L3, CONFIRMED, broken feature

- how: two readers agree
- cause: src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1701 reads only src/Federator.Core/Naming/OutputNameTable.cs:310-327, which checks collisions and nothing else. The name comes from OutputNameTable.cs:245
- proof: Core test
- why: This is the same fault as T1-B8, seen from the check that misses it, and one fix closes both. The name check before a run does not refuse a name the pattern could not build. A Core test on OutputNameTable with an emptied level and one row fails today, because the answer is null
- evidence: FederatorWindow.xaml.cs:1701 `string collisions = nameTable.Only(TickedGroupKeys()).WhyTheRunCannotStart()`, and WhyTheRunCannotStart returns null when `collisions.Count == 0`. OutputNameTable.cs:245 `return "CANNOT BE NAMED: " + error.Message`, and the message comes from NamePattern.cs:90 "The " + labels[i] + " is empty, so the name would have a hole in it." With one group ticked, the job at 1722-1730 carries that sentence as its NWF, its NWD or its workbook name, whichever pattern was emptied. With several ticked, NameCollision.Sentence frames it as "N groups would be written to the same NWF name, CANNOT BE NAMED: ...". So it is worded as a collision, with the empty field sentence inside it as the name. What the engine or SaveFile does with the colon is UNKNOWN

## T1-L4, CONFIRMED, loud failure

- how: two readers agree
- cause: src/Federator.Addin/Ui/FederatorWindow.xaml.cs:1725-1726 calls OutputPaths with no try, after src/Federator.Addin/Ui/GroupRow.cs:118-126 stores an empty typed name
- proof: both
- why: Clearing one NWF or NWD cell and pressing Run throws out of the click handler with nothing to catch it. A Core test can assert that WhyTheRunCannotStart refuses a row whose name is empty. Today it returns null for one such row. A run with one cell cleared then shows the refusal
- evidence: GroupRow.TypeOver: `string tidied = value == null ? string.Empty : value.Trim()` then `names.SetByHand(kind, tidied)`, with no empty check. Collisions only reports a name that more than one row shares (`byName[name].Count > 1`), so one empty name passes. OnRun builds jobs at 1713-1731 with no try, and OutputPaths.cs:38-41 runs `throw new ArgumentException("An output needs a name.", "outputName")`. The window knows about this state. RefreshRunPaths at 681 treats `group.NwfName.Length == 0` as Unknown, and GroupListLines at 1857 prints "no output name". Neither refuses it. An empty workbook name is not passed to OutputPaths at 1728, so it is not caught here either. If the exception leaves ShowDialog, FederatorPlugin.cs:78-80 logs it as "starting the add-in" with "stopped, the window never opened", which would be false. Whether Navisworks handles a dispatcher exception first is UNKNOWN

## T1-L5, CONFIRMED, loud failure

- how: two readers agree
- cause: src/Federator.Core/Clash/AutoReviewRecord.cs:150 calls the constructor, which refuses at 54-60, after StatusFrom at 211-225 has accepted every status
- proof: Core test
- why: The documented contract of In is broken, and one edited comment stops the undo for a whole test. A Core test asserting that In(Marker + " [penetration] [was Approved] why") is null throws today
- evidence: StatusFrom loops over ClashTally.AllStatuses, which holds Reviewed, Approved and Resolved (ClashTally.cs:15-22). In then runs `return new AutoReviewRecord(rule.Value, wasAt, rest.Substring(after).TrimStart())`. The constructor throws ArgumentOutOfRangeException when `!StatusesThisToolMayMoveFrom.Allows(wasAt)`, and that is true for anything but New and Active. The summary at 112-116 says In returns null "where the comment is not one of ours". UndoAutoReviewed.cs:198-205 calls In on every comment. That runs inside Judge, which OneTest calls in the try at 107-118, and the catch logs a Failure and returns, so every clash of that test is left alone. UndoAutoReview.cs:58, MayUndo and WhyNotUndone also call In. AutoReviewRecordTests.cs:110-119 tests an unknown rule and "[was Nonsense]" but never "[was Approved]". This tool cannot write such a comment, so only a comment edited by hand reaches this path

## T1-L6, CONFIRMED, loud failure

- how: two readers agree
- cause: src/Federator.Core/Diagnostics/RunLog.cs:557-581, WriteRaw with no try around the write, the flushes or the handler
- proof: Core test
- why: The text log can stop the run, which is what the rule forbids and what the row log already guards against. A Core test that subscribes a LineWritten handler that throws, then asserts that Line returns and the line is on disk, fails today. The full disk half is UNKNOWN to reproduce in a test
- evidence: Inside `lock (gate)`, `writer.WriteLine(line)`, `writer.Flush()` and `stream.Flush(true)` run with no try, and `handler(line)` at 579 has none either. RowLog.cs:152-156 wraps the same write in a catch, with the comment "A disk that filled up must not stop a run here". core.md says "logging is never the thing that stops a run. That holds for the copy, for retention, and for the log file itself". The fall back to the temp folder and to the window only is at open time, in StartOrDisabled at 263-298. While a disk stays full, every later Line meets the same write, including the log.Failure calls inside the catch blocks that would report it. The one handler is FederatorWindow.xaml.cs:115-119, OnLogLine, which calls LogBox.AppendText. No test in RunLogTests.cs subscribes a handler that throws

## T1-L7, CONFIRMED, silent wrong output

- how: two readers agree
- cause: src/Federator.Addin/Engine/ModelFactsReader.cs:152-155 turns a failed read into an empty site, and src/Federator.Core/Health/AlignmentCheck.cs:255-258 reads an empty site as no site
- proof: both
- why: A read that threw gives a FAILED group with a reason that reads as a fact about the model and is false, and nothing in the log says the read threw. A Core test can give ModelPlacement a site that was not read and assert WhyItFailsTheGroup does not name it as having no site. It fails today because there is no such state. The add-in catch is then proved by review and a run
- evidence: ModelFactsReader.cs:152-155: `catch (Exception) { site = string.Empty }`. It writes no line, so the catch neither logs nor rethrows. ModelPlacement has `NotRead = double.NaN` for X, Y and Z at AlignmentCheck.cs:14, but NamesASharedCoordinate is only `SharedCoordinate.Length > 0` at 55-58. WhyItFailsTheGroup at 255-258 puts that model in withNoSite, and 282 writes "model(s) name no shared site at all". FederationEngine.cs:2090-2097 runs `outcome.AddError(fails)` and counts it in failedOnAlignment. The ModelFactsReader summary at 27-29 says "NOTHING HERE FAILS A GROUP" and "a count that could not be taken comes back as NotCounted and never as zero". Whether RootItem or the Location tab ever throws on a real model is UNKNOWN
