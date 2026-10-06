# The work tracker

Made by tools\tracker\make-tracker.ps1 from steps\tracker.csv, never by hand. Status lives in steps\tracker.csv only.

- By status, of 395 rows: open 153, in progress 55, in review 0, merged 140, proven by a run 22, waiting for Bader 22, dropped 3
- By wave: 1 19, 2a 28, 2a and 2b 1, 2b 29, 2b and before any test run 1, 2c 6, 3a 11, 3b 20, 4 42, 5 68, all 1, before any probe or run starts again 3, before any test run 3, before the test of wave 1 4, before the waves 114, beside the waves 7, first of all since Bader's order of 2026-10-05 1, none 35, outside the waves 2
- In progress now: F109 install, F114 views, F115 sets, F127 coverage of the clash XML, F131 teams, F132 mirrored tests, F134 the code health gate, F138 loop starts with Auto-Save off, Q94 code runs every real file itself, full runs of main and the fixes they show, Q98 the full fix round, Q112-2 coverage of the clash XML (FR-176), Q114 one view per clash test by team (FR-180 to FR-188), Q129 a clean tracker and a code health gate (FR-191 to FR-193), Q132 the new viewpoints first, F136 then F131, F132 and F114, Q135 too many autosave copies, and 40 FR items
- Waits for Bader, 22 rows: F18, Q25, Q27, Q28, Q29, Q30, Q31, Q35, Q36, Q37, Q38, Q39, Q40, Q45, Q46, Q47, Q49, Q50, Q51, Q76, Q77, Q78

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
| FR-010 | empty sets contains judged as equals | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-011 | empty sets names and lists typed in core | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-012 | revit worksets list unread looks empty | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-013 | set sides read failure returns empty so sets removed | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-014 | set leftover walk skips test folders and second source | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-015 | set drift key ignores negation group and comparison | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-016 | set drift lines describe or set as and chain | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-017 | set value unreadable kind read as empty string | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-018 | rebuilt set not found again counted as zero items | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-019 | set parent handle held across replacewithcopy | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-020 | rebuilt sets do not ask for nwf save | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-021 | set lines claim every set matches when unread | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-022 | sets summary counts created sets only | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-023 | set warnings negated category reported as asked | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-024 | identical sets signature ignores flags | F115 | silent wrong number | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-027 | empty sets block never written on a first run | F115 | broken feature | in progress | none, branch fix-F115 | none | 2026-10-05 |
| FR-176 | coverage of the clash xml | F127 | Bader's request | in progress | none, branch fix-F127 | none | 2026-10-05 |
| FR-182 | mirrored tests kept once | F132 | Bader's decision | in progress | none, branch fix-F132 | none | 2026-10-05 |
| FR-183 | mirrored tests in an existing nwf | F132 | Bader's decision | in progress | none, branch fix-F132 | none | 2026-10-05 |
| F115 | sets | F115 | fix | in progress | none, branch fix-F115 | none | 2026-10-05 |
| F127 | coverage of the clash XML | F127 | fix | in progress | none, branch fix-F127 | none | 2026-10-05 |
| F132 | mirrored tests | F132 | fix | in progress | none, branch fix-F132 | none | 2026-10-05 |
| Q112-2 | coverage of the clash XML (FR-176) | F127 | Bader's request | in progress | none, branch fix-F127 | none | 2026-10-05 |
| Q121 | telecom fixtures and telephone devices | F132 | question | merged | 118 | none | 2026-10-05 |
| Q122 | whose status a result carries | F132 | question | merged | 118 | none | 2026-10-05 |
| Q126 | no workbook for the coverage sheet when the clash is skipped | F127 | question | merged | 118 | none | 2026-10-05 |
| Q127 | how RESULT counts the tests of the XML | F127 | question | merged | 118 | none | 2026-10-05 |
| Q133 | a mirrored test can find more than the one it mirrors | F132 | question | merged | 121 | none | 2026-10-05 |

## Wave 2a and 2b

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q114 | one view per clash test by team (FR-180 to FR-188) | F131, F132, F114 | Bader's request | in progress | none, branches fix-F131, fix-F132, fix-F114 | none | 2026-10-05 |

