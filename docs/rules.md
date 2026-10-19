# Bader's rules for the tool

The one list of Bader's rules for what the NWC Federator does. One line a rule, grouped by area. Each line names the question or
order it comes from, the newest where two disagree, and the Core test that proves it as Class.Method under
tests\Federator.Core.Tests. Test, in part, names a test that proves the Core half or one case of the rule, the rest resting on a
run. NO TEST means no Core test reaches it, because it lives in the add-in or outside the code, and every NO TEST rule is in Q224
of steps\02_questions.md. The rules of the repo, the loop and the rounds are in CLAUDE.md and .claude\rules, and the code rules
that carry these out are in .claude\rules. Written by F239 on 19 Oct 2026 from the answered questions of steps\02_questions.md
and the chat notes Bader gave with his round of 19 Oct b, the day each note was written not recorded. Where a chat note and an
answer disagree, the line says CONFLICT and the form at the end of the round asks which holds.

## Models and append

- MA1. A model is appended the way a manual Append does it, and the tool sets no units, origin, transform, file option or Navisworks option before or during an append. Q202. Test, in part: PlacementCheckTests.TheAddInSetsNoModelsUnitsOrTransform
- MA2. Plain Navisworks Manage 2025 only, never the BIM360 one. Chat notes. NO TEST
- MA3. A model is far when it sits more than 30 m from the reference discipline, measured as a straight line, and 30 m is a setting. Q202. Test: DisciplineChecksTests.FarIsMoreThanTheSettingSoExactly30MetresIsNoRow
- MA4. Models are compared by their shared coordinates, never by a bounding box. Q64. Test: AlignmentCheckTests.ItNamesTheDifferentSharedCoordinatesAndTheModelsOnRevitsInternalOrigin
- MA5. A model on Internal or naming no shared site is a NOTICE in the run log, the shared coordinates txt and the group's result line, never a reason for FAILED, and the clash always runs. Q170, Q172, his order of 14 Oct. Test: AlignmentCheckTests.AModelNamingNoSharedSiteIsANoticeThatNamesItAndFailsNoGroup
- MA6. A shared site named DEFAULT in any spelling is reported as the model carries it and fails nothing. Q78. NO TEST
- MA7. A bad NWC never stops an unattended run: its message box is answered with its one button and its group ends FAILED with the file named. His message of 18 Oct, F213. Test, in part: AppendBoxTests.ItIsAnsweredWithTheIdOfItsOneButtonAndNeverIdOk
- MA8. An NWF whose file list matches is opened and never cleared, and one whose list differs is reshaped from the folder, keeping its sets, tests, results, statuses and viewpoints, counted out and back, and saved over only when all came back. Q22, Q34, Q77. Test: RebuildTallyTests.EverythingBackOnAllFourIsTheOnlyWayTheNwfIsSavedOver
- MA9. Bader is on the designer side and the models are LOD 300. Chat notes. NO TEST
- MA10. A model more than 1 m from its group's reference, measured as a straight line, 1 m a setting, is named under a NOTICE that its clash results may be wrong, and the clash always runs. Q99, Q100, his order of 14 Oct, Q170. Test: OffCoordinatesTests.TheNoticeNamesTheGroupTheReferenceTheModelsAndSaysTheResultsMayBeWrong

## Grouping

- GR1. Per building is the default of the four ways of gathering, on the full six character building code. The brief, CLAUDE.md. Test: GroupingModeTests.PerBuildingIsTheDefault
- GR2. A group of one discipline is not built on either page: nothing appended and no NWF, NWD, Excel, Internal Excel, HTML, pictures or viewpoints, and an NWF an earlier run left is named and never deleted. His round of 19 Oct, fix 3, Q203. Test, in part: OneDisciplineGroupsTests.AnEarlierNwfIsNamedAndNeverDeletedOrRun
- GR3. Every ticked group of two disciplines or more is federated and clashed, with no hidden discipline rule. Q21, GR2 for a group of one. Test: SingleDisciplineGroupTests.AGroupWithTwoFilesOfOneDisciplineKnowsItCannotClashEither
- GR4. A Shift click on a Run box ticks or unticks the range from the box clicked before. Q112 request 5. Test: ShiftRangeTests.AShiftClickDownGivesEveryRowFromTheAnchorToTheClickTheAnchorsState

