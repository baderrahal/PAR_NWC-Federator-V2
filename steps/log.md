# log

Newest entry at the top.

## 2026-09-12 F32, the open file is guarded

### What was done

- F32 done, D2. `OpenDocumentJob.CanRun` checked one thing, that the document had a name, so an NWD opened directly was allowed and the NWD this tool publishes would have been written over the file that was open. Five things are now checked in order and `WhyNot` names the first that fails, in a person's words: it has a name, it was opened from a folder and not from an address, it is an NWF, it has a folder in front of its name, and that folder can be read from here. Each refusal says what to do instead
- `CanRun` and `WhyNot` gain a second form with the disk read handed in as `Func<string, bool>`, and the one argument form hands in `Directory.Exists`. That is the seam that lets every reason be proved without a folder that exists on the machine running the tests, and the one argument form is tested against a real temp folder and a missing one
- `NwdBeside` never names the open file. Where the swap lands on the same path, read case blind, empty comes back. `CanRun` refuses such a file before anything is written, this is the second lock on the same door
- The window did not change. `OpenDocumentLine` already shows `Describe`, which returns `WhyNot` when the file cannot run, the button is disabled by `CanRun`, and pressing it warns with `WhyNot`. The engine logs `OPEN     ` and the same reason. The window file is listed under F32 in `01_next.md` and stays unchanged, said there
- B12 narrowed. What Navisworks reports as the file name of a document opened from Autodesk Docs is still UNKNOWN, Q20, so the ACC case is not claimed. The note is under F23 in `01_next.md` and on B12 in `00_analysis.md`. CLAUDE.md's open file bullet carries the five checks
- Proved here: `OpenDocumentJobTests`. `AnNwdOpenedDirectlyStillNamesItsOutputs` is replaced by `AnNwdOpenedDirectlyIsRefusedAndSaysToOpenTheNwf`, `TheRefusalCarriesNoCodeIdentifier` by `EveryRefusalCarriesNoCodeIdentifier` over all seven refusals, and eight tests added: `AnNwcOpenedDirectlyIsRefusedTheSameWay`, `TheExtensionIsReadCaseBlind`, `ANameWithNoFolderBehindItIsRefusedAndSaysSo`, `AnAddressRatherThanAFolderIsRefusedAndNamed`, `AUncPathIsAFolderLikeAnyOther`, `AFolderThatCannotBeReadIsRefusedAndNamed`, `TheOneArgumentFormAsksTheDisk`, `TheNwdNeverLandsOnTheOpenFile`. The UNC test ignores itself where the separator is not a backslash, because a UNC path is only a path on Windows, so it is skipped under mono and runs on the local machine. Core tests under mono on Linux, before: 888 passed, 39 failed, 32 skipped, 959 total. After: 897 passed, 37 failed, 33 skipped, 967 total. The 37 are Windows path and file locking failures, none new, and two fewer than before because `TheLineLeadsWithWeeklyRunAndNeverFirstRun` and `TheLineSaysWhatItWillDoAndWhereBeforeAnyonePressesIt` now build their file in a temp folder that exists, since `Describe` asks the disk, and so pass here too
- Waits for the local machine: nothing to build for this one beyond the ordinary add-in build. Proof: open an NWD in Navisworks, open the window, the Clash step must read the refusal naming .nwd and the Run the open file button must be greyed. Open the NWF and the line must name the NWD and the report folder as before

### What remains

- F33 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F30 entry. B12 narrowed, still open on the ACC case

### What comes next

1. Merge the F32 PR
2. F33, one unit table

## 2026-09-12 F31, the clash side lookup is built once per run

### What was done

- F31 done. `LocatorOf` created a `SelectionSource` for every indexed set, per side, per compared test, and disposed each straight after. On a weekly run plus XML every test is compared, and 61 sets by two sides by 1830 tests is 223,260 sources made and thrown away for the comparison alone. `IndexSets` now builds one source per indexed set into `sourceByPath` beside `byPath`, keyed the same way, and `LocatorOf` compares each side's sources against those. The set wrappers were never disposed at all, now both dictionaries are released by `ReleaseTheIndex` in the one finally at the end of `Run`, sources first and then the set wrappers they point at, and inside `IndexSets` before a throw part way through building leaves
- `byPath` is declared before the try in `Run` so the finally can see it, and `ReleaseTheIndex` takes null as nothing to release, which is what a run that stopped before indexing holds. A throw while releasing is logged and never stops the run
- `sourceByPath` is a field, replacing the `setsForLookup` field, because `LocatorOf` is reached through `SettingsOf` from `CompareAndMaybeApply` and the set wrappers already travel as a parameter. `SettingsOf` and `LocatorOf` lose their `byPath` parameter, which they no longer read
- `FillSide` loses the document parameter nothing in it read. `Create` only handed that parameter on, so it loses it too, and `OneTest` calls `Create` without it. `Apply` and `Create` call `FillSide` positionally. `FillSide` still creates a fresh source each time and says why: the collection takes the source it is handed, so one out of the index would be taken back out from under the test when the index is released
- `Resolve`, `Upwards` and `SetBuilder` are untouched, that is F15
- UNKNOWN until a run: whether `SelectionSource.Equals` matches a source held since the sets were indexed the way it matched one created a moment before. The old code relied on the same `Equals` and it was never measured either. The DRIFT block on the proof run answers it, a locator difference on every compared test would say no
- Proved here: Core tests under mono on Linux, before and after: 888 passed, 39 failed, 32 skipped, 959 total. No Core file changed. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The diff is 108 added against 32 removed, read twice, every call of `FillSide`, `Create`, `SettingsOf`, `LocatorOf`, `IndexSets` and `ReleaseTheIndex` checked against its signature by grep. Proof: run one building twice with the XML picked, the second run's DRIFT block must report no locator difference and the CLASH block must show the same counts as the first, and the run must not be slower

### What remains

- F32 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F30 entry

### What comes next

1. Merge the F31 PR
2. F32, the open file is guarded

## 2026-09-12 F30, one tail for both run paths

### What was done

- F30 done. `RunOne` and `RunOpenDocument` each carried the same six calls after the models were in the document, the units, `ClashStep` with `SaveTheNwfAgain` on a true, `WriteWorkbook`, `WriteNwd` and `ConfirmTheNwfSurvived`, and the comments explaining the order sat on the scanned copy only. The six are now one private method, `FinishTheGroup(document, job, outcome)`, placed right after `RunOne`, called by both at the point the six stood, with the comments moved once. Nothing else in either method changes, the diff is 43 lines added against 42 removed
- Both methods were read again after the move. Ten things still differ and the PR body lists them: the signature and where the job comes from, where the document is read, the group clock and the GROUP lines that `Run` writes for a scanned group and `RunOpenDocument` writes for itself, the two no document messages, the `OpenDocumentJob.CanRun` guard, where `reportFolder` is set, the four OPEN lines against the `Decide` lines, the fixed Open decision against the three `Decide` cases, the two catch headings, and the try shape. None of them is the tail
- One comment moved as it was and is stale since F26: it says the units go before the clash step so the report reads in the units it goes out in, and since F26 the report is converted to metres in one pass whatever the document reads. The move keeps it verbatim because F30 changes nothing but the place. It goes on the audit list
- Proved here: Core tests under mono on Linux, before and after: 888 passed, 39 failed, 32 skipped, 959 total. No Core file changed. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. Proof: build, install, run one building on the scanned path and once on the open file path, and read the two logs. After the models are in, both must show UNITS, then SETS and CLASH, then `NWF      attempt`, XLSX, NWD and the final NWF line in that order

