---
paths:
  - "src/Federator.Core/**"
---

# Rules for Federator.Core

Core has no Navisworks type in it and every rule here is one a test can prove. What
the add-in must do with the Navisworks API is in addin.md. The reasons and the
measurements behind every rule are in docs/history/claude-md-history.md, kept whole.

## File naming

Input:  1104-PAR-1C07BC-ZZZ-AR-MOD-000001.nwc
Split on the hyphen. Part 3 is the building, part 5 is the discipline.
How the files are gathered is a CHOICE, made in the Grouping step, and per building
is only the DEFAULT. This used to be written here as a rule and it is not one. The
four choices are one file per building, one per building and discipline, one per
discipline across every building, and one file for everything. Per building puts
every discipline of a building into one federation, and under it discipline is read
for reporting only. Under the other three it decides the split as well.
Group on the full 6 character building code. 1C07BC and 1C07K1 are two buildings.

The output name is project, originator, building, level, discipline, type, number,
and it is a PATTERN with defaults rather than a fixed string. This used to be written
here as fixed and it is not. The first three come from the file names. The other four
are supplied, because the files in one group can disagree on all of them and outputs
overwrite, so copying from any one input would mean picking a winner.

The names are made by the patterns, one row per group of a package, `OutputNameTable`, and
since F232, Bader's round of 19 Oct 2026, fix 2, no name is typed over: the Outputs step holds
the naming boxes and the logo only, so the table of names a person could type over, its refill,
its cleared cells of FR-160 and its typed names kept across a regroup of FR-153 went with it.

A regroup keeps the Run boxes, FR-153, `Federator.Core.Grouping.KeptRunTicks`: a group the
person left unticked starts unticked, matched by its key exactly, the label of its package and
its group since F232, and every other group starts ticked where it can be, one new to the list
and one that was blocked, whose empty box was never a choice, included.

The collision check runs on the table, so two groups whose patterns give one name are caught.
It names both groups and the package does not run. A group's internal workbook is named after its workbook with the
suffix, `GenericModelsSettings.WorkbookNameFor`, and sits beside it in the Log
folder, so the check reads each internal workbook name against every other group's
workbook name too, ignoring case as the three kinds are read: a group whose workbook is X
Internal beside a group X is named with both groups and the run does not start, F127-R5,
the breaker's point 6 on F127b. `OutputNameTable.WhyTheRunCannotStart` takes the settings
the engine names the internal workbook by, so the suffix the check reads is the one the run
writes.

Every supplied field is shown in the Outputs step and can be changed there. The
defaults are ZZZ for the level, which is the ISO 19650 code for all levels, BM for
the discipline, which is a federated building model, MOD for the type, which is a
model, and 000001 for the number, which never advances because the file is
overwritten in place. A fifth, ZZZZZZ, is the building field for a group that covers
several buildings, following the same all convention.

The building field carries the group's own building code where the group is one
building, and the discipline field carries the group's own discipline where the group
is one discipline. The supplied value is only used where the group spans several.

There is one pattern each for the NWF, the NWD and the workbook, because a project
may want them to differ. They start identical. A set of patterns that would write two
groups to the same name is reported and the run does not start, because outputs
overwrite with no date suffix and the second would silently destroy the first.

The output name is built, not patched. That means a five part input still gives a
full seven field output name, and the readable-name floor is five parts. A six part
name reads fine.

If two files in one group disagree on the project code or the originator, report
it and skip the group. Do not pick one.

The split character, the part positions, the grouping choice and every supplied name
field are settings, never constants.

## What the clash test XML holds

Measured from the real files on 2026-08-27 and corrected on 2026-08-28. Do not
re-derive this. An earlier version of this section said the reference file held no
sets. It holds 61. The numbers below are the measured ones.

1104-PAR_CLASH_AllInOne holds both parts, 61 sets and 1830 tests in one file, and
all 61 test locators resolve against its own sets. It is the reference file. When
this file and another disagree about anything, this one is right.

The tests:

- root exchange, units="ft", one batchtest, 1830 clashtest children
- every test: test_type="hard_conservative", status="new",
  tolerance="0.2460629921", merge_composites="1"
- that tolerance is 75 mm. Tolerance is in the file units, convert to document
  units before use
- each side is one clashselection holding one locator, a name path such as
  lcop_selection_set_tree/Mechanical/Mechanical-HVAC/BLD-ME-Air Terminals
- 1830 is every pair of 61 with no self pairs
- linkage is none and rules are empty in every test here. Read both anyway,
  another project will use them

The sets:

- 61 sets, carrying real rules, not one rule repeated
- 102 conditions across the 61 sets. 30 sets carry one condition, 26 carry two,
  5 carry four
- paths nest. Mechanical has 4 subfolders. Walk the folders, never assume the
  sets tree is flat
- set names contain spaces and ampersands, and two of them end in a space. Match
  the exact string. Never trim a set name or a locator
- the rule vocabulary seen in real files:

      category LcRevitData_Element display Element
        property LcRevitPropertyElementCategory display Category, the Revit category
        property lcldrevit_parameter_-1002053 display Workset

      no category element at all
        property LcOaNodeSourceFile display Source File

- FLAGS 64 IS StartGroup AND IT MEANS OR, F78. Measured on 2026-09-19 over all 102
  conditions: five carry flags="64" and each of those five is the third condition of one
  of the five sets that hold four. A condition with that bit STARTS A NEW GROUP, the
  first group of a set is implicit and carries no flag, the conditions inside a group are
  ANDed and the groups are ORed. So BLD-ME-Ducts&Duct Fittings reads
  (Category equals Ducts and Workset equals ME-DUCTWORK) or (Category equals Duct
  Fittings and Workset equals ME-DUCTWORK). Everything used to be joined with "and", so
  that set was described as asking a question no element can answer, and a set finding
  thousands of items read as one that could find none. The brackets only appear where
  there is more than one group, so the 56 ordinary sets read exactly as they did
- A PROPERTY'S FRIENDLY NAME RIDES BESIDE ITS INTERNAL ONE AND NEVER IN PLACE OF IT, F78.
  lcldrevit_parameter_-1002053 tells a reader nothing and Workset tells them everything,
  and the internal name is what the API matches on. The friendly half is READ OFF THE
  FILE, which carries display="Workset" on every property, and there is NO LOOKUP TABLE
  anywhere: a table would be a second copy of a mapping the file already supplies and it
  would go stale the moment a project used a different parameter. It is left off where
  both halves are the same word

- condition test values seen: equals and contains
- a condition can arrive with no category element. The reader must not assume one
- rebuilding a search through the API uses the internal strings, never the display
  words. Element and Category and Workset and Source File are what a person reads,
  LcRevitData_Element and the rest are what the API matches on

Search_Set_Building.xml and Search_Set_Infra.xml are damaged exports. Every
condition in both reads category Category, property Name, equals Floors. They are
kept as samples only, to prove HealthCheck catches them. Never treat either as a
reference for what a good file looks like.

Counting distinct rules. Two different numbers are both right about the reference
file and they answer different questions:

- 53 is the number of distinct conditions, comparing test, category, property and
  value, with flags left out. This is what HealthCheckResult.DistinctRuleCount counts,
  because it is the one that catches a damaged export: Search_Set_Infra has 2715
  conditions and 1 distinct condition
- 59 is the number of distinct sets, comparing each set's whole ordered list of
  conditions. It is 59 rather than 61 because two pairs of sets carry identical
  rule lists: Telecom Fixtures with Telephone Devices, and Electrical Fixtures
  with Devices. AFTER F87 IT IS 60, because Devices now asks for Nurse Call Devices
  and is no longer the same set as Electrical Fixtures. The Telecom pair is left
  exactly as it is, waiting on the client, and F84 reporting it is correct. Since
  Bader's answer B to Q121 the tests of those two sets pair as mirrors, F132, below

The set level number cannot be used for the damaged export check. On Infra it
gives 6, because the sets differ in how many copies of the one rule they hold,
and 6 does not read as broken.

### The picked file is corrected before a set is built, F116

- THE CLASH XML IS CORRECTED BY ITS LIST, Q104 and Q113 answered by Bader on 2026-10-04. Since
  F155 `MatrixCorrections.ReadPack` is the only way the add-in reads the matrix, the XML of the
  built in pack with the list in the same pack, at the window's open, the run, the open file
  run and both hand buttons, so the list is read wherever the XML is. `ReadPicked` reads a file
  on the disk with the list beside it by the same private Read, and no src calls it, Q165. It reads the file as `ExchangeReader.ReadFile` did, in the
  encoding the file declares, through `ExchangeReader.ReadFileText`, reads the list beside it,
  applies the code that writes the exchange file, `ForPickedFile`, before any set is built,
  and the document carries one line per correction, `ExchangeDocument.Corrections`, which the
  window writes as MATRIX lines after the line saying which file it read. The HEALTH block at
  the pick judges the corrected sets, because those are the sets the run builds
- THE CORRECTIONS ARE ONE PROJECT'S DATA, KEPT BESIDE THE XML IN ITS PACK AND NEVER IN CODE,
  Q113 answered B and F155: the tool serves many projects, and nothing in src may name one project's file.
  The list is a plain file named after the XML, its name without the extension and
  `CorrectionListSettings.Suffix`, default `.corrections.txt`, read by that name in the pack,
  `MatrixCorrectionList.InPack`, and beside a file on the disk one full path tested with
  File.Exists and never a search or a wildcard, `MatrixCorrectionList.Beside`. It holds the
  renames, the catch-all sets, the Source File rules and the workset spellings measured in
  the project's models, one a line, the format at the top of this project's list,
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.corrections.txt, kept beside the corrected XML
  so the two are copied together. The FIRST MATRIX line names the list in full and how many
  corrections and workset spellings it holds. No list there corrects nothing, the file is
  read as written, and the first MATRIX line names the path looked for. A list that cannot
  be read, a line it does not know, a rename it cannot use, bytes that are not UTF-8 in a
  list with no byte order mark or a file that will not open, corrects NOTHING and the first
  MATRIX line says why, never a part of it and never a throw. A list saved as UTF-16 with its
  byte order mark is read as UTF-16. A list that is there and HOLDS NONE, no bytes, comments
  or blank lines, corrects nothing and its first MATRIX line says so, as no list does, F116.
  `revit-worksets.txt` inside Core holds the C02 census alone, the 39 names measured before
  F116. Q113 D keeps all four categories of Q103 in this project's list
- THE CENSUS AND THE LIST ARE PUT TOGETHER IN ONE PLACE, `RevitWorksets.With`, F116 on the
  readings of the Q113 pass. The case corrections take their spellings from it with the
  list's workset lines, and the EMPTY SETS judge is handed it with every spelling the list
  names, `MatrixCorrectionList.Spellings`: the workset lines, then every spelling an also-ask
  line accepts beside its value, F131 on the readers' finding of its first attempt. It
  reaches `EmptySets.Why` through `ExchangeDocument.Worksets` and `SetBuildPlan.Worksets` to
  `EmptySetJudge`, F115, so a spelling a MATRIX line says was measured or accepted is never
  one that block calls carried by no model. The value of an also-ask line is what a set asks,
  measured or not, and is not among them. A file read as it stands is judged against the
  census alone
- A WORKSET VALUE IS CORRECTED ONLY WHERE THE LIST NAMES A SPELLING OF IT, the same but for
  its case, `MatrixCorrectionList.NamesASpellingOf`, F116 on the breaker's finding: a value is
  corrected only from what the list beside the XML says. It is then asked in every spelling
  the census and the list hold between them. A value the list names none of is left as the
  file asks and its line says so, so a list with no workset line corrects no value, as no
  list does
- WHAT THE CORRECTIONS CODE NAMES. MatrixCorrections, MatrixCorrectionList,
  CorrectionListSettings and the classes they read the file with name no set, folder,
  category or spelling in a code line. Their comments, and comments elsewhere in Core, give
  the C02 example ME-DUCTWORK against ME-Ductwork. The EXPORT CHECK line that typed that
  pair into every group is gone since F112's FR-028, which names the pairs a group has
- THE CASE WARNING OF THE EXPORT CHECK LEAVES OUT A SET THAT ALSO ASKS THE CARRIED SPELLING
  EXACTLY, F116 once F112's FR-028 merged. Since Q102 the corrections ask a workset in every
  spelling measured, so a set asks ME-DUCTWORK or ME-Ductwork and finds the items carrying
  either. `ExportCheck` names a pair only with the sets that ask the other spelling and do
  not also ask the carried one, not negated, as a whole name or, for contains, a part of it,
  and a pair no set is left for is not named. A set asking the carried spelling only negated
  finds none of those items and is still named
- A SET OR CONDITION THE CORRECTIONS CANNOT READ IS COUNTED AND SAID, NEVER THROWN, F116.
  `WrittenCondition.Read` gives null for an element that will not parse as one condition or
  holds a value it could not rewrite, `SetConditionsText.Read` then gives null for the set,
  and `ForPickedFile` counts every such set on a NOT EVERY SET COULD BE READ line. The set is
  built exactly as the file asks. A file that will not read as XML at all throws, as
  ReadFile's did
- ONE ESCAPE, ONE WAY TO EDIT A CONDITION, ONE WAY TO FIND A SET, F116.
  `WrittenCondition.Read` reads through `ExchangeReader.ReadCondition`, so a condition
  corrected and a condition built are read one way. `WrittenCondition.Escaped` is the only
  escape, for every value written and every name looked for. `WithValue`, `WithFlags` and
  `WithTest` are the only edits of a condition's text, the catch-all builds its conditions
  through them and `ValuesGiven` is the one place the category rewrite and the one spelling
  correction give a condition another value. A set
  is found by its name through `Named`, and by its folders and its name through `Key` and
  `KeyOf` where the folder matters, the Source File rule, so a set of one name in another
  folder is never given what this one asks
- A WORKSET VALUE ASKS EVERY SPELLING MEASURED, Q102. One measured spelling that differs by
  case alone is the correction, Q68. Two or more are all asked, as Or groups, where until
  Q102 they were refused and the value left as it was. A spelling no model was measured
  carrying is never asked. The spellings go in Ordinal order whichever one the file asked,
  once per workset, so the client's matrix, the one corrected before and the exchange file
  come out as the SAME sets, condition for condition, which a test proves on all three
- A VALUE CORRECTION TOUCHES ONLY A WORKSET CONDITION THAT IS NOT NEGATED, F116. A negation
  asked in a second spelling is (X and not A) or (X and not B), every X, because no element
  sits on both spellings, so `WorksetValuesIn` skips a negated condition and widening, the Or
  rows and the one spelling correction read the condition's property and flags, not its value
  alone. A condition on another property whose value reads like a workset is not a workset.
  A file holding a negated workset condition gets a MATRIX line saying it is left as asked
- THE MATRIX LINES CLAIM ONLY WHAT WAS MEASURED, F116. The spellings are the C02 census in
  Core and, in the list beside the picked file, at most ten names a group of C06 for this
  project's, so a line says every
  spelling measured so far in this project's models, never every spelling the models carry,
  and no model measured so far, never no model in this run. Each correction that changes
  nothing says which kind of zero it is, already made, nothing in the file to change, or not
  readable, and where nothing changed the last line says no correction was applied and counts
  each kind. It never says the file carries every correction, which a count of zero cannot
  tell from a file none of them acts on
- WHERE THE CORRECTIONS REACH IS SAID ON EVERY PICKED FILE, F116, the one a list corrected,
  the one with no list beside it and the one whose list could not be read. One line before
  the last says a set already in an NWF keeps the conditions it was built with and is not
  given what the file asks unless the box `SetRebuildSettings.TickLabel` names is ticked,
  Q72, and that the SETS block names each such set as DRIFTED, so a log naming corrections
  over an NWF built before them is not read as corrected sets
- THE OR ROW IS THE WHOLE GROUP COPIED, FR-025. A group asking one spelling is written once
  per spelling with the rest of the group in it, so (Ducts and ME-DUCTWORK) or (Ducts and
  ME-Ductwork). One flags 64 condition after the workset, what Q69's row was until F116,
  started a group holding the workset alone, which takes every element on that workset
  whatever its category. The text groups conditions by the plan's own rule,
  `PlannedSet.GroupsOf` and `PlannedCondition.StartsAGroupWith`, so the two cannot disagree
- A CATEGORY REWRITE CHANGES A WHOLE VALUE AND NEVER A NAME, FR-026, so a set whose name
  holds the value is not renamed and a second run counts zero
- A SET BESIDE THE ONES ASKING A SOURCE FILE CONDITION ASKS IT TOO where another discipline
  also uses its category, Q103: every AR set whose category another discipline also uses
  gets Source File contains -AR-. The condition, the folder and the category property are
  read off the file, and so are the categories another folder's sets ask for. A category no
  other folder's set asks for that another discipline's MODELS carry can only be measured,
  an AR set finding items in a group holding no AR model, and those are the categories
  after the bar in the project's list, proved by a test against the logs they came from
- THE ROW FILE NAMES EVERY WORKSET OF EVERY MODEL, one model worksets row each, because
  the EXPORT CHECK block lists ten a group and counts the rest, ten being the default of the
  setting `ReportOptions.ExportNamesShown` since F227, nought listing every one and below nought
  refused, which the engine hands to `ExportCheck.Lines`, and the spellings a run
  shows are what the workset list is measured from. Where a model's element walk stopped
  part way, `ModelExport.WalkFinished` false, the row's number is empty and its text
  UNKNOWN, `ExportCheck.WorksetCount` and `EveryWorkset`, never the names seen before it
  stopped as if they were all

### The sets compared, judged and counted, F115

- TWO FLAG BITS ARE PART OF THE QUESTION A CONDITION ASKS, FR-015: the negation, 32, and the
  start of an Or group, 64, `PlannedCondition.QuestionFlagsOf`, the one rule. A condition and
  its negation ask opposite questions, and four conditions in two groups ask an Or where the
  same four in one group ask an And no element answers. The Ignore bits are never part of it,
  because a set this tool built carries them, 37 for a negated condition in 5g, and 1A02MM's
  original import carries none, 5w, and those sets find the same items. The drift key, the
  leftover pairing and the HEALTH block's identical sets read the one rule. The one test of the
  negation bit is `PlannedCondition.NegatedWith`, read by the plan, the judge, the HEALTH block,
  the matrix corrections and the EXPORT CHECK, which wrote it inline seven times
- ONE SHAPE OF A CONDITION'S KEY, `ReadCondition.KeyOf`, FR-015, for a condition read off the
  document and one the file plans, `PlannedCondition.Key`, so the file's side is keyed in Core
  and never a second time in the add-in. `SetDrift.Compare` takes the planned set. A comparison
  other than equals and contains keeps its own name and is another question
- A SET IS SAID AS ITS GROUPS AND ITS NEGATIONS on both lines of a drift, FR-016.
  `PlannedSet.Describe` is the one describer for any conditions, by the plan's own grouping
  rule, so `SetDrift.AskedNow` brackets the document's groups and joins them with or, and
  `WantedNow` is the planned set's own. A negated condition reads not before its test,
  `PlannedCondition.TestWordsOf`, and a comparison the file never writes reads by its own name
- A VALUE THAT WOULD NOT READ IS UNKNOWN AND NEVER AN EMPTY VALUE, FR-017. `ReadCondition.Unread`
  marks it with why, `WhyUnread`, `SetDrift` then gives CouldNotRead and never Drifted, and
  `ReadCondition.KeysOf` gives no key, so the set pairs with no twin. A search that would not
  read hands its why to `SetDrift.Compare`
- `SetDrift.Lines` DECIDES WHICH SETS HAVE LINES IN THE RUN LOG AND WHAT THEY SAY: for a set that
  would not read, SET NOT READ with what would not read and why, the error's type and message as
  the add-in caught it, or why is UNKNOWN where none was handed over, for a drifted set the old
  and the new question, and none for a set asking what the file asks. The add-in's two catches
  kept neither and a run said nothing of such a set, the reviewer's finding on F115 attempt 1
- A PRESENT SET NOT READ IS NEVER SAID TO ASK WHAT THE FILE ASKS, FR-021.
  `SetBuildOutcome.AddDrift` takes every present set compared and keeps the drifted and the
  not read apart, `NotRead`, and the claim that every set asks what the file asks is made only
  where every present set was read
- A SIDE COUNT NOT TAKEN IS UNKNOWN AND REFUSES EVERY LEFTOVER, FR-013.
  `DocumentSet.SidesNotCounted` is never zero, and one set of a document whose sides were not
  counted refuses every leftover of it, `LeftoverSet.SidesUnknown`, with its own line and
  count, because a side not counted makes a set look unused and an unused set is removed
- A COUNT NOT TAKEN IS `SetResult.NotCounted`, printed UNKNOWN, FR-018. `JudgeIfEmpty` is the
  one rule for which sets are judged: one at zero items, never one whose count is UNKNOWN
  and never one whose question could not be read. A created set's count not taken is UNKNOWN
  as a present one's is, `CreatedNotCountedCount`, in no zero count and no item total, never
  summed as minus one
- EVERY CHANGE THE SETS STEP MAKES ASKS FOR THE NWF SAVE, FR-020. `PutAnythingIn` is true for a
  set created, a drifted set rebuilt and a leftover removed or renamed, the one answer. Whether
  the save itself is checked is the run log area's. The run line of what was put in is
  `SetBuildOutcome.PutInLine`, which says the sets rebuilt apart from those left alone and never
  calls a rebuilt set left alone
- EACH SETS COUNT SITS BESIDE THE KIND OF SET IT COUNTS, FR-022. The finding, zero and items
  counts of the sets created stay as they were, and the sets already there have their own,
  `PresentFindingItemsCount`, `PresentZeroCount`, `PresentNotCountedCount` and `PresentItems`.
  The summary reads for example 0 created, 61 already there (23 finding items, 38 at zero).
- A NEGATED CATEGORY IS NOT ONE THE SET ASKS FOR, FR-023, and a SET IS ONE FINDING however many
  categories no model carries it asks, `CategoryNobodyHas.Categories`, because the block counts
  sets. A group asking any of them can never match, so the set is named with how many of its
  groups ask one, and its line says a group can still match only where some group asks none.
  One finding per value, each counting the groups asking that value alone, named a set whose
  two groups asked two different missing values twice, each line saying a group could still
  match, and counted it twice, the breaker's finding on F115 attempt 1. The client's corrected
  matrix reads 13 such sets, where BLD-EL-Devices was the fourteenth for the Telephone Devices
  it leaves out
- THE SIGNATURE OF A SET CARRIES THE QUESTION BITS, FR-024, so two sets differing only by an Or
  group or a negation are not called identical. `RuleSignature` still leaves every flag out,
  because the distinct rule count reads it
- THE WORKSET LIST INSIDE CORE NOT READ IS SAID, FR-012. `RevitWorksets.Read` opens it and reads
  it inside one try and says whether it was read, a null stream, an opening that throws and a
  stream that throws each false and never a throw, `ResourceFound` carries the DLL's answer, and
  the EXPORT CHECK block says the decided pairs are UNKNOWN where it was not read. The MATRIX
  lines do not say it yet
- ONE RULE FOR A VALUE AND ITS TEST, `EmptySets.Carries`, FR-010, read by the EMPTY SETS judge
  and the HEALTH block: equals the whole value, contains a part of a name. A negated condition
  and a comparison the file never writes are not judged as a value the set asks for, by one rule
  too, `EmptySets.Judgeable`, read by both blocks. The HEALTH block kept only the negation half
  and read every other test as equals, so the two disagreed about one set
- THE LISTS INSIDE CORE ARE ONE PROJECT'S, FR-011. Each names the project its models were
  measured on on a line beginning `RevitCategories.ProjectMarker`, read by
  `RevitCategories.Project` and `RevitWorksets.Project`, and a test reads it off the result file
  of the walk that measured each. `EmptySetJudge` reads a list as a group's only where every
  model of the group names the same project, read off their names with the scan's naming
  settings, and otherwise says it cannot tell and why, `EmptySet.WhyNotTold`. A group whose
  models were not read, as on the Build sets button, is said as not read and never as names
  that would not read. Each property's internal name is typed once under src, in `EmptySets`
- A SET CREATED AT ZERO IS JUDGED, FR-027, `SetBuildOutcome.AddCreated` with the planned set, so
  a first run writes the EMPTY SETS block. The judge knows the worksets this group's models
  carry, `EmptySetJudge.GroupWorksets`, by the EXPORT CHECK's own rule, `ExportCheck.WorksetsOf`,
  the one list for both blocks, where a model whose walk stopped adds none, and a value among
  them is carried whatever project the lists are of. A nearest value differing by letter case
  alone says so in words
- A SET IS JUDGED GROUP BY GROUP AND IS WRONG ONLY WHERE EVERY OR GROUP ASKS A VALUE NO MODEL
  CARRIES, the breaker's finding on F115's third pass, 2026-10-07. `EmptySets.Why` splits the
  conditions by the plan's own grouping rule and judges each group as the whole set was judged:
  wrong on a value a complete list does not carry, cannot tell on a value a list that is not this
  project's does not carry, there where every judged value is carried. The set is wrong only
  where every group is, it is there where any group is, and it cannot tell otherwise, a group
  the judge can read nothing of, a Source File alone, among them. The judge read the conditions
  one by one and called a set wrong on the first value nothing carries, so the shape every
  also-ask line of F131 writes, (Ducts and a workset nobody has) or (Ducts and a workset the
  models carry), was told its condition is wrong. A set of one group reads as it did
- ONE UNUSED TWIN SERVES ONE LEFTOVER, the same reading. `SetLeftovers.For` hands a twin to the
  first leftover asking its question and refuses the next, because the first removes the twin
  and takes its name, so the second would then remove the first, a working set with sides, by
  that name, while both lines said RENAMED
- A REBUILD IS READ BACK AGAINST THE FILE, F115 R19. `SetBuildOutcome.AddDrift` takes the set read
  again after the rebuild, and `Took` is the one rule for a rebuild that took: found again, read,
  and asking what the file asks. One that did not is in `RebuiltNotTaken` and the lines say so.
  It still counts as rebuilt for the save, since the document changed
- A SET PUT IN AND THEN LOST ASKS FOR THE SAVE, F115 R2 and R22. `AddFailedAfterPutIn` records a
  set created or replaced and then lost track of as FAILED with `SetResult.PutIn`, its line says
  the document holds it, and `PutAnythingIn` reads `FailedAfterPutInCount`
- THE BOX IS BLAMED ONLY WHERE IT IS OFF, F115 R18. `RebuildsAsked` is set by the builder, a
  drifted set not rebuilt with it ticked is said as a rebuild that failed, `NotRebuiltCount`, and
  the old and new questions are said to be in the run log, which the window does not show
- THE SUMMARY CARRIES THE DRIFT COUNTS AND THE DAMAGE, F115 R7 and R15. `Summary()` adds the
  drifted, rebuilt and could not be read counts where any is above nought, and `Summary()` and
  `Lines()` say a document the leftover pair damaged, so the Build sets button shows it
- A SET AT ZERO NOT READ IS COUNTED IN THE EMPTY SETS BLOCK, F115 R6. `JudgeIfEmpty` counts a set at
  zero with no question read in `AtZeroNotJudged` where a judge was handed, and `EmptySets.Lines`
  counts it in the block's header with a line of its own. `EmptySets.LinesWithNoTest` is the block
  where no clash test ran, F115 R4, saying no cost was counted
- THE JUDGE NEVER THROWS ON THE NAMING SETTINGS, F115 R9, and KNOWS OF A MODEL THE EXPORT CHECK
  DROPPED, F115 R14. `EmptySetJudge.For` reads the settings through Validate first and makes their
  words the reason the project is UNKNOWN, and takes how many models the document holds, minus one
  where not read, so a workset no model read carries is one it cannot tell about where a model was
  dropped
- ONE PATH HOLDS ONE SET IN THE PLAN, F115 R23. `SetBuildPlan.From` skips a second set at a path
  the picked file already used, by name with why. `NamesInTheFile` is every name the file holds,
  the skipped among them, and is what the leftover walk reads as named, F115 R16
- THE LEFTOVERS ARE REPORTED WITH THE BOX OFF, F115 R20. `SetLeftovers.Lines` and `LeftoverSet.Line`
  take whether the box is ticked and with it off say nothing is done and what a ticked run would
  do, F115 attempt 2. A read of the walk that failed with the box off is one line,
  `SetLeftovers.NotReadWithTheBoxOff`, a FINDING and never a FAILURE
- A LEFTOVER CARRIES ITS TWIN'S PATH AND WHERE ITS RENAME LANDS, `TwinPath` and `RenamedPath`, F115
  attempt 2, so the add-in acts on the sets at the paths Core decided and never on the first of a
  name. `StoppedBecause` makes a build that stopped say so in `Summary()` and `Lines()`, and
  `AddLeftoverUntouched` counts the leftovers not acted on once the document is damaged

### The teams of the matrix, F131

- THE TEAM MAP IS ONE PROJECT'S DATA, IN A FILE OF ITS OWN BESIDE THE XML IN ITS PACK, Q114
  points 1 and 2 decided by Bader on 2026-10-04 and Q115 answered A on 2026-10-05. Its name is the
  XML's without the extension and `TeamMapSettings.Suffix`, default `.teams.txt`, read by that
  name in the pack, `TeamMap.InPack`, and beside a file on the disk one full path tested with
  File.Exists and never a search, joined by `ListFile.PathBeside`, the join
  the list of corrections is found by. It is read by `ListFile`, the one way a list beside
  the XML is read, so its lines split and its bytes are judged as the list's are.
  `MatrixCorrections.ReadPack` reads it on its own, `TeamMap.InPack`, and the document carries it in
  `ExchangeDocument.Teams`, so the window's open, the run, the open file run and both hand
  buttons read one map, and a fault in one list never leaves the other unread. This project's
  map is the pack's byte for byte copy of exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.teams.txt,
  held to Bader's map by a test. Nothing in src names a team or a code
- ONE TEAM A LINE, `team: name | code | code`, in the order a pair is written, point 12, and
  `size-folder: team | team`, the teams whose pairs carry the size folder, point 11. Any other
  code is a team of its own named by its code, Bader's rule. Codes compare Ordinal, nothing
  trimmed
- A SET NAME WITH NO CODE TAKES THE TEAM ITS FOLDER NAMES, AND UNKNOWN WHERE NONE DOES, Q117
  answered C, and A where the XML's set tree names no team, by Bader on 2026-10-05.
  `TeamMap.TeamOfSet`: the code's team where the set name carries one, `CodeOf.Set`, and where
  it carries none the folder above the set in the clash XML's set tree whose whole name is a
  team of the map, Ordinal, the folder nearest the set first, so the client's BLD-Security
  Devices in the folder Electrical is Electrical. Where no folder does, and with no map, it is
  `TeamMapSettings.UnknownTeam`, UNKNOWN, the one word `ViewpointSettings.DefaultUnknownDiscipline`
  reads. NAMED IN THE LOG: `TeamMap.SetLines` gives a TEAMS line for each set of the picked XML
  whose name carries no code of the map, with its team or UNKNOWN, said after the map's own
  lines, and none where the map maps no team. A set with no name is said as one, by the
  folders above it or at the root, as one no test side can name, so no team is read for it,
  row F114-K31, and never as a line naming no set
- A MAP THAT CANNOT BE READ IS SAID, NEVER HALF READ AND NEVER A THROW. A line it does not
  know, a code on two teams or twice on one line, a team named twice, a team with no code,
  an empty name or code, a code holding any space or a character a person cannot see, a team
  name with a space at either end or such a character anywhere, on a team line or a
  size-folder line, a size-folder line naming a team no team line names, or bytes that are not
  UTF-8, make the whole map unread with its line and why. The character is named the way
  `InvisibleDifference` names it, TAB (U+0009), because a code holding one never equals the
  code read off a file name and would make a team of its own while the map read as whole, F131
  on the breaker's finding of its first attempt. Missing, unread or holding no team the map
  maps nothing: every code is a team of its own and no pair carries the size folder. The TEAMS
  lines say which, `TeamMap.Lines`, in words held in Core
- A RUN WITH NO CLASH XML READS THE MAP THE WINDOW KEPT, Q123 answered B by Bader on
  2026-10-05. Since F155 every run reads the built in matrix and the map in its pack, so the
  kept map serves a tool with no pack built in alone. `TeamMapMemory` is one file,
  `team-map.txt`, beside the logs, holding ONE FULL PATH, `kept: path`, read by `ListFile`.
  NOTHING WRITES IT SINCE F155, which removed `Remember` and `ForPick` with the XML box, and a
  file kept before is still read. `TeamMap.Kept` reads the kept map at its path, said as the
  kept map on every TEAMS line. None kept, it maps nothing and says so. A memory that cannot be
  read, a line it does not know, a second map or a path that is not a full one, is said and maps
  nothing, never a throw, and its TEAMS line names the memory as what could not be read and
  never calls it the kept map, the reviewer's K26. `TeamMapMemory.ForRun` is the one rule for
  the map of a run: the matrix's, carried in `ExchangeDocument.Teams` by `ReadPack` or
  `ReadPicked`, or the kept one where no matrix is read, and a document read by neither is
  refused. The grey line under the XML box, `TeamMap.WindowLine`, went with the box
- WHERE THE TEAM APPLIES, Q116 answered A by Bader on 2026-10-05. The views read the team, and the log,
  COVERAGE and the form write it beside the code, `TeamMap.CodeWithTeam`, HV in Mechanical.
  The grouping, the one-discipline judgement, the alignment and export checks and the
  workbook keep the code, so HV against PL is still created and lands in Mechanical vs
  Mechanical