## Clash tests

- CT1. The clash matrix, the priorities and the by design pairs are built into the tool as one matrix pack per project, which Bader supplies, and no code names a project's file. Q162-4, Q162-5. Test, in part: MatrixPackTests.PacksAreFoundByTheirResourceNamesAndTwoAreNotChosenBetween
- CT2. A run chooses its pack by the project code in the NWC file names, and no match, or two, stops the run before anything is opened. Q164 B, carried by F166, his words not recorded under it. Test: PackChoiceTests.ACodeWithNoPackStopsAndSaysWhatToSend
- CT3. Only tests across two different set codes run, on both pages, and each group logs how many the rule left out. Q209. Test: SetCodeRuleTests.ATestIsLeftOutOnlyWhereBothSidesReadOneCode
- CT4. A test saved in the NWF that the matrix does not name runs too, where it sits, after the tests of the matrix. Q168 A, carried by F168, his words not recorded under it. Test: SavedTestsTheMatrixDoesNotNameTests.ASavedTestNotInTheMatrixIsAddedAfterTheMatrixTestsAndRunsWhereItSits
- CT5. A test whose side finds nothing is not created, and the Coverage sheet names it with why. Q46. Test: CreationPlanTests.ATestWhoseSideFindsNothingIsNotCreatedAtAll
- CT6. Both tests of a mirrored pair, sets swapped or of one rule list, are created and run, the mirror keeping its own name with (mirror) at the end, and where CT7 merges them each clash is counted once, under the kept test. Q133 D, Q121 B, Q136 A, chat notes. Test: MirrorRuleTests.TheCorrectedMatrixHolds59PairsByRuleListAndEveryTestStillRuns
- CT7. A mirror's clashes merge only where this run made both tests and their sets from the matrix, and otherwise each keeps its own and the log says a clash may be counted twice. Q142 A, Q137 A. Test: MirrorMergedWhereTests.BothTestsCreatedFromTheXmlOverSetsBuiltFromItAreMerged
- CT8. A clash both tests find shows Reviewed or Approved over the rest, and otherwise New or Active over Resolved. Q138 B. Test: BothFoundStatusTests.AStatusAPersonSetsWinsOverEveryOtherWhicheverTestCarriesIt
- CT9. HV, PL, FP, ME, DR and FF are one team, Mechanical, EL and EV Electrical, AR Architecture, ST Structure, and any other code a team of its own. Q114 point 1, chat notes. Test: ProjectTeamMapTests.TheProjectsMapReadsExactlyToBadersMapInHisOrder
- CT10. A set name with no code takes the team its folder in the set tree names, and UNKNOWN where none does. Q117. Test: TeamMapTests.ASetWithNoCodeTakesTheTeamItsFolderNamesAndUnknownWhereNone
- CT11. The team map is read for the views, and the log, COVERAGE and the form write the team beside the code, while the grouping, the checks and the workbook keep the code. Q116 A. Test, in part: CodeOfTests.ASetNameGivesTheFirstKnownCodeAndItsTeam
- CT12. The matrix is corrected by its list of corrections, kept as the project's data in its pack and never in code, before any set is built. Q104, Q113 B, Q162-5. Test: MatrixCorrectionsTests.ARenameChangesEveryPlaceTheNameAppears
- CT13. A workset the buildings spell two ways is asked in every measured spelling, and the export check still names which model carries which. Q102, Q69. Test: MatrixCorrectionsTests.TheWorksetValuesTheBuildingsSpellTwoWaysAreAskedInBothAndTheRestAsBefore
- CT14. Every AR set whose category another discipline also uses asks Source File contains -AR-, Ramps, Railings, Furniture and Site among them. Q103, Q113 D. Test: MatrixCorrectionsTests.ASetBesideTheOnesAskingSourceFileAsksItWhereAnotherFolderAsksItsCategory
- CT15. Clash sets never depend on worksets. Chat notes. CONFLICT: the corrections of Q68, Q102 and Q103 make the matrix ask worksets, and the code follows them. NO TEST
- CT16. A set already in the NWF that differs from the matrix is rebuilt only with its box ticked, off by default. Q72. Test: SetDriftTests.TheBoxIsOffByDefaultBecauseItChangesTheNwf
- CT17. Of two twin sets the unused one is removed and the working one renamed into its name. Q74. Test: SetLeftoversTests.TheWorkingHalfOfAPairIsRenamedAndTheUnusedHalfIsRemoved
- CT18. After each group every test of the matrix is set against what happened to it, with a reason, on a COVERAGE block and the Coverage sheet, and a count in Clash Detective that differs from the workbook is a FAILED line in COVERAGE and RESULT while the group keeps its own result. Q112 request 2 and its notes, Q127 A. Test: CoverageAcrossTheRunTests.ACountThatDiffersIsAFailedLineInResultAndTheGroupKeepsItsOwnResult
- CT19. The Generic Models of each model are counted in the log and on a sheet, with one search set a model in a Generic Models folder, and no clash test is made for them. Q112 request 3. Test, in part: GenericModelsReportTests.EachModelIsCountedOffTheResultOfItsOwnSet
- CT20. With the by design box ticked the pass also reaches the tests saved in the NWF, and its block says so. Q144 B. Test: ByDesignRuleTests.TheBlockSaysWhenTheSidesWereReadOffTheTestsSavedInTheNwf
- CT21. A test a rule now leaves out that an NWF still holds keeps its results, is not compared and is never FAILED. Q209, Q212 open. Test: LeftOutByARuleTests.ATestOfOneSetCodeAnNwfStillHoldsIsNotComparedAndNeverFailed
- CT22. Compact resolved stays removed, and the tool calls no compact and no reset of results. Q178. NO TEST