### What remains

- F31 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F27 entry, plus the stale units comment above, for the audit

### What comes next

1. Merge the F30 PR
2. F31, the clash side lookup is built once per run

## 2026-09-12 F29, the rebuild keeps the sets on their own count

### What was done

- F29 done. `RebuildFromScan` used to put the sets back only when the test count had dropped, and never counted them, so a clear that kept the tests and lost the sets would have saved an NWF whose tests point at nothing. Now the sets are counted by walking `SelectionSets.RootItem` before the clear, after the appends and after the copy is put back, every wrapper disposed on the way, and put back whenever the count after the appends is lower than before, whether or not the tests dropped. Sets first, then the tests as before, because a test side points at a set
- One line, `SETS     before clear <n>, after appends <n>, after restore <n>`, then the saved tests line. When the sets cannot be put back the error goes on the group's list the way the tests do, so the group is FAILED and the NWF on disk is not saved over
- `setsCopy` is disposed in the finally beside `testsCopy`, through `as IDisposable`, because `SavedItemCollection` was not IDisposable on the DLL measured on 2026-08-31, docs/scan.md 4g, and a plain `Dispose()` would not compile if it still is not
- The `Decision == Changed` branch at the top of `ClashStep` is deleted. Since F24 a CHANGED group is rebuilt before the clash step or returns before reaching it, so it could not run
- The set line and the two rules live in `NwfRebuildPlan`: `SetsLine`, `SetsKept`, `SetsNeedRestoring`
- Proved here: `NwfRebuildPlanTests` gains `TheSetsLineCarriesTheThreeCountsInOrder`, `SetsThatDidNotComeBackAreLostAndTheLineSaysTheNwfWasNotSavedOver`, `TheSetsArePutBackOnAnyDropWhetherOrNotTheTestsDropped`, `AnNwfWithNoSetHasNothingToKeepOrRestore`. Core tests under mono on Linux, before: 884 passed, 39 failed, 32 skipped, 955 total. After: 888 passed, 39 failed, 32 skipped, 959 total. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The engine change is read twice. `DocumentSelectionSets.CreateCopy` and `CopyFrom` are still unmeasured, as under F24. Proof: scan the C06 folder against the old NWFs, the six rebuilt groups must each log a SETS line whose three numbers agree, and the sets tree of the rebuilt NWF opened in Navisworks must hold the same sets as before

### What remains

- F30 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F27 entry

### What comes next

1. Merge the F29 PR
2. F30, one tail for both run paths

## 2026-09-12 F28, a set already there is not counted as created

### What was done

- F28 done. `SetBuilder.BuildOne` called `outcome.AddAlreadyPresent(path)` and then `outcome.AddCreated(...)` for the same set, so `CreatedCount` and `PutAnythingIn` counted present sets and the second NWF save fired on every weekly run
- `SetBuildOutcome.AddAlreadyPresent` is now one call carrying the path, the name, the condition count and the item count. It returns a `SetResult` marked `Present`, printed by `Lines()` as `present <path>  <n> conditions  <n> items  already there, left alone`, counted in `AlreadyPresentCount` and never in `CreatedCount`, `FindingItemsCount`, `ZeroCount`, `FailedCount` or `TotalItems`. `BuildOne` makes that one call and logs the line it returns
- The separate list of present paths is gone, the count comes off the one results list the lines come from, so the two cannot disagree
- Proved here: `SetBuildOutcomeTests`, every `AddAlreadyPresent` use takes the add-in shape, and three tests added: `SixtyOnePresentAndNoneCreatedPutNothingInAndPrintsNoOkLine`, `SixtyPresentAndOneCreatedPutSomethingIn`, `APresentLineCarriesItsItemCountAndIsNotCreated`. Core tests under mono on Linux, before: 881 passed, 39 failed, 32 skipped, 952 total. After: 884 passed, 39 failed, 32 skipped, 955 total. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The `BuildOne` change is five lines, read twice. Proof: run one building twice with the XML picked, the second run's SETS block must show every set as present, `sets created      : 0`, `already there     : 61`, and no second `NWF      attempt` line after the CLASH block unless a test was created

### What remains

- F29 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F27 entry

### What comes next

1. Merge the F28 PR
2. F29, the rebuild keeps the sets on their own count

## 2026-09-12 F27, the GROUPS block tells the truth

### What was done

- F27 done, F25 closed. The claim was checked: nothing in src sets `GroupRow.Include` to false except the Run column binding and `GroupRow.Blocked`, and `git show be0b9b37:src/Federator.Addin/Ui/GroupRow.cs` shows the same. There is no hidden discipline rule. The run log of 2026-09-07 showed 12 groups as skipped because the GROUPS block printed skipped for any group unticked in the Run column
- The block now prints `unticked` for such a group and ends with one line, `<n> groups unticked in the Run column, nothing else drops a group`. The line lives in `RunLog.UntickedGroupsLine` so it is tested, and the window calls it
- `01_next.md` rewritten: F25 closed with the reason, F27 to F38 added after F26 in the order set on 2026-09-12, F12, F13 and F14 marked absorbed by F37 and F38. B15 closed in `00_analysis.md` and Q21 closed in `02_questions.md` with the same reason
- `00_analysis.md` step 8 fixed. `HealthCheck.Run` is not called by anything in src today. F34 wires it, D1
- README label table matches `RunPath.All`: Rebuilt added, Skipped kept and reworded, because a rebuild that does not finish still ends its group as Changed and the GROUP line reads Skipped
- Proved here: `RunLogTests` gains `TheUntickedLineCountsTheGroupsAndNamesTheRunColumn` and `NothingUntickedStillSaysSo`. Core tests under mono on Linux, before: 879 passed, 39 failed, 32 skipped, 950 total. After: 881 passed, 39 failed, 32 skipped, 952 total. The 39 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The window change is nine lines in `GroupListLines`, read twice. The proof is to scan any folder, untick two groups, press Run and Cancel, and read the GROUPS block in the log

### What remains

- F28 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- B15 closed. Everything else as in the F26 entry

### What comes next

1. Merge the F27 PR
2. F28, a set already there is not counted as created

## 2026-09-12 The plan for the audit round, F27 to F38

### What was done

- Read first: CLAUDE.md, `steps/00_analysis.md`, `steps/01_next.md`, `steps/02_questions.md`, `steps/03_bader_next.md`, `steps/log.md`, then every file under `src`, `tests`, `build`, `docs`, `.github`, `.githooks`, `bundle` and the root. 47,658 lines. The files every fix touches were read by hand. One reader per folder is still walking the rest for members without a caller, doubled comment blocks and stale comments, and what they find goes into F36 and the audit at the end
- Ground state on main at c89dad07. Core tests under mono on Linux: 879 passed, 39 failed, 32 skipped, 950 total. The 39 are the Windows path and file locking failures recorded since F5
- `dotnet build src/Federator.Addin/Federator.Addin.csproj -c Release` was run once. It cannot build here: `Autodesk.Navisworks.Api.dll` is not on this machine and the build says so by name. So every add-in change is read twice before it is pushed, the PR body says so, and the local proof goes into `steps/03_bader_next.md` as numbered one action steps
- There is no `gh` on this machine. The checks are watched through the GitHub tools the session has, the same way F5 to F26 were. Merge only when the tests job is green on the branch head. A branch delete through git is refused by the session's proxy, so D6 goes through the GitHub tools
- Eleven doubled summary blocks found by grep, in `RunLog.cs`, `PageCheck.cs`, `WorkbookWriter.cs`, `ReportPaths.cs` twice, `ClientFormat.cs`, `ClashHarvest.cs`, `SetBuilder.cs`, `ClashRunner.cs` and `FederatorWindow.xaml.cs` twice. Twelve private `Or` helpers across Core and the add-in. All for F36
- The F36 greps run on main: `OutputNameCheck`, `ClashWork.Any`, `BuildStamp.OfCore`, `PickerStart.Remembers`, `RunLog.TimesFailed`, `DistinctFailureCount` and `TotalFailureCount` have no caller in src. `BuildOutputName` is called only by itself and `ForcedLevel`, `ForcedTypeCode`, `ForcedNumber` and `OutputDisciplineCode` only by it. `GroupRecords` is read once inside `RunLog` itself. `RunPath.Skipped` IS reached: a rebuild that fails ends the group as Changed and the GROUP line reads it, so it stays

