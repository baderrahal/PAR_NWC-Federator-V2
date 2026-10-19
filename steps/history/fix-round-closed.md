# Fix round, the closed items

Moved whole and in their order out of steps\fix-round.md on 19 Oct 2026, F242, Bader's round of 19 Oct b, item 4. Nothing was deleted.

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
  discipline group, then the workbook tolerance cell of every block of a test the document
  holds reads the chosen value and those blocks count under Tool. Since F77 a test whose side
  finds nothing is never created, so its block keeps the XML's value and counts under the clash
  XML, the reviewer's reading of F118's add-in pass, attempt 1.
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
- Confirmed again on 2026-10-05 by F132's Core half: its test over every skip reason failed on
  NoTolerance alone, turn5\f132-finding-notolerance.txt, against a stub run that cannot be repeated
  from what is kept. F132 took that test out of its tree rather than fix this item, which stays
  F119's in wave 3b, and F132's branch adds Mirror to the same list, so the two meet at
  ClashRunOutcome.cs lines 479 to 492

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
- Changed by Bader on 2026-10-04 in the evening, Q114: one view per clash test of its open
  clashes, in folders by priority and team pair, replaces the viewpoint per clash, so what is made
  faster is that design, FR-184 to FR-188. The 94.5 percent of 2 h 12 min on 1A02MM in the
  baseline, steps\runs\04\item1-C02, is what it is measured against, and FR-187 replaces the Proof
  above, which held the viewpoints the same

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
- Designed on 2026-10-05 by three plans, evidence first, the rules in Core and the cost, each its
  own agent, turn5\f127-plans.txt, and a judge, turn5\f127-design.md, evidence the base. Seven
  probes on Navisworks before any add-in code, and an eighth only if P2 fails, section 5, on copies
  once no Navisworks runs, the first F104's documents read of set 04's 1A02MM NWF as the
  independent witness. The Core steps 1 to 3 of section 6 touch no file F116 changes and start now,
  step 4 after F116 merges and P2 to P5 are read, the add-in after P1 to P7. The measured parts add
  about 0.4 to 1.1 s on a three-model group and 0.5 to 1.8 s on a ten-model group, with no FAILED
  line, and the rest is UNKNOWN until P6 and the first run. read-workbook.ps1 changes in the same pull
  request, or every F104 verdict reads NOT PROVED. Questions for Bader: Q126, a group whose clash
  is skipped has no workbook for the sheet, and Q127, how RESULT counts the XML's tests. Of the
  design's other six, his words settle the sheet's place, a sheet named Coverage in the workbook,
  the second and last sheet being the lead's reading of them. The rest are the lead's readings,
  each built as the design's default A: a test whose NWF holds earlier results reads FAILED,
  extending his words that a count that differs is a FAILED line, caught is read per item,
  extending his words on categories no set catches, the run's list of empty sets goes into the
  log and the .tsv, the discipline is read by code until F131, and with no XML each model's
  categories are listed with their counts and whether a set catches them is UNKNOWN and said.
  Reading the saved sets' conditions off the document instead waits for a probe not yet on the
  list
- Found while designing: set 03's C06 run did not pick the exchange file's bytes, since its SET
  lines ask ME-PIPING, ME-DUCTWORK and ME-EQUIPMENT where the exchange file asks ME-Piping,
  turn5\f127-design.md section 0

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
- Measured on 2026-10-05 at 11:45 on a stand-in window of the lead's own, not the tool's window and
  not the driver, turn5\measure-shift-driver-result.md: none of the three ways open to the driver,
  which never sends real input, carried Shift. UI Automation toggled a box with no Shift, posted
  mouse messages raised no click, and a posted Shift key did not reach the keyboard state WPF
  reads. Whether Navisworks hosts WPF the same way is not measured. So by Bader's note on Q112 its test is numbered steps in steps\03_bader_next.md when F130 is
  built