## Tolerance

- TO1. The tolerance comes only from the tool's drop down, 25, 50 and 75 mm, with 25 selected when the window opens, and it is set on every test, beating the matrix and the document. Q162-5, Q167 A, chat notes. Test: ToleranceChoiceTests.TheDropDownOffersTwentyFiveFiftyAndSeventyFiveAndNothingElse
- TO2. 25 mm is the project's standard from the DD stage. Q66. Test: ToleranceChoiceTests.TwentyFiveIsTheDefaultAndTheFirstEntry
- TO3. A test with no tolerance in its matrix runs at the chosen value like any other. Q167 A, carried by F167, his words not recorded under it. Test: ClashTestPlanTests.ATestWithNoToleranceAttributeIsPlannedLikeAnyOther

## Pictures

- PI1. Only a test of more than 750 clashes is capped, at its first 150 clashes for priority A, 100 for B and 50 for C, and a test of 750 or fewer gets a picture for every clash, 750 a setting. Q187, replacing Q171, Q176 and Q180. Test: PictureLimitsTests.ATestOf750ClashesOrFewerGetsAPictureForEveryClashAndOneOf751GetsItsCap
- PI2. A saved test the matrix does not name, and a test with no priority read, is capped as C, and the cap per test of the options keeps its job, the least of the two holding. Q171, Q175, Q180, kept by Q187. Test: PictureLimitsTests.ATestWithNoPriorityReadAtAllIsCappedAsCAndNeverAsTheMost
- PI3. The pictures stop at 9000 GDI objects or 12 GB of private memory, whichever fires first, both settings. Q175 C, Q179, Q210. Test: PictureLimitsTests.TheFiveLimitsAreSettingsOfTheRunWithBadersNumbersAsDefaults
- PI4. The pictures come after the NWF is saved and the NWD published. Q175 C. Test: GroupOrderTests.ThePicturesEndedBeforeTheNwdIsNamedAsAFaultAndTheRightOrderIsNot
- PI5. Only the models of the test are shown for its pictures, at the export's square size and style. Q150. Test: PictureModelsTests.OnlyTheModelsTheSidesLiveInStayShown
- PI6. The tool sets Clash Detective's Hide Other for its pictures and puts the user's value back after, the ghosted look one setting off by default. Q151, Q161 B. Test: OtherItemsInPicturesTests.ReadOffItIsSetOnForThePicturesReadBackAndPutBack
- PI7. A picture that shows only the sky is taken again through the active view, from the clash's camera moved back to 30 ft, its items red and green and the other models hidden. Q208, chat notes. Test, in part: PictureRetakeTests.TheCameraMovesBackAlongItsLineOnlyWhereItStandsCloser
- PI8. The pictures are numbered in the Navisworks export order. Q8. Test: PictureWalkTests.TheTestsAreWalkedInTheReportsOrderAndNotTheOrderTheyRan
- PI9. Navisworks' texture cache under %TEMP%\ogs is cleared between tests once it passes 5 GB, its size said before and after. Q152 item 2. Test: TextureCacheTests.OnlyASizeReadAndPastTheLimitClears
- PI10. Every picture is disposed, the camera moves by jump cut and the undo stays off for the pictures. Q152 item 2. NO TEST
- PI11. A picture is 1024 by 1024, the size of the accepted report's pictures. Q150, Q188 open. Test: ImageOptionsTests.TheSizeIsTheOneTheClientAlreadyHas
- PI12. The Excel's pictures are linked pictures in the form of the client's own file, never embedded, with the cell link kept. Q162-2, chat notes. Test: LinkedPicturesTests.OneLinkedAnchorPerClashRowWithAnImageAndEveryTargetAJpgTheRunWrote
- PI13. Each picture also carries a click link to its jpg, relative and forward slashed. Q163 A, carried by F169, his words not recorded under it. Test: LinkedPicturesTests.EveryPictureHasAClickLinkToItsJpgRelativeAndForwardSlashed

