# 04 The audit of 2026-09-12

Run after F38 merged, on the tree that became main at F38 plus the F39 fix it found. Five questions were put to every file under src, tests, build, tools, docs, steps and the root: where does the code contradict CLAUDE.md or a rules file, which public member has no caller, which comment is doubled, which doc line disagrees with the code, and which test cannot run here.

## How it was done

- Fourteen readers, one per area, each reading every file in its area in full and grepping src for every member it questioned. They returned 393 findings, each with a file, a line, the quoted line, the rule it cites and the evidence. Every finding was then handed to a verifier told to refute it. The verifiers refuted four and confirmed the rest, and where they refuted, this file says so
- Three checks of my own that need no judgement. A regex over src for two summary blocks stacked on one member, which found three. A script that counts, for every public or internal member under src, the references to its bare name outside its own file, its own file, and the tests, which every no caller finding below was checked against and then checked again with the type in front of the name. A trx of the Core test run under mono, read back for every failed and skipped test with its first message line
- A script over the add-in that counts the arguments of every `new` and every static call and compares them with the arities declared under src. It found F39 and nothing else. It is a session script, not a repo file, because the local build is the real check

The counts. 393 findings, of which 161 name a member with no caller, 124 a doc line that disagrees with the code, 48 a contradiction with a rule, 43 a test that only runs on Windows, 17 a doubled comment. Many are the same fault seen from two areas, so the lists below are shorter than the counts.

## The one that matters most, fixed

F34 deleted the republish flag and the three engine constructors that took it. The two calls in the window that construct the engine for a run kept passing `true` as their third argument, so neither matched a constructor that exists and the add-in had not compiled since F34 merged. F35, F36 and F37 changed the add-in further without a build. Found by the steps-and-rules reader, confirmed by grep, fixed in F39 and merged. The build is the first step of `03_bader_next.md` and this is why it comes before any proof.

Why the reading missed it. The add-in does not build here, so every add-in change is read twice as a diff, and a diff shows the constructors that went and not the callers that stayed. The compiler puts the two side by side and nothing here did until the audit.

## Contradictions with the rules

The rule is named, then the code. Checked by grep here, not only by the reader.

Settings that are constants. CLAUDE.md says every number that shapes a run is a setting and names the stop after count and the log count.

- `RepeatedFailureGuard.DefaultThreshold` is 50 and the two places that construct the guard, `FederationEngine.cs` line 44 and `ClashRunner.cs` line 68, pass nothing, so no run can change it. The rules file says the 50 is a setting
- `RunLog.DefaultKeepLogs` is 30 and the one add-in caller, `FederatorPlugin.cs` line 46, passes nothing. The verifier refuted this one, saying the shape is the same as the stop after count, a named default threaded through as a parameter. Both are the same shape and neither is reachable from a run, so both stay on the list
- `ClashRunner.ProgressEvery` is a const of 25 that nothing can set
- `ReportPaths.DefaultSubfolder` is a const with a comment above it saying a setting, not a constant, and nothing under src carries it as a property
- `ImageOptions.DefaultStopAfterFailures` repeats the 50 rather than reading `RepeatedFailureGuard.DefaultThreshold`

Framework messages in labels. CLAUDE.md says no code identifier and no framework message ever reaches a label, and a dialog may carry one.

- `RunLog.cs` line 141 builds `DisabledReason` from the exception type name and message, and the window writes it into the progress line at `FederatorWindow.xaml.cs` line 104
- `ClashRunner.cs` line 629 builds the guard reason from the exception type name and message, and line 454 hands it to the progress label
- `FederationEngine.Describe` appends every outcome error, type names included, and the window writes it into `SetsSummary` at line 1604 for the open file run
- `FederatorWindow.Describe(path)` returns `error.Message` at line 1502 and line 1481 writes it into the same label

One rule in two places. CLAUDE.md says a copy of a rule in a second file is a bug.