- Measured on 2026-10-04 off the window code, turn5\measure-shift.md: a range tick is not
  written. The Run column is a stock DataGridCheckBoxColumn bound to Include with no handler,
  FederatorWindow.xaml lines 184 to 191, and nothing under src reads Shift or the row selection.
  In a stock DataGrid Shift extends the row selection, read from PresentationFramework.dll's IL
  by reflection with no click made, and whether Navisworks loads that same file is UNKNOWN. Also every untick in the group list goes back to
  ticked when the groups are built again, after a scan, a grouping change or a change to a file's
  Use box, FederatorWindow.xaml.cs lines 247 to 318 and GroupRow.cs line 39
- Answered by Bader on 2026-10-04 in the evening, the notes of Q112: if the driver cannot test the
  Shift click without real input, the Shift test is written as numbered steps for him in
  steps\03_bader_next.md

### FR-189 nwd-listed-as-written-when-its-publish-failed

- Sources: F112's closing pass, turn5\f112d-dev-return.json new_items, read off main 1ae6771.
  Area F119, wave 3b
- Evidence: FederationEngine.WriteNwd logs the publish returned false, or calls log.Failure on a
  throw, then calls log.WriteFinished("NWD", job.NwdPath) whatever the publish gave, line 3357 on
  main 1ae6771, since a6957a6 of 2026-08-27. RunLog.WriteFinished records any file found at the
  path as written, with its size, so on a rerun last week's NWD is listed as written by this run
- Root cause: the call to WriteFinished does not read the publish result
- Class: silent wrong number, the files written list, against the rule that only a file this run
  wrote goes in it. Once F112 merges, the group's RESULT reason names such an NWD as not from this
  run, so only the list stays wrong. FR-044 and FR-045 are the same root for the NWF. Found after
  the list was written, so outside the counts of the table below
- Proof: a Core member of RunLog that takes the publish result, with a test that breaks it, then
  a run where the publish is made to fail if one can be made safely

### FR-180 team-map-beside-the-picked-xml

- Sources: Q114, points 1 and 2. Area F131, wave 2b
- What he decided: Mechanical is HV, PL, FP, ME, DR and FF, Electrical is EL and EV, Architecture
  is AR, Structure is ST, and any other code is a team of its own named by its code. The map is a
  plain list kept with the corrections file beside the picked XML, read when the XML is picked and
  named in the log, so src names no project, and it applies wherever the tool reads a discipline,
  from a set name or a file name
- Measure first: where the tool reads a discipline today and how codes are mapped,
  turn5\measure-views.md, not yet written. The discipline code is part 5 of the file name split
  on the separator, read by ContainerName.Parse, turn5\measure-teams.md section 1
- Proof: Core tests for the map, a missing map, a code on no team, and each place a discipline is
  read
- Class: Bader's decision
- Designed on 2026-10-05 by three independent plans and a judge, %LOCALAPPDATA%\NwcFederatorLoop\turn5\q114-design.md sections 1.1 and 1.2. Questions for Bader: Q115, Q116 and Q123

### FR-182 mirrored-tests-kept-once

- Sources: Q114, points 4 to 6 and 8. Area F132, wave 2a
- What he decided: a test whose two sides are another test's two sets swapped is a mirror, a rule
  of the code for any project. One test of each pair is kept, the higher priority, A before B
  before C, and where equal the one first in the XML. The other is not created and not run, and
  the coverage sheet names it as a mirror of the one kept. Different priorities or tolerances in a
  pair are named in the log. The pairs are listed in the COVERAGE block. The workbook, the viewpoints
  and every count then hold each clash once
- Measured on 2026-10-04, read only, turn5\measure-mirrors.md: the picked XML, sha256 792b01fb,
  holds no mirrored pair. Its 1830 tests are every unordered pair of its 61 sets exactly once, 61
  times 60 over 2, read by locator, by set name and by test name. So on this matrix the rule drops
  nothing. The nearest thing: BLD-EL-Telecom Fixtures and BLD-EL-Telephone Devices carry the same
  rule list, both Category equals Telephone Devices, so 59 test pairs are alike by their rules
  though not by name, and one test has two sides alike by their rules. His rule as written does
  not catch them, and on 1A02MM none of them ran since both sets found 0 items
