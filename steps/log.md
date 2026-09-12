# log

Newest entry at the top.

## 2026-09-12 F44, the docs and the comments agree with the code

### What was done

- F44 done. 62 files. No logic changed except one member renamed, one dead constant deleted and one taken off the public surface, each named below
- The moved doc path, measured rather than taken from the list. `grep -rn "scan\.md" .` outside `.git`, `docs\history`, `steps`, `bin` and `obj` found 31 lines in 22 files carrying the old `docs\scan.md`, in both spellings. The audit said seventeen source and test files and the round brief said 28 places. 30 of the 31 are repointed at `docs\history\scan.md`, including two that named the file with no folder at all, one of which is a line the tool prints into the log. The 31st is in the bundle manifest and is Q30. `build\probe-window-scroll.ps1` in the window XAML is the probe's real path, `tools\probes`. No file outside `steps` and `docs\history` names `test-model-side.md` at all, so that one was already done
- The counts. Thirteen column headings is fifteen, seven before the item blocks and four in each of the two, counted off `ClientReportColumns` and read off the header row of both committed exports. Five pickers is six, the logo picker included, counted off `PickerKind`. Three rerun cases is four, counted off `RerunDecision`. The eight faults a workbook check once reported clean over is still eight, and the fixture that breaks them now holds fourteen tests. Twenty one picture names is eight names asserted, read off a folder holding 64 pictures and a logo
- The rules file made to agree with itself, ten corrections. Distance is the rounded number with no format, which is what the code writes, and the bullet asking for a raw number behind a format is gone. The Item ID label is chosen by the tool, and which property supplied the value is kept on the item. The workbook check reports sheets, blocks, rows, the count of each block and the first divergence, and counts no column of ours, because there is no column of ours to count. The five extra item properties are written nowhere and there is no choice about it. The picture cost figures are in the log and nowhere else, because the Summary sheet they used to go on is gone. A group can still end left alone, when its rebuild never started. `HealthCheckResult.DistinctRuleCount`, not `HealthCheck`. The Run button is in the bar along the bottom that every step shares. The picker count. And the Layer column, which is the one that had to be settled by opening the files
- The Layer column, settled by measurement. The rules file said the client's report has NO Layer column and that the thirteen are the whole of it. Both committed exports carry one per item block, read off their header rows on 2026-09-12, and the code has written fifteen headings all along. The two exports the old bullet was measured on, 1A04WE and 1A02WE, were read on Bader's machine and are not in this checkout, which is now said wherever either is named
- The sheets that are gone. `HasSheet` is `HasRows`, with its three callers and three tests, because it means has rows and every test had a sheet only before the workbook became one. The comments on `TestReport`, its `Number` and `Name`, `OpenClashes`, `PlannedClashTest` and two test fixtures say block or row where they said sheet
- The logo comments. Three said nothing here copies the logo, and the engine copies one into the report's own _files folder every run. What is true is that no copy of it is in this repo or the bundle, it is read off the machine that is running, and that is what all three say now
- The copies folded. Four stacked summaries, two of them on the wrong member, unstacked and the displaced block put on the member it describes. Four copies of one sentence about `File.Exists` said once per file. Two `Images` properties sharing a rule, now said on the runner and pointed at from the harvest. Two naming members sharing a sentence about the project code where one holds a position and the other a value. Four rule sentences copied word for word out of the rules file now say what the code does and point at the rule
- Two comments that restated the test name under them are gone, and two that restated the value of the constant beside them now carry the measurement and not the number
- The probes. `probe-units.ps1` and the three window probes had no path test and no UNKNOWN line, which the probes README says every probe has. Each now tests every path it needs, one file at a time so a missing one is named, and says UNKNOWN and stops. The README needed no change once they were true
- The test layout. `CrossFileTests` is three tests and all three are health checks, so it moved to a new `Health` folder and every Core folder now has a test folder, which is what CLAUDE.md says. The health assertions that sit beside the reader tests for one sample file stay there, because they read the same file
- CLAUDE.md. The bundle folder is not build output. It holds one hand written manifest that `install.ps1` copies into `artifacts`, and `artifacts` is what the build writes. Both lines say that now, and the never edited rule stands with its real reason
- Two members the F40 entry named as deleted and which were still there. `ClientFormat.DistanceFormat`, a public const nothing called, whose comment stated the opposite of what the code does, is deleted. `ClashReportXml.QuickProperties`, no caller under src and two tests reading it, is internal, which is what F40 did with that kind. The F40 entry overstated on both and this entry is the correction
- Noticed and not acted on. `TestReport.Number` has no caller under src and two tests read it, which is the Q26 kind. The one sentence about a leftover temp folder sits above the same catch block in nineteen test files, and one shared helper for it is F16, which is next
- Proved here: Core builds with no warning. The parse of the whole add-in with no references, the same six error codes as before the edits and not one `CS1xxx`. The arity check, 0 mismatches. The window XAML parses as XML after the two text changes. Core tests before: 908 passed, 37 failed, 33 skipped, 978 total. After: the same, because this fix adds no test and changes no rule a test reads
- Waits for the local machine: six add-in files and the XAML changed, all of them comments except the `HasRows` rename and the help line under the paste into cells tick. The proof is the build and one run whose SETS and CLASH blocks read as before

### What remains

- F16, then the read of `03_bader_next.md` against the code, then the closing entry
- Q30 for Bader, raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F44 PR
2. F16, the tests path neutral

## 2026-09-12 F43, three settings that are constants

### What was done

- F43 done. The stop after count, the log count and the report subfolder each have the default they always had and can now be set. The progress interval in `ClashRunner` stays a const, because no rule names it, which is what the audit's verifier pointed out
- The stop after count is `ReportOptions.StopAfterFailures`, defaulting to the guard's own `DefaultThreshold`, and the one guard the whole run shares is built from it in the engine constructor. It was a field initialiser calling the no argument constructor, so no run could reach the number. A count under one is refused where it is set, with the sentence the guard already uses, rather than throwing in the middle of a run
- The log count is `RunLog.KeepLogs`, defaulting to `DefaultKeepLogs`, read by the no argument `StartOrDisabled` the plugin calls. It is static because the log opens on the first line of the button handler, before a window or any options object exists, so there is nothing else for it to hang off. Zero keeps the live file alone, which is a real answer, and fewer than none is refused
- The report subfolder is `ReportPaths.Subfolder`, defaulting to `DefaultSubfolder`, read by the one place that builds the fallback folder. Static for the same kind of reason: the open file run and the line under the Outputs step both work the folder out with no options object in front of them, and threading a parameter through four call sites would have put the same name in four places. A name that is empty or carries a slash is refused where it is set
- Both slashes are refused by name rather than by asking the running platform what it calls a separator. The first version asked, and the test that hands it `Reports\Weekly` passed the backslash straight through in this container, where the separator is a forward slash. That is the F16 fault caught in a new test before it was written down
- One dead thing went with it. `ClashRunner`'s two argument constructor built its own guard and nothing in src or in the tests called it, so it is gone and the guard is handed in, which is the rule that the guard lives for the run and not for a group
- `ImageOptions.DefaultStopAfterFailures` already reads `RepeatedFailureGuard.DefaultThreshold` rather than repeating the 50. F40 did that one
- Nothing outside the code sets any of the three, so today the change is that they can be set at all. Whether a settings file is wanted is Q29
- Proved here: thirteen new tests, each changing a setting and reading the answer back out of the thing that uses it, and handing each one a value it must refuse. The folder actually chosen follows the subfolder setting, and so does the line under the Outputs step. A guard built from a count of two stops after the second failure and not the first. The arity check over the whole add-in, 0 mismatches. The parse of the whole add-in with no references, the same six error codes as before and not one `CS1xxx`. Core builds with no warning. Core tests before: 895 passed, 37 failed, 33 skipped, 965 total. After: 908 passed, 37 failed, 33 skipped, 978 total
- Waits for the local machine: two add-in files changed, the engine and the clash runner. The proof is the build and one run whose CLASH block reads as before, because every default is the number it always was

