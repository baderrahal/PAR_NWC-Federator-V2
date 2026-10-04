# Five C06 groups with a model on Revit's internal origin

From the clash run of the C06 folder on 2026-10-01. In each group below, at least one NWC
names its site "Internal". That is the name Revit gives a model that was not exported on a
shared site, so the model sits in its own coordinate system and not in the project's. Its
clashes with the other disciplines of its group cannot be trusted until it is exported again.

What to do: export the NWC again from Revit on the project's shared coordinates and drop it
into the folder with the same file name. Which shared site is the right one for each building
is for the project to say. The tool cannot tell, and the models of one group below already
name several.

The distances are how far each model sits from the group's reference model, read off the
NWC files by the clash tool. The tool changed nothing in any model.

## 1B06BC

- AR 1104-PAR-1B06BC-ZZZ-AR-MOD-000001.nwc names its site "Internal". It is the group's
  reference model, so every other distance in this group is measured from it
- ME 1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc, site "16-S01", sits in the same place as the AR
- ST 1104-PAR-1B06BC-ZZZ-ST-MOD-000001.nwc, site "T1", about 34 m from the AR
- ST 1104-PAR-1B06BC-ZZZ-ST-MOD-000002.nwc, site "BCTANK", about 34.5 m from the AR
- EL 1104-PAR-1B06BC-ZZZ-EL-MOD-000001.nwc, site "SITEWIDE PHASE 3", about 2,774 km from
  the AR

## 1B06G1

- AR 1104-PAR-1B06G1-ZZZ-AR-MOD-000001.nwc names its site "Internal". It is the group's
  reference model
- ME 1104-PAR-1B06G1-ZZZ-ME-MOD-000001.nwc, site "16-S01", sits in the same place as the AR
- ST 1104-PAR-1B06G1-ZZZ-ST-MOD-000001.nwc, site "WT_1", about 16.9 m from the AR
- ST 1104-PAR-1B06G1-ZZZ-ST-MOD-000002.nwc, site "WT", about 39.2 m from the AR

## 1B06M1

- ST 1104-PAR-1B06M1-ZZZ-ST-MOD-000001.nwc names its site "Internal". It sits 72.5 mm from
  the group's reference model, all of it in height and none on plan
- ME 1104-PAR-1B06M1-ZZZ-ME-MOD-000001.nwc, site "PW3_Shared_Location", the reference,
  because the group has no AR model

## 1B06PS

- AR 1104-PAR-1B06PS-ZZZ-AR-MOD-000001.nwc names its site "Internal". It is the group's
  reference model
- ST 1104-PAR-1B06PS-ZZZ-ST-MOD-000001.nwc also names its site "Internal", and sits about
  2,823 km from the AR
- EL 1104-PAR-1B06PS-ZZZ-EL-MOD-000001.nwc, site "COMMUNITY 14", about 336 km from the AR

## 1C06M2

- ST 1104-PAR-1C06M2-ZZZ-ST-MOD-000001.nwc names its site "Internal". It sits 72.5 mm from
  the group's reference model, all of it in height and none on plan
- ME 1104-PAR-1C06M2-ZZZ-ME-MOD-000001.nwc, site "PW3_Shared_Location", the reference,
  because the group has no AR model

Where this comes from: the ALIGNMENT block of each group in the run's log,
steps\runs\03\item1-C06\run-20261001-140037.log, lines 580 to 594 for 1B06BC, 1332 to 1344
for 1B06G1, 2485 to 2494 for 1B06M1, 5508 to 5519 for 1B06PS and 7610 to 7619 for 1C06M2.
Each group still wrote its NWF, its NWD and its clash report, so the evidence is there to
send with this note.