### The order and what each PR does

1. F27. Branch `fix-F27`. The GROUPS block says unticked instead of skipped and adds one line counting the unticked groups. The line lives in `RunLog` so it is tested. F25 rewritten as never in the code, B15 and Q21 closed with that. `00_analysis.md` step 8 fixed, HealthCheck does not run on pick today. README label table matches `RunPath.All`. This entry and the F27 entry go in with it
2. F28. Branch `fix-F28`. `SetBuildOutcome.AddAlreadyPresent` takes the path, the name, the condition count and the item count, prints as present, counts in `AlreadyPresentCount` only. `BuildOne` makes that one call. Two new tests on `PutAnythingIn`
3. F29. Branch `fix-F29`. `RebuildFromScan` counts the sets before the clear and after the appends by walking `SelectionSets.RootItem`, puts the sets back on their own count, logs `SETS before clear <n>, after appends <n>, after restore <n>`, fails the group when they cannot come back, disposes `setsCopy`. The dead Changed branch at the top of `ClashStep` goes. The set line and the set judgement live in `NwfRebuildPlan` with tests
4. F30. Branch `fix-F30`. One private tail method for `RunOne` and `RunOpenDocument`. The PR body lists every line that still differs
5. F31. Branch `fix-F31`. `IndexSets` builds one `SelectionSource` per set beside `byPath`, `LocatorOf` compares against that, both dictionaries disposed in one finally at the end of `Run`. `FillSide` loses its document parameter. `Resolve`, `Upwards` and `SetBuilder` untouched, that is F15
6. F32. Branch `fix-F32`. `OpenDocumentJob.CanRun` refuses a non .nwf extension, a path with no readable folder, and an NWD path equal to the open path, `WhyNot` names which. Five tests. The window shows `WhyNot`. F23 in `01_next.md` notes the narrowing
7. F33. Branch `fix-F33`. `src/Federator.Core/Units/UnitTable.cs` is the one table. `ExchangeUnits`, `DocumentUnits.Short`, `ClashRunner.UnitName`, the window's `UnitChoices` and `UnitWording` read it. `WantedUnits` fails the group on a name the table does not know. `ExchangeReader` stops converting, `ToleranceMillimetres` and `ToleranceIn` go, `ClashTestPlan.Convert` is the one place a file unit is judged, with a test that reaches its unknown unit line
8. F34. Branch `fix-F34`. Window wiring: `NwdFolderBox` TextChanged, `OnXmlChanged`, `OnRepublishChanged` and `republishNwd` and the three unused engine constructors deleted, `NwdRequested` deleted if nothing reads it, `ReportsWanted` stops setting the three fixed flags, D3 the outstanding count setting deleted, D4 the two hand buttons call one engine method each, D1 HealthCheck wired under the file line, the XAML comments and the window class comment fixed, `GroupOutcome.cs` docs stop naming a republish tick box
9. F35. Branch `fix-F35`. D5. `BuildingGroup` and `FederationJob` carry the discipline count, the flag comes from it, `ClashSkipReason.SingleModel` becomes `SingleDiscipline`, every one model wording becomes one discipline. Two Core tests
10. F36. Branch `fix-F36`. Dead code and copies out, each name grepped and the empty result pasted in the PR body. One `Or` in `Words.cs`. One client column list. `InstallFiles` replaces the two locators. The eleven doubled summaries fixed. `probe-window-defaults.ps1` stops listing the two boxes that are gone
11. F37. Branch `fix-F37`. Moves only. One type per file, tests into src folder names, probes into `tools/probes` with a README and a path parameter, the two docs into `docs/history` with a first line, `docs/workflow.md` written from README and `RunPath`. Closes F12 and F14
12. F38. Branch `fix-F38`. CLAUDE.md under 200 lines, history to `docs/history/claude-md-history.md`, four rules files under `.claude/rules` with paths frontmatter, two PreToolUse hooks under `.claude/hooks` wired in `.claude/settings.json`, the pre-commit hook kept. Closes F13
13. D6. Every `fix-*` branch, `analysis-pass` and `master` deleted on GitHub so main is the only branch
14. The audit. Every file read again, what still contradicts CLAUDE.md, every member without a caller, every doubled comment, every doc line that disagrees with the code, every test that cannot run here, written to `steps/04_audit.md` with a fix list, and the fixes added to `01_next.md`
15. `steps/03_bader_next.md` rewritten so the proofs for F28, F29, F31, F32, F33, F34 and F35 sit in one build, one install and one Navisworks session, with the first proof named and why
16. The closing log entry

### Rules held through the round