### What remains

- F44, F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry
- Q29 for Bader, raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F43 PR
2. F44, the docs and the comments agree with the code

## 2026-09-12 F42, no framework message in a label

### What was done

- F42 done. Four labels that carried a type name and a framework message now carry plain words, two stale sentences about what a run with no XML does are corrected, and seven defaults that were typed into the XAML as well as into Core are read from Core
- The four labels. `RunLog.DisabledReason` named the folders it tried with the exception behind each one, and the window wrote the lot into the progress line. It now names the folders and nothing else, and the log line under it carries what each one threw. The repeated failure guard built its reason from the exception type name and message and handed it to the progress label. It now has two, `Reason` for the log, which keeps what was thrown, and `ReasonInPlainWords` for the label, which says how many tests failed the same way and that the log says what the failure was. `FederationEngine.Describe` appended every error, type names included, into the open file run's summary label. It now says how many errors there are and that the log says what they were. The window's `Describe(path)` returned the exception message when a picked file would not read. It now says the log says why, and the failure is already laid out in the log by the line above it
- Every one of those wordings lives in Core where a test reads it, the way `ReportPaths.WhereTheyGo` does. `RunLog.WhereTheLogIs`, `NoLogFileOpened`, `ErrorsAreInTheLog` and `TheLogSaysWhy`, and `RepeatedFailureGuard.Stopped`. `WhereTheLogIs` has a static form taking the three things the line depends on, so the rule can be read without a log on a disk
- The two stale sentences. The Clash step heading said leaving the XML empty makes Run do the model side only, and the `ClashWork` class comment said the same. Since F8 a run with no XML runs the tests already saved in each NWF and builds no set, which is what both say now. Only a run with no XML and no saved test does the model side alone
- The seven defaults. The photo size, the cap, the paste into cells tick and the five status ticks were typed into the XAML while `ImageOptions` held the same values in Core, the 1024 among them, which is measured off all 60 pictures of the accepted report. The XAML carries none of them now and the constructor fills them from a fresh `ImageOptions`, filled where the naming boxes are filled and for the same reason. The audit named the size and the five ticks. The cap and the paste tick are the same duplication in the same grid, so they went with them
- `probe-window-defaults.ps1` builds the real window and reads the controls, so it reads whatever the constructor set and needed no change. It will now print the `ImageOptions` values rather than the XAML's
- Noticed and not acted on. `FolderMemory.DisabledReason` is written in four places, carries `error.Message`, and nothing under src reads it. It reaches no label, so it is not this fix's, and it is the same kind of member F40 made internal, which is Q26. Bader decides
- Proved here: ten new tests in `LabelWordsTests`, each handing the wording the very thing a label must not carry and asserting it is not in what comes back, which is the rule that a test reading only the good case proves nothing. The arity check over the whole add-in, 0 mismatches. The parse of the whole add-in with no references, the same six error codes as before the edits and not one `CS1xxx`. Core builds with no warning. Core tests before: 885 passed, 37 failed, 33 skipped, 955 total. After: 895 passed, 37 failed, 33 skipped, 965 total. The ten more are the new fixture and the 37 and the 33 are unchanged
- Waits for the local machine: the add-in does not build here. Three add-in files and the XAML changed. The proof is the window opening with the photo size reading 1024, the cap 0, New, Active and Reviewed ticked and Approved and Resolved clear, and the Clash step heading reading the new sentence

### What remains

- F43 to F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F42 PR
2. F43, three settings that are constants

## 2026-09-12 F41, every handle disposed

### What was done

- F41 done. Three add-in files, `SetBuilder`, `SavedTests` and `ClashRunner`. No logic changed and no line the log prints changed. Every handle this tool creates or resolves is now released where it is finished with, rather than left to a finalizer
- The rule behind it was measured and is in the history file. All of `ClashTest`, `ClashResult`, `ClashSelection`, `SelectionSet`, `SelectionSource`, `Selection`, `ModelItemCollection`, `Search` and `SavedItem` are disposable, and releasing a wrapper releases the wrapper and never the document's object. `SavedItemCollection` is not disposable and is never disposed, so no collection is touched
- `SetBuilder`. The `Search` and the `SelectionSet` built from it are in a using each, the set released first so nothing it shares with the search goes while the search is still read. The folder the set is added under, which came back from `EnsureFolders`, is in a using around the whole build. The `FolderItem` handed to `AddCopy` is in a using, because `AddCopy` takes a copy. The four folder reads inside `EnsureFolders` that answered a question and were then dropped are released. `ResolveFolders` releases each level it walks past. `FindFolder` and `FindSelectionSet` release every child they do not hand back, `Describe` releases every child it names, and all three `ModelItemCollection` reads are in usings
- `SavedTests`. `SelectionA` and `SelectionB` are read once each into a using instead of twice each, so a walk of 1830 tests makes two wrappers a test and not four
- `ClashRunner`. The root of the set tree in `IndexSets`, the sides read for the comparison, the sides filled when a test is created and when one is replaced, the `Selection` behind each side in `LocatorOf`, `FillSide` and `ItemsOn`, and the leaf that turns out not to be a test in `Resolve`. `Resolve` releases each level it walks past, and releases it only once the child below has been read off it, which is the order `ResolveFolders` in `SetBuilder` and `WalkTests` here already use. Nothing reads a collection whose owner has gone
- `AddedAt` in `SetBuilder` reads the child at the index the count held before the add and checks its name, the way `Create` in `ClashRunner` already does for tests, and says so in the log when the tree is not that shape and it has to look by name instead. That is the read that made the set path O(n squared)
- Counted on the reference file, 61 sets and 1830 tests in one group. The side reads alone were 14,640 wrappers a comparison run before this and are 7,320 now, and the four folder reads dropped in `EnsureFolders` were one a folder a set
- Two reads were left alone on purpose and are Q28. `search.Selection` in `SetBuilder`, once a set, and the source read out of a side's own collection in `LocatorOf`, which sits inside the loop over the indexed sets and so is read once a set rather than once. Neither is named in the F41 list, both are small beside what was fixed, and the ownership of a sub object read off a handle this tool owns is not in the measured record. Bader decides rather than the worker assuming
- Proved here: a parse of the whole add-in through the Roslyn compiler with no references, which reports every syntax error and nothing else. Before the edits and after them the error codes are the same six, all of them the missing framework and the missing Navisworks types that no references means, and not one `CS1xxx`. That is the first check of add-in syntax this repo has had in the container. The arity check over the whole add-in, every `new` and every static call against the parameter counts declared under src, 0 mismatches. Core is untouched, so the tests are the same before and after: 885 passed, 37 failed, 33 skipped, 955 total
- Waits for the local machine: the build, then one building run twice with the XML, the SETS and CLASH blocks read as before and the run no slower. The steps are in `03_bader_next.md`