- `ScanFindings.cs` line 357 decides single discipline with `group.Disciplines.Count == 1` instead of reading `BuildingGroup.IsSingleDiscipline`, which F35 made the one rule
- `NwfComparison.Moves` and `NwfRebuildPlan.From` both work out that a removed leaf reappearing in the added list is a move, and `LeafOf` is the same method in both files. The engine reads only the plan's
- `ImageNaming.LogoName` and `InstallFiles.LogoName` both say logo.jpg. The verifier refuted this as two measured facts, the name Navisworks ships and the name the report folder carries. They are the same file copied, so it stays on the list as a copy to read from one place
- `RepoRoot`, the walk up to the checkout, is written in `ClientReportColumnsTests`, `MatchOriginalTests`, `ReportLogoTests` and `ClientLayoutTests` when `Samples.Folder` already holds it
- The default photo size 1024 and the default status ticks are typed into the XAML at lines 307 and 318 to 326 while `ImageOptions` holds both in Core

Handles the tool creates or resolves and does not dispose. The addin rules file lists every type.

- `SetBuilder.cs`: the `Search` at line 99, the `SelectionSet` at line 108, the `ModelItemCollection` at lines 283, 291 and 296, every `SavedItem` read in `Describe` at line 221, and every child read and not returned in `FindFolder` at line 246 and `FindSelectionSet` at line 264
- `SavedTests.cs` lines 69 to 73 read `SelectionA` and `SelectionB` four times per test and hold none of them
- `ClashRunner.cs`: the root `FolderItem` at line 1160, which `FederationEngine.CountSets` does dispose, the intermediate `GroupItem` at line 929 when an address is deeper than one level, and every `SelectionA`, `SelectionB` and `Selection` read at lines 523, 524, 700 to 706, 726, 978, 982 and 1024
- `SetBuilder.FindSelectionSet` walks the whole child collection after every `AddCopy`, which is the once per item walk the rule forbids and the shape that was O(n squared) over 1830 tests. The set is at the index the count held before the add, checked by name, as `ClashRunner` does for tests

Rules the clash step breaks.

- `ExchangeReader.ReadDouble` returns 0.0 for a test with no tolerance attribute, so a missing tolerance becomes a zero tolerance that reads as real. The rule says everything comes from the file and never from a constant
- `ClashRunner.cs` lines 825 to 828 put a sentence on `ClashTest.Status.Old`, saying it means something changed after the test ran. The rule says what puts a test into Old is UNKNOWN and no sentence is put on it
- `ClashRunner.cs` line 409 reports the Resolved count read before compacting as the number removed. Nothing counts after. The rule says the count removed is reported after
- `ClashRunner.cs` line 758 says a side whose locator could not be read is never compared, and `TestDrift.Compare` compares the string UNKNOWN like any other and reports it as a difference

Rules the docs promise and the code does not keep.

- The rules file says which property supplied the id goes in the log. `ClashItem.IdFrom` is set at `ClashHarvest.cs` line 317 and read by nothing
- The rules file says the five extra item columns go on the clash XML. `ClashReportXml.Item` writes one objectattribute and two smarttags, so family, type name, material, source file and discipline are harvested and written nowhere. Q25
- The rules file says a cell can be given back to the pattern. `OutputNameRow.ReleaseToPattern` is called by nothing and no control in the window reaches it. Q24
- The Clash step heading in the XAML, line 426, says a run with no XML does the model side only. The engine runs the tests saved in each NWF, which the rules file says and `ClashWork.SourceFor` does. `ClashWork.cs` line 19 says the same wrong thing in its class comment
- `BundleAssemblies.MissingFrom` and the check in `install.ps1` lines 127 to 182 are the same walk written twice, and nothing under src calls the first

Names from one project in src comments. CLAUDE.md says nothing in the code names any one project's file.

- `ExchangeModel.cs` line 25 names a set locator from the reference file, line 254 a set name from the Infra export, and `ModelFileNames.cs` line 12 names the KSA_New Murabba Revit file. Each is an example that could be made up