## Viewpoints

- VP1. One tool folder, Clash Status, carrying the tool's marker, then A, B and C by the test's priority, a test with none in C, then the discipline folder, then one viewpoint per test. Q174-1, Q183, chat notes. Test: ToolViewFolderTests.TheLettersAreAThroughCAndANoPriorityTestIsC
- VP2. The team pair level is removed. Q174-1. CONFLICT: the chat notes say never mix teams in one folder, which a pair folder kept and a discipline folder of Q183 does not, since Mechanical holds a test of ME against ST. Test: TestViewPlanTests.EveryOpenClashOfATestIsInItsOneViewWhateverItsSizeAndPair
- VP3. A test's discipline folder is the code of its first side, AR Architecture, ST Structure, ME, MEP, PL, FP and FF Mechanical, EL Electrical, another code by its team line where that is one of the four, and anything else, a saved test the pack does not name among them, Other. Q183, Q184 open. Test: DisciplineFoldersTests.ACodeTheOrderDoesNotNameGoesByThePacksTeamLine
- VP4. The Over 150mm folder sits only inside Mechanical and Electrical of C, holding the clashes whose larger service is over 150 mm, and elsewhere those clashes stay in the test's one view, counted and named. Q183, Q182 and Q185 open. CONFLICT: the chat notes put services over 150 mm into the discipline viewpoints, read as every letter. Test: DisciplineFoldersTests.OnlyMechanicalAndElectricalHoldTheSizeFolder
- VP5. A viewpoint holds a test's New and Active clashes framed together, shows only the models its clashing items live in, the first item red and the second green and the rest dimmed. Q114 point 13, Q119 B, Q57, Q58. Test: WhatAViewShowsTests.StructureVsElectricalShowsTheModelsItsItemsLiveInAndHidesTheRest
- VP6. A test with no open clash gets no viewpoint, and no mirrored test gets one. Q114 points 15 and 19, Q133 D. Test: TestViewPlanTests.ATestWithNoOpenClashGetsNoView
- VP7. A service is over 150 mm only above it, exactly 150 is not, a rectangular service is sized by its larger side, and the larger of two services decides. Q51, Q80 D2, Q114 point 14. Test: SizeRuleTests.ExactlyTheThresholdIsOutBecauseOverIsWhatWasAskedFor
- VP8. The tool's earlier viewpoints are deleted before the new ones are made, and everything outside the tool folder is deleted but a root folder named Comments in any case, kept whole, and made empty where there is none. Q174-1, Q181, Q190 open, chat notes. Test: OutsideTheToolFolderTests.AFolderNamedCommentsIsKeptWithEverythingInIt
- VP9. A person's view named like a tool view is skipped and named, and the group stays DONE. Q158 B. Test: ViewpointBuildOutcomeTests.ASkippedViewIsNamedAndIsNeitherCreatedNorFailed
- VP10. A test whose open clashes all sit in result groups gets a view of the clashes inside them, each at its own status. Q160 B. Test: ReportClashesTests.AClashInsideAResultGroupIsViewedUnderTheGroupsTestAtItsOwnNameAndStatus
- VP11. The viewpoints box is ticked when the window opens. Q131. Test: ViewpointRequestTests.TheBoxStartsTickedOffTheSetting

