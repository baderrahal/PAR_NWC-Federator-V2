# The work tracker

Made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never by hand. Status lives in steps\tracker.csv only.

- By status, of 618 rows: open 161, in progress 24, in review 0, merged 383, proven by a run 29, waiting for Bader 3, dropped 18
- By wave: 1 19, 2a 73, 2a and 2b 1, 2b 42, 2b and before any test run 1, 2c 12, 3a 22, 3b 21, 4 58, 5 73, 5 and 4 1, all 1, before any probe or run starts again 4, before any test run 3, before the test of wave 1 9, before the waves 115, beside the waves 7, first of all since Bader's order of 2026-10-05 1, none 147, outside the waves 2, right after F133 merges 6
- In progress now: F118 workbook and report, F121 the rest, F123 docs and words, and the noise of every area, F134 the code health gate, F137 no site and no clash groups, Q94 code runs every real file itself, full runs of main and the fixes they show, Q98 the full fix round, Q129 a clean tracker and a code health gate (FR-191 to FR-193), Q135 too many autosave copies, F137-R2 the reference model can itself name no site and ModelsRead counts placements not the document's models so a model whose read threw is never judged, Q145 the new order on the laptop, F132 then F128, F120, F114, the timed runs, F129, F130, F109 and the release, Q147 two small fixes, then carry on, the attribution off, item 5's steps in the repo, F114 then F129, F130 and F109, F109-R1 how the 27 people get a built bundle is UNKNOWN, CLAUDE.md saying they have no compiler while INSTALL.md builds with dotnet, answered by Bader's Q149 item 2, the team package F140, Q149-2 the team package prepared, a zip with a one click install, version, commit and date, the previous add-in kept to put back, a plain page, no zip until Bader asks, Q152 Q150's choice as F145, long runs kept from slowing, the 50 test cut, the GPU read, GenerateImage measured, one full 1A04PK only on his word, Q153 keep fixing the time until it is at its minimum, the four targets, the SPEED table, the 150 times gap first, F145 merged, and 8 FR items
- Waits for Bader, 3 rows: Q169, Q170, Q171

## Wave 1

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-001 | far model group stays done | F112 | silent wrong number | merged | 106 | none | 2026-10-05 |
| FR-002 | model site read failure reads as no site | F112 | silent wrong number | merged | 106 | none | 2026-10-05 |
| FR-003 | export check says every element without counting | F112 | silent wrong number | merged | 106 | none | 2026-10-05 |
| FR-004 | export check id share rounded to 100 | F112 | silent wrong number | merged | 106 | none | 2026-10-05 |
| FR-005 | export check worksets none when not counted | F112 | silent wrong number | merged | 106 | none | 2026-10-05 |
| FR-006 | failed by site name while placement agrees | F112 | loud failure | merged | 106 | none | 2026-10-05 |
| FR-008 | matrix workset spelling differs between c06 buildings | F116 | silent wrong number | merged | 98 | none | 2026-10-05 |
| FR-009 | matrix ar sets match items of other disciplines | F116 | silent wrong number | merged | 98 | none | 2026-10-05 |
| FR-025 | matrix or row splits set without category condition | F116 | silent wrong number | merged | 98 | none | 2026-10-05 |
| FR-026 | matrix category rewrite renames the set itself | F116 | silent wrong number | merged | 98 | none | 2026-10-05 |
| FR-028 | workset case warning printed in every group | F112 | noise | merged | 106 | none | 2026-10-05 |
| FR-030 | matrix corrections has no caller | F116 | noise | merged | 98 | none | 2026-10-05 |
| FR-031 | clash progress line one test short | F113 | silent wrong number | merged | 95 | none | 2026-10-04 |
| FR-032 | empty result group counted as one clash | F113 | silent wrong number | merged | 95 | none | 2026-10-04 |
| FR-033 | bydesign unnamed clash counted as moved | F113 | silent wrong number | merged | 95 | none | 2026-10-04 |
| FR-034 | auto review record in throws on edited comment | F113 | loud failure | merged | 95 | none | 2026-10-04 |
| F112 | alignment | F112 | fix | merged | 106 | none | 2026-10-05 |
| F113 | clash counts | F113 | fix | merged | 95 | none | 2026-10-04 |
| F116 | the clash XML | F116 | fix | merged | 98 | none | 2026-10-05 |

## Wave 2a

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-010 | empty sets contains judged as equals | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-011 | empty sets names and lists typed in core | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-012 | revit worksets list unread looks empty | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-013 | set sides read failure returns empty so sets removed | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-014 | set leftover walk skips test folders and second source | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-015 | set drift key ignores negation group and comparison | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-016 | set drift lines describe or set as and chain | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-017 | set value unreadable kind read as empty string | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-018 | rebuilt set not found again counted as zero items | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-019 | set parent handle held across replacewithcopy | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-020 | rebuilt sets do not ask for nwf save | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-021 | set lines claim every set matches when unread | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-022 | sets summary counts created sets only | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-023 | set warnings negated category reported as asked | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-024 | identical sets signature ignores flags | F115 | silent wrong number | merged | 142 | none | 2026-10-07 |
| FR-027 | empty sets block never written on a first run | F115 | broken feature | merged | 142 | none | 2026-10-07 |
| FR-176 | coverage of the clash xml | F127 | Bader's request | merged | 145 the Core steps 1 to 3, 153 the Coverage sheet, 167 the readers' Core points, 176 the counted words and 195 the add-in half with the sheet in the internal workbook | none | 2026-10-11 |
| FR-182 | mirrored tests kept once | F132 | Bader's decision | merged | 177 | none | 2026-10-08 |
| FR-183 | mirrored tests in an existing nwf | F132 | Bader's decision | merged | 177 | none | 2026-10-08 |
| F115 | sets | F115 | fix | merged | 142 | none | 2026-10-07 |
| F127 | coverage of the clash XML | F127 | fix | merged | 145 the Core steps 1 to 3, 153 the Coverage sheet, 167 the readers' Core points, 176 the counted words and 195 the add-in half with the sheet in the internal workbook | none | 2026-10-11 |
| F132 | mirrored tests | F132 | fix | merged | 177 | none | 2026-10-08 |
| Q112-2 | coverage of the clash XML (FR-176) | F127 | Bader's request | merged | 145 the Core steps 1 to 3, 153 the Coverage sheet, 167 the readers' Core points, 176 the counted words and 195 the add-in half with the sheet in the internal workbook | none | 2026-10-11 |
| Q46 | F77 against the single discipline rule | F127 | question | merged | 131 | none | 2026-10-06 |
| Q121 | telecom fixtures and telephone devices | F132 | question | merged | 118 | none | 2026-10-05 |
| Q122 | whose status a result carries | F132 | question | merged | 118 | none | 2026-10-05 |
| Q126 | no workbook for the coverage sheet when the clash is skipped | F127 | question | merged | 118 | none | 2026-10-05 |
| Q127 | how RESULT counts the tests of the XML | F127 | question | merged | 118 | none | 2026-10-05 |
| Q133 | a mirrored test can find more than the one it mirrors | F132 | question | merged | 121 | none | 2026-10-05 |
| FR-200 | coverage sheet names every test not created | F127 | Bader's decision | merged | 153 | none | 2026-10-07 |
| Q140 | F132's next attempt only for a fault that can change a clash count or its test | F132 | Bader's request | merged | 177 | none | 2026-10-08 |
| F115-R1 | side.Selection and every SelectionSource of the side walk are never disposed | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R2 | a rebuild is recorded only after the re-read so a throw after ReplaceWithCopy reports FAILED and asks for no save | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R3 | two sets of one name in one folder have their drift read off one and the other replaced | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R4 | a picked XML of sets alone or the Build sets button writes no EMPTY SETS block and no tsv row | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R5 | the category list of ten federations is handed to the judge as complete, dropped since the verdict already reads no model measured so far in this project carries it or the project has not been measured that far, F115 attempt 2, and no model read gives a category | F115 | register row | dropped | UNKNOWN | none | 2026-10-12 |
| F115-R6 | a present set at zero whose search would not read is in the at zero count but not judged and no line says so | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R7 | the sets summary carries no drifted or rebuilt or could not read count | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R8 | a clash side whose source is a folder of sets credits the folder and not the sets in it | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R9 | the judge reads the model names through ContainerName.Parse outside any guard so bad naming settings fail the group | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R10 | the judge branch for the workset list inside Core not read has no test | F115 | register row | merged | 172 | none | 2026-10-08 |
| F115-R11 | the Build sets button judges every set at zero CANNOT TELL, dropped since the button reads no model by design and says it cannot tell and why, which is true | F115 | register row | dropped | UNKNOWN | none | 2026-10-12 |
| F115-R12 | whether SearchCondition.Options reads back StartGroup 64 is UNKNOWN until the wave 2a weekly run | F115 | register row | open | none | none | 2026-10-05 |
| F115-R14 | the judge calls a value carried by no model where a model's walk stopped or a model was dropped from the exports | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R15 | a damaged NWF is still saved after a leftover rename fails and the Build sets button says nothing of it | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R16 | a set the plan skipped is read as one the file does not name and can be removed with the box on | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R17 | the HEALTH block's category check reads the 374 categories of one project with no project guard, dropped since the HEALTH line names the folder its list was measured on since PR 151 and the pick reads no model to guard by | F115 | register row | dropped | UNKNOWN | none | 2026-10-12 |
| F115-R18 | the sets lines blame the box where a rebuild failed and name lines the window does not show | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R19 | a rebuilt set is not read against the file after the rebuild so one that did not take is counted rebuilt every run | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R20 | leftovers are neither decided nor reported with the rebuild box off | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R21 | a side whose sources read as none counts as zero sides and never as not counted | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R22 | a set created and then lost track of is reported FAILED and may not ask for the save | F115 | register row | merged | 212 | none | 2026-10-13 |
| F115-R23 | two sets of one name in the picked file are built as one set and counted as one created and one already there | F115 | register row | merged | 212 | none | 2026-10-13 |
| F132-R4 | the views read the document and not the merged report, so a clash only the mirror found gets no viewpoint and a status restated on the kept row is not the one the views read | F132 | silent wrong number | merged | 177 | none | 2026-10-08 |
| F132-R5 | a run with no XML now hands the by design pass real set names, so with the box ticked it writes Reviewed into saved tests and saves the NWF | F132 | silent wrong number | merged | 177, the pass kept and said, Q144 for Bader | none | 2026-10-08 |
| F127-R1 | read-workbook.ps1 reads every sheet as a test sheet so the Coverage sheet makes every F104 verdict NOT PROVED until it reads a sheet named Coverage into a section of its own | F127 | register row | merged | 195, the Coverage sheet out of the clash workbook so the clash workbook is one sheet again and nothing is left to read | none | 2026-10-11 |
| F132-R1 | under Q142 A only a first run over a new NWF merges, a weekly run never does, dropped on 2026-10-12 since it is Bader's answer A to Q142 and every such pair is said on a MIRROR line | F132 | register row | dropped | none | none | 2026-10-12 |
| F132-R2 | a set's flags and findspec are not read by the drift check a merge reads, dropped on 2026-10-12 since attempt 13 merges only on sets this run built, so no merge reads the drift check | F132 | register row | dropped | none | none | 2026-10-12 |
| F132-R3 | two clashes of one test between the same two elements on geometry of one name share a merge key, ClashItem.MergeKey, so a clash only a mirror found on such a pair is counted as a repeat and not added, said on the MIRROR MERGE lines | F132 | register row | merged | 211 | none | 2026-10-12 |
| F132-R6 | a saved test a person grouped gets one viewpoint per result group since the views read the report's rows, as the workbook and the pictures do, where attempt 1 gave each clash under a group its own, the lead's ruling of by design on the breaker's blocking point of attempt 2, turn6\f132q-break-addin.json, for Bader, dropped on 2026-10-12 under Q143 since one view per row is what the workbook and the pictures show | F132 | register row | dropped | none | none | 2026-10-12 |
| F115-R24 | a document the leftover walk declared damaged is still taken through the clash step and saved where a test is created or a view is built, the sets only return being the one guard, the breaker's reading of F128's attempt 2, older than F128 | F115 | register row | merged | 212 | none | 2026-10-13 |
| F127-R2 | the Coverage sheet moves out of the clash workbook into the internal workbook of its own beside it, where the Generic Models sheet is, Bader's answer B to Q146, the clash workbook keeping the Navisworks export's layout | F127 | register row | merged | 195 | none | 2026-10-11 |
| F127-R3 | the coverage reads the run's codes off the ticked groups, not every NWC the scan found, so a category only an unticked building carries is not named, the breaker's point, dropped on 2026-10-12 under Q143 since the row reads UNKNOWN and never a wrong word | F127 | register row | dropped | none | none | 2026-10-12 |
| F127-R4 | a mirrored test's coverage row is named by its run name, not the name the XML gives it, so the row is found by the name Clash Detective shows, dropped on 2026-10-12 under Q143 since the row carries the name the panel and the workbook show | F127 | register row | dropped | none | none | 2026-10-12 |
| F127-R5 | an internal workbook can take the path of another group's clash workbook, a group named X Internal beside a group X with the suffix Internal, one written over the other with no line, the breaker's point 6 | F127 | register row | merged | 209 | none | 2026-10-12 |
| F132-R7 | a status can be carried onto the wrong clash where one kept test has two mirrors and the first holds two copies of one pair with mixed statuses, since the twin check reads each mirror alone, the breaker's re-read of attempt 2 on pull request 211 | F132 | register row | open | none | none | 2026-10-12 |
| F120-R2 | the source file of every clash item reads empty on real runs, set 09's GAP block of 1A02MM naming no Source File line since no item carried one, so the file part of ClashItem.MergeKey is empty on every item and the mirror merge cannot tell two models' items of one id and one geometry name apart, found by FR-198's pass, which keeps the read for the key and says it read and empty | F132 | register row | open | none | none | 2026-10-12 |
| F115-R25 | the leftover walk clears the keys of a set whose search will not read and says nothing of why, SetBuilder.WalkForLeftovers, older than F115 | F115 | register row | open | none | none | 2026-10-12 |
| F115-R26 | the twin of a leftover is paired by set name and not by folder, so removing the twin can take the set at the path the file names while the leftover is renamed elsewhere, and the file's tests lose their side, older than PR 212 | F115 | register row | open | none | none | 2026-10-12 |
| F115-R27 | two sets at one path in the document are not said, the drift read, the rebuild, the finds count and the EMPTY SETS judgement describe the last copy and the other stays, older than PR 212 | F115 | register row | open | none | none | 2026-10-12 |
| F115-R28 | the leftover decision counts sides only off tests already saved in the document, so a set the picked XML's own tests point at but the XML does not define can be removed before those tests are made, older than PR 212 | F115 | register row | open | none | none | 2026-10-12 |
| F115-R29 | a set's planned twin is found by name and not in the leftover's folder, so a file that moved a set between folders can leave one set name in two folders, the breaker's re-read of attempt 2 on pull request 212 | F115 | register row | open | none | none | 2026-10-13 |
| F115-R30 | a side whose selection sources read null makes the side count null and refuses every leftover with a FAILURE line, whether Navisworks returns null there is UNKNOWN until a run, the breaker's re-read of attempt 2 on pull request 212 | F115 | register row | open | none | none | 2026-10-13 |

