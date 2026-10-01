## 2026-09-29 The loop, turn 3, F102 every result file is read for a machine name or a licensing id, DONE

Core tests 1746 passed, 0 failed, 0 skipped, before, on fix-F102 cut from main at e555619,
and after, at each commit and in this branch's pre-commit run by hand. On the Actions runner
1720 passed, 0 failed, 26 skipped. Nothing under src or tests changed, so no add-in build
applies. This entry rides in PR 76. Nothing waits for the local machine, because the check
and the mask were proved here and no Navisworks run applies.

### What was done

- found by the lead on the pushed branch fix-F100 before the plan of turn 3: the start
  probe's result named this machine on line 1 and carried on lines 446 and 447 the command
  lines of two AdskLicensingAgent processes with the licensing agent's ids. The reflection
  file line 1 and scan.md line 4650 named the machine. The same result as 464f79f first
  committed it carried one more id on line 275. Main held none of them
- ROOT CAUSE: tools\probes\probe-automation-start.ps1 as 464f79f committed it, line 538,
  wrote whole the command line of every new Autodesk and licensing process, and line 160
  printed MACHINE with the computer name, as every probe does. Nothing between a result
  and a commit read for them
- tools\checks\evidence-ids.txt is the one place that says what counts as an id: an
  analytics agent id, a GUID on a line naming analyticsagentid, a GUID after -i on any
  line, a GUID on a line naming AdskLicensing, AdskIdentity or GenuineService, and the
  machine name
- tools\loop\mask-evidence.ps1 writes a masked copy of one result file and never changes
  the file it reads. Every other byte comes out as it went in, and the copy is read back
  with the check's own rule and deleted if anything is left
- tools\checks\check-evidence-ids.sh refuses every kind, naming the file, the line and the
  kind and never the text. It reads every file as text, or names it and refuses it, bar a
  .jpg or .xlsx under samples, and a failed read ends it with exit 2. It runs in the
  pre-commit over the staged content and in Actions over the tree and over
  tools\checks\broken, where Actions also masks the fabricated ids on every run and compares
  the copy byte for byte
- PROVED in %LOCALAPPDATA%\NwcFederatorLoop\turn3\f102\proof.txt: the check refused the
  copies of the four files of fix-F100 on 12 lines before, and passed them masked after,
  with only lines 1, 446 and 447 of the result changed. On fabricated samples every kind
  was masked, the near misses were left, NUL and UTF-16 files were refused, four kinds of
  failed read each ended the check with exit 2, and each refusal of the mask was made once
- READ TWICE. The first Actions run failed on the broken folder's README spelling the made
  up name, fixed in c1d5efe. The reviewer and the breaker then found six things at c1d5efe:
  the rule in two files with the check narrower than the mask, files grep called binary
  skipped with no word, a grep failure lost in a pipe, no test that the text is never
  printed, no repeatable test of the mask, and words wider than the code. All six fixed in
  0b48415 and confirmed by a second reading
- USED BEFORE IT MERGED: the lead masked fix-F100's three result files with this branch's
  script in turn 3, and the rules file masks them byte for byte the same
- A COMMIT FROM A WORKTREE RUNS THE MAIN CLONE'S PRE-COMMIT, measured here, so this
  branch's hook was run by hand, refused a staged fabricated session id and passed the change

### What remains

- the check refuses a zip, so a run file over 20 MB cannot be committed zipped as
  .claude\rules\loop.md says until Bader decides, Q90
- main names another machine in 9 places, which the check does not read for, Q88
- fix-F100's pushed commits 464f79f and c98c6f3 still hold the ids on GitHub, Q88
- .claude\settings.json does not name the new check or mask-evidence.ps1 in its allow list

### Known bugs

- none from this fix. The check reads words, so an id spelled in a way no kind names is not
  seen, Actions reads for the runner's name and never Bader's, and on this machine the check
  takes about 45 seconds of every commit, because starting a program costs close to a
  second here

### What comes next

1. PR 74, F100, with main merged in, then F103