- Proof: Core tests for the pairing, the choice of the one kept, the names in the log and the
  pairs listed in the COVERAGE block
- Class: Bader's decision
- Designed on 2026-10-05, turn5\q114-design.md section 1.4. Probe P1 measures that a test and its swap find the same clashes. Question for Bader: Q121
- Measured on 2026-10-05 by probe P1, scan.md 5z-k on the branch fix-F114-probes: the swap of
  BLD-ST-Framing-vs-BLD-ST-Columns, run beside it on a copy of the C02 NWF, found 27 clashes where the
  original found 25, all 25 among the 27. So a mirror can find what the kept test does not. Q133 asks
  Bader, the build going on with A, his rule as written with the risk said in the log

### FR-183 mirrored-tests-in-an-existing-nwf

- Sources: Q114, point 7. Area F132, wave 2a
- What he decided: in an NWF that already holds both, the mirror is not run. A mirror this tool
  created whose results carry no status a person set is removed from the NWF. One that carries a
  person's status is left, not run, and named in the form for Bader
- Measure first: how a test this tool created is told from one a person made, and how a status a
  person set is told from one the tool set, turn5\measure-views.md and the code of the status rules
- Class: Bader's decision, the one place the tool removes a test, by his word
- Designed on 2026-10-05, turn5\q114-design.md section 1.5: a mirror in an NWF is removed only where it matches a test of the picked XML exactly, never with no XML. Probes P2 and P3, P3 a step for Bader. Question for Bader: Q122

### FR-184 views-tree-by-priority-and-team-pair

- Sources: Q114, points 9 to 12. Area F114, wave 2b
- What he decided: priority folder A, B, C or No priority, then one folder per team pair, then the
  views of that pair's clash tests, each named by its test. A pair folder holds only tests whose
  two sides belong to those two teams, so the mechanical team never sees an electrical clash in
  its folders nor the electrical team a mechanical one, and Mechanical vs Electrical is its own
  folder because those clashes belong to both. Two codes of one team go in that team against
  itself. Over
  150mm is a folder inside each pair folder that has Mechanical or Electrical, never shared by two
  pairs. A pair is always written the same way round, Architecture, Structure, Mechanical,
  Electrical, then the rest
- Class: Bader's decision
- Designed on 2026-10-05, turn5\q114-design.md sections 1.6 and 1.7. The pair's order is point 12's, so point 10's Mechanical vs Structure is written Structure vs Mechanical. With no priority file the views go under No priority, point 9. Question for Bader: Q117
- Since Bader's answer B to Q131: F114's pull request sets ViewpointRequest.DefaultMakeViewpoints back to
  true, so the viewpoints box opens ticked once the new views are in, F136

### FR-185 one-view-per-test-of-its-open-clashes

- Sources: Q114, points 13 to 15. Area F114, wave 2b
- What he decided: a view shows a test's clashes still New or Active all at once, the camera framed
  around them, only the models of the test's two sides shown and every other model hidden, the
  clashing items red and green as now, the rest of the shown models dimmed. Reviewed, Approved and
  Resolved are left out. In a pair with Mechanical or Electrical the open clashes split by service
  size, over 150 mm in its view under Over 150mm and the rest in its view in the pair folder, a
  rectangular service sized by its larger side, the larger of two services deciding. No clash in
  two views. A test with no open clashes gets no view
- Class: Bader's decision
- Designed on 2026-10-05, turn5\q114-design.md sections 1.7 and 1.8. Probes P5, P16, P17 and P19. P5 measured on 2026-10-05 off the install: BoundingBox3D has a public constructor taking two Point3D, turn5\q114-probes\p5-p7.md. Questions for Bader: Q118 and Q119