- A SIDE'S CODE IS READ IN ONE PLACE, `CodeOf.Set`: the first part of the set name, split on
  the set name separator, that is a known code, `TeamMap.KnownCodes`, the map's codes and
  those of the group's own models. The coverage reads through it with the views' seven
  codes, and F114's views through `ViewTeams` with the map's. A model's code stays part 5 of its file name
  through `ContainerName.Parse`, and its team is the map's
- NO CODE WAITS ON MAIN FOR A LATER STEP, Bader's answer B to Q134 on 2026-10-05. The pair of
  two teams, its one order and its size folder, Q114 points 10 to 12, are F114's, which calls
  them, so F131 holds no member for them. F131 reads the map's order and its size-folder line,
  refuses a map whose size-folder line is wrong, and says both on its TEAMS lines in the words
  of F114's `TeamPair.OrderWords` and `TeamPair.SizeFolderWords`, kept beside the rule they
  say. `TeamPair` reads the map through its public members alone: its teams in line order,
  `TeamMap.Teams`, and the teams its size-folder line names, `TeamMap.SizeFolderTeams`, which
  the map shows and judges neither
- A SET THAT CANNOT REACH A MODEL OF ITS OWN TEAM IS A SILENT MISS, FR-181, Q114 point 3,
  `SilentMisses`: a set of team T and code C and a model of team T with another code, where
  every group of the set asks, not negated, a workset the model's whole list does not carry
  or a Source File its file name does not hold. Whether a workset asked finds a carried name
  is `ExportCheck.WorksetFinds`, the one place the export check reads it too. The file name
  STANDS IN for the Source File of the model's items, which is not read, and the line of such
  a miss says so. A model whose worksets were not all read is counted as not judged, never
  called missed. It is NAMED only where the coverage count of Q112 request 2 shows the model
  holding items of the set's categories that no set catches, and without that count it is
  counted on one line that says UNKNOWN. A COUNT NOT TAKEN IS NEVER A ZERO: null or below zero,
  as `ModelExport.NotCounted` is, it is UNKNOWN, and a sum beside one not taken is a lower
  bound whose line says at least. A set asking no category by its whole name is counted on a
  line saying no count can confirm it
- A SET WITH NO CODE IS JUDGED BY THE TEAM ITS FOLDER NAMES, Q117 answered C, against every
  model of that team, each having a code other than its none, and its SILENT MISS line says it
  is in that team by its folder. The SILENT MISS line writes the set's code with its team
  beside it, as it writes the model's
- WHAT IS NOT JUDGED IS SAID BESIDE THE ALL CLEAR. A set whose name carries no code the map or
  a model of the group knows and whose folders name no team, and a model whose code was not
  read, have a team that is UNKNOWN, so they are judged against nothing, and `SilentMisses.Lines`
  counts each kind on a line of its own. WHERE NO PAIR IS JUDGED THE LINE SAYS SO AND WHY, NEVER
  THE ALL CLEAR: no set or no model handed in, a list holding none read the same as no list, or
  no model of the group of a set's team with another code. F131 on the breaker's finding of its
  second attempt, an empty list of models printing the all clear over nothing read. A condition
  whose value is empty asks no name and closes nothing, as every reader of workset values skips
  one
- EACH MODEL'S LINE CARRIES ITS TEAM AND HOW MANY SETS CANNOT REACH IT, the group's TEAMS block,
  `SilentMisses.GroupLines`: one line a model, its code with its team beside it, Q116 answered
  A, and how many of the sets of its team with another code cannot reach it, could not be
  judged, or that none of them is kept out of it by a workset or a file name it asks, so a
  model missed by many sets is one line. That last line never says a set can reach the model,
  because the judge reads only a workset and a Source File and a set closed by any other test
  is not judged, the breaker's finding. Where no set was handed in, a run with no XML, each
  model's line says whether a set cannot reach it is UNKNOWN, K27. A model of the
  group not handed in, dropped where its worksets could not be read, is counted against the
  group's model count. No model line where no map maps a team. Until F127's coverage count is
  handed in, no miss is named and each is counted as UNKNOWN until the coverage counts them
- THE CORRECTION FOR A SILENT MISS IS DRAFTED AND NEVER APPLIED, one also-ask line of the
  list of corrections, `also-ask: value | spelling | spelling`, the value and every spelling
  of the model whose text after its prefix is the value's, compared case blind. The prefix is
  split by `WorksetDisagreements.BodyOf`, where `PrefixOf` splits it, and never at the set
  name separator, which is another setting. Bader
  approves it by copying it into the list. Read there, every group asking one of its
  spellings is written once for each, through `AskEverySpelling`, the way Q102 asks every
  spelling, after every other correction. A spelling on two lines unreads the list, because
  two lines sharing a spelling ask each value where the other is or move the file on every
  run. A line written twice is kept once, and a list with no also-ask line says on its first
  MATRIX line what it said before F131. A Source File gets no drafted line. It is THE ONE WAY
  a value also asks another spelling, the Or row of Q69: `ValueOrRow`, a row nothing in src
  called, was deleted with its overload of `MatrixCorrections.Apply` on F131's second attempt,
  and its tests drive an also-ask line

## Rules the code holds

- A locator that does not resolve to a set: report the test by name, skip it.
  Never import a test with an empty side. It returns zero clashes and reads as passed
- A test where either side resolves to zero items in this model counts as skipped,
  not passed. Skipped and passed are counted as different numbers in the report
- A TEST WHOSE SIDE FINDS NOTHING IS NOT CREATED AT ALL, F77. The sets are built and
  resolved before any test is created, so how many items each locator finds is already in
  hand, and creating a test to discover a thing already known was 631 seconds of a 1424
  second run: 1830 created, 1619 of them thrown away moments later by the empty side
  check. `Federator.Core.Clash.CreationPlan` is the rule. It CALLS `ClashSideCheck` rather
  than carrying a second copy of it, and that check STAYS as the second line of defence
  before a test is run. A LOCATOR NOBODY COUNTED IS NOT A LOCATOR FINDING NOTHING: a
  count missing from what the plan is handed means the test is created and the run-time
  check answers, because the safe mistake is an extra test and the unsafe one is leaving
  a real test out of the NWF over a number nobody took. One counted line,
  `CLASH 1830 in the file, 211 created, 1619 not created, a side finds nothing`, and which
  tests they were is said by the ordinary skip block, a count and five examples, where
  every other skip reason is already said. The group's first CLASH line,
  `ClashTestPlan.PlanLine`, is written before the sides are counted, so for a picked file it
  never says how many are to be created, only how many are left after the file's own
  checks and that the count created is said once the sets are counted, FR-058. It read
  1830 to create eight lines above 36 created on set 03. Not creating a test changes what goes
  in the DOCUMENT and never what goes in the report: THE PAGE CARRIES A BLOCK FOR EVERY TEST
  OF THE REPORT and the Coverage sheet of the internal workbook lists every test of the file.
  THE CLASH WORKBOOK CARRIES A BLOCK FOR EVERY TEST WITH A CLASH AND NOTHING FOR A TEST THAT
  FOUND NONE, F153, Bader's decision of 2026-10-13, which retires the rule that it carried
  one for every test in the file, the one row of Q73's answer b among them
- BOTH TESTS OF A MIRRORED PAIR ARE CREATED AND RUN, AND THEIR CLASHES ARE MERGED INTO THE
  ONE KEPT, F132, Bader's Q114 points 4 to 6 and 8, his answer B to Q121 and his answer D to
  Q133, a rule of the code for any project. A MIRROR is a test that asks another test's
  question: its two sides are the same two sets swapped, or its sets ask the same whole
  questions as the other test's, in either order. Two sets ask one WHOLE QUESTION where the
  findspec's mode, disjoint and start and every condition's rule signature and flags are the
  same, `SelectionSetDefinition.WholeQuestion`, grouped by `SetWarnings.FindIdentical`, the
  HEALTH block's grouping handed this signature, over the picked XML's sets handed to
  `Federator.Core.Clash.MirrorRule.Of`. THE FLAGS ARE WHOLE, every bit of
  SearchConditionOptions, the negation, the Or group, ignoring case and the ignore name bits,
  docs\history\scan.md What flags 64 means, because the merge acts on this comparison and adds
  one test's clashes to another's count. The HEALTH block's own signature, `SignatureOf`, is
  not changed by F132. F115's FR-024 adds the negation and the Or group to it and keeps the
  ignore bits out on purpose, with a test that two sets apart only by them are still a pair,
  so the mirror's stricter signature is a second signature over one grouping and not a copy
  of F115's rule, and F132 does not wait on F115. Both matrices write flags 0, 32 and 64 only
  and mode all, disjoint 0 and start / on every set, docs\history\scan.md 5z-mirrors, so the
  whole question pairs the same 117 and 59 as the rule signature alone. A side is matched by
  its locator, Ordinal and never trimmed, on the key the by design pairs are found on,
  `ByDesignPairs.KeyFor`. Of each pair THE HIGHER PRIORITY IS KEPT, A before B before C before
  none, and where equal the one first in the XML, by its FileIndex. WHY BOTH RUN: probe P1
  measured that a swap can find more than the test it mirrors, 27 on the swap of a test that
  found 25, all 25 among them, docs\history\scan.md 5z-k on the branch fix-F114-probes, so
  leaving the mirror out could lose a clash. THE MIRROR'S NAME, Bader's answers D to Q133 and
  A to Q136 and the lead's note under Q136: a mirror of the XML is created under ITS OWN NAME,
  one space and the ending of `MirrorSettings`, a setting whose default is his word (mirror),
  so the mirror Y of the kept test X is Y (mirror) and never X (mirror), and it stays in
  Clash Detective. A name that already ends with the ending gets no second one. Where the
  name with the ending is taken, by another test of the XML or an earlier mirror of the same
  own name, the next number from 2 goes before the ending, Y 2 (mirror), so every mirror's
  name ends with the ending and no two tests of the plan share a name. A test the document
  already holds under it is found by it and runs as the mirror, keeping its own clashes. ON
  AN XML RUN THE RULE IS HANDED THE DOCUMENT AND THE SETS BUILD, F132 attempts 8 and 12:
  `MirrorRule.Of` takes `ClashTestPlan.FromDocument` over every test the document holds and
  this run's `Federator.Core.Sets.SetBuildOutcome`, and refuses an XML's tests without either.
  WHERE A MIRROR'S CLASHES ARE MERGED, F132 attempts 12 and 13, Bader's answer A to Q142 of
  2026-10-07: only where this run created both tests of the pair and every set of both from the
  picked XML, the document holding no test of any type under the kept test's name, the mirror's
  XML name or the name the mirror runs under, no rename landing on one of them, and every set
  either test names created by this run's sets build, `SetBuildOutcome.NotCreatedFromTheFile`. A
  set already in the document is UNKNOWN whatever the set drift check read of it, F132 attempt
  13, since that check reads each condition's category, property, comparison word and value and
  not a set's flags, negation, Or group, ignore bits or its search's base selection, and a set
  the drift check found drifted, rebuilt or not, a set that failed and a set the build never
  reached are UNKNOWN too, so no merge rests on a set this tool did not make. The mirrors of one
  kept test are judged together, so where one fails none of them merges, since a clash two
  mirrors find and the kept test does not would be added from one and still held by the other. A
  run with no XML merges nothing. Every pair not merged still runs both tests, each keeping its
  own clashes under its own name, and one MIRROR line per kept test, five named and the rest
  counted, says why and that a clash both find may be counted twice. `JudgeTheMerges` inside
  `MirrorRule` is the one place this is decided, `MirrorPair.Merges` carries it, starting false
  and set true by `JudgeTheMerges` alone, `JudgedToMerge`, F132 attempt 13, so a pair no judge
  reached merges nothing, and `MirrorMerge.Of` takes only those pairs. THE WORDS SAY WHAT IS
  DONE, F132 attempt 13: a mirror whose run name the document holds, or an old test is renamed
  to, is said run as it is saved on the pair line and in the coverage words,
  `MirrorPair.RunNameInTheDocument`, never created, and the count line and the coverage words
  say a mirror is to be merged once both tests run, never merged, since `MirrorMerge.AddTo` can
  still merge nothing at run time and says so itself. It replaced the rules of attempts 8 to 11,
  which judged a test the document held by its name, its sides and its settings and were each
  found missing a case, `%LOCALAPPDATA%\NwcFederatorLoop\turn5\lanes-1007-break11-F132.json`
  the last, so a mirror is no longer numbered past a test the document holds, no pair is
  refused for what the document holds and no rename is refused for its settings. The
  saved tests are handed once, as the tests where no XML is picked and as the document
  where one is. `MirrorSettings.NameFor` is the one place a mirror's name is made. `ClashTestPlan.WithMirrorsNamed` gives the plan those names,
  refusing a rule built over any list but that plan's own buildable tests. Attempts 5 and 6
  named the mirror after the kept test, which his words do not say. WITH NO XML A SAVED TEST
  PAIRS BY ITS SIDES AND NEVER BY ITS NAME: the ending says a test is a mirror and never which
  test it mirrors, and no name is read back to a test. A saved test whose name ends with the
  ending pairs with THE ONE saved test without it whose question its sides ask as a mirror,
  `AsAMirror` inside `MirrorRule`, the one rule the XML's tests are paired by, read over the
  sets handed, `MirrorKind.Named`, run as it is saved. The one without the ending is kept,
  never a priority or an order read again, so the roles hold from the XML run to the run with
  no XML, and a saved test with the ending is never a test kept. IT IS NOT PAIRED, UNKNOWN,
  and its MIRROR line says a clash both find may then be counted twice, where its sides were
  not read, the placeholders `SavedClashTest.LeftAsSaved` and `RightAsSaved` the add-in hands
  today, where two or more saved tests without the ending ask its question, where none does
  and for one or more whether it does is UNKNOWN for a side or a set's rule list not read, and
  where only saved tests with the ending ask it. Where its sides are another saved test's two
  sets in the same order it is a DUPLICATE and no mirror, and its line says so and that a
  clash both find may be counted twice, F132 attempt 8, where it said it asks no other test's
  question. Where no other saved test asks its question it keeps its own clashes and is said. Every saved test with the ending not paired is named
  on a MIRROR line with why, five named and the rest counted, the count line saying how many of
  the rest may have a clash both find counted twice, UNKNOWN or a duplicate, and how many ask a
  question no other saved test asks as a mirror. The two of those a measurement cannot close, two saved tests asking its
  question and only tests with the ending asking it, are left unpaired by Bader's answer A to
  Q137, each test keeping its own clashes and the log naming it and saying a clash both find may
  be counted twice. So the add-in half hands each saved side as the set it points at, through
  `DocumentSelectionSets.ResolveSelectionSource`, listed in docs\history\scan.md Job 2, and
  5z-b read both sides of all 1830 tests of a copy of 1A02WE resolving to a set, and it hands
  the sets of the document's rule lists where it can read them whole, which is UNKNOWN for the
  flags until it is measured. With no sets handed only the same two sets swapped pair, and the
  59 pairs of the corrected matrix, two sets of one rule list, are then not paired with no XML,
  said, because the add-in cannot yet hand the document's sets, as the rule after this one
  says. Saved tests whose names do not end with the ending are not paired, and one MIRROR line
  says that whether they ask one question is UNKNOWN. A mirror read off the document keeps its
  saved name. A TEST SAVED BEFORE THE MIRROR RULE, Bader's answer A to Q136: an NWF made
  before the rule holds a mirror under the XML's own name of it, Y, and with the XML picked
  `MirrorRule.Renames`, over the document handed to Of, plans each saved test under the XML's
  name of a mirror, whose sides ask the kept test's question as a mirror by the same
  `AsAMirror`, to be renamed to its own name with the ending, Y (mirror) and never X (mirror),
  `MirrorRenames`, its statuses kept, and run as the mirror keeping its own clashes, since
  this run did not create it.
  A mirror whose XML name already ends with the ending is saved under the name it runs under,
  so it is neither renamed nor refused. A rename is refused and named where the document
  already holds any test of the new name, one FromDocument leaves out too, where it holds
  two tests of the old name, and where the sides do not ask that question or were not read,
  and the refused test is left as it is and not run. One MIRROR line counts the renames and
  the refusals, then one names each. A CHANGE OF ROLES BETWEEN TWO XML RUNS, F132 attempt 8:
  where the priority file or the XML's order now keeps Y, whose mirror Y (mirror) an earlier
  run made, and the document does not hold Y, the one saved test carrying a name NameFor
  gives Y as a mirror, under no name the plan runs, with Y's two sets in Y's order, is
  planned to be renamed back to Y, its statuses kept, `MirrorSettings.IsANameFor` reading
  the name with NameFor's own pieces, so no second test of the question is created beside
  it. Two that fit or one whose sides were not read are refused and said, Y then created
  beside them. The same holds for a test the XML now runs alone. A test renamed back keeps
  its own clashes and runs at its own settings, which the DRIFT block names. A second
  MIRROR line counts those, then one names each. No saved test is renamed twice and no two
  renames land on one name or on a name the document holds. The add-in half, not built, is
  to make each rename, by the test's address, before the tests are found by name, so the
  renamed test is found under the new name and run where it sits. THE
  DIFFERENCES: a pair that differs in priority, or in any setting `TestDrift.Compare` reads,
  the tolerance within its epsilon, the test type, merge composites and each side's self
  intersect and primitive types, the mirror's side set against the kept test's side it stands
  for, is named with both values, by that one comparison and no copy of it. A pair read off
  the document has no XML name for a priority, so its priority is not compared, and its sides,
  read and asking the kept test's question, are compared side for side as every other pair's.
  THE MERGE, `Federator.Core.Report.MirrorMerge`,
  one per test kept over all its mirrors, so a clash two mirrors find and the kept test does
  not is added once, over the pairs `MirrorPair.Merges` marks alone, and ALL OR NONE, F132
  attempt 12: where one mirror of a kept test is not merged, because it did not run, is not
  in the report once, or was handed another number of clashes than its report holds or a
  clash with an item not read, none of them is, since a clash it and another mirror find and
  the kept test does not would be added from the other and still held in its own block, the
  breaker's finding 5 on attempt 11, and where the kept test has more than one mirror a
  MIRROR line says so. A clash is the unordered pair of its two items' keys, the add-in's to
  read. TWO CLASHES OF ONE TEST CAN SHARE THAT PAIR, one element carrying two pieces of
  geometry of one name, `ClashItem.MergeKey`, so a pair is counted as many times as each test
  found it: a mirror's clash past the kept test's count of its pair is found by the mirror only
  and added, and past what earlier mirrors added of it too, F132-R3, where until 2026-10-12 it
  was read as a repeat and left out, in no block once the mirror was taken out. Whether two
  copies of one pair in one test are two clashes or one read twice is UNKNOWN and unmeasured,
  and what stops a double count is the guard of AddTo, a mirror merged only where the clashes
  handed are as many as its report holds. WHICH COPY IS THE TWIN OF WHICH IS UNKNOWN, F132-R3
  attempt 2: where a pair has more than one copy under the kept test or under the mirror and
  its copies carry more than one status, no status is carried across, the kept test's stay,
  and one MIRROR line names the pair, its statuses on both sides and that the twin is UNKNOWN.
  A pair with one copy on each side is weighed as below. A later mirror's copy of a clash an
  earlier mirror added keeps the status as added, and where the two differ a MIRROR line names
  it. `AddTo(ClashReport)` is called once the run has set every test's state, and it reads
  ran off `TestReport.State`, never off a count of calls: a kept test that did not run, or is
  not in the report exactly once, merges nothing and each mirror is reported as its own test,
  and a mirror that did not run, or is not in the report exactly once, is said UNKNOWN and
  never 0 and keeps its place. A clash both find is kept once under the kept test, and THE
  STATUS IT SHOWS is Bader's answer B to Q138, `Federator.Core.Clash.BothFoundStatus`, the one
  rule: a status a person sets, Reviewed or Approved, wins over the other statuses, and otherwise
  New or Active wins over Resolved, and where his words do not choose, both a person's or both
  live, or a status not among the five, the kept test's stays, the lead's note under Q138.
  `KeptFound` takes each clash of the kept test with the row of the report that holds it, its
  own or its group's, refusing a status other than its own row's, and AddTo sets the status the
  rule gives on that row with its count by status, `TestReport.Restate`, so the workbook and the
  clash XML read it, weighing each mirror's copy in the rule's order against what the report
  holds. Every clash whose two statuses differ is named on a MIRROR line with both statuses and
  the one the report shows, his words, never five and a count, since each names a different
  clash, as the clashes only a mirror finds are each named. A clash under a group of the kept
  test leaves the group's row as it is, since it stands for every clash under it, and its line
  says what the rule gives and what the report shows. A clash of the kept test handed with a row
  the report does not hold merges nothing, said. WHAT WAS HANDED IS WHAT THE REPORT HOLDS,
  F132 attempt 8: every row the kept test holds must be handed with as many clashes as it
  stands for, or nothing is merged, since a clash of it not handed would read as found by a
  mirror only and be added a second time, and a mirror handed another number of clashes than
  its report holds is not merged and stays in the report as its own test, said, since taken
  out it would take the clashes not handed with it. A clash only a mirror
  finds is added to the kept test, marked `ClashRow.FoundOnlyByMirror` with that mirror's name,
  and a kept test that had passed then found clashes. THE MIRROR'S OWN RESULTS ARE NOT
  REPORTED A SECOND TIME: AddTo takes each mirror that ran out of the group's report,
  `ClashReport.TakeOut`, so every writer and every count over `ClashReport.Tests` holds each
  clash once. TakeOut counts each mirror it takes out, `ClashReport.MirrorsMerged`, and the
  workbook check `CreationPlan.BlockCountLine` takes that count and says how many mirrors are
  in their kept tests' blocks. Since F153 it expects one block for every test of the report
  with a clash, `ClashReport.TestsWithAClash`, counted after the mirrors were taken out. The counts of a merge are refused before AddTo has
  run, and its lines then say UNKNOWN, never 0. After it `FoundByBoth` is
  null, UNKNOWN, where nothing was merged or a mirror was not merged,
  since its clashes were never compared, and `AddedToTheKeptTest` is the rows the merge added, a
  count taken. THE WORKBOOK AND THE CLASH XML the page is drawn from write every row under
  `ClashRow.WrittenName`, which names a clash only a mirror found as found by the mirror only
  in that mirror, so two rows of one block never share a name. FAIL CLOSED: a mirror's clash
  with an item not read is not added, and since F132 attempt 10 a mirror holding one is not
  merged and stays in the report as its own test with every clash it found, so no clash is in
  no block, and its line says whether the kept test found it is UNKNOWN and that a clash both
  find is counted twice. Since F132 attempt 11 a kept test holding a clash with an item not
  read merges nothing, since a mirror's copy of that clash cannot be matched to it and would
  be added as found by the mirror only and held twice, so each of its mirrors stays in the
  report as its own test, and its line says why. A mirror is merged only where every clash on
  both sides has both items read. A group is refused, each clash under it is handed. The MIRROR lines name, for each
  mirror, what each test found, how many both found and how many the mirror only, then every
  clash the mirror alone found by its name, its mirror and its two items, then what the report
  holds under the kept test. THE COUNT CHECK OF DONE MEANS 3, `ReportedCount.Line` with the
  clashes only its mirrors found, `MirrorMerge.AddedToTheKeptTest`: the kept test's rows
  are its panel count plus that many, which the line names and explains, and only rows past
  them read THERE ARE MORE ROWS THAN CLASHES. The COVERAGE SHEET'S NAMES, F127's data, are
  `MirrorRule.CoverageNames`, each test of a pair once in the XML's order: a mirror as a mirror
  of the test kept, how the two ask one question with the sets of one rule list named, and the
  name it runs under, and a test kept with the mirrors whose clashes are added to it. A test
  with one set on both sides, or two sets of one rule list, is its own swap. A second test with
  the same two sets in the kept test's own order is a DUPLICATE and not a mirror by his words,
  named and run as before. A SIDE NOT READ IS UNKNOWN: an XML test with an empty locator,
  UNKNOWN, or a placeholder is never paired, and one MIRROR line says how many. The client's
  matrix holds no two tests swapped, and by rule list 117 pairs,
  `MirrorRuleTests.TheClientsMatrixHoldsNoSwapAnd117PairsByRuleList`. The corrected matrix
  holds 59, docs\history\scan.md 5z-mirrors, every test still created and run,
  `TheCorrectedMatrixHolds59PairsByRuleListAndEveryTestStillRuns`. NOTHING IS REMOVED FROM AN
  NWF: the removal of a mirror this tool made, Q114 point 7 and FR-183, went with Q133 D, and
  so did MirrorInDocument. Its callers are the add-in half, which waits for F131's merge, and
  until it lands nothing in src calls these rules but the writers' WrittenName and the
  workbook check's BlockCountLine, which hands the count of mirrors merged as 0