## Members with no caller

Checked here twice, first by bare name across src outside the declaring file, then with the type in front of it. The tests are not a caller. The rule is that a public member nothing in src calls is deleted with its tests unless a decision keeps it.

Declared and referenced by nothing under src, not even their own file, 43:

- Clash: `ClashRunOutcome.AlreadyPresent`, `RepeatedFailureGuard.Seen`, `RepeatedFailureGuard.Threshold`
- Diagnostics: `BuildStamp.LooksStamped`, `BundleAssemblies.MissingFrom`, `FolderMemory.IsRemembering`
- Exchange: `ExchangeReader.ReadFiles`, `ExchangeReader.ReadText`, `ExchangeDocument.DistinctTestLocators`
- Grouping: `BuildingGroupingResult.FindSkipped`
- Naming: `OutputNameRow.ReleaseToPattern`
- Report: `ClashReport.DisciplineOf`, `ClashReport.SetNameOf`, `ClashReport.TotalRawClashes`, `ClashReport.TotalResolved`, `TestReport.DescribeState`, `TestReport.NewPlusActive`, `TestReport.OpenUnder`, `ClashRow.Position`, `ClashReportXml.Explain` with `ShapeReadFrom`, `Filled` and `LeftOut` that feed only it, `ClientFormat.DistanceFormat`, `ClientShapes.CoordinatesIn`, `ImageNaming.TryRead`, `ImageOptions.Statuses`, `ImageTally.SkippedByCap`, `ImageTally.SkippedByStatus`, `ImageTally.Slowest`, `ImageTally.TotalBytes`, `OutputPlan.ReportSkipReason`, `PageCheck.ExtraColumns`, `PageCheck.FirstProblem`, `PageCheck.HeaderColumns`, `PageCheck.MissingPictures`, `ReportOrder.PictureNumberFor`, `SheetNames.IsAcceptable`, `WorkbookWriter.ClientColumns`, `WorkbookWriter.Options`, `WorkbookWriter.SheetNamesFor`
- Sets: `SetBuildPlan.TotalSets`, `SetBuildPlan.FolderPaths`, `SetBuildPlan.DeepestFolderDepth`
- Add-in: `ClashRunner.Drift`, `JobOutcome.ReportWarnings`, which means the report warnings `AddReportWarning` collects reach neither the log nor the window through it

Referenced under src only with the type in front of another member of the same name, so uncalled once the type is checked, 34:

- `ClashRunOutcome.Ran`, `ClashTestPlan.SkipReasonCounts`, `OpenClashes.All`, `OpenClashes.Default`, `OpenClashes.Describe`, `BundleAssemblies.Resolved`, `RunLog.Start()` with no arguments, `RunLog.StartOrDisabled` with two arguments, `RunLog.GroupFinished` with three and with four arguments, `ExchangeDocument.BatchTests`, which only the dead `ReadFiles` reads, `HealthCheck.Run` with two documents, `ScanFindings.OfKind`, `ScanFindings.Count`, `SourceMismatchFindings.OfKind`, `SourceMismatchFindings.Count`, `SourceMismatchFindings.From` with one argument, `BuildingGroupingResult.GroupCount`, `BuildingGrouping.Group` with one and with two arguments, `BuildingGrouping.GroupNames` both overloads, `ContainerName.Parse` with one argument, `ContainerNameSettings.Copy`, `OutputNaming.All`, `OutputNaming.Copy` and with it `NamePattern.Copy`, `OutputNameTable.Rows`, `OutputNameTable.Build` with three arguments, `NamePattern.NameFor` with five strings, `TestReport.GroupCount`, `ClashReportXml.QuickProperties`, `ImageTally.Written`, `PageCheck.Problems`, `WorkbookCheck.Problems`, `ImageRenumberingOutcome.Problems`, `ReportOptions.FolderFor`, `ReportOrder.Rows`, `ReportPaths.Folder`, `GroupFacts.Errors`, `NwfComparison.Matches`, `NwfComparison.Moved`, `NwfRebuildPlan.Files`, `SetBuildOutcome.Skipped`, `UnitTable.All`, `ClashHarvest.Into` with three arguments, `GroupRow.Names`, which no XAML binding names either