## Wave 2a and 2b

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q114 | one view per clash test by team (FR-180 to FR-188) | F131, F132, F114 | Bader's request | merged | 135 for F131, 177 for F132, 189 for F114 | none | 2026-10-08 |

## Wave 2b

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-065 | viewpoint dimming carries between viewpoints | F114 | silent wrong number | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-066 | size text reads tail of word digits | F114 | silent wrong number | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-067 | clear rebuild drops viewpoints, waits for the P15 probe docs\history\scan.md 5z-w names, not answered, Navisworks having failed twice at a read after CopyFrom | F114 | broken feature | open | none | none | 2026-10-12 |
| FR-068 | size tally never constructed | F114 | broken feature | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-069 | views cost per viewpoint | F114 | slow | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-070 | views 45 minute basis and options | F114 | slow | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-071 | views log silent up to 21 minutes | F114 | slow | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-072 | penetrations upwards walk outside try | F114 | loud failure | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-073 | views seconds parts do not add | F114 | noise | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-177 | generic models counted and a set per model | F128 | Bader's request | merged | 171 and 186 | none | 2026-10-08 |
| FR-180 | team map beside the picked xml | F131 | Bader's decision | merged | 135 | none | 2026-10-07 |
| FR-181 | mechanical sets miss hv pl fp models | F131 | Bader's decision | in progress | 135 merged for F131, the COVERAGE block and the form wait for F127 | none | 2026-10-07 |
| FR-184 | views tree by priority and team pair | F114 | Bader's decision | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-185 | one view per test of its open clashes | F114 | Bader's decision | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-186 | views made fresh only the tools own | F114 | Bader's decision | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-187 | views proof on 1a02mm and the views tree block | F114 | Bader's decision | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| FR-188 | views rules in docs workflow | F114 | Bader's decision | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| F114 | views | F114 | fix | merged | 141 the Core half and 189 the add-in pass | none | 2026-10-08 |
| F128 | generic models | F128 | fix | merged | 171 the Core part by lane B and 186 the probe and the add-in part | none | 2026-10-08 |
| F131 | teams | F131 | fix | merged | 135 | none | 2026-10-07 |
| Q112-3 | generic models counted and a set per model (FR-177) | F128 | Bader's request | merged | 171 and 186 | none | 2026-10-08 |
| Q77 | the clear and rebuild does not copy the viewpoints | F114 | question | merged | 131 | none | 2026-10-06 |
| Q115 | where the team map lives | F131 | question | merged | 118 | none | 2026-10-05 |
| Q116 | where the team map applies | F131 | question | merged | 118 | none | 2026-10-05 |
| Q117 | a set name with no discipline code | F114 | question | merged | 118 | none | 2026-10-05 |
| Q118 | a clashing item in a model of a third team | F114 | question | merged | 118 | none | 2026-10-05 |
| Q119 | which models a view shows | F114 | question | merged | 118 | none | 2026-10-05 |
| Q120 | a view the tool made that a person changed | F114 | question | merged | 118 | none | 2026-10-05 |
| Q123 | a run with no XML picked and the team map | F131 | question | merged | 118 | none | 2026-10-05 |
| F114-K29 | a team map may name a team UNKNOWN, so a side with no code and a side of that team share a pair folder | F114 | register row | merged | 228 | none | 2026-10-13 |
| F114-K30 | a view side is read by its set name alone, so two sets of one name giving two teams read UNKNOWN in the views and two teams on the TEAMS lines | F114 | register row | merged | 213 | none | 2026-10-13 |
| F114-K31 | a set with no name in the clash XML gets a TEAMS line naming no set, TeamMap.SetLines | F114 | register row | merged | 213 | none | 2026-10-13 |
| F114-K32 | a person's unmarked view of a test's name in the view's folder stops the tool's view of that test and costs the group DONE, for Bader | F114 | register row | merged | 227 | none | 2026-10-13 |
| F114-K33 | the read back walks the material overrides one index path each, 3153 for a view of one model of 1A02MM, dropped on 2026-10-12 under Q143 since P18 read the whole read back at 0.014 s a view, no time the team sees | F114 | register row | dropped | none | none | 2026-10-12 |
| F114-K34 | a mirror the rule paired and did not merge gets no view and its clashes are in no view, the VIEWS block counting them | F114 | register row | merged | 224 | none | 2026-10-13 |
| F114-K35 | a model whose code will not read is shown only where a clashing item lives in it, the VIEWS line saying so once a model | F114 | register row | merged | 189 | none | 2026-10-12 |
| F114-K36 | a test whose open clashes are all result groups gets no view and keeps its old ones, not counted FAILED, the lead's decision on the breaker's F3, for Bader | F114 | register row | merged | 229 | none | 2026-10-13 |
| F114-K37 | whether the NWF keeps a saved camera at single precision is UNKNOWN, so a camera far from the origin may read every tool view as a person's after a reopen, read on the timed runs | F114 | register row | open | none | none | 2026-10-08 |
| F114-K38 | a test whose harvest threw after only closed rows reads whole, so its old view is removed while its open clashes were never harvested, the reviewer's D1, UNKNOWN how often | F114 | register row | merged | 213 | none | 2026-10-13 |
| F114-R1 | a mirror pair the rule pairs but the merge refuses keeps its clashes on its own report test, and since the views read the rule's pairs those clashes get no view, so the views can hold fewer clashes than the report for that pair, named in the VIEWS block, the breaker's re-read of attempt 2 on pull request 213 | F114 | register row | merged | 224 | none | 2026-10-13 |
| F114-R2 | a mirror whose open rows are all result groups or that was not read whole never reaches the views' plan, so the line of open clashes of the mirrored tests in no view leaves its clashes out, named elsewhere, the reviewer's first point on pull request 224 | F114 | register row | open | none | none | 2026-10-13 |
| F114-R3 | two clashes inside one result group with the same display name share one key in a view, so the plan holds both under one key and the camera and the address of the second are the first's, fix is in the add-in's ReadTheClashesInside, a counter on the name, not Core only, found with F114-K36 | F114 | register row | open | none | none | 2026-10-13 |

## Wave 2b and before any test run

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q131 | the default of the viewpoints box | F114, F136 | question | merged | 118 | none | 2026-10-05 |