- THE ORDER THE ADD-IN HALF OF F132 KEEPS, read off the reviewer's and the breaker's readings
  of its attempt 6, `%LOCALAPPDATA%\NwcFederatorLoop\turn5\lanes-review-F132.json` and
  `lanes-break-F132.json`, built in `ClashRunner.Run` on 2026-10-07 as addin.md's THE MIRRORED
  TESTS bullet says, with one Core member added for it, `ClashItem.MergeKey`, the key of one
  item of a clash, and not yet run on Navisworks, the lead's timed runs after the merge being
  its proof. REAL SIDES BEFORE ANY RENAME AND ANY
  MIRROR CREATED: `MirrorRule.Renames` refuses a rename whose sides were not read, the plan
  then creates a new Y (mirror), and every run after refuses the rename for good, the name being
  taken, so the old Y and its statuses stay behind against Q136 A. So the change that calls
  `MirrorRule.Of` on an XML run hands it `ClashTestPlan.FromDocument` over every test the
  document holds, each saved side as the set it points at, and this run's
  `SetBuildOutcome` once the sets are built, keeping `SetResult.Asked` set on every set
  already there, since a set whose Asked is not set merges nothing, F132 attempt 12, and in
  the same change makes every
  rename of `MirrorRule.Renames` by the test's address before the tests are found by name,
  and creates no mirror before it. EVERY CLASH HANDED: AddTo merges nothing of a kept test
  whose rows were not all handed with as many clashes as each stands for, and leaves a mirror
  handed another number than its report holds as its own test, so the add-in hands every
  clash of both, each clash under a group on its own, before AddTo. THE DOCUMENT'S SETS WITH NO XML: the 59 mirrors of the corrected matrix pair
  only on their sets' rule lists, handed to `MirrorRule.Of` as
  `Federator.Core.Exchange.SelectionSetDefinition`, whose constructor is internal to Core and
  called by `ExchangeReader` alone, InternalsVisibleTo naming only the tests, so the add-in
  cannot make one. Until a public Core member takes what the add-in reads off the document's
  sets, with its test, every weekly run leaves the 59 unpaired and said. Since F132 attempt
  12 a run with no XML merges nothing, so this decides only whether the log names them as
  pairs, and each clash both find is counted twice either way. Whether the flags can be read whole off the document is UNKNOWN until
  measured. AFTER AddTo AND NEVER BEFORE: the kept test's ROWS line, `ReportedCount.Line` with
  `MirrorMerge.AddedToTheKeptTest`, which the runner writes per test before any merge today,
  the pictures, whose choice reads each row's status and whose numbers
  `ClashReport.ImageIndexFor` hands out, and the BLOCKS check with `ClashReport.MirrorsMerged`.
  `ClashReport.TakeOut` leaves a gap in `TestReport.Number`, which `AddTest` after it repeats,
  read today by `TestReport.ToString` alone. The BLOCKS check is handed minus one where no XML is
  picked, FederationEngine's `CheckTheWorkbook`, so it never prints on a weekly run, where the
  pairing by sides happens, and since F132 attempt 12 no mirror is taken out there. THE
  RENAME'S STATUSES ARE NOT MEASURED: docs\history\scan.md lists
  `DocumentClashTests.TestsEditDisplayName` and nothing has run it on a test holding results and
  statuses, so the add-in counts the test's results and statuses before and after the rename, as
  F50's rebuild counts what the NWF carries, and the Renames lines saying statuses kept, the
  renaming back of attempt 8 among them, are a plan until then. ATTEMPT 2 OF THE ADD-IN HALF,
  2026-10-08, on the breaker's blocking findings R4 and R5 of attempt 1. THE CLASHES OF A TEST
  ARE THE MERGED REPORT'S ROWS FOR IT, WITH THEIR ROW STATUSES, `Federator.Core.Views.ReportClashes.Of`,
  one rule in one place for the per clash views, which the add-in resolves and writes, and for
  the one view per test F114's add-in pass will build over the same rows: every row of every
  test the report holds rows for, in the report's order, under the test the report holds it
  under, at the row's status, named as the workbook writes it, `ClashRow.WrittenName`, so a
  clash only a mirror found is viewed under the kept test and says which mirror, and a test
  name the report carries twice is read once and named, `ReportClashesOutcome.NamedTwice`. The
  camera, the two items and the size are the document's, which the add-in reads off the result
  it resolves by the address the harvest recorded for the row. Test first, ReportClashesTests,
  eight tests. THE BY DESIGN PASS SAYS WHERE ITS SIDES CAME FROM: on a run with no XML the
  add-in calls `ByDesignTally.SidesReadOffTheSavedTests` with the saved test count and the pairs
  in the file, and the block's first line reads `sides read off the NWF : the N tests saved in
  the document, no clash XML picked, so this pass acts on the saved tests, and they reached K of
  the P pairs in the file`, a pair counted once however many clashes reached it, and nothing of
  it on a run with an XML. `ByDesignPairs.ConfirmLine` is the confirm screen's line with the box
  on and null with it off, the rule `SetRebuildSettings.ConfirmLine` keeps, saying the pass
  acts on the tests created from the picked file and on the tests already saved in the NWF.
  Four tests in ByDesignRuleTests
- The Revit container inside an NWC is often a different building from the NWC.
  Where the building code parsed from the NWC name differs from the code in the
  Revit source name, report SOURCE MISMATCH naming both, and where one Revit
  building code was published under more than one NWC building code, report SHARED
  SOURCE, because that means one building has been split in two by a naming error.
  SHARED SOURCE counts the building codes on the NWC names and never the groups, FR-167,
  since the group is the grouping a person chose: per building and discipline one
  building is several groups by choice, and per discipline several codes share a group.
  Neither finding says what the federation will be called, which only per building is
  the code. Both codes are read with the
  same parser used on the NWC names, never a separate rule. This is information. It
  does not block, unpick or merge anything. Bader decides
- When tests skip for the same reason, log the count and at most five examples, then
  the total. Per test detail stays for tests that were created or run. One run wrote
  1830 near identical SKIPPED lines and a 1 MB log, which buries everything worth
  reading
- THE RUNNING COUNT IS TAKEN AFTER THE TEST IT NUMBERS, F113. `ClashRunOutcome
  .ProgressAfter` is the line the log carries at every twenty fifth test and at the last
  one, twenty five being the default of the setting `ProgressSettings.CountEveryTests` since
  F227, refused below one where it is set, which ClashRunner reads off its Progress, so a group that runs to its end ends on a count, and it reads the counts as they stand.
  A group with no test to run writes no count, and one the guard stops ends on RUN STOPPED. The
  runner took it BEFORE the test it numbered until F113, so on the run of 2026-10-01 the
  last count of all 22 groups was one test short of the block under it, and three were
  short of the block's clashes by the last test's own, 1624 against 1629 in 1B06PK. The
  last count of a group and the block read the same numbers
- A test already in the document is left as it is, which means a tolerance changed in the
  XML never reaches it. That is right and it was silent, so now it is REPORTED. Every
  test in both is compared on the tolerance, the test type, merge composites, and per
  side the locator and the self intersect and primitive type flags, and every difference
  is named by test name with both values. Comparing changes nothing on either side.
  Tolerances are compared within an epsilon because the file's number travels through a
  unit conversion, and an exact comparison would report drift on all 1830. Locators are
  compared Ordinal and never trimmed, because two set names in the reference file end in
  a space. A side this tool could not read comes back as the word UNKNOWN, which is a real
  string and compares like any other, so it is left OUT of the comparison and counted as
  not compared. The block says how many, because not compared is not the same as matching.
  The marker lives in Federator.Core.Clash.TestSettings.UnknownLocator, where the
  comparison can read it
- Resolved clashes stay in the file forever and that is the point of them, so the count
  is reported per test and as a total for the group. A test running weekly for months
  accumulates them without limit and nothing else prunes them
- Nothing removed and nothing asked to be removed are different, so the compacted count
  is minus one until a compact actually runs. Zero removed is a real answer. The count is
  the Resolved total read BEFORE less the Resolved total read again after the compact, so
  it is measured rather than assumed, and the log carries both numbers
- Per test, everything comes from the file and never from a constant: the name, the
  test type, the tolerance, merge composites, and per side the self intersect and the
  primitive type flags. A test whose tolerance attribute is not there at all is SKIPPED by
  name, the way an unknown test type is, because a missing tolerance became zero and zero
  reads as a real tolerance. A tolerance written as zero is a real one and is created.
  THE OTHER THREE THE SAME WAY, FR-155: a test whose file leaves out merge_composites, or a
  side's selfintersect or primtypes, is SKIPPED by name as `ClashSkipReason.MissingSetting`,
  the reason naming each left out through `ClashTestDefinition.MissingSettings`, because each
  read as 0 or false with no word said. Any of them, or a condition's flags or a findspec's
  disjoint, written but not a whole number is REFUSED by `ExchangeReader` with an
  InvalidDataException naming the attribute and the test or the set, as a tolerance written
  wrong is, and `WrittenCondition.Read` reads such a condition as one it cannot read. Flags and
  disjoint left out still read as 0 and false, and whether Navisworks reads a condition with no
  flags the same way is UNKNOWN
- THE TOLERANCE IS CHOSEN IN THE TOOL AND BEATS EVERYTHING, F76, AND THE DROP DOWN IS ITS ONLY
  SOURCE, F155, Bader's decision of 2026-10-13. The Clash step carries a drop down,
  `Federator.Core.Clash.ToleranceChoice`, offering 25 mm, 50 mm and 75 mm, `OfferedMillimetres`,
  with 25 mm selected when the window opens, `DefaultMillimetres`. Its value is set on EVERY
  test in the run, the ones created fresh from the matrix and the ones already saved in the NWF
  alike, and it beats the matrix and it beats the document. The tolerance written in the XML is
  never read into a run: the parser still reads the attribute, and a test without it or a file
  with no units is still skipped, FR-155, Q167, but the value reaches no test, no report row and
  no drift line, ClashRunner putting the chosen value on each. The entries Use the value in the XML and Other,
  Other's number box and the reading of typed text are gone with their tests.
  ApplyFileSettings keeps exactly the meaning it had, which is whether the FILE'S settings
  reach a saved test, and this wins over it. The choice is in MILLIMETRES because that is what
  a person says, converted through UnitTable like any other, and a unit the table has not been
  taught throws rather than falling back. `ToleranceChoice.Of` refuses a negative, NaN or
  infinite value. The drop down is read by `ToleranceChoice.FromThePicker`, which never throws,
  FR-166: nothing selected hands back null and `ToleranceChoice.NothingSelected`, so only a run
  that creates or runs clash tests refuses on it, and a button that never reads the tolerance
  hands `ToleranceChoice.Default()` and is not stopped by it. The confirm screen says the value,
  the counts, what it beats, and what changing a saved test actually costs, which is that it
  finds DIFFERENT clashes next run and NOT that its results are reset, 5y, on every run, since
  a value is always chosen. One TOLERANCE line per group carries both counts and
  the converted value. Under Set on a saved test counts only where its tolerance was
  changed, FR-053, and every other saved test is named on the same line by what happened
  to it, already at the value, not found, would not copy as a clash test or threw,
  `SavedTestTolerances`, recorded once per test whichever way it left. The line is told the
  plan's source, FR-063: with no matrix read it says the tolerance is the one each saved test
  holds and a chosen value beats the document alone, and never that an XML was read or
  beaten. Why it exists: the matrix is written at 25 mm, the NWFs on disk
  held tests at 75 mm, a test already in the document is left exactly as it is, so the
  run clashed at 75 while everybody believed it was clashing at 25
- THE REPORT READS ITS TOLERANCE OFF THE CLASH TEST IN THE DOCUMENT, F76, because the
  document is what produced the row. It used to be copied off the PLAN, which is the
  XML, so on every weekly run the cell said what the file asked for and the run had
  clashed at something else. `ClashReportModel.TestReport.ToleranceFrom` carries WHERE it
  was read, an origin of Document, File, Tool or Unknown, kept for the same reason the
  Item ID keeps which property supplied it: a number is only honest while what produced
  it is still visible. `ToleranceChoice.ReadFromLine` counts the four across the run, so
  a report reading the file rather than the document shows as a number rather than being
  trusted
- A ROW WHOSE TEST CARRIES THE CHOSEN VALUE SAYS IT, FR-036, F118. The row is given the plan's
  value before the test is touched, and a test that then does not run, one discipline, off
  the coordinates or a side finding nothing, never reaches the reading off the document. So a
  saved test set to the chosen value and then skipped said its old value and origin Document,
  and a test created at the chosen value said the XML's. `ToleranceChoice.PutOn` gives the row
  the chosen value in the document's units and origin Tool, where the test was created at it,
  set to it or already at it, and does nothing under the default, where a row of a test that
  never ran keeps what the plan gave it, Bader's answer to Q54. A test that runs is still read
  off the document. `ToleranceChoice.BlocksLine` no longer says the blocks that never ran read
  off the picked file, and points at the line below it, `ReadFromLine`, for where each was read
- THE TOLERANCE IS THE DROP DOWN'S ALONE, F167, Bader's answer A to Q167 of 14 Oct 2026. The
  value chosen on the Clash step, 25, 50 or 75 mm, is converted into the document units by
  `ToleranceChoice.InDocumentUnits` and set on every test, a test of the matrix and a test
  already saved in the NWF, whatever the matrix, an XML or the test says. Nothing reads the
  tolerance or the units of a matrix or an XML any more: the reader skips both, the plan carries
  none, and a test with no tolerance or a file with no units is planned like any other. The two
  checks that skipped them, FR-155's no tolerance and UNKNOWN UNITS, and their skip reasons are
  gone. A tolerance in the matrix is data nobody reads. Which units ClashTest.Tolerance is
  measured in is UNKNOWN until a test runs against a real model
- A test type that does not map to a value on the enum is reported by name and
  skipped. It is never approximated to the nearest one, because a hard test standing
  in for a clearance test reports a number that reads as real and is not
- A run that is failing everything stops the run, not the group. After the first 50 tests,
  if every one has failed for the same reason, the whole run stops and says so in one
  line. Every group of that nine hour run failed the same way, so a per group stop would
  have saved none of it. The 50 is a setting, ReportOptions.StopAfterFailures, and the one
  guard the whole run shares is built from it. A test skipped because a side finds nothing
  is the ordinary answer and counts neither way
- An exception repeating with the same heading and the same trace is written out in full
  once and counted after that, and the RESULT block carries the total beside the one
  trace. One run wrote the same stack tens of thousands of times into a 17.8 MB log
- Every picker returns a FOLDER to open at, including the one that chooses a file, and the
  caller never takes the parent of it. Doing that opened the clash XML picker one level
  above the folder it had remembered. The rule lives in
  Federator.Core.Diagnostics.PickerStart so all three, the logo, the parent folder and the Report folder since F232, can be proved rather than clicked
- Every picker remembers its own last folder, in a file beside the logs, and reopens
  there next time. Per picker and never one shared, because with one shared, picking an
  NWD folder moves the source picker to it and the next run reads the wrong folder. A
  remembered folder that has gone is not an error and is never cleared, the picker opens
  at the nearest parent still on disk and says so, which is what happens when a project
  drive is not mounted yet. Remembering never stops a run: an unwritable location is
  recorded as a reason and the session still remembers, it just does not survive a restart.
  AND IT IS SAID, FR-162: `FolderMemory.Remember` hands back one FOLDERS line naming the
  picker, the file and the reason where the save failed, once until a save works again, and
  null otherwise, and the window logs it. It dropped what Save said, so a session's picks were
  gone at the next start with no word
- No code identifier and no framework message ever reaches a label. It said
  "Workbooks go in UNKNOWN" before a scan, with "Parameter name: nwfFolder" behind it,
  because an empty NWF folder threw and the message went straight to the window. Nothing was
  wrong at that point, the folder had not been picked. A failure DIALOG may carry the
  exception message, because that is what gets sent back. A label may not. Every wording a
  label carries lives in Core where a test can read it, and the test hands it the thing the
  label must not carry and asserts it is not in what comes back. RunLog.WhereTheLogIs, NoLogFileOpened, ErrorsAreInTheLog and
  TheLogSaysWhy for anything about the log, so a run that could not open one names the
  folders it tried and the log line keeps what threw. RepeatedFailureGuard.Reason for the
  log and ReasonInPlainWords for the label, so the stopped run says how many tests failed
  the same way and never what they threw
- Findings are written in the words a person would say, never in the words the tool
  thinks in. A shape is described by naming a real code that has it, not by printing
  9A99AA. A shared Revit source says two NWC building codes were published from one Revit
  building, names both, and says that usually means one NWC file name is wrong, FR-167. The
  short code stays as its own column so it can still be sorted on
- A group holding fewer than two disciplines cannot clash with anything, whatever the
  test list says, because every clash test is one discipline against another. One NWC is
  the plain case and two NWCs of the same discipline are the same case, D5 on
  2026-09-12, so the count is of disciplines and not of files, in
  Federator.Core.Grouping.BuildingGroup.CannotClashWith, read by the window row and the
  engine alike, and none of them is run. SINCE F77 THEY ARE NOT ALL CREATED EITHER,
  which is a change from the rule this bullet used to state. That rule said every test is
  still created so the NWF is complete and matches the other groups. It cost 631 seconds
  of a 1424 second run to create 1619 tests that were thrown away moments later, and Q46
  asks whether the completeness was worth it. The reason is SingleDiscipline and it is counted apart from EmptySide on
  purpose. A side finding nothing says a discipline was not exported. One discipline says
  the group was never going to clash and no export would change that. In the last real
  folder that was 1B06BS and 1C06PK, and both ran 1830 tests for nothing. The document
  already open is never judged this way, because nothing was scanned and its count is UNKNOWN.
  A PICKED NWF, THE NWF PAGE'S RUN, IS JUDGED THE SAME WAY SINCE F219, Bader's answer to Q203 of
  18 Oct 2026, a group of one discipline running its tests on neither page: once it opens, and
  only where its tests are asked for, its disciplines are read off each model's own file name,
  Model.FileName and never its source, by the scan's own call, ContainerName.Parse handed the
  whole path, `BuildingGroup.DisciplinesOf`. Where a model has no file name, a name will not read,
  the naming settings refuse, or a model is named with the tool's own federated discipline, BM by
  default, which holds disciplines its name does not say, the count is UNKNOWN, said on a CLASH
  line, `DisciplinesNotReadLine`, and its tests run as before. Both pages say why no test ran in
  one line, `BuildingGroup.OneDisciplineLine`, which names the discipline. A picked NWF refreshed
  before F219 can still hold an earlier run's results for those tests, row F219-R1.
  SINCE F233, BADER'S ROUND OF 19 OCT 2026, FIX 3, A GROUP OF ONE DISCIPLINE IS NOT BUILT, on both
  pages: nothing appended, no NWF, NWD, Excel, Internal Excel, HTML, picture folder or viewpoints.
  The words are `Federator.Core.Communities.OneDisciplineGroups`: one NOT BUILT line a group naming
  its community, building, discipline and file names and `one discipline, not built`, a second
  naming an NWF of it an earlier run left, never deleted and never run, and RESULT's `not built`
  line a community with the count, `PackageTally.NotBuilt`, such a group in no count of done,
  partial or failed and never one with no end to count. On the NWF page its disciplines are read
  once its NWF opens, whatever steps are ticked, before the refresh, and a group of one stops
  there, `RunLog.GroupNotBuilt` closing it on one GROUP line, the line carried to the window that
  started the run as its result's reason, `OneDisciplineGroups.IsTheLine`, which names the NWF of the earlier run too. So the
  F219 reading above, only where its tests are asked for, and `OneDisciplineLine` with the SingleDiscipline skip are no longer
  reached on either page, row F233-R8. AN NWF LINKING A FILE NOT ON THE DISK IS NOT JUDGED, the breaker's finding on F233: a
  model Navisworks could not load may be among no models, so a group of two disciplines would read as one, and
  `OneDisciplineGroups.NotJudgedLine` says so and the group runs as before, the refresh stopping on the file. The words are
  the grid's status word, `NwcPageRule.OneDiscipline`, read once. RESULT of the window that started the run counts them on its
  own line, `one discipline :`, so the counts of done, partial and failed add up, and the live line says them apart and never
  warns of them, `PackageRunWords.Finished` and `Warning`. A run of the NWC page whose every group is of one discipline still
  writes RESULT. Each is said and counted by the window a person runs alone, never again in a group's own Navisworks
  SINCE F224, BADER'S ANSWER TO Q209 OF 18 OCT 2026, ONLY TESTS ACROSS TWO SET CODES RUN, inside
  a group of two or more as well, on both pages, by the one plan step the engine reads before it
  creates any test, `ClashTestPlan.AcrossTwoSetCodes`. BLD-ST against BLD-ST and BLD-AR against
  BLD-AR are left out, BLD-PL against BLD-DR and BLD-HV against BLD-FP still run. Each side's code
  is read by `SetCodeRule.SameCodeOf` through the one reader, CodeOf.Set, off the set name, against
  the codes the run knows, the team map's and the group's own models'. A test is left out only
  where both sides read a code and the two are the same, so a side whose name carries no code,
  such as BLD-Security Devices, leaves its test running, never left out on a guess. The reason is
  SameSetCode, counted apart from SingleDiscipline, its coverage row says not created or run, and
  every group writes one CLASH line, `SetCodeRule.Line`, saying how many of how many planned it
  left out by code, a nought said and no code known said apart, and how many of the tests that run
  have a side whose set name reads no code, `SetCodeRule.WithASideOfNoCode`, so a nought is never
  read as every test judged. In a group of one discipline Q203 runs none of its tests, so the rule
  is not read and its line says so, `SetCodeRule.NotReadInAGroupOfOne`. The run knows it did not
  create a test the rule left out, so the count check reads it held by neither side where the
  document does not hold it. The built in matrix loses exactly the 284 tests Q209 counted, AR 120,
  EL 78, ME 36, ST 15, FF 15, PL 10, DR 10. The words of the workbook and the window carry no
  question number
- The reports never go inside the folder being scanned, whether it was picked or
  defaulted to. A run put the workbooks in the NWC folder it was reading, and the
  Clash step picks an XML at run time, so a report written there is a file a later run
  can be handed as its own input. A picked folder inside the source is refused and the
  default beside the NWF folder is used instead, saying so. Where the NWF folder is
  itself inside the source there is nowhere safe, and that is said rather than written
  somewhere surprising
- Outputs overwrite, the NWD every run and the NWF only when it is being built for
  the first time. No version suffix, and no date suffix, the weekly record tick
  box that dated the NWD gone since F232, `OutputNaming.DateTheNwd` left at its default, off
- The NWF and the NWD are dated differently on purpose, and this is the reason. The
  NWF holds the clash tests and every clash result inside it, so it IS the history and
  overwriting it in place is what keeps that history. A dated NWF would fork the
  history: this week's clashes would go in one file, last week's Active and Resolved
  would be stranded in another, and no file would hold the whole picture. The NWD
  carries no clash results at all, it is the model as it stood, so a dated NWD is a
  weekly record that costs nothing and loses nothing. One is a ledger and one is a
  photograph
- The date goes in the number field, because the number never advanced anyway. It is
  a format string and a setting, DateFormat, defaulting to yyyyMMdd. It is not a
  fixed string and never a fourth name pattern
- DateTime.ToString does NOT throw on a format string nobody can read, it treats what
  it does not recognise as literal text, so "not a real format" comes back as
  "noA a real 0or0aA". Measured, see docs\history\scan.md section 4j. So what is checked is
  what the format PRODUCED, not whether it threw. A result that is empty or holds a
  character Windows refuses in a file name falls back to the number. Anything else is
  used as typed, because the format is the person's setting and their mistakes should
  be visible in the preview rather than silently corrected
- THE MESSAGE BOX OF A BAD NWC IS ANSWERED AND ITS GROUP ENDS FAILED, F213, F210-R6, Bader's message of
  18 Oct 2026. `Federator.Core.Rerun.AppendBox` is the rule the add-in's watch reads: a window is a message
  box only where its class is #32770, `IsAMessageBox`, the one test of the class, and it is the box an append
  of a file opened only where it is one, holds exactly one Button and a Static whose text names the file as a
  whole name, compared without case, `IsTheBoxOf`, so a box of my-a.nwc is never the box of a.nwc. It is
  answered with that button's control id, `ButtonToAnswer`, never IDOK, which scan.md 5z-zi measured leaving
  the box standing. A box of two buttons asks a choice and no box naming another file is answered. `Reason`
  is what Navisworks said, the first line of the text with the file's name taken off, UNKNOWN and never empty
  where it cannot be read, and `GroupError` names the full path, the reason and that the tool answered, with
  no stop at its end. A FILE NAVISWORKS REFUSED IS A FACT AND NOT AN ERROR,
  `GroupFacts.FilesNavisworksRefused`, judged FAILED by `GroupJudgement` straight after the errors and said
  beside them, so no step of a group stops on it and a rebuilt group goes on to its clash, its NWD and its
  reports as it did with the file missing, the breaker's and the reviewer's finding on F213's first attempt.
  `AppendBoxSettings.EveryMilliseconds`, 200, refuses less than 50. Proved by `AppendBoxTests` on the two
  texts of 5z-zi, the random file and the cut file giving one and the held file the other, and by
  `GroupJudgementTests`
- A group ends in one of three states, and the test is always what was ASKED FOR,
  never what happens to be on disk. A step deliberately switched off is not a
  failure. Judging a group by whether an NWD existed, with republishing switched
  off, once reported all 22 groups of a clean run as FAILED.
    DONE     everything requested for this group succeeded
    PARTIAL  something requested did not complete, or the group was CHANGED and
             left alone, which since F24 means only a group whose rebuild never
             started, because a rebuild that ran ends as Rebuilt, or its clash was
             skipped because a model is not on the same shared coordinates
    FAILED   something requested threw or produced nothing, a rebuild that appended
             nothing or could not keep its saved tests included. A model on Internal is
             no reason since F171, Q170, and a model naming no shared site is no reason
             since F176, Q172: both are a NOTICE that names the model
  The rule lives in Federator.Core.Rerun.GroupJudgement, with no Navisworks types
  in it, so it can be tested. The outcome and the reason for it come out of one
  pass, so the two can never disagree. Where the models sit is no fact the judgement
  is handed, `GroupFacts.AlignmentFailure` is gone since F176, and the group is judged
  on its steps alone
- AN NWF SAVE IS JUDGED BY WHAT THE SAVE SAID, as the NWD publish is, FR-043 to FR-045.
  `GroupFacts.NwfSavesAsked` counts every save of the NWF this run asked and
  `NwfSavesThatFailed` those that returned false or threw. Any failed save is FAILED with
  the reason that the NWF does not hold what this run put in, and how many of how many
  where there were two. A Rebuilt group that saved none is FAILED with the reason that its
  NWF keeps the old file list, because the NWD carries the new one. An opened group that
  saved none is not judged on it, since the NWF it opened is the one it leaves
- A GROUP NOT ON THE SAME SHARED COORDINATES SKIPS ITS CLASH AND NOTHING ELSE, Q98 B2
  and Bader's answer to Q99 and Q100 on 2026-10-04. A model is not on the same
  coordinates when it names Internal as its shared site, when its site was read and names
  none at all, his answer B to Q111 on 2026-10-05 built by F137, or when it sits more than a
  metre from its group's reference model, as the ALIGNMENT block measures it. That
  replaced Q70 for a model on Internal or naming no site only where the group's clash was
  skipped, and since F159 nothing is skipped. SINCE F171 (Q170) A MODEL ON INTERNAL NEVER
  FAILS ITS GROUP, AND SINCE F176 (Q172) NEITHER DOES A MODEL NAMING NO SHARED SITE. The clash
  always runs. Each is a NOTICE by name, the models in `OffCoordinates.OnInternal` and
  `OffCoordinates.NoSite`, said by `OffCoordinates.InternalSaid` and `OffCoordinates.NoSiteSaid`,
  in the run log, in the list of models off the coordinates, in the RESULT lines and in the
  group's result line (`FederationEngine.Describe`). `AlignmentCheck.WhyItFailsTheGroup`, its
  FAILED line in the block, `AlignmentCheck.FailedRunLine` and `GroupFacts.AlignmentFailure` are
  gone with the failure. Every other reason for FAILED stays. A site whose
  read threw is neither, FR-002, and is not judged. `AlignmentCheck.NamesNoSite` is the one
  test of a model naming none, and its line in the block, the note and the list for the
  modellers says the model names no shared site at all. His answer B to Q125, PARTIAL with
  the reason models not on the same shared coordinates for a group that runs no clash test
  with such a model, is not built, and is moot since no group fails or is partial on where its
  models sit. The notice is made at the ALIGNMENT step, before any file of the group is
  written, so it names the models and their sites and NOTHING OF A FILE. The distance is the STRAIGHT LINE of dx, dy and
  dz and never each axis on its own, because 1B06WM's ME in the C06 run sat 1.206 m away
  and under a metre on every axis. The metre is a setting, `ReportOptions.FarModelMillimetres`,
  defaulting to `AlignmentCheck.DefaultFarModelMillimetres`.
  SINCE F159 NOTHING IS SKIPPED, Bader's order of 2026-10-14. The option that skipped the clash of
  a group holding such a model, its tick box, its help line, `ReportOptions.SkipClashOffCoordinates`
  and everything that read it are gone, and the clash tests always run, whatever the coordinates
  are. The check itself stays. A model off the coordinates is named under a NOTICE and nothing
  else changes: the ALIGNMENT block says `NOTICE.` and which models, the run log carries a
  `NOTICE` line for the group made by `OffCoordinates.Notice`, and the run's list, the
  "Models not on the same shared coordinates, run" file in the Log folder, carries a NOTICE
  for each group with the reference, the models and that the clash results may be wrong for
  them, `OffCoordinates.ResultsMayBeWrong`. A model naming no site does not fail its group,
  Bader's answer to Q172 built by F176, and neither does one naming Internal, Q170 built by
  F171, and the notice says so.
  One line per such model, made by `AlignmentCheck.NotOnTheSameCoordinates` and nowhere else,
  names its file, its shared site and its distance from the reference in X, Y and Z. The
  ALIGNMENT block's all clear line is written only where every model was measured, and
  otherwise says how many were not.
  ONE TALLY PER RUN, `OffCoordinatesAcrossTheRun`, made by the engine when Run or
  RunOpenDocument starts with that run's start and count of groups, and handed by the window
  to that run's RESULT block, `RunLog.WriteResultBlock(thisRun)`. It is never kept on the
  log, which lives as long as the window, because a second run of a window once listed the
  first run's groups. RESULT and the list say how many groups the check judged, did not
  reach and could not read, and how many models it could not judge, and never say no model
  was found off while any of those is more than nought. Each run's list is its own file,
  `ListName`, named for the second the run started, and a name already in the Log folder takes
  a dated one, so a smaller run never writes over a fuller run's list. A model whose
  placement or site could not be read is never judged on it, and the block says so
- The EXPORT CHECK run line is added up in Core, `ExportCheckAcrossTheRun`, by the rules
  each group's block judges by, `ModelExport.HoldsNoElement` among them, so the line and
  the blocks cannot disagree. The engine kept four counters of its own and none for a model
  holding no Revit element. A group whose whole read threw is counted from the group's catch,
  `ExportCheckAcrossTheRun.GroupNotRead`, and the line says so and is never clean while one
  is. A model `ModelFactsReader.Exports` drops in its own catch is in no count, so the line
  can read clean over it, a known bug of F112. The row file's number for a model whose
  elements could not be counted is empty, `ExportCheck.ElementsNumber`, as for any unknown in that file, never -1
- The open file run's window label takes its clash counts from Core,
  `ClashRunOutcome.CountsForTheLabel`, which says none ran and why for a group whose clash
  was skipped, where the label printed nought run and nought clashes
- The counts in the RESULT block and the errors under it come from one list. A
  failed count with an empty error list is what the log printed once, saying
  "groups failed: 22" and "Nothing failed." in the same block. A group recorded as
  failed always carries a reason, and one is substituted rather than thrown over
  when a caller forgets, because logging never stops a run
- A file written twice reports the size of the SECOND write. The list holds one entry per
  file so the count stays a count of files, and the SIZE is the last one read. The NWF is
  saved twice, once after the append and once after the clash step, and keeping the first
  size made a run report it at 4,141 bytes, an empty federation, when the second save had
  read 165,844 off the disk four minutes earlier. Nothing had shrunk. It read exactly like
  an NWF that had lost every clash result in it, which is the most alarming thing this log
  can say, so it must never say it by accident
- Only a file this run actually wrote goes in the files written list. Outputs
  overwrite with no date suffix, so last week's NWF and NWD sit at exactly the
  paths this run uses. A group that threw before writing anything must not list
  them as its own, and a file that was checked rather than written is logged with
  CheckOnDisk, which reports the size and records nothing. A file this run removed
  leaves the list through WriteRemoved, and only once it is gone from the disk
- THE PICTURES ARE COUNTED BESIDE THE FILES WRITTEN AND NEVER LISTED, FR-059. Each group's
  `ImageTally` is added to `RunLog.PicturesAcrossTheRun` as its IMAGES lines are written, and
  RESULT says the count and the MB straight above files written, from sizes the writer read
  back off the disk, and that the logo copies are on neither line. The copies of the log F158
  makes into each Log folder are made after RESULT, so the line names none.
  Set 03 said 88 files written and left out 5679 pictures, 1376 MB
- THE CLASH TOTAL COUNTS A GROUP THAT RAN NO TEST APART, FR-060. `ClashesAcrossTheRun.Add`
  takes the tests run, and a group at zero clashes that ran none reads `of which ran no test`
  and its row `no test ran`, never `found none`, which is kept for a group that ran a test.
  Set 03 counted six one discipline groups, every block reading tests run : 0, as found none
- THE WARNING BEFORE A CLEAR IS CORE'S, `Federator.Core.Rerun.DiscardWarning`, FR-169. A file
  name or a model count that would not read is said with the reason the read gave, and a
  document whose name would not read is never called the current unsaved document
- A WRITE WHOSE OWN CALL SAYS WHETHER IT WROTE HANDS THAT TO THE LOG, FR-189 and FR-044.
  The save of the NWF and the publish of the NWD call `RunLog.WriteFinished` with the bool
  the call gave, false where it threw. A file there after a call that did not report
  success is read and said on a NOT WRITTEN line with its row, and is never added to the
  files written list, since it is last week's file. A file an earlier write of this run put
  there, the NWF of its first save, keeps its entry with the size read now. The size read
  is returned either way, so the judgement still tells a file there from no file, and
  names the failed save or publish
- The workbook is ONE SHEET, laid out exactly as the report the client receives: every
  test one after another, most clashes first. The Summary sheet, the Matrix sheet and the
  sheet per test are GONE. All three were asked for in an earlier session, before anyone
  had put a real Navisworks report beside ours. Once both were seen together the ask
  became one thing, our output matching theirs, and theirs has none of them. Do not add
  them back because they seem useful. If it is not in theirs it is not in ours
- The tests are ordered MOST CLASHES FIRST, and where two hold the same count they keep
  the order they were created in. Measured off both exports in samples\client-report on
  2026-09-01: both are strictly descending over all 1830 blocks, and every tie group is in
  the order the tests sit in the exchange file rather than alphabetical, including one
  group of 1807. So it is a STABLE sort, and List.Sort is not one
- The OUTPUT is sorted and the document is not. Whether their report is sorted by the
  report writer or simply walks a collection Clash Detective had already sorted is
  UNKNOWN and cannot be read off the files. Sorting the document would mean calling
  TestsSortTests, a mutator that reorders the tests inside the NWF, and the NWF is the
  record of what has been fixed
- A distance or a coordinate is THREE DECIMALS with the trailing zeros kept, except that a
  value which is not zero but would round to 0.000 is written to three significant figures
  instead, in plain decimal and never an exponent. Measured across all 387 coordinates and
  129 distances in the two exports: nine are of that second kind and 0.000 appears nowhere
  in either file. Whether theirs rounds or truncates is UNKNOWN, because both files carry
  only the formatted text, so ours rounds
- HOW MANY MISSING ITEM IDS ARE THIS RUN'S IS ITS OWN NUMBER, F79. One run left 192 item
  id cells blank and nothing said whether that was this run failing to read a property or
  last week's results carrying items that were never read in the first place. Those are
  two different faults and only one is this run's to fix. `ClashReportModel
  .MissingIdLines` splits them on the row's Found date against the group's RunAt and
  nothing else. A row with NO date is UNKNOWN and is its own third number, never folded
  into carried over, because whether the clash date and the run date share a clock and a
  time zone is not readable off the DLL and only a run answers it. It counts ROWS and says
  so: a row is a result group and one date stands for every clash inside it
- The Item ID label is Element ID, and this tool CHOOSES it rather than reading it. It used
  to be the display name of whichever property matched, and since Id is first in the search
  list that came out as "Id: 990299" against their "Element ID: 702888". Which property
  actually supplied the value is kept on the item and goes in the log, because renaming a
  value is only honest while what was renamed is still visible
- A report check compares three things and not one: WHICH columns, in what SHAPE, in what
  ORDER. The check before this compared presence alone and passed while the test order, the
  id label and both number formats all differed from the samples. It reports the FIRST
  divergence with what ours holds and what theirs holds, because one fault has forty
  consequences and the first is the one to act on
- One workbook per group, written after the clash step, the viewpoints, the NWD and the pictures, F182,
  so the three outputs of a group agree with each other. Named like the group's other
  outputs with an xlsx extension. SINCE F158 THE NWF AND NWD FOLDERS HOLD THE NWF AND THE
  NWD ALONE and every log and report goes in ONE folder called Log, a setting,
  `ReportPaths.Subfolder`, beside them, worked out by `ReportPaths.LogFolder` as the deepest
  folder holding both and one level up where the two are one folder or one holds the other.
  It holds the run log and its .tsv, the notes on the shared coordinates, the Excel, the
  Internal workbook and the XML together, the HTML and its _files folder in the Clash report folder since F177, so the page's
  relative links and the Excel's F152 file:/// links keep working, proved by
  `LogFolderLinksTests`. NOTHING IN IT IS EVER OVERWRITTEN OR DELETED: a group whose name is
  already there takes `ReportPaths.NameThatOverwritesNothing`, its name and the moment of the
  run, for all of its files at once, and any other file takes `ReportPaths.FreeFilePath`.
  There is no Excel folder to pick any more. The setting is static because the open file run and the line under the
  Outputs step both work the folder out with no options object in front of them, and a
  name carrying either slash is refused where it is set, by name and never by asking the
  running platform which one it calls a separator
- THE CLASH REPORT FOLDER IS OVERWRITTEN AND NEVER DATED, F177, Bader's order of 14 Oct 2026. The HTML
  and its NAME_files folder of pictures sit in the Clash report folder under the group's plain name,
  `FederationJob.ClashReportName`, which `NameTheReports` never changes. A file the tool writes there under a
  name already there is OVERWRITTEN. The Log folder keeps its dated names, `ReportPaths.NameThatOverwritesNothing`
  no longer looks at the Clash report folder, so an Excel in Log may carry a date while the HTML and the
  pictures keep the plain name, and every link is made from each picture's own path. AN OLDER EXCEL IN LOG
  POINTS AT JPGS A LATER RUN OVERWRITES, so it then shows the later run's pictures, and its link to a
  leftover that was removed no longer resolves. LEFTOVER PICTURES ARE REMOVED, `LeftoverPictures.Remove`, once the
  picture step has run to its end: from the group's NAME_files folder, TOP LEVEL ONLY, every file named cd and
  six or more digits and .jpg, lower case as the tool writes it, that this run did not write, and nothing
  else. Never the logo, another file, a folder, a link, or anything outside a folder that ends _files. Nothing
  is removed where the run did not reach the end of its picture step, the pictures' guard stopped it, the
  group already holds an error, or this run wrote no picture. One IMAGES line says the count removed. Proved
  by `LeftoverPicturesTests` and `ReportPathsTests`
- Results are grouped, not one row per raw clash. A group is one row and its distance is
  the most severe clash in it, which is the minimum: a hard clash reports a negative
  overlap so the worst is the most negative, and a clearance test reports a gap so the
  worst is the smallest. Every row carries the raw count behind it, so the grouping hides
  nothing. A GROUP STANDS FOR THE CLASHES UNDER IT AND FOR NO MORE, F113, and one with none
  under it stands for none. `ClashRow.ForGroup` is the rule. The harvest floored a group at
  one, so an empty group made the Clashes cell and the cell of its status read one higher
  than the clashes the document holds, which the runner counts under every group. Whether
  a saved test can hold an empty group is UNKNOWN
- The clash API has no open against closed notion. Nothing on IClashResult, ClashResult,
  ClashResultGroup, ClashTest or DocumentClashTests names one, ClashResultStatus is a flat
  five value enum, and Navisworks' own report does not mention open or closed either. So
  every status is reported as itself, and where one number is needed for what is
  outstanding, WHICH statuses that number holds is a setting with two choices and never
  a constant, because neither answer is readable off the API and both are stated rules:
    Navisworks open  New, Active and Reviewed. The default, because it is the product's
                     own definition and the number then agrees with the panel
    New plus Active  the two only, for a project that treats Reviewed as dealt with
  Approved and Resolved are closed under both. The choice is written to the log, so
  nobody reads an API meaning into a number that has none. The word open never appears
  unqualified. Since the Summary and Matrix sheets went, no output shows that number,
  so the window no longer offers the choice and the report options no longer carry it,
  D3 on 2026-09-12. The two rules stay in OpenClashes with their tests, because the
  image filter reads Navisworks open off the same place
- The test sheet carries the CLIENT'S columns, in their order and in their words, because
  they have already accepted a report in that shape. Per test:
    Tolerance, Clashes, New, Active, Reviewed, Approved, Resolved, Type, Status
  Per clash:
    Image, Clash Name, Status, Distance, Grid Location, Description, Clash Point,
    then Item 1 and Item 2, each Item ID, Layer, Item Name, Item Type
  Four of these are joins and not columns, which is the part a description gets wrong and
  the stylesheet settles. Grid Location is ONE field, "B-1 : ROF", grid then a spaced colon
  then the level. Clash Point is ONE field, "x:31.643, y:-2.913, z:3.325", three decimals
  each with the trailing zeros kept. Item ID is ONE field, "Element ID: 1554240", and the
  tool CHOOSES that label rather than reading it off whichever property matched, which once
  wrote "Id: 990299" against their "Element ID: 702888". Which property supplied the value
  is kept on the item, because renaming a value is only honest while what was renamed is
  still visible.
  Tolerance carries its unit with no space, "0.025m". Distance is the raw signed number,
  negative on a hard clash, written as a number so it still sorts. Type reads
  "Hard (Conservative)". Measured, see docs\history\scan.md section 4k
- THE PRIORITIES AND THE BY DESIGN PAIRS ARE BUILT IN, F154, Bader's decision of 2026-10-13,
  Q162-4. samples\clash-priority-map.csv and samples\by-design-pairs.csv are copied byte for
  byte into the matrix pack of the project, src\Federator.Core\Packs\<pack>, embedded by the
  one wildcard of Federator.Core.csproj as Federator.Core.Packs\<pack>\<file>, and
  `Federator.Core.Exchange.MatrixPack` finds the packs by listing the resource names under that
  prefix, so no code names a project's file and a project is added by adding a folder. The
  two files are read by their names, `MatrixPack.PriorityFile` and `ByDesignFile`, through
  `PriorityMap.Read` and `ByDesignPairs.Read` unchanged. With one pack the run reads it, and
  with none or more than one it reads none, `MatrixPack.TheOne`, and one PACK line names the
  pack and its files or says why none was read. THE PRIORITY IS ALWAYS ON, so the Priority
  column is on every workbook a pack was read for. The by design box stays, and with it on
  the built in pairs apply. The Priority CSV and By design CSV rows of the Clash step, their
  pickers and their remembered folders are gone, and so are `ReportOptions.PriorityPath` and
  `ByDesignPath`. MatrixPackTests holds each copy to its sample and the priority file to one
  row per test of the built in matrix
- THE CLASH MATRIX IS BUILT IN TOO, F155, Bader's decision of 2026-10-13, Q162-5. EACH PROJECT
  HAS A BUILT IN MATRIX PACK THAT BADER SUPPLIES, and it replaced the clash XML picked each run.
  The pack holds the corrected clash XML with its team map and its list of corrections beside
  the two CSVs, this project's from exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.*, each a byte
  for byte copy held to exchange by MatrixPackTests. `MatrixPack.XmlFile` is the one .xml file
  of the pack, and none or two read no matrix and say why. `MatrixCorrections.ReadPack` reads it
  through `ExchangeReader`, `TeamMap.InPack` and `MatrixCorrectionList.InPack`, the readers and
  the rules a file on the disk is read by, so the sets, the tests, the corrections and the teams
  read the same, a test proves it, and `ExchangeDocument.Pack` names the pack, its sha256 the
  file's. The window's open, every run, both hand buttons and Build sets read the built in
  matrix, so every weekly run is a Weekly run plus matrix: a test of the matrix already saved in an
  NWF runs where it sits with its results kept, and a test of the matrix the NWF lacks is added.
  A TEST SAVED IN THE NWF THAT THE MATRIX DOES NOT NAME IS RUN TOO since F168, Bader's answer A
  to Q168: `ClashTestPlan.WithTheSavedTestsTheMatrixDoesNotName` adds it after the tests of the
  matrix, planned from the document, so it runs where it sits and is never created or compared, at
  the tolerance of the drop down like every test. It is a row of the HTML, the Excel and the
  Coverage sheet, `ClashRunOutcome.SavedTestsRunToo` names it, and its viewpoints go under
  priority C unless the priority map names it, `PriorityMap.OfASavedTest`. With no pack
  built in, the tests saved in each NWF run as before and the RUN line says why no matrix was
  read. The Clash XML row, its Browse button, the Teams line, the remembered ClashXml folder and
  the picked paths are gone. NOTHING IN THE CODE NAMES ANY ONE PROJECT'S FILE still holds: the
  packs are found by the resource prefix alone, and a test reads every code line of src for the
  pack's name or its matrix's
- THE CLASH PRIORITY IS THE ONE COLUMN OF OURS ON THE CLIENT'S SHEET, F83, and since F154
  on every workbook, read off the matrix pack built into the tool. It sits in T, one past the end of their table, and
  `WorkbookWriter.LastColumn` stays 19 so the title merge, both item fills, the boxing,
  the width loop and every loop in the check still describe THEIR table. A pack that could
  not be read means no Priority column: no heading, no cells, no width, no RESULT line and the measured
  block order. Priority is NEVER added to `ClientReportColumns`, which is theirs and is
  re-read off their own exports on every run, and never written into the clash XML,
  because the stylesheet makes a column out of every smarttag and it would then be on the
  page the client receives. `WorkbookCheck.Of` takes whether the priorities were read, because a
  column past their table is invisible to every other check in there, and the heading has
  to be there where they were read and absent where not. Until FR-199 that flag also switched
  the order check off, because a priority sorted workbook was in priority order on purpose,
  and since FR-199 the order is checked whether or not the priorities were read
- THE BLOCKS STAY IN THE MEASURED ORDER WITH THE PRIORITIES READ, FR-199, Bader's answer
  to Q49 on 2026-10-06 built by F118: the priority is a column to sort on in Excel, and the
  blocks keep the order measured off the client's exports, most clashes first with ties in
  creation order. F83 sorted them A, then B, then C, then the tests the file says nothing
  about, by name inside each, an order the client never accepted, and every output
  followed. `ReportOrder.Tests` is still the ONE place the order is decided, and the
  workbook, the clash XML and the picture numbering all read it, so the page and the
  workbook cannot list the same tests differently and a picture cannot keep a number from
  an order nothing else uses. The map lives on the report, `ClashReport.Priorities`, never
  null, because three callers each passing their own copy is how they stop agreeing. The
  engine still tells the check the priorities were read, which since FR-199 means the Priority
  column alone, so no line of its changes
- THE PRIORITY FILE IS MATCHED ON THE TEST NAME, EXACTLY, Ordinal and never trimmed,
  F83. A test name is built out of two set names and two of those end in a space. The
  file carries test_name, left_set, right_set and priority, and the sets are read and
  kept so a person can see what a row meant, but nothing matches on them. A row whose
  letter is not A, B or C is a PROBLEM named in the log and left out, never a silent
  None, because a silent None reads exactly like a test the matrix never mentioned. A TEST
  THE FILE NAMES TWICE IS A PROBLEM TOO, FR-037, F118: the last row still wins, as it did,
  and the repeat is named with both lines and both letters so a person decides which, and
  `PriorityMap.RowCount` is the rows the file holds, a test named twice counted twice.
  The priorities NEVER fail a run: a pack that cannot be read is a finding in the log and
  a line in the window and the run goes on with no Priority column. `Priorities` is the
  words and the order, None last, and the workbook cell is EMPTY for None while the
  viewpoint folder is called No priority, because an unnamed folder is not a folder
- PRIORITY IS NOT STATUS AND THE TWO ARE NEVER USED FOR EACH OTHER. Priority is A, B or
  C off the client's clash matrix. Status is the Navisworks word, New, Active, Reviewed,
  Approved or Resolved, and lives on a clash. Nothing in ClashPriority names a status and
  nothing in ClashStatus names a priority
- Our extra columns, none since FR-198 deleted family, type name, material and the per item
  discipline and left the source file reaching no output, would come AFTER
  theirs and never in place of any of them, on the CLASH XML and nowhere else. THE ONE
  EXCEPTION IS THE PRIORITY COLUMN ABOVE, which goes on the workbook and not on the clash
  XML, and only where the priorities were read, which since F154 is every run with a pack. The Client
  columns only tick box that used to switch them off is GONE, because the workbook became
  one sheet laid out as theirs with none of ours on it, so there was nothing left for it to
  remove and it sat in the window doing nothing. Ours says Type Name
  rather than Type, because the client already has an Item Type column and it holds
  something else, the Navisworks item type, which reads Solid
- The client's report DOES carry a Layer column, in both committed exports, one per item
  block. Read off the header row of each on 2026-09-12. This bullet said the opposite, from
  a measurement of two other exports, and the code has written fifteen headings all along.
  Item Name and Item Type are not fixed columns either, they are the quick properties, and
  the accepted report happens to carry those two. So the fifteen are the whole of it, seven
  before the item blocks and four in each of the two
- Grid Location is built from the grid intersection name, which ALREADY carries the level.
  Appending the level to it wrote "D-8 : LGF : LGF" on a real run. The level is only added
  where the grid does not already end with it
- The Type cell carries the client's wording, "Hard (Conservative)", not the file's token,
  "hard_conservative". One pair is measured and the rule read off it is that the first word
  is the type and the rest is a qualifier in brackets. A value that already has a space or
  a bracket in it is left exactly as it is
- An all zero GUID is not an id. A run wrote
  "Instance GUID: 00000000-0000-0000-0000-000000000000" into all 426 item cells, which
  reads like an id and identifies nothing. The cell is left empty instead, and the real
  cause is fixed too: the clash gives back the geometry leaf, which on a Revit sourced NWC
  carries a material name and no Revit properties, so the id, the family and the type are
  looked for on the item, then on its composite item, then up its ancestors
- AN ID OFF THE GUID FALLBACK IS AN ID, FR-040, F118. An item with no id property is given
  its instance GUID under the label `ClientFormat.GuidIdLabel`, Instance GUID, and the ITEM
  IDS block counted that filled cell as a missing id, named no id property as its source
  and said it was written as Element ID while the cell said otherwise, and the workbook
  and page checks called a first cell of that kind the wrong shape. A missing id is an
  EMPTY cell, `ClashReportModel.Missing`, the source of such an id is
  `ClientFormat.GuidFallbackSource` and its line says the label the cell carries, and
  `ClientShapes.LooksLikeAnItemId` accepts the two labels this tool writes and no other.
  The harvest gives the cell `ClientFormat.GuidIdLabel` since F118's add-in pass, where it
  typed the label as a literal
- EVERY EMPTY GRID LOCATION CELL IS COUNTED, FR-041, F118, `ClashReport.GridLocationLine`, one
  GRID line per group after the units line, with its row in the .tsv: how many clash rows of the report carry an empty
  Grid Location cell, with the heights of the empty rows and of the filled ones in the units
  the rows are in, or that every row has one. Set 03 left the cell empty on 345 of 5679 rows
  and the checks read the first clash row only, so nothing said so. Why the grid gives nothing
  for those rows is UNKNOWN, and the heights are what the next run measures against the grid
  levels. No line where the report holds no clash row
- Distance is the ROUNDED signed number and the cell carries no number format, which is
  what theirs holds and what the code writes. This bullet asked for a raw number behind a
  format, which is the thing the later bullet says was wrong: the cell read -0.328 while
  anyone sorting, filtering or copying the column got -0.328083992004395. The VALUE
  differing between two reports is the two documents' units and not a fault: ours measured
  in feet and theirs in metres, and the tolerance says so too, 0.2461ft against 0.025m
- Client columns only means nothing of ours on the sheet at all, not just no extra columns.
  The notes above the table, the link back to the Summary and the filter arrows are ours
  and all three go. Their columns do not move
- The client's logo is not copied. It is an Autodesk logo that ships with their report and
  not with this install, and there is no right to redistribute it
- A link to an image is written as a relative Uri and never as a plain string. Handed a
  string, ClosedXML reads a relative path as an INTERNAL address, so the cell tries to
  jump to a sheet of that name instead of opening the picture. There is a test that opens
  the written xlsx as the zip it is and reads the relationship back, because the object
  model reported this as fine while the file was wrong
- The report still counts skipped, passed and found as three numbers that are never
  merged. On 1C07BC that is 1164 skipped, 618 passed and 48 with clashes, and those add
  to 1830. A skipped test says so and never reads as passed
- The client's report is HTML (Tabular), not a workbook. Clash Detective cannot export an
  xlsx at all, so what they accepted is a page exported from Clash Detective and opened in
  Excel. That is why the sample declares 53 columns with 17 populated, carries merged cells
  and has absolute file:/// links in it
- The page is rendered by Autodesk's own clash_report_html_tabular.xsl, read from the
  install at run time and never copied into this repo. The path is built and tested
  directly, the language the application reports and then en-US, never searched for. Where
  neither is there the log names both paths, no page is written, and the workbook and the
  XML are untouched. It is on by default, because it is the format the client accepts
- Not one column in that page is fixed. Every one is a boolean over an XPath in the
  stylesheet, so what the page holds is decided by what our XML holds. The Layer column is
  the proof: 1A04WE has it and 1A02WE does not, from the same tool, because one export
  carried layer data and the other did not. Those two were measured on Bader's machine and
  are recorded in docs\history, and they are not the exports in samples\client-report,
  which are 1A02WN and 1A04WN and both carry the column. 1A04WE is not a client file: it is
  the clash report Bader exported from Navisworks and pasted into an empty Excel workbook,
  his words of 2026-10-06. Never write a column list for that page, write the data and let
  the stylesheet decide
- Our XML feeds that stylesheet, so its shape is not ours to choose either. Measured on
  2026-09-01, it answered every column test but three: description, smarttags and the href
  on a result, which are the Description column, the Item Name and Item Type columns, and
  the Image column. See docs\history\scan.md section 4m for the whole table
- Every report this tool writes is READ BACK off the disk and checked, and what the check
  found goes in the log as a block and in the window as one line. Bader was opening every
  report in Excel and searching it by hand, and the tool wrote the file. The FILE is read,
  never the object that produced it, because the object model has twice called a written
  file fine when it was not
- The page check reports the rows, how many carry an Item ID on each item, the first id in
  full so its shape is visible, any column that is not the client's named, the tolerance
  cell exactly as written, how many picture references there are and how many use a
  backslash and how many are really on disk, and whether the logo is there. The workbook
  check reports the sheets and their names, the blocks, the rows, the clash count of each
  block, and the first thing that differs from the client's layout with both sides printed.
  It counts no column of ours, because since the workbook became their one sheet there is
  no column of ours on it to count. EVERY TEST WITH A CLASH IS A BLOCK AND A TEST WITH NONE
  WRITES NOTHING, F153, Bader's decision of 2026-10-13, which RETIRES Q73's answer b, the one
  row of a test that found nothing, and FR-035's count of one block for every test in the file.
  His run of that day held 130 blocks and then rows 5204 to 6903, 1700 tests of one row. The
  check counts the blocks with the Clash Name heading, `WorkbookCheck.Blocks`, and the BLOCKS
  line sets them beside the report's tests with a clash, `ClashReport.TestsWithAClash`, so a
  block missing or one too many is said in capitals. A ROW OUTSIDE EVERY BLOCK HOLDING A NAME
  OR A VALUE WHERE THE TEST HEADER STARTS IS THE OLD ONE ROW SHAPE AND A PROBLEM,
  `WorkbookCheck.NoClashRows`, named by its first row, so a workbook that still holds one fails
  the check. A workbook where no test found a clash holds its title alone, has no block layout
  to compare, so its row 1 and widths are read and the check says no block was compared. The
  page keeps a block for every test and the Coverage sheet lists every test, so no test goes
  unreported
- A report check NEVER fails a group. It is a warning on its own list, apart from the
  errors, because the NWF, the NWD and the workbook were all still written. Judging a group
  on it would repeat the fault that once reported a clean 22 group run as FAILED
- The client column set is read from clash_report_html_tabular.xsl and from the exports in
  samples\client-report, never typed into the code. The stylesheet gives what is possible
  and the samples give what is theirs. A test re-reads both on every run and fails if the
  set has drifted, so the list is checked against the files rather than trusted
- The stylesheet carries a DEAD template named ItemHeaderCells that nothing calls. It
  writes Item Type as a literal, reads ./Name with a capital where the rest of the file
  reads name, and puts Item ID and Layer after the quick properties rather than before.
  None of that is what the reports carry. The live one is mainTableHeader and it is the
  only one anything here reads. Measured, see docs\history\scan.md section 4o
- The page carries ONLY the two quick properties the client's report has, Item Name and
  Item Type, whatever any tick box says. The stylesheet makes a column out of every
  smarttag, so Family, Type Name, Material, Source File and Discipline would each become a
  column on the page the client receives. They are workbook columns. createddate goes the
  same way, because neither supplied report has a Date Found column
- Tolerance is written to THREE decimals, which is how both of theirs are written. Ours
  read 0.2460629921ft against their 0.025m. The units differing is the two documents
  differing and is correct. The precision was ours
- A picture reference on the page uses a BACKSLASH, for the clash pictures and for the
  logo, because that is what both supplied reports write and the client opens these in
  Excel on Windows. The workbook's cell link keeps a forward slash, because a hyperlink there
  is a Uri, and the click link on each picture of Bader's by-hand client xlsx is relative and
  forward slashed too, read on 2026-10-13
- EACH PICTURE ALSO CARRIES A CLICK LINK, F169, Bader's answer A to Q163: an a:hlinkClick on the picture to the
  relative path of its jpg from the folder the workbook sits in, forward slashed, going up with .. where the jpg is
  not below it, a space written %20 and nothing after the file name, LinkedPictures.RelativeLinkFor. The linked picture
  of F152 and the Image cell link are as they were. Since F177 the _files folder sits in the Clash report folder beside the Log
  folder, so the link is ../Clash report/NAME_files/cdNNNNNN.jpg, read off the workbooks of 19 Oct 2026. Whether Excel itself follows a relative link is UNKNOWN until a
  person clicks one
- THE WORKBOOK'S PICTURES ARE LINKED PICTURES IN THE CLIENT'S FORM, F152, Bader's decision of
  2026-10-13, Q162-2: pictures stay links, the same form as the client's own file.
  samples\client-report\1104-PAR-1A02WN-XXX-BM-RPT-000001.xlsx, his by-hand workbook, holds
  xl\drawings\drawing1.xml with one twoCellAnchor of editAs oneCell per picture, 64 and the
  logo, every picture 906780 EMU square from column A of its row, each blip carrying r:link
  and no r:embed, and one relationship per picture of Type image, TargetMode External, to
  file:/// and the jpg's absolute path with a space written %20, read on 2026-10-13.
  `Federator.Core.Report.LinkedPictures` writes the same after ClosedXML's SaveAs, which
  writes no linked picture, by editing the xlsx as the zip it is: the drawing part, its rels,
  its content type, the sheet's relationship to it and the drawing element of the sheet,
  before the first element the schema puts after it. One picture per clash row with a
  picture, anchored at column A of that row, its far corner worked out from the sheet's own
  widths and heights, so a 60 point clash row gives the client's own end, column A at
  906780 and the next row at 144780. The target is the absolute path of the jpg in the
  _files folder beside the workbook, `ImageNaming.PathFor`, as the client's file has it, and
  the cell keeps its relative link. NO PICTURE IS EMBEDDED, so THE PICTURES SHOW ONLY WHILE
  THAT FOLDER STAYS WHERE IT WAS WRITTEN, which the WORKBOOK step's XLSX line says,
  `LinkedPictures.Line`. Every part is made and turned into bytes first, then written into a copy that replaces
  the workbook only once it is whole, so a fault leaves the file as ClosedXML wrote it, and a sheet
  already holding a drawing is refused. Whether Excel shows the pictures, and whether it asks
  anything when it opens a file of thousands of them, only Excel can show, the proof steps of
  steps\03_bader_next.md. The page is unchanged: the page and its _files folder are still what
  gets sent
- Exactly ONE objectattribute per clashobject, and it is the id. The Item ID cell is
  value-of over ./objectattribute/name, which takes the FIRST node, so writing several put
  the item's Name in the id column. Everything else about an item is a smarttag, which is
  how Item Name and Item Type reach the page at all
- Every clashobject carries the SAME list of smarttags, empty values included. The
  stylesheet counts them once off the first clashobject and uses that count for every row,
  so one item with fewer of them slides every column after it sideways
- Date Found and the item properties this tool reads and they do not, two since FR-198, are ours, not theirs. Their report has
  neither, and there is no longer a choice about it: the workbook is their one sheet with
  none of ours on it, so both are left out everywhere and neither is written at all
- The logo is DATA, read by the stylesheet from //logo/@href, not part of the layout, and
  the page carries the one Navisworks puts on its own reports because that is the report the
  client accepts. It is read off the install at run time, the same way the stylesheet is,
  from \Images\logo.jpg at the top of the install with no language folder, measured
  2026-09-01 at 6137 bytes and byte for byte the same file as the logo.jpg in both supplied
  reports. There is nothing to set up before a first run
- No copy of that logo is in this repo, in the bundle, or in install.ps1. It is read off
  the machine that is running, every run. The only two in the checkout are inside the client
  exports Bader committed, where Navisworks had already put them, and a test names that
  exception so one appearing anywhere else fails
- The logo is COPIED into the report's own _files folder beside the clash pictures and
  linked relatively, which is what Navisworks itself does. The report goes to a client, so
  the page must not point at a path on the machine that wrote it. The accepted xlsx carries
  absolute file:/// links and that is exactly why its pictures break everywhere else. A test
  opens the written page and reads the src back out of it, because the object model reported
  a broken link as fine once already
- A logo that cannot be found writes the page without one, names every path it looked at,
  and carries on. A missing picture never stops a report
- The Browse box stays, for a project that needs a different mark, and it starts filled with
  the install's logo. Clearing it means no logo. Most people never touch it
- Writing a logo turns the stylesheet's Image column on even with no clash pictures, because
  its test is boolean(//@href) and the logo carries an href too. That is Autodesk's own
  behaviour and their reports do the same. Pinned by a test so nobody reads it as a fault
- The clash XML is optional and off by default. It is built from the same results the
  workbook is built from, never by reading the workbook and never read by it, so a fault
  in one cannot corrupt the other. Its shape was read off the three stylesheets the
  Navisworks install ships, because there is no clash report schema anywhere in the
  install. What is filled and what is left out is listed in ClashReportXml, and a part
  with nothing to put in it is left out rather than written empty
- The workbook library is ClosedXML, and the writer lives in Federator.Core rather than in
  the add-in so the tests write a real xlsx and read it back without Navisworks. It ships
  twelve more DLLs into the bundle and install.ps1 carries every one of them, because the
  add-in would otherwise load and then throw the first time a group finished. None of them
  collides with a file the Navisworks install ships
- Images are ON by default, because the report the client has already accepted has them
  and one without them is not the thing they agreed to receive. They go where that report
  puts them, in a folder beside the workbook named after it with _files on the end, as
  loose jpg. The workbook links to them and does not paste them in, the linked pictures
  above. The thumbnail tick box that pasted them into the cells is gone, F152, with
  ImageOptions.EmbedThumbnail and the writer's two thumbnail sizes. A PICTURE IS NOT A VIEWPOINT and nothing in the image
  code touches SavedViewpoints. This bullet used to end by saying no clash is ever saved
  as a viewpoint, and F85 reverses that half of it: a clash now gets a viewpoint as well
  as a picture, planned by `Federator.Core.Views.ClashViewpointPlan` and written by
  `ViewpointBuilder` since the viewpoints round on 2026-09-19, when 5j measured that a
  captured viewpoint records the hidden state
- The picture names are theirs and are NOT one running sequence, which is what the first
  dozen look like. It is cd, then the test formatted 00, then the clash within that test
  formatted 0000. Test 0 clash 1 is cd000001.jpg and test 100 clash 1 is cd1000001.jpg,
  seven digits, measured on a real 2672 picture export. Two digits is a floor and not a
  width. The test number counts tests that have pictures, so it can never leave a gap, and
  a test whose first render fails hands its number back
- The pictures are NUMBERED IN THE EXPORT ORDER, which is the order the rows are written:
  tests most clashes first with ties in creation order, and inside a test the clashes as
  Clash Detective lists them. Measured off both exports in samples\client-report, where
  the first block is cd00 and the second cd01 whatever order the tests ran in. The report
  order is only known when the last test has run, so SINCE F149 EVERY PICTURE IS TAKEN ONCE
  EVERY TEST HAS RUN AND EVERY MIRROR MERGE IS MADE, walking `Federator.Core.Report.PictureWalk`:
  the tests in ReportOrder.Tests and each test's rows in its own order, each picture numbered
  by `ClashReport.NextPictureOf`, the test's index claimed at its first picture and one more
  than its rows already pictured, and its row pointed at it by `PictureNumber.PointTheRow`, the
  one place each of the two is done. So every picture is written under its final name, and
  `PictureWalkTests` prove the renumber then renames none and that a walk in the order the
  tests ran leaves pictures to rename. Until F149 a picture was rendered while its test ran,
  under the run order number, and renamed after the run, 39 s of set 09's 1A02MM. Consecutive
  rows of one test of the report read from one test of the document make one stretch, a row
  waiting for no picture ends none, and a row met under a second test of the report is walked
  once and named. The rename stays, ONCE, in one pass, as the check that finds every picture
  already right and the rename of any picture under another number, and the row, the workbook
  link, the XML href and the page all follow it. The
  rename goes through a holding name first, because a swap between two tests would
  otherwise write one picture over another. What a picture shows and its size are
  untouched. The rule lives in Federator.Core.Report.ReportOrder and the rename in
  Federator.Core.Report.ImageRenumbering, so both can be tested without Navisworks. A
  rename that fails is a warning on the report and never fails the group. A MOVE THAT
  THROWS IS CAUGHT AT THE MOVE AND EVERYTHING THE TWO PASSES DID IS UNDONE, FR-075 in
  F120: the pictures already at a final name go back to their holding name, then every
  holding name goes back to the name its picture was written under, each put back read off
  the disk, so the folder is as it was before the rename and every row, which is pointed only
  once every move has gone through, points at the picture it was written under. The
  outcome names the move that threw, how many had been moved before it, how many were under
  a holding name, and every picture that could not be put back with where it is. The
  engine's lines are the outcome's and never a sentence of its own about the disk, since
  the one line it had said the pictures not yet renamed keep their run order numbers while
  they sat under a holding name. The rename is a step of its own, RENUMBER. IT SAYS HOW FAR
  IT HAS GOT AFTER EVERY PICTURE, F146: Apply hands an optional said each picture checked, moved
  aside and renamed with its count, so the engine lets the window answer and writes a PROGRESS
  line, and a said that throws is named once in the outcome and the renaming goes on. A holding name an
  earlier, cut off renaming left is taken out before the move onto it and said
- THE PICTURE LIMITS, F172, F175 and F190, Bader's answers to Q171 and his order of 15 Oct 2026, `PictureLimits` and
  `GdiBackstop`. SINCE F235, BADER'S ANSWER TO Q187 OF 18 OCT 2026, ONLY A TEST OF MORE THAN 750 CLASHES IS CAPPED, the
  setting `ImageOptions.CapOverClashes`, its default `PictureLimits.DefaultCapOverClashes`, read off the test's clashes,
  `TestReport.RawClashes`, by `PictureLimits.LimitFor`, the one rule. Such a test gets pictures for at most its first 150
  clashes at priority A, 100 at priority B and 50 at priority C, and the rest are skipped, per test and not for the run, and
  a test of 750 or fewer gets a picture for every clash. Its IMAGES line, `PictureLimits.TestLine`, names its clashes and the
  750 rule. The GDI and memory stops below are unchanged. A test with fewer clashes than its cap gets a picture for each. A saved test the built in
  matrix does not name is C, F168, and so is a test with no priority read, so the cap is never the most
  by a doubt. The priority is read off the built in priority map at the start of the picture pass into
  `TestReport.PicturePriority`, because `TestReport.Priority` is put on the report only after the clash
  step. One log line per test whose clashes exceed its cap with its name, its priority letter, its clashes, the pictures made and
  the skipped. The clashes stay in the HTML, the Excel and the viewpoints, and every clash keeps its
  viewpoint, only the picture is skipped. The cap per test of ImageOptions keeps its own job, off by default and with no
  box since F232, and the least of it and the rule holds,
  `ImageOptions.RoomFor(test, written)`. A backstop reads the GDI objects of the process before every
  picture and again after each picture is saved and disposed, `ClashImages.Write`, and past 9000 no more
  are made: a NOTICE with the picture number and the count, the pass stops asking for rows, and the run
  goes on, the NWF and the NWD being already written since F182. It is not the pictures' guard, which stops
  the run. The Bitmaps the picture step receives are the one of `TestsImageForResult` and, since F226, a retake's, each disposed in a
  finally on every path. The GDI split line, the MEMORY line, the RATE line of F157 and F161 and the
  texture cache line of F147 are kept. The texture cache is read and reported and no new clearing is made
  by F175. SINCE F227, Bader's answer to Q210, the three caps, the GDI stop and the memory stop are SETTINGS on
  `ImageOptions`, `PicturesForA`, `PicturesForB`, `PicturesForC`, `GdiStop` and `MemoryStopBytes`, and the numbers
  above are their defaults, `PictureLimits.DefaultPicturesForA` and the rest. Each refuses where it is set what
  cannot work: a cap below nought, nought giving that priority no picture, a GDI stop at nought or below or at the
  10000 a process may hold or past it, `PictureLimits.ProcessGdiLimit`, and a memory stop of nought bytes or below.
  `PictureLimits.PicturesFor`, `LimitFor` and `RuleInWords` read the run's `ImageOptions`, and `ClashImages` builds
  both stops off it, so no reader keeps a number of its own
- THE RETAKE OF A PICTURE THAT DRAWS NO ITEM OF ITS CLASH, F226, Bader's answer to Q208 of 18 Oct 2026, so no Image cell
  shows only sky, measured in scan.md 5z-zm, `Federator.Core.Report.PictureRetake`. A picture draws an item of its clash where
  any pixel read, every `PictureRetakeSettings.Every` pixel of every such row, 2, reads red or green above `Bright`, 180, with
  the other two channels below `Dark`, 110, count-sky-pictures.ps1's measure, `ItemPixels`. A picture shows only the sky where
  at least `SkyShare`, 95 percent, of the pixels read are the sky's blue, blue the largest channel, green over red, blue at
  least `SkyBlueAtLeast`, 200, and over red by `SkyBlueOverRed`, 40, `OnlyTheSky`, since the sky runs from 108,144,230 at the
  top to 209,238,254 at the bottom of a picture of 1024 by 1024 and no one colour names it, which F226's first smoke run read
  when its first pass looked for 136,169,238 at the middle and kept 4 pictures of only the sky. The retake's camera is the
  clash's own moved back along the line from the clash's centre through it to `LeastMillimetres`, 9144, 30 ft, where it
  stands closer, `BackedOff`, and left where it stands farther or on the centre. `Choose` keeps the tool's picture where it
  draws an item or where no retake was taken, a result group or one that threw, so a throw never drops a picture, the retake
  where it draws one, the tool's where it is not only the sky and the retake draws none, the retake
  where only the tool's is the sky, and none where both are, so a retake never writes a picture of only the sky. `RetakeTally`
  counts the pictures drawing no item, those taken again with their seconds, those drawing their items, those still drawing
  none, those not taken again and those left with no picture, on one RETAKE line a group, a nought and the setting off each
  said apart. `ImageOptions.Retake` carries the settings, on by default, each refused where it is set if it cannot work
- THE TESTS AT OR UNDER THEIR CAP ARE PICTURED FIRST, F203, PictureWalk.Of(report, readFrom, limitOf). The walk
  takes every test whose rows waiting for a picture are no more than its cap first, then every test over its cap,
  each part in report order, so the memory stop of F182 takes the last pictures of the largest tests and not every
  picture of the small ones. Measured on the C04 run of 15 Oct 14:11: the stop at 3275 pictures left 163 tests with
  none, all 162 under 50 clashes among them, while the 174 tests at or under their cap wanted 2518. A picture taken
  out of report order is renamed by the renumber to its report order name, so the names and the links are as
  before. PictureWalkTests prove the order, a stop after four pictures in each order, and the rename. SINCE F235,
  Q187, A TEST OF 750 CLASHES OR FEWER HAS NO CAP, so every such test is in the first part, in report order, most
  clashes first, and where a stop fires the smallest of them and every test over 750 get none, which no longer holds
  what this bullet says of the small tests. Which order Bader wants then is Q221. The clashes counted toward the 750
  are `TestReport.RawClashes`, every row of every status as the Clashes cell and the panel count them, a kept test
  of a mirrored pair with the clashes only its mirror found among them, Q221
- THE MEMORY STOP AND THE ORDER OF A GROUP, F182, Bader's answer C to Q175, `MemoryBackstop`,
  `ImageOptions.MemoryStopBytes` and `GroupOrder`. Navisworks's private memory grew about 3.3 MB for each
  picture, to 26 GB on C04 on a PC with 15.8 GB of RAM, and the NWD publish after the pictures hung with a 0
  byte file. A SECOND STOP beside the GDI one: `ClashImages.Write` reads the private bytes of the process
  before every picture and again after each is saved, `ProcessMemory.PrivateBytesNow`, and past 12 GB, the
  setting's default since F227, no more are made. One line says it at once, and once the pass ends a NOTICE says the picture
  number, the private bytes and how many pictures were skipped, `ClashImages.PicturesSkippedByTheStop`. The
  first of the two stops to fire ends the pictures, and a stop is a normal end of the picture step, so F177
  removes the leftover pictures after it as after a full pass, where a run that threw removes none. THE
  PICTURES COME AFTER THE NWD. A group's order is the clash step with no picture, the viewpoints, the NWF
  saved where anything changed, the NWD published, the NWF confirmed, then the pictures, the renumber, the
  leftover removal, then the workbook with the HTML and the XML, the coverage and the internal workbook,
  `RunSteps.All`. `ClashRunner.DeferPictures` leaves the pass out of `Run` and
  `TakeTheDeferredPictures` takes it, each test found again by its address and name as it always was. The
  viewpoints read the rows and their addresses and no picture file. The picture step is not followed by an
  NWF save, since it writes nothing into the document and the hiding is put back, so a put back that failed is
  no longer carried into the NWF and the NWD. F185 removed the Compact resolved option and its call of
  TestsCompactAllTests, so no result moves between the clash step and the pictures. `GroupOrder` writes the ORDER lines. A
  stop of the hand button's Run tests is unchanged, it takes its pictures inside the clash step.
- THE VIEWPOINT TREE, F186, Bader's answer 1 of Q174, `ToolViewFolder`. Everything the tool
  makes goes in ONE root folder named Clash Status, F198, named NWC Federator (auto) until then, carrying the comment
  NWCF-AUTO run=<stamp>. A folder of the old name an earlier run made is outside it and deleted by F191. Under it A, B
  and C, the letter of the test's priority, a test with none being C, then one view for each test, directly. The team
  pair level is gone, and every open clash of a test is in its one view, except in C, F192, where in a pair that carries the size folder the clashes over the threshold are the test's view in C/Over 150mm and the rest its view in C. A and B have no Over 150mm folder, and the clashes the earlier rule would have split off there stay in the one view and are counted and named for Bader's decision. The team pair still
  decides which models a view shows. On every run the top folder of the last run is found by its name AND its marker and
  deleted with all it holds, `SavedViewpoints.RemoveToolFolder`, before the new views are made, and a folder of that
  name with no marker is a person's and is left. Nothing outside the folder is read for removal: the inventory and the
  seven tree checks are handed `ToolViewFolder.NodesUnder` and nothing else. A view a person moved into the folder goes
  with it, and the NWF page says so. DELETE EVERYTHING OUTSIDE IT, F191, Bader's order of 15 Oct 2026,
  `OutsideTheToolFolder`: after the new tool folder is built and its marker is there, every viewpoint and every folder at
  the root is deleted except the tool folder of this run and a root folder named Comments, whatever its case, which is
  kept with all inside it. A folder named like it but not exactly it, "Comments old", is deleted, and so is a viewpoint
  named Comments. An empty Comments folder is made where there is none. It runs after the build so a build that stopped
  or threw deletes nothing of a coordinator's, and where no view was planned there is no tool folder to wait for. A
  tool folder expected and not found once refuses the whole delete. The tests and results are counted before, after the
  first item and after all, and a change stops the delete and fails the step. One line says the number of items and the
  top level names, 50 at most. This replaced the one time clean up of the root folders A, B and C of F186.
- THE DISCIPLINE FOLDERS, F199, Bader's order of round 6, Q183, `DisciplineFolders`. Under each letter the test's
  discipline folder, Architecture, Structure, Mechanical, Electrical or Other, then its views. It is decided in ONE
  place, `DisciplineFolders.DisciplineFolderFor(test, inThePack, map, separator)`, and nothing else reads a code into
  a folder, so a later switch to the team that fixes a clash changes that function alone. The default rule is the code
  of the test's first side, its left set, read by `CodeOf.Set`: the order's words first, AR and Architecture,
  ST and Structure, ME, MEP, Mechanical, PL, FP and FF, EL and Electrical, then for a code they do not name the team the
  pack's team line gives it where that is one of the four folders, HV and DR Mechanical and EV Electrical, Q184, else
  Other. A saved test the picked matrix does not name, `ClashRunOutcome.SavedTestsRunToo`, is Other. With no matrix
  picked the plan is handed null and says that list is UNKNOWN. A folder is made only where a view is written into it.
  Over 150mm stands inside Mechanical and Electrical of C only, `HoldsTheSizeFolder`. A test of C in Architecture,
  Structure or Other keeps its clashes over the threshold in its one view and they are counted and named, Q185, as
  A and B are for Q182. Tree check 2 allows a letter under the top folder, a discipline under a letter and Over 150mm
  under C/Mechanical and C/Electrical, nothing else. The plan sorts by the letter folder, then the discipline in
  `DisciplineFolders.Names` order, so a test with no priority is read with those of C.
- THE TIME OF EACH VIEW, F194, `ViewTimings` and `ViewTiming`. Every view written is timed from the parts before and after
  it, `ViewsSeconds.SecondsIn`, with the read back split into its hidden items, the walk of the material overrides and the
  colour of the clashing items, and the group says once in VIEWS TIMING lines the count, the total, the average, the first
  quarter against the last, the process's private bytes, the Pearson r of the seconds against the clashes, the overrides, the
  tree at the start and the order, and the five slowest. A number that cannot be computed says UNKNOWN. The lines change no
  view and no tree, they are read to find the cause before any fix is made.
  THE CAUSE, F196: `GetAppearanceOverrides` of a saved view lists its overrides as managed objects with a native item each, and
  nothing disposes them, so the next call is slower for every call before it, 0.75 s then 6.4 s on one view in one session
  and 8.1 then 0.5 to 0.7 s with a full collection between. The read back is 88 to 95 percent of the views, 3089 s of 3294 s on
  15 Oct 2026. The objects of each read back are collected before the next view, `ViewsPart.Collecting`, and the same
  refresh took 215 s. Anything else that lists a thousand items of a saved view has to dispose them or collect after.
- THE REFRESH OF A PICKED NWF, F187, Bader's order of 14 Oct 2026 and his answer 2 of Q174, `RefreshRun`,
  `NwcManifest`, `NwfBackupName`, `ResultSnapshot` and `RefreshComparison`. A picked NWF, the NWF page's run, is
  copied into a Backup folder beside the Log folder before anything is opened, under its name and the moment to the
  second and never over an earlier copy, `File.Copy` told not to overwrite. It is then opened with
  `FileInteractiveResolving` listened for: a linked file Navisworks cannot resolve is answered Ignore, named, and
  STOPS the run with nothing saved, so the NWF on disk is as it was and no model is removed. The linked files are
  compared with `nwc-manifest.tsv`, the tool's own state file with one fixed name in the Log folder, overwritten
  each run: size and last write time, a SHA-256 only where one of them differs, so a file written again with the same
  content is TOUCHED and not changed. `Document.UpdateFiles` is asked once. It returned False in no time on 14 Oct,
  both with no file changed and with one changed before the open, an NWF reloading its linked files as it opens, so
  a False is read as nothing left out of date and is a NOTICE, never a failure. `TryCloseFileHandles` is called
  after it so a new export may overwrite a linked NWC. The units and each model's origin are read before and after
  and said. Every clash result is read before the tests run again in place and after, by Guid and otherwise by test
  and name: status, assigned to, comments, approver and group. A Reviewed or Approved a coordinator set that is not
  the same after is named, a New found again being Active is counted and not listed, and a test that dropped clashes
  gets a WARNING with its name. The route never removes a model, appends one again, moves or renames an NWC, or
  deletes a file, and a test of its source says so. The option Close NWC/NWD files on load, Options, Model,
  Performance, is `LcOpPerformanceProps.GetCloseFileAfterLoad` and `SetCloseFileAfterLoad`, which no public member of
  the managed API returns, `Application.Options` holding only Grids, so whether the COM API reaches it is UNKNOWN
  and the NWF page tells the user to set it by hand. The route's Log and NWD folders are the open file's own, F158:
  the Log folder beside the NWF's folder and the NWD beside the NWF.
- THE TWO PAGES, F188, Bader's answer 3 of Q174 and his order of 14 Oct 2026, task 9. The window is a TabControl,
  `Pages`, whose first page is NWC and whose second beside it is NWF. The NWC page holds four steps since F232, Bader's
  round of 19 Oct 2026, fix 2, 1. Packages, 2. Grouping, 3. Outputs and 4. Clash, in `Steps`, and one Run button, RunButton,
  and is for the first build of a group. A group whose NWF is already there is not ticked and cannot be ticked, nor a group
  of one discipline, which reads `NwcPageRule.OneDiscipline`, `NwcPageRule.CanTick` and `StatusOf`, unless the Rebuild box is ticked and confirmed with
  the clash history named, `NwcPageRule.RebuildConfirm`, Cancel the default. A rebuild goes into a new dated folder
  under the NWF's and the NWD's own folders, `RebuildFolder.Name`, and never over the files there,
  `FederationEngine.TheDatedJobWhereTheNwfIsThere`. The NWF page holds its packages first since F232, a tree
  of community, package and NWF read off the Report folder, `PackageTree.ReadNwfs`, its ticks `NwfTicks`, then the tolerance
  drop down only, the list of changed NWC files of the one NWF ticked read off its last manifest and the disk, the steps Refresh, Run tests, Pictures,
  Viewpoints and Reports with a tick box each, `NwfPageSteps`, and a Run button. A step whose need is off is greyed
  and says why. The folder grouping, the matrix pack and the search set build are not on this page. The confirmation
  before the run, `RefreshConfirm`, lists the NWFs and packages, the tests, the tool viewpoint folder with its viewpoints
  and that a viewpoint moved into it by hand is deleted, and the old jpg files, with Cancel the default. Where a
  number is not known it says UNKNOWN. THE OLD JPG FILES ARE COUNTED BY ONE RULE ON BOTH CONFIRMS, F217, Bader's F-R5 of
  18 Oct 2026: `RefreshConfirm.PicturesFolderOf` is where an NWF's pictures sit, composed as the engine composes them, ImageNaming.FolderFor of the page OpenDocumentJob names in its Clash report folder, and `RefreshConfirm.OldPictures` counts, across
  every folder, the files named as the tool names its pictures, `LeftoverPictures.IsOurPictureName`, which is what F177's
  removal reads, so the one NWF's confirm and the confirm of a run of packages, which said UNKNOWN until F217, count the
  same files. A folder not there holds none, and one that will not read, denied or a drive gone, makes the count UNKNOWN, never a part, with the folder and what the disk said on a CONFIRM line. A run of several NWFs counts every one. After the run a table shows each test that moved, New, Active, Reviewed,
  Approved and Resolved before > after, from `JobOutcome.RefreshResults`. The NWF page's run, each NWF in its own Navisworks,
  keeps the pack choice by project code, the capped pictures, the stops and the order of F182. The words of the page
  are Core's, `NwfPageWords`, and what its controls hold is read off the XAML by a test.
- THE NWF'S OWN FOLDERS, F189, `NwfPlaces` and `NwfPlacesStore`. The NWC page builds a group with its NWF and NWD in
  folders of the user's choosing and the Log folder beside both. The run that built or refreshed an NWF writes where
  they are into `nwf-places.tsv` under %LOCALAPPDATA%\ParsonsNwcFederator, one NWF a line, and the NWF page's run, its list
  of changed files and its confirmation read the NWD path and the Log folder there, so the NWD, the Log, the Backup and
  the Clash report folders stay the group's own. An NWF the list does not know is run beside its file, the open file
  route's rule of F158, until one run has written it.
- The image settings: on by default, a cap per test defaulting to off, a status filter
  defaulting to New Active and Reviewed, and a size defaulting to 1024 by 1024 because
  that is what all 60 pictures of the accepted report measure. ONLY THE TEST'S MODELS SHOWN FOR
  ITS PICTURES, F145, Bader's choice of Q150, ImageOptions.OnlyTheTestsModels on by default with no
  box in the window, and only for a stretch with a row that takes a picture by
  `ImageOptions.TakesAPictureOf`, F149, which replaced F145's WouldTakeAPicture over a test's tally: which
  models stay shown is PictureModels' rule over the PictureSide of each of the test's own two sides,
  and every doubt, no side, a side not read, a side of no item, an item in a model whose file name
  another model shares, an item whose model was not read, no model read or a model outside the
  document's, keeps every model shown and says why. An image that fails renders
  nothing, is logged by name, leaves its cell empty and is told to the guard, and one such
  failure never stops the run. A
  ROW TAKES A PICTURE BY ONE RULE, `ImageOptions.TakesAPictureOf`, F149: pictures on, the
  guard not fired, its status asked for and its test under the cap, a row passed over on its
  status or the cap counted on the tally there and nowhere else. The writer reads it and so
  does the picture pass before it resolves a row's result, so a row that takes no picture
  costs no walk and a stretch with none is not resolved or hidden for. THE ROWS ONE FAULT LEFT
  ARE COUNTED AND THE FAULT IS TOLD TO THE GUARD ONCE, F149 and its second reading,
  `ImageOptions.CountTheRowsLeft`: where a stretch's test is not found again once every test
  has run, or a throw ends the stretch, each row from there on is counted on the IMAGES tally
  as the writer would have counted it had it been asked for, a row of a status not asked for
  passed over on its status, as many as the room under the cap counted as failed, the room
  taken as if each had been written, and the rest passed over on the cap, and the guard is told
  of the fault ONCE, and only where a row was counted as failed. Told once a row, one
  ObjectDisposedException at row 11 of a 1556 row test fired the guard and stopped the run
  saying fifty images failed when not one render was tried. Nothing is counted once the guard
  has fired. A clash not found again in its test is one attempt and is told on its own. The
  reasons are `ImageFailure.TestNotFoundAgain`, `ImageFailure.ClashNotFoundAgain` and a throw's
  own, the row the throw met named with the throw's words and the rest with the words that it
  ended the stretch, and the add-in says each fault on one IMAGE line with its counts, so an
  empty Image cell never reads as a picture nobody asked for. Fifty
  failures of the same reason stop the run, the same way and with the same default as the
  clash step, and the line says images rather than tests. THE GUARD IS ONE FOR THE WHOLE
  RUN AND IS READ WHERE A PICTURE IS WRITTEN, FR-076 in F120: the engine builds it beside
  the tests' guard and hands it to each group's writer the way ClashRunner takes its own,
  so fifty failures in a row across groups stop the run, and nothing is rendered, resolved
  or counted once it has fired. SINCE F149 THE GUARD FIRES LATE, A COST OF F149: every
  picture is taken once every test of the group has run, so the runner reads the guard when
  the picture pass ends, and a run whose pictures all fail runs every test of the group the
  fiftieth failure fell in before it stops, where before F149 it stopped after the test the
  fiftieth fell in. The pass breaks out of its stretches and rows as soon as the guard fires,
  and the runner reads it once more before a group's first test, for a guard an earlier
  group fired whose stop never reached the engine, row F149-R1. A
  reason is one key for one fault: a render that finished with no file says
  `Federator.Core.Report.ImageFailure.NoFileArrived` and the path beside it on the line,
  because with the path inside the reason fifty of them were fifty reasons and the guard
  never fired. The stop words are ImageFailure's, worded for images, and the label's plain
  words carry the count alone
- CLASH DETECTIVE IS ASKED TO HIDE THE OTHER ITEMS FOR THE PICTURES, F150, Bader's answer to Q151 and his
  order of 2026-10-12. The picture call draws by Clash Detective's display settings, which are per user,
  scan.md 5z-ze, so the add-in sets Hide Other for each group's picture pass and puts the user's value back
  after. `OtherItemsInPictures` is the rule of one group and its one IMAGES line. The value asked for is ON
  where `ImageOptions.GhostTheOtherItems` is unticked, its default, and OFF where it is ticked, the ghosted
  look. It is set only where it was READ and reads other than that, never where it could not be read, since
  a value never read cannot be put back, and it is read back at once after the set. The value written back,
  `PutBackTo`, is the user's as the RUN'S FIRST GROUP read it, `HideOtherAcrossTheRun`, so a put back that
  failed in one group is never taken as the user's value by the next, and a group reading another value says
  so and puts the run's back. A group that leaves it other than the user's value, or whose put back threw,
  owes a put back, tried once more at the end of the run with its own line, `AtTheEnd`, and hands the window
  one error, `TakeTheGroupError`. A group that takes no picture touches nothing and says so. THE LINE CLAIMS
  ONLY WHAT WAS READ: where the tool set the value it says whether the pictures follow is UNKNOWN until a
  run's picture sizes show it, near 48 KB a picture with the other items hidden against about 550 KB with
  them drawn, and `Describe` says Clash Detective is asked, never what it draws. The box is
  gone with its label and grey line since F232, the ghosting at the default of `ImageOptions`. A stop between the set and the
  put back is row F150-R1
  never fired. A render or a save that threw is `ImageFailure.BecauseItThrewWriting`, the
  type and the message with the picture's path and the picture's folder said as those words
  wherever the message named them, in any capitals, and the path beside it on the line, so
  fifty throws of access denied, each naming its own file, are one reason, F120-R1. The stop
  words are ImageFailure's, worded for images, and the label's plain words carry the count
  alone
- What the pictures cost is MEASURED and logged per group: seconds per image, total
  seconds, how many, and how many megabytes. Every number anyone has given for this,
  including mine, was a guess until this existed. They are in the log and nowhere else,
  because the Summary sheet they used to go on is gone. THE IMAGES STEP SAYS WHERE ITS
  OWN SECONDS WENT, CALL BY CALL, FR-077 in F120: `Federator.Core.Report.ImagesSeconds`
  holds one part per call a picture is made of, on the log's own clock, rendering, saving
  the JPEG and reading the file back, and the whole is the visits to the step added, so
  what no part holds is the bookkeeping inside a visit and never the walk between two
  pictures, which since F149 is PICTURES less IMAGES on the timing block, the pass's resolves
  and hiding among it, and was HARVEST less IMAGES before. A row passed over opens no visit
  since F149, so the visits are the pictures asked for. Set 03 timed the render and the save as ONE
  watch, 643.150 s over 5679 pictures at 0.113 s each, so which of the two cost the time
  was UNKNOWN and the speed work could not start. The line is written once per group beside
  the tally, and the tally's seconds per picture are read off the same visit, so the two
  come off one clock. The one change made to the writing keeps every picture the same bytes
  for bytes: the JPEG encoder is found once per group and not looked up on every save, which
  is what Image.Save with an ImageFormat does before calling the same overload. The size,
  the format, the quality, the name and the folder of a picture are untouched
- THE TEXTURE CACHE, F147, Bader's order of 2026-10-11, Q152 item 2: Navisworks' cache under
  %TEMP%\ogs is cleared between tests once it passes 5 GB, its size said before and after.
  `Federator.Core.Report.TextureCache` holds the rule and `TextureCacheWatch` the watch over one picture
  pass: checked as it begins and between two stretches at most once every 60 s, the interval counted
  from the end of the last check, cleared there only once a size read whole is past the limit, and read
  once more as it ends, never clearing there, for one IMAGES line of the size and the files at the start
  and the end, the checks against the limit and the reads not whole, the clearings and the files they
  removed, and the seconds the watch took, every check whole with its clearing and the clearing's
  deletions, and the last read, Q155. A count not read whole is UNKNOWN on every line and never
  nought. Bader's words are between tests, and the 60 s is the loop's, a setting, so a walk of a large
  cache is not made between every two of 741 tests. A gigabyte is 1024 cubed bytes, and a size compared
  with the limit carries its exact bytes. THE WALK NEVER LEAVES THE FOLDER: only a fully qualified folder
  named ogs is walked, any other path refused before anything is read and said once on the pass line,
  the walk is made a folder at a time and never into a link, a junction or a symbolic link, counted and
  left, since the framework's walk with AllDirectories follows links, and a cache folder that is itself a
  link is never walked. A folder inside that goes while it is walked is passed over and counted. A
  clearing walks again keeping the paths, removes only the files that walk read, never a folder, counts
  a file removed only where it was there to remove, counts what it could not remove and what had gone
  before its turn, and says the size and the files before and after. ONCE A CLEARING LEAVES THE CACHE
  NOT UNDER THE LIMIT, files held open among it, the checks go on at the interval for the rest of the
  pass, and the cache is cleared again only once it holds more than that clearing left, its size read
  after the clearing or, where that read was not whole, before it, `TextureCache.ShouldClear` and
  `LeftBy`, Bader's choice A of Q155 on 2026-10-12. So new files are cleared at the next check, and a
  cache that cannot be cleared is not cleared again while it has not grown, and the clearing's line names
  the size it must grow past. Before Q155 one such clearing stopped the checks for the rest of the pass,
  so new files grew unseen until the next group, the breaker's re-read of attempt 2. A walk that fails is
  said once with why and counted as not whole, and clears nothing. Every line names the folder walked.
  The watch never throws. The limit and the interval are settings on ImageOptions,
  `TextureCacheLimitBytes` and `TextureCacheCheckEverySeconds`, a limit of zero switching the clearing
  off. The cache held no file when read on 2026-10-12, so what removing its files while Navisworks runs
  does, and the seconds of a walk of a large cache, are UNKNOWN. A second Navisworks of the same user
  shares the folder, and its files not held open would go with the rest
- The report goes out in METERS, always, whatever the document measures in. Q23. It used
  to go out in the document's units, with the rule that converting a number ourselves
  while the document read feet would make the numbers and the label disagree. The run of
  2026-09-07 settled it the other way: 11 of 14 groups set every model to meters, the
  document still reported feet, and the report went out in feet with the tolerance at
  0.246ft. So the conversion is done, once, in one pass over the FINISHED report, in
  Federator.Core.Report.ReportUnits, before the workbook, the page or the XML is written.
  All three read the same converted report, and the same pass writes the unit label, so a
  number and its label still cannot disagree. What is converted is the tolerance of every
  test and the distance and the clash point of every row, and nothing else, because a grid
  location is text and a raw count is a count. The factors are UnitTable, the one unit
  table in this repo, read through ExchangeUnits, and a unit it has not been taught is
  REFUSED: nothing is written and the group FAILS, because a report in the wrong unit
  reads as real and is not
- Every unit this tool knows is one row of Federator.Core.Units.UnitTable, F33: the name
  on the Navisworks enum, the words the window shows, the short label the report and the
  log write, the exchange file's units code, and the millimetres in one. ExchangeUnits
  converts through it, ClashRunner and the add-in's ModelBoxReader name units through it.
  There is no other list, because the lists had drifted, the same unit was micrometers in
  one and um in another. Since F201 no model's units are set, so there is no model units
  name to judge. `PlacementCheck` says, one line a model, whether each model held its place
  from the append to the NWD within 1 mm, a setting, and names a model whose units are not
  the document's, which an NWF built before F201 carries. The exchange reader reads the tolerance as written and converts
  nothing, so a file in a unit the tool does not know reads, and ClashTestPlan.Convert,
  the one place a file unit is judged, skips each of its tests by name
- DECIDE WAITS FOR THE MODELS BEFORE IT COUNTS THEM, F74. `Document.TryOpenFile`
  returning true does not mean the models are in the document. On the first real run all
  five existing NWFs reported 0 unchanged, 4 added, 0 removed and were rebuilt, and the
  step finished in 0.248 seconds against 2.3 to 4.8 seconds for every open in the one
  earlier log on record. `Federator.Core.Rerun.ModelLoadWait` is the rule: the add-in
  reads the count and the monotonic clock and hands both over, so the run and the preview
  read one rule and cannot disagree. A count above zero that stops moving over three
  readings a quarter second apart is settled. ZERO NEVER SETTLES, because zero cannot be
  told apart from a document that has not started filling, so it is only accepted at the
  thirty second ceiling. All three numbers are settings. One LOADING line is written
  either way. Whether the API reports readiness on its own is UNKNOWN, scan.md 5e, and
  the poll is written so a member can replace it without anything else moving
- AN NWF THAT OPENED AND READ EMPTY IS STOPPED, NEVER REBUILT AND NEVER OPENED, F74.
  `NwfComparison.ReadEmpty` gives `RerunDecision.Refused` and a reason naming the file
  and what to do. Rebuilding one threw five federations and every clash result and status
  decision in them away. Treating it as a match would run every test against an empty
  document, which reports every test as passing, and a wrong answer that reads as a good
  one is worse than a stop. `NwfComparison.Compare` must NOT try to answer this: handed
  an empty list it cannot tell an empty NWF from one that has not loaded, because both
  are an empty list, and only the caller knows the NWF was just opened and waited on.
  `RunPath.Stopped` is the label and both the confirm dialog and the RESULT block count
  it. The preview reads the same two rules, because a preview that says Rebuilt about a
  healthy NWF is the confirm dialog lying
- THE DOCUMENT IS EMPTIED AT THE TOP OF EVERY GROUP, BEFORE DECIDE, F75. Before this
  there were two clears in the whole engine and both ran AFTER Decide had read the file
  list, so Decide compared the scan against whatever the previous building had left
  behind. `CensusRule.StartOfGroupLine` and `StartOfGroupReason` are the Core half: the
  first census of a group must read models 0, sets 0, tests 0, results 0, views 0, and a
  group that was emptied and still holds something is named count by count and is not
  DONE, because everything it goes on to read comes out of that document. A count that
  could not be taken is never called dirty, the same way UNKNOWN is never called a move.
  THE OPEN FILE RUN EMPTIES NOTHING, because the document IS the file list there, so it
  passes cleared false, says what it found and is never a fault. The two weekly lines in
  the confirm dialog say nothing INSIDE the NWF is cleared, which is what a person is
  asking about, rather than the old "Nothing is cleared" that F75 made untrue
- A federation that already exists needs NO SCAN. In Navisworks a person opens a file,
  opens Clash Detective, presses Run and reads the results, and nothing asks them where
  their models came from. So there are two ways to run and they are told apart by what they
  name: the Run button in the bar along the bottom, which every step shares, names a count
  of ticked groups, and the Run the open file button on the Clash step names the open FILE
  and the two paths it will write. The open path is
  the same flow with the first step removed. There is no Decide, because the document IS
  the file list, nothing is appended and nothing is cleared, so the clash results inside it
  survive. The clash file is optional there, and without one the tests saved in the
  document are run where they sit, which is the ordinary weekly case, and a document
  holding none runs nothing and the log says so. The outputs are named after the open
  file with the extension swapped and the logs and the report in the Log folder beside its folder, which is
  exactly where the scanned path puts them, so the same building run either way writes the
  same files. No pattern is applied, because the name is already on the file. Five things
  are checked before it runs and the first that fails is named in the window and the log,
  D2 on 2026-09-12: it has a name, because an unsaved document has nowhere to put an NWD
  beside it, it was opened from a folder and not from an address, it is an NWF, because
  that is where the clash tests and their results live and the NWD this tool publishes
  would otherwise be written over the file that is open, it has a folder in front of its
  name, and that folder can be read from here. The NWD path is never the open path, read
  case blind, as a second lock on the same door. The rule lives in
  Federator.Core.Rerun.OpenDocumentJob so it can be tested without Navisworks
- THE NWFS OF THE NWF PAGE, F129 and F188, Bader's request 4 under Q112, FR-178, and since F232 his round of 19 Oct
  2026, fix 2: each NWF ticked in the tree the page reads off the Report folder runs the way the open file run runs it, so
  every rule above applies to each. The picker of F129, its File and Folder buttons, NwfPick.From, NwfPickPlan and its words
  went with F232. An NWF is judged by its extension alone, .nwf read case blind, NwfPick.IsNwf, so an NWD, an NWC, a .nwf.bak
  and a .nwfx are never taken. Its outputs go into its package's folders, `PackageNwf`. THE LABEL OF A GROUP OF THIS
  ROUTE is RunPath.ForAnOpenFile, F129 attempt 2: the document already open is a Weekly run, and an NWF this run opened is
  labelled off its decision, so one that opened and read empty is Stopped, and RESULT counts it under NWF read empty, and
  one that would not open carries Build, the label the scanned run gives a group whose Decide threw. The DECIDE step's
  phrase is NwfPick.DecidePhrase, opened and read empty, not opened, or the label of the run
- Distance is the ROUNDED number and carries no number format, because that is what theirs
  holds. Ours stored -0.328083992004395 behind a format of 0.000, so the cell read -0.328
  and anyone sorting, filtering or copying the column got the long value. Rounding the
  value and rounding the display are different things and theirs rounds the value
- The Layer column carries the LEVEL. It was an item property nothing ever filled, so the
  same clash read LGF on the page and nothing in the workbook. Where the item has its own
  layer that wins, and otherwise the clash's level is used
- The Image cell is EMPTY and carries the link. Theirs holds nothing there at all with the
  picture behind it, so writing the file name put a string where their report shows a photo
- A test status of Complete is written OK, because that is their wording. Anything else
  goes through as itself
- The workbook is BANDED, and the colours are theirs, read off the accepted export. Grey
  behind every heading, blue over Item 1 and pink over Item 2, paler on the clash rows than
  on the headings, every cell boxed medium and the test header ruled thick. Ours was plain
  white with no border anywhere, which is the first thing a person sees before they see a
  single number. The five colours, the seven row heights and the nineteen column widths are
  in Federator.Core.Report.ClientLayout, and ClientLayoutTests opens their file and asserts
  every one of them against it, so the table answers to their export rather than to whoever
  typed it
- Their test header is a table NINE columns wide that stops at Status, so L to S on those
  two rows are not cells of theirs and are not expected to carry anything. Their borders
  follow the merge runs, the left edge on the first column of a run and the right on the
  last, which is what keeps our cells identical to theirs where a run is merged
- The report check compares CELL AGAINST CELL: the value, the data type, the number format,
  the fill, the borders, the row height and the column width, with both sides printed. The
  check before this compared which columns, in what shape, in what order, and passed with
  eight visible differences sitting in the file, because none of them was in front of it.
  It walks the first block only, since every block is painted by the same code and 1830
  blocks by nineteen columns is a check nobody reads
- A check that reads the same table the writer wrote from CANNOT catch that table being
  wrong, and this is the third time a check here has reported clean over a real difference,
  so what it cannot catch is written into its pass line rather than left to be discovered.
  Four things. The table being wrong, which only ClientLayoutTests catches by reading their
  file. Anything not in the compared list, which is fonts, the sheet name, freeze panes,
  print setup and merged ranges, all listed in docs\history\scan.md 4q with both sides. Whether a
  value is TRUE, since a number to three decimals off the wrong clash still reads right.
  And a block that was never written at all
- Every check gets a test that BREAKS one thing and asserts the check names it. A test that
  only asserts the good file passes would have passed against all eight of the differences
  above. They live in WorkbookCellCheckTests, fifteen tests read on 2026-10-18
- F85'S PER CLASH VIEWPOINTS ARE WHAT F114 REPLACES, and `LegacyClashView` knows them by
  the strict shape F85's own plan wrote, so the inventory removes them once the new tree is
  written and read back and never a person's view: the folders an optional priority word,
  then a pair of two codes or the unknown word joined by `PairSeparator`, the first not after
  the second Ordinal, then an optional size folder, `SubGroupFolderName`, one to three deep,
  the name a test's name, `NameSeparator`, `LegacyClashPrefix` and digits, the test one the
  document or the picked XML holds, and no comment and no redline. Those words are settings
  read for that alone since F114's add-in pass, which deleted `ClashViewpointPlan`,
  `DisciplinePairRule` and the per clash settings `MaxPerTest`, `SubGroupDisciplines` and
  `HasSubGroup`, and `DisciplineCodes`, the seven codes, is read by the coverage alone. What
  F85 decided and F114 keeps: a service at or under the threshold is never in the size
  folder, decided on the SIZE and never on the status, since F72a is off by default and
  leaves a service against a service alone, Q44, and a size that could not be read is in
  the pair view and named, never under Over 150mm, `TestViewPlan` branching on `SizeVerdict`
  and never on a reading that folds Large and SizeUnknown together, because a fitting
  usually carries no size property and dropping it would leave real geometry out with
  nothing said
- WHETHER A GROUP ASKS FOR ITS VIEWPOINTS IS ONE RULE, `ViewpointRequest.WhyNone`, F136,
  Bader's word of 2026-10-05 that every test run has viewpoints switched off until F114 is
  merged. Three things stop them, named in this order: the box on the Clash step unticked,
  the clash skipped because a model is not on the same shared coordinates, and no report
  built. The box comes first because it is the run's own choice and holds for the whole
  run, so in every group that reaches the viewpoints step an unticked run says the same
  line. A group that returns before that step, such as on a bad units name, says nothing
  about viewpoints. A group that asks for none cannot fail at them, F52's rule for a step
  not asked for. The box is a SETTING, `ReportOptions.MakeViewpoints`, defaulting to
  `ViewpointRequest.DefaultMakeViewpoints`, ON since F114's add-in pass, Bader's answer B
  to Q131 on 2026-10-05: unticked until F114 merged, so nobody made the old viewpoints,
  and ticked once it merges. Its label, its grey line,
  the RUN SETTINGS line `ViewpointRequest.SettingsLine` and the one RESULT line
  `ViewpointRequest.ResultLine` are read off Core and never typed into the window. The
  label does not say every clash, because a service of 150 mm and under gets no viewpoint,
  and the grey line says what the box costs, measured on the C02 weekly of 2026-10-05,
  which sat in VIEWS for 3 h 15 min, Q130. The RESULT line sits under the group counts and
  is written only where the run had the box unticked, because a group of such a run is
  DONE without a viewpoint and RESULT read alone would show the same DONE count as a run
  that made them
- Pipes, ducts, cable trays and their fittings OVER 150 mm go in the viewpoints and
  smaller ones do not, and the large ones of Mechanical and Electrical sit in a sub group
  of their own. 150 is a setting in millimetres, named once in
  Federator.Core.Views.SizeSettings, and over 150 means over: exactly 150 is out. Which
  properties carry a size is a setting too, Diameter, Width, Height, Size, Nominal
  Diameter and Overall Size, and the LARGEST of them is the size, `SizeRule.LargestMillimetres`,
  F72a's reading, because which one holds the size differs per kind and per exporter and
  reading one name would drop every item that calls it something else, and a 600 by 150
  duct is 600. The number a property hands back is in the
  DOCUMENT'S units and is converted through UnitTable before anything is compared, never
  compared raw: a document in feet reporting 0.5 is 152.4 mm and is IN, and comparing 0.5
  against 150 would put it out while the same model in millimetres put it in, so one
  building would give two different sets of viewpoints depending on a setting nobody
  changed. A unit the table does not know FAILS rather than falling back, which is F33's
  rule. ANYTHING WHOSE SIZE CANNOT BE READ IS INCLUDED, because a fitting usually carries
  no size property at all and dropping it would leave real geometry out of a viewpoint with
  nothing in the output to say so. In the per test views of F114 such a clash is in its
  test's view in the pair folder and never under Over 150mm, so the Over 150mm view holds
  only the services whose size was read over the threshold. Every one of them in a pair
  carrying the size folder is named with its test and clash in the VIEWS block,
  `TestViewPlanOutcome.Lines`, under a line saying how many and that none was dropped,
  FR-068, since F114's add-in pass. That is a
  deliberate departure from the rule about logging a count and five examples, which is
  about many lines saying ONE thing: these lines each name a different item that may be
  wrongly in or out, and reading five tells you nothing about the sixth. Naming every one
  is a setting and turning it off makes the block SAY it truncated. The whole rule is
  Federator.Core.Views, SizeSettings, SizeRule and SizeText, with their tests. SizeTally,
  SizeRule.Decide and SizeDecision, F53's first property reading, had no caller in src and
  went with their tests in F114. The add-in reads the properties and calls it and has no
  opinion about any number in it. A SIZE WRITTEN INSIDE A WORD IS REFUSED WHOLE, FR-066:
  DN150 mm is no size, never 50 mm, because `SizeText` takes the whole run of digits after
  a letter with the word
- ONE VIEW PER CLASH TEST OF ITS OPEN CLASHES, IN FOLDERS BY PRIORITY AND TEAM PAIR, Bader's
  Q114 points 9 to 19, F114, its Core half, with Bader's answers of 2026-10-05, A to Q118
  and Q120, B to Q119 and Q123, and C to Q117 with A where the XML's set tree names no team.
  `TestViewPlan` gives each test one view of its clashes at the `ViewStatuses` setting, New
  and Active, in the folder of its priority, A, B, C or No priority, always there, then its
  team pair, `TeamPair` read through `ViewTeams`, the one place the views read a team, and
  never copied. A SIDE'S TEAM IS `TeamMap.TeamOfSet`, the one rule the TEAMS lines read,
  with the clash XML's sets handed to `ViewTeams`: the team of the code its set name
  carries, and where it carries none the team a folder above it names, so BLD-Security
  Devices is Electrical in both. A SIDE IS THE TEST'S LOCATOR WHOLE, row F114-K30,
  `ViewClash.LeftSide` off `TestReport.LeftLocator`: where a set of the tree is at exactly that
  path, `SelectionSetDefinition.Path`, the side reads the folders of the sets at that path
  alone, so two sets of one name in two folders each read the team their own folder names.
  Where none is, or the side is a set name alone, every set of that name is read, and two
  whose folders give two teams read UNKNOWN. An UNKNOWN side is kept by its whole locator,
  so two sides of one set name write each its own reason, N5 of F114c. With no set tree read,
  a weekly run with no clash XML, the locator reads exactly the team its set name read before
  F114-K30, the code its name carries, so no pair and no size folder moves. A SET WITH NO NAME NEVER THROWS, F114
  attempt 8: the reader gives a set or a set folder with no name attribute a null name, no
  side can name it, so `ViewTeams` leaves it out, and a side with no set name reads UNKNOWN,
  as `ExportCheck` and `CodeOf` treat a set with no name. A side reading UNKNOWN is named in
  the plan's lines with why, `ViewTeams.UnknownWords`, saying only what was read: no set
  name, no set tree read, no set of that name in it, no folder naming a team, sets of one
  name giving two teams, or a code whose team reads UNKNOWN. A PAIR OF TEAMS IS WRITTEN ONE
  WAY ROUND, `TeamPair.For` with `TeamPair.Compare`: the map's lines first, then any other
  team by its name, then UNKNOWN, so one pair is one folder. Two codes of one team pair as
  that team against itself, point 10. A pair carries the size folder where the map's
  size-folder line names one of its teams, `TeamPair.TeamCarriesSizeFolder`, and with no map
  none does. The TEAMS lines say both in the words of `TeamPair.OrderWords` and
  `TeamPair.SizeFolderWords`, kept beside the rule. Built by F131 and carried here by
  Bader's answer B to Q134, the pair reads the map through its public members alone,
  `Teams`, `UnknownTeam` and `SizeFolderTeams`. In a pair carrying the size folder a clash
  whose larger service is over the threshold goes in the test's view under Over 150mm and
  every other clash in its view in the pair folder, so no clash is in two views, and a test
  with no open clash gets no view. NO MIRRORED TEST GETS A VIEW, Bader's point that there
  are no mirrored tests: the plan takes the run names of the mirrors F132's rule paired,
  `MirrorPair.MirrorName` off `MirrorRule.Pairs`, handed by the engine since F114's add-in
  pass, null and said where no rule ran. A mirror's clashes are left out and counted and the
  test named, and with no mirror rule handed in the plan's lines say so. A MIRROR NOT MERGED
  IS COUNTED BY ITS OPEN CLASHES, rows F114-K34 and F114-R1 of 2026-10-13: a mirror whose
  clashes the merge took is out of the report, so every mirror the plan meets was not merged
  and keeps its rows under its own name, and its open clashes are in no view. Each mirror is
  named with its open clashes and those at a status no view shows, and one line gives the
  open ones in all and says a clash only that mirror finds is in no view, how many UNKNOWN.
  They never go into the kept test's view, which no rule says, and whether they should is
  Bader's. The plan keeps the
  mirror list it was handed, `MirrorRule`, the one copy of it the VIEWS TREE reads. The
  views come in the order a person reads them, priority, pair in the map's order, the pair's
  views, its size folder, test name Ordinal. A VIEW SHOWS ONLY THE MODELS ITS CLASHING ITEMS
  LIVE IN, Bader's answer B to Q119 on 2026-10-05: `ShownModels` shows the model each
  clashing item lives in and hides every other, a model of the pair's own teams and a model
  whose code will not read among them. A home in a third team's model is shown and named as
  an exception, Q118 A, and a home whose model's code will not read is shown with its team
  UNKNOWN. A home that could not be read is counted and one naming no model of the group is
  named, since whether that model is shown is UNKNOWN. A VIEW'S HOMES ARE GATHERED ONCE,
  `PlannedTestView.Homes`, each clash's first home then its second, read by the tree line
  and check 3 through `ShownModels`, F114 attempt 4. A NAME IS TIED TO A MODEL BY ONE RULE,
  `ModelNames`, for a home and for a hidden model read back alike, in the plan, the tree
  line and check 3: `ContainerName.Stem` under `ContainerName.StemComparer`, without case,
  the stem and the comparer `SameName` reads for SimilarNames and the NWF folder, so a path,
  a bare file name and a display name with no extension all reach the model, F114 attempt 4
  on the breaker's finding that a home written as a path missed every model. A NAME THAT
  REACHES NO MODEL, OR MORE THAN ONE, IS NOT TIED and is named, F114 attempt 5: more than
  one is one file name in two folders, which a group gathered with subfolders can hold, and
  a home of two models shows both. A model whose file name has no stem can be reached by no
  name and is named. `PaintPlan` paints every first item red and every second green unless
  already red. `FramingBox` frames the open clash centres padded by
  `FramingMarginMillimetres`, chosen and not measured, through UnitTable, and gives a view
  of one clash no box so it keeps Clash Detective's own camera
- ONLY WHAT THIS TOOL MADE IS EVER REMOVED, Q114 point 16, F114, by the design's four safety
  rules. THE PLACE OF A VIEW IS WRITTEN ONCE, `ViewPlace`: its folders and name joined by a
  slash for a person to read, and a key telling a folder from a view and a folder named A/B
  from two folders. The inventory, the block's state of a view, checks 1, 2, 6 and 7 and the
  read backs compare a place by the key. The read backs of a view are kept under
  `PlannedTestView.Key`, that key and no other shape, F114 attempt 3. The mark compares the
  written place, so a view moved from folder A/B holding C to folder A holding B/C reads as
  unmoved, row F114-K14. A second way of writing it would make every tool view read as a
  person's. `ToolViewMark` writes one comment on every view and folder the tool makes, a
  sentence a person reads and a fingerprint of its place, name and camera, with a Guid
  field the add-in writes empty, P10 having read none on any item the tool's routes make,
  5z-p, each text written with its length so any name reads back exactly, and its author
  the `MarkAuthor` setting, P9. THE CAMERA IS JUDGED AS THE MARK STORES IT, attempt 2 of the
  add-in pass on the breaker's B6: the read camera goes through the same three decimal
  rounding the mark was written with before the distance to the marked camera is taken
  against `CameraReadBackTolerance`, so rounding alone never reads a view as moved however
  far from the origin it sits, tested at 500000 units. Whether the NWF keeps a camera at
  single precision is UNKNOWN until the timed runs. It is the
  tool's only while that one comment is there and everything reads as written, and
  renamed, moved, turned, commented on, drawn on, copied or not provable it is a person's
  and kept and named, Q120 A. `LegacyClashView` knows F85's unmarked per clash viewpoints by
  their strict shape. AN ITEM WHOSE COMMENTS COULD NOT BE READ IS KEPT, B4: its
  `ViewNode.Comments` are null, `ViewNode.CommentsNotRead`, never an empty list, since an
  unread comment may be a person's, so the inventory keeps such a leaf as not the tool's and
  such a folder as a folder, each saying why, and LegacyClashView never reads it.
  `ViewsInventory` decides every item after the new views are written:
  the tool's earlier view goes only once every view of its test this run planned is
  written, marked and read back, a per clash viewpoint only once the whole new tree is, a
  view this run could not mark or read back goes at once, a test not read keeps its views,
  and a test not read is one `TestsRead` did not read whole, B1, never one the clash step
  did not run: keyed by the report's test name, its in scope rows alone needing reading, a
  test the clash step ran with no row whole with no row, and a test whose open rows are all
  result groups in neither list, kept and never failed, F114's second reading. A test the
  clash step skipped as FAILED is never whole, met before or after, and nor is the kept test
  of a merged mirror it failed on, read by the mirror's run name, `TestsRead.ClashStepFailed`,
  row F114-K38, each saying why, since a harvest that threw after only closed rows leaves the
  report with no open clash of it while the document may hold some,
  nothing goes when the clash step was not sound, nothing goes under two folders of one
  name side by side, and a folder goes only when it is the tool's and this run empties it.
  Removals go deepest first and latest index first, 5z, and `FolderGoesWithChildren`, true
  since P14 measured a folder going with its 2617 viewpoints in one call, 5z-v, says
  whether a folder takes its views in one call. The add-in finds the view it just recorded
  as the child of its folder with its name and no mark, `ToolViewMark.CarriesAMark`, P12,
  and knows a folder that held nothing before the run off the walk before,
  `ViewNode.EmptyFolderKeys` and `HeldNothing`, F114's add-in pass, so a folder empty
  before the run is never removed and a priority folder of the old tree the run reuses is
  no twin, row F114-K5
- THE VIEWS TREE BLOCK AND ITS SEVEN CHECKS, Q114 point 19, F114. `ViewsTreeCheck.Of` reads
  a fresh walk after the removals where the document can say and the plan where it cannot,
  and says which: no pair folder holds another pair's test, no Over 150mm outside its own
  pair, no view shows a third team's model beyond the named exceptions, no clash in two
  views, no mirrored test given a view of this tool's, read off the plan's one mirror list
  against the walk after, since under Bader's answer D to Q133 both tests of a pair run and
  a mirror among the tests run is the ordinary case, row F114-K20, every view and folder the
  inventory kept
  still there, and no per clash viewpoint left without a reason. ITS after LINE COUNTS WHAT THE
  REMOVALS REPORTED, B5: `ViewsTreeFacts.Removals`, one `RemovalOutcome` per removal the
  inventory asked for, gives the removed at once, earlier views, per clash viewpoints and
  folders, each refusal or throw counted and named as not removed, and so is a RemoveAt whose
  folder did not fall by exactly one, `RemovalOutcome.Counted`, and with none handed in the
  line says what was removed is UNKNOWN, never the inventory's decisions read as done. A
  failed check is a FAILED
  line naming what broke it and the group keeps its own result. A CHECK THAT COULD NOT RUN IS
  NEVER COUNTED AS HOLDING: check 5 with no mirror rule handed to the plan, checks 1 and 2
  with no walk, no run stamp or not one planned view found marked by this run, check 3 with
  no models or no view's hidden models read back, checks 6 and 7 with no walk or no
  inventory, and 7 with no codes or test names say DID NOT RUN and why, and the last line
  counts them apart, CLAUDE.md, never report a check that did not run. A CHECK THAT RAN
  WITHOUT SOMETHING IT NAMES DID NOT RUN FOR THAT THING and is never counted as holding
  either, F114 attempt 3: a view whose read back is missing or null for check 3 or 4, a
  planned view not found marked by this run for checks 1 and 2, a view showing a model whose
  team is UNKNOWN for check 3, a view with a clashing item whose model could not be read or
  whose home matches no model of the group for check 3, F114 attempt 4, since a view whose
  homes all miss hides every model and would otherwise hold over a blank view, and the tests
  walk after for check 5 where there are mirrors. A NAME OR AN ITEM NOT TIED IS
  NEVER COUNTED AS HOLDING, F114 attempt 5, closing the class whole: every name check 3
  reads, homes and hidden models read back alike, goes through `ModelNames`, and a name that
  is blank, reaches no model or reaches more than one makes that view one check 3 did not
  run for, named and not judged, since a model it would name as shown may be the one that
  name means. A model whose file name has no stem is named once. A painted read back holds
  item paths and no name, so check 4 ties it to the view's clashes by the path: a clashing
  item that could not be pointed at, or a null item read back, makes that view one check 4
  did not run for, named and not judged. The tree line counts the hidden names it could not
  tie, and a view's read back of either kind is looked up in one place, `ViewsTreeFacts`.
  Each is named under the check, which reads RAN IN PART where nothing broke it, and
  read backs handed in under a key no planned view gives are counted in a note. The tests that
  ran with no open clash are counted off the tests the clash step ran, UNKNOWN where those
  were not handed in, since a test with no clash at all never reaches the plan. `ViewsTree.Lines` cuts the tree at `TreeLinesInLog` for the
  .log and says so, the .tsv whole, and never cuts a check. `ViewsProgress` writes a VIEWS
  line at least every `ProgressEverySeconds`, FR-071
- A group about to BUILD an NWF beside a file whose name is nearly the one it would write
  says so, and does nothing about it. On the run of 2026-09-19 Bader got First run on
  buildings that already had an NWF, because the NWF folder and the name pattern together
  did not resolve to his file, and nothing anywhere said so. CLOSE MEANS ONE OF TWO
  THINGS: the same building code sits in both names, or project, originator and building
  all agree and one of the four SUPPLIED fields differs, which is the level, the
  discipline, the type or the number. Those four are exactly the ones NamePattern supplies
  rather than reads, so they are the four a person is most likely to have set differently
  from the file on disk. The rule is Federator.Core.Naming.SimilarNames, it is handed a
  list of names and knows nothing about a disk, and the add-in lists the folder ONCE per
  refresh and hands the names over. An EXACT match is never reported, compared on the stem
  so a name with an extension and one without compare the same, because that file being
  there is what makes the group a Weekly run and not a finding. THE LABEL IS NEVER
  WIDENED: RunPath.Count maps a label it does not know onto Unknown and both the confirm
  dialog and the RESULT block read those counts, so RunAs stays one of the seven and the
  sentence rides beside it in RunAsShown, which is what the column and the group list
  block read. One grey line under the group table when any group is in that state, saying
  the NWF Name cell can be typed over. Nothing is auto corrected, nothing is unticked and
  no name is changed. Bader decides, which is the rule this tool keeps for every finding
- A CLASH BECOMES REVIEWED WHEN ALL FOUR ARE TRUE, F72, which answers Q33: one side is a
  SERVICE by item category, the other side is a SOLID by item category, the service
  measures the threshold OR LESS, and the clash is at New or Active. Everything else is
  left exactly as it is and counted by reason. The rule is
  Federator.Core.Clash.PenetrationRule, the lists are PenetrationSettings and the block is
  PenetrationTally, all three with their tests. It was off by default, because it writes into
  the NWF, until Bader's answer to Q195 of 18 Oct 2026, F235, ticked it by default on both pages,
  `PenetrationSettings.DefaultMarkPenetrations`, read by `ReportOptions`: a pipe, duct, cable tray or
  conduit, or their fittings, of 150 mm or less hitting a wall, floor or roof becomes Reviewed, a
  larger one stays New, Reviewed is the only status set and the by design box is unchanged. The lists
  keep what his earlier answers added, Q220
- THE 150 IS THE SAME NUMBER F53 USES AND IT IS NAMED ONCE, in
  Federator.Core.Views.SizeSettings. F53 puts an item in a viewpoint when it is OVER the
  threshold, so exactly 150 is OUT there. F72 marks a service Reviewed when it is AT OR
  UNDER, so exactly 150 is IN here. That is not a contradiction: one rule is over and the
  other is at or under, and together they cover every size with no gap and no overlap. Two
  copies of 150 would drift and nobody would notice until a report was wrong, so the
  comment at that one number says both readings side by side
- A DUCT IS NOT ONE NUMBER. The penetration rule reads EVERY size property the item
  carries and takes the LARGEST, through SizeRule.LargestMillimetres, because a 600 by 150
  duct has to fit a 600 through the wall and is a real coordination item. F53 took the
  FIRST property on the list, and that reading went with its tests in F114, so the views
  read the largest too, the one reading in SizeRule
- THE UNREADABLE SIZE GOES THE OPPOSITE WAY IN THE TWO RULES ON PURPOSE. F53 INCLUDES an
  item whose size cannot be read, because a fitting usually carries no size property and
  the safe mistake there is showing something unnecessary in a viewpoint. F72 LEAVES ALONE
  a service whose size cannot be read, because the safe mistake here is leaving a clash at
  New for a person to look at. Both reasons are written at both places. Do not make them
  agree
- NEVER OVERWRITE A DECISION. Only New and Active move, which is
  Federator.Core.Clash.StatusesThisToolMayMoveFrom, a rule with its own tests rather than a
  condition inside a loop. Reviewed, Approved and Resolved are all somebody's statement
  about a clash and the NWF is the only record that they made it.
  StatusesThisToolMaySet still answers the other half, which is that Reviewed is the only
  status this tool ever sets, and the two together are what stop a caller moving an
  Approved clash by mistake
- A SECOND RULE MOVES A CLASH TO REVIEWED, F72b, AND IT IS A LIST AND NOT A JUDGEMENT.
  A column on its foundation, a door in a wall, a valve in a pipe run. Every one is a
  clash and none is a problem. `Federator.Core.Clash.ByDesignPairs` reads a CSV of
  left_set, right_set, reason, since F154 the one built into the matrix pack and never a
  picked file, `ByDesignRule` decides and `ByDesignTally` is the block.
  It knows nothing about items, categories, sizes or disciplines, deliberately: reading
  every item once per clash is the walk shape that once built 1.7 million native handles
  in one group, and this rule needs none of it. THE TWO SET NAMES ARE SORTED BEFORE
  MATCHING, so a pair written one way round matches a test written the other way round.
  Set names are Ordinal and NEVER trimmed or lowered, which is the opposite of how a
  CATEGORY is matched and is deliberate: two set names in the reference file end in a
  space and the pairs file carries two different ampersand spellings, both real. OFF by
  default, the same status guard, `StatusesThisToolMayMoveFrom`, and Reviewed is still
  the only status set. THE PENETRATION RULE OWNS A CLASH THEY BOTH WANT, counted here
  under ThePenetrationRuleHasIt, so the two blocks add up to the number of clashes that
  moved rather than to twice it. A pair naming a set not in this run is a FINDING, named
  once across the whole run and not once per group, and nothing acts on it. A CLASH WITH
  NO NAME IS NEVER REVIEWED BY IT, F113, because nothing can address it to move it.
  `ByDesignRule.Judge` takes the name and asks about it last, and the block counts it under
  a reason of its own. Judged Reviewed it was counted as moved in the block, the RESULT
  line and the rule B line while the add-in left it off the list the editor applies
- THE REVIEWED LINE IS ONE RULE IN ONE PLACE, `Federator.Core.Clash.ReviewedLine`. Two
  rules now write one and only the WHY differs. A second copy of that string in a second
  tally is how the two would start reading differently, and a person scanning a log for
  REVIEWED would then find one rule's moves and not the other's
- THIS TOOL LEAVES A RECORD IN THE NWF OF EVERY CLASH IT MOVED, F72c, and the record IS
  the comment. `AutoReviewRecord` is the shape: a marker no person would type, the rule,
  THE STATUS THE CLASH WAS MOVED OFF, then the reason in plain words. The old status is
  on it because New and Active are both statuses this tool may move from and an undo that
  put everything back to New would destroy a real difference somebody made. It goes in
  the NWF and never in a side file, because the NWF travels and a side file is lost the
  first time somebody copies the federation. WHETHER A COMMENT CAN BE WRITTEN ON A CLASH
  RESULT AT ALL IS UNKNOWN, scan.md 5h. If it cannot, this tool says so in ONE LINE and
  carries on with the status alone, and NOTHING stands in for it: no side file, no
  encoded clash name, no second copy anywhere. A comment that could not be written is not
  a comment
- THE UNDO TOUCHES ONLY WHAT THIS TOOL MOVED AND ONLY WHERE NOBODY HAS MOVED IT SINCE,
  F72c. `UndoAutoReview` judges: it carries one of our records AND it is still at
  Reviewed. A clash this tool set to Reviewed in week one that a person moved to Approved
  in week two still carries the record and is LEFT ALONE, because that person's decision
  is the one thing this tool never overwrites. `StatusesThisToolMaySet.AllowsAsUndo` is a
  separate answer from `Allows` and NOT a loosening of it: an undo may set the exact
  status one of this tool's own records names and nothing else, so Approved and Resolved
  are still never set, because a record can only be written for a status this tool was
  allowed to move from and the record's own constructor refuses the other three. Q50 asks
  whether that is the right shape. A COMMENT NAMING ONE OF THOSE THREE IS NOT ONE OF OURS,
  F113, and `AutoReviewRecord.In` reads it as no record, because only a hand can have
  written it. It used to read all five and hand the three to the constructor, which
  threw, and the undo then left every clash of that test alone. AN UNDO LEAVES ITS OWN
  RECORD, FR-152, `AutoReviewRecord.UndoneText`, a comment starting
  `AutoReviewRecord.UndoneMarker`, never the marker of a move, beside the record it undoes,
  and what is judged is the LAST of ours, a move or an undo, `AutoReviewRecord.LastOfOurs`.
  Where that is an undo's record the verdict is `UndoVerdict.AlreadyPutBack` and the clash is
  left alone, so a Reviewed a person sets after an undo is never put back by the next. A later
  run that moves it again writes a newer move, which the next undo puts back. An undo whose
  record could not be written still puts the clash back, so the block's count stays true, and
  says so on `UndoAutoReview.CannotRecordTheUndoLine`. `MayUndo` and `WhyNotUndone`, a second
  copy of the judgement with the same gap and no caller in src, are gone
- NOTHING MOVES SILENTLY. A PENETRATION block per group names every clash moved with both
  categories and the service size, then the totals and ONE LINE PER REASON for every clash
  left alone, including the reasons at zero, because a reason missing from the block reads
  as one nobody thought of. The run total goes in the RESULT block, but only where the box
  was on, because a line reading zero on every run teaches people to skip it. The workbook
  carries it in the CLIENT'S OWN Reviewed column, per test, because the status is applied
  before the harvest reads it. No column of ours goes on that sheet, which is the rule
  above about the workbook being their one sheet
- THE PROPERTY PROBE READS AND CHANGES NOTHING, F86. `Federator.Core.Probe` holds the
  whole of its rule: ProbeSettings for what is read and where the CSV goes, ProbeTally for
  the counting, the cap and the order, ProbeCsv for the file, ProbeVerdict for the block.
  It takes a folder of NWC files or the open document, and writes one
  `<file>-properties.csv` beside each file with five columns, category, property tab,
  property name, distinct value, how many elements. IT NEVER OPENS AN NWF, never saves and
  never publishes, and `ProbeSettings.MayRead` is what makes that enforceable rather than a
  promise in a comment: an NWF is refused by name because opening one replaces whatever is
  open and the NWF is where every clash result lives, and an NWD is refused because it is
  something this tool writes. THE SEVENTEEN CATEGORIES ARE NOT TYPED. They are the
  thirteen `PenetrationSettings.DefaultServiceCategories` plus the four
  `DefaultNotAServiceCategories`, which together are every category a service discipline
  set in the client's matrix asks for, and a test reads the matrix and asserts it on every
  run. Distinct values are capped at 100 per property, a setting, and what was left out is
  SAID in a line of its own in the CSV and again in the verdict block, never dropped
  silently. Rows are sorted by category, then property tab, then property name, then how
  many elements carry the value, highest first, with the value itself as the tie break so
  the same model gives the same file twice running. Nothing is trimmed, because a value
  with a space on the end is a real thing in these files and the probe exists to find out
  what is really there. The verdict block says whether FS or Fire Suppression appears in
  any tab, name or value, and says so as plainly when it does not, because a probe that
  only speaks up when it finds something reads as one that found nothing rather than as
  one that ran. FS is matched as a WHOLE TOKEN and never inside a word, or OFFSET and
  TRANSFER would both read as fire suppression
- A CATEGORY A SERVICE SET ASKS FOR IS EITHER A SERVICE OR DECIDED NOT TO BE ONE, F72a.
  `PenetrationSettings.IsDecided` is the rule and `DefaultNotAServiceCategories` is the
  second list: Air Terminals, Mechanical Equipment, Plumbing Fixtures, Sprinklers. The
  test that reads the client's matrix asserts the DECISION and not the membership, because
  the literal reading would force Mechanical Equipment onto the service list and an air
  handling unit against a wall would be moved to Reviewed automatically, which is the
  opposite of what the penetration rule is for. A condition written as CONTAINS is a STEM
  and not a category: Cable Tray matches Cable Trays and Cable Tray Fittings, Conduit
  matches Conduits and Conduit Fittings, and all four are on the service list. Air
  Terminals and Sprinklers are arguable both ways and are Q47
- The scan reports what it noticed and never acts on it. ODD SHAPE, NEAR MATCH,
  SINGLE DISCIPLINE and MISSING are information. Nothing is blocked, unticked or
  merged, and no code is assumed right. Bader decides
- Findings are worked out from the run itself. The set of disciplines and the set
  of code shapes come from the files in front of it, never from a list in the
  code, because every project differs. A shape is the pattern of letters and
  digits, so 1B06PK is 9A99AA. A code is odd only when its shape is held by no
  other code and some other shape is shared, because with every shape unique
  there is no majority to differ from
- Findings show under the tree of step 1. Packages before Run is pressed, over the NWC files of the
  ticked packages, F232, and go in the log in a FINDINGS block after the PACKAGE GROUPS block. Nothing odd is one line, not an empty
  panel
- A SHIFT CLICK ON A RUN BOX TICKS OR UNTICKS A RANGE, F130, Bader's request 5 under Q112,
  FR-179. `Federator.Core.Grouping.ShiftRange.Of` is handed the rows in the order the grid
  shows them, the anchor, which is the row clicked last, with the state that click left it
  in, the row clicked now with the state its own box flipped to, and whether Shift was held.
  With Shift it hands back every row from the anchor to the row clicked, both ends included,
  in the shown order, each to take the ANCHOR'S state, so a sorted or filtered grid ranges
  over what the person sees. Anything else is a plain click and hands back the row clicked
  alone with its own state: no Shift, no anchor, an anchor the grid no longer shows, the
  anchor itself, or a row the grid does not show. Rows are matched by reference, because the
  groups made again are new rows under the same buildings. The rule knows nothing of a
  blocked row, which the row itself refuses. The grey line under the group list is
  `ShiftRange.HelpLine`, at most twelve words by the tick box rule

## Communities and packages, F208

Bader's message of 17 Oct 2026 late evening, feature 1. The folder is Federator.Core.Communities and not Packages,
because the NuGet line packages/ of .gitignore reads a folder of that name case blind and keeps it out of every commit.

- THE TREE IS READ OFF ONE PARENT FOLDER, `PackageTree.Read`. The first level under it is the community. Every folder
  at any depth below a community that directly holds files of the kind read is ONE PACKAGE, its own files only, so a
  folder holding files and subfolders with files is one package and each subfolder another, and two folders are never
  mixed into one package. The flat layout, a community holding its files itself, is the package the community is, named
  `PackageWords.ItselfName`, and the ACC layout, folders of files under it, are packages side by side. Folder names with
  spaces, the word and or an ampersand read like any other. A file is judged by its extension alone, read case blind, a
  folder is listed whole and never matched by a wildcard, the disk is handed in as `NwfPickDisk` so every rule is proved
  on folders a test makes. A link or a junction is named and never followed. A folder that will not read is named with
  what the disk said and the rest are read. A PARENT HOLDING FILES ITSELF IS ONE PACKAGE NAMED AFTER IT, F232, Bader's round
  of 19 Oct 2026, fix 2, the community of the parent's name holding the package `<parent> itself`, unless a folder under the
  parent carries that name, where the parent's own files are left out and said, Q213. A community with no file below it is
  named in `LeftOut` and is in no package, never dropped in silence. A Report folder that would put a package's own report
  folder in the parent or inside it, Report\Parent being the parent where the Report folder is the parent's parent, is
  refused before anything, `PackageOutput.WhyNot` handed the packages
- THE NWF PAGE'S TREE IS READ OFF THE REPORT FOLDER, F232, `PackageTree.ReadNwfs`: the first level under it the community,
  and a folder below it whose NWF folder, `PackageOutput.DefaultNwfSubfolder`, directly holds NWF files is one package at the
  path the NWC page wrote it under, its files those NWFs in name order, so `PackageNwfs.For` finds the same files. The NWF
  folder is not walked again. A link is named and never followed, a folder that will not read is named, and a community
  holding none is left out and said. Nothing is ticked at the start, `NwfTicks`: a community or a package ticks every NWF under
  it, one NWF is ticked alone, a path the tree does not hold is never ticked, and a run is refused while none is ticked
- NOTHING IS TICKED AT THE START, `PackageTicks`, so a parent of ten communities never runs all ten by mistake. A
  community's tick sets every one of its packages, `StateOf` reads neither for a community partly ticked, and Tick all and
  Clear all set every package. `WhyNoRun` refuses a run while none is ticked, and a tree not read refuses with its own
  reason. The ticked run in the tree's order. `Lines` names every package ticked and every one left out with its path from
  the parent, its files and its full path, then the count of each. EVERY PACKAGE LINE OF THE PACKAGE TREE AND PACKAGES
  TICKED BLOCKS CARRIES ITS FULL PATH, F216, Bader's F-R4 of 18 Oct 2026, `PackageWords.PackageLine`, the one shape of a
  package's line in both blocks, so each line alone says where the package is. The path from the parent stays first, since
  every group line names a package by it
- A PACKAGE WRITES UNDER ITS OWN REPORT FOLDER, `PackageOutput`: the Report folder and the package's path from the parent,
  such as Report\C06\Infrastructure, its NWFs in NWF and its NWDs in NWD under it, `DefaultNwfSubfolder` and
  `DefaultNwdSubfolder`, so `ReportPaths.LogFolder` puts the Log and the Clash report under it as well and every output of
  a package is in its one folder. A Report folder that is the parent or inside it is refused, the rule that the reports
  never go inside the folder being scanned. The folders are made only for a ticked package that runs, one line each saying
  made or there
- ONE PLAN PER PACKAGE, `PackagePlan.For`: its files parsed with the naming settings, gathered by the grouping chosen,
  named by the patterns, each group's NWF and NWD in the package's folders, a name the scanned run's guard refuses
  refusing that package and no other, a file that does not read and a group the grouping blocks named and run nothing. A
  group is named with its package, `PackagePlan.LabelFor`, the package's path then the group, the words every GROUP line
  and RESULT carry, and the plain group where a run has no package
- THE NWF PAGE RUNS THE NWFS THE NWC PAGE WROTE, `PackageNwfs.For`: the NWF files directly in the package's NWF folder,
  judged by `NwfPick.IsNwf`, in name order, each with its NWD in the package's NWD folder and its Log folder by
  ReportPaths, so the two pages write into the same folders of the same package. No NWF there gives none, and the run names
  the package with `PackageWords.NoNwfYet`
- RESULT NAMES EVERY GROUP WITH ITS COMMUNITY AND PACKAGE, `PackageTally`, then one total per package, one per community
  and one for the run, written by `RunLog.WriteResultBlock` under the group counts. A package that ran nothing is named with
  why. A clash count not taken is UNKNOWN and a total holding one says at least
- The words of the step and the panel are `PackageWords`, the buttons within the tick box rule, and the two pickers are
  `PickerKind.PackageParent` and `PackageReport`, each remembered on its own
- A FRESH NAVISWORKS PER PACKAGE, F214, Bader's answer to Q204 of 18 Oct 2026, measured in scan.md 5z-zj. What passes
  between the starts is Core's. `PackageRunOrder` is what the window a person runs hands every package's Navisworks: the
  page, the parent and Report folders, the packages in their order, every control of the window by name as `ControlValue`,
  text, tick or choice, the run's own folder named for its second by `FolderFor` and never one already there, the plugin and
  the `PackageRunSettings`, a ceiling of 30 minutes a group since F222, 240 a package before it, a wait of 60 s for a Navisworks to be gone and a poll of 200 ms,
  all chosen and not measured, each refusing what cannot work, and `RunnerLimitMinutes`, every package's ceiling and wait added
  up, past which the window ends the runner. `PackageRunResult` is what each package's Navisworks writes back: its groups,
  each with its outcome, its clashes, minus one where not taken, and its reason, why none ran, its own log and the lines of its
  own RESULT block, `ResultLinesOf`, everything after the last `RunLog.ResultTitle` between two `RunLog.SectionRule` lines,
  the title and the rule RunLog writes it with and no copy of them, proved on a block RunLog wrote. Both files are one shape,
  `PackageRunFile`: a first line naming what the file is, then a key and values a line, each value escaped by
  `EventRow.Escape`, the machine readable log's rule, so a path with a tab or a line break reads back whole. A file not there,
  of another first line, with a key it does not know, a wrong count of values, a number that is not one or that the settings
  refuse, an outcome that is not one by its name, or a result with no end line, which is a Navisworks that stopped writing it,
  reads as null with why and never as a part. `PackageRunWords` holds every line the runner and the window write about it and
  the one RESULT, `JoinedResult`, written by `RunLog.WriteJoinedResult`, which closes the log's account as every RESULT does:
  the group counts read off the package tally, `PackageTally.CountOf`, and never counted a second time, how many packages wrote
  no result and how many ran no group, the tally, each group not DONE with its reason, then each package's own RESULT under its
  name and its log, or NO RESULT with why. A PACKAGE WITH NO RESULT IS NEVER SAID AS NOT RUN, `PackageTally.NoResult`, since
  its groups may have written files before its Navisworks ended, and whether any did is UNKNOWN. The live line counts the
  packages that wrote a result, `Finished`, and `Warning` is the window's warning where one did not. Proved by
  `PackageFreshStartTests`
- A FRESH NAVISWORKS PER GROUP, F222, Bader's answer to Q211 of 18 Oct 2026, not only per package, on both pages, Run
  pressed once. The order names each GROUP of the run, `PackageRunUnit`: its package, its NWF, which no two groups of a run
  share and which is the one key a group's own Navisworks finds its group by, and its name, the building on the NWC page and
  the NWF's name on the NWF page, a package's groups together in the tree's order. The order's first line names its shape, 2,
  so an order of the shape before is refused. Every count and word above that read a package now reads a group: one result
  file a group, `ResultPathFor`, the runner's lines, `Starting`, `Started`, `Gone`, the live line, the warning, and the
  joined RESULT, which names every group under its package and name, `JoinedResult` over the units. A GROUP WITH NO RESULT
  IS NAMED ON ITS OWN LINE AND NEVER AS NOT RUN, `PackageTally.NoResult(package, group, why)`, and one whose Navisworks ran
  nothing on its own, `NotRun(package, group, why)`, and its package, community and run lines add how many groups have no
  end to count and that their clashes are then at least the total, never exact. An order of two groups writing one NWF is
  refused, `WhyNot`, since neither could find its own, and the one rule a group is found by is `PackageRunUnit.IsTheGroupOf`,
  without case. The live line and the warning count a group as ran only where its result names a group. A group whose files
  were not read, its result missing or an NWF whose models never read, is said not read by the missing discipline check and
  never missing a discipline, `GroupDisciplinesCheck.Missing`. The ceiling is one group's since F222, 30 minutes over the 139.7 s
  of the longest group of F220. The
  runner's last line adds up the seconds of every start, from the call that makes the Navisworks to its process told, and of
  every quit, from the call that tells it to quit to its process gone, `RunnerEnded`. THE DISCIPLINE REPORTS ARE WRITTEN BY
  THE WINDOW THAT STARTED THE RUN, since no Navisworks holds every group of a package: each group's result carries its files
  and the facts of its models, `PackageRunGroup.Files`, `Facts` and `FactsNotRead`, a value not read written with the word
  not and its why so it never reads back as a zero or as none found, and `PackageRunFacts.GroupsOf` turns the results of a
  package into the groups its workbooks read, on the NWC page every group of its plan by its NWF and on the NWF page every
  group of the run in that package, a group with no facts kept with its files and why. A facts line out of its place, of the
  wrong count or a number that is not one makes the whole result no result. Proved by `FreshNavisworksPerGroupTests`

## The discipline reports, F209 and F234

Bader's message of 17 Oct 2026 late evening, feature 2, rewritten by his round of 19 Oct 2026, fix 4, F234.
Federator.Core.Disciplines. ONE WORKBOOK PER DISCIPLINE PER PACKAGE, AR, ST, ME and EL by default,
`DisciplineReportSettings.Disciplines`, each one sheet named after the discipline, saved in the package's report folder under
`DisciplineWorkbook.NameFor`, the package's path in words, the discipline and the suffix, the name it had before F234, a name
already there taking a dated one by `ReportPaths.FreeFilePath`.

- EVERY CELL IS MADE BY A RULE OF THE FOLDER and none is free text, the words `CheckWords`. Rows 1 to 3 the title
  `<community> <package> <discipline> model checks`, `CheckWords.Title` with the package's name, the run date and the reference
  discipline. Then the summary table from row 4, `DisciplineWorkbook.SummaryHeaderRow`, Check, Files checked, Issues and Result,
  `CheckWords.SummaryColumns`, the seven checks in Bader's order, `CheckWords.All`: Coordinates, Missing model, One discipline
  group, Naming, File age, CAD, Generic Models. Result is OK where Issues is nought and Issues otherwise. One blank row, then the
  issues table, `DisciplineWorkbook.IssuesHeaderRow`, its header row frozen and carrying the filter, columns No., Severity,
  Building, File, Check, What is wrong, Value, Action, Element IDs, `CheckWords.IssueColumns`. Headers bold on grey, every
  issue cell wrapped, the widths `DisciplineWorkbook.Widths`
- ONLY ROWS THAT NEED ACTION, `DisciplineIssue`, and no row for a file that passed. What a check could not read is never a
  pass and never a row: it is a line of the DISCIPLINE REPORT block in the log, `CheckFindings.Notes`, and the file is not in
  the check's Files checked. THE SUMMARY'S ISSUES ARE THE ROWS OF THAT CHECK IN THE SAME WORKBOOK, `DisciplineReport.SummaryFor`,
  by construction, and the workbook read back off the disk is held to it, `DisciplineWorkbookRead.SummaryMatchesTheRows`, the
  CHECKS line saying THE SUMMARY AND THE ROWS DIFFER where it does not. Files checked is the files of the workbook's discipline the
  check read, and for Missing model the files of the groups of two disciplines or more, the lead's reading, Q218
- SEVERITY, `CheckWords.SeverityOf`: High for Coordinates and Missing model, a light red row, `DisciplineWorkbook.HighFill`,
  Medium for Naming, File age and One discipline group, light amber, `MediumFill`, Low for CAD and Generic Models with no fill.
  The rows are sorted by severity, then building, then file, Ordinal, and where all three are one in the order the checks
  made them, a stable sort, `DisciplineReport.Sort`. Building is the code read off the file's name, and for a row about a group
  and no one file the codes of its files, `DisciplineOf.BuildingsOf`
- COORDINATES, `CoordinatesCheck`: ALIGNMENT'S RULE AND NUMBERS AND NEVER THE BOX GAP. Each model is placed by the translation of
  its transform in millimetres, the one read of it, `ModelFactsReader.TranslationOf`, carried as `ModelFacts.X`, `Y` and `Z`,
  measured from the group's reference, `AlignmentCheck.ReferenceIn` with the setting's reference discipline, by the straight line
  of the differences, `AlignmentCheck.StraightLineMillimetres`, which the ALIGNMENT block reads too. Over `FarMetres`, 30 m,
  Bader's answer to Q202, a model is a row in its own discipline's workbook, the value the distance in metres to three decimals
  and both shared sites, `CheckWords.FarValue`, Action Export again on the project shared coordinates. A reference on Internal,
  `AlignmentCheck.NamesInternal`, while another model of its group names a shared site that is not Internal is one row in the
  reference discipline's workbook, on Revit internal origin, its value the other models' sites, Action Export on the project
  shared coordinates. ST of 1B06BC sits 34.014 m and 34.472 m from AR, read off its ALIGNMENT block of 19 Oct 2026 and held by a test
- MISSING MODEL, `GroupDisciplinesCheck.Missing`, off the file names by F219's one rule, `BuildingGroup.DisciplinesOf`: in a
  group of two disciplines or more, one row in the workbook of each discipline of the setting it holds no file of, no
  <discipline> model in this group, its value the disciplines the group holds, Action Issue the <discipline> model for this
  building. A group whose files were not read, or whose disciplines that rule says are UNKNOWN, a name that does not read or a
  model of the federated discipline, is a line of the log and never counted as missing one, F222. ONE DISCIPLINE GROUP,
  `GroupDisciplinesCheck.OneDiscipline`: a group F233 did not build is a row in that discipline's workbook, only discipline in
  this group, not federated, its value its file names. The decision is the run's, carried as
  `DisciplineGroup.NotBuiltForOneDiscipline` by `PackageRunFacts`, the plan's `BuildingGroup.CannotClashWith` on the NWC page and
  the NOT BUILT line, `OneDisciplineGroups.IsTheLine`, on the NWF page, and never judged a second time off the files, so an NWF
  built with a link not on the disk is never such a row, the reviewer's finding on F234's first pass. Its files reach the report on both pages: on the NWC page off its plan, on the NWF page
  off its result's `PackageRunResult.NotBuiltFiles`, read off its NWF's models, and a package whose every group is of one
  discipline still gets its workbooks, rows F233-R2 and R3
- NAMING, `ReferenceNaming`, not against ISO: the reference discipline's names split on the naming separator, the part count
  most of them have, each part all of them hold alike, and each part's MERGED SHAPE, `MergedShape`, a letter, a digit, either
  where a letter and a digit meet, or a mark, the length kept, so C06's building codes 1B06BC and 1B06G1 share one. The
  discipline's own part is left out and the parts of `ShapeOnlyParts` are judged by their shape alone. Only a file of another
  discipline that breaks the pattern is a row, one row per part broken, `NamingBreak`, What is wrong the part and what it reads,
  Value what the reference has there, a shape said by naming a real part that has it. The pattern learned is a line of the log,
  and with no reference file none is learned and the log says so. The reference's own files are the pattern and not judged
- FILE AGE, `FileAgeCheck`: only a file whose date the disk gives older than `OlderThanDays`, 7, is a row, its value the date
  and the age in whole days. Exactly the limit is within it
- CAD AND GENERIC MODELS, `ElementChecks.Cad` and `Generic`: one row per file and CAD file name, and one per file, family and
  type, each with its count in Value and every element id comma separated, the setting's words, not readable, for an id not read.
  `LevelName` and `CadFileName` read what the add-in hands in, both measured in 5z-zg. A model with none is no row, and one whose
  items were not walked is a line of the log
- The Action cell of One discipline group, Naming, File age, CAD and Generic Models is empty, since Bader's words give it for
  the other two checks alone, Q218
- A ROW NO WORKBOOK TAKES, a file of a code outside the list or whose name does not read, is in `DisciplineReport.LeftOut` and
  counted by its code on the DISCIPLINE REPORT block. The internal names the add-in reads are `DisciplineProperties`, each
  measured and typed once

## The clash count check, F236

Bader's round of 19 Oct 2026, fix 6. Federator.Core.Communities. EVERY RUN WRITES ONE WORKBOOK A COMMUNITY,
`ClashCountWorkbook.NameFor`, "<community> clash count check.xlsx" in the community's report folder,
`PackageOutput.CommunityFolderFor`, a name already there taking a dated one by `ReportPaths.FreeFilePath`, its one sheet named
by `SheetNames.ForReport`, written by the window that started the run once every group has run, on both pages, and on a run of
the NWC page whose every group is of one discipline too.

- EVERY NUMBER IS READ OFF THE FILES THE GROUP WROTE. `GroupRunFiles.Read` reads the group's .tsv a row at a time by
  `EventRow.Parse`, the one reader of the shape `EventRow.Line` writes, and each event by its name in `RunEvents`, the one place
  the writers and this reader name them: each test's clashes as Clash Detective counted them, nought for a test that passed, the
  paths of the clash workbook and the HTML page written, the name the writer gave the workbook's sheet, `RunEvents.WorkbookSheet`,
  since the file's name need not carry it, the reviewer's finding 1, each test's open clashes in its views off the VIEWS TREE
  lines, `ViewsTree.ReadViewLine`, the one reader of the shape the block writes, the views of one test added up, the mirrors the
  rule paired, `RunEvents.Mirror`, those the merge took and the clashes only they found, `MirrorMerged` and `AddedByMirrors`, and
  the tests the rule of one discipline and of two set codes left out, read off each coverage row by `CoverageBlock.RowGives`, the
  one reader of the shape `RowText` writes. The clash workbook is read test by test by `WorkbookTests`, each block's rows, its
  pictures by the link its Image cell carries, its rows by the Status column and its letter off the Priority column. The page is
  read test by test by `Federator.Core.Report.PageTests`, off the shape Navisworks' stylesheet gives it, proved on the client's
  own page in samples
- A COUNT NOT READ IS UNKNOWN AND NEVER A NOUGHT, `ClashCountRow.Unknown`, written UNKNOWN, judged against nothing and left out
  of every total, whose Why skipped cell then says at least: the workbook or the page not written, not on the disk or that would
  not read, a test with two blocks of one name, a block whose clash table was not read, a test the .tsv names no count of, the
  views where no VIEWS TREE was written, the box unticked or the NWF page's step off, and a test one of whose views was not read
  back, so a view that failed never adds its clashes as agreeing, the reviewer's findings 2 and 3. A workbook or a page read that
  holds no block of the test is a real nought. A row with a count UNKNOWN has a COUNTS UNKNOWN line naming each, and is not red,
  since nothing was read to judge it against. Whether such a row should be red is Q223
- ONE ROW PER GROUP PER TEST WITH CLASHES, `ClashCountRow`, Bader's columns in his order, `ClashCountRow.Columns`: building,
  test, priority, clashes in Clash Detective, Excel rows, HTML rows, clashes in viewpoints, pictures made, pictures skipped, why
  skipped, New, Active, Reviewed, Approved, Resolved. A test is one Clash Detective found clashes in or one the workbook holds a
  block of. Pictures skipped are the Excel rows with no picture, and why, `ClashCountCheck.WhySkipped`, says every one where the
  run took no pictures, then splits them into those at a status the pictures do not take, those past the cap of a test of more
  than the setting's clashes, Q187, read off the block's Clashes cell as the pictures read `TestReport.RawClashes`, and the rest
  UNKNOWN, a stop, a failure or a retake showing only the sky, which the group's IMAGES and RETAKE lines name
- THE MIRRORS, F132: a mirror the merge took into its kept test is no row, its clashes being the kept test's, and the kept test's
  Excel and HTML rows are held to Clash Detective's count and the clashes only its mirrors found, `ClashCountRow.RowsExpected`, as
  the kept test's ROWS line holds them, with a COUNTS MIRRORS line. A mirror the merge did not take keeps its rows and is held to
  no view, `UnmergedMirror`, since no mirror gets a view. How the workbook should show them is Q223
- THE FOUR COUNTS, `ClashCountRow.Differs`, over the counts read alone: a row is red and has a COUNTS DIFFER line where the rows
  Clash Detective's count gives differ from the Excel rows or the HTML rows, the two differ from each other, or the clashes in its
  views differ from its rows at the statuses a view shows, `ViewsExpected`, the run's view setting handed in,
  `ViewpointSettings.ViewStatuses`, New and Active by default, since a view holds the open clashes alone and a clash the penetration rule set
  Reviewed is in none, the lead's reading of his words, Q222. A total is never red
- A TOTAL PER GROUP AND ONE FOR THE COMMUNITY, `ClashCountRow.Total`, then the groups not built for one discipline, off the plans
  on the NWC page and the NOT BUILT line of a result on the NWF page, then the tests a rule left out, a count a group, a group not
  built reading every test left out, then the groups whose counts could not be read, each with why, `ClashCountCheck.NotRead`,
  never dropped in silence: no result, a result naming no group for another reason, no .tsv, a .tsv naming no test that ran,
  `GroupRunFiles.AnyTestRead`, the breaker's S1 on attempt 2, or a read that threw, and the community's total then says at least. A
  test whose harvest threw after Clash Detective counted it keeps that count on a row of its own, `RunEvents.TestFailedAfterItsCount`,
  and a group with no coverage row reads its tests left out UNKNOWN. The CHECKS line
  reads the workbook back off the disk against the check it was written from, its test rows and its red ones, and says THE
  WORKBOOK DIFFERS with both sides where they do not match, `ClashCountWorkbook.ReadBackLine`

## The diagnostic log

The log is what Bader sends back when something goes wrong, so it is built to
survive the crash rather than to be tidy.

- every build stamps the assembly with the git commit and the moment it was built,
  into AssemblyInformationalVersion, and the log prints it in SESSION and the
  window shows it in its title bar. The plain assembly version is 1.0.0.0 and
  always will be, so on its own it cannot tell a fresh install from a stale one and
  Bader ran an old binary twice before this existed. Never fake this with a hand
  edited version number. It costs a full recompile on every build, which is about
  three seconds, and that is the trade

- every line is written and flushed all the way to the disk as it happens, with
  FileStream.Flush(true). Nothing is held back to the end, so a process that dies
  inside a Navisworks call still leaves everything up to that moment on disk
- it opens on the first line of the button handler, before the folder is read and
  before the window opens, so a run that dies at startup still produces a file
- ONE PLACE, the fixed path
  %LOCALAPPDATA%\ParsonsNwcFederator\logs\run-yyyyMMdd-HHmmss.log, which never depends
  on a folder the user picked. No copy goes beside the NWF, Bader's decision of
  2026-10-13, F151, after his run of that day on 1A02MM wrote the copy into his NWF
  folder, and RunLog.TryCopyTo and NwfPick.LogFolder are gone. Since F158 the log and its
  .tsv are copied into each Log folder the run wrote into, once the RESULT block is on the
  disk, `LogCopy.Into`, so its LOGS lines come after RESULT. In a package's own Navisworks,
  F214, one line naming the package's result file follows them and is not in the copy, and
  the window a person runs for a run of packages copies nothing, its joined RESULT last
- logging is never the thing that stops a run. That holds for retention and for the
  log file itself, which falls back to the temp folder and then to window only output
- a size is only logged after File.Exists passes and the real size is read back.
  Never log a size that was not read
- A SAVE THAT THREW LISTS NOTHING AS WRITTEN, `RunLog.WriteThrew`, F118-R3. A save goes
  straight onto the final path, so after a throw the file there may be an earlier run's
  untouched or one this save cut short, and which is UNKNOWN. WriteFinished would list it
  with its size as this run's, which is what the clash workbook's write did with last week's
  workbook. WriteThrew records nothing, takes an entry of that path and kind off the list,
  returns minus one and writes one NOT WRITTEN line saying whether a file of that name is at
  the path. The clash workbook and the internal workbook both write through it. FR-189 is the
  same fault for the NWD and FR-044 and FR-045 for the NWF, area F119
- on start, the oldest logs are deleted until 30 remain, the live file included, bar the log of a group's own Navisworks of a
  run of packages, which deletes none, `RunLog.KeepEveryLog`, F222, so a later group never deletes the log of one before it.
  The live file is never a candidate. A delete that fails writes one line naming
  the file and the reason. 30 is a setting, RunLog.KeepLogs, static because the log opens
  on the first line of the button handler before a window or any options object exists.
  Zero keeps the live file alone and is a real answer, fewer than none is refused

### The steps, F59

Where the time went is the question the log exists to answer, and it can only answer
it if the same work carries the same name every time it is timed.

- the step names live in `Federator.Core.Diagnostics.RunSteps` and nowhere else. Nothing
  types a step name as a string. Twenty of them, in the order a group meets them since F182
  and F187: DECIDE, REFRESH, APPEND, NWF SAVE, UNITS, SETS, TESTS CREATE, TESTS RUN, HARVEST,
  VIEWS, NWD, CONFIRM, PICTURES, IMAGES, RENUMBER, WORKBOOK, HTML, XML, COVERAGE, INTERNAL.
  PICTURES is F149's, the picture pass once every test has run, which its attempt 1 ran
  inside no step. VIEWS is F85's and it was untimed before
  that: building the viewpoints was called outside every step, so its seconds came off
  no total and the run read as faster than it was. RENUMBER is FR-077's in F120, the
  pictures renamed into report order once the clash step has run every test, and it was
  untimed before that the same way, 20.6 s of one group of set 03 in none of the totals.
  A name that is not on the list is refused where the
  step opens, because a timing block holding a step nobody named is worse than a short one
- a step is always opened in a using block, so it closes on the way out whether the work
  finished, returned early or threw. A step left open is the one thing that would make
  the block lie, because seconds it never recorded come off no total and the run reads as
  faster than it was. A group that ends with one still open names it and says NEVER CLOSED
- a step whose work threw says THREW and still carries its seconds. Time spent failing is
  time the run spent
- the clock is monotonic, `Stopwatch` through `RunLog.ElapsedSeconds`, and never two wall
  clock readings subtracted. A clock that goes back, which is what a machine syncing its
  time does, would otherwise give a step a negative duration and a group a total smaller
  than one of its own steps. A step never reports less than no time
- one clock per piece of work. The step IS the measurement, so where a Stopwatch was
  already timing the same thing it went, and the seconds the log reports and the seconds
  the report row carries come off one reading
- a step entered more than once in a group writes its start and finish pair the FIRST
  time, one line the second time saying the rest are counted, and nothing after that. The
  totals go in one line per repeated step when the group finishes, with the visits and the
  seconds. TESTS RUN is entered once per test and a real group holds 1830 of them, so the
  pair per visit would be 3660 lines. This is the same rule the repeated failure follows
  and it is here for the same reason, a run that once left a 17.8 MB log
- a step inside a step is indented by its depth and carries that depth, so the timing
  block can work its shares out over the top level alone. IMAGES is the only one nested
  today, inside PICTURES since F149, because each picture's visit is taken inside the picture
  pass. It nested inside HARVEST while a picture was written as the harvest walked the
  results, so against a run before F149 HARVEST drops by the pictures and PICTURES is a row of
  its own. TESTS CREATE, TESTS RUN, HARVEST and PICTURES each open and close on their own
- outside a group there is no group to count visits against, so each press of the two
  hand buttons on the Clash step is its own occasion and writes its own pair of lines
- the step never changes what the run does. It opens, the work runs exactly as it did,
  and it closes. Nothing is skipped, reordered or waited for

### The timing blocks, F60, which closes F21

- a TIMING block per group, written straight after its GROUP finished line, and a
  TIMING, THE WHOLE RUN block just before RESULT, so RESULT is the last block of the run,
  the LOGS lines of F158 after it. Both are `Federator.Core.Diagnostics.TimingBlock` and nothing else builds them
- every number is measured. The group total is the engine's own clock as GroupFinished
  read it, and the run total is the log's elapsed clock read where the block is written,
  NOT the groups added up, because the scan and the preview happen outside every group
  and a total that left them out would be smaller than the run took
- whatever the steps do not account for is a ROW of its own, named outside every step,
  so the share column reads down to a hundred. A block that spread it over the steps it
  does know about would be inventing numbers, and one that left it off the page would
  leave a reader guessing. There is a test that adds the column up
- steps adding to MORE than the group took is said in words. It means something was
  timed outside the stretch the group clock covered, which is a fault in the timing and
  not in the run
- a nested step is left out of the share column and listed under the total, because its
  seconds are already inside its parent's and counting both would give a group shares
  adding to more than a hundred
- the run block reads twice: the groups slowest first, then the same seconds by STEP
  NAME added across every group. Which building cost the run and which step cost it are
  two questions and only the second one says what to fix
- the run block ends by saying whether the run fitted in the forty five minutes
  criterion 2 asks for, with the measured time in minutes and in seconds either way. One
  second over is OVER. The forty five is `TimingBlock.UnattendedSeconds`, a setting, and
  a value at or below zero is refused where it is set
- one line per test, `ROWS`, carries what the workbook got beside what the document
  holds, COLLAPSED in the text log since F81 and never in the machine readable one, so criterion 3 is answered off the log rather than by opening Excel and
  Navisworks side by side for 1830 tests. The two numbers are not the same thing: the
  tally counts leaves, descending into every result group, and the harvest writes one
  row per group without descending, which is also what the panel shows. So FEWER rows
  than clashes is the grouping and the line says so. MORE rows than clashes is a finding
  nothing in this tool explains, said in capitals, and nothing acts on it
- THE VIEWS STEP SAYS WHERE ITS OWN SECONDS WENT, CALL BY CALL, FR-073 in F114.
  `Federator.Core.Views.ViewsSeconds` holds one part per call the viewpoint work is made
  of, on the log's own clock: reading the clashes and planning, saying how far it has
  got, looking whether each was already there, showing and hiding the models, dimming,
  finding or making the folders, making the view, finding its folder again in the COM
  tree, adding the view, the root route's move, reading back and putting the document
  back. Set 03 timed recording as ONE watch around the folders, the COM folder and the
  add, 1602.798 s of 1B06G1's 1769.389 s, and left 67.5 s of that group and 131.9 s of
  1B06PP in none of its four parts, so which call cost the time was UNKNOWN and the speed
  work could not start. The rules above hold for it: what no part holds is named on the
  line and never spread, and parts adding to more than the whole are said in words. A
  part the work never entered is left off, so the default route never shows the root
  route's move. The line is written once per group, whichever way the work ended. The
  stretches, the rest no part holds and the words for it are
  `Federator.Core.Diagnostics.SecondsByPart`, one copy since F120, which the VIEWS line and
  the IMAGES line both wear, each with its own parts, its own words and its own whole
- EACH GROUP'S TIME STANDS BESIDE WHAT IT HELD, Q101 in F114. Bader's answer on
  2026-10-04: the 45 minutes is not judged in this round, and each group's time is
  reported beside the sizes of its NWC files and its item counts so the target can be set
  after the proof run. `TimingBlock.BesideSize` is the TIMING BESIDE SIZE block, written
  once after the last group, slowest first, and `Federator.Core.Diagnostics.GroupSize` is
  one row: the group clock, the files the group was handed and their bytes on the disk,
  the Revit elements the EXPORT CHECK counted, the clash total and the viewpoints created,
  every one a number the run already read and none a new Navisworks call. The block says
  where each number comes from. A number that could not be read is UNKNOWN and never
  zero, and a total over parts of which one could not be read is UNKNOWN too. A group
  whose clash was skipped for the shared coordinates has UNKNOWN clashes, as RESULT says,
  `GroupSize.ClashesIn`, T6-R-ZEROCLASHES, and a test that ran and found nothing is a real
  zero. It judges
  nothing and sets no target, and the line about the 45 minutes stays the last line of
  the run block, as it was

### The memory around each step, F147

- Bader's answer to Q155 on 2026-10-12, item 2 of his order: the memory at the start and the end of
  each step. `RunLog.Step` writes one MEMORY line under a step's started line and one before its
  finished line, through `RunLog.MemoryReader`, which the engine sets to `ProcessMemory.Read`, so every
  step gets it in one place and no step's code says it. Three numbers in megabytes of 1024 squared
  bytes: the private bytes and the working set as the loop's life check reads them, and the managed heap
  read without a collection, so reading it never changes it. The end line says how far each moved since
  the start, up, down or unchanged
- read where the step's own started and finished lines are written, the first visit of a step in a
  group, so TESTS RUN, entered once a test, costs two reads a group and not two a test. The start is
  read after the census before a step and the end before the census after it, so the census's reading
  is not counted to the step. A nested step's pair is inside its parent's
- a number not read is UNKNOWN and never nought, and the reason is said on the line. A read Windows
  refuses keeps the managed heap, and a reader that throws is said as the reason, so logging never
  stops a run. With no reader set, no MEMORY line is written. RunLogMemoryTests

### The document census, F61

Five counts read out of the open document, before and after a step, so a count that
moved where nothing should have moved it is named rather than discovered a week later.

- the five are models, selection sets, clash tests, clash results and saved viewpoints.
  Every one is something the NWF carries and something there is no second copy of
- A COUNT THAT COULD NOT BE TAKEN IS MINUS ONE AND NEVER ZERO. Zero reads as a real
  count and would let a run throw 1830 clash tests away and report that nothing moved.
  A census holding a minus one says UNKNOWN for that count, and a count either census
  could not take is never called a move, in either direction
- `CensusRule` says which step may move which count, and the useful half is the
  refusals. DECIDE opens the NWF that is on disk, and opening a document replaces
  everything in it, so all five may move there and only there. APPEND moves the models.
  SETS the sets, TESTS CREATE the tests, TESTS RUN the results, VIEWS the viewpoints,
  F85. Every other step writes
  a FILE and not the document, so none of them may move anything
- THE RULE HAS THREE ANSWERS AND NOT TWO, F73. `CensusRule.Judge` gives Allowed, Noted
  or Refused, and `CensusMove` is the enum. Allowed is the step doing the job it exists
  for and writes nothing. Refused is the fault. NOTED is a count a step moves while
  doing its own work, which writes a line and no reason. APPEND raising the saved
  viewpoints is the one noted pair today: an NWC exported from Revit carries that
  model's saved viewpoints and appending it brings them in. On the first real run that
  was refused, so all seven groups were reported FAILED while 28 files had been written
  correctly. `MayMove` is the narrow reading, Allowed only, and `StepsThatMayWrite` is
  built from it, so a side effect never widens what the census is taken around
- a move the rule REFUSES gets a line beginning CENSUS CHANGED, naming the step, the
  count, the before and the after, and the reason goes on the group so it is not
  reported DONE. A move it NOTES gets a line beginning CENSUS NOTED with the same
  numbers, a sentence saying why that step moves them, and no reason at all, so the
  group can still be DONE. Nothing is undone, nothing is skipped and the run carries on.
  The tool reports what it noticed and Bader decides
- THE CENSUS IS TAKEN AT MOST ONCE PER STEP PER GROUP, on the first visit, for the same
  reason the start and finish lines are written once. TESTS RUN is entered 1830 times
  and counting the whole document around every visit would be the log slowing the thing
  it is watching
- what the census COSTS is measured off the same monotonic clock and said once per
  group. Over `CensusCost.TooLongSeconds`, one second by default and a setting, the
  census narrows to the steps that may write and the next group says so at its top, so
  no reader ever wonders why a step has no census around it
- `DocumentCensusReader` in the add-in is the ONE place that reads all five. Each is
  walked and not asked for, every wrapper disposed on the way down, and each read is in
  its own try so one count failing does not turn the other four into UNKNOWN. The set
  walk `FederationEngine.CountSets` used to carry moved into it, so the rebuild and the
  census read the same number the same way
- clash results are counted as LEAVES, descending every result group, the same rule
  `ClashRunner` counts by. A result group counting as one would let a run lose every
  clash inside it and report the same number

### The gap block, F63

Bader's standing rule built into the tool: when the code knows something the report
does not show, it becomes a question. That rule was worked by hand every time, and Q25
is exactly it, found by an audit of every file under src. The run says it itself now.

- a GAP is something the run MEASURED off the model that no output carries. Not
  something the code could have measured and did not, and not something the report
  leaves out on purpose that a reader can see anyway. It has to be a number the run
  paid for and then threw away
- a gap names three things: what is missing, what it came to on this group, and where
  it would belong. A gap with no value is a complaint and one with no place to go is a
  shrug
- a property read on every item and empty on every one is SAID, with its count, `read on N
  item cells and empty on every one`, Bader's Q76 in FR-198. It was left out, so the block
  could not tell an output carrying it from the run reading nothing. A report with no item
  read nothing and has no gap
- the block is written even when it is empty, saying nothing was held back, because a
  missing block reads as a check that did not run
- nothing acts on it. A gap does not fail a group, does not stop a run and does not
  change an output. It is information in the log, which is the rule this tool is built
  on
- the run line counts gaps by NAME and not by line. The same ones are held back on
  every group, so a run of twenty two groups would report 44 and say nothing except
  how many groups there were. The number that means something is how many distinct
  things this tool knows and does not show
- the six were MEASURED on 2026-09-19 by reading all three writers. WorkbookWriter,
  HtmlTabularWriter and ClientReportColumns name none of Family, Type, Material,
  SourceFile, Discipline or IdFrom anywhere, and ClashReportXml writes two quick
  properties and says in its own comment that the five used to be written and are not
- TWO ARE LEFT SINCE FR-198. Bader deleted Family, Type Name, Material and the per item
  discipline with their harvest and their names settings on 2026-10-06, Q35, Q36, Q37 and
  Q39, so ClashItem holds none of them and the harvest reads none. The discipline read off
  part 5 of the NWC name is untouched. The source file is still read off every item, since
  `ClashItem.MergeKey` reads it for the mirror merge, F132, which came after Q38 was
  measured, and the id's property is kept as the ITEM IDS block counts it, Q40. Each is in
  no output and the block names both

### The run against the session, F80

- THE HEADLINE TIME IS THE RUN AND NOT THE SESSION. `Federator.Core.Diagnostics.RunClock`
  holds three stretches off the ONE monotonic clock: waiting for the person, the run
  itself, and whatever happened after the run finished. `RunLog.RunStarted` and
  `RunFinished` take the two marks and write the two lines together, so the marks and the
  lines can never disagree. The run of 2026-09-19 read 1424 seconds, of which 473 were a
  person reading the group table and pressing Run, and every share in the timing block was
  worked off the bigger number so every step read as a smaller part of the run than it was
- NONE OF THE THREE VANISHES. The tail between the last group finishing and the result
  block being written is small and is NAMED, for the same reason the timing block has a
  row for everything outside every step. Both numbers reach RESULT, because how long the
  run took and how long the window was open answer different questions, and criterion 2 is
  about the run
- WITH NO RUN MARK THE RUN IS THE SESSION AND THE BLOCK SAYS IT FELL BACK. The open file
  run and the two hand buttons on the Clash step never mark a run, and a block that
  divided by zero or reported a negative there would be worse than one that says what it
  did. A clock that went back gives no time rather than time owed
- THE LOG STILL OPENS ON THE FIRST LINE OF THE BUTTON HANDLER and the session clock still
  starts there, so a run that dies at startup still leaves a file. The run is two MARKS on
  that clock and never a second stopwatch and never two wall clock readings subtracted

### Trimming the .log and never the .tsv, F81

- THE TEXT LOG IS TRIMMED AND THE MACHINE READABLE LOG KEEPS EVERYTHING. That is the whole
  rule and it is the thing that would be easy to get wrong. `RunLog.NumberedRepeat` writes
  the ROW FIRST and then decides about the sentence, so collapsing a line can never cost
  the .tsv a row. One run wrote 936 ROWS lines all saying the two numbers agree, and
  collapsing them by simply not calling Numbered would have lost all 936 rows
- THE KEY IS WHAT MAKES TWO LINES THE SAME LINE and it is NEVER the sentence, because the
  sentence carries the test name and the names are exactly what varies. Five go out in
  full, then ONE line saying the rest are counted, then nothing, which is the same shape
  the skips and the drift already use. The count is carried into RESULT by
  `RunLog.CollapsedLines`, because a log that quietly wrote fewer lines than it had is a
  log nobody can check
- WHAT WAS COLLAPSED, measured on the run of 2026-09-19: 10,980 CLASH created lines, 1830
  drift lines, 854 duplicate SET lines, 1584 passed lines and 936 ROWS lines. The drift
  is GROUPED by `TestDrift.Grouped` on the field and both values, because every one of the
  1830 said the same thing about a different name. The passed tests are collapsed by
  `ClashRunOutcome.RanLines`, and a test that found CLASHES still gets its own line,
  because every one of those says something different
- THE PER GROUP SETS BLOCK IS GONE. `SetBuilder` already writes a live line per set as it
  builds, so the block was a second copy of all 61 of them. The totals it also carried are
  still said, in the one line after it and in the SETS step's own finish phrase
- RESULT SAYS WHAT BOTH FILES CAME TO. The .log number is read BEFORE the last lines are
  flushed and the line says so, because a size is only ever reported as what was actually
  read off the disk

### Sets across the run, F82

- A SET AT ZERO IN ONE GROUP AND A SET AT ZERO IN EVERY GROUP ARE DIFFERENT FINDINGS. The
  first says a discipline was not exported for that building. The second says the set
  itself is wrong, and 38 of the client's 61 were in that state on the first real run with
  nothing anywhere adding it up. `Federator.Core.Sets.SetsAcrossTheRun` is the tally and
  the block goes straight after SOURCE FINDINGS, because it is the same kind of thing:
  something the run noticed that no output carries
- IT COUNTS A SET THAT WAS ALREADY THERE AS WELL AS ONE THIS RUN CREATED. `SetResult
  .IsZero` is true only of a CREATED set that found nothing, because that is what the SETS
  totals and the NWF save decision read. On a weekly run every set is already there, so
  IsZero is false for all 61 while 38 of them found nothing, and reading it here would
  make the block empty on exactly the runs it was written for
- A SET THAT NEVER RESOLVED IS UNKNOWN AND NOT ZERO. It carries an item count of minus one
  and is left out of both counts, the same way a census minus one is never called a move.
  A set a group never looked at was not at zero there, so the tally counts groups SEEN as
  well as groups at zero
- EVERY SET THAT FOUND NOTHING IN EVERY GROUP IS NAMED, F127, Bader's request 2 under Q112,
  so a set spelled wrong or pointing at nothing shows at once. It was TEN NAMED AND THE REST
  COUNTED, a constant F82 chose for a reason its entry does not say, and set 03's C06 run
  named 10 of its 14, log lines 8346 to 8357. How many are named is the setting
  `CoverageSettings.SetsAtZeroNamedInTheRun`, nought for every one by default, and where it
  caps them the rest are counted and the block SAYS it truncated. One row per set is written
  on its own, because `Block` writes no row at all and the .tsv has to carry what the block
  carries

### Coverage of the picked tests, F127

Bader's request 2 under Q112: after each group, every test of the picked XML, or of the
tests saved in the document where none was picked, against what happened to it, with one
reason for each test that has no results. The design is
%LOCALAPPDATA%\NwcFederatorLoop\turn5\f127-design.md. Its Core steps 1 to 3 were built on
branch fix-F127 and put on main by lane B as PR 145 on 2026-10-07, with the RESULT lines of
the count check. The Coverage sheet, the sheets the workbook check allows and the RESULT
count of the tests of Q127 followed in the worktree session's pull request. THE ADD-IN HALF
WIRES THE ENGINE since branch fix-F127b of 2026-10-11, addin.md THE COVERAGE OF THE PICKED
TESTS: each group's COVERAGE block, its .tsv rows, the Coverage sheet in the internal workbook
and the run's tally in RESULT. Nothing of it has run on Navisworks yet.

- THE REASON IS THE RUNNER'S OWN RECORD, READ IN THE ORDER THE CODE APPLIES IT.
  `Federator.Core.Coverage.CoverageRule.For` takes the plan, the clash step's
  `ClashRunOutcome`, the discipline codes of the group's files and of every file of the run,
  and the settings that say which codes a set name carries, and gives one `TestCoverage` per
  test in the order of the file. A test of the picked file the plan dropped before the
  model, or one naming a set not in the document, reads the test was not created, beside the
  words of `ClashTestPlan.Describe`. With no XML picked the plan is read off the tests saved
  in the document, and a saved test it drops, a type number this tool does not run or no
  name, was read out of the document, so it reads already there and not run,
  `CoverageReason.SavedTestNotRun`, with the plan's own words, and never the test was not
  created. F77's creation plan comes next, so a test it kept out of a group
  whose clash the coordinates rule skipped keeps its side reason and never reads as a
  coordinates skip, which would hide every gap of the matrix on both wave buildings. Then
  the coordinates rule, the one discipline rule, the run time side check and the run
- THE RUNNER'S RECORD IS READ AND NEVER CHANGED. `ClashRunOutcome` hands out its created and
  already there names, the tests that ran and the skips as read only views, keeps a copy of
  the counts the creation plan decided on, `KeepItemsByLocator`, and the two counts the run
  time check read off each test's own sides, `RecordSides`. A side reason reads the run time
  counts where the check reached the test, because they decided the skip, and the creation
  plan's where it did not. ClashRunner calls the two beside its two skip sites, the first
  before `CreationPlan.For` and the second as soon as a test's sides are read, before the
  coordinates, one discipline and side checks, so all three skips read the counts that made them
- THE PLAN THE COVERAGE READS IS THE PLAN THE RUNNER RAN, `ClashRunner.RunPlan`: the picked
  file's tests resolved against the document's sets and each mirror under the name it runs
  under, or the saved tests read with their sides, so a mirror is read under the name its
  record keeps and a saved test's sides are its sets and never the placeholder. The plan it
  was handed where the runner stopped before it had one
- THE CODES A SIDE IS JUDGED BY are the group's models' codes, `CoverageRule.GroupCodesOf`,
  read off the EXPORT CHECK's models, and the run's, `CoverageRule.RunCodesOf`, the ticked groups
  of this run and the group's own, so a code the group carries is always a code of the run. The
  open file run has no scan and its run is its one group. A group the scan found and the run did
  not tick is not among them, so a code only it carries reads UNKNOWN and never not in the
  group, row F127-R3, dropped on 2026-10-12 under Q143 since UNKNOWN is never a wrong word
- A TEST THE RUN LEFT NO RECORD OF, OR NEVER REACHED, IS NOT COMPARED, and one the run knows it
  did not create, F77 kept it out or the plan dropped it before the model, is held by neither
  side. A test the plan dropped before the model that Clash Detective holds with results names
  an earlier run as the cause, and the same is not said of a test F77 kept out, since the
  runner walks the document first and finds none of that name. The tests the file does not
  name are counted once by name across the run. The headline counts test places, a test once
  for each group it is in. A set is counted once per group and is not at zero where one set of
  its path was not counted, and the F82 section names a set never counted on its own line
- PRESENCE IS SAID APART FROM THE REASON, `TestPresence`: created this run, already there,
  not created by F77, or UNKNOWN where the runner never looked. The words for both are
  `CoverageWords`, and no reason claims a test was created, because a test already in the
  NWF is skipped for the same reasons as one created a minute ago
- A SIDE THAT FOUND NOTHING IS JUDGED BY ITS SET'S CODE, read off the set's own name through
  `DisciplinePairRule.CodeIn` until F131 re-points it to the team map. A code no file of the
  group carries and a file of the run does reads the discipline is not in the group. A code
  a file of the group carries reads a side's set found no items in this group. A code no
  file of the run carries, FF, PL and DR on this project, a name carrying no code,
  BLD-Security Devices, and a run whose codes were not handed in or a group whose files gave
  no code, an empty set read as not read, each say the set found no items and that whether
  its discipline is in the group is UNKNOWN, and never the definite the discipline is not in
  the group. Where both sides found
  nothing the stronger is given in that order and both sets are named. On the exchange file
  with the counts of group 100000 of set 03's C06 run that is 36 tests both find, then 969,
  561, 255 and 9, held by a test
- A COUNT OF MINUS ONE IS NEVER A SET THAT FOUND NOTHING. It reads a side was not counted,
  UNKNOWN, and a side that did find nothing beside it still gives its reason, because a
  definite reason is given wherever one exists
- TWO TESTS OF ONE NAME READ UNKNOWN, because the runner keys its record on the name and a
  guess would hand one test the other's result. That holds for a test sharing its name with
  one the plan dropped too, since the runner adds the plan's skips to its record first. The
  dropped test keeps the plan's own reason, which the plan keys on its place in the file. A
  group with no clash step reads UNKNOWN, but for the tests the plan dropped itself
- EVERY KIND THE RUNNER RECORDS MAPS TO A REASON, `CoverageRule.ReasonFor`, and a test
  enumerates `ClashSkipReason`, so F132's Mirror fails it until it is mapped rather than
  reading UNKNOWN on every test it skips
- A test that ran reads has clashes or ran and found no clashes off the runner's own count.
  The design's row for a test that ran with its count UNKNOWN is not built, because the
  runner records a test as ran only once it has counted it and records it Failed otherwise
- A CLASH COUNT IN CLASH DETECTIVE THAT DIFFERS FROM THE WORKBOOK ROWS IS A FAILED LINE, his
  words, and it is F104's checks 1 and 2 restated in Core, not copied, `CountCheck.Judge`:
  the clash rows under a test's block against the document's results at the top level, a
  result group counting as one, and the Clashes cell against every clash in the test, the
  clashes inside each group counted. Both exact. Two differences from compare-document.ps1:
  its check 2 names an empty result group, which the harvest counts as one clash, and this
  has no such words, and a test in the workbook only whose every number is nought is counted
  and not judged there, where here it is a test F77 did not create and is held by neither
  side, counted apart from agree since PR 145. Agree only where both equal AND THE TEST RAN THIS
  RUN, the breaker's third finding on F127b: a test that did not run with equal counts, nought
  and nought most often, is NOT COMPARED, `CountCheck.DidNotRunWhy`. SINCE F153 A TEST WITH NO
  BLOCK READS NOUGHT ROWS AND CLASHES NOUGHT, since a test with no clash writes nothing in the
  workbook, so a test that ran and found none agrees with a document holding none, and its
  Coverage sheet cells read 0 and not UNKNOWN. Failed for any other difference, for a test the
  document holds clashes of with no block, and for a block with numbers that the document does
  not hold. NOT COMPARED, NEVER AGREE, where a count on
  either side is minus one, the workbook's included, so a block that could not be read whole
  is never a FAILED line, a name is on two tests of one side, a test
  the run holds as created or already there was not returned by the read of the document, or
  the workbook or the document's counts were not read, with the reason. The tests the
  document holds and the file does not are named and not judged. compare-document.ps1 stays
  the independent witness, because this reads the document through the add-in as the harvest
  does, and a check sharing the code it checks cannot catch a fault common to both
- WHERE THE RUN KNOWS WHY A COUNT DIFFERS, THE LINE SAYS IT, AND ONLY WHAT THE RECORD PROVES.
  The runner records a test as Failed for a throw anywhere from its creation to its count,
  its harvest included, and never as ran, so a Failed test is never said to be not run: one
  created this run that Clash Detective holds results for ran this run and then threw before
  its rows or count were taken, and any other says whether it ran is UNKNOWN, each with what
  it threw. A test already in the document and not run this run still holds an earlier
  run's results, the design's decision 4 at its default A, and only such a test is given
  that cause. Compact is named as something that could account for a gap, never its cause,
  where its count of Resolved clashes removed across the group is at least the test's gap,
  and where it is fewer the line says so
- THE DOCUMENT'S SIDE IS `DocumentTestCount`, plain numbers the add-in fills from one walk of
  the tests after the clash step, so they are what the NWF holds, nothing compacting since F185, and include
  the tests a weekly run did not run. A count not taken is minus one and never zero.
  Statuses are not read, because they are not judged, PQ4 of F104 being unmeasured
- THE TESTS OF A MIRRORED PAIR WHOSE CLASHES F132 MERGED ARE NOT COMPARED, `CountCheck.InAMergeWhy`,
  the kept test and each mirror by its run name, `MirrorMerge.MergedNames` read through
  `ClashRunner.InAMerge`, because a merge carries a mirror's clashes in the kept test's block of
  the clash workbook and leaves none of the mirror's, so neither block is one test's count. READ
  OFF WHAT AddTo DID AND NEVER OFF THE PLAN, the breaker's first finding on F127b: a pair the
  rule planned and AddTo refused at run time keeps both blocks and names no test, so it is
  compared like any other test and a real difference is a FAILED line, which a test breaks. A
  merge never makes a COVERAGE FAILED line
- A TEST A RULE LEFT OUT THAT CLASH DETECTIVE STILL HOLDS IS NOT RUN BY THE RULE AND NEVER FAILED, F225, row F219-R1 with
  Bader's answer to Q209, Q212 open. A test the rule of one discipline, Q203, or of two set codes, Q209, left out of this
  run, read already there, or of a presence UNKNOWN where the rule of two set codes left it out of the plan, that the
  document holds and this run's workbook has no block for, is an NWF's test saved before the rule with an earlier run's
  results, kept and never deleted. `CountCheck` reads it NOT COMPARED with its rule's words and what Clash Detective still
  holds for it, and marks it `CountedTest.KeptFromAnEarlierRun`, so a COVERAGE FAILED line never names it. One the document
  does not hold, left out of a plan before any test was looked for, is held by neither side by the rule the run did not
  create it. One created this run, one of one discipline with no record of where it is, one the run walked the document for
  and did not find, and one a block of this run's workbook names, which the writer never makes, are judged as before. The
  COVERAGE block of each group writes one line after its count line, `CountCheck.LeftOutByARuleLine`, how many tests a rule
  left out, counted by what the check read of each: still held from an earlier run and with how many clashes, held by
  neither side, FAILED, and the rest whose holding is UNKNOWN, a side or a count not read or a name on two tests, never a
  none the check did not read, the reviewer's and the breaker's finding, then that whether those held are removed is
  Bader's, Q212. None where no rule left a test out. Measured on the NWF page on 18 Oct over NWFs built before F224, its
  first pass: 1B06BS held 28 with no result, 1B06M1 14 with 22 clashes and 1B06PE 32 with 16, 0 FAILED. Rows F225-R1 to R3 hold what is left
- THE CLIENT'S SHEET IS READ BACK BY ITS NAME, `WorkbookTests.ReadFile`, the name the writer
  gives it, `SheetNames.ForReport` of the report's output name, and never by its place, the
  readers' point that a sheet put before it would be read in its stead. No file or no sheet of
  that name exactly is a reason and never a throw. AND ONLY WHERE THIS RUN WROTE IT WHOLE, the
  readers' point on the stamp: the engine hands the count check no workbook where the write
  threw, since last week's file at the same path would be read as this week's, and the reason
  says so
- THE CLIENT'S SHEET IS READ BACK TEST BY TEST, `Federator.Core.Report.WorkbookTests.Read`, by
  the name in column A, one shape since F153: a full block gives the clash rows under it,
  counted the way WorkbookCheck counts them, and its Clashes cell. The one row Q73 gave a test
  that found nothing is never read as a test, the workbook check names it. It takes a sheet of a workbook
  already open, so the file is loaded once. A name on two tests of the sheet, a test row with
  no name, a Clashes cell that is no whole number and a block with no clash table under it
  are each a doubt, the last two read as minus one. Names are read exactly and never trimmed
- THE FAILED LINE STARTS COVERAGE FAILED, `CountCheck.FailedPrefix`, so it is never read as a
  group's FAILED, names the group and the test, gives all four numbers or says which side
  holds no test of that name, adds the reason where known, is one line whatever an exception
  message held, and ends saying the group keeps its own result. THE GROUP KEEPS IT BY
  CONSTRUCTION: GroupFacts has no coverage member and nothing in the coverage adds an error,
  which is his answer to the notes under Q112 and the rule that a report check never fails a
  group
- RESULT CARRIES EVERY FAILED LINE, his answer, a FAILED line in COVERAGE and RESULT.
  `CoverageAcrossTheRun` rolls in each group's check, and `RunLog.WriteResultBlock` takes it
  beside the coordinates tally, handed in by the run and never kept on the log. It writes
  `COVERAGE checked :` with how many were compared, agreed, FAILED in how many groups, not
  compared and held by neither side, a label that is not the FAILED prefix, how many tests
  Clash Detective holds that the picked file does not name, then every FAILED line in full.
  Nothing compared reads UNKNOWN and never nought FAILED, because both wave buildings skip
  their clash under F112 and compare nothing. A group whose coverage could not be taken, or
  whose clash step made no plan of tests, is counted not checked. EVERY GROUP THE RUN TOOK IS
  COUNTED, AND ONCE, `CoverageAcrossTheRun.For` and `GroupInTheRun`, the breaker's second and
  fifth findings on F127b: a group that ended before its COVERAGE step, refused, a rebuild
  that failed, a throw or no document, is counted not checked when it ends, and the first word
  on a group is the one counted, so a throw after its check was counted never counts it twice. Where no group failed and no error was logged but a count differs,
  RESULT closes on a line saying so and never on Nothing failed, and the group's count,
  facts and judgement do not move. A run that hands in no coverage writes that none was
  taken, because a missing line reads as a check that did not run
- RESULT COUNTS THE TESTS OF THE PICKED FILE ONCE ACROSS THE RUN, Bader's answer A to Q127,
  `CoverageAcrossTheRun.AddTests` and `TestLines`, FR-176: how many names, how many were in
  the document in one group at least and never in the document, run in one group at least,
  with clashes in one group at least, run and never with a clash, then the same counts added
  up over every group, places, created this run, already there, not created, run, with
  clashes, without, and in the document and not run. The lines come before the check's in
  RESULT and none is written where no group's tests were handed in, since RESULT then says
  no coverage was taken. Counted apart from the check, so a group whose check could not be
  made still counts its tests
- EVERY NUMBER THAT SHAPES THE COVERAGE IS A SETTING, `CoverageSettings`: the sheet's name,
  Coverage by default and refused where Excel would refuse it, by `SheetNames`' own rule, the
  examples named per reason, five off `RunLog.KeptOfARepeat`, the categories no set catches
  named per model in the log, ten by the design's choice and measured by nothing, and the
  FAILED lines in RESULT and the sets named across the run, nought for every one. A count of
  lines below one, or a cap below nought, is refused where it is set. The width of each
  column of the sheet is `CoverageSheet.Widths`, chosen so a heading and a locator read and
  measured by nothing, because no report of the client's has these columns
- THE PICKED XML IS FINGERPRINTED, `Federator.Core.Diagnostics.FileFingerprint.Sha256`, 64
  lower case hexadecimal characters, so a run says which bytes it read. Set 03's C06 run
  carried none, and its XML had to be told from the exchange file by its SET lines asking
  ME-PIPING where the exchange file asks ME-Piping. A file not there throws naming its path
  and nothing is caught, because a fingerprint that could not be read is not one. The add-in
  reads it ONCE when the run starts, one COVERAGE line, so every group names the same bytes.
  It is still a second read beside the window's parse at the press, so a file changed between
  the two would be named by bytes the plan did not parse, a known gap the add-in half narrowed
  to the seconds between the press and the run and did not close
- THE MIRRORED PAIRS OF Q114 POINT 5 ARE F132'S `MirrorRule.Pairs`, on main since F132, whose
  MIRROR and MIRROR COVERAGE blocks the clash step writes. F127 builds no second copy, and the
  sheet's MIRRORED PAIRS section says UNKNOWN on the sheet and names those blocks,
  `CoverageSheet.MirrorsUnknown`, never that the rule is missing
- THE COVERAGE SHEET IS A SHEET OF THE INTERNAL WORKBOOK, Bader's answer B to Q146 of 11 Oct
  2026, `Federator.Core.Report.CoverageSheet` written by `InternalWorkbook.Write` after the
  Generic Models sheet in one save, beside the clash workbook, which goes to the client and
  keeps the Navisworks export's layout, the client's sheet alone. It was the second and last
  sheet of the clash workbook by FR-200, Bader's request 2 under Q112 and his decision under
  Q46, until his answer moved it. A test writes both workbooks of one group and reads the
  clash workbook as one sheet and the internal one as both. ONE ROW
  PER TEST OF THE PICKED FILE, IN THE FILE'S ORDER, A TEST NOT CREATED INCLUDED, with its
  position, its name, its priority where the priorities were read, both sets and their items, its
  presence, whether it ran, the count check's four numbers and verdict where the check holds
  one test of that name, its reason and its detail, `CoverageSheet.TestRows`, the ONE SHAPE
  the writer and any check of the sheet read. AN UNKNOWN VALUE IS THE WORD UNKNOWN, never an
  empty cell and never nought. The head names the picked file, its sha256 and its list of
  corrections, or the tests saved in the document, the models with their codes and elements,
  and the counts read off the rows alone, `CoverageSheet.Counts`. The SETS section is the
  sets step's own results, each set with its conditions, its items in this group and whether
  it is at zero, or UNKNOWN where none was handed in. The items no set catches and the
  mirrored pairs read UNKNOWN until their rules are on main, and the tests Clash Detective
  holds that the file does not name are listed off the count check. Every group with a
  coverage gets a sheet, a group whose clash is skipped included, Q126 answered B, since its
  internal workbook is written whether or not the clash ran
- THE INTERNAL WORKBOOK WRITES WHAT THE GROUP HAS, `InternalWorkbook.Write`: the Generic Models
  sheet and the Coverage sheet, the one alone where the other was not taken, and a group with
  neither is refused so the engine writes none and says so. Where the two names are one to
  Excel the Generic Models sheet is left out by `GenericSheet.WhyRefused`, the one rule for it,
  `InternalWorkbook.WhyNoGenericSheet` handing it the Coverage sheet's name. Read back by its
  sheets' names, `InternalWorkbook.Check`, one line where it holds what was written and a line
  for each sheet missing or not written, never a throw
- THE WORKBOOK CHECK EXPECTS THE CLIENT'S SHEET ALONE, AS IT DID BEFORE FR-200, Q146 answered B:
  a second sheet of any name, the Coverage sheet included, is a problem line naming the sheets,
  a warning that never fails a group. Only sheet 1 is compared with the client's layout
- THE LOOP'S read-workbook.ps1 READ EVERY SHEET AS A TEST SHEET, the design's section 3, so a
  Coverage sheet in the clash workbook would have made every F104 verdict NOT PROVED, F127-R1.
  With the Coverage sheet in the internal workbook the clash workbook is one sheet again and
  F127-R1 has nothing left to change. The loop's run.ps1 reads every workbook of the run with
  that script, so the internal workbook's read-out carries doubts of its own, and that
  read-out is never paired with an NWF, since the add-in writes it under its own kind,
  INTERNAL, and not XLSX

### Long steps, F143

- THE HARVEST AND THE VIEWS SAY WHERE THEY ARE AT LEAST EVERY 30 S BETWEEN TWO PIECES, Bader's order of
  2026-10-11 headed 1A04PK IS FROZEN, a piece being a row of a test, an item under a group row of a test
  in a merge, a picture of the picture pass, one count across the whole pass since F149, or a view,
  so a single piece longer than that is
  silent for its length and its call is named instead. `StepProgress` is the one rule of when a progress
  line is due, the setting's seconds since the step
  began or the last line on the log's own clock, or every piece at nought or below, and of its words,
  PROGRESS, the step, what it is on, the piece and its count of the total, UNKNOWN where the total is
  below nought, and its seconds into the step. `ViewsProgress`, FR-071, reads its rule, so the two cannot
  drift apart
- TESTSRUNTEST AND TESTSIMAGEFORRESULT ARE NAMED WHERE ONE CALL TOOK LONG, `LongCall.Line`: CALL, the
  call, what it was on and its seconds, the call alone timed, with the words that the window could not
  answer for that long, and null under the setting or for a length not known. `LongCall.Say` writes it
  with its .tsv row through RunLog.Numbered, the seconds as its number, and nothing with no log
- THE NUMBERS ARE `ProgressSettings`: EverySeconds and LongCallSeconds, 30 each by his words. A long call
  of nought or below is refused where it is set. Since F227 it also holds CountEveryTests, the clash step's
  running count, 25, refused below one. The VIEWS step's default,
  `ViewpointSettings.DefaultProgressEverySeconds`, is `ProgressSettings.DefaultEverySeconds`, 60 s until
  F143

### Generic models, F128

Bader's request 3 under Q112 and FR-177, taken first by his order of 8 Oct 2026, Q145 item 2.
Lane B's Core part is pull request 171, the add-in part follows the one measurement allowed,
docs\history\scan.md 5z-zb of 2026-10-08, and nothing of it has run on Navisworks yet.

- THE SETS ARE PLANNED SETS, `GenericModelsPlan.For`, one search set for each model of the
  group named after its NWC, in the folder `GenericModelsSettings.FolderName`, asking the
  Element tab's Category equals the value and the Source File contains the model's text, in
  one group so they are ANDed, the words the client's own file writes and a test holds them to
  it. The value, the folder, the sheet and the workbook suffix are `GenericModelsSettings`, each
  refused where it is set if it cannot work, and none is typed in the window or the XAML. The
  value and the tab were measured right on 1A04PK, 572 items in five of ten models on the
  Element tab's Category, and on 1A02MM no item of 19,028 carries the value on any tab
- THE TEXT EACH SET LOOKS FOR IS THE FILE NAME OF Model.SourceFileName, `GenericModelInput.From`,
  the bare name after the last slash or backslash with its .rvt, measured on 2026-10-08: the
  Source File of an item is the name of the Revit file the NWC was published from, and for four
  of 1A04PK's ten models that is not the NWC's, so the NWC's stem found 531 of 572 and the
  Revit file's name all 572. A model whose source name is blank or ends in a separator is
  looked for by the stem of its NWC's name, `GenericModelsPlan.LookingForTheStem`, AND THE PLAN
  SAYS SO, a note naming each such model and the one SETS line, `LooksFor`, saying which text
  the sets look for, because in silence a model published under another Revit name counts
  nought, the readers' finding on attempt 1. The set is still named after the NWC
- A COUNT NOBODY TOOK IS UNKNOWN AND NEVER NOUGHT, `GenericModelsReport.From`: a set not built,
  a count not taken, two results of one path, and a present set whose question was not read
  or asks other than the plan's are each a model not counted with why. A PRESENT SET IS
  COMPARED WITH THE PLAN BY THE ONE DRIFT RULE, `SetDrift.Compare` on the keys of the
  conditions read off it, `SetResult.AskedConditions`, and never the two prose strings, because
  the question read off a set carries no display names and the plan's carries them, so the
  words called every present set on a weekly rerun a set asking another question and every
  model was UNKNOWN after the first run, the readers' finding on attempt 1. The total says at
  least where any model was not counted, and where the texts of some sets meet it says it is
  not a count of items, because an item two sets find is in both counts. The count is of
  items, the elements the set finds, and the block says items
- A MODEL AT NOUGHT IS LEFT OUT OF THE LIST, counted in the line above it and followed by a
  line saying what a nought can mean, and where every model is at nought a second line says
  that is also the count a wrong value or a wrong text gives, which on 1A02MM is the true
  reading by measurement
- NO JUDGE, NO JUDGEMENT, `SetBuildOutcome.JudgeIfEmpty` handed a null judge, so the Generic
  Models sets, built with none, are in no EMPTY list and never read as a set nobody should
  have left empty. They have a SetBuildOutcome of their own and SETS ACROSS THE RUN never
  reads it
- THE SETS OF THE OTHER PLAN ARE WANTED BY THE LEFTOVER WALK, `GenericModelsPlan.SetNames`,
  handed to the picked file's build, because `SetLeftovers.For` treats every set the names it
  is given do not cover as a leftover and a Generic Models set of the last run has no sides.
  THE WALK RUNS ONLY WHERE THE GENERIC MODELS PLAN WAS MADE AND HOLDS A MODEL,
  `GenericModelsPlan.WhyNoLeftoverWalk`, because a plan that was not made, a model's read
  threw, or that holds no set, the document read no models, would hand the walk no name and
  with the rebuild box on last week's set of every model would be removed and the NWF saved,
  the readers' finding on attempt 1. Where it is refused one line says why and no set of
  either plan is removed. A set of a model that has left the group is stale and the walk
  removes it with the box on, which is right. The Generic Models build walks no leftovers at
  all, since a walk handed its plan would remove every set of the picked file that no test
  points at
- THE SHEET IS ONE SHAPE, `GenericSheet.Rows`, models with items first in the plan's order, then
  the ones not counted with why, none for a model at nought, named by
  `GenericModelsSettings.SheetName`, and it is the first sheet of THE INTERNAL WORKBOOK,
  `InternalWorkbook.Write`, beside the clash workbook in the Log folder, named by
  `GenericModelsSettings.WorkbookNameFor`, the group's workbook name, a space and the suffix,
  Internal by default, Bader's answer B to Q146 of 11 Oct 2026, which also put the Coverage
  sheet after it. The suffix was Generic Models while the workbook held that sheet alone, and a
  name never says Generic Models where the workbook holds both. Written whether or not the
  clash step ran, since the count does not depend on it, and read back by size and by its
  sheets' names
- ONE LINE ACROSS THE RUN, `GenericModelsAcrossTheRun.ResultLine`, `generic models :` in RESULT
  under the coverage lines, written by `RunLog.WriteResultBlock` off the tally the window hands
  it and never kept: the items, the models with items of the models planned over the groups,
  the models at nought and the ones not counted, at least where any was not counted, not a
  count of items where the texts met in any group, and the groups that reached no count named.
  A run that hands none in writes that no count was handed in, because a missing line reads as
  a count that did not run
- THE INTERNAL STEP, `RunSteps.InternalWorkbook`, after COVERAGE, times the internal workbook,
  and the plan, the build and the count are inside the SETS step's own seconds, so the timed
  runs show what each model's search over the whole document costs. It was GENERIC XLSX, and
  it is eight letters because a name longer than TESTS CREATE widens every STEP line of the log.
  THE COVERAGE STEP, `RunSteps.Coverage`, after the clash workbook, its page and its XML, times
  the walk of Clash Detective's tests, the read back and the block
- A THROW IN THE PLAN, THE BUILD OR THE WORKBOOK WRITE NEVER FAILS THE GROUP, the lead's
  decision on attempt 2, as F127's coverage does and as a report check never fails a group:
  it is a FAILED line in the GENERIC MODELS block, `GenericModelsReport.FailedLines`, saying
  what threw and that the count is UNKNOWN and the group keeps its own result, the group is
  counted as not counted in RESULT, and the failure is logged with what happens next
- No clash test is made for these sets, and the block says so

### The machine readable log, F64

A second file beside the text log, same name and a different extension, `.tsv`. One row
per event, eight columns, tab separated, with a header.

- the text log is STILL the one a person reads and nothing about it got worse for this
  file existing. Where the two disagree the text log is right, because it is the one
  that has been read against a real run
- ONE WRITER, so the two cannot drift. `RunLog.Numbered` writes the text line and the
  row together and is the only way a line carrying a number reaches the log. A line
  cannot be added without its row and a row cannot say something the text log does not
- a sentence with no number in it writes NO row. That is the rule and not an oversight:
  a row whose number column is empty is noise in a file whose whole purpose is numbers
- a tolerance in the number column keeps the ten decimals the clash file gives,
  `EventRow.Tolerance`, FR-047, the one formatter of a tolerance as a number, which the
  plan, the drift and the created line read too. `EventRow.Exact` is for sizes and
  seconds, three decimals, and made 0.0820209974 ft read 0.082
- tab separated and not comma, because a clash name, a set locator and a file path all
  hold commas and none of them holds a tab, and Excel opens a tab separated file with no
  import dialog
- a tab, a newline, a carriage return or a backslash inside a value is escaped. The
  BACKSLASH GOES FIRST, or reading a value back would turn a path ending in a t into a
  tab, and every Windows path in this tool holds backslashes. Escaping is proved by
  reading the value back and not by asserting that it changed
- line by line and flushed with `Flush(true)`, exactly like the text log, so a run that
  dies mid group leaves both files whole up to that moment
- it never stops a run. A row file that cannot be opened leaves the text log untouched
  and says why in it, and a write that throws is swallowed. The second log is a
  convenience and the first one is the record

Two things that look like mistakes and are not:

- while a run holds the log open, File.ReadAllText fails with a sharing error.
  Anything reading a live log opens it share-aware. Notepad and the Copy log
  button both work. There is a test pinning this
- the header is two blocks on purpose. SESSION is written at button press and
  proves the button fired even if the scan never ran. RUN SETTINGS is written at
  Run because the folders and the counts do not exist until the scan finishes.
  Do not merge them
