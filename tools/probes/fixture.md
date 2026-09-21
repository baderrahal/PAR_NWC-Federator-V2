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
through structure clash SOUNDS like AR against ST and it is not.
`PenetrationSettings.DefaultSolidCategories` holds **Walls, Floors and Roofs**, and in
these models those are ARCHITECTURE categories. ST carries Structural Foundations,
Structural Framing and Structural Columns, none of which is on the solid list, so a ME
against ST fixture would exercise the rule's plumbing and move NOTHING, and would read
exactly like the rule being broken. Whether the structural categories should join the
list is Q63 and is not decided.

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