## Wave 2c

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-035 | workbook check counts only blocks with clashes | F118 | silent wrong number | merged | 147 | none | 2026-10-07 |
| FR-036 | skipped test row keeps old tolerance after chosen edit | F118 | silent wrong number | merged | 210 | none | 2026-10-12 |
| FR-037 | priority csv repeated test name last row wins silently | F118 | silent wrong number | merged | 147 | none | 2026-10-07 |
| FR-040 | item ids guid fallback counted missing | F118 | silent wrong number | merged | 159 | none | 2026-10-07 |
| FR-041 | grid location empty on 345 rows | F118 | broken feature | in progress | UNKNOWN | none | 2026-10-12 |
| F118 | workbook and report | F118 | fix | in progress | 147 FR-035 and FR-037 and 159 FR-199 and FR-040 merged, FR-036 and F118-R3 in review on fix-F118c, FR-041 counted on fix-F118c and its cause waits for a run | none | 2026-10-12 |
| Q45 | a count of the clashes this run moved to Reviewed | F118 | question | merged | 131 | none | 2026-10-06 |
| Q49 | a priority file replaces the measured block order | F118 | question | merged | 131 | none | 2026-10-06 |
| FR-199 | blocks stay in measured order with a priority file | F118 | Bader's decision | merged | 159 | none | 2026-10-07 |
| F118-R1 | the order check of the workbook reads the full blocks only so a one row test placed before a full block is not named | F118 | register row | merged | 176 | none | 2026-10-08 |
| F118-R2 | with a priority file picked the check does not run the Priority heading check on a sheet with no full block | F118 | register row | dropped | 176, lane B read it as no fault, a sheet of one row tests has no heading row | none | 2026-10-08 |
| F118-R3 | the group's own workbook is listed as written with last week's size where its save threw and an earlier run's file is at the path, the developer's reading on F128's attempt 2, older than F128, the same fault FR-189 names for the NWD | F118 | register row | merged | 210 | none | 2026-10-12 |

## Wave 3a

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-075 | picture rename two pass has no undo | F120 | silent wrong number | merged | 183 | none | 2026-10-08 |
| FR-076 | image guard per group not per run | F120 | broken feature | merged | 183 | none | 2026-10-08 |
| FR-077 | pictures cost 643 seconds | F120 | slow | merged | 183, the IMAGES seconds split and the encoder looked up once, the seconds saved UNKNOWN until the timed runs | none | 2026-10-08 |
| FR-078 | install recursive delete without junction check and partial first install | F109 | broken feature | merged | 194 | none | 2026-10-11 |
| FR-079 | install killed between move and removal | F109 | broken feature | merged | 194 | none | 2026-10-11 |
| FR-080 | install leftovers exit codes and success line | F109 | broken feature | merged | 194 | none | 2026-10-11 |
| FR-178 | start from an existing nwf | F129 | Bader's request | merged | 193 | none | 2026-10-11 |
| F109 | install | F109 | fix | merged | 194 | none | 2026-10-11 |
| F120 | harvest and pictures | F120 | fix | merged | 183, 215 FR-198 and F120-R1 | none | 2026-10-12 |
| F129 | start from an existing NWF | F129 | fix | merged | 193, F129-R1 dropped on fix-F129c | none | 2026-10-12 |
| Q112-4 | start from an existing NWF (FR-178) | F129 | Bader's request | merged | 193 | none | 2026-10-11 |
| Q25 | five clash item properties reach no output, split into Q35 to Q40 | F120 | question | merged | 131 | none | 2026-10-06 |
| Q35 | the GAP block, Family reaches no output | F120 | question | merged | 131 | none | 2026-10-06 |
| Q36 | the GAP block, Type Name | F120 | question | merged | 131 | none | 2026-10-06 |
| Q37 | the GAP block, Material | F120 | question | merged | 131 | none | 2026-10-06 |
| Q38 | the GAP block, Source File | F120 | question | merged | 131 | none | 2026-10-06 |
| Q39 | the GAP block, Discipline | F120 | question | merged | 131 | none | 2026-10-06 |
| Q40 | the GAP block, Id From | F120 | question | merged | 131 | none | 2026-10-06 |
| Q76 | the source file column empty on every row | F120 | question | merged | 131 | none | 2026-10-06 |
| FR-198 | item properties harvest deleted and gap says read and empty | F120 | Bader's decision | merged | 215 | none | 2026-10-13 |
| F120-R1 | a render that throws with a path inside the exception's message is still one reason per path for the fifty failures guard, so such failures never stop the run, the developer's finding on FR-076 | F120 | register row | merged | 215 | none | 2026-10-13 |
| F129-R1 | the picked NWFs walk follows a junction looping back until Windows refuses the path, so the same NWFs can be read under longer paths, a person's mklink needed, the scan sharing the exposure, the reviewer's point on attempt 1 | F129 | register row | dropped | 216 | none | 2026-10-12 |

## Wave 3b

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-043 | reshaped group done but nwf never saved | F119 | silent wrong number | merged | 219 | none | 2026-10-13 |
| FR-044 | nwf save false return ignored after rebuild | F119 | silent wrong number | merged | 219 | none | 2026-10-13 |
| FR-045 | nwf save after clash work false return ignored | F119 | silent wrong number | merged | 219 | none | 2026-10-13 |
| FR-046 | result file sizes wrong for the two open logs | F119 | silent wrong number | merged | 148 | none | 2026-10-07 |
| FR-047 | tsv number column rounds a tolerance | F119 | silent wrong number | merged | 219 | none | 2026-10-13 |
| FR-048 | tsv size zero for nwc not on disk | F119 | silent wrong number | merged | 148 | none | 2026-10-07 |
| FR-049 | second run in window result covers both runs | F119 | silent wrong number | merged | 179 | none | 2026-10-08 |
| FR-050 | run started but not finished reads as no run marked | F119 | silent wrong number | merged | 148 | none | 2026-10-07 |
| FR-051 | result states zero waiting when no run marked | F119 | silent wrong number | merged | 148 | none | 2026-10-07 |
| FR-052 | notolerance skip missing from skip reasons | F119 | silent wrong number | merged | 148 | none | 2026-10-07 |
| FR-053 | tolerance set on count counts tests not edited | F119 | silent wrong number | merged | 219 | none | 2026-10-13 |
| FR-054 | log prune in temp fallback deletes other programs logs | F119 | broken feature | merged | 148 | none | 2026-10-07 |
| FR-055 | tsv files never pruned | F119 | broken feature | merged | 148 | none | 2026-10-07 |
| FR-056 | live line slower compares one test with whole group | F119 | broken feature | merged | 148 | none | 2026-10-07 |
| FR-057 | run log write has no try | F119 | loud failure | merged | 148 the listener half, 169 the disk half | none | 2026-10-08 |
| FR-179 | shift range tick in the group list | F130 | Bader's request | merged | 192 | none | 2026-10-11 |
| FR-189 | nwd listed as written when its publish failed | F119 | silent wrong number | merged | 219 | none | 2026-10-13 |
| F119 | run log and RESULT | F119 | fix | merged | 148 nine items, 168, 169 and 179, and 219 the last six | none | 2026-10-13 |
| F130 | the Shift range in the group list | F130 | fix | merged | 192, 218 F130-R1 | none | 2026-10-12 |
| Q112-5 | Shift range tick in the group list (FR-179) | F130 | Bader's request | merged | 192 | none | 2026-10-11 |
| F130-R1 | a person who arrows onto the Run cell and presses Space no longer toggles it since the column is read only, the box itself still taking Space, the reviewer's point, UNKNOWN whether anyone ticks that way | F130 | register row | merged | 218 | none | 2026-10-12 |

## Wave 4

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-082 | clash xml copy not the exchange matrix | F122 | silent wrong number | open | none | none | 2026-10-04 |
| FR-083 | f106 verdict ran ignores group results | F122 | silent wrong number | open | none | none | 2026-10-04 |
| FR-084 | copy set takes bader outputs as inputs | F122 | silent wrong number | open | none | none | 2026-10-04 |
| FR-085 | f106 autosave putback deletes unowned files | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-086 | guard unreadable process list reads as none | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-087 | adopt runs after deadline path with watchdog stopped | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-088 | adopt start has no time bound | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-089 | put back key unreadable at end counted nowhere | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-090 | watch append failure switches off put back | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-091 | m5 walk has no time limit | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-092 | f106 harness stale after window run | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-093 | remove restore refuse name in both communities | F122 | broken feature | open | none | none | 2026-10-04 |
| FR-094 | f106 no hang clock when log not single | F122 | slow | open | none | none | 2026-10-04 |
| FR-095 | f106 monitor fault leaves plugin call unbounded | F122 | slow | open | none | none | 2026-10-04 |
| FR-096 | f106 read workbook no time limit | F122 | slow | open | none | none | 2026-10-04 |
| FR-097 | f106 foreign window stops driver and close | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-098 | f106 confirm child read timeout not seen | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-099 | unproved start refuses on any process holding the pid | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-100 | git wall reads main clone branch | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-101 | copy set no navisworks check | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-102 | copy manifest kept short or changed | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-103 | remove writes note before delete | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-104 | set remove advice sends to a refusal | F122 | loud failure | open | none | none | 2026-10-04 |
| FR-150 | apply file settings does not save nwf | F121 | silent wrong number | merged | 222 | none | 2026-10-13 |
| FR-151 | rebuild tally before minus one reads as none | F121 | silent wrong number | merged | 150 | none | 2026-10-07 |
| FR-152 | undo auto reviewed undoes a persons reviewed | F121 | silent wrong number | merged | 222 | none | 2026-10-13 |
| FR-153 | regroup throws away typed names and run ticks | F121 | silent wrong number | merged | 223 | none | 2026-10-13 |
| FR-154 | scan findings use group key as building code | F121 | silent wrong number | merged | 150 | none | 2026-10-07 |
| FR-155 | exchange reader missing attribute becomes zero or false | F121 | silent wrong number | merged | 222 | none | 2026-10-13 |
| FR-156 | probe csv listed written after write threw | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-157 | probe unread value written as empty value | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-158 | probe category match trims but tally does not | F121 | silent wrong number | merged | 150 | none | 2026-10-07 |
| FR-159 | emptied pattern field passes name check | F121 | broken feature | merged | 150 | none | 2026-10-07 |
| FR-160 | cleared name cell pinned as empty name | F121 | broken feature | merged | 223 | none | 2026-10-13 |
| FR-161 | date format has no control | F121 | broken feature | open | none | none | 2026-10-04 |
| FR-162 | folder memory save failure never reported | F121 | broken feature | merged | 222 | none | 2026-10-13 |
| FR-163 | title bar close mid run | F121 | broken feature | merged | 223 | none | 2026-10-13 |
| FR-164 | census sets overload returns zero not minus one | F121 | loud failure | merged | 150 | none | 2026-10-07 |
| FR-165 | cleared name cell throws out of run click | F121 | loud failure | merged | 150, and 160 the second half, a hand typed name Windows refuses | none | 2026-10-07 |
| FR-166 | tolerance other blank stops hand buttons | F121 | loud failure | merged | 223 | none | 2026-10-13 |
| F121 | the rest | F121 | fix | in progress | 222, 150 six items, 160 and 173 the SINGLE DISCIPLINE finding merged, FR-150, FR-152, FR-155 and FR-162 in review on fix-F121d, the rest on the laptop | none | 2026-10-13 |
| F122 | the loop tools | F122 | fix | open | none | none | 2026-10-04 |
| F125-R1 | a modeless window up after Run stops the driver while the monitor reads a pane | F122 | register row | open | none | none | 2026-10-04 |
| F125-R3 | a pane made again with a new handle after Run stops the driver | F122 | register row | open | none | none | 2026-10-04 |
| F125-R4 | a WPF pane and panes with other owners stay a dialog | F122 | register row | open | none | none | 2026-10-04 |
| F125-R5 | a message box owned by the pane before Run stops the driver after Run | F122 | register row | open | none | none | 2026-10-04 |
| F125-R6 | prove-run.ps1 has no PANE row and is out of date since F106 | F122 | register row | open | none | none | 2026-10-04 |
| F125-R7 | a modal dialog up when the window opens reads as a pane | F122 | register row | open | none | none | 2026-10-04 |
| F125-R8 | a dialog counted, then read as a pane, gets a line saying not a finding | F122 | register row | open | none | none | 2026-10-04 |
| F125-R9 | a noted pane that later reads as a dialog ends a finished run HUNG | F122 | register row | open | none | none | 2026-10-04 |
| F125-R10 | the monitor counts a dialog once per caption | F122 | register row | open | none | none | 2026-10-04 |
| F125-R11 | a window first written with children not read never gets its text | F122 | register row | open | none | none | 2026-10-04 |
| F125-R12 | two copies of the words for an owner of another process | F122 | register row | open | none | none | 2026-10-04 |
| F125-R13 | the driver counts passes after Run that held any window, not one window | F122 | register row | open | none | none | 2026-10-04 |
| F125-R14 | nw-guard.ps1's header and the F125 section still say not modal | F122 | register row | open | none | none | 2026-10-04 |
| T5-R-CLOSE | the C02 weekly closed by a copy of the guard's Kill, and run.ps1 cannot be asked to close | F122 | register row | open | none | none | 2026-10-05 |
| FR-202 | sub object ownership measured before two reads change | F121 | Bader's decision | open | none | none | 2026-10-06 |
| F109-R2 | run.ps1 reads the new exit 3 of install.ps1 as STOP where it read INSTALLED with a FINDING, and prove-run.ps1 trial 6c cannot run as written, the loop scripts frozen by Q143 item 5 | F122 | register row | open | none | none | 2026-10-08 |

