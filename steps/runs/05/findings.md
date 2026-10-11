# Set 05, main 4bc4c83, 1A02MM and 1A04PK with the coordinates rule on, 2026-10-11

Main 4bc4c83 installed in place at 13:05, its stamp read back. Set 05's copy of 8 Oct, the copy's clash XML
with 1830 tests at 25 mm and no corrections or teams file beside it, every group ticked, the viewpoints box
ticked and Skip clash for models off shared coordinates left ticked, through the real window with no click.
Both VERDICT RAN. "C02" is item1-C02\run-20261011-130815.log and "C04" is item1-C04\run-20261011-132041.log.

## Bader's three tests of done

1. GROUPS: 1A02MM PARTIAL and 1A04PK PARTIAL, each because its clash was skipped by the coordinates rule, C02:598
   and C04:657
2. TIME: 31 s and 58 s against 45 min, C02:711 and C04:780. It means nothing for the 45 minutes, since no test ran
3. CLASH DETECTIVE: nothing to compare, no test ran, C02:474 "clashes found : not counted"

## What the runs prove

- The coordinates rule, F112 and F137: PROVEN. 1A02MM's EL on the site LTB2 and ST on Internal sit 2,823 km from
  the reference and the clash is skipped, C02:126 to 134. 1A04PK's skip names 6 models, C04:170. A model 5 mm off
  is listed and not counted, which is right under the 1 m rule
- Generic Models, F128: PROVEN. 1A04PK, 572 items over 5 of 10 models, 3, 6, 32, 528 and 3, C04:297 to 308, and the
  internal workbook's Generic Models sheet holds the five with their Items, read off the xlsx itself. 1A02MM, 0 in
  4 models, C02:245
- The sets, F115: PROVEN. 61 created in each, C02:232 and C04:281, EMPTY SETS and SETS ACROSS THE RUN agree
- The coverage, F127, with no test run: PROVEN in that case. The reasons add to 1830 in each, C04:589 to 591, the
  Coverage sheet in the internal workbook with the picked file's sha256 beside it, read off the xlsx, and 1830
  coverage test rows in each .tsv
- The run log and RESULT, F119: PROVEN. RESULT lists the six files of each run at the sizes outputs.txt reads
- NOT REACHED: the views, the mirror merge, the clash workbook and its check, the clash counts, the pictures, the
  corrections and the team map. Set 06 runs them with the box unticked

## Findings, worst first

Silent wrong numbers:

1. TIMING BESIDE SIZE writes "0 clashes" for a group whose clash was skipped, C02:625 and C04:684, while RESULT
   says "clashes found : UNKNOWN", C02:726, and the block's own legend says UNKNOWN is never a zero, C02:633.
   Row T6-R-ZEROCLASHES

Noise:

2. The MIRROR block says "Both tests of each pair are run" over a group whose clash is skipped, C02:263 and
   C04:322. Row T6-R-MIRRORWORDS
3. The workbook read-out reads a one row sheet in columns C to K, so the Generic Models sheet's Items in column B
   read as missing. The xlsx holds them. A reader of the loop's read-outs, frozen under Q143, and not a fault of
   the tool, so no row