## Wave 2b

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-065 | viewpoint dimming carries between viewpoints | F114 | silent wrong number | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-066 | size text reads tail of word digits | F114 | silent wrong number | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-067 | clear rebuild drops viewpoints | F114 | broken feature | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-068 | size tally never constructed | F114 | broken feature | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-069 | views cost per viewpoint | F114 | slow | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-070 | views 45 minute basis and options | F114 | slow | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-071 | views log silent up to 21 minutes | F114 | slow | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-072 | penetrations upwards walk outside try | F114 | loud failure | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-073 | views seconds parts do not add | F114 | noise | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-177 | generic models counted and a set per model | F128 | Bader's request | open | none | none | 2026-10-04 |
| FR-180 | team map beside the picked xml | F131 | Bader's decision | in progress | none, branch fix-F131 | none | 2026-10-05 |
| FR-181 | mechanical sets miss hv pl fp models | F131 | Bader's decision | in progress | none, branch fix-F131 | none | 2026-10-05 |
| FR-184 | views tree by priority and team pair | F114 | Bader's decision | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-185 | one view per test of its open clashes | F114 | Bader's decision | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-186 | views made fresh only the tools own | F114 | Bader's decision | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-187 | views proof on 1a02mm and the views tree block | F114 | Bader's decision | in progress | none, branch fix-F114 | none | 2026-10-04 |
| FR-188 | views rules in docs workflow | F114 | Bader's decision | in progress | none, branch fix-F114 | none | 2026-10-04 |
| F114 | views | F114 | fix | in progress | none, branch fix-F114 | none | 2026-10-04 |
| F128 | generic models | F128 | fix | open | none | none | 2026-10-04 |
| F131 | teams | F131 | fix | in progress | none, branch fix-F131 | none | 2026-10-05 |
| Q112-3 | generic models counted and a set per model (FR-177) | F128 | Bader's request | open | none | none | 2026-10-04 |
| Q77 | the clear and rebuild does not copy the viewpoints | F114 | question | waiting for Bader | none | none | 2026-09-21 |
| Q115 | where the team map lives | F131 | question | merged | 118 | none | 2026-10-05 |
| Q116 | where the team map applies | F131 | question | merged | 118 | none | 2026-10-05 |
| Q117 | a set name with no discipline code | F114 | question | merged | 118 | none | 2026-10-05 |
| Q118 | a clashing item in a model of a third team | F114 | question | merged | 118 | none | 2026-10-05 |
| Q119 | which models a view shows | F114 | question | merged | 118 | none | 2026-10-05 |
| Q120 | a view the tool made that a person changed | F114 | question | merged | 118 | none | 2026-10-05 |
| Q123 | a run with no XML picked and the team map | F131 | question | merged | 118 | none | 2026-10-05 |

## Wave 2b and before any test run

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q131 | the default of the viewpoints box | F114, F136 | question | merged | 118 | none | 2026-10-05 |

## Wave 2c

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-035 | workbook check counts only blocks with clashes | F118 | silent wrong number | open | none | none | 2026-10-04 |
| FR-036 | skipped test row keeps old tolerance after chosen edit | F118 | silent wrong number | open | none | none | 2026-10-04 |
| FR-037 | priority csv repeated test name last row wins silently | F118 | silent wrong number | open | none | none | 2026-10-04 |
| FR-040 | item ids guid fallback counted missing | F118 | silent wrong number | open | none | none | 2026-10-04 |
| FR-041 | grid location empty on 345 rows | F118 | broken feature | open | none | none | 2026-10-04 |
| F118 | workbook and report | F118 | fix | open | none | none | 2026-10-04 |