## Wave 5

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-007 | invisible difference fallback for visible space | F123 | noise | merged | 151 | none | 2026-10-07 |
| FR-029 | set builder handles never disposed | F123 | noise | open | none | none | 2026-10-04 |
| FR-038 | workbook stray grey row after last empty test | F123 | noise | merged | 181 | none | 2026-10-08 |
| FR-039 | thumbnail row height overwritten by clash row height | F123 | noise | merged | 181 | none | 2026-10-08 |
| FR-042 | page check warns wrong order on priority sorted page | F123 | noise | merged | 159, closed by FR-199, the page always in the measured order, read by lane B at 181 | none | 2026-10-08 |
| FR-058 | first clash line says all tests to create | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-059 | result files written leaves out pictures | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-060 | result found none counts groups that ran no test | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-061 | log says tsv keeps collapsed lines when tsv did not open | F123 | noise | merged | 151 | none | 2026-10-07 |
| FR-062 | units line names unit when second read threw | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-063 | tolerance log line says read from xml on no xml run | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-064 | category line names no folder | F123 | noise | merged | 151 | none | 2026-10-07 |
| FR-074 | views unknown set not named | F123 | noise | merged | 181 | none | 2026-10-08 |
| FR-081 | f103w install failure claims and refused text | F123 | noise | open | none | none | 2026-10-04 |
| FR-105 | run verdict wording and dead fields | F123 | noise | open | none | none | 2026-10-04 |
| FR-106 | record writes unmasked exception text | F123 | noise | open | none | none | 2026-10-04 |
| FR-107 | close fails twice text says closed | F123 | noise | open | none | none | 2026-10-04 |
| FR-108 | run ps1 polish closeown buildserver ownertext | F123 | noise | open | none | none | 2026-10-04 |
| FR-109 | f106 evidence carries profile paths | F117 | noise | open | none | none | 2026-10-04 |
| FR-110 | evidence check blind to older machine name | F117 | noise | open | none | none | 2026-10-04 |
| FR-111 | f106 link check skips output folders | F123 | noise | open | none | none | 2026-10-04 |
| FR-112 | f106 logs after changed file counts covered | F123 | noise | open | none | none | 2026-10-04 |
| FR-113 | f106 toollog name first log wins | F123 | noise | open | none | none | 2026-10-04 |
| FR-114 | f106 answer handle reuse after lock wait | F123 | noise | open | none | none | 2026-10-04 |
| FR-115 | f106 confirm buttons matched by english names | F123 | noise | open | none | none | 2026-10-04 |
| FR-116 | f106 no free space check | F123 | noise | open | none | none | 2026-10-04 |
| FR-117 | watchdog load unmeasured | F123 | noise | open | none | none | 2026-10-04 |
| FR-118 | set number rule and readback written twice | F123 | noise | open | none | none | 2026-10-04 |
| FR-119 | probe finalizer claim printed as fact | F123 | noise | open | none | none | 2026-10-04 |
| FR-120 | probe main window read by pid alone | F123 | noise | open | none | none | 2026-10-04 |
| FR-121 | probe polish stale sentences and unnamed limit | F123 | noise | open | none | none | 2026-10-04 |
| FR-122 | f105 il reader prints zero for kinds never attempted | F123 | noise | open | none | none | 2026-10-04 |
| FR-123 | f105 failure list only printed with built addin | F123 | noise | open | none | none | 2026-10-04 |
| FR-124 | f105 probes share resolver and helpers in copies | F123 | noise | open | none | none | 2026-10-04 |
| FR-126 | single discipline sentence says every test is created | F123 | noise | merged | 154 | none | 2026-10-07 |
| FR-127 | tolerance help line says resets results | F123 | noise | merged | 154 | none | 2026-10-07 |
| FR-128 | apply line says results reset contradicts kept | F123 | noise | open | none | none | 2026-10-04 |
| FR-129 | failure guard words say first n tests | F123 | noise | merged | 154 | none | 2026-10-07 |
| FR-130 | refill message counts rows not cells | F123 | noise | merged | 154 | none | 2026-10-07 |
| FR-131 | outputs label wrong reason when nwf inside source | F123 | noise | merged | 154 | none | 2026-10-07 |
| FR-132 | rebuild box help line omits removal | F123 | noise | merged | 154 | none | 2026-10-07 |
| FR-133 | savedviewpoints canbuild switch nothing | F123 | noise | open | none | none | 2026-10-04 |
| FR-134 | stale units comment in finish the group | F123 | noise | open | none | none | 2026-10-04 |
| FR-135 | docs bader next says c06 never existed | F123 | noise | open | none | none | 2026-10-04 |
| FR-136 | docs step 377 log bar 300 kb | F123 | noise | open | none | none | 2026-10-04 |
| FR-139 | rule and runner still say source copy | F123 | noise | open | none | none | 2026-10-04 |
| FR-140 | f103w prove run header words | F123 | noise | open | none | none | 2026-10-04 |
| FR-141 | f103w readme run10 exit codes wrap | F123 | noise | open | none | none | 2026-10-04 |
| FR-142 | f103w probe and run comment words | F123 | noise | open | none | none | 2026-10-04 |
| FR-143 | f106 driver header no desktop search claim | F123 | noise | open | none | none | 2026-10-04 |
| FR-144 | junction recursion claim contradicted | F123 | noise | open | none | none | 2026-10-04 |
| FR-145 | readme write list names only source copy | F123 | noise | open | none | none | 2026-10-04 |
| FR-146 | f105 register wording attempts and helpers | F123 | noise | open | none | none | 2026-10-04 |
| FR-147 | restated facts check missing | F123 | noise | open | none | none | 2026-10-04 |
| FR-148 | audit 180 findings without verifier | F123 | noise | open | none | none | 2026-10-04 |
| FR-149 | older machine account name left in tree | F117 | noise | open | none | none | 2026-10-04 |
| FR-167 | shared source uses group key not building code | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-168 | folders memory read failure logged as first run | F123 | noise | merged | 155 | none | 2026-10-07 |
| FR-169 | document guard unreadable name called unsaved | F123 | noise | merged | 225 | none | 2026-10-13 |
| FR-170 | penetration size catch swallows unit throw | F123 | noise | open | none | none | 2026-10-04 |
| FR-171 | t1 noise findings 93 | F123 | noise | in progress | 162, 163, 164, 165, 174, 176 and 180 in part, the rest on the laptop | none | 2026-10-08 |
| FR-172 | t1 uncalled members 150 | F123 | noise | in progress | 161 two members deleted, the rest on the laptop | none | 2026-10-07 |
| FR-173 | t1 catch swallowing 77 | F123 | noise | in progress | 184 the 34 catches in Core sorted and five mended by lane B, the rest on the laptop | none | 2026-10-08 |
| FR-174 | one public type per file | F124 | noise | open | none | none | 2026-10-04 |
| F117 | the names | F117 | fix | open | none | none | 2026-10-04 |
| F123 | docs and words, and the noise of every area | F123 | fix | in progress | 225, 151, 154, 155, 161 to 165, 174, 176, 178, 180, 181, 182 and 184 merged in part, lane B stopped at 185, FR-058, FR-059, FR-060, FR-062, FR-063, FR-167 and FR-169 in review on fix-F123b, the rest on the laptop | none | 2026-10-13 |
| F124 | D1, one public type per file | F124 | fix | open | none | none | 2026-10-04 |
| Q27 | the two choice rule for still outstanding lost every reader | F123 | question | merged | 131 | none | 2026-10-06 |
| Q30 | the bundle manifest points at the old scan.md path | F123 | question | merged | 131 | none | 2026-10-06 |
| FR-203 | open statuses rule stays naming its reader | F123 | Bader's decision | merged | 131, records only, main already does it | none | 2026-10-06 |
| FR-204 | manifest line 9 and its one exception | F123 | Bader's decision | open | none | none | 2026-10-06 |
| F123-R1 | the file name or model count the guard cannot read is said in the discard warning with its reason, but written to the log only on the preview path, so the open file confirm and the run confirm leave no log line of it, the reviewer's second point on pull request 225 | F123 | register row | open | none | none | 2026-10-13 |
| F123-R2 | proof steps 1239 to 1242 can pass whatever NWF the document holds after the per discipline run, since they do not say which path the warning should name, the reviewer's note on pull request 225 | F123 | register row | open | none | none | 2026-10-13 |

## Wave 5 and 4

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q28 | two handle reads left where they are | F123, F121 | question | merged | 131 | none | 2026-10-06 |

## all

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q98 | the full fix round | all | Bader's request | in progress | none | none | 2026-10-04 |

