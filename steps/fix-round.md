# Summary for Bader

Written when the round ends.

# Fix round, turn 5

The fix list of Bader's full fix round, Q98, written on 2026-10-04 before the first fix, from
the returns of five readers: the findings of set 03, the confirmed bugs of turn 1 in two halves,
the register in steps\loop.md, and the readings of the fixes in flight. Their 196 raw items
come to 174 here. 22 were the same fault as another item and were folded into it, with every
source id kept on the item that stayed.

How it is ordered. By area: alignment, sets, clash counts, workbook, report, run log and
RESULT, views, harvest and pictures, install, loop tools, docs and words, rest. Inside an area
by class: silent wrong number, broken feature, slow, loud failure, noise. FR-001 is B2 of Q98,
the far model rule.

What each item holds. The heading gives its number and the key the readers used. Then the
line of what is wrong, its Sources, the Evidence, the Root cause, its Class and the Proof that
shows it fixed. Needs Bader was written only where a choice of his came first, 12 items, all
answered on 2026-10-04, each answer on its item.
Branch is written where the item rests on code merged from a branch, waits for a merge not yet
made, or cites lines that moved when a branch merged. Note carries what the readers added and
where the item meets others.

What main holds now, read with git on 2026-10-04.
- F108 merged into main as c9b223b (PR 84) and F106 as 3449521 (PR 88), both on 2026-10-04.
  F104, F105, F107 and F109 were not merged at 3449521. Since then PR 89, the records with the
  note for the modellers, merged as 51a0cb6, PR 83, the rules, as 086a348, and PR 90, F105, as
  2c89788, all on 2026-10-04. FR-125, FR-137 and FR-138 are closed by them and FR-139 in part,
  each saying so.
- src, tests, build, exchange, docs, CLAUDE.md, INSTALL.md, .claude and
  steps\03_bader_next.md read the same at 3449521 as at f38edd5, so every line cited in them
  still reads where it is cited.
- F106 rewrote tools\loop\run.ps1 and changed tools\loop\nw-guard.ps1 and
  tools\probes\drive-window-run.ps1. A line a reader cited in those three at f38edd5 has
  moved, and the item gives the function and its line on main where it was looked up. The
  lines readers cited at fix-F106 32dce24 read the same on main.
- F108 changed tools\loop\prepare-copy.ps1 and tools\loop\README.md. The lines cited at
  fix-F108 read the same on main.
- "rests on F106, merged as 3449521, lines unchanged" means the item is about F106's code and
  its cited lines read the same on main. The same words with F108 and c9b223b mean the same
  for F108.
- A profile folder name is written <profile> or <name> in this draft, because whether it is a
  trace to keep out of committed files is FR-109 and FR-149, which are Bader's.

Key to the source ids.
- S03-n is finding n of steps\runs\03\findings.md. C06-Jn is the nth finding in array order of
  turn4\c06-find.json, 30 in all. C06-DONE-n is rank n of the findings in turn4\c06-done.json,
  21 in all. FIND-nn and the F, A, B, L, P, step and LOOK ids are rows of
  turn4\c06-register.json, 218 rows, or of the register in steps\loop.md, its line given where
  a reader gave one. Register row B2 is not Q98 B2.
- T1-S, T1-B and T1-L ids are the confirmed bugs of the turn 1 read (turn4\c06-rows-a.json and
  steps\loop-read.md). T1-N, T1-UNCALLED and T1-CATCH are the three lists in steps\loop-read.md.
  T3-G, T3-P, T3-W and T3-B are register rows from the readings of F103.
- Q98 B1 to B5 are Bader's decisions at steps\02_questions.md:504.
- F106-HARM-REV-n and F106-HARM-BRK-n are the nth entries of the lists of the F106 harm
  reviewer ab4bf4ea39f7d1669 and breaker a2c8cbb853c41abcb, journal wf_8100973e-871.
  F108-REV-n and F108-BRK-n are the nth findings of a7c3aa7fb3982742a and a0c1e9264e4a0aaed,
  F105-REV-n and F105-BRK-n of aa1682651519f441d and a233e87838c44af6f, F107-REV-n and
  F107-BRK-n of a2eb40a6e1ac0f019 and aa9d7aa0e66b15452, in the order of their result lines.
  R1 to R4 of F106 are the finishing developer's, turn4\f106-finish-report.md.
- F108-R1 to F108-R5, F107-R1 and F107-R2 are the register rows PR 83 adds,
  turn5\pr83-diff-rest.txt lines 111 to 117. F103-W is the register row of the nine word
  entries, F103-W-1 to F103-W-9 are the entries of option A in
  steps\notes\f103-final-reading.md, and F103-W-wrap the three over long README lines it names.
- turn4 and turn5 are folders under %LOCALAPPDATA%\NwcFederatorLoop. log:N is line N of
  steps\runs\03\item1-C06\run-20261001-140037.log, and tsv:N, record.txt, driver.txt,
  watch.txt, outputs.txt and workbooks\ are files of the same folder.

## How the round works it

One pull request per area, each finding its own commit with the test that fails before it and
passes after, read by a reviewer and a breaker, its body by the claim-checker, and merged one at
a time once Actions is green, Q98. Up to three developers at once, on different files. An area
takes the F number The waves give it, and its section and DONE line in
steps\01_next.md say which FR items it closed. The lanes and the order are at the end of this
file, under Areas that can be worked at the same time.

- the product lane: alignment first with FR-001, then sets, clash counts, workbook, report, run
  log and RESULT, views, harvest and pictures and rest, as their files allow
- the loop tools lane: install with F109, then the loop tools, FR-092 first
- the words lane: the items that touch no code

Q93 holds in the loop tools lane: a reading of the loop's own scripts blocks only for a fault
that could harm Bader's things or make a run's evidence wrong, and the rest of that lane's items
are register rows, fixed where their file next changes. The product code keeps every house rule.

The 12 items that need Bader wait for the form, Q99 to Q109 in steps\02_questions.md, with Q24
and Q26, asked before. He answered them all on 2026-10-04 at 15:23, each answer on its item.
FR-001 is built with the 1 m he gave and its number is
Q99. Nothing of an item that needed him changed before he answered.

## The waves, Bader's order of 2026-10-04 at 15:42

Bader does not wait for all 174 items before a run. The fixes go in waves, each tested at once
on two buildings, 1A02MM of C02 and 1A04PK of C04, a first run with the XML and a weekly run,
against the baseline of set 04 and the wave before. The waves follow his wished order by area,
adjusted by the file table at the end of this list, so the silent wrong numbers sit in waves 1
to 4 with the areas that hold them. Inside that, every noise item goes to wave 5 with the docs
and words and D1, bar three kept early and named below with why. The lead departed from his
wished order in two places: F115 sets, 16 items his list did not name, in wave 2, and workbook
and report one area because they share WorkbookCheck.cs. FR-136, Q108's fix, and F117, the names,
moved from the waves 3 and 4 the lead first told him to wave 5, since both are noise by the class
this list gives them and noise comes last in his order. Four pairs in one wave or half
share a file and merge one after the other: in wave 1 F112 and F116 share ExportCheck.cs, F116
after F112, in 2b F128 and F118 the workbook writer, in 3b F130 and F119
FederatorWindow.xaml.cs, and in wave 5 F123 and F117 docs\history\scan.md. The four finds of Q108, Q109, Q24
and Q26 write their test steps under their items and run them at the start of the wave that
holds them. Each area takes the F number shown.

- Wave 1:
  - F112 alignment: FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-028. Noise kept here: FR-028, started by F112 before the waves
  - F113 clash counts: FR-031, FR-032, FR-033, FR-034
  - F116 the clash XML: FR-008, FR-009, FR-025, FR-026, FR-030. Noise kept here: FR-030, Bader put the XML corrections of Q102 to Q104 in wave 1
- Wave 2, in two halves since Bader's five requests, Q112, each half at most three areas:
  - 2a, F127 coverage first, Bader's request 2: FR-176
  - 2a, F115 sets: FR-010, FR-011, FR-012, FR-013, FR-014, FR-015, FR-016, FR-017, FR-018, FR-019, FR-020, FR-021, FR-022, FR-023, FR-024, FR-027
  - 2a, F114 views: FR-065, FR-066, FR-067, FR-068, FR-069, FR-070, FR-071, FR-072, FR-073. Noise kept here: FR-073, committed by F114 before it paused
  - 2b, F128 generic models, Bader's request 3: FR-177
  - 2b, F118 workbook and report: FR-035, FR-036, FR-037, FR-040, FR-041. F128 and F118 both add to the workbook writer, so they merge one after the other
- Wave 3, in two halves the same way:
  - 3a, F129 start from an existing NWF, Bader's request 4: FR-178
  - 3a, F120 harvest and pictures: FR-075, FR-076, FR-077
  - 3a, F109 install: FR-078, FR-079, FR-080
  - 3b, F130 the Shift range in the group list, Bader's request 5: FR-179. In the half after F129 because both change the window's files
  - 3b, F119 run log and RESULT: FR-043, FR-044, FR-045, FR-046, FR-047, FR-048, FR-049, FR-050, FR-051, FR-052, FR-053, FR-054, FR-055, FR-056, FR-057
- Wave 4:
  - F121 the rest: FR-150, FR-151, FR-152, FR-153, FR-154, FR-155, FR-156, FR-157, FR-158, FR-159, FR-160, FR-161, FR-162, FR-163, FR-164, FR-165, FR-166. The find of Q24 on FR-160 and Q109 on FR-161 first
  - F122 the loop tools: FR-082, FR-083, FR-084, FR-085, FR-086, FR-087, FR-088, FR-089, FR-090, FR-091, FR-092, FR-093, FR-094, FR-095, FR-096, FR-097, FR-098, FR-099, FR-100, FR-101, FR-102, FR-103, FR-104
- Wave 5:
  - F123 docs and words, and the noise of every area: FR-007, FR-029, FR-038, FR-039, FR-042, FR-058, FR-059, FR-060, FR-061, FR-062, FR-063, FR-064, FR-074, FR-081, FR-105, FR-106, FR-107, FR-108, FR-111, FR-112, FR-113, FR-114, FR-115, FR-116, FR-117, FR-118, FR-119, FR-120, FR-121, FR-122, FR-123, FR-124, FR-126, FR-127, FR-128, FR-129, FR-130, FR-131, FR-132, FR-133, FR-134, FR-135, FR-136, FR-139, FR-140, FR-141, FR-142, FR-143, FR-144, FR-145, FR-146, FR-147, FR-148, FR-167, FR-168, FR-169, FR-170, FR-171, FR-172, FR-173. Closed in part, the part that stays: FR-139, FR-146. The finds of Q108 on FR-136 and Q26 on FR-172 first. Closed already: FR-125, FR-137, FR-138
  - F117 the names, by Bader's answer to Q105 to Q107: FR-109, FR-110, FR-149
  - then F124 D1, one public type per file: FR-174, moves only, last

Bader's five requests of the evening of 2026-10-04, Q112, are added to the round as FR-175 to
FR-179, under their own heading below, each measured before it is written, test first, and
tested on 1A02MM and 1A04PK like the fixes. FR-175, no sleep, was done by the lead at once. Two
tools of the loop sit outside the product waves: F125, the window driver leaving a floating pane
alone, and F126, the driver unticking a named box, which the run of a building off its shared
coordinates with the rule switched off needs in the test of wave 1.

## Alignment, FR-001 to FR-007

### FR-001 far-model-group-stays-done

A group with a model hundreds or thousands of kilometres from its reference model still ends
DONE, so its clash count reads as whole while that model's clashes with the other disciplines
are never found.

- Sources: S03-1, C06-J1, C06-DONE-1, Q98 B2
- Evidence: log:1834 'ST 1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc DIFFERENT dx -658144882.33 mm
  dy -2746014844.6 mm dz -683828.41 mm, shared coordinate COMMUNITY 4A' then log:2128 'GROUP
  finished 1B06K1 DONE'. The same for 1B06P1 ST (log:2849, DONE at log:3157), 1B06PH EL
  (log:3908, DONE at log:4216) and 1B06PP AR-MOD-000002 (log:5044, DONE at log:5442). log:8358
  'ALIGNMENT across the run: 40 model(s) sit somewhere their group's reference model does
  not'. log:4282, 1B06PK ST is 188.8 m from the ME reference and the group reads DONE at
  log:4579.
- Root cause: src\Federator.Core\Health\AlignmentCheck.cs:243-287, WhyItFailsTheGroup fails a
  group only for a model named Internal or naming no site, :190-195 a DIFFERENT model is one
  log line, and :290-323 DifferentCount only counts. src\Federator.Addin\Engine\FederationEngine.cs:2090-2097
  hands only that reason to AddError, and :2112-2113 the difference only adds to a run count.
  src\Federator.Core\Rerun\GroupJudgement.cs:142-232 has no path to PARTIAL for a far model.
- Class: silent wrong number
- Proof: New Core tests that fail before and pass after. In
  tests\Federator.Core.Tests\Health\AlignmentCheckTests.cs a model more than the Core setting
  of 1 m from the reference gives one line naming the model, its distance and that its clashes
  with the other disciplines cannot be trusted, and a model at 0.9 m gives none. In
  tests\Federator.Core.Tests\Rerun\GroupJudgementTests.cs facts carrying a far model judge
  PARTIAL with that reason, and a group with an Internal model still judges FAILED. Run line of
  set 05: GROUP finished 1B06K1, 1B06P1, 1B06PH and 1B06PP read PARTIAL with the reason, where
  set 03 read DONE, and so does every other group the rule catches.
- Note: Decided in Q98 B2: more than 1 m from the reference model as the ALIGNMENT block
  measures it, PARTIAL, one line, the 1 m a setting in Core. The 40 DIFFERENT lines run from
  log:195 to log:7614. By straight line distance 35 of the 40 offsets are over 1 m and 5 are
  not (1B06K1 ME 21 mm, 1B06M1 ST and 1C06M2 ST 72.5 mm, 1B06WO EL 684 mm and ST 200 mm), and
  8 are over 300 km. 1B06WM ME is 1.2 m in a straight line but under 1 m on each axis, so the
  rule must say which it measures. Each of the 11 DONE groups that hold more than one model has
  a model over 1 m, so all 11 would end PARTIAL and not four, which is the number Bader
  confirms from the 40 distances in the form. The far model line is also written for the
  FAILED groups that hold one (1B06BC EL 2,774 km, 1B06PS EL 336 km and ST 2,823 km), because
  their FAILED reason names only the Internal models. GroupFacts in GroupJudgement.cs is also
  changed by FR-043 to FR-045 and AlignmentCheck.cs by FR-002, so those go with this one or
  after it.
- Asked: the 1 m itself, Q99 in steps\02_questions.md, with what each number does to the
  groups of set 03.
- Answered by Bader on 2026-10-04 at 15:23, Q99 and Q100, in short, his words being under the question in steps\02_questions.md: a model is not on the same coordinates when it names Internal as its shared site, or when it sits more than 1 m from the reference model, measured as the straight line, the 1 m a setting in Core, and in a group with such a model ONLY THE CLASH IS SKIPPED. The NWF is built with all its models, sets and tests and the NWD is published, no clash test is run and no viewpoint and no clash report is made, the log has one line per such model with its file name, its shared site name and its distance from the reference in X, Y and Z, a short note file with the same lines goes beside the NWD and into the group's Clash Report folder, the group ends PARTIAL, never DONE, with the reason clash skipped, models not on the same shared coordinates, the RESULT block lists these groups, the run writes one short list he can forward to the modellers, the rule is checked on every run, and it replaces Q65 and Q70 for this case. The lead's note: to be worked by F112 in wave 1, the PARTIAL only rule above giving way to it, and the rule switched off by its setting for a second test run of a building whose models are off the shared coordinates.

### FR-002 model-site-read-failure-reads-as-no-site

When reading a model's shared site throws, the catch turns it into an empty site, which the
alignment rule reads as the model naming no shared site at all, so the group is FAILED with a
reason that reads as a fact about the model and nothing in the log says the read threw.

- Sources: T1-L7
- Evidence: src\Federator.Addin\Engine\ModelFactsReader.cs:152-155 'catch (Exception)' sets
  site = string.Empty with no log line. src\Federator.Core\Health\AlignmentCheck.cs:55-58
  NamesASharedCoordinate is SharedCoordinate.Length > 0, :255-258 WhyItFailsTheGroup puts that
  model in withNoSite and :282 writes 'model(s) name no shared site at all'. ModelPlacement has
  NotRead only for X, Y and Z (:14). src\Federator.Addin\Engine\FederationEngine.cs:2090-2097
  runs outcome.AddError(fails) and counts it in failedOnAlignment. ModelFactsReader.cs:27-29
  says NOTHING HERE FAILS A GROUP and a count that could not be taken comes back as NotCounted.
  Set 03: 67 model placement rows and none has an empty site, and the five FAILED groups name
  the real site Internal (log:583, 593), so those reasons came from a real read.
- Root cause: src\Federator.Addin\Engine\ModelFactsReader.cs:152-155 turns a failed read into
  an empty site, and src\Federator.Core\Health\AlignmentCheck.cs:55-58 and :255-258 read an
  empty site as no site.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Health\AlignmentCheckTests.cs: a
  ModelPlacement carrying a site that was not read (a NotRead state for the site, like NotRead
  for X, Y and Z) does not appear in WhyItFailsTheGroup as having no site, and the block says
  UNKNOWN for it. Fails today because the state does not exist. The add-in catch, a log line
  and the state passed on, is proved by review and a run.
- Note: Whether RootItem or the Location tab ever throws on a real model is UNKNOWN. It changes
  AlignmentCheck.cs, the file of FR-001, so it goes in the same pull request or after it.

### FR-003 export-check-says-every-element-without-counting

The EXPORT CHECK ends 'all N model(s) carry a workset on every element and an element id on
every element' when its only workset test is whether any element carries one, so a model with
one element on a workset, a model whose count failed and a model with no Revit element all
pass, and the sets that filter on workset find nothing for the rest without a warning. The
worksets figure it prints counts names, not elements.

- Sources: T1-S50 (placed by the findings reader and by a bug reader, folded here)
- Evidence: log:205 'AR 1104-PAR-100000-ZZZ-AR-MOD-003000.nwc elements 53 worksets 1 element id
  100%' then log:208 'all 3 model(s) carry a workset on every element and an element id on
  every element', written in all 22 groups. 'worksets 1' counts names (log:209). WithWorkset is
  never printed. The tsv holds 67 model export rows with no -1 and no 0 element count (tsv:57),
  so none of the failing shapes occurred in this run, and the run shows the unsupported
  sentence and not the harm.
- Root cause: src\Federator.Core\Health\ExportCheck.cs:51-54 (CarriesAWorkset is WithWorkset
  greater than 0, the only workset test), :130-141 (the two counters, the tests at :130 and
  :136) and :238-243 (the sentence, written at :240-242). src\Federator.Addin\Engine\FederationEngine.cs:2147
  counts the same way.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Health\ExportCheckTests.cs: a model with 1 of
  1000 elements on a workset, and a model whose walk threw, are not called whole, and the line
  carries how many elements hold a workset. Fails before. Run line of set 05: the EXPORT CHECK
  block prints the elements with a workset per model, and the closing sentence is written only
  when it holds (set 03 log:205 and 208).
- Note: Confirmed by reading in steps\notes\turn1-read-verified.md:399. The same sentence hides
  a 99.5 percent id share, which is FR-004. FR-004, FR-005 and FR-007 are the same family, and
  FR-028 changes ExportCheck.cs too.

### FR-004 export-check-id-share-rounded-to-100

A model with 99.5 per cent or more of its elements carrying an id reads as 100, so no
re-export line is written, its blank id cells are not counted, and the closing sentence says
every element carries an id.

- Sources: T1-S48
- Evidence: src\Federator.Core\Health\ExportCheck.cs:66 'return (int)Math.Round(100.0 *
  WithElementId / Elements, MidpointRounding.AwayFromZero)', :136 'if (model.IdShare !=
  ModelExport.NotCounted && model.IdShare < 100)', :242 'carry a workset on every element and
  an element id on every element'. src\Federator.Addin\Engine\FederationEngine.cs:2152 has the
  same test feeding modelsMissingAnId (:2154, :2190). 1051 of 1052 is 99.9 and reads 100.
  Set 03: all 67 model lines read 100%, and whether the rounding hid any is UNKNOWN.
- Root cause: src\Federator.Core\Health\ExportCheck.cs:66 (rounding before the test at :136)
  and src\Federator.Addin\Engine\FederationEngine.cs:2152.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Health\ExportCheckTests.cs, which covers 100
  and 76 per cent and nothing between 99.5 and 100: a ModelExport with 1052 elements and 1051
  ids gets the re-export line naming 1 element, and the closing sentence does not say every
  element carries an id. Fails today. The test at FederationEngine.cs:2152 reads the same Core
  rule so the run line cannot disagree.
- Note: Up to 50 blank id cells pass as none on a 10000 element model. The share can stay a
  whole per cent for display, and the test uses the counts.

### FR-005 export-check-worksets-none-when-not-counted

A model whose walk threw prints 'worksets NONE' beside 'elements UNKNOWN', which states a
workset count nobody took, and the partly gathered names are still listed under worksets seen.

- Sources: T1-S49
- Evidence: src\Federator.Core\Health\ExportCheck.cs:127 writes '   worksets ' and then the
  count when model.CarriesAWorkset, or "NONE", and :53 'get { return WithWorkset > 0 }', so
  the not counted value minus one reads as none. src\Federator.Addin\Engine\ModelFactsReader.cs:209
  returns all three counts as NotCounted when the walk throws and still hands on the partly
  gathered worksets list, which :143 and :230 then list. ExportCheck.cs:13 says a count that
  could not be taken is never zero. Set 03: the 67 model export rows hold no minus one and no
  'elements UNKNOWN' line exists.
- Root cause: src\Federator.Core\Health\ExportCheck.cs:127, with CarriesAWorkset at :51-54, fed
  by src\Federator.Addin\Engine\ModelFactsReader.cs:209.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Health\ExportCheckTests.cs (:103-115 asserts
  'elements UNKNOWN' and 'element id UNKNOWN' and never the worksets cell): a ModelExport with
  all three counts NotCounted prints 'worksets UNKNOWN', and its partial names are not offered
  as seen. Fails today.
- Note: Rare, it needs the walk to throw.

### FR-006 failed-by-site-name-while-placement-agrees

1B06M1 and 1C06M2 end FAILED on a model named Internal that sits 72.5 mm from the reference,
so the group fails on the name alone while the measured placement agrees.

- Sources: C06-DONE-10, S03-11
- Evidence: log:2489 and log:7614 'ST ... DIFFERENT dx 0 mm dy 0 mm dz 72.5 mm, shared
  coordinate Internal', and log:2774 and log:7899 'GROUP finished ... FAILED ... puts them in a
  different coordinate system from the rest of the group'. log:5511 and log:583 name the
  reference AR as the Internal model in 1B06PS and 1B06BC, while the displaced models are EL
  and ST.
- Root cause: src\Federator.Core\Health\AlignmentCheck.cs:243-287, the compare at :259 reads
  the site name only. Q70 was answered b on 2026-09-20, the rule goes by name. Whether the ST
  and the ME reference both sit at the origin by chance is UNKNOWN.
- Class: loud failure
- Proof: Only if Bader changes Q70. Then a Core test in
  tests\Federator.Core.Tests\Health\AlignmentCheckTests.cs: an Internal model within the
  tolerance of the reference does not fail the group. Otherwise none, and the note for the
  modellers (FR-125) says it.
- Needed Bader, asked as Q100, answered on 2026-10-04, see the answered line of this item. Raised for the form beside the 1 m number of FR-001, since the two rules
  meet in the same block. Nothing was changed before he answered on 2026-10-04.
- Answered by Bader on 2026-10-04 at 15:23, Q100 with Q99: a model naming Internal as its shared site is not on the same coordinates, so its group's clash is skipped and the group ends PARTIAL, replacing Q70 for this case. The lead's note: to be worked by F112 with FR-001, in wave 1.

### FR-007 invisible-difference-fallback-for-visible-space

Two workset names that differ by an ordinary space are described as 'THEY LOOK IDENTICAL AND
ARE NOT: they differ only in invisible characters', and a double, leading or trailing ordinary
space is named by neither the character nor where it sits.

- Sources: T1-S51
- Evidence: src\Federator.Core\Health\InvisibleDifference.cs:158 strips ordinary spaces as well
  as invisible ones (the test reads not IsInvisible(one) and one is not a space), :100-105
  IsInvisible has no ordinary space so FirstInvisibleIn (:131-150) returns null, and :91
  'return extra ?? "they differ only in invisible characters"'.
  src\Federator.Core\Health\WorksetDisagreements.cs:50-51 'THEY LOOK IDENTICAL AND ARE NOT: '.
  The class doc at :7-8 promises the character and where it sits.
  tests\Federator.Core.Tests\Health\InvisibleDifferenceTests.cs:53 and :59 only assert
  Is.Not.Null. Set 03 wrote no such line.
- Root cause: src\Federator.Core\Health\InvisibleDifference.cs:158, with the fallback text at
  :91.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Health\InvisibleDifferenceTests.cs:
  Between("EL-Fire alarm", "EL-Firealarm") does not return the invisible fallback, and Between
  with a double space names the extra ordinary space and its position. Fails today.
- Note: Wrong words on a rare pair that is still listed with both names.

## Sets, FR-008 to FR-030

### FR-008 matrix-workset-spelling-differs-between-c06-buildings

No single case sensitive workset spelling in the matrix suits every C06 building. 1B06BC
carries ME-DUCTWORK and ME-EQUIPMENT in capitals (log:605) and 1B06G1 ME-DUCTWORK, ME-PIPING and
ME-EQUIPMENT (log:1354), while 1B06M1, 1B06P1, 1B06PK and 1C06M2 carry ME-Ductwork (log:2502,
2864, 4293 and 7627), and seven groups list only some of their worksets. So the corrected matrix
fixes the groups the findings name and can stop the sets that found items in 1B06BC and 1B06G1,
the second 1511 clashes, the most tests that found something.

- Sources: S03-2, C06-J2, FIND-04, Q98 B1
- Evidence: log:605, 1B06BC 'worksets seen: ... ME-DUCTWORK, ME-EQUIPMENT', and log:639
  'BLD-ME-Ducts&Duct Fittings 4 conditions 13 items'. log:1354, 1B06G1 'ME-DUCTWORK,
  ME-PIPING, PL-Domestic Water, ... FP-PIPING, ... ME-EQUIPMENT', and log:1388 '134 items',
  with 113 of its 1511 clashes in the ten tests at log:1544 to 1600. log:4293, 1B06PK
  'ME-Ductwork, PL-Drainage, FF-Fire Fighting, PL-Domestic Water', and log:4327 '0 items'.
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml:27929-28205 asks ME-Ductwork, ME-Equipment
  and ME-Piping, :28236 and :28374 still ask FF-FIRE FIGHTING, :28265-28403 ask FP-PIPING, and
  :28434-28572 ask 'PL-Domestic water' where the C06 models carry 'PL-Domestic Water', for
  which the copy's XML found 361 items (log:1406).
- Root cause: Not code. exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml:27929-28572 holds one
  spelling per workset with test equals, and the compare is case sensitive. Bader refused an
  ignore case flag on 2026-09-20 (src\Federator.Core\Exchange\revit-worksets.txt:17-21). The
  measured list there came from the C02 folder, whose models write 'PL-Domestic water'.
- Class: silent wrong number
- Proof: No Core test until Bader chose, which he did on 2026-10-04, Q102. Now,
  tests\Federator.Core.Tests\Exchange\AllInOneFileTests.cs can assert the spellings the matrix
  asks against the spellings the C06 models carry. Run line of set 05: an EMPTY SETS block per
  group (FR-027), and the SET line for BLD-ME-Ducts&Duct Fittings reads more than 0 items in
  1B06BC (13 in set 03, log:639), in 1B06G1 (134, log:1388) and in 1B06PK (0 in set 03,
  log:4327).
- Needed Bader, asked as Q102, answered on 2026-10-04, see the answered line of this item. The way out is his: the matrix asking both spellings as an OR, the models
  exported again with one spelling, or something else. B1 as written only says the proof copy
  must ask ME-Ductwork, and that moves the loss from seven groups to two and from ME to PL, FF
  and FP.
- Note: The tool can only say which sets are affected, FR-027 and FR-028. The copy's XML is
  FR-082. An OR made through MatrixCorrections would meet FR-025.
- Answered by Bader on 2026-10-04 at 15:23, Q102, in short, his words being under the question in steps\02_questions.md: look for both spellings of each workset the buildings spell differently, as an OR row built the way Q69's rows are, and the export check keeps naming which models carry which spelling. The lead's note: built with the set's other conditions carried into the OR group, FR-025, as written under Q102, and to be worked by F116, the clash XML, in wave 1.

### FR-009 matrix-ar-sets-match-items-of-other-disciplines

In 1B06PK, which holds only ME and ST models, 414 of its 1629 clashes name the AR Ramps set,
because that set asks Category equals Ramps with no Source File condition and so finds the
structure model's ramps. The AR Railings set does the same.

- Sources: S03-3, FIND-24
- Evidence: log:4280 'ME ... is the reference, because this group carries no AR model'.
  log:4304 'SET ok BLD-AR-Ramps 1 condition 36 items'. log:4417 'clashes
  BLD-ST-Columns-vs-BLD-AR-Ramps items 325 v 36 New 248', log:4418 114 and log:4422 52, lines
  9, 11 and 12 of the 1B06PK read-out. log:4316 'BLD-AR-Railings 1 condition 18 items' and
  log:4425 'BLD-ST-Stair-vs-BLD-AR-Railings New 25'. log:4302, 4303 and 4308: BLD-AR-Floors,
  Stairs and Walls also carry Source File contains -AR-.
- Root cause: Not code. The matrix, exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml:27510-27527,
  where BLD-AR-Ramps has one Category condition. The tool counts a set's items across all models
  and never says which model they live in: src\Federator.Addin\Engine\SetBuilder.cs:753
  (Resolve) and :756-758 (AddCreated). Whether it is wrong is UNKNOWN, FIND-24 says so.
- Class: silent wrong number
- Proof: No Core test until Bader decided, which he did on 2026-10-04, Q103, so F116 adds the Source File condition, and
  tests\Federator.Core.Tests\Exchange\AllInOneFileTests.cs can assert it for every AR set whose
  category another discipline also uses. Run line of set 05: the 1B06PK read-out has no clash
  block for BLD-AR-Ramps (set 03 lines 9, 11 and 12), and the SET line for BLD-AR-Ramps in
  1B06PK reads 0 items (36 at set 03 log:4304).
- Needed Bader, asked as Q103, answered on 2026-10-04, see the answered line of this item. The matrix is his (Q98 B1).
- Note: A SET line could also say which discipline's model an AR coded set found its items in.
  The tool reports and never acts, so no behaviour change is asked.
- Answered by Bader on 2026-10-04 at 15:23, Q103, in short, his words being under the question in steps\02_questions.md: every AR set whose category another discipline also uses gets Source File contains -AR-. The lead's note: to be worked by F116, the clash XML, in wave 1.
- Answered by Bader on 2026-10-04 in the evening, Q113 D: all four categories, Ramps, Railings,
  Furniture and Site. An AR set only takes items from an AR file, and a set this leaves empty in a
  landscape group shows on the coverage sheet of wave 2

### FR-010 empty-sets-contains-judged-as-equals

EmptySets.Why compares a set's condition value with the measured category list by exact
equality whatever the condition's test is. A set asking 'contains Cable Tray', 'contains
Conduit' or 'contains Devices' that finds nothing in a group is reported as asking for a value
no model in this project carries, although the models carry Cable Trays, Conduits and the five
Devices categories.

- Sources: T1-S62 (a second fault found while reading it, marked NEW by its reader)
- Evidence: src\Federator.Core\Sets\EmptySets.cs:110-142, Why calls Holds(known,
  asked[i].Value) (:263-274, string.Equals Ordinal) and never reads ReadCondition.Test, and a
  grep of EmptySets.cs for .Test finds nothing. The corrected matrix has three contains
  conditions on the category property: Cable Tray
  (exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml:28754-28764), Conduit (:28808-28818) and
  Devices (:28826-28836). src\Federator.Core\Exchange\revit-categories.txt holds no line equal
  to Cable Tray, Conduit or Devices, only Cable Trays (:41), Conduits (:48) and the five
  Devices categories. src\Federator.Core\Health\SetWarnings.cs:248-264 (Known) already handles
  contains by looking for the stem inside each category.
  tests\Federator.Core.Tests\Sets\EmptySetsTests.cs has one contains test, on Source File
  (:77).
- Root cause: src\Federator.Core\Sets\EmptySets.cs:110-142 and :263-274, which ignore
  ReadCondition.Test (src\Federator.Core\Sets\SetDrift.cs:25).
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\EmptySetsTests.cs: Why on a
  ReadCondition(LcRevitData_Element, LcRevitPropertyElementCategory, contains, Cable Tray)
  returns TheValueIsThereAnyway and not NoModelCarriesTheValue. Fails today. Run: a rerun with
  the XML picked, in a group with none of those categories (1B06PK holds ME and ST only), does
  not print the NO MODEL IN THIS PROJECT CARRIES wording for BLD-EL-Cable Trays, BLD-EL-Conduit
  or BLD-EL-Devices.
- Note: It fires on any run that picks the XML over NWFs whose sets are already there. UNKNOWN
  whether it was ever seen, set 03 created every set and wrote no EMPTY SETS block. The loop's
  weekly runs 2 to 4 pick no XML (steps\notes\f103-design.md:108-110), so the SETS step is not
  entered (src\Federator.Core\Clash\ClashWork.cs:34-46), and a run proof needs the XML picked
  on a rerun. FR-027 puts created sets through the same judge, so after it a first run reaches
  this too.

### FR-011 empty-sets-names-and-lists-typed-in-core

EmptySets types two property internal names into Core, the category name a second time since
HealthCheck holds it too, and judges a set against word lists measured off this one project's
models, so another project's XML would be told that no model carries values its models do
carry.

- Sources: T1-S62
- Evidence: src\Federator.Core\Sets\EmptySets.cs:100 CategoryProperty
  (LcRevitPropertyElementCategory) and :103 WorksetProperty (lcldrevit_parameter_-1002053).
  src\Federator.Core\Health\HealthCheck.cs:129 CategoryPropertyInternalName holds the same name
  again. KnownFor at :248-258 returns RevitCategories.All() and RevitWorksets.All(), built into
  the DLL from this project's models (RevitWorksets.cs:9 'Every workset name a Revit model in
  this project really carries'). EmptySets.cs:170 writes 'NO MODEL IN THIS PROJECT CARRIES'.
  Set 03 did not reach it, the first run creates every set so the judge never ran.
- Root cause: src\Federator.Core\Sets\EmptySets.cs:100, :103 and :248-258 (KnownFor), with
  src\Federator.Core\Health\HealthCheck.cs:129.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\EmptySetsTests.cs: a value absent from
  the embedded lists is judged CannotTell and not NoModelCarriesTheValue unless the lists are
  known to belong to the run's project, and one constant holds the category internal name that
  HealthCheck and EmptySets both read. Fails today. No run shows it on this project, the lists
  are right for it.
- Note: Latent today, a false log line the day another project's XML is picked. CLAUDE.md says
  property internal names from the clash XML appear in tests as sample data only. The two names
  are standard Revit exporter names, so the letter of the rule is breached and the lists are the
  real fault. How the tool knows the lists belong to a project is a design step for the
  developer. The add-in also types Revit names in src\Federator.Addin\Engine\ModelFactsReader.cs:34-40.
  FR-027 notes that judging a set against the worksets this group's models carry avoids the C02
  lists.

### FR-012 revit-worksets-list-unread-looks-empty

A missing or unreadable revit-worksets list reads the same as an empty one, with no flag and
no log line, so the export check can name two pairs a person already decided are not typos and
the empty set judge says it cannot tell with no reason.

- Sources: T1-S44
- Evidence: src\Federator.Core\Exchange\RevitWorksets.cs:131-134, 'if (stream == null)' returns
  the empty known list, and :159-163 'catch (Exception)' resets known and decided to empty
  lists with no comment, flag or line. src\Federator.Core\Exchange\RevitCategories.cs:94-120
  has ResourceFound and an UNKNOWN line, and RevitWorksets has neither. Set 03 did not reach
  it: log:607 shows the decided list loaded ('2 more pair(s) are close enough too and are NOT
  listed').
- Root cause: src\Federator.Core\Exchange\RevitWorksets.cs:131-134 and :159-163, read by
  src\Federator.Core\Health\WorksetDisagreements.cs:171, src\Federator.Core\Health\ExportCheck.cs:186
  and src\Federator.Core\Sets\EmptySets.cs:257.
- Class: silent wrong number
- Proof: Core test in a new tests\Federator.Core.Tests\Exchange\RevitWorksetsTests.cs. Add a
  seam in RevitWorksets that loads the list from a given stream, shaped like
  RevitCategories.ResourceFound. Hand it a null stream and a stream that throws and assert
  ResourceFound is false and the EXPORT CHECK block carries a line saying UNKNOWN. Fails today
  because neither member exists. No run shows it.
- Note: Latent. A build that lost the resource already fails
  tests\Federator.Core.Tests\Health\ExportCheckTests.cs:198 and
  APairAPersonHasDecidedAboutIsCountedAndNotNamed in Actions, so the only live route is a read
  that throws inside the catch at run time, and how likely that is is UNKNOWN. The real harm is
  two spurious typo pairs, because the empty set half is hedged text.

### FR-013 set-sides-read-failure-returns-empty-so-sets-removed

When the clash test sides cannot be counted, sets the file does not name read as pointed at by
nothing and are removed with the rebuild box on, which orphans the tests that use them. If the
whole read throws, SidesBySetName logs that no set is removed and then returns an empty map, so
every such set reads 0 sides. If one side throws, it is left uncounted, which lowers the count
of the set it may point at, and the comment over that catch argues the opposite of what the
code does.

- Sources: T1-S18, T1-S19, NEW-QA SetBuilder.cs:284 (steps\loop.md:554)
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:284-292: log.Failure('reading what the
  clash tests point at', error, 'no set is removed or renamed and the run goes on') then return
  new Dictionary<string, int>(StringComparer.Ordinal). :243 'sides.TryGetValue(child.DisplayName,
  out pointing) ? pointing : 0' gives every set 0 sides. :320-325, the CountSide catch: 'A side
  this tool cannot read is a side it does not count, which is the safe direction: an uncounted
  side makes a set look SAFER to remove, so it is never counted and the refusal errs towards
  leaving things alone.' The count at :317 is skipped. src\Federator.Core\Sets\SetLeftovers.cs:159-161
  'if (set.Sides <= 0)' adds LeftoverAction.Remove, DocumentSet.Sides (:77-92) has no uncounted
  state, and SetBuilder.cs:574-581 removes and logs REMOVED.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:284-292 (the SidesBySetName catch) and
  :320-325 (the CountSide catch), with the decision at src\Federator.Core\Sets\SetLeftovers.cs:159-161
  and DocumentSet at :77-92, which has no way to say a side count is unknown, so minus one also
  removes.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetLeftoversTests.cs: a DocumentSet whose
  side count could not be taken (for example -1) that the file does not name gives Refuse,
  never Remove or RemoveTheTwinThenRename, and one uncounted set refuses every leftover of that
  document. Fails today because Sides at 0 or -1 both give Remove. The add-in then passes that
  state, or stops HandleLeftovers when the read fails, and marks the whole count unknown when
  any side throws, proved by review. A run line is UNKNOWN because neither read failure can be
  forced in the standard runs.
- Note: Only reached when the rebuild drifted sets box is on (SetBuilder.cs:506, off by
  default in SetRebuildSettings). How often either read throws is UNKNOWN, and no run has
  shown it. Not reached in set 03: no REMOVED or LEFTOVER line. The two catches are also among
  the 77 of FR-173.

### FR-014 set-leftover-walk-skips-test-folders-and-second-source

With the rebuild box on, a set the file no longer names that is pointed at only by tests a
person moved into a Clash Detective folder, or only by the second source of a side, counts 0
sides and is removed, which orphans those sides.

- Sources: T1-S17
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:273 'ClashTest test = tests.Tests[t] as
  ClashTest' then :275-278 'if (test == null) continue', with no descent into a folder. :309
  ResolveSelectionSource(sources[0]) reads only the first source of a side. Other test walks
  descend folders, for example src\Federator.Addin\Engine\SavedTests.cs:84-91 and
  UndoAutoReviewed.cs:87-92. src\Federator.Core\Sets\SetLeftovers.cs:159-161 removes at 0 sides
  and SetBuilder.cs:579-580 logs 'REMOVED ... which nothing pointed at'.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:271-282 (SidesBySetName walks only the
  root tests) and :302-309 (CountSide reads only sources[0]), with the decision at
  src\Federator.Core\Sets\SetLeftovers.cs:159-161.
- Class: silent wrong number
- Proof: Navisworks only, no Core test for the walk. The set 05 line that will show it fixed:
  on a copy of an NWF with one test moved into a Clash Detective folder, pointing at a set the
  XML no longer names, with the rebuild drifted sets box ticked, the SET block shows that
  leftover refused with 'NOTHING IS DONE' (SetLeftovers.cs:66-69) and not 'REMOVED <path>,
  which nothing pointed at' (SetBuilder.cs:580). That run is not in the standard five and needs
  the weekly run to pick the XML, so the line is UNKNOWN until the lead adds it.
- Note: Only reached when the rebuild drifted sets box is on (SetBuilder.cs:506). Scan 5z
  measured that a side whose set is removed stops resolving. Not reached in set 03: 61 created,
  0 already there, no REMOVED or LEFTOVER line. The same walk leaves its wrappers undisposed
  (the SavedItem at :273, the two ClashSelection wrappers at :280-281 and the Selection at
  :302), which is FR-029, so the two go together.

### FR-015 set-drift-key-ignores-negation-group-and-comparison

A saved set that differs from the file only by a negation (flag 32), by its OR grouping (flag
64) or by a comparison other than contains is read as the same, because the compared key
carries no flags and maps every comparison but contains to equals. So it is never called
drifted, never rebuilt, and can be taken for a twin of another set.

- Sources: T1-S15, T1-S67
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:83-90 builds ReadCondition from the two
  names, the comparison and the value and never reads condition.Options. :88 maps
  SearchConditionComparison.DisplayStringContains to "contains" and every other comparison to
  "equals", and :93-99 KeyOf builds category, property, contains or equals and value with no
  flags, while :1029-1032 passes (SearchConditionOptions)condition.Flags when building.
  src\Federator.Core\Sets\SetDrift.cs:46 'return CategoryInternalName + "|" +
  PropertyInternalName + "|" + Test + "|" + Value'. src\Federator.Core\Sets\SetLeftovers.cs:207-223
  SameQuestion compares the same flagless keys, although its doc at :110-112 says same flags.
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml carries flags=64 five times and flags=32 six
  times, and MatrixCorrections.cs:859 names 32 as NegateCondition. SetDrift.cs:63-69 justifies
  leaving flags out only for the two Ignore display name bits and says the flags are reported,
  but nothing reads them. Set 03 did not reach it: all 61 sets were created and none read back.
- Root cause: src\Federator.Core\Sets\SetDrift.cs:44-47 (Key), src\Federator.Addin\Engine\SetBuilder.cs:83-90
  (Read), :88 and :93-99 (KeyOf, a second copy of the key), and
  src\Federator.Core\Sets\SetLeftovers.cs:207-223 (SameQuestion).
- Class: silent wrong number
- Proof: Core tests. In tests\Federator.Core.Tests\Sets\SetDriftTests.cs, one key builder in
  Core that carries the comparison word and the 32 and 64 bits, where SetDrift.Compare on four
  conditions whose third carries the group bit only in the file asserts Drifted, one differing
  only by flag 32 asserts Drifted, and a comparison other than equals and contains gives a
  different key. In tests\Federator.Core.Tests\Sets\SetLeftoversTests.cs, SetLeftovers.For does
  not call two sets differing only by that bit twins. All fail today. Then a set 05 weekly run
  that picks the corrected XML with the rebuild box on (not in the standard five), because what
  SearchCondition.Options returns for a set saved in an NWF is UNKNOWN.
- Note: Mask to the 32 and 64 bits only. A set this tool built reads Options 37, which is 32
  plus the two Ignore bits (docs\history\scan.md:3833), and 1A02MM's original import carries
  none of the Ignore bits (SetDrift.cs:63-69), so comparing every bit would rebuild 61 sets
  over nothing. FR-023 and FR-024 drop the same bits in the HEALTH checks, so one Core rule for
  which flag bits are part of the question serves all three, the constants being in
  SetBuildPlan.cs:77 and MatrixCorrections.cs:670 and :859 today. FR-016 follows this one.

### FR-016 set-drift-lines-describe-or-set-as-and-chain

For a set already in the NWF the SET DRIFT lines and the SETS ACROSS THE RUN line join every
condition with 'and', so an OR set such as BLD-ME-Ducts&Duct Fittings is printed as a four way
AND that no element can answer, which F78 fixed for a created set.

- Sources: T1-S68
- Evidence: src\Federator.Core\Sets\SetDrift.cs:148 'return string.Join(" and ",
  parts.ToArray())' and :161 'return string.Join(" and ", array)'.
  src\Federator.Addin\Engine\SetBuilder.cs:73-77 'described.Add(condition.Describe())' hands one
  string per condition, so the file's grouping is lost before WantedNow sees it. :663 'string
  askedNow = drift.AskedNow()' and :693 'present.Asked = askedNow' reach
  src\Federator.Core\Sets\SetsAcrossTheRun.cs:198 '"asks " + set.Asked'. A created set gets
  PlannedSet.Describe (src\Federator.Core\Sets\SetBuildPlan.cs:181-206), which brackets the
  groups and joins them with ' or ', at SetBuilder.cs:757. Set 03: the created path grouped the
  OR correctly (log:243) and SETS ACROSS THE RUN lists two condition sets only, so the already
  there path was not shown.
- Root cause: src\Federator.Core\Sets\SetDrift.cs:148 and :161, fed by
  src\Federator.Addin\Engine\SetBuilder.cs:73-77, carried by :663 and :693 into
  src\Federator.Core\Sets\SetsAcrossTheRun.cs:198.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetDriftTests.cs: SetDrift fed a four
  condition set with the group bit on the third asserts AskedNow and WantedNow both read as two
  bracketed groups joined by or. Fails today. Then a run that shows the line on a real OR set,
  which needs the XML picked on a rerun, UNKNOWN whether set 05 holds one.
- Note: AskedNow cannot group until ReadCondition carries the group bit, so this follows FR-015.
  WantedNow could group today if the add-in passed the planned set.

### FR-017 set-value-unreadable-kind-read-as-empty-string

A set condition value of any kind other than display string or identifier string throws in
ToDisplayString and is read as an empty string, so the set is reported drifted as asking for an
empty value and is replaced when the rebuild box is on.

- Sources: T1-S16
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:110-112 'return value.DataType ==
  VariantDataType.IdentifierString ? value.ToIdentifierString() : value.ToDisplayString()' and
  :114-116 'catch (Exception) { return string.Empty }'. src\Federator.Core\Sets\SetDrift.cs:103
  sets CouldNotRead only when asked == null, so an empty value compares as a real question and
  :118-121 marks Drifted. SetBuilder.cs:648-651 rebuilds when the box is on. addin.md says a
  property value is read by its kind, and ClashHarvest.Text is the kind based reader in the
  same assembly.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:101-118 (ValueOf).
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetDriftTests.cs: a ReadCondition marker
  for a value that would not read, and SetDrift.Compare then gives CouldNotRead true and
  Drifted false. Fails today because no marker exists and the empty string compares as a
  question. The add-in switch to the kind based reader is proved by a set 05 weekly run that
  reads saved sets (not in the standard five).
- Note: Sets this tool builds carry VariantData.FromDisplayString (SetBuilder.cs:1051) and read
  fine. Whether a saved set of another origin, such as the original import in 1A02MM, carries
  another kind is UNKNOWN, and the 5w count of 0 unreadable cannot rule it out because this
  catch hides the throw from the whole read. Not reached in set 03: no saved set condition was
  read.

### FR-018 rebuilt-set-not-found-again-counted-as-zero-items

A present set that could not be found again after a rebuild is recorded as finding 0 items
instead of unknown, judged as empty on the old question, and counted at zero across the run.

- Sources: T1-S20
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:646 'int found = 0' and :665-669 set it
  only 'if (now != null)', then :683-684 outcome.AddAlreadyPresent(planned.Path, planned.Name,
  planned.ConditionCount, found), and :701-704 call outcome.AddEmpty(EmptySets.Why(planned.Path,
  asking)) when found is 0 and the drift could be read, where asking is still drift.Asked, the
  question read before the rebuild. src\Federator.Core\Sets\SetsAcrossTheRun.cs:134 skips only
  result.ItemCount < 0 and :152 counts 0 as a group at zero. SetResult.cs:43 'Minus one when
  it never resolved'.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:646 and :665-704.
- Class: silent wrong number
- Proof: Navisworks only for the miss itself. The fix starts found at minus one and is proved by
  review. A Core half in tests\Federator.Core.Tests\Sets\SetBuildOutcomeTests.cs: a present set
  whose count is minus one is left out of the zero counts and not judged empty. The set 05 line,
  in a weekly run that picks the corrected XML with the rebuild box on (not in the standard
  five), is that no REBUILT set is counted at 0 items unless a fresh count reads 0.
- Note: Without a rebuild the second find cannot plausibly miss, since no mutator sits between
  the two finds on the same parent. After a rebuild it depends on FR-019, which is UNKNOWN. Not
  reached in set 03: every set took the created branch.

### FR-019 set-parent-handle-held-across-replacewithcopy

After a set is replaced with ReplaceWithCopy, the new set is read back through the same parent
folder handle that was held across the mutator, which the create path avoids by resolving the
folders again from a fresh root.

- Sources: T1-S21
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:631 'using (GroupItem parent =
  EnsureFolders(sets, planned.Folders))', :650 'rebuilt = Rebuild(document, sets, planned,
  parent)', :155 'sets.ReplaceWithCopy(parent, at, made)', then :665 'using (SelectionSet now =
  FindSelectionSet(parent, planned.Name))'. The create path resolves again at :750 'using
  (GroupItem fresh = ResolveFolders(sets, planned.Folders, planned.Folders.Count))', and
  :809-812 say a handle held across an AddCopy does not show the new child. The Rebuild comment
  at :126-128 says the parent is resolved fresh, but it is the one from :631.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:155 and :665, with the parent from :631.
- Class: silent wrong number
- Proof: Navisworks only. Set 05 line: in a weekly run that picks the corrected XML with the
  rebuild box on, the SET line '   REBUILT from the picked file, and it now finds N item(s)'
  (SetBuilder.cs:716) gives the same N as a count read after resolving the folder again from a
  fresh RootItem, and no REBUILT set also reads FAILED. That run is not in the standard five, so
  the line is UNKNOWN until added.
- Note: What ReplaceWithCopy does to a parent handle held across it is UNKNOWN. If it acts like
  AddCopy, the count, AskedNow and the judgement after a rebuild come off the old set. If the
  handle dies, the outer catch at :762-772 reports FAILED for a set that was in fact rebuilt. No
  log in steps\logs carries 'and it now finds', so no run has read this path. Not reached in
  set 03.

### FR-020 rebuilt-sets-do-not-ask-for-nwf-save

A set rebuilt from the picked file changes the document and is logged REBUILT, but the sets
step does not ask for the NWF to be saved again. Where no test is created or run and no status
or viewpoint is written, the NWD is published from the rebuilt document while the NWF on disk
keeps the old sets.

- Sources: T1-S64
- Evidence: src\Federator.Core\Sets\SetBuildOutcome.cs:205-208, PutAnythingIn returns
  CreatedCount > 0, while RebuiltCount (:138-141) is kept apart.
  src\Federator.Addin\Engine\FederationEngine.cs:2456 'return sets.PutAnythingIn ||
  sets.ActedOnLeftovers > 0'. The rebuild is SetBuilder.cs:155 'sets.ReplaceWithCopy(parent,
  at, made)'. ClashStep at :2225-2231 returns CreateAndRunTheTests(...) or changed,
  FinishTheGroup at :959 saves the NWF only on clashPutSomethingIn or viewsPutSomethingIn, and
  CreateAndRunTheTests ends 'clash.CreatedCount > 0 || clash.RanCount > 0 ||
  runner.ChangedAStatus' (:2725). In the usual weekly run tests run and the save happens
  anyway. Set 03 did not reach it, 0 already there in all 22.
- Root cause: src\Federator.Core\Sets\SetBuildOutcome.cs:205-208 and
  src\Federator.Addin\Engine\FederationEngine.cs:2456, with the gate at FederationEngine.cs:959.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetBuildOutcomeTests.cs, beside
  SixtyPresentAndOneCreatedPutSomethingIn at :299: a drift added as rebuilt with nothing
  created gives PutAnythingIn true. Fails today.
- Note: The same family as FR-043, FR-044, FR-045 and FR-150, an NWF save that nothing asks
  for or checks. It shows only for a single discipline group whose tests all exist, or a file
  holding sets only. A run proof needs the rebuild box ticked and the XML picked on a rerun,
  which the loop's runs 2 to 4 do not do, UNKNOWN.

### FR-021 set-lines-claim-every-set-matches-when-unread

The sets lines say every set in the document asks what the file asks whenever nothing drifted,
including when a present set's search could not be read or it holds no search, because the
outcome keeps no count of unread sets.

- Sources: T1-S65
- Evidence: src\Federator.Core\Sets\SetBuildOutcome.cs:370-372 'if (Drifted.Count == 0)' then
  'none of them drifted. Every set in the document asks what the file asks'.
  src\Federator.Core\Sets\SetDrift.cs:92-93, Drifted is false where it could not be read, and
  :103 marks CouldNotRead when asked is null. src\Federator.Addin\Engine\SetBuilder.cs:51-68
  sets asked to null when existing.HasSearch is false or the read throws, and :706 'if
  (drift.Drifted)' is the only route into Drifted. Lines() reaches the window only, through
  ShowSetLines at src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2408 after the Build sets
  button. Set 03 did not press it (driver.txt:19).
- Root cause: src\Federator.Core\Sets\SetBuildOutcome.cs:370-373, with
  src\Federator.Addin\Engine\SetBuilder.cs:51-68 and :706, where an unread set is never
  recorded.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetBuildOutcomeTests.cs: the outcome
  learns of an unread set (a new call or flag) and Lines() then does not claim every set
  matches the file. Fails today because the state does not exist.
- Note: Low reach, the window text after Build sets by hand only, and 5w measured 0 unreadable
  searches on all ten of his groups. UNKNOWN how often a search cannot be read.

### FR-022 sets-summary-counts-created-sets-only

The sets summary and the sets lines count finding items, at zero and items found over the sets
created by this build only, with nothing in the words saying so. A run where every set is
already there logs 0 finding items and 0 at zero while the present sets found thousands of
items and many found none.

- Sources: T1-S66
- Evidence: src\Federator.Core\Sets\SetBuildOutcome.cs:313-316, Summary writes the created
  count, the already there count, FindingItemsCount and ZeroCount as 'N created, N already
  there, N finding items, N at zero'. FindingItemsCount at :211-214 is Count(true, true),
  ZeroCount counts result.IsZero, which src\Federator.Core\Sets\SetResult.cs:66 defines as
  Created && ItemCount == 0, TotalItems at :258-274 adds only 'if (result.Created)', and Lines
  at :386-387 and :399 write 'sets finding items: ', 'sets at zero      : ' and 'items found
  : '. Summary is the SETS step finish phrase on every run
  (src\Federator.Addin\Engine\FederationEngine.cs:2389).
  tests\Federator.Core.Tests\Sets\SetBuildOutcomeTests.cs:313-314 pins the counting on purpose.
  Set 03's first run adds up (log:288 '61 created, 0 already there, 9 finding items, 52 at
  zero.') and the weekly wording was not run.
- Root cause: src\Federator.Core\Sets\SetBuildOutcome.cs:313-319 and :386-399, counting through
  :196, :211-214, :217-233 and :258-274.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetBuildOutcomeTests.cs: an outcome of 61
  present sets, 38 of them at zero, no longer reads as 0 finding items and 0 at zero. The counts
  stay and the labels change, so the assertion at :313-314 stays. Fails today. Run: the SETS
  step finish phrase of a rerun with the XML picked.
- Note: SetsAcrossTheRun (src\Federator.Core\Sets\SetsAcrossTheRun.cs:132-160) already counts
  present sets at zero, so the numbers exist. A run proof needs the XML picked on a rerun, which
  the loop's runs 2 to 4 do not do, UNKNOWN.

### FR-023 set-warnings-negated-category-reported-as-asked

The unknown category check reads a negated condition as a category the set asks for.
BLD-EL-Devices is reported as asking for Telephone Devices while it only excludes it, so the
HEALTH count of 14 holds one false entry, and the warning says such a set can never match
anything.

- Sources: T1-S52
- Evidence: src\Federator.Core\Health\SetWarnings.cs:231-238 'string asked = condition.Value ==
  null ? string.Empty : condition.Value.Data' then 'found.Add(new CategoryNobodyHas(set,
  asked))', with nothing reading condition.Flags.
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml:28823-28902 holds BLD-EL-Devices as
  'contains Devices' flags=0 and six 'equals' flags=32, the last on Telephone Devices at
  :28892-28902, which no model carries (revit-categories.txt has Communication :45, Data :51,
  Fire Alarm :63, Lighting :81 and Security Devices :390).
  tests\Federator.Core.Tests\Health\SetWarningsTests.cs:344 pins 'Sets asking for a category no
  model carries: 14'. docs\history\scan.md:3866 measured that a negated equals on a value with
  no items, beside contains Devices, excludes nothing. The claim is SetWarnings.cs:47-48 'never
  match anything'. Set 03 wrote no HEALTH block.
- Root cause: src\Federator.Core\Health\SetWarnings.cs:221-238, with the claims at :47-48 and
  src\Federator.Core\Health\HealthCheckResult.cs:204-205.
- Class: silent wrong number
- Proof: Core tests in tests\Federator.Core.Tests\Health\SetWarningsTests.cs: a set holding
  'contains Devices' and a flags=32 'equals Telephone Devices' is not reported, and the count
  pinned at :344 moves from 14 to 13. A second test: a set of two flags=64 groups where only
  one names an unknown category is not described as one that can never match. Both fail today.
- Note: The negation half is live on the committed matrix. The OR half is latent, the five
  flags=64 sets all name categories in the list. What a clash test does with a set holding only
  the four model roots is UNKNOWN. The HEALTH block is written only when the XML is picked with
  Browse (src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2106-2166), and the set 03 driver
  typed the path, so a run proof needs the Browse route. The same flag bits as FR-015.

### FR-024 identical-sets-signature-ignores-flags

Two sets whose conditions differ only in flags are reported as asking exactly the same
question, for example one OR of two groups and one AND that nothing can satisfy, or a condition
and its negation. The code comment says the opposite of what the code does.

- Sources: T1-S53
- Evidence: src\Federator.Core\Health\SetWarnings.cs:119-121 'THE SIGNATURE LEAVES THE FLAGS OUT
  ... two sets that differ only in how their conditions are grouped ask a different question
  and are not identical', while :183 'parts.Add(condition.RuleSignature)' uses
  src\Federator.Core\Exchange\ExchangeModel.cs:181-196, which appends only test, category,
  property and value ('Flags is left out on purpose'). No SetWarningsTests case varies flags,
  and the helper at tests\Federator.Core.Tests\Health\SetWarningsTests.cs:34 always writes
  flags=0. HealthCheckResult.cs:208 prints 'Sets asking exactly the same question: '. Neither
  committed matrix carries such a pair, so the named pairs on the real files are right. Set 03
  wrote no HEALTH block.
- Root cause: src\Federator.Core\Health\SetWarnings.cs:119-121 and :172-189, with
  src\Federator.Core\Exchange\ExchangeModel.cs:181-196.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Health\SetWarningsTests.cs: two sets holding
  the same four conditions, one with flags 64 on the third and one without, are not reported
  identical, and a condition against its flags 32 negation is not identical. Fails today.
- Note: Latent. RuleSignature itself stays as it is: src\Federator.Core\Health\HealthCheck.cs:179
  counts DistinctRuleCount with it and core.md fixes the 53 with flags left out. The 32 and 64
  bits go in the set level signature only. The same flag rule as FR-015.

### FR-025 matrix-or-row-splits-set-without-category-condition

MatrixCorrections writes an Or row as one flags 64 condition straight after the matching
condition, which starts a new group holding only the workset, so a set of Category X and
Workset V becomes (X and V) or (V other spelling), which takes every element on the other
workset whatever its category.

- Sources: T1-S42
- Evidence: src\Federator.Core\Exchange\MatrixCorrections.cs:643-644 'string or =
  OneCondition(template, "equals", StartGroup, row.AlsoAccept)' then 'text = text.Substring(0,
  closes) + or + text.Substring(closes)'. src\Federator.Core\Sets\SetBuildPlan.cs:219-223 'if
  (current == null || condition.StartsAGroup)' starts a new list in groups.
- Root cause: src\Federator.Core\Exchange\MatrixCorrections.cs:643-644.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Exchange\MatrixCorrectionsTests.cs: add an Or
  row to a two condition set, read it back through ExchangeReader and SetBuildPlan.Groups, and
  assert every group still holds the category condition. Fails today.
  MatrixCorrectionsTests.cs:585-629 uses only the one condition set built by Set().
- Note: Latent. Nothing in src calls MatrixCorrections (FR-030), the committed corrected matrix
  is made without Or rows, and Q68 names the class as the way the matrix is corrected. What the
  doc means needs the whole group copied with the other spelling, (X and V) or (X and V other).
  Not reachable by a run. It matters if FR-008 is answered with an OR of both spellings made
  through this class.
- Answered through Q102 and Q104 on 2026-10-04. The lead's note: this fault will be reachable once F116 applies the class at pick time and builds Q102's OR rows, so F116 works it in wave 1.

### FR-026 matrix-category-rewrite-renames-the-set-itself

A category rewrite replaces its From text across the whole set block, which includes the set's
own name attribute, and the constructor does not refuse a To that holds From, so the set can be
renamed inside its block and the value can grow every time it is applied.

- Sources: T1-S43
- Evidence: src\Federator.Core\Exchange\MatrixCorrections.cs:898 'return text.Substring(0, at)
  + block.Replace(rewrite.From, rewrite.To) + text.Substring(ends)' over a block that starts at
  the selectionset opening with its name. The CategoryRewrite constructor at :55-71 checks only
  for empty strings, while SetRename refuses at :26-31 'if (to.IndexOf(from,
  StringComparison.Ordinal) >= 0)'.
- Root cause: src\Federator.Core\Exchange\MatrixCorrections.cs:898 and the CategoryRewrite
  constructor at :55-71.
- Class: silent wrong number
- Proof: Two Core tests in tests\Federator.Core.Tests\Exchange\MatrixCorrectionsTests.cs: one
  uses a set whose name holds From and asserts the name is unchanged, the other applies a
  rewrite whose To holds From twice and asserts TotalChanged is 0 the second time, or that the
  constructor refuses it. Both fail today. ARewriteChangesTheNamedSetAndNoOther (:200) asserts
  on a different set's name, so the rewritten set's own name is never checked.
- Note: Latent. Nothing in src calls MatrixCorrections and the generator in the tests passes
  null for these rewrites. The class doc at :382 says safe to run twice is the whole point.
  Where From is also in the name, the second pass finds no set. Not reachable by a run.
- Answered through Q104 on 2026-10-04. The lead's note: this fault will be reachable once F116 applies the class at pick time, so F116 works it in wave 1.

### FR-027 empty-sets-block-never-written-on-a-first-run

No EMPTY SETS block and no 'set finding nothing' row exists in any group although every group
had sets that found nothing, because only a set already in the NWF is ever judged for why it
found nothing. So which sets are wrong, and what they cost in clash tests, is never said on a
first run, the run that creates every set.

- Sources: S03-12 (one reader gave it as set 03 findings.md item 12), C06-J15, C06-DONE-8,
  T1-S63, LOOK 62, the register row step NEW EMPTY SETS never written on a first run
  (steps\loop.md:563)
- Evidence: log:288 'STEP SETS finished 0.221s 61 created, 0 already there, 9 finding items, 52
  at zero.' and log:8346 'found nothing in every group : 14'. log:219 'SET ZERO
  .../BLD-AR-Stairs 2 conditions 0 items asked for ...'. No 'EMPTY SETS' line in the log and no
  'set finding nothing' row in the tsv across the 22 groups (tsv:71 is the SETS step row).
  src\Federator.Core\Sets\SetBuildOutcome.cs:74-75 says Empty is every set that found nothing.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:756-758, the created path calls
  AddCreated and never AddEmpty. The only caller of EmptySets.Why, and of AddEmpty, is
  SetBuilder.cs:701-704, inside the already there branch (:638-720). The block at
  src\Federator.Addin\Engine\FederationEngine.cs:2626-2636 needs outcome.Sets.Empty.Count above
  0.
- Class: broken feature
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetBuildOutcomeTests.cs once the
  judgement moves into SetBuildOutcome: a created set that found 0 items, with its asked
  conditions, lands in Empty with a reason. Fails before. Run line of set 05: an 'EMPTY SETS
  <building>' block after the CLASH block in every group that has a set at zero, and tsv rows
  of kind 'set finding nothing' (set 03 has none of either, log:288).
- Note: The first run judges what the file asks, so the created path needs a mapping from the
  planned conditions to ReadCondition in Core. Judge a set against the worksets this group's
  models carry, which EXPORT CHECK already reads (ModelExport.Worksets,
  src\Federator.Core\Health\ExportCheck.cs:48), and not only against the project wide lists
  measured on C02. EmptySets.KnownFor (src\Federator.Core\Sets\EmptySets.cs:248-261) reads
  revit-worksets.txt and revit-categories.txt, so on C06 it would say no model carries
  ME-DUCTWORK for 1B06BC, whose models do (log:605). A nearest value that differs only by
  letter case says so in words. The quality of the judgement rests on FR-010 and FR-011. LOOK
  62 reads CONTRADICTED and the order of the other blocks holds.

### FR-028 workset-case-warning-printed-in-every-group

The EXPORT CHECK prints its case sensitive warning in every group whether or not any name
differs, with one project's two spellings typed into the sentence.

- Sources: S03-2, C06-J2
- Evidence: log:4294 '(1B06PK) check these against the workset each set in the matrix asks
  for. The match is CASE SENSITIVE, so ME-Ductwork does not match ME-DUCTWORK and that set finds
  nothing', and the same sentence at log:210, 606 and 1847 and in all 22 groups, 1B06BC among
  them, whose models carry ME-DUCTWORK.
- Root cause: src\Federator.Core\Health\ExportCheck.cs:234-235, AddWorksets writes the sentence
  whenever any workset was seen. ExportCheck.Lines takes the models only, so it cannot compare
  with what the sets ask. tests\Federator.Core.Tests\Health\ExportCheckTests.cs:56-62 pins the
  sentence.
- Class: noise
- Proof: New Core test in tests\Federator.Core.Tests\Health\ExportCheckTests.cs replacing the
  pin at :56-62: given the workset values the picked file asks, a set whose asked name differs
  from a carried name only by letter case is named, and a group where no name differs prints no
  warning. Fails before. Run line of set 05: no case warning in groups where nothing differs.
- Note: The replaced test changes because the rule changes, not to weaken it, and the pull
  request says so. The silent part of the same trouble is FR-027. ExportCheck.cs is also the
  file of FR-003 to FR-005 in alignment, so this goes in their pull request or after it.
  S03-2 and C06-J2 also sit on FR-008, the matrix half.

### FR-029 set-builder-handles-never-disposed

SidesBySetName and WalkForLeftovers hold wrappers they never dispose, so they are left to the
finalizer.

- Sources: NEW-QA SetBuilder.cs:271 (steps\loop.md:555)
- Evidence: src\Federator.Addin\Engine\SetBuilder.cs:273, the ClashTest from tests.Tests[t] has
  no using, :280-281 SelectionA and SelectionB, :302 side.Selection, :309 sources[0].
  WalkForLeftovers at :224-226 reads set.Search twice and its SearchConditions, none disposed.
  .claude\rules\addin.md says what this tool creates or resolves it disposes.
- Root cause: src\Federator.Addin\Engine\SetBuilder.cs:273, :280-281, :302, :309 and :224-226.
- Class: noise
- Proof: No Core test reaches it. Run line: a run with the rebuild box on in set 05 logs no
  ObjectDisposedException and the handle count at the end does not climb.
- Note: Q28 (needs Bader, unanswered) asks the same for sub objects read off a handle this tool
  made, such as side.Selection and set.Search, because their ownership is not measured. Items
  read out of a document collection (the ClashTest, the SavedItem) are covered by the measured
  rule. Only runs with the rebuild box on reach this code. The same walk is FR-014, so the two
  go together.

### FR-030 matrix-corrections-has-no-caller

MatrixCorrections is a public class nothing in src calls, and nothing at run time says which
form of a set a picked file carries.

- Sources: A15 (steps\loop.md:546)
- Evidence: src\Federator.Core\Exchange\MatrixCorrections.cs:391 'public static class
  MatrixCorrections'. A grep finds it in src only in a comment at RevitWorksets.cs:15, and in
  tests only in tests\Federator.Core.Tests\Exchange\MatrixCorrectionsTests.cs.
  steps\04_audit_first_run.md:152 'either the tool applies the corrections at pick time or the
  class moves to tools'. exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml is the file it
  produced.
- Root cause: src\Federator.Core\Exchange\MatrixCorrections.cs:391, declared and never called by
  src.
- Class: noise
- Proof: If it moves to tools: a grep for MatrixCorrections over src reads 0, and the test that
  proves exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml equals what the rule produces still
  passes. If it is applied at pick time: a Core test that the picked file's outcome lines reach
  the HEALTH block.
- Needed Bader, asked as Q104, answered on 2026-10-04, see the answered line of this item. Q68 (answered) says the tool must not silently fix the matrix, and the
  answer was to correct the file, which is what the exchange folder holds. Applying it at pick
  time, even with a log line, changes what the tool does to the client's XML, and moving it out
  of Core takes its 22 test references with it.
- Note: The case in point is finding 2 of findings.md, where the desktop XML asked
  ME-DUCTWORK. Bader has since put the corrected file in place (Q98 B1). FR-025 and FR-026 are
  faults inside this class, and it is among the 150 of FR-172.
- Answered by Bader on 2026-10-04 at 15:23, Q104, in short, his words being under the question in steps\02_questions.md: the tool uses the code that builds the corrected XML, MatrixCorrections is applied to whichever XML is picked before any set is built, the log names every correction it made, the Q102 and Q103 rules live in it, and a test is to prove the old uncorrected XML and the exchange file give the same sets once corrected. The lead's note: to be worked by F116, the clash XML, in wave 1.
- Answered by Bader on 2026-10-04 in the evening, Q113 B: the correction list, the three matrix
  corrections, the Q103 rule and the 30 workset spellings, is a plain file kept beside the picked
  XML, read when the XML is picked and named in the log, and the code that applies it stays in
  Core. A picked XML with no list beside it is corrected by nothing and the log says so. This
  project's list goes in exchange\ beside the corrected XML, and Bader is told when it is there.
  F116 carries it before it merges

## Clash counts, FR-031 to FR-034

### FR-031 clash-progress-line-one-test-short

The last CLASH progress line of every group counts one test too few, and in three groups its
clash total is short by the last test's clashes.

- Sources: S03-6, C06-J5, C06-DONE-5, T1-S25
- Evidence: log:4411 'CLASH 91 of 91 tests, 90 run, 1739 skipped, 1624 clashes so far' against
  log:4452 'tests run : 91' and log:4455 'clashes found : 1629'. log:2617 '21 of 21 tests, 20
  run, 1809 skipped, 18 clashes so far' against log:2652 'clashes found : 20'. log:1144 '36 of
  36 tests, 0 run, 1829 skipped' against log:1168 'tests skipped : 1830'. log:348 '35 run'
  against log:379 'tests run : 36'.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:572-579 writes the line before OneTest
  runs at :581, with i+1 as the test count and RanCount, SkippedCount and TotalClashes as they
  stood before that test.
- Class: silent wrong number
- Proof: Run line of set 05: in every group the last 'CLASH N of N tests' line shows tests run
  plus tests skipped equal to the block, and its clashes so far equal to the block's 'clashes
  found' (set 03 log:4411 against log:4455). A Core test is possible once the sentence is built
  in Core as a method of Federator.Core.Clash.ClashRunOutcome taking the counts, in
  tests\Federator.Core.Tests\Clash\ClashRunOutcomeTests.cs: after the last test the line equals
  the block. It fails before since there is no such method.
- Note: Nothing reads the progress line, and the block under it is right and agrees with the
  workbook. The progress line is also written only every 25 tests (ClashRunner.cs:54) and left
  the log silent for 3 min 17 s in 1B06PK (log:4407 to 4408). T1-S25 sits here as a side note
  and is FR-052, and ClashRunOutcome.cs is also FR-052's file.

### FR-032 empty-result-group-counted-as-one-clash

A result group with no clashes under it becomes one row with one clash, so the Clashes cell and
the group's status cell in the workbook read one higher than the Clash Detective panel.

- Sources: T1-S2
- Evidence: src\Federator.Addin\Engine\ClashHarvest.cs:160 'int raw =
  CountLeaves(group.Children, distances)' then :164 'row.RawClashes = raw < 1 ? 1 : raw'.
  src\Federator.Core\Report\ClashReportModel.cs:396 tally.Add(row.Status, row.RawClashes).
  src\Federator.Core\Report\WorkbookWriter.cs:289 and :330 write test.RawClashes.
  src\Federator.Addin\Engine\ClashRunner.cs:1608-1636 CountInto adds nothing for a group with no
  ClashResult under it.
- Root cause: src\Federator.Addin\Engine\ClashHarvest.cs:164.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Report\ClashReportTests.cs: a row built for a
  result group with no leaves carries 0 raw clashes (or is not added) and adds 0 to the status
  tally. The floor moves into Core on ClashRow for that to be testable. It fails today because
  the floor of 1 is in the add-in. Then the ROWS line of a set 05 group that holds an empty
  result group, if one exists, agrees with the panel.
- Note: Whether a saved test can hold an empty result group is UNKNOWN. No log in steps\logs or
  set 03 ever wrote 'result group', so it is also UNKNOWN whether groups appear on this tool's
  tests, though a person grouping results in the panel of a weekly NWF would make them. Set 03
  did not reach it: the read-outs show the tests' own total equal to the clash rows in all 22
  groups. A related gap, UNKNOWN: GroupRow gives every leaf the group's own status
  (ClashHarvest.cs:166) while CountInto counts each leaf by its own, so the status columns could
  also disagree with the panel if a leaf's status can differ from its group's. The panel match
  is the third test of done.

### FR-033 bydesign-unnamed-clash-counted-as-moved

An unnamed clash that the by design rule judges Reviewed is counted as moved in the BY DESIGN
block, the RESULT line and the rule B run line, though it is left out of the wanted list and
never moved.

- Sources: T1-S1
- Evidence: src\Federator.Addin\Engine\ByDesign.cs:148 'tally.Add(testName, clashName.Length ==
  0 ? "an unnamed clash" : clashName, verdict, pair)', but :150 'if (verdict ==
  ByDesignVerdict.Reviewed && clashName.Length > 0)' is the only place the name is checked.
  src\Federator.Core\Clash\ByDesignRule.cs:36-74 Judge never sees the name.
  src\Federator.Core\Clash\ByDesignTally.cs:98 MovedCount returns Of(ByDesignVerdict.Reviewed)
  and :143-145 Add writes a REVIEWED line for it. src\Federator.Addin\Engine\FederationEngine.cs:2667
  'log.ByDesignMoved += byDesignTally.MovedCount'.
- Root cause: src\Federator.Addin\Engine\ByDesign.cs:148 and :150, with
  src\Federator.Core\Clash\ByDesignRule.cs:36-74 and src\Federator.Core\Clash\ByDesignTally.cs:96-99
  and :138-145.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Clash\ByDesignRuleTests.cs: a clash with an
  empty name at New between a listed pair does not count in MovedCount and writes no REVIEWED
  line. It fails today because MovedCount reads 1. The shape of the fix is open (Judge taking
  the name, or the tally refusing an empty name), so the test is written against the tally's
  MovedCount and Reviewed lines.
- Note: Only when the by design box is ticked. src\Federator.Addin\Engine\Penetrations.cs:150-155
  counts the same case as NotAPenetration and returns, so the two rules disagree. The undo has
  the same shape at UndoAutoReviewed.cs:173-175 (FR-152). Whether Navisworks ever hands back a
  ClashResult with an empty DisplayName is UNKNOWN. Not reached in set 03: 'by design : no, the
  pairs file is not read'.

### FR-034 auto-review-record-in-throws-on-edited-comment

AutoReviewRecord.In throws ArgumentOutOfRangeException instead of returning null for a marker
comment reading was Approved, was Reviewed or was Resolved, so one hand edited comment stops the
undo for the whole test.

- Sources: T1-L5
- Evidence: src\Federator.Core\Clash\AutoReviewRecord.cs:211-225, StatusFrom loops over
  ClashTally.AllStatuses so it accepts all five, :150 'return new
  AutoReviewRecord(rule.Value, wasAt, rest.Substring(after).TrimStart())', and the constructor
  at :54-60 throws when StatusesThisToolMayMoveFrom.Allows(wasAt) is false, which it is for
  anything but New and Active. The summary at :112-116 says In returns null where the comment
  is not one of ours. src\Federator.Addin\Engine\UndoAutoReviewed.cs:198-205 calls In on every
  comment inside Judge, which OneTest calls in a try, so every clash of that test is left alone.
  tests\Federator.Core.Tests\Clash\AutoReviewRecordTests.cs:110-119 tests an unknown rule and
  '[was Nonsense]' and never '[was Approved]'. Set 03 moved no status and ran no undo (log:62,
  log:65).
- Root cause: src\Federator.Core\Clash\AutoReviewRecord.cs:150 (the constructor call), the
  constructor at :54-60 and StatusFrom at :211-225.
- Class: loud failure
- Proof: Core test in tests\Federator.Core.Tests\Clash\AutoReviewRecordTests.cs: In with the
  marker, a penetration rule and '[was Approved]' returns null. It throws today.
- Note: This tool cannot write such a comment, so only a comment edited by hand reaches the
  path. MayUndo (:162) and WhyNotUndone (:171) also call In. AutoReviewRecord.cs is also read by
  FR-152.

## Workbook, FR-035 to FR-039

### FR-035 workbook-check-counts-only-blocks-with-clashes

The WORKBOOK CHECK counts only the test blocks that hold clashes, so every group says in
capitals that its workbook is short of the matrix while the workbook holds all 1830 tests, the
window line says 'Workbook: one sheet, 5 tests', and the check cannot see a missing one row
test.

- Sources: S03-4, C06-J3, FIND-03, step 269c, LOOK 269c, C06-DONE-4, Q73 (the register row at
  steps\loop.md:587)
- Evidence: log:451 'CHECK 1 sheet, ..., 5 test blocks, 13 clash rows.' and log:455 'BLOCKS 5 in
  the workbook against 1830 tests in the file. THE WORKBOOK MUST CARRY A BLOCK FOR EVERY TEST IN
  THE FILE' against workbooks\Clash_Report_C06_1104-PAR-100000-ZZZ-BM-MOD-000001.xlsx.txt:1855
  'TOTALS: tests 1830 (5 full blocks, 1825 one row, 0 with no name)'. The 22 check counts add to
  457 against log:8366 'WORKBOOK 40,260 test blocks'. 1B06BS log:1220 '0 test blocks' against
  1830 tests in its read-out. src\Federator.Core\Report\WorkbookCheck.cs:729-730 prints
  'Workbook: one sheet, N tests' from the same count.
- Root cause: src\Federator.Core\Report\WorkbookCheck.cs:196-265, ReadSheet counts a block only
  on a row whose Clash Name cell reads Clash Name (Blocks++ at :204 and :209), and since Q73 a
  test with no clash is one row with no such heading (src\Federator.Core\Report\WorkbookWriter.cs:227-231,
  written at :229 and :277). The count is handed to
  src\Federator.Addin\Engine\FederationEngine.cs:2779-2784 and
  src\Federator.Core\Clash\CreationPlan.cs:141-152, and printed again at WorkbookCheck.cs:680.
- Class: silent wrong number
- Proof: New Core test in tests\Federator.Core.Tests\Report\WorkbookCellCheckTests.cs or
  ReportCheckTests.cs: a workbook written with 2 tests that hold clashes and 3 that hold none
  reports 5 tests, and CreationPlan.BlockCountLine with that count says one for every test. It
  fails before, Blocks reads 2. A second test with a workbook missing a one row test names the
  shortfall. Run line of set 05: every WORKBOOK CHECK block reads 'BLOCKS 1830 in the workbook,
  one for every test in the file' (set 03 log:455).
- Note: The workbook is right and only the check is wrong. Because the check never counted one
  row tests it also could not name a real shortfall of them, which is the harm.
  WorkbookCheck.CheckOrder at :625-648 also sees only the full blocks. The independent reader
  tools\loop\read-workbook.ps1 already knows both shapes and can be the oracle. F77 and Q73.
  WorkbookCheck.cs is also read by FR-040.

### FR-036 skipped-test-row-keeps-old-tolerance-after-chosen-edit

On a run with a chosen tolerance, a saved test that is edited to that tolerance and then
skipped keeps the old tolerance and origin Document in its workbook block and in the TOLERANCE
counts, though the document no longer holds that value.

- Sources: T1-S3
- Evidence: src\Federator.Addin\Engine\ClashRunner.cs:465 'report.Tolerance =
  planned.Tolerance' and :467-469 'report.ToleranceFrom = plan.Source ==
  ClashPlanSource.Document ? ToleranceOrigin.Document : ToleranceOrigin.File'. :617
  ApplyChosenTolerance(clashTests, address, planned.Name) edits the test, then the skips at
  :692-703 (single discipline) and :707-722 (side check) return without touching the row. Only
  a test that ran reaches :856-857 'summary.Tolerance = after.Tolerance'.
  src\Federator.Core\Report\WorkbookWriter.cs:288 writes test.ClientTolerance() for every
  block.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:465-469, with the early returns at
  :692-703 and :707-722.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Clash\ToleranceChoiceTests.cs: a rule that
  gives a skipped row the chosen value in document units with origin Tool, for example
  ToleranceChoice returning both. It fails today because the row copies the plan. Run proof in
  set 05: a weekly run with a tolerance chosen (not in the standard five) on a single
  discipline group, then the workbook tolerance cell of every block reads the chosen value and
  the origin counts say Tool.
- Note: The XML path does the same for a test already in the document (:630 then a skip), where
  the row says File with the XML value. The same file, ClashRunner.cs, carries FR-047, FR-053,
  FR-063, FR-128 and FR-150. Not reached in set 03: no saved test existed, 'the document already
  holds 0 clash tests' in all 22 groups, and 'chosen in the tool for 0'.

### FR-037 priority-csv-repeated-test-name-last-row-wins-silently

A priority CSV that gives one test A on one line and C on a later line gives it C with no
problem written, and the logged row count is the number of distinct names and not of rows.

- Sources: T1-S26
- Evidence: src\Federator.Core\Clash\PriorityMap.cs:136 'map.byTestName[name] = priority' with
  no check for a name already held. Compare src\Federator.Core\Clash\ByDesignPairs.cs:181-185,
  'if (found.byKey.ContainsKey(pair.Key))' then the problem 'names the same pair as an earlier
  line'. PriorityMap.cs:60-64 RowCount returns byTestName.Count, and
  src\Federator.Addin\Engine\FederationEngine.cs:243-244 logs it as 'read <path>, N rows'.
- Root cause: src\Federator.Core\Clash\PriorityMap.cs:136, with :60-64.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Report\PriorityMapTests.cs: read two rows with
  one name and different letters and assert a problem is recorded and RowCount matches the file.
  It fails today because no problem is recorded and the last letter wins.
- Note: The workbook priority column, the block order and the viewpoint folder all follow the
  winning row, so nobody is told. Following ByDesignPairs, which keeps the first and names the
  repeat, would change which letter wins today. No Core test covers a repeated name. Not reached
  in set 03: 'priority file : none'.

### FR-038 workbook-stray-grey-row-after-last-empty-test

The one row form of an empty test borrows the two row header painter, so the row under it is
filled grey and boxed. The next block repaints it, but after the last block it stays as a stray
grey boxed row, and in the default order the last test is an empty one on almost every
workbook.

- Sources: T1-S59
- Evidence: src\Federator.Core\Report\WorkbookWriter.cs:229-230 'WriteEmptyTestRow(sheet,
  start, test, priority)' then 'return start + 1', and :279 'ClientStyle.TestHeader(sheet, row,
  ColumnTestHeader - 1, LastTestHeaderColumn)'. src\Federator.Core\Report\ClientStyle.cs:120-121
  fills top and top + 1 grey, :123 boxes both rows and :163 gives the second a Thick bottom.
  Set 03: the last block of 100000 is a one row test
  (workbooks\Clash_Report_C06_1104-PAR-100000-ZZZ-BM-MOD-000001.xlsx.txt:1836 'one row 1881
  BLD-EL-Data Devices-vs-BLD-EL-Telephone Devices'), but the read-out holds no fill or border,
  so the stray row cannot be seen in it.
- Root cause: src\Federator.Core\Report\WorkbookWriter.cs:279 with :229-230, painting through
  src\Federator.Core\Report\ClientStyle.cs:118-167.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Report\OneSheetWorkbookTests.cs
  (AnEmptyTestIsOneRowCarryingTheFiveFacts at :577 only checks that row + 1 has no Clash Name
  heading): write a report whose last test is empty, read the saved file back and assert the row
  below carries no fill and no border. Fails today.
- Note: Cosmetic, in the client's deliverable. The empty row's name cells carry no bottom edge
  (ClientStyle.cs:154-156), so at the end they read as merged into the stray row.

### FR-039 thumbnail-row-height-overwritten-by-clash-row-height

With the thumbnail box ticked, the 72 point row height set for a pasted 95 pixel picture is
replaced by 60 points as soon as the clash row is written, so the picture overhangs the next
row.

- Sources: T1-S60
- Evidence: src\Federator.Core\Report\WorkbookWriter.cs:594
  'sheet.Row(cell.Address.RowNumber).Height = ThumbnailPoints' (72.0 at :606, ThumbnailPixels
  95 at :604) is overwritten by :260 'sheet.Row(row).Height = ClashRowHeight' (60.0 at :189)
  after WriteClashRow returns at :247. EmbedThumbnail defaults to false
  (src\Federator.Core\Report\ImageOptions.cs:37). Set 03: thumbnails were linked and not pasted,
  'pictures stored in the workbook 0'.
- Root cause: src\Federator.Core\Report\WorkbookWriter.cs:260 overwriting :594.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Report\MatchClientReportTests.cs
  (TickingThumbnailsOnDoesPasteThem at :281 counts pictures only): with EmbedThumbnail on, read
  the saved row height back and assert it is 72. Fails today.
- Note: Off by default, so no default run is touched. Cosmetic.

## Report, FR-040 to FR-042

### FR-040 item-ids-guid-fallback-counted-missing

An item whose id came from the GUID fallback has a filled Item ID cell and an empty IdFrom, so
the ITEM IDS block counts it as a missing id, names 'no id property' as its source, says it was
written as Element ID while the cell says Instance GUID, and the report check calls a first cell
of that kind the wrong shape.

- Sources: T1-S56
- Evidence: src\Federator.Addin\Engine\ClashHarvest.cs:310 'into.ElementId =
  FirstProperty(lookIn, ElementIdNames, out idFrom)' leaves idFrom empty when nothing matched,
  :322 'into.IdFrom = idFrom', and :334-335 'into.ElementId = guid' and 'into.IdLabel =
  "Instance GUID"'. src\Federator.Core\Report\ClashReportModel.cs:726 'return item != null &&
  string.IsNullOrEmpty(item.IdFrom) ? 1 : 0' counts that filled cell missing, :736 'string from
  = string.IsNullOrEmpty(item.IdFrom) ? NoIdProperty : item.IdFrom', and :643-645 then log
  'written as Element ID'. src\Federator.Core\Report\ClientShapes.cs:29-30 builds the pattern
  for 'Element ID: ' and one more character, and src\Federator.Core\Report\WorkbookCheck.cs:516
  and PageCheck.cs:536 call a first cell of the Instance GUID kind the wrong shape. Set 03: all
  16 ITEM IDS lines read N of N (log:393), so no GUID fallback id was met.
- Root cause: src\Federator.Core\Report\ClashReportModel.cs:726 and :736, fed by
  src\Federator.Addin\Engine\ClashHarvest.cs:322 and :334-335.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Report\ClashReportTests.cs, which covers IdFrom
  present and absent only: a ClashItem with ElementId set, IdLabel Instance GUID and IdFrom
  empty is not counted missing and not described as written as Element ID, and the shape check
  does not call its first cell wrong. Fails today. A run cannot show it while every item has an
  id property.
- Note: The only caller of IdSourceLines is FederationEngine.cs:2678, the ITEM IDS block.

### FR-041 grid-location-empty-on-345-rows

The Grid Location cell is empty on 345 of 5679 clash rows (13 of 13 in 100000, 40 of 42 in
1B06K1, 292 of 695 in 1B06PP), and in 100000, where all 13 are empty, the HTML report loses its
Grid Location column and the REPORT CHECK says so.

- Sources: S03-13, S03-17, C06-J14, C06-J20, FIND-06, C06-DONE-14
- Evidence: workbooks\Clash_Report_C06_1104-PAR-100000-ZZZ-BM-MOD-000001.xlsx.txt:1839-1851, all
  13 Grid Location cells empty, and the 1B06K1 read-out at :1872 and :1873 'D-1 : RO1' on 2 of
  42. log:474 'Column 5 of the clash table is wrong. Ours reads Description and the client's
  report reads Grid Location', while the other 21 REPORT CHECK blocks end 'Nothing wrong with
  it.' (log:935). log:453 and log:2075 'The Item 1 Layer cell is empty'. log:5410, 1B06PP reads
  clean with 292 rows empty.
- Root cause: UNKNOWN why the grid is not found. The cell is set at
  src\Federator.Addin\Engine\ClashHarvest.cs:244-251 from grid.ClosestIntersection(centre),
  which gave nothing for these rows. In the 1B06K1 read-out the two filled cells are at z
  694.491 and the others at other heights, so height against the grid levels is the first thing
  to measure, and not a finding. The missing HTML column follows from
  src\Federator.Core\Report\ClashReportXml.cs:249-254, which leaves out an empty gridlocation
  element, and the stylesheet's showGridLocation reading //gridlocation
  (docs\history\scan.md:1364, tests\Federator.Core.Tests\Report\HtmlTabularTests.cs:172).
- Class: broken feature
- Proof: Run line of set 05: a new count line 'Grid Location empty on N of M rows' per group with
  N well under 345 of 5679, and the 100000 REPORT CHECK reads 'Nothing wrong with it.' (set 03
  log:474). No Core test can fail before until the cause is measured, by a probe printing
  ClosestIntersection for the 345 points. Once known, a Core test in
  tests\Federator.Core.Tests\Report\ClashReportXmlTests.cs pins what the writer does for a group
  with no grid.
- Note: core.md says write the data and let the stylesheet decide, so the column dropping when
  no row has a grid is Autodesk's own behaviour and the real fault is the missing data. The
  WORKBOOK and REPORT CHECKs read the first clash row only, so a count of empty grid cells per
  group is what would have named 1B06PP. ClashHarvest.cs is also changed by FR-032 and FR-076.

### FR-042 page-check-warns-wrong-order-on-priority-sorted-page

The page REPORT CHECK says the tests are in the wrong order on any page written in priority
order, because PageCheck does not know a priority file was picked and WorkbookCheck does.

- Sources: step NEW, the page test order warning (steps\loop.md:574)
- Evidence: src\Federator.Core\Report\PageCheck.cs:277-289, 'The tests are in the wrong order.
  Block N holds A clashes and block N+1 holds B' whenever a block holds more than the one
  before. FederationEngine.cs:2772 passes ThePriorities().Picked to WorkbookCheck and :2929
  calls PageCheck.Of(path) with nothing. steps\logs\run-20260920-162006.log:277 'PRIORITY read
  ...clash-priority-map.csv, 1830 rows' and :1303 'Block 15 holds 0 clashes and block 16 holds
  78', and the same four block numbers in run-20260919-211323.log. The C06 run picked none ('NO
  PRIORITY FILE was picked', log:421) and its 22 REPORT CHECK blocks carry no such line.
- Root cause: src\Federator.Core\Report\PageCheck.cs:277-289 and
  src\Federator.Addin\Engine\FederationEngine.cs:2929.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Report\MatchOriginalTests.cs beside the order
  cases at :388 and :424-432: a page whose blocks are in priority order with a priority file
  picked does not produce the wrong order line, and an unsorted page with none picked still
  does. Fails before.
- Note: Both committed C02 runs picked a priority file, which fits all seven groups, but that
  the cause is the same in every one of the seven is UNKNOWN from the logs alone. It never fails
  a group. T1-N82, in FR-171, is the same shape in WorkbookCheck.

## Run log and RESULT, FR-043 to FR-064

### FR-043 reshaped-group-done-but-nwf-never-saved

A group whose file list changed is reshaped and judged Rebuilt and DONE, but nothing on that
path saves the NWF, so unless the clash step or the viewpoints put something in, the NWD carries
the new file list and the NWF on disk keeps the old one.

- Sources: T1-S9
- Evidence: src\Federator.Addin\Engine\FederationEngine.cs:1529-1530 'They return TRUE now,
  which stops the fallback, and nothing on this path saves.' and :1573 'outcome.Decision =
  RerunDecision.Rebuilt'. :704-705 'outcome.NwfSize = SizeOnDiskOrMinusOne(job.NwfPath)' reads
  the old file. :959 'if (clashPutSomethingIn || viewsPutSomethingIn)' is the only later save,
  and :972-975 publishes the NWD either way. src\Federator.Core\Rerun\GroupJudgement.cs:189
  checks only AppendedCount for a Rebuilt group.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:1573 with :959 and :700-705, and
  src\Federator.Core\Rerun\GroupJudgement.cs:161-231, which holds no fact that this run saved
  the NWF.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Rerun\GroupJudgementTests.cs: add a GroupFacts
  fact for whether this run saved the NWF, then a Rebuilt group with that fact false and
  AppendedCount above 0 is not DONE. It fails today because the fact does not exist and the
  group is DONE. Run proof in set 05: the file gone run on a group that runs nothing (a single
  discipline group or one with no saved tests), reading the NWF byte size and model count on
  disk before and after. The standard five runs name a group with two or more disciplines, so
  such a group has to be chosen to show this line.
- Note: Narrow path. A reshaped group with two or more disciplines and saved tests runs them,
  RanCount is above 0, SaveTheNwfAgain runs and the NWF comes out right, so the fault hides
  behind that. The next run reads CHANGED again and reshapes again. Set 03 did not reach it
  (RESULT first run 22, rebuilt 0). The fix is a save after a successful reshape, which matches
  addin.md saying the NWF is saved over once every counted thing came back, plus the Core fact.
  One GroupFacts change serves FR-043, FR-044 and FR-045. GroupFacts and GroupJudgement are also
  changed by FR-001, so those pull requests meet in one file and go in order.

### FR-044 nwf-save-false-return-ignored-after-rebuild

When the NWF save after a rebuild returns false or throws, the group is still judged DONE and
the older NWF at that path is logged as written with its old size.

- Sources: T1-S10
- Evidence: src\Federator.Addin\Engine\FederationEngine.cs:2042-2045 'if
  (!document.TrySaveFile(job.NwfPath))' then log.Line('NWF      the save returned false for ' +
  job.Building) and nothing else. :2057-2058 'outcome.NwfSize = log.WriteFinished("NWF",
  job.NwfPath)' and 'outcome.NwfOnDisk = outcome.NwfSize >= 0'.
  src\Federator.Core\Diagnostics\RunLog.cs:1374-1380 WriteFinished only tests that a file is at
  the path. src\Federator.Core\Rerun\GroupJudgement.cs:11-116 GroupFacts holds no fact about the
  NWF save, unlike the NWD check at :213.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:2042-2058 (SaveTheNwf), with
  src\Federator.Core\Rerun\GroupJudgement.cs:11-116 and :161-231, which cannot see a failed NWF
  save.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Rerun\GroupJudgementTests.cs: with a new NWF
  save fact set from the TrySaveFile bool and the catch, break only that fact on an otherwise
  DONE Rebuilt group and assert the reason names the NWF save. It fails today because the fact
  does not exist and the group is DONE. Run line: not in the standard five, it would need a
  CHANGED group with its NWF made read only, and then RESULT and the group line name the failed
  save. Whether TrySaveFile returns false or throws on a read only NWF is UNKNOWN until that
  run.
- Note: Only the rebuild path is silent. On the Build path a missing file is caught by
  WriteFinished and the group fails with 'the NWF is not on disk'. The stale NWF only stays when
  the second save (FR-045) does not fix it. A throw is logged through log.Failure but the group
  is still DONE, and a false return leaves one plain line. ConfirmTheNwfSurvived
  (FederationEngine.cs:2795-2823) compares against the stale size it was handed, so it reports
  the NWF intact. Set 03 did not reach it: 44 NWF attempt lines, 44 NWF written lines, no
  'returned false' line, all groups built from scratch.

### FR-045 nwf-save-after-clash-work-false-return-ignored

When the second NWF save, after the clash work, returns false, nothing goes on the outcome, so
the sets, tests, statuses and viewpoints this run put in reach the NWD but not the NWF, and the
group can still be DONE.

- Sources: T1-S11
- Evidence: src\Federator.Addin\Engine\FederationEngine.cs:3152-3155 'if
  (!document.TrySaveFile(job.NwfPath))' then log.Line('NWF      the save after the clash work
  returned false for ' + job.Building) with no outcome.AddError. Only the catch adds one, at
  :3159-3160 'outcome.AddError("saving the NWF after the clash work threw ...")'. :3167-3168
  'outcome.NwfSize = log.WriteFinished("NWF", job.NwfPath)' and 'outcome.NwfOnDisk =
  outcome.NwfSize >= 0'.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:3152-3155 and :3167-3168
  (SaveTheNwfAgain).
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Rerun\GroupJudgementTests.cs with the same NWF
  save fact as FR-044, set from this call. It fails today for the same reason. A set 05 weekly
  run with the NWF made read only between the two saves is not part of the standard five, so
  the Core test is the proof.
- Note: A throw and a false return give two different answers for the same failed save: the
  throw adds an error and fails the group, the false return adds nothing. On an OPENED group
  this run never wrote that file before this save (FederationEngine.cs:724-725). Set 03 did not
  reach it: the second save wrote in all 22 groups, for example 4,169 bytes growing to 93,589
  for group 100000.

### FR-046 result-file-sizes-wrong-for-the-two-open-logs

The RESULT block prints the .tsv as 0 bytes and the .log as 678,363 bytes while the files held
1,499,250 and about 1,098,000, both files being held open for writing by the tool itself.

- Sources: S03-5, C06-J4, FIND-01, FIND-02, F81, T1-S38, C06-DONE-3
- Evidence: log:8582 'the .log so far: 678,363 bytes, read before this block finished writing'
  and log:8583 'the .tsv : 0 bytes, which keeps every line the .log collapsed'. record.txt:787,
  the monitor read the log at 1046020 bytes at 15:45:45, five seconds before. record.txt:1044,
  the .tsv is 1499250 bytes. record.txt:639, the log read 678363 bytes at 15:26:20, 19 minutes
  earlier. record.txt:809, the log ended at 1098049 bytes. log:8584 and outputs.txt:25, the copy
  is 1,097,850 bytes.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:2062 (SizeOnDisk reads new
  FileInfo(path).Length), called at :1948 and :1950 for the two files the same process still
  holds open. The loop's monitor reads the same log with a share aware FileStream length
  (tools\loop\run.ps1:584 at f38edd5, in Monitor) and was right. That FileInfo.Length of an open
  file lags on this disk is the likely cause and is not measured, so the mechanism is UNKNOWN
  until a test or probe shows it.
- Class: silent wrong number
- Proof: New Core test in tests\Federator.Core.Tests\Diagnostics\LogTrimmingTests.cs: write rows
  and lines, write the RESULT block, read the .log and .tsv through a share aware stream and
  assert the sizes the block prints match what the files held, and that the .tsv size is not 0
  when rows were written. It fails before only where FileInfo.Length lags, so on the Windows
  runner and not in the container. Run line of set 05: RESULT 'the .tsv' equals the toollog
  .tsv size, and 'the .log so far' is within the lines written after it of the copy's size (set
  03 log:8582 and 8583).
- Note: CLAUDE.md says a size is only logged after the real size is read back. Register row F81
  reads CONTRADICTED. Fix by reading the length through a stream opened with
  FileShare.ReadWrite. T1-S38 sits here as a side note and is FR-061, both placements right.

### FR-047 tsv-number-column-rounds-a-tolerance

The .tsv number column holds a clash tolerance rounded to three decimals, 0.082 for
0.0820209974 ft, in 3024 of 3215 test created rows, because two call sites pass a tolerance to a
formatter made for sizes and seconds.

- Sources: T1-S32 (placed by the findings reader and by a bug reader, folded here)
- Evidence: run-20261001-140037.tsv:78 'test created BLD-AR-Ceilings-vs-BLD-AR-Floors 0.082
  hard_conservative, 0.0820209974 ft is 0.0820209974 ft'. The 3024 rows are in the 16 groups
  whose document measures in ft, and the 191 rows of the six metre documents read 0.025 and
  lose nothing. log:306 shows the full value in the text line.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:1328 'EventRow.Exact(set)' and :1202
  'EventRow.Exact(wanted)' pass a tolerance to EventRow.Exact
  (src\Federator.Core\Diagnostics\EventRow.cs:198-202, 'A size or a duration, with three
  decimals and no separators', number.ToString("0.###", CultureInfo.InvariantCulture)). The
  fault is the two call sites and not Exact, whose four other callers are seconds.
- Class: silent wrong number
- Proof: New Core test in tests\Federator.Core.Tests\Diagnostics\EventRowTests.cs: a tolerance
  formatter that keeps the full value returns 0.0820209974 and 0.2460629921, and fails if 0.082
  comes back. It fails before since none exists and the two call sites call Exact.
  EventRowTests.cs:151 pins Exact at three decimals on purpose, so the new formatter is a
  separate member. Moving the call sites is add-in code, proved by the run line: the 'test
  created' rows of set 05 carry 0.0820209974 in the number column (set 03 tsv:78 reads 0.082).
- Note: Nothing in src reads these rows back, so the harm is to a person or script reading the
  .tsv. At :1328 the text column carries the full value, so only the number column is lossy. At
  :1202 the new tolerance is in that row only in rounded form, and from the sixth test of a
  group on NumberedRepeat writes no text line. ClashRunner already has a private Plain formatter
  of 12 decimals that could move to Core.

### FR-048 tsv-size-zero-for-nwc-not-on-disk

The .tsv row for an NWC that is not on disk says 0 bytes, a size that was never read, while the
text line says NOT ON DISK.

- Sources: T1-S39
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:1342 'long size = SizeOnDisk(file)' and
  :1348 'EventRow.Count(size < 0 ? 0L : size)', while the text line uses DescribeSize(size),
  which gives 'NOT ON DISK' for minus one. WriteFinished (:1376-1380) and CheckOnDisk write no
  size row for a missing file. The caller is src\Federator.Addin\Engine\FederationEngine.cs:1298
  log.AppendFinished(file, appended), once per file.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:1348.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\RunLogTests.cs: AppendFinished on a
  missing file, and assert the .tsv number column is not 0 (empty or a marker). It fails today.
  RunLogTests.cs:469-479 checks only the text line.
- Note: It breaks the rule that a size is only logged after File.Exists passes and the real size
  is read back, and the row says something the text log does not. Not reached in set 03: every
  NWC was on disk, SCAN found 67 readable.

### FR-049 second-run-in-window-result-covers-both-runs

If Run is pressed a second time in the same window, the RESULT and the timing block cover both
runs, because RunStarted only moves a mark and every list on the log keeps the first run's
entries, so the second RESULT counts the groups, errors, written files and clash total of both
presses.

- Sources: T1-S36, the register row step NEW, RESULT counts carry across Run presses
  (steps\loop.md:566)
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:482-490, RunStarted only sets
  runStartedAt = ElapsedSeconds and writes the line. The lists written (:44), failures (:45),
  groupRecords (:66), stepRecords (:77), ClashesFound (:113 and :1717) and collapsedLines (:644)
  are never cleared, and the only Clear calls are visitsInGroup (:1118, :1235) and censusFaults
  (:1236). src\Federator.Addin\FederatorPlugin.cs:46 starts one RunLog for the window and :93
  disposes it after ShowDialog. FederatorWindow.xaml.cs:2073 'RunButton.IsEnabled = true' after
  each run. RunLog.cs:1761 'TimingBlock.ForRun(GroupRecords, StepRecords, where.RunSeconds)'.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:482-490, with the lists it never resets at
  :44-45, :66, :77 and :644, and src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2073, which
  turns Run on again.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\RunLogTests.cs or
  ResultBlockInvariantTests.cs: two cycles of RunStarted, GroupFinished and RunFinished on one
  log, then assert the second RESULT counts only the second run's groups, files, errors and
  clashes. It fails today. If the fix goes in the window, a new log per run, a window run proves
  it instead.
- Note: Groups done, partial and failed, files written, errors, clashes found, the penetration
  and by design lines and the collapsed counts all carry over. The timing block divides both
  runs' group seconds by the second run's time, and waiting for the person then includes the
  whole first run. A penetration line also stays after a second run with the box off, because
  log.PenetrationsWanted is never set back. No test puts two runs through one log. Not reached
  in set 03: one RUN started and one finished.

### FR-050 run-started-but-not-finished-reads-as-no-run-marked

A scanned run that throws after RUN started gets a timing block and a RESULT that say no run was
marked, so every share is worked off the whole session including the time spent waiting for
the person.

- Sources: T1-S37
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:514-516 return RunClock.NotMarked when
  runStartedAt or runFinishedAt is below 0, and RunClock.From otherwise.
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1973 log.RunStarted(jobs.Count) and :2056
  log.RunFinished() are both inside the try, and the finally at :2067-2074 writes the RESULT.
  src\Federator.Core\Diagnostics\RunClock.cs:115-116 then writes 'no run was marked, so the run
  time below IS the session time.' and RunLog.cs:1938 adds ', which is the session, no run was
  marked'.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:514-516, with the call at
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2056 inside the try.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\LogTrimmingTests.cs: call
  RunStarted and then WriteResultBlock with no RunFinished, and assert the block does not say no
  run was marked. It fails today. LogTrimmingTests.cs:164-177 covers only a run that never
  started.
- Note: PickedExchange at FederatorWindow.xaml.cs:1977 reads the picked XML and can throw between
  the two marks. A second run in the same window that throws behaves differently: runFinishedAt
  still holds the first run's finish, so the block says a run was marked and gives it 0
  seconds, which ties to FR-049. Not reached in set 03: both marks were set.

### FR-051 result-states-zero-waiting-when-no-run-marked

Every open file run and every hand button press ends with a RESULT that states 'waiting for the
person : 0.000s', a number nobody measured, beside a run time that already counts the waiting.

- Sources: T1-S40
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:1939-1941 writes 'waiting for the person :
  ' with spent.WaitingSeconds and 's, which is not work this tool did' every time.
  src\Federator.Core\Diagnostics\RunClock.cs:45 'StartedAt = marked ? Never(started) : 0.0', so
  WaitingSeconds is 0 when not marked. RunClock.Lines at :113-118 leaves that row out when not
  marked, and RunLog.cs:1937-1938 adds ', which is the session, no run was marked' to the run
  time line only.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:1939-1941.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\LogTrimmingTests.cs: WriteResultBlock
  with no RunStarted, and assert the waiting line is absent or says UNKNOWN. It fails today.
  LogTrimmingTests.cs:164-177 checks the unmarked RESULT but not this line. The set 05 open
  document run marks no run and shows the line.
- Note: Set 03 was a marked run, 'waiting for the person : 17.982s' at log:8580, so the unmarked
  line did not occur.

### FR-052 notolerance-skip-missing-from-skip-reasons

A test skipped for having no tolerance attribute is counted in tests skipped but has no reason
row and no SKIPPED group, so the reason rows add up to less than the total.

- Sources: T1-S25
- Evidence: src\Federator.Core\Clash\ClashRunOutcome.cs:429-440 SkipReasonsInOrder lists
  SingleDiscipline, EmptySide, LocatorNotResolved, UnknownTestType, NoLocator, UnknownUnits,
  NoName and Failed, not NoTolerance. src\Federator.Core\Clash\ClashSkipReason.cs:26 declares
  NoTolerance and ClashTestPlan.cs:249 adds it for a test with no tolerance attribute.
  ClashRunner.cs:364-366 'outcome.AddSkipped(test)' for every resolved.Skipped, and
  ClashRunOutcome.cs:281 'tests skipped     : ' + SkippedCount counts it.
- Root cause: src\Federator.Core\Clash\ClashRunOutcome.cs:429-440.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Clash\ClashRunOutcomeTests.cs: add one
  NoTolerance skip and assert SkipLines names it and the reason rows sum to SkippedCount, or loop
  every ClashSkipReason value. It fails today.
  SingleDisciplineGroupTests.EverySkipReasonStillHasWordsForIt checks Describe only, and no test
  puts a NoTolerance skip into an outcome.
- Note: LogSkip still names the first five as they happen, because it uses Describe, which knows
  NoTolerance (ClashTestPlan.cs:447). Not reached in set 03: no NoTolerance skip, and every
  group's reason rows added up to its skipped total. T1-S25 also sits on FR-031 as a side note,
  both placements right.

### FR-053 tolerance-set-on-count-counts-tests-not-edited

The TOLERANCE line says the chosen tolerance was set on every saved test it counted, but the
count goes up before the test is resolved, so a test that was not found, would not copy as a
clash test, threw, or was already at the value is counted as set.

- Sources: T1-S30
- Evidence: src\Federator.Addin\Engine\ClashRunner.cs:1152 'toleranceOnExisting++' is the first
  line of ApplyChosenTolerance, before 'if (!Tolerance.ChosenInTheTool)'. Early exits: :1165-1168
  existing == null returns with no line, :1172-1175 already within TestDrift.ToleranceEpsilon,
  :1181-1187 the copy is not a ClashTest, :1206-1211 a throw. Only :1189-1190 'copy.Tolerance =
  wanted' and TestsEditTestFromCopy set it. src\Federator.Core\Clash\ToleranceChoice.cs:206-208
  'Set on ' + total + ' tests, '.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:1152.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Clash\ToleranceChoiceTests.cs: a tally with one
  count per outcome (set, already at the value, not found, would not copy, threw) and a LogLine
  that says them apart, asserting 'Set on N' counts only the first. It fails today because
  LogLine takes two counts. Counting at each return is add-in work, seen in a set 05 weekly run
  with a tolerance chosen twice, where the second run says 0 set and N already at the value (not
  in the standard five).
- Note: A test already at the value does carry the chosen tolerance, so counting it as set is
  arguable, but the unresolved case writes nothing at all. Not reached in set 03: 'chosen in the
  tool for 0'.

### FR-054 log-prune-in-temp-fallback-deletes-other-programs-logs

When the logs folder cannot be opened, the log falls back to the shared system temp folder and
prunes every run-*.log there past 30, which can delete another program's files, and the .tsv
files beside it are never pruned.

- Sources: T1-S35
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:268 'foreach (string folder in new[] {
  preferredFolder, System.IO.Path.GetTempPath() })' reaches :277 'return Start(folder,
  startedAt, keepLogs)', which ends at :358 log.PruneOldLogs(folder, keepLogs). :376 'all = new
  DirectoryInfo(folder).GetFiles(LogFileSearchPattern)' with :34 LogFileSearchPattern =
  FileNamePrefix + '*' + FileNameExtension, which is run-*.log, and :411-415 delete every one
  past the newest keepLogs minus one. The comment at :30-33 says nothing else in the folder is
  touched.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:268 and :358, with the pattern at :34.
- Class: broken feature
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\RunLogRetentionTests.cs: make the
  preferred folder unusable, put a foreign run-other.log among 35 in the fallback folder, and
  assert it survives. RunLogRetentionTests.cs:260 passes its own folder as the preferred one, so
  the bare system temp folder is never tested, and a seam for the fallback folder is needed for
  the test.
- Note: Only on the fallback. Whether any program on the 27 machines writes run-*.log into the
  temp folder is UNKNOWN. Not reached in set 03: the preferred logs folder opened and 'RETAIN
  keeping 30 logs, deleted 1, could not delete 0' is at log:3, which removed the oldest log in
  Bader's own folder as Q82 allows. PruneOldLogs is also FR-055, so one pass.

### FR-055 tsv-files-never-pruned

Log retention keeps 30 run-*.log files and never touches the .tsv written beside each one, so
the .tsv files grow without limit on every machine.

- Sources: T1-B5
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:34 'public const string
  LogFileSearchPattern = FileNamePrefix + "*" + FileNameExtension' with FileNameExtension .log at
  :22, and :376 'all = new DirectoryInfo(folder).GetFiles(LogFileSearchPattern)'.
  src\Federator.Core\Diagnostics\RowLog.cs:80 writes GetFileNameWithoutExtension(logPath) +
  EventRow.FileNameExtension (.tsv) beside it. Set 03: log:3 'RETAIN keeping 30 logs, deleted 1,
  could not delete 0', and the run's .tsv is 1,499,250 bytes (record.txt:1044). UNKNOWN whether
  the pruned log had a .tsv, logs-before.txt is not in the evidence folder.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:34 and :368-434 (PruneOldLogs, the
  listing at :376).
- Class: broken feature
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\RunLogRetentionTests.cs, which
  covers .log files only: a temp folder holding forty old run logs each with its .tsv, Start
  with keep 30, at most 30 .tsv remain and the live one is kept. Fails today. Run: the RETAIN
  line of a set 05 run, and an outputs listing of the logs folder.
- Note: The same function as FR-054, so one pass. About 1.5 MB per run, so the growth is slow.
  Once fixed, the older .tsv files of Bader's pruned logs go with them, which follows Q82.

### FR-056 live-line-slower-compares-one-test-with-whole-group

The live line's SLOWER check compares one test's seconds with twice the sum of every visit of
the same step on the group before, so for the three per test steps one test would have to
outlast the group before's whole step to trigger it.

- Sources: T1-S34
- Evidence: src\Federator.Core\Diagnostics\LiveLine.cs:231-241, OnTheGroupBefore adds
  record.Seconds over every record of the group before and returns the sum, and :172 'return
  lastGroupSeconds > 0 && SecondsOnStep() > lastGroupSeconds * SlowerThan'.
  src\Federator.Core\Diagnostics\RunLog.cs:931 adds one StepRecord per visit.
  src\Federator.Addin\Engine\ClashRunner.cs:732 StepStarted(RunSteps.TestsRun) runs once per
  test and LiveLine.cs:127-133 StepStarted resets stepStartedAt, so SecondsOnStep is one test.
  FederationEngine.cs:146 'this.live.PaceReader = OnTheGroupBefore'.
- Root cause: src\Federator.Core\Diagnostics\LiveLine.cs:231-241 (OnTheGroupBefore), fed once
  per test from src\Federator.Addin\Engine\ClashRunner.cs:636, :732 and :865 through
  src\Federator.Addin\Engine\FederationEngine.cs:146 and :1038-1043.
- Class: broken feature
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\LiveLineTests.cs: give
  OnTheGroupBefore a group with many visits of one step and assert what a single visit is
  compared with, a per visit figure. It fails today. LiveLineTests.cs:283-294 pins the sum (10
  plus 15 gives 25) and changes with the fix.
- Note: Steps opened once per group (NWD, APPEND) compare like with like and are fine. The live
  line text is in no file of set 03, so it is UNKNOWN whether SLOWER ever showed. C06-J18,
  FIND-15 and C06-DONE-12 (steps over twice the same step on the group before) are left out as
  not a fault for that reason, the SLOWER text being a window line only.

### FR-057 run-log-write-has-no-try

RunLog.WriteRaw has no try around the file write, the two flushes or the LineWritten handler,
so a full disk or a window handler that throws can stop the run from a log line, and the
failure lines that would report it hit the same write.

- Sources: T1-L6
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:565-571 'writer.WriteLine(line)',
  'writer.Flush()' and 'stream.Flush(true)' inside 'lock (gate)' with no try, and :579
  'handler(line)' with none. src\Federator.Core\Diagnostics\RowLog.cs:152-156 wraps the same
  kind of write with 'A disk that filled up must not stop a run here'. core.md says logging is
  never the thing that stops a run. The fall back to temp and to window only exists at open time
  (StartOrDisabled :263-298). The one handler is
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:115-119 OnLogLine (LogBox.AppendText). No test
  in tests\Federator.Core.Tests\Diagnostics\RunLogTests.cs subscribes a handler that throws. Set
  03 wrote 8585 lines with no write failure.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:557-581 (WriteRaw).
- Class: loud failure
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\RunLogTests.cs: subscribe a
  LineWritten handler that throws, call Line, and assert it returns and the line is on disk.
  Fails today. The full disk half is UNKNOWN to reproduce in a test.
- Note: FR-061 (T1-S38) is the matching swallow in the row log. Keep the order: write first,
  then the handler inside its own try.

### FR-058 first-clash-line-says-all-tests-to-create

Each group's first CLASH line says all 1830 tests are to be created and the line after it says
far fewer were, two counts of the same thing.

- Sources: S03-15, C06-J22
- Evidence: log:289 'CLASH 100000, 1830 in the file, 1830 to create, 0 skipped before the model'
  then log:297 'CLASH 1830 in the file, 36 created, 1794 not created, a side finds nothing'. The
  second agrees with the CLASH block and the workbook.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:2496-2497 writes
  plan.Buildable.Count as 'to create' before the creation plan runs. The second line is
  src\Federator.Core\Clash\CreationPlan.cs:129-133.
- Class: noise
- Proof: Run line of set 05: the first CLASH line of each group no longer says 'to create' with
  the full count (set 03 log:289) or reads the same number as the line after it. A Core test
  only if the sentence moves into Federator.Core.Clash.ClashTestPlan, in
  tests\Federator.Core.Tests\Clash\ClashTestPlanTests.cs.
- Note: Wording only. FR-126 is the related one discipline sentence.

### FR-059 result-files-written-leaves-out-pictures

RESULT says 88 files written while the run wrote 5790, leaving out 5679 pictures, 22 logos and
the log copy.

- Sources: S03-18, C06-J28, C06-DONE-15
- Evidence: log:8466 'files written : 88, every size read back off the disk'. outputs.txt lists
  5790 files: 88 NWF, XLSX, HTML and NWD, 5679 _files\cd pictures, 22 logo.jpg and the log copy
  (outputs.txt:25). Every one of the 88 sizes matches outputs.txt, so the line is right for what
  it counts. The pictures add to 1376 MB.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:1850-1865 lists only what
  RunLog.WriteFinished recorded (:1372-1422). src\Federator.Addin\Engine\ClashImages.cs:188
  counts a picture in the per group image tally and never in the written list, and the logo copy
  (FederationEngine.cs:2987) and the log copy (RunLog.cs:1999) are not recorded either.
- Class: noise
- Proof: New Core test in tests\Federator.Core.Tests\Diagnostics\WrittenSizeTests.cs: after
  pictures are tallied for the run, RESULT states how many pictures and how many MB besides the
  files it lists. Fails before. Run line of set 05: RESULT names 5679 pictures and their size
  beside the 88 files (set 03 log:8466 names none).
- Note: The rule that a file this tool did not write is never listed still holds. The fix is a
  picture count line and not 5679 listed paths.

### FR-060 result-found-none-counts-groups-that-ran-no-test

The RESULT line '6 of which found none' counts the six one discipline groups where no test ran,
so it reads like six clean groups.

- Sources: C06-DONE-2
- Evidence: log:8441 'clashes found : 5679 across 22 groups, 6 of which found none'. The six
  rows read '0 from 0 test(s) that found something' (log:8444, 8447, 8451, 8457, 8458, 8463)
  and their blocks read 'tests run : 0' (log:1171, 2332, 3739, 6005, 6300, 8102). Their
  workbooks list all 1830 tests with a blank status.
- Root cause: src\Federator.Core\Clash\ClashesAcrossTheRun.cs:30-35 (Add carries no count of
  tests run), :59-76 and :95-99 (a group with zero clashes is counted as found none whether or
  not any test ran). Called at src\Federator.Addin\Engine\FederationEngine.cs:2619.
- Class: noise
- Proof: New Core test in tests\Federator.Core.Tests\Clash\ClashesAcrossTheRunTests.cs: a group
  that ran 0 tests is counted as 'ran no test' and not under 'found none'. Fails before. Run line
  of set 05: the RESULT line reads 'N of which ran no test' for the six (set 03 log:8441).
- Note: The arithmetic is right, the wording is the fault.

### FR-061 log-says-tsv-keeps-collapsed-lines-when-tsv-did-not-open

When the .tsv did not open or a write to it failed, the text log still says every collapsed line
is in the .tsv.

- Sources: T1-S38
- Evidence: src\Federator.Core\Diagnostics\RunLog.cs:687-688 'every further line of this kind is
  counted and not written out. The machine readable log carries every one of them', :718 'Every
  one is in the machine readable log' and :1922 'lines collapsed in this file, all of them kept
  in the .tsv beside it:' are written without checking the row file. :743-746 Row returns when
  its RowLog is null. src\Federator.Core\Diagnostics\RowLog.cs:107-111 StartBeside returns a
  RowLog with no writer when the .tsv will not open, :138-141 WriteLine returns on writer ==
  null, and :152-156 swallows a write that throws.
- Root cause: src\Federator.Core\Diagnostics\RunLog.cs:687-688, :716-718 and :1922.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\LogTrimmingTests.cs: put a folder at
  the .tsv path before Start, write six NumberedRepeat lines with one key plus the RESULT block,
  and assert neither claim appears. It fails today.
- Note: The log contradicts itself rather than hiding the problem: the start line from
  RowLog.WhereItIs (RowLog.cs:115-121) says there is no machine readable log, and RunLog.cs:1950
  prints NOT ON DISK for it. Not reached in set 03: the .tsv opened and held every collapsed row.
  RESULT printing the .tsv as 0 bytes in set 03 (log:8583) is FR-046, where T1-S38 also sits as
  a side note, both placements right.

### FR-062 units-line-names-unit-when-second-read-threw

When the second read of Document.Units throws, the UNITS line still says the document shows the
units it had before, which reads like a measurement that Document.Units did not follow.

- Sources: T1-S8
- Evidence: src\Federator.Addin\Engine\DocumentUnits.cs:118 'Units after = before', :124-127
  'catch (Exception) { // The line below still says what was attempted. }' and :129-132, the
  UNITS line naming how many models were set, how many would not, and 'document shows ' +
  Name(after). FederationEngine.cs:941 throws the return value away, so only the log line is
  affected.
- Root cause: src\Federator.Addin\Engine\DocumentUnits.cs:118-127 and :129-132.
- Class: noise
- Proof: New Core test in tests\Federator.Core.Tests\Units\: the UNITS line built in Core from a
  value that was not read says UNKNOWN and names no unit. It fails once the line moves to Core,
  so the test comes with that move.
- Note: Whether Document.Units follows the model units is exactly what the class says is
  UNKNOWN. The report is not affected, because ReportUnits converts to metres anyway. The empty
  catch also breaks the rule against a catch that swallows. Set 03: Apply ran 22 times and the
  line read Feet or Meters as expected with no sign of a throw, but the catch writes nothing, so
  it is UNKNOWN whether any second read threw. DocumentUnits.cs is also FR-134's file.

### FR-063 tolerance-log-line-says-read-from-xml-on-no-xml-run

On a run with no XML picked every group logs that the tolerance was read per test out of the
XML, then says N were left as the document has them, so the first half is false.

- Sources: T1-S29
- Evidence: src\Federator.Core\Clash\ToleranceChoice.cs:200-203, the prefix then ' read per test
  out of the XML, which is what this tool does unless a tolerance is chosen on the Clash step. '
  and later ' left as the document has them'. src\Federator.Addin\Engine\ClashRunner.cs:396
  WriteTheToleranceLine() runs on every group, and :1225
  log.Line(Tolerance.LogLine(toleranceOnCreated, toleranceOnExisting, documentUnits)). LogLine
  takes no plan source. On the no XML path ClashRunner.cs:610-617 'if
  (planned.IsFromDocument)' goes to ApplyChosenTolerance.
- Root cause: src\Federator.Core\Clash\ToleranceChoice.cs:198-204, called from
  src\Federator.Addin\Engine\ClashRunner.cs:396 and :1225.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Clash\ToleranceChoiceTests.cs: a LogLine told the
  plan source is the document does not say 'read per test out of the XML'. It fails today
  because LogLine takes no source. Then the set 05 weekly run, which picks no XML, shows the
  TOLERANCE line of each group without that phrase.
- Note: The log is the evidence Bader reads. Set 03 picked an XML, so the wording was true in
  all 22 lines (log:349) and the no XML path was not run.

### FR-064 category-line-names-no-folder

The HEALTH lines 'Revit categories known: 374' and 'Sets asking for a category no model carries:
N' do not say the 374 values were measured on the C02 folder only, so a count against another
folder can be read as a count of broken sets.

- Sources: Q56 (steps\loop.md:537)
- Evidence: steps\02_questions.md:256, Bader's answer '56b is a: the block SAYS WHICH FOLDER the
  list was measured from'. src\Federator.Core\Exchange\RevitCategories.cs:108-120 Line() prints
  only the count, and src\Federator.Core\Health\HealthCheckResult.cs:215-216 prints the no model
  carries count. tests\Federator.Core.Tests\Health\SetWarningsTests.cs:151 pins the line without
  a folder. The measured folder is only in the header of
  src\Federator.Core\Exchange\revit-categories.txt:3-8.
- Root cause: src\Federator.Core\Exchange\RevitCategories.cs:108-120 and
  src\Federator.Core\Health\HealthCheckResult.cs:215-216.
- Class: noise
- Proof: In tests\Federator.Core.Tests\Health\SetWarningsTests.cs, change the assertions at :151
  and :343 so RevitCategories.Line() and HealthCheckResult.Summary() carry the folder the list
  was measured on, read from the resource header and not typed into code. Fails before. Set 05
  line: a HEALTH block naming the folder.
- Note: HEALTH is written only when the XML is picked through the Browse dialog, and the driver
  types the path, so set 03 has no HEALTH block (the register row for steps 35-37). The proof run
  picks through Browse to show it. The name comes from the data file, because CLAUDE.md says
  nothing in the code names one project's file. HealthCheckResult.cs is also FR-023's file.

## Views, FR-065 to FR-074

### FR-065 viewpoint-dimming-carries-between-viewpoints

A viewpoint whose two clashing items are not both placed is recorded with whatever dimming and
red and green paint the previous viewpoint left and is counted as written undimmed, and models
dimmed for an earlier viewpoint stay dimmed while hidden for later ones.

- Sources: T1-S22
- Evidence: src\Federator.Addin\Engine\ViewpointBuilder.cs:668 'if (views.DimsAnything && place
  != null && place.BothPlaced)' is the only block that touches temporary materials, and Undim is
  called only at :702 when its own dim failed and at :823 at the end of the group. :786-789
  'else { notDimmed++ }' counts the skipped case, logged at :223 as 'written undimmed'.
  src\Federator.Addin\Engine\SavedViewpoints.cs:221 'view.ApplyMaterialAttribs = true' on every
  record, :433-441 dim only RootsOf(document, shown), and :362 ShowOnlyModels resets hidden
  state only.
- Root cause: src\Federator.Addin\Engine\ViewpointBuilder.cs:668-708 and :782-789, with
  src\Federator.Addin\Engine\SavedViewpoints.cs:221 and :433-441.
- Class: silent wrong number
- Proof: Navisworks only. Set 05 first run, VIEWS block: the material override count of an
  undimmed viewpoint and of viewpoints after a pair change, and the NWF byte size of group
  1B06G1 (set 03 wrote 29,834,756 bytes for 1165 viewpoints, log:8479), before and after an
  Undim at the top of each viewpoint. The read back at ViewpointBuilder.cs:760 only checks a
  count above zero, so a log line that prints the count per viewpoint has to exist for this line
  to be shown, which is UNKNOWN today.
- Note: Whether a new root transparency replaces an earlier leaf colour is UNKNOWN.
  SavedViewpoints.cs:433-437 says this is the shape that put 33 MB into a 120 KB NWF. An Undim
  per viewpoint costs time in the step that is already 72 percent of the run, so the fix meets
  FR-069. Not reached in set 03: planned, dimmed, painted and created were equal in every group
  and no 'written undimmed' line exists.

### FR-066 size-text-reads-tail-of-word-digits

A size written inside a word is read as its tail instead of being refused. 'DN150 mm' reads as
50 mm and 'DN200 mm' as 0 mm, so a 200 mm service can be taken as at or under the threshold,
kept out of the viewpoint tree, and moved to Reviewed by the penetration rule when that box is
on.

- Sources: T1-S70
- Evidence: src\Federator.Core\Views\SizeText.cs:204 'return at == 0 || !IsLetter(text[at - 1])
  || IsSeparator(text[at - 1])', and :50-53 move one character on and carry on when
  IsNumberStart is false. Walking 'DN150 mm': the 1 follows N and is refused, the walk moves one
  character, the 5 follows a digit and starts a number, and '50' then ' mm' gives 50. The
  comment at :201-203 says a digit after a letter is inside a word already being read. Used at
  src\Federator.Addin\Engine\ItemSizes.cs:118
  'SizeText.LargestMillimetres(data.ToDisplayString())', read by Penetrations.cs:328 and by
  ViewpointBuilder.cs:393 through Penetrations.ServiceSizeOf.
  tests\Federator.Core.Tests\Views\SizeTextTests.cs has no case of digits after a letter. Set 03:
  879 clashes in 13 groups were set aside as 'a service at or under the threshold' (log:882,
  1677) and no line gives a per item size, so whether any was read as 0 is UNKNOWN.
- Root cause: src\Federator.Core\Views\SizeText.cs:204, with the walk at :50-53.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Views\SizeTextTests.cs:
  SizeText.Millimetres("DN150 mm") is empty and LargestMillimetres("DN200 mm") is null. Today
  they give 50 and 0. Fails today.
- Note: Whether any model writes a size property shaped like DN150 mm is UNKNOWN, the real
  strings in the tests are '53 mmø' and '600 mmx100 mm'. The design already says a size it
  cannot read is refused so the clash is left for a person. The viewpoint rule is on by default,
  so this reaches the tree.

### FR-067 clear-rebuild-drops-viewpoints

The clear and rebuild fallback copies the sets and the tests out and back but not the saved
viewpoints, so a group that carries viewpoints can never succeed on that path and the NWF is
left as it was.

- Sources: Q77 (steps\loop.md:539)
- Evidence: steps\02_questions.md:418, Q77 'saved viewpoints LOST: before clear 52, after appends
  22, after restore 22' and REFUSED TO SAVE. src\Federator.Addin\Engine\FederationEngine.cs:1742-1743
  copy only testsCopy and setsCopy. :1805-1810, the comment says the viewpoints 'ride back inside
  the two copies above', which Q77 found false. Only the count is taken, at :1721 and :1811.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:1742-1743 and :1805-1812.
- Class: broken feature
- Proof: No Core test reaches it (Navisworks only, and RebuildTally already has tests). Proof run
  line: a forced clear and rebuild on a small copy, the log VIEWS tally reading 'saved
  viewpoints' before equal to after restore, and then the line that the NWF was saved over. Set
  05 reshapes and does not take the fallback, so it needs this one extra forced run.
- Note: Q77 has no answer written under it, but the register class is open fault, and the rule
  that the NWF is the record already answers it. Whether DocumentSavedViewpoints.CreateCopy and
  CopyFrom work for this is measured first, as the row says, UNKNOWN until then. The viewpoint
  count before the clear goes through RebuildTally, whose minus one fault is FR-151.

### FR-068 size-tally-never-constructed

The SIZE block that names every clash side whose size could not be read is never written.
SizeTally is a public class nothing in src builds, so SizeSettings.NameEveryUnknown and
ExamplesWhenNotNamingEvery change nothing, and the live VIEWS block only counts.

- Sources: T1-B9, step 247 (steps\loop.md:584)
- Evidence: A grep of src for SizeTally finds only src\Federator.Core\Views\SizeTally.cs
  (declared at :16), and its settings are read only at SizeTally.cs:111 'bool everyOne =
  settings.NameEveryUnknown' and :114. Only tests\Federator.Core.Tests\Views\SizeTallyTests.cs
  reads it. src\Federator.Core\Views\ClashViewpointPlan.cs:267 writes 'size could not be read :
  ' + SizeUnknownCount and :413-415 only adds one to a count. core.md says every one of them is
  named in the SIZE block. steps\03_bader_next.md:473, step 247, already records that no SIZE
  block is written on any run. No question in steps\02_questions.md keeps the class. Set 03:
  'size could not be read : 0' in all 22 groups (log:419), so nothing needed naming.
- Root cause: src\Federator.Core\Views\SizeTally.cs:16-129, nothing in src constructs it. Naming
  would belong in the plan outcome at src\Federator.Core\Views\ClashViewpointPlan.cs:267 and
  :413-415.
- Class: broken feature
- Proof: If wired: a Core test in tests\Federator.Core.Tests\Views\ClashViewpointPlanTests.cs or
  SizeTallyTests.cs that feeds the plan outcome SizeUnknown clashes and asserts each one is
  named, failing today, and a set 05 VIEWS block line, though with 'size could not be read : 0'
  in every group nothing shows, UNKNOWN. If deleted: a grep for SizeTally over src and tests
  reads 0, the build at 0 errors and 0 warnings, and the Core count falls by the count in
  SizeTallyTests.cs.
- Note: Two written rules pull opposite ways. core.md says name every one, and CLAUDE.md says
  delete a public member nothing in src calls unless a decision keeps it. Wiring follows the
  written core.md rule. One reader calls deleting the written house rule, the other would put a
  delete to the form, and either way core.md changes to match. Wiring needs the clash name
  carried to the plan outcome, because SizeTally.Add takes a SizeDecision from SizeRule.Decide
  while the viewpoint path keeps only a SizeVerdict. It is also among the 150 of FR-172.

### FR-069 views-cost-per-viewpoint

VIEWS is 71.9 percent of the run, 4523.564 s of 6292.198 s, and recording a viewpoint costs
1.376 s in 1B06G1 against 0.0075 s in 1B06M1, its 0.150 s of recording for 20 at log:2675, so
three groups take 4768 s, 75.8 percent of the run, and the run took 1 h 44 min 52 s against 45
minutes.

- Sources: S03-7, S03-8, C06-J6, C06-J7, FIND-08, FIND-14, LOOK 387, C06-DONE-9, C06-DONE-11,
  P2 (steps\loop.md:606), NEW-LOG the viewpoints step does not scale (steps\loop.md:607), Q98 B3
- Evidence: log:8408 'VIEWS 4523.564s 71.9% 22 visits'. log:1663 'the step's seconds went:
  0.201s looking whether each was already there, 1.986s dimming, 1602.798s recording, 96.899s
  reading back' with log:1669 'STEP VIEWS finished 1769.389s 1165 created' for 1B06G1. log:4480
  and log:4486, 1B06PK 1174.927 s recording, 1421.383 s for 1581. log:5343 and log:5349, 1B06PP
  479.409 s recording, 693.614 s for 549. log:2675, 1B06M1 0.150 s recording, and log:2681, 0.525 s
the step for 20. log:8380-8382, the
  three groups are 75.8 percent. log:8426 'The run took 1 hour 44 minutes 52 seconds, 6292.198s.
  That is OVER the 45 minutes 0 seconds a run has to finish in, by 59 minutes 52 seconds.'
  findings.md:17-18 'Without it the run would have taken 29 min 29 s'. LOOK 387 expects a VIEWS
  step of a few seconds and reads CONTRADICTED.
- Root cause: UNKNOWN which call. The time is in recording each viewpoint:
  src\Federator.Addin\Engine\ViewpointBuilder.cs:710-713 times SavedViewpoints.EnsureFolders and
  SavedViewpoints.Record together as recording, 91 percent of the step in 1B06G1. Inside,
  src\Federator.Addin\Engine\SavedViewpoints.cs:202-264 adds a COM view with ApplyHideAttribs
  (:213) and ApplyMaterialAttribs (:221) both on, so every viewpoint carries one material
  override per dimmed item. With RecordsThroughTheFolder true, its default at
  src\Federator.Core\Views\ViewpointSettings.cs:107, which nothing in src changes, the view goes
  into the folder found by FindComFolder and Record returns at :236, so AddCopy (:256) and Remove
  (:259) do not run. The folder resolves per viewpoint (:109, :129, :299) read every child of a
  folder through a new wrapper each time (:784-825, :902-920). docs\history\scan.md 5p measured 558 ms a
  viewpoint in a big group against 7.9 ms in a small one and said what grows is the tree the
  write walks.
- Class: slow
- Proof: Run line of set 05: 'STEP VIEWS finished' for 1B06G1, 1B06PK and 1B06PP with the same
  created counts (1165, 1581, 549) and fewer seconds than log:1669, log:4486 and log:5349, the
  TIMING 'by step across every group' VIEWS line below 4523.564 s (log:8408), and the viewpoints
  the same, the read back lines (the log:4479 to 4482 pattern) with equal counts.
  tests\Federator.Core.Tests\Views\ClashViewpointPlanTests.cs passes unchanged, to show the same
  viewpoints are planned. No Core test for the cost, which sits in Navisworks calls.
- Note: Q98 B3 decides: VIEWS faster with the same viewpoints, measured before and after.
  Measure first: split the recording watch (ViewpointBuilder.cs:90, :710-713) into
  EnsureFolders, FindComFolder and the COM add, so the next run says which call, and account
  for the seconds no part holds (FR-073). The register row said three read backs a viewpoint,
  and the run shows recording is about 94 percent of the accounted VIEWS seconds in 1B06G1 and
  reading back 6. VIEWS falls to about 930 s for the whole run to fit in 45 minutes. What the 45
  minutes counts and what the viewpoints may hold were Bader's, FR-070, answered on 2026-10-04,
  Q101, and this speed work goes
  first. LOOK 387 reads CONFIRMED only for a step the size of the fixtures, so its wording is
  the lead's to check. FR-065 and FR-071 change the same loop.
- Answered by Bader on 2026-10-04 at 15:23, Q101, in short, his words being under the question in steps\02_questions.md: make VIEWS faster with the same viewpoints. The lead's note: to be worked by F114, the views area, in wave 2, after wave 1's test run measures the split of FR-073.

### FR-070 views-45-minute-basis-and-options

The run took 1 h 44 min 52 s against 45 minutes, and even with VIEWS at zero it would take 29
min 29 s, so whether the 45 minutes counts per building, per community or for the whole NM Fed
folder, and what the viewpoints may hold, were Bader's to choose, and he answered on 2026-10-04, Q101.

- Sources: S03-7, C06-J6, FIND-08, LOOK 387, C06-DONE-9, Q98 B3
- Evidence: log:8426 'The run took 1 hour 44 minutes 52 seconds, 6292.198s. That is OVER the 45
  minutes 0 seconds a run has to finish in, by 59 minutes 52 seconds.' log:8408, VIEWS 4523.564
  s, so without it 6292.198 less 4523.564 leaves 1768.634 s. Read per building every group is
  under 45 minutes, the slowest being 1B06G1 at 2056.835 s, 34 min 17 s (log:1762).
  steps\02_questions.md Q84 says the whole folder is 45 groups and 140 NWC.
- Root cause: src\Federator.Core\Diagnostics\TimingBlock.cs:65-77 and :259-266 judge one run
  against 45 minutes. CLAUDE.md says 'every ticked building runs unattended in under 45
  minutes', which reads per building, per community or per run. Which is meant is UNKNOWN.
- Class: slow
- Proof: Not a test. Q98 B3 puts the answers and the measured times of two or three viewpoint
  options in the form. After it, tests\Federator.Core.Tests\Diagnostics\TimingBlockTests.cs pins
  the chosen scope, and the TIMING line of set 05 says INSIDE or OVER the same way (set 03
  log:8426).
- Needed Bader, asked as Q101, answered on 2026-10-04, see the answered line of this item. The scope of the 45 minutes and what the viewpoints may hold are his, Q98 B3.
- Note: The options are measured in set 05 or by a probe, not guessed. The speed fix itself is
  FR-069 and goes first.
- Answered by Bader on 2026-10-04 at 15:23, Q101, in short, his words being under the question in steps\02_questions.md: do not judge the 45 minutes in this round. Make VIEWS faster with the same viewpoints, then report each group's time beside its NWC sizes and item counts, so the target can be set after the proof run. The lead's note: the per group times to be worked by F114 in wave 2, committed on its branch at f915396 before it paused.

### FR-071 views-log-silent-up-to-21-minutes

The log is silent for up to 21 minutes inside the VIEWS step, so a busy step and a stuck step
look the same, and only the processor clause of the hang rule kept the loop from calling it
hung.

- Sources: S03-9, C06-J17, FIND-14, C06-DONE-13
- Evidence: log:4476 '14:56:15.269 +3337.540s VIEWS no model in this group carries AR ...' then
  log:4477 '15:17:24.465 +4606.720s VIEWS no model in this group carries DR ...' (1269 s,
  1B06PK). log:1657 at 14:13:31 then log:1658 at 14:28:48 (917 s, 1B06G1). log:5339 at 15:26:11
  then log:5340 at 15:37:14 (11 min, 1B06PP). record.txt:578 'the log still for 1256 s against
  300'.
- Root cause: src\Federator.Addin\Engine\ViewpointBuilder.cs:186-204, the loop over the planned
  viewpoints calls progress at :190 and writes no log line.
  src\Federator.Addin\Engine\FederationEngine.cs:1025-1031 (Tick) sends to the window only,
  although its comment at :1022-1023 says anything said through it is in the log.
- Class: slow
- Proof: Run line of set 05: the log has a VIEWS progress line at least every minute inside the
  step, no gap like log:4476 to 4477. A Core test is possible once the sentence is built in
  Core, in tests\Federator.Core.Tests\Views\ViewpointBuildOutcomeTests.cs: the line at viewpoint
  N of M carries the created count and the seconds.
- Note: How often is a setting, as ClashRunner.ProgressEvery is. The comment at
  FederationEngine.cs:1022-1023 is false for every caller of Tick and is corrected with it.
  FIND-14 also sits on FR-069.

### FR-072 penetrations-upwards-walk-outside-try

Penetrations.ReadSide walks up the parents before its try and the walk has no catch, so a throw
from Parent escapes a method whose summary promises it never throws, leaves the wrappers already
walked undisposed, and ends that test's viewpoints or penetration decisions.

- Sources: T1-L1
- Evidence: src\Federator.Addin\Engine\Penetrations.cs:260 'IList<ModelItem> lookIn =
  Upwards(item)' sits before the try at :262, :371 'walker = walker.Parent' has no catch,
  :187-188 'Never throws: a side that will not read is a side with no size', and :198-199 read
  result.Item1 and result.Item2 outside any try. src\Federator.Addin\Engine\ViewpointBuilder.cs:392-394
  calls ServiceSizeOf with no try, caught once per test at :297-303 ('the clashes read before it
  threw are planned and the rest are not'), and Penetrations.cs:89-95 says 'every clash this
  test holds is left exactly as it was'. src\Federator.Addin\Engine\ClashHarvest.cs:424-441
  guards the same walk. Set 03 had no Failure line from it (log:401-402 'both clashing items
  were pointed at for 13 of 13').
- Root cause: src\Federator.Addin\Engine\Penetrations.cs:260 (Upwards before the try at :262)
  and :356-380 (:371 walker.Parent with no catch), the summary at :187-188, and Item1 and Item2
  at :158-159 and :198-199.
- Class: loud failure
- Proof: Run. The fix is add-in code with no part free of Navisworks, so no Core test. After a
  reviewed change that puts the walk inside the try, or catches and logs inside Upwards the way
  ClashHarvest.cs:424-441 does, and disposes the chain on every path, the set 05 first run shows
  VIEWS and PENETRATION blocks equal to the run before and no Failure line (the set 03 log:401-402
  style).
- Note: Whether Parent ever throws on a clash item is UNKNOWN, no doc under docs\history records
  it. One more cost: in the penetration pass the clashes judged before the throw still move to
  Reviewed (WantedFor returns the partial list at :97 and ClashRunner.cs:811-815 applies it)
  while the log says every clash was left as it was, which is then untrue. Penetrations.cs is
  also FR-170's file.

### FR-073 views-seconds-parts-do-not-add

The four parts the VIEWS step reports for its own seconds do not add up to the step, leaving
131.9 s, 81.4 s, 67.5 s and 19.3 s in 1B06PP, 1B06PK, 1B06G1 and 1B06BC that are in the step and
in none of the four parts.

- Sources: S03-16, C06-J19
- Evidence: log:5343 'the step's seconds went: 0.075s looking whether each was already there,
  1.682s dimming, 479.409s recording, 80.520s reading back' (561.686 s) against log:5349
  'finished 693.614s'. log:4480 (1339.994 s) against log:4486, 1421.383 s. log:1663 (1701.884 s)
  against log:1669, 1769.389 s. log:868 (243.958 s) against log:874, 263.228 s.
- Root cause: src\Federator.Addin\Engine\ViewpointBuilder.cs:88-91 holds four stopwatches, and
  the calls at :643-654 (Touch, ShowOnlyModels, ResetAllHidden) and :178-179 (Collect and the
  plan, 10 s in 1B06PP, log:5338) run inside none of them, nor does the loop around them.
- Class: noise
- Proof: New Core test once the sentence is built in Core, in a new
  tests\Federator.Core.Tests\Views\ViewsSecondsTests.cs: the parts plus a named setting up part
  equal the step to a small tolerance, or the line says how many seconds no part holds. Run line
  of set 05: the 'the step's seconds went' line sums to within 1 percent of 'STEP VIEWS
  finished' in 1B06BC, 1B06G1, 1B06PK and 1B06PP.
- Note: It is part of the work nothing times yet that the TIMING blocks admit to. It matters
  because FR-069 needs the missing 5 to 19 percent split first.

### FR-074 views-unknown-set-not-named

7 viewpoints sit in pair folders called UNKNOWN and hide nothing, and the log counts them
without naming the set that has no discipline code.

- Sources: C06-J23, FIND-22, C06-DONE-7, C06-DONE-20
- Evidence: log:5340 'VIEWS AR vs UNKNOWN: its pair has a code this tool does not know, so its
  viewpoints hide nothing', log:5341 'EL vs UNKNOWN', log:5359 'a set name with no code this
  tool knows : 5' (1B06PP), log:6661 and log:6679 ': 2' (1B06WL). The sets are inferred, not
  named, to be BLD-Security Devices (1B06PP read-out lines 76, 77 and 97, 1B06WL line 15).
- Root cause: src\Federator.Core\Views\ClashViewpointPlan.cs:213 and :267-270 hold a count only,
  incremented at :399-402, where clash.LeftSet and clash.RightSet are in hand.
  src\Federator.Addin\Engine\ViewpointBuilder.cs:637-641 and :651 write the same sentence for
  any such pair.
- Class: noise
- Proof: New Core test in tests\Federator.Core.Tests\Views\ClashViewpointPlanTests.cs: the VIEWS
  block names the set names with no known code, up to ten, and counts the rest. Fails before. Run
  line of set 05: the 1B06PP VIEWS block names BLD-Security Devices (set 03 log:5359 names
  nothing).
- Note: By design the folder says UNKNOWN and nothing is guessed (core.md). Which discipline
  BLD-Security Devices belongs to is his matrix, not this item. FIND-22, C06-J23 and C06-DONE-20
  also point at UNKNOWN lines that are right by design, log:5340, 5341 and 6661, and 'UNKNOWN
  for 0' at log:8367.

## Harvest and pictures, FR-075 to FR-077

### FR-075 picture-rename-two-pass-has-no-undo

If a move fails part way through the picture renumbering, the pictures already moved sit under
.moving names, their rows still point at the old names, the workbook and page are written from
those rows, and the log line says the pictures not yet renamed keep their run order numbers,
which is not the state on disk.

- Sources: T1-S57
- Evidence: src\Federator.Core\Report\ReportOrder.cs:272-275 'foreach (Move move in moves)' then
  'File.Move(move.From, move.From + Holding)' with Holding '.moving' (:222), and :278-288 move
  each to its final name and only then call 'Point(move.Number, workbookPath)'. Nothing in Apply
  catches or rolls back. src\Federator.Addin\Engine\FederationEngine.cs:2754-2759 catches and
  logs 'kept going, the pictures that were not yet renamed keep their run order numbers'. A
  throw in pass two at move k leaves moves k onwards as '<old name>.moving' while their rows
  keep the old ImageFile, ImageLink and ImagePath. Set 03: 16 groups renumbered with 0 missing
  and no kept going line, and outputs.txt lists no .moving file.
- Root cause: src\Federator.Core\Report\ReportOrder.cs:270-288 (ImageRenumbering.Apply), with
  the wording at src\Federator.Addin\Engine\FederationEngine.cs:2754-2759.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Report\ReportOrderTests.cs, which covers swap,
  missing file and no holding file left on success but no move that throws: force a move to fail
  with a folder sitting at one target path, then assert no .moving file is left and every row
  still points at a file on disk, or that the outcome names what was left. Fails today.
- Note: Low likelihood, it needs a move to fail, for example a picture open in a viewer. When it
  fires the workbook and the page carry links to pictures that are not there.

### FR-076 image-guard-per-group-not-per-run

The 50 image failures guard is built new for each group and read only after the group's clash
step returns, so a run where every picture fails never stops unless one group alone reaches
fifty in a row, and once it fires the rest of that group is still rendered. A render that
finishes with no file carries the path in its reason, so that failure never repeats the same
reason.

- Sources: T1-B1
- Evidence: src\Federator.Addin\Engine\FederationEngine.cs:2589 'runner.Images = new
  ClashImages(log, reports.Images)' inside the per group clash step.
  src\Federator.Addin\Engine\ClashImages.cs:66-68 builds a new RepeatedFailureGuard when
  StopAfterFailures is above 0. ClashImages.Write (:124-208) and
  src\Federator.Addin\Engine\ClashHarvest.cs:382-393 never read ShouldStopTheRun.
  FederationEngine.cs:2706 'if (runner.Images.ShouldStopTheRun && stopTheRun == null)' runs only
  after runner.Run(plan) at :2602. ClashImages.cs:183-184 'the render finished but no file
  arrived at ' + path. The test guard, by contrast, is handed in for the whole run
  (ClashRunner.cs:104-109). Set 03: no image failed (log:395 'IMAGES 13 written').
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:2589 with
  src\Federator.Addin\Engine\ClashImages.cs:66-68 and :183-184, ClashHarvest.cs:382-393 and
  FederationEngine.cs:2706-2710.
- Class: broken feature
- Proof: Run. With renders forced to fail across groups of under fifty clashes, the run stops
  after fifty image failures in a row across groups, and RESULT names the images. The five runs
  of set 05 have no forced image failure, so the proof needs one added, UNKNOWN whether the lead
  adds it. A Core test can pin only the same reason key if the reason wording moves to Core:
  tests\Federator.Core.Tests\Clash\RepeatedFailureGuardTests.cs, two failures differing only by
  path count as the same reason.
- Note: core.md says fifty image failures stop the run the same way and with the same default as
  the clash step. The guard's words are FR-129.

### FR-077 pictures-cost-643-seconds

HARVEST is 11.7 percent of the run, 738.036 s, nearly all of it the 643.150 s spent writing 5679
pictures of 1376 MB.

- Sources: S03-10, C06-J16
- Evidence: log:8409 'HARVEST 738.036s 11.7% 3024 visits' and log:8424 'IMAGES 643.150s 10.2%
  5679 visits'. log:4468 '1629 written, 507.45 MB, 188.2 seconds in total' and log:4469 '0.116
  seconds each on average'. 738.036 less 643.150 leaves 94.886 s for the harvest itself.
- Root cause: src\Federator.Addin\Engine\ClashImages.cs:217-230, one TestsImageForResult render
  at 1024 by 1024 and one JPEG save per clash. How much of the 0.113 s each is render and how
  much is save is UNKNOWN.
- Class: slow
- Proof: Run line of set 05: the IMAGES line per group with the same written counts (5679 in
  all) and fewer seconds, the TIMING IMAGES line below 643.150 s (log:8424), and the pictures
  still 1024 by 1024 JPEG (log:291).
- Note: A speed change that keeps the output the same (Q98), measured before and after. The
  picture rename pass at src\Federator.Addin\Engine\FederationEngine.cs:2743-2761 runs inside no
  step, 20.6 s in 1B06PK (log:4466 to 4467), and is part of the 181.993 s outside every step
  (log:8421). TESTS RUN is another 584.150 s and is Navisworks's own work.

## Install, FR-078 to FR-081

### FR-078 install-recursive-delete-without-junction-check-and-partial-first-install

build\install.ps1 deletes recursively with no check for a junction inside the bundle, and on a
first install with nothing moved aside a failed copy leaves a partial bundle at the load path.

- Sources: T3-G11
- Evidence: build\install.ps1:50, :129 and :260 'Remove-Item ... -Recurse -Force'. The junction
  check at :96-106 walks only from %APPDATA% down to the bundle folder. At :152-159, on a copy
  failure with no old bundle it throws 'It is at <target>, and no add-in was installed before',
  leaving the partial folder where Navisworks loads it.
- Root cause: build\install.ps1:50, :129 and :260 (the deletes) and :152-159 (the partial first
  install).
- Class: broken feature
- Proof: Harness cases in tools\loop\prove-run.ps1 (H12b runs copies of build\install.ps1
  against a fake APPDATA): a junction inside the old bundle is refused, and a failed copy on a
  first install leaves no folder at the load path. No Core test applies.
- Branch: taken with F109, which edits build\install.ps1 on fix-F109 and is not merged at main
  3449521. build\install.ps1 and prove-run.ps1 read the same on main as at f38edd5.
- Note: build\install.ps1 reaches all 27 machines. FR-078 to FR-081 go with F109.

### FR-079 install-killed-between-move-and-removal

If run.ps1 or build\install.ps1 is killed or hangs between the move aside of the installed bundle
and the removal of the old one, the load path is empty or holds part of the new bundle and
Bader's old bundle sits whole at ParsonsNwcFederator.bundle.replaced-<time>, with no line saying
so and nothing that puts it back, and run.ps1 waits on install.ps1 with no limit.

- Sources: T3-G14 (placed by the register reader and the readings reader, folded here)
- Evidence: build\install.ps1:117-121 renames the installed bundle to .replaced-<time> (the
  rename at :118-120), :152-159 copy the new one in, :259-262 remove the old one, and only the
  caught failures at :157 and :251 call PutOldBack. tools\loop\run.ps1 starts install.ps1 through
  RunChild, whose WaitForExit() has no limit (run.ps1:748 and :923 at f38edd5, RunChild at :1226
  with WaitForExit at :1238 and the install call at :1404 on main 3449521). BundleLeftovers
  (run.ps1:784-789 at f38edd5, :1274-1281 on main) is read only after a good install, and Check
  mode never looks for a .replaced- folder.
- Root cause: build\install.ps1:117-121, :152-159 and :259-262, no recovery state between the
  rename and the end, with RunChild in tools\loop\run.ps1, no time limit, and its call for
  install.ps1.
- Class: broken feature
- Proof: Harness cases in tools\loop\prove-run.ps1: a copy of install.ps1 that exits right after
  the rename leaves a state that Check names, and the next Install puts it right or refuses with
  a line, and a child that never ends makes RunChild return a named timeout. The time limit is a
  setting. No Core test applies.
- Branch: taken with F109, the same script, fix-F109 not merged at main 3449521. The run.ps1
  lines moved with F106 (3449521) and the main lines are given above.
- Note: Silent in the register. The time limit in RunChild is the same change as FR-096, which
  waits on read-workbook.ps1 the same way.

### FR-080 install-leftovers-exit-codes-and-success-line

Install leftovers are read wrong. A failed removal of the old bundle after every check passed
prints one LEFT line and still ends on the success line with exit 0, which INSTALL.md reads as
success in a direct install. BundleLeftovers names every old .replaced- and .failed- folder as
the add-in installed before, so one stale folder makes every later Install exit 5. A wrong stamp
returns before a leftover is named, a removal that stops part way leaves part of the old bundle,
and the checks that gate the removal read the staging copy, not the installed one.

- Sources: T3-G15 (placed by the register reader and the readings reader, folded here)
- Evidence: build\install.ps1:259-262 prints the LEFT line and :272 still prints 'Start
  Navisworks Manage 2025 and look on the Tool Add-ins tab for Parsons NWC Federator.' with exit
  0, against INSTALL.md:29-33 'It worked if the terminal's last line reads'. BundleLeftovers in
  tools\loop\run.ps1 matches names that start .replaced- or .failed- (run.ps1:788 at f38edd5,
  :1278 on main 3449521) with Code = 5 in InstallVerdict (run.ps1:779 at f38edd5, InstallVerdict
  at :1264 on main), and the Install flow breaks on a wrong stamp (run.ps1:947 at f38edd5) before
  it names the leftovers (:948). build\install.ps1:51 and :197-243 read $contents, the staging
  copy. The register row (turn5\pr83-diff-rest.txt:101) adds that a removal that stops part way
  leaves part of the old bundle.
- Root cause: build\install.ps1:259-262 and :272, :197-243, INSTALL.md:29-33, and BundleLeftovers
  and the Install flow in tools\loop\run.ps1.
- Class: broken feature
- Proof: Harness cases in tools\loop\prove-run.ps1 for each part: trial 6c (prove-run.ps1:1059)
  ends with the success line absent and a non zero exit, a stale .replaced- folder of an earlier
  call is told apart from the one this install moved aside, a wrong stamp exit still names the
  leftovers, and the checks read the installed bundle. INSTALL.md's success line reads the LEFT
  line. No Core test applies.
- Branch: taken with F109, fix-F109 not merged at main 3449521. The run.ps1 lines moved with
  F106 and the main lines are given above.
- Note: Both breakers classed it polish, and the register lists it for after F103.
  steps\loop.md:1014-1017 records that no .replaced- or .failed- folder sat beside the bundle on
  2026-09-30, so the first Install was not blocked. Whether Navisworks loads a folder whose name
  does not end in .bundle is UNKNOWN (install.ps1:115).

### FR-081 f103w-install-failure-claims-and-refused-text

Words say a failure after the move takes the new bundle out and puts the old one back, and that
a folder it cannot remove is named in the verdict of run.ps1 -Mode Install. Harness case ta2
shows the new bundle can stay at the load path with the old one still aside, and the record
names neither folder. The REFUSED text of install.ps1 says the rename was refused most likely by
a running Navisworks, right after the Roamer read found none.

- Sources: F103-W-5, F103-W-6, F103-W
- Evidence: build\install.ps1:113-114, tools\loop\README.md:162-164 at f38edd5 and
  .claude\rules\loop.md:70-72 against build\install.ps1:126-145 (PutOldBack leaves the old one
  aside when the new one can be neither removed nor moved) and prove-run.ps1:1055-1056 (case
  ta2). build\install.ps1:120 'most likely by a Navisworks that is running' after the read at
  :91-94. steps\loop.md:1089-1105 records the rename denied on 2026-10-01 with no Navisworks
  running.
- Root cause: build\install.ps1:113-114 and :120, tools\loop\README.md (:162-164 at f38edd5, moved
  by F108's change to the README) and .claude\rules\loop.md:70-72.
- Class: noise
- Proof: A read of the words after the change. The printed REFUSED line is checked by the
  prove-run.ps1 install trials (prove-run.ps1:1044-1065), which stay green. No Core test applies.
- Branch: taken with F109, which is about what holds the file, fix-F109 not merged at main
  3449521.
- Note: F103-W-6 changes a line that runs. What holds the file is UNKNOWN and is the subject of
  F109. The other F103-W entries are FR-140, FR-141 and FR-142.

## Loop tools, FR-082 to FR-124

### FR-082 clash-xml-copy-not-the-exchange-matrix

The first run read the desktop clash XML as it stood, asking ME-DUCTWORK in capitals, and not
the corrected matrix in exchange\, and nothing in the loop compared the two, so F87, F84 and Q68
were not tested and 14 sets found nothing in all 22 groups.

- Sources: FIND-04, S03-2, C06-J2, Q98 B1
- Evidence: log:146 'RUN the Clash step picked ...NMFed/1104-PAR_CLASH_AllInOne_25mm_FIXED.xml'
  names a path only, and driver.txt:12 typed it. FIND-04: line 28826 of the copy reads condition
  test equals where line 28826 of exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml reads
  contains, the copy has ME-DUCTWORK five times and flags 32 no times, and the exchange file
  ME-Ductwork five times and flags 32 six times. log:243 asks ME-DUCTWORK. log:8346 'found
  nothing in every group : 14'.
- Root cause: tools\loop\prepare-copy.ps1 copies the desktop folder as it stands and never reads
  the XML (:224-230 at f38edd5, the copy code moved with F108). On main 3449521 run.ps1 takes
  -Xml since F106 (RunPaths at :115-155, the file check at :365, handed to the driver at :1599),
  and a grep of run.ps1 and prepare-copy.ps1 on main finds no mention of the exchange file, so
  nothing compares the two. The data cause was the desktop file, which Bader renamed -OLD and
  replaced with the exchange file on 2026-10-04 (Q98 B1).
- Class: silent wrong number
- Proof: Run record of set 05: a line giving the sha256 of the copy's XML and whether it equals
  exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml, and the tool log SET line for
  BLD-ME-Ducts&Duct Fittings asking ME-Ductwork (set 03 log:243 asked ME-DUCTWORK). A case in
  tools\loop\prove-run.ps1 that a copy whose XML differs from the exchange file is named in the
  record. No Core test.
- Branch: rests on F106 (run.ps1), merged as 3449521, and F108 (prepare-copy.ps1), merged as
  c9b223b.
- Note: B1 allows either: the desktop file is the exchange file, or the corrected one goes into
  the copy and the turn says so. prepare-copy.ps1 already sees a changed desktop file by sha256
  and remakes the copy, so the set 05 copy will carry the new file. The corrected file is not
  enough on its own, FR-008.

### FR-083 f106-verdict-ran-ignores-group-results

A window run reads VERDICT RAN with exit 0 whenever the tool's log holds a RESULT block, the
installed stamp and no unticked group, and never looks at how many groups are done, partial or
failed, so a run in which every group FAILED still reads RAN.

- Sources: F106-HARM-BRK-1
- Evidence: tools\loop\run.ps1:810-835 (ToolLogVerdict reads the RESULT title, the SESSION
  plugin version and the unticked count only), :715-721 (RunVerdict) and :1784-1787. Run line:
  steps\runs\03\item1-C06\record.txt:1070 'VERDICT: RAN, item 1 through the window, the log's
  RESULT block read, closed by Dispose, put back' while steps\runs\03\findings.md:11 reads '17
  DONE, 0 PARTIAL, 5 FAILED of 22'.
- Root cause: tools\loop\run.ps1:810-835 and :715-721.
- Class: silent wrong number
- Proof: No Core test applies, nothing under src changes. A prove-run.ps1 case: ToolLogVerdict
  and RunVerdict given a log whose RESULT block reads 0 groups done and 22 failed do not return a
  plain RAN with exit 0. Set 05: the VERDICT line of record.txt carries the done, partial and
  failed counts.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Whether the counts go into the text or a FAILED group gives a non zero exit is the
  lead's design call. FR-105 is the separate ordering fault inside RunVerdict.

### FR-084 copy-set-takes-bader-outputs-as-inputs

-Set NN copies whatever NM Fed holds in NWF, NWD and Clash Report and reports a whole fresh copy,
so outputs that Bader's own runs left in NM Fed land in runs\NN\NMFed and in NMFed.manifest.txt
as if they were input files, and nothing refuses or flags it.

- Sources: F108-BRK-1, F108-R5
- Evidence: tools\loop\prepare-copy.ps1:282-288 check only that NWC, Clash Report and a folder
  under NWC exist, :273 reads, and :345-351 copy and mark every file whole.
  steps\runs\00\source-listing-during-bader-run-1B06BC.txt (780 files, an NWF at NWF\C06 and a
  pictures folder under Clash Report) is NM Fed holding outputs on 2026-09-27. The register row
  F108-R5 (turn5\pr83-diff-rest.txt:115) says the same.
- Root cause: tools\loop\prepare-copy.ps1:273-288 and :345-351, the -Set mode. The plain command
  reads and copies everything the same way (:188-192 and :224-230 at f38edd5).
- Class: silent wrong number
- Proof: No Core test applies. A case in the F108 proof script (turn4\f108-prove.sh, outside the
  repo) or a new tools\loop harness case: a file in NM Fed under NWF or Clash Report makes -Set
  refuse, or stay out of the manifest and say so. Set 05: NMFed.manifest.txt of the fresh copy
  names no file outside NWC and the XML.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: F106 check 9 (run.ps1:355-362, OutputsRefusal) refuses item 1 loudly when
  NMFed\NWF\C06, NWD\C06 or Clash Report\C06 hold a file, which is the way the F108-R5 row names,
  so the silent case left is a folder at the top of Clash Report or in another community.
  F108-R5 also sits on FR-101.

### FR-085 f106-autosave-putback-deletes-unowned-files

The AutoSave put back treats every file that appeared in his AutoSave folder during the run as
the run's own and deletes it with no copy taken first, and makes a gone file again from the
backup, so a file put there or taken away by anything but the adopted Navisworks is deleted or
made again.

- Sources: F106-HARM-BRK-2
- Evidence: tools\loop\run.ps1:557 labels any file absent before as 'ADDED by the run', :563-567
  delete it with [System.IO.File]::Delete after only a Roamer check (:559-560) and a hash read
  again (:565), and :571-581 copy a gone or changed one back from autosave-backup.
- Root cause: tools\loop\run.ps1:546-585 (PutBackAutoSave), mainly :557 and :563-567.
- Class: broken feature
- Proof: No Core test applies. A prove-run.ps1 case on fake folders: a file added to the AutoSave
  folder that is not an autosave of one of the run's own documents is left, or copied aside
  first, and the run's own autosaves are still removed and read back. Set 05: the AutoSave lines
  of record.txt name the rule that owned each file removed.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Low odds. Q86 lets the loop remove the autosaves its own runs added, so the fix stays
  inside that answer. Set 03 put back 78 of his files after Navisworks rotated them out
  (record.txt:877 on, steps\loop.md:80-81). The rule text that still says nothing is written
  into his AutoSave folder is FR-138.

### FR-086 guard-unreadable-process-list-reads-as-none

In the put back a process list that cannot be read counts as no Navisworks running, and
ProcState turns a failed read into gone, so a write can go ahead beside a Navisworks the loop
cannot see.

- Sources: T3-G1, T3-G8
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:1226 '$ListRoamers = { @(Get-Process -Name Roamer
  -ErrorAction SilentlyContinue) }' is the last check before each write, called at :627 and
  :693. :532-533, ProcState returns gone when the process is null. :1215 and :783 read the list
  the same way. The start refusal at :603 does it right, 'the process list could not be read'.
  On main 3449521 the list is at nw-guard.ps1:1318 in SettingsPutBack (:1311), ProcState at :623,
  PutBackRegistry at :713, PutBackFiles at :781, NewRoamers at :873, PutBackReasons at :1279 and
  RoamerRefusal at :692.
- Root cause: tools\loop\nw-guard.ps1, the ListRoamers block in SettingsPutBack and its use in
  PutBackRegistry and PutBackFiles, ProcState, and the reads in NewRoamers and PutBackReasons.
- Class: broken feature
- Proof: A new case in tools\loop\prove-run.ps1 that gives the put back a process list that
  throws and expects no write and an UNKNOWN reason. No Core test reaches PowerShell.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: T3-G1 and T3-G8 are one fault in two places (G8 is the family of G1), so one fix.
  steps\01_next.md:974 says F103 left T3-G1 to G5 unfixed on purpose. The harm needs a list that
  fails to read at the moment of the put back, and no run has shown it.

### FR-087 adopt-runs-after-deadline-path-with-watchdog-stopped

After the constructor deadline path ran and TerminateProcess returned false, the watchdog stops
itself and nothing checks that before adoption, so the hold, Visible and Dispose can run with no
watchdog, no ceiling and no call limit.

- Sources: T3-G2
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:1001-1002 '$sync.Stop = $true' then 'break'
  after the TerminateProcess line. DeadlineDone is read only at :964 and :1195 (grep). AdoptStart
  at :1109-1163 and run.ps1:1053-1055 never read it. On main 3449521 Stop is at
  nw-guard.ps1:1093, DeadlineDone is read at :1056 and :1287, AdoptStart is at :1201, and the
  adoption call at run.ps1:1544.
- Root cause: AdoptStart in tools\loop\nw-guard.ps1 reads neither $sync.DeadlineDone nor
  $sync.Stop, and neither does its caller in tools\loop\run.ps1.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 where the deadline path runs, TerminateProcess
  is made to return false, and adoption is refused. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: PutBackReasons already refuses the put back in this case, so his settings stay as they
  are. What is lost is the ceiling and the call limit.

### FR-088 adopt-start-has-no-time-bound

The stretch from the constructor returning to the pid being held has no time bound, so a hung
process list or WMI read inside AdoptStart can stall the run with nothing to end it.

- Sources: T3-G9
- Evidence: At f38edd5, tools\loop\run.ps1:1049 '$sync.CtorReturned = $true' comes before
  AdoptStart at :1053. The constructor deadline at nw-guard.ps1:964 needs CtorReturned false and
  the call limit at :1026 needs MyPid to be set, which happens only inside AdoptStart (:1160).
  AdoptStart calls NewRoamers and RoamerRecord, which read the process list and Win32_Process. On
  main 3449521 CtorReturned is set at run.ps1:1540, AdoptStart is called at :1544, the deadline
  is at nw-guard.ps1:1056 and AdoptStart at :1201.
- Root cause: neither the constructor deadline nor the call limit in tools\loop\nw-guard.ps1
  covers the stretch, with the order in tools\loop\run.ps1 and AdoptStart in nw-guard.ps1.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 where the WMI read inside adoption is made to
  hang and the run ends by a bound. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: The breaker on ownership found a WMI stall right after the constructor returns, noted in
  steps\notes\f103-final-reading.md:118-119.

### FR-089 put-back-key-unreadable-at-end-counted-nowhere

A key under HKCU Navisworks Manage 22.0 that read at the start and cannot be read at the end is
neither compared nor counted, so the put back reads whole and the run exits 0.

- Sources: T3-G6
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:238 adds the line 'could not read at the end ...
  Nothing is written under it' and :258 adds 'could not be read at the end, not compared, nothing
  written under it', and neither adds an entry to Changes. The summary at :1253-1259 counts
  Changes, appeared files, in use files and not backed up files only. tools\loop\run.ps1:1200 sets
  NotPutBack from NotWritten and AutoSave alone. On main 3449521 the first line is at
  nw-guard.ps1:260 in DiffRegistry (:258), and the summary is in SettingsPutBack (:1311).
- Root cause: DiffRegistry in tools\loop\nw-guard.ps1 leaves these keys out of Changes, the
  summary in SettingsPutBack leaves them out of the count, and run.ps1 sets NotPutBack without
  them.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 with a key made unreadable at the end,
  expecting NotWritten above 0 and exit 6. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: The register says silent. Nothing is written under such a key, so nothing is damaged.
  Only the claim of a whole put back is wrong.

### FR-090 watch-append-failure-switches-off-put-back

Anything that reads watch.txt with a locking read during a run can make one watchdog append
fail, which switches off the whole put back and blames the watchdog.

- Sources: T3-G7
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:849-850, the watchdog W function, catches a
  failed append into $sync.WriteErrors with no retry, and :1194 adds the reason 'the watchdog
  could not write N lines' whenever WriteErrors holds any. ReadShared (:558-561) protects only
  run.ps1's own heartbeat read at run.ps1:646. On main 3449521 the catches are at
  nw-guard.ps1:942 and :946, the reason is in PutBackReasons (:1279) and ReadShared is at :650.
- Root cause: the W function of the Watchdog in tools\loop\nw-guard.ps1 and the reason it feeds
  in PutBackReasons.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 that holds watch.txt open with a locking read
  during an append and expects the append retried or the reason told apart. No Core test
  applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: Silent until the end, when the put back is skipped and the backup is kept, so nothing of
  his is lost.

### FR-091 m5-walk-has-no-time-limit

M5 walks %TEMP% and the Autodesk folders with no time limit and the put back waits on it.

- Sources: T3-G10
- Evidence: At f38edd5, tools\loop\run.ps1:429-441 NewerFiles runs 'Get-ChildItem -LiteralPath
  $dir -Recurse -File -Force' over every root, and :1124-1125 hand it $env:TEMP and four
  Autodesk folders and wait for the list before the put back at :1163. On main 3449521
  NewerFiles is at run.ps1:645.
- Root cause: NewerFiles in tools\loop\run.ps1 and its call before the put back.
- Class: broken feature
- Proof: Harness case in tools\loop\prove-run.ps1 with a walk made slow and a bound that cuts it
  and says so. No Core test applies.
- Branch: lines moved with F106 (3449521), the main line of NewerFiles is given above.
- Note: On the third real start M5 listed 62 files, 33 of them under %TEMP% (steps\loop.md, turn
  4), so the walk finishes today. Only the missing bound is the fault.

### FR-092 f106-harness-stale-after-window-run

prove-run.ps1 is still F103's. Now that F106 is merged, its H0 forbids PostMessageW and a file
delete in run.ps1 and nw-guard.ps1, which F106 adds on purpose, its H6 calls HangVerdict with
the old four arguments, and it holds no case for PutBackAutoSave, PostClose, Answer, PathsOutside
or IsConfirm.

- Sources: F106-HARM-REV-2, F106-HARM-BRK-13, R1 of F106, R4 of F106
- Evidence: tools\loop\prove-run.ps1:224, the word list holds 'PostMessageW', :242-243 flag a
  Delete call, :270 holds the PostMessageW bad case, and :579 and :586 call 'HangVerdict $row[1]
  $row[2] $tNow 20' against the five argument HangVerdict at run.ps1:668. F106's PostMessageW is
  at nw-guard.ps1:406, run.ps1:886 and drive-window-run.ps1:245, and its delete at run.ps1:566.
- Root cause: tools\loop\prove-run.ps1:224, :242-243, :254, :270, :579 and :586, unchanged on
  main 3449521, where they now disagree with F106's run.ps1.
- Class: broken feature
- Proof: No Core test applies. prove-run.ps1 itself runs green on the merged F106 code and gains
  the cases of turn4\f106-proof\prove-f106.ps1 (47 passed, outside the repo) for PutBackAutoSave,
  PostClose, Answer, PathsOutside and IsConfirm. R4 of F106: H0 allows PostMessageW in PostClose
  and the driver's Answer only.
- Branch: rests on F106, merged as 3449521. The harness reads the same on main as at f38edd5.
- Note: Without it the loop has no proof of its own guards since F106 merged. The other loop
  tool fixes are proved in this harness, so this goes first in the lane.

### FR-093 remove-restore-refuse-name-in-both-communities

-Remove and -Restore refuse any NWC whose file name sits in both communities, which is all three
files of group 100000, so items 3 and 4 of a run set cannot take a file out of that group or put
one back.

- Sources: F108-BRK-5, F108-R1
- Evidence: At f38edd5, tools\loop\prepare-copy.ps1:112 throws 'the name is under the folder 2
  times', from Find-One (:107-114), called at :152 for -Remove and :165 for -Restore. On main
  3449521, since F108, Find-One is at :180-187 and is called at :226 and :239.
  %LOCALAPPDATA%\NwcFederatorLoop\source.manifest.txt lines 2-4 and 69-71 hold the same three
  names with the same sha256 and size under NWC\C06 and NWC\C07.
- Root cause: Find-One in tools\loop\prepare-copy.ps1 and its two callers.
- Class: broken feature
- Proof: No Core test applies. A proof case in the F108 proof script (outside the repo) or a new
  harness case: -Remove of 1104-PAR-100000-ZZZ-AR-MOD-003000.nwc with a community or a relative
  path takes out exactly one copy, and -Restore puts it back by hash, the other community's copy
  untouched. Set 05: items 3 and 4 run on a group 100000 file. The F108-R1 row's way out: the
  file named with its community, or the group for runs 3 and 4 chosen outside 100000.
- Branch: on main since 2026-09-27 and in F108's code, merged as c9b223b.
- Note: The group is the one the per community Clash Report folders were made for. The F108
  proof picked its two names with uniq -u and never met this.

### FR-094 f106-no-hang-clock-when-log-not-single

When no new log, or more than one, names the installed stamp in its SESSION block, for example
because Bader opened the tool himself, the monitor starts no hang clock and posts no WM_CLOSE
after a RESULT block, so a run that finished waits for the 12 hour ceiling with the keep awake
request on and the put back waiting.

- Sources: F106-HARM-REV-6, F106-HARM-BRK-5, R2 of F106
- Evidence: tools\loop\run.ps1:962-986 takes a log only when exactly one new log names the stamp
  (:968), and more than one writes UNKNOWN to toollog-name.txt (:978-982) and starts no clock.
  The RESULT sighting that posts WM_CLOSE is set only from the log stream (:1010) and needed at
  :1097.
- Root cause: tools\loop\run.ps1:962-986 and :1095-1117.
- Class: slow
- Proof: No Core test applies. A prove-run.ps1 case with two stand-in logs naming the stamp
  inside one pass: the run still ends by a rule other than the 12 hour ceiling and says why.
  Set 05: record.txt has a log found line before any RESULT.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: RunLog.cs:1142 writes only 'plugin version :' in SESSION and no process id, so how to
  tell the loop's log from Bader's own is a design call. FR-113 is the same choice seen from the
  name file.

### FR-095 f106-monitor-fault-leaves-plugin-call-unbounded

A fault in the monitor sets RunOver FAULT but never calls EndArm, and the main thread sits in
ExecuteAddInPlugin with CallLimit 0, so nothing posts WM_CLOSE or applies the hang rule and only
the 12 hour ceiling frees the run, with the tool's window up on his screen and the keep awake
request on.

- Sources: F106-HARM-REV-7, F106-HARM-BRK-4
- Evidence: tools\loop\run.ps1:1137-1141, the catch sets MonitorFault and RunOver without
  EndArm, :1147-1151 EndArm sets the 120 s limit and is called only at :1089, :1091 and :1107,
  and :1593 '$sync.CallLimit = 0'.
- Root cause: tools\loop\run.ps1:1137-1141 and :1593.
- Class: slow
- Proof: No Core test applies. A prove-run.ps1 case: force a fault inside Monitor and assert
  CallLimit is bounded and WM_CLOSE or a forced end follows within the limit.
- Branch: rests on F106, merged as 3449521, lines unchanged.

### FR-096 f106-read-workbook-no-time-limit

read-workbook.ps1 runs through RunChild, which waits with no time limit, before the keep awake
release, so a read-out that never ends holds the run, the loop's lock and the keep awake
request.

- Sources: F106-HARM-REV-8
- Evidence: tools\loop\run.ps1:1832 calls RunChild, whose WaitForExit() at :1238 has no limit,
  and the keep awake release is at :1846-1853, after it. Set 03 read all 22 workbooks without a
  hang (record.txt:1046-1067).
- Root cause: tools\loop\run.ps1:1226-1240 (RunChild) and :1826-1835. RunChild was the same
  function at run.ps1:736-750 at f38edd5.
- Class: slow
- Proof: No Core test applies. A prove-run.ps1 case with a child that never ends: the call
  returns with a named timeout and the keep awake request is still released.
- Branch: the call rests on F106, merged as 3449521, lines unchanged. RunChild was on main
  before it.
- Note: The limit is a setting. The same RunChild change serves FR-079.

### FR-097 f106-foreign-window-stops-driver-and-close

Any visible window of the adopted Navisworks other than the tool's window and the main window, a
floating panel or a dialog of the tool among them, stops the driver as DIALOG after three passes
and keeps WM_CLOSE from being posted, so the run ends HUNG after 300 s and is closed through the
held handle instead of RAN.

- Sources: F106-HARM-REV-5, R3 of F106
- Evidence: tools\probes\drive-window-run.ps1:316-317 and :359-363 stop on three passes of other
  dialogs, tools\loop\run.ps1:1104 takes as busy any visible window whose kind is neither WINDOW
  nor MAIN, and :1110-1111 leave WM_CLOSE unposted. tools\loop\nw-guard.ps1:535-540 (WindowKind)
  returns DIALOG for any other window.
- Root cause: tools\loop\run.ps1:1104-1111, tools\probes\drive-window-run.ps1:281-284, :316-317
  and :359-363, and tools\loop\nw-guard.ps1:535-540.
- Class: loud failure
- Proof: No Core test applies. A prove-run.ps1 case with the stand-in's three window role: a
  floating panel and a tool dialog end the run with a named finding and no forced kill. Set 05:
  record.txt shows no HANG line and a VERDICT that names any dialog.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: R3 of F106 is the design side: a dialog of the tool is left up on purpose, because the
  rule says every dialog is a finding with its text, so it ends only by HUNG after 5 minutes.
  Whether to answer known harmless dialogs is the lead's call.

### FR-098 f106-confirm-child-read-timeout-not-seen

A child of the tool's confirm whose WM_GETTEXT read times out at 500 ms leaves no text and no
marker, so the driver's 'not read whole' rule cannot see it. The confirm has one text, and
losing it makes IsConfirm false, so the driver stops on it as a DIALOG and the run ends HUNG.

- Sources: F106-HARM-REV-4
- Evidence: tools\loop\nw-guard.ps1:472-476 reads each child with SendMessageTimeoutW and a 500
  ms limit and adds a text only when it is not empty. tools\probes\drive-window-run.ps1:370 looks
  only for a text of 2040 characters or more or the markers 'and N more visible children not
  read' and 'the walk of its children stopped'. IsConfirm (nw-guard.ps1:547-551) then reads
  false, and drive-window-run.ps1:359-363 stop the driver after three passes.
- Root cause: tools\loop\nw-guard.ps1:472-476 and tools\probes\drive-window-run.ps1:359-370.
- Class: loud failure
- Proof: No Core test applies. A prove-run.ps1 case with a stand-in confirm whose child never
  answers WM_GETTEXT: the driver answers Cancel or names the unread child, and does not stop as
  DIALOG. Set 05: driver.txt shows the confirm texts read whole.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Set 03 did not hit it (record.txt:81 'DRIVER exited 0, PRESSED').

### FR-099 unproved-start-refuses-on-any-process-holding-the-pid

A line of unproved-starts.txt that carries start ticks refuses the next run whenever any process
holds that pid and its start time cannot be read, whatever its name.

- Sources: T3-G3
- Evidence: At f38edd5, tools\loop\nw-guard.ps1:768 adds 'pid N, start ticks N, a process holds
  that id now and its start time cannot be read' when the start time is null. The branch for a
  line with no ticks at :749 does test that the process is a Roamer, and this one does not. On
  main 3449521 the two are at :860 and :841, in UnprovedRefusal (:826).
- Root cause: UnprovedRefusal in tools\loop\nw-guard.ps1 (:765-769 at f38edd5).
- Class: loud failure
- Proof: Harness case in tools\loop\prove-run.ps1: a line with ticks whose pid is now a process
  other than a Roamer, with an unreadable start time, does not refuse. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: Old, from F100. It refuses on the safe side and a person looks.

### FR-100 git-wall-reads-main-clone-branch

The git wall judges a push by the branch of the main clone, so it refuses a push from a worktree
on a fix branch whenever the main clone has main checked out.

- Sources: T3-W1
- Evidence: .claude\hooks\refuse-git-on-main.sh:57-58 change to "${CLAUDE_PROJECT_DIR:-.}" and
  then read the branch with git rev-parse --abbrev-ref HEAD, the main clone, once. :120 skips the
  -C argument and does not follow it. :161 'main is checked out and nothing is pushed from it.'
- Root cause: .claude\hooks\refuse-git-on-main.sh:57-58, :120 and :161.
- Class: loud failure
- Proof: A new case in tools\loop\prove-hooks.sh: a clone with main checked out and a git
  worktree on a fix branch, where a push of that branch from the worktree exits 0 and a push to
  main from it exits 2.
- Note: A false refusal on the safe side. A wall change goes into its own folder, is proved with
  prove-hooks.sh, read by a breaker, and copied in by a command (.claude\rules\loop.md). The main
  clone is on a fix branch, never on main, so today's pushes pass.

### FR-101 copy-set-no-navisworks-check

prepare-copy.ps1 never looks for a running Navisworks. -Set reads and copies NM Fed, and -Remove
and -Restore change a set's copy whoever runs what, so a read of a file can make his own tool's
write to it fail inside his run while the loop sees success.

- Sources: F108-BRK-2, F108-R5
- Evidence: No Roamer or Get-Process anywhere in tools\loop\prepare-copy.ps1 at f38edd5 or in
  F108's version (read whole). Reads at :188 and :229 at f38edd5 and at :273, :323 and :345 in
  F108's version. Bader ran his tool with NM Fed as its output folder on 2026-09-27 from 16:37 to
  about 17:00 (steps\loop.md:238-246). The register row F108-R5 says the same, nothing in it
  reads for a running Navisworks.
- Root cause: tools\loop\prepare-copy.ps1, the plain command (:188-192 and :224-230 at f38edd5)
  and -Set, -Remove and -Restore (:273, :323, :345 and :223-254 on main 3449521).
- Class: loud failure
- Proof: No Core test applies. A proof case with a stand-in Roamer running: every mode refuses
  with one line naming the pid and writes nothing.
- Branch: rests on F108, merged as c9b223b, lines unchanged, and on the plain command, which was
  on main before.
- Note: It needs his write to hit the very file being read in that instant. The plain command
  has read NM Fed this way since 2026-09-27, and F108 only adds reads. F108-R5 also sits on
  FR-084.

### FR-102 copy-manifest-kept-short-or-changed

The plain command keeps the source copy when it matches NM Fed and never reads
source.manifest.txt, which is written with WriteAllLines and so not atomically. A short or
changed manifest is kept until NM Fed next changes, and every -Set then copies about 208 MB,
fails at the read back, and says to give the next set number, which fails the same way and
leaves a copy with no manifest each time.

- Sources: F108-REV-4, F108-BRK-3, F108-R2
- Evidence: tools\loop\prepare-copy.ps1:290-308 (the keep test), :329 (the manifest written),
  :342 and :346-350 (the read back against it), and :349 (the advice 'Give the next set
  number', read on main 3449521). Proof case A6 of F108, one sha256 zeroed in the manifest, ends
  REFUSED at the read back, turn4\f108-proof.txt.
- Root cause: tools\loop\prepare-copy.ps1:290-308, :329 and :349. At f38edd5 the same keep test
  was at :194-206 and the write at :239, and nothing read the manifest content, so the damage
  did not show there.
- Class: loud failure
- Proof: No Core test applies. A proof case: zero one sha256 in source.manifest.txt, then -Set
  NN finds it at the plain check and remakes the source copy, or refuses naming
  source.manifest.txt, and its advice does not send to the next set number. The F108-R2 row's
  way out: the keep check reading the manifest too.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Loud each time, and it touches nothing of Bader's.

### FR-103 remove-writes-note-before-delete

-Remove writes its note (source.removed.txt or NMFed.removed.txt) before it deletes the file. If
the delete fails, for example a read only file or one a scanner holds, the note names a file
still in the copy, -Remove says already out, -Restore says already in the copy, the plain
command and -Set NN refuse, and only deleting the note by hand ends it. A -Restore that fails
after the .partial copy leaves the .partial.

- Sources: F108-REV-3, F108-BRK-4, F108-R3
- Evidence: At f38edd5, tools\loop\prepare-copy.ps1:155 '[IO.File]::WriteAllLines($removed, ...'
  then :156 'Remove-Item -LiteralPath $f.FullName'. The same order on main 3449521 at :229-231
  (WriteAllLines of $targetRemoved, then Remove-Item, then the check that the file is gone), and
  the .partial at :247-251.
- Root cause: tools\loop\prepare-copy.ps1:155-156 and :173-177 at f38edd5, and :229-231 and
  :247-251 on main.
- Class: loud failure
- Proof: No Core test applies. A proof case: mark the NWC read only or hold it open, -Remove
  fails, and no note is left standing, or the failure removes the note. Set 05: item 3 reads
  NMFed.removed.txt only for a file really gone. The F108-R3 row's way out: the note written
  after the delete.
- Branch: on main since 2026-09-27 and copied into -Set by F108, merged as c9b223b.
- Note: F106 check 8 (run.ps1:347) names the state and refuses, so it is loud.

### FR-104 set-remove-advice-sends-to-a-refusal

-Set NN -Remove on a set whose copy failed its read back says to run -Set NN alone first, and
-Set NN then refuses because the folder is there.

- Sources: F108-BRK-8, F108-R4
- Evidence: tools\loop\prepare-copy.ps1:224 'Run $make first' with $make = '-Set $Set alone'
  (:85), against the refusal at :258-263. Lines 85 and 224 read so on main 3449521.
- Root cause: tools\loop\prepare-copy.ps1:85, :224 and :258-263.
- Class: loud failure
- Proof: No Core test applies. A proof case on a copy with no manifest: -Remove names the real
  next step.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Words only, the advice is wrong. The F108-R4 row's way out: each made true where its
  file next changes.

### FR-105 run-verdict-wording-and-dead-fields

Run verdict faults: HUNG, a forced call and CEILING are decided before the end state, UNKNOWN
wins when the end state later reads gone, one unreadable sample ends the hold, a forced close
still exits 0, a foreign Roamer that changed no setting shows only in the put back reasons, and
two sync fields were set and never read.

- Sources: T3-G12
- Evidence: At f38edd5, tools\loop\run.ps1:464-466 against :468-469 (the order), :472 'RAN ...
  closed, FORCED' with exit code 0, :559-565 the monitor ends the run on one 'unreadable'
  sample, and :571, :574 and :1067 set $sync.LogAmbiguous and $sync.ToolLog, and a grep found no
  read of either. tools\loop\nw-guard.ps1:1204-1209 put a foreign Roamer in the reasons only. On
  main 3449521 RunVerdict is at run.ps1:697-731, where HUNG, END FORCED, a forced call and
  CEILING are still decided before the end state and 'RAN ... closed, FORCED' still takes code
  0. $sync.ToolLog is now read (run.ps1:1753 and :1780 on), and $sync.LogAmbiguous is still only
  set (:979 and :1578).
- Root cause: RunVerdict and the Monitor in tools\loop\run.ps1, the field set and never read,
  and PutBackReasons in tools\loop\nw-guard.ps1 (:1204-1209 at f38edd5, PutBackReasons at :1279
  on main).
- Class: noise
- Proof: Harness cases in tools\loop\prove-run.ps1 H18 (RunVerdict is pure and the harness calls
  it) for each order, and LogAmbiguous read or removed. No Core test applies.
- Branch: F106 rewrote RunVerdict and the monitor, merged as 3449521, so the f38edd5 lines moved
  as given above.
- Note: The sharper edge is the forced close that exits 0 as RAN. The text side of a close that
  fails twice is FR-107, and FR-083 is the separate fault that RAN ignores the group results.

### FR-106 record-writes-unmasked-exception-text

run.ps1 writes an exception's text, which holds the full path of a file of Bader's, into the
record unmasked at three lines. Run's record is copied into steps\runs, and Install's stays in
the loop folder.

- Sources: T3-G13 (placed by the register reader and the readings reader, folded here)
- Evidence: At f38edd5, tools\loop\run.ps1:903 'Say ("STOP: the installed bundle cannot be read
  whole, " + $inst.Why', :1014 '$stopText = "STOP before the start: " + $lb.Why' and :1020 the
  same with $ab.Why. Why is built from Err at run.ps1:286, :289, :304 and :329 and
  nw-guard.ps1:37-41, which prints the exception message unmasked. run.ps1:1211-1215 copy
  record.txt into steps\runs. On main 3449521 the three lines are at run.ps1:1384, :1499 and
  :1505, and Err is at nw-guard.ps1:47.
- Root cause: the three STOP lines in tools\loop\run.ps1, with the Why texts built in
  ListFolder and BackupNew and by Err in tools\loop\nw-guard.ps1.
- Class: noise
- Proof: A new prove-run.ps1 case: make ListFolder or BackupNew fail on a file under a fake
  profile path and assert the record holds the path masked the way install.ps1's REFUSED lines
  are (MaskLine, at nw-guard.ps1:76 on main). No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: No real start's record holds such a line. The breaker on Bader's things called it noise
  and not a new leak, because the tree already holds the user name in 13 files. It is the kind
  item 12 of fix list 3 fixed, at lines it did not name. FR-109 is the wider masking question.

### FR-107 close-fails-twice-text-says-closed

When the watchdog's close fails and the end close's Kill fails too, CloseAtEnd still writes that
the process was CLOSED here, and CloseOwn then refuses the folder because it holds a VERDICT
line, so only a person can end that Navisworks.

- Sources: T3-G16 (placed by the register reader and the readings reader, folded here)
- Evidence: At f38edd5, tools\loop\run.ps1:493 '... so it was CLOSED here through the held
  handle: ' + $cr.Text, where $cr.Text can read 'was sent Kill through the held handle and still
  runs' (nw-guard.ps1:576), and :687 'has a VERDICT line, so its run finished. Nothing was
  closed'. On main 3449521 the text is at run.ps1:744 in CloseAtEnd (:732-755), the refusal at
  :1177 in CloseOwn (:1174), and CloseAdopted at nw-guard.ps1:659.
- Root cause: CloseAtEnd and CloseOwn in tools\loop\run.ps1.
- Class: noise
- Proof: A new prove-run.ps1 case with CloseAdopted returning State same: the end text says the
  process was not closed, and CloseOwn accepts a folder whose VERDICT line is STOPPED with the
  process still running. No Core test applies.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: Classed polish and old by the breaker on ownership. The verdict side of it is FR-105.

### FR-108 run-ps1-polish-closeown-buildserver-ownertext

Polish in run.ps1, the harness and the probe: CloseOwn reads autosave-before.txt with no count
or name check, dotnet build-server shutdown after an install stops every build server of the
account, an owner window destroyed mid read is written as a window of another process with its
handle, item 8's harness case sets Forced by hand and never makes the watchdog's Kill fail, and
the probe closes the adopted process outside the close lock, which run.ps1's CloseAtEnd takes.

- Sources: T3-P2 (one register item and two readings items, folded here)
- Evidence: At f38edd5, tools\loop\run.ps1:727 'DiffAutoSave (ReadListing $abFile ...' with no
  AutoSaveBefore check (:367-377 does it for Run), :927 'RunChild "dotnet" "build-server
  shutdown" $repo', and :666-669 OwnerText, where OwnerPid reads 0 for a destroyed owner and so
  differs from the adopted pid (nw-guard.ps1:451-458). On main 3449521 these are at
  run.ps1:1217, :1408 and :1156-1159. tools\loop\prove-run.ps1:1353 '$sy2.Forced = "the adopted
  deadline of 5 s passed, the close of pid ... through the held handle threw"' (in :1348-1356).
  tools\probes\probe-automation-start.ps1:869 and :889 call CloseAdopted with no CloseLock. The
  parts are listed in steps\notes\f103-final-reading.md:111-116.
- Root cause: tools\loop\run.ps1 (CloseOwn, the Install flow, OwnerText), tools\loop\nw-guard.ps1
  (the owner read), tools\loop\prove-run.ps1:1348-1356 and
  tools\probes\probe-automation-start.ps1:869 and :889.
- Class: noise
- Proof: prove-run.ps1 cases: CloseOwn with a cut autosave-before.txt names the difference,
  Install says which servers it stops or stops none it did not start, a destroyed owner reads as
  owner gone, and the watchdog's own close fails for real (a stand-in that survives Kill, or a
  CloseAdopted that returns State same). The probe's two closes take the lock. Each part gets its
  own harness case only when it is taken. No Core test applies.
- Branch: the run.ps1 lines moved with F106 (3449521) and are given above. prove-run.ps1 and
  probe-automation-start.ps1 read the same on main.
- Note: Q93: words and polish are register rows and not fix attempts. OwnerText was written by
  fix attempt 3 of F103. The probe is F100's measurement and keeps its own copy by design
  (tools\probes\README.md), so the lock change is the lead's call.

### FR-109 f106-evidence-carries-profile-paths

Evidence written into steps\runs before F102's mask carries the full profile path. The tool's
log copy masks only its FOLDERS REMEMBERED block, its .tsv and the first line of every workbook
read-out go in as written, and DIALOG and CONFIRM texts go into record.txt unmasked.

- Sources: F106-HARM-REV-15, F106-HARM-BRK-9
- Evidence: steps\runs\03\item1-C06\run-20261001-140037.log:1 'Log opened at
  C:\Users\<profile>\AppData\Local\ParsonsNwcFederator\logs\run-20261001-140037.log' and 720 more
  lines, the .tsv 177 lines, and line 1 of each of the 22 workbook read-outs 'workbook
  C:\Users\<profile>\AppData\Local\NwcFederatorLoop\runs\03\NMFed\Clash Report\C06\...xlsx', 920
  lines in 24 files, 987 times on them, and 22 more lines of the log hold it as the author on
  the NWD publish line, log:482 for one, 1009 times on 942 lines in all, read with grep on
  2026-10-04 into turn5\profile-name-count.txt. Code: tools\loop\run.ps1:1792-1796 (MaskRemembered only), :1802 (the
  tsv copied as is), :1059-1060 (the texts), and tools\loop\read-workbook.ps1:102.
- Root cause: tools\loop\run.ps1:1792-1802 and :1059-1060, and tools\loop\read-workbook.ps1:102.
  The kinds F102 masks are in tools\checks\evidence-ids.txt and name no profile folder.
- Class: noise
- Proof: No Core test applies. A check-evidence-ids.sh and mask-evidence.ps1 case: a line holding
  the profile folder is masked and refused. A prove-run.ps1 case that the record's DIALOG and
  CONFIRM lines go through MaskLine. Set 05: the committed log, tsv and read-outs hold no profile
  folder.
- Needed Bader, asked as Q105, answered on 2026-10-04, see the answered line of this item. Whether his Windows profile name is a trace to keep out of committed evidence
  was his call, answered on 2026-10-04, Q105, because F102 masks only the machine name and the licensing ids and the tree
  already holds the name in many files. Masking the record's own DIALOG and CONFIRM texts with
  the existing MaskLine needs no answer.
- Branch: the run.ps1 part rests on F106, merged as 3449521, lines unchanged. read-workbook.ps1
  was on main before.
- Answered by Bader on 2026-10-04 at 15:23, Q105, in short, his words being under the question in steps\02_questions.md: mask the profile folder name in paths and as the author, and the evidence check refuses this machine's name, the older machine's name and both account names, the older ones held as a hash. The lead's note: to be worked by F117, the names, in wave 5 with the noise.

### FR-110 evidence-check-blind-to-older-machine-name

Nothing in the repo refuses the older machine's name once F107 masks it, so it can come back and
every check stays green. F102's mask and check read for the name of the machine they run on and
for no other.

- Sources: F107-BRK-2, F107-R1
- Evidence: tools\checks\check-evidence-ids.sh:17 'The machine name is read for only when
  COMPUTERNAME' and :106 'machine=${COMPUTERNAME:-}', and tools\checks\evidence-ids.txt:67-71,
  where the machine name kind uses {machine}. Main named that machine on 9 lines at f38edd5, for
  example docs\history\scan.md:3123 and tools\probes\drive-window-run.ps1:21, and F106 has since
  rewritten that driver header (the readings reader saw 'the machine of 2026-09-19' at line 69 on
  fix-F106). The F107-R1 row (turn5\pr83-diff-rest.txt:116) adds that copies of files holding the
  name sit in the loop folder.
- Root cause: tools\checks\check-evidence-ids.sh:17 and :106 and tools\checks\evidence-ids.txt:67-71.
- Class: noise
- Proof: No Core test applies. A tools\checks\broken fixture that carries the older name and is
  refused, and a clean file that passes, run by Actions. The name itself must not go back into
  the rules file in clear, so a hash or another way is needed, UNKNOWN how, as the F107-R1 row
  says.
- Needed Bader, asked as Q106, answered on 2026-10-04, see the answered line of this item. Q88 asked only for the mask on main. Making the check refuse a second machine
  name is new scope and a design choice, and the F107 section itself says the check would never
  refuse that name.
- Branch: waits for F107's merge, fix-F107 not merged at main 3449521.
- Answered by Bader on 2026-10-04 at 15:23, Q106, in short, his words being under the question in steps\02_questions.md: the evidence check refuses the older machine's name, held as a hash. The lead's note: to be worked by F117, the names, in wave 5 with the noise.

### FR-111 f106-link-check-skips-output-folders

ParamRefusal looks for a junction or link only on the run folder, the NWC folder, the XML and
the open file. The copy's NWF, NWD and Clash Report folders, where the tool writes, are not
checked, and the driver's UnderRoot compares path text only, so a link made by hand there would
carry the tool's writes out of runs\NN.

- Sources: F106-HARM-REV-3, F106-HARM-BRK-7
- Evidence: tools\loop\run.ps1:231-240, the PathRefusal loop over Nwc, XmlFile and OpenSource
  only, :355-369 (OutputsRefusal checks the folders exist and are empty, not for a link), and
  tools\probes\drive-window-run.ps1:116-120 (UnderRoot).
- Root cause: tools\loop\run.ps1:231-241 and tools\probes\drive-window-run.ps1:116-120.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case: a junction at NMFed\NWF\<Folder> is refused
  by ParamRefusal or Check with a REFUSED line.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Not reachable now, F108's copy makes plain folders.

### FR-112 f106-logs-after-changed-file-counts-covered

LogsAfterWindow treats a file of his that CHANGED as covered whenever logs-backup holds its old
content, so a changed file of his stays changed and never counts as not put back. Q82 allows
only the pruning of his oldest logs, which reads as GONE.

- Sources: F106-HARM-REV-10
- Evidence: tools\loop\run.ps1:599-601, CHANGED is labelled a FINDING when the backup holds the
  old content, and Lost is counted only in the else.
- Root cause: tools\loop\run.ps1:592-609 (LogsAfterWindow), :599-601.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case on LogsAfterWindow: a CHANGED file of his
  counts as not put back.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Nothing in a run changes an existing log of his today, so low odds.

### FR-113 f106-toollog-name-first-log-wins

toollog-name.txt names the one new log that carries the installed stamp at the first pass that
sees exactly one, so a tool session of Bader's own whose SESSION line lands before the loop's
would be named as the loop's, and the close of the loop, which removes what this file names,
would take his log.

- Sources: F106-HARM-REV-9
- Evidence: tools\loop\run.ps1:966-972, when exactly one log is found toollog-name.txt is
  written. The removal of the loop's own log is not built yet, run.ps1:1788-1791 leaves both
  files for the close of the loop.
- Root cause: tools\loop\run.ps1:962-972.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case: a first log whose content is not the plugin
  call's session is not named, and the close reads the log's own content before it removes
  anything.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Unlikely inside one 15 s pass. FR-094 is the same choice of log seen from the hang clock.

### FR-114 f106-answer-handle-reuse-after-lock-wait

The driver's Answer is handed the confirm's handle before Gate, and after a long lock wait it
checks again only the pid and the button name, not that the window is still the confirm, so a
handle reused by another dialog of the same pid that has an OK button would be answered.

- Sources: F106-HARM-BRK-6
- Evidence: tools\probes\drive-window-run.ps1:235-247: Gate at :236, the pid check at :238-239,
  the button name at :242, WM_COMMAND posted at :245, with no IsConfirm read again.
- Root cause: tools\probes\drive-window-run.ps1:235-247.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case: after a lock wait the confirm is read again
  with IsConfirm and a different dialog is not answered.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Very low odds.

### FR-115 f106-confirm-buttons-matched-by-english-names

The confirm's OK and Cancel buttons are matched by their English UI Automation names, so on a
Windows display language other than English the driver sends nothing to the confirm and the run
ends only when the hang rule fires after about 5 minutes.

- Sources: F106-HARM-BRK-3
- Evidence: tools\probes\drive-window-run.ps1:242 'if ($b.Current.Name -cne $what)', with Answer
  called as 'OK' at :377 and 'Cancel' at :374.
- Root cause: tools\probes\drive-window-run.ps1:241-243.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case with a stand-in confirm whose buttons carry
  other names: the driver answers by button id 1 and 2 alone.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: Bader's machine is English, so it bites only a loop run on another.

### FR-116 f106-no-free-space-check

run.ps1 makes no free space check on C: before the start, though the backups, the outputs and
the adopted Navisworks's autosaves all land there. A failed backup copy stops the run safely,
and a full disk during the run is not read.

- Sources: F106-HARM-BRK-10
- Evidence: tools\loop\run.ps1:1497-1506, checks 13 to 15 hold no DriveInfo.
  tools\loop\prepare-copy.ps1:214-219 at f38edd5 does check room for its copy.
- Root cause: tools\loop\run.ps1:1497-1521.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case with a stubbed free figure under the needed
  size: Check and Run refuse with one line. The needed size is a setting.
- Branch: rests on F106, merged as 3449521, lines unchanged.

### FR-117 watchdog-load-unmeasured

The watchdog lists every process every half second for the whole run and sends WM_GETTEXT reads
to the adopted Navisworks's windows, proved for minutes and never for hours, so its load on
Bader's machine while he works is unmeasured.

- Sources: F106-HARM-BRK-11
- Evidence: tools\loop\nw-guard.ps1:885 '$procs = @(Get-Process)' and :1048 'Start-Sleep
  -Milliseconds 500' at f38edd5, :977 and :1140 on main 3449521. Run line:
  steps\runs\03\item1-C06\record.txt:826 'watchdog passes 12073, longest pass 7763 ms, longest
  gap 8270 ms'. Its processor time is in no record.
- Root cause: the Watchdog loop in tools\loop\nw-guard.ps1 (:877-1049 at f38edd5, :969-1141 on
  main).
- Class: noise
- Proof: No Core test applies. The watchdog's end line (run.ps1:1144 at f38edd5) gains the
  watchdog's processor seconds against the run's seconds, a prove-run.ps1 case checks the line,
  and set 05 record.txt shows the figure.
- Branch: lines moved with F106 (3449521), the main lines are given above.
- Note: UNKNOWN whether it slows his machine or the tool's run. Item 0 runs show longest passes
  of 8.9 to 11.8 s (record.txt:86 of steps\runs\02\item0 and :85 of steps\runs\01\item0).

### FR-118 set-number-rule-and-readback-written-twice

The two digit set number rule is written as a strict pattern in prepare-copy.ps1 and as a looser
one in run.ps1, where \d also takes digits of other writing systems and $ takes a trailing
newline. The read back of the source copy and the read back of the set's copy are two near
copies of one check.

- Sources: F108-REV-6, F108-R4
- Evidence: tools\loop\prepare-copy.ps1:74 '\A[0-9]{2}\z', read on main 3449521, against the
  run.ps1 rule '^\d\d$' (run.ps1:147 at f38edd5, :195 on main), and prepare-copy.ps1:325-327
  against :347-349.
- Root cause: tools\loop\prepare-copy.ps1:74, :325-327 and :347-349, and tools\loop\run.ps1:195
  on main.
- Class: noise
- Proof: No Core test applies. A prove-run.ps1 case that run.ps1 refuses a set number with a
  trailing newline and with digits outside ASCII.
- Branch: rests on F108, merged as c9b223b, and run.ps1's rule moved with F106 (3449521).
- Note: CLAUDE.md: one rule lives in one place. Every difference ends in a refusal, so nothing
  is harmed. Polish.

### FR-119 probe-finalizer-claim-printed-as-fact

A named limits line prints what the finalizer does as a fact of this run, but it was read off the
IL of run 3 and no code on the run checks it.

- Sources: T3-G4
- Evidence: tools\probes\probe-automation-start.ps1:951 'Dispose did not complete and the
  finalizer was not suppressed, so at this process's exit it calls Bridge.Terminate, against a
  process that is gone if it was closed. What that does is UNKNOWN'. The header at :147-149 says
  the same.
- Root cause: tools\probes\probe-automation-start.ps1:951, and :147-149.
- Class: noise
- Proof: No test applies. The line says it was read off run 3, or reads UNKNOWN, checked by the
  probe's -ReflectionOnly output and a reader.
- Note: Old, from F100. The probe is a one off measurement and run.ps1 does not print this line.
  probe-automation-start.ps1 reads the same on main 3449521 as at f38edd5.

### FR-120 probe-main-window-read-by-pid-alone

After adoption the probe reads MainWindowHandle and MainWindowTitle by pid alone, without the
start ticks check that WindowsOf makes.

- Sources: T3-G5
- Evidence: tools\probes\probe-automation-start.ps1:760-761 '$mp = Get-Process -Id $myPid' then a
  Say of MainWindowHandle and MainWindowTitle, again at :768-769 and :774-775.
- Root cause: tools\probes\probe-automation-start.ps1:760-761, :768-769 and :774-775.
- Class: noise
- Proof: No test applies. A reader checks the diff and the copy of the probe's harness 4 is run
  again.
- Note: The register records that the breaker says MainWindowTitle sends WM_GETTEXT to whatever
  holds the pid and the reviewer says GetWindowText sends none. Which is right is UNKNOWN. The
  adopted pid is held through a handle at that point, so no other process can have it.

### FR-121 probe-polish-stale-sentences-and-unnamed-limit

Probe polish: a sentence that went stale when B3 landed, an always empty before set still read,
the work folder moved aside before step 2 can refuse, and the gap between the last Roamer read
and the constructor call not named as a limit.

- Sources: T3-P
- Evidence: tools\probes\probe-automation-start.ps1:151-153 rename the work folder before step 2.
  :140-149, the named limits omit the gap between the last read at :728 and the constructor at
  :739. tools\loop\run.ps1:72-74 '$beforeTicks = @{}' at f38edd5 (:94 on main 3449521), always
  none, is still read at nw-guard.ps1:786, :894 and :1197 (:878, :986 and :1289 on main). The
  stale sentences were at lines 395 and 1836 of b01ad71, and which lines they are on main is
  UNKNOWN.
- Root cause: tools\probes\probe-automation-start.ps1:140-153 and the reads of $beforeTicks in
  tools\loop\nw-guard.ps1. The two stale sentences: UNKNOWN.
- Class: noise
- Proof: No test applies. The diff is read and the probe's -ReflectionOnly output is compared.
- Branch: the run.ps1 and nw-guard.ps1 lines moved with F106 (3449521) and are given above.
- Note: Q93 words and polish.

### FR-122 f105-il-reader-prints-zero-for-kinds-never-attempted

The shared IL reader prints a zero for every failure kind in every probe, kinds the probe never
attempts included, and 5z-f reads those zeros as clean.

- Sources: F105-R1, F105-REV-6 (the F105 reviewer's finding of the same code fault)
- Evidence: On main since F105 merged as 2c89788: tools\probes\il-reader.ps1:71-84, the $IlFailureKinds table,
  and :104-112, IlFailureLines walks every key of it and prints 'label: count' even at zero.
- Root cause: tools\probes\il-reader.ps1:104-112, on main since F105 merged as 2c89788.
- Class: noise
- Proof: No Core test. Each probe run again on this machine prints only the kinds it attempts,
  and a reviewer reads the diff.
- Branch: F105 merged as 2c89788 on 2026-10-04, so this item can be worked.
- Note: Plan step 2 merges F105 first. FR-146 corrects the register wording of this row.

### FR-123 f105-failure-list-only-printed-with-built-addin

One of F105's probes prints its failure list only when a built add-in is there.

- Sources: F105-R2
- Evidence: On main since F105 merged as 2c89788: tools\probes\probe-viewpoint-calls.ps1:318-320 test whether
  $AddinPath is there, and the list is printed inside the else at :396-402 'IlFailureLines'.
- Root cause: tools\probes\probe-viewpoint-calls.ps1:318-411, on main since F105 merged as 2c89788.
- Class: noise
- Proof: No Core test. The probe is run with no built add-in and still prints the list.
- Branch: F105 merged as 2c89788 on 2026-10-04, so this item can be worked.

### FR-124 f105-probes-share-resolver-and-helpers-in-copies

A reflection resolver and two small helpers still sit in more than one of F105's probes instead
of one copy.

- Sources: F105-R3
- Evidence: On main since F105 merged as 2c89788: add_ReflectionOnlyAssemblyResolve at
  probe-viewpoint-calls.ps1:66, probe-roamer-switches.ps1:332 and probe-clash-report-api.ps1:52,
  and function ParamText at probe-viewpoint-calls.ps1:77 and probe-clash-report-api.ps1:53.
- Root cause: The lines in the evidence, on main since F105 merged as 2c89788.
- Class: noise
- Proof: No Core test. A grep for each name finds one definition, in tools\probes\il-reader.ps1.
- Branch: F105 merged as 2c89788 on 2026-10-04, so this item can be worked.
- Note: Which second helper is meant is not written in the row. ParamText is one that is
  certainly doubled. FR-146 corrects the register wording of this row.

## Docs and words, FR-125 to FR-149

### FR-125 five-failed-groups-need-note-for-modellers

Five groups ended FAILED because a model was exported on Revit's internal origin and not on a
shared site, six models in all, and every output was still written, so the models need exporting
again and Bader asked for a short note he can forward.

- Sources: S03-11, C06-J8, C06-J9, C06-J10, C06-J11, C06-J12, C06-J13, FIND-09, FIND-10,
  FIND-11, FIND-12, FIND-13, C06-DONE-10, Q98 B4
- Evidence: log:967 (1B06BC AR), log:1762 (1B06G1 AR), log:2774 (1B06M1 ST), log:5802 (1B06PS
  AR and ST), log:7899 (1C06M2 ST), and log:8359-8364 'ALIGNMENT failed 5 group(s), each because
  a model was exported on the internal origin or names no shared site. Every one of them still
  wrote its NWF, its NWD and its report.' The sites: log:583, 1335, 2489, 5511, 5514 and 7614
  each read shared coordinate 'Internal'. Groups failed 5 at log:8433, no group PARTIAL at
  log:8432.
- Root cause: Not a code fault, the rule does what Q70 b says
  (src\Federator.Core\Health\AlignmentCheck.cs:243-287). The deliverable is
  steps\runs\03\for-modellers.md (Q98 B4), which did not exist when the readers read.
- Class: loud failure
- Proof: No Core test. steps\runs\03\for-modellers.md exists, names each group, model file, the
  site it names and its log line, and is read by the claim-checker against log:583, 1335, 2489,
  5511, 5514 and 7614. The five groups can only read DONE or PARTIAL in set 05 once the models
  are exported again.
- Note: 1B06M1 and 1C06M2 are one Revit building, 1B06MM, published under two names with the
  same 20 clashes (log:8336). In 1B06BC and 1B06PS the model named Internal is the reference or
  sits where the others do not, and the models 2,774 km and 336 km away (EL) and 2,823 km away
  (PS ST) are not named in the FAILED reason (log:583, 584, 5512, 5514), so the note names them
  too, as FR-001's line will in the log. The reason sentence is repeated three times per group
  by design, and C06-J13, the RESULT block repeating the five errors in full, is by design too
  and goes when the models are exported again. steps\runs is behind the paths wall, so the note
  is written elsewhere and copied in by a command, the way the runner collects evidence. FR-006
  asks whether two of the five should fail at all.
- Closed: steps\runs\03\for-modellers.md was written, read by a claim-checker against the log and
  merged in PR 89 as 51a0cb6 on 2026-10-04.

### FR-126 single-discipline-sentence-says-every-test-is-created

The SINGLE DISCIPLINE finding and the window row say every clash test is still created in a one
discipline group, and the CLASH block of the same group says 36 of 1830 were created. Since F77
only the tests whose two sides both find something are created.

- Sources: S03-14, C06-J21, FIND-05, T1-S46, step 94, step 92 (the register row at
  steps\loop.md:568)
- Evidence: log:112 'The federation is still built and every clash test is still created, and
  none of them is run, because one discipline cannot clash with itself' for 1B06BS, against
  log:1125 'CLASH 1830 in the file, 36 created, 1794 not created, a side finds nothing' and
  log:1167 'tests created : 36'. The same sentence at log:119, 126, 133, 136 and 143 for the
  other five groups. steps\03_bader_next.md:205 already calls the window text 'Ready. One
  discipline, so every test is created and none is run.' false. The run's own CLASH line at
  src\Federator.Addin\Engine\FederationEngine.cs:2557-2559 already says it right, and core.md
  says that since F77 they are not all created either.
- Root cause: src\Federator.Core\Findings\ScanFindings.cs:366-368 (the log and the findings
  panel), src\Federator.Core\Grouping\BuildingGroup.cs:55-57 (a comment),
  src\Federator.Addin\Ui\GroupRow.cs:153-157 (a comment) and :257 (the window status text). What
  the code does is src\Federator.Addin\Engine\ClashRunner.cs:1708 CreationPlan.For and
  :1690-1693 (no XML creates nothing), before the single discipline return at :692-703.
- Class: noise
- Proof: New Core test in tests\Federator.Core.Tests\Findings\ScanFindingsTests.cs: the SINGLE
  DISCIPLINE detail does not say every test is created and says only the tests whose sides find
  something are created (the test at :297-310 asserts the headline only). Fails before. The
  GroupRow label moves into Core with a case in
  tests\Federator.Core.Tests\Diagnostics\LabelWordsTests.cs, since core.md says every label
  wording lives in Core where a test can read it, or it is read twice as add-in text. Run line of
  set 05: the six detail lines (set 03 log:112, 119, 126, 133, 136, 143) no longer say every
  clash test is still created.
- Note: tests\Federator.Core.Tests\Clash\SingleDisciplineGroupTests.cs:18 and :146 carry the same
  old wording in a comment and an assertion message. Words only, no number is wrong. FR-058 is
  the related first CLASH line. ScanFindings.cs is also FR-154's file, and GroupRow.cs FR-160's
  and FR-165's.

### FR-127 tolerance-help-line-says-resets-results

The grey line under the tolerance drop down says changing a saved test resets results, which 5y
measured is false and which the confirm screen then contradicts.

- Sources: T1-S28
- Evidence: src\Federator.Core\Clash\ToleranceChoice.cs:78-79 HelpLine = 'Beats the XML and the
  document. Changing a saved test resets results', shown by
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1040 'ToleranceHelp.Text =
  ToleranceChoice.HelpLine'. The same class says at :50-52 'Changing the tolerance on a test
  already in the document RESETS NOTHING' and WarningLines at :239 'Every recorded result and
  every status a person set is KEPT.'
- Root cause: src\Federator.Core\Clash\ToleranceChoice.cs:78-79.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Clash\ToleranceChoiceTests.cs: HelpLine does not
  say results are reset. It fails today. ToleranceChoiceTests.cs:216-223 checks only that the
  line is twelve words and holds no code identifier, so the new wording stays at twelve words or
  fewer.
- Note: The class's own comment at :53-56 says the wrong wording made a person hesitate over an
  action that costs nothing. The same old claim survives in ClashRunner.cs:1145-1146 ('because
  an edit resets its results') and in the log line at :1106-1108, which belong to FR-128. Not
  shown by set 03, the label text is in no file.

### FR-128 apply-line-says-results-reset-contradicts-kept

With Apply the file's settings on, one run writes a drift block saying results are KEPT and then
an APPLIED line per test saying the edit reset them, and the window help under the box says it
resets clashes to New, so at least one statement is wrong.

- Sources: T1-S5
- Evidence: src\Federator.Addin\Engine\ClashRunner.cs:1106-1108 'the file's settings were put
  onto the test in the document, which reset its results', against :175 'IT DOES NOT RESET
  ANYTHING BY ITSELF' and :1069-1070 'It RESETS NOTHING by itself, measured 5y'.
  src\Federator.Core\Clash\TestDrift.cs:307 'APPLYING these to the tests in the document. Their
  recorded results and the statuses on them are KEPT'.
  src\Federator.Addin\Ui\FederatorWindow.xaml:566 'Resets those clashes to New. Differences are
  listed either way.' The same split is inside ApplyChosenTolerance: the comment at :1145-1146
  'because an edit resets its results' against the log line at :1199 'Its results and its
  statuses are KEPT, measured 5y'.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:1106-1108 (Apply, called at :985 when
  ApplyFileSettings is on).
- Class: noise
- Proof: A measurement first, then words. Run: on a copy of an NWF with Apply the file's settings
  ticked, against an XML that differs from a saved test holding results with statuses, read the
  result count and the status count before the edit, after it, and after a save and a reopen.
  Then every statement above agrees with what the run shows. After that a Core test can pin the
  one wording once the APPLIED sentence, the drift block line and the help lines live in Core
  beside TestDrift and ToleranceChoice. This run is not in the standard five of set 05.
- Note: PARTLY: the contradiction is real but the APPLIED line is not known to be false. The 5y
  measurement covered TestsReplaceWithCopy on a copy that already held the test's results with
  only the tolerance changed. Apply edits from a fresh ClashTest that holds no results through
  TestsEditTestFromCopy and changes type, merge and both sides as well. Whether that keeps the
  results and statuses is UNKNOWN and nothing in the repo measured it. No count, number or file
  is wrong because of this line, so the harm is words, though about whether review history was
  lost. If the run shows the edit does reset results, whether the box stays is a question for
  Bader, UNKNOWN until then. Not reached in set 03: 'apply file to old: no'.

### FR-129 failure-guard-words-say-first-n-tests

The repeated failure guard says the first N tests all failed for the same reason, but N failures
in a row can follow any number of good tests anywhere in the run, so the log and the window can
tell a reader that nothing ever worked.

- Sources: T1-S27
- Evidence: src\Federator.Core\Clash\RepeatedFailureGuard.cs:61 'the first ' + consecutive + '
  tests all failed for the same reason' and :81 'The run was stopped. The first ' +
  consecutive. :90-94, RecordSuccess sets consecutive to 0. One guard is built for the whole run
  at src\Federator.Addin\Engine\FederationEngine.cs:143 and handed to every ClashRunner.
- Root cause: src\Federator.Core\Clash\RepeatedFailureGuard.cs:61 and :81.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Clash\RepeatedFailureGuardTests.cs: record 199
  successes and then 50 failures, and assert Reason and ReasonInPlainWords do not say first. It
  fails today. RepeatedFailureGuardTests.cs:159-168 already asserts '50' and 'the rest of the run
  was not attempted' and stays true.
- Note: Stopping is right, per the class summary at :9-11, so only the words are wrong. The rule
  text in .claude\rules\core.md says 'After the first 50 tests', the same word. Not reached in
  set 03: no test failed in any group. The image guard of FR-076 uses the same class.

### FR-130 refill-message-counts-rows-not-cells

The refill message says rows were typed over by hand and left alone when only one of their three
names was, because the other two were refilled, and with one row and nothing kept it says '1
name refilled.' when three names were.

- Sources: T1-S54
- Evidence: src\Federator.Core\Naming\OutputNameTable.cs:178-181 'if (row.WasEdited)' counts the
  row kept when any cell was typed, :183-191 still refill every cell not typed by hand, :206
  builds 'name refilled.' for one row with nothing kept, and :209-211 build 'rows were typed
  over by hand and left alone'. It reaches the window through
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:580 and :638-640.
  tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs:130-136 types one cell in each of two
  rows and asserts '2 rows were typed over by hand and left alone'. The driver of set 03 typed no
  name.
- Root cause: src\Federator.Core\Naming\OutputNameTable.cs:202-212 (DescribeRefill, :206 and
  :209-211), with the count at :176-181.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs: a row with only
  its NWF name typed gives a message that says one name was kept and two refilled, and one row
  with nothing kept does not say '1 name refilled'. Fails today. The assertion at :136 pins the
  old wording and changes with the fix, which is a wording fix and not a weakened test.
- Note: core.md makes the per cell refill the rule, so the refill is right and only the message
  is wrong. OutputNameTable.cs is also the file of FR-153, FR-159, FR-160 and FR-165.

### FR-131 outputs-label-wrong-reason-when-nwf-inside-source

When the NWF folder sits inside the scanned folder, the label under the Outputs step says the
NWF folder cannot be read, which is not the reason. The run itself stops with the right reason
in the log and the dialog.

- Sources: T1-S58
- Evidence: src\Federator.Core\Report\ReportPaths.cs:128-131 'catch (Exception)' returns 'not
  worked out yet, the NWF folder on this step cannot be read' and swallows the ArgumentException
  thrown on purpose at :178-183 'is inside the folder being scanned'. The only caller is
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1641 (WorkbookFolderOrWhyNot), read into
  OutputsSummary.Text at :1673. tests\Federator.Core.Tests\Report\WindowWordingTests.cs has no
  case where the NWF folder is inside the source folder. Set 03 did not reach it.
- Root cause: src\Federator.Core\Report\ReportPaths.cs:128-131, thrown at :178-183.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Report\ReportPathsTests.cs or
  WindowWordingTests.cs: WhereTheyGo with the NWF folder under the source folder returns words
  that name the scanned folder and do not carry Parameter name. Fails today.
- Note: The log half of the original finding does not hold, nothing logs WhereTheyGo. The label
  is the only harm.

### FR-132 rebuild-box-help-line-omits-removal

The grey line under the rebuild sets box says only sets whose question changed are touched, but
ticking the box also removes every set the file no longer names that nothing points at, and
renames others.

- Sources: T1-S69
- Evidence: src\Federator.Core\Sets\SetRebuildSettings.cs:39-40 'Only sets whose question
  changed. Results and statuses are kept, measured', shown at
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1189. src\Federator.Addin\Engine\SetBuilder.cs:506-509
  'if (rebuilds.RebuildDriftedSets)' runs HandleLeftovers, and CarryOut removes unused leftovers
  (:574-583) and renames pointed at ones (:597-614). ConfirmLine (SetRebuildSettings.cs:65) does
  say 'an unused one is removed'. Two comments keep the old scope: SetRebuildSettings.cs:20-22
  and src\Federator.Addin\Ui\FederatorWindow.xaml:540-545 'it destroys nothing'. The window
  label is in no file of set 03.
- Root cause: src\Federator.Core\Sets\SetRebuildSettings.cs:39-40, and the comments at :20-22.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Sets\SetDriftTests.cs next to
  TheLabelIsAtMostEightWordsAndTheHelpLineAtMostTwelve (:120, which only counts words): HelpLine
  names the removal and stays at twelve words or fewer. Fails today. The line is 11 words now, so
  one word of headroom.
- Note: The confirm screen names the removal before the run, so a person is told once, which is
  why this is noise. Whether the box now belongs under Things that destroy data is a layout
  choice for Bader and is not needed for the wording fix. The removal itself is FR-013 and
  FR-014.

### FR-133 savedviewpoints-canbuild-switch-nothing

SavedViewpoints.CanBuild always returns true and nothing reads it, yet three places call it the
one switch for viewpoints.

- Sources: T1-B2
- Evidence: src\Federator.Addin\Engine\SavedViewpoints.cs:59-62 'public static bool CanBuild'
  with 'get { return true }', and the doc at :54-57 'While this was false the engine asked for no
  viewpoints'. A grep of src finds only that and
  src\Federator.Addin\Engine\FederationEngine.cs:3179 'SavedViewpoints.CanBuild is the one
  switch, true since the viewpoints round.' BuildViewpoints gates only on 'if (outcome.Report ==
  null)' (:3183). .claude\rules\addin.md:261 and CLAUDE.md:190 say the same. A grep of tests finds
  none.
- Root cause: src\Federator.Addin\Engine\SavedViewpoints.cs:59-62, with the comment at
  src\Federator.Addin\Engine\FederationEngine.cs:3179.
- Class: noise
- Proof: None needed, no number or count is wrong, and no Core test applies because the member is
  in the add-in. The proof is the add-in build with 0 errors and a grep of src, tests,
  .claude\rules and CLAUDE.md for CanBuild finding nothing. The VIEWS lines of the set 05 first
  run read as before.
- Note: CLAUDE.md says a public member nothing in src calls is deleted with its tests unless a
  decision in steps\02_questions.md keeps it, and none does. The CLAUDE.md and addin.md edits are
  rule edits.

### FR-134 stale-units-comment-in-finish-the-group

A comment F30 moved says the units are set before the clash step so the report reads in its
units, which F26 made untrue because ReportUnits converts the finished report.

- Sources: NEW-LOG, F30 comment stale since F26 (steps\loop.md:611)
- Evidence: src\Federator.Addin\Engine\FederationEngine.cs:915-917 'BEFORE the clash step, so
  every tolerance and every distance is read in the units the report is going out in. Converting
  afterwards would mean a report whose numbers and whose unit label disagree.' steps\log.md:5811
  says it is stale since F26.
- Root cause: src\Federator.Addin\Engine\FederationEngine.cs:915-917.
- Class: noise
- Proof: No test reads a comment. The comment is reworded to what F26 does, and a grep for the
  old sentence reads 0.
- Note: src\Federator.Addin\Engine\DocumentUnits.cs:12-18 'Converting the numbers ourselves is not
  an option' and Core ReportOptions.cs:57-64 carry the same old reasoning and are read with it.
  DocumentUnits.cs is also FR-062's file.

### FR-135 docs-bader-next-says-c06-never-existed

steps\03_bader_next.md says the C06 folder and the 1B06 group names were never on his machine,
and about twenty steps still name groups from the old C02 and C04 folders, while the run scanned
C06 with 67 NWC and 22 groups from 100000 to 1C06PK.

- Sources: step NEW, steps 9-17 and every step naming C0 (steps\loop.md:556)
- Evidence: steps\03_bader_next.md:16-18 'THE C06 FOLDER AND THE 1B06* GROUP NAMES ARE GONE FROM
  THIS FILE. They were never on his machine, and every count in here derived from fourteen
  groups was wrong for that reason.' against log:58 'groups built : 22', driver.txt:7 '67 NWC
  found, 67 readable' and log:74-95. steps\runs\03\findings.md:11 '17 DONE, 0 PARTIAL, 5 FAILED
  of 22' on C06, and :121 'Steps 9 to 17, which say C06 never existed, while C06 has 22 groups
  here'.
- Root cause: steps\03_bader_next.md:9-31, the header block written on 2026-09-20 about the C02
  and C04 folders, and the steps that name C02 and C04 groups. Steps 39, 95, 100, 101, 121, 123,
  136 and 281 still name the C06 folder.
- Class: noise
- Proof: No test. A read of steps\03_bader_next.md:9-31 after the fix: the block states the
  folders the loop actually runs, C06 with 22 groups (one reader adds C07 with 24 groups and no
  NWF, measured on this machine), says no folder never existed, and carries no count derived
  from the old 14 groups. The claim-checker reads it against log:58.
- Note: The file is rewritten and not appended when the proofs change (the steps rules). A Look
  for line is never changed to match wrong output, and only the false statement about the
  machine is meant here. Q94 made code run the whole of NM Fed itself, so most of this file is
  what the loop's run set now does, and the lead may rewrite or retire it. The NEW-LOG row that
  the proof steps of 03_bader_next never worked as a whole (steps\loop.md:613) is left out, its
  fault part being this item.

### FR-136 docs-step-377-log-bar-300-kb

Step 377 asks for a .log under 300 KB and the 22 group log is 1,097,850 bytes, about 50 KB a
group, so the step reads CONTRADICTED, and on what the bar was set is UNKNOWN.

- Sources: step 377 (F81, steps\loop.md:595)
- Evidence: steps\03_bader_next.md:824-827 'check the .log is under 300 KB and the .tsv still
  carries a row for every test' against log:8584 'COPY written ... 1,097,850 bytes' and
  record.txt:809 'the log 1098049 bytes'. The tsv half holds, 3215 'test created' and 3024 'rows
  for the workbook' rows. steps\runs\03\findings.md:119-120 lists F81 and step 377 as
  CONTRADICTED.
- Root cause: steps\03_bader_next.md:826, a bar whose basis is UNKNOWN, while CLAUDE.md says a
  number in a doc is measured, never estimated. Which blocks make most of the 1.1 MB is not
  measured. The trimming is
  src\Federator.Core\Diagnostics\RunLog.cs:644-720 (collapsedLines).
- Class: noise
- Proof: No test. After the choice, steps\03_bader_next.md:826 states the bar, and the set 05
  RESULT line 'COPY written ... bytes' meets it.
- Needed Bader, asked as Q108, answered on 2026-10-04, see the answered line of this item. Two ways out and the choice is his: trim the log further, or move the bar to
  a size per group, measured at 49.9 KB a group here. A Look for line is never changed to match
  wrong output (.claude\rules\loop.md, how a finding is worked, rule 6), so the bar moves only if
  he accepts the size.
- Note: The RESULT sizes printing 0 and 678,363 bytes are a separate fault, FR-046.
- Answered by Bader on 2026-10-04 at 15:23, Q108, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Measure which blocks and lines make the C06 log 1.1 MB, find repeated or wasted lines, and fix those. The tsv keeps every line in full, R6. Then set the size bar from the measured result. The lead's note: a noise item, so the fix is in wave 5.

### FR-137 rules-loop-md-says-logs-never-pushed-out

The loop rules say loop runs write their logs inside the work folder so the thirty logs the tool
keeps never push one of his out, and the run wrote its log into his logs folder and pruned his
oldest, which Q82 allows.

- Sources: S03-19, C06-J29, FIND-23, Q82
- Evidence: .claude\rules\loop.md:80-83 'Loop runs write their logs inside the work folder, so
  the thirty logs the tool keeps never push one of his out.' against log:1 'Log opened at
  C:/Users/<profile>/AppData/Local/ParsonsNwcFederator/logs/run-20261001-140037.log' and log:3
  'RETAIN keeping 30 logs, deleted 1, could not delete 0'. record.txt:1037 'GONE
  run-20260901-191711.log, logs-backup holds it'. steps\02_questions.md:440, the Q82 answer: the
  window runs may prune his oldest logs.
- Root cause: .claude\rules\loop.md:80-83. Q82's answer ends 'To be carried out in turn 4 of the
  loop, steps\loop.md', and the sentence was not changed on main at 3449521.
- Class: noise
- Proof: No test. A read of .claude\rules\loop.md:80-83 after the fix: it says what Q82 says,
  that the window runs may prune his oldest logs because logs-backup holds each by sha256 and his
  folder is put back at the close of the loop.
- Closed: PR 83, merged as 086a348 on 2026-10-04, rewrote this rule to say what Q82 says, and
  F106, merged as 3449521, rewrote the README line in the note below, so neither stands on main.
- Note: Nothing was lost, record.txt:1040 reads 0 LOST. tools\loop\README.md:199 at f38edd5 still
  says part 1 waits for Q82, which was answered on 2026-10-01, and PR 83 does not change that
  line. The prune itself (S03-19, C06-J29, FIND-23) is not a fault.

### FR-138 rules-say-loop-never-writes-autosave

.claude\rules\loop.md still says the only folder the loop deletes from is its own folder and that
nothing is ever written into his AutoSave folder, which F106's put back does under Q86.

- Sources: F106-HARM-BRK-12
- Evidence: .claude\rules\loop.md:75-79 'The only folder the loop deletes from is
  %LOCALAPPDATA%\NwcFederatorLoop' and :111-116 'nothing is ever written into his folder'.
  tools\loop\run.ps1:566 deletes and :579 copies into his AutoSave folder, F106's code now on
  main 3449521.
- Root cause: .claude\rules\loop.md:75-79 and :111-116, unchanged on main 3449521.
- Class: noise
- Proof: No Core test applies. A read of the two rules after the change: both name the Q86 put
  back.
- Closed: PR 83, merged as 086a348 on 2026-10-04, rewrote both sentences to name the Q86 put
  back.

### FR-139 rule-and-runner-still-say-source-copy

The loop rule says every run works on the copy under ...\source, the runner agent names
prepare-copy.ps1 -Remove or -Restore with no -Set NN, and run.ps1 Check reports only
source.manifest.txt and source.removed.txt. All three read wrong now that F108 and F106 are
merged and runs work on runs\NN\NMFed.

- Sources: F108-REV-1, F108-BRK-6, F108-R4 (the runner agent part)
- Evidence: .claude\rules\loop.md:73 'Every run works on the copy under
  %LOCALAPPDATA%\NwcFederatorLoop\source', .claude\agents\runner.md:21-22, and CheckMode in
  tools\loop\run.ps1 (:847-848 at f38edd5), which on main 3449521 still prints '----
  source.manifest.txt and source.removed.txt ----' (CheckMode at :1296).
- Root cause: .claude\rules\loop.md:73, .claude\agents\runner.md:21-22 and CheckMode in
  tools\loop\run.ps1.
- Class: noise
- Proof: No Core test applies. A read of the three, and a prove-run.ps1 Check case that lists
  NMFed.manifest.txt and NMFed.removed.txt.
- Closed in part: rests on F108 (c9b223b) and F106 (3449521). PR 83, merged as 086a348 on
  2026-10-04, rewrote the rule line to name runs\NN\NMFed, and its change to runner.md touched
  only the hang rule line, so the runner part and the Check part stay.
- Note: A runner that follows the old words would take a file out of source with the plain
  -Remove, which makes -Set refuse. CLAUDE.md step 3 of how a fix is worked asks the rule to
  change with the code.

### FR-140 f103w-prove-run-header-words

Three sentences in prove-run.ps1 say more than the code. The header says H17's copy of run.ps1
has its constructor line replaced by a line that throws, where the line only sets the error. It
says check 6 and check 7 would each refuse an Install call, where the second guard of Install is
the HEAD check. A check label says the put back's own writes are never listed as the start's,
where M5 lists what any program wrote.

- Sources: F103-W-1, F103-W-3, F103-W-8, F103-W
- Evidence: tools\loop\prove-run.ps1:23-25 'whose constructor line is replaced by a line that
  throws' against :1167, which sets $err to a new System.Exception reading 'HARNESS COPY: the
  constructor line is removed from this copy, and this line was reached'. Lines 19-22 'so check 6
  and check 7 would each refuse it' against run.ps1:894-896 at f38edd5 (check 6 then
  TreeRefusal). Line 284 'never listed as the start's'. The sentence at 23-25 survived the
  readings of fix attempts 1, 2 and 3.
- Root cause: tools\loop\prove-run.ps1:19-25, :284 and :1167, unchanged on main 3449521.
- Class: noise
- Proof: No Core test applies. A read of the three places after the change, and prove-run.ps1 run
  again on the changed file with the same pass count (242 passed on a63c284).
- Note: F103-W is at steps\loop.md:432. Words only, no change of logic. FR-092 changes the same
  file and goes first or with it.

### FR-141 f103w-readme-run10-exit-codes-wrap

tools\loop\README.md lists the harness runs after fix attempts 1 and 3 and has no entry for run
10 after fix attempt 2. It says RunVerdict and InstallVerdict decide the exit codes, where
run.ps1's main flow sets codes 2, 1 and 3 itself. Three lines are over long.

- Sources: F103-W-2, F103-W-4, F103-W-wrap, F103-W
- Evidence: At f38edd5, tools\loop\README.md:315-319 (no run 10), :209 'RunVerdict and
  InstallVerdict decide them', and run.ps1:934, :936 and :966, where the main flow sets $code to
  2 and 1. Over long lines at README.md:186, :218 and :319. F108 changed the README, and on main
  3449521 the sentence is at :263. The main flow of F106's run.ps1 still sets $code itself (for
  example run.ps1:1384, :1397 and :1499 on main).
- Root cause: tools\loop\README.md (:186, :209, :218 and :315-319 at f38edd5).
- Class: noise
- Proof: No Core test applies. A read of the README after the change: an entry for run 10, a
  sentence that names the exit codes the main flow sets, and the three lines wrapped with no word
  changed.
- Note: The wrap was noticed by the claim-checker, not by the readers (F103-W-wrap).

### FR-142 f103w-probe-and-run-comment-words

The probe's header says it reads no window of any other process, where the guard enumerates
every top level window and reads each one's process id and an owner that may belong to another
process. A comment in run.ps1 ends on a comma.

- Sources: F103-W-7, F103-W-9, F103-W
- Evidence: tools\probes\probe-automation-start.ps1:96 'It reads no window of any other process'
  against tools\loop\nw-guard.ps1:363-367 and :414-427 (EnumWindows over every window, the owner
  read). The comment '# Keep awake, the session lock and what changed outside the loop folder
  while a start ran,' at tools\loop\run.ps1:380 at f38edd5, :612 on main 3449521.
- Root cause: tools\probes\probe-automation-start.ps1:96 and the comment in tools\loop\run.ps1,
  now at :612.
- Class: noise
- Proof: No Core test applies. A read of the two lines after the change. run.ps1 changes only a
  comment, so the harness shows the diff and nothing else.
- Note: The probe is F100's measurement, so the change keeps to the words. FR-143 is the same
  claim in F106's driver.

### FR-143 f106-driver-header-no-desktop-search-claim

The driver's header says it never searches the desktop, while the window reads it relies on
enumerate every top level window with EnumWindows and keep those of the adopted pid. Only each
window's pid is read, so this is wording and not a search.

- Sources: F106-HARM-REV-13
- Evidence: tools\probes\drive-window-run.ps1:29 'It never searches the desktop' against
  tools\loop\nw-guard.ps1:414-427 (WinHandlesOf over EnumWindows).
- Root cause: tools\probes\drive-window-run.ps1:29.
- Class: noise
- Proof: No Core test applies. A read of the header after the change.
- Branch: rests on F106, merged as 3449521, lines unchanged.
- Note: The finish brief's 'no desktop walk' holds only in that sense.

### FR-144 junction-recursion-claim-contradicted

A comment and the README say Windows PowerShell 5.1 follows a junction when it recurses, and the
F108 developer's scratch test on 5.1.26100.9444 says it did not. Which is right is UNKNOWN. If
the old claim is right, Find-One and the recursive deletes pass a junction planted inside a copy.

- Sources: F108-REV-2, F108-BRK-7, F108-R4
- Evidence: At f38edd5, tools\loop\prepare-copy.ps1:79-80 'because Windows PowerShell 5.1 follows
  a junction when it recurses' and tools\loop\README.md:30-31, with Get-ChildItem -Recurse at
  :110 and Remove-Item -Recurse at :222. On main 3449521 the comment is at
  prepare-copy.ps1:123-124 and the README line at :45. The scratch test is kept in no file.
- Root cause: the comment in tools\loop\prepare-copy.ps1 and the README line, with the recursion
  in Find-One and the deletes (:122-127, :183, :315 and :321 in F108's version).
- Class: noise
- Proof: No Core test applies. A measurement in docs\history\scan.md with its date: a scratch
  junction, Get-ChildItem -Recurse and Remove-Item -Recurse on Windows PowerShell 5.1. Then the
  words follow it, or Find-One refuses a link inside the copy if 5.1 does follow one.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Nothing in the loop plants a junction in a copy today, and the guard at
  prepare-copy.ps1:105-115 in F108's version refuses one at the folders above the copy.

### FR-145 readme-write-list-names-only-source-copy

The README's line of what prepare-copy.ps1 writes outside the repo names only the source copy and
source.manifest.txt. runs\NN\NMFed, NMFed.manifest.txt and NMFed.removed.txt appear only in the
-Set bullets further down.

- Sources: F108-REV-5, F108-R4
- Evidence: tools\loop\README.md:18-22 against :30-44, read the same on main 3449521. The lead's
  PR body for F108 (turn5\pr-f108.md:12-13) calls it register row F108-R4.
- Root cause: tools\loop\README.md:18-22.
- Class: noise
- Proof: No Core test applies. A read of the README line after the change.
- Branch: rests on F108, merged as c9b223b, lines unchanged.
- Note: Words only. CLAUDE.md says the tools\loop README says what each script writes outside the
  repo.

### FR-146 f105-register-wording-attempts-and-helpers

Register row F105-R1 and Q89 say the probes never attempt some failure kinds, and F105-R3 says
two small helpers sit in more than one probe. In plain words the probes do read those kinds and
only never count a failure of them, and by function name one helper plus the resolver repeats,
the second helper being UNKNOWN.

- Sources: F105-BRK-2, F105-BRK-5, F105-REV-5, F105-REV-1 (its part on main)
- Evidence: steps\loop.md:433 'kinds a probe never attempts included' and :435 'A resolver and
  two small helpers', and steps\02_questions.md:468 and :470 'kinds never attempted', all read
  the same on main 3449521. On main since F105 merged as 2c89788, probe-roamer-switches.ps1:595 reads constant values and
  probe-clash-report-api.ps1:73 reads field types with no count, ParamText is in
  probe-clash-report-api.ps1:53 and probe-viewpoint-calls.ps1:77, and $roResolve is in all three
  and in probe-automation-start.ps1:558.
- Root cause: steps\loop.md:433 and :435 and steps\02_questions.md:468 and :470. The same words
  in docs\history\scan.md 5z-f were narrowed by F105, merged as 2c89788.
- Class: noise
- Proof: No Core test applies. A read of the two register rows after the change, and a
  correction line under Q89 that does not alter his answer.
- Closed in part: F105, merged as 2c89788 on 2026-10-04, changed rows F105-R1 and F105-R3 to
  the narrowed words and added F105-R4. What stays is the correction line under Q89.
- Note: steps\loop.md is the lead's to write. The branch part of F105-REV-1 and F105-BRK-2 was
  fixed in F105, merged as 2c89788, and this is the part on main. F105-REV-1 also said the lead may want Bader
  told Q89 was partly wrong.

### FR-147 restated-facts-check-missing

The check that refuses a broken restatement of the four named facts does not exist, and the
facts have drifted three times in prose.

- Sources: F96 (steps\loop.md:513)
- Evidence: steps\01_next.md:46 'F96, the restated facts check, FIRST ITEM OF THE NEXT ROUND' and
  :925 'Files a new tools/checks/check-restated-facts.sh'. tools\checks holds check-imports.sh,
  check-locals.sh and check-evidence-ids.sh and no check-restated-facts.sh.
- Root cause: tools\checks\check-restated-facts.sh does not exist, and steps\01_next.md:925
  specifies it.
- Class: noise
- Proof: No Core test. tools\checks\check-restated-facts.sh run by Actions over the tree, and
  over a broken fixture under tools\checks\broken for each of the four facts (the solid category
  list, the 150 mm threshold, the tick box count, the event kinds), where it refuses.
- Note: The shape to copy is
  PenetrationRuleTests.TheHelpLineNamesEverySolidTheRuleCoversAndStaysWithinTwelveWords, which
  iterates the code value and asserts the restatement names every entry. steps\01_next.md gained
  17 lines since f38edd5, so its line numbers are read again.

### FR-148 audit-180-findings-without-verifier

180 of the 393 findings of the 2026-09-12 audit were never read by a second reader, because 24
of 54 verifier batches died on a usage limit.

- Sources: NEW-LOG, 180 findings of the 2026-09-12 audit carry no verifier (steps\loop.md:612)
- Evidence: steps\04_audit.md:171 'the other 24 batches died on the session's usage limit before
  they ran, so 180 findings ... carry a reader's evidence and my grep and no verifier'.
  steps\log.md:5556 says the same and that no journal is kept in the repo.
- Root cause: UNKNOWN as a code location. It is a reading backlog in steps\04_audit.md:171.
- Class: noise
- Proof: No Core test. Each of the 180 is checked again by grep and marked verified or refuted in
  steps\04_audit.md.
- Note: A backlog kept as an item. It is a reading job and not a code fault, and most of that
  audit became F39 to F46 and later rounds. The lead may choose to skip it, since the turn 1 read
  of the whole repo on 2026-09-27 read the code again.

### FR-149 older-machine-account-name-left-in-tree

F107 masks the older machine's name, which Q88 asked for, and leaves behind the account name of
that machine's user, in C:\Users\<name> paths in committed probe results, docs\history\scan.md
and steps\logs.

- Sources: F107-BRK-6
- Evidence: docs\history\scan.md:3465 holds a
  C:\Users\<name>\AppData\Local\Temp\claude\round-viewpoints\probe path. A read finds that
  account's C:\Users path in 37 files outside the worktrees, among them INSTALL.md:8,
  tools\probes\fixture.md and 17 files under steps\logs, which the paths wall keeps unedited.
- Root cause: docs\history\scan.md:3465, INSTALL.md:8, tools\probes\ViewpointProbe\*-result-*.txt
  and steps\logs\*.log on main.
- Class: noise
- Proof: No Core test applies. A grep of the tree for the account name over the files that may be
  edited, the 20 of the 37 outside steps\logs before and none after, as Bader's answer to Q107 asks.
- Needed Bader, asked as Q107, answered on 2026-10-04, see the answered line of this item. Whether the account name of the older machine counts as a trace he wants
  masked is his choice, Q88 named only the machine name. steps\logs cannot be edited.
- Branch: works after F107's merge, which masks the machine name in the same files, fix-F107 not
  merged at main 3449521.
- Note: The account name is written <name> here, so the draft adds no copy of it.
- Answered by Bader on 2026-10-04 at 15:23, Q107, in short, his words being under the question in steps\02_questions.md: mask the older account name in the 20 files outside steps\logs, the check refuses it, held as a hash, and steps\logs stays untouched. The lead's note: to be worked by F117, the names, in wave 5 with the noise.

## Rest, FR-150 to FR-174

### FR-150 apply-file-settings-does-not-save-nwf

After the file's settings are applied to a saved test that is then skipped, nothing asks for the
NWF to be saved again, so the NWD carries the edit and the NWF does not, and the next run finds
the same drift and applies it again.

- Sources: T1-S4
- Evidence: src\Federator.Addin\Engine\ClashRunner.cs:1102
  'clashTests.TestsEditTestFromCopy(existing, replacement)' and then only :1106-1108, the CLASH
  APPLIED log line, with no changedTheDocument. ApplyChosenTolerance sets it at :1193
  'changedTheDocument = true'. src\Federator.Addin\Engine\FederationEngine.cs:2725 'return
  clash.CreatedCount > 0 || clash.RanCount > 0 || runner.ChangedAStatus'. :959 saves again only
  if clashPutSomethingIn or viewsPutSomethingIn.
- Root cause: src\Federator.Addin\Engine\ClashRunner.cs:1102-1108 (Apply), with
  src\Federator.Addin\Engine\FederationEngine.cs:2725.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Clash\ClashWorkTests.cs: a rule over the counts
  (created, ran, a status changed, applied) that asks to save again when the applied count is
  above 0. It fails today because no such rule exists and the engine's expression leaves the
  applied count out. Run proof: a rerun of a single discipline group with an XML that differs and
  the apply box ticked, then the tolerance read back from the NWF on disk (not in the standard
  five).
- Note: Narrow path. Apply is reached only when an XML is picked, the apply box is ticked and
  drift is found (ClashRunner.cs:964-985), and the test is then skipped at :692-703 or :707-722.
  If the test runs, RanCount is above 0 and the NWF is saved. Not reached in set 03: 'apply file
  to old: no, differences are reported and nothing is changed'. The same family as FR-020 and
  FR-043 to FR-045, and the same file as FR-036.

### FR-151 rebuild-tally-before-minus-one-reads-as-none

A viewpoint count that could not be taken before the clear, minus one, is read as nothing to
keep, so the clear and rebuild path counts the viewpoints as kept, logs 'none, the NWF held none
before the clear' and saves the NWF over without knowing whether they came back.

- Sources: T1-S61
- Evidence: src\Federator.Core\Rerun\RebuildTally.cs:65 'get { return Before <= 0 ||
  AfterRestore >= Before }', :75 'get { return AfterAppends >= 0 && AfterRestore >= 0 }' ignores
  Before, :91-94 'none, the NWF held none before the clear', and the constructor at :20-26 leaves
  Before at 0. src\Federator.Addin\Engine\SavedViewpoints.cs:88-93 return -1 when the count
  throws. src\Federator.Addin\Engine\FederationEngine.cs:1721 'views.Before =
  SavedViewpoints.Count(document)' and :1811-1812 fill the after counts, then :1833 'if
  (!tally.EverythingKept)' is the only gate before WriteNwf(document, job, outcome) at :1840. The
  reshape path does the same at :1398 and :1512. tests\Federator.Core.Tests\Rerun\RebuildTallyTests.cs
  has no case with Before at -1. addin.md says a count that could not be taken reports minus one
  and holds the NWF shut. Set 03 rebuilt nothing (log:8438 'rebuilt : 0').
- Root cause: src\Federator.Core\Rerun\RebuildTally.cs:65 and :75, with Before never defaulted to
  minus one at :20-26, fed by src\Federator.Addin\Engine\FederationEngine.cs:1398 and :1721.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Rerun\RebuildTallyTests.cs: Before -1 with
  AfterAppends 0 and AfterRestore 0 gives WasCounted false or EverythingKept false and a NOT
  COUNTED line. Fails today.
- Note: Order matters. This lands before any change that makes DocumentCensusReader.Sets return
  minus one (FR-164), because RebuildTally would read that Before as nothing to keep and turn a
  loud failure into a silent keep. The viewpoints are rebuilt each run, so the loss when it
  fires is limited. FR-067 is the same clear and rebuild path.

### FR-152 undo-auto-reviewed-undoes-a-persons-reviewed

After one undo a clash is at New and still carries this tool's record, so if a person then sets
it to Reviewed by hand, the next undo reads the same record, sees Reviewed, and moves it back to
New as put back where the tool found it.

- Sources: T1-S31
- Evidence: src\Federator.Core\Clash\UndoAutoReview.cs:60-70: no record gives NotOurs, a status
  other than Reviewed gives SomebodyMovedItOn, and otherwise putBackTo = record.WasAt and
  PutBack. src\Federator.Addin\Engine\ClashStatusEditor.cs:240-245 'An undo writes none: the
  record it is undoing stays as the history of what happened', with the record written only when
  the change is not an undo. src\Federator.Addin\Engine\UndoAutoReviewed.cs:188-208 OurComment
  returns the last comment carrying the marker.
- Root cause: src\Federator.Core\Clash\UndoAutoReview.cs:55-71 (Judge asks only for a record and
  Reviewed), with src\Federator.Addin\Engine\ClashStatusEditor.cs:240-245, which writes no
  record on an undo.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Clash\AutoReviewRecordTests.cs, where
  UndoAutoReview.Judge is tested: a clash that carries one record, was undone once, and is
  Reviewed again is not PutBack on the second undo. It fails today because Judge cannot tell the
  tool's Reviewed from a person's. The fix needs an undo to leave its own record and Judge to
  read the last one, so writing that record in ClashStatusEditor is add-in work, proved by a run
  with the Undo button pressed twice around a hand set Reviewed, which is not in the standard
  five, so that run line is UNKNOWN.
- Note: It breaks core.md 'ONLY WHERE NOBODY HAS MOVED IT SINCE' and 'NEVER OVERWRITE A
  DECISION'. Whether Navisworks keeps or adds comments when a person changes a status in the
  panel is UNKNOWN and does not change the verdict, since OurComment reads only comments with the
  marker. AutoReviewRecord.MayUndo (AutoReviewRecord.cs:162) and WhyNotUndone (:171) hold a
  second copy of the same judgement with the same gap, and nothing in src calls either. The
  unnamed clash shape of FR-033 also sits at UndoAutoReviewed.cs:173-175. Not reached in set 03:
  the undo was not pressed. AutoReviewRecord.cs is also FR-034's file.

### FR-153 regroup-throws-away-typed-names-and-run-ticks

Unticking one file in one building rebuilds every group row, so every hand typed output name in
every other building is lost and every group the person had unticked in the Run column is ticked
again.

- Sources: T1-S24
- Evidence: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:239-241 call Regroup() when the
  property that changed is Include, :254 groups.Clear(), and :288 'nameTable =
  OutputNameTable.From(result.Groups, naming, settings)' under the comment 'any hand edit
  belonged to groups that no longer exist'. src\Federator.Addin\Ui\GroupRow.cs:39 'row.include =
  true'. src\Federator.Core\Naming\OutputNameTable.cs:134-160 builds fresh rows with no by hand
  flag.
- Root cause: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:237-243, :254 and :288, and
  src\Federator.Addin\Ui\GroupRow.cs:39.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs: type a name in
  one group, rebuild after one file of another group is removed, and assert the typed name
  survives by group key. It fails today because From builds fresh rows. The Run tick lives in
  GroupRow in the add-in, so it is proved by a set 05 window run where the driver unticks one
  group's Run box, unticks one file in another building, and reads the Run box back.
- Note: The comment at :285-287 is false for any group whose key did not change. Regroup also
  leaves lastRefill as it was, so the preview can still say rows were typed over by hand and left
  alone after they were thrown away. A reverted NWF name could point the group at a different NWF
  path, and the only thing that may catch it is the F71 near NWF sentence, and only when the two
  names are close by that rule. Not reached in set 03: the driver pressed Scan, typed folders and
  pressed Run, and no tick changed.

### FR-154 scan-findings-use-group-key-as-building-code

The scan findings read the group key as the building code. In the per building and discipline
mode and the per discipline mode a mistyped code is not flagged, a confusable pair is written
once per discipline, and discipline codes are called building codes.

- Sources: T1-S45
- Evidence: src\Federator.Core\Findings\ScanFindings.cs:243 'string shape =
  Shape(group.Building)', :279 'if (holders.Count != 1)', :302 'The building code ' +
  odd.Building, and :325 'DescribeConfusion(left.Building, right.Building)'.
  src\Federator.Core\Grouping\BuildingGrouping.cs:119 'return name.Building + settings.Separator +
  name.Discipline' and :121 'return name.Discipline'. src\Federator.Addin\Ui\FederatorWindow.xaml.cs:313
  'findings = ScanFindings.From(result)' with no check on the mode. Set 03 ran one file per
  building only (log:59).
- Root cause: src\Federator.Core\Findings\ScanFindings.cs:243, :302 and :325, fed by the key built
  at src\Federator.Core\Grouping\BuildingGrouping.cs:118-121. BuildingGroup.BuildingCode
  (src\Federator.Core\Grouping\BuildingGroup.cs:43, set at BuildingGrouping.cs:98) now exists and
  ScanFindings never reads it.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Findings\ScanFindingsTests.cs, which groups only
  through GroupNames in the default mode today (:30, :146, :185, :422). Group 1C7BC with AR and
  ME beside ten normal codes through BuildingGrouping.Group in PerBuildingAndDiscipline and
  assert 1C7BC is flagged as an odd shape. Assert a 1B06PK against 1806PK pair gives one NEAR
  MATCH and not one per discipline. In PerDiscipline assert no sentence calls AR a building
  code. All fail today. A run cannot show it, the five runs use one file per building.
- Note: Only the three non default modes, so low reach. The same file writes a SINGLE DISCIPLINE
  finding for every group under both one discipline modes (ScanFindings.cs:357-370), because each
  group holds one discipline. UNKNOWN whether Bader means no clash test to run at all in those
  two modes, since the job takes the group's discipline count
  (src\Federator.Addin\Engine\FederationJob.cs:82-89). FR-167 has the same root in
  SourceMismatchFindings, and ScanFindings.cs is also FR-126's file.

### FR-155 exchange-reader-missing-attribute-becomes-zero-or-false

A missing or unreadable merge_composites, selfintersect, primtypes or flags attribute in the
clash XML becomes false or 0 with no word said, while a missing tolerance is skipped by name and
a bad one throws.

- Sources: T1-S41
- Evidence: src\Federator.Core\Exchange\ExchangeReader.cs:378-390, ReadInt returns 0 for an empty
  text and for text that does not parse, and :373-376 ReadFlag returns ReadInt(text) != 0.
  Callers: :139 merge_composites, :184-185 selfintersect and primtypes, :262 flags. The tolerance
  by contrast becomes HasTolerance at :136, and ReadDouble at :397-415 throws
  InvalidDataException.
- Root cause: src\Federator.Core\Exchange\ExchangeReader.cs:373-390 (ReadFlag and ReadInt), used
  at :139, :184-185 and :262.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Exchange\ExchangeReaderShapeTests.cs: read a
  clashtest with no primtypes and one with merge_composites set to x, and assert the test is
  skipped by name or the reader refuses, the way a missing tolerance is. It fails today because
  both read as 0 and false and are planned as buildable.
- Note: Latent: every sample file writes all four (all 1830 clashtests carry merge_composites 1,
  all clashselections selfintersect 0 and primtypes 1, all 102 conditions a numeric flags), but a
  file from another project or exporter might not. The worst case is primtypes 0 on a side, and
  whether Navisworks finds any clash on a side with no primitive types is UNKNOWN. An unreadable
  flags drops bit 64 or 32 and turns an OR into an AND or loses a negation. For a missing flags,
  0 may be the right default, UNKNOWN whether any exporter omits it. Not reached in set 03.

### FR-156 probe-csv-listed-written-after-write-threw

When File.WriteAllText throws for the probe CSV, the old CSV at that path, for example last
week's held open in Excel, is still logged as written with its size and named in the verdict
block as written.

- Sources: T1-S13
- Evidence: src\Federator.Addin\Engine\PropertyProbe.cs:114 'File.WriteAllText(csv,
  ProbeCsv.Text(tally.Rows()), Encoding.UTF8)' sits in a try whose catch at :116-122 logs a
  failure and says 'kept going', then :124 log.WriteFinished("CSV", csv) runs unconditionally and
  adds the path to the written list. src\Federator.Core\Probe\ProbeVerdict.cs:186 adds 'written
  to           : ' plus the path. RunLog.CheckOnDisk (RunLog.cs:1497) exists for a file that was
  checked rather than written.
- Root cause: src\Federator.Addin\Engine\PropertyProbe.cs:112-124.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Probe\PropertyProbeTests.cs: move the write into
  Core as a ProbeCsv write that returns whether it wrote, write into a folder under a file
  (which no system makes) and assert the path is not in the written list. It fails today because
  the write and the record are separate and the record runs unconditionally. Otherwise a run
  with the CSV open in Excel proves it.
- Note: The trigger is ordinary: a person opens last week's CSV in Excel, Excel locks it, and the
  next probe of that model throws on the write. Both callers reach it, FederationEngine.cs:2254
  and :2314. Not reached in set 03: only Scan and Run were pressed and the log has no PROBE line.
  CLAUDE.md says a file this tool did not write is never listed as written.

### FR-157 probe-unread-value-written-as-empty-value

A property value that will not read is tallied as an empty value, so the probe CSV cannot tell a
value that threw from one that is empty in the model.

- Sources: T1-S14
- Evidence: src\Federator.Addin\Engine\PropertyProbe.cs:205-209, a catch whose comment says 'Its
  own try, so one value that will not read costs one cell.' sets the value to string.Empty, and
  :211 tally.Add(category, tabName, Words.Or(property.DisplayName, string.Empty), value).
  src\Federator.Addin\Engine\ClashHarvest.cs:606-607 also maps VariantDataType.None to
  string.Empty.
- Root cause: src\Federator.Addin\Engine\PropertyProbe.cs:198-211.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Probe\PropertyProbeTests.cs: a marker for an
  unread value beside ProbeTally.CappedMarker, and a test that adds one unread and one empty
  value for the same property and asserts two different rows. It fails today because both are
  the empty string. The add-in side then passes the marker from the catch.
- Note: The CSV exists to show what the model really holds. How often ClashHarvest.Text throws in
  practice is UNKNOWN, since every known kind has its accessor and the default uses ToString. A
  throw, a None kind and a real empty display string all land in one row. Not reached in set 03:
  no probe ran.

### FR-158 probe-category-match-trims-but-tally-does-not

The probe accepts a category by trimming and ignoring case but counts the elements under the raw
spelling, so an element reading 'Pipe Fittings ' or 'pipe fittings' is walked and written to the
CSV while the verdict block lists Pipe Fittings under categories with no elements at all.

- Sources: T1-S55
- Evidence: src\Federator.Addin\Engine\PropertyProbe.cs:149 skips a category that is empty or
  that settings.Asks refuses, then :154 'tally.AddElement(category)' records the raw string.
  src\Federator.Core\Probe\ProbeSettings.cs:85-87, Asks goes to PenetrationSettings.Names, which
  trims and compares OrdinalIgnoreCase (src\Federator.Core\Clash\PenetrationSettings.cs:212-228).
  src\Federator.Core\Probe\ProbeTally.cs:42 holds an Ordinal dictionary and :84-88 ElementsIn
  looks up the exact string. src\Federator.Core\Probe\ProbeVerdict.cs:134 'if
  (tally.ElementsIn(category) == 0)'. No PropertyProbeTests case uses a trailing space or another
  case. UNKNOWN whether the client's models carry such a spelling. The probe did not run in set
  03.
- Root cause: src\Federator.Core\Probe\ProbeVerdict.cs:134 against
  src\Federator.Core\Probe\ProbeTally.cs:84-88, fed by
  src\Federator.Addin\Engine\PropertyProbe.cs:149-154.
- Class: silent wrong number
- Proof: Core test in tests\Federator.Core.Tests\Probe\PropertyProbeTests.cs: a tally holding an
  element added as 'Pipe Fittings ' and a verdict asking for Pipe Fittings does not list it as
  having no elements. Fails today. No run: the probe is a hand button and not in the five runs.
- Note: The mismatch is in Core, so a Core test proves it. The probe reads and changes nothing, so
  the harm is a wrong verdict line.

### FR-159 emptied-pattern-field-passes-name-check

Emptying one of the five pattern boxes turns the refusal sentence into the file name, and the
only check before a run, which looks at collisions only, passes it for one ticked group. With
several groups ticked the sentence is worded as a collision.

- Sources: T1-B8, T1-L3
- Evidence: src\Federator.Core\Naming\OutputNameTable.cs:243-246 'catch
  (InvalidOperationException error)' returns 'CANNOT BE NAMED: ' + error.Message, and :310-327
  WhyTheRunCannotStart returns null when no name collides. src\Federator.Core\Naming\NamePattern.cs:189-194
  throws new InvalidOperationException(unusable), and :90 'The level is empty, so the name would
  have a hole in it.' src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1701
  'nameTable.Only(TickedGroupKeys()).WhyTheRunCannotStart()' is the one check, then :1722-1730
  build the job with the sentence as the name. src\Federator.Core\Rerun\OutputPaths.cs:38 refuses
  only an empty name. The preview prints the sentence (FederatorWindow.xaml.cs:621-623). No test
  of the CANNOT BE NAMED path in OutputNameTableTests.cs. Set 03 emptied no pattern.
- Root cause: src\Federator.Core\Naming\OutputNameTable.cs:243-246 and :310-327, checked at
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1701.
- Class: broken feature
- Proof: Core test in tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs: empty
  naming.Nwf.Level on a one row table and assert WhyTheRunCannotStart names the empty field.
  Today it returns null. Then a run with a pattern box emptied shows the refusal.
- Note: T1-B8 and T1-L3 are one fault, seen from the preview and from the check, and one fix
  closes both. Dead code seen: FederatorWindow.SafeName (xaml.cs:324-343) has no caller. What
  SaveFile does with the colon in the sentence is UNKNOWN. Fixed with FR-165, both extend
  WhyTheRunCannotStart.

### FR-160 cleared-name-cell-pinned-as-empty-name

A name cell in the group table cannot be given back to the pattern. Clearing it pins an empty
name as typed over, ReleaseToPattern has no caller in src, and only a regroup that drops every
hand edit gets the pattern back.

- Sources: T1-B4, T1-B7
- Evidence: src\Federator.Addin\Ui\GroupRow.cs:118 'string tidied = value == null ? string.Empty
  : value.Trim()' and :126 'names.SetByHand(kind, tidied)' for any value, the empty one
  included. src\Federator.Core\Naming\OutputNameTable.cs:77-81 SetByHand sets byHand true, :84-87
  'public void ReleaseToPattern(OutputKind kind)' is called only by
  tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs:163, and :185-188 Refill then skips
  that cell for good. src\Federator.Addin\Ui\FederatorWindow.xaml.cs:288 'nameTable =
  OutputNameTable.From(result.Groups, naming, settings)' is the only reset. core.md says a cell
  can be given back to the pattern. steps\02_questions.md:113, Q24 asks to wire a way in or drop
  the sentence and the member, and had no answer until 2026-10-04. Set 03 edited no cell (driver.txt:3-19).
- Root cause: src\Federator.Addin\Ui\GroupRow.cs:111-129 with
  src\Federator.Core\Naming\OutputNameTable.cs:77-87.
- Class: broken feature
- Proof: If wired: a Core test in tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs that a
  cell typed empty goes back to the pattern on the next Refill, failing today, then a run of the
  grid. If dropped: the build and a grep for ReleaseToPattern returning nothing.
- Needed Bader, asked as Q24 on 2026-09-12 and answered on 2026-10-04, see the answered line of this item. Q24 was exactly this choice: wire a right click or a button
  per cell, or drop the sentence and the member with its test.
- Note: T1-B7 is the same fault from the Core side. FR-165 is a separate guard that is still
  needed whichever he chooses.
- Answered by Bader on 2026-10-04 at 15:23, Q24, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Test what happens to a typed-over name cell through a run, and whether a person can put it back to the pattern today. Find what is broken before deciding to wire or remove anything. The lead's note: in wave 4 with the rest.

### FR-161 date-format-has-no-control

NamePattern.DateFormat is a setting nothing in the window sets, so a dated NWD is always
yyyyMMdd and the fallbacks in NumberOrDate cannot be reached by a person, although core.md calls
the format the person's setting whose mistakes should be visible in the preview.

- Sources: T1-B6
- Evidence: A grep of src for DateFormat finds only src\Federator.Core\Naming\NamePattern.cs:33,
  :42, :69 (the public DateFormat property) and :139.
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:543-547 set Level, Discipline, TypeCode, Number
  and AllBuildings only, and the XAML has no box. Set 03: log:66 'NWD naming : overwrites', and
  the NWD names carry a number and no date.
- Root cause: src\Federator.Core\Naming\NamePattern.cs:69, never set by
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:535-548.
- Class: broken feature
- Proof: Run. A box on the Outputs step whose value reaches the preview and the NWD name with the
  dated NWD tick on. The Core half is already tested (OutputNameTableTests.TheDateFormatIsASetting
  at :264). Not in the five run plan, so a driver step is needed, UNKNOWN.
- Needed Bader, asked as Q109, answered on 2026-10-04, see the answered line of this item. The separator and the part positions are in the same state
  (steps\notes\f101-design.md:42 records that the window does not offer them). So the choice is a
  box on the Outputs step, or core.md reworded to say it is a code setting, and that is his
  because the window is meant to carry fewer decisions.
- Answered by Bader on 2026-10-04 at 15:23, Q109, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Test that each setting, the date format, the separator and the part positions, changes the NWD name the way it should, that a wrong value is refused with a clear line, and find where it breaks. The lead's note: in wave 4 with the rest.

### FR-162 folder-memory-save-failure-never-reported

FolderMemory.Remember drops the bool from Save and nothing reads DisabledReason, so when
folders.txt cannot be written every folder remembered in the session is gone at the next restart
and nothing in the log or the window says why.

- Sources: T1-S33
- Evidence: src\Federator.Core\Diagnostics\FolderMemory.cs:241-242 'folders[kind] = value' then
  Save() with the bool dropped, and Remember returns void. :283-286, Save catches, sets
  DisabledReason = error.Message and returns false. No code in src reads DisabledReason, only the
  setters at FolderMemory.cs:78, :120, :280 and :285. The eight window callers are
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:130, 913, 926, 1157, 1220, 1458, 1471 and 2129.
- Root cause: src\Federator.Core\Diagnostics\FolderMemory.cs:241-242.
- Class: broken feature
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\FolderMemoryTests.cs: Remember
  returns the reason, or false, when folders.txt cannot be written, using the unwritable path the
  test at :196-213 already builds. It fails today because Remember returns void. The window
  logging it is add-in work, seen in a set 05 window run only if a write can be made to fail, so
  that run line is UNKNOWN.
- Note: PARTLY: the two rules the finding cited are misread. The Try forms rule in addin.md is
  about Navisworks Try calls, and core.md asks only that an unwritable location is recorded as a
  reason, which line 285 does. What it breaks is CLAUDE.md 'The tool reports what it noticed' and
  'A public member nothing in src calls is deleted with its tests', because DisabledReason has no
  reader in src. The same root as FR-168. Whether folders.txt was written in set 03 is UNKNOWN.

### FR-163 title-bar-close-mid-run

Only the Close button refuses while a run is going. The title bar X and Alt+F4 are not guarded,
and the run's repaint pump lets them reach Close mid run with nothing to cancel them.

- Sources: T1-B3
- Evidence: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2588-2597, OnClose warns 'The run is
  still going. Let it finish.' and returns while running, and it is wired only at
  src\Federator.Addin\Ui\FederatorWindow.xaml:61 'Click=OnClose'. A grep of src\Federator.Addin
  for Closing finds nothing and the Window element (FederatorWindow.xaml:1) has no Closing
  attribute, line 102 only unhooks the log handler on Closed. Pump at :2511-2514
  'Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, ...)' runs on every
  SetProgress and Log. src\Federator.Addin\FederatorPlugin.cs:74 'window.ShowDialog()' with the
  Navisworks main window as owner. 'running' is set for the run, undo, probe, open file run,
  Build sets and Run tests.
- Root cause: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2588-2597 with Pump at :2511-2514 and
  no Closing handler anywhere.
- Class: broken feature
- Proof: Run. Press the title bar close during a run and expect a refusal like the Close
  button's, the run carrying on to RESULT. The set 03 loop posted WM_CLOSE only after RESULT
  (record.txt:803), so no run has tried it, and set 05 needs a mid run close step in the driver,
  UNKNOWN whether it has one. No Core test, this is window code.
- Note: What the Navisworks main window does when a dialog closes under a running job, and
  whether a later Warn on the closed window throws, are UNKNOWN.

### FR-164 census-sets-overload-returns-zero-not-minus-one

The sets count used by the rebuild returns 0 for a null collection and has no try, against the
census rule that a count which cannot be taken is minus one, and RebuildTally reads a Before it
could not take as nothing to keep.

- Sources: T1-S6
- Evidence: src\Federator.Addin\Engine\DocumentCensusReader.cs:91-94 return 0 when sets is null,
  and :96-99 count SetsUnder the RootItem in a using with no try, against the class rule at
  :24-27. The other overload at :71-86 does have a try. The calls at
  src\Federator.Addin\Engine\FederationEngine.cs:1396, 1510, 1719 and 1767 have no try around
  them. src\Federator.Core\Rerun\RebuildTally.cs:65 'return Before <= 0 || AfterRestore >=
  Before' and :75, WasCounted reads only AfterAppends and AfterRestore, so a Before of minus one
  reads 'none, the NWF held none before the clear' (:93).
- Root cause: src\Federator.Addin\Engine\DocumentCensusReader.cs:89-100, and the real hole at
  src\Federator.Core\Rerun\RebuildTally.cs:65 and :75, which is FR-151.
- Class: loud failure
- Proof: Core test in tests\Federator.Core.Tests\Rerun\RebuildTallyTests.cs: a RebuiltThing with
  Before minus one, AfterAppends 0 and AfterRestore 0 gives EverythingKept false. It fails today
  because Kept is true and WasCounted is true. That test goes with any fix here, because
  changing the overload alone to return minus one would make things worse, a loud failure would
  become a silent keep.
- Note: PARTLY. Not silent as the code stands: a throw fails the group loudly through the catch
  in RunOne (FederationEngine.cs:732-749) and saves nothing, and at :1396 it only costs the
  fallback to the clear and rebuild. The silent half needs a null collection, and whether
  document.SelectionSets can ever be null is UNKNOWN. The line beside it, 'tests.Before =
  SavedTests.Read(document).Count' at :1397, has no try either. Not reached in set 03: all 22
  groups were First run, rebuilt 0. FR-151 lands first.

### FR-165 cleared-name-cell-throws-out-of-run-click

Clearing one NWF or NWD name cell and pressing Run throws ArgumentException out of the click
handler, because the name check passes an empty name and the job is built with no try.

- Sources: T1-L4
- Evidence: src\Federator.Addin\Ui\GroupRow.cs:118 'string tidied = value == null ? string.Empty
  : value.Trim()' and :126 'names.SetByHand(kind, tidied)' with no empty check.
  src\Federator.Core\Naming\OutputNameTable.cs:296 'if (byName[name].Count > 1)' reports only a
  name more than one row shares, so one empty name passes (:310-327).
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1722-1726 build the job with
  'OutputPaths.Nwf(nwfFolder, group.NwfName)' in OnRun with no try, and
  src\Federator.Core\Rerun\OutputPaths.cs:38-41 throw ArgumentException 'An output needs a
  name.' The window knows the state: RefreshRunPaths at :681 treats an empty NwfName as Unknown
  and :1857 prints 'no output name', and neither refuses. An empty workbook name is not passed
  to OutputPaths (:1728), so it is not caught here. Set 03 cleared no cell (driver.txt:3-19).
- Root cause: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1722-1730 (no try) after
  src\Federator.Addin\Ui\GroupRow.cs:118-126, passing the check at
  src\Federator.Core\Naming\OutputNameTable.cs:310-327, thrown at
  src\Federator.Core\Rerun\OutputPaths.cs:38-41.
- Class: loud failure
- Proof: Core test in tests\Federator.Core.Tests\Naming\OutputNameTableTests.cs:
  WhyTheRunCannotStart refuses a row whose name is empty, for the NWF, the NWD and the workbook.
  Today it returns null for one such row. Then a run with one cell cleared shows the refusal.
- Note: If the exception leaves ShowDialog, src\Federator.Addin\FederatorPlugin.cs:78-80 would log
  it as 'starting the add-in ... stopped, the window never opened', which would be false. Whether
  Navisworks handles a dispatcher exception first is UNKNOWN. If Q24 is wired so an emptied cell
  goes back to the pattern (FR-160), this guard is still needed for any name that cannot be
  built. The same function as FR-159.

### FR-166 tolerance-other-blank-stops-hand-buttons

With the tolerance drop down on Other and the number box blank or not a number, ReportsWanted
throws from ToleranceChoice.Of, so the Undo, Probe, Build sets and Run tests buttons stop with
their own stopped message and the open file run fails inside its try, although the first three
never use the tolerance.

- Sources: T1-L2
- Evidence: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1508 'options.Tolerance =
  ChosenTolerance()' inside ReportsWanted. ChosenTolerance at :1087-1100 sets typed to
  double.NaN when the Other box does not read and calls ToleranceChoice.Of(typed), which throws
  ArgumentOutOfRangeException for NaN, infinity and below zero
  (src\Federator.Core\Clash\ToleranceChoice.cs:107-118). ReportsWanted is called by Undo
  (:1261), Probe (:1361), the open file run (:2243), Build sets (:2405) and Run tests (:2455).
  Only OnRun refuses first (:1743-1751). Set 03 left the tolerance box on Use the value in the
  XML (driver.txt:18).
- Root cause: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1508 (ReportsWanted) with
  :1073-1101, and src\Federator.Core\Clash\ToleranceChoice.cs:105-121 throwing.
- Class: loud failure
- Proof: Core test in tests\Federator.Core.Tests\Clash\ToleranceChoiceTests.cs: a parse of the
  Other text that does not throw and hands back a refusal reason for blank, text, negative and
  infinity, so the caller refuses before the run. Fails today because no such member exists.
  Run: Other chosen with the box blank, then Undo, Probe and Build sets run without a stop. Not
  in the five run plan.
- Note: Moving the parse into Core as a call that does not throw is the fix the Core test proves.
  The buttons are proved by a run. ToleranceChoice.cs is also changed by FR-036, FR-053, FR-063
  and FR-127.

### FR-167 shared-source-uses-group-key-not-building-code

SHARED SOURCE counts groups by the group key. In the per building and discipline mode and the per
discipline mode every Revit building with more than one discipline is reported as split by a
naming error, and the mismatch sentence says the federation will be called the NWC building code.

- Sources: T1-S47
- Evidence: src\Federator.Core\Findings\SourceMismatchFindings.cs:240-242 add
  one.GroupBuilding to the groups when it is not there yet, :250 reports any source with 2 or
  more groups, and :267 says 'That usually means one of the NWC file names is wrong'. :209-210
  say 'it will be called' the NWC building code. src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1722-1723
  hand the group key on to each FederationJob, then
  src\Federator.Addin\Engine\FederationEngine.cs:1193 RecordSource(building, cached, source) and
  :1216 'sourcePairs.Add(new SourcePair(building, nwcPath, sourceName))'. Set 03 ran per building
  only, and its SHARED SOURCE at log:8332 is two real buildings.
- Root cause: src\Federator.Core\Findings\SourceMismatchFindings.cs:240-242, fed with the group
  key from src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1722-1723 through
  src\Federator.Addin\Engine\FederationEngine.cs:1193 and :1216.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Findings\SourceMismatchFindingsTests.cs: pairs
  whose GroupBuilding are 1C07BC-AR and 1C07BC-ME with one Revit source building 1C07BC give no
  SHARED SOURCE finding, and the mismatch sentence does not say the federation will be called the
  building code in the per discipline mode. Fails today. A run cannot show it unless a run uses a
  non default mode.
- Note: A false alarm that blames a file name for what is only the chosen split, so noise and not
  a wrong number. The fix needs the group's building code on the pair (BuildingGroup.BuildingCode
  exists), or keying by the pair's NWC building, which passes through FederationJob and
  FederationEngine, add-in code that needs a run in a non default mode. UNKNOWN whether the five
  runs of set 05 use any non default mode. The same root as FR-154.

### FR-168 folders-memory-read-failure-logged-as-first-run

A folders.txt that throws on read leaves the memory empty and the startup block says 'Nothing
remembered yet', which reads as a first run and not as a file that would not read.

- Sources: T1-S23
- Evidence: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:95 log.Block("FOLDERS REMEMBERED",
  folders.Lines()). src\Federator.Core\Diagnostics\FolderMemory.cs:116-121, a catch whose comment
  says 'A settings file that will not read is not worth a message.' sets DisabledReason, and
  :312-315 add 'Nothing remembered yet. Every picker opens where it always did.'.
  DisabledReason has no reader in src.
- Root cause: src\Federator.Addin\Ui\FederatorWindow.xaml.cs:95 with
  src\Federator.Core\Diagnostics\FolderMemory.cs:116-121 and :312-315.
- Class: noise
- Proof: Core test in tests\Federator.Core.Tests\Diagnostics\FolderMemoryTests.cs: Lines() on a
  memory whose read threw names the reason and does not say 'Nothing remembered yet'. It fails
  today. The fix lives in Lines(), which a Core test can call.
- Note: PARTLY: the unwritable half works differently. The block is written before any Save, so
  it cannot know about a later write failure, and a readable but unwritable file lists its
  folders normally. That failed Save is FR-162. Where Load() itself fails, Path is null, nothing
  survives a restart and nothing says so either. Not reached in set 03: the block listed five
  folders, so folders.txt read fine.

### FR-169 document-guard-unreadable-name-called-unsaved

When the open file's name cannot be read, the warning before a clear calls it 'The current
unsaved document', and the throw is swallowed with no log line.

- Sources: T1-S7
- Evidence: src\Federator.Addin\Engine\DocumentGuard.cs:79-82, SafeFileName returns null from
  its catch, then :52-55 append 'The current unsaved document'. The text reaches the confirm in
  src\Federator.Addin\Ui\FederatorWindow.xaml.cs:1940-1942 'This will be discarded without
  saving:'.
- Root cause: src\Federator.Addin\Engine\DocumentGuard.cs:79-82 and :52-55.
- Class: noise
- Proof: New Core test beside tests\Federator.Core.Tests\Rerun\OpenDocumentJobTests.cs: the
  wording takes a name, no name, or a name that could not be read, and asserts the third case
  does not say unsaved. It fails once the wording moves to Core, so the test comes with that
  move.
- Note: The person is still warned, because the text is not null, and the harm is small. The
  swallowed catch also breaks the rule against a catch that swallows. Whether CurrentFileName or
  FileName can throw is UNKNOWN. Not reached in set 03: 'open document : none, the document is
  clear' at log:11.

### FR-170 penetration-size-catch-swallows-unit-throw

A catch in the penetration size reader swallows a unit or property throw and its comment gives
only the F72 reason, though for the viewpoint caller the same null files the clash in its pair
folder, and it logs nothing.

- Sources: T1-S12
- Evidence: src\Federator.Addin\Engine\Penetrations.cs:335 'return
  SizeRule.LargestMillimetres(read, unitEnumName, sizes)', then :338-347, a catch whose comment
  says 'A unit the table does not know throws out of SizeRule, and so does a property that will
  not read. Either way no size could be read, which the rule answers by LEAVING THE CLASH ALONE
  ...', and :349 'return null'. src\Federator.Core\Units\UnitTable.cs:198-202 ByEnumName throws
  NotSupportedException for an unknown name.
- Root cause: src\Federator.Addin\Engine\Penetrations.cs:338-347 (LargestOf), reached by F72
  through WantedFor and by F85 through ServiceSizeOf from
  src\Federator.Addin\Engine\ViewpointBuilder.cs:393, with the unit read at
  Penetrations.cs:405-408.
- Class: noise
- Proof: No Core test fails today, because the unknown unit branch cannot be reached on 2025: the
  unit is document.Units.ToString() at Penetrations.cs:407, the enum has eleven members
  (docs\history\scan.md:526-528) and UnitTable.cs:62-77 carries all eleven. The guard is a Core
  test in tests\Federator.Core.Tests\Units\UnitTableTests.cs asserting each of the eleven names
  resolves through UnitTable.ByEnumName, which passes today and fails when a row is dropped
  (UnitTableTests.cs asserts only UnitTable.All.Count above 0 now). For a swallowed property read
  throw at ItemSizes.cs:48 the proof is UNKNOWN.
- Note: PARTLY. Real code but not the harm the finding named: no service is filed as size unknown
  for the unit reason today, and what is left is a comment and a catch that logs nothing. A throw
  at ItemSizes.cs:48 on the leaf ends the whole LargestOf loop so the ancestors are never read,
  and whether PropertyCategories on a ClashResult.Item1 item ever throws is UNKNOWN. Set 03:
  'size could not be read : 0' in all 22 groups on the viewpoint path. Penetrations.cs is also
  FR-072's file.

### FR-171 t1-noise-findings-93

93 noise findings from the turn 1 read, each one reader's claim at 42499bf and none read again
since.

- Sources: T1-N1 to T1-N93 (steps\loop.md:700)
- Evidence: steps\loop-read.md:110 '### noise, 93', entries at :112-204, counted 93 by grep of the
  T1-N tags. steps\loop.md:864-867 says the second reading of 2026-09-29 covered only the 86
  silent, broken and loud faults.
- Root cause: UNKNOWN as one place. Each entry carries its own file and line at 42499bf, none
  checked on main.
- Class: noise
- Proof: None as a group. Each entry that is taken gets its own Core test or a grep.
- Note: A backlog kept as an item, not opened one by one. Some entries are already other items
  here, for example T1-N82 is the same shape as FR-042, the page check order warning.

### FR-172 t1-uncalled-members-150

150 public members nothing in src calls, as counted at 42499bf.

- Sources: T1-UNCALLED (steps\loop.md:701)
- Evidence: steps\loop-read.md:255 '## 3. Members nothing in src calls, 150', entries at
  :260-409, counted 150 lines. steps\loop.md:701 'each deleted with its tests, or kept by a
  decision in 02_questions'.
- Root cause: UNKNOWN as one place. Each entry has its file and line at 42499bf, not counted
  again today.
- Class: noise
- Proof: None as a group. For each member a grep over src reading 0 and the member deleted with
  its tests, or a decision in steps\02_questions.md that keeps it.
- Needed Bader, asked as Q26 on 2026-09-12 and answered on 2026-10-04, see the answered line of this item. Q26 asked whether the ones read only by a test go internal
  instead, which decides about 30 of them.
- Note: A backlog kept as an item. CLAUDE.md deletes a member nothing in src calls unless a
  decision keeps it. SizeTally and MatrixCorrections, which are in this list, are separate items
  here (FR-068 and FR-030). Other uncalled members are separate items too:
  SavedViewpoints.CanBuild (FR-133), FolderMemory.DisabledReason (FR-162) and
  OutputNameTable.ReleaseToPattern (FR-160).
- Answered by Bader on 2026-10-04 at 15:23, Q26, in short, his words being under the question in steps\02_questions.md: do not pick an answer. Write detailed test steps under this item, run them, and say where the mistake is, if there is one, before anything changes. Break each member on purpose and check its test fails. A test that still passes proves nothing, and that member goes in the form. The lead's note: in wave 5.

### FR-173 t1-catch-swallowing-77

77 catches a reader called swallowing, as counted at 42499bf, some of which are the rule that
logging never stops a run.

- Sources: T1-CATCH (steps\loop.md:702)
- Evidence: steps\loop-read.md:411 '## 4. Catches a reader called swallowing, 77', entries at
  :416-492, counted 77 lines. :413 'Logging never stops a run, so some of these are the rule and
  not a fault.'
- Root cause: UNKNOWN as one place. Each entry has its file and line at 42499bf, not read again
  on main.
- Class: noise
- Proof: None as a group. Each catch is sorted into the logging rule or a fault, and a fault gets
  its own Core test.
- Note: A backlog kept as an item. Some of the faults inside it are already other items here, for
  example SetBuilder.cs:284 and :320 are FR-013.

### FR-174 one-public-type-per-file

46 files under src hold more than one top level type, and the one public type per file rule that
Bader answered in Q80 is written in no rule file.

- Sources: D1 (steps\loop.md:437), A23 (steps\loop.md:553), Q98 B5
- Evidence: steps\loop-read.md:206 '## 2. D1, files holding more than one top level type, 46',
  listed at :208-253. The register reader's text count over src of namespace level type
  declarations also reads 46 files, for example Exchange\MatrixCorrections.cs and
  ExchangeModel.cs with 8 each, and Clash\PenetrationRule.cs, Views\ClashViewpointPlan.cs and
  Report\ClashReportModel.cs with 5. A grep for 'one public type' in .claude\rules and CLAUDE.md
  finds nothing. steps\04_audit_first_run.md:218, A23 names 13 files, and 11 of them still hold
  more than one type (Csv left PriorityMap.cs and CensusMove.cs holds one).
- Root cause: The 46 files in steps\loop-read.md:208-253, counted again on main. The rule text is
  missing from .claude\rules\core.md and addin.md.
- Class: noise
- Proof: A new Core test, tests\Federator.Core.Tests\SourceLayoutTests.cs, that reads
  src\**\*.cs and fails today on 46 files, and passes when every file holds one top level type.
  Plus the rule written into core.md and addin.md.
- Note: Answered in Q80 D1. Q98 B5 makes it the last pull request of the round, moves only and no
  logic. Splitting changes no behaviour, so the Core tests and the add-in build read the same
  counts before and after. It touches the files of every product area, so nothing else is open
  when it runs.

## Bader's five requests, FR-175 to FR-179

Added by Bader's message of the evening of 2026-10-04 headed FIVE REQUESTS ADDED TO THE ROUND,
in his words under Q112 of steps\02_questions.md. They are requests, not faults found, so they
sit outside the counts of the table below. After each one, three lines in the Claude tab: what
was done, what the test showed, and anything for Bader.

### FR-175 no-sleep-and-keep-awake

- Sources: Q112, request 1
- What he asked: sleep when plugged in set to Never with powercfg after its old value is saved,
  and put back at the close. The keep-awake running while this Claude Code session is open,
  STATE WAITING included, checked every 30 minutes and started again if gone. Every sleep and
  wake in the System log since 1 Oct
- Measured on 2026-10-04 at 19:27, turn5\power-before.txt: the Balanced scheme, sleep after,
  hibernate after and turn off display after each read 0, Never, when plugged in, and no key
  exists under HKLM\SOFTWARE\Policies\Microsoft\Power. Only Standby (S0 Low Power Idle) is available, and hibernation is not
  enabled. The lock screen's display timeout is hidden from powercfg, so it is UNKNOWN
- Done by the lead: nothing was written to the power settings, since the value asked for was
  already there, so nothing is put back at the close, read again unchanged at 20:04,
  turn5\power-after.txt. The System log since 2026-10-01 holds no
  sleep and no wake, only starts, a shutdown and an update restart,
  turn5\sleep-wake-since-1oct.txt. keep-awake.ps1 now watches the session's claude.exe, pid
  19148 by turn5\session-chain.txt, and stops only when no Claude Code claude.exe runs or
  steps\loop.md reads STATE CLOSED. A wrong session, STATE WAITING, a second copy and STATE CLOSED proved on a copy with its own mutex, 8 passed and 0 failed, turn5\keep-awake-test\prove-result.txt, and the takeover of another claude.exe when the watched one ends, 5 passed and 0 failed, the watched one a stand-in copy of PING.EXE named claude.exe, turn5\keep-awake-test\prove-takeover-result.txt. The stop when no Claude Code claude.exe runs at all is read in the code only, UNKNOWN by a run, since a run of it would end the session. turn5\check-keep-awake.ps1 starts it again
  when it is gone, proved by ending it at 19:36:35, turn5\keep-awake-checks.txt, and is
  scheduled at 13 and 43 minutes past each hour while the session is idle,
  turn5\keep-awake-schedule.txt
- Class: Bader's request
- Closed: 2026-10-04, by the lead, as above

### FR-176 coverage-of-the-clash-xml

- Sources: Q112, request 2. Area F127, the first item of wave 2
- What he asked: after each group, every test of the picked XML, or the saved tests when there
  is no XML, against what happened: created or not, run or not, its count in Clash Detective, its
  rows in the workbook. A reason for each test with no results. The categories in the models
  that no set catches, with each one's item count per model. Every set that found no items in
  any group of the run. A Coverage sheet and a COVERAGE block. RESULT counting the tests of the
  XML: created, run, with clashes and without. A count in Clash Detective that differs from the
  workbook rows a FAILED line, using the F104 check. Every wave test after it shows the sheet
- Measure first: what the log, the .tsv and the workbook already hold for each test, the test
  created and rows for the workbook rows, the SET ZERO lines, the CENSUS lines, and whether the
  categories each model carries with their counts are read today. UNKNOWN until read
- The lead's reading, Q112: the FAILED line sits in the COVERAGE block and RESULT and the group's
  own result is left as it is, since CLAUDE.md says a report check never fails a group, until
  Bader says the group should fail
- Proof: a Core test for each reason, each list and the FAILED line, each breaking one thing,
  then the test of wave 2 on 1A02MM and 1A04PK with the sheet read back and set beside F104's
  documents read of the same NWFs
- Class: Bader's request
- Measured on 2026-10-04 off set 03 and the code, turn5\measure-coverage.md. Partly there today:
  created per test, the test created rows of the .tsv, 3215 on C06, with no row for a test not
  created. Run per test, the test passed and test found clashes rows. The count in Clash Detective,
  the number column of those rows. The workbook rows, the rows for the workbook rows, which carry
  the document's count as text, 3024 on C06, every one equal. A side's set that finds nothing, in
  log lines with five examples per reason. The single discipline group. The sets empty across the
  run, of which the log names 10 of 14. Not there: the categories each model carries with their
  counts under the property the sets ask, a Coverage sheet, since the workbook check counts a
  second sheet as a fault, a COVERAGE block, RESULT counts of the tests of the XML, and a FAILED
  line. The add-in does not count top-level results, and the F104 comparison lives in PowerShell
  and a probe. Its rule can be built in Core with the document side read by the add-in, while the
  probe stays the independent witness, since compare-document.ps1 lines 12 to 17 say a check that
  shares code with the harvest proves nothing
- Seen while measuring: the BLOCKS false alarm on all 22 C06 groups is FR-035 and the SINGLE
  DISCIPLINE sentence is FR-126, both already on this list. A single discipline group's CLASH
  block counts its 36 created tests among its 1830 skipped, log:1167 to 1170 of set 03, and that
  is the block's own word, skipped meaning not run and not passed, split there into 36 for the
  one discipline and 1794 for a side that finds nothing. Not a fault, but the coverage counts
  keep created and run apart
- Answered by Bader on 2026-10-04 in the evening, the notes of Q112: right as read. A count that
  differs is a FAILED line in COVERAGE and RESULT, and the group keeps its own result

### FR-177 generic-models-counted-and-a-set-per-model

- Sources: Q112, request 3. Area F128, wave 2
- What he asked: first measure on the two buildings which property and value name Generic
  Models. Then a GENERIC block and a sheet per group, each model file with its count, models with
  none left out, and in the NWF a search set folder named Generic Models with one search set per
  model, its conditions on the category and the source file. No clash test for them
- Measure first: the property and value, on 1A02MM and 1A04PK, off the baseline's logs where
  they name categories, or by a probe on a baseline NWF through the guarded start. UNKNOWN until
  measured
- Proof: Core tests for the counts, the sheet and the set plan, then the test of wave 2 on both
  buildings, the sets read back off the NWF
- Class: Bader's request
- Measured on 2026-10-04 off the repo, turn5\measure-generic.md: no log, read-out or sample names
  Generic Models. One probe of 2026-09-20 counted 62 items with that category over all ten C02 NWFs,
  40 models, tools\probes\ViewpointProbe\5i-result-20260920.txt line 64, with no count per
  model or per building and items rather than elements. The tool reads a category from the
  property shown as Category, then Revit Category, then Element Category, and on a 1A02MM element
  that is the Element tab, LcRevitData_Element, LcRevitPropertyElementCategory. Whether Generic
  Models items carry it there is UNKNOWN, and nothing has read 1A04PK. No line the installed
  build writes can answer it, read off set 03's log and the code, so a probe on the baseline's NWFs of both buildings, through the guarded start,
  measures it before F128 is written

### FR-178 start-from-an-existing-nwf

- Sources: Q112, request 4. Area F129, wave 3
- What he asked: a choice beside the NWC folder to pick one NWF or a folder of NWFs, each run
  the way the weekly run runs it, the tests of the XML if one is picked else the tests saved
  inside, then the reports and the NWD, through the same engine route as the weekly run and the
  open document, every rule applying
- Measure first: the engine routes of the weekly run and the open document run today, so the
  new choice reuses one and copies neither
- Proof: Core tests for what the choice decides, then a run on copies of the NWFs set 04 makes
  for 1A02MM and 1A04PK, never on the baseline's own files
- Class: Bader's request

### FR-179 shift-range-tick-in-the-group-list

- Sources: Q112, request 5. Area F130, wave 3
- What he asked: a click on one Run box then a Shift click on another gives every row between
  them the first one's state. Why it fails today found first, then fixed, then tested through the
  window with the driver
- Root cause: in the measured line below
- Measure first: whether the driver can make a Shift click on the tool's own window without real
  input, since it never clicks, sends no key and never moves the pointer, the loop's own choice.
  If it cannot, the way to test it goes to Bader
- Class: Bader's request, a broken feature by his words
- Measured on 2026-10-04 off the window code, turn5\measure-shift.md: a range tick is not
  written. The Run column is a stock DataGridCheckBoxColumn bound to Include with no handler,
  FederatorWindow.xaml lines 184 to 191, and nothing under src reads Shift or the row selection.
  In a stock DataGrid Shift extends the row selection, read from PresentationFramework.dll's IL
  by reflection with no click made, and whether Navisworks loads that same file is UNKNOWN. Also every untick in the group list goes back to
  ticked when the groups are built again, after a scan, a grouping change or a change to a file's
  Use box, FederatorWindow.xaml.cs lines 247 to 318 and GroupRow.cs line 39
- Whether the driver can make a Shift click without real input is NOT MEASURED: the stand-in
  measurement waits for no Navisworks to run, turn5\measure-shift-driver.md
- Answered by Bader on 2026-10-04 in the evening, the notes of Q112: if the driver cannot test the
  Shift click without real input, the Shift test is written as numbered steps for him in
  steps\03_bader_next.md

## The areas at a glance

| Area | Items | Count | Silent wrong number | Broken feature | Slow | Loud failure | Noise | Needs Bader |
|---|---|---|---|---|---|---|---|---|
| Alignment | FR-001 to FR-007 | 7 | 5 | 0 | 0 | 1 | 1 | 1 |
| Sets | FR-008 to FR-030 | 23 | 19 | 1 | 0 | 0 | 3 | 3 |
| Clash counts | FR-031 to FR-034 | 4 | 3 | 0 | 0 | 1 | 0 | 0 |
| Workbook | FR-035 to FR-039 | 5 | 3 | 0 | 0 | 0 | 2 | 0 |
| Report | FR-040 to FR-042 | 3 | 1 | 1 | 0 | 0 | 1 | 0 |
| Run log and RESULT | FR-043 to FR-064 | 22 | 11 | 3 | 0 | 1 | 7 | 0 |
| Views | FR-065 to FR-074 | 10 | 2 | 2 | 3 | 1 | 2 | 1 |
| Harvest and pictures | FR-075 to FR-077 | 3 | 1 | 1 | 1 | 0 | 0 | 0 |
| Install | FR-078 to FR-081 | 4 | 0 | 3 | 0 | 0 | 1 | 0 |
| Loop tools | FR-082 to FR-124 | 43 | 3 | 9 | 3 | 8 | 20 | 2 |
| Docs and words | FR-125 to FR-149 | 25 | 0 | 0 | 0 | 1 | 24 | 2 |
| Rest | FR-150 to FR-174 | 25 | 9 | 5 | 0 | 3 | 8 | 3 |
| All | FR-001 to FR-174 | 174 | 57 | 25 | 7 | 16 | 69 | 12 |

Needed Bader, all answered on 2026-10-04: FR-006, FR-008, FR-009, FR-030, FR-070, FR-109, FR-110, FR-136, FR-149, FR-160,
FR-161 and FR-172.

Waiting for a merge not yet made: F107 for FR-110 and FR-149, and F109 for FR-078 to FR-081.
F105 merged as 2c89788, so FR-122, FR-123, FR-124 and FR-146 no longer wait. PR 83, merged as
086a348, closed FR-138, and with F106 FR-137, and the rule half of FR-139.

Resting on code merged on 2026-10-04: F106 (3449521) under FR-082, FR-083, FR-085, FR-092,
FR-094 to FR-098, FR-109, FR-111 to FR-116, FR-138, FR-139 and FR-143. F108 (c9b223b) under
FR-082, FR-084, FR-093, FR-101 to FR-104, FR-118, FR-139, FR-144 and FR-145. Lines that moved
with F106 and are given on main in the item: FR-079, FR-080, FR-086 to FR-091, FR-099, FR-105
to FR-108, FR-117, FR-118 and FR-121.

## The files each area changes

| Area | Files |
|---|---|
| Alignment | src\Federator.Core\Health\AlignmentCheck.cs, ExportCheck.cs, InvisibleDifference.cs, src\Federator.Core\Rerun\GroupJudgement.cs, src\Federator.Addin\Engine\FederationEngine.cs (2090 to 2152), ModelFactsReader.cs, and their tests under Health and Rerun |
| Sets | src\Federator.Addin\Engine\SetBuilder.cs, FederationEngine.cs (2389, 2456, 2626 to 2636), src\Federator.Core\Sets\ (SetDrift, SetLeftovers, SetBuildOutcome, SetsAcrossTheRun, EmptySets, SetResult, SetBuildPlan), src\Federator.Core\Health\ (SetWarnings, HealthCheckResult, HealthCheck, ExportCheck, WorksetDisagreements), src\Federator.Core\Exchange\ (RevitWorksets, MatrixCorrections), exchange\1104-PAR_CLASH_AllInOne_25mm_FIXED.xml only by Bader's choice, and the tests under Sets, Health and Exchange |
| Clash counts | src\Federator.Addin\Engine\ClashRunner.cs (572 to 579), ClashHarvest.cs (164), ByDesign.cs, src\Federator.Core\Clash\ (ClashRunOutcome, ByDesignRule, ByDesignTally, AutoReviewRecord), src\Federator.Core\Report\ClashReportModel.cs, and the tests under Clash and Report |
| Workbook | src\Federator.Core\Report\ (WorkbookCheck, WorkbookWriter, ClientStyle), src\Federator.Core\Clash\ (CreationPlan, ToleranceChoice, PriorityMap), src\Federator.Addin\Engine\ClashRunner.cs (465 to 469), FederationEngine.cs (243, 2779 to 2784), and the tests under Report and Clash |
| Report | src\Federator.Core\Report\ (ClashReportModel, ClashReportXml, ClientShapes, WorkbookCheck, PageCheck), src\Federator.Addin\Engine\ClashHarvest.cs (244 to 335), FederationEngine.cs (2678, 2772, 2929), the tests under Report, and a probe for the grid |
| Run log and RESULT | src\Federator.Core\Diagnostics\ (RunLog, RowLog, EventRow, RunClock, LiveLine), src\Federator.Core\Rerun\GroupJudgement.cs, src\Federator.Core\Clash\ (ClashRunOutcome, ToleranceChoice, ClashesAcrossTheRun), src\Federator.Core\Exchange\RevitCategories.cs, src\Federator.Core\Health\HealthCheckResult.cs, src\Federator.Addin\Engine\ (FederationEngine, ClashRunner, DocumentUnits), src\Federator.Addin\Ui\FederatorWindow.xaml.cs, and the tests under Diagnostics, Rerun, Clash, Health and Units |
| Views | src\Federator.Addin\Engine\ (ViewpointBuilder, SavedViewpoints, Penetrations, ItemSizes, FederationEngine 1022 to 1031 and 1721 to 1840), src\Federator.Core\Views\ (SizeText, SizeTally, ClashViewpointPlan), src\Federator.Core\Diagnostics\TimingBlock.cs, and the tests under Views and Diagnostics |
| Harvest and pictures | src\Federator.Addin\Engine\ (ClashImages, ClashHarvest 382 to 393, FederationEngine 2589, 2706 and 2743 to 2761), src\Federator.Core\Report\ReportOrder.cs, and the tests under Report and Clash |
| Install | build\install.ps1, INSTALL.md, tools\loop\run.ps1 (RunChild, BundleLeftovers, InstallVerdict, the Install flow, CheckMode), tools\loop\prove-run.ps1, tools\loop\README.md and .claude\rules\loop.md for words |
| Loop tools | tools\loop\run.ps1, nw-guard.ps1, prepare-copy.ps1, prove-run.ps1, read-workbook.ps1, mask-evidence.ps1, prove-hooks.sh, tools\probes\drive-window-run.ps1, probe-automation-start.ps1, the F105 probes and il-reader.ps1, tools\checks\check-evidence-ids.sh and evidence-ids.txt, and .claude\hooks\refuse-git-on-main.sh through a proved copy |
| Docs and words | steps\runs\03\for-modellers.md (copied in by a command), steps\03_bader_next.md, steps\04_audit.md, steps\loop.md and steps\02_questions.md (the lead's), .claude\rules\loop.md, core.md and addin.md, CLAUDE.md, .claude\agents\runner.md, docs\history\scan.md, INSTALL.md, tools\loop\README.md, the headers and comments of tools\loop\prove-run.ps1, run.ps1 and prepare-copy.ps1 and of tools\probes\probe-automation-start.ps1 and drive-window-run.ps1, a new tools\checks\check-restated-facts.sh, and in src ScanFindings.cs, BuildingGroup.cs, GroupRow.cs, ToleranceChoice.cs, RepeatedFailureGuard.cs, ClashRunner.cs, TestDrift.cs, FederatorWindow.xaml, OutputNameTable.cs, ReportPaths.cs, SetRebuildSettings.cs, SavedViewpoints.cs, FederationEngine.cs (915 and 3179), DocumentUnits.cs and ReportOptions.cs |
| Rest | src\Federator.Addin\Ui\ (FederatorWindow.xaml.cs, FederatorWindow.xaml, GroupRow.cs), src\Federator.Core\Naming\ (OutputNameTable, NamePattern), src\Federator.Core\Rerun\ (OutputPaths, RebuildTally), src\Federator.Core\Diagnostics\FolderMemory.cs, src\Federator.Core\Clash\ (UndoAutoReview, ToleranceChoice, a ClashWork rule), src\Federator.Addin\Engine\ (ClashStatusEditor, UndoAutoReviewed, ClashRunner 1102 to 1108, FederationEngine 1396 to 1398 and 2725, PropertyProbe, DocumentCensusReader, DocumentGuard, Penetrations 338 to 347), src\Federator.Core\Probe\ (ProbeCsv, ProbeTally, ProbeVerdict), src\Federator.Core\Exchange\ExchangeReader.cs, src\Federator.Core\Findings\ (ScanFindings, SourceMismatchFindings), src\Federator.Core\Grouping\BuildingGrouping.cs, and for FR-171 to FR-174 any file in src |

## Areas that can be worked at the same time

Files two areas share, beyond FederationEngine.cs, which every product area but clash counts
changes in its own methods:
- alignment and sets: ExportCheck.cs (FR-003 to FR-005 and FR-028)
- alignment and run log: GroupJudgement.cs (FR-001 and FR-043 to FR-045)
- sets and run log: HealthCheckResult.cs (FR-023 and FR-064)
- sets and docs: SetRebuildSettings.cs (FR-132)
- clash counts with workbook, run log, docs and rest: ClashRunner.cs, and with run log also
  ClashRunOutcome.cs (FR-031 and FR-052)
- clash counts with report and harvest: ClashHarvest.cs, and with report ClashReportModel.cs
- clash counts and rest: AutoReviewRecord.cs (FR-034 and FR-152)
- workbook and report: WorkbookCheck.cs (FR-035 and FR-040)
- workbook, run log, docs and rest: ToleranceChoice.cs (FR-036, FR-053, FR-063, FR-127, FR-166)
- report and harvest: ClashHarvest.cs (FR-041 and FR-076)
- run log and rest: FederatorWindow.xaml.cs (FR-049, FR-050 and FR-153, FR-159 to FR-166)
- run log and docs: DocumentUnits.cs (FR-062 and FR-134)
- views and rest: Penetrations.cs (FR-072 and FR-170)
- views and docs: SavedViewpoints.cs (FR-133)
- docs and rest: ScanFindings.cs, GroupRow.cs and OutputNameTable.cs (FR-126, FR-130 with
  FR-153, FR-154, FR-159, FR-160, FR-165)
- install and loop tools: tools\loop\run.ps1 (RunChild, BundleLeftovers, CheckMode) and
  tools\loop\prove-run.ps1
- docs with loop tools and install: tools\loop\README.md, the prove-run.ps1 header, the
  drive-window-run.ps1 header, the prepare-copy.ps1 comment and .claude\rules\loop.md

What that leaves:
- Lane one, the loop's tools. Loop tools and install touch no file under src or tests, so they
  run beside every product area. Inside the lane they share run.ps1 and prove-run.ps1, so their
  pull requests go one after another. FR-092, the harness, goes first because the others are
  proved in it. Install goes with F109, and FR-122 to FR-124 can go since F105 merged. The docs items that
  change only tool headers and the loop README, FR-140 to FR-145, ride in this lane.
- Lane two, the product. Clash counts shares no file at all with alignment, sets or views, so
  those can run at once. Alignment, sets, views and harvest and pictures share only
  FederationEngine.cs, in different methods, so they can run at once if each pull request takes
  main in before it merges. Clash counts, workbook, report, run log and rest share ClashRunner.cs,
  ClashHarvest.cs, ClashReportModel.cs, ClashRunOutcome.cs, WorkbookCheck.cs, ToleranceChoice.cs
  and FederatorWindow.xaml.cs, so they go one after another by file.
- Lane three, words in steps and rules. FR-125, FR-135 to FR-139 and FR-146 to FR-149 touch no
  code, so they run beside both lanes, what is left of FR-139 among them. The docs
  items that
  change src, FR-126 to FR-134, go with the area whose file they change.

Order inside the round:
- FR-001 before FR-043 to FR-045, and FR-002 with FR-001 (GroupJudgement.cs and
  AlignmentCheck.cs).
- FR-003 to FR-005 and FR-028 in one pull request or in order (ExportCheck.cs).
- FR-015 before FR-016, with FR-023 and FR-024 sharing its flag rule.
- FR-010 and FR-011 before or with FR-027, whose judgement they set.
- FR-151 before FR-164.
- FR-073's split of the seconds before FR-069's speed work, and FR-069 before FR-070's options
  are measured.
- FR-092 first in the loop tools lane.
- FR-159 with FR-165, and FR-160 after the find Bader's answer to Q24 asks.
- FR-174 last, alone, by Q98 B5.

## Source ids left out

From the findings of set 03:
- C06-J18, FIND-15, C06-DONE-12: steps over twice the same step on the group before. Every pair
  follows the clash and viewpoint count of the later group, and the SLOWER text is a window line
  only, so no log line could show it. The real fault in it, the VIEWS cost per viewpoint, is
  FR-069, and the SLOWER compare is FR-056.
- C06-J24, FIND-21, C06-DONE-18: the 22 GAP blocks and log:8218. Information the tool is built to
  give, the log says 'Nothing here is a fault' (log:495), and Q35 to Q40 wait on the counts.
- C06-J25, FIND-17, C06-DONE-19: 22 CENSUS NOTED lines and no CHANGED. By design, F73, nothing
  moved that should not.
- C06-J26, FIND-20, C06-DONE-17: one confirm dialog answered OK (driver.txt:21) and 20 Navisworks
  progress windows (watch.txt:19 to 114), one with an empty caption (watch.txt:94). No error
  dialog, and nothing needed a person.
- C06-J27, C06-DONE-6: 27 SOURCE MISMATCH entries and 4 SHARED SOURCE pairs (log:8223 to 8339).
  The tool reports and Bader decides, nothing is merged or unticked (core.md). The pair 1B06M1
  and 1C06M2 is named in FR-125.
- C06-J30, FIND-16, FIND-18, FIND-19, C06-DONE-21: checked and clean. 290 steps started and 290
  finished, no exception, no step threw, the RESULT counts agree with the lists under them except
  the two size lines (FR-046), and the 22 workbook read-outs agree with the log test by test and
  status by status.
- FIND-07: no document read in the evidence folder, so workbook against document is UNKNOWN. An
  evidence gap and not a fault of the product. The panel side is Bader's three tests
  (findings.md, Q97), the tool's own ROWS lines agree 3024 of 3024 (log:8577), and a tool that
  reads the saved NWFs would be a new loop feature that Q93 puts under register rows.
- C06-DONE-16: the document reads Feet in 16 groups and Meters in 6 after every model was set to
  Meters. The report is converted whatever the document says (core.md, Q23), and the smallest
  distance in any read-out is 0.034 m against a 25 mm tolerance (register row F26).
- READ ME, a register row: not a finding.
- The section of findings.md on Bader's three tests of done and the three panel tests (4 at
  100000, 11 at 1B06PH, 636 at 1B06PK): the panel counts are Bader's to read, and nothing was
  read in this run.
- Register rows PROVED WORKING by set 03: RUN-1637, F52, F73, F78, F80, F82, F11, F26, F27, F33,
  F34, F35, F39, F58, F59, F60, F61, F63, F64, F65, F91, LOOK 11, LOOK 27, LOOK 61, LOOK 70, LOOK
  80, LOOK 92, LOOK 97 98 99, LOOK 107 108 110 111, LOOK 118 120, LOOK 156 157, LOOK 159 163 164
  167, LOOK 215, LOOK 217, LOOK 227, LOOK 235 236 239, LOOK 248, LOOK 254 255 256 257, LOOK 269b,
  LOOK 270, LOOK 289 to 292, LOOK 296 to 301, LOOK 302 to 307, LOOK 308 309, LOOK 311 to 317, LOOK
  325 326 327 329, LOOK 331 332 334 335, and LOOK 388 tail.
- Register F, A, B, L and P rows NOT REACHED by a first run, so set 03 shows nothing about them:
  F50, F88, F87, F72a, F84, F79, F47a, F47b, F47c, F5, F6, F7, F8, F22, F1, F2, F4, F9, F10, F17,
  F16, F20, F24, F28, F29, F30, F31, F32, F36, F37, F38, F46, F40, F41, F42, F43, F44, F45, F51,
  F53, F54, F55, F56, F57, F62, F66, F67, F68, F71, F90, F94, A10 to A14, A17 to A22, B13, L2,
  B14, B2 (the register row, not Q98 B2), B11, L1, L3, L4, B7 and P3. F84, LOOK 37 and LOOK 388
  HEALTH were NOT SHOWN because the driver typed the XML path (driver.txt:12) and the HEALTH block
  is written only from the Browse button (src\Federator.Addin\Ui\FederatorWindow.xaml.cs:2129-2131,
  Describe at :2141-2166). A person who browses gets it, so it is not a fault of the run.
- Register step rows NOT REACHED: steps 20-21 (F68), 29-34, 35-37 and the HEALTH half, 39-48
  (F27), 49-71 (F24, F29, F50), 62 and 185 (LOOK 62 itself is on FR-027), 75, 81-82 and 86-87
  (F31), 88-96 and 100-101 (F35), 102-115 (F26), 123-136 (F71), 137-146 (F9), 147-155 (F10),
  160-167 (F17), 168-174 (F8), 175-178 (F32), 179-191, 192-198 (F7), 209-212, 213-214 (F41), 219
  (F45), 228-233, 244-245 and 250-252 (F53), 260-262, 269 and 269a, 271-274 and 277-278 (F72),
  279-280, 286-287 and 318-324 (F62), 310, 340-343, 346-352 and 364. Three rows the findings
  reader listed as not reached are on items from the register reader and are not left out: the
  RESULT counts across Run presses row (FR-049), the page test order row (FR-042) and step 247
  (FR-068).
- Register LOOK rows NOT SHOWN, not reached so no fault shown: LOOK 10, 13, 15, 17, 19, 21, 23,
  28, 30, 31, 33, 34, 36, 37, 40, 46, 47, 51, 52, 54, 56, 58, 59, 60, 63, 64, 66, 67, 69, 75, 78,
  79, 83 to 87, 94, 106, 112 to 114, 122, 127 to 129, 131, 133, 135, 140, 142, 143, 146, 151 to
  153, 162, 166, 170, 172 to 174, 177, 182, 185, 187, 188, 190, 191, 193, 195 to 198, 210, 212,
  214, 221, 223, 224, 363, 216, 218, 219, 229, 231, 233, 241, 243 to 245, 251, 252, 259, 261, 262,
  264 to 269, 269a, 269d, 272, 274, 276, 278, 280, 286, 287, 310, 318 to 324, 333, 337, 339 to
  350, 380, 383 to 385, 391, 394, 395, 397, 398 and 388 HEALTH.
- The look_for rows of turn4\c06-find.json and turn4\c06-done.json: the CONFIRMED rows are not
  faults and the NOT SHOWN rows were not reached. The CONTRADICTED ones are on items: step 62 on
  FR-027, step 377 on FR-046 and FR-136, step 387 part 2 on FR-069, and step 269c on FR-035.
- The NEW-LOG rows that INSTALL.md steps 5 and 6 never ran (steps\loop.md:596) and what the
  penetration pass costs with the box off against on (steps\loop.md:609): done in code and not
  proved by a run, a proof and a measurement owed on the local machine and not faults. Set 05
  gives the second only if it runs the box both ways.
- The NEW-LOG row that the proof steps of 03_bader_next never worked as a whole
  (steps\loop.md:613): not a fault by itself, its fault part is FR-135.

From the register:
- T3-B: already done on main. TreeRefusal in tools\loop\run.ps1 (:754-765 at f38edd5, :1244 on
  main) refuses a HEAD that is not the stamp asked for and any line git status prints, untracked
  files included, steps\01_next.md:974 says T3-B is kept by Install, and the in place install of
  e4484d15 on 2026-10-01 read its stamp back equal (steps\loop.md:44-46).
  Directory.Build.targets:34-45 still writes +edits by design. Its register status still reads
  open, for F103, and is the lead's to close.
- F107-R2: a check at a merge, not a fault. F106 rewrote the header of
  tools\probes\drive-window-run.ps1 whose line 21 F107 masks, so the merge must keep the masked
  words, and the lead greps for the name after it. F106 merged first, as 3449521, so the check
  now falls at F107's merge.
- F104, F106, F107 and F109 as register rows: work in flight of plan step 2, not faults of this
  round. F106 has since merged, and F104, F107 and F109 have not.
- The known limits F102-L1, F102-L2 and Q88-IDS: limits Bader accepted (Q91 B, Q88), not faults
  to fix.
- CHAT-19, the DONE rows, the rows done in code and not yet proved by a run, and the rows that
  wait for Bader in the form: outside the register reader's source and not faults of this round.

From the readings of the fixes in flight:
- F106-HARM-REV-1: a clean statement of what the reviewer read whole.
- F106-HARM-REV-11: measured by set 03. Navisworks rotated out his autosaves of the same document
  names (record.txt:877 on) and the Q86 put back restored 78 files (steps\loop.md:80-81). What
  is left is a request to Bader not to open C06 in his own Navisworks during a run, no code
  fault.
- F106-HARM-REV-12: UNKNOWN until a run with Bader typing at the machine. The driver holds no
  key, click or pointer call, so nothing changes without a measurement.
- F106-HARM-REV-14: items 2 to 5 are wired and not yet proved, and set 05 runs them.
- F106-HARM-REV-16: UNKNOWN build counts of the stand-in, and the real run does not use the
  stand-in.
- F106-HARM-BRK-8: the breaker itself calls it harmless, the read back at
  drive-window-run.ps1:335-336 gates Run.
- F106-HARM-BRK-14, F106-HARM-BRK-15, F106-HARM-BRK-16 and F106-HARM-BRK-17: clean statements,
  turned over and nothing found.
- F108-REV-7: a missing Known bugs heading in the developer's draft log entry, never committed.
  The lead's steps\log.md entry for F108 carries the heading.
- F108-REV-8: the reviewer's own NOT A FAULT row.
- F108-BRK-9: a coverage note.
- F105-REV-2 and F105-BRK-1: blocking findings of F105, fixed and merged with it as 2c89788
  (docs\history\scan.md:5025-5030 and tools\probes\README.md:136-152 say probe-viewpoint-calls.ps1
  uses neither LoaderLines nor the string patterns).
- F105-REV-3 and F105-BRK-6: fixed and merged with F105 as 2c89788, its order line 35 and its
  steps\log.md entry the lead's.
- F105-REV-4: already on main, steps\loop.md:402 and 433-435 hold the F105 row and F105-R1 to
  F105-R3.
- F105-REV-7 and F105-BRK-8: checks that found nothing. F105-BRK-8 notes that scan.md 5z-f cites
  branch commits such as 30ae471 that a squash merge drops, and that the blob id is the pointer
  that resolves.
- F105-REV-8 and F105-BRK-7: what the readers could not check, not faults.
- F105-BRK-3: its six places fixed and merged with F105 as 2c89788 (docs\history\scan.md:5035-5039, 5330-5333 and
  5438-5439, tools\probes\README.md:111-113 and 144-152, steps\01_next.md:984).
- F105-BRK-4: fixed and merged with F105 as 2c89788 (docs\history\scan.md:5348-5359 names the nine assemblies not
  read and says whether they hold the names is UNKNOWN).
- F107-REV-1 and F107-BRK-1: blocking findings of F107, fixed on fix-F107 at 2063c29 (wt-f107
  steps\01_next.md:985 says every line number as read at 821ed6e and finds each line by its
  heading).
- F107-REV-2 and F107-BRK-4: merge conflict notes for the lead's merge order at the end of
  steps\01_next.md.
- F107-REV-3: fixed on fix-F107, order line 33.
- F107-REV-4 and F107-BRK-5: already on main, steps\02_questions.md:466 carries Bader's answer to
  Q88 and steps\loop.md:426 and 428 hold the rows.
- F107-REV-5: the steps\log.md entry is the lead's at the merge.
- F107-REV-6, F107-REV-7, F107-REV-8, F107-REV-9, F107-REV-10, F107-REV-11 and F107-REV-12:
  checks that found nothing.
- F107-BRK-3: the conflict with F106's driver header is in the lead's plan (steps\log.md:28-30),
  and F106's driver already says the machine of 2026-09-19 (drive-window-run.ps1:69 on fix-F106,
  now on main).

Placed and not left out, for the record:
- The bug readers left nothing out. T1-B7 sits on FR-160 and T1-L3 on FR-159.
- S03-19, C06-J29 and FIND-23 sit on FR-137, and C06-J13 on FR-125, as notes that the part they
  name is not a fault. FIND-22, C06-J23 and C06-DONE-20 sit on FR-074.
- T1-S25 sits on FR-031 and FR-052, and T1-S38 on FR-046 and FR-061, both placements right.
- T3-G6 to T3-G12 and T3-P, which steps\notes\f103-final-reading.md names as already registered,
  sit on FR-089, FR-090, FR-086, FR-088, FR-091, FR-078, FR-105 and FR-121.
- F105-REV-1 sits on FR-146 for its part on main, its branch part fixed in F105, merged as 2c89788, and
  F105-REV-6 on FR-122 as the same code fault as F105-R1.
- F108-R1 to F108-R5 and F107-R1, the rows the register reader could not see, sit on FR-093,
  FR-102, FR-103, FR-104, FR-118, FR-139, FR-144, FR-145, FR-084, FR-101 and FR-110.