## Statuses and penetrations

- SP1. Reviewed is the only status the tool sets, it moves only a clash at New or Active, and it never overwrites Reviewed, Approved or Resolved. Q33, Q195, chat notes. Test: StatusesThisToolMaySetTests.ReviewedIsTheOneStatusThisToolSets
- SP2. A pipe, duct, cable tray or conduit, or their fittings, of 150 mm or less hitting a wall, floor or roof becomes Reviewed, a larger one stays New, and the box is ticked by default on both pages. Q195, Q43, chat notes. Test: PenetrationRuleTests.A100PipeAgainstAWallMovesAndTheRuleIsOnByDefaultSinceQ195
- SP3. A wall, floor or roof counts from any file, with no discipline filter. Q41, Q42, chat notes. Test: PenetrationRuleTests.NothingInTheRuleKnowsWhatADisciplineIs
- SP4. A structural foundation counts with walls, floors and roofs, while framing and columns stay out. Q63, Q195 silent on foundations, Q220 open. CONFLICT: the chat notes name walls, floors and roofs alone. Test: PenetrationRuleTests.APipeThroughAFoundationMoves
- SP5. A service against a service is left alone and counted. Q44, chat notes. Test: PenetrationRuleTests.APipeAgainstAPipeDoesNotMove
- SP6. Air Terminals, Mechanical Equipment, Plumbing Fixtures and Sprinklers are not services. Q47. Test, in part: PenetrationRuleTests.EveryCategoryAServiceSetAsksForHasBeenDecidedAbout
- SP7. A service whose size cannot be read is left at New by the penetration rule and named, and counts as over 150 mm for the viewpoints. Q80 D2. Test: PenetrationRuleTests.ASizeThatCannotBeReadDoesNotMove
- SP8. The by design pairs built into the pack move their clashes to Reviewed with the box ticked, off by default, and the penetration rule owns a clash both want. Q62, Q195. Test: ByDesignRuleTests.ThePenetrationRuleOwnsAClashTheyBothWant
- SP9. Every clash the tool moves carries a record in the NWF, and an undo puts back exactly the status the record names. Q50. Test: AutoReviewRecordTests.AnUndoMaySetOnlyTheStatusItsOwnRecordNames
- SP10. On an NWF refresh the status, assigned to and comments of clashes still there are kept. Q174-2, chat notes. Test: RefreshTests.WhatACoordinatorWroteIsKeptWhenTheClashIsStillThere
- SP11. A clash under a result group whose two copies carry two statuses leaves the group's row as it is and is named. Q141 A. Test: MirrorMergeTests.AClashUnderAGroupOfTheKeptTestLeavesTheGroupAsItIsAndIsSaid

