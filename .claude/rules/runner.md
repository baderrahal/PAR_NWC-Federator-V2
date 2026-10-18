---
paths:
  - "src/Federator.Runner/**"
---

# Rules for Federator.Runner

The runner is the program the window a person runs starts for a run of packages, so each group of the run gets a Navisworks
of its own, F214 and F222. It references no Navisworks file and holds no rule a test can prove, which is Core's,
core.md A FRESH NAVISWORKS PER PACKAGE and A FRESH NAVISWORKS PER GROUP, and every word it writes is Core's,
`PackageRunWords` and `AutoSaveSwitch`. Its rules sat in addin.md, whose paths line is src/Federator.Addin/**, so they were
never read where the runner is touched, claude\audit-round-2026-10-18.md item 19. They are here since F228, moved and not
copied.

## Each group in its own Navisworks, F214

THE RUNNER, src\Federator.Runner, references no Navisworks file, loads the Automation DLL from the install folder by
reflection as the probe's helper did, scan.md 5z-zj, and for each group in turn, each package before F222, starts one
Navisworks and takes the one Roamer new since before as its own and never any other. Where it cannot tell which is its own,
more than one or none new, the group is not run, its Navisworks told to quit and every Navisworks new since its start waited
on and none ended. Otherwise it sets it visible, runs the installed plugin with the mode package-run, the order and the
index, quits it by Dispose and waits the order's GoneWithinSeconds for its process to be gone, ending it past that, so the
next starts only once the last has closed. A start that threw has the Navisworks it brought up found and waited on too. The
order's ceiling runs from before the start to after the wait, so a start, a run, a quit or a wait that hangs is ended where
its Navisworks is told, and said where it is not. A throw, a crash or the ceiling of one group is said and the next runs.

## Written once with the window's half

Two rules are one rule each with a half in the window and a half here, so each is written once, in addin.md, and read there
whole, never copied here:

- addin.md, A fresh Navisworks per group, F222: one Navisworks a group, the seconds of each start and each quit and the
  runner's last line adding them up, and the ceiling of one group, 30 minutes.
- addin.md, AUTO-SAVE IS OFF WHILE THE TOOL WORKS AND PUT BACK EXACTLY AS IT WAS, F223: the value before kept on its record
  as the runner starts, "3 0" before every group inside its try, and the one put back in Main's finally, deleting the record
  only where it reads as it was and no Navisworks new since the run began still runs.