## before any probe or run starts again

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| F138 | loop starts with Auto-Save off | F138 | fix | proven by a run | 127 | probes\q133-import-20261007-113149, Auto-Save off held on its real start | 2026-10-07 |
| FR-196 | loop runs write no autosave and copy his folder once | F138 | Bader's decision | in progress | 127 merged for part 1, part 2 in run.ps1 after F131 | none | 2026-10-06 |
| Q135 | too many autosave copies | F138 | Bader's request | in progress | 127 merged for part 1, part 2 in run.ps1 after F131 | none | 2026-10-06 |
| T5-R-HARNESSLIMIT | no harness run has a time limit, built in F138's branch | F138 | register row | merged | 127 | none | 2026-10-06 |

## before any test run

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-194 | viewpoints switched off for test runs | F136 | Bader's decision | merged | 117 | none | 2026-10-05 |
| F136 | the viewpoints switch | F136 | fix | merged | 117 | none | 2026-10-05 |
| Q130 | stop the C02 weekly run and no viewpoints in test runs until F114 merges (FR-194) | F136 | Bader's request | merged | 117 for F136, 116 records the stop | none | 2026-10-05 |

## before the test of wave 1

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-195 | no site and no clash groups end partial | F137 | Bader's decision | in progress | 146 part 1 merged, Q125 B waits for F114 add-in pass | none | 2026-10-07 |
| F137 | no site and no clash groups | F137 | fix | in progress | 146 part 1 merged, Q125 B waits for F114 add-in pass | none | 2026-10-07 |
| Q78 | C04 models naming misspelled shared sites | F137 | question | merged | 131 | none | 2026-10-06 |
| Q111 | a model that names no site at all | F137 | question | merged | 118 | none | 2026-10-05 |
| Q125 | a group that runs no clash test, a model off the coordinates | F137 | question | merged | 118 | none | 2026-10-05 |
| FR-201 | shared site named default reported only | F137 | Bader's decision | merged | 131, records only, main already does it | none | 2026-10-06 |
| F137-R1 | ModelFactsReader.SharedCoordinateOn returns an empty site where the Location tab is missing so a read fault reads as a model naming no site and skips the clash | F137 | register row | open | none | none | 2026-10-07 |
| F137-R2 | the reference model can itself name no site and ModelsRead counts placements not the document's models so a model whose read threw is never judged | F137 | register row | in progress | 173 the reference line merged by lane B, the ModelsRead half open | none | 2026-10-08 |
| F137-R3 | the comments of FederationEngine near lines 2154 and 2300 and ModelFactsReader near 154 still say a model naming no site fails its group either way | F137 | register row | open | none | none | 2026-10-07 |

## before the waves

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| F1 | fix the test name typo | F1 | fix | merged | merge 0432e82 | none | 2026-09-07 |
| F2 | fix the hardcoded probe path | F2 | fix | merged | merge 0432e82 | none | 2026-09-07 |
| F3 | fix the two wrong messages, done inside F22 | F22 | fix | merged | merge b041093 | none | 2026-09-07 |
| F4 | fix the units docstring | F4 | fix | merged | merge 0432e82 | none | 2026-09-07 |
| F5 | fix the sets built test | F5 | fix | merged | merge b8e502a | none | 2026-09-06 |
| F6 | fix the open file report folder | F6 | fix | merged | merge 8f44b9e | none | 2026-09-06 |
| F7 | write the RESULT block for the open file run | F7 | fix | merged | merge 41d05fb | none | 2026-09-06 |
| F8 | run the existing tests when no XML is picked | F8 | fix | merged | merge 1bc475f | none | 2026-09-06 |
| F9 | skip the units change on a CHANGED group | F9 | fix | merged | merge 7a21029 | none | 2026-09-07 |
| F10 | gate the page on its own flag | F10 | fix | merged | merge e2421e2 | none | 2026-09-07 |
| F11 | remove the dead code | F11 | fix | proven by a run | merge 18208aa | steps\runs\03 | 2026-10-01 |
| F12 | fix the docs that contradict the code, closed by F37 | F37 | fix | merged | merge 1f69d73 | none | 2026-09-12 |
| F13 | split CLAUDE.md into rules, closed by F38 | F38 | fix | merged | merge 27edcee | none | 2026-09-12 |
| F14 | add a root README, closed by F37 | F37 | fix | merged | merge 1f69d73 | none | 2026-09-12 |
| F15 | dispose in SetBuilder and ClashRunner.Resolve, closed by F41 | F41 | fix | merged | merge 80023a6 | none | 2026-09-12 |
| F16 | make the tests path neutral | F16 | fix | merged | merge 47e3a9f | none | 2026-09-12 |
| F17 | picture numbering by block order | F17 | fix | merged | merge ff9811b | none | 2026-09-07 |
| F18 | add the 1A04WE sample, dropped by Bader on 2026-10-06, Q9 | F18 | fix | dropped | 131 | none | 2026-10-06 |
| F19 | CI for the add-in, dropped on Q10 | F19 | fix | dropped | none | none | 2026-09-06 |
| F20 | run the Core tests on every push to main | F20 | fix | merged | merge c58f4b1 | none | 2026-09-07 |
| F21 | the log answers timing and counts, closed by F60 | F60 | fix | merged | merge 5e69c2b | none | 2026-09-19 |
| F22 | two clear workflows in the window | F22 | fix | merged | merge b041093 | none | 2026-09-07 |
| F23 | fix B12, the NWD opened from ACC, closed as not reproducible, Q20 | F23 | fix | dropped | 70 | none | 2026-09-21 |
| F24 | rebuild a CHANGED NWF from the scan | F24 | fix | merged | merge 477438c | none | 2026-09-07 |
| F25 | drop the hidden discipline rule, closed by F27 | F27 | fix | merged | merge df4ed8e | none | 2026-09-12 |
| F26 | units always meters | F26 | fix | proven by a run | merge c89dad0 | steps\runs\03 | 2026-10-01 |
| F27 | the GROUPS block tells the truth | F27 | fix | proven by a run | merge df4ed8e | steps\runs\03 | 2026-10-01 |
| F28 | a set already there is not counted as created | F28 | fix | merged | merge 799320e | none | 2026-09-12 |
| F29 | the rebuild keeps the sets on their own count | F29 | fix | merged | merge fc8f7ee | none | 2026-09-12 |
| F30 | one tail for both run paths | F30 | fix | merged | merge 0b0a745 | none | 2026-09-12 |
| F31 | the clash side lookup is built once per run | F31 | fix | merged | merge e10fee3 | none | 2026-09-12 |
| F32 | the open file is guarded | F32 | fix | merged | merge d6ce6e1 | none | 2026-09-12 |
| F33 | one unit table | F33 | fix | proven by a run | merge 5b7bf04 | steps\runs\03 | 2026-10-01 |
| F34 | window wiring | F34 | fix | proven by a run | merge 2019584 | steps\runs\03 | 2026-10-01 |
| F35 | clash only where two disciplines meet | F35 | fix | proven by a run | merge 41f6399 | steps\runs\03 | 2026-10-01 |
| F36 | dead code and copies out | F36 | fix | merged | merge 6cd3858 | none | 2026-09-12 |
| F37 | one type per file and one place per kind of file | F37 | fix | merged | merge 1f69d73 | none | 2026-09-12 |
| F38 | CLAUDE.md under 200 lines, rules in .claude, walls in hooks | F38 | fix | merged | merge 27edcee | none | 2026-09-12 |
| F39 | the window compiles again | F39 | fix | proven by a run | merge c8644bd | steps\runs\03 | 2026-10-01 |
| F40 | dead members out, second pass | F40 | fix | merged | merge 0eb5275 | none | 2026-09-12 |
| F41 | every handle disposed | F41 | fix | merged | merge 80023a6 | none | 2026-09-12 |
| F42 | no framework message in a label | F42 | fix | merged | merge eb85bac | none | 2026-09-12 |
| F43 | three settings that are constants | F43 | fix | merged | merge caad972 | none | 2026-09-12 |
| F44 | the docs and the comments agree with the code | F44 | fix | merged | merge 42aab9a | none | 2026-09-12 |
| F45 | the clash step keeps its rules | F45 | fix | merged | merge 6f16161 | none | 2026-09-12 |
| F46 | the four the chat audit found | F46 | fix | merged | merge b00b6e2 | none | 2026-09-12 |
| F47 | what the chat audit of 2026-09-18 found, F47a to F47c | F47 | fix | merged | merge 8be47d3 | none | 2026-09-18 |
| F47a | the hooks do not run on a Windows checkout | F47 | fix | merged | merge f7f9760 | none | 2026-09-18 |
| F47b | one doubled comment left | F47 | fix | merged | merge 272f4a4 | none | 2026-09-18 |
| F47c | two names recorded wrongly in the F40 entry | F47 | fix | merged | merge 8be47d3 | none | 2026-09-18 |
| F50 | the NWF strategy, new against existing | F50 | fix | merged | merge 295d46c | none | 2026-09-18 |
| F51 | the ACC warning | F51 | fix | merged | merge 16391c8 | none | 2026-09-18 |
| F52 | a viewpoint per discipline, grouped by discipline | F52 | fix | proven by a run | merge b57a6c4 | steps\runs\03 | 2026-10-01 |
| F53 | the 150 mm rule and the sub groups | F53 | fix | merged | merge ffce870 | none | 2026-09-18 |
| F54 | clashes that cannot be solved become Reviewed | F54 | fix | merged | merge 3a35838 | none | 2026-09-18 |
| F55 | the rules and the docs catch up | F55 | fix | merged | merge 4cc8c69 | none | 2026-09-18 |
| F56 | what the real read of 03_bader_next.md found | F56 | fix | merged | merge 4092018 | none | 2026-09-18 |
| F57 | five Look for lines older than the feature round | F57 | fix | merged | 67 | none | 2026-09-20 |
| F58 | the add-in compiles again | F58 | fix | proven by a run | merge 2d0bda1 | steps\runs\03 | 2026-10-01 |
| F59 | every step is named and timed | F59 | fix | proven by a run | merge e8db199 | steps\runs\03 | 2026-10-01 |
| F60 | the timing blocks, and F21 closes here | F60 | fix | proven by a run | merge 5e69c2b | steps\runs\03 | 2026-10-01 |
| F61 | the document census | F61 | fix | proven by a run | merge 3d3bf83 | steps\runs\03 | 2026-10-01 |
| F62 | the live line in the window | F62 | fix | merged | merge 2200df1 | none | 2026-09-19 |
| F63 | the report gap block | F63 | fix | proven by a run | merge 6743887 | steps\runs\03 | 2026-10-01 |
| F64 | the machine readable log | F64 | fix | proven by a run | merge fd534c2 | steps\runs\03 | 2026-10-01 |
| F65 | the missing import | F65 | fix | proven by a run | 58 | steps\runs\03 | 2026-10-01 |
| F66 | the check that would have caught it | F66 | fix | merged | 58 | none | 2026-09-19 |
| F67 | one doubled comment | F67 | fix | merged | 58 | none | 2026-09-19 |
| F68 | the build section learns what today cost | F68 | fix | merged | 58 | none | 2026-09-19 |
| F69 | the other eighteen errors, and the first proved build | F69 | fix | merged | 58 | none | 2026-09-19 |
| F70 | both probes run, and two standing unknowns answered | F70 | fix | merged | 58 | none | 2026-09-19 |
| F71 | say when an NWF is nearly matched | F71 | fix | merged | 59 | none | 2026-09-19 |
| F72 | penetrations become Reviewed, which answers Q33 | F72 | fix | merged | 65 | none | 2026-09-20 |
| F72a | Pipe Insulation is a service, and the matrix test | F72a | fix | merged | 60 | none | 2026-09-19 |
| F72b | by design connections become Reviewed | F72b | fix | merged | 62 | none | 2026-09-19 |
| F72c | the record in the NWF and the undo | F72c | fix | merged | 62 | none | 2026-09-19 |
| F73 | appending brings the viewpoints in, which is not a fault | F73 | fix | proven by a run | 60 | steps\runs\03 | 2026-10-01 |
| F74 | the NWF that read empty is stopped and never rebuilt | F74 | fix | merged | 62 | none | 2026-09-19 |
| F75 | the document is emptied at the top of every group | F75 | fix | merged | 62 | none | 2026-09-19 |
| F76 | the tolerance can be chosen in the tool and it wins | F76 | fix | merged | 62 | none | 2026-09-19 |
| F77 | a test whose side finds nothing is not created at all | F77 | fix | merged | 62 | none | 2026-09-19 |
| F78 | the log says or where the file says or | F78 | fix | proven by a run | 60 | steps\runs\03 | 2026-10-01 |
| F79 | which missing item ids are this run's | F79 | fix | merged | 60 | none | 2026-09-19 |
| F80 | the run time is the run and not the session | F80 | fix | proven by a run | 60 | steps\runs\03 | 2026-10-01 |
| F81 | the .log is trimmed and the .tsv keeps everything | F81 | fix | merged | 60 | none | 2026-09-19 |
| F82 | sets across the run | F82 | fix | proven by a run | 60 | steps\runs\03 | 2026-10-01 |
| F83 | the clash priority reaches the report | F83 | fix | merged | 62 | none | 2026-09-19 |
| F84 | the sets that cannot match anything | F84 | fix | merged | 60 | none | 2026-09-19 |
| F85 | the saved viewpoints in three layers | F85 | fix | merged | 63 | none | 2026-09-20 |
| F86 | the property probe | F86 | fix | merged | 62 | none | 2026-09-19 |
| F87 | correct the matrix, the hyphen and BLD-EL-Devices | F87 | fix | merged | 60 | none | 2026-09-19 |
| F88 | restore the sample thirteen tests read | F88 | fix | merged | 60 | none | 2026-09-19 |
| F89 | the tick box that rebuilds a drifted set | F89 | fix | merged | 67 | none | 2026-09-20 |
| F90 | a CHANGED group brought up to date without clearing it | F90 | fix | merged | 67 | none | 2026-09-20 |
| F91 | the run tail carries the clash total and the decisions | F91 | fix | proven by a run | 70 | steps\runs\03 | 2026-10-01 |
| F92 | a clash test that found nothing is one row | F92 | fix | merged | 70 | none | 2026-09-21 |
| F93 | the unused twin removed and the working set renamed | F93 | fix | merged | 70 | none | 2026-09-21 |
| F94 | the reshape's three defects and the damaged document rule | F94 | fix | merged | 70 | none | 2026-09-21 |
| F95 | the two fixtures | F95 | fix | merged | 70 | none | 2026-09-21 |
| F96 | the restated facts check | F96 | fix | open | none | none | 2026-09-21 |
| F97 | the house of the loop | F97 | fix | merged | 72 | none | 2026-09-28 |
| F98 | the close round heading back in steps\log.md | F98 | fix | merged | 73 | none | 2026-09-28 |
| F99 | the git wall through PowerShell | F99 | fix | merged | 72 | none | 2026-09-28 |
| F100 | a start of Navisworks with no click, measured | F100 | fix | merged | 74 | none | 2026-09-29 |
| F101 | the no-click entry, built first only if the window cannot be driven, D4 of Q80, never started | F101 | fix | open | none | none | 2026-09-29 |
| F102 | every result file read for a machine name or licensing id | F102 | fix | merged | 76 | none | 2026-10-01 |
| F103 | tools\loop\run.ps1, part 1 | F103 | fix | proven by a run | 78 | steps\runs\02\item0 | 2026-10-01 |
| F104 | the check of a workbook against its document | F104 | fix | merged | 96 | none | 2026-10-04 |
| F105 | four reads off the install with no Navisworks started | F105 | fix | merged | 90 | none | 2026-10-04 |
| F106 | the window run, items 1 to 5 through the real window | F106 | fix | merged | 88 | none | 2026-10-06 |
| F107 | the name of the machine of 2026-09-19 masked on main | F107 | fix | merged | 92 | none | 2026-10-04 |
| F108 | a fresh copy of the real files for each run set | F108 | fix | merged | 84 | none | 2026-10-04 |
| F110 | the WORKBOOK CHECK counts only the blocks that found clashes, set 03 finding 4 (FR-035) | F110 | fix | open | none | none | 2026-10-06 |
| F111 | the RESULT block prints file sizes that are not the files', set 03 finding 5 (FR-046) | F111 | fix | open | none | none | 2026-10-06 |
| T3-B | the build stamp reads +edits for any untracked file, done by run.ps1's tree refusal | F103 | register row | merged | 78 | none | 2026-10-01 |