## Wave 3a

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-075 | picture rename two pass has no undo | F120 | silent wrong number | open | none | none | 2026-10-04 |
| FR-076 | image guard per group not per run | F120 | broken feature | open | none | none | 2026-10-04 |
| FR-077 | pictures cost 643 seconds | F120 | slow | open | none | none | 2026-10-04 |
| FR-078 | install recursive delete without junction check and partial first install | F109 | broken feature | in progress | none, branch fix-F109 at a0c3829 | none | 2026-10-04 |
| FR-079 | install killed between move and removal | F109 | broken feature | in progress | none, branch fix-F109 at a0c3829 | none | 2026-10-04 |
| FR-080 | install leftovers exit codes and success line | F109 | broken feature | in progress | none, branch fix-F109 at a0c3829 | none | 2026-10-04 |
| FR-178 | start from an existing nwf | F129 | Bader's request | open | none | none | 2026-10-04 |
| F109 | install | F109 | fix | in progress | none, branch fix-F109 at a0c3829 | none | 2026-10-04 |
| F120 | harvest and pictures | F120 | fix | open | none | none | 2026-10-04 |
| F129 | start from an existing NWF | F129 | fix | open | none | none | 2026-10-04 |
| Q112-4 | start from an existing NWF (FR-178) | F129 | Bader's request | open | none | none | 2026-10-04 |

## Wave 3b

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-043 | reshaped group done but nwf never saved | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-044 | nwf save false return ignored after rebuild | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-045 | nwf save after clash work false return ignored | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-046 | result file sizes wrong for the two open logs | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-047 | tsv number column rounds a tolerance | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-048 | tsv size zero for nwc not on disk | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-049 | second run in window result covers both runs | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-050 | run started but not finished reads as no run marked | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-051 | result states zero waiting when no run marked | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-052 | notolerance skip missing from skip reasons | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-053 | tolerance set on count counts tests not edited | F119 | silent wrong number | open | none | none | 2026-10-04 |
| FR-054 | log prune in temp fallback deletes other programs logs | F119 | broken feature | open | none | none | 2026-10-04 |
| FR-055 | tsv files never pruned | F119 | broken feature | open | none | none | 2026-10-04 |
| FR-056 | live line slower compares one test with whole group | F119 | broken feature | open | none | none | 2026-10-04 |
| FR-057 | run log write has no try | F119 | loud failure | open | none | none | 2026-10-04 |
| FR-179 | shift range tick in the group list | F130 | Bader's request | open | none | none | 2026-10-04 |
| FR-189 | nwd listed as written when its publish failed | F119 | silent wrong number | open | none | none | 2026-10-05 |
| F119 | run log and RESULT | F119 | fix | open | none | none | 2026-10-04 |
| F130 | the Shift range in the group list | F130 | fix | open | none | none | 2026-10-04 |
| Q112-5 | Shift range tick in the group list (FR-179) | F130 | Bader's request | open | none | none | 2026-10-04 |

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
| FR-150 | apply file settings does not save nwf | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-151 | rebuild tally before minus one reads as none | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-152 | undo auto reviewed undoes a persons reviewed | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-153 | regroup throws away typed names and run ticks | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-154 | scan findings use group key as building code | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-155 | exchange reader missing attribute becomes zero or false | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-156 | probe csv listed written after write threw | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-157 | probe unread value written as empty value | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-158 | probe category match trims but tally does not | F121 | silent wrong number | open | none | none | 2026-10-04 |
| FR-159 | emptied pattern field passes name check | F121 | broken feature | open | none | none | 2026-10-04 |
| FR-160 | cleared name cell pinned as empty name | F121 | broken feature | open | none | none | 2026-10-04 |
| FR-161 | date format has no control | F121 | broken feature | open | none | none | 2026-10-04 |
| FR-162 | folder memory save failure never reported | F121 | broken feature | open | none | none | 2026-10-04 |
| FR-163 | title bar close mid run | F121 | broken feature | open | none | none | 2026-10-04 |
| FR-164 | census sets overload returns zero not minus one | F121 | loud failure | open | none | none | 2026-10-04 |
| FR-165 | cleared name cell throws out of run click | F121 | loud failure | open | none | none | 2026-10-04 |
| FR-166 | tolerance other blank stops hand buttons | F121 | loud failure | open | none | none | 2026-10-04 |
| F121 | the rest | F121 | fix | open | none | none | 2026-10-04 |
| F122 | the loop tools | F122 | fix | open | none | none | 2026-10-04 |