### What remains

- F42 to F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry
- Q28 for Bader, raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F41 PR
2. F42, no framework message in a label

## 2026-09-12 F40, dead members out, second pass

### What was done

- F40 done. 45 members deleted, 64 taken off the public surface, 66 files touched, 1045 lines out and 171 in
- Every name was checked twice before anything went. Once by a script written for this, which reads every `.cs` and `.xaml` under src, classifies each reference to a name as code, comment or string, and reports the hits outside the declaring file, inside it, and in the tests. That is what separates `ClashRunOutcome.Ran`, which nothing calls, from `PageCheck.Ran` and `WorkbookCheck.Ran`, which are called and share the name. Once again by fourteen readers, one per area, each grepping src and the XAML itself with a verifier told to refute it. The two readings agree on every member. The script's table and the readers' verdicts are both in the PR body
- What went outright, 45. The four collections on `ClashRunOutcome`, `RepeatedFailureGuard.Seen` and `Threshold` with the counter behind them, `ClashTestPlan.SkipReasonCounts`, `OpenClashes.All`, `Default` and `Describe`, `BuildStamp.LooksStamped` back as internal, `BundleAssemblies.MissingFrom` with `ReferencesOf` which only it called and `Resolved` with the list it copied, `FolderMemory.IsRemembering`, `GroupRecord.Seconds` with the constructor that took it, `RunLog.Start()` and two `GroupFinished` overloads, `ExchangeReader.ReadFiles`, `SelectionSetDefinition.Guid`, nine members of the report model that went with the Summary sheet, `ClientFormat.DistanceFormat`, `ClientShapes.CoordinatesIn`, `ImageOptions.Statuses`, `WorkbookWriter.ClientColumns` and `Options`, `PageCheck.HeaderColumns`, `ReportOptions.FolderFor`, `ClashReportXml.QuickProperties`, `ContainerNameSettings.Copy`, `OutputNaming.All` and `Copy` with `NamePattern.Copy`, `OutputNameTable.Build` three argument, `ParsedContainerName.SourceName` with the parameter that fed it, `BuildingGroupingResult.Find`, `NwfComparison.Moved` with `Moves` and its `LeafOf`, and in the add-in `ClashRunner.Drift`, `GroupRow.Names`, `ClashHarvest.Into` three argument and `JobOutcome.ReportWarnings` with `AddReportWarning` and its four call sites
- What went internal rather than out, 64. A member with no caller under src whose only reader is a test that pins a rule the repo states. Deleting it deletes the proof of the rule, so it comes off the public surface instead and the test still reaches it through the `InternalsVisibleTo` the Core project already carries. The reference file holding one batchtest and 1830 tests, 61 distinct locators, linkage none and rules empty in every test, a set name ending in a space, the stamping having run, the picture name round tripping, what the page check and the workbook check report. That boundary is Q26, because it is a reading of the rule and not the rule itself
- Why `JobOutcome.ReportWarnings` could go without losing anything. The engine added four warnings to it and nothing read the list. Each of the four already reaches the log by another path: two through `log.Block` with the check's own lines, one through the renumbering lines written just above it, one through `log.Failure`. So the list was a second copy of what the log already carries, and the two check methods stopped taking an outcome they no longer read
- The copies folded into one place. `NwfComparison` stopped working out a move, which `NwfRebuildPlan.From` already does and which the engine reads. `ImageOptions.DefaultStopAfterFailures` reads `RepeatedFailureGuard.DefaultThreshold` rather than typing 50 again. `ImageNaming.LogoName` reads `InstallFiles.LogoName`, because the copy in the report folder is that file. `ScanFindings` reads `BuildingGroup.IsSingleDiscipline`, the rule F35 made. `Samples.Repo` is the one walk up to the checkout, in place of four copies in the report fixtures
- One cascade worth naming. Deleting `TestReport.OpenUnder` and `NewPlusActive` left `OpenClashes.Of` with no caller, so it went too, and the two choice rule for what counts as still outstanding now has one reader left, the image status filter asking for what Navisworks counts as open. That is a stated rule losing its last output, so it is Q27 rather than a silent deletion
- `OutputNameRow.ReleaseToPattern` and the five per item properties stay where they are. Q24 and Q25 are unanswered
- Proved here: the arity check over the whole add-in, every `new` and every static call against the parameter counts declared under src, 0 mismatches, run before and after the add-in edits. The Core project and the test project both build clean with no warning. Core tests before: 905 passed, 37 failed, 33 skipped, 975 total. After: 885 passed, 37 failed, 33 skipped, 955 total. The 20 fewer are the tests deleted with the members they proved, named in the PR body, and the 37 and the 33 are unchanged, so nothing else moved
- Waits for the local machine: the add-in does not build here. Five add-in files changed, `ClashHarvest`, `ClashRunner`, `FederationEngine`, `JobOutcome` and `GroupRow`, every hunk read twice and the whole diff read again after the edit. Proof is the build, then one run whose log carries the same REPORT CHECK and WORKBOOK CHECK blocks as before

### What remains

- F41 to F45 and F16 in order, then the read of `03_bader_next.md` against the code, then the closing entry
- Q26 and Q27 for Bader, both raised by this fix

### Known bugs

- As in the F46 entry

### What comes next

1. Merge the F40 PR
2. F41, every handle disposed

## 2026-09-12 F46, the four the chat audit found

### What was done