### FR-186 views-made-fresh-only-the-tools-own

- Sources: Q114, points 16 and 17. Area F114, wave 2b
- What he decided: the views are made fresh every run. Only viewpoints this tool made are removed
  or replaced, the per-clash viewpoints of earlier runs included. A viewpoint a person made is
  never touched, wherever it sits. Clash Detective keeps every clash with its own view and the
  workbook every clash with its picture, each clash once
- It changes three rules of .claude\rules\addin.md, ONE SAVED VIEWPOINT PER CLASH, SINCE F85 A
  VIEWPOINT IS PER CLASH, and that a viewpoint already at its path is left exactly as it is, and
  they change with the fix
- Measure first: how the tool tells a viewpoint it made from one a person made, today and after,
  turn5\measure-views.md, not yet written
- Class: Bader's decision
- Designed on 2026-10-05, turn5\q114-design.md section 1.9: a mark on each view the tool makes, the new views written before the old are removed, the lead's choice since a run that stops part way then never leaves an NWF with neither. Probes P6, P7 and P8 to P15, P8 a read only dump of the baseline's 2847 viewpoints. P6 and P7 measured on 2026-10-05 off the install: the COM view has Comments() returning a comments collection with Add, and the redlines list has a Size() method, turn5\q114-probes\p5-p7.md. If P9 shows a comment does not survive a save, a question follows. Question for Bader: Q120

### FR-187 views-proof-on-1a02mm-and-the-views-tree-block

- Sources: Q114, points 18 and 19. Area F114, wave 2b
- What he asked: measured on 1A02MM against the 2 h 12 min of the baseline, the VIEWS seconds and
  the total before and after. A test on 1A02MM that no pair folder holds a test of another pair, no
  Over 150mm folder sits outside its own pair, no view shows a model of a third team, no clash is
  in two views and no mirrored test is run. The log lists the tree as a VIEWS TREE block
- Before: VIEWS 7487 s of 7926 s, 94.5 percent, steps\runs\04\item1-C02 run log lines 575 and 587
- Class: Bader's decision
- Designed on 2026-10-05, turn5\q114-design.md sections 5 and 6: the VIEWS TREE block and its seven checks, the five of point 19 and two more. The estimate on the three recording rates measured, not a bound, is VIEWS 89.701 to 811.516 s on 1A02MM, the run 528.567 to 1250.382 s, with what is UNKNOWN in it named there. Probe P18 times one view

### FR-188 views-rules-in-docs-workflow

- Sources: Q114, point 20. Area F114, wave 2b
- What he asked: the rules in docs\workflow.md with examples of where a clash goes, so each team can
  read how to use it, Saved Viewpoints for the A, B, C picture of their own team and Clash
  Detective for one clash close up
- Class: Bader's decision
- Designed on 2026-10-05, turn5\q114-design.md section 7

### FR-191 work-tracker-one-place-for-status

- Sources: Q129, part 1. Area F133, beside the waves
- What he decided: steps\tracker.csv, one row per item, covering every FR item, every F area, his
  requests and every question waiting for him, with the columns id, short title, area, wave, class, status, PR, the run that
  proved it and the date of the last change. Status is one of open, in progress, in review, merged,
  proven by a run, waiting for Bader and dropped. steps\tracker.md is made from it by a script under
  tools\tracker, never by hand, at its top in under 15 lines the counts by status and by wave, what is
  in progress now and what waits for him, then the list by wave. Every
  pull request that changes an item's status updates its row. steps\loop.md and this file keep their
  narrative and point at the tracker for status
- Proof: a check in Actions that fails when the tracker does not parse, an id appears twice, a status
  is not one of the list or an FR item of this file has no row, his words. The lead adds, by the test
  rule of CLAUDE.md, a fixture for each failure that breaks one thing
- Class: Bader's decision

### FR-194 viewpoints-switched-off-for-test-runs

- Sources: Q130. Area F136, before any test run
- What he decided: until F114, the new viewpoints, is merged, every test run has viewpoints switched
  off