Public and used only inside their own file, so public for nothing, 9: `SelectionSetDefinition.Guid`, `FindSpecMode`, `FindSpecLocator`, `Disjoint`, `ClashTestDefinition.LinkageMode`, `ClashTestDefinition.Rules`, `ParsedContainerName.Parts`, `PlannedCondition.ValueType`, `SheetNames.Sanitise` with one argument.

Two need a decision before they go, Q24 for `ReleaseToPattern` and Q25 for the five item properties. `LinkageMode` and `Rules` are read off the file because the rules file says to read them for another project, so they stay and go private. The rest go with their tests in F40.

## Doubled and copied comments

- Three stacked summaries, two on one member: `ReportPaths.cs` lines 6 to 10, where the first block describes `ReportPaths` and sits on `ReportFolderChoice`, `PageCheck.cs` lines 352 to 356, where the first block describes `FirstCell` and sits on `Cell`, and `SetBuilder.cs` lines 275 to 280, where the first block describes `Resolve` and sits on `CountOf`
- The same sentence on two members: `GroupJudgement.cs` line 22 and `JobOutcome.cs` line 135, `ClashHarvest.Images` and `ClashRunner.Images`, `ContainerNameSettings.cs` lines 30 and 33 against `ParsedContainerName.cs` lines 46 and 49, where the settings hold positions and the sentence is about values
- A rule sentence copied word for word out of the rules file into a code comment: `ClashTestKind.cs` line 16, `ClashSkipReason.cs` line 33, `ClashTestPlan.cs` line 269, `TestDrift.cs` line 169
- A comment restating the constant it uses: `WorkbookWriter.cs` lines 155 and 209
- A comment restating the test name under it: `ClashSkipSummaryTests.cs` line 38, `RepeatedFailureGuardTests.cs` line 64
- One sentence, a leftover temp folder is not worth failing a test over, above the same catch block in nineteen test files. One shared helper with the comment written once

## Doc lines that disagree with the code

Paths that moved in F37 and were not followed:

- `docs\scan.md` in seventeen source and test files, `build\install.ps1` line 59, `Federator.Core.csproj` line 17, `PackageContents.xml` line 9, and `build\probe-window-scroll.ps1` in `FederatorWindow.xaml` line 198. `CLAUDE.md` named as the place of a measurement in `ClashTestPlan.cs` line 19, `ClashTestPlanTests.cs` line 68 and `UnitTableTests.cs` line 458, where the rules file now holds it

Counts that are wrong:

- Thirteen columns where the code writes fifteen, Layer being a fourth per item: `ClientLayout.cs` line 42, `WorkbookWriter.cs` lines 187 and 356, `ClientFormatTests.cs` line 394, and in the rules file at lines 340 and 356 to 359, where one bullet says the client's report has no Layer column and a later one says the Layer column carries the level
- Five pickers where there are six since the logo picker: `PickerStart.cs` lines 9 and 13, `PickerStartTests.cs` line 13, the rules file line 190, and the other four comment in `OnBrowseExchangeFile`
- Three rerun cases where the enum has four: `GroupJudgement.cs` line 22
- Eight faults where `WorkbookCellCheckTests` holds twelve, twenty one picture names where the test lists eight

The rules file against itself, in core.md:

- Line 373 says Distance carries a three decimal number format and line 572 says it is the rounded number with no format. The code does the second
- Line 345 says the Item ID label is read off the property and line 300 says the tool chooses Element ID. The code does the second, and `IdFrom` is never logged
- Line 418 says the workbook check reports per column how many rows filled it. `WorkbookCheck` reports no such count. Line 595 describes what it does
- Line 457 says the five extra properties are appended after theirs when the client layout is not asked for. There is no such choice any more and they are written nowhere
- Line 525 says the picture cost figures go on the Summary sheet. There is no Summary sheet
- Line 255 says since F24 no group ends as left alone, and `RunPath.cs` line 38 and `NwfComparison.cs` line 20 say the same. A group whose rebuild does not finish still ends as Changed with that label, which `docs/workflow.md` says correctly
- Line 126 names `HealthCheck.DistinctRuleCount` and the member is on `HealthCheckResult`
- Line 402 names 1A04WE and 1A02WE, which are exports measured on Bader's machine and recorded in `docs\history`, while the committed samples are 1A02WN and 1A04WN. The verifier refuted the reader here, and rightly: the sentence records a measurement and is not wrong, it is a sentence that names a file the reader cannot open without saying so. The same in `ClientStyle.cs` line 8. `MatchClientReportTests.cs` lines 18 and 19 and `HtmlTabularTests.cs` lines 92 and 306 name the same two, and each should say which is the committed sample and which the measured one. `MatchColumnsTests.cs` line 17 names a log under `docs\logs` that never existed

Comments that describe a workbook that is gone. The Summary sheet, the Matrix sheet and the sheet per test went, and these did not:

- `ClashReportModel.cs` lines 252, 279, 282, 415, 600, where `HasSheet` is a member name that means has rows, `OpenClashes.cs` line 6 on a matrix cell, `PlannedClashTest.cs` line 58, `SingleDisciplineGroupTests.cs` lines 170 and 178, `OpenClashesTests.cs` lines 12, 13 and 126, which say the window offers the choice D3 removed

Comments that say the logo is not copied, when the engine copies it every run: `ClashReportXml.cs` line 73, `ReportOptions.cs` line 66, `ImageNaming.cs` line 16.

Comments that say the five extra properties are workbook columns, when the workbook carries the client's fifteen and nothing of ours: `ClashReportModel.cs` line 10, `ClashReportXml.cs` line 270 and its `Windows` summary, `ClashReportXmlTests.cs` line 245, `MatchColumnsTests.cs` line 178, `HtmlTabularTests.cs` line 322.

Other lines:

- `FederationEngine.cs` line 1001 says `WriteNwf` only runs when there was no NWF there, and `RebuildFromScan` calls it with one on disk
- `DocumentUnits.cs` line 46 says the window sets the units step on, and nothing in the window touches it
- `ClashImages.cs` line 78 says fifty renders and the number is `StopAfterFailures`
- `FindingKind.cs` line 16 says one character apart and the rule is one confusable character. `ScanFinding.cs` lines 55 and 58 undercount which findings carry two codes and which carry files. `BuildingGroup.cs` line 35 leaves out the Everything mode
- `ClientLayout.cs` line 173 describes a boolean on a string constant. `ClientReportXml.cs` line 32 lists what is filled and stops short of four elements the code writes
- `FederatorWindow.xaml.cs` line 1550 puts the Run button on the Source step and it is in the bottom bar. `FederatorWindow.xaml` line 302 says neither report embeds photos under the box that embeds them
- `SetBuilder.cs` line 304 says CLAUDE.md asks for two options no rule names
- `README.md` line 13 names CLAUDE.md as the one place the rules live, and since F38 there are two. `docs/workflow.md` line 64 names the hand buttons Build sets and Run tests, and they read Sets into open model and Tests into open model
- `tools/probes/README.md` lines 33 and 34 say every probe tests its path and says UNKNOWN, and `probe-units.ps1` and the three window probes do not
- `CLAUDE.md` line 36 says one test folder per Core folder, and the `Health` tests sit under `Exchange`. Line 43 says the bundle folder is written by the build, and it is a hand written manifest the installer copies, with `artifacts` being what the build writes
- `steps/03_bader_next.md`, four steps: the install prints the count of expected files present and every reference satisfied, never the words the bundle is complete. The first units entry reads Metres, which is what the models are set to. The confirm dialog for a weekly run ends with Nothing is cleared, so the word is on it. And the NWF is saved again whenever the clash step put anything into the document, which running a test does, so a second run with the XML still writes a second NWF attempt line. Fixed in this file's own round