- One PR per fix, off main, merged only when the tests job is green on the branch head, the branch deleted after. Never a PR left open at the end of a step
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`
- A public member nothing in src calls is deleted with its tests, unless a decision keeps it. No member is added that no running code calls
- No em dash, no semicolon in prose, no emoji, plain words. Every log line starts with its block word
- Each PR body says what was proved here and what waits for the local machine. Each log entry records the Core test counts before and after
- `samples` and `steps/logs` are never touched

### What remains

- Everything in the order above. Nothing in this round is proved on the local machine, the add-in cannot build here

### Known bugs

- Unchanged from the F26 entry until each fix lands. B15 closes with F27, F25 was never in the code

### What comes next

1. F27 on `fix-F27`, this plan goes in with it
2. F28 to F38 in order, one PR each
3. D6, the audit, `03_bader_next.md`, the closing entry

## 2026-09-07 F26, the report is always in metres

### What was done

- `steps/logs` holds one run log, `run-20260907-093440.log`, already read and recorded under F24. No new log since, so every fix from F5 onward stays pending local proof
- F26 done, bug B14. Every UNITS line of that log was read. 14 groups say the document is in Feet and the report needs Meters. 12 lines say THE DOCUMENT DID NOT FOLLOW. Only 2 groups followed, 1B06K1 and 1B06M1, and both had exactly one model set. Every group with 2 or more models set stayed in feet
- What the code set and what it read. It SET each model through `DocumentModels.SetModelUnitsAndTransform`, which is the only public managed member in the API that sets units at all, measured across all 4027 types on 2026-09-01, docs/scan.md 4q. It then READ `Document.Units`, which has no setter anywhere, and judged the report by it
- Why a document can stay in feet after every model is set. Two different things carry the word units. A model's units are a per file override, which is what `SetModelUnitsAndTransform` writes and what the Units and Transform dialog shows. What the scene is measured and displayed in is an application option, Options, Interface, Display Units, which Autodesk describes as used to measure geometry, align appended models and set tolerances for clash detection. It is not a document property and the API exposes no setter for it. So setting every model changes each file and leaves the display unit alone. Why one model set made `Document.Units` report metres and two did not is UNKNOWN and cannot be read off the DLL
- The fix does not wait for the document to follow. New `src/Federator.Core/Report/ReportUnits.cs` converts the FINISHED report into metres in one pass, before the workbook, the page or the XML is written, so all three read the same numbers and the same label. Converted: the tolerance of every test, and the distance and the clash point of every row. Not converted: grid location, level, status, counts, names, because none of them is a measurement
- The factors are `ExchangeUnits`, which is the one conversion table in this repo. Nothing was duplicated
- A unit the table has not been taught is REFUSED. Nothing is written, three skipped lines are logged, and the group is FAILED through the error list, which `GroupJudgement` already turns into FAILED. No rule in the judgement needed changing
- Setting the models is kept, unchanged, because it fixes what the person sees in Navisworks. No application option is touched, so nothing has to be restored afterwards
- The log line changed. `UNITS    <n> models set, <n> that would not, document shows Feet (ft). Every report number is converted to Meters (m) before it is written`, then after the clash step `UNITS    every number converted from ft to m, one ft is 0.3048 m, <n> tests and <n> rows. The report is written in Meters (m)`. The words DID NOT FOLLOW are gone from the code
- The window combo is now labelled Model units, its grey line reads `What Navisworks shows. The report is always in metres.`, and the run settings carry two lines, `model units` and `report units : Meters (m), always, converted before anything is written`
- Tests: `ReportUnitsTests`, sixteen. The table over m, ft, in, mm and cm. The two report unit names. The tolerance converted with its label, 0.2460629921 ft reading 0.075m in the cell. A row's distance and clash point converted with the sign kept. Nothing but the measured numbers touched. The report label afterwards. A document already in metres converting nothing and not doubling. An unknown unit refused with nothing half converted and the label not faked. A document that said nothing about its units refused. A null report refused. The log line naming the unit, the factor and the counts. Every test and every row counted
- Core tests under mono on Linux, before: 863 passed, 39 failed, 32 skipped, 934 total. After: 879 passed, 39 failed, 32 skipped, 950 total. The 39 are the same Windows path and file locking failures, none new
- The add-in was not compiled. It cannot compile in the container

### What remains

- F25 next, then F16, F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered
- The proofs from F5 onward on the local machine, F24 first and F26 second, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- B14 fixed in code, pending local proof. Run one group whose models are in feet, the report header must say Meters (m), one clash distance must match the Clash Detective panel when the panel is switched to metres, and the log must carry the new UNITS line with no DID NOT FOLLOW anywhere
- B13 fixed in code, pending local proof. Pull main, build, install, scan the same C06 folder, the six groups must show Rebuilt in the list, and after the run each NWF must hold every scanned file and the NWD must hold them too
- B15 OPEN, F25 next
- M5 closed on the Core side, still open on the add-in side because the add-in build stays local
- L4 fixed in code, pending local proof. The picture on row N of a block must be picture N in the order the Clash Detective panel lists the clashes
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. The XML box on with every image status unticked, then the other way round
- L2 fixed in code, pending local proof. Under F24 a folder that differs is rebuilt, so the proof is that no UNITS line comes before the REBUILT block
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED
- B2 fixed in code, pending local proof. One Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. The open file run must end with a RESULT block and a log copy
- L1 fixed in code, pending local proof. No XML, the tests saved in the NWF must run
- F22 done in code, pending local proof. The list must show First run and Weekly run per group
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M4, M6 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F26 PR
2. Bader pulls main, builds, installs, and works through `03_bader_next.md`, F24 first and F26 second, then the rest, and drops every log into `steps/logs`
3. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
4. Worker reads the logs and records the proofs in this file
5. Worker starts F25, dropping the hidden discipline rule

## 2026-09-07 F24, a CHANGED NWF is rebuilt from the scan

### What was done

- The first run log arrived. `steps/logs/run-20260907-093440.log`, moved there from a root `Log` folder, which is deleted. It is a run on the old build be0b9b37 of 1 Sep, before any fix, on the C06 folder, 76 files, 26 groups, no clash XML
- What the log proves. F9 was needed and is seen live: six CHANGED groups fell through to UNITS and published an NWD off a federation missing models. F22 labels would have shown those six as Skipped. The 8 DONE groups are right, OPENED, units, NWD, NWF intact. The SOURCE MISMATCH and SHARED SOURCE lines are Revit naming, 1B06BC published from 0000BC and so on, and 1B06M1 with 1C06M2 both from 1B06MM. Not the tool. Total 352 seconds for 14 groups with no clash step, which is the first timing baseline. Noted against M2: about 2 to 6 seconds per group to open, set units and publish, so the clash step is the whole of the 45 minutes
- Three new bugs from the log, in `00_analysis.md` section 3. B13 CHANGED left the NWF alone with the scan folder holding the right files. B14 UNITS said THE DOCUMENT DID NOT FOLLOW on 11 of 14 groups and the report went out in feet. B15 12 of 26 groups were dropped before the run by a discipline rule the window does not show
- Bader answered Q21, Q22 and Q23 in `02_questions.md`. Q21 federate what is ticked, no hidden discipline rule. Q22 rebuild a CHANGED NWF from the scan folder, keep the saved tests, say what was added, moved and removed. Q23 the report is always in meters, force the document and fail loudly
- `01_next.md` gains F24, F26 and F25 right after F20, in that order, then F16 and the rest
- F24 done, bug B13. Before, a CHANGED group was left alone entirely and judged PARTIAL, the F9 rule. After, it is rebuilt: the saved tests are read, the document's own copies of the tests and the sets are taken, the document is cleared, the scan is appended in scan order, the copies are put back where the clear dropped them, the count is read again, and only then is the NWF saved over. From there it is an opened group: units, the clash step, the reports, the NWD
- New `src/Federator.Core/Rerun/NwfRebuildPlan.cs`. A file on both sides under a different folder is a move, a file only in the scan is an addition, a file only in the NWF is a removal, the files to append are the scan in scan order. `Lines` gives the REBUILT block, `SavedTestsLine` the one line with the three numbers, `SavedTestsKept` the rule the engine acts on
- `RerunDecision.Rebuilt` added. `RunPath.Rebuilt` is the label, with `AfterOpening` for a comparison that reads Changed before the run. The confirm dialog counts Rebuilt groups and says the NWF is cleared and rebuilt from the scan folder with its saved tests kept. The RESULT block carries `rebuilt        : n`. `GroupJudgement` judges a rebuilt group DONE when everything after went right and FAILED when the rebuild appended nothing
- The window opens each NWF on disk at Run, before the confirm dialog, so the six show Rebuilt in the list and the dialog counts them. Only when nothing open would be lost, because opening an NWF replaces the open document and Run cancelled must still mean nothing was cleared. Otherwise the dialog says Rebuilt is only known once each NWF is opened. After the run every group reads what it actually did
- Whether the tests survive `Document.Clear` is UNKNOWN from here. `DocumentClashTests.CreateCopy` and `CopyFrom` were read off the DLL on 2026-08-27, docs/scan.md section 4, and `ClashTestsData` is disposable. The same pair on `DocumentSelectionSets` was NOT measured and is used on the strength of the pattern every document part follows, so a build error on Bader's machine would name exactly that. The engine reads the count after the appends, puts the copies back only where the count dropped, reads it again, and the log line says which of the two happened
- `NwfComparison.Lines` on CHANGED is the heading and the counts only, the per file lines moved to the REBUILT block so a move never reads as an addition and a removal in one place and a move in another. `SkipLine` deleted with its test, nothing skips any more
- Tests: `NwfRebuildPlanTests`, fifteen, the 1B06BC case from the log with 4 moves, 1 addition and 0 removals, the append order, a move with its from and to folders, a pure addition, a pure removal, a move and a removal of one name told apart, no rebuild for Open or Build, the null refusal, the REBUILT lines, the removed line, and the four saved tests lines. `RunPathTests` gains four, six labels, Rebuilt with or without an XML, AfterOpening, the confirm lines with and without a Rebuilt group. `GroupJudgementTests` gains four, rebuilt done, rebuild appended nothing failed, rebuilt with a failed file partial, rebuilt NWD not published failed
- Core tests under mono on Linux, before: 840 passed, 40 failed, 32 skipped, 912 total. After: 863 passed, 39 failed, 32 skipped, 934 total. The 39 are the same Windows path and file locking failures, one fewer because the CHANGED lines test no longer asserts a Windows path. None new
- The add-in was not compiled. It cannot compile in the container

### What remains

- F26 next, then F25, F16, F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered
- The proofs from F5 onward on the local machine, F24 first, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- B13 fixed in code, pending local proof. Pull main, build, install, scan the same C06 folder, the six groups 1B06BC, 1B06G1, 1B06K1, 1B06M1, 1B06P1 and 1B06PE must show Rebuilt in the list, and after the run each NWF must hold every scanned file and the NWD must hold them too. Drop the log into `steps/logs`
- B14 OPEN, F26 next. B15 OPEN, F25 after it
- M5 closed on the Core side, still open on the add-in side because the add-in build stays local
- L4 fixed in code, pending local proof. Run one group with pictures on, open the Excel, the picture on row N of a block must be picture N in the same order the Clash Detective panel lists the clashes, the blocks numbered most clashes first, and every link must open its picture. The log must carry one IMAGES numbered in report order line
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code and seen needed in the run log, pending local proof. The proof changes with F24: a folder that differs from the NWF now reads Rebuilt, so the proof is that no UNITS line comes before the REBUILT block
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M4, M6 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F24 PR
2. Bader pulls main, builds, installs, and runs the C06 folder again, F24 first in `03_bader_next.md`, then the rest of the proofs, and drops every log into `steps/logs`
3. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
4. Worker reads the logs and records the proofs in this file
5. Worker starts F26, the report always in meters

## 2026-09-07 F20, the Core tests run on every push to main

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- Order changed in `01_next.md` as Bader set it. F15 moved to the end of the code fixes. From here: F20, F16, F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered. F15 carries one new line, read `steps/05_api_notes.md` first if it exists. That file does not exist yet
- F20 done. `.github/workflows/tests.yml` triggered on pull_request and workflow_dispatch only. It now also triggers on push to main. Same job, same four steps, no second workflow file
- The job runs the Core tests only. It restores and tests `tests/Federator.Core.Tests/Federator.Core.Tests.csproj`, whose only project reference is `src/Federator.Core/Federator.Core.csproj`. Nothing in it names the add-in project or the solution, so the runner never tries to build the add-in. Nothing to fix there
- On a PR merge the same commit runs once as the PR check and once as the push run on main. The push run is a second run of the same job on the same commit and blocks nothing, because no branch rule waits on it. That is the ordinary shape of a push plus pull_request workflow and it is left as it is
- No test was changed
- Core tests under mono on Linux, unchanged: 840 passed, 40 failed, 32 skipped, 912 total. The 40 are the same Windows path and file locking failures
- The add-in was not compiled. It cannot compile in the container

### What remains

- F16 next, then F21, F12, F13, F14, F15, F18 when the sample arrives, F23 when Q20 is answered
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- M5 closed on the Core side. The Core tests run on every PR and every push to main. Still open on the add-in side, because the add-in build stays local, F19 dropped by Q10
- L4 fixed in code, pending local proof. Run one group with pictures on, open the Excel, the picture on row N of a block must be picture N in the same order the Clash Detective panel lists the clashes, the blocks numbered most clashes first, and every link must open its picture. The log must carry one IMAGES numbered in report order line
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M4, M6 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F20 PR and read the push run on main
2. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F16, the tests made path neutral

## 2026-09-07 F17, the pictures are numbered in the export order

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- F17 done, logic problem L4. The pictures used to take their test number from the order the tests RAN, because they are rendered while the tests run and the report is sorted afterwards. The rows are written in report order. So the first block of the workbook, the test with the most clashes, carried whatever number its run position gave it, and the Navisworks export numbers that block cd00
- Worked example, three tests in run order: Floors ran first with one clash, Walls second with three, Columns third with two. Before: Floors 1 was cd000001, Walls 1 to 3 were cd010001 to cd010003, Columns 1 and 2 were cd020001 and cd020002. The workbook lists Walls, Columns, Floors, so row 1 of the workbook linked to cd010001. After: Walls 1 to 3 are cd000001 to cd000003, Columns 1 and 2 are cd010001 and cd010002, Floors 1 is cd020001. Row N of a block links to picture N of that block
- The rule in one sentence: the pictures are numbered in the order the rows are written, tests most clashes first with ties in creation order, and inside a test the clashes as Clash Detective lists them
- The order is only known when the last test has run, so the pictures are still rendered under the run order number and renamed ONCE after the run, in one pass, before any report is written. The rename goes through a holding name first, because a swap between two tests would otherwise write one picture over another. What a picture shows and its size are untouched, only the number changes
- New `src/Federator.Core/Report/ReportOrder.cs`. `ReportOrder.Tests`, `Rows` and `PictureNumbers` give the order and the number of every picture, `PictureNumberFor` gives one row its number, `ImageRenumbering.Apply` does the rename and repoints the row's file, link and path, and `ImageRenumberingOutcome` says how many were renamed, already right, or missing
- `FederationEngine.RenumberThePictures` calls it right after the clash step when pictures were rendered. The log gets one line, `IMAGES   numbered in report order: <n> renamed, <n> already right, <n> missing`. A missing picture or a throw is a report warning and never fails the group
- The workbook link, the XML href and the page all read the row's link, so repointing the row is what moves all three. Two tests write the workbook and the XML after the rename and read the link back off the file
- CLAUDE.md has a new picture bullet, `docs/scan.md` 4p closes the difference it recorded and the 4q row reads same
- Tests: `ReportOrderTests`, sixteen. The order on a mixed sample, a tie keeping creation order, rows keeping Clash Detective's order, every picture carrying its row number, the old run order numbering shown as wrong, no gap for a test without a picture, a row without a picture not numbered, the rename giving row number equal to picture number with each file checked by content, a swap overwriting nothing, no holding file left, already right pictures not moved, a missing picture named and the rest renamed, no pictures renaming nothing, the workbook link and the XML href pointing at the renamed file
- Core tests under mono on Linux, before: 824 passed, 40 failed, 32 skipped, 896 total. After: 840 passed, 40 failed, 32 skipped, 912 total. The 40 are the same Windows path and file locking failures, none new, none in the new fixture
- The add-in was not compiled. It cannot compile in the container

### What remains

- F23 once Q20 is answered, then F15 onward in `01_next.md` in order
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- L4 fixed in code, pending local proof. Run one group with pictures on, open the Excel, the picture on row N of a block must be picture N in the same order the Clash Detective panel lists the clashes, the blocks numbered most clashes first, and every link must open its picture. The log must carry one IMAGES numbered in report order line
- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L5 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F17 PR
2. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
3. Bader follows `03_bader_next.md`, the F17 proof sits after the F10 proof, and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F15, or F23 if Q20 is answered first

## 2026-09-07 F11, the dead code is gone

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- F11 done. Every name in B9 was searched across source, tests, docs, CLAUDE.md, samples and build scripts, and sorted by who reads it: running code, a test only, or a doc only
- Deleted, because nothing running read them: `ReportOptions.ClientColumnsOnly`, `SheetNames.SummarySheet`, `SheetNames.MatrixSheet`, `SheetNames.ForTest` with `TestPrefix` and `TestDigits`, `TestReport.SheetName`, the whole of `ClashMatrix.cs` with `ClashMatrix`, `MatrixCell` and `MatrixCellKind`, and two members orphaned by that, `OpenClashes.Heading` and `OpenClashes.SheetLabel`, which only the sheets that are gone ever showed
- Kept: `TestReport.HasSheet`, read by `ClashReportXml` and `ClashRunner`. Kept: the outstanding count setting, `ReportOptions.OpenCount` and `OpenClashes`, read by the window, the engine and `ImageOptions`, though no output shows the number now that the Summary and Matrix sheets are gone. Flagged for Bader rather than deleted, because it is a window control and a CLAUDE.md rule
- Tests: `ClashMatrixTests` deleted whole, the three matrix tests and the sheet label test cut out of `OpenClashesTests`, the matrix test cut out of `SingleModelGroupTests`, the seven `ForTest` tests cut out of `SheetNameTests` with two rewritten onto the one sheet name, and the `SheetName` asserts cut out of `ClashReportTests`. Twenty nine tests removed and three rewritten in their place, so twenty six fewer
- Docs: CLAUDE.md lost the three bullets that described the T0001 sheets, the Summary rows and the matrix cell, and two lines were reworded so nothing points at the matrix or a sheet label. `docs/test-model-side.md` step 53 now says those sheets are history. The `docs/scan.md` 4q row for `ClientColumnsOnly` records its removal and stays. The probe script no longer lists `ClientColumnsOnly`. The window label `The matrix counts` reads `Outstanding counts` and the log key with it
- The report output is unchanged. The tests that pin the one sheet, the cells, the client layout, the page and the client columns all still pass: `OneSheetWorkbookTests`, `WorkbookCellCheckTests`, `ClientLayoutTests`, `ReportCheckTests`, `ClientColumnsTests`
- Core tests under mono on Linux, before: 850 passed, 40 failed, 32 skipped, 922 total. After: 824 passed, 40 failed, 32 skipped, 896 total. The 40 are the same Windows path and file locking failures, none new
- The add-in was not compiled. It cannot compile in the container. A plain text search of `src/Federator.Addin` for every deleted name returns nothing
- `03_bader_next.md` build step says the build must finish with no error about a missing name

### What remains

- F23 once Q20 is answered, then F17 onward in `01_next.md` in order
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader, and whether the outstanding count setting stays now that nothing shows it

### Known bugs

- B9 fixed. The local proof is only that the add-in builds
- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, L4 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F11 PR
2. Bader answers Q20 in `02_questions.md` and says whether the outstanding count setting stays
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F17, or F23 if Q20 is answered first

## 2026-09-07 F10, each output gated on its own flag

### What was done

- `steps/logs` still holds only its README. No run log yet, so every fix from F5 onward stays pending local proof and nothing about them was recorded
- F10 done. The ClashReport that feeds the workbook, the XML and the HTML page was built only when the workbook or the XML was wanted, so the page rode on the workbook flag. The pictures were rendered only when the workbook was wanted, so they rode on it too. Both are fixed on today, so nothing was lost yet, and switching the workbook off would have taken the page and the photos with it
- The rule is `Federator.Core.Report.OutputPlan`, built from the ReportOptions flags. The report is built when the workbook, the XML or the page is wanted. Pictures are rendered when they are wanted and at least one of the three is wanted to link them from, because the page embeds them, the workbook links them and the XML carries their href. With none wanted there is nowhere a picture could be found
- The engine reads that plan in `CreateAndRunTheTests` and `WriteWorkbook`. The NWD keeps its own flag, republishNwd, which was already its own
- Every output gets one log line whichever way it went. Written ones keep the `attempt` and `written` lines. Skipped ones get `XLSX     skipped  <reason>` from the new `RunLog.WriteSkipped`, in the same shape, with the reason: not wanted this run, no report folder, no clash step ran, images switched off, nowhere to link a picture, or the stylesheet not found. One `OUTPUTS` line names the four flags before the clash step. The lines come from the shared engine methods, so the scanned run and the open file run write the same
- The window wiring was checked. Every tick box reaches the engine: the XML box, the thumbnails box, the five image status boxes, apply file settings, compact resolved, date the NWD, include subfolders. The engine reads three report flags that no tick box sets, because CLAUDE.md fixed them on: the workbook, the page and the pictures. Pictures still switch off when every status box is unticked. Nothing was broken and no box was added
- What each output contains is untouched
- Thirteen Core tests added. Twelve in `OutputPlanTests`, eight of them the full table of the three report flags, and one in `RunLogTests` for the skipped line
- Core tests under mono on Linux: 850 passed, 40 failed, 32 skipped, 922 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` has the F10 proof after the F9 proof. The proof Bader named needs an Excel box and an HTML box that the window does not have and that were not added, so the proof uses what the window can switch: the XML box and the five image status boxes

