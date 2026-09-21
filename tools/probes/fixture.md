# The fixtures

Built on 2026-09-21 in the close round, because nothing sat between a probe, which
measures ONE API call, and a live run, which writes into a live project folder and takes
15 to 25 minutes. Every fix in the drift round cost a whole live run to prove, which is
why four runs were needed in one night to land four fixes.

A fixture is a SMALL COPY GROUP the tool runs end to end against, in about twenty
seconds. It is the iteration loop. A LIVE RUN IS THE FINAL PROOF AND NEVER THE LOOP.

They live under `C:\Users\bader\AppData\Local\Temp\claude\round-close`, never in the repo
and never in a project folder. The NWC files are COPIES. His folders are read and never
written.

## fixture, the general one

`1104-PAR-1A04WE-ZZZ-AR/ME/ST-MOD-000001.nwc`, three files, 308 KB, 81 elements, group
`1A04WE`. Measured on 2026-09-21: 66 tests created and run, 1,764 skipped, **27 clashes**,
8 tests with clashes, 27 viewpoints created and read back, by design moved 21 of 27, and
it writes an NWF, an NWD, a 477 KB workbook and a 3.9 MB page. **19.8 seconds.**

It was chosen as the SMALLEST C04 building spanning more than one discipline. EL is left
out because it is 2,233 KB of that building's 2,532 KB and buys one more discipline for
eight times the size.

## fixture-pen, the penetration one

`1104-PAR-1A04WM-ZZZ-AR/ME-MOD-000001.nwc`, two files, 988 KB, group `1A04WM`. Measured on
2026-09-21: 21 tests run, **14 clashes**, and the penetration rule **moved 7 of them**,
reading real sizes of 21 mm, 41.7 mm and 82 mm through `BLD-AR-Floors`. **17.8 seconds.**

**IT IS AR AND ME, NOT ST, AND THAT IS THE WHOLE POINT OF THIS PARAGRAPH.** A services
through structure clash SOUNDS like AR against ST, and AR is the reliable pair.
`PenetrationSettings.DefaultSolidCategories` holds **FOUR**: Walls, Floors, Roofs and
Structural Foundations. THREE OF THE FOUR are architecture categories in these models,
which is why AR against ME fires readily.

An ST against ME fixture would fire ONLY on foundations, which most buildings have few of
at service level, so it would probably move nothing and read exactly like the rule being
broken. That is the reason to avoid it. It is NOT that ST can never fire: a service
through a FOUNDATION does move, and a fixture built on the belief that it cannot would be
built wrong.

Structural Framing and Structural Columns ARE out, and Q63 SETTLED THAT on 2026-09-20
rather than leaving it open. The reason is in the doc comment above that list, in Bader's
own evidence: of the 422 clashes he marked by hand in 1A04PW he moved 69 pipes through
slabs to Reviewed and LEFT 7 pipes through precast beams Active. A service through a slab,
a wall, a roof or a foundation is a hole somebody cuts. A service through a beam or a
column is a structural decision an engineer has to make, so it stays at New for a person
to look at.

AN EARLIER VERSION OF THIS NOTE SAID THE LIST HELD THREE, SAID STRUCTURAL FOUNDATIONS WAS
NOT ON IT, AND SAID Q63 WAS UNDECIDED. All three were wrong, all three were written with
confidence, and the second would have made the next person build the wrong fixture. They
are corrected here rather than quietly rewritten, because a document a reader trusts is
exactly where a wrong sentence does its damage.

It exists because the penetration rule is the most fragile thing in this tool by history.
It shipped on 2026-09-19 and moved nothing, then moved 4, then moved 56 once the worded
size reader landed on 2026-09-20. Wrong twice in three days, and it had no fast loop.

## What they cannot show

Said here so nobody reads a green fixture as a green run. One small group each, so
neither shows scale, the weekly path, a CHANGED group, alignment across many models, the
two non buildings, the single discipline groups, or anything needing the real matrix
against real content. `fixture` exercises by design and NOT penetrations, because 1A04WE
has no small service through a solid at 25 mm, and its zero is real: its six buckets read
4 over the size, 0 unreadable, 0 already decided, 0 both a service, 7 both a solid and 16
not a service against a solid, which adds to 27. A zero with a spread of reasons under it
is a measurement. A zero with everything in one bucket is 5r's signature and a finding.

## How to run one

Install the bundle, then drive the window at the fixture's own four folders:

    build\install.ps1
    <temp>\round-close\open-addin.ps1
    <temp>\round-close\drive.ps1 -Source <fixture>\NWC -Nwf <fixture>\NWF `
                                 -Nwd <fixture>\NWD -Excel <fixture>\Report

The log lands in the fixture's own NWF folder and nothing outside the temp folder is
touched.

## the three scale modes, added 2026-09-21

`tools\probes\ViewpointProbe` gained `scale`, `scalemany` and `scaleclash`, because the
fixtures answer "does this work" in twenty seconds and could not answer "what does this
cost at two thousand". A fixture has 27 clashes and the group that broke had 2,566.

    scale       writes 400 viewpoints into ONE folder of one copy, reporting the cost
                every 25, twice, once with the dimming on and once off
    scalemany   writes 40 into each of several copies, printing what each document holds
                beside the cost, so the driver can be read off a line
    scaleclash  THE CONTROL. One open document, 40 written, then TestsClearResults on
                every test and 40 more, then SavedViewpoints.Clear() and 40 more. One
                thing changes at a time and the others do not move

**THE CONTROL IS THE POINT OF THE THIRD ONE.** `scalemany` showed the cost tracking the
clash result count across nine files, from 59 ms at 29 results to 1,872 ms at 568, and
that correlation is FALSE. `scaleclash` cleared every clash result on the same open
document and the cost did not move, 1,878 ms to 1,873 ms. Clearing the saved viewpoints
took it to 6.5 ms. Nine files agreeing with a wrong explanation is what a control is for.

**WHAT THESE CANNOT SHOW, and it is the same shape as everything else here.** They add
through the COM collection and never touch `document.SavedViewpoints`, so the .NET tree is
never made to materialise. The real writer reads every viewpoint back through that tree,
which forces exactly that, and that is why the probe's 400 viewpoint pass stayed flat at
411 ms while the live run climbed. **The probe proves the DRIVER and understates the
SIZE.** A number off these modes is a lower bound and never a prediction of a run.