- Measured on 2026-10-05: the tool has no such switch. FederationEngine.BuildViewpoints makes them for
  every group whose clash was not skipped and that has a report, src\Federator.Addin\Engine\
  FederationEngine.cs on main 35bd7fd. The window has no viewpoints option
- What will be built: a tick box in the window with its own AutomationId, read by the engine, and when
  unticked no viewpoint is made and none is asked for, so a group cannot fail at them, the rule F52 set
  for a step not asked for. The loop's driver unticks it through F126's -Untick in every test run
  until F114 merges. Its default is unticked until F114 merges and ticked after, Bader's answer to Q131
- Proof: a Core test of the rule that decides whether viewpoints are asked for, and the test of wave 1
  run with the box unticked, its log saying no viewpoint was made and why
- Class: Bader's decision

### FR-197 one-page-progress-file-that-keeps-itself-current

- Sources: Q139. Area F139, right after F133 merges, beside the viewpoints
- What he decided: steps\PROGRESS.md, never more than 60 lines, in his order: STATE with the date and
  time of the last update, the counts by wave made from steps\tracker.csv and never typed, now, next,
  waiting for Bader, at most five lines of blockers, and where the long history is. The pre-commit runs
  the tracker script whenever the csv changes, the lead rewrites the other lines after every merge, run
  and answer and before any stop, a check in the pre-commit and in Actions refuses a page over 60 lines
  or counts that do not match the csv, and the Stop gate refuses a stop while STATE is OPEN until the
  page is rewritten since the last merge or run. steps\loop.md and steps\log.md move to steps\history\.
  Every session reads CLAUDE.md, .claude\rules and the page first. When a wave merges STATE reads NEXT
  WAVE and a fresh session can start from steps\START.md, a start prompt of under 20 lines
- Proof: the check refusing a fixture for each fault of the page, each breaking one thing, the maker
  refusing a page or markers that are wrong and writing nothing, and the Stop gate's cases in
  tools\loop\prove-hooks.sh, each run before its code and after
- Class: Bader's decision

### FR-198 item-properties-harvest-deleted-and-gap-says-read-and-empty

- Sources: Q25, Q35 to Q40 and Q76. Area F120, wave 3a, after the viewpoints
- What he decided: Family, Type Name and Material deleted with their harvest. The per item harvest of
  Source File deleted, the SOURCE FINDINGS block being enough, and the per item harvest of Discipline
  deleted, the discipline read off part 5 of the NWC name staying as it is. The per item Id From in no
  output, keeping only what the ITEM IDS block needs to count. The GAP block saying plainly when a
  property was read on every item and came back empty every time, with its count, and no longer
  leaving such a property out
- Measured on 2026-10-06 off main 2eda020: src\Federator.Addin\Engine\ClashHarvest.cs reads Family,
  Type Name and Material at lines 290 to 292, by the names of lines 41 to 43 held in the settings of
  lines 67 to 71, the id and the property it came from at lines 308 to 321, and the source file and
  the discipline of each item at lines 353 to 359. They sit on ClashItem,
  src\Federator.Core\Report\ClashReportModel.cs lines 55 and 85 to 106. Of the five, GapRule alone
  reads them, src\Federator.Core\Report\GapRule.cs lines 82 to 87, and Id From is read by the ITEM IDS
  block too, ClashReportModel.cs lines 746 to 763. GapRule.cs lines 152 to 159 leave out a property no
  item carried. Set 04's 1A02MM log, steps\runs\04\item1-C02\run-20261004-185652.log lines 499 to 505,
  holds four GAP lines, Family and Type Name on 5,878 of 5,878 item cells, Material on 3,156 and Id
  From on 5,878, and leaves out Source File and Discipline, read on every item and empty every time