## beside the waves

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-191 | work tracker one place for status | F133 | Bader's decision | merged | 122 | none | 2026-10-06 |
| FR-192 | code health gate lists only shrink | F134 | Bader's decision | in progress | none, measured under %LOCALAPPDATA%\NwcFederatorLoop\health | none | 2026-10-05 |
| FR-193 | analyser settings merged alone | F135 | Bader's decision | open | none | none | 2026-10-05 |
| F133 | the work tracker | F133 | fix | merged | 122 | none | 2026-10-06 |
| F134 | the code health gate | F134 | fix | in progress | none, measured under %LOCALAPPDATA%\NwcFederatorLoop\health | none | 2026-10-05 |
| F135 | the analyser settings that touch every project | F135 | fix | open | none | none | 2026-10-05 |
| Q129 | a clean tracker and a code health gate (FR-191 to FR-193) | F133, F134, F135 | Bader's request | in progress | 122 merged for F133, F134 and F135 not merged | none | 2026-10-06 |

## first of all since Bader's order of 2026-10-05

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q132 | the new viewpoints first, F136 then F131, F132 and F114 | F136, F131, F132, F114 | Bader's request | merged | 117 for F136, 135 for F131, 177 for F132, 189 for F114 | none | 2026-10-08 |

## none

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-125 | five failed groups need note for modellers | none | loud failure | merged | 89 | none | 2026-10-04 |
| FR-137 | rules loop md says logs never pushed out | none | noise | merged | 83 | none | 2026-10-04 |
| FR-138 | rules say loop never writes autosave | none | noise | merged | 83 | none | 2026-10-04 |
| FR-175 | no sleep and keep awake | none | Bader's request | merged | 99, records only, nothing to merge | none | 2026-10-04 |
| Q81 | local main tracked origin/master, its upstream set to origin/main | none | Bader's request | merged | 74, records only, nothing to merge | none | 2026-09-29 |
| Q93 | a reading of the loop's own scripts blocks a run only for harm or wrong evidence | none | Bader's request | merged | 83 | none | 2026-10-04 |
| Q94 | code runs every real file itself, full runs of main and the fixes they show | none | Bader's request | in progress | none | none | 2026-10-06 |
| Q95 | keep the PC awake for the whole loop | none | Bader's request | merged | 82, records only, nothing to merge | none | 2026-10-01 |
| Q96 | main installed and the first run of C06 through the real window | none | Bader's request | proven by a run | 85 | steps\runs\03 | 2026-10-01 |
| Q97-1 | the findings of the first C06 run in steps\runs\03\findings.md | none | Bader's request | merged | 86 | none | 2026-10-01 |
| Q97-2 | the first run of C07 and the run set that night, replaced by Q98's no run of C07 now | none | Bader's request | dropped | none | none | 2026-10-04 |
| Q112-1 | no sleep and keep awake (FR-175) | none | Bader's request | merged | 99, records only, nothing to merge | none | 2026-10-04 |
| Q29 | three settable properties nothing outside the code sets | none | question | merged | 131 | none | 2026-10-06 |
| Q31 | five names listed wrongly in the F40 entry | none | question | merged | 131 | none | 2026-10-06 |
| Q47 | four service categories not on the service list | none | question | merged | 131 | none | 2026-10-06 |
| Q50 | F72c widens the one status guard for the undo | none | question | merged | 131 | none | 2026-10-06 |
| Q51 | the first size property or the largest | none | question | merged | 131 | none | 2026-10-06 |
| Q110 | a skipped group and its tests | none | question | merged | 118 | none | 2026-10-05 |
| Q124 | the baseline of 1A04PK takes a day or more | none | question | merged | 118 | none | 2026-10-05 |
| Q128 | what the C04 baseline run did not put back | none | question | merged | 121 | none | 2026-10-05 |
| Q134 | code that waits for F114 by Bader's order | none | question | merged | 124 | none | 2026-10-05 |
| Q137 | a saved mirror whose kept test the document does not say | none | question | merged | 128 | none | 2026-10-06 |
| Q138 | a clash both tests find, with two statuses | none | question | merged | 128 | none | 2026-10-06 |
| F105-R4 | probe comments say nothing is dropped where a read handed to no IlFail is in no count | none | register row | open | none | none | 2026-10-04 |
| F104-R1 | a documents read ends READ when nothing was read | none | register row | open | none | none | 2026-10-04 |
| F104-R2 | the picture and priority switches of a documents read are typed by hand | none | register row | open | none | none | 2026-10-04 |
| F104-R3 | a group with no NWF and no workbook is in no count of a documents read | none | register row | open | none | none | 2026-10-04 |
| F104-R4 | AGREE can stand while the panel count differs for a result group, PQ4 | none | register row | open | none | none | 2026-10-04 |
| F104-R5 | the probe's viewpoint walk is unmeasured and unbounded | none | register row | open | none | none | 2026-10-04 |
| F104-R6 | a documents read fills his recent files with the loop's NWFs | none | register row | open | none | none | 2026-10-04 |
| F104-R7 | the other notes of both readers of F104 part 2 | none | register row | open | none | none | 2026-10-04 |
| T5-R-GUARDRACE | the guard refused P10's put back over a sighting before the adoption | none | register row | open | none | none | 2026-10-05 |
| T5-R-OLDKEY | 40 values of his 22.0 key went back to older values, the writer UNKNOWN, for Bader | none | register row | open | none | none | 2026-10-06 |
| F133-R1 | an F number given to work elsewhere gets its tracker row by hand only | none | register row | open | none | none | 2026-10-05 |
| F133-R4 | work done outside the repo has no status of the seven, for Bader | none | register row | open | none | none | 2026-10-05 |
| F133-R5 | the PR column holds free text beside numbers | none | register row | open | none | none | 2026-10-05 |
| F133-R6 | a row is checked against its own cells in one way only | none | register row | open | none | none | 2026-10-05 |
| F133-R9 | a wave 10 would sort between 1 and 2a | none | register row | open | none | none | 2026-10-05 |
| F133-R10 | two open pull requests both change the counts, of tracker.md and since F139 of the page | none | register row | open | none | none | 2026-10-05 |
| F133-R12 | the counts count rows, and one piece of work can be three rows | none | register row | open | none | none | 2026-10-05 |
| F133-R13 | no status of the seven names an order still in force | none | register row | open | none | none | 2026-10-05 |
| F133-R16 | the tracker fixtures are written by scripts kept outside the repo | none | register row | open | none | none | 2026-10-05 |
| F133-R17 | a request of Bader's is known only by From Bader at its start | none | register row | open | none | none | 2026-10-05 |
| F133-R18 | a question row's area and wave come only from the FR items naming it | none | register row | open | none | none | 2026-10-05 |
| F133-R19 | any text after Answer: reads as Bader's answer | none | register row | open | none | none | 2026-10-05 |
| F133-R20 | lines error: failed to delete in the commit output, for the lead | none | register row | open | none | none | 2026-10-06 |
| F133-R21 | the class column has an allowed value only on an FR row | none | register row | open | none | none | 2026-10-06 |
| F133-R22 | only an indented Answer: line counts | none | register row | open | none | none | 2026-10-06 |
| F133-R23 | the question reader ends an item only at the next item | none | register row | open | none | none | 2026-10-06 |
| F133-R24 | rows of class Bader's request are checked for being there only | none | register row | open | none | none | 2026-10-06 |
| F133-R25 | a pull request opened again under another number leaves rows naming the old one | none | register row | open | none | none | 2026-10-06 |
| F133-R26 | the check reads that a merged question row's PR is a number, not which merge | none | register row | open | none | none | 2026-10-06 |
| F139-R1 | the keep-awake script reads steps\loop.md for STATE CLOSED, gone once F139 merges, for the lead | none | register row | proven by a run | none, outside the repo | turn5\keep-awake.txt 13:38:15 reads steps\PROGRESS.md | 2026-10-07 |
| F139-R4 | the Stop gate's VERDICT rule is a copy in sh of run.ps1's HasVerdict | none | register row | open | none | none | 2026-10-06 |
| F139-R6 | tools\loop\prove-hooks.sh, which holds the Stop gate's cases, is not run by Actions | none | register row | open | none | none | 2026-10-06 |
| T5-R-HARNESSWAITS | waits of the harness with no limit of their own, the H0 read covering none of them | none | register row | open | none | none | 2026-10-06 |
| T5-R-HARNESSARGS | prove-run.ps1's param block has no CmdletBinding so a misspelt limit is dropped in silence | none | register row | open | none | none | 2026-10-06 |
| T5-R-M5AUTOSAVE | the switch comes after M5's read, so m5.txt names the loop's own write of the autosave key | none | register row | open | none | none | 2026-10-06 |
| T5-R-HELDOFF | a run that leaves 3 0 makes the next backup read it as his value, put back by hand | none | register row | open | none | none | 2026-10-06 |
| F139-R7 | a commit staging tracker.csv or the page is refused where no powershell is on PATH, such as a container, a stated limit the lead accepted | none | register row | dropped | none | none | 2026-10-07 |
| F139-R8 | steps\logs\README.md line 11 names steps/log.md, in a folder never edited, for Bader | none | register row | merged | 139 | none | 2026-10-07 |
| F139-R9 | the Stop gate's merge test reads only a line starting STATE and a space in capitals, while its reader takes other spellings, so a page written another way is refused until respelled, the lead writing the plain form | none | register row | open | none | none | 2026-10-07 |
| step 228-233 | the published NWD checked in ACC, by Bader himself at the final run after F114 merges, dropped by his Q149 item 1 | none | register row | dropped | none | none | 2026-10-11 |
| step 346-352 | the remote branches other than main whose pull requests merged, deleted by the lead by Bader's answer, D6 | none | register row | proven by a run | none | turn5\d6-deleted.txt, 67 deleted and read back, 17 kept by his rule | 2026-10-07 |
| step 364 | the property probe on one mechanical NWC, run by the lead by Bader's answer, its CSV and PROBE block kept | none | register row | proven by a run | none | turn5\probe-step364-result.txt and its CSV, scan.md 5z-za, one ME NWC of run set 04 | 2026-10-07 |
| T5-R-WALKRACE | the harness's walk of the loop folder races a lane building under a wt- folder | none | register row | open | none | none | 2026-10-07 |
| Q141 | a clash in a result group with two statuses | none | question | merged | 139 | none | 2026-10-07 |
| Q142 | when a mirror's clashes may be merged | none | question | merged | 139 | none | 2026-10-07 |
| T5-R-P15CRASH | probe P15 read a viewpoints copy after Document.Clear, Navisworks crashed and the guard refused the put back, put back by the lead at 14:53 from its backup | none | register row | proven by a run | none | turn5\restart\putback-rebuild-views-20261007-144356-write, 0 still different | 2026-10-07 |
| F115-R13 | the F115 log entry was written as the lead's delegate | none | register row | open | none | none | 2026-10-05 |
| Q143 | fast to a team release, the seven limits, the two lanes and the hourly lines | none | Bader's request | merged | 157 | none | 2026-10-07 |
| Q144 | a run with no XML and the by design pass on the saved tests | none | question | merged | 191 | none | 2026-10-11 |
| Q145 | the new order on the laptop, F132 then F128, F120, F114, the timed runs, F129, F130, F109 and the release | none | Bader's request | in progress | none | none | 2026-10-08 |
| Q146 | where the Generic Models sheet goes | none | question | merged | 191 | none | 2026-10-11 |
| Q147 | two small fixes, then carry on, the attribution off, item 5's steps in the repo, F114 then F129, F130 and F109 | none | Bader's request | in progress | none | none | 2026-10-08 |
| Q148 | a fault found at the second reading | none | question | merged | 191 | none | 2026-10-11 |
| T6-R-TESTTEMP | RowLogTests.ALogThatFellBackTakesItsRowFileWithIt writes a log into the shared temp folder on every run, so it fails once about 100 have built up and when two worktrees test at once, seen on the F130 and F109 commits, the files moved aside to turn6 f109b-temp-runlogs | none | register row | open | none | none | 2026-10-08 |
| F109-R1 | how the 27 people get a built bundle is UNKNOWN, CLAUDE.md saying they have no compiler while INSTALL.md builds with dotnet, answered by Bader's Q149 item 2, the team package F140 | F140 | register row | in progress | none, F140 prepares it, no team zip until Bader asks | none | 2026-10-11 |
| Q149-1 | no release runs, the full runs of C06 and C07 and the ACC check dropped, the proof runs of 1A02MM and 1A04PK the last | none | Bader's request | merged | 194 | none | 2026-10-11 |
| Q149-2 | the team package prepared, a zip with a one click install, version, commit and date, the previous add-in kept to put back, a plain page, no zip until Bader asks | F140 | Bader's request | in progress | none | none | 2026-10-11 |
| F140 | the team package, prepared and no team zip made until Bader writes make the team package | F140 | fix | merged | 208 | none | 2026-10-12 |
| T6-R-ZEROCLASHES | TIMING BESIDE SIZE writes 0 clashes for a group whose clash was skipped, while RESULT reads clashes found UNKNOWN and the block's own legend says UNKNOWN is never a zero, set 05 item1-C02 line 625 and item1-C04 line 684 | F119 | register row | merged | 219 | none | 2026-10-13 |
| T6-R-MIRRORWORDS | the MIRROR block says both tests of each pair are run over a group whose clash the coordinates rule skipped, set 05 item1-C02 line 263 and item1-C04 line 322 | F132 | register row | open | none | none | 2026-10-11 |
| T6-R-TREEREMOVED | the VIEWS TREE after line reads removed 0 at once where the inventory removed 14 views at once with their folder in one RemoveAt, so the views written and removed read as never removed, set 06 item1-C02 lines 1504 to 1520 against 1745 | F114 | register row | open | none | none | 2026-10-11 |
| F141 | the framed views that threw | F141 | fix | proven by a run | 196 the first form of attempt 1, 197 the readers' points and the records, and 201 attempt 2 | steps\runs\08\item1-C02 | 2026-10-11 |
| F141-R1 | every view of one clash in set 06's run of 1A02MM, 14 of 61, and every view in set 07's, 69 of 69, was written, marked and read back with its camera and then failed as added without its dimming, the read back finding no material override where P17 read 3153 off a framed view, so each was removed and the test has no view | F141 | register row | proven by a run | 201 | steps\runs\08\item1-C02 | 2026-10-11 |
| F141-R2 | the read back compares the camera's position alone, so a framed view whose zoom or direction was lost reads back at distance 0 and is counted created, the breaker's point 2 on F141, older than F141 | F141 | register row | open | none | none | 2026-10-11 |
| F141-R3 | a view of two clashes or more whose framing box is not made, every centre unread, keeps Clash Detective's camera of one clash with no line saying so, the breaker's point 3 on F141, older than F141 | F141 | register row | open | none | none | 2026-10-11 |
| F143 | the harvest answers between rows | F143 | fix | merged | 199 | none | 2026-10-11 |
| F143-R1 | the penetration and by design passes read both sides of every clash of a test with no piece, so with either box ticked a test of many clashes leaves the window not answering and the log silent for that pass, the breaker's point 2 on F143 | F143 | register row | open | none | none | 2026-10-11 |
| F143-R2 | one render on 1A04PK cost 1.4 to 10.5 s a row in set 06, so between two pumps the window can read Not Responding, which Windows shows after 5 s, for up to that long, under the 30 s Bader set and above the 5 s Windows does | F143 | register row | open | none | none | 2026-10-11 |
| F142 | the life check | F142 | fix | merged | 198 | none | 2026-10-11 |
| F142-R1 | one run of prove-life.ps1 of three ended exit -1 right after its first check in the runspace it makes, no error printed, its finally not run and its three LifeStandIn stand-ins left to end by themselves, turn6 f142b-prove-1.txt, cause UNKNOWN | F142 | register row | open | none | none | 2026-10-11 |
| F142-R2 | the life check's query of the machine's memory has a 5 s timeout on reading its results, and whether that timeout also bounds the connection to WMI is UNKNOWN, so a WinMgmt that will not connect could still hold a pass of the monitor, the reviewer's point 6 on attempt 2 | F142 | register row | open | none | none | 2026-10-11 |
| F144 | the first-50 copy for the fast checks | F144 | fix | merged | 200 | none | 2026-10-11 |
| F144-R1 | the first 50 tests of the corrected matrix on 1A04PK hold BLD-AR-Walls-vs-BLD-AR-Floors, whose harvest took 5817 s in set 06, and set 07's cut reached row 305 of its 1556 in 37 min 38 s at 4 to 8.6 s a picture row, so the cut down 1A04PK is a check of hours and not minutes while every picture is rendered, Q150 | F144 | register row | open | none | none | 2026-10-11 |
| Q150 | the pictures of 1A04PK, the four ways measured, only the test's models shown, shaded, 800 by 600, or all three | none | question | merged | 206 | none | 2026-10-12 |
| F148 | two tests that filled the shared temp folder | F148 | fix | merged | 203 | none | 2026-10-11 |
| Q151 | a picture of the two clashing items alone, through Hide Other or GenerateImage, faster but not the export's look, and which look every machine writes | none | question | merged | 206 | none | 2026-10-12 |
| Q152 | Q150's choice as F145, long runs kept from slowing, the 50 test cut, the GPU read, GenerateImage measured, one full 1A04PK only on his word | none | Bader's request | in progress | none | none | 2026-10-12 |
| Q153 | keep fixing the time until it is at its minimum, the four targets, the SPEED table, the 150 times gap first, F145 merged | none | Bader's request | in progress | none | none | 2026-10-12 |
| F147 | long runs kept from slowing, the undo kept from growing during the pictures, the texture cache under %TEMP%\ogs cleared between tests past 5 GB, the memory at the start and end of each step and the rows each step holds released as it ends, Q152 item 2 and Q155 | F147 | fix | merged | 217 | none | 2026-10-13 |
| T6-R-WPFBOX | a caption less WPF window of Navisworks came up as the tool's window closed in both of set 09's runs, steps\runs\09, its text UNKNOWN, first seen with acceleration on | none | register row | open | none | none | 2026-10-12 |
| Q154 | Hide Other for every picture set by the tool, the permission that blocks the merges, the five merges, then the speed loop | none | Bader's request | merged | 206 | none | 2026-10-12 |
| Q155 | F147 stopped under Q148 B at the re-read, a third attempt for the seconds of the deletions and the checks after a partial clearing | none | question | merged | 214 | none | 2026-10-13 |
| F150 | Hide Other set by the tool for its pictures and the user's value put back, the ghosted look one setting off by default, Q151 | F150 | fix | merged | 214 | none | 2026-10-13 |
| F150-R1 | a stop between the set of Clash Detective's Hide Other and its put back, Navisworks closed or killed in the picture pass, leaves the value set for the user, and whether Navisworks keeps it for the next start is UNKNOWN, the breaker's third point on attempt 1 | F150 | register row | open | none | none | 2026-10-12 |
| F145 | only the test's models shown for its pictures | F145 | fix | merged | 204 | none | 2026-10-12 |
| F149 | pictures taken in the report's order so none is renamed | F149 | fix | merged | 207 | none | 2026-10-12 |
| F149-R1 | since F149 every picture is taken once every test of the group has run, so the pictures' guard can stop a run only after the whole group's clash tests, where before it stopped after the test the fiftieth failure in a row fell in, a cost said in the rule that Bader has not weighed, the reviewer's first point on attempt 1 | F149 | register row | open | none | none | 2026-10-12 |
| F146 | the renumber answers while it renames | F146 | fix | merged | 205 | none | 2026-10-12 |
| Q156 | wrap up now, no more tests, the merges, the team zip for his PC, the final state and stop | none | Bader's request | merged | 206 | none | 2026-10-12 |
| Q157 | the fix round of waves 2 to 5 until 17:30, F150 after one API test, F147, a 1A02MM check after each wave, a new zip at the end | none | Bader's request | merged | 214 | none | 2026-10-13 |
| Q158 | a person's own view named like a tool view: skipped and named, the group stays DONE, Bader's answer B on F114-K32 | none | question | merged | 227 | none | 2026-10-13 |
| Q159 | only the teams named in the team map get view pair folders, a test outside the map or on UNKNOWN gets no view, nothing deleted, Bader's words on F114-K29 and the lead's reading | none | question | merged | 228 | none | 2026-10-13 |
| Q160 | a test whose open clashes are all result groups gets a view built from the clashes inside the groups, Bader's answer B on F114-K36 | none | question | merged | 229 | none | 2026-10-13 |
| Q161 | the put back of Hide Other left as it is, F150-R2 dropped, Bader's answer B | none | question | merged | 230 | none | 2026-10-13 |
| F150-R2 | the put back of Hide Other writes the value read, off, so the registry holds the set form 3 0 where it held 0, a value never set, before the first run on a PC, Clash Detective reading off either way, set 10, dropped by Bader's answer B of 2026-10-13, Q161 | F150 | register row | dropped | 230 | none | 2026-10-13 |
| F151 | no copy of the log beside the NWF folder, the picked NWFs or the open file, the log stays in the logs folder, Q162-1 | F151 | fix | merged | 232 | none | 2026-10-13 |
| Q162-1 | no log copy in the NWF folder, the log in the logs folder only and RESULT still written, F151 | none | Bader's request | merged | 232 | none | 2026-10-13 |
| Q162-2 | Excel pictures as linked pictures in the form of his by-hand client workbook, F152 | none | Bader's request | merged | 233 | none | 2026-10-13 |
| Q162-3 | no Excel rows for tests with no clash, the HTML and the Coverage sheet keep every test, F153 | none | Bader's request | merged | 234 | none | 2026-10-13 |
| Q162-4 | the priority and by design CSVs built in, the two picker rows gone, F154 | none | Bader's request | merged | 235 | none | 2026-10-13 |
| Q162-5 | the clash matrix built in as a pack, the tolerance from the drop down alone, F155 | none | Bader's request | merged | 236 | none | 2026-10-14 |
| F152 | the workbook's pictures as linked pictures in the form of the client's own file, the thumbnail box gone, Q162-2 | F152 | fix | merged | 233 | none | 2026-10-13 |
| Q163 | whether each linked picture also carries the client's click link to its jpg, F152 | none | question | merged | 244 | none | 2026-10-14 |
| F153 | a test with no clash writes nothing in the workbook, which retires Q73 b, the page and the Coverage sheet keep every test, Q162-3 | F153 | fix | merged | 234 | none | 2026-10-13 |
| F154 | the priority CSV and the by design CSV built into the matrix pack, read through the existing readers, the two picker rows gone, Q162-4 | F154 | fix | merged | 235 | none | 2026-10-13 |
| Q164 | how a run chooses its matrix pack once a second project's pack is built in, F154 and F155 | none | question | merged | 244 | none | 2026-10-14 |
| F155 | the clash matrix built into the matrix pack with its team map and list of corrections, read through ReadPack, the Clash XML row gone, the tolerance from the drop down alone at 25, 50 or 75 mm, Q162-5 | F155 | fix | merged | 236 | none | 2026-10-14 |
| Q165 | whether MatrixCorrections.ReadPicked and tools\loop\cut-xml.ps1 stay now that no run reads an XML from the disk, F155 | none | question | merged | 244 | none | 2026-10-14 |
| Q166 | whether the Run as label Weekly run plus XML is renamed now that every weekly run reads the built in matrix, F155 | none | question | merged | 244 | none | 2026-10-14 |
| Q167 | whether a test with no tolerance or a file with no units is still skipped now that the XML tolerance is never used, F155 | none | question | merged | 244 | none | 2026-10-14 |
| Q168 | whether a test saved in the NWF that the built in matrix does not name is run too, F155 | none | question | merged | 244 | none | 2026-10-14 |
| F156 | build\install.ps1 refused every build since F152, Federator.Core needs System.IO.Compression 4.2.0.0, read now after binding policy | F156 | fix | merged | 237 | none | 2026-10-14 |
| F157 | the picture pass leaves a trail for a run that dies, MEMORY with GDI and USER counts, the RATE, a stamped failure line with the whole exception, a warning for two clashes on one file | F157 | fix | merged | 239 | none | 2026-10-14 |
| F158 | the logs and the reports in one Log folder beside the NWF and NWD folders, the Excel folder box gone, nothing in Log overwritten or deleted | F158 | fix | merged | 240 | none | 2026-10-14 |
| Q169 | whether the Log folder is at the deepest folder holding the NWF and NWD folders, or one Log folder for each group, F158 | none | question | waiting for Bader | none | none | 2026-10-14 |
| F159 | the option that skipped the clash for models off the shared coordinates removed, the clash always runs, a NOTICE in the log and in the list names the models | F159 | fix | merged | 241 | none | 2026-10-14 |
| Q170 | whether a group with a model on Internal or naming no site still ends FAILED now that its clash always runs, F159 | none | question | waiting for Bader | none | none | 2026-10-14 |
| F161 | the picture pass names where its GDI objects go, render or save, and warns of one file named twice only where a picture was written, after run 20261014-105308 | F161 | fix | merged | 242 | none | 2026-10-14 |
| F160 | the run on C04 under NM fed with the build of F157 to F159, its crash in lcodwin.dll at picture 8907 of 10170 at 9999 GDI objects, the evidence and the report | F160 | fix | merged | 243 | none | 2026-10-14 |
| Q171 | whether the picture step stops at 9000 GDI objects and lets the run finish, or all pictures in several sessions, F160 | none | question | waiting for Bader | none | none | 2026-10-14 |
| F163 | tools\loop and tools\probes\drive-window-run.ps1 still type into the Excel folder box and tick the skip box, both gone with F158 and F159 | F163 | fix | open | none | none | 2026-10-14 |
| F164 | the Run as label Weekly run plus XML renamed Weekly run plus matrix in the window, the confirm and the log, Q166 A | F164 | fix | merged | 244 | none | 2026-10-14 |
| F165 | MatrixCorrections.ReadPicked and cut-xml.ps1 kept for the coming clash test round, the mirror check recorded in 01_next, Q165 | F165 | fix | merged | 245 | none | 2026-10-14 |
| F166 | the matrix pack chosen by the project code of the NWC file names, a run with no pack, two packs or mixed codes stopped before anything is opened, Q164 B | F166 | fix | merged | 246 | none | 2026-10-14 |

## outside the waves

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| F125 | a window that is not modal is a pane and not a dialog | F125 | fix | merged | 102 | none | 2026-10-06 |
| F126 | the window driver unticks a tick box by its AutomationId | F126 | fix | merged | 112 | none | 2026-10-05 |

## right after F133 merges

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| F139 | the one page progress file | F139 | fix | merged | 130 | none | 2026-10-07 |
| FR-197 | one page progress file that keeps itself current | F139 | Bader's decision | merged | 130 | none | 2026-10-07 |
| Q139 | a one page progress file that keeps itself current (FR-197) | F139 | Bader's request | merged | 130 | none | 2026-10-07 |
| F139-R2 | make-tracker.ps1 writing new counts moves the page's time, which the Stop gate reads as a rewrite until the next fetch | F139 | register row | merged | 130 | none | 2026-10-07 |
| F139-R3 | a checkout that writes the page reads to the Stop gate as a rewrite | F139 | register row | merged | 130 | none | 2026-10-07 |
| F139-R5 | the needs Bader rows of steps 228-233, 346-352 and 364 have no tracker row and no question, the lead's call | F139 | register row | merged | 130 | none | 2026-10-07 |
