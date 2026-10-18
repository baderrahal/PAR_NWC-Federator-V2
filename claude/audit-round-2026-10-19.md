# Audit of the round of 19 Oct, F231 to F238

Bader's round of 19 Oct 2026, read off multi3 on 18 Oct: six fixes in his order, the full run for him and the end of the
round. This file is begun by F231 with the append table his fix 1 asks for, and each later fix adds its part. F238 writes the
rest.

## F231, the append measured against a manual Append

The NWCs of 1B06BC, 1B06PS and 1B06K1 copied out of the desktop folder's NWC\C06 into C:\NWC-PC-Test\work\append-check, their
dates kept, the desktop folder 0 lines differing before and after. Each group appended by hand in the window of a plain
Navisworks Manage 2025, File, New, Append with the NWCs in the tool's order and Save as <group> manual.nwf, no option touched,
then built by the tool's NWC page of main f92023f into the same folder, then run on the NWF page, its Refresh and UpdateFiles.
tools\probes\AppendProbe read each NWF and NWD read only, every file the same SHA-256 after. docs\history\scan.md 5z-zn.

Positions in metres, E N Z, as Navisworks reports the units of each model. T is the translation of the model's transform, box
its root item's box, min to max. A difference is the largest of the nine numbers set against the manual Append, in mm.

| model | manual Append | tool after the NWC page | tool after the NWF page | difference after the NWC page, mm | difference after the NWF page, mm |
|---|---|---|---|---|---|
| 1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -0.372 to 11.416 7.827 8.457 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -0.372 to 11.416 7.827 8.457 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -0.372 to 11.416 7.827 8.457 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc | Feet, T 323886.396 2755364.307 11.033, box 323867.750 2755346.922 6.602 to 323922.019 2755377.178 18.729 | Feet, T 323886.396 2755364.307 11.033, box 323867.750 2755346.922 6.602 to 323922.019 2755377.178 18.729 | Feet, T 323886.396 2755364.307 11.033, box 323867.750 2755346.922 6.602 to 323922.019 2755377.178 18.729 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box -7.535 -9.170 -5.331 to 25.142 6.919 7.325 | Feet, T 0.000 0.000 0.000, box -7.535 -9.170 -5.331 to 25.142 6.919 7.325 | Feet, T 0.000 0.000 0.000, box -7.535 -9.170 -5.331 to 25.142 6.919 7.325 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc | Feet, T -33.976 -1.625 0.000, box -16.774 -15.145 -2.100 to 11.170 12.327 8.000 | Feet, T -33.976 -1.625 0.000, box -16.774 -15.145 -2.100 to 11.170 12.327 8.000 | Feet, T -33.976 -1.625 0.000, box -16.774 -15.145 -2.100 to 11.170 12.327 8.000 | 0.000 | 0.000 |
| 1104-PAR-1B06BC-ZZZ-ST-MOD-000002.nwc | Feet, T 34.196 0.452 -4.331, box 19.325 -10.098 -6.381 to 34.946 1.202 0.094 | Feet, T 34.196 0.452 -4.331, box 19.325 -10.098 -6.381 to 34.946 1.202 0.094 | Feet, T 34.196 0.452 -4.331, box 19.325 -10.098 -6.381 to 34.946 1.202 0.094 | 0.000 | 0.000 |
| 1104-PAR-1B06K1-ZZZ-AR-MOD-000001.nwc | Feet, T 658144.882 2746014.845 683.828, box 658136.196 2746027.197 683.632 to 658147.127 2746042.493 687.531 | Feet, T 658144.882 2746014.845 683.828, box 658136.196 2746027.197 683.632 to 658147.127 2746042.493 687.531 | Feet, T 658144.882 2746014.845 683.828, box 658136.196 2746027.197 683.632 to 658147.127 2746042.493 687.531 | 0.000 | 0.000 |
| 1104-PAR-1B06K1-ZZZ-ME-MOD-000001.nwc | Feet, T 658144.890 2746014.825 683.828, box 658136.693 2746039.006 682.678 to 658144.804 2746043.103 687.694 | Feet, T 658144.890 2746014.825 683.828, box 658136.693 2746039.006 682.678 to 658144.804 2746043.103 687.694 | Feet, T 658144.890 2746014.825 683.828, box 658136.693 2746039.006 682.678 to 658144.804 2746043.103 687.694 | 0.000 | 0.000 |
| 1104-PAR-1B06K1-ZZZ-ST-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 12.875 6.258 3.639 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 12.875 6.258 3.639 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 12.875 6.258 3.639 | 0.000 | 0.000 |
| 1104-PAR-1B06PS-ZZZ-AR-MOD-000001.nwc | Feet, T 660063.927 2744376.693 675.667, box 660062.071 2744451.163 675.065 to 660077.795 2744466.295 681.568 | Feet, T 660063.927 2744376.693 675.667, box 660062.071 2744451.163 675.065 to 660077.795 2744466.295 681.568 | Feet, T 660063.927 2744376.693 675.667, box 660062.071 2744451.163 675.065 to 660077.795 2744466.295 681.568 | 0.000 | 0.000 |
| 1104-PAR-1B06PS-ZZZ-EL-MOD-000001.nwc | Feet, T 323886.396 2755364.307 11.033, box 323961.367 2755357.037 8.608 to 323976.914 2755373.546 15.981 | Feet, T 323886.396 2755364.307 11.033, box 323961.367 2755357.037 8.608 to 323976.914 2755373.546 15.981 | Feet, T 323886.396 2755364.307 11.033, box 323961.367 2755357.037 8.608 to 323976.914 2755373.546 15.981 | 0.000 | 0.000 |
| 1104-PAR-1B06PS-ZZZ-ST-MOD-000001.nwc | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 88.197 11.033 6.467 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 88.197 11.033 6.467 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 88.197 11.033 6.467 | 0.000 | 0.000 |