## Outputs and folders

- OF1. The logs and every report but the HTML go in one folder, Log, beside the NWF and NWD folders, and the HTML goes with its pictures in the Clash report folder. Q169, chat notes. Test: ReportPathsTests.TheLogFolderSitsInTheGroupAreaBesideTheNwfAndNwdFolders
- OF2. A file the tool writes in the Clash report folder overwrites one of its name, with no dated names, and the tool's leftover jpgs there are removed, top level only. Q173, chat notes. Test: LeftoverPicturesTests.ALeftoverPictureOfAnEarlierRunIsRemovedAndNothingElseIs
- OF3. The Log folder keeps its dated names and overwrites nothing. Q173. Test: LogCopyTests.ANameAlreadyInTheLogFolderIsLeftAloneAndTheCopyTakesADatedName
- OF4. The run log lives under %LOCALAPPDATA%\ParsonsNwcFederator\logs, never in the NWF folder, and is copied into each Log folder once RESULT is written. Q162-1, Q169. Test: LogCopyTests.TheLogAndItsTsvAreCopiedWhileTheRunStillHoldsThemOpen
- OF5. Each package writes under its own report folder, its path from the parent, never inside the folder it reads. His message of 17 Oct, feature 1. Test: PackagePlanTests.TheReportFolderOfAPackageMirrorsItsPathFromTheParent
- OF6. A picked NWF is copied into a Backup folder beside the Log folder before anything is opened. His order of 14 Oct, F187. Test: RefreshTests.TheBackupFolderIsBesideTheLogFolder
- OF7. Nothing goes to ACC until the tool is finished and its bugs are fixed, and the NWF stays on the project drive. Q32. NO TEST

## Reports

- RE1. The report is the Navisworks HTML tabular export dragged into Excel, one sheet laid out as the client's. Q1. Test: OneSheetWorkbookTests.ThereIsOneSheetAndNoOthers
- RE2. A test with no clash is left out of the Excel, while the HTML keeps every test and the Coverage sheet lists every test. Q162-3, chat notes. Test: WorkbookTestCountTests.TwoTestsWithClashesAndThreeWithNoneAreTwoBlocksAndNothingElse
- RE3. The report goes out in metres, always. Q23. Test: ReportUnitsTests.TheToleranceIsConvertedAndItsLabelWithIt
- RE4. Priority is a column to sort on, and the blocks stay in the order measured off the Navisworks exports. Q49. Test: PriorityColumnTests.PickingAFileLeavesTheBlocksInTheMeasuredOrder
- RE5. No column or cell of the tool's own is on the client's sheet, and the penetration count stays in the log. Q45. Test: ClientReportColumnsTests.TheirsAreTheirsAndAnythingElseIsNot
- RE6. Family, Type Name, Material and the per item source file and discipline are not read, and the source column stays empty as the client's does. Q35 to Q40, Q55. NO TEST
- RE7. The GAP block says plainly a property read on every item and empty every time, with its count. Q76. Test: GapRuleTests.APropertyReadOnEveryItemAndEmptyEveryTimeIsSaidWithItsCount
- RE8. The Internal workbook beside the clash workbook holds the Generic Models and Coverage sheets, and the clash workbook the client's sheet alone. Q146 B, Q126 B. Test: CoverageSheetTests.TheCoverageSheetIsInTheInternalWorkbookAndTheClashWorkbookHoldsTheClientsSheetAlone
- RE9. One workbook per discipline per package, AR, ST, ME and EL, with the summary of the seven checks and only the rows that need action, sorted by severity. His round of 19 Oct, fix 4, Q218 and Q219 open. Test: DisciplineReportTests.EveryRowIsInItsOwnDisciplinesWorkbookSortedBySeverityThenBuildingThenFile
- RE10. One clash count check a community, its four counts side by side, a row red where they differ, the views held to the statuses a view shows. His round of 19 Oct, fix 6, Q222 and Q223 open. Test: ClashCountCheckTests.EachOfTheFourCountsThatDiffersMakesTheRowRedAndALogLine
- RE11. A report check never fails a group, and a count that differs is a line of the log and RESULT. Q112 notes. Test: CoverageAcrossTheRunTests.ACountThatDiffersIsAFailedLineInResultAndTheGroupKeepsItsOwnResult