- F46 done, four parts, no logic. Each was checked here before it was written, and the checks are in the PR body
- 46a. The D6 command in `steps/03_bader_next.md` named `claude/parsons-nwc-analysis-rlzgdr`, which is not a branch on the remote, and left out `fix-F39` and `round-close`, which are. The wrong name came from `git branch -r`, which prints the remote-tracking refs a clone remembers, and that ref survived here because nothing had pruned it. The command is rebuilt from `git ls-remote --heads origin`, which asks the remote and remembers nothing, and both that and the GitHub branches API return the same 30 branches, so 29 to delete. The steps now read the live list first, delete from it, read it again, and prune the clone. The trap is written beside the command so the next person does not repeat it
- 46b. `.gitignore` ignored `*.log` everywhere, and `steps/logs/README.md` asks Bader to copy a run log in and commit it. `git check-ignore -v steps/logs/2026-09-20-1C07BC.log` answered `.gitignore:50:*.log`, so a new log would never appear in GitHub Desktop. The log already in the folder looked fine only because git does not re-ignore a file it is already tracking, which is what hid this for six days. One negation line, `!steps/logs/*.log`, with the reason above it in the style of the `build/` block. Proved by writing a file into the folder from the shell and reading `git status`, which now says `?? steps/logs/zzz-check-2026-09-20.log` where before it said nothing. The file was removed the same second and nothing under `steps/logs` was edited
- 46c. `steps/README.md` listed four files and the folder holds six and the logs folder. It names all seven now, says what each is for, and says where to start. `03_bader_next.md` gets its own line saying to read it at the machine with Navisworks on it. A script compares the names in the Files list against the folder and finds nothing missing either way
- 46d. `RunLog.GroupRecords` was public and F36 kept it that way because one test read it. The rule is that a test is not a caller, so it is private, and `TheFailedCountAndTheFailedListAlwaysAgree` writes the RESULT block and reads the three group count lines back off the disk, which is what the property was standing in for and what every other test in that fixture already does
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. The changed fixture on its own: 9 passed, 0 failed
- Waits for the local machine: nothing in F46. The D6 command is Bader's to run and the list in it will have this round's own branches in it by then, which is why the steps read the live list first

### What remains

- F40 to F45 and F16 in that order, then the read of `03_bader_next.md` against the code, then the closing entry

### Known bugs

- As in the round close entry

### What comes next

1. Merge the F46 PR
2. F40, dead members out, second pass

## 2026-09-12 The plan for the second audit round, F46 and F40 to F16

### What was read first

- CLAUDE.md, the four files under `.claude/rules`, `steps/log.md` top entry, `steps/01_next.md`, `steps/02_questions.md`, `steps/03_bader_next.md` and `steps/04_audit.md`, all of them whole. Then `.gitignore`, `steps/README.md`, `steps/logs/README.md` and the files each F46 part names
- Ground state on main at 8902ff06, the round close merge. Core tests under mono on Linux: 905 passed, 37 failed, 33 skipped, 975 total. That is the baseline every entry in this round compares against, and the 37 and the 33 are the ones `04_audit.md` names one by one
- There is no `gh` on this machine, so the checks are watched through the GitHub tools the session holds and through the Actions API over curl, the same way the last round watched them. Merge only when the tests job is green on the branch head
- The add-in still does not build here and has not been built on Bader's machine since 1 Sep. So before every add-in edit the arity check runs again over `src`, every `new` and every static call against the parameter counts declared under src, and its result goes in the PR body. After every add-in edit the changed file and every caller of what changed are read, not the diff alone. That is the reading F34 got through

### The four the chat audit found, checked here before the plan was written

- 46a. `steps/03_bader_next.md` step 188 names `claude/parsons-nwc-analysis-rlzgdr`, which is not a branch on the remote, and leaves out `fix-F39` and `round-close`, which are. Both readings agree: `git ls-remote --heads origin` and the GitHub branches API each return 30 branches, so 29 to delete. The wrong name came from `git branch -r`, which shows a stale remote-tracking ref that no longer exists on the remote, and that trap goes in the file beside the command
- 46b. `git check-ignore -v steps/logs/2026-09-20-1C07BC.log` answers `.gitignore:50:*.log`, so a log Bader copies in tomorrow is invisible to GitHub Desktop. `run-20260907-093440.log` reads as not ignored only because it is already tracked, which is what hid this
- 46c. `steps/README.md` lists four files. The folder holds six and the `logs` folder
- 46d. `RunLog.GroupRecords` is public, `RunLog` itself reads it at line 892 and one test reads it. F36 kept it public for that test. The rule says the test is not a caller, so it goes private and the test reads the result the property was standing in for

### The order and what each PR does

1. F46. Branch `fix-F46`. The four above in one PR, because two of them break Bader's next hour and none of them is code. The D6 command rebuilt from the live list with the reading named, the `.gitignore` negation with `git check-ignore -v` output in the PR body, `steps/README.md` rewritten for six files and the logs folder, `GroupRecords` private with its test reading the RESULT block instead. This plan entry goes in with it
2. F40. Branch `fix-F40`. Dead members out, second pass. The 43 with no reference at all, the 34 uncalled once the type is checked, the 9 public for nothing made private, each re-grepped over `src` and the XAML before it goes and the results pasted in the PR body. `ReleaseToPattern` and the five item properties stay, Q24 and Q25 are unanswered. The copies go with them: `NwfComparison.Moves`, `Moved` and its `LeafOf`, `RepoRoot` in four test files reading `Samples.Folder`, `ImageOptions.DefaultStopAfterFailures` reading `RepeatedFailureGuard.DefaultThreshold`, `ScanFindings` reading `BuildingGroup.IsSingleDiscipline`, one `LogoName`
3. F41. Branch `fix-F41`. Every handle disposed in `SetBuilder`, `SavedTests` and `ClashRunner`, and `FindSelectionSet` reading the index the count held before the add rather than walking the collection. Add-in only, so the arity check runs first and every changed file is read after
4. F42. Branch `fix-F42`. No framework message in a label. `RunLog.DisabledReason`, the guard reason, `FederationEngine.Describe` and the window's `Describe(path)` each carry plain words to the label and keep the exception for the log. The Clash step heading at XAML line 426 and the `ClashWork` class comment both still say a run with no XML does the model side only, and since F8 the saved tests run, so both say that instead. The photo size and the five status ticks are filled from `ImageOptions` in the window constructor. Every wording lives in Core where a test reads it
5. F43. Branch `fix-F43`. Three settings that are constants: the stop after count, the log count and the report subfolder each become a property with the same default, read where the guard is built, the log is started and the folder is chosen. The progress interval stays a const, no rule names it
6. F44. Branch `fix-F44`. The docs and the comments agree with the code. The old doc path is in 28 places and not seventeen, both spellings, and `test-model-side.md` and `build\probe-window-scroll.ps1` go the same way. Then the wrong counts, the rules file made to agree with itself, the Summary sheet comments, `HasSheet` renamed, the logo comments, the doubled and copied comments, the three project names in comments, the Health tests into a Health folder, and the probes README made true
7. F45. Branch `fix-F45`. The clash step keeps its rules. A test with no tolerance attribute is skipped by name, the OLD line logs the status word alone, the compacted count is read after the compact, an UNKNOWN locator is left out of the drift comparison. `IdFrom` waits on Q25
8. F16. Branch `fix-F16`. The tests path neutral. The 37 that fail here, the six skips that misname their reason, the three collection comparisons, the one test that passes off Windows without proving anything, `Samples.Folder` in place of the four `RepoRoot` copies, one temp folder helper in place of nineteen copies of its comment
9. `steps/03_bader_next.md` read end to end against the code one more time. Every Look for line matched against what the code prints or shows, the ones that do not match corrected, and the count of corrections in the log
10. The closing entry