| NWD | union of the manual boxes | the NWD after the NWC page | the NWD after the NWF page | difference after the NWC page, mm | difference after the NWF page, mm |
|---|---|---|---|---|---|
| 1B06BC | box -17.020 -15.391 -6.381 to 323922.019 2755377.178 18.729 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -6.381 to 323922.019 2755377.178 18.729 | Feet, T 0.000 0.000 0.000, box -17.020 -15.391 -6.381 to 323922.019 2755377.178 18.729 | 0.000 | 0.000 |
| 1B06K1 | box -1.370 -1.140 -1.550 to 658147.127 2746043.103 687.694 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 658147.127 2746043.103 687.694 | Feet, T 0.000 0.000 0.000, box -1.370 -1.140 -1.550 to 658147.127 2746043.103 687.694 | 0.000 | 0.000 |
| 1B06PS | box 77.005 -1.827 -1.400 to 660077.795 2755373.546 681.568 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 660077.795 2755373.546 681.568 | Feet, T 0.000 0.000 0.000, box 77.005 -1.827 -1.400 to 660077.795 2755373.546 681.568 | 0.000 | 0.000 |

Largest difference 0.000 mm, 0 numbers UNKNOWN, the source file names and units of all 22 pairs the same.

So the tool's append matches the manual Append, and the far models are in the NWC files themselves. No code changed.

## F232, packages first, one route per page

The NWC page has four steps, 1. Packages, 2. Grouping, 3. Outputs and 4. Clash, and the one Run button in the bar under the
steps. The NWF page has its packages first, a tree of community, package and NWF read off the Report folder, then the
tolerance, the five step boxes and one Run. Gone: the Source step, step 5 and its Run button, the NWF and NWD folder boxes,
More, rarely changed, the NWF picker with its File and Folder buttons, the Recent box and the panel on the right, with every
member and test only they used: the scanned run of the window, the picked NWFs' plan, the list of NWFs last used, the names
typed over, four picker kinds, and the engine's scanned run constructor and preview. 47 files under src, tests and tools,
2175 lines added and 4452 taken out.

Proved on copies under C:\NWC-PC-Test\work\smoke232, a parent holding 1B06PS's three NWCs itself and C06 with 1B06BC's five
and 1B06PG's one, the build ab1c9167: the tree read the parent as community Parent with package Parent itself, Run with
nothing ticked was refused, the findings under the tree counted 9 files of the ticked packages, step 2 showed 1B06BC and 1B06PS
ticked and 1B06PG reading One discipline, not built with its box off and greyed, the NWC page ran 2 groups each in its own
Navisworks, 2 DONE, 1B06PG named left out, then the NWF page read 2 NWFs off the Report folder, ran both each in its own
Navisworks, 2 DONE. The desktop folder 0 lines differing, Navisworks' settings put back with 0 lines differing.

The reviewer and the breaker read the change. Fixed here: a Report folder that would put the parent's own package inside the
parent is refused, a ticked NWF gone at Run is named, a row its rule left unticked is no person's untick, a group whose NWF is
there reads NWF exists. Use the NWF page., the stand-in of the loop builds again, and the window's new words are Core's. Asked:
Q213 a parent and a folder of its name, Q214 the changed files list and the table on the NWF page, Q215 where the one Run
button sits. Open as rows F232-R1 to R9, most older than F232: the loop's window driver and two probes name the controls that
went, one manifest per package Log folder, rebuilt NWFs in dated folders the tree does not show, a run reading the last Read,
the two groupings that make one discipline groups, the discipline workbooks missing unread files, left out groups in no RESULT
line, a drive root parent and a wide Report folder, ticks reset by a package ticked again.