## Logs

- LO1. The .tsv keeps every line in full while the tool is in test, and only the .log is trimmed. Q108, chat notes. Test: LogTrimmingTests.ARepeatedLineCollapsesInTheLogAndEveryRowStillReachesTheTsv
- LO2. Logging never stops a run. CLAUDE.md. Test: RunLogHardeningTests.AWriteThatFailsStopsTheFileAndNeverTheRun
- LO3. Each group's time stands beside its NWC sizes and item counts, and the 45 minutes is not judged until the target is set. Q101. Test: TimingBesideSizeTests.EachGroupsTimeStandsBesideWhatItHeld
- LO4. The logs folder keeps 30 logs, and a window run may prune the oldest. Q82. Test: RunLogRetentionTests.ThirtyFiveFilesLeavesThirty
- LO5. The ALIGNMENT and EXPORT CHECK blocks report and the run goes on, never skipping a group or stopping a run for what they find, and each names its total even at nought. Q65, as Q170 and Q172 left it. Test, in part: AlignmentCheckTests.AGroupWithEveryModelOnOneNamedSiteIsNotFailed

## Packages and runs

- PR1. One parent folder holds the communities, every folder below a community holding NWC files directly is one package, and nothing is ticked at the start. His message of 17 Oct, feature 1. Test: PackageTicksTests.NothingIsTickedAtTheStartAndRunIsRefusedWithAReason
- PR2. Packages come first on both pages, one route a page, one Run button. His round of 19 Oct, fix 2, Q213 to Q215 open. Test: PackagesFirstSourceTests.TheNwcPageHasOneRunButtonInTheBarUnderTheStepsAndNoControlOfTheRemovedSteps
- PR3. Two pages: the NWC page builds a group once, and the NWF page runs it every week. Q174-3. Test, in part: NwfPageTests.EveryStepIsOnWhenThePageOpens
- PR4. A fresh Navisworks for every group, Run pressed once, a group that fails never stopping the next, RESULT joining every group and each start and quit timed. Q211, Q204, chat notes. Test, in part: FreshNavisworksPerGroupTests.AGroupsOwnNavisworksKnowsItsGroupByItsNwfAlone
- PR5. Auto-Save is off while the tool works and put back exactly as it was, even when a group fails. Q207, chat notes. Test: AutoSaveSwitchTests.ARunKeepsTheValueOnItsRecordSwitchesOffPerGroupAndPutsItBackOnceAsItEnds
- PR6. Every number that shapes a run is a setting with today's value as its default, never a constant. Q210, CLAUDE.md, chat notes. Test, in part: PictureLimitsTests.TheFiveLimitsAreSettingsOfTheRunWithBadersNumbersAsDefaults
- PR7. Done means one building federates, clashes and writes Excel with no errors, every ticked building runs unattended in under 45 minutes, and the Excel's clash counts match Clash Detective exactly. Q3, CLAUDE.md. NO TEST
- PR8. The team package is made only when Bader writes make the team package. Q149. NO TEST