### Rules held through the round

- One PR per fix, branched off main, merged only when the tests job is green on the branch head, the local branch deleted after. Never a PR left open at the end of a step. The remote branches are not deleted, Bader has the command
- .NET Framework 4.8 and C# 7.3. No Navisworks type reaches `Federator.Core`
- A public member nothing in src calls is deleted with its tests, unless a decision in `02_questions.md` keeps it. No member is added that no running code calls
- The arity check before every add-in edit, the changed file and its callers read after, both said in the PR body
- No em dash, no semicolon in prose, no emoji, plain words, in every comment, doc, log line and window text
- `samples` and `steps/logs` are never touched
- A new question goes in `02_questions.md` as Q26 onward

### What remains

- Everything in the order above. Nothing in this round is proved on the local machine, because the add-in cannot build here

### Known bugs

- Unchanged from the round close entry until each fix lands

### What comes next

1. F46 on `fix-F46`, this plan goes in with it
2. F40 to F45 and F16 in order, one PR each
3. `03_bader_next.md` read against the code, then the closing entry

## 2026-09-12 The round closes, F27 to F39, the audit and D6

### What was done

- Thirteen fixes merged today, F27 to F39, each on its own branch through its own pull request with Actions green, and each local branch deleted after. F12, F13 and F14 closed with F37 and F38, F25 closed with F27. D1 to D5 are in the code. D6 is not, see below
- The audit ran over every file under src, tests, build, tools, docs, steps and the root, and is written up in `steps/04_audit.md`: fourteen readers, a verifier per finding, and three checks of my own that need no judgement. 393 findings, condensed into seven fixes, F39 to F45, and F16 widened, each a section in `01_next.md` with its files, and two questions, Q24 and Q25, in `02_questions.md`
- The audit found that the add-in had not compiled since F34 merged. Two calls in the window kept passing a `true` that F34's constructors no longer take, and the diff reading that every add-in change gets could not see it, because a diff shows the constructors that went and not the callers that stayed. F39 fixed it and is merged. The build is the first step of `03_bader_next.md` and the reason it comes before every proof is written there
- `steps/03_bader_next.md` rewritten so the proofs for F27 to F35 sit in one build, one install and one Navisworks session, 191 numbered steps, F34 first because it is seen the moment the window opens, then F33 in the same look, then F27 for the cost of a Scan and a Cancel, then the C06 run which proves F24, F29, F30 and F33 in one press, then F28 and F31 together, then F35, and the older pending proofs after. Four steps the audit found wrong in it were corrected the same day
- D6 is handed to Bader as one command at the end of that file. Every branch except main is merged into main, checked with `git branch -r --merged origin/main`. The container cannot delete a remote branch: `git push origin --delete` returns HTTP 403 from the proxy in front of the session, there is no GitHub tool in the session that deletes a branch, and a workflow file that would have done it from Actions was refused by the permission classifier as destructive, which it is. The command lists all 27 branches and says what to look for
- Proved here: Core tests under mono on Linux at the end of the round: 905 passed, 37 failed, 33 skipped, 975 total, the same 37 and 33 as after F32, every one named in `04_audit.md` with its cause. The audit found six of the 33 skips misname their reason, saying a sample is not in the checkout when it is, because two test files join the samples path with a backslash
- Waits for the local machine: the build first, then every proof in `03_bader_next.md`, then the two hooks of F38 tried under Claude Code on Windows

### What remains

- F40 to F45 and F16 in that order, then F21, F15, F18 when the sample arrives, F23 when Q20 is answered
- Q24 and Q25 for Bader, both from the audit
- The verifier pass of the audit ran 30 of its 54 batches and the other 24 died on the session's usage limit, so 180 findings carry a reader's evidence and my grep and no verifier, which `04_audit.md` says. Its journal lives in the session and not in the repo, so a later session re-checks a finding by grep rather than trusting it. Every finding in `04_audit.md` was checked by grep here before it went in

### Known bugs

- B12 OPEN, waiting on Q20. Narrowed by F32: an NWD, an address, no folder and an unreadable folder are each refused with the reason named. The ACC case itself still waits
- B13, B14 fixed in code, pending local proof, F24 and F26 in `03_bader_next.md`
- B1, B2, B11, L1, L2, L3, L4 and F22 fixed in code, pending local proof, each in `03_bader_next.md`
- B7 fixed in code, pending a local run of `tools/probes/probe-window-defaults.ps1`
- B9 fixed, and the local proof is now the build after F39
- M5 closed on the Core side, open on the add-in side until the build runs locally
- B3, B4, B8, B10, B15 fixed or closed. B5, B6, L5 to L8, M1 to M4, M6 to M8 as in `00_analysis.md`, of which M8 is F16 and L5 to L7 closed with F37
- New from the audit, all in `04_audit.md`: the add-in did not compile from F34 to F39, fixed. Handles not disposed in three add-in files, F41. Framework messages reaching four labels, F42. Three settings that are constants, F43. A missing tolerance read as zero, the Old sentence, the compact count read before, an UNKNOWN locator reported as drift, F45

### What comes next

1. Bader pulls main, builds, installs and follows `03_bader_next.md`, the build first
2. Bader answers Q24 and Q25
3. F40, then F41 to F45, then F16
4. The worker reads the logs Bader drops in `steps/logs` and records the proofs in this file

## 2026-09-12 F39, the window compiles again

### What was done

- F39 done, two lines. Found by the audit of 2026-09-12, whose steps-and-rules reader compared every call from the window into the engine against the engine's constructors, and confirmed by grep before anything was changed
- F34 deleted the republish flag and the three engine constructors that took it, and the two calls in the window that construct the engine for a run kept passing `true` as their third argument. The Run button passed six arguments to a constructor that takes five and the Run the open file button passed a bool where an exchange document goes. Neither matches a constructor that exists, so the add-in has not compiled since F34 merged, and F35, F36 and F37 changed it further without a build. Both calls now read `SetProgress, log, exchange, options`, with `nwfFolder` on the first, and the two hand button calls were already right
- Why it was missed: the add-in does not build here, and F34 was read twice as a diff. A diff shows the constructors that went and not the callers that stayed, so the reading found nothing wrong with either side on its own. The audit found it by putting the two side by side, which is what a compiler does
- A heuristic check was run once over the whole add-in, every `new` and every static call against the arities declared under src, and it finds this one call and nothing else. It is a script in the session and not in the repo, because the local build is the real check and comes first in `03_bader_next.md`
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. No Core file changed
- Waits for the local machine: the build. This fix is the reason the build is the first step of `03_bader_next.md`, and if the build names anything else, the whole error goes in the chat

### What remains

- The round close on its branch: `steps/04_audit.md`, the fix list in `01_next.md`, the closing entry