- What will be built: the reads, the names settings and the item's properties of Family, Type Name and
  Material deleted, and the per item reads of the source file and the discipline, which nothing else
  in src reads. The SOURCE FINDINGS block and the discipline off the NWC name untouched. Id From kept
  only as the ITEM IDS block counts it. GapRule naming a property read on every item and empty every
  time, with its count. Which properties the GAP block still reads once the harvests go is read off
  the code at the start, UNKNOWN until then. ClashReportModel.cs is shared with clash counts and
  report, the file table below
- Proof: Core tests that fail first, a GAP line for a property read and empty on every item, and the
  ITEM IDS counts the same before and after the cut, then the test of wave 3a on 1A02MM and 1A04PK,
  their GAP and ITEM IDS blocks read against set 04's
- Class: Bader's decision

### FR-199 blocks-stay-in-measured-order-with-a-priority-file

- Sources: Q45 and Q49. Area F118, wave 2c, after the viewpoints
- What he decided: Q45, no cell of ours, the Reviewed column of the Navisworks layout being enough and
  the run total staying in the log. Q49, priority a column to sort on in Excel, and the blocks staying
  in the order measured off the Navisworks exports, even when a priority file is picked
- Measured on 2026-10-06 off main 2eda020: Q45 is what main does. The workbook holds no count of ours
  of what a run moved, the Reviewed column is the client's, src\Federator.Core\Report\ClientFormat.cs
  line 34, and the run total is the RESULT line of src\Federator.Core\Diagnostics\RunLog.cs lines 1846
  to 1851, written where the penetration box was on. Q49 is not. With a priority file picked the
  blocks are sorted A, B, C, then the tests the file does not name, then by test name,
  src\Federator.Core\Report\ReportOrder.cs lines 57 to 98, the one list the workbook, the clash XML
  and the picture numbers walk, WorkbookWriter.cs line 138, ClashReportXml.cs line 160 and
  ReportOrder.cs line 132. The Priority column is there already, on every row of a block and on an
  empty test's row, src\Federator.Core\Report\WorkbookWriter.cs lines 249 to 256, 299 to 302 and 475
  to 478, and WorkbookCheck is told which order was asked for,
  src\Federator.Addin\Engine\FederationEngine.cs line 3031
- What will be built: nothing for Q45. For Q49 the priority sort of ReportOrder.Tests taken out, so
  every output walks the measured order with a file picked or not, the Priority column kept. What
  WorkbookCheck is told at FederationEngine.cs line 3031 is read again in the item, since every
  workbook is then in the measured order, and so is FR-042, the page check's false order line on a
  page in priority order, which then has no such page
- Proof: a Core test that a report with a priority file picked lists its blocks in the measured
  order, failing first, the tests of the Priority column kept, and the test of wave 2c on 1A02MM with
  the priority file, its blocks in the order of set 04's
- Class: Bader's decision

### FR-200 coverage-sheet-names-every-test-not-created

- Sources: Q46. Area F127, wave 2a, after the viewpoints
- What he decided: F77 kept. A test whose side finds nothing is not created, on every run. When a
  model grows the run is made with the XML picked, and the Coverage sheet names every test that was
  not created and why, so nothing is missed without a line saying so
- Measured on 2026-10-06 off main 2eda020: F77 is on main, src\Federator.Core\Clash\CreationPlan.cs
  and its call at src\Federator.Addin\Engine\ClashRunner.cs lines 395 to 401, each test not created
  held in CreationPlan.NotCreated with the side that finds nothing. The Coverage sheet is not on main.
  F127's Core half on the branch fix-F127 at dccf351, not merged, gives every test of the picked file
  one reason, src\Federator.Core\Coverage\CoverageReason.cs there, among them NotCreated,
  SideFoundNothing and DisciplineNotInGroup, and the sheet is its add-in half, not built
- What will be built: nothing for F77. In F127, a Core test that fails where a test the plan did not
  create has no row on the sheet or no reason on its row, beside FR-176's tests, and the sheet written
  from those rows by the add-in half
- Proof: that test, failing first against rows that leave a test not created out, and the test of
  wave 2a, the Coverage sheet of 1A02MM read back with a row and a reason for every test not created