### What remains

- F23 once Q20 is answered, then F11 onward in `01_next.md` in order
- The proofs from F5 onward on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- L3 fixed in code, pending local proof. Run one group with the XML box on and every image status unticked, the log must show XML written, IMAGES skipped with images switched off, XLSX and HTML written. Then the XML box off and the statuses back on, the log must show XML skipped as not wanted and the pictures rendered
- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, B9, L4 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F10 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F11, or F23 if Q20 is answered first

## 2026-09-07 F9, a CHANGED group is left alone before anything touches it

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11, L1, F22 and B7 stay pending local proof and nothing about them was recorded
- F9 done. A CHANGED group used to fall through Decide into the units change, the clash step, the workbook, the NWD publish and the survival check before anyone left it alone. Every model had its units set and a UNITS line was logged, an NWD was published off a federation that no longer matched the folder, and the log said `NWF reused`
- The CHANGED check now sits right after Decide and before anything touches the document in memory. Nothing is done: no units change, no sets, no tests, no save, no NWD. The NWF and NWD sizes are read off the disk for the RESULT block and nothing is recorded as written
- One log line, `NwfComparison.SkipLine` in Core, names the group, the files added and removed, and every step that was not done. The CHANGED block from Decide still lists each file above it
- Reading the file list is the one thing that happens before the check, and it opens the NWF to read it. That is a read, not a change to the NWF on disk
- `GroupJudgement` judges a CHANGED group PARTIAL right after the error and NWF on disk checks, before the NWD checks, so the NWD it no longer publishes cannot make it FAILED. The GROUP finished line carries `Skipped (changed on disk)` from F22 and the RESULT block counts it under skipped
- CLAUDE.md updated: the NWD is republished in the Build and Open cases, and the CHANGED rule says what the check comes before
- Three Core tests added, two in `GroupJudgementTests` and one in `NwfComparisonTests`. The order of the steps lives in the add-in and cannot be tested here
- Core tests under mono on Linux: 837 passed, 40 failed, 32 skipped, 909 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` has the F9 proof after the F22 proof

### What remains

- F23 once Q20 is answered, then F10 onward in `01_next.md` in order
- The F5, F6, F7, F8, F22, F1 to F4 and F9 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- L2 fixed in code, pending local proof. Add or remove one NWC in a folder whose NWF exists, press Run, the group must show Skipped (changed on disk), the log must show no UNITS line for it, and the NWF must keep its old modified time
- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3, B4, B8 and B10 fixed
- B12 OPEN, waiting on Q20
- B5, B6, B9, L3 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F9 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F10, or F23 if Q20 is answered first

## 2026-09-07 F1, F2 and F4, the small fixes

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11, L1 and F22 stay pending local proof and nothing about them was recorded
- F1, B8. The test `ALogoOnTheePageIsCheckedAgainstTheDisk` in `ReportCheckTests.cs` is now `ALogoOnThePageIsCheckedAgainstTheDisk`. Name only
- F2, B7. `build/probe-window-defaults.ps1` read `$repo = "C:\Users\p003653k\source\repos\Parsons NWC Federator"`. It now reads `$repo = Split-Path $PSScriptRoot`, the same line the scroll and labels probes use
- F4, B10. The `DocumentUnits` docstring said the units change is off unless asked for. The window sets it on for every run, so the sentence now says it is on for every run, because the window sets it on and every report the team sends is metric
- The same machine path sat in `docs/test-model-side.md` step 2 as a `cd` line. It now says to change into the folder the repo is checked out in. `docs/scan.md` line 7 still names the user as part of the machine record of the scan, which is a measurement and not a path, so it stays
- F3 was closed inside F22, so F1 to F4 are all done
- Core tests under mono on Linux: 834 passed, 40 failed, 32 skipped, 906 total. Unchanged, none new. The renamed test is one of the 32 that skip without a Navisworks install
- The add-in was not compiled. It cannot compile in the container. The probe runs on Windows only, against the built add-in
- `03_bader_next.md` has one step at the end to run the probe

### What remains

- F23 once Q20 is answered, then F9 onward in `01_next.md` in order
- The F5, F6, F7, F8 and F22 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B7 fixed in code, pending a local run of `build/probe-window-defaults.ps1`
- B8 fixed. B10 fixed
- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3 and B4 fixed in code inside F22, pending the same proof
- B12 OPEN, waiting on Q20
- B5, B6, B9, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F1 F2 F4 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md`, runs the probe at the end, and drops the logs into `steps/logs`
4. Worker reads the logs and records the proofs in this file
5. Worker starts F9, or F23 if Q20 is answered first