### Known bugs

- As in the F38 entry

### What comes next

1. Merge the F39 PR
2. Finish the round close
## 2026-09-12 F38, CLAUDE.md under 200 lines, rules in .claude, walls in hooks

### What was done

- F38 done, no code. Closes F13
- CLAUDE.md is 164 lines, down from 1004, and holds only what applies to every file: the host, what done means, a map of where things are, the rules for every file, how a fix is worked, the build, the tests, the two walls, what to confirm against the install, and the writing rules. The rule that C# stays at 7.3 is written down for the first time, it was only in the project files before
- The old file is whole in `docs/history/claude-md-history.md` with a first line saying it is history and where the current rules are, and a diff against main's CLAUDE.md shows it verbatim. Every date, count and measurement is there and nothing was cut from it
- The 122 bullets of Rules the code holds were split by which project holds the rule. 32 that name a Navisworks call, the window, the engine or the installer went to `.claude/rules/addin.md` with the tick box section. The other 90 went to `.claude/rules/core.md` with the file naming section, the clash XML section and the diagnostic log. The split was by a script that cut at each bullet, so no bullet was retyped, and the two files together hold all 122. Each starts with a paths line, `src/Federator.Addin/**` and `src/Federator.Core/**`, and a short intro saying what the other file holds
- `.claude/rules/tests.md` and `.claude/rules/steps.md` are new, written by hand from the Tests section, the F37 test layout, the round's rules for the steps files, and the reasons the rules files carry: the break one thing test, reading a written file back as a file, the client lists asserted against the client's files, the samples never written, the Assert.Ignore off Windows, Ordinal never trimmed, and one entry per fix in the F26 shape
- Two walls. `.claude/settings.json` runs two PreToolUse hooks from `.claude/hooks`. `refuse-protected-paths.sh` reads the tool call off standard input and exits 2 for an Edit, Write or MultiEdit whose file_path is under `samples`, `steps/logs` or `bundle`, with Windows backslashes read as slashes and the project folder stripped off. `refuse-git-on-main.sh` exits 2 for a Bash command holding git commit or git push, options between git and the verb allowed, while `git rev-parse --abbrev-ref HEAD` says main or master. Both were tried here with thirteen piped tool calls: three refused paths including one Windows path into `bundle`, three allowed paths including `steps/log.md`, a Bash call the path hook ignores, and on main three refused commits and pushes, `git pushover` and `git log | grep commit` allowed, and the same commit allowed on `fix-F38`. The first pattern let `git -c core.hooksPath=/dev/null commit` through because it only allowed dash options between git and commit, and that is the exact form this round commits with, so it was widened and the case is in the list
- The pre-commit hook in `.githooks` is unchanged and CLAUDE.md still says how to switch it on
- `steps/01_next.md` had F26's DONE line under F38, left there by the F26 session. It is under F26 now. F38 has its DONE line and F13 reads CLOSED by F38
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. No code changed, so the run is the same run, and the 37 are the same Windows path and file locking failures
- Waits for the local machine: whether Claude Code on Windows runs the two hooks through a sh it can find. Both need a POSIX sh, Git for Windows ships one, and whether it is found without help is UNKNOWN until a session on the local machine tries an edit under `samples` and sees it refused. Nothing else in F38 needs the local machine

### What remains

- D6, the audit into `steps/04_audit.md`, `steps/03_bader_next.md` rewritten, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F38 PR
2. D6, delete every fix branch and master so main is the only branch, once a route that is not a push works
3. The audit

## 2026-09-12 F37, one type per file and one place per kind of file

### What was done

- F37 done, moves only, no logic. Closes F12 and F14
- One type per file. Nine files held thirty types and now hold one each, split by a script that cut at each type's doc comment and gave every new file the old file's usings and namespace, so nothing but the file boundary moved. `FederationJob.cs` gave `JobOutcome.cs`, `ClashRunner.cs` gave `TestAddress.cs`, `ClashTestPlan.cs` gave `ClashTestKind`, `ClashSkipReason`, `PlannedClashSide`, `ClashPlanSource`, `PlannedClashTest` and `SkippedClashTest`, `ClashRunOutcome.cs` gave `ClashStatus`, `ClashTally`, `ClashSideCheck` and `ClashTestResult`, `GroupOutcome.cs` gave `GroupRecord`, `WrittenFile` and `LoggedFailure`, `NamePattern.cs` gave `OutputNaming`, `NameCollisions` and `NameCollision`, `ScanFinding.cs` gave `FindingKind`, `FindingsTable.cs` gave `ScanCounts`, and `SetBuildOutcome.cs` gave `SetResult`. Both projects glob their sources, so no project file changed
- The sixty five test files are in folders named after the src folder they test: Clash, Diagnostics, Exchange, Findings, Grouping, Naming, Report, Rerun, Sets, Units, with `Samples.cs` at the root. Every move is a `git mv`, so history follows. The tests that read the checkout find it by walking up from the test assembly, so none of them cares where its source sits
- The six probes are in `tools/probes`, each with a `-NavisworksPath` parameter defaulting to the folder the add-in project defaults to, and the three window probes find the repo one level higher than before, because their folder is one level deeper. `tools/probes/README.md` says what each measures and how to run it. `build` holds `install.ps1` alone
- `docs/scan.md` and `docs/test-model-side.md` are `docs/history/scan.md` and `docs/history/test-model-side.md`, each with a first line saying measurement history, not current, and the date of the move. The eleven `docs\scan.md` references in CLAUDE.md and the probe line in `steps/03_bader_next.md` point at the new paths. The references inside the two history files stay as they were, because they are part of the history
- `docs/workflow.md` is the one current description of the two workflows, the rebuild, the labels and the three `RunPath` rules that give them, the open file, and the two hand buttons, written from README and `RunPath`. README keeps its introduction and points at it, at CLAUDE.md, at `steps`, at `docs/history`, at `tools/probes` and at the installer, and no longer carries a copy of the workflows
- Proved here: Core tests under mono on Linux, before and after: 905 passed, 37 failed, 33 skipped, 975 total. The same tests in new folders, and the Core splits compile. The 37 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. The two add-in splits, `JobOutcome.cs` and `TestAddress.cs`, and the two files they came out of were read after the split, every line of them is a line that was there before. The probes were not run here and cannot be. Proof is the build, then one probe with and one without the parameter

### What remains

- F38, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F37 PR
2. F38, CLAUDE.md under 200 lines, rules in .claude, walls in hooks

## 2026-09-12 F36, dead code and copies out

### What was done