## Wave 5

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-007 | invisible difference fallback for visible space | F123 | noise | open | none | none | 2026-10-04 |
| FR-029 | set builder handles never disposed | F123 | noise | open | none | none | 2026-10-04 |
| FR-038 | workbook stray grey row after last empty test | F123 | noise | open | none | none | 2026-10-04 |
| FR-039 | thumbnail row height overwritten by clash row height | F123 | noise | open | none | none | 2026-10-04 |
| FR-042 | page check warns wrong order on priority sorted page | F123 | noise | open | none | none | 2026-10-04 |
| FR-058 | first clash line says all tests to create | F123 | noise | open | none | none | 2026-10-04 |
| FR-059 | result files written leaves out pictures | F123 | noise | open | none | none | 2026-10-04 |
| FR-060 | result found none counts groups that ran no test | F123 | noise | open | none | none | 2026-10-04 |
| FR-061 | log says tsv keeps collapsed lines when tsv did not open | F123 | noise | open | none | none | 2026-10-04 |
| FR-062 | units line names unit when second read threw | F123 | noise | open | none | none | 2026-10-04 |
| FR-063 | tolerance log line says read from xml on no xml run | F123 | noise | open | none | none | 2026-10-04 |
| FR-064 | category line names no folder | F123 | noise | open | none | none | 2026-10-04 |
| FR-074 | views unknown set not named | F123 | noise | open | none | none | 2026-10-04 |
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
| FR-126 | single discipline sentence says every test is created | F123 | noise | open | none | none | 2026-10-04 |
| FR-127 | tolerance help line says resets results | F123 | noise | open | none | none | 2026-10-04 |
| FR-128 | apply line says results reset contradicts kept | F123 | noise | open | none | none | 2026-10-04 |
| FR-129 | failure guard words say first n tests | F123 | noise | open | none | none | 2026-10-04 |
| FR-130 | refill message counts rows not cells | F123 | noise | open | none | none | 2026-10-04 |
| FR-131 | outputs label wrong reason when nwf inside source | F123 | noise | open | none | none | 2026-10-04 |
| FR-132 | rebuild box help line omits removal | F123 | noise | open | none | none | 2026-10-04 |
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
| FR-167 | shared source uses group key not building code | F123 | noise | open | none | none | 2026-10-04 |
| FR-168 | folders memory read failure logged as first run | F123 | noise | open | none | none | 2026-10-04 |
| FR-169 | document guard unreadable name called unsaved | F123 | noise | open | none | none | 2026-10-04 |
| FR-170 | penetration size catch swallows unit throw | F123 | noise | open | none | none | 2026-10-04 |
| FR-171 | t1 noise findings 93 | F123 | noise | open | none | none | 2026-10-04 |
| FR-172 | t1 uncalled members 150 | F123 | noise | open | none | none | 2026-10-04 |
| FR-173 | t1 catch swallowing 77 | F123 | noise | open | none | none | 2026-10-04 |
| FR-174 | one public type per file | F124 | noise | open | none | none | 2026-10-04 |
| F117 | the names | F117 | fix | open | none | none | 2026-10-04 |
| F123 | docs and words, and the noise of every area | F123 | fix | open | none | none | 2026-10-04 |
| F124 | D1, one public type per file | F124 | fix | open | none | none | 2026-10-04 |
| Q28 | two handle reads left where they are | F123 | question | waiting for Bader | none | none | 2026-09-12 |

## all

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q98 | the full fix round | all | Bader's request | in progress | none | none | 2026-10-04 |

## before any probe or run starts again

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| F138 | loop starts with Auto-Save off | F138 | fix | in progress | none, branch fix-F138 | none | 2026-10-05 |
| FR-196 | loop runs write no autosave and copy his folder once | F138 | Bader's decision | in progress | none, branch fix-F138 | none | 2026-10-05 |
| Q135 | too many autosave copies | F138 | Bader's request | in progress | none, branch fix-F138 | none | 2026-10-05 |

## before any test run

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-194 | viewpoints switched off for test runs | F136 | Bader's decision | merged | 117 | none | 2026-10-05 |
| F136 | the viewpoints switch | F136 | fix | merged | 117 | none | 2026-10-05 |
| Q130 | stop the C02 weekly run and no viewpoints in test runs until F114 merges (FR-194) | F136 | Bader's request | merged | 117 for F136, 116 records the stop | none | 2026-10-05 |