## 2026-09-07 F22, two clear workflows in the window, and F3 with it

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2, B11 and L1 stay pending local proof and nothing about them was recorded
- The F8 entry sat in this file twice. The fuller one is kept and the other is gone
- F22 marked confirmed by Bader on 7 Sep 2026 in `01_next.md` with the two definitions
- F22 done. The rule is `Federator.Core.Rerun.RunPath`, beside Decide. Given the Decide result and whether an XML is picked it returns one of five labels: First run, Weekly run, Weekly run plus XML, Skipped (changed on disk), Unknown. Before the run the label is worked out from whether the NWF is already at its output path, because CHANGED is only known once the NWF is opened
- The group list has a Run as column, filled after Scan and refreshed when the NWF folder, a name, or the XML box changes. Blocked groups show nothing there
- The confirm dialog opens with the count per label and says cleared only for the First run groups. The Skipped line says it is only known once each NWF is opened. Then the existing lines about what is open and Carry on. That closes F3: B4 was the dialog saying cleared before every group, and B3 the NWD line naming a tick box that is gone, which is reworded too
- Run the open file: the blue line leads with Weekly run or Weekly run plus XML, and the help says there is no First run on that path
- The log carries the label on every GROUP finished line and in the GROUPS block before the run, and the RESULT block totals them: first run, weekly run, weekly plus XML, skipped
- CLAUDE.md has a Two workflows section with the two definitions and the labels. `README.md` created at the root with the same section. F14 stays open for the rest of a README
- The engine does the same work as before. Only what the person is told changed, plus one log string
- Fourteen Core tests added. Eleven in `RunPathTests`, one in `OpenDocumentJobTests`, two in `RunLogTests`
- Core tests under mono on Linux: 834 passed, 40 failed, 32 skipped, 906 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7, F8 and F22 proofs in one session