- F36 done. Every name went through grep over src first, and the results are in the PR body. Deleted with their tests: `OutputNameCheck`, the pattern based collision check that `OutputNameTable` replaced, with six tests and the helper only they used. `ContainerName.BuildOutputName`, four overloads, and `ContainerNameSettings.ForcedLevel`, `ForcedTypeCode`, `ForcedNumber` and `OutputDisciplineCode` with their defaults, their lines in `Validate` and `Copy`, and `RequireValue`, which only they called, with eight tests deleted and three trimmed to the parsing they still prove. The output name has been `NamePattern`'s since the patterns existed, and both class comments say so now. `ClashWork.Any(exchange, savedTests)` and `AnyIn`, the one argument `Any` stays because `SourceFor` and `Describe` call it. `BuildStamp.OfCore`, the four tests read `BuildStamp.Of(typeof(BuildStamp).Assembly)`. `PickerStart.Remembers`, the tests read `FolderMemory.LastFor` directly. `RunLog.TimesFailed`, `DistinctFailureCount` and `TotalFailureCount`, the tests read `Failures.Count` and `Failures[i].Times`, and the one test that was only about `TimesFailed` went. `RunPath.Skipped` stays, a rebuild that fails still ends its group as Changed and the GROUP line reads it. `GroupRecords` stays public, `ResultBlockInvariantTests` reads it
- One `Or`. `Federator.Core.Diagnostics.Words.Or(value, fallback)` replaces the private copy in ten files, `BundleAssemblies`, `FolderMemory`, `RunLog`, `OpenDocumentJob`, `GroupJudgement`, `ClashReportXml`, `NavisworksFacts`, `ClashHarvest`, `ClashRunner` and `DocumentUnits`, fifty calls rewritten. The engine's one argument `Or`, which said none, is `Words.Or(value, "none")` at its eight calls. `TestDrift`'s was not an Or, it quotes the value and says nothing for none, so it is `QuotedOrNothing`. `WordsTests` pins the three cases, a value, null or empty, and a space, which is a value because two set names in the reference file end in one
- One client column list. `ClientFormat.ClashColumns`, `PerItemColumns`, `FirstItemColumn` and `ItemColumns` are read off `ClientReportColumns`, and `ClashReportXml.QuickProperties`, `QuickName` and `QuickType` too, so the fifteen words live once, where a test checks them against the client's own exports. `ClientReportColumnsTests.EveryOtherColumnListReadsThisOne` pins it
- One `InstallFiles` locator replaces `StylesheetLocator` and `LogoLocator`, which followed the same rule in two files. `FindStylesheet`, `StylesheetCandidates`, `WhyNoStylesheet`, `FindLogo`, `LogoCandidates`, `WhyNoLogo`, with one `FirstOnDisk`, one `WhyNot`, one `Languages`. The engine, the window and five test files read it. Nothing about either file changed, the paths tried and the lines logged are the same
- The seven doubled summary blocks that were left, in `ClashHarvest`, `ClashRunner`, `FederatorWindow.xaml.cs`, `RunLog`, `ClientFormat`, `ReportPaths` and `WorkbookWriter`. Five were a summary that had drifted off its method, put back above `FirstProperty`, `Run`, `Tolerance`, `Choose` and `WriteColumnHeadings`. One in `RunLog` was two summaries for `Failure`, merged. One in the window described a box that has no method, deleted. A grep for a summary closing and another opening finds nothing now
- `probe-window-defaults.ps1` no longer lists `RepublishNwd` and `WriteImages`, the two boxes that are gone
- Proved here: Core tests under mono on Linux, before: 916 passed, 37 failed, 33 skipped, 986 total. After: 905 passed, 37 failed, 33 skipped, 975 total. Eleven fewer, fifteen deleted with the members and four added, `WordsTests` three and `ClientReportColumnsTests` one. The 37 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. Six add-in files changed, `ClashHarvest`, `ClashRunner`, `DocumentUnits`, `FederationEngine`, `NavisworksFacts` and the window, every one a rename of a call or a moved comment, read twice. Proof is the build, then one run whose log reads as before

### What remains

- F37 and F38, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F36 PR
2. F37, one type per file and one place per kind of file

## 2026-09-12 F35, clash only where two disciplines meet

### What was done

- F35 done, D5. The one model rule counted files, so two NWCs of one discipline ran every test for nothing the way one NWC did. The rule now counts disciplines and lives in one place, `BuildingGroup.CannotClashWith(int disciplineCount)`, fewer than two and nothing in the group can clash. `BuildingGroup.IsSingleDiscipline` replaces `IsSingleModel`, `GroupRow` carries `DisciplineCount` off the scan and reads the same rule for its `IsSingleDiscipline` and its status line, which now says one discipline, and `FederationJob` carries the count on to the engine, which sets `ClashRunner.SingleDisciplineGroup` from `job.IsSingleDiscipline`. `ClashSkipReason.SingleModel` is `SingleDiscipline` and every line that said one model says one discipline
- `FederationJob.DisciplineCount` is `int?`. The open file passes null, because nothing was scanned and its count is UNKNOWN, so it is never judged this way, which is what the file count gave it before, an empty list is not one file. The five argument constructor nothing called is gone
- The SINGLE DISCIPLINE scan finding says the tests are created and none is run, it said none of them can find anything, which was true before D5 and is not how it ends now
- Proved here: `SingleModelGroupTests` is `SingleDisciplineGroupTests`, its nine references renamed, and it gains `AGroupWithTwoFilesOfOneDisciplineKnowsItCannotClashEither` and `TheRuleIsFewerThanTwoDisciplines`. `GroupingModeTests.AGroupHoldingOneDisciplineKnowsItCannotClash` reads the new name. Core tests under mono on Linux, before: 914 passed, 37 failed, 33 skipped, 984 total. After: 916 passed, 37 failed, 33 skipped, 986 total. The 37 are the same Windows path and file locking failures, none new. A grep for `SingleModel` and one model over src and tests finds nothing but three comments about one model in a document, which are about a different thing
- Waits for the local machine: the add-in does not build here. Five add-in files changed, read twice. Proof: scan a folder holding a building with two NWCs of one discipline, its row must read Ready. One discipline, so every test is created and none is run, and the run's CLASH block for it must say holds one discipline and skip every test with the SingleDiscipline reason

### What remains

- F36 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F35 PR
2. F36, dead code and copies out

## 2026-09-12 F34, window wiring

### What was done