## Tests that cannot run here


Measured on 2026-09-12 with `dotnet test` under mono on Linux, logged to a trx and read back: 905 passed, 37 failed, 33 skipped, 975 total. The same 37 and 33 every run since F32. On the Windows runner in Actions all 975 pass or skip and none fails, so every one of the 37 is a test written for Windows paths, not a fault in Core.

The 37 that fail here, by cause:

- Windows paths in the expectation, 28. A drive letter or a backslash typed into the expected string, so the answer built with `Path.Combine` on Linux differs from index 10 or 14 or 28, which is where the separator sits. `ADriveThatIsNotThereFallsAllTheWayBackToNothing`, `AFolderThatOnlySharesAPrefixIsNotInsideIt`, `AFolderUnderTheSourceFolderIsRefusedToo`, `AnEmptyPickGoesBesideTheNwfFolderInClashReports`, `AnNwfFolderInsideTheSourceFolderIsRefusedRatherThanUsed`, `APickedFolderInsideTheSourceFolderIsRefusedAndSaysWhy`, `ImagesSitsAtTheTopOfTheInstallAndIsTriedFirst`, `ItNamesEveryPathItLookedAtWhenItCannotFindOne`, `ItSaysEveryPathItLookedAtWhenItCannotFindOne`, `TheDefaultLandsBesideTheNwfFolderAndNotInTheSourceFolder`, `TheExtensionOnTheWorkbookMakesNoDifference`, `TheFolderIsTheReportNameWithFilesOnTheEnd`, `TheLanguageTheApplicationReportsIsTriedFirst`, `TheLinkIsRelativeSoThePairCanBeMoved`, `TheNameIsReadOffTheOpenFile`, `TheNwdPathIsTheNameWithAnNwdExtension`, `TheNwdSitsBesideItWithTheExtensionSwapped`, `TheNwfPathIsTheNameWithAnNwfExtension`, `TheOptionsCarryTheSourceFolderThroughToTheChoice`, `TheOptionsWorkOutTheSameFolderThePathsDo`, `TheOutputsMatchWhatTheScannedRunWouldWriteForTheSameName`, `ThePathIsTheFolderAndTheName`, `TheReportGoesInTheSameSubfolderTheScannedRunUses`, `TheWorkbookIsNamedLikeTheNwfAndTheNwd`, `TheWorkbookLinkIsStillAForwardSlashUri`, `TheXmlSitsBesideItUnderTheSameName`, `AFormatThatWouldMakeAnIllegalFileNameFallsBackToTheNumber` (a colon is legal in a Linux file name), `TheOnlyLogosInTheCheckoutAreInsideTheSuppliedClientExports` (asserts the path contains `samples\` with a backslash)
- Windows file system rules, 6. Case blind paths and share locking are Windows behaviour and Linux has neither. `APathThatDiffersOnlyInCaseStillMatches`, `CaseAndTrailingSlashesDoNotChangeTheAnswer`, `ALockedFileLogsTheReasonAndDoesNotThrow`, `APlainFileReadCannotOpenTheLogWhileTheRunIsStillWriting`, `AnUnwritableLocationIsRecordedRatherThanThrown`, `ABadOutputFolderStillLeavesTheLogInTheFixedPath`
- A ReadOnlyCollection compared against an array with `Is.EqualTo`, 3. `AFileAddedSinceLastTimeIsReportedAndNothingIsTouched`, `AFileRemovedSinceLastTimeIsReported`, `OneFileAddedAndOneRemovedAtTheSameTimeNamesBoth`. This one is UNKNOWN as a Windows against Linux difference, because NUnit compares collections by element on both. It may be a mono difference. It passes on the Windows runner

The 33 that skip here, by reason:

- 26 say Navisworks is not on this machine, 24 because its stylesheet cannot be read and 2 because its logo cannot. Right, and they say so
- 1, `AUncPathIsAFolderLikeAnyOther`, says a UNC path is only a path on Windows. Right, F32 wrote it that way
- 6 say the supplied client exports or the exchange file are not in this checkout, and they ARE. `MatchOriginalTests.ExchangeOrder` joins `samples\1104-PAR_CLASH_AllInOne (2) (1).xml` with a backslash and `ClientReportColumnsTests.Samples` joins `samples\client-report` with a backslash, so on Linux neither is found under that name and the skip message says the checkout is missing something it holds. `EverySuppliedExportAgreesWithTheOthers`, `NoCoordinateInTheirFilesIsEverZeroPointZeroZeroZero`, `TheSetMatchesEverySuppliedClientExport` and `TheirReportsArePutTheMostClashesFirst` from the first, `OurOrderReproducesTheirsBlockForBlock` and `TheirTieRuleIsTheOrderTheTestsWereCreatedInAndNotTheName` from the second. A skip that misnames its reason is a finding, because it reads as a missing sample

All of the above is F16, make the tests path neutral, which is next in the order. The three collection comparisons and the six misnamed skips are added to it below.

## What the verifiers refuted

Seven of 393 by the time this was committed, and each is answered above: the log count stays on the settings list because the shape it shares with the stop after count is the shape of the fault, the PickerStart line is 9 and not 8, the two `LogoName` constants are the same file copied and stay a copy, the `ClashReportXml` finding about Found being in the workbook is at the `Windows` summary rather than line 260, the 1A04WE and 1A02WE names are measurements recorded in `docs\history` and not wrong names, so F44 makes them say so rather than replacing them, and the `NwfComparison` class comment about a move called out on its own describes the Moved list the class does carry, so that finding is dropped and only the copy of the rule stays. The verifier pass was still running when this was committed, 27 of about 50 batches done, and a later refutation is the next session's to read off the journal and add here.

## The fix list

Every finding above is in one of these, and each is a section in `01_next.md` with its files.

- F39, done and merged. The window compiles again
- F40. Dead members out, second pass: the 43, the 34 and the 9 above, with their tests, less the two that wait on Q24 and Q25. The copies: `NwfComparison.Moves` and `LeafOf`, `RepoRoot` in four test files, `ImageOptions.DefaultStopAfterFailures` reading the guard, `ScanFindings` reading `IsSingleDiscipline`, one `LogoName`
- F41. Every handle disposed in `SetBuilder`, `SavedTests` and `ClashRunner`, and `FindSelectionSet` reading the index the count held
- F42. No framework message in a label: `RunLog.DisabledReason`, the guard reason, `FederationEngine.Describe`, the window's `Describe(path)`. The Clash step heading and `ClashWork`'s comment saying what a run with no XML does. The photo size and the status ticks read off `ImageOptions` rather than typed into the XAML
- F43. Four settings that are constants: the stop after count, the log count, the progress interval, the report subfolder
- F44. Every doc line above, the doubled comments, the three project names in comments, `HasSheet` renamed, the rules file made to agree with itself, the Health tests into a Health folder, the probes README made true or the probes made to match it
- F45. The clash step: a test with no tolerance skipped by name, Old logged as itself, the compacted count read after, an UNKNOWN locator left out of the drift comparison, `IdFrom` logged or dropped with Q25
- F16, widened. Every windows-only test above, the six misnamed skips, the three collection comparisons, the one test that passes off Windows without proving anything, `Samples.Folder` used everywhere, one temp folder helper

## Questions

- Q24. A cell can be given back to the pattern is a rule with no way to do it in the window. Wire a way, or drop the sentence and the member
- Q25. Family, type name, material, source file and discipline are harvested per item and written to no output since the workbook became the client's one sheet. Delete them and their harvest, or keep them for a workbook column that does not exist yet