### What remains

- F23 once Q20 is answered, then F1, F2 and F4 in one PR, then the rest of `01_next.md` in order
- The F5, F6, F7, F8 and F22 proofs on the local machine, then P1, P2, P3
- Q20 from Bader

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the same on the scanned run with no XML on a folder whose NWFs already hold tests
- F22 done in code, pending local proof. Scan a folder that holds some NWFs and lacks others, pick no XML, the list must show First run beside the groups with no NWF and Weekly run beside the others, and the confirm dialog must show the counts
- B3 and B4 fixed in code inside F22, pending the same proof
- B12 OPEN, waiting on Q20
- B5 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F22 PR
2. Bader answers Q20 in `02_questions.md`
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the five proofs in this file
5. Worker starts F1, F2 and F4 as one PR, or F23 if Q20 is answered first

## 2026-09-06 F8, run the saved tests when no XML is picked

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1, B2 and B11 stay pending local proof and nothing about them was recorded
- B12 registered in `00_analysis.md`, OPEN, waiting on Bader. Q20 added to `02_questions.md` with the four unknowns. F23 added to `01_next.md` after F22
- F8 fixed. With no XML, `ClashStep` returned before anything ran, on both the scanned run and the open file run, while the window logged that it was running the tests already in the document
- The clash step now does one of three things and the log names which, in one line shape on both runs: `CLASH    source   tests from XML, ...`, `CLASH    source   tests saved in the document, N of them, no XML picked`, or `CLASH    source   nothing, no XML picked and the document holds no clash test, so nothing ran`. The rule is `ClashWork.SourceFor` in Core
- A second way to build a `ClashTestPlan`, `ClashTestPlan.FromDocument`, in Core. Every saved test goes in by its address, in the order found. No drift compare, nothing created, sets not touched. `SavedClashTest` is the shape it takes, with no Navisworks type on it
- The add-in fills it in `SavedTests.Read`, one walk of `DocumentClashTests` that disposes every wrapper and keeps only the address
- `ClashRunner.Run` and `OneTest` take a document plan through the same path: `ItemsOn` both sides, the SingleModel and EmptySide skips, `TestsRunTest`, the harvest, the images, the report, the `RepeatedFailureGuard`. A document plan skips the set resolution and the zero sets guard, because its sides are already inside the tests
- The XML path is unchanged. An XML holding neither sets nor tests is still nothing, whatever the document holds
- The words fixed to match the log: CLAUDE.md in two places, the help line under Run the open file, the OPEN log line in the window, and the engine docstring
- Eleven Core tests added in `SavedTestPlanTests`
- Core tests under mono on Linux: 820 passed, 40 failed, 32 skipped, 892 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6, F7 and F8 proofs in one session

### What remains

- F22 next, which waits on Bader confirming the two workflow definitions in chat, then F23 once Q20 is answered, then F1 to F4 as one PR
- The F5, F6, F7 and F8 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- L1 fixed in code, pending local proof. Open one NWF that holds tests, pick no XML, press Run the open file, the tests must run and the Excel must be written. Then the scanned run with no XML on a folder whose NWFs already hold tests
- B12 OPEN, waiting on Q20
- B3 to B10, L2 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F8 PR
2. Bader answers Q20 and confirms the two workflow definitions for F22 in chat
3. Bader follows `03_bader_next.md` and drops the logs into `steps/logs`
4. Worker reads the logs and records the four proofs in this file
5. Worker starts F22

## 2026-09-06 F7, the RESULT block and log copy for the open file run

### What was done