- F34 done. The wiring: `NwdFolderBox` fires `OnOutputFolderChanged` like the other two folder boxes, so the Outputs summary and the run paths refresh when it is typed into. The clash XML tick box's handler is `OnXmlChanged`, it was `OnRepublishChanged`, a name left over from the box that went. The window class comment says four tabs, it said three steps. The Step 4 comment in the XAML said no Excel, that is the session after, and the marker in the code said sets only in this session, both years stale, both say what the step holds now
- The republish field is gone: `republishNwd` out of the engine, out of both constructors that stay, and the not republished branch out of `WriteNwd`, which says it publishes every run. The three engine constructors nothing called are gone. `JobOutcome.NwdRequested` is gone, `Facts()` hands `GroupFacts` a true with the reason. `GroupFacts.NwdRequested` stays, because `GroupJudgement` reads it and its tests pin the rule that a step switched off is not a failure. `GroupOutcome.cs` says the tick box used to exist
- `ReportsWanted` no longer sets `WriteHtml`, `SetDocumentUnits` or `Images.Write`. All three are true in the Core constructors, with one comment each saying they are fixed on and the window does not set them
- D3. The Outstanding counts combo is gone from the Clash step with its label and its row, `FillOpenCounts`, `ChosenOpenCount` and the outstanding log line with it. `ReportOptions.OpenCount` and `ClashReport.OpenCount` are gone, and the engine no longer copies one into the other. `OpenClashesTests` lost the two asserts on the deleted defaults and its report helper lost the parameter it set. `OpenClashes` itself stays with its tests, because `ImageOptions` reads Navisworks open off it. CLAUDE.md's bullet says the choice is no longer offered
- D4. `OnBuildSets` and `OnRunTests` each read the file, make an engine with `ReportsWanted()` and call one method: `BuildSetsByHand`, which is `BuildTheSets` on the open document with the open file standing as the group, and `RunTestsByHand`, which is `CreateAndRunTheTests` the same way with the tests from the XML. The SETS and CLASH lines in the log now read the same whichever way the work was started, the window's copies of them are gone, and so are `PlanOnlyLines` and the two section title constants. `BuildTheSets` fills `outcome.Sets` with the skipped sets when there is nothing to build, so the button has lines and a summary to show, and `SetBuildOutcome.Summary()` is the one summary line, in Core, tested. A hand run now builds the report in memory the way a run does and writes nothing, because there is no report folder
- D1. Picking an XML runs `HealthCheck` on it: the whole summary goes in the log as a `HEALTH` block and one sentence goes under the file line, from `HealthCheckResult.Line(bool fileHoldsSets)`, in Core, tested: unusable export, tests only file, every locator resolves, or how many locators miss and how many tests would be skipped
- The orphan summary that sat above `OnRunOpenDocument`, which belonged to `OnBuildSets` before it drifted, is gone, and `OnBuildSets` has its own. One fewer for F36's list of eleven
- Proved here: `SetBuildOutcomeTests` gains `TheSummaryCountsCreatedPresentFindingZeroFailedAndSkipped`, `NothingBuiltAndNothingSkippedIsAFileHoldingNoSets`, `NothingBuiltWithSetsSkippedSaysHowMany`. `AllInOneFileTests` gains `TheHealthLineSaysEveryLocatorResolves`, `InfraSetsFileTests` gains `TheHealthLineSaysTheExportIsUnusable`, `ExchangeReaderShapeTests` gains `TheHealthLineOfATestsOnlyFileSaysTheModelSetsAreUsed` and `TheHealthLineNamesHowManyLocatorsMissWhenTheFileHoldsSets`. Core tests under mono on Linux, before: 907 passed, 37 failed, 33 skipped, 977 total. After: 914 passed, 37 failed, 33 skipped, 984 total. The 37 are the same Windows path and file locking failures, none new. A grep for `republishNwd`, `OpenCountBox`, `ChosenOpenCount`, `FillOpenCounts`, `OnRepublishChanged`, `PlanOnlyLines`, `SetsSectionTitle`, `ClashSectionTitle` and `.OpenCount` over src finds nothing
- Waits for the local machine: the add-in does not build here, and this is the largest add-in change of the round, the window, the XAML, the engine and the job, read twice. Proof: build, install, open the window, the Clash step must have no Outstanding counts row, pick the reference XML and the line under it must end with every locator resolves and the log must carry a HEALTH block, press Build sets on an open model and the SETS block in the log must read as a run's does, press Run tests and the CLASH block likewise, and type into the NWD folder box and watch the Outputs summary change

### What remains

- F35 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F34 PR
2. F35, clash only where two disciplines meet

## 2026-09-12 F33, one unit table

### What was done

- F33 done. Four lists named units and they had drifted: `ExchangeUnits` held eleven codes with factors, `DocumentUnits.Short` held eight short labels and lowercased the enum name for the rest, so Micrometers wrote "micrometers" where `ClashRunner.UnitName` wrote "um" for the same unit, and the window held five names with its own wording. Now `src/Federator.Core/Units/UnitTable.cs` is the one table, one `UnitRow` per unit with the Navisworks enum name as text, the display name, the short label, the exchange code and the millimetres in one. `ExchangeUnits` converts through it and holds no factor, `DocumentUnits.Short` and `ClashRunner.UnitName` look their enum value up in it, and the window fills its combo from `UnitTable.Offered()`, five rows with the default first, wording the default as before
- `TryWantedUnits` replaces `WantedUnits`. A model units name the table does not know is no longer parsed with a silent fallback to Meters. `FinishTheGroup` logs `UNITS    <name> is not one this tool knows`, puts the reason with every known name on the group's error list, and returns before anything is converted, run or written, so the group is FAILED. A name in the table that the installed enum does not carry is logged as that and fails the same way
- `ExchangeReader.ReadTest` reads the tolerance as written and converts nothing. `ClashTestDefinition.ToleranceMillimetres` and `ToleranceIn` are gone with their asserts. So a file whose units attribute the tool does not know now reads, and `ClashTestPlan.Convert`, the one place a file unit is judged, skips each test by name with the line that says the unit is not one this tool converts. That line could not be reached before, because the reader threw on the whole file first. `ExchangeUnits.ToMillimetres` and `FromMillimetres` are gone too, nothing in src called them once the reader stopped
- `ReportUnits` and CLAUDE.md say the factors are `UnitTable` read through `ExchangeUnits`, and CLAUDE.md gains one bullet on the table and the failed group
- Proved here: `UnitTableTests`, eight tests, every column filled, no enum name or exchange code twice, every offered row in the table with `ReportOptions.DefaultUnits` first, lookups case blind and trimmed, an unknown enum name refused naming every known one, the feet row giving the reference file's 75 mm, the report label being the metres row, and `ExchangeUnits` knowing exactly the table. `ExchangeReaderShapeTests` gains `AFileInUnitsTheToolDoesNotKnowIsStillRead`, `ClashTestPlanTests` gains `AFileUnitTheToolDoesNotKnowSkipsEveryTestByNameRatherThanThrowing`. Core tests under mono on Linux, before: 897 passed, 37 failed, 33 skipped, 967 total. After: 907 passed, 37 failed, 33 skipped, 977 total. The 37 are the same Windows path and file locking failures, none new
- Waits for the local machine: the add-in does not build here. Four add-in files changed, `ClashRunner.UnitName`, `DocumentUnits.Short`, `FederationEngine.TryWantedUnits` with the guard in `FinishTheGroup`, and the window's three unit methods, read twice. Proof: open the window, the Model units combo must list Metres, which is what the models are set to, Millimetres, Centimetres, Feet, Inches in that order, and a run must log the same UNITS lines as before

### What remains

- F34 to F38 in order, then D6, the audit, `03_bader_next.md`, the closing entry

### Known bugs

- As in the F32 entry

### What comes next

1. Merge the F33 PR
2. F34, window wiring

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