- Class: Bader's decision

### FR-201 shared-site-named-default-reported-only

- Sources: Q78. Area F137, before the test of wave 1
- What he decided: report only. A shared site named DEFAULT, in any spelling, does not skip the clash
  by itself. The 1 m check catches a model that is really off, and the log names the site as the
  model carries it
- Measured on 2026-10-06 off main 2eda020: main does what he says. A model is on Internal only where
  its site reads Internal exactly, src\Federator.Core\Health\AlignmentCheck.cs lines 593 to 596 with
  the name at line 136, so a site named DEFAULT or DEFUALT skips nothing and fails nothing by its
  name. Where it sits is judged by the 1 m of line 152, read through ReportOptions.FarModelMillimetres.
  Its line names the site in quotes as the model carries it, lines 669 to 679, and so does the row
  file, lines 686 to 694. Set 03 shows the name as carried, a run of main before F112:
  steps\runs\03\item1-C06\run-20261001-140037.log line 2849, 1B06P1's ST model with shared coordinate
  "DEFUALT", thousands of kilometres off by its dx and dy there, and line 2854 naming DEFUALT among
  the group's four sites
- What will be built: nothing. No test names DEFAULT or DEFUALT, and the rule reads no site name but
  Internal
- Proof: the test of wave 1 shows it only where a model of the two buildings names such a site,
  UNKNOWN which
- Class: Bader's decision

### FR-203 open-statuses-rule-stays-naming-its-reader

- Sources: Q27. Area F123, wave 5
- What he decided: the rule kept in .claude\rules\core.md, naming its one reader, the image status
  filter
- Measured on 2026-10-06 off main 2eda020: main does what he says. The bullet is at
  .claude\rules\core.md lines 634 to 648, and its last sentence names the image filter, which reads
  Navisworks open at src\Federator.Core\Report\ImageOptions.cs line 43. Seen while measuring, the
  lead's to place: the viewpoint plan of F85 reads the same choice,
  src\Federator.Core\Views\ClashViewpointPlan.cs line 326, called from
  src\Federator.Addin\Engine\ViewpointBuilder.cs lines 392 and 400, which core.md line 1036 says in
  the F85 bullet, so the summary of src\Federator.Core\Clash\OpenClashes.cs lines 11 to 13, calling
  the image filter the one reader left, has not been true since F85. New plus Active is read by
  nothing outside OpenClashes.cs
- What will be built: nothing. Closed by this record, main already doing what he says
- Proof: none needed, the rule being in place
- Class: Bader's decision

### FR-204 manifest-line-9-and-its-one-exception

- Sources: Q30. Area F123, wave 5, in a pull request of its own
- What he decided: the hand written manifest may be edited. Line 9 pointed at docs\history\scan.md.
  The never edited rule stays for the rest of the bundle folder, and the rule says the manifest is
  the one exception
- Measured on 2026-10-06 off main 2eda020: line 9 of
  bundle\ParsonsNwcFederator.bundle\PackageContents.xml reads see docs\scan.md, the one file outside
  docs\history and steps naming that path, and the bundle folder holds that one file. CLAUDE.md lines
  87 to 89 say samples, steps\logs and bundle are never edited, the third changed on its own and never
  inside a fix, and lines 53 and 54 say it is not edited in a fix. The paths wall refuses a file tool
  under bundle, .claude\hooks\refuse-protected-paths.sh lines 145 and 146
- What will be built: in one pull request of its own and inside no fix, as CLAUDE.md asks of bundle,
  line 9 made to read docs\history\scan.md and the rule of CLAUDE.md made to say the manifest is the
  one exception. The paths wall refuses a file tool there, so the line is changed by a command and
  read back, and whether the wall's words change too is the lead's to say, UNKNOWN until then
- Proof: the line read back after the change, and a search finding docs\scan.md in no file outside
  docs\history and steps
- Class: Bader's decision