- `steps/logs` still holds only its README. No run log yet, so B1 and B2 stay pending local proof and nothing about them was recorded
- F7 fixed. The open file run ended with no GROUP line, no block naming what it did, no RESULT block and no copy of the log. The scanned run has all four
- `FederationEngine.RunOpenDocument` now calls `log.GroupStarted` with the models inside the file, and `log.GroupFinished` in a finally with the same outcome shape the scanned run uses, so `GroupJudgement` gives DONE, PARTIAL or FAILED by the same rule and the RESULT block counts it. The group name is the open file's name from `OpenDocumentJob`
- In the same finally the engine writes a block titled OPEN FILE, built by `OpenDocumentJob.SummaryLines` in Core: the file, the decision, the clash file, the clash outcome, the NWD path, the report folder from F6, and the outcome. Source folder, grouping and files ticked say not applicable rather than being left blank
- `FederatorWindow.OnRunOpenDocument` calls the existing `WriteTheResultAndCopyTheLog` in its finally, handing it the open file's own folder through `OpenDocumentJob.FolderOf`, so a failure still leaves a RESULT block and the copy sits beside the open file. No second copy of that method
- `SourceMismatchFindings` is skipped on the open file run with one log line saying why. Nothing was scanned, so there is no NWC name to compare the Revit source against
- The window's old OPEN DOCUMENT one line block is gone, the engine's OPEN FILE block replaces it
- The scanned path is untouched
- Five Core tests added. Two in `GroupJudgementTests` for the open file case, three in `OpenDocumentJobTests` for the folder and the summary lines
- Core tests under mono on Linux: 809 passed, 40 failed, 32 skipped, 881 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5, F6 and F7 proofs in one session

### What remains

- F8 onward in `01_next.md`, in that order
- The F5, F6 and F7 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B11 fixed in code, pending local proof. Open one NWF, press Run the open file, the log must end with a RESULT block for that file and a copy of the log must sit beside it
- B3 to B10, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F7 PR
2. Bader follows `03_bader_next.md` and drops the three logs into `steps/logs`
3. Worker reads the logs and records the three proofs in this file
4. Worker starts F8

## 2026-09-06 F6, the open file report folder

### What was done

- `steps/logs` holds only its README. No run log yet, so B1 is still pending local proof and nothing about it was recorded
- F6 fixed. The window built `<folder>\Clash Reports` with `OpenDocumentJob.ReportFolderBeside` and handed it to the engine as the NWF folder. The engine then built `Clash Reports` beside that again through `ReportPaths.Choose`, so the reports went to `<folder>\Clash Reports\Clash Reports`
- The decision now lives in one place, `OpenDocumentJob.ReportFolder(openPath, pickedExcelFolder)` in Core. It calls the same `ReportPaths.Choose` the scanned run uses, with the open file's own folder where the scanned run puts the NWF folder. A picked Excel folder wins, the same as the scanned run
- The engine's open file path calls that rule in `RunOpenDocument` and takes no folder from the window. The window's blue line calls the same rule, so what it says and what is written cannot differ
- `ReportFolderBeside` is gone. `RunOpenDocument` and `OpenJob` lost a subfolder parameter nothing read
- The scan's source folder refusal is skipped on the open file run. No source folder is handed to `ReportPaths.Choose` there, because nothing was scanned. The engine still creates the report folder and reads every written file back, so a folder that cannot be written is caught by the write itself
- The scanned path is untouched. The six argument engine constructor now delegates to a new five argument one and then sets the report folder exactly as before
- Five Core tests added in `OpenDocumentJobTests`, one of which pins the old doubling as the trap it was
- Core tests under mono on Linux: 804 passed, 40 failed, 32 skipped, 876 total. The 40 are the same Windows path and file locking failures as before, none new
- The add-in was not compiled. It cannot compile in the container
- `03_bader_next.md` rewritten for the F5 and F6 proofs together

### What remains

- F7 onward in `01_next.md`, in that order
- The F5 and F6 proofs on the local machine, then P1, P2, P3

### Known bugs

- B1 fixed in code, pending local proof. Run one building twice, the second press must show OPENED, the sets present and the tests still run
- B2 fixed in code, pending local proof. Open one NWF, press Run the open file, the reports must land in one Clash Reports folder beside the file, no folder inside a folder
- B3 to B11, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F6 PR
2. Bader follows `03_bader_next.md` and drops the three logs into `steps/logs`
3. Worker reads the logs and records both proofs in this file
4. Worker starts F7

## 2026-09-06 F5, the sets built test, and Bader's answers

### What was done

- Bader answered every question. The answers sit under each question in `02_questions.md`
- `01_next.md` reordered to Bader's order. F19 dropped. F20, F21 and F22 added
- `steps/logs` created with a README for the run logs
- `03_bader_next.md` written, the local steps for the build, the install and the first proof
- F5 fixed. `FederationEngine.BuildTheSets` returned `CreatedCount > AlreadyPresentCount`. It now returns `SetBuildOutcome.PutAnythingIn`, which is `CreatedCount > 0`
- The bool has one meaning. It feeds `ClashStep`, whose only caller uses it to decide the second NWF save. The tests were never gated on it, `ClashStep` runs them either way. So B1 in `00_analysis.md` was corrected: what was lost was the second NWF save, not the tests
- One log line added after the SETS block saying how many sets were put into the document and how many were already there
- Four Core tests added in `SetBuildOutcomeTests` for `PutAnythingIn`
- Core tests under mono on Linux: 800 passed, 40 failed, 32 skipped, 872 total. The 40 are the same Windows path and file locking failures as before, none in the sets tests
- The add-in was not compiled. It cannot compile in the container

### What remains

- F6 onward in `01_next.md`, in that order
- The three proofs P1, P2, P3 on the local machine

### Known bugs

- B1 fixed in code, pending local proof. The proof: run one building twice, the second press must show OPENED, the sets present and the tests still run, then drop the log in `steps/logs`
- B2 to B11, L1 to L8, M1 to M8 still open. See `00_analysis.md`

### What comes next

1. Merge the F5 PR
2. Bader follows `03_bader_next.md` and drops the two logs into `steps/logs`
3. Worker reads the logs and records the proof in this file
4. Worker starts F6

## 2026-09-06 analysis pass

### What was done

- Read every non sample file in the repo, 151 files, in full
- Sample data looked at by head and line counts only, 355 jpg not opened
- Environment checked. No Navisworks in the container
- Installed .NET 8 SDK and mono in the container to run the Core tests
- Ran the Core tests under mono on Linux. 796 passed, 40 failed, 32 skipped
- All 40 failures are Windows path and file locking assumptions, not code faults
- Wrote this folder
- No source code changed

### What remains

- Every fix in `01_next.md`. Nothing is fixed yet
- Every question in `02_questions.md`. Nothing is answered yet
- The three done criteria. None is proved yet. See section 5 of `00_analysis.md`

### Known bugs

Eleven bugs B1 to B11, eight logic problems L1 to L8, eight missing pieces M1 to M8.
All in `00_analysis.md`.

The ones that change what a run produces:

- B1  a group whose sets were all already there is treated as if nothing was built
- B2  the open file path writes reports one folder too deep
- B11 the open file path writes no RESULT block and copies no log
- L1  the open file with no clash XML runs nothing, the log says it runs the tests
- L2  a CHANGED group still has its model units changed

### What comes next

1. Bader answers `02_questions.md`, at least Q1, Q2 and Q6
2. Worker does the fixes in `01_next.md` in order, one branch per fix or one branch for the small ones
3. Bader runs one building on the local machine and sends the log
4. Worker reads the log for OPENED, the clash timing and the Excel counts