## before the test of wave 1

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-195 | no site and no clash groups end partial | F137 | Bader's decision | open | none | none | 2026-10-05 |
| F137 | no site and no clash groups | F137 | fix | open | none | none | 2026-10-05 |
| Q111 | a model that names no site at all | F137 | question | merged | 118 | none | 2026-10-05 |
| Q125 | a group that runs no clash test, a model off the coordinates | F137 | question | merged | 118 | none | 2026-10-05 |

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
| F18 | add the 1A04WE sample, when Bader uploads it, Q9 | F18 | fix | waiting for Bader | none | none | 2026-09-06 |
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

## beside the waves

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| FR-191 | work tracker one place for status | F133 | Bader's decision | merged | 122 | none | 2026-10-05 |
| FR-192 | code health gate lists only shrink | F134 | Bader's decision | in progress | none, measured under %LOCALAPPDATA%\NwcFederatorLoop\health | none | 2026-10-05 |
| FR-193 | analyser settings merged alone | F135 | Bader's decision | open | none | none | 2026-10-05 |
| F133 | the work tracker | F133 | fix | merged | 122 | none | 2026-10-05 |
| F134 | the code health gate | F134 | fix | in progress | none, measured under %LOCALAPPDATA%\NwcFederatorLoop\health | none | 2026-10-05 |
| F135 | the analyser settings that touch every project | F135 | fix | open | none | none | 2026-10-05 |
| Q129 | a clean tracker and a code health gate (FR-191 to FR-193) | F133, F134, F135 | Bader's request | in progress | none, branch fix-F133 | none | 2026-10-05 |

## first of all since Bader's order of 2026-10-05

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| Q132 | the new viewpoints first, F136 then F131, F132 and F114 | F136, F131, F132, F114 | Bader's request | in progress | 117 merged for F136, branches fix-F131, fix-F132, fix-F114 | none | 2026-10-05 |

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
| Q25 | five clash item properties reach no output, split into Q35 to Q40 | none | question | waiting for Bader | none | none | 2026-09-12 |
| Q27 | the two choice rule for still outstanding lost every reader | none | question | waiting for Bader | none | none | 2026-09-12 |
| Q29 | three settable properties nothing outside the code sets | none | question | waiting for Bader | none | none | 2026-09-12 |
| Q30 | the bundle manifest points at the old scan.md path | none | question | waiting for Bader | none | none | 2026-09-12 |
| Q31 | five names listed wrongly in the F40 entry | none | question | waiting for Bader | none | none | 2026-09-18 |
| Q35 | the GAP block, Family reaches no output | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q36 | the GAP block, Type Name | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q37 | the GAP block, Material | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q38 | the GAP block, Source File | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q39 | the GAP block, Discipline | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q40 | the GAP block, Id From | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q45 | a count of the clashes this run moved to Reviewed | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q46 | F77 against the single discipline rule | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q47 | four service categories not on the service list | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q49 | a priority file replaces the measured block order | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q50 | F72c widens the one status guard for the undo | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q51 | the first size property or the largest | none | question | waiting for Bader | none | none | 2026-09-19 |
| Q76 | the source file column empty on every row | none | question | waiting for Bader | none | none | 2026-09-21 |
| Q78 | C04 models naming misspelled shared sites | none | question | waiting for Bader | none | none | 2026-09-21 |
| Q110 | a skipped group and its tests | none | question | merged | 118 | none | 2026-10-05 |
| Q124 | the baseline of 1A04PK takes a day or more | none | question | merged | 118 | none | 2026-10-05 |
| Q128 | what the C04 baseline run did not put back | none | question | merged | 121 | none | 2026-10-05 |
| Q134 | code that waits for F114 by Bader's order | none | question | merged | 124 | none | 2026-10-05 |

## outside the waves

| id | short title | area | class | status | PR | the run that proved it | the date of the last change |
|---|---|---|---|---|---|---|---|
| F125 | a window that is not modal is a pane and not a dialog | F125 | fix | merged | 102 | none | 2026-10-06 |
| F126 | the window driver unticks a tick box by its AutomationId | F126 | fix | merged | 112 | none | 2026-10-05 |
